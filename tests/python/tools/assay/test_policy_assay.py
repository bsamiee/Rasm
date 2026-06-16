"""Assay-bound policy laws that depend on concrete ``tools.assay`` owners."""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

import importlib
from pathlib import Path  # noqa: TC003  # module-level _PYPROJECT assignment prevents deferral
import tomllib

from tests.python._testkit.runtime import REPO_ROOT
from tools.assay.composition.catalog import BENCHMARK_STORAGE_URI


# --- [CONSTANTS] ------------------------------------------------------------------------

_PYPROJECT: Path = REPO_ROOT / "pyproject.toml"

# --- [OPERATIONS] -----------------------------------------------------------------------


def _addopts() -> list[str]:
    """Return ``[tool.pytest] addopts`` as strings; absent config yields an empty list."""
    data: dict[str, object] = tomllib.loads(_PYPROJECT.read_text(encoding="utf-8"))
    match data:
        case {"tool": {"pytest": {"addopts": list() as addopts}}}:
            return [str(o) for o in addopts]
        case _:
            return []


def _benchmark_storage_flags(addopts: list[str]) -> list[str]:
    return [o for o in addopts if o.startswith("--benchmark-storage")]


# --- [BENCHMARK_STORAGE_POLICY]


def test_benchmark_storage_single_owner_in_addopts() -> None:
    """``--benchmark-storage`` appears in addopts exactly once."""
    flags = _benchmark_storage_flags(_addopts())
    assert len(flags) == 1, f"Expected exactly one --benchmark-storage in addopts, found {len(flags)}: {flags!r}"


def test_benchmark_storage_uri_matches_catalog_constant() -> None:
    """The addopts benchmark-storage URI matches the catalog owner constant."""
    flags = _benchmark_storage_flags(_addopts())
    assert flags, "--benchmark-storage not found in addopts (prerequisite for URI-match law)"
    _, _, uri = flags[0].partition("=")
    assert uri == BENCHMARK_STORAGE_URI, f"addopts URI {uri!r} != catalog.BENCHMARK_STORAGE_URI {BENCHMARK_STORAGE_URI!r}"


def test_benchmark_storage_uri_not_hardcoded_in_conftest_files() -> None:
    """No conftest.py under ``tests/`` hardcodes a benchmark-storage ``file://`` URI.

    addopts is the single source of the storage value; the assay conftest's belt-and-suspenders
    ``pytest_configure`` may rebind the option off pytest-benchmark's repo-root default, but only by
    importing ``catalog.BENCHMARK_STORAGE_URI`` — never by restating a ``file://`` literal that could drift.
    """
    violators: list[str] = [
        str(cf.relative_to(REPO_ROOT))
        for cf in (REPO_ROOT / "tests").rglob("conftest.py")
        for text in (cf.read_text(encoding="utf-8"),)
        if "benchmark_storage" in text and "file://.artifacts" in text
    ]
    assert not violators, f"hardcoded benchmark-storage URI in conftest — rebind via catalog.BENCHMARK_STORAGE_URI instead: {violators!r}"


def test_catalog_module_exposes_benchmark_storage_uri() -> None:
    """The catalog module exposes a non-empty benchmark-storage URI."""
    mod = importlib.import_module("tools.assay.composition.catalog")
    uri: object = getattr(mod, "BENCHMARK_STORAGE_URI", None)
    assert isinstance(uri, str), f"BENCHMARK_STORAGE_URI is absent or not str: {uri!r}"
    assert uri, f"BENCHMARK_STORAGE_URI is empty: {uri!r}"
