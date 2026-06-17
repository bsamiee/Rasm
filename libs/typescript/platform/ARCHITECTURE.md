# [PLATFORM_ARCHITECTURE]

`platform` is the browser host as one folder: one `CompositionRoot` composes the closed five app-services into one Layer graph and one runtime, and every infrastructure concern is one platform-bound host owner — routing, the service worker, crash telemetry, remote config, and the web-vitals budget — never a sixth app-service. Mechanics live in the finalized `.planning/` pages; this page is the atlas — the source tree and build order, the owner registry (the one owner-state surface), dependency direction, cross-folder seams, boundaries, and prohibitions.

## [1]-[SOURCE_TREE]

The flat module layout IS the build order: the substrate owners are platform-layer requirements, host-runtime composes the substrate plus the infrastructure owners, the five infra modules land between the substrate and the entry because each reads a substrate or host owner, and `browser.ts` is the `./web` entry that lands last. Each leaf is one transcription unit annotated with the owners it transcribes and the owning page#cluster.

```text codemap
platform/
├── platform-substrate.ts           # SelfTelemetry, MetricRegistry, BuildPipeline, DecodeWorkerPool, LocalPersistence — platform-substrate#PLATFORM_SUBSTRATE
├── host-runtime.ts                 # CompositionRoot, BrowserPlatform, AuthSession, RuntimeConfig — host-runtime#HOST_RUNTIME
├── routing-navigation.ts           # AppRouter, NavigationGuard, RouteParamCodec — routing-navigation#ROUTING_NAVIGATION
├── service-worker.ts               # ServiceWorkerHost, CacheStrategy, BackgroundSyncReplay — service-worker#SERVICE_WORKER
├── error-boundary.ts               # CrashTelemetry, ErrorBoundaryBinding, CrashReport — error-boundary#ERROR_BOUNDARY
├── feature-flags-config.ts         # RemoteConfig, FlagKey, FlagEvaluation — feature-flags-config#FEATURE_FLAGS_CONFIG
├── web-vitals.ts                   # PerformanceBudget, VitalMetric, BudgetThreshold — web-vitals#WEB_VITALS
└── browser.ts                      # the `./web` SPA entry composing ui + interchange + projection into one runtime — host-runtime#HOST_RUNTIME
```

`platform-substrate.ts` precedes the entry because the substrate owners (`MetricRegistry`, `LocalPersistence`, `SelfTelemetry`, `DecodeWorkerPool`) are platform-layer requirements. `host-runtime.ts` composes the substrate plus `AuthSession`/`RuntimeConfig`. The five infra modules land between the substrate and the entry because each reads `RuntimeConfig`, `MetricRegistry`, `LocalPersistence`, `AuthSession`, or `SelfTelemetry`. `browser.ts` lands last: its `CompositionRoot` composes `ui` + `interchange` + `projection` into the one Layer graph and the one runtime over the `./web` subpath.

## [2]-[OWNER_REGISTRY]

The single owner-state surface for the folder. A new feature is a row or case, never a new surface; the five infrastructure owners are platform-bound host owners, never app-services, and the closed five-app-service budget is unchanged. `[STATE]` is `FINALIZED` where the owner is a transcription-complete fence with no open gate, `SPIKE` where the owner is fence-complete but its proof carries a residual native, browser-runtime, or live-server probe named in the page RESEARCH cluster. This is the ONLY place owner state lives.

