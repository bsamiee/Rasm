# [PY_RUNTIME_HOOKS]

`Hooks` is the one scoped hook registry at Python grain: a hook point is a registered row with a package-qualified id, a closed msgspec payload type, and one modality, and a fire is an evidence event the emitter never re-narrates. Telemetry is a tap — receipts, metrics, and span attributes project FROM hook facts through registered subscribers — so domain code fires facts and observability subscribes, never the reverse. Apps add subscribers at composition; the registry wires no exporter, provider, or transport.

This registry formalizes the standing contributor fold: `@receipted` remains the operation-exit harvest, and a hook point is the finer-grained fact seam a lifecycle surface fires mid-operation. `Signals.emit`, `Receipt.of`, `Redaction`, and `OPEN` arrive settled from `observability/receipts#RECEIPT`; `Metrics.record` from `observability/metrics#METRIC`; the fault fences from `reliability/faults#FAULT`. Point ids carry the same uniqueness law the `SCOPES` vocabulary holds for instrumentation scopes — a colliding registration refuses at composition, so shadowing dies structurally.

## [01]-[INDEX]

- [01]-[HOOKS]: the point row, the modality vocabulary, the composition-unique registry, the polymorphic fire, the isolation fence, and the built-in telemetry taps.

## [02]-[HOOKS]

- Owner: `HookPoint[P]` is the registered row — id, payload type, modality, replay bound — and `Hooks` the registry service holding the point table, subscriber table, and bounded replay buffers as immutable maps behind one `RLock`, each keyed first by the receipts-owned `ScopeKey` so two compositions embedding the runtime in one process partition point custody, subscriber fan-out, and replay windows structurally while the `DEFAULT_SCOPE` keyword default preserves the bare call shape. `register` refuses a duplicate id and a malformed id at composition through the `boundary` fence, so every live point spells `rasm.<pkg>.<domain>.<point>` and owns its id alone within its scope.
- Cases: `Modality` is the closed dispatch vocabulary — `VETO` runs subscribers as a sequential sync transform-or-reject fold whose rail returns to the emitter; `OBSERVE` runs each subscriber as a fenced sync or async tap whose fault lands on the receipt stream while the emitter's value passes through untouched; `REPLAY` admits sync taps alone and buffers the last `buffer` payloads so attach drains the ring before forward observation. Callable instances and sync-declared callables returning awaitables follow the returned value's modality rather than their declaration shape.
- Entry: `fire(point_id, payload)` is one polymorphic emitter surface — `_delivery` admits the payload, updates any replay ring, and snapshots taps once before the registered modality selects the sync arm; the async mirror `fire_async` consumes the same delivery result and awaits awaitable taps. An unregistered id is a fault on the rail, never a silent drop.
- Auto: each registry read-modify-write and snapshot runs under the free-threading gate; tap execution never runs under it — fire-path delivery runs after release, and REPLAY attach snapshots its ring under the gate then drains it outside through a tap-local replay/forward barrier that queues concurrent forward payloads and flushes them after the retained window, so no forward payload overtakes retained facts. Replay attach is transactional: a drain fault — a raising tap or the sync contract breached by a returned awaitable — detaches the tap before the fence rails the refusal, so a half-drained subscriber never stays attached. Subscriber isolation is the `boundary`/`async_boundary` fence per tap — async delivery awaits any awaitable result, sync delivery closes a closeable awaitable before railing its modality fault, and a raising observe tap becomes a `BoundaryFault` emitted as a `rejected` receipt under the point id and original `ScopeKey`; the emitter's rail stays `Ok`, and only a `VETO` subscriber can reject. A fire runs under whatever span is active, so span correlation rides the emitter's `measured` weave and no hook opens a span of its own.
- Receipt: `tap_receipts(owner, scope=...)` and `tap_metrics(measures, domain, kind)` are the built-in taps — one streams each payload as a scope-preserving `Receipt.of(owner, ("emitted", point_id, structs.asdict(payload)))` row, the other projects the payload's numeric measures onto the `Metrics.record` mapping arm — so metrics and log lines are projections of the same fired fact and cannot disagree with it; `replayed` projects every REPLAY point, including an empty ring, as bounded data for the bundle capsule, the same retained window a late subscriber drains.
- Packages: `msgspec` (payload rows, `structs.asdict`), `expression` (`Block`/`Map`, the rail), runtime (`boundary`/`async_boundary`/`BoundaryFault`, `Signals`/`Receipt`/`OPEN`, `Metrics`), stdlib (`re`, `RLock`).
- Growth: a new hook point is one `HookPoint` row registered at composition; a new payload field is one `Struct` field every tap reads through the same `asdict` projection; a new modality is one `Modality` member with one `fire` arm under `assert_never`; a new composition is one `ScopeKey` value threaded through the `scope` keyword, never a sibling registry.
- Boundary: the registry composes the receipts and metrics owners and adds no second egress — a subscriber that needs OTLP reaches it through the taps, and a library registers points while only the app root registers subscribers.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import re
from collections.abc import Awaitable, Callable, Mapping
from enum import StrEnum
from inspect import isawaitable, iscoroutinefunction
from threading import Lock, RLock
from typing import ClassVar, Final

