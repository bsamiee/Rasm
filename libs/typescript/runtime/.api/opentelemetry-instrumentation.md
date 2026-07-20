# [TS_RUNTIME_API_OPENTELEMETRY_INSTRUMENTATION]

`@opentelemetry/instrumentation` is the activation substrate every instrumentation row extends: `registerInstrumentations` binds a row set to explicit providers and answers the unload thunk the registration bracket releases. Rasm consumes it in the browser boot's `Instrument` node — the fetch, document-load, and user-interaction rows extend its `InstrumentationBase`, and the facade registers no global provider, so activation passes the web lane's tracer provider explicitly or every instrumentation span dies on the no-op global.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/instrumentation`
- package: `@opentelemetry/instrumentation`
- license: `Apache-2.0`
- base: peers on `@opentelemetry/api` (+ `@opentelemetry/api-logs` for the logger-provider slot)
- consumed-by: the browser composition root's `Instrument` registration bracket; every admitted instrumentation row extends its base class
- runtime: neutral — the node module-patching machinery and the browser global-patching rows both ride this base

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: activation + authoring contracts
- rail: observability/rum

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
- rail: observability/rum
- One call activates the whole row set; the returned thunk is the only deactivation path, so registration is bracket-shaped by construction.
- call: `registerInstrumentations({ instrumentations?, tracerProvider?, meterProvider?, loggerProvider? }): () => void` — the `AutoLoaderOptions` record

| [INDEX] | [SURFACE]                                       | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                                         |
| :-----: | :---------------------------------------------- | :------------- | :---------------------------------------------------------- |
|  [01]   | `registerInstrumentations(options): () => void` | activation     | one call inside the `Instrument` bracket; thunk on release  |
|  [02]   | `tracerProvider` option                         | provider bind  | the web lane's exposed `OtelTracerProvider` — never omitted |
|  [03]   | `isWrapped` / `safeExecuteInTheMiddle`          | author util    | row-author interior; no Rasm call site                      |

## [04]-[IMPLEMENTATION_LAW]

[ACTIVATION_TOPOLOGY]:
- explicit providers only — an omitted `tracerProvider` falls back to the api global, which the facade never registers, so the spans record nowhere; the `Instrument` bracket passes the web lane's provider Tag value.
- bracket law — the returned unload thunk pairs with the zone manager's `disable` inside one `acquireRelease`, so a torn-down browser graph leaves no patched global behind.

[INTEGRATION_LAW]:
- Stack with `opentelemetry-instrumentation-{fetch,document-load,user-interaction}.md`: the three rows are `Instrumentation` values in one registered array; construction policy lives on each row's config, activation lives here.
- Stack with `.api/../.api/effect-opentelemetry.md`: `Tracer.OtelTracerProvider` is the provider the options bind; the web lane's facade-leg assembly is what makes the Tag reachable.
- Stack with `opentelemetry-context-zone.md`: the zone manager installs globally in the same bracket, so interaction spans parent the fetches they trigger.

[LOCAL_ADMISSION]:
- `scope:runtime`; the activation call lives only in the browser boot graph's `Instrument` node.

[RAIL_LAW]:
- Package: `@opentelemetry/instrumentation`
- Owns: instrumentation-row activation, the base class the rows extend, and the semconv opt-in gate
- Accept: one `registerInstrumentations` call per browser graph with explicit `tracerProvider`, unload thunk released on scope close
- Reject: activation without an explicit provider, per-library registration, authoring node require-patch definitions in Rasm code
