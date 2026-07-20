# [PY_DATA_IDEAS]

Forward pool of higher-order concepts for `data`, grounded in the host-free interchange role. Each idea is a card — slug leader with the capability, what it unlocks, and the gap or technique it draws on — and spawns one or more tasks in `TASKLOG.md`. `[1]-[OPEN]` holds live concepts; `[2]-[CLOSED]` records dispositions so an idea is never re-litigated.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis>.
- Capability: <higher-order concept, invariant, or owner capability>.
- Shape: <what the idea becomes as a system, product, owner, or feature set(s)>.
- Unlocks: <new branch, package, workflow, proof, user, or agent capability made possible>.
- Anchors: <owners, seams, packages, doctrines, or techniques that make the idea plausible>.
- Tension: <only when an unresolved constraint, boundary, bet, or dependency shapes the idea>.
- Ripple: <origin/counterpart card this entry pairs with across folders, as `pkg` `[SLUG]`; present only on a cross-folder ripple counterpart card>.
-->

[ENGINE_PROFILE_PARITY]-[QUEUED]: engine-native profiling depth beyond the DuckDB arm.
- Capability: polars, daft, and datafusion arms harvest engine-native profile evidence into the shared `QueryReceipt.profile` band beside the DuckDB `EngineProfile`.
- Shape: per-engine harvest rows — `polars` `LazyFrame.profile()` node timings, `datafusion` `EXPLAIN ANALYZE` operator metrics, daft runner statistics — decoded into the one profile band; the instrument projection keeps firing at the receipt's own `contribute`.
- Unlocks: engine-uniform query telemetry and cross-engine comparison over one `QuerySpec` without a DuckDB-only depth asymmetry.
- Anchors: `tabular/columnar#SCAN` `EngineProfile`/`QueryReceipt.profile`; `tabular/query#QUERY` `_provenance` fold; the `polars`/`daft`/`datafusion` catalogues.
- Tension: each engine spells its own profile schema — the band stays one decoded shape with per-engine adapters, never three parallel receipt fields.

[QUERY_BENCH_LANE]-[QUEUED]: repeatable engine benchmarks on the query plane.
- Capability: one `QuerySpec` benchmarked across engines through the runtime bench runner — warmup-disciplined latency and throughput receipts keyed by the engine discriminant.
- Shape: a bench lane wrapping `QueryEngine.run` under runtime `Bench.run` subjects; throughput mode for scan-bound specs, latency for point queries; receipts beside `QueryReceipt` with projection through the standing `domain="bench"` rows.
- Unlocks: engine A/B evidence over identical specs and regression tracking across engine upgrades.
- Anchors: runtime `observability/profiles#BENCH` `Bench.run`/`BenchmarkReceipt`; `tabular/query#QUERY` `QuerySpec` axis; the bench growth law absorbing new subjects with zero runtime edits.
- Tension: a bench lane re-executes its spec — mutation specs never ride it, and a process-terminal bench run rides the runtime job envelope.

[LAYER_TOPOLOGY_GRAPH_FACTS]-[QUEUED]: Decoded `LayerTopologyFact` rows fold into a containment graph the graph plane analyzes for host organization.
- Capability: Wire-carried layer and relation keys decode into a `GraphPayload` whose nodes are layer identities and whose edges are layer-path nesting and membership, so topology analysis — containment ancestry, nesting depth, membership closure — answers layer organization over the decoded graph with no host handle; per-viewport overrides ride the decoded rows as detached facts.
- Shape: A boundary decoder folds the detached fact rows into the `graph/graph` plane's node-link source; `GraphPayload.analyze` runs the containment and nesting queries on the one `rustworkx` kernel keyed by the stable `NodeId` index, and the node-keyed `GraphResult.frame` left-joins layer organization onto the `tabular/columnar` scan plane by `node`.
- Unlocks: Host-organized element queries over the graph plane, layer-scoped dataset slicing, and one decoded organizational axis every interchange consumer reads by name.
- Anchors: `graph/graph.md` `GraphPayload`/`analyze`/`NodeId`/`GraphResult.frame`; `rasm.runtime.identity` `ContentIdentity`/`ContentKey` for the stable wire identity; `README.md` host-free interchange role meeting C# only at the content-identity wire.
- Tension: Wire schema and codec mint in C#; this plane decodes and never re-mints, and the containment graph carries only detached fact rows, never a host layer handle.
- Ripple: `libs/.planning` `[LAYER_TOPOLOGY_GRAPH_FACTS]`.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

[EMBEDDED_ENGINE_OBSERVABILITY]-[COMPLETE]: embedded engines carry no scrape surface, so the profiled session bracket became the DuckDB observability owner — harvest folded onto the one `QueryReceipt` stream, instruments projected through the runtime metric spine, DBAPI spans owned by the root-composed instrumentor train.
