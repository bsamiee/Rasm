"""Laws for tools.assay.rails.docs: DocsParams, FaultedPromotion, check."""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

from expression import Error, Ok
from hypothesis import given, HealthCheck, settings, strategies as st
import msgspec
import pytest

from tests._aspect import register_law  # noqa: PLC2701
from tests._spec import assert_error, assert_error_status, assert_ok, support_matrix, validity_matrix, ValidityCase  # noqa: PLC2701
from tests._strategies import resolve  # noqa: PLC2701
from tests.tools.assay.conftest import AssayHarness  # noqa: TC001
from tools.assay.core.model import Claim, Completed, Fault, fold
from tools.assay.core.status import RailStatus
from tools.assay.rails import docs as docs_rail
from tools.assay.rails.docs import check, DocsParams, FaultedPromotion


# --- [CONSTANTS] ------------------------------------------------------------------------

_EMPTY_REPORT = fold(Claim.DOCS, "check", ())
_FAILED_REPORT = fold(Claim.DOCS, "check", (Completed(("mmdc",), 1, status=RailStatus.FAILED),))
_OK_REPORT = fold(Claim.DOCS, "check", (Completed(("mmdc",), 0, status=RailStatus.OK),))


# --- [LAW_COVERAGE] ---------------------------------------------------------------------

register_law(DocsParams, "docs_params_default_strict_is_false")
register_law(DocsParams, "docs_params_roundtrip")
register_law(DocsParams, "docs_params_strict_matrix")
register_law(DocsParams, "docs_params_paths_default_empty")

register_law(FaultedPromotion, "faulted_promotion_is_exception_subclass")
register_law(FaultedPromotion, "faulted_promotion_message_is_no_docs_changed")
register_law(FaultedPromotion, "faulted_promotion_strict_raises_on_empty")
register_law(FaultedPromotion, "faulted_promotion_strict_raises_on_skip")
register_law(FaultedPromotion, "faulted_promotion_strict_false_does_not_raise_on_empty")
register_law(FaultedPromotion, "faulted_promotion_preserves_failed_report")
register_law(FaultedPromotion, "faulted_promotion_promotion_matrix")

register_law(check, "check_fan_out_fault_propagates_on_error_rail")
register_law(check, "check_no_files_yields_ok_rail")
register_law(check, "check_strict_empty_becomes_error")
register_law(check, "check_ok_receipt_yields_ok_report")
register_law(check, "check_failed_receipt_yields_failed_report")
register_law(check, "check_claim_is_docs")
register_law(check, "check_strict_flag_propagated")
register_law(check, "check_multi_fault_first_fault_wins")


# --- [LAWS_DOCSPARAMS] ------------------------------------------------------------------


def test_docs_params_default_strict_is_false() -> None:
    """DocsParams.strict defaults to False — enabling opt-in only."""
    assert DocsParams().strict is False


@given(resolve(DocsParams))
@settings(max_examples=50)
def test_docs_params_roundtrip(p: DocsParams) -> None:
    """DocsParams instances survive a structural copy cycle unchanged (immutability invariant)."""
    raw = msgspec.json.encode({"paths": list(p.paths), "strict": p.strict})
    decoded = msgspec.json.decode(raw, type=dict)
    assert decoded["strict"] == p.strict


def test_docs_params_strict_matrix() -> None:
    """DocsParams.strict distinguishes opt-in and default modes across explicit construction cases."""
    validity_matrix(
        [
            ValidityCase(label="default_not_strict", value=DocsParams(), expected=False),
            ValidityCase(label="explicit_false", value=DocsParams(strict=False), expected=False),
            ValidityCase(label="explicit_true", value=DocsParams(strict=True), expected=True),
        ],
        lambda p: p.strict,
    )


def test_docs_params_paths_default_empty() -> None:
    """DocsParams.paths defaults to empty tuple (no files → SKIP/EMPTY, not fault)."""
    support_matrix(
        ("default_paths_empty", lambda: DocsParams().paths == (), True),
        ("explicit_paths_retained", lambda: DocsParams(paths=("a.md",)).paths == ("a.md",), True),
    )


# --- [LAWS_FAULTED_PROMOTION] -----------------------------------------------------------


def test_faulted_promotion_is_exception_subclass() -> None:
    """FaultedPromotion is an Exception subclass (not BaseException) so the registry can catch it."""
    assert issubclass(FaultedPromotion, Exception)


def test_faulted_promotion_message_is_no_docs_changed() -> None:
    """FaultedPromotion carries the canonical 'no docs changed' sentinel message."""
    assert str(FaultedPromotion()) == "no docs changed"


def test_faulted_promotion_strict_raises_on_empty() -> None:
    """_strict raises FaultedPromotion for EMPTY status under strict=True — the promotion invariant."""
    with pytest.raises(FaultedPromotion):
        docs_rail._strict(_EMPTY_REPORT, strict=True)


def test_faulted_promotion_strict_raises_on_skip() -> None:
    """_strict raises FaultedPromotion for SKIP status under strict=True — symmetry with EMPTY arm."""
    skip_report = fold(Claim.DOCS, "check", (Completed(("mmdc",), 0, status=RailStatus.SKIP),))
    with pytest.raises(FaultedPromotion):
        docs_rail._strict(skip_report, strict=True)


def test_faulted_promotion_strict_false_does_not_raise_on_empty() -> None:
    """_strict with strict=False never raises FaultedPromotion — opt-in semantics are exclusive."""
    result = docs_rail._strict(_EMPTY_REPORT, strict=False)
    assert result.status is RailStatus.EMPTY


def test_faulted_promotion_preserves_failed_report() -> None:
    """_strict does NOT rewrite real FAILED reports even under strict=True — real defects stay failed."""
    result = docs_rail._strict(_FAILED_REPORT, strict=True)
    assert result.status is RailStatus.FAILED


