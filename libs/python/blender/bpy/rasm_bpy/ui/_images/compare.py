"""Before and after compare in the image editor."""

from collections.abc import Callable, Mapping
from enum import auto, Enum
from functools import cache, cached_property, partial
from typing import Annotated, ClassVar, Final, Self

import bpy
from expression import Option, Result, Some
import gpu
from gpu_extras.batch import batch_for_shader
from mathutils import Matrix
import msgspec
import numpy as np

from rasm_bpy.commands.operators import Answer, Call, Done, Empty, Operator
from rasm_bpy.data._extension.identity import Identity, Key
from rasm_bpy.data._extension.registration import Acquisitions, Declaration
from rasm_bpy.data._rna.conditions import Setting
from rasm_bpy.data._rna.descriptors import Value, written
from rasm_bpy.data._rna.paths import RnaPath, root_in, rooted
from rasm_bpy.data._rna.properties import enum_choices
from rasm_bpy.data._rna.snapshots import assigned
from rasm_bpy.settings._attachments import Attached, Attachment
from rasm_bpy.ui._drawing.canvas import Align, Canvas, configured, DECODED, draw_batch, DrawType, FontStyle, programs, QUAD, Rect, Shadow, UNCLIPPED
from rasm_bpy.ui._drawing.handlers import Refresh
from rasm_bpy.ui._elements.gizmos import Gizmo, GizmoGroup, TargetHandler
from rasm_bpy.ui._images.views import framed, HANDLE, image_type, overlaid
from rasm_bpy.ui._layout.members import Member
from rasm_bpy.ui._layout.rows import Layout, Property, Separator
from rasm_bpy.ui._resources.icons import Icon
from rasm_bpy.ui._resources.previews import Texture
from rasm_bpy.ui._resources.theme import display_color, Metrics
from rasm_records import Nothing, Ok, Outcome, paired

# --- [TYPES] ----------------------------------------------------------------------------


class Mode(Enum):
    """Before frame layout: hidden, wiped along either axis, or set beside the shown frame."""

    OFF = auto()
    VERTICAL_WIPE = auto()
    HORIZONTAL_WIPE = auto()
    SIDE_BY_SIDE = auto()


type _Wipe = tuple[Compare, Rect, tuple[float, float]]

# --- [CONSTANTS] ------------------------------------------------------------------------

_MODES: Final = enum_choices(Mode)
_CHANNELS: Final = ("RED", "GREEN", "BLUE", "ALPHA")

# --- [MODELS] ---------------------------------------------------------------------------


class Compare(bpy.types.PropertyGroup):
    """Screen compare: the before frame's layout, split, and side."""

    mode: Annotated[str, bpy.props.EnumProperty(name="Compare", description="Before frame layout over the shown image", items=_MODES)]
    split: Annotated[float, bpy.props.FloatProperty(name="Split", description="Wipe position across the frame", subtype="FACTOR", min=0.0, max=1.0, default=0.5)]
    swapped: Annotated[bool, bpy.props.BoolProperty(name="Swap", description="Draw the before frame right of or below the split")]


class Showing(bpy.types.PropertyGroup):
    """Layout a show command toggles."""

    mode: Annotated[str, bpy.props.EnumProperty(name="Compare", description="Layout to show, hiding the before frame while it shows in this layout", items=_MODES)]


