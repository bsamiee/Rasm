# [TS_RUNTIME_RULINGS]

`typescript/runtime` rulings settle package-scoped decisions.

## [01]-[PACKAGES]

- `@opentelemetry/*` holds per-package pins — api, core, and exporter ride three upstream tracks, so one version strands pins that never matched.
- `@effect/cluster-node` stays never-admitted — the runner is a runtime-row selection, so welding it deletes the `BunClusterSocket.layer` peer row.
- Rpc on its own listener stays unadmitted — `@effect/rpc` dials outbound and Connect serving rides the `Mount` port, so HTTP keeps ONE front door.
- `@confluentinc/kafka-javascript` admits over `kafkajs` — its librdkafka core is what C# `Confluent.Kafka` binds, so both branches speak one client.

## [02]-[SHAPE]

- `@pyroscope/nodejs`'s `cpu` profiler pair stays never-seated — it aliases the wall profiler, so a row beside `wall` arms one engine twice.
- Ambient OTel globals serve foreign libraries and `Carrier` spells every branch seam — `CompositePropagator` continues a trace across a foreign hop.
- Explicit-bucket fallback takes two seats — a `ViewOptions` re-arm reaches raw-provider instruments, so `rasm.*` fixes bounds at its Effect mint.
- Metric governance rides `otel/emit#GOVERNANCE` — Effect's bridge takes a `MetricProducer` and no `MeterProvider`, so reader knobs govern nothing.
- Cumulative rail tallies reach counters as DELTAS against one held sample — a counter set to a running total re-counts every prior interval.
- Columns only one reason fills seat REQUIRED there — an `Option` shared family-wide lets a refusal construct without the evidence that IS it.
- `otel/vital` owns Core Web Vitals graded on `web-vitals`'s own `*Thresholds` — a second capture double-counts, a local cutoff forks the standard.
- One `web-vitals` accounting runs per vital kind per document — a second accounting forks the session total restore-minted instances chain.
- `long-animation-frame` supersedes the bare `longtask` entry wherever both ship — the richer family carries script attribution the bare one cannot.
- Seams closing verdicts over domain classes declare the store channel — a decoding discharge fails `SqlError` beside `ParseError`, never the cause.
- Every dispatch row elects its own re-drive class — a table with no class column re-drives one arm's host crossing on the budget its sibling earned.
- Batch discharges answer one fence per requested identity — a pass metering on statement silence counts a displaced claimant's mark delivered.
- `_Host` extends with `surface`/`lanes`/`document` and `_Provider` with `supplies`; each forecloses a coordinate on the family lead, never a column.
- `event-timing` readers share ONE floor — the INP registrar and `ui:system/vital`'s `durationThreshold` read it, a literal at either stranding both.
- Fanout transport rows extend the consumption descriptor with `serves` and `anchors` alone — `serves` maps the PORT MEMBERS a refusal mints off.
- Every fanout engine answers `pulse` — a failure no await reaches projects onto the owner's fault family; a row behind none answers `Stream.empty`.
- Foreign void-returning SDK members bridge through `Runtime.runSync` over a total effect — hosts swallow throws and the promise bridge strands work.
- Provider verdicts read the whole finish roster as three bands — a per-modality table forks where one finish reason grades two ways.
- One store-backed limiter port serves refusal and delay alike — serve refuses, work delays; the root Layer picks per-process or fleet-wide.
- Inbound W3C hops continue exactly once, at `Seam.guard` — a mounted protocol re-extracting the same headers mints a second trace of one hop.
- Long-lived regions take the profile band's effectful arm, their scoped span the anchor — sample labels ride synchronous kernels alone.
- Transport credentials project off ONE `Machine` port keyed by AUDIENCE — a lane-keyed read hands one service's token to another.
- `present` spells credential residence on every egress-lane and engine row — where the credential lives, what a rotation costs; no second column.
- Rotating credentials ride the transport's own refresh seam — dial-time authenticator, provider callback, per-call stamp — never a timer beside it.
- Residency supersession keys the depot's own replacement epoch — `Manifest.version` is a schema pin, so a guard on it admits every stale arrival.

## [03]-[COLLAPSE]

- `Hooks.Dispatch` IS the app's `Tap.Rail` seat — a runtime rail table, publish permit, or replay journal forks the veto order and breach account.
- Transports hand `Carrier.extract`'s extraction WHOLE to `Propagation.ingress` — destructuring at the seam drops a census no second reader keeps.
- `_Provider` rows differing only by a frozen option record collapse to ONE row taking the policy — configuration alone is one capability twice.
- Credential-header masking is TWO disjoint rosters — `Redactable` covers a live `Headers` value, `Redaction.sealed` every bag copied out of it.

## [04]-[STRUCTURE]

- Condition modules carrying a row roster earn a codemap node and a page; one-row seats (`otel/dev.ts`, `proc/worker.main.ts`) ride strata prose.

## [05]-[PROCESS]

- (none)
