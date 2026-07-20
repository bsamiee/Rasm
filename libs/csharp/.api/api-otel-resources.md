# [RASM_API_OTEL_RESOURCES]

Five resource-detector packages enrich the OpenTelemetry `Resource` with semantic-convention host, operating-system, process, container, and .NET-runtime attributes. Each package exposes one public `ResourceBuilder` extension that appends a single internal `IResourceDetector`; detectors run once at provider build and fold their detected key-value attributes into the resolved resource. All five compose at app roots like the rest of the OpenTelemetry train, carrying branch-tier catalogs without a substrate registry row.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `OpenTelemetry.Resources.Container`
- package: `OpenTelemetry.Resources.Container`
- assembly: `OpenTelemetry.Resources.Container`
- namespace: `OpenTelemetry.Resources`, `OpenTelemetry.Resources.Container`
- asset: runtime library
- rail: resource detection
- detector: `ContainerDetector` — internal sealed `IResourceDetector`
- emits: `container.id`

[PACKAGE_SURFACE]: `OpenTelemetry.Resources.Host`
- package: `OpenTelemetry.Resources.Host`
- assembly: `OpenTelemetry.Resources.Host`
- namespace: `OpenTelemetry.Resources`, `OpenTelemetry.Resources.Host`
- asset: runtime library
- rail: resource detection
- detector: `HostDetector` — internal sealed `IResourceDetector`
- emits: `host.name`, `host.id`, `host.arch`

[PACKAGE_SURFACE]: `OpenTelemetry.Resources.OperatingSystem`
- package: `OpenTelemetry.Resources.OperatingSystem`
- assembly: `OpenTelemetry.Resources.OperatingSystem`
- namespace: `OpenTelemetry.Resources`, `OpenTelemetry.Resources.OperatingSystem`
- asset: runtime library
- rail: resource detection
- detector: `OperatingSystemDetector` — internal sealed `IResourceDetector`
- emits: `os.type`, `os.description`, `os.build_id`, `os.name`, `os.version`

[PACKAGE_SURFACE]: `OpenTelemetry.Resources.Process`
- package: `OpenTelemetry.Resources.Process`
- assembly: `OpenTelemetry.Resources.Process`
- namespace: `OpenTelemetry.Resources`, `OpenTelemetry.Resources.Process`
- asset: runtime library
- rail: resource detection
- detector: `ProcessDetector` — internal sealed `IResourceDetector`
- emits: `process.owner`, `process.pid`, `process.creation.time`

[PACKAGE_SURFACE]: `OpenTelemetry.Resources.ProcessRuntime`
- package: `OpenTelemetry.Resources.ProcessRuntime`
- assembly: `OpenTelemetry.Resources.ProcessRuntime`
- namespace: `OpenTelemetry.Resources`, `OpenTelemetry.Resources.ProcessRuntime`
- asset: runtime library
- rail: resource detection
- detector: `ProcessRuntimeDetector` — internal sealed `IResourceDetector`
- emits: `process.runtime.description`, `process.runtime.name`, `process.runtime.version`

## [02]-[PUBLIC_TYPES]

[DETECTOR_EXTENSIONS]: public resource-builder extensions (namespace `OpenTelemetry.Resources`)
- rail: resource detection

| [INDEX] | [SYMBOL]                                   | [PACKAGE_ROLE]            | [CAPABILITY]                        |
| :-----: | :----------------------------------------- | :------------------------ | :---------------------------------- |
|  [01]   | `ContainerResourceBuilderExtensions`       | container detector        | registers `ContainerDetector`       |
|  [02]   | `HostResourceBuilderExtensions`            | host detector             | registers `HostDetector`            |
|  [03]   | `OperatingSystemResourceBuilderExtensions` | operating-system detector | registers `OperatingSystemDetector` |
|  [04]   | `ProcessResourceBuilderExtensions`         | process detector          | registers `ProcessDetector`         |
|  [05]   | `ProcessRuntimeResourceBuilderExtensions`  | process-runtime detector  | registers `ProcessRuntimeDetector`  |

