# [PY_ARTIFACTS_PLAN]

The content-keyed artifact-production planning layer over the runtime session lane. `ArtifactPipeline` is ONE plan owner that folds a producer dependency graph into the `runtime/execution#LANE` admission shape AND schedules it: each `ArtifactWork` node carries a producer's pre-minted `ContentIdentity`-derived `ContentKey`, its `Work[ArtifactReceipt]` coroutine, its parent content keys, its `cost` work-weight, and the closed `Admission` case it enters the lane on, one `rustworkx.PyDiGraph` over that content-key producer graph resolves the dependency-level fronts through `topological_generations`, one `Schedule.of` Critical-Path-Method pass over the same in-set digraph settles every node's earliest/latest finish so the plan reports the cost-weighted `critical_path`, the hop `span`, per-node `slack`, the zero-slack `critical` chain, and the bounded-capacity `makespan`, and each front is min-slack-ordered before lowering so the lane admits the latency-critical producers first under a saturated `CapacityLimiter` — the `cost` model now DRIVING the order it only reported before. `[04]-[DEPENDENCY_POLICY]` forbids hand-rolling an admitted dependency's functions, so the topology is the Rust-core `rustworkx` graph surface end to end — `topological_generations` the fronts, `topological_sort` the CPM walk order, `predecessor_indices`/`successor_indices` the CPM adjacency, `digraph_find_cycle` the cycle gate, `ancestors` the target closure — never a re-hand-rolled `graphlib` drive or a `tailrec` parent-walk. Each node lowers through its own `ArtifactWork.lowered` admission `match` into its `runtime/execution#LANE` `Admit` case — a `keyed` node to the `(ContentKey, Work)` cache unit so a downstream front re-admitting an already-produced artifact replays the upstream `Ok` from the threaded session cache rather than re-rendering, a `bare` node to a forced-live unit, a `retried` node to a transient-guarded unit re-spawning a worker cold-start under the `RetryClass` its admission case mandates. It owns NO cache, NO store, NO scheduler, NO drain, and NO retry binding — the `runtime/execution#LANE` `LanePolicy` owns the `CapacityLimiter`/`move_on_after`/`DrainReceipt`, the front-to-front `DrainReceipt.cache` thread, and the concurrent per-front drive, and the `runtime/execution#LANE` `Admit` cases own the content short-circuit, the always-run, and the `guard(cls)` retry; `ArtifactPipeline` only PROJECTS the producer graph into those runtime ports, resolving the front ORDER and the schedule while the lane resolves the front DRIVE, and surfaces its own topology, schedule, and coverage evidence — `keys`/`dangling`/`elided`/`cyclic`/`collided`/`untargeted` the coverage sets, `profile`/`cardinality`/`depth`/`width`/`contention`/`live` the derived front shape (`contention` the peak over-subscription of the lane's `CapacityLimiter` the front structure conceals), `roots`/`leaves` the source-artifact and terminal-deliverable DAG facts, `schedule` the one CPM owner from which `critical_path`/`span`/`makespan`/`slack`/`critical` project, and `severed: Option[PlanFault]` the cause-carrying coverage verdict (`cyclic` versus `untargeted` versus `collided` versus `dangling`, never a `bool` erasing which). The synchronous `rustworkx` graph kernel is GIL-releasing native CPU work, so `_compute` crosses onto an `anyio.to_thread` worker bounded by a module `CapacityLimiter` — never the event loop, where a large producer-set build/generations/CPM/cycle sweep would starve the scheduler (OFFLOAD_LANE). The runtime `StagePlan` is NOT this driver: it keys stages by `str`, drives a `graphlib.TopologicalSorter`, and forces every unit `retried`, so it cannot host a `ContentKey`-keyed cost-weighted graph whose nodes mix `keyed`/`bare`/`retried` admission; the plan emits the front-ordered `Block[Admit]` tuple the lane's `drain` folds over directly, and the two share ONLY that hand-off shape, never the graph engine. The plan is the third reuse-fabric consumer of the one `core/receipt#RECEIPT` `contribute` fold beside the `runtime/execution` lane elision and the `runtime/observability` `MeterProvider` signal stream — it reads each producer's contributed `ArtifactReceipt` as the content-keyed evidence the elision distinguishes hit from miss on, AND emits its own `planned`-phase `Receipt` `fact` (the `core/receipt#SIGNALS [PLAN_FABRIC]` consumer obligation) carrying the topology + CPM shape onto that one stream so the planning stage is observable beside the `admitted`/`emitted` producer facts. `ArtifactPipeline.plan` returns a `RuntimeRail[PipelinePlan]` carrying the front-ordered `Block[Admit[ArtifactReceipt]]` lanes — emitting the planned-phase fact on the resolved-plan arm — never an executed result.

## [01]-[INDEX]

- [01]-[PLAN]: content-keyed production-planning AND scheduling axis folding the `ArtifactWork` producer-node graph into the `runtime/execution#LANE` `Admit` admission shape — the `ArtifactWork` node value object carrying a producer's `ContentKey`, its `Work[ArtifactReceipt]` coroutine, its dependency parents, its `cost` work-weight, and its closed `Admission` lane-case union (the `retried` case carrying its mandatory `RetryClass`), the `ArtifactWork.lowered` admission `match` lowering each node to its `bare`/`keyed`/`retried` `Admit` case, the `rustworkx.PyDiGraph` producer graph whose `topological_generations` resolves the dependency-level fronts and `digraph_find_cycle` gates the cycle, the `Schedule` Critical-Path-Method owner whose one forward/backward `Block.fold` pair over the native `topological_sort` order settles every node's `(earliest_finish, hop)` and `latest_finish` so `duration`/`span`/`slack`/`critical`/`makespan` project from one memo, the min-slack front ordering the lane admits under, the `PipelinePlan` front-ordered `Block[Admit[ArtifactReceipt]]` projection (plus its `keys`/`dangling`/`elided`/`cyclic`/`collided`/`untargeted` coverage sets, its `requires` adjacency, its `profile`/`cardinality`/`depth`/`width`/`contention`/`live`/`roots`/`leaves` derived front shape, its `schedule`-projected `critical_path`/`span`/`makespan`, and its `severed: Option[PlanFault]` cause-carrying coverage verdict) the runtime lane drains, the `rustworkx.ancestors` target-closure scope a partial rebuild plans against, the `anyio.to_thread` offload the native graph kernel crosses on, the `planned`-phase `Receipt` `fact` the plan emits onto the one receipt stream, and the one `LanePolicy` policy value the plan carries and the lane drives under.

## [02]-[PLAN]

