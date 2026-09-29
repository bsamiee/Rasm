# mypy: disable-error-code="import-not-found, import-untyped, no-any-return, union-attr, arg-type"
# ty: ignore[unresolved-import, unresolved-attribute, invalid-argument-type]
"""Write one scene view to `.artifacts/blender/<name>.png` without moving the user's view."""

from collections.abc import Iterable, Iterator
from contextlib import contextmanager, ExitStack
from enum import StrEnum
from pathlib import Path
from typing import Self

import attrs
import bpy
import gpu
from mathutils import Euler, Matrix, Vector
import numpy as np
from numpy.typing import NDArray
from OpenImageIO import ImageBuf, UINT8
from PyOpenColorIO import GetCurrentConfig
from results import artifacts, JSON, unknown, UnknownObjects
from scene import bounds, Viewport, viewport

# --- [TYPES] ----------------------------------------------------------------------------


class View(StrEnum):
    """Views a capture draws, each with its camera's XYZ Euler rotation in degrees as Blender's numpad views orient it, `USER` for the viewport's own view."""

    rotation: tuple[float, float, float] | None

    def __new__(cls, value: str, rotation: tuple[float, float, float] | None = None) -> Self:
        """Member with its name as value and its camera rotation."""
        member = str.__new__(cls, value)
        member._value_ = value
        member.rotation = rotation
        return member

    ISO = "iso", (60, 0, 45)
    TOP = "top", (0, 0, 0)
    BOTTOM = "bottom", (180, 0, 0)
    FRONT = "front", (90, 0, 0)
    BACK = "back", (90, 0, 180)
    RIGHT = "right", (90, 0, 90)
    LEFT = "left", (90, 0, -90)
    USER = "user"


# --- [MODELS] ---------------------------------------------------------------------------


@attrs.frozen
class Difference:
    """Pixels differing from the earlier capture beyond Blender's render-test threshold, marked red in a diff file, `outside` true when geometry left the earlier frame."""

    since: str
    differing: int
    diff: str
    outside: bool


@attrs.frozen
class Capture:
    """Written file, world box an axis or iso view framed as lower and upper corners (`None` for the user's view), and comparison when `since` named an earlier capture."""

    path: str
    frame: tuple[tuple[float, float, float], tuple[float, float, float]] | None
    view: View
    comparison: Difference | None


# --- [ERRORS] ---------------------------------------------------------------------------


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


@attrs.frozen
class MissingCapture:
    """Capture name with no file under `.artifacts/blender/`."""

    name: str


# --- [OPERATIONS] -----------------------------------------------------------------------


