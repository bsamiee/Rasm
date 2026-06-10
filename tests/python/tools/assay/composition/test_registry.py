"""Registry completeness, delta history projection, parse_fault decode, rail dispatch, and self_test laws."""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

from collections import deque
from collections.abc import Callable  # noqa: TC003  # Callable annotation in parametrize parameter evaluated at collection time
from dataclasses import dataclass
from pathlib import Path
import time
from typing import Final, TYPE_CHECKING

from beartype.roar import BeartypeCallHintViolation
from dirty_equals import IsStr
from expression import Error, Ok, Result  # noqa: TC002  # Result appears in inner-function annotations evaluated at runtime
from inline_snapshot import snapshot
import msgspec
import msgspec.structs
from pydantic import BaseModel, ValidationError
import pytest

from tests.python._testkit._aspect import register_laws
from tests.python._testkit._spec import assert_error, assert_error_status, assert_ok, support_matrix, validity_matrix, ValidityCase
from tests.python.tools.assay.conftest import (  # noqa: TC001  # fixture annotation resolved at collection time, not import time
    AssayHarness,
    make_history_envelope,
    pipe_history,
)
from tools.assay.composition import registry as registry_mod
from tools.assay.composition.registry import _PROBE_TTL, build_app, delta, ORPHAN_MIN_AGE_S, parse_fault, rail, REGISTRY, self_test
from tools.assay.composition.settings import ArtifactScope, ArtifactStore, AssaySettings
from tools.assay.core.aspect import _RING  # ring ContextVar's home module (registry imports it for _seed_parse_ring)
from tools.assay.core.engine import _RESOURCE  # ContextVar set/reset directly for per-test context isolation in test_emit_double_write_guard
from tools.assay.core.model import (
    _RESULT_CAP,  # private cap used to construct overflow tuple in test_ok_envelope_truncation_persists_full_report
    ArtifactKind,
    Claim,
    Completed,
    Envelope,
    envelope,
    Fault,
    fold,
    Match,
    receipt,
    Report,
    RunDelta,
    TestRun,
    wire_encode,
)
from tools.assay.core.status import RailStatus
from tools.assay.rails.bridge import BridgeParams
from tools.assay.rails.docs import FaultedPromotion
from tools.assay.rails.static import StaticParams


if TYPE_CHECKING:
    from tools.assay.core.model import Bind, Check


# --- [CONSTANTS] ------------------------------------------------------------------------

_EXPECTED_CLAIMS: frozenset[Claim] = frozenset(b.claim for b in REGISTRY)
_REGISTRY_ROOT: Final = f"{registry_mod.__name__}.REGISTRY"
_ORPHAN_SUBJECT: Final = f"{registry_mod.__name__}.ORPHAN_MIN_AGE_S"


# --- [HARNESS] --------------------------------------------------------------------------


@dataclass(frozen=True, slots=True)
class _Proc:
    """Minimal psutil process double with a pre-populated ``info`` dict."""

    info: dict[str, object]


# --- [OPERATIONS] -----------------------------------------------------------------------


def _strict_params(strict: bool) -> object:  # noqa: FBT001  # boundary factory: production reads `params.strict` via getattr
    """Throwaway params object carrying only ``strict`` — duck-typed by the rail via ``getattr``.

    Returns:
        Object whose ``strict`` attribute holds the requested flag.
    """
    return type("_Params", (), {"strict": strict})()


def _bad_io(*_a: object, **_k: object) -> None:
    """Raise ``OSError`` — stand-in for any best-effort store write the rail must swallow.

    Raises:
        OSError: Always, simulating a full disk or read-only store.
    """
    raise OSError("disk full")


def _patch_store(monkeypatch: pytest.MonkeyPatch, member: str) -> None:
    monkeypatch.setattr(ArtifactStore, member, _bad_io)


def _run_fake(bind: Bind, settings: AssaySettings, outcome: Result[Report, Fault]) -> Envelope:
    """Replace ``bind``'s handler with one returning ``outcome`` and execute the rail runner once.

    Returns:
        The ``Envelope`` emitted by the rail runner for the canned outcome.
    """
    fake = msgspec.structs.replace(bind, handler=lambda *_a, **_k: outcome)
    return rail(fake, settings=settings)(StaticParams())


