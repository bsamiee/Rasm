# [TS_RUNTIME_IDEAS]

Forward pool of higher-order runtime concepts grounded in the execution-substrate domain and the monorepo purpose. `[1]-[OPEN]` carries live ideas; `[2]-[CLOSED]` records a finished or dropped idea with a one-line disposition so it is never re-litigated. Ideas drive one or more `TASKLOG.md` tasks.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis — the capability outcome, never the landing motion>.
- Capability: <the higher-order invariant, owner capability, or concept established — concept grain only, never a page path, row list, or member spelling>.
- Shape: <where the work lands and at what grain — repo-relative page with section/row, or a new-page path; the concrete surface, so Capability never names it>.
- Unlocks: <the downstream capability at the consumer grain — a task narrows its parent idea's Unlocks to THIS slice as `IDEAS.md [SLUG] — consequence`; a set-completion card states the completeness bar that is its acceptance contract>.
- Anchors: <owners, seams, packages, catalogs, doctrines, and techniques making the work plausible — anchors, never procedures>.
- Arms: <BLOCKED or gated cards only; the exact observable flipping it actionable — catalog row landing, member query evidence, package admitted>.
- Route: <present only on a probe, research, or member-pin card; the ordered verification path run before any fence lands>.
- Tension: <only when an unresolved constraint, boundary, or bet shapes the work — the genuine bet, never the arming condition Arms carries>.
- Ripple: <counterpart — cross-folder `pkg` `[SLUG]`, same-folder prerequisite `[SLUG]`; load-bearing build order prefixes follows/precedes/mirrors>.
Capability, Shape, Unlocks, and Anchors are required on every open card; statuses closed — `ACTIVE|QUEUED|BLOCKED` open, `COMPLETE|DROPPED` closed; IDs are SEMANTIC UPPERCASE_SNAKE slugs carrying meaning — never numeric (`[0007]`-class NNNN IDs are a defect), for cards AND research tokens alike; a hyphenated slug anywhere is a defect; repo-relative paths only. Design pages carry the terminal `[RESEARCH]` section always — `(none)` marks empty, absence is an error. Ideas state higher-order concepts, never landing-grain tasks.
-->

(none)

## [02]-[CLOSED]

