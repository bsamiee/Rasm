"""Affine maps, camera re-aiming, and covered warp reconstruction."""

import functools
from itertools import chain
from typing import Literal

import bpy
from expression import Error, Ok, Option, Some
import msgspec
import msgspec.inspect as native

from blender.bpy.data.rna.animation import Animation, Keys, sampled as sampled_channel
from blender.bpy.data.rna.conditions import AllOf, Condition
from blender.bpy.extension.identity import Identity
from blender.bpy.faults import BlenderChoiceFault, BlenderResult, BlenderUnmetFault
from blender.bpy.nodes.graphs import Component, End, Expr, math, Scalar, switched, vector, where, xyz
from blender.bpy.nodes.interface import item_key
from blender.bpy.render.effects.coordinates import clocked, covered, extended, FrameSpace, lensed, sampled
from blender.bpy.render.effects.generators.noise import sample
from blender.bpy.render.effects.records import Field, KeyPath, path, reached
from blender.bpy.render.effects.rows import Alpha, CameraFrame, CompositorEffect, Context, Derived, Inspected, precise, Shell, Shutter, shutter, shuttered
from blender.bpy.scene.cameras import camera_type
from blender.bpy.settings.shapes import unwrapped
from blender.imaging.compositing.layers import Affine
from blender.imaging.effects.generators.noise import Cut, Interpolation, Octaves, Perlin
from blender.imaging.effects.warp.transforms import Shake, Tilt, Transform
from blender.imaging.faults import accumulated, paired
from blender.imaging.geometry.homography import Homography, Quad
from blender.imaging.parameters import Gated, GraphForm, markers, Motion

# --- [MODELS] ---------------------------------------------------------------------------


class ConvexQuad(msgspec.Struct, frozen=True, kw_only=True):
    """Corners forming no strictly convex quadrilateral."""

    quad: Quad

    @property
    def message(self) -> str:
        """Requirement text with requested corners."""
        return f"Corner Pin {self.quad} is not strictly convex"


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [CHECKS]


def validated(identity: Identity, scene: bpy.types.Scene, entry: Option[bpy.types.SceneCompositorEffect], record: Transform, _context: Context) -> BlenderResult[None]:
    """Checks the native reconstruction filter, then Corner Pin convexity at rendered frames and shutter endpoints.

    Args:
        identity: Owner of existing parameter channels.
        scene: Scene supplying render range and shutter.
        entry: Entry holding keyed inputs, absent for new entry.
        record: Proposed values for unkeyed inputs.
        _context: Stage input facts, unused by normalized corners.

    Returns:
        None for the default reconstruction filter over convex pinned quadrilaterals, else the choice fault, channel faults, or unmet convexity faults.
    """
    frames = tuple(dict.fromkeys((scene.frame_current_final, *(float(frame) for frame in range(scene.frame_start, scene.frame_end + 1, scene.frame_step)))))
    pin, _, _, _, _, _, _, _, filtername, _, motion_blur = Transform.__struct_fields__
    _, value = Gated.__struct_fields__
    fields = msgspec.structs.fields(Quad)
    stock = Transform()
    reconstruction: BlenderResult[None] = (
        Ok(None)
        if (record.filtername, record.filterwidth) == (stock.filtername, stock.filterwidth)
        else Error((BlenderChoiceFault(path=filtername, identifier=f"{record.filtername} ({record.filterwidth})", accepted=(f"{stock.filtername} ({stock.filterwidth})",)),))
    )

    def samples(key: str, proposed: float, times: tuple[float, ...], index: int = 0) -> BlenderResult[tuple[float, ...]]:
        return entry.map(lambda held: path(identity, held, key).bind(lambda path: sampled_channel(scene, path, times, index, Some(proposed)))).default_with(lambda: Ok(tuple(proposed for _ in times)))

    def accepted(times: tuple[float, ...]) -> BlenderResult[None]:
        def quads(enabled: tuple[float, ...], corners: tuple[tuple[tuple[float, ...], ...], ...]) -> BlenderResult[None]:
            return accumulated(
                Ok(None) if Homography.of(quad).is_some() else Error((BlenderUnmetFault(requirement=ConvexQuad(quad=quad)),))
                for frame, active in enumerate(enabled)
                if active
                for quad in (Quad(**{field.name: (corner[0][frame], corner[1][frame]) for field, corner in zip(fields, corners)}),)
            ).map(lambda _: None)

        return paired(
            samples(pin, float(record.pin.enabled), times),
            accumulated(accumulated(samples(item_key(pin, value, field.name), component, times, index) for index, component in enumerate(getattr(record.pin.value, field.name))) for field in fields),
        ).bind(lambda values: quads(*values))

    def endpoints(lengths: tuple[float, ...], enabled: tuple[float, ...]) -> BlenderResult[None]:
        opening = shuttered(scene).opening
        times = tuple(dict.fromkeys((*frames, *(frame + (opening + side) * length for frame, length, active in zip(frames, lengths, enabled) if active for side in range(2)))))
        return accepted(times)

    convexity = paired(sampled_channel(scene, "render.motion_blur_shutter", frames), samples(motion_blur, float(record.motion_blur), frames)).bind(lambda values: endpoints(*values))
    return paired(reconstruction, convexity).map(lambda _: None)


