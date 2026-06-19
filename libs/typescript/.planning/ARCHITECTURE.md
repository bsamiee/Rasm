# [TYPESCRIPT_BRANCH_ARCHITECTURE]

The branch domain map of `libs/typescript` — the host-free web/edge/backend platform. A browser publication, a node durable interior, and a public edge ingress, meeting the C# wire as settled vocabulary and owning no geometry.

Each node is a package folder; the language's `.planning/` scaffold is authoring substrate, never part of the map.

## [1]-[PACKAGE_MAP]

```text codemap
libs/typescript/
├── interchange/  # the byte-to-typed-and-back wire boundary and inbound dependency root
├── projection/   # the read-side fold-algebra owner over keyedFold and StreamPolicy
├── ui/           # the host-free browser UI/UX/components library over the decoded wire
├── platform/     # the browser AppHost-analog: composition root, runtime, and host owners
├── services/     # the host-free node durable interior and its deploy-time IaC
├── edge/         # the public HTTP/edge ingress tier (NEW-FOLDER CANDIDATE)
└── testing/      # the shared property-testing spine and wire fixtures (NEW-FOLDER CANDIDATE)
```

`interchange`/`projection` form the platform-neutral interior every publication composes; `ui`/`platform` are the browser publication; `services` is the node durable interior; `edge` is the public ingress leaf. `edge/` and `testing/` are NEW-FOLDER CANDIDATES the branch adopts on the `libs/typescript/*` glob.

## [2]-[SEAMS]

```text seams
interchange  ←  csharp:Rasm.AppHost      # ReceiptEnvelope/HLC/Tenant + capability SDK (wire)
interchange  ←  csharp:Rasm.Compute      # proto suite wire + FaultDetail (wire)
interchange  ←  csharp:Rasm.Persistence  # OpLog/Snapshot CRDT wire (wire)
interchange  ⇄  csharp:Rasm              # XxHash128 content-key parity (content-key)
projection   ←  csharp:Rasm.AppUi        # evidence / availability / command wire (wire)
ui           ←  csharp:Rasm.Bim          # BCF topic / viewpoint wire (wire)
```

## [3]-[DEPENDENCY_DIRECTION]

The graph is acyclic with the wire boundary at the base and the publications at the leaves; dependency flows inward toward `interchange`. No folder imports another's interior — each consumes only the published surface of the package below it.

- `interchange` decodes the wire and imports no sibling; it is the inbound dependency root the whole branch reaches through.
- `projection` folds the decoded `interchange` shapes; it dials no transport and the `@connectrpc/*` import never crosses into the fold interior.
- `ui` and `platform` consume `projection` folds and the `interchange` rails and gateway; `platform` composes `ui` into the SPA root, and `ui` never imports `platform` — the one-way intra-browser direction.
- `services` is the node durable interior: it composes `interchange` and `projection` over its own execution (durable engine, backplane, outbox, SLO), persistence (store and object blob), search, messaging RPC, security, agent, and provisioning owners, and keeps the deploy-time `@pulumi/*` closure off the runtime hot path behind the `./provisioning` subpath and the AI/agent closure behind `./agent`.
- `edge` is the public ingress leaf above `services`: it imports `services` (the durable workflows and `messaging/rpc` proxy are the handler implementation) plus `interchange`/`projection` for decode and the availability gate, and exposes the one public `HttpApi` over `NodeHttpServer.layer` or the `toWebHandler` fetch-handler runtime. The node durable interior never exposes itself; the public front door is `edge` alone, and a second public-API mint is the named branch defect.
- `testing` is the neutral test-infrastructure lib the five publication folders consume; it imports `interchange` for the `*Wire` shapes its fixtures pin and dials no transport.

The only read-back edge is the dial-time gate: the `interchange` `CommandGateway` reads the `projection` `AvailabilityStore` before a command fires. The dialing gateway is resident in the transport-owning folder, so this is a read across the neutral interior, never a transport leak into the fold tier or a stratum reversal. The publication bundle stays clean by construction — the browser bundle never carries `@effect/cluster`/`@effect/sql-pg`/`@pulumi/*`/`@effect/platform-node`, the node bundle never carries `@effect/platform-browser`/`react`/`maplibre-gl`/`@deck.gl/*`/`arctic`/`workbox-*`, and the `edge` ingress bundle carries `@effect/platform`/`@effect/platform-node` HttpApi but never the browser surface — enforced by the centralized monorepo config, never a runtime guard.

## [4]-[FAULT_OWNERSHIP]

One fault concept, three altitudes, never merged into a single branch-wide fault family. The wire-reconstruction owner, the node-side typed rails, and the public problem-detail projection are distinct in kind: the wire owner reconstructs what the C# packages emit across the boundary; a node-tier rail is a local failure the node process raises and folds; the edge owner projects either of those outward to the one public error shape a third-party client decodes.

- `interchange` owns the one wire-fault reconstruction: `FaultDetail`, the `Data.TaggedEnum` family rebuilding every .NET fault the `grpc-status-details-bin` trailer carries, with the `fromConnect` infallible boundary fold and the `Match.tagsExhaustive` render table. `FaultDetail` is WIRE-ONLY — it reconstructs the C#-minted fault, authors no node-side failure, and never leaks past the decode boundary as a general node error type.
- Each node-tier surface owns its own page-local typed rail for node-side failures — the `services` `DurableFault` (`execution/engine`), the artifact-transfer, rerank, drift, auth, SLO, secret, and object faults — declared as a `Data.TaggedEnum` or `Data.TaggedError` at the page that raises it, never reaching for `interchange` `FaultDetail` to carry a failure that never crossed the wire. A node-tier surface importing `FaultDetail` to model a local failure is the named branch defect: the wire owner reconstructs the boundary, the node rail owns the interior.
- `edge` owns the one public problem-detail projection: the `HttpApiError`/RFC 9457 problem-detail fold mapping every node-tier rail and every reconstructed `interchange` `FaultDetail` onto the public error schema each `HttpApiEndpoint` declares, with the status/title/detail rows carried as a vocabulary the middleware reads. The projection is OUTBOUND-ONLY — it shapes the public boundary response and authors no interior failure, and a node-tier rail leaking its raw `_tag` to the public client unprojected is the named defect: the node rail owns the interior, the edge owner shapes the public boundary.
