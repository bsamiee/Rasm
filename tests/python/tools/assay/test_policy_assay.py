"""Assay-bound policy laws: the ``tools.assay`` couplings split out of the generic policy layer.

The generic ``tests.python._testkit.test_policy`` layer owns SUT-agnostic policy (law-coverage,
markers, litter) parameterized over ``SUT_PACKAGES`` and imports no SUT. These laws are the ones that
genuinely reference ``tools.assay``: the benchmark-storage single-owner contract whose canonical owner
is ``tools.assay.composition.catalog.BENCHMARK_STORAGE_URI``, the addopts↔catalog URI identity, and the
catalog-import surface. They live beside the assay suite so the testkit stays project-agnostic.
"""

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
    """Return the ``[tool.pytest.ini_options] addopts`` list as a flat string list; empty when absent."""
    data: dict[str, object] = tomllib.loads(_PYPROJECT.read_text(encoding="utf-8"))
    match data:
        case {"tool": {"pytest": {"addopts": list() as addopts}}}:
            return [str(o) for o in addopts]
        case _:
            return []


def _benchmark_storage_flags(addopts: list[str]) -> list[str]:
    """Return addopts entries that start with ``--benchmark-storage``."""
    return [o for o in addopts if o.startswith("--benchmark-storage")]


# --- [BENCHMARK_STORAGE_POLICY]


def test_benchmark_storage_single_owner_in_addopts() -> None:
    """``--benchmark-storage`` appears in addopts exactly once.

    Falsifiable by: adding a second ``--benchmark-storage=...`` flag in addopts or a duplicate inline
    pytest invocation — the single-owner policy breaks and the storage path becomes ambiguous.
    """
    flags = _benchmark_storage_flags(_addopts())
    assert len(flags) == 1, f"Expected exactly one --benchmark-storage in addopts, found {len(flags)}: {flags!r}"


def test_benchmark_storage_uri_matches_catalog_constant() -> None:
    """The ``--benchmark-storage`` URI in addopts equals ``catalog.BENCHMARK_STORAGE_URI``.

    Falsifiable by: updating the catalog constant without updating pyproject.toml, or vice versa —
    the benchmark runner would write to a different directory than the catalog's storage declaration.
    """
    flags = _benchmark_storage_flags(_addopts())
    assert flags, "--benchmark-storage not found in addopts (prerequisite for URI-match law)"
    _, _, uri = flags[0].partition("=")
    assert uri == BENCHMARK_STORAGE_URI, f"addopts URI {uri!r} != catalog.BENCHMARK_STORAGE_URI {BENCHMARK_STORAGE_URI!r}"


def test_benchmark_storage_not_duplicated_in_conftest_files() -> None:
    """No conftest.py file under ``tests/`` carries a ``--benchmark-storage`` flag.

    Falsifiable by: adding a per-suite ``pytest.ini_options`` or ``addopts`` override in a sub-conftest
    that duplicates or overrides the benchmark storage path — single-owner policy breaks silently.
    """
    violators: list[str] = [
        str(cf.relative_to(REPO_ROOT)) for cf in (REPO_ROOT / "tests").rglob("conftest.py") if "--benchmark-storage" in cf.read_text(encoding="utf-8")
    ]
    assert not violators, f"--benchmark-storage found in conftest file(s) outside addopts: {violators!r}"


def test_catalog_module_exposes_benchmark_storage_uri() -> None:
    """``tools.assay.composition.catalog`` is importable and ``BENCHMARK_STORAGE_URI`` is a non-empty str.

    Falsifiable by: renaming or removing ``BENCHMARK_STORAGE_URI`` from catalog.py — the single-owner
    policy tests above would also fail, but this law isolates the catalog as the canonical declaration.
    """
    mod = importlib.import_module("tools.assay.composition.catalog")
    uri: object = getattr(mod, "BENCHMARK_STORAGE_URI", None)
    assert isinstance(uri, str), f"BENCHMARK_STORAGE_URI is absent or not str: {uri!r}"
    assert uri, f"BENCHMARK_STORAGE_URI is empty: {uri!r}"
