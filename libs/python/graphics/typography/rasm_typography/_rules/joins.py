"""Joins: one join band with its foot on the baseline, every joining edge extending the band into its neighbor, and the kashida drawn as the band."""

lazy from graphlib import TopologicalSorter
lazy import math

import msgspec
lazy from fontTools.misc.transform import Identity, Offset, Transform

from rasm_typography._faces.declaration import Em
from rasm_typography._rules import measures
lazy from rasm_records import Error, Fault, Ok, Outcome, Reason
lazy from rasm_typography._faces.glyphs import Glyph, Letter, Ligature, Source
lazy from rasm_typography._outlines import paths

# --- [MODELS] ---------------------------------------------------------------------------


class Joins(measures.Rule, frozen=True, kw_only=True):
    """One join band with its foot on the baseline, each joining edge bridged at band height from the ink nearest it, within a band's width, to the overlap in whole units into its neighbor, and the joining glyph drawn as the band at its length."""

    overlap: Em
    kashida: int | None
    length: Em = 0.25

    def applied(self, source: Source, /, **_: object) -> Outcome[Source]:
        """Sets the join band's foot on the baseline, bridges every joining edge into its neighbor, and draws the joining glyph as the band.

        Returns:
            Ok(joined source), `ABSENT` for a face without medial forms to read the band from.
        """
        match measures.band(source):
            case (low, high):
                thickness, reach, length, kashida = high - low, math.ceil(self.overlap * source.upm), self.length * source.upm, self.kashida
                matrices: dict[str, Transform] = {}
                for name in TopologicalSorter({name: tuple(part.name for part in glyph.components) for name, glyph in source.named.items()}).static_order():
                    glyph = source.named[name]
                    match glyph:
                        case Glyph(role=Letter() | Ligature()):
                            matrices[name] = Offset(0.0, -low)
                        case Glyph(contours=(), components=(part,)):
                            target = matrices[part.name]
                            matrices[name] = Offset(*Transform(*part.transform).transformVector((target.dx, target.dy)))
                        case _:
                            matrices[name] = Identity
                placed = measures.mapped(source, matrices)
                moved = tuple(
                    msgspec.structs.replace(glyph, contours=paths.union(glyph.contours, *bars)) if bars else glyph
                    for glyph in placed.glyphs
                    if isinstance(glyph.role, Letter | Ligature) and glyph.name.codes != (kashida,)
                    for bars in (
                        tuple(
                            (paths.Box(xmin=min(inner, edge) - reach, ymin=0.0, xmax=max(inner, edge) + reach, ymax=thickness).contour,)
                            for joined, edge, low in ((glyph.name.form.right, glyph.advance, glyph.advance - thickness), (glyph.name.form.left, 0.0, 0.0))
                            if joined
                            for ink in (paths.bounds(paths.strip(glyph.contours, low, low + thickness)),)
                            if ink
                            for inner in (min(ink.xmin, ink.xmax, key=lambda x: abs(x - edge)),)
                        ),
                    )
                )
                bar = paths.Box(xmin=-reach, ymin=0.0, xmax=length + reach, ymax=thickness).contour
                drawn = tuple(msgspec.structs.replace(glyph, advance=length, contours=(bar,)) for glyph in placed.glyphs if glyph.name.codes == (kashida,))
                return Ok(placed.replaced(*moved, *drawn))
            case _:
                return Error(Fault.of(Reason.ABSENT, source.face.postscript, member="medial forms for a join band"))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Joins"]
