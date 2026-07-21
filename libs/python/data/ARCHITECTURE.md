# [PY_DATA_ARCHITECTURE]

`data` maps host-free data interchange onto one module per domain concept, each closing its whole concern behind a single polymorphic owner. A `tabular` interchange core carries the columnar, lakehouse, query, materialize, contract, interop, and egress spine, and the `spatial`, `gridded`, `graph`, and `impact` planes each own a distinct domain. Every `from rasm.data.*` import binds a strictly-earlier module, so the module set is a provable acyclic DAG; `[02]-[SEAMS]` records only the cross-`libs/` and cross-language crossings, never an intra-`data` composition.

## [01]-[DOMAIN_MAP]

```text codemap
data/
├── tabular/              # Columnar, relational, and lakehouse interchange plane and its object-store egress
│   ├── interop.py        # Backend-agnostic frame owner, FieldShape minter, and Arrow zero-copy carrier
│   ├── columnar.py       # Dataset-ref owner and the one request-scoped DuckDbSession scan rail
│   ├── lakehouse.py      # Lakehouse owner over the LakeOp lifecycle and table-format bindings
│   ├── query.py          # Query engine folding every QuerySpec frontend to uniform Arrow
│   ├── materialize.py    # CDC materialization composing lakehouse, query, and columnar downward
│   ├── contract.py       # Structural admission, covenant, and quality gate folded on one ContractClaim
│   ├── profile.py        # Quality-profile owner grading a frame the artifacts renderer renders
│   ├── egress.py         # Object-store egress owner over one StoreOp axis keyed by content identity
│   └── cost.py           # Cost ledger folding the receipt families into the priced tenant-attributed frame
├── spatial/              # Vector and raster claims, the DuckDB-spatial engine, the DGG plane, STAC catalog, mesh exchange
│   ├── geospatial.py     # Vector and raster geo claims over the VectorOp and RasterOp axes
│   ├── query.py          # DuckDB-spatial join, transform, and H3 engine on the shared session rail
│   ├── grid.py           # GridSystem DGG plane and frame-native geometry algebra
│   ├── catalog.py        # StacCatalog owner over search, item table, and asset-href egress
│   └── mesh.py           # Mesh-file exchange owner over the backend axis and point-cloud row
├── gridded/              # Chunked N-D dense, virtual, and ragged tensor stores with the CF labelled-field store
│   ├── store.py          # Dense chunked N-D tensor store over the TensorBackend engines and the bounded-memory cubed plan
│   ├── virtual.py        # Sole manifest-cube owner over manifest write, registration, and the icechunk version-operation axis
│   ├── ragged.py         # Ragged N-D store over awkward with a zero-copy Arrow bridge
│   └── field.py          # CF field-dataset owner over the CF engines and grouped reductions
├── graph/                # Rustworkx graph payloads with a networkx codec lane and typed receipts
│   └── graph.py          # Graph-payload owner over the run kernel and the community-detection split
└── impact/               # Material environmental impact: EPD ingest and LCA compute on one EN 15804 carrier
    └── impact.py         # MaterialImpact owner folding the ImpactSource axis into one EN 15804 matrix
```

## [02]-[SEAMS]

