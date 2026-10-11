"""Outline SVG tool masters into ordered painted contours for Blender geometry icons."""

from itertools import pairwise
from math import prod
from pathlib import Path
from typing import override

from coloraide import Color
from fontTools.pens.basePen import BasePen
from fontTools.pens.explicitClosingLinePen import ExplicitClosingLinePen
from picosvg.geometric_types import Rect
from picosvg.svg import SVG
from picosvg.svg_pathops import skia_path
from picosvg.svg_transform import Affine2D
from picosvg.svg_types import SVGShape

# --- [TYPES] ----------------------------------------------------------------------------

type Point = tuple[float, float]
type Knot = tuple[Point, Point, Point]
type Contours = tuple[tuple[Knot, ...], ...]
type RGBA = tuple[float, float, float, float]
type Glyphs = dict[str, tuple[tuple[RGBA, Contours], ...]]

# --- [OPERATIONS] -----------------------------------------------------------------------


def outlined(source: Path) -> Glyphs:
    """Read ordered solid SVG paints as scene-linear RGBA and closed cubic contours.

    Symbolic currentColor uses white tool ink. DAT paint replaces covered pixels;
    per-shape alpha carries no SVG source-over or isolated-group compositing.

    Args:
        source: Directory containing WorkSpaceTool SVG masters.

    Returns:
        Tool glyph names mapped to painted contours in SVG draw order.

    Raises:
        ValueError: If a master lacks viewport dimensions or uses a nonliteral paint.
    """

    class Cubics(BasePen):
        """Collect cubic knots through the native fontTools pen protocol."""

        def __init__(self) -> None:
            """Initialize the native pen and its contour buffers."""
            super().__init__()  # type: ignore[no-untyped-call]
            self.contours: list[tuple[Knot, ...]] = []
            self.segments: list[tuple[Point, Point, Point, Point]] = []

        @override
        def _moveTo(self, pt: Point) -> None:
            self.segments = []

        @override
        def _lineTo(self, pt: Point) -> None:
            self._curveToOne(self._getCurrentPoint(), pt, pt)  # type: ignore[no-untyped-call]

        @override
        def _curveToOne(self, pt1: Point, pt2: Point, pt3: Point) -> None:
            self.segments.append((self._getCurrentPoint(), pt1, pt2, pt3))  # type: ignore[no-untyped-call]

        @override
        def _closePath(self) -> None:
            self.contours.append(tuple((segment[0], previous[2], segment[1]) for previous, segment in pairwise((self.segments[-1], *self.segments))))

    def paint(shape: SVGShape, root_opacity: float, ancestry: float, transform: Affine2D) -> tuple[RGBA, Contours]:
        color = Color("white" if shape.fill.lower() == "currentcolor" else shape.fill).convert("srgb-linear").fit()
        red, green, blue = color.coords(nans=False)
        alpha = color.alpha(nans=False) * root_opacity * shape.opacity * ancestry
        pen = Cubics()
        skia_path(shape.as_path().remove_overlaps().as_cmd_seq(), "nonzero").transform(*transform).draw(ExplicitClosingLinePen(pen))  # type: ignore[no-untyped-call]
        return (red, green, blue, alpha), tuple(pen.contours)

    glyphs: Glyphs = {}
    for master in sorted(source.glob("*.svg")):
        styled: SVG = SVG.parse(master).apply_style_attributes()  # type: ignore[no-untyped-call]
        root_opacity = float(styled.svg_root.get("opacity", "1"))
        if (viewbox := (svg := styled.topicosvg()).view_box()) is None:  # type: ignore[no-untyped-call]
            raise ValueError(f"SVG master has no viewport dimensions: {master}")
        transform = Affine2D.flip_y() @ Affine2D.rect_to_rect(viewbox, Rect(-1, -1, 2, 2), svg.svg_root.get("preserveAspectRatio", "xMidYMid meet"))
        glyphs[master.stem] = tuple(
            paint(context.shape(), root_opacity, prod(float(ancestor.get("opacity", "1")) for ancestor in context.element.iterancestors()), transform)
            for context in svg.depth_first()
            if context.is_shape()
        )
    return glyphs


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["RGBA", "Contours", "Glyphs", "Knot", "Point", "outlined"]
