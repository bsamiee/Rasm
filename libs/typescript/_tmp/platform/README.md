# [PLATFORM]

`platform` is the host-free browser/edge platform substrate and the SPA browser entry of the TypeScript branch — the browser `AppHost` analog. It owns the runtime composition root and one runtime, the browser platform bindings, the runtime-state spine (the page-lifecycle `Phase` axis, the capability-rank cell, the connectivity edge, the generic scoped-event-stream bridge), the OIDC/PKCE auth-session boot edge, the single typed runtime-config boundary, the self-telemetry export edge with the OTLP `WebSdk` and trace `Sampler`, the build-time-only asset pipeline, the `Transport/decode` main-thread-offload decode leg, the local offline store, client routing and navigation, the service-worker plus offline-first cache with redial drain, the global crash/error-boundary fault sink, the feature-flag/remote-config read-side with the SSE flag-stream, and the web-vitals performance budget. It imports `ui`, `interchange`, and `projection` and consumes the C#-owned wire only (decode, never re-mint); `ui` never imports `platform`; it owns no geometry and no domain state. The infrastructure owners — including the runtime-state spine and the `Transport/decode` decode leg — are platform-bound HOST owners, never app-services, so the closed five-app-service budget is unchanged. This README routes the design pages and lists the external packages; the domain folder-map is `ARCHITECTURE.md`, the forward pool `IDEAS.md`, and the open work `TASKLOG.md`.

## [01]-[ROUTER]

- [01]-[COMPOSITION](.planning/Runtime/composition.md): Layer graph, one runtime, and the `BrowserRuntime.runMain` `./web` entry.
- [02]-[BINDINGS](.planning/Runtime/bindings.md): HTTP/key-value/worker platform bindings.
- [03]-[EVENTS](.planning/Runtime/events.md): Generic `addEventListener` `Stream` bridge for non-`WindowEventMap` targets.
- [04]-[LIFECYCLE](.planning/Runtime/lifecycle.md): One `Phase` enum folding `visibilitychange`, `pagehide(freeze)`, and `beforeunload`.
- [05]-[RANK](.planning/Runtime/rank.md): One closed `Rank` fold with escalate-fast/recover-slow hysteresis.
- [06]-[CONNECTIVITY](.planning/Runtime/connectivity.md): Online/offline cell, the redial edge, and the native `SyncManager` wake.
- [07]-[CONFIG](.planning/Config/config.md): One `Config` schema and `ConfigProvider` boundary.
- [08]-[FLAGS](.planning/Config/flags.md): `FlagSet` decode and bucket evaluation.
- [09]-[STREAM](.planning/Config/stream.md): Native `EventSource` SSE flag-delta ingress patching the `FlagSet` cell.
- [10]-[SOCKET](.planning/Transport/socket.md): One `BrowserSocket` duplex `Channel` `SocketTransport` owner, the decoded inbound frame stream, and the outbound write over the one scoped resource.
- [11]-[MODALITY](.planning/Transport/modality.md): Closed `TransportModality` `Data.TaggedEnum` (WebSocket/WebTransport) growth axis under the reused `projection` `StreamPolicy` reconnect.
- [12]-[DECODE](.planning/Transport/decode.md): Transferable-buffer decode offload pool, the residency-manifest decode, and the single content-key mint.
- [13]-[SESSION](.planning/Session/session.md): OIDC PKCE, the session fold, refresh, and `tokenHeader`.
- [14]-[ROUTER](.planning/Session/router.md): Route axis, param codec, `Navigation` ingress, and the view-transition fold.
- [15]-[GUARD](.planning/Session/guard.md): Route-admission fold over auth and availability.
- [16]-[STORE](.planning/Session/store.md): Last-good snapshot and the offline command queue.
- [17]-[TELEMETRY](.planning/Observability/telemetry.md): Telemetry edge, the trace `Sampler` row, and the instrument/span vocabulary.
- [18]-[VITALS](.planning/Observability/vitals.md): `PerformanceObserver` capture, the breach fold, the soft-navigation reset, and the visibility-hidden terminal flush.
- [19]-[CRASH](.planning/Observability/crash.md): Global capture, crash fold, and report ship.
- [20]-[BOUNDARY](.planning/Observability/boundary.md): `react-error-boundary` integration and escalation.
- [21]-[REPLAY](.planning/Observability/replay.md): Flag-gated sampled DOM/interaction recorder, the redaction-at-capture fold, and the trace-correlated replay-window id.
- [22]-[SERVICEWORKER](.planning/Shell/serviceworker.md): SW registration lifecycle and the cache strategy.
- [23]-[SYNC](.planning/Shell/sync.md): Redial drain into the command gateway.
- [24]-[BUILD](.planning/Shell/build.md): Build-time-only Vite plugin set and the styling/PWA-asset emit.
- [25]-[CAPABILITY](.planning/Shell/capability.md): One Permissions-backed `BrowserCapability` owner over the notification/clipboard/geolocation/persistent-storage `CapabilityKind` axis, the per-kind `PermissionState` cell, and the storage-persist quota grant feeding `Runtime/rank`.
- [26]-[GRANT](.planning/Shell/grant.md): `PermissionStatus.change`->`PermissionState` fold over `scopedEventStream` patching the per-kind cell.

