# [PLATFORM_BINDINGS]

One page owns the browser platform bindings layer — `BrowserPlatform`, the one `Layer.mergeAll` of the `@effect/platform-browser` fetch HTTP client, the local-storage key-value store, and the worker-manager binding that every host owner composes for its IO primitives. The layer is config-free stock bindings sitting directly above `RuntimeConfig` in the graph; it owns no policy and authors no decode.

## [01]-[INDEX]

- [01]-[BROWSER_PLATFORM]: the HTTP client, the key-value store, and the worker-manager binding.

## [02]-[BROWSER_PLATFORM]

- Owner: `BrowserPlatform`, the browser platform layer owning the HTTP client, the key-value store, and the `WorkerManager` binding bound under one `Layer.mergeAll`.
- Cases: `BrowserPlatform` binds the platform services from one `Layer.mergeAll` of the `@effect/platform-browser` `FetchHttpClient.layer`, `BrowserKeyValueStore.layerLocalStorage`, and the `WorkerManager` binding, so the fetch client `feature-flags`'s `RemoteConfig` config fetch and `observability`'s OTLP exporters resolve over, the key-value store `local-persistence` composes, and the `WorkerManager` `worker/`'s `DecodeWorkerPool` requires each resolve from this one binding rather than a per-owner platform layer; the layer is config-free stock bindings, so it sits directly above `RuntimeConfig.provider` in the graph and below the config-reading owners.
- Auto: the `WorkerManager` binding is `Worker.layerManager` (the platform-neutral `@effect/platform` `Worker` module member) provided over `BrowserWorker.layerPlatform(spawn)` — `layerPlatform` yields `Worker.PlatformWorker | Worker.Spawner` from the `spawn` factory that constructs the `worker/` decode-worker entry module, and `Worker.layerManager` lifts that `PlatformWorker` into the `Worker.WorkerManager` tag the pool requires; the `BrowserWorker.layer(spawn)` one-shot that yields `Worker.WorkerManager | Worker.Spawner` together is the rejected form here because the manager and the spawner enter as two composed rows, never one fused layer that hides the driver seam.
- Packages: `@effect/platform` for the `HttpClient`/`KeyValueStore`/`Worker.WorkerManager` service tags and the `Worker.layerManager` over-`PlatformWorker` lift; `@effect/platform-browser` for the `FetchHttpClient`, `BrowserKeyValueStore`, and `BrowserWorker.layerPlatform` browser bindings.
- Growth: a new platform binding lands as one platform-layer row on `BrowserPlatform`, never a parallel platform layer.
- Boundary: `BrowserPlatform` owns no policy and authors no decode; the key-value backing store choice is `local-persistence`'s concern composed over this binding, never a second store here; the worker driver is the `worker/` decode pool's `WorkerManager` requirement, never an inline `spawn` at a sink; the node platform bindings never enter this browser layer.

```ts contract
const spawnDecodeWorker = (id: number): globalThis.Worker =>
  new Worker(new URL("../worker/decode.worker.ts", import.meta.url), { type: "module", name: `decode-${id}` });

const BrowserPlatformLayer: Layer.Layer<
  HttpClient.HttpClient | KeyValueStore.KeyValueStore | Worker.WorkerManager
> = Layer.mergeAll(
  FetchHttpClient.layer,
  BrowserKeyValueStore.layerLocalStorage,
  Worker.layerManager.pipe(Layer.provide(BrowserWorker.layerPlatform(spawnDecodeWorker))),
);
```
