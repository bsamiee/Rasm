"""Variable-radius bokeh gathered through the iris from thin-lens depth, an elliptical sharp region, or a tilted sharp band."""

from math import pi
from typing import Annotated

import bpy
from expression import Error, Nothing, Ok, Option, Some
import msgspec

from blender.bpy.commands.operators import Call, Done, Operator, Outcome, Unchanged
from blender.bpy.faults import BlenderAbsentFault, BlenderResult
from blender.bpy.nodes.graphs import End, Expr, math, vector
from blender.bpy.render.compositor import lenses
from blender.bpy.render.compositor.coordinates import pixels
from blender.bpy.render.compositor.effects import Alpha, CameraFrame, CompositorEffect, depth_precise, Holds, Inspects, Named, Registers, Requests, revised, Shell
from blender.bpy.render.compositor.shapes import turned
from blender.imaging.geometry.lenses import Aperture
from blender.imaging.geometry.regions import FrameExtent, FrameLength, Position
from blender.records.faults import DomainFault, paired
from blender.records.fields import Case, Positive, Presentation, Quantity

from ...chain.stages import Effect, Stage

# --- [MODELS] ---------------------------------------------------------------------------


class DepthOfField(Case, frozen=True, kw_only=True):
    """Thin-lens circle of confusion from camera and depth pass."""

    title = "Depth of Field"
    f_stop: Annotated[Positive, msgspec.Meta(title="F-Stop", description="Focal length over aperture diameter"), Presentation(soft=Some((0.1, 128.0)), precision=Some(1))] = 2.8


class FieldCurvature(Case, frozen=True, kw_only=True):
    """Defocus growing quadratically beyond a sharp ellipse."""

    title = "Radial"
    center: Annotated[Position, msgspec.Meta(description="Sharp ellipse center")] = (0.5, 0.5)
    angle: Annotated[float, msgspec.Meta(ge=-pi / 2.0, le=pi / 2.0, description="Ellipse turn from horizontal"), Presentation(quantity=Some(Quantity.ANGLE))] = 0.0
    size: Annotated[tuple[Positive, Positive], msgspec.Meta(description="Ellipse half extents along rotated axes"), Presentation(quantity=Some(Quantity.FRAME))] = (0.4, 0.4)
    feather: Annotated[FrameExtent, msgspec.Meta(description="Outward distance to full defocus")] = 0.2


class TiltShift(Case, frozen=True, kw_only=True):
    """Defocus growing linearly beyond a tilted sharp band."""

    title = "Tilt-Shift"
    center: Annotated[Position, msgspec.Meta(description="Point sharp band passes through")] = (0.5, 0.5)
    angle: Annotated[float, msgspec.Meta(ge=-pi / 2.0, le=pi / 2.0, description="Band turn from horizontal"), Presentation(quantity=Some(Quantity.ANGLE))] = 0.0
    width: Annotated[FrameLength, msgspec.Meta(description="Sharp band width")] = 0.16875
    feather: Annotated[FrameExtent, msgspec.Meta(description="Outward distance to full defocus")] = 0.140625


class Defocus(Effect, frozen=True, kw_only=True):
    """Variable-radius lens blur through the iris."""

    stage = Stage.DEFOCUS
    pointwise = False
    focus: Annotated[DepthOfField | FieldCurvature | TiltShift, msgspec.Meta(description="Bokeh size source")] = DepthOfField()
    aperture: Annotated[Aperture, msgspec.Meta(description="Bokeh iris outline")] = Aperture()
    radius: Annotated[FrameLength, msgspec.Meta(title="Max Blur", description="Largest bokeh radius"), Presentation(soft=Some((0.0, 0.05)))] = 0.02
    fringe: Annotated[float, msgspec.Meta(ge=-1.0, le=1.0, description="Bokeh rim color shift, toward blue above 0")] = 0.0


# --- [OPERATIONS] -----------------------------------------------------------------------