def aimed(record: Transform) -> Condition:
    """Requires perspective camera when tilt turns view."""
    return camera_type("PERSP") if record.tilt.heading != 0.0 or record.tilt.elevation != 0.0 else AllOf(conditions=())


# --- [POSES]


def pose_inputs() -> tuple[Field, ...]:
    """Derives numeric temporal leaves of Transform record."""

    def temporal(field: msgspec.structs.FieldInfo, shape: native.Type) -> bool:
        _, _, _, _, _, _, _, _, _, filterwidth, _ = Transform.__struct_fields__
        match unwrapped(shape):
            case native.FloatType() | native.IntType():
                return not markers(field.type, GraphForm) and field.name != filterwidth
            case native.TupleType(item_types=items):
                return all(temporal(field, item) for item in items)
            case _:
                return False

    return tuple(field for field in Field.of(Transform) if temporal(field.info, field.shape))


def posed(_identity: Identity, scene: bpy.types.Scene, record: Transform) -> BlenderResult[msgspec.Struct]:
    """Builds both shutter endpoint poses from record and scene shutter."""
    return Ok(Poses(**msgspec.structs.asdict(shuttered(scene)), **{item_key("pose", side, *field.path): reached(record, field.path) for side in range(2) for field in pose_inputs()}))


def keyed(identity: Identity, scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect, _record: Transform) -> BlenderResult[tuple[Animation, ...]]:
    """Samples every temporal component at both fractional shutter endpoints."""
    frames = tuple(range(scene.frame_start, scene.frame_end + 1))
    opening = shuttered(scene).opening

    def channels(lengths: tuple[float, ...], field: Field) -> BlenderResult[tuple[Animation, ...]]:
        at = field.path
        indices = tuple(range(len(kind.item_types))) if isinstance(kind := unwrapped(field.shape), native.TupleType) else (None,)

        def component(side: int, index: int | None) -> BlenderResult[Animation]:
            def samples(source: str, target: str) -> BlenderResult[Animation]:
                struct, _, member = target.rpartition(".")
                return sampled_channel(scene, source, (frame + (opening + side) * length for frame, length in zip(frames, lengths)), 0 if index is None else index).map(
                    lambda values: Animation(struct=struct, member=member, index=index, source=Keys(points=tuple(zip(map(float, frames), values))))
                )

            return paired(path(identity, entry, item_key(*at)), path(identity, entry, item_key("pose", side, *at))).bind(lambda values: samples(*values))

        return accumulated(component(side, index) for side in range(2) for index in indices)

    return (
        sampled_channel(scene, "render.motion_blur_shutter", frames)
        .bind(lambda lengths: accumulated(channels(lengths, field) for field in pose_inputs()))
        .map(lambda groups: tuple(chain.from_iterable(groups)))
    )


