"""Server rows of `infra/compose.yaml` and one server of each kind shared by the test process."""

from collections.abc import Callable
import functools
import uuid

import pytest
lazy from testcontainers.community.clickhouse import ClickHouseContainer
lazy from testcontainers.community.postgres import PostgresContainer
lazy from testcontainers.community.valkey import ValkeyContainer

lazy from rasm_testing import Defined, defined, RustfsContainer, settled, Started

# --- [COMPOSITION] ----------------------------------------------------------------------


@pytest.fixture(scope="session")
def rows(pytestconfig: pytest.Config) -> Callable[[str], Defined]:
    """Returns the reader of a server row of `infra/compose.yaml`, each row resolved by Compose once for the test process."""
    return functools.cache(lambda service: settled(defined(pytestconfig.rootpath / "infra" / "compose.yaml", service)))


@pytest.fixture(scope="session")
def postgres_server(shared: Started, rows: Callable[[str], Defined]) -> PostgresContainer:
    """Returns a Postgres server of the platform's image, started once for the test process."""
    return shared(PostgresContainer(rows("postgres").image, driver=None))


@pytest.fixture(scope="session")
def clickhouse_server(shared: Started, rows: Callable[[str], Defined]) -> ClickHouseContainer:
    """Returns a ClickHouse server of the platform's image, started once for the test process."""
    return shared(ClickHouseContainer(rows("clickhouse").image))


@pytest.fixture(scope="session")
def cache_server(shared: Started, rows: Callable[[str], Defined]) -> ValkeyContainer:
    """Returns a Valkey server of the platform's image, started once for the test process."""
    return shared(ValkeyContainer(rows("cache").image))


@pytest.fixture(scope="session")
def storage_server(shared: Started, rows: Callable[[str], Defined]) -> RustfsContainer:
    """Returns a RustFS server of the platform's image under keys of this run, started once for the test process."""
    return shared(RustfsContainer(rows("storage").image, access_key=uuid.uuid4().hex, secret_key=uuid.uuid4().hex))


@pytest.fixture(scope="session")
def cache_row(rows: Callable[[str], Defined]) -> Defined:
    """Returns the cache row, resolved at session setup while the process's own `HOME` still names the Docker CLI's plugins, which `pytester` replaces."""
    return rows("cache")
