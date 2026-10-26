"""Variable-radius bokeh gathered through the iris from thin-lens depth, an elliptical sharp region, or a tilted sharp band."""

from math import pi
from typing import Final

import bpy
from expression import Error, Nothing, Option, Some

from blender.bpy.commands.operators import Answer, Call, Done, Operator, Unchanged
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.faults import BlenderReason
from blender.bpy.nodes.graphs import Expr, math, vector
from blender.bpy.nodes.interface import Interface, InterfacePanel, item_key, MenuItem, MenuItems, Socket
from blender.bpy.render.compositor import lenses
from blender.bpy.render.compositor.coordinates import pixels
from blender.bpy.render.compositor.effects import Alpha, CompositorEffect, depth_precise, entered, Holds, Named, Registers, Requests, Shell
from blender.bpy.render.compositor.shapes import turned
from blender.records.faults import Fault, Outcome, paired, RecordReason

from ...chain.stages import Effect, Stage

# --- [CONSTANTS] ------------------------------------------------------------------------

IRIS: Final = InterfacePanel(
    key="iris",
    name="Iris",
    description="Bokeh iris outline",
    inputs=tuple(
        Socket(key=item_key("iris", field), name=bpy.path.display_name(field), kind=kind, description=description, members={"subtype": subtype, **numbers})
        for field, kind, subtype, description, numbers in (
            ("blades", bpy.types.NodeSocketInt, "NONE", "Blades outlining bokeh", {"min_value": 3, "max_value": 24, "default_value": 5}),
            ("rotation", bpy.types.NodeSocketFloat, "ANGLE", "Clockwise blade vertex turn from vertical", {"min_value": -pi, "max_value": pi}),
            ("roundness", bpy.types.NodeSocketFloat, "FACTOR", "Blade curvature, circular at 1", {"min_value": 0.0, "max_value": 1.0, "default_value": 1.0}),
            ("obstruction", bpy.types.NodeSocketFloat, "FACTOR", "Central mirror share of iris", {"min_value": 0.0, "max_value": 1.0}),
        )
    ),
)

FOCUS: Final = MenuItems(
    items=(
        MenuItem(name="Depth of Field", description="Thin-lens circle of confusion from camera and depth pass"),
        MenuItem(name="Radial", description="Defocus growing quadratically beyond a sharp ellipse"),
        MenuItem(name="Tilt-Shift", description="Defocus growing linearly beyond a tilted sharp band"),
    )
)

# --- [MODELS] ---------------------------------------------------------------------------


class Defocus(Effect, frozen=True, kw_only=True):
    """Variable-radius lens blur through the iris."""

    stage = Stage.DEFOCUS
    pointwise = False


# --- [OPERATIONS] -----------------------------------------------------------------------


def focused(shell: Shell[Defocus]) -> Outcome[Expr]:
    """Gathers the stage input through the iris at the selected radius field."""
    inlet, at = shell.input, shell.space
    cap = inlet("max_blur")
    projected = lenses.confusion(shell.layer("Depth"), inlet("depth_of_field_f_stop"), inlet(Shell.frame, "meters"), cap)
    spread = Expr(kind=bpy.types.CompositorNodeBlur, inputs=(("Image", projected), ("Size", pixels(at, cap))), socket="Image")
    size = inlet("radial_size")
    local = turned(at, inlet("radial_center"), inlet("radial_angle"))
    curvature = math("DIVIDE", size["Y"] * math("MAXIMUM", vector("LENGTH", local / size) - 1.0, 0.0), inlet("radial_feather"), clamp=True)
    across = abs(turned(at, inlet("tilt_shift_center"), inlet("tilt_shift_angle"))["Y"])
    tilt = math("DIVIDE", math("MULTIPLY_ADD", inlet("tilt_shift_width"), -0.5, across), inlet("tilt_shift_feather"), clamp=True)
    bokeh = lenses.iris(lambda field: inlet("iris", field.name), shift=inlet("fringe"))
    return shell.menu("focus", cases={"Depth of Field": math("MINIMUM", spread, projected), "Radial": curvature * curvature * cap, "Tilt-Shift": tilt * cap}, kind="FLOAT").map(
        lambda radius: Expr(kind=bpy.types.CompositorNodeBokehBlur, inputs=(("Image", shell.color), ("Bokeh", bokeh), ("Size", pixels(at, radius.named("select")))), socket="Image", shape="RGBA")
    )


