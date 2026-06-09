"""Live-stderr resolution law for tools.assay [configure_logging].

Proves the configured ``logger_factory`` resolves ``sys.stderr`` per write: a stream closed since
configure time cannot strand a later log on a dead fd (``ValueError: I/O operation on closed file``),
the cross-package cascade that motivated the leaf logging owner. Drives the installed logger directly
rather than through ``capture_logs`` (which bypasses the factory).
"""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

import io
from typing import TYPE_CHECKING

import structlog

from tests._aspect import register_law  # noqa: PLC2701  # sibling test-internal module; _-named by S1 design
from tools.assay import configure_logging
from tools.assay._logging import _StderrLogger  # noqa: PLC2701  # the live-resolving factory product under test
from tools.assay.composition.settings import LogFormat


if TYPE_CHECKING:
    import pytest


# --- [OPERATIONS] -----------------------------------------------------------------------


def test_configure_logging_resolves_stderr_per_write(monkeypatch: pytest.MonkeyPatch) -> None:
    """The factory configure_logging installs writes to the live sys.stderr, surviving a closed prior stream."""
    configure_logging(LogFormat.CI)
    logger = structlog.get_config()["logger_factory"]()
    assert isinstance(logger, _StderrLogger)

    first = io.StringIO()
    monkeypatch.setattr("sys.stderr", first)
    logger.info("before-close")
    assert "before-close" in first.getvalue()

    first.close()
    second = io.StringIO()
    monkeypatch.setattr("sys.stderr", second)
    logger.info("after-reassign")  # captured stream would raise ValueError here; live resolution writes to second

    assert "after-reassign" in second.getvalue()


register_law(configure_logging, "test_configure_logging_resolves_stderr_per_write")
