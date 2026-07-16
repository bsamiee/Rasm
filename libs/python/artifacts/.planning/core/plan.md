# [PY_ARTIFACTS_PLAN]

`ArtifactPipeline` is the one content-keyed production-planning-AND-scheduling owner over the runtime session lane — it folds a producer dependency graph into the `runtime/execution#LANE` admission shape AND schedules it. Each `ArtifactWork` node carries a producer's pre-minted `ContentKey`, its `Work[ArtifactReceipt]` coroutine, its parent content keys, its `cost` work-weight, and the closed `Admission` case it enters the lane on. One `rustworkx.PyDiGraph` over that content-key graph resolves the dependency-level fronts, one `Schedule.of` Critical-Path-Method pass settles every node's earliest/latest finish, and each front is min-slack-ordered before lowering so the lane admits the latency-critical producers first — the `cost` model DRIVING the order it only reported before. The plan executes nothing: it projects the producer graph into the runtime ports, resolves the front ORDER and the schedule, and surfaces its own topology, schedule, and coverage evidence while the `runtime/execution#LANE` `LanePolicy` owns the drive.

`[04]-[DEPENDENCY_POLICY]` forbids re-implementing an admitted dependency, so the DAG algebra is the Rust-core `rustworkx` surface end to end — `topological_generations` the fronts, `topological_sort` the CPM walk order, `digraph_find_cycle` the cycle gate, `ancestors` the target closure — never a hand-rolled `graphlib` drive or `tailrec` parent-walk. The synchronous graph kernel is GIL-releasing native CPU work, so `_compute` crosses onto an `anyio.to_thread` worker bounded by a module `CapacityLimiter`, never the event loop. The plan is the third reuse-fabric consumer of the one `core/receipt#RECEIPT` `contribute` fold, beside the runtime lane elision and the `MeterProvider` signal stream: it reads each producer's `ArtifactReceipt` as the content-keyed evidence the elision distinguishes hit from miss on, and emits its own `planned`-phase `Receipt` fact — the `core/receipt#SIGNALS [PLAN_FABRIC]` consumer obligation — carrying the topology and CPM shape onto that stream. `core/issue#ISSUE` is THE constructing owner that folds producers into `ArtifactPipeline.of` and drains the fronts through the runtime lane; the plan is constructed in exactly one place and imports no producer.

## [01]-[INDEX]

- [01]-[PLAN]: the content-keyed production-planning-and-scheduling axis folding the `ArtifactWork` producer graph into the `runtime/execution#LANE` admission shape, scheduling it by CPM, and surfacing the coverage evidence the lane does not own.

## [02]-[PLAN]

