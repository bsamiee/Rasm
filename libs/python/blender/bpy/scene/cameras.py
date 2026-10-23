"""Camera framing, requirements, display rows, and render views."""

from collections import Counter
from collections.abc import Callable
from contextlib import ExitStack
from itertools import chain
from typing import Annotated, ClassVar

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
from blender.bpy.data.rna.snapshots import Endpoints, NativeValue, Persisted, Snapshot
from blender.bpy.extension.identity import Identity, Name
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.faults import BlenderAbsentFault, BlenderCollisionFault, BlenderEditFault, BlenderReadFault, BlenderResult
from blender.bpy.settings.attachments import Attachment, endpoints, reconciling, Stored
from blender.bpy.ui.layout.rows import Caption, Member, Popover, Property
from blender.bpy.ui.layout.sections import Hosted, Section, Single, Store
from blender.bpy.ui.surfaces.appends import Append
from blender.imaging.faults import accumulated, paired
from blender.imaging.parameters import Case, Finite, Gated
from blender.rendering.edits import merge, transfer, Tree
from blender.rendering.history import Retention

# --- [TYPES] ----------------------------------------------------------------------------

type ViewSet = Stereo | Multiview

# --- [MODELS] ---------------------------------------------------------------------------


class View(msgspec.Struct, frozen=True, kw_only=True):
    """Render view with camera and image file suffix."""

    name: Annotated[str, msgspec.Meta(min_length=1, title="View", description="Render view name")]
    suffix: Annotated[str, msgspec.Meta(description="Camera name and image file suffix")]
    use: Annotated[bool, msgspec.Meta(title="Render", description="Render view")] = True


class Stereo(Case, frozen=True, kw_only=True, tag=1):
    """Left and right camera stereo views."""

    title = "Stereo 3D"


class Multiview(Case, frozen=True, kw_only=True, tag=2):
    """Named views reading cameras by suffix."""

    title = "Multi-View"
    views: Annotated[tuple[View, ...], msgspec.Meta(description="Views rendered beside left and right")] = ()


class RenderViews(msgspec.Struct, frozen=True, kw_only=True):
    """Scene render views, one view rendering while disabled."""

    views: Annotated[Gated[ViewSet], msgspec.Meta(description="Views each frame renders")] = Gated(enabled=False, value=Stereo())


class CameraDisplay(msgspec.Struct, frozen=True, kw_only=True):
    """Scene camera display rows with context menu entry."""

    camera: ClassVar[Setting] = Setting(descriptor=Descriptor.declared(bpy.types.Scene, "camera.type", bpy.types.EnumProperty), accepted=("CAMERA",))

    @property
    def passepartout(self) -> tuple[Property, Property]:
        """Passepartout switch gating alpha row."""
        return _camera_row("show_passepartout"), _camera_row("passepartout_alpha", Some("show_passepartout"))

    def acquisitions(self, _identity: Identity) -> BlenderResult[Acquisitions]:
        """Returns passepartout rows on object context menu in camera view."""
        perspective = Descriptor(root=bpy.types.RegionView3D, owner="", prop=bpy.types.RegionView3D.bl_rna.properties["view_perspective"])
        return Ok((Append(target=VIEW3D_MT_object_context_menu, rows=self.passepartout, condition=Some(AllOf(conditions=(Setting(descriptor=perspective, accepted=("CAMERA",)), self.camera)))),))

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
            condition=Some(self.camera),
        )


