# [TS_RUNTIME_ARCHITECTURE]

`runtime` owns the branch's execution substrate across both process planes: `proc`, `net`, `otel`, `serve`, `work`, and `ai` meet through one runtime-row table, one budget ledger, one fault law, and one front-door assembly law, and `browser` is the same package under the browser condition, never a sibling. Owners align with the core, security, and data peers by seam contract, never a cross-package reference.

## [01]-[DOMAIN_MAP]

```text codemap
runtime/
└── src/
    ├── proc/                  # Process substrate: runtime rows, config, flags, lifecycle, off-thread compute
    │   ├── exec.ts            # Keyed node|bun runtime-row binding table; child processes as declarative values
    │   ├── config.ts          # Ordered provider chain and the boot-validated Setting contract resolved once
    │   ├── flag.ts            # OpenFeature server Provider: a recursive rule family over content-key bucketing
    │   ├── life.ts            # Ranked lifecycle and health rows on severed fibers folded into one graded receipt
    │   └── worker.ts          # Off-thread worker protocol: zero-copy crossings over one pool
    ├── net/                   # Outbound transport and the fanout/replay port
    │   ├── client.ts          # Outbound HTTP lane table: status admission and retry pulses off the core budget
    │   ├── channel.ts         # Framed long-lived byte channels: socket duplex and SSE feeds over one frame vocabulary
    │   ├── pubsub.ts          # Fanout — the engine-blind broadcast, replay, and blob port over one Broker
    │   └── coordinate.ts      # Accord — the engine-blind lease, elect, and CAS coordination port
    ├── otel/                  # OTLP wire: export/ingest, crash capture, browser RUM
    │   ├── emit.ts            # One OTLP egress Layer and collector ingress under the redaction scrub
    │   ├── crash.ts           # Total Cause-to-fatal-emission fold through the core forensic fault band
    │   └── vital.ts           # Six RUM vital rows over one scoped PerformanceObserver bridge
    ├── serve/                 # One public front door
    │   ├── api.ts             # Assembly law: sub-domains export group data, the app assembles one HttpApi
    │   ├── route.ts           # HttpLayerRouter serving fold: api mount, upload dispatch, and intake verify
    │   ├── live.ts            # Realtime SSE/WS serving over branch feeds under the resume-token and admission laws
    │   ├── problem.ts         # Problem — the RFC 9457 owner rendering itself as a self-describing response
    │   └── cli.ts             # Command-value verb families the app folds into one root
    ├── work/                  # Durable work: actors, workflows, queues, schedules, delivery, documents
    │   ├── entity.ts          # Durable-actor plane: the WorkClass service-class table over tiered mailboxes
    │   ├── flow.ts            # Workflow suspend-and-replay: minted steps, two-tier deadlines, one durable pause timer
    │   ├── queue.ts           # DurableQueue families and rate-limiter throttles over the pg lane policy and DLQ fold
    │   ├── schedule.ts        # Cadence rows minted into cluster cron with misfire and catch-up postures
    │   ├── deliver.ts         # One channel table for mail and webhook egress: one receipt, one fault, one suppression
    │   └── report.ts          # Report specs folded through three engine arms over the same decoded rows
    ├── ai/                    # Intelligence spine
    │   ├── model.ts           # Five provider families on one capability-asymmetry table with ranked fallback
    │   ├── embed.ts           # Deterministic chunking and embedding rows satisfying the data retrieval ports
    │   ├── tool.ts            # Schema-typed tools and toolkit assembly across both MCP lanes under one safety owner
    │   └── agent.ts           # Agent altitude: transition-machine sessions with persisted-chat durability
    └── browser/               # Browser runtime condition
        ├── boot.ts            # Single-boot law: the app-spec budget, connect cells, and the capability roster
        ├── shell.ts           # PWA shell: the manifest as a typed value under a scoped resource and update handshake
        ├── persist.ts         # IndexedDB domain vocabulary with batch read and write modalities
        ├── route.ts           # Navigation-API typed router carrying the Vault session plane and admission fold
        └── fetch.ts           # Browser byte transport: XHR, WebSocket, and worker binding rows for verified arrivals
```

## [02]-[SEAMS]

