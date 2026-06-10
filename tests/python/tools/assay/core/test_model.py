"""Model spine laws: wire round-trip encode-probe, the fold count oracle, envelope projection, enum payloads.

Every wire struct registered in the conftest is swept through assert_roundtrip (encode, decode,
re-encode byte-identity), embodying the former session-fixture validation gate as a collectable,
falsifiable law. The fold count invariant and Report arithmetic are asserted via assert_counts_consistent.
"""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

from typing import get_args

from hypothesis import example, given, strategies as st, target
import msgspec
import msgspec.inspect as msgspec_inspect
import pytest

from tests.python._testkit._aspect import register_law, register_laws, spec  # sibling test-internal module; `_`-named by S1 design
from tests.python._testkit._spec import assert_roundtrip, idempotent, metamorphic, support_matrix  # sibling test-internal oracle surface
from tests.python.tools.assay.conftest import (
    api_resolution_st,
    api_source_st,
    api_surface_st,
    artifact_st,
    assert_counts_consistent,
    binds_st,
    check_st,
    completed_st,
    counts_st,
    detail_st,
    diagnostic_st,
    envelope_st,
    fault_st,
    match_st,
    package_run_st,
    report_st,
    run_delta_st,
    run_snapshot_st,
    stage_st,
    test_run_st,
    tool_st,
    verify_summary_st,
    WIRE_ENCODER,
)
from tools.assay.core.model import (
    AnyDetail,
    ApiResolution,
    ApiSource,
    ApiSurface,
    Artifact,
    ArtifactKind,
    Base,
    BaseParams,
    Bind,
    Check,
    Claim,
    Completed,
    Counts,
    Detail,
    Diagnostic,
    Envelope,
    envelope,
    Fault,
    field_cap,
    fold,
    Input,
    Language,
    Match,
    Mode,
    MutationLane,
    PackageRun,
    receipt,
    Report,
    RunDelta,
    Runner,
    RunSnapshot,
    SourceKind,
    Stage,
    SymbolShape,
    TestRun,
    Tool,
    validate_detail,
    VerifySummary,
    wire_encode,
    wire_safe,
)
from tools.assay.core.status import RailStatus


# --- [CONSTANTS] ------------------------------------------------------------------------

# get_args avoids a manual parallel list that would drift from the union definition.
_DETAIL_VARIANTS: tuple[type[Detail], ...] = get_args(AnyDetail.__value__)

# Each row drives one register_law call so coverage is total — adding a wire struct here
# is the only registration required.
_WIRE_ROWS: tuple[tuple[type[Base], st.SearchStrategy[Base]], ...] = (
    (Stage, stage_st),
    (Tool, tool_st),
    (Check, check_st),
    (Artifact, artifact_st),
    (Completed, completed_st),
    (Fault, fault_st),
    (Counts, counts_st),
    (Match, match_st),
    (ApiSource, api_source_st),
    (ApiSurface, api_surface_st),
    (VerifySummary, verify_summary_st),
    (TestRun, test_run_st),
    (PackageRun, package_run_st),
    (ApiResolution, api_resolution_st),
    (Diagnostic, diagnostic_st),
    (RunSnapshot, run_snapshot_st),
    (RunDelta, run_delta_st),
    (Report, report_st),
    (Envelope, envelope_st),
)

_BARE_STRENUM_CLASSES: tuple[type[Claim | SourceKind | ArtifactKind | MutationLane | SymbolShape], ...] = (
    Claim,
    SourceKind,
    ArtifactKind,
    MutationLane,
    SymbolShape,
)


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [WIRE_ROUNDTRIP]


@pytest.mark.mutation
@pytest.mark.parametrize("subject, strategy", _WIRE_ROWS, ids=[row[0].__name__ for row in _WIRE_ROWS])
def test_wire_struct_round_trips(subject: type[Base], strategy: st.SearchStrategy[Base]) -> None:
    """Every conftest-registered wire struct survives the deterministic codec byte-identically.

    A non-deterministic codec or a non-decode-clean field defect dies on the re-encode identity step.
    """

    @given(strategy)
    def _probe(value: Base) -> None:
        assert_roundtrip(value, subject, codec=WIRE_ENCODER)

    _probe()


@given(detail_st)
def test_any_detail_variant_round_trips(detail: AnyDetail) -> None:
    """Every AnyDetail variant subclasses Detail and survives the deterministic codec byte-identically."""
    assert isinstance(detail, Detail)
    assert_roundtrip(detail, type(detail), codec=WIRE_ENCODER)


def test_any_detail_tags_are_injective() -> None:
    """Every AnyDetail variant carries a unique msgspec tag — wire disambiguation is injective."""
    tags = [t.tag for v in _DETAIL_VARIANTS if isinstance(t := msgspec_inspect.type_info(v), msgspec_inspect.StructType)]
    assert len(tags) == len(set(tags)), f"duplicate tags: {tags}"