class CameraViews(msgspec.Struct, frozen=True, kw_only=True):
    """Render view attachment and stereo rows of current scene."""

    retention: Option[Callable[[Identity], BlenderResult[Retention]]] = Nothing

    @staticmethod
    def synced(scene: Handle[bpy.types.Scene], identity: Identity) -> BlenderResult[Option[str]]:
        """Reconciles scene render views to stored views, naming no undo step."""
        return paired(scene.resolve(), CameraViews.attachment.bound(identity)).bind(lambda pair: pair[1].read(pair[0]).bind(lambda views: reconciled(views, pair[0]))).map(lambda _: Nothing)

    attachment: ClassVar[Attachment[RenderViews]] = Attachment(key="render_views", form=RenderViews, owner=bpy.types.Scene, changed=Some(reconciling(bpy.types.Scene, synced)))

    def acquisitions(self, _identity: Identity) -> BlenderResult[Acquisitions]:
        """Returns unbound attachment for registration scope."""
        return Ok((self.attachment,))

    def section(self) -> Section:
        """Returns attached views and camera stereo rows."""
        stereo = tuple(f"stereo.{prop.identifier}" for prop in bpy.types.CameraStereoData.bl_rna.properties if not prop.is_hidden)
        multiview = Setting(descriptor=Descriptor(root=bpy.types.Scene, owner="render", prop=bpy.types.RenderSettings.bl_rna.properties["use_multiview"]))
        return Single(
            key="render_views",
            title="Render Views",
            rows=(Caption(rows=tuple(map(_camera_row, stereo)), heading=Some("Stereo"), separator=Nothing, condition=Some(multiview)),),
            store=Some(Hosted(host=Member(root=bpy.types.Context, path="scene"), store=Store(attachment=self.attachment, retention=self.retention))),
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
                return Error((BlenderReadFault(request=repr(coordinates), error=error),))

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
    """Applies requirements to render cameras, storing earliest prior and final values."""

    def applied(stored: Stored[Endpoints]) -> BlenderResult[None]:
        def prepared(camera: bpy.types.Camera) -> BlenderResult[tuple[bpy.types.Camera, Persisted, Tree[NativeValue], Option[Tree[NativeValue]]]]:
            def rebased(
                current: Tree[NativeValue], prior: Option[tuple[Tree[NativeValue], Tree[NativeValue]]]
            ) -> BlenderResult[tuple[bpy.types.Camera, Persisted, Tree[NativeValue], Option[Tree[NativeValue]]]]:
                earliest = prior.map(lambda ends: Snapshot.capture(camera, prior=Some(ends[0]), native=True)).default_value(Ok(current))
                return earliest.bind(Snapshot.persisted).map(lambda before: (camera, before, current, prior.map(lambda ends: ends[1])))

            held = stored.read(camera).bind(lambda found: found.map(lambda ends: ends.decoded(camera, stored.name).map(Some)).default_value(Ok(Nothing)))
            return paired(Snapshot.capture(camera, native=True), held).bind(lambda pair: rebased(*pair))

        def changed(row: tuple[bpy.types.Camera, Persisted, Tree[NativeValue], Option[Tree[NativeValue]]]) -> BlenderResult[None]:
            camera, before, current, previous = row
            endpoint = (
                Snapshot
                .capture(camera, native=True)
                .map(lambda after: previous.map(lambda held: merge(merge(current, transfer(current, held)), transfer(current, after))).default_value(after))
                .bind(Snapshot.persisted)
                .bind(lambda after: stored.write(camera, Endpoints(before=before, after=after)))
            )
            return paired(values.apply(camera).outcome, endpoint).map(lambda _: None)

        return accumulated(prepared(camera) for camera in dict.fromkeys(render_cameras(scene).values())).bind(
            lambda rows: accumulated(stored.write(camera, Endpoints(before=before)) for camera, before, _, _ in rows).bind(lambda _: accumulated(changed(row) for row in rows)).map(lambda _: None)
        )

    return _priors(identity).bind(applied)


def released(identity: Identity) -> BlenderResult[None]:
    """Restores prior camera values unless later edits changed them."""

    def restored(stored: Stored[Endpoints]) -> BlenderResult[None]:
        def camera_back(camera: bpy.types.Camera) -> BlenderResult[None]:
            return stored.read(camera).bind(
                lambda held: held.map(
                    lambda ends: ends.decoded(camera, stored.name).bind(lambda trees: Snapshot.restore(*trees).outcome).bind(lambda _: stored.removed(camera)).map(lambda _: None)
                ).default_value(Ok(None))
            )

        return accumulated(camera_back(camera) for camera in bpy.data.cameras).map(lambda _: None)

    return _priors(identity).bind(restored)


def _priors(identity: Identity) -> BlenderResult[Stored[Endpoints]]:
    """Returns camera storage of requirement endpoints."""
    return identity.name(Name.ATTACHMENT, "camera_prior").map(endpoints)


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
            paired(
                accumulated(Error((fault,)) for fault in collisions),
                Descriptor.resolve(bpy.types.SceneRenderView, "name", bpy.types.StringProperty).bind(
                    lambda name: accumulated(Descriptor.validated(name.prop, view.name) for view in (*missing, *named))
                ),
            )
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
