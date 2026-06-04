"""Composition root: one ``Bind`` table, one ``rail`` runner, one ``_emit`` stdout writer.

The runner stack is ``checked ▷ logged ▷ traced`` (no ``@retried`` — a rail is a ``Hom``, not a
``Spawn``), folded by ``Slot`` rank in ``compose`` so a ``Slot`` inversion is a decoration-time
``TypeError``. ``_emit`` alone writes stdout; structlog lines and truncation notes ride stderr.
"""

from collections import deque
from collections.abc import Callable
from contextvars import ContextVar
from functools import reduce
from itertools import count, groupby
from operator import attrgetter
import sys
import time
from types import FunctionType
from typing import Final, TYPE_CHECKING

from cyclopts import App, Parameter
from expression import Error, Ok, Result
import msgspec
import structlog

from tools.assay.composition.catalog import select, TOOLS  # intra-package import; tools.assay is the package root
from tools.assay.composition.settings import ArtifactScope, AssaySettings  # intra-package import; tools.assay is the package root
from tools.assay.core.aspect import _RING, checked, compose, logged, traced  # noqa: PLC2701  # intra-package import; tools.assay is the package root
from tools.assay.core.engine import fan_out  # intra-package import; tools.assay is the package root
from tools.assay.core.model import (  # intra-package import; tools.assay is the package root
    ArtifactKind,
    Bind,
    Check,
    Claim,
    Completed,
    Counts,
    Diagnostic,
    Envelope,
    Fault,
    fold,
    Input,
    Language,
    receipt,
    Report,
    RunDelta,
    Runner,
    RunSnapshot,
    Tool,
    validate_detail,
)
from tools.assay.core.routing import Routed, Scope  # intra-package import; tools.assay is the package root
from tools.assay.core.status import RailStatus  # intra-package import; tools.assay is the package root
from tools.assay.rails import (  # intra-package import; tools.assay is the package root
    api as api_rail,
    bridge as bridge_rail,
    code as code_rail,
    docs as docs_rail,
    package as package_rail,
    static as static_rail,
    test as test_rail,
)
from tools.assay.rails.api import ApiParams  # intra-package import; tools.assay is the package root
from tools.assay.rails.bridge import BridgeParams  # intra-package import; tools.assay is the package root
from tools.assay.rails.code import CodeParams  # intra-package import; tools.assay is the package root
from tools.assay.rails.docs import DocsParams, FaultedPromotion  # intra-package import; tools.assay is the package root
from tools.assay.rails.package import PackageParams  # intra-package import; tools.assay is the package root
from tools.assay.rails.static import StaticParams  # intra-package import; tools.assay is the package root
from tools.assay.rails.test import TestParams  # intra-package import; tools.assay is the package root


if TYPE_CHECKING:
    from collections.abc import Iterator, Mapping

    from tools.assay.composition.settings import ArtifactStore  # intra-package import; tools.assay is the package root
    from tools.assay.core.aspect import Layer  # intra-package import; tools.assay is the package root


# --- [TYPES] ----------------------------------------------------------------------------

# The per-verb 3-arg adapter the registry binds. ``P`` is the verb's Params type — a single TypeVar,
# never a ParamSpec: the third positional is one value, not a call signature, so ``Callable[[S, A, P], R]`` holds.
type Handler[P] = Callable[[AssaySettings, ArtifactScope, P], Result[Report, Fault]]


# --- [CONSTANTS] ------------------------------------------------------------------------

_RESULT_CAP: Final = 1000  # the Report.results bound the fold saturates; a saturated collection signals truncation to _emit
_ARTIFACT_CAP: Final = 100  # the Report.artifacts bound the fold saturates; the full output rides the run's scope dir
# PER-INVOCATION one-Envelope guard (Invariant 1), seeded fresh in rail.run: a process-static
# count() would fault every fire after the first in the long-lived automation loop.
_WRITES: ContextVar[Iterator[int]] = ContextVar("assay_writes")
_LOG: Final = structlog.get_logger("assay.registry")  # best-effort run-history persist rail (history writes never fault a rail)
_PROBE_ROUTED: Final[Routed] = Routed(language=Language.PYTHON, scope=Scope.CHANGED)  # Input.NONE probe seed: place emits one empty tail
_PROBE_TIMEOUT: Final = 8.0  # per `--version`/git probe deadline so a hung tool never strands self-test
_ENVELOPE_DECODER: Final[msgspec.json.Decoder[Envelope]] = msgspec.json.Decoder(Envelope)  # run-history read-back


