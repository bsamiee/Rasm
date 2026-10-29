"""Compositor frame positions, data-window pixel grids, affine and projective placement, and bordered Map UV sampling."""

from enum import Enum
from typing import Final, Literal, Self

import bpy
from expression import Option, Some
import msgspec
import numpy as np
import OpenImageIO

from blender.bpy.data.rna.descriptors import Value
from blender.bpy.extension.identity import Key
from blender.bpy.nodes.desired import MenuItem, MenuItems, Socket
from blender.bpy.nodes.graphs import cell, composed, directed, Expr, inverted, math, matrix, menu, projected, Scalar, Shape, switched, Term, transform, transformed, vector, where, xyz
from blender.bpy.render.compositor.colors import AXES, with_alpha
from blender.imaging.geometry.frames import Homography

# --- [TYPES] ----------------------------------------------------------------------------


class Border(Enum):
    """Sampling past an image edge, valued by OpenImageIO's wrap mode."""

    BLACK = OpenImageIO.Wrap.Black
    CLAMP = OpenImageIO.Wrap.Clamp
    PERIODIC = OpenImageIO.Wrap.Periodic
    MIRROR = OpenImageIO.Wrap.Mirror


# --- [CONSTANTS] ------------------------------------------------------------------------

BORDERS: Final = MenuItems.of(Border)
PIN_ROWS: Final = tuple(Socket(key=key, kind=bpy.types.NodeSocketVector, description="Corner pin homography row") for key in ("first", "second", "third"))

# --- [MODELS] ---------------------------------------------------------------------------


