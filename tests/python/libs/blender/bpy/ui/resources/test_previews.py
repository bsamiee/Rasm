"""Generated preview tiles: slot writes, pixelless requests, drops, and checker compositing."""

from expression import Nothing, Ok

lazy from tests.python.libs.blender.bpy.conftest import Observations

# --- [OPERATIONS] -----------------------------------------------------------------------


def test_slots_write_drop_and_finish_requests(observed: Observations) -> None:
    """A plane writes the slot at its size and finishes its request, a pixelless request drops the pixels yet finishes, and a drop clears both."""
    assert observed["resources.previewed"] == {
        "written": Ok(True),
        "after write": (True, True, (8, 4)),
        "emptied": Ok(Nothing),
        "after empty": (True, Nothing),
        "cleared": Ok(None),
        "after clear": False,
    }


def test_tiles_composite_over_the_checker(observed: Observations) -> None:
    """A transparent plane shows the checker's two-pixel cells, an opaque plane covers it, every output pixel is opaque, and requests default to the host preview size."""
    assert observed["resources.checkered"] == {
        "transparent": ((1.0, 1.0, 0.0, 0.0), (1.0, 1.0, 0.0, 0.0), (0.0, 0.0, 1.0, 1.0), (0.0, 0.0, 1.0, 1.0)),
        "opaque": (1.0, 0.0, 0.0, 1.0),
        "alpha": 1.0,
        "side": True,
    }
