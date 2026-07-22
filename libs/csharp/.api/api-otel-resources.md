# [RASM_API_OTEL_RESOURCES]

Resource detection folds host environment facts into semantic-convention attributes on the OpenTelemetry `Resource`: each package seats one `IResourceDetector` behind a single public `ResourceBuilder` extension contributing only the keys it resolves at provider build. A new detection dimension lands as one package record with its extension row.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `OpenTelemetry.Resources.Container`
- package: `OpenTelemetry.Resources.Container`
- assembly: `OpenTelemetry.Resources.Container`
- namespace: `OpenTelemetry.Resources`
- rail: container identity detection

[PACKAGE_SURFACE]: `OpenTelemetry.Resources.Host`
- package: `OpenTelemetry.Resources.Host`
- assembly: `OpenTelemetry.Resources.Host`
- namespace: `OpenTelemetry.Resources`
- rail: host attribute detection

[PACKAGE_SURFACE]: `OpenTelemetry.Resources.OperatingSystem`
- package: `OpenTelemetry.Resources.OperatingSystem`
- assembly: `OpenTelemetry.Resources.OperatingSystem`
- namespace: `OpenTelemetry.Resources`
- rail: operating-system attribute detection

[PACKAGE_SURFACE]: `OpenTelemetry.Resources.Process`
- package: `OpenTelemetry.Resources.Process`
- assembly: `OpenTelemetry.Resources.Process`
- namespace: `OpenTelemetry.Resources`
- rail: process attribute detection

[PACKAGE_SURFACE]: `OpenTelemetry.Resources.ProcessRuntime`
- package: `OpenTelemetry.Resources.ProcessRuntime`
- assembly: `OpenTelemetry.Resources.ProcessRuntime`
- namespace: `OpenTelemetry.Resources`
- rail: .NET runtime attribute detection

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: static extension classes on `ResourceBuilder`, each seating one `internal sealed` `IResourceDetector`

| [INDEX] | [SYMBOL]                                   | [TYPE_FAMILY] | [CAPABILITY]              |
| :-----: | :----------------------------------------- | :------------ | :------------------------ |
|  [01]   | `ContainerResourceBuilderExtensions`       | class         | `ContainerDetector`       |
|  [02]   | `HostResourceBuilderExtensions`            | class         | `HostDetector`            |
|  [03]   | `OperatingSystemResourceBuilderExtensions` | class         | `OperatingSystemDetector` |
|  [04]   | `ProcessResourceBuilderExtensions`         | class         | `ProcessDetector`         |
|  [05]   | `ProcessRuntimeResourceBuilderExtensions`  | class         | `ProcessRuntimeDetector`  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: one overload each, `Add<X>Detector(ResourceBuilder) -> ResourceBuilder` returning the supplied builder so admission chains; a null builder throws

| [INDEX] | [SURFACE]                    | [SHAPE] | [CAPABILITY]                                                                   |
| :-----: | :--------------------------- | :------ | :----------------------------------------------------------------------------- |
|  [01]   | `AddContainerDetector`       | static  | `container.id`                                                                 |
|  [02]   | `AddHostDetector`            | static  | `host.name` `host.id` `host.arch`                                              |
|  [03]   | `AddOperatingSystemDetector` | static  | `os.type` `os.description` `os.build_id` `os.name` `os.version`                |
|  [04]   | `AddProcessDetector`         | static  | `process.owner` `process.pid` `process.creation.time`                          |
|  [05]   | `AddProcessRuntimeDetector`  | static  | `process.runtime.description` `process.runtime.name` `process.runtime.version` |

- `AddContainerDetector`: `container.id` lands only where a cgroup v1 read, then a v2 mountinfo read, yields a valid id.
- `AddHostDetector`: `host.name` always lands; `host.id` and `host.arch` drop where no machine id resolves or the architecture maps to nothing.
- `AddOperatingSystemDetector`: a platform outside `windows`, `linux`, and `darwin` contributes nothing; the descriptive keys read the Windows registry, `os-release`, or the system plist.
- `AddProcessDetector`: `process.creation.time` lands as an ISO round-trip string, dropping where `Process.StartTime` faults.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `ResourceBuilder.Build` folds every registered detector's `Resource` through `Resource.Merge` in registration order, so the last extension chained wins any key two contributors share.
- A detector resolving nothing contributes `Resource.Empty` and an extraction fault drops its key, so detection never throws out of `Build`.
- Detected keys stay disjoint from the minted `service.*` identity, so detection adds placement dimensions without contending for the identity slots.

[STACKING]:
- `OpenTelemetry`(`api-opentelemetry.md`): `ResourceBuilder.AddDetector` is the seam every extension rides and `ConfigureResource` the augmenting delegate carrying them onto each provider builder; `CreateDefault` seats `AddEnvironmentVariableDetector` ahead of them, so a chained detector outranks an `OTEL_RESOURCE_ATTRIBUTES` value for the same key.
- AppHost observability root: its identity delegate mints the `service.*` triple, then chains the detector extensions onto that same `ResourceBuilder`, yielding one `Resource` carrying identity and placement together.

[LOCAL_ADMISSION]:
- Detector chaining sits inside the one `ConfigureResource(identity)` delegate, each root selecting its detector rows by deployment profile.
- Branch-tier catalog: these packages compose at app roots and carry no substrate registry row.

[RAIL_LAW]:
- Package: `OpenTelemetry.Resources.Container` `OpenTelemetry.Resources.Host` `OpenTelemetry.Resources.OperatingSystem` `OpenTelemetry.Resources.Process` `OpenTelemetry.Resources.ProcessRuntime`
- Owns: semconv resource-attribute detection for container, host, operating system, process, and .NET runtime
- Accept: `Add<X>Detector` chained onto the identity `ResourceBuilder` at a composition root
- Reject: hand-rolled attribute construction for these semconv keys; `SetResourceBuilder` discarding the minted identity