```mermaid
---
config:
  theme: base
  look: classic
  layout: elk
  flowchart:
    curve: linear
    padding: 25
  themeVariables:
    darkMode: true
    fontFamily: "SF Mono, Menlo, Cascadia Mono, Segoe UI Mono, Consolas, monospace"
    useGradient: false
    dropShadow: "none"
    background: "#282A36"
    primaryColor: "#44475A"
    primaryTextColor: "#F8F8F2"
    primaryBorderColor: "#BD93F9"
    lineColor: "#FF79C6"
    textColor: "#F8F8F2"
    clusterBkg: "#21222C"
    clusterBorder: "#D6BCFA"
    edgeLabelBackground: "#21222C"
    labelBackgroundColor: "#21222C"
    titleColor: "#D6BCFA"
  themeCSS: ".nodeLabel{font-size:13px;font-weight:500}.edgeLabel{font-size:12px;font-weight:500}.cluster-label .nodeLabel{font-size:13.5px;font-weight:700;letter-spacing:.08em}.edge-thickness-normal{stroke-width:2px}.edge-thickness-thick{stroke-width:3px}.edge-pattern-dashed,.edge-pattern-dotted{stroke-width:1.5px;stroke-dasharray:4 6}.node rect,.node circle,.node polygon,.node path,.node .outer-path{stroke-width:1.5px;filter:none!important}.cluster rect{stroke-width:1px!important;stroke-dasharray:5 4!important;filter:none!important}.marker path{transform:scale(.8);transform-origin:5px 5px}.marker circle{transform:scale(.48);transform-origin:5px 5px}.edgeLabel rect{transform-box:fill-box;transform-origin:center;transform:scale(1.1,1.2)}"
---
flowchart LR
    accTitle: Runtime domain-peer seam registry
    accDescr: Runtime sub-domain owners exchanging flag, budget, convention, identity, custody, and durable-store shapes with the core, security, and data TypeScript domain peers, edge rails colored by kind and nodes classed by seam direction.
    subgraph runtime[RUNTIME]
        Proc[Proc substrate]
        Net[Net egress]
        Otel[Otel wire]
        Browser[Browser runtime]
        Serve[Serve front door]
        Work[Work plane]
        Ai[AI spine]
    end
    Core{{core}}
    Security{{security}}
    Data[(data)]
    Core e1@-->|"[SHAPE]: FlagVerdict"| Proc
    Core e2@-->|"[SHAPE]: Budget.schedule"| Net
    Core e3@-->|"[SHAPE]: Convention"| Otel
    Browser e4@<-->|"[CONTENT_KEY]: Digest.mint"| Core
    Browser e5@<-->|"[SHAPE]: CookieSpec"| Security
    Browser e6@<-->|"[BOUNDARY]: OAuth redirect"| Security
    Security e7@-->|"[PORT]: BearerGuard"| Serve
    Security e8@-->|"[BOUNDARY]: Intake verify"| Serve
    Data e9@-->|"[BOUNDARY]: Tus dispatcher"| Serve
    Data e10@-->|"[SHAPE]: Reactivity feed"| Serve
    Work e11@<-->|"[BOUNDARY]: Outbox claim"| Data
    Ai e12@-->|"[PORT]: Embedder"| Data
    classDef primary fill:#44475A,stroke:#FF79C6,color:#F8F8F2
    classDef external fill:#8BE9FDBF,stroke:#8BE9FD,color:#282A36
    classDef data fill:#FFB86CBF,stroke:#FFB86C,color:#282A36
    classDef edgeData stroke:#FFB86C,color:#F8F8F2
    classDef edgeControl stroke:#FF79C6,color:#F8F8F2
    class Proc,Net,Otel,Browser,Serve,Work,Ai primary
    class Core,Security external
    class Data data
    class e4 edgeData
    class e1,e2,e3,e5,e6,e7,e8,e9,e10,e11,e12 edgeControl
```

