"""pytest plugin of the `sqlite` extra: a private SQLite database with the `vec0` extension loaded."""

from collections.abc import Iterator
from contextlib import closing
from pathlib import Path
import sqlite3

import pytest
import sqlite_vec

from rasm_testing._outcomes import FAILED

# --- [COMPOSITION] ----------------------------------------------------------------------

pytest_plugins = ("rasm_testing.plugin",)


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


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = []
