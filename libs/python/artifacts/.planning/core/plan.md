# [PY_ARTIFACTS_PLAN]

`ArtifactPipeline` is the one content-keyed production-planning-AND-scheduling owner over the runtime session lane — it folds a producer dependency graph into the `runtime/execution/lanes#LANE` admission shape AND schedules it. Each `ArtifactWork` node carries a producer's pre-minted `ContentKey`, its `Work[ArtifactReceipt]` coroutine, its parent content keys, its `cost` work-weight, and the closed `Admission` case it enters the lane on. One `rustworkx.PyDiGraph` over that content-key graph resolves the dependency-level fronts, one `Schedule.of` Critical-Path-Method pass settles every node's earliest/latest finish, and each front is min-slack-ordered before lowering so the lane admits the latency-critical producers first — the `cost` model DRIVING the order it only reported before. `ArtifactPipeline` executes no producer: it projects the producer graph into the runtime ports, resolves the front order and the schedule, and surfaces its own topology, schedule, and coverage evidence while the `runtime/execution/lanes#LANE` `LanePolicy` owns the drive.

`[04]-[DEPENDENCY_POLICY]` forbids re-implementing an admitted dependency, so the DAG algebra is the Rust-core `rustworkx` surface end to end — `topological_generations` the fronts, `topological_sort` the CPM walk order, `digraph_find_cycle` the cycle gate, `ancestors` the target closure, `node_link_json` the introspection wire — never a hand-rolled `graphlib` drive or `tailrec` parent-walk. `_compute`'s synchronous graph kernel is GIL-releasing native CPU work, so `plan` crosses it through the runtime lane's thread arm — `self.lane.offload(Kernel.of(self._compute, KernelTrait.RELEASING))`, `THREAD_BAND`-bounded and deadline-scoped by the lane, never the event loop and never a folder-minted limiter beside the runtime band. This plan is the third reuse-fabric consumer of the one `core/receipt#RECEIPT` `contribute` fold, beside the runtime lane elision and the runtime `Metrics` metric stream: it reads each producer's `ArtifactReceipt` as the content-keyed evidence the elision distinguishes hit from miss on, and emits its own `planned`-phase `Receipt` fact — the `core/receipt#SIGNALS [PLAN_FABRIC]` consumer obligation — carrying the topology and CPM shape onto that stream. `core/issue#ISSUE` is THE constructing owner that folds producers into `ArtifactPipeline.of`, reads `severed` before driving, and drains the fronts through the runtime lane; the plan is constructed in exactly one place and imports no producer.

## [01]-[INDEX]

- [02]-[PLAN]: the content-keyed production-planning-and-scheduling axis folding the `ArtifactWork` producer graph into the `runtime/execution/lanes#LANE` admission shape, scheduling it by CPM, and surfacing the coverage evidence the lane does not own.

## [02]-[PLAN]

