# ast-grep-ignore: no-json-codec, no-stdlib-record
# mypy: disable-error-code="import-not-found"
# ty: ignore[unresolved-import]
"""Write one framed view of the scene to `.artifacts/blender/<name>.png` without moving the user's view, run inside Blender through `runpy.run_path`."""

from collections.abc import Iterator
from contextlib import contextmanager, ExitStack
from dataclasses import asdict, dataclass
from enum import StrEnum
import json
from pathlib import Path
from typing import cast, Self

import bpy
import gpu
from mathutils import Euler, Matrix, Vector
import numpy as np
from numpy.typing import NDArray
from OpenImageIO import ImageBuf, UINT8

# --- [TYPES] ----------------------------------------------------------------------------

type Corner = tuple[float, float, float]
type Outcome = Capture | UnknownObjects | UnknownView | HiddenInViewport | EmptyFrame | NoViewport | MissingCapture


class View(StrEnum):
    """Views a capture draws, each with its camera's XYZ Euler rotation in degrees as Blender's numpad views orient it, `USER` taking the viewport's own view."""

    rotation: tuple[float, float, float] | None

    def __new__(cls, value: str, rotation: tuple[float, float, float] | None) -> Self:
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
    USER = "user", None


# --- [MODELS] ---------------------------------------------------------------------------


@dataclass(frozen=True)
class Difference:
    """Pixels that differ from the earlier capture beyond Blender's render-test threshold, marked red in the diff file."""

    since: str
    differing: int
    diff: str


@dataclass(frozen=True)
class Capture:
    """Written file, the world box the view framed as lower and upper corners, and the comparison when `since` named an earlier capture."""

    path: str
    frame: tuple[Corner, Corner]
    view: View
    comparison: Difference | None


# --- [ERRORS] ---------------------------------------------------------------------------


@dataclass(frozen=True)
class UnknownObjects:
    """Object names absent from the scene."""

    names: tuple[str, ...]


@dataclass(frozen=True)
class UnknownView:
    """View name outside the views a capture draws."""

    view: str
    views: tuple[View, ...]


@dataclass(frozen=True)
class HiddenInViewport:
    """Named objects the drawing viewport does not show, hidden or outside its local view."""

    names: tuple[str, ...]


@dataclass(frozen=True)
class EmptyFrame:
    """Objects considered for the frame, none with evaluated geometry or instances."""

    objects: tuple[str, ...]


@dataclass(frozen=True)
class NoViewport:
    """Background run with no 3D Viewport for the user's view."""

    view: View


@dataclass(frozen=True)
class MissingCapture:
    """Capture name with no file under `.artifacts/blender/`."""

    name: str


# --- [OPERATIONS] -----------------------------------------------------------------------


