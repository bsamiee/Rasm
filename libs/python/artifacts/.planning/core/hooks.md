# [PY_ARTIFACTS_HOOKS]

`ArtifactHook` is the artifact plane's production-fact point table — registered rows on the runtime `Hooks` registry, each a `rasm.artifacts.<domain>.<point>` id bound to one closed msgspec payload and one modality, so an app vetoes an issue pre-drain, audits every emitted receipt, and replays the last drain facts without touching a producer page. Telemetry is a tap: observability subscribes through the runtime `tap_receipts`/`tap_metrics` taps, no producer page emits a hook or opens a span, and every payload projects FROM the receipt, fault, or drain evidence already in hand.

`Production` is the composed entry over the runtime registry — `registered` lands `ARTIFACT_POINTS` through `Hooks.register` under a scope-keyed one-shot cell, `fired` rides that cell so every seam self-registers before `Hooks.fire` and a hook-free app's `VETO` gate passes clean, and `subscribed` is the app-root attach for the runtime taps. Every arm threads the runtime `ScopeKey` composition axis with the `DEFAULT_SCOPE` keyword default preserving the bare call shape, so two compositions embedding artifacts in one process partition points, subscribers, and replay rings structurally — per-app composition means a scope value, never a second registry. `ISSUE_BAGGAGE` is the issue-scope correlation key every payload carries as `scope`; the fire seams live at their emitting owners — the issue rails, the receipt `contribute` fold, and the delivery notice seal — never here.

## [01]-[INDEX]

- [01]-[POINTS]: the `ArtifactHook` id vocabulary, the seven payload projections, the `ARTIFACT_POINTS` row table, the `scoped` baggage read, and the latched `Production` register/fire/subscribe surface.

## [02]-[POINTS]

- Owner: `ArtifactHook` is the closed `StrEnum` id vocabulary — every member spells the runtime `HOOK_ID` grammar `rasm.artifacts.<domain>.<point>`, so a point id travels as a symbol, never a string literal a fire seam re-spells. `ARTIFACT_POINTS` is the one row table binding each id to its payload `Struct` and `Modality`; the table is the single growth site and no second registry, subscriber map, or ring exists at artifacts grain — the runtime `Hooks` registry owns registration, subscription, isolation, and replay.
- Cases: modality is capability, never preference — `ISSUE_ADMITTED` is the one `VETO` row because pre-drain rejection is the point's declared contract (a veto subscriber's `Error` rides the issue rail as its `BoundaryFault` refusal, and the payload projection means a veto rejects, never rewrites, the staged work); `FRONT_DRAINED` is the one `REPLAY` row, its ring bound sized to the deepest CPM front chain a sheet-set issue drains so a late subscriber reads the whole last drain; every other row is `OBSERVE` — a fenced tap whose fault lands on the receipt stream while the emitter's value passes untouched.
- Payloads: each payload is a closed `Struct(frozen=True, gc=False)` of native scalars projected from the evidence in hand — `IssueAdmitted` from the staged node set, `IssuePlanned` from the cleared plan, `IssueRefused` from the terminal `BoundaryFault.tag`, `FrontDrained` from the runtime `DrainReceipt` columns, `ReceiptEmitted` and `TransmittalIssued` from the `ArtifactReceipt` case scalars at the `contribute` fold, `NoticeIssued` from the sealed CloudEvents envelope's content type and byte length at the delivery notice owner — and every payload carries `scope`, the `ISSUE_BAGGAGE` correlation id, so a subscriber slices facts per issue with no join against a second stream. A field a payload cannot project from landed evidence is a field the point does not carry.
- Entry: `scoped(context)` reads the `ISSUE_BAGGAGE` baggage entry off an explicit context — the empty string when no issue scope is live, so a receipt contributed outside an issue drain still fires lawfully. `Production.registered` folds the table through `Hooks.register` under `Disposition.ACCUMULATE` so every collision reports, its locked `_wired` map returning the prior rail on same-scope re-entry — the scope-keyed one-shot mirrors the runtime registry's own `ScopeKey` partition and stays singular under free-threaded concurrent first use; `Production.fired` composes the cell before `Hooks.fire`, so the first fire from any seam self-registers, a `VETO` rail returns to the emitter, and an `OBSERVE` rail is fire-and-forget by modality contract; `Production.subscribed` composes the same cell before `Hooks.subscribe`. Subscribers stay app-root — `Production.subscribed(point, Hooks.tap_receipts(...))` or a domain tap — and this page registers points alone.
- Packages: `msgspec` (`Struct` payload rows and `to_builtins(payload, str_keys=True)` the direct telemetry projection), `expression` (`Block` the row table, `Option`/`Map` the one-shot cell), `opentelemetry-api` (`baggage.get_baggage` the scope read), runtime (`HookPoint`/`Hooks`/`Modality`/`Tap`/`Veto`, `traversed`/`Disposition`/`RuntimeRail`, `ScopeKey`/`DEFAULT_SCOPE` the composition axis).
- Growth: a new production fact is one `ArtifactHook` member, one payload `Struct`, and one `ARTIFACT_POINTS` row — the fire seam lands at the owner that holds the evidence, and registration, isolation, and taps follow with zero edits here; a new payload field is one `Struct` field every tap projects through `msgspec.to_builtins(payload, str_keys=True)`; a new consumer is one `Production.subscribed` call at the app root with zero artifacts edit; a wider replay window is the `FRONT_DRAINED` row's `buffer` value; a second composition is one `ScopeKey` value threaded through `scope` with every bare call untouched.
- Boundary: this page imports no artifacts sibling — payloads carry native scalars so the floor stays acyclic under `core/receipt` and `core/issue` composing it downward; the fire seams, the veto consequence, and the baggage bind are the emitting owners'; exporter, provider, and transport wiring stay the runtime telemetry owner's; and a hook payload never re-narrates a receipt — the `contribute` fold remains the one evidence truth the fired fact projects. Artifacts taps are synchronous by charter — payloads are small scalar facts the built-in taps project onto receipts and metrics in-line; an async tap rides the runtime `fire_async` surface an app wires itself, and no fire opens a span, since a fire runs under whatever span is active.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from enum import StrEnum
from threading import RLock
from typing import ClassVar, Final

