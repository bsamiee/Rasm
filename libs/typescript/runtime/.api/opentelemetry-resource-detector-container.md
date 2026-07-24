# [TS_RUNTIME_API_OPENTELEMETRY_RESOURCE_DETECTOR_CONTAINER]

`@opentelemetry/resource-detector-container` mints one `ResourceDetector`, `containerDetector`, reading `/proc` cgroup and mountinfo facts to stamp `container.id` onto the OTLP `Resource`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/resource-detector-container`
- package: `@opentelemetry/resource-detector-container` (Apache-2.0)
- base: `containerDetector` implements `@opentelemetry/resources` `ResourceDetector`; `detect()` returns a `DetectedResource`
- consumed-by: `otel/emit` node detector roster
- runtime: node only — reads `/proc/self/cgroup` (v1) and `/proc/self/mountinfo` (v2) for a 64-hex container id

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: detector instance + contract
- rail: observability/resource/detect

| [INDEX] | [SYMBOL]                                          | [TYPE_FAMILY]     | [CAPABILITY]                                    |
| :-----: | :------------------------------------------------ | :---------------- | :---------------------------------------------- |
|  [01]   | `containerDetector: ContainerDetector`            | detector instance | one row in the `otel/emit` node roster          |
|  [02]   | `ResourceDetector { detect(): DetectedResource }` | detector contract | enricher interface the node roster folds        |
|  [03]   | `DetectedResource { attributes? }`                | detector output   | `container.id` attribute map `detect()` returns |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: detector composition
- rail: observability/resource/detect

| [INDEX] | [SURFACE]           | [SHAPE]  | [CAPABILITY]                                  |
| :-----: | :------------------ | :------- | :-------------------------------------------- |
|  [01]   | `containerDetector` | instance | one entry in `detectResources({ detectors })` |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `containerDetector` folds into the `otel/emit` node lane's `ResourceDetector[]`; a library-altitude compose double-detects the host.
- deploy-target selection governs the row — a container arm composes `containerDetector`, a bare-host arm carries none.

[STACKING]:
- `otel/emit` node lane: `containerDetector` enters the detector roster and the `Hooks` registry's `ResourceDetector` contribution cell; its `container.id` merges onto the `AppIdentity` base `Resource` at boot.
- `opentelemetry-resources.md` detector fold: `detectResources({ detectors })` runs `containerDetector` in the ordered set and `merge`s its output onto the base resource; `waitForAsyncAttributes` gates first export until the cgroup read resolves.

[LOCAL_ADMISSION]:
- `scope:runtime` node lane; the row lives only in a container-arm node boot graph.

[RAIL_LAW]:
- Package: `@opentelemetry/resource-detector-container`
- Owns: `container.id` detection from cgroup and mountinfo container-runtime facts
- Accept: one `containerDetector` row in the `otel/emit` node roster on a container deploy arm
- Reject: library-altitude composition, a browser-lane row, a hand-rolled cgroup reader where this detector belongs