```mermaid
---
config:
  theme: base
  look: classic
  layout: elk
  flowchart:
    curve: linear
    padding: 25
  themeVariables:
    darkMode: true
    fontFamily: "SF Mono, Menlo, Cascadia Mono, Segoe UI Mono, Consolas, monospace"
    useGradient: false
    dropShadow: "none"
    background: "#282A36"
    primaryColor: "#44475A"
    primaryTextColor: "#F8F8F2"
    primaryBorderColor: "#BD93F9"
    lineColor: "#FF79C6"
    textColor: "#F8F8F2"
    clusterBkg: "#21222C"
    clusterBorder: "#D6BCFA"
    edgeLabelBackground: "#21222C"
    labelBackgroundColor: "#21222C"
    titleColor: "#D6BCFA"
  themeCSS: ".nodeLabel{font-size:13px;font-weight:500}.edgeLabel{font-size:12px;font-weight:500}.cluster-label .nodeLabel{font-size:13.5px;font-weight:700;letter-spacing:.08em}.edge-thickness-normal{stroke-width:2px}.edge-thickness-thick{stroke-width:3px}.edge-pattern-dashed,.edge-pattern-dotted{stroke-width:1.5px;stroke-dasharray:4 6}.node rect,.node circle,.node polygon,.node path,.node .outer-path{stroke-width:1.5px;filter:none!important}.cluster rect{stroke-width:1px!important;stroke-dasharray:5 4!important;filter:none!important}.marker path{transform:scale(.8);transform-origin:5px 5px}.marker circle{transform:scale(.48);transform-origin:5px 5px}.edgeLabel rect{transform-box:fill-box;transform-origin:center;transform:scale(1.1,1.2)}"
---
flowchart LR
    accTitle: Runtime platform and cross-runtime seam registry
    accDescr: Runtime sub-domain owners exchanging settings, stack outputs, transcoder assets, subscribable planes, and OTLP telemetry with the iac and ui TypeScript peers and the Rasm.AppHost cross-runtime host, edge rails colored by kind and nodes classed by seam direction.
    subgraph runtime[RUNTIME]
        Otel[Otel wire]
        Proc[Proc substrate]
        Net[Net egress]
        Serve[Serve front door]
        Browser[Browser runtime]
    end
    AppHost([Rasm.AppHost])
    Iac{{iac}}
    Ui([ui])
    AppHost e1@-->|"[TRANSPORT]: OtelExport"| Otel
    Proc e2@-->|"[SHAPE]: Setting.life"| Iac
    Net e3@-->|"[BOUNDARY]: JetStream posture"| Iac
    Iac e4@-->|"[PORT]: StackOutputs"| Proc
    Serve e5@-->|"[BOUNDARY]: transcoder assets"| Ui
    Browser e6@-->|"[PORT]: Subscribable planes"| Ui
    classDef primary fill:#44475A,stroke:#FF79C6,color:#F8F8F2
    classDef external fill:#8BE9FDBF,stroke:#8BE9FD,color:#282A36
    classDef annotation fill:#21222C,stroke:#6272A4,color:#F8F8F2
    classDef edgeExternal stroke:#8BE9FD,color:#F8F8F2
    classDef edgeControl stroke:#FF79C6,color:#F8F8F2
    class Otel,Proc,Net,Serve,Browser primary
    class Iac external
    class AppHost,Ui annotation
    class e1 edgeExternal
    class e2,e3,e4,e5,e6 edgeControl
```

## [03]-[ORGANIZATION]

`proc` is the substrate every plane boots on: a runtime is a row, config resolves once, flags evaluate as data, lifecycle folds evidence, workers speak one protocol. `net` owns egress geometry — every outbound call inherits a lane's compiled pulse and circuit row, every long-lived channel one frame vocabulary, every broadcast the engine-blind fanout port, every agreement the coordination port over the same wire. `otel` owns the wire half of observability; its vocabulary lives in core. `serve` enforces the one front-door law: libraries export route, verb, and group data, the app assembles exactly one `HttpApi`, one CLI root, and one serve fold, and faults leave only as self-rendering `Problem`s. `work` prices every durable surface against one `WorkClass` table, so entities, queues, cron, and relay pacing share a single service-class economy. `ai` folds five providers onto one capability table and satisfies the data wave's retrieval ports. `browser` is the same package under the browser condition: one boot, one shell, one persistence vocabulary, one typed router carrying the session plane, one byte transport delegating identity to the core mint.

## [04]-[BOUNDARIES]

- App root, never this folder, assembles the `HttpApi`, satisfies port `Tag`s, selects runtime rows, and binds the browser composition root.
- Data wave owns the record of truth; work surfaces compose its outbox and mailbox statements, never a second store, and NATS carries fanout and replay, never the system of record.
- Content identity is never minted here; the browser decode worker delegates to the core `Digest` engine.
- Cluster runs leaderless over `RunnerStorage` advisory locks, so the node-bound `@effect/cluster-node`, `@effect/cluster-browser`, `@effect/cluster-workflow`, and `@effect/rpc-http` upstreams are never admitted.