class _CompareLayout(msgspec.Struct, frozen=True, kw_only=True):
    """Compare layout in region pixels: the before plate, the clip it draws in, the split rule as the rectangle its diagonal spans, and the captions."""

    plate: Rect
    clip: Rect
    rule: Option[Rect]
    captions: tuple[tuple[str, float, float, Align], ...]

    @classmethod
    def of(cls, compare: Compare, frame: Rect, bounds: Rect, size: tuple[float, float], names: tuple[str, str], metrics: Metrics, text: float) -> Self:
        """Lays out the plate inside visible bounds with the before and shown captions on either side of the split."""
        (width, height), right, top = size, bounds.x + bounds.width, bounds.y + bounds.height
        gap, inset = metrics.line(1.0), metrics.margin
        plate = Rect(x=frame.x + (frame.width - width) / 2, y=frame.y + (frame.height - height) / 2, width=width, height=height)
        low, high, left = max(frame.y, bounds.y), min(frame.y + frame.height, top), max(frame.x, bounds.x)
        first, last = reversed(names) if compare.swapped else names
        match Mode[compare.mode]:
            case Mode.SIDE_BY_SIDE:
                beside = msgspec.structs.replace(plate, x=frame.x + frame.width + gap if compare.swapped else frame.x - gap - width)
                outer = (beside.x + inset, Align.START) if compare.swapped else (beside.x + width - inset, Align.END)
                inner = (frame.x + frame.width - inset, Align.END) if compare.swapped else (frame.x + inset, Align.START)
                captions = ((names[0], outer[0], high - inset - text, outer[1]), (names[1], inner[0], high - inset - text, inner[1]))
                return cls(plate=beside, clip=beside, rule=Nothing, captions=captions)
            case Mode.VERTICAL_WIPE:
                at = frame.x + compare.split * frame.width
                clip = msgspec.structs.replace(bounds, x=at, width=max(right - at, 0.0)) if compare.swapped else msgspec.structs.replace(bounds, width=max(at - bounds.x, 0.0))
                captions = ((first, at - inset, high - inset - text, Align.END), (last, at + inset, high - inset - text, Align.START))
                return cls(plate=plate, clip=clip, rule=Some(Rect(x=at, y=low, width=0.0, height=high - low)), captions=captions)
            case Mode.HORIZONTAL_WIPE:
                at = frame.y + (1.0 - compare.split) * frame.height
                clip = msgspec.structs.replace(bounds, height=max(at - bounds.y, 0.0)) if compare.swapped else msgspec.structs.replace(bounds, y=at, height=max(top - at, 0.0))
                captions = ((first, left + inset, at + inset, Align.START), (last, left + inset, at - inset - text, Align.START))
                return cls(plate=plate, clip=clip, rule=Some(Rect(x=left, y=at, width=min(frame.x + frame.width, right) - left, height=0.0)), captions=captions)
            case Mode.OFF:
                return cls(plate=plate, clip=bounds, rule=Nothing, captions=())


class _CompareOverlay(msgspec.Struct, frozen=True, kw_only=True):
    """Part drawing the before frame in the editor's display channels under the screen's compare, with its split rule and captions."""

    draw_type: ClassVar[DrawType] = "POST_PIXEL"
    read: Callable[[Metrics], Outcome[Option[Texture[tuple[str, str]]]]]
    compare: Callable[[bpy.types.Context], Option[Compare]]

    def drawn(self, state: Texture[tuple[str, str]], canvas: Canvas) -> None:
        """Draws the plate in the editor's display channels, then the rule and captions over it."""
        (texture, names), metrics = state, canvas.metrics
        style, tint = metrics.preferences.ui_styles[0].widget, display_color(metrics.preferences.themes[0].image_editor.metadatatext)
        strokes = (*((shadow.color, shadow.offset) for shadow in Shadow.of(style).to_list()), (tint, (0.0, 0.0)))

        def pictured(plate: Rect, channels: str) -> None:
            """Draws the frame over the plate as the editor shows its display channels: color alone, color over the transparency checker, or one channel in gray."""
            match channels:
                case "COLOR":
                    Canvas.picture(texture, plate, blend="NONE")
                case "COLOR_ALPHA":
                    canvas.checkered(plate, metrics.checker())
                    Canvas.picture(texture, plate, blend="ALPHA_PREMULT")
                case channel if channel in _CHANNELS:
                    shader, quad = _channeled()
                    with Canvas.mapped(plate):
                        draw_batch(quad, configured((shader, UNCLIPPED), partial(shader.uniform_sampler, "image", texture), partial(shader.uniform_int, "shown", _CHANNELS.index(channel))))
                case _:
                    pass

        def ruled(rule: Rect) -> None:
            """Draws the rule once per stroke color at its offset, the shadow under the line."""
            lines = canvas.lines(1.0)
            for shade, (dx, dy) in strokes:
                with Canvas.mapped(msgspec.structs.replace(rule, x=rule.x + dx, y=rule.y + dy)):
                    draw_batch(_diagonal(), configured(lines, partial(lines[0].uniform_float, "color", shade)))

        match canvas.space:
            case bpy.types.SpaceImageEditor(display_channels=channels) as space:
                across, up = np.asarray(space.zoom)
                for compare in self.compare(bpy.context).to_list():
                    laid = _CompareLayout.of(compare, framed(canvas.region), canvas.extent, (texture.width * across, texture.height * up), names, metrics, metrics.font(style))
                    texture.filter_mode(min(across, up) < 1.0)
                    with canvas.clipped(laid.clip):
                        pictured(laid.plate, channels)
                    for rule in laid.rule.to_list():
                        ruled(rule)
                    for caption, x, y, align in laid.captions:
                        canvas.label(caption, (x, y), tint, FontStyle(size=style, align=align))
            case _:
                pass


