"""Native image handle coordinates, frame bounds, and picker registration lifetimes."""

import weakref

import bpy
from expression import Option, Result, Some
import numpy as np
import pytest

from blender.bpy.commands.operators import Answer, Call, called, Done, operation, rna_type
from blender.bpy.ui.drawing.canvas import Rect
from blender.bpy.ui.elements.panels import Sidebar
from blender.bpy.ui.images.views import coverage, FrameRegion, Picked, picking, RenderView, viewing
from blender.imaging.geometry.frames import Quad, Region
from blender.records.faults import Fault, Nothing, Ok, Outcome, Reason
from tests.python.libs.blender.bpy.background.session import Acquired

# --- [OPERATIONS] -----------------------------------------------------------------------


def test_coverage_upload_releases_its_source_plane() -> None:
    """Scalar coverage becomes associated white RGBA and its native texture releases the source plane."""
    source = np.arange(3 * 5, dtype=np.float32).reshape(3, 5)
    source /= 16
    height, width = source.shape
    expected = np.broadcast_to(source[..., None], (*source.shape, 4))[::-1].copy()
    reference = weakref.ref(source)
    uploaded = coverage(source)
    del source
    match uploaded:
        case Result(tag="ok", ok=texture):
            assert (texture.width, texture.height) == (width, height)
            assert reference() is None
            np.testing.assert_array_equal(np.asarray(texture.read()), expected)
        case found:
            pytest.fail(repr(found))


def test_render_view_command_preserves_missing_native_window_failure(acquired: Acquired) -> None:
    """Render view command registers and a context without a window retains its native operator failure."""
    view = RenderView(at=Sidebar(space="IMAGE_EDITOR", category="Fixture"))
    with acquired((view,)) as identity:
        command = view.command
        assert operation(identity, command).is_ok()
        with bpy.context.temp_override(window=None):
            assert viewing(bpy.context).swap().to_option().map(lambda faults: tuple(fault.reason for fault in faults)) == Some((Reason.ABSENT,))
            assert operation(identity, command).bind(lambda found: called(found, {})).swap().to_option().map(lambda faults: tuple((fault.reason, type(fault.cause)) for fault in faults)) == Some((
                (Reason.FAILED, RuntimeError),
            ))
    assert operation(identity, command).swap().to_option().map(lambda faults: tuple((fault.reason, type(fault.cause)) for fault in faults)) == Some(((Reason.ABSENT, KeyError),))


def test_picker_registers_native_cursor_properties_and_releases(acquired: Acquired) -> None:
    """Picker registers its native invoke contract and releases the operator after its scope."""

    def sampled(_call: Call[Picked], _region: Region) -> Outcome[Answer]:
        return Ok(Done())

    pick = picking("EYEDROPPER", Picked, sampled)
    with acquired((pick,)) as identity:
        match operation(identity, pick):
            case Result(tag="ok", ok=found):
                rna = rna_type(found)
            case failed:
                pytest.fail(repr(failed))
        name = rna.identifier
        native = bpy.types.Operator.bl_rna_get_subclass_py(name)
        assert native is not None
        assert native.bl_options == {"DEPENDS_ON_CURSOR", "REGISTER", "UNDO"}
        assert native.bl_cursor_pending == "EYEDROPPER"
        assert set(rna.properties.keys()) - {"rna_type"} == {"radius", "x", "y"}
    assert bpy.types.Operator.bl_rna_get_subclass_py(name) is None


def test_absent_native_region_produces_no_snapshot_and_refuses_edits() -> None:
    """Missing image region hides handles and preserves the frame key in an attempted edit fault."""
    handles = FrameRegion(key="frame", read=lambda _context: Ok(Some(Quad())), write=lambda _context, _quad: Ok(None))
    window = bpy.data.window_managers[0].windows[0]
    with bpy.context.temp_override(window=window, area=None, region=None):
        assert handles.held(bpy.context) == Ok(Nothing)
        match handles.caged(bpy.context, tuple(np.identity(4).ravel())):
            case Result(error=(Fault(reason=reason, member=member),)):
                assert (reason, member) == (Reason.ABSENT, handles.key)
            case found:
                pytest.fail(repr(found))


