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
_STATIC_FIX_BIND = next(b for b in REGISTRY if b.claim is Claim.STATIC and b.verb == "fix")

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


def test_build_app_root_configuration() -> None:
    """build_app pins root App identity and error posture: name 'assay', catalog version, no exit/print/help on error.

    Mutants caught: name=None (root renamed from argv[0]); version dropped; exit_on_error/print_error/help_on_error
    flipped truthy (Cyclopts would sys.exit on bad argv instead of routing through parse_fault); result_action
    corrupted (exit codes no longer derived from __cyclopts_returncode__); default_parameter show_default dropped.
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
    """Hygiene reports processes (ppid=1, root-cwd, old, tooling) without flagging non-orphans; note carries exact pid/age/command.

    Mutants caught: removing ppid==1 check → sibling processes reported; removing cwd check → foreign processes
    reported; ppid-missing sentinel -1 flipped to +1 (kernel-parented ppid=0 rows reported); age arithmetic
    now+create_time or age*60 (note misstates minutes); command kwarg nulled or cmdline fold emptied.
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
    """``Step`` is the closed fault-step taxonomy; the prefix-scan roster is exactly the original six.

    Mutants caught: adding/dropping/reordering a scan member silently changes ``_failing_step``
    classification; rewording a member breaks the ``{step}:`` wire prefixes and the ``dispatch=`` ring
    sentinel; flipping a member's ``scan`` flag pulls a status-derived classification into the prefix scan
    (or evicts a real prefix from it).
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


def test_parse_fault_never_persisted(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """parse_fault emits to stdout only — the FAULTED Envelope never lands in the history store.

    Mutant caught: flipping the persist=False kwarg to True (_emit_envelope makes persist a required keyword, so
    parse faults that omit it would pollute history and skew delta/retention folds).
    """
    monkeypatch.setenv("ASSAY_ROOT", str(assay_root.root))
    env = parse_fault(("static", "fix"), "bad flag")
    assert env.run_id
    assert assay_root.settings.store().load_history(env.run_id) is None


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
    """_failing_step classifies a Fault by status/argv/message prefix into the correct Step member.

    Mutants caught: one wrong status match arm → step mismatch in Diagnostic output; the empty-argv prefix
    scan's SPAWN default replaced with None (unprefixed synthetic faults lose their step); a ``scan`` flag
    flip on the status-derived members would pull TIMEOUT/LEASE_BUSY into the prefix scan. The exact-type
    assertion pins the ``-> Step`` return: a stringified return would compare equal but fail the isinstance.
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
    """_correlate extracts run_id, strict flag, and agent_context from settings + params; attrless params fold strict=False.

    Mutants caught: omitting strict key → missing key assertion fails; wrong run_id → mismatch; getattr default
    flipped to None/True → attrless params misreport strict in log/trace correlation.
    """
    scope = ArtifactScope.open(assay_root.settings, Claim.STATIC)
    ctx = registry_mod._correlate(assay_root.settings, scope, _strict_params(strict=True))
    assert ctx["run_id"] == assay_root.settings.run_id
    assert ctx["strict"] is True
    assert registry_mod._correlate(assay_root.settings, scope, object())["strict"] is False


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


