"""Modal invoke form: gesture flags and the modal class it derives."""

import pytest

lazy from tests.python.libs.blender.bpy.conftest import Observations

# --- [OPERATIONS] -----------------------------------------------------------------------


@pytest.mark.parametrize(
    "gesture,options", [("DRAG", ("BLOCKING", "GRAB_CURSOR")), ("DRAG_X", ("BLOCKING", "GRAB_CURSOR_X")), ("DRAG_Y", ("BLOCKING", "GRAB_CURSOR_Y")), ("TRACK", ("BLOCKING",)), ("TIMED", ())]
)
def test_gestures_declare_their_flags(observed: Observations, gesture: str, options: tuple[str, ...]) -> None:
    """Cursor gestures block, drags grab along their axes, and a timed run declares neither."""
    assert observed["commands.gestures", gesture] == options


def test_modal_form_derives_modal_callbacks_and_flags(observed: Observations) -> None:
    """The modal form adds `invoke`, `modal`, and `cancel` with its gesture flags and modal priority."""
    assert observed["commands.classes", "dragged", "options"] == ("BLOCKING", "GRAB_CURSOR", "MODAL_PRIORITY", "REGISTER", "UNDO")
    assert observed["commands.classes", "dragged", "members"] == ("cancel", "invoke", "modal")
