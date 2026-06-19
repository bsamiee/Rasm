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
|   [4]   | `RecyclableMemoryStreamManager.Events`  | ETW source     | emits pool counters      |

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
|  [11]   | `StreamLengthEventArgs`           | event payload  | reports stream length    |

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

[ENTRYPOINT_SCOPE]: `GetStream` decompile-verified overloads
- rail: staging-and-streams#STREAM_POOL

| [INDEX] | [MEMBER]                                     | [SIGNATURE]                                                                                          |
| :-----: | :------------------------------------------- | :--------------------------------------------------------------------------------------------------- |
|   [1]   | `GetStream()`                                | `RecyclableMemoryStream GetStream()`                                                                 |
|   [2]   | `GetStream(Guid)`                            | `RecyclableMemoryStream GetStream(Guid id)`                                                          |
|   [3]   | `GetStream(string?)`                         | `RecyclableMemoryStream GetStream(string? tag)`                                                      |
|   [4]   | `GetStream(Guid,string?)`                    | `RecyclableMemoryStream GetStream(Guid id, string? tag)`                                             |
|   [5]   | `GetStream(string?,long)`                    | `RecyclableMemoryStream GetStream(string? tag, long requiredSize)`                                   |
|   [6]   | `GetStream(Guid,string?,long)`               | `RecyclableMemoryStream GetStream(Guid id, string? tag, long requiredSize)`                          |
|   [7]   | `GetStream(Guid,string?,long,bool)`          | `RecyclableMemoryStream GetStream(Guid id, string? tag, long requiredSize, bool asContiguousBuffer)` |
|   [8]   | `GetStream(string?,long,bool)`               | `RecyclableMemoryStream GetStream(string? tag, long requiredSize, bool asContiguousBuffer)`          |
|   [9]   | `GetStream(Guid,string?,byte[],int,int)`     | `RecyclableMemoryStream GetStream(Guid id, string? tag, byte[] buffer, int offset, int count)`       |
|  [10]   | `GetStream(byte[])`                          | `RecyclableMemoryStream GetStream(byte[] buffer)`                                                    |
|  [11]   | `GetStream(string?,byte[],int,int)`          | `RecyclableMemoryStream GetStream(string? tag, byte[] buffer, int offset, int count)`                |
|  [12]   | `GetStream(Guid,string?,ReadOnlySpan<byte>)` | `RecyclableMemoryStream GetStream(Guid id, string? tag, ReadOnlySpan<byte> buffer)`                  |
|  [13]   | `GetStream(ReadOnlySpan<byte>)`              | `RecyclableMemoryStream GetStream(ReadOnlySpan<byte> buffer)`                                        |
|  [14]   | `GetStream(string?,ReadOnlySpan<byte>)`      | `RecyclableMemoryStream GetStream(string? tag, ReadOnlySpan<byte> buffer)`                           |

[ENTRYPOINT_SCOPE]: pool policy and telemetry
- rail: staging

| [INDEX] | [SURFACE]                   | [CALL_SHAPE]    | [CAPABILITY]               |
| :-----: | :-------------------------- | :-------------- | :------------------------- |
|   [1]   | `BlockSize`                 | option property | sets block size            |
|   [2]   | `LargeBufferMultiple`       | option property | sets large increment       |
|   [3]   | `MaximumBufferSize`         | option property | caps buffer size           |
|   [4]   | `MaximumSmallPoolFreeBytes` | option property | caps small pool retention  |
|   [5]   | `MaximumLargePoolFreeBytes` | option property | caps large pool retention  |
|   [6]   | `MaximumStreamCapacity`     | option property | caps stream capacity       |
|   [7]   | `UseExponentialLargeBuffer` | option property | selects large growth curve |
|   [8]   | `GenerateCallStacks`        | option property | enables allocation trace   |
|   [9]   | `AggressiveBufferReturn`    | option property | returns buffers eagerly    |
|  [10]   | `ZeroOutBuffer`             | option property | clears buffers on cycle    |
|  [11]   | `ThrowExceptionOnToArray`   | option property | rejects array copy         |
|  [12]   | `UsageReport`               | manager event   | reports pool usage         |

[ENTRYPOINT_SCOPE]: `Options` class decompile-verified property signatures
- rail: staging-and-streams#STREAM_POOL

