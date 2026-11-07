"""Charts drawn in the colors of Blender's scopes."""

from collections.abc import Callable, Iterable
from functools import partial
from typing import Final, Literal, Self

from expression import Option
import gpu
from gpu_extras.batch import batch_for_shader
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from rasm_bpy.ui._drawing.canvas import _draw_batch, Align, Blend, Canvas, FontStyle, Program, Rect
from rasm_bpy.ui._resources.theme import _CLEAR, Color, Metrics, Spacing
from rasm_imaging import Bands, Chromaticity, colorimetry, DisplayView, Histogram
from rasm_records import Nothing, Outcome, paired

# --- [TYPES] ----------------------------------------------------------------------------

type _Mark = Literal["LINES", "LINE_STRIP", "POINTS", "TRIS"]

# --- [CONSTANTS] ------------------------------------------------------------------------

_FINE: Final[Color] = (1.0, 1.0, 1.0, 0.08)
GRID: Final[Color] = (1.0, 1.0, 1.0, 0.2)
_MAJOR: Final[Color] = (1.0, 1.0, 1.0, 0.5)
TRACE: Final[Color] = (1.0, 1.0, 1.0, 0.75)
_GROUND: Final[Color] = (0.0, 0.0, 0.0, 1.0)
_RIM: Final[Color] = (0.0, 0.0, 0.0, 0.5)
_TEXT: Final[Color] = (1.0, 1.0, 1.0, 0.2)
_TEXT_POINTS: Final = 8.0
_RADIUS: Final = 3.0
_UNITS: Final = 10

# --- [MODELS] ---------------------------------------------------------------------------


