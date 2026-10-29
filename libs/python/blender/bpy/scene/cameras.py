"""Camera framing, render camera requirements, and the camera display and multi-view sections."""

from functools import partial
from typing import Final

from bl_ui.properties_data_camera import CAMERA_PT_presets, DATA_PT_camera_display_composition_guides
from bl_ui.space_view3d import VIEW3D_MT_object_context_menu
import bpy
from expression import Nothing, Ok, Option, Some, try_downcast
from expression.extra.result import catch
from mathutils import Vector
import numpy as np
import numpy.typing as npt

from blender.bpy.commands.calls import Called, Opened
from blender.bpy.data.rna.conditions import Setting
from blender.bpy.data.rna.descriptors import Descriptor, Value
from blender.bpy.data.rna.paths import RnaPath, Struct
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.identity import Identity
from blender.bpy.settings.attachments import Recovery
from blender.bpy.ui.layout.rows import Layout, Member, Opens, Property
from blender.bpy.ui.layout.sections import Section
from blender.bpy.ui.surfaces.menus import Append
from blender.records.faults import accumulated, Fault, Outcome, paired, Reason, typed

# --- [CONSTANTS] ------------------------------------------------------------------------

_SCENE_CAMERA: Final = Setting(descriptor=Descriptor.declared(bpy.types.Scene, ("camera", "type"), bpy.types.EnumProperty), accepted=("CAMERA",))
_CAMERA_VIEW: Final = Setting(descriptor=Descriptor.declared(bpy.types.RegionView3D, ("view_perspective",), bpy.types.EnumProperty), accepted=("CAMERA",))
_MULTIVIEW: Final = Setting(descriptor=Descriptor.declared(bpy.types.Scene, ("render", "use_multiview"), bpy.types.BoolProperty))
_PRIORS: Final = "camera_prior"

# --- [OPERATIONS] -----------------------------------------------------------------------
# --- [FRAMING]


def render_cameras(scene: bpy.types.Scene) -> tuple[bpy.types.Camera, ...]:
    """Returns camera data of the scene camera and the marker cameras in frame order, once each."""
    return tuple(
        dict.fromkeys(
            camera
            for item in (scene.camera, *(marker.camera for marker in sorted(scene.timeline_markers, key=lambda marker: marker.frame)))
            if item is not None
            for camera in (item.data,)
            if isinstance(camera, bpy.types.Camera)
        )
    )


def widened(camera: bpy.types.Camera, factor: float) -> Option[Snapshot]:
    """Returns snapshot scaling fit axis, `Nothing` for panoramic and custom cameras."""
    match camera.type:
        case "ORTHO":
            return Some(Snapshot.of(bpy.types.Camera, {("ortho_scale",): camera.ortho_scale * factor}))
        case "PERSP":
            return Some(Snapshot.of(bpy.types.Camera, {("sensor_height",): camera.sensor_height * factor} if camera.sensor_fit == "VERTICAL" else {("sensor_width",): camera.sensor_width * factor}))
        case "PANO" | "CUSTOM":
            return Nothing


def framed(item: bpy.types.Object, coordinates: npt.NDArray[np.float32], depsgraph: bpy.types.Depsgraph) -> Outcome[None]:
    """Fits camera object to two or more finite world coordinates, moving it and scaling an orthographic camera, the host fitting fewer to the origin."""

    def fitted(camera: bpy.types.Camera, fit: tuple[Vector, float]) -> None:
        location, scale = fit
        matrix = item.matrix_world.copy()
        matrix.translation = location
        item.matrix_world = matrix
        if camera.type == "ORTHO":
            camera.ortho_scale = scale

    fit: Outcome[tuple[Vector, float]] = (
        Ok(coordinates.reshape(-1, 3))
        .filter_with(lambda points: len(points) > 1 and bool(np.isfinite(points).all()), lambda points: Fault.of(Reason.OUTSIDE, f"{item!r} framing", str(points), member="two or more finite points"))
        .bind(lambda points: catch(item.camera_fit_coords, exception=RuntimeError)(depsgraph, points.ravel().tolist()).map_error(partial(Fault.of, Reason.REFUSED, repr(item))))
    )
    return paired(typed(bpy.types.Camera, f"{item!r}.data", item.data), fit).map(lambda pair: fitted(*pair))


