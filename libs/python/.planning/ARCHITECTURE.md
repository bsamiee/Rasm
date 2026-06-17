# [PYTHON_BRANCH_ARCHITECTURE]

The Python branch atlas: the source tree and build order across the five packages, the inter-package dependency graph, and the intra-branch cross-folder seams. Per-folder owner registries (the one owner-state surface) live on each package `ARCHITECTURE.md`; this page carries only the branch-altitude topology. Cross-language consequences ride the Tier-0 `region-map/seam-splits.md` and are referenced as Tier-0 seams, never restated here.

## [1]-[SOURCE_TREE]

The package layout IS the branch build order: `runtime` is the foundation every sibling consumes, authored first as settled vocabulary; the remaining four author against the runtime ports and the ledger's pinned seam contracts. Within a package, the file layout is the package build order on its own `ARCHITECTURE.md`. Each package folder carries `README.md`, `.planning/` design pages, `.api/` catalogues, and the future source root directly under the package.

```text codemap
libs/python/
├── .planning/                 # branch charter, atlas, forward pool, open work, .api governance, region ledger
│   ├── README.md              # [PYTHON_BRANCH] charter + topology + TEST_POLICY
│   ├── ARCHITECTURE.md        # this atlas — source tree, dependency direction, seams
│   ├── IDEAS.md               # [PYTHON_IDEAS] forward pool + concert + folder horizons
│   ├── TASKLOG.md             # open work — manifest floors, .api gaps, page grades
│   ├── api-catalogues.md      # .api evidence protocol + distribution→owner routing
│   └── region-map/
│       └── seam-splits.md     # intra-branch cross-folder seam ledger
├── runtime/                   # FOUNDATION (>=3.15 core; companion-floor server leg)
│   # context/settings, fault+rail, content identity, resources/lanes, receipts, ServerHost, evidence, Entrypoint
├── compute/                   # offline scientific evidence that graduates (>=3.15 core; marker-floor rows)
│   # ArrayPayload, NumericIntent solver, units/study/model, GraduationReceipt + HandoffAxis, StubCodegen, Inference
├── data/                      # portable data interchange (>=3.15 core; cp315-wheel-gated rows)
│   # DatasetRef, ScanPlan/Lakehouse/QueryEngine/DataQuality, FrameInterop/geospatial, GraphPayload/TensorStore/MeshPayload
├── geometry/                  # geometry + IFC interchange, load-bearing companion (python_version<'3.13')
│   # IfcCompanion daemon, IfcAnalysis, ScanProcessing, GeometryAlgebra
└── artifacts/                 # artifact production (>=3.15 core; image-toolchain + native-VTK rows)
    # DocumentPlan/ReportPlan/ArtifactReceipt, VisualSpec/ExportPlan/Preview/Compression
```

`runtime` lands first because every sibling consumes its `ContentIdentity`, `BoundaryFault`/`RuntimeRail`, `Retry`, `ResourceRoot`, `LanePolicy`, `ReceiptContributor`, and `ServerHost`/`Credential` ports as settled vocabulary. `compute`, `data`, `geometry`, and `artifacts` author against those ports and the pinned seam contracts; no sibling owner enters the runtime graph. The geometry companion daemon and the runtime server-host are the offline/companion legs that ride the `python_version<'3.13'` floor.

## [2]-[DEPENDENCY_DIRECTION]

`runtime` is the foundation every package composes and references nothing first-wave; the other four consume runtime ports inward and never re-mint them. `compute` additionally composes `data` dataset/array shapes as study inputs and accepts `geometry` evidence through the graduation `HandoffAxis` geometry case, but imports no sibling interior beyond those two named seams. No package imports another package's interior except along a listed seam.

