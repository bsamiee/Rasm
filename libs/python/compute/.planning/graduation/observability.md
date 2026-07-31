# [PY_COMPUTE_OBSERVABILITY]

Observability owner of the compute plane: the hook rail registers compute's derived point vocabulary on the runtime `Hooks` registry, and the resource ledger prices every measured kernel off the runtime `Cost` substrate bracket. Both weave through the hub `evidence_run` binding, so a producer composing the hub gains enter/exit facts, UCUM metric rows, receipt lines, and the cpu-rss-io-switch band with zero page-local emit calls. Domain code fires facts and observability subscribes — the telemetry-as-tap law — so metric, receipt, and log lines project from one fired fact.

Composition splits by altitude: `registered()` proves the census and the scope map then registers points, `tapped()` attaches the built-in taps, and firing stays hub-owned — `ledgered` wraps every producer dispatch inside the weave and the graduation rail fires its admission facts on both arms. Every leg threads the caller's composition `ScopeKey`, so two embedded compositions partition point custody, tap fan-out, and refusal receipts exactly as the registry does.

Tenant attribution joins at the backend through the `rasm.tenant` baggage fold on `Metrics.record`; this page imports no OTel symbol and constructs no SDK surface.

## [01]-[INDEX]

- [02]-[HOOK_RAIL]: derived `COMPUTE_POINTS` table over the `SCOPE_DOMAIN` correspondence, closed payload family, measure-mapping rows with their `MEASURES` descriptor roster and its census proof, and the registration/tap split under one `ComputeInstall` receipt.
- [03]-[RESOURCE_LEDGER]: `ResourceUsage` band over the runtime `Cost` bracket, its settled/raised outcome, and the `ledgered` hub weave every producer rides.

## [02]-[HOOK_RAIL]

