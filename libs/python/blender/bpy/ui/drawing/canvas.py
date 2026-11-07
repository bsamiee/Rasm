"""GPU drawing in editor regions and offscreen buffers."""

from collections.abc import Callable, Generator
from contextlib import AbstractContextManager, contextmanager, ExitStack
from enum import Enum
from functools import cache, partial
import math
from typing import Final, Literal, Self

import blf
import bpy
from expression import Option, Some
from expression.extra.result import catch
import gpu
from gpu_extras.batch import batch_for_shader
from mathutils import Matrix
import msgspec
import numpy as np
import numpy.typing as npt

from blender.bpy.extension.registration import Resource
from blender.bpy.ui.resources.theme import Checker, CLEAR, Color, Metrics, Theme
from blender.bpy.ui.screens.areas import Located, region_in, visible_bounds
from blender.records.faults import Error, Fault, Nothing, Ok, Outcome, Reason

# --- [TYPES] ----------------------------------------------------------------------------

type Blend = Literal["NONE", "ALPHA", "ALPHA_PREMULT", "ADDITIVE", "ADDITIVE_PREMULT", "MULTIPLY", "SUBTRACT", "INVERT"]
type DrawType = Literal["PRE_VIEW", "POST_VIEW", "POST_PIXEL", "BACKDROP"]
type _DepthTest = Literal["NONE", "ALWAYS", "LESS", "LESS_EQUAL", "EQUAL", "GREATER", "GREATER_EQUAL"]
type _Polyline = Literal["POLYLINE_UNIFORM_COLOR", "POLYLINE_FLAT_COLOR", "POLYLINE_SMOOTH_COLOR"]
type Program = tuple[gpu.types.GPUShader, Callable[[], None]]


class Align(Enum):
    """Label width fraction before its anchor."""

    START = 0.0
    CENTER = 0.5
    END = 1.0


# --- [CONSTANTS] ------------------------------------------------------------------------

QUAD: Final = np.array(((0.0, 0.0), (1.0, 0.0), (0.0, 1.0), (1.0, 1.0)), np.float32)
QUAD.setflags(write=False)
_CLIPPED: Final = frozenset({"UNIFORM_COLOR", "FLAT_COLOR", "SMOOTH_COLOR", "POLYLINE_UNIFORM_COLOR"})
_BLURS: Final = frozenset({3, 5, 6})
UNCLIPPED: Final = partial(gpu.state.clip_distances_set, 0)

DECODED: Final = """
vec3 targeted(vec3 srgb) {
    vec3 c = max(srgb, vec3(0.0));
    return srgbTarget ? mix(c / 12.92, pow((c + 0.055) / 1.055, vec3(2.4)), step(vec3(0.04045), c)) : srgb;
}
"""

# --- [MODELS] ---------------------------------------------------------------------------

# --- [CANVAS]


class Rect(msgspec.Struct, frozen=True, kw_only=True):
    """Axis-aligned rectangle from its lower-left corner."""

    x: float
    y: float
    width: float
    height: float


class Shadow(msgspec.Struct, frozen=True, kw_only=True):
    """Text shadow, offset in device pixels, its level the blur Blender's font shadow types name."""

    color: Color
    offset: tuple[float, float]
    level: int = 0

    @classmethod
    def of(cls, style: bpy.types.ThemeFontStyle) -> Option[Self]:
        """Reads a theme font style's shadow, Nothing for a style drawing none, a type `blf.shadow` lacks drawing unblurred."""
        gray = style.shadow_value
        return (
            Some(cls(level=style.shadow if style.shadow in _BLURS else 0, color=(gray, gray, gray, style.shadow_alpha), offset=(style.shadow_offset_x, style.shadow_offset_y)))
            if style.shadow
            else Nothing
        )


class FontStyle(msgspec.Struct, frozen=True, kw_only=True):
    """Text size in device pixels or a theme font style's, with alignment and `blf` options, `shadow` the theme style's while unstated and `Some(Nothing)` none."""

    size: float | bpy.types.ThemeFontStyle
    align: Align = Align.START
    shadow: Option[Option[Shadow]] = Nothing
    font: int = 0
    wrap: Option[float] = Nothing
    clip: Option[Rect] = Nothing
    rotation: float = 0.0


class Canvas(msgspec.Struct, frozen=True, kw_only=True):
    """Draw state of one region, with the clip planes of a clipping 3D View and the pointer of a cursor drawing."""

    # --- [STATE]

    metrics: Metrics
    extent: Rect
    space: bpy.types.Space
    region: bpy.types.Region
    clipping: Option[gpu.types.GPUUniformBuf] = Nothing
    pointer: Option[tuple[int, int]] = Nothing

    # --- [CONTEXT]

    @classmethod
    def of(cls, area: bpy.types.Area, region: bpy.types.Region, phase: DrawType = "POST_PIXEL", pointer: Option[tuple[int, int]] = Nothing) -> Self:
        """Reads the region's visible extent, in window pixels while a pointer is given, and the clip planes a view phase of a clipping 3D View draws under."""
        space: bpy.types.Space = area.spaces.active  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        x, y, width, height = visible_bounds(area, region)
        origin_x, origin_y = pointer.map(lambda _: (region.x, region.y)).default_value((0, 0))
        match phase, space, region.data:
            case "PRE_VIEW" | "POST_VIEW", bpy.types.SpaceView3D(shading=bpy.types.View3DShading(type="WIREFRAME" | "SOLID")), bpy.types.RegionView3D(use_clip_planes=True) as view:
                values = np.concatenate((np.identity(4, np.float32).ravel(), np.asarray(view.clip_planes, np.float32).ravel()))
                clipping = Some(gpu.types.GPUUniformBuf(gpu.types.Buffer("FLOAT", values.shape, values)))  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
            case _:
                clipping = Nothing
        extent = Rect(x=float(x + origin_x), y=float(y + origin_y), width=float(width), height=float(height))
        return cls(metrics=Metrics.read(), extent=extent, space=space, region=region, clipping=clipping, pointer=pointer)

    @property
    def theme(self) -> Theme:
        """Theme colors of the region drawn in."""
        return Theme(space=self.space.type, region=self.region.type, preferences=self.metrics.preferences)

    # --- [SCOPES]

    @contextmanager
    def drawn(self) -> Generator[Self]:
        """Scopes matrices and alpha blending, establishing enabled scissor and clip count zero on entry and exit."""
        with ExitStack() as stack:
            stack.callback(UNCLIPPED)
            stack.callback(gpu.state.scissor_test_set, True)  # ruff: ignore[boolean-positional-value-in-call]
            UNCLIPPED()
            gpu.state.scissor_test_set(True)  # ruff: ignore[boolean-positional-value-in-call]
            stack.enter_context(gpu.matrix.push_pop())  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
            stack.enter_context(gpu.matrix.push_pop_projection())  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
            stack.enter_context(Canvas.blended("ALPHA"))
            yield self

    @staticmethod
    def blended(mode: Blend) -> AbstractContextManager[None]:
        """Scopes blend mode, restoring the mode it found."""
        return _restored(gpu.state.blend_get, gpu.state.blend_set, mode)  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]

    @staticmethod
    def depth_tested(mode: _DepthTest = "LESS_EQUAL") -> AbstractContextManager[None]:
        """Scopes the depth test, by default the test a view drawing hides behind scene geometry with, restoring the test it found."""
        return _restored(gpu.state.depth_test_get, gpu.state.depth_test_set, mode)  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]

    @staticmethod
    @contextmanager
    def mapped(rect: Rect) -> Generator[None]:
        """Scopes a model matrix mapping the unit square onto a rectangle."""
        with ExitStack() as stack:
            stack.enter_context(gpu.matrix.push_pop())  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
            gpu.matrix.translate((rect.x, rect.y))
            gpu.matrix.scale((rect.width, rect.height))
            yield

    @staticmethod
    @contextmanager
    def clipped(rect: Rect, *, view: Option[bpy.types.View2D] = Nothing) -> Generator[None]:
        """Narrows an enabled scissor to a pixel or View2D rectangle, restoring its box.

        The caller owns the enabled test boundary, established by Canvas.drawn.
        """
        corners = ((rect.x, rect.y), (rect.x + rect.width, rect.y + rect.height))
        match view:
            case Option(tag="some", some=held):
                (left, bottom), (right, top) = (held.view_to_region(*corner, clip=False) for corner in corners)  # type: ignore[misc]
            case _:
                (left, bottom), (right, top) = ((round(x), round(y)) for x, y in corners)
        held_x, held_y, held_width, held_height = found = gpu.state.scissor_get()
        x, y = max(held_x, left), max(held_y, bottom)
        right, top = min(held_x + held_width, right), min(held_y + held_height, top)
        with ExitStack() as stack:
            stack.callback(gpu.state.scissor_set, *found)
            gpu.state.scissor_set(x, y, max(0, right - x), max(0, top - y))
            yield

    # --- [SHADERS]

    def shader(self, name: str) -> Program:
        """Returns a built-in program with its declared clipping configuration."""
        match self.clipping, self.region.data:
            case Option(tag="some", some=planes), bpy.types.RegionView3D() as view if name in _CLIPPED:
                shader = gpu.shader.from_builtin(name, config="CLIPPED")

                def setup() -> None:
                    shader.uniform_block("clip_planes", planes)
                    shader.uniform_float("ClipModelMatrix", view.view_matrix.inverted() @ gpu.matrix.get_model_view_matrix())  # type: ignore[arg-type]
                    gpu.state.clip_distances_set(len(view.clip_planes))

                return shader, setup
            case _:
                return gpu.shader.from_builtin(name), UNCLIPPED

    def lines(self, width: float, shader: _Polyline = "POLYLINE_UNIFORM_COLOR") -> Program:
        """Prepares a polyline program at device-pixel width."""
        bound, clip = self.shader(shader)

        def setup() -> None:
            bound.uniform_float("viewportSize", gpu.state.viewport_get()[2:])
            bound.uniform_float("lineWidth", self.metrics.line(width))
            bound.uniform_bool("lineSmooth", True)  # ruff: ignore[boolean-positional-value-in-call]
            clip()

        return bound, setup

    def points(self) -> Program:
        """Prepares the flat point program at device pixel size, the point state overriding its size uniform."""
        shader, clip = self.shader("POINT_FLAT_COLOR")

        def setup() -> None:
            gpu.state.point_size_set(float(self.metrics.pixel_size))
            clip()

        return shader, setup

    # --- [TEXT]

    def measured(self, text: str, style: FontStyle) -> tuple[float, float]:
        """Returns the width and height text takes in a font style."""
        with _typeset(self.metrics, style):
            return blf.dimensions(style.font, text)

    def label(self, text: str, at: tuple[float, float], color: Color, style: FontStyle) -> None:
        """Draws text as given in a font style under the blend mode it found."""
        font, x, y = style.font, *at
        with ExitStack() as stack, _typeset(self.metrics, style):
            stack.callback(gpu.state.blend_set, gpu.state.blend_get())  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
            reach = blf.dimensions(font, text)[0] * style.align.value
            blf.color(font, *color)
            blf.position(font, x - reach * math.cos(style.rotation), y - reach * math.sin(style.rotation), 0.0)
            UNCLIPPED()
            blf.draw(font, text)

    # --- [PRIMITIVES]

    def frame(self, rect: Rect, fill: Color, edge: Color, radius: float) -> None:
        """Draws antialiased rounded frame."""
        shader, quad = _rounded()
        values = (rect.x, rect.y, rect.width, rect.height, *fill, *edge, radius, self.metrics.line(1.0), 0.0, 0.0)
        box = gpu.types.GPUUniformBuf(gpu.types.Buffer("FLOAT", len(values), values))
        with Canvas.blended("ALPHA_PREMULT"):
            draw_batch(quad, configured((shader, UNCLIPPED), partial(shader.uniform_block, "box", box)))

    @staticmethod
    def picture(source: gpu.types.GPUTexture | bpy.types.Image, rect: Rect, *, blend: Blend, color: Color = (1.0, 1.0, 1.0, 1.0)) -> None:
        """Draws display colors from a texture, or interprets an image's cached GPU values as scene linear through the Rec.709 display shader, under a tint and blend."""
        match source:
            case bpy.types.Image():
                shader, texture = gpu.shader.from_builtin("IMAGE_COLOR_SCENE_LINEAR_TO_REC709_SRGB"), gpu.texture.from_image(source)
            case _:
                shader, texture = gpu.shader.from_builtin("IMAGE_COLOR"), source
        with Canvas.mapped(rect), Canvas.blended(blend):
            draw_batch(_textured(), configured((shader, UNCLIPPED), partial(shader.uniform_sampler, "image", texture), partial(shader.uniform_float, "color", color)))

    def checkered(self, rect: Rect, checker: Checker) -> None:
        """Draws the transparency checker as indexed cell quads, clipping edge cells to the rectangle, an empty rectangle holding no cell to draw."""
        size = float(checker.cell)
        left, bottom = np.meshgrid(rect.x + np.arange(math.ceil(rect.width / size)) * size, rect.y + np.arange(math.ceil(rect.height / size)) * size)
        if not left.size:
            return
        right, top = np.minimum(left + size, rect.x + rect.width), np.minimum(bottom + size, rect.y + rect.height)
        corners = np.stack((left, bottom, right, bottom, right, top, left, top), axis=-1).reshape(-1, 2).astype(np.float32)
        rows, columns = np.indices(left.shape, sparse=True)
        odd = ((rows + columns) % 2).reshape(-1, 1).astype(bool)
        tints = np.repeat(np.where(odd, np.asarray(checker.primary, np.float32), np.asarray(checker.secondary, np.float32)), 4, axis=0)
        indices = (np.arange(left.size, dtype=np.uint32)[:, None, None] * 4 + np.array(((0, 1, 2), (0, 2, 3)), np.uint32)).reshape(-1, 3)
        program = self.shader("FLAT_COLOR")
        draw_batch(batch_for_shader(program[0], "TRIS", {"pos": corners, "color": tints}, indices=indices), program)  # type: ignore[arg-type, dict-item]  # ty: ignore[invalid-argument-type]


# --- [OFFSCREENS]


class Offscreen(msgspec.Struct, frozen=True, kw_only=True):
    """Owned 2D color and depth attachments at caller device-pixel size and precision."""

    width: int
    height: int
    format: Literal["RGBA8", "RGBA16", "RGBA16F", "RGBA32F"] = "RGBA8"

    def read(self, draw: Callable[[gpu.types.GPUFrameBuffer], Outcome[None]]) -> Outcome[npt.NDArray[np.float32]]:
        """Draws into an owned framebuffer and reads top-first RGBA, restoring caller binding, viewport and scissor."""

        def opened() -> Outcome[tuple[gpu.types.GPUTexture, gpu.types.GPUTexture, gpu.types.GPUFrameBuffer]]:
            try:
                color = gpu.types.GPUTexture((max(1, self.width), max(1, self.height)), format=self.format)
                depth = gpu.types.GPUTexture((color.width, color.height), format="DEPTH32F_STENCIL8")
                buffer = gpu.types.GPUFrameBuffer(depth_slot=depth, color_slots=color)
            except (TypeError, RuntimeError) as error:
                return Error(Fault.of(Reason.REFUSED, type(self).__name__, error))
            return Ok((color, depth, buffer))

        def drawn(target: tuple[gpu.types.GPUTexture, gpu.types.GPUTexture, gpu.types.GPUFrameBuffer]) -> Outcome[npt.NDArray[np.float32]]:
            color, _, buffer = target
            with ExitStack() as stack:
                stack.callback(gpu.state.viewport_set, *gpu.state.viewport_get())
                stack.callback(gpu.state.scissor_set, *gpu.state.scissor_get())
                with buffer.bind():  # type: ignore[attr-defined,func-returns-value]  # ty: ignore[invalid-context-manager]
                    buffer.viewport_set(0, 0, color.width, color.height)
                    gpu.state.scissor_set(0, 0, color.width, color.height)
                    return draw(buffer).bind(lambda _: captured(Rect(x=0.0, y=0.0, width=float(color.width), height=float(color.height))))

        return catch(gpu.init, exception=SystemError)().map_error(partial(Fault.of, Reason.REFUSED, "gpu.init")).bind(lambda _: opened()).bind(drawn)

    def drawn(self, canvas: Canvas, draw: Callable[[Canvas], None]) -> Outcome[npt.NDArray[np.float32]]:
        """Draws a cleared pixel canvas under pixel projection with depth test off, returning top-first RGBA."""

        def painted(buffer: gpu.types.GPUFrameBuffer) -> Outcome[None]:
            _, _, width, height = buffer.viewport_get()
            pixels = msgspec.structs.replace(canvas, extent=Rect(x=0.0, y=0.0, width=float(width), height=float(height)), clipping=Nothing, pointer=Nothing)
            with pixels.drawn(), Canvas.depth_tested("NONE"):
                buffer.clear(color=CLEAR, depth=1.0)
                gpu.matrix.load_identity()
                gpu.matrix.load_projection_matrix(Matrix.Translation((-1.0, -1.0, 0.0)) @ Matrix.Diagonal((2.0 / width, 2.0 / height, 1.0, 1.0)))
                draw(pixels)
            return Ok(None)

        return self.read(painted)


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [DRAWING]


def draw_batch(batch: gpu.types.GPUBatch, program: Program) -> None:
    """Applies a program's native bindings and draws its batch.

    Args:
        batch: Native geometry to draw.
        program: Shader and setup replaying its native resources, uniforms and clip count.
    """
    shader, setup = program
    setup()
    batch.draw(shader)


def configured(program: Program, *uniforms: Callable[[], None]) -> Program:
    """Adds native uniform bindings replayed before a program's setup at every draw.

    Args:
        program: Native shader and its existing setup.
        *uniforms: Native setters with their arguments bound.

    Returns:
        Program retaining those bindings and their resources for each draw.
    """
    shader, previous = program

    def setup() -> None:
        for uniform in uniforms:
            uniform()
        previous()

    return shader, setup


@contextmanager
def _restored[T](read: Callable[[], T], write: Callable[[T], None], value: T) -> Generator[None]:
    """Scopes one GPU state value, restoring its prior value on every exit."""
    with ExitStack() as stack:
        stack.callback(write, read())
        write(value)
        yield


@contextmanager
def _typeset(metrics: Metrics, style: FontStyle) -> Generator[None]:
    """Sets font size, a theme font style's at interface scale, and enables the shadow, wrap, clip, and rotation given, disabling each at scope exit."""
    font, size, wrap, clip, rotation = style.font, style.size, style.wrap, style.clip, style.rotation
    shadow = style.shadow.default_with(lambda: Shadow.of(size) if isinstance(size, bpy.types.ThemeFontStyle) else Nothing)
    blf.size(font, metrics.font(size) if isinstance(size, bpy.types.ThemeFontStyle) else size)
    with ExitStack() as stack:
        for shade in shadow.to_list():
            blf.shadow(font, shade.level, *shade.color)
            blf.shadow_offset(font, round(shade.offset[0]), round(shade.offset[1]))
        for width in wrap.to_list():
            blf.word_wrap(font, round(width))
        for bounds in clip.to_list():
            blf.clipping(font, bounds.x, bounds.y, bounds.x + bounds.width, bounds.y + bounds.height)
        blf.rotation(font, rotation)
        for option in (flag for flag, held in ((blf.SHADOW, shadow.is_some()), (blf.WORD_WRAP, wrap.is_some()), (blf.CLIPPING, clip.is_some()), (blf.ROTATION, rotation != 0.0)) if held):
            blf.enable(font, option)
            stack.callback(blf.disable, font, option)
        yield


# --- [READS]


def viewed(
    buffer: gpu.types.GPUOffScreen,
    located: Located,
    camera: bpy.types.Object,
    scene: bpy.types.Scene,
    view_layer: bpy.types.ViewLayer,
    *,
    do_color_management: bool = True,
    draw_background: bool = True,
) -> Outcome[npt.NDArray[np.float32]]:
    """Captures a camera view through a caller-owned native offscreen allocated at a default-framebuffer boundary.

    Args:
        buffer: Native offscreen whose allocation and release the caller owns.
        located: 3D View and optional main region to draw through.
        camera: Object supplying the view and projection matrices.
        scene: Scene to draw.
        view_layer: View layer supplying its dependency graph.
        do_color_management: Whether native viewport drawing applies color management.
        draw_background: Whether the native view draws its background.

    Returns:
        Top-first RGBA at the native resource's dimensions, or editor, transform and drawing faults.
    """
    space = located.area.spaces.active
    if not isinstance(space, bpy.types.SpaceView3D):
        return Error(Fault.of(Reason.ABSENT, repr(located.area), member=bpy.types.SpaceView3D.__name__))

    def painted(region: bpy.types.Region, view: Matrix) -> Outcome[npt.NDArray[np.float32]]:
        overlay = space.overlay
        with ExitStack() as stack:
            stack.callback(setattr, overlay, "show_overlays", overlay.show_overlays)
            stack.callback(gpu.state.viewport_set, *gpu.state.viewport_get())
            stack.callback(gpu.state.scissor_set, *gpu.state.scissor_get())
            stack.enter_context(buffer.bind())  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
            overlay.show_overlays = False
            return (
                catch(buffer.draw_view3d, exception=RuntimeError)(
                    scene,
                    view_layer,
                    space,
                    region,
                    view,
                    camera.calc_matrix_camera(view_layer.depsgraph, x=buffer.width, y=buffer.height),
                    do_color_management=do_color_management,
                    draw_background=draw_background,
                )
                .map_error(partial(Fault.of, Reason.REFUSED, "gpu.types.GPUOffScreen.draw_view3d"))
                .bind(lambda _: captured(Rect(x=0.0, y=0.0, width=float(buffer.width), height=float(buffer.height))))
            )

    return (
        located.region
        .map(Ok)
        .default_with(lambda: region_in(located.area))
        .bind(
            lambda region: (
                catch(camera.matrix_world.normalized().inverted, exception=ValueError)().map_error(partial(Fault.of, Reason.REFUSED, "mathutils.Matrix.inverted")).bind(partial(painted, region))
            )
        )
    )


