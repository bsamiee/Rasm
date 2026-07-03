# [API_CATALOGUE] @opentelemetry/resources

`@opentelemetry/resources` supplies the `Resource` interface (the service-identity attribute carrier every SDK signal provider stamps onto its exported spans/metrics/logs), the factory functions (`resourceFromAttributes`, `emptyResource`, `defaultResource`, `resourceFromDetectedResource`), the built-in detectors (`envDetector`, `hostDetector`, `osDetector`, `processDetector`, `serviceInstanceIdDetector`), and `detectResources` for folding environment/host/process attributes into one merged resource. In the pinned WebSdk lane this package is a TRANSITIVE substrate under `@effect/opentelemetry` `Resource`: the design page passes a plain `{ serviceName, attributes }` object to `WebSdk.Configuration.resource` (or the `Resource.layer({ serviceName, ... })` config), and the `@effect/opentelemetry` `Resource` Tag — typed `Context.Tag<Resource, Resources.Resource>` — builds the concrete `Resources.Resource` internally by calling `resourceFromAttributes`. The DIRECT surface (`detectResources` + detectors) is reached only to fold env/host/process detection into that identity or to build a raw `MeterProvider`/`WebTracerProvider` outside the effect layer.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/resources`
- package: `@opentelemetry/resources`
- version: `2.8.0` (central pin `pnpm-workspace.yaml`; matches `@effect/opentelemetry@0.63.0` peer resolution)
- license: `Apache-2.0`
- api-peer: `@opentelemetry/api ^1.9.x` — `Attributes`/`AttributeValue`
- module: `@opentelemetry/resources` (barrel `build/src/index.d.ts`; `browser`-conditional detector platform swap)
- runtime: dual — browser + node; `hostDetector`/`osDetector`/`processDetector` are node-effective, `envDetector`/`serviceInstanceIdDetector` are browser-safe
- asset: runtime library — side-effects-free (`sideEffects: false`), tree-shakeable
- rail: resource
- collapse-fence: `[R3]`-SURVIVOR — the `Resource`-identity substrate `@effect/opentelemetry` `Resource.layer` lowers `{ serviceName, attributes }` to via `resourceFromAttributes` in BOTH the native and SDK-bridge lanes (a transitive dep of `@effect/opentelemetry` itself, never directly admitted), so it PERSISTS when the native `Otlp` lane retires the sdk-*/exporter machinery (`libs/typescript/.api/effect-opentelemetry.md`); fenced to `scope:telemetry`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: resource and detection family
- rail: resource
- `Resource` is NOT user-implementable (the interface doc explicitly forbids it); it is the same interface `sdk-metrics` `MeterProviderOptions.resource`/`ResourceMetrics.resource` and `sdk-trace-base` `TracerConfig.resource` consume — one cross-package identity type. `ResourceOptions` is barrel-private (defined in `types.d.ts`, surfaced only as the `options?` param of the factories).

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY] | [CONSUMER / BOUNDARY]                                          |
| :-----: | :--------------------------- | :------------ | :------------------------------------------------------------ |
|  [01]   | `Resource`                   | interface     | `attributes`/`schemaUrl?`/`asyncAttributesPending?`/`merge`/`waitForAsyncAttributes?`/`getRawAttributes` |
|  [02]   | `ResourceDetector`           | interface     | `detect(config?: ResourceDetectionConfig) => DetectedResource` |
|  [03]   | `DetectedResource`           | type          | `{ attributes?: DetectedResourceAttributes }`                 |
|  [04]   | `DetectedResourceAttributes` | type          | `Record<string, MaybePromise<AttributeValue \| undefined>>`   |
|  [05]   | `RawResourceAttribute`       | type          | `[string, MaybePromise<AttributeValue \| undefined>]`         |
|  [06]   | `MaybePromise<T>`            | type          | `T \| Promise<T>` — the async-attribute lift                  |
|  [07]   | `ResourceDetectionConfig`    | interface     | `{ detectors?: Array<ResourceDetector> }`                     |
|  [08]   | `ResourceOptions`            | type (private)| `{ schemaUrl?: string }` — the factory `options?` param shape |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: resource factory functions
- rail: resource

| [INDEX] | [SURFACE]                                                       | [ENTRY_FAMILY] | [CONSUMER / BOUNDARY]                                     |
| :-----: | :-------------------------------------------------------------- | :------------- | :------------------------------------------------------- |
|  [01]   | `resourceFromAttributes(attributes: DetectedResourceAttributes, options?: ResourceOptions): Resource` | factory | resource from sync-or-async attribute map (effect `Resource.layer` calls this) |
|  [02]   | `resourceFromDetectedResource(detected: DetectedResource, options?): Resource` | factory | resource from a detector result                          |
|  [03]   | `emptyResource(): Resource`                                     | factory        | attribute-free resource — test/merge base                |
|  [04]   | `defaultResource(): Resource`                                  | factory        | SDK telemetry-identity baseline (`telemetry.sdk.*`)      |
|  [05]   | `detectResources(config?: ResourceDetectionConfig): Resource`  | detection      | run detectors, merge in array order, last wins on key    |
|  [06]   | `defaultServiceName(): string`                                 | utility        | `"unknown_service[:argv0]"` fallback name                |

[ENTRYPOINT_SCOPE]: built-in detectors
- rail: resource

| [INDEX] | [SURFACE]                   | [ENTRY_FAMILY] | [CONSUMER / BOUNDARY]                                   |
| :-----: | :-------------------------- | :------------- | :----------------------------------------------------- |
|  [01]   | `envDetector`               | detector       | `OTEL_RESOURCE_ATTRIBUTES` + `OTEL_SERVICE_NAME` (browser-safe) |
|  [02]   | `hostDetector`              | detector       | `host.name`/`host.arch` (node)                         |
|  [03]   | `osDetector`                | detector       | `os.type`/`os.version` (node)                          |
|  [04]   | `processDetector`           | detector       | `process.*` attributes (node)                          |
|  [05]   | `serviceInstanceIdDetector` | detector       | random `service.instance.id` UUID (browser-safe)       |

[ENTRYPOINT_SCOPE]: Resource instance operations
- rail: resource

| [INDEX] | [SURFACE]                             | [ENTRY_FAMILY] | [CONSUMER / BOUNDARY]                                       |
| :-----: | :------------------------------------ | :------------- | :--------------------------------------------------------- |
|  [01]   | `resource.merge(other: Resource \| null): Resource` | merge | new resource; `other` wins on collision; `null` copies `this` |
|  [02]   | `resource.attributes`                 | property       | resolved `Attributes` map                                  |
|  [03]   | `resource.schemaUrl?`                 | property       | optional schema URL                                        |
|  [04]   | `resource.asyncAttributesPending?`    | property       | `true` while async attributes still resolving (optional)   |
|  [05]   | `resource.waitForAsyncAttributes?()`  | async settle   | resolves when async attribute promises done (never rejects) |
|  [06]   | `resource.getRawAttributes()`         | raw access     | `RawResourceAttribute[]` incl. unresolved promises         |

## [04]-[IMPLEMENTATION_LAW]

[RESOURCE_TOPOLOGY]:
- `Resource` is an interface, NOT a class; valid instances come only from `resourceFromAttributes`/`emptyResource`/`defaultResource`/`detectResources` — the interface doc explicitly prohibits user implementations
- async attributes (`MaybePromise<AttributeValue | undefined>`) are supported at construction; `asyncAttributesPending` and `waitForAsyncAttributes` are OPTIONAL members — an exporter that must block on final identity awaits `waitForAsyncAttributes` (which never rejects) guarded by `asyncAttributesPending`
- `resource.merge(other)` returns a NEW resource; `other` attributes override `this` on collision; a `null` other returns a copy of `this`
- `detectResources` runs `config.detectors` (or defaults) and merges in array order, last detector winning on key collision; `detect()` returns a `DetectedResource` synchronously, individual attribute VALUES may be promises resolved at settle time

[INTEGRATION_LAW]:
- Stack with `@effect/opentelemetry` `Resource` (the pinned WebSdk path): `WebSdk.Configuration.resource` is a PLAIN `{ serviceName: string; serviceVersion?: string; attributes?: Attributes }` object, NOT a `Resource`; `Resource.layer({ serviceName, ... })` and `Resource.configToAttributes(...)` build the `Resources.Resource` internally via `resourceFromAttributes`, and the `Resource.Resource` Tag is `Context.Tag<Resource, Resources.Resource>` — so the design page derives identity from `AppIdentity`, passes `{ serviceName, attributes }`, and never calls a `resources` factory directly in this lane
- Stack with the detectors: reach the DIRECT surface only to fold environment/host/process attributes in — `Resource.layerFromEnv(additional?)` IS the `envDetector` behavior (`OTEL_RESOURCE_ATTRIBUTES` comma-separated `key=value` + `OTEL_SERVICE_NAME`); to add host/process attributes, `detectResources({ detectors: [envDetector, hostDetector, processDetector] })` then merge onto the identity resource
- Stack across signal providers: the SAME `Resource` interface flows to `sdk-metrics` `MeterProviderOptions.resource`, `sdk-trace-base` `TracerConfig.resource`, and back out as `ResourceMetrics.resource` on every collection — one identity spine, so a per-signal resource fork is structurally a defect
- Stack with `semantic-conventions`: resource attribute KEYS are `ATTR_SERVICE_NAME`/`ATTR_SERVICE_VERSION`/`ATTR_SERVICE_NAMESPACE`/`ATTR_SERVICE_INSTANCE_ID` from `@opentelemetry/semantic-conventions`, never hardcoded `"service.name"` strings

[LOCAL_ADMISSION]:
- in the WebSdk lane the resource is expressed as the `{ serviceName, attributes }` config `@effect/opentelemetry` consumes; `resourceFromAttributes` is the factory that config lowers to, `detectResources`/detectors are the opt-in env/host enrichment, `emptyResource` is the merge/test base, `defaultResource` is the SDK identity baseline
- `envDetector` first in the `detectors` array lets `OTEL_*` env override programmatic attributes; keep it the head of the merge order
- `@opentelemetry/resources` is admitted ONLY inside `scope:telemetry`; no folder constructs a `Resource` outside the telemetry export edge

[RAIL_LAW]:
- Package: `@opentelemetry/resources`
- Owns: resource construction, the detector protocol, async attribute settlement, and the cross-provider `Resource` identity interface
- Accept: the `{ serviceName, attributes }` config lowered through `@effect/opentelemetry` `Resource.layer` in the WebSdk lane; `detectResources` + `envDetector`/`hostDetector`/`processDetector` for env/host enrichment; `merge` for composition
- Reject: direct `Resource` interface implementations; hand-rolled attribute merging outside `merge`; passing a constructed `Resource` where `WebSdk.Configuration.resource`/`Resource.layer` expect the plain `{ serviceName, ... }` config; a per-signal resource fork
