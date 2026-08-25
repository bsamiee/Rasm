# [TS_RUNTIME_API_OPENTELEMETRY_BROWSER_DETECTOR]

`@opentelemetry/opentelemetry-browser-detector` mints one `browserDetector` singleton that stamps `browser.*` user-agent facts onto the OTLP `Resource` from the UA client-hints API. A browser app folds it as one detector row on the `otel/emit` `web` lane; a node arm omits it.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the detector class the singleton instances; its `ResourceDetector`/`DetectedResource`/`ResourceDetectionConfig` contract is owned by `opentelemetry-resources.md`

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY] | [CAPABILITY]                                         |
| :-----: | :---------------- | :------------ | :--------------------------------------------------- |
|  [01]   | `BrowserDetector` | class         | implements the resources `ResourceDetector` contract |

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: one singleton is the whole surface a browser boot folds into the `web` lane detector set

| [INDEX] | [SURFACE]                                                              | [SHAPE]  | [CAPABILITY]                                  |
| :-----: | :--------------------------------------------------------------------- | :------- | :-------------------------------------------- |
|  [01]   | `browserDetector`                                                      | instance | the detector row a browser arm composes       |
|  [02]   | `browserDetector.detect(ResourceDetectionConfig?) -> DetectedResource` | instance | lane-driven read yielding the `browser.*` map |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- UA client-hints reads degrade — absent `navigator.userAgentData` leaves brand, platform, and mobile unset, stamping only what the API exposes.

[STACKING]:
- `@opentelemetry/resources`(`.api/opentelemetry-resources.md`): `browserDetector` implements its `ResourceDetector` contract; `detectResources({ detectors })` runs it in the ordered set and `merge`s the returned `DetectedResource` onto the base `Resource`.
- `otel/emit` web lane: `browserDetector` enters the `web` detector set beside the browser-degraded `@opentelemetry/resources` detectors, merging its `browser.*` attributes onto the `AppIdentity` base `Resource` at boot.

[LOCAL_ADMISSION]:
- `scope:runtime`, browser lane only — the row lives in the browser boot graph, never a node or library composition.
