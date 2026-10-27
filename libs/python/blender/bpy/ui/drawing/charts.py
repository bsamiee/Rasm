"""Chart wells in the paints of Blender's scopes and chart geometry over plain arrays in plot fractions."""

from collections.abc import Callable, Generator, Mapping
from contextlib import contextmanager, ExitStack
from typing import ClassVar, Final, Literal, Protocol, Self

from expression import Nothing, Option, Some
import gpu
from gpu_extras.batch import batch_for_shader
import msgspec
import numpy as np
import numpy.typing as npt

from blender.bpy.ui.drawing.canvas import Align, Blend, Canvas, PUSH_POP, Rect
from blender.bpy.ui.resources.theme import CLEAR, Color, Metrics
from blender.records.faults import Outcome

# --- [TYPES] ----------------------------------------------------------------------------


class Plot(Protocol):
    """Cached chart geometry drawing itself in its well."""

    def drawn(self, canvas: Canvas, /) -> None:
        """Draws chart on canvas."""
        ...


type ChartLayer = Rules | Trace | Points

# --- [CONSTANTS] ------------------------------------------------------------------------

_BATCH_FOR_SHADER: Final[Callable[[gpu.types.GPUShader, str, Mapping[str, npt.NDArray[np.float32]]], gpu.types.GPUBatch]] = batch_for_shader  # type: ignore[assignment]  # ty: ignore[invalid-assignment]

# --- [MODELS] ---------------------------------------------------------------------------


class Paint(msgspec.Struct, frozen=True, kw_only=True):
    """Layer paint over a well: display color, blend, and stroke width in points."""

    color: Color
    blend: Blend = "ALPHA"
    stroke: float = 1.0


class Well(msgspec.Struct, frozen=True, kw_only=True):
    """Rounded chart well in the black ground, half-black rim, soft white text, and white layer paints of Blender's scopes, its inset and radius in device pixels."""

    ground: ClassVar[Color] = (0.0, 0.0, 0.0, 1.0)
    rim: ClassVar[Color] = (0.0, 0.0, 0.0, 0.5)
    text: ClassVar[Color] = (1.0, 1.0, 1.0, 0.7)
    fine: ClassVar[Paint] = Paint(color=(1.0, 1.0, 1.0, 0.08))
    grid: ClassVar[Paint] = Paint(color=(1.0, 1.0, 1.0, 0.2))
    major: ClassVar[Paint] = Paint(color=(1.0, 1.0, 1.0, 0.5))
    trace: ClassVar[Paint] = Paint(color=(1.0, 1.0, 1.0, 0.75), blend="ADDITIVE", stroke=1.5)
    width: float
    height: float
    inset: float
    radius: float

    @classmethod
    def of(cls, metrics: Metrics, aspect: float) -> Self:
        """Returns a ten-unit well whose plot height is `aspect` times its plot width."""
        width = float(metrics.unit * 10)
        return cls(width=width, height=(width - 2.0) * aspect + 2.0, inset=float(metrics.scaled(7.2)), radius=float(metrics.scaled(3.0)))

    @property
    def plot(self) -> Rect:
        """Plot rectangle one pixel inside the well."""
        return Rect(x=1.0, y=1.0, width=self.width - 2.0, height=self.height - 2.0)

    @contextmanager
    def placed(self, canvas: Canvas, x: float, y: float) -> Generator[None]:
        """Draws the well background at a corner, the body in well coordinates, then the rim above the lower edge."""
        with PUSH_POP(), ExitStack() as scope:
            gpu.matrix.translate((x, y))
            canvas.frame(Rect(x=0.0, y=0.0, width=self.width, height=self.height), Well.ground, CLEAR, self.radius)
            scope.callback(canvas.frame, Rect(x=0.0, y=1.0, width=self.width, height=self.height - 1.0), CLEAR, Well.rim, self.radius)
            yield


