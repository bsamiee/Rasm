# [PY_DATA]

`data` is the host-free data-interchange companion of the Python branch. It owns typed dataset refs, columnar lazy/streaming scan and egress, the transactional table-format lakehouse, cross-engine relational query, a data-contract gate, dataframe-agnostic interop with a pyarrow-free Arrow carrier, vector and raster geospatial, graph payloads, chunked tensor stores, and mesh-file exchange. It consumes runtime `ContentIdentity`, `ReceiptContributor`, and `TransportResource` at the boundary and never re-mints them, integrating with C# only at the wire (content-identity plus GLB) and the companion/offline seams. `ARCHITECTURE.md` carries the domain map, `IDEAS.md` the forward pool, and `TASKLOG.md` the open work.

## [01]-[ROUTER]

- [01]-[COLUMNAR](.planning/tabular/columnar.md): Dataset-ref owner discriminating by source shape; cross-engine scan plans, typed columnar egress, content-keyed query receipt, and incremental CDC-materialization owner.
- [02]-[LAKEHOUSE](.planning/tabular/lakehouse.md): Transactional table-format lakehouse over one `LakeOp` operation axis crossed with one `TableFormat` provider axis (Delta/Iceberg/Lance).
- [03]-[QUERY](.planning/tabular/query.md): Relational query engine over one `QuerySpec` axis (DuckDB, narwhals, Ibis IR, ADBC/ConnectorX/Flight SQL remote transport, daft elasticity, DuckDB-Substrait plan portability) to uniform Arrow.
- [04]-[CONTRACT](.planning/tabular/contract.md): Data-contract gate over dataframely tabular column rules and pandera xarray/statistical checks folded on the one `SchemaClaim` for the package.
- [05]-[INTEROP](.planning/tabular/interop.md): Backend-agnostic frame translation over narwhals and the pyarrow-free Arrow C Data Interface carrier.
- [06]-[PROFILE](.planning/tabular/profile.md): Graded data-quality plane over `pointblank` Thresholds, keyed by `ContentIdentity`, emitting a `QualityProfile` frame the artifacts great-tables renderer renders.
- [07]-[EGRESS](.planning/tabular/egress.md): Native object-store egress façade over `obstore` composing the runtime transport, keyed by `ContentIdentity`.
- [08]-[GEOSPATIAL](.planning/spatial/geospatial.md): Vector and raster geospatial claims; in-frame `VectorOp`/`RasterOp` operation axes, spatial egress with GeoArrow encoding, and the DuckDB-spatial join/index/H3 query engine.
- [09]-[CATALOG](.planning/spatial/catalog.md): Cloud-native STAC item/collection discovery over `pystac-client`, the `stac-geoparquet` item table, and asset-href fold into object-store egress and the gridded virtual cube.
- [10]-[MESH](.planning/spatial/mesh.md): Mesh-file identity, cell-block topology, units, GLB preview export, and the LAS/LAZ/COPC point-cloud interchange row.
- [11]-[STORE](.planning/gridded/store.md): Dense chunked N-D tensor store over a 2-row `TensorBackend` axis (`zarr` write, `cubed` plan, `tensorstore` async read) with codec and region axes.
- [12]-[VIRTUAL](.planning/gridded/virtual.md): Virtual-reference cube owner over `virtualizarr` manifest parsers and `icechunk` `set_virtual_ref` native virtual-chunk addressing.
- [13]-[RAGGED](.planning/gridded/ragged.md): Ragged N-D store owner over `awkward` with the `from_arrow`/`to_arrow` zero-copy bridge to the interop Arrow carrier.
- [14]-[FIELD](.planning/gridded/field.md): CF-conventioned labelled N-D field dataset over `xarray` (netcdf4/HDF5/Zarr engines), CF-aware selection, and `flox` grouped/resampled reductions.
- [15]-[GRAPH](.planning/graph/graph.md): Graph payloads over `rustworkx` with `networkx` compat, typed algorithm receipts, and graph egress.

## [02]-[DOMAIN_PACKAGES]

Every data-domain library the folder uses, planned or implemented; versions are centralized in the one branch manifest and never pinned here. API evidence lives in the adjacent `.api/` folder.

[DATAFRAME]:
- `polars`
- `polars-st`
- `narwhals`
- `pandas` — External producers hand pandas frames at the wire; no internal owner constructs one. `DatasetKind.PANDAS_FILE` stays foreign-file row.

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

DuckDB loadable extensions back plan and table-format rows without a pip dependency: `substrait` (the `duckdb-substrait` community extension wired on `query.py` producing the portable binary/JSON plan blob) is implemented; `ducklake` (core-loadable `LOAD`, the SQL-catalog lakehouse over Parquet for `lakehouse.py`) and `iceberg` (the cp315-clean DuckDB Iceberg route for `lakehouse.py`) are planned admissions, both provisioned through the Forge DuckDB-extensions catalog.

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
- `cubed`
- `tensorstore`
- `awkward`
- `xarray`
- `flox`
- `icechunk`
- `virtualizarr`
- `h5py`
- `netcdf4`

[MESH_INTERCHANGE]:
- `meshio`
- `trimesh`
- `rhino3dm`
- `laspy` — Owns the COPC octree-subset read via `laspy.copc`; uncompressed reads run on cp315 core, compressed `.copc.laz` reads bind `lazrs`/`laszip`.
- `lazrs`
- `laszip`
- `pdal` — Stays the geometry-side point-cloud filter-graph owner; the data COPC arm rebinds to `laspy.copc` without removing it.

[OBJECT_STORE]:
- `obstore`

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
