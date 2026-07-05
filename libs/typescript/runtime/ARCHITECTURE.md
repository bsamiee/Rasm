# [TS_RUNTIME_ARCHITECTURE]

The domain map of `runtime` — the wave-3 execution package spanning both process planes. The sub-domains `proc`, `net`, `otel`, `serve`, `work`, `ai`, and `browser` meet through one runtime-row table, one budget ledger, one fault law, and one front-door assembly law; the browser sub-domain is the same package's browser condition, never a sibling package.

Each codemap node is the eventual source file its `.planning/` design page becomes, named in the language's own folder and file casing — PascalCase `.cs`, lowercase `.py`, camelCase `.ts`. Treat every node as realized code; the `.planning/` scaffold is the authoring substrate, never part of the map.

## [01]-[DOMAIN_MAP]

```text codemap
runtime/
└── src/
    ├── proc/                  # The process substrate: runtime rows, config, flags, lifecycle, off-thread compute
    │   ├── exec.ts            # RUNTIME_ROWS — the keyed node|bun binding table; child processes as declarative values
    │   ├── config.ts          # The ordered provider chain + the boot-validated Setting contract resolved exactly once
    │   ├── flag.ts            # The OpenFeature server Provider: recursive rule family, content-key bucketing, FlagVerdict consumption
    │   ├── life.ts            # Life — ranked lifecycle/health rows on severed fibers folded into one graded receipt
    │   └── worker.ts          # The off-thread protocol: Schema.TaggedRequest union, zero-copy crossings, one pool
    ├── net/                   # Outbound transport and the fanout/replay port
    │   ├── client.ts          # The outbound HTTP lane table — status admission, retry pulses compiled from core Budget rows
    │   ├── channel.ts         # Framed long-lived byte channels: socket duplex under a closed frame vocabulary + SSE feeds
    │   ├── pubsub.ts          # Fanout — engine-blind broadcast/replay/blob port; local, cross-tab, and NATS JetStream rows over one Broker
    │   └── coordinate.ts      # Accord — engine-blind lease/elect/CAS port; NATS KV revision row + browser Web Locks row
    ├── otel/                  # The OTLP wire: export/ingest, crash capture, browser RUM
    │   ├── emit.ts            # Export.live(policy) — the one OTLP egress Layer + collector ingress, with the Redaction scrub
    │   ├── crash.ts           # The total Cause→fatal-emission fold through Convention rows and the core fault enrichers
    │   └── vital.ts           # Six RUM vital rows over one scoped PerformanceObserver bridge
    ├── serve/                 # The one public front door
    │   ├── api.ts             # The assembly law: domains export HttpApiGroup/RpcGroup data; the APP assembles exactly one HttpApi
    │   ├── route.ts           # HttpLayerRouter serving: addHttpApi, Mount port, tus dispatchers, Intake verify, ASSET_ROWS
    │   ├── live.ts            # Realtime serving: SSE/WS over branch feeds under the resume-token and admission laws
    │   ├── problem.ts         # Problem — the RFC 9457 owner rendering itself via HttpServerRespondable; Problem.net seam
    │   └── cli.ts             # Command-value verb families folded by the APP into one root via withSubcommands
    ├── work/                  # Durable work: actors, workflows, queues, schedules, delivery, documents
    │   ├── entity.ts          # The durable-actor plane: WorkClass service-class table, mailbox tiers, Snowflake, ClusterError bridge
    │   ├── flow.ts            # Workflow suspend-and-replay: Step mint, two-tier deadlines, Signal.pause durable timer
    │   ├── queue.ts           # DurableQueue families, DurableRateLimiter throttles, the pg lane policy + LaneVerdict DLQ fold
    │   ├── schedule.ts        # Cadence rows minted into ClusterCron with misfire/catch-up postures
    │   ├── deliver.ts         # ONE channel table for mail/webhook egress: one receipt, one fault family, one suppression fold
    │   └── report.ts          # Report.Spec folded through three engine arms (xlsx/pdf/csv) over the same decoded rows
    ├── ai/                    # The intelligence spine
    │   ├── model.ts           # Five provider families on one capability-asymmetry table; fallback via Effect.withExecutionPlan
    │   ├── embed.ts           # Deterministic chunking, embedding rows, the data Embedder/Reranker port satisfaction
    │   ├── tool.ts            # Schema-typed tools, Toolkit assembly, both MCP lanes, the one Safety owner
    │   └── agent.ts           # The agent altitude: Transition-machine sessions, Chat.layerPersisted durability
    └── browser/               # The browser runtime condition
        ├── boot.ts            # BrowserRuntime.runMain single-boot law, AppSpec budget, Connect cells, Capability roster
        ├── shell.ts           # The PWA shell: manifest as a typed value, Workbox scoped resource, the update handshake
        ├── persist.ts         # The _domains IndexedDB vocabulary over idb-keyval with batch read/write modalities
        ├── route.ts           # The Navigation-API typed router, the Vault session plane, the navigation admission fold
        └── fetch.ts           # The browser byte transport: XHR/WebSocket/BrowserWorker binding rows, Depot verified arrivals
```