- Owner: `ArtifactPipeline` folds the `ArtifactWork` producer-node graph into `runtime/execution#LANE` admission units and schedules it. `ArtifactWork` is the frozen producer-node value object — a node IS its content key, a dependency IS a content-key edge, and its lane case IS one closed `Admission` member carrying exactly its payload, never a node id beside the key, never a path, never an `admission` tag beside a parallel `Option[RetryClass]` where the `retried` case already carries the class. `Admission` (`keyed`/`bare`/`retried`) selects which `Admit` case a node lowers to: `keyed` the cache-eligible default the elision is built on (payload the optional `KeyedThread` host-wire thread, `Admission(keyed=None)` the host-default spelling), `bare` a forced-live one-shot, `retried` a transient producer carrying its MANDATORY `RetryClass` so it states its class or is not retried. `PlanFault` (`dangling`/`cyclic`/`collided`/`untargeted`) is the closed coverage-failure vocabulary the `severed` verdict carries, each case holding its offending `frozenset[ContentKey]` so the root recovers on the cause rather than a `bool` collapsing all to "broken". `Schedule` is the CPM evidence computed ONCE over the in-set graph — a forward `(earliest_finish, hop)` pass and a backward `latest_finish` pass from which `duration`/`span`/`slack`/`critical` all PROJECT, so no metric re-walks the graph; the passes walk the native `topological_sort` order, but the per-node total-float memo stays own-code because `rustworkx.dag_weighted_longest_path` weighs EDGES and yields only the single longest path, carrying neither node weights nor per-node slack. `PipelinePlan` carries the front-ordered `tuple[Block[Admit], ...]`, the `requires` adjacency, the `schedule`, the `cache_seed`, the `threads` host-wire projection, and the `keys`/`dangling`/`elided`/`cyclic`/`collided`/`untargeted` coverage sets, with `profile`/`cardinality`/`depth`/`width`/`contention`/`live`/`roots`/`leaves`/`critical_path`/`span`/`makespan` deriving the front and schedule shape and `severed` the cause-carrying verdict the composition root reads before driving. `LanePolicy` is carried as ONE policy value (capacity + budget), never split into loose scalars, and the `Admit` union is consumed, never re-spelled.
- Cases: `_compute` is the one synchronous boundary kernel offloaded onto the runtime thread lane — it scopes the node set (`_scoped` when `targets` is non-empty), indexes `by_key`, builds the `requires` adjacency and `weights` map, derives `keys`/`dangling`/`elided`, captures each keyed node's `KeyedThread` into `threads`, captures `collided` from one `Counter` over the node keys (the double-claimed identities a last-wins `Map.of_seq` silently drops), narrows the graph through `_digraph`, gates the cycle through `digraph_find_cycle` (endpoints folding into `cyclic` and an empty-front plan `severed` refuses, never a raise), runs one `Schedule.of` settling the CPM passes, binds `priorities = schedule.slack` ONCE (the property rebuilds the map per access), and projects each `topological_generations` front through `Block.of_seq(front).sort_with(...)` min-slack-ordering before `Block.choose` lowers each node through its own `ArtifactWork.lowered` match. `_digraph` is the one graph-build kernel: keys are sorted by `ContentKey.hex` before `add_nodes_from` so the node-index assignment — and the within-generation front order every front inherits — is run-STABLE against salted-hash `frozenset` iteration, `get_node_data` is the index-to-key projection with no parallel bijection map, and `multigraph=False` folds a doubly-declared edge to one. `_scoped` is the `rustworkx.ancestors` target closure replacing the deleted `tailrec` parent-walk, keeping exactly the ancestor sub-graph a partial rebuild needs.
- Entry: `ArtifactPipeline.plan` is `async` over the runtime `async_boundary` and offloads the synchronous `rustworkx` kernel onto `anyio.to_thread.run_sync(self._compute, limiter=_PLAN_LANES)` so the GIL-releasing native sweep never runs on the event loop, returning a `RuntimeRail[PipelinePlan]` — the plan executes nothing, the runtime lane drives it front by front. One `ArtifactPipeline.of` owns every construction arity (a lone `ArtifactWork` or any `Iterable` normalized once at the head by input shape, never a `single`/`many` suffix pair); its keyword-only `targets` scopes the plan to the requested-deliverable ancestor closure (empty = whole graph), the discriminant the value of `targets` itself, and `_compute` measures `targets - keys` against the FULL node set so a requested target the producer set never mints surfaces as `untargeted` rather than a silently-empty plan.
- Auto: the producer-node graph is content-addressed end to end — each `ArtifactWork.key` is the `evidence/identity#IDENTITY` `ContentKey` the producer's `emit()` minted PRE-RUN over its frozen spec's canonical bytes ⊕ its parent keys, so `keyed` admission probes the threaded `cache_seed` and the lane short-circuits before `work` runs, and `_emit` threads the same key into its terminal receipt so `receipt.slot == node.key`. Each producer's `emit()` returns `ArtifactWork | Iterable[ArtifactWork]` — a batch producer one node per member for per-member elision, a composite ONE aggregate node whose `parents` are its member keys — so the plan re-mints no key and a settings change that shifts a content key correctly misses the cache. A parent `ContentKey` no node mints is captured once into `dangling` rather than silently absorbed as an already-done external node; two nodes minting one key into `collided`; a `keyed` node whose key already resolved into `elided` (the `Schedule.of` forward pass treats an `elided` hit as zero cost yet still one hop, so `critical_path`/`makespan` collapse correctly on a warm replay). Each node lowers through its own `lowered` match so the runtime lane owns the short-circuit, always-run, and transient retry by `Admit` case — the plan never probes a cache to DECIDE a drain — and the `cache_seed` is the only threaded state, a `DrainReceipt.cache` the runtime minted carried forward, never a durable store.
- Receipt: the plan contributes no `ArtifactReceipt` case — it is the CONSUMER of the one `core/receipt#RECEIPT` `contribute` fold every producer satisfies, reading each producer's `ArtifactReceipt` as the keyed evidence the sub-graph elision distinguishes a hit from a miss on. It emits its OWN `planned`-phase fact (the `core/receipt#SIGNALS [PLAN_FABRIC]` obligation): the module-level `_emitted` tap fires `Signals.emit(Receipt.of("artifacts", ("planned", "pipeline.plan", plan.facts)), OPEN)` once per plan over the owner-derived `PipelinePlan.facts` projection, native ints/floats serialized unstringified plus the `severed` cause tag. `_emitted` fires only on the resolved-plan arm so a coverage-severed plan still rides the rail as evidence; the plan owns no metric for the EXECUTED drain — the `runtime/execution#LANE` `@drained` aspect owns that.
- Packages: `rustworkx` (the producer digraph, `topological_generations` fronts, `topological_sort` CPM order, `digraph_find_cycle` gate, `ancestors` closure — the Rust-core DAG surface `[04]-[DEPENDENCY_POLICY]` mandates over a hand-rolled `graphlib` drive, deferred so its native load lands on first `_compute` under the import lock inside the `to_thread` worker); `anyio` (the thread lane offloading the GIL-releasing kernel, `CapacityLimiter` bounding it at `os.process_cpu_count()`); `expression` (`Block`/`Map` the front projection, the two CPM folds, the min-slack order, the `severed` cause verdict; `tagged_union` declaring `Admission`/`PlanFault` and consuming the imported `Admit` shape); `collections.Counter` (the `collided` double-claim surface); `msgspec` (`Struct` the value objects); `os` (`process_cpu_count` sizing the offload limiter); runtime (`identity.ContentKey`, `lanes.Admit`/`LanePolicy`/`Work`, `resilience.RetryClass`, `faults.RuntimeRail`/`async_boundary`, `receipts.Receipt`/`Signals`/`OPEN`); `core/receipt#RECEIPT` (`ArtifactReceipt` the `Work` payload type, consumed as keyed evidence, the producer modules never imported because the `Work` thunk carries the call).
- Growth: a new producer is one `ArtifactWork` node — `_digraph` places it, `topological_generations` fronts it, `Schedule.of` settles its CPM finish, and `_compute` min-slack-lowers it through its own `lowered` match with zero plan edit; a new admission modality is one `Admission` case plus one `lowered` arm (the `assert_never` tail proving total); a transient producer is one `retried` admission carrying its `RetryClass`; a new dependency is one parent `ContentKey`; a warm replay is one non-empty `cache_seed`; a new bounded-drain trait is one `LanePolicy` field; a new per-node weight axis is one `ArtifactWork` field reaching the `weights` map and the `Schedule` passes; a new front-shape or schedule metric is one derived `PipelinePlan` property or `Schedule` projection off the existing memo, never a re-walk; a new coverage-failure cause is one `PlanFault` case plus one `severed` arm and one `_compute` detection; a partial rebuild is one `targets` set on `of`.
- Boundary: the lane mechanics stay the runtime's — no cache, durable store, scheduler, drain, `CapacityLimiter` for the drain, `move_on_after`, `guard` binding, or `DrainReceipt` construction, since the `runtime/execution#LANE` `LanePolicy` owns the bounded drive, the front-to-front `DrainReceipt.cache` thread, and the `Admit`-case short-circuit/always-run/transient retry, and each sub-domain owns its own render. The lowering is the node's own `match`, never a hardcoded single `Admit.keyed` or a `_LOWER` dispatch table; a `retried` node states its class, never a `RetryClass.OCCT` default papering over an absent one; a `keyed` node with an internal offload-retry KEEPS `keyed` and rides `LanePolicy.offload(kernel, retry=...)` inside its own `_emit`. The coverage evidence is acted on, never inert: `dangling`/`collided` surface what a last-wins `Map.of_seq` silently drops, `severed` names WHICH of the four causes over a `bool`, and the COST-weighted `critical_path` is the serial-latency bound where an unweighted hop count is not. The CPM settles once (`Schedule.of`), never a per-access re-walk; the DAG algebra is `rustworkx`, never a `graphlib` drive or `tailrec` parent-walk; the kernel offloads onto `anyio.to_thread`, never the event loop; the `severed` scope refuses an unknown `untargeted` target rather than a silent empty plan; and the `Work` thunk carries each producer call, never a plan-side producer import. The Gate #3 host-wire thread rides `PipelinePlan.threads` (each keyed node's stated `KeyedThread`), the projection the AppHost boundary reads beside the drain receipts to mint the Persistence `Admit(kind, key, bytes, classification, at, sourceKey)` blob-index row — the runtime `Admit` shape UNCHANGED.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import os
from collections import Counter
from collections.abc import Iterable
from typing import Final, Literal, assert_never

