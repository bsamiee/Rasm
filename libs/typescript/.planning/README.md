# [TYPESCRIPT_PLANNING]

Rasm TypeScript owns the web UI: the co-hosted SPA, the evidence and benchmark dashboards, and the companion control panels. The four .NET packages ship the complete wire surface; TS consumes every contract as settled vocabulary and never re-designs a wire shape. Four ordered stages build the lib branch as foundationally sound lib-grade code.

## [1]-[PAGE_INDEX]

Two posture pages, the byte-to-typed wire-consumption inventory, the six deep domain pages, the cross-cutting test-strategy page, and the architecture-tree atlas. The deep pages land at stage [2.D] and the TS region ledger registers them; a page flips to finalized only on a cold all-PASS sweep.

| [INDEX] | [PAGE]                                          | [OWNS]                                                                | [STATE]   |
| :-----: | :---------------------------------------------- | :-------------------------------------------------------------------- | :-------- |
|   [1]   | [wire-consumption](wire-consumption.md)         | Contract inventory, codegen tooling, codec posture, tolerance law     | authored  |
|   [2]   | [architecture-posture](architecture-posture.md) | Effect app shape, state layer, host topology                          | authored  |
|   [3]   | [wire-contracts](wire-contracts.md)             | Byte-to-typed boundary; transport, five decode rails, quarantine fold | finalized |
|   [4]   | [state-stores](state-stores.md)                 | Key-discriminated folds, envelope carrier, staleness and availability | finalized |
|   [5]   | [view-surfaces](view-surfaces.md)               | Atom-bound leaf render and read-only observation routes               | finalized |
|   [6]   | [control-edge](control-edge.md)                 | The write edge; command gateway and intent registry                   | finalized |
|   [7]   | [runtime-host](runtime-host.md)                 | SPA root, Layer graph, platform, auth session, telemetry, build, pool | finalized |
|   [8]   | [node-tier](node-tier.md)                       | Node-only deploy, durable work, runner and scheduling, observability  | authored  |
|   [9]   | [test-strategy](test-strategy.md)               | Algebraic PBT spine, browser/E2E runner, mutation and durable harness | authored  |
|  [10]   | [architecture-tree](architecture-tree.md)       | The source-tree atlas, entry split, descriptor-pipeline placement     | authored  |

## [2]-[EXECUTION_STAGES]

Each stage's output is the next stage's sole input; a stage closes only when its gate holds.

| [INDEX] | [STAGE]                    | [OUTPUT]                                                    | [GATE]                                                          |
| :-----: | :------------------------- | :---------------------------------------------------------- | :-------------------------------------------------------------- |
|   [A]   | root infra finalization    | refreshed catalog, regenerated lockfile, closed root drift  | registry probes match; one install resolves; tsgo proof green   |
|   [B]   | lib scaffolding            | one real workspace package under `libs/typescript/`         | package tsgo proof green; workspace row links; zero C# coupling |
|   [C]   | api extraction             | per-dependency surface catalogues; executed wire admissions | every admitted dependency owns a catalogue page                 |
|   [D]   | planning corpus completion | deep pages, TS region ledger, finalized PAGE_INDEX rows     | cold-grader all-PASS sweeps; zero accepted findings             |

### [2.A]-[ROOT_INFRA_FINALIZATION]

The catalog refresh moves every consumed row in `pnpm-workspace.yaml` to newest stable in one pass. The stage gate is three proofs:

- Registry probe: `pnpm view <package> dist-tags.latest` per refreshed row; the catalog cell equals the probe output exactly under the `saveExact` law.
- One install: a single `pnpm install` resolves the entire refreshed catalog under `catalogMode: strict`.
- Typecheck proof: `pnpm exec tsgo --noEmit -p tsconfig.json` is the canonical compiler proof; `pnpm exec tsc --noEmit -p tsconfig.json` is a peer-bridge smoke.

Root infra drift closes in the same pass:

- Dependency-usage truth: every root package entry re-justifies against a real consumer or is deleted; a row whose package leaves the catalog is deleted with it.
- Lockfile law: every catalog or override edit regenerates `pnpm-lock.yaml` in the same change; workspace-owned overrides and `peerDependencyRules` rows re-validate against the refreshed catalog and obsolete rows are deleted.
- Engine alignment: `engines.node` and `packageManager` stay exact and bump to newest stable in the same pass.

### [2.B]-[LIB_SCAFFOLDING_LAW]

The lib branch is one real pnpm workspace package. The package name is decided at scaffolding and registered in the region ledger.