def capture(name: str, objects: tuple[str, ...] = (), view: str | None = None, since: str | None = None) -> Outcome:
    """Frame the named objects, or every visible one, from a view and write the image, `since` redraws an earlier capture's frame and view and counts the pixels that differ."""
    limit, margin, threshold = (1280, 720), 1.05, 0.016
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
            earlier, stored, remembered = (
                (source.stem, np.asarray(image.get_pixels(UINT8)[..., :3], dtype=np.uint8)),
                json.loads(image.spec().getattribute(frame_key)),
                image.spec().getattribute(view_key),
            )
    if (chosen := next((v for v in View if v == (requested := view if view is not None else remembered)), None)) is None:
        return UnknownView(requested, tuple(View))
    scene, layer = cast("bpy.types.Scene", bpy.context.scene), cast("bpy.types.ViewLayer", bpy.context.view_layer)
    if missing := tuple(n for n in objects if n not in scene.objects):
        return UnknownObjects(missing)
    window = None if bpy.app.background else bpy.context.window
    viewport = max(
        (
            (space, region, data)
            for area in (window.screen.areas if window else ())
            if isinstance(space := area.spaces.active, bpy.types.SpaceView3D)
            for region in area.regions
            if isinstance(data := region.data, bpy.types.RegionView3D)
        ),
        key=lambda found: found[1].width * found[1].height,
        default=None,
    )
    space = viewport[0] if viewport else None
    if hidden := tuple(n for n in objects if not scene.objects[n].visible_get(viewport=space)):
        return HiddenInViewport(hidden)
    depsgraph = bpy.context.evaluated_depsgraph_get()
    named = frozenset(objects)
    corners = [
        instance.matrix_world @ Vector(c)
        for instance in depsgraph.object_instances
        if (body := instance.object) is not None and (holder := instance.parent if instance.is_instance else body) is not None and (owner := holder.original) is not None
        if (owner.name in named if named else owner.visible_get(viewport=space)) and body.bound_box[0][:] != body.bound_box[6][:]
        for c in body.bound_box
    ]
    if stored is None and not corners:
        return EmptyFrame(objects or tuple(o.name for o in scene.objects if o.visible_get(viewport=space)))
    points = np.array(corners, dtype=np.float64)
    bounds = np.array(stored if stored is not None else (points.min(axis=0), points.max(axis=0)), dtype=np.float64)
    frame: tuple[Corner, Corner] = (tuple(bounds[0].tolist()), tuple(bounds[1].tolist()))
    center, radius = bounds.mean(axis=0), float(np.linalg.norm(bounds[1] - bounds[0])) / 2
    coords = np.stack(np.meshgrid(*(center + (bounds - center) * margin).T), axis=-1).ravel().tolist()
    match earlier, chosen, viewport:
        case (_, previous), _, _:
            height, width = previous.shape[:2]
        case None, View.USER, (_, region, _):
            width, height = region.width, region.height
        case _:
            width, height = limit
    scale = min(limit[0] / width, limit[1] / height)
    size = (round(width * scale), round(height * scale))

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
    def framed(rotation: Euler) -> Iterator[bpy.types.Object]:
        """Temporary camera at `rotation` fit to the frame grown by the margin at the capture's resolution."""
        camera = bpy.data.cameras.new(name)
        eye = bpy.data.objects.new(name, camera)
        with ExitStack() as stack:
            stack.callback(bpy.data.cameras.remove, camera)
            stack.callback(bpy.data.objects.remove, eye)
            stack.enter_context(assigned((scene.render, "resolution_x", size[0]), (scene.render, "resolution_y", size[1]), (scene.render, "resolution_percentage", 100)))
            scene.collection.objects.link(eye)
            camera.type, camera.sensor_fit, eye.matrix_world = "PERSP" if chosen is View.ISO else "ORTHO", "VERTICAL", rotation.to_matrix().to_4x4()
            camera.clip_start, camera.clip_end = camera.clip_start * radius, camera.clip_end * radius
            depsgraph.update()
            location, camera.ortho_scale = eye.camera_fit_coords(depsgraph, coords)
            eye.matrix_world = Matrix.LocRotScale(location[:], rotation, None)
            depsgraph.update()
            yield eye

    def draw(space: bpy.types.SpaceView3D, region: bpy.types.Region, view_matrix: Matrix, projection: Matrix) -> NDArray[np.uint8]:
        """Offscreen draw through the viewport's shading with overlays and gizmos off, rows top down."""
        offscreen = gpu.types.GPUOffScreen(*size)
        with ExitStack() as stack:
            stack.callback(offscreen.free)
            stack.enter_context(assigned((space.overlay, "show_overlays", False), (space, "show_gizmo", False)))
            offscreen.draw_view3d(scene, layer, space, region, view_matrix, projection, do_color_management=True)
            return np.array(offscreen.texture_color.read(), dtype=np.uint8).reshape(size[1], size[0], 4)[::-1, :, :3]

    def render(eye: bpy.types.Object) -> NDArray[np.uint8]:
        """Workbench render through the camera written to the capture path, rows top down."""
        settings, output = scene.render, scene.render.image_settings
        with assigned((scene, "camera", eye), (settings, "engine", "BLENDER_WORKBENCH"), (settings, "filepath", str(path)), (output, "media_type", "IMAGE"), (output, "file_format", "PNG")):
            bpy.ops.render.render(write_still=True)
        return np.asarray(ImageBuf(str(path)).get_pixels(UINT8)[..., :3], dtype=np.uint8)

    def write(target: Path, pixels: NDArray[np.uint8]) -> None:
        """PNG of the pixels holding the frame and view as text the next `since` reads."""
        image = ImageBuf(np.ascontiguousarray(pixels))
        image.specmod().attribute(frame_key, json.dumps(frame))
        image.specmod().attribute(view_key, chosen.value)
        if not image.write(str(target)):
            raise RuntimeError(image.geterror())

    path.parent.mkdir(parents=True, exist_ok=True)
    match chosen.rotation:
        case None:
            match viewport:
                case None:
                    return NoViewport(chosen)
                case (shown, region, data):
                    pixels = draw(shown, region, data.view_matrix, data.window_matrix)
        case degrees:
            with framed(Euler(np.radians(degrees).tolist())) as eye:
                pixels = render(eye) if viewport is None else draw(viewport[0], viewport[1], eye.matrix_world.inverted(), eye.calc_matrix_camera(depsgraph, x=size[0], y=size[1]))
    write(path, pixels)
    if earlier is None:
        return Capture(str(path), frame, chosen, None)
    label, previous = earlier
    mask = (np.abs(previous.astype(np.int16) - pixels) > threshold * 255).any(axis=-1)
    diff = path.with_stem(f"{name}-diff")
    write(diff, np.where(mask[..., None], np.array((255, 0, 0), dtype=np.uint8), previous // 2))
    return Capture(str(path), frame, chosen, Difference(label, int(mask.sum()), str(diff)))


def as_result(value: Outcome) -> dict[str, object]:
    """`result` dict for `execute_blender_code`, the case name under `kind`."""
    return {"kind": type(value).__name__, **asdict(value)}


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Capture", "Corner", "Difference", "EmptyFrame", "HiddenInViewport", "MissingCapture", "NoViewport", "Outcome", "UnknownObjects", "UnknownView", "View", "as_result", "capture"]
