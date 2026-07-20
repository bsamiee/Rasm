# [APPHOST_IDEAS]

Forward pool of higher-order concepts for the runtime spine, each grounded in the folder's domain and current platform capability — some are new sub-domain folders that deepen a thin owner, others bind a concrete admitted package to a settled abstract surface. Open ideas drive the tasks in `TASKLOG.md`; a finished or dropped idea moves to `[2]-[CLOSED]` with a one-line disposition so it is never re-litigated.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis>.
- Capability: <higher-order concept, invariant, or owner capability>.
- Shape: <what the idea becomes as a system, product, owner, or feature set(s)>.
- Unlocks: <new branch, package, workflow, proof, user, or agent capability made possible>.
- Anchors: <owners, seams, packages, doctrines, or techniques that make the idea plausible>.
- Tension: <only when an unresolved constraint, boundary, bet, or dependency shapes the idea>.
- Ripple: <origin/counterpart card this entry pairs with across folders, as `pkg` `[SLUG]`; present only on a cross-folder ripple counterpart card>.
-->

[WIRE_CARRIER_ADAPTERS]-[QUEUED]: Trace context crosses every estate transport — NATS headers, MQTT v5 user properties, and CloudEvents tracing attributes become adapter rows on the one propagation spine.
- Capability: `TraceContext` grows three carrier adapters — getter/setter pairs delegating to the composite propagator — so `traceparent`/`tracestate` and baggage ride `NatsHeaders`, the MQTT v5 user-property collection, and the CloudEvents `traceparent`/`tracestate` extension attributes; per-transport hand-rolled header writes stay the deleted form.
- Shape: three rows under the `Observability/telemetry.md#CORRELATION_SPINE` growth law — a new propagation carrier is one `IPropagator`-backed adapter on the same `Spine` composite; the MQTT adapter mounts at the `Wire/livewire.md` `MqttLane` publish and receive edges, the NATS and CloudEvents adapters compose at the Persistence egress legs.
- Unlocks: broker-hop trace continuity — a spine delivery, CDC CloudEvent, or live-wire MQTT command joins the same trace the gRPC and Kafka legs already carry, and TraceBased exemplars survive the hop.
- Anchors: the wire-law carrier table (NATS carries no OTel instrumentation by design — manual inject and extract), `NATS.Net` `NatsHeaders`, MQTTnet `MqttApplicationMessageBuilder`, `CloudNative.CloudEvents` extension-attribute surface, `TextMapPropagator.Inject`/`Extract`.
- Ripple: `Rasm.Persistence` `[0003]`.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

- [0001]-[COMPLETE]: Domain-instrument catalog over the receipt fan — landed as `.planning/Observability/instruments.md` with the roster, projection fold, per-ALC provider capsule, and observation rail.
- [0002]-[COMPLETE]: Typed hook rail over the existing bus, lifecycle, and receipt seams — landed as `.planning/Observability/hooks.md` with id grammar, modality rows, registry, and fault isolation.
- [0003]-[COMPLETE]: Benchmark receipt family and corpus-gate ownership — landed as `.planning/Observability/benchmarks.md` with the gate fold, bundle capture seam, and span-profile correlation.