from expression import Nothing, Option, Some, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct

from rasm.runtime.identity import ContentKey
from rasm.runtime.faults import RuntimeRail, async_boundary
from rasm.runtime.lanes import Admit, LanePolicy, Modality, Work
from rasm.runtime.receipts import OPEN, Receipt, Signals
from rasm.runtime.resilience import RetryClass

# deferred so the native load lands on first compute inside the `to_thread` kernel, under the import lock.
lazy import rustworkx as rx

from artifacts.core.receipt import ArtifactReceipt

# --- [TYPES] ----------------------------------------------------------------------------


class KeyedThread(Struct, frozen=True):
    # the Gate-#3 host-wire thread a keyed node states so the AppHost boundary mints the Persistence
    # `Admit(kind, key, bytes, classification, at, sourceKey)` row post-drain: `classification` the
    # `DataClassification` wire tag ("" = the host `NoDataClassification` default), `source` the
    # source-artifact key the index's `sourceKey` projection groups by.
    classification: str = ""
    source: ContentKey | None = None


# one case per `Admit` arm; `retried` carries its MANDATORY `RetryClass` so a transient node states its
# class or is not retried; `keyed` optionally carries the `KeyedThread` host-wire thread.
@tagged_union(frozen=True)
class Admission:
    tag: Literal["keyed", "bare", "retried"] = tag()
    keyed: KeyedThread | None = case()
    bare: None = case()
    retried: RetryClass = case()


