# [TS_RUNTIME_IDEAS]

Forward pool of higher-order runtime concepts grounded in the execution-substrate domain and the monorepo purpose. `[1]-[OPEN]` carries live ideas; `[2]-[CLOSED]` records a finished or dropped idea with a one-line disposition so it is never re-litigated. An idea drives one or more `TASKLOG.md` tasks.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis — the capability outcome, never the landing motion>.
- Capability: <the higher-order invariant, owner capability, or concept established — altitude only, never a page path, row list, or member spelling>.
- Shape: <where the work lands and at what grain — repo-relative page with section/row, or a new-page path; the concrete surface, so Capability never names it>.
- Unlocks: <the downstream capability at the consumer grain — a task narrows its parent idea's Unlocks to THIS slice as `IDEAS.md [SLUG] — consequence`; a set-completion card states the completeness bar that is its acceptance contract>.
- Anchors: <owners, seams, packages, catalogs, doctrines, and techniques making the work plausible — anchors, never procedures>.
- Arms: <BLOCKED or gated cards only; the exact observable flipping it actionable — catalog row landing, member query evidence, package admitted>.
- Route: <present only on a probe, research, or member-pin card; the ordered verification path run before any fence lands>.
- Tension: <only when an unresolved constraint, boundary, or bet shapes the work — the genuine bet, never the arming condition Arms carries>.
- Ripple: <counterpart — cross-folder `pkg` `[SLUG]`, same-folder prerequisite `[SLUG]`; load-bearing build order prefixes follows/precedes/mirrors>.
Capability, Shape, Unlocks, and Anchors are required on every open card; statuses closed — `ACTIVE|QUEUED|BLOCKED` open, `COMPLETE|DROPPED` closed; IDs are SEMANTIC UPPERCASE_SNAKE slugs carrying meaning — never numeric (`[0007]`-class NNNN IDs are a defect), for cards AND research tokens alike; a hyphenated slug anywhere is a defect; repo-relative paths only. Design pages carry the terminal `[RESEARCH]` section always — `(none)` marks empty, absence is an error. Ideas state higher-order concepts, never landing-grain tasks.
-->

[EVENTLOG_SYNC]-[QUEUED]: Offline-first closes its loop — the EventLog overlay gains its server half.
- Capability: browser EventLog writes replicate through a mounted server handler so the persist overlay stops being a client-only diary — encrypted event sync, remote flush on reconnect, and multi-device convergence ride the shipped protocol instead of a bespoke sync endpoint.
- Shape: an `EventLogServer.makeHandlerHttp` mount row in `libs/typescript/runtime/.planning/serve/live.md`'s foreign-protocol Mount port with storage satisfied by the data plane; the overlay's remote registration declared at the seam in `libs/typescript/runtime/.planning/browser/persist.md`.
- Unlocks: field capture — site surveys, fabrication travelers — syncs when connectivity returns; the browser plane gains durable multi-device state without a second store.
- Anchors: branch `.api/effect-experimental.md` (`EventLogServer.makeHandlerHttp`); `browser/persist.md` overlay bindings; `serve/live.md` mount port.

[SERVE_LIMITER]-[QUEUED]: Admission throttling becomes a policy value on the serving edge and tenant egress.
- Capability: request admission and tenant egress inherit persistent token-cost rate limits — window, tokens, cost-per-route as policy rows over a store-backed limiter — so a burst tenant degrades to a `Problem`-rendered refusal instead of starving peers, and the durable-queue throttles and the serving gate share one limiter vocabulary.
- Shape: a limiter ceremony row in `libs/typescript/runtime/.planning/serve/route.md` with per-principal and per-route cost columns, and a `RateLimiter.makeWithRateLimiter`-backed policy row beside the keyed throttles in `libs/typescript/runtime/.planning/work/queue.md`.
- Unlocks: multi-tenant fairness as data; the app-neutrality law holds under contention.
- Anchors: branch `.api/effect-experimental.md` (`RateLimiter.makeWithRateLimiter`); `serve/route.md` ceremony rows; `work/queue.md` throttle cluster; the limiter-posture ruling at `libs/typescript/.planning/RULINGS.md` `[02]-[COLLAPSE]` — shared row shape, three site-owned postures, never one owner.