def _identity_hom(*_a: object, **_k: object) -> Result[Report, Fault]:
    # Identity Hom _composes weaves to probe the decoration-time Slot inversion.
    return Ok(None)  # type: ignore[arg-type]  # ty: ignore[invalid-return-type]  # the probe never inspects the value; Ok(None) stands in for any Report


def _correlate(settings: AssaySettings, _scope: ArtifactScope, params: object) -> Mapping[str, object]:
    # The one correlation map both logged and traced bind; strict read via getattr stays inert on Params lacking the field.
    return {"run_id": settings.run_id, "strict": getattr(params, "strict", False), **settings.agent_context}


# checked ▷ logged ▷ traced sorted by Slot in compose; NO retried (a rail is a Hom, not a Spawn).
_RAIL_LAYERS: Final[tuple[Layer[..., Report], ...]] = (
    checked(),
    logged(event="rail", keys=_correlate),
    traced(span="assay.rail", attrs=_correlate),  # run_id + agent_task_id correlate the rail span with each run_check child span
)

REGISTRY: Final[tuple[Bind, ...]] = (  # binds the per-verb ADAPTERS (static_rail.fix etc.), never thin_rail
    Bind(Claim.STATIC, "fix", static_rail.fix, StaticParams, "Format, style, analyzer autofix."),
    Bind(Claim.STATIC, "report", static_rail.report, StaticParams, "Non-mutating diagnostics."),
    Bind(Claim.STATIC, "build", static_rail.build, StaticParams, "Closure-leased restore + build + analyzers."),
    Bind(Claim.STATIC, "full", static_rail.full, StaticParams, "Workspace.slnx parity; Debug+Release."),
    Bind(Claim.STATIC, "plan", static_rail.plan, StaticParams, "Owners, triggers, closure into notes."),
    Bind(Claim.CODE, "search", code_rail.search, CodeParams, "Search: $-metavar -> ast-grep structural; literal -> ripgrep content."),
    Bind(Claim.CODE, "rewrite", code_rail.rewrite, CodeParams, "Structural rewrite preview; --apply writes under lease."),
    Bind(Claim.CODE, "query", code_rail.query, CodeParams, "AST query via tree-sitter (in-process)."),
    Bind(Claim.TEST, "run", test_rail.run, TestParams, "Unit + coverage + mutation fold."),
    Bind(Claim.TEST, "list", test_rail.list, TestParams, "Bounded discovery JSON."),
    Bind(Claim.TEST, "coverage", test_rail.coverage, TestParams, "Coverlet json + cobertura."),
    Bind(Claim.BRIDGE, "verify", bridge_rail.verify, BridgeParams, "Live RhinoWIP scenario fold."),
    Bind(Claim.BRIDGE, "doctor", bridge_rail.doctor, BridgeParams, "Bridge host health."),
    Bind(Claim.BRIDGE, "launch", bridge_rail.launch, BridgeParams, "Start RhinoWIP under lease."),
    Bind(Claim.BRIDGE, "quit", bridge_rail.quit, BridgeParams, "Clean Cocoa terminate."),
    Bind(Claim.BRIDGE, "check", bridge_rail.check, BridgeParams, "Liveness probe."),
    Bind(Claim.BRIDGE, "clean", bridge_rail.clean, BridgeParams, "Clear crash markers + autosave."),
    Bind(Claim.BRIDGE, "build", bridge_rail.build, BridgeParams, "Compile rasm-bridge plugin."),
    Bind(Claim.PACKAGE, "stage", package_rail.stage, PackageParams, "Yak stage commit under lease."),
    Bind(Claim.PACKAGE, "deploy", package_rail.deploy, PackageParams, "Yak install to live host."),
    Bind(Claim.PACKAGE, "publish", package_rail.publish, PackageParams, "Yak push to server."),
    Bind(Claim.PACKAGE, "list", package_rail.list, PackageParams, "Staged + published manifests."),
    Bind(Claim.PACKAGE, "plan", package_rail.plan, PackageParams, "Stage plan into notes."),
    Bind(Claim.API, "doctor", api_rail.doctor, ApiParams, "Host/NuGet/tool health; --strict -> FAULTED."),
    Bind(Claim.API, "resolve", api_rail.resolve, ApiParams, "Asset path resolution."),
    Bind(Claim.API, "query", api_rail.query, ApiParams, "Polymorphic ilspy surface; fingerprint cache."),
    Bind(Claim.API, "show", api_rail.show, ApiParams, "Artifact preview."),
    Bind(Claim.DOCS, "check", docs_rail.check, DocsParams, "Markdown + Mermaid validation."),
)


