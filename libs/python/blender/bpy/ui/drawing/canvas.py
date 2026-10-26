"""Region draw state, text, and cached GPU primitives."""

from collections.abc import Callable, Generator
from contextlib import contextmanager
from enum import Enum
import functools
from typing import Final, Literal, Self

import blf
import bpy
from expression import Nothing, Option, Some
import gpu
from gpu_extras.batch import batch_for_shader
import msgspec

from blender.bpy.extension.scoped import Scoped
from blender.bpy.ui.resources.theme import Color, Metrics, Theme

# --- [TYPES] ----------------------------------------------------------------------------

type Blend = Literal["NONE", "ALPHA", "ALPHA_PREMULT", "ADDITIVE", "ADDITIVE_PREMULT", "MULTIPLY", "SUBTRACT", "INVERT"]


class Align(Enum):
    """Label width fraction before its anchor."""

    START = 0.0
    CENTER = 0.5
    END = 1.0


# --- [CONSTANTS] ------------------------------------------------------------------------

QUAD: Final = ((0.0, 0.0), (1.0, 0.0), (0.0, 1.0), (1.0, 1.0))

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
    """Text shadow, offset in device pixels."""

    color: Color
    offset: tuple[float, float]
    level: int = 0

    @classmethod
    def of(cls, style: bpy.types.ThemeFontStyle) -> Self:
        """Reads theme font style shadow."""
        gray = style.shadow_value
        return cls(level=style.shadow, color=(gray, gray, gray, style.shadow_alpha), offset=(style.shadow_offset_x, style.shadow_offset_y))


