# [PY_ARTIFACTS_PLAN]

The content-keyed artifact-production planning layer over the runtime session lane. `ArtifactPipeline` is ONE plan owner that folds a producer dependency graph into the `runtime/execution#LANE` admission shape AND schedules it: each `ArtifactWork` node carries a producer's pre-minted `ContentIdentity`-derived `ContentKey`, its `Work[ArtifactReceipt]` coroutine, its parent content keys, its `cost` work-weight, and the closed `Admission` case it enters the lane on, the `graphlib.TopologicalSorter` resolves the dependency-level fronts, one `Schedule.of` Critical-Path-Method pass over the in-set graph settles every node's earliest/latest finish so the plan reports the cost-weighted `critical_path`, the hop `span`, per-node `slack`, the zero-slack `critical` chain, and the bounded-capacity `makespan`, and each front is min-slack-ordered before lowering so the lane admits the latency-critical producers first under a saturated `CapacityLimiter` — the `cost` model now DRIVING the order it only reported before. Each node lowers through its own `ArtifactWork.lowered` admission `match` into its `runtime/execution#LANE` `Admit` case — a `keyed` node to the `(ContentKey, Work)` cache unit so a downstream front re-admitting an already-produced artifact replays the upstream `Ok` from the threaded session cache rather than re-rendering, a `bare` node to a forced-live unit, a `retried` node to a transient-guarded unit re-spawning a worker cold-start under the `RetryClass` its admission case mandates. It owns NO cache, NO store, NO scheduler, NO drain, and NO retry binding — the `runtime/execution#LANE` `LanePolicy` owns the `CapacityLimiter`/`move_on_after`/`DrainReceipt`, the front-to-front `DrainReceipt.cache` thread, and the concurrent per-front drive, and the `runtime/execution#LANE` `Admit` cases own the content short-circuit, the always-run, and the `guard(cls)` retry; `ArtifactPipeline` only PROJECTS the producer graph into those runtime ports, resolving the front ORDER and the schedule while the lane resolves the front DRIVE, and surfaces its own topology, schedule, and coverage evidence — `keys`/`dangling`/`elided`/`cyclic`/`collided`/`untargeted` the coverage sets, `profile`/`cardinality`/`depth`/`width`/`contention`/`live` the derived front shape (`contention` the peak over-subscription of the lane's `CapacityLimiter` the front structure conceals), `roots`/`leaves` the source-artifact and terminal-deliverable DAG facts, `schedule` the one CPM owner from which `critical_path`/`span`/`makespan`/`slack`/`critical` project, and `severed: Option[PlanFault]` the cause-carrying coverage verdict (`cyclic` versus `untargeted` versus `collided` versus `dangling`, never a `bool` erasing which). The runtime `StagePlan` is NOT this driver: it keys stages by `str` and forces every unit `retried`, so it cannot host a `ContentKey`-keyed graph whose nodes mix `keyed`/`bare`/`retried` admission; the plan emits the front-ordered `Block[Admit]` tuple the lane's `drain` folds over directly, never a `StagePlan` re-spelling. The plan is the third reuse-fabric consumer of the one `core/receipt#RECEIPT` `contribute` fold beside the `runtime/execution` lane elision and the `runtime/observability` `MeterProvider` signal stream — it reads each producer's contributed `ArtifactReceipt` as the content-keyed evidence the elision distinguishes hit from miss on, AND emits its own `planned`-phase `Receipt` `fact` (the `core/receipt#SIGNALS [PLAN_FABRIC]` consumer obligation) carrying the topology + CPM shape onto that one stream so the planning stage is observable beside the `admitted`/`emitted` producer facts. `ArtifactPipeline.plan` returns a `RuntimeRail[PipelinePlan]` carrying the front-ordered `Block[Admit[ArtifactReceipt]]` lanes — emitting the planned-phase fact on the resolved-plan arm — never an executed result.

## [01]-[INDEX]

- [01]-[PLAN]: content-keyed production-planning AND scheduling axis folding the `ArtifactWork` producer-node graph into the `runtime/execution#LANE` `Admit` admission shape — the `ArtifactWork` node value object carrying a producer's `ContentKey`, its `Work[ArtifactReceipt]` coroutine, its dependency parents, its `cost` work-weight, and its closed `Admission` lane-case union (the `retried` case carrying its mandatory `RetryClass`), the `ArtifactWork.lowered` admission `match` lowering each node to its `bare`/`keyed`/`retried` `Admit` case, the `graphlib.TopologicalSorter` front resolver, the `Schedule` Critical-Path-Method owner whose one forward/backward `Block.fold` pair settles every node's `(earliest_finish, hop)` and `latest_finish` so `duration`/`span`/`slack`/`critical`/`makespan` project from one memo, the min-slack front ordering the lane admits under, the `PipelinePlan` front-ordered `Block[Admit[ArtifactReceipt]]` projection (plus its `keys`/`dangling`/`elided`/`cyclic`/`collided`/`untargeted` coverage sets, its `requires` adjacency, its `profile`/`cardinality`/`depth`/`width`/`contention`/`live`/`roots`/`leaves` derived front shape, its `schedule`-projected `critical_path`/`span`/`makespan`, and its `severed: Option[PlanFault]` cause-carrying coverage verdict) the runtime lane drains, the `_reachable` target-closure scope a partial rebuild plans against, the `planned`-phase `Receipt` `fact` the plan emits onto the one receipt stream, and the one `LanePolicy` policy value the plan carries and the lane drives under.

## [02]-[PLAN]

- Owner: `ArtifactPipeline` the one production-planning-and-scheduling axis folding the `ArtifactWork` producer-node graph into `runtime/execution#LANE` admission units; `ArtifactWork` the frozen producer-node value object carrying the producer's pre-minted `key: ContentKey` (the `ContentIdentity.of(...)` derivation every producer `_emit` already returns), the `work: Work[ArtifactReceipt]` thunk (the producer's own `RuntimeRail[ArtifactReceipt]`-returning coroutine the plan never invokes, only schedules), the `parents: tuple[ContentKey, ...]` upstream content keys the node depends on, the `cost: float` estimated work-weight (render-time seconds / byte-volume, a unit default) the CPM forward pass sums into earliest-finish, and its `admission: Admission` lane-case union owning its own `lowered` `match`, so a node IS its content key, a dependency IS a content-key edge, and the lane case it enters IS one closed vocabulary member carrying exactly its case payload — never a node id beside the key, never a path, never a hardcoded single admission case for a three-case lane union, and never an `admission` tag beside a parallel `Option[RetryClass]` field where the `retried` case already carries the class; `Admission` the closed `@tagged_union` (`keyed`/`bare`/`retried`) selecting which `runtime/execution#LANE` `Admit` case a node lowers to, `keyed` the cache-eligible default the elision is built on (empty payload), `bare` a forced-live non-content-addressable one-shot (empty payload), `retried` a transient-prone offload producer carrying its mandatory `RetryClass` so a `retried` node states its class or is not retried — never a `RetryClass.OCCT` default papering over an absent class the case forbids by construction; `PlanFault` the closed `dangling`/`cyclic`/`collided`/`untargeted` coverage-failure vocabulary the `severed` verdict carries, each case holding its offending `frozenset[ContentKey]` so the root recovers on the cause (re-key a typo'd upstream edge, re-wire a producer cycle, dedupe-re-key a double-claimed artifact identity, or re-key a requested target the producer set never mints) rather than a `bool` collapsing all to one undifferentiated "broken"; `Schedule` the Critical-Path-Method evidence owner computed ONCE over the in-set adjacency — `finish: Map[ContentKey, tuple[float, int]]` the per-node `(earliest_finish, hop)` forward pass and `latest: Map[ContentKey, float]` the latest-finish backward pass and `work: float` the total live cost, from which `duration` (the cost-weighted critical path), `span` (the hop-count longest chain), `slack` (per-node total float `latest - earliest_finish`), and `critical` (the zero-slack chain) all PROJECT, so neither metric re-walks the graph where the naive page rebuilt the whole topological memo twice (once for `critical_path`, once for `span`); `PipelinePlan` the frozen plan projection carrying the dependency-front-ordered `fronts: tuple[Block[Admit[ArtifactReceipt]], ...]` (one min-slack-ordered `Block` per `graphlib.TopologicalSorter` level), the `requires: Map[ContentKey, tuple[ContentKey, ...]]` producer-graph adjacency the DAG-shape evidence derives from, the `schedule: Schedule` CPM evidence computed once (it carries the per-node `cost`-weighted finish, so the raw `weights` map is the `_plan` local the CPM consumes, never a redundant plan field), the resolved `cache_seed: Map[ContentKey, ArtifactReceipt]` the runtime drain threads forward, the `keys: frozenset[ContentKey]` the plan produces, the `dangling: frozenset[ContentKey]` parent keys no node mints (the silent external-node trap `graphlib` would treat as already-done), the `elided: frozenset[ContentKey]` `keyed` nodes the threaded `cache_seed` already carries (the short-circuit set `keyed` keys ∩ cache keys), the `cyclic: frozenset[ContentKey]` chain a `CycleError` named, the `collided: frozenset[ContentKey]` keys minted by more than one node (the last-wins drop `Map.of_seq` would otherwise absorb silently), and the `untargeted: frozenset[ContentKey]` requested `targets` deliverables the producer set never mints (the target-axis silent-truncation `_reachable` would otherwise drop into an empty/partial plan), with `profile`/`cardinality`/`depth`/`width`/`contention`/`live` deriving the per-front size vector, the total node count, the front count, the widest front, the peak over-subscription `max(width - lane.capacity, 0)` the `CapacityLimiter` queues, and the units that reach a coroutine (`cardinality - len(elided)`), `roots`/`leaves` the source-artifact and terminal-deliverable sets over `adjacency`, `critical_path`/`span`/`makespan` projecting the `schedule`'s cost-weighted serial latency, hop depth, and Graham bounded-capacity floor (`max(critical_path, work / lane.capacity)`), and `severed: Option[PlanFault]` the cause-carrying coverage verdict (`cyclic` then `untargeted` then `collided` then `dangling`, `Nothing` for a sound plan) the composition root reads before driving — never an executed `DrainReceipt`, never a flat unordered unit list that loses the front structure, never a topology that drops the dangling/cyclic/collided/elision/source-sink/critical-path/slack/contention evidence, an unweighted hop count masquerading as the serial-latency bound, a recomputed-per-access schedule, or a `bool` verdict erasing which coverage cause the consumer must act on; `LanePolicy` the `runtime/execution#LANE` bounded-drain owner the plan carries as ONE policy value (capacity + `Deadline.seconds`-derived `Option[float]` budget) and the lane drives under, the plan minting no limiter, no scope, and never splitting the policy into loose `capacity`/`deadline` scalars; `Admit` the `runtime/execution#LANE` closed admission union whose `bare`/`keyed`/`retried` cases each node lowers into through its own `ArtifactWork.lowered` `match`, the plan re-spelling no admission shape; the projection-and-schedule layer between the producer graph and the runtime session lane.
- Cases: `_plan` is the one inlined boundary fold — it indexes `by_key`, builds the `requires` adjacency and the `weights` cost map, derives `keys`/`dangling`/`elided`, captures `collided` from one `Counter` over the node keys (the double-claimed identities `Map.of_seq` last-wins would otherwise drop without a signal), narrows `adjacency = requires ∩ keys` once, builds one `graphlib.TopologicalSorter[ContentKey]` adding each node and its `parents` through `add(node.key, *node.parents)`, then drives the active `prepare`/`is_active`/`get_ready`/`done` protocol so each `get_ready()` tuple is one dependency-level front the runtime lane then DRIVES. After `prepare` it runs one `Schedule.of(adjacency, weights, elided)` settling the CPM passes once, binds `priorities = schedule.slack` ONCE (the property rebuilds the slack map per access, so reading it inside the front loop would re-pay the projection per front), and each front projects back through `by_key` with `Block.of_seq(front).sort_with(...)` min-slack-ordering the keys before `Block.choose` over the `try_find` `Option` lowers each resolved node through its own `ArtifactWork.lowered` admission `match` into its `Admit` case — a `keyed` node to `Admit(keyed=(key, work))` the lane's `ADMIT_TABLE` `keyed` row probes the threaded session cache with (a `ContentKey` already carrying an `Ok` short-circuits without re-rendering and increments the lane `hit`), a `bare` node to `Admit(bare=work)` that always runs, a `retried` node to `Admit(retried=(cls, work))` whose `guard(cls)` re-spawns a transient worker cold-start under the `RetryClass` the admission case carried. There is no single-call `_topo`/`_front_units`/`_heights` hop beside `_plan` — the sorter build, the CPM `Schedule.of`, the min-slack sort, and the per-node admission `match` are the loop's own steps. A `graphlib.CycleError` from `prepare` is caught IN `_plan` and its `args[1]` node chain folds into `PipelinePlan.cyclic` returning an empty-front, empty-`Schedule` plan the `severed` verdict refuses, never a raise the caller must trap. This is NOT the `StagePlan.execute` driver: `StagePlan` keys stages by `str` and folds every unit into `Admit(retried=...)`, so it structurally cannot host a `ContentKey`-keyed graph whose nodes mix `keyed`/`bare`/`retried` admission — the plan emits the front-ordered `Block[Admit]` tuple the lane's `drain` folds over directly. `_plan` assembles the per-front `Block` tuple (accumulated forward through immutable `Block.append` of one `Block.singleton(front)` per level, never a mutable `list`) into one `PipelinePlan` carrying the front order, the `requires` adjacency, the `schedule` CPM evidence, the `cache_seed`, the produced `keys`, and the `dangling`/`elided`/`collided`/`untargeted` coverage sets, the plan owning the ELISION SHAPE, the topology, the CPM schedule, and the coverage evidence while the runtime lane owns the drain; the resolved-plan rail then threads the module-level `_emitted` tap firing the one `planned`-phase `Signals.emit(Receipt.of("artifacts", ("planned", "pipeline.plan", plan.facts)), _PLAN_REDACTION)` over the owner-derived `PipelinePlan.facts` projection so the planning stage rides the receipt stream; `_reachable` is the one `tailrec` fixpoint walking the `parents` edges up from a requested target set so `of` scopes the node set to the ancestor closure a partial rebuild needs.
- Modality: `ArtifactPipeline.plan` is `async` over the runtime `async_boundary`, folding the producer graph inside the one fault capsule — `_plan` resolving the `graphlib.TopologicalSorter` fronts, settling the `Schedule` CPM passes, and min-slack-lowering each front into a `Block[Admit[ArtifactReceipt]]` in one loop — and returns a `RuntimeRail[PipelinePlan]`; the runtime owner then drives the plan by draining each front through `LanePolicy.drain(front, carried)` and threading each front's `DrainReceipt.cache` into the next, the plan itself executing nothing. One `ArtifactPipeline.of` owns every construction arity — a lone `ArtifactWork` or any `Iterable[ArtifactWork]` normalized once at the head by input shape, never a `single`/`many` suffix pair — and its keyword-only `targets: frozenset[ContentKey]` scopes the plan to the requested-deliverable ancestor closure (`_reachable`), an empty set planning the whole graph (the closure is the identity), so a caller rebuilding one cover page and its bound figures plans exactly that sub-graph rather than the entire producer set, the discriminant the value of `targets` itself, never a `mode`/`partial` flag; a requested target the producer set never mints surfaces as `untargeted` evidence the `severed` verdict refuses rather than a silently-empty plan, `ArtifactPipeline` retaining the requested `targets` so `_plan` computes `targets - keys` after scoping. A cyclic producer graph surfaces as `PipelinePlan.cyclic` evidence the `severed` verdict gates on, caught at `prepare` inside `_plan` so the rail still carries a structured plan rather than the `async_boundary` converting a `CycleError` raise.
- Auto: the producer-node graph is content-addressed end to end — each `ArtifactWork.key` is the `evidence/identity#IDENTITY` `ContentKey` the producing sub-domain's `_emit` already minted (the `document/emit#DOCUMENT`, `document/report#REPORT`, `visualization/chart/export#CHART`, `visualization/table#TABLE`, `scene#SCENE`, `package#BUNDLE`, and `exchange/credential#PROVENANCE` arms all return `ContentIdentity.of(...)`), so the plan re-mints no key and a settings change that shifts a producer's content key correctly misses the cache; each dependency edge is a parent `ContentKey`, so the `graphlib.TopologicalSorter` orders the graph by content-key reachability and a report node's `parents` are exactly its bound `FigureRef.asset_key` content keys; a parent `ContentKey` no node in the set produces is captured once into `PipelinePlan.dangling` rather than silently absorbed as a `graphlib` external already-done node; two nodes minting one `ContentKey` are captured into `PipelinePlan.collided` rather than dropped last-wins by `Map.of_seq`, so a double-claimed artifact identity surfaces as plan evidence instead of a quietly-missing producer; a `keyed` node whose key already resolved into the threaded `cache_seed` is captured into `PipelinePlan.elided` so the plan reports the short-circuit set (and the `live` count) before the lane probes one key; the `Schedule.of` forward pass treats an `elided` cache hit as zero cost (it never runs) yet still one hop (it still gates its dependents), so `critical_path`/`makespan` collapse correctly on a warm replay; each node lowers through its own `ArtifactWork.lowered` admission `match` so the runtime lane owns the short-circuit, the always-run, and the transient retry by `Admit` case — the plan never probes a cache to DECIDE a drain, opens a scope, or binds a retry itself, it only shapes and orders the units the lane drives; the `cache_seed` is the only mutable-by-reference state the plan threads, a `runtime/execution#LANE` `DrainReceipt.cache` `Map[ContentKey, ArtifactReceipt]` the runtime minted carried forward for a warm replay, never a durable store the plan owns.
- Receipt: the plan contributes no `ArtifactReceipt` CASE of its own — it is the CONSUMER of the one `core/receipt#RECEIPT` `contribute` fold every producer satisfies, reading each producer's contributed `ArtifactReceipt` as the keyed evidence the sub-graph elision distinguishes a hit from a miss on, the third reuse-fabric consumer of that single fold beside the `runtime/execution` lane `(ContentKey, Work)` elision and the `runtime/observability` `MeterProvider` signal stream named in `core/receipt#SIGNALS`. It DOES emit its own `planned`-phase fact on that one stream, the consumer-side obligation `core/receipt#SIGNALS [PLAN_FABRIC]` names: `plan` threads the module-level `_emitted` tap over the resolved-plan rail to fire `Signals.emit(Receipt.of("artifacts", ("planned", "pipeline.plan", plan.facts)), _PLAN_REDACTION)` once per plan — the `runtime/observability#RECEIPT` `Receipt.of` `(Phase, subject, facts)` triple minting the `fact` case the `PHASE_LEVEL` table logs at `debug`, so the planning stage is observable on the same line family the `admitted`/`emitted` producer facts ride. The facts map is the owner-derived `PipelinePlan.facts` property (the plan projects its own observability view, mirroring the `core/receipt#RECEIPT` `ArtifactReceipt._facts` shape — never an `ArtifactPipeline`-side `_evidence` hop) and carries the native-int/float coverage AND CPM shape (`cardinality`/`live`/`depth`/`width`/`contention`/`critical_path`/`makespan`/`span`/`critical`/`elided`/`dangling`/`cyclic`/`collided`/`roots`/`leaves`) the receipts `Encoder(enc_hook=repr, order="deterministic")` renderer serializes unstringified plus the `severed.map(tag).default_value("ok")` cause, never a `str()`-pre-formatted scalar. `_emitted` fires only on the resolved-plan arm so a coverage-severed plan still rides the rail as evidence rather than a blind emit, and the plan owns no `Metrics.observe`/`Signals.emit` for the EXECUTED drain — the `runtime/execution#LANE` `@drained` aspect owns that as the front drains. The executed plan's per-front `runtime/execution#LANE` `DrainReceipt[ArtifactReceipt]` carries the `values`/`cache`/`hit`/`completed` that ARE the elision evidence — the plan shapes, schedules, and emits the planned-phase fact, the runtime drain produces the drain receipt, and the `core/receipt#RECEIPT` `contribute` fold both consume.
- Packages: `graphlib` (stdlib `TopologicalSorter[ContentKey]` driven in active `prepare`/`is_active`/`get_ready`/`done` mode for the dependency-level fronts and in static `static_order` mode inside `Schedule.of` for the two CPM passes, `CycleError` caught at `prepare` so its `args[1]` node chain folds into `PipelinePlan.cyclic` — the same stdlib pattern the `runtime/execution#LANE` `StagePlan` runs, applied here to the content-key producer graph the plan ORDERS and SCHEDULES while the lane DRIVES); `expression` (`Block.of_seq`/`Block.choose`/`Block.empty`/`Block.append`/`Block.singleton`/`Block.fold`/`Block.sort_with`/`Block.filter` the front projection, the immutable forward front accumulator, the two CPM `Block.fold` passes, the min-slack front ordering, and the `_reachable` closure filter; `Map.of_seq`/`Map.try_find`/`Map.empty`/`Map.keys`/`Map.map`/`Map.items`/`Map.values`/`Map.add`/`Map.change`/`map[key]` the node/cost lookup, the `adjacency` projection, the CPM `finish`/`latest`/`children` accumulators, and the `slack` projection; `Option`/`Some`/`Nothing`/`Option.map`/`Option.default_value` the `severed` cause verdict, the `weights.try_find` cost lookup, and the owner-derived `PipelinePlan.facts` projection; `tailrec`/`TailCall`/`TailCallResult` the `_reachable` ancestor-closure fixpoint; `tagged_union`/`tag`/`case` declaring the `Admission` and `PlanFault` selector unions and consuming the imported `runtime/execution#LANE` `Admit` shape, never re-declared); `collections.Counter` (the stdlib multiset surfacing the `collided` double-claimed keys `Map.of_seq` last-wins would drop); `msgspec` (`Struct(frozen=True)` the `ArtifactWork`/`Schedule`/`PipelinePlan`/`ArtifactPipeline` value objects); runtime (`content_identity.ContentKey` the producer key derivation and node identity, `lanes.Admit`/`LanePolicy`/`Work` the three-case admission shape and the bounded-drain policy value the plan carries and the lane drives under, `resilience.RetryClass` the offload transient class a `retried` `Admission` case mandates, `faults.RuntimeRail`/`async_boundary` the rail and fault capsule, `receipts.Receipt`/`Signals`/`Redaction` the shape-polymorphic `Receipt.of` factory and the `Signals.emit` sink the `planned`-phase fact rides); `core/receipt#RECEIPT` (`ArtifactReceipt` the `Work` payload type every node's coroutine resolves to, consumed as keyed evidence, the producer modules never imported because the `Work` thunk carries the producer call). No external library and no `frozendict` table — the admission lowering is the node's own `match`, the evidence sets are `frozenset`, the schedule is `expression` folds over `graphlib` order, and the planned-phase emit is the runtime receipt port the producers already ride.
- Growth: a new producer is one `ArtifactWork` node carrying its pre-minted `ContentKey`, its `Work[ArtifactReceipt]` thunk, its parent content keys, its `cost`, and its `Admission` case — the `graphlib.TopologicalSorter` places it in its dependency front, the `Schedule.of` passes settle its CPM finish, and the `_plan` loop min-slack-lowers it through its own `ArtifactWork.lowered` `match` with zero plan edit; a new admission modality is one `Admission` case plus one `lowered` arm (the `assert_never` tail proving total at type-check), never a per-mode method or a parallel lowering table; a transient-prone producer is one `retried` admission carrying its `RetryClass`; a new dependency is one parent `ContentKey` reaching the `requires` adjacency and the CPM passes for free; a warm replay is one non-empty `cache_seed` the runtime drain threads and the `elided` set (plus the zero-cost CPM contribution) pre-reports; a new bounded-drain trait reaches the plan as one `runtime/execution#LANE` `LanePolicy` field; a new per-node weight axis is one `ArtifactWork` field reaching the `weights` map and the `Schedule` passes for free; a new front-shape or schedule metric is one derived `PipelinePlan` property (`live` over `elided`, `contention` over `width`/`lane.capacity`, `makespan` over `schedule`/`lane.capacity`) or one `Schedule` projection (`slack`/`critical` over `finish`/`latest`) — derived from the existing memo, never a re-walk; a new coverage-failure cause is one `PlanFault` case plus one `severed` arm and one `_plan` detection (the `collided` and `untargeted` cases are exactly that growth — one on the identity axis, one on the target axis); a partial rebuild is one `targets` set on `of`; zero new surface — the plan owns the producer-graph-to-admission projection and the CPM schedule and grows by node, case, derived property, and coverage cause, never by method.
- Boundary: no cache, no durable store, no scheduler, no drain, no `CapacityLimiter`, no `move_on_after`, no `guard` binding, no `DrainReceipt` construction, and no producer execution — the `runtime/execution#LANE` `LanePolicy` owns the bounded drain, the limiter, the active per-front drive, the front-to-front `DrainReceipt.cache` thread, and the `Admit`-case content short-circuit / always-run / transient retry, and each producing sub-domain owns its own render; the plan re-mints no `ContentKey` and owns no second scheduler beside the `runtime/execution#LANE` lane. A node id beside the content key, a path-keyed dependency edge, a hardcoded single `Admit.keyed` lowering where the node's `Admission` case selects among the lane's three, a `bool cached`/`bool retried` knob or an `admission` tag beside a parallel `Option[RetryClass]` field, a `RetryClass.OCCT` default papering over an absent class, a `_LOWER` `frozendict` table or external dispatcher where the node's own `lowered` `match` owns the lowering, a plan-owned `Map[ContentKey, ...]` cache where the runtime `DrainReceipt.cache` is threaded, a plan-built `CapacityLimiter`/`move_on_after`/`guard(cls)` where the `LanePolicy` and the `retried` admission own them, a `StagePlan` re-spelling where it forces every unit `retried`, a re-declared `Admit`/`Work` shape, loose `capacity`/`deadline` scalars where one `LanePolicy` carries the budget, a `.default_value(None)`-then-`is not None` filter where `Block.choose` over the `Option` drops the misses, a mutable `list` front accumulator where immutable `Block.append` carries the rebound loop state, a flat unordered unit list where the front-structured `tuple[Block[Admit], ...]` preserves the levels, a fixed `get_ready()` front order ignoring the cost model where `Block.sort_with` over `schedule.slack` admits the latency-critical producers first, a re-paid slack map inside the front loop where `priorities` is bound once, a dropped dangling-parent edge where `PipelinePlan.dangling` surfaces it, a silent `Map.of_seq` last-wins key collision where `Counter`-derived `collided` surfaces it, an inert evidence set with no verdict where `severed`/`live`/`contention`/`roots`/`leaves`/`critical_path`/`span`/`makespan`/`slack`/`critical` act on it, a `bool severed` collapsing the `dangling`/`cyclic`/`collided`/`untargeted` causes to one undifferentiated "broken" where the closed `PlanFault`-carrying `Option[PlanFault]` names the cause and its offending keys, a `depth` front-count OR an unweighted hop count masquerading as the serial-latency bound where the COST-weighted `critical_path` is the latency, the `width` peak `contention` the saturation, and the `makespan` Graham area bound the bounded-capacity wall-clock floor, a per-access `_heights` re-walk recomputing the whole topological memo for `critical_path` and AGAIN for `span` where the one `Schedule.of` settles both passes once and every metric projects, an open per-root `def height(...): return height(...)` recursion that grows the stack and recomputes a diamond's spine exponentially where the `Schedule.of` `Block.fold` over `static_order` settles each `(finish, hop)`/`latest` once, an exact `== 0.0` slack test where `<= _SLACK_EPS` admits the critical chain over an accumulation-built float, an all-or-nothing plan that cannot scope to requested deliverables where `targets` walks the `_reachable` ancestor closure, a silently-dropped unknown target where `_plan`'s `targets - keys` surfaces it as `untargeted` and `severed` refuses the partial plan, a prose-only receipt-fold consumer that emits NOTHING where `_emitted` fires the `planned`-phase `Receipt.of("artifacts", ("planned", ...))` fact over the owner-derived `PipelinePlan.facts`, an `ArtifactPipeline`-side facts map where the `PipelinePlan` owner derives its own observability projection, a hardcoded `emitted` phase blind to the planning stage where the `phase`-modal `Receipt` `fact` case carries `planned`, a plan-owned `Metrics.observe`/`Signals.emit` for the EXECUTED drain where the `runtime/execution#LANE` `@drained` aspect owns it, a `str()`-pre-formatted coverage scalar where the receipts `enc_hook=repr` renderer serializes the native int/float, a `CycleError` escaping the projection where `_plan` folds it into `cyclic`, a per-node `ContentIdentity.of` re-mint where the producer already keyed, a `DrainReceipt` the plan constructs where the runtime drain produces it, and a producer module the plan imports where the `Work` thunk carries the call are the deleted forms — `ArtifactPipeline` is the producer-graph-to-`Admit` projection-and-schedule owner of the elision shape, the topology, the CPM evidence, and its one planned-phase fact, never the lane mechanics.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections import Counter
from collections.abc import Iterable
from graphlib import CycleError, TopologicalSorter
from typing import Final, Literal, assert_never

from expression import Nothing, Option, Some, TailCall, TailCallResult, case, tag, tagged_union, tailrec
from expression.collections import Block, Map
from msgspec import Struct

from rasm.runtime.content_identity import ContentKey
from rasm.runtime.faults import RuntimeRail, async_boundary
from rasm.runtime.lanes import Admit, LanePolicy, Work
from rasm.runtime.receipts import Receipt, Redaction, Signals
from rasm.runtime.resilience import RetryClass

from artifacts.core.receipt import ArtifactReceipt

# --- [TYPES] ----------------------------------------------------------------------------

# the producer-side admission selector, one case per `Admit` arm; `retried` carries its MANDATORY
# `RetryClass` so a transient node states its class or is not retried — no `Option[RetryClass]`-beside-tag.
@tagged_union(frozen=True)
class Admission:
    tag: Literal["keyed", "bare", "retried"] = tag()
    keyed: None = case()
    bare: None = case()
    retried: RetryClass = case()


# the closed coverage-failure vocabulary the `severed` verdict carries — `cyclic` (a producer cycle),
# `untargeted` (a requested `targets` deliverable no node mints), `collided` (two nodes minting one
# `ContentKey`), and `dangling` (an unsatisfied upstream edge) are FOUR distinct causes the root repairs
# differently (re-wire a cycle, re-key or drop a typo'd target request, dedupe-or-re-key a double-claimed
# identity, re-key a typo'd `FigureRef.asset_key` parent), so the verdict is a tagged cause carrying its
# offending key set, never one `bool` collapsing all four to "broken" with the cause erased. `untargeted`
# is the target-axis counterpart of `dangling`: a partial rebuild scoped to a deliverable the producer set
# never mints is the same silent-truncation defect on the request edge the parent edge surfaces.
@tagged_union(frozen=True)
class PlanFault:
    tag: Literal["dangling", "cyclic", "collided", "untargeted"] = tag()
    dangling: frozenset[ContentKey] = case()
    cyclic: frozenset[ContentKey] = case()
    collided: frozenset[ContentKey] = case()
    untargeted: frozenset[ContentKey] = case()

# --- [CONSTANTS] ------------------------------------------------------------------------

_DEFAULT_LANE: Final[LanePolicy] = LanePolicy(capacity=8)
# the keep-all policy the planned-phase emit rides: the topology + CPM facts are non-sensitive
# native coverage scalars, none classified, so the `redact` processor scrubs nothing on this line.
_PLAN_REDACTION: Final[Redaction] = Redaction(classified=Map.empty())
# a node is on the critical chain when its total float collapses to (within rounding of) zero, so the
# membership test is `<= _SLACK_EPS`, never an exact `== 0.0` an accumulation-built `latest - finish`
# never lands on.
_SLACK_EPS: Final[float] = 1e-9

# --- [MODELS] ---------------------------------------------------------------------------


class ArtifactWork(Struct, frozen=True):
    key: ContentKey
    work: Work[ArtifactReceipt]
    parents: tuple[ContentKey, ...] = ()
    admission: Admission = Admission(keyed=None)
    # the producer's estimated work weight (render-time seconds, byte-volume, or a unit default) the CPM
    # forward pass sums into earliest-finish and the weighted critical path; an `elided` cache hit
    # contributes zero (it never runs) yet still gates its dependents at one hop.
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
    # the Critical-Path-Method evidence computed ONCE over the in-set adjacency and carried on the plan:
    # a forward pass settling each node's `(earliest_finish, hop)` and a backward pass settling its
    # `latest_finish` at the project duration, so `duration`/`span`/`slack`/`critical` PROJECT from one
    # pair of `Block.fold`s rather than the per-metric re-walk the prior page paid — its `critical_path`
    # and `span` each rebuilt the whole topological memo on every access. The maps are empty for a
    # cyclic (un-orderable) plan, so `duration`/`span` read zero and `critical` is empty.
    finish: Map[ContentKey, tuple[float, int]] = Map.empty()
    latest: Map[ContentKey, float] = Map.empty()
    work: float = 0.0

    @property
    def duration(self) -> float:
        # the cost-weighted longest dependency chain — the serial-latency lower bound the lane's
        # concurrency cannot collapse (a 400ms `scene` render dominates five 5ms tables a hop count
        # would rank longer), distinct from `span` the hop count and the front-count `depth`.
        return max((finish for finish, _ in self.finish.values()), default=0.0)

    @property
    def span(self) -> int:
        # the longest chain by HOP COUNT — the deepest path's topological depth, the structural
        # counterpart to the cost-weighted `duration`, distinct from the front-count `depth` (which
        # merges independent chains into shared levels).
        return max((hop for _, hop in self.finish.values()), default=0)

    @property
    def slack(self) -> Map[ContentKey, float]:
        # total float per node — `latest_finish - earliest_finish`, zero on the critical chain — the
        # min-slack-first priority the plan orders each front by so the lane admits the latency-critical
        # producers before the slack-heavy ones under a saturated `CapacityLimiter`.
        return self.finish.map(lambda key, pair: self.latest[key] - pair[0])

    @property
    def critical(self) -> frozenset[ContentKey]:
        # the zero-slack chain — the producers whose every delay slips the whole plan, the set the
        # composition root prioritizes and a `severed`-clear plan still surfaces as schedule evidence.
        return frozenset(key for key, (finish, _) in self.finish.items() if self.latest[key] - finish <= _SLACK_EPS)

    @staticmethod
    def of(adjacency: Map[ContentKey, tuple[ContentKey, ...]], weights: Map[ContentKey, float], elided: frozenset[ContentKey], /) -> "Schedule":
        cost = lambda key: 0.0 if key in elided else weights.try_find(key).default_value(1.0)
        order = tuple(TopologicalSorter[ContentKey](dict(adjacency.items())).static_order())

        # forward pass over the parents-first order: `ef(k) = cost(k) + max(parent ef)`, every parent
        # already settled, so `finish` and the hop depth land in one fold with no re-walk.
        def ahead(memo: Map[ContentKey, tuple[float, int]], key: ContentKey, /) -> Map[ContentKey, tuple[float, int]]:
            parents = adjacency[key]
            finish = cost(key) + max((memo[parent][0] for parent in parents), default=0.0)
            hop = 1 + max((memo[parent][1] for parent in parents), default=0)
            return memo.add(key, (finish, hop))

        finish = Block.of_seq(order).fold(ahead, Map.empty())
        duration = max((value for value, _ in finish.values()), default=0.0)
        children = Block.of_seq((parent, node) for node, parents in adjacency.items() for parent in parents).fold(
            lambda acc, edge: acc.change(edge[0], lambda cur, child=edge[1]: Some((*cur.default_value(()), child))), Map.empty()
        )

        # backward pass over the reversed order (sinks first): `lf(k) = min(lf(child) - cost(child))`,
        # a leaf's latest finish defaulting to the project `duration`, so `slack = latest - finish`.
        def behind(memo: Map[ContentKey, float], key: ContentKey, /) -> Map[ContentKey, float]:
            kids = children.try_find(key).default_value(())
            return memo.add(key, min((memo[kid] - cost(kid) for kid in kids), default=duration))

        latest = Block.of_seq(reversed(order)).fold(behind, Map.empty())
        return Schedule(finish=finish, latest=latest, work=sum(cost(key) for key in order))


class PipelinePlan(Struct, frozen=True):
    # `fronts` is the drive input (one `LanePolicy.drain(front, cache)` per element, each front already
    # min-slack-ordered); `requires` is the producer-graph adjacency the DAG-shape evidence derives
    # from; `schedule` is the CPM evidence computed once; every other field is the coverage evidence the
    # runtime does NOT own and `severed` gates before the drive.
    fronts: tuple[Block[Admit[ArtifactReceipt]], ...]
    requires: Map[ContentKey, tuple[ContentKey, ...]] = Map.empty()
    schedule: Schedule = Schedule()
    lane: LanePolicy = _DEFAULT_LANE
    cache_seed: Map[ContentKey, ArtifactReceipt] = Map.empty()
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
        # the peak over-subscription the lane's one `CapacityLimiter` QUEUES rather than runs: a plan
        # whose widest front exceeds `lane.capacity` has a hidden serialization the front structure
        # conceals, so the root reads `max(0, width - capacity)` before the drive — the saturation
        # verdict `depth` and `width` alone do not answer, complementary to the `makespan` area bound.
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
        # the bounded-capacity wall-clock lower bound (Graham's area bound): no schedule on
        # `lane.capacity` workers beats `max(critical_path, total_work / capacity)`, so a wide cheap
        # plan is work-bound and a deep expensive one is path-bound — the latency verdict the serial
        # `critical_path` alone under-reports when the live work cannot fit the capacity.
        return max(self.schedule.duration, self.schedule.work / self.lane.capacity) if self.lane.capacity else self.schedule.duration

    @property
    def adjacency(self) -> Map[ContentKey, tuple[ContentKey, ...]]:
        # the in-set predecessor projection `roots`/`leaves` and `Schedule.of` each read — every node's
        # `requires` parents narrowed to the keys the set mints, dropping the `dangling` out-of-set edge
        # once. Recomputed per access from `requires ∩ keys` (a frozen `Struct` carries no writable slot
        # for a `cached_property`); derived, never stored beside `requires` it is recoverable from.
        return self.requires.map(lambda _key, parents: tuple(parent for parent in parents if parent in self.keys))

    @property
    def roots(self) -> frozenset[ContentKey]:
        # the source artifacts — minted nodes with no in-set parent, the upstream producers a warm
        # replay seeds first; a node whose every parent is `dangling` (out-of-set) is still a root.
        return frozenset(key for key in self.keys if not self.adjacency.try_find(key).default_value(()))

    @property
    def leaves(self) -> frozenset[ContentKey]:
        # the terminal deliverables — minted nodes no in-set node depends on (a report binding figures
        # is a leaf, its figures are not), the outputs the root surfaces and a `targets`-scoped plan
        # converges on.
        return self.keys - frozenset(parent for parents in self.adjacency.values() for parent in parents)

    @property
    def severed(self) -> Option[PlanFault]:
        # which coverage cause makes the front order a lie, in repair-priority order: `cyclic` (the
        # hardest structural break, schedule undefined) before `untargeted` (the request names a
        # deliverable the producer set never mints, so the whole scope is wrong) before `collided` (a
        # double-claimed identity `by_key` silently drops one of) before `dangling` (a missing upstream),
        # `Nothing` for a sound plan — a `bool` collapsing the four erases the cause and the offending
        # keys the root acts on.
        if self.cyclic:
            return Some(PlanFault(cyclic=self.cyclic))
        if self.untargeted:
            return Some(PlanFault(untargeted=self.untargeted))
        if self.collided:
            return Some(PlanFault(collided=self.collided))
        return Some(PlanFault(dangling=self.dangling)) if self.dangling else Nothing

    @property
    def facts(self) -> dict[str, object]:
        # the planned-phase observability projection the `Receipt.of("artifacts", ("planned", ...))`
        # triple carries — owner-derived (the plan projects its own view, the receipt-page `_facts`
        # shape) so the emit tap names no `ArtifactPipeline`-side fact map: native int/float coverage +
        # CPM shape the receipts `enc_hook=repr`/`order="deterministic"` renderer serializes
        # unstringified, with the full `dangling`/`cyclic`/`collided`/`untargeted` coverage set reported
        # beside `severed`'s gating-cause tag (or `"ok"`) so the line names WHICH failure AND its magnitude.
        return {
            "cardinality": self.cardinality, "live": self.live, "depth": self.depth, "width": self.width,
            "contention": self.contention, "critical_path": self.critical_path, "makespan": self.makespan,
            "span": self.span, "critical": len(self.schedule.critical), "elided": len(self.elided),
            "dangling": len(self.dangling), "cyclic": len(self.cyclic), "collided": len(self.collided),
            "untargeted": len(self.untargeted), "roots": len(self.roots), "leaves": len(self.leaves),
            "severed": self.severed.map(lambda fault: fault.tag).default_value("ok"),
        }

# --- [OPERATIONS] -----------------------------------------------------------------------


def _reachable(nodes: Block[ArtifactWork], targets: frozenset[ContentKey], /) -> Block[ArtifactWork]:
    # the requested-deliverable closure: a `tailrec` fixpoint walking the `parents` edges UP from the
    # target leaves so a plan scopes to exactly the ancestor sub-graph a partial rebuild needs, every
    # other producer dropped before `_plan` orders one front; the `seen` set bounds the walk even over a
    # not-yet-cycle-checked graph. An empty `targets` is handled by the caller (the closure would be
    # empty), so this only runs when a real deliverable set scopes the plan.
    index: Map[ContentKey, ArtifactWork] = Map.of_seq((node.key, node) for node in nodes)

    @tailrec
    def walk(frontier: frozenset[ContentKey], seen: frozenset[ContentKey], /) -> TailCallResult[frozenset[ContentKey], ...]:
        if not (fresh := frontier - seen):
            return seen
        parents = frozenset(parent for key in fresh for parent in index.try_find(key).map(lambda node: node.parents).default_value(()))
        return TailCall(parents, seen | fresh)

    closure = walk(targets, frozenset())
    return nodes.filter(lambda node: node.key in closure)


def _emitted(plan: PipelinePlan, /) -> PipelinePlan:
    # the resolved-plan tap firing the one `planned`-phase fact on the shared receipt stream and
    # returning the plan unchanged — the facts projection is the `PipelinePlan.facts` owner derivation,
    # so this tap only emits and a coverage-severed plan still rides the rail rather than a blind emit.
    Signals.emit(Receipt.of("artifacts", ("planned", "pipeline.plan", plan.facts)), _PLAN_REDACTION)
    return plan


class ArtifactPipeline(Struct, frozen=True):
    nodes: Block[ArtifactWork]
    lane: LanePolicy = _DEFAULT_LANE
    cache_seed: Map[ContentKey, ArtifactReceipt] = Map.empty()
    # the requested-deliverable set `of` scoped `nodes` to (empty = whole graph); retained so `_plan`
    # surfaces a typo'd target the producer set never mints as `untargeted` rather than a silent drop.
    targets: frozenset[ContentKey] = frozenset()

    async def plan(self, /) -> RuntimeRail[PipelinePlan]:
        # the planner is the third `core/receipt#RECEIPT` `contribute` consumer: a `planned`-phase fact
        # rides the one receipt stream the reuse-fabric elision and the `MeterProvider` read, so the
        # planning stage is observable WITHOUT a parallel plan-receipt rail — `_emitted` fires it only on
        # the resolved-plan arm so a coverage-severed plan still rides the rail rather than a blind emit.
        railed = await async_boundary("pipeline.plan", self._plan)
        return railed.map(_emitted)

    async def _plan(self, /) -> PipelinePlan:
        by_key: Map[ContentKey, ArtifactWork] = Map.of_seq((node.key, node) for node in self.nodes)
        requires: Map[ContentKey, tuple[ContentKey, ...]] = Map.of_seq((node.key, node.parents) for node in self.nodes)
        weights: Map[ContentKey, float] = Map.of_seq((node.key, node.cost) for node in self.nodes)
        keys = frozenset(node.key for node in self.nodes)
        dangling = frozenset(parent for node in self.nodes for parent in node.parents) - keys
        # the target-axis counterpart of `dangling`: a requested deliverable the (scoped) producer set
        # never mints, surfaced rather than silently dropped by `_reachable` into an empty/partial plan.
        untargeted = self.targets - keys
        # the silent-collision trap: `Map.of_seq` is last-wins, so two nodes minting one `ContentKey`
        # drop one producer from `by_key`/`requires`/`weights` without a signal — `Counter` surfaces the
        # double-claimed keys as `collided` coverage evidence the `severed` verdict refuses.
        collided = frozenset(key for key, count in Counter(node.key for node in self.nodes).items() if count > 1)
        # the elision evidence: a `keyed` node whose content key already resolved into the threaded
        # session cache replays the upstream `Ok` in the runtime drain rather than re-rendering, so the
        # plan reports which keys short-circuit before the lane probes a single one.
        elided = frozenset(node.key for node in self.nodes if node.admission.tag == "keyed") & frozenset(self.cache_seed.keys())
        adjacency: Map[ContentKey, tuple[ContentKey, ...]] = requires.map(lambda _key, parents: tuple(parent for parent in parents if parent in keys))
        order: TopologicalSorter[ContentKey] = TopologicalSorter()
        for node in self.nodes:
            order.add(node.key, *node.parents)
        try:
            order.prepare()
        except CycleError as cycle:
            # `CycleError.args[1]` is the cyclic node chain; surface it as evidence and return an
            # empty-front, empty-`Schedule` plan the `severed` verdict refuses, never a raise escaping.
            return PipelinePlan(fronts=(), requires=requires, lane=self.lane, cache_seed=self.cache_seed, keys=keys, dangling=dangling, elided=elided, cyclic=frozenset(cycle.args[1]), collided=collided, untargeted=untargeted)
        schedule = Schedule.of(adjacency, weights, elided)
        # the min-slack priority read ONCE before the front loop (the property rebuilds the map per
        # access): each `get_ready()` front sorts ascending by slack so the lane admits the critical
        # producers first — the cost model now DRIVING the order it only reported before.
        priorities = schedule.slack
        # the stateful graphlib active driver (`is_active`/`get_ready`/`done`) is the one interop loop;
        # depth is in fronts not nodes (no async recursion), `fronts` rebound immutably per front, a
        # dangling `get_ready` key dropping through `by_key.try_find`. The plan ORDERS, the lane DRIVES.
        fronts: Block[Block[Admit[ArtifactReceipt]]] = Block.empty()
        while order.is_active():
            front = order.get_ready()
            units = Block.of_seq(front).sort_with(lambda key: priorities.try_find(key).default_value(0.0)).choose(lambda key: by_key.try_find(key).map(lambda node: node.lowered()))
            fronts = fronts.append(Block.singleton(units))
            order.done(*front)
        return PipelinePlan(fronts=tuple(fronts), requires=requires, schedule=schedule, lane=self.lane, cache_seed=self.cache_seed, keys=keys, dangling=dangling, elided=elided, collided=collided, untargeted=untargeted)

    @staticmethod
    def of(works: ArtifactWork | Iterable[ArtifactWork], *, lane: LanePolicy = _DEFAULT_LANE, warm: Map[ContentKey, ArtifactReceipt] = Map.empty(), targets: frozenset[ContentKey] = frozenset()) -> "ArtifactPipeline":
        nodes = Block.singleton(works) if isinstance(works, ArtifactWork) else Block.of_seq(works)
        scoped = nodes if not targets else _reachable(nodes, targets)
        return ArtifactPipeline(nodes=scoped, lane=lane, cache_seed=warm, targets=targets)
```

## [03]-[RESEARCH]

- [ADMISSION_LOWERING] [RESOLVED]: each `ArtifactWork` node lowers through its OWN `lowered` admission `match` into the `Admit` case its closed `Admission` union selects, covering ALL THREE cases the `runtime/execution#LANE` `@tagged_union(frozen=True) class Admit[T]` declares (`bare: Work[T]`, `keyed: tuple[ContentKey, Work[T]]`, `retried: tuple[RetryClass, Work[T]]`, with `Work[T] = Callable[[], Awaitable[RuntimeRail[T]]]`) rather than a hardcoded single `keyed` lowering — a `keyed` node lowers to `Admit(keyed=(self.key, self.work))`, a `bare` node (a forced-live non-content-addressable one-shot) to `Admit(bare=self.work)`, and a `retried` node (a transient-prone offload producer) to `Admit(retried=(cls, self.work))` where `cls` is the `RetryClass` the `retried` `Admission` case MANDATES. The `Admission` selector is itself a closed `@tagged_union` (`keyed`/`bare` empty-payload cases, `retried` carrying its mandatory `RetryClass`), so a transient node states its class IN its `retried` case or is not retried — no `RetryClass.OCCT` default papers over a `Nothing` the case forbids by construction. A `keyed`-plus-internal-offload-retry producer instead keeps `keyed` and rides its `LanePolicy.offload(kernel, retry=...)` inside its own `work` coroutine per the lane's "a content-`keyed` source stays keyed for the cache yet still retries a transient hop" leg. The `Admit.bare`/`keyed`/`retried` cases, the `RetryClass` import, and the `LanePolicy`/`Work` spellings verify against the `runtime/execution#LANE` `lanes.md` owner; the plan re-declares neither `Admit` nor `Work`, imports no `DrainReceipt` (it constructs none), and reaches only `Admit`/`LanePolicy`/`Work` from `rasm.runtime.lanes`.
- [CRITICAL_PATH_SCHEDULE] [RESOLVED]: `Schedule` is the one Critical-Path-Method owner computed exactly once per plan, replacing the prior page's per-access `_heights` re-walk where `critical_path` and `span` EACH rebuilt the entire `graphlib.static_order` + `Block.fold` memo (the prose claimed "one shared memo" the code violated by calling `self._heights()` twice). `Schedule.of` runs one forward `Block.fold` over `static_order` settling each node's `(earliest_finish, hop)` as `ef(k) = cost(k) + max(parent ef)` with every parent already settled, then one backward `Block.fold` over `reversed(static_order)` settling `latest_finish` as `lf(k) = min(lf(child) - cost(child))` (a leaf defaulting to the project `duration`), so `duration` (the cost-weighted critical path), `span` (the hop-count longest chain), `slack` (`latest - earliest_finish` per node), and `critical` (the zero-slack chain) all PROJECT from the one stored `finish`/`latest` pair rather than re-walking the graph — an `elided` cache hit contributing zero cost but one hop. `PipelinePlan.makespan` composes `max(schedule.duration, schedule.work / lane.capacity)`, Graham's bounded-capacity area bound the serial `critical_path` under-reports when the live work cannot fit the lane, complementary to `contention` the peak-width saturation. The forward/backward CPM passes verify against the per-node `cost` weights and the stdlib `TopologicalSorter.static_order` parents-first sequence; the `Block.fold`/`Map.add`/`Map.change`/`Map.try_find`/`Map.map`/`Map.items`/`Map.values` spellings against `.api/expression.md` collection-ops, the `1e-9` `_SLACK_EPS` critical-membership cut against the float-rounding of an accumulation-built `latest - finish`.
- [FRONT_PRIORITY] [RESOLVED]: each dependency-level front is min-slack-ordered before lowering — `Block.of_seq(front).sort_with(lambda key: priorities.try_find(key).default_value(0.0))` sorts the front's keys ascending by their CPM `slack` (the zero-slack critical producers first) so the runtime `LanePolicy.drain` under one `CapacityLimiter` admits the latency-critical producers before the slack-heavy ones, the `cost` model now DRIVING the front order where the prior page computed `critical_path` purely as a report and left `get_ready()`'s arbitrary order to the drive. `priorities = schedule.slack` is bound ONCE before the `while order.is_active()` loop because the `Schedule.slack` property rebuilds the map per access (a frozen `Struct` carries no `cached_property` slot), so reading it per front would re-pay the projection per level. `Block.sort_with(key)` verifies as a key-selector ascending sort against `.api/expression.md` sequence-ops [05]; the min-slack-first (MinSlack/MSLK) list-scheduling rule is the standard bounded-resource heuristic, slack monotone in criticality so a stable sort keeps the `get_ready()` order for equal-slack nodes.
- [TOPOLOGY_EVIDENCE] [RESOLVED]: `PipelinePlan` carries its own topology, schedule, AND coverage evidence rather than a bare front list — `keys`/`requires`/`adjacency` the produced set, the producer-graph adjacency, and its in-set narrowing; `dangling` the parent keys NO node mints (`frozenset(parent for node in nodes for parent in node.parents) - keys`, the silent external-node trap `graphlib.TopologicalSorter.add(key, *parents)` would treat as already-done); `collided` the keys minted by MORE than one node (`Counter`-derived, the last-wins drop `Map.of_seq` would otherwise absorb so a downstream consumer of the silently-dropped producer plans a lie); `untargeted` the requested `targets` deliverables the producer set never mints (`self.targets - keys` after scoping, the target-axis counterpart of `dangling`); `elided` the `keyed` nodes the threaded `cache_seed` already carries; `cyclic` the `CycleError.args[1]` chain; the derived `profile`/`cardinality`/`depth`/`width`/`contention`/`live` front shape and the `roots`/`leaves` source/sink sets over `adjacency`; and the `schedule`-projected `critical_path`/`span`/`makespan`. The `dangling`, `collided`, and `untargeted` sets are the real correctness fixes — a typo'd `FigureRef.asset_key` parent produces a quietly-truncated front order with no error, two producers claiming one content key drop one without a signal, and a partial rebuild scoped to a non-existent deliverable plans an empty/partial graph silently — all three surfaced as actionable plan evidence. The derived `PipelinePlan.severed: Option[PlanFault]` is the one cause-carrying coverage verdict the composition root reads before the drive — `cyclic` first, `untargeted` next, `collided` then, `dangling` last, `Nothing` for a sound plan — so the root recovers on WHICH cause (re-wire a cycle, re-key a target request, dedupe a collision, re-key a missing edge) rather than a `bool(self.dangling or self.cyclic or self.collided or self.untargeted)` collapsing all to one undifferentiated truth. The `frozenset` evidence is the immutable order-insensitive owner; the `graphlib` external-node absorption and `CycleError.args[1]` payload verify against the stdlib `TopologicalSorter` contract the `runtime/execution#LANE` `StagePlan` also drives, the `Counter` multiset against `collections.Counter`, the `roots`/`leaves` folds against the `parents` reachability the edges encode.
- [COLLISION_COVERAGE] [RESOLVED]: a content-keyed producer graph where two `ArtifactWork` nodes mint the same `ContentKey` is a real coverage failure the prior page silently absorbed — `Map.of_seq((node.key, node) for node in nodes)` is last-wins, so the second producer overwrites the first in `by_key`/`requires`/`weights` and the plan schedules and drives only one, the other's dependents either rewired to the survivor or left `dangling`. `_plan` captures `collided = frozenset(key for key, count in Counter(node.key for node in self.nodes).items() if count > 1)` and `severed` carries it as `PlanFault(collided=...)` between `cyclic` and `dangling`, so a double-claimed artifact identity (a genuine duplicate to dedupe, or a key-derivation bug producing a collision the content-addressing should have separated) surfaces as a typed cause carrying its offending keys rather than a quietly-dropped producer. `collections.Counter` is the stdlib multiset for the count, not banned by the doctrine import policy; the `collided` set rides the owner-derived `PipelinePlan.facts` projection as `len(self.collided)` beside its `dangling`/`cyclic` siblings.
- [TARGET_SCOPE] [RESOLVED]: `ArtifactPipeline.of(..., targets=frozenset())` scopes the plan to the requested-deliverable ancestor closure — when `targets` is non-empty, `_reachable` runs one `tailrec` fixpoint walking the `parents` edges UP from the targets (the `seen` set bounding the walk over a not-yet-cycle-checked graph) and `nodes.filter(lambda node: node.key in closure)` keeps exactly the targets and their transitive ancestors, every other producer dropped before `_plan` orders one front, so a partial rebuild ("rebuild the cover page and its bound figures") plans precisely that sub-graph rather than the entire producer set. The closure includes every ancestor of every kept node, so pruning never spuriously fires `dangling` (a kept node's parents are all kept); the discriminant is the `targets` value itself (empty = the whole graph), never a `mode`/`partial` flag. A requested target the producer set never mints is the target-axis counterpart of `dangling`: `_reachable` would otherwise drop it into an empty/partial plan silently, so `ArtifactPipeline` retains the requested `targets` and `_plan` computes `untargeted = self.targets - keys` after scoping, surfacing it as a `PlanFault(untargeted=...)` the `severed` verdict refuses ahead of `collided`/`dangling` — the same silent-truncation defect the page refuses on the parent and identity axes, closed on the request axis. `tailrec`/`TailCall`/`TailCallResult` and `Block.filter`/`Map.try_find` verify against `.api/expression.md`; the ancestor-closure fixpoint is the doctrinal bounded-iteration form (surfaces `[ITERATIVE_DISPATCH]`) over an open recursion that would grow the stack on a deep chain.
- [TOPO_FRONTS] [RESOLVED]: `_plan` builds one stdlib `graphlib.TopologicalSorter[ContentKey]` keyed by each node's `ContentKey` with each dependency edge added through `add(node.key, *node.parents)` inline, and drives the active `prepare`/`is_active`/`get_ready`/`done` protocol so each `get_ready()` tuple is one dependency-level front — the same stdlib active-driver PATTERN the `runtime/execution#LANE` `StagePlan.execute` runs over its named-stage graph, but NOT a `StagePlan` the plan could reuse: `StagePlan` keys stages by `str` and folds every unit into `Admit(retried=...)`, so a `ContentKey`-keyed producer graph whose nodes mix `keyed`/`bare`/`retried` admission cannot be expressed as a `StagePlan`. The sorter build, the `Schedule.of` CPM passes, the min-slack `Block.sort_with`, and the per-front `Block.choose` lowering are the loop's own steps, not single-call `_topo`/`_front_units`/`_heights` hops. The plan resolves the front ORDER and the schedule (a pure projection over the producer graph) while the runtime `LanePolicy.drain` owns the front DRIVE. A `graphlib.CycleError` raised by `prepare` is caught inside `_plan` and its `args[1]` cyclic-node chain folds into `PipelinePlan.cyclic` (an empty-front, empty-`Schedule` plan the `severed` verdict refuses). `graphlib.TopologicalSorter` is stdlib; the active `prepare`/`get_ready`/`done` mode and the static `static_order` mode (inside `Schedule.of`) and the `CycleError.args[1]` chain payload verify against the `runtime/execution#LANE` `[STAGE_FRONT_CONCURRENCY]` resolution that runs the same driver.
- [CONTENT_KEY_NODES] [RESOLVED]: each `ArtifactWork.key` is the `evidence/identity#IDENTITY` `ContentKey` the producing sub-domain's `_emit` already minted through `ContentIdentity.of(...)` — the `document/emit#DOCUMENT` `produce`, the `document/report#REPORT` `render`, the `visualization/chart/export#CHART`/`visualization/table#TABLE`/`scene#SCENE` arms, the `package#BUNDLE` `pack`, and the `exchange/credential#PROVENANCE` sign all return a content-keyed `RuntimeRail`, so the plan consumes the pre-minted key and re-mints nothing; each dependency edge is a parent `ContentKey`, so a report node's `parents` are exactly its bound `FigureRef.asset_key` content keys and the `graphlib.TopologicalSorter` orders the graph by content-key reachability. The `ContentKey` identity satisfies the `graphlib` hashable-node requirement because `ContentKey` is a `Struct(frozen=True, gc=False)` per the `evidence/identity#IDENTITY` owner. The `ContentIdentity`/`ContentKey` spellings verify against the `evidence/identity#IDENTITY` owner; the per-producer `ContentIdentity.of(...)` derivations verify against each producer page's `_emit`.
- [RECEIPT_CONSUMER] [RESOLVED]: the plan contributes no `ArtifactReceipt` CASE and is the third reuse-fabric CONSUMER of the one `core/receipt#RECEIPT` `contribute` fold, named as such in `core/receipt#SIGNALS [PLAN_FABRIC]` beside the `runtime/execution` lane `(ContentKey, Work)` elision and the `runtime/observability` `MeterProvider` signal stream — each producer's `Work[ArtifactReceipt]` coroutine resolves to one `ArtifactReceipt` whose `contribute` fold the runtime lane elision reads as the keyed hit/miss evidence, so the plan reads the SAME fold rather than a parallel plan-receipt rail. The plan imports no producer module because each `Work` thunk carries its own producer call, so no producer→plan→producer cycle forms. The consume-side obligation `core/receipt#SIGNALS [PLAN_FABRIC]` names — the planner minting its OWN `Receipt.of("artifacts", ("planned", "pipeline.plan", facts))` over the pipeline-topology facts — is realized HERE: `ArtifactPipeline.plan` threads the module-level `_emitted` tap over the resolved-plan rail so a sound plan fires `Signals.emit(Receipt.of("artifacts", ("planned", "pipeline.plan", plan.facts)), _PLAN_REDACTION)` exactly once over the owner-derived `PipelinePlan.facts` projection (the plan derives its own observability view exactly as `core/receipt#RECEIPT` `ArtifactReceipt._facts` does, not an `ArtifactPipeline`-side fact map), the `runtime/observability#RECEIPT` `Receipt.of` `(Phase, subject, facts)` triple minting the `fact` case the `PHASE_LEVEL` table logs at `debug`. That `facts` projection carries the native int/float coverage AND CPM shape (`cardinality`/`live`/`depth`/`width`/`contention`/`critical_path`/`makespan`/`span`/`critical`/`elided`/`dangling`/`cyclic`/`collided`/`roots`/`leaves`) the receipts `Encoder(enc_hook=repr, order="deterministic")` renderer serializes without a `str()` coerce, plus the `severed.map(lambda fault: fault.tag).default_value("ok")` cause — the `cardinality`/`depth`/`width`/`critical_path` facts `core/receipt#SIGNALS [PLAN_FABRIC]` names as the planner's topology facts, the full `dangling`/`cyclic`/`collided` coverage triad reported beside `severed`'s gating tag so the debug line names both the failure cause and its magnitude. `_emitted` fires on the resolved-plan arm so a coverage-severed plan still rides the rail. The `Receipt.of`/`Signals.emit`/`Redaction(classified=Map.empty())` spellings verify against the `runtime/observability#RECEIPT` owner, the `Phase` `planned` member and `PHASE_LEVEL` `debug` row against its `type Phase = Literal["admitted", "planned", "emitted"]`, and the `[PLAN_FABRIC]` consumer naming against `core/receipt#RECEIPT`'s `[03]-[SIGNALS]` block.
- [NO_LANE_MECHANICS] [RESOLVED]: the plan owns the producer-graph-to-`Admit` PROJECTION, the front ORDER, the CPM SCHEDULE, the min-slack front lowering, and its topology/coverage evidence, and owns none of the lane mechanics — no `CapacityLimiter` (the `runtime/execution#LANE` `LanePolicy.limiter` `functools.cache`-memoised `_limiter` owns it), no `move_on_after` scope, no `guard(cls)` binding, no `DrainReceipt` construction, no DRAIN-time `cache.try_find` probe to DECIDE a hit, and no producer execution. The plan's own `Map.try_find` is the node-lookup over its OWN `by_key` index and the `cache_seed.keys()` read derives the `elided` evidence set — a pure projection over the threaded cache, NOT a drain-time decision the lane owns. The `LanePolicy` is ONE policy value the plan carries and the lane drives under (the `capacity` + `Option[float]` deadline), never split into loose scalars; `lane.capacity` is read only by `contention` and `makespan` as a saturation/area-bound divisor, not to mint a limiter. The plan carries an EMPTY EXTERNAL-LIBRARY set — `RetryClass` is a sibling `reliability/resilience#RESILIENCE` runtime owner, not a PyPI dependency, so the plan reaches no package beyond the stdlib `graphlib`/`collections.Counter`, the substrate `expression`/`msgspec`, and the runtime/receipt owners it composes (no `frozendict` — the admission lowering is the node's own `match`, the evidence sets are `frozenset`, the schedule is `expression` folds over `graphlib` order).