| [INDEX] | [PACKAGE]   | [DEPENDS_ON]                                                          | [DEPENDED_BY]                          |
| :-----: | :---------- | :------------------------------------------------------------------- | :------------------------------------- |
|   [1]   | `runtime`   | none (first-wave foundation; references no sibling)                  | `compute`, `data`, `geometry`, `artifacts` |
|   [2]   | `data`      | `runtime` (`ContentIdentity`, `ReceiptContributor`, `TransportResource`) | `compute` (dataset/array shapes as study inputs) |
|   [3]   | `geometry`  | `runtime` (`ServerHost`, `ContentIdentity`, rails, lanes, `ReceiptContributor`) | `compute` (evidence via `HandoffAxis` geometry case) |
|   [4]   | `compute`   | `runtime` (`ContentIdentity`, `ReceiptContributor`), `data` (dataset/array shapes), `geometry` (evidence via `HandoffAxis`) | none |
|   [5]   | `artifacts` | `runtime` (`ContentIdentity`, `ReceiptContributor`)                  | none |

The geometry companion daemon (served through the runtime `ServerHost`) and the runtime server-host are the offline/companion legs riding the `python_version<'3.13'` floor; the rest of the branch is `>=3.15` core. No Python package imports a managed-host interior, product host lifecycle, durable store, bridge lifecycle, or web UI state; every such coupling rides the Tier-0 ledger as a Tier-0 seam.

## [3]-[SEAMS]

Every two-folder fact splits by altitude: mechanics at the owning page, consequence at the consumer, in `pkg/page#CLUSTER` notation. A seam re-taught instead of consumed is the named drift defect repaired by routing to the owner. The companion gRPC wire, the content-identity seed parity, the two-hop IFC tessellation, and the graduation-evidence seam are cross-language and live in the Tier-0 ledger; this page carries only the intra-Python legs.

| [INDEX] | [SEAM]              | [MECHANICS_AT]                       | [CONSEQUENCE_AT]                                                                                  |
| :-----: | :------------------ | :----------------------------------- | :----------------------------------------------------------------------------------------------- |
|   [1]   | content identity    | `runtime/content-identity#IDENTITY`  | `data/columnar-query#SCAN`, `geometry/ifc-companion#DAEMON`, `artifacts/documents#RECEIPT` key by one `ContentIdentity`; never re-minted |
|   [2]   | contributor port    | `runtime/observability#RECEIPT`      | `data/columnar-query#SCAN`, `compute/graduation#GRADUATION`, `geometry/ifc-analysis#ANALYSIS`, `artifacts/documents#RECEIPT` typed receipts wire through one `ReceiptContributor` |
|   [3]   | boundary-fault rail | `runtime/rails-resilience#FAULT`     | every sibling raises through `BoundaryFault` and returns the `RuntimeRail` carrier               |
|   [4]   | resilience policy   | `runtime/rails-resilience#RESILIENCE`| sibling and transport retry ride one `Retry` table; never a second retry owner                   |
|   [5]   | companion serve     | `runtime/server-host#SERVE`          | `geometry/ifc-companion#DAEMON` hosts its tessellation daemon through `ServerHost`; never a second wire vocabulary |
|   [6]   | transport resource  | `runtime/resources-lanes#RESOURCE`   | `data/graph-mesh#MESH` and AEC remote streams acquire through `TransportResource`; never a second transport |
|   [7]   | concurrency lanes   | `runtime/resources-lanes#LANE`       | sibling bounded fan-out rides `LanePolicy` task groups and `DrainReceipt`                        |
|   [8]   | labelled-array catalogue | `data/.api` (`xarray`, `dask`)  | `compute/array-solver#ARRAY` composes the data catalogues as study-input shapes; deletes its duplicate stubs |
|   [9]   | study-input shape   | `data/columnar-query#DATASET`        | `compute/array-solver#ARRAY` composes the dataset/array shape, never re-catalogued               |
|  [10]   | mesh-file exchange  | `data/graph-mesh#MESH`               | `geometry` consumes mesh-file shapes as inputs; the IFC-to-GLB rail stays in `geometry`          |
|  [11]   | geometry graduation | `compute/graduation#GRADUATION`      | `geometry/scan-processing#REGISTRATION` and `geometry/geometry-algebra#ALGEBRA` evidence reaches the managed owner system through the `HandoffAxis` geometry case |
|  [12]   | report figure bind  | `artifacts/documents#REPORT`         | `artifacts/visual-export#VISUAL` outputs bind into the report-templating document tree           |
