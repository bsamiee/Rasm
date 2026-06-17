# [CSHARP_BRANCH_ARCHITECTURE]

The C# branch source tree, project-reference graph, and the cross-folder seams that no single package owns. The per-folder owner registries live in each package `ARCHITECTURE.md` `[OWNER_REGISTRY]`; this node carries only the branch-level build order, the `.csproj` reference edges, and the durable cross-folder seams. The full seam ledger is `region-map/seam-splits.md`; the seam table below pulls the durable cross-folder C# facts in `pkg/page#CLUSTER` notation.

## [1]-[SOURCE_TREE]

The build order is the dependency order through the strata: the kernel first, then the host-neutral AEC-domain packages, then the app-platform packages, then the host-boundary packages composed only at the future app roots. The full hierarchy law is `libs/.planning/architecture.md`.

```text codemap
libs/csharp/
  Rasm/               KERNEL geometry/numeric kernel — Vectors/Analysis/Domain + Geometry/ robust-core (predicates, spatial index, topology, healing, constraints); RhinoCommon-aware
  Rasm.Materials/     AEC-DOMAIN host-neutral — Profiles/ (profile families) + Appearance/ (BSDF/spectral/photometric) + Construction/ (elements→assemblies→layout); composes the kernel
  Rasm.Bim/           AEC-DOMAIN host-neutral — BIM object model + IFC/glTF/STEP exchange codec + element-set/classification/assembly; composes the kernel, consumes the Compute tessellation/identity seam
  Rasm.Fabrication/   AEC-DOMAIN host-neutral — portable HLR/CAM/nesting frontier; composes the kernel predicates + spatial index
  Rasm.AppHost/       APP-PLATFORM runtime spine — host profiles, lifecycle/drain, ports, telemetry, outbound, capability/sandbox/determinism
  Rasm.Persistence/   APP-PLATFORM durable stores — store profiles, query rail, snapshots, sync, cache indexes, server tier, version-control
  Rasm.Compute/       APP-PLATFORM measured execution — tensors, numeric solve, model lane, remote-lane wire, tessellation/identity, solver/optimizer
  Rasm.AppUi/         APP-PLATFORM Avalonia product UI — surfaces, shell, screens, commands, charts, visuals, viewport, drafting, notebook
  Rasm.Rhino/         HOST-BOUNDARY (source-only) — RhinoCommon host APIs; references Rasm; admitted only at the future app roots
  Rasm.Grasshopper/   HOST-BOUNDARY (source-only) — GH2 host APIs; references Rasm; admitted only at the future app roots
```

`Rasm.AppHost` is authored before its app-platform siblings — every app-platform package consumes its ports as settled vocabulary. `Rasm.AppUi` is the app-platform consuming leaf: it references the kernel + the app-platform packages and NO host-boundary package; the host boundaries (`Rasm.Rhino`/`Rasm.Grasshopper`) enter only at the future app root that composes a live host. The AEC-domain packages (`Rasm.Materials`/`Rasm.Bim`/`Rasm.Fabrication`) are host-neutral, compose the kernel, and are consumed by the app-platform at minimal one-directional boundaries.

## [2]-[PROJECT_REFERENCE_GRAPH]

Derived from the `.csproj` `ProjectReference` edges and the planned strata. `Rasm` is referenced by every higher stratum; the host-neutral AEC-domain packages reference only the kernel; `Rasm.AppHost` is referenced by its app-platform siblings; the host packages reference only `Rasm` and are admitted only at the future app roots.

| [INDEX] | [PROJECT]          | [REFERENCES]                                       | [REFERENCED_BY]                                                       |
| :-----: | :----------------- | :------------------------------------------------- | :------------------------------------------------------------------- |
|   [1]   | `Rasm`             | (none)                                             | `Materials`, `Bim`, `Fabrication`, `Compute`, `AppUi`, `Rhino`, `Grasshopper` |
|   [2]   | `Rasm.Materials`   | `Rasm`                                             | AppUi/Compute boundaries (AEC-domain→app-platform, one-directional)  |
|   [3]   | `Rasm.Bim`         | `Rasm`                                             | app roots; consumes Compute content-identity/tessellation at the seam (no project reference — strata-upward) |
|   [4]   | `Rasm.Fabrication` | `Rasm`                                             | AppUi drafting (hidden-line); app roots                              |
|   [5]   | `Rasm.AppHost`     | (none)                                             | `AppUi`, `Compute`, `Persistence`                                    |
|   [6]   | `Rasm.Persistence` | `AppHost`                                          | `AppUi`, `Compute`                                                   |
|   [7]   | `Rasm.Compute`     | `Rasm`, `AppHost`, `Persistence`                   | `AppUi`                                                              |
|   [8]   | `Rasm.AppUi`       | `Rasm`, `AppHost`, `Compute`, `Persistence`        | (none — consuming leaf)                                              |
|   [9]   | `Rasm.Rhino`       | `Rasm`                                             | app root (NOT AppUi)                                                 |
|  [10]   | `Rasm.Grasshopper` | `Rasm`                                             | app root (NOT AppUi)                                                 |

The graph is acyclic with `AppHost` as the runtime root: `AppHost` references nothing, and execution/UI/store flow downstream of it. `Compute` references `Persistence` (cache/artifact/benchmark indexes); `Persistence` never references `Compute`. No host-neutral package references a host boundary; the host boundaries are admitted only at the future app root. `Rasm.Bim` consumes the shared content-identity seed and the tessellation rail at the Compute seam as settled vocabulary (reproduced bit-identically, the same pattern Python/TS use), NOT through a `Rasm.Compute` project reference — so AEC-domain never depends upward on app-platform.

