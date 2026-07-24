# [PY_DATA_TASKLOG]

Open and closed work for `data`, distilled from `IDEAS.md`. Each task card leads with a status marker — `[QUEUED]`, `[ACTIVE]`, or `[BLOCKED]` open; `[COMPLETE]` or `[DROPPED]` closed — and carries the capability or file to build, the external packages to integrate, the integration points and boundaries, and the key considerations. `[1]-[OPEN]` holds live work; `[2]-[CLOSED]` records finished or dropped tasks.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with. `Atomic` flags a minor-scope task so a later session sizes its turn correctly and does not overscope a batch of small items.

## [01]-[OPEN]

<!-- source-only: open task card template:
[ID]-[STATUS]: <ambitious concise thesis — the capability outcome, never the landing motion>.
- Capability: <the higher-order invariant, owner capability, or concept established — altitude only, never a page path, row list, or member spelling>.
- Shape: <where the work lands and at what grain — repo-relative page with section/row, or a new-page path; the concrete surface, so Capability never names it>.
- Unlocks: <the downstream capability at the consumer grain — a task narrows its parent idea's Unlocks to THIS slice as `IDEAS.md [SLUG] — consequence`; a set-completion card states the completeness bar that is its acceptance contract>.
- Anchors: <owners, seams, packages, catalogs, doctrines, and techniques making the work plausible — anchors, never procedures>.
- Arms: <BLOCKED or gated cards only; the exact observable flipping it actionable — catalog row landing, member query evidence, package admitted>.
- Route: <present only on a probe, research, or member-pin card; the ordered verification path run before any fence lands>.
- Tension: <only when an unresolved constraint, boundary, or bet shapes the work — the genuine bet, never the arming condition Arms carries>.
- Ripple: <counterpart — cross-folder `pkg` `[SLUG]`, same-folder prerequisite `[SLUG]`; load-bearing build order prefixes follows/precedes/mirrors>.
- Atomic: <present only on a minor-scope task; names the small unit so a later session sizes its turn>.
Capability, Shape, Unlocks, and Anchors are required on every open card, Atomic included; statuses closed — `ACTIVE|QUEUED|BLOCKED` open, `COMPLETE|DROPPED` closed; IDs are SEMANTIC UPPERCASE_SNAKE slugs carrying meaning — never numeric (`[0007]`-class NNNN IDs are a defect), for cards AND research tokens alike; a hyphenated slug anywhere is a defect; repo-relative paths only. Design pages carry the terminal `[RESEARCH]` section always — `(none)` marks empty, absence is an error. Tasks state landing-grain work decomposing an idea.
-->

[ENGINE_PROFILE_ADAPTERS]-[BLOCKED]: Native DuckDB and Polars profile adapters land beside the settled Daft arm.
- Capability: every engine contributes its native operator, CPU, blocked-time, and byte detail without widening `QueryReceipt.profile`.
- Shape: one native `ProfileHarvest` case and decoder per proven payload behind the existing profiled bracket; `EngineProfile.of` remains the only decoder.
- Unlocks: IDEAS.md [ENGINE_PROFILE_PARITY] — every engine contributes native detail through one decoder.
- Anchors: `.planning/tabular/columnar.md` profile research row; `.planning/tabular/query.md` Daft and DataFusion harvest arms; `.api/duckdb.md` profiling row.
- Arms: the duckdb folder-tier catalog carries the exact return type and payload schema of `get_profiling_information()`.

[COHORT_ARROW_PROJECTION]-[QUEUED]: numeric cohorts serialize to canonical Arrow through one data-owned projection.
- Capability: a parameterized numeric-cohort projection — axes, coordinates, responses — builds the table and rides the one serialization fold, so a sibling emits canonical Arrow bytes without importing pyarrow construction members.
- Shape: one public cohort-to-bytes projection on `libs/python/data/.planning/tabular/interop.md` beside `DoeDataset`, folding through the columnar `arrow_bytes` owner.
- Unlocks: compute's study cohort emit — the blocked research row on the compute study page resolves against this public fold.
- Anchors: `DoeDataset.frame` sweep wire; `arrow_bytes` on `libs/python/data/.planning/tabular/columnar.md`; the compute study research row `[STUDY_FRAME_RENDER]`.
- Atomic: one projection on one existing page.

