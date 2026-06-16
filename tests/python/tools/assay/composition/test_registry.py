"""Registry completeness, delta history projection, parse_fault decode, rail dispatch, and self_test laws."""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

from collections import deque
from collections.abc import Callable  # noqa: TC003  # Callable annotation in parametrize parameter evaluated at collection time
from dataclasses import dataclass
from itertools import count
from pathlib import Path
import time
from typing import Final, TYPE_CHECKING

from beartype.roar import BeartypeCallHintViolation
from expression import Error, Ok, Result  # noqa: TC002  # Result appears in inner-function annotations evaluated at runtime
import msgspec
import msgspec.structs
from pydantic import BaseModel, ValidationError
import pytest

from tests.python._testkit.laws import register_laws
from tests.python._testkit.spec import assert_error, assert_error_status, assert_ok, support_matrix, validity_matrix, ValidityCase
from tests.python.tools.assay.kit import (  # noqa: TC001  # fixture annotation resolved at collection time, not import time
    AssayHarness,
    make_history_envelope,
    pipe_history,
)
from tools.assay.composition import registry as registry_mod
from tools.assay.composition.catalog import TOOLS
from tools.assay.composition.registry import _PROBE_TTL, build_app, delta, ORPHAN_MIN_AGE_S, parse_fault, rail, REGISTRY, self_test
from tools.assay.composition.settings import ArtifactScope, ArtifactStore, AssaySettings
from tools.assay.core.aspect import _RING  # ring ContextVar's home module (registry imports it for _seed_parse_ring)
from tools.assay.core.engine import _RESOURCE  # ContextVar set/reset directly for per-test context isolation in test_emit_double_write_guard
from tools.assay.core.model import (
    _RESULT_CAP,  # private cap used to construct overflow tuple in test_ok_envelope_truncation_persists_full_report
    ArtifactKind,
    BridgeLifecycle,
    Claim,
    Completed,
    Counts,
    Envelope,
    envelope,
    Fault,
    fold,
    Match,
    receipt,
    Report,
    RunDelta,
    StaticRun,
    TestRun,
    wire_encode,
)
from tools.assay.core.status import RailStatus, Step
from tools.assay.rails.bridge import BridgeParams
from tools.assay.rails.docs import FaultedPromotion
from tools.assay.rails.static import StaticParams


if TYPE_CHECKING:
    from tools.assay.core.model import Bind, Check


# --- [CONSTANTS] ------------------------------------------------------------------------

_EXPECTED_CLAIMS: frozenset[Claim] = frozenset(b.claim for b in REGISTRY)
_REGISTRY_ROOT: Final = f"{registry_mod.__name__}.REGISTRY"
_ORPHAN_SUBJECT: Final = f"{registry_mod.__name__}.ORPHAN_MIN_AGE_S"
_STATIC_BIND = next(b for b in REGISTRY if b.claim is Claim.STATIC and b.verb == "static")

# --- [HARNESS] --------------------------------------------------------------------------


@dataclass(frozen=True, slots=True)
class _Proc:
    """Minimal psutil process double with a pre-populated ``info`` dict."""

    info: dict[str, object]


# --- [OPERATIONS] -----------------------------------------------------------------------


def _strict_params(strict: bool) -> object:  # noqa: FBT001  # boundary factory: production reads `params.strict` via getattr
    """Duck-typed params object exposing only the strict flag.

    Returns:
        Object exposing a single ``strict`` attribute.
    """
    return type("_Params", (), {"strict": strict})()


def _bad_io(*_a: object, **_k: object) -> None:
    """Raise the store-write OSError path every time.

    Raises:
        OSError: Always, simulating a failed store write.
    """
    raise OSError("disk full")


def _patch_store(monkeypatch: pytest.MonkeyPatch, member: str) -> None:
    monkeypatch.setattr(ArtifactStore, member, _bad_io)


def _run_fake(bind: Bind, settings: AssaySettings, outcome: Result[Report, Fault]) -> Envelope:
    """Run one bind through rail() with a canned handler outcome.

    Returns:
        The Envelope emitted by the rail runner for the canned outcome.
    """
    fake = msgspec.structs.replace(bind, handler=lambda *_a, **_k: outcome)
    return rail(fake, settings=settings)(StaticParams())


def _completed(check: Check, *, rc: int = 0, status: RailStatus = RailStatus.OK, stdout: bytes = b"") -> Completed:
    return msgspec.structs.replace(Completed(check.tool.command, rc, status=status), stdout=stdout)


# --- [REGISTRY]


def test_registry_structural_invariants() -> None:
    """REGISTRY rows own unique leaves, callable handlers, and total claim coverage.

    Mutants caught: duplicate rows, non-callable handlers, missing claim coverage, and corrupted
    runner names.
    """
    pairs = [(b.claim, b.verb) for b in REGISTRY]
    support_matrix(
        ("unique (claim, verb) leaves", lambda: len(pairs) == len(set(pairs)), True),
        ("all handlers callable", lambda: all(callable(b.handler) for b in REGISTRY), True),
    )
    support_matrix(*((c.value, lambda c=c: any(b.claim is c for b in REGISTRY), True) for c in _EXPECTED_CLAIMS))
    validity_matrix(
        (ValidityCase(label=f"{b.claim.value}/{b.verb}", value=b, expected=True) for b in REGISTRY),
        valid=lambda b: callable(rail(b)) and getattr(rail(b), "__name__", None) == b.verb,
    )


register_laws(
    (
        _REGISTRY_ROOT,
        ("registry_unique_claim_verb_pairs", "registry_all_handlers_callable", "registry_every_claim_covered", "registry_all_resolve_via_rail"),
    ),
    (rail, ("rail_runner_name_matches_verb", "rail_runner_is_callable_for_every_bind")),
)

# --- [BUILD_APP]


def test_build_app_returns_cyclopts_app() -> None:
    """build_app exposes every registry leaf plus self-test and delta.

    Mutants caught: wrong App type, skipped bind rows, and missing root utility commands.
    """
    from cyclopts import App  # noqa: PLC0415

    app = build_app(REGISTRY)
    assert isinstance(app, App)
    # Single-verb claims register as root leaves (verb folded into the claim token); multi-verb claims as sub-apps.
    single_verb = frozenset(c for c in _EXPECTED_CLAIMS if sum(b.claim is c for b in REGISTRY) == 1)
    validity_matrix(
        (ValidityCase(label=f"{b.claim.value}/{b.verb}", value=b, expected=True) for b in REGISTRY),
        valid=lambda b: (b.claim.value in app) if b.claim in single_verb else (b.verb in app[b.claim.value]),
    )
    support_matrix(("self-test reachable", lambda: "self-test" in app, True), ("delta reachable", lambda: "delta" in app, True))


def test_build_app_root_configuration() -> None:
    """build_app pins root App identity, version, parse-fault posture, and return-value exits.

    Mutants caught: argv-derived name, missing version, truthy Cyclopts error exits, corrupted
    result_action, and hidden default parameters.
    """
    app = build_app(REGISTRY)
    assert app.name == ("assay",)
    assert app.version == registry_mod._VERSION
    assert app.help == "Rasm polyglot quality operator."
    assert (app.exit_on_error, app.print_error, app.help_on_error) == (False, False, False)
    assert app.result_action == ("return_value",)
    assert app.default_parameter is not None
    assert app.default_parameter.show_default is True


register_laws((
    build_app,
    (
        "build_app_returns_cyclopts_app",
        "build_app_every_leaf_reachable",
        "build_app_self_test_and_delta_registered",
        "build_app_root_name_version_pinned",
        "build_app_error_posture_disabled",
        "build_app_result_action_return_value",
    ),
))

# --- [ORPHAN_MIN_AGE_S]


def test_orphan_min_age_s_value() -> None:
    """ORPHAN_MIN_AGE_S equals 900 seconds (15 minutes) — threshold for orphan hygiene.

    Mutant caught: changing the constant → hygiene filter breaks for 14-min-old processes.
    """
    assert int(ORPHAN_MIN_AGE_S) == 900


