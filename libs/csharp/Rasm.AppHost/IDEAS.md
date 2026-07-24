# [APPHOST_IDEAS]

Forward pool of higher-order concepts for the runtime spine, each grounded in the folder's domain and current platform capability — some are new sub-domain folders that deepen a thin owner, others bind a concrete admitted package to a settled abstract surface. Open ideas drive the tasks in `TASKLOG.md`; a finished or dropped idea moves to `[2]-[CLOSED]` with a one-line disposition so it is never re-litigated.

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

[WIRE_CARRIER_ADAPTERS]-[BLOCKED]: One wire-carrier adapter family spans every transport crossing in both directions.
- Capability: gRPC, generic, and MQTT publish adapters — all settled — complete with MQTT receive, so every context-carrying crossing shares one carrier law.
- Shape: the MQTT receive adapter beside the settled publish adapters on the AppHost wire plane.
- Unlocks: bidirectional MQTT crossings under the one carrier adapter family.
- Anchors: the settled gRPC, generic, and MQTT publish adapters; `libs/csharp/Rasm.AppHost/.api/api-mqtt.md`; `libs/csharp/.api/`.
- Arms: `libs/csharp/Rasm.AppHost/.api/api-mqtt.md` or `libs/csharp/.api/` carries the exact `MqttApplicationMessage` user-property collection and entry accessors.
[TELEMETRY_OFFLINE_SPINE]-[BLOCKED]: Telemetry survives disconnection through a per-signal disk-retry spine.
- Capability: an `OfflineQueue`-backed exporter retry binding holds every signal through an outage, so a field machine ships complete history on reconnect.
- Shape: the offline-retry binding on the AppHost telemetry exporter owners.
- Unlocks: lossless telemetry from disconnected sites.
- Anchors: `libs/csharp/Rasm.AppHost/.api/api-otel-exporter.md`; `libs/csharp/.api/api-opentelemetry-exporter-otlp.md`.
- Arms: either catalog carries an exact per-signal disk-retry member or environment binding and its `FileBlobProvider` composition law.
[CORPUS_GATE_INGEST]-[BLOCKED]: One corpus gate ingests benchmark claim projections from every producer.
- Capability: kernel, Bim, Persistence, and Rhino projections — all settled — complete with Materials and Grasshopper, estate-wide claims grading through one gate.
- Shape: the Materials and Grasshopper projections routed by `Observability/benchmarks.md#[CLAIM_FAMILY_ADMISSION]`.
- Unlocks: estate-wide benchmark claims grade through one corpus gate.
- Anchors: the settled kernel, Bim, Persistence, and Rhino projections; `Observability/benchmarks.md#[CLAIM_FAMILY_ADMISSION]`.
- Arms: both owner routes in `Observability/benchmarks.md#[CLAIM_FAMILY_ADMISSION]` close.

## [02]-[CLOSED]

<!-- source-only: closed idea card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[TENANT_COST_SPAN_PROMOTION]-[COMPLETE]: `AddBaggageActivityProcessor(PromotedBaggage)` allowlist rows landed at both provider owners with the governance-table policy row.
[FLEET_DEPLOY_ANNOTATIONS]-[COMPLETE]: `RollAnnotationWire` record and per-wave projection fold landed on `Sandbox/provisioning.md#ROLLOVER_DRAIN`, fanned under `InstrumentFan.RollKind` and joined to the `AppHostWireContext` roster.
[DIAGNOSTIC_HEAP_ANALYSIS]-[COMPLETE]: `DumpTriage.Walk` ClrMD fold and the `SupportArtifact.DumpAnalysis` factory row landed on `Observability/bundles.md#CAPTURE_PIPELINE` under `DumpPolicy.CensusCap`/`TriageRows`/`FrameCap` bounds with shallow heap-sample semantics.
[MACHINE_OBSERVATION_LANE]-[COMPLETE]: `MachineLane`/`MachineObservationWire` decode lane landed on `Wire/livewire.md` fanned under `InstrumentFan.ObservationKind`; the `MachineObservation` domain vocabulary is Fabrication-owned per its observation card.
[DOMAIN_INSTRUMENT_CATALOG]-[COMPLETE]: Domain-instrument catalog over the receipt fan — landed as `.planning/Observability/instruments.md` with the roster, projection fold, per-ALC provider capsule, and observation rail.
[TYPED_HOOK_RAIL]-[COMPLETE]: Typed hook rail over the existing bus, lifecycle, and receipt seams — landed as `.planning/Observability/hooks.md` with id grammar, modality rows, registry, and fault isolation.
[BENCHMARK_RECEIPT_FAMILY]-[COMPLETE]: Benchmark receipt family and corpus-gate ownership — landed as `.planning/Observability/benchmarks.md` with the gate fold, bundle capture seam, and span-profile correlation.
[PARTITIONED_OBSERVABLE_LEVELS]-[COMPLETE]: realized kernel-side per the substrate-homing collapse — keyed `LevelCells` families with `Reader` project per-key levels as tagged `Measurement<long>` rows through the multi-measurement observe overload at `libs/csharp/Rasm/.planning/Domain/telemetry.md` `[03]-[INSTRUMENT_MECHANISM]`, so a bespoke per-partition gauge is the deleted form.
[CONTRIBUTED_ARM_ROSTER]-[COMPLETE]: the `[CONTRIBUTED_ARMS]` table landed at `Observability/instruments.md` `[03]-[RECEIPT_PROJECTION]` with all ten contributor rows — each domain package's projection, port mint, and custody named.