- Owner: `COMPUTE_POINTS` — one derived point table, never hand-enumerated rows: the `Domain` literal is the point-id domain set `POINT_DOMAINS` projects, `SCOPE_DOMAIN` maps every `EvidenceScope` member onto one domain totally, and the lifecycle pair (`dispatched` enter fact, `resolved` resource band) derives per domain by comprehension, so a new module leaf is one `SCOPE_DOMAIN` row whose lifecycle points already exist. Ids spell `rasm.<DOMAIN>.<domain>.<point>` under the runtime `runtime/observability/hooks#HOOKS` grammar off the one `DOMAIN` segment constant the tap argument also reads, and a colliding registration refuses structurally within the composition it registers into. That point-id `<domain>` segment is Tier-0's carved-out hook vocabulary, disjoint from the capability roster the census gate resolves against, so each keeps its own name and neither shadows the other.
- Cases: every `Domain` member carries its derived pair; graduation carries two admission facts beside its pair — `rasm.compute.graduation.admitted` as an `OBSERVE` fact off the cleared receipt, `rasm.compute.graduation.rejected` on a `REPLAY` ring (`buffer=64`) so a late-attaching diagnostic subscriber drains the recent refusals on attach, then observes forward. `VETO` admission lands as one more row when an app claims the policy; `Modality` already carries the arm.
- Entry: `registered(composition)` is the library leg and `ComputeInstall` its receipt — the `MEASURES` descriptor proof and the `SCOPE_DOMAIN` totality proof BOTH run before any registration, so a measure the census never admitted, one whose unit or fold the census mounts differently, and an unmapped module leaf each refuse at composition with typed evidence rather than as a dispatch-time `KeyError`, a producer-killing record fault, or a board silently reading a rescaled series; the two read-only families accumulate into ONE refusal, and the registration phase then claims the WHOLE point roster in one gated transition whose own collisions report together, since a descriptor divergence returned ahead of the scope walk hides every leaf behind it and a per-point traverse leaves the half-mounted custody a refusal can never retire; `tapped(composition)` is the app leg — per point one `tap_receipts` and one `tap_metrics` subscription, the id's domain segment riding the tap's `kind` slot and landing under the runtime `rasm.kind` attribute, so the backend discriminates domains on one instrument family. Both legs default `DEFAULT_SCOPE` and thread the caller's key otherwise, so an embedded composition owns its points, taps, and refusal receipts. Producers hold no fire call: the hub weave fires the lifecycle points and the graduation rail its admission facts, so every producer dispatch reports itself.
- Measures: `_measures` is the one polymorphic projection every metrics tap shares — keys spell the runtime instrument rows exactly, so `Metrics.record`'s census resolution is a lookup, never a rename; a payload without numeric measures projects `{}` and contributes receipts alone. `MEASURES` carries the WHOLE descriptor per row — spelling, UCUM unit, and mounted instrument family — derives each row's roster segment off its own spelling, and IS what `registered` proves against the imported runtime `CENSUS` map, so this table is the transcription source its census counterpart reads and a unit or fold divergence refuses by name instead of silently rescaling a board.
- Growth: a new module leaf is one `SCOPE_DOMAIN` row; a new point domain is one `Domain` literal member whose pair derives; a new measure is one instrument-name constant, one `MEASURES` row, one `_measures` arm, and its runtime mapped `INSTRUMENTS` row in the SAME change, the install gate refusing a lagging, mis-united, or mis-folded row by name; a new substrate cost column is one `ResourceUsage` field off `Cost`, reaching the receipt line with no tap edit and earning a measure only where a board joins on it; a new admission fact is one point row and one fire arm on the owning rail; a new composition is one `ScopeKey` threaded from the hub binding through every leg, never a sibling registry.
- Boundary: the runtime owns the registry, the taps, the domain roster, and the instrument table — `domain="compute"` is a `DOMAINS` row and each measure a mapped `InstrumentSpec` row on `runtime/observability/metrics#METRIC`, a runtime ripple landing in the same change, never a compute-side mint. Composition custody reaches point rows, tap fan-out, and recorded series alike: `tapped` threads its key into `Hooks.tap_metrics(..., scope=composition)`, so an embedded composition's series carry its stamp exactly as its receipts do and a page-local recording closure beside the tap stays the forked projection this owner refuses. No folder-local process handle or `oneshot` bracket beside the runtime `Cost` owner, and no folder-local honest-RSS or no-peak rationale re-stated beside it. Libraries register points only; subscriber attachment stays at app composition, and no second egress rides beside the taps.

