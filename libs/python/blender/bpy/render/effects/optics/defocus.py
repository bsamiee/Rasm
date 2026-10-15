"""Camera and spatial defocus fields, shared iris geometry, and native bokeh gathering."""

from math import pi, tau

import bpy
from expression import Nothing, Option, Some
from expression.extra.result import catch
import msgspec

from blender.bpy.commands.operators import Call, Done, Operator, Outcome, Unchanged
from blender.bpy.extension.identity import Key
from blender.bpy.faults import BlenderAbsentFault, BlenderReadFault, BlenderResult
from blender.bpy.nodes.coordinates import framed, FrameSpace, lensed, pixels
from blender.bpy.nodes.graphs import End, ended, Fragment, math, menu, node, Operand, switched, vector
from blender.bpy.render.effects.stack import CompositorEffect, Coverage, Named, Shell
from blender.bpy.render.effects.viewers import inspection
from blender.bpy.render.view_layers.passes import PassRequest
from blender.imaging.chain import Effect
from blender.imaging.compositing.blending import Entry
from blender.imaging.effects.optics.defocus import Aperture, Defocus, DepthOfField, FieldCurvature, TiltShift
from blender.imaging.faults import accumulated, paired

# --- [OPERATIONS] -----------------------------------------------------------------------


def outlined(
    key: Key, point: Operand, radius: Operand, blades: Operand, rotation: Operand, roundness: Operand, obstruction: Operand
) -> BlenderResult[tuple[tuple[Fragment, End], tuple[Fragment, End]]]:
    """Returns outer and obstructed inner iris boundary fields.

    Args:
        key: Prefix of the boundary's native node keys.
        point: Iris-centered XY position, with squeeze and reflection already applied.
        radius: Circumradius in the position's units.
        blades: Polygon side count.
        rotation: Clockwise turn of a vertex from vertical.
        roundness: Interpolation from the blade outline to a circle.
        obstruction: Central mirror as a fraction of the outer outline.

    Returns:
        Signed outer and inner offsets, positive outside each boundary, with their
        shared graph, or accumulated native construction faults.
    """
    polar = node(f"{key}_axes", bpy.types.ShaderNodeSeparateXYZ, {"Vector": point}, {}).bind(
        lambda axes: math(f"{key}_angle", "ARCTAN2", ended(f"{key}_axes", "Y")(axes), End(node=f"{key}_axes", socket="X"))
    )
    sector = math(f"{key}_sector", "DIVIDE", tau, blades)
    rho = vector(f"{key}_radius", "LENGTH", point)

    def bounded(angle: Operand, sector: Operand, rho: Operand) -> BlenderResult[tuple[tuple[Fragment, End], tuple[Fragment, End]]]:
        half = math(f"{key}_half_sector", "MULTIPLY", sector, 0.5)
        phase = (
            math(f"{key}_turned", "ADD", angle, rotation)
            .bind(lambda turned: math(f"{key}_phase", "SUBTRACT", turned, pi / 2.0))
            .bind(lambda phase: math(f"{key}_wrapped", "FLOORED_MODULO", phase, sector))
        )
        return (
            paired(half, phase)
            .bind(
                lambda parts: paired(
                    math(f"{key}_apothem_cosine", "COSINE", parts[0]).bind(lambda cosine: math(f"{key}_apothem", "MULTIPLY", radius, cosine)),
                    math(f"{key}_local", "SUBTRACT", parts[1], parts[0]).bind(lambda local: math(f"{key}_sine", "SINE", local)).bind(lambda sine: math(f"{key}_across", "MULTIPLY", rho, sine)),
                )
            )
            .bind(lambda parts: node(f"{key}_triangle", bpy.types.ShaderNodeCombineXYZ, {"X": parts[0], "Y": parts[1], "Z": 0.0}, {}))
            .bind(lambda triangle: vector(f"{key}_edge", "LENGTH", ended(f"{key}_triangle", "Vector")(triangle)))
            .bind(
                lambda edge: paired(
                    math(f"{key}_polygon", "SUBTRACT", 1.0, roundness).bind(lambda polygon: math(f"{key}_straight", "MULTIPLY", polygon, edge)), math(f"{key}_round", "MULTIPLY", radius, roundness)
                )
            )
            .bind(lambda parts: math(f"{key}_bound", "ADD", *parts))
            .bind(
                lambda bound: paired(
                    math(f"{key}_outer", "SUBTRACT", rho, bound), math(f"{key}_obstructed", "MULTIPLY", obstruction, bound).bind(lambda inner: math(f"{key}_inner", "SUBTRACT", rho, inner))
                )
            )
        )

    return paired(paired(polar, sector), rho).bind(lambda parts: bounded(*parts[0], parts[1]))


