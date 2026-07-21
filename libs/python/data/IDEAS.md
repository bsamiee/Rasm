# [PY_DATA_IDEAS]

Forward pool of higher-order concepts for `data`, grounded in the host-free interchange role. Each idea is a card — slug leader with the capability, what it unlocks, and the gap or technique it draws on — and spawns one or more tasks in `TASKLOG.md`. `[1]-[OPEN]` holds live concepts; `[2]-[CLOSED]` records dispositions so an idea is never re-litigated.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis — the capability outcome, never the landing motion>.
- Capability: <the higher-order invariant, owner capability, or concept established — altitude only, never a page path, row list, or member spelling>.
- Shape: <where the work lands and at what grain — repo-relative page with section/row, or a new-page path; the concrete surface, so Capability never names it>.
- Unlocks: <the downstream capability at the consumer grain — a task narrows its parent idea's Unlocks to THIS slice as `IDEAS.md [SLUG] — consequence`; a set-completion card states the completeness bar that is its acceptance contract>.
- Anchors: <owners, seams, packages, catalogs, doctrines, and techniques making the work plausible — anchors, never procedures>.
- Arms: <present only on a BLOCKED or gated card; the exact observable that flips it actionable — a catalog row landing, a member query returning evidence, a package admitted>.
- Route: <present only on a probe, research, or member-pin card; the ordered verification path run before any fence lands>.
- Tension: <only when an unresolved constraint, boundary, or bet shapes the work — the genuine bet, never the arming condition Arms carries>.
- Ripple: <counterpart card — cross-folder as `pkg` `[SLUG]` or a same-folder prerequisite `[SLUG]`, prefixed follows/precedes/mirrors when build order is load-bearing>.
Capability, Shape, Unlocks, and Anchors are required on every open card; statuses closed — `ACTIVE|QUEUED|BLOCKED` open, `COMPLETE|DROPPED` closed; IDs are SEMANTIC UPPERCASE_SNAKE slugs carrying meaning — never numeric (`[0007]`-class NNNN IDs are a defect), for cards AND research tokens alike; a hyphenated slug anywhere is a defect; repo-relative paths only. Design pages carry the terminal `[RESEARCH]` section always — `(none)` marks empty, absence is an error. Ideas state higher-order concepts, never landing-grain tasks.
-->

[ENGINE_PROFILE_PARITY]-[BLOCKED]: Native profile payloads converge on the shared query profile band.
- Capability: DuckDB and Polars native operator evidence joins the settled scalar band and Daft operator rows through `EngineProfile.of`.
- Shape: one `ProfileHarvest` case per proven native payload; portable scalar harvest remains the truthful floor for every engine.
- Unlocks: engine-native operator evidence joins the shared profile band, cost comparison across DuckDB, Polars, and Daft reading one receipt shape.
- Anchors: `.planning/tabular/columnar.md` `[DUCKDB_PROFILE_PAYLOAD]` and `[POLARS_PROFILE_PAYLOAD]`; `QueryReceipt.profile`.
- Arms: both folder-tier catalog rows carry exact return types and payload schemas for `get_profiling_information()` and `LazyFrame.profile()`.

[LAYER_TOPOLOGY_GRAPH_FACTS]-[QUEUED]: Decoded `LayerTopologyFact` rows fold into a containment graph the graph plane analyzes for host organization.
- Capability: Wire-carried layer and relation keys decode into a `GraphPayload` whose nodes are layer identities and whose edges are layer-path nesting and membership, so topology analysis — containment ancestry, nesting depth, membership closure — answers layer organization over the decoded graph with no host handle; per-viewport overrides ride the decoded rows as detached facts.
- Shape: A boundary decoder folds the detached fact rows into the `graph/graph` plane's node-link source; `GraphPayload.analyze` runs the containment and nesting queries on the one `rustworkx` kernel keyed by the stable `NodeId` index, and the node-keyed `GraphResult.frame` left-joins layer organization onto the `tabular/columnar` scan plane by `node`.
- Unlocks: Host-organized element queries over the graph plane, layer-scoped dataset slicing, and one decoded organizational axis every interchange consumer reads by name.
- Anchors: `graph/graph.md` `GraphPayload`/`analyze`/`NodeId`/`GraphResult.frame`; `rasm.runtime.identity` `ContentIdentity`/`ContentKey` for the stable wire identity; `README.md` host-free interchange role meeting C# only at the content-identity wire.
- Tension: Wire schema and codec mint in C#; this plane decodes and never re-mints, and the containment graph carries only detached fact rows, never a host layer handle.
- Ripple: `libs/.planning` `[LAYER_TOPOLOGY_GRAPH_FACTS]`.

[ADBC_DRIVER_SET]-[QUEUED]: ADBC driver family completes with native Postgres, SQLite, and Snowflake arms beside the admitted manager and Flight SQL rows.
- Capability: `QuerySpec.Remote` reaches Postgres, SQLite files, and Snowflake warehouses through native ADBC drivers on the one `RemoteOp` sub-axis — same DBAPI bracket, same retry class, same receipt fold — closing the partial-family state where only the manager and Flight SQL arms exist.
- Shape: driver rows on the query plane's remote dispatch — `adbc-driver-postgresql`, `adbc-driver-sqlite`, `adbc-driver-snowflake` — each a row naming its driver entrypoint and `db_kwargs` knob family, ConnectorX still accelerating the read-parallel paths per the landed division.
- Unlocks: local-file and warehouse federation with zero new query surface, and the sqlite storage arm the branch instrumentor train already covers.
- Anchors: `.planning/tabular/query.md` `Remote`/`RemoteOp`/`guarded(RetryClass.REMOTE_DB)`; `.api/adbc-driver-manager.md` `dbapi.connect`; the set-completion law — a partial admitted family names every member.
- Tension: driver admission rides the serialized admission lane; cards assume it lands.

[SUBSTRAIT_PLAN_GATE]-[QUEUED]: inbound Substrait plan bytes validate and introspect through a typed plan model before any engine executes them.
- Capability: a Persistence-authored plan admits through typed parse and validation — a malformed or version-skewed plan becomes a typed refusal at the gate instead of a datafusion engine fault mid-execution — and a relation-op census off the parsed plan enriches the receipt beside `lineage_edges`.
- Shape: a `substrait`-package admission fence on the query plane's `Federated` and `Flight` arms — parse, validate, census, then hand the untouched original bytes to `Serde.deserialize_bytes`; the plan-bytes identity law survives because the gate never re-serializes.
- Unlocks: fail-fast federation over the C# wire and plan-structure evidence on the one `QueryReceipt` stream.
- Anchors: `.planning/tabular/query.md` `Federated`/`Flight` arms, `ContentIdentity.of("query.plan", wire)`; `Rasm.Persistence` federation wire per `ARCHITECTURE.md` `[02]`.
- Tension: data executes foreign plans and never re-plans them — the gate inspects and refuses, never rewrites.

[GEOARROW_NATIVE_SET]-[QUEUED]: GeoArrow family completes — core array types, native format IO, and the pyarrow extension bridge beside the admitted compute kernels.
- Capability: FlatGeobuf, GeoParquet, shapefile, and GeoJSON decode straight into GeoArrow memory, shapely and geopandas geometry bridges zero-copy through pyarrow extension arrays, and the full compute member set — `frechet_distance` for scan-trajectory similarity, `line_locate_point`, geodesic measures — runs on the arrays the claims plane already egresses.
- Shape: `geoarrow-rust-core` array types as the typed memory model under the existing native-GeoArrow egress, `geoarrow-rust-io` reader and writer rows on the vector format axis where the GDAL hop adds no value, `geoarrow-pyarrow` as the shapely-interop bridge, and a compute-member sweep landing the unexploited kernel rows.
- Unlocks: GDAL-free vector IO on the hot paths and trajectory analytics over point-sequence datasets.
- Anchors: `.planning/spatial/geospatial.md` `VectorOp` axis and native-GeoArrow egress; `.api/geoarrow-rust-compute.md` member table; `pyogrio` staying the GDAL long-tail owner.
- Tension: pyogrio keeps every format the rust readers do not spell — the family adds a fast path, never a second format owner.

[ARRO3_SLIM_SET]-[QUEUED]: arro3 family completes — compute kernels and format IO beside the admitted core, so pyarrow-free hops stop re-importing.
- Capability: aggregation, cast, take, and filter kernels run directly on arro3 frames, and IPC, Parquet, and CSV read-write exists without the full pyarrow import — closing the exact hop `materialize` documents where an arro3 `RecordBatchReader` re-imports through `pa.table(...)` only to reach a compute surface.
- Shape: `arro3-compute` kernel rows where the operand is already arro3-native, `arro3-io` rows on the interop carrier for slim IPC legs; pyarrow stays the rich-surface owner wherever acero, dataset, or flight capability is in play.
- Unlocks: lighter interchange legs for host-embedded consumers where pyarrow weight matters, and the arro3 family admitted as a set instead of a stray core.
- Anchors: `.planning/tabular/materialize.md` PyCapsule re-import hop; `.planning/tabular/interop.md` `ArrowCStream`; `.api/arro3-core.md`.
- Tension: two Arrow surfaces stay lawful only with a ruled split — arro3 for slim carrier legs, pyarrow for engine-adjacent work; an arbitrary per-page choice is the rejected form.

[VECTOR_DATA_CUBES]-[QUEUED]: geometry-indexed field cubes land as one owner — zone-keyed simulation results queryable by space.
- Capability: an xarray dimension indexed by shapely geometry carries per-zone, per-room, and per-sensor-location results — energy series by thermal zone, daylight grids by room — so a spatial predicate selects cube slices and a cube variable joins the vector claims plane without a hand-rolled zone-id join table.
- Shape: `libs/python/data/.planning/spatial/cube.md` — an `xvec`-backed vector-cube owner bridging `FieldDataset` cubes and `VectorGeoClaim` frames, geometry-predicate selection lowering to the claims plane's CRS law, egress through the existing content-keyed field receipt family.
- Unlocks: simulation-result interchange keyed by building geometry — the missing bridge between the gridded and spatial planes.
- Anchors: `.planning/gridded/field.md` `FieldDataset` CF owner; `.planning/spatial/geospatial.md` `VectorGeoClaim.reproject`; branch `xarray` substrate; `shapely`.
- Tension: field.md stays the pure CF owner by its own charter law — the cube owner is a distinct page composing it, never a second labelled-array store inside `gridded`.

[COMPRESSED_CARRIER_BAND]-[QUEUED]: transport-band IPC compression with content identity preserved on uncompressed bytes.
- Capability: large frames crossing to siblings, C# peers, or object storage ride lz4- or zstd-compressed Arrow IPC on the transport band while every `ContentKey` keeps deriving off the uncompressed serialization — wire volume drops without a single identity changing.
- Shape: a compression option on the interop carrier's IPC egress (`ipc.IpcWriteOptions(compression=)`) parameterized as a closed codec vocabulary, applied at transport and egress seams only; the `arrow_bytes` identity fold stays byte-stable and uncompressed by law.
- Unlocks: cheaper egress and cross-runtime transfer for scan-scale point-cloud and tensor frames.
- Anchors: `.planning/tabular/interop.md` `ArrowCStream` and IPC lowering; `.planning/tabular/egress.md` put paths; branch `lz4` catalog; `.api/pyarrow.md` `ipc` submodule.
- Tension: a compressed byte stream must never reach an identity fold — the codec vocabulary lives on the transport parameter, and the C# decode arm lands as the cross-language counterpart.

[APP_NEUTRAL_STORE_SCOPES]-[QUEUED]: per-app write guards and scoped provider handles keep two apps from fighting over one store.
- Capability: concurrent same-process apps composing the lakehouse, tensor, or catalog planes never collide — a single-writer guard rejects concurrent commit use per app scope instead of surfacing as a late commit conflict, and provider handles carrying account state scope per composition instead of per process.
- Shape: `anyio` `ResourceGuard` rows on the lakehouse commit and tensor-store write paths, guard ownership riding the composition-bound policy the pages already thread; the catalog plane's process-wide `planetary_computer` signing handle re-scopes to a composition-bound row so two apps hold distinct subscription configurations.
- Unlocks: the app-neutrality law made structural on the store planes — collision dies at the guard, not in conflict-retry telemetry.
- Anchors: branch `.api/anyio.md` `ResourceGuard`; `.planning/tabular/lakehouse.md` `LAKE_COMMIT` conflict tags; `.planning/gridded/store.md` write paths; `.planning/spatial/catalog.md` scheme rows.
- Tension: cross-process coordination stays the store format's own optimistic-commit protocol — the guard owns in-process app isolation only.

