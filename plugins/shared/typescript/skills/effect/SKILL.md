---
name: effect
description: "Use when calling an Effect module family, covering HttpApi, HTTP clients and routes, RPC, sockets and atoms, SQL, AI, MCP and chat platforms, child processes, workers and CLIs, persistence and durable execution, files, encodings, crypto, and telemetry"
---

# [TYPESCRIPT_EFFECT]

Covers module families `effect` exports by path (`effect/<family>`) and layers `@effect/platform-node` adds.

[REFERENCES]:
- [01]-[HTTP](references/http.md): HttpApi contracts, middleware, handlers, HTTP clients, raw routes, and GraphQL clients
- [02]-[RPC](references/rpc.md): RPC groups, protocols, sockets, and atoms over derived clients
- [03]-[SQL](references/sql.md): Statements, schema queries, transactions, and drivers
- [04]-[AI](references/ai.md): Tools, MCP servers and clients, language and decision models, and chat platforms
- [05]-[PROCESSES](references/processes.md): Child processes, worker threads and pools, and command-line programs
- [06]-[PERSISTENCE](references/persistence.md): Stores, caches, queues, limits, and durable execution
- [07]-[PLATFORM](references/platform.md): Files, encodings, crypto, tokens, and addresses

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
|  [10]   | `effect/cli`                   | Command-line programs                      | Long-running servers (`Layer.launch`)    |
|  [11]   | `effect/persistence`           | Caches, stores, queues outliving one call  | Values one process caches (`Cache`)      |
|  [12]   | `effect/workflow`, `/cluster`  | Work surviving restarts and machines       | Single jobs (`PersistedQueue`)           |
|  [13]   | `effect/eventlog`              | Local-first state synced across devices    | Server-owned rows (`effect/sql`)         |
|  [14]   | `effect/encoding`              | Byte, text, and archive formats            | Records a schema decodes (`Schema`)      |
|  [15]   | `effect/protobuf`              | Protobuf messages from descriptors         | Schema-typed binary (`SchemaBinary`)     |
|  [16]   | `effect/crypto`, `effect/jose` | Digests, keys, ciphers, signed tokens      | In-process keys (`Hash.hash`)            |
|  [17]   | `effect/net`                   | Addresses and name resolution              | Connections (`effect/socket`)            |
|  [18]   | `effect/observability`         | Telemetry export                           | Spans and logs in code (`Effect.fn`)     |
|  [19]   | `effect/devtools`              | Spans streamed to a devtools process       | Collectors (`effect/observability`)      |

## [02]-[HOSTS]

Runtimes supply platform services through published layers, and the root provides them once:

| [INDEX] | [RUNTIME]     | [LAYERS]                                                                                             |
| :-----: | :------------ | :--------------------------------------------------------------------------------------------------- |
|  [01]   | Node process  | `NodeServices.layer`, `NodeHttpServer.layerConfig`, `NodeHttpClient.layerUndici`, `NodeCrypto.layer` |
|  [02]   | Browser       | `FetchHttpClient.layer`, `layerWebCrypto` per crypto service, `KeyValueStore.layerStorage`           |
|  [03]   | Fetch handler | `HttpServer.layerServices`, `FetchHttpClient.layer`, `layerWebCrypto` per crypto service             |

- `NodeServices.layer` provides `ChildProcessSpawner`, `FileSystem`, `Path`, `Stdio`, and `Terminal`, the environment a CLI command reads
- `NodeHttpServer.layerConfig` supplies `HttpPlatform`, `Etag.Generator`, and every `NodeServices` service beside the server
- `HttpServer.layerServices` supplies `HttpPlatform`, `Etag.Generator`, `Path`, and a `FileSystem` holding no files where no platform does

## [03]-[CONTRACTS]

Contracts (`HttpApi`, `RpcGroup`, `Toolkit`, `GraphQLGroup`, `Entity`, `Workflow`, `EventGroup`) are schema declarations every side derives from, handlers binding through one layer built from services read once and clients deriving a method per member. Each family runs a test client in memory over real encoding:

| [INDEX] | [FAMILY] | [TEST_CLIENT]                                                 |
| :-----: | :------- | :------------------------------------------------------------ |
|  [01]   | HttpApi  | `HttpApiTest.groups(Api, groups)`, `NodeHttpServer.layerTest` |
|  [02]   | RPC      | `RpcTest.makeClient(group)`                                   |
|  [03]   | MCP      | `McpTest.makeClient(registrations, { protocols })`            |
|  [04]   | GraphQL  | `GraphQLClient.layerTest`                                     |
|  [05]   | Chat     | `ChatTest.layer`                                              |
|  [06]   | Cluster  | `Entity.makeTestClient`, `TestRunner.layer`                   |
|  [07]   | Workflow | `WorkflowEngine.layerMemory`                                  |

`@effect/vitest` runs effects as tests:
- `it.effect` runs a scoped test under `TestClock` and `TestConsole`, a sleep or timeout advancing only through `TestClock.adjust`
- `it.live` runs a scoped test with live services
- `layer(L)(name, (it) => ...)` shares one build of `L` across a block's tests, `{ buildPerTest: true }` building it per test
- `new TestSchema.Asserts(S)` of `effect/testing` asserts a schema's construction, decoding, encoding, and round trip

