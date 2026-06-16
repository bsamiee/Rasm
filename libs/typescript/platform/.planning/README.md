# [PLATFORM_PLANNING]

`platform` is the browser application INFRASTRUCTURE concern and the SPA browser ENTRY — the browser AppHost-analog. It owns the composition root, the host runtime, the platform substrate, and the flagship-SPA infrastructure a world-class browser application requires: client routing and navigation, the service-worker / PWA offline-first cache, the error-boundary and crash telemetry sink, the feature-flag read-side, and the web-vitals performance budget. Zero consumers exist; implementation is full-capability with no holding back; pages are transcribed, not re-designed. The `./web` entry (`platform/browser.ts`) is the one `CompositionRoot` that composes the `ui` library plus the neutral `interchange` and `projection` domains into one Layer graph and one runtime; `platform` imports `ui` (the entry composes the UI library) but `ui` never imports `platform`. The five new infrastructure owners are platform-bound HOST owners, never app-services — the closed five-app-service budget is unchanged.

## [1]-[PAGE_INDEX]

| [INDEX] | [PAGE]                  | [OWNS]                                                                                                          | [STATE]     |
| :-----: | :---------------------- | :------------------------------------------------------------------------------------------------------------- | :---------- |
|   [1]   | host-runtime.md         | CompositionRoot + BrowserPlatform + AuthSession + RuntimeConfig + the `./web` SPA entry Layer graph              | provisional |
|   [2]   | platform-substrate.md   | SelfTelemetry + MetricRegistry + BuildPipeline + DecodeWorkerPool + LocalPersistence                            | provisional |
|   [3]   | routing-navigation.md   | AppRouter + NavigationGuard + RouteParamCodec — the client routing and navigation infrastructure                | provisional |
|   [4]   | service-worker.md       | ServiceWorkerHost + CacheStrategy + BackgroundSyncReplay — the PWA offline-first cache and SW lifecycle          | provisional |
|   [5]   | error-boundary.md       | CrashTelemetry + ErrorBoundaryBinding + CrashReport — the error-boundary and crash/exception telemetry sink      | provisional |
|   [6]   | feature-flags-config.md | RemoteConfig + FlagKey + FlagEvaluation — the feature-flag and remote-config read-side                          | provisional |
|   [7]   | web-vitals.md           | PerformanceBudget + VitalMetric + BudgetThreshold — the web-vitals / performance-budget observability surface    | provisional |

## [2]-[WIRE_PAGES]

`platform` authors no .NET wire shape; it composes the wire surface the neutral domains expose and consumes every contract as settled vocabulary. The wire-relevant consumer surface:

- host-runtime.md: the runtime feed is read through `projection`; `AuthSession.tokenHeader` is the per-call producer the `interchange` `WireTransport` interceptor reads — the assembled `Bearer ...` string crosses the seam, the `Redacted` value never does, and an expired-or-rejected token folds to the `interchange` `FaultDetail` re-auth fault.
- service-worker.md: `BackgroundSyncReplay` drains the `platform-substrate.md` `LocalPersistence.offlineQueue` of `CommandPayloadWire` into the `interchange` `CommandGateway` on redial — the same intra-package seam `platform-substrate.md` names, never a parallel queue.
- error-boundary.md: uncaught `window.onerror` / `unhandledrejection` faults reconstruct as an `interchange` `FaultDetail` `HopFault`/`ConfigError` arm, never a parallel error type; the sanitized `CrashReport` ships through `SelfTelemetry` to the collector.
- feature-flags-config.md: `FlagKey` references the `services` `persistence#WORK_AND_SIGNALS` `FeatureFlags` bucket/variant `Schema.Literal` vocabulary as settled — the bucket/variant axis is declared once in `services` and never re-authored; the `FlagSet` decode shape is the only platform-local Schema.

## [3]-[CATALOGUE_PENDING]

- `vite-plugin-pwa` + `workbox-build` + `workbox-window`: catalogued (browser stratum); `service-worker.md` `ServiceWorkerHost` composes them — `vite-plugin-pwa`/`workbox-build` at the `BuildPipeline` emit edge, `workbox-window` at the registration/update-available runtime lifecycle.
- `nuqs`: catalogued (browser stratum); `routing-navigation.md` `RouteParamCodec` composes it for query-state round-trip over the `history` `SubscriptionRef`.
- `react-error-boundary`: catalogued (browser stratum); `error-boundary.md` `ErrorBoundaryBinding` integrates it as the one Effect-native fault sink emitting to `CrashTelemetry`.

