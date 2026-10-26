"""Image editor frame compare, charts, clip masks, regions, and picks."""

from collections.abc import Callable, Iterable
from enum import auto, Enum, IntEnum
import functools
import math
from typing import Annotated, ClassVar, Final, get_args, Self, TYPE_CHECKING

import bpy
from expression import Nothing, Ok, Option, Some
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
from blender.bpy.data.rna.conditions import AllOf, check, Setting
from blender.bpy.data.rna.descriptors import Descriptor, root_in, rooted
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions, Registrable
from blender.bpy.extension.scoped import Scoped
from blender.bpy.faults import BlenderAbsentFault, BlenderResult
from blender.bpy.render.color.exposure import AutoExposure, Metering
from blender.bpy.render.color.spaces import config_of
from blender.bpy.render.effects.formation import formation, viewed
from blender.bpy.render.outputs.exports import combined, displayed, image_type, Source
from blender.bpy.render.outputs.results import ResultRead
from blender.bpy.settings.attachments import Attached, Attachment
from blender.bpy.settings.properties import Definition
from blender.bpy.ui.drawing.canvas import Align, Canvas, DECODED, QUAD, Rect, released, Shadow
from blender.bpy.ui.drawing.charts import ChartFrame, gamuts, histogram, Plotted
from blender.bpy.ui.drawing.handlers import Drawing, Overlay, Part, Refresh, Signal, Watch
from blender.bpy.ui.layout.rows import Caption, Divider, Flow, Member, Property, Row, Rows
from blender.bpy.ui.layout.sections import Single
from blender.bpy.ui.resources.icons import Glyph
from blender.bpy.ui.resources.textures import uploaded
from blender.bpy.ui.resources.theme import Color, color, Look, Metrics, Shipped
from blender.bpy.ui.screens.areas import AreaQuery, Arrangement, Located, SETTLING
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
UNIT: Final[MatrixValues] = msgspec.convert(np.asarray(Matrix.Identity(4)).ravel().tolist(), type=MatrixValues)
CATEGORY: Final = Descriptor.declared(bpy.types.Region, "active_panel_category", bpy.types.EnumProperty)
SIDEBAR: Final = EditorState(values=((Descriptor.declared(bpy.types.SpaceImageEditor, "show_region_ui", bpy.types.BoolProperty), True),))


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
    name: str


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

    caption: ClassVar[Look] = Shipped.read().look("overlay-caption")
    light: ClassVar[Color] = caption.text
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
        text = canvas.metrics.points()
        shadow = Shadow.cast(Wipe.caption, canvas.metrics)
        shader, quad = Wipe.channeled()
        comparisons = self.compare.map(lambda read: self.identity.reports.reported(read)(context).bind(lambda found: found).map(Some)).default_value(Some(Nothing))
        for space, compare, shown, region in (
            (held, compare, channel, region)
            for held in root_in(bpy.types.SpaceImageEditor, context).to_list()
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
            for (start, end), (ink, (dx, dy)) in ((rule, pair) for rule in sides.rule.to_list() for pair in (*((shade.color, shade.offset) for shade in shadow.to_list()), (Wipe.light, (0.0, 0.0)))):
                lines.uniform_float("color", ink)
                batch_for_shader(lines, "LINES", {"pos": ((start[0] + dx, start[1] + dy), (end[0] + dx, end[1] + dy))}).draw(lines)
            for caption, x, y, align in sides.captions:
                canvas.label(caption, (x, y), Wipe.light, text, align, shadow)

    @staticmethod
    @functools.cache
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
        return shader, batch_for_shader(shader, "TRI_STRIP", {"pos": QUAD})


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
        for space, texture, region in ((space, texture, region) for space in root_in(bpy.types.SpaceImageEditor, bpy.context).to_list() for texture in state for region in canvas.region.to_list()):
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
            (enabled, value), (mode, _, swapped) = Gated.__struct_fields__, Compare.__struct_fields__
            return self.state.bound(identity).map(
                lambda attached: (
                    Property(target=Member(root=bpy.types.Screen, path=f"{attached.name}.{enabled}"), icon=Some(self.glyph), icon_only=True),
                    Divider(type="SPACE"),
                    Property(target=Member(root=bpy.types.Screen, path=f"{attached.name}.{value}.{mode}"), expand=True),
                    Divider(type="SPACE"),
                    Property(target=Member(root=bpy.types.Screen, path=f"{attached.name}.{value}.{swapped}"), icon=Some("ARROW_LEFTRIGHT"), icon_only=True),
                )
            )

        return (Rows(rows=rows, condition=Some(image_type(self.kinds)), hidden=True),)

    def acquisitions(self, identity: Identity) -> BlenderResult[Acquisitions]:
        """Returns split handle and overlay declarations."""
        shown = image_type(self.kinds)

        def ruled(region: bpy.types.Region, compare: Compare) -> Option[tuple[float, float]]:
            frame = framed(region)
            middle = ((max(frame.x, 0.0) + min(frame.x + frame.width, region.width)) / 2, (max(frame.y, 0.0) + min(frame.y + frame.height, region.height)) / 2)
            match compare.mode:
                case Mode.VERTICAL:
                    return Some((frame.x + compare.split * frame.width, middle[1]))
                case Mode.HORIZONTAL:
                    return Some((middle[0], frame.y + (1.0 - compare.split) * frame.height))
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
                return self.state.revised(identity, screen, lambda gated: Ok(msgspec.structs.replace(gated, value=msgspec.structs.replace(gated.value, split=split)))).map(lambda _: None)

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
            handle = Move(place=place, bind=command, scale_basis=HANDLE, draw_style="RING_2D")
            return (
                command,
                Gizmos(key=self.key, title="Compare", space="IMAGE_EDITOR", parts=(handle,), condition=Some(shown)),
                overlaid(Wipe(identity=identity, source=lambda metrics: self.read(identity, metrics), compare=Some(compared)), shown, refreshes),
                WIPES,
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
            held = Region.bounding(access.quad.samples)
            across, up = (held.right - held.left) * frame.width, (held.top - held.bottom) * frame.height
            return (across, 0.0, 0.0, 0.0, 0.0, up, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0, (held.left + held.right) / 2.0 * frame.width, (held.bottom + held.top) / 2.0 * frame.height, 0.0, 1.0)

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

                return access.write(Quad.of_region(Region.bounding(np.stack(aligned.bounds.map(bounded).default_value((center - half, center + half))))))

            return self.held(context).bind(lambda found: found.to_result_with(lambda: (BlenderAbsentFault(owner=repr(context.area), key=self.key),))).bind(lambda pair: written(*pair))

        return Cage(
            place=place,
            bind=Handler(get=get, set=wrote),
            dimensions=(1.0, 1.0),
            transform=frozenset({"TRANSLATE", "SCALE_UNIFORM"} if aligned.locked else {"TRANSLATE", "SCALE"}),
            draw_style="BOX",
            draw_options=frozenset({"CORNER_HANDLES"}),
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
            return command, Move(place=place, bind=command, scale_basis=HANDLE, draw_style="CROSS_2D" if isinstance(part, int) else "RING_2D")

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

        def access(context: bpy.types.Context) -> BlenderResult[Option[Access]]:
            def read(scene: bpy.types.Scene) -> BlenderResult[Option[Access]]:
                def write(quad: Quad) -> BlenderResult[None]:
                    return metered(self.attachment, identity, scene, Region.bounding(quad.samples))

                return self.attachment.read(identity, scene).map(
                    lambda held: Some(held.meter.region).filter(lambda gated: gated.enabled).map(lambda gated: Access(quad=Quad.of_region(gated.value), write=write))
                )

            return root_in(bpy.types.Scene, context).map(read).default_value(Ok(Nothing))

        return Ok((FrameRegion(key="meter_region", title="Meter Region", access=access, shape=Aligned(bounds=Some(Region()))),))


class Frames(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Last render's scene-linear chart frame, display plane, and written integer depth."""

    linear: ChartFrame
    formed: npt.NDArray[np.float32]
    depth: Option[IntegerDepth]


class FrameCharts(msgspec.Struct, frozen=True, kw_only=True):
    """Row drawing last render charts and clip ends over Render Result."""

    key: ClassVar[Key] = "charts"
    under: ClassVar[Color] = Shipped.read().look("clip-under").fill
    over: ClassVar[Color] = Shipped.read().look("clip-over").fill
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
                        rows=tuple(
                            Property(target=Member(root=bpy.types.Screen, path=f"{attached.name}.{field}"), icon=Some(glyph), icon_only=True)
                            for field, glyph in zip(ClipEnds.__struct_fields__, (self.low, self.high))
                        ),
                        flow=Flow.GRID,
                    ),
                )
            )

        return (Rows(rows=rows, condition=Some(RESULT), hidden=True),)

    def section(self) -> Single:
        """Chart switches section, inactive for another image."""

        def rows(identity: Identity) -> BlenderResult[tuple[Row, ...]]:
            return self.switches.bound(identity).map(
                lambda attached: tuple(Property(target=Member(root=bpy.types.Screen, path=f"{attached.name}.{row.name}")) for row in msgspec.structs.fields(Charted))
            )

        return Single(key=self.key, title="Charts", rows=(Rows(rows=rows),), condition=Some(RESULT))

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
                return paired(combined(path, Source.RENDER, Some(view_layer.name)), formation.current(identity, scene).map(lambda found: found.default_with(lambda: viewed(scene))), config_of()).bind(
                    lambda found: displayed(formation, identity, scene, found[0][1], PyOpenColorIO.ROLE_SCENE_LINEAR, found[2]).map(
                        lambda file: Frames(linear=ChartFrame(pixels=found[0][1], view=found[1]), formed=file.pixels, depth=depth)
                    )
                )

            return rooted(bpy.types.ViewLayer, bpy.context).bind(read).bind(kept)

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
                overlaid(Plotted(read=functools.partial(histogram, lambda: charted(lambda held: held.outline))), RESULT, refreshes),
                overlaid(Plotted(read=functools.partial(gamuts, lambda: charted(lambda held: held.gamuts))), RESULT, refreshes),
                FrameMask(read=lambda _: tints(), refreshes=refreshes),
            )

        return paired(self.switches.bound(identity), self.ends.bound(identity)).map(lambda pair: rows(*pair))