[LAYER_TOPOLOGY_DECODER]-[QUEUED]: Decoded `LayerTopologyFact` rows land as the graph plane's containment node-link source.
- Capability: one boundary decoder folds wire-carried layer and relation keys into the `graph/graph.md` `GraphPayload` node-link source — nodes the layer identities, edges the nesting and membership rows.
- Shape: a decoder fence on `graph/graph.md` beside `GraphPayload.of`; containment queries ride the existing `analyze` axis, and `GraphResult.frame` left-joins layer organization by `node`.
- Unlocks: IDEAS.md [LAYER_TOPOLOGY_GRAPH_FACTS] — host-organized element queries over the graph plane, layer-scoped dataset slicing, and one decoded organizational axis every interchange consumer reads by name.
- Anchors: `graph/graph.md` `GraphPayload`/`NodeId`/`GraphResult.frame`; runtime `ContentIdentity` for the stable wire identity.
- Tension: wire schema and codec mint in C#; the decoder lands after the wire freezes and carries only detached fact rows, never a host layer handle.
- Ripple: `libs/.planning` `[LAYER_TOPOLOGY_GRAPH_FACTS]`.

[ADBC_DRIVER_ROWS]-[QUEUED]: land the Postgres, SQLite, and Snowflake driver rows on the query remote sub-axis.
- Capability: one row per driver on `.planning/tabular/query.md` `RemoteOp` dispatch — driver entrypoint, `db_kwargs` knob family, transient-retry classification — under the existing DBAPI bracket and receipt fold.
- Shape: one Postgres, SQLite, and Snowflake driver row on `.planning/tabular/query.md` `RemoteOp` dispatch — driver entrypoint, `db_kwargs` knob family, transient-retry class — under the existing DBAPI bracket and receipt fold.
- Unlocks: IDEAS.md [ADBC_DRIVER_SET] — local-file and warehouse federation with zero new query surface, and the sqlite storage arm the branch instrumentor train already covers.
- Anchors: `.api/adbc-driver-manager.md` `dbapi.connect`; `guarded(RetryClass.REMOTE_DB)`; idea `[ADBC_DRIVER_SET]`.
- Atomic: three dispatch rows and their knob tables, zero new query surface.

[SUBSTRAIT_GATE_FENCE]-[QUEUED]: land the typed plan admission fence on the federated arms.
- Capability: parse-validate-census over inbound plan bytes on `.planning/tabular/query.md` `Federated`/`Flight` arms — typed refusal on malformed or version-skewed plans, relation-op census onto the receipt, original bytes handed onward untouched.
- Shape: a `substrait` parse-validate-census fence on the `Federated` and `Flight` arms of `.planning/tabular/query.md` — typed refusal on malformed or version-skewed plans, relation-op census onto the receipt, original bytes handed to `Serde.deserialize_bytes` untouched.
- Unlocks: IDEAS.md [SUBSTRAIT_PLAN_GATE] — fail-fast federation over the C# wire and plan-structure evidence on the one `QueryReceipt` stream.
- Anchors: `substrait` package plan model; `Serde.deserialize_bytes` hand-off; `ContentIdentity.of("query.plan", wire)`; idea `[SUBSTRAIT_PLAN_GATE]`.
- Tension: never re-serialize — the identity law keys the original bytes.

[GEOARROW_IO_ROWS]-[QUEUED]: land the GeoArrow-native reader and writer rows on the vector format axis.
- Capability: FlatGeobuf, GeoParquet, shapefile, and GeoJSON rows over `geoarrow-rust-io` on `.planning/spatial/geospatial.md`, `geoarrow-rust-core` arrays as the typed memory model, `geoarrow-pyarrow` as the shapely bridge; pyogrio retained as the GDAL long-tail row.
- Shape: FlatGeobuf, GeoParquet, shapefile, and GeoJSON reader/writer rows over `geoarrow-rust-io` on the `VectorOp` axis of `.planning/spatial/geospatial.md`, `geoarrow-rust-core` arrays the typed memory model and `geoarrow-pyarrow` the shapely bridge, pyogrio retained as the GDAL long-tail row.
- Unlocks: IDEAS.md [GEOARROW_NATIVE_SET] — GDAL-free vector IO on the hot paths, a fast path beside the GDAL owner rather than a second format owner.
- Anchors: `VectorOp` axis and native-GeoArrow egress; `.api/geoarrow-rust-compute.md`; idea `[GEOARROW_NATIVE_SET]`.
- Tension: a fast path beside the GDAL owner, never a second format owner.

