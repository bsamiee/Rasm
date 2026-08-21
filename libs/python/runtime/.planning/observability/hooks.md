# [PY_RUNTIME_HOOKS]

`Hooks` is the one scoped hook registry at Python grain: a hook point is a registered row with a package-qualified id drawn from its own package's roster, a closed msgspec payload type, and one modality arm, and a fire is an evidence event the emitter never re-narrates. Telemetry is a tap — receipts, metrics, and span attributes project FROM hook facts through registered subscribers — so domain code fires facts and observability subscribes, never the reverse. Apps add subscribers at composition; the registry wires no exporter, provider, or transport.

This registry formalizes the standing contributor fold: `@receipted` remains the operation-exit harvest, and a hook point is the finer-grained fact seam a lifecycle surface fires mid-operation. `Signals.emit`, `Receipt.of`, `Redaction`, and `OPEN` arrive settled from `observability/receipts#RECEIPT`; `Metrics.record` from `observability/metrics#METRIC`; the fault fences from `reliability/faults#FAULT`. Point ids carry the same uniqueness law the `SCOPES` vocabulary holds for instrumentation scopes — a colliding registration refuses at composition, so shadowing dies structurally.

## [01]-[INDEX]

- [02]-[HOOKS]: the point row, the shared long-fold `StageMark` payload, the modality family carrying its own retained depth, the composition-unique registry, the polymorphic fire, subscription custody and whole-scope release, the fenced isolation with its counted fault window, and the built-in telemetry tap rows.

## [02]-[HOOKS]