# --- [SERVICES] -------------------------------------------------------------------------


class CompareView(msgspec.Struct, frozen=True, kw_only=True, dict=True):
    """Before frame over images of `kinds` under each screen's compare, with its split handle, header toggles, and show, mirror, and swap commands."""

    # --- [STATE]

    key: Key
    icon: Icon
    read: Callable[[Identity, Metrics], Outcome[Option[Texture[tuple[str, str]]]]]
    kinds: tuple[str, ...] = ("RENDER_RESULT",)
    refreshes: Callable[[Identity], tuple[Refresh, ...] | Outcome[tuple[Refresh, ...]]] = lambda _identity: ()

    @cached_property
    def state(self) -> Attachment[Compare]:
        """Screen compare."""
        return Attachment(key=self.key, form=Compare, owner=bpy.types.Screen)

    @property
    def shown(self) -> Setting:
        """Condition that the editor shows an image of `kinds`."""
        return image_type(self.kinds)

    @property
    def prerequisites(self) -> tuple[Declaration, ...]:
        """Screen compare the handle, overlay, and header bind."""
        return (self.state,)

    # --- [COMMANDS]

    def screened[T: bpy.types.PropertyGroup](self, call: Call[T], change: Callable[[Compare], Mapping[RnaPath, Value]]) -> Outcome[Answer]:
        """Writes one change of the calling screen's compare."""
        return rooted(bpy.types.Screen, call.context).bind(lambda screen: self.state.read(call.identity, screen)).bind(lambda held: assigned(held, change(held))).map(lambda _: Done())

    def show_compare(self, call: Call[Showing]) -> Outcome[Answer]:
        """Show the before frame in a layout, hiding it while it shows in that layout."""
        wanted = call.properties.mode
        return self.screened(call, lambda held: {("mode",): Mode.OFF.name if held.mode == wanted else wanted})

    def mirror_split(self, call: Call[Empty]) -> Outcome[Answer]:
        """Mirror the wipe across the frame."""
        return self.screened(call, lambda held: {("split",): 1.0 - held.split})

    def swap_sides(self, call: Call[Empty]) -> Outcome[Answer]:
        """Draw the before frame on the other side of the split."""
        return self.screened(call, lambda held: {("swapped",): not held.swapped})

    @cached_property
    def showing(self) -> Operator[Showing]:
        """Command toggling a layout, keyed `show_<key>`."""
        return Operator.of(Showing, condition=Some(self.shown), options=frozenset(), named=Some(f"show_{self.key}"))(self.show_compare)

    @cached_property
    def mirroring(self) -> Operator[Empty]:
        """Command mirroring the split, keyed `mirror_<key>`."""
        return Operator.of(Empty, condition=Some(self.shown), options=frozenset(), icon=Some("MOD_MIRROR"), named=Some(f"mirror_{self.key}"))(self.mirror_split)

    @cached_property
    def swapping(self) -> Operator[Empty]:
        """Command swapping the sides, keyed `swap_<key>`."""
        return Operator.of(Empty, condition=Some(self.shown), options=frozenset(), icon=Some("ARROW_LEFTRIGHT"), named=Some(f"swap_{self.key}"))(self.swap_sides)

    # --- [HANDLES]

    @staticmethod
    def compared(attached: Attached[Compare], context: bpy.types.Context) -> Option[Compare]:
        """Returns the context screen's compare while it shows the before frame."""
        return root_in(bpy.types.Screen, context).map(attached.struct).filter(lambda held: Mode[held.mode] is not Mode.OFF)

    def wiped(self, attached: Attached[Compare], context: bpy.types.Context) -> Option[_Wipe]:
        """Returns a shown wipe with the frame and the visible frame middle from the frame origin, Nothing beside or hidden."""

        def measured(held: Compare, region: bpy.types.Region, frame: Rect) -> tuple[Compare, Rect, tuple[float, float]]:
            return held, frame, ((max(frame.x, 0.0) + min(frame.x + frame.width, region.width)) / 2 - frame.x, (max(frame.y, 0.0) + min(frame.y + frame.height, region.height)) / 2 - frame.y)

        return (
            self
            .compared(attached, context)
            .filter(lambda held: Mode[held.mode] is not Mode.SIDE_BY_SIDE)
            .bind(lambda held: root_in(bpy.types.Region, context).map(lambda region: measured(held, region, framed(region))))
        )

    @staticmethod
    def handle(state: _Wipe) -> Outcome[Option[Matrix]]:
        """Places the split handle at the frame origin."""
        _, frame, _ = state
        return Ok(Some(Matrix.Translation((frame.x, frame.y, 0.0))))

    @staticmethod
    def offset(state: _Wipe) -> tuple[float, ...]:
        """Returns the split offset at the visible frame middle."""
        held, frame, middle = state
        return (held.split * frame.width, middle[1], 0.0) if Mode[held.mode] is Mode.VERTICAL_WIPE else (middle[0], (1.0 - held.split) * frame.height, 0.0)

    def moved(self, attached: Attached[Compare], context: bpy.types.Context, values: tuple[float, ...]) -> Outcome[None]:
        """Writes the split the handle's offset along the wipe axis gives, the property bounds keeping it on the frame."""

        def split(held: Compare, frame: Rect, _middle: tuple[float, float]) -> Outcome[None]:
            return written(held, ("split",), values[0] / frame.width if Mode[held.mode] is Mode.VERTICAL_WIPE else 1.0 - values[1] / frame.height).map(lambda _: None)

        return self.wiped(attached, context).starmap(split).default_value(Ok(None))

    # --- [SECTIONS]

    @property
    def header(self) -> tuple[Layout]:
        """Layout and swap toggles, drawn while the editor shows an image of `kinds`."""
        return (
            Layout(
                rows=(
                    Property(target=Member(root=self.state, path=("mode",)), icon=Some(self.icon)),
                    Separator(keywords={"type": "SPACE"}),
                    Property(target=Member(root=self.state, path=("swapped",)), keywords={"icon_only": True}, icon=Some("ARROW_LEFTRIGHT")),
                ),
                visible=Some(self.shown),
            ),
        )

    # --- [REGISTRATION]

    def acquisitions(self, identity: Identity) -> Outcome[Acquisitions]:
        """Returns the compare, its commands, the split handle, the before frame overlay, and its shader release."""

        def rows(attached: Attached[Compare], refreshes: tuple[Refresh, ...]) -> Acquisitions:
            bind = TargetHandler[tuple[float, ...], _Wipe](get=self.offset, set=partial(self.moved, attached))
            handle = Gizmo(kind="GIZMO_GT_move_3d", place=self.handle, bind=bind, properties={"scale_basis": HANDLE, "draw_style": "RING_2D"})
            return (
                self.showing,
                self.mirroring,
                self.swapping,
                GizmoGroup(key=self.key, space="IMAGE_EDITOR", gizmos=(handle,), read=lambda context: Ok(self.wiped(attached, context)), condition=Some(self.shown)),
                overlaid(_CompareOverlay(read=partial(self.read, identity), compare=partial(self.compared, attached)), self.shown, refreshes),
                _PROGRAMS,
            )

        refreshes = self.refreshes(identity)
        return paired(self.state.bound(identity), refreshes if isinstance(refreshes, Result) else Ok(refreshes)).map(lambda found: rows(*found))


