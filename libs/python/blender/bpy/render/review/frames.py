"""Frame view cases over the image editor: compare wipe, drawn charts and clip ends, coverage masks, frame regions, and frame picks."""

from collections.abc import Callable, Iterable
from enum import auto, Enum, IntEnum
import functools
import math
from typing import Annotated, ClassVar, Final, get_args, Self, TYPE_CHECKING

import bpy
from expression import Nothing, Ok, Option, Result, Some
from expression.extra.result import catch
import gpu
from gpu_extras.batch import batch_for_shader
import mathutils
from mathutils import Matrix
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.bpy.commands.calls import called
from blender.bpy.commands.modal import Gesture, Modal, Travel
from blender.bpy.commands.operators import Call, Done, Empty, Operator, Outcome, Pick
from blender.bpy.data.ids.images import ResultRead
from blender.bpy.data.rna.conditions import AllOf, check, holds, Setting
from blender.bpy.data.rna.descriptors import Descriptor, IdName, root_in
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions, Registrable
from blender.bpy.faults import BlenderAbsentFault, BlenderFaults, BlenderReadFault, BlenderResult
from blender.bpy.render.color.exposure import AutoExposure, Metering
from blender.bpy.render.effects.stack import EffectName, formation, viewed
from blender.bpy.render.effects.viewers import EffectViewer, EntryRegion
from blender.bpy.render.outputs.exports import config_of, displayed, image_type, plane
from blender.bpy.settings.attachments import Attached, Attachment
from blender.bpy.settings.properties import Definition
from blender.bpy.ui.drawing.canvas import Align, Canvas, compiled, DECODED, Rect, Shadow
from blender.bpy.ui.drawing.charts import ChartFrame, FalseColor, Gamuts, histogram, Outline
from blender.bpy.ui.drawing.handlers import Drawing, Overlay, Part, Refresh, Signal, Watch
from blender.bpy.ui.layout.rows import Caption, Flow, joined, Line, Member, placed, Property, Row, RowDraw
from blender.bpy.ui.layout.sections import Single
from blender.bpy.ui.resources.icons import Glyph
from blender.bpy.ui.resources.textures import uploaded
from blender.bpy.ui.resources.theme import Color, color, Metrics
from blender.bpy.ui.screens.areas import AreaQuery, Located
from blender.bpy.ui.screens.editors import EditorState
from blender.bpy.ui.surfaces.gizmos import Gizmos, Move
from blender.imaging.analysis.metering import Region
from blender.imaging.analysis.scopes import Clipping
from blender.imaging.color.processors import unassociated
from blender.imaging.effects.quantization import codes, IntegerDepth
from blender.imaging.faults import accumulated, paired
from blender.imaging.geometry.homography import Quad
from blender.imaging.parameters import Choice, Gated

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type Shape = Aligned | Free
type RegionPart = int | tuple[int, int] | Motion


class Motion(Enum):
    """Whole-region gestures alongside individual corners and edges."""

    TRANSLATE = auto()
    ROTATE = auto()
    SCALE = auto()


class Mode(Choice):
    """Compare layout: a wipe across either axis or both frames side by side."""

    VERTICAL = 1, "Vertical Wipe"
    HORIZONTAL = 2, "Horizontal Wipe"
    SIDE = 3, "Side by Side"


class Shown(IntEnum):
    """Image editor channel views the before frame follows, each value the channel shader's selector."""

    COLOR = 0
    COLOR_ALPHA = 1
    RED = 2
    GREEN = 3
    BLUE = 4
    ALPHA = 5


# --- [CONSTANTS] ------------------------------------------------------------------------

HANDLE: Final = 80 * 0.35 / 2


# --- [MODELS] ---------------------------------------------------------------------------


class Compare(msgspec.Struct, frozen=True, kw_only=True):
    """Compare layout, split, and side of the before frame."""

    mode: Annotated[Mode, msgspec.Meta(title="Compare Mode", description="Wipe orientation, or both frames side by side")] = Mode.VERTICAL
    split: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, title="Split", description="Wipe position across the frame")] = 0.5
    swapped: Annotated[bool, msgspec.Meta(title="Swap", description="Put the before frame right of or below the split")] = False


class Charted(msgspec.Struct, frozen=True, kw_only=True):
    """Switches of the charts drawn over a screen's Render Result."""

    outline: Annotated[bool, msgspec.Meta(title="Outline", description="Draw the scene-linear luminance outline of the last render")] = True
    gamuts: Annotated[bool, msgspec.Meta(title="Chromaticity", description="Draw the chromaticity diagram of the last render with its working and display gamuts")] = False


class ClipEnds(msgspec.Struct, frozen=True, kw_only=True):
    """Clip ends tinted over a screen's Render Result."""

    low: Annotated[bool, msgspec.Meta(title="Low Clip", description="Tint the pixels the written file holds at its lowest code")] = False
    high: Annotated[bool, msgspec.Meta(title="High Clip", description="Tint the pixels the written file holds at its highest code")] = False


