# [RASM_APPHOST_API_RUNTIME]

Runtime APIs supply AppHost time, logging, diagnostics, cancellation, channels, dataflow, and pooling primitives.

## [1]-[SURFACES]

This table is a lookup by runtime package or platform surface.

| [INDEX] | [PACKAGE]                                      | [ASSEMBLY]                                  | [LOCAL_RAIL] |
| :-----: | :--------------------------------------------- | :------------------------------------------ | :----------- |
|   [1]   | `Microsoft.Extensions.Logging.Abstractions`    | `Microsoft.Extensions.Logging.Abstractions` | telemetry    |
|   [2]   | `NodaTime`                                     | `NodaTime`                                  | clock        |
|   [3]   | `System.Threading.Tasks.Dataflow`              | `System.Threading.Tasks.Dataflow`           | flow         |
|   [4]   | `Microsoft.Extensions.ObjectPool`              | `Microsoft.Extensions.ObjectPool`           | allocation   |
|   [5]   | BCL diagnostics                                | `System.Diagnostics.DiagnosticSource`       | telemetry    |
|   [6]   | BCL threading                                  | `System.Threading`                          | lifecycle    |

## [2]-[API_LOCATORS]

This table is a lookup by assembly.

| [INDEX] | [ASSEMBLY]                                  | [NAMESPACE]                          | [USING]                               | [API_LOCATOR] |
| :-----: | :------------------------------------------ | :----------------------------------- | :------------------------------------ | :------------ |
|   [1]   | `Microsoft.Extensions.Logging.Abstractions` | `Microsoft.Extensions.Logging`       | `Microsoft.Extensions.Logging`        | `.cache/nuget/packages/microsoft.extensions.logging.abstractions/` |
|   [2]   | `NodaTime`                                  | `NodaTime`                           | `NodaTime`                            | `.cache/nuget/packages/nodatime/` |
|   [3]   | `System.Threading.Tasks.Dataflow`           | `System.Threading.Tasks.Dataflow`    | `System.Threading.Tasks.Dataflow`     | `.cache/nuget/packages/system.threading.tasks.dataflow/` |
|   [4]   | `Microsoft.Extensions.ObjectPool`           | `Microsoft.Extensions.ObjectPool`    | `Microsoft.Extensions.ObjectPool`     | `.cache/nuget/packages/microsoft.extensions.objectpool/` |
|   [5]   | `System.Diagnostics.DiagnosticSource`       | `System.Diagnostics`                 | `System.Diagnostics`                  | shared framework |
|   [6]   | `System.Threading`                          | `System.Threading`                   | `System.Threading`                    | shared framework |

## [3]-[CAPABILITIES]

This table is a lookup by type family.

| [INDEX] | [TYPE_FAMILY]                       | [ENTRY_SURFACE]                     | [LOCAL_RAIL] |
| :-----: | :---------------------------------- | :---------------------------------- | :----------- |
|   [1]   | `ILogger<T>`                        | structured log event                | telemetry    |
|   [2]   | `Instant`                           | semantic timestamp                  | clock        |
|   [3]   | `TimeProvider`                      | elapsed time and deadlines          | lifecycle    |
|   [4]   | `CancellationTokenSource`           | timed cancellation scope            | lifecycle    |
|   [5]   | `ActivitySource`                    | activity identity                   | telemetry    |
|   [6]   | `Meter`                             | counter and histogram identity      | telemetry    |
|   [7]   | `Channel<T>`                        | bounded producer-consumer flow      | flow         |
|   [8]   | `BufferBlock<T>`                    | staged topology boundary            | flow         |
|   [9]   | `ObjectPool<T>`                     | pooled allocation family            | allocation   |

## [4]-[REJECTED]

This table is a lookup by rejected API.

| [INDEX] | [REJECT]                              | [LOCAL_RAIL] | [REASON]                  |
| :-----: | :------------------------------------ | :----------- | :------------------------ |
|   [1]   | `OpenTelemetry.Instrumentation.Process` | telemetry    | process metrics are AppHost meters |
