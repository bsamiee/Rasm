"""Stores of the store service: each kind through its catalog driver, template clones under concurrent builders, and the release rule a passed, a failed, and a leaking test meet."""

from collections.abc import Callable
from functools import partial
from pathlib import Path
import uuid

import anyio
from connectrpc.code import Code
from connectrpc.errors import ConnectError
from expression import Result
import pytest
from rasm.store.store_connect import StoreServiceClientSync
from rasm.store.store_pb import BucketStore, StoreRequest
lazy from clickhouse_connect.driver import Client
lazy import obstore
lazy from psycopg_pool import NullConnectionPool

from rasm_records import Fault
from rasm_testing import BUCKET, CACHE, CLICKHOUSE, Lease, POSTGRES, Refusal, Stores, Template

# --- [CONSTANTS] ------------------------------------------------------------------------

CASES = Path(__file__).with_name("store_cases.py")

# --- [COMPOSITION] ----------------------------------------------------------------------


@pytest.fixture
def cases(pytester: pytest.Pytester, pytestconfig: pytest.Config) -> Callable[[str], pytest.HookRecorder]:
    """Returns the inner run of the cases a `-k` expression selects, against this run's store service."""
    return partial(pytester.inline_run, str(CASES), "-p", "rasm_testing.plugin", "--store-port", str(pytestconfig.getoption("store_port")), "-k")


# --- [OPERATIONS] -----------------------------------------------------------------------


def _marked(pool: NullConnectionPool, /) -> None:
    with pool.connection() as connection:
        connection.execute("create table marks as select 'held' as mark")


def _unbuilt(pool: NullConnectionPool, /) -> None:
    raise NotImplementedError(pool)


def _ranked(client: Client, /) -> None:
    client.command("CREATE TABLE marks ENGINE = MergeTree ORDER BY mark AS SELECT 'held' AS mark")


def test_a_postgres_store_answers_its_login_within_its_connection_limit(stores: Stores) -> None:
    """The pool logs in as the store's user to the store's database and opens at most the login's connection limit."""
    postgres = stores(POSTGRES)
    with postgres.client.connection() as connection:
        assert connection.execute("select current_user, current_database()").fetchone() == (postgres.record.username, postgres.record.database)
    assert postgres.client.max_size == postgres.record.connection_limit


def test_a_test_holds_several_stores_of_one_kind(stores: Stores) -> None:
    """Two stores of one kind in one test are two databases, each pool reaching its own."""
    first, second = stores(POSTGRES), stores(POSTGRES)
    with first.client.connection() as one, second.client.connection() as other:
        assert (one.execute("select current_database()").fetchone(), other.execute("select current_database()").fetchone()) == ((first.record.database,), (second.record.database,))
    assert first.record.database != second.record.database


def test_concurrent_clones_of_a_new_template_share_one_build(stores: Stores) -> None:
    """Four clones of a digest no session holds wait on one build, each holding the rows the build wrote."""
    template = Template(kind=POSTGRES, digest=uuid.uuid7().hex, build=_marked)
    clone = partial(anyio.to_thread.run_sync, stores, template)
    clones = anyio.run(lambda: anyio.gather(clone(), clone(), clone(), clone()))
    for clone in clones:
        with clone.client.connection() as connection:
            assert connection.execute("select mark from marks").fetchall() == [("held",)]
    assert len({clone.record.database for clone in clones}) == len(clones)


def test_a_failed_build_abandons_its_template_for_the_next_builder(stores: Stores) -> None:
    """A build that raises fails its acquisition and frees the digest, so the next acquisition builds it at once."""
    digest = uuid.uuid7().hex
    with pytest.raises(NotImplementedError):
        stores(Template(kind=POSTGRES, digest=digest, build=_unbuilt))
    with stores(Template(kind=POSTGRES, digest=digest, build=_marked)).client.connection() as connection:
        assert connection.execute("select mark from marks").fetchall() == [("held",)]


