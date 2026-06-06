"""Docs rail laws."""

from typing import TYPE_CHECKING

from expression import Error
import pytest

from tools.assay.core.model import Claim, Completed, Fault, fold, Language, Mode
from tools.assay.core.routing import Routed, Scope
from tools.assay.core.status import RailStatus
from tools.assay.rails import docs as docs_rail
from tools.assay.rails.docs import _strict, FaultedPromotion  # noqa: PLC2701


if TYPE_CHECKING:
    from tests.tools.assay.conftest import AssayHarness


def test_strict_promotes_empty_docs_report() -> None:
    """Strict docs checks promote empty/skipped reports at the rail boundary."""
    with pytest.raises(FaultedPromotion):
        _strict(fold(Claim.DOCS, "check", ()), strict=True)


def test_strict_preserves_real_docs_failures() -> None:
    """Strict mode does not rewrite real failed diagnostics."""
    failed = fold(Claim.DOCS, "check", (Completed(("mmdc",), 1, status=RailStatus.FAILED),))

    assert _strict(failed, strict=True).status is RailStatus.FAILED


def test_docs_outcomes_preserve_spawn_faults(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """Docs fan-out faults stay on the fault rail instead of collapsing to an empty report."""
    fault = Fault(("mmdc",), RailStatus.UNSUPPORTED, "missing mmdc")
    monkeypatch.setattr(docs_rail, "fan_out", lambda *_a, **_k: (Error(fault),))

    result = docs_rail._outcomes(
        Routed(Language.DOCS, Scope.CHANGED, files=("README.md",)),
        settings=assay_root.settings,
        scope=assay_root.scope(Claim.DOCS),
        claim=Claim.DOCS,
        verb="check",
        mode=Mode.CHECK,
    )

    assert result.is_error()
    assert result.error == fault
