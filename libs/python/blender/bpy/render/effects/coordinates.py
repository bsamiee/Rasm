"""Compositor camera frame positions, data-window pixel grids, held clocks, noise origins, and bordered Map UV sampling."""

from typing import Literal, overload

import bpy
import msgspec

from blender.bpy.nodes.graphs import End, Expr, math, menu, Scalar, switched, vector, xyz
from blender.bpy.nodes.interface import MenuItems
from blender.imaging.geometry.stmaps import Extension

# --- [MODELS] ---------------------------------------------------------------------------


class PixelGrid(msgspec.Struct, frozen=True, kw_only=True):
    """Data-window UV centers and steps, displayed pixel centers, and single-value window offset."""

    base: Expr
    inverse: Expr
    point: Expr
    offset: Expr

    def at(self, offset: Scalar | tuple[float, ...], scale: Scalar | tuple[float, ...]) -> Expr:
        """Builds Map UV coordinate `offset ⊙ scale + base`.

        Args:
            offset: Offset in pixels.
            scale: Grid `inverse` or a signed multiple of it.

        Returns:
            `Vector` expression.
        """
        return vector("MULTIPLY_ADD", offset, scale, self.base)


class FrameSpace(msgspec.Struct, frozen=True, kw_only=True):
    """Source camera frame in displayed greater-side shares, frame fractions, pixels, and Map UV placement."""

    point: Expr
    extent: Expr
    greater: Expr
    fraction: Expr
    scale: Expr
    placing: Expr
    margin: Expr


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [FRAMES]


def gridded(image: Scalar) -> PixelGrid:
    """Builds pixel grid of image.

    Args:
        image: Image grid samples.

    Returns:
        Grid over the image's realized data window.
    """
    return _coordinates(image)[0]


def framed(source: Scalar, aspect: Scalar, scale: Scalar = 1.0) -> FrameSpace:
    """Builds positions of camera frame centered in source.

    Args:
        source: Image the build reads pixels from.
        aspect: Pixel aspect `pixel_aspect_x / pixel_aspect_y`.
        scale: Camera frame share of each source side, 1 for a source holding camera frame alone.

    Returns:
        Source `FrameSpace`.
    """
    grid, dimensions = _coordinates(source)
    axes = xyz(aspect, 1.0)
    margin = math("MULTIPLY_ADD", scale, -0.5, 0.5)
    camera, offset = vector("SCALE", dimensions, scale), vector("SCALE", dimensions, margin)
    sized = vector("MULTIPLY", camera, axes)
    greater = math("MAXIMUM", sized["X"], sized["Y"])
    point, extent = vector("DIVIDE", vector("MULTIPLY", vector("SUBTRACT", grid.point, offset), axes), greater), vector("DIVIDE", sized, greater)
    return FrameSpace(
        point=point,
        extent=extent,
        greater=greater,
        fraction=vector("DIVIDE", point, extent),
        scale=vector("DIVIDE", greater, axes),
        placing=vector("MULTIPLY", camera, grid.inverse),
        margin=vector("ADD", vector("MULTIPLY", vector("SUBTRACT", offset, grid.offset), grid.inverse), (0.0, 0.0, 1.0)),
    )


def sampled(frame: FrameSpace, fraction: Scalar) -> Expr:
    """Maps camera frame fraction to Map UV coordinate of source holding frame, coverage 1 in Z.

    Args:
        frame: Frame space of sampled source.
        fraction: Position as camera frame fractions from bottom-left corner.

    Returns:
        `Vector` expression.
    """
    return vector("MULTIPLY_ADD", fraction, frame.placing, frame.margin)


def covered(frame: FrameSpace, position: Expr) -> Expr:
    """Maps camera frame fractions to the Map UV coordinate keeping their geometric coverage in Z.

    Args:
        frame: Frame space of sampled source.
        position: Camera frame fractions with geometric coverage in Z.

    Returns:
        `Vector` expression of Map UV position and coverage.
    """
    uv = sampled(frame, position)
    return xyz(uv["X"], uv["Y"], position["Z"])


@overload
def pixels(frame: FrameSpace, length: Scalar, kind: Literal["FLOAT"] = "FLOAT") -> Expr: ...
@overload
def pixels(frame: FrameSpace, length: Scalar | tuple[float, ...], kind: Literal["VECTOR"]) -> Expr: ...
def pixels(frame: FrameSpace, length: Scalar | tuple[float, ...], kind: Literal["FLOAT", "VECTOR"] = "FLOAT") -> Expr:
    """Converts length or offset over greater frame side to stored pixels.

    Args:
        frame: `framed` space of measured source.
        length: Length, offset, or 2D size in displayed units.
        kind: `FLOAT` for stored X pixels, `VECTOR` for per-axis stored pixels.

    Returns:
        Pixel expression of `kind`.
    """
    match kind, length:
        case "FLOAT", Expr() | End() | float() as scalar:
            return math("MULTIPLY", scalar, frame.scale["X"])
        case _:
            return vector("MULTIPLY", length, frame.scale)


def shorter(extent: Scalar) -> Expr:
    """Builds smaller side of 2D extent."""
    sides = Expr(kind=bpy.types.ShaderNodeSeparateXYZ, inputs=(("Vector", extent),))
    return math("MINIMUM", msgspec.structs.replace(sides, socket="X"), msgspec.structs.replace(sides, socket="Y"))


