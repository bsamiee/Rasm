# [CENSUS_RUNTIME]

`libs/typescript/runtime` — wave-3 execution package, seven sub-domains, 31 design pages, 7247 LOC, 38 folder-local `.api` catalogs (3549 LOC) plus 7 shared substrate catalogs at `libs/typescript/.api/`.

## [A]-[FILE_REGISTER]

Each row: page — owned concept — entry surface(s) — fence mass class (by LOC: S <150, M 150-300, L >300).

### proc/ (process substrate — 1054 LOC)
| Page | Owns | Entry surface | Class |
|---|---|---|---|
| `exec.md` (157) | keyed `node\|bun` runtime-row table, `runMain` boot edge, `Proc.Spec` subprocess | `Runtime`, `Proc`, `ExecFault` | S |
| `config.md` (203) | ordered `ConfigProvider` chain, boot-validated `Setting` contract | `Provider`, `Setting` | S |
| `flag.md` (436) | OpenFeature `Provider` impl, recursive targeting-rule family, `Verdict` | `Rollout`, `Sticky`, `Verdict`, `Flags` | L |
| `life.md` (168) | phase spine, ranked drain/probe registries, one budgeted row executor | `Life` | S |
| `worker.md` (90) | `Schema.TaggedRequest` off-thread protocol, one pool, runner boot | request classes, pool Tag/Layer, runner Layer | S |

### net/ (egress + fanout — 458 LOC)
| Page | Owns | Entry surface | Class |
|---|---|---|---|
| `client.md` (111) | one outbound HTTP lane table, budget-ledger-derived retry/status/trace composition | `Client` | S |
| `channel.md` (130) | framed socket duplex + SSE `Sse` codec (both directions) | `Socket`/`Sse` owners | S |
| `pubsub.md` (217) | `Fanout` port — in-process `PubSub` row + NATS JetStream row, guarantee-ledger data | `Fanout` | S |

### otel/ (telemetry wire — 602 LOC)
| Page | Owns | Entry surface | Class |
|---|---|---|---|
| `emit.md` (291) | `Export.live(policy)` OTLP egress Layer, W3C ingress continuation, `Redaction` scrub | `Export` | M |
| `crash.md` (146) | total `Cause→fatal` fold, forensic enrichment, breadcrumb ring | crash capture Layer | S |
| `vital.md` (165) | six RUM vital rows, one `PerformanceObserver` bridge, `mapAccum` grading | vital emission | S |

### serve/ (front door — 1417 LOC)
| Page | Owns | Entry surface | Class |
|---|---|---|---|
| `api.md` (432) | assembly law — domain `HttpApiGroup`/`RpcGroup` data, app assembles one `HttpApi`, OpenAPI/SDK/RPC-caller projections | assembly law (no lib-side value) | L |
| `route.md` (258) | `HttpLayerRouter` serving — `addHttpApi`, `Mount` port, tus dispatchers, Intake verify, `ASSET_ROWS` | route Layers | M |
| `live.md` (277) | SSE/WS realtime over branch feeds, resume-token law, admission gate | realtime endpoints | M |
| `problem.md` (201) | `Problem` — RFC 9457 self-rendering owner, total refusal fold, `blame`-derived exposure | `Problem` | S |
| `cli.md` (249) | `Command` verb-family data, flag-config bridge, one `Doc<Ansi>` render seam, `Ops` runbook | `Verb`, `Ops` | M |

### work/ (durable work — 1154 LOC)
| Page | Owns | Entry surface | Class |
|---|---|---|---|
| `entity.md` (186) | `WorkClass` pricing table, `Actor` mint, durable `Mailbox`, leaderless `Grid` sharding | `WorkClass`, `Actor`, `Mailbox`, `Grid` | S |
| `flow.md` (156) | `Step` mint, `Flow` definition, saga compensation, `Gate` signal owner, `WorkflowProxy` | `Flow`, `Gate` | S |
| `queue.md` (181) | native `DurableQueue`/`DurableRateLimiter`, pg-composed lane policy, dead-letter/replay | queue Layer | S |
| `schedule.md` (110) | `Cadence` row, `ClusterCron` registration fold, misfire/catch-up posture | `Cadence` | S |
| `deliver.md` (280) | one channel table (mail/webhook), one receipt, one suppression fold, outbox relay singleton | `Deliver` | M |
| `report.md` (241) | `Report.Spec`, three engine arms (xlsx/pdf/csv) over one decoded-row set, `bundle` archive fold | `Report` | S |

### ai/ (intelligence spine — 730 LOC)
| Page | Owns | Entry surface | Class |
|---|---|---|---|
| `model.md` (207) | five-provider capability table, `ExecutionPlan` fallback, guardrail gate, token economy | `Model`, `Tokenizer` | S |
| `embed.md` (173) | deterministic chunking, embedding capability rows, two-tier cache, `Embedder`/`Reranker` port satisfaction | `Embedder`, `Reranker` | S |
| `tool.md` (211) | `Toolkit` assembly, `Safety` partition, `Arsenal` provider-tool ledger, dual MCP lanes (native host / SDK-client consume) | `Toolkit`, `Safety` | S |
| `agent.md` (139) | `Transition`-machine session, persisted `Chat`, one turn fold, `Act`/`Turn`/`AgentFault` triple | `Agent` | S |

