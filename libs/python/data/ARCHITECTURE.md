# [PY_DATA_ARCHITECTURE]

The domain map of `data` — the host-free data-interchange companion. A `tabular` interchange core (columnar scan, lakehouse, query, contract, interop, egress) plus the `spatial`, `gridded`, and `graph` planes, each a real domain concept.

Each codemap node is the eventual source file its `.planning/` design page becomes, named in the language's own folder and file casing — PascalCase `.cs`, lowercase `.py`, lowercase `.ts`. Treat every node as realized code; the `.planning/` scaffold is the authoring substrate, never part of the map.

## [1]-[DOMAIN_MAP]

```text codemap
data/
├── tabular/              # Columnar, relational, and lakehouse interchange plane + its object-store egress leg
│   ├── columnar.py       # Dataset-ref owner, engine scan plans, typed egress, query receipt
│   ├── lakehouse.py      # Lakehouse owner over LakeOp lifecycle and Delta table-format binding
│   ├── query.py          # Query engine over QuerySpec frontends materializing to uniform Arrow
│   ├── contract.py       # Pandera data-quality gate and structural narwhals frame admission
│   ├── interop.py        # Backend-agnostic frame owner and Arrow PyCapsule zero-copy carrier
│   └── egress.py         # Object-store egress owner over one StoreOp axis keyed by ContentIdentity
├── spatial/              # Vector + raster geo, STAC catalog discovery, mesh-file exchange
│   ├── geospatial.py     # Vector and raster geo claims, VectorOp/RasterOp in-frame operation axes, and egress-format spatial export
│   ├── catalog.py        # StacCatalog owner over pystac-client search, stac-geoparquet item table, asset-href egress fold
│   └── mesh.py           # Mesh-file exchange owner over backend axis and point-cloud row
├── gridded/              # Chunked N-D tensor store + CF labelled-field store
│   ├── tensor.py         # Chunked N-D tensor store owner over backend, codec, and region axes
│   └── field.py          # FieldDataset owner over netcdf4/HDF5/Zarr CF engines, CF-aware selection, grouped/resampled reductions
└── graph/                # Rustworkx graph payloads with networkx compat, typed result receipts
    └── graph.py          # Graph-payload owner, algorithm axis, typed result receipt
```

## [2]-[SEAMS]

```text seams
tabular/columnar  →  csharp:Rasm.Compute/Runtime   # [SHAPE]: DOE dataset / labelled-array study input
spatial/mesh      →  python:runtime/observability  # [CONTENT_KEY]: ContentIdentity over mesh point coordinates
tabular/egress    →  python:runtime/observability  # [CONTENT_KEY]: ContentIdentity over put payload + e-tag
tabular/*         ←  python:runtime                # [PORT]: TransportResource remote connection
tabular           ←  python:artifacts/documents    # [WIRE]: to_corpus_row flat record
tabular           ←  python:artifacts/figures      # [WIRE]: color palette arrays / appearance correlates
tabular/columnar  ←  python:runtime/transport      # [TRANSPORT]: ResourceRef path resolution through fsspec
*                 →  python:runtime                # [RECEIPT]: Receipt contribution
spatial/mesh      ←  python:geometry/scan          # [SHAPE]: Arrow point-record columnar bridge x/y/z
spatial/mesh      →  python:geometry/mesh          # [SHAPE]: MeshPayload cell-block topology
tabular           →  python:compute/experiments    # [SHAPE]: DOE dataset / labelled-array study input
```
