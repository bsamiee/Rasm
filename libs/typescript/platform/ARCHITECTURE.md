# [PLATFORM_ARCHITECTURE]

The professional domain folder-map of the browser host-free platform substrate. `platform` is the SPA browser entry and AppHost-analog: one `CompositionRoot` composes the closed five app-services into one `Layer` graph and one runtime, and every infrastructure concern is one platform-bound host owner — never a sixth app-service. The map is the sub-domain structure mirroring the eventual source tree, including the planned-but-empty `session-replay` sub-domain that fuels an idea. Each leaf carries a one-line charter; mechanics live in the `.planning/<sub-domain>/<page>.md` design pages.

## [1]-[DOMAIN_MAP]

```text codemap
platform/
├── runtime-composition/                  # the composition root and browser platform bindings
│   ├── composition-root                  # the one Layer graph, the one ManagedRuntime, and the ./web SPA boot entry
│   └── browser-platform                  # the HTTP/key-value/worker platform layer every host owner composes
├── identity-session/                     # the browser credential lifecycle
│   └── auth-session                      # OIDC PKCE acquisition, the session fold, silent refresh, and tokenHeader
├── runtime-config/                       # the single typed env boundary
│   └── runtime-config                    # one Config schema and one ConfigProvider over the browser env snapshot
├── observability/                        # the self-telemetry export edge
│   └── metric-registry                   # the bounded instrument/span vocabulary and the OTLP WebSdk collector path
├── build-pipeline/                       # the build and main-thread-offload pipeline
│   ├── build-pipeline                    # the Vite plugin set, the styling/PWA-asset emit
│   └── decode-worker-pool                # the transferable-buffer Worker.makePool snapshot/artifact-frame offload
├── local-persistence/                    # the single browser-local store
│   └── local-persistence                 # the Schema-encoded last-good snapshot and the offline command queue
├── routing/                              # the client routing and navigation infrastructure
│   ├── app-router                        # the route-key axis, the param codec, and the Navigation API ingress
│   └── navigation-guard                  # the route-admission fold over auth status and projection availability
├── offline-cache/                        # the service-worker / PWA offline-first cache
│   ├── service-worker-host               # the registration lifecycle and the cache-strategy axis
│   └── background-sync-replay            # the redial-driven drain of the offline queue into the gateway
├── fault-capture/                        # the browser crash/error-boundary fault sink
│   ├── crash-telemetry                   # the global capture, the crash fold, and the sanitized CrashReport ship
│   └── error-boundary-binding            # the react-error-boundary integration and the escalation hook
├── feature-flags/                        # the feature-flag and remote-config read-side
│   ├── remote-config                     # the decode-once FlagSet, the deterministic-bucket flag evaluation, and the poll/backfill fallback
│   └── flag-stream                       # PLANNED — the SSE flag-delta ingress patching the FlagSet cell in place for near-real-time propagation
├── web-vitals/                           # the performance-budget and web-vitals observability
│   └── performance-budget                # the native PerformanceObserver capture, the threshold fold, and the breach span
└── session-replay/                       # PLANNED — trace-correlated, privacy-sanitized sampled session recording bound to the SelfTelemetry trace context
```

## [2]-[ALTITUDE]

The folder is the browser AppHost-analog: it owns runtime composition and host policy, never domain state, decode, or UI components. The five infrastructure owners (`AppRouter`, `ServiceWorkerHost`, `CrashTelemetry`, `RemoteConfig`, `PerformanceBudget`) are platform-bound host owners exactly as `AuthSession`/`BrowserPlatform`/`SelfTelemetry`; none enters the closed five-app-service budget. `runtime-config` is the single env boundary; `observability` is the single instrument owner and the single collector path; `local-persistence` is the single browser-local store; uncaught faults reconstruct as the `interchange` typed fault family; the flag bucket/variant vocabulary is referenced from `services` as settled. The PWA worker concern splits at the build edge — `build-pipeline` emits the asset, `offline-cache` owns its lifecycle — and the dependency direction within the branch (`platform` imports `ui`/`interchange`/`projection`; `ui` never imports `platform`) is stated once at the branch `ARCHITECTURE.md`, never restated here.
