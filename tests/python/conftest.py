"""Pytest configuration for tests/python, package registration follows the libs/python layout."""

# --- [IMPORTS] --------------------------------------------------------------------------

from pathlib import Path

import pytest

from tests.python.support.properties import register_package_tree

# --- [COMPOSITION] ----------------------------------------------------------------------


def pytest_configure(config: pytest.Config) -> None:
    """Register every package under libs/python for public-API coverage on the session stash."""
    register_package_tree(config, config.rootpath / "libs" / "python", Path(__file__).resolve().parent / "libs")
