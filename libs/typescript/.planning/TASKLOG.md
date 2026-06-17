# [TYPESCRIPT_BRANCH_TASKLOG]

Branch-level open work for the TypeScript packages — the cross-folder finalization, deepening, spike, and infra-wiring work no single folder owns. Per-folder open work lives in each folder's `TASKLOG.md`; closed rows live in neither. `[STATUS]` is one of `QUEUED`, `ACTIVE`, `BLOCKED`, `SPIKE`; owner state is read at the owning folder's `ARCHITECTURE.md` owner registry.

## [1]-[INFRA_WIRING]

| [INDEX] | [ITEM]                                                                                                                                                          | [PAGE#CLUSTER]                          | [STATUS] |
| :-----: | :------------------------------------------------------------------------------------------------------------------------------------------------------------ | :-------------------------------------- | :------: |
|   [1]   | Wire `@stryker-mutator/core` + `@stryker-mutator/typescript-checker` + `@stryker-mutator/vitest-runner` + `@bufbuild/buf` into root `package.json` devDependencies (present in the workspace catalog, missing from the root manifest) so the mutation gate and the descriptor codegen rail resolve | `interchange/transport#CODEGEN_TOOLING` | QUEUED   |
|   [2]   | Author the minimal per-package `@rasm/<name>` `package.json` files for the five folders so the `libs/typescript/*` workspace glob resolves each package and its subpath `exports` (`.`/`./ui`/`./web`/`./node`/`./provisioning`) | `interchange/transport#TRANSPORT_AND_CLIENTS` | QUEUED   |

## [2]-[CROSS_FOLDER_FINALIZATION]

| [INDEX] | [ITEM]                                                                                                                                                          | [PAGE#CLUSTER]                          | [STATUS] |
| :-----: | :------------------------------------------------------------------------------------------------------------------------------------------------------------ | :-------------------------------------- | :------: |
|   [1]   | Cold all-PASS sweep flipping every folder's `ARCHITECTURE.md` owner-registry `[STATE]` clean against the planning standard review law                            | `interchange/codec-rails#CODEC_RAILS`   | ACTIVE   |
|   [2]   | Verify the mechanical guards materialize through the centralized config at implementation start: the `projection/**` `@connectrpc/*` ban, the `./provisioning` subpath isolating the deploy-time closure, the folder-scoped `browser`/`node`/`neutral` strata, and the `ui/**` `../platform` ban | `services/provisioning#PROVISIONING`    | QUEUED   |

## [3]-[SPIKE_RESOLUTION]

| [INDEX] | [ITEM]                                                                                                                                                          | [PAGE#CLUSTER]                          | [STATUS] |
| :-----: | :------------------------------------------------------------------------------------------------------------------------------------------------------------ | :-------------------------------------- | :------: |
|   [1]   | `ArtifactFrameRail` content-key byte-identity harness (tier-2, highest-risk): prove the assembled-blob `XxHash128` digest is byte-identical across runtimes (seed=0, fixed endianness, two-64-bit-half byte order); flips `ArtifactFrameRail` `SPIKE`→`FINALIZED` before the content-addressed cache is trusted | `interchange/codec-rails#CODEC_RAILS`   | SPIKE    |
|   [2]   | Standing-query window fold (`WindowKind`/`windowFold`) live-changefeed confirmation of tumbling/sliding/session behavior against a running op-log replay         | `projection/fold-algebra#FOLD_ALGEBRA`  | SPIKE    |
|   [3]   | Convergent `conflictPresenceFold` (LWW by HLC) cross-peer strong-eventual-consistency harness proving two divergent-delivery folds reach a byte-identical state  | `projection/fold-algebra#FOLD_ALGEBRA`  | SPIKE    |
|   [4]   | `ServiceWorkerHost` live-browser install/activate/skipWaiting + offline-queue redial-drain probe                                                                | `platform/service-worker#SERVICE_WORKER` | SPIKE   |
|   [5]   | `CrashTelemetry` live-browser global-capture probe proving crash marshalling ships through to the typed fault family                                            | `platform/error-boundary#ERROR_BOUNDARY` | SPIKE   |
|   [6]   | `PerformanceBudget` live-browser `PerformanceObserver` probe feeding LCP/INP/CLS/TTFB/FCP attribution into the `MetricRegistry` rows                            | `platform/web-vitals#WEB_VITALS`        | SPIKE    |

## [4]-[CROSS_BRANCH_PRECONDITION_DAG]

| [INDEX] | [ITEM]                                                                                                                                                          | [PAGE#CLUSTER]                          | [STATUS] |
| :-----: | :------------------------------------------------------------------------------------------------------------------------------------------------------------ | :-------------------------------------- | :------: |
|   [1]   | `GlbViewport` WebGL mesh render: blocked on the upstream mesh-wire promotion routed through the Tier-0 seam ledger; once the mesh wire lands `ui` admits the WebGL viewer and point-cloud/voxel decode cases land as sibling `Match` arms on `RendererBackend` | `ui/render-surfaces#GLB_VIEWPORT`       | BLOCKED  |
|   [2]   | `GraphFork` CRDT op vocabulary on `conflictPresenceFold`: blocked on the Tier-0 CrdtOp wire amendment before `projection` folds the CRDT op vocabulary          | `projection/fold-algebra#FOLD_ALGEBRA`  | BLOCKED  |
|   [3]   | Capability-descriptor SDK + MCP client codegen on `CapabilitySdk`: blocked on the Tier-0 capability-descriptor SDK source before the codegen leg emits the typed effect-classed SDK | `interchange/transport#CODEGEN_TOOLING` | BLOCKED  |

## [5]-[NEXT_LOOP_DEEPENING]

| [INDEX] | [ITEM]                                                                                                                                                          | [PAGE#CLUSTER]                          | [STATUS] |
| :-----: | :------------------------------------------------------------------------------------------------------------------------------------------------------------ | :-------------------------------------- | :------: |
|   [1]   | Drive each folder's refinement horizon: chunked frame streaming on `interchange`, the standing-query window vocabulary on `projection`, deeper render GPU paths and component-system role breadth on `ui`, deeper SPA infrastructure on `platform`, durable saga composition + hybrid-search re-ranking + IaC lifecycle drift detection on `services` | `interchange/transport#CODEGEN_TOOLING` | QUEUED   |
