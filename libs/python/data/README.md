# [PY_DATA]

`data` is the Python branch's host-free interchange plane, every AEC dataset crossing as a typed, content-keyed, Arrow-carried claim graded on interchange trust: a frame that leaves is self-describing, so a consumer decodes by name and never re-derives attribution. It composes the runtime `ContentIdentity` and `TransportResource` owners at the boundary, and peer branches meet it only through contract-conforming datasets and plans.

## [01]-[ROUTER]

[TABULAR]:
- [01]-[INTEROP](.planning/tabular/interop.md): Frame translation over any backend, the Arrow C Data carrier, the `DataLeg`/`DataHook` rosters.
- [02]-[COLUMNAR](.planning/tabular/columnar.md): Dataset-ref owner and the one request-scoped `DuckDbSession` scan rail behind columnar egress.
- [03]-[LAKEHOUSE](.planning/tabular/lakehouse.md): Transactional lakehouse over one `LakeOp` axis; capability demand, fence, `Generation` roster.
- [04]-[QUERY](.planning/tabular/query.md): Relational engine folding every `QuerySpec` frontend to uniform Arrow with column-provenance lineage.
- [05]-[MATERIALIZE](.planning/tabular/materialize.md): Incremental CDC materialization composing lakehouse, query, and columnar downward.
- [06]-[CONTRACT](.planning/tabular/contract.md): Data-contract gate folding dataframely covenants and pandera rules onto one `ContractClaim`.
- [07]-[PROFILE](.planning/tabular/profile.md): Graded data-quality plane over `pointblank` thresholds emitting the `QualityProfile` frame.
- [08]-[EGRESS](.planning/tabular/egress.md): Object-store egress over the runtime store lane with canonical operation results.
- [09]-[COST](.planning/tabular/cost.md): Cost ledger pricing canonical operation results and durable resource facts under one rate policy.
- [10]-[JOURNAL](.planning/tabular/journal.md): `FactJournal` Ledger-port implementation landing audit and meter facts over commit and scan.

[SPATIAL]:
- [11]-[GEOSPATIAL](.planning/spatial/geospatial.md): Vector and raster geo claims over the `VectorOp`/`RasterOp` axes with native-GeoArrow egress.
- [12]-[QUERY](.planning/spatial/query.md): DuckDB-spatial join, transform, and H3-SQL engine on the shared `DuckDbSession` rail.
- [13]-[GRID](.planning/spatial/grid.md): Discrete-global-grid plane over `h3ronpy` vectorized cell algebra with the raster-cell bridge.
- [14]-[CATALOG](.planning/spatial/catalog.md): Cloud-native STAC discovery over `pystac-client` folding asset hrefs into object-store egress.
- [15]-[MESH](.planning/spatial/mesh.md): Mesh-file identity and topology owner with the LAS/LAZ/COPC point-cloud interchange row.
- [16]-[CUBE](.planning/spatial/cube.md): Vector-data-cube owner over `xvec` geometry-indexed dimensions bridging field cubes and vector claims.

[GRIDDED]:
- [17]-[STORE](.planning/gridded/store.md): Dense chunked N-D tensor store over a `TensorBackend` axis with codec and region axes.
- [18]-[VIRTUAL](.planning/gridded/virtual.md): Sole manifest-cube owner over `icechunk` virtual-chunk addressing and the per-variable manifest wire.
- [19]-[RAGGED](.planning/gridded/ragged.md): `RaggedArray` variable-length nested-array owner over `awkward` with the zero-copy Arrow bridge.
- [20]-[FIELD](.planning/gridded/field.md): CF field-dataset owner over `xarray` engines — flox reductions, the raw read leg, the ensemble corpus.
- [21]-[ENSEMBLE](.planning/gridded/ensemble.md): Scenario-tree owner over `DataTree` hierarchies carrying multi-scenario families with group folds.

[GRAPH]:
- [22]-[GRAPH](.planning/graph/graph.md): Graph-payload owner over `rustworkx`, the GPL-confined community split, and the layer-topology decoder.
- [23]-[NETWORK](.planning/graph/network.md): Capacity-network flow owner over the `networkx` flow family the rustworkx kernel does not spell.

