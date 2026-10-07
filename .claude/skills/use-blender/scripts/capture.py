# ty: ignore[invalid-argument-type, unresolved-attribute]
# mypy: disable-error-code="arg-type, attr-defined, no-any-return, union-attr"
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

type Placement = Camera | Fault


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
class Camera:
    """World-to-view and projection matrices with the pixel size a capture draws through and a `since` capture replays."""

    view: Matrix
    projection: Matrix
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
    path, largest = artifacts() / f"{name}.png", viewport()
    source = bpy.context if largest is None else largest.window
    scene, layer = source.scene, source.view_layer
    image = None if since is None else ImageBuf(str(path.with_stem(since)))
    replay = JSON.loads(stored, Capture) if image is not None and (stored := image.spec().getattribute(Capture.__name__)) else None
    if faults := collect_faults(
        None if since is None or replay is not None else Fault(Path, since, tuple(sorted(p.stem for p in path.parent.glob("*.png") if ImageBuf(str(p)).spec().getattribute(Capture.__name__)))),
        None if replay is not None or view in View else Fault(View, view, tuple(View)),
        unknown(scene.objects, objects),
    ):
        return faults
    previous = None if image is None else image.get_pixels(UINT8)
    chosen = View(view) if replay is None else replay.view
    live, space = (None if bpy.app.background else largest), (None if largest is None else largest.space)
    shown = tuple(o.name for o in scene.objects if o.visible_get(view_layer=layer, viewport=space))
    if hidden := collect_faults(*(Fault(bpy.types.SpaceView3D, n, shown) for n in objects if n not in shown)):
        return hidden
    with bpy.context.temp_override(scene=scene, view_layer=layer):
        depsgraph = bpy.context.evaluated_depsgraph_get()
    considered = objects or shown
    drawn = {owner: box for owner, box in bounds(depsgraph, drawn=True).items() if owner in shown}
    boxes = np.array([box for owner, box in drawn.items() if owner in considered], dtype=np.float64).reshape(-1, 2, 3)
    corners = boxes[:, np.array(list(product((0, 1), repeat=3))), np.arange(3)].reshape(-1, 3)

    def sized(aspect: float) -> tuple[int, int, NDArray[np.float64]]:
        """Exact `size`, else the budget's pixels at the aspect, with each side over the longer one."""
        width, height = size or (round(float(np.sqrt(budget * aspect))), round(float(np.sqrt(budget / aspect))))
        return width, height, np.divide((width, height), max(width, height))

    def framed(rotation: Matrix) -> Placement:
        """Camera at `rotation` bordering the corners' projected extent by half the margin's excess over its longer side, an iso eye at the default lens's fit distance with its sensor widened to the border, an axis eye one extent span above it, or a fault for an extent with no area."""
        basis, tangent = np.asarray(rotation), data.sensor_width / 2 / data.lens
        seen = corners @ basis
        if not (corners.size and np.ptp(seen[:, :2], axis=0).any()):
            return Fault(Capture, considered, tuple(drawn))
        plus, minus = ((sign * seen[:, :2] + tangent * seen[:, 2:]).max(axis=0) for sign in (1, -1))
        center, distance = (
            ((plus - minus) / 2, float(((plus + minus) / (2 * tangent)).max()))
            if chosen is View.ISO
            else ((seen.min(axis=0) + seen.max(axis=0))[:2] / 2, float(seen[:, 2].max() + np.ptp(seen, axis=0).max()))
        )
        projected = (seen[:, :2] - center) / ((distance - seen[:, 2:]) if chosen is View.ISO else 1.0)
        lower, upper = projected.min(axis=0), projected.max(axis=0)
        frame = upper - lower + (margin - 1) * float((upper - lower).max())
        width, height, sides = sized(float(frame[0] / frame[1]))
        span = float((frame / sides).max())
        depths = distance - seen[:, 2]
        data.shift_x, data.shift_y = ((lower + upper) / 2 / span).tolist()
        data.clip_start, data.clip_end = float(depths.min()) / margin, float(depths.max()) * margin
        if chosen is View.ISO:
            data.sensor_width = span * data.lens
        else:
            data.type, data.ortho_scale = "ORTHO", span
        depsgraph.update()
        world = Matrix.LocRotScale(basis @ np.array((*center, distance)), rotation, None)
        return Camera(world.inverted(), eye.calc_matrix_camera(depsgraph, x=width, y=height), (width, height))

    def viewed(found: Viewport) -> Camera:
        """Viewport's view and projection matrices at the output aspect, the projection's fit across the longer side kept."""
        region_3d = found.space.region_3d
        projection = np.array(region_3d.window_matrix)
        width, height, sides = sized(found.region.width / found.region.height)
        projection[0, 0], projection[1, 1] = projection.diagonal()[:2].min() / sides
        return Camera(region_3d.view_matrix.copy(), Matrix(projection.tolist()), (width, height))

    def placed() -> Placement:
        """Camera the earlier capture stores, else the view's own, or a fault for a user view with no 3D Viewport or a frame with no drawn extent."""
        match replay, chosen.rotation:
            case Capture(camera=replayed), _:
                return replayed
            case _, None:
                return Fault(View, chosen, tuple(member for member in View if member.rotation is not None)) if largest is None else viewed(largest)
            case _, tuple() as degrees:
                return framed(Euler(np.radians(degrees).tolist()).to_matrix())

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
        with ExitStack() as stack:
            data = bpy.data.cameras.new(name)
            stack.callback(bpy.data.cameras.remove, data)
            eye = bpy.data.objects.new(name, data)
            stack.callback(bpy.data.objects.remove, eye)
            scene.collection.objects.link(eye)
            yield eye, data

    def draw(found: Viewport, camera: Camera) -> NDArray[np.uint8]:
        """Offscreen draw of the viewport window's scene through the camera's matrices under the viewport's shading with overlays and gizmos off, rows top down."""
        width, height = camera.size
        offscreen = gpu.types.GPUOffScreen(width, height)
        with ExitStack() as stack:
            stack.callback(offscreen.free)
            stack.enter_context(assigned((found.space.overlay, "show_overlays", False), (found.space, "show_gizmo", False)))
            offscreen.draw_view3d(found.window.scene, found.window.view_layer, found.space, found.region, camera.view, camera.projection, do_color_management=True)
            return np.array(offscreen.texture_color.read(), dtype=np.uint8).reshape(height, width, 4)[::-1, :, :3]

    def render(camera: Camera) -> NDArray[np.uint8]:
        """Opaque Workbench render through the eye the camera's matrices place, to an RGB PNG at the capture path under the viewport's Solid shading, theme fill, and color management, rows top down, removing each Render Result it creates and keeping each shading enum that reads empty."""
        (width, height), projection = camera.size, np.array(camera.projection)
        eye.matrix_world, data.sensor_fit = camera.view.inverted(), "HORIZONTAL"
        data.shift_x, data.shift_y = ((projection[:2, 2] - projection[:2, 3]) / 2 * (1.0, height / width)).tolist()
        if projection[3, 3] == 0:
            data.sensor_width = max(data.sensor_width, 2 * data.bl_rna.properties["lens"].hard_min / projection[0, 0])
            data.type, data.lens = "PERSP", projection[0, 0] * data.sensor_width / 2
            data.clip_start, data.clip_end = projection[2, 3] / (projection[2, 2] - 1), projection[2, 3] / (projection[2, 2] + 1)
        else:
            near, far = (projection[2, 3] + 1) / projection[2, 2], (projection[2, 3] - 1) / projection[2, 2]
            offset = data.clip_start - near
            eye.matrix_world @= Matrix.Translation((0.0, 0.0, offset))
            data.type, data.ortho_scale, data.clip_end = "ORTHO", 2 / projection[0, 0], far + offset
        settings, output, display, shading = scene.render, scene.render.image_settings, scene.display_settings, scene.display.shading
        display_settings, view_settings = (output.display_settings, output.view_settings) if output.color_management == "OVERRIDE" else (display, scene.view_settings)
        solid = shading if space is None else space.shading
        backdrop = bpy.context.preferences.themes[0].view_3d.space.gradients.high_gradient.copy().from_srgb_to_scene_linear()
        members = {
            shading: {
                **{
                    p.identifier: getattr(solid, p.identifier)
                    for p in solid.bl_rna.properties
                    if not (isinstance(p, bpy.types.PointerProperty) or p.is_readonly) and (not isinstance(p, bpy.types.EnumProperty) or getattr(shading, p.identifier))
                },
                **({"background_type": "VIEWPORT", "background_color": Color([channel ** (1 / 2.2) for channel in backdrop]).from_srgb_to_scene_linear()} if solid.background_type == "THEME" else {}),
            },
            scene: {"camera": eye},
            settings: {
                **dict.fromkeys(("film_transparent", "use_border", "use_stamp", "use_compositing", "use_sequencer"), False),
                "engine": "BLENDER_WORKBENCH",
                "resolution_x": width,
                "resolution_y": height,
                "resolution_percentage": 100,
                "pixel_aspect_x": 1.0,
                "pixel_aspect_y": 1.0,
                "filepath": str(path),
                "dither_intensity": 0.0,
            },
            output: {"media_type": "IMAGE", "file_format": "PNG", "color_mode": "RGB", "color_depth": "8"},
            display_settings: {p.identifier: getattr(display, p.identifier) for p in display.bl_rna.properties if not p.is_readonly},
            view_settings: {
                "view_transform": GetCurrentConfig().getDefaultView(display.display_device),
                "look": "None",
                **{p.identifier: p.default for p in view_settings.bl_rna.properties if isinstance(p, bpy.types.BoolProperty | bpy.types.FloatProperty) and not (p.is_readonly or p.is_array)},
            },
        }
        with assigned(
            *((output, p.identifier, getattr(output, p.identifier)) for p in output.bl_rna.properties if not (isinstance(p, bpy.types.PointerProperty) or p.is_readonly)),
            *(() if output.file_format == "PNG" else ((output.linear_colorspace_settings, "name", output.linear_colorspace_settings.name),)),
            *((block, "hide_render", block.hide_viewport) for block in (scene.collection, *scene.collection.children_recursive)),
            *((o, "hide_render", not (o.visible_get(view_layer=layer, viewport=space) and o.display_type in SHOWN)) for o in layer.objects),
            *((owner, key, value) for owner, values in members.items() for key, value in values.items()),
        ):
            held = set(bpy.data.images)
            try:
                bpy.ops.render.render(write_still=True, scene=scene.name, layer=layer.name)
            finally:
                bpy.data.batch_remove(set(bpy.data.images) - held)
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
        if isinstance(camera := placed(), Fault):
            return Faults.of(camera)
        pixels = render(camera) if live is None else draw(live, camera)
    match previous:
        case None:
            comparison = None
        case _:
            mask = (np.abs(previous.astype(np.int16) - pixels) > threshold * 255).any(axis=-1)
            diff = str(path.with_stem(f"{name}-diff"))
            write(diff, np.where(mask[..., None], (previous.astype(np.uint16) + np.array((255, 0, 0), dtype=np.uint16)) // 2, previous // 2).astype(np.uint8), {})
            projected = np.c_[corners, np.ones(len(corners))] @ np.array(camera.projection @ camera.view).T
            comparison = Difference(int(mask.sum()), diff, bool((np.abs(projected[:, :3]) > projected[:, 3:]).any()))
    record = Capture(str(path), chosen, camera, comparison)
    write(str(path), pixels, {Capture.__name__: JSON.dumps(record)})
    return record


# --- [COMPOSITION] ----------------------------------------------------------------------

JSON.register_unstructure_hook(Matrix, lambda matrix: [list(row) for row in matrix])
JSON.register_structure_hook(Matrix, lambda rows, _: Matrix(rows))

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Camera", "Capture", "Difference", "View", "capture"]
