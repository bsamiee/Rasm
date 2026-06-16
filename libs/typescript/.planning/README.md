# [TYPESCRIPT_PLANNING]

Rasm TypeScript is a first-class, agnostic, universal Effect library branch — ONE package whose four top-level folders ARE the higher-order bounded domains, never a `packages/@rasm/<pkg>` nesting, never four separately-published packages with cross-package import edges, and never a thin coupling to the C# tree. The branch ships as a single npm package `@rasm/ts` with four subpath exports (`.`/`./web`/`./node`/`./provisioning`); the domains are internal folders, the inter-domain dependency direction is folder-scoped ESLint `no-restricted-imports`, and a "cross-domain import" is an intra-package module edge, not a published-package boundary. This file is the SUITE PLANNING STANDARD analog for the branch: it carries the cross-domain PAGE_INDEX, the wire-page map, the ADMISSIONS_RECORD, the cross-package laws, and the refinement horizon. It carries NO DENSITY_BAR — the four per-domain charters each own their own DENSITY_BAR, satisfying the single-state-surface law. The four .NET packages ship the complete wire surface; the branch consumes every contract as settled vocabulary and authors no wire shape.

## [1]-[BRANCH_TOPOLOGY]

Four FLAT higher-order domain folders inside ONE package, each a real bounded concern a senior architect names from first principles; the publication/reachability split is a subpath-export attribute plus a folder-scoped lint stratum, never a separate package and never the folder axis. The branch is one npm package `@rasm/ts` under `libs/typescript/`; `nx libsDir=libs/typescript` registers it as ONE Nx project. The bundle fence is the folder-stratum `no-restricted-imports` graph (`browser`/`node`/`neutral`) the single project's ESLint flat-config enforces — the four domains never become four `@rasm/<pkg>` workspace packages.

| [INDEX] | [DOMAIN]      | [STRATUM] | [SUBPATH] | [OWNS]                                                                              | [CHARTER]                                |
| :-----: | :------------ | :-------- | :-------- | :--------------------------------------------------------------------------------- | :--------------------------------------- |
|   [1]   | `interchange` | neutral   | `.`       | the entire byte-to-typed-and-back wire boundary: transport, six-codec rails, artifact frames, fault reconstruction, quarantine, outbound gateway | [interchange](../interchange/.planning/README.md) |
|   [2]   | `projection`  | neutral   | `.`       | the unified key-discriminated transport-free fold algebra over decoded wire vocabulary | [projection](../projection/.planning/README.md)  |
|   [3]   | `web`         | browser   | `./web`   | the complete browser web/UI/UX platform — binding, render, component system, host runtime, platform substrate (browser entry) | [web](../web/.planning/README.md)        |
|   [4]   | `services`    | node      | `./node` + `./provisioning` | the node services tier AND the infrastructure that hosts it — durable, persistence, search, RPC, provisioning (node entry) | [services](../services/.planning/README.md) |

## [2]-[PAGE_INDEX]

Twelve domain pages across four charters plus four branch-level non-domain pages. Per the single-state-surface law, page-finalization state lives in exactly one cell — the owning charter PAGE_INDEX `[STATE]` column — so this index carries no STATE column and routes to the charter; a page flips to finalized only on a cold all-PASS sweep of the re-derived corpus.

