# [PLATFORM]

`platform` is the host-free browser/edge platform substrate and the SPA browser entry of the TypeScript branch — the browser AppHost-analog. It owns the runtime composition root and one runtime, the browser platform bindings, the OIDC/PKCE auth-session boot edge, the single typed runtime-config boundary, the self-telemetry export edge, the build and main-thread-offload pipeline, the local offline store, client routing and navigation, the service-worker plus offline-first cache with redial drain, the global crash/error-boundary fault sink, the feature-flag/remote-config read-side, and the web-vitals performance budget. It imports `ui`, `interchange`, and `projection` and consumes the C#-owned wire ONLY (decode, never re-mint); `ui` never imports `platform`; it owns no geometry and no domain state. The infrastructure owners are platform-bound HOST owners, never app-services — the closed five-app-service budget is unchanged. This README routes the design pages and lists the external packages; the domain folder-map is `ARCHITECTURE.md`, the forward pool `IDEAS.md`, the open work `TASKLOG.md`.

## [1]-[ROUTER]

The design pages under `.planning/`, grouped by sub-domain in build order; `ARCHITECTURE.md` carries the per-folder charter.

- runtime-composition: [composition-root](.planning/runtime-composition/composition-root.md) — the Layer graph, the one runtime, the `./web` entry; [browser-platform](.planning/runtime-composition/browser-platform.md) — the HTTP/key-value/worker platform bindings.
- identity-session: [auth-session](.planning/identity-session/auth-session.md) — OIDC PKCE, the session fold, refresh, and tokenHeader.
- runtime-config: [runtime-config](.planning/runtime-config/runtime-config.md) — the one Config schema and ConfigProvider boundary.
- observability: [metric-registry](.planning/observability/metric-registry.md) — the telemetry edge and the instrument/span vocabulary.
- build-pipeline: [build-pipeline](.planning/build-pipeline/build-pipeline.md) — the Vite plugin set and the styling/PWA-asset emit; [decode-worker-pool](.planning/build-pipeline/decode-worker-pool.md) — the transferable-buffer decode offload pool.
- local-persistence: [local-persistence](.planning/local-persistence/local-persistence.md) — the last-good snapshot and the offline command queue.
- routing: [app-router](.planning/routing/app-router.md) — the route axis, the param codec, the Navigation ingress; [navigation-guard](.planning/routing/navigation-guard.md) — the route-admission fold over auth and availability.
- offline-cache: [service-worker-host](.planning/offline-cache/service-worker-host.md) — the SW registration lifecycle and the cache strategy; [background-sync-replay](.planning/offline-cache/background-sync-replay.md) — the redial drain into the command gateway.
- fault-capture: [crash-telemetry](.planning/fault-capture/crash-telemetry.md) — the global capture, the crash fold, the report ship; [error-boundary-binding](.planning/fault-capture/error-boundary-binding.md) — the react-error-boundary integration and escalation.
- feature-flags: [remote-config](.planning/feature-flags/remote-config.md) — the FlagSet decode and the bucket evaluation.
- web-vitals: [performance-budget](.planning/web-vitals/performance-budget.md) — the PerformanceObserver capture and the breach fold.

The `feature-flags/flag-stream` (the SSE flag-delta ingress patching the FlagSet cell) and `session-replay` (trace-correlated sampled session recording) sub-domains are planned and hold no page yet; both are mapped in `ARCHITECTURE.md` and driven from `IDEAS.md`.

## [2]-[PACKAGES]

Every external library the folder uses, planned or implemented, as a flat list. No version pin (centralized in the one language manifest); no link into `.api/`. The trailing no-admission entries record concerns the platform owns natively, so a future reach for the named package is the rejected reflex rather than an open gap.

- `@effect/platform-browser` — the FetchHttpClient, BrowserKeyValueStore, BrowserWorker, BrowserStream, and BrowserHttpClient bindings
- `@opentelemetry/sdk-trace-web` — the browser trace SDK the WebSdk binds
- `@opentelemetry/sdk-metrics` — the metric reader the WebSdk binds
- `@opentelemetry/resources` — the telemetry resource attributes
- `@opentelemetry/semantic-conventions` — the standard span/resource attribute keys
- `idb-keyval` — the IndexedDB backing store under the KeyValueStore abstraction
- `arctic` — the OAuth/OIDC authorization-code-with-PKCE public-client flow
- `@simplewebauthn/browser` — the WebAuthn passkey registration/authentication ceremony (the `@simplewebauthn/server` verifier and `otplib` TOTP verification are the `services` `Authn` concern, never imported in `platform`)
- `nuqs` — the query-string state codec composed inside the route param codec
- `react` — the host render bridge for the error-boundary binding
- `react-error-boundary` — the React render-tree fault integration and the escalation hook
- `react-dom` — the browser root mount (`createRoot`) for the SPA entry in `browser.ts`
- `@vitejs/plugin-react` — the React Compiler and Fast Refresh Vite plugin
- `@rolldown/plugin-babel` — the Rolldown-Babel bridge required when `reactCompilerPreset` activates the React Compiler transform
- `@tailwindcss/vite` — the Tailwind CSS v4 Vite plugin driving the styling pipeline
- `vite` — the build toolchain
- `vite-plugin-image-optimizer` — the image asset optimization pass in the build pipeline
- `vite-plugin-svgr` — the SVG-to-React-component transform
- `vite-plugin-webfont-dl` — the web-font download and self-hosting emit
- `vite-plugin-pwa` — the PWA worker emit and precache manifest
- `vite-plugin-compression` — the precompressed asset emit
- `vite-plugin-csp` — the content-security-policy header emit
- `vite-plugin-inspect` — the build-graph diagnostics
- `rollup-plugin-visualizer` — the bundle-size diagnostics
- `browserslist` — the target-runtime matrix the transform set reads
- `workbox-build` — the RuntimeCaching/StrategyName projection for the cache strategy
- `workbox-window` — the Workbox registration and lifecycle event target
- NO-ADMISSION (deliberate): `react-router` / `tanstack-router` / `history` — `AppRouter` is Schema.Literal + nuqs + the native Navigation API
- NO-ADMISSION (deliberate): `web-vitals` — `PerformanceBudget` captures via the native PerformanceObserver API

## [3]-[CROSS_CUTTING]

Branch-level cross-cutting packages consumed by this folder.

- `effect` — the core: Layer graph, ManagedRuntime, Config/ConfigProvider, Schema, Schema.TaggedRequest, Match, SubscriptionRef, Schedule, Stream, Metric, Worker.makePoolSerialized, Data.TaggedEnum
- `@effect/platform` — the HTTP/KeyValueStore/WorkerManager service tags and the worker primitives
- `@effect/opentelemetry` — the WebSdk trace/metric exporter edge
