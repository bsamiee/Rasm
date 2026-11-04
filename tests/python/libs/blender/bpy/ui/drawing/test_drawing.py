"""Native framebuffer pixels, chart placement, GPU failure cleanup, and refresh lifetimes."""

from contextlib import ExitStack

import bpy
from expression import Result, Some
import gpu
from gpu_extras.batch import batch_for_shader
import numpy as np
import pytest

from blender.bpy.ui.drawing.canvas import Align, Canvas, captured, draw_batch, FontStyle, Offscreen, Rect
from blender.bpy.ui.drawing.charts import Chart, Layer, TRACE
from blender.bpy.ui.drawing.handlers import HandlerRefresh, refreshing, Signal
from blender.bpy.ui.resources.theme import Checker
from blender.records.faults import Fault, Ok, Outcome, Reason
from tests.python.libs.blender.bpy.background.session import Acquired

# --- [OPERATIONS] -----------------------------------------------------------------------


def test_text_extents_and_drawn_glyphs_use_native_font(canvas: Canvas) -> None:
    """Longer text grows horizontally, wrapped text grows vertically, and glyphs cover pixels."""
    style = FontStyle(size=14.0)
    width, height = canvas.measured("Fixture", style)
    assert width > 0
    assert height > 0
    assert canvas.measured("Fixture Fixture", style)[0] > width
    assert canvas.measured("Fixture Fixture Fixture", FontStyle(size=14.0, wrap=Some(width)))[1] > height
    match Offscreen(width=32, height=32).drawn(canvas, lambda pixels: pixels.label("F", (4.0, 4.0), (1.0, 1.0, 1.0, 1.0), FontStyle(size=24.0))):
        case Result(tag="ok", ok=plane):
            assert plane[..., 3].max() == 1.0
            assert np.any(plane[..., :3] > 0.0)
        case failed:
            pytest.fail(repr(failed))


def test_checker_cells_and_empty_extents_draw_native_fragments(canvas: Canvas) -> None:
    """Native checker alternates opaque cells and empty rectangles leave cleared pixels unchanged."""
    checker = Checker(primary=(1.0, 1.0, 1.0, 1.0), secondary=(0.0, 0.0, 0.0, 1.0), cell=4)
    match Offscreen(width=8, height=4).drawn(canvas, lambda pixels: pixels.checkered(Rect(x=0.0, y=0.0, width=8.0, height=4.0), checker)):
        case Result(tag="ok", ok=plane):
            np.testing.assert_array_equal(plane[:, :4], np.broadcast_to((0.0, 0.0, 0.0, 1.0), plane[:, :4].shape))
            np.testing.assert_array_equal(plane[:, 4:], np.broadcast_to((1.0, 1.0, 1.0, 1.0), plane[:, 4:].shape))
        case failed:
            pytest.fail(repr(failed))
    for width, height in ((0.0, 4.0), (4.0, 0.0)):
        match Offscreen(width=8, height=8).drawn(canvas, lambda pixels: pixels.checkered(Rect(x=0.0, y=0.0, width=width, height=height), checker)):
            case Result(tag="ok", ok=plane):
                np.testing.assert_array_equal(plane, np.zeros_like(plane))
            case failed:
                pytest.fail(repr(failed))


