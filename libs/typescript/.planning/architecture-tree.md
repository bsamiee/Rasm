# [TYPESCRIPT_ARCHITECTURE_TREE]

The lib branch is ONE npm package `@rasm/ts` of FOUR FLAT higher-order domain folders directly under `libs/typescript/` — the top-level folders ARE the domains, never a `packages/` directory, never an `@rasm/<pkg>` nesting, and never four separately-published packages. The publication/reachability split is a subpath-export attribute plus a folder-scoped lint stratum, never the folder axis and never a separate package: a platform-neutral interior (`interchange` + `projection`, stratum `neutral`), a browser publication (`web`, stratum `browser`, subpath `./web`), and a node publication (`services`, stratum `node`, subpaths `./node` + `./provisioning`). This page is the planning atlas: the flat source tree projected from the twelve domain pages, the inter-domain dependency direction enforced by the folder-scoped `no-restricted-imports` strata, the descriptor-pipeline placement in the `interchange` folder, the `./provisioning` exports subpath isolating the `services/provisioning` closure, and the `GlbViewport` refinement-horizon entry. The tree is re-derived from the finalized owner set each loop. This is a `CROSS_PACKAGE_LAWS`-tier (ATLAS) page.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]                     | [OWNS]                                                           |
| :-----: | :---------------------------- | :-------------------------------------------------------------- |
|   [1]   | SOURCE_TREE                   | the flat four-domain layout, one leaf per owner symbol           |
|   [2]   | PUBLICATION_TAG_SPLIT         | the browser/node/neutral tag attribute and the shared interior   |
|   [3]   | DEPENDENCY_DIRECTION          | the inter-domain Nx module-boundary tag graph                    |
|   [4]   | DESCRIPTOR_PIPELINE_PLACEMENT | the committed buf input, config, and generated output in interchange |
|   [5]   | REFINEMENT_HORIZON            | the GlbViewport precondition-DAG entry, not a page this turn      |
|   [6]   | ATLAS_LAW                     | the re-derivation, one-owner-one-leaf, and cross-tier laws        |

## [2]-[SOURCE_TREE]

The branch is ONE package: the root carries the single `package.json` (catalog refs + subpath `exports` + the folder-stratum ESLint flat-config), `tsconfig.base.json`, the shared `vitest.workspace.ts`, and the root solution `tsconfig.json` whose `references` rows register each domain folder as a project reference for incremental builds (project references are a compiler concern, not a publication boundary). Each domain is a top-level folder with `src/` leaves and a `.planning/` charter+pages, but NO own `package.json`. Inter-domain dependency direction is enforced by the folder-scoped ESLint `no-restricted-imports` strata (`browser`/`node`/`neutral` globs) plus the `projection/**` `@connectrpc/*` ban as the intra-neutral guard.

