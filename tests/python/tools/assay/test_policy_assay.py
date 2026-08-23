"""Assay-bound policy laws that depend on concrete ``tools.assay`` owners."""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

from pathlib import Path  # module-level _PYPROJECT assignment prevents deferral
import tomllib

from tests.python._testkit.runtime import REPO_ROOT
from tests.python._testkit.spec import validity_matrix
from tools.assay.composition.catalog import BENCHMARK_STORAGE_URI
from tools.assay.core.model import Language


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


# --- [GOVERNOR_ROSTER_POLICY]


def test_every_declared_governor_names_a_real_root_file() -> None:
    """Each lane governor resolves on disk, so no roster entry is an escalation that can never fire.

    A misspelled governor never escalates and never raises; the lane simply stops re-checking itself
    when the config that decides every one of its verdicts moves.
    """
    validity_matrix(
        [(f"{lang.value}:{name}", name, True) for lang in Language for name in sorted(lang.governors)], lambda n: (REPO_ROOT / n).is_file()
    )
