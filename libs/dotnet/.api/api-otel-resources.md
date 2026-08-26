# [RASM_API_OTEL_RESOURCES]

Resource detection folds host environment facts into semantic-convention attributes on the OpenTelemetry `Resource`: each package seats one `IResourceDetector` behind a single public `ResourceBuilder` extension contributing only the keys it resolves at provider build. Each new detection dimension lands as one package record with its extension row.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: static extension classes on `ResourceBuilder`, each seating one `internal sealed` `IResourceDetector`

| [INDEX] | [SYMBOL]                                   | [TYPE_FAMILY] | [CAPABILITY]              |
| :-----: | :----------------------------------------- | :------------ | :------------------------ |
|  [01]   | `ContainerResourceBuilderExtensions`       | class         | `ContainerDetector`       |
|  [02]   | `HostResourceBuilderExtensions`            | class         | `HostDetector`            |
|  [03]   | `OperatingSystemResourceBuilderExtensions` | class         | `OperatingSystemDetector` |
|  [04]   | `ProcessResourceBuilderExtensions`         | class         | `ProcessDetector`         |
|  [05]   | `ProcessRuntimeResourceBuilderExtensions`  | class         | `ProcessRuntimeDetector`  |

## [02]-[ENTRYPOINTS]

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
- `AddOperatingSystemDetector`: contributes nothing outside `windows`, `linux`, and `darwin`; `os.type` always lands and `os.description` reads the runtime description, while `os.build_id`/`os.name`/`os.version` read the Windows `CurrentVersion` registry key, the linux `os-release` beside `/proc/sys/kernel/osrelease`, or the darwin system plist. All five keys reach all three platforms — the linux arm falls `os.name` back to `Linux` where `os-release` names nothing, and an empty extraction drops its key rather than writing a blank.
- Linux `os.build_id` sources the `os-release` `BUILD_ID=` field and falls back to the KERNEL release string, so that key carries a distribution build on one host and a kernel version on another; a query grouping on it folds the two into one dimension unless `os.name` disambiguates.
- Darwin plist extraction abandons ALL THREE of its keys whenever the plist's `key` and `string` element counts disagree, so a malformed system plist drops `os.build_id`, `os.name`, and `os.version` together rather than the one unreadable key.
- `AddProcessDetector`: `process.creation.time` lands as a UTC ISO round-trip string, dropping where `Process.StartTime` faults.
- Every detector here stamps `https://opentelemetry.io/schemas/1.43.0` on the `Resource` it returns, so the five agree with each other and with a branch pinning that same semconv coordinate.

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `ResourceBuilder.Build` folds every registered detector's `Resource` through `Resource.Merge` in registration order, so the last extension chained wins any key two contributors share.
- That same fold merges SCHEMA URLS, and two contributors disagreeing annihilates the coordinate: the merge nulls the url, raises `ResourceSchemaUrlMergeConflict`, and LATCHES — every later merge in the chain short-circuits to null, so one divergent contributor strips the schema url off the whole `Resource`, not off its own row.
- Chains therefore pin ONE semconv coordinate end to end: identity states the same url these detectors stamp, or states none and adopts theirs, and a value chosen for symmetry with a peer branch rather than for agreement with the chain silently deletes the pin it asserts.
- Detectors resolving nothing contribute `Resource.Empty` and an extraction fault drops its key, so detection never throws out of `Build`.
- Detected keys stay disjoint from the minted `service.*` identity, so detection adds placement dimensions without contending for the identity slots.

[STACKING]:
- `OpenTelemetry`(`api-opentelemetry.md`): `ResourceBuilder.AddDetector` is the hook every extension rides and `ConfigureResource` the augmenting delegate carrying them onto each provider builder; `CreateDefault` seats `AddEnvironmentVariableDetector` ahead of them, so a chained detector outranks an `OTEL_RESOURCE_ATTRIBUTES` value for the same key.
- AppHost observability root: its identity delegate mints the `service.*` triple, chains the detector extensions onto that same `ResourceBuilder`, and re-chains `AddEnvironmentVariableDetector` last, yielding one `Resource` carrying identity, placement, and the deployment override in that precedence.

[LOCAL_ADMISSION]:
- Detector chaining sits inside the one `ConfigureResource(identity)` delegate, each root selecting its detector rows by deployment profile.
- Merge order IS precedence, so a root wanting the deployment override to win re-chains `AddEnvironmentVariableDetector` as the chain TAIL: `CreateDefault` seats it ahead of every `ConfigureResource` row, which otherwise lets a minted identity attribute outrank the `OTEL_RESOURCE_ATTRIBUTES` value a deploy plane set for the same key.
- Branch-tier catalog: these packages compose at app roots and carry no substrate registry row.
