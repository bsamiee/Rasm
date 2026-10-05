# ty: ignore[unresolved-attribute, unresolved-import]
# mypy: disable-error-code="arg-type, no-any-return, union-attr"
"""One scene view as `.artifacts/blender/<name>.png` with the user's view kept, diffed against an earlier one."""

from collections.abc import Generator, Iterable
from contextlib import contextmanager, ExitStack
from enum import StrEnum
from itertools import product
from pathlib import Path
from typing import Self

import attrs
import bpy
import gpu
from mathutils import Color, Euler, Matrix
import numpy as np
from numpy.typing import NDArray
from OpenImageIO import ImageBuf, UINT8
from PyOpenColorIO import GetCurrentConfig
from results import artifacts, collect_faults, Fault, Faults, JSON, Resolved, unknown
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
class Perspective:
    """Focal length in millimeters over the default sensor across the longer side."""

    lens: float


@attrs.frozen
class Orthographic:
    """Span in meters across the longer side."""

    ortho_scale: float


@attrs.frozen
class Camera:
    """Eye XYZ Euler rotation in radians and location, projection, shifts in longer-side units, clip distances, and pixel size a capture draws through and a `since` capture replays."""

    rotation: tuple[float, float, float]
    location: tuple[float, float, float]
    projection: Perspective | Orthographic
    shift: tuple[float, float]
    clip: tuple[float, float]
    size: tuple[int, int]


@attrs.frozen
class Difference:
    """Count of pixels past Blender's render-test threshold, a diff PNG mixing them half with red over the earlier capture at half brightness, and whether drawn geometry leaves the earlier frame or clip range."""

    changed: int
    diff: str
    outside: bool


@attrs.frozen
class Capture:
    """Record the written PNG stores with its path, view, camera, and comparison against a `since` capture."""

    path: str
    view: View
    camera: Camera
    comparison: Difference | None


# --- [OPERATIONS] -----------------------------------------------------------------------


