# [BROWSER_ARCHITECTURE]

The domain map of `browser` — the W4 browser-runtime folder, runtime peer of `ui`. One boot law admits an app once (`BrowserRuntime.runMain` plus the `AppSpec` budget VALUE), and the `shell`, `persist`, `transport`, `session`, and `route` sub-domains carry the PWA shell, local persistence, the decode-worker transport pool, session ceremonies, and Navigation-API routing; the port records `ui` declares are satisfied here at app composition, never by import.

Each codemap node is the eventual source file its `.planning/` design page becomes, named in the language's own folder and file casing — PascalCase `.cs`, lowercase `.py`, lowercase `.ts`. Treat every node as realized code; the `.planning/` scaffold is the authoring substrate, never part of the map.

## [01]-[DOMAIN_MAP]

```text codemap
browser/src/
├── boot/                # single-boot runtime law and connectivity state
│   ├── runtime.ts       # the single BrowserRuntime.runMain law (a second boot is the named defect) + the AppSpec budget VALUE apps construct
│   └── connect.ts       # connectivity/visibility/network state rows
├── shell/               # PWA shell
│   ├── worker.ts        # service-worker/workbox rows + background-sync replay
│   └── install.ts       # PWA manifest/install/update rows
├── persist/             # local persistence
│   ├── kv.ts            # idb-keyval typed KV
│   └── opfs.ts          # OPFS sqlite-wasm local-first lane + the EventLog overlay client
├── transport/           # decode transport
│   ├── pool.ts          # decode worker pool: frame reassembly + off-thread content-key verify (kernel-delegating mint site)
│   └── fetch.ts         # fetch/stream transport rows + backpressure
├── session/             # session ceremonies and storage
│   ├── ceremony.ts      # webauthn/oauth browser ceremonies over security runtime-neutral subpaths
│   └── store.ts         # browser session/token storage law
└── route/               # Navigation-API routing
    ├── navigate.ts      # Navigation-API typed router: Schema route table/params + traversal folds (the nuqs composition site); zero routing package
    └── guard.ts         # NavigationGuard admission/confirm folds over security/host verdicts
```

## [02]-[SEAMS]

```text seams
transport/pool  ←  csharp:Rasm.Compute/Runtime/channels  # [WIRE]: ArtifactFrameWire reassembly
transport/pool  ←  csharp:Rasm.AppUi/Render/pipeline     # [PROJECTION]: GeometryResidencyWire ResidencyManifest content-key
transport/pool  ←  typescript:kernel/identity            # [CONTENT_KEY]: the delegating mint site — off-thread ContentKey verify through the one kernel mint, never a second content-address notion
transport/pool  →  typescript:ui/viewer/scene/glb        # [PORT]: the GlbViewport decode-worker residency port record ui declares; browser provides the Layer at app composition
```