# four distinct coverage-failure causes the root repairs differently, so the `severed` verdict is a tagged
# cause carrying its offending key set, never one `bool` collapsing all four with the cause erased.
@tagged_union(frozen=True)
class PlanFault:
    tag: Literal["dangling", "cyclic", "collided", "untargeted"] = tag()
    dangling: frozenset[ContentKey] = case()
    cyclic: frozenset[ContentKey] = case()
    collided: frozenset[ContentKey] = case()
    untargeted: frozenset[ContentKey] = case()


# --- [CONSTANTS] ------------------------------------------------------------------------

_DEFAULT_LANE: Final[LanePolicy] = LanePolicy(capacity=8)
# critical-chain membership is `<= _SLACK_EPS`, never `== 0.0` an accumulation-built `latest - finish`
# never lands on.
_SLACK_EPS: Final[float] = 1e-9
# offload throttle for the synchronous `rustworkx` kernel bounded on a GIL-releasing worker thread.

# --- [MODELS] ---------------------------------------------------------------------------


class ArtifactWork(Struct, frozen=True):
    key: ContentKey
    work: Work[ArtifactReceipt]
    parents: tuple[ContentKey, ...] = ()
    admission: Admission = Admission(keyed=None)
    # estimated work weight the CPM forward pass sums into earliest-finish; an `elided` hit contributes
    # zero yet still gates its dependents at one hop.
    cost: float = 1.0

    def lowered(self, /) -> Admit[ArtifactReceipt]:
        match self.admission:
            case Admission(tag="keyed"):
                return Admit(keyed=(self.key, self.work))
            case Admission(tag="bare"):
                return Admit(bare=self.work)
            case Admission(tag="retried", retried=cls):
                return Admit(retried=(cls, self.work))
            case _ as unreachable:
                assert_never(unreachable)


