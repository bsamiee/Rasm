# [PY_COMPUTE_OBSERVABILITY]

Observability owner of the compute plane: the hook rail registers compute's derived point vocabulary on the runtime `Hooks` registry, and the resource ledger samples every measured kernel's true cost — both woven through the hub `evidence_run` binding, so every producer composing the hub gains enter/exit facts, UCUM metric rows, receipt lines, and the CPU-and-RSS band with zero page-local emit calls. Domain code fires facts and observability subscribes — the telemetry-as-tap law — so metric, receipt, and log lines project from the same fired fact and cannot disagree.

Composition splits by altitude: the library leg `registered()` registers points at composition, the app leg `tapped()` attaches the built-in taps, and firing is hub-owned — `ledgered` wraps every producer dispatch inside the weave and the graduation rail fires its admission facts on both arms. Tenant attribution joins at the backend through the `rasm.tenant` baggage fold on `Metrics.record`; this page imports no OTel symbol and constructs no SDK surface.

## [01]-[INDEX]

- [01]-[HOOK_RAIL]: the derived `COMPUTE_POINTS` table over the `SCOPE_DOMAIN` correspondence, the closed payload family, the measure-mapping rows, and the registration/tap split.
- [02]-[RESOURCE_LEDGER]: the `ResourceUsage` band, the two-block `oneshot` bracket, and the `ledgered` hub weave every producer rides.

## [02]-[HOOK_RAIL]

- Owner: `COMPUTE_POINTS` — one derived point table, never hand-enumerated rows: the `Domain` literal is the domain set, `SCOPE_DOMAIN` maps every `EvidenceScope` member onto one domain totally, and the lifecycle pair (`dispatched` enter fact, `resolved` resource band) derives per domain by comprehension, so a new module leaf is one `SCOPE_DOMAIN` row whose lifecycle points already exist. Ids spell `rasm.compute.<domain>.<point>` under the runtime `observability/hooks#HOOKS` grammar, and a colliding registration refuses structurally on the register fence — two apps composed on this library never fight over points.
- Cases: every `Domain` member carries its derived pair; graduation carries two admission facts beside its pair — `rasm.compute.graduation.admitted` as an `OBSERVE` fact off the cleared receipt, `rasm.compute.graduation.rejected` on a `REPLAY` ring (`buffer=64`) so a late-attaching diagnostic subscriber drains the recent refusals on attach, then observes forward. A `VETO` admission point lands as one more row when an app claims the policy; `Modality` already carries the arm.
- Entry: `registered()` is the library leg — the `SCOPE_DOMAIN` totality proof, then one `traverse` over the table aborting on the first refused row, so an `EvidenceScope` member without a domain row refuses at composition, never as a dispatch-time `KeyError`; `tapped()` is the app leg — per point one `tap_receipts` and one `tap_metrics` subscription, the id's domain segment riding the tap's `kind` slot and landing as the `compute=<domain>` metric attribute, so the backend discriminates domains on one instrument family. Producers hold no fire call: the hub weave fires the lifecycle points and the graduation rail its admission facts, so every producer dispatch reports itself.
- Measures: `_measures` is the one polymorphic projection every metrics tap shares — keys spell the runtime instrument rows exactly, so `Metrics.record`'s `_DOMAIN_SLOT` resolution is a lookup, never a rename; a payload without numeric measures projects `{}` and contributes receipts alone.

| [INDEX] | [MEASURE]                                | [UNIT]       | [KIND]    | [SOURCE]                            |
| :-----: | :--------------------------------------- | :----------- | :-------- | :---------------------------------- |
|  [01]   | `rasm.compute.evidence.duration`         | `ms`         | histogram | `ResourceUsage.wall_s` scaled to ms |
|  [02]   | `rasm.compute.evidence.cpu_time`         | `s`          | histogram | `user_s + system_s` delta           |
|  [03]   | `rasm.compute.evidence.rss_growth`       | `By`         | histogram | `rss_after - rss_before`, signed    |
|  [04]   | `rasm.compute.graduation.residual_count` | `{residual}` | histogram | `GraduationAdmitted.residual_count` |

- Growth: a new module leaf is one `SCOPE_DOMAIN` row; a new domain is one `Domain` literal member whose pair derives; a new measure is one instrument-name constant, one `_measures` arm, and one runtime `INSTRUMENTS` row; a new admission fact is one point row plus one fire arm on the owning rail.
- Boundary: the runtime owns the registry, the taps, and the instrument table — the `rasm.compute.*` rows above register on `runtime/observability/metrics#METRIC` `INSTRUMENTS` under `domain="compute"` beside their `SyncInstruments` fields, a runtime ripple, never a compute-side mint. Libraries register points only; subscriber attachment stays at app composition, and no second egress rides beside the taps.

