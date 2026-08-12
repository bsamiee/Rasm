# [TS_RUNTIME_API_OPENTELEMETRY_OTLP_EXPORTER_BASE]

`@opentelemetry/otlp-exporter-base` owns the configuration substrate every OTLP exporter shares: the transport option records the six `exporter-{trace,metrics,logs}-otlp-{http,proto}` classes extend, the `CompressionAlgorithm` vocabulary their `compression` field takes, the export-delegate and transport contracts, and the retry and bounded-queue machinery behind each `export` call. Every signal exporter's constructor argument is one of this package's records, so a shared transport row lands once and every signal inherits it.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/otlp-exporter-base`
- package: `@opentelemetry/otlp-exporter-base` (Apache-2.0)
- module: dual CJS + ESM with a node-http platform split (`index-node-http`); `@opentelemetry/api` `^1.3.0` is the one peer
- runtime: runtime-neutral base with node-only fields (`compression`, `keepAlive`, `httpAgentOptions`, `userAgent`) on the node config record
- rail: observability/export transport

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the exporter configuration records and the transport contracts

| [INDEX] | [SYMBOL]                                           | [TYPE_FAMILY] | [CAPABILITY]                                          |
| :-----: | :------------------------------------------------- | :------------ | :---------------------------------------------------- |
|  [01]   | `CompressionAlgorithm`                             | enum          | `NONE = "none"` \| `GZIP = "gzip"`                    |
|  [02]   | `OTLPExporterConfigBase`                           | interface     | signal-neutral transport options                      |
|  [03]   | `OTLPExporterNodeConfigBase`                       | interface     | node record adding keep-alive, agent, compression     |
|  [04]   | `OtlpSharedConfiguration`                          | interface     | resolved timeout, concurrency, and compression triple |
|  [05]   | `IExporterTransport` / `IOtlpExportDelegate`       | interface     | send and delegate contracts behind every exporter     |
|  [06]   | `ExportResponse` (`Success`/`Failure`/`Retryable`) | union         | transport verdict retry policy dispatches on          |
|  [07]   | `OTLPExporterError`                                | class         | transport error carrying status and data              |

- `OTLPExporterConfigBase` carries `headers?` (`Record<string,string>` or an async `HeadersFactory`), `url?`, `concurrencyLimit?`, `timeoutMillis?` (default 10000), and `selfObsMeterProvider?` (`@experimental`).
- `OTLPExporterNodeConfigBase` extends it with `keepAlive?`, `compression?: CompressionAlgorithm`, `httpAgentOptions?` (`http.AgentOptions`, `https.AgentOptions`, or an async factory), and `userAgent?` prepended to the exporter's own value.
- `OtlpSharedConfiguration` is the merged result the exporter runs on, so every option resolves to those three transport facts beside signal-specific rows.
- `HeadersFactory` resolves per export, so a rotating bearer never bakes into construction; a factory must not throw, and it must not statically import `http`/`https` ahead of the http instrumentation's patch.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: configuration merge and delegate construction — the surface exporters build on

| [INDEX] | [SURFACE]                                                            | [SHAPE]  | [CAPABILITY]                                 |
| :-----: | :------------------------------------------------------------------- | :------- | :------------------------------------------- |
|  [01]   | `mergeOtlpSharedConfigurationWithDefaults(user, fallback, defaults)` | function | fold user options over env and spec defaults |
|  [02]   | `getSharedConfigurationDefaults()`                                   | function | spec transport defaults                      |
|  [03]   | `createOtlpNetworkExportDelegate(config, serializer, metrics, transport)` | factory | 4-arg delegate an exporter's `export` drives — the third slot takes the `ExporterMetrics` self-observation handle |
|  [04]   | `OTLPExporterBase`                                                   | class    | base class every signal exporter extends     |
|  [05]   | `ExporterMetrics`                                                    | class    | exporter self-observability counters         |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- one config record per signal exporter — each signal package's `./platform` node class takes `OTLPExporterNodeConfigBase` widened by that signal's own fields, so compression, timeout, concurrency, keep-alive, and headers are one shared row spelled once per signal.
- browser platform classes take `OTLPExporterConfigBase` instead, so `compression`, `keepAlive`, `httpAgentOptions`, and `userAgent` are sender columns the browser build neither accepts nor honors — its `fetch`/`XHR` transport compresses nothing and holds no socket. Serving both runtimes off one transport projection folds those columns in per sender rather than passing them universally; the `types` entry resolves the node class, so the browser divergence is a runtime fact the type check never reports.
- `CompressionAlgorithm` lives here and nowhere else — no signal exporter package re-exports it, so a composition setting gzip imports the enum from this package.
- `CompressionAlgorithm` is an enum, not a string union, so `"gzip"` is not assignable and the member is the only spelling.

[STACKING]:
- `opentelemetry-exporter-trace-otlp-http.md` / `-metrics-` / `-logs-` and their `-proto` twins: every constructor argument extends `OTLPExporterNodeConfigBase`, so one transport projection feeds all six classes.
- `opentelemetry-sdk-trace-base.md` `BatchSpanProcessor` and `opentelemetry-sdk-logs.md` `BatchLogRecordProcessor`: `exportTimeoutMillis` bounds the processor's export call while `timeoutMillis` here bounds the transport's own request, so the two are distinct budgets and the processor's is the outer one.
- `effect-opentelemetry.md` `Otlp.layer*`: the native lane carries none of this record — its option bag has no compression, timeout, or concurrency key — which is why an estate compression pin selects an SDK-bridge lane.
- `otel/emit` `[06]-[LANES]`: one `_transport` projection builds this record per signal from the export policy and one `_SENDER` row folds the node-only columns in, so no exporter construction spells a transport literal and no browser row carries a field its build drops.

[LOCAL_ADMISSION]:
- `scope:runtime`, the `otel/` folder alone — every other folder emits through Effect's own signal surfaces and never constructs an exporter.
- construction stays at the composition-root Layer; a per-call exporter mint re-opens a connection pool the keep-alive row exists to hold.

[RAIL_LAW]:
- Package: `@opentelemetry/otlp-exporter-base`
- Owns: the OTLP exporter configuration records, the compression vocabulary, the transport and delegate contracts, and the retry and bounded-queue machinery
- Accept: one shared transport projection feeding every signal exporter with timeout, concurrency, and headers from policy, and the node-only compression and keep-alive columns folded in per sender
- Reject: a raw `"gzip"` string, a per-signal transport divergence, node sender columns passed on a browser row, a per-call exporter construction, a header factory that throws or statically imports `http`
