# [TS_RUNTIME_ARCHITECTURE]

`runtime` owns the branch's execution substrate across both process planes: `proc`, `net`, `otel`, `serve`, `work`, and `ai` meet through one runtime-row table, one budget ledger, one fault law, and one front-door assembly law, and `browser` is the same package under the browser condition, never a sibling. Owners align with the core, security, and data peers, the interface and deploy planes, and peer branches by seam contract, never a cross-package reference.

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
    │   ├── channel.ts         # Framed long-lived byte channels: socket duplex, SSE, and MQTT v5 over one frame vocabulary
    │   ├── pubsub.ts          # Fanout — the engine-blind broadcast, replay, and blob port over one Broker
    │   └── coordinate.ts      # Accord — the engine-blind lease, elect, and CAS coordination port
    ├── otel/                  # OTLP wire: egress, W3C continuation, crash capture, browser RUM
    │   ├── emit.ts            # One OTLP egress Layer and the W3C continuation ingress under the redaction scrub
    │   ├── instrument.ts      # Browser-condition registration node: request, document, and interaction rows over the zone manager
    │   ├── server.ts          # Server-condition registration node: engine vitals and http/undici/pg rows over the async-local manager
    │   ├── dev.ts             # plane:dev DevTools registration node on the ./dev subpath; the gauge fails any runtime import
    │   ├── crash.ts           # Total Cause-to-fatal-emission fold through the core forensic fault band
    │   ├── meter.ts           # Work-plane fact-to-instrument bridge, census gauges, log floor, tenant views
    │   ├── profile.ts         # Continuous-profiling lane: Pyroscope push lifecycle under the identity projection
    │   └── vital.ts           # The estate's one CWV owner: web-vitals capture, graded facts, the render-report intake
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
    │   ├── model.ts           # Provider families on one capability-asymmetry table with ranked fallback
    │   ├── embed.ts           # Deterministic chunking and embedding rows satisfying the data retrieval ports
    │   ├── tool.ts            # Schema-typed tools and toolkit assembly across both MCP lanes under one safety owner
    │   └── agent.ts           # Agent altitude: transition-machine sessions with persisted-chat durability
    └── browser/               # Browser runtime condition
        ├── boot.ts            # Single-boot law: the app-spec budget, connect cells, and the capability roster
        ├── shell.ts           # PWA shell: the manifest as a typed value under a scoped resource and update handshake
        ├── persist.ts         # IndexedDB domain vocabulary, storage-residency verdicts, the file-egress routes
        ├── route.ts           # Navigation-API typed router carrying the Vault session plane and admission fold
        └── fetch.ts           # Browser byte transport: XHR, WebSocket, and worker binding rows for verified arrivals
```

## [02]-[STRATA]

- S0 `net` egress floor — `client` lanes and `channel` frames mint outbound transport (`Client`, `Feed`) and import no runtime sibling.
- S1 `proc` — `config` resolves `Setting` once over `Client`, `flag` rides `Feed` channels, `exec`/`life`/`worker` mint their rails floor-free.
- S1 `worker.main.ts` hands `Report.worker` in as composition-root code, never a stratum import.
- S2 carriers + work — `pubsub`/`coordinate`, `otel`, `browser`, and `work` seat at one rank; every lateral edge stays inside the stratum.
- S2 `browser` stands parallel to the server plane, importing none of serve, work, or ai.
- S2 `work` prices the durable plane over `Setting`, `Client`, and the `Bench` protocol, marking settlement facts through the otel meter bridge.
- S3 `serve` — the front door composing `Fanout`, `Propagation`, and `Life`; nothing imports serve.
- `ai` composes no runtime sibling — its edges run outward to core, data, and security alone, standing beside the strata rather than inside them.

```mermaid
---
config:
  layout: elk
  flowchart:
    curve: linear
    padding: 25
---
flowchart TB
    accTitle: Runtime interior import strata
    accDescr: Four strata — serve over the carriers, browser and work at the same rank, over proc onto the net floor; lateral edges stay inside S2.
    subgraph S3["S3 SERVE"]
        Serve["api · route · live · problem · cli"]
    end
    subgraph S2["S2 CARRIERS + WORK"]
        Fanout["pubsub · coordinate"]
        Otel["emit · crash · dev · instrument · meter · profile · server · vital"]
        Browser["boot · shell · persist · route · fetch"]
        Work["entity · flow · queue · schedule · deliver · report"]
    end
    subgraph S1["S1 PROC"]
        Proc["config · flag · exec · life · worker"]
    end
    subgraph S0["S0 NET FLOOR"]
        NetFloor["client · channel"]
    end
    Proc e1@-->|"[IMPORT]: Client"| NetFloor
    Fanout e2@-->|"[IMPORT]: Setting"| Proc
    Otel e3@-->|"[IMPORT]: Life"| Proc
    Otel e11@-->|"[IMPORT]: Setting"| Proc
    Browser e4@-->|"[IMPORT]: Client"| NetFloor
    Work e5@-->|"[IMPORT]: Setting"| Proc
    Work e6@-->|"[IMPORT]: Client"| NetFloor
    Work e10@-->|"[IMPORT]: Bench"| Proc
    Work e12@-->|"[IMPORT]: Pulse"| Otel
    Browser e13@-->|"[IMPORT]: Vital"| Otel
    Fanout e14@-->|"[IMPORT]: Propagation"| Otel
    Serve e7@-->|"[IMPORT]: Fanout"| Fanout
    Serve e8@-->|"[IMPORT]: Propagation"| Otel
    Serve e9@-->|"[IMPORT]: Life"| Proc
    Serve ~~~ Fanout
    S0 f1@-->|"forbidden: upward import"| S3
