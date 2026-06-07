"""Laws for type-checker routing over ast-grep probe fixtures."""

from pathlib import Path
import tomllib
from typing import TYPE_CHECKING

import anyio

from tools.assay.composition.catalog import BENCHMARK_STORAGE_URI
from tools.assay.core.engine import argv_for
from tools.assay.core.model import Check, Claim, Input, Language, Runner, Tool
from tools.assay.core.routing import place, Routed, Scope


if TYPE_CHECKING:
    from tests.tools.assay.conftest import AssayHarness


_REPO_ROOT = Path(__file__).resolve().parents[4]
_FAIL_PROBE = "tests/tools/ast-grep/fail/helper_import.py"
_TY_TOOL = Tool("ty", Runner.UV, ("ty", "check", "--no-progress"), Input.FILES, Language.PYTHON, Claim.STATIC)


def test_pyproject_benchmark_storage_matches_catalog_owner() -> None:
    """Benchmark storage URI is single-owned in catalog and mirrored in pytest addopts."""
    addopts = tomllib.loads((_REPO_ROOT / "pyproject.toml").read_text(encoding="utf-8"))["tool"]["pytest"]["addopts"]

    assert f"--benchmark-storage={BENCHMARK_STORAGE_URI}" in addopts


def test_explicit_ty_argv_still_typechecks_excluded_fail_probe() -> None:
    """Documented hazard: explicit argv bypasses pyproject excludes until routing strips probe paths."""

    async def run() -> int:
        completed = await anyio.run_process(["uv", "run", "ty", "check", _FAIL_PROBE], cwd=_REPO_ROOT, check=False)
        return completed.returncode

    assert anyio.run(run) != 0


def test_routed_ty_argv_omits_ast_grep_fail_probe(assay_root: AssayHarness) -> None:
    """Assay FILE routing drops probe fixtures so LIST/CHECK rows do not bypass ty excludes."""
    routed = Routed(Language.PYTHON, Scope.CHANGED, files=(_FAIL_PROBE, "tools/assay/core/model.py"))
    check = Check(tool=_TY_TOOL)

    assert place(routed, _TY_TOOL, settings=assay_root.settings) == (("tools/assay/core/model.py",),)
    assert _FAIL_PROBE not in argv_for(check, routed, settings=assay_root.settings, scope=None)
