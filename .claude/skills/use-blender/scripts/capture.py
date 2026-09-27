# mypy: disable-error-code="import-not-found, union-attr, arg-type"
# ty: ignore[unresolved-import, unresolved-attribute, invalid-argument-type]
"""Write one framed view of the scene to `.artifacts/blender/<name>.png` without moving the user's view, run inside Blender through `runpy.run_path`."""

from collections.abc import Iterator
from contextlib import contextmanager, ExitStack
from enum import StrEnum
from pathlib import Path
from typing import Final, Self

import attrs
import bpy
from cattrs.preconf.json import make_converter
import gpu
from mathutils import Euler, Matrix, Vector
import numpy as np
from numpy.typing import NDArray
from OpenImageIO import ImageBuf, UINT8

# --- [TYPES] ----------------------------------------------------------------------------

type Corner = tuple[float, float, float]
type Outcome = Capture | UnknownObjects | UnknownView | HiddenInViewport | EmptyFrame | NoViewport | MissingCapture


class View(StrEnum):
    """Views a capture draws, each with its camera's XYZ Euler rotation in degrees as Blender's numpad views orient it, `USER` for the viewport's own view."""

    rotation: tuple[float, float, float] | None

    def __new__(cls, value: str, rotation: tuple[float, float, float] | None = None) -> Self:
        """Member holding its name as the value and its camera rotation."""
        member = str.__new__(cls, value)
        member._value_ = value
        member.rotation = rotation
        return member

    ISO = "iso", (60.0, 0.0, 45.0)
    TOP = "top", (0.0, 0.0, 0.0)
    BOTTOM = "bottom", (180.0, 0.0, 0.0)
    FRONT = "front", (90.0, 0.0, 0.0)
    BACK = "back", (90.0, 0.0, 180.0)
    RIGHT = "right", (90.0, 0.0, 90.0)
    LEFT = "left", (90.0, 0.0, -90.0)
    USER = "user"


# --- [CONSTANTS] ------------------------------------------------------------------------

JSON: Final = make_converter()

# --- [MODELS] ---------------------------------------------------------------------------


@attrs.frozen
class Difference:
    """Pixels that differ from the earlier capture beyond Blender's render-test threshold, marked red in the diff file, `outside` true when geometry left the earlier frame."""

    since: str
    differing: int
    diff: str
    outside: bool


@attrs.frozen
class Capture:
    """Written file, the world box an axis or iso view framed as lower and upper corners, `None` for the user's view, and the comparison when `since` named an earlier capture."""

    path: str
    frame: tuple[Corner, Corner] | None
    view: View
    comparison: Difference | None


@attrs.frozen
class Viewport:
    """3D Viewport space with one of its view regions and that region's view."""

    space: bpy.types.SpaceView3D
    region: bpy.types.Region
    view: bpy.types.RegionView3D


# --- [ERRORS] ---------------------------------------------------------------------------


@attrs.frozen
class UnknownObjects:
    """Object names absent from the scene."""

    names: tuple[str, ...]


@attrs.frozen
class UnknownView:
    """View name outside the views a capture draws."""

    view: str
    views: tuple[View, ...]


@attrs.frozen
class HiddenInViewport:
    """Named objects the drawing viewport does not show, hidden or outside its local view."""

    names: tuple[str, ...]


@attrs.frozen
class EmptyFrame:
    """Objects considered for the frame, none with evaluated geometry or instances."""

    objects: tuple[str, ...]


@attrs.frozen
class NoViewport:
    """Window with no 3D Viewport for the user's view."""

    view: View


@attrs.frozen
class MissingCapture:
    """Capture name with no file under `.artifacts/blender/`."""

    name: str


# --- [OPERATIONS] -----------------------------------------------------------------------