```

## [03]-[SEAMS]

```mermaid
---
config:
  layout: elk
  flowchart:
    curve: linear
    padding: 25
---
flowchart LR
    accTitle: Runtime domain-peer seam registry
    accDescr: Runtime owners exchanging flag, budget, convention, identity, custody, and tap shapes with the core, security, and data peers.
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
    Core e2@-->|"[SHAPE]: Fault.Budget"| Net
    Core e3@-->|"[SHAPE]: Convention"| Otel
    Core e4@-->|"[CONTENT_KEY]: Digest.Key&lt;&quot;content&quot;&gt;"| Browser
    Core e14@-->|"[SHAPE]: Identity.App"| Browser
    Security e5@-->|"[SHAPE]: CookieSpec"| Browser
    Browser e6@<-->|"[BOUNDARY]: OAuth"| Security
    Security e7@-->|"[PORT]: BearerGuard"| Serve
    Security e8@-->|"[BOUNDARY]: Intake"| Serve
    Security e13@-->|"[PORT]: FlagGate"| Proc
    Data e9@-->|"[BOUNDARY]: Rail"| Serve
    Data e10@-->|"[SHAPE]: Live.changes"| Serve
    Work e11@<-->|"[BOUNDARY]: Journal.claimBatch"| Data
    Ai e12@-->|"[PORT]: Embedder"| Data
    Data e15@-->|"[PORT]: Journal.census"| Otel
    Security e16@-->|"[SHAPE]: TenantScope.metered"| Serve
    Core e17@-->|"[SHAPE]: Tap.Registry"| Otel
    Data e18@-->|"[SHAPE]: Tap.Registry"| Otel
    Data e20@-->|"[SHAPE]: Journal.envelope"| Work
    Core e21@-->|"[SHAPE]: Carrier.Context"| Otel
    Data e22@-->|"[SHAPE]: Backend.Generation"| Net
    Core e23@-->|"[SHAPE]: Convention"| Proc
```

```mermaid
---
config:
  layout: elk
  flowchart:
    curve: linear
    padding: 25
---
flowchart LR
    accTitle: Runtime platform and cross-runtime seam registry
    accDescr: Runtime owners exchanging settings, stack outputs, subscribable planes, and OTLP telemetry with iac, ui, and the Rasm.AppHost host.
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
    Net e3@-->|"[BOUNDARY]: Fanout.jetstream"| Iac
    Iac e4@-->|"[PORT]: StackOutputs"| Proc
    Serve e5@-->|"[BOUNDARY]: EXT_meshopt_compression"| Ui
    Browser e6@-->|"[PORT]: Atom.subscribable"| Ui
    Browser e7@-->|"[PORT]: GlbViewport"| Ui
    Browser e12@-->|"[PORT]: Egress"| Ui
    Otel e8@-->|"[TRANSPORT]: Export.live"| Iac
    Otel e10@-->|"[TRANSPORT]: Profile.live"| Iac
    Otel e11@-->|"[PORT]: Vital.Report"| Ui
```

## [04]-[INTERNAL]

```mermaid
---
config:
  layout: elk
  flowchart:
    curve: linear
    padding: 25
---
flowchart LR
    accTitle: Runtime execution spine
    accDescr: Protocol ingress continues its carrier through serving and work, then emits signals and delivery outcomes.
    Ingress([protocol ingress])
    Carrier[otel · Carrier continuation]
    Serve[serve · front door]
    Work[work · durable execution]
    Signals[otel · signal bridge]
    Delivery[net/work · delivery]
    Egress([response + transport])
    Ingress e1@-->|"continue: Carrier.Context"| Carrier
    Carrier e2@-->|"scope: request"| Serve
    Serve e3@-->|"dispatch: intent"| Work
    Work e4@-->|"publish: work facts"| Signals
    Work e5@-->|"deliver: claimed work"| Delivery
    Signals e6@-->|"export: signals"| Egress
    Delivery e7@-->|"emit: transport"| Egress
```

`proc` is the substrate every plane boots on: a runtime is a row, config resolves once, flags evaluate as data, lifecycle folds evidence, workers speak one protocol. `net` owns egress geometry — every outbound call inherits a lane's compiled pulse and circuit row, every long-lived channel one frame vocabulary, every broadcast the engine-blind fanout port, every agreement the coordination port over the same wire. `otel` owns the wire half of observability under one ambient redaction scrub every capture seam inherits; its vocabulary lives in core.

`serve` enforces the one front-door law: libraries export route, verb, and group data, the app assembles exactly one `HttpApi`, one CLI root, and one serve fold, and faults leave only as self-rendering `Problem`s. `work` prices every durable surface against one `WorkClass` table, so the durable plane shares one service-class economy. `ai` folds five providers onto one capability table and satisfies the data stratum's retrieval ports. `browser` is the same package under the browser condition: one boot, one shell, one persistence vocabulary, one typed router carrying the session plane.

## [05]-[BOUNDARIES]

- App root, never this folder, assembles the `HttpApi`, satisfies port `Tag`s, selects runtime rows, and binds the browser composition root.
- Degradation reads as a `Layer` choice the composition root makes; a capability never narrows its own surface under pressure as hidden behavior.
- Data owns the record of truth; work composes data's outbox and mailbox, never a second store; NATS carries fanout and replay, never truth.
- Content identity is never minted here; the browser decode worker delegates to the core `Digest` engine.
- Cluster runs leaderless over `RunnerStorage` advisory locks; the node-bound cluster and rpc-http upstreams are never admitted.
