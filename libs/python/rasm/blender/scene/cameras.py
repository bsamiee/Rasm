"""Render cameras, native framing, saved requirements, display rows, and render views."""

from collections import Counter
from collections.abc import Callable
from contextlib import ExitStack
from itertools import chain
from typing import Annotated, ClassVar

from bl_ui.properties_data_camera import CAMERA_PT_presets
from bl_ui.space_view3d import VIEW3D_MT_object_context_menu
import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
from expression.extra.result import catch
import msgspec
import numpy as np
import numpy.typing as npt

from rasm.blender.data.ids.store import Handle
from rasm.blender.data.rna.conditions import AllOf, Setting
from rasm.blender.data.rna.descriptors import Descriptor, written
from rasm.blender.data.rna.snapshots import Applied, NativeValue, Snapshot
from rasm.blender.extension.identity import Identity, Name
from rasm.blender.extension.registration import Acquisitions
from rasm.blender.faults import BlenderAbsentFault, BlenderCollisionFault, BlenderEditFault, BlenderFaults, BlenderReadFault, BlenderResult
from rasm.blender.nodes.graphs import reconciling
from rasm.blender.settings.attachments import Attachment, Stored
from rasm.blender.ui.layout.rows import Caption, Member, Popover, Property
from rasm.blender.ui.layout.sections import Hosted, Section, Single, Store
from rasm.blender.ui.surfaces.appends import Append
from rasm.imaging.faults import accumulated, paired
from rasm.imaging.parameters import Case, Finite, Gated
from rasm.rendering.edits import merge, transfer, Tree
from rasm.rendering.history import Retention

# --- [TYPES] ----------------------------------------------------------------------------

type ViewSet = Stereo | Multiview

# --- [MODELS] ---------------------------------------------------------------------------


class View(msgspec.Struct, frozen=True, kw_only=True):
    """Named render view and suffix of its camera and image files."""

    name: Annotated[str, msgspec.Meta(min_length=1, title="View", description="Render view name")]
    suffix: Annotated[str, msgspec.Meta(title="Suffix", description="Suffix of the camera name and image files")]
    use: Annotated[bool, msgspec.Meta(title="Render", description="Render this view")] = True


class Stereo(Case, frozen=True, kw_only=True, tag=1):
    """Left and right views from the camera's stereo settings."""

    title = "Stereo 3D"


class Multiview(Case, frozen=True, kw_only=True, tag=2):
    """Named views reading cameras by their suffixes."""

    title = "Multi-View"
    views: Annotated[tuple[View, ...], msgspec.Meta(title="Views", description="Views rendered beside left and right")] = ()


class RenderViews(msgspec.Struct, frozen=True, kw_only=True):
    """Scene's retained render views, rendering one view while disabled."""

    views: Annotated[Gated[ViewSet], msgspec.Meta(title="Views", description="Views each frame renders")] = Gated(enabled=False, value=Stereo())


