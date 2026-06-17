# [PY_DATA]

`data` is the host-free data-interchange companion of the Python branch: typed dataset refs, columnar lazy/streaming scan and egress, the transactional table-format lakehouse, cross-engine relational query, a data-contract gate, dataframe-agnostic interop with a pyarrow-free Arrow carrier, vector and raster geospatial, graph payloads, chunked tensor stores, and mesh-file exchange. It is a peer producer that consumes runtime `ContentIdentity`, `ReceiptContributor`, and `TransportResource` at the boundary and never re-mints them, integrating with C# only at the wire (content-identity plus GLB) and the companion/offline seams. This file routes the design pages and registers the external packages the folder uses; `ARCHITECTURE.md` carries the domain map, `IDEAS.md` the forward pool, and `TASKLOG.md` the open work.

## [1]-[PAGES]

The design pages under `.planning/`, one sub-domain folder per eventual source sub-tree. The `cloud-egress` sub-domain is planned and carries no page yet; `ARCHITECTURE.md` shows it as a visible gap.

- `.planning/columnar/dataset.md` — the dataset-ref owner discriminating by source shape, the cross-engine scan plans, the typed columnar egress, and the content-keyed query receipt.
- `.planning/lakehouse/table.md` — the transactional table-format lakehouse over one `LakeOp` axis with the Delta/Iceberg/Lance table-format binding.
- `.planning/query/relational.md` — the relational query engine over one `QuerySpec` axis (DuckDB, narwhals, Ibis IR, ADBC/ConnectorX) to uniform Arrow.
- `.planning/contracts/admission.md` — the data-contract gate over pandera and the structural frame admission, the one `SchemaClaim` for the package.
- `.planning/interop/frame.md` — backend-agnostic frame translation over narwhals and the pyarrow-free Arrow C Data Interface carrier.
- `.planning/geospatial/claim.md` — vector and raster geospatial claims, spatial egress, GeoArrow encoding, and the DuckDB-spatial join engine.
- `.planning/graph/payload.md` — graph payloads over rustworkx with networkx compat, typed algorithm receipts, and graph egress.
- `.planning/tensor/store.md` — the chunked N-D tensor store over a backend axis with virtual-reference cubes.
- `.planning/mesh/exchange.md` — mesh-file identity, cell-block topology, units, GLB preview export, and the point-cloud interchange row.

## [2]-[PACKAGES]

Every external library the folder uses, planned or implemented, as a flat list; versions live in the one branch manifest.

- Columnar and frame: `polars`, `pyarrow`, `arro3-core`, `nanoarrow`, `pandas`, `narwhals`, `dask`
- Query and ingest: `duckdb`, `ibis-framework`, `connectorx`, `adbc-driver-manager`
- Table formats: `deltalake`, `pyiceberg`, `lance`
- Contracts: `pandera`
- Geospatial: `geopandas`, `shapely`, `pyproj`, `pyogrio`, `rasterio`
- Graph: `networkx`, `rustworkx`
- Tensor: `zarr`, `cubed`, `awkward`, `xarray`, `icechunk`, `virtualizarr`, `h5py`
- Mesh and point cloud: `meshio`, `trimesh`, `rhino3dm`, `laspy`, `pdal`
- Cloud egress (planned): `obstore`
