"""pytest plugin every other plugin of the package requires: each test's outcome and time limit recorded on its stash, loaded by pytest alone so its assertions are rewritten."""

from collections.abc import Generator
from datetime import timedelta
from typing import Protocol

import pytest

from rasm_testing._outcomes import FAILED, LIMIT

# --- [TYPES] ----------------------------------------------------------------------------


class Timed(Protocol):
    """Settings pytest-timeout resolves for one test."""

    @property
    def timeout(self) -> float:
        """Seconds the test may run."""
        ...


# --- [COMPOSITION] ----------------------------------------------------------------------


@pytest.hookimpl(optionalhook=True, tryfirst=True)
def pytest_timeout_set_timer(item: pytest.Item, settings: Timed) -> None:
    """Records on a test the time pytest-timeout allows it, the limit of each call a fixture makes for the test, and leaves the timer to pytest-timeout."""
    item.stash[LIMIT] = timedelta(seconds=settings.timeout)


@pytest.hookimpl(wrapper=True, tryfirst=True)
def pytest_runtest_makereport(item: pytest.Item) -> Generator[None, pytest.TestReport, pytest.TestReport]:
    """Records on a test whether its setup or call failed, the fact a fixture's release and a server's log report read."""
    report = yield
    item.stash[FAILED] = item.stash.get(FAILED, False) or report.failed
    return report


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = []
