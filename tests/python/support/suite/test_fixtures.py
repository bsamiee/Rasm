"""SQLite fixture and the TCP rule, each through the plugin rows a suite loads."""

from contextlib import closing
import sqlite3

import pytest
import sqlite_vec

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
    (torn,) = (report for report in pytester.inline_run("-p", "rasm_testing.sqlite").getreports("pytest_runtest_logreport") if report.when == "teardown")
    (kept,) = (text for name, text in torn.sections if "kept sqlite" in name)
    with closing(sqlite3.connect(kept)) as reader:
        assert reader.execute("select mark from marks").fetchall() == [("held",)]


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
    pytester.runpytest_subprocess("-p", "rasm_testing.containers", "-p", "rasm_testing.sqlite", "--disable-socket", "-W", "error").assert_outcomes(passed=2)
