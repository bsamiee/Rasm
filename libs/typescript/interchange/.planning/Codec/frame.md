# [INTERCHANGE_FRAME]

Content-addressed artifact-frame reassembly: the framing fold lifted off `Transport/transport.md` is stitched into one content-addressed blob through a per-frame Crc32 verify, a single pre-sized sink, and a whole-artifact 128-bit content-key derivation, run off the main thread under the `platform` `DecodeWorkerPool`. `ArtifactFrameRail` composes the `Codec/codec.md` `DecodeRail` decode discipline over the frame bytes the `proto` codec row admits — the artifact frame is not a fourth `CodecKey`, it is the reassembly owner of the server-streamed frames the proto row decodes; this page owns the reassembly, the owned table-driven Crc32, and the transferable-stream worker boundary. The content key is the one 16-byte `ContentKey` brand byte-identical to the C#-owned `XxHash128` seed; the browser-side 128-bit provider is the admitted `hash-wasm` `createXXHash128(0, 0)` `IHasher` digesting to a 16-byte `Uint8Array` (`digest("binary")`), so the provider question is settled at the package and the cross-runtime byte-identity assertion is realized at `Codec/parity#CONTENT_KEY_PARITY` against the FROZEN `CANONICAL_BYTE_IDENTITY` corpus fixture (the `[4]-[RESEARCH]` `CONTENT_HASHING` endianness normalization the parity binding reproduces).

## [1]-[INDEX]

- [1]-[FRAME_RAIL]: the reassembly, the owned Crc32, the transferable worker boundary.
- [2]-[TS_PROJECTION]: the artifact-frame wire shape the rail reassembles.


## [2]-[FRAME_RAIL]

- Owner: `ArtifactFrameRail`, the row that reassembles the server-streamed `ArtifactFrameWire` frames named on `csharp:Rasm.Compute/Runtime/channels#TS_PROJECTION` (the `Generate`/`SubtreeFetch` server-stream artifact-delivery path and the GLB tessellation result re-entering through the remote lane) into one content-addressed blob, plus the one owned `Crc32` table-driven rail the whole branch reads. Each frame carries `artifactId`, `artifactBytes`, `offset`, the `frameCrc` `fixed32`, and the `payload` `Uint8Array`.
- Cases: the rail verifies the per-frame Crc32 through the owned `Crc32` rail (a direct number-eq compare against the wire `frameCrc fixed32`, no package), allocates one pre-sized `Uint8Array` of `artifactBytes` length at the first frame and writes each payload by `offset` into that single owned sink, and on the final frame derives the whole-artifact content key over the assembled bytes through the branch-owned `XxHash128` interface (the `[4]-[RESEARCH]` `CONTENT_HASHING` row owns its provider and the `Codec/parity#CONTENT_KEY_PARITY` reproduction binding), so a re-fetch of identical content keys identically and a frame whose Crc32 or assembled digest mismatches faults through `Ingress/fault.md` rather than yielding a torn blob. The bidi `ArtifactSyncShape.sync` method stays structurally excluded on the grpc-web row, but the frame TYPE is browser-reachable over the server-stream and lands here.
- Entry: the rail OWNS the pure reassembly algebra — the `reassemble(crc, xxh, frames)` fold, the `transferFrames` projection, the table-driven `Crc32`, and the `xxHash128Of` `IHasher` binding — and lifts the `Transport/transport.md` framing fold output `Stream<ArtifactFrameWire, FaultDetail>` across the worker boundary as a transferable seam. `transferFrames` projects the Effect `Stream` to a native `ReadableStream<ArtifactFrameWire>` through the verified `Stream.toReadableStream` destructor (data-last, `strategy` carrying the one-frame-window backpressure); the `platform/Transport/decode.md` `DecodeWorkerPool.reassemble` is the EXECUTION host that posts that `ReadableStream` to the worker as a structured-clone transfer, consumes it back through `Stream.fromReadableStream`/`Stream.fromReadableStreamByob` for zero-copy reads, runs this rail's Crc32-verify-and-stitch fold off the main thread, and mints the one 16-byte `ContentKey` at the worker assembly seam — so the heavy stitch and the digest never block the main thread and the single-hash-mint invariant lives at `worker/`, the `interchange` service a thin delegate to `pool.reassemble`. The main thread stays responsive during large GLB delivery; a future `WebTransport` raw-byte transport lands as a transport protocol case feeding the same transferable seam. The `Ingress/refinement.md` `boundedFrames` budget gates the stream before the projection so an unbounded frame count or assembled-byte ceiling faults before the transfer and the sink allocation.
- Packages: `effect` for the `Stream` reassembly fold and the `Stream.toReadableStream`/`Stream.fromReadableStream`/`Stream.fromReadableStreamByob` transferable destructor set; the per-frame `Crc32` is one owned table-driven rail comparing the wire `frameCrc fixed32` directly with no package. The whole-artifact 128-bit content-key digest is the admitted `hash-wasm` `createXXHash128(0, 0)` `IHasher` (the `xxHash128Of` binding this rail owns); the worker resolves it once at the worker composition root, `update`-feeds the assembled bytes, and `digest("binary")`-finalizes to the 16-byte little-endian digest the `h128` boundary byte-reverses to the C# big-endian `ContentKey`.
- Growth: a new artifact-delivery server-stream method lands as one source the rail folds; a transferable `WebTransport` leg lands as one transport protocol case feeding the same worker boundary; zero second content-address notion.
- Boundary: `ArtifactFrameRail` reassembles only the server-streamed artifact frame type and never the bidi `ArtifactSyncShape.sync` method, and its content key is the one 16-byte `ContentKey` brand, never a second identity notion; the rail reads each frame `payload` as a transferable `Uint8Array` so the heavy stitch never blocks the main thread; the Crc32 is the one owned rail the transport-side `splitFrames` and the reassembly both consume, never a free `crc32Of` duplicating the surface; the worker-side consume uses `Stream.fromReadableStream` (or `Stream.fromReadableStreamByob` where the worker reads the raw byte source directly) so the same `reassemble` fold runs identically on either thread, never a parallel worker-only reassembly. The `createXXHash128` `IHasher` is resolved once at the worker composition root and threaded through the service layer, never re-resolved per artifact, and the digest is byte-reversed once at the `h128` boundary so the branch holds the C# big-endian canonical key, never the raw little-endian hex.