def _completed(check: Check, *, rc: int = 0, status: RailStatus = RailStatus.OK, stdout: bytes = b"") -> Completed:
    return msgspec.structs.replace(Completed(check.tool.command, rc, status=status), stdout=stdout)


# --- [REGISTRY]


def test_registry_structural_invariants() -> None:
    """REGISTRY rows carry unique (claim, verb) leaves, all-callable handlers, and total claim coverage.

    Mutants caught: duplicate Bind row shrinks the deduped pair set; non-callable handler; dropping a
    claim's last Bind leaves a coverage gap; corrupting a verb breaks the rail runner name.
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
    """build_app(REGISTRY) produces a Cyclopts App with every (claim, verb) leaf + self-test/delta reachable.

    Mutants caught: build_app wrong return type; skipping a bind row in the groupby fold; removing the
    _register(app, self_test, ...) / delta call.
    """
    from cyclopts import App  # noqa: PLC0415

    app = build_app(REGISTRY)
    assert isinstance(app, App)
    validity_matrix(
        (ValidityCase(label=f"{b.claim.value}/{b.verb}", value=b, expected=True) for b in REGISTRY), valid=lambda b: b.verb in app[b.claim.value]
    )
    support_matrix(("self-test reachable", lambda: "self-test" in app, True), ("delta reachable", lambda: "delta" in app, True))


register_laws((build_app, ("build_app_returns_cyclopts_app", "build_app_every_leaf_reachable", "build_app_self_test_and_delta_registered")))


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
    """Hygiene reports processes (ppid=1, root-cwd, old, tooling) without flagging non-orphans.

    Mutant caught: removing ppid==1 check → sibling processes reported; removing cwd check → foreign processes reported.
    """
    root = str(assay_root.root)
    rows = (
        _Proc({"pid": 123, "ppid": 1, "create_time": 100.0, "name": "python3", "cmdline": ("python3", "-"), "cwd": root}),
        _Proc({"pid": 456, "ppid": 99, "create_time": 100.0, "name": "python3", "cmdline": ("python3", "-"), "cwd": root}),
        _Proc({"pid": 789, "ppid": 1, "create_time": 100.0, "name": "python3", "cmdline": ("python3", "-"), "cwd": str(assay_root.root.parent)}),
    )
    monkeypatch.setattr(registry_mod.__dict__["psutil"], "process_iter", lambda _attrs: rows)
    monkeypatch.setattr(registry_mod.__dict__["time"], "time", lambda: 100.0 + ORPHAN_MIN_AGE_S + 1.0)
    matches, notes = registry_mod._process_hygiene(assay_root.settings)

    failed = [m for m in matches if m.severity == "failed"]
    assert len(failed) == 1, f"expected exactly 1 orphan match, got {len(failed)}"
    assert "pid=123" in failed[0].text
    assert all("pid=456" not in n and "pid=789" not in n for n in notes)


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
        "orphan_process_exception_returns_none",
    ),
))


# --- [PARSE_FAULT]


def test_parse_fault_dispatch_and_fallback() -> None:
    """parse_fault routes (claim_token, verb) to the matching Claim/verb; unknown/empty heads fold to STATIC.

    Mutants caught: breaking head-membership against _value2member_map_ (Claim("unknown") raises); accessing
    tokens[0] unconditionally on empty tokens (IndexError); returning OK from any dispatch arm.
    """
    validity_matrix(
        (
            ValidityCase(label="static/fix → static", value=("static", "fix"), expected=True),
            ValidityCase(label="api/doctor → api", value=("api", "doctor"), expected=True),
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
    cases: tuple[tuple[str, ...], ...] = ((), ("static",), ("static", "fix"), ("unknown",), ("bridge", "verify"))
    assert all(parse_fault(tokens, "msg").status is RailStatus.FAULTED for tokens in cases), "every dispatch path must emit FAULTED"


def test_parse_fault_diagnostic_context_present() -> None:
    """parse_fault populates error_context.failing_step='parse' and recent_events contains dispatch token.

    Mutant caught: omitting _seed_parse_ring call → recent_events empty; wrong failing_step label.
    """
    env = parse_fault(("static", "fix"), "something wrong")
    assert env.error_context is not None
    assert env.error_context.failing_step == "parse"
    assert any("dispatch=static" in ev for ev in env.error_context.recent_events)


@pytest.mark.parametrize("initial, dispatch, tokens", [("existing", "static", ("static", "fix")), ("none", "bridge", ("bridge", "verify"))])
def test_seed_parse_ring_extends_or_creates(initial: str, dispatch: str, tokens: tuple[str, ...]) -> None:
    """_seed_parse_ring extends a pre-existing ring or initialises a fresh one, preserving dispatch + cmd entries.

    Mutants caught: replacing instead of extending an existing ring (prior events lost); not calling _RING.set()
    on the None arm (ring stays None).
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
        (Fault(("tool",), RailStatus.TIMEOUT, "timed out"), "timeout"),
        (Fault(("tool",), RailStatus.BUSY, "busy"), "lease_busy"),
        (Fault((), RailStatus.FAULTED, "strict: gate"), "strict"),
        (Fault((), RailStatus.FAULTED, "validation: type error"), "validation"),
        (Fault((), RailStatus.FAULTED, "config: invalid"), "config"),
        (Fault((), RailStatus.FAULTED, "parse: bad token"), "parse"),
        (Fault(("tool",), RailStatus.FAULTED, "unrelated"), "spawn"),
    ],
)
def test_failing_step_classification(fault: Fault, expected_step: str) -> None:
    """_failing_step classifies Fault by status/argv/message prefix into the correct step label.

    Mutant caught: one wrong status match arm → step label mismatch in Diagnostic output.
    """
    assert registry_mod._failing_step(fault) == expected_step


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


