"""Darkroom render charts over Render Result: the last render's scene-linear luminance histogram with its view's exposure bands, its chromaticity with working and display gamuts, and tints at the written file's code ends."""

from collections.abc import Callable, Iterable
from typing import ClassVar, Final, get_args

from bl_ui.space_image import IMAGE_PT_view_histogram
import bpy
from expression import Nothing, Ok, Option, Some
from expression.collections import Block
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.bpy.commands.keymaps import Chord, Keymap, Toggle
from blender.bpy.data.rna.descriptors import root_in
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions, Registrable
from blender.bpy.render.color.spaces import viewed
from blender.bpy.render.outputs.exports import combined
from blender.bpy.render.outputs.results import ResultRead
from blender.bpy.settings.attachments import annotated, Attached, Attachment
from blender.bpy.ui.drawing.canvas import Align
from blender.bpy.ui.drawing.charts import Chart, Label, Paint, Plotted, Points, Rules, Trace, Well
from blender.bpy.ui.drawing.handlers import Refresh, Signal, Watch
from blender.bpy.ui.images.overlays import FrameMask, overlaid, Tint
from blender.bpy.ui.images.views import RESULT
from blender.bpy.ui.layout.rows import Caption, Flow, Member, Property, Row, Rows
from blender.bpy.ui.layout.sections import Single
from blender.bpy.ui.resources.icons import Glyph
from blender.bpy.ui.resources.theme import Color, Metrics
from blender.bpy.ui.surfaces.appends import Append
from blender.bpy.ui.surfaces.panels import Joined, Panel
from blender.imaging.analysis.scopes import Bands, Chromaticity, Clipping, Histogram
from blender.imaging.color import observer
from blender.imaging.color.processors import DisplayView
from blender.imaging.files.buffers import unassociated
from blender.imaging.files.coding import IntegerDepth, Rounded
from blender.records.faults import Outcome, paired

from ..catalog import chain
from ..effects.formation.display import displayed_frame

# --- [MODELS] ---------------------------------------------------------------------------


