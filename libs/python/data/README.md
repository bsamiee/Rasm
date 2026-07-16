# [PY_DATA]

`data` is the host-free data-interchange plane of the Python branch: every AEC dataset the platform touches — BIM quantity tables, scan point clouds, simulation tensors, geospatial context, carbon declarations — crosses through it as typed, content-keyed, Arrow-carried claims. One columnar, relational, and lakehouse tabular core carries the `spatial`, `gridded`, `graph`, and `impact` planes, and the bar is interchange trust: an admitted package is mined to fence depth, a claimed provider arm is a realized fold, and a frame that leaves is self-describing — source, unit, identifier, and content key ride the columns, so a consumer decodes by name and never re-derives attribution. Compute schedules studies over its frames, C# Persistence federates its content keys and Substrait plans, and artifacts renders its profiles and quality reports.

It consumes runtime `ContentIdentity`, `ReceiptContributor`, and `TransportResource` at the boundary and re-mints none, meeting C# only at the content-identity wire and the companion seams.

## [01]-[ROUTER]

[TABULAR]:
- [01]-[INTEROP](.planning/tabular/interop.md): Backend-agnostic frame translation over `narwhals` with a pyarrow-free Arrow C Data Interface carrier.
- [02]-[COLUMNAR](.planning/tabular/columnar.md): Dataset-ref owner and the one request-scoped `DuckDbSession` scan rail behind columnar egress.
- [03]-[LAKEHOUSE](.planning/tabular/lakehouse.md): Transactional lakehouse crossing one `LakeOp` axis over the Delta/Iceberg/Lance/DuckLake formats.
- [04]-[QUERY](.planning/tabular/query.md): Relational engine folding every `QuerySpec` frontend to uniform Arrow with column-provenance lineage.
- [05]-[MATERIALIZE](.planning/tabular/materialize.md): Incremental CDC materialization composing lakehouse, query, and columnar downward.
- [06]-[CONTRACT](.planning/tabular/contract.md): Data-contract gate folding dataframely covenants and pandera rules onto one `ContractClaim`.
- [07]-[PROFILE](.planning/tabular/profile.md): Graded data-quality plane over `pointblank` thresholds emitting the `QualityProfile` frame.
- [08]-[EGRESS](.planning/tabular/egress.md): Native object-store egress façade over `obstore` keyed by content identity.

[SPATIAL]:
- [09]-[GEOSPATIAL](.planning/spatial/geospatial.md): Vector and raster geo claims over the `VectorOp`/`RasterOp` axes with native-GeoArrow egress.
- [10]-[SPATIAL_QUERY](.planning/spatial/query.md): DuckDB-spatial join, transform, and H3-SQL engine on the shared `DuckDbSession` rail.
- [11]-[GRID](.planning/spatial/grid.md): Discrete-global-grid plane over `h3ronpy` vectorized cell algebra with the raster-cell bridge.
- [12]-[CATALOG](.planning/spatial/catalog.md): Cloud-native STAC discovery over `pystac-client` folding asset hrefs into object-store egress.
- [13]-[MESH](.planning/spatial/mesh.md): Mesh-file identity and topology owner with the LAS/LAZ/COPC point-cloud interchange row.

[GRIDDED]:
- [14]-[STORE](.planning/gridded/store.md): Dense chunked N-D tensor store over a `TensorBackend` axis with codec and region axes.
- [15]-[VIRTUAL](.planning/gridded/virtual.md): Sole manifest-cube owner over `icechunk` virtual-chunk addressing and the per-variable manifest wire.
- [16]-[RAGGED](.planning/gridded/ragged.md): Ragged N-D store over `awkward` with the zero-copy Arrow bridge to the interop carrier.
- [17]-[FIELD](.planning/gridded/field.md): CF field-dataset owner over `xarray` engines with flox grouped and resampled reductions.

[GRAPH]:
- [18]-[GRAPH](.planning/graph/graph.md): Graph-payload owner over the `rustworkx` run kernel with the GPL-confined community-detection split.

[IMPACT]:
- [19]-[IMPACT](.planning/impact/impact.md): Material environmental-impact owner normalizing EPD and LCA results onto one EN 15804 carrier.

## [02]-[DOMAIN_PACKAGES]

Data-domain libraries admitted by this folder; versions centralize in the one branch manifest, and the adjacent `.api/` folder holds the API evidence.

[FRAMES]:
- `polars`
- `polars-st`
- `narwhals`
- `pyarrow`
- `arro3-core`
- `nanoarrow`
- `fastexcel`
- `dataframely`
- `pointblank`
- `pandera`
- `pandas` — Boundary lowering only: external frames arrive at the wire, `read_fwf` the fixed-width decode; no internal owner constructs one.

[LAKEHOUSE_QUERY]:
- `deltalake`
- `pyiceberg`
- `pylance`
- `daft`
- `duckdb`
- `ibis-framework`
- `sqlglot`
- `datafusion`
- `connectorx`
- `adbc-driver-manager`
- `adbc-driver-flightsql`
- `obspec-utils` — multi-store `ObjectStoreRegistry` router companion to `obstore`.

DuckDB loadable extensions back the plan and table-format rows without a pip dependency, all riding the one `DuckDbSession` rail: `substrait`, `ducklake`, `iceberg`, `httpfs`, and `spatial`/`h3`, provisioned through the Forge DuckDB-extensions catalog.

[GEOSPATIAL]:
- `geopandas`
- `shapely`
- `pyproj`
- `pyogrio`
- `rasterio`
- `rioxarray`
- `geoarrow-rust-compute`
- `h3ronpy`
- `xarray-spatial`
- `pystac`
- `pystac-client`
- `stac-geoparquet`
- `odc-stac`
- `planetary-computer`

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

[GRAPH_MESH]:
- `networkx`
- `rustworkx`
- `igraph`
- `trimesh`
- `rhino3dm`
- `lazrs`
- `laszip`
- `pdal` — Geometry-side point-cloud filter-graph owner; the data COPC arm rebinds to `laspy.copc` without removing it.

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

Shared Python substrate consumed from the branch registry; `libs/python/.planning/README.md` and its charters own the full contracts, and `libs/python/.api/` holds the shared API evidence.

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
- `fsspec` — Filesystem-resolution substrate beneath `universal-pathlib`; the `UPath.fs` handle threads into the DuckDB scan session.
- `obstore` — Object-store substrate for content-keyed egress, conditional mutation, Arrow listing, credentials, retry, and fsspec adaptation.