class Schedule(Struct, frozen=True):
    # CPM evidence computed ONCE over the in-set graph: a forward `(earliest_finish, hop)` fold and a
    # backward `latest_finish` fold from which `duration`/`span`/`slack`/`critical` PROJECT. rustworkx
    # owns the topology the passes walk, NOT the per-node total-float memo — `dag_weighted_longest_path`
    # weighs EDGES and yields one path, carrying neither node weights nor per-node slack — so the folds
    # stay own-code. The maps are empty for a cyclic plan `digraph_find_cycle` refuses before `of` runs.
    finish: Map[ContentKey, tuple[float, int]] = Map.empty()
    latest: Map[ContentKey, float] = Map.empty()
    work: float = 0.0

    @property
    def duration(self) -> float:
        # the cost-weighted longest chain — the serial-latency lower bound lane concurrency cannot
        # collapse, distinct from `span` the hop count and the front-count `depth`.
        return max((finish for finish, _ in self.finish.values()), default=0.0)

    @property
    def span(self) -> int:
        # longest chain by HOP COUNT, the structural counterpart to `duration`, distinct from `depth`.
        return max((hop for _, hop in self.finish.values()), default=0)

    @property
    def slack(self) -> Map[ContentKey, float]:
        # total float per node — the min-slack-first priority the plan orders each front by so the lane
        # admits the latency-critical producers first.
        return self.finish.map(lambda key, pair: self.latest[key] - pair[0])

    @property
    def critical(self) -> frozenset[ContentKey]:
        # the zero-slack chain — every producer whose delay slips the whole plan.
        return frozenset(key for key, (finish, _) in self.finish.items() if self.latest[key] - finish <= _SLACK_EPS)

    @staticmethod
    def of(graph: "rx.PyDiGraph[ContentKey, None]", weights: Map[ContentKey, float], elided: frozenset[ContentKey], /) -> "Schedule":
        cost = lambda key: 0.0 if key in elided else weights.try_find(key).default_value(1.0)
        order = tuple(rx.topological_sort(graph))  # NodeIndices, parents-before-children

        # forward pass, parents-first: `ef(k) = cost(k) + max(parent ef)`, so `finish` and hop depth land in one fold.
        def ahead(memo: Map[ContentKey, tuple[float, int]], idx: int, /) -> Map[ContentKey, tuple[float, int]]:
            key = graph.get_node_data(idx)
            parents = tuple(graph.get_node_data(parent) for parent in graph.predecessor_indices(idx))
            finish = cost(key) + max((memo[parent][0] for parent in parents), default=0.0)
            hop = 1 + max((memo[parent][1] for parent in parents), default=0)
            return memo.add(key, (finish, hop))

        finish = Block.of_seq(order).fold(ahead, Map.empty())
        duration = max((value for value, _ in finish.values()), default=0.0)

        # backward pass, sinks-first: `lf(k) = min(lf(child) - cost(child))`, a leaf defaulting to `duration`.
        def behind(memo: Map[ContentKey, float], idx: int, /) -> Map[ContentKey, float]:
            kids = tuple(graph.get_node_data(child) for child in graph.successor_indices(idx))
            return memo.add(graph.get_node_data(idx), min((memo[kid] - cost(kid) for kid in kids), default=duration))

        latest = Block.of_seq(reversed(order)).fold(behind, Map.empty())
        return Schedule(finish=finish, latest=latest, work=sum(cost(graph.get_node_data(idx)) for idx in order))


