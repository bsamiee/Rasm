# [PLATFORM_PLATFORM_SUBSTRATE]

One page owns the browser platform substrate — the self-telemetry export edge, the build and main-thread-offload pipeline, the transferable-buffer decode worker pool, and the offline local-persistence store. `SelfTelemetry` and `MetricRegistry` ship the host's own spans and metrics to the collector over the browser OpenTelemetry web exporter; `BuildPipeline` produces the co-hosted bundle and EMITS the service-worker asset (the `service-worker.md` `ServiceWorkerHost` owns the worker's runtime lifecycle, never re-emitting the asset); `DecodeWorkerPool` moves heavy snapshot and artifact-frame decode off the main thread; `LocalPersistence` holds the last-good snapshot and the offline command queue over the Effect key-value abstraction. The page authors no decode and references no telemetry wire type — the collector is the only telemetry path.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]          | [OWNS]                                                          |
| :-----: | :----------------- | :-------------------------------------------------------------- |
|   [1]   | PLATFORM_SUBSTRATE | the telemetry edge, the build/offload pipeline, and persistence |

## [2]-[PLATFORM_SUBSTRATE]

- Owner: `SelfTelemetry`, the host instrumentation export edge over the browser OpenTelemetry web layer; `MetricRegistry`, the bounded instrument-and-span vocabulary the host edge ships; `BuildPipeline`, the build and PWA and styling pipeline; `DecodeWorkerPool`, the transferable-buffer browser decode pool; and `LocalPersistence`, the last-good-snapshot and offline-command-queue store over the browser key-value binding.
- Cases: `SelfTelemetry` ships the host's own spans and metrics to the collector through the OTLP web export, the collector the only telemetry path and dashboards reading it through the `ui/render-surfaces.md` `CollectorPanel`; `MetricRegistry` owns the closed `Metric` instrument vocabulary mirroring the C# `HostMetrics` names, the closed `Effect.withSpan` span vocabulary (the `crash.report` row the `error-boundary.md` `CrashTelemetry` ships and the `web.vital.breach` row the `web-vitals.md` `PerformanceBudget` ships are span literals on this one axis, never free-string span names authored at the sink), and the Core Web Vitals instrument family the `web-vitals.md` `PerformanceBudget` feeds (the registry declares the Core-Web-Vitals instrument rows; `PerformanceBudget` owns the capture and budget-gating, never a parallel metric construction), so every instrument is a named row rather than an ad-hoc construction; `BuildPipeline` produces the co-hosted same-origin asset bundle with its plugin set, the styling pipeline, and EMITS the service-worker asset through `vite-plugin-pwa` (the `service-worker.md` `ServiceWorkerHost` owns the install/activate/cache-strategy/background-sync runtime lifecycle of that asset), and strips the development-build atom inspector from the production bundle; `DecodeWorkerPool` moves heavy snapshot decode and fold work onto a structured `Worker.makePool` whose `WorkerManager` (carrying the `Spawner` binding) arrives as a layer requirement in the `R` channel, never a `spawn` inline in the options bag, so the scoped-fiber guarantee holds without blocking the main thread; the worker message ships a `DecodeSchemaKey` registry identifier and the raw bytes — never a live `Schema` instance, which is not structured-cloneable — so the worker resolves the schema from its own registry and the main thread re-`Schema.decodeUnknown`s the worker's plain decoded value, and the heaviest case is the `interchange` `ArtifactFrameRail` server-stream reassembly — the per-frame Crc32 verify, the offset-ordered stitch, and the whole-artifact XxHash128 content-key derivation run off the main thread, each frame payload arriving as a `Transferable` `Uint8Array`, so a large GLB or support-bundle artifact never blocks the SPA; `LocalPersistence` holds the Schema-encoded last-good store snapshots and the offline command queue so a redial restores from the last good state rather than a cold boot.
- Auto: `MetricRegistry` binds the `@effect/opentelemetry` `WebSdk` Layer with its resource attributes and the OTLP trace-and-metric exporters reading the collector endpoint from `RuntimeConfig`, so the export edge is one layer over a named instrument set and an inline `Metric.counter` construction outside the registry is the deleted form; `LocalPersistence` is the `@effect/platform-browser` `KeyValueStore`/`BrowserKeyValueStore` surface composing `idb-keyval` as the BACKING store only — never a hand-rolled `idb-keyval` call — so a snapshot persists as a Schema-encoded value and the offline command queue drains in order on redial; a hand-rolled `localStorage` read or a JSON-stringified blob outside the Schema-encoded store is the deleted form.
- Packages: `effect` for the `Metric`, `Effect.withSpan`, and `Worker.makePool` primitives, `@effect/platform-browser` for the browser OpenTelemetry layer, the `KeyValueStore`/`BrowserKeyValueStore` surface, and the worker primitives, `@effect/opentelemetry` for the `WebSdk` exporter, `@opentelemetry/sdk-trace-web` as the browser trace SDK the `WebSdk` binds, `idb-keyval` as the IndexedDB backing store under the KV abstraction, and `vite` and its plugin family for the build toolchain and PWA worker.
- Growth: a new signal lands as one instrument row on `MetricRegistry`; a new span lands as one `Effect.withSpan` vocabulary row; a new build concern lands as one plugin row; a new offload concern lands as one worker-pool task; a new persisted store lands as one `LocalPersistence` key-value row.
- Boundary: the self-telemetry spans and metrics cross no wire contract — they ship from the host root to the collector, which is the only telemetry path; the observability stack that backs the collector is provisioned by `services` `provisioning.md`; `MetricRegistry` is the single instrument owner and an inline metric construction outside it is the named defect; the browser worker pool is a main-thread-offload concern and never a durable tier — the durable cluster is the `services` concern; `LocalPersistence` is the single browser-local store and a direct `localStorage`/`IndexedDB`/`idb-keyval` access outside it is the named defect; the offline-queue drain reaches the `interchange` `CommandGateway` across the intra-package folder seam (the `service-worker.md` `BackgroundSyncReplay` owns the SW-triggered redial drive over this same `LocalPersistence.offlineQueue`) and never re-dials a transport here; the node OTel SDK never enters this browser edge.