class Canvas(msgspec.Struct, frozen=True, kw_only=True):
    """Draw state of one region."""

    theme: Theme
    metrics: Metrics
    extent: Rect
    pointer: Option[tuple[int, int]] = Nothing
    space: Option[bpy.types.Space] = Nothing
    region: Option[bpy.types.Region] = Nothing

    # --- [CONTEXT]

    @classmethod
    def of(cls, area: bpy.types.Area, region: bpy.types.Region, pointer: Option[tuple[int, int]] = Nothing) -> Self:
        """Reads region extent less overlapping siblings, each inset from the bounds the earlier siblings left."""
        metrics = Metrics.read()

        def inset(bounds: tuple[int, int, int, int], sibling: bpy.types.Region) -> tuple[int, int, int, int]:
            left, bottom, right, top = bounds
            x, y = sibling.x, sibling.y
            xmax, ymax = x + sibling.width - 1, y + sibling.height - 1
            match sibling.alignment:
                case _ if not (left <= xmax and right >= x and bottom <= ymax and top >= y):
                    return bounds
                case "LEFT" | "RIGHT":
                    return (xmax if abs(left - x) < 2 else left), bottom, (x if abs(right - xmax) < 2 else right), top
                case "TOP" | "BOTTOM":
                    return left, (ymax if abs(bottom - y) < 2 else bottom), right, (y if abs(top - ymax) < 2 else top)
                case _:
                    return bounds

        overlapping = (other for other in area.regions if other != region and Theme(space=area.type, region=other.type, preferences=metrics.preferences).overlaps)
        left, bottom, right, top = functools.reduce(inset, overlapping, (region.x, region.y, region.x + region.width - 1, region.y + region.height - 1))
        origin_x, origin_y = pointer.map(lambda _: (0, 0)).default_value((region.x, region.y))
        return cls(
            theme=Theme(space=area.type, region=region.type, preferences=metrics.preferences),
            metrics=metrics,
            extent=Rect(x=float(left - origin_x), y=float(bottom - origin_y), width=float(right - left + 1), height=float(top - bottom + 1)),
            pointer=pointer,
            space=Option.of_optional(area.spaces.active),
            region=Some(region),
        )

    # --- [STATE]

    @contextmanager
    def drawn(self) -> Generator[Self]:
        """Scopes matrix stacks and alpha blending."""
        with gpu.matrix.push_pop(), gpu.matrix.push_pop_projection(), Canvas.blended("ALPHA"):  # type: ignore[attr-defined]  # ty: ignore[invalid-context-manager]
            yield self

    @staticmethod
    @contextmanager
    def blended(mode: Blend) -> Generator[None]:
        """Scopes blend mode, restoring the mode it found."""
        held = gpu.state.blend_get()
        gpu.state.blend_set(mode)
        try:
            yield
        finally:
            gpu.state.blend_set(held)  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]

    @staticmethod
    @contextmanager
    def clipped(rect: Rect, *, view: Option[bpy.types.View2D] = Nothing) -> Generator[None]:
        """Narrows scissor to a pixel or View2D rectangle."""
        corners = ((rect.x, rect.y), (rect.x + rect.width, rect.y + rect.height))
        match view:
            case Option(tag="some", some=region):
                origin_x, origin_y, _, _ = gpu.state.viewport_get()
                (left, bottom), (right, top) = ((origin_x + column, origin_y + row) for column, row in (region.view_to_region(*corner, clip=False) for corner in corners))  # type: ignore[misc, has-type]
            case _:
                (left, bottom), (right, top) = ((round(x), round(y)) for x, y in corners)
        held = gpu.state.scissor_get()
        held_x, held_y, held_width, held_height = held
        x, y = max(held_x, left), max(held_y, bottom)
        right = min(held_x + held_width, right)
        top = min(held_y + held_height, top)
        gpu.state.scissor_set(x, y, max(0, right - x), max(0, top - y))
        try:
            yield
        finally:
            gpu.state.scissor_set(*held)

    # --- [PRIMITIVES]

    def lines(self, width: float) -> gpu.types.GPUShader:
        """Binds polyline shader at device-pixel width."""
        shader = gpu.shader.from_builtin("POLYLINE_UNIFORM_COLOR")
        shader.bind()
        shader.uniform_float("viewportSize", gpu.state.viewport_get()[2:])
        shader.uniform_float("lineWidth", self.metrics.line(width))
        shader.uniform_bool("lineSmooth", True)
        return shader

    def points(self) -> gpu.types.GPUShader:
        """Binds point shader at device pixel size."""
        shader = gpu.shader.from_builtin("POINT_FLAT_COLOR")
        size = float(self.metrics.pixel_size)
        gpu.state.point_size_set(size)
        shader.bind()
        shader.uniform_float("size", size)
        return shader

    @staticmethod
    def label(
        text: str, at: tuple[float, float], color: Color, points: float, align: Align = Align.START, shadow: Option[Shadow] = Nothing, *, font: int = 0, translation_context: str | None = None
    ) -> None:
        """Draws translated text under the blend mode it found, interface font by default."""
        blf.size(font, points)
        blf.color(font, *color)
        shown = bpy.app.translations.pgettext_iface(text, translation_context)
        x, y = at
        width, _ = blf.dimensions(font, shown)
        blf.position(font, x - width * align.value, y, 0.0)
        blf.disable(font, blf.SHADOW)
        for shade in shadow.to_list():
            blf.enable(font, blf.SHADOW)
            blf.shadow(font, shade.level, *shade.color)
            offset_x, offset_y = shade.offset
            blf.shadow_offset(font, round(offset_x), round(offset_y))
        with Canvas.blended(gpu.state.blend_get()):  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
            try:
                blf.draw(font, shown)
            finally:
                blf.disable(font, blf.SHADOW)

    def frame(self, rect: Rect, fill: Color, edge: Color, radius: float) -> None:
        """Draws antialiased rounded frame."""
        shader, quad = rounded()
        shader.bind()
        box = gpu.types.GPUUniformBuf(gpu.types.Buffer("FLOAT", 16, (rect.x, rect.y, rect.width, rect.height, *fill, *edge, radius, self.metrics.line(1.0), 0.0, 0.0)))
        shader.uniform_block("box", box)
        with Canvas.blended("ALPHA_PREMULT"):
            quad.draw(shader)

    @staticmethod
    def picture(texture: gpu.types.GPUTexture, rect: Rect, shader: gpu.types.GPUShader, *, blend: Blend, color: Color = (1.0, 1.0, 1.0, 1.0)) -> None:
        """Draws texture under its blend mode."""
        quad = textured()
        shader.bind()
        shader.uniform_sampler("image", texture)
        shader.uniform_float("color", color)
        with gpu.matrix.push_pop(), Canvas.blended(blend):  # type: ignore[attr-defined]  # ty: ignore[invalid-context-manager]
            gpu.matrix.translate((rect.x, rect.y))
            gpu.matrix.scale((rect.width, rect.height))
            quad.draw(shader)


# --- [OPERATIONS] -----------------------------------------------------------------------


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
    return shader, batch_for_shader(shader, "TRI_STRIP", {"pos": QUAD})


@functools.cache
def textured() -> gpu.types.GPUBatch:
    """Returns UV quad for image shaders."""
    return batch_for_shader(gpu.shader.from_builtin("IMAGE_COLOR"), "TRI_STRIP", {"pos": QUAD, "texCoord": QUAD})


@contextmanager
def released(*clears: Callable[[], None]) -> Generator[None]:
    """Holds cached GPU programs until scope exit, then drops them inside the GPU context unregistration runs in.

    Args:
        clears: `cache_clear` of each cached program builder.

    Yields:
        None while the programs stay cached.
    """
    try:
        yield
    finally:
        for clear in clears:
            clear()


# --- [COMPOSITION] ----------------------------------------------------------------------

PROGRAMS: Final = Scoped[None](made=lambda _identity: released(rounded.cache_clear, textured.cache_clear))

# --- [EXPORTS] --------------------------------------------------------------------------


__all__ = ["DECODED", "PROGRAMS", "QUAD", "Align", "Blend", "Canvas", "Rect", "Shadow", "released", "rounded", "textured"]
