"""Camera framing, requirements, display rows, and multi-view rows."""

from functools import partial
from itertools import chain
from typing import Annotated, Final

from bl_ui.properties_data_camera import CAMERA_PT_presets, DATA_PT_camera_display_composition_guides
from bl_ui.space_view3d import VIEW3D_MT_object_context_menu
import bpy
from expression import Nothing, Ok, Option, Result, Some, try_downcast
from expression.extra.result import catch
from mathutils import Vector
import msgspec
import numpy as np
import numpy.typing as npt

from blender.bpy.commands.calls import Called, Opened
from blender.bpy.data.rna.conditions import Setting
from blender.bpy.data.rna.descriptors import Descriptor, written
from blender.bpy.data.rna.paths import RnaPath, Struct
from blender.bpy.data.rna.restoration import Endpoints
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.faults import BlenderReason
from blender.bpy.settings.attachments import Recovery
from blender.bpy.ui.layout.members import Member
from blender.bpy.ui.layout.rows import Layout, Opens, Property
from blender.bpy.ui.layout.sections import Section
from blender.bpy.ui.surfaces.menus import Append
from blender.records.faults import accumulated, Fault, Faults, Outcome, RecordReason, typed
from blender.records.fields import Finite

# --- [CONSTANTS] ------------------------------------------------------------------------

_SCENE_CAMERA: Final = Setting(descriptor=Descriptor.declared(bpy.types.Scene, ("camera", "type"), bpy.types.EnumProperty), accepted=("CAMERA",))
_CAMERA_VIEW: Final = Setting(descriptor=Descriptor.declared(bpy.types.RegionView3D, ("view_perspective",), bpy.types.EnumProperty), accepted=("CAMERA",))
_MULTIVIEW: Final = Setting(descriptor=Descriptor.declared(bpy.types.Scene, ("render", "use_multiview"), bpy.types.BoolProperty))

# --- [MODELS] ---------------------------------------------------------------------------


class CameraDisplay(msgspec.Struct, frozen=True, kw_only=True):
    """Scene camera display rows with context menu entry."""

    @property
    def passepartout(self) -> tuple[Property, Property]:
        """Passepartout switch gating alpha row."""
        return _camera_row("show_passepartout"), _camera_row("passepartout_alpha", Some("show_passepartout"))

    def acquisitions(self, _identity: Identity) -> Outcome[Acquisitions]:
        """Returns passepartout rows on object context menu in camera view."""
        return Ok((Append(target=VIEW3D_MT_object_context_menu, rows=self.passepartout, condition=Some(_CAMERA_VIEW & _SCENE_CAMERA)),))

    def section(self) -> Section:
        """Returns display rows under sensor preset popover, composition guides through the stock panel's drawing."""

        def guides(layout: bpy.types.UILayout, _holder: Struct, context: bpy.types.Context) -> None:
            for camera in _camera(context).to_list():
                DATA_PT_camera_display_composition_guides.draw_panel(layout.column(), camera)

        return Section(
            key="camera_display",
            title="Camera Display",
            rows=(
                *map(_camera_row, ("lens", "lens_unit")),
                *self.passepartout,
                _camera_row("show_safe_areas"),
                _camera_row("show_safe_center", Some("show_safe_areas")),
                Layout(rows=(guides,), container=Some(lambda layout: layout.column(heading="Composition Guides"))),
                *map(_camera_row, ("show_name", "show_sensor", "show_limits", "show_mist")),
            ),
            header=(Opens(target=Opened(CAMERA_PT_presets, Called.PANEL)),),
            condition=Some(_SCENE_CAMERA),
        )


# --- [OPERATIONS] -----------------------------------------------------------------------
# --- [FRAMING]


def render_cameras(scene: bpy.types.Scene) -> dict[bpy.types.Object, bpy.types.Camera]:
    """Returns camera objects and data in scene and marker order, once each."""
    return {
        item: camera
        for item in dict.fromkeys((scene.camera, *(marker.camera for marker in sorted(scene.timeline_markers, key=lambda marker: marker.frame))))
        if item is not None
        for camera in (item.data,)
        if isinstance(camera, bpy.types.Camera)
    }


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
    """Fits camera object to two or more finite world coordinates."""
    points: Outcome[tuple[tuple[float, float, float], ...]] = catch(msgspec.convert, exception=msgspec.ValidationError)(
        coordinates.tolist(), Annotated[tuple[tuple[Finite, Finite, Finite], ...], msgspec.Meta(min_length=2)]
    ).map_error(lambda error: Fault.of(RecordReason.OUTSIDE, f"{item!r} framing", str(coordinates), member=str(error)))

    def fitted(camera: bpy.types.Camera, fit: tuple[Vector, float]) -> Outcome[None]:
        location, scale = fit
        matrix = item.matrix_world.copy()
        matrix.translation = location
        item.matrix_world = matrix
        return written(camera, ("ortho_scale",), scale).map(lambda _: None) if camera.type == "ORTHO" else Ok(None)

    def fit(camera: bpy.types.Camera, held: tuple[tuple[float, float, float], ...]) -> Outcome[None]:
        return Result[tuple[Vector, float], Faults].bind(
            catch(item.camera_fit_coords, exception=RuntimeError)(depsgraph, tuple(chain.from_iterable(held))).map_error(partial(Fault.of, BlenderReason.REFUSED, repr(item))), partial(fitted, camera)
        )

    return typed(bpy.types.Camera, f"{item!r}.data", item.data).bind(lambda camera: Result[tuple[tuple[float, float, float], ...], Faults].bind(points, partial(fit, camera)))


# --- [REQUIREMENTS]


def camera_type(*accepted: str) -> Setting:
    """Returns condition on scene camera type."""
    return Setting(descriptor=Descriptor(root=bpy.types.Scene, owner=("camera", "data"), prop=bpy.types.Camera.bl_rna.properties["type"]), accepted=accepted)


def required(values: Snapshot, scene: bpy.types.Scene, identity: Identity) -> Outcome[None]:
    """Applies requirements to render cameras, recording earliest prior and final values."""
    return _priors(identity).bind(
        lambda recovery: accumulated(recovery.recorded(camera, ((camera, values),), lambda ends: ends) for camera in dict.fromkeys(render_cameras(scene).values())).map(lambda _: None)
    )


def released(identity: Identity) -> Outcome[None]:
    """Restores prior camera values unless later edits changed them."""
    return _priors(identity).bind(lambda recovery: accumulated(map(recovery.reverted, bpy.data.cameras)).map(lambda _: None))


def _priors(identity: Identity) -> Outcome[Recovery[Endpoints]]:
    """Returns camera recovery of requirement endpoints."""
    return Recovery.endpoints(identity, "camera_prior")


# --- [DISPLAY]


def _camera(context: bpy.types.Context) -> Option[bpy.types.Camera]:
    return Option.of_optional(context.scene).bind(lambda scene: Option.of_optional(scene.camera)).bind(lambda item: Option.of_optional(try_downcast(bpy.types.Camera, item.data)))


def _camera_row(member: str, switch: Option[str] = Nothing, *, under: RnaPath = ()) -> Property:
    return Property(
        target=Member(root=bpy.types.Camera, path=(*under, member), reach=Some(_camera)),
        condition=switch.map(lambda name: Setting(descriptor=Descriptor(root=bpy.types.Scene, owner=("camera", "data"), prop=bpy.types.Camera.bl_rna.properties[name]))),
    )


# --- [COMPOSITION] ----------------------------------------------------------------------

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

__all__ = ["CameraDisplay", "camera_type", "camera_views", "framed", "released", "render_cameras", "required", "widened"]
