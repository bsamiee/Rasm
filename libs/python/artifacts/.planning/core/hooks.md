# [PY_ARTIFACTS_HOOKS]

`ArtifactHook` is the artifact plane's production-fact point table — registered rows on the runtime `Hooks` registry, each a `rasm.artifacts.<domain>.<point>` id bound to one closed msgspec payload and one modality, so an app vetoes an issue pre-drain, audits every emitted receipt, and replays the last drain facts without touching a producer page. Telemetry is a tap: observability subscribes through the runtime `tap_receipts`/`tap_metrics` taps, no producer page emits a hook or opens a span, and every payload projects FROM the receipt, fault, or drain evidence already in hand.

`Production` is the composed entry over the runtime registry: `registered` lands `ARTIFACT_POINTS` through `Hooks.register` under a scope-keyed one-shot cell and deposits its `ArtifactInstall` receipt on that registry's install ledger, so the support-bundle capsule reads this plane's admission where an absent row is its stated diagnosis. `fired` rides the cell so every seam self-registers before `Hooks.fire` and a hook-free app's `VETO` gate passes clean, and `subscribed` is the app-root attach for the runtime taps.

Every arm threads the runtime `ScopeKey` composition axis under a `DEFAULT_SCOPE` keyword default, so two compositions embedding artifacts in one process partition points, subscribers, and replay rings structurally. `ISSUE_BAGGAGE` is the issue-scope correlation key every payload carries as `scope`; fire seams live at their emitting owners — the issue rails, the receipt `contribute` fold, and the transmittal close.

## [01]-[INDEX]

- [02]-[POINTS]: `ArtifactHook` id vocabulary, its payload projections, the `ARTIFACT_POINTS` row table, the `scoped` baggage read, and the latched `Production` register/fire/subscribe surface.

## [02]-[POINTS]