def capture(
    name: str, objects: tuple[str, ...] = (), view: str | None = None, since: str | None = None
) -> Capture | UnknownObjects | UnknownView | HiddenInViewport | EmptyFrame | NoViewport | MissingCapture:
    """Frame the named or every visible object from a view and write the image, `since` redrawing an earlier capture's frame and view to count differing pixels."""
    limit_x, limit_y, margin, threshold = 1280, 720, 1.05, 0.016
    frame_key, view_key = "capture:frame", "capture:view"
    path = artifacts() / f"{name}.png"
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
                (source.stem, image.get_pixels(UINT8)),
                None if text is None else JSON.loads(text, tuple[tuple[float, float, float], tuple[float, float, float]]),
                image.spec().getattribute(view_key),
            )
    if (requested := view if view is not None else remembered) not in View:
        return UnknownView(requested, tuple(View))
    chosen, scene, layer = View(requested), bpy.context.scene, bpy.context.view_layer
    if (absent := unknown(scene.objects, objects)) is not None:
        return absent
    largest = viewport()
    live, space = (None if bpy.app.background else largest), (None if largest is None else largest.space)
    if hidden := tuple(n for n in objects if not scene.objects[n].visible_get(viewport=space)):
        return HiddenInViewport(hidden)
    depsgraph = bpy.context.evaluated_depsgraph_get()
    boxes = [box for owner, box in bounds(depsgraph).items() if (owner in objects if objects else scene.objects[owner].visible_get(viewport=space))]
    corners = np.array(boxes, dtype=np.float64).reshape(-1, 3)
    match earlier, chosen, largest:
        case (_, previous), _, _:
            height, width = previous.shape[:2]
        case None, View.USER, Viewport(region=region):
            width, height = region.width, region.height
        case _:
            width, height = limit_x, limit_y
    scale = min(limit_x / width, limit_y / height)
    resolution_x, resolution_y = round(width * scale), round(height * scale)

    @contextmanager
    def assigned(*changes: "tuple[bpy.types.bpy_struct[object], str, object]") -> Iterator[None]:
        """Attributes set for the scope and restored in order after it, including values a later row's update changed."""

        def put(rows: "Iterable[tuple[bpy.types.bpy_struct[object], str, object]]") -> None:
            """Set each attribute holding a value other than its row's."""
            for owner, key, value in rows:
                if getattr(owner, key) != value:
                    setattr(owner, key, value)

        saved = [(owner, key, getattr(owner, key)) for owner, key, _ in changes]
        try:
            put(changes)
            yield
        finally:
            put(saved)

    @contextmanager
    def temporary() -> Iterator[tuple[bpy.types.Object, bpy.types.Camera]]:
        """Camera object linked into the scene at the capture's resolution for the scope, removed with its data after it."""
        camera = bpy.data.cameras.new(name)
        eye = bpy.data.objects.new(name, camera)
        with ExitStack() as stack:
            stack.callback(bpy.data.cameras.remove, camera)
            stack.callback(bpy.data.objects.remove, eye)
            stack.enter_context(assigned((scene.render, "resolution_x", resolution_x), (scene.render, "resolution_y", resolution_y), (scene.render, "resolution_percentage", 100)))
            scene.collection.objects.link(eye)
            yield eye, camera

    def framed(rotation: Euler, box: NDArray[np.float64]) -> NDArray[np.uint8]:
        """Pixels through a camera at `rotation` fit to the box grown by the margin, drawn in the live viewport or rendered in a background run."""
        center, radius, basis = box.mean(axis=0), float(np.linalg.norm(box[1] - box[0])) / 2, rotation.to_matrix()
        grown = np.stack(np.meshgrid(*(center + (box - center) * margin).T), axis=-1).reshape(-1, 3)
        with temporary() as (eye, camera):
            eye.rotation_euler = rotation
            camera.clip_start, camera.clip_end = camera.clip_start * radius, camera.clip_end * radius
            if chosen is View.ISO:
                depsgraph.update()
                eye.location, _ = eye.camera_fit_coords(depsgraph, grown.ravel().tolist())
            else:
                lower, upper = (seen := grown @ np.asarray(basis)).min(axis=0), seen.max(axis=0)
                camera.type, camera.ortho_scale = "ORTHO", float(max((upper[:2] - lower[:2]) / (resolution_x, resolution_y))) * max(resolution_x, resolution_y)
                eye.location = basis @ Vector((*((lower[:2] + upper[:2]) / 2), upper[2] + radius))
            depsgraph.update()
            return render(eye) if live is None else draw(live.space, live.region, eye.matrix_world.inverted(), eye.calc_matrix_camera(depsgraph, x=resolution_x, y=resolution_y))

    def viewed(found: Viewport) -> NDArray[np.uint8]:
        """Pixels of the viewport's own view, drawn live or rendered in a background run through a camera from stored view and window matrices, an orthographic eye set back by half the clip range the viewport centers on it."""
        region_3d = found.space.region_3d
        if live is not None:
            return draw(found.space, found.region, region_3d.view_matrix, region_3d.window_matrix)
        projection, inverse = region_3d.window_matrix, region_3d.view_matrix.inverted()
        with temporary() as (eye, camera):
            camera.sensor_fit, camera.clip_start, camera.clip_end = "HORIZONTAL", found.space.clip_start, found.space.clip_end
            if region_3d.is_perspective:
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
        """Opaque Workbench render through the camera to an RGB PNG at the capture path under Solid mode's view settings, rows top down."""
        settings, output, display = scene.render, scene.render.image_settings, scene.display_settings
        display_settings, view_settings = (output.display_settings, output.view_settings) if output.color_management == "OVERRIDE" else (display, scene.view_settings)
        with assigned(
            *((output, p.identifier, getattr(output, p.identifier)) for p in output.bl_rna.properties if p.type != "POINTER" and not p.is_readonly),
            *(() if output.file_format == "PNG" else ((output.linear_colorspace_settings, "name", output.linear_colorspace_settings.name),)),
            *((block, "hide_render", block.hide_viewport) for block in (scene.collection, *scene.collection.children_recursive)),
            *((o, "hide_render", not o.visible_get(viewport=space)) for o in layer.objects),
            *((settings, flag, False) for flag in ("film_transparent", "use_border", "use_stamp", "use_compositing", "use_sequencer")),
            (scene, "camera", eye),
            (settings, "engine", "BLENDER_WORKBENCH"),
            (settings, "filepath", str(path)),
            (settings, "dither_intensity", 0.0),
            (output, "media_type", "IMAGE"),
            (output, "file_format", "PNG"),
            (output, "color_mode", "RGB"),
            *((display_settings, p.identifier, getattr(display, p.identifier)) for p in display.bl_rna.properties if not p.is_readonly),
            (view_settings, "view_transform", GetCurrentConfig().getDefaultView(display.display_device)),
            (view_settings, "look", "None"),
            *((view_settings, p.identifier, p.default) for p in view_settings.bl_rna.properties if p.type in {"BOOLEAN", "FLOAT"} and not (p.is_readonly or p.is_array)),
        ):
            bpy.ops.render.render(write_still=True)
        return ImageBuf(str(path)).get_pixels(UINT8)

    def write(target: Path, pixels: NDArray[np.uint8]) -> None:
        """Write the pixels as a PNG holding the view name, and the frame when the view has one, as text the next `since` reads."""
        image = ImageBuf(np.ascontiguousarray(pixels))
        if frame is not None:
            image.specmod().attribute(frame_key, JSON.dumps(frame))
        image.specmod().attribute(view_key, chosen.value)
        if not image.write(str(target)):
            raise RuntimeError(image.geterror())

    match chosen.rotation:
        case None if largest is not None:
            frame, pixels = None, viewed(largest)
        case None:
            return NoViewport()
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


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Capture", "Difference", "EmptyFrame", "HiddenInViewport", "MissingCapture", "NoViewport", "UnknownView", "View", "capture"]
