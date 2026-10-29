"""Darkroom two-image compare: a before frame wiped across or set beside the shown image, its split handle, header toggles, and the image editor keys changing it."""

from collections.abc import Callable, Mapping
from enum import auto, Enum
from functools import cache, cached_property, partial, update_wrapper
from typing import ClassVar, Final, Self

import bpy
from expression import Nothing, Ok, Option, Result, Some
import gpu
from gpu_extras.batch import batch_for_shader
from mathutils import Matrix
import msgspec
import numpy as np
import numpy.typing as npt

from blender.bpy.commands.calls import Placement
from blender.bpy.commands.keymaps import Chord, Keymap
from blender.bpy.commands.modal import Gesture, Modal, Travel
from blender.bpy.commands.operators import Answer, Call, Done, Empty, Operator
from blender.bpy.data.rna.conditions import Setting
from blender.bpy.data.rna.descriptors import Value, written
from blender.bpy.data.rna.mirrors import choices
from blender.bpy.data.rna.paths import RnaPath, root_in, rooted
from blender.bpy.data.rna.snapshots import assigned
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions, Declaration, Registrable
from blender.bpy.settings.attachments import Attached, Attachment
from blender.bpy.ui.drawing.canvas import Align, Canvas, DECODED, DrawType, programs, QUAD, Rect, Shadow
from blender.bpy.ui.drawing.handlers import Refresh
from blender.bpy.ui.images.views import framed, HANDLE, image_type, overlaid
from blender.bpy.ui.layout.rows import Layout, Member, Property, RowContent
from blender.bpy.ui.resources.icons import Glyph
from blender.bpy.ui.resources.previews import uploaded
from blender.bpy.ui.resources.theme import Color, Metrics
from blender.bpy.ui.surfaces.gizmos import GizmoPart, Gizmos
from blender.imaging.files.buffers import unassociated
from blender.records.faults import Fault, Faults, Outcome, paired, Reason

# --- [TYPES] ----------------------------------------------------------------------------

type Dragged = tuple[Compare, float, Rect]


class Mode(Enum):
    """Compare layout, a wipe on either axis or both frames side by side."""

    VERTICAL_WIPE = auto()
    HORIZONTAL_WIPE = auto()
    SIDE_BY_SIDE = auto()


# --- [CONSTANTS] ------------------------------------------------------------------------

MODES: Final = choices((member.name, bpy.path.display_name(member.name.lower()), "") for member in Mode)

# --- [MODELS] ---------------------------------------------------------------------------


