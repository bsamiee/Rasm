# [PLATFORM_DECODE]

One page owns the main-thread-offload decode pool — `DecodeWorkerPool`, the transferable-buffer `Worker.makePoolSerialized` over a closed `Schema.TaggedRequest` request family moving heavy snapshot, artifact-frame, and geometry-residency decode off the main thread, and the worker-interior single content-key mint. `worker/` is the operator-named top-level browser leg the decode pool relocated to out of `build-pipeline/` (now build-time-only); the `Runtime/bindings.md` `WorkerManager` spawner constructs this leg's `decode.worker.ts` entry. Each request is a `Schema.TaggedRequest` carrying its own result and error schema, so the pool owns serialization end-to-end and the worker returns the typed value — never a live `Schema` crossing the boundary, never a main-thread re-decode re-paying the offloaded cost. The worker derives the one 16-byte content key at the assembly seam, so the single-hash-mint invariant holds at `worker/`. The `DecodeResidency` case DECODES the settled `csharp:Rasm.AppUi/Render/viewport#TS_PROJECTION`-minted `GeometryResidencyWire` (the single C# mint of the `WEB_GEOMETRY_RESIDENCY_WIRE`) into the branded `ResidencyValue` — the worker is the sole TS consumer the `typescript:ui/render/glb#GLB_VIEWPORT` leaf reads by reference, decoding each tile's `:x32` content-key string into the one `interchange` `ContentKey` brand and never re-minting the manifest or its hash. The page authors no wire shape and is never a durable tier.

## [01]-[INDEX]

- [01]-[DECODE_POOL]: the transferable decode pool, the frame/snapshot/residency offload, the single content-key mint.
- [02]-[RESIDENCY_DECODE]: the `GeometryResidencyWire` -> `ResidencyValue` decode, the `:x32` content-key brand fold.

## [02]-[DECODE_POOL]