## [3]-[SEAMS]

The durable cross-folder C# seams in `pkg/page#CLUSTER` notation; `region-map/seam-splits.md` is the full ledger. Cross-language consequences route to the Tier-0 seam ledger and never appear here.

| [INDEX] | [SEAM]                       | [MECHANICS_AT]                                          | [CONSEQUENCE_AT]                                                            |
| :-----: | :--------------------------- | :----------------------------------------------------- | :------------------------------------------------------------------------- |
|   [1]   | HybridCache                  | AppHost/resource-lanes#CACHE_PORT                      | Persistence/cache-indexes#L2_CONTRIBUTION                                   |
|   [2]   | outbound retry               | AppHost/outbound-resilience#KEYED_PIPELINES            | Compute/receipts-and-benchmarks#RECEIPT_UNION; store retry stays at Persistence |
|   [3]   | correlation + trace context  | AppHost/diagnostics-and-telemetry#CORRELATION_SPINE    | every sibling signal; gRPC metadata over the UDS hop                        |
|   [4]   | drain order                  | AppHost/lifecycle-and-drain#DRAIN_CONDUCTOR            | each sibling's DrainParticipantPort registrations                          |
|   [5]   | data classification          | AppHost/diagnostics-and-telemetry#REDACTION_TAXONOMY   | Persistence/redaction-retention#CLASSIFICATION_ENFORCEMENT                  |
|   [6]   | profile variance + roots     | AppHost/host-profiles#PROFILE_AXIS                     | Persistence/store-profiles#PROFILE_AXIS consumes ResolvedProfile           |
|   [7]   | clock seam                   | AppHost/time-and-deadlines#CLOCK_SPLIT                 | Persistence TTL/retention/HLC/lease stamping; Compute elapsed              |
|   [8]   | receipt + telemetry sinks    | AppHost/runtime-ports#PORT_RECORDS                     | Compute/Persistence/AppUi receipt and instrument projections              |
|   [9]   | wire vocabulary              | Compute/remote-lane#PROTO_VOCABULARY                   | AppHost/runtime-ports#WIRE_LAW suite merge                                 |
|  [10]   | artifact framing             | Compute/remote-lane#ARTIFACT_FRAMES                    | Persistence/sync-collaboration#PRESENCE_AND_BLOB + store-profiles BlobRemote |
|  [11]   | model-result horizon         | Persistence/cache-indexes#MODEL_RESULT_INDEX           | Compute/model-lane#RESULT_CACHE; Compute/receipts-and-benchmarks#BENCHMARK_CLAIMS |
|  [12]   | tenancy threading            | AppHost/runtime-ports#PORT_RECORDS                     | Persistence/server-tier#TENANCY_RLS + content-address cache-key partition  |
|  [13]   | UI scheduler boundary        | AppUi/surface-hosts#SCHEDULER_BOUNDARY                 | AppHost/runtime-ports#PORT_RECORDS UiSchedulerPort at the composition root |
|  [14]   | phase-set conformance        | Compute/progress-and-observation#PHASE_FAMILY          | AppUi/motion-tokens#PHASE_MAPPING sweep fails on phase-set drift           |
|  [15]   | clash acceleration           | Rasm/spatial-index#SPATIAL_INDEX                       | Compute/solver-and-optimization#CLASH_AND_TWIN consumes ClashScale.Detect  |
|  [16]   | hidden-line substance        | Fabrication/hidden-line#PROJECTION_HIDDEN_LINE         | AppUi/drafting-sheets#PROJECTION Viewport2D                                |
|  [17]   | structural-diff identity     | Rasm/topology#NAMING_HASH                              | Persistence/version-control#STRUCTURAL_DIFF GeometryHash; federation#ENTITY_GRAPH |
|  [18]   | path-trace shading           | Materials/bsdf#LAYERED_COMPOSITION                     | AppUi/viewport-pipeline#PATH_TRACE shades from LayeredBsdf.Evaluate        |
|  [19]   | photometric unit algebra     | Materials/texture-photometric#PHOTOMETRIC             | Compute/units-boundary#QUANTITY_TABLE QuantityFamily admission             |
|  [20]   | command dispatch             | AppHost/capability-registry#COMMAND_ALGEBRA           | Compute/intent-and-selection#INTENT_FAMILY executes the descriptor invocation |
|  [21]   | event-log durability         | AppHost/determinism-and-replay#EVENT_LOG              | Persistence/sync-collaboration#OPLOG_CHANGEFEED stores each LogEntry        |
|  [22]   | interchange content identity | Compute/interchange#CONTENT_ADDRESSING               | Persistence/cache-indexes#ARTIFACT_BLOB_INDEX blob residence; Bim/exchange#EXPORT_RAIL consumes the same seed |
|  [23]   | IFC semantic graph           | Bim/exchange#IMPORT_RAIL                             | Persistence/federation#ENTITY_GRAPH ingests the IFC semantic model; Compute/interchange#TWO_HOP_TESSELLATION reads it as tessellation input |
|  [24]   | BIM tessellation request     | Compute/interchange#TWO_HOP_TESSELLATION             | Bim/exchange#TESSELLATION_BRIDGE builds the TessellationRequest; Compute issues it over the transport and re-imports the GLB |
|  [25]   | classification + element-set | Bim/object-model#ELEMENT_SET                        | Persistence/catalog-cost reads IFC classification psets; AppUi inspector projects the element model |