def capture(name: str, objects: tuple[str, ...] = (), view: str | None = None, since: str | None = None) -> Outcome:
    """Frame the named objects, or every visible one, from a view and write the image, the user's view drawn as it stands, `since` redrawing an earlier capture's frame and view and counting the pixels that differ."""
    limit_x, limit_y, margin, threshold = 1280, 720, 1.05, 0.016
    frame_key, view_key = "capture:frame", "capture:view"
    path = next(p for p in Path(__file__).resolve().parents if (p / ".git").exists()) / ".artifacts" / "blender" / f"{name}.png"
    source = path.with_stem(since) if since is not None else None
    match source:
        case None:
            earlier, stored, remembered = None, None, View.ISO
        case Path() if not source.exists():
            return MissingCapture(source.stem)
        case Path():
            image = ImageBuf(str(source))
            text = image.spec().getattribute(frame_key)
            earlier, stored, remembered = (
                (source.stem, np.asarray(image.get_pixels(UINT8)[..., :3], dtype=np.uint8)),
                None if text is None else JSON.loads(text, tuple[Corner, Corner]),
                image.spec().getattribute(view_key),
            )
    if (requested := view if view is not None else remembered) not in View:
        return UnknownView(requested, tuple(View))
    chosen, scene, layer = View(requested), bpy.context.scene, bpy.context.view_layer
    if missing := tuple(n for n in objects if n not in scene.objects):
        return UnknownObjects(missing)
    window = bpy.data.window_managers[0].windows[0] if bpy.app.background else bpy.context.window
    viewport = max(
        (
            Viewport(shown, region, data)
            for area in (window.screen.areas if window else ())
            if isinstance(shown := area.spaces.active, bpy.types.SpaceView3D) and (data := shown.region_3d) is not None
            for region in area.regions
            if region.type == "WINDOW"
        ),
        key=lambda found: found.region.width * found.region.height,
        default=None,
    )
    live, space = (None if bpy.app.background else viewport), (None if viewport is None else viewport.space)
    if hidden := tuple(n for n in objects if not scene.objects[n].visible_get(viewport=space)):
        return HiddenInViewport(hidden)
    depsgraph = bpy.context.evaluated_depsgraph_get()
    named = frozenset(objects)
    corners = np.array(
        [
            instance.matrix_world @ Vector(c)
            for instance in depsgraph.object_instances
            if (body := instance.object) is not None and (holder := instance.parent if instance.is_instance else body) is not None and (owner := holder.original) is not None
            if (owner.name in named if named else owner.visible_get(viewport=space)) and body.bound_box[0][:] != body.bound_box[6][:]
            for c in body.bound_box
        ],
        dtype=np.float64,
    ).reshape(-1, 3)
    match earlier, chosen, viewport:
        case (_, previous), _, _:
            height, width = previous.shape[:2]
        case None, View.USER, Viewport(region=region):
            width, height = region.width, region.height
        case _:
            width, height = limit_x, limit_y
    scale = min(limit_x / width, limit_y / height)
    resolution_x, resolution_y = round(width * scale), round(height * scale)

    @contextmanager
    def assigned(*changes: tuple[bpy.types.bpy_struct, str, object]) -> Iterator[None]:
        """Each attribute set for the scope and restored in order after it."""
        saved = [(owner, key, getattr(owner, key)) for owner, key, _ in changes]
        try:
            for owner, key, value in changes:
                setattr(owner, key, value)
            yield
        finally:
            for owner, key, value in saved:
                setattr(owner, key, value)

    @contextmanager
    def temporary() -> Iterator[tuple[bpy.types.Object, bpy.types.Camera]]:
        """Temporary camera in the scene at the capture's resolution, removed with its object after the scope."""
        camera = bpy.data.cameras.new(name)
        eye = bpy.data.objects.new(name, camera)
        with ExitStack() as stack:
            stack.callback(bpy.data.cameras.remove, camera)
            stack.callback(bpy.data.objects.remove, eye)
            stack.enter_context(assigned((scene.render, "resolution_x", resolution_x), (scene.render, "resolution_y", resolution_y), (scene.render, "resolution_percentage", 100)))
            scene.collection.objects.link(eye)
            yield eye, camera

    def framed(rotation: Euler, bounds: NDArray[np.float64]) -> NDArray[np.uint8]:
        """Pixels through a camera at `rotation` fit to the bounds grown by the margin, perspective for `ISO` and orthographic for an axis view, drawn in the live viewport or rendered in a background run."""
        center, radius, basis = bounds.mean(axis=0), float(np.linalg.norm(bounds[1] - bounds[0])) / 2, rotation.to_matrix()
        grown = np.stack(np.meshgrid(*(center + (bounds - center) * margin).T), axis=-1).reshape(-1, 3)
        with temporary() as (eye, camera):
            camera.clip_start, camera.clip_end = camera.clip_start * radius, camera.clip_end * radius
            if chosen is View.ISO:
                eye.matrix_world = basis.to_4x4()
                depsgraph.update()
                location, _ = eye.camera_fit_coords(depsgraph, grown.ravel().tolist())
            else:
                lower, upper = (seen := grown @ np.asarray(basis)).min(axis=0), seen.max(axis=0)
                camera.type, camera.ortho_scale = "ORTHO", float(max((upper[:2] - lower[:2]) / (resolution_x, resolution_y))) * max(resolution_x, resolution_y)
                location = basis @ Vector((*((lower[:2] + upper[:2]) / 2), upper[2] + radius))
            eye.matrix_world = Matrix.LocRotScale(location[:], rotation, None)
            depsgraph.update()
            return render(eye) if live is None else draw(live.space, live.region, eye.matrix_world.inverted(), eye.calc_matrix_camera(depsgraph, x=resolution_x, y=resolution_y))

    def viewed(found: Viewport) -> NDArray[np.uint8]:
        """Pixels of the viewport's own view, drawn live, or rendered in a background run through a camera built from the stored view and window matrices, an orthographic eye set back by the half clip range the viewport centers on it."""
        if live is not None:
            return draw(found.space, found.region, found.view.view_matrix, found.view.window_matrix)
        projection, inverse = found.view.window_matrix, found.view.view_matrix.inverted()
        with temporary() as (eye, camera):
            camera.sensor_fit, camera.clip_start, camera.clip_end = "HORIZONTAL", found.space.clip_start, found.space.clip_end
            if found.view.is_perspective:
                eye.matrix_world, camera.lens = inverse, projection[0][0] * camera.sensor_width / 2
                camera.shift_x, camera.shift_y = projection[0][2] / 2, projection[1][2] / 2 * resolution_y / resolution_x
            else:
                eye.matrix_world = inverse @ Matrix.Translation((0.0, 0.0, found.space.clip_end / 2))
                camera.type, camera.ortho_scale = "ORTHO", 2 / projection[0][0]
                camera.shift_x, camera.shift_y = -projection[0][3] / 2, -projection[1][3] / 2 * resolution_y / resolution_x
            depsgraph.update()
            return render(eye)

    def draw(space: bpy.types.SpaceView3D, region: bpy.types.Region, view_matrix: Matrix, projection: Matrix) -> NDArray[np.uint8]:
        """Offscreen draw through the viewport's shading with overlays and gizmos off, rows top down."""
        offscreen = gpu.types.GPUOffScreen(resolution_x, resolution_y)
        with ExitStack() as stack:
            stack.callback(offscreen.free)
            stack.enter_context(assigned((space.overlay, "show_overlays", False), (space, "show_gizmo", False)))
            offscreen.draw_view3d(scene, layer, space, region, view_matrix, projection, do_color_management=True)
            return np.array(offscreen.texture_color.read(), dtype=np.uint8).reshape(resolution_y, resolution_x, 4)[::-1, :, :3]

    def render(eye: bpy.types.Object) -> NDArray[np.uint8]:
        """Workbench render through the camera to the capture path with render visibility mirroring viewport visibility, in the `Standard` view at exposure 0 that Solid mode draws with, rows top down."""
        settings, output, view_settings = scene.render, scene.render.image_settings, scene.view_settings
        with assigned(
            *((block, "hide_render", block.hide_viewport) for block in (scene.collection, *scene.collection.children_recursive)),
            *((o, "hide_render", not o.visible_get(viewport=space)) for o in layer.objects),
            (scene, "camera", eye),
            (settings, "engine", "BLENDER_WORKBENCH"),
            (settings, "filepath", str(path)),
            (output, "media_type", "IMAGE"),
            (output, "file_format", "PNG"),
            (view_settings, "view_transform", "Standard"),
            (view_settings, "look", "None"),
            (view_settings, "exposure", 0.0),
        ):
            bpy.ops.render.render(write_still=True)
        return np.asarray(ImageBuf(str(path)).get_pixels(UINT8)[..., :3], dtype=np.uint8)

    def write(target: Path, pixels: NDArray[np.uint8]) -> None:
        """PNG of the pixels holding the view, and the frame when the view has one, as text the next `since` reads."""
        image = ImageBuf(np.ascontiguousarray(pixels))
        if frame is not None:
            image.specmod().attribute(frame_key, JSON.dumps(frame))
        image.specmod().attribute(view_key, chosen.value)
        if not image.write(str(target)):
            raise RuntimeError(image.geterror())

    path.parent.mkdir(parents=True, exist_ok=True)
    match chosen.rotation:
        case None if viewport is not None:
            frame, pixels = None, viewed(viewport)
        case None:
            return NoViewport(chosen)
        case _ if stored is None and not corners.size:
            return EmptyFrame(objects or tuple(o.name for o in scene.objects if o.visible_get(viewport=space)))
        case degrees:
            frame = stored if stored is not None else (tuple(corners.min(axis=0).tolist()), tuple(corners.max(axis=0).tolist()))
            pixels = framed(Euler(np.radians(degrees).tolist()), np.array(frame, dtype=np.float64))
    write(path, pixels)
    if earlier is None:
        return Capture(str(path), frame, chosen, None)
    label, previous = earlier
    mask = (np.abs(previous.astype(np.int16) - pixels) > threshold * 255).any(axis=-1)
    diff = path.with_stem(f"{name}-diff")
    write(diff, np.where(mask[..., None], np.array((255, 0, 0), dtype=np.uint8), previous // 2))
    outside = frame is not None and bool((np.clip(corners, *frame) != corners).any())
    return Capture(str(path), frame, chosen, Difference(label, int(mask.sum()), str(diff), outside))


def as_result(value: Outcome) -> dict[str, object]:
    """`result` dict for `execute_blender_code`, the case name under `kind`."""
    return {"kind": type(value).__name__, **attrs.asdict(value)}


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Capture", "Corner", "Difference", "EmptyFrame", "HiddenInViewport", "MissingCapture", "NoViewport", "Outcome", "UnknownObjects", "UnknownView", "View", "Viewport", "as_result", "capture"]
