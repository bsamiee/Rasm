# [RASM_COMPUTE_API_RECYCLABLE_STREAM]

`Microsoft.IO.RecyclableMemoryStream` supplies recyclable stream allocation,
pooled block management, stream diagnostics, capacity policy, and buffer lifetime
events for execution payload staging.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.IO.RecyclableMemoryStream`
- package: `Microsoft.IO.RecyclableMemoryStream`
- assembly: `Microsoft.IO.RecyclableMemoryStream`
- namespace: `Microsoft.IO`
- asset: runtime library
- rail: staging

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: stream ownership
- rail: staging

| [INDEX] | [SYMBOL]                                | [PACKAGE_ROLE] | [CAPABILITY]             |
| :-----: | :-------------------------------------- | :------------- | :----------------------- |
|   [1]   | `RecyclableMemoryStreamManager`         | manager        | owns stream pool         |
|   [2]   | `RecyclableMemoryStream`                | pooled stream  | carries staged bytes     |
|   [3]   | `RecyclableMemoryStreamManager.Options` | policy object  | configures pool behavior |
|   [4]   | `BlockAndOffset`                        | position shape | addresses pooled blocks  |
|   [5]   | `BlockSegment`                          | segment shape  | exposes stream segment   |

[PUBLIC_TYPE_SCOPE]: event contracts
- rail: staging

| [INDEX] | [SYMBOL]                          | [PACKAGE_ROLE] | [CAPABILITY]             |
| :-----: | :-------------------------------- | :------------- | :----------------------- |
|   [1]   | `StreamCreatedEventArgs`          | event payload  | reports creation         |
|   [2]   | `StreamDisposedEventArgs`         | event payload  | reports disposal         |
|   [3]   | `StreamDoubleDisposedEventArgs`   | event payload  | reports double disposal  |
|   [4]   | `StreamFinalizedEventArgs`        | event payload  | reports finalization     |
|   [5]   | `StreamConvertedToArrayEventArgs` | event payload  | reports array conversion |
|   [6]   | `StreamOverCapacityEventArgs`     | event payload  | reports capacity breach  |
|   [7]   | `BlockCreatedEventArgs`           | event payload  | reports block allocation |
|   [8]   | `LargeBufferCreatedEventArgs`     | event payload  | reports large allocation |
|   [9]   | `BufferDiscardedEventArgs`        | event payload  | reports discard reason   |
|  [10]   | `UsageReportEventArgs`            | event payload  | reports pool usage       |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: stream allocation
- rail: staging

| [INDEX] | [SURFACE]             | [CALL_SHAPE]  | [CAPABILITY]           |
| :-----: | :-------------------- | :------------ | :--------------------- |
|   [1]   | `GetStream`           | factory call  | rents stream           |
|   [2]   | `GetReadOnlySequence` | stream call   | exposes sequence view  |
|   [3]   | `GetBuffer`           | stream call   | exposes buffer view    |
|   [4]   | `TryGetBuffer`        | stream call   | exposes buffer segment |
|   [5]   | `ToArray`             | stream call   | copies staged bytes    |
|   [6]   | `WriteTo`             | stream call   | copies stream bytes    |
|   [7]   | `Dispose`             | lifetime call | returns stream to pool |

[ENTRYPOINT_SCOPE]: pool policy and telemetry
- rail: staging

| [INDEX] | [SURFACE]                 | [CALL_SHAPE]    | [CAPABILITY]             |
| :-----: | :------------------------ | :-------------- | :----------------------- |
|   [1]   | `BlockSize`               | option property | sets block size          |
|   [2]   | `LargeBufferMultiple`     | option property | sets large increment     |
|   [3]   | `MaximumBufferSize`       | option property | caps buffer size         |
|   [4]   | `MaximumStreamCapacity`   | option property | caps stream capacity     |
|   [5]   | `GenerateCallStacks`      | option property | enables allocation trace |
|   [6]   | `AggressiveBufferReturn`  | option property | returns buffers eagerly  |
|   [7]   | `ThrowExceptionOnToArray` | option property | rejects array copy       |
|   [8]   | `ReportUsage`             | manager call    | emits usage event        |

## [4]-[IMPLEMENTATION_LAW]

[STREAM_POOL]:
- namespace: `Microsoft.IO`
- manager: `RecyclableMemoryStreamManager`
- stream: `RecyclableMemoryStream`
- policy: block size, large buffer multiple, max buffer size, max stream capacity
- telemetry: stream lifecycle, buffer allocation, discard, capacity, and usage events

[LOCAL_ADMISSION]:
- Compute staging uses recyclable streams for remote and model payload byte buffers.
- `ToArray` is an explicit edge-copy operation and requires a receipt reason.
- Stream events feed execution evidence and leak diagnostics.
- Pool policy is centralized with Compute staging policy, not hidden inside callers.

[RAIL_LAW]:
- Package: `Microsoft.IO.RecyclableMemoryStream`
- Owns: recyclable byte streams and pool diagnostics
- Accept: bounded staged byte payloads
- Reject: unmanaged ad hoc memory streams
