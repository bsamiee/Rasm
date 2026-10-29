"""Operator calls, host names, and placements in the host keymap-data form."""

from expression import Error, Ok
import pytest

lazy from tests.python.libs.blender.bpy.conftest import Observations

# --- [OPERATIONS] -----------------------------------------------------------------------


@pytest.mark.parametrize(
    "case,name",
    [
        ("command", "fixture.counted"),
        ("menu", "FIXTURE_MT_main"),
        ("pie", "FIXTURE_MT_main"),
        ("panel", "FIXTURE_PT_main"),
        ("shelf", "FIXTURE_AST_main"),
        ("stock menu", "VIEW3D_MT_add"),
        ("stock operator", "wm.context_toggle"),
    ],
)
def test_targets_resolve_their_host_names(observed: Observations, case: str, name: str) -> None:
    """Commands, extension popups by key, stock classes, and stock operators resolve to the name the host calls them by."""
    assert observed["commands.named", case] == Ok(name)


@pytest.mark.parametrize(
    "case,placed",
    [
        ("command", ("fixture.counted", (("amount", 4),))),
        ("pie", ("wm.call_menu_pie", (("name", "VIEW3D_MT_pivot_pie"),))),
        ("toggle", ("wm.context_toggle", (("data_path", "scene.use_gravity"),))),
        ("assign", ("wm.context_set_value", (("data_path", "scene.frame_current"), ("value", "5")))),
    ],
)
def test_placements_resolve_to_keymap_data(observed: Observations, case: str, placed: object) -> None:
    """Placements resolve to an operator idname with property pairs, a popup through its call operator, a context value through toggle or assignment."""
    assert observed["commands.placed", case] == Ok(placed)


def test_filling_refuses_an_object_holding_no_properties(observed: Observations) -> None:
    """Placed values written to an object other than operator properties fault."""
    assert observed["commands.placed", "unfilled"] == Error(("ABSENT",))


def test_opened_refuses_a_class_outside_its_kind(observed: Observations) -> None:
    """A menu class opened as a panel raises `TypeError` at construction."""
    assert observed["commands.mismatched"] == "TypeError"


def test_calls_fault_on_absent_operators_and_bad_keywords(observed: Observations) -> None:
    """An operator the host lacks faults as absent, and a keyword the operator lacks as failed."""
    assert observed["commands.unbound"] == Error(("ABSENT",))
    assert observed["commands.executed", "bogus"] == (Error(("FAILED",)), 0)
