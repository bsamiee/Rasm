# [PY_DATA_ARCHITECTURE]

`data` is one portable-interchange surface: every concern is an axis owner with closed cases, every dataset discriminates by source shape rather than an operation family, and every egress bundle keys by the runtime `ContentIdentity` owner. Mechanics live in the finalized `.planning/` pages; this page is the atlas — the implementation source tree, the owner registry (the one owner-state surface), dependency direction, cross-folder seams, the package boundaries, and the prohibitions.

## [1]-[SOURCE_TREE]

The planned module layout IS the build order: each file is one transcription unit, dataset identity and scan plans before the schema/geospatial and graph/mesh owners that compose them. Each leaf is annotated with the owners it transcribes and the owning page#cluster.

```text codemap
data/
├── datasets.py          # DatasetRef — columnar-query#DATASET
├── scan.py              # ScanPlan, ColumnarEgress, QueryReceipt, Lakehouse, QueryEngine, DataQuality — columnar-query#SCAN, #LAKEHOUSE, #QUERY, #QUALITY
├── schema_geo.py        # FrameInterop, FieldShape, FrameAdmission, VectorGeoClaim, RasterGeoClaim — schema-geo#INTEROP, #ADMISSION, #GEO
└── graph_mesh.py        # GraphPayload, TensorStore, MeshPayload — graph-mesh#GRAPH, #TENSOR, #MESH
```

`datasets.py` lands first: `DatasetRef` is the one polymorphic dataset owner discriminating by source shape. `scan.py` follows with the engine scan plans, the typed columnar egress folding one `QueryReceipt`, the Delta lakehouse lifecycle over one `LakeOp` axis, the relational query engine over one `QuerySpec` axis, and the data-quality gate folding `QualityRule` rows into one pandera schema. `schema_geo.py` holds the dataframe-agnostic interop owner over narwhals, the structural frame admission, and the two-axis geospatial surface; `FrameAdmission` routes contract enforcement to the canonical `DataQuality`/`SchemaClaim` on the sibling scan owner rather than minting a second gate. `graph_mesh.py` lands last with the graph payload over a rustworkx fast-path, the chunked N-D tensor store, and the mesh-file exchange owner.

## [2]-[OWNER_REGISTRY]

The single owner-state surface for the package. Implementation collapses to one owner per axis and one entrypoint family per rail; density means no parallel rails, no near-duplicate shapes, no re-derived logic — a file is as large as its owner's concern requires, never trimmed to a line count. A new feature is a row or case, never a new surface; a public type outside these owner regions is the named defect. `[STATE]` is `FINALIZED` where the owner is a transcription-complete fence with no open gate, `SPIKE` where the owner is fence-complete but its proof carries a residual wheel-floor probe named in the page RESEARCH cluster. This is the ONLY place owner state lives.

