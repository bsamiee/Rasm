"""Joins: one join band with its foot on the baseline, every joining edge extending the band into its neighbor, and the kashida drawn as the band."""

lazy from graphlib import TopologicalSorter

import msgspec
lazy from fontTools.misc.transform import Identity, Offset, Transform

from typography.faces.declaration import Em
from typography.rules.measures import band, mapped, Rule
lazy from typography.faces.faults import Error, Fault, Ok, Outcome, Reason
lazy from typography.faces.glyphs import Glyph, Letter, Ligature, Source
lazy from typography.outlines.paths import Box, strip, union

# --- [MODELS] ---------------------------------------------------------------------------


class Joins(Rule, frozen=True, kw_only=True):
    """One join band with its foot on the baseline, each joining edge reaching the overlap into its neighbor, and the joining glyph drawn as the band at its length."""

    overlap: Em
    kashida: int | None
    length: Em = 0.25


# --- [OPERATIONS] -----------------------------------------------------------------------


def _bridged(glyph: Glyph, thickness: float, overlap: float, /) -> Glyph:
    """Extends the band across each drawn joining edge, from one band thickness inside the edge to the overlap past it."""
    form = glyph.name.form
    edges = (
        (form.right and strip(glyph.contours, glyph.advance - thickness, glyph.advance), Box(xmin=glyph.advance - thickness, ymin=0.0, xmax=glyph.advance + overlap, ymax=thickness)),
        (form.left and strip(glyph.contours, 0.0, thickness), Box(xmin=-overlap, ymin=0.0, xmax=thickness, ymax=thickness)),
    )
    bars = tuple((box.contour,) for drawn, box in edges if drawn)
    return msgspec.structs.replace(glyph, contours=union(glyph.contours, *bars)) if bars else glyph


def joined(source: Source, rule: Joins, /) -> Outcome[Source]:
    """Sets the join band's foot on the baseline, bridges every joining edge into its neighbor, and draws the joining glyph as the band.

    Returns:
        Ok(joined source), `ABSENT` for a face without medial forms to read the band from.
    """
    match band(source):
        case (low, high):
            thickness, reach, length, kashida = high - low, rule.overlap * source.upm, rule.length * source.upm, rule.kashida
            matrices: dict[str, Transform] = {}
            for name in TopologicalSorter({name: tuple(part.name for part in glyph.components) for name, glyph in source.named.items()}).static_order():
                glyph = source.named[name]
                match glyph:
                    case Glyph(role=Letter() | Ligature()):
                        matrices[name] = Offset(0.0, -low)
                    case Glyph(contours=(), components=(part,)):
                        target = matrices[part.name]
                        matrices[name] = Offset(*Transform(*part.transform).transformVector((target.dx, target.dy)))  # type: ignore[no-untyped-call]
                    case _:
                        matrices[name] = Identity
            placed = mapped(source, matrices)
            moved = tuple(_bridged(glyph, thickness, reach) for glyph in placed.glyphs if isinstance(glyph.role, Letter | Ligature) and glyph.name.codes != (kashida,))
            bar = Box(xmin=-reach, ymin=0.0, xmax=length + reach, ymax=thickness).contour
            drawn = tuple(msgspec.structs.replace(glyph, advance=length, contours=(bar,)) for glyph in placed.glyphs if glyph.name.codes == (kashida,))
            return Ok(placed.replaced(*moved, *drawn))
        case _:
            return Error(Fault.of(Reason.ABSENT, source.face.stem, member="medial forms for a join band"))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Joins", "joined"]
