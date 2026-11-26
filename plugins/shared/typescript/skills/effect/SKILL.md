---
name: effect
description: "Use when calling an Effect module family, covering HttpApi, HTTP clients and routes, RPC, sockets and atoms, SQL, AI, MCP and chat platforms, child processes, workers and CLIs, persistence and durable execution, files, encodings, crypto, and telemetry"
---

# [TYPESCRIPT_EFFECT]

Covers module families `effect` exports by path (`effect/<family>`) and the `@effect/*` packages adding platform layers, drivers, and bridges.

[REFERENCES]:
- [01]-[HTTP](references/http.md): HttpApi contracts, middleware, handlers, HTTP clients, raw routes, and GraphQL clients
- [02]-[RPC](references/rpc.md): RPC groups, servers, clients, protocols, and sockets
- [03]-[REACTIVITY](references/reactivity.md): Atoms over HttpApi and RPC clients, invalidation keys, and React hooks
- [04]-[SQL](references/sql.md): Statements, schema queries, transactions, drivers, and table reconciliation
- [05]-[AI](references/ai.md): Tools, language and decision models, and chat platforms
- [06]-[MCP](references/mcp.md): MCP servers and clients
- [07]-[PROCESSES](references/processes.md): Child processes, worker threads and pools, and command-line programs
- [08]-[PERSISTENCE](references/persistence.md): Stores, caches, queues, limits, and durable execution
- [09]-[PLATFORM](references/platform.md): Files, encodings, crypto, tokens, and addresses

## [01]-[FAMILIES]

Families serve one need each, and a need a row excludes takes the family it names:

| [INDEX] | [FAMILY]                       | [FITS]                                     | [NOT]                                    |
| :-----: | :----------------------------- | :----------------------------------------- | :--------------------------------------- |
|  [01]   | `effect/http-api`              | Endpoints a codebase serves and calls      | Third-party APIs (`HttpClient`)          |
|  [02]   | `effect/http`                  | Third-party HTTP, raw routes, static files | Endpoints a contract types (`HttpApi`)   |
|  [03]   | `effect/graphql`               | GraphQL servers a client calls             | Serving GraphQL, no server module        |
|  [04]   | `effect/rpc`, `effect/socket`  | Procedures over sockets, stdio, workers    | Public APIs needing OpenAPI (`HttpApi`)  |
|  [05]   | `effect/reactivity`            | UI state derived from effects              | Server state (`SubscriptionRef`)         |
|  [06]   | `effect/sql`                   | Relational reads and writes                | Key lookups (`KeyValueStore`)            |
|  [07]   | `effect/ai`                    | Language models, tools, MCP                | Chat platform bots (`effect/chat`)       |
|  [08]   | `effect/chat`                  | Chat platform bots                         | Model conversations (`effect/ai` `Chat`) |
|  [09]   | `effect/process`, `/workers`   | Programs and modules beside the process    | Work one thread's fibers run             |
|  [10]   | `effect/cli`                   | Programs taking flags, servers included    | Programs taking no flag (`Layer.launch`) |
|  [11]   | `effect/persistence`           | Caches, stores, queues outliving one call  | Values one process caches (`Cache`)      |
|  [12]   | `effect/workflow`, `/cluster`  | Work surviving restarts and machines       | Single jobs (`PersistedQueue`)           |
|  [13]   | `effect/eventlog`              | Local-first state synced across devices    | Server-owned rows (`effect/sql`)         |
|  [14]   | `effect/encoding`              | Byte, text, and archive formats            | Records a schema decodes (`Schema`)      |
|  [15]   | `effect/protobuf`              | Protobuf messages from descriptors         | Schema-typed binary (`SchemaBinary`)     |
|  [16]   | `effect/crypto`, `effect/jose` | Digests, keys, ciphers, signed tokens      | Structural hashes (`Hash.hash`)          |
|  [17]   | `effect/net`                   | Addresses and name resolution              | Connections (`effect/socket`)            |
|  [18]   | `effect/observability`         | Telemetry export                           | Spans and logs in code (`Effect.fn`)     |
|  [19]   | `effect/devtools`              | Spans streamed to a devtools process       | Collectors (`effect/observability`)      |

## [02]-[HOSTS]

Runtimes supply platform services through layers, and the root provides them once:

| [INDEX] | [RUNTIME]             | [LAYERS]                                                                                             |
| :-----: | :-------------------- | :--------------------------------------------------------------------------------------------------- |
|  [01]   | Node process          | `NodeServices.layer`, `NodeHttpServer.layerConfig`, `NodeHttpClient.layerUndici`, `NodeCrypto.layer` |
|  [02]   | Browser               | `FetchHttpClient.layer`, `layerWebCrypto` per crypto service, `KeyValueStore.layerStorage`           |
|  [03]   | Fetch handler         | `HttpServer.layerServices`, `FetchHttpClient.layer`, `ConfigProvider.fromUnknown(env)`               |
|  [04]   | Engine with own calls | `FileSystem.make(FileSystem.makeNoop(...))`, `ChildProcessSpawner.make`, `Path.make`                 |