class ReviewWindow(msgspec.Struct, frozen=True, kw_only=True):
    """Render view where the render display preference places it, its sidebar opened, then turned to a tab on the next host pass once the region has drawn its tabs."""

    key: Key
    title: str
    description: str
    tab: str

    @property
    def command(self) -> Operator[Empty]:
        """Command placements and registration share, equal across reads."""
        return Operator(key=self.key, title=self.title, description=self.description, undo=False, form=Empty, run=self.opened)

    def opened(self, call: Call[Empty]) -> BlenderResult[Outcome]:
        """Opens the render view unless the window is temporary or shows one, keeping a view the host toggle would hide, then settles its sidebar."""
        tabbed = EditorState(region=Some("UI"), values=((CATEGORY, self.tab),))
        return (
            Located
            .current(call.context)
            .bind(lambda located: Ok(None) if located.window.screen.is_temporary or RENDERED.located((located.window,)) else located.run(bpy.ops.render.view_show, {}, context="INVOKE_DEFAULT"))
            .bind(lambda _: viewing(call.context))
            .bind(lambda located: SETTLING.of(call.identity).settled(Arrangement(at=located, steps=(SIDEBAR, tabbed))))
            .map(lambda _: Done())
        )

    def acquisitions(self, _identity: Identity) -> BlenderResult[Acquisitions]:
        """Returns the command and the settling driver its sidebar tab waits on."""
        return Ok((self.command, SETTLING))


