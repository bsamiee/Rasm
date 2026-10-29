"""Compositor frame positions, data-window pixel grids, affine and projective placement, and bordered Map UV sampling."""

from enum import Enum
from typing import Final, Literal, overload, Self

import bpy
from expression import Some
import msgspec
import OpenImageIO

from blender.bpy.nodes.desired import End
from blender.bpy.nodes.graphs import cell, composed, directed, Expr, inverted, math, matrix, menu, projected, Scalar, switched, transform, transformed, vector, where, xyz
from blender.bpy.nodes.interface import MenuItem, MenuItems
from blender.bpy.render.compositor.colors import AXES
from blender.imaging.geometry.frames import Homography
from blender.records.fields import Finite

# --- [TYPES] ----------------------------------------------------------------------------


class Border(Enum):
    """Sampling past an image edge, valued by OpenImageIO's wrap mode."""

    BLACK = OpenImageIO.Wrap.Black
    CLAMP = OpenImageIO.Wrap.Clamp
    PERIODIC = OpenImageIO.Wrap.Periodic
    MIRROR = OpenImageIO.Wrap.Mirror

    @property
    def wrap(self) -> OpenImageIO.Wrap:
        """OpenImageIO wrap mode of the border."""
        return self.value

    @property
    def item(self) -> MenuItem:
        """Menu item of the border."""
        return MenuItem(name=bpy.path.display_name(self.name.lower()))


# --- [CONSTANTS] ------------------------------------------------------------------------

BORDERS: Final = MenuItems(items=tuple(border.item for border in Border))

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


class Projective(msgspec.Struct, frozen=True, kw_only=True):
    """Rows of the homography taking the unit plane onto an image quad, derived inputs of a corner pin."""

    first: tuple[Finite, Finite, Finite] = (1.0, 0.0, 0.0)
    second: tuple[Finite, Finite, Finite] = (0.0, 1.0, 0.0)
    third: tuple[Finite, Finite, Finite] = (0.0, 0.0, 1.0)

    @classmethod
    def of(cls, homography: Homography) -> Self:
        """Reads the rows of a homography matrix."""
        first, second, third = homography.matrix.tolist()
        return msgspec.convert({"first": first, "second": second, "third": third}, cls)


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [FRAMES]


def dimensions(image: Scalar) -> Expr:
    """Data-window size of an image in pixels."""
    return Expr(kind=bpy.types.CompositorNodeImageInfo, inputs=(("Image", image),), socket="Dimensions", shape=bpy.types.NodeSocketVector)


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
    grid, sides = _coordinates(source)
    axes = xyz(aspect, 1.0)
    margin = math("MULTIPLY_ADD", scale, -0.5, 0.5)
    camera, offset = vector("SCALE", sides, scale), vector("SCALE", sides, margin)
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


def entering(frame: FrameSpace, uv: Scalar | tuple[float, ...]) -> Expr:
    """Maps a Map UV coordinate of the source back to camera frame fractions with unit coverage in Z."""
    return vector("ADD", vector("DIVIDE", vector("SUBTRACT", uv, frame.margin), frame.placing), (0.0, 0.0, 1.0))


@overload
def pixels(frame: FrameSpace, length: Scalar, kind: type[bpy.types.NodeSocketFloat] = bpy.types.NodeSocketFloat) -> Expr: ...
@overload
def pixels(frame: FrameSpace, length: Scalar | tuple[float, ...], kind: type[bpy.types.NodeSocketVector]) -> Expr: ...
def pixels(frame: FrameSpace, length: Scalar | tuple[float, ...], kind: type[bpy.types.NodeSocketFloat | bpy.types.NodeSocketVector] = bpy.types.NodeSocketFloat) -> Expr:
    """Converts length or offset over greater frame side to stored pixels.

    Args:
        frame: `framed` space of measured source.
        length: Length, offset, or 2D size in displayed units.
        kind: Float socket class for stored X pixels, vector socket class for per-axis stored pixels.

    Returns:
        Pixel expression of `kind`.
    """
    match length:
        case Expr() | End() | float() as scalar if kind is bpy.types.NodeSocketFloat:
            return math("MULTIPLY", scalar, frame.scale["X"])
        case _:
            return vector("MULTIPLY", length, frame.scale)


def shorter(extent: Expr) -> Expr:
    """Builds smaller side of 2D extent."""
    return math("MINIMUM", extent["X"], extent["Y"])


# --- [PLACEMENT]


