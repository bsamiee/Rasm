"""Encodes extension icon masters as nanosvg previews and toolbar geometry icons."""

from typing import override
lazy from math import ceil, pi, prod, sqrt
lazy from pathlib import Path

from fontTools.misc.bezierTools import cubicPointAtT
from fontTools.pens.basePen import BasePen
import numpy as np
lazy from coloraide import Color
lazy import numpy.typing as npt
lazy from picosvg.geometric_types import Rect
lazy from picosvg.svg import SVG
lazy from picosvg.svg_pathops import skia_path
lazy from picosvg.svg_transform import Affine2D
lazy from picosvg.svg_types import SVGShape
lazy import shapely

# --- [TYPES] ----------------------------------------------------------------------------

type _Point = tuple[float, float]

# --- [CONSTANTS] ------------------------------------------------------------------------

_EXTENT = np.iinfo(np.uint8).max
_GRID = 1.0

# --- [SERVICES] -------------------------------------------------------------------------


class _Rings(BasePen):
    """Collects closed contours as polylines within half a grid step of each cubic."""

    def __init__(self) -> None:
        """Initializes pen and ring buffer."""
        super().__init__()  # type: ignore[no-untyped-call]
        self.rings: list[list[_Point]] = []

    @override
    def _moveTo(self, pt: _Point) -> None:
        self.rings.append([pt])

    @override
    def _lineTo(self, pt: _Point) -> None:
        self.rings[-1].append(pt)

    @override
    def _curveToOne(self, pt1: _Point, pt2: _Point, pt3: _Point) -> None:
        start = self.rings[-1][-1]
        knots = np.array((start, pt1, pt2, pt3))
        steps = max(1, ceil(sqrt(3 * 2 / 8 * float(np.hypot(*np.diff(knots, 2, axis=0).T).max()) / (_GRID / 2))))
        t = np.linspace(0, 1, steps + 1)[1:]
        self.rings[-1].extend(zip(*cubicPointAtT(start, pt1, pt2, pt3, t)))  # type: ignore[no-untyped-call]


# --- [OPERATIONS] -----------------------------------------------------------------------


def _painted(transform: Affine2D, shape: SVGShape, opacity: float, /) -> tuple[bytes, npt.NDArray[np.uint8]]:
    """Returns the sRGB byte RGBA of each corner and the byte corners of a solid shape's triangles."""
    color = Color(shape.fill).convert("srgb").fit()
    rgba = bytes(round(channel * _EXTENT) for channel in (*color.coords(nans=False), color.alpha(nans=False) * shape.opacity * opacity))
    pen = _Rings()
    skia_path(shape.as_path().remove_overlaps().as_cmd_seq(), "nonzero").transform(*transform).draw(pen)
    region = shapely.set_precision(shapely.build_area(shapely.GeometryCollection([shapely.LinearRing(ring) for ring in pen.rings])).intersection(shapely.box(0, 0, _EXTENT, _EXTENT)), _GRID)
    corners = shapely.get_coordinates(shapely.orient_polygons(shapely.get_parts(shapely.constrained_delaunay_triangles(region)))).reshape(-1, 4, 2)[:, :3].astype(np.uint8)
    return rgba * corners.shape[0] * 3, corners


def _encoded(master: Path, paint: str, /, *, turned: bool = False) -> tuple[str, bytes]:
    """Encodes master as the picosvg preview Blender's nanosvg loader rasterizes and as geometry icon triangles of its solid paints.

    Icon files hold `VCO` header, byte XY corners per triangle over -1..1 square, then sRGB byte RGBA per corner, in SVG draw order.
    Shape alpha applies without SVG source-over or isolated-group compositing.

    Args:
        master: WorkSpaceTool SVG master.
        paint: CSS color `currentColor` paints.
        turned: Whether geometry turns a quarter clockwise about the center of the viewport, over each element's own transform.

    Returns:
        Preview document text and icon file bytes.

    Raises:
        ValueError: If master has no viewport dimensions or holds a nonliteral paint.
    """
    svg: SVG = SVG.parse(master).apply_style_attributes(inplace=True)  # type: ignore[no-untyped-call]  # ty: ignore[unsound-assignment]
    for element in svg.svg_root.iter():
        for name, value in element.attrib.items():
            if name in {"fill", "stroke", "stop-color"} and value.strip().casefold() == "currentcolor":
                element.set(name, paint)
    if (viewbox := svg.view_box()) is None:
        raise ValueError(f"SVG master {master} has no viewport dimensions")
    for element in svg.svg_root if turned else ():
        element.set("transform", " ".join((Affine2D.identity().rotate(pi / 2, viewbox.x + viewbox.w / 2, viewbox.y + viewbox.h / 2).tostring(), element.get("transform", ""))))  # type: ignore[no-untyped-call]
    opacity = float(svg.svg_root.get("opacity", "1"))
    svg.topicosvg(inplace=True)  # type: ignore[no-untyped-call]
    transform = Affine2D(1, 0, 0, -1, 0, _EXTENT) @ Affine2D.rect_to_rect(viewbox, Rect(0, 0, _EXTENT, _EXTENT), svg.svg_root.get("preserveAspectRatio", "xMidYMid meet"))  # type: ignore[call-arg]
    paints = tuple(
        _painted(transform, context.shape(), opacity * prod(float(ancestor.get("opacity", "1")) for ancestor in context.element.iterancestors()))
        for context in svg.depth_first()  # type: ignore[no-untyped-call]
        if context.is_shape()
    )
    return svg.tostring(), b"".join((b"VCO\x00", bytes((_EXTENT, _EXTENT, 0, 0)), *(corners.tobytes() for _, corners in paints), *(colors for colors, _ in paints)))  # type: ignore[no-untyped-call]  # ty: ignore[unsound-return-statement]


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = []
