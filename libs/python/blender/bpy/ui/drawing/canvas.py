"""Region draw state, text, built-in shader binds and batches, cached GPU programs, charts in the colors of Blender's scopes, and offscreen buffers with framebuffer reads."""

from collections.abc import Callable, Generator, Mapping
from contextlib import AbstractContextManager, contextmanager, ExitStack
from enum import Enum
import functools
from functools import partial
import math
from typing import Final, Literal, Self

import blf
import bpy
from expression import Error, Nothing, Ok, Option, Some
from expression.extra.result import catch
import gpu
from gpu_extras.batch import batch_for_shader
from mathutils import Matrix
import msgspec
import numpy as np
import numpy.typing as npt

from blender.bpy.extension.identity import Resource
from blender.bpy.ui.resources.theme import Checker, CLEAR, Color, Metrics, Spacing, Theme
from blender.bpy.ui.screens.areas import EditorState, Located, region_in, visible_bounds
from blender.records.faults import Fault, Outcome, paired, Reason

# --- [TYPES] ----------------------------------------------------------------------------

type Blend = Literal["NONE", "ALPHA", "ALPHA_PREMULT", "ADDITIVE", "ADDITIVE_PREMULT", "MULTIPLY", "SUBTRACT", "INVERT"]
type DrawType = Literal["PRE_VIEW", "POST_VIEW", "POST_PIXEL", "BACKDROP"]
type _DepthTest = Literal["NONE", "ALWAYS", "LESS", "LESS_EQUAL", "EQUAL", "GREATER", "GREATER_EQUAL"]
type _Polyline = Literal["POLYLINE_UNIFORM_COLOR", "POLYLINE_FLAT_COLOR", "POLYLINE_SMOOTH_COLOR"]
type _Primitive = Literal["POINTS", "LINES", "TRIS", "LINE_STRIP", "TRI_STRIP", "LINES_ADJ", "TRIS_ADJ", "LINE_STRIP_ADJ"]
type _Mark = Literal["LINES", "LINE_STRIP", "POINTS"]


class Align(Enum):
    """Label width fraction before its anchor."""

    START = 0.0
    CENTER = 0.5
    END = 1.0


# --- [CONSTANTS] ------------------------------------------------------------------------

QUAD: Final = np.array(((0.0, 0.0), (1.0, 0.0), (0.0, 1.0), (1.0, 1.0)), np.float32)
QUAD.setflags(write=False)
FINE: Final[Color] = (1.0, 1.0, 1.0, 0.08)
GRID: Final[Color] = (1.0, 1.0, 1.0, 0.2)
MAJOR: Final[Color] = (1.0, 1.0, 1.0, 0.5)
TRACE: Final[Color] = (1.0, 1.0, 1.0, 0.75)
_GROUND: Final[Color] = (0.0, 0.0, 0.0, 1.0)
_RIM: Final[Color] = (0.0, 0.0, 0.0, 0.5)
_TEXT: Final[Color] = (1.0, 1.0, 1.0, 0.2)
_TEXT_POINTS: Final = 8.0
_RADIUS: Final = 3.0
_PUSH_POP: Final[Callable[[], AbstractContextManager[None]]] = gpu.matrix.push_pop  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
_PUSH_POP_PROJECTION: Final[Callable[[], AbstractContextManager[None]]] = gpu.matrix.push_pop_projection  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
_BATCH_FOR_SHADER: Final[Callable[[gpu.types.GPUShader, _Primitive, Mapping[str, npt.NDArray[np.float32]]], gpu.types.GPUBatch]] = batch_for_shader  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
_BLEND_GET: Final[Callable[[], Blend]] = gpu.state.blend_get  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
_DEPTH_TEST_GET: Final[Callable[[], _DepthTest]] = gpu.state.depth_test_get  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
_VIEWED: Final[frozenset[DrawType]] = frozenset({"PRE_VIEW", "POST_VIEW"})
_CLIPPED: Final = frozenset({"UNIFORM_COLOR", "FLAT_COLOR", "SMOOTH_COLOR", "POLYLINE_UNIFORM_COLOR"})
_BLURS: Final = frozenset({3, 5, 6})

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
        """Reads a theme font style's shadow, Nothing for a style drawing none, as `fontstyle_draw_ex` reads it, a type `blf.shadow` lacks drawing unblurred."""
        gray = style.shadow_value
        return (
            Some(cls(level=style.shadow if style.shadow in _BLURS else 0, color=(gray, gray, gray, style.shadow_alpha), offset=(style.shadow_offset_x, style.shadow_offset_y)))
            if style.shadow
            else Nothing
        )