class CameraDisplay(msgspec.Struct, frozen=True, kw_only=True):
    """Native scene-camera display controls and their context-menu entry."""

    @property
    def passepartout(self) -> tuple[Property, Property]:
        """Passepartout switch and its enabled alpha control."""
        return _camera_row("show_passepartout"), _camera_row("passepartout_alpha", Some("show_passepartout"))

    def acquisitions(self, identity: Identity) -> BlenderResult[Acquisitions]:
        """Returns the retained passepartout append for camera views."""
        perspective = Descriptor(root=bpy.types.RegionView3D, owner="", prop=bpy.types.RegionView3D.bl_rna.properties["view_perspective"])
        camera = Descriptor(root=bpy.types.Scene, owner="camera", prop=bpy.types.Object.bl_rna.properties["type"])
        return Ok((
            Append(
                target=VIEW3D_MT_object_context_menu,  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
                rows=self.passepartout,
                condition=Some(AllOf(conditions=(Setting(descriptor=perspective, accepted=("CAMERA",)), Setting(descriptor=camera, accepted=("CAMERA",))))),
            ),
        ))

    def section(self) -> Section:
        """Returns lens, safe-area, guide, and display rows under native sensor presets."""
        guides = tuple(prop.identifier for prop in bpy.types.Camera.bl_rna.properties if prop.identifier.startswith("show_composition_"))
        return Single(
            key="camera_display",
            title="Camera Display",
            rows=(
                *map(_camera_row, ("lens", "lens_unit")),
                *self.passepartout,
                _camera_row("show_safe_areas"),
                _camera_row("show_safe_center", Some("show_safe_areas")),
                Caption(rows=tuple(map(_camera_row, (*guides, "composition_guide_color"))), heading=Some("Composition Guides")),
                *map(_camera_row, ("show_name", "show_sensor", "show_limits", "show_mist")),
            ),
            header=(Popover(panel=CAMERA_PT_presets),),  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
            condition=Some(Setting(descriptor=Descriptor(root=bpy.types.Scene, owner="camera", prop=bpy.types.Object.bl_rna.properties["type"]), accepted=("CAMERA",))),
        )


class CameraViews(msgspec.Struct, frozen=True, kw_only=True):
    """Render-view attachment and native stereo controls over the current scene."""

    retention: Callable[[Identity], BlenderResult[Retention]]

    @staticmethod
    def synced(scene: Handle[bpy.types.Scene], identity: Identity) -> BlenderResult[Option[str]]:
        """Reconciles the scene's current attached views, returning no undo label."""
        return paired(scene.resolve(), CameraViews.attachment.bound(identity)).bind(lambda pair: pair[1].read(pair[0]).bind(lambda views: reconciled(views, pair[0]))).map(lambda _: Nothing)

    attachment: ClassVar[Attachment[RenderViews]] = Attachment(key="render_views", form=RenderViews, owner=bpy.types.Scene, changed=Some(reconciling(bpy.types.Scene, synced)))

    def acquisitions(self, identity: Identity) -> BlenderResult[Acquisitions]:
        """Returns the original attachment declaration for scoped construction."""
        return Ok((self.attachment,))

    def section(self) -> Section:
        """Returns the attached views and the camera's native stereo members."""
        stereo = tuple(f"stereo.{prop.identifier}" for prop in bpy.types.CameraStereoData.bl_rna.properties if not prop.is_hidden)
        multiview = Setting(descriptor=Descriptor(root=bpy.types.Scene, owner="render", prop=bpy.types.RenderSettings.bl_rna.properties["use_multiview"]))
        return Single(
            key="render_views",
            title="Render Views",
            rows=(Caption(rows=tuple(map(_camera_row, stereo)), heading=Some("Stereo"), condition=Some(multiview)),),
            store=Some(Hosted(host=Member(root=bpy.types.Context, path="scene"), store=Store(attachment=self.attachment, retention=self.retention))),
        )


# --- [OPERATIONS] -----------------------------------------------------------------------
# --- [FRAMING]


def render_cameras(scene: bpy.types.Scene) -> dict[bpy.types.Object, bpy.types.Camera]:
    """Returns native camera objects and their data in scene and marker order, once each."""
    return {
        item: camera
        for item in dict.fromkeys((scene.camera, *(marker.camera for marker in sorted(scene.timeline_markers, key=lambda marker: marker.frame))))
        if item is not None
        for camera in (item.data,)
        if isinstance(camera, bpy.types.Camera)
    }


def widened(camera: bpy.types.Camera, factor: float) -> Option[Snapshot]:
    """Returns the grown fit-axis snapshot, or absence for panoramic and custom cameras."""
    match camera.type:
        case "ORTHO":
            return Some(Snapshot.of(bpy.types.Camera, {"ortho_scale": camera.ortho_scale * factor}))
        case "PERSP":
            return Some(Snapshot.of(bpy.types.Camera, {"sensor_height": camera.sensor_height * factor} if camera.sensor_fit == "VERTICAL" else {"sensor_width": camera.sensor_width * factor}))
        case "PANO" | "CUSTOM":
            return Nothing


def framed(item: bpy.types.Object, coordinates: npt.NDArray[np.float32], depsgraph: bpy.types.Depsgraph) -> BlenderResult[None]:
    """Fits a native camera to world coordinates, returning absent or write refusals."""
    match item.data:
        case bpy.types.Camera() as camera:
            try:
                points = msgspec.convert(coordinates.tolist(), Annotated[tuple[tuple[Finite, Finite, Finite], ...], msgspec.Meta(min_length=2)])
            except msgspec.ValidationError as error:
                return Error((BlenderReadFault(request=repr(coordinates), error=error),))
            try:
                location, scale = item.camera_fit_coords(depsgraph, tuple(chain.from_iterable(points)))
            except RuntimeError as error:
                return Error((BlenderEditFault(request=repr(item), error=error),))
            matrix = item.matrix_world.copy()
            matrix.translation = location
            try:
                item.matrix_world = matrix
                placement = Result[None, BlenderFaults](tag="ok", ok=None)
            except (ReferenceError, RuntimeError, TypeError, ValueError) as error:
                placement = Result[None, BlenderFaults](tag="error", error=(BlenderEditFault(request=f"{item!r}.matrix_world", error=error),))
            return paired(placement, written(camera, "ortho_scale", scale) if camera.type == "ORTHO" else Ok(None)).map(lambda _: None)
        case _:
            return Error((BlenderAbsentFault(owner=repr(item), key=bpy.types.Camera.__name__),))


# --- [REQUIREMENTS]


def camera_type(*accepted: str) -> Setting:
    """Returns the condition accepting the scene camera's native type identifiers."""
    return Setting(descriptor=Descriptor(root=bpy.types.Scene, owner="camera.data", prop=bpy.types.Camera.bl_rna.properties["type"]), accepted=accepted)


def required(values: Snapshot, scene: bpy.types.Scene, identity: Identity) -> BlenderResult[tuple[Applied, ...]]:
    """Applies requirements with persistent earliest and actual final native values."""

    def applied(key: str) -> BlenderResult[tuple[Applied, ...]]:
        stored = Stored(name=key, decoder=msgspec.msgpack.Decoder(tuple[Tree[tuple[tuple[object, str], object]], Option[Tree[tuple[tuple[object, str], object]]]]))

        def prepared(camera: bpy.types.Camera) -> BlenderResult[tuple[bpy.types.Camera, Tree[tuple[tuple[object, str], object]], Tree[NativeValue], Option[Tree[NativeValue]]]]:
            prior = stored.read(camera).bind(
                lambda held: held.map(
                    lambda pair: (
                        Result[Tree[tuple[tuple[object, str], object]], BlenderFaults]
                        .bind(pair[1].to_result_with(lambda: (BlenderAbsentFault(owner=repr(camera), key=f"{key}.after"),)), lambda after: paired(Snapshot.decoded(pair[0]), Snapshot.decoded(after)))
                        .map(lambda endpoints: (Some(endpoints[0]), Some(endpoints[1])))
                    )
                ).default_value(Ok((Nothing, Nothing)))
            )
            return paired(Snapshot.capture(camera, native=True), prior).bind(
                lambda pair: (
                    pair[1][0]
                    .map(lambda before: Snapshot.capture(camera, prior=Some(before), native=True))
                    .default_value(Ok(pair[0]))
                    .bind(Snapshot.persisted)
                    .map(lambda before: (camera, before, pair[0], pair[1][1]))
                )
            )

        def changed(row: tuple[bpy.types.Camera, Tree[tuple[tuple[object, str], object]], Tree[NativeValue], Option[Tree[NativeValue]]]) -> BlenderResult[Applied]:
            camera, before, current, previous = row
            outcome = values.apply(camera)
            endpoint = (
                Snapshot
                .capture(camera, native=True)
                .map(lambda after: previous.map(lambda held: merge(merge(current, transfer(current, held)), transfer(current, after))).default_value(after))
                .bind(Snapshot.persisted)
                .bind(lambda after: stored.write(camera, (before, Some(after))))
            )
            return paired(outcome.outcome, endpoint).map(lambda _: outcome)

        return accumulated(prepared(camera) for camera in dict.fromkeys(render_cameras(scene).values())).bind(
            lambda rows: accumulated(stored.write(camera, (before, Nothing)) for camera, before, _, _ in rows).bind(lambda _: accumulated(changed(row) for row in rows))
        )

    return identity.name(Name.ATTACHMENT, "camera_prior").bind(applied)


