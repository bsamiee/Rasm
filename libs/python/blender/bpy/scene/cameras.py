"""Camera framing, requirements, display rows, and render views."""

from collections import Counter
from collections.abc import Callable
from contextlib import ExitStack
from itertools import chain
from typing import Annotated, ClassVar, Final

from bl_ui.properties_data_camera import CAMERA_PT_presets
from bl_ui.space_view3d import VIEW3D_MT_object_context_menu
import bpy
from expression import Error, Nothing, Ok, Option, Some
from mathutils import Vector
import msgspec
import numpy as np
import numpy.typing as npt

from blender.bpy.data.ids.ownership import committed
from blender.bpy.data.ids.store import Handle
from blender.bpy.data.rna.conditions import AllOf, Setting
from blender.bpy.data.rna.descriptors import Descriptor, written
from blender.bpy.data.rna.snapshots import Endpoints, Snapshot
from blender.bpy.extension.identity import Identity, Name
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.faults import BlenderAbsentFault, BlenderCollisionFault, BlenderEditFault, BlenderResult
from blender.bpy.settings.attachments import Attachment, reconciling, Recovery
from blender.bpy.settings.history import Retention
from blender.bpy.ui.layout.rows import Caption, Member, Popover, Property
from blender.bpy.ui.layout.sections import Section, Single
from blender.bpy.ui.layout.values import Hosted, Store
from blender.bpy.ui.surfaces.appends import Append
from blender.records.faults import accumulated, DomainFault, paired
from blender.records.fields import Case, Finite, Gated

# --- [TYPES] ----------------------------------------------------------------------------

type ViewSet = Stereo | Multiview

# --- [CONSTANTS] ------------------------------------------------------------------------

_SCENE_CAMERA: Final = Setting(descriptor=Descriptor.declared(bpy.types.Scene, "camera.type", bpy.types.EnumProperty), accepted=("CAMERA",))
_CAMERA_VIEW: Final = Setting(descriptor=Descriptor.declared(bpy.types.RegionView3D, "view_perspective", bpy.types.EnumProperty), accepted=("CAMERA",))
_MULTIVIEW: Final = Setting(descriptor=Descriptor.declared(bpy.types.Scene, "render.use_multiview", bpy.types.BoolProperty))
_VIEW_NAME: Final = Descriptor.declared(bpy.types.SceneRenderView, "name", bpy.types.StringProperty)

# --- [MODELS] ---------------------------------------------------------------------------


class View(msgspec.Struct, frozen=True, kw_only=True):
    """Render view with camera and image file suffix."""

    name: Annotated[str, msgspec.Meta(min_length=1, title="View", description="Render view name")]
    suffix: Annotated[str, msgspec.Meta(description="Camera name and image file suffix")]
    use: Annotated[bool, msgspec.Meta(title="Render", description="Render view")] = True


class Stereo(Case, frozen=True, kw_only=True):
    """Left and right camera stereo views."""

    title = "Stereo 3D"


class Multiview(Case, frozen=True, kw_only=True):
    """Named views reading cameras by suffix."""

    title = "Multi-View"
    views: Annotated[tuple[View, ...], msgspec.Meta(description="Views rendered beside left and right")] = ()


class RenderViews(msgspec.Struct, frozen=True, kw_only=True):
    """Scene render views, one view rendering while disabled."""

    views: Annotated[Gated[ViewSet], msgspec.Meta(description="Views each frame renders")] = Gated(enabled=False, value=Stereo())