class Canvas(msgspec.Struct, frozen=True, kw_only=True):
    """Draw state of one region: interface metrics, visible extent, the editor and region drawn in, the clip planes of a clipping 3D View while drawing in its world space, and the window pointer a cursor drawing draws at."""

    metrics: Metrics
    extent: Rect
    space: bpy.types.Space
    region: bpy.types.Region
    clipping: Option[gpu.types.GPUUniformBuf] = Nothing
    pointer: Option[tuple[int, int]] = Nothing

    # --- [CONTEXT]

    @classmethod
    def of(cls, area: bpy.types.Area, region: bpy.types.Region, phase: DrawType = "POST_PIXEL", pointer: Option[tuple[int, int]] = Nothing) -> Self:
        """Reads region extent less overlapping siblings, in window pixels while a pointer is given, with the clip planes a view phase of a 3D View clipping its solid or wireframe shading draws under, by Blender's `RV3D_CLIPPING_ENABLED`."""
        space: bpy.types.Space = area.spaces.active  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        x, y, width, height = visible_bounds(area, region)
        origin_x, origin_y = pointer.map(lambda _: (region.x, region.y)).default_value((0, 0))
        match phase in _VIEWED, space, region.data:
            case True, bpy.types.SpaceView3D(shading=bpy.types.View3DShading(type="WIREFRAME" | "SOLID")), bpy.types.RegionView3D(use_clip_planes=True) as view:
                clipping = Some(gpu.types.GPUUniformBuf(gpu.types.Buffer("FLOAT", 40, np.concatenate((np.identity(4, np.float32).ravel(), np.asarray(view.clip_planes, np.float32).ravel())))))  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
            case _:
                clipping = Nothing
        extent = Rect(x=float(x + origin_x), y=float(y + origin_y), width=float(width), height=float(height))
        return cls(metrics=Metrics.read(), extent=extent, space=space, region=region, clipping=clipping, pointer=pointer)

    @property
    def theme(self) -> Theme:
        """Theme colors of the region drawn in."""
        return Theme(space=self.space.type, region=self.region.type, preferences=self.metrics.preferences)

    # --- [STATE]

    @contextmanager
    def drawn(self) -> Generator[Self]:
        """Scopes matrix stacks and alpha blending."""
        with _PUSH_POP(), _PUSH_POP_PROJECTION(), Canvas.blended("ALPHA"):
            yield self

    @staticmethod
    def blended(mode: Blend) -> AbstractContextManager[None]:
        """Scopes blend mode, restoring the mode it found."""
        return _restored(_BLEND_GET, gpu.state.blend_set, mode)

    @staticmethod
    def depth_tested(mode: _DepthTest = "LESS_EQUAL") -> AbstractContextManager[None]:
        """Scopes the depth test, by default the test a view drawing hides behind scene geometry with, restoring the test it found."""
        return _restored(_DEPTH_TEST_GET, gpu.state.depth_test_set, mode)

    @staticmethod
    @contextmanager
    def mapped(rect: Rect) -> Generator[None]:
        """Scopes a model matrix mapping the unit square onto a rectangle."""
        with _PUSH_POP():
            gpu.matrix.translate((rect.x, rect.y))
            gpu.matrix.scale((rect.width, rect.height))
            yield

    @staticmethod
    @contextmanager
    def clipped(rect: Rect, *, view: Option[bpy.types.View2D] = Nothing) -> Generator[None]:
        """Narrows scissor to a pixel or View2D rectangle under the scissor test, which region drawing outside viewports already holds on, restoring the box it found."""
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
            gpu.state.scissor_test_set(True)
            gpu.state.scissor_set(x, y, max(0, right - x), max(0, top - y))
            yield

    # --- [SHADERS]

    @contextmanager
    def shader(self, name: str) -> Generator[gpu.types.GPUShader]:
        """Binds a built-in shader for the scope, under held clip planes its clipped configuration with six clip distances and the model matrix the view matrix leaves of the stack, the configuration only uniform, flat, smooth, and uniform polyline shaders hold."""
        with ExitStack() as stack:
            match self.clipping, self.region.data:
                case Option(tag="some", some=planes), bpy.types.RegionView3D() as view if name in _CLIPPED:
                    shader = gpu.shader.from_builtin(name, config="CLIPPED")
                    shader.bind()
                    shader.uniform_block("clip_planes", planes)
                    shader.uniform_float("ClipModelMatrix", view.view_matrix.inverted_safe() @ gpu.matrix.get_model_view_matrix())  # type: ignore[arg-type]
                    gpu.state.clip_distances_set(6)
                    stack.callback(gpu.state.clip_distances_set, 0)
                case _:
                    shader = gpu.shader.from_builtin(name)
                    shader.bind()
            yield shader

    @contextmanager
    def lines(self, width: float, shader: _Polyline = "POLYLINE_UNIFORM_COLOR") -> Generator[gpu.types.GPUShader]:
        """Binds a polyline shader at device-pixel width for the scope."""
        with self.shader(shader) as bound:
            bound.uniform_float("viewportSize", gpu.state.viewport_get()[2:])
            bound.uniform_float("lineWidth", self.metrics.line(width))
            bound.uniform_bool("lineSmooth", True)
            yield bound

    @contextmanager
    def points(self) -> Generator[gpu.types.GPUShader]:
        """Binds the flat point shader for the scope at device pixel size, the point size state overriding its size uniform."""
        gpu.state.point_size_set(float(self.metrics.pixel_size))
        with self.shader("POINT_FLAT_COLOR") as bound:
            yield bound

    @staticmethod
    def batched(name: str, primitive: _Primitive, attributes: Mapping[str, npt.NDArray[np.float32]]) -> gpu.types.GPUBatch:
        """Batches vertex attributes in the format a built-in shader reads, its clipped configuration reading the same format."""
        return _BATCH_FOR_SHADER(gpu.shader.from_builtin(name), primitive, attributes)

    # --- [TEXT]

    def measured(self, text: str, size: float | bpy.types.ThemeFontStyle, *, font: int = 0, wrap: Option[float] = Nothing) -> tuple[float, float]:
        """Returns the width and height text takes at a device-pixel size or a theme font style's scaled size, wrapped to a width when given."""
        with _typeset(self.metrics, font, size, Nothing, wrap, Nothing, 0.0):
            return blf.dimensions(font, text)

    def label(  # ruff: ignore[too-many-arguments]
        self,
        text: str,
        at: tuple[float, float],
        color: Color,
        size: float | bpy.types.ThemeFontStyle,
        align: Align = Align.START,
        shadow: Option[Option[Shadow]] = Nothing,
        *,
        font: int = 0,
        wrap: Option[float] = Nothing,
        clip: Option[Rect] = Nothing,
        rotation: float = 0.0,
    ) -> None:
        """Draws text as given under the blend mode it found, aligned along its rotation, a theme font style giving its scaled size and its shadow unless one is stated, `Some(Nothing)` drawing none."""
        styled = shadow.default_with(lambda: Shadow.of(size) if isinstance(size, bpy.types.ThemeFontStyle) else Nothing)
        x, y = at
        with Canvas.blended(_BLEND_GET()), _typeset(self.metrics, font, size, styled, wrap, clip, rotation):
            reach = blf.dimensions(font, text)[0] * align.value
            blf.color(font, *color)
            blf.position(font, x - reach * math.cos(rotation), y - reach * math.sin(rotation), 0.0)
            blf.draw(font, text)

    # --- [PRIMITIVES]

    def frame(self, rect: Rect, fill: Color, edge: Color, radius: float) -> None:
        """Draws antialiased rounded frame."""
        shader, quad = _rounded()
        shader.bind()
        box = gpu.types.GPUUniformBuf(gpu.types.Buffer("FLOAT", 16, (rect.x, rect.y, rect.width, rect.height, *fill, *edge, radius, self.metrics.line(1.0), 0.0, 0.0)))
        shader.uniform_block("box", box)
        with Canvas.blended("ALPHA_PREMULT"):
            quad.draw(shader)

    @staticmethod
    def picture(source: gpu.types.GPUTexture | bpy.types.Image, rect: Rect, *, blend: Blend, color: Color = (1.0, 1.0, 1.0, 1.0)) -> None:
        """Draws a texture of display colors, or an image's cached scene-linear texture through the Rec.709 display shader, tinted under a blend mode."""
        match source:
            case bpy.types.Image():
                shader, texture = gpu.shader.from_builtin("IMAGE_COLOR_SCENE_LINEAR_TO_REC709_SRGB"), gpu.texture.from_image(source)
            case _:
                shader, texture = gpu.shader.from_builtin("IMAGE_COLOR"), source
        shader.bind()
        shader.uniform_sampler("image", texture)
        shader.uniform_float("color", color)
        with Canvas.mapped(rect), Canvas.blended(blend):
            _textured().draw(shader)

    def checkered(self, rect: Rect, checker: Checker) -> None:
        """Draws the transparency checker over a rect as one flat-colored triangle batch, edge squares clipped to the rect."""
        size = float(checker.cell)
        left, bottom = np.meshgrid(rect.x + np.arange(math.ceil(rect.width / size)) * size, rect.y + np.arange(math.ceil(rect.height / size)) * size)
        right, top = np.minimum(left + size, rect.x + rect.width), np.minimum(bottom + size, rect.y + rect.height)
        corners = np.stack((left, bottom, right, bottom, right, top, left, bottom, right, top, left, top), axis=-1).reshape(-1, 2).astype(np.float32)
        odd = (np.indices(left.shape).sum(axis=0) % 2).reshape(-1, 1).astype(bool)
        tints = np.repeat(np.where(odd, np.asarray(checker.primary, np.float32), np.asarray(checker.secondary, np.float32)), 6, axis=0)
        with self.shader("FLAT_COLOR") as shader:
            Canvas.batched("FLAT_COLOR", "TRIS", {"pos": corners, "color": tints}).draw(shader)


