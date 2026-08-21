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
# --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
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

# --- [TYPES] --------------------------------------------------------------------------------

type Domain = Literal["analysis", "graduation", "jit", "numerics", "program", "solve", "study"]
type Measures = Mapping[str, float]
type Outcome = Literal["settled", "raised"]


class ComputePoint(StrEnum):
    # the folder's ONE hook-id roster: `runtime/observability/hooks#HOOKS` closes the id vocabulary AT the roster, so
    # a bare string constructs no row and a fire seam cannot re-spell an id its roster already names. The comprehension
    # that used to mint ids at registration time is INVERTED rather than lost — the roster is the source now and every
    # table below folds over it, so the `rasm.<pkg>.<domain>.<tail>` grammar the register fence proves is stated once
    # per point while `LIFECYCLE`, `COMPUTE_POINTS`, the staged resolution, and the tap fan all read the same members.
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
        # third id segment IS the point's domain under the grammar `register` proves, so the tap `kind`, the
        # lifecycle grouping, and the id itself read one source rather than a parallel row.
        return self.value.split(".")[2]

    @property
    def tail(self) -> str:
        return self.value.split(".")[3]


class WeaveStage(StrEnum):
    # the weave's OWN milestone beside every producer roster: the fold's extent publishes at the open, so a
    # subscriber reads the census before the first interior position and a fold that rails still reported that it
    # started. The interior belongs to the producer's closed roster and this vocabulary never grows into it.
    OPENED = "opened"

# --- [CONSTANTS] ------------------------------------------------------------------------

# roster segment every compute measure and every point id records under; the runtime DOMAINS row admits it, and the
# census map keys on the pair, so the tap argument and the id derivation read one spelling.
DOMAIN: Final[str] = "compute"

# the producing PACKAGE identity every receipt tap attributes its rows to, derived off that one segment so a rename
# cannot fork the owner label from the domain, and estate-rooted like every other receipt owner. The per-fact subject
# is the FIRED point's own and resolves inside the tap, so no call site here re-spells a point id as an owner.
PACKAGE: Final[str] = f"rasm.{DOMAIN}"

# derived column law: the Literal IS the point-id domain set, so point derivation, `SCOPE_DOMAIN` codomain, and tap kind
# attribute can never disagree. This roster stays spelled APART from the runtime `DOMAINS` capability roster the census
# gate reads: a hook-point id's third segment and a metric name's `<domain>` segment are two vocabularies Tier-0 carves
# apart, so one name over both silently tests the module-leaf roster where the capability roster belongs and every
# census pair falls out unmatched.
POINT_DOMAINS: Final[tuple[Domain, ...]] = get_args(Domain.__value__)

# the three tails every domain carries, named once so no fold spells a segment literal and the install census proves
# the triple by membership rather than by counting rows.
DISPATCHED: Final[str] = "dispatched"
RESOLVED: Final[str] = "resolved"
STAGED: Final[str] = "staged"
DOMAIN_TAILS: Final[frozenset[str]] = frozenset({DISPATCHED, RESOLVED, STAGED})

# retention depth of the ONE retaining row: a late-attaching diagnostic subscriber drains the recent refusals on
# attach, then observes forward. Depth rides the retaining arm alone, so no observe row spells a window it never reads.
REFUSAL_WINDOW: Final[int] = 64

# instrument spellings `_measures` records under — wire law spelled whole so a grep resolves the series, matching
# runtime `INSTRUMENTS` row style; a drifted key has no slot and no owner.
EVIDENCE_DURATION: Final[str] = "rasm.compute.evidence.duration"
EVIDENCE_CPU: Final[str] = "rasm.compute.evidence.cpu_time"
EVIDENCE_RSS: Final[str] = "rasm.compute.evidence.rss_delta"
GRADUATION_RESIDUALS: Final[str] = "rasm.compute.graduation.residual_count"

