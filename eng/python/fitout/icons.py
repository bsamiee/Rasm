# mypy: disable-error-code="no-untyped-call"

"""Outline SVG masters into closed cubic contours for Blender geometry icons."""

from collections import defaultdict
from itertools import pairwise
from math import prod
from pathlib import Path
from typing import override

from fontTools.pens.basePen import BasePen
from fontTools.pens.explicitClosingLinePen import ExplicitClosingLinePen
from picosvg.geometric_types import Rect
from picosvg.svg import SVG
from picosvg.svg_pathops import skia_path
from picosvg.svg_transform import Affine2D
from picosvg.svg_types import SVGShape, union

# --- [TYPES] ---------------------------------------------------------------------------

type Point = tuple[float, float]
type Knot = tuple[Point, Point, Point]
type Contours = tuple[tuple[Knot, ...], ...]
type Glyphs = dict[str, tuple[tuple[float, Contours], ...]]

# --- [OPERATIONS] ----------------------------------------------------------------------


def outlined(source: Path) -> Glyphs:
    """Read SVG masters as alpha groups of closed cubic contours.

    Args:
        source: Directory containing the root SVG masters.

    Returns:
        Glyph names mapped to alpha groups of closed cubic contours.

    Raises:
        ValueError: If a master has neither a viewBox nor viewport dimensions.
    """

    class Cubics(BasePen):
        """Collect cubic knots through the native fontTools pen protocol."""

        def __init__(self) -> None:
            """Initialize the native pen and its contour buffers."""
            super().__init__()
            self.contours: list[tuple[Knot, ...]] = []
            self.segments: list[tuple[Point, Point, Point, Point]] = []

        @override
        def _moveTo(self, pt: Point) -> None:
            self.segments = []

        @override
        def _lineTo(self, pt: Point) -> None:
            self._curveToOne(self._getCurrentPoint(), pt, pt)

        @override
        def _curveToOne(self, pt1: Point, pt2: Point, pt3: Point) -> None:
            self.segments.append((self._getCurrentPoint(), pt1, pt2, pt3))

        @override
        def _closePath(self) -> None:
            self.contours.append(tuple((segment[0], previous[2], segment[1]) for previous, segment in pairwise((self.segments[-1], *self.segments))))

    glyphs: Glyphs = {}
    for master in sorted(source.glob("*.svg")):
        svg: SVG = SVG.parse(master).topicosvg()
        if (viewbox := svg.view_box()) is None:
            raise ValueError(f"SVG master has no viewport dimensions: {master}")
        transform = Affine2D.flip_y() @ Affine2D.rect_to_rect(viewbox, Rect(-1, -1, 2, 2))
        groups: defaultdict[float, list[SVGShape]] = defaultdict(list)
        for context in svg.depth_first():
            if context.is_shape():
                shape = context.shape()
                alpha = shape.opacity * prod(float(ancestor.get("opacity", "1")) for ancestor in context.element.iterancestors())
                groups[alpha].append(shape)
        contours: list[tuple[float, Contours]] = []
        for alpha, shapes in sorted(groups.items()):
            pen = Cubics()
            skia_path(union(shapes), "nonzero").transform(*transform).draw(ExplicitClosingLinePen(pen))
            contours.append((alpha, tuple(pen.contours)))
        glyphs[master.stem] = tuple(contours)
    return glyphs


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Contours", "Glyphs", "Knot", "Point", "outlined"]