## [04]-[ERRORS]

Family errors are one class holding a tagged `reason` union: `HttpClientError`, `SqlError`, `AiError`, `McpClientError`, `PlatformError`, `WorkerError`, `RpcClientError`, `SocketError`, `CryptoError`, `JoseError`, `GraphQLClientError`, `ChatError`, `RateLimiterError`, and `CircuitBreakerError`. `PlatformError` reasons are `BadArgument` and each system error kind (`NotFound`, `PermissionDenied`), and `Effect.catchReason('PlatformError', 'NotFound', f)` handles a missing file alone.

## [05]-[TELEMETRY]

Logs, spans, and metrics leave the process through one root layer, and a collector's transport selects it:

| [INDEX] | [TRANSPORT]                     | [LAYER]                                                          |
| :-----: | :------------------------------ | :--------------------------------------------------------------- |
|  [01]   | OTLP push, endpoint from env    | `Otlp.layerFromConfig()`                                         |
|  [02]   | OTLP push, endpoint from code   | `Otlp.layerJson({ baseUrl })`, `Otlp.layerProtobuf({ baseUrl })` |
|  [03]   | Prometheus scrape               | `PrometheusMetrics.layerHttp({ path })` on `HttpRouter`          |
|  [04]   | Browser OTLP through own origin | `OtlpProxy.layer({ baseUrl })` on the server's `HttpRouter`      |
|  [05]   | OpenTelemetry SDK               | `NodeSdk.layer` of `@effect/opentelemetry`                       |

```ts
const Telemetry = Otlp.layerFromConfig().pipe(Layer.provide([FetchHttpClient.layer, OtlpSerialization.layerProtobuf]), Layer.provideMerge(Logger.layer([Logger.consoleJson, Logger.tracerLogger])));
```

`Otlp.layerFromConfig` exports a signal when `OTEL_EXPORTER_OTLP_<SIGNAL>_ENDPOINT` or `OTEL_EXPORTER_OTLP_ENDPOINT` holds a URL and `OTEL_<SIGNAL>_EXPORTER` lists `otlp`, and sends nothing otherwise:
- Endpoints from `OTEL_EXPORTER_OTLP_ENDPOINT` take `/v1/<signal>` appended, and headers read `OTEL_EXPORTER_OTLP_HEADERS`
- Resources read `OTEL_SERVICE_NAME` and `OTEL_RESOURCE_ATTRIBUTES`
- `OTEL_SDK_DISABLED` disables every signal, and `OTEL_METRIC_EXPORT_INTERVAL` sets the push interval
- `OtlpSerialization.layerJson` serves a collector taking JSON in place of protobuf
- `OtlpExporter.Flusher` drains every signal before a short-lived host returns, awaiting no export in flight and bounded by `Effect.timeoutOption`
- `OtlpProxy` forwards browser exports with credentials the server adds, the browser pointing `Otlp.layerJson({ baseUrl })` at its own origin

Loggers are a set the root replaces or extends, and a stream's reading process decides where records go:
- `Logger.layer(loggers)` replaces the current loggers, `{ mergeWithExisting: true }` adds to them, and `OtlpLogger` adds itself beside them
- `Logger.tracerLogger` writes each log as an event on the current span, a log outside a span writing none
- `Logger.LogToStderr` set `true` moves default and pretty loggers to stderr, and `Logger.withConsoleError(Logger.formatJson)` logs JSON to stderr
- `Command.run` parses `--log-level` into `References.MinimumLogLevel`, and a root running no command provides that reference
- `stdout` and `stderr` hold logs for the supervisor or container owning their file, `Logger.toFile` serving a tool that reads its own records back
- `Effect.annotateLogs` covers one effect, `Effect.annotateLogsScoped` the rest of a scope, and `Effect.withLogSpan(label)` times each line

Spans name an operation, and attributes hold the values that differ per call:
- Spans take `{ attributes, links, parent, kind, sampled, level }`, and spans under `Tracer.MinimumTraceLevel` drop with their children
- HTTP server and client spans take the method alone as name, and `HttpMiddleware.SpanNameGenerator` and `HttpClient.SpanNameGenerator` rename them
- Request span names are `<method> <target>` over a low-cardinality target (a route, a URI template), each concrete value an attribute
- `HttpTraceContext.fromHeaders(headers)` returns an `Option` of the remote span a request or protocol message holds, the `parent` of a new span

Metrics update per run through `Effect.track(metric)`, `trackDuration(timer)`, `trackErrors`, and `trackSuccesses`, `Metric.withAttributes` tagging each. `Metric.timer` records milliseconds, and a duration a convention states in seconds maps its histogram's input:

```ts
const requestDuration = (boundaries: readonly number[]) => Metric.histogram('http.server.request.duration', { boundaries }).pipe(Metric.mapInput(Duration.toSeconds));
```