# --- [OPERATIONS] -----------------------------------------------------------------------


def _strict(outcome: Result[Report, Fault], params: object) -> Result[Report, Fault]:
    # Promote a no-op fold to Error(Fault(FAULTED)) under --strict. Only EMPTY/SKIP promotes —
    # FAILED/BUSY/TIMEOUT already dominate by severity; getattr keeps it inert on Params lacking strict.
    match outcome:
        case Result(tag="ok", ok=report) if getattr(params, "strict", False) and report.status in {RailStatus.EMPTY, RailStatus.SKIP}:
            return Error(Fault((), RailStatus.FAULTED, "strict: empty/skipped fold"))
        case _:
            return outcome


def _distill(fault: Fault, duration_ms: float) -> Diagnostic:
    # Distill the invocation-scoped _RING + Fault into one Diagnostic for the Error branch only — the
    # ring (seated in rail.run, surviving the engine's anyio.run via the copied context-var ref) is read
    # back here at envelope time. The success branch leaves error_context None so omit_defaults stays terse.
    events = tuple(_RING.get() or ())
    step = _failing_step(fault)
    reason = fault.message.removeprefix(f"{step}: ") or (events[-1] if events else "")  # strip the canonical prefix the step already names
    hint = f"{step}: {reason} after {duration_ms:.1f}ms"
    return Diagnostic(failing_step=step, recent_events=events, elapsed_ms=duration_ms, hint=hint)


def _failing_step(fault: Fault) -> str:
    # Name the faulting stage structurally: status ▷ argv ▷ canonical message prefix. Synthetic promotions
    # carry argv=() plus a strict:/validation: prefix (never a heuristic ring scan); an empty-argv fault
    # with no such prefix is the rare lease-fd OSError, folded to spawn.
    match (fault.status, fault.argv):
        case (RailStatus.TIMEOUT, _):
            return "timeout"
        case (RailStatus.BUSY, _):
            return "lease_busy"
        case (_, ()):
            return next((step for step in ("strict", "validation") if fault.message.startswith(f"{step}:")), "spawn")
        case _:
            return "spawn"


def _emit(bind: Bind, settings: AssaySettings, started: float, outcome: Result[Report, Fault]) -> Envelope:
    # The sole stdout writer: fold the rail Result into one Envelope. Truncation is derived off the
    # saturated results/artifacts bounds (Report carries no truncated field). _WRITES is the one-Envelope
    # guard (Invariant 1): the first write rides stdout; any later rank is a wiring defect → FAULTED to stderr.
    ms = (time.perf_counter() - started) * 1000.0
    match outcome:
        case Result(tag="ok", ok=report):
            truncated = len(report.results) >= _RESULT_CAP or len(report.artifacts) >= _ARTIFACT_CAP
            truncated and sys.stderr.write(f"assay: {bind.claim.value} {bind.verb} output truncated; full results under {settings.run_id}\n")
            envelope = Envelope(
                schema_version=1,
                claim=bind.claim,
                verb=bind.verb,
                status=report.status,
                exit_code=report.status.exit_code,
                run_id=settings.run_id,
                duration_ms=ms,
                report=report,
                truncated=truncated,
                notes=report.notes,
            )
        case Result(error=fault):
            envelope = Envelope(
                schema_version=1,
                claim=bind.claim,
                verb=bind.verb,
                status=fault.status,
                exit_code=fault.status.exit_code,
                run_id=settings.run_id,
                duration_ms=ms,
                error=fault,
                error_context=_distill(fault, ms),
            )
    match next(_WRITES.get()):
        case 0:
            sys.stdout.buffer.write(_ENCODER.encode(envelope) + b"\n")
            _persist(settings, envelope)
            return envelope
        case rank:
            doubled = Envelope(
                schema_version=1,
                claim=bind.claim,
                verb=bind.verb,
                status=RailStatus.FAULTED,
                exit_code=RailStatus.FAULTED.exit_code,
                run_id=settings.run_id,
                error=Fault((), RailStatus.FAULTED, f"second Envelope suppressed (write #{rank}); Invariant 1 violated"),
            )
            sys.stderr.buffer.write(_ENCODER.encode(doubled) + b"\n")
            return doubled


