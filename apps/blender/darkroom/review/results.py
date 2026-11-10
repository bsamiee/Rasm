"""Darkroom render review: the last render's luminance histogram, chromaticity, and clipping tints over Render Result switched per screen, and the render history browsed as the before frame of a compare in the render view's Sidebar tab."""

from collections.abc import Callable
from contextlib import nullcontext
from functools import cache, cached_property, lru_cache, partial
from typing import Annotated, ClassVar, Final

from bl_ui.space_image import IMAGE_HT_header, IMAGE_PT_render_slots, IMAGE_PT_view_histogram
import bpy
from expression import Option, Result, Some
import gpu
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from rasm_bpy.commands import Chord, Keymap, Placement
from rasm_bpy.data import Acquisitions, annotated, Declaration, Handle, Identity, Resource, retained
from rasm_bpy.render import CompletedRender, Consume, EXPOSURE, HistoryBrowser, ResultRead
from rasm_bpy.settings import Attachment
from rasm_bpy.ui import (
    Append,
    Beside,
    Chart,
    chromaticity,
    Color,
    coverage,
    DrawingPart,
    even,
    histogram,
    Icon,
    Layout,
    Masks,
    Member,
    Metrics,
    Mode,
    overlaid,
    Panel,
    Property,
    RenderView,
    RESULT,
    Section,
    Separator,
    Signal,
    Texture,
)
from rasm_imaging import Clipping, converted_image, DisplayView, ImageFile
from rasm_records import accumulated, Error, Nothing, Ok, Outcome, paired

from ..effects.formation.display import current_view
from ..effects.grade.exposure import gain

# --- [MODELS] ---------------------------------------------------------------------------


class Charted(bpy.types.PropertyGroup):
    """Render Result chart switches of one screen: the last render's outlines and the written file's clipping tints."""

    outline: Annotated[bool, bpy.props.BoolProperty(name="Outline", description="Last render's scene-linear luminance outline", default=True)]
    chromaticity: Annotated[bool, bpy.props.BoolProperty(name="Chromaticity", description="Last render's chromaticity with working and display gamuts")]
    low: Annotated[bool, bpy.props.BoolProperty(name="Low Clip", description="Tint pixels at the written file's lowest code")]
    high: Annotated[bool, bpy.props.BoolProperty(name="High Clip", description="Tint pixels at the written file's highest code")]


class _Frames(msgspec.Struct, frozen=True, kw_only=True, eq=False, dict=True):
    """Last render's tagged final image, captured encoding and view, and optional written-code clipping."""

    source: ImageFile
    encoding: Outcome[Option[PyOpenColorIO.Processor]]
    view: DisplayView
    codes: Outcome[Option[npt.NDArray[np.uint8 | np.uint16]]]

    @cached_property
    def chart(self) -> Callable[[Metrics, str, Callable[[Metrics, PyOpenColorIO.Config, npt.NDArray[np.float32], DisplayView], Outcome[Chart]]], Outcome[Chart]]:
        """Charts and native batches owned by this frame's lifetime."""
        linear = lru_cache(maxsize=1)(
            lambda _config_id: self.encoding.bind(lambda source: converted_image(PyOpenColorIO.GetCurrentConfig(), PyOpenColorIO.ROLE_SCENE_LINEAR, self.source, source=source))
        )
        return lru_cache()(lambda metrics, config_id, plot: linear(config_id).bind(lambda file: plot(metrics, PyOpenColorIO.GetCurrentConfig(), file.pixels, self.view)))

    @cached_property
    def textures(self) -> Outcome[tuple[Callable[[], Outcome[gpu.types.GPUTexture]], ...]]:
        """Uploads each clipping mask when drawing first selects it."""

        def uploaded(read: Callable[[], npt.NDArray[np.bool]]) -> Callable[[], Outcome[gpu.types.GPUTexture]]:
            return cache(lambda: coverage(read().astype(np.float32)))

        return self.codes.map(lambda codes: codes.map(Clipping.of).map(lambda clip: tuple(map(uploaded, (lambda: clip.low, lambda: clip.high)))).default_value(()))


# --- [SERVICES] -------------------------------------------------------------------------

_FRAMES: Final = Resource[dict[Handle[bpy.types.Scene], _Frames]](made=lambda _identity: nullcontext({}), renewed=Some(lambda _frames: {}))
_SIGNAL: Final = Resource[Signal](made=lambda _identity: nullcontext(Signal()))


