# [PY_DATA]

`data` is the Python branch's host-free interchange plane: every AEC dataset — BIM quantities, scan point clouds, simulation tensors, carbon declarations — crosses as typed, content-keyed, Arrow-carried claims. One columnar, relational, and lakehouse tabular core carries the `spatial`, `gridded`, `graph`, and `impact` planes; the bar is interchange trust — a frame that leaves is self-describing, so a consumer decodes by name and never re-derives attribution. Compute schedules studies over its frames, Persistence federates its content keys and Substrait plans, artifacts renders its profiles.

It consumes runtime `ContentIdentity`, `ReceiptContributor`, and `TransportResource` at the boundary and re-mints none, meeting C# only at the content-identity wire and the companion seams.

## [01]-[ROUTER]

[TABULAR]:
- [01]-[INTEROP](.planning/tabular/interop.md): Backend-agnostic frame translation over `narwhals`; pyarrow-free Arrow C Data Interface carrier.
- [02]-[COLUMNAR](.planning/tabular/columnar.md): Dataset-ref owner and the one request-scoped `DuckDbSession` scan rail behind columnar egress.
- [03]-[LAKEHOUSE](.planning/tabular/lakehouse.md): Transactional lakehouse crossing one `LakeOp` axis over the Delta/Iceberg/Lance/DuckLake formats.
- [04]-[QUERY](.planning/tabular/query.md): Relational engine folding every `QuerySpec` frontend to uniform Arrow with column-provenance lineage.
- [05]-[MATERIALIZE](.planning/tabular/materialize.md): Incremental CDC materialization composing lakehouse, query, and columnar downward.
- [06]-[CONTRACT](.planning/tabular/contract.md): Data-contract gate folding dataframely covenants and pandera rules onto one `ContractClaim`.
- [07]-[PROFILE](.planning/tabular/profile.md): Graded data-quality plane over `pointblank` thresholds emitting the `QualityProfile` frame.
- [08]-[EGRESS](.planning/tabular/egress.md): Native object-store egress façade over `obstore` keyed by content identity.
- [09]-[COST](.planning/tabular/cost.md): Cost ledger folding the receipt families into one content-keyed, tenant-attributed priced frame.

[SPATIAL]:
- [10]-[GEOSPATIAL](.planning/spatial/geospatial.md): Vector and raster geo claims over the `VectorOp`/`RasterOp` axes with native-GeoArrow egress.
- [11]-[SPATIAL_QUERY](.planning/spatial/query.md): DuckDB-spatial join, transform, and H3-SQL engine on the shared `DuckDbSession` rail.
- [12]-[GRID](.planning/spatial/grid.md): Discrete-global-grid plane over `h3ronpy` vectorized cell algebra with the raster-cell bridge.
- [13]-[CATALOG](.planning/spatial/catalog.md): Cloud-native STAC discovery over `pystac-client` folding asset hrefs into object-store egress.
- [14]-[MESH](.planning/spatial/mesh.md): Mesh-file identity and topology owner with the LAS/LAZ/COPC point-cloud interchange row.

[GRIDDED]:
- [15]-[STORE](.planning/gridded/store.md): Dense chunked N-D tensor store over a `TensorBackend` axis with codec and region axes.
- [16]-[VIRTUAL](.planning/gridded/virtual.md): Sole manifest-cube owner over `icechunk` virtual-chunk addressing and the per-variable manifest wire.
- [17]-[RAGGED](.planning/gridded/ragged.md): Ragged N-D store over `awkward` with the zero-copy Arrow bridge to the interop carrier.
- [18]-[FIELD](.planning/gridded/field.md): CF field-dataset owner over `xarray` engines with flox grouped and resampled reductions.

[GRAPH]:
- [19]-[GRAPH](.planning/graph/graph.md): Graph-payload owner over the `rustworkx` run kernel with the GPL-confined community-detection split.

[IMPACT]:
- [20]-[IMPACT](.planning/impact/impact.md): Material environmental-impact owner normalizing EPD and LCA results onto one EN 15804 carrier.

## [02]-[DOMAIN_PACKAGES]

Data-domain libraries admitted by this folder; versions centralize in the one branch manifest, and the adjacent `.api/` folder holds the API evidence.

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
- `connectorx`
- `adbc-driver-manager`
- `adbc-driver-flightsql`
- `adbc-driver-postgresql`
- `adbc-driver-snowflake`
- `adbc-driver-sqlite`
- `obspec-utils` — multi-store object-store routing.

DuckDB loadable extensions back the plan and table-format rows without a pip dependency, all riding the one `DuckDbSession` rail: `substrait`, `ducklake`, `iceberg`, `httpfs`, and `spatial`/`h3`, provisioned through the Forge DuckDB-extensions catalog.

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
- `tensorstore`
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

[NUMERIC_SUBSTRATE]:
- `numpy`
- `xarray`

[GRAPH_SUBSTRATE]:
- `networkx`

[MESH_INTERCHANGE]:
- `meshio`

[OBSERVABILITY]:
- `opentelemetry-api` — one module tracer per measured leg across every plane; the SDK, exporters, and instrumentor train stay runtime-owned.

[TRANSPORT]:
- `fsspec` — Filesystem-resolution substrate beneath `universal-pathlib`; the `UPath.fs` handle threads into the DuckDB scan session.
- `obstore` — Object-store substrate beneath the content-keyed egress owner.
