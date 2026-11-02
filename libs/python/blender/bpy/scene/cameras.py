"""Camera framing, render camera requirements, and the camera display and multi-view sections."""

from functools import partial
from typing import Final

from bl_ui.properties_data_camera import CAMERA_PT_presets, DATA_PT_camera_display_composition_guides
from bl_ui.space_view3d import VIEW3D_MT_object_context_menu
import bpy
from bpy_extras.view3d_utils import location_3d_to_region_2d
from expression import Nothing, Ok, Option, Some, try_downcast
from expression.extra.result import catch
from mathutils import Vector
import numpy as np
import numpy.typing as npt

from blender.bpy.commands.calls import PopupKind, PopupTarget
from blender.bpy.data.rna.conditions import Setting
from blender.bpy.data.rna.descriptors import Descriptor, Value
from blender.bpy.data.rna.paths import RnaPath, Struct
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.identity import Identity
from blender.bpy.settings.attachments import Recovery
from blender.bpy.ui.drawing.canvas import Rect
from blender.bpy.ui.elements.menus import Append
from blender.bpy.ui.layout.members import Member
from blender.bpy.ui.layout.rows import Layout, Opens, Property
from blender.bpy.ui.layout.sections import Section
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


def camera_frame(scene: bpy.types.Scene, region: bpy.types.Region, view: bpy.types.RegionView3D, bounds: Rect, /) -> Option[Rect]:
    """Returns the region pixels the scene camera's frame covers inside `bounds`, `Nothing` without a scene camera, for a corner behind the view, or for a frame outside the bounds."""
    corners = tuple(
        Option.of_optional(location_3d_to_region_2d(region, view, camera.matrix_world @ corner))
        for camera in Option.of_optional(scene.camera).to_list()
        for lens in Option.of_optional(try_downcast(bpy.types.Camera, camera.data)).to_list()
        for corner in lens.view_frame(scene=scene)
    )
    return (
        Some(np.asarray(tuple(point.to_tuple() for corner in corners for point in corner.to_list()), np.float64).reshape(-1, 2))
        .filter(lambda points: 0 < len(points) == len(corners))
        .map(lambda points: (np.maximum(points.min(axis=0), (bounds.x, bounds.y)), np.minimum(points.max(axis=0), (bounds.x + bounds.width, bounds.y + bounds.height))))
        .map(lambda ends: Rect(x=float(ends[0][0]), y=float(ends[0][1]), width=float(ends[1][0] - ends[0][0]), height=float(ends[1][1] - ends[0][1])))
        .filter(lambda rect: rect.width > 0.0 and rect.height > 0.0)
    )


# --- [REQUIREMENTS]


def camera_type(*accepted: str) -> Setting:
    """Returns condition on scene camera type."""
    return _on_camera(bpy.types.Camera.bl_rna.properties["type"], *accepted)


def _on_camera(prop: bpy.types.Property, *accepted: Value) -> Setting:
    return Setting(descriptor=Descriptor(root=bpy.types.Scene, owner=("camera", "data"), prop=prop), accepted=accepted)


def required(values: Snapshot, scene: bpy.types.Scene, identity: Identity) -> Outcome[None]:
    """Applies requirements to render cameras, recording earliest prior and final values."""
    return Recovery.before_after(identity, _PRIORS).bind(
        lambda recovery: accumulated(recovery.recorded(camera, ((camera, values),), lambda ends: ends) for camera in render_cameras(scene)).map(lambda _: None)
    )


def released(identity: Identity) -> Outcome[None]:
    """Restores prior camera values unless later edits changed them."""
    return Recovery.before_after(identity, _PRIORS).bind(lambda recovery: accumulated(map(recovery.reverted, bpy.data.cameras)).map(lambda _: None))


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
    rows=(
        *map(_camera_row, ("lens", "lens_unit")),
        *_PASSEPARTOUT,
        _camera_row("show_safe_areas"),
        _camera_row("show_safe_center", Some("show_safe_areas")),
        Layout(rows=(_guides,), container=Some(lambda layout: layout.column(heading="Composition Guides"))),
        *map(_camera_row, ("show_name", "show_sensor", "show_limits", "show_mist")),
    ),
    header=(Opens(target=PopupTarget(CAMERA_PT_presets, PopupKind.PANEL)),),
    condition=Some(_SCENE_CAMERA),
)
camera_views: Final = Section(
    key="render_views",
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

__all__ = ["camera_display", "camera_frame", "camera_menu", "camera_type", "camera_views", "framed", "released", "render_cameras", "required", "widened"]
