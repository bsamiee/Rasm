"""Darkroom two-image compare: a before frame wiped across or set beside the shown image, its split handle, header toggles, and the image editor keys changing it."""

from collections.abc import Callable
from functools import cache, cached_property, partial
from typing import Annotated, Final, Self

import bpy
from expression import Nothing, Ok, Option, Some
import gpu
from gpu_extras.batch import batch_for_shader
from mathutils import Matrix
import msgspec
import numpy as np
import numpy.typing as npt

from blender.bpy.commands.keymaps import Chord, Keymap, Toggle
from blender.bpy.commands.modal import Gesture, Modal, Travel
from blender.bpy.commands.operators import Call, Done, Empty, Operator, Outcome
from blender.bpy.data.rna.conditions import Setting
from blender.bpy.data.rna.descriptors import root_in, rooted
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions, Registrable
from blender.bpy.extension.scoped import Scoped
from blender.bpy.faults import BlenderAbsentFault, BlenderResult
from blender.bpy.settings.attachments import Attached, Attachment
from blender.bpy.ui.drawing.canvas import Align, Canvas, DECODED, QUAD, Rect, released, Shadow
from blender.bpy.ui.drawing.handlers import Refresh
from blender.bpy.ui.images.cages import HANDLE
from blender.bpy.ui.images.overlays import overlaid
from blender.bpy.ui.images.views import framed, image_type
from blender.bpy.ui.layout.rows import Divider, Member, Property, Row, Rows
from blender.bpy.ui.resources.icons import Glyph
from blender.bpy.ui.resources.textures import uploaded
from blender.bpy.ui.resources.theme import Look, Metrics, Shipped
from blender.bpy.ui.surfaces.gizmos import Gizmos, Move
from blender.imaging.files.images import unassociated
from blender.records.faults import paired
from blender.records.fields import Choice, Gated, heading

# --- [TYPES] ----------------------------------------------------------------------------


class Mode(Choice):
    """Compare layout, a wipe on either axis or both frames side by side."""

    VERTICAL_WIPE = 1
    HORIZONTAL_WIPE = 2
    SIDE_BY_SIDE = 3


# --- [MODELS] ---------------------------------------------------------------------------


class Compare(msgspec.Struct, frozen=True, kw_only=True):
    """Compare layout, split, and before frame side."""

    mode: Annotated[Mode, msgspec.Meta(title="Compare Mode", description="Wipe orientation, or both frames side by side")] = Mode.VERTICAL_WIPE
    split: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, title="Split", description="Wipe position")] = 0.5
    swapped: Annotated[bool, msgspec.Meta(title="Swap", description="Before frame right of or below split")] = False


