# [PY_DATA]

`data` is the Python branch's host-free interchange plane — every AEC dataset crosses as a typed, content-keyed, Arrow-carried claim, graded on interchange trust: a frame that leaves is self-describing, so a consumer decodes by name and never re-derives attribution. It composes the runtime `ContentIdentity`, `ReceiptContributor`, and `TransportResource` owners at the boundary and re-mints none, and peer branches meet it only through contract-conforming datasets and plans.

## [01]-[ROUTER]

[TABULAR]:
- [01]-[INTEROP](.planning/tabular/interop.md): Backend-agnostic frame translation over `narwhals`; pyarrow-free Arrow C Data Interface carrier.
- [02]-[COLUMNAR](.planning/tabular/columnar.md): Dataset-ref owner and the one request-scoped `DuckDbSession` scan rail behind columnar egress.
- [03]-[LAKEHOUSE](.planning/tabular/lakehouse.md): Transactional lakehouse crossing one `LakeOp` axis over the Delta/Iceberg/Lance/DuckLake formats.
- [04]-[QUERY](.planning/tabular/query.md): Relational engine folding every `QuerySpec` frontend to uniform Arrow with column-provenance lineage.
- [05]-[MATERIALIZE](.planning/tabular/materialize.md): Incremental CDC materialization composing lakehouse, query, and columnar downward.
- [06]-[CONTRACT](.planning/tabular/contract.md): Data-contract gate folding dataframely covenants and pandera rules onto one `ContractClaim`.
- [07]-[PROFILE](.planning/tabular/profile.md): Graded data-quality plane over `pointblank` thresholds emitting the `QualityProfile` frame.
- [08]-[EGRESS](.planning/tabular/egress.md): Native object-store egress receipt owner over the runtime store lane, keyed by content identity.
- [09]-[COST](.planning/tabular/cost.md): Cost ledger folding the receipt families into one content-keyed, tenant-attributed priced frame.
- [10]-[JOURNAL](.planning/tabular/journal.md): `Ledger` implementer landing runtime audit and meter facts over the commit matrix and the scan reader.

[SPATIAL]:
- [11]-[GEOSPATIAL](.planning/spatial/geospatial.md): Vector and raster geo claims over the `VectorOp`/`RasterOp` axes with native-GeoArrow egress.
- [12]-[SPATIAL_QUERY](.planning/spatial/query.md): DuckDB-spatial join, transform, and H3-SQL engine on the shared `DuckDbSession` rail.
- [13]-[GRID](.planning/spatial/grid.md): Discrete-global-grid plane over `h3ronpy` vectorized cell algebra with the raster-cell bridge.
- [14]-[CATALOG](.planning/spatial/catalog.md): Cloud-native STAC discovery over `pystac-client` folding asset hrefs into object-store egress.
- [15]-[MESH](.planning/spatial/mesh.md): Mesh-file identity and topology owner with the LAS/LAZ/COPC point-cloud interchange row.

[GRIDDED]:
- [16]-[STORE](.planning/gridded/store.md): Dense chunked N-D tensor store over a `TensorBackend` axis with codec and region axes.
- [17]-[VIRTUAL](.planning/gridded/virtual.md): Sole manifest-cube owner over `icechunk` virtual-chunk addressing and the per-variable manifest wire.
- [18]-[RAGGED](.planning/gridded/ragged.md): Ragged N-D store over `awkward` with the zero-copy Arrow bridge to the interop carrier.
- [19]-[FIELD](.planning/gridded/field.md): CF field-dataset owner over `xarray` engines with flox grouped and resampled reductions.

[GRAPH]:
- [20]-[GRAPH](.planning/graph/graph.md): Graph-payload owner over the `rustworkx` run kernel with the GPL-confined community-detection split.

[IMPACT]:
- [21]-[IMPACT](.planning/impact/impact.md): Material environmental-impact owner normalizing EPD and LCA results onto one EN 15804 carrier.

## [02]-[DOMAIN_PACKAGES]

Domain-specific libraries admitted by this folder; versions centralize in the root `pyproject.toml` and corroborate against this folder's `.api/`. FLOOR-GATED marks a row whose `python_version` marker in that manifest no supported interpreter satisfies, so its module resolves nowhere and its owning page refuses every selection naming it through an import-time `find_spec` row — admission stands, reach does not, and the mark keeps a gated row from reading as a live provider.

[FRAMES]:
- `polars`
- `polars-st`
- `narwhals`
- `pyarrow`
- `arro3-core`
- `arro3-compute`
- `arro3-io` — pyarrow-free Arrow codec and object-store transport.
- `nanoarrow`
- `fastexcel`
- `dataframely`
- `pointblank`
- `pandera`
- `pandas` — boundary-only external frame lowering.