# --- [REQUIREMENTS]


def camera_type(*accepted: str) -> Setting:
    """Returns condition on scene camera type."""
    return _on_camera(bpy.types.Camera.bl_rna.properties["type"], *accepted)


def _on_camera(prop: bpy.types.Property, *accepted: Value) -> Setting:
    return Setting(descriptor=Descriptor(root=bpy.types.Scene, owner=("camera", "data"), prop=prop), accepted=accepted)


def required(values: Snapshot, scene: bpy.types.Scene, identity: Identity) -> Outcome[None]:
    """Applies requirements to render cameras, recording earliest prior and final values."""
    return Recovery.endpoints(identity, _PRIORS).bind(
        lambda recovery: accumulated(recovery.recorded(camera, ((camera, values),), lambda ends: ends) for camera in render_cameras(scene)).map(lambda _: None)
    )


def released(identity: Identity) -> Outcome[None]:
    """Restores prior camera values unless later edits changed them."""
    return Recovery.endpoints(identity, _PRIORS).bind(lambda recovery: accumulated(map(recovery.reverted, bpy.data.cameras)).map(lambda _: None))


# --- [DISPLAY]


def _camera(context: bpy.types.Context) -> Option[bpy.types.Camera]:
    return Option.of_optional(context.scene).bind(lambda scene: Option.of_optional(scene.camera)).bind(lambda item: Option.of_optional(try_downcast(bpy.types.Camera, item.data)))


def _camera_row(member: str, switch: Option[str] = Nothing, *, under: RnaPath = ()) -> Property:
    return Property(target=Member(root=bpy.types.Camera, path=(*under, member), reach=Some(_camera)), condition=switch.map(lambda name: _on_camera(bpy.types.Camera.bl_rna.properties[name], True)))


def _guides(layout: bpy.types.UILayout, _holder: Struct, context: bpy.types.Context) -> None:
    """Draws the scene camera's composition guides through the stock panel's drawing."""
    for camera in _camera(context).to_list():
        DATA_PT_camera_display_composition_guides.draw_panel(layout.column(), camera)


# --- [COMPOSITION] ----------------------------------------------------------------------

_PASSEPARTOUT: Final = (_camera_row("show_passepartout"), _camera_row("passepartout_alpha", Some("show_passepartout")))
camera_menu: Final = Append(target=VIEW3D_MT_object_context_menu, rows=_PASSEPARTOUT, condition=Some(_CAMERA_VIEW & _SCENE_CAMERA))
camera_display: Final = Section(
    key="camera_display",
    title="Camera Display",
    rows=(
        *map(_camera_row, ("lens", "lens_unit")),
        *_PASSEPARTOUT,
        _camera_row("show_safe_areas"),
        _camera_row("show_safe_center", Some("show_safe_areas")),
        Layout(rows=(_guides,), container=Some(lambda layout: layout.column(heading="Composition Guides"))),
        *map(_camera_row, ("show_name", "show_sensor", "show_limits", "show_mist")),
    ),
    header=(Opens(target=Opened(CAMERA_PT_presets, Called.PANEL)),),
    condition=Some(_SCENE_CAMERA),
)
camera_views: Final = Section(
    key="render_views",
    title="Render Views",
    rows=(
        Property(target=Member(root=bpy.types.Scene, path=("render", "use_multiview"))),
        Property(target=Member(root=bpy.types.Scene, path=("render", "views_format")), condition=Some(_MULTIVIEW)),
        Layout(
            rows=tuple(_camera_row(prop.identifier, under=("stereo",)) for prop in bpy.types.CameraStereoData.bl_rna.properties if not prop.is_hidden),
            container=Some(lambda layout: layout.column(heading="Stereo")),
            condition=Some(_MULTIVIEW),
        ),
    ),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["camera_display", "camera_menu", "camera_type", "camera_views", "framed", "released", "render_cameras", "required", "widened"]