from expression import Option
from expression.collections import Block, Map
from msgspec import Struct
from opentelemetry import baggage
from opentelemetry import context as otel_context

from rasm.runtime.faults import Disposition, RuntimeRail, traversed
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
    NOTICE_ISSUED = "rasm.artifacts.delivery.notice"


# --- [CONSTANTS] ------------------------------------------------------------------------

# issue-scope correlation key: core/issue binds it as baggage + log key, every payload carries it as `scope`.
ISSUE_BAGGAGE: Final[str] = "rasm.artifacts.issue"

# --- [MODELS] ---------------------------------------------------------------------------


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
    key: str
    transmittal_id: str
    sheets: int
    suitability: str
    container: str
    validation_state: str
    scope: str


class NoticeIssued(Struct, frozen=True, gc=False):
    key: str
    event_id: str
    content_type: str
    body_bytes: int
    scope: str


# --- [TABLES] ---------------------------------------------------------------------------

ARTIFACT_POINTS: Final[Block[HookPoint[Struct]]] = Block.of_seq([
    HookPoint(ArtifactHook.ISSUE_ADMITTED, IssueAdmitted, Modality.VETO),
    HookPoint(ArtifactHook.ISSUE_PLANNED, IssuePlanned, Modality.OBSERVE),
    HookPoint(ArtifactHook.ISSUE_REFUSED, IssueRefused, Modality.OBSERVE),
    HookPoint(ArtifactHook.FRONT_DRAINED, FrontDrained, Modality.REPLAY, buffer=8),
    HookPoint(ArtifactHook.RECEIPT_EMITTED, ReceiptEmitted, Modality.OBSERVE),
    HookPoint(ArtifactHook.TRANSMITTAL_ISSUED, TransmittalIssued, Modality.OBSERVE),
    HookPoint(ArtifactHook.NOTICE_ISSUED, NoticeIssued, Modality.OBSERVE),
])

# --- [OPERATIONS] -----------------------------------------------------------------------


def scoped(context: otel_context.Context, /) -> str:
    entry = baggage.get_baggage(ISSUE_BAGGAGE, context)
    return entry if isinstance(entry, str) else ""


# --- [SERVICES] -------------------------------------------------------------------------


class Production:
    # Locked scope-keyed one-shot: each composition registers once under concurrent first use.
    _lock: ClassVar[RLock] = RLock()
    _wired: ClassVar[Map[ScopeKey, RuntimeRail[Block[HookPoint[Struct]]]]] = Map.empty()

    @classmethod
    def registered(cls, *, scope: ScopeKey = DEFAULT_SCOPE) -> RuntimeRail[Block[HookPoint[Struct]]]:
        with cls._lock:
            match cls._wired.try_find(scope):
                case Option(tag="some", some=prior):
                    return prior
                case _:
                    rail = traversed(ARTIFACT_POINTS.map(lambda point: Hooks.register(point, scope=scope)), by=Disposition.ACCUMULATE)
                    cls._wired = cls._wired.add(scope, rail)
                    return rail

    @classmethod
    def fired[P: Struct](cls, point: ArtifactHook, payload: P, *, scope: ScopeKey = DEFAULT_SCOPE) -> RuntimeRail[P]:
        return cls.registered(scope=scope).bind(lambda _points: Hooks.fire(point, payload, scope=scope))

    @classmethod
    def subscribed[P: Struct](cls, point: ArtifactHook, tap: Tap[P] | Veto[P], *, scope: ScopeKey = DEFAULT_SCOPE) -> RuntimeRail[int]:
        return cls.registered(scope=scope).bind(lambda _points: Hooks.subscribe(point, tap, scope=scope))


# --- [EXPORTS] ----------------------------------------------------------------------------

__all__ = (
    "ARTIFACT_POINTS",
    "ISSUE_BAGGAGE",
    "ArtifactHook",
    "FrontDrained",
    "IssueAdmitted",
    "IssuePlanned",
    "IssueRefused",
    "NoticeIssued",
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
