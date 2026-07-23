# [TS_RUNTIME_API_OPENTELEMETRY_RESOURCE_DETECTOR_CONTAINER]

`@opentelemetry/resource-detector-container` mints one `ResourceDetector` — `containerDetector` — that reads cgroup and container-runtime facts and stamps `container.id` onto the OTLP `Resource`. It lands as a detector row in the `otel/emit` node roster beside `envDetector`/`hostDetector`/`osDetector`/`processDetector`/`serviceInstanceIdDetector`, never composed in a library. Detector selection is a deploy-target fact — an app on a container arm composes this row and a bare-host arm omits it.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/resource-detector-container`
- package: `@opentelemetry/resource-detector-container` (Apache-2.0)
- base: `containerDetector` implements `@opentelemetry/resources` `ResourceDetector`; `detect()` returns a `DetectedResource`
- consumed-by: `otel/emit` node detector roster; folds beside the `@opentelemetry/resources` `env`/`host`/`os`/`process`/`serviceInstanceId` detectors
- runtime: node only — reads `/proc/self/cgroup` and `/proc/self/mountinfo` container-runtime facts

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: detector instance + contract
- rail: observability/resource/detect

| [INDEX] | [SYMBOL]                                          | [TYPE_FAMILY]     | [CONSUMER_BOUNDARY]                                  |
| :-----: | :------------------------------------------------ | :---------------- | :--------------------------------------------------- |
|  [01]   | `containerDetector: ContainerDetector`            | detector instance | one row in the `otel/emit` node detector roster      |
|  [02]   | `ResourceDetector { detect(): DetectedResource }` | detector contract | enricher interface the node roster folds             |
|  [03]   | `DetectedResource { attributes? }`                | detector output   | `container.id` attribute map the detect call returns |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: detector composition
- rail: observability/resource/detect
- One singleton is the whole surface; a container arm folds it into the node lane's `ResourceDetector[]`.

| [INDEX] | [SURFACE]                    | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                                            |
| :-----: | :--------------------------- | :------------- | :------------------------------------------------------------- |
|  [01]   | `containerDetector`          | detector row   | one entry in `detectResources({ detectors })` on the node lane |
|  [02]   | `containerDetector.detect()` | detect call    | roster-driven; yields the `container.id` `DetectedResource`    |

## [04]-[IMPLEMENTATION_LAW]

[CONTAINER_DETECTOR_TOPOLOGY]:
- node-roster row only — `containerDetector` folds into the `otel/emit` node lane's `ResourceDetector[]` beside the `@opentelemetry/resources` detectors; a library-altitude compose double-detects the host.
- deploy-target selection governs the row — a container arm composes `containerDetector`; a bare-host arm carries no container row.

[INTEGRATION_LAW]:
- Stack with `otel/emit` node row: `containerDetector` enters the node lane's detector roster and the `Hooks` registry's `ResourceDetector` contribution cell; its `container.id` merges onto the `AppIdentity` base `Resource` at boot.
- Stack with `opentelemetry-resources.md` detector fold: `detectResources({ detectors })` runs `containerDetector` in the ordered set and `merge`s its output onto the base resource; the async-attribute barrier gates first export until the cgroup read resolves.

[LOCAL_ADMISSION]:
- `scope:runtime`, node lane; the row lives only in the node boot graph, never a browser or library composition.

[RAIL_LAW]:
- Package: `@opentelemetry/resource-detector-container`
- Owns: `container.id` detection from cgroup and mountinfo container-runtime facts
- Accept: one `containerDetector` row in the `otel/emit` node roster on a container deploy arm
- Reject: library-altitude composition, a browser-lane row, a hand-rolled cgroup reader where this detector belongs
