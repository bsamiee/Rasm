"""Requirements a build meets per genre and treatment, the findings that measure them, and approved shaping expectations."""

import sys
from typing import Annotated
lazy from functools import partial
lazy from pathlib import Path
lazy import re
lazy import subprocess
lazy from tempfile import TemporaryDirectory

from expression.extra.result import catch
import msgspec
lazy import ots

from typography.faces.declaration import Band, Em, Ratio
lazy from typography.faces.coverage import Coverage as Covered
lazy from typography.faces.declaration import Contour, Face, Faces, OrnamentFace, Ringed, Segmented, Shadow, Sheared
lazy from typography.faces.faults import accumulated, Fault, Outcome, Reason
lazy from typography.faces.glyphs import Source
lazy from typography.shaping.corpus import Corpus
lazy from typography.shaping.expectations import encoded, expected
lazy from typography.shaping.runs import Binary
lazy from typography.verification.geometry import Breach, clearance, distinct, identity, join_parity, ligature_ink, mark_line, metrics, reachable, seam

# --- [MODELS] ---------------------------------------------------------------------------
# --- [REQUIREMENTS]


class _Requirement(msgspec.Struct, frozen=True, kw_only=True, tag_field="kind", tag=str.lower):
    """Property a build holds, its tag the finding's requirement name."""


class Sanitized(_Requirement, frozen=True, kw_only=True):
    """OTS accepts the built file."""


class Profile(_Requirement, frozen=True, kw_only=True):
    """Fontspector profiles report no FAIL, ERROR, or FATAL outside the excluded check ids."""

    profiles: tuple[str, ...] = ("universal",)
    excluded: tuple[str, ...] = ()


class Expectations(_Requirement, frozen=True, kw_only=True):
    """Corpus shaping equals the approved expectations under fontspector `shaping/regression`."""


class Reachable(_Requirement, frozen=True, kw_only=True):
    """No corpus line shapes to glyph 0 under any shaper and language."""

    shapers: tuple[str, ...] = ("ot", *(("coretext",) if sys.platform == "darwin" else ()))


class Coverage(_Requirement, frozen=True, kw_only=True):
    """Every required code point mapped and every mapped one required, excluded, or inside a kept block."""


class MarkClearance(_Requirement, frozen=True, kw_only=True):
    """Marks clear their own cluster and stand a gap in em from every other, and dots and hamzas clear other letters."""

    gap: Em


class MarkLine(_Requirement, frozen=True, kw_only=True):
    """Single above marks over one letter form share one bottom line within a tolerance in em."""

    tolerance: Em = 0.04


class LigatureInk(_Requirement, frozen=True, kw_only=True):
    """Each ligature holds a ratio of its components' ink and spans that ratio of its advance."""

    ratio: Ratio = 0.5


class JoinParity(_Requirement, frozen=True, kw_only=True):
    """Joined letter pairs keep their distance with `kern` on and off."""


class Seam(_Requirement, frozen=True, kw_only=True):
    """Seam columns composited glyph by glyph stay inside a band of the merged outline's ink at each pixel size."""

    band: tuple[Ratio, Ratio] = (0.87, 1.58)
    sizes: tuple[Annotated[int, msgspec.Meta(ge=1)], ...] = (12, 16, 24, 96)


class Distinct(_Requirement, frozen=True, kw_only=True):
    """No two digits of one script reach an intersection over union bound."""

    bound: Ratio = 0.9


class Metrics(_Requirement, frozen=True, kw_only=True):
    """Line pitch in em inside its band, the original's pitch where none is stated, and win metrics over every glyph and corpus run."""

    pitch: Band | None = None


class Identity(_Requirement, frozen=True, kw_only=True):
    """Dots and hamzas of each letter form the original draws meet the original's, piece for piece."""


type Requirements = Sanitized | Profile | Expectations | Reachable | Coverage | MarkClearance | MarkLine | LigatureInk | JoinParity | Seam | Distinct | Metrics | Identity

# --- [FINDINGS]


class Finding(msgspec.Struct, frozen=True, kw_only=True):
    """Requirement a build misses on one subject, with the measured value and its bound."""

    requirement: str
    subject: str
    measured: float | str
    bound: float | str


