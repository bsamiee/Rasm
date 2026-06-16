# [PYTHON_PAGE_REGIONS]

Per-package planning page regions. A cluster token resides once, on its owning page. The central charter, architecture, method, catalogues, atlas, and ledger are the branch law; the package pages carry the transcription fences.

## [1]-[CENTRAL]

- `README.md` [FINAL]: branch charter — page index, owner records, build sequence, proof gates, prohibitions, refinement horizon.
- `architecture.md` [FINAL]: re-derived five-package topology, interpreter floors, dependency direction, cross-boundary seams.
- `campaign-method.md` [FINAL]: branch-local method aligned to the suite campaign method; the bar, fence law, named defect class, red-team passes.
- `api-catalogues.md` [FINAL]: evidence protocol, capture order, capture-floor law over the final `.api` set.
- `FEATURES.md` [FINAL]: branch capability atlas — feature, owner, page#cluster.
- `TASKLOG.md` [FINAL]: open work and install/catalogue gaps only.

## [2]-[RUNTIME]

- `context-settings.md` [FINAL]: caller-owned context, profile, correlation, deadline, settings admission. Clusters: CONTEXT, SETTINGS.
- `rails-resilience.md` [FINAL]: the single boundary-fault tagged union, Result/Option rail, and one Retry policy owner. Clusters: FAULT, RESILIENCE.
- `content-identity.md` [FINAL]: the single XxHash128 content-identity owner reproducing the C# seed. Cluster: IDENTITY.
- `resources-lanes.md` [FINAL]: resource roots, transport resources, bounded anyio lanes, stage-plan DAG. Clusters: RESOURCE, LANE.
- `observability.md` [FINAL]: local receipts, the contributor port, redaction, structlog/OTel/psutil signals. Cluster: RECEIPT.
- `server-host.md` [FINAL]: the inbound companion gRPC server lifecycle and credential axis. Cluster: SERVE.
- `evidence.md` [FINAL]: API + structural-parsing evidence and the private entrypoint grammar. Clusters: API, ENTRY.

## [3]-[DATA]

- `columnar-query.md` [FINAL]: dataset refs, scan plans, columnar egress, query receipts. Clusters: DATASET, SCAN.
- `schema-geo.md` [FINAL]: schema claims, contract gate, vector + raster geospatial. Clusters: SCHEMA, GEO.
- `graph-mesh.md` [FINAL]: graph payloads and mesh-file exchange. Clusters: GRAPH, MESH.

## [4]-[GEOMETRY]

- `ifc-companion.md` [FINAL]: the IfcOpenShell tessellation daemon over the C# gRPC contract. Cluster: DAEMON.
- `ifc-analysis.md` [FINAL]: IFC property/quantity/relationship analysis (QTO/clash/rule-check). Cluster: ANALYSIS.
- `scan-processing.md` [FINAL]: point-cloud registration, reconstruction. Cluster: REGISTRATION.
- `geometry-algebra.md` [FINAL]: non-manifold topology + AEC computational geometry. Cluster: ALGEBRA.

## [5]-[COMPUTE]

- `array-solver.md` [FINAL]: array admission, numeric-intent solver, symbolic derivation, accelerators. Clusters: ARRAY, SOLVER.
- `units-study.md` [FINAL]: units/uncertainty claims, study + run-history orchestration, model assets. Clusters: QUANTITY, STUDY, MODEL.
- `graduation.md` [FINAL]: the C# graduation receipt with the geometry handoff case. Cluster: GRADUATION.

## [6]-[ARTIFACTS]

- `documents.md` [FINAL]: the document/PDF/Office/structured-text plan, report templating, artifact receipt. Clusters: DOCUMENT, REPORT, RECEIPT.
- `visual-export.md` [FINAL]: the VisualSpec/ExportPlan axis (2D + 3D), preview, compression. Clusters: VISUAL, EXPORT, PREVIEW, COMPRESSION.

## [7]-[PROPOSED] — next-loop ideation pages + reserved cluster tokens (2026-06-16)

Reserved by the ideation pass (`.artifacts/planning-briefs/python-ideation.md`, PY1-PY26). State `[PROPOSED]` reserves the page path and its cluster tokens so the ledger stops contradicting the TASKLOG; the page finalizes (`[FINAL]`) when its TASKLOG row lands the transcription fence. A cluster token still resides once, on its proposed owning page. New owned pages and the fold-in deepenings that add a cluster to an existing page:

- compute `differentiable.md` [PROPOSED]: adjoint-through-solve inverse design. Cluster: INVERSE. (`PY_DIFF_001`)
- compute `rigor.md` [PROPOSED]: validated-numerics certifier. Cluster: CERTIFY. (`PY_RIGOR_001`)
- compute `simframe.md` [PROPOSED]: sim mesh+field interchange + weak-form FEM assembly + in-situ. Clusters: INTEROP, ASSEMBLY. (`PY_SIM_001`)
- data `lakehouse.md` [PROPOSED]: transactional Delta lifecycle + medallion. Cluster: TABLE. (`PY_LAKE_001`)
- data `tensorstore.md` [PROPOSED]: Zarr-v3/Icechunk versioned tensor store + out-of-core + ragged + CF/DGG cube. Cluster: CUBE. (`PY_TENSOR_001`)
- data `quality.md` [PROPOSED]: profiling/drift/expectations/quarantine firewall (`ContractGate` deepened). Cluster: PROFILE. (`PY_QUAL_001`)
- geometry `parametric.md` [PROPOSED]: constraint solver + object-graph version control. Clusters: CONSTRAIN, VERSION. (`PY_PARA_001`)
- artifacts `docmodel.md` [PROPOSED]: semantic document tree + bidirectional extraction + query. Cluster: MODEL. (`PY_DOC_002`)
- artifacts `conformance.md` [PROPOSED]: PDF/A+UA grading + redaction + sealing. Clusters: GRADE, SEAL. (`PY_CONF_001`)
- artifacts `color.md` [PROPOSED]: ICC/CMYK/wide-gamut + imposition. Cluster: GAMUT. (`PY_COLOR_001`)
- runtime `capabilities.md` [PROPOSED]: typed op catalog + MCP/SDK/CLI projection. Cluster: CATALOG. (`PY_CAP_001`)
- runtime `notebook.md` [PROPOSED]: parametrized headless notebook execution + content-addressed cell outputs. Cluster: RUN. (`PY_NB_001`)
- artifacts `imaging.md` [PROPOSED]: scikit-image/ndimage scientific image processing. Cluster: IMAGE. (`PY_IMG_001`)

Cluster tokens added to EXISTING pages by a deepening (no new page): compute `units-study.md` STUDY cluster gains the `StudyReceipt`/`SensitivityReceipt` lake (`PY_STUDY_001`); compute `array-solver.md` SOLVER cluster gains `SolverReceipt`/`SpectralReceipt`/`EquivalenceLaw` and a SPATIAL cluster for array-native computational geometry (`PY_SOLVE_001`/`PY_SIG_001`/`PY_CODEGEN_001`/`PY_SPATIAL_001`); data `columnar-query.md` gains a QUERY cluster (`StandingQuery`/`FusionRank`, `PY_QUERY_001`) and the Arrow zero-copy interchange carrier on SCAN (`PY_INTEROP_001`); runtime `resources-lanes.md` LANE cluster gains `Trigger` + the streaming-iterator extension (`PY_ORCH_001`/`PY_STREAM_001`); runtime `content-identity.md`/`observability.md` gain the provider-fingerprint determinism extension (`PY_DET_001`); data `graph-mesh.md` GRAPH cluster gains the accelerated network-science family (`PY_GRAPH_001`).
