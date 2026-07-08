# [PY_DATA]

`data` is the host-free data-interchange companion of the Python branch. It owns typed dataset refs, columnar lazy/streaming scan and egress, the transactional table-format lakehouse, cross-engine relational query, a data-contract gate, dataframe-agnostic interop with a pyarrow-free Arrow carrier, vector and raster geospatial, graph payloads, chunked tensor stores, mesh-file exchange, and a material environmental-impact (EPD/LCA) normalizer. It consumes runtime `ContentIdentity`, `ReceiptContributor`, and `TransportResource` at the boundary and never re-mints them, integrating with C# only at the wire (content-identity plus GLB) and the companion/offline seams. `ARCHITECTURE.md` carries the domain map, `IDEAS.md` the forward pool, and `TASKLOG.md` the open work.

## [01]-[ROUTER]

- [01]-[INTEROP](.planning/tabular/interop.md): Backend-agnostic frame translation over narwhals — the seven-row eager/lazy `Backend` axis (POLARS/PANDAS/PYARROW/MODIN eager, DUCKDB/IBIS/DASK lazy), the locally-declared `FieldShape` minter, and the pyarrow-free Arrow C Data Interface carrier with public `ArrowCStream.of` construction, chunked `c_array_stream` consumption, schema-only negotiation, and the `nanoarrow.device` C Device Data Interface row.
- [02]-[COLUMNAR](.planning/tabular/columnar.md): Dataset-ref owner discriminating by source shape; the ONE request-scoped `DuckDbSession` rail with `DuckDbExtension` policy rows (httpfs/spatial/h3/substrait/iceberg/ducklake); cross-engine scan plans (the `DuckDb` arm source-scoped SQL-over-ref), typed columnar egress, the public `arrow_bytes` canonical-bytes fold, and the content-keyed query receipt.
- [03]-[LAKEHOUSE](.planning/tabular/lakehouse.md): Transactional table-format lakehouse over one `LakeOp` operation axis crossed with one `TableFormat` provider axis (Delta/Iceberg/Lance/DuckLake) — the DuckDB `iceberg`-extension read promote with the pyiceberg `<3.15` catalog-write fallback, the DuckLake `ATTACH` arms over the session rail, and the mined Delta governance rows.
- [04]-[QUERY](.planning/tabular/query.md): Relational query engine over one `QuerySpec` axis (DuckDB, narwhals, Ibis IR, ADBC/ConnectorX/Flight SQL remote transport, daft elasticity, DuckDB-Substrait plan portability, and the `datafusion` `Federated` case over BOTH Substrait codec directions with plan-keyed receipts) to uniform Arrow, with the realized end-to-end `lineage_edges` column-provenance projection.
- [05]-[MATERIALIZE](.planning/tabular/materialize.md): `DerivedSnapshot`/`PartitionBundle` incremental CDC-materialization composing lakehouse + query + the columnar `arrow_bytes` fold downward — partition-delta recompute, Merkle snapshot key, anyio task-group concurrency.
- [06]-[CONTRACT](.planning/tabular/contract.md): Data-contract gate over dataframely referential covenants and pandera column rules folded on the one `ContractClaim`; `FieldShape` imported downward from its interop minter in one module-top prelude.
- [07]-[PROFILE](.planning/tabular/profile.md): Graded data-quality plane over `pointblank` Thresholds, keyed by `ContentIdentity`, emitting a `QualityProfile` frame the artifacts great-tables renderer renders.
- [08]-[EGRESS](.planning/tabular/egress.md): Native object-store egress façade over `obstore` composing the runtime transport, keyed by `ContentIdentity`.
- [09]-[GEOSPATIAL](.planning/spatial/geospatial.md): The geospatial CLAIMS plane — vector/raster claims (`RasterGeoClaim` carries the affine `transform`), `VectorOp`/`RasterOp` axes plus the `Linear`/`Geodesic` families, the `VectorIngress` OGR pushdown row, native-GeoArrow egress with the `geoarrow_wire` Compute-GLB hand-off, and the rioxarray `COVERAGE` CF bridge with the `.rio.to_raster(driver="COG")` write.
- [10]-[SPATIAL_QUERY](.planning/spatial/query.md): The DuckDB-spatial join/transform/H3-SQL columnar engine over the shared `DuckDbSession` rail — the `ST_GeomFromWKB` geometry-view prelude, the one `QueryPlan` projection, the bbox-cached `SPATIAL_JOIN` prefilter; the in-DB half of the two-H3-substrate law.
- [11]-[GRID](.planning/spatial/grid.md): The `GridSystem` discrete-global-grid plane — h3ronpy vectorized cell algebra over Arrow `u64` columns, the `CellKind` collapse, the bidirectional raster↔cell bridge, the polars-st `GeoLift`/`GeoFrameOp` frame-native geometry vocabulary, and `engine_bin` composing the in-DB binning leg; S2 the deferred numba-cp315 hold.
- [12]-[CATALOG](.planning/spatial/catalog.md): Cloud-native STAC item/collection discovery over `pystac-client`, the `stac-geoparquet` item table, the RE-HOMED `StacGeoClaim`/`StacIngest` interchange claim, and the asset-href fold into object-store egress, the gridded virtual cube, and the odc-stac coverage load.
- [13]-[MESH](.planning/spatial/mesh.md): Mesh-file identity, cell-block topology, units, GLB preview export, and the LAS/LAZ/COPC point-cloud interchange row.
- [14]-[STORE](.planning/gridded/store.md): Dense chunked N-D tensor store over a 2-row `TensorBackend` axis (`zarr` write, `cubed` plan, `tensorstore` async read) with codec and region axes; the numcodecs filter family binds the absorbed `zarr.codecs.numcodecs` home.
- [15]-[VIRTUAL](.planning/gridded/virtual.md): The SOLE manifest-cube owner — the absorbed `virtualizarr` manifest construction (`FieldVirtual`, the 8-row `VirtualParser`, `CFDtype`, the h5py native path) plus `icechunk` `set_virtual_ref` native virtual-chunk addressing, one `ManifestWrite` export/registration axis, the canonical per-variable manifest wire, and the Persistence snapshot-seed parity recorded on the runtime `ParityReceipt` rail.
- [16]-[RAGGED](.planning/gridded/ragged.md): Ragged N-D store owner over `awkward` with the `from_arrow`/`to_arrow` zero-copy bridge to the interop Arrow carrier.
- [17]-[FIELD](.planning/gridded/field.md): The CF owner ONLY — `FieldDataset` over `xarray` (netcdf4/h5netcdf/Zarr engines), CF-aware selection, `flox` grouped/resampled reductions, and the content-keyed egress; the virtual leg lives on [15]-[VIRTUAL].
- [18]-[GRAPH](.planning/graph/graph.md): Graph payloads over `rustworkx` — the realized ~40-arm `_run_rx` kernel (traversal/path/all-pairs/DAG/connectivity/cut/centrality/coloring/matching/spanning/structure/layout) with carried `WeightSelector` payloads, the GPL-confined `IG_COMMUNITY` Leiden/Louvain/Infomap split, the `networkx` codec/egress lane, typed algorithm receipts, and the node-keyed frame seam into tabular.
- [19]-[IMPACT](.planning/impact/impact.md): Material environmental-impact owner normalizing external EPD declarations (OpenEPD/EC3, ILCD+EPD) and computed LCA results (the staged Brightway solve with Monte Carlo spread and `bw2analyzer` contribution depth, the live openLCA lifecycle, prospective `premise` backgrounds) into one EN 15804 indicator × life-cycle-stage carrier with canonical unit rows, keyed by `ContentIdentity` and lowered to the eight-column self-describing assessment frame.

## [02]-[DOMAIN_PACKAGES]

Every data-domain library the folder uses, planned or implemented; versions are centralized in the one branch manifest. API evidence lives in the adjacent `.api/` folder.

[DATAFRAME]:
- `polars`
- `polars-st`
- `narwhals`
- `pandas` — Boundary lowering only: external producers hand pandas frames at the wire (`narwhals` owns backend-agnosticism), and `read_fwf` is the fixed-width foreign decode closing the struck-`csvkit` gap; no internal owner constructs a pandas frame.

[ARROW]:
- `pyarrow`
- `arro3-core`
- `nanoarrow`
- `fastexcel`

[LAKEHOUSE]:
- `deltalake`
- `pyiceberg`
- `pylance`
- `daft`

DuckDB loadable extensions back plan and table-format rows without a pip dependency, all riding the one `columnar.py` `DuckDbSession`/`DuckDbExtension` rail: `substrait` (the community extension wired on `query.py` producing the portable binary/JSON plan blob), `ducklake` (core-loadable — the `TableFormat.DUCKLAKE` `ATTACH 'ducklake:<dsn>'` arms on `lakehouse.py`), `iceberg` (core-loadable — the primary `iceberg_scan` read path on `lakehouse.py`), `httpfs` (the remote-glob scan), and `spatial`/`h3` (the `spatial/query.py` engine rows) — all provisioned through the Forge DuckDB-extensions catalog.