@given(detail_st)
def test_wire_struct_forbids_unknown_fields(detail: AnyDetail) -> None:
    """The shared Base policy forbids unknown fields — a surplus key is rejected at decode."""
    raw: dict[str, object] = msgspec.json.decode(WIRE_ENCODER.encode(detail), type=dict)
    raw["__probe__"] = 1
    with pytest.raises(msgspec.ValidationError, match="__probe__"):
        msgspec.json.decode(msgspec.json.encode(raw), type=type(detail))


# --- [FOLD]


@pytest.mark.mutation
@given(st.lists(completed_st, min_size=0, max_size=20))
def test_fold_count_oracle(outcomes: list[Completed]) -> None:
    """Fold count invariant: OK/EMPTY/SKIP counts as ok, FAILED as failed, FAULTED/BUSY/TIMEOUT as neither; total equals ok+failed."""
    tup = tuple(outcomes)
    report = fold(Claim.STATIC, "check", tup)
    ok_n = sum(1 for o in tup if o.status in {RailStatus.OK, RailStatus.EMPTY, RailStatus.SKIP})
    fail_n = sum(1 for o in tup if o.status is RailStatus.FAILED)
    target(float(len(tup)), label="fold_outcome_count")
    target(float(ok_n + fail_n), label="fold_counted_total")
    assert report.counts.ok == ok_n
    assert report.counts.failed == fail_n
    assert report.counts.total == ok_n + fail_n


@given(st.lists(completed_st, min_size=1, max_size=10))
def test_fold_defect_row_per_failed(outcomes: list[Completed]) -> None:
    """Each FAILED Completed yields exactly one Match row with severity 'failed'; counts stay consistent."""
    report = fold(Claim.CODE, "check", tuple(outcomes))
    failed_count = sum(1 for o in outcomes if o.status is RailStatus.FAILED)
    assert len(report.results) == failed_count
    assert all(m.severity == "failed" for m in report.results)
    assert_counts_consistent(report)


def test_fold_empty_outcomes_is_empty_report() -> None:
    """Fold over an empty tuple yields all-zero counts and EMPTY status."""
    report = fold(Claim.TEST, "run", ())
    assert report.counts == Counts()
    assert report.status is RailStatus.EMPTY


def test_fold_failed_stderr_reaches_results_text() -> None:
    """Fold carries a FAILED Completed.stderr value into results[0].text."""
    marker = b"unique-sentinel-stderr-content"
    report = fold(Claim.STATIC, "check", (receipt(("tool",), 1, stderr=marker),))
    assert report.results
    assert marker.decode() in report.results[0].text


# --- [ENVELOPE]


@given(st.lists(completed_st, min_size=0, max_size=5))
def test_envelope_projects_report_status(outcomes: list[Completed]) -> None:
    """envelope(report, ...) projects status and exit_code from the folded report."""
    report = fold(Claim.STATIC, "check", tuple(outcomes))
    env = envelope(report, claim=Claim.STATIC, verb="check")
    assert env.report is report
    assert env.error is None
    assert env.status == report.status
    assert env.exit_code == report.status.exit_code


@given(fault_st)
def test_envelope_projects_fault_status(fault: Fault) -> None:
    """envelope(fault, ...) carries the Fault in error, clears report, and projects exit_code."""
    env = envelope(fault, claim=Claim.CODE, verb="check")
    assert env.error is fault
    assert env.report is None
    assert env.status is fault.status
    assert env.exit_code == fault.status.exit_code


# Real pre-trace history artifact shape (.artifacts/assay/history/2026-06-10T05-34-31.783586-61744/envelope.json),
# captured before Diagnostic carried trace_id/span_id; embedded so the law stays hermetic off this machine.
_PRE_TRACE_ENVELOPE: bytes = (
    b'{"claim":"static","verb":"fix","status":"faulted","exit_code":2,'
    b'"run_id":"2026-06-10T05-34-31.783586-61744","error":{"argv":[],"message":"parse: x"},'
    b'"error_context":{"kind":"diagnostic","failing_step":"parse",'
    b'"recent_events":["dispatch=static","static fix","dispatch=static","static fix"],'
    b'"elapsed_ms":0.0,"hint":"parse: x after 0.0ms",'
    b'"resource":[["mem.rss_bytes",95715328.0],["sys.mem_percent",52.6],["sys.swap_percent",80.9]]}}'
)


