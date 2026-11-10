"""SQLite fixture, the store session's refusals, and the TCP rule, each through the plugin row every suite loads."""

from contextlib import closing
import socket
import sqlite3

import pytest
import sqlite_vec

from rasm_testing import Refusal

# --- [OPERATIONS] -----------------------------------------------------------------------


@pytest.mark.parametrize("mark", ["first", "second"])
def test_sqlite_is_private_to_its_test(sqlite: sqlite3.Connection, mark: str) -> None:
    """Each case creates the same table in an empty database and reads its own row alone."""
    sqlite.execute("create table marks (mark text)")
    sqlite.execute("insert into marks (mark) values (?)", (mark,))
    assert sqlite.execute("select mark from marks").fetchall() == [(mark,)]


def test_sqlite_ranks_vectors_through_vec0(sqlite: sqlite3.Connection) -> None:
    """A `vec0` table answers a nearest-neighbour query in distance order."""
    sqlite.execute("create virtual table points using vec0(position float[2])")
    sqlite.executemany(
        "insert into points (rowid, position) values (?, ?)", ((row, sqlite_vec.serialize_float32(list(position))) for row, position in enumerate(((0.0, 0.0), (3.0, 4.0), (9.0, 9.0)), start=1))
    )
    nearest = sqlite.execute("select rowid, distance from points where position match ? order by distance limit 2", (sqlite_vec.serialize_float32([3.0, 4.5]),)).fetchall()
    assert [row for row, _ in nearest] == [2, 1]
    assert nearest[0][1] == pytest.approx(0.5)


def test_sqlite_refuses_extension_loading_from_sql(sqlite: sqlite3.Connection) -> None:
    """The fixture opens extension loading for the C API alone, so SQL `load_extension` stays refused."""
    with pytest.raises(sqlite3.OperationalError, match="not authorized"):
        sqlite.execute("select load_extension('vec0')")


def test_a_failed_test_keeps_its_sqlite_file(pytester: pytest.Pytester) -> None:
    """The database file of a failing test outlives it at the path its teardown report names, holding the rows the test wrote."""
    pytester.makepyfile("""
        import pytest


        def test_fails(sqlite):
            sqlite.execute("create table marks (mark text)")
            sqlite.execute("insert into marks (mark) values ('held')")
            sqlite.commit()
            pytest.fail("refused")
    """)
    (torn,) = (report for report in pytester.inline_run("-p", "rasm_testing.plugin").getreports("pytest_runtest_logreport") if report.when == "teardown")
    (kept,) = (text for name, text in torn.sections if "kept sqlite" in name)
    with closing(sqlite3.connect(kept)) as reader:
        assert reader.execute("select mark from marks").fetchall() == [("held",)]


@pytest.mark.enable_socket
@pytest.mark.parametrize("template", [pytest.param(None, id="own-template"), pytest.param("no session at {subject}, start it", id="caller-template")])
def test_a_silent_store_service_fails_the_case_with_its_address(pytester: pytest.Pytester, template: str | None) -> None:
    """A store case against a port nothing listens on fails at setup, naming the address in the refusal's own template or in the one the caller configures."""
    with socket.socket() as unused:
        unused.bind(("127.0.0.1", 0))
        port = unused.getsockname()[1]
    pytester.makepyfile("def test_reads(stores): ...")
    configured = ("-o", f"store_silent={template}") if template is not None else ()
    (report,) = (report for report in pytester.inline_run("-p", "rasm_testing.plugin", "--store-port", str(port), *configured).getreports("pytest_runtest_logreport") if report.failed)
    assert report.when == "setup"
    assert (template or Refusal.SILENT).format(subject=f"http://127.0.0.1:{port}") in report.longreprtext


def test_a_port_outside_tcp_fails_the_case_as_a_silent_service(pytester: pytest.Pytester) -> None:
    """A store case under a port no TCP port holds fails at setup with the transport's refusal of the address."""
    pytester.makepyfile("def test_reads(stores): ...")
    (report,) = (report for report in pytester.inline_run("-p", "rasm_testing.plugin", "--store-port", "65536").getreports("pytest_runtest_logreport") if report.failed)
    assert report.when == "setup"
    assert Refusal.SILENT.format(subject="http://127.0.0.1:65536") in report.longreprtext


def test_tcp_opens_for_a_test_requesting_a_server_and_stays_refused_for_others(pytester: pytest.Pytester) -> None:
    """Under `--disable-socket` with warnings as errors, a test requesting `started` creates a socket and a test requesting `sqlite` is refused one."""
    pytester.makepyfile("""
        import socket

        import pytest


        def test_reaching(started):
            socket.socket().close()


        def test_refused(sqlite):
            with pytest.raises(UserWarning, match="tried to use socket"):
                socket.socket()
    """)
    pytester.runpytest_subprocess("-p", "rasm_testing.plugin", "--disable-socket", "-W", "error").assert_outcomes(passed=2)