def placement(
    frame: FrameSpace, position: Scalar | tuple[float, ...], rotation: Scalar, scale: Scalar | tuple[float, ...], skew: Scalar | tuple[float, ...], pivot: Scalar | tuple[float, ...]
) -> Expr:
    """Builds scale, X and Y shear, and rotation about pivot, then translation.

    Args:
        frame: Displayed frame coordinates.
        position: Translation as shares of each displayed side.
        rotation: Counterclockwise angle in radians.
        scale: Signed scale pair.
        skew: Horizontal then vertical shear angles.
        pivot: Fixed point as shares of each displayed side.

    Returns:
        Forward affine `Matrix` expression, singular and reflected placements included.
    """
    anchor = vector("MULTIPLY", pivot, frame.extent)
    scale_x, scale_y, skew_x, skew_y = (
        term[axis] if isinstance(term, Expr) else Expr(kind=bpy.types.ShaderNodeSeparateXYZ, inputs=(("Vector", term),), socket=axis) for term in (scale, skew) for axis in AXES[:2]
    )
    horizontal, vertical = matrix({(1, 2): math("TANGENT", skew_x)}), matrix({(2, 1): math("TANGENT", skew_y)})
    turn = transform("Rotation", Expr(kind=bpy.types.FunctionNodeEulerToRotation, inputs=(("Euler", xyz(z=rotation)),), socket="Rotation"))
    linear = composed(turn, composed(vertical, composed(horizontal, transform("Scale", xyz(scale_x, scale_y, 1.0)))))
    return composed(transform("Translation", vector("SUBTRACT", vector("ADD", vector("MULTIPLY", position, frame.extent), anchor), directed(anchor, linear))), linear)


def placed(source: Scalar, frame: FrameSpace, forward: Expr) -> Expr:
    """Places current displayed rectangle through rotate, scale, rotate, and translate nodes of the matrix's singular value decomposition, keeping deferred bounds.

    Args:
        source: Associated content with its transformation.
        frame: Current displayed extent and conversion to stored pixel distances.
        forward: Forward matrix from `placement` in same frame.

    Returns:
        Content transformed about its center, transparent when singular.
    """
    basis = transform("Scale", xyz(frame.scale["X"], frame.scale["Y"], 1.0))
    pixel = composed(basis, composed(forward, inverted(basis)))
    planar = matrix({**{(row, column): cell(pixel, row, column) for column in range(1, 3) for row in range(1, 3)}, (3, 3): 0.0})
    decomposed = Expr(kind=bpy.types.FunctionNodeMatrixSVD, inputs=(("Matrix", planar),), socket="S", shape=bpy.types.NodeSocketVector)
    reflection = math("SIGN", Expr(kind=bpy.types.FunctionNodeMatrixDeterminant, inputs=(("Matrix", pixel),), socket="Determinant"))
    center = vector("SCALE", frame.extent, 0.5)
    location = Expr(kind=bpy.types.CompositorNodeImageInfo, inputs=(("Image", source),), socket="Location", shape=bpy.types.NodeSocketVector)

    def rotated(image: Expr, side: Literal["U", "V"], direction: float) -> Expr:
        factor = msgspec.structs.replace(decomposed, socket=side, shape=bpy.types.NodeSocketMatrix)
        return Expr(
            kind=bpy.types.CompositorNodeRotate,
            inputs=(("Image", image), ("Angle", math("ARCTAN2", cell(factor, 2, 1), cell(factor, 1, 1)) * direction)),
            socket="Image",
            shape=bpy.types.NodeSocketColor,
        )

    def translated(image: Scalar, offset: Expr, *extras: tuple[str, str]) -> Expr:
        return Expr(kind=bpy.types.CompositorNodeTranslate, inputs=(("Image", image), ("X", offset["X"]), ("Y", offset["Y"]), *extras), socket="Image", shape=bpy.types.NodeSocketColor)

    unrotated = rotated(translated(source, vector("SCALE", location, -1.0)), "V", -1.0)
    stretched = Expr(
        kind=bpy.types.CompositorNodeScale,
        inputs=(("Image", unrotated), ("Type", "Relative"), ("X", decomposed["X"]), ("Y", decomposed["Y"] * reflection)),
        socket="Image",
        shape=bpy.types.NodeSocketColor,
    )
    shifted = vector("ADD", location, vector("MULTIPLY", vector("SUBTRACT", transformed(center, forward), center), frame.scale))
    image = translated(rotated(stretched, "U", 1.0), shifted, ("Interpolation", "Bilinear"), ("Extension X", "Clip"), ("Extension Y", "Clip"))
    return switched(msgspec.structs.replace(inverted(pixel), socket="Invertible", shape=bpy.types.NodeSocketBool), (0.0, 0.0, 0.0, 0.0), image)


