# [API_CATALOGUE] @opentelemetry/sdk-trace-node

`@opentelemetry/sdk-trace-node` supplies the Node.js-specific `NodeTracerProvider` and re-exports the full `@opentelemetry/sdk-trace-base` surface — span processors, samplers, exporters, and configuration types — as the single import point for configuring OTel tracing in Node.js services.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/sdk-trace-node`
- package: `@opentelemetry/sdk-trace-node`
- module: `@opentelemetry/sdk-trace-node`
- asset: runtime library
- rail: tracing

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: provider and config
- rail: tracing

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [RAIL]                        |
| :-----: | :------------------- | :------------ | :---------------------------- |
|  [01]   | `NodeTracerProvider` | class         | Node.js tracer provider       |
|  [02]   | `NodeTracerConfig`   | type alias    | `TracerConfig` alias for Node |

[PUBLIC_TYPE_SCOPE]: base span processors (re-export)
- rail: tracing

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [RAIL]                       |
| :-----: | :--------------------- | :------------ | :--------------------------- |
|  [01]   | `BasicTracerProvider`  | class         | base provider implementation |
|  [02]   | `BatchSpanProcessor`   | class         | batching span processor      |
|  [03]   | `SimpleSpanProcessor`  | class         | synchronous span processor   |
|  [04]   | `NoopSpanProcessor`    | class         | no-op span processor         |
|  [05]   | `ConsoleSpanExporter`  | class         | console diagnostic exporter  |
|  [06]   | `InMemorySpanExporter` | class         | in-memory test exporter      |

[PUBLIC_TYPE_SCOPE]: samplers (re-export)
- rail: tracing

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY] | [RAIL]                                       |
| :-----: | :------------------------- | :------------ | :------------------------------------------- |
|  [01]   | `AlwaysOnSampler`          | class         | sample all spans                             |
|  [02]   | `AlwaysOffSampler`         | class         | drop all spans                               |
|  [03]   | `ParentBasedSampler`       | class         | parent-context-aware sampler                 |
|  [04]   | `TraceIdRatioBasedSampler` | class         | probabilistic trace-ID sampler               |
|  [05]   | `SamplingDecision`         | enum          | `NOT_RECORD`, `RECORD`, `RECORD_AND_SAMPLED` |

[PUBLIC_TYPE_SCOPE]: configuration types (re-export)
- rail: tracing

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY] | [RAIL]                         |
| :-----: | :-------------------------------- | :------------ | :----------------------------- |
|  [01]   | `TracerConfig`                    | interface     | base tracer provider config    |
|  [02]   | `SpanProcessor`                   | interface     | span processor contract        |
|  [03]   | `SpanExporter`                    | interface     | span exporter contract         |
|  [04]   | `Sampler`                         | interface     | sampler contract               |
|  [05]   | `ReadableSpan`                    | interface     | finished span read model       |
|  [06]   | `SDKRegistrationConfig`           | interface     | global API registration config |
|  [07]   | `BatchSpanProcessorBrowserConfig` | interface     | batch processor browser config |
|  [08]   | `BufferConfig`                    | interface     | buffer/batch size config       |
|  [09]   | `IdGenerator`                     | interface     | trace/span ID generator        |
|  [10]   | `RandomIdGenerator`               | class         | default random ID generator    |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: NodeTracerProvider
- rail: tracing

| [INDEX] | [SURFACE]                         | [ENTRY_FAMILY] | [RAIL]                          |
| :-----: | :-------------------------------- | :------------- | :------------------------------ |
|  [01]   | `new NodeTracerProvider(config?)` | constructor    | creates Node.js tracer provider |
|  [02]   | `provider.register(config?)`      | registration   | registers with global OTel API  |

## [04]-[IMPLEMENTATION_LAW]

[TRACE_NODE_TOPOLOGY]:
- namespace: `@opentelemetry/sdk-trace-node`; two own public symbols (`NodeTracerProvider`, `NodeTracerConfig`)
- all `@opentelemetry/sdk-trace-base` exports are re-exported directly from this package's index
- `NodeTracerConfig` is a type alias for `TracerConfig` from `@opentelemetry/sdk-trace-base`; no additional Node-specific fields exist
- `NodeTracerProvider` extends `BasicTracerProvider` and adds Node.js instrumentation loader support via `register()`

[LOCAL_ADMISSION]:
- Import the full trace SDK from `@opentelemetry/sdk-trace-node`; do not import span processors or samplers from `@opentelemetry/sdk-trace-base` directly when this package is in use.
- `register()` installs the provider as the global OTel tracer and configures propagators and context managers; call once at application startup.

[RAIL_LAW]:
- Package: `@opentelemetry/sdk-trace-node`
- Owns: Node.js OTel tracing bootstrap and re-export hub for sdk-trace-base
- Accept: `NodeTracerConfig` / `SDKRegistrationConfig` at construction and registration
- Reject: direct `@opentelemetry/sdk-trace-base` imports alongside this package for the same symbols