class Rules(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Line segments in plot fractions, each row pair a start and an end."""

    primitive: ClassVar[Literal["LINES"]] = "LINES"
    segments: npt.NDArray[np.float32]
    paint: Paint = Well.grid


class Trace(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Polyline through points in plot fractions."""

    primitive: ClassVar[Literal["LINE_STRIP"]] = "LINE_STRIP"
    points: npt.NDArray[np.float32]
    paint: Paint = Well.trace


class Points(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Points in plot fractions, each weight scaling the paint's alpha."""

    points: npt.NDArray[np.float32]
    weights: npt.NDArray[np.float32]
    paint: Paint = Well.trace


class Label(msgspec.Struct, frozen=True, kw_only=True):
    """Text anchored at a plot fraction, offset half a line into the plot."""

    text: str
    x: float
    y: float
    align: Align = Align.START


class LayerBatch(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Chart layer batch with its paint, points drawing at no width."""

    batch: gpu.types.GPUBatch
    paint: Paint
    width: Option[float]

    @classmethod
    def of(cls, layer: ChartLayer, plot: Rect) -> Self:
        """Builds a layer's batch over plot pixels."""
        color = layer.paint.color

        def placed(at: npt.NDArray[np.float32]) -> npt.NDArray[np.float32]:
            return (at * (plot.width, plot.height) + (plot.x, plot.y)).astype(np.float32)

        match layer:
            case Points(points=at, weights=weights):
                tints = np.column_stack((np.broadcast_to(np.asarray(color[:3], np.float32), (len(weights), 3)), color[3] * weights)).astype(np.float32)
                batch = _BATCH_FOR_SHADER(gpu.shader.from_builtin("POINT_FLAT_COLOR"), "POINTS", {"pos": placed(at), "color": tints})
                return cls(batch=batch, paint=layer.paint, width=Nothing)
            case Rules(segments=at) | Trace(points=at):
                batch = _BATCH_FOR_SHADER(gpu.shader.from_builtin("POLYLINE_UNIFORM_COLOR"), layer.primitive, {"pos": placed(at)})
                return cls(batch=batch, paint=layer.paint, width=Some(layer.paint.stroke))

    def drawn(self, canvas: Canvas) -> None:
        """Draws the batch under its paint's blend."""
        with Canvas.blended(self.paint.blend):
            match self.width:
                case Option(tag="some", some=width):
                    shader = canvas.lines(width)
                    shader.uniform_float("color", self.paint.color)
                case _:
                    shader = canvas.points()
            self.batch.draw(shader)


class Chart(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Chart geometry in a well at one interface scale: layers in order clipped to the plot, labels at plot fractions, caption lines stacked down from the top on the corner's side."""

    well: Well
    marks: tuple[LayerBatch, ...]
    labels: tuple[Label, ...]
    caption: tuple[str, ...]
    corner: Align
    size: float

    @classmethod
    def of(cls, metrics: Metrics, *, well: Well, layers: tuple[ChartLayer, ...], labels: tuple[Label, ...] = (), caption: tuple[str, ...] = (), corner: Align = Align.START) -> Self:
        """Builds every layer's batch once for the interface scale of `metrics`, text at the scopes' fraction of the widget font."""
        return cls(well=well, marks=tuple(LayerBatch.of(layer, well.plot) for layer in layers), labels=labels, caption=caption, corner=corner, size=metrics.points(0.73))

    def drawn(self, canvas: Canvas) -> None:
        """Draws the chart at its lower corner of the canvas extent."""
        well, size, plot = self.well, self.size, self.well.plot
        x = canvas.extent.x + well.inset + (canvas.extent.width - 2.0 * well.inset - well.width) * self.corner.value
        y = canvas.extent.y + well.inset
        pitch = size + canvas.metrics.spacing.button_y
        with well.placed(canvas, x, y):
            with canvas.clipped(msgspec.structs.replace(plot, x=x + plot.x, y=y + plot.y)):
                for mark in self.marks:
                    mark.drawn(canvas)
            for label in self.labels:
                canvas.label(label.text, (plot.x + label.x * plot.width + size / 2 * (1 - 2 * label.align.value), plot.y + label.y * plot.height + size / 2), Well.text, size, label.align)
            for index, line in enumerate(self.caption, 1):
                canvas.label(line, (plot.x + size / 2 + (plot.width - size) * self.corner.value, plot.y + plot.height - size / 2 - index * pitch), Well.text, size, self.corner)


class Plotted[S: Plot](msgspec.Struct, frozen=True, kw_only=True):
    """Part reading chart geometry per interface scale and drawing it in its well."""

    read: Callable[[Metrics], Outcome[Option[S]]]

    @staticmethod
    def drawn(canvas: Canvas, plot: S, /) -> None:
        """Draws the plot on canvas."""
        plot.drawn(canvas)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Chart", "ChartLayer", "Label", "LayerBatch", "Paint", "Plot", "Plotted", "Points", "Rules", "Trace", "Well"]