def pinned(fraction: Expr, rows: tuple[Expr, Expr, Expr], *, forward: bool) -> Expr:
    """Maps camera frame fractions through a corner pin homography, coverage in Z kept where the position faces the plane.

    Args:
        fraction: Camera frame fractions with geometric coverage in Z.
        rows: Homography rows taking the unit plane onto the image quad, the `Projective` inputs.
        forward: Plane to image in place of image to plane sampling.

    Returns:
        Mapped fractions with front-facing coverage in Z.
    """
    homography = matrix({(row, column): held[axis] for row, held in zip((1, 2, 4), rows) for column, axis in zip((1, 2, 4), AXES)})
    mapping = homography if forward else inverted(homography)
    planar = vector("MULTIPLY", fraction, (1.0, 1.0, 0.0))
    front = vector("DOT_PRODUCT", xyz(cell(mapping, 4, 1), cell(mapping, 4, 2), cell(mapping, 4, 4)), vector("ADD", planar, (0.0, 0.0, 1.0))) > 0.0
    position = where(front, projected(planar, mapping), (0.0, 0.0, 0.0))
    return vector("MULTIPLY_ADD", (0.0, 0.0, 1.0), fraction["Z"] * front, position)


# --- [SAMPLING]


def extended(source: Scalar, uv: Expr, borders: tuple[Scalar | str, Scalar | str], *, clipped: bool) -> Expr:
    """Samples source at Map UV positions under each axis' `Border` menu, Map UV premultiplying the geometric coverage in Z.

    Args:
        source: Associated image being sampled.
        uv: Map UV positions with geometric coverage in Z.
        borders: `Border` menu input or item name for X, then for Y.
        clipped: Whether black borders fade over the edge pixel as coverage, else Map UV clips them.

    Returns:
        Covered associated sample.
    """
    sides = dimensions(source)
    native = {Border.BLACK: "Extend" if clipped else "Clip", Border.CLAMP: "Extend", Border.PERIODIC: "Repeat", Border.MIRROR: "Extend"}

    def axis(name: Literal["X", "Y"], selector: Scalar | str) -> tuple[Expr, Expr, Expr]:
        mode = menu(selector, {member.item: native[member] for member in Border}, shape=Some(bpy.types.NodeSocketMenu))
        mirrored, black = (msgspec.structs.replace(mode, socket=member.item.name, shape=bpy.types.NodeSocketFloat) for member in (Border.MIRROR, Border.BLACK))
        coordinate = uv[name]
        edge = math("MULTIPLY_ADD", math("MINIMUM", coordinate, 1.0 - coordinate), sides[name], 0.5, clamp=True)
        return switched(mirrored, coordinate, math("PINGPONG", coordinate, 1.0)), switched(black, 1.0, edge), mode

    (x, x_coverage, x_mode), (y, y_coverage, y_mode) = axis("X", borders[0]), axis("Y", borders[1])
    image = Expr(
        kind=bpy.types.CompositorNodeMapUV,
        inputs=(("Image", source), ("UV", xyz(x, y, uv["Z"])), ("Interpolation", "Bilinear"), ("Extension X", x_mode), ("Extension Y", y_mode)),
        socket="Image",
        shape=bpy.types.NodeSocketColor,
    )
    return (
        Expr(kind=bpy.types.CompositorNodeSetAlpha, inputs=(("Image", image), ("Alpha", x_coverage * y_coverage), ("Type", "Apply Mask")), socket="Image", shape=bpy.types.NodeSocketColor)
        if clipped
        else image
    )


def _coordinates(image: Scalar) -> tuple[PixelGrid, Expr]:
    """Builds data and display coordinates over realized source."""
    realized = Expr(kind=bpy.types.CompositorNodeBlur, inputs=(("Image", image),), socket="Image", shape=bpy.types.NodeSocketColor)
    pixel = Expr(kind=bpy.types.CompositorNodeImageCoordinates, inputs=(("Image", realized),), socket="Pixel", shape=bpy.types.NodeSocketVector)
    display = Expr(
        kind=bpy.types.CompositorNodeRelativeToPixel,
        inputs=(("Image", image), ("Float Value", 1.0)),
        members={"data_type": "FLOAT", "reference_dimension": "PER_DIMENSION"},
        socket="Vector Value",
        shape=bpy.types.NodeSocketVector,
    )
    lowest = tuple(Expr(kind=bpy.types.CompositorNodeLevels, inputs=(("Image", pixel), ("Channel", channel)), socket="Minimum") for channel in ("Red", "Green"))
    centre, offset, inverse = vector("ADD", pixel, (0.5, 0.5, 0.0)), xyz(*lowest), vector("DIVIDE", (1.0, 1.0, 0.0), dimensions(image))
    return PixelGrid(base=vector("ADD", vector("MULTIPLY", vector("SUBTRACT", centre, offset), inverse), (0.0, 0.0, 1.0)), inverse=inverse, point=centre, offset=offset), display


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "BORDERS",
    "Border",
    "FrameSpace",
    "PixelGrid",
    "Projective",
    "covered",
    "dimensions",
    "entering",
    "extended",
    "framed",
    "gridded",
    "pinned",
    "pixels",
    "placed",
    "placement",
    "sampled",
    "shorter",
]