# --- [OPERATIONS] -----------------------------------------------------------------------


def framed(region: bpy.types.Region) -> Rect:
    """Returns frame rectangle in region pixels."""
    view = region.view2d
    low: tuple[float, float] = view.region_to_view(0.0, 0.0)  # type: ignore[assignment]
    high: tuple[float, float] = view.region_to_view(1.0, 1.0)  # type: ignore[assignment]
    (left, bottom), (right, top) = low, high
    across, up = right - left, top - bottom
    return Rect(x=-left / across, y=-bottom / up, width=1.0 / across, height=1.0 / up)


def overlaid[S](part: Part[S], shown: Setting, refreshes: tuple[Refresh, ...]) -> Overlay[S]:
    """Places part over image editor while setting holds."""
    area = AreaQuery(space="IMAGE_EDITOR", ui_type=Some("IMAGE_EDITOR"), shows=Some(shown))
    return Overlay(drawing=Drawing(area=area, part=part, refreshes=refreshes), space=bpy.types.SpaceImageEditor)


def metered(attachment: Attachment[Metering], identity: Identity, scene: bpy.types.Scene, region: Region) -> BlenderResult[None]:
    """Writes region into scene meter with its gate on."""
    return attachment.revised(identity, scene, lambda held: Ok(msgspec.structs.replace(held, meter=msgspec.structs.replace(held.meter, region=Gated(enabled=True, value=region))))).map(lambda _: None)


