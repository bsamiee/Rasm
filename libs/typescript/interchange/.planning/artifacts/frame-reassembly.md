# [INTERCHANGE_FRAME_REASSEMBLY]

Content-addressed artifact-frame reassembly: the framing fold lifted off `transport/transport.md` is stitched into one content-addressed blob through a per-frame Crc32 verify, a single pre-sized sink, and a whole-artifact 128-bit content-key derivation, run off the main thread under the `platform` `DecodeWorkerPool`. `ArtifactFrameRail` is one row on `codecs/decode-rail.md` `DecodeRail` read by the `artifact-frame` codec key; this page owns the reassembly, the owned table-driven Crc32, and the transferable-stream worker boundary. The content key is the one 16-byte `ContentKey` brand byte-identical to the C#-owned `XxHash128` seed; the browser-side 128-bit provider is the `[3]-[RESEARCH]` gate (the admitted `xxhash-wasm` carries only `h32`/`h64`), so the cross-runtime content-addressed cache is blocked on it.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]     | [OWNS]                                                          |
| :-----: | :------------ | :------------------------------------------------------------- |
|   [1]   | FRAME_RAIL    | the reassembly, the owned Crc32, the transferable worker boundary |
|   [2]   | TS_PROJECTION | the artifact-frame wire shape the rail reassembles              |
|   [3]   | RESEARCH      | the XxHash128 and bigint byte-identity probes                  |

## [2]-[FRAME_RAIL]

- Owner: `ArtifactFrameRail`, the row that reassembles the server-streamed `ArtifactFrameWire` frames named on `csharp:Rasm.Compute/remote/remote#TS_PROJECTION` (the `Generate`/`SubtreeFetch` server-stream artifact-delivery path and the GLB tessellation result re-entering through the remote lane) into one content-addressed blob, plus the one owned `Crc32` table-driven rail the whole branch reads. Each frame carries `artifactId`, `artifactBytes`, `offset`, the `frameCrc` `fixed32`, and the `payload` `Uint8Array`.
- Cases: the rail verifies the per-frame Crc32 through the owned `Crc32` rail (a direct number-eq compare against the wire `frameCrc fixed32`, no package), allocates one pre-sized `Uint8Array` of `artifactBytes` length at the first frame and writes each payload by `offset` into that single owned sink, and on the final frame derives the whole-artifact content key over the assembled bytes through the branch-owned `XxHash128` interface (the `[3]-[RESEARCH]` `CONTENT_HASHING` gate owns its provider), so a re-fetch of identical content keys identically and a frame whose Crc32 or assembled digest mismatches faults through `faults/fault-family.md` rather than yielding a torn blob. The bidi `ArtifactSyncShape.sync` method stays structurally excluded on the grpc-web row, but the frame TYPE is browser-reachable over the server-stream and lands here.
- Entry: the rail lifts the `transport/transport.md` framing fold output `Stream<ArtifactFrameWire, FaultDetail>` and reassembles it as a transferable-stream boundary — the Effect `Stream` projects to a transferable `ReadableStream` piped to the `platform` `DecodeWorkerPool`, so the Crc32-verify-and-stitch and the content-key derivation run off the main thread with zero-copy BYOB reads, the worker boundary an explicit transferable seam. The main thread stays responsive during large GLB delivery; a future `WebTransport` raw-byte transport lands as a transport protocol case feeding the same transferable seam. The `refinement/schema-refinement.md` `boundedFrames` budget gates the stream before reassembly so an unbounded frame count or assembled-byte ceiling faults before the sink allocates.
- Packages: `effect` for the `Stream` and the reassembly fold; the per-frame `Crc32` is one owned table-driven rail comparing the wire `frameCrc fixed32` directly with no package. The whole-artifact 128-bit content-key digest is the `[3]-[RESEARCH]` `CONTENT_HASHING` gate — the admitted `xxhash-wasm` carries only `h32`/`h64`, so the 128-bit provider is unresolved.
- Growth: a new artifact-delivery server-stream method lands as one source the rail folds; a transferable `WebTransport` leg lands as one transport protocol case feeding the same worker boundary; zero second content-address notion.
- Boundary: `ArtifactFrameRail` reassembles only the server-streamed artifact frame type and never the bidi `ArtifactSyncShape.sync` method, and its content key is the one 16-byte `ContentKey` brand, never a second identity notion; the rail reads each frame `payload` as a transferable `Uint8Array` so the heavy stitch never blocks the main thread; the Crc32 is the one owned rail the transport-side `splitFrames` and the reassembly both consume, never a free `crc32Of` duplicating the surface.

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
          ? Effect.fail(FaultDetail.HopFault({ code: "frame-crc-mismatch", evidence: { artifactId: frame.artifactId } }))
          : Effect.sync(() => {
              const sink = Option.getOrElse(state.sink, () => new Uint8Array(frame.artifactBytes));
              sink.set(frame.payload, frame.offset);
              return { id: frame.artifactId, sink: Option.some(sink) } satisfies AssemblyState;
            }),
    ),
    Effect.flatMap((state) =>
      Option.match(state.sink, {
        onNone: () => Effect.fail(FaultDetail.HopFault({ code: "empty-artifact", evidence: {} })),
        onSome: (bytes) => Effect.succeed({ artifactId: state.id, contentKey: xxh.h128(bytes), bytes }),
      })),
  );
```

## [3]-[TS_PROJECTION]

- Owner: the `ArtifactFrameWire` frame shape the rail reassembles, transcribed verbatim from `csharp:Rasm.Compute/remote/remote#TS_PROJECTION`.
- Entry: each frame carries `artifactId`, `artifactBytes` (the FULL original length on every frame), `offset`, the `frameCrc` `fixed32`, and the `payload` `Uint8Array`.
- Packages: `effect` `Schema` for the frame surface.
- Growth: a new frame member lands as one field row; the branch authors no shape absent from the C# fence.
- Boundary: the shape transcribes the upstream `#TS_PROJECTION` fence verbatim.

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

- [CONTENT_HASHING]: the C# content seed is `System.IO.Hashing.XxHash128` (seed=0, fixed endianness, two-64-bit-half byte order); the admitted `xxhash-wasm` exposes only `h32`/`h64`/`h64Raw` and carries no 128-bit digest, so the browser-side `XxHash128` capability has no implementing member yet. The 128-bit content-key derivation is gated on admitting a 128-bit-capable wasm hash (`hash-wasm` `xxhash128`, or an XXH3-128 wasm build feeding the worker) or downgrading the C#-owned shared seed to XXH64 so `h64Raw` carries it; until the package question resolves, the tier-2 byte-identity harness cannot assert browser-to-C# parity. The per-frame Crc32 is a direct compare against the wire `frameCrc fixed32` and needs no probe.
- [BIGINT_ROUNDTRIP]: the `SnapshotHeaderWire.schemaFingerprint` and the `OpLogEntryWire.logical`/`sequence` bigints crossing `@msgpack/msgpack` `useBigInt64: true` and the `DataView` `getBigUint64` header read round-trip bit-for-bit against the C# `long`/`ulong` HLC encoding — the same tier-2 byte-identity harness asserts the two-64-bit-half order so a half-swap never silently corrupts the conflict-presence fold, since an HLC `logical` off-by-one-half folds a fresh op as stale with no other signal.
