# [PLATFORM]

`platform` is the browser application infrastructure concern and the SPA browser entry of the TypeScript branch — the browser AppHost-analog. It owns the composition root, the host runtime, the platform substrate, and the flagship-SPA infrastructure a world-class browser application requires: client routing and navigation, the service-worker / PWA offline-first cache, the error-boundary and crash telemetry sink, the feature-flag read-side, and the web-vitals performance budget. Zero consumers exist; implementation is full-capability with no holding back; `.planning/` pages are transcribed, never re-designed. The `./web` entry is the one `CompositionRoot` that composes the `ui` library plus the neutral `interchange` and `projection` domains into one Layer graph and one runtime; `platform` imports `ui`, `ui` never imports `platform`. The infrastructure owners are platform-bound HOST owners, never app-services — the closed five-app-service budget is unchanged. Owner-state and the rails/axes registry live in `ARCHITECTURE.md`; the realized capability list in `FEATURES.md`; open work in `TASKLOG.md`.

## [1]-[PAGE_INDEX]

| [INDEX] | [PAGE]                                                          | [OWNS]                                                                            |
| :-----: | :----------------------------------------------------------- | :------------------------------------------------------------------------------- |
|   [1]   | [host-runtime](.planning/host-runtime.md)                   | CompositionRoot, BrowserPlatform, AuthSession, RuntimeConfig, the `./web` Layer graph |
|   [2]   | [platform-substrate](.planning/platform-substrate.md)       | SelfTelemetry, MetricRegistry, BuildPipeline, DecodeWorkerPool, LocalPersistence   |
|   [3]   | [routing-navigation](.planning/routing-navigation.md)       | AppRouter, NavigationGuard, RouteParamCodec                                        |
|   [4]   | [service-worker](.planning/service-worker.md)               | ServiceWorkerHost, CacheStrategy, BackgroundSyncReplay                             |
|   [5]   | [error-boundary](.planning/error-boundary.md)               | CrashTelemetry, ErrorBoundaryBinding, CrashReport                                  |
|   [6]   | [feature-flags-config](.planning/feature-flags-config.md)   | RemoteConfig, FlagKey, FlagEvaluation                                              |
|   [7]   | [web-vitals](.planning/web-vitals.md)                       | PerformanceBudget, VitalMetric, BudgetThreshold                                    |

## [2]-[ADMISSIONS_RECORD]

Each package maps to its consuming page, central catalogue at `libs/typescript/.api/`, and admission status. Concrete coordinates live in the workspace catalog (`pnpm-workspace.yaml` `catalog:`); this table never carries a pin. The no-admission rows record the world-class infrastructure strengthened WITHOUT admitting a package, per the strict catalog mode.

| [INDEX] | [PACKAGE]                                            | [PAGE]                              | [CATALOGUE]                        | [STATUS]                  |
| :-----: | :-------------------------------------------------- | :--------------------------------- | :--------------------------------- | :------------------------ |
|   [1]   | effect (core, v3-held)                              | every page                         | `.api/api-effect.md`               | admitted                  |
|   [2]   | @effect/platform-browser                            | host-runtime, platform-substrate   | `.api/api-effect-platform.md`      | admitted                  |
|   [3]   | @effect/opentelemetry + @opentelemetry/sdk-trace-web | platform-substrate               | `.api/api-effect-opentelemetry.md` | admitted                  |
|   [4]   | idb-keyval                                          | platform-substrate                 | `.api/api-effect-platform.md`      | admitted                  |
|   [5]   | arctic                                              | host-runtime                       | `.api/api-ui-stack.md`             | admitted                  |
|   [6]   | otplib + @simplewebauthn/server                     | host-runtime                       | `.api/api-ui-stack.md`             | admitted                  |
|   [7]   | vite + plugin family                                | platform-substrate                 | `.api/api-ui-stack.md`             | admitted                  |
|   [8]   | vite-plugin-pwa + workbox-build + workbox-window     | service-worker                     | `.api/api-ui-stack.md`             | catalogue-pending         |
|   [9]   | nuqs                                                | routing-navigation                 | `.api/api-ui-stack.md`             | catalogue-pending         |
|  [10]   | react-error-boundary                                | error-boundary                     | `.api/api-ui-stack.md`             | catalogue-pending         |
|  [11]   | react-router / tanstack-router / history            | routing-navigation                 | —                                  | no-admission (deliberate) |
|  [12]   | web-vitals                                          | web-vitals                         | —                                  | no-admission (deliberate) |

## [3]-[PROOF_GATES]

`[RAIL]` names the owning rail; the executable command lives with that rail owner, never restated here.

| [INDEX] | [GATE]              | [RAIL]                            | [EVIDENCE]                                                       |
| :-----: | :------------------ | :-------------------------------- | :-------------------------------------------------------------- |
|  [G1]   | catalog resolve     | `pnpm` install/restore            | `catalogMode` strict resolves the browser-stratum catalog; no router/web-vitals package present |
|  [G2]   | typecheck           | `tsgo` typecheck                  | zero diagnostics; isolatedDeclarations emits `.d.ts`            |
|  [G3]   | import stratum      | centralized lint config            | `platform/**` imports only browser + neutral; `ui`->`platform` rejected |
|  [G4]   | browser-e2e         | `vitest` browser-mode (playwright) | routing transition + SW registration + crash-boundary + flag-eval DOM scenarios pass |
|  [G5]   | sw lifecycle spike  | live-browser SW probe              | install/activate/skipWaiting + offline-queue redial drain prove |
|  [G6]   | crash capture spike | live-browser error probe           | global error capture marshals into the typed fault family       |
|  [G7]   | vitals spike        | live-browser PerformanceObserver probe | LCP/INP/CLS/TTFB/FCP attribution feeds the MetricRegistry rows |
|  [G8]   | page render         | local mermaid-cli                  | page diagrams render through the local renderer                  |