def captured(rect: Rect) -> Outcome[npt.NDArray[np.float32]]:
    """Reads normalized integer or stored floating RGBA as top-first float32 from the active framebuffer.

    Args:
        rect: Pixel rectangle of the active framebuffer.

    Returns:
        Top-first float32 RGBA plane, refusal fault for a rectangle the framebuffer cannot read.
    """
    return (
        catch(gpu.state.active_framebuffer_get().read_color, exception=ValueError)(round(rect.x), round(rect.y), round(rect.width), round(rect.height), 4, 0, "FLOAT")
        .map(lambda read: np.ascontiguousarray(np.asarray(read, np.float32)[::-1]))
        .map_error(partial(Fault.of, Reason.REFUSED, "gpu.types.GPUFrameBuffer.read_color"))
    )


# --- [PROGRAMS]


@cache
def _rounded() -> tuple[gpu.types.GPUShader, gpu.types.GPUBatch]:
    """Builds rounded-frame shader and unit quad."""
    vertex = "void main() { local = pos * box.rect.zw; gl_Position = ModelViewProjectionMatrix * vec4(box.rect.xy + local, 0.0, 1.0); }"
    fragment = (
        DECODED
        + """
void main() {
    vec2 half_size = 0.5 * box.rect.zw;
    vec2 q = abs(local - half_size) - half_size + box.widths.x;
    float d = length(max(q, 0.0)) + min(max(q.x, q.y), 0.0) - box.widths.x;
    float pixel = fwidth(d);
    float inside = clamp(0.5 - d / pixel, 0.0, 1.0);
    float rim = clamp(0.5 + (d + box.widths.y) / pixel, 0.0, 1.0) * box.edge.a;
    float body = box.fill.a * (1.0 - rim);
    fragColor = vec4(targeted(box.edge.rgb) * rim + targeted(box.fill.rgb) * body, rim + body) * inside;
}
"""
    )
    interface = gpu.types.GPUStageInterfaceInfo("rasm_rounded")
    interface.smooth("VEC2", "local")
    info = gpu.types.GPUShaderCreateInfo()
    info.typedef_source("struct RoundedBox { vec4 rect; vec4 fill; vec4 edge; vec4 widths; };")
    info.uniform_buf(0, "RoundedBox", "box")
    info.vertex_in(0, "VEC2", "pos")
    info.vertex_out(interface)
    info.push_constant("MAT4", "ModelViewProjectionMatrix")
    info.push_constant("BOOL", "srgbTarget")
    info.fragment_out(0, "VEC4", "fragColor")
    info.vertex_source(vertex)
    info.fragment_source(fragment)
    shader = gpu.shader.create_from_info(info)
    return shader, batch_for_shader(shader, "TRI_STRIP", {"pos": QUAD})  # type: ignore[dict-item]  # ty: ignore[invalid-argument-type]


