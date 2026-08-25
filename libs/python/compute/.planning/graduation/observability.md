# [PY_COMPUTE_OBSERVABILITY]

Observability owner of the compute plane: the hook rail registers compute's derived point vocabulary on the runtime `Hooks` registry, and the resource ledger prices every measured kernel off the runtime `Cost` substrate bracket. Both weave through the hub `evidence_run` binding, so a producer composing the hub gains enter/exit facts, UCUM metric rows, receipt lines, and the cpu-rss-io-switch band with zero page-local emit calls. Domain code fires facts and observability subscribes — the telemetry-as-tap law — so metric, receipt, and log lines project from one fired fact.

Composition splits by altitude: `registered()` proves the census and the scope map then registers points, `tapped()` attaches the built-in taps, and firing stays hub-owned — `ledgered` wraps every producer dispatch inside the weave and the graduation rail fires its admission facts on both arms. Every leg threads the caller's composition `ScopeKey`, so two embedded compositions partition point custody, tap fan-out, and refusal receipts exactly as the registry does.

Tenant attribution joins at the backend through the `rasm.tenant` baggage fold on `Metrics.record`; this page imports no OTel symbol and constructs no SDK surface.

## [01]-[INDEX]

- [02]-[HOOK_RAIL]: the `ComputePoint` id roster and the tail-keyed `POINT_ROWS`/`LIFECYCLE`/`COMPUTE_POINTS` tables folded off it, the `SCOPE_DOMAIN` correspondence, closed payload family, measure-mapping rows with their `MEASURES` descriptor roster and its census proof, and the registration/tap split under one `ComputeInstall` receipt.
- [03]-[RESOURCE_LEDGER]: `ResourceUsage` band over the runtime `Cost` bracket, its settled/raised outcome, and the `ledgered` hub weave every producer rides with the package's one `Resource.COMPUTE` charge in its async close and the optional `StageTap` stream at its open.

## [02]-[HOOK_RAIL]