| [INDEX] | [MEMBER]                              | [SIGNATURE]                                                                                                                              |
| :-----: | :------------------------------------ | :--------------------------------------------------------------------------------------------------------------------------------------- |
|   [1]   | `BlockSize`                           | `int BlockSize { get; set; }` default `131072` (128KB)                                                                                   |
|   [2]   | `LargeBufferMultiple`                 | `int LargeBufferMultiple { get; set; }` default `1048576` (1MB)                                                                          |
|   [3]   | `MaximumBufferSize`                   | `int MaximumBufferSize { get; set; }` default `134217728` (128MB)                                                                        |
|   [4]   | `MaximumSmallPoolFreeBytes`           | `long MaximumSmallPoolFreeBytes { get; set; }` default `0`                                                                               |
|   [5]   | `MaximumLargePoolFreeBytes`           | `long MaximumLargePoolFreeBytes { get; set; }` default `0`                                                                               |
|   [6]   | `UseExponentialLargeBuffer`           | `bool UseExponentialLargeBuffer { get; set; }` default `false`                                                                           |
|   [7]   | `MaximumStreamCapacity`               | `long MaximumStreamCapacity { get; set; }` default `0` (no limit)                                                                        |
|   [8]   | `GenerateCallStacks`                  | `bool GenerateCallStacks { get; set; }` default `false`                                                                                  |
|   [9]   | `AggressiveBufferReturn`              | `bool AggressiveBufferReturn { get; set; }` default `false`                                                                              |
|  [10]   | `ThrowExceptionOnToArray`             | `bool ThrowExceptionOnToArray { get; set; }` default `false`                                                                             |
|  [11]   | `ZeroOutBuffer`                       | `bool ZeroOutBuffer { get; set; }` default `false`                                                                                       |
|  [12]   | `Options()` ctor                      | `Options()`                                                                                                                              |
|  [13]   | `Options(int,int,int,long,long)` ctor | `Options(int blockSize, int largeBufferMultiple, int maximumBufferSize, long maximumSmallPoolFreeBytes, long maximumLargePoolFreeBytes)` |

[ENTRYPOINT_SCOPE]: manager events and ETW counters
- rail: staging-and-streams#STREAM_POOL

| [INDEX] | [MEMBER]                 | [SIGNATURE]                                                                              |
| :-----: | :----------------------- | :--------------------------------------------------------------------------------------- |
|   [1]   | `BlockCreated`           | `event EventHandler<BlockCreatedEventArgs>? BlockCreated`                                |
|   [2]   | `LargeBufferCreated`     | `event EventHandler<LargeBufferCreatedEventArgs>? LargeBufferCreated`                    |
|   [3]   | `StreamCreated`          | `event EventHandler<StreamCreatedEventArgs>? StreamCreated`                              |
|   [4]   | `StreamDisposed`         | `event EventHandler<StreamDisposedEventArgs>? StreamDisposed`                            |
|   [5]   | `StreamDoubleDisposed`   | `event EventHandler<StreamDoubleDisposedEventArgs>? StreamDoubleDisposed`                |
|   [6]   | `StreamFinalized`        | `event EventHandler<StreamFinalizedEventArgs>? StreamFinalized`                          |
|   [7]   | `StreamLength`           | `event EventHandler<StreamLengthEventArgs>? StreamLength`                                |
|   [8]   | `StreamConvertedToArray` | `event EventHandler<StreamConvertedToArrayEventArgs>? StreamConvertedToArray`            |
|   [9]   | `StreamOverCapacity`     | `event EventHandler<StreamOverCapacityEventArgs>? StreamOverCapacity`                    |
|  [10]   | `BufferDiscarded`        | `event EventHandler<BufferDiscardedEventArgs>? BufferDiscarded`                          |
|  [11]   | `UsageReport`            | `event EventHandler<UsageReportEventArgs>? UsageReport`                                  |
|  [12]   | `Events.Writer`          | `static Events Writer` (ETW `EventSource`, name `"Microsoft-IO-RecyclableMemoryStream"`) |
|  [13]   | `Settings`               | `Options Settings { get; }` — exposes live `Options` reference                           |

## [4]-[IMPLEMENTATION_LAW]

[STREAM_POOL]:
- namespace: `Microsoft.IO`
- manager: `RecyclableMemoryStreamManager`
- stream: `RecyclableMemoryStream`
- policy: `Options` class with block size, large buffer multiple, max buffer size, max stream capacity
- telemetry: stream lifecycle, buffer allocation, discard, capacity, and usage events plus ETW via `Events.Writer`

[OPTIONS_DETAIL]:
- `MaximumSmallPoolFreeBytes` and `MaximumLargePoolFreeBytes` default to `0` — callers must set reasonable bounds or the pool grows unbounded
- `ZeroOutBuffer` clears blocks on allocation and return; only use when data-leak avoidance outweighs the perf cost
- `UseExponentialLargeBuffer` switches large buffer growth from linear multiples to exponential; changes the `largePools` bucket count
- `GenerateCallStacks` is a debug-only option — NEVER enable in production

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