@cache
def _textured() -> gpu.types.GPUBatch:
    """Returns UV quad for image shaders."""
    return batch_for_shader(gpu.shader.from_builtin("IMAGE_COLOR"), "TRI_STRIP", {"pos": QUAD, "texCoord": QUAD})  # type: ignore[dict-item]  # ty: ignore[invalid-argument-type]


def programs(*clears: Callable[[], None]) -> Resource[None]:
    """Holds memoized GPU programs while an extension's registration stands.

    Args:
        *clears: `cache_clear` of each program builder under `functools.cache`.

    Returns:
        Resource dropping every builder's program at release, inside the GPU context unregistration runs in.
    """

    @contextmanager
    def cleared(_identity: object) -> Generator[None]:
        """Clears each builder's memoized program at scope exit."""
        with ExitStack() as stack:
            for clear in clears:
                stack.callback(clear)
            stack.callback(gpu.shader.unbind)
            yield

    return Resource[None](made=cleared)


# --- [COMPOSITION] ----------------------------------------------------------------------

PROGRAMS: Final = programs(_rounded.cache_clear, _textured.cache_clear)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "DECODED",
    "PROGRAMS",
    "QUAD",
    "UNCLIPPED",
    "Align",
    "Blend",
    "Canvas",
    "DrawType",
    "FontStyle",
    "Offscreen",
    "Program",
    "Rect",
    "Shadow",
    "captured",
    "configured",
    "draw_batch",
    "programs",
    "viewed",
]
