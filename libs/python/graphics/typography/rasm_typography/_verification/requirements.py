"""Requirements a build meets per genre and treatment, and the findings their measures give over a build, its original, a corpus, and approved shaping."""

import sys
from typing import Annotated
lazy from functools import partial

import msgspec
lazy from expression import Option
lazy from expression.extra.result import catch
lazy from pathops import PathOpsError

from rasm_typography._faces.declaration import Band, Contour, Em, Face, Faces, OrnamentFace, Ratio, Ringed, Segmented, Shadow, Sheared
lazy from rasm_records import Fault, Outcome, Reason
lazy from rasm_typography._faces import coverage
lazy from rasm_typography._faces.glyphs import _fetched
lazy from rasm_typography._shaping.corpus import Corpus
lazy from rasm_typography._shaping.expectations import Expected
lazy from rasm_typography._shaping.runs import Binary
lazy from rasm_typography._verification.geometry import _Breach, _clearance, _distinct, _identity, _join_parity, _ligature_area, _mark_line, _metrics, _reachable, _seam

# --- [MODELS] ---------------------------------------------------------------------------

# --- [REQUIREMENTS]


class _Requirement(msgspec.Struct, frozen=True, kw_only=True, tag_field="kind", tag=str.lower):
    """Property a build holds, its tag the finding's requirement name."""


class Expectations(_Requirement, frozen=True, kw_only=True):
    """An approval exists, every approved test shapes to its expectation, and no test shapes to a forbidden glyph."""


class Reachable(_Requirement, frozen=True, kw_only=True):
    """No corpus line shapes to glyph 0 under any shaper and language."""

    shapers: tuple[str, ...] = ("ot", *(("coretext",) if sys.platform == "darwin" else ()))


class Covered(_Requirement, frozen=True, kw_only=True, tag="coverage"):
    """Every required code point mapped and every mapped one required, excluded, or inside a kept block."""


class MarkClearance(_Requirement, frozen=True, kw_only=True):
    """Marks clear their own cluster and stand a gap in em from every other, and dots and hamzas clear other letters."""

    gap: Em


class MarkLine(_Requirement, frozen=True, kw_only=True):
    """Single above marks over one letter form share one bottom line within a tolerance in em."""

    tolerance: Em = 0.04


class LigatureArea(_Requirement, frozen=True, kw_only=True):
    """Each ligature holds a ratio of its components' filled area and spans that ratio of its advance."""

    ratio: Ratio = 0.5


class JoinParity(_Requirement, frozen=True, kw_only=True):
    """Joined letter pairs keep their distance with `kern` on and off."""


class Seam(_Requirement, frozen=True, kw_only=True):
    """Seam columns composited glyph by glyph stay inside a band of the merged outline's filled area at each pixel size."""

    band: tuple[Ratio, Ratio] = (0.87, 1.58)
    sizes: tuple[Annotated[int, msgspec.Meta(ge=1)], ...] = (12, 16, 24, 96)


class Distinct(_Requirement, frozen=True, kw_only=True):
    """No two digits of one script reach an intersection over union bound."""

    bound: Ratio = 0.9


class LineMetrics(_Requirement, frozen=True, kw_only=True, tag="metrics"):
    """Line pitch in em inside its band, the original's pitch where none is stated, and win metrics over every glyph and corpus run."""

    pitch: Band | None = None


class Identity(_Requirement, frozen=True, kw_only=True):
    """Dots and hamzas of each letter form the original draws meet the original's, piece for piece."""


type Requirements = Expectations | Reachable | Covered | MarkClearance | MarkLine | LigatureArea | JoinParity | Seam | Distinct | LineMetrics | Identity

# --- [FINDINGS]


class Finding(msgspec.Struct, frozen=True, kw_only=True):
    """Requirement a build misses on one subject, with the measured value and its bound."""

    requirement: str
    subject: str
    measured: float | str
    bound: float | str


