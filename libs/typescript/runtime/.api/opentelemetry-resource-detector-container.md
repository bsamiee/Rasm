# [TS_RUNTIME_API_OPENTELEMETRY_RESOURCE_DETECTOR_CONTAINER]

`@opentelemetry/resource-detector-container` mints one `ResourceDetector`, `containerDetector`, reading `/proc` cgroup and mountinfo facts to stamp `container.id` onto the OTLP `Resource`.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: detector instance + contract
- rail: observability/resource/detect

| [INDEX] | [SYMBOL]                                          | [TYPE_FAMILY]     | [CAPABILITY]                                    |
| :-----: | :------------------------------------------------ | :---------------- | :---------------------------------------------- |
|  [01]   | `containerDetector: ContainerDetector`            | detector instance | one row in the `otel/emit` node roster          |
|  [02]   | `ResourceDetector { detect(): DetectedResource }` | detector contract | enricher interface the node roster folds        |
|  [03]   | `DetectedResource { attributes? }`                | detector output   | `container.id` attribute map `detect()` returns |

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: detector composition
- rail: observability/resource/detect

| [INDEX] | [SURFACE]           | [SHAPE]  | [CAPABILITY]                                  |
| :-----: | :------------------ | :------- | :-------------------------------------------- |
|  [01]   | `containerDetector` | instance | one entry in `detectResources({ detectors })` |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `containerDetector` folds into the `otel/emit` node lane's `ResourceDetector[]`; a library-altitude compose double-detects the host.
- deploy-target selection governs the row — a container arm composes `containerDetector`, a bare-host arm carries none.

[STACKING]:
- `otel/emit` node lane: `containerDetector` enters the detector roster and the `Hooks` registry's `ResourceDetector` contribution cell; its `container.id` merges onto the `AppIdentity` base `Resource` at boot.
- `opentelemetry-resources.md` detector fold: `detectResources({ detectors })` runs `containerDetector` in the ordered set and `merge`s its output onto the base resource; `waitForAsyncAttributes` gates first export until the cgroup read resolves.

[LOCAL_ADMISSION]:
- `scope:runtime` node lane; the row lives only in a container-arm node boot graph.
