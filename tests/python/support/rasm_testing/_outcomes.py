"""Values the plugins of the package share: the stash keys of a test's outcome and time limit, the outcome a test step settles, and TCP opened for the tests requesting a plugin's fixtures."""

from collections.abc import Callable, Mapping
from datetime import timedelta
from enum import StrEnum
from types import MappingProxyType
from typing import Protocol

from expression import Result
import pytest

from rasm_records import Fault, Outcome

# --- [TYPES] ----------------------------------------------------------------------------


class Named(Protocol):
    """Fixture definition pytest registers under a name."""

    @property
    def name(self) -> str:
        """Name tests request the fixture by."""
        ...


# --- [CONSTANTS] ------------------------------------------------------------------------

FAILED = pytest.StashKey[bool]()
LIMIT = pytest.StashKey[timedelta]()

# --- [OPERATIONS] -----------------------------------------------------------------------


def settled[T](outcome: Outcome[T], /, *, templates: Mapping[StrEnum, str] = MappingProxyType({})) -> T:
    """Returns the value of an outcome, or fails the test with each fault's report text, a reason's template from `templates` where it holds one.

    Returns:
        The value of an `Ok` outcome.
    """
    match outcome:
        case Result(tag="ok", ok=value):
            return value
        case Result(error=faults):
            pytest.fail("\n".join(fault.filled(templates.get(fault.reason, fault.reason)) for fault in faults), pytrace=False)


def raised[T](outcome: Outcome[T], /) -> T:
    """Returns the value of an outcome, or raises the exception its first fault holds as cause, a fault holding none settled as `settled` does.

    Returns:
        The value of an `Ok` outcome.
    """
    match outcome:
        case Result(tag="error", error=(Fault(cause=BaseException() as cause), *_)):
            raise cause
        case _:
            return settled(outcome)


def reaching(*fixtures: Named) -> Callable[[pytest.Item], None]:
    """Returns the `pytest_itemcollected` hook of a plugin, which opens TCP for each test requesting one of its fixtures and leaves every other test refused by pytest-socket."""
    names = frozenset(fixture.name for fixture in fixtures)

    def pytest_itemcollected(item: pytest.Item) -> None:
        if isinstance(item, pytest.Function) and not names.isdisjoint(item.fixturenames):
            item.add_marker(pytest.mark.enable_socket)

    return pytest_itemcollected


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["FAILED", "LIMIT", "raised", "reaching", "settled"]
