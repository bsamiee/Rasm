"""Encodes extension icon masters as nanosvg previews and toolbar geometry icons."""

lazy from math import ceil, prod, sqrt
lazy from pathlib import Path
lazy from typing import override

lazy from coloraide import Color
lazy from cyclopts.types import ResolvedExistingDirectory, ResolvedPath
lazy from fontTools.misc.bezierTools import cubicPointAtT
lazy from fontTools.pens.basePen import BasePen
lazy import numpy as np
lazy import numpy.typing as npt
lazy from picosvg.geometric_types import Rect
lazy from picosvg.svg import SVG
lazy from picosvg.svg_pathops import skia_path
lazy from picosvg.svg_transform import Affine2D
lazy from picosvg.svg_types import SVGShape
lazy import shapely

lazy from blender.records.faults import accumulated, Error, Fault, Ok, Outcome, Reason
lazy from blender.records.stores import committed, folder_made
lazy from eng.python.blender.assembly import Blender, probed

# --- [OPERATIONS] -----------------------------------------------------------------------


def encoded(master: Path, paint: str, /, *, turned: bool = False) -> tuple[str, bytes]:
    """Encodes master as the picosvg preview Blender's nanosvg loader rasterizes and as geometry icon triangles of its solid paints.

    Icon files hold `VCO` header, byte XY corners per triangle over -1..1 square, then sRGB byte RGBA per corner, in SVG draw order.
    Shape alpha applies without SVG source-over or isolated-group compositing.

    Args:
        master: WorkSpaceTool SVG master.
        paint: CSS color `currentColor` paints.
        turned: Whether geometry turns a quarter clockwise about the square viewport.

    Returns:
        Preview document text and icon file bytes.

    Raises:
        ValueError: If master has no viewport dimensions or holds a nonliteral paint.
    """
    type Point = tuple[float, float]

    size = np.iinfo(np.uint8).max
    grid = 1.0

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
            start = self.rings[-1][-1]
            knots = np.array((start, pt1, pt2, pt3))
            steps = max(1, ceil(sqrt(3 * 2 / 8 * float(np.hypot(*np.diff(knots, 2, axis=0).T).max()) / (grid / 2))))
            t = np.linspace(0, 1, steps + 1)[1:]
            self.rings[-1].extend(zip(*cubicPointAtT(start, pt1, pt2, pt3, t)))  # type: ignore[no-untyped-call]

    def painted(shape: SVGShape, opacity: float) -> tuple[bytes, npt.NDArray[np.uint8]]:
        color = Color(shape.fill).convert("srgb").fit()
        rgba = bytes(round(channel * size) for channel in (*color.coords(nans=False), color.alpha(nans=False) * shape.opacity * opacity))
        pen = Rings()
        skia_path(shape.as_path().remove_overlaps().as_cmd_seq(), "nonzero").transform(*transform).draw(pen)
        region = shapely.set_precision(shapely.build_area(shapely.GeometryCollection([shapely.LinearRing(ring) for ring in pen.rings])).intersection(shapely.box(0, 0, size, size)), grid)
        corners = shapely.get_coordinates(shapely.orient_polygons(shapely.get_parts(shapely.constrained_delaunay_triangles(region)))).reshape(-1, 4, 2)[:, :3].astype(np.uint8)
        return rgba * corners.shape[0] * 3, corners

    svg: SVG = SVG.parse(master).apply_style_attributes(inplace=True)  # type: ignore[no-untyped-call]  # ty: ignore[unsound-assignment]
    for element in svg.svg_root.iter():
        for name, value in element.attrib.items():
            if name in {"fill", "stroke", "stop-color"} and value.strip().casefold() == "currentcolor":
                element.set(name, paint)
    if (viewbox := svg.view_box()) is None:
        raise ValueError(f"SVG master {master} has no viewport dimensions")
    for element in svg.svg_root if turned else ():
        element.set("transform", Affine2D(0, 1, -1, 0, viewbox.h, 0).tostring())
    opacity = float(svg.svg_root.get("opacity", "1"))
    svg.topicosvg(inplace=True)  # type: ignore[no-untyped-call]
    transform = Affine2D(1, 0, 0, -1, 0, size) @ Affine2D.rect_to_rect(viewbox, Rect(0, 0, size, size), svg.svg_root.get("preserveAspectRatio", "xMidYMid meet"))  # type: ignore[call-arg]
    paints = tuple(
        painted(context.shape(), opacity * prod(float(ancestor.get("opacity", "1")) for ancestor in context.element.iterancestors()))
        for context in svg.depth_first()  # type: ignore[no-untyped-call]
        if context.is_shape()
    )
    return svg.tostring(), b"".join((b"VCO\x00", bytes((size, size, 0, 0)), *(corners.tobytes() for _, corners in paints), *(colors for colors, _ in paints)))  # type: ignore[no-untyped-call]  # ty: ignore[unsound-return-statement]


def icons(source: ResolvedExistingDirectory, directory: ResolvedPath, /, *, blender: Blender, turned: tuple[tuple[str, str], ...] = ()) -> Outcome[int]:
    """Writes every `icons/**/*.svg` master of an extension folder as a preview and geometry icon, `currentColor` painting the factory theme's tool text color.

    Args:
        source: Extension folder holding the masters.
        directory: Folder the encoded icons replace in one step.
        blender: Blender executable.
        turned: Variant and master stem pairs, each variant the master turned a quarter clockwise and written beside the masters.

    Returns:
        Zero, or fault of the probe, the folder commit, and every master.
    """

    def written(stage: Path, paint: str, name: Path, master: Path, *, turn: bool) -> Outcome[Path]:
        try:
            preview, icon = encoded(master, paint, turned=turn)
            (target := stage / name).parent.mkdir(parents=True, exist_ok=True)
            target.write_text(preview)
            target.with_suffix(".dat").write_bytes(icon)
            return Ok(target)
        except (OSError, ValueError, SyntaxError) as error:
            return Error(Fault.of(Reason.FAILED if isinstance(error, OSError) else Reason.INVALID, str(master), error))

    folder = source / "icons"
    rows = (*((master.relative_to(folder), master, False) for master in sorted(folder.glob("**/*.svg"))), *((Path(f"{variant}.svg"), folder / f"{master}.svg", True) for variant, master in turned))
    return (
        probed(blender)
        .map(lambda host: Color("srgb", list(host.text)).to_string(hex=True))
        .bind(lambda paint: committed(directory, lambda stage: folder_made(stage).bind(lambda _: accumulated(written(stage, paint, name, master, turn=turn) for name, master, turn in rows))))
        .map(lambda _: 0)
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["encoded", "icons"]