# --- [GRAPHS]


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
    linear = _product(turn, _product(vertical, _product(horizontal, _transform("Scale", xyz(scale_x, scale_y, 1.0)))))
    moved = Expr(kind=bpy.types.FunctionNodeTransformDirection, inputs=(("Direction", anchor), ("Transform", linear)), socket="Direction", shape="VECTOR")
    return _product(_transform("Translation", vector("SUBTRACT", vector("ADD", vector("MULTIPLY", position, frame.extent), anchor), moved)), linear)


def placed(source: Scalar, frame: FrameSpace, matrix: Expr) -> Expr:
    """Places current displayed rectangle through rotate, scale, rotate, and translate nodes, keeping deferred bounds.

    Args:
        source: Associated content with its transformation.
        frame: Current displayed extent and conversion to stored pixel distances.
        matrix: Forward matrix from `placement` in same frame.

    Returns:
        Content transformed about its center, transparent when singular.
    """
    basis = _transform("Scale", xyz(frame.scale["X"], frame.scale["Y"], 1.0))
    pixel = _product(basis, _product(matrix, _inverse(basis)))
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
    return switched(msgspec.structs.replace(_inverse(pixel), socket="Invertible"), (0.0, 0.0, 0.0, 0.0), image)


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

    ray = _projected(vector("MULTIPLY_ADD", uv, (2.0, 2.0, 0.0), (-1.0, -1.0, 1.0)), _inverse(lens))
    turned = rotated(rotated(ray, first, first_axis), math("MULTIPLY", second, -1.0), second_axis)
    position = vector("MULTIPLY_ADD", _projected(turned, lens), (0.5, 0.5, 0.0), (0.5, 0.5, 0.0))
    return vector("MULTIPLY_ADD", (0.0, 0.0, 1.0), (turned["Z"] < 0.0) * uv["Z"], position)