[IMPACT]:
- [24]-[IMPACT](.planning/impact/impact.md): Material environmental-impact owner normalizing EPD and LCA results onto one EN 15804 carrier.
- [25]-[DECLARATION](.planning/impact/declaration.md): Registry-ingest owner minting the `declaration-record` contract per verified declaration.
- [26]-[INVENTORY](.planning/impact/inventory.md): Brightway project and LCI-ingestion custodian with the matrix-datapackage substrate.
- [27]-[SOLVE](.planning/impact/solve.md): `MultiLCA` shared-factorization batch and the contribution driver-mining axis.
- [28]-[SCENARIO](.planning/impact/scenario.md): Prospective-background owner driving the floor-gated `premise` transform and its write-back proof.

## [02]-[DOMAIN_PACKAGES]

Domain-specific libraries admitted by this folder; admission rows ride the workspace manifests as bare names, `uv.lock` fixes every version, and this folder's `.api/` corroborates.

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
- `pandas` — Boundary-only external frame lowering.

[LAKEHOUSE_QUERY]:
- `deltalake`
- `pyiceberg`
- `pylance`
- `daft`
- `duckdb`
- `ibis-framework`
- `sqlglot`
- `datafusion`
- `substrait` — Typed plan admission IR.
- `connectorx` — FLOOR-GATED; `tabular/query#QUERY` refuses every `RemoteDriver.CONNECTORX` spec while the marker holds.
- `adbc-driver-manager`
- `adbc-driver-flightsql`
- `adbc-driver-postgresql`
- `adbc-driver-snowflake`
- `adbc-driver-sqlite`
- `obspec-utils` — Multi-store object-store routing.

[DUCKDB_EXTENSIONS]: Loadable extensions backing the plan and table-format rows without a pip dependency, riding the one `DuckDbSession` rail and provisioned through the Forge DuckDB-extensions catalog.
- `substrait`
- `ducklake`
- `iceberg`
- `httpfs`
- `spatial`
- `h3`
- `aws` — `credential_chain` provider over the s3, gcs, and r2 secret types `httpfs` registers.
- `azure` — Azure blob protocol carrying its own `credential_chain` secret type.
- `postgres_scanner` — Operational-store attach joining evidence rows against live PostgreSQL in one statement.
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
- `xvec` — Geometry-indexed xarray data cubes.
- `pystac`
- `pystac-client`
- `stac-geoparquet`
- `odc-stac`
- `planetary-computer`

[GRIDDED]:
- `zarr`
- `numcodecs` — zarr v3 chunk filter/compressor codec registry.
- `cubed`
- `tensorstore` — FLOOR-GATED; `gridded/store#STORE` refuses a NAMED `TensorBackend.TENSORSTORE`; unnamed residences ride `zarr.storage.ObjectStore`.
- `awkward`
- `flox`
- `icechunk`
- `virtualizarr`
- `h5py`
- `netcdf4`
- `h5netcdf` — Pure-h5py netCDF-4 engine backing `FieldEngine.H5NETCDF`.

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
- `olca-ipc` — Live openLCA IPC/REST client, carrying `olca-schema` as its wire model.
- `premise` — FLOOR-GATED ecoinvent prospective-background transformer over IAM scenarios; gated, it refuses every build.

## [03]-[SUBSTRATE_PACKAGES]

Shared substrate consumed from the Python registry, whose charters own the full contracts; `libs/python/.api/` holds the shared API evidence.

[TYPING_RAILS]:
- `expression`
- `msgspec`
- `beartype`
- `pydantic`

[CONCURRENCY]:
- `anyio`

[OBSERVABILITY]:
- `opentelemetry-api` — One module tracer per measured leg across every plane.

[NUMERIC_SUBSTRATE]:
- `numpy`
- `xarray`

[GRAPH_SUBSTRATE]:
- `networkx`

[TRANSPORT]:
- `fsspec` — Filesystem-resolution substrate beneath `universal-pathlib`; the `UPath.fs` handle threads into the DuckDB scan session.
- `obstore` — Object-store substrate reached through the runtime store lane; this branch names its credential providers alone (`spatial/catalog`).

[MESH_INTERCHANGE]:
- `meshio`

[WIRE_CODEGEN]:
- `protobuf` — Google message runtime beneath the Substrait plan IR; `tabular/query#QUERY` reads its decode fault as the inbound-plan refusal.
- `protovalidate` — Evaluates generated organization rules after the bounded recursive census and before graph allocation.
