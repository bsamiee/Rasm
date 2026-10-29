"""Invoke forms: window manager popups, suppressible confirmation, file and folder browsers, and cursor picks derived onto the command's class."""

from expression import Error, Ok, Some
import pytest

lazy from tests.python.libs.blender.bpy.conftest import Observations

# --- [OPERATIONS] -----------------------------------------------------------------------


@pytest.mark.parametrize(
    "key,options,members,properties",
    [
        ("dialog", ("REGISTER", "UNDO"), ("invoke",), ("amount", "mode")),
        ("redo", ("REGISTER", "UNDO"), ("invoke",), ("amount", "mode")),
        ("search", ("REGISTER", "UNDO"), ("invoke",), ("amount", "mode")),
        ("confirmed", ("REGISTER", "UNDO"), ("draw", "invoke"), ("amount", "mode", "suppress")),
        ("opened", ("REGISTER", "UNDO"), ("check", "invoke"), ("directory", "filepath", "files", "filter_glob")),
        ("saved", ("REGISTER", "UNDO"), ("check", "invoke"), ("check_existing", "filepath", "filter_glob")),
        ("folder", ("REGISTER", "UNDO"), ("invoke",), ("directory", "filter_folder")),
        ("picked", ("DEPENDS_ON_CURSOR", "REGISTER", "UNDO"), (), ()),
    ],
)
def test_forms_derive_their_class(observed: Observations, key: str, options: tuple[str, ...], members: tuple[str, ...], properties: tuple[str, ...]) -> None:
    """Each form adds its callbacks and properties, a redo popup adds the `UNDO` its command left out, and a pick defers through `DEPENDS_ON_CURSOR`."""
    assert observed["commands.classes", key, "options"] == options
    assert observed["commands.classes", key, "members"] == members
    assert observed["commands.classes", key, "properties"] == properties


def test_file_forms_extend_the_host_helpers(observed: Observations) -> None:
    """The open form extends `ImportHelper` and the save form `ExportHelper`."""
    assert (observed["commands.classes", "opened", "helpers"], observed["commands.classes", "saved", "helpers"]) == (("ImportHelper",), ("ExportHelper",))


def test_pick_shows_its_pending_cursor(observed: Observations) -> None:
    """A pick names the cursor the host shows while the pick is pending."""
    assert observed["commands.classes", "picked", "cursor"] == Some("EYEDROPPER")


def test_suppressed_confirmation_clears_its_preference(observed: Observations) -> None:
    """A run with its checkbox set clears the flat `ask_<key>` preference the confirmation reads."""
    assert observed["commands.suppressed", "before", "some"] is True
    assert observed["commands.suppressed", "ran"] == (Ok(("FINISHED",)), 1)
    assert observed["commands.suppressed", "after", "some"] is False


def test_browsers_run_the_body_over_chosen_paths(observed: Observations) -> None:
    """A folder run without a directory fails for its unchosen path, and folder and file runs pass the chosen path to the body."""
    assert observed["commands.browsed"] == {"unchosen": Error(("FAILED",)), "folder": Ok(("FINISHED",)), "opened": Ok(("FINISHED",)), "paths": True}