```python signature
# --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
from collections.abc import Awaitable, Callable, Mapping
from inspect import iscoroutinefunction
from time import perf_counter
from typing import Final, Literal, get_args

import psutil
from expression import Error, Ok
from expression.collections import Block, Map
from expression.extra.result import traverse
from msgspec import Struct

from rasm.compute.graduation.handoff import EvidenceScope
from rasm.runtime.faults import BoundaryFault, RuntimeRail
from rasm.runtime.hooks import HookPoint, Hooks, Modality
from rasm.runtime.receipts import OPEN, Receipt, Signals

# --- [TYPES] --------------------------------------------------------------------------------

type Domain = Literal["analysis", "graduation", "jit", "numerics", "program", "solve", "study"]
type Measures = Mapping[str, float]

# --- [CONSTANTS] ------------------------------------------------------------------------

# derived column law: the Literal IS the domain set, so the point derivation, the SCOPE_DOMAIN
# codomain, and the tap kind attribute can never disagree.
DOMAINS: Final[tuple[Domain, ...]] = get_args(Domain.__value__)

# graduation's two admission facts beside the derived lifecycle pairs; ids obey the runtime
# `rasm.<pkg>.<domain>.<point>` grammar the register fence proves.
ADMITTED: Final[str] = "rasm.compute.graduation.admitted"
REJECTED: Final[str] = "rasm.compute.graduation.rejected"

# instrument spellings `_measures` records under — the exact `rasm.compute.*` rows the runtime
# INSTRUMENTS table registers under domain="compute"; a drifted key has no slot and no owner.
EVIDENCE_DURATION: Final[str] = "rasm.compute.evidence.duration"
EVIDENCE_CPU: Final[str] = "rasm.compute.evidence.cpu_time"
EVIDENCE_RSS: Final[str] = "rasm.compute.evidence.rss_growth"
GRADUATION_RESIDUALS: Final[str] = "rasm.compute.graduation.residual_count"

# own-process handle minted once; the runtime receipts and metrics owners each hold theirs.
_PROCESS: Final[psutil.Process] = psutil.Process()

# --- [MODELS] ---------------------------------------------------------------------------


class EvidenceFired(Struct, frozen=True, gc=False):
    # enter fact of every hub dispatch: scope names the producer, subject its call-time discriminant.
    scope: str
    subject: str


class ResourceUsage(Struct, frozen=True, gc=False):
    # exit band of every hub dispatch: two-block `oneshot` CPU deltas beside wallclock; RSS crosses
    # as before/after samples — macOS `pmem(rss, vms, pfaults, pageins)` carries no peak field, so a
    # peak claim has no cross-platform source and the honest band is the pair.
    scope: str
    subject: str
    user_s: float
    system_s: float
    wall_s: float
    rss_before: int
    rss_after: int

    @property
    def cpu_s(self) -> float:
        return self.user_s + self.system_s

    @property
    def rss_growth(self) -> int:
        return self.rss_after - self.rss_before


class GraduationAdmitted(Struct, frozen=True, gc=False):
    axis: str
    subject: str
    evidence_key: str
    residual_count: int


class GraduationRejected(Struct, frozen=True, gc=False):
    boundary: str
    reason: str


# --- [TABLES] -----------------------------------------------------------------------------


def _point(domain: Domain, tail: str) -> str:
    return f"rasm.compute.{domain}.{tail}"


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
    *(HookPoint(id=_point(domain, tail), payload=payload, modality=Modality.OBSERVE) for domain in DOMAINS for tail, payload in _LIFECYCLE),
    HookPoint(id=ADMITTED, payload=GraduationAdmitted, modality=Modality.OBSERVE),
    HookPoint(id=REJECTED, payload=GraduationRejected, modality=Modality.REPLAY, buffer=64),
])

# --- [OPERATIONS] -----------------------------------------------------------------------


def _measures(payload: Struct) -> Measures:
    # one polymorphic projection every metrics tap shares: keys spell the instrument rows exactly,
    # so `Metrics.record`'s `_DOMAIN_SLOT` resolution is a lookup, never a rename; the band's own
    # `cpu_s`/`rss_growth` projections stay the one derivation site, and the enter fact and the
    # rejection carry no numeric measure and contribute receipts alone.
    match payload:
        case ResourceUsage() as band:
            return {EVIDENCE_DURATION: band.wall_s * 1000.0, EVIDENCE_CPU: band.cpu_s, EVIDENCE_RSS: float(band.rss_growth)}
        case GraduationAdmitted(residual_count=count):
            return {GRADUATION_RESIDUALS: float(count)}
        case _:
            return {}


def registered() -> RuntimeRail[Block[HookPoint[Struct]]]:
    # library leg: the SCOPE_DOMAIN totality proof rides the same composition gate — an EvidenceScope
    # member without a domain row refuses before any point registers — then every compute point
    # registers once; a duplicate or malformed id refuses on the runtime register fence and the
    # traverse aborts on the first refusal.
    unmapped = Block.of_seq(scope for scope in EvidenceScope if SCOPE_DOMAIN.try_find(scope).is_none())
    return (
        traverse(Hooks.register, COMPUTE_POINTS)
        if unmapped.is_empty()
        else Error(BoundaryFault(boundary=("graduation.observability", f"unmapped-scope:{unmapped.head().value}")))
    )


def tapped() -> RuntimeRail[int]:
    # app leg: one receipts tap and one metrics tap per point, so metric, receipt, and log lines
    # project from the same fired fact; the id's domain segment rides `kind`, landing as the
    # `compute=<domain>` attribute, and the count terminal names how many taps attached.
    def attach(count: RuntimeRail[int], point: HookPoint[Struct]) -> RuntimeRail[int]:
        domain = point.id.split(".")[2]
        return count.bind(
            lambda n: Hooks.subscribe(point.id, Hooks.tap_receipts(point.id)).bind(
                lambda _one: Hooks.subscribe(point.id, Hooks.tap_metrics(_measures, domain="compute", kind=domain)).map(lambda _two: n + 2)
            )
        )

    return COMPUTE_POINTS.fold(attach, Ok(0))
```