```python signature
# --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
from collections.abc import Awaitable, Callable, Mapping
from inspect import iscoroutinefunction
from sys import exception
from time import perf_counter
from typing import Final, Literal, get_args

from expression import Error, Nothing, Option, Some
from expression.collections import Block, Map
from msgspec import Struct

from rasm.compute.graduation.handoff import EvidenceScope
from rasm.runtime.faults import BoundaryFault, Disposition, RuntimeRail, traversed
from rasm.runtime.hooks import HookPoint, Hooks, Modality
from rasm.runtime.metrics import MEASURES as CENSUS
from rasm.runtime.metrics import InstrumentKind
from rasm.runtime.receipts import DEFAULT_SCOPE, OPEN, Cost, Receipt, ScopeKey, Signals

# --- [TYPES] --------------------------------------------------------------------------------

type Domain = Literal["analysis", "graduation", "jit", "numerics", "program", "solve", "study"]
type Measures = Mapping[str, float]
type Outcome = Literal["settled", "raised"]

# --- [CONSTANTS] ------------------------------------------------------------------------

# roster segment every compute measure and every point id records under; the runtime DOMAINS row admits it, and the
# census map keys on the pair, so the tap argument and the id derivation read one spelling.
DOMAIN: Final[str] = "compute"

# derived column law: the Literal IS the point-id domain set, so point derivation, `SCOPE_DOMAIN` codomain, and tap kind
# attribute can never disagree. This roster stays spelled APART from the runtime `DOMAINS` capability roster the census
# gate reads: a hook-point id's third segment and a metric name's `<domain>` segment are two vocabularies Tier-0 carves
# apart, so one name over both silently tests the module-leaf roster where the capability roster belongs and every
# census pair falls out unmatched.
POINT_DOMAINS: Final[tuple[Domain, ...]] = get_args(Domain.__value__)

# graduation's two admission facts beside the derived lifecycle pairs; ids obey the runtime
# `rasm.<pkg>.<domain>.<point>` grammar the register fence proves.
ADMITTED: Final[str] = "rasm.compute.graduation.admitted"
REJECTED: Final[str] = "rasm.compute.graduation.rejected"

# instrument spellings `_measures` records under — wire law spelled whole so a grep resolves the series, matching
# runtime `INSTRUMENTS` row style; a drifted key has no slot and no owner.
EVIDENCE_DURATION: Final[str] = "rasm.compute.evidence.duration"
EVIDENCE_CPU: Final[str] = "rasm.compute.evidence.cpu_time"
EVIDENCE_RSS: Final[str] = "rasm.compute.evidence.rss_delta"
GRADUATION_RESIDUALS: Final[str] = "rasm.compute.graduation.residual_count"

# --- [MODELS] ---------------------------------------------------------------------------


class MeasureRow(Struct, frozen=True, gc=False):
    # whole descriptor of one recorded measure — the spelling `_measures` keys on, the UCUM unit the exported descriptor
    # carries, and the instrument family the runtime census mounts. Every column is proved at install rather than
    # transcribed and trusted, because a unit divergence rescales every board reading the series by a silent factor and
    # a kind divergence hands the backend a fold this table never declared.
    measure: str
    unit: str
    kind: InstrumentKind

    @property
    def domain(self) -> str:
        # derived exactly as the runtime `InstrumentSpec` derives its own, so a row cannot declare one roster segment
        # and spell another, and every pair keys the census map by construction rather than by convention.
        return self.measure.split(".", 2)[1]


class EvidenceFired(Struct, frozen=True, gc=False):
    # enter fact of every hub dispatch: scope names the producer, subject its call-time discriminant.
    scope: str
    subject: str


class ResourceUsage(Struct, frozen=True, gc=False):
    # exit band of every hub dispatch: wallclock beside the runtime `Cost` substrate delta, so cpu, rss, io, and
    # context switches ride the ONE process-sampling owner and this page mints no second bracket, no second process
    # handle, and no second honest-RSS rationale. Columns stay FLAT: the generic `tap_receipts` projection is a
    # shallow `structs.asdict` and OTLP log attributes refuse a nested mapping, so the substrate struct never
    # crosses whole. `outcome` discriminates a settled dispatch from a raising one, so the band survives — and
    # reports — the failure path whose resource price matters most.
    scope: str
    subject: str
    outcome: Outcome
    wall_ms: float
    cpu_s: float  # UCUM second its census row exports; the substrate spends milliseconds
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
    boundary: str
    reason: str


class ComputeInstall(Struct, frozen=True, gc=False):
    # composition-time proof of this owner's whole admission — point ids now deliverable in the caller's composition
    # beside the measure spellings the runtime census admits. One receipt answers what compute registered and what it
    # may record, where handing the caller the registry's own `HookPoint` rows leaks a `type[Struct]` field no receipt
    # projection renders and names the registry's product rather than this owner's.
    points: tuple[str, ...]
    measures: tuple[str, ...]


# --- [TABLES] -----------------------------------------------------------------------------


def _point(domain: Domain, tail: str) -> str:
    return f"rasm.{DOMAIN}.{domain}.{tail}"


def _domain(point_id: str) -> str:
    # third id segment IS the point's domain under the grammar `register` proves, so the tap `kind` and the
    # derivation that minted the id read one source rather than a parallel row.
    return point_id.split(".", 3)[2]


# every measure this owner records, each carrying the whole descriptor its census counterpart mounts; the roster IS the
# transcription source the runtime `INSTRUMENTS` row reads, so the two tables are one edit and `registered` proves it.
MEASURES: Final[tuple[MeasureRow, ...]] = (
    MeasureRow(EVIDENCE_DURATION, "ms", InstrumentKind.HISTOGRAM),
    MeasureRow(EVIDENCE_CPU, "s", InstrumentKind.COUNTER),
    MeasureRow(EVIDENCE_RSS, "By", InstrumentKind.HISTOGRAM),
    MeasureRow(GRADUATION_RESIDUALS, "{residual}", InstrumentKind.HISTOGRAM),
)

# total over EvidenceScope: every scope maps onto one domain, so a producer's lifecycle points exist
# before its first dispatch; `registered()` proves the totality at composition, never a dispatch KeyError.
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

# lifecycle pair per domain: the enter fact and the exit band; both derive, neither is hand-listed.
_LIFECYCLE: Final[tuple[tuple[str, type[Struct]], ...]] = (("dispatched", EvidenceFired), ("resolved", ResourceUsage))

COMPUTE_POINTS: Final[Block[HookPoint[Struct]]] = Block.of_seq([
    *(HookPoint(id=_point(domain, tail), payload=payload, modality=Modality.OBSERVE) for domain in POINT_DOMAINS for tail, payload in _LIFECYCLE),
    HookPoint(id=ADMITTED, payload=GraduationAdmitted, modality=Modality.OBSERVE),
    HookPoint(id=REJECTED, payload=GraduationRejected, modality=Modality.REPLAY, buffer=64),
])

# --- [OPERATIONS] -----------------------------------------------------------------------


def _measures(payload: Struct) -> Measures:
    # one polymorphic projection every metrics tap shares: keys spell the instrument rows exactly, so
    # `Metrics.record`'s census resolution is a lookup, never a rename; the band's flat substrate columns
    # project without a second derivation, and the enter fact and the rejection carry no numeric measure and
    # contribute receipts alone. The band's `io_bytes`/`switches` columns ride receipts alone — worker-crossing
    # byte and switch metering is the runtime `rasm.cost.*` family's, one measure one owner.
    match payload:
        case ResourceUsage() as band:
            return {EVIDENCE_DURATION: band.wall_ms, EVIDENCE_CPU: band.cpu_s, EVIDENCE_RSS: float(band.rss_bytes)}
        case GraduationAdmitted(residual_count=count):
            return {GRADUATION_RESIDUALS: float(count)}
        case _:
            return {}


def _diverged(row: MeasureRow) -> Option[str]:
    # one descriptor comparison the install gate folds over every measure row, each divergence naming itself: a pair no
    # mapped row admits under a rostered domain has no census key and kills its tap at the first fired band; a unit
    # divergence exports the recorded magnitude against a descriptor scaled differently, which every board reads off by
    # that factor; a kind divergence mounts a fold this roster never declared. `CENSUS` IS the runtime `MEASURES` map
    # every `Metrics.record` resolves, imported rather than re-derived, so the gate and the recording path read one map.
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
    # library leg in two phases, both read-only proofs BEFORE any registry mutation: every `MEASURES` row resolves its
    # WHOLE descriptor against runtime mapped instrument rows under a rostered domain — this gate resolves the
    # `CENSUS` pair the record performs, and proves the unit and kind columns rather than trusting them, so two
    # tables cannot drift into a silently rescaled series — and `SCOPE_DOMAIN` covers every `EvidenceScope` member, so
    # a leaf without its domain row refuses at composition, never as a dispatch-time `KeyError`. The two families
    # accumulate into ONE refusal: a descriptor divergence returned ahead of the scope walk hides every unmapped leaf
    # behind it and buys one restart per family. Only then does every compute point register once into the caller's
    # composition, its own collisions accumulating on the registration traversal, since mounting custody against an
    # unproved roster is exactly the state the first fired band then kills.
    refused = Block.of_seq(MEASURES).choose(_diverged).append(
        Block.of_seq(scope for scope in EvidenceScope if SCOPE_DOMAIN.try_find(scope).is_none()).map(lambda scope: f"unmapped-scope:{scope.value}")
    )
    if not refused.is_empty():
        return Error(BoundaryFault(boundary=("graduation.observability", ";".join(refused))))
    # ONE roster claim, never a per-point traverse: the registry's own arm swaps the point table only past its last
    # admitted row and reports every breach together, so a refusal leaves custody exactly as it stood. An accumulating
    # per-point fold instead half-mounts the table — every point ahead of the first breach stays claimed against a
    # composition whose install never completed, and no retire verb is owed against a claim that never returned.
    # Receipts deposit on the scoped registry inside their own mint: runtime imports no producer folder, so the
    # `Hooks` ledger is where this install reaches the support bundle, and an absent compute row in a captured archive
    # names the leg that never ran rather than leaving every unregistered-id refusal unexplained.
    return Hooks.register(COMPUTE_POINTS, scope=composition).map(
        lambda points: Hooks.installed(
            "compute.graduation",
            ComputeInstall(points=tuple(point.id for point in points), measures=tuple(row.measure for row in MEASURES)),
            scope=composition,
        )
    )


def tapped(composition: ScopeKey = DEFAULT_SCOPE) -> RuntimeRail[int]:
    # app leg: one receipts tap and one metrics tap per point, so metric, receipt, and log lines project from the
    # same fired fact; the id's domain segment rides `kind`, landing under the runtime `rasm.kind` attribute so the
    # backend discriminates domains on one instrument family, and the count terminal names how many taps attached.
    # Both taps subscribe into the caller's composition AND record under it, so an embedded second composition never
    # drains the first's facts and its series carry its own stamp rather than unioning into the root's; a tap on a
    # point this composition never registered refuses on the runtime subscribe fence.
    rails = COMPUTE_POINTS.collect(
        lambda point: Block.of_seq([
            Hooks.subscribe(point.id, Hooks.tap_receipts(point.id, scope=composition), scope=composition),
            Hooks.subscribe(point.id, Hooks.tap_metrics(_measures, domain=DOMAIN, kind=_domain(point.id), scope=composition), scope=composition),
        ])
    )
    return traversed(rails, by=Disposition.ACCUMULATE).map(len)
```

