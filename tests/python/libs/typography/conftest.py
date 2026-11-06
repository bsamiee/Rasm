"""Fetch Unicode data before pytest disables sockets."""

import pytest
from youseedee import ensure_files

# --- [COMPOSITION] ----------------------------------------------------------------------
pytest_sessionstart = pytest.hookimpl(ensure_files)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["pytest_sessionstart"]