class PipelinePlan(Struct, frozen=True):
    # `fronts` the drive input (each already min-slack-ordered), `requires` the DAG-shape adjacency,
    # `schedule` the CPM evidence; every other field is coverage the runtime does NOT own and `severed` gates.
    fronts: tuple[Block[Admit[ArtifactReceipt]], ...]
    requires: Map[ContentKey, tuple[ContentKey, ...]] = Map.empty()
    schedule: Schedule = Schedule()
    lane: LanePolicy = _DEFAULT_LANE
    cache_seed: Map[ContentKey, ArtifactReceipt] = Map.empty()
    # host-wire projection: each keyed node's stated `KeyedThread`, read beside the drain receipts so the
    # AppHost boundary mints the Persistence blob-index row without re-probing the node set.
    threads: Map[ContentKey, KeyedThread] = Map.empty()
    keys: frozenset[ContentKey] = frozenset()
    dangling: frozenset[ContentKey] = frozenset()
    elided: frozenset[ContentKey] = frozenset()
    cyclic: frozenset[ContentKey] = frozenset()
    collided: frozenset[ContentKey] = frozenset()
    untargeted: frozenset[ContentKey] = frozenset()

    @property
    def profile(self) -> tuple[int, ...]:
        return tuple(len(front) for front in self.fronts)

    @property
    def cardinality(self) -> int:
        return sum(self.profile)

    @property
    def depth(self) -> int:
        return len(self.fronts)

    @property
    def width(self) -> int:
        return max(self.profile, default=0)

    @property
    def contention(self) -> int:
        # peak over-subscription the `CapacityLimiter` QUEUES rather than runs — the hidden serialization
        # the front structure conceals, complementary to the `makespan` area bound.
        return max(self.width - self.lane.capacity, 0)

    @property
    def live(self) -> int:
        # units that reach a coroutine: total minus the `elided` cache hits, the saving the plan exists for.
        return self.cardinality - len(self.elided)

    @property
    def critical_path(self) -> float:
        return self.schedule.duration

    @property
    def span(self) -> int:
        return self.schedule.span

    @property
    def makespan(self) -> float:
        # bounded-capacity wall-clock floor (Graham's area bound): no schedule on `lane.capacity` workers
        # beats `max(critical_path, work / capacity)`, so a wide cheap plan is work-bound, a deep one path-bound.
        return max(self.schedule.duration, self.schedule.work / self.lane.capacity) if self.lane.capacity else self.schedule.duration

    @property
    def adjacency(self) -> Map[ContentKey, tuple[ContentKey, ...]]:
        # in-set predecessor projection `roots`/`leaves` read — `requires ∩ keys`, dropping the `dangling`
        # out-of-set edge; recomputed per access (a frozen `Struct` carries no `cached_property` slot).
        return self.requires.map(lambda _key, parents: tuple(parent for parent in parents if parent in self.keys))

    @property
    def roots(self) -> frozenset[ContentKey]:
        # source artifacts — minted nodes with no in-set parent; a node whose every parent is `dangling` is still a root.
        return frozenset(key for key in self.keys if not self.adjacency.try_find(key).default_value(()))

    @property
    def leaves(self) -> frozenset[ContentKey]:
        # terminal deliverables — minted nodes no in-set node depends on, the outputs a `targets`-scoped plan converges on.
        return self.keys - frozenset(parent for parents in self.adjacency.values() for parent in parents)

    @property
    def severed(self) -> Option[PlanFault]:
        # the gating cause in repair-priority order: `cyclic` (schedule undefined) before `untargeted`
        # (scope wrong) before `collided` (a double-claimed identity dropped) before `dangling` (missing upstream).
        if self.cyclic:
            return Some(PlanFault(cyclic=self.cyclic))
        if self.untargeted:
            return Some(PlanFault(untargeted=self.untargeted))
        if self.collided:
            return Some(PlanFault(collided=self.collided))
        return Some(PlanFault(dangling=self.dangling)) if self.dangling else Nothing

    @property
    def facts(self) -> dict[str, object]:
        # the owner-derived planned-phase projection the emit tap carries: native int/float coverage + CPM
        # shape serialized unstringified, with the full coverage set beside `severed`'s gating-cause tag (or `"ok"`).
        return {
            "cardinality": self.cardinality,
            "live": self.live,
            "depth": self.depth,
            "width": self.width,
            "contention": self.contention,
            "critical_path": self.critical_path,
            "makespan": self.makespan,
            "span": self.span,
            "critical": len(self.schedule.critical),
            "elided": len(self.elided),
            "dangling": len(self.dangling),
            "cyclic": len(self.cyclic),
            "collided": len(self.collided),
            "untargeted": len(self.untargeted),
            "roots": len(self.roots),
            "leaves": len(self.leaves),
            "severed": self.severed.map(lambda fault: fault.tag).default_value("ok"),
        }