## [03]-[RESOURCE_LEDGER]

- Owner: `ResourceUsage` — wallclock beside the runtime `Cost` substrate delta (cpu, rss, io, context switches) around each measured kernel, with the settled-or-raised outcome; the band is a hook payload, so it reaches metrics, receipts, and log lines through the taps rather than a second emit surface, and the backend joins solve cost to the `rasm.tenant` baggage the runtime promotes. Columns stay flat because the generic receipt tap is a shallow `structs.asdict` and OTLP log attributes refuse a nested mapping.
- Entry: `ledgered` is the ONE band bracket — the hub weave leg `evidence_run` composes around every producer dispatch, enter fact, band, exit fact — so every module-level measured kernel contributes the band with zero per-solver code, and a kernel outside the weave composes the same wrap rather than a settle-only sibling that returned its band as a value: handing a resource band back into domain code inverts the tap law this page holds, duplicates the bracket, and drops the raising arm the wrap already covers.
- Auto: the band closes in `finally` and `sys.exception()` decides its outcome — a raising kernel is exactly the dispatch whose resource price matters most, the weave's boundary fence one level up converts that raise, and an exit fact fired on the settled path alone loses every expensive failure. Sampling itself is the runtime `Cost` bracket's: one `oneshot` collection per edge, `cpu_times` never `cpu_percent` because attribution needs exact spend rather than a since-last-call ratio shared across readers, io platform-gated independently, and RSS an instantaneous pair whose signed delta claims no unobserved peak. Refused fires — pre-registration, unregistered id, wrong composition — land as rejected receipts through `fired` under the same `ScopeKey`, never a silent drop.
- Growth: a new band field is one `ResourceUsage` field off the substrate `Cost` column, one `_measures` key where a board joins on it, and its runtime instrument row in the same change; a new sampling site is nothing — the weave already covers every producer.
- Boundary: the substrate bracket reads its own process handle, which inside a lane is the worker's; cross-process aggregation stays the runtime lanes owner's, and this page mints no handle and no bracket of its own. Awaited bands price whole-process spend across the await window, so a concurrent task's burn lands in the same band — the crossing price, never isolated kernel attribution. Sync-declared dispatch minting an awaitable bands the mint alone — the settle rides the runtime `measured` continuation — so an async kernel declares the async modality to band its await.

