"""pytest plugin every other plugin of the package requires: each test's outcome recorded on its stash, loaded by pytest alone so its assertions are rewritten."""

from collections.abc import Generator

import pytest

from rasm.testing._outcomes import FAILED

# --- [COMPOSITION] ----------------------------------------------------------------------


@pytest.hookimpl(wrapper=True, tryfirst=True)
def pytest_runtest_makereport(item: pytest.Item) -> Generator[None, pytest.TestReport, pytest.TestReport]:
    """Records on a test whether its setup or call failed, the fact a fixture's release reads."""
    report = yield
    item.stash[FAILED] = item.stash.get(FAILED, False) or report.failed
    return report


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = []
