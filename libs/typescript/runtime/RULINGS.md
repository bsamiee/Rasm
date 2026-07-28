# [TS_RUNTIME_RULINGS]

`typescript/runtime` rulings settle package-scoped decisions.

## [01]-[PACKAGES]

- `@opentelemetry/*` HOLDS as independent per-package pins, never one wave — the frozen api line, the stable core/resources/sdk line, and the pre-1.0 exporter and instrumentation line ride three upstream release tracks, and every detector, engine-vitals, and per-library instrumentation package versions on its own cadence beside them. Each bump moves the packages one upstream release shipped together and leaves the rest, so unifying the roster to a single version is the refuted consistency fix and claiming matched lines strands every pin that never matched.
- `@effect/cluster-node` runner welding stays never-admitted — the runner binding is a runtime-row selection (`NodeClusterSocket.layer`/`BunClusterSocket.layer`) keeping node and bun peer rows over the admitted `@effect/cluster` work plane; a survey re-proposes the node-welded family and deletes the bun row. Reopens only on a runner-neutral upstream cluster transport.
- Rpc serving on its own listener stays never-admitted — `@effect/rpc` and `@connectrpc/connect-node` are the admitted outbound dial, and Connect serving lands through serve's foreign-protocol `Mount` port, so HTTP serving keeps one front door; a standalone rpc listener mints a second public surface beside `serve`.
- `@confluentinc/kafka-javascript` is admitted over pure-JS `kafkajs` — the librdkafka client matches the C# branch's `Confluent.Kafka` on the shared broker plane, so both languages speak one client family's protocol, config, and delivery semantics; a survey reading the manifest alone re-proposes `kafkajs` blind to the parity constraint, forking the broker-plane client from its C# counterpart. Reopens only when the C# branch leaves `Confluent.Kafka`.

## [02]-[SHAPE]

- Ambient OTel globals serve foreign libraries alone while typed `Carrier` stays every branch seam's spelling — the export lane registers one `CompositePropagator` over the W3C pair and each condition node installs its own context manager, so a third-party client continues the trace instead of the no-op default and a library reading `context.active()` sees the live span, not ROOT. Refusing them strands every foreign hop; routing branch seams through them forks the dialect rows `Carrier` holds.
- Explicit-bucket histogram fallback carries two seats, one per metric plane — a `ViewOptions` aggregation re-arm reaches raw-provider instruments alone, since a producer-collected point carries finished buckets no view recomputes, so a `rasm.*` distribution fixes boundaries at its Effect mint and a foreign one names itself on the view row as an instrument-name glob. Typing that row against the Convention roster aims it at the one plane it cannot govern.
- Metric governance rides the producer seam, never the reader knobs — Effect's metric bridge registers a `MetricProducer` straight onto the reader and constructs no `MeterProvider`, so view rows and the reader's own selectors reach only raw-provider instruments while every `rasm.*` series governs through the collection-time projection at `otel/emit#GOVERNANCE`. Knobs left on the reader alone govern nothing; a second row vocabulary for the producer plane forks one governance language into two.
- `otel/vital` is the estate's one Core Web Vitals owner and `web-vitals` measures every one of them — the package folds session-windowed CLS, interactionId-grouped INP, input-finalized LCP, and activation-corrected TTFB against the standard, so a hand-rolled `PerformanceObserver` fold beside it re-derives four accounting laws that drift the moment the standard moves; the raw observer survives only for entry families the package leaves unmeasured, and a second capture registration in any package double-counts the accounting this owner performs. Grading reads the shipped `*Thresholds` pairs and the `rating` each `Metric` already carries, so the estate's grade vocabulary IS the standard's triple and a cutoff re-derived beside those pairs forks it.
- One document runs one ACCOUNTING per vital kind, never one observer per Performance-Timeline family — `web-vitals` itself observes `event` and `long-animation-frame`, so an observer-count rule refuses windows the display plane cannot avoid sharing; the browser serves every registered observer from one buffer, and a board forks only on a second capture or a second graded series, neither reachable from a plane that mints no instrument.
- `long-animation-frame` supersedes the bare `longtask` entry wherever both ship — the richer family carries the script attribution the bare entry cannot, so the display plane windows it while `runtime:otel/vital` keeps the graded jank ceiling as its one raw observer row.
- Both readers of one `event-timing` buffer take the same floor — `runtime:otel/vital` hands it to the INP registrar and `ui:system/vital` hands the same number to its own `durationThreshold`, because a display window flooring higher than the estimator strands a graded interaction on an event no board can drill into, and a floor literal at either row re-mints the divergence.

## [03]-[COLLAPSE]

- (none)

## [04]-[STRUCTURE]

- (none)

## [05]-[PROCESS]

- (none)
