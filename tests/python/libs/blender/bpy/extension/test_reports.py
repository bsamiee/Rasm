"""Callback fault reports: report types per reason, distinct held entries, entries dropped on load, and the request queue in background mode."""

from expression import Ok
import pytest

lazy from tests.python.libs.blender.bpy.conftest import Observations

# --- [OPERATIONS] -----------------------------------------------------------------------


@pytest.mark.parametrize(
    "reason,level",
    [
        ("OUTSIDE", "ERROR_INVALID_INPUT"),
        ("INVALID", "ERROR_INVALID_INPUT"),
        ("ABSENT", "ERROR_INVALID_INPUT"),
        ("TAKEN", "ERROR_INVALID_INPUT"),
        ("REQUIRED", "ERROR_INVALID_CONTEXT"),
        ("UNSEEN", "ERROR_INVALID_CONTEXT"),
        ("UNDATED", "ERROR_INVALID_CONTEXT"),
        ("UNROOTED", "ERROR_INVALID_CONTEXT"),
        ("UNVARIED", "ERROR_INVALID_CONTEXT"),
        ("UNCHOSEN", "ERROR_INVALID_CONTEXT"),
        ("UNREGISTERED", "ERROR"),
        ("REFUSED", "ERROR"),
        ("ANSWERED", "ERROR"),
        ("FAILED", "ERROR"),
        ("NUMERIC", "ERROR"),
        ("UNREADABLE", "ERROR"),
        ("UNWRITABLE", "ERROR"),
    ],
)
def test_reasons_report_at_their_level(observed: Observations, reason: str, level: str) -> None:
    """Domain refusals report invalid input, unmet requirements invalid context, and every other fault a plain error."""
    assert observed["extension.levels", reason] == level


def test_equal_faults_hold_one_entry(observed: Observations) -> None:
    """Three faults, two of them equal, hold two entries in fault order."""
    assert observed["extension.held"] == ("ERROR_INVALID_INPUT", "ERROR")


def test_file_load_drops_entries_of_the_replaced_file(observed: Observations) -> None:
    """A load drops the faults held before it, keeping those its load handlers raised."""
    assert observed["extension.reloaded", "entries"] == ("ERROR_INVALID_CONTEXT",)


def test_queued_request_settles_at_once_in_background_mode(observed: Observations) -> None:
    """A three-step request applies every step at its queueing, no timer running in background mode."""
    assert observed["extension.queued"] == {"outcome": Ok(None), "steps": (3, 2, 1)}


def test_deferred_write_applies_inside_its_scope_alone(observed: Observations) -> None:
    """Deferred writes apply per call in background mode while the scope stands, and a call after it applies nothing."""
    assert observed["extension.deferred"] == ("inside", "inside")
