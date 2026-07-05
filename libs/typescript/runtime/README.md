# [TS_RUNTIME]

`libs/typescript/runtime` is the wave-3 execution package of the branch: the process substrate (runtime rows, config chain, flags, lifecycle, workers), outbound and framed transport with the fanout/replay port, the OTLP telemetry wire, the one public front door (HttpApi assembly, serving, realtime, problem law, CLI), durable work (entities, workflows, queues, schedules, delivery, reports), the intelligence spine (models, embeddings, tools, agents, MCP), and the browser runtime (boot, PWA shell, persistence, routing, byte transport). One arity owns each modality; engines, providers, and channels are rows. `ARCHITECTURE.md` carries the domain map and seams, `IDEAS.md` the forward pool, and `TASKLOG.md` the open work.

## [01]-[ROUTER]

- [01]-[EXEC](.planning/proc/exec.md)
- [02]-[CONFIG](.planning/proc/config.md)
- [03]-[FLAG](.planning/proc/flag.md)
- [04]-[LIFE](.planning/proc/life.md)
- [05]-[WORKER](.planning/proc/worker.md)
- [06]-[CLIENT](.planning/net/client.md)
- [07]-[CHANNEL](.planning/net/channel.md)
- [08]-[PUBSUB](.planning/net/pubsub.md)
- [09]-[EMIT](.planning/otel/emit.md)
- [10]-[CRASH](.planning/otel/crash.md)
- [11]-[VITAL](.planning/otel/vital.md)
- [12]-[API](.planning/serve/api.md)
- [13]-[ROUTE](.planning/serve/route.md)
- [14]-[LIVE](.planning/serve/live.md)
- [15]-[PROBLEM](.planning/serve/problem.md)
- [16]-[CLI](.planning/serve/cli.md)
- [17]-[ENTITY](.planning/work/entity.md)
- [18]-[FLOW](.planning/work/flow.md)
- [19]-[QUEUE](.planning/work/queue.md)
- [20]-[SCHEDULE](.planning/work/schedule.md)
- [21]-[DELIVER](.planning/work/deliver.md)
- [22]-[REPORT](.planning/work/report.md)
- [23]-[MODEL](.planning/ai/model.md)
- [24]-[EMBED](.planning/ai/embed.md)
- [25]-[TOOL](.planning/ai/tool.md)
- [26]-[AGENT](.planning/ai/agent.md)
- [27]-[BOOT](.planning/browser/boot.md)
- [28]-[SHELL](.planning/browser/shell.md)
- [29]-[PERSIST](.planning/browser/persist.md)
- [30]-[ROUTE_BROWSER](.planning/browser/route.md)
- [31]-[FETCH](.planning/browser/fetch.md)

## [02]-[DOMAIN_PACKAGES]

Every folder-specific external library, planned or implemented. Versions are centralized in `pnpm-workspace.yaml`; corroborating API evidence lives in the adjacent `.api/` folder.

[DURABLE_EXECUTION]:
- `@effect/cluster`
- `@effect/workflow`
- `@effect/rpc`

[FANOUT_REPLAY]:
- `@nats-io/nats-core`
- `@nats-io/jetstream`
- `@nats-io/kv`
- `@nats-io/obj`

[INTELLIGENCE]:
- `@effect/ai`
- `@effect/ai-anthropic`
- `@effect/ai-openai`
- `@effect/ai-google`
- `@effect/ai-amazon-bedrock`
- `@effect/ai-openrouter`
- `@modelcontextprotocol/sdk`

[TERMINAL]:
- `@effect/cli`
- `@effect/printer`
- `@effect/printer-ansi`

[FLAGS]:
- `@openfeature/server-sdk`

[OTLP_WIRE]:
- `@opentelemetry/api-logs`
- `@opentelemetry/core`
- `@opentelemetry/resources`
- `@opentelemetry/sdk-logs`
- `@opentelemetry/sdk-metrics`
- `@opentelemetry/sdk-trace-base`
- `@opentelemetry/sdk-trace-node`
- `@opentelemetry/sdk-trace-web`
- `@opentelemetry/exporter-trace-otlp-http`
- `@opentelemetry/exporter-metrics-otlp-http`
- `@opentelemetry/exporter-logs-otlp-http`

[DELIVERY_DOCUMENTS]:
- `nodemailer`
- `exceljs`
- `jspdf`
- `jszip`
- `papaparse`

[BROWSER_SHELL]:
- `workbox-build`
- `workbox-window`
- `idb-keyval`
- `nuqs`

## [03]-[SUBSTRATE_PACKAGES]

Cross-cutting TypeScript substrate this folder consumes; canonical registry and charters live in `libs/typescript/.planning/README.md` and the adjacent `libs/typescript/.api/` folder.

[TYPING_RAILS]:
- `effect`

[PLATFORM]:
- `@effect/platform`
- `@effect/platform-node`
- `@effect/platform-bun`
- `@effect/platform-browser`

[OVERLAY]:
- `@effect/experimental`

[OTLP_BRIDGE]:
- `@effect/opentelemetry`
