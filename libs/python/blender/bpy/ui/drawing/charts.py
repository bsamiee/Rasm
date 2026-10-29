"""Charts in the well, rim, grid, trace, and text colors of Blender's scopes, which no theme member holds (`interface_draw.cc` `draw_but_HISTOGRAM`, `draw_scope_end`, `draw_but_WAVEFORM`), over geometry batched once in plot fractions."""

from typing import Final, Literal, Self

import gpu
import msgspec
import numpy as np
import numpy.typing as npt

from blender.bpy.ui.drawing.canvas import Align, Blend, Canvas, Rect
from blender.bpy.ui.resources.theme import CLEAR, Color, Spacing

# --- [TYPES] ----------------------------------------------------------------------------

type Mark = Literal["LINES", "LINE_STRIP", "POINTS"]

# --- [CONSTANTS] ------------------------------------------------------------------------

FINE: Final[Color] = (1.0, 1.0, 1.0, 0.08)
GRID: Final[Color] = (1.0, 1.0, 1.0, 0.2)
MAJOR: Final[Color] = (1.0, 1.0, 1.0, 0.5)
TRACE: Final[Color] = (1.0, 1.0, 1.0, 0.75)
_GROUND: Final[Color] = (0.0, 0.0, 0.0, 1.0)
_RIM: Final[Color] = (0.0, 0.0, 0.0, 0.5)
_TEXT: Final[Color] = (1.0, 1.0, 1.0, 0.2)
_TEXT_POINTS: Final = 8.0
_RADIUS: Final = 3.0

# --- [MODELS] ---------------------------------------------------------------------------


class Layer(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Chart geometry batched once in plot fractions, drawn under its blend at its stroke width in points."""

    primitive: Mark
    batch: gpu.types.GPUBatch
    blend: Blend
    width: float

    @classmethod
    def of(cls, primitive: Mark, positions: npt.NDArray[np.float32], color: Color | npt.NDArray[np.float32], *, blend: Blend = "ALPHA", width: float = 1.0) -> Self:
        """Batches `(N, 2)` plot-fraction positions in one color or per-vertex `(N, 4)` colors."""
        colors = np.ascontiguousarray(np.broadcast_to(np.asarray(color, np.float32), (len(positions), 4)))
        batch = Canvas.batched("POINT_FLAT_COLOR" if primitive == "POINTS" else "POLYLINE_FLAT_COLOR", primitive, {"pos": np.ascontiguousarray(positions, np.float32), "color": colors})
        return cls(primitive=primitive, batch=batch, blend=blend, width=width)

    def drawn(self, canvas: Canvas) -> None:
        """Draws the batch under its blend."""
        with Canvas.blended(self.blend), canvas.points() if self.primitive == "POINTS" else canvas.lines(self.width, "POLYLINE_FLAT_COLOR") as shader:
            self.batch.draw(shader)


class Label(msgspec.Struct, frozen=True, kw_only=True):
    """Text anchored at a plot fraction, offset half a line into the plot."""

    text: str
    x: float
    y: float
    align: Align = Align.START


class Chart(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Plot of a size in device pixels in the scopes' well one pixel wider each side: layers in order clipped to the plot, labels at plot fractions, and caption lines stacked down from the top on the corner's side."""

    width: float
    height: float
    layers: tuple[Layer, ...] = ()
    labels: tuple[Label, ...] = ()
    caption: tuple[str, ...] = ()
    corner: Align = Align.START

    def drawn(self, canvas: Canvas) -> None:
        """Draws the chart at its lower corner of the canvas extent, inset by Blender's panel margin, text at the scopes' label size."""
        metrics, extent = canvas.metrics, canvas.extent
        size, inset = float(metrics.scaled(_TEXT_POINTS)), metrics.unit * 0.4
        x = extent.x + inset + (extent.width - 2.0 * inset - self.width - 2.0) * self.corner.value
        y = extent.y + inset
        pitch = size + metrics.spaced(Spacing.BUTTON_Y)
        with Canvas.mapped(Rect(x=x, y=y, width=1.0, height=1.0)):
            canvas.frame(Rect(x=0.0, y=0.0, width=self.width + 2.0, height=self.height + 2.0), _GROUND, CLEAR, _RADIUS)
            with canvas.clipped(Rect(x=x + 1.0, y=y + 1.0, width=self.width, height=self.height)), Canvas.mapped(Rect(x=1.0, y=1.0, width=self.width, height=self.height)):
                for layer in self.layers:
                    layer.drawn(canvas)
            for label in self.labels:
                canvas.label(label.text, (1.0 + label.x * self.width + size / 2 * (1 - 2 * label.align.value), 1.0 + label.y * self.height + size / 2), _TEXT, size, label.align)
            for index, line in enumerate(self.caption, 1):
                canvas.label(line, (1.0 + size / 2 + (self.width - size) * self.corner.value, 1.0 + self.height - size / 2 - index * pitch), _TEXT, size, self.corner)
            canvas.frame(Rect(x=0.0, y=1.0, width=self.width + 2.0, height=self.height + 1.0), CLEAR, _RIM, _RADIUS)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["FINE", "GRID", "MAJOR", "TRACE", "Chart", "Label", "Layer", "Mark"]