<!-- source-only: closed idea card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[ADMIT_STORE_OUTAGE]-[COMPLETE]: realized as `serve/api.md`'s `_lifted` — the credential lift reads `FaultClass[FaultClass.of(fault)].blame`, folds a caller-blamed class to `Option.none()` and refuses everything else as `shed` with the class-default window; the Tension resolved AGAINST a sixth reason, because the family is sized by outward route and the 503 route already exists — a route-sized partition takes a second cause as `detail`, never a new row.
[CONSUMPTION_AXIS_SPELLING]-[COMPLETE]: the six-axis roster landed at all three branch minters with identical closed-axis vocabularies and one common open-axis descriptor shape; refusal is one axis/value/reason grammar everywhere, and the corpus entry's roster blocker is discharged.
[WORK_METER_BRIDGE]-[COMPLETE]: work-plane meter bridge — realized as `otel/meter.md` (`Pulse`): fact-to-instrument projection over one polymorphic mount, row-projected census gauges, log-floor wiring, and the one governance view table.
[CHANNEL_MQTT]-[COMPLETE]: `net/channel.md` `Mqtt` composes the catalog-verified v5 members (`connectAsync`, `subscribeAsync`, `publishAsync`, `endAsync`, the `userProperties` carrier frame) under scoped acquisition; the `[MQTT_V5]` research row resolved against `libs/typescript/runtime/.api/mqtt.md` and is deleted.
[PROFILE_SIGNAL]-[COMPLETE]: realized as the minted `otel/profile.md` — `Profile.live` brackets init and the armed sampler roster over `@pyroscope/nodejs` with `SourceMapper.create` symbolication, the engine log bridge, `StripFilenamesMode` posture, rank-91 `Life` drain, and `Setting.otel.profile` admission.
[NODE_VITALS]-[COMPLETE]: already landed — `server.md` `[02]-[REGISTRATION]` binds `HostMetrics` and `RuntimeNodeInstrumentation` on the raw `Hooks.Meter` provider inside its one `registerInstrumentations` call, and `meter.md` `[05]-[VIEWS]` guards `v8js.*` with `createDenyListAttributesProcessor`.
[CWV_SINGLE_OWNER]-[COMPLETE]: `otel/vital.md` is the estate's one Core Web Vitals owner — `web-vitals/attribution` registrars replace the hand-rolled folds, the shipped `*Thresholds` pairs fill the budget columns, the `rating` field is the grade, the `Vital.Report` service carries both the render intake and the accounted fact stream, and the RUM context stamps `browser.*`/`device.*`/`session.*`/`network.connection.type` on the evidence span.
[TENANT_SIGNAL]-[COMPLETE]: already landed — `emit.md` `_sdk` wires `BaggageSpanProcessor(_admitted(policy.promote))` before the shared scrub, `Propagation.ingress` carries the Effect-side promotion half, `config.md` admits `Setting.otel.promote`.
[WIRE_PROTOBUF]-[COMPLETE]: already landed — `emit.md` `_wire` binds the json and protobuf exporter trios and each `_lanes` row fixes its own framing, so every deployed lane frames protobuf and the JSON trio survives as the `local` developer row.
[BROKER_ENGINE_SET]-[COMPLETE]: realized — `exec.md` runtime rows carry the `nats` TCP/TLS `connect` binding `Broker.live(dial)` consumes, and `pubsub.md` `[07]-[KAFKA_ROW]` lands `Fanout.kafka` with honest guarantee-ledger degradation over the librdkafka promise surface.
[BOARD_FEED]-[COMPLETE]: `meter.md` `[06]-[BOARD]` mints `Pulse.Board`/`Pulse.board`, and iac `operate/observe.md` admits `runtime.pulse` in `_PACKS` through the shared producer-pack ingest arm.
[HOOK_DISPATCH]-[COMPLETE]: already landed — `emit.md` `Hooks.Dispatch` seats one `Tap.Rail` per app inside the graph scope; delivery, arbitration, isolation, and the breach account are core's.
[CARRIER_CODEC_BINDING]-[COMPLETE]: `emit.md` `Propagation.current` and `pubsub.md` local, tab, NATS, and Kafka rows compose core `Carrier` with matching `fanout`, `nats`, and `kafka` dialects; `core/.planning/interchange/carrier.md` owns the exact table.
[CLOUDEVENTS_ENVELOPE]-[COMPLETE]: both HTTP directions land on the core owner rather than a page-local codec — `serve/route.md` `Inbound` detects the frame through `Format.event.framed` before decoding, sanitizes the header band, admits through `Event.Fact`/`Event.read`, and continues each member's creation-time trace, while `work/deliver.md` projects at claim time through `Hook.project`, seals the attribute set as `dssematerial`, and signs the encoded octets once; the abuse-protection handshake landed on both halves.
[JOURNAL_ENVELOPE_CARRIAGE]-[COMPLETE]: `pubsub.md` keeps `Envelope` opaque and preserves the projected body and band; `data/.planning/journal/append.md` owns strict CloudEvents construction and inverse carrier decode, so runtime carries the value without duplicating its codec.
[EVENTLOG_SYNC]-[COMPLETE]: the server half landed — `serve/live.md` `[07]` mounts `EventLogServer.makeHandlerHttp` as a row EFFECT over the `Storage` port (rows admit `Scope`, `Mount.of` takes effects uniformly), and `browser/persist.md` binds sync path ≡ mount prefix under one root value with the E2E-key law; the data-side `SqlEventLogServer.layerStorage` row is handed to the data pass.
[SERVE_LIMITER]-[COMPLETE]: `serve/route.md` `Seam.priced` lands the refusing token-cost quota (per-principal + per-route off one axis word, rendered through `Gate.fenced`'s one price) and `work/queue.md` `Throttle.pace` the delaying posture — one four-column vocabulary, one `Fleet.RateLimiter` port, scope-joined keys because the store namespaces nothing; `api.md` `Gate.fenced` widened to the `Gate.Spend` record.
[WORKLOAD_CREDENTIAL]-[COMPLETE]: the projection family landed on all three transports — `net/client.md` `Machine` audience-keyed port with the `present` lane column, `net/pubsub.md` NATS thunk-authenticator at dial (rotation replaces the connection) and Kafka SASL/OAUTHBEARER provider (rotates in place); a `dpop` principal refuses to security's proved call.
[BENCH_CLAIM_PRODUCER]-[COMPLETE]: `proc/exec.md` `[05]` rebuilt on mitata's own kernel — the hand-rolled rung sampler deleted, gc/heap/counters bands fill from engine stats through `Board.Bench.fromMitata`, counter absence is a host-fingerprint verdict, and `benchCounterKind` stamps the platform-forked leaf; no `Unknown` evidence bag survives.
[GRPC_LANE]-[COMPLETE]: the blocker fell on installed-tree re-proof — `Mount.node` was already published, and the mount completed with its missing clauses: carrier continuation foreclosed both directions (`Seam.guard` continues the one hop), `contextValues` named as the principal seam, and the interceptor-inheritance trap declared re-prove-on-bump at the validate-copy mechanism.
