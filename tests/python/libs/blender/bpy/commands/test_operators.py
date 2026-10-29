"""Operator and macro classes: names, texts, and flags derived from the command, primary property, body answers as host returns and reports, and folded item edits."""

from expression import Error, Nothing, Ok, Some
import pytest

lazy from tests.python.libs.blender.bpy.conftest import Observations

# --- [OPERATIONS] -----------------------------------------------------------------------


@pytest.mark.parametrize(
    "key,options,properties",
    [
        ("counted", ("REGISTER", "UNDO"), ("amount", "mode")),
        ("kept", ("REGISTER", "UNDO"), ()),
        ("grouped", ("REGISTER", "UNDO_GROUPED"), ()),
        ("plain", ("INTERNAL", "PRESET", "REGISTER"), ()),
        ("seeded", ("REGISTER", "UNDO"), ("amount", "mode")),
        ("twice", ("REGISTER", "UNDO"), ("FIXTURE_OT_counted", "FIXTURE_OT_counted")),
    ],
)
def test_commands_register_their_flags_and_form(observed: Observations, key: str, options: tuple[str, ...], properties: tuple[str, ...]) -> None:
    """Undo, grouped undo, presets, and internal declarations become `bl_options`, form properties become operator properties, and macro steps become its sub-operators."""
    assert observed["commands.classes", key, "options"] == options
    assert observed["commands.classes", key, "properties"] == properties


def test_texts_derive_from_key_and_docstring(observed: Observations) -> None:
    """The label is the key's display name and the description the docstring's first paragraph without its period, a macro taking its stated description."""
    assert (observed["commands.classes", "counted", "label"], observed["commands.classes", "counted", "description"]) == ("Counted", "Adds the amount to the scene count, or sets the count to it")
    assert (observed["commands.classes", "twice", "label"], observed["commands.classes", "twice", "description"]) == ("Twice", "Counts by two, then by three")


def test_single_enum_becomes_the_primary_property(observed: Observations) -> None:
    """A form holding one enum names it `bl_property`, and a form with none leaves it unset."""
    assert (observed["commands.classes", "counted", "property"], observed["commands.classes", "kept", "property"]) == (Some("mode"), Nothing)


def test_grouped_undo_names_its_group(observed: Observations) -> None:
    """A string undo declaration becomes `bl_undo_group`."""
    assert observed["commands.classes", "grouped", "undo_group"] == Some("fixture")


def test_seed_adds_an_invoke(observed: Observations) -> None:
    """A seed adds `invoke` to a command with no invoke form, and a plain command holds no callback beyond `execute`."""
    assert (observed["commands.classes", "seeded", "members"], observed["commands.classes", "counted", "members"]) == (("invoke",), ())


@pytest.mark.parametrize(
    "case,answer,count",
    [
        ("counted", Ok(("FINISHED",)), 3),
        ("set", Ok(("FINISHED",)), 3),
        ("unset", Ok(("FINISHED",)), 1),
        ("kept", Error(("ANSWERED",)), 0),
        ("refused", Error(("FAILED",)), 0),
        ("macro", Ok(("FINISHED",)), 5),
    ],
)
def test_bodies_answer_through_the_host(observed: Observations, case: str, answer: object, count: int) -> None:
    """`Done` finishes, `Unchanged` cancels, faults cancel with an error the host raises, unset values take their defaults, and a macro runs its steps in order."""
    assert observed["commands.executed", case] == (answer, count)


def test_faults_report_their_rendered_text(observed: Observations) -> None:
    """A refused body reports its fault's text at its report type."""
    assert observed["commands.refusal"] == "Invalid Context Error: Requires a fixture strip\n"


def test_item_edits_fold_into_one_answer(observed: Observations) -> None:
    """Edits all changed finish, some refused finish skipping their faults, and all refused fail with every fault."""
    assert observed["commands.answered"] == {"all": Ok({"notes": (), "skipped": Nothing}), "some": Ok({"notes": (), "skipped": Some(("ABSENT",))}), "none": Error(("ABSENT", "ABSENT"))}


def test_release_removes_every_operator(released: Observations) -> None:
    """No extension operator stays registered after release."""
    assert released["commands.operators"] == ()