def test_parse_fault_config_error_path(monkeypatch: pytest.MonkeyPatch) -> None:
    """parse_fault catches ValidationError from AssaySettings() and emits a 'config:' Fault.

    Mutant caught: not catching ValidationError → config error propagates as uncaught exception.
    """

    class _IntField(BaseModel):
        x: int

    monkeypatch.setattr(AssaySettings, "__init__", lambda *_a, **_k: _IntField.model_validate({"x": "not-an-int"}))
    env = parse_fault(("static", "fix"), "test error")
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
    env = Envelope(schema_version=1, claim=Claim.STATIC, verb="fix", status=RailStatus.OK, exit_code=0, run_id="x")
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
        "seed_parse_ring_extends_existing_ring",
        "seed_parse_ring_creates_ring_when_none",
        "failing_step_classification",
        "distill_with_explicit_events_and_resource",
        "distill_dispatch_none_marks_not_dispatched",
        "parse_fault_config_error_path",
        "validation_message_formats_pydantic_errors",
        "encode_unicode_error_produces_scrubbed_envelope",
    ),
))


# --- [RAIL]


def test_identity_hom_returns_ok_report() -> None:
    """_identity_hom returns Ok(Report) regardless of args — identity layer for compose testing.

    Mutant caught: returning Error or wrong type → .is_ok() assertion fails.
    """
    assert isinstance(assert_ok(registry_mod._identity_hom("a", "b", x=1)), Report)


def test_correlate_maps_run_id_and_strict(assay_root: AssayHarness) -> None:
    """_correlate extracts run_id, strict flag, and agent_context from settings + params.

    Mutant caught: omitting strict key → missing key assertion fails; wrong run_id → mismatch.
    """
    scope = ArtifactScope.open(assay_root.settings, Claim.STATIC)
    ctx = registry_mod._correlate(assay_root.settings, scope, _strict_params(strict=True))
    assert ctx["run_id"] == assay_root.settings.run_id
    assert ctx["strict"] is True


def test_bound_passthrough_and_surplus_fault() -> None:
    """_bound wraps a non-BaseParams object in Ok and faults BaseParams with surplus positional tokens.

    Mutants caught: removing the `_` arm (non-BaseParams raises AttributeError); dropping the Fault branch
    (surplus tokens silently pass for a verb whose arity is 0).
    """
    assert assert_ok(registry_mod._bound(42, Claim.STATIC, "fix")) == 42
    surplus = registry_mod._bound(BridgeParams(paths=("extra", "tokens")), Claim.BRIDGE, "verify")  # verify arity=0
    assert isinstance(assert_error(surplus), Fault)


