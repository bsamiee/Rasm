# [TS_RUNTIME_API_OPENTELEMETRY_BROWSER_DETECTOR]

`@opentelemetry/opentelemetry-browser-detector` mints one `ResourceDetector` — `browserDetector` — that stamps user-agent brand, platform, mobile, and language facts onto the OTLP `Resource`. It lands as a detector row on the `otel/emit` `web` lane beside the browser-degraded `@opentelemetry/resources` detectors, never composed in a library. Detector selection is a deploy-target fact — a browser app composes this row and a node arm omits it.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/opentelemetry-browser-detector`
- package: `@opentelemetry/opentelemetry-browser-detector` (Apache-2.0)
- base: `browserDetector` implements `@opentelemetry/resources` `ResourceDetector`; `detect(config?)` returns a `DetectedResource`
- consumed-by: `otel/emit` `web` lane detector set; folds beside the browser-degraded `@opentelemetry/resources` detectors
- runtime: browser only — reads `navigator.userAgentData` brands, platform, and mobile, and `navigator.language`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: detector instance + contract
- rail: observability/resource/detect
- browser attribute keys: `browser.brands`, `browser.platform`, `browser.mobile`, `browser.language` from the UA client-hints API.

| [INDEX] | [SYMBOL]                                                 | [TYPE_FAMILY]     | [CONSUMER_BOUNDARY]                                |
| :-----: | :------------------------------------------------------- | :---------------- | :------------------------------------------------- |
|  [01]   | `browserDetector: BrowserDetector`                       | detector instance | one row on the `otel/emit` `web` lane detector set |
|  [02]   | `ResourceDetector { detect(config?): DetectedResource }` | detector contract | enricher interface the `web` lane folds            |
|  [03]   | `DetectedResource { attributes? }`                       | detector output   | `browser.*` attribute map the detect call returns  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: detector composition
- rail: observability/resource/detect
- One singleton is the whole surface; a browser arm folds it into the `web` lane's detector set.

| [INDEX] | [SURFACE]                         | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                                    |
| :-----: | :-------------------------------- | :------------- | :----------------------------------------------------- |
|  [01]   | `browserDetector`                 | detector row   | one entry on the `otel/emit` `web` lane detector set   |
|  [02]   | `browserDetector.detect(config?)` | detect call    | lane-driven; yields the `browser.*` `DetectedResource` |

## [04]-[IMPLEMENTATION_LAW]

[BROWSER_DETECTOR_TOPOLOGY]:
- web-lane row only — `browserDetector` folds into the `otel/emit` `web` lane's detector set; the node detector roster never carries it.
- browser reads degrade — `navigator.userAgentData` absent leaves brand, platform, and mobile unset; `browserDetector` stamps only what the UA client-hints API exposes.
- deploy-target selection governs the row — a browser app composes `browserDetector`; a node arm carries no browser row.

[INTEGRATION_LAW]:
- Stack with `otel/emit` web row: `browserDetector` enters the `web` lane detector set beside the browser-degraded `@opentelemetry/resources` detectors; its `browser.*` attributes merge onto the `AppIdentity` base `Resource` at boot.
- Stack with `opentelemetry-resources.md` detector fold: `detectResources({ detectors })` runs `browserDetector` in the ordered set and `merge`s its output onto the base resource.

[LOCAL_ADMISSION]:
- `scope:runtime`, browser lane; the row lives only in the browser boot graph, never a node or library composition.

[RAIL_LAW]:
- Package: `@opentelemetry/opentelemetry-browser-detector`
- Owns: `browser.brands`/`browser.platform`/`browser.mobile`/`browser.language` resource detection from the UA client-hints API
- Accept: one `browserDetector` row on the `otel/emit` `web` lane
- Reject: node-lane composition, library-altitude registration, a hand-rolled `navigator` reader where this detector belongs