- Owner: `ArtifactPipeline` the one production-planning-and-scheduling axis folding the `ArtifactWork` producer-node graph into `runtime/execution#LANE` admission units; `ArtifactWork` the frozen producer-node value object carrying the producer's pre-minted `key: ContentKey` (the `ContentIdentity.of(...)` derivation every producer `_emit` already returns), the `work: Work[ArtifactReceipt]` thunk (the producer's own `RuntimeRail[ArtifactReceipt]`-returning coroutine the plan never invokes, only schedules), the `parents: tuple[ContentKey, ...]` upstream content keys the node depends on, the `cost: float` estimated work-weight (render-time seconds / byte-volume, a unit default) the CPM forward pass sums into earliest-finish, and its `admission: Admission` lane-case union owning its own `lowered` `match`, so a node IS its content key, a dependency IS a content-key edge, and the lane case it enters IS one closed vocabulary member carrying exactly its case payload — never a node id beside the key, never a path, never a hardcoded single admission case for a three-case lane union, and never an `admission` tag beside a parallel `Option[RetryClass]` field where the `retried` case already carries the class; `Admission` the closed `@tagged_union` (`keyed`/`bare`/`retried`) selecting which `runtime/execution#LANE` `Admit` case a node lowers to, `keyed` the cache-eligible default the elision is built on (empty payload), `bare` a forced-live non-content-addressable one-shot (empty payload), `retried` a transient-prone offload producer carrying its mandatory `RetryClass` so a `retried` node states its class or is not retried — never a `RetryClass.OCCT` default papering over an absent class the case forbids by construction; `PlanFault` the closed `dangling`/`cyclic`/`collided`/`untargeted` coverage-failure vocabulary the `severed` verdict carries, each case holding its offending `frozenset[ContentKey]` so the root recovers on the cause (re-key a typo'd upstream edge, re-wire a producer cycle, dedupe-re-key a double-claimed artifact identity, or re-key a requested target the producer set never mints) rather than a `bool` collapsing all to one undifferentiated "broken"; `Schedule` the Critical-Path-Method evidence owner computed ONCE over the in-set `rustworkx.PyDiGraph` — `finish: Map[ContentKey, tuple[float, int]]` the per-node `(earliest_finish, hop)` forward pass and `latest: Map[ContentKey, float]` the latest-finish backward pass and `work: float` the total live cost, from which `duration` (the cost-weighted critical path), `span` (the hop-count longest chain), `slack` (per-node total float `latest - earliest_finish`), and `critical` (the zero-slack chain) all PROJECT, so neither metric re-walks the graph where the naive page rebuilt the whole topological memo twice (once for `critical_path`, once for `span`); the passes walk the native `topological_sort` order and read `predecessor_indices`/`successor_indices` for adjacency, but the node-weighted per-node earliest/latest/slack memo stays own-code because `rustworkx.dag_weighted_longest_path` weighs EDGES and yields only the single longest path, carrying neither node weights nor the per-node total float the min-slack order and the critical set read; `PipelinePlan` the frozen plan projection carrying the dependency-front-ordered `fronts: tuple[Block[Admit[ArtifactReceipt]], ...]` (one min-slack-ordered `Block` per `topological_generations` level), the `requires: Map[ContentKey, tuple[ContentKey, ...]]` producer-graph adjacency the DAG-shape evidence derives from, the `schedule: Schedule` CPM evidence computed once (it carries the per-node `cost`-weighted finish, so the raw `weights` map is the `_compute` local the CPM consumes, never a redundant plan field), the resolved `cache_seed: Map[ContentKey, ArtifactReceipt]` the runtime drain threads forward, the `keys: frozenset[ContentKey]` the plan produces, the `dangling: frozenset[ContentKey]` parent keys no node mints (the silent external-node trap `graphlib` would treat as already-done), the `elided: frozenset[ContentKey]` `keyed` nodes the threaded `cache_seed` already carries (the short-circuit set `keyed` keys ∩ cache keys), the `cyclic: frozenset[ContentKey]` chain `digraph_find_cycle` named, the `collided: frozenset[ContentKey]` keys minted by more than one node (the last-wins drop `Map.of_seq` would otherwise absorb silently), and the `untargeted: frozenset[ContentKey]` requested `targets` deliverables the producer set never mints (the target-axis silent-truncation `rustworkx.ancestors` would otherwise drop into an empty/partial plan), with `profile`/`cardinality`/`depth`/`width`/`contention`/`live` deriving the per-front size vector, the total node count, the front count, the widest front, the peak over-subscription `max(width - lane.capacity, 0)` the `CapacityLimiter` queues, and the units that reach a coroutine (`cardinality - len(elided)`), `roots`/`leaves` the source-artifact and terminal-deliverable sets over `adjacency`, `critical_path`/`span`/`makespan` projecting the `schedule`'s cost-weighted serial latency, hop depth, and Graham bounded-capacity floor (`max(critical_path, work / lane.capacity)`), and `severed: Option[PlanFault]` the cause-carrying coverage verdict (`cyclic` then `untargeted` then `collided` then `dangling`, `Nothing` for a sound plan) the composition root reads before driving — never an executed `DrainReceipt`, never a flat unordered unit list that loses the front structure, never a topology that drops the dangling/cyclic/collided/elision/source-sink/critical-path/slack/contention evidence, an unweighted hop count masquerading as the serial-latency bound, a recomputed-per-access schedule, or a `bool` verdict erasing which coverage cause the consumer must act on; `LanePolicy` the `runtime/execution#LANE` bounded-drain owner the plan carries as ONE policy value (capacity + `Deadline.seconds`-derived `Option[float]` budget) and the lane drives under, the plan minting no limiter, no scope, and never splitting the policy into loose `capacity`/`deadline` scalars; `Admit` the `runtime/execution#LANE` closed admission union whose `bare`/`keyed`/`retried` cases each node lowers into through its own `ArtifactWork.lowered` `match`, the plan re-spelling no admission shape; the projection-and-schedule layer between the producer graph and the runtime session lane.
- Cases: `_compute` is the one synchronous boundary kernel offloaded onto `anyio.to_thread` — it scopes the node set (`_scoped` when `targets` is non-empty), indexes `by_key`, builds the `requires` adjacency and the `weights` cost map, derives `keys`/`dangling`/`elided`, captures `collided` from one `Counter` over the node keys (the double-claimed identities `Map.of_seq` last-wins would otherwise drop without a signal), narrows the producer graph to one `rustworkx.PyDiGraph` through `_digraph` (each unique `ContentKey` a node whose payload IS the key, each in-set `parents` edge a `parent -> child` arc), gates the cycle through `digraph_find_cycle` (its offending-edge endpoints folding into `PipelinePlan.cyclic` and an empty-front, empty-`Schedule` plan the `severed` verdict refuses, never a raise the caller must trap), runs one `Schedule.of(graph, weights, elided)` settling the CPM passes once, binds `priorities = schedule.slack` ONCE (the property rebuilds the slack map per access, so reading it inside the front comprehension would re-pay the projection per front), and projects each `topological_generations(graph)` front back through `by_key` with `Block.of_seq(front).sort_with(...)` min-slack-ordering the indices before `Block.choose` over the `try_find` `Option` lowers each resolved node through its own `ArtifactWork.lowered` admission `match` into its `Admit` case — a `keyed` node to `Admit(keyed=(key, work))` the lane's `ADMIT_TABLE` `keyed` row probes the threaded session cache with (a `ContentKey` already carrying an `Ok` short-circuits without re-rendering and increments the lane `hit`), a `bare` node to `Admit(bare=work)` that always runs, a `retried` node to `Admit(retried=(cls, work))` whose `guard(cls)` re-spawns a transient worker cold-start under the `RetryClass` the admission case carried. There is no single-call `_topo`/`_front_units`/`_heights` hop and no `graphlib` active `prepare`/`is_active`/`get_ready`/`done` drive — the graph build, the `topological_generations` fronts, the `Schedule.of` CPM passes, the min-slack sort, and the per-front `Block.choose` lowering are the kernel's own steps over the native surface. `_digraph` is the one graph-build kernel (shared by `_compute` and `_scoped`): `add_nodes_from` over the unique keys stores each key as its node payload so `get_node_data` is the index-to-key projection with no parallel bijection map, and one edge loop adds each in-set `parent -> child` arc (`multigraph=False` folding a doubly-declared edge to one). `_scoped` is the `rustworkx.ancestors` target closure — the graph-native replacement for the deleted `tailrec` parent-walk — building the full producer digraph, unioning each in-set target's `ancestors` index set with the targets themselves, projecting back through `get_node_data`, and keeping exactly that ancestor sub-graph a partial rebuild needs. `_compute` assembles the per-front `Block` tuple directly from the `topological_generations` comprehension (no mutable `list` and no `Block.append` accumulator — the generations ARE the fronts) into one `PipelinePlan` carrying the front order, the `requires` adjacency, the `schedule` CPM evidence, the `cache_seed`, the produced `keys`, and the `dangling`/`elided`/`collided`/`untargeted` coverage sets, the plan owning the ELISION SHAPE, the topology, the CPM schedule, and the coverage evidence while the runtime lane owns the drain; the resolved-plan rail then threads the module-level `_emitted` tap firing the one `planned`-phase `Signals.emit(Receipt.of("artifacts", ("planned", "pipeline.plan", plan.facts)), OPEN)` over the owner-derived `PipelinePlan.facts` projection so the planning stage rides the receipt stream.
- Modality: `ArtifactPipeline.plan` is `async` over the runtime `async_boundary`, folding the producer graph inside the one fault capsule and offloading the synchronous `rustworkx` kernel onto `anyio.to_thread.run_sync(self._compute, limiter=_PLAN_LANES)` so the GIL-releasing native build/generations/CPM/cycle sweep never runs on the event loop — and returns a `RuntimeRail[PipelinePlan]`; the runtime owner then drives the plan by draining each front through `LanePolicy.drain(front, carried)` and threading each front's `DrainReceipt.cache` into the next, the plan itself executing nothing. One `ArtifactPipeline.of` owns every construction arity — a lone `ArtifactWork` or any `Iterable[ArtifactWork]` normalized once at the head by input shape, never a `single`/`many` suffix pair — and its keyword-only `targets: frozenset[ContentKey]` scopes the plan to the requested-deliverable ancestor closure, an empty set planning the whole graph, so a caller rebuilding one cover page and its bound figures plans exactly that sub-graph rather than the entire producer set, the discriminant the value of `targets` itself, never a `mode`/`partial` flag; `of` retains the requested `targets` unscoped and `_compute` computes `targets - keys` against the FULL node set after scoping, so a requested target the producer set never mints surfaces as `untargeted` evidence the `severed` verdict refuses rather than a silently-empty plan. A cyclic producer graph surfaces as `PipelinePlan.cyclic` evidence the `severed` verdict gates on, `digraph_find_cycle` returning the offending edges inside `_compute` so the rail still carries a structured plan rather than a raise.
- Auto: the producer-node graph is content-addressed end to end — each `ArtifactWork.key` is the `evidence/identity#IDENTITY` `ContentKey` the producing sub-domain's `_emit` already minted (every production sub-domain returns `ContentIdentity.of(...)`: the `document/emit#DOCUMENT`, `document/report#REPORT`, `document/egress#FINISH`, and `document/lens#LENS` document arms; the `visualization/chart/export#CHART`, `visualization/table#TABLE`, and `visualization/diagram/layout#LAYOUT` visual arms; `scene#SCENE`; `package#BUNDLE`; the `graphic/marks#MARK`+`graphic/raster/measure#MEASURE`+`graphic/color/managed#COLOR_MANAGED` `Preview`-minting arms; the `typography/font#FONT`/`shape#SHAPE`/`layout#LAYOUT` `Document`-rail arms (an axis catalog, positioned glyph run, or line-broken stream that is neither a PDF nor an office file); the `exchange/credential#PROVENANCE`, `exchange/metadata#METADATA`, and `exchange/conformance#CONFORMANCE` exchange arms; the AEC drawing plane `drawing/dimension#DIMENSION`/`annotate`/`symbol`/`detail`/`schedule#SCHEDULE`; the `composition/sheet#SHEET` titled-sheet arm (its `Pdf`/`Egress`/`Preview` cases keyed on the placed figures' parent keys); the `export/dxf#DXF` CAD arm; the `export/layered#LAYERED`/`export/indesign#INDESIGN` editable-export arms; the `specification/section` spec-into-`document/model` authoring and the `delivery/register`+`delivery/transmittal` assembly; and the `media/*` pages), so the plan re-mints no key and a settings change that shifts a producer's content key correctly misses the cache; each dependency edge is a parent `ContentKey`, so the `rustworkx.PyDiGraph` orders the graph by content-key reachability and a report node's `parents` are exactly its bound `FigureRef.asset_key` content keys; a parent `ContentKey` no node in the set produces is captured once into `PipelinePlan.dangling` rather than silently absorbed as an already-done external node; two nodes minting one `ContentKey` are captured into `PipelinePlan.collided` rather than dropped last-wins by `Map.of_seq`, so a double-claimed artifact identity surfaces as plan evidence instead of a quietly-missing producer; a `keyed` node whose key already resolved into the threaded `cache_seed` is captured into `PipelinePlan.elided` so the plan reports the short-circuit set (and the `live` count) before the lane probes one key; the `Schedule.of` forward pass treats an `elided` cache hit as zero cost (it never runs) yet still one hop (it still gates its dependents), so `critical_path`/`makespan` collapse correctly on a warm replay; each node lowers through its own `ArtifactWork.lowered` admission `match` so the runtime lane owns the short-circuit, the always-run, and the transient retry by `Admit` case — the plan never probes a cache to DECIDE a drain, opens a scope, or binds a retry itself, it only shapes and orders the units the lane drives; the `cache_seed` is the only mutable-by-reference state the plan threads, a `runtime/execution#LANE` `DrainReceipt.cache` `Map[ContentKey, ArtifactReceipt]` the runtime minted carried forward for a warm replay, never a durable store the plan owns.
- Receipt: the plan contributes no `ArtifactReceipt` CASE of its own — it is the CONSUMER of the one `core/receipt#RECEIPT` `contribute` fold every producer satisfies, reading each producer's contributed `ArtifactReceipt` as the keyed evidence the sub-graph elision distinguishes a hit from a miss on, the third reuse-fabric consumer of that single fold beside the `runtime/execution` lane `(ContentKey, Work)` elision and the `runtime/observability` `MeterProvider` signal stream named in `core/receipt#SIGNALS`. It DOES emit its own `planned`-phase fact on that one stream, the consumer-side obligation `core/receipt#SIGNALS [PLAN_FABRIC]` names: `plan` threads the module-level `_emitted` tap over the resolved-plan rail to fire `Signals.emit(Receipt.of("artifacts", ("planned", "pipeline.plan", plan.facts)), OPEN)` once per plan — the `runtime/observability#RECEIPT` `Receipt.of` `(Phase, subject, facts)` triple minting the `fact` case the `PHASE_LEVEL` table logs at `debug`, so the planning stage is observable on the same line family the `admitted`/`emitted` producer facts ride. The facts map is the owner-derived `PipelinePlan.facts` property (the plan projects its own observability view, mirroring the `core/receipt#RECEIPT` `ArtifactReceipt._facts` shape — never an `ArtifactPipeline`-side `_evidence` hop) and carries the native-int/float coverage AND CPM shape (`cardinality`/`live`/`depth`/`width`/`contention`/`critical_path`/`makespan`/`span`/`critical`/`elided`/`dangling`/`cyclic`/`collided`/`roots`/`leaves`) the receipts `Encoder(enc_hook=repr, order="deterministic")` renderer serializes unstringified plus the `severed.map(tag).default_value("ok")` cause, never a `str()`-pre-formatted scalar. `_emitted` fires only on the resolved-plan arm so a coverage-severed plan still rides the rail as evidence rather than a blind emit, and the plan owns no `Metrics.observe`/`Signals.emit` for the EXECUTED drain — the `runtime/execution#LANE` `@drained` aspect owns that as the front drains. The executed plan's per-front `runtime/execution#LANE` `DrainReceipt[ArtifactReceipt]` carries the `values`/`cache`/`hit`/`completed` that ARE the elision evidence — the plan shapes, schedules, and emits the planned-phase fact, the runtime drain produces the drain receipt, and the `core/receipt#RECEIPT` `contribute` fold both consume.
- Packages: `rustworkx` (`PyDiGraph`/`add_nodes_from`/`add_edge`/`get_node_data`/`node_indices`/`predecessor_indices`/`successor_indices` the producer digraph build over content-key node payloads, `topological_generations` the dependency-level fronts, `topological_sort` the CPM forward/backward walk order, `digraph_find_cycle` the cycle gate returning the offending `EdgeList`, `ancestors` the target ancestor closure — the Rust-core DAG surface `[04]-[DEPENDENCY_POLICY]` mandates over a re-hand-rolled `graphlib` drive, deferred through `lazy import` so its native load lands on first `_compute` and reifies under the import lock inside the `to_thread` worker); `anyio` (`to_thread.run_sync` offloading the synchronous GIL-releasing `rustworkx` kernel off the event loop, `CapacityLimiter` bounding the offload lane at `os.process_cpu_count()`); `expression` (`Block.of_seq`/`Block.choose`/`Block.fold`/`Block.sort_with`/`Block.filter`/`Block.singleton` the front projection, the two CPM `Block.fold` passes, the min-slack front ordering, and the `_scoped` closure filter; `Map.of_seq`/`Map.try_find`/`Map.empty`/`Map.keys`/`Map.map`/`Map.items`/`Map.values`/`Map.add`/`map[key]` the node/cost lookup, the CPM `finish`/`latest` accumulators, and the `slack` projection; `Option`/`Some`/`Nothing`/`Option.map`/`Option.default_value` the `severed` cause verdict, the `weights.try_find` cost lookup, and the owner-derived `PipelinePlan.facts` projection; `tagged_union`/`tag`/`case` declaring the `Admission` and `PlanFault` selector unions and consuming the imported `runtime/execution#LANE` `Admit` shape, never re-declared); `collections.Counter` (the stdlib multiset surfacing the `collided` double-claimed keys `Map.of_seq` last-wins would drop); `msgspec` (`Struct(frozen=True)` the `ArtifactWork`/`Schedule`/`PipelinePlan`/`ArtifactPipeline` value objects); `os` (`process_cpu_count` sizing the offload limiter per the runtime worker-sizing owner); runtime (`content_identity.ContentKey` the producer key derivation and node identity, `lanes.Admit`/`LanePolicy`/`Work` the three-case admission shape and the bounded-drain policy value the plan carries and the lane drives under, `resilience.RetryClass` the offload transient class a `retried` `Admission` case mandates, `faults.RuntimeRail`/`async_boundary` the rail and fault capsule, `receipts.Receipt`/`Signals`/`OPEN` the shape-polymorphic `Receipt.of` factory, the `Signals.emit` sink the `planned`-phase fact rides, and the runtime-owned keep-all `Redaction` `OPEN` threaded as the emit's redaction rather than a re-minted `Redaction(classified=Map.empty())` the `runtime/observability#RECEIPT` owner forbids per file); `core/receipt#RECEIPT` (`ArtifactReceipt` the `Work` payload type every node's coroutine resolves to, consumed as keyed evidence, the producer modules never imported because the `Work` thunk carries the producer call). No `frozendict` table — the admission lowering is the node's own `match`, the evidence sets are `frozenset`, the DAG algebra is `rustworkx`, the schedule is `expression` folds over the native order, and the planned-phase emit is the runtime receipt port the producers already ride.
- Growth: a new producer is one `ArtifactWork` node carrying its pre-minted `ContentKey`, its `Work[ArtifactReceipt]` thunk, its parent content keys, its `cost`, and its `Admission` case — `_digraph` places it in the `rustworkx` graph, `topological_generations` fronts it, the `Schedule.of` passes settle its CPM finish, and the `_compute` comprehension min-slack-lowers it through its own `ArtifactWork.lowered` `match` with zero plan edit; a new admission modality is one `Admission` case plus one `lowered` arm (the `assert_never` tail proving total at type-check), never a per-mode method or a parallel lowering table; a transient-prone producer is one `retried` admission carrying its `RetryClass`; a new dependency is one parent `ContentKey` reaching the `requires` adjacency, the graph edge, and the CPM passes for free; a warm replay is one non-empty `cache_seed` the runtime drain threads and the `elided` set (plus the zero-cost CPM contribution) pre-reports; a new bounded-drain trait reaches the plan as one `runtime/execution#LANE` `LanePolicy` field; a new per-node weight axis is one `ArtifactWork` field reaching the `weights` map and the `Schedule` passes for free; a new front-shape or schedule metric is one derived `PipelinePlan` property (`live` over `elided`, `contention` over `width`/`lane.capacity`, `makespan` over `schedule`/`lane.capacity`) or one `Schedule` projection (`slack`/`critical` over `finish`/`latest`) — derived from the existing memo, never a re-walk; a new coverage-failure cause is one `PlanFault` case plus one `severed` arm and one `_compute` detection (the `collided` and `untargeted` cases are exactly that growth — one on the identity axis, one on the target axis); a partial rebuild is one `targets` set on `of`; zero new surface — the plan owns the producer-graph-to-admission projection and the CPM schedule and grows by node, case, derived property, and coverage cause, never by method.
- Boundary: no cache, no durable store, no scheduler, no drain, no `CapacityLimiter` for the DRAIN, no `move_on_after`, no `guard` binding, no `DrainReceipt` construction, and no producer execution — the `runtime/execution#LANE` `LanePolicy` owns the bounded drain, the lane limiter, the active per-front drive, the front-to-front `DrainReceipt.cache` thread, and the `Admit`-case content short-circuit / always-run / transient retry, and each producing sub-domain owns its own render; the plan re-mints no `ContentKey` and owns no second scheduler beside the `runtime/execution#LANE` lane. A node id beside the content key, a path-keyed dependency edge, a hardcoded single `Admit.keyed` lowering where the node's `Admission` case selects among the lane's three, a `bool cached`/`bool retried` knob or an `admission` tag beside a parallel `Option[RetryClass]` field, a `RetryClass.OCCT` default papering over an absent class, a `_LOWER` `frozendict` table or external dispatcher where the node's own `lowered` `match` owns the lowering, a plan-owned `Map[ContentKey, ...]` cache where the runtime `DrainReceipt.cache` is threaded, a plan-built lane `CapacityLimiter`/`move_on_after`/`guard(cls)` where the `LanePolicy` and the `retried` admission own them, a `StagePlan` re-spelling where it forces every unit `retried` and keys by `str`, a re-declared `Admit`/`Work` shape, loose `capacity`/`deadline` scalars where one `LanePolicy` carries the budget, a `.default_value(None)`-then-`is not None` filter where `Block.choose` over the `Option` drops the misses, a mutable `list` front accumulator or a `Block.append` rebind where the `topological_generations` comprehension IS the front tuple, a flat unordered unit list where the front-structured `tuple[Block[Admit], ...]` preserves the levels, a fixed generation order ignoring the cost model where `Block.sort_with` over `schedule.slack` admits the latency-critical producers first, a re-paid slack map inside the front comprehension where `priorities` is bound once, a dropped dangling-parent edge where `PipelinePlan.dangling` surfaces it, a silent `Map.of_seq` last-wins key collision where `Counter`-derived `collided` surfaces it, an inert evidence set with no verdict where `severed`/`live`/`contention`/`roots`/`leaves`/`critical_path`/`span`/`makespan`/`slack`/`critical` act on it, a `bool severed` collapsing the `dangling`/`cyclic`/`collided`/`untargeted` causes to one undifferentiated "broken" where the closed `PlanFault`-carrying `Option[PlanFault]` names the cause and its offending keys, a `depth` front-count OR an unweighted hop count masquerading as the serial-latency bound where the COST-weighted `critical_path` is the latency, the `width` peak `contention` the saturation, and the `makespan` Graham area bound the bounded-capacity wall-clock floor, a per-access `_heights` re-walk recomputing the whole topological memo for `critical_path` and AGAIN for `span` where the one `Schedule.of` settles both passes once and every metric projects, a hand-rolled `graphlib.TopologicalSorter` drive or a `tailrec` `_reachable` parent-walk where `rustworkx.topological_generations`/`digraph_find_cycle`/`ancestors` own the DAG algebra `[04]-[DEPENDENCY_POLICY]` forbids re-implementing, a `dag_weighted_longest_path` cross-check standing in for the node-weighted per-node slack memo it cannot carry, a synchronous `rustworkx` kernel left on the event loop where `anyio.to_thread` offloads it under `_PLAN_LANES`, an exact `== 0.0` slack test where `<= _SLACK_EPS` admits the critical chain over an accumulation-built float, an all-or-nothing plan that cannot scope to requested deliverables where `targets` walks the `rustworkx.ancestors` closure, a silently-dropped unknown target where `_compute`'s `targets - keys` surfaces it as `untargeted` and `severed` refuses the partial plan, a prose-only receipt-fold consumer that emits NOTHING where `_emitted` fires the `planned`-phase `Receipt.of("artifacts", ("planned", ...))` fact over the owner-derived `PipelinePlan.facts`, an `ArtifactPipeline`-side facts map where the `PipelinePlan` owner derives its own observability projection, a hardcoded `emitted` phase blind to the planning stage where the `phase`-modal `Receipt` `fact` case carries `planned`, a plan-owned `Metrics.observe`/`Signals.emit` for the EXECUTED drain where the `runtime/execution#LANE` `@drained` aspect owns it, a `str()`-pre-formatted coverage scalar where the receipts `enc_hook=repr` renderer serializes the native int/float, a per-node `ContentIdentity.of` re-mint where the producer already keyed, a `DrainReceipt` the plan constructs where the runtime drain produces it, and a producer module the plan imports where the `Work` thunk carries the call are the deleted forms — `ArtifactPipeline` is the producer-graph-to-`Admit` projection-and-schedule owner of the elision shape, the topology, the CPM evidence, and its one planned-phase fact, never the lane mechanics.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import os
from collections import Counter
from collections.abc import Iterable
from typing import Final, Literal, assert_never

