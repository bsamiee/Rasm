# [TS_RUNTIME_API_OPENTELEMETRY_RESOURCE_DETECTOR_GCP]

`@opentelemetry/resource-detector-gcp` mints one `ResourceDetector` — `gcpDetector` — reading the GCP metadata server and stamping GCE, GKE, Cloud Run, Cloud Functions, and App Engine facts onto the OTLP `Resource`. A GCP deploy arm folds it as one row in the `otel/emit` node roster beside the `@opentelemetry/resources` detectors; a non-GCP arm omits it, and no library composes it.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/resource-detector-gcp`
- package: `@opentelemetry/resource-detector-gcp` (Apache-2.0)
- base: `gcpDetector` implements `@opentelemetry/resources` `ResourceDetector`; `detect()` returns a `DetectedResource`
- consumed-by: `otel/emit` node detector roster; folds beside the `@opentelemetry/resources` detectors
- runtime: node only — reads the GCP metadata server over the `gcp-metadata` client
- rail: observability/resource/detect

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: detector instance + contract

| [INDEX] | [SYMBOL]                                          | [TYPE_FAMILY]     | [CAPABILITY]                                    |
| :-----: | :------------------------------------------------ | :---------------- | :---------------------------------------------- |
|  [01]   | `gcpDetector: GcpDetector`                        | detector instance | the GCP metadata-server detector row            |
|  [02]   | `ResourceDetector { detect(): DetectedResource }` | detector contract | enricher interface the node roster folds        |
|  [03]   | `DetectedResource { attributes? }`                | detector output   | async `cloud.*`/`host.*`/`faas.*` attribute map |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: detector composition
- One singleton is the whole surface; a GCP arm folds it into the node lane's `ResourceDetector[]`.

| [INDEX] | [SURFACE]               | [SHAPE]  | [CAPABILITY]                                                 |
| :-----: | :---------------------- | :------- | :----------------------------------------------------------- |
|  [01]   | `gcpDetector`           | instance | one row in `detectResources({ detectors })` on the node lane |
|  [02]   | `resetIsAvailableCache` | static   | resets the `gcp-metadata` availability cache for probes      |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- node-roster row only — `gcpDetector` folds into the `otel/emit` node lane's `ResourceDetector[]` beside the `@opentelemetry/resources` detectors; a library-altitude compose double-detects the host.
- deploy-target selection governs the row — a GCP deploy arm composes `gcpDetector`; a non-GCP arm carries no metadata-server row.

[STACKING]:
- `otel/emit` node lane: `gcpDetector` enters the detector roster and the `Hooks` registry's `ResourceDetector` contribution cell; its metadata facts merge onto the `AppIdentity` base `Resource`.
- `@opentelemetry/resources`(`.api/opentelemetry-resources.md`): `detectResources({ detectors })` runs `gcpDetector` in the ordered set and `merge`s its async output; `waitForAsyncAttributes` gates export until the metadata probe resolves.

[LOCAL_ADMISSION]:
- `scope:runtime`, node lane; the row lives only in a GCP-arm node boot graph, never a browser or library composition.

[RAIL_LAW]:
- Package: `@opentelemetry/resource-detector-gcp`
- Owns: GCP metadata-server resource detection across GCE, GKE, Cloud Run, Cloud Functions, and App Engine
- Accept: one `gcpDetector` row in the `otel/emit` node roster on a GCP deploy arm; `resetIsAvailableCache` for metadata-probe cache hygiene
- Reject: library-altitude composition, a browser-lane row, a hand-rolled metadata-server client where this detector belongs