- Owner: `HookPoint[P]` is the registered row — id, payload type, and one modality arm carrying whatever depth that arm admits — and `Hooks` the registry service holding the point table, subscriber table, bounded replay windows, install ledger, and per-composition isolated-fault window as immutable maps behind one `RLock`, each keyed first by the receipts-owned `ScopeKey` so two compositions embedding the runtime in one process partition point custody, subscriber fan-out, replay windows, and fault accounting structurally while the `DEFAULT_SCOPE` keyword default preserves the bare call shape. `register` is the one polymorphic claim over a point or a whole roster, refusing a duplicate id, a malformed id, and a retaining row declaring a window that holds nothing, all at composition through the `boundary` fence, so every live point spells `rasm.<pkg>.<domain>.<point>` and owns its id alone within its scope.
- Cases: `Modality` is the closed delivery family — `veto` runs subscribers as a sequential sync transform-or-reject fold whose rail returns to the emitter; `observe` runs each subscriber as a fenced sync or async tap whose fault lands on the fault window and the receipt stream while the emitter's value passes through untouched; `replay` admits sync taps alone and retains the last `replay` payloads so attach drains the window before forward observation. Retained DEPTH rides the retaining arm alone and stays a point fact, so a non-retaining row cannot spell a window bound and the branch's live replay rows keep their three distinct depths. Callable instances and sync-declared callables returning awaitables follow the returned value's modality rather than their declaration shape, and each arm admits its own return contract at the fence — a veto answering anything but the payload rail and an observe tap answering an awaitable a sync delivery cannot drive each refuse there rather than escaping the fold that consumes them. Every delivery carries the fired point id beside the payload, so one tap fanned across a roster names which point it is reading.
- Cases: `HookId` is the vocabulary SHAPE the mechanism owns while every package owns its members — a point id is a member of its own folder's `StrEnum` roster, so a bare string literal constructs no row, a fire seam re-spells no id, and the grammar gate still proves each member's value against the package-qualified pattern no enum membership can decide.
- Law: `installed(owner, receipt, scope=...)` is the producer-install ledger and `installs(scope=...)` its total read; `points(scope=...)` is the membership probe over the point side, so a mounted roster carrying no install row names the leg that ran half.
- Law: a producer's composition leg deposits its own receipt here, runtime importing no producer folder to type one.
- Law: an empty roster IS the diagnosis — no producer leg ran, so every `fired` call took the unregistered-id path.
- Law: a producer claims its whole point table in ONE gated transition — the roster arm swaps the table only past its last admitted row and reports every breach together, so a refusal leaves custody exactly as it stood and no retire verb is owed against a half-mount the claim cannot produce.
- Law: custody RETIRES whole — `release(scope=...)` drops one composition's points, taps, replay windows, install rows, and fault window in one gated swap and answers the `Released` verdict carrying both accounting windows, so an embedded runtime that shut down owns no id its re-admission then collides with and its counted losses leave with the value rather than dying with the tables.
- Law: every lossy path COUNTS AND RECORDS — the replay trim and the isolated-fault window both park through the receipts-owned `Ring`, and one `_counted` fold reads that ring's own MOVEMENT onto `rasm.runtime.hook.shed`/`.lost`, so an evicted fact and a fault the receipt stream never carried are a number a capsule subtracts AND a series a board trends, exactly as the lane conduit already counts its authorized pulse drops. The record is a DELTA and lands outside the gate: a running total re-adds the window's whole history at every fire, and holding the registry lock across a metric write serializes every fire in the process behind one export-side write.
- Law: `StageMark` is the ONE long-fold mark carrier every pulse point in the estate registers as its payload — position, units closed, units expected — so a producing lane declares no mark shape of its own and one tap reads every lane's progress. `stage` is ERASED at the point and CLOSED at the producer, each lane declaring its own `<Lane>Stage(StrEnum)` and passing `.value`, which keeps ONE payload type per registered point while the roster stays a producer-side type fact and no cross-lane phase ladder forms. `total` rides `Option` because a fold over a stream or an unbounded search knows no total, and a zero default publishes exactly the "no work" reading such a producer never took.
- Entry: `register(points, scope=...)` and `subscribe(points, tap, scope=...)` fold arity off the request shape — one `HookPoint` or one point id serves that point, a `Block` serves the roster whole — so a producer claims and taps its point table at one grain and the roster-to-subscribe fold lives here rather than at every caller. Subscription answers the DETACHER: one `Attachment` per attached member, or a `Block` of them for a roster, each holding the member attach actually used so a bracket or an `AsyncExitStack` retires exactly what it opened. Registration swaps one gated table; subscription unwinds instead, closing every attachment it opened on the first refusal, because a replay row's retained drain runs outside the registry gate and no swap can cover it. `fire(point_id, payload)` is one polymorphic emitter surface — `_delivery` admits the payload, parks into any replay window, and snapshots taps once before the registered modality selects the sync arm; the async mirror `fire_async` consumes the same delivery result and awaits awaitable taps. An unregistered id is a fault on the rail, never a silent drop.
- Auto: each registry read-modify-write and snapshot runs under the free-threading gate; tap execution never runs under it — fire-path delivery runs after release, and replay attach snapshots its window under the gate then drains it outside through a tap-local replay/forward barrier that queues concurrent forward payloads and flushes them after the retained window, so no forward payload overtakes retained facts. Replay attach is transactional: a drain fault — a raising tap or the sync contract breached by a returned awaitable — detaches the tap before the fence rails the refusal, so a half-drained subscriber never stays attached. Subscriber isolation is the `boundary`/`async_boundary` fence per tap — async delivery awaits any awaitable result, sync delivery closes a closeable awaitable before railing its modality fault, a veto answering anything but a rail refuses on that same fence so the fold never carries a value without rail members into the next member's `bind`, and a raising observe tap becomes a `BoundaryFault` parked as evidence and emitted as a `rejected` receipt under the point id and original `ScopeKey`. That emission rides its OWN fence: a closed stream or a refusing processor raising there would propagate out of the tap walk and out of `fire`, destroying the emitter value the isolation exists to protect, so a refused sink parks the same evidence and counts it instead. The emitter's rail stays `Ok`, and only a `veto` subscriber can reject. A fire runs under whatever span is active, so span correlation rides the emitter's `measured` weave and no hook opens a span of its own.
- Receipt: `TapRow` is the built-in tap family through the one subscribe door — the `receipts` case streams each payload as a scope-preserving `Receipt.of(owner, ("emitted", point_id, structs.asdict(payload)))` row, the `metrics` case projects the payload's numeric measures onto the `Metrics.record` mapping arm — so metrics and log lines are projections of the same fired fact under the SAME composition key the subscription carries, where two independent scope arguments could partition one fact's evidence planes across compositions. Each case binds a named member the returned `Attachment` detaches by identity, so no built-in reaches the registry as a lambda nothing holds. `replayed` projects every retaining point, including an empty window, as bounded data for the bundle capsule beside what its trim shed, and `faults` projects this composition's isolation window the same way.
- Packages: `msgspec` (payload rows, `structs.asdict`), `expression` (`Block`/`Map`/`Option`, `@tagged_union`, the rail), runtime (the rostered `HOOKS_*` fence rows beside `boundary`/`async_boundary`/`BoundaryFault`, `Signals`/`Receipt`/`Ring`/`OPEN`, `Metrics`), stdlib (`re`, `StrEnum`, `RLock`).
- Growth: a new long fold reporting progress is one `<Lane>Stage(StrEnum)` at its own producer and one `HookPoint(id=…, payload=StageMark, modality=Modality(observe=None))` row — the carrier, the tap, and the capsule projection are untouched; a new hook point is one `HookPoint` row registered at composition and a producer's whole table is one `Block` through the same entry, tapped through the same shape; a new point id is one member on its package's own roster, never a literal at a fire seam; a new payload field is one `Struct` field every tap reads through the same `asdict` projection; a new modality is one `Modality` case with one arm on each `fire` surface, the standing `assert_never` arm breaking every surface that lacks it; a new built-in tap is one `TapRow` case with its own `bound` arm; a new producer install is one `installed` deposit inside its own leg; a new composition is one `ScopeKey` value threaded through the `scope` keyword and retired through one `release`, never a sibling registry.
- Law: every fence resolves ONE `reliability/faults#FAULT` `RAISES` anchor under `RuntimeLeg.HOOKS` — register, release, subscribe, payload admission, the tap walk, and the isolation sink — and the tap, subscribe, and isolation fences keep the plane's catch-all with the reason stated — a subscriber is caller code whose raise surface no registry can roster, and a leak destroys the emitter value the isolation exists to protect. The FIRED point leaves the fault subject and rides the isolated receipt's OWNER slot, which is where a reader already collects it.
- Boundary: the registry composes the receipts and metrics owners and adds no second egress — a subscriber that needs OTLP reaches it through the taps, and a library registers points while only the app root registers subscribers.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import re
from collections.abc import Awaitable, Callable, Mapping
from enum import StrEnum
from inspect import isawaitable, iscoroutinefunction
from threading import Lock, RLock
from typing import ClassVar, Final, Literal, assert_never, overload

