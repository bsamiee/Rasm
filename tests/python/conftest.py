"""Registers packages for public-API test coverage."""

import importlib
from pathlib import Path

import pytest

from tests.python.support.properties import register_package_tree

# --- [COMPOSITION] ----------------------------------------------------------------------


def pytest_configure(config: pytest.Config) -> None:
    """Register the folder the `blender` library imports from with its mirrored ``tests/python`` suite."""
    module_root = Path(next(iter(importlib.import_module("blender").__path__))).parent.relative_to(config.rootpath)
    register_package_tree(config.stash, config.rootpath / module_root, Path(__file__).resolve().parent / module_root.parent)
