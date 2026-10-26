"""Camera and spatial defocus fields, shared iris geometry, and bokeh gathering."""

from math import pi, tau

import bpy
from expression import Nothing, Ok, Option, Some
from expression.extra.result import catch
import msgspec

from blender.bpy.commands.operators import Call, Done, Operator, Outcome, Unchanged
from blender.bpy.extension.identity import Key
from blender.bpy.faults import BlenderAbsentFault, BlenderResult
from blender.bpy.nodes.graphs import End, Expr, math, Scalar, switched, vector, xyz
from blender.bpy.render.effects.coordinates import lensed, pixels
from blender.bpy.render.effects.rows import Alpha, CameraFrame, CompositorEffect, depth_precise, Inspected, Named, revised, Shell
from blender.imaging.chain import Effect
from blender.imaging.effects.optics.defocus import Aperture, Defocus, DepthOfField, FieldCurvature, TiltShift
from blender.imaging.faults import Access, ImagingFileFault, paired
from blender.imaging.parameters import Case

# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [IRIS]


def outlined(point: Scalar, radius: Scalar, blades: Scalar, rotation: Scalar, roundness: Scalar, obstruction: Scalar) -> tuple[Expr, Expr]:
    """Returns outer and obstructed inner iris boundary fields, positive outside each boundary.

    Args:
        point: Iris-centered XY position after squeeze and reflection.
        radius: Circumradius in position units.
        blades: Polygon side count.
        rotation: Clockwise turn of vertex from vertical.
        roundness: Blend from blade outline to circle.
        obstruction: Central mirror as a share of outer outline.

    Returns:
        Signed outer and inner offsets over one shared boundary.
    """
    axes = Expr(kind=bpy.types.ShaderNodeSeparateXYZ, inputs=(("Vector", point),))
    sector = math("DIVIDE", tau, blades)
    half, rho = sector * 0.5, vector("LENGTH", point)
    phase = math("FLOORED_MODULO", math("ADD", math("ARCTAN2", msgspec.structs.replace(axes, socket="Y"), msgspec.structs.replace(axes, socket="X")), rotation) - pi / 2.0, sector)
    edge = vector("LENGTH", xyz(math("MULTIPLY", radius, math("COSINE", half)), rho * math("SINE", phase - half)))
    bound = math("SUBTRACT", 1.0, roundness) * edge + math("MULTIPLY", radius, roundness)
    return rho - bound, rho - math("MULTIPLY", obstruction, bound)


def iris[R: Effect](shell: Shell[R], *path: Key, shift: Scalar = 0.0) -> Expr:
    """Builds Bokeh Image node from the aperture panel at `path`, with a bokeh color shift."""
    blades, rotation, roundness, obstruction = Aperture.__struct_fields__
    sockets = (("Flaps", blades), ("Angle", rotation), ("Roundness", roundness), ("Catadioptric Size", obstruction))
    return Expr(kind=bpy.types.CompositorNodeBokehImage, inputs=(*((socket, shell.input(*path, field)) for socket, field in sockets), ("Color Shift", shift)), socket="Image", shape="RGBA")


# --- [FIELDS]