def test_envelope_decodes_pre_trace_history_artifact() -> None:
    """Additive-compat: a pre-trace Envelope decodes with ``trace_id``/``span_id`` defaulted under ``schema_version=1``.

    The decoded value survives encode → decode intact through the deterministic wire encoder.
    """
    decoded = msgspec.json.decode(_PRE_TRACE_ENVELOPE, type=Envelope)
    ctx = decoded.error_context
    assert ctx is not None, "pre-trace artifact lost its Diagnostic through the new decoder"
    assert (ctx.trace_id, ctx.span_id) == ("", ""), f"additive fields not defaulted: {ctx.trace_id!r}/{ctx.span_id!r}"
    assert (decoded.schema_version, ctx.failing_step, ctx.hint) == (1, "parse", "parse: x after 0.0ms")
    assert msgspec.json.decode(WIRE_ENCODER.encode(decoded), type=Envelope) == decoded, "pre-trace Envelope does not round-trip"


# --- [RECEIPT]


@pytest.mark.mutation
@pytest.mark.parametrize(
    "rc, explicit, expected",
    [(0, None, RailStatus.EMPTY), (1, None, RailStatus.FAILED), (5, None, RailStatus.BUSY), (0, RailStatus.OK, RailStatus.OK)],
)
def test_receipt_status_derivation(rc: int, explicit: RailStatus | None, expected: RailStatus) -> None:
    """Receipt derives status from the return code unless an explicit override is supplied."""
    done = receipt(("ruff",), rc) if explicit is None else receipt(("ruff",), rc, status=explicit)
    assert done.argv == ("ruff",)
    assert done.returncode == rc
    assert done.status is expected


# --- [FIELD_CAP]


@pytest.mark.mutation
@pytest.mark.parametrize(
    "subject, name, default, expected",
    [
        (Fault, "message", 0, 1024),
        (Diagnostic, "hint", 0, 256),
        (Match, "text", 0, 400),
        (VerifySummary, "first_fault_output", 0, 256),
        (Fault, "argv", 42, 42),
        (Counts, "absent", 7, 7),
    ],
)
def test_field_cap_introspection(subject: type[msgspec.Struct], name: str, default: int, expected: int) -> None:
    """field_cap reads msgspec StrType max_length; returns the configured max_length, or the default when the field is non-string or absent."""
    assert field_cap(subject, name, default=default) == expected


# --- [VALIDATE_DETAIL]


@given(detail_st)
def test_validate_detail_round_trips(detail: AnyDetail) -> None:
    """validate_detail round-trips any AnyDetail variant through the tagged-union codec."""
    assert validate_detail(detail) == detail


def test_validate_detail_none_passthrough() -> None:
    """validate_detail(None) returns None — the optional slot survives the codec."""
    assert validate_detail(None) is None


# --- [WIRE_CODEC]


@spec(Diagnostic, mutation=True, law="determinism")
def test_wire_encode_is_deterministic(detail: Diagnostic) -> None:
    """wire_encode agrees with the deterministic conftest encoder for any wire value."""
    metamorphic(detail, wire_encode, WIRE_ENCODER.encode)


@given(detail_st)
def test_wire_encode_decodes_clean(detail: AnyDetail) -> None:
    """wire_encode output decodes back to a structurally equal value."""
    assert msgspec.json.decode(wire_encode(detail), type=type(detail)) == detail


@given(st.text(alphabet=st.characters(min_codepoint=0xDC80, max_codepoint=0xDCFF), min_size=1, max_size=8))
def test_wire_safe_neutralizes_surrogates(surrogates: str) -> None:
    """wire_safe makes any lone-surrogate string UTF-8 encodable, where the raw string would raise."""
    with pytest.raises(UnicodeEncodeError):
        surrogates.encode("utf-8")
    wire_safe(surrogates).encode("utf-8")  # would raise if a surrogate survived


@example(text="")
@example(text="\U0010ffff")  # max valid codepoint, multi-byte
@example(text="a" * 64)  # max-size boundary
@given(st.text(max_size=64))
def test_wire_safe_idempotent_on_clean(text: str) -> None:
    """wire_safe is idempotent: a UTF-8-clean string passes through unchanged."""
    idempotent(wire_safe(text), wire_safe)


# --- [PARAMS]


@given(st.text(max_size=64), st.lists(st.text(max_size=32), min_size=0, max_size=2000))
def test_baseparams_surplus_within_fault_cap(verb: str, tokens: list[str]) -> None:
    """BaseParams.surplus is total and always clips the Fault message to the 1024-byte wire cap."""
    fault = BaseParams.surplus(verb, tuple(tokens))
    assert isinstance(fault, Fault)
    assert fault.status is RailStatus.FAULTED
    target(float(len(fault.message.encode())), label="surplus_message_bytes")
    assert len(fault.message.encode()) <= field_cap(Fault, "message", default=0)