def rail(bind: Bind) -> Callable[[object], Envelope]:
    """Build the per-verb runner — weaves ``checked ▷ logged ▷ traced`` once over the bound ``Handler``.

    ``_strict`` and the docs ``FaultedPromotion`` catch sit between the handler and ``_emit``; the
    runner is named for the verb so structlog/trace correlation and the Cyclopts leaf read a stable
    identity.
    """
    handler: Handler[object] = compose(*_RAIL_LAYERS)(_narrow(bind.handler))

    def run(params: object) -> Envelope:
        settings = AssaySettings()
        started = time.perf_counter()
        scope = ArtifactScope.open(settings, bind.claim)
        # invocation-scoped ring + one-Envelope counter; both reset in finally so the automation loop reuses rail() per fire
        ring_token, writes_token = _RING.set(deque(maxlen=16)), _WRITES.set(count())
        try:
            outcome = _guard(lambda: _validated(_strict(handler(settings, scope, params), params)))
            return _emit(bind, settings, started, outcome)
        finally:
            _WRITES.reset(writes_token)
            _RING.reset(ring_token)

    run.__name__ = bind.verb
    return run


def _narrow(handler: object) -> Handler[object]:
    # Narrow Bind.handler (erased to object) to the 3-arg Handler without cast (banned by TID251). Each
    # rail imports its -> Result[Report, Fault] return annotation unconditionally so checked's beartype
    # forward-ref resolution succeeds under PEP 649 without mutating __globals__; a non-function bind faults.
    match handler:
        case FunctionType() as fn:
            return fn  # the FunctionType narrow satisfies the Handler[object] callable shape; the REGISTRY rows are the proof of arity
        case _:
            raise TypeError(f"Bind.handler must be a module-level def (FunctionType), got {type(handler).__name__}")


def _validated(outcome: Result[Report, Fault]) -> Result[Report, Fault]:
    # Round-trip the success Report.detail through the tagged-union codec so malformity fails loud:
    # validate_detail raises msgspec.MsgspecError inside the _guard thunk. Asserts the wire is decodable
    # only — the outcome passes through untouched, the detail is never re-emitted.
    match outcome:
        case Result(tag="ok", ok=report):
            validate_detail(report.detail)
            return outcome
        case _:
            return outcome


def _guard(thunk: Callable[[], Result[Report, Fault]]) -> Result[Report, Fault]:
    # The one except boundary: catch the docs FaultedPromotion + a malformed Detail, each mapped to
    # Fault{argv=(), FAULTED} under a canonical strict:/validation: prefix so the composition root OWNS the
    # synthetic-fault taxonomy and _failing_step names the stage by prefix, never a heuristic ring scan.
    try:
        return thunk()
    except FaultedPromotion as promoted:
        return Error(Fault((), RailStatus.FAULTED, f"strict: {promoted}"))
    except msgspec.MsgspecError as malformed:
        return Error(Fault((), RailStatus.FAULTED, f"validation: {malformed}"))


# --- [COMPOSITION] ----------------------------------------------------------------------

_ENCODER: Final = msgspec.json.Encoder(order="deterministic")  # content-addressable wire order; the sole stdout codec, cached once


def _persist(settings: AssaySettings, envelope: Envelope) -> None:
    # Side-write the emitted Envelope to run-history AFTER the sole stdout line (Invariant 1 untouched),
    # then prune to artifact_retention. Best-effort: an FS error degrades to a structlog warning, never a
    # fault. pipe_file writes the same deterministic-order wire bytes so a run round-trips through delta.
    try:
        store = settings.store()
        directory = store.ensure(ArtifactKind.HISTORY.value, settings.run_id)
        store.fs.pipe_file(f"{directory}/envelope.json", _ENCODER.encode(envelope))
        _retain(store, settings.artifact_retention)
    except OSError as exc:
        _LOG.warning("history.persist_failed", run_id=settings.run_id, error=str(exc)[:200])


