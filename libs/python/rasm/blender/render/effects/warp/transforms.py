# mypy: disable-error-code="attr-defined"
# ty: ignore[unresolved-attribute]
"""Native affine maps, camera re-aiming, and covered warp reconstruction."""

import bpy
from expression import Error, Nothing, Ok, Option, Some
import msgspec
import msgspec.inspect as native

from rasm.blender.data.rna.animation import Animation, Keys, sampled as sampled_channel
from rasm.blender.data.rna.conditions import AllOf, Condition
from rasm.blender.extension.identity import Identity, Key
from rasm.blender.faults import BlenderChoiceFault, BlenderResult, BlenderUnmetFault
from rasm.blender.nodes.graphs import Clock, compared, End, ended, Fragment, framed, FrameSpace, item_key, masked, math, menu, MenuItems, node, Operand, sampled, switched, vector
from rasm.blender.render.effects.compositing.blending import product
from rasm.blender.render.effects.generators.noise import sample
from rasm.blender.render.effects.stack import CompositorEffect, Context, Coverage, Derived, KeyPath, path, precise, reached, rows, Shell, Shutter, shutter, ShutterPosition
from rasm.blender.render.effects.viewers import inspection
from rasm.blender.scene.cameras import camera_type
from rasm.imaging.compositing.layers import Affine
from rasm.imaging.effects.generators.noise import Octaves, Perlin
from rasm.imaging.effects.warp.transforms import Shake, Tilt, Transform
from rasm.imaging.faults import accumulated, paired
from rasm.imaging.geometry.homography import Homography, Quad
from rasm.imaging.geometry.stmaps import Extension
from rasm.imaging.parameters import Gated, GraphForm, markers, Motion

# --- [MODELS] ---------------------------------------------------------------------------


class ConvexQuad(msgspec.Struct, frozen=True, kw_only=True):
    """Requested corner positions without a strictly convex image plane."""

    quad: Quad

    @property
    def message(self) -> str:
        """Requested geometry that requires a convex plane."""
        return f"Corner Pin requires a strictly convex quadrilateral: {self.quad}"


# --- [OPERATIONS] -----------------------------------------------------------------------


def admitted(identity: Identity, scene: bpy.types.Scene, entry: Option[bpy.types.SceneCompositorEffect], record: Transform, _context: Context) -> BlenderResult[None]:
    """Accepts the requested corner geometry at rendered frames and shutter endpoints.

    Args:
        identity: Owner of existing native parameter channels.
        scene: Render range and native shutter.
        entry: Existing keyed inputs, or absence for a new entry.
        record: Proposed transform values used by unkeyed inputs.
        _context: Entering raster facts, unused by normalized corner geometry.

    Returns:
        Acceptance, native channel faults, or the requested nonconvex quadrilateral.
    """
    frames = tuple(dict.fromkeys((scene.frame_current_final, *(float(frame) for frame in range(scene.frame_start, scene.frame_end + 1, scene.frame_step)))))
    at = (Transform.pin.__name__, Gated.value.__name__)  # type: ignore[misc]  # ty: ignore[invalid-attribute-access]
    fields = msgspec.structs.fields(Quad)

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
            samples(Transform.pin.__name__, float(record.pin.enabled), times),
            accumulated(accumulated(samples(item_key(*at, field.name), component, times, index) for index, component in enumerate(getattr(record.pin.value, field.name))) for field in fields),
        ).bind(lambda values: quads(*values))

    def endpoints(lengths: tuple[float, ...], enabled: tuple[float, ...]) -> BlenderResult[None]:
        opening = ShutterPosition[scene.render.motion_blur_position].value
        times = tuple(dict.fromkeys((*frames, *(frame + (opening + side) * length for frame, length, active in zip(frames, lengths, enabled) if active for side in range(2)))))
        return accepted(times)

    return paired(sampled_channel(scene, "render.motion_blur_shutter", frames), samples(Transform.motion_blur.__name__, float(record.motion_blur), frames)).bind(lambda values: endpoints(*values))


def pose_inputs() -> tuple[tuple[KeyPath, msgspec.structs.FieldInfo, native.Type], ...]:
    """Derives numeric temporal leaves from the complete Transform record."""

    def temporal(field: msgspec.structs.FieldInfo, shape: native.Type) -> bool:
        match shape:
            case native.Metadata(type=inner):
                return temporal(field, inner)
            case native.FloatType() | native.IntType():
                return not markers(field.type, GraphForm) and field.name != Transform.filterwidth.__name__
            case native.TupleType(item_types=items):
                return all(temporal(field, item) for item in items)
            case _:
                return False

    return tuple(row for row in rows(Transform) if temporal(row[1], row[2]))


def posed(_identity: Identity, scene: bpy.types.Scene, record: Transform) -> BlenderResult[msgspec.Struct]:
    """Initializes both endpoint records from the effect and native scene shutter."""
    return Ok(
        Poses(
            length=scene.render.motion_blur_shutter,
            opening=ShutterPosition[scene.render.motion_blur_position].value,
            **{item_key("pose", side, *at): reached(record, at) for side in range(2) for at, _, _ in pose_inputs()},
        )
    )


def keyed(identity: Identity, scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect, _record: Transform) -> BlenderResult[tuple[Animation, ...]]:
    """Samples every temporal component at both fractional shutter endpoints."""
    frames = tuple(range(scene.frame_start, scene.frame_end + 1))
    opening = ShutterPosition[scene.render.motion_blur_position].value

    def channels(lengths: tuple[float, ...], at: KeyPath, _field: msgspec.structs.FieldInfo, shape: native.Type) -> BlenderResult[tuple[Animation, ...]]:
        while isinstance(shape, native.Metadata):
            shape = shape.type
        indices = tuple(range(len(shape.item_types))) if isinstance(shape, native.TupleType) else (None,)

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
        .bind(lambda lengths: accumulated(channels(lengths, *row) for row in pose_inputs()))
        .map(lambda groups: tuple(channel for group in groups for channel in group))
    )


