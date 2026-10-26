"""Compositor frame positions, data-window pixel grids, clocks and shutter poses, affine and projective placement, camera re-aim, and bordered Map UV sampling."""

from typing import Annotated, Literal, overload, Self

import bpy
from expression import Some
import msgspec
import msgspec.inspect
import numpy as np
import OpenImageIO

from blender.bpy.data.rna import animation
from blender.bpy.data.rna.animation import Animation, Keys
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.faults import BlenderResult
from blender.bpy.nodes.graphs import End, Expr, math, menu, Scalar, switched, Term, vector, where, xyz
from blender.bpy.nodes.interface import item_key, MenuItems
from blender.bpy.render.compositor.colors import AXES, composed, inverted
from blender.bpy.render.compositor.inputs import entry_input, Field, path, reached
from blender.bpy.settings.shapes import unwrapped
from blender.imaging.geometry.homography import Homography
from blender.records.faults import accumulated, paired
from blender.records.fields import Case, Choice, Finite, NonNegative, Presentation

# --- [TYPES] ----------------------------------------------------------------------------


class Border(Choice):
    """Sampling past an image edge under OpenImageIO's wrap names and values."""

    BLACK = 1
    CLAMP = 2
    PERIODIC = 3
    MIRROR = 4

    @property
    def wrap(self) -> OpenImageIO.Wrap:
        """OpenImageIO wrap mode of the border."""
        return OpenImageIO.Wrap(self.value)


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


class Clock(msgspec.Struct, frozen=True, kw_only=True):
    """Clock speed and frame hold of animated parameters."""

    speed: Annotated[NonNegative, msgspec.Meta(description="Clock frames per scene frame, still at 0"), Presentation(soft=Some((0.0, 4.0)))] = 1.0
    hold: Annotated[int, msgspec.Meta(ge=1, description="Scene frames each clock value holds")] = 1

    def held(self, frame: float) -> float:
        """Returns scene `frame` held to the first frame of its hold."""
        return self.hold * float(np.floor(frame / self.hold))

    def clock(self, frame: float) -> float:
        """Returns clock frames at held scene `frame`."""
        return self.speed * self.held(frame)


class Shutter(msgspec.Struct, frozen=True, kw_only=True):
    """Shutter length in frames and opening offset relative to frame."""

    length: float = 0.5
    opening: float = -0.5

    @classmethod
    def of(cls, scene: bpy.types.Scene) -> Self:
        """Reads shutter length and the opening offset `motion_blur_position` places in shutter lengths."""
        match scene.render.motion_blur_position:
            case "START":
                opening = 0.0
            case "CENTER":
                opening = -0.5
            case "END":
                opening = -1.0
        return cls(length=scene.render.motion_blur_shutter, opening=opening)


class ShutterPoses(msgspec.Struct, frozen=True, kw_only=True):
    """Numeric record fields sampled at both shutter endpoints into hidden pose inputs keyed `pose_<side>_<path>`, the shutter beside them."""

    fields: tuple[Field, ...]
    record: type[msgspec.Struct]

    @classmethod
    def of(cls, fields: tuple[Field, ...]) -> Self:
        """Declares the pose record of `fields` over the shutter record."""
        return cls(
            fields=fields,
            record=msgspec.defstruct(
                "Poses",
                [
                    (
                        cls.key(side, *field.path),
                        field.info.type,
                        msgspec.field(default_factory=field.info.default_factory) if field.info.default_factory is not msgspec.NODEFAULT else msgspec.field(default=field.info.default),
                    )
                    for side in range(2)
                    for field in fields
                ],
                bases=(Shutter,),
                module=__name__,
                frozen=True,
                kw_only=True,
            ),
        )

    @staticmethod
    def key(side: int, *at: Key | int | type[Case]) -> Key:
        """Pose input key of a field path at one shutter endpoint."""
        return item_key("pose", side, *at)

    def posed(self, scene: bpy.types.Scene, record: msgspec.Struct) -> msgspec.Struct:
        """Returns the scene shutter with both endpoint poses held at the record's values."""
        return msgspec.convert(
            msgspec.structs.asdict(Shutter.of(scene)) | {self.key(side, *field.path): msgspec.to_builtins(reached(record, field.path)) for side in range(2) for field in self.fields}, self.record
        )

    def keyed(self, identity: Identity, scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect) -> BlenderResult[tuple[Animation, ...]]:
        """Keys every pose component at its field channel sampled at both fractional shutter endpoints of every rendered frame."""
        frames = tuple(range(scene.frame_start, scene.frame_end + 1))
        opening = Shutter.of(scene).opening

        def components(shape: msgspec.inspect.Type) -> tuple[int | None, ...]:
            return tuple(range(len(shape.item_types))) if isinstance(shape, msgspec.inspect.TupleType) else (None,)

        def component(lengths: tuple[float, ...], field: Field, side: int, index: int | None) -> BlenderResult[Animation]:
            def samples(source: str, target: str) -> BlenderResult[Animation]:
                return animation.sampled(scene, source, (frame + (opening + side) * length for frame, length in zip(frames, lengths)), 0 if index is None else index).map(
                    lambda values: Animation(struct=target, member="value", index=index, source=Keys(points=tuple(zip(map(float, frames), values))))
                )

            return paired(path(identity, entry, item_key(*field.path)), entry_input(identity, entry, self.key(side, *field.path)).map(lambda held: held.path_from_id())).bind(
                lambda values: samples(*values)
            )

        return animation.sampled(scene, "render.motion_blur_shutter", frames).bind(
            lambda lengths: accumulated(component(lengths, field, side, index) for field in self.fields for side in range(2) for index in components(unwrapped(field.shape)))
        )


