"""Servers a test starts through Testcontainers: each catalog driver against its server, a restart a test controls, and the logs a failed test reports."""

import pytest
lazy import obstore
lazy from testcontainers.community.clickhouse import ClickHouseContainer
lazy from testcontainers.community.postgres import PostgresContainer
lazy from testcontainers.community.valkey import ValkeyContainer

lazy from rasm_testing import Defined, opened, RustfsContainer, Started

# --- [OPERATIONS] -----------------------------------------------------------------------


def test_a_postgres_server_answers_through_its_pool(postgres_server: PostgresContainer) -> None:
    """The pool `opened` builds logs in as the server's user and round-trips a row."""
    with opened(postgres_server) as pool, pool.connection() as connection:
        connection.execute("create temporary table marks (mark text)")
        connection.execute("insert into marks (mark) values (%s)", ("held",))
        assert connection.execute("select current_user, mark from marks").fetchall() == [(postgres_server.username, "held")]


def test_a_clickhouse_server_answers_through_its_client(clickhouse_server: ClickHouseContainer) -> None:
    """The client `opened` builds reaches the HTTP port as the server's user in its database."""
    with opened(clickhouse_server) as client:
        assert client.query("SELECT currentUser(), currentDatabase(), sum(number) FROM numbers(4)").first_row == (clickhouse_server.username, clickhouse_server.dbname, 6)


def test_a_cache_server_answers_through_its_client(cache_server: ValkeyContainer) -> None:
    """The client `opened` builds writes a key and reads it back."""
    with opened(cache_server) as cache:
        cache.set("mark", "held")
        assert cache.get("mark") == b"held"


def test_a_storage_server_holds_objects_in_a_bucket_per_open(storage_server: RustfsContainer) -> None:
    """Each `opened` store holds a new bucket: an object written through one is read back from it and absent from the next."""
    with opened(storage_server) as first, opened(storage_server) as second:
        obstore.put(first, "mark", b"held")
        assert bytes(obstore.get(first, "mark").bytes()) == b"held"
        assert [entry["path"] for entry in obstore.list(second).collect()] == []


@pytest.mark.parametrize("command,kept", [pytest.param((), b"held", id="image-default"), pytest.param(None, None, id="compose-row")])
def test_the_cache_row_keeps_no_key_across_a_restart(started: Started, cache_row: Defined, command: tuple[str, ...] | None, kept: bytes | None) -> None:
    """Under the image's default command a key survives a restart the test issues, and under the Compose row's command it is gone."""
    server = started(ValkeyContainer(cache_row.image).with_command([*(command if command is not None else cache_row.command or ())]))
    with opened(server) as cache:
        cache.set("mark", "held")
    server.get_wrapped_container().restart()
    with opened(server) as cache:
        assert cache.get("mark") == kept


@pytest.mark.enable_socket
def test_a_failed_test_reports_the_logs_of_its_server(pytester: pytest.Pytester, cache_row: Defined) -> None:
    """A failing inner test that started a server holds that server's log under its image name in the teardown report."""
    pytester.makepyfile(f"""
        import pytest
        from testcontainers.community.valkey import ValkeyContainer


        def test_fails(started):
            started(ValkeyContainer({cache_row.image!r}))
            pytest.fail("refused")
    """)
    (torn,) = (report for report in pytester.inline_run("-p", "rasm_testing.plugin").getreports("pytest_runtest_logreport") if report.when == "teardown")
    assert [name for name, text in torn.sections if "Ready to accept connections" in text] == [f"Captured {cache_row.image} teardown"]
