# [TYPESCRIPT_BRANCH_ARCHITECTURE]

The branch domain map of the TypeScript web/edge platform — the five packages by stratum and altitude, each in one line, and the dependency direction stated once. The branch consumes the C# wire as settled vocabulary and authors no wire shape; it owns no geometry. Per-package domain maps live in each folder's `ARCHITECTURE.md`; this page draws only the cross-package picture. Cross-folder and cross-language wires live on the folder task cards, never a standalone seam ledger that drifts.

## [1]-[PACKAGE_MAP]

Three strata carry the five packages: the platform-neutral interchange/projection interior every publication composes, the browser ui/platform publication, and the node services publication. Each package is a genuine higher-order domain, not a weak sibling.

```text codemap
libs/typescript/
├── interchange/    # neutral interior, "." export — the byte-to-typed-and-back wire boundary: one protocol-selection transport, the codec rail family, content-addressed artifact-frame reassembly, the exhaustive fault reconstruction, the contract-drift quarantine, the outbound command gateway
├── projection/     # neutral interior, "." export — the read-side fold-algebra owner: one keyedFold combinator over one StreamPolicy into SubscriptionRef-backed keyed maps, the differential-dataflow standing-query engine, the strong-eventual-consistency CRDT fold, the receipt/evidence/availability/clock-uncertainty projection
├── ui/             # browser publication, "./ui" library — the host-free browser UI/UX/components library: the one AtomBinding reactive spine, the headless interaction-role vocabulary, the OKLCH theming engine, and the cartography/viewport/observation leaf surfaces over the decoded wire
├── platform/       # browser publication, "./web" SPA entry — the browser AppHost-analog: the composition root and one runtime, the browser platform bindings, the OIDC/PKCE auth-session, the typed runtime-config boundary, the self-telemetry edge, the build and main-thread-offload pipeline, routing, the PWA offline cache, crash capture, feature flags, and the web-vitals budget
└── services/       # node publication, "./node" + "./provisioning" — the host-free node tier and its deploy-time IaC, deliberately NOT coupled to the AEC/Rhino pipelines: durable execution over the cluster engine, the multi-tenant Postgres store, hybrid search, internal RPC, the addressable-actor modality, and the two-mode provisioning tier
```

## [2]-[DEPENDENCY_DIRECTION]

The graph is acyclic with the wire boundary at the base and the publications at the leaves; dependency flows inward toward `interchange`. No folder imports another's interior — each consumes only the published surface of the package below it.

- `interchange` decodes the wire and imports no sibling; it is the inbound dependency root the whole branch reaches through.
- `projection` folds the decoded `interchange` shapes; it dials no transport and the `@connectrpc/*` import never crosses into the fold interior.
- `ui` and `platform` consume `projection` folds and the `interchange` rails and gateway; `platform` composes `ui` into the SPA root, and `ui` never imports `platform` — the one-way intra-browser direction.
- `services` is the node peer: it composes `interchange` and `projection` over its own durable, persistence, search, RPC, and provisioning owners, and keeps the deploy-time `@pulumi/*` closure off the runtime hot path behind the `./provisioning` subpath.

The only read-back edge is the dial-time gate: the `interchange` `CommandGateway` reads the `projection` `AvailabilityStore` before a command fires. The dialing gateway is resident in the transport-owning folder, so this is a read across the neutral interior, never a transport leak into the fold tier or a stratum reversal. The publication bundle stays clean by construction — the browser bundle never carries `@effect/cluster`/`@effect/sql-pg`/`@pulumi/*`/`@effect/platform-node`, and the node bundle never carries `@effect/platform-browser`/`react`/`maplibre-gl`/`@deck.gl/*`/`arctic`/`workbox-*` — enforced by the centralized monorepo config, never a runtime guard.

## [3]-[FAULT_OWNERSHIP]

One fault concept, two altitudes, never merged into a single branch-wide fault family. The wire-reconstruction owner and the node-side typed rails are distinct in kind: the wire owner reconstructs what the C# packages emit across the boundary; a node-tier rail is a local failure the node process raises and folds, carried on its own `Effect` error channel.

- `interchange` owns the one wire-fault reconstruction: `FaultDetail`, the `Data.TaggedEnum` family rebuilding every .NET fault the `grpc-status-details-bin` trailer carries, with the `fromConnect` infallible boundary fold and the `Match.tagsExhaustive` render table. `FaultDetail` is WIRE-ONLY — it reconstructs the C#-minted fault, authors no node-side failure, and never leaks past the decode boundary as a general node error type.
- Each node-tier surface owns its own page-local typed rail for node-side failures — the `services` `DurableFault` (durable-execution engine), the artifact-transfer, rerank, and drift faults — declared as a `Data.TaggedEnum` or `Data.TaggedError` at the page that raises it, never reaching for `interchange` `FaultDetail` to carry a failure that never crossed the wire. A node-tier surface importing `FaultDetail` to model a local failure is the named branch defect: the wire owner reconstructs the boundary, the node rail owns the interior.
