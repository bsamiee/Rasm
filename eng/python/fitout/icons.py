"""Encodes SVG tool masters as Blender geometry icons."""

from functools import reduce
from math import ceil, comb, prod, sqrt
from pathlib import Path
from typing import override

from coloraide import Color
from fontTools.pens.basePen import BasePen
import numpy as np
import numpy.typing as npt
from picosvg.geometric_types import Rect
from picosvg.svg import SVG
from picosvg.svg_pathops import skia_path
from picosvg.svg_transform import Affine2D
from picosvg.svg_types import SVGShape
import shapely
from shapely.geometry.base import BaseGeometry

# --- [TYPES] ----------------------------------------------------------------------------

type Point = tuple[float, float]

# --- [OPERATIONS] -----------------------------------------------------------------------


def encoded(master: Path) -> bytes:
    """Encodes master solid paints as Blender geometry icon triangles.

    Icon files hold `VCO` header, byte XY corners per triangle over -1..1 square, then sRGB byte RGBA per corner, in SVG draw order.
    `currentColor` paints white tool ink.
    Shape alpha applies without SVG source-over or isolated-group compositing.

    Args:
        master: WorkSpaceTool SVG master.

    Returns:
        Icon file bytes.

    Raises:
        ValueError: If master has no viewport dimensions or holds a nonliteral paint.
    """
    size = 255

    class Rings(BasePen):
        """Collects closed contours as polylines within half a byte step of each cubic."""

        def __init__(self) -> None:
            """Initializes pen and ring buffer."""
            super().__init__()  # type: ignore[no-untyped-call]
            self.rings: list[list[Point]] = []

        @override
        def _moveTo(self, pt: Point) -> None:
            self.rings.append([pt])

        @override
        def _lineTo(self, pt: Point) -> None:
            self.rings[-1].append(pt)

        @override
        def _curveToOne(self, pt1: Point, pt2: Point, pt3: Point) -> None:
            start: Point = self._getCurrentPoint()  # type: ignore[no-untyped-call]
            knots = np.array((start, pt1, pt2, pt3))
            steps = max(1, ceil(sqrt(3 * 2 / 8 * float(np.hypot(*np.diff(knots, 2, axis=0).T).max()) * size)))
            t = np.linspace(0, 1, steps + 1)[1:]
            self.rings[-1].extend(map(tuple, (np.array([comb(3, k) * t**k * (1 - t) ** (3 - k) for k in range(4)]).T @ knots).tolist()))

    def painted(shape: SVGShape, opacity: float, transform: Affine2D) -> tuple[bytes, npt.NDArray[np.uint8]]:
        color = Color("white" if shape.fill.lower() == "currentcolor" else shape.fill).convert("srgb").fit()
        rgba = bytes(round(channel * size) for channel in (*color.coords(nans=False), color.alpha(nans=False) * shape.opacity * opacity))
        pen = Rings()
        skia_path(shape.as_path().remove_overlaps().as_cmd_seq(), "nonzero").transform(*transform).draw(pen)
        contours: list[BaseGeometry] = [shapely.Polygon(ring) for ring in pen.rings]
        region = reduce(BaseGeometry.symmetric_difference, contours).intersection(shapely.box(-1, -1, 1, 1))
        triangles = shapely.get_coordinates(shapely.orient_polygons(shapely.get_parts(shapely.constrained_delaunay_triangles(region)))).reshape(-1, 4, 2)[:, :3]
        corners = np.rint((triangles + 1) / 2 * size).astype(np.int64)
        (first, second, third) = corners.transpose(1, 0, 2)
        kept = corners[(first[:, 0] - second[:, 0]) * (second[:, 1] - third[:, 1]) + (first[:, 1] - second[:, 1]) * (third[:, 0] - second[:, 0]) > 0]
        return rgba * kept.shape[0] * 3, kept.astype(np.uint8)

    styled: SVG = SVG.parse(master).apply_style_attributes()
    if (viewbox := (svg := styled.topicosvg()).view_box()) is None:
        raise ValueError(f"SVG master {master} has no viewport dimensions")
    transform = Affine2D.flip_y() @ Affine2D.rect_to_rect(viewbox, Rect(-1, -1, 2, 2), svg.svg_root.get("preserveAspectRatio", "xMidYMid meet"))
    paints = tuple(
        painted(context.shape(), float(styled.svg_root.get("opacity", "1")) * prod(float(ancestor.get("opacity", "1")) for ancestor in context.element.iterancestors()), transform)
        for context in svg.depth_first()
        if context.is_shape()
    )
    return b"".join((b"VCO\x00", bytes((size, size, 0, 0)), *(corners.tobytes() for _, corners in paints), *(colors for colors, _ in paints)))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["encoded"]