class Layer(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Chart marks batched once in plot fractions under the flat-color shader their primitive binds, drawn under their blend."""

    batch: gpu.types.GPUBatch
    bind: Callable[[Canvas], Program]
    blend: Blend

    @classmethod
    def of(
        cls,
        primitive: _Mark,
        positions: npt.NDArray[np.float32],
        color: Color | npt.NDArray[np.float32],
        *,
        indices: Option[npt.NDArray[np.uint32]] = Nothing,
        blend: Blend = "ALPHA",
        width: float = 1.0,
    ) -> Self:
        """Batches `(N, 2)` plot-fraction positions in one color or per-vertex `(N, 4)` colors, `indices` naming each mark's vertices."""
        match primitive:
            case "POINTS":
                shader, bind = "POINT_FLAT_COLOR", Canvas.points
            case "TRIS":
                shader, bind = "FLAT_COLOR", partial(Canvas.shader, name="FLAT_COLOR")
            case _:
                shader, bind = "POLYLINE_FLAT_COLOR", partial(Canvas.lines, width=width, shader="POLYLINE_FLAT_COLOR")
        colors = np.ascontiguousarray(np.broadcast_to(np.asarray(color, np.float32), (len(positions), 4)))
        batch = batch_for_shader(gpu.shader.from_builtin(shader), primitive, {"pos": np.ascontiguousarray(positions, np.float32), "color": colors}, indices=indices.to_optional())  # type: ignore[arg-type, dict-item]  # ty: ignore[invalid-argument-type]
        return cls(batch=batch, bind=bind, blend=blend)

    def drawn(self, canvas: Canvas) -> None:
        """Draws the batch under its blend."""
        with Canvas.blended(self.blend):
            _draw_batch(self.batch, self.bind(canvas))


class Label(msgspec.Struct, frozen=True, kw_only=True):
    """Text anchored at a plot fraction, offset half a line into the plot."""

    text: str
    x: float
    y: float
    align: Align = Align.START


class Chart(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Plot of a size in device pixels in the well, rim, and text colors of Blender's scopes, which no theme member holds, the well one pixel wider each side.

    Layers draw in order clipped to the plot, labels at plot fractions, and caption lines stack down from the top on the corner's side.
    """

    width: float
    height: float
    layers: tuple[Layer, ...] = ()
    labels: tuple[Label, ...] = ()
    caption: tuple[str, ...] = ()
    corner: Align = Align.START

    def drawn(self, canvas: Canvas) -> None:
        """Draws the chart at its lower corner of the canvas extent, inset by Blender's panel margin, text at the scopes' label size."""
        metrics, extent = canvas.metrics, canvas.extent
        size, inset = float(metrics.scaled(_TEXT_POINTS)), metrics.margin
        x = extent.x + inset + (extent.width - 2.0 * inset - self.width - 2.0) * self.corner.value
        y = extent.y + inset
        pitch = size + metrics.spaced(Spacing.BUTTON_Y)
        with Canvas.mapped(Rect(x=x, y=y, width=1.0, height=1.0)):
            canvas.frame(Rect(x=0.0, y=0.0, width=self.width + 2.0, height=self.height + 2.0), _GROUND, _CLEAR, _RADIUS)
            with canvas.clipped(Rect(x=x + 1.0, y=y + 1.0, width=self.width, height=self.height)), Canvas.mapped(Rect(x=1.0, y=1.0, width=self.width, height=self.height)):
                for layer in self.layers:
                    layer.drawn(canvas)
            for label in self.labels:
                canvas.label(label.text, (1.0 + label.x * self.width + size / 2 * (1 - 2 * label.align.value), 1.0 + label.y * self.height + size / 2), _TEXT, FontStyle(size=size, align=label.align))
            for index, line in enumerate(self.caption, 1):
                canvas.label(line, (1.0 + size / 2 + (self.width - size) * self.corner.value, 1.0 + self.height - size / 2 - index * pitch), _TEXT, FontStyle(size=size, align=self.corner))
            canvas.frame(Rect(x=0.0, y=1.0, width=self.width + 2.0, height=self.height + 1.0), _CLEAR, _RIM, _RADIUS)


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [PLOTS]


def histogram(metrics: Metrics, config: PyOpenColorIO.Config, plane: npt.NDArray[np.float32], view: DisplayView, stops: tuple[float, float]) -> Outcome[Chart]:
    """Charts a frame's scene-linear luminance counts, linear to one then four log stops, with the view's exposure bands across `stops` ticked along the floor.

    Args:
        metrics: Interface metrics sizing the plot at ten widget units by five.
        config: Configuration whose scene-linear role the frame holds.
        plane: Scene-linear RGBA frame.
        view: View whose constant levels band the floor.
        stops: Lowest and highest exposure the bands span.

    Returns:
        Chart captioned with the scene-linear space, or each color fault of the reads.
    """
    width = float(metrics.unit * _UNITS)

    def plotted(histogram: Histogram, bands: Option[Bands]) -> Chart:
        counts = histogram.counts
        bins, peak = counts.size, counts.max()
        bounds = np.arange(1, Histogram.headroom + 1) * Histogram.resolution / bins
        return Chart(
            width=width,
            height=width * 0.5,
            layers=(
                Layer.of("LINES", ruled(bounds[1:], (0.25, 0.5, 0.75)), _FINE),
                Layer.of("LINES", ruled((bounds[0], 1.0), (1.0,)), _MAJOR),
                *(Layer.of("LINES", ruled(np.searchsorted(Histogram.edges(), held.edges, side="right") / bins, reach=0.5 * metrics.unit / width), _MAJOR) for held in bands.to_list()),
                Layer.of("LINE_STRIP", np.column_stack((np.arange(bins) / bins, np.divide(counts, peak, out=np.zeros(bins), where=peak > 0))).astype(np.float32), TRACE, blend="ADDITIVE", width=1.5),
            ),
            labels=(*(Label(text=f"{2**stop:g}", x=float(x), y=0.0) for stop, x in enumerate(bounds)), Label(text=f"{2**Histogram.headroom:g}", x=1.0, y=0.0, align=Align.END)),
            caption=(config.getColorSpace(PyOpenColorIO.ROLE_SCENE_LINEAR).getName(),),
        )

    return paired(Histogram.of(config, plane), Bands.of(config, view, stops)).map(lambda found: plotted(*found))


def chromaticity(metrics: Metrics, config: PyOpenColorIO.Config, plane: npt.NDArray[np.float32], view: DisplayView, *, extent: Option[npt.NDArray[np.float64]] = Nothing) -> Outcome[Chart]:
    """Charts a frame's scene-linear xy density inside the spectral locus, ticked every ten nanometres, with the working and display gamuts and their white points boxed.

    Args:
        metrics: Interface metrics sizing the plot ten widget units wide.
        config: Configuration whose scene-linear role the frame holds.
        plane: Scene-linear RGBA frame.
        view: View whose display space draws the second gamut, none for a data view.
        extent: Far xy corner the diagram frames from the origin, the locus maxima rounded up to tenths while unstated.

    Returns:
        Chart captioned with each gamut's primaries and white, or each color fault of the read.
    """
    framed = extent.default_with(colorimetry.diagram_extent)
    tenths = np.rint(framed * 10.0).astype(np.intp)
    width, locus = float(metrics.unit * _UNITS), colorimetry.locus()
    height = width * float(framed[1] / framed[0])
    outward = np.gradient(locus, axis=0)[:, ::-1] * (-1.0, 1.0)
    normal = np.divide(outward, length := np.linalg.norm(outward, axis=1, keepdims=True), out=np.zeros_like(outward), where=length > 0.0)
    tenth = np.flatnonzero(colorimetry.observer()[:, 0] % 10 == 0)
    square = np.array(((-1, -1), (1, -1), (1, -1), (1, 1), (1, 1), (-1, 1), (-1, 1), (-1, -1)), np.float64) * float(metrics.scaled(2.0)) / (width, height)

    def gamut(rows: npt.NDArray[np.float64], color: Color, blend: Blend, stroke: float) -> Layer:
        return Layer.of("LINES", np.vstack((rows[[0, 1, 1, 2, 2, 0]] / framed, rows[3] / framed + square)).astype(np.float32), color, blend=blend, width=stroke)

    def block(name: str, rows: npt.NDArray[np.float64]) -> tuple[str, ...]:
        return (name, *(f"{letter} {x:.4f} {y:.4f}" for letter, (x, y) in zip("RGBW", rows.tolist())))

    def plotted(reading: Chromaticity) -> Chart:
        counts, found = reading.trace, np.argwhere(reading.trace > 0)
        display = reading.display.map2(lambda rows, space: (space.getName(), rows), view.colorspace(config))
        weights = np.log1p(counts[tuple(found.T)]) / np.log1p(counts.max())
        density = (
            (
                Layer.of(
                    "POINTS",
                    ((found + 0.5) / counts.shape).astype(np.float32),
                    np.column_stack((np.broadcast_to(TRACE[:3], (len(weights), 3)), TRACE[3] * weights)).astype(np.float32),
                    blend="ADDITIVE",
                ),
            )
            if found.size
            else ()
        )
        return Chart(
            width=width,
            height=height,
            layers=(
                Layer.of("LINES", ruled(np.linspace(0.0, 1.0, tenths[0] + 1), np.linspace(0.0, 1.0, tenths[1] + 1)), GRID),
                Layer.of("LINE_STRIP", (np.vstack((locus, locus[:1])) / framed).astype(np.float32), _MAJOR),
                Layer.of("LINES", (np.stack((locus[tenth], locus[tenth] + 0.01 * normal[tenth]), axis=1).reshape(-1, 2) / framed).astype(np.float32), _MAJOR),
                *(gamut(rows, _MAJOR, "ALPHA", 1.0) for _, rows in display.to_list()),
                gamut(reading.working, TRACE, "ADDITIVE", 1.5),
                *density,
            ),
            caption=(*block(config.getColorSpace(PyOpenColorIO.ROLE_SCENE_LINEAR).getName(), reading.working), *(line for named in display.to_list() for line in block(*named))),
            corner=Align.END,
        )

    return Chromaticity.of(config, plane, view, (round(width), round(height)), framed).map(plotted)


# --- [MARKS]


def ruled(verticals: Iterable[float], horizontals: Iterable[float] = (), reach: float = 1.0) -> npt.NDArray[np.float32]:
    """Returns plot-fraction `LINES` segments rising from the floor to `reach` at each vertical and spanning the plot at each horizontal."""
    return np.array([*((x, 0.0, x, reach) for x in verticals), *((0.0, y, 1.0, y) for y in horizontals)], np.float32).reshape(-1, 2)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["GRID", "TRACE", "Chart", "Label", "Layer", "chromaticity", "histogram", "ruled"]
