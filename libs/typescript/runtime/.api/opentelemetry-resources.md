# [@opentelemetry/resources] — the concrete `Resource` value the `@effect/opentelemetry` `Resource` Tag carries, and the detector family behind the `AppIdentity`-derived resource

`@opentelemetry/resources` owns the OTLP `Resource` — the immutable, `merge`-composable attribute bundle (`service.name`, `telemetry.sdk.*`, host/os/process facts) stamped on every span, metric, and log so a backend can attribute a signal to its emitter. Its value is the exact type the facade's `Resource.Resource` Tag wraps (`@effect/opentelemetry` declares `Tag<Resource, Resources.Resource>` — this package IS the concrete carrier), so the one Rasm identity spine flows `AppIdentity → Resource.layer → resourceFromAttributes → Resources.Resource → both export lanes`. Its second capability is the `ResourceDetector` family (`env`/`host`/`os`/`process`/`serviceInstanceId`) that `detectResources` folds onto that base to enrich the resource with environment facts the app root does not carry by hand. Inside Rasm it is one row of the `[OTLP_SDK]` SDK-bridge pin block; the edge ledger fences `@opentelemetry/*` to `scope:runtime`, and it is an `[R3]`-collapse member (the native `Otlp` lane's `OtlpResource` replaces it once parity closes; `semantic-conventions` survives, this does not).

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/resources`
- package: `@opentelemetry/resources`
- version: `2.8.0`
- license: `Apache-2.0`
- otel-peer: `@opentelemetry/api >=1.3.0 <1.10.0` (the `Attributes`/`AttributeValue` source); deps `@opentelemetry/core 2.8.0` (`SDK_INFO`/env readers seed `defaultResource`) + `@opentelemetry/semantic-conventions ^1.29.0` (the attribute-key vocabulary)
- consumed-by: `otel/emit` resource composition; the facade's `Resource` module (`@effect/opentelemetry` `Resource.Resource` = `Tag<_, Resources.Resource>`); every `@opentelemetry/sdk-*` provider requires a `Resource`
- catalog-verdict: KEEP as SDK-bridge peer; edge-ledger fences `@opentelemetry/*` to `scope:runtime`; `[R3]`-collapse member (native `OtlpResource` supersedes)
- runtime: dual — one index over a `detectors/platform/{node,browser}` split; node detectors read `os`/`process`/machine-id, browser detectors degrade to `noop`; the `Resource` value + constructors are runtime-neutral
- module-families: the `Resource` value (`merge`, async-attributes), the `resourceFromAttributes`/`defaultResource`/`emptyResource` constructors, and the `ResourceDetector` family run by `detectResources`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the `Resource` value + detector contract
- rail: observability/resource
- `Resource` is an immutable attribute bundle with a `merge` monoid (`AppIdentity` base ⊕ detector output ⊕ env) and an async-attribute channel: `asyncAttributesPending` flags detectors that resolve a `Promise` (machine-id, service-instance-id), and `waitForAsyncAttributes()` is the barrier the SDK awaits before first export. Resource sources are ONE parameterized family — every enricher implements `ResourceDetector.detect(config?): DetectedResource`, so a new source is a detector row in the `detectResources` set, never a new constructor.

| [INDEX] | [SYMBOL]                                                        | [TYPE_FAMILY]     | [CONSUMER / BOUNDARY]                                              |
| :-----: | :-------------------------------------------------------------- | :---------------- | :---------------------------------------------------------------- |
|  [01]   | `Resource { attributes: Attributes; schemaUrl?; asyncAttributesPending? }` | resource value | the identity bundle both export lanes stamp on every signal |
|  [02]   | `Resource.merge(other: Resource \| null): Resource`             | monoid            | fold `AppIdentity` base ⊕ detector ⊕ env into one resource        |
|  [03]   | `Resource.waitForAsyncAttributes?(): Promise<void>` / `getRawAttributes(): RawResourceAttribute[]` | async barrier | await pending detectors; read unresolved `[key, MaybePromise]` pairs |
|  [04]   | `ResourceDetector { detect(config?): DetectedResource }`        | detector contract | the one enricher interface `detectResources` folds               |
|  [05]   | `DetectedResource { attributes?: DetectedResourceAttributes }` / `RawResourceAttribute = [string, MaybePromise<AttributeValue \| undefined>]` | detector output | sync-or-async attribute map a detector returns |
|  [06]   | `ResourceDetectionConfig { detectors?: ResourceDetector[] }` / `MaybePromise<T> = T \| Promise<T>` | run config | the ordered detector set + the async-attribute value shape |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: resource construction
- rail: observability/resource
- The `AppIdentity`-derived resource is `defaultResource()` (which seeds `service.name` + core's `SDK_INFO` `telemetry.sdk.*`) merged with `resourceFromAttributes(AppIdentity attributes)`. `emptyResource()` is the merge identity element. The facade's `Resource.layer({ serviceName, serviceVersion, attributes })` sits directly on top of `resourceFromAttributes`, so design code composes the facade layer and this constructor is the concrete builder it delegates to.

| [INDEX] | [SURFACE]                                                                     | [ENTRY_FAMILY] | [CONSUMER / BOUNDARY]                                          |
| :-----: | :--------------------------------------------------------------------------- | :------------- | :------------------------------------------------------------ |
|  [01]   | `resourceFromAttributes(attributes, options?: { schemaUrl? }): Resource`      | constructor    | the `AppIdentity`-attribute resource behind `Resource.layer`  |
|  [02]   | `defaultResource(): Resource`                                                 | constructor    | `service.name` + `telemetry.sdk.*` seed; the merge base        |
|  [03]   | `emptyResource(): Resource`                                                   | identity       | the `merge` identity element for optional-resource folds       |
|  [04]   | `defaultServiceName(): string`                                               | default        | fallback `service.name` when `AppIdentity` omits one           |

[ENTRYPOINT_SCOPE]: environment detectors
- rail: observability/resource/detect
- `detectResources({ detectors })` runs the detector family and returns a `Resource` whose async attributes resolve lazily; it is merged onto the `AppIdentity` base. `envDetector` ingests `OTEL_RESOURCE_ATTRIBUTES`/`OTEL_SERVICE_NAME` (via core's env readers); the host/os/process/serviceInstanceId detectors add environment facts a multi-tenant deployment attributes on — the node platform reads `os`/`process`, the browser platform degrades these to a `noop` detector.

| [INDEX] | [SURFACE]                                                        | [ENTRY_FAMILY] | [CONSUMER / BOUNDARY]                                          |
| :-----: | :-------------------------------------------------------------- | :------------- | :------------------------------------------------------------ |
|  [01]   | `detectResources(config?: ResourceDetectionConfig): Resource`   | detector fold  | run an ordered `ResourceDetector[]` into one merged resource  |
|  [02]   | `envDetector`                                                   | detector       | `OTEL_RESOURCE_ATTRIBUTES` / `OTEL_SERVICE_NAME` ingestion    |
|  [03]   | `hostDetector` / `osDetector`                                   | detector       | `host.*` / `os.*` attributes (node; browser → noop)           |
|  [04]   | `processDetector` / `serviceInstanceIdDetector`                 | detector       | `process.*` / async `service.instance.id` (node; browser → noop) |

## [04]-[IMPLEMENTATION_LAW]

[SDK_BRIDGE_TOPOLOGY]:
- one resource, one identity: both export lanes consume ONE `Resource` derived from `AppIdentity` — the same value `browser` boot and the `store` `StoreHandle` scope use — so a per-app telemetry fork is structurally impossible (`core/observe/board` dashboards are `AppIdentity → DashboardModel` total functions). This package is the concrete carrier of that one value.
- `merge` is the composition law, not construction: the `AppIdentity` base, the detector output, and the `OTEL_RESOURCE_ATTRIBUTES` env are three `Resource` values folded by `merge` — never three constructors or a mutable attribute map.

[INTEGRATION_LAW]:
- Stack with `.api/effect-opentelemetry.md` `Resource` (the primary seam): the facade's `Resource.Resource` Tag is `Tag<_, Resources.Resource>` — it literally carries this package's `Resource`. `Resource.layer({ serviceName, serviceVersion, attributes })` and `configToAttributes` build the attributes; `resourceFromAttributes`/`defaultResource` are the concrete constructors underneath. Design code composes `Resource.layer` fed the `AppIdentity` value; both `NodeSdk`/`WebSdk` (`Configuration.resource`) and the native `Otlp` lane require that one Tag.
- Stack with the sibling SDK providers: `opentelemetry-sdk-trace-node` (`NodeTracerProvider`), `sdk-metrics` (`MeterProvider`), and `sdk-logs` (`LoggerProvider`) each take a `Resource` at construction; the facade's `NodeSdk`/`WebSdk` wire this one resource into all three so trace/metric/log carry identical identity.
- Stack with `.api/opentelemetry-core.md`: `defaultResource()` merges core's `SDK_INFO` (`telemetry.sdk.name/language/version`); the `envDetector` reads `OTEL_RESOURCE_ATTRIBUTES` through core's `getStringListFromEnv`. The async-attribute barrier (`waitForAsyncAttributes`) gates first export until the `serviceInstanceIdDetector` promise resolves.
- Stack with `kernel/identity` `AppIdentity`: the resource attributes ARE the `AppIdentity` projection; the egress-redaction policy rows on `otel/emit` scrub PII from attributes at the export boundary before serialization, so a detector-added host fact never leaks a hostname into a shared backend unfiltered.

[LOCAL_ADMISSION]:
- `@opentelemetry/*` is admitted ONLY inside `scope:runtime` (edge-ledger ban); no other folder constructs a `Resource`. Instrumentation code never imports this package — it emits through Effect's native signals against the one facade `Resource`.
- prefer the facade `Resource.layer` over raw `resourceFromAttributes`; reach for `detectResources` + the detector family only when SDK-only environment attributes (host/os/process) are required, and record it as an `[R3]` non-collapsed dependency.
- `resourceFromDetectedResource` and `noopDetector` exist in source but are NOT on the package index — do not transcribe them; the public detector set is the five named rows.

[RAIL_LAW]:
- Package: `@opentelemetry/resources`
- Owns: the immutable `merge`-composable `Resource` value (the concrete carrier of the facade `Resource` Tag), the `resourceFromAttributes`/`defaultResource`/`emptyResource` constructors, and the `env`/`host`/`os`/`process`/`serviceInstanceId` `ResourceDetector` family run by `detectResources`
- Accept: one `AppIdentity`-derived `Resource` via the facade `Resource.layer`; `merge` as the composition law folding base ⊕ detectors ⊕ env; `detectResources({ detectors })` for SDK-only environment enrichment; the async-attribute barrier before first export
- Reject: `@opentelemetry/*` imports outside `scope:runtime`, per-app resource forks (dashboards are identity-derived data), a new constructor where a `ResourceDetector` row belongs, mutable attribute accumulation instead of `merge`, transcribing the non-indexed `resourceFromDetectedResource`/`noopDetector`