def extended(
    key: Key, source: Operand, uv: Operand, selector: Operand, items: MenuItems, *, vertical: Option[tuple[Operand, MenuItems]] = Nothing, filtername: str = "triangle", filterwidth: float = 2.0
) -> BlenderResult[tuple[Fragment, End]]:
    """Samples a covered map with independent borders and native triangle reconstruction.

    Args:
        key: Prefix of the sampling nodes.
        source: Associated image being reconstructed.
        uv: Source positions with geometric coverage in Z.
        selector: Horizontal extension menu, shared vertically when absent there.
        items: Native metadata-derived extension choices.
        vertical: Independent vertical extension menu and its choices.
        filtername: OIIO reconstruction request.
        filterwidth: Full support width in source pixels.

    Returns:
        The covered associated sample, or a typed refusal of unsupported reconstruction.
    """
    if (filtername, filterwidth) != ("triangle", 2.0):
        return Error((BlenderChoiceFault(path="reconstruction", identifier=f"{filtername} ({filterwidth})", accepted=("triangle (2.0)",)),))

    def selected(axis: str, choice: Operand, choices: MenuItems) -> BlenderResult[tuple[Fragment, End]]:
        return menu(f"{key}_{axis}_extension", "MENU", choice, choices, {mode.label: "Repeat" if mode is Extension.REPEAT else "Extend" for mode in Extension})

    horizontal = selected("x", selector, items)
    borders = vertical.map(lambda other: paired(horizontal, selected("y", *other))).default_with(lambda: horizontal.map(lambda held: (held, held)))

    def sampled(sides: Fragment, dimensions: Fragment, extensions: tuple[tuple[Fragment, End], tuple[Fragment, End]]) -> BlenderResult[tuple[Fragment, End]]:
        def axis(name: str, border: tuple[Fragment, End]) -> BlenderResult[tuple[tuple[Fragment, End], tuple[Fragment, End]]]:
            coordinate = ended(f"{key}_uv", name)(sides)
            size = ended(f"{key}_dimensions", name)(dimensions)
            folded = math(f"{key}_{name}_folded", "PINGPONG", coordinate, 1.0).bind(
                lambda reflected: switched(f"{key}_{name}_coordinate", "FLOAT", End(node=border[1].node, socket=Extension.MIRROR.label), coordinate, reflected)
            )
            edge = (
                math(f"{key}_{name}_opposite", "SUBTRACT", 1.0, coordinate)
                .bind(lambda opposite: math(f"{key}_{name}_edge", "MINIMUM", coordinate, opposite))
                .bind(lambda distance: math(f"{key}_{name}_support", "MULTIPLY_ADD", distance, size, 0.5, clamp=True))
                .bind(lambda support: switched(f"{key}_{name}_coverage", "FLOAT", End(node=border[1].node, socket=Extension.CLIP.label), 1.0, support))
            )
            return paired(folded, edge)

        def reconstructed(x: tuple[tuple[Fragment, End], tuple[Fragment, End]], y: tuple[tuple[Fragment, End], tuple[Fragment, End]]) -> BlenderResult[tuple[Fragment, End]]:
            coordinates = node(f"{key}_coordinates", bpy.types.ShaderNodeCombineXYZ, {"X": x[0], "Y": y[0], "Z": 1.0}, {}).map(ended(f"{key}_coordinates", "Vector"))
            coverage = math(f"{key}_border_coverage", "MULTIPLY", x[1], y[1]).bind(lambda border: math(f"{key}_coverage", "MULTIPLY", border, ended(f"{key}_uv", "Z")(sides)))
            return (
                paired(
                    coordinates.bind(
                        lambda position: node(
                            f"{key}_sample",
                            bpy.types.CompositorNodeMapUV,
                            {"Image": source, "UV": position, "Interpolation": "Bilinear", "Extension X": extensions[0], "Extension Y": extensions[1]},
                            {},
                        )
                    ).map(ended(f"{key}_sample", "Image")),
                    coverage,
                )
                .bind(lambda image: node(key, bpy.types.CompositorNodeSetAlpha, {"Image": image[0], "Alpha": image[1], "Type": "Apply Mask"}, {}))
                .map(ended(key, "Image"))
            )

        return paired(axis("X", extensions[0]), axis("Y", extensions[1])).bind(lambda axes: reconstructed(*axes))

    return paired(
        paired(
            node(f"{key}_uv", bpy.types.ShaderNodeSeparateXYZ, {"Vector": uv}, {}),
            node(f"{key}_info", bpy.types.CompositorNodeImageInfo, {"Image": source}, {}).bind(
                lambda info: node(f"{key}_dimensions", bpy.types.ShaderNodeSeparateXYZ, {"Vector": ended(f"{key}_info", "Dimensions")(info)}, {})
            ),
        ),
        borders,
    ).bind(lambda parts: sampled(*parts[0], parts[1]))


def placement(key: Key, frame: FrameSpace, position: Operand, rotation: Operand, scale: Operand, skew: Operand, pivot: Operand) -> BlenderResult[tuple[Fragment, End]]:
    """Builds scale, X shear, Y shear, and rotation about a frame pivot, then translation.

    Args:
        key: Prefix of the affine nodes.
        frame: Displayed frame coordinates.
        position: Translation as shares of each displayed side.
        rotation: Counterclockwise angle in radians.
        scale: Signed scale pair.
        skew: Horizontal then vertical shear angles.
        pivot: Fixed point as shares of each displayed side.

    Returns:
        The native forward affine matrix, including singular and reflected placements.
    """
    anchor = vector(f"{key}_pivot", "MULTIPLY", pivot, frame.extent)
    shear = node(f"{key}_skew", bpy.types.ShaderNodeSeparateXYZ, {"Vector": skew}, {})
    scaling = (
        node(f"{key}_scale_axes", bpy.types.ShaderNodeSeparateXYZ, {"Vector": scale}, {})
        .bind(lambda axes: node(f"{key}_scale_vector", bpy.types.ShaderNodeCombineXYZ, {"X": ended(f"{key}_scale_axes", "X")(axes), "Y": End(node=f"{key}_scale_axes", socket="Y"), "Z": 1.0}, {}))
        .bind(lambda axes: node(f"{key}_scale", bpy.types.FunctionNodeCombineTransform, {"Scale": ended(f"{key}_scale_vector", "Vector")(axes)}, {}))
        .map(ended(f"{key}_scale", "Transform"))
    )
    horizontal = (
        shear
        .bind(lambda axes: math(f"{key}_horizontal", "TANGENT", ended(f"{key}_skew", "X")(axes)))
        .bind(lambda tangent: node(f"{key}_shear_x", bpy.types.FunctionNodeCombineMatrix, {"Column 2 Row 1": tangent}, {}))
        .map(ended(f"{key}_shear_x", "Matrix"))
    )
    vertical = (
        shear
        .bind(lambda axes: math(f"{key}_vertical", "TANGENT", ended(f"{key}_skew", "Y")(axes)))
        .bind(lambda tangent: node(f"{key}_shear_y", bpy.types.FunctionNodeCombineMatrix, {"Column 1 Row 2": tangent}, {}))
        .map(ended(f"{key}_shear_y", "Matrix"))
    )
    turned = (
        node(f"{key}_euler", bpy.types.ShaderNodeCombineXYZ, {"Z": rotation}, {})
        .bind(lambda euler: node(f"{key}_rotation", bpy.types.FunctionNodeEulerToRotation, {"Euler": ended(f"{key}_euler", "Vector")(euler)}, {}))
        .bind(lambda angle: node(f"{key}_turn", bpy.types.FunctionNodeCombineTransform, {"Rotation": ended(f"{key}_rotation", "Rotation")(angle)}, {}))
        .map(ended(f"{key}_turn", "Transform"))
    )
    linear = (
        paired(horizontal, scaling)
        .bind(lambda parts: node(f"{key}_scaled_shear", bpy.types.FunctionNodeMatrixMultiply, dict(zip(("Matrix", "Matrix_001"), parts)), {}))
        .map(ended(f"{key}_scaled_shear", "Matrix"))
    )
    linear = (
        paired(vertical, linear).bind(lambda parts: node(f"{key}_sheared", bpy.types.FunctionNodeMatrixMultiply, dict(zip(("Matrix", "Matrix_001"), parts)), {})).map(ended(f"{key}_sheared", "Matrix"))
    )
    linear = (
        paired(turned, linear).bind(lambda parts: node(f"{key}_linear", bpy.types.FunctionNodeMatrixMultiply, dict(zip(("Matrix", "Matrix_001"), parts)), {})).map(ended(f"{key}_linear", "Matrix"))
    )

    def shifted(center: tuple[Fragment, End], matrix: tuple[Fragment, End]) -> BlenderResult[tuple[Fragment, End]]:
        moved = node(f"{key}_moved_pivot", bpy.types.FunctionNodeTransformDirection, {"Direction": center, "Transform": matrix}, {}).map(ended(f"{key}_moved_pivot", "Direction"))
        offset = vector(f"{key}_position", "MULTIPLY", position, frame.extent).bind(lambda shift: vector(f"{key}_placed_pivot", "ADD", shift, center))
        return (
            paired(offset, moved)
            .bind(lambda parts: vector(f"{key}_translation", "SUBTRACT", *parts))
            .bind(lambda shift: node(f"{key}_shift", bpy.types.FunctionNodeCombineTransform, {"Translation": shift}, {}))
            .bind(lambda translation: node(key, bpy.types.FunctionNodeMatrixMultiply, {"Matrix": ended(f"{key}_shift", "Transform")(translation), "Matrix_001": matrix}, {}))
            .map(ended(key, "Matrix"))
        )

    return paired(anchor, linear).bind(lambda parts: shifted(*parts))