## [02]-[DOMAIN_PACKAGES]

Domain packages this folder uses, planned or implemented; versions are centralized in the one language workspace catalog and never pinned here. API evidence lives in the folder `.api/`. The no-admission entries record native-API concerns so a future reach for the named package is the rejected reflex rather than an open gap.

[PLATFORM_BINDINGS]:
- `@effect/platform-browser` — `BrowserKeyValueStore`, `BrowserWorker`, `BrowserStream`, `BrowserHttpClient` (the XHR client: progress + arraybuffer), the Permissions/Clipboard/Geolocation capability capsules, and the `BrowserSocket` duplex socket driver

[OTEL_SDK]:
- `@opentelemetry/core` — `W3CTraceContextPropagator`/`W3CBaggagePropagator` composite registered via `WebTracerProvider.register({propagator})` alongside the WebSdk layer as the extract-and-continue propagator for cross-runtime trace continuation
- `@opentelemetry/sdk-trace-web` — the browser trace SDK the WebSdk binds
- `@opentelemetry/sdk-metrics` — the metric reader the WebSdk binds
- `@opentelemetry/exporter-trace-otlp-http` — the concrete `SpanExporter` (OTLP/HTTP) bound under the batch span processor
- `@opentelemetry/exporter-metrics-otlp-http` — the concrete `PushMetricExporter` (OTLP/HTTP) bound under the periodic metric reader
- `@opentelemetry/resources` — the telemetry resource attributes
- `@opentelemetry/semantic-conventions` — the standard span/resource attribute keys

[STORAGE]:
- `idb-keyval` — the IndexedDB backing store under the `KeyValueStore` abstraction

[AUTH]:
- `arctic` — the OAuth/OIDC authorization-code-with-PKCE public-client flow
- `@simplewebauthn/browser` — the WebAuthn passkey registration/authentication ceremony (`@simplewebauthn/server` and `otplib` TOTP verification are the `services` `Authn` concern, never imported in `platform`)

[ROUTING]:
- `nuqs` — the query-string state codec composed inside the route param codec
- `react-error-boundary` — the React render-tree fault integration and the escalation hook
- NO-ADMISSION: `react-router` / `tanstack-router` / `history` — `AppRouter` is `Schema.Literal` + `nuqs` + the native Navigation API

[EFFECT_DOMAIN]:
- `@effect/experimental` — `Sse.makeParser`/`Sse.Retry.lastEventId` SSE-frame decode the `flag-stream` ingress folds (branch `.api/` catalogue; the `services` `agent/mcp` is the other consumer)

[BUILD_TOOLCHAIN]:
- `vite` — the build toolchain
- `@vitejs/plugin-react` — the React Compiler and Fast Refresh Vite plugin
- `@rolldown/plugin-babel` — the Rolldown-Babel bridge required when `reactCompilerPreset` activates the React Compiler transform
- `@tailwindcss/vite` — the Tailwind CSS v4 Vite plugin driving the styling pipeline
- `vite-plugin-image-optimizer` — the image asset optimization pass in the build pipeline
- `vite-plugin-svgr` — the SVG-to-React-component transform
- `vite-plugin-webfont-dl` — the web-font download and self-hosting emit
- `vite-plugin-compression` — the precompressed asset emit
- `vite-plugin-csp` — the content-security-policy header emit
- `vite-plugin-inspect` — the build-graph diagnostics
- `rollup-plugin-visualizer` — the bundle-size diagnostics
- `browserslist` — the target-runtime matrix the transform set reads

[PWA]:
- `vite-plugin-pwa` — the PWA worker emit and precache manifest
- `workbox-build` — the `RuntimeCaching`/`StrategyName` projection for the cache strategy
- `workbox-window` — the Workbox registration and lifecycle event target
- NO-ADMISSION: `web-vitals` — `PerformanceBudget` captures via the native `PerformanceObserver` API

## [03]-[SUBSTRATE_PACKAGES]

Branch-level substrate packages this folder consumes; charters and API evidence live in `libs/typescript/.planning/README.md` and the adjacent `libs/typescript/.api/` branch.

[RUNTIME_CORE]:
- `effect` — `Layer`, `ManagedRuntime`, `Config`/`ConfigProvider`, `Schema`, `Schema.TaggedRequest`, `Match`, `SubscriptionRef`, `Schedule`, `Stream`, `Metric`, `Worker.makePoolSerialized`, `Data.TaggedEnum`
- `@effect/platform` — the HTTP/`KeyValueStore`/`WorkerManager` service tags, the `FetchHttpClient.layer` fetch client the `BrowserPlatform` binds, and the worker primitives

[OBSERVABILITY]:
- `@effect/opentelemetry` — the WebSdk trace/metric exporter edge

[VIEW_CORE]:
- `react` — the React 19 core render bridge for the SPA shell and the error-boundary binding
- `react-dom` — the browser root mount (`createRoot`) and resource-hint surface for the SPA entry

[IDENTITY]:
- `hash-wasm` — the worker-interior `createXXHash128` 16-byte content-key mint (the catalogue lives at the `interchange` folder `.api/` as a shared content-address seam)
