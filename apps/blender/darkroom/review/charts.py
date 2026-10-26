"""Darkroom render charts over Render Result: the last render's scene-linear luminance histogram with its view's exposure bands, its chromaticity with working and display gamuts, and tints at the written file's code ends."""

from collections.abc import Callable
from functools import cached_property
from typing import Annotated, Final, get_args

from bl_ui.space_image import IMAGE_PT_view_histogram
import bpy
from expression import Nothing, Ok, Option, Some
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.bpy.commands.keymaps import Chord, Keymap
from blender.bpy.commands.operators import Call, Done, Empty, Operator, Outcome
from blender.bpy.data.rna.descriptors import root_in
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions, Registrable
from blender.bpy.faults import BlenderResult
from blender.bpy.render.color.spaces import config_of, viewed
from blender.bpy.render.outputs.exports import combined
from blender.bpy.render.outputs.results import ResultRead
from blender.bpy.settings.attachments import Attached, Attachment
from blender.bpy.ui.drawing.canvas import Align
from blender.bpy.ui.drawing.charts import Chart, Label, Plotted, Points, Rules, Trace, Well
from blender.bpy.ui.drawing.handlers import Refresh, Signal, Watch
from blender.bpy.ui.images.overlays import FrameMask, overlaid, Tint
from blender.bpy.ui.images.views import RESULT
from blender.bpy.ui.layout.rows import Caption, Flow, Member, Property, Row, Rows
from blender.bpy.ui.layout.sections import Single
from blender.bpy.ui.resources.icons import Glyph
from blender.bpy.ui.resources.theme import Color, Metrics, Shipped
from blender.bpy.ui.surfaces.appends import Append
from blender.bpy.ui.surfaces.panels import Joined, Panel
from blender.imaging.analysis.scopes import Bands, Chromaticity, Clipping, Histogram
from blender.imaging.color import observer
from blender.imaging.color.processors import DisplayView
from blender.imaging.files.images import IntegerDepth, Rounded, unassociated
from blender.records.faults import paired
from blender.records.fields import heading

from ..catalog import chain
from ..effects.formation.display import displayed_frame

# --- [MODELS] ---------------------------------------------------------------------------


class Charted(msgspec.Struct, frozen=True, kw_only=True):
    """Render Result chart switches."""

    outline: Annotated[bool, msgspec.Meta(title="Outline", description="Last render's scene-linear luminance outline")] = True
    chromaticity: Annotated[bool, msgspec.Meta(title="Chromaticity", description="Last render's chromaticity with working and display gamuts")] = False


class ClipEnds(msgspec.Struct, frozen=True, kw_only=True):
    """Render Result tints at the written file's code ends."""

    low: Annotated[bool, msgspec.Meta(title="Low Clip", description="Tint pixels at the written file's lowest code")] = False
    high: Annotated[bool, msgspec.Meta(title="High Clip", description="Tint pixels at the written file's highest code")] = False