class PixelGrid(msgspec.Struct, frozen=True, kw_only=True):
    """Data-window UV centers and steps, displayed pixel centers, single-value window offset, and displayed sides in pixels."""

    base: Expr
    inverse: Expr
    point: Expr
    offset: Expr
    sides: Expr

    @classmethod
    def of(cls, image: Scalar, /) -> Self:
        """Builds the grid over an image's realized data window."""
        realized = Expr(kind=bpy.types.CompositorNodeBlur, inputs=(("Image", image),), socket="Image", shape=bpy.types.NodeSocketColor)
        pixel = Expr(kind=bpy.types.CompositorNodeImageCoordinates, inputs=(("Image", realized),), socket="Pixel", shape=bpy.types.NodeSocketVector)
        lowest = tuple(Expr(kind=bpy.types.CompositorNodeLevels, inputs=(("Image", pixel), ("Channel", channel)), socket="Minimum") for channel in ("Red", "Green"))
        centre, offset, inverse = vector("ADD", pixel, (0.5, 0.5, 0.0)), xyz(*lowest), vector("DIVIDE", (1.0, 1.0, 0.0), dimensions(image))
        sides = Expr(
            kind=bpy.types.CompositorNodeRelativeToPixel,
            inputs=(("Image", image), ("Float Value", 1.0)),
            members={"data_type": "FLOAT", "reference_dimension": "PER_DIMENSION"},
            socket="Vector Value",
            shape=bpy.types.NodeSocketVector,
        )
        return cls(base=vector("ADD", vector("MULTIPLY", vector("SUBTRACT", centre, offset), inverse), (0.0, 0.0, 1.0)), inverse=inverse, point=centre, offset=offset, sides=sides)

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

    # --- [POSITIONS]

    @classmethod
    def of(cls, source: Scalar, aspect: Scalar, scale: Scalar = 1.0, /) -> Self:
        """Builds positions of the camera frame centered in a source.

        Args:
            source: Image the build reads pixels from.
            aspect: Pixel aspect `pixel_aspect_x / pixel_aspect_y`.
            scale: Camera frame share of each source side, 1 for a source holding camera frame alone.

        Returns:
            Source frame space.
        """
        grid = PixelGrid.of(source)
        axes = xyz(aspect, 1.0)
        margin = math("MULTIPLY_ADD", scale, -0.5, 0.5)
        camera, offset = vector("SCALE", grid.sides, scale), vector("SCALE", grid.sides, margin)
        sized = vector("MULTIPLY", camera, axes)
        greater = math("MAXIMUM", sized["X"], sized["Y"])
        point, extent = vector("DIVIDE", vector("MULTIPLY", vector("SUBTRACT", grid.point, offset), axes), greater), vector("DIVIDE", sized, greater)
        return cls(
            point=point,
            extent=extent,
            greater=greater,
            fraction=vector("DIVIDE", point, extent),
            scale=vector("DIVIDE", greater, axes),
            placing=vector("MULTIPLY", camera, grid.inverse),
            margin=vector("ADD", vector("MULTIPLY", vector("SUBTRACT", offset, grid.offset), grid.inverse), (0.0, 0.0, 1.0)),
        )

    def sampled(self, fraction: Scalar) -> Expr:
        """Maps camera frame fractions from the bottom-left corner to the Map UV coordinate of the source holding the frame, coverage 1 in Z."""
        return vector("MULTIPLY_ADD", fraction, self.placing, self.margin)

    def covered(self, position: Expr) -> Expr:
        """Maps camera frame fractions to the Map UV coordinate keeping their geometric coverage in Z."""
        uv = self.sampled(position)
        return xyz(uv["X"], uv["Y"], position["Z"])

    def entering(self, uv: Scalar | tuple[float, ...]) -> Expr:
        """Maps a Map UV coordinate of the source back to camera frame fractions with unit coverage in Z."""
        return vector("ADD", vector("DIVIDE", vector("SUBTRACT", uv, self.margin), self.placing), (0.0, 0.0, 1.0))

    def pixels(self, length: Scalar) -> Expr:
        """Converts a length over the greater frame side to stored X pixels."""
        return math("MULTIPLY", length, self.scale["X"])

    def pixel_size(self, length: Scalar | tuple[float, ...]) -> Expr:
        """Converts a length or 2D size over the greater frame side to stored pixels per axis."""
        return vector("MULTIPLY", length, self.scale)

    # --- [PLACEMENT]

    def placement(self, position: Scalar | tuple[float, ...], rotation: Scalar, scale: Scalar | tuple[float, ...], skew: Scalar | tuple[float, ...], pivot: Scalar | tuple[float, ...]) -> Expr:
        """Builds scale, X and Y shear, and rotation about pivot, then translation.

        Args:
            position: Translation as shares of each displayed side.
            rotation: Counterclockwise angle in radians.
            scale: Signed scale pair.
            skew: Horizontal then vertical shear angles.
            pivot: Fixed point as shares of each displayed side.

        Returns:
            Forward affine `Matrix` expression, singular and reflected placements included.
        """
        anchor = vector("MULTIPLY", pivot, self.extent)
        scale_x, scale_y, skew_x, skew_y = (
            term[axis] if isinstance(term, Expr) else Expr(kind=bpy.types.ShaderNodeSeparateXYZ, inputs=(("Vector", term),), socket=axis) for term in (scale, skew) for axis in AXES[:2]
        )
        horizontal, vertical = matrix({(1, 2): math("TANGENT", skew_x)}), matrix({(2, 1): math("TANGENT", skew_y)})
        turn = transform("Rotation", Expr(kind=bpy.types.FunctionNodeEulerToRotation, inputs=(("Euler", xyz(z=rotation)),), socket="Rotation"))
        linear = composed(turn, composed(vertical, composed(horizontal, transform("Scale", xyz(scale_x, scale_y, 1.0)))))
        return composed(transform("Translation", vector("SUBTRACT", vector("ADD", vector("MULTIPLY", position, self.extent), anchor), directed(anchor, linear))), linear)

    def placed(self, source: Scalar, forward: Expr) -> Expr:
        """Places the displayed rectangle through rotate, scale, rotate, and translate nodes of the matrix's singular value decomposition, keeping deferred bounds.

        Args:
            source: Associated content with its transformation.
            forward: Forward matrix from `placement` in the same frame.

        Returns:
            Content transformed about its center, transparent when singular.
        """
        basis = transform("Scale", xyz(self.scale["X"], self.scale["Y"], 1.0))
        pixel = composed(basis, composed(forward, inverted(basis)))
        planar = matrix({**{(row, column): cell(pixel, row, column) for column in range(1, 3) for row in range(1, 3)}, (3, 3): 0.0})
        decomposed = Expr(kind=bpy.types.FunctionNodeMatrixSVD, inputs=(("Matrix", planar),), socket="S", shape=bpy.types.NodeSocketVector)
        reflection = math("SIGN", Expr(kind=bpy.types.FunctionNodeMatrixDeterminant, inputs=(("Matrix", pixel),), socket="Determinant"))
        center = vector("SCALE", self.extent, 0.5)
        location = msgspec.structs.replace(dimensions(source), socket="Location")

        def rotated(image: Expr, side: Literal["U", "V"], direction: float) -> Expr:
            factor = msgspec.structs.replace(decomposed, socket=side, shape=bpy.types.NodeSocketMatrix)
            angle = math("ARCTAN2", cell(factor, 2, 1), cell(factor, 1, 1)) * direction
            return Expr(kind=bpy.types.CompositorNodeRotate, inputs=(("Image", image), ("Angle", angle)), socket="Image", shape=bpy.types.NodeSocketColor)

        def translated(image: Scalar, offset: Expr, *extras: tuple[str, str]) -> Expr:
            return Expr(kind=bpy.types.CompositorNodeTranslate, inputs=(("Image", image), ("X", offset["X"]), ("Y", offset["Y"]), *extras), socket="Image", shape=bpy.types.NodeSocketColor)

        unrotated = rotated(translated(source, vector("SCALE", location, -1.0)), "V", -1.0)
        stretched = Expr(
            kind=bpy.types.CompositorNodeScale,
            inputs=(("Image", unrotated), ("Type", "Relative"), ("X", decomposed["X"]), ("Y", decomposed["Y"] * reflection)),
            socket="Image",
            shape=bpy.types.NodeSocketColor,
        )
        shifted = vector("ADD", location, vector("MULTIPLY", vector("SUBTRACT", transformed(center, forward), center), self.scale))
        image = translated(rotated(stretched, "U", 1.0), shifted, ("Interpolation", "Bilinear"), ("Extension X", "Clip"), ("Extension Y", "Clip"))
        return switched(msgspec.structs.replace(inverted(pixel), socket="Invertible", shape=bpy.types.NodeSocketBool), (0.0, 0.0, 0.0, 0.0), image)


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [FRAMES]


