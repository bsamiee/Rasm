"""Compositor frame positions, data-window pixel grids, affine and projective placement, unit-interval border folds, and bordered Map UV sampling."""

from enum import IntEnum
from typing import Final, Literal, Self

import bpy
from expression import Option, Some
import msgspec
import numpy as np
import OpenImageIO

from rasm_bpy.data._extension.identity import Key
from rasm_bpy.data._rna.descriptors import Value
from rasm_bpy.nodes._desired import MenuItem, MenuItems
from rasm_bpy.nodes._graphs import cell, composed, directed, Expr, inverted, math, matrix, menu, projected, Scalar, switched, Term, transform, transformed, vector, where, xyz
from rasm_bpy.nodes._interfaces import Socket
from rasm_bpy.render._compositor.colors import AXES, with_alpha
from rasm_imaging import Homography

# --- [TYPES] ----------------------------------------------------------------------------


class Border(IntEnum):
    """Sampling past an image edge, valued by the number of OpenImageIO's wrap mode, records holding it by that number."""

    BLACK = OpenImageIO.Wrap.Black.value
    CLAMP = OpenImageIO.Wrap.Clamp.value
    PERIODIC = OpenImageIO.Wrap.Periodic.value
    MIRROR = OpenImageIO.Wrap.Mirror.value

    @property
    def wrap(self) -> OpenImageIO.Wrap:
        """OpenImageIO wrap mode of the border."""
        return OpenImageIO.Wrap(self.value)


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
        realized = Expr(kind=bpy.types.CompositorNodeBlur, inputs=(("Image", image),), socket="Image", socket_type=bpy.types.NodeSocketColor)
        pixel = Expr(kind=bpy.types.CompositorNodeImageCoordinates, inputs=(("Image", realized),), socket="Pixel", socket_type=bpy.types.NodeSocketVector)
        lowest = tuple(Expr(kind=bpy.types.CompositorNodeLevels, inputs=(("Image", pixel), ("Channel", channel)), socket="Minimum") for channel in ("Red", "Green"))
        centre, offset, inverse = vector("ADD", pixel, (0.5, 0.5, 0.0)), xyz(*lowest), vector("DIVIDE", (1.0, 1.0, 0.0), dimensions(image))
        sides = Expr(
            kind=bpy.types.CompositorNodeRelativeToPixel,
            inputs=(("Image", image), ("Float Value", 1.0)),
            members={"data_type": "FLOAT", "reference_dimension": "PER_DIMENSION"},
            socket="Vector Value",
            socket_type=bpy.types.NodeSocketVector,
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

    # --- [STATE]

    point: Expr
    extent: Expr
    greater: Expr
    fraction: Expr
    scale: Expr
    uv_scale: Expr
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
            uv_scale=vector("MULTIPLY", camera, grid.inverse),
            margin=vector("ADD", vector("MULTIPLY", vector("SUBTRACT", offset, grid.offset), grid.inverse), (0.0, 0.0, 1.0)),
        )

    def sampled(self, fraction: Scalar) -> Expr:
        """Maps camera frame fractions from the bottom-left corner to the Map UV coordinate of the source holding the frame, coverage 1 in Z."""
        return vector("MULTIPLY_ADD", fraction, self.uv_scale, self.margin)

    def covered(self, position: Expr) -> Expr:
        """Maps camera frame fractions to the Map UV coordinate keeping their geometric coverage in Z."""
        uv = self.sampled(position)
        return xyz(uv["X"], uv["Y"], position["Z"])

    def from_uv(self, uv: Scalar | tuple[float, ...]) -> Expr:
        """Maps a Map UV coordinate of the source back to camera frame fractions with unit coverage in Z."""
        return vector("ADD", vector("DIVIDE", vector("SUBTRACT", uv, self.margin), self.uv_scale), (0.0, 0.0, 1.0))

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
        pivot_point = vector("MULTIPLY", pivot, self.extent)
        scale_x, scale_y, skew_x, skew_y = (
            term[axis] if isinstance(term, Expr) else Expr(kind=bpy.types.ShaderNodeSeparateXYZ, inputs=(("Vector", term),), socket=axis) for term in (scale, skew) for axis in AXES[:2]
        )
        horizontal, vertical = matrix({(1, 2): math("TANGENT", skew_x)}), matrix({(2, 1): math("TANGENT", skew_y)})
        turn = transform("Rotation", Expr(kind=bpy.types.FunctionNodeEulerToRotation, inputs=(("Euler", xyz(z=rotation)),), socket="Rotation"))
        linear = composed(turn, composed(vertical, composed(horizontal, transform("Scale", xyz(scale_x, scale_y, 1.0)))))
        return composed(transform("Translation", vector("SUBTRACT", vector("ADD", vector("MULTIPLY", position, self.extent), pivot_point), directed(pivot_point, linear))), linear)

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
        decomposed = Expr(kind=bpy.types.FunctionNodeMatrixSVD, inputs=(("Matrix", planar),), socket="S", socket_type=bpy.types.NodeSocketVector)
        reflection = math("SIGN", Expr(kind=bpy.types.FunctionNodeMatrixDeterminant, inputs=(("Matrix", pixel),), socket="Determinant"))
        center = vector("SCALE", self.extent, 0.5)
        location = dimensions(source).at("Location", bpy.types.NodeSocketVector)

        def rotated(image: Expr, side: Literal["U", "V"], direction: float) -> Expr:
            factor = decomposed.at(side, bpy.types.NodeSocketMatrix)
            angle = math("ARCTAN2", cell(factor, 2, 1), cell(factor, 1, 1)) * direction
            return Expr(kind=bpy.types.CompositorNodeRotate, inputs=(("Image", image), ("Angle", angle)), socket="Image", socket_type=bpy.types.NodeSocketColor)

        def translated(image: Scalar, offset: Expr) -> Expr:
            return Expr(kind=bpy.types.CompositorNodeTranslate, inputs=(("Image", image), ("X", offset["X"]), ("Y", offset["Y"])), socket="Image", socket_type=bpy.types.NodeSocketColor)

        unrotated = rotated(translated(source, vector("SCALE", location, -1.0)), "V", -1.0)
        stretched = Expr(
            kind=bpy.types.CompositorNodeScale, inputs=(("Image", unrotated), ("X", decomposed["X"]), ("Y", decomposed["Y"] * reflection)), socket="Image", socket_type=bpy.types.NodeSocketColor
        )
        shifted = vector("ADD", location, vector("MULTIPLY", vector("SUBTRACT", transformed(center, forward), center), self.scale))
        image = translated(rotated(stretched, "U", 1.0), shifted)
        return switched(inverted(pixel).at("Invertible", bpy.types.NodeSocketBool), (0.0, 0.0, 0.0, 0.0), image)


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [FRAMES]


