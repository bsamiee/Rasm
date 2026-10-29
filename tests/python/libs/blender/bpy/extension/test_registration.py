"""Registration service: band order and reverse release, dependency-ordered classes, translations, manual links, command-line commands, and refused registrations unwinding."""

from typing import Final

from expression import Error, Nothing, Ok
import pytest

lazy from tests.python.libs.blender.bpy.conftest import Observations

# --- [CONSTANTS] ------------------------------------------------------------------------

COMMANDS: Final = ("confirmed", "counted", "dialog", "dragged", "folder", "grouped", "kept", "opened", "picked", "plain", "redo", "refused", "saved", "search", "seeded", "twice")
ENTERED: Final = ("enter GROUPS", "enter INTERFACE", "enter TIMERS")

# --- [OPERATIONS] -----------------------------------------------------------------------


def test_bands_enter_in_band_order_and_leave_in_reverse(observed: Observations, released: Observations) -> None:
    """Scopes one row declares out of band order enter by band and leave in reverse at release."""
    assert observed["extension.ordered", "events"] == ENTERED
    assert released["extension.order", "events"] == (*ENTERED, "exit TIMERS", "exit INTERFACE", "exit GROUPS")


def test_classes_register_after_their_targets_and_release(observed: Observations, released: Observations) -> None:
    """A child panel listed before its parent and a group listed before its pointer target both register, and every class leaves the host at release."""
    names = ("ChildPanel", "ParentPanel", "Outer", "Inner")
    assert observed["extension.ordered", "registered"] == dict.fromkeys(names, True)
    assert observed["extension.ordered", "parent"] == "FIXTURE_PT_child"
    assert released["extension.order", "registered"] == dict.fromkeys(names, False)


def test_release_leaves_handler_lists_as_found(released: Observations) -> None:
    """Load and depsgraph handler lists return to their size before registration."""
    assert released["extension.order", "handlers"] is True


def test_translations_hold_while_registered(observed: Observations, released: Observations) -> None:
    """Interface text translates under the French interface while the scope stands, and stays untranslated after release."""
    assert observed["extension.translated"] == "Bonjour Fixture"
    assert released["extension.texts", "translated"] == "Fixture Hello"


def test_manual_map_links_operators_types_and_preferences(observed: Observations, released: Observations) -> None:
    """The manifest website maps every extension operator, extension types and members, the preferences struct, and attached groups, and maps nothing after release."""
    patterns = frozenset(observed.of(tuple, "extension.manual"))
    expected = {("bpy.types.fixture_*", ""), ("bpy.types.*.fixture_*", ""), ("bpy.types.bl_ext.user_default.fixture.*", ""), ("bpy.types.look.*", "")}
    assert {*((f"bpy.ops.fixture.{key}", f"#{key}") for key in COMMANDS), *expected} <= patterns
    assert released["extension.texts", "patterns"] == ()


def test_preferences_read_absent_after_release(released: Observations) -> None:
    """Add-on preferences read absent once the extension is disabled."""
    assert released["extension.texts", "preferences"] == Nothing


@pytest.mark.parametrize("panel", ["orphan", "drawless"])
def test_refused_class_unwinds_earlier_acquisitions(released: Observations, panel: str) -> None:
    """A panel the host refuses, for a missing parent or a missing `draw`, raises the scope's `RuntimeError` after the operator registered before it and the scope's load handler are released."""
    assert released[f"extension.{panel}"] == {"raised": "RuntimeError", "operators": (), "panel": False, "handlers": True}


def test_command_decodes_its_document(observed: Observations) -> None:
    """An encoded document runs the command, no argument faults as absent, and an undecodable one as unreadable."""
    assert observed["commands.commanded"] == {"code": Ok(3), "missing": Error(("ABSENT",)), "garbled": Error(("UNREADABLE",))}