## [4]-[GAP_LEDGER]

| [INDEX] | [GAP]                                                                | [CLOSED_BY (page#cluster)]                                                                |
| :-----: | :------------------------------------------------------------------- | :---------------------------------------------------------------------------------------- |
|   [1]   | a sixth app-service landing beside the closed five                   | host-runtime#HOST_RUNTIME (AuthSession a platform-bound host owner, not a sixth service)   |
|   [2]   | a scattered `import.meta.env` flag read past the config boundary     | host-runtime#HOST_RUNTIME (one RuntimeConfig schema + one ConfigProvider layer)            |
|   [3]   | an inline `Metric.counter` / a second telemetry path                 | platform-substrate#PLATFORM_SUBSTRATE (MetricRegistry the one instrument owner, collector the only path) |
|   [4]   | a hand-rolled `localStorage` blob outside the Schema-encoded store   | platform-substrate#PLATFORM_SUBSTRATE (LocalPersistence over the KV abstraction)           |
|   [5]   | a router package admitted beside the hand-held history SubscriptionRef | routing-navigation#ROUTING_NAVIGATION (AppRouter over Schema.Literal + nuqs, zero router admitted) |
|   [6]   | a second offline command queue parallel to LocalPersistence.offlineQueue | service-worker#SERVICE_WORKER (BackgroundSyncReplay drains the one LocalPersistence queue) |
|   [7]   | BuildPipeline and ServiceWorkerHost splitting the PWA worker concern | service-worker#SERVICE_WORKER (BuildPipeline emits the asset, ServiceWorkerHost owns its lifecycle) |
|   [8]   | a parallel uncaught-error type beside the interchange FaultDetail family | error-boundary#ERROR_BOUNDARY (CrashTelemetry reconstructs uncaught faults as FaultDetail) |
|   [9]   | RemoteConfig re-authoring the services flag bucket/variant vocabulary | feature-flags-config#FEATURE_FLAGS_CONFIG (FlagKey references the services FeatureFlags axis as settled) |
|  [10]   | a parallel web-vitals metric construction outside MetricRegistry     | web-vitals#WEB_VITALS (PerformanceBudget feeds the existing Core-Web-Vitals instrument rows) |
|  [11]   | a web-vitals package admitted beside the native PerformanceObserver  | web-vitals#WEB_VITALS (PerformanceBudget captures via native PerformanceObserver, zero package admitted) |

## [5]-[DENSITY_BAR]

A new feature is a row or case, never a new surface. The owner-count budget folds every extension, layer wiring, and mapping descriptor under its axis owner. The five new infrastructure owners (`AppRouter`, `ServiceWorkerHost`, `CrashTelemetry`, `RemoteConfig`, `PerformanceBudget`) are platform-bound HOST owners exactly as `AuthSession`/`BrowserPlatform`/`SelfTelemetry` are — never app-services; the closed five-app-service budget (`WireClients`/`CommandGateway` in `interchange`, `SnapshotFeed`/`RuntimeFeed`/`EvidenceFeed` in `projection`) is unchanged and none of these owners enters it.

The `[STATE]` column carries `FINALIZED` where the owner is a transcription-complete fence with no open gate and `SPIKE` where the owner is fence-complete but its proof carries a residual native, browser-runtime, or live-server probe named in the page RESEARCH cluster; a SPIKE owner is fully shaped now, never a deferred surface.

| [INDEX] | [AXIS/CONCERN]               | [OWNER]                | [KIND]                       | [CASES]                                                                                          | [STATE]   |
| :-----: | :--------------------------- | :--------------------- | :--------------------------- | :----------------------------------------------------------------------------------------------- | :-------- |
|   [1]   | composition root             | `CompositionRoot`      | Effect.Service + Layer graph | one Layer graph + one runtime over the closed five app-services [platform-bound host owner]       | FINALIZED |
|   [2]   | browser platform             | `BrowserPlatform`      | Layer wiring                 | HTTP client + KeyValueStore + worker pool platform bindings [platform-bound host owner]            | FINALIZED |
|   [3]   | auth session                 | `AuthSession`          | Effect.Service               | OIDC PKCE + SubscriptionRef session fold + silent-refresh Schedule + tokenHeader [host owner]      | FINALIZED |
|   [4]   | runtime config               | `RuntimeConfig`        | Config schema + provider     | one Config schema + one ConfigProvider layer [platform-bound host owner]                          | FINALIZED |
|   [5]   | self telemetry               | `SelfTelemetry`/`MetricRegistry` | WebSdk Layer + instrument set | OTLP web export + closed instrument/span vocabulary incl. Core-Web-Vitals rows [host owner]   | FINALIZED |
|   [6]   | build + offload pipeline      | `BuildPipeline`/`DecodeWorkerPool` | vite plugin set + Worker.makePool | bundle + SW-asset emit + transferable-buffer decode pool [host owner]                        | FINALIZED |
|   [7]   | local persistence            | `LocalPersistence`     | KeyValueStore over idb-keyval | last-good snapshot + offline command queue [platform-bound host owner]                            | FINALIZED |
|   [8]   | client routing               | `AppRouter`            | Effect.Service over Schema.Literal route axis | route-key axis + history SubscriptionRef + push/replace/back transition [host owner]   | FINALIZED |
|   [9]   | route guard                  | `NavigationGuard`      | guard fold                   | gates a route on AuthSession.status + projection AvailabilityStore [host owner]                    | FINALIZED |
|  [10]   | route-param codec            | `RouteParamCodec`      | Schema round-trip over nuqs  | query-state + path-segment round-trip [host owner]                                                | FINALIZED |
|  [11]   | service-worker lifecycle      | `ServiceWorkerHost`    | Effect.Service over registration lifecycle | install/activate/skipWaiting + update-available SubscriptionRef [host owner]        | SPIKE     |
|  [12]   | cache strategy               | `CacheStrategy`        | Schema.Literal route-strategy axis | cache-first / network-first / stale-while-revalidate [host owner]                            | FINALIZED |
|  [13]   | background-sync replay        | `BackgroundSyncReplay` | offline-queue drain fold     | drains LocalPersistence.offlineQueue into interchange CommandGateway on redial [host owner]         | FINALIZED |
|  [14]   | crash telemetry              | `CrashTelemetry`       | Effect.Service over global error capture | window.onerror + unhandledrejection -> FaultDetail + crash fold [host owner]            | SPIKE     |
|  [15]   | error-boundary binding        | `ErrorBoundaryBinding` | react-error-boundary integration | the one Effect-native fault sink emitting to CrashTelemetry [host owner]                       | FINALIZED |
|  [16]   | crash report                 | `CrashReport`          | sanitized envelope + breadcrumb ring | sanitized exception envelope shipped via SelfTelemetry [host owner]                          | FINALIZED |
|  [17]   | remote config                | `RemoteConfig`         | Effect.Service over FlagSet fold | RuntimeConfig-fed decode-once + poll/refresh Schedule [host owner]                            | FINALIZED |
|  [18]   | flag key                     | `FlagKey`              | Schema.Literal flag axis     | references the services FeatureFlags bucket/variant vocabulary as settled [host owner]             | FINALIZED |
|  [19]   | flag evaluation              | `FlagEvaluation`       | Match total dispatch         | total Match over the services bucket/variant resolution [host owner]                              | FINALIZED |
|  [20]   | performance budget            | `PerformanceBudget`    | Effect.Service over web-vitals capture | PerformanceObserver capture + budget-exceeded fold + breach span [host owner]              | SPIKE     |
|  [21]   | vital metric                 | `VitalMetric`          | Schema.Literal vital axis     | LCP/INP/CLS/TTFB/FCP feeding the existing MetricRegistry Core-Web-Vitals rows [host owner]         | FINALIZED |
|  [22]   | budget threshold             | `BudgetThreshold`      | data-driven Record           | threshold table gating the budget-exceeded fold [host owner]                                      | FINALIZED |

## [6]-[BUILD_ORDER]

`platform-substrate.ts` precedes the SPA entry (the substrate owners are platform-layer requirements); `host-runtime.ts` composes the substrate plus the routing/SW/crash/config/vitals owners; `browser.ts` is the `./web` entry that composes `ui` + `interchange` + `projection` into the one runtime and lands last. The five new infra modules land between the substrate and the entry because each reads `RuntimeConfig`, `MetricRegistry`, `LocalPersistence`, `AuthSession`, or `SelfTelemetry` from the substrate and host-runtime owners.

| [INDEX] | [FILE]                          | [TRANSCRIBES]                          | [GATE]                                          |
| :-----: | :------------------------------ | :------------------------------------- | :---------------------------------------------- |
|   [1]   | platform/platform-substrate.ts  | platform-substrate#PLATFORM_SUBSTRATE  | tsgo + isolatedDeclarations emit                |
|   [2]   | platform/host-runtime.ts        | host-runtime#HOST_RUNTIME              | tsgo + Layer graph resolves the five app-services |
|   [3]   | platform/routing-navigation.ts  | routing-navigation#ROUTING_NAVIGATION  | tsgo + history SubscriptionRef + zero router import |
|   [4]   | platform/service-worker.ts      | service-worker#SERVICE_WORKER          | tsgo + SW registration + offline-queue redial drain |
|   [5]   | platform/error-boundary.ts      | error-boundary#ERROR_BOUNDARY          | tsgo + uncaught-fault -> FaultDetail reconstruction |
|   [6]   | platform/feature-flags-config.ts | feature-flags-config#FEATURE_FLAGS_CONFIG | tsgo + FlagKey references services FeatureFlags  |
|   [7]   | platform/web-vitals.ts          | web-vitals#WEB_VITALS                  | tsgo + native PerformanceObserver capture        |
|   [8]   | platform/browser.ts             | the `./web` SPA entry composing ui + interchange + projection | bundle builds; `./web` export resolves |

## [7]-[PROOF_GATES]

| [GATE]            | [COMMAND]                                | [EVIDENCE]                                                       |
| :---------------- | :--------------------------------------- | :--------------------------------------------------------------- |
| catalog resolve   | `pnpm install`                           | catalogMode strict resolves the browser-stratum catalog; no router/web-vitals package present |
| typecheck         | tsgo `--noEmit` over the domain          | zero diagnostics; isolatedDeclarations emits `.d.ts`             |
| import stratum    | centralized config (nx + root eslint)    | `platform/**` imports only browser + neutral; `ui/**` -> `platform/**` rejected |
| browser-e2e       | vitest browser-mode (playwright) project | routing transition + SW registration + crash-boundary + flag-eval DOM scenarios pass |
| sw lifecycle spike | live-browser SW probe                   | install/activate/skipWaiting + offline-queue redial drain prove (SPIKE) |
| crash capture spike | live-browser error probe               | window.onerror + unhandledrejection marshal into FaultDetail (SPIKE) |
| vitals spike      | live-browser PerformanceObserver probe   | LCP/INP/CLS/TTFB/FCP attribution feeds MetricRegistry rows (SPIKE) |

## [8]-[PROHIBITIONS]

- NEVER a sixth app-service beside the closed five; `AuthSession`, `AppRouter`, `ServiceWorkerHost`, `CrashTelemetry`, `RemoteConfig`, and `PerformanceBudget` are platform-bound host owners, never app-services.
- NEVER a `ui/**` import of `platform/**`; `ui` is the lower AppUi-analog library and `platform` the AppHost-analog entry above it — a `ui` -> `platform` import is the named browser-internal coupling defect; `platform` -> `ui` is the allowed composition direction.
- NEVER a router package (`react-router`/`tanstack-router`/`history`); `AppRouter` is `Effect` `Schema.Literal` + `nuqs` (catalogued) + a hand-held `history` `SubscriptionRef`.
- NEVER a `web-vitals` package; `PerformanceBudget` captures via the native `PerformanceObserver` API.
- NEVER a scattered `import.meta.env` read past `RuntimeConfig`; a direct env read in any owner is the named defect.
- NEVER a second telemetry path beside the collector; `CrashReport` and `PerformanceBudget` breach spans ship through `SelfTelemetry`, and `MetricRegistry` is the single instrument owner — an inline `Metric.counter` is the deleted form.
- NEVER a parallel uncaught-error type; uncaught faults reconstruct as an `interchange` `FaultDetail` arm.
- NEVER a second offline command queue; `BackgroundSyncReplay` drains the one `LocalPersistence.offlineQueue`.
- NEVER a re-authored flag bucket/variant vocabulary; `RemoteConfig` references the `services` `FeatureFlags` axis as settled and authors only the `FlagSet` decode shape.
- NEVER a two-owner split of the PWA worker concern; `BuildPipeline` emits the asset, `ServiceWorkerHost` owns its runtime lifecycle.
- NEVER a `localStorage`/`IndexedDB`/`idb-keyval` access outside `LocalPersistence`; NEVER a `@rasm/<domain>` import idiom — intra-package modules import by relative path or the single `@rasm/ts` package name, never a `workspace:*` protocol.
- NEVER a comment carrying task or process narration.

## [9]-[ADMISSIONS_RECORD]

`[CATALOGUE]` names the `.api` doctrine page; concrete coordinates live in the workspace catalog (`pnpm-workspace.yaml` `catalog:`), never in planning prose. The deliberate no-admission rows record the world-class infrastructure that was strengthened WITHOUT admitting a new package, per the strict catalog mode.

| [INDEX] | [PACKAGE]                                  | [PAGE]                  | [CATALOGUE]                | [STATUS]                |
| :-----: | :----------------------------------------- | :---------------------- | :------------------------- | :---------------------- |
|   [1]   | effect (core, v3-held)                     | every page              | api-effect                 | admitted                |
|   [2]   | @effect/platform-browser                   | host-runtime.md, platform-substrate.md | api-effect-platform | admitted             |
|   [3]   | @effect/opentelemetry + @opentelemetry/sdk-trace-web | platform-substrate.md | api-effect-opentelemetry | admitted           |
|   [4]   | idb-keyval                                 | platform-substrate.md   | api-effect-platform        | admitted                |
|   [5]   | arctic                                     | host-runtime.md         | api-auth-web               | admitted                |
|   [6]   | otplib + @simplewebauthn/server            | host-runtime.md         | api-auth-web               | admitted                |
|   [7]   | vite + plugin family                       | platform-substrate.md   | api-build-web              | admitted                |
|   [8]   | vite-plugin-pwa + workbox-build + workbox-window | service-worker.md  | api-build-web              | catalogue-pending       |
|   [9]   | nuqs                                       | routing-navigation.md   | api-build-web              | catalogue-pending       |
|  [10]   | react-error-boundary                       | error-boundary.md       | api-build-web              | catalogue-pending       |
|  [11]   | react-router / tanstack-router / history   | routing-navigation.md   | —                          | no-admission (deliberate) |
|  [12]   | web-vitals                                 | web-vitals.md           | —                          | no-admission (deliberate) |

## [10]-[REFINEMENT_HORIZON]

The next deepening drives deeper SPA infrastructure across the five new owners and re-derives the substrate against live-browser probes: the `ServiceWorkerHost` install/activate/skipWaiting lifecycle and the offline-first navigation fallback settle their member shapes off `workbox-window`, but the redial-drain convergence stays a live-browser residual until the offline-queue probe lands (service-worker#SERVICE_WORKER, the SPIKE row); the `CrashTelemetry` global capture settles `window.onerror`/`unhandledrejection` marshalling into `FaultDetail`, but the breadcrumb-ring + sanitized-envelope ship-through stays a live-browser residual (error-boundary#ERROR_BOUNDARY, the SPIKE row); the `PerformanceBudget` Core-Web-Vitals capture settles the native `PerformanceObserver` LCP/INP/CLS/TTFB/FCP attribution, but the budget-breach span feed into the existing `MetricRegistry` rows stays a live-browser residual (web-vitals#WEB_VITALS, the SPIKE row) — the single `web-vitals` catalog row activates only if native attribution is judged insufficient, default zero-package. The `AppRouter` history-SubscriptionRef + scroll-restoration cell, the `RemoteConfig` poll/refresh Schedule, and the cross-origin `AuthSession` bearer stamp are settled member shapes with no open residual. The bar: the SPA boots, routes, registers its service worker, caches offline-first, captures every crash into the typed fault family, reads every flag as settled vocabulary, and reports its web-vitals budget — all through this one host spine with zero app-side ceremony, the closed five-app-service budget unchanged.
