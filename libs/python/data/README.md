# [PY_DATA]

`data` is the host-free data-interchange companion of the Python branch: typed dataset refs, columnar lazy/streaming scan and egress, the transactional table-format lakehouse, cross-engine relational query, a data-contract gate, dataframe-agnostic interop with a pyarrow-free Arrow carrier, vector and raster geospatial, graph payloads, chunked tensor stores, and mesh-file exchange. It is a peer producer that consumes runtime `ContentIdentity`, `ReceiptContributor`, and `TransportResource` at the boundary and never re-mints them, integrating with C# only at the wire (content-identity plus GLB) and the companion/offline seams. This file routes the design pages and registers the external packages the folder uses; `ARCHITECTURE.md` carries the domain map, `IDEAS.md` the forward pool, and `TASKLOG.md` the open work.

## [1]-[ROUTER]

The design pages under `.planning/`, one sub-domain folder per eventual source sub-tree.

- `.planning/columnar/dataset.md` — the dataset-ref owner discriminating by source shape, the cross-engine scan plans, the typed columnar egress, the content-keyed query receipt, and the incremental CDC-materialization owner.
- `.planning/lakehouse/table.md` — the transactional table-format lakehouse over one `LakeOp` operation axis crossed with one `TableFormat` provider axis (Delta/Iceberg/Lance).
- `.planning/query/relational.md` — the relational query engine over one `QuerySpec` axis (DuckDB, narwhals, Ibis IR, ADBC/ConnectorX remote transport) to uniform Arrow.
- `.planning/contracts/admission.md` — the data-contract gate over pandera and the structural frame admission, the one `SchemaClaim` for the package.
- `.planning/interop/frame.md` — backend-agnostic frame translation over narwhals and the pyarrow-free Arrow C Data Interface carrier.
- `.planning/geospatial/claim.md` — vector and raster geospatial claims, the in-frame `VectorOp`/`RasterOp` operation axes, spatial egress with GeoArrow encoding, and the DuckDB-spatial join/index/H3 query engine.
- `.planning/stac-catalog/catalog.md` — cloud-native STAC item/collection discovery over pystac-client, the stac-geoparquet item table, and the asset-href fold into cloud-egress and the tensor virtual cube.
- `.planning/graph/payload.md` — graph payloads over rustworkx with networkx compat, typed algorithm receipts, and graph egress.
- `.planning/tensor/store.md` — the chunked N-D tensor store over a backend axis with the VirtualiZarr virtual-reference cube.
- `.planning/field-dataset/dataset.md` — the CF-conventioned labelled N-D field dataset over xarray (netcdf4/HDF5/Zarr engines), CF-aware selection, and grouped/resampled reductions.
- `.planning/mesh/exchange.md` — mesh-file identity, cell-block topology, units, GLB preview export, and the LAS/LAZ/COPC point-cloud interchange row.
- `.planning/cloud-egress/store.md` — the native object-store egress façade over obstore composing the runtime transport, keyed by `ContentIdentity`.

## [2]-[PACKAGES]

Every external library the folder uses, planned or implemented, as a flat list; versions live in the one branch manifest.

- Columnar and frame: `polars`, `pyarrow`, `arro3-core`, `nanoarrow`, `pandas`, `narwhals`
- Query and ingest: `duckdb`, `ibis-framework`, `connectorx`, `adbc-driver-manager`
- Table formats: `deltalake`, `pyiceberg`, `pylance`
- Contracts: `pandera`
- Geospatial: `geopandas`, `shapely`, `pyproj`, `pyogrio`, `rasterio`
- STAC catalog: `pystac-client`, `pystac`, `stac-geoparquet`
- Graph: `networkx`, `rustworkx`
- Tensor: `zarr`, `cubed`, `awkward`, `xarray`, `icechunk`, `virtualizarr`, `h5py`, `netcdf4`
- Mesh and point cloud: `meshio`, `trimesh`, `rhino3dm`, `laspy`, `pdal`
- Cloud egress: `obstore`

## [3]-[CROSS_CUTTING]

Branch-wide packages from the Python infrastructure layer that this folder directly consumes.

- Expression and struct: `expression`, `msgspec`
- Array substrate: `numpy`