- `NodeServices.layer` provides `ChildProcessSpawner`, `FileSystem`, `Path`, `Stdio`, and `Terminal`, the environment a CLI command reads
- `NodeHttpServer.layerConfig` supplies `HttpPlatform`, `Etag.Generator`, and every `NodeServices` service beside the server
- `NodeCrypto.layer` provides every `effect/crypto` service, each supplying its own `SecureRandom`, and `NodeServices` holds none
- `HttpServer.layerServices` supplies `HttpPlatform`, `Etag.Generator`, `Path`, and a `FileSystem` holding no files, meeting `HttpApiBuilder.layer` on a fetch handler

Engines that run code with calls of their own and no platform layer (the Claude Code hooks engine) take platform services built over those calls, so libraries reading `FileSystem`, `ChildProcessSpawner`, and `Path` run unchanged:
- Engine rejections map to `PlatformError.systemError({ _tag, module, method, pathOrDescriptor, cause })`, `_tag` read from the errno code
- `Layer.succeed(Scheduler.Scheduler, scheduler)` sets how fibers yield, `new Scheduler.MixedScheduler('sync')` yielding through microtasks
- `MixedScheduler` subclasses overriding `shouldYield` yield by elapsed clock time for an engine that stops a long synchronous stretch

## [03]-[CONTRACTS]

Contracts (`HttpApi`, `RpcGroup`, `Toolkit`, `GraphQLGroup`, `Entity`, `Workflow`, `EventGroup`) are schema declarations every side derives from, handlers binding through one layer built from services read once and clients deriving a method per member:
- Contract fields read the record schema owning them (`params: { handle: Bound.fields.handle }`), never a restated `Schema.String`

Use `typescript:testing` for contract test clients.

## [04]-[ERRORS]

Family errors are one class holding a tagged `reason` union: `HttpClientError`, `HttpServerError`, `SqlError`, `AiError`, `McpClientError`, `PlatformError`, `WorkerError`, `RpcClientError`, `SocketError`, `CryptoError`, `JoseError`, `GraphQLClientError`, `ChatError`, `RateLimiterError`, and `CircuitBreakerError`. Handlers select a reason by tag, and a package error declaring a `reason` union takes the same members:
- `Effect.catchReason(error, reason, f, orElse)` handles one reason, `orElse` receiving every other reason of that error
- `Effect.catchReason('JoseError', 'KeySetUnavailable', Effect.fail, () => Effect.succeedNone)` fails one reason and recovers the rest
- `Effect.catchReasons(error, { Reason: f }, orElse)` handles a reason per key, and `Effect.unwrapReason(error)` puts the reasons in place of their error
- Causes holding a defect skip the handler and turn every typed error into a defect
- `PlatformError` reasons are `BadArgument` and a `SystemError` with a `SystemErrorTag` (`NotFound`, `PermissionDenied`, `AlreadyExists`)
- `Effect.catchReason('PlatformError', 'NotFound', () => Effect.succeed(Array.empty<string>()))` reads a missing folder as empty

## [05]-[TELEMETRY]

Libraries emit logs, spans, and metrics and export nothing. Each host entry provides one telemetry layer, `layer` of `@rasm/telemetry/otlp` in browser, Worker, and plug-in runtimes and `layerNodeFromConfig` of `@rasm/telemetry/node` in a Node process, and Node roots pass its `teardown` to `runMain`. A collector's transport selects the export member that layer composes:

| [INDEX] | [TRANSPORT]                     | [LAYER]                                                                    |
| :-----: | :------------------------------ | :------------------------------------------------------------------------- |
|  [01]   | OTLP push, endpoint from env    | `Otlp.layerFromConfig({ resource })` over an `OtlpSerialization` layer     |
|  [02]   | OTLP push, endpoint from code   | `Otlp.layerJson({ baseUrl })`, `Otlp.layerProtobuf({ baseUrl })`           |
|  [03]   | Prometheus scrape               | `PrometheusMetrics.layerHttp({ path })` on `HttpRouter`                    |
|  [04]   | Browser OTLP through own origin | `OtlpProxy.layer({ baseUrl, headers })` on the server's `HttpRouter`       |
|  [05]   | OpenTelemetry Node SDK          | `startNodeSdk` of `@opentelemetry/sdk-node` under `OtelTracer.layerGlobal` |

