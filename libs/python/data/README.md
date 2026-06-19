# [PY_DATA]

`data` is the host-free data-interchange companion of the Python branch. It owns typed dataset refs, columnar lazy/streaming scan and egress, the transactional table-format lakehouse, cross-engine relational query, a data-contract gate, dataframe-agnostic interop with a pyarrow-free Arrow carrier, vector and raster geospatial, graph payloads, chunked tensor stores, and mesh-file exchange. It consumes runtime `ContentIdentity`, `ReceiptContributor`, and `TransportResource` at the boundary and never re-mints them, integrating with C# only at the wire (content-identity plus GLB) and the companion/offline seams. `ARCHITECTURE.md` carries the domain map, `IDEAS.md` the forward pool, and `TASKLOG.md` the open work.

## [01]-[ROUTER]

- [01]-[COLUMNAR](.planning/tabular/columnar.md): Dataset-ref owner discriminating by source shape; cross-engine scan plans, typed columnar egress, content-keyed query receipt, and incremental CDC-materialization owner.
- [02]-[LAKEHOUSE](.planning/tabular/lakehouse.md): Transactional table-format lakehouse over one `LakeOp` operation axis crossed with one `TableFormat` provider axis (Delta/Iceberg/Lance).
- [03]-[QUERY](.planning/tabular/query.md): Relational query engine over one `QuerySpec` axis (DuckDB, narwhals, Ibis IR, ADBC/ConnectorX remote transport) to uniform Arrow.
- [04]-[CONTRACT](.planning/tabular/contract.md): Data-contract gate over pandera and structural frame admission; the one `SchemaClaim` for the package.
- [05]-[INTEROP](.planning/tabular/interop.md): Backend-agnostic frame translation over narwhals and the pyarrow-free Arrow C Data Interface carrier.
- [06]-[EGRESS](.planning/tabular/egress.md): Native object-store egress façade over `obstore` composing the runtime transport, keyed by `ContentIdentity`.
- [07]-[GEOSPATIAL](.planning/spatial/geospatial.md): Vector and raster geospatial claims; in-frame `VectorOp`/`RasterOp` operation axes, spatial egress with GeoArrow encoding, and the DuckDB-spatial join/index/H3 query engine.
- [08]-[CATALOG](.planning/spatial/catalog.md): Cloud-native STAC item/collection discovery over `pystac-client`, the `stac-geoparquet` item table, and asset-href fold into object-store egress and the gridded tensor virtual cube.
- [09]-[MESH](.planning/spatial/mesh.md): Mesh-file identity, cell-block topology, units, GLB preview export, and the LAS/LAZ/COPC point-cloud interchange row.
- [10]-[TENSOR](.planning/gridded/tensor.md): Chunked N-D tensor store over a backend axis with the VirtualiZarr virtual-reference cube.
- [11]-[FIELD](.planning/gridded/field.md): CF-conventioned labelled N-D field dataset over `xarray` (netcdf4/HDF5/Zarr engines), CF-aware selection, and grouped/resampled reductions.
- [12]-[GRAPH](.planning/graph/graph.md): Graph payloads over `rustworkx` with `networkx` compat, typed algorithm receipts, and graph egress.

## [02]-[DOMAIN_PACKAGES]

Every data-domain library the folder uses, planned or implemented; versions are centralized in the one branch manifest and never pinned here. API evidence lives in the adjacent `.api/` folder.

[DATAFRAME]:
- `polars`
- `pandas`
- `narwhals`

[ARROW]:
- `pyarrow`
- `arro3-core`
- `nanoarrow`

[LAKEHOUSE]:
- `deltalake`
- `pyiceberg`
- `pylance`

[QUERY]:
- `duckdb`
- `ibis-framework`
- `connectorx`
- `adbc-driver-manager`

[CONTRACT]:
- `pandera`

[GEOSPATIAL]:
- `geopandas`
- `shapely`
- `pyproj`
- `pyogrio`
- `rasterio`

[STAC]:
- `pystac`
- `pystac-client`
- `stac-geoparquet`

[GRAPH]:
- `networkx`
- `rustworkx`

[GRIDDED]:
- `zarr`
- `cubed`
- `awkward`
- `xarray`
- `icechunk`
- `virtualizarr`
- `h5py`
- `netcdf4`

[MESH_INTERCHANGE]:
- `meshio`
- `trimesh`
- `rhino3dm`
- `laspy`
- `pdal`

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
