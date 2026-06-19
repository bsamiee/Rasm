# [PLATFORM_ARCHITECTURE]

The domain map of `platform` — the browser host-free platform substrate and SPA AppHost-analog. One `CompositionRoot` folds the app-services into one `Layer` graph and runtime; every infrastructure concern is a platform-bound host owner across the `Runtime`, `Config`, `Transport`, `Session`, `Observability`, and `Shell` folders.

Each codemap node is the eventual source file its `.planning/` design page becomes, named in the language's own folder and file casing — PascalCase `.cs`, lowercase `.py`, lowercase `.ts`. Treat every node as realized code; the `.planning/` scaffold is the authoring substrate, never part of the map.

## [1]-[DOMAIN_MAP]

```text codemap
platform/
├── runtime/              # Composition root, browser bindings, runtime-state spine, and connectivity edge
│   ├── composition.ts    # One Layer graph, one ManagedRuntime, and SPA boot entry
│   ├── bindings.ts       # HTTP/key-value/worker platform layer every host owner composes
│   ├── events.ts         # Generic addEventListener->removeEventListener Stream bridge
│   ├── lifecycle.ts      # One closed Phase enum folding visibility/freeze/unload
│   ├── rank.ts           # One closed Rank fold with escalate-fast/recover-slow hysteresis
│   └── connectivity.ts   # Online/offline cell, redial edge, and SyncManager wake
├── config/               # Typed env boundary and feature-flag read-side
│   ├── config.ts         # One Config schema and one ConfigProvider over browser env snapshot
│   ├── flags.ts          # Decode-once FlagSet and deterministic-bucket flag evaluation
│   └── stream.ts         # Native EventSource SSE flag-delta ingress patching FlagSet cell
├── transport/            # Bidirectional socket modality and main-thread-offload decode leg
│   ├── socket.ts         # One BrowserSocket duplex Channel and decoded inbound frame stream
│   ├── modality.ts       # Closed TransportModality (WebSocket/WebTransport) growth axis
│   └── decode.ts         # Transferable Worker.makePool decode offload and content-key mint
├── session/              # Browser credential lifecycle, client routing, and local store
│   ├── session.ts        # OIDC PKCE acquisition, session fold, silent refresh, and tokenHeader
│   ├── router.ts         # Route-key axis, param codec, and Navigation API ingress
│   ├── guard.ts          # Route-admission fold over auth status and projection availability
│   └── store.ts          # Schema-encoded last-good snapshot and offline command queue
├── observability/        # Self-telemetry export edge, web-vitals, crash sink, and session replay
│   ├── telemetry.ts      # Instrument/span vocabulary, trace Sampler row, and OTLP WebSdk path
│   ├── vitals.ts         # PerformanceObserver capture, threshold fold, and breach span
│   ├── crash.ts          # Global capture, crash fold, and sanitized CrashReport ship
│   ├── boundary.ts       # React-error-boundary integration and escalation hook
│   └── replay.ts         # Flag-gated sampled DOM/interaction recorder with redaction-at-capture
└── shell/                # PWA service-worker shell, offline drain, build pipeline, and capability grants
    ├── serviceworker.ts  # Registration lifecycle and cache-strategy axis
    ├── sync.ts           # Redial-driven drain of offline queue into gateway
    ├── build.ts          # Vite plugin set and styling/PWA-asset emit (build-time only)
    ├── capability.ts     # Permissions-backed CapabilityKind axis and per-kind PermissionState cell
    └── grant.ts          # PermissionStatus.change -> PermissionState fold patching per-kind cell
```

## [2]-[SEAMS]

```text seams
transport/transport      ←  csharp:Rasm.Compute/Runtime        # [WIRE]: ArtifactFrameWire reassembly
observability/telemetry  ⇄  csharp:Rasm.AppHost/Observability  # [TRANSPORT]: W3C trace-context continuation
observability/telemetry  ←  csharp:Rasm.AppHost/Observability  # [TRANSPORT]: OtelExport OTLP egress
transport/decode         ←  csharp:Rasm.AppUi/Render           # [PROJECTION]: GeometryResidencyWire ResidencyManifest decode
transport/decode         ⇄  typescript:ui/render               # [CONTENT_KEY]: ContentKey mint and tile keying
transport/decode         ⇄  typescript:interchange/ingress     # [CONTENT_KEY]: ContentKey brand mint and tile keying
config/stream            ⇄  typescript:projection/fold         # [PORT]: StreamPolicy reconnect reuse
persistence              ←  typescript:ui/binding              # [PORT]: LocalPersistence KeyValueStore via Atom.kvs
session/session          →  typescript:services/security       # [PORT]: TOTP/WebAuthn ceremony verification
transport                →  typescript:ui/render               # [PORT]: ViewportHost / DecodeWorkerPool / BrowserPlatform
runtime/bindings         →  typescript:interchange/codec       # [WIRE]: DecodeWorkerPool TaggedRequest serialization
transport/socket         →  typescript:interchange/codec       # [WIRE]: inbound frame decode via Schema.decodeUnknown
shell/sync               →  typescript:interchange/transport   # [TRANSPORT]: offline queue drain resolved-intent replay
```

## [3]-[ALTITUDE]

The folder is the browser AppHost-analog: it owns runtime composition and host policy, never domain state, decode, or UI components. The infrastructure owners, the runtime-state spine (`AppLifecycle`, `CapabilityRank`), the `Connectivity` edge, the `BrowserCapability` permission edge, the `SocketTransport` modality, and the `DecodeWorkerPool` leg are platform-bound host owners, never one of the closed five app-services. `Shell/capability` is the single permission-grant owner the host policies resolve through, feeding `Runtime/rank` a denied-storage health input; `Transport/socket` is the single bidirectional-socket owner the `WebTransport`/CRDT-push-back legs compose. `Runtime/lifecycle` is the one page-lifecycle axis the former `visibilitychange` ingresses project from; `Runtime/connectivity` is the one online/offline owner read by `Runtime/rank`, `Shell/sync`, `Config/flags`, and `Observability/telemetry`. `Config/config` is the single env boundary, `Observability/telemetry` the single instrument owner and collector path, `Session/store` the single browser-local store, and uncaught faults reconstruct as the `interchange` typed fault family. The PWA shell splits at the build edge — `Shell/build` build-time-only, `Shell/serviceworker` the SW lifecycle, `Shell/sync` the redial drain — and `Transport/decode` hosts the residency-wire decode and the single content-key mint.
