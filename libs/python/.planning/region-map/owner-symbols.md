# [PYTHON_OWNER_SYMBOLS]

The branch symbol registry. One symbol resolves to one owner; cross-package consumption resolves only against finalized owners. Append-ordered by authoring position within each package. Every type name is unique across the five-package branch.

## [1]-[RUNTIME]

| [SYMBOL]             | [REGION]                    | [KIND]           |
| :------------------- | :-------------------------- | :--------------- |
| `RuntimeContext`     | context-settings#CONTEXT    | frozen owner     |
| `RuntimeProfile`     | context-settings#CONTEXT    | StrEnum vocab    |
| `Correlation`        | context-settings#CONTEXT    | value object     |
| `Deadline`           | context-settings#CONTEXT    | value object     |
| `SettingsAdmission`  | context-settings#SETTINGS   | settings owner   |
| `BoundaryFault`      | rails-resilience#FAULT      | tagged union     |
| `RuntimeRail`        | rails-resilience#FAULT      | rail carrier     |
| `Retry`              | rails-resilience#RESILIENCE | policy table     |
| `ContentIdentity`    | content-identity#IDENTITY   | static surface   |
| `ContentKey`         | content-identity#IDENTITY   | value object     |
| `ResourceRoot`       | resources-lanes#RESOURCE    | frozen owner     |
| `ResourceRef`        | resources-lanes#RESOURCE    | value object     |
| `TransportResource`  | resources-lanes#RESOURCE    | tagged union     |
| `LanePolicy`         | resources-lanes#LANE        | frozen owner     |
| `DrainReceipt`       | resources-lanes#LANE        | receipt          |
| `StagePlan`          | resources-lanes#LANE        | DAG owner        |
| `Receipt`            | observability#RECEIPT       | receipt union    |
| `ReceiptContributor` | observability#RECEIPT       | Protocol port    |
| `Redaction`          | observability#RECEIPT       | policy           |
| `ServerHost`         | server-host#SERVE           | boundary capsule |
| `Credential`         | server-host#SERVE           | tagged union     |
| `ApiPackage`         | evidence#API                | record           |
| `ApiMember`          | evidence#API                | record           |
| `Entrypoint`         | evidence#ENTRY              | command grammar  |

## [2]-[DATA]

| [SYMBOL]         | [REGION]               | [KIND]         |
| :--------------- | :--------------------- | :------------- |
| `DatasetRef`     | columnar-query#DATASET | frozen owner   |
| `DatasetKind`    | columnar-query#DATASET | StrEnum vocab  |
| `ScanPlan`       | columnar-query#SCAN    | frozen owner   |
| `ColumnarEgress` | columnar-query#SCAN    | static surface |
| `QueryReceipt`   | columnar-query#SCAN    | receipt        |
| `SchemaClaim`    | schema-geo#SCHEMA      | value object   |
| `FrameAdmission` | schema-geo#SCHEMA      | static surface |
| `ContractGate`   | schema-geo#SCHEMA      | rail           |
| `VectorGeoClaim` | schema-geo#GEO         | value object   |
| `SpatialEgress`  | schema-geo#GEO         | static surface |
| `RasterGeoClaim` | schema-geo#GEO         | value object   |
| `GraphPayload`   | graph-mesh#GRAPH       | frozen owner   |
| `GraphEgress`    | graph-mesh#GRAPH       | static surface |
| `MeshPayload`    | graph-mesh#MESH        | frozen owner   |

## [3]-[GEOMETRY]

| [SYMBOL]          | [REGION]                     | [KIND]           |
| :---------------- | :--------------------------- | :--------------- |
| `IfcCompanion`    | ifc-companion#DAEMON         | boundary capsule |
| `IfcAnalysis`     | ifc-analysis#ANALYSIS        | static surface   |
| `ScanProcessing`  | scan-processing#REGISTRATION | frozen owner     |
| `GeometryAlgebra` | geometry-algebra#ALGEBRA     | tagged union     |

## [4]-[COMPUTE]

| [SYMBOL]             | [REGION]              | [KIND]         |
| :------------------- | :-------------------- | :------------- |
| `ArrayPayload`       | array-solver#ARRAY    | frozen owner   |
| `NamedAxis`          | array-solver#ARRAY    | value object   |
| `NumericIntent`      | array-solver#SOLVER   | tagged union   |
| `SymbolicDerivation` | array-solver#SOLVER   | static surface |
| `QuantityClaim`      | units-study#QUANTITY  | value object   |
| `StudyPlan`          | units-study#STUDY     | frozen owner   |
| `RunHistory`         | units-study#STUDY     | static surface |
| `ModelAsset`         | units-study#MODEL     | frozen owner   |
| `ModelAssetManifest` | units-study#MODEL     | value object   |
| `GraduationReceipt`  | graduation#GRADUATION | frozen owner   |
| `HandoffAxis`        | graduation#GRADUATION | Literal union  |

## [5]-[ARTIFACTS]