def lensed(socket: str) -> Expr:
    """Reads one Camera Info output of the scene camera, nodes keyed `camera` and `lens`."""
    camera = Expr(kind=bpy.types.GeometryNodeInputActiveCamera, socket="Active Camera").named("camera")
    return Expr(kind=bpy.types.GeometryNodeCameraInfo, inputs=(("Camera", camera),), socket=socket).named("lens")


# --- [MOTION]


def clocked(speed: Scalar, hold: Scalar) -> Expr:
    """Clock frames as scene frame held `hold` frames at a time, scaled by `speed`."""
    frame = Expr(kind=bpy.types.CompositorNodeSceneTime, socket="Frame")
    return math("MULTIPLY", math("MULTIPLY", math("FLOOR", math("DIVIDE", frame, hold)), hold), speed)


def origin(seed: Scalar, noise: Scalar, pattern: Scalar = 0, instance: Scalar = 0) -> Expr:
    """Builds decorrelation origin of one noise.

    Args:
        seed: Record `Seed` operand.
        noise: Member of effect noise set, or stream index operand.
        pattern: Whole `clocked` frames for origins redrawn per pattern, else 0.
        instance: Repeated draw index, else 0.

    Returns:
        3D origin `Vector` expression.
    """
    hashed = Expr(kind=bpy.types.ShaderNodeTexWhiteNoise, inputs=(("Vector", xyz(seed, noise, pattern)), ("W", instance)), members=(("noise_dimensions", "4D"),), socket="Color", shape="RGBA")
    return vector("MULTIPLY", hashed, (256.0, 256.0, 256.0))


# --- [SAMPLING]


def extended(source: Scalar, uv: Expr, borders: tuple[tuple[Scalar | str, MenuItems], tuple[Scalar | str, MenuItems]], *, clipped: bool) -> Expr:
    """Samples source at Map UV positions under each axis' Extension menu, Map UV premultiplying the geometric coverage in Z.

    Args:
        source: Associated image being sampled.
        uv: Map UV positions with geometric coverage in Z.
        borders: Extension menu input or item name, and items, for X, then for Y.
        clipped: Whether Clip borders fade over the edge pixel as coverage, else Map UV clips them.

    Returns:
        Covered associated sample.
    """
    dimensions = Expr(kind=bpy.types.CompositorNodeImageInfo, inputs=(("Image", source),), socket="Dimensions", shape="VECTOR")
    held = {member: "Repeat" if member is Extension.REPEAT else "Clip" if member is Extension.CLIP and not clipped else "Extend" for member in Extension}

    def axis(name: Literal["X", "Y"], border: tuple[Scalar | str, MenuItems]) -> tuple[Expr, Expr, Expr]:
        selector, items = border
        mode = menu(selector, items, {member.label: held[member] for member in Extension}, kind="MENU")
        gate, clip = (msgspec.structs.replace(mode, socket=member.label, shape="FLOAT") for member in (Extension.MIRROR, Extension.CLIP))
        coordinate = uv[name]
        edge = math("MULTIPLY_ADD", math("MINIMUM", coordinate, 1.0 - coordinate), dimensions[name], 0.5, clamp=True)
        return switched(gate, coordinate, math("PINGPONG", coordinate, 1.0)), switched(clip, 1.0, edge), mode

    (x, x_coverage, x_mode), (y, y_coverage, y_mode) = axis("X", borders[0]), axis("Y", borders[1])
    image = Expr(
        kind=bpy.types.CompositorNodeMapUV,
        inputs=(("Image", source), ("UV", xyz(x, y, uv["Z"])), ("Interpolation", "Bilinear"), ("Extension X", x_mode), ("Extension Y", y_mode)),
        socket="Image",
        shape="RGBA",
    )
    return Expr(kind=bpy.types.CompositorNodeSetAlpha, inputs=(("Image", image), ("Alpha", x_coverage * y_coverage), ("Type", "Apply Mask")), socket="Image", shape="RGBA") if clipped else image


def _coordinates(image: Scalar) -> tuple[PixelGrid, Expr]:
    """Builds data and display coordinates over realized source."""
    realized = Expr(kind=bpy.types.CompositorNodeBlur, inputs=(("Image", image),), socket="Image", shape="RGBA")
    pixel = Expr(kind=bpy.types.CompositorNodeImageCoordinates, inputs=(("Image", realized),), socket="Pixel", shape="VECTOR")
    data = Expr(kind=bpy.types.CompositorNodeImageInfo, inputs=(("Image", image),), socket="Dimensions", shape="VECTOR")
    display = Expr(
        kind=bpy.types.CompositorNodeRelativeToPixel,
        inputs=(("Image", image), ("Float Value", 1.0)),
        members=(("data_type", "FLOAT"), ("reference_dimension", "PER_DIMENSION")),
        socket="Vector Value",
        shape="VECTOR",
    )
    lowest = tuple(Expr(kind=bpy.types.CompositorNodeLevels, inputs=(("Image", pixel), ("Channel", channel)), socket="Minimum") for channel in ("Red", "Green"))
    centre, offset, inverse = vector("ADD", pixel, (0.5, 0.5, 0.0)), xyz(*lowest), vector("DIVIDE", (1.0, 1.0, 0.0), data)
    return PixelGrid(base=vector("ADD", vector("MULTIPLY", vector("SUBTRACT", centre, offset), inverse), (0.0, 0.0, 1.0)), inverse=inverse, point=centre, offset=offset), display


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["FrameSpace", "PixelGrid", "clocked", "covered", "extended", "framed", "gridded", "lensed", "origin", "pixels", "sampled", "shorter"]