def placed(key: Key, source: Operand, frame: FrameSpace, matrix: Operand) -> BlenderResult[tuple[Fragment, End]]:
    """Places the current displayed rectangle while retaining deferred native bounds.

    Args:
        key: Prefix of the placement nodes.
        source: Associated content, including its existing native transformation.
        frame: Current displayed extent and conversion to stored pixel distances.
        matrix: Forward affine matrix from placement in the same frame.

    Returns:
        Native content transformed about its current center, or transparent singular content.
    """
    axes = node(f"{key}_pixel_axes", bpy.types.ShaderNodeSeparateXYZ, {"Vector": frame.scale}, {})
    basis = (
        axes
        .bind(lambda axes: node(f"{key}_pixel_scale", bpy.types.ShaderNodeCombineXYZ, {"X": ended(f"{key}_pixel_axes", "X")(axes), "Y": End(node=f"{key}_pixel_axes", socket="Y"), "Z": 1.0}, {}))
        .bind(lambda scale: node(f"{key}_pixel_basis", bpy.types.FunctionNodeCombineTransform, {"Scale": ended(f"{key}_pixel_scale", "Vector")(scale)}, {}))
        .map(ended(f"{key}_pixel_basis", "Transform"))
    )
    inverse_basis = basis.bind(lambda basis: node(f"{key}_inverse_basis", bpy.types.FunctionNodeInvertMatrix, {"Matrix": basis}, {})).map(ended(f"{key}_inverse_basis", "Matrix"))
    pixel = inverse_basis.bind(lambda inverse: node(f"{key}_in_pixel_units", bpy.types.FunctionNodeMatrixMultiply, {"Matrix": matrix, "Matrix_001": inverse}, {})).map(
        ended(f"{key}_in_pixel_units", "Matrix")
    )
    pixel = (
        paired(basis, pixel)
        .bind(lambda parts: node(f"{key}_pixel_matrix", bpy.types.FunctionNodeMatrixMultiply, dict(zip(("Matrix", "Matrix_001"), parts)), {}))
        .map(ended(f"{key}_pixel_matrix", "Matrix"))
    )
    planar = (
        pixel
        .bind(lambda pixel: node(f"{key}_components", bpy.types.FunctionNodeSeparateMatrix, {"Matrix": pixel}, {}))
        .bind(
            lambda components: node(
                f"{key}_planar",
                bpy.types.FunctionNodeCombineMatrix,
                {**{f"Column {column} Row {row}": ended(f"{key}_components", f"Column {column} Row {row}")(components) for column in range(1, 3) for row in range(1, 3)}, "Column 3 Row 3": 0.0},
                {},
            )
        )
        .map(ended(f"{key}_planar", "Matrix"))
    )
    decomposition = planar.bind(lambda planar: node(f"{key}_svd", bpy.types.FunctionNodeMatrixSVD, {"Matrix": planar}, {}))
    determinant = pixel.bind(lambda pixel: node(f"{key}_determinant", bpy.types.FunctionNodeMatrixDeterminant, {"Matrix": pixel}, {})).map(ended(f"{key}_determinant", "Determinant"))
    reflection = determinant.bind(lambda determinant: math(f"{key}_orientation", "SIGN", determinant))
    invertible = pixel.bind(lambda pixel: node(f"{key}_inverse", bpy.types.FunctionNodeInvertMatrix, {"Matrix": pixel}, {})).map(ended(f"{key}_inverse", "Invertible"))
    center = vector(f"{key}_center", "SCALE", frame.extent, 0.5)
    translation = center.bind(
        lambda center: (
            node(f"{key}_moved_center", bpy.types.FunctionNodeTransformPoint, {"Vector": center, "Transform": matrix}, {})
            .map(ended(f"{key}_moved_center", "Vector"))
            .bind(lambda moved: vector(f"{key}_center_offset", "SUBTRACT", moved, center))
        )
    ).bind(lambda offset: vector(f"{key}_pixel_offset", "MULTIPLY", offset, frame.scale))
    location = node(f"{key}_source_info", bpy.types.CompositorNodeImageInfo, {"Image": source}, {}).map(ended(f"{key}_source_info", "Location"))

    def rotated(image: Operand, decomposition: Fragment, side: str, direction: float) -> BlenderResult[tuple[Fragment, End]]:
        components = node(f"{key}_{side}_components", bpy.types.FunctionNodeSeparateMatrix, {"Matrix": ended(f"{key}_svd", side)(decomposition)}, {})
        angle = components.bind(
            lambda components: math(f"{key}_{side}_angle", "ARCTAN2", ended(f"{key}_{side}_components", "Column 1 Row 2")(components), End(node=f"{key}_{side}_components", socket="Column 1 Row 1"))
        ).bind(lambda angle: math(f"{key}_{side}_turn", "MULTIPLY", angle, direction))
        return angle.bind(lambda angle: node(f"{key}_{side}_rotated", bpy.types.CompositorNodeRotate, {"Image": image, "Angle": angle}, {})).map(ended(f"{key}_{side}_rotated", "Image"))

    def positioned(
        location: tuple[Fragment, End], decomposition: Fragment, reflection: tuple[Fragment, End], translation: tuple[Fragment, End], invertible: tuple[Fragment, End]
    ) -> BlenderResult[tuple[Fragment, End]]:
        recentered = (
            vector(f"{key}_negative_location", "SCALE", location, -1.0)
            .bind(lambda negative: node(f"{key}_recentering", bpy.types.ShaderNodeSeparateXYZ, {"Vector": negative}, {}))
            .bind(
                lambda components: node(
                    f"{key}_recentered", bpy.types.CompositorNodeTranslate, {"Image": source, "X": ended(f"{key}_recentering", "X")(components), "Y": End(node=f"{key}_recentering", socket="Y")}, {}
                )
            )
            .map(ended(f"{key}_recentered", "Image"))
        )
        scales = node(f"{key}_singular_values", bpy.types.ShaderNodeSeparateXYZ, {"Vector": ended(f"{key}_svd", "S")(decomposition)}, {})
        signed = scales.bind(lambda values: math(f"{key}_signed_scale", "MULTIPLY", ended(f"{key}_singular_values", "Y")(values), reflection))
        stretched = (
            paired(recentered.bind(lambda image: rotated(image, decomposition, "V", -1.0)), paired(scales, signed))
            .bind(
                lambda parts: node(
                    f"{key}_stretched", bpy.types.CompositorNodeScale, {"Image": parts[0], "Type": "Relative", "X": ended(f"{key}_singular_values", "X")(parts[1][0]), "Y": parts[1][1]}, {}
                )
            )
            .map(ended(f"{key}_stretched", "Image"))
            .bind(lambda image: rotated(image, decomposition, "U", 1.0))
        )
        offset = vector(f"{key}_location", "ADD", location, translation).bind(lambda offset: node(f"{key}_location_components", bpy.types.ShaderNodeSeparateXYZ, {"Vector": offset}, {}))
        image = (
            paired(stretched, offset)
            .bind(
                lambda parts: node(
                    f"{key}_translated",
                    bpy.types.CompositorNodeTranslate,
                    {
                        "Image": parts[0],
                        "X": ended(f"{key}_location_components", "X")(parts[1]),
                        "Y": End(node=f"{key}_location_components", socket="Y"),
                        "Interpolation": "Bilinear",
                        "Extension X": "Clip",
                        "Extension Y": "Clip",
                    },
                    {},
                )
            )
            .map(ended(f"{key}_translated", "Image"))
        )
        return image.bind(lambda image: switched(key, "RGBA", invertible, (0.0,) * 4, image))

    return paired(paired(location, decomposition), paired(paired(reflection, translation), invertible)).bind(lambda parts: positioned(*parts[0], *parts[1][0], parts[1][1]))