# shielded grace the band's crossing charge awaits under: a cancelled dispatch still burned the cpu it owes, so the
# charge survives the cancel, while an unbounded shield behind an intake nothing is draining converts a bounded
# teardown into a hang. One cell, so the whole package's close-out patience tunes here.
METER_GRACE: Final[float] = 1.0

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
    # handle, and no second honest-RSS rationale. Columns stay FLAT: the generic receipts tap row projects a
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
    # the refusal coordinate off the fault's OWN projection, never a case-indexed read: `_admit`'s refinement breach
    # classifies `api` and `_clear`'s ceiling rejection `boundary`, so the retired `fault.boundary[0]` pair access
    # raised on exactly the arm this replay ring exists to retain. `detail` carries the remaining `facts()` columns
    # whole, so a `domain` case's carried sibling token survives beside the five detail-bearing arms.
    subject: str
    tag: str
    detail: str


class ComputeInstall(Struct, frozen=True, gc=False):
    # composition-time proof of this owner's whole admission — point ids now deliverable in the caller's composition
    # beside the measure spellings the runtime census admits. One receipt answers what compute registered and what it
    # may record, where handing the caller the registry's own `HookPoint` rows leaks a `type[Struct]` field no receipt
    # projection renders and names the registry's product rather than this owner's.
    points: tuple[str, ...]
    measures: tuple[str, ...]


# --- [TABLES] -----------------------------------------------------------------------------


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

# one row per TAIL, keyed on the segment the roster already spells: the payload the point declares beside the
# delivery arm it registers under. Keying on the tail rather than on the id is what keeps a new point domain free —
# its three rows exist the moment its three members land — and `rejected` is the one retaining row, its depth riding
# the retaining arm alone rather than a flat column every observe row would spell and never read.
POINT_ROWS: Final[Map[str, tuple[type[Struct], Modality]]] = Map.of_seq([
    (DISPATCHED, (EvidenceFired, Modality(observe=None))),
    (RESOLVED, (ResourceUsage, Modality(observe=None))),
    (STAGED, (StageMark, Modality(observe=None))),
    (ComputePoint.ADMITTED.tail, (GraduationAdmitted, Modality(observe=None))),
    (ComputePoint.REJECTED.tail, (GraduationRejected, Modality(replay=REFUSAL_WINDOW))),
])

# the roster grouped on its own `<domain>.<tail>` grammar: `ledgered` and `stage_point` read a member out of this
# fold rather than reconstructing an id from a format string, so the reverse value lookup the `EvidenceScope`
# charter forecloses never lands here either and a root rename cannot strand a resolution.
LIFECYCLE: Final[Map[str, Map[str, ComputePoint]]] = Block.of_seq(ComputePoint).fold(
    lambda held, point: held.add(point.domain, held.try_find(point.domain).default_value(Map.empty()).add(point.tail, point)),
    Map.empty(),
)

COMPUTE_POINTS: Final[Block[HookPoint[Struct]]] = Block.of_seq(ComputePoint).map(
    lambda point: HookPoint(id=point, payload=POINT_ROWS[point.tail][0], modality=POINT_ROWS[point.tail][1])
)

# this page's raise-side anchor under the hub `ComputeLeg` roster: the install gate accumulates its whole breach set
# into ONE refusal, so the coordinate a caller repairs is the breach roster and it rides the row's declared slot
# rather than a subject string spelled at the raise.
INSTALL_CENSUS: Final[FaultRow[ComputeLeg]] = FaultRow(
    leg=ComputeLeg.OBSERVABILITY, point="install", arm="config", defect="census-breach", retriability=TERMINAL, slots=("breaches",)
)
RAISES: Final[Block[FaultRow[ComputeLeg]]] = rostered(Block.of_seq([INSTALL_CENSUS]))

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


def tapped(composition: ScopeKey = DEFAULT_SCOPE) -> RuntimeRail[Block[Attachment]]:
    # app leg: one receipts tap and one metrics tap per point, so metric, receipt, and log lines project from the
    # same fired fact; the id's domain segment rides `kind`, landing under the runtime `rasm.kind` attribute so the
    # backend discriminates domains on one instrument family. Both taps subscribe into the caller's composition AND
    # record under it — one `scope` reaches both halves through the row, so a fired fact's two evidence planes can
    # never partition across compositions — and a tap on a point this composition never registered refuses on the
    # runtime subscribe fence. The answer is the DETACHER block, never a count: a composition root brackets what it
    # attached and releases through the `Attachment` values, where a count retires nothing.
    # the receipts row is point-INDEPENDENT — it takes the package identity and the delivery site supplies each row's
    # subject off the FIRED point — so ONE subscribe fans it across the whole claimed roster and the built-in binds
    # once. The metrics row stays per point, since `kind` is that point's own domain segment.
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
    # the hub's `StageTap.of` resolves a producer's staged point off its own scope, so a long fold names the
    # milestone roster it owns and never an id. The read is a member lookup through the roster's own grouping —
    # never a `ComputePoint(f"...{domain}.staged")` value reconstruction, which the `EvidenceScope` charter
    # forecloses for the same reason: a spelling the enum already owns re-breaks on the next root change.
    return LIFECYCLE[SCOPE_DOMAIN[scope]][STAGED]