# --- [REPORTS]


class _Result(msgspec.Struct, frozen=True, kw_only=True):
    severity: str
    code: str = ""
    message: str = ""


class _Check(msgspec.Struct, frozen=True, kw_only=True):
    check_id: str
    subresults: tuple[_Result, ...] = ()


class _Report(msgspec.Struct, frozen=True, kw_only=True):
    results: frozendict[str, frozendict[str, tuple[_Check, ...]]]


# --- [OPERATIONS] -----------------------------------------------------------------------
# --- [REQUIREMENTS]


def requirements(face: Faces, /) -> tuple[Requirements, ...]:
    """Derives the requirements of a face from its genre and treatments, an ornament face holding the ones art without letters meets.

    Args:
        face: Declared face.

    Returns:
        The requirements in measure order.
    """
    match face:
        case Face(genre=genre, treatments=treatments):
            drawn = not any(isinstance(treatment, Ringed | Contour | Shadow | Segmented) for treatment in treatments)
            upright = drawn and not any(isinstance(treatment, Sheared) for treatment in treatments)
            return (
                Sanitized(),
                Profile(),
                Expectations(),
                Reachable(),
                Coverage(),
                MarkClearance(gap=genre.clearance),
                MarkLine(),
                LigatureInk(),
                JoinParity(),
                *((Seam(),) if drawn else ()),
                Distinct(),
                Metrics(pitch=genre.pitch),
                *((Identity(),) if upright else ()),
            )
        case OrnamentFace():
            return (Sanitized(), Profile(excluded=("opentype/monospace",)), Coverage(), Metrics())


def _covered(source: Source, binary: Binary, /) -> tuple[Breach, ...]:
    """Unmapped required and unrecorded mapped code points, ornament mappings derived from the planned source."""
    codes = binary.encoded
    match source.face:
        case Face(genre=genre):
            coverage = genre.coverage
        case OrnamentFace():
            coverage = Covered(required=frozenset(source.encoded), excluded=(), blocks=frozenset())
    return (*((f"U+{code:04X}", "unmapped", "required") for code in coverage.uncovered(codes)), *((f"U+{code:04X}", "mapped", "required, excluded, or kept") for code in coverage.unrecorded(codes)))


# --- [PROCESSES]


@catch(exception=OSError)
def _staged(folder: Path, tests: Path, /) -> tuple[str, ...]:
    """Isolates one approval through a symlink and writes the native configuration naming its directory."""
    (directory := folder / "shaping").mkdir()
    (directory / tests.name).symlink_to(tests.resolve(strict=True))
    (configuration := folder / "configuration.json").write_bytes(msgspec.json.encode({"shaping": {"test_directory": str(directory)}}))
    return ("--configuration", str(configuration))


@catch(exception=OSError)
def _launched(argv: tuple[str, ...], /) -> subprocess.CompletedProcess[bytes]:
    return subprocess.run(argv, capture_output=True, check=False)


def _checked(path: Path, /, *options: str) -> Outcome[tuple[Breach, ...]]:
    """Decodes fontspector's native stdout report, one breach per failed check and changed shaping input."""
    failed = partial(Fault.of, Reason.FAILED, "fontspector")
    return (
        _launched(("fontspector", *options, "--quiet", "--skip-network", "--json", "-", str(path)))
        .map_error(failed)
        .bind(
            lambda done: catch(lambda: msgspec.json.decode(done.stdout, type=_Report), exception=msgspec.DecodeError)().map_error(lambda error: Fault.of(Reason.INVALID, "fontspector", (done, error)))
        )
        .map(
            lambda decoded: tuple(
                (subject, result.severity, "WARN")
                for sections in decoded.results.values()
                for checks in sections.values()
                for check in checks
                for result in check.subresults
                if result.severity in {"FAIL", "ERROR", "FATAL"}
                for subject in tuple(match.group() for match in re.finditer(r"(?<=^Shaping did not match: ).+$", result.message, re.MULTILINE)) or (f"{check.check_id} {result.code}".strip(),)
            )
        )
    )