| [INDEX] | [AXIS/RAIL]        | [OWNER]          | [KIND]                  | [CASES]                                   | [PAGE#CLUSTER]            |  [STATE]  |
| :-----: | :----------------- | :--------------- | :---------------------- | :---------------------------------------- | :----------------------- | :-------: |
|   [1]   | dataset identity   | `DatasetRef`     | frozen owner + kind row | csv/parquet/arrow/delta/3dm/mesh/hdf      | columnar-query#DATASET   |   SPIKE   |
|   [2]   | scan plan          | `ScanPlan`       | tagged union            | polars-lazy/duckdb/pyarrow-dataset        | columnar-query#SCAN      |   SPIKE   |
|   [3]   | columnar egress    | `ColumnarEgress` | static surface          | arrow/parquet/ipc/lazy-scan               | columnar-query#SCAN      |   SPIKE   |
|   [4]   | query receipt      | `QueryReceipt`   | receipt                 | engine/source/columns/predicate/row-count | columnar-query#SCAN      |   SPIKE   |
|   [5]   | lakehouse          | `Lakehouse`      | static surface + `LakeOp` | write/read/time-travel/optimize/vacuum/CDF | columnar-query#LAKEHOUSE |   SPIKE   |
|   [6]   | query engine       | `QueryEngine`    | tagged union `QuerySpec` | duckdb-sql/relational / narwhals          | columnar-query#QUERY     |   SPIKE   |
|   [7]   | data quality       | `DataQuality`    | rail + `QualityRule`/`SchemaClaim` | IDS-style rules over pandera     | columnar-query#QUALITY   |   SPIKE   |
|   [8]   | frame interop      | `FrameInterop`   | frozen owner            | narwhals backend axis, Arrow C-data rows  | schema-geo#INTEROP       |   SPIKE   |
|   [9]   | frame admission    | `FrameAdmission` | static surface + `FieldShape` | structural field shapes, narwhals admit | schema-geo#ADMISSION    |   SPIKE   |
|  [10]   | vector geospatial  | `VectorGeoClaim` | value object            | crs/units/axis-order/geometry-family      | schema-geo#GEO           |   SPIKE   |
|  [11]   | raster geospatial  | `RasterGeoClaim` | value object            | coverage/band/resampling/nodata           | schema-geo#GEO           |   SPIKE   |
|  [12]   | graph payload      | `GraphPayload`   | frozen owner            | kind/nodes/edges/attrs/directionality     | graph-mesh#GRAPH         | FINALIZED |
|  [13]   | tensor store       | `TensorStore`    | static surface          | zarr/cubed/awkward/icechunk rows          | graph-mesh#TENSOR        |   SPIKE   |
|  [14]   | mesh payload       | `MeshPayload`    | frozen owner            | identity/cell-block/units/metadata        | graph-mesh#MESH          |   SPIKE   |

Every receipt is wired through runtime `ReceiptContributor` and keyed by runtime `ContentIdentity`. `DatasetRef` discriminates by source shape — no `get`/`get_many`/`scan`/`list` family. Vector and raster stay two value objects because band/resampling semantics differ from CRS/axis-order; the backend axis lives on one interop owner with no parallel polars/pandas/pyarrow adapter types.

## [3]-[DEPENDENCY_DIRECTION]

| [INDEX] | [PACKAGE]   | [MAY_REFERENCE_DATA] | [DATA_MAY_REFERENCE] | [BOUNDARY]                                       |
| :-----: | :---------- | :------------------: | :------------------: | :----------------------------------------------- |
|   [1]   | `runtime`   |          no          |         yes          | content key, receipt port, transport consumed inward |
|   [2]   | `compute`   |         yes          |          no          | compute composes data shapes as study inputs     |
|   [3]   | `geometry`  |          no          |          no          | IFC tessellation/registration/topology at `geometry` |
|   [4]   | `artifacts` |          no          |          no          | artifact production stays at `artifacts`         |

`data` consumes runtime `ContentIdentity`, `ReceiptContributor`, and `TransportResource` and never re-mints them. `compute` composes data dataset/array shapes as study inputs without re-cataloguing them. Cross-language consumer projections of the emitted bundles ride the Tier-0 `region-map/seam-splits.md`.

## [4]-[SEAMS]

Every two-folder fact splits by altitude: mechanics live at the named data cluster, consequences land at the consumer. Intra-Python seams ride `pkg/page#CLUSTER`; cross-language consequences ride the Tier-0 `region-map/seam-splits.md` and are referenced as a Tier-0 seam, never restated here.

| [INDEX] | [SEAM]            | [MECHANICS_AT]                    | [CONSEQUENCE_AT]                                                          |
| :-----: | :---------------- | :-------------------------------- | :----------------------------------------------------------------------- |
|   [1]   | content identity  | runtime/content-identity#IDENTITY | columnar-query#SCAN egress and graph-mesh#MESH key by one `ContentIdentity` |
|   [2]   | query receipt     | columnar-query#SCAN               | runtime/observability#RECEIPT wires `QueryReceipt` through `ReceiptContributor` |
|   [3]   | transport sourcing | runtime/resources-lanes#RESOURCE  | ADBC/ConnectorX remote sourcing acquires through `TransportResource`      |
|   [4]   | study input shape | columnar-query#DATASET            | compute/array-solver#ARRAY composes the dataset/array shape, never re-catalogued |
|   [5]   | quality gate      | columnar-query#QUALITY            | schema-geo#ADMISSION routes contract enforcement to one `DataQuality`/`SchemaClaim` |
|   [6]   | mesh exchange     | graph-mesh#MESH                   | mesh-file round-trip stays file exchange; IFC tessellation at geometry/ifc-companion#DAEMON |

## [5]-[BOUNDARIES]

- `data` is not a durable store, schema-migration owner, product repository, query rail, or document-mutation owner; it emits portable import/export bundles.
- IFC tessellation, registration, topology, and AEC geometry belong to `geometry`; the numeric trio and labelled-array compute belong to `compute`; remote-stream transport is a runtime `TransportResource` row.
- Statement carve-outs are named per fence: the engine-dispatch acceptors on `ScanPlan` and `QueryEngine`, the `Lakehouse` lifecycle fold, and the narwhals admission on `FrameAdmission` are the boundary capsules; every other member stays expression-shaped.
- Vector and raster geospatial are two value objects; the geospatial axis is never under-collapsed into one claim.
- Every emitted bundle carries one runtime `ContentIdentity` key; content identity is never re-minted.

## [6]-[PROHIBITIONS]

The closed NEVER list — the deleted patterns the owner registry forecloses.

- NEVER re-mint content identity; the egress bundle key is one runtime `ContentIdentity` key.
- NEVER own a durable store, schema migration, product repository, query rail, or document mutation; data emits portable bundles.
- NEVER own IFC tessellation/registration/topology (geometry), the numeric trio or labelled-array compute (compute), or remote-stream transport (runtime `TransportResource`).
- NEVER create a `get`/`get_many`/`scan`/`list` family; `DatasetRef` discriminates by source shape.
- NEVER under-collapse the geospatial axis into one claim; vector and raster are two value objects because band/resampling differs from CRS/axis-order.