from expression import Nothing, Ok, Option, Result, Some, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct, structs

from rasm.runtime.faults import (
    HOOKS_ISOLATED,
    HOOKS_PAYLOAD,
    HOOKS_REGISTER,
    HOOKS_RELEASE,
    HOOKS_SUBSCRIBE,
    HOOKS_TAP,
    BoundaryFault,
    RuntimeRail,
    async_boundary,
    boundary,
)
from rasm.runtime.metrics import Metrics
from rasm.runtime.receipts import DEFAULT_SCOPE, OPEN, Receipt, Ring, ScopeKey, Signals

# --- [TYPES] ----------------------------------------------------------------------------

# the mechanism owns the SHAPE of the id vocabulary and never its members: a point id is a member of the owning
# package's own `StrEnum` roster, so a bare string literal constructs no row and a fire seam cannot re-spell an id its
# roster already names. Two folders already prove the form — one derives its whole `HookPoint` block by comprehension
# off its roster — while the `HOOK_ID` gate still proves each member's VALUE, which no enum membership can decide.
type HookId = StrEnum

# every delivery carries the FIRED POINT beside its payload, because `subscribe` reaches roster grain: one tap fanned
# across a producer's whole table would otherwise read every point's payload with nothing naming which fired, and the
# built-in receipt tap would stamp one subject over the lot. The delivery site already holds the id, so carrying it is
# free and its absence is what a subscriber cannot recover.
type Tap[P: Struct] = Callable[[HookId, P], object | Awaitable[object]]
type Veto[P: Struct] = Callable[[HookId, P], RuntimeRail[P]]


@tagged_union(frozen=True)
class Modality:
    # the closed delivery family, and the retained DEPTH rides the retaining arm alone: a window bound is a fact only
    # a replay point has, so a flat column on the row spells one on every observe and veto row that never reads it,
    # while a case payload makes that spelling unrepresentable rather than merely unread. Depth stays a POINT fact —
    # the branch runs live replay rows at three distinct depths and a folder declares a wider window its own growth
    # axis — so the arm carries the value each row supplies, never a depth the vocabulary fixes for every point.
    tag: Literal["veto", "observe", "replay"] = tag()
    veto: None = case()
    observe: None = case()
    replay: int = case()


# --- [CONSTANTS] ------------------------------------------------------------------------

# package-qualified point grammar; the same uniqueness law SCOPES holds for instrumentation scopes. Anchors are
# `\A`/`\Z` so one spelling bounds the pattern under every match verb, exactly as the receipts-owned scope grammar does.
HOOK_ID: Final[re.Pattern[str]] = re.compile(r"\Arasm\.[a-z0-9_-]+\.[a-z0-9_-]+\.[a-z0-9_-]+\Z")

# isolated-fault window per composition: a subscriber fault is evidence the emitter's rail cannot carry by law, so the
# registry retains the recent ones under one cap and counts what falls past it. The window is a diagnostic TAIL rather
# than a stream — a composition faulting past this depth already published every earlier fault as its own receipt line.
FAULT_WINDOW: Final[int] = 64

# the registry's own capability segment and its two counted losses, spelled once. `MEASURES` raises on an
# unregistered pair, so both rows land at `observability/metrics#METRIC` before a single record fires here; the
# recording reads the ring's own MOVEMENT, so a fire that shed nothing records nothing and the hot path of a
# composition losing no fact allocates no series at all. `shed` is what a bounded window EVICTED under pressure and
# `lost` a fact whose own sink refused it — both authorized drops the emitter's rail cannot carry by law, so an
# unrecorded one reads to an operator as no loss, which is the exact silence the counted `Ring` exists to end.
DOMAIN: Final[str] = "runtime"
HOOK_SHED: Final[str] = "rasm.runtime.hook.shed"
HOOK_LOST: Final[str] = "rasm.runtime.hook.lost"

# --- [MODELS] ---------------------------------------------------------------------------


class StageMark(Struct, frozen=True, gc=False):
    # THE shared long-fold mark every pulse point in the estate carries: which position a fold reached, how many
    # units it has closed, and how many it expects. `stage` is ERASED here and CLOSED at the producer — each lane
    # declares its own `<Lane>Stage(StrEnum)` and passes `.value` — so the registry proves ONE payload type per
    # point while the roster stays a producer-side type fact, and no cross-lane phase ladder forms out of the
    # union of every lane's positions. `total` rides `Option` because a fold over a stream, a generator, or an
    # unbounded search KNOWS no total: the retired `total: int = 0` default published a zero every such producer's
    # reader then took for "no work", the forged reading `docs/laws/scars.md` `[FORGED_ZERO]` names outright, where
    # absence is the honest answer and a progress consumer renders it as indeterminate rather than as complete.
    stage: str
    done: int
    total: Option[int] = Nothing


class HookPoint[P: Struct](Struct, frozen=True):
    id: HookId
    payload: type[P]
    modality: Modality