def dimensions(image: Scalar) -> Expr:
    """Data-window size of an image in pixels."""
    return Expr(kind=bpy.types.CompositorNodeImageInfo, inputs=(("Image", image),), socket="Dimensions", shape=bpy.types.NodeSocketVector)


def shorter(extent: Expr) -> Expr:
    """Builds smaller side of 2D extent."""
    return math("MINIMUM", extent["X"], extent["Y"])


def pin_rows(homography: Option[Homography]) -> dict[Key, Value]:
    """Returns the `PIN_ROWS` values of the homography taking the unit plane onto an image quad, the identity rows without one."""
    rows = homography.map(lambda found: found.matrix).default_with(lambda: np.eye(3)).tolist()
    return {socket.key: tuple(row) for socket, row in zip(PIN_ROWS, rows, strict=True)}


def pinned(fraction: Expr, rows: tuple[Expr, Expr, Expr], *, forward: bool) -> Expr:
    """Maps camera frame fractions through a corner pin homography, coverage in Z kept where the position faces the plane.

    Args:
        fraction: Camera frame fractions with geometric coverage in Z.
        rows: Homography rows taking the unit plane onto the image quad, the `PIN_ROWS` inputs.
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


def uv_mapped(image: Term, uv: Term, *, interpolation: str = "Bilinear", extension: tuple[Term, Term] = ("Clip", "Clip"), shape: Shape = bpy.types.NodeSocketColor) -> Expr:
    """Samples an image at Map UV positions under one interpolation and each axis' extension, Map UV premultiplying the coverage in Z."""
    inputs = (("Image", image), ("UV", uv), ("Interpolation", interpolation), ("Extension X", extension[0]), ("Extension Y", extension[1]))
    return Expr(kind=bpy.types.CompositorNodeMapUV, inputs=inputs, socket="Image", shape=shape)


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
        mode = menu(selector, {MenuItem.of(member): native[member] for member in Border}, shape=Some(bpy.types.NodeSocketMenu))
        mirrored, black = (msgspec.structs.replace(mode, socket=MenuItem.of(member).name, shape=bpy.types.NodeSocketFloat) for member in (Border.MIRROR, Border.BLACK))
        coordinate = uv[name]
        edge = math("MULTIPLY_ADD", math("MINIMUM", coordinate, 1.0 - coordinate), sides[name], 0.5, clamp=True)
        return switched(mirrored, coordinate, math("PINGPONG", coordinate, 1.0)), switched(black, 1.0, edge), mode

    (x, x_coverage, x_mode), (y, y_coverage, y_mode) = axis("X", borders[0]), axis("Y", borders[1])
    image = uv_mapped(source, xyz(x, y, uv["Z"]), extension=(x_mode, y_mode))
    return with_alpha(image, x_coverage * y_coverage, masked=True) if clipped else image


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["BORDERS", "PIN_ROWS", "Border", "FrameSpace", "PixelGrid", "dimensions", "extended", "pin_rows", "pinned", "shorter", "uv_mapped"]
