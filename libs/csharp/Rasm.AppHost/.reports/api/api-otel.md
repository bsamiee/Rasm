# [RASM_APPHOST_API_OTEL]

`OpenTelemetry` supplies traces, metrics, resources, processors, readers, and activity/meter provider builders.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `OpenTelemetry`
- package: `OpenTelemetry`
- assembly: `OpenTelemetry`
- namespace: `OpenTelemetry`
- asset: runtime library
- rail: telemetry

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: telemetry family
- rail: telemetry

| [INDEX] | [SYMBOL]                  | [PACKAGE_ROLE]   | [CAPABILITY]               |
| :-----: | :------------------------ | :--------------- | :------------------------- |
|   [1]   | `TracerProviderBuilder`   | builder surface  | constructs configured root |
|   [2]   | `MeterProviderBuilder`    | builder surface  | constructs configured root |
|   [3]   | `ResourceBuilder`         | builder surface  | constructs configured root |
|   [4]   | `ActivitySource`          | trace source     | anchors telemetry contract |
|   [5]   | `Meter`                   | metric source    | anchors telemetry contract |
|   [6]   | `BaseProcessor<T>`        | signal processor | anchors telemetry contract |
|   [7]   | `BatchExportProcessor<T>` | batch processor  | anchors telemetry contract |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: telemetry operations
- rail: telemetry

| [INDEX] | [SURFACE]            | [CALL_SHAPE]      | [CAPABILITY]              |
| :-----: | :------------------- | :---------------- | :------------------------ |
|   [1]   | `AddSource`          | DI extension      | admits configured surface |
|   [2]   | `AddMeter`           | DI extension      | admits configured surface |
|   [3]   | `SetResourceBuilder` | mutation call     | admits configured surface |
|   [4]   | `AddProcessor`       | DI extension      | admits configured surface |
|   [5]   | `AddReader`          | builder extension | admits metric reader      |
|   [6]   | `Build`              | factory call      | creates configured handle |
|   [7]   | `Dispose`            | operation call    | executes operation        |

## [4]-[IMPLEMENTATION_LAW]

[TELEMETRY_TOPOLOGY]:
- namespaces: `OpenTelemetry`, `OpenTelemetry.Trace`, `OpenTelemetry.Metrics`, `OpenTelemetry.Resources`
- signal rails: activity traces, metric instruments, log projection
- processor rails: simple processor, batch processor, composite processor
- processor contract: processors own signal batching, force-flush, shutdown, and disposal
- reader contract: metric readers own collection cadence and export cadence

[LOCAL_ADMISSION]:
- Runtime code emits signals through provider builders and processor chains.
- Force-flush and shutdown are drain actions tied to unload receipts.
- Projection failures never mutate runtime state directly.

[RAIL_LAW]:
- Package: `OpenTelemetry`
- Owns: trace and metric provider construction
- Accept: signals project through providers
- Reject: exporter packages inside lower runtime logic
