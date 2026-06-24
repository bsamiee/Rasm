# [RASM_COMPUTE_API_RECYCLABLE_STREAM]

`Microsoft.IO.RecyclableMemoryStream` supplies the pooled byte-buffer staging
substrate for the Compute remote and model lanes: a `RecyclableMemoryStreamManager`
owning small-block and large-buffer pools, a `RecyclableMemoryStream` that is BOTH a
`MemoryStream` and an `IBufferWriter<byte>` (so it is a zero-copy serialization sink
and a zero-copy `ReadOnlySequence<byte>` source), an `Options` capacity/telemetry
policy, and a full lifecycle event + ETW counter surface. It is the buffer face the
`Runtime/channels#ARTIFACT_SYNC` `FrameEdge` fold writes into and the
`api-protobuf` `MessageExtensions.WriteTo(IBufferWriter<byte>)` / `ParseFrom(ReadOnlySequence<byte>)`
fast path composes against; its pool events fold to the staging-evidence sink and the
staged bytes are content-keyed by the suite `XxHash128` law (`api-hashing`). This page
is HOST-LOCAL and carries no TS_PROJECTION.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.IO.RecyclableMemoryStream`
- package: `Microsoft.IO.RecyclableMemoryStream` (version 3.0.1, direct pin)
- license: MIT (`microsoft/Microsoft.IO.RecyclableMemoryStream`)
- assembly: `Microsoft.IO.RecyclableMemoryStream`
- managed-tfm: multi-target (`net6.0`, `netstandard2.0`, `netstandard2.1`); the `net10.0` consumer binds `lib/net6.0`
- namespace: `Microsoft.IO`
- asset: managed runtime library; emits ETW under `EventSource` name `Microsoft-IO-RecyclableMemoryStream`
- abi: `RecyclableMemoryStream : MemoryStream, IBufferWriter<byte>` — the `IBufferWriter<byte>` implementation is the load-bearing serialization-sink contract; a consumer that only treats it as a `MemoryStream` misses the zero-copy face
- rail: staging

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: stream ownership and policy
- rail: staging

| [INDEX] | [SYMBOL]                                | [PACKAGE_ROLE] | [CAPABILITY]                                                              |
| :-----: | :-------------------------------------- | :------------- | :----------------------------------------------------------------------- |
|  [01]   | `RecyclableMemoryStreamManager`         | pool owner     | owns the small-block + large-buffer pools and the 14-overload `GetStream` factory |
|  [02]   | `RecyclableMemoryStream`                | pooled stream  | `MemoryStream` + `IBufferWriter<byte>`; the staging byte carrier and zero-copy sink/source |
|  [03]   | `RecyclableMemoryStreamManager.Options` | policy object  | block size, large-buffer multiple, pool caps, capacity cap, and debug/zeroing flags |

[PUBLIC_TYPE_SCOPE]: lifecycle event payloads
- rail: staging
- note: each rides a `RecyclableMemoryStreamManager.EventHandler<...>?` event; the same facts mirror to ETW via `Events.Writer`.

| [INDEX] | [SYMBOL]                          | [PACKAGE_ROLE] | [CAPABILITY]                                  |
| :-----: | :-------------------------------- | :------------- | :-------------------------------------------- |
|  [01]   | `StreamCreatedEventArgs`          | event payload  | a stream was rented (id, tag, requested size) |
|  [02]   | `StreamDisposedEventArgs`         | event payload  | a stream returned to the pool                 |
|  [03]   | `StreamDoubleDisposedEventArgs`   | event payload  | a double-dispose leak signal (allocation stack when traced) |
|  [04]   | `StreamFinalizedEventArgs`        | event payload  | a stream was finalized without disposal       |
|  [05]   | `StreamConvertedToArrayEventArgs` | event payload  | a `ToArray`/`GetBuffer` edge copy occurred    |
|  [06]   | `StreamOverCapacityEventArgs`     | event payload  | a write exceeded `MaximumStreamCapacity`      |
|  [07]   | `BlockCreatedEventArgs`           | event payload  | a small-pool block was allocated              |
|  [08]   | `LargeBufferCreatedEventArgs`     | event payload  | a large-pool buffer was allocated             |
|  [09]   | `BufferDiscardedEventArgs`        | event payload  | a buffer was discarded over the pool cap (reason) |
|  [10]   | `UsageReportEventArgs`            | event payload  | periodic pool-usage snapshot                  |
|  [11]   | `StreamLengthEventArgs`           | event payload  | final stream length at disposal               |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `RecyclableMemoryStreamManager` factory and configuration
- rail: staging
- note: one manager is a process-singleton owning the pools; construct it once with an `Options` policy and rent every stream from it.

| [INDEX] | [SURFACE]                                  | [CALL_SHAPE]                                            | [CAPABILITY]                              |
| :-----: | :----------------------------------------- | :------------------------------------------------------ | :---------------------------------------- |
|  [01]   | `new RecyclableMemoryStreamManager()`      | `RecyclableMemoryStreamManager()`                       | manager with default `Options`            |
|  [02]   | `new RecyclableMemoryStreamManager(Options)` | `RecyclableMemoryStreamManager(Options options)`      | manager with an explicit capacity/telemetry policy |
|  [03]   | `Settings`                                 | `Options Settings { get; }`                             | the live `Options` reference (read policy back) |

[ENTRYPOINT_SCOPE]: `GetStream` overloads
- rail: staging#STREAM_POOL
- note: the full rent surface; `requiredSize` pre-grows the stream, `asContiguousBuffer` forces a single large buffer, and the byte-seed overloads rent a stream pre-filled from a source buffer. The `Guid id` / `string? tag` arguments are the telemetry correlation keys that flow into every lifecycle event.

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

[ENTRYPOINT_SCOPE]: `RecyclableMemoryStream` zero-copy sink and source
- rail: staging#STREAM_POOL
- note: this is the integration surface — `IBufferWriter<byte>` makes the stream a direct serialization sink, `GetReadOnlySequence` makes it a fragmented zero-copy source, and the `SafeRead*` family reads without mutating the shared `Position`. `GetBuffer`/`ToArray` are edge copies that fire `StreamConvertedToArray`.

| [INDEX] | [MEMBER]                       | [SIGNATURE]                                                              | [CAPABILITY]                                                  |
| :-----: | :----------------------------- | :----------------------------------------------------------------------- | :----------------------------------------------------------- |
|  [01]   | `GetSpan`                      | `Span<byte> GetSpan(int sizeHint = 0)`                                   | `IBufferWriter<byte>` write window                           |
|  [02]   | `GetMemory`                    | `Memory<byte> GetMemory(int sizeHint = 0)`                               | `IBufferWriter<byte>` write window (memory form)             |
|  [03]   | `Advance`                      | `void Advance(int count)`                                                | commits `count` written bytes                                |
|  [04]   | `GetReadOnlySequence`          | `ReadOnlySequence<byte> GetReadOnlySequence()`                           | zero-copy fragmented view over the staged blocks — feeds `MessageParser.ParseFrom(ReadOnlySequence)` |
|  [05]   | `Write`                        | `void Write(ReadOnlySpan<byte> source)` / `void Write(byte[], int, int)` | appends bytes                                                |
|  [06]   | `WriteTo`                      | `void WriteTo(Stream)` / `WriteTo(byte[], long, long, int)`              | copies staged bytes to a stream or pre-sized array          |
|  [07]   | `CopyToAsync`                  | `Task CopyToAsync(Stream, int bufferSize, CancellationToken)`           | async drain to a destination stream                          |
|  [08]   | `SafeRead`                     | `int SafeRead(Span<byte> buffer, ref long streamPosition)` / `SafeRead(byte[], int, int, ref long)` | thread-safe read passing position by ref (no shared-`Position` mutation) |
|  [09]   | `SafeReadByte`                 | `int SafeReadByte(ref long streamPosition)`                             | thread-safe single-byte read                                 |
|  [10]   | `GetBuffer`                    | `byte[] GetBuffer()`                                                     | the contiguous backing array (forces a single large buffer)  |
|  [11]   | `TryGetBuffer`                 | `bool TryGetBuffer(out ArraySegment<byte> buffer)`                       | non-copy segment when already contiguous                     |
|  [12]   | `ToArray`                      | `byte[] ToArray()`                                                       | EDGE COPY — allocates; throws if `ThrowExceptionOnToArray`   |
|  [13]   | `Capacity64`                   | `long Capacity64 { get; }`                                               | 64-bit capacity (the `int Capacity` override saturates)      |
|  [14]   | `Id` / `Tag`                   | `Guid Id { get; }` / `string? Tag { get; }`                             | the telemetry correlation keys carried into events           |

[ENTRYPOINT_SCOPE]: `Options` capacity, pool, and debug policy
- rail: staging#STREAM_POOL
- note: `MaximumSmallPoolFreeBytes`/`MaximumLargePoolFreeBytes` default to `0` (unbounded retention) — Compute staging policy MUST set bounds; `GenerateCallStacks` is debug-only.

| [INDEX] | [MEMBER]                              | [SIGNATURE]                                                                                                                              |
| :-----: | :------------------------------------ | :--------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `BlockSize`                           | `int BlockSize { get; set; }` default `131072` (128KB)                                                                                   |
|  [02]   | `LargeBufferMultiple`                 | `int LargeBufferMultiple { get; set; }` default `1048576` (1MB)                                                                          |
|  [03]   | `MaximumBufferSize`                   | `int MaximumBufferSize { get; set; }` default `134217728` (128MB)                                                                        |
|  [04]   | `MaximumSmallPoolFreeBytes`           | `long MaximumSmallPoolFreeBytes { get; set; }` default `0` (unbounded retention)                                                         |
|  [05]   | `MaximumLargePoolFreeBytes`           | `long MaximumLargePoolFreeBytes { get; set; }` default `0` (unbounded retention)                                                         |
|  [06]   | `UseExponentialLargeBuffer`           | `bool UseExponentialLargeBuffer { get; set; }` default `false` — switches large-pool bucketing linear→exponential                        |
|  [07]   | `MaximumStreamCapacity`               | `long MaximumStreamCapacity { get; set; }` default `0` (no limit) — caps a single stream; over-cap fires `StreamOverCapacity`            |
|  [08]   | `GenerateCallStacks`                  | `bool GenerateCallStacks { get; set; }` default `false` — debug-only, captures allocation/dispose stacks                                 |
|  [09]   | `AggressiveBufferReturn`              | `bool AggressiveBufferReturn { get; set; }` default `false` — returns buffers eagerly                                                    |
|  [10]   | `ThrowExceptionOnToArray`            | `bool ThrowExceptionOnToArray { get; set; }` default `false` — turns an unintended `ToArray` edge copy into a fault                      |
|  [11]   | `ZeroOutBuffer`                       | `bool ZeroOutBuffer { get; set; }` default `false` — clears blocks on allocation and return                                             |
|  [12]   | `Options()` ctor                      | `Options()`                                                                                                                              |
|  [13]   | `Options(int,int,int,long,long)` ctor | `Options(int blockSize, int largeBufferMultiple, int maximumBufferSize, long maximumSmallPoolFreeBytes, long maximumLargePoolFreeBytes)` |

[ENTRYPOINT_SCOPE]: lifecycle events and ETW counters
- rail: staging#STREAM_POOL
- note: the pool-evidence surface; subscribe these on the singleton manager and fold to the staging-evidence sink. `Events.Writer` is the process ETW source for out-of-band collectors.

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

## [04]-[IMPLEMENTATION_LAW]

[STREAM_POOL]:
- namespace: `Microsoft.IO`; one `RecyclableMemoryStreamManager` is a process-singleton owning the small-block and large-buffer pools; every staged buffer is rented through its `GetStream` family and returned by `Dispose` (`using`).
- abi: `RecyclableMemoryStream : MemoryStream, IBufferWriter<byte>` — it is simultaneously a stream (drop-in for any `Stream` API) and a buffer writer (`GetSpan`/`GetMemory`/`Advance`), and exposes a `GetReadOnlySequence()` zero-copy read view; treating it as only a `MemoryStream` forfeits the zero-copy serialization stack.
- policy: the `Options` class owns block size, large-buffer multiple, max buffer size, max stream capacity, pool retention caps, and the debug/zeroing flags; pool policy is centralized with Compute staging policy, never re-set per caller.
- telemetry: stream lifecycle, buffer allocation, discard, capacity, and usage events plus ETW via `Events.Writer`; the events carry the `GetStream` `Guid id`/`string? tag` so a leak or over-capacity event correlates to a specific staged payload.

[OPTIONS_DETAIL]:
- `MaximumSmallPoolFreeBytes` and `MaximumLargePoolFreeBytes` default to `0` (unbounded retention) — Compute staging policy MUST set bounds or the pool grows unbounded.
- `MaximumStreamCapacity` caps a single stream's growth; an over-cap write fires `StreamOverCapacity` and is the bounded-payload guard the admission rail reads.
- `ThrowExceptionOnToArray` upgrades an unintended `ToArray` edge copy from a silent allocation into a fault — set it where the lane must stay zero-copy.
- `ZeroOutBuffer` clears blocks on allocation and return; enable only when data-leak avoidance outweighs the per-cycle cost.
- `UseExponentialLargeBuffer` switches large-buffer growth from linear multiples to exponential and changes the large-pool bucket count.
- `GenerateCallStacks` is a debug-only option that captures allocation/dispose stacks for leak attribution — NEVER enable in production.

[INTEGRATION_STACK]:
- serialize sink: `api-protobuf` `MessageExtensions.WriteTo(IBufferWriter<byte>)` / `WriteLengthPrefixedTo(IBufferWriter<byte>)` writes a message body directly into a rented `RecyclableMemoryStream` with no intermediate array; the `Runtime/channels#ARTIFACT_SYNC` 64 KiB `FrameEdge` fold sizes the frame via `IMessage.CalculateSize` and stages it here.
- parse source: a received frame's `GetReadOnlySequence()` feeds `api-protobuf` `MessageParser.ParseFrom(ReadOnlySequence<byte>)` / `MergeFrom(ReadOnlySequence<byte>)`, so a fragmented pooled payload decodes without a contiguous copy.
- content key: the staged bytes are hashed by `api-hashing` (`System.IO.Hashing`) over the `GetReadOnlySequence()` view to mint the `Runtime/codecs#CONTENT_ADDRESSING` artifact identity and the per-frame integrity checksum, reading the pool in place — never a `ToArray` copy. The whole-artifact identity over a MULTI-SEGMENT sequence drains incrementally (`foreach (var seg in GetReadOnlySequence()) hasher.Append(seg.Span);` then `XxHash128.GetCurrentHashAsUInt128()`), since the static `XxHash128.HashToUInt128(ReadOnlySpan<byte>, long seed = 0)` one-shot is span-only; a single-segment view (`IsSingleSegment`) takes the `HashToUInt128(seq.FirstSpan)` fast path. Per-frame integrity is the contiguous form — `Crc32.HashToUInt32(ReadOnlySpan<byte>)` over the `ByteString`/segment span (`Crc32` carries no seed; the `Runtime/channels#ARTIFACT_FRAMES` `FrameEdge` realized call).
- evidence: the manager's `UsageReport`/`StreamDoubleDisposed`/`StreamOverCapacity`/`BufferDiscarded` events fold to the `Runtime/receipts` / `Stats` staging-evidence sink, so pool pressure and leaks become execution evidence rather than silent allocations.
- model lane: the `Model` inference/embedding lanes stage tensor and token byte buffers through the same manager, so generative-payload staging shares one pool and one telemetry stream with the remote lane.

[LOCAL_ADMISSION]:
- Compute staging uses recyclable streams for every remote and model payload byte buffer; an ad hoc `new MemoryStream()` beside the pool is the rejected form.
- `ToArray`/`GetBuffer` are explicit edge-copy operations that fire `StreamConvertedToArray` and require a receipt reason; the zero-copy path (`GetSpan`/`Advance`/`GetReadOnlySequence`) is the default.
- Pool policy is centralized with Compute staging policy through one `Options` instance on the singleton manager, never re-tuned inside callers; `SafeRead*` is the only admitted concurrent-read path.

[RAIL_LAW]:
- Package: `Microsoft.IO.RecyclableMemoryStream`
- Owns: pooled recyclable byte streams, the `IBufferWriter<byte>`/`ReadOnlySequence<byte>` zero-copy face, capacity policy, and pool lifecycle telemetry
- Accept: bounded staged byte payloads rented from the singleton manager and stacked as the `api-protobuf` serialize sink / parse source under the `XxHash128`/`Crc32` content-identity law
- Reject: unmanaged ad hoc `MemoryStream`; unbounded pools (the `0`-default retention caps left unset); silent `ToArray` edge copies on the zero-copy path; per-caller `Options` re-tuning
