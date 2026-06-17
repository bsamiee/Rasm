# [PY_RUNTIME_ARCHITECTURE]

`runtime` is one execution foundation: every concern is an axis owner with closed cases, every entrypoint is a typed Result/Option rail, and every sibling consumes its owners without a second owner anywhere. Mechanics live in the finalized `.planning/` pages; this page is the atlas — the implementation source tree, the owner registry (the one owner-state surface), dependency direction, cross-folder seams, the package boundaries, and the prohibitions.

## [1]-[SOURCE_TREE]

The planned module layout IS the build order: each file is one transcription unit, vocabulary owners before consumers, faults and context before the rails that carry them, identity and resources before the lanes and server that ride them. Each leaf is annotated with the owners it transcribes and the owning page#cluster.

```text codemap
runtime/
├── faults.py            # BoundaryFault, RuntimeRail, Retry — rails-resilience#FAULT, #RESILIENCE
├── context.py           # RuntimeContext, RuntimeProfile, SettingsAdmission — context-settings#CONTEXT, #SETTINGS
├── identity.py          # ContentIdentity — content-identity#IDENTITY
├── resources.py         # ResourceRoot, ResourceRef, TransportResource — resources-lanes#RESOURCE
├── lanes.py             # LanePolicy, DrainReceipt, StagePlan — resources-lanes#LANE
├── observability.py     # Receipt, ReceiptContributor, Redaction — observability#RECEIPT
├── server.py            # ServerHost, Credential — server-host#SERVE
└── evidence.py          # ApiPackage, ApiMember, Entrypoint — evidence#API, #ENTRY
```

`faults.py` lands first because every later owner returns through `BoundaryFault` and the `RuntimeRail` carrier. `context.py` precedes the resource and server files that read the admitted `RuntimeContext`. `identity.py` lands before `resources.py` because resource and transport receipts key by the `ContentIdentity` key. `lanes.py` follows `resources.py`: the `LanePolicy` task groups and the `StagePlan` DAG drive resource and transport acquisition. `observability.py` lands after the owners whose receipts wire through `ReceiptContributor`. `server.py` is the inbound serving counterpart, composing `LanePolicy` (lanes), `Credential` (its own credential axis), and the admitted `RuntimeContext` without re-declaring them. `evidence.py` lands last: `ApiPackage`/`ApiMember` feed the structural-evidence rail and `Entrypoint` backs the companion's private daemon entry.

## [2]-[OWNER_REGISTRY]

The single owner-state surface for the package. Implementation collapses to one owner per axis and one entrypoint family per rail; density means no parallel rails, no near-duplicate shapes, no re-derived logic — a file is as large as its owner's concern requires, never trimmed to a line count. A new feature is a row or case, never a new surface; a public type outside these owner regions is the named defect. `[STATE]` is `FINALIZED` where the owner is a transcription-complete fence with no open gate, `SPIKE` where the owner is fence-complete but its proof carries a residual floor/bridge/live-server probe named in the page RESEARCH cluster. This is the ONLY place owner state lives.