[WORKLOAD_CREDENTIAL]-[QUEUED]: Workload-identity credential projection mounts on the transport lanes.
- Capability: per-call transport credentials — gRPC metadata, NATS auth header — source from the security machine principal and refresh on its grant lifecycle, so a fleet worker authenticates every outbound call without a hand-carried static token and credential rotation never restarts a lane.
- Shape: a credential-projection row on `libs/typescript/runtime/.planning/net/client.md`'s lane table and the NATS auth row in `libs/typescript/runtime/.planning/net/pubsub.md`, both reading a `Redacted`-typed principal the security plane resolves; refresh rides the grant lifecycle, never a lane timer.
- Unlocks: service-to-service auth on every transport axis with one principal source; the C# gRPC host accepts TS calls under one credential law.
- Anchors: security `authn/workload.md` machine-principal projection (carded); `net/client.md` lane table budget and circuit rows; `net/pubsub.md` connection rows.
- Tension: principal mint and refresh are security's — this plane mounts the projection and never touches grant grammar.
- Ripple: `security` `[WORKLOAD_IDENTITY]`.

[BENCH_CLAIM_PRODUCER]-[BLOCKED]: Measured-run receipts gain the mitata deep-sampling modality.
- Capability: benchmark claims ride one typed receipt producer whose sampling evidence carries no `Unknown` evidence bag.
- Shape: the sampling modality row on `libs/typescript/runtime/.planning/proc/exec.md` `[05]-[MEASURED_RUN]`, gated by its `[06]-[RESEARCH]` `[TRIAL_ENGINE]` row.
- Unlocks: package-independent benchmark claims with typed deep-sampling evidence on the measured-run rail.
- Anchors: `proc/exec.md` `[05]-[MEASURED_RUN]` receipts; `.api/mitata.md` at the folder and branch tiers.
- Arms: an applicable mitata catalog carries exact rows for every composed member — `measure`, `do_not_optimize`, result fields, batch and GC controls.

[CLOUDEVENTS_ENVELOPE]-[BLOCKED]: Delivery egress and serving intake speak verified CloudEvents HTTP bindings.
- Capability: one CloudEvents codec pair — egress preserving structured content type, binary data bytes, and W3C extension attributes; intake auto-detecting binary versus structured mode with decode evidence on the `Problem` rail.
- Shape: the egress codec on `libs/typescript/runtime/.planning/work/deliver.md`, gated by its `[07]-[RESEARCH]` `[CLOUDEVENTS_EGRESS]` row; the intake codec on `libs/typescript/runtime/.planning/serve/route.md`, gated by its `[07]-[RESEARCH]` `[CLOUDEVENTS_INTAKE]` row.
- Unlocks: standards-shaped event crossings on both HTTP directions without an unverified fence.
- Anchors: `.api/cloudevents.md` at the folder and branch tiers; `work/deliver.md` `HookPayload` signing seam; `serve/route.md` `Problem` rail.
- Arms: an applicable CloudEvents catalog carries exact rows for both HTTP binding directions.