def focused(shell: Shell[Defocus]) -> BlenderResult[Expr]:
    """Gathers the stage input through the iris at the selected radius field."""
    inlet, at = shell.input, shell.space
    focus, aperture, radius_field, fringe = Defocus.__struct_fields__
    (f_stop,) = DepthOfField.__struct_fields__
    center, angle, size_field, feather = FieldCurvature.__struct_fields__
    band_center, band_angle, width, band_feather = TiltShift.__struct_fields__
    _, _, meters = CameraFrame.__struct_fields__
    cap = inlet(radius_field)
    projected = lenses.confusion(shell.layer("Depth"), inlet(focus, DepthOfField, f_stop), inlet(Shell.frame, meters), cap)
    spread = Expr(kind=bpy.types.CompositorNodeBlur, inputs=(("Image", projected), ("Size", pixels(at, cap))), socket="Image")
    size = inlet(focus, FieldCurvature, size_field)
    local = turned(at, inlet(focus, FieldCurvature, center), inlet(focus, FieldCurvature, angle))
    curvature = math("DIVIDE", size["Y"] * math("MAXIMUM", vector("LENGTH", local / size) - 1.0, 0.0), inlet(focus, FieldCurvature, feather), clamp=True)
    across = abs(turned(at, inlet(focus, TiltShift, band_center), inlet(focus, TiltShift, band_angle))["Y"])
    tilt = math("DIVIDE", math("MULTIPLY_ADD", inlet(focus, TiltShift, width), -0.5, across), inlet(focus, TiltShift, band_feather), clamp=True)
    fields: dict[type[Case], Expr] = {DepthOfField: math("MINIMUM", spread, projected), FieldCurvature: curvature * curvature * cap, TiltShift: tilt * cap}
    radius = shell.menu(focus, cases={type(case).title: fields[type(case)] for case in shell.cases[focus,]}, kind="FLOAT").named("select")
    bokeh = lenses.iris(lambda field: inlet(aperture, field.name), shift=inlet(fringe))
    return Ok(Expr(kind=bpy.types.CompositorNodeBokehBlur, inputs=(("Image", shell.color), ("Bokeh", bokeh), ("Size", pixels(at, radius))), socket="Image", shape="RGBA"))


def matched(call: Call[Named]) -> BlenderResult[Outcome]:
    """Copies the scene camera's f-stop and iris into the named entry."""
    (f_stop,) = msgspec.structs.fields(DepthOfField)

    def converted(dof: bpy.types.CameraDOFSettings, record: Defocus) -> BlenderResult[Defocus]:
        opening = (
            msgspec.structs.replace(record.aperture, rotation=dof.aperture_rotation, roundness=1.0)
            if dof.aperture_blades == 0
            else msgspec.structs.replace(record.aperture, rotation=dof.aperture_rotation, blades=dof.aperture_blades, roundness=0.0)
        )
        return (
            Ok(msgspec.structs.replace(record, focus=DepthOfField(f_stop=dof.aperture_fstop), aperture=opening))
            if dof.aperture_fstop > 0.0
            else Error((DomainFault(subject=f_stop.name, value=dof.aperture_fstop, accepted="positive f-numbers"),))
        )

    def written(scene: bpy.types.Scene) -> BlenderResult[Outcome]:
        camera: Option[tuple[str, bpy.types.CameraDOFSettings]] = Option.of_optional(scene.camera).bind(
            lambda held: Option.of_optional(held.data.dof).map(lambda dof: (held.name, dof)) if isinstance(held.data, bpy.types.Camera) else Nothing
        )
        return paired(camera.to_result_with(lambda: (BlenderAbsentFault(owner=repr(scene), key="camera"),)), defocus.named(call.identity, scene, call.record.name)).bind(
            lambda found: revised(defocus, call.identity, scene, found[1], lambda record: converted(found[0][1], record)).map(
                lambda changed: Done(notes=(("INFO", f"Copied f-stop and iris from {found[0][0]}"),)) if changed else Unchanged(reason=f"{found[1].name} matches {found[0][0]} f-stop and iris")
            )
        )

    return call.root(bpy.types.Scene).bind(written)


# --- [COMPOSITION] ----------------------------------------------------------------------

defocus = CompositorEffect(
    record=Defocus,
    graph=focused,
    alpha=Alpha.RESULT,
    parts=(
        Requests[Defocus](request=depth_precise),
        Holds(cameras={"dof.use_dof": False}),
        Inspects(key="defocus_radius", end=End(node="select", socket="Output"), description="Gather radius per pixel"),
        Registers(rows=(Operator(key="match_camera_iris", title="Match Camera", description="Set f-stop and iris from scene camera", icon=Some("CAMERA_DATA"), form=Named, run=matched),)),
    ),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Defocus", "DepthOfField", "FieldCurvature", "TiltShift", "defocus"]
