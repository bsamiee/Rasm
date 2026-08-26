# [PY_RUNTIME_API_OPENTELEMETRY_RESOURCE_DETECTOR_CONTAINERID]

`opentelemetry-resource-detector-containerid` reads the host `container.id` into an OpenTelemetry `Resource`: one `ResourceDetector` returning the cgroup container id on a container host and an empty resource everywhere else, composed as one entry in the runtime `get_aggregated_resources` detector list ahead of the env detector on the observability pipeline.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: resource detector

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY] | [CAPABILITY]                                       |
| :-----: | :-------------------------- | :------------ | :------------------------------------------------- |
|  [01]   | `ContainerResourceDetector` | detector      | cgroup v1/v2 `container.id` read into a `Resource` |

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: detector construction and registration

| [INDEX] | [SURFACE]                                                 | [SHAPE]  | [CAPABILITY]                            |
| :-----: | :-------------------------------------------------------- | :------- | :-------------------------------------- |
|  [01]   | `ContainerResourceDetector(raise_on_error=False)`         | ctor     | cgroup detector instance                |
|  [02]   | `ContainerResourceDetector().detect() -> Resource`        | instance | `Resource` with `container.id` or empty |
|  [03]   | `get_aggregated_resources([ContainerResourceDetector()])` | static   | merge the detector list in order        |

- `ContainerResourceDetector.detect`: a broad-except guard logs a warning and returns `Resource.get_empty()` on a read failure, re-raising only when `raise_on_error` is set.

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `detect()` reads cgroup v1 `/proc/self/cgroup`, falls to cgroup v2 `/proc/self/mountinfo` on a v1 miss, and merges the first 64-hex container id onto `Resource.get_empty()` under `ResourceAttributes.CONTAINER_ID`; a non-container host misses both readers and returns `Resource.get_empty()`.
- off a container host the absent `/proc` files raise `FileNotFoundError` swallowed to a debug log inside each id reader, so `detect()` returns the empty resource with no `container.id` and no warning.
- `ContainerResourceDetector` carries no `schema_url`, so the pinned initial resource keeps its own schema url through the merge.

[STACKING]:
- `opentelemetry-sdk`(`.api/opentelemetry-sdk.md`): `ContainerResourceDetector` subclasses `sdk.resources.ResourceDetector` and implements `detect()`, merging `container.id` onto `Resource.get_empty()`; it enters `get_aggregated_resources([...])` as one detector ahead of `OTELResourceDetector`, so the env detector merges last and wins a `container.id` conflict.
- within-lib: the runtime composition root threads it through the same `get_aggregated_resources` sequence that builds the shared `Resource` labeling every SDK provider.

[LOCAL_ADMISSION]:
- construct once at the runtime composition root as a `get_aggregated_resources` entry ahead of the env detector; container hosts gain `container.id`, dev hosts no-op.
