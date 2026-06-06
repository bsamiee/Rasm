"""Catalog laws for Python tool storage ownership."""

from tools.assay.composition.catalog import select
from tools.assay.core.model import Claim, Input, Language, Mode, Runner


def test_python_storage_rows_are_catalog_owned() -> None:
    """Python benchmark and mutation storage are declared as reusable tool-row policy."""
    rows = select(Claim.TEST, Language.PYTHON)
    benchmark = next(t for t in rows if t.name == "pytest-benchmark")
    mutation = next(t for t in rows if t.name == "mutmut" and t.mode is Mode.MUTATION)

    assert "--benchmark-storage=file://.artifacts/python/benchmarks" in benchmark.command
    assert (mutation.runner, mutation.input, mutation.thunk) == (Runner.UV, Input.NONE, None)
    assert mutation.stage.root == ".artifacts/python/mutmut/work"
    assert mutation.stage.project is True
    assert mutation.stage.inputs == ("pyproject.toml", ".gitignore", "tools/assay", "tests/conftest.py", "tests/tools/assay")