class Before(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Before frame: a top-first associated RGBA plane and its caption."""

    plane: npt.NDArray[np.float32]
    name: str = "Before"


class Plate(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Uploaded before frame and its caption."""

    texture: gpu.types.GPUTexture
    name: str


class Sides(msgspec.Struct, frozen=True, kw_only=True):
    """Layout of one compare draw: the plate, its clip, the rule, and the captions."""

    after: ClassVar[str] = "After"
    plate: Rect
    clip: Rect
    rule: Option[tuple[tuple[float, float], tuple[float, float]]]
    captions: tuple[tuple[str, float, float, Align], ...]

    @classmethod
    def of(cls, compare: Compare, frame: Rect, bounds: Rect, size: tuple[float, float], name: str, gap: float, inset: float, text: float) -> Self:
        """Lays out the plate, clip, rule, and captions of a compare over a frame inside the visible bounds."""
        (width, height), right, top = size, bounds.x + bounds.width, bounds.y + bounds.height
        plate = Rect(x=frame.x + (frame.width - width) / 2, y=frame.y + (frame.height - height) / 2, width=width, height=height)
        low, high, left = max(frame.y, bounds.y), min(frame.y + frame.height, top), max(frame.x, bounds.x)
        first, last = (cls.after, name) if compare.swapped else (name, cls.after)
        match compare.mode:
            case Mode.SIDE:
                beside = msgspec.structs.replace(plate, x=frame.x + frame.width + gap if compare.swapped else frame.x - gap - width)
                outer = (beside.x + inset, Align.START) if compare.swapped else (beside.x + width - inset, Align.END)
                inner = (frame.x + frame.width - inset, Align.END) if compare.swapped else (frame.x + inset, Align.START)
                return cls(plate=beside, clip=beside, rule=Nothing, captions=((name, outer[0], high - inset - text, outer[1]), (cls.after, inner[0], high - inset - text, inner[1])))
            case Mode.VERTICAL:
                at = frame.x + compare.split * frame.width
                clip = msgspec.structs.replace(bounds, x=at, width=max(right - at, 0.0)) if compare.swapped else msgspec.structs.replace(bounds, width=max(at - bounds.x, 0.0))
                captions = ((first, at - inset, high - inset - text, Align.END), (last, at + inset, high - inset - text, Align.START))
                return cls(plate=plate, clip=clip, rule=Some(((at, low), (at, high))), captions=captions)
            case Mode.HORIZONTAL:
                at = frame.y + (1.0 - compare.split) * frame.height
                clip = msgspec.structs.replace(bounds, height=max(at - bounds.y, 0.0)) if compare.swapped else msgspec.structs.replace(bounds, y=at, height=max(top - at, 0.0))
                captions = ((first, left + inset, at + inset, Align.START), (last, left + inset, at - inset - text, Align.START))
                return cls(plate=plate, clip=clip, rule=Some(((left, at), (min(frame.x + frame.width, right), at))), captions=captions)


class Wipe(msgspec.Struct, frozen=True, kw_only=True):
    """Formed frame drawn in native channels, with an optional comparison layout."""

    light: ClassVar[Color] = (1.0, 1.0, 1.0, 1.0)
    dark: ClassVar[Color] = (0.0, 0.0, 0.0, 0.5)
    identity: Identity
    source: Callable[[Metrics], BlenderResult[Option[Before]]]
    compare: Option[Callable[[bpy.types.Context], BlenderResult[Option[Compare]]]] = Nothing

    def read(self, metrics: Metrics) -> BlenderResult[Option[Plate]]:
        """Reads the before frame and uploads its plane."""
        return self.source(metrics).bind(
            lambda found: found.map(lambda before: uploaded(unassociated(before.plane)).map(lambda texture: Some(Plate(texture=texture, name=before.name)))).default_value(Ok(Nothing))
        )

    def drawn(self, canvas: Canvas, state: Plate) -> None:
        """Draws the plate in the editor's channel view, the rule, and the captions over the editor's frame."""
        context = bpy.context
        gap, checker = canvas.metrics.line(1.0), canvas.metrics.checker()
        text = canvas.metrics.text(context.preferences.ui_styles[0].widget)  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
        shadow = Shadow(color=Wipe.dark, offset=(gap, -gap))
        shader, quad = Wipe.channeled()
        comparisons = self.compare.map(lambda read: self.identity.reports.reported(read)(context).bind(lambda found: found).map(Some)).default_value(Some(Nothing))
        for space, compare, shown, region in (
            (held, compare, channel, region)
            for held in canvas.space.bind(edited).to_list()
            for compare in comparisons.to_list()
            for channel in Option.of_optional(Shown.__members__.get(held.display_channels)).to_list()
            for region in canvas.region.to_list()
        ):
            size = (state.texture.width * space.zoom[0], state.texture.height * space.zoom[1])  # type: ignore[index]
            frame = framed(region)
            sides = compare.map(lambda held: Sides.of(held, frame, canvas.extent, size, state.name, gap, canvas.metrics.unit * 0.4, text)).default_with(
                lambda: Sides(
                    plate=Rect(x=frame.x + (frame.width - size[0]) / 2, y=frame.y + (frame.height - size[1]) / 2, width=size[0], height=size[1]), clip=canvas.extent, rule=Nothing, captions=()
                )
            )
            state.texture.filter_mode(space.zoom[0] < 1.0)  # type: ignore[index]
            with canvas.clipped(sides.clip):
                shader.bind()
                shader.uniform_sampler("image", state.texture)
                plate = gpu.types.GPUUniformBuf(gpu.types.Buffer("FLOAT", 12, (sides.plate.x, sides.plate.y, sides.plate.width, sides.plate.height, *checker.primary, *checker.secondary)))
                shader.uniform_block("plate", plate)
                shader.uniform_float("cell", float(checker.cell))
                shader.uniform_int("shown", shown.value)
                quad.draw(shader)
            lines = canvas.lines(1.0)
            for (start, end), (ink, shift) in ((rule, pair) for rule in sides.rule.to_list() for pair in ((Wipe.dark, gap), (Wipe.light, 0.0))):
                lines.uniform_float("color", ink)
                batch_for_shader(lines, "LINES", {"pos": ((start[0] + shift, start[1] - shift), (end[0] + shift, end[1] - shift))}).draw(lines)
            for caption, x, y, align in sides.captions:
                canvas.label(caption, (x, y), Wipe.light, text, align, Some(shadow))

    @staticmethod
    @compiled
    def channeled() -> tuple[gpu.types.GPUShader, gpu.types.GPUBatch]:
        """Builds the before frame's channel shader and its unit quad."""
        vertex = "void main() { uv = pos; gl_Position = ModelViewProjectionMatrix * vec4(plate.rect.xy + pos * plate.rect.zw, 0.0, 1.0); }"
        fragment = (
            DECODED
            + """
void main() {
    vec4 texel = texture(image, uv);
    vec2 cells = floor(gl_FragCoord.xy / cell);
    vec3 ground = mod(cells.x + cells.y, 2.0) < 1.0 ? plate.primary.rgb : plate.secondary.rgb;
    vec3 shade = shown > 1 ? vec3(texel[shown - 2]) : shown == 1 ? texel.rgb * texel.a + (1.0 - texel.a) * ground : texel.rgb;
    fragColor = vec4(targeted(shade), 1.0);
}
"""
        )
        interface = gpu.types.GPUStageInterfaceInfo("rasm_channeled")
        interface.smooth("VEC2", "uv")
        info = gpu.types.GPUShaderCreateInfo()
        info.typedef_source("struct ChannelPlate { vec4 rect; vec4 primary; vec4 secondary; };")
        info.uniform_buf(0, "ChannelPlate", "plate")
        info.vertex_in(0, "VEC2", "pos")
        info.vertex_out(interface)
        info.sampler(0, "FLOAT_2D", "image")
        info.push_constant("MAT4", "ModelViewProjectionMatrix")
        info.push_constant("FLOAT", "cell")
        info.push_constant("INT", "shown")
        info.push_constant("BOOL", "srgbTarget")
        info.fragment_out(0, "VEC4", "fragColor")
        info.vertex_source(vertex)
        info.fragment_source(fragment)
        shader = gpu.shader.create_from_info(info)
        return shader, batch_for_shader(shader, "TRI_STRIP", {"pos": ((0.0, 0.0), (1.0, 0.0), (0.0, 1.0), (1.0, 1.0))})


class Tint(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Coverage plane of a frame and the display color it tints."""

    plane: npt.NDArray[np.float32]
    color: Color


class Masks(msgspec.Struct, frozen=True, kw_only=True):
    """Coverage masks part: tints baked into textures once per refresh, drawn over the frame's rectangle."""

    source: Callable[[Metrics], BlenderResult[Option[tuple[Tint, ...]]]]

    def read(self, metrics: Metrics) -> BlenderResult[Option[tuple[gpu.types.GPUTexture, ...]]]:
        """Reads the tints and uploads each one's color with alpha times coverage."""
        return self.source(metrics).bind(
            lambda found: found.map(
                lambda tints: accumulated(
                    uploaded(np.concatenate((np.broadcast_to(np.asarray(tint.color[:3], np.float32), (*tint.plane.shape, 3)), (tint.plane * tint.color[3])[..., None]), axis=-1)) for tint in tints
                ).map(Some)
            ).default_value(Ok(Nothing))
        )

    @staticmethod
    def drawn(canvas: Canvas, state: tuple[gpu.types.GPUTexture, ...], /) -> None:
        """Draws every mask over the frame of the canvas's image editor."""
        for space, texture, region in ((space, texture, region) for space in canvas.space.bind(edited).to_list() for texture in state for region in canvas.region.to_list()):
            frame = framed(region)
            texture.filter_mode(space.zoom[0] < 1.0)  # type: ignore[index]
            canvas.picture(texture, frame, gpu.shader.from_builtin("IMAGE_COLOR"))


class Access(msgspec.Struct, frozen=True, kw_only=True):
    """Admitted region snapshot and a writer retaining its actual native owner."""

    quad: Quad
    write: Callable[[Quad], BlenderResult[None]]


class Edges(msgspec.Struct, frozen=True, kw_only=True):
    """Edge part of a free region: the closed corner strip read once per refresh."""

    access: Callable[[bpy.types.Context], BlenderResult[Option[Access]]]
    role: Callable[[bpy.types.ThemeUserInterface], mathutils.Color | bpy.types.bpy_prop_array | Iterable[float]]

    def read(self, _metrics: Metrics, /) -> BlenderResult[Option[npt.NDArray[np.float32]]]:
        """Reads the region's quad on the shown image and closes its corner strip in frame fractions."""
        return self.access(bpy.context).map(lambda found: found.map(lambda held: held.quad.samples[(0, 1, 2, 3, 0), :2].astype(np.float32)))

    def drawn(self, canvas: Canvas, state: npt.NDArray[np.float32], /) -> None:
        """Draws the strip scaled through the frame using its corner handle's native theme projection."""
        lines = canvas.lines(1.0)
        lines.uniform_float("color", color(self.role(bpy.context.preferences.themes[0].user_interface)))  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
        for frame in canvas.region.map(framed).to_list():
            batch_for_shader(lines, "LINE_STRIP", {"pos": state * (frame.width, frame.height) + (frame.x, frame.y)}).draw(lines)  # type: ignore[dict-item]  # ty: ignore[invalid-argument-type]


class Aligned(msgspec.Struct, frozen=True, kw_only=True):
    """Axis-aligned region with optional aspect lock and admitted pointer bounds."""

    locked: bool = False
    bounds: Option[Region] = Nothing


class Free(msgspec.Struct, frozen=True, kw_only=True):
    """Four corners edited independently or translated, rotated, and scaled together."""


class Picked(msgspec.Struct, frozen=True, kw_only=True):
    """Pick point in region pixels and the sampled square's half side."""

    x: Annotated[int, msgspec.Meta(title="X", description="Pick column in region pixels"), Definition(options=frozenset({"HIDDEN", "SKIP_SAVE"}))] = 0
    y: Annotated[int, msgspec.Meta(title="Y", description="Pick row in region pixels"), Definition(options=frozenset({"HIDDEN", "SKIP_SAVE"}))] = 0
    radius: Annotated[int, msgspec.Meta(ge=0, le=3, title="Sample Radius", description="Frame pixels sampled on each side of the picked pixel")] = 0


class EntryPick(Picked, frozen=True, kw_only=True):
    """Pick naming the effect entry it reads."""

    name: EffectName


class Bound(Line, frozen=True, kw_only=True):
    """Rows an identity builds at realization, drawn while the shown condition holds."""

    rows: Callable[[Identity], BlenderResult[tuple[Row, ...]]]
    shown: Option[Setting] = Nothing

    def drawn(self, identity: Identity) -> BlenderResult[RowDraw]:
        """Builds the rows over the identity and realizes them on one grid."""

        def draw(draws: tuple[RowDraw, ...]) -> RowDraw:
            inner = joined(draws)

            def shown(layout: bpy.types.UILayout, holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> None:
                if self.shown.filter(lambda setting: not holds(setting, context)).is_none():
                    inner(layout, holder, context)

            return shown

        return self.rows(identity).bind(lambda rows: placed(rows, identity)).map(draw)


class CompareView(msgspec.Struct, frozen=True, kw_only=True):
    """Row placing a before frame over the editor's frame, split by a handle or set beside it."""

    key: Key
    glyph: Glyph
    read: Callable[[Identity, Metrics], BlenderResult[Option[Before]]]
    kinds: tuple[str, ...] = ("RENDER_RESULT",)
    refreshes: Callable[[Identity], BlenderResult[tuple[Refresh, ...]]] = lambda _identity: Ok(())

    @property
    def state(self) -> Attachment[Gated[Compare]]:
        """Screen's compare record, which the preferences page acquires."""
        return Attachment(key=self.key, form=Gated[Compare], owner=bpy.types.Screen)

    @property
    def header(self) -> tuple[Row]:
        """Gate toggle on the glyph, mode segments, and swap, drawn while the editor shows a kind the row names."""

        def rows(identity: Identity) -> BlenderResult[tuple[Row, ...]]:
            enabled = next(field for field in msgspec.structs.fields(Gated[Compare]) if field.name == "enabled")
            value = next(field for field in msgspec.structs.fields(Gated[Compare]) if field.name == "value")
            return self.state.bound(identity).map(
                lambda attached: (
                    Property(target=Member(root=bpy.types.Screen, path=f"{attached.name}.{enabled.name}"), icon=Some(self.glyph), icon_only=True),
                    *(
                        Property(
                            target=Member(root=bpy.types.Screen, path=f"{attached.name}.{value.name}.{field.name}"),
                            icon=Some("ARROW_LEFTRIGHT") if field.name == "swapped" else Nothing,
                            icon_only=field.name == "swapped",
                        )
                        for field in msgspec.structs.fields(Compare)
                        if field.name != "split"
                    ),
                )
            )

        return (Bound(rows=rows, shown=Some(image_type(self.kinds))),)

    def acquisitions(self, identity: Identity) -> BlenderResult[Acquisitions]:
        """Answers the split handle and overlay declarations over the bound record and the identity's refreshes."""
        shown = image_type(self.kinds)

        def ruled(region: bpy.types.Region, compare: Compare) -> Option[tuple[int, int]]:
            view = region.view2d
            low: tuple[float, float] = view.region_to_view(0.0, 0.0)  # type: ignore[assignment]
            high: tuple[float, float] = view.region_to_view(region.width, region.height)  # type: ignore[assignment]
            (left, bottom), (right, top) = low, high
            middle = ((max(left, 0.0) + min(right, 1.0)) / 2, (max(bottom, 0.0) + min(top, 1.0)) / 2)
            match compare.mode:
                case Mode.VERTICAL:
                    return Some(view.view_to_region(compare.split, middle[1], clip=False))  # type: ignore[arg-type]
                case Mode.HORIZONTAL:
                    return Some(view.view_to_region(middle[0], 1.0 - compare.split, clip=False))  # type: ignore[arg-type]
                case Mode.SIDE:
                    return Nothing

        def rows(attached: Attached[Gated[Compare]], refreshes: tuple[Refresh, ...]) -> tuple[Registrable, ...]:
            def place(context: bpy.types.Context) -> BlenderResult[Option[Matrix]]:
                return held_compare(attached, context).map(
                    lambda found: found.bind(lambda compare: root_in(bpy.types.Region, context).bind(lambda region: ruled(region, compare))).map(lambda at: Matrix.Translation((*at, 0.0)))
                )

            def write(screen: bpy.types.Screen, split: float) -> BlenderResult[None]:
                return attached.read(screen).bind(lambda gated: attached.write(screen, msgspec.structs.replace(gated, value=msgspec.structs.replace(gated.value, split=split))))

            def begin(context: bpy.types.Context, _travel: Travel) -> BlenderResult[tuple[bpy.types.Screen, Compare, Rect]]:
                return (
                    held_compare(attached, context)
                    .bind(lambda found: found.filter(lambda compare: compare.mode is not Mode.SIDE).to_result_with(lambda: (BlenderAbsentFault(owner=repr(context.screen), key=self.key),)))
                    .bind(
                        lambda compare: paired(
                            root_in(bpy.types.Screen, context).to_result_with(lambda: (BlenderAbsentFault(owner=repr(context), key="screen"),)),
                            root_in(bpy.types.Region, context).to_result_with(lambda: (BlenderAbsentFault(owner=repr(context), key="region"),)),
                        ).map(lambda pair: (pair[0], compare, framed(pair[1])))
                    )
                )

            def preview(_context: bpy.types.Context, state: tuple[bpy.types.Screen, Compare, Rect], travel: Travel) -> BlenderResult[Option[str]]:
                screen, compare, frame = state
                change = travel.dx / frame.width if compare.mode is Mode.VERTICAL else -travel.dy / frame.height
                return write(screen, min(max(compare.split + change * (0.1 if travel.precise else 1.0), 0.0), 1.0)).map(lambda _: Nothing)

            command = Operator(
                key=f"{self.key}_split",
                title="Compare Split",
                description="Drag the compare split; Shift changes precision and Escape restores the split",
                form=Empty,
                run=lambda _call: Ok(Done()),
                invoke=Some(Modal(gesture=Gesture.DRAG, begin=begin, preview=preview, restore=lambda _context, state: write(state[0], state[1].split))),
                condition=Some(shown),
                internal=True,
            )
            handle = Move(place=place, bind=command, scale_basis=Some(HANDLE), draw_style=Some("RING_2D"))
            return (
                command,
                Gizmos(key=self.key, title="Compare", space="IMAGE_EDITOR", parts=(handle,), condition=Some(shown)),
                overlaid(Wipe(identity=identity, source=lambda metrics: self.read(identity, metrics), compare=Some(functools.partial(held_compare, attached))), shown, refreshes),
            )

        return paired(self.state.bound(identity), self.refreshes(identity)).map(lambda pair: rows(*pair))


class FrameMask(msgspec.Struct, frozen=True, kw_only=True):
    """Row drawing coverage masks over the editor's frame."""

    read: Callable[[Metrics], BlenderResult[Option[tuple[Tint, ...]]]]
    kinds: tuple[str, ...] = ("RENDER_RESULT",)
    refreshes: tuple[Refresh, ...] = ()

    def acquisitions(self, _identity: Identity) -> BlenderResult[Acquisitions]:
        """Answers the mask overlay declaration."""
        return Ok((overlaid(Masks(source=self.read), image_type(self.kinds), self.refreshes),))


class FrameRegion(msgspec.Struct, frozen=True, kw_only=True):
    """Native corner, edge, and affine handles with fallible gestures and rollback."""

    key: Key
    title: str
    access: Callable[[bpy.types.Context], BlenderResult[Option[Access]]]
    shape: Shape = Aligned()
    kinds: tuple[str, ...] = ("RENDER_RESULT",)
    refreshes: tuple[Refresh, ...] = ()

    def acquisitions(self, _identity: Identity) -> BlenderResult[Acquisitions]:
        """Answers retained gesture operators, native handles, and the region outline."""
        return Ok(self.rows(image_type(self.kinds)))

    def rows(self, shown: Setting) -> tuple[Registrable, ...]:
        """Returns each operator before its native handle and the region outline."""
        corners = tuple(range(len(Quad.__struct_fields__)))
        parts: tuple[RegionPart, ...] = (
            *corners,
            *(tuple((index, (index + 1) % len(corners)) for index in corners) if isinstance(self.shape, Aligned) and not self.shape.locked else ()),
            Motion.TRANSLATE,
            *((Motion.ROTATE, Motion.SCALE) if isinstance(self.shape, Free) else ()),
        )

        def handle(part: RegionPart, order: int) -> tuple[Operator[Empty, tuple[Access, Rect]], Move]:
            def point(quad: Quad, frame: Rect) -> npt.NDArray[np.float64]:
                samples = quad.samples[:, :2] * (frame.width, frame.height) + (frame.x, frame.y)
                match part:
                    case int(index):
                        return samples[index, :]
                    case tuple(edge):
                        return samples[list(edge)].mean(axis=0)
                    case Motion.TRANSLATE:
                        return samples.mean(axis=0)
                    case Motion.ROTATE:
                        return np.add(samples.mean(axis=0), (0.0, HANDLE * 2))
                    case Motion.SCALE:
                        return np.add(samples.mean(axis=0), (HANDLE * 2, 0.0))

            def place(context: bpy.types.Context) -> BlenderResult[Option[Matrix]]:
                return self.access(context).map(
                    lambda found: found.bind(lambda held: root_in(bpy.types.Region, context).map(lambda region: Matrix.Translation((*point(held.quad, framed(region)), 0.0))))
                )

            def begin(context: bpy.types.Context, _travel: Travel) -> BlenderResult[tuple[Access, Rect]]:
                return paired(
                    self.access(context).bind(lambda found: found.to_result_with(lambda: (BlenderAbsentFault(owner=repr(context.area), key=self.key),))),
                    root_in(bpy.types.Region, context).to_result_with(lambda: (BlenderAbsentFault(owner=repr(context), key="region"),)),
                ).map(lambda pair: (pair[0], framed(pair[1])))

            def preview(_context: bpy.types.Context, state: tuple[Access, Rect], travel: Travel) -> BlenderResult[Option[str]]:
                held, frame = state
                pixels = held.quad.samples[:, :2] * (frame.width, frame.height)
                delta = np.array((travel.dx, travel.dy), dtype=np.float64) * (0.1 if travel.precise else 1.0)
                center = pixels.mean(axis=0)
                bounds = (
                    self.shape.bounds.map(lambda region: np.array(((region.left, region.bottom), (region.right, region.top))) * (frame.width, frame.height))
                    if isinstance(self.shape, Aligned)
                    else Nothing
                )
                match part:
                    case Motion.TRANSLATE:
                        movement = bounds.map(lambda limits: np.clip(delta, limits[0] - pixels.min(axis=0), limits[1] - pixels.max(axis=0))).default_value(delta)
                        changed = pixels + movement
                    case Motion.ROTATE:
                        angle = math.atan2(-delta[0], HANDLE * 2 + delta[1])
                        cosine, sine = math.cos(angle), math.sin(angle)
                        changed = (pixels - center) @ np.array(((cosine, sine), (-sine, cosine))) + center
                    case Motion.SCALE:
                        changed = (pixels - center) * math.hypot(HANDLE * 2 + delta[0], delta[1]) / (HANDLE * 2) + center
                    case int(index) if isinstance(self.shape, Free):
                        changed = pixels + np.where((np.arange(len(corners)) == index)[:, None], delta, 0.0)
                    case int(index):
                        opposite = (index + len(corners) // 2) % len(corners)
                        selected = bounds.map(lambda limits: np.clip(pixels[index] + delta, limits[0], limits[1])).default_value(pixels[index] + delta)
                        if isinstance(self.shape, Aligned) and self.shape.locked:
                            diagonal = pixels[index] - pixels[opposite]
                            requested = float(np.linalg.lstsq(diagonal[:, None], selected - pixels[opposite], rcond=None)[0][0])
                            maximum = bounds.map(
                                lambda limits: min(((limits[1 if value > 0 else 0, axis] - pixels[opposite, axis]) / value for axis, value in enumerate(diagonal) if value != 0), default=math.inf)
                            ).default_value(math.inf)
                            changed = (pixels - pixels[opposite]) * min(max(requested, 0.0), maximum) + pixels[opposite]
                        else:
                            low, high = np.minimum(selected, pixels[opposite]), np.maximum(selected, pixels[opposite])
                            changed = np.array(((low[0], high[1]), high, (high[0], low[1]), low))
                    case tuple(edge):
                        axis = 1 if edge[0] % 2 == 0 else 0
                        selected = bounds.map(lambda limits: np.clip(pixels[edge[0], axis] + delta[axis], limits[0, axis], limits[1, axis])).default_value(pixels[edge[0], axis] + delta[axis])
                        changed = pixels.copy()
                        changed[list(edge), axis] = selected
                return held.write(Quad.of(np.column_stack((changed / (frame.width, frame.height), np.ones(len(corners)))))).map(lambda _: Nothing)

            command = Operator(
                key=f"{self.key}_{order}",
                title=self.title,
                description="Drag the region handle; Shift changes precision and Escape restores the region",
                form=Empty,
                run=lambda _call: Ok(Done()),
                invoke=Some(Modal(gesture=Gesture.DRAG, begin=begin, preview=preview, restore=lambda _context, state: state[0].write(state[0].quad))),
                condition=Some(shown),
                internal=True,
            )
            return command, Move(place=place, bind=command, scale_basis=Some(HANDLE), draw_style=Some("CROSS_2D" if isinstance(part, int) else "RING_2D"))

        pairs = tuple(handle(part, order) for order, part in enumerate(parts))
        moves = tuple(move for _, move in pairs)
        return (
            *(command for command, _ in pairs),
            Gizmos(key=self.key, title=self.title, space="IMAGE_EDITOR", parts=moves, condition=Some(shown)),
            overlaid(Edges(access=self.access, role=moves[0].role), shown, self.refreshes),
        )


class MeterRegion(msgspec.Struct, frozen=True, kw_only=True):
    """Native bounded handles for the gated meter region of a scene's metering record."""

    attachment: Attachment[Metering]

    def acquisitions(self, identity: Identity) -> BlenderResult[Acquisitions]:
        """Answers the aligned native region over the bound metering record."""

        def rows(attached: Attached[Metering]) -> tuple[FrameRegion]:
            def access(context: bpy.types.Context) -> BlenderResult[Option[Access]]:
                def read(scene: bpy.types.Scene) -> BlenderResult[Option[Access]]:
                    def write(quad: Quad) -> BlenderResult[None]:
                        low, high = quad.samples[:, :2].min(axis=0), quad.samples[:, :2].max(axis=0)
                        left, bottom = low
                        right, top = high
                        return metered(attached, scene, Region(left=float(left), bottom=float(bottom), right=float(right), top=float(top)))

                    return attached.read(scene).map(
                        lambda held: (
                            Some(held.meter.region)
                            .filter(lambda gated: gated.enabled)
                            .map(
                                lambda gated: Access(
                                    quad=Quad(
                                        top_left=(gated.value.left, gated.value.top),
                                        top_right=(gated.value.right, gated.value.top),
                                        bottom_right=(gated.value.right, gated.value.bottom),
                                        bottom_left=(gated.value.left, gated.value.bottom),
                                    ),
                                    write=write,
                                )
                            )
                        )
                    )

                return root_in(bpy.types.Scene, context).map(read).default_value(Ok(Nothing))

            return (FrameRegion(key="meter_region", title="Meter Region", access=access, shape=Aligned(bounds=Some(Region()))),)

        return self.attachment.bound(identity).map(rows)


class Frames(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Last render's scene-linear chart frame, its display-encoded plane, and the integer depth its output writes."""

    linear: ChartFrame
    formed: npt.NDArray[np.float32]
    depth: Option[IntegerDepth]


class FrameCharts(msgspec.Struct, frozen=True, kw_only=True):
    """Row drawing the last render's scene-linear outline, chromaticity, band key, and clip ends over the Render Result."""

    key: ClassVar[Key] = "charts"
    under: ClassVar[Color] = (0.0, 0.0, 1.0, 1.0)
    over: ClassVar[Color] = (1.0, 0.0, 0.0, 1.0)
    switches: ClassVar[Attachment[Charted]] = Attachment(key=key, form=Charted, owner=bpy.types.Screen)
    ends: ClassVar[Attachment[ClipEnds]] = Attachment(key=f"{key}_clips", form=ClipEnds, owner=bpy.types.Screen)
    page_rows: ClassVar[tuple[Registrable, ...]] = (switches, ends)
    low: Glyph
    high: Glyph

    @property
    def clips(self) -> tuple[Row]:
        """Low and high clip toggles, icon only, drawn while the editor shows the Render Result."""

        def rows(identity: Identity) -> BlenderResult[tuple[Row, ...]]:
            return self.ends.bound(identity).map(
                lambda attached: (
                    Caption(
                        rows=(
                            *(
                                Property(target=Member(root=bpy.types.Screen, path=f"{attached.name}.{field.name}"), icon=Some(self.low if field.name == "low" else self.high), icon_only=True)
                                for field in msgspec.structs.fields(ClipEnds)
                            ),
                        ),
                        flow=Flow.GRID,
                    ),
                )
            )

        return (Bound(rows=rows, shown=Some(RESULT)),)

    def section(self) -> Single:
        """Charts section over the chart switches, inactive while the editor shows another image."""

        def rows(identity: Identity) -> BlenderResult[tuple[Row, ...]]:
            return self.switches.bound(identity).map(
                lambda attached: tuple(Property(target=Member(root=bpy.types.Screen, path=f"{attached.name}.{row.name}")) for row in msgspec.structs.fields(Charted))
            )

        return Single(key=self.key, title="Charts", rows=(Bound(rows=rows),), condition=Some(RESULT))

    def acquisitions(self, identity: Identity) -> BlenderResult[Acquisitions]:
        """Answers the render read, chart overlays, and clip-end mask declarations."""
        signal = Signal()
        last: Option[Frames] = Nothing

        def consumed(scene: bpy.types.Scene, path: str) -> BlenderResult[Option[str]]:
            settings = scene.render.image_settings
            depth = Option.of_optional(next((held for held in get_args(IntegerDepth.__value__) if not settings.has_linear_colorspace and held == int(settings.color_depth)), None))

            def kept(frames: Frames) -> BlenderResult[Option[str]]:
                nonlocal last
                last = Some(frames)
                return signal.fired().map(lambda _: Nothing)

            def read(view_layer: bpy.types.ViewLayer) -> BlenderResult[Frames]:
                layer = f"{view_layer.name}.Combined"
                return paired(
                    paired(plane(path, ("", layer), ("R", "G", "B", "A")), formation.current(identity, scene).map(lambda found: found.default_with(lambda: viewed(scene)))), config_of()
                ).bind(
                    lambda found: (
                        plane(path, ("Composite.Combined", "", layer), ("R", "G", "B", "A"))
                        .bind(lambda pixels: displayed(formation, identity, scene, pixels, PyOpenColorIO.ROLE_SCENE_LINEAR, found[1]))
                        .map(lambda file: Frames(linear=ChartFrame(pixels=found[0][0], view=found[0][1]), formed=file.pixels, depth=depth))
                    )
                )

            return (
                Result[bpy.types.ViewLayer, BlenderFaults]
                .bind(Option.of_optional(bpy.context.view_layer).to_result_with(lambda: (BlenderAbsentFault(owner=repr(bpy.context), key="view_layer"),)), read)
                .bind(kept)
            )

        def rows(switched: Attached[Charted], clipped: Attached[ClipEnds]) -> tuple[Registrable, ...]:
            refreshes: tuple[Refresh, ...] = (signal, *(Watch(owner=group.struct, identifier=row.name) for group in (switched.group, clipped.group) for row in msgspec.structs.fields(group.record)))

            def charted(on: Callable[[Charted], bool]) -> BlenderResult[Option[ChartFrame]]:
                return (
                    root_in(bpy.types.Screen, bpy.context)
                    .map(lambda screen: switched.read(screen).map(lambda held: last.filter(lambda _: on(held)).map(lambda frames: frames.linear)))
                    .default_value(Ok(Nothing))
                )

            def tints() -> BlenderResult[Option[tuple[Tint, ...]]]:
                def read(screen: bpy.types.Screen) -> BlenderResult[Option[tuple[Tint, ...]]]:
                    return clipped.read(screen).map(
                        lambda lit: (
                            last
                            .bind(lambda frames: frames.depth.map(lambda depth: Clipping.of(codes(unassociated(frames.formed), depth))))
                            .map(lambda clip: tuple(Tint(plane=mask.astype(np.float32), color=ink) for mask, ink, on in ((clip.low, self.under, lit.low), (clip.high, self.over, lit.high)) if on))
                            .filter(bool)
                        )
                    )

                return root_in(bpy.types.Screen, bpy.context).map(read).default_value(Ok(Nothing))

            return (
                ResultRead(consume=consumed),
                overlaid(Outline(read=functools.partial(histogram, lambda: charted(lambda held: held.outline))), RESULT, refreshes),
                overlaid(Gamuts(source=lambda: charted(lambda held: held.gamuts)), RESULT, refreshes),
                overlaid(FalseColor(source=lambda: Ok(last.map(lambda frames: frames.linear.view))), RESULT, refreshes),
                FrameMask(read=lambda _: tints(), refreshes=refreshes),
            )

        return paired(self.switches.bound(identity), self.ends.bound(identity)).map(lambda pair: rows(*pair))


# --- [OPERATIONS] -----------------------------------------------------------------------


def framed(region: bpy.types.Region) -> Rect:
    """Answers the shown frame's rectangle in region pixels."""
    view = region.view2d
    low: tuple[float, float] = view.region_to_view(0.0, 0.0)  # type: ignore[assignment]
    high: tuple[float, float] = view.region_to_view(1.0, 1.0)  # type: ignore[assignment]
    (left, bottom), (right, top) = low, high
    across, up = right - left, top - bottom
    return Rect(x=-left / across, y=-bottom / up, width=1.0 / across, height=1.0 / up)


def edited(space: bpy.types.Space) -> Option[bpy.types.SpaceImageEditor]:
    """Answers a space as the image editor it is, `Nothing` for another editor."""
    match space:
        case bpy.types.SpaceImageEditor():
            return Some(space)
        case _:
            return Nothing


def editor(context: bpy.types.Context) -> BlenderResult[bpy.types.SpaceImageEditor]:
    """Answers the image editor the context's area shows, `absent` for another editor."""
    return Option.of_optional(context.space_data).bind(edited).to_result_with(lambda: (BlenderAbsentFault(owner=repr(context), key=bpy.types.SpaceImageEditor.__name__),))


def held_compare(attached: Attached[Gated[Compare]], context: bpy.types.Context) -> BlenderResult[Option[Compare]]:
    """Answers the screen's compare while its gate holds."""
    return (
        root_in(bpy.types.Screen, context)
        .map(lambda screen: attached.read(screen).map(lambda gated: Some(gated).filter(lambda held: held.enabled).map(lambda held: held.value)))
        .default_value(Ok(Nothing))
    )


def overlaid[S](part: Part[S], shown: Setting, refreshes: tuple[Refresh, ...]) -> Overlay[S]:
    """Places a part over the image editor's window region while its setting holds."""
    area = AreaQuery(space="IMAGE_EDITOR", ui_type=Some("IMAGE_EDITOR"), shows=Some(shown))
    return Overlay(drawing=Drawing(area=area, part=part, refreshes=refreshes), space=bpy.types.SpaceImageEditor, scissor_enabled=False)


def metered(attached: Attached[Metering], scene: bpy.types.Scene, region: Region) -> BlenderResult[None]:
    """Writes a region into the scene's meter with its gate on."""
    return attached.read(scene).bind(lambda held: attached.write(scene, msgspec.structs.replace(held, meter=msgspec.structs.replace(held.meter, region=Gated(enabled=True, value=region)))))


def picking[P: Picked](key: Key, title: str, description: str, cursor: "rna_enums.WindowCursorItems", form: type[P], taken: Callable[[Call[P], Region], BlenderResult[Outcome]]) -> Operator[P]:
    """Answers the operator row converting a pending pick into a frame region its edit reads."""
    window = Setting(descriptor=Descriptor(root=bpy.types.Context, owner="region", prop=bpy.types.Region.bl_rna.properties["type"]), accepted=("WINDOW",))

    def ran(call: Call[P]) -> BlenderResult[Outcome]:
        context, record = call.context, call.record

        def regioned(space: bpy.types.SpaceImageEditor, region: bpy.types.Region) -> BlenderResult[Region]:
            frame, zoom = framed(region), space.zoom
            width, height = round(frame.width / zoom[0]), round(frame.height / zoom[1])  # type: ignore[index]
            column, row = math.floor((record.x - frame.x) / zoom[0]), math.floor((record.y - frame.y) / zoom[1])  # type: ignore[index]
            pixel = {"left": column / width, "bottom": row / height, "right": (column + 1) / width, "top": (row + 1) / height}
            return (
                catch(lambda: msgspec.convert(pixel, Region), exception=msgspec.ValidationError)()
                .map(
                    lambda at: Region(
                        left=max(at.left - record.radius / width, 0.0),
                        bottom=max(at.bottom - record.radius / height, 0.0),
                        right=min(at.right + record.radius / width, 1.0),
                        top=min(at.top + record.radius / height, 1.0),
                    )
                )
                .map_error(lambda error: (BlenderReadFault(request=repr(space), error=error),))
            )

        return (
            check(AllOf(conditions=(RESULT, window)), context)
            .bind(lambda _: paired(editor(context), call.root(bpy.types.Region)))
            .bind(lambda pair: regioned(*pair))
            .bind(lambda region: taken(call, region))
        )

    return Operator(
        key=key,
        title=title,
        description=description,
        form=form,
        run=ran,
        invoke=Some(Pick(cursor=cursor)),
        seed=Some(lambda record, event: msgspec.structs.replace(record, x=event.mouse_region_x, y=event.mouse_region_y)),
    )


def revealed(context: bpy.types.Context) -> BlenderResult[Option[Located]]:
    """Answers the largest editor showing the Render Result, opening the render view where none does."""
    return Result[bpy.types.WindowManager, BlenderFaults].bind(
        root_in(bpy.types.WindowManager, context).to_result_with(lambda: (BlenderAbsentFault(owner=repr(context), key="window_manager"),)),
        lambda manager: (
            AreaQuery(space="IMAGE_EDITOR", ui_type=Some("IMAGE_EDITOR"), shows=Some(RESULT))
            .first(manager.windows)
            .map(Some)
            .or_else_with(lambda _: called(bpy.ops.render.view_show, {}, context="INVOKE_DEFAULT").map(lambda _: Nothing))
        ),
    )


def showing(image: bpy.types.Image) -> BlenderResult[EditorState]:
    """Answers the image editor state showing an image."""
    return EditorState.of(bpy.types.SpaceImageEditor, {"image": IdName.of(image)})


# --- [COMPOSITION] ----------------------------------------------------------------------

RESULT: Final = image_type(("RENDER_RESULT",))
meter_pick = picking(
    "pick_meter",
    "Pick Meter Region",
    "Meter the next frame over the picked square",
    "CROSSHAIR",
    Picked,
    lambda call, region: (
        paired(AutoExposure.attachment.bound(call.identity), call.root(bpy.types.Scene))
        .bind(lambda pair: metered(pair[0], pair[1], region))
        .map(lambda _: Done(notes=(("INFO", "Metering the picked region"),)))
    ),
)
meter_region = MeterRegion(attachment=AutoExposure.attachment)


def read_pick(viewer: EffectViewer) -> Operator[EntryPick]:
    """Answers the pick handing the picked square of the frame entering an entry to the read its row holds."""
    return picking(
        "pick_read",
        "Pick Read Region",
        "Hand the picked square of the frame entering the entry to the entry's own read",
        "EYEDROPPER",
        EntryPick,
        lambda call, region: viewer.reading.run(Call(identity=call.identity, context=call.context, record=EntryRegion(name=call.record.name, region=region))),
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "HANDLE",
    "RESULT",
    "Access",
    "Aligned",
    "Before",
    "Bound",
    "Charted",
    "ClipEnds",
    "Compare",
    "CompareView",
    "EntryPick",
    "FrameCharts",
    "FrameMask",
    "FrameRegion",
    "Free",
    "MeterRegion",
    "Mode",
    "Picked",
    "Shape",
    "Shown",
    "Tint",
    "edited",
    "editor",
    "framed",
    "meter_pick",
    "meter_region",
    "metered",
    "overlaid",
    "picking",
    "read_pick",
    "revealed",
    "showing",
]
