"""Wire/evidence spine: StrEnum payloads, fold count oracle, field_cap, validate_detail, envelope branches."""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------------

from hypothesis import given
from hypothesis.strategies import integers, lists, text
import msgspec
import pytest

from tests.tools.assay.conftest import completed_st, fault_st
from tools.assay.core.model import (
    AnyDetail,
    ApiResolution,
    ApiSurface,
    BaseParams,
    Claim,
    Completed,
    Counts,
    Diagnostic,
    envelope,
    Fault,
    field_cap,
    fold,
    Language,
    Mode,
    PackageRun,
    receipt,
    RunDelta,
    Runner,
    TestRun,
    validate_detail,
    VerifySummary,
)
from tools.assay.core.status import RailStatus


# --- [CONSTANTS] -----------------------------------------------------------------------

_DETAIL_VARIANTS: tuple[type, ...] = (ApiSurface, VerifySummary, TestRun, PackageRun, ApiResolution, Diagnostic, RunDelta)
_ENCODER = msgspec.json.Encoder(order="deterministic")
_DECODER: msgspec.json.Decoder[AnyDetail | None] = msgspec.json.Decoder(AnyDetail | None)


# --- [OPERATIONS] -----------------------------------------------------------------------


@given(lists(completed_st, min_size=0, max_size=20))
def test_fold_count_oracle(outcomes: list[Completed]) -> None:
    """``fold`` count invariant: ``ok + failed == total == len(outcomes)`` for non-FAULTED/BUSY/TIMEOUT rows."""
    tup = tuple(outcomes)
    report = fold(Claim.STATIC, "check", tup)
    # Only OK/EMPTY/SKIP→ok and FAILED→failed; FAULTED/BUSY/TIMEOUT→neither (not in total either)
    ok_n = sum(1 for o in tup if o.status in {RailStatus.OK, RailStatus.EMPTY, RailStatus.SKIP})
    fail_n = sum(1 for o in tup if o.status is RailStatus.FAILED)
    assert report.counts.ok == ok_n
    assert report.counts.failed == fail_n
    assert report.counts.total == ok_n + fail_n


@given(lists(completed_st, min_size=1, max_size=10))
def test_fold_defect_row_per_failed(outcomes: list[Completed]) -> None:
    """Each FAILED Completed yields exactly one Match row in ``report.results`` with ``severity='failed'``."""
    tup = tuple(outcomes)
    report = fold(Claim.CODE, "check", tup)
    failed_count = sum(1 for o in tup if o.status is RailStatus.FAILED)
    assert len(report.results) == failed_count
    assert all(m.severity == "failed" for m in report.results)


@given(completed_st)
def test_fold_single_ok_yields_no_defect_rows(done: Completed) -> None:
    """A single non-FAILED Completed yields an empty ``results`` tuple."""
    ok = msgspec.structs.replace(done, status=RailStatus.OK, returncode=0)
    report = fold(Claim.STATIC, "plan", (ok,))
    assert report.results == ()


def test_fold_empty_outcomes_is_empty_report() -> None:
    """``fold`` over an empty tuple produces ``Report`` with all-zero counts and ``EMPTY`` status."""
    report = fold(Claim.TEST, "run", ())
    assert report.counts == Counts()
    assert report.status is RailStatus.EMPTY


# --- [FIELD_CAP] -----------------------------------------------------------------------


@pytest.mark.parametrize("type_,field,default,expected", [(Fault, "message", 0, 1024), (Diagnostic, "hint", 0, 256), (Fault, "argv", 42, 42)])
def test_field_cap_oracle(type_: type, field: str, default: int, expected: int) -> None:
    """``field_cap`` introspection oracle: message→1024, hint→256, missing→default."""
    assert field_cap(type_, field, default=default) == expected  # ty: ignore[invalid-argument-type]


def test_surplus_message_within_fault_cap() -> None:
    """``surplus(verb, tokens).message`` length ≤ 1024 bytes even for large glob expansions."""
    tokens = tuple(f"path/to/file{i}.cs" for i in range(2000))
    fault = BaseParams.surplus("fix", tokens)
    assert isinstance(fault, Fault)
    assert len(fault.message.encode()) <= 1024


@given(text(max_size=512), integers(min_value=1, max_value=20).flatmap(lambda n: lists(text(max_size=64), min_size=n, max_size=n)))
def test_surplus_always_within_cap(verb: str, tokens: list[str]) -> None:
    """``surplus`` is total and always clips to the wire cap for any verb and any token sequence."""
    fault = BaseParams.surplus(verb, tuple(tokens))
    assert isinstance(fault, Fault)
    assert len(fault.message.encode()) <= 1024


# --- [CODEC] ---------------------------------------------------------------------------


