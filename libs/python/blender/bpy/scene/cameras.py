"""Camera framing, render camera requirements, and the camera display and multi-view sections."""

from functools import partial
from typing import Final

from bl_ui.properties_data_camera import CAMERA_PT_presets, DATA_PT_camera_display_composition_guides
from bl_ui.space_view3d import VIEW3D_MT_object_context_menu
import bpy
from bpy_extras.view3d_utils import location_3d_to_region_2d
from expression import Option, Result, Some, try_downcast
from expression.extra.result import catch
from mathutils import Matrix, Vector
import numpy as np
import numpy.typing as npt

from blender.bpy.commands.operators import PopupKind, PopupTarget
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
from blender.records.faults import accumulated, Error, Fault, Nothing, Ok, Outcome, paired, Reason, typed

# --- [CONSTANTS] ------------------------------------------------------------------------

_SCENE_CAMERA: Final = Setting(descriptor=Descriptor(root=bpy.types.Scene, owner=("camera",), prop=bpy.types.Object.bl_rna.properties["type"]), accepted=("CAMERA",))
_CAMERA_VIEW: Final = Setting(descriptor=Descriptor(root=bpy.types.RegionView3D, prop=bpy.types.RegionView3D.bl_rna.properties["view_perspective"]), accepted=("CAMERA",))
_MULTIVIEW: Final = Setting(descriptor=Descriptor(root=bpy.types.Scene, owner=("render",), prop=bpy.types.RenderSettings.bl_rna.properties["use_multiview"]))
_PRIORS: Final = "camera_prior"

# --- [OPERATIONS] -----------------------------------------------------------------------
# --- [FRAMING]


def render_views(render: bpy.types.RenderSettings) -> tuple[bpy.types.SceneRenderView, ...]:
    """Returns enabled native views in render order, stereo views alone in stereo mode, empty when multiview is off."""
    return tuple(view for view in (render.views if render.views_format == "MULTIVIEW" else render.stereo_views) if view.use) if render.use_multiview else ()


def view_cameras(scene: bpy.types.Scene, cameras: tuple[bpy.types.Object, ...]) -> Outcome[dict[tuple[bpy.types.Object, bpy.types.SceneRenderView], bpy.types.Object]]:
    """Returns native cameras keyed by supplied camera and active view, refreshing searched layers once, or their native update faults."""
    if not cameras:
        return Ok({})
    views = render_views(scene.render)
    if scene.render.views_format != "MULTIVIEW" or not views:
        return Ok({(camera, view): camera for camera in cameras for view in views})
    suffix: bpy.types.StringProperty = bpy.types.SceneRenderView.bl_rna.properties["camera_suffix"]  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    capacity = suffix.length_max
    suffixes = tuple(view.camera_suffix for view in scene.render.views)
    requests = tuple(
        (camera, view, prefix.map(lambda prefix: (prefix + view.camera_suffix).encode()[: capacity - 1]))
        for camera in cameras
        for name in (camera.name,)
        for prefix in (Option.of_optional(max((suffix for suffix in suffixes if name.endswith(suffix)), key=len, default=None)).map(name.removesuffix),)
        for view in views
    )
    wanted = frozenset(key for _, _, request in requests for key in request.to_list() if key)
    found: dict[bytes, bpy.types.Object] = {}
    for layer in scene.view_layers if wanted else ():
        if wanted.issubset(found):
            break
        match catch(layer.update, exception=RuntimeError)().map_error(partial(Fault.of, Reason.REFUSED, repr(layer))):
            case Result(tag="ok", ok=None):
                for key, item in ((key, item) for item in layer.objects if (key := item.name.encode()) in wanted):
                    found.setdefault(key, item)
            case Result(error=faults):
                return Error(faults)
    return Ok({(camera, view): request.bind(lambda key: Option.of_optional(found.get(key))).default_value(camera) for camera, view, request in requests})


def render_cameras(scene: bpy.types.Scene) -> Outcome[tuple[bpy.types.Camera, ...]]:
    """Returns scene and frame-ordered marker camera data, their active view cameras included once each, or native layer update faults."""
    bases = tuple(dict.fromkeys(item for item in (scene.camera, *(marker.camera for marker in sorted(scene.timeline_markers, key=lambda marker: marker.frame))) if item is not None))
    return view_cameras(scene, bases).map(lambda views: tuple(dict.fromkeys(camera for item in (*bases, *views.values()) for camera in (item.data,) if isinstance(camera, bpy.types.Camera))))


def widened(camera: bpy.types.Camera, factor: float) -> Option[Snapshot]:
    """Returns a wider camera snapshot preserving field center and stereo convergence, `Nothing` for panoramic and custom cameras."""
    values: dict[RnaPath, Value]
    match camera.type:
        case "ORTHO":
            values = {("ortho_scale",): camera.ortho_scale * factor}
        case "PERSP":
            values = {("sensor_width",): camera.sensor_width * factor, **({("sensor_height",): camera.sensor_height * factor} if camera.sensor_fit == "VERTICAL" else {})}
        case "PANO" | "CUSTOM":
            return Nothing
    return Some(Snapshot.of(bpy.types.Camera, {**values, ("shift_x",): camera.shift_x / factor, ("shift_y",): camera.shift_y / factor}))


