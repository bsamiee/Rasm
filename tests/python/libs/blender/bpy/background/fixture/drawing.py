"""Drawing rows: an overlay over the 3D View with every refresh source, with probes drawing charts and text offscreen and holding draw handles and refreshes."""

from collections.abc import Callable, Mapping
from types import MappingProxyType
from typing import ClassVar, Final

import bpy
from expression import Ok, Option, Some
import gpu
import numpy as np
import numpy.typing as npt

from blender.bpy.extension.identity import Identity
from blender.bpy.ui.drawing.canvas import Align, Canvas, DrawType, Rect
from blender.bpy.ui.drawing.charts import Chart, Label, Layer, TRACE
from blender.bpy.ui.drawing.handlers import Drawing, Handled, hooked, Overlay, Refresh, refreshing, Signal
from blender.bpy.ui.drawing.offscreen import captured, Offscreen
from blender.bpy.ui.resources.theme import Metrics
from blender.bpy.ui.screens.areas import AreaQuery
from blender.records.faults import Outcome

# --- [CONSTANTS] ------------------------------------------------------------------------

_HALF_ALPHA: Final = 0.5

# --- [MODELS] ---------------------------------------------------------------------------


class Marker:
    """Part drawing a white square once its read holds state."""

    __slots__ = ()
    draw_type: ClassVar[DrawType] = "POST_PIXEL"

    @staticmethod
    def read(_metrics: Metrics, /) -> Outcome[Option[int]]:
        """Holds a fixed state."""
        return Ok(Some(1))

    @staticmethod
    def drawn(canvas: Canvas, _state: int, /) -> None:
        """Draws the square at the canvas origin."""
        canvas.frame(Rect(x=canvas.extent.x, y=canvas.extent.y, width=8.0, height=8.0), (1.0, 1.0, 1.0, 1.0), (1.0, 1.0, 1.0, 1.0), 0.0)


# --- [OPERATIONS] -----------------------------------------------------------------------
# --- [READS]


def _canvas() -> Canvas:
    """Reads the canvas of the 3D View main region."""
    manager: bpy.types.WindowManager = bpy.context.window_manager  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    area = next(area for area in manager.windows[0].screen.areas if area.type == "VIEW_3D")
    return Canvas.of(area, next(region for region in area.regions if region.type == "WINDOW"))


def _covered(plane: npt.NDArray[np.float32]) -> tuple[int, int, int, int]:
    """Returns the top-first row and column span of pixels at least half opaque."""
    rows, columns = np.nonzero(plane[..., 3] >= _HALF_ALPHA)
    height, width, _ = plane.shape
    return int(rows.min(initial=height)), int(rows.max(initial=-1)), int(columns.min(initial=width)), int(columns.max(initial=-1))


def _chart(corner: Align) -> Callable[[Canvas], None]:
    """Returns the draw of a chart with a diagonal trace, points, a label, and a caption at a corner."""

    def draw(canvas: Canvas) -> None:
        diagonal = np.array(((0.0, 0.0), (1.0, 1.0)), np.float32)
        chart = Chart(
            width=40.0,
            height=30.0,
            layers=(Layer.of("LINE_STRIP", diagonal, TRACE, width=2.0), Layer.of("POINTS", diagonal, np.ones((2, 4), np.float32))),
            labels=(Label(text="0", x=0.0, y=0.0),),
            caption=("Fixture",),
            corner=corner,
        )
        chart.drawn(canvas)

    return draw


# --- [PROBES]


def charted(_identity: Identity) -> dict[str, object]:
    """Draws the chart offscreen at the start and end corners, answering the buffer shape, the opaque span, and the brightest trace value."""
    canvas = _canvas()

    def read(corner: Align) -> object:
        return Offscreen(width=64, height=48).drawn(canvas, _chart(corner)).map(lambda plane: {"shape": plane.shape, "covered": _covered(plane), "brightest": float(plane[..., :3].max())})

    return {"start": read(Align.START), "end": read(Align.END), "inset": canvas.metrics.unit * 0.4}