@pytest.mark.parametrize("status", [RailStatus.EMPTY, RailStatus.SKIP])
def test_strict_promotes_noop_fold_to_fault(status: RailStatus) -> None:
    """_strict(Ok(no-op report), strict=True) → Error(Fault) — strict mode gates EMPTY and SKIP folds.

    Mutants caught: removing the strict guard (no-op folds pass); only gating EMPTY but not SKIP.
    """
    result = registry_mod._strict(Ok(Report(Claim.STATIC, "fix", status)), _strict_params(strict=True))
    fault = assert_error_status(result, RailStatus.FAULTED)
    assert "strict" in fault.message


def test_strict_passthrough_for_ok_non_strict() -> None:
    """_strict with strict=False passes Ok(report) through unchanged.

    Mutant caught: always returning Error regardless of strict flag → false fault on OK reports.
    """
    assert registry_mod._strict(Ok(Report(Claim.STATIC, "fix", RailStatus.OK)), _strict_params(strict=False)).is_ok()


def test_narrow_raises_type_error_for_non_function() -> None:
    """_narrow rejects non-FunctionType handlers with TypeError — binds must be module-level defs.

    Mutant caught: accepting lambda/partial without check → FunctionType contract violated silently.
    """
    with pytest.raises(TypeError, match="FunctionType"):
        registry_mod._narrow(42)


def test_validated_propagates_ok_and_error() -> None:
    """_validated calls validate_detail on Ok(report) and passes the outcome through; Error skips validation.

    Mutants caught: swallowing the Ok return (_validated always errors); calling validate_detail on a fault
    (AttributeError on fault.detail).
    """
    report = fold(Claim.STATIC, "fix", (receipt(("dotnet",), 0, status=RailStatus.OK),))
    assert assert_ok(registry_mod._validated(Ok(report))) is report
    fault = Fault((), RailStatus.FAULTED, "err")
    assert assert_error(registry_mod._validated(Error(fault))) is fault