| [INDEX] | [PAGE]                                          | [DOMAIN]            | [OWNS]                                                                |
| :-----: | :---------------------------------------------- | :------------------ | :------------------------------------------------------------------- |
|   [1]   | interchange/transport.md                        | `@rasm/interchange` | shared grpc-web transport, four generated clients, the buf descriptor codegen edge, the transport-capability shape |
|   [2]   | interchange/codec-rails.md                      | `@rasm/interchange` | the six-codec rail family, encode direction, refinement, geometry, artifact frames, exhaustive fault family |
|   [3]   | interchange/gateway-and-quarantine.md           | `@rasm/interchange` | the quarantine fold, contract inventory, intent registry, outbound command gateway |
|   [4]   | projection/fold-algebra.md                      | `@rasm/projection`  | the one StreamPolicy, the key-discriminated fold combinator, the five stream stores, the standing-query horizon |
|   [5]   | projection/envelope-and-evidence.md             | `@rasm/projection`  | the receipt/evidence/availability folds, the envelope carrier, the SkewBand HLC confidence-interval projection |
|   [6]   | web/binding.md                                  | `@rasm/web`         | AtomBinding, DeepLinkBinding, url-state, offline persistence, undo/redo, dev inspector |
|   [7]   | web/render-surfaces.md                          | `@rasm/web`         | the leaf observation routes, GeoSeriesSurface/GeoSeriesLayer, the GlbViewport refinement-horizon entry |
|   [8]   | web/component-system.md                         | `@rasm/web`         | the role-based headless interaction-role vocabulary owner-block, theme tokens, CSS-var sync |
|   [9]   | web/host-runtime.md                             | `@rasm/web`         | CompositionRoot, BrowserPlatform, AuthSession, RuntimeConfig, the SPA browser entry Layer graph |
|  [10]   | web/platform-substrate.md                       | `@rasm/web`         | SelfTelemetry, MetricRegistry, BuildPipeline, DecodeWorkerPool, LocalPersistence |
|  [11]   | services/durable-execution.md                   | `@rasm/services`    | WorkflowOwner, ActivityOwner, ClusterEngine, the AiProvider literal axis, the agent journal, resilience |
|  [12]   | services/persistence.md                         | `@rasm/services`    | SqlBoundary, the entity-model registry, multi-tenant RLS, jobs/DLQ, events, notifications, assets/export-codec, flags |
|  [13]   | services/hybrid-search.md                       | `@rasm/services`    | the semantic+lexical+trigram+phonetic fused weighted-rank search owner |
|  [14]   | services/internal-rpc.md                        | `@rasm/services`    | InternalRpc RpcGroup, WorkflowProxy projection, RunnerBackplane, ScheduledWork |
|  [15]   | services/provisioning.md                        | `@rasm/services`    | the data/compute/observe tier model, two-mode dispatch, the ./provisioning subpath, StackOutputs, bootstrap |
|  [16]   | [architecture-posture](architecture-posture.md) | branch              | altitude grammar, app-service budget, neutrality strata, runtime, admission, anti-spam/co-location, RULE_ENFORCEMENT |
|  [17]   | [test-strategy](test-strategy.md)               | branch              | PBT spine, BrowserE2E, MutationHarness; per-domain project rows in one vitest.workspace.ts |
|  [18]   | [architecture-tree](architecture-tree.md)       | branch              | the flat four-domain-folder single-package source tree, the folder-stratum import graph, the descriptor placement, the GlbViewport horizon |

services/durable-execution.md, services/persistence.md, services/hybrid-search.md, services/internal-rpc.md, and services/provisioning.md are five domain pages of `@rasm/services`; the page set models the implementation file set, where `provisioning.md` transcribes the `./provisioning` exports subpath.

## [3]-[WIRE_PAGES]

The branch synchronizes from eleven C# `#TS_PROJECTION` clusters and authors no wire shape; each row is one C# Gate-3 wire page whose fence is the authoritative shape, transcribed verbatim into the named TS consumer. The `interchange/gateway-and-quarantine.md` CONTRACT_INVENTORY is the canonical map; the [branch ledger](region-map/seam-splits.md) carries the mechanics-to-consequence seam per row.

