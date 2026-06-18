# [API_CATALOGUE] @opentelemetry/sdk-trace-node

`@opentelemetry/sdk-trace-node` supplies the Node.js-specific `NodeTracerProvider` and re-exports the full `@opentelemetry/sdk-trace-base` surface — span processors, samplers, exporters, and configuration types — as the single import point for configuring OTel tracing in Node.js services.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/sdk-trace-node`
- package: `@opentelemetry/sdk-trace-node`
- module: `@opentelemetry/sdk-trace-node`
- asset: runtime library
- rail: tracing

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: provider and config
- rail: tracing

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [RAIL]                        |
| :-----: | :------------------- | :------------ | :---------------------------- |
|   [1]   | `NodeTracerProvider` | class         | Node.js tracer provider       |
|   [2]   | `NodeTracerConfig`   | type alias    | `TracerConfig` alias for Node |

[PUBLIC_TYPE_SCOPE]: base span processors (re-export)
- rail: tracing

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [RAIL]                       |
| :-----: | :--------------------- | :------------ | :--------------------------- |
|   [1]   | `BasicTracerProvider`  | class         | base provider implementation |
|   [2]   | `BatchSpanProcessor`   | class         | batching span processor      |
|   [3]   | `SimpleSpanProcessor`  | class         | synchronous span processor   |
|   [4]   | `NoopSpanProcessor`    | class         | no-op span processor         |
|   [5]   | `ConsoleSpanExporter`  | class         | console diagnostic exporter  |
|   [6]   | `InMemorySpanExporter` | class         | in-memory test exporter      |

[PUBLIC_TYPE_SCOPE]: samplers (re-export)
- rail: tracing

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY] | [RAIL]                                       |
| :-----: | :------------------------- | :------------ | :------------------------------------------- |
|   [1]   | `AlwaysOnSampler`          | class         | sample all spans                             |
|   [2]   | `AlwaysOffSampler`         | class         | drop all spans                               |
|   [3]   | `ParentBasedSampler`       | class         | parent-context-aware sampler                 |
|   [4]   | `TraceIdRatioBasedSampler` | class         | probabilistic trace-ID sampler               |
|   [5]   | `SamplingDecision`         | enum          | `NOT_RECORD`, `RECORD`, `RECORD_AND_SAMPLED` |

[PUBLIC_TYPE_SCOPE]: configuration types (re-export)
- rail: tracing

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY] | [RAIL]                         |
| :-----: | :-------------------------------- | :------------ | :----------------------------- |
|   [1]   | `TracerConfig`                    | interface     | base tracer provider config    |
|   [2]   | `SpanProcessor`                   | interface     | span processor contract        |
|   [3]   | `SpanExporter`                    | interface     | span exporter contract         |
|   [4]   | `Sampler`                         | interface     | sampler contract               |
|   [5]   | `ReadableSpan`                    | interface     | finished span read model       |
|   [6]   | `SDKRegistrationConfig`           | interface     | global API registration config |
|   [7]   | `BatchSpanProcessorBrowserConfig` | interface     | batch processor browser config |
|   [8]   | `BufferConfig`                    | interface     | buffer/batch size config       |
|   [9]   | `IdGenerator`                     | interface     | trace/span ID generator        |
|  [10]   | `RandomIdGenerator`               | class         | default random ID generator    |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: NodeTracerProvider
- rail: tracing

| [INDEX] | [SURFACE]                         | [ENTRY_FAMILY] | [RAIL]                          |
| :-----: | :-------------------------------- | :------------- | :------------------------------ |
|   [1]   | `new NodeTracerProvider(config?)` | constructor    | creates Node.js tracer provider |
|   [2]   | `provider.register(config?)`      | registration   | registers with global OTel API  |

## [4]-[IMPLEMENTATION_LAW]

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
