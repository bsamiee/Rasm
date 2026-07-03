# [API_CATALOGUE] @opentelemetry/resources

`@opentelemetry/resources` supplies the `Resource` interface, factory functions (`resourceFromAttributes`, `emptyResource`, `defaultResource`), built-in detectors (`envDetector`, `hostDetector`, `osDetector`, `processDetector`, `serviceInstanceIdDetector`), and `detectResources` for composing service-identity and environment attributes into a single merged resource for all SDK signal providers.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/resources`
- package: `@opentelemetry/resources`
- module: `@opentelemetry/resources`
- asset: runtime library
- rail: resource

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: resource and detection family
- rail: resource

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY] | [RAIL]                                                        |
| :-----: | :--------------------------- | :------------ | :------------------------------------------------------------ |
|  [01]   | `Resource`                   | interface     | attributes, schemaUrl, merge, waitForAsyncAttributes          |
|  [02]   | `ResourceDetector`           | interface     | detect(config?) -> DetectedResource                           |
|  [03]   | `DetectedResource`           | type          | { attributes?: DetectedResourceAttributes }                   |
|  [04]   | `DetectedResourceAttributes` | type          | Record\<string, MaybePromise\<AttributeValue \| undefined\>\> |
|  [05]   | `RawResourceAttribute`       | type          | [string, MaybePromise\<AttributeValue \| undefined\>]         |
|  [06]   | `MaybePromise<T>`            | type          | T \| Promise\<T\>                                             |
|  [07]   | `ResourceDetectionConfig`    | interface     | detectors?: Array\<ResourceDetector\>                         |
|  [08]   | `ResourceOptions`            | type          | { schemaUrl?: string }                                        |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: resource factory functions
- rail: resource

| [INDEX] | [SURFACE]                                      | [ENTRY_FAMILY] | [RAIL]                                           |
| :-----: | :--------------------------------------------- | :------------- | :----------------------------------------------- |
|  [01]   | `resourceFromAttributes(attributes, options?)` | factory        | resource from synchronous or async attribute map |
|  [02]   | `emptyResource()`                              | factory        | resource with no attributes                      |
|  [03]   | `defaultResource()`                            | factory        | resource with SDK telemetry identity attributes  |
|  [04]   | `detectResources(config?)`                     | detection      | run all detectors and merge results              |
|  [05]   | `defaultServiceName()`                         | utility        | derive default service name string               |

[ENTRYPOINT_SCOPE]: built-in detectors
- rail: resource

| [INDEX] | [SURFACE]                   | [ENTRY_FAMILY] | [RAIL]                                      |
| :-----: | :-------------------------- | :------------- | :------------------------------------------ |
|  [01]   | `envDetector`               | detector       | reads `OTEL_RESOURCE_ATTRIBUTES` env var    |
|  [02]   | `hostDetector`              | detector       | adds `host.name` and `host.arch` attributes |
|  [03]   | `osDetector`                | detector       | adds `os.type` and `os.version` attributes  |
|  [04]   | `processDetector`           | detector       | adds `process.*` attributes (Node.js)       |
|  [05]   | `serviceInstanceIdDetector` | detector       | adds a random `service.instance.id` UUID    |

[ENTRYPOINT_SCOPE]: Resource instance operations
- rail: resource

| [INDEX] | [SURFACE]                           | [ENTRY_FAMILY] | [RAIL]                                            |
| :-----: | :---------------------------------- | :------------- | :------------------------------------------------ |
|  [01]   | `resource.merge(other)`             | merge          | combine resources; other takes precedence         |
|  [02]   | `resource.attributes`               | property       | resolved `Attributes` map                         |
|  [03]   | `resource.schemaUrl`                | property       | optional schema URL string                        |
|  [04]   | `resource.asyncAttributesPending`   | property       | true when async attributes still resolving        |
|  [05]   | `resource.waitForAsyncAttributes()` | async settle   | resolves when all async attribute promises done   |
|  [06]   | `resource.getRawAttributes()`       | raw access     | `RawResourceAttribute[]` with unresolved promises |

## [04]-[IMPLEMENTATION_LAW]

[RESOURCE_TOPOLOGY]:
- `Resource` is an interface, not a class; valid instances come only from `resourceFromAttributes`, `emptyResource`, `defaultResource`, or `detectResources` — the docs explicitly prohibit user implementations
- Async attributes (`MaybePromise<AttributeValue | undefined>`) are supported at construction; `resource.asyncAttributesPending` guards whether `waitForAsyncAttributes` is needed before export
- `resource.merge(other)` produces a new resource; `other` attributes override `this` attributes on collision; `null` other is accepted and returns a copy of `this`
- `detectResources` runs all detectors in `config.detectors` (or defaults when omitted) and merges their results in array order; last detector wins on key collision

[LOCAL_ADMISSION]:
- `resourceFromAttributes` is the production factory; `emptyResource` is the test-mode factory; `defaultResource` is the SDK-provided identity baseline
- `envDetector` reads `OTEL_RESOURCE_ATTRIBUTES` (comma-separated `key=value`) and `OTEL_SERVICE_NAME` at detection time; use as the first detector in `detectResources` to allow environment override
- Pass the merged `Resource` as `options.resource` to `MeterProvider` and as the `Resource` tag to `@effect/opentelemetry`'s `Resource.layer`

[RAIL_LAW]:
- Package: `@opentelemetry/resources`
- Owns: resource construction, detector protocol, async attribute settlement
- Accept: `resourceFromAttributes` or `detectResources` as the resource source; `merge` for composition
- Reject: direct `Resource` interface implementations; hand-rolled attribute merging outside `merge`
