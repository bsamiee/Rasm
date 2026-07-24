# [TS_RUNTIME_API_OPENTELEMETRY_RESOURCES]

`@opentelemetry/resources` owns the OTLP `Resource`: an immutable, `merge`-composable attribute bundle (`service.name`, `telemetry.sdk.*`, host/os/process facts) every span, metric, and log carries so a backend attributes a signal to its emitter, and `detectResources` folds the `ResourceDetector` family onto that base to enrich it with environment facts. It is the concrete value the facade `Resource.Resource` Tag wraps, so one `AppIdentity`-derived resource reaches both export lanes.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/resources`
- package: `@opentelemetry/resources` (Apache-2.0)
- module: ESM, single index; a `detectors/platform/{node,browser}` split resolves per runtime
- runtime: isomorphic — node detectors read `os`/`process`/machine-id, browser detectors degrade to `noop`; the `Resource` value and constructors are runtime-neutral
- depends: `@opentelemetry/api` (`Attributes`/`AttributeValue`), `@opentelemetry/core` (`SDK_INFO`, env readers), `@opentelemetry/semantic-conventions` (attribute-key vocabulary)
- rail: observability/resource — the identity bundle both export lanes stamp on every signal

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the `Resource` value and the detector contract

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY] | [CAPABILITY]                                                                              |
| :-----: | :------------------------ | :------------ | :---------------------------------------------------------------------------------------- |
|  [01]   | `Resource`                | interface     | immutable `attributes`/`schemaUrl?`/`asyncAttributesPending?` bundle both lanes stamp     |
|  [02]   | `ResourceDetector`        | interface     | the one enricher contract `detectResources` folds — `detect(config?) -> DetectedResource` |
|  [03]   | `DetectedResource`        | type          | `{ attributes?: DetectedResourceAttributes }` sync-or-async map a detector returns        |
|  [04]   | `ResourceDetectionConfig` | interface     | `{ detectors?: ResourceDetector[] }` ordered set `detectResources` runs                   |
|  [05]   | `RawResourceAttribute`    | tuple         | `[string, MaybePromise<AttributeValue \| undefined>]` unresolved attribute pair           |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: resource construction and the `Resource` monoid

| [INDEX] | [SURFACE]                                               | [SHAPE]  | [CAPABILITY]                                              |
| :-----: | :------------------------------------------------------ | :------- | :-------------------------------------------------------- |
|  [01]   | `resourceFromAttributes(DetectedResourceAttributes)`    | factory  | the `AppIdentity` resource behind `Resource.layer`        |
|  [02]   | `defaultResource()`                                     | factory  | `service.name` + `telemetry.sdk.*` seed; the `merge` base |
|  [03]   | `emptyResource()`                                       | factory  | the `merge` identity element                              |
|  [04]   | `defaultServiceName() -> string`                        | factory  | recovery `service.name` when `AppIdentity` omits one      |
|  [05]   | `Resource.merge(Resource \| null) -> Resource`          | instance | fold base ⊕ detector ⊕ env; other wins collisions         |
|  [06]   | `Resource.waitForAsyncAttributes() -> Promise<void>`    | instance | await pending async detectors before first export         |
|  [07]   | `Resource.getRawAttributes() -> RawResourceAttribute[]` | instance | unresolved `[key, MaybePromise]` pairs                    |

- `resourceFromAttributes`: `options?` sets `schemaUrl`.

[ENTRYPOINT_SCOPE]: environment detectors — `detectResources` folds an ordered `ResourceDetector[]` onto the `AppIdentity` base

| [INDEX] | [SURFACE]                                               | [SHAPE] | [CAPABILITY]                                                        |
| :-----: | :------------------------------------------------------ | :------ | :------------------------------------------------------------------ |
|  [01]   | `detectResources(ResourceDetectionConfig?) -> Resource` | fold    | the detector fold; async attributes resolve lazily                  |
|  [02]   | `envDetector`                                           | static  | `OTEL_RESOURCE_ATTRIBUTES`/`OTEL_SERVICE_NAME` via core env readers |
|  [03]   | `hostDetector` / `osDetector`                           | static  | `host.*`/`os.*` facts; node reads `os`, browser `noop`              |
|  [04]   | `processDetector` / `serviceInstanceIdDetector`         | static  | `process.*` / async `service.instance.id`                           |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- one resource, one identity: both export lanes consume one `Resource` derived from `AppIdentity` — the value `browser` boot and the `store` `StoreHandle` scope share — so a per-app telemetry fork is structurally impossible, this package being that value's concrete carrier.
- `merge` is the composition law, not construction: the `AppIdentity` base, detector output, and `OTEL_RESOURCE_ATTRIBUTES` env fold as three `Resource` values, never a mutable attribute map.
- resource sources are one parameterized family: every enricher implements `ResourceDetector.detect`, so a new source is a detector row in `detectResources`, never a new constructor.
- `asyncAttributesPending` flags a detector resolving a `Promise` (machine-id, service-instance-id) and `waitForAsyncAttributes` is the barrier the SDK awaits before first export.

[STACKING]:
- `effect-opentelemetry`(`.api/effect-opentelemetry.md`): the facade `Resource.Resource` Tag is `Tag<_, Resources.Resource>` carrying this value; `Resource.layer({ serviceName, serviceVersion, attributes })` builds the attributes over `resourceFromAttributes`, and native `Otlp` with `NodeSdk`/`WebSdk` (`Configuration.resource`) all require that one Tag.
- `opentelemetry-core`(`.api/opentelemetry-core.md`): `defaultResource` merges core's `SDK_INFO` (`telemetry.sdk.name/language/version`); `envDetector` reads `OTEL_RESOURCE_ATTRIBUTES` through core's `getStringListFromEnv`.
- SDK providers `opentelemetry-sdk-trace-node`(`.api/opentelemetry-sdk-trace-node.md`) `NodeTracerProvider`, `opentelemetry-sdk-metrics`(`.api/opentelemetry-sdk-metrics.md`) `MeterProvider`, and `opentelemetry-sdk-logs`(`.api/opentelemetry-sdk-logs.md`) `LoggerProvider` each take a `Resource` at construction, so the facade wires this one value into all three and trace/metric/log carry identical identity.
- `otel/emit` (within-lib): the export-boundary owner feeds `Resource.layer` the `core/value/identity` `AppIdentity` and scrubs PII from attributes through egress-redaction rows before serialization, so a detector-added host fact never leaks a hostname unfiltered.

[LOCAL_ADMISSION]:
- `@opentelemetry/*` admits only inside `scope:runtime` (edge-ledger ban); no other folder constructs a `Resource`, and instrumentation emits through Effect's native signals against the one facade `Resource`.
- design code composes the facade `Resource.layer` over raw `resourceFromAttributes`, reaching for `detectResources` and the detector family only where SDK-only environment attributes (host/os/process) are required.
- it persists as the native lane's `Resource`-identity substrate; `.api/effect-opentelemetry.md` owns the `[OTEL_PIN_BLOCK]` survive-and-collapse roster.

[RAIL_LAW]:
- Package: `@opentelemetry/resources`
- Owns: the immutable `merge`-composable `Resource` value (concrete carrier of the facade `Resource` Tag), the `resourceFromAttributes`/`defaultResource`/`emptyResource` constructors, and the `env`/`host`/`os`/`process`/`serviceInstanceId` `ResourceDetector` family run by `detectResources`
- Accept: one `AppIdentity`-derived `Resource` via the facade `Resource.layer`; `merge` folding base ⊕ detectors ⊕ env; `detectResources({ detectors })` for SDK-only environment enrichment; the async-attribute barrier before first export
- Reject: `@opentelemetry/*` imports outside `scope:runtime`, per-app resource forks, a new constructor where a `ResourceDetector` row belongs, mutable attribute accumulation instead of `merge`