[GEOARROW_COMPUTE_SWEEP]-[QUEUED]: sweep the unexploited compute kernels onto the claims plane.
- Capability: `frechet_distance`, `line_locate_point`, geodesic length and perimeter, `total_bounds` as vector-op rows on `.planning/spatial/geospatial.md`, operating on the GeoArrow arrays the plane already carries.
- Shape: `frechet_distance`, `line_locate_point`, geodesic length and perimeter, and `total_bounds` as `VectorOp` rows on `.planning/spatial/geospatial.md`, operating on the GeoArrow arrays the claims plane already carries, no new owner.
- Unlocks: IDEAS.md [GEOARROW_NATIVE_SET] — trajectory analytics over point-sequence datasets and the compute kernel set exploited to depth beside the native IO rows.
- Anchors: `.api/geoarrow-rust-compute.md` member table; idea `[GEOARROW_NATIVE_SET]`.
- Atomic: kernel rows on the existing op axis, no new owner.

[ARRO3_HOP_REPLACEMENTS]-[QUEUED]: land the arro3 compute and IO rows where pyarrow re-import is the only reason pyarrow appears.
- Capability: `arro3-compute` kernels on the `.planning/tabular/materialize.md` CDF hop replacing the `pa.table(...)` re-import where sort and filter are the whole need; `arro3-io` IPC rows on `.planning/tabular/interop.md` slim carrier legs.
- Shape: `arro3-compute` kernel rows replacing the `pa.table(...)` re-import on the CDF hop of `.planning/tabular/materialize.md` where sort and filter are the whole need, and `arro3-io` IPC rows on the slim carrier legs of `.planning/tabular/interop.md`, under a ruled arro3-versus-pyarrow split.
- Unlocks: IDEAS.md [ARRO3_SLIM_SET] — lighter interchange legs for host-embedded consumers where pyarrow weight matters, and the arro3 family admitted as a set instead of a stray core.
- Anchors: materialize PyCapsule hop comment; `ArrowCStream`; idea `[ARRO3_SLIM_SET]`.
- Atomic: hop substitutions under a ruled arro3-versus-pyarrow split, no surface change.

[ZONE_CUBE_OWNER]-[QUEUED]: land the vector-cube page bridging field cubes and vector claims.
- Capability: `libs/python/data/.planning/spatial/cube.md` — the `xvec` geometry-indexed dimension owner over `FieldDataset` cubes, geometry-predicate selection through the claims plane's CRS law, zone-variable join onto `VectorGeoClaim` frames, content-keyed egress on the field receipt family.
- Shape: a new page `.planning/spatial/cube.md` owning the `xvec` geometry-indexed dimension over `FieldDataset` cubes — geometry-predicate selection through the claims-plane CRS law, zone-variable join onto `VectorGeoClaim` frames, content-keyed egress on the field receipt family — composing the CF owner, never a second labelled-array store.
- Unlocks: IDEAS.md [VECTOR_DATA_CUBES] — simulation-result interchange keyed by building geometry, the missing bridge between the gridded and spatial planes.
- Anchors: `.planning/gridded/field.md` `FieldDataset`; `.planning/spatial/geospatial.md` `VectorGeoClaim.reproject`; branch `xarray`; idea `[VECTOR_DATA_CUBES]`.
- Tension: composes the CF owner, never a second labelled-array store.

[CARRIER_COMPRESSION_OPTION]-[QUEUED]: land the transport-band compression codec vocabulary on the carrier IPC egress.
- Capability: a closed codec parameter (`lz4`/`zstd`/`none`) on the `.planning/tabular/interop.md` IPC lowering via `ipc.IpcWriteOptions(compression=)`, threaded to `.planning/tabular/egress.md` put paths; the `arrow_bytes` identity fold pinned uncompressed by law.
- Shape: a closed `lz4`/`zstd`/`none` codec option on the IPC lowering of `.planning/tabular/interop.md` via `ipc.IpcWriteOptions(compression=)`, threaded to the put paths of `.planning/tabular/egress.md`, the `arrow_bytes` identity fold pinned uncompressed by law.
- Unlocks: IDEAS.md [COMPRESSED_CARRIER_BAND] — cheaper egress and cross-runtime transfer for scan-scale point-cloud and tensor frames with every `ContentKey` unchanged.
- Anchors: `.api/pyarrow.md` `ipc` submodule; branch `.api/lz4.md`; idea `[COMPRESSED_CARRIER_BAND]`.
- Atomic: one parameterized option and its identity-law fence comment.

