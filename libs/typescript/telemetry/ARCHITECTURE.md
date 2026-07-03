# [TELEMETRY_ARCHITECTURE]

The domain map of `telemetry` — the W1 four-signal observability plane over four sub-domains: `otlp` (OTLP egress and trace-context continuity), `signal` (the convention vocabulary, RUM vitals, crash capture, and the audit/meter fact streams), `slo` (the burn-rate algebra and alert vocabulary), and `board` (dashboards as total functions). Every surface parameterizes on the kernel `AppIdentity`; the folder imports `kernel` and `host` only and publishes per-runtime subpaths (`./server`, `./browser`) plus the `plane:dev`-fenced `./dev` row.

Each codemap node is the eventual source file its `.planning/` design page becomes, named in the language's own folder and file casing — PascalCase `.cs`, lowercase `.py`, lowercase `.ts`. Treat every node as realized code; the `.planning/` scaffold is the authoring substrate, never part of the map.

## [01]-[DOMAIN_MAP]

```text codemap
telemetry/src/
├── otlp/                # OTLP egress and trace-context continuity
│   ├── export.ts        # Otlp trace/metric/log exporters; NodeSdk/WebSdk rows; egress redaction policy rows (export-boundary PII scrub); the ./dev DevTools row (plane:dev-fenced)
│   └── context.ts       # W3C composite extract-and-continue at every ingress (edge middleware, browser boot, work entities)
├── signal/              # The signal families: convention vocabulary and the vital/crash/audit/meter fact streams
│   ├── convention.ts    # semantic-convention vocabulary rows — names as data, never string literals
│   ├── vital.ts         # browser RUM via native PerformanceObserver budgets
│   ├── crash.ts         # crash capture reconstructing FaultDetail through the kernel enricher contract; replay redaction-at-capture
│   ├── audit.ts         # audit fact stream: actor/action/target vocabulary + typed diff evidence + retention classes; durable via a journal port Tag
│   └── meter.ts         # usage/cost metering fact stream: (app, tenant)-keyed request/compute/storage/token counters + rating policy rows; same journal port law
├── slo/                 # SLO as algebra
│   ├── burnrate.ts      # multi-window multi-burn-rate typed policy rows
│   └── alert.ts         # alert emission vocabulary feeding board + iac/observe
└── board/               # Dashboards as data
    ├── model.ts         # DashboardModel algebra: AppIdentity -> DashboardModel total functions
    └── library.ts       # reusable dashboard/alert pack functions over the convention vocabulary (foundation-sdk behind the facade)
```

The `signal/convention` vocabulary rows are the spine every other node composes: `otlp/export` stamps them on egress, `signal/vital`/`signal/crash`/`signal/audit`/`signal/meter` emit fact families named by them, `slo/burnrate` folds typed policy rows over the metric streams they name, `slo/alert` emits over them, and `board/model`/`board/library` derive dashboard data from the same rows keyed by `AppIdentity`. Redaction is two distinct laws: replay redaction-at-capture lives on `signal/crash`; OTLP egress redaction policy rows live on `otlp/export` at the export boundary.

## [02]-[SEAMS]

```text seams
otlp/export   ←  csharp:Rasm.AppHost/Observability/Telemetry  # [TRANSPORT]: OtelExport OTLP egress
signal/crash  ←  typescript:wire/gateway                      # [WIRE]: support-capture verb — the crash consumer through the kernel-declared fault-enricher contract, never a wire import
signal/audit  →  typescript:store/journal                     # [PORT]: audit fact stream durable through the audit journal port Tag the app root satisfies with store journal Layers
signal/meter  →  typescript:store/journal                     # [PORT]: meter fact stream durable through the meter journal port Tag the app root satisfies with store journal Layers
```

The crash seam rides the port registry: `kernel/fault` declares the fault-enricher contract, `wire` implements it, `telemetry` consumes it — the app root provides the `wire` Layer, so `signal/crash` reconstructs `FaultDetail` with no `telemetry → wire` edge. The audit and meter streams follow the same law against `store`: `telemetry` declares both journal port Tags against its own fact models, the app root satisfies them with `store` journal Layers, and the edge ledger keeps `telemetry → store` absent. The OTel Resource on `otlp/export` derives from the same kernel `AppIdentity` value `browser` boot and the `store` `StoreHandle` scope use — one identity spine from signal to dashboard.