| [INDEX] | [AXIS/RAIL]        | [OWNER]              | [KIND]                        | [CASES]                                           | [PAGE#CLUSTER]                 |  [STATE]  |
| :-----: | :----------------- | :------------------- | :---------------------------- | :------------------------------------------------ | :----------------------------- | :-------: |
|   [1]   | fault family       | `BoundaryFault`      | tagged union                  | config/resource/deadline/api/import/wire/boundary | rails-resilience#FAULT         | FINALIZED |
|   [2]   | rail carrier       | `RuntimeRail`        | Result/Option alias           | abort/accumulate algebras                         | rails-resilience#FAULT         | FINALIZED |
|   [3]   | resilience policy  | `Retry`              | frozen policy table           | row per retryable class                           | rails-resilience#RESILIENCE    | FINALIZED |
|   [4]   | context admission  | `RuntimeContext`     | frozen Struct + factory       | profile/correlation/deadline/class                | context-settings#CONTEXT       | FINALIZED |
|   [5]   | profile vocabulary | `RuntimeProfile`     | StrEnum + policy table        | tool/sidecar/package/test                         | context-settings#CONTEXT       | FINALIZED |
|   [6]   | settings admission | `SettingsAdmission`  | pydantic-settings owner       | mapping/env/payload sources                       | context-settings#SETTINGS      | FINALIZED |
|   [7]   | content identity   | `ContentIdentity`    | static surface                | `key`/`seed`/`fold`                               | content-identity#IDENTITY      | FINALIZED |
|   [8]   | resource roots     | `ResourceRoot`       | frozen owner + `ResourceRef`  | file/object-store/scratch                         | resources-lanes#RESOURCE       | FINALIZED |
|   [9]   | transport resource | `TransportResource`  | tagged union                  | http/ssh/speckle                                  | resources-lanes#RESOURCE       | FINALIZED |
|  [10]   | concurrency lanes  | `LanePolicy`         | frozen owner + `DrainReceipt` | capacity/cancellation/drain                       | resources-lanes#LANE           | FINALIZED |
|  [11]   | stage DAG          | `StagePlan`          | frozen owner                  | stage edges, per-stage retry                      | resources-lanes#LANE           | FINALIZED |
|  [12]   | receipt evidence   | `Receipt`            | tagged union                  | admitted/planned/emitted/rejected/drained         | observability#RECEIPT          | FINALIZED |
|  [13]   | contributor port   | `ReceiptContributor` | Protocol                      | one `contribute` method                           | observability#RECEIPT          | FINALIZED |
|  [14]   | server host        | `ServerHost`         | boundary capsule              | `serve`/`drain` over `grpc.aio`                   | server-host#SERVE              |   SPIKE   |
|  [15]   | credential axis    | `Credential`         | tagged union                  | token/keyring/insecure-loopback                   | server-host#SERVE              |   SPIKE   |
|  [16]   | API evidence       | `ApiPackage`         | record + `ApiMember`          | distribution/import/owner/surface                 | evidence#API                   | FINALIZED |
|  [17]   | entrypoint grammar | `Entrypoint`         | cyclopts app                  | companion private entry only                      | evidence#ENTRY                 | FINALIZED |

One rail per entrypoint, named in the return type: the `RuntimeRail` `Result` aborts, the accumulate algebra gathers, `Option` carries absence. Receipts route through `ReceiptContributor`; `ContentIdentity` keys every emitted bundle. Domain flow never raises — exceptions convert to a `BoundaryFault` exactly once at the owning boundary.

## [3]-[DEPENDENCY_DIRECTION]

| [INDEX] | [PACKAGE]   | [MAY_REFERENCE_RUNTIME] | [RUNTIME_MAY_REFERENCE] | [BOUNDARY]                                  |
| :-----: | :---------- | :---------------------: | :---------------------: | :------------------------------------------ |
|   [1]   | `data`      |           yes           |           no            | egress bundles key by `ContentIdentity`     |
|   [2]   | `compute`   |           yes           |           no            | receipts wire through `ReceiptContributor`  |
|   [3]   | `geometry`  |           yes           |           no            | companion serves through `ServerHost`       |
|   [4]   | `artifacts` |           yes           |           no            | artifact bundles key by `ContentIdentity`   |

`runtime` is the branch foundation; no sibling owner enters its graph. Siblings consume runtime ports — `ContentIdentity`, `BoundaryFault`/`RuntimeRail`, `Retry`, `ResourceRoot`, `LanePolicy`, `ReceiptContributor`, `ServerHost` — and never re-mint them. Host facts arrive from a caller-supplied `RuntimeContext`; the package never discovers the host. Cross-language host-lifecycle and wire-vocabulary consequences ride the Tier-0 `region-map/seam-splits.md`.

## [4]-[SEAMS]

Every two-folder fact splits by altitude: mechanics live at the named runtime cluster, consequences land at the consumer. Intra-Python seams ride `pkg/page#CLUSTER`; cross-language consequences ride the Tier-0 `region-map/seam-splits.md` and are referenced as a Tier-0 seam, never restated here.

| [INDEX] | [SEAM]              | [MECHANICS_AT]             | [CONSEQUENCE_AT]                                                                            |
| :-----: | :------------------ | :------------------------- | :----------------------------------------------------------------------------------------- |
|   [1]   | content identity    | content-identity#IDENTITY  | data/columnar-query#SCAN, artifacts/documents#RECEIPT, geometry/ifc-companion#DAEMON key by one `ContentIdentity` |
|   [2]   | contributor port    | observability#RECEIPT      | data/columnar-query#SCAN `QueryReceipt`, compute/graduation#GRADUATION `GraduationReceipt`, geometry and artifacts receipts wire through `ReceiptContributor` |
|   [3]   | boundary fault rail | rails-resilience#FAULT     | every sibling raises through `BoundaryFault` and returns the `RuntimeRail` carrier         |
|   [4]   | resilience policy   | rails-resilience#RESILIENCE | sibling retry rides one `Retry` table; no second retry owner                               |
|   [5]   | transport resource  | resources-lanes#RESOURCE   | data/graph-mesh#MESH and AEC remote streams acquire through `TransportResource`, never a second transport |
|   [6]   | concurrency lanes   | resources-lanes#LANE       | sibling bounded fan-out rides `LanePolicy` task groups and `DrainReceipt`                  |
|   [7]   | companion serve     | server-host#SERVE          | geometry/ifc-companion#DAEMON hosts its tessellation daemon through `ServerHost`           |

## [5]-[BOUNDARIES]

- `runtime` is not a host package, app-runtime, job framework, telemetry-export package, durable store, or product service layer.
- `runtime` receives host facts from a caller-supplied `RuntimeContext` and never discovers the host, starts product services, owns host lifecycle, derives product roots, or owns a global clock.
- Statement carve-outs are named per fence: `ServerHost.serve`/`drain` and the `SettingsAdmission` source-order admission are the boundary capsules; every other member stays expression-shaped on the `RuntimeRail`.
- `runtime` contributes receipts and structured facts through `ReceiptContributor`; product telemetry export and health stay outside this package.
- `Entrypoint` is the companion's private daemon entry only; the package mints no public command surface.
- The companion server wire vocabulary is minted elsewhere and consumed at the seam; `ServerHost` implements the inbound serve and never mints a second wire vocabulary. The cross-language wire and host-lifecycle facts ride the Tier-0 seam ledger.

## [6]-[PROHIBITIONS]

The closed NEVER list — the deleted patterns the owner registry forecloses.

- NEVER a public surface beside the budgeted owners; a new capability is a row or case.
- NEVER a second content-identity, receipt, retry, correlation, or wire-vocabulary owner.
- NEVER a public CLI command; `Entrypoint` is the companion's private entry only.
- NEVER host discovery, product-service start, host-lifecycle ownership, product-root derivation, or a global clock.
- NEVER a raise inside domain flow, `None`-as-failure or sentinel propagation, or stringly-typed dispatch.
- NEVER a direct process-environment read after context admission; `SettingsAdmission` is the one source-order owner.
