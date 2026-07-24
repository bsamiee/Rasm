# [TS_RUNTIME_API_OPENTELEMETRY_SDK_TRACE_NODE]

`@opentelemetry/sdk-trace-node` adds one symbol over `sdk-trace-base`: `NodeTracerProvider`, whose `register()` installs the Node async-context spine — `AsyncLocalStorageContextManager` and the W3C composite propagator — so span parenting survives `await`/callback boundaries without manual threading. Its barrel re-exports the whole `sdk-trace-base` roster for one node import site, backs the `@effect/opentelemetry` `NodeSdk` layer, and collapses at `[OTEL_PIN_BLOCK]`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/sdk-trace-node`
- package: `@opentelemetry/sdk-trace-node` (Apache-2.0)
- module: dual — CJS default (`build/src/index.js`) + ESM mirror (`build/esm/index.js`); flat barrel, no `exports` subpath map.
- asset: TSDECL `build/src/index.d.ts`.
- runtime: node/bun only — the async-local-storage context manager binds node; the browser counterpart is `sdk-trace-web` (`WebTracerProvider`, `StackContextManager`).
- peer: `@opentelemetry/api`; deps `@opentelemetry/context-async-hooks` (`AsyncLocalStorageContextManager`), `@opentelemetry/core` (W3C propagators), `@opentelemetry/sdk-trace-base` (re-exported roster).
- plane: `plane:runtime` / `plane:server`, edge-ledger-fenced to `scope:runtime`.
- rail: observability/sdk-bridge; `[OTEL_PIN_BLOCK]` collapse target.

## [02]-[NODE_PROVIDER]

Node behavior lives entirely in `register()`: `NodeTracerConfig` aliases `TracerConfig` with no node axis, and `SDKRegistrationConfig` selects the global context manager and propagator — `null` skips a global install, `undefined` takes the node default.

| [INDEX] | [SYMBOL]                | [KIND]                | [CAPABILITY_BOUNDARY]                                                 |
| :-----: | :---------------------- | :-------------------- | :-------------------------------------------------------------------- |
|  [01]   | `NodeTracerProvider`    | class                 | `extends BasicTracerProvider`; `register()` installs the node globals |
|  [02]   | `NodeTracerConfig`      | type alias            | `= TracerConfig` — no node-specific field                             |
|  [03]   | `SDKRegistrationConfig` | interface (re-export) | `register()` arg — `{ propagator?, contextManager? }`                 |

[NODE_TRACER_PROVIDER]: `NodeTracerProvider(NodeTracerConfig?)` `NodeTracerProvider.register(SDKRegistrationConfig?) -> void`
[SDKREGISTRATION_CONFIG]: `SDKRegistrationConfig.propagator: TextMapPropagator|null` `SDKRegistrationConfig.contextManager: ContextManager|null`

## [03]-[SUPERSET_BARREL]

Barrel re-exports the entire `sdk-trace-base` public surface — samplers, processors, exporters, id generator, every type — so a node consumer reaches the roster and `NodeTracerProvider` from one import; that roster is owned by `opentelemetry-sdk-trace-base.md`, never re-cataloged here.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Node behavior lives entirely in `register()`; `NodeTracerConfig` aliases `TracerConfig` with no node axis.
- `@effect/opentelemetry` consumes the `NodeTracerProvider` constructor and owns global tracer/context wiring — no `.register()`; a pure-SDK path calls `register()` to install the global provider, `AsyncLocalStorageContextManager`, and W3C composite propagator in one call.
- Under effect the `resource` enters through the facade's `AppIdentity`-derived `Resource` layer, never `TracerConfig.resource`.

[STACKING]:
- `@effect/opentelemetry`(`.api/effect-opentelemetry.md`): `NodeSdk.layer(config)` constructs `new NodeTracerProvider({ ...tracerConfig, resource, spanProcessors })` and drives it through the Effect runtime via the `Tracer.OtelTracerProvider` Tag — the consumed surface is the constructor, never `register()`.
- `@opentelemetry/sdk-trace-base`(`.api/opentelemetry-sdk-trace-base.md`): every processor/exporter/sampler passed through this barrel is a base symbol — the production node pipeline is `BatchSpanProcessor(OTLPTraceExporter(opts))` under `{ sampler: ParentBasedSampler({ root: TraceIdRatioBasedSampler(ratio) }) }`.
- runtime split (within-lib): `sdk-trace-node` (`NodeTracerProvider`) vs `sdk-trace-web` (`WebTracerProvider`) is a node/browser lane selection at the composition root, not an instrumentation fork; the native `Otlp` lane is runtime-neutral over whichever `HttpClient` the runtime supplies.

[LOCAL_ADMISSION]:
- `register()` only in a pure-SDK non-Effect path — under the effect facade a `.register()` call double-registers the global context wiring effect already owns. Import only inside `scope:runtime` (edge-ledger); the browser lane is `sdk-trace-web`.

[RAIL_LAW]:
- Package: `@opentelemetry/sdk-trace-node`
- Owns: the node trace provider — `NodeTracerProvider` and its `register()` global-install semantics (async-local-storage context manager + W3C composite propagator) — over the barrel re-exporting the full `sdk-trace-base` roster.
- Accept: `new NodeTracerProvider(tracerConfig)` reached through `@effect/opentelemetry` `NodeSdk` for the node/bun telemetry lane; `register()` only in a pure-SDK path; base processors/exporters/samplers imported from this barrel.
- Reject: `.register()` under the effect facade (double registration); this leg in the browser (`sdk-trace-web` owns it); re-documenting the base roster; import outside `scope:runtime`; treating the node lane as permanent — it collapses at `[OTEL_PIN_BLOCK]`.