def picking[P: Picked](key: Key, title: str, description: str, cursor: "rna_enums.WindowCursorItems", form: type[P], taken: Callable[[Call[P], Region], BlenderResult[Outcome]]) -> Operator[P]:
    """Returns operator converting a pick into a frame region for its edit."""

    def ran(call: Call[P]) -> BlenderResult[Outcome]:
        context, record = call.context, call.record

        def regioned(space: bpy.types.SpaceImageEditor, region: bpy.types.Region) -> BlenderResult[Region]:
            frame, zoom = framed(region), space.zoom
            width, height = round(frame.width / zoom[0]), round(frame.height / zoom[1])  # type: ignore[index]
            column, row = math.floor((record.x - frame.x) / zoom[0]), math.floor((record.y - frame.y) / zoom[1])  # type: ignore[index]
            reach = np.array(((-record.radius, -record.radius), (record.radius + 1, record.radius + 1)))
            return (
                Some((column, row))
                .filter(lambda pixel: 0 <= pixel[0] < width and 0 <= pixel[1] < height)
                .map(lambda pixel: Region.bounding(np.clip((np.array(pixel) + reach) / (width, height), 0.0, 1.0)))
                .to_result_with(lambda: (BlenderAbsentFault(owner=repr(space.image), key=f"pixel {column}, {row}"),))
            )

        return (
            check(AllOf(conditions=(RESULT, WINDOW)), context)
            .bind(lambda _: paired(rooted(bpy.types.SpaceImageEditor, context), call.root(bpy.types.Region)))
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


def viewing(context: bpy.types.Context) -> BlenderResult[Located]:
    """Returns largest editor showing Render Result in a window of the context scene."""
    return rooted(bpy.types.WindowManager, context).bind(lambda manager: RENDERED.first(window for window in manager.windows if window.scene == context.scene))


def revealed(context: bpy.types.Context) -> BlenderResult[Option[Located]]:
    """Returns the editor `viewing` finds, `Nothing` after opening the render view when none shows Render Result."""
    return viewing(context).map(Some).or_else_with(lambda _: called(bpy.ops.render.view_show, {}, context="INVOKE_DEFAULT").map(lambda _: Nothing))


# --- [COMPOSITION] ----------------------------------------------------------------------

RESULT: Final = image_type(("RENDER_RESULT",))
WIPES: Final = Scoped[None](made=lambda _identity: released(Wipe.channeled.cache_clear))
RENDERED: Final = AreaQuery(space="IMAGE_EDITOR", ui_type=Some("IMAGE_EDITOR"), shows=Some(RESULT))
WINDOW: Final = Setting(descriptor=Descriptor.declared(bpy.types.Context, "region.type", bpy.types.EnumProperty), accepted=("WINDOW",))
meter_pick = picking(
    "pick_meter",
    "Pick Meter Region",
    "Meters next frame over picked square",
    "CROSSHAIR",
    Picked,
    lambda call, region: call.root(bpy.types.Scene).bind(lambda scene: metered(AutoExposure.attachment, call.identity, scene, region)).map(lambda _: Done(notes=(("INFO", "Metering picked region"),))),
)
meter_region = MeterRegion(attachment=AutoExposure.attachment)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "CATEGORY",
    "HANDLE",
    "RENDERED",
    "RESULT",
    "SIDEBAR",
    "UNIT",
    "WINDOW",
    "WIPES",
    "Access",
    "Aligned",
    "Before",
    "Charted",
    "ClipEnds",
    "Compare",
    "CompareView",
    "FrameCharts",
    "FrameMask",
    "FrameRegion",
    "Free",
    "MeterRegion",
    "Mode",
    "Picked",
    "ReviewWindow",
    "Shape",
    "Shown",
    "Tint",
    "framed",
    "meter_pick",
    "meter_region",
    "metered",
    "overlaid",
    "picking",
    "revealed",
    "viewing",
]