def iris[R: Effect](key: Key, shell: Shell[R], *path: Key, shift: Operand = 0.0) -> BlenderResult[tuple[Fragment, End]]:
    """Builds the native Bokeh Image from the record's aperture panel.

    Args:
        key: Identity of the Bokeh Image node.
        shell: Group source and native field endpoints.
        *path: Aperture panel's path from the record root.
        shift: Native bokeh color shift.

    Returns:
        The bokeh image and graph, or native construction faults.
    """
    blades, rotation, roundness, obstruction = Aperture.__struct_fields__
    return node(
        key,
        bpy.types.CompositorNodeBokehImage,
        {
            "Flaps": shell.socket(*path, blades),
            "Angle": shell.socket(*path, rotation),
            "Roundness": shell.socket(*path, roundness),
            "Catadioptric Size": shell.socket(*path, obstruction),
            "Color Shift": shift,
        },
        {},
    ).map(ended(key, "Image"))


def focused(shell: Shell[Defocus]) -> BlenderResult[tuple[Fragment, End]]:
    """Builds depth, elliptical, and tilted radius fields into one bokeh gather.

    Args:
        shell: Source image, physical frame measures, focus cases, and field sockets.

    Returns:
        The gathered image and graph, or accumulated native construction faults.
    """
    inlet = shell.socket
    focus, aperture, radius_field, fringe = Defocus.__struct_fields__
    cap = inlet(radius_field)

    def depth(at: FrameSpace, lens: Fragment) -> BlenderResult[tuple[Fragment, End]]:
        (f_stop,) = DepthOfField.__struct_fields__
        focal_length, sensor, distance = ((lens, End(node="lens", socket=socket)) for socket in ("Focal Length", "Sensor", "Focus Distance"))
        ratio = (
            node("sensor", bpy.types.ShaderNodeSeparateXYZ, {"Vector": sensor}, {})
            .bind(lambda sides: math("sensor_span", "MAXIMUM", ended("sensor", "X")(sides), End(node="sensor", socket="Y")))
            .bind(lambda span: math("lens_ratio", "DIVIDE", focal_length, span))
        )
        factor = (
            math("focus_ratio", "DIVIDE", distance, shell.layer("Depth"))
            .bind(lambda quotient: math("focus_offset", "SUBTRACT", quotient, 1.0))
            .bind(lambda offset: math("defocus", "ABSOLUTE", offset))
        )

        focal = math("millimeter", "DIVIDE", 0.001, shell.meters).bind(lambda millimeter: math("focal", "MULTIPLY", focal_length, millimeter))
        gap = focal.bind(lambda focal: math("focus_gap", "SUBTRACT", distance, focal)).bind(lambda gap: math("focus_reach", "ABSOLUTE", gap))
        coefficient = paired(focal.bind(lambda focal: math("half_focal", "MULTIPLY", focal, 0.5)), gap.bind(lambda gap: math("opening", "MULTIPLY", inlet(focus, DepthOfField, f_stop), gap))).bind(
            lambda parts: math("coefficient", "DIVIDE", *parts)
        )
        singular = gap.bind(lambda gap: node("singular", bpy.types.FunctionNodeCompare, {"A": gap, "B": 0.0, "Epsilon": 0.0}, {"data_type": "FLOAT", "operation": "EQUAL"})).map(
            ended("singular", "Result")
        )
        infinity = paired(ratio, coefficient).bind(lambda parts: math("infinity", "MULTIPLY", *parts))
        radius = paired(infinity, factor).bind(lambda parts: math("confusion", "MULTIPLY", *parts)).bind(lambda confusion: math("capped", "MINIMUM", confusion, cap))
        projected = (
            paired(radius, singular)
            .bind(lambda parts: switched("finite", "FLOAT", parts[1], parts[0], cap))
            .bind(lambda finite: switched("projected", "FLOAT", (lens, End(node="lens", socket="Is Orthographic")), finite, 0.0))
        )
        spread = paired(projected, pixels("window", at, cap)).bind(lambda parts: node("spread", bpy.types.CompositorNodeBlur, {"Image": parts[0], "Size": parts[1]}, {})).map(ended("spread", "Image"))
        return paired(spread, projected).bind(lambda parts: math("eroded", "MINIMUM", *parts))

    def built(at: FrameSpace) -> BlenderResult[tuple[Fragment, End]]:
        center, angle, size_field, feather = FieldCurvature.__struct_fields__
        center_band, angle_band, width, feather_band = TiltShift.__struct_fields__
        size = inlet(focus, FieldCurvature, size_field)
        curved = (
            paired(
                node("ellipse_axes", bpy.types.ShaderNodeSeparateXYZ, {"Vector": size}, {}).map(ended("ellipse_axes", "Y")),
                vector("ellipse_anchor", "MULTIPLY", inlet(focus, FieldCurvature, center), at.extent)
                .bind(lambda anchor: vector("ellipse_offset", "SUBTRACT", at.point, anchor))
                .bind(
                    lambda offset: node(
                        "ellipse_rotation", bpy.types.ShaderNodeVectorRotate, {"Vector": offset, "Angle": inlet(focus, FieldCurvature, angle)}, {"rotation_type": "Z_AXIS", "invert": True}
                    )
                )
                .bind(lambda rotation: vector("ellipse_normalized", "DIVIDE", ended("ellipse_rotation", "Vector")(rotation), size))
                .bind(lambda normalized: vector("ellipse_radius", "LENGTH", normalized))
                .bind(lambda radius: math("ellipse_edge", "SUBTRACT", radius, 1.0))
                .bind(lambda edge: math("ellipse_outside", "MAXIMUM", edge, 0.0)),
            )
            .bind(lambda parts: math("ellipse_excess", "MULTIPLY", *parts))
            .bind(lambda excess: math("ellipse_ramp", "DIVIDE", excess, inlet(focus, FieldCurvature, feather), clamp=True))
            .bind(lambda ramp: math("curvature", "MULTIPLY", ramp, ramp))
            .bind(lambda curvature: math("curved", "MULTIPLY", curvature, cap))
        )
        tilted = (
            paired(
                vector("band_anchor", "MULTIPLY", inlet(focus, TiltShift, center_band), at.extent).bind(lambda anchor: vector("band_offset", "SUBTRACT", at.point, anchor)),
                vector("band_phase", "MULTIPLY_ADD", inlet(focus, TiltShift, angle_band), (1.0, 1.0, 0.0), (pi / 2.0, 0.0, 0.0)).bind(lambda phase: vector("band_normal", "COSINE", phase)),
            )
            .bind(lambda parts: vector("band_across", "DOT_PRODUCT", *parts))
            .bind(lambda across: math("band_distance", "ABSOLUTE", across))
            .bind(lambda distance: math("band_edge", "MULTIPLY_ADD", inlet(focus, TiltShift, width), -0.5, distance))
            .bind(lambda edge: math("band_ramp", "DIVIDE", edge, inlet(focus, TiltShift, feather_band), clamp=True))
            .bind(lambda ramp: math("tilted", "MULTIPLY", ramp, cap))
        )
        optical = lensed().bind(lambda lens: depth(at, lens))
        radius = (
            accumulated((optical if isinstance(case, DepthOfField) else curved if isinstance(case, FieldCurvature) else tilted).map(lambda field: (case.title, field)) for case in shell.cases[focus,])
            .bind(lambda fields: menu("select", "FLOAT", inlet(focus), shell.items[focus,], dict(fields)))
            .bind(lambda select: pixels("size", at, select))
        )
        return (
            paired(radius, iris("iris", shell, aperture, shift=inlet(fringe)))
            .bind(lambda parts: node("gather", bpy.types.CompositorNodeBokehBlur, {"Image": shell.source, "Bokeh": parts[1], "Size": parts[0]}, {}))
            .map(ended("gather", "Image"))
        )

    return framed("frame", shell.source, shell.aspect, shell.scale).bind(built)