[IMPACT_PLANE_BUILDOUT]-[QUEUED]: impact grows from one compressed page into a provider-deep plane realizing the staged Brightway, openLCA, and prospective-scenario capability.
- Capability: EPD declaration ingest, LCI inventory custody, LCA solve and contribution analysis, and IAM prospective scenarios each get owner-depth treatment — the EC3 search stream, `MultiLCA` shared-factorization batch, contribution rows, and staged system boundaries move from named-deferred to realized.
- Shape: `impact/impact.md` stays the EN 15804 carrier and `ImpactSource` axis owner; sibling pages land as `libs/python/data/.planning/impact/declaration.md` (openepd, epdx, EC3 sync and offline bundles), `libs/python/data/.planning/impact/inventory.md` (bw2data, bw2io, bw-processing project and datapackage custody), `libs/python/data/.planning/impact/solve.md` (bw2calc, bw2analyzer, olca-ipc solve and contribution), and `libs/python/data/.planning/impact/scenario.md` (premise IAM background transforms).
- Unlocks: whole-building LCA at package depth — nine admitted packages each exploited past the normalization floor, and the stub-folder ruling satisfied with real capability.
- Anchors: `.planning/impact/impact.md` `ImpactSource`/`_normalize`/Growth staged rows; `.api/bw2io.md`, `.api/bw2calc.md`, `.api/olca-ipc.md`, `.api/premise.md` member depth.
- Tension: the carrier page keeps the one normalization fold — sibling pages feed `ImpactSource` cases, never a second EN 15804 matrix.

[GEOMETRY_FRAME_ADMISSION]-[QUEUED]: geometry's analytic frame band admits into the analytics plane beside `ResultFrame`.
- Capability: subject-keyed columnar frames minted by geometry's frame port — deviation bands, quality metrics, analytic boards, section properties, lifecycle rollups — land as admitted frame families on the scan plane, duckdb- and lake-queryable beside every other columnar source.
- Shape: one admission row family on `libs/python/data/.planning/tabular/columnar.md` keyed by the `GeometrySubject` discriminant, joins keyed on `ContentKey`, receipts on the standing `domain="query"` projection; the crossing rides the existing geometry-to-data seam beside `ResultFrame`.
- Unlocks: estate analytics over geometry evidence — cross-model deviation trends, quality rollups, section-property queries — with zero bespoke receipt parsing.
- Anchors: geometry frame port on `libs/python/geometry/.planning/graduation.md`; `tabular/columnar#SCAN`; runtime `ContentKey`.
- Ripple: `geometry` `[ANALYTIC_FRAME_EGRESS]`.

[IFC_CRS_SOURCE]-[QUEUED]: IFC-minted georeference admits as a first-class CRS source on the reproject prelude.
- Capability: CRS identity and local-to-map transform extracted by geometry's IFC georeference band enter `VectorGeoClaim`'s pyproj prelude as a typed CRS source, so site-local claims lift to map frames off model truth instead of caller-supplied CRS guesses.
- Shape: one CRS-source row on `libs/python/data/.planning/spatial/geospatial.md` decoding the georeference fact — CRS name, map-conversion transform, true north — into the reproject prelude beside the existing CRS inputs.
- Unlocks: scan-vs-model claims, vector egress, and map-frame joins keyed to the model's own georeference; the python data leg of the estate geospatial root move.
- Anchors: geometry georeference band on `libs/python/geometry/.planning/ifc/analysis.md`; `VectorGeoClaim.reproject` and the CRS law on `spatial/geospatial.md`; `.api/pyproj.md`.
- Ripple: `geometry` `[IFC_GEOREFERENCE]`.

[SCENARIO_FIELD_TREES]-[QUEUED]: scenario and ensemble families land as one labelled tree over the CF field plane.
- Capability: a `DataTree` hierarchy carries multi-scenario simulation families — design options, climate years, IAM prospective backgrounds — as parent/child groups whose leaves are `FieldDataset` cubes, so cross-scenario map, reduce, and difference run group-wise in one call instead of N hand-looped cubes.
- Shape: one new page `libs/python/data/.planning/gridded/ensemble.md` owning the tree constructor, the scenario-axis vocabulary, group-wise operation folds, and content-keyed egress on the `FieldReceipt` family; composes the CF owner, never a second labelled-array store.
- Unlocks: scenario-set interchange for energy and impact result families; per-scenario deltas queryable beside the vector-cube and claims planes.
- Anchors: `xarray.DataTree` on branch `libs/python/.api/xarray.md`; `FieldDataset`/`FieldReceipt` on `gridded/field.md`; the `[IMPACT_PLANE_BUILDOUT]` scenario page family.