class Attachment(Struct, frozen=True):
    # the DETACHER subscription answers with, holding the exact member attach used — a replay row's barrier, never the
    # caller's own tap, which the identity filter would leave standing while reading as a clean release. Spelled
    # `Attachment` because `transport/filter#FILTER` already owns `Subscription` for the CloudEvents resource, and one
    # package carrying two unrelated concepts under one name is a seating failure rather than a naming coincidence.
    # The value and the registry are one owner declared in two halves, so `close` reaches the registry's own retire.
    point: HookId
    scope: ScopeKey
    member: Tap[Struct] | Veto[Struct]

    def close(self) -> None:
        Hooks._detach(self.point, self.member, self.scope)

    def __enter__(self) -> "Attachment":
        return self

    def __exit__(self, *_exc: object) -> None:
        self.close()


class Released(Struct, frozen=True):
    # the retirement VERDICT: what release actually retired, carrying BOTH accounting windows, because a capsule
    # captured after release reaches no registry at all — the shed and refused-sink counts leave through this value or
    # they die with the tables that held them. Point and install rosters answer the other shutdown question: which
    # producer legs this composition was still holding when its root let go.
    scope: ScopeKey
    points: Block[HookId]
    installs: Block[str]
    faults: Ring[Receipt]
    retained: Map[HookId, Ring[Struct]]


@tagged_union(frozen=True)
class TapRow:
    # the registry's OWN taps as rows through the one subscribe door: each case owns its projection, so a built-in
    # attaches as a NAMED member the returned `Attachment` detaches by identity rather than as the inline lambda a
    # factory returned and no caller ever held. One `scope` reaches both halves — the subscription's own — where a
    # second scope argument on the metrics tap could disagree with the point's custody key and partition one fired
    # fact's two evidence planes across compositions while every line and every series still read as correct.
    tag: Literal["receipts", "metrics"] = tag()
    receipts: str = case()
    metrics: tuple[Callable[[Struct], Mapping[str, float]], str, str] = case()

    def bound(self, scope: ScopeKey) -> Tap[Struct]:
        match self:
            case TapRow(tag="receipts", receipts=owner):

                def emitted(point_id: HookId, payload: Struct) -> None:
                    # each line takes the FIRED POINT as its subject and the producer as its owner: one tap over a
                    # whole roster emits a distinguishable row per point, where repeating the owner into both slots
                    # collapses a producer's table onto one name and strands the reader with the payload alone.
                    Signals.emit(Receipt.of(owner, ("emitted", point_id, dict(structs.asdict(payload)))), OPEN, scope=scope)

                return emitted
            case TapRow(tag="metrics", metrics=(measures, domain, kind)):

                def recorded(_point_id: HookId, payload: Struct) -> None:
                    # `kind` stays the caller's rather than the fired point's: a roster fan measures one producing
                    # subject across its whole table, and the measure NAMES already separate the rows.
                    Metrics.record(measures(payload), domain=domain, kind=kind, scope=scope)

                return recorded
            case _ as unreachable:
                assert_never(unreachable)


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

    def __call__(self, point_id: HookId, payload: Struct) -> object:
        with self._gate:  # Exemption: per-subscriber barrier — delivery serializes on the tap-local lock, never the registry gate
            if self._pending is not None:
                self._pending.append(payload)
                return None
            return self._tap(point_id, payload)

    def opened(self, deliver: Callable[[Struct], object]) -> None:
        with self._gate:  # Exemption: flush completes under the barrier so no forward payload overtakes the queue
            for fact in self._pending or ():
                deliver(fact)
            self._pending = None


