# [PY_RUNTIME_HOOKS]

`Hooks` is the one scoped hook registry at Python grain: a hook point is a registered row with a package-qualified id, a closed msgspec payload type, and one modality, and a fire is an evidence event the emitter never re-narrates. Telemetry is a tap — receipts, metrics, and span attributes project FROM hook facts through registered subscribers — so domain code fires facts and observability subscribes, never the reverse. Apps add subscribers at composition; the registry wires no exporter, provider, or transport.

This registry formalizes the standing contributor fold: `@receipted` remains the operation-exit harvest, and a hook point is the finer-grained fact seam a lifecycle surface fires mid-operation. `Signals.emit`, `Receipt.of`, `Redaction`, and `OPEN` arrive settled from `observability/receipts#RECEIPT`; `Metrics.record` from `observability/metrics#METRIC`; the fault fences from `reliability/faults#FAULT`. Point ids carry the same uniqueness law the `SCOPES` vocabulary holds for instrumentation scopes — a colliding registration refuses at composition, so shadowing dies structurally.

## [01]-[INDEX]

- [01]-[HOOKS]: the point row, the modality vocabulary, the composition-unique registry, the polymorphic fire, the isolation fence, and the built-in telemetry taps.

## [02]-[HOOKS]

- Owner: `HookPoint[P]` is the registered row — id, payload type, modality, replay bound — and `Hooks` the registry service holding the point table, the subscriber table, and the bounded replay buffers as atomically swapped immutable maps. `register` refuses a duplicate id and a malformed id at composition through the `boundary` fence, so every live point spells `rasm.<pkg>.<domain>.<point>` and owns its id alone.
- Cases: `Modality` is the closed dispatch vocabulary — `VETO` runs subscribers as a sequential sync transform-or-reject fold whose rail returns to the emitter; `OBSERVE` runs each subscriber as a fenced tap whose fault lands on the receipt stream while the emitter's value passes through untouched; `REPLAY` buffers the last `buffer` payloads so a late subscriber drains the ring on attach, then observes forward.
- Entry: `fire(point_id, payload)` is one polymorphic emitter surface — the registered row's modality selects the arm, the payload type-checks against the row's declared `Struct`, and the async mirror `fire_async` awaits awaitable taps off the emitter's own scope. An unregistered id is a fault on the rail, never a silent drop.
- Auto: subscriber isolation is the `boundary`/`async_boundary` fence per tap — a raising observe tap becomes a `BoundaryFault` emitted as a `rejected` receipt under the point id, and the emitter's rail stays `Ok`; only a `VETO` subscriber can reject, because rejection is that modality's declared contract. A fire runs under whatever span is active, so span correlation rides the emitter's `measured` weave and no hook opens a span of its own.
- Receipt: `tap_receipts(owner)` and `tap_metrics(measures, domain, kind)` are the built-in taps — one streams each payload as a `Receipt.of(owner, ("emitted", point_id, structs.asdict(payload)))` row, the other projects the payload's numeric measures onto the `Metrics.record` mapping arm — so metrics and log lines are projections of the same fired fact and cannot disagree with it.
- Packages: `msgspec` (payload rows, `structs.asdict`), `expression` (`Block`/`Map`, the rail), runtime (`boundary`/`async_boundary`/`BoundaryFault`, `Signals`/`Receipt`/`OPEN`, `Metrics`), stdlib `re` (one module-level id grammar pattern).
- Growth: a new hook point is one `HookPoint` row registered at composition; a new payload field is one `Struct` field every tap reads through the same `asdict` projection; a new modality is one `Modality` member with one `fire` arm under `assert_never`.
- Boundary: the registry composes the receipts and metrics owners and adds no second egress — a subscriber that needs OTLP reaches it through the taps, and a library registers points while only the app root registers subscribers.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import re
from collections.abc import Awaitable, Callable, Mapping
from enum import StrEnum
from inspect import iscoroutinefunction
from typing import ClassVar, Final

from expression import Ok
from expression.collections import Block, Map
from msgspec import Struct, structs

from rasm.runtime.faults import BoundaryFault, RuntimeRail, async_boundary, boundary
from rasm.runtime.metrics import Metrics
from rasm.runtime.receipts import OPEN, Receipt, Signals

# --- [TYPES] ----------------------------------------------------------------------------

type Tap[P: Struct] = Callable[[P], object] | Callable[[P], Awaitable[object]]
type Veto[P: Struct] = Callable[[P], RuntimeRail[P]]


class Modality(StrEnum):
    VETO = "veto"
    OBSERVE = "observe"
    REPLAY = "replay"


# --- [CONSTANTS] ------------------------------------------------------------------------

# package-qualified point grammar; the same uniqueness law SCOPES holds for instrumentation scopes.
HOOK_ID: Final[re.Pattern[str]] = re.compile(r"^rasm\.[a-z0-9_]+\.[a-z0-9_]+\.[a-z0-9_]+$")

# --- [MODELS] ---------------------------------------------------------------------------


class HookPoint[P: Struct](Struct, frozen=True):
    id: str
    payload: type[P]
    modality: Modality
    buffer: int = 0  # replay ring bound; read only by the REPLAY arm


# --- [SERVICES] -------------------------------------------------------------------------