class CameraDisplay(msgspec.Struct, frozen=True, kw_only=True):
    """Scene camera display rows with context menu entry."""

    @property
    def passepartout(self) -> tuple[Property, Property]:
        """Passepartout switch gating alpha row."""
        return _camera_row("show_passepartout"), _camera_row("passepartout_alpha", Some("show_passepartout"))

    def acquisitions(self, _identity: Identity) -> BlenderResult[Acquisitions]:
        """Returns passepartout rows on object context menu in camera view."""
        return Ok((Append(target=VIEW3D_MT_object_context_menu, rows=self.passepartout, condition=Some(AllOf(conditions=(_CAMERA_VIEW, _SCENE_CAMERA)))),))

    def section(self) -> Section:
        """Returns display rows under sensor preset popover."""
        guides = tuple(prop.identifier for prop in bpy.types.Camera.bl_rna.properties if prop.identifier.startswith("show_composition_"))
        return Single(
            key="camera_display",
            title="Camera Display",
            rows=(
                *map(_camera_row, ("lens", "lens_unit")),
                *self.passepartout,
                _camera_row("show_safe_areas"),
                _camera_row("show_safe_center", Some("show_safe_areas")),
                Caption(rows=tuple(map(_camera_row, (*guides, "composition_guide_color"))), heading=Some("Composition Guides"), separator=Nothing),
                *map(_camera_row, ("show_name", "show_sensor", "show_limits", "show_mist")),
            ),
            header=(Popover(panel=CAMERA_PT_presets),),
            condition=Some(_SCENE_CAMERA),
        )