def dimensions(image: Scalar) -> Expr:
    """Data-window size of an image in pixels."""
    return Expr(kind=bpy.types.CompositorNodeImageInfo, inputs=(("Image", image),), socket="Dimensions", socket_type=bpy.types.NodeSocketVector)


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


def uv_mapped(image: Term, uv: Term, *, interpolation: Term = "Bilinear", extension: tuple[Term, Term] = ("Clip", "Clip"), socket_type: type[bpy.types.NodeSocket] = bpy.types.NodeSocketColor) -> Expr:
    """Samples an image at Map UV positions under one interpolation and each axis' extension, Map UV premultiplying the coverage in Z."""
    inputs = (("Image", image), ("UV", uv), ("Interpolation", interpolation), ("Extension X", extension[0]), ("Extension Y", extension[1]))
    return Expr(kind=bpy.types.CompositorNodeMapUV, inputs=inputs, socket="Image", socket_type=socket_type)


def folded(border: Border | Scalar, coordinate: Expr, /) -> tuple[Expr, Expr | float]:
    """Folds a coordinate into the unit interval under a border, with the coverage a black border keeps inside the interval and every other border keeps whole.

    Args:
        border: `Border` member, chosen at build time, or a `BORDERS` menu operand.
        coordinate: Coordinate whose unit interval the border extends.

    Returns:
        Folded coordinate and its coverage, one Menu Switch choosing the fold under a menu operand and its black item's boolean the coverage.
    """
    clamped = Expr(kind=bpy.types.ShaderNodeClamp, inputs=(("Value", coordinate),), socket="Result")
    inside = (1.0 - (coordinate < 0.0)) * (1.0 - (coordinate > 1.0))
    folds = {Border.BLACK: clamped, Border.CLAMP: clamped, Border.PERIODIC: math("FRACT", coordinate), Border.MIRROR: math("PINGPONG", coordinate, 1.0)}
    match border:
        case Border.BLACK:
            return clamped, inside
        case Border():
            return folds[border], 1.0
        case selector:
            fold = menu(selector, {MenuItem.of(member): held for member, held in folds.items()})
            return fold, switched(fold.at(MenuItem.of(Border.BLACK).name), 1.0, inside)


