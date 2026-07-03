"""Assay-bound policy laws that depend on concrete ``tools.assay`` owners."""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

from pathlib import Path  # noqa: TC003  # module-level _PYPROJECT assignment prevents deferral
import tomllib

from tests.python._testkit.runtime import REPO_ROOT
from tools.assay.composition.catalog import BENCHMARK_STORAGE_URI


# --- [CONSTANTS] ------------------------------------------------------------------------

_PYPROJECT: Path = REPO_ROOT / "pyproject.toml"

# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [BENCHMARK_STORAGE_POLICY]


def test_benchmark_storage_addopts_pins_catalog_uri() -> None:
    """Exactly one ``--benchmark-storage`` rides addopts and its URI equals the catalog owner constant.

    A drifted or duplicated pin lets pytest-benchmark fall back to its repo-root ``.benchmarks`` default,
    which the litter-containment gate then catches only after the fact.
    """
    data: dict[str, object] = tomllib.loads(_PYPROJECT.read_text(encoding="utf-8"))
    match data:
        case {"tool": {"pytest": {"addopts": list() as addopts}}}:
            flags = [str(o) for o in addopts if str(o).startswith("--benchmark-storage")]
        case _:
            flags = []
    assert len(flags) == 1, f"expected exactly one --benchmark-storage in addopts, found {len(flags)}: {flags!r}"
    _, _, uri = flags[0].partition("=")
    assert uri == BENCHMARK_STORAGE_URI, f"addopts URI {uri!r} != catalog.BENCHMARK_STORAGE_URI {BENCHMARK_STORAGE_URI!r}"