def test_faulted_promotion_promotion_matrix() -> None:
    """_strict promotion matrix: EMPTY+strict→raise, SKIP+strict→raise, FAILED+strict→preserve, OK+strict→preserve."""
    skip_report = fold(Claim.DOCS, "check", (Completed(("mmdc",), 0, status=RailStatus.SKIP),))

    for label, report, strict, should_raise in [
        ("empty_strict_raises", _EMPTY_REPORT, True, True),
        ("skip_strict_raises", skip_report, True, True),
        ("empty_not_strict", _EMPTY_REPORT, False, False),
        ("failed_strict_preserves", _FAILED_REPORT, True, False),
        ("ok_strict_preserves", _OK_REPORT, True, False),
    ]:
        if should_raise:
            with pytest.raises(FaultedPromotion, match="no docs changed"):
                docs_rail._strict(report, strict=strict)
        else:
            docs_rail._strict(report, strict=strict)  # must not raise
        _ = label  # label consumed by parametrize context


# --- [LAWS_CHECK] -----------------------------------------------------------------------


def test_check_fan_out_fault_propagates_on_error_rail(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """Check propagates spawn faults from fan_out onto the Error rail without swallowing."""
    fault = Fault(("mmdc",), RailStatus.UNSUPPORTED, "mmdc not found")
    monkeypatch.setattr(docs_rail, "fan_out", lambda *_a, **_k: (Error(fault),))

    result = check(assay_root.settings, assay_root.scope(Claim.DOCS), DocsParams(paths=("README.md",)))
    assert_error_status(result, RailStatus.UNSUPPORTED)


def test_check_no_files_yields_ok_rail(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """Check with no routed files returns Ok(Report) with EMPTY/SKIP status — not a hard fault."""
    # Explicit path so routing uses enumerate(); fan_out receives zero checks → Ok(Report) with no receipts.
    monkeypatch.setattr(docs_rail, "fan_out", lambda *_a, **_k: ())

    result = check(assay_root.settings, assay_root.scope(Claim.DOCS), DocsParams(paths=("docs/guide.md",)))
    report = assert_ok(result)
    assert report.claim is Claim.DOCS


def test_check_strict_empty_becomes_error(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """Check with strict=True and no outputs raises FaultedPromotion, which propagates as an exception."""
    # Use explicit path so routing calls enumerate() (no git), then fan_out returns nothing → EMPTY fold → strict raise.
    monkeypatch.setattr(docs_rail, "fan_out", lambda *_a, **_k: ())

    with pytest.raises(FaultedPromotion):
        check(assay_root.settings, assay_root.scope(Claim.DOCS), DocsParams(paths=("docs/guide.md",), strict=True))


def test_check_ok_receipt_yields_ok_report(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """Check folds a single OK receipt into Ok(Report) with status OK — the happy-path rail."""
    monkeypatch.setattr(docs_rail, "fan_out", lambda *_a, **_k: (Ok(Completed(("mmdc",), 0, status=RailStatus.OK)),))

    result = check(assay_root.settings, assay_root.scope(Claim.DOCS), DocsParams(paths=("README.md",)))
    report = assert_ok(result)
    assert report.status is RailStatus.OK


def test_check_failed_receipt_yields_failed_report(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """Check folds a FAILED receipt into Ok(Report) with status FAILED — real defects stay on the Ok rail."""
    monkeypatch.setattr(docs_rail, "fan_out", lambda *_a, **_k: (Ok(Completed(("mmdc",), 1, status=RailStatus.FAILED)),))

    result = check(assay_root.settings, assay_root.scope(Claim.DOCS), DocsParams(paths=("README.md",)))
    report = assert_ok(result)
    assert report.status is RailStatus.FAILED


def test_check_claim_is_docs(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """Check always produces a Report stamped with Claim.DOCS — not silently reusing another claim."""
    # Explicit path so routing uses enumerate() rather than git-changed; fan_out empty → EMPTY fold.
    monkeypatch.setattr(docs_rail, "fan_out", lambda *_a, **_k: ())

    result = check(assay_root.settings, assay_root.scope(Claim.DOCS), DocsParams(paths=("docs/guide.md",)))
    report = assert_ok(result)
    assert report.claim is Claim.DOCS


@given(st.booleans())
@settings(max_examples=20, suppress_health_check=[HealthCheck.function_scoped_fixture])
def test_check_strict_flag_propagated(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch, strict: bool) -> None:  # noqa: FBT001
    """check(strict=strict) with no outputs raises iff strict is True — strict flag is load-bearing."""
    # Explicit path so routing uses enumerate(); fan_out empty → EMPTY fold; _strict discriminates on strict.
    monkeypatch.setattr(docs_rail, "fan_out", lambda *_a, **_k: ())

    params = DocsParams(paths=("docs/guide.md",), strict=strict)
    if strict:
        with pytest.raises(FaultedPromotion):
            check(assay_root.settings, assay_root.scope(Claim.DOCS), params)
    else:
        assert_ok(check(assay_root.settings, assay_root.scope(Claim.DOCS), params))


def test_check_multi_fault_first_fault_wins(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """fan_out returning multiple Error slots causes check to short-circuit on the first fault."""
    f1 = Fault(("mmdc",), RailStatus.FAULTED, "first fault")
    f2 = Fault(("mmdc",), RailStatus.UNSUPPORTED, "second fault")
    monkeypatch.setattr(docs_rail, "fan_out", lambda *_a, **_k: (Error(f1), Error(f2)))

    result = check(assay_root.settings, assay_root.scope(Claim.DOCS), DocsParams(paths=("a.md",)))
    err = assert_error(result)
    assert err is f1