def match_camera_iris(call: Call[Named]) -> Outcome[Answer]:
    """Set f-stop and iris from scene camera."""

    def copied(camera: tuple[str, bpy.types.CameraDOFSettings], entry: bpy.types.SceneCompositorEffect) -> Outcome[Answer]:
        name, dof = camera
        written = (
            entered(
                call.identity,
                entry,
                {"focus": "Depth of Field", "depth_of_field_f_stop": dof.aperture_fstop, "iris_rotation": dof.aperture_rotation, "iris_roundness": 1.0}
                if dof.aperture_blades == 0
                else {"focus": "Depth of Field", "depth_of_field_f_stop": dof.aperture_fstop, "iris_rotation": dof.aperture_rotation, "iris_blades": dof.aperture_blades, "iris_roundness": 0.0},
            )
            if dof.aperture_fstop > 0.0
            else Error((Fault(reason=RecordReason.OUTSIDE, subject="depth_of_field_f_stop", member="positive f-numbers", cause=str(dof.aperture_fstop)),))
        )
        return written.map(lambda changed: Done(notes=(("INFO", f"Copied f-stop and iris from {name}"),)) if changed else Unchanged(reason=f"{entry.name} matches {name} f-stop and iris"))

    def matched(scene: bpy.types.Scene) -> Outcome[Answer]:
        camera: Option[tuple[str, bpy.types.CameraDOFSettings]] = Option.of_optional(scene.camera).bind(
            lambda held: Option.of_optional(held.data.dof).map(lambda dof: (held.name, dof)) if isinstance(held.data, bpy.types.Camera) else Nothing
        )
        return paired(camera.to_result_with(lambda: (Fault(reason=BlenderReason.ABSENT, subject=repr(scene), member="camera"),)), defocus.named(call.identity, scene, call.properties.name)).bind(
            lambda found: copied(*found)
        )

    return call.root(bpy.types.Scene).bind(matched)


# --- [COMPOSITION] ----------------------------------------------------------------------

defocus = CompositorEffect(
    record=Defocus,
    graph=focused,
    interface=Interface(
        inputs=(
            Socket(key="focus", name="Focus", kind=bpy.types.NodeSocketMenu, description="Bokeh size source", members={"default_value": "Depth of Field"}, items=Some(FOCUS)),
            *(
                Socket(key=key, name=bpy.path.display_name(key), kind=bpy.types.NodeSocketFloat, description=description, members=numbers)
                for key, description, numbers in (
                    ("max_blur", "Largest bokeh radius", {"min_value": 0.0, "default_value": 0.02}),
                    ("fringe", "Bokeh rim color shift, toward blue above 0", {"min_value": -1.0, "max_value": 1.0}),
                )
            ),
        ),
        panels=(
            InterfacePanel(
                key="depth_of_field",
                name=FOCUS.items[0].name,
                description=FOCUS.items[0].description,
                inputs=(
                    Socket(
                        key="depth_of_field_f_stop", name="F-Stop", kind=bpy.types.NodeSocketFloat, description="Focal length over aperture diameter", members={"min_value": 0.0, "default_value": 2.8}
                    ),
                ),
            ),
            *(
                InterfacePanel(
                    key=prefix,
                    name=item.name,
                    description=item.description,
                    inputs=(
                        Socket(key=item_key(prefix, "center"), name="Center", kind=bpy.types.NodeSocketVector, description=center, members={"dimensions": 2, "default_value": (0.5, 0.5)}),
                        Socket(
                            key=item_key(prefix, "angle"), name="Angle", kind=bpy.types.NodeSocketFloat, description=angle, members={"subtype": "ANGLE", "min_value": -pi / 2.0, "max_value": pi / 2.0}
                        ),
                        extent,
                        Socket(
                            key=item_key(prefix, "feather"),
                            name="Feather",
                            kind=bpy.types.NodeSocketFloat,
                            description="Outward distance to full defocus",
                            members={"min_value": 0.0, "default_value": feather},
                        ),
                    ),
                )
                for prefix, item, center, angle, extent, feather in (
                    (
                        "radial",
                        FOCUS.items[1],
                        "Sharp ellipse center",
                        "Ellipse turn from horizontal",
                        Socket(
                            key="radial_size",
                            name="Size",
                            kind=bpy.types.NodeSocketVector,
                            description="Ellipse half extents along rotated axes",
                            members={"dimensions": 2, "min_value": 0.0, "default_value": (0.4, 0.4)},
                        ),
                        0.2,
                    ),
                    (
                        "tilt_shift",
                        FOCUS.items[2],
                        "Point sharp band passes through",
                        "Band turn from horizontal",
                        Socket(key="tilt_shift_width", name="Width", kind=bpy.types.NodeSocketFloat, description="Sharp band width", members={"min_value": 0.0, "default_value": 0.16875}),
                        0.140625,
                    ),
                )
            ),
            IRIS,
        ),
    ),
    alpha=Alpha.RESULT,
    parts=(
        Requests[Defocus](request=depth_precise),
        Holds(cameras=Snapshot.of(bpy.types.Camera, {"dof.use_dof": False})),
        Registers(rows=(Operator(form=Named, run=match_camera_iris, icon=Some("CAMERA_DATA")),)),
    ),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["IRIS", "Defocus", "defocus"]
