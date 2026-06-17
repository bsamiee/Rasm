# [TYPESCRIPT_IDEAS]

The TypeScript-branch forward pool — the ambitious universal-library concepts imagined first and distilled into branch `TASKLOG.md` items, plus the cross-package concert the five folders compose and the folder-tagged refinement horizons aggregated from each folder's owner registry. A concept lands here when it couples TypeScript folders to each other or deepens one folder's surface; a concept that crosses a language boundary lives only in the Tier-0 [IDEAS](../../.planning/IDEAS.md) and routes through the [Tier-0 seam ledger](../../.planning/region-map/seam-splits.md). Mechanics live at the owning `pkg/page#CLUSTER`; owner state is read at the owning folder's `ARCHITECTURE.md` owner registry, never marked here.

## [1]-[BRANCH_CONCERT]

The cross-folder capability the five packages compose into one web/edge platform:

- One package family, every modality — the branch is an adoptable Effect library that simultaneously serves a flagship browser analytics/control SPA, a node durable-execution + multi-tenant Postgres backend, and a self-hosted-or-cloud two-mode IaC tier, each face built to full capability rather than one face built and the rest stubbed.
- One fold algebra, every feed — `projection` folds every decoded wire vocabulary through one `keyedFold` combinator over one `StreamPolicy` reconnect posture into `SubscriptionRef`-backed keyed maps, and `ui` subscribes through the one `AtomBinding` so a new feed is a fold row, never a parallel view-state library.
- One outbound verb face — every mutation crosses the single `CommandGateway` gated by the `AvailabilityStore` read fold, with `IntentRegistry` deep-link keys resolving into gateway verbs, so the SPA, the service-worker redial drain, and the deep-link router share one egress surface.
- One content-address identity in the browser — the `ArtifactFrameRail` reassembles content-addressed artifact bytes into an `XxHash128`-keyed blob the worker pool stitches off the main thread, and the off-main-thread decode pool, the offline last-good store, and the future mesh viewer all key on the same digest.
- One HLC ordering primitive surfaced as product UI — the `SkewBand` HLC skew-band fold is elevated to a first-class confidence-interval projection output, rendering a "within +/-N ms confidence" dashboard the desktop face structurally cannot own.
- One node tier as a peer of the topology — `services` dials the document and compute surfaces, ingests capture-event client-streams structurally non-dialable from the browser, and participates as a hub peer over the op-log changefeed, composing durable workflows, hybrid search, internal RPC, and the two-mode provisioning tier under one node runtime.

## [2]-[BRANCH_CONCEPTS]