[GRPC_LANE]-[BLOCKED]: Connect transport completes — the W3C interceptor pair and the guarded server mount.
- Capability: immutable W3C injection and extraction ride the runtime `Propagation` owner on both Connect directions, and the server handler mounts behind `Seam.guard` with context continued before the handler.
- Shape: the interceptor pair on `libs/typescript/runtime/.planning/net/client.md` `[06]-[CONNECT_ROW]`, gated by its `[07]-[RESEARCH]` `[CONNECT_INTERCEPTORS]` row; the guarded mount on `libs/typescript/runtime/.planning/serve/live.md`, gated by its `[08]-[RESEARCH]` `[CONNECT_MOUNT]` row.
- Unlocks: gRPC lanes carry trace context and mount under the one guard law with no call-site header thunk.
- Anchors: `.api/connectrpc-connect-node.md` at the folder and branch tiers; core `.api/connectrpc-connect.md` peer contract; `.api/effect-platform-node.md` host interop rows; the rpc admission boundary at `libs/typescript/runtime/RULINGS.md` `[01]-[PACKAGES]` — dial admitted, serving only through the `Mount` port.
- Arms: the client and server interceptor members, the composite carrier setter, and the `connectNodeAdapter` handler lift all carry exact catalog rows.

## [02]-[CLOSED]

<!-- source-only: closed idea card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[WORK_METER_BRIDGE]-[COMPLETE]: work-plane meter bridge — realized as `otel/meter.md` (`Pulse`): fact→instrument projection, census gauges, log-floor wiring, tenant views.
[CHANNEL_MQTT]-[COMPLETE]: `net/channel.md` `Mqtt` composes the catalog-verified v5 members (`connectAsync`, `subscribeAsync`, `publishAsync`, `endAsync`, the `userProperties` carrier frame) under scoped acquisition; the `[MQTT_V5]` research row resolved against `libs/typescript/core/.api/mqtt.md` and is deleted.
[PROFILE_SIGNAL]-[COMPLETE]: realized as the minted `otel/profile.md` — `Profile.live` init/start/stop bracket over `@pyroscope/nodejs` with `SourceMapper.create` symbolication, `StripFilenamesMode` posture, rank-91 `Life` drain, and `Setting.otel.profile` admission.
[NODE_VITALS]-[COMPLETE]: already landed — `emit.md` `_vitals` binds `HostMetrics` and `RuntimeNodeInstrumentation` on the raw `Hooks.Meter` provider, `meter.md` `[06]-[ENGINE]` guards `v8js.*` with `createDenyListAttributesProcessor`.
[TENANT_SIGNAL]-[COMPLETE]: already landed — `emit.md` `_sdk` wires `BaggageSpanProcessor(_admitted(policy.promote))` before the shared scrub, `Propagation.ingress` carries the Effect-side promotion half, `config.md` admits `Setting.otel.promote`.
[WIRE_PROTOBUF]-[COMPLETE]: already landed — `emit.md` `_wire` dispatches `policy.serialization` across the json/protobuf exporter trios for traces, metrics, and logs under one policy row set.
[BROKER_ENGINE_SET]-[COMPLETE]: realized — `exec.md` runtime rows carry the `nats` TCP/TLS `connect` binding `Broker.live(dial)` consumes, and `pubsub.md` `[07]-[KAFKA_ROW]` lands `Fanout.kafka` with honest guarantee-ledger degradation over the librdkafka promise surface.
[BOARD_FEED]-[COMPLETE]: `meter.md` `[07]-[BOARD]` mints `Pulse.Board`/`Pulse.board`, and iac `operate/observe.md` admits `runtime.pulse` in `_PACKS` through the shared producer-pack ingest arm.
[HOOK_DISPATCH]-[COMPLETE]: already landed — `emit.md` `Hooks.Dispatch` executes the core `Tap` vocabulary with app-scoped rails, pure veto fold, isolated delivery fibers, and the bounded replay ring.
[CARRIER_CODEC_BINDING]-[COMPLETE]: `emit.md` `Propagation.current` and `pubsub.md` local, tab, NATS, and Kafka rows compose core `Carrier` with matching `fanout`, `nats`, and `kafka` dialects; `core/.planning/interchange/carrier.md` owns the exact table.
[JOURNAL_ENVELOPE_CARRIAGE]-[COMPLETE]: `pubsub.md` keeps `Envelope` opaque and preserves the projected body and band; `data/.planning/journal/append.md` owns strict CloudEvents construction and inverse carrier decode, so runtime carries the value without duplicating its codec.
