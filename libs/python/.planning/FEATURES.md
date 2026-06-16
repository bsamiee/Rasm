# [PYTHON_FEATURES]

The branch capability atlas. Rows name capability, owner, and the page#cluster that owns it — no state (state lives on each package charter DENSITY_BAR), no case counts, no owner-symbol chains. The atlas is the idea reservoir: every capability rides one owner axis, and a new capability is a row, a case, or a policy value on an existing owner, never a sibling surface.

## [1]-[RUNTIME]

| [FEATURE]                                 | [OWNER]             | [PAGE#CLUSTER]              |
| :---------------------------------------- | :------------------ | :-------------------------- |
| caller-owned context admission            | `RuntimeContext`    | context-settings#CONTEXT    |
| profile-keyed policy rows                 | `RuntimeProfile`    | context-settings#CONTEXT    |
| validated local settings source order     | `SettingsAdmission` | context-settings#SETTINGS   |
| one boundary-fault tagged union           | `BoundaryFault`     | rails-resilience#FAULT      |
| Result/Option ROP rail                    | `RuntimeRail`       | rails-resilience#FAULT      |
| one retry policy table over stamina       | `Retry`             | rails-resilience#RESILIENCE |
| one XxHash128 content identity            | `ContentIdentity`   | content-identity#IDENTITY   |
| resource roots over fsspec/UPath          | `ResourceRoot`      | resources-lanes#RESOURCE    |
| http/ssh/speckle transport                | `TransportResource` | resources-lanes#RESOURCE    |
| bounded anyio lanes + drain receipts      | `LanePolicy`        | resources-lanes#LANE        |
| multi-stage DAG orchestration             | `StagePlan`         | resources-lanes#LANE        |
| local receipts + contributor port         | `Receipt`           | observability#RECEIPT       |
| structlog/OTel/psutil signals + redaction | `Signals`           | observability#RECEIPT       |
| inbound companion gRPC server lifecycle   | `ServerHost`        | server-host#SERVE           |
| credential axis (token/keyring/loopback)  | `Credential`        | server-host#SERVE           |
| API + structural-parsing evidence         | `ApiPackage`        | evidence#API                |
| private companion entrypoint grammar      | `Entrypoint`        | evidence#ENTRY              |

## [2]-[DATA]

| [FEATURE]                                | [OWNER]          | [PAGE#CLUSTER]         |
| :--------------------------------------- | :--------------- | :--------------------- |
| polymorphic dataset refs by source shape | `DatasetRef`     | columnar-query#DATASET |
| cross-engine lazy/streaming scans        | `ScanPlan`       | columnar-query#SCAN    |
| typed columnar egress + query receipt    | `ColumnarEgress` | columnar-query#SCAN    |
| schema claims + data-contract gate       | `ContractGate`   | schema-geo#SCHEMA      |
| vector geospatial (CRS/axis-order)       | `VectorGeoClaim` | schema-geo#GEO         |
| raster geospatial (band/resampling)      | `RasterGeoClaim` | schema-geo#GEO         |
| graph payloads + algorithms              | `GraphPayload`   | graph-mesh#GRAPH       |
| mesh-file exchange                       | `MeshPayload`    | graph-mesh#MESH        |

## [3]-[GEOMETRY]

| [FEATURE]                                 | [OWNER]           | [PAGE#CLUSTER]               |
| :---------------------------------------- | :---------------- | :--------------------------- |
| IFC to mesh/GLB tessellation daemon       | `IfcCompanion`    | ifc-companion#DAEMON         |
| IFC QTO/clash/space-program/IDS analysis  | `IfcAnalysis`     | ifc-analysis#ANALYSIS        |
| point-cloud registration + reconstruction | `ScanProcessing`  | scan-processing#REGISTRATION |
| non-manifold topology + AEC geometry      | `GeometryAlgebra` | geometry-algebra#ALGEBRA     |

## [4]-[COMPUTE]

| [FEATURE]                                 | [OWNER]              | [PAGE#CLUSTER]        |
| :---------------------------------------- | :------------------- | :-------------------- |
| array admission + finite policy           | `ArrayPayload`       | array-solver#ARRAY    |
| route-discriminated solver + accelerators | `NumericIntent`      | array-solver#SOLVER   |
| symbolic derivation + C# codegen handoff  | `SymbolicDerivation` | array-solver#SOLVER   |
| unit + uncertainty quantity claims        | `QuantityClaim`      | units-study#QUANTITY  |
| study + benchmark mode + run history      | `StudyPlan`          | units-study#STUDY     |
| model-asset validation + sklearn-to-ONNX  | `ModelAsset`         | units-study#MODEL     |
| C# graduation receipt + geometry handoff  | `GraduationReceipt`  | graduation#GRADUATION |

## [5]-[ARTIFACTS]

| [FEATURE]                                 | [OWNER]           | [PAGE#CLUSTER]            |
| :---------------------------------------- | :---------------- | :------------------------ |
| document/PDF/Office/structured-text plan  | `DocumentPlan`    | documents#DOCUMENT        |
| report-templating composition over jinja2 | `ReportPlan`      | documents#REPORT          |
| 2D charts + 3D scientific visualization   | `VisualSpec`      | visual-export#VISUAL      |
| visual export backends                    | `ExportPlan`      | visual-export#EXPORT      |
| image/preview/media detection             | `Preview`         | visual-export#PREVIEW     |
| algorithm-row compression                 | `Compression`     | visual-export#COMPRESSION |
| kind-discriminated artifact receipt       | `ArtifactReceipt` | documents#RECEIPT         |

## [6]-[CROSS_BOUNDARY]

| [FEATURE]                                  | [OWNER]             | [PAGE#CLUSTER]            |
| :----------------------------------------- | :------------------ | :------------------------ |
| companion gRPC over the C# contract        | `IfcCompanion`      | ifc-companion#DAEMON      |
| content-key cache-hit-by-reference         | `ContentIdentity`   | content-identity#IDENTITY |
| geometry evidence into the C# owner system | `GraduationReceipt` | graduation#GRADUATION     |

## [7]-[IDEATION_2026-06-16] — net-new ambition concepts (next-loop pool)

Net-new high-value capability concepts surfaced by the cross-lens + frontier-atlas ideation pass, ranked in `.artifacts/planning-briefs/python-ideation.md` (PY1-PY26). Each rides ONE owner axis — a new owned page or a deepened existing owner — never a sibling surface. Concepts the corpus already owns world-class are excluded. AI/ML/generative-design is out of scope; `ModelAsset` is reused only for non-AI surrogate/classification consumers — NOTE the GP/PCE/RBF study surrogates are scikit-learn/scipy-native and live on the `study` page, NOT on `ModelAsset` (which owns only ONNX-shaped assets). Owner symbols, pages, clusters, seams, and distribution routing for every row are registered `[PROPOSED]` in `region-map/{owner-symbols.md,page-regions.md,seam-splits.md,api-owners.md}`; the new pins are gated on `PY_FLOOR_003`. Eight rows collide with a named C# V-concept — each carries a boundary verb (CONSUMES/DISJOINT/EXTENDS/PEER-PLANE) at `python-ideation.md#[1.1]` and `seam-splits.md#[5]` so the not-a-mirror claim is discharged.

| [CONCEPT]                                          | [OWNER / PAGE]                       | [VALUE]                                                                                  |
| :------------------------------------------------- | :----------------------------------- | :--------------------------------------------------------------------------------------- |
| per-algorithm typed solver-receipt family          | `NumericIntent` + `SolverReceipt`    | every solve route returns Factorization/Integration/Convergence evidence, never generic  |
| deep dense linear-algebra lane                      | `NumericIntent` array-solver#SOLVER  | LU/QR/Cholesky/SVD/eig/Schur + `LinearProvider` backend row (numpy·mkl·openblas·jax)     |
| deep sparse linear-algebra lane                     | `NumericIntent` array-solver#SOLVER  | CSR/CSC/COO + SuperLU/CHOLMOD + Krylov CG/GMRES/BiCGSTAB + ILU/AMG + AMD/METIS reorder    |
| ODE/DAE adaptive integration lane                   | `NumericIntent` array-solver#SOLVER  | Radau/BDF/LSODA/DOP853 + event detection + dense-output interpolant + stiffness estimate  |
| full optimization + root-finding lane               | `NumericIntent` array-solver#SOLVER  | constrained NLP/global/NLLS/multi-objective + Powell/Anderson/Broyden vector root-finding |
| study DOE generation + global sensitivity           | `StudyPlan` units-study#STUDY        | LHS/Sobol/Halton/factorial/CCD/Box-Behnken designs + Sobol/Morris/FAST/Delta indices      |
| study calibration + Pareto + surrogates             | `StudyPlan` units-study#STUDY        | Bayesian calibration + multi-objective Pareto + GP/PCE/RBF response-surface active-learn   |
| persistent queryable study-receipt lake             | `StudyPlan` units-study#STUDY        | design matrix + per-point receipts in duckdb/parquet, AS-OF + provenance-DAG query shape   |
| adjoint-through-solve inverse design                | new `differentiable` (compute)       | exact gradients THROUGH the FEM/PDE solve; topology/shape opt + PDE-constrained inverse    |
| validated-numerics certifier                        | new `rigor` (compute)                | interval/ball/Taylor-model guaranteed enclosures — proven bound, peer to confidence band   |
| signal & spectral workbench                         | `NumericIntent` `analyze` route      | FFT/STFT/wavelet/Welch-PSD/filter-design over unit-and-coordinate-aware labelled arrays    |
| verified symbolic-to-native codegen                 | `SymbolicDerivation` array-solver    | sympy → numba/jax/C with `EquivalenceLaw.Prove` tolerance gate before a solver lane uses it |
| quantity+uncertainty threaded through the graph     | `QuantityClaim` units-study#QUANTITY | dimensioned pint × uncertainties dual-path over xarray axes; dimensional proof + band       |
| transactional lakehouse table engine                | new `lakehouse` (data)               | MERGE/time-travel/OPTIMIZE+Z-order/VACUUM/schema+partition evolution/CDC on one `TableOp`   |
| versioned tensor datacube + out-of-core array        | new `tensorstore` (data)             | Zarr-v3/Icechunk branches/tags/time-travel + dask/cubed blockwise + Awkward ragged + CF cube |
| data-contract statistical firewall                  | `ContractGate` + new `quality`(data) | per-column profile + PSI/KS drift vs baseline + expectations + quarantine; medallion gate    |
| federated search + continuous standing query        | `ScanPlan` columnar-query (data)     | vector·spatial·text fusion ranking + windowed watermark standing queries w/ per-hit lineage  |
| simulation mesh+field interchange + FEM assembly      | new `simframe` (compute/geometry)    | CGNS/EXODUS/VTK/XDMF/MED/Gmsh over HDF5/NetCDF + scikit-fem weak-form assembly + in-situ      |
| parametric constraint solver + git-for-geometry      | new `parametric` (geometry)          | DOF/over-under-constrained verdict + hash-dedup object-graph diff/merge/three-way conflict    |
| semantic DocumentModel + bidirectional seam          | new `docmodel` + `DocumentPlan`      | one document tree every backend lowers from / every extractor recovers to; query 10k-PDF corpus |
| conformance grading + sealing plane                  | new `conformance` (artifacts)        | PDF/A+UA grade + physical redaction + PAdES/RFC-3161 sign + Merkle bundle over `ContentKey`   |
| print-production color management                    | new `color` (artifacts)              | ICC/CMYK/spot/wide-gamut + imposition; ColorSpace folded into `ContentIdentity` seed          |
| provider-fingerprint + replay-verify determinism      | `ContentIdentity`/`Receipt` (runtime)| BLAS/threads/SIMD/version/RNG capture; bit-divergence verdict derivable, not a flag           |
| orchestration triggers + offline durable queue        | `StagePlan`/`LanePolicy` (runtime)   | content-addressed node memo + watch/cron/event `Trigger` union + exactly-once WAL replay       |
| typed capability catalog + MCP/SDK projection         | new `capabilities` (runtime)         | every op a self-describing effect-classed cost-modeled permission-gated row; CLI/MCP/SDK views |
| parametrized headless notebook execution              | new `notebook` (runtime)             | content-addressed cell outputs + export-to-replay; reproducible authorship, CONSUMES capabilities |
| dataframe-agnostic Arrow zero-copy carrier            | `ColumnarEgress`/`ScanPlan` (data)   | `__dataframe__`/narwhals/Arrow-PyCapsule cross-library polars↔pandas↔duckdb↔cudf zero-copy plane |
| array-native computational geometry                   | `NumericIntent` `spatial` route      | scipy.spatial KD-tree/Voronoi/Delaunay/ConvexHull/alpha-shape/Hausdorff over numpy point sets    |
| scientific image processing                           | new `imaging` (artifacts)            | scikit-image/ndimage segmentation/morphology/registration; raster/voxel peer to ScanProcessing   |
| accelerated network-science algorithm family          | `GraphPayload` (data)                | networkx→rustworkx backend + community/centrality/flow/spectral (re-graded PARTIAL from EXISTS)   |
| bounded-memory chunked-streaming iterator             | `LanePolicy`/`StagePlan` (runtime)   | larger-than-memory transform streaming under a memory cap; the primitive PY5/PY7/PY16 each assume |
