# [TS_RUNTIME_API_OPENTELEMETRY_INSTRUMENTATION]

`registerInstrumentations` binds a row set to explicit providers and answers the unload thunk its registration bracket releases; `InstrumentationBase` is the class every instrumentation row extends. Registration installs no global provider, so an omitted `tracerProvider` falls to the no-op api global and every span dies — the browser boot's `Instrument` node passes the web lane's tracer provider explicitly.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/instrumentation`
- package: `@opentelemetry/instrumentation` (Apache-2.0)
- base: peers on `@opentelemetry/api` (+ `@opentelemetry/api-logs` for the logger-provider slot)
- consumed-by: the browser boot's `Instrument` bracket; every admitted instrumentation row extends its base class
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
|  [02]   | `tracerProvider` option                         | provider bind  | the web lane's exposed `OtelTracerProvider` — never omitted |
|  [03]   | `isWrapped` / `safeExecuteInTheMiddle`          | author util    | row-author interior; no Rasm call site                      |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- explicit providers only — an omitted `tracerProvider` falls to the api global the facade never registers, so spans record nowhere; the `Instrument` bracket passes the web lane's provider Tag.
- unload thunk and zone-manager `disable` pair inside one `acquireRelease`, so a torn-down browser graph leaves no patched global behind.

[STACKING]:
- `opentelemetry-instrumentation-{fetch,document-load,user-interaction}`(`.api/opentelemetry-instrumentation-fetch.md`): the three rows are `Instrumentation` values in one registered array, each extending `InstrumentationBase`; construction policy lives on each row's config, activation here.
- `@effect/opentelemetry`(`.api/effect-opentelemetry.md`): `WebSdk.layerTracerProvider` exposes the `Tracer.OtelTracerProvider` Tag the `tracerProvider` option binds — the facade-leg assembly makes the Tag reachable.
- `@opentelemetry/context-zone`(`.api/opentelemetry-context-zone.md`): `ZoneContextManager.enable()` installs in the same bracket, so interaction spans parent the fetches they trigger.
- `browser/boot`: composes the one `registerInstrumentations` call at the `Instrument` node and hands its unload thunk to the graph teardown.

[LOCAL_ADMISSION]:
- `scope:runtime`; the activation call lives only in the browser boot graph's `Instrument` node.

[RAIL_LAW]:
- Package: `@opentelemetry/instrumentation`
- Owns: instrumentation-row activation, the base class the rows extend, and the semconv opt-in gate
- Accept: one `registerInstrumentations` call per browser graph with explicit `tracerProvider`, unload thunk released on scope close
- Reject: activation without an explicit provider, per-library registration, authoring node require-patch definitions in Rasm code
