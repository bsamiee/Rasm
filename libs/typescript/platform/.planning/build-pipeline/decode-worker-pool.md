# [PLATFORM_DECODE_WORKER_POOL]

One page owns the main-thread-offload decode pool — `DecodeWorkerPool`, the transferable-buffer `Worker.makePoolSerialized` over a closed `Schema.TaggedRequest` request family that moves heavy snapshot and artifact-frame decode off the main thread. Each request is a `Schema.TaggedRequest` carrying its own result and error schema, so the pool owns serialization end-to-end and the worker returns the typed value — never a live `Schema` instance crossing the boundary and never a main-thread re-decode that re-pays the offloaded cost. The heaviest case is the `interchange` `ArtifactFrameRail` server-stream reassembly run off the main thread. The page authors no wire shape and is never a durable tier.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]         | [OWNS]                                                            |
| :-----: | :---------------- | :-------------------------------------------------------------- |
|   [1]   | DECODE_WORKER_POOL | the transferable-buffer decode pool and the frame reassembly offload |

## [2]-[DECODE_WORKER_POOL]

- Owner: `DecodeWorkerPool`, the transferable-buffer browser decode pool over a `Worker.makePoolSerialized` of a closed `Schema.TaggedRequest` request family.
- Cases: `DecodeWorkerPool` moves heavy snapshot decode and fold work onto a `Worker.makePoolSerialized` whose `WorkerManager` (carrying the `Spawner` binding) arrives as a layer requirement in the `R` channel, never a `spawn` inline in the options bag, so the scoped-fiber guarantee holds without blocking the main thread; each request is a `Schema.TaggedRequest` whose `Success`/`Failure` schema the pool serializes end-to-end, so the worker decodes the raw bytes against its own registered handler and ships back the typed value — no live `Schema` instance crosses the structured-clone boundary and no main-thread `Schema.decodeUnknown` re-pays the cost the offload exists to move; the heaviest case is the `interchange` `ArtifactFrameRail` server-stream reassembly — the per-frame Crc32 verify, the offset-ordered stitch, and the whole-artifact XxHash128 content-key derivation run off the main thread, each frame payload arriving as a `Transferable` `Uint8Array` declared on the request's `transfers`, so a large GLB or support-bundle artifact never blocks the SPA.
- Packages: `effect` for the `Schema.TaggedRequest` request family and the `Stream` frame ingress; `@effect/platform` `Worker.makePoolSerialized`/`WorkerRunner.layerSerialized` for the pool and the worker-side handler set; `@effect/platform-browser` for the worker primitives and the `WorkerManager` binding.
- Growth: a new offload concern lands as one `Schema.TaggedRequest` case in the `DecodeRequest` family with its own result schema, never a parallel pool or an untyped message arm.
- Boundary: the browser worker pool is a main-thread-offload concern and never a durable tier — the durable cluster is the `services` concern; the `WorkerManager` is provided by `runtime-composition`'s `BrowserPlatform`, never an inline `spawn` here; the reassembled artifact keys on the `interchange`-owned content-address digest, never a second mint; the pool authors no wire shape, and the `WorkerRunner.layerSerialized` handler set lives in the `decode.worker.ts` entry the `BrowserPlatform` spawner constructs.

```ts contract
class Reassemble extends Schema.TaggedRequest<Reassemble>()("Reassemble", {
  payload: { frames: Schema.Array(ArtifactFrameWire) },
  success: ArtifactBlob,
  failure: FaultDetail,
}) {}

class DecodeSnapshot extends Schema.TaggedRequest<DecodeSnapshot>()("DecodeSnapshot", {
  payload: { kind: Schema.Literal("snapshot-header", "snapshot-delta", "oplog-entry", "receipt-envelope"), bytes: Schema.Uint8ArrayFromSelf },
  success: SnapshotValue,
  failure: FaultDetail,
}) {}

type DecodeRequest = Reassemble | DecodeSnapshot;

interface DecodeWorkerPool {
  readonly reassemble: (frames: Stream.Stream<ArtifactFrameWire, FaultDetail>) => Effect.Effect<ArtifactBlob, FaultDetail>;
  readonly decode: (request: DecodeSnapshot) => Effect.Effect<SnapshotValue, FaultDetail>;
}

class DecodeWorkerPoolLive extends Effect.Service<DecodeWorkerPoolLive>()("@rasm/ts/platform/DecodeWorkerPool", {
  scoped: Effect.gen(function* () {
    const pool = yield* Worker.makePoolSerialized<DecodeRequest>({ size: navigator.hardwareConcurrency });
    const reassemble = (frames: Stream.Stream<ArtifactFrameWire, FaultDetail>) =>
      Stream.runCollect(frames).pipe(
        Effect.flatMap((chunk) => pool.executeEffect(new Reassemble({ frames: Chunk.toReadonlyArray(chunk) }))),
        Effect.catchTags({
          WorkerError: () => Effect.fail(FaultDetail.HopFault({ code: "worker-reassemble", evidence: {} })),
          ParseError: () => Effect.fail(FaultDetail.HopFault({ code: "worker-protocol", evidence: {} })),
        }),
      );
    const decode = (request: DecodeSnapshot) =>
      pool.executeEffect(request).pipe(
        Effect.catchTags({
          WorkerError: () => Effect.fail(FaultDetail.HopFault({ code: "worker-decode", evidence: {} })),
          ParseError: () => Effect.fail(FaultDetail.HopFault({ code: "worker-protocol", evidence: {} })),
        }),
      );
    return { reassemble, decode } satisfies DecodeWorkerPool;
  }),
}) {}
```