| [INDEX] | [CONCEPT]                                                                                                       | [PAGE#CLUSTER]                                              |
| :-----: | :------------------------------------------------------------------------------------------------------------- | :--------------------------------------------------------- |
|   [1]   | The unified key-discriminated fold algebra over every decoded feed, one combinator one reconnect policy         | `projection/fold-algebra#FOLD_ALGEBRA`                     |
|   [2]   | The `SkewBand` HLC skew-band fold as a first-class confidence-interval projection output, the clock-uncertainty product UI | `projection/envelope-and-evidence#ENVELOPE_AND_EVIDENCE`   |
|   [3]   | The single outbound `CommandGateway` verb face gated by the `AvailabilityStore` read fold, with `IntentRegistry` deep-link keys | `interchange/gateway-and-quarantine#GATEWAY_AND_QUARANTINE` |
|   [4]   | The content-addressed `ArtifactFrameRail` reassembly stitched off the main thread by the `DecodeWorkerPool`     | `interchange/codec-rails#CODEC_RAILS` · `platform/platform-substrate#PLATFORM_SUBSTRATE` |
|   [5]   | The offline-first SPA — service-worker cache lifecycle draining the offline queue into the gateway on redial    | `platform/service-worker#SERVICE_WORKER`                   |
|   [6]   | The zero-router-package client routing — `Schema.Literal` route-key axis + nuqs query-state + a history `SubscriptionRef` | `platform/routing-navigation#ROUTING_NAVIGATION`          |
|   [7]   | The durable saga composition + parallel-branch fan-out over the cluster-backed `WorkflowEngine`                 | `services/durable-execution#DURABLE_EXECUTION`            |
|   [8]   | The fused semantic+lexical+trigram+phonetic weighted-rank search owner with HNSW tuning                         | `services/hybrid-search#HYBRID_SEARCH`                    |
|   [9]   | The two-mode IaC tier provisioning the observability stack the node tier and the SPA both export to and read from | `services/provisioning#PROVISIONING`                      |
|  [10]   | The role-based headless interaction-role component-system owner-block with runtime CSS-var theme sync           | `ui/component-system#COMPONENT_SYSTEM`                    |
|  [11]   | The `GeoSeriesSurface` maplibre + deck.gl interleaved geo render off the proto `GeometryPayload` projection      | `ui/render-surfaces#RENDER_SURFACES`                     |
|  [12]   | The browser crash-telemetry sink reconstructing every uncaught fault into the typed fault family, one collector path | `platform/error-boundary#ERROR_BOUNDARY`                 |

## [3]-[FOLDER_HORIZONS]

The next-deepening targets, folder-tagged and read from each folder's owner registry: a `SPIKE` owner is fence-complete with a residual local/runtime probe, a `BLOCKED` owner is fence-complete with its seam gated on an unmet cross-branch precondition routed through the Tier-0 seam ledger. Cross-language preconditions are named as Tier-0 seams, never restated.

| [INDEX] | [HORIZON]                                                                                                       | [PAGE#CLUSTER]                          |
| :-----: | :------------------------------------------------------------------------------------------------------------- | :-------------------------------------- |
|   [1]   | `interchange` — chunked frame streaming over `ArtifactFrameStreaming` and capability-descriptor SDK codegen (`CapabilitySdk` off the descriptor source at the Tier-0 seam); deeper standing-query windowing over the frame stream | `interchange/transport#CODEGEN_TOOLING` |
|   [2]   | `interchange` — `ArtifactFrameRail` content-key SPIKE: the tier-2 `XxHash128` byte-identity harness (seed=0, fixed endianness, two-64-bit-half byte order) flips it `FINALIZED` before the content-addressed cache is trusted cross-runtime | `interchange/codec-rails#CODEC_RAILS`   |
|   [3]   | `projection` — the standing-query window vocabulary SPIKE: `WindowKind` tumbling/sliding/session + `windowFold` IVM, against a live changefeed replay confirmation | `projection/fold-algebra#FOLD_ALGEBRA`  |
|   [4]   | `projection` — the convergent `conflictPresenceFold` SPIKE (LWW by HLC, `ConflictOutcomeKind` ledger): the cross-peer strong-eventual-consistency harness proving two divergent-delivery folds reach a byte-identical state | `projection/fold-algebra#FOLD_ALGEBRA`  |
|   [5]   | `platform` — `ServiceWorkerHost` offline-first lifecycle SPIKE: the live-browser install/activate/skipWaiting + offline-queue redial-drain probe | `platform/service-worker#SERVICE_WORKER` |
|   [6]   | `platform` — `CrashTelemetry` global-capture SPIKE: the live-browser probe proving global crash marshalling ships through to the typed fault family | `platform/error-boundary#ERROR_BOUNDARY` |
|   [7]   | `platform` — `PerformanceBudget` Core-Web-Vitals SPIKE: the live-browser `PerformanceObserver` probe feeding LCP/INP/CLS/TTFB/FCP attribution into the `MetricRegistry` rows | `platform/web-vitals#WEB_VITALS`        |
|   [8]   | `services` — durable saga composition + parallel-branch fan-out, hybrid-search re-ranking, IaC lifecycle drift detection; the `GraphFork` CRDT op vocabulary on `conflictPresenceFold` waits on the Tier-0 wire amendment | `services/durable-execution#DURABLE_EXECUTION` |
|   [9]   | `ui` — `GlbViewport` WebGL mesh render BLOCKED on the upstream mesh-wire promotion routed through the Tier-0 seam; once the mesh wire lands the point-cloud/voxel decode cases land as sibling `Match` arms on `RendererBackend` | `ui/render-surfaces#GLB_VIEWPORT`       |
