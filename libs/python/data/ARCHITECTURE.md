# [PY_DATA_ARCHITECTURE]

The domain map of `data` — the host-free data-interchange companion. A `tabular` interchange core (columnar scan, lakehouse, query, materialize, contract, interop, egress) plus the `spatial`, `gridded`, `graph`, and `impact` planes, each a real domain concept. The module set is a provable import DAG: `tabular/interop < tabular/columnar < {tabular/contract, tabular/egress, graph/graph, gridded/store, gridded/field} < {tabular/lakehouse, tabular/query, tabular/profile, gridded/ragged, gridded/virtual, spatial/geospatial, spatial/mesh} < {tabular/materialize, spatial/catalog, spatial/query, impact/impact} < spatial/grid` — every `from rasm.data.*` fence import binds a strictly-earlier module, and the `[02]-[SEAMS]` ledger declares every eager intra-data edge beside the cross-`libs/` and cross-language rows.

Each codemap node is the eventual source file its `.planning/` design page becomes, named in the language's own folder and file casing — PascalCase `.cs`, lowercase `.py`, lowercase `.ts`. Treat every node as realized code; the `.planning/` scaffold is the authoring substrate, never part of the map.

## [01]-[DOMAIN_MAP]

```text codemap
data/
├── tabular/              # Columnar, relational, and lakehouse interchange plane + its object-store egress leg
│   ├── interop.py        # Backend-agnostic frame owner, FieldShape minter, and Arrow PyCapsule zero-copy carrier
│   ├── columnar.py       # Dataset-ref owner, the one request-scoped DuckDbSession rail (extension policy rows), engine scan plans, typed egress
│   ├── lakehouse.py      # Lakehouse owner over LakeOp lifecycle and table-format bindings
│   ├── query.py          # Query engine over QuerySpec frontends (sql/rel/agnostic/ir/remote/streaming/federated) to uniform Arrow; datafusion
│   ├── materialize.py    # DerivedSnapshot/PartitionBundle CDC-materialization composing lakehouse+query+columnar downward; partition-delta
│   ├── contract.py       # Dataframely covenant and pandera quality gate folded on one ContractClaim; FieldShape imported downward from interop
│   ├── profile.py        # QualityProfile owner over pointblank Thresholds, graded data-quality plane emitting a frame the artifacts renderer renders
│   └── egress.py         # Object-store egress owner over one StoreOp axis keyed by ContentIdentity
├── spatial/              # Vector + raster claims, DuckDB-spatial engine, DGG plane, STAC catalog, mesh-file exchange
│   ├── geospatial.py     # CLAIMS plane: vector/raster geo claims (RasterGeoClaim carries transform), VectorOp/RasterOp axes + Linear/Geodesic
│   ├── query.py          # SpatialQuery/SpatialEngine DuckDB-spatial join/transform/H3-SQL engine over the shared DuckDbSession rail; the in-DB half
│   ├── grid.py           # GridSystem h3ronpy DGG plane (in-frame vectorized cell algebra) + polars-st GeoLift/GeoFrameOp frame-native geometry
│   ├── catalog.py        # StacCatalog owner over search, item table, asset-href egress, and catalog claims
│   └── mesh.py           # Mesh-file exchange owner over backend axis and point-cloud row
├── gridded/              # Chunked N-D dense/virtual/ragged tensor stores + CF labelled-field store
│   ├── store.py          # Dense chunked N-D tensor store owner over zarr write, cubed plan, and tensorstore async read
│   ├── virtual.py        # SOLE manifest-cube owner: virtualizarr manifest parsers + native addressing, ManifestWrite export/registration axis
│   ├── ragged.py         # Ragged N-D store owner over awkward, with from_arrow/to_arrow zero-copy bridge to the interop Arrow carrier
│   └── field.py          # CF owner ONLY: FieldDataset over netcdf4/h5netcdf/Zarr CF engines, flox grouped/resampled reductions, content-keyed egress
├── graph/                # Rustworkx graph payloads with networkx codec lane, typed result receipts
│   └── graph.py          # Graph-payload owner: n-arm _run_rx kernel + the GPL-confined Leiden/Louvain/Infomap split, carried WeightSelector payloads
└── impact/               # Material environmental-impact: EPD declaration ingest + LCA compute, normalized to one EN 15804 carrier
    └── impact.py         # MaterialImpact owner folding the five-source ImpactSource axis into one canonical EN 15804 indicator × stage matrix
```

## [02]-[SEAMS]