- Owner: `ComputePoint` — the folder's ONE hook-id roster, and the source every point table folds over. The runtime registry closes its id vocabulary at the owning package's roster, so the derivation that used to mint ids at registration inverts rather than disappears: the roster states each `rasm.<DOMAIN>.<domain>.<tail>` id once, `POINT_ROWS` keys payload and delivery arm on the TAIL so a new point domain costs no row, `LIFECYCLE` groups the roster on its own grammar, and `COMPUTE_POINTS` is one map over it. `SCOPE_DOMAIN` maps every `EvidenceScope` member onto one domain totally, so a new module leaf is one row whose lifecycle triple already exists, and a colliding registration refuses structurally within the composition it registers into. That point-id `<domain>` segment is Tier-0's carved-out hook vocabulary, disjoint from the capability roster the census gate resolves against, so each keeps its own name and neither shadows the other.
- Cases: every domain carries a lifecycle TRIPLE — the `dispatched` enter fact, the `resolved` resource band, and the `staged` mid-operation mark whose payload is the hook registry's shared `StageMark`, composed rather than cloned. Graduation carries two admission facts beside its triple — `admitted` observed off the cleared receipt, `rejected` on the one retaining row so a late-attaching diagnostic subscriber drains the recent refusals on attach, then observes forward. Depth rides that retaining arm alone, never a flat column every observe row spells and never reads. A `Modality(veto=None)` row lands when an app claims the admission policy; the family already carries the arm.
- Entry: `registered(composition)` is the library leg and `ComputeInstall` its receipt — the `MEASURES` descriptor proof and the `SCOPE_DOMAIN` totality proof BOTH run before any registration, so a measure the census never admitted, one whose unit or fold the census mounts differently, and an unmapped module leaf each refuse at composition with typed evidence rather than as a dispatch-time `KeyError`, a producer-killing record fault, or a board silently reading a rescaled series; the two read-only families accumulate into ONE refusal, and the registration phase then claims the WHOLE point roster in one gated transition whose own collisions report together, since a descriptor divergence returned ahead of the scope walk hides every leaf behind it and a per-point traverse leaves the half-mounted custody a refusal can never retire; `tapped(composition)` is the app leg — one package-identity `TapRow(receipts=...)` fanned across the claimed roster in ONE subscribe beside a per-point `TapRow(metrics=...)`, the id's domain segment riding the row's `kind` slot and landing under the runtime `rasm.kind` attribute, so the backend discriminates domains on one instrument family. Its answer is the `Attachment` block a composition root brackets and releases through, never a count that retires nothing. Both legs default `DEFAULT_SCOPE` and thread the caller's key otherwise, so an embedded composition owns its points, taps, and refusal receipts, and one `scope` reaches both halves of a row so a fired fact's two evidence planes cannot partition. Producers hold no fire call: the hub weave fires the lifecycle points and the graduation rail its admission facts, so every producer dispatch reports itself.
- Auto: `_measures` is the one polymorphic projection every metrics tap shares — keys spell the runtime instrument rows exactly, so `Metrics.record`'s census resolution is a lookup, never a rename; a payload without numeric measures projects `{}` and contributes receipts alone. `MEASURES` carries the WHOLE descriptor per row — spelling, UCUM unit, and mounted instrument family — derives each row's roster segment off its own spelling, and IS what `registered` proves against the imported runtime `CENSUS` map, so this table is the transcription source its census counterpart reads and a unit or fold divergence refuses by name instead of silently rescaling a board.
- Growth: a new module leaf is one `SCOPE_DOMAIN` row; a new point domain is one `Domain` literal member beside its three `ComputePoint` rows, whose payload, delivery arm, lifecycle grouping, and registration all derive; a new point tail is one `POINT_ROWS` row reaching every domain at once; a new measure is one instrument-name constant, one `MEASURES` row, one `_measures` arm, and its runtime mapped `INSTRUMENTS` row in the SAME change, the install gate refusing a lagging, mis-united, or mis-folded row by name; a new substrate cost column is one `ResourceUsage` field off `Cost`, reaching the receipt line with no tap edit and earning a measure only where a board joins on it; a new admission fact is one point row and one fire arm on the owning rail; a new composition is one `ScopeKey` threaded from the hub binding through every leg, never a sibling registry.
- Boundary: the runtime owns the registry, the taps, the domain roster, and the instrument table — `domain="compute"` is a `DOMAINS` row and each measure a mapped `InstrumentSpec` row on `runtime/observability/metrics#METRIC`, a runtime ripple landing in the same change, never a compute-side mint. Composition custody reaches point rows, tap fan-out, and recorded series alike: `tapped` threads its key into the `TapRow` it binds through the one subscribe door, so an embedded composition's series carry its stamp exactly as its receipts do and a page-local recording closure beside the tap stays the forked projection this owner refuses. No folder-local process handle or `oneshot` bracket beside the runtime `Cost` owner, and no folder-local honest-RSS or no-peak rationale re-stated beside it. Libraries register points only; subscriber attachment stays at app composition, and no second egress rides beside the taps.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Awaitable, Callable, Mapping
from enum import StrEnum
from inspect import iscoroutinefunction
from sys import exception
from time import perf_counter
from typing import Final, Literal, get_args

import anyio
from expression import Error, Nothing, Option, Some
from expression.collections import Block, Map
from msgspec import Struct

from rasm.compute.graduation.handoff import ComputeLeg, EvidenceScope, StageTap
from rasm.runtime.faults import TERMINAL, Disposition, FaultRow, RuntimeRail, rostered, traversed
from rasm.runtime.hooks import Attachment, HookPoint, Hooks, Modality, StageMark, TapRow
from rasm.runtime.journal import Journal, MeterFact, Resource
from rasm.runtime.metrics import MEASURES as CENSUS
from rasm.runtime.metrics import InstrumentKind
from rasm.runtime.receipts import DEFAULT_SCOPE, OPEN, Cost, Receipt, ScopeKey, Signals

# --- [TYPES] ----------------------------------------------------------------------------

type Domain = Literal["analysis", "graduation", "jit", "numerics", "program", "solve", "study"]
type Measures = Mapping[str, float]
type Outcome = Literal["settled", "raised"]


