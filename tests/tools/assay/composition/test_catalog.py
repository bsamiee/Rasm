"""Catalog laws for Python tool storage ownership."""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------------

import pytest

from tools.assay.composition.catalog import BENCHMARK_STORAGE_URI, select, TOOLS
from tools.assay.core.model import Claim, Input, Language, Mode, Runner


# --- [STORAGE_OWNERSHIP] ---------------------------------------------------------------


def test_python_storage_rows_are_catalog_owned() -> None:
    """Python benchmark and mutation storage are declared as reusable tool-row policy."""
    rows = select(Claim.TEST, Language.PYTHON)
    benchmark = next(t for t in rows if t.name == "pytest-benchmark")
    mutation = next(t for t in rows if t.name == "mutmut" and t.mode is Mode.MUTATION)

    assert f"--benchmark-storage={BENCHMARK_STORAGE_URI}" in benchmark.command
    assert (mutation.runner, mutation.input, mutation.thunk) == (Runner.UV, Input.NONE, None)
    assert mutation.command == ("mutmut", "run")
    assert mutation.groups == ("mutation",)
    assert mutation.stage.root == ".artifacts/python/mutmut/work"
    assert mutation.stage.project is True
    assert mutation.stage.inputs == ("pyproject.toml", ".gitignore", "tools/assay", "tests/conftest.py", "tests/tools/assay")


# --- [CENSUS_INVARIANT] ----------------------------------------------------------------


@pytest.mark.parametrize("claim", list(Claim))
def test_catalog_census_every_tool_selects_back(claim: Claim) -> None:
    """``_census`` invariant per claim: every catalog row selects back through its own claim/language axes."""
    assert all(t in select(t.claim, t.language) for t in TOOLS if t.claim is claim)