def transformed(shell: Shell[Transform]) -> BlenderResult[Expr]:
    """Composes sampled camera poses into the covered warp and its shutter approximation.

    Each pose maps camera frame fractions carrying geometric coverage in Z, inverse for sampling and forward for the blur endpoints.

    Args:
        shell: Record inputs, both pose endpoints, shutter, camera frame, and the extension menu.

    Returns:
        Warped associated image, the stage input where every map is stationary identity.
    """
    pin, tilt, affine, travel, turn, zoom, shake, extension, _, _, motion_blur = Transform.__struct_fields__
    _, value_field = Gated.__struct_fields__
    heading, elevation = Tilt.__struct_fields__
    length_field, opening_field = Shutter.__struct_fields__
    position_field, rotation_field, scale_field, motion, octaves_field, seed = Shake.__struct_fields__
    speed, hold = Motion.__struct_fields__
    _, _, _, _, octaves, _, _, _, _ = Perlin.__struct_fields__
    _, share_field, _ = CameraFrame.__struct_fields__
    *_, pivot = Affine.__struct_fields__
    basis = Perlin(dimensions=1, octaves=Octaves(detail=4.0), cut=Cut(threshold=0.17, softness=0.83, interpolation=Interpolation.LINEAR))
    frame, lens, share = shell.space, lensed("Projection Matrix"), shell.input(Shell.frame, share_field)
    margin = math("MULTIPLY_ADD", share, -0.5, 0.5)
    axes: tuple[Component, Component] = ("X", "Y")

    def value(side: int | None, *at: str) -> Expr:
        held = shell.input(*at)
        return held if side is None else msgspec.structs.replace(held, socket=item_key("pose", side, *at))

    def pinned(side: int | None, fraction: Expr, *, forward: bool) -> Expr:
        upper_left, upper_right, lower_right, lower_left = (value(side, pin, value_field, corner) for corner in Quad.__struct_fields__)
        horizontal, vertical = vector("SUBTRACT", lower_right, upper_right), vector("SUBTRACT", upper_left, upper_right)
        diagonal = vector("SUBTRACT", vector("ADD", vector("SUBTRACT", lower_left, lower_right), upper_right), upper_left)

        def crossed(first: Expr, second: Expr) -> Expr:
            return vector("DOT_PRODUCT", vector("CROSS_PRODUCT", first, second), (0.0, 0.0, 1.0))

        g, h = crossed(diagonal, vertical) / crossed(horizontal, vertical), crossed(horizontal, diagonal) / crossed(horizontal, vertical)
        columns = (
            (1, vector("MULTIPLY_ADD", lower_right, g, vector("SUBTRACT", lower_right, lower_left))),
            (2, vector("MULTIPLY_ADD", upper_left, h, vector("SUBTRACT", upper_left, lower_left))),
            (4, lower_left),
        )
        matrix = Expr(
            kind=bpy.types.FunctionNodeCombineMatrix,
            inputs=(*((f"Column {column} Row {row}", held[axis]) for column, held in columns for row, axis in enumerate(axes, 1)), ("Column 1 Row 4", g), ("Column 2 Row 4", h)),
            socket="Matrix",
        )
        mapping = matrix if forward else _inverse(matrix)
        planar = vector("MULTIPLY", fraction, (1.0, 1.0, 0.0))
        front = vector("DOT_PRODUCT", xyz(_cell(mapping, 1, 4), _cell(mapping, 2, 4), _cell(mapping, 4, 4)), vector("ADD", planar, (0.0, 0.0, 1.0))) > 0.0
        position = where(front, _projected(planar, mapping), (0.0, 0.0, 0.0))
        return switched(shell.gate(pin), fraction, vector("MULTIPLY_ADD", (0.0, 0.0, 1.0), fraction["Z"] * front, position))

    def reaimed(side: int | None, fraction: Expr, *, forward: bool) -> Expr:
        turned, raised = value(side, tilt, heading), value(side, tilt, elevation)
        device = vector("ADD", vector("MULTIPLY", vector("MULTIPLY_ADD", fraction, share, margin), (1.0, 1.0, 0.0)), (0.0, 0.0, 1.0))
        projected = tilted(device, lens, turned, raised, forward=forward)
        position = vector("MULTIPLY", vector("DIVIDE", vector("SUBTRACT", projected, margin), share), (1.0, 1.0, 0.0))
        return switched((abs(turned) + abs(raised)) > 0.0, fraction, vector("MULTIPLY_ADD", (0.0, 0.0, 1.0), fraction["Z"] * projected["Z"], position))

    def pose(side: int | None) -> Expr:
        def shaken(*at: str) -> Expr:
            return value(side, shake, value_field, *at)

        offset: Scalar = 0.0 if side is None else shell.input(opening_field) + float(side)
        time = math("MULTIPLY_ADD", math("MULTIPLY", offset, shell.input(length_field)), shaken(motion, speed), clocked(shaken(motion, speed), shaken(motion, hold)))
        detail: dict[KeyPath, Scalar] = {(octaves, field.name): shaken(octaves_field, field.name) for field in msgspec.structs.fields(Octaves)}
        x, y, angle, stops = (math("MULTIPLY_ADD", sample(basis, xyz(time), 0.0, shaken(seed), stream, detail), 2.0, -1.0) for stream in range(4))
        gate = shell.gate(shake)
        jitter = placement(
            frame,
            switched(gate, (0.0, 0.0, 0.0), vector("DIVIDE", vector("MULTIPLY", xyz(x, y), shaken(position_field)), frame.extent)),
            switched(gate, 0.0, angle * shaken(rotation_field)),
            switched(gate, 1.0, 2.0 ** (stops * shaken(scale_field))),
            (0.0, 0.0, 0.0),
            (0.5, 0.5, 0.0),
        )
        drift = placement(frame, vector("SCALE", value(side, travel), offset), value(side, turn) * offset, 2.0 ** (value(side, zoom) * offset), (0.0, 0.0, 0.0), value(side, affine, pivot))
        return _product(drift, _product(jitter, placement(frame, **{field.name: value(side, affine, field.name) for field in msgspec.structs.fields(Affine)})))

    def mapped(side: int | None, fraction: Expr, *, forward: bool = False) -> Expr:
        matrix = pose(side)
        lifted = reaimed(side, pinned(side, fraction, forward=True), forward=True) if forward else fraction
        moved = Expr(
            kind=bpy.types.FunctionNodeTransformPoint,
            inputs=(("Vector", vector("MULTIPLY", lifted, frame.extent)), ("Transform", matrix if forward else _inverse(matrix))),
            socket="Vector",
            shape="VECTOR",
        )
        landed = vector("MULTIPLY_ADD", (0.0, 0.0, 1.0), lifted["Z"], vector("DIVIDE", moved, frame.extent))
        projected = landed if forward else pinned(side, reaimed(side, landed, forward=False), forward=False)
        return vector("MULTIPLY_ADD", (0.0, 0.0, 1.0), projected["Z"] * msgspec.structs.replace(_inverse(matrix), socket="Invertible"), vector("MULTIPLY", projected, (1.0, 1.0, 0.0)))

    def entered(uv: Scalar | tuple[float, ...]) -> Expr:
        return vector("ADD", vector("DIVIDE", vector("SUBTRACT", uv, frame.margin), frame.placing), (0.0, 0.0, 1.0))

    start = vector("ADD", frame.fraction, (0.0, 0.0, 1.0))
    border = (shell.input(extension), shell.items[extension,])
    image = extended(shell.color, covered(frame, switched(shell.gate(motion_blur), mapped(None, start), mapped(0, start))).named("uv"), (border, border), clipped=True)
    dimensions = Expr(kind=bpy.types.CompositorNodeImageInfo, inputs=(("Image", shell.color),), socket="Dimensions", shape="VECTOR")
    center = (0.5, 0.5, 1.0)
    opened, stepped = (sampled(frame, mapped(1, mapped(0, entered(at)), forward=True)) for at in (center, vector("ADD", center, vector("DIVIDE", (1.0, 0.0, 0.0), dimensions))))
    translation = vector("MULTIPLY", vector("SUBTRACT", opened, center), dimensions)
    factor = vector("MULTIPLY", vector("SUBTRACT", stepped, opened), dimensions)
    scale, rotation, length, diagonal = vector("LENGTH", factor), math("ARCTAN2", factor["Y"], factor["X"]), vector("LENGTH", translation), vector("LENGTH", dimensions)
    settings = (
        ("Scale", scale),
        ("Rotation", rotation),
        ("Translation Amount", length / (scale * diagonal)),
        ("Translation Direction", math("ARCTAN2", translation["Y"], -translation["X"])),
        ("Samples", math("CEIL", math("LOGARITHM", length + (abs(scale - 1.0) + abs(rotation)) * diagonal, 2.0))),
    )

    def trailed(source: Expr) -> Expr:
        return Expr(kind=bpy.types.CompositorNodeDBlur, inputs=(("Image", source), ("Center", (0.5, 0.5)), *settings), socket="Image", shape="RGBA")

    unit = vector("ADD", vector("SCALE", Expr(kind=bpy.types.CompositorNodeImageCoordinates, inputs=(("Image", shell.color),), socket="Pixel", shape="VECTOR"), 0.0), (1.0, 1.0, 1.0))
    trail, coverage = trailed(image), trailed(Expr(kind=bpy.types.CompositorNodeSetAlpha, inputs=(("Image", unit), ("Alpha", 1.0), ("Type", "Replace Alpha")), socket="Image", shape="RGBA"))
    blurred = Expr(
        kind=bpy.types.CompositorNodeSetAlpha,
        inputs=(("Image", vector("DIVIDE", trail, coverage)), ("Alpha", trail["Alpha"] / coverage["Alpha"]), ("Type", "Replace Alpha")),
        socket="Image",
        shape="RGBA",
    )
    reference, defaults = pose(None), Quad()
    matches = (
        *(math("COMPARE", _cell(reference, column, row), float(column == row), 0.0) for column in (1, 2, 4) for row in range(1, 3)),
        switched(
            shell.gate(pin),
            1.0,
            functools.reduce(
                lambda held, match: held * match,
                (
                    math("COMPARE", value(None, pin, value_field, field.name)[axis], default, 0.0)
                    for field in msgspec.structs.fields(Quad)
                    for axis, default in zip(axes, getattr(defaults, field.name))
                ),
            ),
        ),
        math("COMPARE", abs(value(None, tilt, heading)) + abs(value(None, tilt, elevation)), 0.0, 0.0),
    )
    stationary = switched(shell.gate(motion_blur), functools.reduce(lambda held, match: held * match, matches), 0.0)
    return Ok(switched(stationary, switched(shell.gate(motion_blur), image, blurred), shell.color))