class Sighted(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Frame fractions of a point or direction seen through the scene camera, whether it lies ahead of the camera, and its camera-space depth."""

    position: Expr
    ahead: Expr
    depth: Expr

    @classmethod
    def of(cls, target: Term, direction: Scalar) -> Self:
        """Projects an object's location, or its local Z axis as a direction at the clip end where `direction` holds, through the scene camera.

        Args:
            target: Object socket operand.
            direction: Boolean operand choosing the object's axis over its location.

        Returns:
            Device positions as frame fractions, the gate of points ahead, and the depth a Depth pass occludes them at.
        """
        camera = Expr(
            kind=bpy.types.GeometryNodeObjectInfo, inputs=(("Object", Expr(kind=bpy.types.GeometryNodeInputActiveCamera, socket="Active Camera").named("camera")),), socket="Location", shape="VECTOR"
        )
        located = Expr(kind=bpy.types.GeometryNodeObjectInfo, inputs=(("Object", target),), socket="Location", shape="VECTOR")
        axis = Expr(kind=bpy.types.FunctionNodeRotateVector, inputs=(("Vector", (0.0, 0.0, 1.0)), ("Rotation", msgspec.structs.replace(located, socket="Rotation"))), socket="Vector", shape="VECTOR")
        inverse = Expr(kind=bpy.types.FunctionNodeInvertRotation, inputs=(("Rotation", msgspec.structs.replace(camera, socket="Rotation")),), socket="Rotation")
        local = Expr(kind=bpy.types.FunctionNodeRotateVector, inputs=(("Vector", switched(direction, located - camera, axis)), ("Rotation", inverse)), socket="Vector", shape="VECTOR")
        device = Expr(kind=bpy.types.FunctionNodeProjectPoint, inputs=(("Vector", local), ("Transform", lensed("Projection Matrix"))), socket="Vector", shape="VECTOR")
        return cls(position=vector("MULTIPLY_ADD", device, (0.5, 0.5, 0.0), (0.5, 0.5, 0.0)), ahead=local["Z"] < 0.0, depth=switched(direction, -local["Z"], lensed("Clip End")))


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


def entering(frame: FrameSpace, uv: Scalar | tuple[float, ...]) -> Expr:
    """Maps a Map UV coordinate of the source back to camera frame fractions with unit coverage in Z."""
    return vector("ADD", vector("DIVIDE", vector("SUBTRACT", uv, frame.margin), frame.placing), (0.0, 0.0, 1.0))


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
        noise: Stream index operand.
        pattern: Whole `clocked` frames for origins redrawn per pattern, else 0.
        instance: Repeated draw index, else 0.

    Returns:
        3D origin `Vector` expression.
    """
    hashed = Expr(kind=bpy.types.ShaderNodeTexWhiteNoise, inputs=(("Vector", xyz(seed, noise, pattern)), ("W", instance)), members=(("noise_dimensions", "4D"),), socket="Color", shape="RGBA")
    return vector("MULTIPLY", hashed, (256.0, 256.0, 256.0))


def trailed(image: Expr, source: Scalar, opened: Expr, stepped: Expr) -> Expr:
    """Smears an associated image along the motion between shutter endpoints through Directional Blur, normalized by the trail's own coverage.

    Args:
        image: Associated image at the shutter opening.
        source: Image whose data window the trail covers.
        opened: Map UV of the frame center at the shutter close.
        stepped: Map UV of the point one pixel right of the center at the shutter close.

    Returns:
        Covered associated trail.
    """
    dimensions = Expr(kind=bpy.types.CompositorNodeImageInfo, inputs=(("Image", source),), socket="Dimensions", shape="VECTOR")
    translation = vector("MULTIPLY", vector("SUBTRACT", opened, (0.5, 0.5, 1.0)), dimensions)
    factor = vector("MULTIPLY", vector("SUBTRACT", stepped, opened), dimensions)
    scale, rotation, length, diagonal = vector("LENGTH", factor), math("ARCTAN2", factor["Y"], factor["X"]), vector("LENGTH", translation), vector("LENGTH", dimensions)
    settings = (
        ("Scale", scale),
        ("Rotation", rotation),
        ("Translation Amount", length / (scale * diagonal)),
        ("Translation Direction", math("ARCTAN2", translation["Y"], -translation["X"])),
        ("Samples", math("CEIL", math("LOGARITHM", length + (abs(scale - 1.0) + abs(rotation)) * diagonal, 2.0))),
    )

    def blurred(held: Expr) -> Expr:
        return Expr(kind=bpy.types.CompositorNodeDBlur, inputs=(("Image", held), ("Center", (0.5, 0.5)), *settings), socket="Image", shape="RGBA")

    unit = vector("ADD", vector("SCALE", Expr(kind=bpy.types.CompositorNodeImageCoordinates, inputs=(("Image", source),), socket="Pixel", shape="VECTOR"), 0.0), (1.0, 1.0, 1.0))
    trail, coverage = blurred(image), blurred(Expr(kind=bpy.types.CompositorNodeSetAlpha, inputs=(("Image", unit), ("Alpha", 1.0), ("Type", "Replace Alpha")), socket="Image", shape="RGBA"))
    return Expr(
        kind=bpy.types.CompositorNodeSetAlpha,
        inputs=(("Image", vector("DIVIDE", trail, coverage)), ("Alpha", trail["Alpha"] / coverage["Alpha"]), ("Type", "Replace Alpha")),
        socket="Image",
        shape="RGBA",
    )


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
    scale_x, scale_y, skew_x, skew_y = (msgspec.structs.replace(Expr(kind=bpy.types.ShaderNodeSeparateXYZ, inputs=(("Vector", term),)), socket=axis) for term in (scale, skew) for axis in ("X", "Y"))
    horizontal, vertical = (
        Expr(kind=bpy.types.FunctionNodeCombineMatrix, inputs=((cell, math("TANGENT", angle)),), socket="Matrix") for cell, angle in (("Column 2 Row 1", skew_x), ("Column 1 Row 2", skew_y))
    )
    turn = _transform("Rotation", Expr(kind=bpy.types.FunctionNodeEulerToRotation, inputs=(("Euler", xyz(z=rotation)),), socket="Rotation"))
    linear = composed(turn, composed(vertical, composed(horizontal, _transform("Scale", xyz(scale_x, scale_y, 1.0)))))
    moved = Expr(kind=bpy.types.FunctionNodeTransformDirection, inputs=(("Direction", anchor), ("Transform", linear)), socket="Direction", shape="VECTOR")
    return composed(_transform("Translation", vector("SUBTRACT", vector("ADD", vector("MULTIPLY", position, frame.extent), anchor), moved)), linear)


def placed(source: Scalar, frame: FrameSpace, matrix: Expr) -> Expr:
    """Places current displayed rectangle through rotate, scale, rotate, and translate nodes of the matrix's singular value decomposition, keeping deferred bounds.

    Args:
        source: Associated content with its transformation.
        frame: Current displayed extent and conversion to stored pixel distances.
        matrix: Forward matrix from `placement` in same frame.

    Returns:
        Content transformed about its center, transparent when singular.
    """
    basis = _transform("Scale", xyz(frame.scale["X"], frame.scale["Y"], 1.0))
    pixel = composed(basis, composed(matrix, inverted(basis)))
    planar = Expr(
        kind=bpy.types.FunctionNodeCombineMatrix,
        inputs=(*((f"Column {column} Row {row}", _cell(pixel, column, row)) for column in range(1, 3) for row in range(1, 3)), ("Column 3 Row 3", 0.0)),
        socket="Matrix",
    )
    decomposed = Expr(kind=bpy.types.FunctionNodeMatrixSVD, inputs=(("Matrix", planar),), socket="S", shape="VECTOR")
    reflection = math("SIGN", Expr(kind=bpy.types.FunctionNodeMatrixDeterminant, inputs=(("Matrix", pixel),), socket="Determinant"))
    center = vector("SCALE", frame.extent, 0.5)
    moved = Expr(kind=bpy.types.FunctionNodeTransformPoint, inputs=(("Vector", center), ("Transform", matrix)), socket="Vector", shape="VECTOR")
    location = Expr(kind=bpy.types.CompositorNodeImageInfo, inputs=(("Image", source),), socket="Location", shape="VECTOR")

    def rotated(image: Expr, side: Literal["U", "V"], direction: float) -> Expr:
        factor = msgspec.structs.replace(decomposed, socket=side, shape="FLOAT")
        return Expr(kind=bpy.types.CompositorNodeRotate, inputs=(("Image", image), ("Angle", math("ARCTAN2", _cell(factor, 1, 2), _cell(factor, 1, 1)) * direction)), socket="Image", shape="RGBA")

    def translated(image: Scalar, offset: Expr, *extras: tuple[str, str]) -> Expr:
        return Expr(kind=bpy.types.CompositorNodeTranslate, inputs=(("Image", image), ("X", offset["X"]), ("Y", offset["Y"]), *extras), socket="Image", shape="RGBA")

    unrotated = rotated(translated(source, vector("SCALE", location, -1.0)), "V", -1.0)
    stretched = Expr(kind=bpy.types.CompositorNodeScale, inputs=(("Image", unrotated), ("Type", "Relative"), ("X", decomposed["X"]), ("Y", decomposed["Y"] * reflection)), socket="Image", shape="RGBA")
    shifted = vector("ADD", location, vector("MULTIPLY", vector("SUBTRACT", moved, center), frame.scale))
    image = translated(rotated(stretched, "U", 1.0), shifted, ("Interpolation", "Bilinear"), ("Extension X", "Clip"), ("Extension Y", "Clip"))
    return switched(msgspec.structs.replace(inverted(pixel), socket="Invertible"), (0.0, 0.0, 0.0, 0.0), image)


def pinned(fraction: Expr, rows: tuple[Expr, Expr, Expr], *, forward: bool) -> Expr:
    """Maps camera frame fractions through a corner pin homography, coverage in Z kept where the position faces the plane.

    Args:
        fraction: Camera frame fractions with geometric coverage in Z.
        rows: Homography rows taking the unit plane onto the image quad, the `Projective` inputs.
        forward: Plane to image in place of image to plane sampling.

    Returns:
        Mapped fractions with front-facing coverage in Z.
    """
    homography = Expr(
        kind=bpy.types.FunctionNodeCombineMatrix,
        inputs=tuple((f"Column {column} Row {row}", held[axis]) for row, held in zip((1, 2, 4), rows) for column, axis in zip((1, 2, 4), AXES)),
        socket="Matrix",
    )
    mapping = homography if forward else inverted(homography)
    planar = vector("MULTIPLY", fraction, (1.0, 1.0, 0.0))
    front = vector("DOT_PRODUCT", xyz(_cell(mapping, 1, 4), _cell(mapping, 2, 4), _cell(mapping, 4, 4)), vector("ADD", planar, (0.0, 0.0, 1.0))) > 0.0
    position = where(front, _projected(planar, mapping), (0.0, 0.0, 0.0))
    return vector("MULTIPLY_ADD", (0.0, 0.0, 1.0), fraction["Z"] * front, position)


def tilted(uv: Expr, lens: Expr, heading: Scalar, elevation: Scalar, *, forward: bool = False) -> Expr:
    """Maps positions through camera lens and YXZ tilt.

    Args:
        uv: Positions in normalized frame of whole image, coverage in Z.
        lens: Camera projection matrix of image.
        heading: Rightward turn of view.
        elevation: Upward tilt of view.
        forward: Forward content placement in place of inverse sampling.

    Returns:
        Source positions and front-facing geometric coverage in Z.
    """
    (first, first_axis), (second, second_axis) = ((elevation, "X_AXIS"), (heading, "Y_AXIS")) if forward else ((heading, "Y_AXIS"), (elevation, "X_AXIS"))

    def rotated(direction: Expr, angle: Scalar, axis: str) -> Expr:
        return Expr(kind=bpy.types.ShaderNodeVectorRotate, inputs=(("Vector", direction), ("Angle", angle)), members=(("rotation_type", axis),), socket="Vector", shape="VECTOR")

    ray = _projected(vector("MULTIPLY_ADD", uv, (2.0, 2.0, 0.0), (-1.0, -1.0, 1.0)), inverted(lens))
    turned = rotated(rotated(ray, first, first_axis), math("MULTIPLY", second, -1.0), second_axis)
    position = vector("MULTIPLY_ADD", _projected(turned, lens), (0.5, 0.5, 0.0), (0.5, 0.5, 0.0))
    return vector("MULTIPLY_ADD", (0.0, 0.0, 1.0), (turned["Z"] < 0.0) * uv["Z"], position)


def reaimed(fraction: Expr, lens: Expr, heading: Expr, elevation: Expr, share: Scalar, *, forward: bool) -> Expr:
    """Re-aims the camera of an overscanned frame, camera frame fractions in and out, identity at zero turn.

    Args:
        fraction: Camera frame fractions with geometric coverage in Z.
        lens: Camera projection matrix.
        heading: Rightward turn of view.
        elevation: Upward tilt of view.
        share: Camera frame share of each overscanned side.
        forward: Forward content placement in place of inverse sampling.

    Returns:
        Re-aimed fractions with front-facing coverage in Z.
    """
    margin = math("MULTIPLY_ADD", share, -0.5, 0.5)
    device = vector("ADD", vector("MULTIPLY", vector("MULTIPLY_ADD", fraction, share, margin), (1.0, 1.0, 0.0)), (0.0, 0.0, 1.0))
    projected = tilted(device, lens, heading, elevation, forward=forward)
    position = vector("MULTIPLY", vector("DIVIDE", vector("SUBTRACT", projected, margin), share), (1.0, 1.0, 0.0))
    return switched((abs(heading) + abs(elevation)) > 0.0, fraction, vector("MULTIPLY_ADD", (0.0, 0.0, 1.0), fraction["Z"] * projected["Z"], position))


# --- [SAMPLING]


def extended(source: Scalar, uv: Expr, borders: tuple[Scalar | str, Scalar | str], *, clipped: bool) -> Expr:
    """Samples source at Map UV positions under each axis' `Border` menu, Map UV premultiplying the geometric coverage in Z.

    Args:
        source: Associated image being sampled.
        uv: Map UV positions with geometric coverage in Z.
        borders: `Border` menu input or item label for X, then for Y.
        clipped: Whether black borders fade over the edge pixel as coverage, else Map UV clips them.

    Returns:
        Covered associated sample.
    """
    dimensions = Expr(kind=bpy.types.CompositorNodeImageInfo, inputs=(("Image", source),), socket="Dimensions", shape="VECTOR")
    items = MenuItems.of(Border)
    native = {Border.BLACK: "Extend" if clipped else "Clip", Border.CLAMP: "Extend", Border.PERIODIC: "Repeat", Border.MIRROR: "Extend"}

    def axis(name: Literal["X", "Y"], selector: Scalar | str) -> tuple[Expr, Expr, Expr]:
        mode = menu(selector, items, {member.label: native[member] for member in Border}, kind="MENU")
        mirrored, black = (msgspec.structs.replace(mode, socket=member.label, shape="FLOAT") for member in (Border.MIRROR, Border.BLACK))
        coordinate = uv[name]
        edge = math("MULTIPLY_ADD", math("MINIMUM", coordinate, 1.0 - coordinate), dimensions[name], 0.5, clamp=True)
        return switched(mirrored, coordinate, math("PINGPONG", coordinate, 1.0)), switched(black, 1.0, edge), mode

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


def _transform(component: Literal["Translation", "Rotation", "Scale"], value: Expr) -> Expr:
    """Builds the transform matrix of one component."""
    return Expr(kind=bpy.types.FunctionNodeCombineTransform, inputs=((component, value),), socket="Transform")


def _cell(matrix: Expr, column: int, row: int) -> Expr:
    """Reads one matrix entry."""
    return Expr(kind=bpy.types.FunctionNodeSeparateMatrix, inputs=(("Matrix", matrix),), socket=f"Column {column} Row {row}")


def _projected(point: Expr, transform: Expr) -> Expr:
    """Projects a point through a matrix with perspective division."""
    return Expr(kind=bpy.types.FunctionNodeProjectPoint, inputs=(("Vector", point), ("Transform", transform)), socket="Vector", shape="VECTOR")


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "Border",
    "Clock",
    "FrameSpace",
    "PixelGrid",
    "Projective",
    "Shutter",
    "ShutterPoses",
    "Sighted",
    "clocked",
    "covered",
    "entering",
    "extended",
    "framed",
    "gridded",
    "lensed",
    "origin",
    "pinned",
    "pixels",
    "placed",
    "placement",
    "reaimed",
    "sampled",
    "shorter",
    "tilted",
    "trailed",
]
