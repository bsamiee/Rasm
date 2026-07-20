# [PY_DATA_TASKLOG]

Open and closed work for `data`, distilled from `IDEAS.md`. Each task card leads with a status marker — `[QUEUED]`, `[ACTIVE]`, or `[BLOCKED]` open; `[COMPLETE]` or `[DROPPED]` closed — and carries the capability or file to build, the external packages to integrate, the integration points and boundaries, and the key considerations. `[1]-[OPEN]` holds live work; `[2]-[CLOSED]` records finished or dropped tasks.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with. `Atomic` flags a minor-scope task so a later session sizes its turn correctly and does not overscope a batch of small items.

## [01]-[OPEN]

<!-- source-only: open task card template:
[ID]-[STATUS]: <ambitious concise thesis>.
- Capability: <higher-order concept, invariant, or owner capability>.
- Shape: <what the idea becomes as a system, product, owner, or feature set(s)>.
- Unlocks: <new branch, package, workflow, proof, user, or agent capability made possible>.
- Anchors: <owners, seams, packages, doctrines, or techniques that make the idea plausible>.
- Tension: <only when an unresolved constraint, boundary, bet, or dependency shapes the idea>.
- Ripple: <origin/counterpart card this entry pairs with across folders, as `pkg` `[SLUG]`; present only on a cross-folder ripple counterpart card>.
- Atomic: <present only on a minor-scope task; one short phrase naming the small unit so a later session does not overscope its turn>.
-->

[ENGINE_PROFILE_ADAPTERS]-[QUEUED]: land the per-engine profile harvest rows on the query plane.
- Capability: polars/datafusion/daft profile evidence decoded into the `QueryReceipt.profile` band beside the DuckDB harvest.
- Anchors: `tabular/columnar#SCAN` `EngineProfile`; `tabular/query#QUERY` arms; the `polars`/`datafusion`/`daft` catalogues.
- Tension: one decoded band shape, per-engine adapters at the arm — never parallel receipt fields.

[QUERY_BENCH_HARNESS]-[QUEUED]: land the query bench lane over `QueryEngine.run`.
- Capability: runtime `Bench.run` subjects per engine discriminant with latency and throughput rows over one repeated `QuerySpec`.
- Anchors: runtime `observability/profiles#BENCH`; `tabular/query#QUERY` `QuerySpec` axis.
- Tension: mutation specs excluded; process-terminal runs ride the runtime job envelope.
- Atomic: bench subjects and card fields, zero new instrument rows.

[LAYER_TOPOLOGY_DECODER]-[QUEUED]: Decoded `LayerTopologyFact` rows land as the graph plane's containment node-link source.
- Capability: one boundary decoder folds wire-carried layer and relation keys into the `graph/graph.md` `GraphPayload` node-link source — nodes the layer identities, edges the nesting and membership rows.
- Shape: a decoder fence on `graph/graph.md` beside `GraphPayload.of`; containment queries ride the existing `analyze` axis, and `GraphResult.frame` left-joins layer organization by `node`.
- Anchors: `graph/graph.md` `GraphPayload`/`NodeId`/`GraphResult.frame`; runtime `ContentIdentity` for the stable wire identity.
- Tension: wire schema and codec mint in C#; the decoder lands after the wire freezes and carries only detached fact rows, never a host layer handle.
- Ripple: `libs/.planning` `[LAYER_TOPOLOGY_GRAPH_FACTS]`.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

[ENGINE_HARVEST]-[COMPLETE]: landed in `.planning/tabular/columnar.md` — `DuckDbSession.profiled()` JSON-profiling bracket, `EngineProfile` decode over the engine's lowercase profile keys, the `QueryReceipt.profile` band with the `domain="query"` `Metrics.record` projection at `contribute`, and the query-page rows routing DBAPI span coverage to the runtime composition-root instrumentor train; README substrate registry gained the `opentelemetry-api` row.

[OBSERVABILITY_DEPTH]-[COMPLETE]: every measured plane instruments. `impact`/`graph`/`profile` gained solve/kernel/interrogate spans and `domain="impact"`/`"graph"`/`"quality"` projections; `egress` projects `rasm.egress.byte_volume`; `materialize` records `rasm.materialize.rows`; `lakehouse` spans commits; the gridded plane carries per-leg spans and the `PlanReceipt` `to_builtins` projection; remote legs open `SpanKind.CLIENT`. ERROR marking stays the runtime `boundary` fence's, tenant-on-span the telemetry install's; `add_link` is ruled out — nesting and content keys already correlate.