class ComputePoint(StrEnum):
    ANALYSIS_DISPATCHED = "rasm.compute.analysis.dispatched"
    ANALYSIS_RESOLVED = "rasm.compute.analysis.resolved"
    ANALYSIS_STAGED = "rasm.compute.analysis.staged"
    GRADUATION_DISPATCHED = "rasm.compute.graduation.dispatched"
    GRADUATION_RESOLVED = "rasm.compute.graduation.resolved"
    GRADUATION_STAGED = "rasm.compute.graduation.staged"
    JIT_DISPATCHED = "rasm.compute.jit.dispatched"
    JIT_RESOLVED = "rasm.compute.jit.resolved"
    JIT_STAGED = "rasm.compute.jit.staged"
    NUMERICS_DISPATCHED = "rasm.compute.numerics.dispatched"
    NUMERICS_RESOLVED = "rasm.compute.numerics.resolved"
    NUMERICS_STAGED = "rasm.compute.numerics.staged"
    PROGRAM_DISPATCHED = "rasm.compute.program.dispatched"
    PROGRAM_RESOLVED = "rasm.compute.program.resolved"
    PROGRAM_STAGED = "rasm.compute.program.staged"
    SOLVE_DISPATCHED = "rasm.compute.solve.dispatched"
    SOLVE_RESOLVED = "rasm.compute.solve.resolved"
    SOLVE_STAGED = "rasm.compute.solve.staged"
    STUDY_DISPATCHED = "rasm.compute.study.dispatched"
    STUDY_RESOLVED = "rasm.compute.study.resolved"
    STUDY_STAGED = "rasm.compute.study.staged"
    ADMITTED = "rasm.compute.graduation.admitted"
    REJECTED = "rasm.compute.graduation.rejected"

    @property
    def domain(self) -> str:
        return self.value.split(".")[2]

    @property
    def tail(self) -> str:
        return self.value.split(".")[3]


class WeaveStage(StrEnum):
    OPENED = "opened"

# --- [CONSTANTS] ------------------------------------------------------------------------

DOMAIN: Final[str] = "compute"

PACKAGE: Final[str] = f"rasm.{DOMAIN}"

POINT_DOMAINS: Final[tuple[Domain, ...]] = get_args(Domain.__value__)

DISPATCHED: Final[str] = "dispatched"
RESOLVED: Final[str] = "resolved"
STAGED: Final[str] = "staged"
DOMAIN_TAILS: Final[frozenset[str]] = frozenset({DISPATCHED, RESOLVED, STAGED})

REFUSAL_WINDOW: Final[int] = 64

EVIDENCE_DURATION: Final[str] = "rasm.compute.evidence.duration"
EVIDENCE_CPU: Final[str] = "rasm.compute.evidence.cpu_time"
EVIDENCE_RSS: Final[str] = "rasm.compute.evidence.rss_delta"
GRADUATION_RESIDUALS: Final[str] = "rasm.compute.graduation.residual_count"

METER_GRACE: Final[float] = 1.0

# --- [MODELS] ---------------------------------------------------------------------------


class MeasureRow(Struct, frozen=True, gc=False):
    measure: str
    unit: str
    kind: InstrumentKind

    @property
    def domain(self) -> str:
        return self.measure.split(".", 2)[1]


class EvidenceFired(Struct, frozen=True, gc=False):
    scope: str
    subject: str


class ResourceUsage(Struct, frozen=True, gc=False):
    scope: str
    subject: str
    outcome: Outcome
    wall_ms: float
    cpu_s: float
    rss_bytes: int
    io_bytes: int
    switches: int

    @classmethod
    def of(cls, scope: EvidenceScope, subject: str, outcome: Outcome, started: float, before: Cost) -> "ResourceUsage":
        spend = Cost.own().delta(before)
        return cls(
            scope=scope.value,
            subject=subject,
            outcome=outcome,
            wall_ms=(perf_counter() - started) * 1000.0,
            cpu_s=spend.cpu_ms / 1000.0,
            rss_bytes=spend.rss_bytes,
            io_bytes=spend.io_bytes,
            switches=spend.switches,
        )


class GraduationAdmitted(Struct, frozen=True, gc=False):
    axis: str
    subject: str
    evidence_key: str
    residual_count: int


class GraduationRejected(Struct, frozen=True, gc=False):
    subject: str
    tag: str
    detail: str


class ComputeInstall(Struct, frozen=True, gc=False):
    points: tuple[str, ...]
    measures: tuple[str, ...]