| [INDEX] | [AXIS/RAIL]              | [OWNER]                            | [KIND]                                  | [CASES]                                                                          | [PAGE#CLUSTER]                          |  [STATE]  |
| :-----: | :---------------------- | :--------------------------------- | :-------------------------------------- | :------------------------------------------------------------------------------ | :-------------------------------------- | :-------: |
|   [1]   | composition root        | `CompositionRoot`                  | Effect.Service + Layer graph            | one Layer graph + one runtime over the closed five app-services                   | host-runtime#HOST_RUNTIME               | FINALIZED |
|   [2]   | browser platform        | `BrowserPlatform`                  | Layer wiring                            | HTTP client + KeyValueStore + worker pool platform bindings                       | host-runtime#HOST_RUNTIME               | FINALIZED |
|   [3]   | auth session            | `AuthSession`                      | Effect.Service                          | OIDC PKCE + SubscriptionRef session fold + silent-refresh Schedule + tokenHeader  | host-runtime#HOST_RUNTIME               | FINALIZED |
|   [4]   | runtime config          | `RuntimeConfig`                    | Config schema + provider                | one Config schema + one ConfigProvider layer                                      | host-runtime#HOST_RUNTIME               | FINALIZED |
|   [5]   | self telemetry          | `SelfTelemetry`/`MetricRegistry`   | WebSdk Layer + instrument set           | OTLP web export + closed instrument/span vocabulary incl. Core-Web-Vitals rows    | platform-substrate#PLATFORM_SUBSTRATE   | FINALIZED |
|   [6]   | build + offload pipeline | `BuildPipeline`/`DecodeWorkerPool` | vite plugin set + Worker.makePool       | bundle + SW-asset emit + transferable-buffer decode pool                          | platform-substrate#PLATFORM_SUBSTRATE   | FINALIZED |
|   [7]   | local persistence       | `LocalPersistence`                 | KeyValueStore over idb-keyval           | last-good snapshot + offline command queue                                        | platform-substrate#PLATFORM_SUBSTRATE   | FINALIZED |
|   [8]   | client routing          | `AppRouter`                        | Effect.Service over Schema.Literal route axis | route-key axis + history SubscriptionRef + push/replace/back transition       | routing-navigation#ROUTING_NAVIGATION   | FINALIZED |
|   [9]   | route guard             | `NavigationGuard`                  | guard fold                              | gates a route on AuthSession.status + the projection availability store           | routing-navigation#ROUTING_NAVIGATION   | FINALIZED |
|  [10]   | route-param codec       | `RouteParamCodec`                  | Schema round-trip over nuqs             | query-state + path-segment round-trip                                             | routing-navigation#ROUTING_NAVIGATION   | FINALIZED |
|  [11]   | service-worker lifecycle | `ServiceWorkerHost`               | Effect.Service over registration lifecycle | install/activate/skipWaiting + update-available SubscriptionRef                | service-worker#SERVICE_WORKER           | SPIKE     |
|  [12]   | cache strategy          | `CacheStrategy`                    | Schema.Literal route-strategy axis      | cache-first / network-first / stale-while-revalidate                              | service-worker#SERVICE_WORKER           | FINALIZED |
|  [13]   | background-sync replay   | `BackgroundSyncReplay`            | offline-queue drain fold                | drains LocalPersistence.offlineQueue into the interchange CommandGateway on redial | service-worker#SERVICE_WORKER           | FINALIZED |
|  [14]   | crash telemetry         | `CrashTelemetry`                   | Effect.Service over global error capture | global error capture marshalled into the typed fault family + crash fold          | error-boundary#ERROR_BOUNDARY           | SPIKE     |
|  [15]   | error-boundary binding   | `ErrorBoundaryBinding`            | react-error-boundary integration        | the one Effect-native fault sink emitting to CrashTelemetry                        | error-boundary#ERROR_BOUNDARY           | FINALIZED |
|  [16]   | crash report            | `CrashReport`                      | sanitized envelope + breadcrumb ring    | sanitized exception envelope shipped via SelfTelemetry                             | error-boundary#ERROR_BOUNDARY           | FINALIZED |
|  [17]   | remote config           | `RemoteConfig`                     | Effect.Service over FlagSet fold        | RuntimeConfig-fed decode-once + poll/refresh Schedule                              | feature-flags-config#FEATURE_FLAGS_CONFIG | FINALIZED |
|  [18]   | flag key                | `FlagKey`                          | Schema.Literal flag axis                | references the services FeatureFlags bucket/variant vocabulary as settled          | feature-flags-config#FEATURE_FLAGS_CONFIG | FINALIZED |
|  [19]   | flag evaluation         | `FlagEvaluation`                   | Match total dispatch                    | total Match over the services bucket/variant resolution                           | feature-flags-config#FEATURE_FLAGS_CONFIG | FINALIZED |
|  [20]   | performance budget      | `PerformanceBudget`                | Effect.Service over web-vitals capture  | PerformanceObserver capture + budget-exceeded fold + breach span                  | web-vitals#WEB_VITALS                   | SPIKE     |
|  [21]   | vital metric            | `VitalMetric`                      | Schema.Literal vital axis               | LCP/INP/CLS/TTFB/FCP feeding the existing MetricRegistry Core-Web-Vitals rows      | web-vitals#WEB_VITALS                   | FINALIZED |
|  [22]   | budget threshold        | `BudgetThreshold`                  | data-driven Record                      | threshold table gating the budget-exceeded fold                                   | web-vitals#WEB_VITALS                   | FINALIZED |

`ServiceWorkerHost`, `CrashTelemetry`, and `PerformanceBudget` are SPIKE pending their live-browser probes (offline-queue redial drain, global crash marshalling ship-through, Core-Web-Vitals attribution feed); each is fully shaped now, not a deferred surface.

## [3]-[DEPENDENCY_DIRECTION]

| [INDEX] | [FOLDER]      | [MAY_REFERENCE_PLATFORM] | [PLATFORM_MAY_REFERENCE] | [BOUNDARY]                                          |
| :-----: | :------------ | :----------------------: | :----------------------: | :------------------------------------------------- |
|   [1]   | `ui`          |            no            |           yes            | the CompositionRoot composes the UI library         |
|   [2]   | `interchange` |            no            |           yes            | composes the transport + gateway at the entry        |
|   [3]   | `projection`  |            no            |           yes            | the runtime feed is read through projection           |
|   [4]   | `services`    |            no            |            no            | node tier is out of the browser stratum              |

`platform` is the browser AppHost-analog entry above `ui`: it imports `ui` + `interchange` + `projection`; `ui` never imports `platform`. The intra-browser direction (`platform`->`ui` allowed, `ui`->`platform` forbidden) mirrors the inward neutral direction.

## [4]-[SEAMS]

Every two-folder fact splits by altitude: mechanics live at the named owner cluster, consequences land at the consumer. Intra-TypeScript seams ride `pkg/page#CLUSTER`; the wire contracts the host consumes route through the Tier-0 seam ledger.

| [INDEX] | [SEAM]               | [MECHANICS_AT]                                | [CONSEQUENCE_AT]                                                       |
| :-----: | :------------------- | :-------------------------------------------- | :-------------------------------------------------------------------- |
|   [1]   | token stamp          | host-runtime#HOST_RUNTIME                      | interchange/transport#TRANSPORT_AND_CLIENTS reads `AuthSession.tokenHeader` per call |
|   [2]   | runtime feed         | projection/fold-algebra#FOLD_ALGEBRA           | host-runtime#HOST_RUNTIME reads the `RuntimeFeed`                       |
|   [3]   | offline-queue drain  | platform-substrate#PLATFORM_SUBSTRATE          | service-worker#SERVICE_WORKER drains the one `LocalPersistence.offlineQueue` into the interchange CommandGateway |
|   [4]   | crash reconstruction | error-boundary#ERROR_BOUNDARY                  | interchange/codec-rails#FAULT_FAMILY reconstructs uncaught faults as a fault arm |
|   [5]   | flag vocabulary      | services/persistence#WORK_AND_SIGNALS          | feature-flags-config#FEATURE_FLAGS_CONFIG references the FeatureFlags bucket/variant axis as settled |
|   [6]   | offline cell         | platform-substrate#PLATFORM_SUBSTRATE          | ui/binding#BINDING `OfflineState` reads `LocalPersistence`              |
|   [7]   | wire contract source | the Tier-0 seam ledger                         | the host composes the wire surface the neutral domains expose          |

## [5]-[BOUNDARIES]

- `platform` is the browser AppHost-analog: it owns runtime composition and host policy, never domain state, decode, or UI components.
- The five infrastructure owners (`AppRouter`, `ServiceWorkerHost`, `CrashTelemetry`, `RemoteConfig`, `PerformanceBudget`) are platform-bound host owners exactly as `AuthSession`/`BrowserPlatform`/`SelfTelemetry`; none enters the closed five-app-service budget.
- `RuntimeConfig` is the single env boundary; a scattered `import.meta.env` read past it is the named defect.
- `MetricRegistry` is the single instrument owner and `SelfTelemetry` the single collector path; `CrashReport` and `PerformanceBudget` breach spans ship through it.
- Uncaught faults reconstruct as a typed fault arm; the flag bucket/variant vocabulary is referenced as settled, never re-authored.
- `localStorage`/`IndexedDB`/`idb-keyval` access stays inside `LocalPersistence`; the PWA worker concern splits at the build edge (`BuildPipeline` emits the asset, `ServiceWorkerHost` owns its lifecycle).

## [6]-[PROHIBITIONS]

The closed NEVER list — the deleted patterns the owner registry forecloses.

- NEVER a sixth app-service beside the closed five; the infrastructure owners are platform-bound host owners.
- NEVER a `ui/**` import of `platform/**`; a `ui`->`platform` import is the named browser-internal coupling defect.
- NEVER a router package (`react-router`/`tanstack-router`/`history`); `AppRouter` is Schema.Literal + nuqs + a hand-held history `SubscriptionRef`.
- NEVER a `web-vitals` package; `PerformanceBudget` captures via the native `PerformanceObserver` API.
- NEVER a scattered `import.meta.env` read past `RuntimeConfig`.
- NEVER a second telemetry path beside the collector or an inline `Metric.counter` beside `MetricRegistry`.
- NEVER a parallel uncaught-error type; uncaught faults reconstruct as a typed fault arm.
- NEVER a second offline command queue; `BackgroundSyncReplay` drains the one `LocalPersistence.offlineQueue`.
- NEVER a re-authored flag bucket/variant vocabulary; `RemoteConfig` references the FeatureFlags axis as settled and authors only the `FlagSet` decode shape.
- NEVER a two-owner split of the PWA worker concern; NEVER a `localStorage`/`IndexedDB`/`idb-keyval` access outside `LocalPersistence`.
- NEVER a comment carrying task or process narration.