def fired(point_id: ComputePoint, payload: Struct, composition: ScopeKey = DEFAULT_SCOPE) -> None:
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
    scope: EvidenceScope, subject: str, dispatch: Callable[[], T] | Callable[[], Awaitable[T]], *, composition: ScopeKey = DEFAULT_SCOPE,
    stage: Option[StageTap] = Nothing,
) -> Callable[[], T] | Callable[[], Awaitable[T]]:
    # hub weave leg: enter fact, band, exit fact around the one dispatch — `_awaiting` mirrors the runtime `measured`
    # split so an async dispatch samples around its await. The band closes in `finally` because a raising kernel is
    # exactly the dispatch whose resource price matters most: the weave's boundary fence one level up converts the
    # raise, so an exit fact fired only on the settled path would lose every expensive failure. `stage` is the
    # OPTIONAL mid-operation slot: the weave opens the pulse stream so a subscriber reads the fold's extent before
    # its first interior position, and the interior positions stay the producer's own closed roster.
    domain = SCOPE_DOMAIN[scope]
    enter, resolve = LIFECYCLE[domain][DISPATCHED], LIFECYCLE[domain][RESOLVED]

    def opened() -> tuple[Cost, float]:
        # enter fact first, then the baseline — tap execution never lands inside the band it precedes.
        fired(enter, EvidenceFired(scope=scope.value, subject=subject), composition)
        stage.map(lambda mark: mark.beat(WeaveStage.OPENED, 0))
        return Cost.own(), perf_counter()

    def closed(before: Cost, started: float) -> ResourceUsage:
        # `sys.exception()` is the active-exception read: it returns the propagating exception during a `finally`
        # unwind and None on the settled return, so the exit fact never claims a settle it did not observe. The band
        # RETURNS so the async arm charges the burn it already sampled — a second bracket there prices a different one.
        outcome: Outcome = "raised" if exception() is not None else "settled"
        band = ResourceUsage.of(scope, subject, outcome, started, before)
        fired(resolve, band, composition)
        return band

    async def charged(band: ResourceUsage) -> None:
        # the package's ONE `Resource.COMPUTE` seat: every producer dispatch rides this weave, so the burn charges
        # here once and a second COMPUTE fact at any producer bills it twice. Quantity converts the band's UCUM
        # second back to whole milliseconds and clamps at zero, because `Quantity` admits `int >= 0` and a delta a
        # sampling boundary reads below it refuses the whole fact at decode; the surface is the point domain, the
        # same discriminant the taps already ride, so charge and series cut on one axis. The scope SHIELDS under one
        # bounded grace — a cancelled dispatch still burned the cpu it owes, while an unbounded shield behind an
        # unrelieved intake converts a bounded teardown into a hang — and the record's rail is DISCARDED because
        # this runs inside `finally`, where binding it would replace the kernel fault the band exists to price. An
        # unjournalled composition folds to the lawful no-op and pays one map read.
        with anyio.move_on_after(METER_GRACE, shield=True):
            await Journal.record(
                MeterFact(resource=Resource.COMPUTE, quantity=max(int(band.cpu_s * 1000.0), 0), surface=domain), scope=composition
            )

    if _awaiting(dispatch):

        async def sampled_async() -> T:
            before, started = opened()
            try:  # Exemption: the two-sided band close is the platform-forced statement kernel — no expression form observes an unwind.
                return await dispatch()
            finally:
                # the async arm alone reaches the charge: recording suspends, so the sync arm below bands, fires,
                # and records nothing, its dispatch's cost staying the exit fact and its taps.
                await charged(closed(before, started))

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

<!-- source-only: research row template; every landed row opens on the list dash this placeholder omits, the census reading `^- [TOKEN]-[OPEN|BLOCKED]:` alone:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