```ts contract
interface ArtifactBlob {
  readonly artifactId: string;
  readonly contentKey: RefinedIdentity["contentKey"];
  readonly bytes: Uint8Array;
}

interface ArtifactFrameRail {
  readonly reassemble: (frames: Stream.Stream<ArtifactFrameWire, FaultDetail>) => Effect.Effect<ArtifactBlob, FaultDetail>;
}

interface Crc32 {
  readonly of: (bytes: Uint8Array) => number;
}

interface XxHash128 {
  readonly h128: (bytes: Uint8Array) => RefinedIdentity["contentKey"];
}

// Binds a resolved `hash-wasm` `IHasher` to the `XxHash128` interface: `init`/`update`/
// `digest("binary")` yields the 16-byte little-endian digest, byte-reversed once to the C#
// big-endian canonical content key the brand admits.
const xxHash128Of = (hasher: IHasher): XxHash128 => ({
  h128: (bytes) => Schema.decodeSync(ContentKey)(hasher.init().update(bytes).digest("binary").reverse()),
});

const CRC32_TABLE: Uint32Array = Uint32Array.from({ length: 256 }, (_, n) =>
  Array.range(0, 7).reduce((c) => ((c & 1) !== 0 ? 0xedb88320 ^ (c >>> 1) : c >>> 1), n));

const crc32: Crc32 = {
  of: (bytes) =>
    (bytes.reduce((c, b) => CRC32_TABLE[(c ^ b) & 0xff] ^ (c >>> 8), 0xffffffff) ^ 0xffffffff) >>> 0,
};

interface AssemblyState {
  readonly id: string;
  readonly sink: Option.Option<Uint8Array>;
}

const reassemble = (crc: Crc32, xxh: XxHash128, frames: Stream.Stream<ArtifactFrameWire, FaultDetail>): Effect.Effect<ArtifactBlob, FaultDetail> =>
  frames.pipe(
    Stream.runFoldEffect(
      { id: "", sink: Option.none<Uint8Array>() } as AssemblyState,
      (state, frame) =>
        crc.of(frame.payload) !== frame.frameCrc
          ? Effect.fail(FaultDetail.HopFault({ reason: "frame-crc-mismatch", evidence: { artifactId: frame.artifactId } }))
          : Effect.sync(() => {
              const sink = Option.getOrElse(state.sink, () => new Uint8Array(frame.artifactBytes));
              sink.set(frame.payload, frame.offset);
              return { id: frame.artifactId, sink: Option.some(sink) } satisfies AssemblyState;
            }),
    ),
    Effect.flatMap((state) =>
      Option.match(state.sink, {
        onNone: () => Effect.fail(FaultDetail.HopFault({ reason: "empty-artifact", evidence: {} })),
        onSome: (bytes) => Effect.succeed({ artifactId: state.id, contentKey: xxh.h128(bytes), bytes }),
      })),
  );

// The transferable seam: the bounded frame `Stream` projects to a native `ReadableStream`
// the `platform` `DecodeWorkerPool` posts to the worker as a structured-clone transfer
// (zero-copy). `Stream.toReadableStream` is the verified data-last destructor; the
// `highWaterMark: 1` strategy carries the one-frame backpressure window across the boundary.
const transferFrames = (frames: Stream.Stream<ArtifactFrameWire, FaultDetail>): ReadableStream<ArtifactFrameWire> =>
  boundedFrames(frames, INGRESS_BUDGET).pipe(
    Stream.toReadableStream({ strategy: new CountQueuingStrategy({ highWaterMark: 1 }) }),
  );

// `ArtifactFrameRail` is the pure reassembly ALGEBRA — `reassemble`, `transferFrames`, the
// owned `Crc32`, and `xxHash128Of` — the `platform/Transport/decode.md` `DecodeWorkerPool`
// imports verbatim and executes off the main thread. `DecodeWorkerPool.reassemble` is the
// worker-resident execution host that resolves the one `createXXHash128(0, 0)` `IHasher` at
// the worker root and mints the single 16-byte `ContentKey` (the single-hash-mint invariant
// lives at `worker/`), so this rail never re-resolves the hasher main-thread-side and the
// `interchange` service is a thin delegate that hands the worker the fold it owns.
class ArtifactFrameRailLive extends Effect.Service<ArtifactFrameRailLive>()("@rasm/ts/interchange/ArtifactFrameRail", {
  effect: Effect.gen(function* () {
    const pool = yield* DecodeWorkerPool;
    return { reassemble: pool.reassemble } satisfies ArtifactFrameRail;
  }),
  dependencies: [DecodeWorkerPoolLive.Default],
}) {}
```

