"""pytest plugin: a private SQLite database, Testcontainers servers started for a test or shared by its process, an event loop thread and the HTTP servers a test hosts on it, each test's outcome and time limit held on its stash, and the outcome a test step settles."""

from collections.abc import Callable, Generator, Iterator, Mapping, Sequence
from contextlib import closing, ExitStack
from datetime import timedelta
from enum import StrEnum
import functools
from pathlib import Path
import sqlite3
from typing import Protocol

import pytest
lazy from anyio.from_thread import BlockingPortal, start_blocking_portal
lazy from expression import Result
lazy import sqlite_vec

lazy from rasm_records import Outcome
lazy from rasm_testing._servers import Answer, hosting, logged, Received, serving, Started

# --- [TYPES] ----------------------------------------------------------------------------


class Timed(Protocol):
    """Settings pytest-timeout resolves for one test."""

    @property
    def timeout(self) -> float:
        """Seconds the test may run."""
        ...


# --- [CONSTANTS] ------------------------------------------------------------------------

FAILED = pytest.StashKey[bool]()
LIMIT = pytest.StashKey[timedelta]()

# --- [OPERATIONS] -----------------------------------------------------------------------


def settled[T](outcome: Outcome[T], /, *, templates: Mapping[StrEnum, str] = frozendict()) -> T:
    """Returns the value of an outcome, or fails the test with each fault's report text, a reason's template from `templates` where it holds one.

    Returns:
        The value of an `Ok` outcome.
    """
    match outcome:
        case Result(tag="ok", ok=value):
            return value
        case Result(error=faults):
            pytest.fail("\n".join(fault.filled(templates.get(fault.reason, fault.reason)) for fault in faults), pytrace=False)


# --- [COMPOSITION] ----------------------------------------------------------------------


@pytest.fixture
def sqlite(request: pytest.FixtureRequest, tmp_path: Path) -> Iterator[sqlite3.Connection]:
    """Yields a private SQLite database with the `vec0` extension loaded, a file under the test's `tmp_path` closed after the test and named in the report of a failed one."""
    file = tmp_path / "store.sqlite3"
    with closing(sqlite3.connect(file)) as connection:
        connection.setconfig(sqlite3.SQLITE_DBCONFIG_ENABLE_LOAD_EXTENSION)
        sqlite_vec.load(connection)
        yield connection
    if request.node.stash.get(FAILED, False):
        request.node.add_report_section("teardown", "kept sqlite", str(file))


@pytest.fixture(scope="session")
def shared() -> Iterator[Started]:
    """Yields the start of a server the test process shares, stopped after the process's last test."""
    with ExitStack() as stack:
        yield functools.partial(serving, stack, ())


@pytest.fixture
def started(request: pytest.FixtureRequest) -> Iterator[Started]:
    """Yields the start of a server the test controls, stopped after the test with its logs in the report of a failed one."""
    with ExitStack() as stack:
        yield functools.partial(serving, stack, (functools.partial(logged, request.node, FAILED),))


@pytest.fixture
def portal() -> Iterator[BlockingPortal]:
    """Yields an event loop running on its own thread until the test ends, where the test hosts the servers it reaches over loopback."""
    with start_blocking_portal() as running:
        yield running


@pytest.fixture
def hosted(portal: BlockingPortal) -> Iterator[Callable[[Callable[[Received], Answer]], tuple[str, Sequence[Received]]]]:
    """Yields the start of an HTTP server answering from a function, which returns the server's base URL and the requests it received, each server stopped after the test."""
    with ExitStack() as stack:
        yield functools.partial(hosting, stack, portal)


REACHING = frozenset(fixture.name for fixture in (shared, started, portal))


def pytest_itemcollected(item: pytest.Item) -> None:
    """Opens TCP for each test requesting a server, which pytest-socket refuses every other test."""
    if isinstance(item, pytest.Function) and not REACHING.isdisjoint(item.fixturenames):
        item.add_marker(pytest.mark.enable_socket)


@pytest.hookimpl(optionalhook=True, tryfirst=True)
def pytest_timeout_set_timer(item: pytest.Item, settings: Timed) -> None:
    """Records on a test the time pytest-timeout allows it, the limit of each call a fixture makes for the test, and leaves the timer to pytest-timeout."""
    item.stash[LIMIT] = timedelta(seconds=settings.timeout)


@pytest.hookimpl(wrapper=True, tryfirst=True)
def pytest_runtest_makereport(item: pytest.Item) -> Generator[None, pytest.TestReport, pytest.TestReport]:
    """Records on a test whether its setup or call failed, the fact a fixture's release and a server's log report read."""
    report = yield
    item.stash[FAILED] = item.stash.get(FAILED, False) or report.failed
    return report


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["FAILED", "LIMIT", "settled"]