class Frames(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Last render's scene-linear plane with the view forming it, its display plane, and the written integer depth."""

    linear: npt.NDArray[np.float32]
    view: DisplayView
    formed: npt.NDArray[np.float32]
    depth: Option[IntegerDepth]


# --- [SERVICES] -------------------------------------------------------------------------


class FrameCharts(msgspec.Struct, frozen=True, kw_only=True, dict=True):
    """Last render's charts and code-end tints over Render Result, switched per screen, the tints in the clip looks."""

    low: Glyph = "TRIA_LEFT"
    high: Glyph = "TRIA_RIGHT"
    under: Color = msgspec.field(default_factory=lambda: Shipped.read().look("clip-under").fill)
    over: Color = msgspec.field(default_factory=lambda: Shipped.read().look("clip-over").fill)

    # --- [STATE]

    @cached_property
    def switches(self) -> Attachment[Charted]:
        """Screen chart switches."""
        return Attachment(key="charts", form=Charted, owner=bpy.types.Screen)

    @cached_property
    def ends(self) -> Attachment[ClipEnds]:
        """Screen code-end tints."""
        return Attachment(key="charts_clips", form=ClipEnds, owner=bpy.types.Screen)

    @property
    def prerequisites(self) -> tuple[Registrable, ...]:
        """Switch and tint records the rows and overlays bind."""
        return (self.switches, self.ends)

    # --- [COMMANDS]

    @cached_property
    def commands(self) -> tuple[tuple[Chord, Operator[Empty]], ...]:
        """Code-end tint toggles with their image editor key."""

        def flipped(key: str, description: str, change: Callable[[ClipEnds], ClipEnds]) -> Operator[Empty]:
            def run(call: Call[Empty]) -> BlenderResult[Outcome]:
                return call.root(bpy.types.Screen).bind(lambda screen: self.ends.revised(call.identity, screen, lambda held: Ok(change(held)))).map(lambda _: Done())

            return Operator(key=key, title=heading(key), description=description, undo=False, form=Empty, run=run, condition=Some(RESULT))

        return (
            (Chord(type="O"), flipped("high_clip", "Tint or clear pixels at the written file's highest code", lambda ends: msgspec.structs.replace(ends, high=not ends.high))),
            (Chord(type="U"), flipped("low_clip", "Tint or clear pixels at the written file's lowest code", lambda ends: msgspec.structs.replace(ends, low=not ends.low))),
        )

    # --- [ROWS]

    @property
    def clips(self) -> tuple[Row]:
        """Icon code-end toggles drawn while the editor shows Render Result."""

        def rows(identity: Identity) -> BlenderResult[tuple[Row, ...]]:
            return self.ends.bound(identity).map(
                lambda attached: (
                    Caption(
                        rows=tuple(
                            Property(target=Member(root=bpy.types.Screen, path=f"{attached.name}.{field.name}"), icon=Some(glyph), icon_only=True)
                            for field, glyph in zip(msgspec.structs.fields(ClipEnds), (self.low, self.high))
                        ),
                        flow=Flow.GRID,
                    ),
                )
            )

        return (Rows(rows=rows, condition=Some(RESULT), hidden=True),)

    def section(self) -> Single:
        """Chart switches section, inactive for another image."""

        def rows(identity: Identity) -> BlenderResult[tuple[Row, ...]]:
            return self.switches.bound(identity).map(lambda attached: tuple(Property(target=Member(root=bpy.types.Screen, path=f"{attached.name}.{field.name}")) for field in msgspec.structs.fields(Charted)))

        return Single(key=self.switches.key, title=heading(self.switches.key), rows=(Rows(rows=rows),), condition=Some(RESULT))

    # --- [REGISTRATION]

    def acquisitions(self, identity: Identity) -> BlenderResult[Acquisitions]:
        """Returns the render read, both charts, the code-end masks, and the tint keys."""
        signal = Signal()
        last: Option[Frames] = Nothing

        def consumed(scene: bpy.types.Scene, path: str) -> BlenderResult[Option[str]]:
            settings = scene.render.image_settings
            depth = Option.of_optional(next((held for held in get_args(IntegerDepth.__value__) if not settings.has_linear_colorspace and held == int(settings.color_depth)), None))

            def kept(frames: Frames) -> BlenderResult[Option[str]]:
                nonlocal last
                last = Some(frames)
                return signal.fired().map(lambda _: Nothing)

            def read(plane: npt.NDArray[np.float32], view: Option[DisplayView], config: PyOpenColorIO.Config) -> BlenderResult[Frames]:
                return displayed_frame(identity, scene, plane, PyOpenColorIO.ROLE_SCENE_LINEAR, config).map(
                    lambda file: Frames(linear=plane, view=view.default_with(lambda: viewed(scene)), formed=file.pixels, depth=depth)
                )

            return paired(combined(path).map(lambda found: found[1]), chain.view(identity, scene), config_of()).bind(lambda found: read(*found)).bind(kept)

        def rows(switched: Attached[Charted], clipped: Attached[ClipEnds]) -> tuple[Registrable, ...]:
            refreshes: tuple[Refresh, ...] = (signal, *(Watch(owner=group.struct, identifier=field.name) for group in (switched.group, clipped.group) for field in msgspec.structs.fields(group.record)))

            def charted(on: Callable[[Charted], bool]) -> BlenderResult[Option[Frames]]:
                return root_in(bpy.types.Screen, bpy.context).map(lambda screen: switched.read(screen).map(lambda held: last.filter(lambda _: on(held)))).default_value(Ok(Nothing))

            def tints() -> BlenderResult[Option[tuple[Tint, ...]]]:
                def read(screen: bpy.types.Screen) -> BlenderResult[Option[tuple[Tint, ...]]]:
                    return clipped.read(screen).map(
                        lambda lit: (
                            last
                            .bind(lambda frames: frames.depth.map(lambda depth: Clipping.of(Rounded().coded(unassociated(frames.formed), depth))))
                            .map(lambda clip: tuple(Tint(plane=mask.astype(np.float32), color=ink) for mask, ink, on in ((clip.low, self.under, lit.low), (clip.high, self.over, lit.high)) if on))
                            .filter(bool)
                        )
                    )

                return root_in(bpy.types.Screen, bpy.context).map(read).default_value(Ok(Nothing))

            def plotted(on: Callable[[Charted], bool], chart: Callable[[Metrics, Frames], BlenderResult[Chart]]) -> Plotted[Chart]:
                return Plotted(read=lambda metrics: charted(on).bind(lambda found: found.map(lambda frames: chart(metrics, frames).map(Some)).default_value(Ok(Nothing))))

            keys = Keymap(keymap="Image", bindings=dict(self.commands))
            return (
                ResultRead(consume=consumed),
                overlaid(plotted(lambda held: held.outline, FrameCharts.histogram), RESULT, refreshes),
                overlaid(plotted(lambda held: held.chromaticity, FrameCharts.chromaticity), RESULT, refreshes),
                FrameMask(read=lambda _: tints(), refreshes=refreshes),
                *(command for _, command in self.commands),
                keys,
            )

        return paired(self.switches.bound(identity), self.ends.bound(identity)).map(lambda pair: rows(*pair))


# --- [COMPOSITION] ----------------------------------------------------------------------

charts: Final = FrameCharts()
rows: Final = (charts, Append(target=IMAGE_PT_view_histogram, rows=charts.clips), Panel(section=charts.section(), at=Joined.beside(IMAGE_PT_view_histogram)))

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Charted", "ClipEnds", "FrameCharts", "charts", "rows"]