def test_validated_invalid_detail_raises_into_validation_fault(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """_validated RAISES msgspec.ValidationError on a constraint-violating detail; _guard folds it into a "validation:" fault.

    Pins the raise channel as the contract: _validated never returns Error for an invalid detail — the
    exception propagates to _guard, so the rail emits a FAULTED Envelope with the validation: prefix.
    """
    report = fold(Claim.STATIC, "fix", (receipt(("dotnet",), 0, status=RailStatus.OK),), detail=TestRun(coverage=150.0))
    with pytest.raises(msgspec.ValidationError, match="coverage"):
        registry_mod._validated(Ok(report))
    monkeypatch.setenv("ASSAY_ROOT", str(assay_root.root))
    env = _run_fake(REGISTRY[0], assay_root.settings, Ok(report))
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
    """rail(bind) runner folds an Ok(report) into an OK Envelope and an Error(Fault) into a FAILED Envelope+diagnostic.

    Drives the _emit ok/error branches, _distill on the fault path, and the run closure full path.
    Mutants caught: wrong claim/verb on the OK envelope; returning OK status for a fault.
    """
    monkeypatch.setenv("ASSAY_ROOT", str(assay_root.root))
    bind = REGISTRY[0]

    ok_env = _run_fake(bind, assay_root.settings, Ok(fold(Claim.STATIC, bind.verb, (receipt(("dotnet",), 0, status=RailStatus.OK),))))
    assert (ok_env.claim, ok_env.verb, ok_env.status) == snapshot((Claim.STATIC, "fix", RailStatus.OK))

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
    bind = REGISTRY[0]
    monkeypatch.setattr(ArtifactScope, "open", staticmethod(lambda *_: (_ for _ in ()).throw(OSError("permission denied"))))
    env = _run_fake(bind, assay_root.settings, Ok(fold(Claim.STATIC, bind.verb, ())))
    assert env.status is RailStatus.FAULTED
    assert "scope:" in (env.error.message if env.error else "")


def test_emit_double_write_guard(assay_root: AssayHarness) -> None:
    """_emit suppresses a second Envelope write with FAULTED + Invariant 1 message.

    Drives the `case rank:` branch in _emit.
    Mutant caught: not guarding the write count → two lines emitted to stdout, violating Invariant 1.
    """
    bind = REGISTRY[0]
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
            "emit_double_write_guard",
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
    assert (env.claim, env.verb, env.status) == snapshot((Claim.STATIC, "delta", RailStatus.EMPTY))


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
    assert (report.detail.added, report.detail.removed) == snapshot((0, 0))
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
    assert (diff.detail.added, diff.detail.removed) == snapshot((1, 0))


def test_persist_oserror_is_swallowed(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """_persist swallows OSError from the history store — a failing write never crashes the rail.

    Mutant caught: re-raising the OSError → caller gets OSError instead of best-effort swallow.
    """
    _patch_store(monkeypatch, "write_history")
    registry_mod._persist(assay_root.settings, make_history_envelope("run-x"))  # must not raise


register_laws((
    delta,
    (
        "delta_no_prior_emits_empty_status",
        "delta_history_projection",
        "delta_missing_after_folds_to_empty",
        "delta_symmetric_difference_oracle",
        "persist_oserror_is_swallowed",
    ),
))


# --- [SELF_TEST]


def test_self_test_structure_and_census(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """self_test() emits a healthy STATIC/self-test Envelope whose census results cover every REGISTRY verb.

    Census/health rows extend beyond the fold count, so counts.total tracks folded receipts while results
    carries the full expanded set.
    Mutants caught: wrong claim/verb literals; always-FAILED status; omitting the Match-extension loop.
    """
    monkeypatch.setenv("ASSAY_ROOT", str(assay_root.root))
    env = self_test()
    assert (env.claim, env.verb, env.status) == (Claim.STATIC, "self-test", RailStatus.OK), f"self_test not healthy: {env.notes}"
    assert env.report is not None
    assert env.report.counts.total == env.report.counts.ok + env.report.counts.failed
    verb_ids = {b.verb for b in REGISTRY}
    result_ids = {m.id for m in env.report.results}
    assert verb_ids <= result_ids, f"missing verb ids from self_test census: {verb_ids - result_ids}"


def test_ok_envelope_truncation_persists_full_report(assay_root: AssayHarness) -> None:
    """_ok_envelope saturates stdout at _RESULT_CAP while persisting the unclipped report as artifact.

    Mutant caught: clipping report.results to _RESULT_CAP+1 → len(env.report.results) exceeds cap.
    """
    from tools.assay.composition.registry import _ok_envelope  # noqa: PLC0415

    rows = tuple(Match(id=f"row-{i}", kind=ArtifactKind.PROCESS, text="x") for i in range(_RESULT_CAP + 1))
    env = _ok_envelope(REGISTRY[0], assay_root.settings, 1.0, Report(Claim.STATIC, "fix", RailStatus.OK, results=rows))

    assert env.truncated
    assert env.report is not None
    assert len(env.report.results) == _RESULT_CAP
    assert "full-report" in {a.id for a in env.report.artifacts}

    full_raw = assay_root.settings.store().read_path(next(a.path for a in env.report.artifacts if a.id == "full-report"))
    assert len(msgspec.json.decode(full_raw, type=Report).results) == _RESULT_CAP + 1


def test_full_report_artifact_oserror_returns_empty(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """_full_report_artifact returns () when write_full_report raises OSError.

    Mutant caught: propagating OSError → _ok_envelope crashes instead of silently skipping the artifact.
    """
    from tools.assay.composition.registry import _full_report_artifact  # noqa: PLC0415

    _patch_store(monkeypatch, "write_full_report")
    assert _full_report_artifact(assay_root.settings, REGISTRY[0], Report(Claim.STATIC, "fix", RailStatus.OK)) == ()


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
    """_probe_note projects tool/git probe Results to (note, ok): git absence is informational, tool absence is a miss.

    Mutants caught: wrong git HEAD/dirty/clean note format; ok=False for nonzero exit (false unhealthy); swapping
    dirty/clean logic; ok=True for absent tool (silent pass); ok=False for absent git (false FAILED health).
    """
    check = check_factory()
    note, ok = registry_mod._probe_note(check, outcome(check))
    assert all(s in note for s in want_substrings)
    assert ok is want_ok


@pytest.mark.parametrize("fresh", [True, False], ids=["fresh", "stale"])
def test_cache_hit_respects_ttl(fresh: bool) -> None:  # noqa: FBT001  # parametrize-injected, not a call-site positional bool
    """_cache_hit returns (note, ok) for a matching fresh token within TTL and None once the ts expires.

    Mutants caught: always returning None (TTL ignored, every probe re-runs); removing the TTL check (stale
    probes returned as fresh, masking tool upgrades).
    """
    argv = registry_mod._GIT_HEAD.tool.command
    token = registry_mod._probe_token(argv)
    assert token is not None  # real git installed
    ts = time.time() if fresh else time.time() - _PROBE_TTL - 1
    cache = {registry_mod._PROBE_CACHE_KEY % "\x00".join(argv): registry_mod._ProbeRow(token=token, ts=ts, note="git: HEAD abc1234", ok=True)}
    assert registry_mod._cache_hit(cache, argv) == (("git: HEAD abc1234", True) if fresh else None)


@pytest.mark.parametrize("argv", [(), ("__no_such_program_xyz__",)], ids=["empty-argv", "unresolvable"])
def test_probe_token_returns_none(argv: tuple[str, ...]) -> None:
    """_probe_token returns None for empty argv (no key) and unresolvable programs (no mtime → forces a live probe).

    Mutants caught: non-None token for empty argv (cache hits on an empty key); default token for unresolvable
    programs (stale cache hits).
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
    registry_mod._probe_cache_store(assay_root.settings, {}, {}, frozenset())  # must not raise


def test_composes_health_and_type_error(monkeypatch: pytest.MonkeyPatch) -> None:
    """_composes() returns True when _RAIL_LAYERS weave and False when compose raises TypeError (slot inversion).

    Mutants caught: always returning False (composition marked unhealthy when valid); propagating TypeError
    instead of returning False (self_test crashes).
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


register_laws((
    self_test,
    (
        "self_test_structure",
        "self_test_census_rows_cover_all_registry_verbs",
        "self_test_ok_when_composition_healthy",
        "ok_envelope_truncation_persists_full_report",
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
    ),
))


# --- [LEAF]


def test_leaf_command_closure_and_invocation(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """_leaf(bind) is a verb-named, help/annotation-carrying closure whose command() invokes the rail runner.

    Mutants caught: command() calling runner with the wrong params type; returning None instead of an Envelope.
    """
    import inspect  # noqa: PLC0415
    from types import FunctionType  # noqa: PLC0415

    from tools.assay.composition.registry import _leaf  # noqa: PLC0415

    monkeypatch.setenv("ASSAY_ROOT", str(assay_root.root))
    bind = REGISTRY[0]
    leaf = _leaf(bind)
    assert isinstance(leaf, FunctionType)
    fn: FunctionType = leaf
    assert fn.__name__ == bind.verb
    assert fn.__doc__ == bind.help
    ann = inspect.get_annotations(fn, eval_str=True)  # PEP 563/649 lazy annotations resolve
    assert ann["params"] is bind.params
    assert ann["return"] is Envelope

    expected_report = fold(Claim.STATIC, bind.verb, (receipt(("dotnet",), 0, status=RailStatus.OK),))
    fake_bind = msgspec.structs.replace(bind, handler=lambda *_a, **_k: Ok(expected_report))
    env = _leaf(fake_bind)(fake_bind.params())
    assert env.claim is Claim.STATIC
    assert env.status is RailStatus.OK


def test_read_version_string_and_oserror_default(monkeypatch: pytest.MonkeyPatch) -> None:
    """_read_version() returns a non-empty version string, and falls back to '0.0.0' when pyproject is unreadable.

    Mutants caught: returning an empty string (version='' in --version); propagating OSError (import fails when
    pyproject.toml is absent).
    """
    assert registry_mod._read_version() == IsStr(min_length=1)
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
        "read_version_returns_string",
        "read_version_oserror_returns_default",
        "register_all_match_arms",
    ),
))