from expression import Ok, Result
from expression.collections import Block, Map
from msgspec import Struct, structs

from rasm.runtime.faults import BoundaryFault, RuntimeRail, async_boundary, boundary
from rasm.runtime.metrics import Metrics
from rasm.runtime.receipts import DEFAULT_SCOPE, OPEN, Receipt, ScopeKey, Signals

# --- [TYPES] ----------------------------------------------------------------------------

type Tap[P: Struct] = Callable[[P], object | Awaitable[object]]
type Veto[P: Struct] = Callable[[P], RuntimeRail[P]]


class Modality(StrEnum):
    VETO = "veto"
    OBSERVE = "observe"
    REPLAY = "replay"


# --- [CONSTANTS] ------------------------------------------------------------------------

# package-qualified point grammar; the same uniqueness law SCOPES holds for instrumentation scopes.
HOOK_ID: Final[re.Pattern[str]] = re.compile(r"^rasm\.[a-z0-9_-]+\.[a-z0-9_-]+\.[a-z0-9_-]+$")

# --- [MODELS] ---------------------------------------------------------------------------


class HookPoint[P: Struct](Struct, frozen=True):
    id: str
    payload: type[P]
    modality: Modality
    buffer: int = 0  # replay ring bound; read only by the REPLAY arm


# --- [SERVICES] -------------------------------------------------------------------------


class _ReplayAttach:
    # tap-local replay/forward barrier: while the retained window drains outside the registry gate, concurrent
    # forward payloads queue here in arrival order; `opened` flushes the queue then retires the barrier to a
    # passthrough, so retained-before-forward holds without any subscriber code executing under `Hooks._gate`.
    __slots__ = ("_gate", "_pending", "_tap")

    def __init__(self, tap: Tap[Struct] | Veto[Struct]) -> None:
        self._tap: Tap[Struct] | Veto[Struct] = tap
        self._pending: list[Struct] | None = []
        self._gate = Lock()

    def __call__(self, payload: Struct) -> object:
        with self._gate:  # Exemption: per-subscriber barrier — delivery serializes on the tap-local lock, never the registry gate
            if self._pending is not None:
                self._pending.append(payload)
                return None
            return self._tap(payload)

    def opened(self, deliver: Callable[[Struct], object]) -> None:
        with self._gate:  # Exemption: flush completes under the barrier so no forward payload overtakes the queue
            for fact in self._pending or ():
                deliver(fact)
            self._pending = None