- Owner: `ArtifactHook` is the closed `StrEnum` id vocabulary — every member spells the runtime `HOOK_ID` grammar `rasm.artifacts.<domain>.<point>`, so a point id travels as a symbol, never a string literal a fire seam re-spells. `ARTIFACT_POINTS` is the one row table binding each id to its payload `Struct` and `Modality`; the table is the single growth site and no second registry, subscriber map, or ring exists at artifacts grain — the runtime `Hooks` registry owns registration, subscription, isolation, and replay.
- Cases: modality is capability, never preference — `ISSUE_ADMITTED` is the one `VETO` row because pre-drain rejection is the point's declared contract (a veto subscriber's `Error` rides the issue rail as its `BoundaryFault` refusal, and the payload projection means a veto rejects, never rewrites, the staged work); `FRONT_DRAINED` is the one `REPLAY` row, its ring bound sized to the deepest CPM front chain a sheet-set issue drains so a late subscriber reads the whole last drain; every other row is `OBSERVE` — a fenced tap whose fault lands on the receipt stream while the emitter's value passes untouched.
- Law: each payload is a closed `Struct(frozen=True, gc=False)` of native scalars projected from the evidence in hand — `IssueAdmitted` from the staged node set, `IssuePlanned` from the cleared plan, `IssueRefused` from the terminal `BoundaryFault.tag`, `FrontDrained` from the runtime `DrainReceipt` columns, `ReceiptEmitted` from the `ArtifactReceipt` case scalars at the `contribute` fold, `TransmittalIssued` from the settled `TransmittalEvidence` at the transmittal close — and every payload carries `scope`, the `ISSUE_BAGGAGE` correlation id, so a subscriber slices facts per issue with no join against a second stream. Fields a payload cannot project from landed evidence stay off the point.
- Law: `TransmittalIssued` is the plane's one ANNOUNCED fact and its width is the announcement's, not the receipt's — an issue crosses to a downstream system as attributes and a routed payload, so every scalar an ingesting system routes on rides the point and the delivery projection at `delivery/notice#NOTICE` invents none. Its identity fields carry the runtime `ContentKey` renders rather than the values, because a tap projects each payload onto a receipt through `structs.asdict` and a nested value object reaches that fact stream as an object no reader renders.
- Entry: `scoped(context)` reads the `ISSUE_BAGGAGE` baggage entry off an explicit context — the empty string when no issue scope is live, so a receipt contributed outside an issue drain still fires lawfully. `Production.registered` claims the WHOLE table through one `Hooks.register` roster call — the registry's gated transition swaps only past its last admitted row and reports every collision together, so a refusal leaves this plane's custody untouched instead of half-mounted — then deposits one `ArtifactInstall` naming the landed ids through `Hooks.installed` under `OWNER` — the deposit passes its receipt through, so the install IS the rail's terminal and the cached one-shot holds it; its locked `_wired` map returns that prior rail on same-scope re-entry — the scope-keyed one-shot mirrors the runtime registry's own `ScopeKey` partition and stays singular under free-threaded concurrent first use; `Production.fired` composes the cell before `Hooks.fire`, so the first fire from any seam self-registers, a `VETO` rail returns to the emitter, and an `OBSERVE` rail is fire-and-forget by modality contract; `Production.subscribed` composes the same cell before `Hooks.subscribe`. Subscribers stay app-root — `Production.subscribed(point, Hooks.tap_receipts(...))` or a domain tap — and this page registers points alone.
- Packages: `msgspec` (`Struct` payload rows and `to_builtins(payload, str_keys=True)` the direct telemetry projection), `expression` (`Block` the row table, `Option`/`Map` the one-shot cell), `opentelemetry-api` (`baggage.get_baggage` the scope read), runtime (`HookPoint`/`Hooks`/`Modality`/`Tap`/`Veto`, `Hooks.register`'s roster arm and `Hooks.installed` the producer-install ledger, `RuntimeRail`, `ScopeKey`/`DEFAULT_SCOPE` the composition axis).
- Growth: a new production fact is one `ArtifactHook` member, one payload `Struct`, and one `ARTIFACT_POINTS` row — the fire seam lands at the owner that holds the evidence, registration, isolation, and taps follow with zero edits here, and the install receipt widens by derivation because it names the landed ids rather than a hand-kept list; a new admission fact this plane can prove at composition is one `ArtifactInstall` field of native scalars; a new payload field is one `Struct` field every tap projects through `msgspec.to_builtins(payload, str_keys=True)`; a new consumer is one `Production.subscribed` call at the app root with zero artifacts edit; a wider replay window is the `FRONT_DRAINED` row's `buffer` value; a second composition is one `ScopeKey` value threaded through `scope` with every bare call untouched.
- Boundary: this page imports no artifacts sibling — payloads carry native scalars so the floor stays acyclic under `core/receipt` and `core/issue` composing it downward; the fire seams, the veto consequence, and the baggage bind are the emitting owners'; exporter, provider, and transport wiring stay the runtime telemetry owner's; and a hook payload never re-narrates a receipt — the settled receipt and its evidence remain the one truth the fired fact projects. Artifacts taps are synchronous by charter — payloads are small scalar facts the built-in taps project onto receipts and metrics in-line; an async tap rides the runtime `fire_async` surface an app wires itself, and no fire opens a span, since a fire runs under whatever span is active.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from datetime import datetime
from enum import StrEnum
from threading import RLock
from typing import ClassVar, Final

from expression import Option
from expression.collections import Block, Map
from msgspec import Struct
from opentelemetry import baggage
from opentelemetry import context as otel_context

from rasm.runtime.faults import RuntimeRail
from rasm.runtime.hooks import HookPoint, Hooks, Modality, Tap, Veto
from rasm.runtime.receipts import DEFAULT_SCOPE, ScopeKey

# --- [TYPES] ----------------------------------------------------------------------------


class ArtifactHook(StrEnum):
    ISSUE_ADMITTED = "rasm.artifacts.issue.admitted"
    ISSUE_PLANNED = "rasm.artifacts.issue.planned"
    ISSUE_REFUSED = "rasm.artifacts.issue.refused"
    FRONT_DRAINED = "rasm.artifacts.issue.drained"
    RECEIPT_EMITTED = "rasm.artifacts.receipt.emitted"
    TRANSMITTAL_ISSUED = "rasm.artifacts.delivery.issued"


# --- [CONSTANTS] ------------------------------------------------------------------------

# issue-scope correlation key: core/issue binds it as baggage + log key, every payload carries it as `scope`.
ISSUE_BAGGAGE: Final[str] = "rasm.artifacts.issue"

# This plane deposits its install receipt under this ledger key; a capsule reads an ABSENT row as the diagnosis
# that this leg never ran, so one constant carries the name rather than a literal re-spelled at the deposit.
OWNER: Final[str] = "artifacts.production"

# --- [MODELS] ---------------------------------------------------------------------------


class ArtifactInstall(Struct, frozen=True, gc=False):
    # composition-time proof this plane's WHOLE point roster landed in the caller's composition — the ids now
    # deliverable there, flat native scalars alone so the support-bundle capsule renders the row through
    # `structs.asdict` with no nested mapping to breach its depth-walking redaction. Handing the registry's own
    # `HookPoint` rows back instead leaks a `type[Struct]` field no receipt projection renders and names the
    # registry's product rather than this owner's admission.
    points: tuple[str, ...]


class IssueAdmitted(Struct, frozen=True, gc=False):
    modality: str
    works: int
    targets: int
    scope: str


class IssuePlanned(Struct, frozen=True, gc=False):
    works: int
    fronts: int
    targets: int
    scope: str


class IssueRefused(Struct, frozen=True, gc=False):
    cause: str
    scope: str


class FrontDrained(Struct, frozen=True, gc=False):
    front: int
    accepted: int
    completed: int
    cancelled: int
    rejected: int
    hit: int
    scope: str


class ReceiptEmitted(Struct, frozen=True, gc=False):
    kind: str
    key: str
    scope: str


class TransmittalIssued(Struct, frozen=True, gc=False):
    # ANNOUNCED fact: width is the announcement's, so every scalar a downstream system routes on rides here and
    # its wire projection invents none. `key` renders the pre-run aggregate — operation identity, since the reuse
    # fabric elides two runs over identical inputs onto one — `register` names the issued index a consumer
    # resolves each row from, and `occurred` stamps the settled close instant aware by construction.
    key: str
    register: str
    container: str
    transmittal_id: str
    issuing_party: str
    purpose: str
    revision: str
    revision_ordinal: int
    confidentiality: str
    issued_at: str
    occurred: datetime
    sheets: int
    lineage: int
    suitability: str
    pades_level: str
    validation_state: str
    record_state: str
    scope: str


# --- [TABLES] ---------------------------------------------------------------------------

ARTIFACT_POINTS: Final[Block[HookPoint[Struct]]] = Block.of_seq([
    HookPoint(ArtifactHook.ISSUE_ADMITTED, IssueAdmitted, Modality.VETO),
    HookPoint(ArtifactHook.ISSUE_PLANNED, IssuePlanned, Modality.OBSERVE),
    HookPoint(ArtifactHook.ISSUE_REFUSED, IssueRefused, Modality.OBSERVE),
    HookPoint(ArtifactHook.FRONT_DRAINED, FrontDrained, Modality.REPLAY, buffer=8),
    HookPoint(ArtifactHook.RECEIPT_EMITTED, ReceiptEmitted, Modality.OBSERVE),
    HookPoint(ArtifactHook.TRANSMITTAL_ISSUED, TransmittalIssued, Modality.OBSERVE),
])

# --- [OPERATIONS] -----------------------------------------------------------------------


def scoped(context: otel_context.Context, /) -> str:
    entry = baggage.get_baggage(ISSUE_BAGGAGE, context)
    return entry if isinstance(entry, str) else ""


# --- [SERVICES] -------------------------------------------------------------------------


class Production:
    # Locked scope-keyed one-shot: each composition registers once under concurrent first use.
    _lock: ClassVar[RLock] = RLock()
    _wired: ClassVar[Map[ScopeKey, RuntimeRail[ArtifactInstall]]] = Map.empty()

    @classmethod
    def registered(cls, *, scope: ScopeKey = DEFAULT_SCOPE) -> RuntimeRail[ArtifactInstall]:
        # This mint deposits its OWN receipt, the same leg its two peer producer folders run: runtime imports no
        # producer, so the scoped `Hooks` ledger is where this plane's admission reaches the support-bundle capsule
        # at all, and a captured archive missing this row names a leg that never ran rather than leaving every
        # unregistered-id refusal unexplained. Depositing passes the receipt through, so it IS the rail's terminal.
        with cls._lock:
            match cls._wired.try_find(scope):
                case Option(tag="some", some=prior):
                    return prior
                case _:
                    # ONE roster claim: the registry's whole-set arm swaps the point table only past its last
                    # admitted row and reports every breach together, so a refused claim leaves custody exactly as
                    # it stood. A per-point traverse bought the same accumulating diagnosis by surrendering that
                    # atomicity — it mounts each prior row before the breach and owes a retire verb this plane
                    # cannot spell, and the install receipt below would then name a roster only partly standing.
                    rail = Hooks.register(ARTIFACT_POINTS, scope=scope).map(
                        lambda points: Hooks.installed(OWNER, ArtifactInstall(points=tuple(point.id for point in points)), scope=scope)
                    )
                    cls._wired = cls._wired.add(scope, rail)
                    return rail

    @classmethod
    def fired[P: Struct](cls, point: ArtifactHook, payload: P, *, scope: ScopeKey = DEFAULT_SCOPE) -> RuntimeRail[P]:
        return cls.registered(scope=scope).bind(lambda _install: Hooks.fire(point, payload, scope=scope))

    @classmethod
    def subscribed[P: Struct](cls, point: ArtifactHook, tap: Tap[P] | Veto[P], *, scope: ScopeKey = DEFAULT_SCOPE) -> RuntimeRail[int]:
        return cls.registered(scope=scope).bind(lambda _install: Hooks.subscribe(point, tap, scope=scope))


# --- [EXPORTS] ----------------------------------------------------------------------------

__all__ = (
    "ARTIFACT_POINTS",
    "ISSUE_BAGGAGE",
    "OWNER",
    "ArtifactHook",
    "ArtifactInstall",
    "FrontDrained",
    "IssueAdmitted",
    "IssuePlanned",
    "IssueRefused",
    "Production",
    "ReceiptEmitted",
    "TransmittalIssued",
    "scoped",
)
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