def extended(source: Scalar, uv: Expr, borders: Border | Scalar | tuple[Border | Scalar, Border | Scalar], *, clipped: bool, interpolation: Term = "Bilinear") -> Expr:
    """Samples source at Map UV positions past its edges under `Border`s, a member through Map UV's native extension and a menu operand through one Menu Switch, Map UV premultiplying the geometric coverage in Z.

    Args:
        source: Associated image being sampled.
        uv: Map UV positions with geometric coverage in Z.
        borders: One border for both axes, or one for X then one for Y, each a `Border` member or a `BORDERS` menu operand.
        clipped: Whether black borders fade over the edge pixel as coverage, else Map UV clips them.
        interpolation: Map UV interpolation item name or menu operand (`Anisotropic` filtering each lookup over its UV footprint).

    Returns:
        Covered associated sample.
    """
    sides = dimensions(source)
    native = {Border.BLACK: "Extend" if clipped else "Clip", Border.CLAMP: "Extend", Border.PERIODIC: "Repeat", Border.MIRROR: "Extend"}

    def switch(border: Border | Scalar) -> Border | Expr:
        match border:
            case Border():
                return border
            case selector:
                return menu(selector, {MenuItem.of(member): native[member] for member in Border}, socket_type=Some(bpy.types.NodeSocketMenu))

    def axis(name: Literal["X", "Y"], choice: Border | Expr) -> tuple[Expr, Expr | float, Term]:
        coordinate = uv[name]
        mirrored = math("PINGPONG", coordinate, 1.0)
        edge = math("MULTIPLY_ADD", math("MINIMUM", coordinate, 1.0 - coordinate), sides[name], 0.5, clamp=True)
        match choice:
            case Border.MIRROR:
                return mirrored, 1.0, native[choice]
            case Border.BLACK:
                return coordinate, edge, native[choice]
            case Border():
                return coordinate, 1.0, native[choice]
            case mode:
                return switched(mode.at(MenuItem.of(Border.MIRROR).name), coordinate, mirrored), switched(mode.at(MenuItem.of(Border.BLACK).name), 1.0, edge), mode

    across, up = (switch(borders[0]), switch(borders[1])) if isinstance(borders, tuple) else (switch(borders),) * 2
    (x, x_coverage, x_extension), (y, y_coverage, y_extension) = axis("X", across), axis("Y", up)
    image, coverage = uv_mapped(source, xyz(x, y, uv["Z"]), interpolation=interpolation, extension=(x_extension, y_extension)), x_coverage * y_coverage
    return with_alpha(image, coverage, masked=True) if clipped and isinstance(coverage, Expr) else image


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["BORDERS", "PIN_ROWS", "Border", "FrameSpace", "PixelGrid", "dimensions", "extended", "folded", "pin_rows", "pinned", "shorter", "uv_mapped"]
