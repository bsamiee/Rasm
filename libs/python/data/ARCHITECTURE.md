# [PY_DATA_ARCHITECTURE]

The domain map of `data` — the host-free data-interchange companion. A `tabular` interchange core (columnar scan, lakehouse, query, contract, interop, egress) plus the `spatial`, `gridded`, `graph`, and `impact` planes, each a real domain concept.

Each codemap node is the eventual source file its `.planning/` design page becomes, named in the language's own folder and file casing — PascalCase `.cs`, lowercase `.py`, lowercase `.ts`. Treat every node as realized code; the `.planning/` scaffold is the authoring substrate, never part of the map.

## [01]-[DOMAIN_MAP]

```text codemap
data/
├── tabular/              # Columnar, relational, and lakehouse interchange plane + its object-store egress leg
│   ├── columnar.py       # Dataset-ref owner, engine scan plans, typed egress, query receipt
│   ├── lakehouse.py      # Lakehouse owner over LakeOp lifecycle and Delta/Iceberg/Lance table-format binding
│   ├── query.py          # Query engine over QuerySpec frontends materializing to uniform Arrow, with Substrait/SQL plan portability and column-level lineage
│   ├── contract.py       # Dataframely tabular gate and pandera xarray/statistical gate folded on one SchemaClaim
│   ├── interop.py        # Backend-agnostic frame owner and Arrow PyCapsule zero-copy carrier
│   ├── profile.py        # QualityProfile owner over pointblank Thresholds, graded data-quality plane emitting a frame the artifacts renderer renders
│   └── egress.py         # Object-store egress owner over one StoreOp axis keyed by ContentIdentity
├── spatial/              # Vector + raster geo, STAC catalog discovery, mesh-file exchange
│   ├── geospatial.py     # Vector and raster geo claims, VectorOp/RasterOp in-frame operation axes, and egress-format spatial export
│   ├── catalog.py        # StacCatalog owner over pystac-client search, stac-geoparquet item table, asset-href egress fold
│   └── mesh.py           # Mesh-file exchange owner over backend axis and point-cloud row
├── gridded/              # Chunked N-D dense/virtual/ragged tensor stores + CF labelled-field store
│   ├── store.py          # Dense chunked N-D tensor store owner over 2-row TensorBackend (zarr write+cubed plan+tensorstore async read) and codec/region axes
│   ├── virtual.py        # Virtual-reference cube owner over virtualizarr manifest parsers and icechunk set_virtual_ref native virtual-chunk addressing
│   ├── ragged.py         # Ragged N-D store owner over awkward, with from_arrow/to_arrow zero-copy bridge to the interop Arrow carrier
│   └── field.py          # FieldDataset owner over netcdf4/HDF5/Zarr CF engines, flox grouped/resampled reductions, virtualizarr/h5py virtual leg
├── graph/                # Rustworkx graph payloads with networkx compat, typed result receipts
│   └── graph.py          # Graph-payload owner, algorithm axis, typed result receipt; GraphResult.frame left-joins tabular/columnar by `node`
└── impact/               # Material environmental-impact: EPD declaration ingest + LCA compute, normalized to one EN 15804 carrier
    └── impact.py         # MaterialImpact owner folding the ImpactSource axis into one canonical EN 15804 indicator × stage matrix, keyed by ContentIdentity
```

## [02]-[SEAMS]

```text seams
tabular             →   csharp:Rasm.Compute/Runtime               # [SHAPE]: DOE dataset / labelled-array study input
spatial/mesh        →   python:runtime/evidence                   # [CONTENT_KEY]: ContentIdentity over mesh point coordinates
tabular/egress      →   python:runtime/evidence                   # [CONTENT_KEY]: ContentIdentity over put payload + e-tag
tabular/*           ←   python:runtime                            # [PORT]: TransportResource remote connection
tabular             ←   python:artifacts/documents                # [WIRE]: to_corpus_row flat record
tabular             ←   python:runtime/evidence                   # [CONTENT_KEY]: ContentIdentity content-key
spatial/geospatial  ←   python:artifacts/export                   # [WIRE]: addons.geo GeoProxy GeoJSON georeferenced wire; CRS authority stays here
tabular             ←   python:artifacts/figures                  # [WIRE]: color palette arrays / appearance correlates
tabular/columnar    ←   python:runtime/transport                  # [TRANSPORT]: ResourceRef path resolution through fsspec
*                   →   python:runtime                            # [RECEIPT]: Receipt contribution
spatial/mesh        ←   python:geometry/scan                      # [SHAPE]: Arrow point-record columnar bridge x/y/z
spatial/mesh        ←   python:geometry/mesh                      # [BOUNDARY]: mesh-file decode/encode + GLB preview here; repair returns in-memory Trimesh
spatial/mesh        →   python:geometry/mesh                      # [SHAPE]: MeshPayload cell-block topology
tabular             →   python:compute/experiments/study          # [SHAPE]: DOE dataset / labelled-array study input
tabular/*           →   csharp:Rasm.Persistence                   # [CONTENT_KEY]: C#-seed ContentKey stamped on outputs, federated as durable reuse ledger
tabular/query       ⇄   csharp:Rasm.Persistence/Query/federation  # [WIRE]: Substrait binary plan + ibis-to_sql portable SQL plan interchange
tabular/query       →   python:runtime/observability              # [RECEIPT]: QueryReceipt.lineage_edges column-level lineage contribution
tabular/profile     →   python:artifacts/figures                  # [SHAPE]: QualityProfile frame rendered by the great-tables tier
gridded/virtual     →   csharp:Rasm.Persistence                   # [CONTENT_KEY]: icechunk as-of snapshot identity reproduced from the XxHash128 seed
spatial/geospatial  →   csharp:Rasm.Compute                       # [SHAPE]: native GeoArrow buffers sharing the GLB wire layout
spatial/mesh        →   python:geometry/scan/ingestion            # [SHAPE]: data COPC arm decode leaving the pdal filter-graph owner unchanged
impact              ←   python:runtime                            # [PORT]: TransportResource for the EC3 + openLCA server endpoints
impact              →   python:runtime/observability              # [RECEIPT]: ImpactReceipt contribution keyed by ContentIdentity
impact              →   csharp:Rasm.Materials                     # [WIRE]: EN 15804 set as Discipline.Environmental / MaterialPropertySet.Environmental
impact              ⇄   csharp:Rasm.Persistence                   # [CONTENT_KEY]: EPD/LCA identity deduped in the durable reuse ledger
```
