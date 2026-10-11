"""Native drawing policy, region geometry, text, and cached GPU primitives."""

from collections.abc import Generator
from contextlib import contextmanager
from enum import Enum
import functools
from typing import Final, Self

import blf
import bpy
from expression import Nothing, Option, Some
import gpu
from gpu_extras.batch import batch_for_shader
import msgspec

from rasm.blender.ui.resources.theme import Color, Metrics, Theme

# --- [TYPES] ----------------------------------------------------------------------------


class Align(Enum):
    """Fraction of a label's width preceding its anchor."""

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
    """Axis-aligned rectangle measured from its lower left corner."""

    x: float
    y: float
    width: float
    height: float


class Shadow(msgspec.Struct, frozen=True, kw_only=True):
    """Text shadow level, display color, and device-pixel offset."""

    color: Color
    offset: tuple[float, float]
    level: int = 0

    @classmethod
    def of(cls, style: bpy.types.ThemeFontStyle) -> Self:
        """Reads a native font style's shadow."""
        gray = style.shadow_value
        return cls(level=style.shadow, color=(gray, gray, gray, style.shadow_alpha), offset=(style.shadow_offset_x, style.shadow_offset_y))


class Canvas(msgspec.Struct, frozen=True, kw_only=True):
    """Theme, metrics, visible region, pointer, and editor for one draw."""

    theme: Theme
    metrics: Metrics
    extent: Rect
    pointer: Option[tuple[int, int]] = Nothing
    space: Option[bpy.types.Space] = Nothing
    region: Option[bpy.types.Region] = Nothing

    # --- [CONTEXT]

    @classmethod
    def of(cls, area: bpy.types.Area, region: bpy.types.Region, pointer: Option[tuple[int, int]] = Nothing) -> Self:
        """Reads the visible extent from Blender's stored region rectangles."""
        left, bottom = region.x, region.y
        right, top = left + region.width - 1, bottom + region.height - 1
        for sibling in (other for other in area.regions if other != region and Theme(space=area.type, region=other.type).overlaps):
            x, y = sibling.x, sibling.y
            xmax, ymax = x + sibling.width - 1, y + sibling.height - 1
            if not (left <= xmax and right >= x and bottom <= ymax and top >= y):
                continue
            if sibling.alignment in {"LEFT", "RIGHT"}:
                left, right = (xmax if abs(left - x) < 2 else left), (x if abs(right - xmax) < 2 else right)
            elif sibling.alignment in {"TOP", "BOTTOM"}:
                bottom, top = (ymax if abs(bottom - y) < 2 else bottom), (y if abs(top - ymax) < 2 else top)
        origin_x, origin_y = pointer.map(lambda _: (0, 0)).default_value((region.x, region.y))
        return cls(
            theme=Theme(space=area.type, region=region.type),
            metrics=Metrics.read(),
            extent=Rect(x=float(left - origin_x), y=float(bottom - origin_y), width=float(right - left + 1), height=float(top - bottom + 1)),
            pointer=pointer,
            space=Option.of_optional(area.spaces.active),
            region=Some(region),
        )

    # --- [STATE]

    @contextmanager
    def drawn(self) -> Generator[Self]:
        """Scopes both matrix stacks and restores the entering blend equation."""
        blend = gpu.state.blend_get()
        with gpu.matrix.push_pop(), gpu.matrix.push_pop_projection():  # type: ignore[attr-defined]  # ty: ignore[invalid-context-manager]
            gpu.state.blend_set("ALPHA")
            try:
                yield self
            finally:
                gpu.state.blend_set(blend)  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]

    @staticmethod
    @contextmanager
    def clipped(rect: Rect, *, view: Option[bpy.types.View2D] = Nothing) -> Generator[None]:
        """Narrows the framebuffer scissor using pixel or native View2D coordinates."""
        corners = ((rect.x, rect.y), (rect.x + rect.width, rect.y + rect.height))
        match view.tag:
            case "some":
                origin_x, origin_y, _, _ = gpu.state.viewport_get()
                (left, bottom), (right, top) = (  # type: ignore[misc]
                    (origin_x + x, origin_y + y)  # type: ignore[has-type]
                    for x, y in (view.some.view_to_region(*corner, clip=False) for corner in corners)
                )
            case "none":
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
        """Binds native lines at the current viewport and device-pixel width."""
        shader = gpu.shader.from_builtin("POLYLINE_UNIFORM_COLOR")
        shader.bind()
        shader.uniform_float("viewportSize", gpu.state.viewport_get()[2:])
        shader.uniform_float("lineWidth", self.metrics.line(width))
        shader.uniform_bool("lineSmooth", True)
        return shader

    def points(self) -> gpu.types.GPUShader:
        """Sets native raster and shader point diameters to the device pixel size."""
        shader = gpu.shader.from_builtin("POINT_FLAT_COLOR")
        size = float(self.metrics.pixel_size)
        gpu.state.point_size_set(size)
        shader.bind()
        shader.uniform_float("size", size)
        return shader

    def label(
        self, text: str, at: tuple[float, float], color: Color, points: float, align: Align = Align.START, shadow: Option[Shadow] = Nothing, *, font: int = 0, translation_context: str | None = None
    ) -> None:
        """Draws translated text through a caller-owned font, defaulting to the UI font."""
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
        try:
            blf.draw(font, shown)
        finally:
            blf.disable(font, blf.SHADOW)

    def frame(self, rect: Rect, fill: Color, edge: Color, radius: float) -> None:
        """Draws an antialiased rounded frame with premultiplied composition."""
        shader, quad = rounded()
        shader.bind()
        shader.uniform_float("rect", (rect.x, rect.y, rect.width, rect.height))
        shader.uniform_float("fill", fill)
        shader.uniform_float("edge", edge)
        shader.uniform_float("radius", radius)
        shader.uniform_float("edgeWidth", self.metrics.line(1.0))
        blend = gpu.state.blend_get()
        gpu.state.blend_set("ALPHA_PREMULT")
        try:
            quad.draw(shader)
        finally:
            gpu.state.blend_set(blend)  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]

    def picture(self, texture: gpu.types.GPUTexture, rect: Rect, shader: gpu.types.GPUShader, color: Color = (1.0, 1.0, 1.0, 1.0)) -> None:
        """Draws a floating-point position/UV image shader under the caller's blend."""
        quad = textured()
        shader.bind()
        shader.uniform_sampler("image", texture)
        shader.uniform_float("color", color)
        with gpu.matrix.push_pop():  # type: ignore[attr-defined]  # ty: ignore[invalid-context-manager]
            gpu.matrix.translate((rect.x, rect.y))
            gpu.matrix.scale((rect.width, rect.height))
            quad.draw(shader)


