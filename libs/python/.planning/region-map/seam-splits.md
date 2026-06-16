# [PYTHON_SEAM_SPLITS]

Every cross-owner fact records its altitude split: mechanics at the owning page, consequence at the consumer. A seam re-taught instead of consumed is a defect repaired by routing to the owner.

## [1]-[CROSS_BOUNDARY_SEAMS]

[COMPANION_GRPC_WIRE]:
- Mechanics owner: `geometry/ifc-companion#DAEMON`
- Consumer consequence: C# `remote-lane#TRANSPORT_AXIS` reaches the companion over the EXISTING contract; no new wire

[COMPANION_SERVE_RUNTIME]:
- Mechanics owner: `runtime/server-host#SERVE`
- Consumer consequence: geometry `IfcCompanion` hosts its inbound serve through `ServerHost`; never the C# host lifecycle

[CONTENT_IDENTITY]:
- Mechanics owner: `runtime/content-identity#IDENTITY`
- Consumer consequence: data/geometry/artifacts key by `ContentIdentity` reproducing C# `InterchangeIdentity`; never re-minted

[GEOMETRY_GRADUATION]:
- Mechanics owner: `compute/graduation#GRADUATION`
- Consumer consequence: geometry evidence reaches the C# owner system through the `HandoffAxis` geometry case

[RECEIPT_CONTRIBUTION]:
- Mechanics owner: `runtime/observability#RECEIPT`
- Consumer consequence: data/compute/geometry/artifacts typed receipts wire through one `ReceiptContributor` port

[RESILIENCE_POLICY]:
- Mechanics owner: `runtime/rails-resilience#RESILIENCE`
- Consumer consequence: rails AND transport clusters consume one `Retry` owner via policy rows; never two

## [2]-[BOUNDARY_SEAMS]

| [SEAM]                  | [MECHANICS_OWNER]            | [CONSUMER_CONSEQUENCE]                                                             |
| :---------------------- | :--------------------------- | :--------------------------------------------------------------------------------- |
| host lifecycle          | `Rasm.AppHost`               | runtime accepts caller-owned context and resource roots only                       |
| product telemetry       | `Rasm.AppHost`               | runtime emits Python-local receipt facts only                                      |
| durable stores          | `Rasm.Persistence`           | data emits portable import/export bundles                                          |
| query rails             | `Rasm.Persistence`           | data owns offline scan plans and query receipts                                    |
| production compute      | `Rasm.Compute`               | compute owns studies, assets, and handoff receipts                                 |
| benchmark authority     | `Rasm.Compute`               | compute emits research evidence only                                               |
| IFC semantic in-process | `Rasm.Compute` (GeometryGym) | geometry owns only the tessellation hop the managed surface cannot do              |
| glTF in-process         | `Rasm.Compute` (SharpGLTF)   | geometry returns GLB the C# side reads; no Python glTF authoring                   |
| live UI                 | `Rasm.AppUi` and TypeScript  | artifacts emits static files and visual specs                                      |
| Rhino/GH mutation       | bridge and C# host owners    | data/geometry read or emit files only                                              |
| Assay command surface   | `tools/assay`                | runtime `Entrypoint` is the companion's PRIVATE entry only; no new public commands |
| external API members    | package `.api` owner         | a fence names a member only after evidence verifies its spelling                   |

## [3]-[INTRA_BRANCH_SEAMS]

[LABELLED_ARRAY_CATALOGUE]:
- Mechanics owner: `data/.api` (`xarray`, `dask`)
- Consumer consequence: compute composes the data catalogues as study-input bundle shapes; deletes its duplicate stubs

[MESH_FILE_EXCHANGE]:
- Mechanics owner: `data/graph-mesh#MESH`
- Consumer consequence: geometry consumes mesh-file shapes as inputs; the IFC->GLB rail stays in geometry

[REMOTE_AEC_TRANSPORT]:
- Mechanics owner: `runtime/resources-lanes#RESOURCE`
- Consumer consequence: data/geometry reach Speckle streams through the `TransportResource` row, not a data owner