[QUERY]:
- `duckdb`
- `ibis-framework`
- `sqlglot`
- `datafusion`
- `connectorx`
- `adbc-driver-manager`
- `adbc-driver-flightsql`

[CONTRACT]:
- `dataframely`
- `pointblank`
- `pandera`

[GEOSPATIAL]:
- `geopandas`
- `shapely`
- `pyproj`
- `pyogrio`
- `rasterio`
- `rioxarray`
- `geoarrow-rust-compute`
- `polars-st`
- `h3ronpy`
- `xarray-spatial`

[STAC]:
- `pystac`
- `pystac-client`
- `stac-geoparquet`
- `odc-stac`
- `planetary-computer`

[GRAPH]:
- `networkx`
- `rustworkx`
- `igraph`

[GRIDDED]:
- `zarr`
- `numcodecs` — zarr v3 chunk filter/compressor codec registry; `Blosc`/`Zstd` own archival numeric chunk compression.
- `cubed`
- `tensorstore`
- `awkward`
- `xarray`
- `flox`
- `icechunk`
- `virtualizarr`
- `h5py`
- `netcdf4`
- `h5netcdf` — pure-h5py netCDF-4 engine backing `FieldEngine.H5NETCDF`; rejects the netCDF-C lossy-quantization keys.

[MESH_INTERCHANGE]:
- `trimesh`
- `rhino3dm`
- `lazrs`
- `laszip`
- `pdal` — Stays the geometry-side point-cloud filter-graph owner; the data COPC arm rebinds to `laspy.copc` without removing it.

[EPD_LCA]:
- `openepd` — OpenEPD/EC3 typed declaration model, EC3 sync client, and offline bundle IO.
- `epdx` — ILCD+EPD to EPDx common-format conversion.
- `bw2data` — Brightway project and node/edge graph store (system of record).
- `bw2calc` — Brightway LCA solver (sparse matrix assembly and score).
- `bw2io` — Brightway LCI/LCIA import/export and database ingestion; owns the `bw2setup` and ecoinvent/EEIO bootstrap against the current `bw2data` project.
- `bw2analyzer` — Brightway contribution/comparison analysis: `annotated_top_processes`/`annotated_top_emissions` depth on the solve leg.
- `bw-processing` — Brightway matrix-datapackage substrate (COO triples).
- `olca-ipc` — live openLCA IPC/REST client and result queries; carries `olca-schema` as its wire model.
- `premise` — prospective ecoinvent background-database transformer over IAM scenarios.

[OBJECT_STORE]:
- `obspec-utils` — multi-store `ObjectStoreRegistry` router companion to `obstore`.

## [03]-[SUBSTRATE_PACKAGES]

Cross-cutting Python substrate libraries this folder directly consumes; these are owned at the branch substrate layer. Package charters and API evidence live in `libs/python/.planning/README.md` and the adjacent `libs/python/.api/` folder.

[TYPING_RAILS]:
- `expression`
- `msgspec`
- `beartype`
- `pydantic`

[CONCURRENCY]:
- `anyio`

[NUMERIC_SUBSTRATE]:
- `numpy`

[MESH_INTERCHANGE]:
- `meshio`

[TRANSPORT]:
- `fsspec` — Filesystem-resolution substrate beneath `universal-pathlib`; the `UPath.fs` handle threads into the DuckDB scan session via `register_filesystem`.
- `obstore` — Object-store substrate for content-keyed egress, conditional mutation, Arrow listing, credentials, retry, and fsspec adaptation.