# --- [OPERATIONS] -----------------------------------------------------------------------


@functools.cache
def rounded() -> tuple[gpu.types.GPUShader, gpu.types.GPUBatch]:
    """Builds the rounded-frame shader and unit quad in the draw context."""
    vertex = "void main() { local = pos * rect.zw; gl_Position = ModelViewProjectionMatrix * vec4(rect.xy + local, 0.0, 1.0); }"
    fragment = (
        DECODED
        + """
void main() {
    vec2 half_size = 0.5 * rect.zw;
    vec2 q = abs(local - half_size) - half_size + radius;
    float d = length(max(q, 0.0)) + min(max(q.x, q.y), 0.0) - radius;
    float pixel = fwidth(d);
    float inside = clamp(0.5 - d / pixel, 0.0, 1.0);
    float rim = clamp(0.5 + (d + edgeWidth) / pixel, 0.0, 1.0) * edge.a;
    float body = fill.a * (1.0 - rim);
    fragColor = vec4(targeted(edge.rgb) * rim + targeted(fill.rgb) * body, rim + body) * inside;
}
"""
    )
    interface = gpu.types.GPUStageInterfaceInfo("rasm_rounded")
    interface.smooth("VEC2", "local")
    info = gpu.types.GPUShaderCreateInfo()
    info.vertex_in(0, "VEC2", "pos")
    info.vertex_out(interface)
    info.push_constant("MAT4", "ModelViewProjectionMatrix")
    for name in ("rect", "fill", "edge"):
        info.push_constant("VEC4", name)
    for name in ("radius", "edgeWidth"):
        info.push_constant("FLOAT", name)
    info.push_constant("BOOL", "srgbTarget")
    info.fragment_out(0, "VEC4", "fragColor")
    info.vertex_source(vertex)
    info.fragment_source(fragment)
    shader = gpu.shader.create_from_info(info)
    return shader, batch_for_shader(shader, "TRI_STRIP", {"pos": QUAD})


@functools.cache
def textured() -> gpu.types.GPUBatch:
    """Returns one cached floating-point position/UV quad for image shaders."""
    return batch_for_shader(gpu.shader.from_builtin("IMAGE_COLOR"), "TRI_STRIP", {"pos": QUAD, "texCoord": QUAD})


# --- [EXPORTS] --------------------------------------------------------------------------


__all__ = ["DECODED", "QUAD", "Align", "Canvas", "Rect", "Shadow", "rounded", "textured"]