def tilted(key: Key, uv: Operand, lens: Operand, heading: Operand, elevation: Operand, *, forward: bool = False) -> BlenderResult[tuple[Fragment, End]]:
    """Maps positions through the actual lens and the panorama owner's YXZ tilt.

    Args:
        key: Prefix of the camera nodes.
        uv: Positions in the complete image's normalized frame.
        lens: Native camera projection matrix of that image.
        heading: Rightward view turn.
        elevation: Upward view turn.
        forward: Forward content placement instead of inverse sampling.

    Returns:
        Source positions and front-facing geometric coverage in Z.
    """
    device = vector(f"{key}_device", "MULTIPLY_ADD", uv, (2.0, 2.0, 0.0), (-1.0, -1.0, 1.0))
    inverse = node(f"{key}_inverse_lens", bpy.types.FunctionNodeInvertMatrix, {"Matrix": lens}, {}).map(ended(f"{key}_inverse_lens", "Matrix"))
    ray = paired(device, inverse).bind(lambda parts: node(f"{key}_ray", bpy.types.FunctionNodeProjectPoint, dict(zip(("Vector", "Transform"), parts)), {})).map(ended(f"{key}_ray", "Vector"))
    negative = math(f"{key}_negative", "MULTIPLY", heading if forward else elevation, -1.0)
    first_axis, second_axis = ("X_AXIS", "Y_AXIS") if forward else ("Y_AXIS", "X_AXIS")
    first = ray.bind(lambda direction: node(f"{key}_first", bpy.types.ShaderNodeVectorRotate, {"Vector": direction, "Angle": elevation if forward else heading}, {"rotation_type": first_axis})).map(
        ended(f"{key}_first", "Vector")
    )
    turned = (
        paired(first, negative)
        .bind(lambda parts: node(f"{key}_turned", bpy.types.ShaderNodeVectorRotate, dict(zip(("Vector", "Angle"), parts)), {"rotation_type": second_axis}))
        .map(ended(f"{key}_turned", "Vector"))
    )
    front = turned.bind(lambda direction: node(f"{key}_depth", bpy.types.ShaderNodeSeparateXYZ, {"Vector": direction}, {})).bind(
        lambda depth: math(f"{key}_front", "LESS_THAN", ended(f"{key}_depth", "Z")(depth), 0.0)
    )
    coverage = node(f"{key}_coverage_axes", bpy.types.ShaderNodeSeparateXYZ, {"Vector": uv}, {}).map(ended(f"{key}_coverage_axes", "Z"))
    covered = paired(front, coverage).bind(lambda parts: math(f"{key}_coverage", "MULTIPLY", *parts))
    projected = turned.bind(lambda direction: node(f"{key}_projected", bpy.types.FunctionNodeProjectPoint, {"Vector": direction, "Transform": lens}, {})).map(ended(f"{key}_projected", "Vector"))
    return paired(projected, covered).bind(
        lambda parts: vector(f"{key}_position", "MULTIPLY_ADD", parts[0], (0.5, 0.5, 0.0), (0.5, 0.5, 0.0)).bind(lambda position: vector(key, "MULTIPLY_ADD", (0.0, 0.0, 1.0), parts[1], position))
    )


