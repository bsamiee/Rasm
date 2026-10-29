"""Draw handles, signals, and the refresh sources of drawings."""

from expression import Error, Ok

lazy from tests.python.libs.blender.bpy.conftest import Observations

# --- [OPERATIONS] -----------------------------------------------------------------------


def test_draw_handle_holds_on_a_space_region(observed: Observations) -> None:
    """A draw handle on the 3D View main region holds while its scope stands."""
    assert observed["drawing.handled"] == Ok(None)


def test_signal_refreshes_followers_while_held(observed: Observations) -> None:
    """A fired signal refreshes the drawing following it, and refreshes nothing once the follow released."""
    assert (observed["drawing.signalled", "inside"], observed["drawing.signalled", "after"], observed["drawing.signalled", "refreshes"]) == (Ok(None), Ok(None), ("fired",))


def test_refresh_sources_hold_under_one_scope(observed: Observations) -> None:
    """Watched msgbus keys, depsgraph updates, signals, and handler lists hold together under the deferred refresh scope, and a key naming no member faults."""
    assert (observed["drawing.signalled", "sources"], observed["drawing.signalled", "absent"]) == (Ok(None), Error(("ABSENT",)))
