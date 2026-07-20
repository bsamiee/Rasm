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
- Capability: polars/datafusion/daft profile evidence decoded into the `QueryReceipt.profile` band beside the DuckDB harvest; the daft adapter controls partition grain through `DataFrame.into_partitions` so runner statistics arrive at a chosen fan-out.
- Anchors: `tabular/columnar#SCAN` `EngineProfile`; `tabular/query#QUERY` arms; the `polars`/`datafusion`/`daft` catalogues; `.api/daft.md` `DataFrame.into_partitions`.
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

[HOOK_POINT_ROWS]-[QUEUED]: register the data-plane hook points on the runtime registry, one row per mutation edge.
- Capability: `rasm.data.lakehouse.commit` (veto), `rasm.data.egress.put`/`rasm.data.egress.delete` (veto), `rasm.data.materialize.refresh` (replay), `rasm.data.contract.verdict` (observe) — `HookPoint` rows registered at composition on `.planning/tabular/lakehouse.md`, `.planning/tabular/egress.md`, `.planning/tabular/materialize.md`, and `.planning/tabular/contract.md`, payloads the pages' existing receipt structs.
- Anchors: runtime `observability/hooks#HOOKS` `HookPoint`/`Hooks.register`/`fire`/`Modality`/`tap_receipts`/`tap_metrics`; idea `[DATA_HOOK_POINTS]`.
- Tension: fires ride the emitter's active span — no hook opens a span, no page wires a subscriber.

[COST_FACT_HARVEST]-[QUEUED]: land the cost-ledger page harvesting receipt families into the priced frame.
- Capability: `libs/python/data/.planning/tabular/cost.md` — `CostFact` decode rows off `QueryReceipt`, egress byte volume, materialize row counts, and gridded `PlanReceipt`; the `CostLedger` group-fold by content key, tenant, and domain; the rate-policy parameter and the priced Arrow egress.
- Anchors: `.planning/tabular/columnar.md` `QueryReceipt`; `.planning/tabular/egress.md`; `.planning/gridded/store.md` `PlanReceipt`; idea `[DATASET_COST_LEDGER]`.
- Tension: a projection over receipts, never a second metering pipeline; unbounded partition ids stay receipt-only.
- Ripple: `libs/.planning` `[COST_ATTRIBUTION_BAGGAGE]`.

[ADBC_DRIVER_ROWS]-[QUEUED]: land the Postgres, SQLite, and Snowflake driver rows on the query remote sub-axis.
- Capability: one row per driver on `.planning/tabular/query.md` `RemoteOp` dispatch — driver entrypoint, `db_kwargs` knob family, transient-retry classification — under the existing DBAPI bracket and receipt fold.
- Anchors: `.api/adbc-driver-manager.md` `dbapi.connect`; `guarded(RetryClass.REMOTE_DB)`; idea `[ADBC_DRIVER_SET]`.
- Atomic: three dispatch rows and their knob tables, zero new query surface.

[SUBSTRAIT_GATE_FENCE]-[QUEUED]: land the typed plan admission fence on the federated arms.
- Capability: parse-validate-census over inbound plan bytes on `.planning/tabular/query.md` `Federated`/`Flight` arms — typed refusal on malformed or version-skewed plans, relation-op census onto the receipt, original bytes handed onward untouched.
- Anchors: `substrait` package plan model; `Serde.deserialize_bytes` hand-off; `ContentIdentity.of("query.plan", wire)`; idea `[SUBSTRAIT_PLAN_GATE]`.
- Tension: never re-serialize — the identity law keys the original bytes.

[GEOARROW_IO_ROWS]-[QUEUED]: land the GeoArrow-native reader and writer rows on the vector format axis.
- Capability: FlatGeobuf, GeoParquet, shapefile, and GeoJSON rows over `geoarrow-rust-io` on `.planning/spatial/geospatial.md`, `geoarrow-rust-core` arrays as the typed memory model, `geoarrow-pyarrow` as the shapely bridge; pyogrio retained as the GDAL long-tail row.
- Anchors: `VectorOp` axis and native-GeoArrow egress; `.api/geoarrow-rust-compute.md`; idea `[GEOARROW_NATIVE_SET]`.
- Tension: a fast path beside the GDAL owner, never a second format owner.

