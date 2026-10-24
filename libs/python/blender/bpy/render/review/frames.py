"""Image editor frame compare, charts, clip masks, regions, and picks."""

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
from blender.bpy.data.rna.descriptors import Descriptor, IdName, root_in, rooted
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions, Registrable
from blender.bpy.faults import BlenderAbsentFault, BlenderFaults, BlenderReadFault, BlenderResult
from blender.bpy.render.color.exposure import AutoExposure, Metering
from blender.bpy.render.color.spaces import config_of
from blender.bpy.render.effects.stack import EffectName, formation, viewed
from blender.bpy.render.effects.viewers import EffectViewer, EntryRegion
from blender.bpy.render.outputs.exports import displayed, image_type, plane
from blender.bpy.settings.attachments import Attached, Attachment
from blender.bpy.settings.properties import Definition
from blender.bpy.ui.drawing.canvas import Align, Canvas, compiled, DECODED, Rect, Shadow
from blender.bpy.ui.drawing.charts import ChartFrame, FalseColor, Gamuts, histogram, Outline
from blender.bpy.ui.drawing.handlers import Drawing, Overlay, Part, Refresh, Signal, Watch
from blender.bpy.ui.layout.rows import Caption, Divider, Flow, joined, Line, Member, placed, Property, Row, RowDraw
from blender.bpy.ui.layout.sections import Single
from blender.bpy.ui.resources.icons import Glyph
from blender.bpy.ui.resources.textures import uploaded
from blender.bpy.ui.resources.theme import Color, color, fixed, Metrics, Shipped
from blender.bpy.ui.screens.areas import AreaQuery, Located, NewWindow, windowed
from blender.bpy.ui.screens.editors import EditorState
from blender.bpy.ui.surfaces.gizmos import Cage, Gizmos, Handler, MatrixValues, Move
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
type RegionPart = int | Motion


class Motion(Enum):
    """Whole-region gestures beside corner and edge handles."""

    TRANSLATE = auto()
    ROTATE = auto()
    SCALE = auto()


class Mode(Choice):
    """Compare layout, a wipe on either axis or side by side."""

    VERTICAL = 1, "Vertical Wipe"
    HORIZONTAL = 2, "Horizontal Wipe"
    SIDE = 3, "Side by Side"


class Shown(IntEnum):
    """Channel views the before frame follows, each value a shader selector."""

    COLOR = 0
    COLOR_ALPHA = 1
    RED = 2
    GREEN = 3
    BLUE = 4
    ALPHA = 5


# --- [CONSTANTS] ------------------------------------------------------------------------

HANDLE: Final = 80 * 0.35 / 2
UNIT: Final[MatrixValues] = (1.0, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 0.0, 1.0)


# --- [MODELS] ---------------------------------------------------------------------------


class Compare(msgspec.Struct, frozen=True, kw_only=True):
    """Compare layout, split, and before frame side."""

    mode: Annotated[Mode, msgspec.Meta(title="Compare Mode", description="Wipe orientation, or both frames side by side")] = Mode.VERTICAL
    split: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, title="Split", description="Wipe position")] = 0.5
    swapped: Annotated[bool, msgspec.Meta(title="Swap", description="Before frame right of or below split")] = False


class Charted(msgspec.Struct, frozen=True, kw_only=True):
    """Render Result chart switches."""

    outline: Annotated[bool, msgspec.Meta(title="Outline", description="Last render's scene-linear luminance outline")] = True
    gamuts: Annotated[bool, msgspec.Meta(title="Chromaticity", description="Last render's chromaticity with working and display gamuts")] = False


class ClipEnds(msgspec.Struct, frozen=True, kw_only=True):
    """Render Result clip-end tints."""

    low: Annotated[bool, msgspec.Meta(title="Low Clip", description="Tint pixels at written file's lowest code")] = False
    high: Annotated[bool, msgspec.Meta(title="High Clip", description="Tint pixels at written file's highest code")] = False