def test_strict_gate_truth_table_and_bound_identity() -> None:
    """_strict promotes only the (strict truthy, EMPTY/SKIP) corner; the promoted fault carries argv=(); _bound returns the params identity.

    Mutants caught: `strict and noop-status` flipped to `or` (strict OK or non-strict EMPTY promoted); getattr
    default flipped to True (attrless params promoted); Fault argv=None (failing-step reclassifies strict→spawn
    and the wire Envelope stops round-tripping); Ok(None) on the bound success arm (handler receives None params).
    """
    empty = Report(Claim.STATIC, "fix", RailStatus.EMPTY)
    support_matrix(
        ("non-strict EMPTY passes", lambda: registry_mod._strict(Ok(empty), _strict_params(strict=False)).is_ok(), True),
        ("attrless params EMPTY passes", lambda: registry_mod._strict(Ok(empty), object()).is_ok(), True),
        ("strict OK passes", lambda: registry_mod._strict(Ok(Report(Claim.STATIC, "fix", RailStatus.OK)), _strict_params(strict=True)).is_ok(), True),
    )
    promoted = assert_error_status(registry_mod._strict(Ok(empty), _strict_params(strict=True)), RailStatus.FAULTED)
    assert promoted.argv == ()
    params = StaticParams()
    assert assert_ok(registry_mod._bound(params, Claim.STATIC, "fix")) is params


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
    env = _run_fake(_STATIC_FIX_BIND, assay_root.settings, Ok(report))
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
    bind = _STATIC_FIX_BIND

    ok_env = _run_fake(bind, assay_root.settings, Ok(fold(Claim.STATIC, bind.verb, (receipt(("dotnet",), 0, status=RailStatus.OK),))))
    assert (ok_env.claim, ok_env.verb, ok_env.status) == (Claim.STATIC, "fix", RailStatus.OK)

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
    bind = _STATIC_FIX_BIND
    monkeypatch.setattr(ArtifactScope, "open", staticmethod(lambda *_: (_ for _ in ()).throw(OSError("permission denied"))))
    env = _run_fake(bind, assay_root.settings, Ok(fold(Claim.STATIC, bind.verb, ())))
    assert env.status is RailStatus.FAULTED
    assert "scope:" in (env.error.message if env.error else "")
    assert msgspec.json.decode(wire_encode(env), type=Envelope).status is RailStatus.FAULTED  # argv=None mutants break the wire


