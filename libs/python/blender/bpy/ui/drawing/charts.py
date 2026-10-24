"""Scene-linear scope charts and false-color legends."""

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

from blender.bpy.data.ids import images
from blender.bpy.faults import BlenderImagingFault, BlenderResult
from blender.bpy.ui.drawing.canvas import Align, Canvas, Rect
from blender.bpy.ui.resources.textures import uploaded
from blender.bpy.ui.resources.theme import CLEAR, Color, Metrics, Shipped
from blender.imaging.analysis.scopes import Bands, Chromaticity, chromaticity_of, Histogram, histogram_of
from blender.imaging.chain import GREY
from blender.imaging.color import observer
from blender.imaging.color.processors import applied, DisplayView, processor
from blender.imaging.faults import paired

# --- [MODELS] ---------------------------------------------------------------------------


class ChartFrame(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Scene-linear pixels or image under a view."""

    pixels: npt.NDArray[np.float32] | bpy.types.Image
    view: DisplayView

    def plane(self, config: PyOpenColorIO.Config) -> BlenderResult[npt.NDArray[np.float32]]:
        """Returns scene-linear RGB or RGBA, scalar passes as RGB.

        Args:
            config: OCIO config with image colorspace and scene-linear role.

        Returns:
            Scene-linear plane, or image read and processor faults.
        """
        match self.pixels:
            case bpy.types.Image() as image:
                return images.read(image, config).bind(
                    lambda file: (
                        processor(config, PyOpenColorIO.ColorSpaceTransform(src=file.colorspace, dst=PyOpenColorIO.ROLE_SCENE_LINEAR))
                        .map(lambda built: applied(built.getDefaultCPUProcessor(), np.repeat(file.pixels, 3, axis=-1) if file.pixels.shape[-1] == 1 else file.pixels))
                        .map_error(BlenderImagingFault.lifted)
                    )
                )
            case np.ndarray() as plane:
                return Ok(plane)


class Well(msgspec.Struct, frozen=True, kw_only=True):
    """Black chart well of the stock scopes, its inks read from the shared well looks."""

    ground: ClassVar[Color] = Shipped.read().look("well").color("fill")
    rim: ClassVar[Color] = Shipped.read().look("well").color("edge")
    fine: ClassVar[Color] = Shipped.read().look("grid-fine").color("edge")
    grid: ClassVar[Color] = Shipped.read().look("grid").color("edge")
    major: ClassVar[Color] = Shipped.read().look("major").color("edge")
    trace: ClassVar[Color] = Shipped.read().look("trace").color("edge")
    label: ClassVar[Color] = Shipped.read().look("label").color("text")
    width: float
    height: float

    # --- [SIZING]

    @classmethod
    def scope(cls, metrics: Metrics) -> Self:
        """Returns well at stock histogram size."""
        return cls(width=float(metrics.unit * 10), height=float(math.floor(float(metrics.scaled(100.0)) + 0.5)))

    @classmethod
    def gamut(cls, metrics: Metrics) -> Self:
        """Returns ten-unit well whose plot takes the chromaticity frame aspect."""
        extent, _ = observer.frame()
        square = cls(width=float(metrics.unit * 10), height=float(metrics.unit * 10))
        return msgspec.structs.replace(square, height=square.plot.width * float(extent[1] / extent[0]) + square.height - square.plot.height)

    @classmethod
    def inset(cls, metrics: Metrics) -> float:
        """Returns well margin from region edges."""
        return metrics.sized("well", "inset")

    @classmethod
    def font(cls, metrics: Metrics) -> float:
        """Returns label font size in device pixels."""
        return metrics.sized("label", "type")

    @property
    def plot(self) -> Rect:
        """Plot rectangle one pixel inside the well."""
        return Rect(x=1.0, y=1.0, width=self.width - 2.0, height=self.height - 2.0)

    # --- [DRAWING]

    def under(self, canvas: Canvas) -> None:
        """Draws rounded black background."""
        canvas.frame(Rect(x=0.0, y=0.0, width=self.width, height=self.height), self.ground, CLEAR, canvas.metrics.sized("well", "radius"))

    def over(self, canvas: Canvas) -> None:
        """Draws outline above lower edge."""
        canvas.frame(Rect(x=0.0, y=1.0, width=self.width, height=self.height - 1.0), CLEAR, self.rim, canvas.metrics.sized("well", "radius"))


class Grid(msgspec.Struct, frozen=True, kw_only=True):
    """Normalized grid line positions per axis and their ink."""

    ink: Color
    verticals: tuple[float, ...] = ()
    horizontals: tuple[float, ...] = ()


class TracePlot(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Cached line chart geometry in a well."""

    well: Well
    grid: gpu.types.GPUBatch
    rule: Color
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
        grid: Grid,
        labels: tuple[tuple[str, float, float, Align], ...],
        caption: str,
        ticks: tuple[float, ...] = (),
        majors: tuple[float, ...] = (),
    ) -> Self:
        """Builds chart geometry from normalized positions."""
        well, shader = Well.scope(metrics), gpu.shader.from_builtin("POLYLINE_UNIFORM_COLOR")
        plot = well.plot
        right, top = plot.x + plot.width, plot.y + plot.height
        bounds = tuple(plot.x + position * plot.width for position in grid.verticals)
        rows = tuple(plot.y + position * plot.height for position in grid.horizontals)

        def lines(points: Iterable[tuple[float, float]]) -> gpu.types.GPUBatch:
            """Builds grid and tick line pairs."""
            return batch_for_shader(shader, "LINES", {"pos": tuple(points)})

        return cls(
            well=well,
            grid=lines((*(point for x in bounds for point in ((x, plot.y), (x, top))), *(point for y in rows for point in ((plot.x, y), (right, y))))),
            rule=grid.ink,
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
    """Read-only line chart over cached trace geometry."""

    read: Callable[[Metrics], BlenderResult[Option[TracePlot]]]

    def drawn(self, canvas: Canvas, geometry: TracePlot) -> None:
        """Draws chart at lower-left corner."""
        inset, points, plot = Well.inset(canvas.metrics), Well.font(canvas.metrics), geometry.well.plot
        x, y = canvas.extent.x + inset, canvas.extent.y + inset
        with gpu.matrix.push_pop():  # type: ignore[attr-defined]  # ty: ignore[invalid-context-manager]
            gpu.matrix.translate((x, y))
            geometry.well.under(canvas)
            for batch, color, role in ((geometry.grid, geometry.rule, "grid"), (geometry.majors, Well.major, "major"), *((ticks, Well.major, "major") for ticks in geometry.ticks.to_list())):
                shader = canvas.lines(Shipped.read().metric(role, "stroke"))
                shader.uniform_float("color", color)
                batch.draw(shader)
            with canvas.clipped(msgspec.structs.replace(plot, x=x + plot.x, y=y + plot.y)), Canvas.blended("ADDITIVE"):
                stroke = canvas.lines(Shipped.read().metric("trace", "stroke"))
                for batch, color in geometry.traces:
                    stroke.uniform_float("color", color)
                    batch.draw(stroke)
            for text, at_x, at_y, align in geometry.labels:
                canvas.label(text, (at_x + points / 2 * (1 - 2 * align.value), at_y + points / 2), Well.label, points, align)
            canvas.label(geometry.caption, (plot.x + points / 2, plot.y + plot.height - points * 1.5), Well.label, points)
            geometry.well.over(canvas)


class GamutPlot(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Cached chromaticity geometry at one interface scale."""

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
        """Builds chromaticity diagram geometry."""
        counts = reading.trace
        working = space, reading.working
        display = reading.display.map2(lambda rows, native: (native.getName(), rows), display_space)
        well, locus, (extent, tenths) = Well.gamut(metrics), observer.locus(), observer.frame()
        plot, side = well.plot, float(metrics.scaled(4.0)) / 2.0
        line_shader, point_shader = gpu.shader.from_builtin("POLYLINE_UNIFORM_COLOR"), gpu.shader.from_builtin("POINT_FLAT_COLOR")
        lattice = tuple(np.linspace(0.0, extent[axis], tenths[axis] + 1) for axis in (0, 1))
        occupied = np.argwhere(counts > 0)
        tangent = np.gradient(locus, axis=0)
        outward = np.column_stack((-tangent[:, 1], tangent[:, 0]))
        length = np.linalg.norm(outward, axis=1, keepdims=True)
        normal = np.divide(outward, length, out=np.zeros_like(outward), where=length > 0.0)
        tenth = np.flatnonzero(observer.table()[:, 0] % 10 == 0)

        def placed(xy: npt.ArrayLike) -> npt.NDArray[np.float32]:
            """Maps chromaticity onto plot per axis."""
            return (np.asarray(xy) / extent * (plot.width, plot.height) + (plot.x, plot.y)).astype(np.float32)

        def gamut(rows: npt.NDArray[np.float64]) -> gpu.types.GPUBatch:
            """Builds gamut triangle with white-point square."""
            square = np.array(((-1, -1), (1, -1), (1, -1), (1, 1), (1, 1), (-1, 1), (-1, 1), (-1, -1)), np.float32) * side
            return batch_for_shader(line_shader, "LINES", {"pos": np.vstack((placed(rows[[0, 1, 1, 2, 2, 0]]), placed(rows[3]) + square))})  # type: ignore[dict-item]  # ty: ignore[invalid-argument-type]

        def block(named: tuple[str, npt.NDArray[np.float64]], color: Color) -> tuple[tuple[str, Color], ...]:
            """Formats gamut name and RGBW coordinates."""
            name, rows = named
            return ((name, color), *((f"{letter} {x:.4f} {y:.4f}", color) for letter, x, y in zip("RGBW", rows[:, 0], rows[:, 1])))

        def traced(found: npt.NDArray[np.intp]) -> gpu.types.GPUBatch:
            """Builds occupied cells at log peak-normalized opacity."""
            alpha = Well.trace[3] * np.log1p(counts[tuple(found.T)]) / np.log1p(counts.max())
            positions = ((found + 0.5) / counts.shape * (plot.width, plot.height) + (plot.x, plot.y)).astype(np.float32)
            return batch_for_shader(point_shader, "POINTS", {"pos": positions, "color": np.column_stack((np.ones((len(found), 3)), alpha)).astype(np.float32)})  # type: ignore[dict-item]  # ty: ignore[invalid-argument-type]

        grid = (*(point for x in lattice[0] for point in ((x, 0.0), (x, extent[1]))), *(point for y in lattice[1] for point in ((0.0, y), (extent[0], y))))
        return cls(
            well=well,
            grid=batch_for_shader(line_shader, "LINES", {"pos": placed(grid)}),  # type: ignore[dict-item]  # ty: ignore[invalid-argument-type]
            locus=batch_for_shader(line_shader, "LINE_STRIP", {"pos": placed(np.vstack((locus, locus[:1])))}),  # type: ignore[dict-item]  # ty: ignore[invalid-argument-type]
            ticks=batch_for_shader(line_shader, "LINES", {"pos": placed(np.stack((locus[tenth], locus[tenth] + 0.01 * normal[tenth]), axis=1).reshape(-1, 2))}),  # type: ignore[dict-item]  # ty: ignore[invalid-argument-type]
            working=gamut(reading.working),
            display=reading.display.map2(lambda rows, _space: gamut(rows), display_space),
            trace=Some(occupied).filter(lambda found: found.size > 0).map(traced),
            lines=(*block(working, Well.label), *(line for named in display.to_list() for line in block(named, Well.label))),
        )


class Gamuts(msgspec.Struct, frozen=True, kw_only=True):
    """Chromaticity density with working and display gamuts."""

    source: Callable[[], BlenderResult[Option[ChartFrame]]]

    def read(self, metrics: Metrics) -> BlenderResult[Option[GamutPlot]]:
        """Reads chromaticities at plot cell size."""
        config, plot = PyOpenColorIO.GetCurrentConfig(), Well.gamut(metrics).plot

        def charted(frame: ChartFrame) -> BlenderResult[GamutPlot]:
            """Builds diagram from scene-linear frame."""
            return (
                frame
                .plane(config)
                .bind(lambda plane: chromaticity_of(config, plane, frame.view, (round(plot.width), round(plot.height))).map_error(BlenderImagingFault.lifted))
                .map(lambda reading: GamutPlot.of(metrics, reading, config.getColorSpace(PyOpenColorIO.ROLE_SCENE_LINEAR).getName(), frame.view.colorspace(config)))
            )

        return self.source().bind(lambda frame: frame.map(lambda held: charted(held).map(Some)).default_value(Ok(Nothing)))

    def drawn(self, canvas: Canvas, geometry: GamutPlot) -> None:
        """Draws diagram at lower-right corner."""
        inset, points, plot = Well.inset(canvas.metrics), Well.font(canvas.metrics), geometry.well.plot
        x, y = canvas.extent.x + canvas.extent.width - inset - geometry.well.width, canvas.extent.y + inset
        pitch = points + canvas.metrics.spacing.button_y
        with gpu.matrix.push_pop():  # type: ignore[attr-defined]  # ty: ignore[invalid-context-manager]
            gpu.matrix.translate((x, y))
            geometry.well.under(canvas)
            with canvas.clipped(msgspec.structs.replace(plot, x=x + plot.x, y=y + plot.y)):
                for batch, color, role in (
                    (geometry.grid, Well.grid, "grid"),
                    (geometry.locus, Well.major, "major"),
                    (geometry.ticks, Well.major, "major"),
                    *((display, Well.major, "major") for display in geometry.display.to_list()),
                    (geometry.working, Well.trace, "trace"),
                ):
                    shader = canvas.lines(Shipped.read().metric(role, "stroke"))
                    shader.uniform_float("color", color)
                    batch.draw(shader)
                with Canvas.blended("ADDITIVE"):
                    for trace in geometry.trace.to_list():
                        trace.draw(canvas.points())
                for index, (text, color) in enumerate(geometry.lines, 1):
                    canvas.label(text, (plot.x + plot.width - points / 2, plot.y + plot.height - points / 2 - index * pitch), color, points, Align.END)
            geometry.well.over(canvas)


class KeyPlot(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """False-color legend with exposure labels."""

    well: Well
    bar: Rect
    texture: gpu.types.GPUTexture
    labels: tuple[tuple[str, float, float], ...]

    @staticmethod
    def of(metrics: Metrics, bands: Bands) -> "BlenderResult[KeyPlot]":
        """Builds nearest-filtered key."""
        points, pad, width = Well.font(metrics), float(metrics.spacing.template), float(metrics.unit * 20)
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
    """False-color bands in stops from middle gray."""

    source: Callable[[], BlenderResult[Option[DisplayView]]]

    def read(self, metrics: Metrics) -> BlenderResult[Option[KeyPlot]]:
        """Builds key at default display."""
        config = PyOpenColorIO.GetCurrentConfig()

        def keyed(view: DisplayView) -> BlenderResult[Option[KeyPlot]]:
            """Builds key when view has constant bands."""
            return (
                Bands
                .of(config, msgspec.structs.replace(view, display=config.getDefaultDisplay()))
                .map_error(BlenderImagingFault.lifted)
                .bind(lambda bands: bands.map(lambda held: KeyPlot.of(metrics, held).map(Some)).default_value(Ok(Nothing)))
            )

        return self.source().bind(lambda view: view.map(keyed).default_value(Ok(Nothing)))

    def drawn(self, canvas: Canvas, geometry: KeyPlot) -> None:
        """Draws key at upper-left corner."""
        inset, points = Well.inset(canvas.metrics), Well.font(canvas.metrics)
        with gpu.matrix.push_pop():  # type: ignore[attr-defined]  # ty: ignore[invalid-context-manager]
            gpu.matrix.translate((canvas.extent.x + inset, canvas.extent.y + canvas.extent.height - inset - geometry.well.height))
            geometry.well.under(canvas)
            canvas.picture(geometry.texture, geometry.bar, gpu.shader.from_builtin("IMAGE_COLOR"), blend="ALPHA")
            for text, x, y in geometry.labels:
                canvas.label(text, (x, y), Well.label, points, Align.CENTER)
            geometry.well.over(canvas)


# --- [OPERATIONS] -----------------------------------------------------------------------


def histogram(source: Callable[[], BlenderResult[Option[ChartFrame]]], metrics: Metrics) -> BlenderResult[Option[TracePlot]]:
    """Returns scene-linear histogram with exposure-band ticks."""
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
                grid=Grid(ink=Well.fine, verticals=bounds[1:], horizontals=(0.25, 0.5, 0.75)),
                majors=bounds[:1],
                ticks=tuple(position / bins for reading in bands.to_list() for position in np.searchsorted(Histogram.edges(), reading.edges, side="right")),
                labels=(*((f"{2 ** (stop - 1):g}", x, 0.0, Align.START) for stop, x in enumerate(bounds, 1)), (f"{2**Histogram.headroom:g}", 1.0, 0.0, Align.END)),
                caption=config.getColorSpace(PyOpenColorIO.ROLE_SCENE_LINEAR).getName(),
            )

        return paired(
            frame.plane(config).bind(lambda plane: histogram_of(config, plane).map_error(BlenderImagingFault.lifted)), Bands.of(config, frame.view).map_error(BlenderImagingFault.lifted)
        ).map(plotted)

    return source().bind(lambda frame: frame.map(lambda held: traced(held).map(Some)).default_value(Ok(Nothing)))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["ChartFrame", "FalseColor", "GamutPlot", "Gamuts", "Grid", "KeyPlot", "Outline", "TracePlot", "Well", "histogram"]
