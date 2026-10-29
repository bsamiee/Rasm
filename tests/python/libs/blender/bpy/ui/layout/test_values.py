"""Value stores and preset listings: their verb commands, history steps over the context host, and preset apply, step, and save."""

from expression import Error, Ok, Some

lazy from tests.python.libs.blender.bpy.conftest import Observations

# --- [OPERATIONS] -----------------------------------------------------------------------


def test_store_registers_one_command_per_verb(observed: Observations) -> None:
    """A store without presets registers its reset, clipboard, and history verbs."""
    assert observed["sections.stored", "verbs"] == ("COPY", "FORGET", "PASTE", "RESET", "RESTORE")


def test_store_resets_and_steps_through_history(observed: Observations) -> None:
    """Reset restores defaults and records the prior values, older restores them, and newer returns to the reset values."""
    assert observed["sections.stored", "reset"] == (Ok(("FINISHED",)), 1.0)
    assert observed["sections.stored", "older"] == (Ok(("FINISHED",)), 2.0)
    assert observed["sections.stored", "newer"] == (Ok(("FINISHED",)), 1.0)


def test_reset_of_default_values_changes_nothing(observed: Observations) -> None:
    """Resetting values already at their defaults answers unchanged, the host reading the cancel as a refusal."""
    assert observed["sections.stored", "unchanged"] == Error(("ANSWERED",))


def test_store_commands_fail_without_their_host(observed: Observations) -> None:
    """A store command run where no context pointer names its host fails."""
    assert observed["sections.stored", "unhosted"] == Error(("FAILED",))


def test_listing_registers_one_command_per_verb(observed: Observations) -> None:
    """A preset listing registers apply, step, save, import, export, replace, rename, and delete verbs."""
    assert observed["sections.preset", "verbs"] == ("APPLY", "DELETE", "EXPORT", "IMPORT", "RENAME", "REPLACE", "SAVE", "STEP")


def test_presets_apply_step_and_save(observed: Observations) -> None:
    """Applying a stock preset writes its values, next steps past the matching preset and wraps, saving adds a user preset after the stock ones, and a name a stock preset holds is taken."""
    finished = Ok(("FINISHED",))
    assert (observed["sections.preset", "applied"], observed["sections.preset", "next"], observed["sections.preset", "wrapped"]) == ((finished, 1), (finished, 5), (finished, 1))
    assert (observed["sections.preset", "saved"], observed["sections.preset", "listed"]) == ((finished, 9), ("soft", "hard", "Mine"))
    assert observed["sections.preset", "taken"] == Error(("FAILED",))


def test_preset_menu_searches_on_key_press(observed: Observations, released: Observations) -> None:
    """A preset menu registers searchable and leaves with release."""
    assert (observed["sections.preset", "menu"], released["sections.lists", "menu"]) == (Some(("SEARCH_ON_KEY_PRESS",)), False)