# --- [TABLES] ---------------------------------------------------------------------------


MEASURES: Final[tuple[MeasureRow, ...]] = (
    MeasureRow(EVIDENCE_DURATION, "ms", InstrumentKind.HISTOGRAM),
    MeasureRow(EVIDENCE_CPU, "s", InstrumentKind.COUNTER),
    MeasureRow(EVIDENCE_RSS, "By", InstrumentKind.HISTOGRAM),
    MeasureRow(GRADUATION_RESIDUALS, "{residual}", InstrumentKind.HISTOGRAM),
)

SCOPE_DOMAIN: Final[Map[EvidenceScope, Domain]] = Map.of_seq([
    (EvidenceScope.ARRAY, "numerics"),
    (EvidenceScope.CODEGEN, "graduation"),
    (EvidenceScope.CONVEX, "program"),
    (EvidenceScope.DESIGN, "program"),
    (EvidenceScope.DIFFERENTIAL, "solve"),
    (EvidenceScope.FIELD, "solve"),
    (EvidenceScope.HANDOFF, "graduation"),
    (EvidenceScope.HISTORY, "study"),
    (EvidenceScope.INFERENCE, "study"),
    (EvidenceScope.INTERVAL, "numerics"),
    (EvidenceScope.JIT, "jit"),
    (EvidenceScope.LINEAR, "solve"),
    (EvidenceScope.MESH, "solve"),
    (EvidenceScope.MODEL, "study"),
    (EvidenceScope.NONLINEAR, "solve"),
    (EvidenceScope.PROGRAM, "program"),
    (EvidenceScope.QUADRATURE, "solve"),
    (EvidenceScope.QUANTITY, "numerics"),
    (EvidenceScope.RECEIPT, "solve"),
    (EvidenceScope.SENSITIVITY, "solve"),
    (EvidenceScope.SIGNAL, "analysis"),
    (EvidenceScope.SPATIAL, "analysis"),
    (EvidenceScope.STATISTICS, "numerics"),
    (EvidenceScope.STUDY, "study"),
    (EvidenceScope.SYMBOLIC, "analysis"),
    (EvidenceScope.TRANSFORM, "analysis"),
])

POINT_ROWS: Final[Map[str, tuple[type[Struct], Modality]]] = Map.of_seq([
    (DISPATCHED, (EvidenceFired, Modality(observe=None))),
    (RESOLVED, (ResourceUsage, Modality(observe=None))),
    (STAGED, (StageMark, Modality(observe=None))),
    (ComputePoint.ADMITTED.tail, (GraduationAdmitted, Modality(observe=None))),
    (ComputePoint.REJECTED.tail, (GraduationRejected, Modality(replay=REFUSAL_WINDOW))),
])

LIFECYCLE: Final[Map[str, Map[str, ComputePoint]]] = Block.of_seq(ComputePoint).fold(
    lambda held, point: held.add(point.domain, held.try_find(point.domain).default_value(Map.empty()).add(point.tail, point)),
    Map.empty(),
)

COMPUTE_POINTS: Final[Block[HookPoint[Struct]]] = Block.of_seq(ComputePoint).map(
    lambda point: HookPoint(id=point, payload=POINT_ROWS[point.tail][0], modality=POINT_ROWS[point.tail][1])
)

INSTALL_CENSUS: Final[FaultRow[ComputeLeg]] = FaultRow(
    leg=ComputeLeg.OBSERVABILITY, point="install", arm="config", defect="census-breach", retriability=TERMINAL, slots=("breaches",)
)
RAISES: Final[Block[FaultRow[ComputeLeg]]] = rostered(Block.of_seq([INSTALL_CENSUS]))

# --- [OPERATIONS] -----------------------------------------------------------------------


def _measures(payload: Struct) -> Measures:
    match payload:
        case ResourceUsage() as band:
            return {EVIDENCE_DURATION: band.wall_ms, EVIDENCE_CPU: band.cpu_s, EVIDENCE_RSS: float(band.rss_bytes)}
        case GraduationAdmitted(residual_count=count):
            return {GRADUATION_RESIDUALS: float(count)}
        case _:
            return {}