[GEOARROW_COMPUTE_SWEEP]-[QUEUED]: sweep the unexploited compute kernels onto the claims plane.
- Capability: `frechet_distance`, `line_locate_point`, geodesic length and perimeter, `total_bounds` as vector-op rows on `.planning/spatial/geospatial.md`, operating on the GeoArrow arrays the plane already carries.
- Anchors: `.api/geoarrow-rust-compute.md` member table; idea `[GEOARROW_NATIVE_SET]`.
- Atomic: kernel rows on the existing op axis, no new owner.

[ARRO3_HOP_REPLACEMENTS]-[QUEUED]: land the arro3 compute and IO rows where pyarrow re-import is the only reason pyarrow appears.
- Capability: `arro3-compute` kernels on the `.planning/tabular/materialize.md` CDF hop replacing the `pa.table(...)` re-import where sort and filter are the whole need; `arro3-io` IPC rows on `.planning/tabular/interop.md` slim carrier legs.
- Anchors: materialize PyCapsule hop comment; `ArrowCStream`; idea `[ARRO3_SLIM_SET]`.
- Atomic: hop substitutions under a ruled arro3-versus-pyarrow split, no surface change.

[ZONE_CUBE_OWNER]-[QUEUED]: land the vector-cube page bridging field cubes and vector claims.
- Capability: `libs/python/data/.planning/spatial/cube.md` — the `xvec` geometry-indexed dimension owner over `FieldDataset` cubes, geometry-predicate selection through the claims plane's CRS law, zone-variable join onto `VectorGeoClaim` frames, content-keyed egress on the field receipt family.
- Anchors: `.planning/gridded/field.md` `FieldDataset`; `.planning/spatial/geospatial.md` `VectorGeoClaim.reproject`; branch `xarray`; idea `[VECTOR_DATA_CUBES]`.
- Tension: composes the CF owner, never a second labelled-array store.

[CARRIER_COMPRESSION_OPTION]-[QUEUED]: land the transport-band compression codec vocabulary on the carrier IPC egress.
- Capability: a closed codec parameter (`lz4`/`zstd`/`none`) on the `.planning/tabular/interop.md` IPC lowering via `ipc.IpcWriteOptions(compression=)`, threaded to `.planning/tabular/egress.md` put paths; the `arrow_bytes` identity fold pinned uncompressed by law.
- Anchors: `.api/pyarrow.md` `ipc` submodule; branch `.api/lz4.md`; idea `[COMPRESSED_CARRIER_BAND]`.
- Atomic: one parameterized option and its identity-law fence comment.

[STORE_WRITE_GUARDS]-[QUEUED]: land the single-writer guards and re-scope the catalog signing handle.
- Capability: `anyio` `ResourceGuard` rows on the `.planning/tabular/lakehouse.md` commit path and `.planning/gridded/store.md` write paths, guard ownership on the composition-bound policy; the `.planning/spatial/catalog.md` process-wide `planetary_computer` handle re-scoped to a composition-bound scheme row.
- Anchors: branch `.api/anyio.md` `ResourceGuard`; `LAKE_COMMIT` conflict tags; idea `[APP_NEUTRAL_STORE_SCOPES]`.
- Atomic: guard rows and one handle re-scope, no protocol change.

[IMPACT_PAGE_SPLIT]-[QUEUED]: land the four provider-depth impact pages and redistribute the compressed surface.
- Capability: `libs/python/data/.planning/impact/declaration.md` (openepd, epdx, EC3 search stream and offline bundles), `libs/python/data/.planning/impact/inventory.md` (bw2data, bw2io, bw-processing custody), `libs/python/data/.planning/impact/solve.md` (bw2calc `MultiLCA`, bw2analyzer contribution rows, olca-ipc live solve), `libs/python/data/.planning/impact/scenario.md` (premise IAM transforms); `impact/impact.md` keeps the EN 15804 carrier and `ImpactSource` axis.
- Anchors: `.planning/impact/impact.md` Growth staged rows naming exactly this deferred set; the four provider catalog files under `.api/`; idea `[IMPACT_PLANE_BUILDOUT]`.
- Tension: sibling pages feed `ImpactSource` cases — one normalization fold, one matrix owner.