[GRAPH_NETWORK_ANALYSIS]-[QUEUED]: capacity-constrained network analysis lands as the graph plane's flow page.
- Capability: building-service networks — duct, pipe, cable, egress circulation — answer max-flow, min-cost flow, and network-simplex questions over the graph plane's payloads, results lowering to the same node-keyed frame the columnar plane joins.
- Shape: `libs/python/data/.planning/graph/network.md` — a capacity-annotated network owner riding the existing networkx codec lane for the flow family the rustworkx kernel does not spell, flow dictionaries lowering through `GraphResult.frame`, receipts on the standing `domain="graph"` projection.
- Unlocks: MEP sizing and circulation evidence over interchange graphs, and the graph stub folder deepened with real capability.
- Anchors: `.planning/graph/graph.md` codec lane and `GraphResult.frame`; branch `.api/networkx.md` `maximum_flow`/`min_cost_flow`/`network_simplex`; `rustworkx` capability line proving the flow-family absence.
- Tension: rustworkx stays the kernel for everything it spells — the networkx leg exists only for the flow family, never a parallel analysis kernel.

[EPD_RECORD_WIRE]-[QUEUED]: registry-sourced EPD records carry the Assessment edge — declared units, module coverage, and expiry keyed to the Materials identity.
- Capability: EC3, Ökobaudat, and EPD-Norge records normalize through the standing `openepd`/`epdx` declaration arms into `MaterialImpact` rows carrying declared unit, EN 15804 module coverage, and expiry, keyed to `MaterialId`/component designation so the C# Materials assessment landing decodes real product vectors in place of its authored generic-EPD constants.
- Shape: a registry-source arm on `libs/python/data/.planning/impact/impact.md` beside the declaration fold — provider fetch discriminated by payload shape, never a provider knob — with the record schema and transport co-signed as the `Discipline.Environmental` `Assessment` seam payload the page already routes; redistribution follows the `[IMPACT_PAGE_SPLIT]` landing when the provider pages mint.
- Unlocks: audited whole-life carbon on the C# side from evidence-dated records; one EPD sourcing plane serving every estate consumer.
- Anchors: `impact.md` `MaterialImpact` eight-column frame and `Assessment` crossing; `openepd`/`epdx` wire parsers; the `tabular/columnar` content-keyed Arrow-bytes fold.
- Tension: provenance stays with the registry record — expiry and declared-unit facts cross untranslated, and the Materials end demotes its authored constants to declared fallback per its own card.
- Ripple: `csharp:Rasm.Materials` `[EPD_DATA_INGESTION]`.

## [02]-[CLOSED]

<!-- source-only: closed idea card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[EMBEDDED_ENGINE_OBSERVABILITY]-[COMPLETE]: embedded engines carry no scrape surface, so the profiled session bracket became the DuckDB observability owner — harvest folded onto the one `QueryReceipt` stream, instruments projected through the runtime metric spine, DBAPI spans owned by the root-composed instrumentor train.
[QUERY_BENCH_LANE]-[COMPLETE]: landed — `tabular/query#QUERY` `QueryEngine.bench` drives runtime `Bench.run` per `QuerySpec` tag under the `_BENCH_MODE` rows, refuses the mutation `Remote` INGEST spec, and rides `anyio.run` per round.
[DATA_HOOK_POINTS]-[COMPLETE]: `.planning/tabular/materialize.md` `DATA_HOOK_POINTS` and `register_data_hooks(scope)` consume every registration rail at composition; each emitting owner carries the same `ScopeKey` into `Hooks.fire`.
[DATASET_COST_LEDGER]-[COMPLETE]: `.planning/tabular/cost.md` `CostFact.of` normalizes receipts, wire mappings, and facts; `CostLedger.of` harvests that mixed stream before the content-keyed priced-frame fold.
[DBAPI_SPAN_THREADING]-[COMPLETE]: landed — `tabular/query#QUERY` `dbapi_seams()` declares the duckdb/ADBC/Flight SQL `DbapiSeam` rows the composition root threads through `Instrumentation.dbapi`; ConnectorX excluded by shape.