import anyio.to_thread
from anyio import CapacityLimiter
from expression import Nothing, Option, Some, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct

from rasm.runtime.content_identity import ContentKey
from rasm.runtime.faults import RuntimeRail, async_boundary
from rasm.runtime.lanes import Admit, LanePolicy, Work
from rasm.runtime.receipts import OPEN, Receipt, Signals
from rasm.runtime.resilience import RetryClass

# the Rust graph core defers so its native load lands on first plan compute, not module import; every
# `rx.*` call runs inside the `to_thread` kernel, so the proxy reifies under the import lock off the loop.
lazy import rustworkx as rx

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
# a node is on the critical chain when its total float collapses to (within rounding of) zero, so the
# membership test is `<= _SLACK_EPS`, never an exact `== 0.0` an accumulation-built `latest - finish`
# never lands on.
_SLACK_EPS: Final[float] = 1e-9
# the offload throttle for the synchronous `rustworkx` graph kernel — the native build/generations/CPM/
# cycle sweep crosses onto a GIL-releasing worker thread bounded here, never the per-loop 40-token default.
_PLAN_LANES: Final[CapacityLimiter] = CapacityLimiter(os.process_cpu_count() or 1)

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
    # the Critical-Path-Method evidence computed ONCE over the in-set `rustworkx.PyDiGraph` and carried on
    # the plan: a forward pass settling each node's `(earliest_finish, hop)` and a backward pass settling
    # its `latest_finish` at the project duration, so `duration`/`span`/`slack`/`critical` PROJECT from one
    # pair of `Block.fold`s rather than the per-metric re-walk the prior page paid. rustworkx owns the
    # TOPOLOGY the passes walk (`topological_sort` parents-first order, `predecessor_indices`/
    # `successor_indices` adjacency) but NOT the per-node total-float memo the min-slack front order and the
    # critical set read — `dag_weighted_longest_path` weighs EDGES and yields only the single longest path,
    # carrying neither node weights nor per-node slack — so the two folds stay own-code over the native
    # order. The maps are empty for a cyclic plan the `digraph_find_cycle` gate refuses before `of` runs.
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
    def of(graph: "rx.PyDiGraph[ContentKey, None]", weights: Map[ContentKey, float], elided: frozenset[ContentKey], /) -> "Schedule":
        cost = lambda key: 0.0 if key in elided else weights.try_find(key).default_value(1.0)
        order = tuple(rx.topological_sort(graph))  # NodeIndices, parents-before-children

        # forward pass over the parents-first order: `ef(k) = cost(k) + max(parent ef)`, the parents read
        # off `predecessor_indices` (already settled), so `finish` and the hop depth land in one fold.
        def ahead(memo: Map[ContentKey, tuple[float, int]], idx: int, /) -> Map[ContentKey, tuple[float, int]]:
            key = graph.get_node_data(idx)
            parents = tuple(graph.get_node_data(parent) for parent in graph.predecessor_indices(idx))
            finish = cost(key) + max((memo[parent][0] for parent in parents), default=0.0)
            hop = 1 + max((memo[parent][1] for parent in parents), default=0)
            return memo.add(key, (finish, hop))

        finish = Block.of_seq(order).fold(ahead, Map.empty())
        duration = max((value for value, _ in finish.values()), default=0.0)

        # backward pass over the reversed order (sinks first): `lf(k) = min(lf(child) - cost(child))`, the
        # children read off `successor_indices`, a leaf's latest finish defaulting to the project `duration`.
        def behind(memo: Map[ContentKey, float], idx: int, /) -> Map[ContentKey, float]:
            kids = tuple(graph.get_node_data(child) for child in graph.successor_indices(idx))
            return memo.add(graph.get_node_data(idx), min((memo[kid] - cost(kid) for kid in kids), default=duration))

        latest = Block.of_seq(reversed(order)).fold(behind, Map.empty())
        return Schedule(finish=finish, latest=latest, work=sum(cost(graph.get_node_data(idx)) for idx in order))


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
        # the in-set predecessor projection `roots`/`leaves` each read — every node's `requires` parents
        # narrowed to the keys the set mints, dropping the `dangling` out-of-set edge once. Recomputed per
        # access from `requires ∩ keys` (a frozen `Struct` carries no writable slot for a `cached_property`);
        # derived, never stored beside `requires` it is recoverable from.
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
    # the producer graph as ONE `rustworkx.PyDiGraph` — each unique `ContentKey` is a node whose payload IS
    # the key (so `get_node_data` is the index->key projection with no parallel bijection map), each in-set
    # `parents` edge a `parent -> child` arc; an out-of-set (dangling) parent drops here exactly as the
    # `requires ∩ keys` narrowing does, and `multigraph=False` folds a doubly-declared edge to one. A
    # collided key resolves to one node (last-wins index) whose plan the `collided` `severed` verdict refuses.
    # keys are ordered by `ContentKey.hex` before `add_nodes_from`, NOT by raw `frozenset` iteration: a
    # `frozenset[ContentKey]` iterates in salted-hash order (`str`-hash randomization varies the order per
    # process), so a content-hex sort makes the node-index assignment — and thus the `topological_generations`
    # within-generation order every front inherits — run-STABLE, so one producer set always plans to one graph.
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
    # the requested-deliverable closure via `rustworkx.ancestors` — the graph-native replacement for the
    # deleted hand-rolled `tailrec` parent-walk: build the full producer digraph, union each in-set
    # target's `ancestors` index set with the target itself, project back through `get_node_data`, and keep
    # exactly that ancestor sub-graph so a partial rebuild plans its slice; every kept node's parents are
    # ancestors too, so scoping never spuriously fires `dangling`. An empty closure (every target
    # untargeted) yields an empty scope the `_compute` `untargeted` verdict refuses.
    keys = frozenset(node.key for node in nodes)
    graph = _digraph(nodes, keys)
    index = {graph.get_node_data(idx): idx for idx in graph.node_indices()}
    seeds = frozenset(index[target] for target in targets if target in index)
    closure = frozenset(graph.get_node_data(idx) for seed in seeds for idx in {seed, *rx.ancestors(graph, seed)})
    return nodes.filter(lambda node: node.key in closure)