# --- [CHARTS]


class Layer(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Chart geometry batched once in plot fractions, drawn under its blend at its stroke width in points."""

    primitive: _Mark
    batch: gpu.types.GPUBatch
    blend: Blend
    width: float

    @classmethod
    def of(cls, primitive: _Mark, positions: npt.NDArray[np.float32], color: Color | npt.NDArray[np.float32], *, blend: Blend = "ALPHA", width: float = 1.0) -> Self:
        """Batches `(N, 2)` plot-fraction positions in one color or per-vertex `(N, 4)` colors."""
        colors = np.ascontiguousarray(np.broadcast_to(np.asarray(color, np.float32), (len(positions), 4)))
        batch = Canvas.batched("POINT_FLAT_COLOR" if primitive == "POINTS" else "POLYLINE_FLAT_COLOR", primitive, {"pos": np.ascontiguousarray(positions, np.float32), "color": colors})
        return cls(primitive=primitive, batch=batch, blend=blend, width=width)

    def drawn(self, canvas: Canvas) -> None:
        """Draws the batch under its blend."""
        with Canvas.blended(self.blend), canvas.points() if self.primitive == "POINTS" else canvas.lines(self.width, "POLYLINE_FLAT_COLOR") as shader:
            self.batch.draw(shader)


class Label(msgspec.Struct, frozen=True, kw_only=True):
    """Text anchored at a plot fraction, offset half a line into the plot."""

    text: str
    x: float
    y: float
    align: Align = Align.START


class Chart(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Plot of a size in device pixels in the well, rim, and text colors of Blender's scopes, which no theme member holds (`interface_draw.cc` `draw_but_HISTOGRAM`, `draw_scope_end`, `draw_but_WAVEFORM`), the well one pixel wider each side.

    Layers draw in order clipped to the plot, labels at plot fractions, and caption lines stack down from the top on the corner's side.
    """

    width: float
    height: float
    layers: tuple[Layer, ...] = ()
    labels: tuple[Label, ...] = ()
    caption: tuple[str, ...] = ()
    corner: Align = Align.START

    def drawn(self, canvas: Canvas) -> None:
        """Draws the chart at its lower corner of the canvas extent, inset by Blender's panel margin, text at the scopes' label size."""
        metrics, extent = canvas.metrics, canvas.extent
        size, inset = float(metrics.scaled(_TEXT_POINTS)), metrics.unit * 0.4
        x = extent.x + inset + (extent.width - 2.0 * inset - self.width - 2.0) * self.corner.value
        y = extent.y + inset
        pitch = size + metrics.spaced(Spacing.BUTTON_Y)
        with Canvas.mapped(Rect(x=x, y=y, width=1.0, height=1.0)):
            canvas.frame(Rect(x=0.0, y=0.0, width=self.width + 2.0, height=self.height + 2.0), _GROUND, CLEAR, _RADIUS)
            with canvas.clipped(Rect(x=x + 1.0, y=y + 1.0, width=self.width, height=self.height)), Canvas.mapped(Rect(x=1.0, y=1.0, width=self.width, height=self.height)):
                for layer in self.layers:
                    layer.drawn(canvas)
            for label in self.labels:
                canvas.label(label.text, (1.0 + label.x * self.width + size / 2 * (1 - 2 * label.align.value), 1.0 + label.y * self.height + size / 2), _TEXT, size, label.align)
            for index, line in enumerate(self.caption, 1):
                canvas.label(line, (1.0 + size / 2 + (self.width - size) * self.corner.value, 1.0 + self.height - size / 2 - index * pitch), _TEXT, size, self.corner)
            canvas.frame(Rect(x=0.0, y=1.0, width=self.width + 2.0, height=self.height + 1.0), CLEAR, _RIM, _RADIUS)


# --- [OFFSCREEN]


class Offscreen(msgspec.Struct, frozen=True, kw_only=True):
    """Color buffer at caller device-pixel size and precision."""

    width: int
    height: int
    format: Literal["RGBA8", "RGBA16", "RGBA16F", "RGBA32F"] = "RGBA8"

    def read(self, draw: Callable[[gpu.types.GPUOffScreen], Outcome[None]]) -> Outcome[npt.NDArray[np.float32]]:
        """Draws into a buffer it owns and binds for the call, reading normalized float RGBA top row first under that bind, refusing sizes past the texture limit."""

        @catch(exception=SystemError)
        def limit() -> int:
            gpu.init()
            return gpu.capabilities.max_texture_size_get()

        @catch(exception=RuntimeError)
        def opened() -> gpu.types.GPUOffScreen:
            return gpu.types.GPUOffScreen(self.width, self.height, format=self.format)

        def drawn(buffer: gpu.types.GPUOffScreen) -> Outcome[npt.NDArray[np.float32]]:
            with ExitStack() as stack:
                stack.callback(buffer.free)
                stack.enter_context(buffer.bind())  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
                return draw(buffer).bind(lambda _: captured(Rect(x=0.0, y=0.0, width=float(self.width), height=float(self.height))))

        return (
            limit()
            .map_error(partial(Fault.of, Reason.REFUSED, "gpu.init"))
            .filter_with(lambda bound: max(self.width, self.height) <= bound, lambda bound: Fault.of(Reason.OUTSIDE, "gpu.types.GPUOffScreen", f"{self.width}x{self.height}", member=str(bound)))
            .bind(lambda _: opened().map_error(partial(Fault.of, Reason.REFUSED, "gpu.types.GPUOffScreen")))
            .bind(drawn)
        )

    def drawn(self, canvas: Canvas, draw: Callable[[Canvas], None]) -> Outcome[npt.NDArray[np.float32]]:
        """Draws on a pixel canvas in the metrics and editor theme of `canvas`, under pixel projection with depth test off, returning top-first RGBA."""
        pixels = msgspec.structs.replace(canvas, extent=Rect(x=0.0, y=0.0, width=float(self.width), height=float(self.height)), clipping=Nothing, pointer=Nothing)

        def painted(_buffer: gpu.types.GPUOffScreen) -> Outcome[None]:
            """Draws callback on the cleared buffer under the scissor test over the whole buffer, the test region drawing holds on staying on, restoring the box it changed."""
            with pixels.drawn(), Canvas.depth_tested("NONE"), ExitStack() as stack:
                stack.callback(gpu.state.scissor_set, *gpu.state.scissor_get())
                gpu.state.active_framebuffer_get().clear(color=CLEAR)
                gpu.state.scissor_set(0, 0, self.width, self.height)
                gpu.state.scissor_test_set(True)
                gpu.matrix.load_identity()
                gpu.matrix.load_projection_matrix(Matrix.Translation((-1.0, -1.0, 0.0)) @ Matrix.Diagonal((2.0 / self.width, 2.0 / self.height, 1.0, 1.0)))
                draw(pixels)
            return Ok(None)

        return self.read(painted)

    def viewed(
        self, located: Located, camera: bpy.types.Object, scene: bpy.types.Scene, view_layer: bpy.types.ViewLayer, *, do_color_management: bool = True, draw_background: bool = True
    ) -> Outcome[npt.NDArray[np.float32]]:
        """Draws camera view with overlays off, restoring the editor value after, the offscreen view drawing no gizmos."""
        space = located.area.spaces.active
        if not isinstance(space, bpy.types.SpaceView3D):
            return Error(Fault.of(Reason.ABSENT, repr(located.area), member=bpy.types.SpaceView3D.__name__))

        def painted(region: bpy.types.Region, buffer: gpu.types.GPUOffScreen) -> Outcome[None]:
            """Draws camera view into buffer, refusing a camera transform with no inverse."""

            def projected(view: Matrix) -> Outcome[None]:
                return catch(buffer.draw_view3d, exception=RuntimeError)(
                    scene,
                    view_layer,
                    space,
                    region,
                    view,
                    camera.calc_matrix_camera(view_layer.depsgraph, x=buffer.width, y=buffer.height),
                    do_color_management=do_color_management,
                    draw_background=draw_background,
                ).map_error(partial(Fault.of, Reason.REFUSED, "gpu.types.GPUOffScreen.draw_view3d"))

            return catch(camera.matrix_world.normalized().inverted, exception=ValueError)().map_error(partial(Fault.of, Reason.REFUSED, "mathutils.Matrix.inverted")).bind(projected)

        def restored(region: bpy.types.Region, state: EditorState) -> Outcome[npt.NDArray[np.float32]]:
            """Reads view pixels between the editor write and its restore."""
            return state.applied(located.area).bind(
                lambda prior: paired(self.read(lambda buffer: painted(region, buffer)), prior.map(lambda held: held.applied(located.area)).default_value(Ok(Nothing))).map(lambda pair: pair[0])
            )

        region = located.region.map(Ok).default_with(lambda: region_in(located.area))
        return paired(region, EditorState.of(bpy.types.SpaceView3D, {("overlay", "show_overlays"): False})).bind(lambda pair: restored(*pair))


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [STATE]


@contextmanager
def _restored[T](read: Callable[[], T], write: Callable[[T], None], value: T) -> Generator[None]:
    """Writes a GPU state value, restoring the value read before it at scope exit."""
    with ExitStack() as stack:
        stack.callback(write, read())
        write(value)
        yield


@contextmanager
def _typeset(metrics: Metrics, font: int, size: float | bpy.types.ThemeFontStyle, shadow: Option[Shadow], wrap: Option[float], clip: Option[Rect], rotation: float) -> Generator[None]:
    """Sets font size, a theme font style's at interface scale, and enables the shadow, wrap, clip, and rotation given, disabling each at scope exit."""
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


def captured(rect: Rect) -> Outcome[npt.NDArray[np.float32]]:
    """Reads normalized float RGBA of a framebuffer rectangle, top row first, from the bound offscreen buffer or inside a draw callback.

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


@functools.cache
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
    return shader, _BATCH_FOR_SHADER(shader, "TRI_STRIP", {"pos": QUAD})


@functools.cache
def _textured() -> gpu.types.GPUBatch:
    """Returns UV quad for image shaders."""
    return Canvas.batched("IMAGE_COLOR", "TRI_STRIP", {"pos": QUAD, "texCoord": QUAD})


def programs(*clears: Callable[[], None]) -> Resource[None]:
    """Holds memoized GPU programs while an extension's registration stands.

    Args:
        *clears: `cache_clear` of each program builder under `functools.cache`.

    Returns:
        Resource dropping every builder's program at release, inside the GPU context unregistration runs in.
    """
    return Resource[None](made=lambda _identity: _cleared(clears))


@contextmanager
def _cleared(clears: tuple[Callable[[], None], ...]) -> Generator[None]:
    """Clears each builder's memoized program at scope exit."""
    with ExitStack() as stack:
        for clear in clears:
            stack.callback(clear)
        yield


# --- [COMPOSITION] ----------------------------------------------------------------------

PROGRAMS: Final = programs(_rounded.cache_clear, _textured.cache_clear)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["DECODED", "FINE", "GRID", "MAJOR", "PROGRAMS", "QUAD", "TRACE", "Align", "Blend", "Canvas", "Chart", "DrawType", "Label", "Layer", "Offscreen", "Rect", "Shadow", "captured", "programs"]