| [C# WIRE PAGE]                   | [CODEC]     | [TS CONSUMER]                                                                       |
| :------------------------------- | :---------- | :--------------------------------------------------------------------------------- |
| AppHost/lifecycle-and-drain      | json-stj    | projection/fold-algebra#FOLD_ALGEBRA (`RuntimeFeed`)                                |
| AppHost/health-and-degradation   | json-stj    | projection/fold-algebra#FOLD_ALGEBRA (`HealthStore`)                               |
| AppHost/support-bundles          | json-stj    | interchange/gateway-and-quarantine#GATEWAY_AND_QUARANTINE (capture-support verb)    |
| AppHost/runtime-ports            | json-stj    | projection/envelope-and-evidence#ENVELOPE_AND_EVIDENCE (envelope carrier)           |
| Persistence/snapshot-codecs      | messagepack | interchange/codec-rails#CODEC_RAILS (`GeometryRail`) + projection (`SnapshotFeed`)  |
| Persistence/sync-collaboration   | messagepack | interchange/codec-rails#CODEC_RAILS + projection (`ConflictPresenceStore`)          |
| Compute/remote-lane              | proto       | interchange/transport#TRANSPORT_AND_CLIENTS + codec-rails `ArtifactFrameRail`/`FaultDetailRail` |
| Compute/progress-and-observation | proto       | projection/fold-algebra#FOLD_ALGEBRA (`ProgressStore`)                             |
| Compute/receipts-and-benchmarks  | json-stj    | web/render-surfaces#RENDER_SURFACES (`BenchmarkRoute`) + projection (`ReceiptStore`) |
| AppUi/commands-availability      | json-stj    | interchange/gateway-and-quarantine#GATEWAY_AND_QUARANTINE + projection (`AvailabilityStore`) |
| AppUi/diagnostics-evidence       | json-stj    | web/render-surfaces#RENDER_SURFACES (`EvidenceTimelineRoute`) + projection (`EvidenceFeed`) |

## [4]-[CROSS_PACKAGE_LAWS]

The branch-level non-domain pages legislate across all four domains:

- [architecture-posture](architecture-posture.md): the seven-altitude one-form grammar, the closed app-service budget, the platform-neutrality strata, the held Effect-v3 runtime with the dual-compiler tsgo typecheck floor, the admission doctrine collapsing the AI provider set into one `AiProvider` axis, the anti-spam cluster law, the altitude-co-location law, and the RULE_ENFORCEMENT mechanical-guard table (projection-no-connect banned-import; provisioning `./provisioning` subpath; Effect-KV-over-idb; RpcGroup-single-owner; Model.Class-single-owner; exhaustive-fault-family).
- [test-strategy](test-strategy.md): the UnitProperty PBT spine, the BrowserE2E DOM/navigation runner, and the MutationHarness/DurableHarness gate, with per-domain `vitest`/`stryker` project rows in the single shared `vitest.workspace.ts` at the branch root.
- [architecture-tree](architecture-tree.md): the flat four-domain-folder source tree inside one package, the inter-domain dependency direction (folder-scoped `no-restricted-imports` strata), the browser/node/neutral publication split as a subpath-export attribute, the descriptor-pipeline placement in the `interchange` folder, and the `GlbViewport` refinement-horizon entry.

The single-state-surface law: one owner, one state cell, across two orthogonal axes that never conflate. The page-finalization axis (`provisional|finalized`) lives only in each charter PAGE_INDEX `[STATE]` column — the branch README PAGE_INDEX and the ledger `page-regions.md` carry no page-state cell and route to the charter. The owner-finalization axis (`FINALIZED|SPIKE`) lives only in each charter DENSITY_BAR `[STATE]` column — a `FINALIZED` owner is a transcription-complete fence with no open gate, a `SPIKE` owner is fence-complete with a residual probe. This branch README references the four charters and carries neither. The branch ledger lives at [region-map](region-map/README.md) as ONE ledger with four per-domain blocks, mirroring the C# suite ledger shape.

## [5]-[ADMISSIONS_RECORD]

Branch-cross-cutting toolchain and the executed catalog edits; concrete package coordinates live in the workspace catalog (`pnpm-workspace.yaml` `catalog:`), never in planning prose. Per-domain ADMISSIONS_RECORD rows live in each charter.

| [PACKAGE]                     | [STRATUM] | [CATALOGUE]                                                                                                         |
| :---------------------------- | :-------- | :----------------------------------------------------------------------------------------------------------------- |
| typescript / tsgo             | toolchain | catalog; stable `typescript` peer-bridge smoke, native tsgo dev-nightly canonical `--noEmit`, refreshed each loop  |
| effect (core, v3-held)        | toolchain | catalog; v3 held past the async-local-storage patched line; v4 a designed-only growth row                          |
| eslint flat-config            | toolchain | catalog; the bundle-fence owner — folder-scoped `no-restricted-imports` enforcing the `browser`/`node`/`neutral` stratum graph inside the one project |
| @bufbuild/buf                 | interchange | catalog (`allowBuilds: true`); the build-time descriptor codegen CLI                                       |
| xxhash-wasm                   | interchange | admitted; the `ContentKey` XxHash128 16-byte digest, byte-identical to C# `System.IO.Hashing.XxHash128`    |
| maplibre-gl + @deck.gl/*      | web  | admitted; `core` engine + `layers` constructors + `mapbox` `MapboxOverlay` interleave for the geo render          |
| @opentelemetry/sdk-trace-web  | web  | admitted; the browser OTel exporter the `WebSdk` layer binds (the node SDK cannot serve the browser stratum)      |
| arctic                        | web  | admitted; the single OAuth/OIDC owner — the `oauth4webapi` pending row is void                                     |
| idb-keyval                    | web  | admitted; the offline IndexedDB backing store under the Effect `KeyValueStore` abstraction, never called directly |
| @stryker-mutator/core         | test      | admitted; the mutation kill-ratio gate (vitest-runner + typescript-checker peers); catalog-only until consumed     |
| @effect/cluster-workflow      | (deleted) | deleted; superseded by the `@effect/cluster` + `@effect/workflow` split, zero catalog and source consumers         |
| oauth4webapi, @effect/printer-ansi | (void) | not admitted; `arctic` supersedes the OAuth row, `@effect/cli` owns its own renderer                            |

## [6]-[REFINEMENT_HORIZON]

The single render refinement-horizon owner is `GlbViewport`, the WebGL mesh render (web/render-surfaces.md REFINEMENT_HORIZON + architecture-tree.md + the four-end cross-branch precondition-DAG seam in region-map/seam-splits.md): admitted only when the upstream C# `remote-lane.md#TS_PROJECTION` promotes the mesh shape, the Python `libs/python/compute` IFC->GLB two-hop companion authors, and only then `@rasm/web` admits a WebGL viewer with a `Schema.Literal` renderer-backend axis. The GLB byte/artifact layer is NOT on this horizon — `@rasm/interchange` `ArtifactFrameRail` consumes the server-streamed content-addressed GLB artifact bytes today over the existing wire fence.

Three further cross-branch precondition-DAG horizons the branch records but cannot pre-author (SIGNATURE_LAW forbids authoring an unsettled wire shape), each routed in region-map/seam-splits.md as a TS-downstream entry: (1) the GraphFork CRDT / branch-merge-time-travel projection-fold row waits on the C# `sync-collaboration#MERGE_LAW` op-log amendment; (2) the capability-descriptor SDK + MCP client codegen row on `@rasm/interchange` waits on the C# `capability-registry#CAPABILITY_CATALOG`; (3) the BCF anchor-algebra web render-surface + interchange anchor-codec waits on the C# `annotation#ANCHOR_ALGEBRA`. Each domain charter carries its own REFINEMENT_HORIZON entry route; the next loop drives each domain's deepening (deeper codec streaming, deeper fold combinators including the standing-query window vocabulary, deeper render GPU paths, deeper durable saga composition, deeper IaC lifecycle) and resolves the GlbViewport DAG bottom-up from the C# fence amendment.
