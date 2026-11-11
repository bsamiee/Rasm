"""pytest plugin: the session a test process holds on the store service and the stores a test makes, loaded beside `rasm_testing.plugin` through `pytest_plugins` of a suite's top conftest or `-p rasm_store_testing.plugin`."""

from collections.abc import Iterator
from contextlib import ExitStack
from enum import StrEnum
import functools
from ipaddress import IPv4Address
import os
from urllib.parse import urlunsplit
lazy import socket

import pytest
lazy from expression import Option

lazy from rasm_records import Fault, Reason
lazy from rasm_store_testing._stores import Lease, Refusal, Stores
lazy from rasm_testing import FAILED, LIMIT, settled

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


REACHING = frozenset(fixture.name for fixture in (store_session, stores))


def pytest_itemcollected(item: pytest.Item) -> None:
    """Opens TCP for each test requesting the store session or stores, which pytest-socket refuses every other test."""
    if isinstance(item, pytest.Function) and not REACHING.isdisjoint(item.fixturenames):
        item.add_marker(pytest.mark.enable_socket)
