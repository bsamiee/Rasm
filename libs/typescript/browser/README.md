# [BROWSER]

`browser` is the W4 browser-runtime folder, the runtime peer of `ui`: components are capability, boot is runtime, and neither folder imports the other — `ui` declares port records and `browser` provides the Layers at app composition. `boot` ships the single `BrowserRuntime.runMain` law — a second boot is the named defect — and the `AppSpec` budget VALUE apps construct; the closed five-feed budget is Rasm-product guidance an app constructs, never lib law. `shell` owns the PWA shell: workbox service-worker rows, background-sync replay, and manifest/install/update rows. `persist` owns local persistence: the `idb-keyval` typed KV lane, the OPFS sqlite-wasm local-first lane, and the `EventLog` overlay client — overlay law, never the record of truth. `transport` owns the decode-worker pool — frame reassembly and off-thread content-key verify delegating to the one `kernel/identity` mint — and fetch/stream rows with backpressure. `session` composes `security`'s runtime-neutral ceremony subpaths (webauthn/oauth) with the browser session/token storage law; node-only crypto stays out of browser bundles by tag law. `route` owns Navigation-API routing under the zero-routing-package law: a Schema-typed route table with traversal folds and the nuqs query-state composition, plus admission/confirm guard folds over `security`/`host` verdicts. The render posture is law: client-rendered PWA plus build-time prerender rows — per-route static HTML emitted at app build, hydrated by `boot` — own the SEO surface; a streaming-SSR react server runtime is the named non-goal. The domain map and seam record live in `ARCHITECTURE.md`, the forward concept pool in `IDEAS.md`, and the work log in `TASKLOG.md`.

## [1]-[ROUTER]

- [01]-[RUNTIME](.planning/boot/runtime.md)
- [02]-[CONNECT](.planning/boot/connect.md)
- [03]-[WORKER](.planning/shell/worker.md)
- [04]-[INSTALL](.planning/shell/install.md)
- [05]-[KV](.planning/persist/kv.md)
- [06]-[OPFS](.planning/persist/opfs.md)
- [07]-[POOL](.planning/transport/pool.md)
- [08]-[FETCH](.planning/transport/fetch.md)
- [09]-[CEREMONY](.planning/session/ceremony.md)
- [10]-[STORE](.planning/session/store.md)
- [11]-[NAVIGATE](.planning/route/navigate.md)
- [12]-[GUARD](.planning/route/guard.md)

## [2]-[DOMAIN_PACKAGES]

Every browser-domain library the folder uses. Versions are centralized in the `pnpm-workspace.yaml` catalog and never pinned here; API evidence lives in the adjacent `.api/` folder.

[LOCAL_PERSISTENCE]:
- `idb-keyval` — the `persist/kv` typed KV lane over IndexedDB.

[ROUTE_STATE]:
- `nuqs` — the `route/navigate` query-state composition; URL query state only, never a router — the zero-routing-package law stands.

[PWA_SHELL]:
- `workbox-build` — the PWA build rows: precache manifest emitted at app build.
- `workbox-window` — window-side service-worker lifecycle, update, and background-sync replay rows.

## [3]-[SUBSTRATE_PACKAGES]

The branch substrate the folder consumes; the registry is `../.planning/README.md` and the catalogues are `libs/typescript/.api/`.

- `effect` — rails, `Schema`, `Layer`, `Match`, `Stream` vocabulary substrate.
- `@effect/platform` — the abstract `Worker`, `Transferable`, and `HttpClient` contracts the transport pool and fetch rows compose; the browser binding satisfies them at composition.
- `@effect/platform-browser` — the browser platform binding backing the runtime.
- `@effect/experimental` — the `EventLog` local-first client, overlay lanes only.
