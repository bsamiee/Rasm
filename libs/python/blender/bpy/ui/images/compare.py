"""Before/after compare in the image editor: a before frame wiped across or set beside the shown image in the editor's display channels, its split handle, header toggles, and the commands a consumer's keys bind."""

from collections.abc import Callable, Mapping
from enum import auto, Enum
from functools import cache, cached_property
from typing import ClassVar, Final, Self

import bpy
from expression import Nothing, Ok, Option, Some
import gpu
from mathutils import Matrix
import msgspec
import numpy as np
import numpy.typing as npt

from blender.bpy.commands.operators import Answer, Call, Done, Empty, Operator
from blender.bpy.data.rna.conditions import Setting
from blender.bpy.data.rna.descriptors import Value, written
from blender.bpy.data.rna.mirrors import enum_choices
from blender.bpy.data.rna.paths import RnaPath, root_in, rooted
from blender.bpy.data.rna.snapshots import assigned
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions, Declaration
from blender.bpy.settings.attachments import Attached, Attachment
from blender.bpy.ui.drawing.canvas import Align, Canvas, DECODED, DrawType, programs, QUAD, Rect, Shadow
from blender.bpy.ui.drawing.handlers import Refresh
from blender.bpy.ui.images.views import framed, HANDLE, image_type, overlaid
from blender.bpy.ui.layout.members import Member
from blender.bpy.ui.layout.rows import Layout, Property, Separator
from blender.bpy.ui.resources.icons import Glyph
from blender.bpy.ui.resources.previews import uploaded
from blender.bpy.ui.resources.theme import color, Metrics
from blender.bpy.ui.surfaces.gizmos import GizmoPart, Gizmos, TargetHandler
from blender.records.faults import Outcome

# --- [TYPES] ----------------------------------------------------------------------------


class Mode(Enum):
    """Before frame layout: hidden, wiped along either axis, or set beside the shown frame."""

    OFF = auto()
    VERTICAL_WIPE = auto()
    HORIZONTAL_WIPE = auto()
    SIDE_BY_SIDE = auto()


# --- [CONSTANTS] ------------------------------------------------------------------------

_MODES: Final = enum_choices(Mode)
_CHANNELS: Final = ("RED", "GREEN", "BLUE", "ALPHA")
_DIAGONAL: Final = np.array(((0.0, 0.0), (1.0, 1.0)), np.float32)
_DIAGONAL.setflags(write=False)

# --- [MODELS] ---------------------------------------------------------------------------