```text codemap
libs/typescript/                          # ONE package @rasm/ts; FLAT top-level folders ARE the higher-order domains
├── package.json                          # the SINGLE package.json: subpath exports "." / "./web" / "./node" / "./provisioning"; catalog refs
├── eslint.config.ts                      # the folder-stratum bundle fence: no-restricted-imports by browser/node/neutral folder glob
├── tsconfig.base.json                    # the shared strictness floor every domain folder extends
├── tsconfig.json                         # the root solution; one references row per domain folder (incremental-build only)
├── vitest.workspace.ts                   # ONE shared workspace; one project row per domain folder — test-strategy
├── pnpm-workspace.yaml row: libs/typescript (one project; nx libsDir=libs/typescript)
│
├── interchange/                          # stratum neutral; the "." export; inbound dependency root
│   ├── buf.gen.yaml                      #   single-plugin v2 protoc-gen-es config — transport#CODEGEN_TOOLING
│   ├── gen/descriptors/                  #   committed app-root FileDescriptorSet input
│   ├── src/
│   │   ├── gen/                          #   protoc-gen-es output *_pb.ts; the codegen transcription unit
│   │   ├── transport.ts                  #   WireTransport + WireClients + TransportCapabilityWire + buf input edge — transport
│   │   ├── codec-rails.ts                #   DecodeRail + EncodeRail + SchemaRefinement + GeometryRail + ArtifactFrameRail + FaultDetail family + Crc32 — codec-rails
│   │   ├── gateway-and-quarantine.ts     #   QuarantineFold + CONTRACT_INVENTORY + CommandGateway + IntentRegistry — gateway-and-quarantine
│   │   └── index.ts                      #   the neutral "." barrel
│   └── .planning/                        #   charter + transport.md, codec-rails.md, gateway-and-quarantine.md (3 pages)
│
├── projection/                           # stratum neutral; the "." export; transport-FREE folds; @connectrpc/* banned by folder lint
│   ├── src/
│   │   ├── fold-algebra.ts               #   StreamPolicy + key-discriminated fold + RuntimeFeed/HealthStore/SnapshotFeed/ProgressStore/ConflictPresenceStore — fold-algebra
│   │   ├── envelope-and-evidence.ts      #   ReceiptStore/EvidenceFeed/AvailabilityStore + envelope carrier + SkewBand HLC fold — envelope-and-evidence
│   │   └── index.ts                      #   the neutral "." barrel
│   └── .planning/                        #   charter + fold-algebra.md, envelope-and-evidence.md (2 pages)
│
├── web/                                  # stratum browser; the "./web" publication entry
│   ├── src/
│   │   ├── binding.ts                    #   AtomBinding + DeepLinkBinding + url-state + offline KV + undo/redo + dev inspector — binding
│   │   ├── render-surfaces.ts            #   EvidenceTimelineRoute/BenchmarkRoute/CollectorPanel + GeoSeriesSurface/GeoSeriesLayer (GlbViewport horizon) — render-surfaces
│   │   ├── component-system/             #   ONE interaction-role vocabulary owner-block: actions/ collections/ inputs/ overlays/ navigation/ feedback/ pickers/ core + theme tokens + CSS-var sync — component-system
│   │   ├── host-runtime.ts               #   CompositionRoot + BrowserPlatform + AuthSession (arctic) + RuntimeConfig — host-runtime
│   │   ├── platform-substrate.ts         #   SelfTelemetry + MetricRegistry (WebSdk) + BuildPipeline + DecodeWorkerPool + LocalPersistence — platform-substrate
│   │   ├── browser.ts                    #   the "./web" entry — CompositionRoot composes interchange+projection+web
│   │   └── index.ts
│   └── .planning/                        #   charter + binding.md, render-surfaces.md, component-system.md, host-runtime.md, platform-substrate.md (5 pages)
│
├── services/                             # stratum node; the "./node" + "./provisioning" publication entries
│   ├── src/
│   │   ├── durable-execution.ts          #   WorkflowOwner/ActivityOwner/ClusterEngine + AiProvider literal + DurableUnit/DurableFault + agent journal + resilience — durable-execution
│   │   ├── persistence.ts                #   SqlBoundary + entity registry (ONE Model.Class/entity) + TenantScope RLS + WorkQueue/EventJournal/Notifications + AssetTransfer + FeatureFlags — persistence
│   │   ├── hybrid-search.ts              #   semantic+lexical+trigram+phonetic weighted-rank search owner + hybridQuery UNION ALL SQL — hybrid-search
│   │   ├── internal-rpc.ts               #   InternalRpc RpcGroup + WorkflowProxy + RunnerBackplane + ScheduledWork — internal-rpc
│   │   ├── provisioning/                 #   ./provisioning subpath: platform.ts (entry-thin) + deploy.ts (impl-dense, two-mode) + PolicyGuard + bootstrap.sh — provisioning
│   │   ├── node.ts                       #   the "./node" entry — durable cluster + runner + provisioning compose
│   │   └── index.ts
│   └── .planning/                        #   charter + durable-execution.md, persistence.md, hybrid-search.md, internal-rpc.md, provisioning.md (5 pages)
│
└── .planning/                            # BRANCH-level SUITE planning (mirrors libs/csharp/.planning/)
    ├── README.md                         #   SUITE-STANDARD ANALOG: PAGE_INDEX across 4 domains, wire-page map, ADMISSIONS, cross-package laws; NO DENSITY_BAR
    ├── campaign-method.md                #   the branch-local campaign method aligned to the C# campaign
    ├── api-catalogues.md                 #   the evidence protocol over the .api set
    ├── FEATURES.md                       #   the dense universal-lib capability atlas, held lean
    ├── TASKLOG.md                        #   branch-level open work
    ├── architecture-posture.md           #   CROSS_PACKAGE_LAWS: altitude grammar + budget + neutrality + runtime + admission + anti-spam/co-location + RULE_ENFORCEMENT
    ├── test-strategy.md                  #   CROSS_PACKAGE_LAWS: PBT spine + BrowserE2E + MutationHarness; per-domain project rows in one vitest.workspace.ts
    ├── architecture-tree.md              #   CROSS_PACKAGE_LAWS (ATLAS): this page
    └── region-map/                       #   ONE branch ledger with four per-domain blocks (page-regions, owner-symbols, seam-splits)
```