def _retain(store: ArtifactStore, keep: int) -> None:
    # Prune run-history to the newest keep runs. run_id is an ISO-timestamp prefix so a lexical sort is
    # chronological (the current run survives); fsspec rm is idempotent so concurrent invocations never fault.
    runs = sorted(store.glob(f"{ArtifactKind.HISTORY.value}/*"))
    excess = runs[: max(0, len(runs) - keep)]
    tuple(store.fs.rm(path, recursive=True) for path in excess)


def _run_ids(store: ArtifactStore) -> tuple[str, ...]:
    # The persisted run_ids, chronologically sorted (ISO-timestamp basename of each history run dir).
    return tuple(path.rstrip("/").rsplit("/", 1)[-1] for path in sorted(store.glob(f"{ArtifactKind.HISTORY.value}/*")))


def _prior(run_ids: tuple[str, ...], run_id: str) -> str:
    # The largest persisted id < run_id (chronologically just before it); "" when none precedes it.
    earlier = tuple(r for r in run_ids if r < run_id)
    return earlier[-1] if earlier else ""


def _load_run(store: ArtifactStore, run_id: str) -> Envelope | None:
    # Decode one persisted run Envelope; absence / decode-failure / an empty id folds to None.
    match run_id:
        case "":
            return None
        case _:
            try:
                return _ENVELOPE_DECODER.decode(store.fs.cat_file(f"{store.root}/{ArtifactKind.HISTORY.value}/{run_id}/envelope.json"))
            except OSError, msgspec.MsgspecError:
                return None


def _delta_report(before_id: str, after_id: str, before: Envelope | None, after: Envelope | None) -> Report:
    # Fold two persisted Envelopes' status/counts/result drift into a RunDelta Report (missing side → EMPTY).
    # Results keyed by (id, line) so added/removed are symmetric-difference cardinalities; an error-channel
    # Envelope (no report) contributes zero counts.
    def snapshot(
        run_id: str, env: Envelope
    ) -> tuple[RunSnapshot, frozenset[tuple[str, int]]]:  # one projection per endpoint: no per-field None cascade
        report = env.report
        counts = report.counts if report is not None else Counts()
        keys = frozenset((m.id, m.line) for m in (report.results if report is not None else ()))
        return RunSnapshot(id=run_id, status=env.status, counts=counts), keys

    match (before, after):
        case (Envelope() as b, Envelope() as a):
            (before_snap, before_keys), (after_snap, after_keys) = snapshot(before_id, b), snapshot(after_id, a)
            detail = RunDelta(before=before_snap, after=after_snap, added=len(after_keys - before_keys), removed=len(before_keys - after_keys))
            note = f"delta {before_id} -> {after_id}: {b.status.value} -> {a.status.value}, +{detail.added}/-{detail.removed} results"
            return fold(Claim.STATIC, "delta", (Completed(("delta", after_id), 0, status=RailStatus.OK, notes=(note,)),), detail=detail)
        case _:
            missing = after_id if after is None else before_id
            note = f"delta: run not found in history: {missing or '(no prior run)'}"
            return fold(Claim.STATIC, "delta", (Completed(("delta", after_id), 0, status=RailStatus.EMPTY, notes=(note,)),))


def delta(run_id: str, *, against: str = "") -> Envelope:
    """Root verb ``delta <run-id> [--against <id>]``: diff two persisted run Envelopes into a ``RunDelta`` Detail.

    Sibling to ``self-test`` (a ROOT command, NOT a Claim); ``--against`` defaults to the chronologically
    prior persisted run. A run absent from history folds to EMPTY. Read-only — no rail params, no lease.
    """
    settings = AssaySettings()
    store = settings.store()
    before_id = against or _prior(_run_ids(store), run_id)
    report = _delta_report(before_id, run_id, _load_run(store, before_id), _load_run(store, run_id))
    envelope = Envelope(
        schema_version=1,
        claim=Claim.STATIC,
        verb="delta",
        status=report.status,
        exit_code=report.status.exit_code,
        run_id=settings.run_id,
        report=report,
        notes=report.notes,
    )
    sys.stdout.buffer.write(_ENCODER.encode(envelope) + b"\n")  # root command: the sole stdout Envelope writer for `delta`
    return envelope