class FrameCharts(msgspec.Struct, frozen=True, kw_only=True):
    """Each scene's last render charted and tinted where its written file clips over Render Result, switched per screen, low codes tinted `under` under the `low` toggle and high codes `over` under `high`."""

    # --- [STATE]

    low: Icon = "TRIA_LEFT"
    high: Icon = "TRIA_RIGHT"
    under: Color = (0.0, 0.0, 1.0, 1.0)
    over: Color = (1.0, 0.0, 0.0, 1.0)
    switches: ClassVar[Attachment[Charted]] = Attachment(key="charts", form=Charted, owner=bpy.types.Screen)
    prerequisites: ClassVar[tuple[Declaration, ...]] = (switches, _FRAMES, _SIGNAL)

    # --- [FRAMES]

    @staticmethod
    def captured(identity: Identity, scene: bpy.types.Scene, _config: PyOpenColorIO.Config) -> Outcome[Consume]:
        """Captures the review view before retaining the completed frame's source and written codes."""
        handle = Handle.of(scene)

        def kept(view: DisplayView, completed: CompletedRender) -> Outcome[Option[str]]:
            return (
                completed
                .read()
                .map(lambda file: _FRAMES[identity].update({handle: _Frames(source=file, encoding=completed.encoding, view=view, codes=completed.codes)}))
                .bind(lambda _: _SIGNAL[identity].fired())
                .map(lambda _: Nothing)
            )

        return current_view(identity, scene).map(lambda view: partial(kept, view))

    @staticmethod
    def shown(identity: Identity) -> Outcome[Option[tuple[Charted, _Frames]]]:
        """Returns the context screen's switches with the context scene's last render, `Nothing` before its first render."""
        frames = _FRAMES[identity]
        retained(frames, bpy.data.scenes)
        match bpy.context.screen, bpy.context.scene:
            case bpy.types.Screen() as screen, bpy.types.Scene() as scene:
                return FrameCharts.switches.read(identity, screen).map(lambda switches: Option.of_optional(frames.get(Handle.of(scene))).map(lambda frames: (switches, frames)))
            case _:
                return Ok(Nothing)

    # --- [CHARTS]

    def charted(self, identity: Identity, on: Callable[[Charted], bool], chart: Callable[[Metrics, PyOpenColorIO.Config, npt.NDArray[np.float32], DisplayView], Outcome[Chart]]) -> DrawingPart[Chart]:
        """Reads a chart of the shown last render while its switch is on."""
        return DrawingPart(
            drawn=Chart.drawn,
            read=lambda metrics: self.shown(identity).bind(
                lambda found: found.filter(lambda pair: on(pair[0])).map(lambda pair: pair[1].chart(metrics, PyOpenColorIO.GetCurrentConfig().getCacheID(), chart).map(Some)).default_value(Ok(Nothing))
            ),
        )

    def tints(self, identity: Identity) -> Outcome[Option[tuple[Texture[Color], ...]]]:
        """Returns the clipping tints the context screen switches on over the shown last render."""
        match self.shown(identity):
            case Result(tag="ok", ok=Option(tag="some", some=(switches, frame))) if switches.low or switches.high:
                return frame.textures.bind(
                    lambda builders: accumulated(paired(builder(), Ok(color)) for builder, color, on in zip(builders, (self.under, self.over), (switches.low, switches.high)) if on)
                ).map(lambda tints: Some(tints).filter(bool))
            case Result(tag="ok"):
                return Ok(Nothing)
            case Result(error=faults):
                return Error(faults)

    # --- [SECTIONS]

    @property
    def clips(self) -> Layout:
        """Icon clipping toggles under a line, drawn while the editor shows Render Result."""
        return Layout(
            rows=(
                Separator(keywords={"type": "LINE"}),
                Layout(
                    rows=tuple(Property(target=Member(root=self.switches, path=(name,)), keywords={"icon_only": True}, icon=Some(icon)) for name, icon in (("low", self.low), ("high", self.high))),
                    container=Some(even),
                ),
            ),
            visible=Some(RESULT),
        )

    @property
    def section(self) -> Section:
        """Chart switches section, inactive for another image."""
        return Section(key=self.switches.key, rows=tuple(Property(target=Member(root=self.switches, path=(name,))) for name in ("outline", "chromaticity")), condition=Some(RESULT))

    # --- [REGISTRATION]

    def acquisitions(self, identity: Identity) -> Outcome[Acquisitions]:
        """Returns the render read, both charts, the clipping masks, and their O and U keys."""
        refreshes = (_SIGNAL[identity], *((Charted, name) for name in annotated(Charted)))
        return self.switches.bound(identity).map(
            lambda attached: (
                ResultRead(capture=partial(self.captured, identity), codes=True),
                overlaid(self.charted(identity, lambda held: held.outline, partial(histogram, stops=(EXPOSURE.hard_min, EXPOSURE.hard_max))), RESULT, refreshes),
                overlaid(self.charted(identity, lambda held: held.chromaticity, chromaticity), RESULT, refreshes),
                overlaid(Masks(read=lambda _metrics: self.tints(identity)), RESULT, refreshes),
                Keymap(
                    keymap="Image",
                    bindings=tuple((chord, Placement.context(bpy.ops.wm.context_toggle, ("screen", attached.name, name))) for chord, name in ((Chord(type="O"), "high"), (Chord(type="U"), "low"))),
                ),
            )
        )


# --- [COMPOSITION] ----------------------------------------------------------------------

_charts: Final = FrameCharts()
history: Final = HistoryBrowser(
    icon="SPLIT_VERTICAL",
    view=lambda identity, scene: paired(current_view(identity, scene), gain(identity, scene)).map(lambda parts: msgspec.structs.replace(parts[0], exposure=parts[0].exposure + parts[1])),
)
rows: Final = (
    _charts,
    Append(target=IMAGE_PT_view_histogram, rows=(_charts.clips,)),
    Panel(section=_charts.section, at=Beside(IMAGE_PT_view_histogram)),
    history,
    RenderView(at=Beside(IMAGE_PT_render_slots)),
    Keymap(
        keymap="Image",
        bindings=(
            (Chord(type="Y"), Placement(history.compare.showing, {"mode": Mode.SIDE_BY_SIDE.name})),
            (Chord(type="Y", shift=1), Placement(history.compare.showing, {"mode": Mode.VERTICAL_WIPE.name})),
            (Chord(type="W"), Placement(history.compare.showing, {"mode": Mode.HORIZONTAL_WIPE.name})),
            (Chord(type="BACK_SLASH"), Placement(history.compare.showing, {"mode": Mode.OFF.name})),
            (Chord(type="W", alt=1), Placement(history.compare.mirroring)),
            (Chord(type="RET"), Placement(history.compare.swapping)),
            (Chord(type="D", shift=1), Placement.context(bpy.ops.wm.context_toggle, ("scene", "render", "use_compositing"))),
        ),
    ),
    Append(target=IMAGE_HT_header, rows=history.compare.header),
    Panel(section=history.section(), at=Beside(IMAGE_PT_render_slots)),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["history", "rows"]