```mermaid
---
config:
  layout: elk
  flowchart:
    curve: linear
    padding: 25
---
flowchart LR
    accTitle: Data package Python-peer seam registry
    accDescr: Data sub-domain owners exchanging content identity, transport, receipts, wires, and frame shapes with the Python runtime, artifacts, geometry, and compute siblings.
    subgraph data[DATA]
        Tabular[Tabular interchange]
        Egress[Object egress]
        Query[Query engine]
        Materialize[CDC materialize]
        Profile[Quality profile]
        Cost[Cost ledger]
        Geospatial[Geospatial claims]
        Catalog[STAC catalog]
        Mesh[Mesh exchange]
        Gridded[Gridded tensors]
        Impact[Material impact]
    end
    Runtime{{python:runtime}}
    Artifacts{{python:artifacts}}
    Geometry{{python:geometry}}
    Compute([python:compute])
    Egress e1@-->|"[CONTENT_KEY]: ContentIdentity"| Runtime
    Mesh e2@-->|"[CONTENT_KEY]: ContentIdentity"| Runtime
    Query e3@-->|"[RECEIPT]: QueryReceipt"| Runtime
    Gridded e19@-->|"[RECEIPT]: TensorReceipt"| Runtime
    Runtime e4@-->|"[TRANSPORT]: ResourceRef"| Tabular
    Runtime e20@-->|"[TRANSPORT]: ResourceRef"| Egress
    Runtime e21@-->|"[TRANSPORT]: ResourceRef"| Gridded
    Runtime e22@-->|"[TRANSPORT]: ResourceRef"| Mesh
    Runtime e23@-->|"[TRANSPORT]: ResourceRef"| Catalog
    Runtime e5@-->|"[TRANSPORT]: TransportResource"| Impact
    Artifacts e6@-->|"[WIRE]: CorpusRow"| Tabular
    Profile e7@-->|"[SHAPE]: QualityProfile"| Artifacts
    Artifacts e8@-->|"[WIRE]: GeoJSON"| Geospatial
    Mesh e9@-->|"[SHAPE]: MeshPayload"| Geometry
    Mesh e15@-->|"[SHAPE]: PointRecordTable"| Geometry
    Geometry e16@-->|"[BOUNDARY]: Trimesh"| Mesh
    Tabular e10@-->|"[SHAPE]: FrameAdmission"| Compute
    Runtime e11@-->|"[BOUNDARY]: on_thread"| Query
    Runtime e12@-->|"[BOUNDARY]: on_thread"| Geospatial
    Runtime e13@-->|"[BOUNDARY]: on_thread"| Mesh
    Runtime e14@-->|"[BOUNDARY]: on_thread"| Profile
    Runtime e24@-->|"[BOUNDARY]: on_thread"| Impact
    Runtime e17@-->|"[BOUNDARY]: LanePolicy"| Materialize
    Runtime e18@-->|"[BOUNDARY]: on_thread"| Catalog
    Cost e25@-->|"[SHAPE]: CostFrame"| Artifacts
    class Tabular,Egress,Query,Materialize,Profile,Cost,Geospatial,Catalog,Mesh,Gridded,Impact primary
    class Runtime data
    class Artifacts,Geometry,Compute annotation
    class e1,e2,e6,e8 edgeData
    class e3,e7,e9,e10,e15,e19,e25 edgeSuccess
    class e4,e5,e11,e12,e13,e14,e16,e17,e18,e20,e21,e22,e23,e24 edgeControl
    classDef primary fill:#44475A,stroke:#FF79C6,color:#F8F8F2
    classDef data fill:#FFB86CBF,stroke:#FFB86C,color:#282A36
    classDef annotation fill:#21222C,stroke:#6272A4,color:#F8F8F2
    classDef edgeData stroke:#FFB86C,color:#F8F8F2
    classDef edgeSuccess stroke:#50FA7B,color:#F8F8F2
    classDef edgeControl stroke:#FF79C6,color:#F8F8F2
```

```mermaid
---
config:
  layout: elk
  flowchart:
    curve: linear
    padding: 25
---
flowchart LR
    accTitle: Data package C#-peer seam registry
    accDescr: Data sub-domain owners exchanging frame shapes, content keys, plan wires, and the environmental set with the Rasm.Compute, Persistence, Materials, and Bim C# peers.
    subgraph data[DATA]
        Tabular[Tabular interchange]
        Query[Query engine]
        Geospatial[Geospatial claims]
        Virtual[Manifest cube]
        Impact[Material impact]
    end
    Persistence[(Rasm.Persistence)]
    Compute([Rasm.Compute])
    Materials([Rasm.Materials])
    Bim([Rasm.Bim])
    Compute e1@-->|"[SHAPE]: DoeDataset"| Tabular
    Geospatial e2@-->|"[SHAPE]: GeoArrow"| Compute
    Tabular e3@-->|"[CONTENT_KEY]: ContentKey"| Persistence
    Query e4@<-->|"[WIRE]: SubstraitPlan"| Persistence
    Virtual e5@-->|"[CONTENT_KEY]: ContentKey"| Persistence
    Impact e6@<-->|"[CONTENT_KEY]: ContentKey"| Persistence
    Impact e7@-->|"[WIRE]: Assessment"| Materials
    Bim e8@<-->|"[WIRE]: GeoFeatureWkb"| Geospatial
    Persistence e9@-->|"[WIRE]: FlightTicket"| Query
    class Tabular,Query,Geospatial,Virtual,Impact primary
    class Persistence data
    class Compute,Materials,Bim annotation
    class e1,e2 edgeSuccess
    class e3,e4,e5,e6,e7,e8,e9 edgeData
    classDef primary fill:#44475A,stroke:#FF79C6,color:#F8F8F2
    classDef data fill:#FFB86CBF,stroke:#FFB86C,color:#282A36
    classDef annotation fill:#21222C,stroke:#6272A4,color:#F8F8F2
    classDef edgeData stroke:#FFB86C,color:#F8F8F2
    classDef edgeSuccess stroke:#50FA7B,color:#F8F8F2
```

Two fences partition by peer runtime: the Python siblings carry the in-process content, transport, receipt, and frame contracts, and the C# peers carry the cross-runtime wire, durable content keys, and the environmental set. Each collapsed edge stands for every contract at that kind between the two owners, and the owning pages enumerate the rest. `GeoFeatureWkb` spells from its Rasm.Bim owner; the crossing carries raw WKB — `GeoDataFrame.to_wkb` outbound, `ST_GeomFromWKB` on admission — and no data-interior type re-mints the label.

An intra-`data` relation is composition, never a seam; `[03]-[INTERNAL]` renders the acyclic import DAG this registry excludes.

Every `[CONTENT_KEY]` edge derives one typed identity through the runtime `ContentIdentity` primitive over the public `arrow_bytes` fold, never a per-page hash, and each crossing agrees with its counterpart page verbatim. A single-sided edge is declared on the producing side and binds its counterpart when that page lands its mirror row.

