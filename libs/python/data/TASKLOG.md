# [PY_DATA_TASKLOG]

Open and closed work for `data`, distilled from `IDEAS.md`. Each task card leads with a status marker — `[QUEUED]`, `[ACTIVE]`, or `[BLOCKED]` open; `[COMPLETE]` or `[DROPPED]` closed — and carries the capability or file to build, the external packages to integrate, the integration points and boundaries, and the key considerations. `[1]-[OPEN]` holds live work; `[2]-[CLOSED]` records finished or dropped tasks.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with. `Atomic` flags a minor-scope task so a later session sizes its turn correctly and does not overscope a batch of small items.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis>.
- Capability: <higher-order concept, invariant, or owner capability>.
- Shape: <what the idea becomes as a system, product, owner, or feature set(s)>.
- Unlocks: <new branch, package, workflow, proof, user, or agent capability made possible>.
- Anchors: <owners, seams, packages, doctrines, or techniques that make the idea plausible>.
- Tension: <only when an unresolved constraint, boundary, bet, or dependency shapes the idea>.
- Ripple: <origin/counterpart card this entry pairs with across folders, as `pkg` `[SLUG]`; present only on a cross-folder ripple counterpart card>.
- Atomic: <present only on a minor-scope task; one short phrase naming the small unit so a later session does not overscope its turn>.
-->

[INTEROP_STREAM]-[QUEUED]-[F+Q]: Deepen the thin tabular/interop owner (3-row Backend cap, no streaming/device) to own the full Arrow C-stream + device pipeline and absorb the streaming carrier the cut STREAM proposed — the structural-defect fix is DEPTH, not collapse.
- Capability: Streaming consumption via `nanoarrow.c_array_stream` (chunked, no full materialization); schema-only negotiation (`__arrow_c_schema__` vs `contract.FieldShape`); the C Device Data Interface (`__arrow_c_device_array__`, one device discriminant); zero-copy into arro3-core constructors. Lift the Backend axis from 3 eager rows to the full lazy backend set (DUCKDB/IBIS/PYSPARK/DASK via narwhals lazy), with `_LOWER` branching eager-vs-lazy on the Implementation level. Owns the `RecordBatchReader` streaming carrier (a `[FEED]` cluster if push-egress needs a distinct home). Promotes the existing private `_export_c_stream` body to the public `ArrowCStream.of` classmethod the `gridded/ragged#RAGGED` owner already composes.
- Shape: large.
- Unlocks: pyarrow-free streaming Arrow interchange across every PyCapsule-exporting engine; DuckDB relations as first-class agnostic lazy frames; the C-stream carrier the cut STREAM proposed, now folded here; settles the `gridded/ragged#RAGGED` `INTEROP_CONSTRUCTION` pending `ArrowCStream.of` promotion.
- Anchors: `interop.md#INTEROP` `Backend`/`_LOWER`/`ArrowCStream`, `narwhals` lazy backends + `from_native(duckdb_relation)`, `nanoarrow.c_array_stream`, `arro3-core` constructors, `contract.FieldShape`, `gridded/ragged#RAGGED`.
- Tension: Lazy backends have no eager `to_polars`/`to_pandas` lower — `_LOWER` must branch eager-vs-lazy on the Implementation level; the device row is RESEARCH; the streaming carrier is an in-page cluster, not a cross-sub-domain seam; the `ArrowCStream.of` promotion is the ripple the ragged owner waits on, so the fence lands the classmethod on the same `ArrowCStream(Struct, frozen=True)` owner.