def written(_identity: Identity) -> dict[str, object]:
    """Measures text at a size, longer, and wrapped narrower than it, and draws a label offscreen."""
    canvas = _canvas()
    single = canvas.measured("Fixture", 14.0)
    return {
        "single": single,
        "longer": canvas.measured("Fixture Fixture", 14.0)[0] > single[0],
        "wrapped": canvas.measured("Fixture Fixture Fixture", 14.0, wrap=Some(single[0]))[1] > single[1],
        "label": Offscreen(width=32, height=32).drawn(canvas, lambda pixels: pixels.label("F", (4.0, 4.0), (1.0, 1.0, 1.0, 1.0), 24.0)).map(lambda plane: float(plane[..., 3].max())),
    }


def read_back(_identity: Identity) -> dict[str, object]:
    """Reads a framebuffer rectangle inside an offscreen draw and the blend mode a blend scope restores, and refuses a buffer past the texture limit."""
    seen: dict[str, object] = {}

    def draw(_buffer: gpu.types.GPUOffScreen) -> Outcome[None]:
        gpu.state.active_framebuffer_get().clear(color=(0.0, 1.0, 0.0, 1.0))
        seen["corner"] = captured(Rect(x=0.0, y=0.0, width=2.0, height=3.0)).map(lambda plane: (plane.shape, plane[0, 0].tolist()))
        with Canvas.blended("ADDITIVE"):
            inside = gpu.state.blend_get()
        seen["blend"] = (inside, gpu.state.blend_get())
        return Ok(None)

    whole = Offscreen(width=8, height=8, format="RGBA16F").read(draw).map(lambda plane: (plane.shape, plane[4, 4].tolist()))
    return {**seen, "whole": whole, "past limit": Offscreen(width=1 << 20, height=1).read(lambda _buffer: Ok(None)).map(lambda _: None)}


def handled(_identity: Identity) -> object:
    """Holds a draw handle on the 3D View main region, answering what the scope yielded."""
    with hooked(bpy.types.SpaceView3D, lambda: None, "WINDOW", "POST_PIXEL") as accepted:
        return accepted


def signalled(identity: Identity) -> dict[str, object]:
    """Fires a signal while a refresh follows it and after its scope released, and holds every refresh source kind and an absent msgbus key."""
    refreshes: list[str] = []

    def refreshed() -> Outcome[None]:
        refreshes.append("fired")
        return Ok(None)

    signal = Signal()
    with signal.followed(refreshed):
        inside = signal.fired()
    after = signal.fired()
    sources: tuple[Refresh, ...] = (
        (bpy.types.Scene, "frame_current"),
        Handled(handlers=bpy.app.handlers.depsgraph_update_post, ids=(bpy.types.Object,)),
        signal,
        Handled(handlers=bpy.app.handlers.frame_change_post),
    )
    with refreshing(identity, sources, lambda: Ok(None)) as held, refreshing(identity, ((bpy.types.Scene, "fixture_absent"),), lambda: Ok(None)) as absent:
        return {"inside": inside, "after": after, "refreshes": tuple(refreshes), "sources": held, "absent": absent}


# --- [COMPOSITION] ----------------------------------------------------------------------

OVERLAY: Final = Overlay(
    drawing=Drawing(
        area=AreaQuery(space=bpy.types.SpaceView3D),
        part=Marker(),
        refreshes=((bpy.types.Scene, "frame_current"), Handled(handlers=bpy.app.handlers.depsgraph_update_post, ids=(bpy.types.Object,)), Handled(handlers=bpy.app.handlers.frame_change_post)),
    )
)

rows: Final = (OVERLAY,)
PROBES: Final[Mapping[str, Callable[[Identity], object]]] = MappingProxyType({"charted": charted, "written": written, "read back": read_back, "handled": handled, "signalled": signalled})
RELEASED: Final[Mapping[str, Callable[[], object]]] = MappingProxyType({})

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["PROBES", "RELEASED", "rows"]