[STORE_WRITE_GUARDS]-[QUEUED]: land the single-writer guards and re-scope the catalog signing handle.
- Capability: `anyio` `ResourceGuard` rows on the `.planning/tabular/lakehouse.md` commit path and `.planning/gridded/store.md` write paths, guard ownership on the composition-bound policy; the `.planning/spatial/catalog.md` process-wide `planetary_computer` handle re-scoped to a composition-bound scheme row.
- Shape: `anyio` `ResourceGuard` rows on the commit path of `.planning/tabular/lakehouse.md` and the write paths of `.planning/gridded/store.md` with guard ownership on the composition-bound policy, and the process-wide `planetary_computer` handle on `.planning/spatial/catalog.md` re-scoped to a composition-bound scheme row.
- Unlocks: IDEAS.md [APP_NEUTRAL_STORE_SCOPES] — the app-neutrality law made structural on the store planes, collision dying at the guard instead of in conflict-retry telemetry.
- Anchors: branch `.api/anyio.md` `ResourceGuard`; `LAKE_COMMIT` conflict tags; idea `[APP_NEUTRAL_STORE_SCOPES]`.
- Atomic: guard rows and one handle re-scope, no protocol change.

[IMPACT_PAGE_SPLIT]-[QUEUED]: land the four provider-depth impact pages and redistribute the compressed surface.
- Capability: `libs/python/data/.planning/impact/declaration.md` (openepd, epdx, EC3 search stream and offline bundles), `libs/python/data/.planning/impact/inventory.md` (bw2data, bw2io, bw-processing custody), `libs/python/data/.planning/impact/solve.md` (bw2calc `MultiLCA`, bw2analyzer contribution rows, olca-ipc live solve), `libs/python/data/.planning/impact/scenario.md` (premise IAM transforms); `impact/impact.md` keeps the EN 15804 carrier and `ImpactSource` axis.
- Shape: four provider-depth pages — `.planning/impact/declaration.md`, `.planning/impact/inventory.md`, `.planning/impact/solve.md`, `.planning/impact/scenario.md` — feeding the `ImpactSource` axis, `.planning/impact/impact.md` keeping the EN 15804 carrier and the one normalization fold.
- Unlocks: IDEAS.md [IMPACT_PLANE_BUILDOUT] — whole-building LCA at package depth, nine admitted packages exploited past the normalization floor and the stub-folder ruling satisfied with real capability.
- Anchors: `.planning/impact/impact.md` Growth staged rows naming exactly this deferred set; the four provider catalog files under `.api/`; idea `[IMPACT_PLANE_BUILDOUT]`.
- Tension: sibling pages feed `ImpactSource` cases — one normalization fold, one matrix owner.

[FRAME_ADMISSION_ROWS]-[QUEUED]: land the geometry frame admission rows on the scan plane.
- Capability: one admission row per geometry frame family on `.planning/tabular/columnar.md` — `GeometrySubject` discriminant, `ContentKey` join keys, `domain="query"` projection at contribute.
- Shape: one admission row per geometry frame family on `.planning/tabular/columnar.md` — `GeometrySubject` discriminant, `ContentKey` join keys, `domain="query"` projection at contribute — riding the existing geometry-to-data seam beside `ResultFrame`.
- Unlocks: IDEAS.md [GEOMETRY_FRAME_ADMISSION] — estate analytics over geometry evidence: cross-model deviation trends, quality rollups, and section-property queries with zero bespoke receipt parsing.
- Anchors: geometry frame port on `libs/python/geometry/.planning/graduation.md`; `tabular/columnar#SCAN`; idea `[GEOMETRY_FRAME_ADMISSION]`.
- Ripple: `geometry` `[ANALYTIC_FRAME_EGRESS]`.
- Atomic: admission rows on one existing page.

[CRS_SOURCE_ROW]-[QUEUED]: land the IFC georeference CRS-source row on the reproject prelude.
- Capability: georeference-fact decode — CRS identity, map-conversion transform, true north — as one typed CRS source on `.planning/spatial/geospatial.md` `VectorGeoClaim.reproject`.
- Shape: one typed CRS source on `VectorGeoClaim.reproject` of `.planning/spatial/geospatial.md` decoding the georeference fact — CRS identity, map-conversion transform, true north — into the reproject prelude beside the existing CRS inputs.
- Unlocks: IDEAS.md [IFC_CRS_SOURCE] — scan-vs-model claims, vector egress, and map-frame joins keyed to the model's own georeference, the python data leg of the estate geospatial root move.
- Anchors: `spatial/geospatial.md` CRS law; `.api/pyproj.md`; idea `[IFC_CRS_SOURCE]`.
- Ripple: `geometry` `[IFC_GEOREFERENCE]`.
- Atomic: one CRS-source row on one existing page.