def capture(name: str, *, objects: tuple[str, ...] = (), view: str = View.ISO, size: tuple[int, int] | None = None, since: str | None = None) -> Resolved[Capture]:
    """Return the capture drawn to `.artifacts/blender/<name>.png` framing `objects` or every visible one from `view` at `size`, else at the pixel budget at the view's aspect, with `since` replaying an earlier capture's view and camera to count changed pixels."""
    margin, threshold, budget = 1.05, 0.016, 300_000
    path, scene, layer = artifacts() / f"{name}.png", bpy.context.scene, bpy.context.view_layer
    image = None if since is None else ImageBuf(str(path.with_stem(since)))
    stored = None if image is None else image.spec().getattribute(Capture.__name__)
    replay = None if stored is None else JSON.loads(stored, Capture)
    earlier = None if image is None or replay is None else (image.get_pixels(UINT8), replay)
    requested = view if replay is None else replay.view
    if faults := collect_faults(
        None if since is None or earlier is not None else Fault(Path, since, tuple(sorted(p.stem for p in path.parent.glob("*.png") if ImageBuf(str(p)).spec().getattribute(Capture.__name__)))),
        None if requested in View else Fault(View, requested, tuple(View)),
        unknown(scene.objects, objects),
    ):
        return faults
    chosen, largest = View(requested), viewport()
    live, space = (None if bpy.app.background else largest), (None if largest is None else largest.space)
    shown = tuple(o.name for o in scene.objects if o.visible_get(viewport=space))
    if hidden := collect_faults(*(Fault(bpy.types.SpaceView3D, n, shown) for n in objects if n not in shown)):
        return hidden
    depsgraph, considered = bpy.context.evaluated_depsgraph_get(), objects or shown
    drawn = {owner: box for owner, box in bounds(depsgraph, drawn=True).items() if owner in shown}
    boxes = np.array([box for owner, box in drawn.items() if owner in considered], dtype=np.float64).reshape(-1, 2, 3)
    corners = boxes[:, np.array(list(product((0, 1), repeat=3))), np.arange(3)].reshape(-1, 3)

    def sized(aspect: float) -> tuple[int, int]:
        """Exact `size`, else the budget's pixels at the aspect."""
        return size or (round(float(np.sqrt(budget * aspect))), round(float(np.sqrt(budget / aspect))))

    def framed(rotation: Euler, data: bpy.types.Camera) -> Camera:
        """Camera at `rotation` framing the corners' projected extent with half the margin's excess over its longer side as a border on every side, an iso eye where the default lens fits the extent, and an axis eye one extent span above it."""
        basis, tangent = np.asarray(rotation.to_matrix()), data.sensor_width / 2 / data.lens
        seen = corners @ basis
        plus, minus = ((sign * seen[:, :2] + tangent * seen[:, 2:]).max(axis=0) for sign in (1, -1))
        center, distance = (
            ((plus - minus) / 2, float(((plus + minus) / (2 * tangent)).max()))
            if chosen is View.ISO
            else ((seen.min(axis=0) + seen.max(axis=0))[:2] / 2, float(seen[:, 2].max() + np.ptp(seen, axis=0).max()))
        )
        projected = (seen[:, :2] - center) / ((distance - seen[:, 2:]) if chosen is View.ISO else 1.0)
        lower, upper = projected.min(axis=0), projected.max(axis=0)
        frame = upper - lower + (margin - 1) * float((upper - lower).max())
        width, height = sized(float(frame[0] / frame[1]))
        span = float((frame / np.array((min(1.0, width / height), min(1.0, height / width)))).max())
        shift_x, shift_y = (lower + upper) / 2 / span
        depths = distance - seen[:, 2]
        location = tuple((basis @ np.array((*center, distance))).tolist())
        projection = Perspective(data.sensor_width / span) if chosen is View.ISO else Orthographic(span)
        return Camera((rotation.x, rotation.y, rotation.z), location, projection, (float(shift_x), float(shift_y)), (float(depths.min()) / margin, float(depths.max()) * margin), (width, height))

    def viewed(found: Viewport, data: bpy.types.Camera) -> Camera:
        """Camera reproducing the viewport's view from its matrices at the region's aspect, with an orthographic eye set back by half the clip range the viewport centers on it."""
        region_3d, near, far = found.space.region_3d, found.space.clip_start, found.space.clip_end
        projection, inverse = np.array(region_3d.window_matrix), region_3d.view_matrix.inverted()
        width, height = sized(found.region.width / found.region.height)
        sides, fit = np.array((min(1.0, width / height), min(1.0, height / width))), float(projection.diagonal()[:2].min())
        eye, optics, (shift_x, shift_y) = (
            (inverse, Perspective(fit * data.sensor_width / 2), projection[:2, 2] / 2 * sides)
            if region_3d.is_perspective
            else (inverse @ Matrix.Translation((0.0, 0.0, far / 2)), Orthographic(2 / fit), -projection[:2, 3] / 2 * sides)
        )
        turn, origin = eye.to_euler(), eye.translation
        return Camera((turn.x, turn.y, turn.z), (origin.x, origin.y, origin.z), optics, (float(shift_x), float(shift_y)), (near, far), (width, height))

    def placed(data: bpy.types.Camera) -> Camera | Fault:
        """Camera the earlier capture stores, else the view's own, or a fault for a frame holding nothing drawn or a user view with no 3D Viewport."""
        match earlier, chosen.rotation:
            case (_, Capture(camera=replayed)), _:
                return replayed
            case _, None:
                return Fault(View, chosen, tuple(member for member in View if member.rotation is not None)) if largest is None else viewed(largest, data)
            case _ if not corners.size:
                return Fault(Capture, considered, tuple(drawn))
            case _, tuple() as degrees:
                return framed(Euler(np.radians(degrees).tolist()), data)

    @contextmanager
    def assigned(*changes: tuple[object, str, object]) -> Generator[None]:
        """Attributes set for the scope and restored in order after it, including values a later row's update changed."""

        def put(rows: Iterable[tuple[object, str, object]]) -> None:
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
    def temporary() -> Generator[tuple[bpy.types.Object, bpy.types.Camera]]:
        """Camera object linked into the scene for the scope, removed with its data after it."""
        data = bpy.data.cameras.new(name)
        eye = bpy.data.objects.new(name, data)
        with ExitStack() as stack:
            stack.callback(bpy.data.cameras.remove, data)
            stack.callback(bpy.data.objects.remove, eye)
            scene.collection.objects.link(eye)
            yield eye, data

    def draw(shown: Viewport, eye: bpy.types.Object, width: int, height: int) -> NDArray[np.uint8]:
        """Offscreen draw through the eye of the viewport window's scene under the viewport's shading with overlays and gizmos off, rows top down."""
        offscreen = gpu.types.GPUOffScreen(width, height)
        with ExitStack() as stack:
            stack.callback(offscreen.free)
            stack.enter_context(assigned((shown.space.overlay, "show_overlays", False), (shown.space, "show_gizmo", False)))
            offscreen.draw_view3d(
                shown.window.scene, shown.window.view_layer, shown.space, shown.region, eye.matrix_world.inverted(), eye.calc_matrix_camera(depsgraph, x=width, y=height), do_color_management=True
            )
            return np.array(offscreen.texture_color.read(), dtype=np.uint8).reshape(height, width, 4)[::-1, :, :3]

    def render(eye: bpy.types.Object, width: int, height: int) -> NDArray[np.uint8]:
        """Opaque Workbench render to an RGB PNG at the capture path under the viewport's Solid shading, theme fill, and color management, rows top down, removing each Render Result it creates and keeping each shading enum that reads empty."""
        settings, output, display, shading = scene.render, scene.render.image_settings, scene.display_settings, scene.display.shading
        display_settings, view_settings = (output.display_settings, output.view_settings) if output.color_management == "OVERRIDE" else (display, scene.view_settings)
        solid = shading if space is None else space.shading
        backdrop = bpy.context.preferences.themes[0].view_3d.space.gradients.high_gradient.from_srgb_to_scene_linear()
        with assigned(
            *((output, p.identifier, getattr(output, p.identifier)) for p in output.bl_rna.properties if not (isinstance(p, bpy.types.PointerProperty) or p.is_readonly)),
            *(() if output.file_format == "PNG" else ((output.linear_colorspace_settings, "name", output.linear_colorspace_settings.name),)),
            *((block, "hide_render", block.hide_viewport) for block in (scene.collection, *scene.collection.children_recursive)),
            *((o, "hide_render", not (o.visible_get(viewport=space) and o.display_type in SHOWN)) for o in layer.objects),
            *((settings, flag, False) for flag in ("film_transparent", "use_border", "use_stamp", "use_compositing", "use_sequencer")),
            *(
                (shading, p.identifier, getattr(solid, p.identifier))
                for p in solid.bl_rna.properties
                if not (isinstance(p, bpy.types.PointerProperty) or p.is_readonly) and (not isinstance(p, bpy.types.EnumProperty) or getattr(shading, p.identifier))
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
            (settings, "resolution_x", width),
            (settings, "resolution_y", height),
            (settings, "resolution_percentage", 100),
            (settings, "pixel_aspect_x", 1.0),
            (settings, "pixel_aspect_y", 1.0),
            (settings, "filepath", str(path)),
            (settings, "dither_intensity", 0.0),
            (output, "media_type", "IMAGE"),
            (output, "file_format", "PNG"),
            (output, "color_mode", "RGB"),
            (output, "color_depth", "8"),
            *((display_settings, p.identifier, getattr(display, p.identifier)) for p in display.bl_rna.properties if not p.is_readonly),
            (view_settings, "view_transform", GetCurrentConfig().getDefaultView(display.display_device)),
            (view_settings, "look", "None"),
            *(
                (view_settings, p.identifier, p.default)
                for p in view_settings.bl_rna.properties
                if isinstance(p, bpy.types.BoolProperty | bpy.types.FloatProperty) and not (p.is_readonly or p.is_array)
            ),
        ):
            held = {image.name for image in bpy.data.images}
            bpy.ops.render.render(write_still=True)
        for created in [image for image in bpy.data.images if image.name not in held]:
            bpy.data.images.remove(created)
        return ImageBuf(str(path)).get_pixels(UINT8)

    def write(target: str, pixels: NDArray[np.uint8], attributes: dict[str, str]) -> None:
        """Encode the pixels in process as a PNG at the highest compression with each attribute's JSON text for the next `since`."""
        image = ImageBuf(np.ascontiguousarray(pixels))
        image.specmod().attribute("png:compressionLevel", 9)
        for key, text in attributes.items():
            image.specmod().attribute(key, text)
        if not image.write(target):
            raise RuntimeError(image.geterror())

    with temporary() as (eye, data):
        if isinstance(camera := placed(data), Fault):
            return Faults.of(camera)
        width, height = camera.size
        eye.rotation_euler, eye.location = camera.rotation, camera.location
        (data.shift_x, data.shift_y), (data.clip_start, data.clip_end) = camera.shift, camera.clip
        match camera.projection:
            case Perspective(lens=lens):
                data.lens = lens
            case Orthographic(ortho_scale=scale):
                data.type, data.ortho_scale = "ORTHO", scale
        depsgraph.update()
        pixels = render(eye, width, height) if live is None else draw(live, eye, width, height)
        frustum = np.asarray(eye.calc_matrix_camera(depsgraph, x=width, y=height) @ eye.matrix_world.inverted())
    match earlier:
        case None:
            comparison = None
        case (previous, _):
            mask = (np.abs(previous.astype(np.int16) - pixels) > threshold * 255).any(axis=-1)
            diff = str(path.with_stem(f"{name}-diff"))
            write(diff, np.where(mask[..., None], (previous.astype(np.uint16) + np.array((255, 0, 0), dtype=np.uint16)) // 2, previous // 2).astype(np.uint8), {})
            projected = np.c_[corners, np.ones(len(corners))] @ frustum.T
            comparison = Difference(int(mask.sum()), diff, bool((np.abs(projected[:, :3]) > projected[:, 3:]).any()))
    record = Capture(str(path), chosen, camera, comparison)
    write(str(path), pixels, {Capture.__name__: JSON.dumps(record)})
    return record


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Camera", "Capture", "Difference", "Orthographic", "Perspective", "View", "capture"]