class Hooks:
    # one free-threading gate serializes each table's read-modify-write; tap execution stays outside the gate.
    # every table keys first by the receipts-owned ScopeKey: two compositions partition custody structurally, and the
    # DEFAULT_SCOPE keyword default keeps the bare call shape scope-free.
    _points: ClassVar[Map[ScopeKey, Map[str, HookPoint[Struct]]]] = Map.empty()
    _taps: ClassVar[Map[ScopeKey, Map[str, Block[Tap[Struct] | Veto[Struct]]]]] = Map.empty()
    _rings: ClassVar[Map[ScopeKey, Map[str, Block[Struct]]]] = Map.empty()
    _gate = RLock()

    @staticmethod
    def _scoped[V](held: Map[ScopeKey, Map[str, V]], scope: ScopeKey) -> Map[str, V]:
        return held.try_find(scope).default_value(Map.empty())

    @classmethod
    def register(cls, point: HookPoint[Struct], *, scope: ScopeKey = DEFAULT_SCOPE) -> RuntimeRail[HookPoint[Struct]]:
        def admitted() -> HookPoint[Struct]:
            with cls._gate:
                points = cls._scoped(cls._points, scope)
                if HOOK_ID.fullmatch(point.id) is None:
                    raise ValueError(f"hook id {point.id!r} breaches the rasm.<pkg>.<domain>.<point> grammar")
                if points.try_find(point.id).is_some():
                    raise ValueError(f"hook id {point.id!r} is already owned")
                cls._points = cls._points.add(scope, points.add(point.id, point))
                return point

        return boundary("hooks.register", admitted)

    @classmethod
    def _attach(cls, point_id: str, member: Tap[Struct] | Veto[Struct], scope: ScopeKey) -> int:
        taps = cls._scoped(cls._taps, scope)
        held = taps.try_find(point_id).default_value(Block.empty()).append(Block.singleton(member))
        cls._taps = cls._taps.add(scope, taps.add(point_id, held))
        return len(held)

    @classmethod
    def _detach(cls, point_id: str, member: Tap[Struct] | Veto[Struct], scope: ScopeKey) -> None:
        with cls._gate:
            taps = cls._scoped(cls._taps, scope)
            survivors = taps.try_find(point_id).default_value(Block.empty()).filter(lambda held: held is not member)
            cls._taps = cls._taps.add(scope, taps.add(point_id, survivors))

    @classmethod
    def subscribe[P: Struct](cls, point_id: str, tap: Tap[P] | Veto[P], *, scope: ScopeKey = DEFAULT_SCOPE) -> RuntimeRail[int]:
        def attached() -> int:
            with cls._gate:
                row = cls._scoped(cls._points, scope)[point_id]  # KeyError converts on the fence: an unregistered point refuses
                if row.modality is not Modality.REPLAY:
                    return cls._attach(point_id, tap, scope)
                if cls._declared_async(tap):
                    raise TypeError("REPLAY hook subscribers must be synchronous so attach drains retained facts before forward observation")
                barrier = _ReplayAttach(tap)
                retained = cls._scoped(cls._rings, scope).try_find(point_id).default_value(Block.empty())
                count = cls._attach(point_id, barrier, scope)
            # the retained drain runs OUTSIDE the registry gate through the barrier, and attach is transactional:
            # `_sync_tap` raises on a tap fault or on the sync contract breached by a returned awaitable — a breach a
            # `_declared_async` probe cannot see — so the fault detaches the barrier before the fence rails the refusal.
            # An empty ring still opens the barrier so the forward contract starts clean.
            try:
                for fact in retained:  # Exemption: un-fenced drain — a raise must reach the subscribe fence, detaching first
                    cls._sync_tap(tap, fact)
                barrier.opened(lambda fact: cls._sync_tap(tap, fact))
            except BaseException:
                cls._detach(point_id, barrier, scope)
                raise
            return count

        return boundary("hooks.subscribe", attached)

    @classmethod
    def _point[P: Struct](cls, point_id: str, payload: P, scope: ScopeKey) -> RuntimeRail[HookPoint[Struct]]:
        def checked() -> HookPoint[Struct]:
            with cls._gate:
                row = cls._scoped(cls._points, scope)[point_id]
            if not isinstance(payload, row.payload):
                raise TypeError(f"hook {point_id!r} requires {row.payload.__name__}, received {type(payload).__name__}")
            return row

        return boundary("hooks.payload", checked)

    @classmethod
    def _delivery[P: Struct](
        cls, point_id: str, payload: P, scope: ScopeKey
    ) -> RuntimeRail[tuple[HookPoint[Struct], Block[Tap[Struct] | Veto[Struct]]]]:
        match cls._point(point_id, payload, scope):
            case Result(tag="error") as refused:
                return refused
            case Result(tag="ok", ok=HookPoint(modality=modality, buffer=bound) as row):
                with cls._gate:
                    if modality is Modality.REPLAY:
                        rings = cls._scoped(cls._rings, scope)
                        ring = rings.try_find(point_id).default_value(Block.empty()).append(Block.singleton(payload))
                        cls._rings = cls._rings.add(scope, rings.add(point_id, ring.skip(max(len(ring) - bound, 0))))
                    taps = cls._scoped(cls._taps, scope).try_find(point_id).default_value(Block.empty())
                return Ok((row, taps))

    @staticmethod
    def _vetoed[P: Struct](point_id: str, payload: P, taps: Block[Tap[Struct] | Veto[Struct]]) -> RuntimeRail[P]:
        return taps.fold(lambda rail, veto: rail.bind(lambda live: boundary(point_id, lambda: veto(live)).bind(lambda r: r)), Ok(payload))

    @classmethod
    def fire[P: Struct](cls, point_id: str, payload: P, *, scope: ScopeKey = DEFAULT_SCOPE) -> RuntimeRail[P]:
        match cls._delivery(point_id, payload, scope):
            case Result(tag="error") as refused:
                return refused
            case Result(tag="ok", ok=(HookPoint(modality=Modality.VETO), taps)):
                return cls._vetoed(point_id, payload, taps)
            case Result(tag="ok", ok=(HookPoint(modality=Modality.OBSERVE | Modality.REPLAY), taps)):
                taps.fold(lambda _, tap: cls._observed(point_id, tap, payload, scope), None)
                return Ok(payload)

    @classmethod
    async def fire_async[P: Struct](cls, point_id: str, payload: P, *, scope: ScopeKey = DEFAULT_SCOPE) -> RuntimeRail[P]:
        match cls._delivery(point_id, payload, scope):
            case Result(tag="error") as refused:
                return refused
            case Result(tag="ok", ok=(HookPoint(modality=Modality.VETO), taps)):
                return cls._vetoed(point_id, payload, taps)
            case Result(tag="ok", ok=(HookPoint(modality=Modality.OBSERVE | Modality.REPLAY), taps)):
                for tap in taps:
                    # Exemption: the sequential tap walk is the async fence seam — each fault folds onto the receipt stream, never out.
                    async def awaited(tap: Tap[Struct] | Veto[Struct] = tap) -> object:
                        returned = tap(payload)
                        return await returned if isawaitable(returned) else returned

                    fenced = await async_boundary(point_id, awaited)
                    fenced.swap().map(lambda fault: cls._faulted(point_id, fault, scope))
                return Ok(payload)

    @classmethod
    def replayed(cls, *, scope: ScopeKey = DEFAULT_SCOPE) -> Map[str, Block[Struct]]:
        # bundle-facing replay projection: every REPLAY point's retained ring as data — each ring arrives pre-trimmed
        # to its registered `HookPoint.buffer`, so the read is bounded by construction and mutates nothing.
        with cls._gate:
            points = cls._scoped(cls._points, scope)
            rings = cls._scoped(cls._rings, scope)
            return Map.of_seq(
                (point_id, rings.try_find(point_id).default_value(Block.empty()))
                for point_id, row in points.items()
                if row.modality is Modality.REPLAY
            )

    @classmethod
    def _observed(cls, point_id: str, tap: Tap[Struct] | Veto[Struct], payload: Struct, scope: ScopeKey) -> None:
        boundary(point_id, lambda: cls._sync_tap(tap, payload)).swap().map(lambda fault: cls._faulted(point_id, fault, scope))

    @staticmethod
    def _declared_async(tap: object) -> bool:
        return iscoroutinefunction(tap) or iscoroutinefunction(getattr(tap, "__call__", None))

    @staticmethod
    def _sync_tap(tap: Tap[Struct] | Veto[Struct], payload: Struct) -> object:
        returned = tap(payload)
        if not isawaitable(returned):
            return returned
        close = getattr(returned, "close", None)
        if callable(close):
            close()
        raise TypeError("synchronous hook delivery cannot consume an awaitable tap result")

    @staticmethod
    def _faulted(point_id: str, fault: BoundaryFault, scope: ScopeKey) -> None:
        # isolation law: a subscriber fault is evidence on the receipt stream, never a break in the emitter.
        Signals.emit(Receipt.of(point_id, fault), OPEN, scope=scope)

    @staticmethod
    def tap_receipts(owner: str, *, scope: ScopeKey = DEFAULT_SCOPE) -> Tap[Struct]:
        return lambda payload: Signals.emit(Receipt.of(owner, ("emitted", owner, dict(structs.asdict(payload)))), OPEN, scope=scope)

    @staticmethod
    def tap_metrics(measures: Callable[[Struct], Mapping[str, float]], domain: str, kind: str) -> Tap[Struct]:
        return lambda payload: Metrics.record(measures(payload), domain=domain, kind=kind)
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
