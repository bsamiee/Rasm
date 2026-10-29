"""Image editor views: the render view command, the editor showing Render Result, and the pick command's deferred invocation and properties."""

from expression import Error, Some

lazy from tests.python.libs.blender.bpy.conftest import Observations

# --- [OPERATIONS] -----------------------------------------------------------------------


def test_render_view_command_registers(observed: Observations) -> None:
    """The render view row registers its command."""
    assert (observed["images.viewed", "command"], observed["images.viewed", "operation"]) == (True, True)


def test_no_editor_shows_render_result_in_a_fresh_file(observed: Observations) -> None:
    """With no image editor showing Render Result the lookup faults as absent."""
    assert observed["images.viewed", "viewing"] == Error(("ABSENT",))


def test_pick_defers_through_the_host_cursor(observed: Observations, released: Observations) -> None:
    """A pick command defers its invocation until the pointer picks under the pending cursor, seeds its point on invoke, and leaves with release."""
    assert tuple(observed["images.picked", member] for member in ("options", "cursor", "members", "properties")) == (
        ("DEPENDS_ON_CURSOR", "REGISTER", "UNDO"),
        Some("EYEDROPPER"),
        ("invoke",),
        ("radius", "x", "y"),
    )
    assert released["images.images", "pick"] is False