def test_frame_handles_share_one_snapshot_coordinate_system() -> None:
    """Cage target, corner offsets, and origin map the same quad into frame pixels."""
    quad = Quad()
    frame = Rect(x=10.0, y=20.0, width=80.0, height=40.0)
    state = quad, frame
    corners = np.asarray(tuple(FrameRegion.corner(index, state) for index in range(len(quad.corners))))
    np.testing.assert_array_equal(corners[:, :2], np.asarray(quad.corners) * (frame.width, frame.height))
    np.testing.assert_array_equal(corners[:, 2], np.zeros(len(quad.corners)))
    cage = np.reshape(FrameRegion.caging(state), (4, 4), order="F")
    local = np.column_stack((np.asarray(quad.corners) - 0.5, np.zeros(len(quad.corners)), np.ones(len(quad.corners))))
    np.testing.assert_array_equal((local @ cage.T)[:, :2], corners[:, :2])
    match FrameRegion.origin(state):
        case Result(tag="ok", ok=Option(some=matrix)):
            expected = np.identity(4)
            expected[:2, 3] = (frame.x, frame.y)
            np.testing.assert_array_equal(matrix, expected)
        case found:
            pytest.fail(repr(found))


def test_bounded_whole_quad_move_preserves_every_edge() -> None:
    """Bounded translation preserves shape and moves the whole quad against its limiting side."""
    bounds = Region.bounding(np.array(((0.0, 0.0), (1.0, 1.0))))
    handles = FrameRegion(key="bounded", read=lambda _context: Ok(Nothing), write=lambda _context, _quad: Ok(None), bounds=Some(bounds))
    source = np.array(((0.8, 0.9), (1.2, 0.9), (1.2, 0.5), (0.8, 0.5)), dtype=np.float64)
    moved = handles.bounded(source, shifted=True)
    tolerance = np.finfo(source.dtype).eps.item()
    np.testing.assert_allclose(np.diff(moved, axis=0), np.diff(source, axis=0), rtol=tolerance, atol=tolerance)
    np.testing.assert_allclose(moved - source, np.broadcast_to(moved[0] - source[0], moved.shape), rtol=tolerance, atol=tolerance)
    assert np.all(moved >= (bounds.left, bounds.bottom))
    assert np.all(moved <= (bounds.right, bounds.top))
    assert moved[:, 0].max() == bounds.right


def test_corner_stretch_clips_outside_components_and_keeps_inside_components() -> None:
    """Corner bounds clip independently and an unbounded handle preserves its input."""
    bounds = Region.bounding(np.array(((0.0, 0.0), (1.0, 1.0))))
    handles = FrameRegion(key="bounded", read=lambda _context: Ok(Nothing), write=lambda _context, _quad: Ok(None), bounds=Some(bounds))
    source = np.array(((-0.2, 1.4), (1.2, 1.4), (1.2, 0.5), (-0.2, 0.5)))
    stretched = handles.bounded(source, shifted=False)
    lower, upper = np.array((bounds.left, bounds.bottom)), np.array((bounds.right, bounds.top))
    inside = (source >= lower) & (source <= upper)
    np.testing.assert_array_equal(stretched[inside], source[inside])
    np.testing.assert_array_equal(stretched[source < lower], np.broadcast_to(lower, source.shape)[source < lower])
    np.testing.assert_array_equal(stretched[source > upper], np.broadcast_to(upper, source.shape)[source > upper])
    loose = FrameRegion(key="loose", read=lambda _context: Ok(Nothing), write=lambda _context, _quad: Ok(None))
    np.testing.assert_array_equal(loose.bounded(source, shifted=True), source)
