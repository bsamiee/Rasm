# [PLATFORM_BROWSER_PLATFORM]

One page owns the browser platform bindings layer — `BrowserPlatform`, the one `Layer.mergeAll` of the `@effect/platform-browser` fetch HTTP client, the local-storage key-value store, and the worker-manager binding that every host owner composes for its IO primitives. The layer is config-free stock bindings sitting directly above `RuntimeConfig` in the graph; it owns no policy and authors no decode.

## [1]-[INDEX]

[BROWSER_PLATFORM]: the HTTP client, the key-value store, and the worker-manager binding.

## [2]-[BROWSER_PLATFORM]

- Owner: `BrowserPlatform`, the browser platform layer owning the HTTP client, the key-value store, and the `WorkerManager` binding bound under one `Layer.mergeAll`.
- Cases: `BrowserPlatform` binds the platform services from one `Layer.mergeAll` of the `@effect/platform-browser` `FetchHttpClient.layer`, `BrowserKeyValueStore.layerLocalStorage`, and the `WorkerManager` binding, so the fetch client `interchange` dials over, the key-value store `local-persistence` composes, and the `WorkerManager` `build-pipeline`'s `DecodeWorkerPool` requires each resolve from this one binding rather than a per-owner platform layer; the layer is config-free stock bindings, so it sits directly above `RuntimeConfig.provider` in the graph and below the config-reading owners.
- Auto: the `WorkerManager` binding is `WorkerManager.layerManager` provided over `BrowserWorker.layerPlatform(spawn)` — `layerPlatform` yields `PlatformWorker | Spawner` from the `spawn` factory that constructs the decode-worker entry module, and `layerManager` lifts that `PlatformWorker` into the `WorkerManager` the pool requires; the `BrowserWorker.layer(spawn)` one-shot that yields `WorkerManager | Spawner` together is the rejected form here because the manager and the spawner enter as two composed rows, never one fused layer that hides the driver seam.
- Packages: `@effect/platform` for the `HttpClient`/`KeyValueStore`/`WorkerManager` service tags and the `WorkerManager.layerManager` over-`PlatformWorker` lift; `@effect/platform-browser` for the `FetchHttpClient`, `BrowserKeyValueStore`, and `BrowserWorker.layerPlatform` browser bindings.
- Growth: a new platform binding lands as one platform-layer row on `BrowserPlatform`, never a parallel platform layer.
- Boundary: `BrowserPlatform` owns no policy and authors no decode; the key-value backing store choice is `local-persistence`'s concern composed over this binding, never a second store here; the worker driver is the `build-pipeline` decode pool's `WorkerManager` requirement, never an inline `spawn` at a sink; the node platform bindings never enter this browser layer.

```ts contract
const spawnDecodeWorker = (id: number): globalThis.Worker =>
  new Worker(new URL("../build-pipeline/decode.worker.ts", import.meta.url), { type: "module", name: `decode-${id}` });

const BrowserPlatformLayer: Layer.Layer<
  HttpClient.HttpClient | KeyValueStore.KeyValueStore | WorkerManager.WorkerManager
> = Layer.mergeAll(
  FetchHttpClient.layer,
  BrowserKeyValueStore.layerLocalStorage,
  WorkerManager.layerManager.pipe(Layer.provide(BrowserWorker.layerPlatform(spawnDecodeWorker))),
);
```