class Charted(bpy.types.PropertyGroup):
    """Render Result chart switches."""

    outline: bpy.props.BoolProperty(name="Outline", description="Last render's scene-linear luminance outline", default=True)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    chromaticity: bpy.props.BoolProperty(name="Chromaticity", description="Last render's chromaticity with working and display gamuts")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class ClipEnds(bpy.types.PropertyGroup):
    """Render Result tints at the written file's code ends."""

    low: bpy.props.BoolProperty(name="Low Clip", description="Tint pixels at the written file's lowest code")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    high: bpy.props.BoolProperty(name="High Clip", description="Tint pixels at the written file's highest code")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class Frames(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Last render's scene-linear plane with the view forming it, its display plane, and the written integer depth."""

    linear: npt.NDArray[np.float32]
    view: DisplayView
    formed: npt.NDArray[np.float32]
    depth: Option[IntegerDepth]


# --- [SERVICES] -------------------------------------------------------------------------


class FrameCharts(msgspec.Struct, frozen=True, kw_only=True, dict=True):
    """Last render's charts and code-end tints over Render Result, switched per screen, low codes tinted blue and high codes red."""

    low: Glyph = "TRIA_LEFT"
    high: Glyph = "TRIA_RIGHT"
    under: Color = (0.0, 0.0, 1.0, 1.0)
    over: Color = (1.0, 0.0, 0.0, 1.0)

    switches: ClassVar[Attachment[Charted]] = Attachment(key="charts", form=Charted, owner=bpy.types.Screen)
    ends: ClassVar[Attachment[ClipEnds]] = Attachment(key="charts_clips", form=ClipEnds, owner=bpy.types.Screen)
    prerequisites: ClassVar[tuple[Registrable, ...]] = (switches, ends)

    # --- [ROWS]

    @property
    def clips(self) -> tuple[Row]:
        """Icon code-end toggles drawn while the editor shows Render Result."""

        def rows(identity: Identity) -> Outcome[tuple[Row, ...]]:
            return self.ends.bound(identity).map(
                lambda attached: (
                    Caption(
                        rows=tuple(
                            Property(target=Member(root=bpy.types.Screen, path=(attached.name, name)), icon=Some(glyph), icon_only=True)
                            for name, glyph in zip(annotated(ClipEnds), (self.low, self.high))
                        ),
                        flow=Flow.GRID,
                    ),
                )
            )

        return (Rows(rows=rows, condition=Some(RESULT), hidden=True),)

    def section(self) -> Single:
        """Chart switches section, inactive for another image."""

        def rows(identity: Identity) -> Outcome[tuple[Row, ...]]:
            return self.switches.bound(identity).map(lambda attached: tuple(Property(target=Member(root=bpy.types.Screen, path=(attached.name, name))) for name in annotated(Charted)))

        return Single(key=self.switches.key, title=bpy.path.display_name(self.switches.key), rows=(Rows(rows=rows),), condition=Some(RESULT))

    # --- [CHARTS]

    @staticmethod
    def histogram(metrics: Metrics, frames: Frames) -> Outcome[Chart]:
        """Charts scene-linear luminance counts, linear to one then four log stops, with the view's exposure bands ticked along the floor."""
        well = Well.of(metrics, 0.5)

        def plotted(config: PyOpenColorIO.Config, histogram: Histogram, bands: Option[Bands]) -> Chart:
            counts = histogram.counts
            bins, peak = counts.size, counts.max()
            bounds = np.arange(1, Histogram.headroom + 1) * Histogram.resolution / bins
            return Chart.of(
                metrics,
                well=well,
                layers=(
                    Rules(segments=_ruled(bounds[1:], (0.25, 0.5, 0.75)), paint=Well.fine),
                    Rules(segments=_ruled((bounds[0], 1.0), (1.0,)), paint=Well.major),
                    *(
                        Rules(segments=_ruled(np.searchsorted(Histogram.edges(), held.edges, side="right") / bins, reach=metrics.unit * 0.25 / well.plot.height), paint=Well.major)
                        for held in bands.to_list()
                    ),
                    Trace(points=np.column_stack((np.arange(bins) / bins, np.divide(counts, peak, out=np.zeros(bins), where=peak > 0))).astype(np.float32)),
                ),
                labels=(*(Label(text=f"{2**stop:g}", x=float(x), y=0.0) for stop, x in enumerate(bounds)), Label(text=f"{2**Histogram.headroom:g}", x=1.0, y=0.0, align=Align.END)),
                caption=(config.getColorSpace(PyOpenColorIO.ROLE_SCENE_LINEAR).getName(),),
            )

        config = PyOpenColorIO.GetCurrentConfig()
        return paired(Histogram.of(config, frames.linear), Bands.of(config, frames.view)).map(lambda found: plotted(config, *found))

    @staticmethod
    def chromaticity(metrics: Metrics, frames: Frames) -> Outcome[Chart]:
        """Charts scene-linear xy density inside the spectral locus, ticked every ten nanometres, with the working and display gamuts and their white points boxed."""
        extent, tenths = observer.frame()
        well = Well.of(metrics, float(extent[1] / extent[0]))
        plot, locus = well.plot, observer.locus()
        outward = np.gradient(locus, axis=0)[:, ::-1] * (-1.0, 1.0)
        normal = np.divide(outward, length := np.linalg.norm(outward, axis=1, keepdims=True), out=np.zeros_like(outward), where=length > 0.0)
        tenth = np.flatnonzero(observer.table()[:, 0] % 10 == 0)
        square = np.array(((-1, -1), (1, -1), (1, -1), (1, 1), (1, 1), (-1, 1), (-1, 1), (-1, -1)), np.float64) * float(metrics.scaled(2.0)) / (plot.width, plot.height)

        def gamut(rows: npt.NDArray[np.float64], paint: Paint) -> Rules:
            return Rules(segments=np.vstack((rows[[0, 1, 1, 2, 2, 0]] / extent, rows[3] / extent + square)).astype(np.float32), paint=paint)

        def block(name: str, rows: npt.NDArray[np.float64]) -> tuple[str, ...]:
            return (name, *(f"{letter} {x:.4f} {y:.4f}" for letter, x, y in zip("RGBW", rows[:, 0], rows[:, 1], strict=True)))

        def plotted(config: PyOpenColorIO.Config, reading: Chromaticity) -> Chart:
            counts, found = reading.trace, np.argwhere(reading.trace > 0)
            display = reading.display.map2(lambda rows, space: (space.getName(), rows), frames.view.colorspace(config))
            return Chart.of(
                metrics,
                well=well,
                layers=(
                    Rules(segments=_ruled(np.linspace(0.0, 1.0, tenths[0] + 1), np.linspace(0.0, 1.0, tenths[1] + 1))),
                    Trace(points=(np.vstack((locus, locus[:1])) / extent).astype(np.float32), paint=Well.major),
                    Rules(segments=(np.stack((locus[tenth], locus[tenth] + 0.01 * normal[tenth]), axis=1).reshape(-1, 2) / extent).astype(np.float32), paint=Well.major),
                    *(gamut(rows, Well.major) for _, rows in display.to_list()),
                    gamut(reading.working, Well.trace),
                    *(
                        (Points(points=((found + 0.5) / counts.shape).astype(np.float32), weights=(np.log1p(counts[tuple(found.T)]) / np.log1p(counts.max())).astype(np.float32)),)
                        if found.size
                        else ()
                    ),
                ),
                caption=(*block(config.getColorSpace(PyOpenColorIO.ROLE_SCENE_LINEAR).getName(), reading.working), *(line for named in display.to_list() for line in block(*named))),
                corner=Align.END,
            )

        config = PyOpenColorIO.GetCurrentConfig()
        return Chromaticity.of(config, frames.linear, frames.view, (round(plot.width), round(plot.height)), extent).map(lambda reading: plotted(config, reading))

    # --- [REGISTRATION]

    def acquisitions(self, identity: Identity) -> Outcome[Acquisitions]:
        """Returns the render read, both charts, the code-end masks, and the tint keys."""
        signal = Signal()
        last: Option[Frames] = Nothing

        def consumed(scene: bpy.types.Scene, path: str) -> Outcome[Option[str]]:
            settings = scene.render.image_settings
            depth = Block.of_seq(get_args(IntegerDepth.__value__)).filter(lambda held: not settings.has_linear_colorspace and held == int(settings.color_depth)).try_head()

            def kept(frames: Frames) -> Outcome[Option[str]]:
                nonlocal last
                last = Some(frames)
                return signal.fired().map(lambda _: Nothing)

            def read(plane: npt.NDArray[np.float32], view: Option[DisplayView], config: PyOpenColorIO.Config) -> Outcome[Frames]:
                return displayed_frame(identity, scene, plane, PyOpenColorIO.ROLE_SCENE_LINEAR, config).map(
                    lambda file: Frames(linear=plane, view=view.default_with(lambda: viewed(scene)), formed=file.pixels, depth=depth)
                )

            return paired(combined(path).map(lambda found: found[1]), chain.view(identity, scene)).bind(lambda found: read(*found, PyOpenColorIO.GetCurrentConfig())).bind(kept)

        def rows(switched: Attached[Charted], clipped: Attached[ClipEnds]) -> tuple[Registrable, ...]:
            refreshes: tuple[Refresh, ...] = (signal, *(Watch(owner=form, identifier=name) for form in (Charted, ClipEnds) for name in annotated(form)))

            def tints(screen: bpy.types.Screen) -> Option[tuple[Tint, ...]]:
                lit = clipped.struct(screen)
                return (
                    last
                    .bind(lambda frames: frames.depth.map(lambda depth: Clipping.of(Rounded.coded(unassociated(frames.formed), depth))))
                    .map(lambda clip: tuple(Tint(plane=mask.astype(np.float32), color=rgba) for mask, rgba, on in ((clip.low, self.under, lit.low), (clip.high, self.over, lit.high)) if on))
                    .filter(bool)
                )

            def plotted(on: Callable[[Charted], bool], chart: Callable[[Metrics, Frames], Outcome[Chart]]) -> Plotted[Chart]:
                return Plotted(
                    read=lambda metrics: (
                        root_in(bpy.types.Screen, bpy.context)
                        .bind(lambda screen: last.filter(lambda _: on(switched.struct(screen))))
                        .map(lambda frames: chart(metrics, frames).map(Some))
                        .default_value(Ok(Nothing))
                    )
                )

            keys = Keymap(keymap="Image", bindings={chord: Toggle(path=("screen", clipped.name, name)) for chord, name in ((Chord(type="O"), "high"), (Chord(type="U"), "low"))})
            return (
                ResultRead(consume=consumed),
                overlaid(plotted(lambda held: held.outline, FrameCharts.histogram), RESULT, refreshes),
                overlaid(plotted(lambda held: held.chromaticity, FrameCharts.chromaticity), RESULT, refreshes),
                FrameMask(read=lambda _: Ok(root_in(bpy.types.Screen, bpy.context).bind(tints)), refreshes=refreshes),
                keys,
            )

        return paired(self.switches.bound(identity), self.ends.bound(identity)).map(lambda pair: rows(*pair))


# --- [OPERATIONS] -----------------------------------------------------------------------


def _ruled(verticals: Iterable[float], horizontals: Iterable[float] = (), reach: float = 1.0) -> npt.NDArray[np.float32]:
    """Returns plot-fraction segments rising from the floor to `reach` at each vertical and spanning the plot at each horizontal."""
    return np.array([*((x, 0.0, x, reach) for x in verticals), *((0.0, y, 1.0, y) for y in horizontals)], np.float32).reshape(-1, 2)


# --- [COMPOSITION] ----------------------------------------------------------------------

charts: Final = FrameCharts()
rows: Final = (charts, Append(target=IMAGE_PT_view_histogram, rows=charts.clips), Panel(section=charts.section(), at=Joined.beside(IMAGE_PT_view_histogram)))

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Charted", "ClipEnds", "FrameCharts", "charts", "rows"]