`Otlp.layerFromConfig` exports a signal when `OTEL_EXPORTER_OTLP_<SIGNAL>_ENDPOINT` or `OTEL_EXPORTER_OTLP_ENDPOINT` holds a URL and `OTEL_<SIGNAL>_EXPORTER` lists `otlp`, and provides a `Flusher` with nothing to flush otherwise:
- Endpoints from `OTEL_EXPORTER_OTLP_ENDPOINT` take `/v1/<signal>` appended
- Headers read `OTEL_EXPORTER_OTLP_<SIGNAL>_HEADERS`, then `OTEL_EXPORTER_OTLP_HEADERS`
- Resources read `OTEL_SERVICE_NAME`, `OTEL_SERVICE_VERSION`, and `OTEL_RESOURCE_ATTRIBUTES`, named variables winning over attributes
- `OTEL_SDK_DISABLED` disables every signal, and `OTEL_METRIC_EXPORT_INTERVAL`, `OTEL_BSP_SCHEDULE_DELAY`, `OTEL_BLRP_SCHEDULE_DELAY` set push intervals
- `OTEL_EXPORTER_OTLP_PROTOCOL` reaches no Effect member, so the telemetry layer maps `http/json` and `http/protobuf` to an `OtlpSerialization` layer
- `OtlpExporter.Flusher` `flush` drains every exporter concurrently, never fails, and has no timeout, a short-lived host bounding it with `Effect.timeoutOption`
- `NodeSdk.layer` enables a signal only for a processor or reader its configuration names and takes no instrumentations
- `Metric.enableRuntimeMetricsLayer` adds fiber runtime metrics where metrics export

Loggers are the set `Logger.CurrentLoggers` holds, the root rewrites it, and a stream's reading process decides where records go:
- `Logger.layer(loggers)` replaces the set, `{ mergeWithExisting: true }` adds to it, and OTLP loggers merge by default
- `Layer.effect(Logger.CurrentLoggers, ...)` over the current set drops `Logger.defaultLogger` and keeps exporter loggers
- `Logger.map(Logger.formatSimple, (line) => host.log(line))` writes each formatted line to a host's own log call
- `Logger.tracerLogger` writes each log as an event on the current local span, writing none under an external or ended span
- `Logger.LogToStderr` set `true` moves default and pretty loggers to stderr, and `Logger.withConsoleError(Logger.formatJson)` logs JSON to stderr
- `Logger.toFile(logger, path)` batches into one file it never rotates, `toRotatingFile` of `@rasm/telemetry/node` rotating by size and age
- `References.MinimumLogLevel` drops lines before any logger runs, so per-scope floors set it to the lowest floor and filter inside each logger
- `Command.run` parses `--log-level` into `References.MinimumLogLevel`, and a root running no command provides that reference
- `ErrorReporter.layer([ErrorReporter.make(report)], { mergeWithExisting: true })` receives each reported failure
- Error classes set `[ErrorReporter.severity]` for their level and `[ErrorReporter.ignore]` to skip reporting
- `Effect.annotateLogs` covers one effect, `Effect.annotateLogsScoped` the rest of a scope, and `Effect.withLogSpan(label)` times each line

Libraries read settings under a config path (`log.level`), and a root's provider maps the path to its spelling. `ConfigProvider.fromEnv().pipe(ConfigProvider.nested(app), ConfigProvider.constantCase)` reads `log.level` from `<APP>_LOG_LEVEL`, `ConfigProvider.orElse(ConfigProvider.fromEnv())` keeping `OTEL_*` variables readable.

Spans name an operation, and attributes hold the values that differ per call:
- Spans take `{ attributes, links, parent, root, kind, sampled, level }`, and spans below `Tracer.MinimumTraceLevel` go unsampled with their descendants
- HTTP server and client spans take the method alone as name, and `HttpMiddleware.SpanNameGenerator` and `HttpClient.SpanNameGenerator` rename them
- Request span names are `<method> <target>` over a low-cardinality target (a route, a URI template), each concrete value an attribute
- `HttpTraceContext.toHeaders(span)` writes a span to headers, and `fromHeaders(headers)` returns an `Option` of the remote span to parent from
- `toEnvironment` of `@rasm/telemetry/node` writes W3C trace context and baggage into a child's environment, and `spawn` of `@rasm/process` merges it
- `layerFromEnvironment` of `@rasm/telemetry/node` parents a child's spans through `Layer.parentSpan(makeExternalSpan(...))`

Metrics take names from `@opentelemetry/semantic-conventions` (`METRIC_*`, `ATTR_*`, `/incubating` for development ones), and an attribute read from the outcome records after the run:

```ts
const operationDuration = Metric.mapInput(Metric.histogram(METRIC_MCP_SERVER_OPERATION_DURATION, { boundaries: [0.01, 0.1, 1, 10, 60] }), Duration.toSeconds);

const measured = Effect.fnUntraced(function* <A, E, R>(effect: Effect.Effect<A, E, R>, attributes: Readonly<Record<string, string>>) {
    const [duration, exit] = yield* Effect.timed(Effect.exit(effect));
    yield* Metric.update(Metric.withAttributes(operationDuration, { ...attributes, ...failureAttributes(exit) }), duration);
    return yield* exit;
});
```

`failureAttributes` maps an exit to `ATTR_ERROR_TYPE` (`cancelled` for an interrupt, the failure's `_tag`, else `ERROR_TYPE_VALUE_OTHER`). Metrics with attributes known before the run take a tracking member:
- `Effect.track(metric)`, `trackDuration`, `trackErrors`, and `trackSuccesses` update a metric once per run
- `Metric.timer` records milliseconds, and a convention stating seconds maps its histogram's input through `Duration.toSeconds`