def _compared(path: Path, tests: Path, /) -> Outcome[tuple[Breach, ...]]:
    """Runs shaping regression with one isolated approval file."""
    with TemporaryDirectory() as scratch:
        return (
            _staged(Path(scratch), tests)
            .map_error(partial(Fault.of, Reason.UNREADABLE, str(tests)))
            .bind(lambda options: _checked(path, *options, "--profile", "googlefonts", "--checkid", "shaping/regression"))
        )


def _held(outcome: Outcome[tuple[Breach, ...]], /) -> tuple[Breach, ...]:
    """Breaches of a run, or one breach per fault where the run could not measure."""
    return outcome.default_with(lambda faults: tuple((fault.subject, fault.message, "measured") for fault in faults))


# --- [VERIFICATION]


def _measured(requirement: Requirements, binary: Binary, /, *, source: Source, original: Binary, corpus: Corpus, path: Path, approved: Path) -> tuple[Breach, ...]:
    match requirement:
        case Sanitized():
            return () if (done := ots.sanitize(str(path), capture_output=True)).returncode == 0 else ((path.name, done.stderr.decode().strip() or f"exit {done.returncode}", "accepted"),)  # type: ignore[no-untyped-call] # ty: ignore[unsound-return-statement]
        case Profile(profiles=profiles, excluded=excluded):
            return _held(
                accumulated(_checked(path, "--profile", profile, *(f"--exclude-checkid={check}" for check in excluded)) for profile in profiles).map(
                    lambda found: tuple(*breaches for breaches in found)
                )
            )
        case Expectations():
            return _held(_compared(path, approved))
        case Reachable(shapers=shapers):
            return reachable(binary, corpus, shapers)
        case Coverage():
            return _covered(source, binary)
        case MarkClearance(gap=floor):
            return clearance(binary, corpus, floor)
        case MarkLine(tolerance=tolerance):
            return mark_line(binary, corpus, tolerance)
        case LigatureInk(ratio=ratio):
            return ligature_ink(binary, ratio)
        case JoinParity():
            return join_parity(binary, corpus)
        case Seam(band=band, sizes=sizes):
            return seam(binary, corpus, band, sizes)
        case Distinct(bound=bound):
            return distinct(binary, bound)
        case Metrics(pitch=pitch):
            return metrics(binary, original, corpus, pitch)
        case Identity():
            return identity(binary, original, corpus)


def verified(binary: Binary, requirements: tuple[Requirements, ...], /, *, source: Source, original: Binary, corpus: Corpus, path: Path, approved: Path) -> tuple[Finding, ...]:
    """Measures every requirement of a build independently and accumulates their findings, an empty tuple passing.

    Args:
        binary: Built font.
        requirements: Requirements the build meets.
        source: Source the build compiled from.
        original: Font the source was imported from.
        corpus: Runs every shaping measure reads.
        path: Built file the process requirements run over.
        approved: Approved shaping expectations of the face.

    Returns:
        Every finding in requirement order.
    """
    return tuple(
        Finding(requirement=str(type(requirement).__struct_config__.tag), subject=subject, measured=measured, bound=bound)
        for requirement in requirements
        for subject, measured, bound in _measured(requirement, binary, source=source, original=original, corpus=corpus, path=path, approved=approved)
    )


def approved(binary: Binary, corpus: Corpus, path: Path, /) -> Outcome[Path]:
    """Writes the shaping expectations of a build's corpus runs to a path, the one writer of approval files.

    Returns:
        Ok(path), `UNWRITABLE` fault where the file cannot be written.
    """

    @catch(exception=OSError)
    def written() -> Path:
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_bytes(encoded(expected(binary, corpus)))
        return path

    return written().map_error(partial(Fault.of, Reason.UNWRITABLE, str(path)))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "Coverage",
    "Distinct",
    "Expectations",
    "Finding",
    "Identity",
    "JoinParity",
    "LigatureInk",
    "MarkClearance",
    "MarkLine",
    "Metrics",
    "Profile",
    "Reachable",
    "Requirements",
    "Sanitized",
    "Seam",
    "approved",
    "requirements",
    "verified",
]