class Before(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Before frame's top-first associated RGBA plane and caption."""

    plane: npt.NDArray[np.float32]
    name: str = "Before"


class Plate(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Uploaded before frame and its caption."""

    texture: gpu.types.GPUTexture
    name: str


class Sides(msgspec.Struct, frozen=True, kw_only=True):
    """Compare draw layout with plate, clip, rule, and captions."""

    after: ClassVar[str] = "After"
    plate: Rect
    clip: Rect
    rule: Option[tuple[tuple[float, float], tuple[float, float]]]
    captions: tuple[tuple[str, float, float, Align], ...]

    @classmethod
    def of(cls, compare: Compare, frame: Rect, bounds: Rect, size: tuple[float, float], name: str, gap: float, inset: float, text: float) -> Self:
        """Lays out compare over a frame inside visible bounds."""
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
    """Formed frame in editor channels with optional compare."""

    light: ClassVar[Color] = Shipped.read().look("overlay-caption").color("text")
    dark: ClassVar[Color] = fixed(Shipped.read().look("overlay-caption").shadows[0].ink)
    identity: Identity
    source: Callable[[Metrics], BlenderResult[Option[Before]]]
    compare: Option[Callable[[bpy.types.Context], BlenderResult[Option[Compare]]]] = Nothing

    def read(self, metrics: Metrics) -> BlenderResult[Option[Plate]]:
        """Uploads before frame plane."""
        return self.source(metrics).bind(
            lambda found: found.map(lambda before: uploaded(unassociated(before.plane)).map(lambda texture: Some(Plate(texture=texture, name=before.name)))).default_value(Ok(Nothing))
        )

    def drawn(self, canvas: Canvas, state: Plate) -> None:
        """Draws plate in editor channel view with rule and captions."""
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
        """Builds channel shader and unit quad."""
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
    """Frame coverage plane with its tint color."""

    plane: npt.NDArray[np.float32]
    color: Color


class Masks(msgspec.Struct, frozen=True, kw_only=True):
    """Coverage mask part baking tints per refresh and drawing them over frame."""

    source: Callable[[Metrics], BlenderResult[Option[tuple[Tint, ...]]]]

    def read(self, metrics: Metrics) -> BlenderResult[Option[tuple[gpu.types.GPUTexture, ...]]]:
        """Reads tints and uploads each color with alpha times coverage."""
        return self.source(metrics).bind(
            lambda found: found.map(
                lambda tints: accumulated(
                    uploaded(np.concatenate((np.broadcast_to(np.asarray(tint.color[:3], np.float32), (*tint.plane.shape, 3)), (tint.plane * tint.color[3])[..., None]), axis=-1)) for tint in tints
                ).map(Some)
            ).default_value(Ok(Nothing))
        )

    @staticmethod
    def drawn(canvas: Canvas, state: tuple[gpu.types.GPUTexture, ...], /) -> None:
        """Draws masks over editor frame."""
        for space, texture, region in ((space, texture, region) for space in canvas.space.bind(edited).to_list() for texture in state for region in canvas.region.to_list()):
            frame = framed(region)
            texture.filter_mode(space.zoom[0] < 1.0)  # type: ignore[index]
            canvas.picture(texture, frame, gpu.shader.from_builtin("IMAGE_COLOR"), blend="ALPHA")


class Access(msgspec.Struct, frozen=True, kw_only=True):
    """Validated region snapshot with owner-bound writer."""

    quad: Quad
    write: Callable[[Quad], BlenderResult[None]]


class Edges(msgspec.Struct, frozen=True, kw_only=True):
    """Free region edge part reading closed corner strip per refresh."""

    access: Callable[[bpy.types.Context], BlenderResult[Option[Access]]]
    role: Callable[[bpy.types.ThemeUserInterface], mathutils.Color | bpy.types.bpy_prop_array | Iterable[float]]

    def read(self, _metrics: Metrics, /) -> BlenderResult[Option[npt.NDArray[np.float32]]]:
        """Reads region quad as closed corner strip in frame fractions."""
        return self.access(bpy.context).map(lambda found: found.map(lambda held: held.quad.samples[(0, 1, 2, 3, 0), :2].astype(np.float32)))

    def drawn(self, canvas: Canvas, state: npt.NDArray[np.float32], /) -> None:
        """Draws strip through frame in corner handle theme color."""
        lines = canvas.lines(1.0)
        lines.uniform_float("color", color(self.role(bpy.context.preferences.themes[0].user_interface)))  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
        for frame in canvas.region.map(framed).to_list():
            batch_for_shader(lines, "LINE_STRIP", {"pos": state * (frame.width, frame.height) + (frame.x, frame.y)}).draw(lines)  # type: ignore[dict-item]  # ty: ignore[invalid-argument-type]


class Aligned(msgspec.Struct, frozen=True, kw_only=True):
    """Axis-aligned region with optional aspect lock and pointer bounds."""

    locked: bool = False
    bounds: Option[Region] = Nothing


class Free(msgspec.Struct, frozen=True, kw_only=True):
    """Four corners edited independently or translated, rotated, and scaled together."""


class Picked(msgspec.Struct, frozen=True, kw_only=True):
    """Pick point and sampled square half side."""

    x: Annotated[int, msgspec.Meta(title="X", description="Pick column in region pixels"), Definition(options=frozenset({"HIDDEN", "SKIP_SAVE"}))] = 0
    y: Annotated[int, msgspec.Meta(title="Y", description="Pick row in region pixels"), Definition(options=frozenset({"HIDDEN", "SKIP_SAVE"}))] = 0
    radius: Annotated[int, msgspec.Meta(ge=0, le=3, title="Sample Radius", description="Pixels sampled each side of pick")] = 0


class EntryPick(Picked, frozen=True, kw_only=True):
    """Pick naming the effect entry it reads."""

    name: EffectName


class Bound(Line, frozen=True, kw_only=True):
    """Identity-built rows drawn while shown condition holds."""

    rows: Callable[[Identity], BlenderResult[tuple[Row, ...]]]
    shown: Option[Setting] = Nothing

    def drawn(self, identity: Identity) -> BlenderResult[RowDraw]:
        """Places identity rows on one grid."""

        def draw(draws: tuple[RowDraw, ...]) -> RowDraw:
            inner = joined(draws)

            def shown(layout: bpy.types.UILayout, holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> None:
                if self.shown.filter(lambda setting: not holds(setting, context)).is_none():
                    inner(layout, holder, context)

            return shown

        return self.rows(identity).bind(lambda rows: placed(rows, identity)).map(draw)


class CompareView(msgspec.Struct, frozen=True, kw_only=True):
    """Row placing a before frame over editor frame, split by a handle or beside it."""

    key: Key
    glyph: Glyph
    read: Callable[[Identity, Metrics], BlenderResult[Option[Before]]]
    kinds: tuple[str, ...] = ("RENDER_RESULT",)
    refreshes: Callable[[Identity], BlenderResult[tuple[Refresh, ...]]] = lambda _identity: Ok(())

    @property
    def state(self) -> Attachment[Gated[Compare]]:
        """Screen compare record."""
        return Attachment(key=self.key, form=Gated[Compare], owner=bpy.types.Screen)

    @property
    def prerequisites(self) -> tuple[Registrable, ...]:
        """Compare record split handle and overlay bind."""
        return (self.state,)

    @property
    def header(self) -> tuple[Row]:
        """Gate, mode, and swap toggles, drawn while editor shows an image of `kinds`."""

        def rows(identity: Identity) -> BlenderResult[tuple[Row, ...]]:
            enabled = next(field for field in msgspec.structs.fields(Gated[Compare]) if field.name == "enabled")
            value = next(field for field in msgspec.structs.fields(Gated[Compare]) if field.name == "value")
            mode, _, swapped = Compare.__struct_fields__
            return self.state.bound(identity).map(
                lambda attached: (
                    Property(target=Member(root=bpy.types.Screen, path=f"{attached.name}.{enabled.name}"), icon=Some(self.glyph), icon_only=True),
                    Divider(type="SPACE"),
                    Property(target=Member(root=bpy.types.Screen, path=f"{attached.name}.{value.name}.{mode}"), expand=True),
                    Divider(type="SPACE"),
                    Property(target=Member(root=bpy.types.Screen, path=f"{attached.name}.{value.name}.{swapped}"), icon=Some("ARROW_LEFTRIGHT"), icon_only=True),
                )
            )

        return (Bound(rows=rows, shown=Some(image_type(self.kinds))),)

    def acquisitions(self, identity: Identity) -> BlenderResult[Acquisitions]:
        """Returns split handle and overlay declarations."""
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
            def compared(context: bpy.types.Context) -> BlenderResult[Option[Compare]]:
                return (
                    root_in(bpy.types.Screen, context)
                    .map(lambda screen: attached.read(screen).map(lambda gated: Some(gated).filter(lambda held: held.enabled).map(lambda held: held.value)))
                    .default_value(Ok(Nothing))
                )

            def place(context: bpy.types.Context) -> BlenderResult[Option[Matrix]]:
                return compared(context).map(
                    lambda found: found.bind(lambda compare: root_in(bpy.types.Region, context).bind(lambda region: ruled(region, compare))).map(lambda at: Matrix.Translation((*at, 0.0)))
                )

            def write(screen: bpy.types.Screen, split: float) -> BlenderResult[None]:
                return attached.read(screen).bind(lambda gated: attached.write(screen, msgspec.structs.replace(gated, value=msgspec.structs.replace(gated.value, split=split))))

            def begin(context: bpy.types.Context, _travel: Travel) -> BlenderResult[tuple[bpy.types.Screen, Compare, Rect]]:
                return (
                    compared(context)
                    .bind(lambda found: found.filter(lambda compare: compare.mode is not Mode.SIDE).to_result_with(lambda: (BlenderAbsentFault(owner=repr(context.screen), key=self.key),)))
                    .bind(lambda compare: paired(rooted(bpy.types.Screen, context), rooted(bpy.types.Region, context)).map(lambda pair: (pair[0], compare, framed(pair[1]))))
                )

            def preview(_context: bpy.types.Context, state: tuple[bpy.types.Screen, Compare, Rect], travel: Travel) -> BlenderResult[Option[str]]:
                screen, compare, frame = state
                change = travel.dx / frame.width if compare.mode is Mode.VERTICAL else -travel.dy / frame.height
                return write(screen, min(max(compare.split + change * (0.1 if travel.precise else 1.0), 0.0), 1.0)).map(lambda _: Nothing)

            command = Operator(
                key=f"{self.key}_split",
                title="Compare Split",
                description="Drag compare split, Shift for tenth speed, Escape restores",
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
                overlaid(Wipe(identity=identity, source=lambda metrics: self.read(identity, metrics), compare=Some(compared)), shown, refreshes),
            )

        return paired(self.state.bound(identity), self.refreshes(identity)).map(lambda pair: rows(*pair))


class FrameMask(msgspec.Struct, frozen=True, kw_only=True):
    """Row drawing coverage masks."""

    read: Callable[[Metrics], BlenderResult[Option[tuple[Tint, ...]]]]
    kinds: tuple[str, ...] = ("RENDER_RESULT",)
    refreshes: tuple[Refresh, ...] = ()

    def acquisitions(self, _identity: Identity) -> BlenderResult[Acquisitions]:
        """Returns mask overlay declaration."""
        return Ok((overlaid(Masks(source=self.read), image_type(self.kinds), self.refreshes),))


class FrameRegion(msgspec.Struct, frozen=True, kw_only=True):
    """Region handles with rollback, one cage for aligned regions, corner and affine handles for free quads."""

    key: Key
    title: str
    access: Callable[[bpy.types.Context], BlenderResult[Option[Access]]]
    shape: Shape = Aligned()
    kinds: tuple[str, ...] = ("RENDER_RESULT",)
    refreshes: tuple[Refresh, ...] = ()

    def acquisitions(self, identity: Identity) -> BlenderResult[Acquisitions]:
        """Returns handle group, free quads with their gesture operators and outline."""
        shown = image_type(self.kinds)
        match self.shape:
            case Aligned() as aligned:
                return Ok((Gizmos(key=self.key, title=self.title, space="IMAGE_EDITOR", parts=(self.cage(identity, aligned),), condition=Some(shown)),))
            case Free():
                return Ok(self.moves(shown))

    def held(self, context: bpy.types.Context) -> BlenderResult[Option[tuple[Access, Rect]]]:
        """Reads region access with the region frame holding it."""
        return self.access(context).map(lambda found: found.bind(lambda access: root_in(bpy.types.Region, context).map(lambda region: (access, framed(region)))))

    def cage(self, identity: Identity, aligned: Aligned) -> Cage:
        """Returns cage over the region rectangle in frame pixels, aspect kept while locked and bounds clipping each edit."""

        def matrix(access: Access, frame: Rect) -> MatrixValues:
            pixels = access.quad.samples[:, :2] * (frame.width, frame.height)
            (left, bottom), (right, top) = pixels.min(axis=0), pixels.max(axis=0)
            return (right - left, 0.0, 0.0, 0.0, 0.0, top - bottom, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0, (left + right) / 2.0, (bottom + top) / 2.0, 0.0, 1.0)

        def place(context: bpy.types.Context) -> BlenderResult[Option[Matrix]]:
            return self.held(context).map(lambda found: found.map(lambda pair: Matrix.Translation((pair[1].x, pair[1].y, 0.0))))

        def get(context: bpy.types.Context) -> MatrixValues:
            return identity.reports.reported(self.held)(context).bind(lambda found: found).map(lambda pair: matrix(*pair)).default_value(UNIT)

        def wrote(context: bpy.types.Context, values: MatrixValues) -> BlenderResult[None]:
            def written(access: Access, frame: Rect) -> BlenderResult[None]:
                size = np.array((frame.width, frame.height))
                center, half = np.array((values[12], values[13])) / size, np.abs((values[0], values[5])) / 2.0 / size
                held = access.quad.samples[:, :2]
                moved = np.allclose(half * 2.0 * size, (held.max(axis=0) - held.min(axis=0)) * size, atol=0.5)

                def bounded(region: Region) -> tuple[npt.NDArray[np.float64], npt.NDArray[np.float64]]:
                    floor, ceiling = np.array((region.left, region.bottom)), np.array((region.right, region.top))
                    kept = np.minimum(half, (ceiling - floor) / 2.0)
                    shifted = np.clip(center, floor + kept, ceiling - kept)
                    return (shifted - kept, shifted + kept) if moved else (np.clip(center - half, floor, ceiling), np.clip(center + half, floor, ceiling))

                (left, bottom), (right, top) = aligned.bounds.map(bounded).default_value((center - half, center + half))
                return access.write(
                    Quad(top_left=(float(left), float(top)), top_right=(float(right), float(top)), bottom_right=(float(right), float(bottom)), bottom_left=(float(left), float(bottom)))
                )

            return self.held(context).bind(lambda found: found.to_result_with(lambda: (BlenderAbsentFault(owner=repr(context.area), key=self.key),))).bind(lambda pair: written(*pair))

        return Cage(
            place=place,
            bind=Handler(get=get, set=wrote),
            dimensions=Some((1.0, 1.0)),
            transform=Some(frozenset({"TRANSLATE", "SCALE_UNIFORM"} if aligned.locked else {"TRANSLATE", "SCALE"})),
            draw_style=Some("BOX"),
            draw_options=Some(frozenset({"CORNER_HANDLES"})),
        )

    def moves(self, shown: Setting) -> tuple[Registrable, ...]:
        """Returns each free quad gesture operator before its handle, then outline."""
        corners = tuple(range(len(Quad.__struct_fields__)))
        parts: tuple[RegionPart, ...] = (*corners, Motion.TRANSLATE, Motion.ROTATE, Motion.SCALE)

        def handle(part: RegionPart, order: int) -> tuple[Operator[Empty, tuple[Access, Rect]], Move]:
            def point(quad: Quad, frame: Rect) -> npt.NDArray[np.float64]:
                samples = quad.samples[:, :2] * (frame.width, frame.height) + (frame.x, frame.y)
                match part:
                    case int(index):
                        return samples[index, :]
                    case Motion.TRANSLATE:
                        return samples.mean(axis=0)
                    case Motion.ROTATE:
                        return np.add(samples.mean(axis=0), (0.0, HANDLE * 2))
                    case Motion.SCALE:
                        return np.add(samples.mean(axis=0), (HANDLE * 2, 0.0))

            def place(context: bpy.types.Context) -> BlenderResult[Option[Matrix]]:
                return self.held(context).map(lambda found: found.map(lambda pair: Matrix.Translation((*point(pair[0].quad, pair[1]), 0.0))))

            def begin(context: bpy.types.Context, _travel: Travel) -> BlenderResult[tuple[Access, Rect]]:
                return self.held(context).bind(lambda found: found.to_result_with(lambda: (BlenderAbsentFault(owner=repr(context.area), key=self.key),)))

            def preview(_context: bpy.types.Context, state: tuple[Access, Rect], travel: Travel) -> BlenderResult[Option[str]]:
                held, frame = state
                pixels = held.quad.samples[:, :2] * (frame.width, frame.height)
                delta = np.array((travel.dx, travel.dy), dtype=np.float64) * (0.1 if travel.precise else 1.0)
                center = pixels.mean(axis=0)
                match part:
                    case Motion.TRANSLATE:
                        changed = pixels + delta
                    case Motion.ROTATE:
                        angle = math.atan2(-delta[0], HANDLE * 2 + delta[1])
                        cosine, sine = math.cos(angle), math.sin(angle)
                        changed = (pixels - center) @ np.array(((cosine, sine), (-sine, cosine))) + center
                    case Motion.SCALE:
                        changed = (pixels - center) * math.hypot(HANDLE * 2 + delta[0], delta[1]) / (HANDLE * 2) + center
                    case int(index):
                        changed = pixels + np.where((np.arange(len(corners)) == index)[:, None], delta, 0.0)
                return held.write(Quad.of(np.column_stack((changed / (frame.width, frame.height), np.ones(len(corners)))))).map(lambda _: Nothing)

            command = Operator(
                key=f"{self.key}_{order}",
                title=self.title,
                description="Drag region handle, Shift for tenth speed, Escape restores",
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
    """Bounded handles for scene's gated meter region."""

    attachment: Attachment[Metering]

    def acquisitions(self, identity: Identity) -> BlenderResult[Acquisitions]:
        """Returns aligned meter region."""

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
    """Last render's scene-linear chart frame, display plane, and written integer depth."""

    linear: ChartFrame
    formed: npt.NDArray[np.float32]
    depth: Option[IntegerDepth]


class FrameCharts(msgspec.Struct, frozen=True, kw_only=True):
    """Row drawing last render charts and clip ends over Render Result."""

    key: ClassVar[Key] = "charts"
    under: ClassVar[Color] = Shipped.read().look("clip-under").color("fill")
    over: ClassVar[Color] = Shipped.read().look("clip-over").color("fill")
    switches: ClassVar[Attachment[Charted]] = Attachment(key=key, form=Charted, owner=bpy.types.Screen)
    ends: ClassVar[Attachment[ClipEnds]] = Attachment(key=f"{key}_clips", form=ClipEnds, owner=bpy.types.Screen)
    prerequisites: ClassVar[tuple[Registrable, ...]] = (switches, ends)
    low: Glyph
    high: Glyph

    @property
    def clips(self) -> tuple[Row]:
        """Icon clip toggles drawn while editor shows Render Result."""

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
        """Chart switches section, inactive for another image."""

        def rows(identity: Identity) -> BlenderResult[tuple[Row, ...]]:
            return self.switches.bound(identity).map(
                lambda attached: tuple(Property(target=Member(root=bpy.types.Screen, path=f"{attached.name}.{row.name}")) for row in msgspec.structs.fields(Charted))
            )

        return Single(key=self.key, title="Charts", rows=(Bound(rows=rows),), condition=Some(RESULT))

    def acquisitions(self, identity: Identity) -> BlenderResult[Acquisitions]:
        """Returns render read, chart, and clip mask declarations."""
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

            return Result[bpy.types.ViewLayer, BlenderFaults].bind(rooted(bpy.types.ViewLayer, bpy.context), read).bind(kept)

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
    """Returns frame rectangle in region pixels."""
    view = region.view2d
    low: tuple[float, float] = view.region_to_view(0.0, 0.0)  # type: ignore[assignment]
    high: tuple[float, float] = view.region_to_view(1.0, 1.0)  # type: ignore[assignment]
    (left, bottom), (right, top) = low, high
    across, up = right - left, top - bottom
    return Rect(x=-left / across, y=-bottom / up, width=1.0 / across, height=1.0 / up)


def edited(space: bpy.types.Space) -> Option[bpy.types.SpaceImageEditor]:
    """Returns space as image editor, `Nothing` for another editor."""
    match space:
        case bpy.types.SpaceImageEditor():
            return Some(space)
        case _:
            return Nothing


def editor(context: bpy.types.Context) -> BlenderResult[bpy.types.SpaceImageEditor]:
    """Returns context image editor."""
    return Option.of_optional(context.space_data).bind(edited).to_result_with(lambda: (BlenderAbsentFault(owner=repr(context), key=bpy.types.SpaceImageEditor.__name__),))


def overlaid[S](part: Part[S], shown: Setting, refreshes: tuple[Refresh, ...]) -> Overlay[S]:
    """Places part over image editor while setting holds."""
    area = AreaQuery(space="IMAGE_EDITOR", ui_type=Some("IMAGE_EDITOR"), shows=Some(shown))
    return Overlay(drawing=Drawing(area=area, part=part, refreshes=refreshes), space=bpy.types.SpaceImageEditor, scissor_enabled=False)


def metered(attached: Attached[Metering], scene: bpy.types.Scene, region: Region) -> BlenderResult[None]:
    """Writes region into scene meter with its gate on."""
    return attached.read(scene).bind(lambda held: attached.write(scene, msgspec.structs.replace(held, meter=msgspec.structs.replace(held.meter, region=Gated(enabled=True, value=region)))))


def picking[P: Picked](key: Key, title: str, description: str, cursor: "rna_enums.WindowCursorItems", form: type[P], taken: Callable[[Call[P], Region], BlenderResult[Outcome]]) -> Operator[P]:
    """Returns operator converting a pick into a frame region for its edit."""
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
        seed=Some(lambda record, event, _identity: msgspec.structs.replace(record, x=event.mouse_region_x, y=event.mouse_region_y)),
    )


def revealed(context: bpy.types.Context) -> BlenderResult[Option[Located]]:
    """Returns largest editor showing Render Result, opening render view when none does."""
    return rooted(bpy.types.WindowManager, context).bind(
        lambda manager: (
            AreaQuery(space="IMAGE_EDITOR", ui_type=Some("IMAGE_EDITOR"), shows=Some(RESULT))
            .first(manager.windows)
            .map(Some)
            .or_else_with(lambda _: called(bpy.ops.render.view_show, {}, context="INVOKE_DEFAULT").map(lambda _: Nothing))
        )
    )


def showing(image: bpy.types.Image) -> BlenderResult[EditorState]:
    """Returns editor state showing image."""
    return EditorState.of(bpy.types.SpaceImageEditor, {"image": IdName.of(image)})


# --- [COMPOSITION] ----------------------------------------------------------------------

RESULT: Final = image_type(("RENDER_RESULT",))
meter_pick = picking(
    "pick_meter",
    "Pick Meter Region",
    "Meters next frame over picked square",
    "CROSSHAIR",
    Picked,
    lambda call, region: (
        paired(AutoExposure.attachment.bound(call.identity), call.root(bpy.types.Scene))
        .bind(lambda pair: metered(pair[0], pair[1], region))
        .map(lambda _: Done(notes=(("INFO", "Metering picked region"),)))
    ),
)
meter_region = MeterRegion(attachment=AutoExposure.attachment)


def review_window(key: Key, title: str, description: str) -> Operator[Empty]:
    """Returns command showing the render view where the render display preference places it, its sidebar shown, keeping a view the window shows that the host toggle would hide."""

    def rendered(area: bpy.types.Area) -> bool:
        return isinstance(space := area.spaces.active, bpy.types.SpaceImageEditor) and space.image is not None and space.image.type == "RENDER_RESULT"

    def shown(located: Located, identity: Identity) -> BlenderResult[None]:
        screen = located.window.screen
        return Ok(None) if screen.is_temporary or any(map(rendered, screen.areas)) else windowed(NewWindow.RENDER, located, identity)

    def viewing(context: bpy.types.Context) -> BlenderResult[bpy.types.Area]:
        return Option.of_optional(
            next(
                (
                    area
                    for manager in Option.of_optional(context.window_manager).to_list()
                    for window in manager.windows
                    if window.scene == context.scene
                    for area in window.screen.areas
                    if rendered(area)
                ),
                None,
            )
        ).to_result_with(lambda: (BlenderAbsentFault(owner="window_manager.windows", key="RENDER_RESULT"),))

    def opened(call: Call[Empty]) -> BlenderResult[Outcome]:
        return (
            Located
            .current(call.context)
            .bind(lambda located: shown(located, call.identity))
            .bind(lambda _: paired(viewing(call.context), EditorState.of(bpy.types.SpaceImageEditor, {"show_region_ui": True})))
            .bind(lambda pair: pair[1].applied(pair[0]))
            .map(lambda _: Done())
        )

    return Operator(key=key, title=title, description=description, undo=False, form=Empty, run=opened)


def read_pick(viewer: EffectViewer) -> Operator[EntryPick]:
    """Returns pick handing entry input square to entry's read."""
    return picking(
        "pick_read",
        "Pick Read Region",
        "Hands picked entry input square to entry's read",
        "EYEDROPPER",
        EntryPick,
        lambda call, region: viewer.reading.run(Call(identity=call.identity, context=call.context, record=EntryRegion(name=call.record.name, region=region))),
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "HANDLE",
    "RESULT",
    "UNIT",
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
    "review_window",
    "showing",
]
