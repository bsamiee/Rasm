"""Region draw state, text, built-in shader binds and batches, and cached GPU programs."""

from collections.abc import Callable, Generator, Mapping
from contextlib import AbstractContextManager, contextmanager, ExitStack
from enum import Enum
import functools
import math
from typing import Final, Literal, Protocol, Self

import blf
import bpy
from expression import Nothing, Option, Some
import gpu
from gpu_extras.batch import batch_for_shader
import msgspec
import numpy as np
import numpy.typing as npt

from blender.bpy.extension.identity import Resource
from blender.bpy.ui.resources.theme import Color, Metrics, Theme
from blender.bpy.ui.screens.regions import visible_bounds

# --- [TYPES] ----------------------------------------------------------------------------

type Blend = Literal["NONE", "ALPHA", "ALPHA_PREMULT", "ADDITIVE", "ADDITIVE_PREMULT", "MULTIPLY", "SUBTRACT", "INVERT"]
type DepthTest = Literal["NONE", "ALWAYS", "LESS", "LESS_EQUAL", "EQUAL", "GREATER", "GREATER_EQUAL"]
type DrawType = Literal["PRE_VIEW", "POST_VIEW", "POST_PIXEL", "BACKDROP"]
type Polyline = Literal["POLYLINE_UNIFORM_COLOR", "POLYLINE_FLAT_COLOR", "POLYLINE_SMOOTH_COLOR"]
type Primitive = Literal["POINTS", "LINES", "TRIS", "LINE_STRIP", "TRI_STRIP", "LINES_ADJ", "TRIS_ADJ", "LINE_STRIP_ADJ"]


class Cached(Protocol):
    """GPU program builder memoized by `functools.cache`."""

    def cache_clear(self) -> None:
        """Drops the memoized program."""
        ...


class Align(Enum):
    """Label width fraction before its anchor."""

    START = 0.0
    CENTER = 0.5
    END = 1.0


# --- [CONSTANTS] ------------------------------------------------------------------------

QUAD: Final = np.array(((0.0, 0.0), (1.0, 0.0), (0.0, 1.0), (1.0, 1.0)), np.float32)
QUAD.setflags(write=False)
_PUSH_POP: Final[Callable[[], AbstractContextManager[None]]] = gpu.matrix.push_pop  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
_PUSH_POP_PROJECTION: Final[Callable[[], AbstractContextManager[None]]] = gpu.matrix.push_pop_projection  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
_BATCH_FOR_SHADER: Final[Callable[[gpu.types.GPUShader, Primitive, Mapping[str, npt.NDArray[np.float32]]], gpu.types.GPUBatch]] = batch_for_shader  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
_BLEND_GET: Final[Callable[[], Blend]] = gpu.state.blend_get  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
_DEPTH_TEST_GET: Final[Callable[[], DepthTest]] = gpu.state.depth_test_get  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
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
    """Draw state of one region: interface metrics, visible extent, the editor and region drawn in, and the clip planes of a clipping 3D View while drawing in its world space."""

    metrics: Metrics
    extent: Rect
    space: bpy.types.Space
    region: bpy.types.Region
    clipping: Option[gpu.types.GPUUniformBuf] = Nothing

    # --- [CONTEXT]

    @classmethod
    def of(cls, area: bpy.types.Area, region: bpy.types.Region, phase: DrawType = "POST_PIXEL", pointer: Option[tuple[int, int]] = Nothing) -> Self:
        """Reads region extent less overlapping siblings, in window pixels while a cursor is given, with the clip planes a view phase of a 3D View clipping its solid or wireframe shading draws under, by Blender's `RV3D_CLIPPING_ENABLED`."""
        space: bpy.types.Space = area.spaces.active  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        x, y, width, height = visible_bounds(area, region)
        origin_x, origin_y = pointer.map(lambda _: (region.x, region.y)).default_value((0, 0))
        match phase in _VIEWED, space, region.data:
            case True, bpy.types.SpaceView3D(shading=bpy.types.View3DShading(type="WIREFRAME" | "SOLID")), bpy.types.RegionView3D(use_clip_planes=True) as view:
                clipping = Some(gpu.types.GPUUniformBuf(gpu.types.Buffer("FLOAT", 40, np.concatenate((np.identity(4, np.float32).ravel(), np.asarray(view.clip_planes, np.float32).ravel())))))  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
            case _:
                clipping = Nothing
        return cls(metrics=Metrics.read(), extent=Rect(x=float(x + origin_x), y=float(y + origin_y), width=float(width), height=float(height)), space=space, region=region, clipping=clipping)

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
    def depth_tested(mode: DepthTest = "LESS_EQUAL") -> AbstractContextManager[None]:
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
    def lines(self, width: float, shader: Polyline = "POLYLINE_UNIFORM_COLOR") -> Generator[gpu.types.GPUShader]:
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
    def batched(name: str, primitive: Primitive, attributes: Mapping[str, npt.NDArray[np.float32]]) -> gpu.types.GPUBatch:
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
        shader, quad = rounded()
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
            textured().draw(shader)


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


# --- [PROGRAMS]


@functools.cache
def rounded() -> tuple[gpu.types.GPUShader, gpu.types.GPUBatch]:
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
def textured() -> gpu.types.GPUBatch:
    """Returns UV quad for image shaders."""
    return Canvas.batched("IMAGE_COLOR", "TRI_STRIP", {"pos": QUAD, "texCoord": QUAD})


def programs(*cached: Cached) -> Resource[None]:
    """Holds memoized GPU programs while an extension's registration stands.

    Args:
        *cached: Program builders under `functools.cache`.

    Returns:
        Resource dropping every builder's program at release, inside the GPU context unregistration runs in.
    """
    return Resource[None](made=lambda _identity: _cleared(cached))


@contextmanager
def _cleared(cached: tuple[Cached, ...]) -> Generator[None]:
    """Clears each builder's memoized program at scope exit."""
    with ExitStack() as stack:
        for builder in cached:
            stack.callback(builder.cache_clear)
        yield


# --- [COMPOSITION] ----------------------------------------------------------------------

PROGRAMS: Final = programs(rounded, textured)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["DECODED", "PROGRAMS", "QUAD", "Align", "Blend", "Cached", "Canvas", "DepthTest", "DrawType", "Polyline", "Primitive", "Rect", "Shadow", "programs", "rounded", "textured"]
