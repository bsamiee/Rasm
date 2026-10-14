"""Pytest configuration for tests/python."""

from pathlib import Path

import msgspec
import pytest

from tests.python.support.properties import register_package_tree

# --- [MODELS] ---------------------------------------------------------------------------


class _Backend(msgspec.Struct, frozen=True, rename="kebab"):
    module_root: str


class _Uv(msgspec.Struct, frozen=True, rename="kebab"):
    build_backend: _Backend


class _Tool(msgspec.Struct, frozen=True):
    uv: _Uv


class _Root(msgspec.Struct, frozen=True):
    tool: _Tool


# --- [COMPOSITION] ----------------------------------------------------------------------


def pytest_configure(config: pytest.Config) -> None:
    """Register every import root under uv's ``module-root`` for public-API coverage, each suite mirroring its package under this directory."""
    module_root = Path(msgspec.toml.decode((config.rootpath / "pyproject.toml").read_bytes(), type=_Root).tool.uv.build_backend.module_root)
    register_package_tree(config.stash, config.rootpath / module_root, Path(__file__).resolve().parent / module_root.parent)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__: list[str] = []