def framed(item: bpy.types.Object, coordinates: npt.NDArray[np.float32], depsgraph: bpy.types.Depsgraph) -> Outcome[None]:
    """Fits camera and clip planes to at least two finite world points, returning coordinate, native range, or host refusal faults."""
    matrix = item.matrix_world.copy()
    frame = matrix.normalized()
    clips: tuple[bpy.types.FloatProperty, ...] = tuple(bpy.types.Camera.bl_rna.properties[name] for name in ("clip_start", "clip_end"))  # type: ignore[misc]  # ty: ignore[invalid-assignment]

    def clipped(camera: bpy.types.Camera, points: tuple[Vector, ...], scale: float) -> Outcome[None]:
        try:
            item.matrix_world = matrix
            depsgraph.update()
            view = item.evaluated_get(depsgraph).matrix_world.normalized().inverted()
        except (RuntimeError, ValueError) as error:
            return Error(Fault.of(Reason.REFUSED, repr(item), error))
        depths = tuple(-(view @ point).z for point in points)
        span = min(depths), max(depths)
        ortho: bpy.types.FloatProperty = bpy.types.Camera.bl_rna.properties["ortho_scale"]  # type: ignore[assignment]  # ty: ignore[invalid-assignment]

        def wrote() -> None:
            camera.clip_start, camera.clip_end = (float(np.nextafter(np.float32(span[0]), np.float32(clips[0].hard_min))), float(np.nextafter(np.float32(span[1]), np.float32(clips[1].hard_max))))
            if camera.type == "ORTHO":
                camera.ortho_scale = scale

        return paired(
            Ok(span).filter_with(
                lambda values: all(clip.hard_min <= value <= clip.hard_max for clip, value in zip(clips, values)),
                lambda values: Fault.of(Reason.OUTSIDE, f"{item!r} clipping", values, member="native camera clip ranges"),
            ),
            Ok(scale).filter_with(lambda value: camera.type != "ORTHO" or value <= ortho.hard_max, lambda value: Fault.of(Reason.OUTSIDE, f"{item!r} framing", value, member=ortho.name)),
        ).bind(lambda _: catch(wrote, exception=RuntimeError)().map_error(partial(Fault.of, Reason.REFUSED, repr(item))))

    def shifted(camera: bpy.types.Camera, points: tuple[Vector, ...], scale: float, view: Matrix) -> Outcome[None]:
        axis = np.asarray(view, np.float64)[2, :3]
        backward = np.asarray(frame)[:3, 2].astype(np.float64)
        clearance = float(np.nextafter(np.float32(clips[0].hard_min), np.float32(np.inf)))
        if displacement := max(0.0, clearance - min(-(view @ point).z for point in points)) / float(axis @ backward):
            translated = np.asarray(matrix.translation, np.float64) + backward * displacement
            matrix.translation = Vector(np.where(axis == 0.0, translated, np.nextafter(translated.astype(np.float32), np.copysign(np.float32(np.inf), axis).astype(np.float32))))  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
        return clipped(camera, points, scale)

    def fitted(camera: bpy.types.Camera, points: npt.NDArray[np.float32], fit: tuple[Vector, float]) -> Outcome[None]:
        location, scale = fit
        matrix.translation = location
        return (
            catch(matrix.normalized().inverted, exception=ValueError)()
            .map_error(partial(Fault.of, Reason.REFUSED, repr(item)))
            .bind(lambda view: shifted(camera, tuple(Vector(point) for point in points), scale, view))
        )

    points = (
        catch(coordinates.reshape, exception=ValueError)(-1, 3)
        .map_error(partial(Fault.of, Reason.OUTSIDE, f"{item!r} framing", member="triples of world coordinates"))
        .filter_with(lambda points: len(points) > 1 and bool(np.isfinite(points).all()), lambda points: Fault.of(Reason.OUTSIDE, f"{item!r} framing", points, member="two or more finite points"))
    )
    return paired(typed(bpy.types.Camera, f"{item!r}.data", item.data), points).bind(
        lambda values: (
            catch(item.camera_fit_coords, exception=RuntimeError)(depsgraph, values[1].ravel().tolist()).map_error(partial(Fault.of, Reason.REFUSED, repr(item))).bind(lambda fit: fitted(*values, fit))
        )
    )


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
    return _on_camera(bpy.types.Camera.bl_rna.properties["type"], accepted=accepted)


def _on_camera(prop: bpy.types.Property, *, accepted: tuple[Value, ...]) -> Setting:
    return Setting(descriptor=Descriptor(root=bpy.types.Scene, owner=("camera", "data"), prop=prop), accepted=accepted)


def required(values: Snapshot, scene: bpy.types.Scene, identity: Identity) -> Outcome[None]:
    """Applies requirements to render cameras, recording earliest prior and final values."""
    recovery = Recovery.before_after(identity, _PRIORS)
    return render_cameras(scene).bind(lambda cameras: accumulated(recovery.recorded(camera, ((camera, values),), lambda ends: ends) for camera in cameras)).map(lambda _: None)


def released(identity: Identity) -> Outcome[None]:
    """Restores prior camera values unless later edits changed them."""
    return accumulated(map(Recovery.before_after(identity, _PRIORS).reverted, bpy.data.cameras)).map(lambda _: None)


# --- [DISPLAY]


def _camera(context: bpy.types.Context) -> Option[bpy.types.Camera]:
    return Option.of_optional(context.scene).bind(lambda scene: Option.of_optional(scene.camera)).bind(lambda item: Option.of_optional(try_downcast(bpy.types.Camera, item.data)))


def _camera_row(member: str, switch: Option[str] = Nothing, *, under: RnaPath = ()) -> Property:
    return Property(
        target=Member(root=bpy.types.Camera, path=(*under, member), reach=Some(_camera)), condition=switch.map(lambda name: _on_camera(bpy.types.Camera.bl_rna.properties[name], accepted=(True,)))
    )


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

__all__ = ["camera_display", "camera_frame", "camera_menu", "camera_type", "camera_views", "framed", "released", "render_cameras", "render_views", "required", "view_cameras", "widened"]