def self_test(*, rhino: bool = False) -> Envelope:
    """Root preflight: affirm the composition is wired + the catalog is unrotted; ``--rhino`` notes the live host.

    Folds the structural invariants plus a ``REGISTRY``+``TOOLS`` census into one ``Report`` (every row
    binds a callable ``Handler``, every claim resolves a non-empty verb set, the rail seam composes
    without a ``Slot`` inversion, every catalog row is routable via ``select()`` and parses an empty
    ``Completed``), folded to ``FAILED`` on the first broken row. ``_health`` deepens it with a concurrent
    toolchain/git probe whose findings ride the notes — surfaced-but-not-fatal. A root command, not a
    bound rail.
    """
    settings = AssaySettings()
    claims = frozenset(b.claim for b in REGISTRY)
    healthy = all(callable(b.handler) for b in REGISTRY) and all(any(b.claim is c for b in REGISTRY) for c in claims) and _composes() and _census()
    status = RailStatus.OK if healthy else RailStatus.FAILED
    summary = f"rows={len(REGISTRY)} claims={len(claims)} tools={len(TOOLS)} rhino={'probed' if rhino else 'skipped'}"
    notes = (summary, *_health(settings))
    report = fold(Claim.STATIC, "self-test", (receipt(("assay", "self-test"), 0 if healthy else 1, status=status, notes=notes),))
    envelope = Envelope(
        schema_version=1,
        claim=Claim.STATIC,
        verb="self-test",
        status=report.status,
        exit_code=report.status.exit_code,
        report=report,
        notes=report.notes,
    )
    sys.stdout.buffer.write(_ENCODER.encode(envelope) + b"\n")  # root command: write the Envelope through the wire so census evidence is visible
    return envelope


def _health(settings: AssaySettings) -> tuple[str, ...]:
    # Probe each distinct tool's --version + git worktree staleness CONCURRENTLY via the rails' fan_out.
    # Surfaced-but-not-fatal: a MISSING tool rides a note, never the self-test OK/FAILED contract.
    probes = (*_tool_probes(), _GIT_HEAD, _GIT_DIRTY)
    results = fan_out(probes, settings=settings, scope=None, routed=_PROBE_ROUTED)
    return tuple(map(_probe_note, probes, results, strict=True))


def _probe(name: str, argv: tuple[str, ...]) -> Check:
    # One DIRECT/Input.NONE probe Check: the full argv runs verbatim under a deadline.
    tool = Tool(name=name, runner=Runner.DIRECT, command=argv, input=Input.NONE, language=Language.PYTHON, claim=Claim.STATIC, timeout=_PROBE_TIMEOUT)
    return Check(tool=tool)


_GIT_HEAD: Final = _probe("git-head", ("git", "rev-parse", "--short", "HEAD"))  # worktree HEAD short-sha probe (read-only, no lease)
_GIT_DIRTY: Final = _probe("git-dirty", ("git", "status", "--porcelain"))  # worktree clean/dirty probe (read-only, no lease)


def _tool_probes() -> tuple[Check, ...]:
    # One --version probe per DISTINCT program in TOOLS (dedup by launcher+program; DOTNET → one SDK probe; INPROC skipped).
    def keyed(tool: Tool) -> tuple[str, tuple[str, ...]]:  # one match per tool: the dedup key plus the version-probe argv
        match tool.runner:
            case Runner.DOTNET:
                return "dotnet", ("dotnet", "--version")  # every DOTNET tool shares the one SDK probe
            case _:
                return f"{tool.runner.value}:{tool.command[0]}", (*tool.runner.prefix, tool.command[0], "--version")

    deduped = dict(keyed(t) for t in TOOLS if t.runner is not Runner.INPROC)
    return tuple(_probe(argv[-2], argv) for argv in deduped.values())


def _probe_note(check: Check, result: Result[Completed, Fault]) -> str:
    # Project one probe Result to a note: a nested match discriminates the probe family (git vs tool) on
    # tool.name, then the resolved Completed (or None on a spawn fault) — one surface, no per-family helper.
    name = check.tool.name
    match result.ok if result.is_ok() else None:  # outer = the Completed|None spawn result (ty-total in two cases); inner = the probe family
        case None:
            return f"git: {name.removeprefix('git-')} unavailable" if name in {"git-head", "git-dirty"} else f"tool {name}: MISSING"
        case Completed() as d if name == "git-head":
            return f"git: HEAD {d.stdout.decode(errors='replace').strip()[:40] or 'unknown'}"
        case Completed() as d if name == "git-dirty":
            return f"git: {'dirty' if d.stdout.strip() else 'clean'}"
        case Completed(returncode=0) as d:
            lines = d.stdout.decode(errors="replace").strip().splitlines()
            return f"tool {name}: {lines[0][:80] if lines else 'present'}"
        case Completed() as d:
            return f"tool {name}: present (exit {d.returncode})"