@pytest.mark.parametrize("corner", [Align.START, Align.END])
def test_chart_draws_its_well_and_trace_at_native_panel_inset(canvas: Canvas, corner: Align) -> None:
    """Opaque well follows its corner and the trace crosses the well away from text."""
    diagonal = np.array(((0.0, 0.0), (1.0, 1.0)), np.float32)
    midtone = 0.5
    chart = Chart(width=40.0, height=30.0, layers=(Layer.of("LINE_STRIP", diagonal, TRACE, width=2.0),), corner=corner)
    match Offscreen(width=64, height=48).drawn(canvas, chart.drawn):
        case Result(tag="ok", ok=plane):
            rows, columns = np.nonzero(plane[..., 3] >= midtone)
            top, bottom, left, right = int(rows.min()), int(rows.max()), int(columns.min()), int(columns.max())
            assert (bottom - top + 1, right - left + 1) == (32, 42)
            inset = int(canvas.metrics.margin)
            assert bottom == plane.shape[0] - 1 - inset
            assert (left if corner is Align.START else plane.shape[1] - 1 - right) == inset
            midpoint = plane[(top + bottom) // 2 - 1 : (top + bottom) // 2 + 2, (left + right) // 2 - 1 : (left + right) // 2 + 2]
            assert np.any(midpoint[..., :3] > midtone)
        case failed:
            pytest.fail(repr(failed))


def test_framebuffer_capture_reads_requested_extent() -> None:
    """Native framebuffer callbacks expose allocated dimensions and capture preserves all channels."""

    def drawn(buffer: gpu.types.GPUFrameBuffer) -> Outcome[None]:
        assert buffer.viewport_get() == (0, 0, 8, 8)
        buffer.clear(color=(0.0, 1.0, 0.0, 1.0))
        match captured(Rect(x=0.0, y=0.0, width=2.0, height=3.0)):
            case Result(tag="ok", ok=plane):
                assert plane.shape == (3, 2, 4)
                np.testing.assert_array_equal(plane, np.broadcast_to((0.0, 1.0, 0.0, 1.0), plane.shape))
            case failed:
                pytest.fail(repr(failed))
        return Ok(None)

    match Offscreen(width=8, height=8, format="RGBA16F").read(drawn):
        case Result(tag="ok", ok=plane):
            np.testing.assert_array_equal(plane, np.broadcast_to((0.0, 1.0, 0.0, 1.0), plane.shape))
        case failed:
            pytest.fail(repr(failed))


def test_throwing_draw_restores_native_state_and_outer_framebuffer(canvas: Canvas) -> None:
    """Draw failure restores matrices, blend, depth, scissor, and an already bound outer framebuffer."""

    def raised(_canvas: Canvas) -> None:
        raise ValueError("fixture draw")

    gpu.init()
    outer = gpu.types.GPUOffScreen(8, 8, format="RGBA32F")
    with ExitStack() as scope:
        scope.callback(outer.free)
        scope.callback(gpu.state.viewport_set, *gpu.state.viewport_get())
        scope.callback(gpu.state.scissor_set, *gpu.state.scissor_get())
        scope.enter_context(outer.bind())  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
        scope.enter_context(gpu.matrix.push_pop())  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
        scope.enter_context(gpu.matrix.push_pop_projection())  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
        scope.callback(gpu.state.blend_set, gpu.state.blend_get())  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
        scope.callback(gpu.state.depth_test_set, gpu.state.depth_test_get())  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
        gpu.state.scissor_set(0, 0, outer.width, outer.height)
        gpu.state.active_framebuffer_get().clear(color=(0.25, 0.5, 0.75, 1.0))
        gpu.state.viewport_set(1, 2, 5, 4)
        gpu.state.blend_set("MULTIPLY")
        gpu.state.depth_test_set("GREATER")
        gpu.state.scissor_set(1, 1, 6, 6)
        gpu.matrix.translate((2.0, 3.0, 4.0))
        before = gpu.matrix.get_model_view_matrix(), gpu.matrix.get_projection_matrix()
        with pytest.raises(ValueError, match="fixture draw"):
            assert Offscreen(width=4, height=4).drawn(canvas, raised).is_ok()
        assert (gpu.state.blend_get(), gpu.state.depth_test_get(), gpu.state.scissor_get(), gpu.state.viewport_get()) == ("MULTIPLY", "GREATER", (1, 1, 6, 6), (1, 2, 5, 4))
        assert (gpu.matrix.get_model_view_matrix(), gpu.matrix.get_projection_matrix()) == before
        match Offscreen(width=0, height=0).drawn(canvas, lambda _pixels: None):
            case Result(tag="ok", ok=plane):
                assert plane.shape == (1, 1, 4)
            case failed:
                pytest.fail(repr(failed))
        for width, kind in ((gpu.capabilities.max_texture_size_get() + 1, RuntimeError), (int(np.iinfo(np.intc).max) + 1, TypeError)):
            match Offscreen(width=width, height=1).read(lambda _buffer: Ok(None)):
                case Result(error=(Fault(reason=reason, subject=subject, cause=cause),)):
                    assert (reason, subject) == (Reason.REFUSED, Offscreen.__name__)
                    assert isinstance(cause, kind)
                case failed:
                    pytest.fail(repr(failed))
            assert (gpu.state.scissor_get(), gpu.state.viewport_get()) == ((1, 1, 6, 6), (1, 2, 5, 4))
        match captured(Rect(x=0.0, y=0.0, width=2.0, height=2.0)):
            case Result(tag="ok", ok=plane):
                np.testing.assert_array_equal(plane, np.broadcast_to((0.25, 0.5, 0.75, 1.0), plane.shape))
            case failed:
                pytest.fail(repr(failed))


def test_each_batch_reapplies_its_native_clip_planes(canvas: Canvas) -> None:
    """Stored programs reapply native clip planes and the model matrix at each actual draw."""
    space, view = canvas.space, canvas.region.data
    assert isinstance(space, bpy.types.SpaceView3D)
    assert isinstance(view, bpy.types.RegionView3D)
    attribute, positions = "pos", ((-1.0, -1.0, 0.0), (3.0, -1.0, 0.0), (-1.0, 3.0, 0.0))
    with ExitStack() as scope:
        scope.callback(setattr, space.shading, "type", space.shading.type)
        scope.callback(setattr, view, "use_clip_planes", view.use_clip_planes)
        scope.callback(setattr, view, "clip_planes", tuple(tuple(plane) for plane in view.clip_planes))
        space.shading.type = "SOLID"
        view.use_clip_planes = True
        planes = ((1.0, 0.0, 0.0, 0.0), (-1.0, 0.0, 0.0, 2.0), (0.0, 1.0, 0.0, 2.0), (0.0, -1.0, 0.0, 2.0), (0.0, 0.0, 1.0, 2.0), (0.0, 0.0, -1.0, 2.0))
        view.clip_planes = planes
        area = next(area for area in bpy.data.window_managers[0].windows[0].screen.areas if area.spaces.active == space)
        clipped = Canvas.of(area, canvas.region, "POST_VIEW")

        def drawn(buffer: gpu.types.GPUFrameBuffer) -> Outcome[None]:
            buffer.clear(color=(0.0, 0.0, 0.0, 1.0))
            with clipped.drawn(), Canvas.blended("NONE"), Canvas.depth_tested("NONE"):
                gpu.matrix.load_matrix(view.view_matrix)
                gpu.matrix.load_projection_matrix(view.view_matrix.inverted())
                outer = clipped.shader("UNIFORM_COLOR")
                batch = batch_for_shader(outer[0], "TRIS", {attribute: positions})
                inner = canvas.shader("UNIFORM_COLOR")
                inner[0].uniform_float("color", (0.0, 1.0, 0.0, 1.0))
                draw_batch(batch_for_shader(inner[0], "TRIS", {attribute: positions}), inner)
                outer[0].uniform_float("color", (1.0, 0.0, 0.0, 1.0))
                draw_batch(batch, outer)
            return Ok(None)

        match Offscreen(width=16, height=8).read(drawn):
            case Result(tag="ok", ok=plane):
                np.testing.assert_array_equal(plane[4, 4], (0.0, 1.0, 0.0, 1.0))
                np.testing.assert_array_equal(plane[4, 12], (1.0, 0.0, 0.0, 1.0))
            case failed:
                pytest.fail(repr(failed))
        view.clip_planes = (tuple(-value for value in planes[0]), *planes[1:])
        opposite = Canvas.of(area, canvas.region, "POST_VIEW")

        def mapped(buffer: gpu.types.GPUFrameBuffer) -> Outcome[None]:
            buffer.clear(color=(0.0, 0.0, 0.0, 1.0))
            with clipped.drawn(), Canvas.blended("NONE"), Canvas.depth_tested("NONE"):
                gpu.matrix.load_matrix(view.view_matrix)
                gpu.matrix.load_projection_matrix(view.view_matrix.inverted())
                outer = clipped.shader("UNIFORM_COLOR")
                inner = opposite.shader("UNIFORM_COLOR")
                inner[0].uniform_float("color", (0.0, 1.0, 0.0, 1.0))
                draw_batch(batch_for_shader(inner[0], "TRIS", {attribute: positions}), inner)
                gpu.matrix.translate((0.5, 0.0, 0.0))
                outer[0].uniform_float("color", (1.0, 0.0, 0.0, 1.0))
                draw_batch(batch_for_shader(outer[0], "TRIS", {attribute: positions}), outer)
            return Ok(None)

        match Offscreen(width=16, height=8).read(mapped):
            case Result(tag="ok", ok=plane):
                np.testing.assert_array_equal(plane[4, 4], (0.0, 1.0, 0.0, 1.0))
                np.testing.assert_array_equal(plane[4, (9, 12)], ((1.0, 0.0, 0.0, 1.0),) * 2)
            case failed:
                pytest.fail(repr(failed))


def test_refresh_sources_release_native_subscriptions(acquired: Acquired) -> None:
    """Signal callbacks stop after release and handler sources restore native handler lists."""
    calls: list[int] = []
    signal = Signal()
    before = tuple(bpy.app.handlers.frame_change_post)

    def refreshed() -> Outcome[None]:
        calls.append(1)
        return Ok(None)

    with acquired(()) as identity:
        sources = (signal, HandlerRefresh(handlers=bpy.app.handlers.frame_change_post))
        with refreshing(identity, sources, refreshed) as held:
            assert held == Ok(None)
            assert signal.fired() == Ok(None)
        assert signal.fired() == Ok(None)
        assert calls == [1]
    assert tuple(bpy.app.handlers.frame_change_post) == before


# --- [COMPOSITION] ----------------------------------------------------------------------


@pytest.fixture
def canvas() -> Canvas:
    """Returns a canvas over the native 3D View main region."""
    gpu.init()
    window = bpy.data.window_managers[0].windows[0]
    area = next(area for area in window.screen.areas if area.type == "VIEW_3D")
    return Canvas.of(area, next(region for region in area.regions if region.type == "WINDOW"))
