# [API_CATALOGUE] @opentelemetry/resources

`@opentelemetry/resources` supplies the `Resource` interface, factory functions (`resourceFromAttributes`, `emptyResource`, `defaultResource`), built-in detectors (`envDetector`, `hostDetector`, `osDetector`, `processDetector`, `serviceInstanceIdDetector`), and `detectResources` for composing service-identity and environment attributes into a single merged resource for all SDK signal providers.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/resources`
- package: `@opentelemetry/resources`
- module: `@opentelemetry/resources`
- asset: runtime library
- rail: resource

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: resource and detection family
- rail: resource

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY] | [RAIL]                                                        |
| :-----: | :--------------------------- | :------------ | :------------------------------------------------------------ |
|   [1]   | `Resource`                   | interface     | attributes, schemaUrl, merge, waitForAsyncAttributes          |
|   [2]   | `ResourceDetector`           | interface     | detect(config?) -> DetectedResource                           |
|   [3]   | `DetectedResource`           | type          | { attributes?: DetectedResourceAttributes }                   |
|   [4]   | `DetectedResourceAttributes` | type          | Record\<string, MaybePromise\<AttributeValue \| undefined\>\> |
|   [5]   | `RawResourceAttribute`       | type          | [string, MaybePromise\<AttributeValue \| undefined\>]         |
|   [6]   | `MaybePromise<T>`            | type          | T \| Promise\<T\>                                             |
|   [7]   | `ResourceDetectionConfig`    | interface     | detectors?: Array\<ResourceDetector\>                         |
|   [8]   | `ResourceOptions`            | type          | { schemaUrl?: string }                                        |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: resource factory functions
- rail: resource

| [INDEX] | [SURFACE]                                      | [ENTRY_FAMILY] | [RAIL]                                           |
| :-----: | :--------------------------------------------- | :------------- | :----------------------------------------------- |
|   [1]   | `resourceFromAttributes(attributes, options?)` | factory        | resource from synchronous or async attribute map |
|   [2]   | `emptyResource()`                              | factory        | resource with no attributes                      |
|   [3]   | `defaultResource()`                            | factory        | resource with SDK telemetry identity attributes  |
|   [4]   | `detectResources(config?)`                     | detection      | run all detectors and merge results              |
|   [5]   | `defaultServiceName()`                         | utility        | derive default service name string               |

[ENTRYPOINT_SCOPE]: built-in detectors
- rail: resource

| [INDEX] | [SURFACE]                   | [ENTRY_FAMILY] | [RAIL]                                      |
| :-----: | :-------------------------- | :------------- | :------------------------------------------ |
|   [1]   | `envDetector`               | detector       | reads `OTEL_RESOURCE_ATTRIBUTES` env var    |
|   [2]   | `hostDetector`              | detector       | adds `host.name` and `host.arch` attributes |
|   [3]   | `osDetector`                | detector       | adds `os.type` and `os.version` attributes  |
|   [4]   | `processDetector`           | detector       | adds `process.*` attributes (Node.js)       |
|   [5]   | `serviceInstanceIdDetector` | detector       | adds a random `service.instance.id` UUID    |

[ENTRYPOINT_SCOPE]: Resource instance operations
- rail: resource

| [INDEX] | [SURFACE]                           | [ENTRY_FAMILY] | [RAIL]                                            |
| :-----: | :---------------------------------- | :------------- | :------------------------------------------------ |
|   [1]   | `resource.merge(other)`             | merge          | combine resources; other takes precedence         |
|   [2]   | `resource.attributes`               | property       | resolved `Attributes` map                         |
|   [3]   | `resource.schemaUrl`                | property       | optional schema URL string                        |
|   [4]   | `resource.asyncAttributesPending`   | property       | true when async attributes still resolving        |
|   [5]   | `resource.waitForAsyncAttributes()` | async settle   | resolves when all async attribute promises done   |
|   [6]   | `resource.getRawAttributes()`       | raw access     | `RawResourceAttribute[]` with unresolved promises |

## [4]-[IMPLEMENTATION_LAW]

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