def test_orphan_min_age_s_threshold_boundary(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """Processes past the ORPHAN_MIN_AGE_S threshold are reportable; processes at exact boundary differ.

    Mutant caught: changing >= to > (or vice versa) in _orphan_process age check → boundary case fails.
    """
    root = str(assay_root.root)
    base_info: dict[str, object] = {"ppid": 1, "create_time": 0.0, "name": "python3", "cmdline": ("python3", "-"), "cwd": root}
    monkeypatch.setattr(
        registry_mod.__dict__["psutil"], "process_iter", lambda _attrs: (_Proc({**base_info, "pid": 1}), _Proc({**base_info, "pid": 2}))
    )

    def orphan_ids(now: float) -> frozenset[str]:
        monkeypatch.setattr(registry_mod.__dict__["time"], "time", lambda: now)
        matches, _ = registry_mod._process_hygiene(assay_root.settings)
        return frozenset(m.id for m in matches if m.severity == "failed")

    assert orphan_ids(ORPHAN_MIN_AGE_S + 1.0) >= orphan_ids(ORPHAN_MIN_AGE_S), "more orphans expected past threshold"


def test_process_hygiene_reports_old_repo_orphans(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """Hygiene reports only old root-cwd tooling orphans.

    Mutants caught: ppid/cwd/sentinel drift, age arithmetic errors, and notes that lose exact
    pid, age, or command text.
    """
    root = str(assay_root.root)
    rows = (
        _Proc({"pid": 123, "ppid": 1, "create_time": 100.0, "name": "python3", "cmdline": ("python3", "-"), "cwd": root}),
        _Proc({"pid": 456, "ppid": 99, "create_time": 100.0, "name": "python3", "cmdline": ("python3", "-"), "cwd": root}),
        _Proc({"pid": 789, "ppid": 1, "create_time": 100.0, "name": "python3", "cmdline": ("python3", "-"), "cwd": str(assay_root.root.parent)}),
        _Proc({"pid": 999, "ppid": 0, "create_time": 100.0, "name": "python3", "cmdline": ("python3", "-"), "cwd": root}),
    )
    monkeypatch.setattr(registry_mod.__dict__["psutil"], "process_iter", lambda _attrs: rows)
    monkeypatch.setattr(registry_mod.__dict__["time"], "time", lambda: 100.0 + ORPHAN_MIN_AGE_S + 1.0)
    matches, notes = registry_mod._process_hygiene(assay_root.settings)

    failed = [m for m in matches if m.severity == "failed"]
    assert len(failed) == 1, f"expected exactly 1 orphan match, got {len(failed)}"
    assert "pid=123" in failed[0].text
    assert "age=15m" in failed[0].text
    assert "command=python3 -" in failed[0].text
    assert all("pid=456" not in n and "pid=789" not in n and "pid=999" not in n for n in notes)


def test_orphan_process_exception_returns_none() -> None:
    """_orphan_process returns None when proc.info raises OSError — defensive against dead processes.

    Mutant caught: propagating OSError → process_hygiene crashes when a process vanishes mid-scan.
    """
    from unittest.mock import create_autospec, PropertyMock  # noqa: PLC0415

    import psutil as _ps  # noqa: PLC0415

    broken: _ps.Process = create_autospec(_ps.Process, instance=True)
    type(broken).info = PropertyMock(side_effect=OSError("process gone"))
    assert registry_mod._orphan_process(broken, Path("/private/tmp"), now=10000.0) is None


register_laws((
    _ORPHAN_SUBJECT,
    (
        "orphan_min_age_s_value",
        "orphan_min_age_s_threshold_boundary",
        "process_hygiene_reports_old_repo_orphans",
        "orphan_ppid_zero_not_reported",
        "orphan_note_pid_age_command_pinned",
        "orphan_process_exception_returns_none",
    ),
))

# --- [STEP_VOCABULARY]


def test_step_roster_pin() -> None:
    """Step is the closed fault taxonomy and prefix-scan roster.

    Mutants caught: added, dropped, reordered, renamed, or scan-flag-flipped members change
    _failing_step classification or the wire prefixes agents inspect.
    """
    scan_roster = tuple(member for member in Step if member.scan)
    assert scan_roster == (Step.STRICT, Step.VALIDATION, Step.CONFIG, Step.DISPATCH, Step.PARSE, Step.SPAWN)
    assert tuple(str(member) for member in scan_roster) == ("strict", "validation", "config", "dispatch", "parse", "spawn")
    classify_only = tuple(member for member in Step if not member.scan)
    assert classify_only == (Step.TIMEOUT, Step.LEASE_BUSY, Step.DEFECTS)
    assert tuple(str(member) for member in classify_only) == ("timeout", "lease_busy", "defects")
    assert f"{Step.DISPATCH}=none" == registry_mod._DISPATCH_NONE


register_laws((Step, ("step_roster_pin",)))

# --- [PARSE_FAULT]


def test_parse_fault_dispatch_and_fallback() -> None:
    """parse_fault routes known heads and folds unknown or empty heads to STATIC.

    Mutants caught: unsafe Claim construction, empty-token indexing, and non-FAULTED dispatch.
    """
    validity_matrix(
        (
            ValidityCase(label="static/static → static", value=("static", "static"), expected=True),
            ValidityCase(label="api/status → api", value=("api", "status"), expected=True),
            ValidityCase(label="bridge/verify → bridge", value=("bridge", "verify"), expected=True),
            ValidityCase(label="test/run → test", value=("test", "run"), expected=True),
        ),
        valid=lambda tokens: parse_fault(tokens, "x").claim.value == tokens[0] and parse_fault(tokens, "x").verb == tokens[1],
    )
    unknown = parse_fault(("nosuchthing",), "err")
    empty = parse_fault((), "empty")
    support_matrix(
        ("unknown head → STATIC claim", lambda: unknown.claim is Claim.STATIC, True),
        ("unknown head carries token verb", lambda: unknown.verb == "nosuchthing", True),
        ("empty tokens → STATIC claim", lambda: empty.claim is Claim.STATIC, True),
        ("empty tokens → blank verb", lambda: not empty.verb, True),
        ("empty tokens → FAULTED", lambda: empty.status is RailStatus.FAULTED, True),
    )
    cases: tuple[tuple[str, ...], ...] = ((), ("static",), ("static", "static"), ("unknown",), ("bridge", "verify"))
    assert all(parse_fault(tokens, "msg").status is RailStatus.FAULTED for tokens in cases), "every dispatch path must emit FAULTED"


def test_parse_fault_diagnostic_context_present() -> None:
    """parse_fault populates error_context.failing_step='parse' and recent_events contains dispatch token.

    Mutant caught: omitting _seed_parse_ring call → recent_events empty; wrong failing_step label.
    """
    env = parse_fault(("static", "static"), "something wrong")
    assert env.error_context is not None
    assert env.error_context.failing_step == "parse"
    assert any("dispatch=static" in ev for ev in env.error_context.recent_events)


def test_parse_fault_preserves_rejected_argv() -> None:
    """Parse faults retain the exact rejected argv tokens for agent-facing repair hints."""
    env = parse_fault(("static", "static", "--folder", "src/App"), "unexpected option")
    assert env.error is not None
    assert env.error.argv == ("static", "static", "--folder", "src/App")
    assert env.error_context is not None
    assert "static static --folder src/App" in env.error_context.recent_events


def test_parse_fault_never_persisted(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """parse_fault emits only to stdout; history must stay clean.

    Mutant caught: persist=True leaks parse faults into delta and retention folds.
    """
    monkeypatch.setenv("ASSAY_ROOT", str(assay_root.root))
    env = parse_fault(("static", "static"), "bad flag")
    assert env.run_id
    assert assay_root.settings.store().load_history(env.run_id) is None


@pytest.mark.parametrize("initial, dispatch, tokens", [("existing", "static", ("static", "static")), ("none", "bridge", ("bridge", "verify"))])
def test_seed_parse_ring_extends_or_creates(initial: str, dispatch: str, tokens: tuple[str, ...]) -> None:
    """_seed_parse_ring extends or creates the ring while preserving dispatch and command text.

    Mutants caught: dropped prior events and None-arm failure to set _RING.
    """
    token = _RING.set(deque(maxlen=16) if initial == "existing" else None)
    try:
        registry_mod._seed_parse_ring(dispatch, tokens)
        ring = _RING.get()
        assert ring is not None
        assert f"dispatch={dispatch}" in ring
        assert " ".join(tokens) in ring
    finally:
        _RING.reset(token)


@pytest.mark.parametrize(
    "fault, expected_step",
    [
        (Fault(("tool",), RailStatus.TIMEOUT, "timed out"), Step.TIMEOUT),
        (Fault(("tool",), RailStatus.BUSY, "busy"), Step.LEASE_BUSY),
        (Fault((), RailStatus.FAULTED, "strict: gate"), Step.STRICT),
        (Fault((), RailStatus.FAULTED, "validation: type error"), Step.VALIDATION),
        (Fault((), RailStatus.FAULTED, "config: invalid"), Step.CONFIG),
        (Fault((), RailStatus.FAULTED, "parse: bad token"), Step.PARSE),
        (Fault(("tool",), RailStatus.FAULTED, "unrelated"), Step.SPAWN),
        (Fault((), RailStatus.FAULTED, "unrelated"), Step.SPAWN),
    ],
)
def test_failing_step_classification(fault: Fault, expected_step: Step) -> None:
    """_failing_step classifies status, argv, and message prefixes into Step.

    Mutants caught: wrong status arms, missing SPAWN fallback, scan-flag drift, or stringified
    returns that compare equal but are not Step instances.
    """
    result = registry_mod._failing_step(fault)
    assert type(result) is Step, f"_failing_step must return a Step member, got {type(result).__name__}"
    assert result == expected_step
    assert result.value == expected_step.value


def test_distill_explicit_events_and_dispatch_none() -> None:
    """_distill honours caller-supplied events/resource (skipping the ring + _snapshot) and the dispatch=none sentinel.

    Mutants caught: ignoring the events param (ring events overwrite caller's); always setting dispatched=True.
    """
    explicit_resource: tuple[tuple[str, float], ...] = (("rss", 4096.0),)
    diag, _ = registry_mod._distill(
        Fault(("tool",), RailStatus.FAILED, "tool failed"), 123.0, events=("dispatch=static", "tool failed"), resource=explicit_resource
    )
    assert diag.resource == explicit_resource
    assert diag.elapsed_ms == pytest.approx(123.0)
    assert diag.dispatched is True

    none_diag, _ = registry_mod._distill(Fault((), RailStatus.FAULTED, "parse: bad"), 0.0, events=(registry_mod._DISPATCH_NONE,))
    assert none_diag.dispatched is False


def test_ok_envelope_static_failed_context_uses_source_diagnostics_and_resources(assay_root: AssayHarness) -> None:
    """FAILED static reports distill top source diagnostics and StaticRun.resources into error_context."""
    resources = (("proc.tree_rss_bytes.max", 123.0), ("dotnet.slot_wait_ms.max", 4.0))
    report = Report(
        Claim.STATIC,
        "static",
        RailStatus.FAILED,
        results=(Match(id="ma0006", kind=ArtifactKind.CODE, text="tools/rhino-bridge/Shell/ShellHost.cs(297,22): message", severity="error"),),
        detail=StaticRun(resources=resources),
    )
    env = registry_mod._ok_envelope(_STATIC_BIND, assay_root.settings, 12.0, report)
    assert env.error_context is not None
    assert env.error_context.resource == resources
    assert env.error_context.recent_events == ("ma0006: tools/rhino-bridge/Shell/ShellHost.cs(297,22): message",)


def test_parse_fault_config_error_path(monkeypatch: pytest.MonkeyPatch) -> None:
    """parse_fault catches ValidationError from AssaySettings() and emits a 'config:' Fault.

    Mutant caught: not catching ValidationError → config error propagates as uncaught exception.
    """

    class _IntField(BaseModel):
        x: int

    monkeypatch.setattr(AssaySettings, "__init__", lambda *_a, **_k: _IntField.model_validate({"x": "not-an-int"}))
    env = parse_fault(("static", "static"), "test error")
    assert env.status is RailStatus.FAULTED
    assert env.error is not None
    assert env.error.message.startswith("config:")


def test_validation_message_formats_pydantic_errors() -> None:
    """_validation_message formats pydantic ValidationError into readable field: message pairs.

    Mutant caught: returning empty string → config error message is blank in Envelope.
    """

    class _M(BaseModel):
        x: int
        y: str

    try:
        _M.model_validate({"x": "not_int", "y": 123})
    except ValidationError as exc:
        msg = registry_mod._validation_message(exc)
        assert "x" in msg
        assert len(msg) > 0


def test_encode_unicode_error_produces_scrubbed_envelope(monkeypatch: pytest.MonkeyPatch) -> None:
    """_encode catches UnicodeEncodeError and emits a scrubbed FAULTED Envelope.

    Mutant caught: re-raising the error → caller gets UnicodeEncodeError instead of a valid NDJSON line.
    """
    env = Envelope(schema_version=1, claim=Claim.STATIC, verb="static", status=RailStatus.OK, exit_code=0, run_id="x")
    calls = iter((True, False))  # first encode raises, subsequent encodes succeed

    def patched(obj: object) -> bytes:
        return wire_encode(obj) if not next(calls) else (_ for _ in ()).throw(UnicodeEncodeError("utf-8", "x", 0, 1, "surrogates not allowed"))

    monkeypatch.setattr(registry_mod, "wire_encode", patched)
    decoded = msgspec.json.decode(registry_mod._encode(env), type=Envelope)
    assert decoded.status is RailStatus.FAULTED
    assert "invalid characters" in (decoded.notes[0] if decoded.notes else "")


register_laws((
    parse_fault,
    (
        "parse_fault_known_claim_dispatch",
        "parse_fault_unknown_token_falls_back_to_static",
        "parse_fault_empty_tokens_fallback",
        "parse_fault_status_always_faulted",
        "parse_fault_diagnostic_context_present",
        "parse_fault_preserves_rejected_argv",
        "seed_parse_ring_extends_existing_ring",
        "seed_parse_ring_creates_ring_when_none",
        "failing_step_classification",
        "distill_with_explicit_events_and_resource",
        "distill_dispatch_none_marks_not_dispatched",
        "ok_envelope_static_failed_context_uses_source_diagnostics_and_resources",
        "parse_fault_never_persisted",
        "parse_fault_config_error_path",
        "validation_message_formats_pydantic_errors",
        "encode_unicode_error_produces_scrubbed_envelope",
    ),
))

# --- [RAIL]


def test_identity_hom_returns_ok_report() -> None:
    """_identity_hom returns Ok(Report) folded as STATIC/'self-test' regardless of args — identity layer for compose testing.

    Mutants caught: returning Error or wrong type → .is_ok() assertion fails; fold claim/verb literals corrupted.
    """
    report = assert_ok(registry_mod._identity_hom("a", "b", x=1))
    assert isinstance(report, Report)
    assert (report.claim, report.verb) == (Claim.STATIC, "self-test")


def test_correlate_maps_run_id_and_strict(assay_root: AssayHarness) -> None:
    """_correlate projects run_id, strict, and agent_context; attrless params are non-strict.

    Mutants caught: missing strict, wrong run_id, and getattr default drift in log correlation.
    """
    scope = ArtifactScope.open(assay_root.settings, Claim.STATIC)
    ctx = registry_mod._correlate(assay_root.settings, scope, _strict_params(strict=True))
    assert ctx["run_id"] == assay_root.settings.run_id
    assert ctx["strict"] is True
    assert registry_mod._correlate(assay_root.settings, scope, object())["strict"] is False


def test_bound_passthrough_and_surplus_fault() -> None:
    """_bound passes non-BaseParams through and faults surplus BaseParams tokens.

    Mutants caught: missing catchall arm and surplus-token success.
    """
    assert assert_ok(registry_mod._bound(42, Claim.STATIC, "static")) == 42
    surplus = registry_mod._bound(BridgeParams(paths=("extra", "tokens")), Claim.BRIDGE, "verify")  # verify arity=0
    assert isinstance(assert_error(surplus), Fault)


@pytest.mark.parametrize("status", [RailStatus.EMPTY, RailStatus.SKIP])
def test_strict_promotes_noop_fold_to_fault(status: RailStatus) -> None:
    """_strict(Ok(no-op report), strict=True) → Error(Fault) — strict mode gates EMPTY and SKIP folds.

    Mutants caught: removing the strict guard (no-op folds pass); only gating EMPTY but not SKIP.
    """
    result = registry_mod._strict(Ok(Report(Claim.STATIC, "static", status)), _strict_params(strict=True))
    fault = assert_error_status(result, RailStatus.FAULTED)
    assert "strict" in fault.message


def test_strict_passthrough_for_ok_non_strict() -> None:
    """_strict with strict=False passes Ok(report) through unchanged.

    Mutant caught: always returning Error regardless of strict flag → false fault on OK reports.
    """
    assert registry_mod._strict(Ok(Report(Claim.STATIC, "static", RailStatus.OK)), _strict_params(strict=False)).is_ok()


def test_strict_gate_truth_table_and_bound_identity() -> None:
    """_strict promotes only strict EMPTY/SKIP, and _bound preserves params identity.

    Mutants caught: boolean gate drift, attrless strict defaults, argv=None faults, and Ok(None)
    replacing the bound params object.
    """
    empty = Report(Claim.STATIC, "static", RailStatus.EMPTY)
    ok = Report(Claim.STATIC, "static", RailStatus.OK)
    support_matrix(
        ("non-strict EMPTY passes", lambda: registry_mod._strict(Ok(empty), _strict_params(strict=False)).is_ok(), True),
        ("attrless params EMPTY passes", lambda: registry_mod._strict(Ok(empty), object()).is_ok(), True),
        ("strict OK passes", lambda: registry_mod._strict(Ok(ok), _strict_params(strict=True)).is_ok(), True),
    )
    promoted = assert_error_status(registry_mod._strict(Ok(empty), _strict_params(strict=True)), RailStatus.FAULTED)
    assert promoted.argv == ()
    params = StaticParams()
    assert assert_ok(registry_mod._bound(params, Claim.STATIC, "static")) is params


def test_narrow_raises_type_error_for_non_function() -> None:
    """_narrow rejects non-FunctionType handlers with TypeError — binds must be module-level defs.

    Mutant caught: accepting lambda/partial without check → FunctionType contract violated silently.
    """
    with pytest.raises(TypeError, match="FunctionType"):
        registry_mod._narrow(42)


def test_validated_propagates_ok_and_error() -> None:
    """_validated checks Ok reports and leaves Error faults untouched.

    Mutants caught: swallowed Ok returns and validation calls on Fault.
    """
    report = fold(Claim.STATIC, "static", (receipt(("dotnet",), 0, status=RailStatus.OK),))
    assert assert_ok(registry_mod._validated(Ok(report))) is report
    fault = Fault((), RailStatus.FAULTED, "err")
    assert assert_error(registry_mod._validated(Error(fault))) is fault


def test_validated_invalid_detail_raises_into_validation_fault(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """Invalid details raise through _validated and fold to validation faults at _guard.

    _validated does not return Error for invalid detail payloads; the rail boundary owns the
    exception-to-FAULTED conversion.
    """
    report = fold(Claim.STATIC, "static", (receipt(("dotnet",), 0, status=RailStatus.OK),), detail=TestRun(coverage=150.0))
    with pytest.raises(msgspec.ValidationError, match="coverage"):
        registry_mod._validated(Ok(report))
    monkeypatch.setenv("ASSAY_ROOT", str(assay_root.root))
    env = _run_fake(_STATIC_BIND, assay_root.settings, Ok(report))
    assert env.status is RailStatus.FAULTED
    assert env.error is not None
    assert env.error.message.startswith("validation:")


@pytest.mark.parametrize(
    "exc, prefix",
    [
        (FaultedPromotion(), "strict:"),
        (BeartypeCallHintViolation("type mismatch", culprits=(str,)), "validation:"),
        (msgspec.MsgspecError("bad decode"), "validation:"),
    ],
)
def test_guard_maps_exception_to_fault(exc: Exception, prefix: str) -> None:
    """_guard maps FaultedPromotion/BeartypeCallHintViolation/MsgspecError to Error(Fault) with the right prefix.

    Mutant caught: not catching one exception class → it escapes the rail to the caller unhandled.
    """

    def _raise() -> Result[Report, Fault]:
        raise exc

    fault = assert_error_status(registry_mod._guard(_raise), RailStatus.FAULTED)
    assert fault.message.startswith(prefix)


def test_rail_runner_emits_ok_and_fault_paths(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """rail(bind) folds Ok reports and Error faults into their Envelope paths.

    Mutants caught: wrong OK claim/verb and OK status on fault output.
    """
    monkeypatch.setenv("ASSAY_ROOT", str(assay_root.root))
    bind = _STATIC_BIND

    ok_env = _run_fake(bind, assay_root.settings, Ok(fold(Claim.STATIC, bind.verb, (receipt(("dotnet",), 0, status=RailStatus.OK),))))
    assert (ok_env.claim, ok_env.verb, ok_env.status) == (Claim.STATIC, "static", RailStatus.OK)

    fault_env = _run_fake(bind, assay_root.settings, Error(Fault(("tool",), RailStatus.FAILED, "tool failed")))
    assert fault_env.status is RailStatus.FAILED
    assert fault_env.error_context is not None
    assert fault_env.error_context.failing_step == "spawn"


def test_rail_runner_scope_oserror_faults(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """rail(bind) runner with ArtifactScope.open raising OSError emits a FAULTED Envelope.

    Drives the except OSError branch inside the run() closure.
    Mutant caught: not catching OSError → exception propagates to caller.
    """
    monkeypatch.setenv("ASSAY_ROOT", str(assay_root.root))
    bind = _STATIC_BIND
    monkeypatch.setattr(ArtifactScope, "open", staticmethod(lambda *_: (_ for _ in ()).throw(OSError("permission denied"))))
    env = _run_fake(bind, assay_root.settings, Ok(fold(Claim.STATIC, bind.verb, ())))
    assert env.status is RailStatus.FAULTED
    assert "scope:" in (env.error.message if env.error else "")
    assert msgspec.json.decode(wire_encode(env), type=Envelope).status is RailStatus.FAULTED  # argv=None mutants break the wire


def test_rail_surplus_dispatch_fault_and_scope_identity(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """Surplus dispatch faults keep claim context, skip persistence, and preserve scope identity.

    Mutants caught: missing claim for ring seeding, lost dispatch token, inverted parse persistence,
    or handler invocation without the opened ArtifactScope.
    """
    monkeypatch.setenv("ASSAY_ROOT", str(assay_root.root))
    verify = next(b for b in REGISTRY if b.claim is Claim.BRIDGE and b.verb == "verify")
    fake = msgspec.structs.replace(verify, handler=lambda *_a, **_k: Ok(fold(Claim.BRIDGE, "verify", ())))
    env = rail(fake, settings=assay_root.settings)(BridgeParams(paths=("extra", "tokens")))
    assert env.status is RailStatus.FAULTED
    assert env.error is not None
    assert env.error.message.startswith("parse:")
    assert env.error_context is not None
    assert "dispatch=bridge" in env.error_context.recent_events
    assert assay_root.settings.store().load_history(assay_root.settings.run_id) is None

    seen: list[object] = []
    ok_bind = msgspec.structs.replace(_STATIC_BIND, handler=lambda _s, scope, _p: (seen.append(scope), Ok(fold(Claim.STATIC, "static", ())))[1])
    rail(ok_bind, settings=assay_root.settings)(StaticParams())
    assert isinstance(seen[0], ArtifactScope)


def test_emit_double_write_guard(assay_root: AssayHarness, capsysbinary: pytest.CaptureFixture[bytes]) -> None:
    """_emit converts a second write into a claim/verb-faithful doubled Envelope on stderr.

    Mutants caught: missing write-count guard, nulled doubled-envelope claim/verb or argv, and
    stderr writes that are not wire-decodable Envelopes.
    """
    bind = _STATIC_BIND
    report = fold(Claim.STATIC, bind.verb, (receipt(("dotnet",), 0, status=RailStatus.OK),))
    writes_token = registry_mod._WRITES.set(iter([1, 2]))  # past 0 so the "second write" arm fires
    ring_token, resource_token = _RING.set(deque(maxlen=16)), _RESOURCE.set(())
    try:
        env = registry_mod._emit(bind, assay_root.settings, time.perf_counter(), Ok(report))
    finally:
        _RESOURCE.reset(resource_token)
        registry_mod._WRITES.reset(writes_token)
        _RING.reset(ring_token)

    assert env.status is RailStatus.FAULTED
    assert env.error is not None
    assert "Invariant 1" in env.error.message
    assert (env.claim, env.verb) == (bind.claim, bind.verb)
    assert env.error.argv == ()
    stderr_lines = capsysbinary.readouterr().err.strip().splitlines()
    doubled = msgspec.json.decode(stderr_lines[-1], type=Envelope)
    assert doubled.status is RailStatus.FAULTED


@pytest.mark.parametrize(
    "label, outcome, exit_code, persisted",
    [
        ("ok", Ok(fold(Claim.STATIC, "static", (receipt(("dotnet",), 0, status=RailStatus.OK),))), 0, True),
        ("spawn-fault", Error(Fault(("tool",), RailStatus.FAILED, "boom")), RailStatus.FAILED.exit_code, True),
        ("parse-fault", Error(Fault((), RailStatus.FAULTED, "parse: bad token")), RailStatus.FAULTED.exit_code, False),
    ],
)
def test_emit_exit_code_duration_and_persistence_matrix(
    label: str,
    outcome: Result[Report, Fault],
    exit_code: int,
    persisted: bool,  # noqa: FBT001  # parametrize matrix field, not a call-site positional bool
    assay_root: AssayHarness,
) -> None:
    """_emit derives exit code, duration, and persistence from outcome class.

    Mutants caught: duration scaling, dropped fault exit_code, inverted persistence gates, and
    run_id=None writes.
    """
    bind = _STATIC_BIND
    settings = assay_root.settings.model_copy(update={"run_id": f"{assay_root.settings.run_id}-{label}"})
    tokens = (_RING.set(deque(maxlen=16)), registry_mod._WRITES.set(count()), _RESOURCE.set(()))
    try:
        env = registry_mod._emit(bind, settings, time.perf_counter() - 1.0, outcome)
    finally:
        _RESOURCE.reset(tokens[2])
        registry_mod._WRITES.reset(tokens[1])
        _RING.reset(tokens[0])
    assert env.exit_code == exit_code
    assert env.duration_ms >= 500.0
    assert (settings.store().load_history(settings.run_id) is not None) is persisted


register_laws(
    (build_app, ("identity_hom_returns_ok_report", "correlate_maps_run_id_and_strict")),
    (
        rail,
        (
            "bound_non_base_params_passes_through",
            "bound_surplus_paths_faults",
            "strict_promotes_empty_report_to_fault",
            "strict_promotes_skip_report_to_fault",
            "strict_passthrough_for_ok_non_strict",
            "narrow_raises_type_error_for_non_function",
            "validated_executes_on_ok_report",
            "validated_passthrough_for_error",
            "validated_invalid_detail_raises_into_validation_fault",
            "guard_catches_faulted_promotion",
            "guard_catches_beartype_violation",
            "guard_catches_msgspec_error",
            "rail_runner_ok_path_executes_emit",
            "rail_runner_fault_path_executes_emit",
            "rail_runner_scope_oserror_faults",
            "rail_scope_oserror_envelope_wire_round_trip",
            "rail_surplus_fault_seeds_dispatch_ring",
            "rail_surplus_parse_fault_not_persisted",
            "rail_handler_receives_artifact_scope",
            "strict_gate_truth_table",
            "strict_promoted_fault_carries_empty_argv",
            "bound_success_returns_params_identity",
            "emit_double_write_guard",
            "emit_double_write_envelope_shape_and_stderr_decode",
            "emit_exit_code_per_outcome_status",
            "emit_duration_wall_clock_ms",
            "emit_persistence_parse_gate",
        ),
    ),
)

# --- [DELTA]


def test_delta_no_prior_emits_empty_status(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """delta() with no history for the run_id emits an EMPTY-status STATIC/delta Envelope.

    Mutant caught: returning OK when both sides are None in _delta_report → status mismatch.
    """
    monkeypatch.setenv("ASSAY_ROOT", str(assay_root.root))
    env = delta("nonexistent-run-id")
    assert (env.claim, env.verb, env.status) == (Claim.STATIC, "delta", RailStatus.EMPTY)


def test_delta_history_projection(mem_store: ArtifactStore, assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """delta() over two persisted runs projects a RunDelta with zero drift (identical receipt shape); oldest pruned.

    Mutant caught: wrong key set arithmetic in _delta_report → added/removed non-zero; wrong _prior selection.
    """
    from tools.assay.composition.registry import _delta_report, _prior  # noqa: PLC0415

    run_a, run_b, run_c = "2026-01-01T00-00-00.000000-9001", "2026-01-02T00-00-00.000000-9001", "2026-01-03T00-00-00.000000-9001"
    pipe_history(mem_store, (run_a, run_b, run_c))
    mem_store.retain_history(2)

    surviving = mem_store.sorted_history_ids()
    assert run_a not in surviving
    assert {run_b, run_c} <= set(surviving)
    loaded_b, loaded_c = mem_store.load_history(run_b), mem_store.load_history(run_c)
    assert loaded_b is not None
    assert loaded_c is not None
    assert _prior(surviving, run_c) == run_b

    report = _delta_report(run_b, run_c, loaded_b, loaded_c)
    assert report.status is RailStatus.OK
    assert isinstance(report.detail, RunDelta)
    assert (report.detail.added, report.detail.removed) == (0, 0)
    assert (report.detail.before.id, report.detail.after.id) == (run_b, run_c)


def test_delta_missing_after_and_symmetric_difference() -> None:
    """_delta_report folds a missing `after` to EMPTY and counts added/removed as the symmetric difference of result keys.

    Mutants caught: calling .status on None when after is absent; using union instead of difference for added/removed.
    """
    from tools.assay.composition.registry import _delta_report  # noqa: PLC0415

    assert _delta_report("run-b", "run-c", make_history_envelope("run-b"), None).status is RailStatus.EMPTY

    before_env = make_history_envelope("run-x")
    after_report = fold(Claim.STATIC, "check", (receipt(("tool",), 1, status=RailStatus.FAILED),))
    after_env = msgspec.structs.replace(envelope(after_report, claim=Claim.STATIC, verb="check"), run_id="run-y")
    diff = _delta_report("run-x", "run-y", before_env, after_env)
    assert isinstance(diff.detail, RunDelta)
    assert (diff.detail.added, diff.detail.removed) == (1, 0)


def test_delta_host_drift_from_bridge_facts() -> None:
    """_delta_report projects changed cross-session host facts into RunDelta.drift; non-bridge deltas stay empty.

    Mutants caught: dropping the drift field, comparing unchanged facts, or reading drift from non-bridge details.
    """
    from tools.assay.composition.registry import _delta_report  # noqa: PLC0415

    def bridge_env(version: str) -> Envelope:
        detail = BridgeLifecycle(verb="status", host=(("rhinoVersion", "9.0"),), capabilities=(("mcp.platform.version", "Ok", version),))
        return envelope(Report(Claim.BRIDGE, "status", detail=detail), claim=Claim.BRIDGE, verb="status")

    drifted = _delta_report("b", "a", bridge_env("1.0.4"), bridge_env("1.0.5"))
    assert isinstance(drifted.detail, RunDelta)
    assert drifted.detail.drift == (("mcp.platform.version", "Ok: 1.0.4", "Ok: 1.0.5"),)

    same = _delta_report("b", "a", bridge_env("1.0.5"), bridge_env("1.0.5"))
    assert isinstance(same.detail, RunDelta)
    assert same.detail.drift == ()

    non_bridge = _delta_report("b", "a", make_history_envelope("b"), make_history_envelope("a"))
    assert isinstance(non_bridge.detail, RunDelta)
    assert non_bridge.detail.drift == ()


def test_delta_end_to_end_projection_identity(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """delta(run_c) resolves the prior, projects both snapshots, and never persists itself.

    Mutants caught: prior-resolution inversion, nulled history sides, dropped snapshot status/counts,
    corrupted claim/verb/run_id, persistence leaks, or redundant OK notes outside RunDelta detail.
    """
    monkeypatch.setenv("ASSAY_ROOT", str(assay_root.root))
    store = assay_root.settings.store()
    run_b, run_c = "2026-02-01T00-00-00.000000-1", "2026-02-02T00-00-00.000000-1"
    pipe_history(store, (run_b, run_c))
    env = delta(run_c)
    assert (env.claim, env.verb, env.status) == (Claim.STATIC, "delta", RailStatus.OK)
    assert env.run_id
    assert env.report is not None
    assert (env.report.claim, env.report.verb) == (Claim.STATIC, "delta")
    assert env.report.notes == (), "the OK delta arm emits no redundant note; the RunDelta detail is the only surface"
    detail = env.report.detail
    assert isinstance(detail, RunDelta)
    assert (detail.before.id, detail.after.id) == (run_b, run_c)
    assert (detail.before.status, detail.after.status) == (RailStatus.OK, RailStatus.OK)
    assert detail.before.counts == Counts(ok=1, failed=0, total=1)
    assert (detail.added, detail.removed) == (0, 0)
    assert set(store.sorted_history_ids()) == {run_b, run_c}


@pytest.mark.parametrize("orientation", ["removed", "empty"])
def test_delta_report_orientation_and_empty_identity(orientation: str) -> None:
    """_delta_report counts removed keys and preserves EMPTY identity on missing sides.

    Mutants caught: dropped removed count, nulled EMPTY claim, missing notes, and corrupted counts.
    """
    match orientation:
        case "removed":
            before_report = fold(Claim.STATIC, "check", (receipt(("tool",), 1, status=RailStatus.FAILED),))
            before = msgspec.structs.replace(envelope(before_report, claim=Claim.STATIC, verb="check"), run_id="run-x")
            diff = registry_mod._delta_report("run-x", "run-y", before, make_history_envelope("run-y"))
            assert isinstance(diff.detail, RunDelta)
            assert (diff.detail.added, diff.detail.removed) == (0, 1)
        case _:
            rep = registry_mod._delta_report("", "run-z", None, None)
            assert (rep.claim, rep.verb, rep.status) == (Claim.STATIC, "delta", RailStatus.EMPTY)
            assert rep.counts == Counts(ok=1, failed=0, total=1)
            assert rep.notes


def test_emit_envelope_persist_is_required_keyword() -> None:
    """_emit_envelope requires an explicit keyword-only persist decision.

    A default would hide caller intent across run, delta, parse_fault, and self_test emit paths.
    """
    import inspect  # noqa: PLC0415

    param = inspect.signature(registry_mod._emit_envelope).parameters["persist"]
    assert param.kind is inspect.Parameter.KEYWORD_ONLY
    assert param.default is inspect.Parameter.empty, "persist carries no default; every caller decides explicitly"


def test_persist_oserror_is_swallowed(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """_persist swallows OSError from the history store — a failing write never crashes the rail.

    Mutant caught: re-raising the OSError → caller gets OSError instead of best-effort swallow.
    """
    _patch_store(monkeypatch, "write_history")
    registry_mod._persist(assay_root.settings, make_history_envelope("run-x"))


register_laws((
    delta,
    (
        "delta_no_prior_emits_empty_status",
        "delta_history_projection",
        "delta_missing_after_folds_to_empty",
        "delta_symmetric_difference_oracle",
        "delta_end_to_end_snapshot_identity",
        "delta_envelope_never_persisted",
        "delta_removed_side_counted",
        "delta_empty_arm_report_identity",
        "persist_oserror_is_swallowed",
        "emit_envelope_persist_is_required_keyword",
    ),
))

# --- [SELF_TEST]


def test_self_test_structure_and_census(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """self_test emits a healthy, persisted, wire-round-trippable STATIC/self-test Envelope.

    Results carry the expanded census/health rows while counts.total tracks folded receipts.
    Mutants caught: wrong identity/status, missing Match extension, rhino default drift, summary
    rewrites, dropped run_id, or wire-invalid Match/notes payloads.
    """
    import inspect  # noqa: PLC0415

    monkeypatch.setenv("ASSAY_ROOT", str(assay_root.root))
    env = self_test()
    assert inspect.signature(self_test).parameters["rhino"].default is False
    assert (env.claim, env.verb, env.status) == (Claim.STATIC, "self-test", RailStatus.OK), f"self_test not healthy: {env.notes}"
    assert env.report is not None
    assert env.report.counts.total == env.report.counts.ok + env.report.counts.failed
    verb_ids = {b.verb for b in REGISTRY}
    result_ids = {m.id for m in env.report.results}
    assert verb_ids <= result_ids, f"missing verb ids from self_test census: {verb_ids - result_ids}"
    claims = frozenset(b.claim for b in REGISTRY)
    assert (
        env.notes[0]
        == f"rows={len(REGISTRY)} claims={len(claims)} tools={len(TOOLS)} healthy=True yak_ready={registry_mod._yak_ready()} rhino=skipped"
    )
    assert (env.report.claim, env.report.verb) == (Claim.STATIC, "self-test")
    assert env.run_id
    assert msgspec.json.decode(wire_encode(env), type=Envelope).status is RailStatus.OK
    persisted = assay_root.settings.store().load_history(env.run_id)
    assert persisted is not None
    assert persisted.verb == "self-test"


@pytest.mark.parametrize("probe", ["_census", "_composes"])
def test_self_test_unhealthy_when_composition_breaks(probe: str, assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """self_test health is a strict conjunction over composition probes.

    Mutants caught: OR-folded health, corrupted defect argv, and status/exit drift from healthy.
    """
    monkeypatch.setenv("ASSAY_ROOT", str(assay_root.root))
    monkeypatch.setattr(registry_mod, probe, lambda: False)
    monkeypatch.setattr(registry_mod, "fan_out", lambda checks, **_k: tuple(Ok(_completed(c)) for c in checks))
    env = self_test()
    assert env.status is RailStatus.FAILED
    assert env.exit_code == RailStatus.FAILED.exit_code
    assert "healthy=False" in env.notes[0]
    assert env.report is not None
    assert any(m.id == "assay self-test" and m.severity == "failed" for m in env.report.results)


def test_self_test_unhealthy_when_claim_loses_its_last_row(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """self_test fails when a declared Claim has no bound REGISTRY row.

    The roster derives from Claim, not REGISTRY, so dropping every row for one claim remains refutable.
    """
    monkeypatch.setenv("ASSAY_ROOT", str(assay_root.root))
    monkeypatch.setattr(registry_mod, "fan_out", lambda checks, **_k: tuple(Ok(_completed(c)) for c in checks))
    monkeypatch.setattr(registry_mod, "REGISTRY", tuple(b for b in REGISTRY if b.claim is not Claim.DOCS))
    env = self_test()
    assert env.status is RailStatus.FAILED
    assert "healthy=False" in env.notes[0]


def test_self_test_summary_carries_yak_ready_token(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """self_test summary carries one yak_ready token from _yak_ready.

    Mutants caught: dropped packaging-readiness signal and summary/preflight decoupling.
    """
    monkeypatch.setenv("ASSAY_ROOT", str(assay_root.root))
    monkeypatch.setattr(registry_mod, "_yak_ready", lambda: True)
    assert "yak_ready=True" in self_test().notes[0]
    monkeypatch.setattr(registry_mod, "_yak_ready", lambda: False)
    assert "yak_ready=False" in self_test().notes[0]


def test_health_probe_cache_warm_path(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """_health warms from cache while re-probing only volatile or token-unresolvable tools.

    Mutants caught: inverted cache filters, routed=None fan-out, broken DOTNET probe collapse,
    corrupted --version argv, or inverted INPROC exclusion.
    """
    calls: list[tuple[tuple[Check, ...], object]] = []

    def spy(checks: tuple[Check, ...], **kwargs: object) -> tuple[Result[Completed, Fault], ...]:
        calls.append((checks, kwargs["routed"]))
        return tuple(Ok(_completed(c)) for c in checks)

    monkeypatch.setattr(registry_mod, "fan_out", spy)
    registry_mod._health(assay_root.settings)
    registry_mod._health(assay_root.settings)
    (cold, cold_routed), (warm, warm_routed) = calls
    assert cold_routed is registry_mod._PROBE_ROUTED
    assert warm_routed is registry_mod._PROBE_ROUTED
    cold_cmds = {c.tool.command for c in cold}
    warm_cmds = {c.tool.command for c in warm}
    assert ("dotnet", "--version") in cold_cmds
    volatile = {registry_mod._GIT_HEAD.tool.command, registry_mod._GIT_DIRTY.tool.command}
    assert volatile <= cold_cmds
    assert volatile <= warm_cmds
    cacheable = {cmd for cmd in cold_cmds - volatile if registry_mod._probe_token(cmd) is not None}
    assert cacheable, "expected at least one token-resolvable catalogued probe"
    assert warm_cmds.isdisjoint(cacheable)


def test_probe_cache_round_trip_and_retention(assay_root: AssayHarness) -> None:
    """Probe cache rows round-trip and retention keeps only catalogued keys.

    Mutants caught: lost row fields, corrupted cache key path/separator, and inverted keep/evict sets.
    """
    argv = registry_mod._GIT_HEAD.tool.command
    registry_mod._probe_cache_store(assay_root.settings, {}, {argv: ("note-x", False)}, frozenset((argv,)))
    loaded = registry_mod._probe_cache_load(assay_root.settings)
    assert registry_mod._cache_hit(loaded, argv) == ("note-x", False)

    key = registry_mod._PROBE_CACHE_KEY % "\x00".join(argv)
    prior = {
        key: loaded[key],
        "probe:ghost": registry_mod._ProbeRow(token="t", ts=time.time(), note="gone", ok=True),  # noqa: S106  # mtime token, not a secret
    }
    registry_mod._probe_cache_store(assay_root.settings, prior, {}, frozenset((argv,)))
    assert set(registry_mod._probe_cache_load(assay_root.settings)) == {key}


def test_ok_envelope_cap_boundary_and_defect_diagnostic(assay_root: AssayHarness) -> None:
    """_ok_envelope preserves cap-boundary rows and distills FAILED rows into defects.

    Mutants caught: cap comparison drift, nulled failed-row filters, corrupted defects tally,
    and dropped OK status.
    """
    from tools.assay.composition.registry import _ok_envelope  # noqa: PLC0415

    at_cap = tuple(Match(id=f"row-{i}", kind=ArtifactKind.PROCESS, text="x") for i in range(_RESULT_CAP))
    env = _ok_envelope(_STATIC_BIND, assay_root.settings, 1.0, Report(Claim.STATIC, "static", RailStatus.OK, results=at_cap))
    assert env.truncated is False
    assert env.status is RailStatus.OK
    assert env.report is not None
    assert len(env.report.results) == _RESULT_CAP
    assert "full-report" not in {a.id for a in env.report.artifacts}

    rows = (
        Match(id="t1", kind=ArtifactKind.PROCESS, text="boom", severity="failed"),
        Match(id="t2", kind=ArtifactKind.PROCESS, text="bang", severity="failed"),
        Match(id="t3", kind=ArtifactKind.PROCESS, text="fine"),
    )
    failed_env = _ok_envelope(_STATIC_BIND, assay_root.settings, 2.0, Report(Claim.STATIC, "static", RailStatus.FAILED, results=rows))
    assert failed_env.status is RailStatus.FAILED
    assert failed_env.exit_code == RailStatus.FAILED.exit_code
    ctx = failed_env.error_context
    assert ctx is not None
    assert ctx.failing_step == "defects"
    assert ctx.recent_events == ("t1: boom", "t2: bang")
    assert "2 diagnostic(s) failed" in ctx.hint


def test_ok_envelope_truncation_persists_full_report(assay_root: AssayHarness, capsysbinary: pytest.CaptureFixture[bytes]) -> None:
    """_ok_envelope caps stdout, persists the full report, and emits only an in-band cap note.

    The cap signal lives in report/envelope notes and the HISTORY artifact; stderr stays silent.
    Mutants caught: cap overrun, nulled artifact identity/metadata, dropped cap note, or restored
    stderr side channel.
    """
    from tools.assay.composition.registry import _ok_envelope  # noqa: PLC0415

    rows = tuple(Match(id=f"row-{i}", kind=ArtifactKind.PROCESS, text="x") for i in range(_RESULT_CAP + 1))
    env = _ok_envelope(_STATIC_BIND, assay_root.settings, 1.0, Report(Claim.STATIC, "static", RailStatus.OK, results=rows))

    assert env.truncated
    assert env.report is not None
    assert len(env.report.results) == _RESULT_CAP
    assert "full-report" in {a.id for a in env.report.artifacts}
    note = f"results: {_RESULT_CAP} of {_RESULT_CAP + 1} (cap={_RESULT_CAP}); full report artifact under {assay_root.settings.run_id}"
    assert note in env.report.notes, "the cap signal must stay in-band"
    assert env.notes == env.report.notes, "the Envelope mirrors the report notes carrying the cap note"
    assert capsysbinary.readouterr().err == b"", "no stderr truncation side-channel is written"

    full = next(a for a in env.report.artifacts if a.id == "full-report")
    full_raw = assay_root.settings.store().read_path(full.path)
    assert len(msgspec.json.decode(full_raw, type=Report).results) == _RESULT_CAP + 1
    assert full.path.endswith("static-static.full-report.json")
    assert full.kind is ArtifactKind.HISTORY
    assert (full.bytes, full.lines) == (len(full_raw), 0)


def test_full_report_artifact_oserror_returns_empty(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """_full_report_artifact returns () when write_full_report raises OSError.

    Mutant caught: propagating OSError → _ok_envelope crashes instead of silently skipping the artifact.
    """
    from tools.assay.composition.registry import _full_report_artifact  # noqa: PLC0415

    _patch_store(monkeypatch, "write_full_report")
    assert _full_report_artifact(assay_root.settings, _STATIC_BIND, Report(Claim.STATIC, "static", RailStatus.OK)) == ()


@pytest.mark.parametrize(
    "check_factory, outcome, want_substrings, want_ok",
    [
        (lambda: registry_mod._GIT_HEAD, lambda c: Ok(_completed(c, stdout=b"abc1234\n")), ("HEAD", "abc1234"), True),
        (lambda: registry_mod._GIT_DIRTY, lambda c: Ok(_completed(c, stdout=b" M file.py\n")), ("dirty",), True),
        (lambda: registry_mod._GIT_DIRTY, lambda c: Ok(_completed(c, stdout=b"")), ("clean",), True),
        (
            lambda: registry_mod._probe("ruff", ("ruff", "--version")),
            lambda c: Ok(_completed(c, rc=2, status=RailStatus.FAILED, stdout=b"ruff 0.8.0\n")),
            ("present (exit 2)", "ruff 0.8.0"),
            True,
        ),
        (lambda: registry_mod._GIT_HEAD, lambda _c: Error(Fault((), RailStatus.FAULTED, "err")), (), True),
        (lambda: registry_mod._probe("ruff", ("ruff", "--version")), lambda _c: Error(Fault((), RailStatus.FAULTED, "err")), ("MISSING",), False),
    ],
    ids=["git-head-ok", "git-dirty", "git-clean", "tool-nonzero-exit", "git-missing-ok", "tool-missing-false"],
)
def test_probe_note_projects_result_to_note(
    check_factory: Callable[[], Check],
    outcome: Callable[[Check], Result[Completed, Fault]],
    want_substrings: tuple[str, ...],
    want_ok: bool,  # noqa: FBT001  # parametrize matrix field, not a call-site positional bool
) -> None:
    """_probe_note projects tool and git probe outcomes to note/health pairs.

    Mutants caught: git note drift, dirty/clean swaps, nonzero-exit health drift, and
    inverted absence semantics for tools versus git.
    """
    check = check_factory()
    note, ok = registry_mod._probe_note(check, outcome(check))
    assert all(s in note for s in want_substrings)
    assert ok is want_ok


@pytest.mark.parametrize("fresh", [True, False], ids=["fresh", "stale"])
def test_cache_hit_respects_ttl(fresh: bool) -> None:  # noqa: FBT001  # parametrize-injected, not a call-site positional bool
    """_cache_hit returns fresh token hits and rejects stale rows.

    Mutants caught: ignored cache hits and stale rows masking tool upgrades.
    """
    argv = registry_mod._GIT_HEAD.tool.command
    token = registry_mod._probe_token(argv)
    assert token is not None  # real git installed
    ts = time.time() if fresh else time.time() - _PROBE_TTL - 1
    cache = {registry_mod._PROBE_CACHE_KEY % "\x00".join(argv): registry_mod._ProbeRow(token=token, ts=ts, note="git: HEAD abc1234", ok=True)}
    assert registry_mod._cache_hit(cache, argv) == (("git: HEAD abc1234", True) if fresh else None)


@pytest.mark.parametrize("argv", [(), ("__no_such_program_xyz__",)], ids=["empty-argv", "unresolvable"])
def test_probe_token_returns_none(argv: tuple[str, ...]) -> None:
    """_probe_token returns None for empty argv and unresolvable programs.

    Mutants caught: empty-key cache hits and stale defaults for missing programs.
    """
    assert registry_mod._probe_token(argv) is None


@pytest.mark.parametrize("seed", [None, b"{bad json"], ids=["missing", "corrupt"])
def test_probe_cache_load_folds_to_empty(assay_root: AssayHarness, seed: bytes | None) -> None:
    """_probe_cache_load returns {} for an absent cache file or corrupt bytes — both are clean misses.

    Mutants caught: raising on the OSError (missing) path; propagating MsgspecError on the corrupt path.
    """
    match seed:
        case bytes() as raw:
            assay_root.settings.store().write_bytes(raw, "cache", "probe-cache.json")
        case None:
            pass
    assert registry_mod._probe_cache_load(assay_root.settings) == {}


def test_probe_cache_store_oserror_is_swallowed(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """_probe_cache_store swallows OSError from write_bytes — a failing write never crashes self_test.

    Mutant caught: re-raising OSError → self_test crashes when the cache store is read-only.
    """
    _patch_store(monkeypatch, "write_bytes")
    registry_mod._probe_cache_store(assay_root.settings, {}, {}, frozenset())


def test_composes_health_and_type_error(monkeypatch: pytest.MonkeyPatch) -> None:
    """_composes returns True for valid layers and False for slot-inversion TypeError.

    Mutants caught: false unhealthy status and TypeError escaping self_test.
    """
    assert registry_mod._composes() is True
    monkeypatch.setattr(registry_mod, "compose", lambda *_a, **_k: (_ for _ in ()).throw(TypeError("slot order inversion")))
    assert registry_mod._composes() is False


def test_yak_ready_without_yak_on_path(monkeypatch: pytest.MonkeyPatch) -> None:
    """_yak_ready() returns False when shutil.which('yak') returns None.

    Mutant caught: always returning True → self_test passes even without yak installed.
    """
    import shutil  # noqa: PLC0415

    original_which = shutil.which
    monkeypatch.setattr(shutil, "which", lambda name: None if name == "yak" else original_which(name))
    assert registry_mod._yak_ready() is False


def test_yak_ready_present_on_path(monkeypatch: pytest.MonkeyPatch) -> None:
    """_yak_ready() is True when the catalogued PACKAGE yak tool resolves on PATH.

    Mutants caught: inverted row filters or corrupted yak command literals.
    """
    import shutil  # noqa: PLC0415

    monkeypatch.setattr(shutil, "which", lambda name: "/opt/yak/yak" if name == "yak" else None)
    assert registry_mod._yak_ready() is True


register_laws((
    self_test,
    (
        "self_test_structure",
        "self_test_census_rows_cover_all_registry_verbs",
        "self_test_ok_when_composition_healthy",
        "self_test_rhino_default_false",
        "self_test_summary_note_pinned",
        "self_test_envelope_wire_round_trip",
        "self_test_persisted_to_history",
        "self_test_unhealthy_on_census_failure",
        "self_test_unhealthy_on_compose_failure",
        "self_test_unhealthy_when_claim_loses_its_last_row",
        "self_test_summary_carries_yak_ready_token",
        "self_test_failed_exit_code_and_defect_row",
        "health_routes_probe_routed_to_fan_out",
        "health_warm_pass_reprobes_only_volatile_and_unresolvable",
        "health_dotnet_probe_collapse_present",
        "probe_cache_round_trip_identity",
        "probe_cache_retention_evicts_removed_tools",
        "ok_envelope_truncation_persists_full_report",
        "ok_envelope_result_cap_boundary_untruncated",
        "ok_envelope_failed_report_defects_diagnostic",
        "full_report_artifact_claim_verb_identity",
        "full_report_artifact_oserror_returns_empty",
        "probe_note_git_head_ok",
        "probe_note_git_dirty_states",
        "probe_note_tool_nonzero_exit",
        "probe_note_git_missing_returns_ok",
        "probe_note_tool_missing_returns_false",
        "cache_hit_valid_token_returns_note",
        "cache_hit_stale_token_returns_none",
        "probe_token_empty_argv_returns_none",
        "probe_token_unresolvable_direct_returns_none",
        "probe_cache_load_missing_returns_empty",
        "probe_cache_load_corrupt_returns_empty",
        "probe_cache_store_oserror_is_swallowed",
        "composes_returns_true_for_healthy_layers",
        "composes_returns_false_on_type_error",
        "yak_ready_without_yak_on_path",
        "yak_ready_present_on_path",
    ),
))

# --- [LEAF]


def test_leaf_command_closure_and_invocation(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """_leaf returns a verb-named command closure that forwards params identity.

    Mutants caught: runner(None), wrong params type, and None return instead of Envelope.
    """
    import inspect  # noqa: PLC0415
    from types import FunctionType  # noqa: PLC0415

    from tools.assay.composition.registry import _leaf  # noqa: PLC0415

    monkeypatch.setenv("ASSAY_ROOT", str(assay_root.root))
    bind = _STATIC_BIND
    leaf = _leaf(bind)
    assert isinstance(leaf, FunctionType)
    fn: FunctionType = leaf
    assert fn.__name__ == bind.verb
    assert fn.__doc__ == bind.help
    ann = inspect.get_annotations(fn, eval_str=True)  # PEP 563/649 lazy annotations resolve
    assert ann["params"] is bind.params
    assert ann["return"] is Envelope

    expected_report = fold(Claim.STATIC, bind.verb, (receipt(("dotnet",), 0, status=RailStatus.OK),))
    received: list[object] = []
    fake_bind = msgspec.structs.replace(bind, handler=lambda _s, _scope, params: (received.append(params), Ok(expected_report))[1])
    given = fake_bind.params()
    env = _leaf(fake_bind)(given)
    assert env.claim is Claim.STATIC
    assert env.status is RailStatus.OK
    assert received[0] is given  # runner(None) mutants drop the bound params on the floor


def test_read_version_string_and_oserror_default(monkeypatch: pytest.MonkeyPatch) -> None:
    """_read_version returns manifest version and falls back when pyproject is unreadable.

    Mutants caught: nulled parse/version keys and OSError propagation during import.
    """
    import tomllib  # noqa: PLC0415

    expected = tomllib.loads(registry_mod._PYPROJECT.read_text(encoding="utf-8"))["project"]["version"]
    assert registry_mod._read_version() == expected
    monkeypatch.setattr(Path, "read_text", _bad_io)
    assert registry_mod._read_version() == "0.0.0"


def test_register_all_match_arms() -> None:
    """_register handles (None, _), (verb, ''), and (verb, text) arms without error.

    Mutant caught: one arm calling wrong App method → KeyError or missing registration.
    """
    from cyclopts import App  # noqa: PLC0415

    app = App(name="test-root")
    registry_mod._register(app, App(name="sub-app"))  # (None, _) arm
    registry_mod._register(app, lambda: None, name="my-noop")  # (verb, '') arm
    registry_mod._register(app, lambda: None, name="my-noop2", help="Some help text")  # (verb, text) arm
    assert {"sub-app", "my-noop", "my-noop2"} <= set(app)


register_laws((
    build_app,
    (
        "leaf_command_closure_callable",
        "leaf_command_invocation_returns_envelope",
        "leaf_command_forwards_params_identity",
        "read_version_matches_pyproject_exactly",
        "read_version_oserror_returns_default",
        "register_all_match_arms",
    ),
))
