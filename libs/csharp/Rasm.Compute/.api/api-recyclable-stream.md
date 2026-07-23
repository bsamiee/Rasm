# [RASM_COMPUTE_API_RECYCLABLE_STREAM]

`Microsoft.IO.RecyclableMemoryStream` owns pooled byte-buffer staging for the Compute remote and model lanes: a `RecyclableMemoryStreamManager` renting from small-block and large-buffer pools, and a `RecyclableMemoryStream` that is at once a `MemoryStream` and an `IBufferWriter<byte>` — a zero-copy serialization sink and a `ReadOnlySequence<byte>` source. Capacity and retention ride one `Options` policy, and a lifecycle-event and ETW-counter surface turns pool pressure and leaks into staging evidence.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.IO.RecyclableMemoryStream`
- package: `Microsoft.IO.RecyclableMemoryStream` (MIT)
- assembly: `Microsoft.IO.RecyclableMemoryStream`
- namespace: `Microsoft.IO`
- abi: `RecyclableMemoryStream : MemoryStream, IBufferWriter<byte>` — the buffer-writer face is the load-bearing serialization-sink contract
- asset: emits ETW under `EventSource` name `Microsoft-IO-RecyclableMemoryStream`
- rail: staging

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: stream ownership and policy

| [INDEX] | [SYMBOL]                                | [TYPE_FAMILY] | [CAPABILITY]                                            |
| :-----: | :-------------------------------------- | :------------ | :------------------------------------------------------ |
|  [01]   | `RecyclableMemoryStreamManager`         | class         | small-block + large-buffer pool owner; the rent factory |
|  [02]   | `RecyclableMemoryStream`                | sealed class  | staging byte carrier; zero-copy sink and source         |
|  [03]   | `RecyclableMemoryStreamManager.Options` | class         | block size, buffer multiple, pool/capacity caps, flags  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `RecyclableMemoryStreamManager` factory and configuration

One manager is a process-singleton owning the pools; construct it once with an `Options` policy and rent every stream from it.

| [INDEX] | [SURFACE]                                | [SHAPE]  | [CAPABILITY]                       |
| :-----: | :--------------------------------------- | :------- | :--------------------------------- |
|  [01]   | `RecyclableMemoryStreamManager()`        | ctor     | manager with default `Options`     |
|  [02]   | `RecyclableMemoryStreamManager(Options)` | ctor     | explicit capacity/telemetry policy |
|  [03]   | `Settings -> Options`                    | property | live `Options`, read policy back   |

[ENTRYPOINT_SCOPE]: `GetStream` rent surface

Every overload returns `RecyclableMemoryStream`, the sole public rent path — there is no public ctor and no `Default` static, so a stream always binds an explicit manager. `Guid id` and `string? tag` are the telemetry correlation keys flowing into every lifecycle event, `requiredSize` pre-grows, `asContiguousBuffer` forces one large buffer, and the `byte[]`/`ReadOnlySpan<byte>` seeds rent a stream pre-filled from a source.

| [INDEX] | [ARGS]                                                               |
| :-----: | :------------------------------------------------------------------- |
|  [01]   | `()`                                                                 |
|  [02]   | `(Guid id)`                                                          |
|  [03]   | `(string? tag)`                                                      |
|  [04]   | `(Guid id, string? tag)`                                             |
|  [05]   | `(string? tag, long requiredSize)`                                   |
|  [06]   | `(Guid id, string? tag, long requiredSize)`                          |
|  [07]   | `(Guid id, string? tag, long requiredSize, bool asContiguousBuffer)` |
|  [08]   | `(string? tag, long requiredSize, bool asContiguousBuffer)`          |
|  [09]   | `(Guid id, string? tag, byte[] buffer, int offset, int count)`       |
|  [10]   | `(byte[] buffer)`                                                    |
|  [11]   | `(string? tag, byte[] buffer, int offset, int count)`                |
|  [12]   | `(Guid id, string? tag, ReadOnlySpan<byte> buffer)`                  |
|  [13]   | `(ReadOnlySpan<byte> buffer)`                                        |
|  [14]   | `(string? tag, ReadOnlySpan<byte> buffer)`                           |

[ENTRYPOINT_SCOPE]: `RecyclableMemoryStream` zero-copy sink and source

`IBufferWriter<byte>` (`GetSpan`/`GetMemory`/`Advance`) makes the stream a direct serialization sink and `GetReadOnlySequence()` a fragmented zero-copy source; `SafeRead*` reads without mutating the shared `Position`, and `GetBuffer`/`ToArray` are edge copies that fire `StreamConvertedToArray`.

| [INDEX] | [SIGNATURE]                                                  | [CAPABILITY]                                       |
| :-----: | :----------------------------------------------------------- | :------------------------------------------------- |
|  [01]   | `Span<byte> GetSpan(int sizeHint = 0)`                       | `IBufferWriter` write window                       |
|  [02]   | `Memory<byte> GetMemory(int sizeHint = 0)`                   | `IBufferWriter` write window (memory form)         |
|  [03]   | `void Advance(int count)`                                    | commits `count` written bytes                      |
|  [04]   | `ReadOnlySequence<byte> GetReadOnlySequence()`               | zero-copy fragmented `ParseFrom` source            |
|  [05]   | `void Write(ReadOnlySpan<byte>)` / `Write(byte[], int, int)` | appends bytes                                      |
|  [06]   | `void WriteTo(Stream)` / `WriteTo(byte[], long, long, int)`  | copies staged bytes to a stream or pre-sized array |
|  [07]   | `Task CopyToAsync(Stream, int, CancellationToken)`           | async drain to a destination stream                |
|  [08]   | `int SafeRead(Span<byte>, ref long)`                         | thread-safe read, position by ref                  |
|  [09]   | `int SafeRead(byte[], int, int, ref long)`                   | thread-safe read (offset/count), position by ref   |
|  [10]   | `int SafeReadByte(ref long)`                                 | thread-safe single-byte read                       |
|  [11]   | `byte[] GetBuffer()`                                         | contiguous backing array (forces one large buffer) |
|  [12]   | `bool TryGetBuffer(out ArraySegment<byte>)`                  | non-copy segment when already contiguous           |
|  [13]   | `byte[] ToArray()`                                           | edge copy; throws if `ThrowExceptionOnToArray`     |
|  [14]   | `long Capacity64 { get; }`                                   | 64-bit capacity (`int Capacity` saturates)         |

[ENTRYPOINT_SCOPE]: `Options` capacity, pool, and debug policy

Every member is a `{ get; set; }` property; `Options()` is the default ctor and `Options(int, int, int, long, long)` sets rows [01]-[05] positionally.

| [INDEX] | [MEMBER]                    | [TYPE] | [DEFAULT]           |
| :-----: | :-------------------------- | :----- | :------------------ |
|  [01]   | `BlockSize`                 | `int`  | `131072` (128KB)    |
|  [02]   | `LargeBufferMultiple`       | `int`  | `1048576` (1MB)     |
|  [03]   | `MaximumBufferSize`         | `int`  | `134217728` (128MB) |
|  [04]   | `MaximumSmallPoolFreeBytes` | `long` | `0` (unbounded)     |
|  [05]   | `MaximumLargePoolFreeBytes` | `long` | `0` (unbounded)     |
|  [06]   | `UseExponentialLargeBuffer` | `bool` | `false`             |
|  [07]   | `MaximumStreamCapacity`     | `long` | `0` (no limit)      |
|  [08]   | `GenerateCallStacks`        | `bool` | `false`             |
|  [09]   | `AggressiveBufferReturn`    | `bool` | `false`             |
|  [10]   | `ThrowExceptionOnToArray`   | `bool` | `false`             |
|  [11]   | `ZeroOutBuffer`             | `bool` | `false`             |

[ENTRYPOINT_SCOPE]: lifecycle events and pool telemetry

Each event is `event EventHandler<<Event>EventArgs>?` on the singleton manager; `Events.Writer` is the static ETW `EventSource` for out-of-band collectors. Subscribe on the manager and fold to the staging-evidence sink.

| [INDEX] | [EVENT]                  | [SIGNALS]                       |
| :-----: | :----------------------- | :------------------------------ |
|  [01]   | `BlockCreated`           | small-pool block allocated      |
|  [02]   | `LargeBufferCreated`     | large-pool buffer allocated     |
|  [03]   | `StreamCreated`          | stream rented and correlated    |
|  [04]   | `StreamDisposed`         | stream returned to the pool     |
|  [05]   | `StreamDoubleDisposed`   | double-dispose leak signal      |
|  [06]   | `StreamFinalized`        | finalized without disposal      |
|  [07]   | `StreamLength`           | final length at disposal        |
|  [08]   | `StreamConvertedToArray` | `ToArray`/`GetBuffer` edge copy |
|  [09]   | `StreamOverCapacity`     | write exceeded capacity cap     |
|  [10]   | `BufferDiscarded`        | buffer discarded over cap       |
|  [11]   | `UsageReport`            | periodic pool-usage snapshot    |

[RecyclableMemoryStreamManager]: live pool metrics — `SmallPoolInUseSize` `SmallPoolFreeSize` `LargePoolInUseSize` `LargePoolFreeSize` `SmallBlocksFree` `LargeBuffersFree`.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- One `RecyclableMemoryStreamManager` is a process-singleton owning the small-block and large-buffer pools; every staged buffer rents through `GetStream` and returns on `Dispose`.
- `RecyclableMemoryStream` is at once a `MemoryStream` and an `IBufferWriter<byte>` carrying a `GetReadOnlySequence()` read view; consuming it as only a `MemoryStream` forfeits the zero-copy serialization stack.
- `Options` centralizes block size, buffer multiple, capacity and pool-retention caps, and debug/zeroing flags with Compute staging policy, never re-set per caller; `MaximumSmallPoolFreeBytes` and `MaximumLargePoolFreeBytes` default to `0` (unbounded retention), so staging policy binds explicit caps.
- `MaximumStreamCapacity` caps a single stream and fires `StreamOverCapacity` — the bounded-payload guard the admission rail reads; `ThrowExceptionOnToArray` upgrades an unintended `ToArray` edge copy to a fault; `GenerateCallStacks` captures allocation/dispose stacks for leak attribution under debug alone.
- Lifecycle, allocation, discard, capacity, and usage events with ETW via `Events.Writer` carry the `GetStream` `Guid id`/`string? tag`, correlating a leak or over-capacity signal to a specific staged payload.

[STACKING]:
- `System.IO.Hashing`(`.api/api-hashing.md`): content-keys the staged bytes over the `GetReadOnlySequence()` view in place — `XxHash128` incremental `Append`/`GetCurrentHashAsUInt128` over a multi-segment sequence, the `HashToUInt128(ReadOnlySpan<byte>)` fast path when `IsSingleSegment`, and `Crc32.HashToUInt32(ReadOnlySpan<byte>)` for the per-frame checksum, minting the `Runtime/codecs` content-addressing identity with no `ToArray` copy.
- `Google.Protobuf`(`.api/api-protobuf.md`): serialize sink — `MessageExtensions.WriteTo(IBufferWriter<byte>)` / `WriteLengthPrefixedTo(IBufferWriter<byte>)` writes a message body into the rented stream with no intermediate array; parse source — a received frame's `GetReadOnlySequence()` feeds `MessageParser.ParseFrom(ReadOnlySequence<byte>)` / `MergeFrom(ReadOnlySequence<byte>)`, decoding a fragmented pooled payload without a contiguous copy.
- within-lib: the `Runtime/transport` `FrameEdge` fold sizes each frame via `IMessage.CalculateSize` and stages it here; the manager's `UsageReport`/`StreamDoubleDisposed`/`StreamOverCapacity`/`BufferDiscarded` events fold to the `Runtime/receipts` staging-evidence sink; the `Model` inference and embedding lanes stage tensor and token buffers through the same manager, sharing one pool and telemetry stream with the remote lane.

[LOCAL_ADMISSION]:
- Every Compute remote and model payload byte buffer rents from the singleton manager; `ToArray`/`GetBuffer` are explicit edge copies that fire `StreamConvertedToArray` and carry a receipt reason, the `GetSpan`/`Advance`/`GetReadOnlySequence` zero-copy path the default.
- `SafeRead`/`SafeReadByte` are the only admitted concurrent-read path, taking the position by `ref` without touching the shared `Position`.

[RAIL_LAW]:
- Package: `Microsoft.IO.RecyclableMemoryStream`
- Owns: pooled recyclable byte streams, the `IBufferWriter<byte>`/`ReadOnlySequence<byte>` zero-copy face, capacity policy, and pool-lifecycle telemetry
- Accept: bounded staged payloads rented from the singleton manager, stacked as the `Google.Protobuf` serialize sink and parse source under the `XxHash128`/`Crc32` content-identity law
- Reject: an ad hoc `MemoryStream`; unbounded pools with retention caps left at the `0` default; a silent `ToArray` edge copy on the zero-copy path; per-caller `Options` re-tuning