# --- [OPERATIONS] -----------------------------------------------------------------------


def requirements(face: Faces, /) -> tuple[Requirements, ...]:
    """Derives the requirements of a face from its genre and treatments, an ornament face holding the ones art without letters meets.

    Args:
        face: Design record of the face.

    Returns:
        The requirements in measure order.
    """
    match face:
        case Face(genre=genre, treatments=treatments):
            drawn = not any(isinstance(treatment, Ringed | Contour | Shadow | Segmented) for treatment in treatments)
            upright = drawn and not any(isinstance(treatment, Sheared) for treatment in treatments)
            return (
                Expectations(),
                Reachable(),
                Covered(),
                MarkClearance(gap=genre.clearance),
                MarkLine(),
                LigatureArea(),
                JoinParity(),
                *((Seam(),) if drawn else ()),
                Distinct(),
                LineMetrics(pitch=genre.pitch),
                *((Identity(),) if upright else ()),
            )
        case OrnamentFace():
            return (Covered(), LineMetrics())


def _measured(requirement: Requirements, binary: Binary, /, *, coverage: coverage.Coverage, original: Binary, corpus: Corpus, approved: Option[Expected]) -> tuple[_Breach, ...]:
    match requirement:
        case Expectations():
            return approved.map(lambda expected: expected.regressed(binary)).default_value((("shaping", "absent", "approved"),))
        case Reachable(shapers=shapers):
            return _reachable(binary, corpus, shapers)
        case Covered():
            return (
                *((f"U+{code:04X}", "unmapped", "required") for code in coverage.uncovered(binary.encoded)),
                *((f"U+{code:04X}", "mapped", "required, excluded, or kept") for code in coverage.unrecorded(binary.encoded)),
            )
        case MarkClearance(gap=floor):
            return _clearance(binary, corpus, floor)
        case MarkLine(tolerance=tolerance):
            return _mark_line(binary, corpus, tolerance)
        case LigatureArea(ratio=ratio):
            return _ligature_area(binary, ratio)
        case JoinParity():
            return _join_parity(binary, corpus)
        case Seam(band=band, sizes=sizes):
            return _seam(binary, corpus, band, sizes)
        case Distinct(bound=bound):
            return _distinct(binary, bound)
        case LineMetrics(pitch=pitch):
            return _metrics(binary, original, corpus, pitch)
        case Identity():
            return _identity(binary, original, corpus)


def verified(binary: Binary, face: Faces, /, *, coverage: coverage.Coverage, original: Binary, corpus: Corpus, approved: Option[Expected]) -> Outcome[tuple[Finding, ...]]:
    """Measures every requirement of a face's build independently and accumulates their findings, an empty tuple passing.

    Args:
        binary: Built font.
        face: Design record whose requirements the build meets.
        coverage: Code points the build answers for, a text face's genre coverage or the mappings an ornament face's source states.
        original: Font the source was imported from.
        corpus: Runs every shaping measure reads.
        approved: Approved shaping of the face, `Nothing` before its first approval.

    Returns:
        Ok(findings) in requirement order, `FAILED` for a measure the native boolean refuses, and the faults of holding the pinned Unicode joining table.
    """
    return _fetched().bind(
        lambda _: catch(
            lambda: tuple(
                Finding(requirement=str(type(requirement).__struct_config__.tag), subject=subject, measured=measured, bound=bound)
                for requirement in requirements(face)
                for subject, measured, bound in _measured(requirement, binary, coverage=coverage, original=original, corpus=corpus, approved=approved)
            ),
            exception=PathOpsError,
        )().map_error(partial(Fault.of, Reason.FAILED, face.postscript))
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "Covered",
    "Distinct",
    "Expectations",
    "Finding",
    "Identity",
    "JoinParity",
    "LigatureArea",
    "LineMetrics",
    "MarkClearance",
    "MarkLine",
    "Reachable",
    "Requirements",
    "Seam",
    "requirements",
    "verified",
]