[SUBSTRAIT_PORTABILITY]-[QUEUED]-[F]: Admit datafusion + duckdb-substrait for a portable binary relational-plan IR — the canonical cross-engine/cross-language query-plan wire (one optimized plan executes on DuckDB/DataFusion/Daft and crosses the C# boundary without SQL re-parse), and the federated multi-source pushdown engine no owner provides.
- Capability: A `QuerySpec.Plan(substrait_blob)` case (duckdb-substrait `get_substrait`/`from_substrait` on the request-scoped connection that self-loads the substrait extension) emitting the plan blob as content-keyed portable evidence; a `QuerySpec.Federated(sql, sources)` case binding datafusion `SessionContext` + `register_object_store` + datafusion-federation, materializing via async streaming to `pyarrow.Table`, carrying a Substrait plan-interchange leg.
- Shape: large.
- Unlocks: Query plan as a content-keyed transportable artifact; federated query over Postgres + lake + Flight SQL with predicate pushdown; the cross-language plan wire.
- Anchors: new `.api/datafusion.md`/`.api/duckdb-substrait.md`, datafusion `SessionContext`/`register_table`/Substrait, duckdb-substrait `get_substrait`/`from_substrait`, `query.md#QUERY` `_dispatch`, the per-connection extension-load precedent.
- Tension: the DuckDB/duckdb-substrait Substrait wire (`query.md#QUERY` `PlanWire.SUBSTRAIT`/`SUBSTRAIT_JSON`) is landed; the remaining open scope is the `datafusion` `SessionContext` federation engine (`QuerySpec.Federated`) — datafusion is a federation/streaming axis distinct from DuckDB's in-process model, one new QuerySpec case, not a duplicate SQL frontend; the C# Persistence federation seam is real design pressure now, not deferred.
- Ripple: `csharp:Rasm.Persistence` `[SUBSTRAIT_FEDERATION_SEAM]` — Persistence `Query/Federation` consumes the portable Substrait binary plan and the ibis `to_sql` SQL this owner emits over the `tabular/query ⇄ csharp:Rasm.Persistence/Query/federation` `[WIRE]` seam as the C# federation counterpart.

[LAKEHOUSE_DUCKLAKE_FORMAT]-[QUEUED]-[F]: Admit DuckLake as a fourth TableFormat row via the already-admitted duckdb (zero new pip dependency) — the SQL-catalog (Postgres/SQLite) lakehouse over Parquet none of Delta/Iceberg/Lance expresses; ducklake is core-loadable on DuckDB 1.5.4.
- Capability: One `TableFormat.DUCKLAKE` row + one `_apply_ducklake` arm: Write->INSERT/CREATE TABLE AS, Read(version)->`AT (VERSION=>N)`/`AT (TIMESTAMP)`, ChangeFeed->`table_changes(t,start,end)`, Optimize/Vacuum->ducklake maintenance CALL functions; on a request-scoped `duckdb.connect()` that LOADs ducklake (core-loadable, no INSTALL round-trip), `ATTACH 'ducklake:<dsn>' (DATA_PATH ...)`. Lands as ONE TableFormat row on lakehouse, not a query frontend.
- Shape: large.
- Unlocks: A Postgres-backed transactional catalog with ACID + CDF + time-travel co-located with the runtime's own Postgres; cp315-clean lakehouse the gated pyiceberg cannot give.
- Anchors: `.api/ducklake.md`, `lakehouse.md#LAKEHOUSE` TableFormat + _apply, the per-connection LOAD pattern `columnar#SCAN` establishes for httpfs.
- Tension: The duckdb connection is request-scoped (never a module global); metadata-DSN routes through runtime TransportResource/ResourceRef, never a second catalog owner; one `_apply_ducklake` arm, never a parallel DuckLakehouse owner; ducklake is core-loadable (LOAD, no INSTALL).
- Ripple: `python:runtime` `[DATA_TRANSPORT_DSN]` — the ducklake catalog DSN resolves through the one runtime `TransportResource` owner, never a second credential owner on the data side.

[DUCKDB_ICEBERG_PROMOTE]-[QUEUED]-[F]: Route the cp315 Iceberg read/write path through the DuckDB iceberg extension (ATTACH TYPE iceberg, SECRET, MERGE INTO) — cp315 Iceberg currently requires the <3.15-gated pyiceberg; the DuckDB extension is a cp315-clean single-engine route (iceberg is core-loadable on 1.5.4), demoting pyiceberg to the <3.15 catalog-write fallback.
- Capability: Route the primary Iceberg path on `lakehouse._apply_iceberg` through the DuckDB iceberg extension on a request-scoped connection (LOAD iceberg — core-loadable, no INSTALL round-trip; `ATTACH 'warehouse' AS cat (TYPE iceberg, SECRET s, ENDPOINT)`, `CREATE SECRET TYPE iceberg`, SELECT/INSERT/MERGE INTO); pyiceberg becomes the `<3.15` catalog-write fallback under the band gate.
- Shape: large.
- Unlocks: cp315-clean Iceberg read+append without the gated pyiceberg dependency; a genuine band upgrade.
- Anchors: extend `.api/duckdb.md` with an `[EXTENSIONS]` section (iceberg/delta/ducklake/httpfs/spatial SQL surfaces), `lakehouse.md#LAKEHOUSE` `_apply_iceberg`, the per-connection LOAD pattern.
- Tension: The `_apply_iceberg` arm now has a cp315 DuckDB path and a `<3.15` pyiceberg path under the band gate; iceberg is core-loadable on 1.5.4 (LOAD, no INSTALL); resolve the `[ICEBERG_SNAPSHOT]` RESEARCH against whichever path is live; confirm the iceberg extension write surface against current upstream.
- Ripple: `python:runtime` `[DATA_TRANSPORT_DSN]` — the DuckDB iceberg `SECRET`/`ENDPOINT` DSN resolves through the one runtime `TransportResource` owner, never a second credential owner on the data side.

[GRAPH_DEEPEN]-[QUEUED]-[F+Q]: Widen the graph owner to rustworkx's real breadth and the README-named-but-omitted capabilities, collapsing the two near-identical 10-arm _run_rx/_run_nx match kernels into one backend-dispatched table generalizing _NX_CENTRALITY — the owner taps ~11 of 40+ algorithms.
- Capability: Add GraphAlgorithm cases AStar/KShortest/Matching/MinCut/SpanningTree/Steiner/DagLongestPath/TransitiveReduction/Dominators/Articulation/Bridges/Biconnected/Isomorphic/Planar/Layout + MaxFlow/MST (the README-named cases), each one arm in the folded backend-dispatched table; add GraphResult cases tree/cut/mapping/positions/flag; bind the networkx file-codec matrix (GEXF/GML/GraphML/Pajek/graph6/sparse6/node-link) + `from_pandas_edgelist`<->`to_pandas_edgelist` / `from_scipy_sparse_array` / `from_numpy_array` bridges as ingest/egress rows; `_run_nx` mirrors only where networkx owns the algo, a backend-missing case folding to a typed BoundaryFault.
- Shape: large.
- Unlocks: Structural/matching/cut/isomorphism/dominance/layout algorithms + max-flow/MST/articulation + graph file interchange + tabular edge-list egress over the same GraphPayload owner.
- Anchors: `.api/rustworkx.md` (matching/cut/structure/isomorphism/DAG/A*/layout/max_flow/minimum_spanning_tree/articulation families), `.api/networkx.md` (file-codec matrix + tabular/array bridges), `graph.md#GRAPH` `_run_rx`/`_run_nx` match, GraphResult union.
- Tension: Collapse the two near-identical match kernels into one backend-dispatched table generalizing `_NX_CENTRALITY` — keep one folded kernel, never parallel arms or a per-codec write_* family; `_run_nx` mirrors only where networkx owns the algo (planarity/layout differ); a backend-missing case folds to RuntimeRail BoundaryFault.

[GRAPH_COMMUNITY]-[QUEUED]-[F]: Admit igraph for the community-detection plane rustworkx lacks (zero community detection in 0.17, networkx Louvain slow) — Leiden/Louvain/Infomap C-core returning a node->community membership matching GraphResult.partition.
- Capability: A third GraphBackend row + Leiden/Louvain/Infomap/LabelPropagation GraphAlgorithm cases + one `_run_ig` arm returning `GraphResult(partition=...)` via `VertexClustering.membership`; `GraphPayload.of` admits an `igraph.Graph` and recovers the backend; community cases pin igraph by class even from a rustworkx source via `Graph.from_networkx`/`Graph.DataFrame`.
- Shape: large.
- Unlocks: Modularity-scored partition discovery over BIM/fabrication graph corpora feeding tabular edge-list egress.
- Anchors: `.api/igraph.md`, igraph `community_leiden`/`community_multilevel`/`modularity`/`VertexClustering.membership`, `graph.md#GRAPH` GraphBackend/GraphResult(partition), Directory [scientific].
- Tension: Admit dist igraph (NOT deprecated python-igraph; import stays `import igraph`); backend recovery stays value-driven — community cases pin igraph, never a knob; execution provisions igraph and probes the cp315 C-core wheel, gating `<3.15` only if it lags; gates on `.api/igraph.md`.

[GEOSPATIAL_INGRESS_DEEPEN]-[QUEUED]-[F]: Add the pushdown ingress the geospatial plane wholly lacks (~15% pyogrio tapped) — full egress but zero pushdown ingress; deepen pyproj/shapely/geopandas underuse; add a native GeoArrow path via geoarrow-rust-compute.
- Capability: A vector ingress path over `pyogrio` `read_dataframe`/`open_arrow` with OGR-side where/bbox/mask/sql pushdown; pyproj geodesic ops + cached transformer; shapely `polygonize`/`node`/linear-referencing/`to_ragged_array`; geopandas `to_arrow`/`estimate_utm_crs`/PostGIS round-trip. A native GeoArrow path via geoarrow-rust-compute (GeoRust on GeoArrow memory, zero GEOS/GDAL) replacing the to_parquet byte-roundtrip for `EgressFormat.GEOARROW`, sharing buffers with the C# GLB wire.
- Shape: large.
- Unlocks: Pushdown vector ingress; geodesic CRS ops; native GeoArrow buffers shared with the C# wire.
- Anchors: `.api/pyogrio.md` (read_dataframe/open_arrow), `.api/pyproj.md`/`.api/shapely.md`/`.api/geopandas.md`, `geospatial.md#GEO`/#SPATIAL, geoarrow-rust-compute.
- Tension: geoarrow-rust-compute is NOT abi3 — cp310..cp314, no cp315 wheel -> gated `<3.15` (no abi3 caveat); polars-st (cp39-abi3) and h3ronpy (cp39-abi3) are cp315-clean DGGS/spatial-frame admissions; sedonadb defers to an IDEAS engine row (no cp315 wheel — the polymorphic spatial-backend axis is the holding pattern); only the GEOARROW egress encoding landed, the pushdown ingress and the geoarrow-rust-compute native path remain.
- Ripple: `csharp:Rasm.Compute` `[DATA_GEOARROW_GLB]` — the native GeoArrow coordinate/offset buffers this path produces share the C# GLB tessellation wire buffer layout, so Compute consumes the geospatial interchange at the wire rather than stranding it one-sided on the Python producer.

[CATALOG_COVERAGE_ODCSTAC]-[QUEUED]-[F+Q]: Add the labelled-raster Coverage owner over rioxarray — the geospatial `[COVERAGE]` cluster bridging bare-ndarray CoverageResult to CF `xarray.DataArray` and closing the raster-WRITE gap; the catalog `odc.stac.load` arm is landed.
- Capability: A new geospatial `[COVERAGE]` cluster over rioxarray bridges bare-ndarray CoverageResult to CF `xarray.DataArray` and closes the raster-WRITE gap (COG output via `.rio.to_raster(driver="COG")`); the `catalog#ASSETS` `Coverage` target already drives `odc.stac.load` for COG cube load and the relocated `FieldDataset.virtual` for HDF5/NetCDF.
- Shape: large.
- Unlocks: Raster as a labelled DataArray feeding the CF field store; COG write-back the EgressFormat lacks; correct COG cube load from STAC discovery is already live.
- Anchors: `.api/odc-stac.md`/`.api/rioxarray.md`, `rioxarray.open_rasterio`/`.rio.{reproject,to_raster}`, `geospatial.md` new `[COVERAGE]`, `gridded/field`, the landed `catalog.md#ASSETS` `Coverage` target.
- Tension: rioxarray/odc-stac are cp315-clean pure-Python (no gate); xarray+dask bind function-local; the catalog `odc.stac.load` arm has replaced the type-incorrect `fold_virtual_cube` COG path, so virtualizarr stays scoped to HDF5/NetCDF; the remaining open scope is the geospatial `[COVERAGE]` rioxarray DataArray bridge and COG write-back.

[GEOSPATIAL_TERRAIN_GATED]-[QUEUED]-[F]: Add the raster-ANALYTICS axis the cluster wholly lacks (current RasterOp is IO/reshape only) — a Terrain DEM-analytics owner over xarray-spatial, gated <3.15 (numba+scipy both gated).
- Capability: A `Terrain` owner over one `TerrainOp` axis (slope/aspect/hillshade/curvature/viewshed/proximity/zonal.stats) via xrspatial, numba-accelerated derivatives over the `[COVERAGE]` DataArray.
- Shape: large.
- Unlocks: DEM/surface analytics — the raster-analytics axis above IO/reshape.
- Anchors: `.api/xarray-spatial.md`, `xrspatial.{slope,aspect,hillshade,curvature,viewshed,proximity,zonal.stats}`, `geospatial.md` new `[TERRAIN]` (gated arm), the `[COVERAGE]` DataArray.
- Tension: CRITICAL gate — numba+scipy gated `<3.15` -> rides the companion band, `python_version<'3.15'` marker, function-local import; depends on the CATALOG_COVERAGE_ODCSTAC `[COVERAGE]` DataArray; execution provisions xarray-spatial gated.
- Ripple: `python:compute` `[DATA_STUDY_INPUT]` — the compute numerics array seam aligns its numba/scipy `<3.15` band to this Terrain owner's `xrspatial` gate, mirroring the `python:data/spatial/geospatial` study-input edge on both endpoints.

[STREAM_PIPELINE]-[QUEUED]-[F]: changefeed-driven incremental and windowed pipeline execution on the new stream/incremental.py owner.
- Capability: incremental/windowed pipeline execution over the polars/daft streaming engines on `stream/incremental.md`, owning window/watermark/state.
- Shape: consumes (never re-mints) `tabular/columnar#MATERIALIZE` DerivedSnapshot deltas + the `tabular/lakehouse` changefeed + the `tabular/interop#INTEROP_STREAM` carrier, and emits on the existing `python:runtime/transport` wire.
- Unlocks: streaming derived-frame materialization without full-snapshot re-reads.
- Anchors: `stream/incremental.md`, `tabular/columnar#MATERIALIZE`, `tabular/interop#INTEROP_STREAM`, polars/daft streaming engines, `python:runtime/transport`.
- Tension: LARGE scope; the page-name `stream/incremental.md` and any streaming-engine admission are validated before fence transcription; the `tabular/interop#INTEROP_STREAM` carrier it consumes is itself still open, so the streaming carrier seam lands only after INTEROP_STREAM.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

[TENSOR_SPLIT]-[COMPLETE]: split the over-stuffed TensorStore into cohesive `gridded/store.md` (dense zarr+cubed-plan+tensorstore, `TensorBackend` 5->2), `gridded/virtual.md` (icechunk `set_virtual_ref` + virtualizarr manifest), and `gridded/ragged.md` (awkward) owners; DOMAIN_MAP + SEAMS retargeted.
[FIELD_VIRTUAL]-[COMPLETE]: folded the virtualizarr virtual cube into `gridded/field.md#VIRTUAL` `FieldVirtual` as a CF-native lazy cube with the h5py `VirtualLayout`/`build_virtual_dataset` HDF5-native leg and `CFDtype` special-types seam; catalog `fold_virtual_cube` re-targets field.
[TENSORSTORE_ADMIT]-[COMPLETE]: admitted tensorstore as the `<3.15`-companion async zarr-v3 `TensorBackend.TENSORSTORE` row on `store.md#STORE` opening the identical Zarr v3 chunk grid through `tensorstore.open` over a native `KvStore`, with `oindex`/`vindex` read-selection and `Transaction(atomic=True)` multi-region writes.
[CUBED_LINALG_DEEPEN]-[COMPLETE]: surfaced the cubed out-of-core linalg (matmul/svd/qr/svdvals/tensordot/...), `map_blocks`/`apply_gufunc`/`rechunk` on `store.md#PLAN` `PlanOp`, plus the `allowed_mem`/peak-memory `PlanReceipt` off the `cubed.Callback`/`TaskEndEvent` lifecycle.
[AWKWARD_ARROW_BRIDGE]-[COMPLETE]: realized the full `gridded/ragged.md#RAGGED` awkward owner — `from_arrow`/`to_arrow` zero-copy to the interop carrier, `ArrayBuilder`/`from_buffers`/`to_layout` form algebra, the one `_FOLD` closure table over 17 reducers/paired/order members, and the `RaggedSink` egress receipt.
[VIRTUALIZARR_MF_PARSERS]-[COMPLETE]: replaced the hand-looped virtual cube with `open_virtual_mfdataset` and the 8-row `VirtualParser` suffix-recovered parser axis (HDF/NetCDF3/Zarr/DMRPP/FITS/kerchunk-json/kerchunk-parquet/icechunk) on `gridded/field.md#VIRTUAL`.
[FLOX_ADMIT]-[COMPLETE]: routed `field.md#SELECT` grouped/binned/resampled reductions through `flox.xarray.xarray_reduce` under the `policy.vectorizable` probe with the `method`/`engine` policy fields, the `xarray.groupers.TimeResampler` resample grouper, and the comprehension-derived `_REDUCE_BASE`/`_FALLBACK_CALL` bare-xarray fallback.
[CONTRACT_GATE_FOLD]-[COMPLETE]: rebuilt `contract.md#QUALITY` over `pandera.polars` (`CheckKind` collapsed onto `_CMP`/`_SET`/`_TEXT`/`_INCLUSIVE` tables, the recorded non-enforcing `ContractClaim`) with the dataframely covenant authority co-located in `[03]-[COLLECTION]`.
[CONTRACT_COLLECTION]-[COMPLETE]: added `contract.md#COLLECTION` `FrameCovenant` over `dataframely.Collection` — `RelationEdge`/`RelationCardinality`/`CovenantOp` (prove/consistent/restrict/extend/persist/sample), the Merkle-folded covenant `ContractClaim` composing columnar/catalog member content-keys.
[PROFILE_POINTBLANK_GRADE]-[COMPLETE]: added the new `tabular/profile.md` `QualityProfile` owner over `pointblank.Validate`/`Thresholds`/`Actions` — the one `ProbeStep` plan axis, the `Grade` severity sweep, the `ProfileReport` GT/wire axis emitting the `[SHAPE]` frame to artifacts, and the plan-content-keyed `ProfileReceipt`.
[CORPUS_INGEST]-[COMPLETE]: added the `columnar.md#SCAN` `Corpus` arm decoding the artifacts `to_corpus_row` flat record into a `pa.Table` through `from_pylist`, riding the agnostic interop carrier — the data-side endpoint of the `tabular <- artifacts/documents [WIRE]` seam.
[COLUMNAR_IOSOURCE_SCAN]-[COMPLETE]: collapsed the eager rows into the `ScanPlan.IoSource` `register_io_source` pushdown plane, routed geometry/HDF reads to their real owners, and dropped RHINO_3DM/MESH/HDF from `DatasetKind`.
[REMOTE_PARTITION_DEEPEN]-[COMPLETE]: deepened the `query.md#QUERY` `Remote` arm into the `RemoteOp` read/stream/ingest/probe/partition sub-axis over the `RemoteDriver` table (ADBC cp315-core default, connectorx `<3.15` partitioned fast-path, Flight SQL), the `Transport` option owner, and the `execute_partitions`/`partition_sql` fan-out.
[QUERY_IR_AND_SQLGATE]-[COMPLETE]: deepened the `query.md#QUERY` `Ir` arm (`to_sql`/`parse_sql`/`to_pyarrow_batches`) and added the `SqlGate` `sqlglot` parse/qualify/optimize/transpile gate on the `Sql` arm, resolving `[IBIS_BACKEND_NAMESPACE]`.
[LAKEHOUSE_MAINTENANCE_REJECT]-[COMPLETE]: fixed the stale reject-law — Iceberg `Vacuum`->`expire_snapshots`, Lance `Optimize`/`Vacuum`->`compact_files`/`cleanup_old_versions`, and shrank `_PORTABLE` so only `changefeed`/`restore` stay Delta-exclusive on the real 11-tag `LakeOp` union.
[LAKEHOUSE_OPS_DEEPEN]-[COMPLETE]: deepened `lakehouse.md#LAKEHOUSE` with `LakeOp.Delete`/`Update`/`Evolve` cases and the `WriteTuning` writer-properties policy on `Write`, each one case absorbed by every provider arm with per-provider reject parity.
[REMOTE_POINTCLOUD_LASPY]-[COMPLETE]: rebound `mesh.md#POINTCLOUD` to native `laspy.copc.CopcReader`, collapsed PointBackend to one laspy owner, and deleted data-side `_backend_of`/`PointBounds.as_pdal`/`[PDAL_COPC]`; pdal stays the geometry owner.
[MESH_DATA_PRESERVE]-[COMPLETE]: deepened `mesh.md#MESH` `MeshPayload` with type-keyed `point_data`/`cell_data`/`field_data` `ArrayRef` carriers, the `xdmf.TimeSeriesReader` FE-result rail, and the `rhino3dm` `.3dm` `MeshBackend` row resolving the 0-ref admission.
[EGRESS_ASYNC_RANGE]-[COMPLETE]: deepened `egress.md#EGRESS` `StoreOp` with the `GetRanges(coalesce)` coalesced multi-range fast-path, the `_async` sibling legs under one `_ROUTE` table, and the typed `obstore.exceptions.*` taxonomy rail translation.
[CATALOG_SIGNING_DISCOVERY]-[COMPLETE]: bound `catalog.md#CATALOG` planetary-computer signing as the `Signing`/`SignScheme` `Client.open(modifier=)` + `odc.stac.load(patch_url=)` row, and added `Intersects`/`Order`/`FreeText` `StacQuery` modalities routing the `Surface` discriminant to `collection_search`.
[CATALOG_RASTER_STREAM_VRT]-[COMPLETE]: bound the rasterio streaming/remote surface on `geospatial.md#GEO` `RasterOp` — `Stream`(block_windows), `Sample`, `Vrt`(WarpedVRT), `RemoteRead`(/vsicurl/+Env), `MemorySource`(MemoryFile), `WriteCog`(default_gtiff_profile) — plus the `stac-geoparquet` NDJSON/Delta `StacIngest` source-sink rows.
[GRID_DGGS]-[COMPLETE]: added the `geospatial.md#GRID` `GridSystem` discrete-global-grid owner over the H3/S2 `GridScheme` axis as vectorized polars-native cell ops over h3ronpy Arrow + polars-st, the `CellKind` cell/vertex/edge collapse and the bidirectional raster<->cell bridge, composing the DuckDB `h3` SQL path.