def transformed(shell: Shell[Transform]) -> BlenderResult[tuple[Fragment, End]]:
    """Composes sampled camera poses and reconstructs their native shutter approximation."""
    shake = (Transform.shake.__name__, Gated.value.__name__)  # type: ignore[misc]  # ty: ignore[invalid-attribute-access]
    affine = (Transform.affine.__name__,)
    tilt = (Transform.tilt.__name__,)
    pin = (Transform.pin.__name__, Gated.value.__name__)  # type: ignore[misc]  # ty: ignore[invalid-attribute-access]
    basis = Perlin(dimensions=1, size=1.0, stretch=1.0, distortion=0.0)
    camera = node("camera", bpy.types.GeometryNodeInputActiveCamera, {}, {}).map(ended("camera", "Active Camera"))
    lens = camera.bind(lambda camera: node("lens", bpy.types.GeometryNodeCameraInfo, {"Camera": camera}, {})).map(ended("lens", "Projection Matrix"))

    def built(frame: FrameSpace, lens: tuple[Fragment, End]) -> BlenderResult[tuple[Fragment, End]]:
        def value(side: int | None, *at: str) -> End:
            return shell.socket(*at) if side is None else shell.socket(item_key("pose", side, *at))

        def pinned(key: str, side: int | None, fraction: Operand, *, forward: bool) -> BlenderResult[tuple[Fragment, End]]:
            lower_left = value(side, *pin, Quad.bottom_left.__name__)
            lower_right = value(side, *pin, Quad.bottom_right.__name__)
            upper_left = value(side, *pin, Quad.top_left.__name__)
            upper_right = value(side, *pin, Quad.top_right.__name__)
            horizontal = vector(f"{key}_horizontal", "SUBTRACT", lower_right, upper_right)
            vertical = vector(f"{key}_vertical", "SUBTRACT", upper_left, upper_right)
            diagonal = (
                vector(f"{key}_diagonal", "SUBTRACT", lower_left, lower_right)
                .bind(lambda difference: vector(f"{key}_diagonal_sum", "ADD", difference, upper_right))
                .bind(lambda difference: vector(f"{key}_diagonal_offset", "SUBTRACT", difference, upper_left))
            )
            denominator = (
                paired(horizontal, vertical).bind(lambda axes: vector(f"{key}_cross", "CROSS_PRODUCT", *axes)).bind(lambda cross: vector(f"{key}_denominator", "DOT_PRODUCT", cross, (0.0, 0.0, 1.0)))
            )
            g = (
                paired(diagonal, vertical)
                .bind(lambda axes: vector(f"{key}_g_cross", "CROSS_PRODUCT", *axes))
                .bind(lambda cross: vector(f"{key}_g_numerator", "DOT_PRODUCT", cross, (0.0, 0.0, 1.0)))
                .bind(lambda numerator: denominator.bind(lambda denominator: math(f"{key}_g", "DIVIDE", numerator, denominator)))
            )
            h = (
                paired(horizontal, diagonal)
                .bind(lambda axes: vector(f"{key}_h_cross", "CROSS_PRODUCT", *axes))
                .bind(lambda cross: vector(f"{key}_h_numerator", "DOT_PRODUCT", cross, (0.0, 0.0, 1.0)))
                .bind(lambda numerator: denominator.bind(lambda denominator: math(f"{key}_h", "DIVIDE", numerator, denominator)))
            )
            columns = paired(
                vector(f"{key}_x_difference", "SUBTRACT", lower_right, lower_left).bind(
                    lambda difference: g.bind(lambda factor: vector(f"{key}_x_column", "MULTIPLY_ADD", lower_right, factor, difference))
                ),
                vector(f"{key}_y_difference", "SUBTRACT", upper_left, lower_left).bind(
                    lambda difference: h.bind(lambda factor: vector(f"{key}_y_column", "MULTIPLY_ADD", upper_left, factor, difference))
                ),
            ).bind(
                lambda columns: accumulated((
                    *(node(f"{key}_column_{column}", bpy.types.ShaderNodeSeparateXYZ, {"Vector": value}, {}) for column, value in enumerate(columns, start=1)),
                    node(f"{key}_column_4", bpy.types.ShaderNodeSeparateXYZ, {"Vector": lower_left}, {}),
                ))
            )
            matrix = (
                paired(columns, paired(g, h))
                .bind(
                    lambda parts: node(
                        f"{key}_matrix",
                        bpy.types.FunctionNodeCombineMatrix,
                        {
                            **{
                                f"Column {column} Row {row}": ended(f"{key}_column_{column}", axis)(components)
                                for column, components in zip((1, 2, 4), parts[0])
                                for row, axis in enumerate(("X", "Y"), start=1)
                            },
                            "Column 1 Row 4": parts[1][0],
                            "Column 2 Row 4": parts[1][1],
                        },
                        {},
                    )
                )
                .map(ended(f"{key}_matrix", "Matrix"))
            )
            transform = matrix if forward else matrix.bind(lambda matrix: node(f"{key}_inverse", bpy.types.FunctionNodeInvertMatrix, {"Matrix": matrix}, {})).map(ended(f"{key}_inverse", "Matrix"))
            homogeneous = (
                transform
                .bind(lambda transform: node(f"{key}_components", bpy.types.FunctionNodeSeparateMatrix, {"Matrix": transform}, {}))
                .bind(
                    lambda components: node(
                        f"{key}_homogeneous",
                        bpy.types.ShaderNodeCombineXYZ,
                        {axis: ended(f"{key}_components", f"Column {column} Row 4")(components) for axis, column in zip(("X", "Y", "Z"), (1, 2, 4))},
                        {},
                    )
                )
                .map(ended(f"{key}_homogeneous", "Vector"))
            )
            planar = vector(f"{key}_planar", "MULTIPLY", fraction, (1.0, 1.0, 0.0))
            weight = (
                paired(homogeneous, planar.bind(lambda point: vector(f"{key}_unit", "ADD", point, (0.0, 0.0, 1.0))))
                .bind(lambda parts: vector(f"{key}_weight", "DOT_PRODUCT", *parts))
                .bind(lambda weight: math(f"{key}_front", "GREATER_THAN", weight, 0.0))
            )
            position = (
                paired(planar, transform)
                .bind(lambda parts: node(f"{key}_projected", bpy.types.FunctionNodeProjectPoint, dict(zip(("Vector", "Transform"), parts)), {}))
                .map(ended(f"{key}_projected", "Vector"))
            )
            position = paired(weight, position).bind(lambda parts: masked(f"{key}_near", "VECTOR", parts[0], (0.0,) * 3, parts[1]))
            coverage = (
                node(f"{key}_coverage", bpy.types.ShaderNodeSeparateXYZ, {"Vector": fraction}, {})
                .map(ended(f"{key}_coverage", "Z"))
                .bind(lambda coverage: weight.bind(lambda weight: math(f"{key}_covered", "MULTIPLY", coverage, weight)))
            )
            return (
                paired(position, coverage)
                .bind(lambda parts: vector(f"{key}_covered_position", "MULTIPLY_ADD", (0.0, 0.0, 1.0), parts[1], parts[0]))
                .bind(lambda mapped: switched(key, "VECTOR", shell.socket(Transform.pin.__name__), fraction, mapped))
            )

        def reaimed(key: str, side: int | None, fraction: Operand, *, forward: bool) -> BlenderResult[tuple[Fragment, End]]:
            active = (
                paired(math(f"{key}_heading", "ABSOLUTE", value(side, *tilt, Tilt.heading.__name__)), math(f"{key}_elevation", "ABSOLUTE", value(side, *tilt, Tilt.elevation.__name__)))
                .bind(lambda angles: math(f"{key}_amount", "ADD", *angles))
                .bind(lambda amount: math(f"{key}_active", "GREATER_THAN", amount, 0.0))
            )
            margin = math(f"{key}_margin", "MULTIPLY_ADD", shell.scale, -0.5, 0.5)
            device = (
                margin
                .bind(lambda margin: vector(f"{key}_device", "MULTIPLY_ADD", fraction, shell.scale, margin))
                .bind(lambda device: vector(f"{key}_device_xy", "MULTIPLY", device, (1.0, 1.0, 0.0)))
                .bind(lambda device: vector(f"{key}_device_position", "ADD", device, (0.0, 0.0, 1.0)))
            )
            projected = device.bind(lambda device: tilted(f"{key}_tilt", device, lens, value(side, *tilt, Tilt.heading.__name__), value(side, *tilt, Tilt.elevation.__name__), forward=forward))
            position = (
                paired(projected, margin)
                .bind(lambda parts: vector(f"{key}_frame_offset", "SUBTRACT", *parts))
                .bind(lambda offset: vector(f"{key}_fraction", "DIVIDE", offset, shell.scale))
                .bind(lambda fraction: vector(f"{key}_xy", "MULTIPLY", fraction, (1.0, 1.0, 0.0)))
            )
            coverage = paired(
                node(f"{key}_entering", bpy.types.ShaderNodeSeparateXYZ, {"Vector": fraction}, {}).map(ended(f"{key}_entering", "Z")),
                projected.bind(lambda projected: node(f"{key}_projected", bpy.types.ShaderNodeSeparateXYZ, {"Vector": projected}, {})).map(ended(f"{key}_projected", "Z")),
            ).bind(lambda parts: math(f"{key}_coverage", "MULTIPLY", *parts))
            return (
                paired(position, coverage)
                .bind(lambda parts: vector(f"{key}_covered", "MULTIPLY_ADD", (0.0, 0.0, 1.0), parts[1], parts[0]))
                .bind(lambda projected: active.bind(lambda active: switched(key, "VECTOR", active, fraction, projected)))
            )

        def pose(side: int | None) -> BlenderResult[tuple[Fragment, End]]:
            key = "reference" if side is None else f"pose_{side}"
            offset = math(f"{key}_zero", "MULTIPLY", shell.socket(Shutter.opening.__name__), 0.0) if side is None else math(f"{key}_offset", "ADD", shell.socket(Shutter.opening.__name__), side)
            clock = Clock(key=f"{key}_clock", speed=value(side, *shake, Shake.motion.__name__, Motion.speed.__name__), hold=value(side, *shake, Shake.motion.__name__, Motion.hold.__name__)).value
            time = paired(offset, clock).bind(
                lambda parts: math(f"{key}_elapsed", "MULTIPLY", parts[0], shell.socket(Shutter.length.__name__)).bind(
                    lambda elapsed: math(f"{key}_time", "MULTIPLY_ADD", elapsed, value(side, *shake, Shake.motion.__name__, Motion.speed.__name__), parts[1])
                )
            )
            point = time.bind(lambda time: node(f"{key}_time_axis", bpy.types.ShaderNodeCombineXYZ, {"X": time}, {})).map(ended(f"{key}_time_axis", "Vector"))
            fields = point.bind(
                lambda point: accumulated(
                    sample(
                        f"{key}_shake_{stream}",
                        basis,
                        point,
                        0.0,
                        value(side, *shake, Shake.seed.__name__),
                        stream,
                        {(Perlin.octaves.__name__, field.name): value(side, *shake, Shake.octaves.__name__, field.name) for field in msgspec.structs.fields(Octaves)},
                    ).bind(lambda noise: math(f"{key}_signed_{stream}", "MULTIPLY_ADD", noise, 2.0, -1.0))
                    for stream in range(4)
                )
            )
            base = placement(f"{key}_affine", frame, **{field.name: value(side, *affine, field.name) for field in msgspec.structs.fields(Affine)})

            def shaken(fields: tuple[tuple[Fragment, End], ...]) -> BlenderResult[tuple[Fragment, End]]:
                position = (
                    node(f"{key}_shake_axes", bpy.types.ShaderNodeCombineXYZ, {"X": fields[0], "Y": fields[1]}, {})
                    .map(ended(f"{key}_shake_axes", "Vector"))
                    .bind(lambda axes: vector(f"{key}_shake_position", "MULTIPLY", axes, value(side, *shake, Shake.position.__name__)))
                    .bind(lambda position: vector(f"{key}_shake_fraction", "DIVIDE", position, frame.extent))
                    .bind(lambda position: switched(f"{key}_shake_translation", "VECTOR", shell.socket(Transform.shake.__name__), (0.0,) * 3, position))
                )
                rotation = math(f"{key}_shake_angle", "MULTIPLY", fields[2], value(side, *shake, Shake.rotation.__name__)).bind(
                    lambda angle: switched(f"{key}_shake_rotation", "FLOAT", shell.socket(Transform.shake.__name__), 0.0, angle)
                )
                scale = (
                    math(f"{key}_shake_stops", "MULTIPLY", fields[3], value(side, *shake, Shake.scale.__name__))
                    .bind(lambda stops: math(f"{key}_shake_factor", "POWER", 2.0, stops))
                    .bind(lambda factor: switched(f"{key}_shake_scale", "FLOAT", shell.socket(Transform.shake.__name__), 1.0, factor))
                )
                return paired(paired(position, rotation), scale).bind(lambda parts: placement(f"{key}_shake", frame, *parts[0], parts[1], (0.0,) * 3, (0.5, 0.5, 0.0)))

            def temporal(offset: tuple[Fragment, End]) -> BlenderResult[tuple[Fragment, End]]:
                position = vector(f"{key}_travel", "SCALE", value(side, Transform.travel.__name__), offset)
                rotation = math(f"{key}_turn", "MULTIPLY", value(side, Transform.turn.__name__), offset)
                scale = math(f"{key}_zoom", "MULTIPLY", value(side, Transform.zoom.__name__), offset).bind(lambda stops: math(f"{key}_factor", "POWER", 2.0, stops))
                return paired(paired(position, rotation), scale).bind(lambda parts: placement(f"{key}_temporal", frame, *parts[0], parts[1], (0.0,) * 3, value(side, *affine, Affine.pivot.__name__)))

            placed = (
                paired(fields.bind(shaken), base)
                .bind(lambda parts: node(f"{key}_placed", bpy.types.FunctionNodeMatrixMultiply, dict(zip(("Matrix", "Matrix_001"), parts)), {}))
                .map(ended(f"{key}_placed", "Matrix"))
            )
            return paired(offset.bind(temporal), placed).bind(lambda parts: node(key, bpy.types.FunctionNodeMatrixMultiply, dict(zip(("Matrix", "Matrix_001"), parts)), {})).map(ended(key, "Matrix"))

        def mapped(key: str, side: int | None, uv: Operand, *, forward: bool = False) -> BlenderResult[tuple[Fragment, End]]:
            matrix = pose(side)
            inverse = matrix.bind(lambda matrix: node(f"{key}_inverse", bpy.types.FunctionNodeInvertMatrix, {"Matrix": matrix}, {}))
            coverage = node(f"{key}_entering", bpy.types.ShaderNodeSeparateXYZ, {"Vector": uv}, {}).map(ended(f"{key}_entering", "Z"))
            fraction = (
                vector(f"{key}_frame_offset", "SUBTRACT", uv, frame.margin)
                .bind(lambda offset: vector(f"{key}_fraction", "DIVIDE", offset, frame.placing))
                .bind(lambda fraction: coverage.bind(lambda coverage: vector(f"{key}_covered_fraction", "MULTIPLY_ADD", (0.0, 0.0, 1.0), coverage, fraction)))
            )
            lifted = (
                fraction.bind(lambda fraction: pinned(f"{key}_pin_forward", side, fraction, forward=True)).bind(lambda fraction: reaimed(f"{key}_tilt_forward", side, fraction, forward=True))
                if forward
                else fraction
            )
            point = lifted.bind(lambda fraction: vector(f"{key}_point", "MULTIPLY", fraction, frame.extent))
            transform = matrix if forward else inverse.map(ended(f"{key}_inverse", "Matrix"))
            transformed = (
                paired(point, transform)
                .bind(lambda parts: node(f"{key}_transformed", bpy.types.FunctionNodeTransformPoint, dict(zip(("Vector", "Transform"), parts)), {}))
                .map(ended(f"{key}_transformed", "Vector"))
            )
            coverage = lifted.bind(lambda fraction: node(f"{key}_lifted_axes", bpy.types.ShaderNodeSeparateXYZ, {"Vector": fraction}, {})).map(ended(f"{key}_lifted_axes", "Z"))
            mapped = transformed.bind(lambda point: vector(f"{key}_mapped_fraction", "DIVIDE", point, frame.extent)).bind(
                lambda fraction: coverage.bind(lambda coverage: vector(f"{key}_mapped_coverage", "MULTIPLY_ADD", (0.0, 0.0, 1.0), coverage, fraction))
            )
            projected = (
                mapped
                if forward
                else mapped.bind(lambda fraction: reaimed(f"{key}_tilt_inverse", side, fraction, forward=False)).bind(lambda fraction: pinned(f"{key}_pin_inverse", side, fraction, forward=False))
            )
            coverage = projected.bind(lambda fraction: node(f"{key}_projected_axes", bpy.types.ShaderNodeSeparateXYZ, {"Vector": fraction}, {})).map(ended(f"{key}_projected_axes", "Z"))
            coverage = paired(coverage, inverse).bind(lambda parts: math(f"{key}_coverage", "MULTIPLY", parts[0], ended(f"{key}_inverse", "Invertible")(parts[1])))
            position = projected.bind(lambda fraction: sampled(f"{key}_position", frame, fraction)).bind(lambda uv: vector(f"{key}_planar", "MULTIPLY", uv, (1.0, 1.0, 0.0)))
            return paired(position, coverage).bind(lambda parts: vector(key, "MULTIPLY_ADD", (0.0, 0.0, 1.0), parts[1], parts[0]))

        coordinates = sampled("output_uv", frame, frame.fraction)
        reference = coordinates.bind(lambda uv: mapped("reference_uv", None, uv))
        opening = coordinates.bind(lambda uv: mapped("opening_uv", 0, uv))
        uv = paired(reference, opening).bind(lambda parts: switched("uv", "VECTOR", shell.socket(Transform.motion_blur.__name__), *parts))
        image = uv.bind(
            lambda uv: extended(
                "sample",
                shell.source,
                uv,
                shell.socket(Transform.extension.__name__),
                shell.items[Transform.extension.__name__,],
                filtername=shell.record.filtername,
                filterwidth=shell.record.filterwidth,
            )
        )
        dimensions = node("image_info", bpy.types.CompositorNodeImageInfo, {"Image": shell.source}, {}).map(ended("image_info", "Dimensions"))

        def blurred(image: tuple[Fragment, End], dimensions: tuple[Fragment, End]) -> BlenderResult[tuple[Fragment, End]]:
            center = (0.5, 0.5, 1.0)
            step = vector("stored_pixel", "DIVIDE", (1.0, 0.0, 0.0), dimensions).bind(lambda step: vector("endpoint_pixel", "ADD", center, step))
            endpoints = paired(
                mapped("center_open", 0, center).bind(lambda uv: mapped("center_close", 1, uv, forward=True)),
                step.bind(lambda uv: mapped("axis_open", 0, uv)).bind(lambda uv: mapped("axis_close", 1, uv, forward=True)),
            )

            def fitted(ends: tuple[tuple[Fragment, End], tuple[Fragment, End]]) -> BlenderResult[tuple[Fragment, End]]:
                translation = vector("endpoint_offset", "SUBTRACT", ends[0], center).bind(lambda offset: vector("endpoint_translation", "MULTIPLY", offset, dimensions))
                factor = vector("endpoint_axis", "SUBTRACT", ends[1], ends[0]).bind(lambda axis: vector("endpoint_factor", "MULTIPLY", axis, dimensions))
                scale = factor.bind(lambda factor: vector("endpoint_scale", "LENGTH", factor))
                rotation = factor.bind(lambda factor: node("endpoint_axes", bpy.types.ShaderNodeSeparateXYZ, {"Vector": factor}, {})).bind(
                    lambda axes: math("endpoint_rotation", "ARCTAN2", ended("endpoint_axes", "Y")(axes), End(node="endpoint_axes", socket="X"))
                )
                length = translation.bind(lambda translation: vector("endpoint_length", "LENGTH", translation))
                diagonal = vector("diagonal", "LENGTH", dimensions)
                amount = paired(paired(length, scale), diagonal).bind(
                    lambda parts: math("scaled_diagonal", "MULTIPLY", parts[0][1], parts[1]).bind(lambda diagonal: math("translation_amount", "DIVIDE", parts[0][0], diagonal))
                )
                direction = translation.bind(lambda translation: node("translation_axes", bpy.types.ShaderNodeSeparateXYZ, {"Vector": translation}, {})).bind(
                    lambda axes: math("negative_translation_x", "MULTIPLY", ended("translation_axes", "X")(axes), -1.0).bind(
                        lambda x: math("translation_direction", "ARCTAN2", End(node="translation_axes", socket="Y"), x)
                    )
                )
                samples = (
                    paired(paired(scale, rotation), diagonal)
                    .bind(
                        lambda parts: (
                            math("scale_travel", "SUBTRACT", parts[0][0], 1.0)
                            .bind(lambda scale: math("scale_distance", "ABSOLUTE", scale))
                            .bind(lambda scale: math("rotation_distance", "ABSOLUTE", parts[0][1]).bind(lambda turn: math("radial_distance", "ADD", scale, turn)))
                            .bind(lambda distance: math("radial_pixels", "MULTIPLY", distance, parts[1]))
                        )
                    )
                    .bind(lambda radial: length.bind(lambda length: math("trail_pixels", "ADD", length, radial)))
                    .bind(lambda distance: math("sample_exponent", "LOGARITHM", distance, 2.0))
                    .bind(lambda exponent: math("samples", "CEIL", exponent))
                )

                def filtered(parameters: tuple[tuple[Fragment, End], ...]) -> BlenderResult[tuple[Fragment, End]]:
                    settings = dict(zip(("Scale", "Rotation", "Translation Amount", "Translation Direction", "Samples"), parameters))
                    unit = node("coverage_coordinates", bpy.types.CompositorNodeImageCoordinates, {"Image": shell.source}, {}).map(ended("coverage_coordinates", "Pixel"))
                    unit = unit.bind(lambda point: vector("coverage_zero", "SCALE", point, 0.0)).bind(lambda zero: vector("coverage_one", "ADD", zero, (1.0,) * 3))
                    coverage = unit.bind(lambda unit: node("coverage", bpy.types.CompositorNodeSetAlpha, {"Image": unit, "Alpha": 1.0, "Type": "Replace Alpha"}, {})).map(ended("coverage", "Image"))
                    held = paired(
                        node("trail", bpy.types.CompositorNodeDBlur, {"Image": image, "Center": (0.5, 0.5, 0.0), **settings}, {}).map(ended("trail", "Image")),
                        coverage.bind(lambda coverage: node("trail_coverage", bpy.types.CompositorNodeDBlur, {"Image": coverage, "Center": (0.5, 0.5, 0.0), **settings}, {})).map(
                            ended("trail_coverage", "Image")
                        ),
                    )
                    rgb = held.bind(lambda parts: vector("normalized_trail", "DIVIDE", *parts))
                    alpha = held.bind(
                        lambda parts: paired(
                            node("trail_channels", bpy.types.CompositorNodeSeparateColor, {"Image": parts[0]}, {"mode": "RGB"}),
                            node("coverage_channels", bpy.types.CompositorNodeSeparateColor, {"Image": parts[1]}, {"mode": "RGB"}),
                        )
                    ).bind(lambda parts: math("normalized_alpha", "DIVIDE", ended("trail_channels", "Alpha")(parts[0]), ended("coverage_channels", "Alpha")(parts[1])))
                    return (
                        paired(rgb, alpha)
                        .bind(lambda parts: node("blurred", bpy.types.CompositorNodeSetAlpha, {"Image": parts[0], "Alpha": parts[1], "Type": "Replace Alpha"}, {}))
                        .map(ended("blurred", "Image"))
                    )

                return accumulated((scale, rotation, amount, direction, samples)).bind(filtered)

            return endpoints.bind(fitted).bind(lambda blurred: switched("result", "RGBA", shell.socket(Transform.motion_blur.__name__), image, blurred))

        affine_identity = (
            pose(None)
            .bind(lambda matrix: node("identity_matrix", bpy.types.FunctionNodeSeparateMatrix, {"Matrix": matrix}, {}))
            .bind(
                lambda components: accumulated(
                    compared(f"identity_{column}_{row}", "EQUAL", ended("identity_matrix", f"Column {column} Row {row}")(components), float(column == row), epsilon=Some(0.0))
                    for column in (1, 2, 4)
                    for row in range(1, 3)
                )
            )
            .bind(lambda matches: product("identity_affine", (1.0, *matches)))
        )
        defaults = Quad()
        pin_identity = (
            accumulated(
                node(
                    f"identity_pin_{field.name}",
                    bpy.types.FunctionNodeCompare,
                    {"A": shell.socket(*pin, field.name), "B": (*getattr(defaults, field.name), 0.0), "Epsilon": 0.0},
                    {"data_type": "VECTOR", "operation": "EQUAL", "mode": "ELEMENT"},
                ).map(ended(f"identity_pin_{field.name}", "Result"))
                for field in msgspec.structs.fields(Quad)
            )
            .bind(lambda matches: product("identity_pin", (1.0, *matches)))
            .bind(lambda identity: switched("identity_pinned", "FLOAT", shell.socket(Transform.pin.__name__), 1.0, identity))
        )
        tilt_identity = (
            paired(math("identity_heading", "ABSOLUTE", value(None, *tilt, Tilt.heading.__name__)), math("identity_elevation", "ABSOLUTE", value(None, *tilt, Tilt.elevation.__name__)))
            .bind(lambda angles: math("identity_tilt_amount", "ADD", *angles))
            .bind(lambda amount: compared("identity_tilt", "EQUAL", amount, 0.0, epsilon=Some(0.0)))
        )
        identity = (
            accumulated((affine_identity, pin_identity, tilt_identity))
            .bind(lambda matches: product("identity", (1.0, *matches)))
            .bind(lambda identity: switched("identity_stationary", "FLOAT", shell.socket(Transform.motion_blur.__name__), identity, 0.0))
        )
        return paired(image, dimensions).bind(lambda parts: blurred(*parts)).bind(lambda image: identity.bind(lambda identity: switched("transformed", "RGBA", identity, image, shell.source)))

    return paired(framed("frame", shell.source, shell.aspect, shell.scale), lens).bind(lambda parts: built(*parts))