### browser/ (browser condition — 1832 LOC)
| Page | Owns | Entry surface | Class |
|---|---|---|---|
| `boot.md` (198) | `BrowserRuntime.runMain` single-boot law, `AppSpec` budget, host-signal cells, capability roster | `Boot`, `AppSpec` | S |
| `shell.md` (392) | PWA manifest as typed value, `Workbox` scoped resource, `SwLifecycle`, background-sync outbox drain | `Shell` | L |
| `persist.md` (306) | closed `_domains` IndexedDB vocabulary, one polymorphic lane surface, `StorageManager` residency verdicts | `Kv` | L |
| `route.md` (552) | Navigation-API typed router, `nuqs` query codec, session-residency plane, admission fold | `Router`, session plane | L |
| `fetch.md` (384) | XHR/WS/worker binding rows, decorated browser dial, decode-worker `Depot` mint delegation | `Fetch`, `Depot` | L |

## [B]-[API_ROSTER]

Folder-local `.api/` (38 files, 3549 LOC) — package documented, depth signal (LOC):

| Cluster | Package | LOC | Depth |
|---|---|---|---|
| Durable execution | `effect-cluster` | 106 | moderate |
| | `effect-workflow` | 103 | moderate |
| | `effect-rpc` | 123 | moderate |
| Fanout/replay | `nats-io-nats-core` | 47 | thin |
| | `nats-io-jetstream` | 51 | thin |
| | `nats-io-kv` | 46 | thin |
| | `nats-io-obj` | 46 | thin |
| Intelligence | `effect-ai` | 354 | deep |
| | `effect-ai-anthropic` | 124 | moderate |
| | `effect-ai-openai` | 157 | moderate |
| | `effect-ai-google` | 84 | thin |
| | `effect-ai-amazon-bedrock` | 122 | moderate |
| | `effect-ai-openrouter` | 123 | moderate |
| | `modelcontextprotocol-sdk` | 152 | moderate |
| Terminal | `effect-cli` | 92 | moderate |
| | `effect-printer` | 140 | moderate |
| | `effect-printer-ansi` | 92 | moderate |
| Flags | `openfeature-server-sdk` | 53 | thin |
| OTLP wire | `opentelemetry-api-logs` | 59 | thin |
| | `opentelemetry-core` | 95 | moderate |
| | `opentelemetry-resources` | 61 | thin |
| | `opentelemetry-sdk-logs` | 92 | moderate |
| | `opentelemetry-sdk-metrics` | 122 | moderate |
| | `opentelemetry-sdk-trace-base` | 117 | moderate |
| | `opentelemetry-sdk-trace-node` | 44 | thin |
| | `opentelemetry-sdk-trace-web` | 71 | thin |
| | `opentelemetry-exporter-trace-otlp-http` | 51 | thin |
| | `opentelemetry-exporter-metrics-otlp-http` | 54 | thin |
| | `opentelemetry-exporter-logs-otlp-http` | 49 | thin |
| Delivery/documents | `nodemailer` | 80 | thin |
| | `exceljs` | 98 | moderate |
| | `jspdf` | 84 | thin |
| | `jszip` | 76 | thin |
| | `papaparse` | 67 | thin |
| Browser shell | `workbox-build` | 73 | thin |
| | `workbox-window` | 59 | thin |
| | `idb-keyval` | 89 | thin |
| | `nuqs` | 93 | moderate |

Shared substrate (owned at `libs/typescript/.api/`, consumed here, not folder-local): `effect.md`, `effect-platform.md`, `effect-platform-node.md`, `effect-platform-bun.md`, `effect-platform-browser.md`, `effect-experimental.md`, `effect-opentelemetry.md` — all seven present; no substrate catalog gap.

Every README `[02]-[DOMAIN_PACKAGES]`/`[03]-[SUBSTRATE_PACKAGES]` entry (38 domain + 7 substrate = 45) has a corresponding catalog file. No orphan `.api` file and no package named in the README without a catalog — the roster is closed and 1:1.

## [C]-[CAPABILITY_MAP]

Scope genuinely owns, matching README/ARCHITECTURE claims, across all seven sub-domains:

- `proc`: runtime-row table, config chain, OpenFeature flag provider, lifecycle/drain/probe fold, off-thread worker protocol — all five pages present, no thin stub.
- `net`: outbound lane table, framed channel + SSE codec, fanout port with in-process and JetStream rows — three pages present, matches ARCHITECTURE's `net` domain-map entries exactly.
- `otel`: OTLP export/ingest, crash capture, browser RUM vitals — three pages present; `emit.md` explicitly marks the SDK/exporter `.api` block as an `[R3]` pin awaiting native `Otlp` parity (a declared, not accidental, transitional state — not a mismatch).
- `serve`: HttpApi assembly law, HttpLayerRouter serving, realtime, RFC 9457 problem, CLI — five pages present, matching the "one public front door" claim.
- `work`: entity/mailbox/grid, workflow suspend-replay, durable queue, cron schedule, mail/webhook delivery, document report — six pages present, matching "durable work" claim in full.
- `ai`: five-provider model table, embed/retrieval-port satisfaction, tool/toolkit/MCP duality, agent turn-fold — four pages present, matching "intelligence spine" claim.
- `browser`: boot, PWA shell, persistence, router, byte transport — five pages present, matching "browser runtime" claim; ARCHITECTURE explicitly frames browser as the same package's browser condition, never a sibling, and no page claims otherwise.

No mismatch found between README `[01]-[ROUTER]` (31 linked pages), ARCHITECTURE `[01]-[DOMAIN_MAP]` (31 codemap leaves across seven `src/` sub-folders), and the actual `.planning` tree (31 files across seven sub-folders) — router index, domain map, and disk are in exact 1:1:1 correspondence. Package roster (README `[02]`/`[03]`) and `.api` catalog set are likewise 1:1 with no orphan or gap. This folder census surfaces zero drift for downstream phases to reconcile; the only forward-looking note is the declared `[R3]` OTel SDK-pin collapse condition inside `emit.md`, which is a scoped future-state marker, not a current mismatch.
