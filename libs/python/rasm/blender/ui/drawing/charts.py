"""Native scene-linear scope charts and false-color legends."""

from collections.abc import Callable, Iterable
import math
from typing import ClassVar, Self

import bpy
from expression import Nothing, Ok, Option, Some
import gpu
from gpu_extras.batch import batch_for_shader
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from rasm.blender.data.ids import images
from rasm.blender.faults import BlenderFaults, BlenderImagingFault, BlenderResult
from rasm.blender.ui.drawing.canvas import Align, Canvas, Rect
from rasm.blender.ui.resources.textures import uploaded
from rasm.blender.ui.resources.theme import Color, Metrics
from rasm.imaging.analysis.scopes import Bands, Chromaticity, chromaticity_of, Histogram, histogram_of
from rasm.imaging.chain import GREY
from rasm.imaging.color import observer
from rasm.imaging.color.processors import applied, DisplayView, processor
from rasm.imaging.faults import ImagingFaults, paired

# --- [MODELS] ---------------------------------------------------------------------------


class ChartFrame(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Scene-linear pixels or a native image, with the view forming them."""

    pixels: npt.NDArray[np.float32] | bpy.types.Image
    view: DisplayView

    def plane(self, config: PyOpenColorIO.Config) -> BlenderResult[npt.NDArray[np.float32]]:
        """Returns scene-linear RGB or RGBA, expanding a native scalar pass to RGB.

        Args:
            config: Admitted config owning the image's colorspace and scene-linear role.

        Returns:
            The plane, or the native image read and color processor faults.
        """
        match self.pixels:
            case bpy.types.Image() as image:
                return images.read(image, config).bind(
                    lambda file: (
                        processor(config, PyOpenColorIO.ColorSpaceTransform(src=file.colorspace, dst=PyOpenColorIO.ROLE_SCENE_LINEAR))
                        .map(lambda built: applied(built.getDefaultCPUProcessor(), np.repeat(file.pixels, 3, axis=-1) if file.pixels.shape[-1] == 1 else file.pixels))
                        .map_error(lambda faults: (BlenderImagingFault(faults=faults),))
                    )
                )
            case np.ndarray() as plane:
                return Ok(plane)


class Well(msgspec.Struct, frozen=True, kw_only=True):
    """Blender's stock scope styling and a plot one pixel inside its well."""

    back: ClassVar[Color] = (0.0, 0.0, 0.0, 1.0)
    rim: ClassVar[Color] = (0.0, 0.0, 0.0, 0.5)
    clear: ClassVar[Color] = (0.0, 0.0, 0.0, 0.0)
    grid: ClassVar[Color] = (1.0, 1.0, 1.0, 0.08)
    major: ClassVar[Color] = (1.0, 1.0, 1.0, 0.5)
    trace: ClassVar[Color] = (1.0, 1.0, 1.0, 0.75)
    text: ClassVar[Color] = (1.0, 1.0, 1.0, 0.2)
    radius: ClassVar[float] = 3.0
    stroke: ClassVar[float] = 1.5
    points: ClassVar[float] = 8.0
    margin: ClassVar[float] = 0.4
    width: float
    height: float

    # --- [SIZING]

    @classmethod
    def scope(cls, metrics: Metrics) -> Self:
        """Returns a well at the stock histogram's scaled dimensions."""
        return cls(width=float(metrics.unit * 10), height=float(math.floor(float(metrics.scaled(100.0)) + 0.5)))

    @classmethod
    def gamut(cls, metrics: Metrics) -> Self:
        """Returns a ten-unit well whose plot follows the spectral locus aspect."""
        span = np.ptp(observer.locus(), axis=0)
        width = float(metrics.unit * 10)
        return cls(width=width, height=(width - 2.0) * float(span[1] / span[0]) + 2.0)

    @classmethod
    def inset(cls, metrics: Metrics) -> float:
        """Returns the well's margin from the visible region's edges."""
        return metrics.unit * cls.margin

    @classmethod
    def label(cls, metrics: Metrics) -> float:
        """Returns the stock scope label size in device pixels."""
        return float(metrics.scaled(cls.points))

    @property
    def plot(self) -> Rect:
        """Plot rectangle one pixel inside the well."""
        return Rect(x=1.0, y=1.0, width=self.width - 2.0, height=self.height - 2.0)

    # --- [DRAWING]

    def under(self, canvas: Canvas) -> None:
        """Draws the stock scope's rounded black background."""
        canvas.frame(Rect(x=0.0, y=0.0, width=self.width, height=self.height), self.back, self.clear, self.radius)

    def over(self, canvas: Canvas) -> None:
        """Draws the stock scope's closing outline above its lower edge."""
        canvas.frame(Rect(x=0.0, y=1.0, width=self.width, height=self.height - 1.0), self.clear, self.rim, self.radius)


class TracePlot(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Cached line traces, guides, ticks, and captions in a shared chart well."""

    well: Well
    grid: gpu.types.GPUBatch
    majors: gpu.types.GPUBatch
    ticks: Option[gpu.types.GPUBatch]
    traces: tuple[tuple[gpu.types.GPUBatch, Color], ...]
    labels: tuple[tuple[str, float, float, Align], ...]
    caption: str

    @classmethod
    def of(
        cls,
        metrics: Metrics,
        *,
        traces: tuple[tuple[npt.NDArray[np.float32], Color], ...],
        verticals: tuple[float, ...],
        horizontals: tuple[float, ...],
        labels: tuple[tuple[str, float, float, Align], ...],
        caption: str,
        ticks: tuple[float, ...] = (),
        majors: tuple[float, ...] = (),
    ) -> Self:
        """Builds chart geometry from normalized positions and caller-owned axis labels."""
        well, shader = Well.scope(metrics), gpu.shader.from_builtin("POLYLINE_UNIFORM_COLOR")
        plot = well.plot
        right, top = plot.x + plot.width, plot.y + plot.height
        bounds = tuple(plot.x + position * plot.width for position in verticals)
        rows = tuple(plot.y + position * plot.height for position in horizontals)

        def lines(points: Iterable[tuple[float, float]]) -> gpu.types.GPUBatch:
            """Builds the line pairs used by this plot's grid and ticks."""
            return batch_for_shader(shader, "LINES", {"pos": tuple(points)})

        return cls(
            well=well,
            grid=lines((*(point for x in bounds for point in ((x, plot.y), (x, top))), *(point for y in rows for point in ((plot.x, y), (right, y))))),
            majors=lines((
                *(point for position in majors for point in ((plot.x + position * plot.width, plot.y), (plot.x + position * plot.width, top))),
                (right, plot.y),
                (right, top),
                (plot.x, top),
                (right, top),
            )),
            ticks=Some(ticks)
            .filter(bool)
            .map(lambda positions: lines(point for position in positions for point in ((plot.x + position * plot.width, plot.y), (plot.x + position * plot.width, plot.y + metrics.unit * 0.25)))),
            traces=tuple(
                (batch_for_shader(shader, "LINE_STRIP", {"pos": (positions * (plot.width, plot.height) + (plot.x, plot.y)).astype(np.float32)}), color)  # type: ignore[dict-item]  # ty: ignore[invalid-argument-type]
                for positions, color in traces
            ),
            labels=tuple((text, plot.x + x * plot.width, plot.y + y * plot.height, align) for text, x, y, align in labels),
            caption=caption,
        )


class Outline(msgspec.Struct, frozen=True, kw_only=True):
    """Read-only line chart with cached preparation and shared drawing policy."""

    read: Callable[[Metrics], BlenderResult[Option[TracePlot]]]

    def drawn(self, canvas: Canvas, geometry: TracePlot) -> None:
        """Draws the outline at the visible extent's lower-left corner."""
        inset, points, plot = Well.inset(canvas.metrics), Well.label(canvas.metrics), geometry.well.plot
        x, y = canvas.extent.x + inset, canvas.extent.y + inset
        with gpu.matrix.push_pop():  # type: ignore[attr-defined]  # ty: ignore[invalid-context-manager]
            gpu.matrix.translate((x, y))
            geometry.well.under(canvas)
            shader = canvas.lines(1.0)
            for batch, color in ((geometry.grid, Well.grid), (geometry.majors, Well.major), *((ticks, Well.major) for ticks in geometry.ticks.to_list())):
                shader.uniform_float("color", color)
                batch.draw(shader)
            with canvas.clipped(Rect(x=x + plot.x, y=y + plot.y, width=plot.width, height=plot.height)):
                gpu.state.blend_set("ADDITIVE")
                stroke = canvas.lines(Well.stroke)
                for batch, color in geometry.traces:
                    stroke.uniform_float("color", color)
                    batch.draw(stroke)
                gpu.state.blend_set("ALPHA")
            for text, at_x, at_y, align in geometry.labels:
                canvas.label(text, (at_x + points / 2 * (1 - 2 * align.value), at_y + points / 2), Well.text, points, align)
            canvas.label(geometry.caption, (plot.x + points / 2, plot.y + plot.height - points * 1.5), Well.text, points)
            geometry.well.over(canvas)


class GamutPlot(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Cached chromaticity batches and gamut captions at one interface scale."""

    well: Well
    grid: gpu.types.GPUBatch
    locus: gpu.types.GPUBatch
    ticks: gpu.types.GPUBatch
    working: gpu.types.GPUBatch
    display: Option[gpu.types.GPUBatch]
    trace: Option[gpu.types.GPUBatch]
    lines: tuple[tuple[str, Color], ...]

    @classmethod
    def of(cls, metrics: Metrics, reading: Chromaticity, space: str, display_space: Option[PyOpenColorIO.ColorSpace]) -> Self:
        """Builds locus, density, gamut edges, white-point marks, and captions."""
        counts = reading.trace
        working = space, reading.working
        display = reading.display.map2(lambda rows, native: (native.getName(), rows), display_space)
        well, locus = Well.gamut(metrics), observer.locus()
        low, span = locus.min(axis=0), np.ptp(locus, axis=0)
        plot, side = well.plot, float(metrics.scaled(4.0)) / 2.0
        line_shader, point_shader = gpu.shader.from_builtin("POLYLINE_UNIFORM_COLOR"), gpu.shader.from_builtin("POINT_FLAT_COLOR")
        tenths = tuple(np.arange(np.ceil(low[axis] * 10), np.floor((low[axis] + span[axis]) * 10) + 1) / 10 for axis in (0, 1))
        occupied = np.argwhere(counts > 0)
        tangent = np.gradient(locus, axis=0)
        outward = np.column_stack((-tangent[:, 1], tangent[:, 0]))
        length = np.linalg.norm(outward, axis=1, keepdims=True)
        normal = np.divide(outward, length, out=np.zeros_like(outward), where=length > 0.0)
        tenth = np.flatnonzero(observer.table()[:, 0] % 10 == 0)

        def placed(xy: npt.ArrayLike) -> npt.NDArray[np.float32]:
            """Maps each chromaticity axis through its own bounds and plot dimension."""
            return ((np.asarray(xy) - low) / span * (plot.width, plot.height) + (plot.x, plot.y)).astype(np.float32)

        def gamut(rows: npt.NDArray[np.float64]) -> gpu.types.GPUBatch:
            """Builds one gamut triangle and a device-sized square at its white point."""
            square = np.array(((-1, -1), (1, -1), (1, -1), (1, 1), (1, 1), (-1, 1), (-1, 1), (-1, -1)), np.float32) * side
            return batch_for_shader(line_shader, "LINES", {"pos": np.vstack((placed(rows[[0, 1, 1, 2, 2, 0]]), placed(rows[3]) + square))})  # type: ignore[dict-item]  # ty: ignore[invalid-argument-type]

        def block(named: tuple[str, npt.NDArray[np.float64]], color: Color) -> tuple[tuple[str, Color], ...]:
            """Formats a gamut's name and RGBW coordinates together."""
            name, rows = named
            return ((name, color), *((f"{letter} {x:.4f} {y:.4f}", color) for letter, x, y in zip("RGBW", rows[:, 0], rows[:, 1])))

        def traced(found: npt.NDArray[np.intp]) -> gpu.types.GPUBatch:
            """Builds occupied cell centers with logarithmic peak-normalized opacity."""
            alpha = Well.trace[3] * np.log1p(counts[tuple(found.T)]) / np.log1p(counts.max())
            positions = ((found + 0.5) / counts.shape * (plot.width, plot.height) + (plot.x, plot.y)).astype(np.float32)
            return batch_for_shader(point_shader, "POINTS", {"pos": positions, "color": np.column_stack((np.ones((len(found), 3)), alpha)).astype(np.float32)})  # type: ignore[dict-item]  # ty: ignore[invalid-argument-type]

        grid = (*(point for x in tenths[0] for point in ((x, low[1]), (x, low[1] + span[1]))), *(point for y in tenths[1] for point in ((low[0], y), (low[0] + span[0], y))))
        return cls(
            well=well,
            grid=batch_for_shader(line_shader, "LINES", {"pos": placed(grid)}),  # type: ignore[dict-item]  # ty: ignore[invalid-argument-type]
            locus=batch_for_shader(line_shader, "LINE_STRIP", {"pos": placed(np.vstack((locus, locus[:1])))}),  # type: ignore[dict-item]  # ty: ignore[invalid-argument-type]
            ticks=batch_for_shader(line_shader, "LINES", {"pos": placed(np.stack((locus[tenth], locus[tenth] + 0.01 * normal[tenth]), axis=1).reshape(-1, 2))}),  # type: ignore[dict-item]  # ty: ignore[invalid-argument-type]
            working=gamut(reading.working),
            display=reading.display.map2(lambda rows, _space: gamut(rows), display_space),
            trace=Some(occupied).filter(lambda found: found.size > 0).map(traced),
            lines=(*block(working, Well.trace), *(line for named in display.to_list() for line in block(named, Well.major))),
        )


class Gamuts(msgspec.Struct, frozen=True, kw_only=True):
    """Chromaticity density with the frame's working and display gamuts."""

    source: Callable[[], BlenderResult[Option[ChartFrame]]]

    def read(self, metrics: Metrics) -> BlenderResult[Option[GamutPlot]]:
        """Reads chromaticities at the plot's cell dimensions and retains native faults."""
        config, plot = PyOpenColorIO.GetCurrentConfig(), Well.gamut(metrics).plot

        def charted(frame: ChartFrame) -> BlenderResult[GamutPlot]:
            """Builds the density and gamut geometry from the scene-linear frame."""
            return (
                frame
                .plane(config)
                .bind(lambda plane: chromaticity_of(config, plane, frame.view, (round(plot.width), round(plot.height))).map_error(lambda faults: (BlenderImagingFault(faults=faults),)))
                .map(lambda reading: GamutPlot.of(metrics, reading, config.getColorSpace(PyOpenColorIO.ROLE_SCENE_LINEAR).getName(), frame.view.colorspace(config)))
            )

        return self.source().bind(lambda frame: frame.map(lambda held: charted(held).map(Some)).default_value(Ok(Nothing)))

    def drawn(self, canvas: Canvas, geometry: GamutPlot) -> None:
        """Draws the diagram within its well at the visible extent's lower right."""
        inset, points, plot = Well.inset(canvas.metrics), Well.label(canvas.metrics), geometry.well.plot
        x, y = canvas.extent.x + canvas.extent.width - inset - geometry.well.width, canvas.extent.y + inset
        pitch = points + canvas.metrics.spacing.button_y
        with gpu.matrix.push_pop():  # type: ignore[attr-defined]  # ty: ignore[invalid-context-manager]
            gpu.matrix.translate((x, y))
            geometry.well.under(canvas)
            with canvas.clipped(Rect(x=x + plot.x, y=y + plot.y, width=plot.width, height=plot.height)):
                shader = canvas.lines(1.0)
                for batch, color in (
                    (geometry.grid, Well.grid),
                    (geometry.locus, Well.major),
                    (geometry.ticks, Well.major),
                    *((display, Well.major) for display in geometry.display.to_list()),
                    (geometry.working, Well.trace),
                ):
                    shader.uniform_float("color", color)
                    batch.draw(shader)
                gpu.state.blend_set("ADDITIVE")
                for trace in geometry.trace.to_list():
                    trace.draw(canvas.points())
                gpu.state.blend_set("ALPHA")
                for index, (text, color) in enumerate(geometry.lines, 1):
                    canvas.label(text, (plot.x + plot.width - points / 2, plot.y + plot.height - points / 2 - index * pitch), color, points, Align.END)
            geometry.well.over(canvas)


class KeyPlot(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """False-color legend with a native band texture and exposure labels."""

    well: Well
    bar: Rect
    texture: gpu.types.GPUTexture
    labels: tuple[tuple[str, float, float], ...]

    @staticmethod
    def of(metrics: Metrics, bands: Bands) -> "BlenderResult[KeyPlot]":
        """Builds a nearest-filtered key, retaining native texture allocation faults."""
        points, pad, width = Well.label(metrics), float(metrics.spacing.template), float(metrics.unit * 20)
        stops = np.log2(bands.edges / GREY)
        low, high = np.min(stops, initial=0.0) - 1.0, np.max(stops, initial=0.0) + 1.0
        bar = Rect(x=pad, y=2 * pad + points, width=width - 2 * pad, height=float(metrics.unit // 2))
        columns = round(bar.width)
        values = GREY * np.exp2(low + (np.arange(columns) + 0.5) / columns * (high - low))
        pixels = np.concatenate((bands.colors[np.searchsorted(bands.edges, values)], np.ones((columns, 1), np.float32)), axis=-1)[None]

        def keyed(texture: gpu.types.GPUTexture) -> KeyPlot:
            texture.filter_mode(False)
            return KeyPlot(
                well=Well(width=width, height=bar.y + bar.height + 2 * pad + points),
                bar=bar,
                texture=texture,
                labels=tuple((f"{stop:+.1f}", bar.x + (stop - low) / (high - low) * bar.width, pad if index % 2 == 0 else bar.y + bar.height + pad) for index, stop in enumerate(stops)),
            )

        return uploaded(pixels).map(keyed)


class FalseColor(msgspec.Struct, frozen=True, kw_only=True):
    """View's false-color bands labeled in stops from middle grey."""

    source: Callable[[], BlenderResult[Option[DisplayView]]]

    def read(self, metrics: Metrics) -> BlenderResult[Option[KeyPlot]]:
        """Forms the default-display key and retains view or GPU allocation faults."""
        config = PyOpenColorIO.GetCurrentConfig()

        def keyed(view: DisplayView) -> BlenderResult[Option[KeyPlot]]:
            """Builds the key only when the selected view has constant bands."""

            def imaged(faults: ImagingFaults) -> BlenderFaults:
                return (BlenderImagingFault(faults=faults),)

            return (
                Bands
                .of(config, msgspec.structs.replace(view, display=config.getDefaultDisplay()))
                .map_error(imaged)
                .bind(lambda bands: bands.map(lambda held: KeyPlot.of(metrics, held).map(Some)).default_value(Ok(Nothing)))
            )

        return self.source().bind(lambda view: view.map(keyed).default_value(Ok(Nothing)))

    def drawn(self, canvas: Canvas, geometry: KeyPlot) -> None:
        """Draws the key inside the visible extent's upper-left corner."""
        inset, points = Well.inset(canvas.metrics), Well.label(canvas.metrics)
        with gpu.matrix.push_pop():  # type: ignore[attr-defined]  # ty: ignore[invalid-context-manager]
            gpu.matrix.translate((canvas.extent.x + inset, canvas.extent.y + canvas.extent.height - inset - geometry.well.height))
            geometry.well.under(canvas)
            canvas.picture(geometry.texture, geometry.bar, gpu.shader.from_builtin("IMAGE_COLOR"))
            for text, x, y in geometry.labels:
                canvas.label(text, (x, y), Well.trace, points, Align.CENTER)
            geometry.well.over(canvas)


# --- [OPERATIONS] -----------------------------------------------------------------------


def histogram(source: Callable[[], BlenderResult[Option[ChartFrame]]], metrics: Metrics) -> BlenderResult[Option[TracePlot]]:
    """Returns the scene-linear histogram with its view's exposure-band ticks."""
    config = PyOpenColorIO.GetCurrentConfig()

    def traced(frame: ChartFrame) -> BlenderResult[TracePlot]:
        def plotted(reading: tuple[Histogram, Option[Bands]]) -> TracePlot:
            histogram, bands = reading
            counts = histogram.counts
            bins, peak = counts.size, counts.max()
            bounds = tuple(Histogram.resolution * stop / bins for stop in range(1, Histogram.headroom + 1))
            heights = np.divide(counts, peak, out=np.zeros(bins), where=peak > 0)
            return TracePlot.of(
                metrics,
                traces=((np.column_stack((np.arange(bins) / bins, heights)).astype(np.float32), Well.trace),),
                verticals=bounds[1:],
                horizontals=(0.25, 0.5, 0.75),
                majors=bounds[:1],
                ticks=tuple(position / bins for reading in bands.to_list() for position in np.searchsorted(Histogram.edges(), reading.edges, side="right")),
                labels=(*((f"{2 ** (stop - 1):g}", x, 0.0, Align.START) for stop, x in enumerate(bounds, 1)), (f"{2**Histogram.headroom:g}", 1.0, 0.0, Align.END)),
                caption=config.getColorSpace(PyOpenColorIO.ROLE_SCENE_LINEAR).getName(),
            )

        return paired(
            frame.plane(config).bind(lambda plane: histogram_of(config, plane).map_error(lambda faults: (BlenderImagingFault(faults=faults),))),
            Bands.of(config, frame.view).map_error(lambda faults: (BlenderImagingFault(faults=faults),)),
        ).map(plotted)

    return source().bind(lambda frame: frame.map(lambda held: traced(held).map(Some)).default_value(Ok(Nothing)))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["ChartFrame", "FalseColor", "GamutPlot", "Gamuts", "KeyPlot", "Outline", "TracePlot", "Well", "histogram"]