def focused(shell: Shell[Defocus]) -> BlenderResult[Expr]:
    """Builds depth, elliptical, and tilted radius fields into one bokeh gather of the stage input."""
    inlet, at = shell.input, shell.space
    focus, aperture, radius_field, fringe = Defocus.__struct_fields__
    (f_stop,) = DepthOfField.__struct_fields__
    center, angle, size_field, feather = FieldCurvature.__struct_fields__
    band_center, band_angle, width, band_feather = TiltShift.__struct_fields__
    _, _, meters = CameraFrame.__struct_fields__
    cap = inlet(radius_field)
    focal_length, sensor, distance = (lensed(socket) for socket in ("Focal Length", "Sensor", "Focus Distance"))
    focal = focal_length * (0.001 / inlet(Shell.frame, meters))
    gap = abs(distance - focal)
    singular = Expr(kind=bpy.types.FunctionNodeCompare, inputs=(("A", gap), ("B", 0.0), ("Epsilon", 0.0)), members=(("data_type", "FLOAT"), ("operation", "EQUAL")), socket="Result")
    confusion = focal_length / math("MAXIMUM", sensor["X"], sensor["Y"]) * (focal * 0.5 / (inlet(focus, DepthOfField, f_stop) * gap)) * abs(math("DIVIDE", distance, shell.layer("Depth")) - 1.0)
    projected = switched(lensed("Is Orthographic"), switched(singular, math("MINIMUM", confusion, cap), cap), 0.0)
    spread = Expr(kind=bpy.types.CompositorNodeBlur, inputs=(("Image", projected), ("Size", pixels(at, cap))), socket="Image")
    size = inlet(focus, FieldCurvature, size_field)
    rotated = Expr(
        kind=bpy.types.ShaderNodeVectorRotate,
        inputs=(("Vector", at.point - inlet(focus, FieldCurvature, center) * at.extent), ("Angle", inlet(focus, FieldCurvature, angle))),
        members=(("rotation_type", "Z_AXIS"), ("invert", True)),
        socket="Vector",
        shape="VECTOR",
    )
    curvature = math("DIVIDE", size["Y"] * math("MAXIMUM", vector("LENGTH", rotated / size) - 1.0, 0.0), inlet(focus, FieldCurvature, feather), clamp=True)
    normal = vector("COSINE", vector("MULTIPLY_ADD", inlet(focus, TiltShift, band_angle), (1.0, 1.0, 0.0), (pi / 2.0, 0.0, 0.0)))
    across = abs(vector("DOT_PRODUCT", at.point - inlet(focus, TiltShift, band_center) * at.extent, normal))
    tilt = math("DIVIDE", math("MULTIPLY_ADD", inlet(focus, TiltShift, width), -0.5, across), inlet(focus, TiltShift, band_feather), clamp=True)
    fields: dict[type[Case], Expr] = {DepthOfField: math("MINIMUM", spread, projected), FieldCurvature: curvature * curvature * cap, TiltShift: tilt * cap}
    radius = shell.menu(focus, cases={case.title: fields[type(case)] for case in shell.cases[focus,]}, kind="FLOAT").named("select")
    return Ok(
        Expr(kind=bpy.types.CompositorNodeBokehBlur, inputs=(("Image", shell.color), ("Bokeh", iris(shell, aperture, shift=inlet(fringe))), ("Size", pixels(at, radius))), socket="Image", shape="RGBA")
    )


# --- [COMMANDS]


def matched(call: Call[Named]) -> BlenderResult[Outcome]:
    """Copies scene camera f-stop and iris into named entry."""

    def converted(camera: bpy.types.Camera, effect: Defocus) -> BlenderResult[Defocus]:
        (f_stop,) = msgspec.structs.fields(DepthOfField)
        dof: bpy.types.CameraDOFSettings = camera.dof  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        opening = (
            msgspec.structs.replace(effect.aperture, rotation=dof.aperture_rotation, roundness=1.0)
            if dof.aperture_blades == 0
            else msgspec.structs.replace(effect.aperture, rotation=dof.aperture_rotation, blades=dof.aperture_blades, roundness=0.0)
        )
        return (
            catch(lambda: msgspec.convert(dof.aperture_fstop, f_stop.type), exception=msgspec.ValidationError)()
            .map_error(lambda error: (ImagingFileFault(access=Access.READ, request=repr(dof), error=error),))
            .map(lambda value: msgspec.structs.replace(effect, focus=DepthOfField(f_stop=value), aperture=opening))
        )

    def written(scene: bpy.types.Scene) -> BlenderResult[Outcome]:
        camera = (
            Option
            .of_optional(scene.camera)
            .bind(lambda held: Some(held.data) if isinstance(held.data, bpy.types.Camera) else Nothing)
            .to_result_with(lambda: (BlenderAbsentFault(owner=repr(scene), key="camera"),))
        )
        return paired(camera, defocus.named(call.identity, scene, call.record.name)).bind(
            lambda found: revised(defocus, call.identity, scene, found[1], lambda effect: converted(found[0], effect)).map(
                lambda changed: Done(notes=(("INFO", f"Copied f-stop and iris from {found[0].name}"),)) if changed else Unchanged(reason=f"{found[1].name} matches {found[0].name} f-stop and iris")
            )
        )

    return call.root(bpy.types.Scene).bind(written)


# --- [COMPOSITION] ----------------------------------------------------------------------

defocus = CompositorEffect(
    record=Defocus,
    graph=focused,
    alpha=Alpha.RESULT,
    requested=Some(depth_precise),
    cameras={"dof.use_dof": False},
    inspected=(Inspected(key="defocus_radius", end=End(node="select", socket="Output"), description="Gather radius per pixel"),),
    registrables=(Operator(key="match_camera_iris", title="Match Camera", description="Set f-stop and iris from scene camera", icon=Some("CAMERA_DATA"), form=Named, run=matched),),
)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["defocus", "focused", "iris", "matched", "outlined"]