def _diverged(row: MeasureRow) -> Option[str]:
    match CENSUS.try_find((row.domain, row.measure)):
        case Option(tag="none"):
            return Some(f"uncensused:{row.measure}")
        case Option(tag="some", some=spec) if spec.unit != row.unit:
            return Some(f"unit:{row.measure}:{row.unit}!={spec.unit}")
        case Option(tag="some", some=spec) if spec.kind is not row.kind:
            return Some(f"fold:{row.measure}:{row.kind.value}!={spec.kind.value}")
        case _:
            return Nothing


def registered(composition: ScopeKey = DEFAULT_SCOPE) -> RuntimeRail[ComputeInstall]:
    refused = (
        Block.of_seq(MEASURES)
        .choose(_diverged)
        .append(Block.of_seq(scope for scope in EvidenceScope if SCOPE_DOMAIN.try_find(scope).is_none()).map(lambda scope: f"unmapped-scope:{scope.value}"))
        .append(
            Block.of_seq(domain for domain in POINT_DOMAINS if not DOMAIN_TAILS <= frozenset(LIFECYCLE.try_find(domain).default_value(Map.empty()).keys()))
            .map(lambda domain: f"partial-lifecycle:{domain}")
        )
    )
    if not refused.is_empty():
        return Error(INSTALL_CENSUS.raised(";".join(refused)))
    return Hooks.register(COMPUTE_POINTS, scope=composition).map(
        lambda points: Hooks.installed(
            "compute.graduation",
            ComputeInstall(points=tuple(point.id for point in points), measures=tuple(row.measure for row in MEASURES)),
            scope=composition,
        )
    )


def tapped(composition: ScopeKey = DEFAULT_SCOPE) -> RuntimeRail[Block[Attachment]]:
    metrics = Block.of_seq(ComputePoint).map(
        lambda point: Hooks.subscribe(point, TapRow(metrics=(_measures, DOMAIN, point.domain)), scope=composition)
    )
    return Hooks.subscribe(COMPUTE_POINTS, TapRow(receipts=PACKAGE), scope=composition).bind(
        lambda fanned: traversed(metrics, by=Disposition.ABORT).map(fanned.append)
    )
```

## [03]-[RESOURCE_LEDGER]

- Owner: `ResourceUsage` — wallclock beside the runtime `Cost` substrate delta (cpu, rss, io, context switches) around each measured kernel, with the settled-or-raised outcome; the band is a hook payload, so it reaches metrics, receipts, and log lines through the taps rather than a second emit surface, and the backend joins solve cost to the `rasm.tenant` baggage the runtime promotes. Columns stay flat because the generic receipt tap is a shallow `structs.asdict` and OTLP log attributes refuse a nested mapping.
- Law: the band's ASYNC close is the package's ONE `Resource.COMPUTE` seat on the `python:runtime/observability/journal#LEDGER` plane — every producer dispatch rides this weave, so the burn charges here once, surfaced by the point domain the taps already discriminate on, and a second COMPUTE fact at any producer bills it twice. The async arm is the whole seat by the producer-seam law: recording suspends, so the sync arm bands, fires, and charges nothing, and declaring a sync dispatch async to reach the seat bands the coroutine MINT and prices the awaited body at zero. The record runs shielded under one bounded grace and its rail is DISCARDED, because this runs inside `finally`: a cancelled dispatch still burned the cpu it owes, an unbounded shield behind an unrelieved intake turns a bounded teardown into a hang, and a rail bound here replaces the kernel fault the band exists to price. No `RETENTION` table seats here — `MeterFact` carries `REGULATORY` by constitution and every compute audit class is fixed by what its own producer WRITES rather than by which axis produced it, so a table keyed on `Domain` or `HandoffAxis` carries one value per row and decides nothing.
- Entry: `ledgered` is the ONE band bracket — the hub weave leg `evidence_run` composes around every producer dispatch, enter fact, opened pulse stream where a fold carries one, band, exit fact — so every module-level measured kernel contributes the band with zero per-solver code, and a kernel outside the weave composes the same wrap rather than a settle-only sibling that returned its band as a value: handing a resource band back into domain code inverts the tap law this page holds, duplicates the bracket, and drops the raising arm the wrap already covers.
- Auto: the band closes in `finally` and `sys.exception()` decides its outcome — a raising kernel is exactly the dispatch whose resource price matters most, the weave's boundary fence one level up converts that raise, and an exit fact fired on the settled path alone loses every expensive failure. Sampling itself is the runtime `Cost` bracket's: one `oneshot` collection per edge, `cpu_times` never `cpu_percent` because attribution needs exact spend rather than a since-last-call ratio shared across readers, io platform-gated independently, and RSS an instantaneous pair whose signed delta claims no unobserved peak. Refused fires — pre-registration, unregistered id, wrong composition — land as rejected receipts through `fired` under the same `ScopeKey`, never a silent drop.
- Growth: a new band field is one `ResourceUsage` field off the substrate `Cost` column, one `_measures` key where a board joins on it, and its runtime instrument row in the same change; a new sampling site is nothing — the weave already covers every producer; a newly metered compute resource is one `MeterFact` at the fold that owns the quantity, never a second `Resource.COMPUTE` seat beside this band's.
- Boundary: no second `Resource.COMPUTE` meter below this weave and no producer-side re-sampling of a burn the band already charged; no ledger, custody, or retention window minted here — the plane arrives bound at the composition root and every `Journal.record` in this package folds lawfully to zero where none was installed. The substrate bracket reads its own process handle, which inside a lane is the worker's; cross-process aggregation stays the runtime lanes owner's, and this page mints no handle and no bracket of its own. Awaited bands price whole-process spend across the await window, so a concurrent task's burn lands in the same band — the crossing price, never isolated kernel attribution. Sync-declared dispatch minting an awaitable bands the mint alone — the settle rides the runtime `measured` continuation — so an async kernel declares the async modality to band its await.