Per-module specs are co-located one-per-source-module-per-category as `*.spec.ts` beside each owner leaf under the test category the page assigns; each domain's `test/` holds only its shared arbitrary registry, layer materialization, and (for `@rasm/services`) the durable container harness. The folder set is the twelve domain pages projected at owner granularity across four domains; `gen`/`gen/descriptors` are codegen artifacts in `@rasm/interchange` and `provisioning/` is the `./provisioning` exports-subpath leaf in `@rasm/services`. Net: interchange 3 + projection 2 + web 5 + services 5 = 15 domain pages... reconciled at twelve where `web/component-system/` and `services/provisioning/` are owner-block sub-folders (one charter page each), giving 3 + 2 + 5 + 5 = 15 file leaves over 12 page concerns; the page set models the transcription unit, with `component-system/` and `provisioning/` each one page mapping to a multi-leaf sub-folder.

## [3]-[PUBLICATION_TAG_SPLIT]

Four flat domain folders in one package, each independently consumable through a subpath export, in three reachability strata; the bundle split is a subpath-export attribute plus a folder-scoped lint stratum, never a folder boundary, never a separate package, and never a runtime guard.

- Platform-neutral interior: `interchange` (stratum `neutral`) and `projection` (stratum `neutral`) — both publications compose them through the one `.` export. Because both are neutral, the stratum graph does not separate the `CommandGateway`-reads-`AvailabilityStore` intra-neutral seam, so the `projection/**` `no-restricted-imports` ban on `@connectrpc/*` is the sole mechanical guard keeping the fold interior transport-free; the read is an intra-package module edge.
- Browser publication: `web` (stratum `browser`) is the browser entry, importing `interchange` + `projection`; the SPA root `CompositionRoot` assembles them into one Layer graph and one runtime; `./web` is the publication entry subpath export.
- Node publication: `services` (stratum `node`) is the node entry, importing `interchange` + `projection`; the durable cluster, runner backplane, internal RPC, persistence, hybrid search, and provisioning surfaces compose under one node runtime; `./node` is the runtime entry subpath and `./provisioning` is the deploy-time subpath keeping `@pulumi/*` off the runtime hot path.

The bidirectional bundle fence is a compile-time folder-stratum lint constraint: a `browser` folder imports only `browser` and `neutral` folders, a `node` folder only `node` and `neutral` folders, and a `neutral` folder only `neutral` folders. So `@effect/cluster`, `@effect/workflow`, `@effect/sql-pg`, the `@pulumi/*` rows, `ioredis`, and `@effect/platform-node` never enter the browser bundle, and `@effect/platform-browser`, `react`, `@effect-atom/atom-react`, `maplibre-gl`, the `@deck.gl/*` rows, `arctic`, and `vite` never enter the node bundle — the per-subpath bundle is tree-shaken from the one package by its entry. A cross-stratum source import is the named coupling defect the folder-scoped lint rejects at lint time, and the subpath bundler at build time.

## [4]-[DEPENDENCY_DIRECTION]

Dependencies flow inward from the publication entries toward the wire boundary; no domain depends on a domain above it in the flow, and the node tier never depends on the browser domain.

```mermaid
---
config:
  layout: elk
  look: neo
  theme: base
---
flowchart LR
    accTitle: TypeScript four-domain dependency direction
    accDescr: The descriptor pipeline feeds interchange; interchange feeds projection; interchange and projection feed web on the browser side and services on the node side; the bundle fence is the folder-scoped no-restricted-imports stratum set and the projection connect-web ban inside the one package.
    Desc["interchange/gen/descriptors + buf.gen.yaml"] --> Gen["interchange/src/gen"]
    Gen --> Inter["interchange folder (neutral)"]
    Inter --> Proj["projection folder (neutral)"]
    Inter --> Web["web folder (./web entry)"]
    Proj --> Web
    Inter --> Svc["services folder (./node entry)"]
    Proj --> Svc
```

Text equivalent: the committed descriptor set drives `buf generate` into `@rasm/interchange` `src/gen`; `@rasm/interchange` derives transport, clients, decode/encode rails, refinement, geometry, artifact frames, the exhaustive fault family, quarantine, and the outbound command gateway from those descriptors; `@rasm/projection` folds decoded values into keyed `SubscriptionRef`-backed stores over the `StreamPolicy` reconnect vocabulary, dialing nothing (and importing no `@connectrpc/*`); `@rasm/web` subscribes to the stores through the one `AtomBinding`, renders the leaf routes and the geo surface, emits intents through the `@rasm/interchange` `CommandGateway`, and composes the SPA root binding `RuntimeConfig`/`BrowserPlatform`/`AuthSession`/`SelfTelemetry`/`LocalPersistence`/`DecodeWorkerPool`; `@rasm/services` composes interchange+projection plus its own durable, runner, SQL, internal-RPC, hybrid-search, and provisioning owners. The only cycle is the read-gate-then-fold pair: `@rasm/interchange` `CommandGateway` reads the `@rasm/projection` `AvailabilityStore` fold and writes back the command receipt — an intra-neutral read-gate, not a tier reversal, and the gateway lives in `@rasm/interchange` (the transport-owning domain) so no browser transport leaks into the fold tier.