def _emitted(plan: PipelinePlan, /) -> PipelinePlan:
    # the resolved-plan tap firing the one `planned`-phase fact on the shared receipt stream and returning
    # the plan unchanged — the facts projection is the `PipelinePlan.facts` owner derivation, so this tap
    # only emits and a coverage-severed plan still rides the rail rather than a blind emit.
    Signals.emit(Receipt.of("artifacts", ("planned", "pipeline.plan", plan.facts)), OPEN)
    return plan


class ArtifactPipeline(Struct, frozen=True):
    nodes: Block[ArtifactWork]
    lane: LanePolicy = _DEFAULT_LANE
    cache_seed: Map[ContentKey, ArtifactReceipt] = Map.empty()
    # the requested-deliverable set `of` retains unscoped (empty = whole graph); `_compute` scopes `nodes`
    # to its `rustworkx.ancestors` closure and surfaces a typo'd target the producer set never mints as
    # `untargeted` (`targets - full keys`) rather than a silent drop into an empty plan.
    targets: frozenset[ContentKey] = frozenset()

    async def plan(self, /) -> RuntimeRail[PipelinePlan]:
        # the planner is the third `core/receipt#RECEIPT` `contribute` consumer: a `planned`-phase fact
        # rides the one receipt stream the reuse-fabric elision and the `MeterProvider` read, so the
        # planning stage is observable WITHOUT a parallel plan-receipt rail — `_emitted` fires it only on
        # the resolved-plan arm so a coverage-severed plan still rides the rail rather than a blind emit.
        railed = await async_boundary("pipeline.plan", self._planned)
        return railed.map(_emitted)

    async def _planned(self, /) -> PipelinePlan:
        # the synchronous `rustworkx` graph kernel is GIL-releasing native CPU work, so it crosses onto a
        # `to_thread` worker bounded by `_PLAN_LANES` — never the event loop, where a large producer-set
        # build/generations/CPM/cycle sweep would starve the scheduler (OFFLOAD_LANE).
        return await anyio.to_thread.run_sync(self._compute, limiter=_PLAN_LANES)

    def _compute(self, /) -> PipelinePlan:
        scoped = self.nodes if not self.targets else _scoped(self.nodes, self.targets)
        by_key: Map[ContentKey, ArtifactWork] = Map.of_seq((node.key, node) for node in scoped)
        requires: Map[ContentKey, tuple[ContentKey, ...]] = Map.of_seq((node.key, node.parents) for node in scoped)
        weights: Map[ContentKey, float] = Map.of_seq((node.key, node.cost) for node in scoped)
        keys = frozenset(node.key for node in scoped)
        dangling = frozenset(parent for node in scoped for parent in node.parents) - keys
        # the target-axis counterpart of `dangling`, measured against the FULL producer set: a requested
        # deliverable no node anywhere mints, surfaced rather than silently scoped into an empty plan.
        untargeted = self.targets - frozenset(node.key for node in self.nodes)
        # the silent-collision trap: `Map.of_seq` is last-wins, so two nodes minting one `ContentKey` drop
        # one producer from `by_key`/`requires`/`weights` without a signal — `Counter` surfaces the
        # double-claimed keys as `collided` coverage evidence the `severed` verdict refuses.
        collided = frozenset(key for key, count in Counter(node.key for node in scoped).items() if count > 1)
        # the elision evidence: a `keyed` node whose content key already resolved into the threaded session
        # cache replays the upstream `Ok` in the runtime drain rather than re-rendering.
        elided = frozenset(node.key for node in scoped if node.admission.tag == "keyed") & frozenset(self.cache_seed.keys())
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
                keys=keys,
                dangling=dangling,
                elided=elided,
                cyclic=cyclic,
                collided=collided,
                untargeted=untargeted,
            )
        schedule = Schedule.of(graph, weights, elided)
        # the min-slack priority read ONCE before the front comprehension (the property rebuilds the map per
        # access): each `topological_generations` front sorts ascending by slack so the lane admits the
        # critical producers first — the cost model DRIVING the order it only reported before.
        priorities = schedule.slack
        # `topological_generations` yields the dependency-level fronts directly (rank = list position), one
        # native call replacing the whole `graphlib` active `prepare`/`get_ready`/`done` drive; each front's
        # indices lower through `get_node_data -> by_key -> lowered`, a `dangling` index dropping through
        # `try_find`. The plan ORDERS the fronts, the runtime lane DRIVES them.
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