def _census() -> bool:
    # Day-one catalog-rot catch: a row not surfaced by select() (routing fracture) or whose parser raises
    # on the empty Completed (decode-shape drift) folds to False. select mirrors the rail fan-out slice.
    return all(t in select(t.claim, t.language) for t in TOOLS) and all(_parses(t.parser) for t in TOOLS)


def _parses(parser: object) -> bool:
    # Probe one catalog Parser against an empty Completed. A None parser is trivially safe; a callable
    # must fold the degenerate Completed((), 0) without raising — a raise signals a decode-shape regression.
    match parser:
        case None:
            return True
        case fn if callable(fn):
            try:
                fn(Completed((), 0))
            except Exception:  # noqa: BLE001  # census probe: ANY parser raise on the empty receipt is a rotted row, folded to False
                return False
            return True
        case _:
            return False


def _composes() -> bool:
    # compose surfaces a Slot inversion as a decoration-time TypeError, so weaving the identity Hom is the
    # cheapest structural proof the checked ▷ logged ▷ traced order is monotonic without spawning a process.
    try:
        compose(*_RAIL_LAYERS)(_identity_hom)
    except TypeError:
        return False
    return True


def _leaf(bind: Bind) -> Callable[..., Envelope]:
    # Wrap rail(bind) as a Cyclopts leaf flattening its Params. The flatten rides BaseParams' inherited
    # @Parameter(name="*"), so the leaf needs only a runtime __annotate__ (PEP 649 lazy form, NOT a direct
    # __annotations__ write) pinning the concrete Params type. Identity is set directly, NOT via
    # functools.wraps — wraps would stamp __wrapped__ at the defaultless run closure, which cyclopts
    # follows during signature inspection and rejects (a flatten param needs a default on the signature).
    runner = rail(bind)

    def command(
        params: object = bind.params(),
    ) -> Envelope:  # cyclopts needs a concrete default on the flatten param; the frozen Params instance is immutable so the shared default is safe
        return runner(params)

    command.__name__ = bind.verb
    command.__qualname__ = f"_leaf.{bind.verb}"
    command.__doc__ = bind.help
    command.__annotate__ = lambda _format: {"params": bind.params, "return": Envelope}  # PEP 649 lazy form, never a direct __annotations__ write
    return command


def build_app(registry: tuple[Bind, ...]) -> App:
    """Fold ``registry`` into the Cyclopts ``App`` tree: ``groupby(claim)`` sub-apps, ``self-test`` on root.

    Pre-sorts on ``b.claim.value`` (the ``StrEnum`` wire token) so each ``groupby`` run is contiguous and
    a future enum reorder can never fragment a claim into two sub-apps. ``result_action="return_value"``
    makes each command return its ``Envelope`` so ``__main__`` resolves the exit code.
    """
    root = App(name="assay", help="Rasm polyglot quality operator.", default_parameter=Parameter(show_default=False), result_action="return_value")
    keyed = sorted(registry, key=lambda b: b.claim.value)
    subs = tuple(
        reduce(lambda app, row: _register(app, _leaf(row), name=row.verb, help=row.help), tuple(rows), App(name=claim.value))
        for claim, rows in groupby(keyed, key=attrgetter("claim"))
    )
    app = reduce(_register, subs, root)
    _register(app, self_test, name="self-test")
    _register(app, delta, name="delta")  # root verb: read-back of the auto-persisted run history (sibling to self-test)
    return app


def _register(app: App, obj: App | Callable[..., object], *, name: str | None = None, help: str = "") -> App:  # noqa: A002  # cyclopts command kwarg is named "help"; mirroring the CLI surface
    # App.command returns the registered object, not the parent, so this seam returns app to keep the
    # groupby/sub-app folds point-free; name/help are forwarded only when present (App-flatten has no name).
    match (name, help):
        case (None, _):
            app.command(obj)
        case (verb, ""):
            app.command(obj, name=verb)
        case (verb, text):
            app.command(obj, name=verb, help=text)
    return app


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["REGISTRY", "Handler", "build_app", "delta", "rail", "self_test"]