```python signature
# --- [OPERATIONS] -----------------------------------------------------------------------


def stage_point(scope: EvidenceScope) -> ComputePoint:
    return LIFECYCLE[SCOPE_DOMAIN[scope]][STAGED]


def fired(point_id: ComputePoint, payload: Struct, composition: ScopeKey = DEFAULT_SCOPE) -> None:
    Hooks.fire(point_id, payload, scope=composition).swap().map(
        lambda fault: Signals.emit(Receipt.of(point_id, fault), OPEN, scope=composition)
    )


def _awaiting(dispatch: object) -> bool:
    return iscoroutinefunction(dispatch) or iscoroutinefunction(getattr(dispatch, "__call__", None))


def ledgered[T](
    scope: EvidenceScope, subject: str, dispatch: Callable[[], T] | Callable[[], Awaitable[T]], *, composition: ScopeKey = DEFAULT_SCOPE,
    stage: Option[StageTap] = Nothing,
) -> Callable[[], T] | Callable[[], Awaitable[T]]:
    domain = SCOPE_DOMAIN[scope]
    enter, resolve = LIFECYCLE[domain][DISPATCHED], LIFECYCLE[domain][RESOLVED]

    def opened() -> tuple[Cost, float]:
        fired(enter, EvidenceFired(scope=scope.value, subject=subject), composition)
        stage.map(lambda mark: mark.beat(WeaveStage.OPENED, 0))
        return Cost.own(), perf_counter()

    def closed(before: Cost, started: float) -> ResourceUsage:
        outcome: Outcome = "raised" if exception() is not None else "settled"
        band = ResourceUsage.of(scope, subject, outcome, started, before)
        fired(resolve, band, composition)
        return band

    async def charged(band: ResourceUsage) -> None:
        with anyio.move_on_after(METER_GRACE, shield=True):
            await Journal.record(
                MeterFact(resource=Resource.COMPUTE, quantity=max(int(band.cpu_s * 1000.0), 0), surface=domain), scope=composition
            )

    if _awaiting(dispatch):

        async def sampled_async() -> T:
            before, started = opened()
            try:
                return await dispatch()
            finally:
                await charged(closed(before, started))

        return sampled_async

    def sampled() -> T:
        before, started = opened()
        try:
            return dispatch()
        finally:
            closed(before, started)

    return sampled
```

## [04]-[RESEARCH]

<!-- source-only: research row template; every landed row opens on the list dash this placeholder omits, the census reading `^- [TOKEN]-[OPEN|BLOCKED]:` alone:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