def test_a_clickhouse_store_starts_as_a_clone_of_its_template(stores: Stores) -> None:
    """A ClickHouse store under a template holds the rows the template's build wrote, in its own database."""
    clickhouse = stores(Template(kind=CLICKHOUSE, digest=uuid.uuid7().hex, build=_ranked))
    assert clickhouse.client.query("SELECT mark, currentDatabase() FROM marks").result_rows == [("held", clickhouse.record.database)]


def test_a_bucket_store_holds_objects_under_its_session_credentials(stores: Stores) -> None:
    """An object written through the store's object store is read back from the store's bucket."""
    bucket = stores(BUCKET)
    obstore.put(bucket.client, "mark", b"held")
    assert bytes(obstore.get(bucket.client, "mark").bytes()) == b"held"


def test_bucket_credentials_past_their_life_are_renewed(stores: Stores, store_session: Lease) -> None:
    """A driver opened on a record whose credentials lapsed is refused its first request and signs the next with the credentials the service renews."""
    held = stores(BUCKET).record
    lapsed = BucketStore(
        id=held.id,
        endpoint=held.endpoint,
        region=held.region,
        bucket=held.bucket,
        access_key_id=held.access_key_id,
        secret_access_key=held.secret_access_key[::-1],
        session_token=held.session_token,
        credentials_ttl_seconds=0,
    )
    with BUCKET.opened(store_session, lapsed) as renewing:
        with pytest.raises(obstore.exceptions.BaseError):
            obstore.put(renewing, "mark", b"held")
        obstore.put(renewing, "mark", b"held")
        assert bytes(obstore.get(renewing, "mark").bytes()) == b"held"


def test_a_clickhouse_store_answers_its_user_in_its_database(stores: Stores) -> None:
    """The client queries as the store's user in the store's database."""
    clickhouse = stores(CLICKHOUSE)
    assert clickhouse.client.query("SELECT currentUser(), currentDatabase()").first_row == (clickhouse.record.username, clickhouse.record.database)


def test_a_cache_store_answers_its_user_in_its_database(stores: Stores) -> None:
    """The client writes and reads a key as the store's user."""
    cache = stores(CACHE)
    cache.client.set("mark", "held")
    assert cache.client.get("mark") == b"held"
    assert cache.client.acl_whoami() == cache.record.username


def test_a_passed_test_drops_its_store(cases: Callable[[str], pytest.HookRecorder], store_session: Lease) -> None:
    """After a passed test of each kind, the service holds no store under the test's id."""
    passed, _, failed = cases("test_passed").listoutcomes()
    assert passed
    assert not failed
    for report in passed:
        match store_session.called(StoreServiceClientSync.keep_store, StoreRequest(id=str(dict(report.user_properties)["store"]))):
            case Result(error=(Fault(cause=ConnectError(code=Code.NOT_FOUND)),)):
                pass
            case kept:
                pytest.fail(f"{report.nodeid}: {kept}")


def test_a_failed_test_keeps_its_store_past_its_session(cases: Callable[[str], pytest.HookRecorder], store_session: Lease) -> None:
    """After a failed test of each kind and the end of its session, the service still holds the test's store, and the test's report holds the store's record."""
    recorder = cases("test_failed")
    _, _, failed = recorder.listoutcomes()
    assert failed
    for report in failed:
        request = StoreRequest(id=str(dict(report.user_properties)["store"]))
        assert store_session.called(StoreServiceClientSync.keep_store, request).is_ok()
        assert store_session.called(StoreServiceClientSync.drop_store, request).is_ok()
    assert sum("kept store" in name for report in recorder.getreports("pytest_runtest_logreport") for name, _ in report.sections) == len(failed)


def test_a_connection_left_open_fails_its_test(cases: Callable[[str], pytest.HookRecorder]) -> None:
    """A passed test whose connection outlives its store fails at teardown, its report naming the store."""
    reports = cases("test_leaked").getreports("pytest_runtest_logreport")
    torn = tuple(report for report in reports if report.when == "teardown" and report.failed)
    assert torn
    assert {report.nodeid for report in torn} == {report.nodeid for report in reports if report.when == "call"}
    assert all(Refusal.LEAKED.format(subject=dict(report.user_properties)["store"]) in report.longreprtext for report in torn)