class Before(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Before frame's top-first associated RGBA plane, its caption, and the shown frame's caption."""

    plane: npt.NDArray[np.float32]
    name: str
    shown: str


class Plate(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Uploaded before frame with the before and shown captions."""

    texture: gpu.types.GPUTexture
    name: str
    shown: str


class Sides(msgspec.Struct, frozen=True, kw_only=True):
    """Compare draw layout with plate, clip, rule, and captions."""

    plate: Rect
    clip: Rect
    rule: Option[tuple[tuple[float, float], tuple[float, float]]]
    captions: tuple[tuple[str, float, float, Align], ...]

    @classmethod
    def of(cls, compare: Option[Compare], frame: Rect, bounds: Rect, size: tuple[float, float], state: Plate, gap: float, inset: float, text: float) -> Self:
        """Lays out the plate over a frame inside visible bounds, captioned by the plate's before and shown names, centred on the frame without a compare."""
        (width, height), right, top = size, bounds.x + bounds.width, bounds.y + bounds.height
        plate = Rect(x=frame.x + (frame.width - width) / 2, y=frame.y + (frame.height - height) / 2, width=width, height=height)
        low, high, left = max(frame.y, bounds.y), min(frame.y + frame.height, top), max(frame.x, bounds.x)
        name, shown = state.name, state.shown
        match compare:
            case Option(tag="some", some=Compare(mode=Mode.SIDE_BY_SIDE, swapped=swapped)):
                beside = msgspec.structs.replace(plate, x=frame.x + frame.width + gap if swapped else frame.x - gap - width)
                outer = (beside.x + inset, Align.START) if swapped else (beside.x + width - inset, Align.END)
                inner = (frame.x + frame.width - inset, Align.END) if swapped else (frame.x + inset, Align.START)
                return cls(plate=beside, clip=beside, rule=Nothing, captions=((name, outer[0], high - inset - text, outer[1]), (shown, inner[0], high - inset - text, inner[1])))
            case Option(tag="some", some=Compare(mode=Mode.VERTICAL_WIPE, split=split, swapped=swapped)):
                at = frame.x + split * frame.width
                clip = msgspec.structs.replace(bounds, x=at, width=max(right - at, 0.0)) if swapped else msgspec.structs.replace(bounds, width=max(at - bounds.x, 0.0))
                first, last = (shown, name) if swapped else (name, shown)
                return cls(
                    plate=plate, clip=clip, rule=Some(((at, low), (at, high))), captions=((first, at - inset, high - inset - text, Align.END), (last, at + inset, high - inset - text, Align.START))
                )
            case Option(tag="some", some=Compare(mode=Mode.HORIZONTAL_WIPE, split=split, swapped=swapped)):
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
    """Before frame drawn in the editor's display channels, wiped or set beside the frame under the shown compare, its rule and captions in the caption look."""

    identity: Identity
    source: Callable[[Metrics], BlenderResult[Option[Before]]]
    compare: Callable[[bpy.types.Context], BlenderResult[Option[Compare]]]
    look: Look

    def read(self, metrics: Metrics) -> BlenderResult[Option[Plate]]:
        """Uploads the before frame plane."""
        return self.source(metrics).bind(
            lambda found: found.map(lambda before: uploaded(unassociated(before.plane)).map(lambda texture: Some(Plate(texture=texture, name=before.name, shown=before.shown)))).default_value(
                Ok(Nothing)
            )
        )

    def drawn(self, canvas: Canvas, state: Plate) -> None:
        """Draws the plate in the editor's channel view with its rule and captions."""
        context, metrics = bpy.context, canvas.metrics
        gap, checker, text, light = metrics.line(1.0), metrics.checker(), metrics.points(), self.look.text
        shadow = Shadow.cast(self.look, metrics)
        shader, quad = channeled()

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

        for space, compare, selector, region in (
            (held, compare, selector, region)
            for held in root_in(bpy.types.SpaceImageEditor, context).to_list()
            for compare in self.identity.reports.reported(self.compare)(context).to_list()
            for selector in channel(held.display_channels).to_list()
            for region in canvas.region.to_list()
        ):
            zoom = np.asarray(space.zoom)
            size = (state.texture.width * float(zoom[0]), state.texture.height * float(zoom[1]))
            sides = Sides.of(compare, framed(region), canvas.extent, size, state, gap, metrics.unit * 0.4, text)
            state.texture.filter_mode(bool(zoom[0] < 1.0))
            with canvas.clipped(sides.clip):
                shader.bind()
                shader.uniform_sampler("image", state.texture)
                shader.uniform_block(
                    "plate", gpu.types.GPUUniformBuf(gpu.types.Buffer("FLOAT", 12, (sides.plate.x, sides.plate.y, sides.plate.width, sides.plate.height, *checker.primary, *checker.secondary)))
                )
                shader.uniform_float("cell", float(checker.cell))
                shader.uniform_int("shown", selector)
                quad.draw(shader)
            lines = canvas.lines(1.0)
            for (start, end), (ink, (dx, dy)) in ((rule, pair) for rule in sides.rule.to_list() for pair in (*((shade.color, shade.offset) for shade in shadow.to_list()), (light, (0.0, 0.0)))):
                lines.uniform_float("color", ink)
                batch_for_shader(lines, "LINES", {"pos": ((start[0] + dx, start[1] + dy), (end[0] + dx, end[1] + dy))}).draw(lines)
            for caption, x, y, align in sides.captions:
                canvas.label(caption, (x, y), light, text, align, shadow)


class CompareView(msgspec.Struct, frozen=True, kw_only=True, dict=True):
    """Before frame over images of `kinds`, its screen compare record, split handle, header toggles, and the keys changing them."""

    key: Key
    glyph: Glyph
    read: Callable[[Identity, Metrics], BlenderResult[Option[Before]]]
    kinds: tuple[str, ...] = ("RENDER_RESULT",)
    refreshes: Callable[[Identity], BlenderResult[tuple[Refresh, ...]]] = lambda _identity: Ok(())
    look: Look = msgspec.field(default_factory=lambda: Shipped.read().look("overlay-caption"))

    # --- [STATE]

    @cached_property
    def state(self) -> Attachment[Gated[Compare]]:
        """Screen compare record, gated by whether the before frame shows."""
        return Attachment(key=self.key, form=Gated[Compare], owner=bpy.types.Screen)

    @property
    def shown(self) -> Setting:
        """Condition that the editor shows an image of `kinds`."""
        return image_type(self.kinds)

    @property
    def prerequisites(self) -> tuple[Registrable, ...]:
        """Compare record the header, handle, and overlay bind."""
        return (self.state,)

    @staticmethod
    def compared(attached: Attached[Gated[Compare]], context: bpy.types.Context) -> BlenderResult[Option[Compare]]:
        """Reads the calling screen's compare while it shows."""
        return root_in(bpy.types.Screen, context).map(lambda screen: attached.read(screen).map(lambda gated: Some(gated.value) if gated.enabled else Nothing)).default_value(Ok(Nothing))

    # --- [COMMANDS]

    def screened(self, key: Key, description: str, change: Callable[[Gated[Compare]], Gated[Compare]]) -> Operator[Empty]:
        """Command writing one change of the calling screen's compare while the editor shows an image of `kinds`."""

        def run(call: Call[Empty]) -> BlenderResult[Outcome]:
            return call.root(bpy.types.Screen).bind(lambda screen: self.state.revised(call.identity, screen, lambda held: Ok(change(held)))).map(lambda _: Done())

        return Operator(key=f"{self.key}_{key}", title=heading(key), description=description, undo=False, form=Empty, run=run, condition=Some(self.shown))

    @cached_property
    def commands(self) -> tuple[tuple[Chord, Operator[Empty]], ...]:
        """Compare commands with their image editor key: before frame, layouts, mirrored split, and swapped sides."""
        return (
            (Chord(type="BACK_SLASH"), self.screened("before", "Show or hide the before frame", lambda held: msgspec.structs.replace(held, enabled=not held.enabled))),
            (Chord(type="Y"), self.screened("side_by_side", "Show the before frame beside the image", partial(moded, Mode.SIDE_BY_SIDE))),
            (Chord(type="Y", shift=1), self.screened("vertical_wipe", "Split the image with the before frame across a vertical wipe", partial(moded, Mode.VERTICAL_WIPE))),
            (Chord(type="W"), self.screened("horizontal_wipe", "Wipe the before frame over the image along a horizontal wipe", partial(moded, Mode.HORIZONTAL_WIPE))),
            (Chord(type="W", alt=1), self.screened("mirror", "Mirror the wipe across the frame", partial(laid, lambda held: msgspec.structs.replace(held, split=1.0 - held.split)))),
            (Chord(type="RET"), self.screened("swap", "Swap the before frame and the image across the wipe", partial(laid, lambda held: msgspec.structs.replace(held, swapped=not held.swapped)))),
        )

    @cached_property
    def keys(self) -> Keymap:
        """Image editor keymap of the compare commands and the scene compositing toggle."""
        return Keymap(keymap="Image", bindings={**dict(self.commands), Chord(type="D", shift=1): Toggle(path="scene.render.use_compositing")})

    # --- [ROWS]

    @property
    def header(self) -> tuple[Row]:
        """Gate, mode, and swap toggles, drawn while the editor shows an image of `kinds`."""

        def rows(identity: Identity) -> BlenderResult[tuple[Row, ...]]:
            (enabled, value), (mode, _, swapped) = (tuple(field.name for field in msgspec.structs.fields(record)) for record in (Gated, Compare))
            return self.state.bound(identity).map(
                lambda attached: (
                    Property(target=Member(root=bpy.types.Screen, path=f"{attached.name}.{enabled}"), icon=Some(self.glyph), icon_only=True),
                    Divider(type="SPACE"),
                    Property(target=Member(root=bpy.types.Screen, path=f"{attached.name}.{value}.{mode}"), expand=True),
                    Divider(type="SPACE"),
                    Property(target=Member(root=bpy.types.Screen, path=f"{attached.name}.{value}.{swapped}"), icon=Some("ARROW_LEFTRIGHT"), icon_only=True),
                )
            )

        return (Rows(rows=rows, condition=Some(self.shown), hidden=True),)

    # --- [REGISTRATION]

    def acquisitions(self, identity: Identity) -> BlenderResult[Acquisitions]:
        """Returns the split drag command and handle, the before frame overlay, its shader release, and the keys with their commands."""

        def ruled(region: bpy.types.Region, compare: Compare) -> Option[tuple[float, float]]:
            frame = framed(region)
            middle = ((max(frame.x, 0.0) + min(frame.x + frame.width, region.width)) / 2, (max(frame.y, 0.0) + min(frame.y + frame.height, region.height)) / 2)
            match compare.mode:
                case Mode.VERTICAL_WIPE:
                    return Some((frame.x + compare.split * frame.width, middle[1]))
                case Mode.HORIZONTAL_WIPE:
                    return Some((middle[0], frame.y + (1.0 - compare.split) * frame.height))
                case Mode.SIDE_BY_SIDE:
                    return Nothing

        def rows(attached: Attached[Gated[Compare]], refreshes: tuple[Refresh, ...]) -> tuple[Registrable, ...]:
            def compared(context: bpy.types.Context) -> BlenderResult[Option[Compare]]:
                return CompareView.compared(attached, context)

            def place(context: bpy.types.Context) -> BlenderResult[Option[Matrix]]:
                return compared(context).map(
                    lambda found: found.bind(lambda compare: root_in(bpy.types.Region, context).bind(lambda region: ruled(region, compare))).map(lambda at: Matrix.Translation((*at, 0.0)))
                )

            def write(screen: bpy.types.Screen, split: float) -> BlenderResult[None]:
                return self.state.revised(identity, screen, lambda gated: Ok(msgspec.structs.replace(gated, value=msgspec.structs.replace(gated.value, split=split)))).map(lambda _: None)

            def begin(context: bpy.types.Context, _travel: Travel) -> BlenderResult[tuple[bpy.types.Screen, Compare, Rect]]:
                return (
                    compared(context)
                    .bind(lambda found: found.filter(lambda compare: compare.mode is not Mode.SIDE_BY_SIDE).to_result_with(lambda: (BlenderAbsentFault(owner=repr(context.screen), key=self.key),)))
                    .bind(lambda compare: paired(rooted(bpy.types.Screen, context), rooted(bpy.types.Region, context)).map(lambda pair: (pair[0], compare, framed(pair[1]))))
                )

            def preview(_context: bpy.types.Context, state: tuple[bpy.types.Screen, Compare, Rect], travel: Travel) -> BlenderResult[Option[str]]:
                screen, compare, frame = state
                change = travel.dx / frame.width if compare.mode is Mode.VERTICAL_WIPE else -travel.dy / frame.height
                return write(screen, min(max(compare.split + change * (0.1 if travel.precise else 1.0), 0.0), 1.0)).map(lambda _: Nothing)

            command = Operator[Empty, tuple[bpy.types.Screen, Compare, Rect]](
                key=f"{self.key}_split",
                title=heading(f"{self.key}_split"),
                description="Drag compare split, Shift for tenth speed, Escape restores",
                form=Empty,
                run=lambda _call: Ok(Done()),
                invoke=Some(Modal(gesture=Gesture.DRAG, begin=begin, preview=preview, restore=lambda _context, state: write(state[0], state[1].split))),
                condition=Some(self.shown),
                internal=True,
            )
            return (
                command,
                Gizmos(key=self.key, title=heading(self.key), space="IMAGE_EDITOR", parts=(Move(place=place, bind=command, scale_basis=HANDLE, draw_style="RING_2D"),), condition=Some(self.shown)),
                overlaid(Wipe(identity=identity, source=lambda metrics: self.read(identity, metrics), compare=compared, look=self.look), self.shown, refreshes),
                _SHADERS,
                *(command for _, command in self.commands),
                self.keys,
            )

        return paired(self.state.bound(identity), self.refreshes(identity)).map(lambda pair: rows(*pair))


# --- [OPERATIONS] -----------------------------------------------------------------------


def moded(mode: Mode, held: Gated[Compare]) -> Gated[Compare]:
    """Shows the before frame in `mode`, hiding it when it already shows so."""
    return Gated(enabled=not (held.enabled and held.value.mode is mode), value=msgspec.structs.replace(held.value, mode=mode))


def laid(change: Callable[[Compare], Compare], held: Gated[Compare]) -> Gated[Compare]:
    """Changes the shown compare layout."""
    return msgspec.structs.replace(held, value=change(held.value))


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
    return shader, batch_for_shader(shader, "TRI_STRIP", {"pos": QUAD})


# --- [COMPOSITION] ----------------------------------------------------------------------

_SHADERS: Final = Scoped[None](made=lambda _identity: released(channeled.cache_clear))

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Before", "Compare", "CompareView", "Mode"]
