"""Variable-radius bokeh gathered through the iris from thin-lens depth, an elliptical sharp region, or a tilted sharp band."""

from functools import partial
from math import pi
from types import MappingProxyType
from typing import Final

import bpy
from expression import Error, Nothing, Ok, Option, Some

from blender.bpy.commands.operators import Answer, Call, Done, Operator, Unchanged
from blender.bpy.data.rna.descriptors import Value
from blender.bpy.data.rna.paths import rooted
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.identity import Key
from blender.bpy.nodes.desired import Interface, InterfacePanel, item_key, MenuItem, MenuItems, Socket
from blender.bpy.nodes.graphs import Expr, math, vector
from blender.bpy.render.compositor import lenses
from blender.bpy.render.compositor.effects import Alpha, Blended, CompositorEffect, depth_precise, Named
from blender.bpy.render.compositor.inputs import entered, FRAME_METERS, Holds, Shell
from blender.bpy.render.compositor.shapes import turned
from blender.imaging.geometry.lenses import Aperture
from blender.records.faults import Fault, Outcome, paired, Reason
from blender.records.fields import member

from ...chain.stages import Effect, Stage

# --- [CONSTANTS] ------------------------------------------------------------------------

IRIS_BLADES: Final = Socket(
    key=item_key("iris", member(Aperture, lambda kind: kind.blades)), name="Blades", kind=bpy.types.NodeSocketInt, description="Blades outlining bokeh", min_value=3, max_value=24, default_value=5
)
IRIS_ROTATION: Final = Socket(
    key=item_key("iris", member(Aperture, lambda kind: kind.rotation)),
    name="Rotation",
    kind=bpy.types.NodeSocketFloatAngle,
    description="Clockwise blade vertex turn from vertical",
    min_value=-pi,
    max_value=pi,
)
IRIS_ROUNDNESS: Final = Socket(
    key=item_key("iris", member(Aperture, lambda kind: kind.roundness)),
    name="Roundness",
    kind=bpy.types.NodeSocketFloatFactor,
    description="Blade curvature, circular at 1",
    min_value=0.0,
    max_value=1.0,
    default_value=1.0,
)
IRIS_OBSTRUCTION: Final = Socket(
    key=item_key("iris", member(Aperture, lambda kind: kind.obstruction)),
    name="Obstruction",
    kind=bpy.types.NodeSocketFloatFactor,
    description="Central mirror share of iris",
    min_value=0.0,
    max_value=1.0,
)
IRIS_INPUTS: Final = MappingProxyType({
    member(Aperture, lambda kind: kind.blades): IRIS_BLADES,
    member(Aperture, lambda kind: kind.rotation): IRIS_ROTATION,
    member(Aperture, lambda kind: kind.roundness): IRIS_ROUNDNESS,
    member(Aperture, lambda kind: kind.obstruction): IRIS_OBSTRUCTION,
})
IRIS: Final = InterfacePanel(key="iris", name="Iris", description="Bokeh iris outline", inputs=tuple(IRIS_INPUTS.values()))