```ts contract
type CounterKey = "wire_calls_total" | "fault_total" | "redial_total" | "offline_drain_total";
type HistogramKey = "wire_call_duration_ms" | "decode_duration_ms" | "frame_reassemble_ms";
type VitalKey = "web_vital_lcp_ms" | "web_vital_inp_ms" | "web_vital_cls" | "web_vital_ttfb_ms" | "web_vital_fcp_ms";
type GaugeKey = "active_subscriptions" | "offline_queue_depth" | VitalKey;
type SpanName = "boot.spa" | "route.transition" | "worker.reassemble" | "auth.refresh" | "sw.activate" | "crash.report" | "web.vital.breach";

interface MetricRegistry {
  readonly counters: { readonly [K in CounterKey]: Metric.Metric.Counter<number> };
  readonly histograms: { readonly [K in HistogramKey]: Metric.Metric.Histogram<number> };
  readonly gauges: { readonly [K in GaugeKey]: Metric.Metric.Gauge<number> };
  readonly span: <A, E, R>(name: SpanName, effect: Effect.Effect<A, E, R>) => Effect.Effect<A, E, R>;
  readonly webSdk: Layer.Layer<never, never, RuntimeConfig>;
}

type DecodeSchemaKey = "snapshot-header" | "snapshot-delta" | "oplog-entry" | "receipt-envelope";

type WorkerRequest =
  | { readonly _tag: "Reassemble"; readonly frames: ReadonlyArray<ArtifactFrameWire> }
  | { readonly _tag: "Decode"; readonly schemaKey: DecodeSchemaKey; readonly bytes: Uint8Array };

type WorkerResponse =
  | { readonly _tag: "Blob"; readonly blob: ArtifactBlob }
  | { readonly _tag: "Decoded"; readonly value: unknown };

interface DecodeWorkerPool {
  readonly reassemble: (frames: Stream.Stream<ArtifactFrameWire, FaultDetail>) => Effect.Effect<ArtifactBlob, FaultDetail>;
  readonly decode: <A>(schemaKey: DecodeSchemaKey, schema: Schema.Schema<A>, bytes: Uint8Array) => Effect.Effect<A, ParseResult.ParseError>;
}

const makeDecodeWorkerPool: Effect.Effect<DecodeWorkerPool, never, Scope.Scope | WorkerManager.WorkerManager> =
  Effect.gen(function* () {
    const pool = yield* Worker.makePool<WorkerRequest>({ size: navigator.hardwareConcurrency });
    const reassemble = (frames: Stream.Stream<ArtifactFrameWire, FaultDetail>) =>
      Stream.runCollect(frames).pipe(
        Effect.flatMap((chunk) => pool.executeEffect({ _tag: "Reassemble", frames: Chunk.toReadonlyArray(chunk) } satisfies WorkerRequest)),
        Effect.flatMap((res) => (res._tag === "Blob" ? Effect.succeed(res.blob) : Effect.fail(FaultDetail.HopFault({ code: "worker-protocol", evidence: {} })))),
        Effect.catchAll(() => Effect.fail(FaultDetail.HopFault({ code: "worker-reassemble", evidence: {} }))),
      );
    const decode = <A>(schemaKey: DecodeSchemaKey, schema: Schema.Schema<A>, bytes: Uint8Array) =>
      pool.executeEffect({ _tag: "Decode", schemaKey, bytes } satisfies WorkerRequest).pipe(
        Effect.mapError(() => new ParseResult.ParseError({ issue: new ParseResult.Unexpected(schemaKey, "worker decode") })),
        Effect.flatMap((res) => (res._tag === "Decoded" ? Schema.decodeUnknown(schema)(res.value) : Effect.fail(new ParseResult.ParseError({ issue: new ParseResult.Unexpected(res, "worker decode") })))),
      );
    return { reassemble, decode } satisfies DecodeWorkerPool;
  });

interface LocalPersistence {
  readonly store: KeyValueStore.KeyValueStore;
  readonly lastGood: <A, I, R>(schema: Schema.Schema<A, I, R>, key: string) => {
    readonly save: (value: A) => Effect.Effect<void, ParseResult.ParseError, R>;
    readonly load: Effect.Effect<Option.Option<A>, ParseResult.ParseError, R>;
  };
  readonly offlineQueue: {
    readonly enqueue: (command: CommandPayloadWire) => Effect.Effect<void>;
    readonly drainOnRedial: Effect.Effect<ReadonlyArray<CommandPayloadWire>>;
  };
}
```