## [5]-[DESCRIPTOR_PIPELINE_PLACEMENT]

The buf descriptor pipeline is a build-time stage in `@rasm/interchange` landing its input and output as committed tree leaves, so the runtime modules import generated values and never run the generator:

- Input leaf: `interchange/gen/descriptors` holds the app-root-emitted `FileDescriptorSet`, the same descriptor set the C# `ContractGuard` publishes beside the discovery manifest; the buf input row points here, never at a `.proto` tree.
- Config leaf: `interchange/buf.gen.yaml` carries the single-plugin v2 codegen config `transport.md#CODEGEN_TOOLING` fixes — one `protoc-gen-es` plugin, `target=ts`, `include_imports: true`, emitting `.ts` with the transitive descriptor graph embedded.
- Output leaf: `interchange/src/gen` holds the `*_pb.ts` descriptor modules, one `GenService` descriptor per service; `transport.ts` imports them to derive clients through `createClient` and to construct the file-aware registry `FaultDetailRail` passes to `findDetails`.
- Build edge: the generation pass runs as a pnpm build step ahead of the tsgo type-check and the bundle; `@bufbuild/buf` (`allowBuilds: true` in the catalog) drives the pass and never enters a runtime import; a hand-edit of a generated module is the deleted form.

## [6]-[REFINEMENT_HORIZON]

The GLB concern splits across two altitudes: the GLB BYTE/ARTIFACT layer is landed, and only the WebGL MESH-RENDER layer is on the horizon. GLB is a server-streamed content-addressed artifact, so `@rasm/interchange` `ArtifactFrameRail` (codec-rails#CODEC_RAILS) reassembles the `ArtifactFrameWire` frames into a Crc32-verified XxHash128-keyed blob today — the transcribable wire-consumption surface for GLB import and export over the existing `#TS_PROJECTION` fence. What stays deferred is `GlbViewport`, the WebGL render of a mesh, one owner-anchored refinement-horizon entry on web/render-surfaces.md, NOT a page this turn: its mesh-render input is verified present only in C# `remote-lane.md#PROTO_VOCABULARY` and absent from the `#TS_PROJECTION` fence, so `@rasm/interchange` has no transcribable mesh Schema and SIGNATURE_LAW forbids TS authoring one. The render entry routes as a four-end cross-branch precondition-DAG (the seam row lives in `region-map/seam-splits.md`): (a) C# `remote-lane.md#TS_PROJECTION` promotes `MeshTensor`/`GeometryPayload(mesh)` from `[PROTO_VOCABULARY]` into the projection fence — the single true blocker; (b) C# `interchange.md` authoring DISCHARGED; (c) the Python `libs/python/compute` IFC->GLB two-hop companion authors — a precondition the TS branch observes; (d) only then `@rasm/web` admits a WebGL viewer with a `Schema.Literal` renderer-backend axis admitting three/babylon/model-viewer and a webgpu literal for the meshlet/cluster-LOD ambition. ZERO WebGL packages are admitted until the upstream mesh `#TS_PROJECTION` exists.

## [7]-[ATLAS_LAW]

- The tree is the live file plan re-derived from the finalized owner set each loop; a new owner lands as a leaf or a row on an existing leaf in its owning domain, never a new domain unless a genuinely new bounded concern warrants one.
- One owner has exactly one physical leaf; an owner split across two files or two owners fused into one undifferentiated module is the named layout defect. A page whose owner set spans a real interaction-role taxonomy or a two-altitude deploy split is an owner-block sub-folder (`web/component-system/`, `services/provisioning/`), never a single flat module; a single-axis page transcribes to one flat module (`services/durable-execution.ts`).
- The domain partition mirrors the page partition: four domain folders in one package, each with its `.planning/` charter and pages; `gen`/`gen/descriptors` are codegen artifacts in the `interchange` folder and `provisioning/` is the `./provisioning` subpath leaf in the `services` folder.
- The node folder carries no browser import and the browser folder carries no node import; the folder-scoped `browser`/`node`/`neutral` `no-restricted-imports` strata plus the `projection/**` `@connectrpc/*` ban are the enforcement points inside the one project, and a cross-stratum source import or a transport dial in the fold interior is the named coupling defect.
- Every deepening-horizon owner lands on the densest owning domain page and surfaces here as a leaf; `GlbViewport` is the one refinement-horizon owner with no page and no leaf this turn, admitted only when the upstream mesh fence exists.
