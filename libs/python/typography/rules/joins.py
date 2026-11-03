"""Joins: one join band with its foot on the baseline, every joining edge carrying the band into its neighbour, and the kashida as that band."""

import msgspec
lazy from expression import Error, Ok

from typography.faces.declaration import Em
from typography.rules.scripts import Rule
lazy from typography.faces.faults import Fault, Outcome, Reason
lazy from typography.faces.glyphs import Glyph, Letter, Ligature, Source
lazy from typography.outlines.paths import Box, strip, union
lazy from typography.rules.measures import band, mapped

# --- [MODELS] ---------------------------------------------------------------------------


class Joins(Rule, frozen=True, kw_only=True):
    """One join band with its foot on the baseline, each joining edge reaching the overlap into its neighbour, and the joining glyph drawn as the band at its length."""

    overlap: Em
    kashida: int | None
    length: Em = 0.25


# --- [OPERATIONS] -----------------------------------------------------------------------


def _bridged(glyph: Glyph, thickness: float, overlap: float, /) -> Glyph:
    """Carries the band across each joining edge that holds ink, from one band inside the edge to the overlap past it."""
    form = glyph.name.form
    edges = (
        (form.right and strip(glyph.contours, glyph.advance - thickness, glyph.advance), Box(xmin=glyph.advance - thickness, ymin=0.0, xmax=glyph.advance + overlap, ymax=thickness)),
        (form.left and strip(glyph.contours, 0.0, thickness), Box(xmin=-overlap, ymin=0.0, xmax=thickness, ymax=thickness)),
    )
    bars = tuple((box.contour,) for inked, box in edges if inked)
    return msgspec.structs.replace(glyph, contours=union(glyph.contours, *bars)) if bars else glyph


def joined(source: Source, rule: Joins, /) -> Outcome[Source]:
    """Sets the join band's foot on the baseline, bridges every joining edge into its neighbour, and draws the joining glyph as the band.

    Returns:
        Ok(joined source), `ABSENT` for a face without medial forms to read the band from.
    """
    match band(source):
        case (low, high):
            thickness, reach, length, kashida = high - low, rule.overlap * source.upm, rule.length * source.upm, rule.kashida
            moved = tuple(
                _bridged(mapped(glyph, (1.0, 0.0, 0.0, 1.0, 0.0, -low)), thickness, reach) for glyph in source.glyphs if isinstance(glyph.role, Letter | Ligature) and glyph.name.codes != (kashida,)
            )
            bar = Box(xmin=-reach, ymin=0.0, xmax=length + reach, ymax=thickness).contour
            drawn = tuple(msgspec.structs.replace(mapped(glyph, (1.0, 0.0, 0.0, 1.0, 0.0, -low)), advance=length, contours=(bar,)) for glyph in source.glyphs if glyph.name.codes == (kashida,))
            return Ok(source.replaced(*moved, *drawn))
        case _:
            return Error(Fault.of(Reason.ABSENT, source.face.stem, member="medial forms for a join band"))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Joins", "joined"]
