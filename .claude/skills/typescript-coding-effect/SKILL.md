---
name: typescript-coding-effect
description: "Use when calling an Effect module family, covering HTTP and HttpApi, SQL, AI and MCP, RPC and atoms, processes, CLI, crypto, encoding, and telemetry"
---

# [TYPESCRIPT_CODING_EFFECT]

[REFERENCES]:
- [01]-[HTTP](references/http.md): HttpApi contracts, handlers, derived clients, and HTTP client policies
- [02]-[SQL](references/sql.md): Statements, schema queries, transactions, and drivers
- [03]-[AI](references/ai.md): Tools, MCP servers and clients, and language and decision models
- [04]-[REACTIVITY](references/reactivity.md): RPC groups and atoms over derived clients
- [05]-[PLATFORM](references/platform.md): Processes, commands, files, crypto, encodings, and durable execution

## [01]-[HOSTS]

Each runtime supplies the platform services a program reads through published layers, and the root provides them once:

| [INDEX] | [RUNTIME]     | [LAYERS]                                                                                                    |
| :-----: | :------------ | :---------------------------------------------------------------------------------------------------------- |
|  [01]   | Node process  | `NodeServices.layer`, `NodeHttpServer.layerConfig`, `NodeHttpClient.layerUndici`, `NodeCrypto.layer`        |
|  [02]   | Browser       | `FetchHttpClient.layer`, `BrowserCrypto.layer`, `BrowserKeyValueStore.layerLocalStorage`                    |
|  [03]   | Fetch handler | `HttpServer.layerServices`, `FetchHttpClient.layer`, `SecureRandom.layerWebCrypto`, `Digest.layerWebCrypto` |

- `NodeServices.layer` provides `ChildProcessSpawner`, `FileSystem`, `Path`, `Stdio`, and `Terminal`, the environment a CLI command reads
- Crypto services have no default layer, the root provides its runtime's layer or a family's `layerWebCrypto`
- `NodeHttpServer.layerConfig` supplies `HttpPlatform`, `Etag.Generator`, `FileSystem`, and `Path` beside the server
- `HttpServer.layerServices` supplies them where no platform does, with a `FileSystem` holding no files
- `HttpRouter.serve` builds its routes in a fork of the memo map, and services sibling layers share are provided outside it
- `HttpRouter.serve` and `toWebHandler` log each request unless `disableLogger`, and `serve` logs listen address unless `disableListenLog`

## [02]-[TELEMETRY]

Logs, spans, and metrics leave the process through one layer at the root:

```ts
const Telemetry = Otlp.layerFromConfig().pipe(
    Layer.provide([FetchHttpClient.layer, OtlpSerialization.layerProtobuf]),
    Layer.provideMerge(Logger.layer([Logger.consoleJson, Logger.tracerLogger])),
);
```

`Otlp.layerFromConfig` reads `OTEL_EXPORTER_OTLP_ENDPOINT`, `OTEL_SERVICE_NAME`, `OTEL_RESOURCE_ATTRIBUTES`, and the batch variables, and `Logger.tracerLogger` keeps each log a span event:
- `Logger.layer(loggers)` replaces the current loggers, `{ mergeWithExisting: true }` adds to them
- `Logger.LogToStderr` set `true` moves the default and pretty loggers to stderr, `Logger.consoleJson` stays on stdout, and `Logger.withConsoleError(Logger.formatJson)` is the stderr JSON logger
- `Command.run` parses `--log-level` into `References.MinimumLogLevel`, a root running no command provides that reference, and a telemetry layer inside the handler holds no level of its own
- stdout and stderr hold logs for the supervisor or container that owns their file, `Logger.toFile` serves a tool reading its own records back
- `Effect.annotateLogs` covers one effect, `Effect.annotateLogsScoped` the rest of a scope, and `Effect.withLogSpan(label)` times each line
- Spans take `{ attributes, links, parent, kind, sampled, level }`, and spans under `Tracer.MinimumTraceLevel` drop with their children
- Request spans take `<method> <target>` names over a low-cardinality target (a route, a URI template), each concrete value an attribute
- `HttpTraceContext.fromHeaders(headers)` reads a remote `parent` from the headers a request or protocol message holds
- `Effect.track(metric)`, `trackDuration(timer)`, `trackErrors`, and `trackSuccesses` update a metric per run, `Metric.withAttributes` tagging it
- `Metric.timer` records milliseconds, and a duration a convention states in seconds is `Metric.mapInput(Metric.histogram(...), Duration.toSeconds)`
- `OtlpExporter.Flusher` drains logs, metrics, and traces before a short-lived host returns