## [3]-[TS_PROJECTION]

- Owner: the `ArtifactFrameWire` frame shape the rail reassembles, sourced from `csharp:Rasm.Compute/Runtime/channels#TS_PROJECTION`.
- Entry: each frame carries `artifactId`, `artifactBytes` (the FULL original length on every frame), `offset`, the `frameCrc` `fixed32`, and the `payload` `Uint8Array`.
- Packages: `effect` `Schema` for the frame surface.

```ts contract
interface ArtifactFrameWire {
  readonly artifactId: string;
  readonly artifactBytes: number;
  readonly offset: number;
  readonly frameCrc: number;
  readonly payload: Uint8Array;
}
```

## [4]-[RESEARCH]

- [CONTENT_HASHING]: the C# content seed is `System.IO.Hashing.XxHash128` (seed=0, big-endian persisted, two-64-bit-half byte order); the admitted `hash-wasm` `createXXHash128(0, 0)` resolves the implementing `IHasher` whose `digest("binary")` returns the 16-byte little-endian digest, so the provider question is settled — the prior `xxhash-wasm`-only `h32`/`h64` gap is closed. The byte-identity gate is RESOLVED: `createXXHash128` emits little-endian and `XxHash128` persists big-endian, so `h128` reverses the 16 bytes once at the boundary and `Codec/parity#CONTENT_KEY_PARITY` `ContentKeyParity` reproduces the FROZEN `csharp:Rasm/Geometry/Spatial/reconciliation#ONE_WIRE_FIXTURE_CORPUS` row [1] `CANONICAL_BYTE_IDENTITY` digest bit-identically through this same `xxHash128Of` binding, comparing NORMALIZED bytes never raw hex. The per-frame Crc32 is a direct compare against the wire `frameCrc fixed32` and needs no probe.
- [BIGINT_ROUNDTRIP]: the `SnapshotHeaderWire.schemaFingerprint` and the `OpLogEntryWire.logical`/`sequence` bigints crossing `@msgpack/msgpack` `useBigInt64: true` and the `DataView` `getBigUint64(offset, false)` big-endian header read round-trip bit-for-bit against the C# `long`/`ulong` HLC encoding — the fixed-width header is `BinaryPrimitives.WriteUInt64BigEndian` on the C# side, so the `littleEndian: false` flag on the `Codec/codec.md` `decodeSnapshotHeader` read is load-bearing and `Codec/parity#HLC_TWO_HALF_PARITY` `HlcTwoHalfParity` asserts the two-64-bit-half order so a half-swap never silently corrupts the conflict-presence fold, since an HLC `logical` off-by-one-half folds a fresh op as stale with no other signal. The `OpLogEntryWire.physical` ISO-8601 instant the `projection` event-time fold (`watermark`, `lww-merge`) reads is the C#-owned HLC physical half against `csharp:Rasm.Persistence/Sync/collaboration#TS_PROJECTION` (`physical: string`), distinct from the `Codec/codec.md` `[3]-[CRDT_OP_DECODE]` `CrdtOpWire` `physicalTicks` flat half (.NET unix ticks); the member spelling and the instant precision (`Date.parse`-admissible extended ISO-8601) resolve against the decoded shape before the convergence-order arms are transcribed.
