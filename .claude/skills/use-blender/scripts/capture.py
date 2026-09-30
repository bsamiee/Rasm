# mypy: disable-error-code="import-not-found, import-untyped, no-any-return, union-attr, arg-type"
# ty: ignore[unresolved-import, unresolved-attribute, invalid-argument-type]
"""Write one scene view to `.artifacts/blender/<name>.png` without moving the user's view."""

from collections.abc import Iterable, Iterator
from contextlib import contextmanager, ExitStack
from enum import StrEnum
from functools import partial
from itertools import product
from pathlib import Path
from typing import Self

import attrs
import bpy
import gpu
from mathutils import Color, Euler, Matrix, Vector
import numpy as np
from numpy.typing import NDArray
from OpenImageIO import ImageBuf, UINT8
from PyOpenColorIO import GetCurrentConfig
from results import artifacts, JSON, unknown, UnknownObjects
from scene import bounds, SHOWN, Viewport, viewport

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
    """Count of pixels past Blender's render-test threshold against the earlier capture, the diff PNG mixing them half with red over the earlier capture, and whether geometry left the earlier frame."""

    changed: int
    diff: str
    outside: bool


@attrs.frozen
class Capture:
    """Record the written PNG stores with its path, view, world box an axis or iso view framed as lower and upper corners (`None` for the user's view), and comparison against a `since` capture."""

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
    """Objects considered for the frame, none showing faces, points, strokes, or hair as solid or textured."""

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
    name: str, *, objects: tuple[str, ...] = (), view: str = View.ISO, size: tuple[int, int] | None = None, since: str | None = None
) -> Capture | UnknownObjects | UnknownView | HiddenInViewport | EmptyFrame | NoViewport | MissingCapture:
    """Draw `.artifacts/blender/<name>.png` framing `objects` or every visible one from `view` at `size`, else the viewport's size or the render's pixel count at the framed extent's aspect within 2000 px, `since` replaying an earlier capture's view, frame, and size to count changed pixels."""
    margin, threshold, longest, held_key = 1.05, 0.016, 2000, "Boxes"
    path = artifacts() / f"{name}.png"
    source = None if since is None else path.with_stem(since)
    match source:
        case None:
            earlier, stored, requested = None, None, view
        case Path() if not source.exists():
            return MissingCapture(source.stem)
        case Path():
            spec = (image := ImageBuf(str(source))).spec()
            record, kept = JSON.loads(spec.getattribute(Capture.__name__), Capture), spec.getattribute(held_key)
            earlier, stored, requested = image.get_pixels(UINT8), (None if kept is None else np.array(JSON.loads(kept, list[list[list[float]]]))), record.view
    if requested not in View:
        return UnknownView(requested, tuple(View))
    chosen, scene, layer = View(requested), bpy.context.scene, bpy.context.view_layer
    if (absent := unknown(scene.objects, objects)) is not None:
        return absent
    largest = viewport()
    live, space = (None if bpy.app.background else largest), (None if largest is None else largest.space)
    if hidden := tuple(n for n in objects if not scene.objects[n].visible_get(viewport=space)):
        return HiddenInViewport(hidden)
    depsgraph, considered = bpy.context.evaluated_depsgraph_get(), objects or tuple(o.name for o in scene.objects if o.visible_get(viewport=space))
    boxes = [box for owner, box in bounds(depsgraph, drawn=True).items() if owner in considered]
    corners = np.array(boxes, dtype=np.float64).reshape(-1, 3)
    rendered = np.array((scene.render.resolution_x, scene.render.resolution_y), dtype=np.float64) * scene.render.resolution_percentage / 100
    area = float(np.prod(rendered * min(1.0, longest / rendered.max())))

    @contextmanager
    def assigned(*changes: "tuple[bpy.types.bpy_struct[object], str, object]") -> Iterator[None]:
        """Attributes set for the scope and restored in order after it, including values a later row's update changed."""

        def put(rows: "Iterable[tuple[bpy.types.bpy_struct[object], str, object]]") -> None:
            """Set each attribute holding a value other than its row's."""
            for owner, key, value in rows:
                if getattr(owner, key) != value:
                    setattr(owner, key, value)

        saved = [(owner, key, tuple(value) if isinstance(value, bpy.types.bpy_prop_array | Color) else value) for owner, key, _ in changes for value in (getattr(owner, key),)]
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

    def framed(rotation: Euler, grown: NDArray[np.float64], seen: NDArray[np.float64]) -> NDArray[np.uint8]:
        """Pixels through a camera at `rotation` fit to the grown box's world corners, `seen` in camera axes, drawn in the live viewport or rendered in a background run."""
        radius, basis, lower, upper = float(np.linalg.norm(np.ptp(grown, axis=0))) / 2, rotation.to_matrix(), seen.min(axis=0), seen.max(axis=0)
        with temporary() as (eye, camera):
            eye.rotation_euler = rotation
            camera.clip_start, camera.clip_end = camera.clip_start * radius, camera.clip_end * radius
            if chosen is View.ISO:
                depsgraph.update()
                eye.location, _ = eye.camera_fit_coords(depsgraph, grown.ravel().tolist())
            else:
                camera.type, camera.ortho_scale = "ORTHO", float(max((upper[:2] - lower[:2]) / (resolution_x, resolution_y))) * max(resolution_x, resolution_y)
                eye.location = basis @ Vector((*((lower[:2] + upper[:2]) / 2), upper[2] + radius))
            depsgraph.update()
            return render(eye) if live is None else draw(live.space, live.region, eye.matrix_world.inverted(), eye.calc_matrix_camera(depsgraph, x=resolution_x, y=resolution_y))

    def viewed(found: Viewport) -> NDArray[np.uint8]:
        """Pixels of the viewport's view at its horizontal field of view, drawn live or rendered through a camera from the stored matrices, an orthographic eye set back by half the clip range the viewport centers on it."""
        region_3d = found.space.region_3d
        if live is not None:
            aspect = Matrix.Diagonal((1.0, resolution_x * found.region.height / (resolution_y * found.region.width), 1.0, 1.0))
            return draw(found.space, found.region, region_3d.view_matrix, aspect @ region_3d.window_matrix)
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
        """Opaque Workbench render to an RGB PNG at the capture path under the viewport's Solid shading, theme fill, and color management, rows top down, a Render Result it created removed and an empty-reading shading enum kept."""
        settings, output, display, shading = scene.render, scene.render.image_settings, scene.display_settings, scene.display.shading
        display_settings, view_settings = (output.display_settings, output.view_settings) if output.color_management == "OVERRIDE" else (display, scene.view_settings)
        solid = shading if space is None else space.shading
        backdrop = Color(bpy.context.preferences.themes[0].view_3d.space.gradients.high_gradient).from_srgb_to_scene_linear()
        with assigned(
            *((output, p.identifier, getattr(output, p.identifier)) for p in output.bl_rna.properties if p.type != "POINTER" and not p.is_readonly),
            *(() if output.file_format == "PNG" else ((output.linear_colorspace_settings, "name", output.linear_colorspace_settings.name),)),
            *((block, "hide_render", block.hide_viewport) for block in (scene.collection, *scene.collection.children_recursive)),
            *((o, "hide_render", not (o.visible_get(viewport=space) and o.display_type in SHOWN)) for o in layer.objects),
            *((settings, flag, False) for flag in ("film_transparent", "use_border", "use_stamp", "use_compositing", "use_sequencer")),
            *(
                (shading, p.identifier, getattr(solid, p.identifier))
                for p in solid.bl_rna.properties
                if p.type != "POINTER" and not p.is_readonly and (p.type != "ENUM" or getattr(shading, p.identifier))
            ),
            *(
                (
                    (shading, "background_type", "VIEWPORT"),
                    (shading, "background_color", Color(tuple(channel ** (1 / 2.2) for channel in (backdrop.r, backdrop.g, backdrop.b))).from_srgb_to_scene_linear()),
                )
                if solid.background_type == "THEME"
                else ()
            ),
            (scene, "camera", eye),
            (settings, "engine", "BLENDER_WORKBENCH"),
            (settings, "filepath", str(path)),
            (settings, "dither_intensity", 0.0),
            (output, "media_type", "IMAGE"),
            (output, "file_format", "PNG"),
            (output, "color_mode", "RGB"),
            (output, "color_depth", "8"),
            *((display_settings, p.identifier, getattr(display, p.identifier)) for p in display.bl_rna.properties if not p.is_readonly),
            (view_settings, "view_transform", GetCurrentConfig().getDefaultView(display.display_device)),
            (view_settings, "look", "None"),
            *((view_settings, p.identifier, p.default) for p in view_settings.bl_rna.properties if p.type in {"BOOLEAN", "FLOAT"} and not (p.is_readonly or p.is_array)),
        ):
            held = {image.name for image in bpy.data.images}
            bpy.ops.render.render(write_still=True)
        for created in [image for image in bpy.data.images if image.name not in held]:
            bpy.data.images.remove(created)
        return ImageBuf(str(path)).get_pixels(UINT8)

    def write(target: Path, pixels: NDArray[np.uint8], attributes: dict[str, str]) -> None:
        """Write the pixels as a PNG at the highest compression, holding each attribute's JSON text for the next `since`."""
        image = ImageBuf(np.ascontiguousarray(pixels))
        image.specmod().attribute("png:compressionLevel", 9)
        for key, text in attributes.items():
            image.specmod().attribute(key, text)
        if not image.write(str(target)):
            raise RuntimeError(image.geterror())

    match chosen.rotation, largest:
        case None, Viewport() as found:
            frame, held, shown, pending = None, None, np.array((found.region.width, found.region.height), dtype=np.float64), partial(viewed, found)
        case None, _:
            return NoViewport()
        case _ if stored is None and not corners.size:
            return EmptyFrame(considered)
        case tuple() as degrees, _:
            held, rotation = (stored if stored is not None else corners.reshape(-1, 2, 3)), Euler(np.radians(degrees).tolist())
            frame = (tuple(held[:, 0].min(axis=0).tolist()), tuple(held[:, 1].max(axis=0).tolist()))
            pad = (margin - 1) / 2 * float(np.ptp(np.array(frame), axis=0).max())
            grown = (held + np.array([[-pad], [pad]]))[:, np.array(list(product((0, 1), repeat=3))), np.arange(3)].reshape(-1, 3)
            extent = np.ptp((seen := grown @ np.asarray(rotation.to_matrix()))[:, :2], axis=0)
            shown, pending = extent * np.sqrt(area / np.prod(extent)), partial(framed, rotation, grown, seen)
    match earlier, size:
        case np.ndarray(), _:
            resolution_y, resolution_x = earlier.shape[:2]
        case None, (width, height):
            resolution_x, resolution_y = width, height
        case _:
            resolution_x, resolution_y = (round(float(side) * min(1.0, longest / float(shown.max()))) for side in shown)
    pixels = pending()
    match earlier:
        case None:
            comparison = None
        case previous:
            mask = (np.abs(previous.astype(np.int16) - pixels) > threshold * 255).any(axis=-1)
            diff = path.with_stem(f"{name}-diff")
            write(diff, np.where(mask[..., None], (previous.astype(np.uint16) + np.array((255, 0, 0), dtype=np.uint16)) // 2, previous // 2).astype(np.uint8), {})
            comparison = Difference(int(mask.sum()), str(diff), frame is not None and bool((np.clip(corners, *frame) != corners).any()))
    record = Capture(str(path), frame, chosen, comparison)
    write(path, pixels, {Capture.__name__: JSON.dumps(record), **({} if held is None else {held_key: JSON.dumps(held.tolist())})})
    return record


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Capture", "Difference", "EmptyFrame", "HiddenInViewport", "MissingCapture", "NoViewport", "UnknownView", "View", "capture"]
