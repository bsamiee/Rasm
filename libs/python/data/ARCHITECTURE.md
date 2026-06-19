# [PY_DATA_ARCHITECTURE]

The professional-domain folder-map for `data`: the host-free data-interchange companion. The map is the full sub-domain structure, each named by its real domain concept with a one-line charter. Columnar identity and scan, the table-format lakehouse, relational query, the data-contract gate, and dataframe-agnostic interop form the interchange core; geospatial, graph, tensor, and mesh carry the domain payloads; cloud-egress is the native object-store egress façade over the runtime transport. Dependency direction across the Python packages lives once in the branch `ARCHITECTURE.md`; boundaries and wires live on the tasks that build them, never as a per-folder seam ledger.

## [1]-[DOMAIN_MAP]

The sub-domain layout mirrors the eventual source tree, one folder per professional concept. Every leaf carries a decision-complete design page under `.planning/`.

```text codemap
data/
├── columnar/              # dataset-ref identity + cross-engine lazy scan + typed Arrow/Parquet/IPC egress
│   └── dataset.py         # the dataset-ref owner, the engine scan plans, the typed egress, the query receipt
├── lakehouse/             # transactional table-format interchange over one LakeOp axis
│   └── table.py           # the lakehouse owner over the LakeOp lifecycle and the Delta table-format binding
├── query/                 # relational query over one QuerySpec axis materializing to uniform Arrow
│   └── relational.py      # the query engine over the QuerySpec frontends materializing to uniform Arrow
├── contracts/             # data-contract gate + structural frame admission, one SchemaClaim for the package
│   └── admission.py       # the pandera data-quality gate and the structural narwhals frame admission
├── interop/               # backend-agnostic frame translation + the pyarrow-free Arrow C-data carrier
│   └── frame.py           # the backend-agnostic frame owner and the Arrow PyCapsule zero-copy carrier
├── geospatial/            # vector + raster geospatial claims, spatial egress, GeoArrow/DuckDB-spatial engine, in-frame vector + raster operation axes
│   └── claim.py           # the vector and raster geo claims, the VectorOp/RasterOp in-frame operation axes, and the egress-format spatial export
├── stac-catalog/          # cloud-native STAC item/collection discovery folding into cloud-egress + tensor virtual cube + columnar catalog
│   └── catalog.py         # the StacCatalog owner over pystac-client search, the stac-geoparquet item table, the asset-href egress fold
├── graph/                 # graph payloads over rustworkx with networkx compat, typed result receipts
│   └── payload.py         # the graph-payload owner, the algorithm axis, the typed result receipt
├── tensor/                # chunked N-D tensor store over a TensorBackend axis, virtual-reference cubes
│   └── store.py           # the chunked N-D tensor store owner over the backend, codec, and region axes
├── field-dataset/         # CF-conventioned labelled N-D field dataset over xarray, the labelled-field counterpart of the dense tensor store
│   └── dataset.py         # the FieldDataset owner over the netcdf4/HDF5/Zarr CF engines, CF-aware selection, grouped/resampled reductions
├── mesh/                  # mesh-file identity/cell-block/units/GLB export + LAS/LAZ/COPC point-cloud row
│   └── exchange.py        # the mesh-file exchange owner over the backend axis and the point-cloud row
└── cloud-egress/          # native object-store egress over obstore composing runtime TransportResource
    └── store.py           # the object-store egress owner over one StoreOp axis keyed by ContentIdentity
```

## [2]-[CHARTERS]

- `columnar`: dataset-ref identity discriminating by source shape, cross-engine lazy/streaming scan over Polars/DuckDB/PyArrow, and typed Arrow/Parquet/IPC egress folding one content-keyed `QueryReceipt`.
- `lakehouse`: transactional table-format interchange over one `LakeOp` axis on one `Lakehouse` owner — write/read/time-travel/optimize/vacuum/changefeed/merge — with the table-format binding admitting Delta now and Iceberg/Lance as sibling axis rows.
- `query`: relational query over one `QuerySpec` axis (DuckDB SQL/relational, narwhals dataframe-agnostic, the admitted Ibis backend-agnostic IR) materializing to uniform Arrow, with ADBC/ConnectorX remote transport acquired through the runtime `TransportResource`.
- `contracts`: the data-contract gate folding `QualityRule` rows into one `pandera` schema recording a non-enforcing `SchemaClaim`, plus structural `FieldShape` frame admission proving required presence before routing enforcement to that same gate — exactly one `SchemaClaim` for the package.
- `interop`: backend-agnostic frame translation over `narwhals` keyed by one backend axis, plus the pyarrow-free Arrow C Data Interface zero-copy carrier over the Arrow PyCapsule protocol via `arro3-core`/`nanoarrow`.
- `geospatial`: vector and raster geospatial claims over geopandas/shapely/pyproj and rasterio, spatial egress as one `EgressFormat`-axis union, with native GeoArrow encoding and the DuckDB-spatial join/H3 index engine as axis rows; the in-frame vector algebra (`VectorOp` over geopandas sjoin/overlay/dissolve/clip/buffer/predicates) and raster-coverage algebra (`RasterOp` over rasterio windowed-read/merge/mask/features) are two operation axes on the `VectorGeoClaim`/`RasterGeoClaim` owners, the GeoDataFrame/NumPy-array counterparts of the columnar `SpatialQuery` SQL engine.
- `stac-catalog`: cloud-native STAC item/collection discovery over `pystac-client` (bbox/datetime/CQL2 search), encoding the item catalog as a `stac-geoparquet` columnar Arrow table the `columnar`/`query` owners consume, and folding the discovered asset hrefs into the `cloud-egress` `GetRange` archival-byte path and the `tensor` virtual-reference cube — the discovery layer above the raster/vector claims and the cloud-egress byte path, never a second object-store transport or virtual-cube builder.
- `graph`: graph payloads over `rustworkx` with `networkx` compat, one `GraphAlgorithm` axis, typed `GraphResult` receipts, and node-link/GraphML/tabular egress.
- `tensor`: chunked N-D tensor store over a `TensorBackend` axis (zarr dense, cubed bounded-memory plan, awkward ragged, icechunk versioned), with VirtualiZarr virtual-reference cubes over archival byte ranges.
- `field-dataset`: the CF-conventioned labelled N-D field dataset over `xarray` (netcdf4/HDF5/Zarr engines) — CF-aware coordinate selection, label-indexed slicing, grouped and resampled reductions, and unit/coordinate-reference metadata, materializing to the `pyarrow`/Zarr egress; the labelled-field counterpart of the dense chunk-grid `tensor` store, binding the admitted-but-unconsumed `netcdf4`, distinct from the chunk-byte store and never a second labelled-array owner inside `tensor`.
- `mesh`: mesh-file identity/cell-block topology/units/GLB export over a `MeshBackend` axis (meshio FE, trimesh surface), plus the LAS/LAZ/COPC point-cloud interchange row feeding the geometry scan companion.
- `cloud-egress`: native object-store egress over `obstore`, the highest-throughput cloud path, over one `StoreOp` axis (put/get/list/delete/copy) composing the runtime `TransportResource`/`ResourceRef` and keying by `ContentIdentity`, never a second transport owner.
