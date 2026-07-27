# [TS_RUNTIME_API_OPENTELEMETRY_INSTRUMENTATION]

`registerInstrumentations` binds a row set to explicit providers and answers the unload thunk its registration bracket releases; `InstrumentationBase` is the class every instrumentation row extends. Registration installs no global provider, so each omitted provider slot falls to the no-op api global and that signal dies silently — every condition's `Instrument` node passes its lane's whole exposed provider set.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/instrumentation`
- package: `@opentelemetry/instrumentation` (Apache-2.0)
- base: peers on `@opentelemetry/api` (+ `@opentelemetry/api-logs` for the logger-provider slot)
- consumed-by: both condition `Instrument` brackets — the server node and the browser node; every admitted instrumentation row extends its base class
- runtime: neutral — node module-patching and browser global-patching rows both ride this base
- rail: observability/rum

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: activation + authoring contracts

| [INDEX] | [SYMBOL]                                                | [TYPE_FAMILY]    | [CONSUMER_BOUNDARY]                                    |
| :-----: | :------------------------------------------------------ | :--------------- | :----------------------------------------------------- |
|  [01]   | `Instrumentation` / `InstrumentationConfig`             | row contract     | the element type of the registered instrumentation set |
|  [02]   | `InstrumentationBase`                                   | base class       | every admitted instrumentation row extends it          |
|  [03]   | `AutoLoaderOptions` / `AutoLoaderResult`                | options shape    | the four-slot options record; providers explicit       |
|  [04]   | `InstrumentationNodeModuleDefinition` / `...ModuleFile` | node patch shape | require-hook authoring; no Rasm row authors one        |
|  [05]   | `SpanCustomizationHook` / `ShimWrapped`                 | hook shape       | row-author surface; consumed through each row's config |
|  [06]   | `SemconvStability` / `semconvStabilityFromStr`          | semconv gate     | the `semconvStabilityOptIn` stable-row gate            |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: activation

| [INDEX] | [SURFACE]                                       | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                                         |
| :-----: | :---------------------------------------------- | :------------- | :---------------------------------------------------------- |
|  [01]   | `registerInstrumentations(options): () => void` | activation     | one call inside the `Instrument` bracket; thunk on release  |
|  [02]   | `tracerProvider`/`meterProvider`/`loggerProvider` | provider bind  | the lane's three exposed provider Tags — none omitted       |
|  [03]   | `isWrapped` / `safeExecuteInTheMiddle`          | author util    | row-author interior; no Rasm call site                      |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- explicit providers only — `AutoLoaderOptions` carries `instrumentations` beside three provider slots, and every omitted slot falls to the api global the facade never registers: spans record nowhere without `tracerProvider`, series drop without `meterProvider`, and a row's log records vanish without `loggerProvider`. Binding two of three leaves the third signal silently dead, so the registration bracket passes the lane's whole exposed provider set.
- unload thunk and zone-manager `disable` pair inside one `acquireRelease`, so a torn-down browser graph leaves no patched global behind.

[STACKING]:
- `opentelemetry-instrumentation-{fetch,document-load,user-interaction,xml-http-request}`(`.api/opentelemetry-instrumentation-fetch.md`) and `-{http,undici,pg,runtime-node}`(`.api/opentelemetry-instrumentation-http.md`): every row is an `Instrumentation` value in one registered array, each extending `InstrumentationBase`; construction policy lives on each row's config, activation here.
- `@effect/opentelemetry`(`.api/effect-opentelemetry.md`): `NodeSdk.layerTracerProvider`/`WebSdk.layerTracerProvider` expose the `Tracer.OtelTracerProvider` Tag and `Logger.layerLoggerProvider` the `Logger.OtelLoggerProvider` Tag the two provider options bind — the facade-leg assembly makes both Tags reachable, while the aggregate `NodeSdk.layer`/`WebSdk.layer` and a `Layer.provide`-consumed logger provider conceal them.
- `@opentelemetry/context-async-hooks`(`.api/opentelemetry-context-async-hooks.md`) + `-context-zone`(`.api/opentelemetry-context-zone.md`): the condition's manager `.enable()` installs in the same bracket, so a client span parents the request that triggered it across the async chain.
- `otel/server`, `otel/instrument`: each composes exactly one `registerInstrumentations` call at its condition's registration node and hands the unload thunk to the graph teardown.

[LOCAL_ADMISSION]:
- `scope:runtime`; exactly one activation call exists per process condition, at that condition's registration node — the server node and the browser node — and no other folder calls it.

[RAIL_LAW]:
- Package: `@opentelemetry/instrumentation`
- Owns: instrumentation-row activation, the base class the rows extend, and the semconv opt-in gate
- Accept: one `registerInstrumentations` call per process condition with explicit `tracerProvider`, `meterProvider`, and `loggerProvider`, unload thunk released on scope close
- Reject: activation without an explicit provider on every slot, a second activation call inside one condition, authoring node require-patch definitions in Rasm code
