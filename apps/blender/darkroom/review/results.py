"""Darkroom render review: the last render's luminance histogram, chromaticity, and clipping tints over Render Result switched per screen, and the render history browsed as the before frame of a compare in the render view's Sidebar tab."""

from collections.abc import Callable
from contextlib import nullcontext
from pathlib import Path
from typing import ClassVar, Final

from bl_ui.space_image import IMAGE_HT_header, IMAGE_PT_render_slots, IMAGE_PT_view_histogram
import bpy
from expression import Nothing, Ok, Option, Some
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.bpy.commands.keymaps import Chord, Keymap
from blender.bpy.commands.operators import Placement
from blender.bpy.data.ids.store import Handle
from blender.bpy.data.rna.properties import annotated
from blender.bpy.extension.identity import Identity, Resource
from blender.bpy.extension.registration import Acquisitions, Declaration
from blender.bpy.render.color.spaces import EXPOSURE
from blender.bpy.render.outputs.formats import depth, rounds_straight
from blender.bpy.render.outputs.results import combined, ResultRead
from blender.bpy.render.review.browsing import HistoryBrowser
from blender.bpy.settings.attachments import Attachment
from blender.bpy.ui.drawing.charts import Chart, chromaticity, histogram
from blender.bpy.ui.drawing.handlers import Refresh, SelfDrawnPart, Signal
from blender.bpy.ui.elements.menus import Append
from blender.bpy.ui.elements.panels import Beside, Panel
from blender.bpy.ui.images.compare import Mode
from blender.bpy.ui.images.views import Masks, overlaid, RenderView, RESULT, Tint
from blender.bpy.ui.layout.members import Member
from blender.bpy.ui.layout.rows import even, Layout, Property, Separator
from blender.bpy.ui.layout.sections import Section
from blender.bpy.ui.resources.icons import Icon
from blender.bpy.ui.resources.theme import Color, Metrics
from blender.imaging.analysis.metering import Clipping
from blender.imaging.color.processors import DisplayView
from blender.imaging.files.buffers import Coding, Depth, unassociated
from blender.records.faults import Outcome, paired

from ..effects.formation.display import current_view, displayed_frame
from ..effects.grade.exposure import gain

# --- [MODELS] ---------------------------------------------------------------------------