class Compare(bpy.types.PropertyGroup):
    """Screen compare: the before frame's layout, split, and side."""

    mode: bpy.props.EnumProperty(name="Compare", description="Before frame layout over the shown image", items=_MODES)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    split: bpy.props.FloatProperty(name="Split", description="Wipe position across the frame", subtype="FACTOR", min=0.0, max=1.0, default=0.5)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    swapped: bpy.props.BoolProperty(name="Swap", description="Draw the before frame right of or below the split")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class Showing(bpy.types.PropertyGroup):
    """Layout a show command toggles."""

    mode: bpy.props.EnumProperty(name="Compare", description="Layout to show, hiding the before frame while it shows in this layout", items=_MODES)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class Before(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Before frame: its top-first associated RGBA plane in display colors, its caption, and the caption of the frame it draws over."""

    plane: npt.NDArray[np.float32]
    name: str
    shown: str


class Sides(msgspec.Struct, frozen=True, kw_only=True):
    """Compare layout in region pixels: the before plate, the clip it draws in, the split rule as the rectangle its diagonal spans, and the captions."""

    plate: Rect
    clip: Rect
    rule: Option[Rect]
    captions: tuple[tuple[str, float, float, Align], ...]

    @classmethod
    def of(cls, compare: Compare, frame: Rect, bounds: Rect, size: tuple[float, float], before: Before, metrics: Metrics, text: float) -> Self:
        """Lays out the plate over a frame inside visible bounds, captioned by the before and shown names on either side of the split."""
        (width, height), right, top = size, bounds.x + bounds.width, bounds.y + bounds.height
        gap, inset = metrics.line(1.0), metrics.margin
        plate = Rect(x=frame.x + (frame.width - width) / 2, y=frame.y + (frame.height - height) / 2, width=width, height=height)
        low, high, left = max(frame.y, bounds.y), min(frame.y + frame.height, top), max(frame.x, bounds.x)
        first, last = (before.shown, before.name) if compare.swapped else (before.name, before.shown)
        match Mode[compare.mode]:
            case Mode.SIDE_BY_SIDE:
                beside = msgspec.structs.replace(plate, x=frame.x + frame.width + gap if compare.swapped else frame.x - gap - width)
                outer = (beside.x + inset, Align.START) if compare.swapped else (beside.x + width - inset, Align.END)
                inner = (frame.x + frame.width - inset, Align.END) if compare.swapped else (frame.x + inset, Align.START)
                captions = ((before.name, outer[0], high - inset - text, outer[1]), (before.shown, inner[0], high - inset - text, inner[1]))
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


class Wipe(msgspec.Struct, frozen=True, kw_only=True):
    """Part drawing the before frame in the editor's display channels under the screen's compare, its rule and captions in the theme's image metadata text color under the widget font style's shadow."""

    draw_type: ClassVar[DrawType] = "POST_PIXEL"
    source: Callable[[Metrics], Outcome[Option[Before]]]
    compare: Callable[[bpy.types.Context], Option[Compare]]

    def read(self, metrics: Metrics) -> Outcome[Option[tuple[gpu.types.GPUTexture, Before]]]:
        """Uploads the before frame plane."""
        return self.source(metrics).bind(lambda found: found.map(lambda before: uploaded(before.plane).map(lambda texture: Some((texture, before)))).default_value(Ok(Nothing)))

    def drawn(self, canvas: Canvas, state: tuple[gpu.types.GPUTexture, Before]) -> None:
        """Draws the plate in the editor's display channels, then the rule and captions over it."""
        (texture, before), metrics = state, canvas.metrics
        style, tint = metrics.preferences.ui_styles[0].widget, color(metrics.preferences.themes[0].image_editor.metadatatext)
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
                    shader.bind()
                    shader.uniform_sampler("image", texture)
                    shader.uniform_int("shown", _CHANNELS.index(channel))
                    with Canvas.mapped(plate):
                        quad.draw(shader)
                case _:
                    pass

        def ruled(rule: Rect) -> None:
            """Draws the rule once per stroke color at its offset, the shadow under the line."""
            with canvas.lines(1.0) as lines:
                for shade, (dx, dy) in strokes:
                    lines.uniform_float("color", shade)
                    with Canvas.mapped(msgspec.structs.replace(rule, x=rule.x + dx, y=rule.y + dy)):
                        _diagonal().draw(lines)

        match canvas.space:
            case bpy.types.SpaceImageEditor(zoom=zoom, display_channels=channels):
                across, up = np.asarray(zoom)
                for compare in self.compare(bpy.context).to_list():
                    sides = Sides.of(compare, framed(canvas.region), canvas.extent, (texture.width * float(across), texture.height * float(up)), before, metrics, metrics.font(style))
                    texture.filter_mode(bool(across < 1.0))
                    with canvas.clipped(sides.clip):
                        pictured(sides.plate, channels)
                    for rule in sides.rule.to_list():
                        ruled(rule)
                    for caption, x, y, align in sides.captions:
                        canvas.label(caption, (x, y), tint, style, align)
            case _:
                pass


# --- [SERVICES] -------------------------------------------------------------------------


class CompareView(msgspec.Struct, frozen=True, kw_only=True, dict=True):
    """Before frame over images of `kinds` under each screen's compare, with its split handle, header toggles, and the show, mirror, and swap commands a consumer's keymap binds, each command keyed after the view."""

    key: Key
    glyph: Glyph
    read: Callable[[Identity, Metrics], Outcome[Option[Before]]]
    kinds: tuple[str, ...] = ("RENDER_RESULT",)
    refreshes: Callable[[Identity], Outcome[tuple[Refresh, ...]]] = lambda _identity: Ok(())

    # --- [STATE]

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
        return Operator.of(Showing, condition=Some(self.shown), undo=Nothing, named=Some(f"show_{self.key}"))(self.show_compare)

    @cached_property
    def mirroring(self) -> Operator[Empty]:
        """Command mirroring the split, keyed `mirror_<key>`."""
        return Operator.of(Empty, condition=Some(self.shown), undo=Nothing, icon=Some("MOD_MIRROR"), named=Some(f"mirror_{self.key}"))(self.mirror_split)

    @cached_property
    def swapping(self) -> Operator[Empty]:
        """Command swapping the sides, keyed `swap_<key>`."""
        return Operator.of(Empty, condition=Some(self.shown), undo=Nothing, icon=Some("ARROW_LEFTRIGHT"), named=Some(f"swap_{self.key}"))(self.swap_sides)

    # --- [HANDLE]

    @staticmethod
    def compared(attached: Attached[Compare], context: bpy.types.Context) -> Option[Compare]:
        """Returns the context screen's compare while it shows the before frame."""
        return root_in(bpy.types.Screen, context).map(attached.struct).filter(lambda held: Mode[held.mode] is not Mode.OFF)

    def wiped(self, attached: Attached[Compare], context: bpy.types.Context) -> Option[tuple[Compare, Rect, tuple[float, float]]]:
        """Returns a shown wipe with the frame and the visible frame middle from the frame origin, Nothing beside or hidden."""

        def middle(region: bpy.types.Region, frame: Rect) -> tuple[float, float]:
            return ((max(frame.x, 0.0) + min(frame.x + frame.width, region.width)) / 2 - frame.x, (max(frame.y, 0.0) + min(frame.y + frame.height, region.height)) / 2 - frame.y)

        return (
            self
            .compared(attached, context)
            .filter(lambda held: Mode[held.mode] is not Mode.SIDE_BY_SIDE)
            .bind(lambda held: root_in(bpy.types.Region, context).map(lambda region: (held, framed(region), middle(region, framed(region)))))
        )

    def handle(self, attached: Attached[Compare], context: bpy.types.Context) -> Outcome[Option[Matrix]]:
        """Places the split handle at the frame origin while a wipe shows."""
        return Ok(self.wiped(attached, context).map(lambda found: Matrix.Translation((found[1].x, found[1].y, 0.0))))

    def offset(self, attached: Attached[Compare], context: bpy.types.Context) -> tuple[float, ...]:
        """Returns the split handle's offset from the frame origin, on the split at the visible frame middle, zero while no wipe shows."""

        def placed(held: Compare, frame: Rect, middle: tuple[float, float]) -> tuple[float, ...]:
            return (held.split * frame.width, middle[1], 0.0) if Mode[held.mode] is Mode.VERTICAL_WIPE else (middle[0], (1.0 - held.split) * frame.height, 0.0)

        return self.wiped(attached, context).map(lambda found: placed(*found)).default_value((0.0, 0.0, 0.0))

    def moved(self, attached: Attached[Compare], context: bpy.types.Context, values: tuple[float, ...]) -> Outcome[None]:
        """Writes the split the handle's offset along the wipe axis gives, the property bounds keeping it on the frame."""

        def split(held: Compare, frame: Rect) -> float:
            return values[0] / frame.width if Mode[held.mode] is Mode.VERTICAL_WIPE else 1.0 - values[1] / frame.height

        return self.wiped(attached, context).map(lambda found: written(found[0], ("split",), split(found[0], found[1])).map(lambda _: None)).default_value(Ok(None))

    # --- [ROWS]

    @property
    def header(self) -> tuple[Layout]:
        """Layout and swap toggles, drawn while the editor shows an image of `kinds`."""
        return (
            Layout(
                rows=(
                    Property(target=Member(root=self.state, path=("mode",)), icon=Some(self.glyph)),
                    Separator(keywords={"type": "SPACE"}),
                    Property(target=Member(root=self.state, path=("swapped",)), keywords={"icon_only": True}, icon=Some("ARROW_LEFTRIGHT")),
                ),
                shown=Some(self.shown),
            ),
        )

    # --- [REGISTRATION]

    def acquisitions(self, identity: Identity) -> Outcome[Acquisitions]:
        """Returns the compare, its commands, the split handle, the before frame overlay, and its shader release."""

        def rows(attached: Attached[Compare], refreshes: tuple[Refresh, ...]) -> Acquisitions:
            bind = TargetHandler[tuple[float, ...]](get=lambda context: self.offset(attached, context), set=lambda context, values: self.moved(attached, context, values))
            handle = GizmoPart(kind="GIZMO_GT_move_3d", place=lambda context: self.handle(attached, context), bind=bind, properties={"scale_basis": HANDLE, "draw_style": "RING_2D"})
            return (
                self.showing,
                self.mirroring,
                self.swapping,
                Gizmos(key=self.key, space="IMAGE_EDITOR", parts=(handle,), condition=Some(self.shown)),
                overlaid(Wipe(source=lambda metrics: self.read(identity, metrics), compare=lambda context: self.compared(attached, context)), self.shown, refreshes),
                _PROGRAMS,
            )

        return self.state.bound(identity).bind(lambda attached: self.refreshes(identity).map(lambda refreshes: rows(attached, refreshes)))


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
    return shader, Canvas.batched(shader, "TRI_STRIP", {"pos": QUAD})


@cache
def _diagonal() -> gpu.types.GPUBatch:
    """Batches the unit diagonal a rule rectangle maps onto its line."""
    return Canvas.batched("POLYLINE_UNIFORM_COLOR", "LINES", {"pos": _DIAGONAL})


# --- [COMPOSITION] ----------------------------------------------------------------------

_PROGRAMS: Final = programs(_channeled.cache_clear, _diagonal.cache_clear)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Before", "Compare", "CompareView", "Mode", "Showing", "Sides", "Wipe"]