# --- [OPERATIONS] -----------------------------------------------------------------------


@cache
def _channeled() -> tuple[gpu.types.GPUShader, gpu.types.GPUBatch]:
    """Builds the one-channel gray shader, the swizzle no built-in shader draws, and its unit quad."""
    interface = gpu.types.GPUStageInterfaceInfo("rasm_channeled")
    interface.smooth("VEC2", "uv")
    info = gpu.types.GPUShaderCreateInfo()
    info.vertex_in(0, "VEC2", "pos")
    info.vertex_out(interface)
    info.sampler(0, "FLOAT_2D", "image")
    info.push_constant("MAT4", "ModelViewProjectionMatrix")
    info.push_constant("INT", "shown")
    info.push_constant("BOOL", "srgbTarget")
    info.fragment_out(0, "VEC4", "fragColor")
    info.vertex_source("void main() { uv = pos; gl_Position = ModelViewProjectionMatrix * vec4(pos, 0.0, 1.0); }")
    info.fragment_source(DECODED + "void main() { fragColor = vec4(targeted(vec3(texture(image, uv)[shown])), 1.0); }")
    shader = gpu.shader.create_from_info(info)
    return shader, batch_for_shader(shader, "TRI_STRIP", {"pos": QUAD})


@cache
def _diagonal() -> gpu.types.GPUBatch:
    """Batches the unit diagonal a rule rectangle maps onto its line."""
    return batch_for_shader(gpu.shader.from_builtin("POLYLINE_UNIFORM_COLOR"), "LINES", {"pos": (QUAD[0], QUAD[-1])})


# --- [COMPOSITION] ----------------------------------------------------------------------

_PROGRAMS: Final = programs(_channeled.cache_clear, _diagonal.cache_clear)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Compare", "CompareView", "Mode", "Showing"]