class Hooks:
    # one free-threading gate serializes each table's read-modify-write; tap execution stays outside the gate.
    # every table keys first by the receipts-owned ScopeKey: two compositions partition custody structurally, the
    # DEFAULT_SCOPE keyword default keeps the bare call shape scope-free, and `release` is what makes that keying a
    # LIFECYCLE rather than a leak — a table nothing ever removes a scope from outlives the composition that filled it,
    # so an embedded runtime's shutdown leaves its ids claimed and its re-admission collides with its own ghost.
    _points: ClassVar[Map[ScopeKey, Map[HookId, HookPoint[Struct]]]] = Map.empty()
    _taps: ClassVar[Map[ScopeKey, Map[HookId, Block[Tap[Struct] | Veto[Struct]]]]] = Map.empty()
    _rings: ClassVar[Map[ScopeKey, Map[HookId, Ring[Struct]]]] = Map.empty()
    _installs: ClassVar[Map[ScopeKey, Map[str, Struct]]] = Map.empty()
    _faults: ClassVar[Map[ScopeKey, Ring[Receipt]]] = Map.empty()
    _gate = RLock()

    @staticmethod
    def _scoped[K, V](held: Map[ScopeKey, Map[K, V]], scope: ScopeKey) -> Map[K, V]:
        return held.try_find(scope).default_value(Map.empty())

    @staticmethod
    def _dropped[V](held: Map[ScopeKey, V], scope: ScopeKey) -> Map[ScopeKey, V]:
        # every table retires under ONE spelling, and membership decides first because a retire is total only over a
        # key the map holds — a scope that claimed points and attached no tap owns no tap row to remove at all.
        return held.remove(scope) if scope in held else held

    @overload
    @classmethod
    def register(cls, points: HookPoint[Struct], *, scope: ScopeKey = ...) -> RuntimeRail[HookPoint[Struct]]: ...
    @overload
    @classmethod
    def register(cls, points: Block[HookPoint[Struct]], *, scope: ScopeKey = ...) -> RuntimeRail[Block[HookPoint[Struct]]]: ...

    @classmethod
    def register(
        cls, points: HookPoint[Struct] | Block[HookPoint[Struct]], *, scope: ScopeKey = DEFAULT_SCOPE
    ) -> RuntimeRail[HookPoint[Struct] | Block[HookPoint[Struct]]]:
        # one polymorphic claim over either arity: a roster admits or refuses WHOLE. `claimed` builds off the
        # immutable held map and `cls._points` takes it only past the last row, so a refusal leaves the standing
        # roster byte-identical and never the half-mounted custody an accumulating per-point traverse leaves behind.
        # Breaches report together, so the whole-set transition keeps the accumulating diagnosis a per-point fold
        # bought by surrendering atomicity, and the single-point arm hands its own point back, so a producer leg
        # composing one row reads exactly as it did before the roster arm existed.
        def admitted() -> HookPoint[Struct] | Block[HookPoint[Struct]]:
            roster = points if isinstance(points, Block) else Block.singleton(points)
            with cls._gate:
                claimed, breaches = cls._scoped(cls._points, scope), Block.empty()
                for point in roster:
                    if HOOK_ID.fullmatch(point.id) is None:
                        breaches = breaches.append(Block.singleton(f"{point.id!r} breaches the rasm.<pkg>.<domain>.<point> grammar"))
                    elif claimed.try_find(point.id).is_some():
                        breaches = breaches.append(Block.singleton(f"{point.id!r} is already owned"))
                    elif point.modality.tag == "replay" and point.modality.replay < 1:
                        # a retaining row whose window holds nothing sheds every fact it parks and drains an empty
                        # ring on every attach, so the depth admits with the id rather than reading as a live window.
                        breaches = breaches.append(Block.singleton(f"{point.id!r} retains a window of {point.modality.replay}"))
                    else:
                        claimed = claimed.add(point.id, point)
                if len(breaches) > 0:
                    raise ValueError(f"hook roster refused: {'; '.join(breaches)}")
                cls._points = cls._points.add(scope, claimed)
            return points

        return boundary(HOOKS_REGISTER, admitted, catch=ValueError)

    @classmethod
    def points(cls, *, scope: ScopeKey = DEFAULT_SCOPE) -> Map[HookId, HookPoint[Struct]]:
        # membership probe, the point-side twin of `installs`: a producer leg proves its claim landed and the bundle
        # capsule reads the standing roster, so a mounted roster carrying no install row names the half-run leg.
        with cls._gate:
            return cls._scoped(cls._points, scope)

    @classmethod
    def installed[R: Struct](cls, owner: str, receipt: R, *, scope: ScopeKey = DEFAULT_SCOPE) -> R:
        # producer-install ledger: a producer folder's composition leg registers points UPWARD through this registry,
        # so its install receipt homes here too rather than in a folder-local global the bundle capsule cannot reach —
        # runtime imports no producer, and a `GraduationInstall`/`ComputeInstall` typed whole would invert the strata.
        # Deposits pass the receipt through, so a leg lands one inside its own terminal with no second statement, and
        # a re-install under one scope REPLACES its row: each receipt names the roster now standing, never a history.
        with cls._gate:
            cls._installs = cls._installs.add(scope, cls._scoped(cls._installs, scope).add(owner, receipt))
        return receipt

    @classmethod
    def installs(cls, *, scope: ScopeKey = DEFAULT_SCOPE) -> Map[str, Struct]:
        # total roster read for the bundle capsule: a composition whose producers never ran their install leg answers
        # EMPTY, which is exactly the diagnosis — every `fired` call took the unregistered-id path.
        with cls._gate:
            return cls._scoped(cls._installs, scope)

    @classmethod
    def release(cls, *, scope: ScopeKey = DEFAULT_SCOPE) -> RuntimeRail[Released]:
        # custody retires WHOLE in one gated swap: points, taps, replay windows, install rows, and the fault window
        # leave together, because a partial retire leaves ids claimed under a scope whose subscribers are gone and the
        # next admission collides with its own predecessor. The verdict carries both accounting windows out — after
        # this swap no read reaches them — and an empty scope REFUSES rather than answering a silent no-op, since a
        # root releasing custody it never claimed is asking about a composition whose producer legs never ran.
        def retired() -> Released:
            with cls._gate:
                points, installs = cls._scoped(cls._points, scope), cls._scoped(cls._installs, scope)
                if len(points) == 0 and len(installs) == 0:
                    raise KeyError(f"{scope!r} holds no hook custody to release")
                held = Released(
                    scope=scope,
                    points=Block.of_seq(points.keys()),
                    installs=Block.of_seq(installs.keys()),
                    faults=cls._faults.try_find(scope).default_value(Ring(cap=FAULT_WINDOW)),
                    retained=cls._scoped(cls._rings, scope),
                )
                cls._points, cls._taps = cls._dropped(cls._points, scope), cls._dropped(cls._taps, scope)
                cls._rings, cls._installs = cls._dropped(cls._rings, scope), cls._dropped(cls._installs, scope)
                cls._faults = cls._dropped(cls._faults, scope)
            return held

        return boundary(HOOKS_RELEASE, retired, catch=KeyError)

    @classmethod
    def _attach(cls, point_id: HookId, member: Tap[Struct] | Veto[Struct], scope: ScopeKey) -> None:
        taps = cls._scoped(cls._taps, scope)
        held = taps.try_find(point_id).default_value(Block.empty()).append(Block.singleton(member))
        cls._taps = cls._taps.add(scope, taps.add(point_id, held))

    @classmethod
    def _detach(cls, point_id: HookId, member: Tap[Struct] | Veto[Struct], scope: ScopeKey) -> None:
        # identity filter, which is exactly why `Attachment` carries the ATTACHED member: a replay row attached its
        # barrier, so a holder handing back its own tap would retire nothing while reading as a clean release.
        with cls._gate:
            taps = cls._scoped(cls._taps, scope)
            survivors = taps.try_find(point_id).default_value(Block.empty()).filter(lambda held: held is not member)
            cls._taps = cls._taps.add(scope, taps.add(point_id, survivors))

    @overload
    @classmethod
    def subscribe[P: Struct](cls, points: HookId, tap: Tap[P] | Veto[P] | TapRow, *, scope: ScopeKey = ...) -> RuntimeRail[Attachment]: ...
    @overload
    @classmethod
    def subscribe[P: Struct](
        cls, points: Block[HookPoint[Struct]], tap: Tap[P] | Veto[P] | TapRow, *, scope: ScopeKey = ...
    ) -> RuntimeRail[Block[Attachment]]: ...

    @classmethod
    def subscribe[P: Struct](
        cls, points: HookId | Block[HookPoint[Struct]], tap: Tap[P] | Veto[P] | TapRow, *, scope: ScopeKey = DEFAULT_SCOPE
    ) -> RuntimeRail[Attachment] | RuntimeRail[Block[Attachment]]:
        # subscription reaches the grain registration reaches: one point id attaches to that point, a claimed roster
        # attaches the same tap across every row of it, and either way the answer is the DETACHER rather than a count
        # naming nothing a caller could retire. A built-in row binds ONCE here, so the roster fan shares one member
        # and one composition key. The roster arm is TRANSACTIONAL the way the retained drain below is — a refusal on
        # any row closes every attachment already opened, so a partial fan-out never stands where a producer asked for
        # a whole table — and it cannot be one gated swap the way `register` is, because a replay row's retained drain
        # runs outside the registry gate by construction.
        member: Tap[Struct] | Veto[Struct] = tap.bound(scope) if isinstance(tap, TapRow) else tap
        match points:
            case StrEnum() as point_id:
                return cls._subscribed(point_id, member, scope)
            case roster:
                held: Block[Attachment] = Block.empty()
                for point in roster:
                    match cls._subscribed(point.id, member, scope):
                        case Result(tag="ok", ok=attachment):
                            held = held.append(Block.singleton(attachment))
                        case Result(tag="error") as refused:
                            for standing in held:  # Exemption: unwind is the transactional half — a partial fan-out never stands
                                standing.close()
                            return refused
                return Ok(held)

    @classmethod
    def _subscribed(cls, point_id: HookId, member: Tap[Struct] | Veto[Struct], scope: ScopeKey) -> RuntimeRail[Attachment]:
        # the per-point attach, answering the detacher over the ATTACHED member: a replay row attaches its barrier
        # rather than the tap itself, so the value a caller brackets holds the object the identity filter can find.
        def attached() -> Attachment:
            with cls._gate:
                row = cls._scoped(cls._points, scope)[point_id]  # KeyError converts on the fence: an unregistered point refuses
                match row.modality:
                    case Modality(tag="replay", replay=depth):
                        if cls._declared_async(member):
                            raise TypeError("replay hook subscribers must be synchronous so attach drains retained facts before forward observation")
                        barrier = _ReplayAttach(member)
                        retained = cls._scoped(cls._rings, scope).try_find(point_id).default_value(Ring(cap=depth)).held
                        cls._attach(point_id, barrier, scope)
                    case _:
                        cls._attach(point_id, member, scope)
                        return Attachment(point=point_id, scope=scope, member=member)
            # the retained drain runs OUTSIDE the registry gate through the barrier, and attach is transactional:
            # `_sync_tap` raises on a tap fault or on the sync contract breached by a returned awaitable — a breach a
            # `_declared_async` probe cannot see — so the fault detaches the barrier before the fence rails the refusal.
            # An empty window still opens the barrier so the forward contract starts clean.
            try:
                for fact in retained:  # Exemption: un-fenced drain — a raise must reach the subscribe fence, detaching first
                    cls._sync_tap(member, point_id, fact)
                barrier.opened(lambda fact: cls._sync_tap(member, point_id, fact))
            except BaseException:
                cls._detach(point_id, barrier, scope)
                raise
            return Attachment(point=point_id, scope=scope, member=barrier)

        # the retained drain runs CALLER code outside the registry gate, so this fence names the two the barrier
        # itself raises beside the catch-all that covers a subscriber refusing mid-drain; the attach detaches first.
        return boundary(HOOKS_SUBSCRIBE, attached, catch=Exception)

    @classmethod
    def _counted[T](cls, point_id: HookId, prior: Ring[T], parked: Ring[T], scope: ScopeKey) -> None:
        # ONE recording site for BOTH lossy planes — the replay trim and the isolation window alike — reading the
        # ring's own movement rather than each plane counting for itself beside a trim it does not own. Runs OUTSIDE
        # `_gate` by law: a record crosses into the metric owner's instrument state, and holding the registry lock
        # across it would serialize every fire in the process behind one export-side write. A zero delta records
        # nothing, so the counter carries only real evictions and real sink refusals.
        shed, lost = parked.moved(prior)
        measures = ({HOOK_SHED: float(shed)} if shed else {}) | ({HOOK_LOST: float(lost)} if lost else {})
        if measures:
            Metrics.record(measures, domain=DOMAIN, kind=point_id.value, scope=scope)

    @classmethod
    def _point[P: Struct](cls, point_id: HookId, payload: P, scope: ScopeKey) -> RuntimeRail[HookPoint[Struct]]:
        def checked() -> HookPoint[Struct]:
            with cls._gate:
                row = cls._scoped(cls._points, scope)[point_id]
            if not isinstance(payload, row.payload):
                raise TypeError(f"hook {point_id!r} requires {row.payload.__name__}, received {type(payload).__name__}")
            return row

        return boundary(HOOKS_PAYLOAD, checked, catch=(KeyError, TypeError))

    @classmethod
    def _delivery[P: Struct](
        cls, point_id: HookId, payload: P, scope: ScopeKey
    ) -> RuntimeRail[tuple[HookPoint[Struct], Block[Tap[Struct] | Veto[Struct]]]]:
        match cls._point(point_id, payload, scope):
            case Result(tag="error") as refused:
                return refused
            case Result(tag="ok", ok=row):
                # the movement crosses the gate as a VALUE so the record lands outside it; a non-retaining point
                # answers `Nothing` and pays neither the pair nor the fold.
                moved: Option[tuple[Ring[Struct], Ring[Struct]]] = Nothing
                with cls._gate:
                    if row.modality.tag == "replay":
                        # the trim is the window's own arm now, so the oldest fact scrolling out lands on `shed` and a
                        # capsule reading a full window knows how much fell past it instead of reading it as whole.
                        rings = cls._scoped(cls._rings, scope)
                        prior = rings.try_find(point_id).default_value(Ring(cap=row.modality.replay))
                        parked = prior.park(payload)
                        cls._rings = cls._rings.add(scope, rings.add(point_id, parked))
                        moved = Some((prior, parked))
                    taps = cls._scoped(cls._taps, scope).try_find(point_id).default_value(Block.empty())
                moved.map(lambda pair: cls._counted(point_id, pair[0], pair[1], scope))
                return Ok((row, taps))

    @staticmethod
    def _veto_tap(veto: Tap[Struct] | Veto[Struct], point_id: HookId, payload: Struct) -> RuntimeRail[Struct]:
        # VETO admission, twin to `_sync_tap`'s below: a subscriber on a VETO point returns the payload
        # RAIL, and `bind` over a raw return hands the fold a value carrying no rail members at all — the NEXT
        # member's `bind` then raises outside every fence, straight into the emitter this registry exists to isolate.
        # Refusing here converts the breach on the fence, so a malformed veto REJECTS the payload, the one reading a
        # gate can safely take of a subscriber it could not run.
        returned = veto(point_id, payload)
        if isinstance(returned, Result):
            return returned
        raise TypeError(f"veto hook {point_id!r} must return the payload rail")

    @classmethod
    def _vetoed[P: Struct](cls, point_id: HookId, payload: P, taps: Block[Tap[Struct] | Veto[Struct]]) -> RuntimeRail[P]:
        return taps.fold(
            lambda rail, veto: rail.bind(lambda live: boundary(HOOKS_TAP, lambda: cls._veto_tap(veto, point_id, live), catch=Exception).bind(lambda r: r)),
            Ok(payload),
        )

    @classmethod
    def fire[P: Struct](cls, point_id: HookId, payload: P, *, scope: ScopeKey = DEFAULT_SCOPE) -> RuntimeRail[P]:
        match cls._delivery(point_id, payload, scope):
            case Result(tag="error") as refused:
                return refused
            case Result(tag="ok", ok=(HookPoint(modality=Modality(tag="veto")), taps)):
                return cls._vetoed(point_id, payload, taps)
            case Result(tag="ok", ok=(HookPoint(modality=Modality(tag="observe" | "replay")), taps)):
                for tap in taps:  # Exemption: the sequential tap walk is the isolation seam — each fault parks in the fault window, never out
                    cls._observed(point_id, tap, payload, scope)
                return Ok(payload)
            case Result(tag="ok", ok=(HookPoint(modality=unreachable), _)):
                # `Modality` proves total HERE, so a new case earns its arm above or breaks at this one
                # rather than falling through and handing a rail-typed caller `None` on the point it just fired.
                assert_never(unreachable)

    @classmethod
    async def fire_async[P: Struct](cls, point_id: HookId, payload: P, *, scope: ScopeKey = DEFAULT_SCOPE) -> RuntimeRail[P]:
        match cls._delivery(point_id, payload, scope):
            case Result(tag="error") as refused:
                return refused
            case Result(tag="ok", ok=(HookPoint(modality=Modality(tag="veto")), taps)):
                return cls._vetoed(point_id, payload, taps)
            case Result(tag="ok", ok=(HookPoint(modality=Modality(tag="observe" | "replay")), taps)):
                for tap in taps:
                    # Exemption: the sequential tap walk is the async fence seam — each fault parks in the fault window, never out.
                    async def awaited(tap: Tap[Struct] | Veto[Struct] = tap) -> object:
                        returned = tap(point_id, payload)
                        return await returned if isawaitable(returned) else returned

                    fenced = await async_boundary(HOOKS_TAP, awaited, catch=Exception)
                    fenced.swap().map(lambda fault: cls._isolated(point_id, fault, scope))
                return Ok(payload)
            case Result(tag="ok", ok=(HookPoint(modality=unreachable), _)):
                assert_never(unreachable)

    @classmethod
    def replayed(cls, *, scope: ScopeKey = DEFAULT_SCOPE) -> Map[HookId, Ring[Struct]]:
        # bundle-facing replay projection: every retaining point's window as data — each arrives pre-trimmed to its own
        # registered depth and carries what that trim shed, so the read is bounded by construction, mutates nothing,
        # and a capsule reads the retained facts beside the count of facts that scrolled past them.
        with cls._gate:
            points = cls._scoped(cls._points, scope)
            rings = cls._scoped(cls._rings, scope)
            return Map.of_seq(
                (point_id, rings.try_find(point_id).default_value(Ring(cap=row.modality.replay)))
                for point_id, row in points.items()
                if row.modality.tag == "replay"
            )

    @classmethod
    def faults(cls, *, scope: ScopeKey = DEFAULT_SCOPE) -> Ring[Receipt]:
        # bundle-facing isolation window, the twin read to `replayed`: the recent subscriber faults this composition
        # isolated, beside what the window evicted and what the receipt stream never carried. An empty window is a
        # composition whose taps never faulted, which is a different fact from a registry whose producer legs never ran.
        with cls._gate:
            return cls._faults.try_find(scope).default_value(Ring(cap=FAULT_WINDOW))

    @classmethod
    def _observed(cls, point_id: HookId, tap: Tap[Struct] | Veto[Struct], payload: Struct, scope: ScopeKey) -> None:
        # the tap runs behind the fence and its fault becomes parked evidence, so the emitter's value returns
        # untouched. No verdict drops here: `_isolated` carries the whole one — fenced emit, window park, counted
        # sink refusal — into the composition's fault window, which is where a reader collects it.
        boundary(HOOKS_TAP, lambda: cls._sync_tap(tap, point_id, payload), catch=Exception).swap().map(
            lambda fault: cls._isolated(point_id, fault, scope)
        )

    @staticmethod
    def _declared_async(tap: object) -> bool:
        return iscoroutinefunction(tap) or iscoroutinefunction(getattr(tap, "__call__", None))

    @staticmethod
    def _sync_tap(tap: Tap[Struct] | Veto[Struct], point_id: HookId, payload: Struct) -> object:
        returned = tap(point_id, payload)
        if not isawaitable(returned):
            return returned
        close = getattr(returned, "close", None)
        if callable(close):
            close()
        raise TypeError("synchronous hook delivery cannot consume an awaitable tap result")

    @classmethod
    def _isolated(cls, point_id: HookId, fault: BoundaryFault, scope: ScopeKey) -> Receipt:
        # isolation law: a subscriber fault is evidence, never a break in the emitter — and THE EMISSION RIDES ITS OWN
        # FENCE, because a closed stream or a refusing processor raising inside this fold would propagate out of the
        # tap walk and out of `fire` uncaught, destroying exactly the emitter value the isolation exists to protect.
        # The refusal is counted rather than swallowed: the evidence parks in this composition's window either way,
        # and `lost` names the faults the receipt stream never carried, which is the one loss no other plane can
        # reconstruct. Named for the ISOLATION it performs, not for the fault it carries — the
        # `reliability/faults#FAULT` `faulted` owner is the span-side Error-arm fold that statuses a live span, logs,
        # and hands the fault back to a rail, where this one opens no span and the emitter's rail stays `Ok` by law.
        # Two folds with no shared consumer and no shared behaviour, so one spelling over both would read as the
        # collapsed form and route a tap fault onto a span the fire never opened.
        evidence = Receipt.of(point_id, fault)
        emitted = boundary(HOOKS_ISOLATED, lambda: Signals.emit(evidence, OPEN, scope=scope), catch=Exception)
        with cls._gate:
            window = cls._faults.try_find(scope).default_value(Ring(cap=FAULT_WINDOW))
            parked = emitted.map(lambda _sunk: window.park(evidence)).default_with(lambda _refused: window.refused(evidence))
            cls._faults = cls._faults.add(scope, parked)
        cls._counted(point_id, window, parked, scope)
        return evidence
```

## [03]-[RESEARCH]

<!-- source-only: research row template; every landed row opens on the list dash this placeholder omits, the census reading `^- [TOKEN]-[STATUS]:` alone:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