[LAKEHOUSE_QUERY]:
- `deltalake`
- `pyiceberg`
- `pylance`
- `daft`
- `duckdb`
- `ibis-framework`
- `sqlglot`
- `datafusion`
- `substrait` — typed plan admission IR.
- `connectorx` — FLOOR-GATED; `tabular/query#QUERY` refuses every `RemoteDriver.CONNECTORX` spec while the marker holds.
- `adbc-driver-manager`
- `adbc-driver-flightsql`
- `adbc-driver-postgresql`
- `adbc-driver-snowflake`
- `adbc-driver-sqlite`
- `obspec-utils` — multi-store object-store routing.

[DUCKDB_EXTENSIONS]: Loadable extensions backing the plan and table-format rows without a pip dependency, riding the one `DuckDbSession` rail and provisioned through the Forge DuckDB-extensions catalog.
- `substrait`
- `ducklake`
- `iceberg`
- `httpfs`
- `spatial`
- `h3`
- `aws` — `credential_chain` provider over the s3, gcs, and r2 secret types `httpfs` registers.
- `azure` — azure blob protocol carrying its own `credential_chain` secret type.
- `postgres_scanner` — operational-store attach joining evidence rows against live PostgreSQL in one statement.
- `delta` — `delta_scan` transaction-log reader binding the analytics evidence residence to the interactive query arm.

[GEOSPATIAL]:
- `geopandas`
- `shapely`
- `pyproj`
- `pyogrio`
- `rasterio`
- `rioxarray`
- `geoarrow-pyarrow` — pyarrow-native GeoArrow interop.
- `geoarrow-rust-compute`
- `geoarrow-rust-core` — GeoArrow-native geometry memory.
- `geoarrow-rust-io` — GDAL-free geospatial file and object-store transport.
- `h3ronpy`
- `xarray-spatial`
- `xvec` — geometry-indexed xarray data cubes.
- `pystac`
- `pystac-client`
- `stac-geoparquet`
- `odc-stac`
- `planetary-computer`

[GRIDDED]:
- `zarr`
- `numcodecs` — zarr v3 chunk filter/compressor codec registry.
- `cubed`
- `tensorstore` — FLOOR-GATED; `gridded/store#STORE` refuses every caller-NAMED `TensorBackend.TENSORSTORE` selection while the marker holds, and an unnamed object-store residence derives the sync engine over `zarr.storage.ObjectStore` instead, so the marker gates one engine rather than the whole remote dense-tensor plane.
- `awkward`
- `flox`
- `icechunk`
- `virtualizarr`
- `h5py`
- `netcdf4`
- `h5netcdf` — pure-h5py netCDF-4 engine backing `FieldEngine.H5NETCDF`.

[GRAPH_MESH]:
- `rustworkx`
- `igraph`
- `trimesh`
- `rhino3dm`
- `laspy` — LAS/LAZ/COPC point-cloud interchange owner.
- `lazrs`
- `laszip`

[IMPACT]:
- `openepd` — OpenEPD/EC3 typed declaration model, EC3 sync client, and offline bundle IO.
- `epdx` — ILCD+EPD to EPDx common-format conversion.
- `bw2data` — Brightway project and node/edge graph store, the system of record.
- `bw2calc` — Brightway LCA solver over sparse matrix assembly and score.
- `bw2io` — Brightway LCI/LCIA import/export and the ecoinvent/EEIO bootstrap against the `bw2data` project.
- `bw2analyzer` — Brightway contribution and comparison analysis on the solve leg.
- `bw-processing` — Brightway matrix-datapackage substrate over COO triples.
- `olca-ipc` — live openLCA IPC/REST client, carrying `olca-schema` as its wire model.
- `premise` — prospective ecoinvent background-database transformer over IAM scenarios.

## [03]-[SUBSTRATE_PACKAGES]

Shared substrate consumed from the Py registry; the registry and its charters own the full contracts, and `libs/python/.api/` holds the shared API evidence.

[TYPING_RAILS]:
- `expression`
- `msgspec`
- `beartype`
- `pydantic`

[CONCURRENCY]:
- `anyio`

[OBSERVABILITY]:
- `opentelemetry-api` — one module tracer per measured leg across every plane.

[NUMERIC_SUBSTRATE]:
- `numpy`
- `xarray`

[GRAPH_SUBSTRATE]:
- `networkx`

[TRANSPORT]:
- `fsspec` — Filesystem-resolution substrate beneath `universal-pathlib`; the `UPath.fs` handle threads into the DuckDB scan session.
- `obstore` — Object-store substrate reached through the runtime store lane; this branch names its credential providers alone (`spatial/catalog`).
- `protobuf` — Wire codec beneath the Substrait plan model; `tabular/query#QUERY` reads its decode fault as the inbound-plan refusal.

[MESH_INTERCHANGE]:
- `meshio`
