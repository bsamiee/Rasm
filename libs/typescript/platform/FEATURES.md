# [PLATFORM_FEATURES]

The realized capability list for the browser host. Every feature is a row or case on a budgeted owner, never a new surface; mechanics live at the `.planning/` page#cluster anchor named on each row, and the owner's realization state is read from `ARCHITECTURE.md` `[OWNER_REGISTRY]`.

## [1]-[HOST_AND_SUBSTRATE]

| [INDEX] | [FEATURE]                                                                       | [PAGE#CLUSTER]                        |
| :-----: | :----------------------------------------------------------------------------- | :------------------------------------ |
|   [1]   | One CompositionRoot composing the closed five app-services into one runtime       | host-runtime#HOST_RUNTIME             |
|   [2]   | Browser platform Layer: HTTP client, KeyValueStore, worker pool bindings          | host-runtime#HOST_RUNTIME             |
|   [3]   | OIDC PKCE auth session with silent-refresh Schedule and per-call token header     | host-runtime#HOST_RUNTIME             |
|   [4]   | One RuntimeConfig schema and ConfigProvider layer as the single env boundary       | host-runtime#HOST_RUNTIME             |
|   [5]   | Self-telemetry WebSdk export with one closed instrument/span vocabulary            | platform-substrate#PLATFORM_SUBSTRATE |
|   [6]   | Build + offload pipeline: bundle, SW-asset emit, transferable-buffer decode pool    | platform-substrate#PLATFORM_SUBSTRATE |
|   [7]   | Local persistence over a KeyValueStore: last-good snapshot + offline command queue  | platform-substrate#PLATFORM_SUBSTRATE |

## [2]-[ROUTING_AND_WORKER]

| [INDEX] | [FEATURE]                                                                       | [PAGE#CLUSTER]                        |
| :-----: | :----------------------------------------------------------------------------- | :------------------------------------ |
|   [8]   | Client routing over a Schema.Literal route axis with a hand-held history ref     | routing-navigation#ROUTING_NAVIGATION |
|   [9]   | Navigation guard gating a route on auth status + the availability store           | routing-navigation#ROUTING_NAVIGATION |
|  [10]   | Route-param codec round-tripping query-state and path segments over nuqs           | routing-navigation#ROUTING_NAVIGATION |
|  [11]   | Service-worker lifecycle: install/activate/skipWaiting + update-available ref       | service-worker#SERVICE_WORKER         |
|  [12]   | Cache strategy axis: cache-first / network-first / stale-while-revalidate           | service-worker#SERVICE_WORKER         |
|  [13]   | Background-sync replay draining the one offline queue into the command gateway       | service-worker#SERVICE_WORKER         |

## [3]-[CRASH_FLAGS_VITALS]

| [INDEX] | [FEATURE]                                                                       | [PAGE#CLUSTER]                            |
| :-----: | :----------------------------------------------------------------------------- | :--------------------------------------- |
|  [14]   | Crash telemetry: global error capture marshalled into the typed fault family      | error-boundary#ERROR_BOUNDARY            |
|  [15]   | Error-boundary binding as the one Effect-native fault sink                         | error-boundary#ERROR_BOUNDARY            |
|  [16]   | Sanitized crash report with breadcrumb ring shipped via self-telemetry             | error-boundary#ERROR_BOUNDARY            |
|  [17]   | Remote config read-side: decode-once with poll/refresh Schedule                    | feature-flags-config#FEATURE_FLAGS_CONFIG |
|  [18]   | Flag key axis referencing the services FeatureFlags vocabulary as settled           | feature-flags-config#FEATURE_FLAGS_CONFIG |
|  [19]   | Flag evaluation as a total Match over the bucket/variant resolution                 | feature-flags-config#FEATURE_FLAGS_CONFIG |
|  [20]   | Performance budget: native PerformanceObserver capture + budget-exceeded fold        | web-vitals#WEB_VITALS                    |
|  [21]   | Vital metric axis feeding the existing MetricRegistry Core-Web-Vitals rows            | web-vitals#WEB_VITALS                    |
|  [22]   | Budget threshold Record gating the budget-exceeded fold                              | web-vitals#WEB_VITALS                    |
