"""pytest plugin: stores of the store service a test makes, a private SQLite database, and Testcontainers servers started for a test or shared by its process."""

from collections.abc import Generator, Iterator
from contextlib import closing, ExitStack
from datetime import timedelta
from enum import StrEnum
import functools
from ipaddress import IPv4Address
import os
from pathlib import Path
import sqlite3
from typing import Protocol
from urllib.parse import urlunsplit
lazy import socket

import pytest
lazy from expression import Option
lazy import sqlite_vec

lazy from rasm_records import Fault, Reason
lazy from rasm_testing._servers import logged, serving, Started
lazy from rasm_testing._stores import Lease, Refusal, settled, Stores

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

# --- [COMPOSITION] ----------------------------------------------------------------------


def pytest_addoption(parser: pytest.Parser) -> None:
    """Registers the store service's port and the report template of a silent service."""
    parser.addoption("--store-port", type=int, default=os.environ.get("STORE_PORT"), help="Loopback port of the store service, `STORE_PORT` by default")
    parser.addini("store_silent", "Report template of a store service granting no session, `{subject}` its address", default=None)


@pytest.fixture(scope="session")
def store_session(pytestconfig: pytest.Config) -> Iterator[Lease]:
    """Yields the session the test process holds on the store service at `--store-port`, ended after the process's last test."""
    port = settled(Option.of_optional(pytestconfig.getoption("store_port")).to_result_with(functools.partial(Fault.of, Reason.ABSENT, "--store-port", member="value or `STORE_PORT` default")))
    templates = Option.of_optional(pytestconfig.getini("store_silent")).map(lambda template: frozendict[StrEnum, str]({Refusal.SILENT: template})).default_value(frozendict[StrEnum, str]())
    with Lease.granted(urlunsplit(("http", f"{IPv4Address(socket.INADDR_LOOPBACK)}:{port}", "", "", "")), LIMIT, FAILED) as granted:
        lease = settled(granted, templates=templates)
        yield lease
        settled(lease.ended())


@pytest.fixture
def stores(request: pytest.FixtureRequest, store_session: Lease) -> Iterator[Stores]:
    """Yields the maker of a test's stores: `stores(POSTGRES)` a new database, `stores(Template(kind=POSTGRES, ...))` a clone, each dropped after a passed test and kept after a failed one."""
    with ExitStack() as stack:
        yield functools.partial(store_session.held, stack, request.node)


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


REACHING = frozenset(fixture.name for fixture in (store_session, stores, shared, started))


def pytest_itemcollected(item: pytest.Item) -> None:
    """Opens TCP for each test requesting the store session, stores, or a server, which pytest-socket refuses every other test."""
    if isinstance(item, pytest.Function) and not REACHING.isdisjoint(item.fixturenames):
        item.add_marker(pytest.mark.enable_socket)


@pytest.hookimpl(optionalhook=True, tryfirst=True)
def pytest_timeout_set_timer(item: pytest.Item, settings: Timed) -> None:
    """Records on a test the time pytest-timeout allows it, the limit of each store call the test makes, and leaves the timer to pytest-timeout."""
    item.stash[LIMIT] = timedelta(seconds=settings.timeout)


@pytest.hookimpl(wrapper=True, tryfirst=True)
def pytest_runtest_makereport(item: pytest.Item) -> Generator[None, pytest.TestReport, pytest.TestReport]:
    """Records on a test whether its setup or call failed, the fact a store's release and a server's log report read."""
    report = yield
    item.stash[FAILED] = item.stash.get(FAILED, False) or report.failed
    return report