@given(st.lists(st.text(max_size=32), min_size=0, max_size=8), st.text(max_size=16))
def test_baseparams_default_arity_is_identity(paths: list[str], verb: str) -> None:
    """The default _arity is unbounded, so bound returns the params unchanged (never a Fault)."""
    params = BaseParams(paths=tuple(paths))
    bound = params.bound(verb)
    assert bound is params


# --- [ENUM_PAYLOADS]


@pytest.mark.parametrize("member", list(Runner), ids=[m.name for m in Runner])
def test_runner_prefix_is_str_tuple(member: Runner) -> None:
    """Runner.prefix is a string tuple; in-process/direct runners are empty, launcher runners are non-empty."""
    empty_prefix = {Runner.DIRECT, Runner.INPROC}
    assert isinstance(member.prefix, tuple)
    assert all(isinstance(p, str) for p in member.prefix)
    support_matrix(("prefix_emptiness_matches_runner_kind", lambda: member.prefix == (), member in empty_prefix))


@pytest.mark.parametrize("member", list(Mode), ids=[m.name for m in Mode])
def test_mode_flags_are_bool(member: Mode) -> None:
    """Mode.stream and Mode.writes are genuine booleans, not int aliases."""
    assert isinstance(member.stream, bool)
    assert isinstance(member.writes, bool)


@pytest.mark.parametrize("member", list(Language), ids=[m.name for m in Language])
def test_language_payload(member: Language) -> None:
    """Language.suffixes are dot-prefixed; strategy is a closed routing discriminant."""
    assert member.suffixes
    assert all(s.startswith(".") for s in member.suffixes)
    assert member.strategy in {"closure", "glob"}


@pytest.mark.parametrize("member", list(Input), ids=[m.name for m in Input])
def test_input_payload(member: Input) -> None:
    """Input.flag is a string tuple and Input.scoped is a genuine boolean."""
    assert isinstance(member.flag, tuple)
    assert all(isinstance(f, str) for f in member.flag)
    assert isinstance(member.scoped, bool)


@pytest.mark.parametrize("enum_cls", [Claim, SourceKind, ArtifactKind, MutationLane, SymbolShape], ids=lambda c: c.__name__)
def test_bare_strenum_token_identity(enum_cls: type[Claim | SourceKind]) -> None:
    """Every bare StrEnum member's wire token equals its lowercase string value — injective, str-typed."""
    members = list(enum_cls)
    assert all(isinstance(m.value, str) and m.value == m for m in members)
    assert len({m.value for m in members}) == len(members)


# --- [BIND]
# Bind carries a callable handler and a type, so it cannot round-trip through the wire codec;
# its law is structural well-formedness over the registry.


@given(binds_st)
def test_bind_is_well_formed(bind: Bind) -> None:
    """Each registry Bind carries a callable handler, a Claim claim, a str verb, and a type params."""
    assert callable(bind.handler)
    assert isinstance(bind.claim, Claim)
    assert isinstance(bind.verb, str)
    assert bind.verb
    assert isinstance(bind.params, type)
    assert isinstance(bind.help, str)


# --- [COMPOSITION] ----------------------------------------------------------------------
# Registrations run at import time so MANIFEST is fully populated before test collection begins.

for _row in _WIRE_ROWS:
    register_law(_row[0], "wire_round_trip")

register_laws(
    (AnyDetail, ("variant_round_trip", "tags_injective")),
    (Detail, ("variant_is_subclass",)),
    (Base, ("forbid_unknown_fields",)),
    (fold, ("count_oracle", "defect_row_per_failed", "empty_report", "failed_stderr_in_text")),
    (Report, ("counts_consistent", "envelope_source")),
    (Counts, ("fold_arithmetic",)),
    (Match, ("defect_row_severity",)),
    (envelope, ("report_status_projection", "fault_status_projection")),
    (Envelope, ("fault_branch",)),
    (Fault, ("envelope_error_payload",)),
    (receipt, ("status_derivation",)),
    (Completed, ("receipt_construction",)),
    (field_cap, ("introspection_oracle",)),
    (validate_detail, ("detail_round_trip", "none_passthrough")),
    # @spec covers wire_encode "determinism" at decoration time.
    (wire_encode, ("decode_clean",)),
    (wire_safe, ("surrogate_neutralization", "idempotent")),
    (BaseParams, ("surplus_within_cap", "default_arity_identity")),
    (Runner, ("prefix_str_tuple",)),
    (Mode, ("flags_are_bool",)),
    (Language, ("suffix_and_strategy",)),
    (Input, ("flag_and_scoped",)),
    (Bind, ("well_formed",)),
)

for _cls in _BARE_STRENUM_CLASSES:
    register_law(_cls, "token_identity")