## [4]-[PROPOSED_INTRA_BRANCH_SEAMS] — next-loop ideation (2026-06-16)

State `[PROPOSED]`; finalizes when both owner pages land. These record the cross-owner facts the ideation states in prose but never registered.

[DELTA_ALGEBRA]:
- Mechanics owner: `artifacts/docmodel#MODEL` (the `DocumentDelta` typed inserted/deleted/moved/reparametrized node-edit algebra)
- Consumer consequence: geometry `parametric#VERSION` `GeometryDelta` REUSES the DocumentDelta node-edit shape. ARCHITECTURE AMENDMENT REQUIRED: `architecture.md#DEPENDENCY_DIRECTION` does not list artifacts as a geometry dependency, so this reuse is an unflagged cross-package violation as written. Resolution: the node-edit delta algebra is a RUNTIME-owned value shape (keyed by `ContentIdentity`), lowered into both `docmodel` and `parametric`, so neither package imports the other's interior. `PY_DOC_002`/`PY_PARA_001` both consume the runtime delta primitive; the prose "GeometryDelta reuses DocumentDelta" is realized as "both consume the one runtime delta algebra."

[MESH_VALUE]:
- Mechanics owner: `runtime` (a DEPENDENCY-FREE STRUCTURAL mesh `Protocol`/typed-buffer — vertices/faces/attributes as plain numpy buffers + a `ContentIdentity` key, NO trimesh/meshio import in runtime)
- Consumer consequence: compute (`differentiable#INVERSE`, `simframe#ASSEMBLY`), data (`tensorstore#CUBE`), geometry (`parametric#VERSION`, `interchange#INGEST`) all CONSUME the runtime mesh Protocol and adapt their backend meshes (trimesh/meshio/OCCT) to it AT THEIR OWN BOUNDARY, without cross-importing each other's interiors. ARCHITECTURE: a trimesh/meshio-BACKED mesh in runtime would force runtime to depend on data's mesh catalogue, inverting `architecture.md#DEPENDENCY_DIRECTION` (runtime imports no first-wave package) — so the runtime owner is a STRUCTURAL contract only, never a backed value-object. The MESH-AS-DIFFERENTIABLE-DOMAIN law is non-violating only under this structural-Protocol residence. (Alternative rejected: relocating the canonical mesh to `data/graph-mesh#MESH` is dependency-direction-legal for compute/geometry but blocks the runtime-keyed determinism closure from referencing the mesh without a data dependency.)

[STUDY_RECEIPT_LAKE]:
- Mechanics owner: `compute/units-study#STUDY` (`StudyReceipt` lake)
- Consumer consequence: per-point `SolverReceipt` cases (`array-solver#SOLVER`) reference into the study lake; the lake keys by the `PY_DET_001` provider fingerprint, never a parallel key.

[DETERMINISM_FINGERPRINT]:
- Mechanics owner: `runtime/content-identity#IDENTITY` + `runtime/observability#RECEIPT` (provider fingerprint folded in)
- Consumer consequence: `study` lake key (`PY_STUDY_001`), `StandingQuery` lineage (`PY_QUERY_001`), DAG node-memo key (`PY_ORCH_001`) all consume the ONE fingerprint closure; no lane mints a parallel determinism key. PARITY: the fingerprint-member canonicalization must match the C# A6 byte layout for the EXTENDS verb to hold (`PY_DET_001` parity exit), else PEER-PLANE.

[RECEIPT_FAMILY]:
- Mechanics owner: `runtime/observability#RECEIPT` (a `Protocol`-bounded OPEN receipt family discriminated by the receipt `tag` Literal, NOT a single closed `Union` rooted in one package)
- Consumer consequence: each owner declares its own arm Struct (compute `SolverReceipt`/`StudyReceipt`/`Enclosure`/`SensitivityField`/`PosteriorReceipt`, data `CubeSnapshot`/`InteropField`, geometry `ConstraintReceipt`/`GeometryDelta`, artifacts `ConformanceGrade`/`DocumentDelta`) and the runtime port discriminates by tag. A single CLOSED msgspec tagged-union over 15+ cross-package arms would force runtime/data/geometry/artifacts to share one import root, re-creating the MESH_VALUE residency problem — decided before `PY_SOLVE_001` closes `SolverReceipt`.