def released(identity: Identity) -> BlenderResult[tuple[Applied, ...]]:
    """Restores requirements still equal to their actual final values, preserving edits."""

    def restored(key: str) -> BlenderResult[tuple[Applied, ...]]:
        stored = Stored(name=key, decoder=msgspec.msgpack.Decoder(tuple[Tree[tuple[tuple[object, str], object]], Option[Tree[tuple[tuple[object, str], object]]]]))

        def camera_back(camera: bpy.types.Camera) -> BlenderResult[tuple[Applied, ...]]:
            return stored.read(camera).bind(
                lambda held: held.map(
                    lambda pair: (
                        paired(
                            Snapshot.decoded(pair[0]),
                            Result[Tree[tuple[tuple[object, str], object]], BlenderFaults].bind(
                                pair[1].to_result_with(lambda: (BlenderAbsentFault(owner=repr(camera), key=f"{key}.after"),)), Snapshot.decoded
                            ),
                        )
                        .map(lambda endpoints: Snapshot.restore(*endpoints))
                        .bind(lambda applied: applied.outcome.bind(lambda _: stored.removed(camera)).map(lambda _: (applied,)))
                    )
                ).default_value(Ok(()))
            )

        return accumulated(camera_back(camera) for camera in bpy.data.cameras).map(lambda rows: tuple(chain.from_iterable(rows)))

    return identity.name(Name.ATTACHMENT, "camera_prior").bind(restored)


# --- [VIEWS]


def reconciled(views: RenderViews, scene: bpy.types.Scene) -> BlenderResult[None]:
    """Writes native view settings and named views, accumulating independent refusals."""
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
        if (item := render.views.get(view.name)) is None:
            try:
                item = render.views.new(view.name)
            except RuntimeError as error:
                return Error((BlenderEditFault(request=repr(render.views), error=error),))
            rollback.callback(render.views.remove, item)
        return paired(written(item, "file_suffix", view.suffix), written(item, "use", view.use)).map(lambda _: None)

    def removed(view: bpy.types.SceneRenderView) -> BlenderResult[None]:
        return catch(lambda: render.views.remove(view), exception=RuntimeError)().map_error(lambda error: (BlenderEditFault(request=repr(render.views), error=error),))

    def published(_: object) -> None:
        rollback.pop_all()

    missing = tuple(view for view in stock if render.views.get(view.name) is None)
    with ExitStack() as rollback:
        return (
            paired(
                accumulated(Error((fault,)) for fault in collisions),
                Descriptor.resolve(bpy.types.SceneRenderView, "name", bpy.types.StringProperty).bind(
                    lambda name: accumulated(Descriptor.admitted(name.prop, view.name) for view in (*missing, *named))
                ),
            )
            .bind(lambda _: paired(written(render, "use_multiview", views.views.enabled), written(render, "views_format", form)))
            .bind(lambda _: accumulated(map(placed, (*missing, *named))))
            .bind(lambda _: accumulated(map(removed, tuple(view for view in render.views if view.name not in kept))))
            .map(published)
        )


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
