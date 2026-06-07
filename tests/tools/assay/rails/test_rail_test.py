"""Test rail laws."""

from typing import TYPE_CHECKING

from expression import Ok
from expression.collections import block
import pytest

from tools.assay.composition.settings import ArtifactScope
from tools.assay.core.model import Claim, Completed, Language, Mode, MutationLane, TestRun
from tools.assay.core.routing import Routed, Scope
from tools.assay.core.status import RailStatus
from tools.assay.rails import test as test_rail
from tools.assay.rails.test import _coverage_artifacts, _coverage_percent, _detail, _rows, TestParams  # noqa: PLC2701


if TYPE_CHECKING:
    from tests.tools.assay.conftest import AssayHarness


def test_coverage_rows_do_not_duplicate_plain_pytest() -> None:
    """Coverage mode uses coverage run plus post-processing rows, not a duplicate pytest run."""
    run_rows = _rows(Language.PYTHON, TestParams(coverage=True), Mode.RUN)
    client_rows = _rows(Language.PYTHON, TestParams(coverage=True), Mode.CLIENT)

    assert "coverage" in {tool.name for tool in run_rows}
    assert "pytest" not in {tool.name for tool in run_rows}
    assert {"coverage-json", "coverage-xml", "coverage-lcov", "coverage-report"} <= {tool.name for tool in client_rows}


def test_mutation_lanes_are_explicit_behavior_controls() -> None:
    """Mutation lanes select mutation tools explicitly instead of a weak boolean flag."""
    off_rows = _rows(Language.PYTHON, TestParams(mutation=MutationLane.OFF), Mode.MUTATION)
    changed_rows = _rows(Language.PYTHON, TestParams(mutation=MutationLane.CHANGED), Mode.MUTATION)
    full_rows = _rows(Language.PYTHON, TestParams(mutation=MutationLane.FULL), Mode.MUTATION)

    assert off_rows == ()
    assert {tool.mode for tool in changed_rows} == {Mode.MUTATION}
    assert {tool.name for tool in full_rows} == {tool.name for tool in changed_rows}


def test_list_counts_roster_rows_and_keeps_discovery_failures_in_notes(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """Mixed discovery results still emit direct test identities, not process defect rows."""
    ok = Completed(("pytest", "--collect-only"), 0, stdout=b"tests/a.py::test_one\ntests/a.py::test_two\n", status=RailStatus.OK)
    failed = Completed(("dotnet", "test", "--list-tests"), 1, stderr=b"no project", status=RailStatus.FAILED)
    scope = ArtifactScope.open(assay_root.settings, Claim.TEST)
    routed = Routed(language=Language.PYTHON, scope=Scope.CHANGED)

    monkeypatch.setattr(test_rail, "_routed", lambda *_args, **_kwargs: Ok(block.of_seq((routed,))))
    monkeypatch.setattr(test_rail, "_dispatch", lambda *_args, **_kwargs: (Ok(ok), Ok(failed)))

    outcome = test_rail.list(assay_root.settings, scope, TestParams(limit=1))

    assert outcome.is_ok()
    report = outcome.ok
    assert report.status is RailStatus.OK
    assert report.counts.total == 1
    assert [row.id for row in report.results] == ["tests/a.py::test_one"]
    assert "discovery: total=2 returned=1" in report.notes
    assert any(note == "discovery failed: dotnet test --list-tests: no project" for note in report.notes)
    roster = next(artifact for artifact in report.artifacts if artifact.id == "test-roster")
    assert roster.lines == 2
    assert scope.store.read_text_path(roster.path).splitlines() == ["tests/a.py::test_one", "tests/a.py::test_two"]


def test_coverage_report_total_populates_test_detail() -> None:
    """Coverage report --format=total stdout becomes TestRun.coverage."""
    done = Completed(("uv", "run", "coverage", "report", "--format=total"), 0, stdout=b"87.5\n", status=RailStatus.OK)

    assert _coverage_percent((done,)) == pytest.approx(87.5)
    detail = _detail((done,), TestParams(coverage=True))
    assert isinstance(detail, TestRun)
    assert detail.coverage == pytest.approx(87.5)


def test_coverage_fail_under_keeps_percent_on_failed_receipt() -> None:
    """Coverage report owns fail_under via its return code while Assay still keeps the measured total."""
    done = Completed(("uv", "run", "coverage", "report", "--format=total"), 2, stdout=b"87.5\n", status=RailStatus.FAILED)

    detail = _detail((done,), TestParams(coverage=True))
    assert isinstance(detail, TestRun)
    assert detail.coverage == pytest.approx(87.5)


def test_coverage_artifacts_require_current_output(assay_root: AssayHarness) -> None:
    """Existing coverage files are reported only after the current run generates them."""
    root = assay_root.root / ".artifacts/python/coverage"
    root.mkdir(parents=True)
    (root / "coverage.json").write_text("{}", encoding="utf-8")
    done = Completed(("uv", "run", "coverage", "json", "-o", ".artifacts/python/coverage/coverage.json"), 0, status=RailStatus.OK)

    assert _coverage_artifacts(assay_root.settings, ()) == ()
    artifact = _coverage_artifacts(assay_root.settings, (done,))[0]
    assert artifact.id == "coverage.json"
    assert artifact.path.endswith(f"/test/{assay_root.settings.run_id}/coverage/coverage.json")