```python signature
# --- [OPERATIONS] -----------------------------------------------------------------------


def fired(point_id: str, payload: Struct, composition: ScopeKey = DEFAULT_SCOPE) -> None:
    # tee fire: the emitter's value is untouched by construction, and a refused fire folds onto the receipt stream
    # as evidence rather than dropping — under the SAME composition, so a refusal never leaks into the default sink.
    Hooks.fire(point_id, payload, scope=composition).swap().map(
        lambda fault: Signals.emit(Receipt.of(point_id, fault), OPEN, scope=composition)
    )


def _awaiting(dispatch: object) -> bool:
    # declared-modality probe mirroring the runtime `measured` split WHOLE: a callable instance whose `__call__` is a
    # coroutine function is async-declared, and the narrower name-only probe routes it to the sync arm where the band
    # closes over the coroutine MINT and prices the awaited body at zero.
    return iscoroutinefunction(dispatch) or iscoroutinefunction(getattr(dispatch, "__call__", None))


def ledgered[T](
    scope: EvidenceScope, subject: str, dispatch: Callable[[], T] | Callable[[], Awaitable[T]], *, composition: ScopeKey = DEFAULT_SCOPE
) -> Callable[[], T] | Callable[[], Awaitable[T]]:
    # hub weave leg: enter fact, band, exit fact around the one dispatch — `_awaiting` mirrors the runtime `measured`
    # split so an async dispatch samples around its await. The band closes in `finally` because a raising kernel is
    # exactly the dispatch whose resource price matters most: the weave's boundary fence one level up converts the
    # raise, so an exit fact fired only on the settled path would lose every expensive failure.
    domain = SCOPE_DOMAIN[scope]
    enter, resolve = _point(domain, "dispatched"), _point(domain, "resolved")

    def opened() -> tuple[Cost, float]:
        # enter fact first, then the baseline — tap execution never lands inside the band it precedes.
        fired(enter, EvidenceFired(scope=scope.value, subject=subject), composition)
        return Cost.own(), perf_counter()

    def closed(before: Cost, started: float) -> None:
        # `sys.exception()` is the active-exception read: it returns the propagating exception during a `finally`
        # unwind and None on the settled return, so the exit fact never claims a settle it did not observe.
        outcome: Outcome = "raised" if exception() is not None else "settled"
        fired(resolve, ResourceUsage.of(scope, subject, outcome, started, before), composition)

    if _awaiting(dispatch):

        async def sampled_async() -> T:
            before, started = opened()
            try:  # Exemption: the two-sided band close is the platform-forced statement kernel — no expression form observes an unwind.
                return await dispatch()
            finally:
                closed(before, started)

        return sampled_async

    def sampled() -> T:
        before, started = opened()
        try:  # Exemption: as above — the sync arm's band must survive the raise its caller's fence converts.
            return dispatch()
        finally:
            closed(before, started)

    return sampled
```

## [04]-[RESEARCH]

<!-- source-only: research row template; every landed row opens on the list dash this placeholder omits, the census reading `^- [TOKEN]-[STATUS]:` alone:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
