# [RASM_COMPUTE_API_RECYCLABLE_STREAM]

`Microsoft.IO.RecyclableMemoryStream` supplies recyclable stream allocation,
pooled block management, stream diagnostics, capacity policy, and buffer lifetime
events for execution payload staging.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.IO.RecyclableMemoryStream`
- package: `Microsoft.IO.RecyclableMemoryStream`
- assembly: `Microsoft.IO.RecyclableMemoryStream`
- namespace: `Microsoft.IO`
- asset: runtime library
- rail: staging

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: stream ownership
- rail: staging

| [INDEX] | [SYMBOL]                                | [PACKAGE_ROLE] | [CAPABILITY]             |
| :-----: | :-------------------------------------- | :------------- | :----------------------- |
|  [01]   | `RecyclableMemoryStreamManager`         | manager        | owns stream pool         |
|  [02]   | `RecyclableMemoryStream`                | pooled stream  | carries staged bytes     |
|  [03]   | `RecyclableMemoryStreamManager.Options` | policy object  | configures pool behavior |
|  [04]   | `RecyclableMemoryStreamManager.Events`  | ETW source     | emits pool counters      |

[PUBLIC_TYPE_SCOPE]: event contracts
- rail: staging

| [INDEX] | [SYMBOL]                          | [PACKAGE_ROLE] | [CAPABILITY]             |
| :-----: | :-------------------------------- | :------------- | :----------------------- |
|  [01]   | `StreamCreatedEventArgs`          | event payload  | reports creation         |
|  [02]   | `StreamDisposedEventArgs`         | event payload  | reports disposal         |
|  [03]   | `StreamDoubleDisposedEventArgs`   | event payload  | reports double disposal  |
|  [04]   | `StreamFinalizedEventArgs`        | event payload  | reports finalization     |
|  [05]   | `StreamConvertedToArrayEventArgs` | event payload  | reports array conversion |
|  [06]   | `StreamOverCapacityEventArgs`     | event payload  | reports capacity breach  |
|  [07]   | `BlockCreatedEventArgs`           | event payload  | reports block allocation |
|  [08]   | `LargeBufferCreatedEventArgs`     | event payload  | reports large allocation |
|  [09]   | `BufferDiscardedEventArgs`        | event payload  | reports discard reason   |
|  [10]   | `UsageReportEventArgs`            | event payload  | reports pool usage       |
|  [11]   | `StreamLengthEventArgs`           | event payload  | reports stream length    |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: stream allocation
- rail: staging

| [INDEX] | [SURFACE]             | [CALL_SHAPE]  | [CAPABILITY]           |
| :-----: | :-------------------- | :------------ | :--------------------- |
|  [01]   | `GetStream`           | factory call  | rents stream           |
|  [02]   | `GetReadOnlySequence` | stream call   | exposes sequence view  |
|  [03]   | `GetBuffer`           | stream call   | exposes buffer view    |
|  [04]   | `TryGetBuffer`        | stream call   | exposes buffer segment |
|  [05]   | `ToArray`             | stream call   | copies staged bytes    |
|  [06]   | `WriteTo`             | stream call   | copies stream bytes    |
|  [07]   | `Dispose`             | lifetime call | returns stream to pool |

[ENTRYPOINT_SCOPE]: `GetStream` decompile-verified overloads
- rail: staging-and-streams#STREAM_POOL

| [INDEX] | [MEMBER]                                     | [SIGNATURE]                                                                                          |
| :-----: | :------------------------------------------- | :--------------------------------------------------------------------------------------------------- |
|  [01]   | `GetStream()`                                | `RecyclableMemoryStream GetStream()`                                                                 |
|  [02]   | `GetStream(Guid)`                            | `RecyclableMemoryStream GetStream(Guid id)`                                                          |
|  [03]   | `GetStream(string?)`                         | `RecyclableMemoryStream GetStream(string? tag)`                                                      |
|  [04]   | `GetStream(Guid,string?)`                    | `RecyclableMemoryStream GetStream(Guid id, string? tag)`                                             |
|  [05]   | `GetStream(string?,long)`                    | `RecyclableMemoryStream GetStream(string? tag, long requiredSize)`                                   |
|  [06]   | `GetStream(Guid,string?,long)`               | `RecyclableMemoryStream GetStream(Guid id, string? tag, long requiredSize)`                          |
|  [07]   | `GetStream(Guid,string?,long,bool)`          | `RecyclableMemoryStream GetStream(Guid id, string? tag, long requiredSize, bool asContiguousBuffer)` |
|  [08]   | `GetStream(string?,long,bool)`               | `RecyclableMemoryStream GetStream(string? tag, long requiredSize, bool asContiguousBuffer)`          |
|  [09]   | `GetStream(Guid,string?,byte[],int,int)`     | `RecyclableMemoryStream GetStream(Guid id, string? tag, byte[] buffer, int offset, int count)`       |
|  [10]   | `GetStream(byte[])`                          | `RecyclableMemoryStream GetStream(byte[] buffer)`                                                    |
|  [11]   | `GetStream(string?,byte[],int,int)`          | `RecyclableMemoryStream GetStream(string? tag, byte[] buffer, int offset, int count)`                |
|  [12]   | `GetStream(Guid,string?,ReadOnlySpan<byte>)` | `RecyclableMemoryStream GetStream(Guid id, string? tag, ReadOnlySpan<byte> buffer)`                  |
|  [13]   | `GetStream(ReadOnlySpan<byte>)`              | `RecyclableMemoryStream GetStream(ReadOnlySpan<byte> buffer)`                                        |
|  [14]   | `GetStream(string?,ReadOnlySpan<byte>)`      | `RecyclableMemoryStream GetStream(string? tag, ReadOnlySpan<byte> buffer)`                           |

[ENTRYPOINT_SCOPE]: pool policy and telemetry
- rail: staging

| [INDEX] | [SURFACE]                   | [CALL_SHAPE]    | [CAPABILITY]               |
| :-----: | :-------------------------- | :-------------- | :------------------------- |
|  [01]   | `BlockSize`                 | option property | sets block size            |
|  [02]   | `LargeBufferMultiple`       | option property | sets large increment       |
|  [03]   | `MaximumBufferSize`         | option property | caps buffer size           |
|  [04]   | `MaximumSmallPoolFreeBytes` | option property | caps small pool retention  |
|  [05]   | `MaximumLargePoolFreeBytes` | option property | caps large pool retention  |
|  [06]   | `MaximumStreamCapacity`     | option property | caps stream capacity       |
|  [07]   | `UseExponentialLargeBuffer` | option property | selects large growth curve |
|  [08]   | `GenerateCallStacks`        | option property | enables allocation trace   |
|  [09]   | `AggressiveBufferReturn`    | option property | returns buffers eagerly    |
|  [10]   | `ZeroOutBuffer`             | option property | clears buffers on cycle    |
|  [11]   | `ThrowExceptionOnToArray`   | option property | rejects array copy         |
|  [12]   | `UsageReport`               | manager event   | reports pool usage         |

[ENTRYPOINT_SCOPE]: `Options` class decompile-verified property signatures
- rail: staging-and-streams#STREAM_POOL

| [INDEX] | [MEMBER]                              | [SIGNATURE]                                                                                                                              |
| :-----: | :------------------------------------ | :--------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `BlockSize`                           | `int BlockSize { get; set; }` default `131072` (128KB)                                                                                   |
|  [02]   | `LargeBufferMultiple`                 | `int LargeBufferMultiple { get; set; }` default `1048576` (1MB)                                                                          |
|  [03]   | `MaximumBufferSize`                   | `int MaximumBufferSize { get; set; }` default `134217728` (128MB)                                                                        |
|  [04]   | `MaximumSmallPoolFreeBytes`           | `long MaximumSmallPoolFreeBytes { get; set; }` default `0`                                                                               |
|  [05]   | `MaximumLargePoolFreeBytes`           | `long MaximumLargePoolFreeBytes { get; set; }` default `0`                                                                               |
|  [06]   | `UseExponentialLargeBuffer`           | `bool UseExponentialLargeBuffer { get; set; }` default `false`                                                                           |
|  [07]   | `MaximumStreamCapacity`               | `long MaximumStreamCapacity { get; set; }` default `0` (no limit)                                                                        |
|  [08]   | `GenerateCallStacks`                  | `bool GenerateCallStacks { get; set; }` default `false`                                                                                  |
|  [09]   | `AggressiveBufferReturn`              | `bool AggressiveBufferReturn { get; set; }` default `false`                                                                              |
|  [10]   | `ThrowExceptionOnToArray`             | `bool ThrowExceptionOnToArray { get; set; }` default `false`                                                                             |
|  [11]   | `ZeroOutBuffer`                       | `bool ZeroOutBuffer { get; set; }` default `false`                                                                                       |
|  [12]   | `Options()` ctor                      | `Options()`                                                                                                                              |
|  [13]   | `Options(int,int,int,long,long)` ctor | `Options(int blockSize, int largeBufferMultiple, int maximumBufferSize, long maximumSmallPoolFreeBytes, long maximumLargePoolFreeBytes)` |

[ENTRYPOINT_SCOPE]: manager events and ETW counters
- rail: staging-and-streams#STREAM_POOL

| [INDEX] | [MEMBER]                 | [SIGNATURE]                                                                              |
| :-----: | :----------------------- | :--------------------------------------------------------------------------------------- |
|  [01]   | `BlockCreated`           | `event EventHandler<BlockCreatedEventArgs>? BlockCreated`                                |
|  [02]   | `LargeBufferCreated`     | `event EventHandler<LargeBufferCreatedEventArgs>? LargeBufferCreated`                    |
|  [03]   | `StreamCreated`          | `event EventHandler<StreamCreatedEventArgs>? StreamCreated`                              |
|  [04]   | `StreamDisposed`         | `event EventHandler<StreamDisposedEventArgs>? StreamDisposed`                            |
|  [05]   | `StreamDoubleDisposed`   | `event EventHandler<StreamDoubleDisposedEventArgs>? StreamDoubleDisposed`                |
|  [06]   | `StreamFinalized`        | `event EventHandler<StreamFinalizedEventArgs>? StreamFinalized`                          |
|  [07]   | `StreamLength`           | `event EventHandler<StreamLengthEventArgs>? StreamLength`                                |
|  [08]   | `StreamConvertedToArray` | `event EventHandler<StreamConvertedToArrayEventArgs>? StreamConvertedToArray`            |
|  [09]   | `StreamOverCapacity`     | `event EventHandler<StreamOverCapacityEventArgs>? StreamOverCapacity`                    |
|  [10]   | `BufferDiscarded`        | `event EventHandler<BufferDiscardedEventArgs>? BufferDiscarded`                          |
|  [11]   | `UsageReport`            | `event EventHandler<UsageReportEventArgs>? UsageReport`                                  |
|  [12]   | `Events.Writer`          | `static Events Writer` (ETW `EventSource`, name `"Microsoft-IO-RecyclableMemoryStream"`) |
|  [13]   | `Settings`               | `Options Settings { get; }` — exposes live `Options` reference                           |

## [04]-[IMPLEMENTATION_LAW]

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