def _product(left: Expr, right: Expr) -> Expr:
    """Builds the matrix product applying `right` first."""
    return Expr(kind=bpy.types.FunctionNodeMatrixMultiply, inputs=(("Matrix", left), ("Matrix_001", right)), socket="Matrix")


def _inverse(matrix: Expr) -> Expr:
    """Builds the inverse matrix, its `Invertible` output beside."""
    return Expr(kind=bpy.types.FunctionNodeInvertMatrix, inputs=(("Matrix", matrix),), socket="Matrix")


def _transform(component: Literal["Translation", "Rotation", "Scale"], value: Expr) -> Expr:
    """Builds the transform matrix of one component."""
    return Expr(kind=bpy.types.FunctionNodeCombineTransform, inputs=((component, value),), socket="Transform")


def _cell(matrix: Expr, column: int, row: int) -> Expr:
    """Reads one matrix entry."""
    return Expr(kind=bpy.types.FunctionNodeSeparateMatrix, inputs=(("Matrix", matrix),), socket=f"Column {column} Row {row}")


def _projected(point: Expr, transform: Expr) -> Expr:
    """Projects a point through a matrix with perspective division."""
    return Expr(kind=bpy.types.FunctionNodeProjectPoint, inputs=(("Vector", point), ("Transform", transform)), socket="Vector", shape="VECTOR")


# --- [COMPOSITION] ----------------------------------------------------------------------

Poses = msgspec.defstruct(
    "Poses",
    [
        (
            item_key("pose", side, *field.path),
            field.info.type,
            msgspec.field(default_factory=field.info.default_factory) if field.info.default_factory is not msgspec.NODEFAULT else msgspec.field(default=field.info.default),
        )
        for side in range(2)
        for field in pose_inputs()
    ],
    bases=(Shutter,),
    module=__name__,
    frozen=True,
    kw_only=True,
)
transform = CompositorEffect(
    record=Transform,
    graph=transformed,
    alpha=Alpha.RESULT,
    requested=Some(precise),
    condition=Some(aimed),
    derived=Some(Derived(record=Some(Poses), of=posed, reads=shutter.reads)),
    animated=Some(keyed),
    native_validated=Some(validated),
    inspected=(Inspected(key="transform_uv", end=End(node="uv", socket="Vector"), description="Source positions and coverage"),),
)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["ConvexQuad", "Poses", "aimed", "keyed", "placed", "placement", "pose_inputs", "posed", "tilted", "transform", "transformed", "validated"]
