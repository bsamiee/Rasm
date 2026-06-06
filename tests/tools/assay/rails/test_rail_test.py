"""Test rail laws."""

from typing import TYPE_CHECKING

import pytest

from tools.assay.core.model import Completed, Language, Mode, TestRun
from tools.assay.core.status import RailStatus
from tools.assay.rails.test import _coverage_artifacts, _coverage_percent, _detail, _rows, TestParams  # noqa: PLC2701


if TYPE_CHECKING:
    from tests.tools.assay.conftest import AssayHarness


def test_coverage_rows_do_not_duplicate_plain_pytest() -> None:
    """Coverage mode uses coverage run plus post-processing rows, not a duplicate pytest run."""
    run_rows = _rows(Language.PYTHON, TestParams(coverage=True), Mode.RUN)
    client_rows = _rows(Language.PYTHON, TestParams(coverage=True), Mode.CLIENT)

    assert "coverage" in {tool.name for tool in run_rows}
    assert "pytest" not in {tool.name for tool in run_rows}
    assert {"coverage-json", "coverage-report"} <= {tool.name for tool in client_rows}


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
    done = Completed(("uv", "run", "coverage", "json"), 0, status=RailStatus.OK)

    assert _coverage_artifacts(assay_root.settings, ()) == ()
    assert _coverage_artifacts(assay_root.settings, (done,))[0].id == "coverage.json"