- [DAG_ENGINE] [RESOLVED]: the producer topology is the Rust-core `rustworkx` graph surface end to end, replacing the prior page's hand-rolled `graphlib.TopologicalSorter` active drive and `tailrec` `_reachable` walk that `[04]-[DEPENDENCY_POLICY]` forbids re-implementing when an admitted dependency owns the concern (`rustworkx` verified installed `0.18.0`, an `abi3` wheel on cp315, Apache-2.0, the shared `data`/`artifacts` substrate per `libs/python/artifacts/.api/rustworkx.md`). `_digraph` builds ONE `rx.PyDiGraph(check_cycle=False, multigraph=False)` whose `add_nodes_from(unique keys)` stores each `ContentKey` as its node payload (so `get_node_data(idx)` is the index-to-key projection with no parallel bijection map) and whose edge loop adds each in-set `parent -> child` arc; `rx.topological_generations(graph)` yields the dependency-level fronts directly (`list[NodeIndices]`, rank = position) replacing the whole `prepare`/`is_active`/`get_ready`/`done` loop; `rx.digraph_find_cycle(graph)` returns the offending `EdgeList` (empty = acyclic) whose endpoints fold into `PipelinePlan.cyclic`, richer than a `CycleError.args[1]` node chain; `rx.topological_sort(graph)` is the parents-first order the CPM `Block.fold` passes walk; `graph.predecessor_indices`/`successor_indices` are the CPM adjacency, deleting the hand-built `adjacency`/`children` maps. The runtime `StagePlan` legitimately keeps `graphlib` for its `str`-keyed stage graph; the artifacts plan is a `ContentKey`-keyed cost-weighted producer DAG rustworkx owns natively, and the two share ONLY the `tuple[Block[Admit], ...]` front hand-off shape, never the engine — a graphlib re-spelling here would re-hand-roll functions the admitted dependency ships. Every cited member verifies against the `rustworkx-0.18.0` `.pyi` (`add_nodes_from -> NodeIndices`, `add_edge`, `get_node_data(node) -> _S`, `node_indices`, `predecessor_indices`/`successor_indices -> NodeIndices`, `topological_generations(dag) -> list[NodeIndices]`, `topological_sort(graph) -> NodeIndices`, `digraph_find_cycle(graph) -> EdgeList` over `tuple[int, int]`, `ancestors(graph, node) -> set[int]`).
- [CRITICAL_PATH_SCHEDULE] [RESOLVED]: `Schedule` is the one Critical-Path-Method owner computed exactly once per plan, replacing the prior page's per-access `_heights` re-walk where `critical_path` and `span` EACH rebuilt the entire memo. `Schedule.of(graph, weights, elided)` runs one forward `Block.fold` over `rx.topological_sort(graph)` settling each node's `(earliest_finish, hop)` as `ef(k) = cost(k) + max(parent ef)` reading parents off `graph.predecessor_indices` (every parent already settled), then one backward `Block.fold` over `reversed(order)` settling `latest_finish` as `lf(k) = min(lf(child) - cost(child))` reading children off `graph.successor_indices` (a leaf defaulting to the project `duration`), so `duration`/`span`/`slack`/`critical` all PROJECT from the one stored `finish`/`latest` pair — an `elided` cache hit contributing zero cost but one hop. The node-weighted per-node earliest/latest/slack memo stays own-code by ADJUDICATION: `rx.dag_weighted_longest_path`/`dag_weighted_longest_path_length` weigh EDGES (`weight_fn: Callable[[int, int, _T], float]`) and yield only the single longest path or its length, carrying neither the per-producer `cost` node weight nor the per-node total float the min-slack front order AND the critical set both require, so composing them would be a redundant, semantically-mismatched second walk — the DAG TOPOLOGY is rustworkx's, the node-weighted CPM float is genuinely own-code the library does not provide. `PipelinePlan.makespan` composes `max(schedule.duration, schedule.work / lane.capacity)`, Graham's bounded-capacity area bound the serial `critical_path` under-reports when the live work cannot fit the lane, complementary to `contention` the peak-width saturation. The `1e-9` `_SLACK_EPS` critical-membership cut guards the float-rounding of an accumulation-built `latest - finish`.
- [FRONT_PRIORITY] [RESOLVED]: each `rx.topological_generations` front is min-slack-ordered before lowering — `Block.of_seq(front).sort_with(lambda idx: (priorities.try_find(graph.get_node_data(idx)).default_value(0.0), graph.get_node_data(idx).hex))` sorts the front's node indices ascending by a `(slack, ContentKey.hex)` composite key: the CPM `slack` orders the zero-slack critical producers first so the runtime `LanePolicy.drain` under one `CapacityLimiter` admits the latency-critical producers before the slack-heavy ones (the `cost` model DRIVING the front order where the prior page left the arbitrary drive order), and the `ContentKey.hex` tail breaks an equal-slack tie DETERMINISTICALLY by content rather than leaving it to the non-deterministic generation order — so the whole front sequence is a reproducible plan artifact, complementing the `ContentKey.hex` node-index ordering in `_digraph`. `priorities = schedule.slack` is bound ONCE before the front comprehension because the `Schedule.slack` property rebuilds the map per access (a frozen `Struct` carries no `cached_property` slot), so reading it per front would re-pay the projection. `Block.sort_with(key)` verifies as a key-selector ascending sort over a `tuple[float, str]` composite against `.api/expression.md` sequence-ops; the min-slack-first (MinSlack) list-scheduling rule is the standard bounded-resource heuristic, slack monotone in criticality, and the content-hex tail (a unique, process-stable total order over the content-addressed keys) is why the reading-map `lexicographical_topological_sort` intent lands on the front tie-break and the graph-build order rather than the CPM WALK — the CPM `finish`/`latest` memo is invariant to which valid topological order the passes walk, so `critical_path`/`span`/`slack`/`critical` and the `facts` they project are already run-stable, and it is the front DRIVE order and the node-index assignment, not the walk, that the determinism fix must pin.
- [TOPOLOGY_EVIDENCE] [RESOLVED]: `PipelinePlan` carries its own topology, schedule, AND coverage evidence rather than a bare front list — `keys`/`requires`/`adjacency` the produced set, the producer-graph adjacency, and its in-set narrowing; `dangling` the parent keys NO node mints (`frozenset(parent for node in scoped for parent in node.parents) - keys`, the silent external-node trap `rx.PyDiGraph` would treat as an unknown index the edge loop skips); `collided` the keys minted by MORE than one node (`Counter`-derived, the last-wins drop `Map.of_seq` would otherwise absorb so a downstream consumer of the silently-dropped producer plans a lie); `untargeted` the requested `targets` deliverables the producer set never mints (`self.targets - full keys`, the target-axis counterpart of `dangling`); `elided` the `keyed` nodes the threaded `cache_seed` already carries; `cyclic` the `digraph_find_cycle` `EdgeList` endpoints; the derived `profile`/`cardinality`/`depth`/`width`/`contention`/`live` front shape and the `roots`/`leaves` source/sink sets over `adjacency`; and the `schedule`-projected `critical_path`/`span`/`makespan`. The `dangling`, `collided`, and `untargeted` sets are the real correctness fixes — a typo'd `FigureRef.asset_key` parent produces a quietly-truncated graph with no error, two producers claiming one content key drop one without a signal, and a partial rebuild scoped to a non-existent deliverable plans an empty/partial graph silently — all three surfaced as actionable plan evidence. The derived `PipelinePlan.severed: Option[PlanFault]` is the one cause-carrying coverage verdict the composition root reads before the drive — `cyclic` first, `untargeted` next, `collided` then, `dangling` last, `Nothing` for a sound plan — so the root recovers on WHICH cause rather than a `bool` collapsing all to one undifferentiated truth. The `frozenset` evidence is the immutable order-insensitive owner; the `roots`/`leaves` folds verify against the `parents` reachability the edges encode.
- [COLLISION_COVERAGE] [RESOLVED]: a content-keyed producer graph where two `ArtifactWork` nodes mint the same `ContentKey` is a real coverage failure the prior page silently absorbed — `Map.of_seq((node.key, node) for node in scoped)` is last-wins, so the second producer overwrites the first in `by_key`/`requires`/`weights`, and `_digraph`'s `index` dict likewise resolves the collided key to one graph node, so the plan schedules and drives only one, the other's dependents rewired to the survivor or left `dangling`. `_compute` captures `collided = frozenset(key for key, count in Counter(node.key for node in scoped).items() if count > 1)` and `severed` carries it as `PlanFault(collided=...)` between `untargeted` and `dangling`, so a double-claimed artifact identity (a genuine duplicate to dedupe, or a key-derivation bug the content-addressing should have separated) surfaces as a typed cause carrying its offending keys rather than a quietly-dropped producer. `collections.Counter` is the stdlib multiset for the count; the `collided` set rides the owner-derived `PipelinePlan.facts` projection as `len(self.collided)` beside its `dangling`/`cyclic` siblings.
- [TARGET_SCOPE] [RESOLVED]: `ArtifactPipeline.of(..., targets=frozenset())` retains the requested targets unscoped, and `_compute` scopes the plan to the requested-deliverable ancestor closure through `_scoped` — when `targets` is non-empty, `_scoped` builds the full producer `rx.PyDiGraph` and unions each in-set target's `rx.ancestors(graph, seed)` index set with the target itself, projecting back through `get_node_data` and keeping exactly the targets and their transitive ancestors, so a partial rebuild ("rebuild the cover page and its bound figures") plans precisely that sub-graph rather than the entire producer set. `rx.ancestors` is the graph-native replacement for the deleted `tailrec` `_reachable` fixpoint the doctrine forbids re-implementing; the closure includes every ancestor of every kept node, so pruning never spuriously fires `dangling`; the discriminant is the `targets` value itself (empty = the whole graph), never a `mode`/`partial` flag. A requested target the producer set never mints is the target-axis counterpart of `dangling`: `_scoped` would otherwise drop it into an empty/partial plan silently, so `ArtifactPipeline` retains the requested `targets` and `_compute` computes `untargeted = self.targets - frozenset(node.key for node in self.nodes)` against the FULL set, surfacing it as a `PlanFault(untargeted=...)` the `severed` verdict refuses ahead of `collided`/`dangling` — the same silent-truncation defect the page refuses on the parent and identity axes, closed on the request axis. `rx.ancestors(graph, node) -> set[int]` and `Block.filter`/`Map.try_find` verify against the installed `.pyi` and `.api/expression.md`.
- [GRAPH_OFFLOAD] [RESOLVED]: the synchronous `rustworkx` kernel (`_digraph` build + `topological_generations` + `Schedule.of` + `digraph_find_cycle` + `_scoped` ancestors) is GIL-releasing native Rust CPU work, so `_planned` crosses it onto `anyio.to_thread.run_sync(self._compute, limiter=_PLAN_LANES)` rather than running it inline on the event loop where a large producer-set sweep would starve the scheduler — the concurrency `[OFFLOAD_LANE]` law and the `.api/rustworkx.md` `[STACKING]` law both name `to_thread.run_sync` (not `to_interpreter`/`to_process`) as the arm for a GIL-releasing native call sharing the address space, bounded by an explicit `CapacityLimiter(os.process_cpu_count() or 1)` — the runtime worker-sizing owner — never the per-loop 40-token default. `_compute` is pure synchronous graph + `expression`-fold + object-construction work safe in a worker thread (it builds `Admit` thunks, invoking no coroutine), and `async_boundary` wraps `_planned` so a `rustworkx` raise or a `BrokenWorkerProcess` converts to a `RuntimeRail` fault; `rx` is `lazy import`-deferred so its native load reifies under the import lock on the worker's first `rx.*` access, and `_emitted` runs post-offload on the event loop. `anyio.to_thread.run_sync`/`CapacityLimiter` verify against `.api/anyio.md`; `os.process_cpu_count` against the `runtime.md` worker-sizing row.
- [ADMISSION_LOWERING] [RESOLVED]: each `ArtifactWork` node lowers through its OWN `lowered` admission `match` into the `Admit` case its closed `Admission` union selects, covering ALL THREE cases the `runtime/execution#LANE` `@tagged_union(frozen=True) class Admit[T]` declares (`bare: Work[T]`, `keyed: tuple[ContentKey, Work[T]]`, `retried: tuple[RetryClass, Work[T]]`, with `Work[T] = Callable[[], Awaitable[RuntimeRail[T]]]`) rather than a hardcoded single `keyed` lowering — a `keyed` node lowers to `Admit(keyed=(self.key, self.work))`, a `bare` node (a forced-live non-content-addressable one-shot) to `Admit(bare=self.work)`, and a `retried` node (a transient-prone offload producer) to `Admit(retried=(cls, self.work))` where `cls` is the `RetryClass` the `retried` `Admission` case MANDATES. The `Admission` selector is itself a closed `@tagged_union` (`keyed`/`bare` empty-payload cases, `retried` carrying its mandatory `RetryClass`), so a transient node states its class IN its `retried` case or is not retried — no `RetryClass.OCCT` default papers over a `Nothing` the case forbids by construction. A `keyed`-plus-internal-offload-retry producer instead keeps `keyed` and rides its `LanePolicy.offload(kernel, retry=...)` inside its own `work` coroutine. The `Admit.bare`/`keyed`/`retried` cases, the `RetryClass` import, and the `LanePolicy`/`Work` spellings verify against the `runtime/execution#LANE` `lanes.md` owner; the plan re-declares neither `Admit` nor `Work`, imports no `DrainReceipt`, and reaches only `Admit`/`LanePolicy`/`Work` from `rasm.runtime.lanes`.
- [CONTENT_KEY_NODES] [RESOLVED]: each `ArtifactWork.key` is the `evidence/identity#IDENTITY` `ContentKey` the producing sub-domain's `_emit` already minted through `ContentIdentity.of(...)`, so the plan consumes the pre-minted key and re-mints nothing — the CURRENT full producer set (refreshed off `core/receipt#RECEIPT`'s case union and the folder `ARCHITECTURE.md` domain map, dropping the stale arm list): the document arms `document/emit#DOCUMENT`/`document/report#REPORT`/`document/egress#FINISH`/`document/lens#LENS`, the `document/tagged#ACCESS` structural author (its `Egress` case beside `document/egress`), the `typography/font#FONT`/`shape#SHAPE`/`layout#LAYOUT` arms minting the `Document` case for a standalone axis catalog / positioned glyph run / line-broken stream, the visual arms `visualization/chart/export#CHART`/`visualization/table#TABLE`/`visualization/diagram/layout#LAYOUT`, `scene#SCENE`/`scene/stage#STAGE`, `package#BUNDLE`, the `Preview`-minting `graphic/marks#MARK`+`graphic/raster/measure#MEASURE`+`graphic/color/managed#COLOR_MANAGED` arms, the exchange arms `exchange/credential#PROVENANCE`/`exchange/metadata#METADATA`/`exchange/conformance#CONFORMANCE`, the AEC drawing plane `drawing/dimension#DIMENSION`/`annotate`/`symbol`/`detail`/`schedule#SCHEDULE`, the `composition/sheet#SHEET` titled-sheet producer (whose `parents` are its placed figures' content keys), the `export/dxf#DXF` CAD arm, the `export/layered#LAYERED`/`export/indesign#INDESIGN` editable-export arms (each a per-instance `emit -> RuntimeRail[ArtifactReceipt]` wrapping its render, the parallel module-level `exported`/`produced` batch drivers collapsed into the pipeline), the `specification/section` spec-into-`document/model` authoring and the `delivery/register`+`delivery/transmittal` assembly, and the `media/*` pages — every one returning a content-keyed `RuntimeRail`, spanning every producing case of the `core/receipt#RECEIPT` union (`Pdf`/`Office`/`Report`/`Document`/`Chart`/`Scene`/`Table`/`Preview`/`Bundle`/`Introspection`/`Egress`/`Verdict`/`Credential`/`Media`/`Diagram`/`Metadata`/`Drawing`/`Schedule`/`Spec`/`Cad`/`Register`/`Transmittal`). Each dependency edge is a parent `ContentKey`, so a report node's `parents` are exactly its bound `FigureRef.asset_key` content keys and the `rx.PyDiGraph` orders the graph by content-key reachability. The `ContentKey` identity satisfies the `rustworkx` hashable-node requirement because `ContentKey` is a `Struct(frozen=True, gc=False)` per the `evidence/identity#IDENTITY` owner; the per-producer `ContentIdentity.of(...)` derivations verify against each producer page's `_emit`.
- [RECEIPT_CONSUMER] [RESOLVED]: the plan contributes no `ArtifactReceipt` CASE and is the third reuse-fabric CONSUMER of the one `core/receipt#RECEIPT` `contribute` fold, named as such in `core/receipt#SIGNALS [PLAN_FABRIC]` beside the `runtime/execution` lane `(ContentKey, Work)` elision and the `runtime/observability` `MeterProvider` signal stream — each producer's `Work[ArtifactReceipt]` coroutine resolves to one `ArtifactReceipt` whose `contribute` fold the runtime lane elision reads as the keyed hit/miss evidence, so the plan reads the SAME fold rather than a parallel plan-receipt rail. The plan imports no producer module because each `Work` thunk carries its own producer call, so no producer→plan→producer cycle forms. The consume-side obligation `core/receipt#SIGNALS [PLAN_FABRIC]` names — the planner minting its OWN `Receipt.of("artifacts", ("planned", "pipeline.plan", facts))` over the pipeline-topology facts — is realized HERE: `ArtifactPipeline.plan` threads the module-level `_emitted` tap over the resolved-plan rail so a sound plan fires `Signals.emit(Receipt.of("artifacts", ("planned", "pipeline.plan", plan.facts)), OPEN)` exactly once over the owner-derived `PipelinePlan.facts` projection (the plan derives its own observability view exactly as `core/receipt#RECEIPT` `ArtifactReceipt._facts` does, not an `ArtifactPipeline`-side fact map), the `runtime/observability#RECEIPT` `Receipt.of` `(Phase, subject, facts)` triple minting the `fact` case the `PHASE_LEVEL` table logs at `debug`. That `facts` projection carries the native int/float coverage AND CPM shape (`cardinality`/`live`/`depth`/`width`/`contention`/`critical_path`/`makespan`/`span`/`critical`/`elided`/`dangling`/`cyclic`/`collided`/`roots`/`leaves`) the receipts `Encoder(enc_hook=repr, order="deterministic")` renderer serializes without a `str()` coerce, plus the `severed.map(lambda fault: fault.tag).default_value("ok")` cause, so the debug line names both the failure cause and its magnitude. `_emitted` fires on the resolved-plan arm so a coverage-severed plan still rides the rail. The `Receipt.of`/`Signals.emit`/`OPEN` (the runtime-owned keep-all `Redaction` threaded rather than a re-minted `Redaction(classified=Map.empty())`) spellings verify against the `runtime/observability#RECEIPT` owner, the `Phase` `planned` member and `PHASE_LEVEL` `debug` row against its `type Phase = Literal["admitted", "planned", "emitted"]`, and the `[PLAN_FABRIC]` consumer naming against `core/receipt#RECEIPT`'s `[03]-[SIGNALS]` block.
- [NO_LANE_MECHANICS] [RESOLVED]: the plan owns the producer-graph-to-`Admit` PROJECTION, the front ORDER, the CPM SCHEDULE, the min-slack front lowering, its topology/coverage evidence, and the offload of its OWN graph kernel, and owns none of the lane DRAIN mechanics — no drain `CapacityLimiter` (the `runtime/execution#LANE` `LanePolicy.limiter` `functools.cache`-memoised `_limiter` owns it), no `move_on_after` scope, no `guard(cls)` binding, no `DrainReceipt` construction, no DRAIN-time `cache.try_find` probe to DECIDE a hit, and no producer execution. The plan's own `Map.try_find` is the node-lookup over its OWN `by_key` index and the `cache_seed.keys()` read derives the `elided` evidence set — a pure projection over the threaded cache, NOT a drain-time decision the lane owns. The `_PLAN_LANES` `CapacityLimiter` is the plan's OWN offload throttle for the synchronous `rustworkx` kernel (a `to_thread` bound, distinct from the lane's drain limiter), never a second drain scheduler. The `LanePolicy` is ONE policy value the plan carries and the lane drives under (the `capacity` + `Option[float]` deadline), never split into loose scalars; `lane.capacity` is read only by `contention` and `makespan` as a saturation/area-bound divisor. The plan's external libraries are `rustworkx` (the DAG algebra) and `anyio` (the offload) plus the stdlib `collections.Counter`/`os`, the substrate `expression`/`msgspec`, and the runtime/receipt owners it composes — no `frozendict` table, `RetryClass` a sibling `reliability/resilience#RESILIENCE` runtime owner, not a PyPI dependency.
