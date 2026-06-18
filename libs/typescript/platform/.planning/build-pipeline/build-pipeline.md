# [PLATFORM_BUILD_PIPELINE]

One page owns the build and asset-emit pipeline — `BuildPipeline`, the Vite plugin set, the styling and PWA-asset emit, and the production stripping of the development atom inspector. `BuildPipeline` produces the co-hosted same-origin bundle and EMITS the service-worker asset; `offline-cache`'s `ServiceWorkerHost` owns the worker's runtime lifecycle, never re-emitting the asset — the two are distinct altitudes over one concern. The page holds no domain state and authors no decode.

## [1]-[INDEX]

[BUILD_PIPELINE]: the Vite plugin set, the styling/PWA emit, and the SW asset.

## [2]-[BUILD_PIPELINE]

- Owner: `BuildPipeline`, the build and PWA and styling pipeline producing the co-hosted same-origin asset bundle.
- Cases: `BuildPipeline` produces the co-hosted same-origin asset bundle with its plugin set and the styling pipeline, EMITS the service-worker asset through `vite-plugin-pwa` (the `offline-cache` `ServiceWorkerHost` owns the install/activate/cache-strategy/background-sync runtime lifecycle of that asset), and strips the development-build atom inspector from the production bundle; the `offline-cache` `runtimeCachingManifest` and the precache `NavigationRoute` app-shell fallback are read by `vite-plugin-pwa` at build time, so the emitted worker asset carries the resolved `RuntimeCaching` array authored by the cache-strategy owner, never a per-route handler authored here.
- Packages: `vite` for the build toolchain; `vite-plugin-pwa` for the PWA worker emit and the precache manifest; `vite-plugin-compression` for the precompressed asset emit; `vite-plugin-csp` for the content-security-policy header emit; `vite-plugin-inspect` and `rollup-plugin-visualizer` for the build-graph and bundle-size diagnostics; `browserslist` for the target-runtime matrix the transform set reads.
- Growth: a new build concern lands as one plugin row, never a parallel build pipeline.
- Boundary: `BuildPipeline` EMITS the worker ASSET at build time and `ServiceWorkerHost` (`offline-cache`) owns the SW RUNTIME lifecycle — distinct altitudes, one concern each, so a strategy or lifecycle row authored on `BuildPipeline` is the named two-owner-one-concern defect; the cache-strategy vocabulary the manifest carries is owned by `offline-cache`, referenced here as settled, never re-authored; the pipeline authors no decode and holds no domain state.
