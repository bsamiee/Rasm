# [PLATFORM_ARCHITECTURE]

The professional domain map of `platform` — the browser host-free platform substrate and SPA AppHost-analog. One `CompositionRoot` folds the five app-services into one `Layer` graph and runtime; every infrastructure concern is a platform-bound host owner across six folders (`Runtime`, `Config`, `Transport`, `Session`, `Observability`, `Shell`).

Each codemap node is the eventual source file its `.planning/` design page becomes, named in the language's own folder and file casing — PascalCase `.cs`, lowercase `.py`, lowercase `.ts`. Treat every node as realized code; the `.planning/` scaffold is the authoring substrate, never part of the map.

## [1]-[DOMAIN_MAP]

```text codemap
platform/
├── runtime/              # the composition root, browser bindings, runtime-state spine, and connectivity edge
│   ├── composition.ts    # the one Layer graph, the one ManagedRuntime, and the SPA boot entry
│   ├── bindings.ts       # the HTTP/key-value/worker platform layer every host owner composes
│   ├── events.ts         # the generic addEventListener->removeEventListener Stream bridge
│   ├── lifecycle.ts      # the one closed Phase enum folding visibility/freeze/unload
│   ├── rank.ts           # the one closed Rank fold with escalate-fast/recover-slow hysteresis
│   └── connectivity.ts   # the online/offline cell, redial edge, and SyncManager wake
├── config/               # the typed env boundary and the feature-flag read-side
│   ├── config.ts         # one Config schema and one ConfigProvider over the browser env snapshot
│   ├── flags.ts          # the decode-once FlagSet and deterministic-bucket flag evaluation
│   └── stream.ts         # the native EventSource SSE flag-delta ingress patching the FlagSet cell
├── transport/            # the bidirectional socket modality and the main-thread-offload decode leg
│   ├── socket.ts         # the one BrowserSocket duplex Channel and decoded inbound frame stream
│   ├── modality.ts       # the closed TransportModality (WebSocket/WebTransport) growth axis
│   └── decode.ts         # the transferable Worker.makePool decode offload and content-key mint
├── session/              # the browser credential lifecycle, client routing, and local store
│   ├── session.ts        # OIDC PKCE acquisition, the session fold, silent refresh, and tokenHeader
│   ├── router.ts         # the route-key axis, param codec, and Navigation API ingress
│   ├── guard.ts          # the route-admission fold over auth status and projection availability
│   └── store.ts          # the Schema-encoded last-good snapshot and the offline command queue
├── observability/        # the self-telemetry export edge, web-vitals, crash sink, and session replay
│   ├── telemetry.ts      # the instrument/span vocabulary, trace Sampler row, and OTLP WebSdk path
│   ├── vitals.ts         # the PerformanceObserver capture, threshold fold, and breach span
│   ├── crash.ts          # the global capture, crash fold, and sanitized CrashReport ship
│   ├── boundary.ts       # the react-error-boundary integration and the escalation hook
│   └── replay.ts         # the flag-gated sampled DOM/interaction recorder with redaction-at-capture
└── shell/                # the PWA service-worker shell, offline drain, build pipeline, and capability grants
    ├── serviceworker.ts  # the registration lifecycle and the cache-strategy axis
    ├── sync.ts           # the redial-driven drain of the offline queue into the gateway
    ├── build.ts          # the Vite plugin set and the styling/PWA-asset emit (build-time only)
    ├── capability.ts     # the Permissions-backed CapabilityKind axis and per-kind PermissionState cell
    └── grant.ts          # the PermissionStatus.change -> PermissionState fold patching the per-kind cell
```

## [2]-[SEAMS]

```text seams
transport/transport      ←  csharp:Rasm.Compute/Runtime        # ArtifactFrameWire reassembly (wire)
observability/telemetry  ⇄  csharp:Rasm.AppHost/Observability  # W3C trace-context continuation (transport)
observability/telemetry  ←  csharp:Rasm.AppHost/Observability  # OtelExport OTLP egress (transport)
transport/decode         ←  csharp:Rasm.AppUi/Render           # GeometryResidencyWire ResidencyManifest decode (projection)
transport/decode         ⇄  typescript:ui/render               # ContentKey mint and tile keying (content-key)
transport/decode         ⇄  typescript:interchange/ingress     # ContentKey brand mint and tile keying (content-key)
config/stream            ⇄  typescript:projection/fold         # StreamPolicy reconnect reuse (port)
persistence              ←  typescript:ui/binding              # LocalPersistence KeyValueStore via Atom.kvs (port)
session/session          →  typescript:services/security       # TOTP/WebAuthn ceremony verification (port)
transport                →  typescript:ui/render               # ViewportHost / DecodeWorkerPool / BrowserPlatform (port)
runtime/bindings         →  typescript:interchange/codec       # DecodeWorkerPool TaggedRequest serialization (wire)
transport/socket         →  typescript:interchange/codec       # inbound frame decode via Schema.decodeUnknown (wire)
shell/sync               →  typescript:interchange/transport   # offline queue drain resolved-intent replay (transport)
```

## [3]-[ALTITUDE]

The folder is the browser AppHost-analog: it owns runtime composition and host policy, never domain state, decode, or UI components. The infrastructure owners, the runtime-state spine (`AppLifecycle`, `CapabilityRank`), the `Connectivity` edge, the `BrowserCapability` permission edge, the `SocketTransport` modality, and the `DecodeWorkerPool` leg are platform-bound host owners, never one of the closed five app-services. `Shell/capability` is the single permission-grant owner the host policies resolve through, feeding `Runtime/rank` a denied-storage health input; `Transport/socket` is the single bidirectional-socket owner the `WebTransport`/CRDT-push-back legs compose. `Runtime/lifecycle` is the one page-lifecycle axis the former `visibilitychange` ingresses project from; `Runtime/connectivity` is the one online/offline owner read by `Runtime/rank`, `Shell/sync`, `Config/flags`, and `Observability/telemetry`. `Config/config` is the single env boundary, `Observability/telemetry` the single instrument owner and collector path, `Session/store` the single browser-local store, and uncaught faults reconstruct as the `interchange` typed fault family. The PWA shell splits at the build edge — `Shell/build` build-time-only, `Shell/serviceworker` the SW lifecycle, `Shell/sync` the redial drain — and `Transport/decode` hosts the residency-wire decode and the single content-key mint.