_DEPTH_OF_FIELD: Final = MenuItem(name="Depth of Field", description="Thin-lens circle of confusion from camera and depth pass")
_RADIAL: Final = MenuItem(name="Radial", description="Defocus growing quadratically beyond a sharp ellipse")
_TILT_SHIFT: Final = MenuItem(name="Tilt-Shift", description="Defocus growing linearly beyond a tilted sharp band")
FOCUS: Final = MenuItems(items=(_DEPTH_OF_FIELD, _RADIAL, _TILT_SHIFT))
_FOCUS: Final = Socket(key="focus", kind=bpy.types.NodeSocketMenu, description="Bokeh size source", default_value=_DEPTH_OF_FIELD.name, items=Some(FOCUS))
_MAX_BLUR: Final = Socket(key="max_blur", kind=bpy.types.NodeSocketFloat, description="Largest bokeh radius", min_value=0.0, default_value=0.02)
_FRINGE: Final = Socket(key="fringe", kind=bpy.types.NodeSocketFloat, description="Bokeh rim color shift, toward blue above 0", min_value=-1.0, max_value=1.0)
_F_STOP: Final = Socket(key="depth_of_field_f_stop", name="F-Stop", kind=bpy.types.NodeSocketFloat, description="Focal length over aperture diameter", min_value=0.0, default_value=2.8)
_RADIAL_CENTER: Final = Socket(key="radial_center", name="Center", kind=bpy.types.NodeSocketVector2D, description="Sharp ellipse center", default_value=(0.5, 0.5))
_RADIAL_ANGLE: Final = Socket(key="radial_angle", name="Angle", kind=bpy.types.NodeSocketFloatAngle, description="Ellipse turn from horizontal", min_value=-pi / 2.0, max_value=pi / 2.0)
_RADIAL_SIZE: Final = Socket(key="radial_size", name="Size", kind=bpy.types.NodeSocketVector2D, description="Ellipse half extents along rotated axes", min_value=0.0, default_value=(0.4, 0.4))
_RADIAL_FEATHER: Final = Socket(key="radial_feather", name="Feather", kind=bpy.types.NodeSocketFloat, description="Outward distance to full defocus", min_value=0.0, default_value=0.2)
_TILT_SHIFT_CENTER: Final = Socket(key="tilt_shift_center", name="Center", kind=bpy.types.NodeSocketVector2D, description="Point sharp band passes through", default_value=(0.5, 0.5))
_TILT_SHIFT_ANGLE: Final = Socket(key="tilt_shift_angle", name="Angle", kind=bpy.types.NodeSocketFloatAngle, description="Band turn from horizontal", min_value=-pi / 2.0, max_value=pi / 2.0)
_TILT_SHIFT_WIDTH: Final = Socket(key="tilt_shift_width", name="Width", kind=bpy.types.NodeSocketFloat, description="Sharp band width", min_value=0.0, default_value=0.16875)
_TILT_SHIFT_FEATHER: Final = Socket(key="tilt_shift_feather", name="Feather", kind=bpy.types.NodeSocketFloat, description="Outward distance to full defocus", min_value=0.0, default_value=0.140625)

# --- [MODELS] ---------------------------------------------------------------------------


class Defocus(Effect, frozen=True, kw_only=True):
    """Variable-radius lens blur through the iris."""

    stage = Stage.DEFOCUS
    pointwise = False


# --- [OPERATIONS] -----------------------------------------------------------------------


def match_camera_iris(call: Call[Named]) -> Outcome[Answer]:
    """Set f-stop and iris from scene camera."""

    def copied(camera: tuple[str, bpy.types.CameraDOFSettings], entry: bpy.types.SceneCompositorEffect) -> Outcome[Answer]:
        name, dof = camera
        common: dict[Key, Value] = {_FOCUS.key: _DEPTH_OF_FIELD.name, _F_STOP.key: dof.aperture_fstop, IRIS_ROTATION.key: dof.aperture_rotation}
        written = (
            entered(call.identity, entry, common | ({IRIS_ROUNDNESS.key: 1.0} if dof.aperture_blades == 0 else {IRIS_BLADES.key: dof.aperture_blades, IRIS_ROUNDNESS.key: 0.0}))
            if dof.aperture_fstop > 0.0
            else Error(Fault.of(Reason.OUTSIDE, _F_STOP.key, str(dof.aperture_fstop), member="positive f-numbers"))
        )
        return written.map(lambda changed: Done(notes=(("INFO", f"Copied f-stop and iris from {name}"),)) if changed else Unchanged(reason=f"{entry.name} matches {name} f-stop and iris"))

    def matched(scene: bpy.types.Scene) -> Outcome[Answer]:
        camera: Option[tuple[str, bpy.types.CameraDOFSettings]] = Option.of_optional(scene.camera).bind(
            lambda held: Option.of_optional(held.data.dof).map(lambda dof: (held.name, dof)) if isinstance(held.data, bpy.types.Camera) else Nothing
        )
        return paired(camera.to_result_with(partial(Fault.of, Reason.ABSENT, repr(scene), member="camera")), defocus.named(call.identity, scene, call.properties.name)).bind(
            lambda found: copied(*found)
        )

    return rooted(bpy.types.Scene, call.context).bind(matched)