Each detector is an `internal sealed` `IResourceDetector`; a package exposes only its extension class publicly, and the extension appends its detector through `ResourceBuilder.AddDetector`. Semantic-convention keys ride the standard `container.*`, `host.*`, `os.*`, `process.*`, and `process.runtime.*` namespaces.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: detector admission on `ResourceBuilder`
- rail: resource detection

| [INDEX] | [SURFACE]                    | [KIND]             | [CAPABILITY]                      |
| :-----: | :--------------------------- | :----------------- | :-------------------------------- |
|  [01]   | `AddContainerDetector`       | detector admission | appends `ContainerDetector`       |
|  [02]   | `AddHostDetector`            | detector admission | appends `HostDetector`            |
|  [03]   | `AddOperatingSystemDetector` | detector admission | appends `OperatingSystemDetector` |
|  [04]   | `AddProcessDetector`         | detector admission | appends `ProcessDetector`         |
|  [05]   | `AddProcessRuntimeDetector`  | detector admission | appends `ProcessRuntimeDetector`  |

Every extension carries one overload — `Add<X>Detector(this ResourceBuilder builder)` returning the same `ResourceBuilder` — so admission chains fluently; a null builder faults through `Guard.ThrowIfNull`. Detectors execute once at provider build, and each contributes only the attributes it resolves on the host.

## [04]-[IMPLEMENTATION_LAW]

[RESOURCE_TOPOLOGY]:
- additive fold: each `Add<X>Detector` appends one `IResourceDetector`; `ResourceBuilder` folds every detector's key-value attributes into the resolved `Resource` at provider build, and registration order is contribution order.
- conditional emission: `HostDetector` always emits `host.name`, adding `host.id`/`host.arch` only when resolvable; `OperatingSystemDetector` always emits `os.type`, adding `os.name`/`os.version`/`os.build_id`/`os.description` per platform (`windows`/`linux`/`darwin`); `ProcessDetector` emits `process.owner`/`process.pid` always and `process.creation.time` when readable; `ContainerDetector` emits `container.id` only inside a container; `ProcessRuntimeDetector` emits all three `process.runtime.*` from `RuntimeInformation`.
- failure rail: extraction faults route to each package's `EventSource`; a detector resolving nothing degrades to an empty contribution, never a throw.

[STACKING]:
- these detectors compose at the AppHost `ConfigureResource(identity)` seam beside the identity triple (`service.namespace=rasm` / `service.name` / `service.instance.id`); resource detectors ENRICH, never replace, the minted identity. Identity mints the triple first, then chains `Add<X>Detector` calls onto the same `ResourceBuilder`.
- `OpenTelemetry`(`api-opentelemetry.md`): `ResourceBuilder` and `ConfigureResource` are that catalog's identity surface, and `SetResourceBuilder` is its replace verb; these five packages are the detector rows the augmenting `ConfigureResource` delegate appends.
- semconv keys stay disjoint from the minted identity keys, so enrichment adds resource dimensions without colliding with `service.*`.

[LOCAL_ADMISSION]:
- composition-root-only, at the AppHost telemetry root; all five `Add<X>Detector` calls chain inside the one `ConfigureResource(identity)` delegate.
- branch-tier catalog like the instrumentation train — these compose at app roots alone and carry no substrate registry row.

[RAIL_LAW]:
- Package: `OpenTelemetry.Resources.Container` / `.Host` / `.OperatingSystem` / `.Process` / `.ProcessRuntime`
- Owns: semconv resource-attribute detection for container, host, operating system, process, and .NET runtime
- Accept: `Add<X>Detector` chained onto the identity `ResourceBuilder` at the composition root
- Reject: hand-rolled resource attributes for these semconv keys; `SetResourceBuilder` replacing the minted identity