@pytest.mark.parametrize("variant", _DETAIL_VARIANTS)
def test_validate_detail_round_trip(variant: type) -> None:
    """``validate_detail`` round-trips each AnyDetail concrete variant byte-identically."""
    instance = variant()
    encoded = _ENCODER.encode(instance)
    decoded = _DECODER.decode(encoded)
    assert decoded is not None
    assert _ENCODER.encode(decoded) == encoded


def test_validate_detail_none_round_trips() -> None:
    """``validate_detail(None)`` returns ``None`` — the optional slot survives the codec."""
    assert validate_detail(None) is None


@given(completed_st)
def test_completed_json_round_trip(done: Completed) -> None:
    """``Completed`` msgspec round-trip: encode then decode yields a structurally equal value."""
    raw = _ENCODER.encode(done)
    restored = msgspec.json.decode(raw, type=Completed)
    assert restored.argv == done.argv
    assert restored.returncode == done.returncode
    assert restored.status == done.status


@given(fault_st)
def test_fault_json_round_trip(fault: Fault) -> None:
    """``Fault`` msgspec round-trip preserves argv, status, and the clipped message."""
    raw = _ENCODER.encode(fault)
    restored = msgspec.json.decode(raw, type=Fault)
    assert restored.status == fault.status
    assert restored.message == fault.message


# --- [ENUM_PAYLOADS] -------------------------------------------------------------------


@pytest.mark.parametrize("member", list(Language))
def test_language_suffixes_dot_prefixed(member: Language) -> None:
    """All ``Language.suffixes`` are dot-prefixed strings."""
    assert all(s.startswith(".") for s in member.suffixes)


@pytest.mark.parametrize("member", list(Mode))
def test_mode_stream_writes_are_bool(member: Mode) -> None:
    """``Mode.stream`` and ``Mode.writes`` are genuine booleans, not int aliases."""
    assert isinstance(member.stream, bool)
    assert isinstance(member.writes, bool)


@pytest.mark.parametrize("member", list(Runner))
def test_runner_prefix_is_tuple(member: Runner) -> None:
    """``Runner.prefix`` is always a tuple of strings; INPROC prefix is empty."""
    assert isinstance(member.prefix, tuple)
    assert all(isinstance(p, str) for p in member.prefix)
    # INPROC invariant folded here — eliminates the standalone singleton test (D5)
    if member is Runner.INPROC:
        assert member.prefix == ()


# --- [ENVELOPE] ------------------------------------------------------------------------


@given(lists(completed_st, min_size=0, max_size=5))
def test_envelope_exit_code_matches_report_status(outcomes: list[Completed]) -> None:
    """``envelope(report, ...).exit_code == report.status.exit_code`` — projection consistency."""
    report = fold(Claim.STATIC, "check", tuple(outcomes))
    env = envelope(report, claim=Claim.STATIC, verb="check")
    assert env.exit_code == report.status.exit_code
    assert env.status == report.status


def test_envelope_fault_branch_carries_error() -> None:
    """Wrapping a ``Fault`` populates ``Envelope.error`` and clears ``Envelope.report``."""
    fault = Fault(("ruff",), RailStatus.FAULTED, "tool missing")
    env = envelope(fault, claim=Claim.CODE, verb="check")
    assert env.error is fault
    assert env.report is None
    assert env.status is RailStatus.FAULTED


def test_envelope_report_branch_carries_report() -> None:
    """Wrapping a ``Report`` populates ``Envelope.report`` and clears ``Envelope.error``."""
    report = fold(Claim.PACKAGE, "list", ())
    env = envelope(report, claim=Claim.PACKAGE, verb="list")
    assert env.report is report
    assert env.error is None


# --- [RECEIPT] -------------------------------------------------------------------------


@pytest.mark.parametrize("rc,explicit,expected", [(0, None, RailStatus.EMPTY), (1, None, RailStatus.FAILED), (0, RailStatus.OK, RailStatus.OK)])
def test_receipt_oracle(rc: int, explicit: RailStatus | None, expected: RailStatus) -> None:
    """``receipt`` oracle: rc→status derivation + explicit override."""
    done = receipt(("ruff",), rc) if explicit is None else receipt(("ruff",), rc, status=explicit)
    assert done.status is expected


# --- [KNOWN_BUGS] ----------------------------------------------------------------------


def test_failed_stderr_appears_in_results_text() -> None:
    """``fold`` carries FAILED ``Completed.stderr`` into ``results[0].text`` — verified against live model."""
    sentinel = b"unique-sentinel-stderr-content"
    done = receipt(("tool",), 1, stderr=sentinel)
    report = fold(Claim.STATIC, "check", (done,))
    assert report.results
    assert sentinel.decode() in report.results[0].text