## [02]-[SEAMS]

```text seams
proc/flag      ←  typescript:core/interchange  # [SHAPE]: FlagVerdict OpenFeature-contract landing
net/client     ←  typescript:core/value        # [SHAPE]: Budget ledger rows compiled into lane pulses
otel/emit      ←  typescript:core/observe      # [SHAPE]: Convention rows stamped at every emission
otel/emit      ←  csharp:Rasm.AppHost          # [TRANSPORT]: OTLP export alignment at the shared collector
serve/route    ←  typescript:security/crypt    # [BOUNDARY]: Intake held-octets verify seam
serve/route    ←  typescript:security/authn    # [PORT]: BearerGuard/ApiKeyGuard HttpApiMiddleware Tags mounted on api routes
serve/route    ←  typescript:data/object       # [BOUNDARY]: tus dispatcher mount rows
serve/route    →  typescript:ui/viewer         # [BOUNDARY]: self-hosted draco/basis/meshopt transcoder assets served byte-identical
serve/live     ←  typescript:data/read         # [SHAPE]: reactivity-keyed feeds under the resume-token law
work/queue     ⇄  typescript:data/journal      # [BOUNDARY]: outbox claim-lease/urgency/park statements
work/entity    ←  typescript:iac/program       # [PORT]: StackOutputs.sharding → ShardingConfig.layerFromEnv
ai/embed       →  typescript:data/read         # [PORT]: Embedder fingerprint Layer + the gated Reranker fold
browser/route  ⇄  typescript:security/authn    # [SHAPE]: Vault session residency + CookieSpec.csrf double-submit read
browser/route  ⇄  typescript:security/authn    # [BOUNDARY]: OAuth redirect-ceremony continuity (depart/land)
browser/fetch  ⇄  typescript:core/value        # [CONTENT_KEY]: Digest.mint("content") off-thread reassembly verify — a delegating mint site
browser/fetch  →  typescript:ui/viewer         # [PORT]: Depot.haul verified arrivals + residency ledger into GlbViewport
browser/*      ⇄  typescript:ui/system         # [PORT]: Router/Install/Guard/Vault Subscribable planes over Atom.subscribable rows
browser/boot   →  typescript:ui/system         # [PORT]: Capability roster satisfies the ui-declared Clipboard Tag
browser/boot   →  typescript:ui/viewer         # [PORT]: Capability roster satisfies ui-declared Position/Grant Tags from geolocation/permissions
proc/life      →  typescript:iac/kube          # [SHAPE]: Setting.life.drain + probe routes mirrored as the workload _LIFE anchor
net/pubsub     →  typescript:iac/kube          # [BOUNDARY]: Setting.fanout.origin dial against the JetStream server posture (fsync, quorum)
```

## [03]-[ORGANIZATION]

`proc` is the substrate every plane boots on: a runtime is a row, config resolves once, flags evaluate as data, lifecycle folds evidence, workers speak one protocol. `net` owns egress geometry — every outbound call inherits a lane's compiled pulse and circuit row, every long-lived channel one frame vocabulary, every broadcast the engine-blind fanout port, every cross-process agreement the coordination port over the same wire. `otel` is the wire half of observability; the vocabulary lives in core. `serve` enforces the one front-door law: libraries export route/verb/group DATA, the app assembles exactly one HttpApi, one CLI root, one serve fold; faults leave only as self-rendering Problems. `work` prices every durable surface against one WorkClass table so entities, queues, cron, and relay pacing share a single service-class economy. `ai` folds five providers onto one capability table and satisfies the data wave's retrieval ports. `browser` is the same package under the browser condition: one boot, one shell, one persistence vocabulary, one typed router carrying the session plane, one byte transport delegating identity to the core mint.

## [04]-[BOUNDARIES]

- The app root, never this folder, assembles the HttpApi, satisfies port Tags, selects runtime rows, and binds the browser composition root (GlbViewport from Depot arrivals, host planes into ui atoms).
- The record of truth is the data wave's; work surfaces compose its outbox and mailbox statements, never a second store. NATS is fanout and replay, never the system of record.
- The folder mints no content identity; the browser decode worker delegates to the core Digest engine as one of its three sanctioned sites.
- Frozen upstream packages `@effect/rpc-http`, `@effect/cluster-node`, `@effect/cluster-browser`, and `@effect/cluster-workflow` are never admitted; cluster runs leaderless over RunnerStorage advisory locks.