## [03]-[INTERNAL]

- S0 `tabular` — interop and columnar form the floor; contract, profile, query, lakehouse, and egress own independent branches; materialize closes the operational apex and folds every hook point through one scope-keyed registration rail; cost closes the evidence apex by folding sibling receipt families into the priced frame.
- S1 `gridded` + `impact` — gridded rides the interop carrier (`ArrowCStream`) for its ragged Arrow bridge, and its virtual owner mints the field owner's `FieldReceipt` family downward inside the folder; the tensor `PlanReceipt` lowering crosses into the tabular cost ledger as wire data, never an import; impact composes the contract, profile, interop, and columnar rows (`FrameAdmission`, `QualityProfile`, `FrameInterop`, `arrow_bytes`).
- S1 `graph` — import-isolated: composes runtime alone, and its `GraphResult.frame` node table crosses into columnar as wire data over the pyarrow left-outer join, never an import.
- S2 `spatial` — the apex consumer: composes columnar (`QueryReceipt`), the object egress (`ObjectEgress`/`StoreOp`), and gridded's virtual plane (`FieldVirtual`, `VirtualReference`).

```mermaid
---
config:
  layout: elk
  flowchart:
    curve: linear
    padding: 25
---
flowchart TB
    accTitle: Data interior import strata
    accDescr: Spatial composes the middle tier over the tabular floor; labeled edges name imported owners, and dashed edges carry wire data.
    subgraph D2["S2 SPATIAL"]
        Spatial[spatial]
    end
    subgraph D1["S1 GRIDDED + IMPACT + GRAPH"]
        Graph[graph]
        Impact[impact]
        Gridded[gridded]
    end
    subgraph D0["S0 TABULAR"]
        Cost[cost]
        Egress[egress]
        Materialize[materialize]
        Query[query]
        Lakehouse[lakehouse]
        Columnar[columnar]
        Contract[contract]
        Profile[profile]
        Interop[interop]
    end
    Spatial s1@-->|"[IMPORT]: QueryReceipt"| Columnar
    Spatial s2@-->|"[IMPORT]: StoreOp"| Egress
    Spatial s3@-->|"[IMPORT]: FieldVirtual"| Gridded
    Gridded s4@-->|"[IMPORT]: ArrowCStream"| Interop
    Impact s5@-->|"[IMPORT]: FrameAdmission"| Contract
    Impact s6@-->|"[IMPORT]: QualityProfile"| Profile
    Impact s7@-->|"[IMPORT]: FrameInterop"| Interop
    Impact s8@-->|"[IMPORT]: arrow_bytes"| Columnar
    Graph s9@-.->|"[WIRE]: GraphResult"| Columnar
    Materialize s10@-->|"[IMPORT]: QuerySpec"| Query
    Materialize s11@-->|"[IMPORT]: TableFormat"| Lakehouse
    Materialize s12@-->|"[IMPORT]: LAKE_COMMIT_POINT"| Lakehouse
    Materialize s13@-->|"[IMPORT]: VERDICT_POINT"| Contract
    Materialize s14@-->|"[IMPORT]: PUT_POINT"| Egress
    Materialize s15@-->|"[IMPORT]: DELETE_POINT"| Egress
    Query s16@-->|"[IMPORT]: DuckDbSession"| Columnar
    Lakehouse s17@-->|"[IMPORT]: DatasetRef"| Columnar
    Contract s18@-->|"[IMPORT]: FrameInterop"| Interop
    Profile s19@-->|"[IMPORT]: FieldShape"| Interop
    Cost s20@-->|"[IMPORT]: QueryReceipt"| Columnar
    Cost s21@-->|"[IMPORT]: LakeReceipt"| Lakehouse
    Cost s22@-->|"[IMPORT]: PartitionBundle"| Materialize
    Cost s23@-->|"[IMPORT]: EgressReceipt"| Egress
    Gridded s24@-.->|"[WIRE]: PlanReceipt"| Cost
    Spatial ~~~ Impact
    Spatial ~~~ Graph
    Interop f1@-->|"forbidden: upward import"| D2
    class Spatial,Graph,Impact,Gridded primary
    class Cost,Egress,Materialize,Query,Lakehouse,Columnar,Contract,Profile,Interop recessed
    class s1,s2,s3,s4,s5,s6,s7,s8,s10,s11,s12,s13,s14,s15,s16,s17,s18,s19,s20,s21,s22,s23 edgeControl
    class s9,s24 edgeData
    class f1 edgeError
    classDef primary fill:#44475A,stroke:#FF79C6,color:#F8F8F2
    classDef recessed fill:#21222C,stroke:#6272A4,color:#F8F8F2
    classDef edgeControl stroke:#FF79C6,color:#F8F8F2
    classDef edgeData stroke:#FFB86C,color:#F8F8F2
    classDef edgeError stroke:#FF5555,stroke-width:3px,color:#F8F8F2
```
