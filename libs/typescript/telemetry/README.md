# [TELEMETRY]

`telemetry` is the W1 four-signal observability plane — traces, metrics, logs, and the vital/crash/audit/meter fact families — serving hundreds of apps through one parameterization: every surface derives from the kernel `AppIdentity` value, the same identity `browser` boot and the `store` `StoreHandle` scope use, so the OTel Resource, the meter keys, and the dashboard identity are one value, never per-app configuration. The `otlp` sub-domain owns OTLP egress (the `@effect/opentelemetry` Otlp exporter family with NodeSdk/WebSdk rows, egress redaction policy rows at the export boundary, the `plane:dev`-fenced `./dev` DevTools row) and W3C composite trace-context extract-and-continue at every ingress. The `signal` sub-domain owns the semantic-convention vocabulary as data rows, browser RUM over native `PerformanceObserver` budgets, crash capture reconstructing `FaultDetail` through the kernel-declared fault-enricher contract (telemetry never imports `wire`), and the two durable fact streams: the audit stream (actor/action/target vocabulary, typed diff evidence, retention classes) and the usage/cost meter stream ((app, tenant)-keyed request/compute/storage/token counters with rating policy rows) — each durable through a journal port Tag the app root satisfies with `store` journal Layers, the meter stream the billing and cost-attribution source every multi-tenant archetype rolls up. The `slo` sub-domain owns SLO as algebra — multi-window multi-burn-rate typed policy rows and the alert emission vocabulary. The `board` sub-domain exports TOTAL FUNCTIONS `AppIdentity -> DashboardModel`, so a per-app dashboard fork is structurally impossible: dashboards are data derived from identity, never files. The folder imports `kernel` and `host` only and publishes per-runtime subpaths (`./server`, `./browser`) plus the dev-only `./dev` row; a new signal convention is a vocabulary row, a new SLO shape a policy row, a new metered resource a counter row, a new dashboard family a library function — never a file. The domain map and seam record live in `ARCHITECTURE.md`, the forward pool in `IDEAS.md`, the work log in `TASKLOG.md`.

## [1]-[ROUTER]

- [01]-[EXPORT](.planning/otlp/export.md): the Otlp trace/metric/log exporter rows (NodeSdk/WebSdk), the egress redaction policy rows at the export boundary, and the `plane:dev`-fenced `./dev` DevTools row.
- [02]-[CONTEXT](.planning/otlp/context.md): W3C composite trace-context extract-and-continue at every ingress — edge middleware, browser boot, work entities.
- [03]-[CONVENTION](.planning/signal/convention.md): the semantic-convention vocabulary rows — signal names as data, never string literals.
- [04]-[VITAL](.planning/signal/vital.md): browser RUM over native `PerformanceObserver` budgets.
- [05]-[CRASH](.planning/signal/crash.md): crash capture reconstructing `FaultDetail` through the kernel fault-enricher contract, with session-replay redaction-at-capture.
- [06]-[AUDIT](.planning/signal/audit.md): the audit fact stream — actor/action/target vocabulary, typed diff evidence, retention classes — durable through the audit journal port.
- [07]-[METER](.planning/signal/meter.md): the usage/cost metering fact stream — (app, tenant)-keyed request/compute/storage/token counters with rating policy rows — durable through the meter journal port.
- [08]-[BURNRATE](.planning/slo/burnrate.md): the multi-window multi-burn-rate typed SLO policy rows.
- [09]-[ALERT](.planning/slo/alert.md): the alert emission vocabulary feeding `board` and `iac/observe`.
- [10]-[MODEL](.planning/board/model.md): the `DashboardModel` algebra — `AppIdentity -> DashboardModel` total functions.
- [11]-[LIBRARY](.planning/board/library.md): reusable dashboard/alert pack functions over the convention vocabulary, the foundation-sdk behind the facade.

## [2]-[DOMAIN_PACKAGES]

Every telemetry-domain library the folder uses, planned or implemented. Versions are centralized in the one `pnpm-workspace.yaml` catalog and never pinned here; API evidence lives in the adjacent `.api/` folder. The `[OTLP_SDK]` rows are one pin block: the bridging sdk/exporter surface behind the `@effect/opentelemetry` facade, composed by `otlp/export` alone and collapsing as one unit when native Otlp coverage reaches parity — `semantic-conventions` survives the collapse as the standing vocabulary source.

[CONVENTIONS]:
- `@opentelemetry/semantic-conventions`

[OTLP_SDK]:
- `@opentelemetry/core`
- `@opentelemetry/resources`
- `@opentelemetry/sdk-metrics`
- `@opentelemetry/sdk-logs`
- `@opentelemetry/sdk-trace-base`
- `@opentelemetry/sdk-trace-node`
- `@opentelemetry/sdk-trace-web`
- `@opentelemetry/exporter-trace-otlp-http`
- `@opentelemetry/exporter-metrics-otlp-http`

## [3]-[SUBSTRATE_PACKAGES]

Cross-cutting TypeScript substrate the folder consumes; the branch registry lives in `libs/typescript/.planning/README.md` and shared catalogues in `libs/typescript/.api/`.

- `effect` — rails, `Schema`, `Layer`, `Match`, `Stream`, vocabulary substrate.
- `@effect/opentelemetry` — the substrate export family: the Otlp exporter/NodeSdk/WebSdk surface `otlp/export` composes; `telemetry` owns it, every folder emits through it.
- `@effect/vitest` — the dev-plane spec runner binding `proof` law combinators to the folder specs.
