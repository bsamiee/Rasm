# [PY_RUNTIME_API_OPENTELEMETRY_RESOURCE_DETECTOR_CONTAINERID]

`opentelemetry-resource-detector-containerid` reads the host container id into a resource: one `ResourceDetector` whose `detect()` scans cgroup v1 `/proc/self/cgroup` first, then cgroup v2 `/proc/self/mountinfo`, and returns a `Resource` carrying `container.id` under `ResourceAttributes.CONTAINER_ID`. Off a container host both reads miss and `detect()` returns `Resource.get_empty()` at debug-log cost, carrying no attribute and no schema url.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `opentelemetry-resource-detector-containerid`
- package: `opentelemetry-resource-detector-containerid`
- module: `opentelemetry.resource.detector.containerid`
- owner: `runtime`
- rail: observability
- asset: pure-Python runtime library
- namespaces: `opentelemetry.resource.detector.containerid`
- capability: cgroup v1/v2 container-id read merged onto a `Resource` as `container.id`, empty resource off a container host

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: resource detector
- rail: observability

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY]     | [RAIL]                                             |
| :-----: | :-------------------------- | :---------------- | :------------------------------------------------- |
|  [01]   | `ContainerResourceDetector` | resource detector | cgroup v1/v2 `container.id` read into a `Resource` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: detector registration
- rail: observability
- `detect()` returns a `Resource`: `container.id` merged onto `Resource.get_empty()` on a container host, `Resource.get_empty()` on a miss; a broad-exception guard honors `raise_on_error` and otherwise returns the empty resource.

| [INDEX] | [SURFACE]                                                 | [ENTRY_FAMILY] | [RAIL]                                  |
| :-----: | :-------------------------------------------------------- | :------------- | :-------------------------------------- |
|  [01]   | `ContainerResourceDetector()`                             | construct      | cgroup detector instance                |
|  [02]   | `ContainerResourceDetector().detect()`                    | detect         | `Resource` with `container.id` or empty |
|  [03]   | `get_aggregated_resources([ContainerResourceDetector()])` | register       | detector list, merged in order          |

## [04]-[IMPLEMENTATION_LAW]

[OBSERVABILITY_TOPOLOGY]:
- detect law: `detect()` reads cgroup v1 `/proc/self/cgroup`, falls to cgroup v2 `/proc/self/mountinfo` on a v1 miss, and merges the first 64-hex container id onto `Resource.get_empty()` under `ResourceAttributes.CONTAINER_ID`; a non-container host misses both readers and returns `Resource.get_empty()`.
- merge law: the detector composes as one entry in the runtime telemetry `get_aggregated_resources` detector list before the env detector, so the env detector runs last and wins the merge on a `container.id` conflict.
- cost law: no-op on macOS and dev hosts — the absent `/proc` files raise `FileNotFoundError`, swallowed to a debug log inside each id reader, so `detect()` returns the empty resource with no warning and no `container.id`.
- schema law: the detector carries no `schema_url`, so the pinned initial resource keeps its own schema url through the merge.

[RAIL_LAW]:
- Package: `opentelemetry-resource-detector-containerid`
- Owns: the `container.id` resource attribute read from cgroup v1/v2 on a container host
- Accept: one detector-list entry ahead of the env detector in `get_aggregated_resources`
- Reject: a detector position after the env detector, a hand-rolled cgroup read beside it