class CameraViews(msgspec.Struct, frozen=True, kw_only=True):
    """Render view attachment and stereo rows of current scene."""

    retention: Option[Callable[[Identity], BlenderResult[Retention]]] = Nothing

    @staticmethod
    def synced(scene: Handle[bpy.types.Scene], identity: Identity) -> BlenderResult[Option[str]]:
        """Reconciles scene render views to stored views, naming no undo step."""
        return scene.resolve().bind(lambda held: CameraViews.attachment.read(identity, held).bind(lambda views: reconciled(views, held))).map(lambda _: Nothing)

    attachment: ClassVar[Attachment[RenderViews]] = Attachment(key="render_views", form=RenderViews, owner=bpy.types.Scene, changed=Some(reconciling(bpy.types.Scene, synced)))

    def acquisitions(self, _identity: Identity) -> BlenderResult[Acquisitions]:
        """Returns unbound attachment for registration scope."""
        return Ok((self.attachment,))

    def section(self) -> Section:
        """Returns attached views and camera stereo rows."""
        stereo = tuple(f"stereo.{prop.identifier}" for prop in bpy.types.CameraStereoData.bl_rna.properties if not prop.is_hidden)
        return Single(
            key="render_views",
            title="Render Views",
            rows=(Caption(rows=tuple(map(_camera_row, stereo)), heading=Some("Stereo"), separator=Nothing, condition=Some(_MULTIVIEW)),),
            store=Some(Hosted(store=Store(attachment=self.attachment, retention=self.retention))),
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
            return Some(Snapshot.of(bpy.types.Camera, {"ortho_scale": camera.ortho_scale * factor}))
        case "PERSP":
            return Some(Snapshot.of(bpy.types.Camera, {"sensor_height": camera.sensor_height * factor} if camera.sensor_fit == "VERTICAL" else {"sensor_width": camera.sensor_width * factor}))
        case "PANO" | "CUSTOM":
            return Nothing


def framed(item: bpy.types.Object, coordinates: npt.NDArray[np.float32], depsgraph: bpy.types.Depsgraph) -> BlenderResult[None]:
    """Fits camera object to two or more finite world coordinates."""
    match item.data:
        case bpy.types.Camera() as camera:
            try:
                points = msgspec.convert(coordinates.tolist(), Annotated[tuple[tuple[Finite, Finite, Finite], ...], msgspec.Meta(min_length=2)])
            except msgspec.ValidationError as error:
                return Error((DomainFault(subject=f"{item!r} framing", value=coordinates, accepted=str(error)),))

            def fitted(fit: tuple[Vector, float]) -> BlenderResult[None]:
                location, scale = fit

                def placed() -> None:
                    matrix = item.matrix_world.copy()
                    matrix.translation = location
                    item.matrix_world = matrix

                return paired(BlenderEditFault.attempted(f"{item!r}.matrix_world", placed), written(camera, "ortho_scale", scale) if camera.type == "ORTHO" else Ok(None)).map(lambda _: None)

            return BlenderEditFault.attempted(repr(item), lambda: item.camera_fit_coords(depsgraph, tuple(chain.from_iterable(points)))).bind(fitted)
        case _:
            return Error((BlenderAbsentFault(owner=repr(item), key=bpy.types.Camera.__name__),))


# --- [REQUIREMENTS]


def camera_type(*accepted: str) -> Setting:
    """Returns condition on scene camera type."""
    return Setting(descriptor=Descriptor(root=bpy.types.Scene, owner="camera.data", prop=bpy.types.Camera.bl_rna.properties["type"]), accepted=accepted)


def required(values: Snapshot, scene: bpy.types.Scene, identity: Identity) -> BlenderResult[None]:
    """Applies requirements to render cameras, recording earliest prior and final values."""
    return _priors(identity).bind(
        lambda recovery: accumulated(recovery.recorded(camera, ((camera, values),), lambda ends: ends) for camera in dict.fromkeys(render_cameras(scene).values())).map(lambda _: None)
    )


def released(identity: Identity) -> BlenderResult[None]:
    """Restores prior camera values unless later edits changed them."""
    return _priors(identity).bind(lambda recovery: accumulated(map(recovery.reverted, bpy.data.cameras)).map(lambda _: None))


def _priors(identity: Identity) -> BlenderResult[Recovery[Endpoints]]:
    """Returns camera recovery of requirement endpoints."""
    return identity.name(Name.ATTACHMENT, "camera_prior").map(Recovery.endpoints)


# --- [VIEWS]


def reconciled(views: RenderViews, scene: bpy.types.Scene) -> BlenderResult[None]:
    """Writes view settings and named views, accumulating independent faults."""
    render = scene.render
    match views.views.value:
        case Multiview(views=named):
            form = "MULTIVIEW"
        case Stereo():
            form, named = "STEREO_3D", ()
    stock = View(name="left", suffix="_L"), View(name="right", suffix="_R")
    kept = frozenset(view.name for view in (*stock, *named))
    collisions = tuple(BlenderCollisionFault(request=repr(render.views), error=name) for name, count in Counter(view.name for view in named).items() if count > 1)

    def placed(view: View) -> BlenderResult[None]:
        def made() -> bpy.types.SceneRenderView:
            item = render.views.new(view.name)
            rollback.callback(render.views.remove, item)
            return item

        return (
            Option
            .of_optional(render.views.get(view.name))
            .map(Ok)
            .default_with(lambda: BlenderEditFault.attempted(repr(render.views), made))
            .bind(lambda item: paired(written(item, "file_suffix", view.suffix), written(item, "use", view.use)))
            .map(lambda _: None)
        )

    def removed(view: bpy.types.SceneRenderView) -> BlenderResult[None]:
        return BlenderEditFault.attempted(repr(render.views), lambda: render.views.remove(view))

    missing = tuple(view for view in stock if render.views.get(view.name) is None)
    with ExitStack() as rollback:
        return committed(
            paired(accumulated(Error((fault,)) for fault in collisions), accumulated(Descriptor.validated(_VIEW_NAME.prop, view.name) for view in (*missing, *named)))
            .bind(lambda _: paired(written(render, "use_multiview", views.views.enabled), written(render, "views_format", form)))
            .bind(lambda _: accumulated(map(placed, (*missing, *named))))
            .bind(lambda _: accumulated(map(removed, tuple(view for view in render.views if view.name not in kept)))),
            rollback,
        ).map(lambda _: None)


# --- [DISPLAY]


def _camera_row(path: str, switch: Option[str] = Nothing) -> Property:
    return Property(
        target=Member(
            root=bpy.types.Camera,
            path=path,
            reach=Some(
                lambda context: (
                    Option.of_optional(context.scene).bind(lambda scene: Option.of_optional(scene.camera)).bind(lambda item: Some(item.data) if isinstance(item.data, bpy.types.Camera) else Nothing)
                )
            ),
        ),
        condition=switch.map(lambda name: Setting(descriptor=Descriptor(root=bpy.types.Scene, owner="camera.data", prop=bpy.types.Camera.bl_rna.properties[name]))),
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["CameraDisplay", "CameraViews", "Multiview", "RenderViews", "Stereo", "View", "ViewSet", "camera_type", "framed", "reconciled", "released", "render_cameras", "required", "widened"]