class Compare(bpy.types.PropertyGroup):
    """Screen compare: whether the before frame shows, its layout, split, and side."""

    shown: bpy.props.BoolProperty(name="Before", description="Show the before frame")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    mode: bpy.props.EnumProperty(name="Compare Mode", description="Wipe orientation, or both frames side by side", items=MODES)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    split: bpy.props.FloatProperty(name="Split", description="Wipe position", subtype="FACTOR", min=0.0, max=1.0, default=0.5)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    swapped: bpy.props.BoolProperty(name="Swap", description="Before frame right of or below split")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class Before(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Before frame's top-first associated RGBA plane, its caption, and the shown frame's caption."""

    plane: npt.NDArray[np.float32]
    name: str
    shown: str


class Sides(msgspec.Struct, frozen=True, kw_only=True):
    """Compare draw layout with plate, clip, rule, and captions."""

    plate: Rect
    clip: Rect
    rule: Option[tuple[tuple[float, float], tuple[float, float]]]
    captions: tuple[tuple[str, float, float, Align], ...]

    @classmethod
    def of(cls, compare: Option[Compare], frame: Rect, bounds: Rect, size: tuple[float, float], before: Before, gap: float, inset: float, text: float) -> Self:
        """Lays out the plate over a frame inside visible bounds, captioned by the before and shown names, centred on the frame without a compare."""
        (width, height), right, top = size, bounds.x + bounds.width, bounds.y + bounds.height
        plate = Rect(x=frame.x + (frame.width - width) / 2, y=frame.y + (frame.height - height) / 2, width=width, height=height)
        low, high, left = max(frame.y, bounds.y), min(frame.y + frame.height, top), max(frame.x, bounds.x)
        name, shown = before.name, before.shown
        match compare.map(lambda held: (Mode[held.mode], held.split, held.swapped)):
            case Option(tag="some", some=(Mode.SIDE_BY_SIDE, _, swapped)):
                beside = msgspec.structs.replace(plate, x=frame.x + frame.width + gap if swapped else frame.x - gap - width)
                outer = (beside.x + inset, Align.START) if swapped else (beside.x + width - inset, Align.END)
                inner = (frame.x + frame.width - inset, Align.END) if swapped else (frame.x + inset, Align.START)
                return cls(plate=beside, clip=beside, rule=Nothing, captions=((name, outer[0], high - inset - text, outer[1]), (shown, inner[0], high - inset - text, inner[1])))
            case Option(tag="some", some=(Mode.VERTICAL_WIPE, split, swapped)):
                at = frame.x + split * frame.width
                clip = msgspec.structs.replace(bounds, x=at, width=max(right - at, 0.0)) if swapped else msgspec.structs.replace(bounds, width=max(at - bounds.x, 0.0))
                first, last = (shown, name) if swapped else (name, shown)
                return cls(
                    plate=plate, clip=clip, rule=Some(((at, low), (at, high))), captions=((first, at - inset, high - inset - text, Align.END), (last, at + inset, high - inset - text, Align.START))
                )
            case Option(tag="some", some=(Mode.HORIZONTAL_WIPE, split, swapped)):
                at = frame.y + (1.0 - split) * frame.height
                clip = msgspec.structs.replace(bounds, height=max(at - bounds.y, 0.0)) if swapped else msgspec.structs.replace(bounds, y=at, height=max(top - at, 0.0))
                first, last = (shown, name) if swapped else (name, shown)
                return cls(
                    plate=plate,
                    clip=clip,
                    rule=Some(((left, at), (min(frame.x + frame.width, right), at))),
                    captions=((first, left + inset, at + inset, Align.START), (last, left + inset, at - inset - text, Align.START)),
                )
            case _:
                return cls(plate=plate, clip=bounds, rule=Nothing, captions=())


class Wipe(msgspec.Struct, frozen=True, kw_only=True):
    """Before frame drawn in the editor's display channels, wiped or set beside the frame under the shown compare, its rule and captions in white over a hard half-black shadow one point right and down."""

    draw_type: ClassVar[DrawType] = "POST_PIXEL"
    source: Callable[[Metrics], Outcome[Option[Before]]]
    compare: Callable[[bpy.types.Context], Option[Compare]]
    light: ClassVar[Color] = (1.0, 1.0, 1.0, 1.0)
    shade: ClassVar[Color] = (0.0, 0.0, 0.0, 0.5)

    def read(self, metrics: Metrics) -> Outcome[Option[tuple[gpu.types.GPUTexture, Before]]]:
        """Uploads the before frame plane."""
        return self.source(metrics).bind(lambda found: found.map(lambda before: uploaded(unassociated(before.plane)).map(lambda texture: Some((texture, before)))).default_value(Ok(Nothing)))

    def drawn(self, canvas: Canvas, state: tuple[gpu.types.GPUTexture, Before]) -> None:
        """Draws the plate in the editor's channel view with its rule and captions."""
        context, metrics = bpy.context, canvas.metrics
        gap, checker, text, drop = metrics.line(1.0), metrics.checker(), metrics.font(metrics.preferences.ui_styles[0].widget), float(metrics.scaled(1.0))
        shadow = Shadow(color=Wipe.shade, offset=(drop, -drop))
        (shader, quad), (texture, before) = channeled(), state

        def channel(shown: str) -> Option[int]:
            match shown:
                case "COLOR":
                    return Some(-1)
                case "COLOR_ALPHA":
                    return Some(-2)
                case "RED" | "GREEN" | "BLUE" | "ALPHA":
                    return Some(("RED", "GREEN", "BLUE", "ALPHA").index(shown))
                case _:
                    return Nothing

        for space, selector, region in (
            (held, selector, region) for held in root_in(bpy.types.SpaceImageEditor, context).to_list() for selector in channel(held.display_channels).to_list() for region in (canvas.region,)
        ):
            zoom = np.asarray(space.zoom)
            size = (texture.width * float(zoom[0]), texture.height * float(zoom[1]))
            sides = Sides.of(self.compare(context), framed(region), canvas.extent, size, before, gap, metrics.unit * 0.4, text)
            texture.filter_mode(bool(zoom[0] < 1.0))
            with canvas.clipped(sides.clip):
                shader.bind()
                shader.uniform_sampler("image", texture)
                shader.uniform_block(
                    "plate", gpu.types.GPUUniformBuf(gpu.types.Buffer("FLOAT", 12, (sides.plate.x, sides.plate.y, sides.plate.width, sides.plate.height, *checker.primary, *checker.secondary)))
                )
                shader.uniform_float("cell", float(checker.cell))
                shader.uniform_int("shown", selector)
                quad.draw(shader)
            with canvas.lines(1.0) as lines:
                for (start, end), (rgba, (dx, dy)) in ((rule, pair) for rule in sides.rule.to_list() for pair in ((shadow.color, shadow.offset), (Wipe.light, (0.0, 0.0)))):
                    lines.uniform_float("color", rgba)
                    batch_for_shader(lines, "LINES", {"pos": ((start[0] + dx, start[1] + dy), (end[0] + dx, end[1] + dy))}).draw(lines)
            for caption, x, y, align in sides.captions:
                canvas.label(caption, (x, y), Wipe.light, text, align, Some(Some(shadow)))


class CompareView(msgspec.Struct, frozen=True, kw_only=True, dict=True):
    """Before frame over images of `kinds`, its screen compare, split handle, header toggles, and the keys changing them."""

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
    def prerequisites(self) -> tuple[Registrable, ...]:
        """Compare the header, handle, and overlay bind."""
        return (self.state,)

    # --- [COMMANDS]

    def screened(self, change: Callable[[Compare], Mapping[RnaPath, Value]], call: Call[Empty]) -> Outcome[Answer]:
        """Writes one change of the calling screen's compare."""
        return rooted(bpy.types.Screen, call.context).bind(lambda screen: self.state.read(call.identity, screen)).bind(lambda held: assigned(held, change(held))).map(lambda _: Done())

    @cached_property
    def commands(self) -> tuple[tuple[Chord, Operator[Empty]], ...]:
        """Layout and mirrored split commands with their image editor key."""

        def show_side_by_side(compare: Compare) -> Mapping[RnaPath, Value]:
            """Show the before frame beside the image."""
            return {("shown",): not (compare.shown and Mode[compare.mode] is Mode.SIDE_BY_SIDE), ("mode",): Mode.SIDE_BY_SIDE.name}

        def show_vertical_wipe(compare: Compare) -> Mapping[RnaPath, Value]:
            """Split the image with the before frame across a vertical wipe."""
            return {("shown",): not (compare.shown and Mode[compare.mode] is Mode.VERTICAL_WIPE), ("mode",): Mode.VERTICAL_WIPE.name}

        def show_horizontal_wipe(compare: Compare) -> Mapping[RnaPath, Value]:
            """Wipe the before frame over the image along a horizontal wipe."""
            return {("shown",): not (compare.shown and Mode[compare.mode] is Mode.HORIZONTAL_WIPE), ("mode",): Mode.HORIZONTAL_WIPE.name}

        def mirror_split(compare: Compare) -> Mapping[RnaPath, Value]:
            """Mirror the wipe across the frame."""
            return {("split",): 1.0 - compare.split}

        return tuple(
            (chord, Operator(undo=Nothing, form=Empty, run=update_wrapper(partial(self.screened, change), change), condition=Some(self.shown)))
            for chord, change in ((Chord(type="Y"), show_side_by_side), (Chord(type="Y", shift=1), show_vertical_wipe), (Chord(type="W"), show_horizontal_wipe), (Chord(type="W", alt=1), mirror_split))
        )

    # --- [ROWS]

    @property
    def header(self) -> tuple[Layout]:
        """Before, mode, and swap toggles, drawn while the editor shows an image of `kinds`."""

        def spaced(layout: bpy.types.UILayout, _holder: object, _context: bpy.types.Context) -> None:
            layout.separator(type="SPACE")

        def rows(identity: Identity) -> Outcome[tuple[RowContent, ...]]:
            return self.state.bound(identity).map(
                lambda attached: (
                    Property(target=Member(root=bpy.types.Screen, path=(attached.name, "shown")), keywords={"icon_only": True}, icon=Some(self.glyph)),
                    spaced,
                    Property(target=Member(root=bpy.types.Screen, path=(attached.name, "mode")), keywords={"expand": True}),
                    spaced,
                    Property(target=Member(root=bpy.types.Screen, path=(attached.name, "swapped")), keywords={"icon_only": True}, icon=Some("ARROW_LEFTRIGHT")),
                )
            )

        return (Layout(rows=rows, shown=Some(self.shown)),)

    # --- [REGISTRATION]

    def acquisitions(self, identity: Identity) -> Outcome[Acquisitions]:
        """Returns the split drag command and handle, the before frame overlay, its shader release, and the keys with their commands."""

        def ruled(region: bpy.types.Region, compare: Compare) -> Option[tuple[float, float]]:
            frame = framed(region)
            middle = ((max(frame.x, 0.0) + min(frame.x + frame.width, region.width)) / 2, (max(frame.y, 0.0) + min(frame.y + frame.height, region.height)) / 2)
            match Mode[compare.mode]:
                case Mode.VERTICAL_WIPE:
                    return Some((frame.x + compare.split * frame.width, middle[1]))
                case Mode.HORIZONTAL_WIPE:
                    return Some((middle[0], frame.y + (1.0 - compare.split) * frame.height))
                case Mode.SIDE_BY_SIDE:
                    return Nothing

        def rows(attached: Attached[Compare], refreshes: tuple[Refresh, ...]) -> tuple[Declaration, ...]:
            def compared(context: bpy.types.Context) -> Option[Compare]:
                return root_in(bpy.types.Screen, context).map(attached.struct).filter(lambda compare: compare.shown)

            def drag_split(_call: Call[Empty]) -> Outcome[Answer]:
                """Drag compare split, Shift for tenth speed, Escape restores."""
                return Ok(Done())

            def place(context: bpy.types.Context) -> Outcome[Option[Matrix]]:
                return Ok(compared(context).bind(lambda compare: root_in(bpy.types.Region, context).bind(lambda region: ruled(region, compare))).map(lambda at: Matrix.Translation((*at, 0.0))))

            def begin(context: bpy.types.Context, _travel: Travel) -> Outcome[Dragged]:
                return Result[Compare, Faults].bind(
                    compared(context).filter(lambda compare: Mode[compare.mode] is not Mode.SIDE_BY_SIDE).to_result_with(partial(Fault.of, Reason.ABSENT, repr(context.screen), member=self.key)),
                    lambda compare: rooted(bpy.types.Region, context).map(lambda region: (compare, compare.split, framed(region))),
                )

            def preview(_context: bpy.types.Context, state: Dragged, travel: Travel) -> Outcome[Option[str]]:
                compare, split, frame = state
                change = travel.dx / frame.width if Mode[compare.mode] is Mode.VERTICAL_WIPE else -travel.dy / frame.height
                return written(compare, ("split",), min(max(split + change * (0.1 if travel.precise else 1.0), 0.0), 1.0)).map(lambda _: Nothing)

            command = Operator[Empty](
                form=Empty,
                run=drag_split,
                invoke=Some(Modal(gesture=Gesture.DRAG, begin=begin, preview=preview, restore=lambda _context, state: written(state[0], ("split",), state[1]).map(lambda _: None))),
                condition=Some(self.shown),
                internal=True,
            )
            return (
                command,
                Gizmos(
                    key=self.key,
                    space="IMAGE_EDITOR",
                    parts=(GizmoPart(kind="GIZMO_GT_move_3d", place=place, bind=(Placement(command),), properties={"scale_basis": HANDLE, "draw_style": "RING_2D"}),),
                    condition=Some(self.shown),
                ),
                overlaid(Wipe(source=lambda metrics: self.read(identity, metrics), compare=compared), self.shown, refreshes),
                _SHADERS,
                *(command for _, command in self.commands),
                Keymap(
                    keymap="Image",
                    bindings=(
                        *((chord, Placement(command)) for chord, command in self.commands),
                        (Chord(type="BACK_SLASH"), Placement.context(bpy.ops.wm.context_toggle, ("screen", attached.name, "shown"))),
                        (Chord(type="RET"), Placement.context(bpy.ops.wm.context_toggle, ("screen", attached.name, "swapped"))),
                        (Chord(type="D", shift=1), Placement.context(bpy.ops.wm.context_toggle, ("scene", "render", "use_compositing"))),
                    ),
                ),
            )

        return paired(self.state.bound(identity), self.refreshes(identity)).map(lambda pair: rows(*pair))


# --- [OPERATIONS] -----------------------------------------------------------------------


@cache
def channeled() -> tuple[gpu.types.GPUShader, gpu.types.GPUBatch]:
    """Builds the channel shader and its unit quad."""
    vertex = "void main() { uv = pos; gl_Position = ModelViewProjectionMatrix * vec4(plate.rect.xy + pos * plate.rect.zw, 0.0, 1.0); }"
    fragment = (
        DECODED
        + """
void main() {
    vec4 texel = texture(image, uv);
    vec2 cells = floor(gl_FragCoord.xy / cell);
    vec3 ground = mod(cells.x + cells.y, 2.0) < 1.0 ? plate.primary.rgb : plate.secondary.rgb;
    vec3 shade = shown >= 0 ? vec3(texel[shown]) : shown == -2 ? texel.rgb * texel.a + (1.0 - texel.a) * ground : texel.rgb;
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
    return shader, batch_for_shader(shader, "TRI_STRIP", {"pos": QUAD.tolist()})


# --- [COMPOSITION] ----------------------------------------------------------------------

_SHADERS: Final = programs(channeled.cache_clear)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Before", "Compare", "CompareView", "Mode"]
