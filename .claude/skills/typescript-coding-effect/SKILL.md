---
name: typescript-coding-effect
description: "Use when calling an Effect module family, covering HTTP and HttpApi, SQL, AI and MCP, RPC and atoms, processes, CLI, crypto, encoding, tests, and telemetry"
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

## [02]-[TESTS]

`@effect/vitest` runs each case as an effect: `it.effect` with `TestClock` and `TestConsole`, `it.live` with the real clock, and `layer(L)(name, (it) => ...)` with one build of `L` for the block:

```ts
layer(Layer.mock(Store, { read: (key) => StoreError.make({ reason: { _tag: 'Unavailable' }, subject: key }) }))('Store', (it) => {
    it.effect('fails with Unavailable once the backoff runs out', () => {
        const base = Duration.millis(100);
        const retries = 3;
        return Effect.all([Effect.flip(retrying('order', base, retries)), TestClock.adjust(Duration.times(base, 2 ** retries - 1))], { concurrency: 'unbounded' }).pipe(Effect.map(([error]) => assert.strictEqual(error.reason._tag, 'Unavailable')));
    });
});
```

`Store` is a service with `read(key)` failing with `StoreError`, and `retrying(key, base, retries)` reads through it under `Schedule.exponential(base)` for `retries` retries. `Layer.mock` implements the members a case reads, and an omitted member dies when called:
- `TestClock` starts at epoch 0, a sleeping fiber resumes after `TestClock.adjust` or `setTime`, and a case runs the effect beside the adjustment
- `it.effect(name, ({ task }) => ...)` receives Vitest's `TestContext`, and `task.name` names what a case writes
- Shared suites take `Vitest.MethodsNonLive<R>`, and test files call `it.effect` per case so lint rules see each assertion
- `layer(L, { buildPerTest: true })` builds `L` per case in a fork of the block's memo map, and `it.layer(nested)` adds services over the parent's
- `it.effect.prop(name, [Schema.Int], fn)` and `it.prop` take schemas or `Arbitrary` values, `Arbitrary.schema(S)` deriving generators from checks
- `assertSome`, `assertNone`, `assertSuccess`, `assertFailure`, and `assertExitSuccess` in `@effect/vitest/utils` read `Option`, `Result`, and `Exit`
- `TestConsole.logLines` returns what a case printed, and `excludeTestServices: true` keeps the live `Clock` and `Console` for a block

## [03]-[TELEMETRY]

Logs, spans, and metrics leave the process through one layer at the root:

```ts
const Telemetry = Otlp.layerFromConfig().pipe(
    Layer.provide([FetchHttpClient.layer, OtlpSerialization.layerProtobuf]),
    Layer.provideMerge(Logger.layer([Logger.consoleJson, Logger.tracerLogger])),
    Layer.provideMerge(Layer.effect(References.MinimumLogLevel, Config.LogLevel('LOG_LEVEL').pipe(Config.withDefault(References.MinimumLogLevel.defaultValue())))),
);
```

`Otlp.layerFromConfig` reads `OTEL_EXPORTER_OTLP_ENDPOINT`, `OTEL_SERVICE_NAME`, `OTEL_RESOURCE_ATTRIBUTES`, and the batch variables, and `Logger.tracerLogger` keeps each log a span event:
- `Logger.layer(loggers)` replaces the current loggers, `{ mergeWithExisting: true }` adds to them
- `References.LogToStderr` set `true` moves console loggers to stderr
- `Effect.annotateLogs` covers one effect, `Effect.annotateLogsScoped` the rest of a scope, and `Effect.withLogSpan(label)` times each line
- Spans take `{ attributes, links, parent, kind, sampled, level }`, and spans under `Tracer.MinimumTraceLevel` drop with their children
- Request spans take `<method> <target>` names over a low-cardinality target (a route, a URI template), each concrete value an attribute
- `HttpTraceContext.fromHeaders(headers)` reads a remote `parent` from the headers a request or protocol message holds
- `Effect.track(metric)`, `trackDuration(timer)`, `trackErrors`, and `trackSuccesses` update a metric per run, `Metric.withAttributes` tagging it
- `OtlpExporter.Flusher` drains logs, metrics, and traces before a short-lived host returns