def defocused(shell: Shell[Defocus]) -> Outcome[Expr]:
    """Gathers the stage input through the iris at the selected radius field."""
    inlet, at = shell.input, shell.space
    cap = inlet(_MAX_BLUR)
    projected = lenses.confusion(shell.layer("Depth"), inlet(_F_STOP), inlet(FRAME_METERS), cap)
    spread = Expr(kind=bpy.types.CompositorNodeBlur, inputs=(("Image", projected), ("Size", at.pixels(cap))), socket="Image")
    size = inlet(_RADIAL_SIZE)
    local = turned(at, inlet(_RADIAL_CENTER), inlet(_RADIAL_ANGLE))
    curvature = math("DIVIDE", size["Y"] * math("MAXIMUM", vector("LENGTH", local / size) - 1.0, 0.0), inlet(_RADIAL_FEATHER), clamp=True)
    across = abs(turned(at, inlet(_TILT_SHIFT_CENTER), inlet(_TILT_SHIFT_ANGLE))["Y"])
    tilt = math("DIVIDE", math("MULTIPLY_ADD", inlet(_TILT_SHIFT_WIDTH), -0.5, across), inlet(_TILT_SHIFT_FEATHER), clamp=True)
    bokeh = lenses.iris(lambda field: inlet(IRIS_INPUTS[field]), shift=inlet(_FRINGE))
    radius = shell.menu(_FOCUS, cases={_DEPTH_OF_FIELD: math("MINIMUM", spread, projected), _RADIAL: curvature * curvature * cap, _TILT_SHIFT: tilt * cap})
    return Ok(
        Expr(kind=bpy.types.CompositorNodeBokehBlur, inputs=(("Image", shell.color), ("Bokeh", bokeh), ("Size", at.pixels(radius.named("select")))), socket="Image", shape=bpy.types.NodeSocketColor)
    )


# --- [COMPOSITION] ----------------------------------------------------------------------

defocus: Final = CompositorEffect(
    defocused,
    record=Defocus,
    interface=Interface(
        inputs=(_FOCUS, _MAX_BLUR, _FRINGE),
        panels=(
            InterfacePanel(key="depth_of_field", name=_DEPTH_OF_FIELD.name, description=_DEPTH_OF_FIELD.description, inputs=(_F_STOP,)),
            InterfacePanel(key="radial", name=_RADIAL.name, description=_RADIAL.description, inputs=(_RADIAL_CENTER, _RADIAL_ANGLE, _RADIAL_SIZE, _RADIAL_FEATHER)),
            InterfacePanel(key="tilt_shift", name=_TILT_SHIFT.name, description=_TILT_SHIFT.description, inputs=(_TILT_SHIFT_CENTER, _TILT_SHIFT_ANGLE, _TILT_SHIFT_WIDTH, _TILT_SHIFT_FEATHER)),
            IRIS,
        ),
    ),
    mix=Blended(alpha=Alpha.RESULT),
    request=Some(depth_precise),
    holds=Holds(cameras=Snapshot.of(bpy.types.Camera, {("dof", "use_dof"): False})),
    registers=(Operator(form=Named, run=match_camera_iris, icon=Some("CAMERA_DATA")),),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["FOCUS", "IRIS", "IRIS_BLADES", "IRIS_INPUTS", "IRIS_OBSTRUCTION", "IRIS_ROTATION", "IRIS_ROUNDNESS", "Defocus", "defocus"]