[ENSEMBLE_TREE_OWNER]-[QUEUED]: author the scenario-tree page over the CF field plane.
- Capability: `libs/python/data/.planning/gridded/ensemble.md` — `DataTree` constructor rows, scenario-axis vocabulary, group-wise map/reduce/difference folds, `FieldReceipt` content-keyed egress.
- Shape: a new page `.planning/gridded/ensemble.md` owning the `DataTree` constructor rows, the scenario-axis vocabulary, group-wise map/reduce/difference folds, and content-keyed `FieldReceipt` egress, composing the CF owner.
- Unlocks: IDEAS.md [SCENARIO_FIELD_TREES] — scenario-set interchange for energy and impact result families, per-scenario deltas queryable beside the vector-cube and claims planes.
- Anchors: branch `.api/xarray.md` `DataTree`; `gridded/field.md` `FieldDataset`; idea `[SCENARIO_FIELD_TREES]`.

[NETWORK_FLOW_PAGE]-[QUEUED]: land the capacity-network page on the graph plane.
- Capability: `libs/python/data/.planning/graph/network.md` — capacity-annotated network owner over the networkx codec lane, `maximum_flow`/`min_cost_flow`/`network_simplex` rows, flow results lowering through `GraphResult.frame` onto the columnar join, `domain="graph"` projection.
- Shape: a new page `.planning/graph/network.md` — capacity-annotated network owner over the networkx codec lane, `maximum_flow`/`min_cost_flow`/`network_simplex` rows lowering through `GraphResult.frame` onto the columnar join under the `domain="graph"` projection.
- Unlocks: IDEAS.md [GRAPH_NETWORK_ANALYSIS] — MEP sizing and circulation evidence over interchange graphs, and the graph stub folder deepened with real capability.
- Anchors: `.planning/graph/graph.md` codec lane; branch `.api/networkx.md` flow family; idea `[GRAPH_NETWORK_ANALYSIS]`.
- Tension: networkx carries the flow family only — rustworkx stays the kernel for everything it spells.

[EPD_WIRE_ROWS]-[QUEUED]: land the registry-source arm and the Assessment record schema rows.
- Capability: provider fetch rows for EC3/Ökobaudat/EPD-Norge folding into `MaterialImpact` with declared unit, module coverage, and expiry columns; the record schema stated once as the `Assessment` seam payload the Materials end decodes.
- Shape: rows on `libs/python/data/.planning/impact/impact.md`; schema co-sign recorded at the seam cluster.
- Unlocks: IDEAS.md [EPD_RECORD_WIRE] — audited whole-life carbon on the C# side from evidence-dated records, one EPD sourcing plane serving every estate consumer.
- Anchors: idea `[EPD_RECORD_WIRE]`; `openepd`/`epdx` declaration arms; the content-keyed Arrow-bytes crossing.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[ENGINE_HARVEST]-[DROPPED]: folded into `[ENGINE_PROFILE_ADAPTERS]` — one arming trigger, one decoder; the DuckDB-only slice held no work the merged adapter card lacks.
[QUERY_BENCH_HARNESS]-[COMPLETE]: `QueryEngine.bench` landed over runtime `Bench.run` with the `_BENCH_MODE` defaults and the mutation-INGEST refusal.
[HOOK_POINT_ROWS]-[COMPLETE]: `.planning/tabular/materialize.md` `register_data_hooks(scope)` folds `DATA_HOOK_POINTS` through `Hooks.register`; lakehouse, egress, materialize, and contract emit through that same scope.
[COST_FACT_HARVEST]-[COMPLETE]: `.planning/tabular/cost.md` `CostFact.of` owns receipt, wire-mapping, and fact normalization; `CostLedger.of` harvests inputs before one priced slot fold.
[DRIVER_WRAP_THREADS]-[COMPLETE]: `dbapi_seams()` rows landed on `tabular/query#QUERY` naming the duckdb/`dbapi.connect`/Flight SQL factories with the ConnectorX exclusion at the comment.
[OBSERVABILITY_DEPTH]-[COMPLETE]: every measured plane instruments. `impact`/`graph`/`profile` gained solve/kernel/interrogate spans and `domain="impact"`/`"graph"`/`"quality"` projections; `egress` projects `rasm.egress.byte_volume`; `materialize` records `rasm.materialize.rows`; `lakehouse` spans commits; the gridded plane carries per-leg spans and the `PlanReceipt` `to_builtins` projection; remote legs open `SpanKind.CLIENT`. ERROR marking stays the runtime `boundary` fence's, tenant-on-span the telemetry install's; `add_link` is ruled out — nesting and content keys already correlate.