# --- [OPERATIONS] -----------------------------------------------------------------------


def _digraph(nodes: Block[ArtifactWork], keys: frozenset[ContentKey], /) -> "rx.PyDiGraph[ContentKey, None]":
    # each unique `ContentKey` a node whose payload IS the key (`get_node_data` the index->key projection),
    # each in-set `parents` edge a `parent -> child` arc, `multigraph=False` folding a doubly-declared edge
    # to one. Keys sort by `ContentKey.hex` before `add_nodes_from`, NOT by raw salted-hash `frozenset`
    # iteration, so the node-index assignment and the within-generation front order are run-STABLE.
    order = tuple(sorted(keys, key=lambda key: key.hex))
    graph: rx.PyDiGraph[ContentKey, None] = rx.PyDiGraph(check_cycle=False, multigraph=False)
    index = dict(zip(order, graph.add_nodes_from(order), strict=True))
    for node in nodes:
        child = index[node.key]
        for parent in node.parents:
            if parent in keys:
                graph.add_edge(index[parent], child, None)
    return graph


def _scoped(nodes: Block[ArtifactWork], targets: frozenset[ContentKey], /) -> Block[ArtifactWork]:
    # requested-deliverable closure via `rustworkx.ancestors`: union each in-set target's ancestor set with
    # the target and keep that sub-graph. Every kept node's parents are ancestors, so scoping never
    # spuriously fires `dangling`; an empty closure yields the empty scope `_compute`'s `untargeted` refuses.
    keys = frozenset(node.key for node in nodes)
    graph = _digraph(nodes, keys)
    index = {graph.get_node_data(idx): idx for idx in graph.node_indices()}
    seeds = frozenset(index[target] for target in targets if target in index)
    closure = frozenset(graph.get_node_data(idx) for seed in seeds for idx in {seed, *rx.ancestors(graph, seed)})
    return nodes.filter(lambda node: node.key in closure)


def _emitted(plan: PipelinePlan, /) -> PipelinePlan:
    # resolved-plan tap firing the one `planned`-phase fact over the `PipelinePlan.facts` owner derivation,
    # returning the plan unchanged — so a coverage-severed plan still rides the rail rather than a blind emit.
    Signals.emit(Receipt.of("artifacts", ("planned", "pipeline.plan", plan.facts)), OPEN)
    return plan


class ArtifactPipeline(Struct, frozen=True):
    nodes: Block[ArtifactWork]
    lane: LanePolicy = _DEFAULT_LANE
    # elision seed `of(warm=)` threads — in-session receipts, the durable cross-run fill the AppHost
    # py↔Persistence boundary filling `warm` from the Persistence artifact index.
    cache_seed: Map[ContentKey, ArtifactReceipt] = Map.empty()
    # `of` retains `targets` unscoped (empty = whole graph); `_compute` scopes to the `ancestors` closure and
    # surfaces a target the producer set never mints as `untargeted` (`targets - full keys`), not a silent drop.
    targets: frozenset[ContentKey] = frozenset()

    async def plan(self, /) -> RuntimeRail[PipelinePlan]:
        # `_emitted` fires the `planned`-phase fact only on the resolved-plan arm, so a coverage-severed
        # plan still rides the rail rather than a blind emit.
        railed = await async_boundary("pipeline.plan", self._planned)
        return railed.map(_emitted)

    async def _planned(self, /) -> PipelinePlan:
        # the GIL-releasing `rustworkx` kernel crosses onto the runtime thread lane, never the event loop.
        return (await LanePolicy.offload(self._compute, modality=Modality.THREAD)).default_with(_plan_raise)


