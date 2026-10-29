"""Layout rows realized once per extension identity: bound members, operator buttons, openers, labels, sub-layouts with plain draws, and keyword checks against UILayout functions."""

from expression import Error, Ok
import pytest

lazy from tests.python.libs.blender.bpy.conftest import Observations

# --- [OPERATIONS] -----------------------------------------------------------------------


@pytest.mark.parametrize(
    "case,realized",
    [
        ("present", Ok(True)),
        ("element", Ok(True)),
        ("reached", Ok(True)),
        ("glyph", Ok(True)),
        ("function", Ok(True)),
        ("absent", Error(("ABSENT",))),
        ("keyword", Error(("OUTSIDE",))),
        ("unknown glyph", Error(("ABSENT",))),
    ],
)
def test_property_rows_bind_their_members(observed: Observations, case: str, realized: object) -> None:
    """Members, elements, members reached through context, glyphs, and other layout functions realize, and absent members, foreign keywords, and unknown glyphs fault."""
    assert observed["layout.bound", case] == realized


@pytest.mark.parametrize("case,realized", [("command", Ok(True)), ("placement", Ok(True)), ("menu", Ok(True)), ("panel", Ok(True)), ("keyword", Error(("OUTSIDE",))), ("pie", Error(("ABSENT",)))])
def test_buttons_and_openers_realize(observed: Observations, case: str, realized: object) -> None:
    """Command and placement buttons and menu and panel openers realize, a foreign keyword faults, and a pie, which opens through its operator alone, has no opener."""
    assert observed["layout.pressed", case] == realized


def test_command_buttons_acquire_their_command(observed: Observations) -> None:
    """A button over an extension command registers that command."""
    assert observed["layout.pressed", "acquired"] == ("Operator",)


def test_labels_realize(observed: Observations) -> None:
    """Fixed and read labels, a label through another label function, and the report status realize, and a foreign keyword faults."""
    assert observed["layout.labelled"] == {"fixed": Ok(True), "read": Ok(True), "single": Ok(True), "keyword": Error(("OUTSIDE",)), "status": Ok(True)}


def test_layouts_compose_rows_and_plain_draws(observed: Observations) -> None:
    """A container of rows and plain draws and a layout built per identity realize, register the rows holding a command, and accumulate every refusal of their rows."""
    assert observed["layout.composed"] == {"layout": Ok(True), "built": Ok(True), "acquired": ("OperatorButton",), "refusals": Error(("OUTSIDE", "ABSENT"))}


def test_keywords_check_against_function_parameters(observed: Observations) -> None:
    """Keywords a layout function names pass unchanged, and others fault."""
    assert observed["layout.checked"] == {"prop": Ok({"text": "Fixture", "slider": True}), "unknown": Error(("OUTSIDE",))}