class Hooks:
    # frozen tables swapped under atomic reference assignment — registration is composition-time, fire is read-only.
    _points: ClassVar[Map[str, HookPoint[Struct]]] = Map.empty()
    _taps: ClassVar[Map[str, Block[Tap[Struct] | Veto[Struct]]]] = Map.empty()
    _rings: ClassVar[Map[str, Block[Struct]]] = Map.empty()

    @classmethod
    def register(cls, point: HookPoint[Struct]) -> RuntimeRail[HookPoint[Struct]]:
        def admitted() -> HookPoint[Struct]:
            if HOOK_ID.fullmatch(point.id) is None:
                raise ValueError(f"hook id {point.id!r} breaches the rasm.<pkg>.<domain>.<point> grammar")
            if cls._points.try_find(point.id).is_some():
                raise ValueError(f"hook id {point.id!r} is already owned")
            cls._points = cls._points.add(point.id, point)
            return point

        return boundary("hooks.register", admitted)

    @classmethod
    def subscribe[P: Struct](cls, point_id: str, tap: Tap[P] | Veto[P]) -> RuntimeRail[int]:
        def attached() -> int:
            row = cls._points[point_id]  # KeyError converts on the fence: subscribing to an unregistered point refuses
            held = cls._taps.try_find(point_id).default_value(Block.empty()).append(Block.singleton(tap))
            cls._taps = cls._taps.add(point_id, held)
            if row.modality is Modality.REPLAY:  # a late subscriber drains the ring on attach, then observes forward
                cls._rings.try_find(point_id).default_value(Block.empty()).fold(lambda _, fact: cls._observed(point_id, tap, fact), None)
            return len(held)

        return boundary("hooks.subscribe", attached)

    @classmethod
    def fire[P: Struct](cls, point_id: str, payload: P) -> RuntimeRail[P]:
        match cls._points.try_find(point_id).map(lambda row: row.modality).default_value(None):
            case Modality.VETO:
                taps = cls._taps.try_find(point_id).default_value(Block.empty())
                return taps.fold(lambda rail, veto: rail.bind(lambda live: boundary(point_id, lambda: veto(live)).bind(lambda r: r)), Ok(payload))
            case Modality.OBSERVE:
                cls._taps.try_find(point_id).default_value(Block.empty()).fold(lambda _, tap: cls._observed(point_id, tap, payload), None)
                return Ok(payload)
            case Modality.REPLAY:
                ring = cls._rings.try_find(point_id).default_value(Block.empty()).append(Block.singleton(payload))
                bound = cls._points[point_id].buffer
                cls._rings = cls._rings.add(point_id, ring.skip(max(len(ring) - bound, 0)))
                cls._taps.try_find(point_id).default_value(Block.empty()).fold(lambda _, tap: cls._observed(point_id, tap, payload), None)
                return Ok(payload)
            case _:
                return boundary("hooks.fire", lambda: (_ for _ in ()).throw(KeyError(point_id)))

    @classmethod
    async def fire_async[P: Struct](cls, point_id: str, payload: P) -> RuntimeRail[P]:
        match cls._points.try_find(point_id).map(lambda row: row.modality).default_value(None):
            case Modality.VETO:
                return cls.fire(point_id, payload)  # veto taps are sync transform/reject by contract; the sync arm owns them
            case Modality.OBSERVE | Modality.REPLAY as modality:
                if modality is Modality.REPLAY:
                    ring = cls._rings.try_find(point_id).default_value(Block.empty()).append(Block.singleton(payload))
                    cls._rings = cls._rings.add(point_id, ring.skip(max(len(ring) - cls._points[point_id].buffer, 0)))
                for tap in cls._taps.try_find(point_id).default_value(Block.empty()):
                    # Exemption: the sequential tap walk is the async fence seam — each fault folds onto the receipt stream, never out.
                    fenced = (
                        await async_boundary(point_id, lambda tap=tap: tap(payload))
                        if iscoroutinefunction(tap)
                        else boundary(point_id, lambda tap=tap: tap(payload))
                    )
                    fenced.swap().map(lambda fault: cls._faulted(point_id, fault))
                return Ok(payload)
            case _:
                return boundary("hooks.fire", lambda: (_ for _ in ()).throw(KeyError(point_id)))

    @classmethod
    def _observed(cls, point_id: str, tap: Tap[Struct] | Veto[Struct], payload: Struct) -> None:
        boundary(point_id, lambda: tap(payload)).swap().map(lambda fault: cls._faulted(point_id, fault))

    @staticmethod
    def _faulted(point_id: str, fault: BoundaryFault) -> None:
        # isolation law: a subscriber fault is evidence on the receipt stream, never a break in the emitter.
        Signals.emit(Receipt.of(point_id, fault), OPEN)

    @staticmethod
    def tap_receipts(owner: str) -> Tap[Struct]:
        return lambda payload: Signals.emit(Receipt.of(owner, ("emitted", owner, dict(structs.asdict(payload)))), OPEN)

    @staticmethod
    def tap_metrics(measures: Callable[[Struct], Mapping[str, float]], domain: str, kind: str) -> Tap[Struct]:
        return lambda payload: Metrics.record(measures(payload), domain=domain, kind=kind)
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
