"""Cases the store suite runs through an inner pytest: a passed and a failed test per store kind, and a test leaving a connection open per kind that counts connections."""

from collections.abc import Callable, Iterator
from contextlib import closing, ExitStack

import pytest
import redis

from rasm_store_testing import BUCKET, CACHE, CLICKHOUSE, POSTGRES, Stores

# --- [OPERATIONS] -----------------------------------------------------------------------


@pytest.mark.usefixtures("held")
def test_passed() -> None:
    """Holds a store and passes."""


@pytest.mark.usefixtures("held")
def test_failed() -> None:
    """Holds a store and fails."""
    pytest.fail("kept")


def test_leaked_postgres(stores: Stores, outliving: ExitStack, record_property: Callable[[str, object], None]) -> None:
    """Takes a connection from the pool and closes it only after the store's release."""
    postgres = stores(POSTGRES)
    record_property("store", postgres.record.id)
    outliving.callback(postgres.client.getconn().close)


def test_leaked_cache(stores: Stores, outliving: ExitStack, record_property: Callable[[str, object], None]) -> None:
    """Opens a connection outside the store's client and closes it only after the store's release."""
    cache = stores(CACHE)
    record_property("store", cache.record.id)
    outliving.enter_context(closing(redis.Redis(connection_pool=redis.ConnectionPool(**cache.client.get_connection_kwargs())))).ping()


# --- [COMPOSITION] ----------------------------------------------------------------------


@pytest.fixture(scope="module")
def outliving() -> Iterator[ExitStack]:
    """Yields a scope that ends after the release of every store this module's tests held."""
    with ExitStack() as stack:
        yield stack


@pytest.fixture(params=[POSTGRES, BUCKET, CLICKHOUSE, CACHE])
def held(request: pytest.FixtureRequest, stores: Stores, record_property: Callable[[str, object], None]) -> None:
    """Holds a store of each kind, its id recorded on the test's report."""
    record_property("store", stores(request.param).record.id)