class Charted(bpy.types.PropertyGroup):
    """Render Result chart switches of one screen: the last render's outlines and the written file's clipping tints."""

    outline: bpy.props.BoolProperty(name="Outline", description="Last render's scene-linear luminance outline", default=True)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    chromaticity: bpy.props.BoolProperty(name="Chromaticity", description="Last render's chromaticity with working and display gamuts")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    low: bpy.props.BoolProperty(name="Low Clip", description="Tint pixels at the written file's lowest code")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    high: bpy.props.BoolProperty(name="High Clip", description="Tint pixels at the written file's highest code")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class Frames(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Last render's scene-linear plane with the view forming it, and the pixels its written file clips, `Nothing` for a format storing floats."""

    linear: npt.NDArray[np.float32]
    view: DisplayView
    clipping: Option[Clipping]


# --- [SERVICES] -------------------------------------------------------------------------

_FRAMES: Final = Resource[dict[Handle[bpy.types.Scene], Frames]](made=lambda _identity: nullcontext({}))
_SIGNAL: Final = Resource[Signal](made=lambda _identity: nullcontext(Signal()))


class FrameCharts(msgspec.Struct, frozen=True, kw_only=True):
    """Each scene's last render charted and tinted where its written file clips over Render Result, switched per screen, low codes tinted `under` under the `low` toggle and high codes `over` under `high`."""

    low: Icon = "TRIA_LEFT"
    high: Icon = "TRIA_RIGHT"
    under: Color = (0.0, 0.0, 1.0, 1.0)
    over: Color = (1.0, 0.0, 0.0, 1.0)
    switches: ClassVar[Attachment[Charted]] = Attachment(key="charts", form=Charted, owner=bpy.types.Screen)
    prerequisites: ClassVar[tuple[Declaration, ...]] = (switches, _FRAMES, _SIGNAL)

    # --- [FRAMES]

    @staticmethod
    def clipped(settings: bpy.types.ImageFormatSettings, formed: npt.NDArray[np.float32]) -> Outcome[Option[Clipping]]:
        """Returns the display pixels the format's integer codes clip, coded straight where its writer rounds straight color, `Nothing` for a format storing floats."""
        match depth(settings):
            case Option(tag="some", some=Depth.UINT8 | Depth.UINT16 as integer):
                return Coding.ROUNDED.coded(unassociated(formed) if rounds_straight(settings) else formed, integer).map(lambda codes: Some(Clipping.of(codes)))
            case _:
                return Ok(Nothing)

    @staticmethod
    def consumed(identity: Identity, scene: bpy.types.Scene, path: str) -> Outcome[Option[str]]:
        """Holds a finished render's frames for its scene, then refreshes every chart."""
        config = PyOpenColorIO.GetCurrentConfig()

        def kept(plane: npt.NDArray[np.float32], view: DisplayView) -> Outcome[None]:
            return (
                displayed_frame(identity, scene, plane, PyOpenColorIO.ROLE_SCENE_LINEAR, config)
                .bind(lambda file: FrameCharts.clipped(scene.render.image_settings, file.pixels))
                .map(lambda clipping: identity.resources[_FRAMES].update({Handle.of(scene): Frames(linear=plane, view=view, clipping=clipping)}))
                .bind(lambda _: identity.resources[_SIGNAL].fired())
            )

        return paired(combined(path), current_view(identity, scene)).bind(lambda found: kept(*found)).map(lambda _: Nothing)

    @staticmethod
    def shown(identity: Identity) -> Outcome[Option[tuple[Charted, Frames]]]:
        """Returns the context screen's switches with the context scene's last render, `Nothing` before its first render."""
        match bpy.context.screen, bpy.context.scene:
            case bpy.types.Screen() as screen, bpy.types.Scene() as scene:
                return FrameCharts.switches.read(identity, screen).map(lambda switches: Option.of_optional(identity.resources[_FRAMES].get(Handle.of(scene))).map(lambda frames: (switches, frames)))
            case _:
                return Ok(Nothing)

    # --- [CHARTS]

    def charted(self, identity: Identity, on: Callable[[Charted], bool], chart: Callable[[Metrics, PyOpenColorIO.Config, Frames], Outcome[Chart]]) -> SelfDrawnPart[Chart]:
        """Reads a chart of the shown last render while its switch is on."""
        return SelfDrawnPart(
            read=lambda metrics: self.shown(identity).bind(
                lambda found: found.filter(lambda pair: on(pair[0])).map(lambda pair: chart(metrics, PyOpenColorIO.GetCurrentConfig(), pair[1]).map(Some)).default_value(Ok(Nothing))
            )
        )

    def tints(self, identity: Identity) -> Outcome[Option[tuple[Tint, ...]]]:
        """Returns the clipping tints the context screen switches on over the shown last render."""
        return self.shown(identity).map(
            lambda found: found.bind(
                lambda pair: pair[1].clipping.map(
                    lambda clip: tuple(Tint(plane=mask.astype(np.float32), color=rgba) for mask, rgba, on in ((clip.low, self.under, pair[0].low), (clip.high, self.over, pair[0].high)) if on)
                )
            ).filter(bool)
        )

    # --- [ROWS]

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
        refreshes: tuple[Refresh, ...] = (identity.resources[_SIGNAL], *((Charted, name) for name in annotated(Charted)))
        return self.switches.bound(identity).map(
            lambda attached: (
                ResultRead(consume=lambda scene, path: self.consumed(identity, scene, path)),
                overlaid(
                    self.charted(
                        identity, lambda held: held.outline, lambda metrics, config, frames: histogram(metrics, config, frames.linear, frames.view, (EXPOSURE.prop.hard_min, EXPOSURE.prop.hard_max))
                    ),
                    RESULT,
                    refreshes,
                ),
                overlaid(self.charted(identity, lambda held: held.chromaticity, lambda metrics, config, frames: chromaticity(metrics, config, frames.linear, frames.view)), RESULT, refreshes),
                overlaid(Masks(source=lambda _metrics: self.tints(identity)), RESULT, refreshes),
                Keymap(
                    keymap="Image",
                    bindings=tuple((chord, Placement.context(bpy.ops.wm.context_toggle, ("screen", attached.name, name))) for chord, name in ((Chord(type="O"), "high"), (Chord(type="U"), "low"))),
                ),
            )
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


def ungraded(identity: Identity, scene: bpy.types.Scene) -> Outcome[DisplayView]:
    """Returns the view the view layer ahead of the chain forms through: the earliest formation entry's, else the scene's, with the Exposure entries' gain added to its exposure."""

    def gained(view: DisplayView, stops: float) -> DisplayView:
        return msgspec.structs.replace(view, exposure=view.exposure + stops)

    return paired(current_view(identity, scene), gain(identity, scene)).map(lambda parts: gained(*parts))


# --- [COMPOSITION] ----------------------------------------------------------------------

charts: Final = FrameCharts()
history: Final = HistoryBrowser(icon=Path("vertical-wipe.svg"), view=ungraded)
rows: Final = (
    charts,
    Append(target=IMAGE_PT_view_histogram, rows=(charts.clips,)),
    Panel(section=charts.section, at=Beside(IMAGE_PT_view_histogram)),
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

__all__ = ["Charted", "FrameCharts", "Frames", "charts", "history", "rows", "ungraded"]