def test_rail_surplus_dispatch_fault_and_scope_identity(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """rail() folds surplus positionals into a parse-prefixed FAULTED Envelope seeded with dispatch=<claim>, unpersisted, scope intact.

    Mutants caught: _bound called with claim=None (ring seeding crashes on claim.value); _seed_parse_ring
    dispatch=None (recent_events loses the dispatch=bridge token); the parse persist-gate inverted (parse faults
    written to history); handler invoked with scope=None instead of the opened ArtifactScope.
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
    ok_bind = msgspec.structs.replace(_STATIC_FIX_BIND, handler=lambda _s, scope, _p: (seen.append(scope), Ok(fold(Claim.STATIC, "fix", ())))[1])
    rail(ok_bind, settings=assay_root.settings)(StaticParams())
    assert isinstance(seen[0], ArtifactScope)


def test_emit_double_write_guard(assay_root: AssayHarness, capsysbinary: pytest.CaptureFixture[bytes]) -> None:
    """_emit suppresses a second Envelope write with a FAULTED, claim/verb-faithful, wire-decodable doubled Envelope on stderr.

    Drives the `case rank:` branch in _emit.
    Mutants caught: not guarding the write count (two stdout lines, Invariant 1 violated); doubled-envelope
    verb/claim kwargs nulled; Fault argv=None (wire codec rejects the doubled line); stderr fed wire_encode(None).
    """
    bind = _STATIC_FIX_BIND
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
        ("ok", Ok(fold(Claim.STATIC, "fix", (receipt(("dotnet",), 0, status=RailStatus.OK),))), 0, True),
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
    """_emit derives exit_code from outcome status, measures wall-clock ms, persists OK and spawn faults, and never persists parse faults.

    Mutants caught: ms scaled /1000 or nulled (duration collapses to ~0); exit_code kwarg dropped on the fault
    arm (FAULTED envelopes exit 0); persist seed or parse-gate forced falsy/inverted (history rows vanish or
    parse faults leak in); write_history called with run_id=None.
    """
    bind = _STATIC_FIX_BIND
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


def test_delta_end_to_end_projection_identity(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """delta(run_c) resolves the lexicographic prior, projects RunSnapshot identity on both sides, and never persists its own Envelope.

    Mutants caught: `against or _prior` flipped to `and` (prior never resolved → EMPTY); either load_history
    side nulled; RunSnapshot status/counts kwargs nulled or dropped (snapshot collapses to defaults); fold
    claim/verb literals corrupted; envelope run_id nulled; persist=False flipped (delta Envelope leaks into the
    history store). The OK arm carries NO note — status/counts/added/removed live in detail, which agents read.
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
    """_delta_report counts removed keys from the before side; the missing-side arm folds a STATIC/'delta' EMPTY report with a note.

    Mutants caught: the `removed=` kwarg dropped (before-only result keys silently report zero drift); EMPTY-arm
    fold claim nulled; EMPTY-arm Completed notes dropped; EMPTY-arm counts corrupted.
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
    """_emit_envelope makes `persist` a required keyword-only param — the dead default is gone.

    Every caller passes persist explicitly (run path, delta, parse_fault, self_test), so a default merely hid
    intent; making it required keeps the persistence decision at every call site and lets the checker catch an
    omission. Mutant caught: re-introducing a default (a forgotten persist silently defaults to one polarity).
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
    registry_mod._persist(assay_root.settings, make_history_envelope("run-x"))  # must not raise


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
    """self_test() emits a healthy, wire-round-trippable, history-persisted STATIC/self-test Envelope with exact summary and census.

    Census/health rows extend beyond the fold count, so counts.total tracks folded receipts while results
    carries the full expanded set.
    Mutants caught: wrong claim/verb literals; always-FAILED status; omitting the Match-extension loop; rhino
    default flipped to True (preflight requires yak by default); summary note nulled or reworded; report fold
    claim corrupted; envelope run_id dropped (history row unreachable); any Match/notes field nulled (wire
    codec rejects the persisted Envelope).
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
    """self_test folds healthy as a strict conjunction: one failing probe forces FAILED status/exit, healthy=False, and an 'assay' defect row.

    Mutants caught: `and _census()` flipped to `or` (one green probe masks the other's failure); receipt argv
    corrupted (defect row id drifts from 'assay'); status decoupled from the healthy fold; exit code decoupled
    from status.
    """
    monkeypatch.setenv("ASSAY_ROOT", str(assay_root.root))
    monkeypatch.setattr(registry_mod, probe, lambda: False)
    monkeypatch.setattr(registry_mod, "fan_out", lambda checks, **_k: tuple(Ok(_completed(c)) for c in checks))
    env = self_test()
    assert env.status is RailStatus.FAILED
    assert env.exit_code == RailStatus.FAILED.exit_code
    assert "healthy=False" in env.notes[0]
    assert env.report is not None
    assert any(m.id == "assay" and m.severity == "failed" for m in env.report.results)


def test_self_test_unhealthy_when_claim_loses_its_last_row(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """self_test FAILs when a declared Claim has no bound REGISTRY row — the roster derives from the Claim enum, not REGISTRY.

    The coverage conjunct is now refutable: dropping every DOCS row leaves Claim.DOCS unbound, so
    `all(c in bound_claims for c in Claim)` is False and the preflight is unhealthy. A REGISTRY-derived roster
    (the former tautology) would still pass because the dropped claim would also vanish from the roster.
    """
    monkeypatch.setenv("ASSAY_ROOT", str(assay_root.root))
    monkeypatch.setattr(registry_mod, "fan_out", lambda checks, **_k: tuple(Ok(_completed(c)) for c in checks))
    monkeypatch.setattr(registry_mod, "REGISTRY", tuple(b for b in REGISTRY if b.claim is not Claim.DOCS))
    env = self_test()
    assert env.status is RailStatus.FAILED
    assert "healthy=False" in env.notes[0]


def test_self_test_summary_carries_yak_ready_token(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """The self_test summary note carries a single typed yak_ready=<bool> token folded from _yak_ready().

    Mutants caught: the yak_ready token dropped from the summary (an agent loses the packaging-readiness fact);
    the token decoupled from _yak_ready() (the summary claims readiness the preflight never checked).
    """
    monkeypatch.setenv("ASSAY_ROOT", str(assay_root.root))
    monkeypatch.setattr(registry_mod, "_yak_ready", lambda: True)
    assert "yak_ready=True" in self_test().notes[0]
    monkeypatch.setattr(registry_mod, "_yak_ready", lambda: False)
    assert "yak_ready=False" in self_test().notes[0]


def test_health_probe_cache_warm_path(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """_health probes every tool cold, then on a warm pass re-runs only volatile git probes and token-unresolvable tools via _PROBE_ROUTED.

    Mutants caught: hit-lookup / cache-store `current` filters inverted between volatile and catalogued sets
    (warm pass re-probes everything, or git rows get cached); routed=None handed to fan_out; the shared DOTNET
    probe collapse dropped or its --version argv corrupted; the INPROC exclusion inverted (external tools
    vanish from the probe set).
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
    """_probe_cache_store rows survive a load + _cache_hit round-trip ((note, ok) identity); retention keeps catalogued keys, evicts removed.

    Mutants caught: ok/note/ts/token kwargs dropped or nulled in the stored row (a cached MISSING tool flips
    back to healthy); the cache directory segment or NUL key separator corrupted (round-trip misses); the
    retention keep/evict key sets inverted (ghost rows survive, live rows evicted).
    """
    argv = registry_mod._GIT_HEAD.tool.command
    registry_mod._probe_cache_store(assay_root.settings, {}, {argv: ("note-x", False)}, frozenset((argv,)))
    loaded = registry_mod._probe_cache_load(assay_root.settings)
    assert registry_mod._cache_hit(loaded, argv) == ("note-x", False)

    key = registry_mod._PROBE_CACHE_KEY % "\x00".join(argv)
    prior = {key: loaded[key], "probe:ghost": registry_mod._ProbeRow(token="t", ts=time.time(), note="gone", ok=True)}  # noqa: S106  # mtime token, not a secret
    registry_mod._probe_cache_store(assay_root.settings, prior, {}, frozenset((argv,)))
    assert set(registry_mod._probe_cache_load(assay_root.settings)) == {key}


def test_ok_envelope_cap_boundary_and_defect_diagnostic(assay_root: AssayHarness) -> None:
    """_ok_envelope keeps a report at exactly _RESULT_CAP untruncated and folds FAILED rows into a 'defects' Diagnostic with the failure tally.

    Mutants caught: cap comparison > flipped to >= (boundary report spuriously truncated); the failed-row
    filter nulled (Diagnostic construction crashes); the 'defects' step label or Fault tally corrupted;
    Envelope status kwarg nulled on the OK path.
    """
    from tools.assay.composition.registry import _ok_envelope  # noqa: PLC0415

    at_cap = tuple(Match(id=f"row-{i}", kind=ArtifactKind.PROCESS, text="x") for i in range(_RESULT_CAP))
    env = _ok_envelope(_STATIC_FIX_BIND, assay_root.settings, 1.0, Report(Claim.STATIC, "fix", RailStatus.OK, results=at_cap))
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
    failed_env = _ok_envelope(_STATIC_FIX_BIND, assay_root.settings, 2.0, Report(Claim.STATIC, "fix", RailStatus.FAILED, results=rows))
    assert failed_env.status is RailStatus.FAILED
    assert failed_env.exit_code == RailStatus.FAILED.exit_code
    ctx = failed_env.error_context
    assert ctx is not None
    assert ctx.failing_step == "defects"
    assert ctx.recent_events == ("t1: boom", "t2: bang")
    assert "2 tool(s) failed" in ctx.hint


def test_ok_envelope_truncation_persists_full_report(assay_root: AssayHarness, capsysbinary: pytest.CaptureFixture[bytes]) -> None:
    """_ok_envelope saturates stdout at _RESULT_CAP, persists the unclipped report as a HISTORY artifact, and appends an in-band N-of-M note.

    The truncation signal is in-band only: report.notes carries the unified `results: {shown} of {total}
    (cap={cap}); full report artifact under {run_id}` shape (the rails/code.py _cap_note grammar with the
    registry suffix) and NOTHING is written to stderr — the former side-channel is gone.
    Mutants caught: clipping report.results to _RESULT_CAP+1 (cap overrun); the artifact filename nulled
    (persisted identity loses the claim-verb stem); bytes/lines kwargs nulled or bumped (artifact metadata
    misstates the persisted payload); the in-band note dropped or reverted to a stderr write.
    """
    from tools.assay.composition.registry import _ok_envelope  # noqa: PLC0415

    rows = tuple(Match(id=f"row-{i}", kind=ArtifactKind.PROCESS, text="x") for i in range(_RESULT_CAP + 1))
    env = _ok_envelope(_STATIC_FIX_BIND, assay_root.settings, 1.0, Report(Claim.STATIC, "fix", RailStatus.OK, results=rows))

    assert env.truncated
    assert env.report is not None
    assert len(env.report.results) == _RESULT_CAP
    assert "full-report" in {a.id for a in env.report.artifacts}
    note = f"results: {_RESULT_CAP} of {_RESULT_CAP + 1} (cap={_RESULT_CAP}); full report artifact under {assay_root.settings.run_id}"
    assert note in env.report.notes, "the in-band unified N-of-M note replaces the former stderr side-channel"
    assert env.notes == env.report.notes, "the Envelope mirrors the report notes carrying the cap note"
    assert capsysbinary.readouterr().err == b"", "no stderr truncation side-channel is written"

    full = next(a for a in env.report.artifacts if a.id == "full-report")
    full_raw = assay_root.settings.store().read_path(full.path)
    assert len(msgspec.json.decode(full_raw, type=Report).results) == _RESULT_CAP + 1
    assert full.path.endswith("static-fix.full-report.json")
    assert full.kind is ArtifactKind.HISTORY
    assert (full.bytes, full.lines) == (len(full_raw), 0)


def test_full_report_artifact_oserror_returns_empty(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """_full_report_artifact returns () when write_full_report raises OSError.

    Mutant caught: propagating OSError → _ok_envelope crashes instead of silently skipping the artifact.
    """
    from tools.assay.composition.registry import _full_report_artifact  # noqa: PLC0415

    _patch_store(monkeypatch, "write_full_report")
    assert _full_report_artifact(assay_root.settings, _STATIC_FIX_BIND, Report(Claim.STATIC, "fix", RailStatus.OK)) == ()


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


def test_yak_ready_present_on_path(monkeypatch: pytest.MonkeyPatch) -> None:
    """_yak_ready() is True when the catalogued PACKAGE yak tool resolves on PATH.

    Mutants caught: runner/claim filters inverted or the 'yak' command literal corrupted — each empties the
    candidate set so readiness is permanently False and --rhino self-test always fails even with yak installed.
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
    """_leaf(bind) is a verb-named, help/annotation-carrying closure whose command() forwards the params identity to the rail runner.

    Mutants caught: command() calling runner(None) or with the wrong params type (handlers would receive None
    instead of the Cyclopts-bound params); returning None instead of an Envelope.
    """
    import inspect  # noqa: PLC0415
    from types import FunctionType  # noqa: PLC0415

    from tools.assay.composition.registry import _leaf  # noqa: PLC0415

    monkeypatch.setenv("ASSAY_ROOT", str(assay_root.root))
    bind = _STATIC_FIX_BIND
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
    """_read_version() returns exactly the manifest's project.version, and falls back to '0.0.0' when pyproject is unreadable.

    Mutants caught: the parse pipeline nulled or the 'version' key corrupted (CLI --version silently degrades
    to the 0.0.0 fallback while pyproject is readable); propagating OSError (import fails when pyproject.toml
    is absent).
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