| [SYMBOL]          | [REGION]                  | [KIND]         |
| :---------------- | :------------------------ | :------------- |
| `DocumentPlan`    | documents#DOCUMENT        | tagged union   |
| `ReportPlan`      | documents#REPORT          | frozen owner   |
| `VisualSpec`      | visual-export#VISUAL      | tagged union   |
| `ExportPlan`      | visual-export#EXPORT      | frozen owner   |
| `Preview`         | visual-export#PREVIEW     | static surface |
| `Compression`     | visual-export#COMPRESSION | StrEnum vocab  |
| `ArtifactReceipt` | documents#RECEIPT         | receipt union  |

## [6]-[PROPOSED] — next-loop ideation owner symbols (2026-06-16)

Reserved by the cross-lens + frontier-atlas ideation pass (`.artifacts/planning-briefs/python-ideation.md`, PY1-PY26). State `[PROPOSED]` is distinct from a finalized owner: cross-package consumption resolves only against finalized owners, so these reserve the symbol/region/kind triple now and finalize when their TASKLOG row admits the page. Each is unique across the five-package branch.

| [SYMBOL]            | [REGION]                  | [KIND]         | [TASKLOG]        |
| :------------------ | :------------------------ | :------------- | :--------------- |
| `SolverReceipt`     | array-solver#SOLVER       | receipt union  | `PY_SOLVE_001`   |
| `SensitivityReceipt`| units-study#STUDY         | receipt        | `PY_STUDY_001`   |
| `StudyReceipt`      | units-study#STUDY         | receipt        | `PY_STUDY_001`   |
| `UncertaintyReceipt`| units-study#QUANTITY      | receipt        | `PY_QUANT_001`   |
| `SpectralReceipt`   | array-solver#SOLVER       | receipt        | `PY_SIG_001`     |
| `EquivalenceLaw`    | array-solver#SOLVER       | proof rail     | `PY_CODEGEN_001` |
| `TableOp`           | lakehouse#TABLE           | tagged union   | `PY_LAKE_001`    |
| `QualityProfile`    | quality#PROFILE           | rail           | `PY_QUAL_001`    |
| `StandingQuery`     | columnar-query#QUERY      | frozen owner   | `PY_QUERY_001`   |
| `FusionRank`        | columnar-query#QUERY      | static surface | `PY_QUERY_001`   |
| `CubeSnapshot`      | tensorstore#CUBE          | receipt        | `PY_TENSOR_001`  |
| `SensitivityField`  | differentiable#INVERSE    | receipt        | `PY_DIFF_001`    |
| `Enclosure`         | rigor#CERTIFY             | receipt        | `PY_RIGOR_001`   |
| `InteropField`      | simframe#INTEROP          | receipt        | `PY_SIM_001`     |
| `AssemblyReceipt`   | simframe#ASSEMBLY         | receipt        | `PY_SIM_001`     |
| `InSituFrame`       | simframe#ASSEMBLY         | provenance edge| `PY_SIM_001`     |
| `ConstraintReceipt` | parametric#CONSTRAIN      | receipt        | `PY_PARA_001`    |
| `GeometryDelta`     | parametric#VERSION        | tagged union   | `PY_PARA_001`    |
| `DocumentModel`     | docmodel#MODEL            | tagged union   | `PY_DOC_002`     |
| `DocumentDelta`     | docmodel#MODEL            | tagged union   | `PY_DOC_002`     |
| `ConformanceGrade`  | conformance#GRADE         | graded property| `PY_CONF_001`    |
| `BundleManifest`    | conformance#SEAL          | value object   | `PY_CONF_001`    |
| `ColorSpace`        | color#GAMUT               | value object   | `PY_COLOR_001`   |
| `CapabilityCatalog` | capabilities#CATALOG      | static surface | `PY_CAP_001`     |
| `Trigger`           | resources-lanes#LANE      | tagged union   | `PY_ORCH_001`    |
| `NotebookRun`       | notebook#RUN              | frozen owner   | `PY_NB_001`      |
| `SpatialIndex`      | array-solver#SPATIAL      | static surface | `PY_SPATIAL_001` |
| `ImageField`        | imaging#IMAGE             | frozen owner   | `PY_IMG_001`     |

The new polymorphic entrypoints `analyze`/`inverse`/`certify`/`cube`/`constrain` are route methods on their owner (the solver family, `differentiable`, `rigor`, `tensorstore`, `parametric`), not standalone symbols — one polymorphic dispatch surface per concept, never a sibling. `LinearProvider` and `Substrate` are orthogonal policy-row vocabularies on `NumericIntent`/`StudyPlan`, not new owners.

## [7]-[COLLAPSE_RECORD]

Symbols dropped by the final-topology collapse, recorded so they are never re-added:
- `ExchangeBundle` (data), `ContentDigest` + `ArtifactBundle` (artifacts), and the companion GLB key — collapsed into runtime `ContentIdentity` + `ContentKey`; the three parallel content-identity owners are one.
- `SolverPlan` (compute) — folded into `NumericIntent` route discrimination.
- `GeoClaim` (data) — split into `VectorGeoClaim` + `RasterGeoClaim` (band/resampling semantics differ from vector CRS/axis-order).
- The data, compute, geometry, and artifacts typed receipts wire through the single runtime `ReceiptContributor` port, never four parallel receipt rails.