def aimed(record: Transform) -> Condition:
    """Requires perspective optics when the effect turns the camera view."""
    return camera_type("PERSP") if record.tilt.heading != 0.0 or record.tilt.elevation != 0.0 else AllOf(conditions=())


# --- [COMPOSITION] ----------------------------------------------------------------------

Poses = msgspec.defstruct(
    "Poses",
    [
        (item_key("pose", side, *at), field.type, msgspec.field(default_factory=field.default_factory) if field.default_factory is not msgspec.NODEFAULT else msgspec.field(default=field.default))
        for side in range(2)
        for at, field, _ in pose_inputs()
    ],
    bases=(Shutter,),
    module=__name__,
    frozen=True,
    kw_only=True,
)
transform: CompositorEffect[Transform] = CompositorEffect(
    record=Transform,
    graph=transformed,
    coverage=Coverage.RESULT,
    requested=Some(precise),
    condition=Some(aimed),
    derived=Some(Derived(record=Poses, of=Some(posed), reads=shutter.reads)),
    animated=Some(keyed),
    native_admitted=Some(admitted),
    registrables=(inspection(lambda: transform, "transform_uv", "Transform UV", "Inspect source positions and mapping coverage", lambda: End(node="uv", socket="Output")),),
)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["ConvexQuad", "Poses", "admitted", "aimed", "extended", "keyed", "placed", "placement", "pose_inputs", "posed", "tilted", "transform", "transformed"]
