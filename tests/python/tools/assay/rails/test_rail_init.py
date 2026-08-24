"""Init rail laws: blessed file sets, the member-row append, and the census gate's refusals."""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

from pathlib import Path
import tomllib

import pytest

from assay.composition.store import ArtifactScope
from assay.core.model import Claim, RailStatus
from assay.rails.init import check, InitParams, python_app, python_lib
from tests.python._testkit.spec import assert_error, assert_ok
from tests.python.tools.assay.kit import assay_settings, SeamExecutor


# --- [CONSTANTS] ------------------------------------------------------------------------

COVERS: tuple[object, ...] = (check, InitParams, python_app, python_lib)

_ROOT_MANIFEST = """\
[tool.uv.workspace]
members = ["libs/contracts", "libs/python/*", "tools/assay"]
"""


# --- [OPERATIONS] -----------------------------------------------------------------------


def _seeded(root: Path) -> None:
    # The contracts estate is a literal row outside the governed libs/python glob; runtime is the glob-admitted existing member.
    (root / "pyproject.toml").write_text(_ROOT_MANIFEST, encoding="utf-8")
    for member in ("libs/contracts", "libs/python/runtime", "tools/assay"):
        (root / member).mkdir(parents=True)
        (root / member / "pyproject.toml").write_text("[project]\nname='x'\nversion='0'\n", encoding="utf-8")


def test_python_lib_mints_the_blessed_file_set(tmp_path: Path) -> None:
    """A libs member lands its manifest, rasm namespace seat, and suite conftest, and the census stays green."""
    _seeded(tmp_path)
    settings = assay_settings(tmp_path)
    scope = ArtifactScope.open(settings, Claim.INIT)
    report = assert_ok(python_lib(settings, scope, InitParams(paths=("libs/python/probe",)), SeamExecutor()))
    assert report.status is RailStatus.OK
    manifest = tomllib.loads((tmp_path / "libs/python/probe/pyproject.toml").read_text(encoding="utf-8"))
    assert manifest["project"]["name"] == "rasm-probe"
    assert manifest["tool"]["uv"]["build-backend"]["module-name"] == "rasm.probe"
    assert (tmp_path / "libs/python/probe/rasm/probe/__init__.py").is_file()
    assert (tmp_path / "tests/python/libs/probe/conftest.py").is_file()
    assert assert_ok(check(settings, scope, InitParams(), SeamExecutor())).status is RailStatus.OK


def test_python_app_appends_its_explicit_member_row(tmp_path: Path) -> None:
    """An apps project mints under its app tree and lands the member row a glob cannot admit."""
    _seeded(tmp_path)
    settings = assay_settings(tmp_path)
    report = assert_ok(python_app(settings, ArtifactScope.open(settings, Claim.INIT), InitParams(paths=("apps/alpha/engine",)), SeamExecutor()))
    assert report.status is RailStatus.OK
    manifest = tomllib.loads((tmp_path / "pyproject.toml").read_text(encoding="utf-8"))
    assert "apps/alpha/engine" in manifest["tool"]["uv"]["workspace"]["members"]
    assert (tmp_path / "apps/alpha/engine/engine/__init__.py").is_file()
    assert assert_ok(check(settings, ArtifactScope.open(settings, Claim.INIT), InitParams(), SeamExecutor())).status is RailStatus.OK


@pytest.mark.parametrize("target", ["libs/python/runtime", "libs/contracts", "libs/nested/too/deep", "libs/python/Bad_Name"])
def test_python_lib_refuses_unlawful_targets(tmp_path: Path, target: str) -> None:
    """An existing member, a foreign tree, and a non-slug name each refuse before any write."""
    _seeded(tmp_path)
    fault = assert_error(python_lib((s := assay_settings(tmp_path)), ArtifactScope.open(s, Claim.INIT), InitParams(paths=(target,)), SeamExecutor()))
    assert fault.status is RailStatus.FAULTED


def test_census_refuses_orphans_and_ghost_rows(tmp_path: Path) -> None:
    """An undeclared on-disk manifest and a dangling member row each seat a FAILED census leaf."""
    _seeded(tmp_path)
    (tmp_path / "apps/beta/engine").mkdir(parents=True)
    (tmp_path / "apps/beta/engine/pyproject.toml").write_text("[project]\nname='beta'\nversion='0'\n", encoding="utf-8")
    (tmp_path / "pyproject.toml").write_text(
        '[tool.uv.workspace]\nmembers = ["libs/contracts", "libs/python/*", "tools/assay", "apps/gone/engine"]\n', encoding="utf-8"
    )
    report = assert_ok(check((s := assay_settings(tmp_path)), ArtifactScope.open(s, Claim.INIT), InitParams(), SeamExecutor()))
    assert report.status is RailStatus.FAILED
    evidence = "\n".join((*report.notes, *(row.text for row in report.results)))
    assert "apps/beta/engine" in evidence
    assert "apps/gone/engine" in evidence