def _plan_raise(fault: object) -> "StagePlanned":
    # terminal collapse at the plan boundary: an offload fault reconstructs the raise the capsule folds.
    raise ValueError(str(fault))

    def _compute(self, /) -> PipelinePlan:
        scoped = self.nodes if not self.targets else _scoped(self.nodes, self.targets)
        by_key: Map[ContentKey, ArtifactWork] = Map.of_seq((node.key, node) for node in scoped)
        requires: Map[ContentKey, tuple[ContentKey, ...]] = Map.of_seq((node.key, node.parents) for node in scoped)
        weights: Map[ContentKey, float] = Map.of_seq((node.key, node.cost) for node in scoped)
        keys = frozenset(node.key for node in scoped)
        dangling = frozenset(parent for node in scoped for parent in node.parents) - keys
        # target-axis counterpart of `dangling`, measured against the FULL set: a requested deliverable no
        # node mints, surfaced rather than silently scoped into an empty plan.
        untargeted = self.targets - frozenset(node.key for node in self.nodes)
        # `Map.of_seq` is last-wins, so two nodes minting one key drop one producer silently — `Counter`
        # surfaces the double-claimed keys as `collided` evidence `severed` refuses.
        collided = frozenset(key for key, count in Counter(node.key for node in scoped).items() if count > 1)
        # a `keyed` node whose key already resolved into the threaded cache replays instead of re-rendering.
        elided = frozenset(node.key for node in scoped if node.admission.tag == "keyed") & frozenset(self.cache_seed.keys())
        # the Gate-#3 host-wire projection: only keyed nodes that STATED a thread ride it; `None`-threaded
        # keyed nodes admit under the host default and mint no row here.
        threads: Map[ContentKey, KeyedThread] = Map.of_seq(
            (node.key, node.admission.keyed) for node in scoped if node.admission.tag == "keyed" and node.admission.keyed is not None
        )
        graph = _digraph(scoped, keys)
        if cycle := rx.digraph_find_cycle(graph):
            # `digraph_find_cycle` returns the offending `EdgeList` (empty = acyclic) — richer than a
            # `graphlib.CycleError` node chain; its endpoints fold into `cyclic` evidence and `Schedule.of`
            # never runs on an un-orderable graph, so the rail carries a structured plan `severed` refuses.
            cyclic = frozenset(graph.get_node_data(idx) for edge in cycle for idx in edge)
            return PipelinePlan(
                fronts=(),
                requires=requires,
                lane=self.lane,
                cache_seed=self.cache_seed,
                threads=threads,
                keys=keys,
                dangling=dangling,
                elided=elided,
                cyclic=cyclic,
                collided=collided,
                untargeted=untargeted,
            )
        schedule = Schedule.of(graph, weights, elided)
        # min-slack priority read ONCE (the property rebuilds the map per access); each front sorts ascending
        # by slack so the lane admits the critical producers first — the cost model DRIVING the order.
        priorities = schedule.slack
        # `topological_generations` yields the dependency-level fronts directly, each front's indices lowering
        # through `get_node_data -> by_key -> lowered`, a `dangling` index dropping through `try_find`.
        fronts = tuple(
            Block
            .of_seq(front)
            .sort_with(lambda idx: (priorities.try_find(graph.get_node_data(idx)).default_value(0.0), graph.get_node_data(idx).hex))
            .choose(lambda idx: by_key.try_find(graph.get_node_data(idx)).map(lambda node: node.lowered()))
            for front in rx.topological_generations(graph)
        )
        return PipelinePlan(
            fronts=fronts,
            requires=requires,
            schedule=schedule,
            lane=self.lane,
            cache_seed=self.cache_seed,
            threads=threads,
            keys=keys,
            dangling=dangling,
            elided=elided,
            collided=collided,
            untargeted=untargeted,
        )

    @staticmethod
    def of(
        works: ArtifactWork | Iterable[ArtifactWork],
        *,
        lane: LanePolicy = _DEFAULT_LANE,
        warm: Map[ContentKey, ArtifactReceipt] = Map.empty(),
        targets: frozenset[ContentKey] = frozenset(),
    ) -> "ArtifactPipeline":
        nodes = Block.singleton(works) if isinstance(works, ArtifactWork) else Block.of_seq(works)
        return ArtifactPipeline(nodes=nodes, lane=lane, cache_seed=warm, targets=targets)
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