## [03]-[RESOURCE_LEDGER]

- Owner: `ResourceUsage` — user and system CPU seconds, wallclock, and the before/after RSS pair sampled around each measured kernel; the band is a hook payload, so it reaches metrics, receipts, and log lines through the taps rather than a second emit surface, and the backend joins solve cost to the `rasm.tenant` baggage the runtime promotes.
- Entry: `ledgered` is the hub weave leg `evidence_run` composes around every producer dispatch — enter fact, two-block band, exit fact — so every module-level measured kernel contributes the band with zero per-solver code; `resource_sampled` is the standalone fold for a kernel outside the weave, returning the value beside its band.
- Auto: two `oneshot` blocks bracket the kernel because one block CACHES its reads — a single block can never observe a delta — and each block collapses its `cpu_times`/`memory_info` pair to one syscall collection; deltas ride `cpu_times`, never `cpu_percent`, because attribution needs exact seconds, not a since-last-call ratio shared across readers. A refused fire — pre-registration, unregistered id — lands as a rejected receipt through `fired`, never a silent drop.
- Growth: a new band field is one `ResourceUsage` field, one `_measures` key, and one runtime instrument row; a new sampling site is nothing — the weave already covers every producer.
- Boundary: the fold reads the worker's own process handle inside the lane; cross-process aggregation stays the runtime lanes owner's. A sync-declared dispatch minting an awaitable bands the mint alone — the settle rides the runtime `measured` continuation — so an async kernel declares the async modality to band its await.

```python signature
# --- [OPERATIONS] -----------------------------------------------------------------------


def fired(point_id: str, payload: Struct) -> None:
    # tee fire: the emitter's value is untouched by construction, and a refused fire folds onto the
    # receipt stream as evidence rather than dropping.
    Hooks.fire(point_id, payload).swap().map(lambda fault: Signals.emit(Receipt.of(point_id, fault), OPEN))


def _opened() -> tuple[float, float, int, float]:
    # Exemption: the measurement bracket is the platform-forced statement kernel — baseline block.
    with _PROCESS.oneshot():
        cpu, rss = _PROCESS.cpu_times(), _PROCESS.memory_info().rss
    return cpu.user, cpu.system, rss, perf_counter()


def _closed(scope: EvidenceScope, subject: str, baseline: tuple[float, float, int, float]) -> ResourceUsage:
    # Exemption: closing block of the bracket — the second `oneshot` reads fresh values the first
    # block's cache cannot supply, and the deltas fold into the one band.
    user_before, system_before, rss_before, started = baseline
    with _PROCESS.oneshot():
        cpu, rss = _PROCESS.cpu_times(), _PROCESS.memory_info().rss
    return ResourceUsage(
        scope=scope.value,
        subject=subject,
        user_s=cpu.user - user_before,
        system_s=cpu.system - system_before,
        wall_s=perf_counter() - started,
        rss_before=rss_before,
        rss_after=rss,
    )


def resource_sampled[T](scope: EvidenceScope, subject: str, kernel: Callable[[], T]) -> tuple[T, ResourceUsage]:
    baseline = _opened()
    value = kernel()
    return value, _closed(scope, subject, baseline)


def ledgered[T](scope: EvidenceScope, subject: str, dispatch: Callable[[], T] | Callable[[], Awaitable[T]]) -> Callable[[], T] | Callable[[], Awaitable[T]]:
    # hub weave leg: enter fact, band, exit fact around the one dispatch — the modality probe
    # mirrors the runtime `measured` split so an async dispatch samples around its await.
    domain = SCOPE_DOMAIN[scope]

    if iscoroutinefunction(dispatch):

        async def sampled_async() -> T:
            fired(_point(domain, "dispatched"), EvidenceFired(scope=scope.value, subject=subject))
            baseline = _opened()
            value = await dispatch()
            fired(_point(domain, "resolved"), _closed(scope, subject, baseline))
            return value

        return sampled_async

    def sampled() -> T:
        fired(_point(domain, "dispatched"), EvidenceFired(scope=scope.value, subject=subject))
        value, usage = resource_sampled(scope, subject, dispatch)
        fired(_point(domain, "resolved"), usage)
        return value

    return sampled
```

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