def matched(call: Call[Named]) -> BlenderResult[Outcome]:
    """Copies scene-camera aperture settings into a named defocus entry."""

    def converted(camera: bpy.types.Camera, held: Entry[Defocus]) -> BlenderResult[Entry[Defocus]]:
        (f_stop_field,) = msgspec.structs.fields(DepthOfField)
        dof: bpy.types.CameraDOFSettings = camera.dof  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        aperture = held.effect.aperture
        opening = (
            msgspec.structs.replace(aperture, rotation=dof.aperture_rotation, roundness=1.0)
            if dof.aperture_blades == 0
            else msgspec.structs.replace(aperture, rotation=dof.aperture_rotation, blades=dof.aperture_blades, roundness=0.0)
        )
        return (
            catch(lambda: msgspec.convert(dof.aperture_fstop, f_stop_field.type), exception=msgspec.ValidationError)()
            .map_error(lambda error: (BlenderReadFault(request=repr(dof), error=error),))
            .map(lambda f_stop: msgspec.structs.replace(held, effect=msgspec.structs.replace(held.effect, focus=DepthOfField(f_stop=f_stop), aperture=opening)))
        )

    def written(scene: bpy.types.Scene) -> BlenderResult[Outcome]:
        camera = (
            Option
            .of_optional(scene.camera)
            .bind(lambda camera: Some(camera.data) if isinstance(camera.data, bpy.types.Camera) else Nothing)
            .to_result_with(lambda: (BlenderAbsentFault(owner=repr(scene), key="camera"),))
        )
        return paired(camera, defocus.named(call.identity, scene, call.record.name)).bind(
            lambda found: (
                defocus
                .read(call.identity, found[1])
                .bind(lambda held: converted(found[0], held))
                .bind(lambda record: defocus.write(call.identity, scene, found[1], record))
                .map(
                    lambda changed: (
                        Done(notes=(("INFO", f"{found[1].name} takes the f-stop and iris of {found[0].name}"),))
                        if changed
                        else Unchanged(reason=f"{found[1].name} already holds the f-stop and iris of {found[0].name}")
                    )
                )
            )
        )

    return call.root(bpy.types.Scene).bind(written)


# --- [COMPOSITION] ----------------------------------------------------------------------

defocus: CompositorEffect[Defocus] = CompositorEffect(
    record=Defocus,
    graph=focused,
    coverage=Coverage.RESULT,
    requested=Some(lambda _record, layer: PassRequest.depth(layer, precise=True)),
    cameras={"dof.use_dof": False},
    registrables=(
        Operator(key="match_camera_iris", title="Match Camera", description="Sets f-stop and iris from the scene camera", form=Named, run=matched),
        inspection(lambda: defocus, "defocus_radius", "View Defocus Radius", "Shows the radius gathered at each pixel", lambda: End(node="select", socket="Output")),
    ),
)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["defocus", "focused", "iris", "matched", "outlined"]