[FRAME_ADMISSION_ROWS]-[QUEUED]: land the geometry frame admission rows on the scan plane.
- Capability: one admission row per geometry frame family on `.planning/tabular/columnar.md` — `GeometrySubject` discriminant, `ContentKey` join keys, `domain="query"` projection at contribute.
- Anchors: geometry frame port on `libs/python/geometry/.planning/graduation.md`; `tabular/columnar#SCAN`; idea `[GEOMETRY_FRAME_ADMISSION]`.
- Ripple: `geometry` `[ANALYTIC_FRAME_EGRESS]`.
- Atomic: admission rows on one existing page.

[CRS_SOURCE_ROW]-[QUEUED]: land the IFC georeference CRS-source row on the reproject prelude.
- Capability: georeference-fact decode — CRS identity, map-conversion transform, true north — as one typed CRS source on `.planning/spatial/geospatial.md` `VectorGeoClaim.reproject`.
- Anchors: `spatial/geospatial.md` CRS law; `.api/pyproj.md`; idea `[IFC_CRS_SOURCE]`.
- Ripple: `geometry` `[IFC_GEOREFERENCE]`.
- Atomic: one CRS-source row on one existing page.

[DRIVER_WRAP_THREADS]-[QUEUED]: name the driver connection factories the composition root threads through the runtime wrap.
- Capability: threading rows for the duckdb session factory and the ADBC `dbapi.connect` legs on `.planning/tabular/query.md`, each naming its factory and the composition-root activation; connectorx exclusion stated at the row.
- Anchors: runtime wrap seam beside `TRAIN`; `.api/adbc-driver-manager.md`; idea `[DBAPI_SPAN_THREADING]`.
- Ripple: `runtime` `[DBAPI_TRAIN_ROW]`.
- Atomic: threading rows on one existing page.

[ENSEMBLE_TREE_OWNER]-[QUEUED]: author the scenario-tree page over the CF field plane.
- Capability: `libs/python/data/.planning/gridded/ensemble.md` — `DataTree` constructor rows, scenario-axis vocabulary, group-wise map/reduce/difference folds, `FieldReceipt` content-keyed egress.
- Anchors: branch `.api/xarray.md` `DataTree`; `gridded/field.md` `FieldDataset`; idea `[SCENARIO_FIELD_TREES]`.

[NETWORK_FLOW_PAGE]-[QUEUED]: land the capacity-network page on the graph plane.
- Capability: `libs/python/data/.planning/graph/network.md` — capacity-annotated network owner over the networkx codec lane, `maximum_flow`/`min_cost_flow`/`network_simplex` rows, flow results lowering through `GraphResult.frame` onto the columnar join, `domain="graph"` projection.
- Anchors: `.planning/graph/graph.md` codec lane; branch `.api/networkx.md` flow family; idea `[GRAPH_NETWORK_ANALYSIS]`.
- Tension: networkx carries the flow family only — rustworkx stays the kernel for everything it spells.

[EPD_WIRE_ROWS]-[QUEUED]: land the registry-source arm and the Assessment record schema rows.
- Capability: provider fetch rows for EC3/Ökobaudat/EPD-Norge folding into `MaterialImpact` with declared unit, module coverage, and expiry columns; the record schema stated once as the `Assessment` seam payload the Materials end decodes.
- Shape: rows on `libs/python/data/.planning/impact/impact.md`; schema co-sign recorded at the seam cluster.
- Anchors: idea `[EPD_RECORD_WIRE]`; `openepd`/`epdx` declaration arms; the content-keyed Arrow-bytes crossing.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

[ENGINE_HARVEST]-[COMPLETE]: landed in `.planning/tabular/columnar.md` — `DuckDbSession.profiled()` JSON-profiling bracket, `EngineProfile` decode over the engine's lowercase profile keys, the `QueryReceipt.profile` band with the `domain="query"` `Metrics.record` projection at `contribute`, and the query-page rows routing DBAPI span coverage to the runtime composition-root instrumentor train; README substrate registry gained the `opentelemetry-api` row.

[OBSERVABILITY_DEPTH]-[COMPLETE]: every measured plane instruments. `impact`/`graph`/`profile` gained solve/kernel/interrogate spans and `domain="impact"`/`"graph"`/`"quality"` projections; `egress` projects `rasm.egress.byte_volume`; `materialize` records `rasm.materialize.rows`; `lakehouse` spans commits; the gridded plane carries per-leg spans and the `PlanReceipt` `to_builtins` projection; remote legs open `SpanKind.CLIENT`. ERROR marking stays the runtime `boundary` fence's, tenant-on-span the telemetry install's; `add_link` is ruled out — nesting and content keys already correlate.
