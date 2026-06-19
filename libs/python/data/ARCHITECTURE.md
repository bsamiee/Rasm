# [PY_DATA_ARCHITECTURE]

The professional domain map of `data` — the host-free data-interchange companion. A `tabular` interchange core (columnar scan, lakehouse, query, contract, interop, egress) plus the `spatial`, `gridded`, and `graph` planes, each a real domain concept.

Each codemap node is the eventual source file its `.planning/` design page becomes, named in the language's own folder and file casing — PascalCase `.cs`, lowercase `.py`, lowercase `.ts`. Treat every node as realized code; the `.planning/` scaffold is the authoring substrate, never part of the map.

## [1]-[DOMAIN_MAP]

```text codemap
data/
├── tabular/              # the columnar, relational, and lakehouse interchange plane + its object-store egress leg
│   ├── columnar.py       # the dataset-ref owner, the engine scan plans, the typed egress, the query receipt
│   ├── lakehouse.py      # the lakehouse owner over the LakeOp lifecycle and the Delta table-format binding
│   ├── query.py          # the query engine over the QuerySpec frontends materializing to uniform Arrow
│   ├── contract.py       # the pandera data-quality gate and the structural narwhals frame admission
│   ├── interop.py        # the backend-agnostic frame owner and the Arrow PyCapsule zero-copy carrier
│   └── egress.py         # the object-store egress owner over one StoreOp axis keyed by ContentIdentity
├── spatial/              # vector + raster geo, STAC catalog discovery, mesh-file exchange
│   ├── geospatial.py     # the vector and raster geo claims, the VectorOp/RasterOp in-frame operation axes, and the egress-format spatial export
│   ├── catalog.py        # the StacCatalog owner over pystac-client search, the stac-geoparquet item table, the asset-href egress fold
│   └── mesh.py           # the mesh-file exchange owner over the backend axis and the point-cloud row
├── gridded/              # chunked N-D tensor store + CF labelled-field store
│   ├── tensor.py         # the chunked N-D tensor store owner over the backend, codec, and region axes
│   └── field.py          # the FieldDataset owner over the netcdf4/HDF5/Zarr CF engines, CF-aware selection, grouped/resampled reductions
└── graph/                # rustworkx graph payloads with networkx compat, typed result receipts
    └── graph.py          # the graph-payload owner, the algorithm axis, the typed result receipt
```

## [2]-[SEAMS]

```text seams
tabular/columnar  →  csharp:Rasm.Compute/Runtime   # DOE dataset / labelled-array study input (shape)
spatial/mesh      →  python:runtime/observability  # ContentIdentity over mesh point coordinates (content-key)
tabular/egress    →  python:runtime/observability  # ContentIdentity over put payload + e-tag (content-key)
tabular/*         ←  python:runtime                # TransportResource remote connection (port)
tabular           ←  python:artifacts/documents    # to_corpus_row flat record (wire)
tabular           ←  python:artifacts/figures      # color palette arrays / appearance correlates (wire)
tabular/columnar  ←  python:runtime/transport      # ResourceRef path resolution through fsspec (transport)
*                 →  python:runtime                # Receipt contribution (receipt)
spatial/mesh      ←  python:geometry/scan          # Arrow point-record columnar bridge x/y/z (shape)
spatial/mesh      →  python:geometry/mesh          # MeshPayload cell-block topology (shape)
```
