# [PY_RUNTIME_IDEAS]

`runtime`'s forward pool of higher-order folder concepts, grounded in the numeric-science domain and the monorepo purpose. `[1]-[OPEN]` carries the active ideas as cards; each card names the capability, what it unlocks, and the gap or technique it draws on. `[2]-[CLOSED]` carries the finished or dropped ideas with a one-line disposition so the same idea is never re-litigated. An idea drives one or more `TASKLOG.md` tasks.

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

[EGRESS_TRANSPORT_ROW]-[QUEUED]: OTLP egress transport becomes a selectable axis on the telemetry install.
- Capability: `opentelemetry-exporter-otlp-proto-grpc` joins as a daemon-only transport row beside the proto-http default — persistent-channel export where streaming throughput dominates.
- Shape: one `EgressTransport` vocabulary keying a per-transport exporter-factory triple; `SignalProfile` gains a `transport` column with the SIDECAR row alone eligible for the gRPC value; http rows keep the `/v1/<signal>` path derivation, the gRPC row targets `host:port` with no path.
- Unlocks: channel-reuse export for the long-lived serve daemon with the flush law and job envelope untouched.
- Anchors: the `SIGNAL_SPECS` exporter-factory seam and `SIGNAL_PROFILE` table on `observability/telemetry#TELEMETRY`; `.api/opentelemetry-exporter-otlp-proto-grpc.md`.
- Tension: a gRPC channel does not survive `fork()`, so every worker-forking profile refuses the row structurally; proto-http stays the sole estate default by settled ruling.

[PRODUCER_DISTRIBUTIONS]-[BLOCKED]: geometry distribution rows on the one `INSTRUMENTS` table.
- Capability: deviation magnitude, mesh genus/aspect, and EUI land as `rasm.geometry.<measure>` histogram rows beside `rasm.geometry.evidence.duration`.
- Shape: one `InstrumentSpec` row with one `SyncInstruments` field per measure, reaching `record`'s mapping arm through `_DOMAIN_SLOT` with zero entrypoint edits.
- Unlocks: geometry-domain dashboards aggregate distributions without receipt post-processing.
- Anchors: the `INSTRUMENTS` growth law and the `record` mapping arm on `observability/metrics#METRIC`.
- Tension: receipts stay the data plane by ruling; a row lands only when a dashboard charter names its measure.
- Ripple: `geometry` `[PRODUCER_DISTRIBUTIONS]`.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

[OBSERVABILITY_SPINE_WAVE]-[COMPLETE]: landed as three observability owners and the metrics deepening — `observability/logging` chain owner under the stdout ship law with the in-process OTLP escape hatch gated by `LogShip`; `observability/hooks` scoped registry (`rasm.<pkg>.<domain>.<point>` ids, veto/observe/replay modalities, telemetry taps); `observability/profiles` pyroscope push, benchmark-receipt family, and the offline-job flush envelope; metrics gained `rasm.<domain>.<measure>` wire naming, the tenant baggage dimension, and the composition-root instrumentor train.

[REMOTE_WORKER_DISPATCH]-[COMPLETE]: landed as `WorkerKind.REMOTE` on `execution/workers` — `KIND_POLICY(fidelity=True, restart=RetryClass.SSH)`, the `WorkerPool` remote arm over one `transport/roots` `RemoteEndpoint` channel with `remote_floor` far-side, shm-wire refusal, channel-liveness supervision; roots scope law widened one seam.

[SHARED_MEMORY_CHANNEL]-[COMPLETE]: landed as the `Wire.SHARED_MEMORY` span channel on `execution/workers#FABRIC` — `ShmSpan` named blocks, exporter-owned unlink, worker-side `numpy.frombuffer` reconstruction, ingress-only law.
