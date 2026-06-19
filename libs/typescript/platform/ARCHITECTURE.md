# [PLATFORM_ARCHITECTURE]

The professional domain folder-map of the browser host-free platform substrate. `platform` is the SPA browser entry and AppHost-analog: one `CompositionRoot` composes the closed five app-services into one `Layer` graph and one runtime, and every infrastructure concern is one platform-bound host owner — never a sixth app-service. The runtime-state spine (`app-lifecycle`, `capability-rank`), the `connectivity` edge, and the operator-named `worker/` decode leg are platform-bound HOST owners on the same budget, not app-services. The map is the sub-domain structure mirroring the eventual source tree, including the `session-replay` sub-domain whose `session-recorder` page binds the sampled recorder to the `observability` trace context. Each leaf carries a one-line charter; mechanics live in the `.planning/<sub-domain>/<page>.md` design pages.

## [1]-[DOMAIN_MAP]

```text codemap
platform/
├── runtime-composition/                  # the composition root, browser platform bindings, and the runtime-state spine
│   ├── composition-root                  # the one Layer graph, the one ManagedRuntime, and the BrowserRuntime.runMain ./web SPA boot entry
│   ├── browser-platform                  # the HTTP/key-value/worker platform layer every host owner composes
│   ├── scoped-event-stream               # the generic addEventListener->removeEventListener Stream bridge for non-WindowEventMap targets
│   ├── app-lifecycle                     # the one closed Phase enum folding visibilitychange + pagehide(freeze) + beforeunload
│   └── capability-rank                   # the one closed Rank fold (Full/Degraded/OfflineOnly/Draining) with escalate-fast/recover-slow hysteresis
├── connectivity/                         # the single online/offline connectivity edge
│   └── connectivity                      # the online/offline cell, the redial edge, and the native SyncManager wake, read by >=4 concerns
├── capabilities/                         # the single browser permission/host-capability grant edge
│   ├── browser-capability                # the one Permissions-backed CapabilityKind axis, the per-kind PermissionState cell, and the storage-persist quota grant feeding capability-rank
│   └── permission-grant-fold             # the PermissionStatus.change -> PermissionState fold over scopedEventStream patching the per-kind cell
├── realtime/                             # the single bidirectional socket transport modality
│   ├── socket-transport                  # the one BrowserSocket duplex Channel, the decoded inbound frame stream, and the outbound write over the one scoped resource
│   └── transport-modality                # the closed TransportModality Data.TaggedEnum (WebSocket/WebTransport) growth axis under the reused StreamPolicy reconnect
├── identity-session/                     # the browser credential lifecycle
│   └── auth-session                      # OIDC PKCE acquisition + redirect-continuity round-trip, the session fold, silent refresh, revocation, and tokenHeader
├── runtime-config/                       # the single typed env boundary
│   └── runtime-config                    # one Config schema and one ConfigProvider over the browser env snapshot
├── observability/                        # the self-telemetry export edge
│   └── metric-registry                   # the bounded instrument/span vocabulary, the trace Sampler row, and the OTLP WebSdk collector path
├── build-pipeline/                       # the build-time-only asset-emit pipeline
│   └── build-pipeline                    # the Vite plugin set and the styling/PWA-asset emit (no worker runtime after the worker/ move)
├── worker/                               # the operator-named top-level browser decode leg
│   └── decode-pool                       # the transferable Worker.makePool snapshot/artifact-frame/residency offload and the single content-key mint
├── local-persistence/                    # the single browser-local store
│   └── local-persistence                 # the Schema-encoded last-good snapshot and the offline command queue
├── routing/                              # the client routing and navigation infrastructure
│   ├── app-router                        # the route-key axis, the param codec, and the Navigation API ingress
│   └── navigation-guard                  # the route-admission fold over auth status and projection availability
├── offline-cache/                        # the service-worker / PWA offline-first cache
│   ├── service-worker-host               # the registration lifecycle and the cache-strategy axis
│   └── background-sync-replay            # the redial-driven drain of the offline queue into the gateway over the connectivity cell
├── fault-capture/                        # the browser crash/error-boundary fault sink
│   ├── crash-telemetry                   # the global capture, the crash fold, and the sanitized CrashReport ship
│   └── error-boundary-binding            # the react-error-boundary integration and the escalation hook
├── feature-flags/                        # the feature-flag and remote-config read-side
│   ├── remote-config                     # the decode-once FlagSet, the deterministic-bucket flag evaluation, and the poll demoted to backfill
│   └── flag-stream                       # the native EventSource SSE flag-delta ingress patching the FlagSet cell in place for near-real-time propagation
├── web-vitals/                           # the performance-budget and web-vitals observability
│   └── performance-budget                # the native PerformanceObserver capture, the threshold fold, and the breach span
└── session-replay/                       # the trace-correlated, privacy-sanitized sampled session recording bound to the SelfTelemetry trace context
    └── session-recorder                  # the flag-gated sampled DOM/interaction recorder, the redaction-at-capture fold, and the replay-window id the crash/breach spans annotate
```

## [2]-[ALTITUDE]

The folder is the browser AppHost-analog: it owns runtime composition and host policy, never domain state, decode, or UI components. The five infrastructure owners (`AppRouter`, `ServiceWorkerHost`, `CrashTelemetry`, `RemoteConfig`, `PerformanceBudget`), the runtime-state spine (`AppLifecycle`, `CapabilityRank`), the `Connectivity` edge, the `BrowserCapability` permission edge, the `SocketTransport` modality, and the `worker/` `DecodeWorkerPool` are platform-bound host owners exactly as `AuthSession`/`BrowserPlatform`/`SelfTelemetry`; none enters the closed five-app-service budget. `BrowserCapability` is the single permission-grant owner the host policies (notification, clipboard, geolocation, persistent-storage) resolve through, holding the per-kind `PermissionState` cell `ui` greys a denied affordance from and feeding `CapabilityRank` a denied-storage health input; `SocketTransport` is the single bidirectional-socket owner the SSE-only ingress lacks, the modality the `WebTransport`/CRDT-push-back legs compose rather than a parallel socket. `app-lifecycle` is the single page-lifecycle axis the three former private `visibilitychange` ingresses (`web-vitals`/`feature-flags`/`fault-capture`) project from; `capability-rank` is the one capability cell `ui` reads and holds no behavior; `connectivity` is the single online/offline owner read by `capability-rank`, `background-sync-replay`, `feature-flags`, and `observability`; `scoped-event-stream` is the one generic listener bridge the non-`WindowEventMap` ingresses compose. `runtime-config` is the single env boundary; `observability` is the single instrument owner, the single collector path, and the one trace-Sampler row; `local-persistence` is the single browser-local store; uncaught faults reconstruct as the `interchange` typed fault family; the flag bucket/variant vocabulary is referenced from `services` as settled and `flag-stream`'s SSE delta patches the one `RemoteConfig` cell. The PWA worker concern splits at the build edge — `build-pipeline` is build-time-only and `offline-cache` owns the SW lifecycle — while the main-thread-offload decode pool is the operator-named `worker/` leg hosting the `[WEB_GEOMETRY_RESIDENCY_WIRE]` decode and the single content-key mint; the dependency direction within the branch (`platform` imports `ui`/`interchange`/`projection`; `ui` never imports `platform`) is stated once at the branch `ARCHITECTURE.md`, never restated here.
