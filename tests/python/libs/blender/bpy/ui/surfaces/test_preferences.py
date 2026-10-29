"""Preferences page gathering the form, confirmation flags, and stored-settings command every other row declares."""

from expression import Ok, Some

lazy from tests.python.libs.blender.bpy.conftest import Observations

# --- [OPERATIONS] -----------------------------------------------------------------------


def test_page_declares_form_fields_and_flat_ask_flags(observed: Observations) -> None:
    """The preferences class holds the form's fields beside one flat `ask_<key>` flag per suppressible command."""
    assert observed["surfaces.preferred", "properties"] == Some(("ask_confirmed", "hidden", "level"))


def test_page_reads_its_form_values(observed: Observations) -> None:
    """The preferences read as the form group, directly and through a value reader."""
    assert (observed["surfaces.preferred", "read"], observed["surfaces.preferred", "value"]) == (Ok(3), Ok(3))


def test_page_registers_its_commands(observed: Observations) -> None:
    """The page registers the command showing it and, with settings attached to data-blocks, the command purging them."""
    assert (observed["surfaces.preferred", "show"], observed["surfaces.preferred", "purge"]) == (True, True)


def test_ask_flag_drives_the_suppressible_command(observed: Observations) -> None:
    """The flag the page declares holds until a run with its checkbox set clears it."""
    assert (observed["commands.suppressed", "before"], observed["commands.suppressed", "after"]) == (Some(True), Some(False))