```text seams
tabular/contract    →   python:data/tabular/interop               # [SHAPE]: FieldShape/Backend/FrameInterop imported downward in one module-top
tabular/profile     →   python:data/tabular/interop               # [SHAPE]: FieldShape drives the schema probe
tabular/lakehouse   →   python:data/tabular/columnar              # [PORT]: DatasetKind/DatasetRef admission + DuckDbSession/DuckDbExtension rail
tabular/query       →   python:data/tabular/columnar              # [RECEIPT]: QueryReceipt + the one exported predicate_count fold
tabular/materialize →   python:data/tabular/columnar              # [CONTENT_KEY]: the public arrow_bytes canonical whole-table fold keys each
tabular/materialize →   python:data/tabular/lakehouse             # [PORT]: Lakehouse/TableFormat source identity + the ChangeFeed load_cdf surface
tabular/materialize →   python:data/tabular/query                 # [PORT]: awaited QueryEngine.run recompute over QuerySpec
gridded/ragged      →   python:data/tabular/interop               # [WIRE]: ArrowCStream.of the one public carrier construction
gridded/virtual     →   python:data/gridded/field                 # [SHAPE]: FieldReceipt family minted downward by the absorbed manifest owner
spatial/geospatial  →   python:data/tabular/columnar              # [RECEIPT]: QueryReceipt.railed over coverage/egress Arrow bytes
spatial/query       →   python:data/tabular/columnar              # [PORT]: DuckDbSession rail (SPATIAL prelude + H3 supplement rows) + QueryReceipt
spatial/grid        →   python:data/spatial/query                 # [PORT]: engine_bin composes the in-DB H3 binning leg (two-substrate law)
spatial/grid        →   python:data/tabular/columnar              # [RECEIPT]: QueryReceipt.railed over the cell frame
spatial/catalog     →   python:data/gridded/virtual               # [PORT]: FieldVirtual + ManifestWrite(cube)/VirtualReference.apply registration
spatial/catalog     →   python:data/spatial/geospatial            # [SHAPE]: RasterGeoClaim with transform + Resampling constructed from STAC
spatial/catalog     →   python:data/tabular/egress                # [PORT]: ObjectEgress.GetRange archival byte windows
spatial/catalog     →   python:data/tabular/columnar              # [RECEIPT]: QueryReceipt over the encoded STAC table
spatial/mesh        →   python:data/tabular/columnar              # [RECEIPT]: QueryReceipt.railed named-array/point-record Arrow egress
graph/graph         →   python:data/tabular/columnar              # [WIRE]: node-keyed GraphResult.frame left-joined as enrichment
impact              →   python:data/tabular/columnar              # [WIRE]: the assessment frame rides the public arrow_bytes fold
impact              →   python:data/tabular/contract              # [SHAPE]: MaterialImpact.gated proves _WIRE_SHAPES through the one contract gate
impact              →   python:data/tabular/interop               # [SHAPE]: Backend/FieldShape/FrameInterop the gate's lowering vocabulary
impact              →   python:data/tabular/profile               # [RECEIPT]: MaterialImpact.profiled grades the frame through QualityProfile
tabular             →   csharp:Rasm.Compute/Runtime               # [SHAPE]: DOE dataset study input
spatial/mesh        →   python:runtime/evidence                   # [CONTENT_KEY]: ContentIdentity over mesh point coordinates
tabular/egress      →   python:runtime/evidence                   # [CONTENT_KEY]: ContentIdentity over put payload + e-tag
tabular/*           ←   python:runtime                            # [PORT]: TransportResource remote connection
tabular             ←   python:artifacts/document                  # [WIRE]: to_corpus_row flat record (the columnar Corpus arm)
tabular             ←   python:runtime/evidence                   # [CONTENT_KEY]: ContentIdentity content-key
spatial/geospatial  ←   python:artifacts/export                   # [WIRE]: addons.geo GeoProxy GeoJSON georeferenced wire; CRS authority stays here
tabular/columnar    ←   python:runtime/transport                  # [TRANSPORT]: ResourceRef path resolution through fsspec/UPath
*                   →   python:runtime                            # [RECEIPT]: Receipt contribution
spatial/mesh        ←   python:geometry/scan                      # [SHAPE]: Arrow point-record columnar bridge x/y/z
spatial/mesh        ←   python:geometry/mesh                      # [BOUNDARY]: mesh-file decode/encode + GLB preview here; repair returns in-memory
spatial/mesh        →   python:geometry/mesh                      # [SHAPE]: MeshPayload cell-block topology
tabular             →   python:compute/experiments/study          # [SHAPE]: FrameAdmission/FrameInterop DOE-frame admission arm
tabular/*           →   csharp:Rasm.Persistence                   # [CONTENT_KEY]: C#-seed ContentKey stamped on outputs, federated as durable reuse
tabular/query       ⇄   csharp:Rasm.Persistence/Query/federation  # [WIRE]: Substrait binary plan interchange — outbound minted by QuerySpec.Federated
tabular/query       →   python:runtime/observability              # [RECEIPT]: QueryReceipt.lineage_edges column-level lineage contribution
tabular/profile     →   python:artifacts/visualization/table      # [SHAPE]: QualityProfile frame rendered by the great-tables tier
gridded/virtual     →   csharp:Rasm.Persistence                   # [CONTENT_KEY]: icechunk as-of snapshot identity reproduced from the XxHash128 seed
spatial/geospatial  →   csharp:Rasm.Compute                       # [SHAPE]: native GeoArrow buffers (geoarrow_wire) sharing the GLB wire layout
spatial/geospatial  ←   csharp:Rasm.Bim/Semantics/geospatial      # [WIRE]: GeoFeature WKB decode via shapely
spatial/mesh        →   python:geometry/scan/ingestion            # [SHAPE]: data COPC arm decode leaving the pdal filter-graph owner unchanged
impact              ←   python:runtime                            # [PORT]: TransportResource for the EC3 + openLCA server endpoints
impact              →   python:runtime/observability              # [RECEIPT]: ImpactReceipt contribution keyed by ContentIdentity
impact              →   csharp:Rasm.Materials                     # [WIRE]: EN 15804 set enters Discipline.Environmental
impact              ⇄   csharp:Rasm.Persistence                   # [CONTENT_KEY]: EPD/LCA identity deduped in the durable reuse ledger
```