- Owner: `ArtifactPipeline` folds the `ArtifactWork` producer-node graph into `runtime/execution/lanes#LANE` admission units and schedules it. `ArtifactWork` is the frozen producer-node value object — a node IS its content key, a dependency IS a content-key edge, and its lane case IS one closed `Admission` member carrying exactly its payload, never a node id beside the key, never a path, never an `admission` tag beside a parallel `Option[RetryClass]` where the `retried` case already carries the class. `Admission` (`keyed`/`bare`/`retried`) selects which `Admit` case a node lowers to: `keyed` the cache-eligible default the elision is built on (payload the optional `KeyedThread` host-wire thread, `Admission(keyed=None)` the host-default spelling), `bare` a forced-live one-shot, `retried` a transient producer carrying its MANDATORY `RetryClass` so it states its class or is not retried. `Fed[P]` is the PARENT-PRODUCT leg on `ArtifactWork`, parameterized by the product its producing and consuming pair agrees on: a fed node's `build` receives its parents' staged in-memory products typed `tuple[P, ...]` (or `None` when any is absent — the elided-parent norm) and returns work yielding its receipt plus its own optional staged product, so a fan of siblings over one heavy intermediate folds it ONCE instead of once per node and a mismatched pair breaks at type-check rather than at the first drain; `ProductLedger` is the drain-scoped keyed exchange the lowering closes over — consumer counts derive from the plan's own adjacency, a product frees on its last take, and the runtime lane never learns products exist because the `Admit` payload stays a plain thunk. The ledger stays heterogeneous behind the one `Fed[Any]` boundary the node field already is, so the type states the pair's contract without forcing a per-product ledger. Products are ACCELERATION only: keys, receipts, and elision remain the sole truth, and every fed node carries a self-sufficient arm. `ProductFact` and `ProductSink[F]` are the STORE-facing counterpart the same node owner declares: a produced FILE crosses as one flat all-native fact — key, publication `root`, leaf, payload, container, producing tool with its leg version, and the producer's own receipt-band preimage — through a caller-threaded sink parameterized by the producer's fault family, so every plane that writes bytes threads one port instead of minting a kind-named, kind-fault-typed twin, and `root` is a name the producer states where a boolean environment split collapses two roots for one plane and answers nothing for the rest. `PlanFault` (`dangling`/`cyclic`/`collided`/`untargeted`) is the closed coverage-failure vocabulary the `severed` verdict carries, each case holding its offending `frozenset[ContentKey]` — projected uniformly through the `keys` or-pattern property — so the root recovers on the cause rather than a `bool` collapsing all to "broken". `Schedule` is the CPM evidence computed ONCE over the in-set graph — a forward `(earliest_finish, hop)` pass and a backward `latest_finish` pass from which `duration`/`span`/`slack`/`critical` all PROJECT, so no metric re-walks the graph; the passes walk the native `topological_sort` order, but the per-node total-float memo stays own-code because `rustworkx.dag_weighted_longest_path` weighs EDGES and yields only the single longest path, carrying neither node weights nor per-node slack. `PipelinePlan` carries the front-ordered `tuple[Block[Admit], ...]`, the `requires` adjacency, the `schedule`, the `cache_seed`, the `threads` host-wire projection, and the `keys`/`dangling`/`elided`/`cyclic`/`collided`/`untargeted` coverage sets. `cardinality` and `live` derive from `keys` so a cyclic plan preserves its node count despite having no runnable front; `profile`/`depth`/`width`/`contention` derive the fronts, `roots`/`leaves` derive adjacency, and `critical_path`/`span`/`makespan` derive the schedule. `severed` is the cause-carrying verdict the composition root reads before driving, and `node_link` is the self-describing build-DAG wire (`rustworkx.node_link_json` with mandatory `node_attrs` carrying each node's `key`/`state`/`slack`/`critical` evidence) a `visualization/diagram/layout#LAYOUT` `NODE_LINK` consumer renders so an issue run explains which sub-graphs elided and where the critical chain sits. `LanePolicy` is carried as ONE policy value (`slots` + `deadline`), never split into loose scalars; this scheduling projection alone reads `slots.total`, while an executing kernel receives only a `LaneGrant.width`. The `Admit` union is consumed, never re-spelled.
- Law: `ProductSink` is awaitable and returns the exact generated `ArtifactRef` produced by the one shared `ArtifactTransfer` lifecycle. A producer retains that value in its receipt before assembly; it never reconstructs artifact identity or extent from a band, `ContentKey`, file name, or store response.
- Cases: `_compute` is the one synchronous boundary kernel offloaded onto the runtime thread lane — it scopes the node set (`_scoped` when `targets` is non-empty), indexes `by_key`, builds the `requires` adjacency and `weights` map, derives `keys`/`dangling`/`elided`, captures each keyed node's `KeyedThread` into `threads`, captures `collided` from one `Counter` over the node keys (the double-claimed identities a last-wins `Map.of_seq` silently drops), narrows the graph through `_digraph`, gates the cycle through `digraph_find_cycle` (endpoints folding into `cyclic`, which collapses `schedule` and `fronts` to their empty forms inside the ONE `PipelinePlan` construction so `severed` refuses, never a raise or a second construction arm), runs one `Schedule.of` on the acyclic path, binds `priorities = schedule.slack` ONCE (the property rebuilds the map per access), and projects each `topological_generations` front through `Block.of_seq(front).sort_with(...)` min-slack-ordering before `Block.choose` lowers each node through its own `ArtifactWork.lowered` match. `_digraph` is the one `ArtifactPipeline` graph-build kernel over `(keys, edges)` — `_compute`, `_scoped`, and `PipelinePlan.node_link` all build through it: keys sort by `ContentKey.hex` before `add_nodes_from` so the node-index assignment and inherited within-generation order stay stable against salted-hash `frozenset` iteration, `add_edges_from_no_data` loads the indexed dependency feed as one package operation, `get_node_data` is the index-to-key projection with no parallel bijection map, and `multigraph=False` folds a doubly-declared edge to one. `_scoped` is the owned `rustworkx.ancestors` target closure replacing the deleted `tailrec` parent-walk, keeping exactly the ancestor sub-graph a partial rebuild needs.
- Entry: `ArtifactPipeline.plan` crosses the synchronous `rustworkx` kernel through the runtime lane's thread arm — `self.lane.offload(Kernel.of(self._compute, KernelTrait.RELEASING))` returns the `RuntimeRail[PipelinePlan]` with the lane's `async_boundary`, `THREAD_BAND` bound, and deadline scope already woven, so no second boundary, limiter, or raw `anyio.to_thread` call stands beside the runtime-owned crossing — the plan executes no producer, and the runtime lane drives it front by front. One `ArtifactPipeline.of` owns every construction arity (a lone `ArtifactWork` or any `Iterable` normalized once at the head by input shape, never a `single`/`many` suffix pair); its keyword-only `targets` scopes the plan to the requested-deliverable ancestor closure (empty = whole graph), the discriminant the value of `targets` itself, and `_compute` measures `targets - keys` against the FULL node set so a requested target the producer set never mints surfaces as `untargeted` rather than a silently-empty plan.
- Auto: the producer-node graph is content-addressed end to end — each `ArtifactWork.key` is the `runtime/evidence/identity#IDENTITY` `ContentKey` the producer's `emit()` minted PRE-RUN over its frozen spec's canonical bytes ⊕ its parent keys, so `keyed` admission probes the threaded `cache_seed` and the lane short-circuits before `work` runs, and `_emit` threads the same key into its terminal receipt so `receipt.slot == node.key`. Each producer's `emit()` returns `ArtifactWork | Iterable[ArtifactWork]` — a batch producer one node per member for per-member elision, a composite ONE aggregate node whose `parents` are its member keys — so the plan re-mints no key and a settings change that shifts a content key correctly misses the cache. A parent `ContentKey` no node mints is captured once into `dangling` rather than silently absorbed as an already-done external node; two nodes minting one key into `collided`; a `keyed` node whose key already resolved into `elided` (the `Schedule.of` forward pass treats an `elided` hit as zero cost yet still one hop, so `critical_path`/`makespan` collapse correctly on a warm replay). Each node lowers through its own `lowered` match so the runtime lane owns the short-circuit, always-run, and transient retry by `Admit` case — the plan never probes a cache to DECIDE a drain — and the `cache_seed` is the only threaded state, a `DrainReceipt.cache` the runtime minted carried forward, never a durable store.
- Receipt: the plan contributes no `ArtifactReceipt` case — it is the consumer of the one `core/receipt#RECEIPT` `contribute` fold every producer satisfies, reading each producer's `ArtifactReceipt` as the keyed evidence the sub-graph elision distinguishes a hit from a miss on. `ArtifactPipeline._emitted` fires `Signals.emit(Receipt.of("artifacts", ("planned", "pipeline.plan", plan.facts)), OPEN)` once per resolved plan over the owner-derived `PipelinePlan.facts` projection, with native numeric facts plus the `severed` cause tag. A coverage-severed plan remains evidence on the rail; the plan owns no metric for the executed drain because the `runtime/execution/lanes#LANE` `@drained` aspect owns that. `_metered` is the durable half beside that line — one `MeterFact` charging `Resource.COMPUTE` for the settled CPM makespan, recorded at the awaitable `plan` fold under the `runtime/observability/journal#LEDGER` producer-seam law. `cost` is therefore a MILLISECOND work-weight, the unit that resource's series declares; an elided node weighs zero, so a warm replay charges nothing and the executed drive is charged by nobody twice. No audit fact rides here — a plan is a projection of nodes a producer already attests, and this owner mints no artifact.
- Packages: `rustworkx` (the producer digraph, `topological_generations` fronts, `topological_sort` CPM order, `digraph_find_cycle` gate, `ancestors` closure, `node_link_json` introspection wire — the Rust-core DAG surface `[04]-[DEPENDENCY_POLICY]` mandates over a hand-rolled `graphlib` drive, deferred so its native load lands on first `_compute` inside the THREAD offload under the import lock); `expression` (`Block`/`Map` the front projection, the two CPM folds, the min-slack order, the `severed` cause verdict; `tagged_union` declaring `Admission`/`PlanFault` and consuming the imported `Admit` shape; `Result` typing the `ProductSink` return); `collections.Counter` (the `collided` double-claim surface); the builtin `frozendict` (the `ProductFact` band — msgspec-native and hashable where `Map` is not); `msgspec` (`Struct` the value objects); runtime (`identity.ContentKey`, `lanes.Admit`/`LanePolicy`/`Work`, `workers.Kernel`/`KernelTrait` — the lane's `offload` owning the thread crossing, its band, and its boundary, `resilience.RetryClass`, `faults.RuntimeRail`, `receipts.Receipt`/`Signals`/`OPEN`, `journal.Journal`/`MeterFact`/`Resource` the durable compute charge); `core/receipt#RECEIPT` (`ArtifactReceipt` the `Work` payload type, consumed as keyed evidence, the producer modules never imported because the `Work` thunk carries the call).
- Growth: a new producer is one `ArtifactWork` node — `_digraph` places it, `topological_generations` fronts it, `Schedule.of` settles its CPM finish, and `_compute` min-slack-lowers it through its own `lowered` match with zero plan edit; a new admission modality is one `Admission` case plus one `lowered` arm (the `assert_never` tail proving total); a transient producer is one `retried` admission carrying its `RetryClass`; a shared heavy intermediate is one `Fed[P]` leg on the producing and consuming nodes naming their agreed product — the ledger, the fronts, and the lane are untouched; a plane that writes files is one `ProductSink[F]` field threaded at its own composition, its faults its own and the port unedited; a new produced-file fact is one `ProductFact` field every sink reads or one `band` key no sink is edited for; a new publication root is one `root` string the producer names; a new dependency is one parent `ContentKey`; a warm replay is one non-empty `cache_seed`; a new bounded-drain trait is one `LanePolicy` field; a new per-node weight axis is one `ArtifactWork` field reaching the `weights` map and the `Schedule` passes; a new front-shape or schedule metric is one derived `PipelinePlan` property or `Schedule` projection off the existing memo, never a re-walk; a new introspection attribute is one `node_link` `node_attrs` entry; a new coverage-failure cause is one `PlanFault` case plus one `keys` or-pattern alternative plus one `severed` arm and one `_compute` detection; a partial rebuild is one `targets` set on `of`.
- Boundary: the lane mechanics stay the runtime's — no cache, durable store, scheduler, drain, `CapacityLimiter` for the drain, `move_on_after`, `guard` binding, or `DrainReceipt` construction, since the `runtime/execution/lanes#LANE` `LanePolicy` owns the bounded drive, the front-to-front `DrainReceipt.cache` thread, and the `Admit`-case short-circuit/always-run/transient retry, and each sub-domain owns its own render. The `ProductSink[F]` port is DECLARED here and bound nowhere: no store, no path algebra, no filesystem, and no fault vocabulary of its own, because the port's whole purpose is that the producer names its fault family and the composition root names the store; a `ProductFact` carries native scalars and the producer's band alone, so no producer value object, codec surface, or `ContentKey` mint crosses the port, and a kind-named sink minted beside it is the deleted form. Lowering is the node's own `match`, never a hardcoded single `Admit.keyed` or a `_LOWER` dispatch table; a `retried` node states its class, never a `RetryClass.OCCT` default papering over an absent one; a `keyed` node with a transient-prone internal offload KEEPS `keyed` and lets its kernel's trait row carry the worker-death retry inside its own `_emit` — a call-site `retry=` override is earned only by a death class the trait row genuinely misnames, never by an engine-transient convention. Coverage evidence is acted on, never inert: `dangling`/`collided` surface what a last-wins `Map.of_seq` silently drops, `severed` names WHICH of the four causes over a `bool`, and the COST-weighted `critical_path` is the serial-latency bound where an unweighted hop count is not. CPM settles once (`Schedule.of`), never a per-access re-walk; the DAG algebra is `rustworkx`, never a `graphlib` drive or `tailrec` parent-walk; the kernel crosses through `self.lane.offload`, never the event loop, a raw `anyio.to_thread` call, or a folder-minted limiter; `rx.lexicographical_topological_sort` is the rejected front form — it yields one tie-broken linear order, not dependency fronts, so the front projection stays `topological_generations` plus the per-front min-slack `sort_with`; the `severed` scope refuses an unknown `untargeted` target rather than a silent empty plan; and the `Work` thunk carries each producer call, never a plan-side producer import. Gate #3's host-wire thread rides `PipelinePlan.threads` (each keyed node's stated `KeyedThread`), the projection the AppHost boundary reads beside the drain receipts to mint the Persistence `Admit(kind, key, bytes, classification, at, sourceKey)` blob-index row — the runtime `Admit` shape UNCHANGED.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections import Counter
from collections.abc import Awaitable, Callable, Iterable
from typing import Any, Final, Literal, Self, assert_never

from builtins import frozendict
from expression import Error, Nothing, Option, Result, Some, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct

from rasm.artifacts.core.receipt import ArtifactReceipt
from rasm.contracts.gen.rasm.contracts.artifact.v1.artifact_pb import ArtifactRef
from rasm.runtime.faults import RuntimeRail
from rasm.runtime.identity import ContentKey
from rasm.runtime.journal import Journal, MeterFact, Resource
from rasm.runtime.lanes import Admit, LanePolicy, Work
from rasm.runtime.receipts import OPEN, Receipt, Signals
from rasm.runtime.resilience import RetryClass
from rasm.runtime.workers import Kernel, KernelTrait

lazy import rustworkx as rx

# --- [TYPES] ----------------------------------------------------------------------------

# The in-memory intermediate a producing node stages for its children — opaque to the plan (the consuming pair
# types it), alive for one drain, never a receipt and never durable state.
type Product = object

# Payload-agnostic egress port every producing plane threads: one produced FILE crosses as a `ProductFact`, the
# awaitable app-root binding publishes through `ArtifactTransfer`, and its exact generated `ArtifactRef` returns on
# the producer's own fault rail. This plan declares the port and binds none; no kind-specific transport rail exists.
type ProductSink[F] = Callable[["ProductFact"], Awaitable[Result[ArtifactRef, F]]]


class KeyedThread(Struct, frozen=True):
    classification: str = ""
    source: ContentKey | None = None


class Fed[P](Struct, frozen=True):
    # PARENT-PRODUCT leg, parameterized by the product the producing and consuming pair AGREES on, so a node
    # whose parent stages a deep plane and whose `build` expects a frame breaks at type-check rather than at the
    # first drain. `build` receives the staged parent products IN `parents` ORDER when every parent staged one, or
    # `None` when any is absent — an elided parent's thunk never ran, so absence is the warm-replay norm, and a fed
    # node therefore always carries a SELF-SUFFICIENT arm that re-derives from its own spec. The returned work
    # yields the receipt AND this node's own staged product (`None` = stages nothing); content keys, receipts, and
    # elision stay the only truth — a product is a shared-intermediate ACCELERATION, never an input a key derives
    # from. The LEDGER stays heterogeneous behind the one `Fed[Any]` boundary `ArtifactWork` already is.
    build: Callable[[tuple[P, ...] | None], Work[tuple[ArtifactReceipt, P | None]]]


@tagged_union(frozen=True)
class Admission:
    tag: Literal["keyed", "bare", "retried"] = tag()
    keyed: KeyedThread | None = case()
    bare: None = case()
    retried: RetryClass = case()


@tagged_union(frozen=True)
class PlanFault:
    tag: Literal["dangling", "cyclic", "collided", "untargeted"] = tag()
    dangling: frozenset[ContentKey] = case()
    cyclic: frozenset[ContentKey] = case()
    collided: frozenset[ContentKey] = case()
    untargeted: frozenset[ContentKey] = case()

    @property
    def keys(self) -> frozenset[ContentKey]:
        match self:
            case (
                PlanFault(tag="dangling", dangling=keys)
                | PlanFault(tag="cyclic", cyclic=keys)
                | PlanFault(tag="collided", collided=keys)
                | PlanFault(tag="untargeted", untargeted=keys)
            ):
                return keys
            case _ as unreachable:
                assert_never(unreachable)


# --- [CONSTANTS] ------------------------------------------------------------------------

_SLACK_EPS: Final[float] = 1e-9

# the one subject naming this fold on both evidence planes — the `planned` receipt line and the durable compute
# charge — so a board joins the two without a second spelling standing between them.
_SURFACE: Final[str] = "pipeline.plan"

# --- [MODELS] ---------------------------------------------------------------------------


class ProductFact(Struct, frozen=True):
    # ONE produced FILE crossing a `ProductSink`, flat native scalars alone: any producing plane fills it, and no
    # sink imports a producer's own carrier or learns its fault vocabulary. A producer NAMES its publication `root`,
    # where a boolean environment-or-not split collapses two roots for one plane and answers nothing for the
    # twenty-odd other kinds; naming it costs one spelling per plane and zero sink edits. `band` carries the
    # receipt-band preimage the producer already built, so its own carrier survives as an internal value and
    # projects this fact at the sink call. Publication returns the generated ArtifactRef; no sink mints a parallel key.
    key: ContentKey
    root: str
    file: str
    payload: bytes
    container: str
    tool: str = ""
    tool_version: str = ""
    band: frozendict[str, float | str] = frozendict()


class ArtifactWork(Struct, frozen=True):
    key: ContentKey
    work: Work[ArtifactReceipt]
    parents: tuple[ContentKey, ...] = ()
    admission: Admission = Admission(keyed=None)
    cost: float = 1.0
    fed: Fed[Any] | None = None  # the parent-product leg; None = the closed thunk alone, and `work` stays the truth

    def lowered(self, ledger: "ProductLedger | None" = None, /) -> Admit[ArtifactReceipt]:
        # The lane sees a plain thunk either way: a fed node's Admit payload closes over the drain's ledger, takes
        # its parents' products at CALL time (earlier fronts have drained by then), builds, and stages its own —
        # the runtime lane owns the drive and never learns products exist.
        fn = self.work if self.fed is None or ledger is None else self._exchanged(self.fed, ledger)
        match self.admission:
            case Admission(tag="keyed"):
                return Admit(keyed=(self.key, fn))
            case Admission(tag="bare"):
                return Admit(bare=fn)
            case Admission(tag="retried", retried=cls):
                return Admit(retried=(cls, fn))
            case _ as unreachable:
                assert_never(unreachable)

    def _exchanged(self, fed: Fed[Any], ledger: "ProductLedger", /) -> Work[ArtifactReceipt]:
        async def run() -> RuntimeRail[ArtifactReceipt]:
            railed = await fed.build(ledger.take(self.parents))()
            return railed.map(lambda pair: (ledger.stage(self.key, pair[1]), pair[0])[1])

        return run


class Schedule(Struct, frozen=True):
    finish: Map[ContentKey, tuple[float, int]] = Map.empty()
    latest: Map[ContentKey, float] = Map.empty()
    work: float = 0.0

    @property
    def duration(self) -> float:
        return max((finish for finish, _ in self.finish.values()), default=0.0)

    @property
    def span(self) -> int:
        return max((hop for _, hop in self.finish.values()), default=0)

    @property
    def slack(self) -> Map[ContentKey, float]:
        return self.finish.map(lambda key, pair: self.latest[key] - pair[0])

    @property
    def critical(self) -> frozenset[ContentKey]:
        return frozenset(key for key, (finish, _) in self.finish.items() if self.latest[key] - finish <= _SLACK_EPS)

    @classmethod
    def of(cls, graph: "rx.PyDiGraph[ContentKey, None]", weights: Map[ContentKey, float], elided: frozenset[ContentKey], /) -> Self:
        cost = lambda key: 0.0 if key in elided else weights.try_find(key).default_value(1.0)
        order = tuple(rx.topological_sort(graph))

        def ahead(memo: Map[ContentKey, tuple[float, int]], idx: int, /) -> Map[ContentKey, tuple[float, int]]:
            key = graph.get_node_data(idx)
            parents = tuple(graph.get_node_data(parent) for parent in graph.predecessor_indices(idx))
            finish = cost(key) + max((memo[parent][0] for parent in parents), default=0.0)
            hop = 1 + max((memo[parent][1] for parent in parents), default=0)
            return memo.add(key, (finish, hop))

        finish = Block.of_seq(order).fold(ahead, Map.empty())
        duration = max((value for value, _ in finish.values()), default=0.0)

        def behind(memo: Map[ContentKey, float], idx: int, /) -> Map[ContentKey, float]:
            kids = tuple(graph.get_node_data(child) for child in graph.successor_indices(idx))
            return memo.add(graph.get_node_data(idx), min((memo[kid] - cost(kid) for kid in kids), default=duration))

        latest = Block.of_seq(reversed(order)).fold(behind, Map.empty())
        return cls(finish=finish, latest=latest, work=sum(cost(graph.get_node_data(idx)) for idx in order))


class PipelinePlan(Struct, frozen=True):
    # `lane` threads from the pipeline that computed the plan — a capacity literal has no owner.
    fronts: tuple[Block[Admit[ArtifactReceipt]], ...]
    lane: LanePolicy
    requires: Map[ContentKey, tuple[ContentKey, ...]] = Map.empty()
    schedule: Schedule = Schedule()
    cache_seed: Map[ContentKey, ArtifactReceipt] = Map.empty()
    threads: Map[ContentKey, KeyedThread] = Map.empty()
    keys: frozenset[ContentKey] = frozenset()
    dangling: frozenset[ContentKey] = frozenset()
    elided: frozenset[ContentKey] = frozenset()
    cyclic: frozenset[ContentKey] = frozenset()
    collided: frozenset[ContentKey] = frozenset()
    untargeted: frozenset[ContentKey] = frozenset()
    fed: frozenset[ContentKey] = frozenset()

    @property
    def profile(self) -> tuple[int, ...]:
        return tuple(len(front) for front in self.fronts)

    @property
    def cardinality(self) -> int:
        return len(self.keys)

    @property
    def depth(self) -> int:
        return len(self.fronts)

    @property
    def width(self) -> int:
        return max(self.profile, default=0)

    @property
    def contention(self) -> int:
        return max(self.width - self.lane.slots.total, 0)

    @property
    def live(self) -> int:
        return self.cardinality - len(self.elided)

    @property
    def critical_path(self) -> float:
        return self.schedule.duration

    @property
    def span(self) -> int:
        return self.schedule.span

    @property
    def makespan(self) -> float:
        return max(self.schedule.duration, self.schedule.work / self.lane.slots.total)

    @property
    def adjacency(self) -> Map[ContentKey, tuple[ContentKey, ...]]:
        return self.requires.map(lambda _key, parents: tuple(parent for parent in parents if parent in self.keys))

    @property
    def roots(self) -> frozenset[ContentKey]:
        return frozenset(key for key in self.keys if not self.adjacency.try_find(key).default_value(()))

    @property
    def leaves(self) -> frozenset[ContentKey]:
        return self.keys - frozenset(parent for parents in self.adjacency.values() for parent in parents)

    @property
    def severed(self) -> Option[PlanFault]:
        match self.cyclic, self.untargeted, self.collided, self.dangling:
            case keys, _, _, _ if keys:
                return Some(PlanFault(cyclic=keys))
            case _, keys, _, _ if keys:
                return Some(PlanFault(untargeted=keys))
            case _, _, keys, _ if keys:
                return Some(PlanFault(collided=keys))
            case _, _, _, keys if keys:
                return Some(PlanFault(dangling=keys))
            case _:
                return Nothing

    def node_link(self) -> str:
        graph = ArtifactPipeline._digraph(self.keys, ((parent, child) for child, parents in self.adjacency.items() for parent in parents))
        slack, critical = self.schedule.slack, self.schedule.critical

        def attrs(key: ContentKey, /) -> dict[str, str]:
            return {
                "key": key.hex,
                "state": "elided" if key in self.elided else "live",
                "slack": f"{slack.try_find(key).default_value(0.0):.3f}",
                "critical": "1" if key in critical else "0",
            }

        return rx.node_link_json(graph, node_attrs=attrs)

    @property
    def facts(self) -> dict[str, object]:
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
            "fed": len(self.fed),
            "roots": len(self.roots),
            "leaves": len(self.leaves),
            "severed": self.severed.map(lambda fault: fault.tag).default_value("ok"),
        }


# --- [SERVICES] -------------------------------------------------------------------------


class ProductLedger:
    # The ONE drain-scoped product exchange — deliberately mutable (a keyed hand-off between fronts IS state) and
    # single-drain: minted per `_compute`, closed into the lowered thunks, dead with the plan. Consumer counts
    # derive from the plan's own adjacency (fed children per parent), so a product with no fed reader drops at
    # `stage` and every read product frees on its LAST `take` — a 16k pyramid never outlives its final consumer.
    # Thunks run on the one event loop (kernels offload; the exchange does not), so the dict needs no lock.
    def __init__(self, consumers: Counter[ContentKey], /) -> None:
        self._consumers = Counter(consumers)
        self._staged: dict[ContentKey, Product] = {}

    def stage(self, key: ContentKey, product: Product | None, /) -> None:
        if product is not None and self._consumers[key] > 0:
            self._staged[key] = product

    def take(self, parents: tuple[ContentKey, ...], /) -> tuple[Product, ...] | None:
        # ALL-OR-NONE: a fed build sees every parent product or None — a partial tuple would hand a fold half its
        # operands and force per-leg absence arms into every consumer. Counts decrement only on a successful take,
        # and a key at zero evicts; a partially-staged set stays staged for a sibling whose own take may complete.
        if any(parent not in self._staged for parent in parents):
            return None
        products = tuple(self._staged[parent] for parent in parents)
        for parent in parents:
            self._consumers[parent] -= 1
            if self._consumers[parent] <= 0:
                self._staged.pop(parent, None)
        return products


class ArtifactPipeline(Struct, frozen=True):
    # `lane` arrives projected via LanePolicy.of(context) at the composition root — a capacity literal has no owner.
    nodes: Block[ArtifactWork]
    lane: LanePolicy
    cache_seed: Map[ContentKey, ArtifactReceipt] = Map.empty()
    targets: frozenset[ContentKey] = frozenset()

    async def plan(self, /) -> RuntimeRail[PipelinePlan]:
        # the durable charge seats at THIS async fold and never on `_emitted`: recording suspends on the journal's
        # bounded intake, so the synchronous projection carries the `planned` line alone and the one awaitable fold
        # owning the settled plan records. The rail BINDS into the verdict — an armed plane refusing a compute fact is
        # an evidence failure this caller owns — while a composition that installed no journal folds to the lawful
        # no-op at one map read and never learns which it ran under.
        settled = (await self.lane.offload(Kernel.of(self._compute, KernelTrait.RELEASING))).map(self._emitted)
        match settled:
            case Result(tag="ok", ok=resolved):
                return (await Journal.record(self._metered(resolved))).map(lambda _landed: resolved)
            case refused:
                return Error(refused.error)

    @staticmethod
    def _digraph(keys: frozenset[ContentKey], edges: Iterable[tuple[ContentKey, ContentKey]], /) -> "rx.PyDiGraph[ContentKey, None]":
        order = tuple(sorted(keys, key=lambda key: key.hex))
        graph: rx.PyDiGraph[ContentKey, None] = rx.PyDiGraph(check_cycle=False, multigraph=False)
        index = dict(zip(order, graph.add_nodes_from(order), strict=True))
        graph.add_edges_from_no_data((index[parent], index[child]) for parent, child in edges)
        return graph

    def _scoped(self, /) -> Block[ArtifactWork]:
        keys = frozenset(node.key for node in self.nodes)
        graph = self._digraph(keys, ((parent, node.key) for node in self.nodes for parent in node.parents if parent in keys))
        index = {graph.get_node_data(idx): idx for idx in graph.node_indices()}
        seeds = frozenset(index[target] for target in self.targets if target in index)
        closure = frozenset(graph.get_node_data(idx) for seed in seeds for idx in {seed, *rx.ancestors(graph, seed)})
        return self.nodes.filter(lambda node: node.key in closure)

    @staticmethod
    def _emitted(plan: PipelinePlan, /) -> PipelinePlan:
        Signals.emit(Receipt.of("artifacts", ("planned", _SURFACE, plan.facts)), OPEN)
        return plan

    @staticmethod
    def _metered(plan: PipelinePlan, /) -> MeterFact:
        # plan SHAPE alone: the CPM makespan is the cost-weighted serial-latency bound this fold just settled, stated
        # in the milliseconds the `COMPUTE` resource's own series declares — which is what fixes `ArtifactWork.cost`
        # as a millisecond work-weight rather than a dimensionless rank. An elided node weighs zero, so a fully-warm
        # replay charges nothing, and the EXECUTED drain's volume stays the runtime lane's `@drained` aspect: metering
        # it here too bills one drive twice off two folds that disagree by exactly the elision.
        return MeterFact(resource=Resource.COMPUTE, quantity=round(plan.makespan), surface=_SURFACE)

    def _compute(self, /) -> PipelinePlan:
        scoped = self.nodes if not self.targets else self._scoped()
        by_key: Map[ContentKey, ArtifactWork] = Map.of_seq((node.key, node) for node in scoped)
        requires: Map[ContentKey, tuple[ContentKey, ...]] = Map.of_seq((node.key, node.parents) for node in scoped)
        weights: Map[ContentKey, float] = Map.of_seq((node.key, node.cost) for node in scoped)
        keys = frozenset(node.key for node in scoped)
        dangling = frozenset(parent for node in scoped for parent in node.parents) - keys
        untargeted = self.targets - frozenset(node.key for node in self.nodes)
        collided = frozenset(key for key, count in Counter(node.key for node in scoped).items() if count > 1)
        elided = frozenset(node.key for node in scoped if node.admission.tag == "keyed") & frozenset(self.cache_seed.keys())
        threads: Map[ContentKey, KeyedThread] = Map.of_seq(
            (node.key, node.admission.keyed) for node in scoped if node.admission.tag == "keyed" and node.admission.keyed is not None
        )
        graph = self._digraph(keys, ((parent, node.key) for node in scoped for parent in node.parents if parent in keys))
        cyclic = frozenset(graph.get_node_data(idx) for edge in rx.digraph_find_cycle(graph) for idx in edge)
        schedule = Schedule() if cyclic else Schedule.of(graph, weights, elided)
        priorities = schedule.slack
        fed = frozenset(node.key for node in scoped if node.fed is not None)
        # one ledger per drain: consumer counts are the fed children per parent, read off the same adjacency the
        # graph loads, so eviction is a plan fact rather than a consumer guess
        ledger = ProductLedger(Counter(parent for node in scoped if node.fed is not None for parent in node.parents))
        fronts = (
            ()
            if cyclic
            else tuple(
                Block
                .of_seq(front)
                .sort_with(lambda idx: (priorities.try_find(graph.get_node_data(idx)).default_value(0.0), graph.get_node_data(idx).hex))
                .choose(lambda idx: by_key.try_find(graph.get_node_data(idx)).map(lambda node: node.lowered(ledger)))
                for front in rx.topological_generations(graph)
            )
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
            cyclic=cyclic,
            collided=collided,
            untargeted=untargeted,
            fed=fed,
        )

    @classmethod
    def of(
        cls,
        works: ArtifactWork | Iterable[ArtifactWork],
        *,
        lane: LanePolicy,
        warm: Map[ContentKey, ArtifactReceipt] = Map.empty(),
        targets: frozenset[ContentKey] = frozenset(),
    ) -> Self:
        nodes = Block.singleton(works) if isinstance(works, ArtifactWork) else Block.of_seq(works)
        return cls(nodes=nodes, lane=lane, cache_seed=warm, targets=targets)


# --- [EXPORTS] ----------------------------------------------------------------------------

__all__ = (
    "Admission",
    "ArtifactPipeline",
    "ArtifactWork",
    "Fed",
    "KeyedThread",
    "PipelinePlan",
    "PlanFault",
    "Product",
    "ProductFact",
    "ProductLedger",
    "ProductSink",
    "Schedule",
)
```

## [03]-[RESEARCH]

<!-- source-only: research row template; every landed row opens on the list dash this placeholder omits, the census reading `^- [TOKEN]-[OPEN|BLOCKED]:` alone:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
