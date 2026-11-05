"""Requirements a build meets per genre and treatment, the fontspector run and findings that measure them, and approved shaping expectations."""

import sys
from typing import Annotated
lazy from collections.abc import Callable
lazy from functools import partial
lazy from pathlib import Path
lazy import re

from expression.extra.result import catch
import msgspec
lazy from fontTools.ufoLib import UFOReader
lazy from fontTools.ufoLib.errors import UFOLibError
lazy import ots

from typography.faces.declaration import Band, Em, Ratio
lazy from typography.faces.coverage import Coverage as Covered
lazy from typography.faces.declaration import Contour, Face, Faces, OrnamentFace, PROFILE, Ringed, Segmented, Shadow, Sheared
lazy from typography.faces.faults import Fault, Ok, Outcome, ran, Reason, stored
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
    """The set's fontspector profile reports no FAIL, ERROR, or FATAL outside its shaping checks and the excluded check ids."""

    excluded: tuple[str, ...] = ()


class Expectations(_Requirement, frozen=True, kw_only=True):
    """An approval exists and corpus shaping meets it under fontspector's shaping checks."""


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


class Check(msgspec.Struct, frozen=True, kw_only=True):
    """Fontspector check result on one font file or on the folder collection holding it."""

    check_id: str
    subresults: tuple[_Result, ...] = ()


class _Report(msgspec.Struct, frozen=True, kw_only=True):
    results: frozendict[str, frozendict[str, tuple[Check, ...]]]


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


def covering(face: Faces, source: Path, /) -> Outcome[Covered]:
    """Returns the code points a built face answers for, a genre's for a text face and the mappings its written UFO states for an ornament face.

    Args:
        face: Declared face.
        source: UFO the face's build wrote.

    Returns:
        Ok(coverage), `UNREADABLE` for an ornament UFO the reader refuses.
    """
    match face:
        case Face(genre=genre):
            return Ok(genre.coverage)
        case OrnamentFace():
            return (
                catch(lambda: UFOReader(source).getGlyphSet().getUnicodes(), exception=UFOLibError)()
                .map_error(partial(Fault.of, Reason.UNREADABLE, str(source)))
                .map(lambda mapped: Covered(required=frozenset(*codes for codes in mapped.values()), excluded=(), blocks=frozenset()))
            )


# --- [PROCESSES]


def inspected(paths: tuple[Path, ...], root: Path, /) -> Outcome[frozendict[Path, tuple[Check, ...]]]:
    """Runs fontspector once over built faces under the library's profile, from the declaration folder its relative test directory resolves against.

    Args:
        paths: Built font files.
        root: Folder of the set's declaration.

    Returns:
        Ok(checks) per path holding the results on the file and on its folder's collection, `FAILED` for a failed launch, `INVALID` for an undecodable report.
    """
    return (
        ran("fontspector", "--profile", str(PROFILE), "--configuration", str(PROFILE), "--quiet", "--skip-network", "--json", "-", *(str(path.absolute()) for path in paths), cwd=root, check=False)
        .bind(lambda raw: catch(lambda: msgspec.json.decode(raw, type=_Report), exception=msgspec.DecodeError)().map_error(partial(Fault.of, Reason.INVALID, "fontspector")))
        .map(
            lambda report: frozendict({
                path: tuple(check for tested in (path.absolute(), path.absolute().parent) for checks in report.results.get(str(tested), frozendict()).values() for check in checks) for path in paths
            })
        )
    )


def _breached(checks: Outcome[tuple[Check, ...]], selected: Callable[[Check], bool], /) -> tuple[Breach, ...]:
    """One breach per failed selected check and per shaping input that changed or produced a forbidden glyph, or one breach per fault where the run could not measure."""
    shaping_input = r"(?<=^Shaping did not match: ).+$|^.+(?= produced forbidden )"
    return checks.map(
        lambda found: tuple(
            (subject, result.severity, "WARN")
            for check in found
            if selected(check)
            for result in check.subresults
            if result.severity in {"FAIL", "ERROR", "FATAL"}
            for subject in tuple(match.group() for match in re.finditer(shaping_input, result.message, re.MULTILINE)) or (f"{check.check_id} {result.code}".strip(),)
        )
    ).default_with(lambda faults: tuple((fault.subject, fault.message, "measured") for fault in faults))


# --- [VERIFICATION]


def _measured(
    requirement: Requirements, binary: Binary, /, *, coverage: Covered, original: Binary, corpus: Corpus, path: Path, approved: Path, checks: Outcome[tuple[Check, ...]]
) -> tuple[Breach, ...]:
    shaping = "shaping/"
    match requirement:
        case Sanitized():
            return () if (done := ots.sanitize(str(path), capture_output=True)).returncode == 0 else ((path.name, done.stderr.decode().strip() or f"exit {done.returncode}", "accepted"),)  # type: ignore[no-untyped-call] # ty: ignore[unsound-return-statement]
        case Profile(excluded=excluded):
            return _breached(checks, lambda check: not check.check_id.startswith(shaping) and not any(part in check.check_id for part in excluded))
        case Expectations():
            return (*(() if approved.is_file() else ((str(approved), "absent", "approved"),)), *_breached(checks, lambda check: check.check_id.startswith(shaping)))
        case Reachable(shapers=shapers):
            return reachable(binary, corpus, shapers)
        case Coverage():
            return (
                *((f"U+{code:04X}", "unmapped", "required") for code in coverage.uncovered(binary.encoded)),
                *((f"U+{code:04X}", "mapped", "required, excluded, or kept") for code in coverage.unrecorded(binary.encoded)),
            )
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


def verified(
    binary: Binary, requirements: tuple[Requirements, ...], /, *, coverage: Covered, original: Binary, corpus: Corpus, path: Path, approved: Path, checks: Outcome[tuple[Check, ...]]
) -> tuple[Finding, ...]:
    """Measures every requirement of a build independently and accumulates their findings, an empty tuple passing.

    Args:
        binary: Built font.
        requirements: Requirements the build meets.
        coverage: Code points the build answers for, from `covering`.
        original: Font the source was imported from.
        corpus: Runs every shaping measure reads.
        path: Built file OTS sanitizes.
        approved: Approved shaping expectations of the face.
        checks: Fontspector results on the built file, from `inspected`.

    Returns:
        Every finding in requirement order.
    """
    return tuple(
        Finding(requirement=str(type(requirement).__struct_config__.tag), subject=subject, measured=measured, bound=bound)
        for requirement in requirements
        for subject, measured, bound in _measured(requirement, binary, coverage=coverage, original=original, corpus=corpus, path=path, approved=approved, checks=checks)
    )


def approved(binary: Binary, corpus: Corpus, path: Path, file: str, /) -> Outcome[Path]:
    """Writes the shaping expectations of a build's corpus runs to a path, the one writer of approval files.

    Args:
        binary: Built font whose shaping the file approves.
        corpus: Lines and languages to shape.
        path: Approval file in the set's shaping test directory.
        file: Built font file name the tests select.

    Returns:
        Ok(path), `UNWRITABLE` fault where the file cannot be written.
    """
    return stored(path, partial(Path.write_bytes, data=encoded(expected(binary, corpus, file))))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "Check",
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
    "covering",
    "inspected",
    "requirements",
    "verified",
]