- `pnpm-workspace.yaml` `packages` gains the `libs/typescript/*` row; the package lives at `libs/typescript/<pkg>` with its own `package.json` carrying `catalog:` references only.
- The package owns its `tsconfig.json`, extending the root `tsconfig.base.json` and registering as a `references` row in the root solution `tsconfig.json`.
- Zero coupling to the C# tree: no import, path mapping, or build edge resolves into `libs/csharp`; integration crosses only the contracts inventoried at [wire-consumption](wire-consumption.md).
- Single entry point: one `exports` map with one public root; internal modules never publish.

Every module scaffolds under the Effect-first doctrine that [architecture-posture](architecture-posture.md) owns: one TS form per altitude, one owner per axis, admission once at the wire edge.

### [2.C]-[API_EXTRACTION]

Catalogue truth for every TS dependency — the TS analog of the C# per-package `.api` catalogues.

- The admission-pending rows of the ADMISSIONS_RECORD land in the catalog here.
- Extraction route: per admitted package, the surface catalogue derives from installed package source — the `node_modules/<pkg>` `exports` map resolved to its types entries, with extraction probes over the published `.d.ts` rollups. One catalogue page per package lands at `libs/typescript/<pkg>/.api/`.
- Resolution law: every API member written into a later planning fence resolves against a catalogue page; an unresolvable member becomes a RESEARCH row with an executable probe route, never prose.
- The ADMISSIONS_RECORD discharges every row at this stage and stage [2.A]; the testing stack is admitted here as well, derived from catalogue truth rather than carried forward.

### [2.D]-[PLANNING_CORPUS_COMPLETION]

- Deep pages author by the campaign method at `libs/csharp/.planning/campaign-method.md` to the review-law bar of the suite planning standard at `libs/csharp/.planning/README.md` — the suite review law with TS overlays: `ts contract` fences are the signature law; package resolution routes to the stage [2.C] catalogues; the comment law, hedge law, and catalogue law apply unchanged.
- The TS region ledger `libs/typescript/.planning/region-map/` is created when deep-page authoring starts, mirroring the suite ledger protocol: provisional rows before authoring, an owner-symbol registry, FINAL flips on the cold all-PASS sweep.
- Ideation-first refinement: per-page blueprint decisions — owner, axis, store, and layer assignments — precede authoring; the two wire pages of this corpus are the settled vocabulary every deep page composes.
- Review-repair waves: a cold grader sweeps each page; the page repairs and re-sweeps until zero accepted findings; only an all-PASS sweep flips a PAGE_INDEX row to finalized.

## [3]-[WIRE_PAGES]

The branch synchronizes from eleven C# `#TS_PROJECTION` clusters and authors no wire shape; each row is one C# Gate-3 wire page whose fence is the authoritative shape, transcribed verbatim into the named TS consumer. The [wire-consumption](wire-consumption.md) inventory is the canonical map; the [region ledger](region-map/seam-splits.md) carries the mechanics-to-consequence seam per row.

| [C# WIRE PAGE]                   | [CODEC]     | [TS CONSUMER]                                                        |
| :------------------------------- | :---------- | :------------------------------------------------------------------- |
| AppHost/lifecycle-and-drain      | json-stj    | state-stores#STREAM_FOLDS (`RuntimeFeed`)                            |
| AppHost/health-and-degradation   | json-stj    | state-stores#STREAM_FOLDS (`HealthStore`)                            |
| AppHost/support-bundles          | json-stj    | control-edge#COMMAND_GATEWAY (capture-support verb)                  |
| AppHost/runtime-ports            | json-stj    | state-stores#RECEIPT_AND_ENVELOPE_FOLDS (envelope carrier)           |
| Persistence/snapshot-codecs      | messagepack | wire-contracts#DECODE_RAILS (binary + `GeometryRail`)                |
| Persistence/sync-collaboration   | messagepack | wire-contracts#DECODE_RAILS + state-stores (`ConflictPresenceStore`) |
| Compute/remote-lane              | proto       | wire-contracts#TRANSPORT_AND_CLIENTS + `FaultDetailRail`             |
| Compute/progress-and-observation | proto       | state-stores#STREAM_FOLDS (`ProgressStore`)                          |
| Compute/receipts-and-benchmarks  | json-stj    | view-surfaces#OBSERVATION_ROUTES + state-stores (`ReceiptStore`)     |
| AppUi/commands-availability      | json-stj    | control-edge#COMMAND_GATEWAY + state-stores (`AvailabilityStore`)    |
| AppUi/diagnostics-evidence       | json-stj    | view-surfaces#OBSERVATION_ROUTES + state-stores (`EvidenceFeed`)     |

## [4]-[ADMISSIONS_RECORD]

The admission record owns package posture and catalogue state. Concrete package coordinates live in the workspace catalog, not in planning prose. Stage [2.A] discharges the toolchain drift and the wire admissions: TypeScript holds at its stable line as the peer-bridge compiler, the native-preview / tsgo native line is the canonical compiler proof and refreshes to the newest published nightly, the connect/buf/msgpack wire set is admitted with one-resolve landing conditions, and the superseded cluster-workflow row is deleted from the catalog. Stage [2.C] discharges the catalogue-pending rows: the test stack, the geo renderers, the OIDC/PKCE browser client, the durable-cluster and runner-backplane packages, and the deploy/provisioning providers each land their catalogue page from installed source. Rows whose package is not yet in the workspace catalog carry `catalog-pending` and name the catalog edit stage [2.A] or [2.C] lands; the geo renderers, the OIDC/PKCE client, and the mutation runner are the three catalog edits the corpus now requires.

| [PACKAGE]                  | [PAGE]               | [CATALOGUE]                                                                                                                                                                                                                                                                          |
| :------------------------- | :------------------- | :----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| typescript                 | architecture-posture | catalog                                                                                                                                                                                                                                                                              |
| effect                     | architecture-posture | catalog; core pin confirmed past the async-local-storage context-loss patched line; v3 held, v4 a designed-only growth row with unstable-import-path risk; consumers node-tier and runtime-host                                                                                      |
| @effect/platform           | architecture-posture | catalog                                                                                                                                                                                                                                                                              |
| @effect/platform-browser   | architecture-posture | catalog                                                                                                                                                                                                                                                                              |
| @effect/platform-node      | node-tier            | catalog                                                                                                                                                                                                                                                                              |
| @effect/rpc                | node-tier            | catalog; RPC pin confirmed past the async-local-storage context-loss patched line; consumers node-tier and runtime-host                                                                                                                                                              |
| @effect/cluster            | node-tier            | catalog; admitted successor — the durable-work split keeps the cluster runtime here while the workflow algebra moves to @effect/workflow; peer-locked to the held effect/platform/rpc/sql lines                                                                                      |
| @effect/workflow           | node-tier            | catalog; admitted successor — the workflow algebra extracted from the merged predecessor; peer-locked to the held effect/platform/rpc/experimental lines                                                                                                                             |
| @effect/cluster-workflow   | node-tier            | deleted; superseded by the @effect/cluster + @effect/workflow split, frozen upstream, zero catalog consumers and zero source consumers; the catalog row is removed at stage [2.A]                                                                                                    |
| @effect/sql                | node-tier            | catalog                                                                                                                                                                                                                                                                              |
| @effect/sql-pg             | node-tier            | catalog                                                                                                                                                                                                                                                                              |
| @effect/opentelemetry      | runtime-host         | catalog                                                                                                                                                                                                                                                                              |
| @effect-atom/atom          | state-stores         | catalog                                                                                                                                                                                                                                                                              |
| @effect-atom/atom-react    | view-surfaces        | catalog                                                                                                                                                                                                                                                                              |
| react                      | architecture-posture | catalog                                                                                                                                                                                                                                                                              |
| react-dom                  | architecture-posture | catalog                                                                                                                                                                                                                                                                              |
| vite                       | architecture-posture | catalog                                                                                                                                                                                                                                                                              |
| @typescript/native-preview | architecture-posture | catalog; the TS 7 / tsgo native compiler has no stable line, so the dev-nightly channel is canonical and the catalog pin refreshes to the newest published nightly at each stage-[2.A] pass; it is the canonical typecheck proof while stable typescript stays the peer-bridge smoke |
| @connectrpc/connect        | wire-consumption     | catalog; admitted — connect runtime for the generated GenService clients; peer-exact across the connect line, lands in the single stage-[2.A] resolve                                                                                                                                |
| @connectrpc/connect-web    | wire-consumption     | catalog; admitted — the grpc-web browser transport peer; pinned identically to @connectrpc/connect and resolves in the same pass                                                                                                                                                     |
| @bufbuild/protobuf         | wire-consumption     | catalog; admitted — generated-message runtime owning `create` and the descriptor types; peer of the protoc-gen-es and connect rows                                                                                                                                                   |
| @bufbuild/protoc-gen-es    | wire-consumption     | catalog; admitted — the single-plugin codegen for messages + service descriptors; version-locked to @bufbuild/protobuf                                                                                                                                                               |
| @bufbuild/buf              | wire-consumption     | catalog; admitted — the buf CLI that drives the codegen line over the app-root descriptor set; build-only tool, no runtime edge                                                                                                                                                      |
| @msgpack/msgpack           | wire-consumption     | catalog; admitted — the snapshot/sync messagepack decoder with `useBigInt64` 64-bit alignment and zero ExtensionCodec rows by contract                                                                                                                                               |
| @effect/experimental       | node-tier            | catalog; the peer-line note becomes the persistence and resumable-work substrate the durable tier reaches through `ClusterEngine`                                                                                                                                                    |
| ioredis                    | node-tier            | catalog; the Redis client for the multi-node runner backplane on `RunnerBackplane`                                                                                                                                                                                                   |
| @pulumi/pulumi             | node-tier            | catalog; the deployment core and Automation API engine `AutomationDriver` drives — `LocalWorkspace`/`Stack`/`events`/`remoteStack`/`StackReference`                                                                                                                                  |
| @pulumi/awsx               | node-tier            | catalog; the cloud and image-build/registry executors `ResourceComponent` composes for the worker tier — `ecr`/`ecs`/`ec2`/`lb`                                                                                                                                                      |
| @pulumi/kubernetes         | node-tier            | catalog; the k8s native, Helm, and kustomize provisioning modalities for the collector tier and server topologies                                                                                                                                                                   |
| @pulumi/policy             | node-tier            | catalog; the CrossGuard PolicyPack engine-side enforcement row on `PolicyGuard`, distinct from the program-side guard                                                                                                                                                                |
| @pulumi/esc-sdk            | node-tier            | catalog; the ESC environments/secrets source row on `SecretResolver`, host-agnostic alongside env and config reads                                                                                                                                                                   |
| @pulumi/command            | node-tier            | catalog; the command helper the deployment lifecycle composes                                                                                                                                                                                                                       |
| @pulumi/docker             | node-tier            | catalog; the container-image executor feeding the worker-tier deploy                                                                                                                                                                                                                 |
| @pulumi/random             | node-tier            | catalog; the random helper for unique resource naming in the deployment lifecycle                                                                                                                                                                                                    |
| @nx-extend/pulumi          | node-tier            | catalog; the monorepo orchestrator scoping deploys to the affected stacks                                                                                                                                                                                                            |
| maplibre-gl                | view-surfaces        | catalog-pending; the GL base-map substrate `GeoSeriesSurface` composes — pan/zoom/style-spec cartography; browser-only `BuildPipeline` bundle row; catalog edit lands stage [2.A]                                                                                                     |
| deck.gl                    | view-surfaces        | catalog-pending; the GPU overlay-layer engine `GeoSeriesSurface` composes for the at-scale geometry family, interleaved into the maplibre GL context via `MapboxOverlay`; browser-only; catalog edit lands stage [2.A]                                                                |
| oidc-pkce-client           | runtime-host         | catalog-pending; the browser authorization-code-with-PKCE client `AuthSession` drives — token-endpoint and silent refresh, no implicit grant, no client secret; catalog edit lands stage [2.A]                                                                                       |
| vitest                     | test-strategy        | catalog; the runner core for the unit spine and the shared browser-mode project surface                                                                                                                                                                                              |
| @effect/vitest             | test-strategy        | catalog; the Effect-aware bridge auto-providing TestClock and TestServices for layer-shared suites                                                                                                                                                                                   |
| fast-check                 | test-strategy        | catalog; the property generator for the algebraic-law packs and model-based command sequences                                                                                                                                                                                        |
| @vitest/coverage-v8        | test-strategy        | catalog; the v8 coverage provider for the per-file statements/branches/functions floor                                                                                                                                                                                               |
| @vitest/browser-playwright | test-strategy        | catalog; the playwright-backed browser provider for the DOM-mode atom-bound view specs                                                                                                                                                                                               |
| @playwright/test           | test-strategy        | catalog; the end-to-end driver for the deep-link and command-intent navigation flows                                                                                                                                                                                                 |
| testcontainers             | test-strategy        | catalog; the ephemeral-container harness proving the durable cluster against real Postgres and Redis engines                                                                                                                                                                         |
| @stryker-mutator/core      | test-strategy        | catalog-pending; the mutation kill-ratio gate over the unit spine with the typescript checker; the vitest-runner plugin is the peer row; catalog edit lands stage [2.A]                                                                                                               |

## [5]-[DEEPENING_HORIZON]

The geo, auth, durable-cluster, runner-backplane, and test-strategy gap findings are landed as owners on their pages and as ADMISSIONS rows above. The remaining Effect-completeness findings are forward-design rows the next deepening wave lands as rows or owner-symbols on the existing pages — each is a row on a named cluster, never a new page, and each names an admitted-but-unexploited package the external-lib-maximization law requires a page to own.

- `RuntimeConfig` — runtime-host#COMPOSITION_AND_PLATFORM: the one typed `effect` `Config` schema and `ConfigProvider` layer making the "one domain config value" claim real; the browser variant projects `import.meta.env` through `ConfigProvider.fromJson`, the node variant rides `SecretResolver` as the provider source; consumed by `BrowserPlatform`, `WireTransport`, and `SelfTelemetry`.
- `EncodeRail` — wire-contracts#DECODE_RAILS: the encode half symmetric to the decode rail, keyed by direction; `CommandGateway` `CommandPayloadWire` encode and `DeepLinkBinding` query-string round-trip both `Schema.encode`-produced, never literal-constructed; `@bufbuild/protobuf` `create` stays the proto constructor.
- Schema refinement vocabulary — wire-contracts#DECODE_RAILS: `Schema.brand` for guid and 16-byte content-key identifiers and `Schema.filter` for the HLC-logical number-envelope bound and the fixed-width header discriminants, so the `wire-consumption#VERSIONING_LAW` invariants become decode-enforced rather than prose; rows on `DecodeRail`, zero new rail.
- `MetricRegistry` deepening — runtime-host#SELF_TELEMETRY: a bounded `Metric` instrument vocabulary mirroring the C# `HostMetrics` names, an `Effect.withSpan` span vocabulary, the `@effect/opentelemetry` `WebSdk` Layer with resource attributes and OTLP trace+metric exporters, plus a Core Web Vitals metric family; the store folds and `CommandGateway` consume the instruments at their seams.
- `StreamPolicy` — state-stores#STREAM_FOLDS: the bounded `Schedule` reconnect/backoff vocabulary and the `Stream` operator posture (buffer/throttle/groupedWithin/scan) every fold composes rather than improvising reconnect; the staleness-forward retry value grounds here.
- `LocalPersistence` — runtime-host#BUILD_AND_WORKER_POOL: `@effect/experimental` `Persistence`/`KeyValueStore` holding Schema-encoded last-good store snapshots over `BrowserPlatform`'s key-value binding plus the offline command queue draining through `CommandGateway` on redial; the `DecodeWorkerPool` transferable-buffer discipline (decoded geometry and snapshot blobs cross the worker port as `Transferable`) is a row on the pool owner.
- `SqlBoundary` deepening / `InternalRpc` grounding — node-tier#DURABLE_WORK_AND_RPC: `@effect/sql-pg` `PgClient` Layer, `Model.Class` row schemas, the `@effect/sql` `Migrator`, and the pool/config boundary `ClusterEngine` consumes as the persistence backend; the `@effect/rpc` `RpcGroup`/`RpcSerialization`/`RpcClient`/`RpcServer` surface and the `WorkflowProxy.toRpcGroup`/`WorkflowProxyServer` projection grounding `InternalRpc`'s workflow-exposure mechanism.
- `StackOutputs` — node-tier#RESOURCE_COMPONENTS_AND_LIFECYCLE: the typed cross-stack `StackReference` contract exporting the C# server topology outputs (Postgres/Timescale DSN, object-store bucket+endpoint, Redis URL, collector OTLP endpoint, SPA-host origin) the composition root consumes, plus the enumerated topology `ResourceComponent` rows; `@effect/cli` binds the `AutomationDriver` lifecycle verbs as typed commands.
- Admission honesty — node-tier and architecture-posture: the `@effect/ai*` and `@effect/cli` catalog rows are admitted-but-unowned; the next wave either lands an AI-activity row on `ActivityOwner` (a durable activity whose body is an `@effect/ai` provider call with retry and compensation) and the `@effect/cli` `AutomationDriver` binding, or deletes the unjustified rows at stage [2.A] — every admitted package carries a page→catalogue row or the row is removed.
- Atom devtools — view-surfaces#ATOM_BINDING_AND_PRIMITIVES: a development-build-only atom-inspector row on `AtomBinding`, stripped from the production bundle by `BuildPipeline`, so fold state is inspectable without a second state binding.