[CAD_SCAN_INGEST]:
- Mechanics owner: `geometry/interchange#INGEST` (`PY_INGEST_001`; pythonocc-core/laspy/pye57/pdal native CAD/scan codecs)
- Consumer consequence: the C# suite is the NAMED consumer over the existing companion wire — `aggressive-ideation.md#[4]` delegates native-format bridges (Revit/Navisworks), IFC5 parsing, CAD-STEP/AP242 (OpenCascade/pythonocc), and the scan/mesh-format breadth to Python; the .NET stack cannot reach native OCCT/E57 in-process. Python OWNS the in-process ingest plane; the C# side reads the resolved mesh-Protocol/B-rep/point-set value keyed by `ContentIdentity`.

## [5]-[PROPOSED_CSHARP_COLLISION_SEAMS] — boundary verb per contested concept (2026-06-16)

Eight PY concepts collide with a named C# `aggressive-ideation` V-concept. Each carries a boundary verb so the "not a mirror" claim is discharged at the concept level. Full table at `.artifacts/planning-briefs/python-ideation.md#[1.1]`.

| [PY OWNER] | [C# COLLISION] | [VERB] | [SHARED CANONICAL OBJECT] |
| :--------- | :------------- | :----- | :------------------------ |
| `capabilities#CATALOG` (`PY_CAP_001`) | C# V7 Capability Control Plane | CONSUMES | the V7 capability atom; Python projects the C# registry to a Python-process MCP/SDK/CLI, never re-minting op identity |
| `differentiable#INVERSE` (`PY_DIFF_001`) | C# V18 Differentiable Geometry | DISJOINT | none; V18 differentiates fixed DDG operators in-host, PY3 differentiates an arbitrary FEM solve via IFT |
| `parametric#CONSTRAIN`/`#VERSION` (`PY_PARA_001`) | C# V2 kernel + V3 GraphFork | PYTHON-OWNS + READS | Python owns the DOF + object-graph-diff half; READS the C# V2 topological-naming wire (naming is C#-Persistence-owned) |
| `content-identity#IDENTITY` determinism (`PY_DET_001`) | C# V8/V10 + A6 provider-determinism | EXTENDS (with parity exit) | the A6 fingerprint closure; Python adds BLAS/RNG/SIMD members to the SAME hash domain ONLY if the fingerprint-member canonicalization (vendor/thread-count/SIMD-level byte serialization) matches the C# A6 layout — content-seed parity is necessary but not sufficient. Absent that proof the verb downgrades to PEER-PLANE (separate fingerprint federated by content key) |
| `columnar-query#QUERY` (`PY_QUERY_001`) | C# V15 Federated Search + V16 Continuous-Query | PEER-PLANE | the shared lineage edge; PY18 is duckdb/polars-embedded, V15/V16 are pgvector/Timescale-server |
| `rigor#CERTIFY` (`PY_RIGOR_001`) | C# V2 interval-certified | DISJOINT | none; arb/Taylor-models certify numeric expressions, V2 certifies geometric predicates |
| `units-study#STUDY` (`PY_STUDY_001`) | C# V6 Solve Farm + V13 Pareto | PEER-PLANE | the design-space/Pareto artifact shape; PY2 is the duckdb-embedded study lake, V6 is the Postgres-resident farm |
| `resources-lanes#LANE` orchestration (`PY_ORCH_001`) | C# V6 job-graph scheduler | CONSUMES (memo) + PYTHON-OWNS (trigger+queue) | SPLIT VERB: the node-memoization half CONSUMES the V6 node content-address (a runtime peer federated by node identity, not a forked scheduler); the `Trigger` union + durable SQLite-WAL exactly-once queue are PYTHON-OWNS, consuming no C# node identity (same split treatment as `PY_PARA_001`) |