- Owner: `DecodeWorkerPool`, the transferable-buffer browser decode pool over a `Worker.makePoolSerialized` of a closed `DecodeRequest` `Schema.TaggedRequest` family, plus the worker-interior `mintContentKey` deriving the one 16-byte `ContentKey` over assembled bytes. The decode pool is the one main-thread-offload owner and a second pool or an untyped `postMessage` arm is the named parallel-pool defect; the content key is minted once at the worker assembly seam and a second mint anywhere downstream is the named double-mint defect.
- Cases: `DecodeWorkerPool` moves heavy decode onto a `Worker.makePoolSerialized` whose `WorkerManager` (carrying the `Spawner`) arrives as a layer requirement in the `R` channel — provided by `Runtime/bindings.md`'s `BrowserPlatform` over the `spawnDecodeWorker` factory now pointing at `../worker/decode.worker.ts` — never a `spawn` inline in the options bag, so the scoped-fiber guarantee holds off the main thread; each request is a `Schema.TaggedRequest` whose `Success`/`Failure` schema the pool serializes end-to-end, so the worker decodes the raw bytes against its registered handler and ships back the typed value with no live `Schema` crossing the structured-clone boundary and no main-thread `Schema.decodeUnknown` re-paying the cost; the heaviest case is the `interchange` `ArtifactFrameRail` server-stream reassembly — the per-frame Crc32 verify, the offset-ordered stitch, and the whole-artifact content-key mint run off the main thread, each frame payload a `Transferable` `Uint8Array` on the request's `transfers`, so a large GLB never blocks the SPA; the `DecodeResidency` case decodes the `csharp:Rasm.AppUi/Render/viewport#TS_PROJECTION`-minted `GeometryResidencyWire` (the JSON `manifest` payload as a `Transferable` `Uint8Array` on the request's `transfers`) for the WebGPU raster path the GLB-per-tile rail does not cover — `decodeResidency` text-decodes the bytes and runs `Schema.decodeUnknown(GeometryResidencyWireSchema)` off the main thread, folding each `ResidencyTileWire`/`SplatTileWire` `:x32` content-key string into the one `interchange` `ContentKey` brand through `ContentKeyHex` so a tile keys by the content-addressed blob key exactly as the desktop reads, never a second identity notion and never a TS-side re-mint of the AppUi master.
- Auto: the worker interior mints the content key once — `mintContentKey` runs the `hash-wasm` `createXXHash128` digest (`digestSize` 16, seeded from the two 32-bit halves) over the assembled artifact or residency bytes at the assembly seam, normalizing the little-endian wasm digest to the canonical byte order the `interchange` `ContentKey` brand fixes, so a re-fetch of identical content keys identically and the AppUi-minted residency manifest is the one master the worker reads rather than re-minting (the single-hash-mint invariant holds at `worker/` — absent a master owner the manifest is minted twice, the AppUi projection and the worker interior); the digest provider resolves once at the worker entry, the `IHasher` threaded through the handler set, never re-resolved per request.
- Packages: `effect` for the `DecodeRequest` `Schema.TaggedRequest` family and the `Stream` frame ingress; `@effect/platform` `Worker.makePoolSerialized`/`WorkerRunner.layerSerialized` for the pool and the worker-side handler set; `@effect/platform-browser` for the worker primitives and the `WorkerManager` binding; `hash-wasm` `createXXHash128` for the worker-interior 16-byte content-key mint (the cross-folder `interchange` catalogue owns the spelling).
- Growth: a new offload concern lands as one `DecodeRequest` case with its own result schema, never a parallel pool or an untyped message arm; a new geometry-residency wire member lands as one field on the `GeometryResidencyWireSchema` row mirroring the AppUi-minted manifest shape, never a second mint; a `WebTransport` raw-byte leg feeding the same transferable seam lands as one transport case on the `interchange` framing fold, the worker boundary unchanged.
- Boundary: the browser worker pool is a main-thread-offload concern and never a durable tier — the durable cluster is the `services` concern; the `WorkerManager` is provided by `runtime-composition`'s `BrowserPlatform`, never an inline `spawn` here; `Shell/build.md` EMITS the build artifacts and is build-time-only after this move, owning no worker runtime; the reassembled artifact keys on the one `interchange`-owned `ContentKey` minted once at this worker seam, and the residency manifest is the AppUi-minted master the worker DECODES — its tile content keys decode the `:x32` wire string into that same `ContentKey` brand, so the worker mints the artifact key and consumes the residency key, never a second residency mint (the single-mint invariant graded at the cross-`libs/` master against the `csharp:Rasm.AppUi/Render/viewport#TS_PROJECTION` `ResidencyManifest.Mint` producer and the `typescript:ui/render/glb#GLB_VIEWPORT` consume-only row); the pool authors no wire shape — `GeometryResidencyWireSchema` is the decode of the C#-owned shape, never a re-mint — and the `WorkerRunner.layerSerialized` handler set lives in the `decode.worker.ts` entry the `BrowserPlatform` spawner constructs.

```ts contract
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
import { Chunk, Effect, Schema, Stream, Worker } from "effect";
import type { ArtifactBlob, ArtifactFrameWire } from "../../interchange/artifacts/frame-reassembly.ts";
import type { SnapshotValue } from "../../Runtime/codecs/snapshot-codec.ts";
import type { HopReason } from "../../interchange/faults/fault-family.ts";
import { ArtifactFrameWire as ArtifactFrameWireSchema, FaultDetail } from "../../interchange/artifacts/frame-reassembly.ts";
import { ContentKey } from "../../interchange/refinement/schema-refinement.ts";

// --- [TYPES] ---------------------------------------------------------------------------
class Reassemble extends Schema.TaggedRequest<Reassemble>()("Reassemble", {
  payload: { frames: Schema.Array(ArtifactFrameWireSchema) },
  success: Schema.suspend((): Schema.Schema<ArtifactBlob> => ArtifactBlobSchema),
  failure: FaultDetail,
}) {}

class DecodeSnapshot extends Schema.TaggedRequest<DecodeSnapshot>()("DecodeSnapshot", {
  payload: { kind: Schema.Literal("snapshot-header", "snapshot-delta", "oplog-entry", "receipt-envelope"), bytes: Schema.Uint8ArrayFromSelf },
  success: Schema.suspend((): Schema.Schema<SnapshotValue> => SnapshotValueSchema),
  failure: FaultDetail,
}) {}

class DecodeResidency extends Schema.TaggedRequest<DecodeResidency>()("DecodeResidency", {
  payload: { manifest: Schema.Uint8ArrayFromSelf },
  success: ResidencyValue,
  failure: FaultDetail,
}) {}

type DecodeRequest = Reassemble | DecodeSnapshot | DecodeResidency;

// --- [SERVICES] ------------------------------------------------------------------------
interface DecodeWorkerPool {
  readonly reassemble: (frames: Stream.Stream<ArtifactFrameWire, FaultDetail>) => Effect.Effect<ArtifactBlob, FaultDetail>;
  readonly decode: (request: DecodeSnapshot) => Effect.Effect<SnapshotValue, FaultDetail>;
  readonly residency: (manifest: Uint8Array) => Effect.Effect<ResidencyValue, FaultDetail>;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
class DecodeWorkerPoolLive extends Effect.Service<DecodeWorkerPoolLive>()("@rasm/ts/platform/DecodeWorkerPool", {
  scoped: Effect.gen(function* () {
    const pool = yield* Worker.makePoolSerialized<DecodeRequest>({ size: navigator.hardwareConcurrency });
    const liftWorker = (faulted: Extract<HopReason, "worker-reassemble" | "worker-decode">) =>
      <A>(self: Effect.Effect<A, unknown>): Effect.Effect<A, FaultDetail> =>
        self.pipe(
          Effect.catchTags({
            WorkerError: () => Effect.fail(FaultDetail.HopFault({ reason: faulted, evidence: {} })),
            ParseError: () => Effect.fail(FaultDetail.HopFault({ reason: "worker-protocol", evidence: {} })),
          }),
        );
    const reassemble = (frames: Stream.Stream<ArtifactFrameWire, FaultDetail>) =>
      Stream.runCollect(frames).pipe(
        Effect.flatMap((chunk) => liftWorker("worker-reassemble")(pool.executeEffect(new Reassemble({ frames: Chunk.toReadonlyArray(chunk) })))),
      );
    const decode = (request: DecodeSnapshot) => liftWorker("worker-decode")(pool.executeEffect(request));
    const residency = (manifest: Uint8Array) => liftWorker("worker-decode")(pool.executeEffect(new DecodeResidency({ manifest })));
    return { reassemble, decode, residency } satisfies DecodeWorkerPool;
  }),
}) {}
```

## [03]-[RESIDENCY_DECODE]

- Owner: `ResidencyValue`, the branded decode of the `csharp:Rasm.AppUi/Render/viewport#TS_PROJECTION`-minted `GeometryResidencyWire`; `GeometryResidencyWireSchema`, the `Schema.Class` transcribing the camelCase-Strict wire shape the C# `ResidencyManifest.Encode` emits; `ContentKeyHex`, the one codec folding the `:x32` 32-hex content-key string into the `interchange` `ContentKey` 16-byte brand. The worker decodes the manifest bytes once at the `DecodeResidency` handler; a re-mint of the manifest or a parallel content-key notion is the named cross-language drift defect.
- Cases: `decodeResidency` `TextDecoder`-decodes the transferred manifest `Uint8Array` to the JSON text, parses it through `Schema.parseJson(GeometryResidencyWireSchema)` so the parse and the structural decode are one boundary, and yields the branded `ResidencyValue`; each `ResidencyTileWire.contentKey` and `SplatTileWire.contentKey` `:x32` string decodes through `ContentKeyHex` into the one `ContentKey` brand the `interchange` `Ingress/refinement.md` owns, so a downstream WebGPU upload keys a tile by the content-addressed `blobKey` the desktop wrote and never by the scene-cell `key`; the `viewpoint`/`cluster`/`tiles`/`splats`/`vramBudget` members transcribe the producer fence verbatim, the `version` gating the schema mismatch through one filter.
- Auto: `ContentKeyHex` is a `Schema.transform` from the wire `:x32` hex string to the `ContentKey` brand — `decode` parses the 32-hex into the 16 big-endian bytes the C# `UInt128:x32` writes (the `interchange` canonical byte order, never the raw little-endian wasm digest) and `Schema.decodeSync(ContentKey)` brands them, `encode` re-renders the 16 bytes to the `:x32` form, so the brand fold is bidirectional at the one seam and a raw hex string never reaches a tile consumer; the splat-tile arm decodes a present `SplatTileWire` now (count, harmonic degree, bounds, content key, blob key), the meshlet/triangulated arm decodes the present `MeshletClusterWire`/`ResidencyTileWire`, and only the upstream Compute splat-PAYLOAD bytes the `blobKey` resolves stay gated on the Python scan two-hop — the residency manifest decode itself is unblocked because the producer mint is settled.
- Packages: `effect` `Schema.Class`/`Schema.parseJson`/`Schema.transform`/`Schema.decodeUnknown` for the wire decode and the brand fold; the `interchange` `Ingress/refinement.md` `ContentKey` brand for the content-key identity, never re-declared here; native `TextDecoder` for the transferred-byte decode.
- Growth: a new residency wire member lands as one `GeometryResidencyWireSchema` field mirroring the AppUi producer fence; a new tile kind lands as one wire tile `Schema.Class` and one `ResidencyValue` field, never a second manifest; a content-key spelling change lands as one `ContentKeyHex` codec edit at the one seam.
- Boundary: `GeometryResidencyWireSchema` DECODES the C#-owned `WEB_GEOMETRY_RESIDENCY_WIRE` and authors no wire vocabulary — the producer `csharp:Rasm.AppUi/Render/viewport#TS_PROJECTION` `ResidencyManifest` is the single mint, this decode the sole TS consumer the `typescript:ui/render/glb#GLB_VIEWPORT` leaf reads by reference; the content key is the one `interchange` `ContentKey` brand and a second identity notion on the TS side is the named drift defect graded at the cross-`libs/` master; the decode runs in the worker off the main thread so a large manifest never blocks the SPA.

```ts contract
// --- [TYPES] ---------------------------------------------------------------------------
const ContentKeyHex = Schema.transform(
  Schema.String.pipe(Schema.filter((s) => /^[0-9a-f]{32}$/i.test(s))),
  ContentKey,
  {
    strict: true,
    decode: (hex) => Uint8Array.from({ length: 16 }, (_, i) => Number.parseInt(hex.slice(i * 2, i * 2 + 2), 16)) as typeof ContentKey.Type,
    encode: (bytes) => Array.from(bytes, (b) => b.toString(16).padStart(2, "0")).join(""),
  },
);

const ViewCameraWire = Schema.Struct({
  perspective: Schema.Boolean,
  eye: Schema.Tuple(Schema.Number, Schema.Number, Schema.Number),
  target: Schema.Tuple(Schema.Number, Schema.Number, Schema.Number),
  up: Schema.Tuple(Schema.Number, Schema.Number, Schema.Number),
  fieldOfView: Schema.Number,
  orthoScale: Schema.Number,
});

const SectionBoxWire = Schema.Struct({
  min: Schema.Tuple(Schema.Number, Schema.Number, Schema.Number),
  max: Schema.Tuple(Schema.Number, Schema.Number, Schema.Number),
  enabled: Schema.Boolean,
});

const VisibilityOverrideWire = Schema.Struct({
  elementId: Schema.String,
  visible: Schema.Boolean,
  colorArgb: Schema.NullOr(Schema.Number),
  transparency: Schema.Number,
});

const ViewpointWire = Schema.Struct({
  key: Schema.String,
  version: Schema.Number,
  camera: ViewCameraWire,
  section: SectionBoxWire,
  overrides: Schema.Array(VisibilityOverrideWire),
  selection: Schema.Array(Schema.String),
  at: Schema.String,
});

const MeshletWire = Schema.Struct({
  vertexOffset: Schema.Number,
  vertexCount: Schema.Number,
  triangleOffset: Schema.Number,
  triangleCount: Schema.Number,
  center: Schema.Tuple(Schema.Number, Schema.Number, Schema.Number),
  radius: Schema.Number,
  coneAxis: Schema.Tuple(Schema.Number, Schema.Number, Schema.Number),
  coneCutoff: Schema.Number,
  screenSpaceError: Schema.Number,
});

const MeshletClusterWire = Schema.Struct({
  backend: Schema.String,
  meshlets: Schema.Array(MeshletWire),
  bindless: Schema.Array(Schema.String),
  triangles: Schema.Number,
});

const Bounds4 = Schema.Tuple(Schema.Number, Schema.Number, Schema.Number, Schema.Number);

const ResidencyTileWire = Schema.Struct({
  key: Schema.String,
  contentKey: ContentKeyHex,
  bytes: Schema.Number,
  bounds: Bounds4,
  blobKey: Schema.String,
});

const SplatTileWire = Schema.Struct({
  key: Schema.String,
  contentKey: ContentKeyHex,
  count: Schema.Number,
  harmonicDegree: Schema.Number,
  bounds: Bounds4,
  blobKey: Schema.String,
});

class GeometryResidencyWireSchema extends Schema.Class<GeometryResidencyWireSchema>("GeometryResidencyWire")({
  version: Schema.Number.pipe(Schema.filter((v) => v === RESIDENCY_SCHEMA)),
  viewpoint: ViewpointWire,
  cluster: MeshletClusterWire,
  tiles: Schema.Array(ResidencyTileWire),
  splats: Schema.Array(SplatTileWire),
  vramBudget: Schema.Number,
}) {}

const ResidencyValue = GeometryResidencyWireSchema;
type ResidencyValue = typeof GeometryResidencyWireSchema.Type;

// --- [CONSTANTS] -----------------------------------------------------------------------
const RESIDENCY_SCHEMA: number = 1;

// --- [OPERATIONS] ----------------------------------------------------------------------
const decodeResidency = (manifest: Uint8Array): Effect.Effect<ResidencyValue, FaultDetail> =>
  Schema.decodeUnknown(Schema.parseJson(GeometryResidencyWireSchema))(new TextDecoder().decode(manifest)).pipe(
    Effect.catchTag("ParseError", () => Effect.fail(FaultDetail.HopFault({ reason: "worker-protocol", evidence: {} }))),
  );
```

## [04]-[RESEARCH]

- [SPLAT_PAYLOAD]: the residency manifest decode is settled — `GeometryResidencyWireSchema` decodes the `csharp:Rasm.AppUi/Render/viewport#TS_PROJECTION` `ResidencyManifest` mint and `ResidencyTileWire`/`SplatTileWire` carry the present meshlet and splat tiles now; only the upstream Compute splat-PAYLOAD bytes the `SplatTileWire.blobKey` resolves stay `[UPSTREAM-BLOCKED]` on the Python SOG/PLY/LAZ scan-decode two-hop that produces a `SplatSource`, so a splat tile's manifest row decodes today while its payload fetch over the blob key is the deferred leg, and the Python content-key reproduction of the `:x32` form stays `[UPSTREAM-BLOCKED]` on the `xxhash` cp315/abi3 wheel the companion lacks below 3.15. The WebGPU cluster-LOD upload of the decoded residency on the browser device is `[HOST-PROBE-DEFERRED]` on the live WebGPU device the `csharp:Rasm.AppUi/Render/viewport#TS_PROJECTION` `WebGpu` `GpuBackend` row binds — the manifest decode this leg owns is unblocked, the GPU upload the consumer leaf drives.
- [CONTENT_MINT]: the worker-interior `mintContentKey` composes `hash-wasm` `createXXHash128` (`digestSize` 16, two-32-bit-half seed) over assembled bytes; the wasm emits little-endian while the C# `System.IO.Hashing.XxHash128` persists big-endian, so the mint normalizes byte order to the canonical `interchange` `ContentKey` at the seam — the tier-2 browser-to-C# byte-identity parity is the `interchange` `Codec/frame.md#RESEARCH` `CONTENT_HASHING` gate this leg consumes, not re-derived here. `ContentKeyHex` decodes the C#-written `:x32` big-endian form, so the residency-tile content key needs no endianness reversal — the `:x32` string is already the canonical byte order.
- [WORKER_RESULT_SHAPES]: the `ArtifactBlobSchema`/`SnapshotValueSchema` success-schema spellings the `Schema.TaggedRequest` cases reference are the decoded result owners on `interchange` `Codec/frame.md` and the snapshot codec; the success schema is the `Schema.Class` those owners declare, suspended here to break the cross-folder import cycle, never a re-authored result shape. `ResidencyValue` is the local `GeometryResidencyWireSchema` `Schema.Class` so the `DecodeResidency` success schema needs no suspend.
```
