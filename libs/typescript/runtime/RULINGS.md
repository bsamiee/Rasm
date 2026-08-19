# [TS_RUNTIME_RULINGS]

`typescript/runtime` rulings settle package-scoped decisions.

## [01]-[PACKAGES]

- `@opentelemetry/*` holds per-package pins — api, core, and exporter ride three upstream tracks, so one version strands pins that never matched.
- `@effect/cluster-node` stays never-admitted — the runner is a runtime-row selection, so welding it deletes the `BunClusterSocket.layer` peer row.
- Rpc on its own listener stays unadmitted — `@effect/rpc` dials outbound and Connect serving rides the `Mount` port, so HTTP keeps ONE front door.
- `@confluentinc/kafka-javascript` admits over `kafkajs` — its librdkafka core is what C# `Confluent.Kafka` binds, so both branches speak one client.
- `@pyroscope/nodejs`'s `cpu` profiler pair stays never-seated — it aliases the wall profiler, so a row beside `wall` arms one engine twice.

## [02]-[SHAPE]

- Ambient OTel globals serve foreign libraries and `Carrier` spells every branch seam — `CompositePropagator` continues a trace across a foreign hop.
- Explicit-bucket fallback takes two seats — a `ViewOptions` re-arm reaches raw-provider instruments, so `rasm.*` fixes bounds at its Effect mint.
- Metric governance rides `otel/emit#GOVERNANCE` — Effect's bridge takes a `MetricProducer` and no `MeterProvider`, so reader knobs govern nothing.
- `otel/vital` owns Core Web Vitals, grading on `web-vitals`'s `*Thresholds` — a second capture double-counts, a local cutoff forks the standard.
- One document runs ONE `web-vitals` accounting per vital kind, never an observer per entry family — one buffer serves every registered observer.
- `long-animation-frame` supersedes the bare `longtask` entry wherever both ship — the richer family carries script attribution the bare one cannot.
- Seams whose verdicts close over domain classes declare their store channel — `Lane.settle` fails `SqlError` rather than widening it into the cause.
- `_Host` extends with `surface`/`lanes`/`document` and `_Provider` with `supplies`; each forecloses a coordinate on the family lead, never a column.
- `event-timing` readers share ONE floor — the INP registrar and `ui:system/vital`'s `durationThreshold` read it, a literal at either stranding both.
- Fanout transport rows extend the descriptor with `serves` and `anchors` alone — `serves` maps the PORT MEMBERS a refusal mints off.
- Every fanout engine answers `pulse` — a failure no await reaches projects onto the owner's fault family; a row behind none answers `Stream.empty`.
- Foreign void-returning SDK members bridge through `Runtime.runSync` over a total effect — hosts swallow throws and the promise bridge strands work.
- Provider verdicts read the whole finish roster as three bands — refusal, unfinished work, settlement — and one table serves every modality.
- One store-backed limiter port serves every posture — serve refuses on it, work delays on it; the root Layer alone picks per-process or fleet-wide.
- Credential-header masking is TWO disjoint rosters — `Redactable` covers a live `Headers` value, `Redaction.sealed` every bag copied out of it.
- Inbound W3C hops continue exactly once, at `Seam.guard` — a mounted protocol re-extracting the same headers mints a second trace of one hop.
- Long-lived regions take the profile band's effectful arm, their scoped span the anchor — sample labels ride synchronous kernels alone.
- Transport credentials project off ONE `Machine` port keyed by AUDIENCE — a lane-keyed read hands one service's token to another.
- `present` spells credential posture on every lane and engine row — where the credential lives, what a rotation costs; no second column states it.
- Rotating credentials ride the transport's own refresh seam — dial-time authenticator, provider callback, or per-call stamp — never a lane timer.

## [03]-[COLLAPSE]

- `_Provider` rows differing only by a frozen option record collapse to ONE row taking the policy — configuration alone is one capability twice.

## [04]-[STRUCTURE]

- (none)

## [05]-[PROCESS]

- (none)
