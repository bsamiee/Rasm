# [TS_RUNTIME_API_OPENTELEMETRY_RESOURCE_DETECTOR_GCP]

`@opentelemetry/resource-detector-gcp` mints one `ResourceDetector` — `gcpDetector` — that reads the GCP metadata server and stamps GCE, GKE, Cloud Run, Cloud Functions, and App Engine facts onto the OTLP `Resource`. It lands as a detector row in the `otel/emit` node roster beside `envDetector`/`hostDetector`/`osDetector`/`processDetector`/`serviceInstanceIdDetector`, never composed in a library. Detector selection is a deploy-target fact — an app on a GCP arm composes this row and a non-GCP arm omits it.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/resource-detector-gcp`
- package: `@opentelemetry/resource-detector-gcp` (Apache-2.0)
- base: `gcpDetector` implements `@opentelemetry/resources` `ResourceDetector`; `detect()` returns a `DetectedResource` resolving async metadata attributes
- consumed-by: `otel/emit` node detector roster; folds beside the `@opentelemetry/resources` `env`/`host`/`os`/`process`/`serviceInstanceId` detectors
- runtime: node only — reads the GCP metadata server over the `gcp-metadata` client

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: detector instance + contract
- rail: observability/resource/detect

| [INDEX] | [SYMBOL]                                          | [TYPE_FAMILY]     | [CONSUMER_BOUNDARY]                                          |
| :-----: | :------------------------------------------------ | :---------------- | :----------------------------------------------------------- |
|  [01]   | `gcpDetector: GcpDetector`                        | detector instance | one row in the `otel/emit` node roster; resolves async facts |
|  [02]   | `ResourceDetector { detect(): DetectedResource }` | detector contract | enricher interface the node roster folds                     |
|  [03]   | `DetectedResource { attributes? }`                | detector output   | async `cloud.*`/`host.*`/`faas.*` attribute map              |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: detector composition
- rail: observability/resource/detect
- One singleton is the detection surface; `resetIsAvailableCache` clears the metadata-server availability cache the `gcp-metadata` probe memoizes.

| [INDEX] | [SURFACE]               | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                                            |
| :-----: | :---------------------- | :------------- | :------------------------------------------------------------- |
|  [01]   | `gcpDetector`           | detector row   | one entry in `detectResources({ detectors })` on the node lane |
|  [02]   | `resetIsAvailableCache` | cache reset    | re-exported `gcp-metadata` availability-cache reset for probes |

## [04]-[IMPLEMENTATION_LAW]

[GCP_DETECTOR_TOPOLOGY]:
- node-roster row only — `gcpDetector` folds into the `otel/emit` node lane's `ResourceDetector[]` beside the `@opentelemetry/resources` detectors; a library-altitude compose double-detects the host.
- deploy-target selection governs the row — a GCE, GKE, Cloud Run, Cloud Functions, or App Engine arm composes `gcpDetector`; a non-GCP arm carries no metadata-server row.
- metadata reads resolve async — `gcpDetector` resolves `DetectedResource` attributes off the metadata server, so the async-attribute barrier gates first export.

[INTEGRATION_LAW]:
- Stack with `otel/emit` node row: `gcpDetector` enters the node lane's detector roster and the `Hooks` registry's `ResourceDetector` contribution cell; its metadata facts merge onto the `AppIdentity` base `Resource`.
- Stack with `opentelemetry-resources.md` detector fold: `detectResources({ detectors })` runs `gcpDetector` in the ordered set and `merge`s its async output; `waitForAsyncAttributes` gates export until the metadata probe resolves.

[LOCAL_ADMISSION]:
- `scope:runtime`, node lane; the row lives only in a GCP-arm node boot graph, never a browser or library composition.

[RAIL_LAW]:
- Package: `@opentelemetry/resource-detector-gcp`
- Owns: GCP metadata-server resource detection across GCE, GKE, Cloud Run, Cloud Functions, and App Engine
- Accept: one `gcpDetector` row in the `otel/emit` node roster on a GCP deploy arm; `resetIsAvailableCache` for metadata-probe cache hygiene
- Reject: library-altitude composition, a browser-lane row, a hand-rolled metadata-server client where this detector belongs
