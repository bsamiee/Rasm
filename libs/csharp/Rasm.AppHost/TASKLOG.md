# [APPHOST_TASKLOG]

Open and closed work for the runtime spine, distilled from `IDEAS.md`. Each open task carries a status marker and the capability-to-build, packages, integration points/boundaries, and key considerations; one idea spawns one or more tasks across one or more files. Closed cards record already-settled cleanup.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with. `Atomic` flags a minor-scope task so a later session sizes its turn correctly and does not overscope a batch of small items.

## [01]-[OPEN]

<!-- source-only: open task card template:
[ID]-[STATUS]: <ambitious concise thesis — the capability outcome, never the landing motion>.
- Capability: <the higher-order invariant, owner capability, or concept established — altitude only, never a page path, row list, or member spelling>.
- Shape: <where the work lands and at what grain — repo-relative page with section/row, or a new-page path; the concrete surface, so Capability never names it>.
- Unlocks: <the downstream capability at the consumer grain — a task narrows its parent idea's Unlocks to THIS slice as `IDEAS.md [SLUG] — consequence`; a set-completion card states the completeness bar that is its acceptance contract>.
- Anchors: <owners, seams, packages, catalogs, doctrines, and techniques making the work plausible — anchors, never procedures>.
- Arms: <BLOCKED or gated cards only; the exact observable flipping it actionable — catalog row landing, member query evidence, package admitted>.
- Route: <present only on a probe, research, or member-pin card; the ordered verification path run before any fence lands>.
- Tension: <only when an unresolved constraint, boundary, or bet shapes the work — the genuine bet, never the arming condition Arms carries>.
- Ripple: <counterpart — cross-folder `pkg` `[SLUG]`, same-folder prerequisite `[SLUG]`; load-bearing build order prefixes follows/precedes/mirrors>.
- Atomic: <present only on a minor-scope task; names the small unit so a later session sizes its turn>.
Capability, Shape, Unlocks, and Anchors are required on every open card, Atomic included; statuses closed — `ACTIVE|QUEUED|BLOCKED` open, `COMPLETE|DROPPED` closed; IDs are SEMANTIC UPPERCASE_SNAKE slugs carrying meaning — never numeric (`[0007]`-class NNNN IDs are a defect), for cards AND research tokens alike; a hyphenated slug anywhere is a defect; repo-relative paths only. Design pages carry the terminal `[RESEARCH]` section always — `(none)` marks empty, absence is an error. Tasks state landing-grain work decomposing an idea.
-->

[CAPSULE_PORT_RETARGET]-[QUEUED]: App-platform ports compose the kernel causal frame — the L3-declared port types retarget to capsule owners.
- Capability: the runtime spine consumes kernel-owned correlation, tenancy, envelope, and receipt-sink types and its instrument rows adopt the kernel bind factories, so the platform declares only what is genuinely app-tier.
- Shape: `libs/csharp/Rasm.AppHost/.planning/Runtime/ports.md` port record fences compose the kernel `CorrelationId`/`TenantContext`/`ReceiptEnvelope`/`ReceiptSinkPort`; `libs/csharp/Rasm.AppHost/.planning/Observability/instruments.md` rows adopt the kernel `InstrumentSpec` bind factories; `libs/csharp/Rasm.AppHost/.planning/Observability/telemetry.md` `[02]` re-cuts `TelemetrySource` to composition of the kernel package-identity rows, keeping foreign-source rows and resource lacing.
- Unlocks: `Rasm.Fabrication` and the L3 peers name the receipt seam with no strata inversion; HLC, OTel, and baggage lacing stays sole AppHost custody.
- Anchors: `libs/csharp/.planning/RULINGS.md` causal-frame and instrument-spec rows; kernel `Domain/telemetry.md` capsule.
- Ripple: follows `Rasm` `[CAPSULE_EXTENSION_MINTS]`.

[EGRESS_CARRIER_SETTERS]-[QUEUED]: Egress carrier setter bodies land beside the propagator so no transport leg hand-formats a traceparent.
- Capability: context injection reaches every egress carrier through propagator setter bodies; a transport leg receives its stamped envelope, never a format recipe.
- Shape: `TraceContext.Inject` setter fences for the CloudEvents and MQTT carriers on `libs/csharp/Rasm.AppHost/.planning/Observability/telemetry.md` `[04]` beside the propagator law.
- Unlocks: `Rasm.Persistence` `[TRACE_STAMP_PORT]` composes the stamped envelope and the hand-formatted header literal deletes.
- Anchors: the page's hand-rolled-write-is-deleted-form clause; `libs/csharp/.api/api-diagnostics-activity.md` propagator rows.
- Ripple: precedes `Rasm.Persistence` `[TRACE_STAMP_PORT]`.
- Atomic: two setter fences.

[IDEMPOTENCY_CLAUSE_RECUT]-[QUEUED]: Re-cut the capability page's shared-semantic clause so it stops reading as a merge license.
- Capability: the op-level idempotency clause states the two-scope truth — the `Keyed` row carries one meaning on both layers while the op and hop owners stay two types — so a dedup sweep starting from either page reads the guard, not an invitation.
- Shape: one sentence repair on `libs/csharp/Rasm.AppHost/.planning/Agent/capability.md` `[02]` Boundary — the "same vocabulary... never two" wording re-cut to the row-meaning-equivalence reading.
- Unlocks: `RULINGS.md` `[01]` collapse refusal holds with zero inviting prose.
- Anchors: the folder `RULINGS.md` idempotency row; `Wire/outbound.md` `HopIdempotency` rows.
- Atomic: one sentence.

[OUTBOX_ENVELOPE_SPELLING]-[QUEUED]: Outbox relay prose names the realized Persistence envelope owner.
- Capability: the relay's drain law cites the projection by its owning spelling, so the cross-package vocabulary carries one name.
- Shape: `libs/csharp/Rasm.AppHost/.planning/Wire/outbox.md` — both `CdcEnvelope` mentions re-spell to the `Rasm.Persistence` `Version/egress.md` `Egress.Envelope` projection.
- Unlocks: the decoded-never-re-minted law anchors to a real member.
- Anchors: Persistence `Version/egress.md` `Egress.Envelope`; the `ONE_OUTBOX_EGRESS_SPINE` three-consumer law.
- Ripple: mirrors `Rasm.Persistence` `[CDC_ENVELOPE_SPELLING]`.
- Atomic: two spellings.

[COORDINATION_CITATION_FIX]-[QUEUED]: Drop the dangling membership-law card citation from the coordination page.
- Capability: the no-second-membership-owner law stands on its own page authority with no reference to a card that does not exist.
- Shape: `libs/csharp/Rasm.AppHost/.planning/Wire/coordination.md` — the "(the IDEAS card's no-Orleans, no-Consul law)" parenthetical deletes; the surrounding clause already carries fact and why.
- Unlocks: the membership law reads as timeless page law.
- Anchors: the page's own deleted-forms clause; the docgen coupling law.
- Atomic: one parenthetical deletion.

[MQTT_RECEIVE_ARM]-[BLOCKED]: MQTT's receive adapter joins the settled carrier continuation and publish arms.
- Capability: the receive direction completes the MQTT carrier adapter over exact user-property members; drives from IDEAS `[WIRE_CARRIER_ADAPTERS]`.
- Shape: the receive adapter row beside the settled publish adapter on the AppHost wire plane.
- Unlocks: IDEAS.md [WIRE_CARRIER_ADAPTERS] — bidirectional MQTT crossings under the one carrier law.
- Anchors: the settled generic carrier continuation and MQTT publish arms; `libs/csharp/Rasm.AppHost/.api/api-mqtt.md`; `libs/csharp/.api/`.
- Arms: either catalog carries the exact user-property collection and entry accessors.

[OFFLINE_RETRY_BINDING]-[BLOCKED]: Per-signal disk-retry binding lands on the exporter owners.
- Capability: an `OfflineQueue`-backed retry binding holds each signal through disconnection; drives from IDEAS `[TELEMETRY_OFFLINE_SPINE]`.
- Shape: the retry binding rows on the AppHost telemetry exporter owners.
- Unlocks: IDEAS.md [TELEMETRY_OFFLINE_SPINE] — lossless telemetry from disconnected sites.
- Anchors: `libs/csharp/Rasm.AppHost/.api/api-otel-exporter.md`; `libs/csharp/.api/api-opentelemetry-exporter-otlp.md`.
- Arms: either catalog carries an exact per-signal disk-retry member or environment binding and its `FileBlobProvider` composition law.

[CLAIM_FAMILY_PROJECTIONS]-[BLOCKED]: Materials and Grasshopper claim projections complete the corpus-gate set.
- Capability: the two remaining producers project claims through the gate the settled kernel, Bim, Persistence, and Rhino projections ride; drives from IDEAS `[CORPUS_GATE_INGEST]`.
- Shape: the Materials and Grasshopper projections routed by `Observability/benchmarks.md#[CLAIM_FAMILY_ADMISSION]`.
- Unlocks: IDEAS.md [CORPUS_GATE_INGEST] — estate-wide claims grade through one corpus gate.
- Anchors: the settled kernel, Bim, Persistence, and Rhino projections; `Observability/benchmarks.md#[CLAIM_FAMILY_ADMISSION]`.
- Arms: both owner routes in `Observability/benchmarks.md#[CLAIM_FAMILY_ADMISSION]` close.

[PROFILE_SAMPLE_CAPTURE_ARM]-[BLOCKED]: Profile sample capture lands over exact SampleProfiler members.
- Capability: the capture arm joins the settled feed and registration rows, completing the profile-sample pipeline.
- Shape: the capture rows routed by `Observability/benchmarks.md#[PROFILE_SAMPLE_CAPTURE]`.
- Unlocks: span-correlated sample profiles on the benchmark evidence rail.
- Anchors: the settled feed and registration rows; `libs/csharp/Rasm.AppHost/.api/api-traceevent.md`; `libs/csharp/.api/`.
- Arms: either catalog carries the exact SampleProfiler subscription and stack/frame accessors routed by `Observability/benchmarks.md#[PROFILE_SAMPLE_CAPTURE]`.

[HLC_HEADER_KEY_MINT]-[QUEUED]: Correlation spine mints the HLC carrier-header key contract every cross-runtime decoder proves against.
- Capability: one host-owned spelling authority for the causal-stamp carrier keys — the wire's header vocabulary becomes a minted contract row set, so a decoder proves its mirror at boot instead of hand-mirroring spellings a host rename silently defaults to zeroed stamps.
- Shape: `libs/csharp/Rasm.AppHost/.planning/Observability/telemetry.md` `[03]-[CORRELATION_SPINE]` — the HLC stamp's carrier header keys spelled as contract rows beside the `TraceContext` `traceparent`/`tracestate` fold.
- Unlocks: the python companion's boot gate arms — its `SLOTS` mirror proves against the minted keys and a silently-defaulted causal-order fork dies at daemon boot.
- Anchors: `Correlation`/`TraceContext` on the `[CORRELATION_SPINE]` owner; python `SLOTS` and `CausalFrame.decode` at `libs/python/runtime/.planning/clock/clock.md`; the cross-libs single-writer measure-authority ruling.
- Ripple: `python runtime` `[HLC_HEADER_DRIFT_GATE]` — precedes.
- Atomic: one contract row set on one owner section.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[RESOURCE_DETECTOR_ROWS]-[COMPLETE]: Five contrib detector rows landed as `ResourceIdentity.Compose` on `Observability/telemetry.md#SIGNAL_GOVERNANCE` — host/os/process/runtime always-on, container gated on the containerized profile rows.
[HTTP_TELEMETRY_ROWS]-[COMPLETE]: `AddHttpClientLatencyTelemetry` and `AddExtendedHttpClientLogging` rows landed in `SignalGovernance.EnrichContext` with the four `*DataClasses` taxonomy maps.
[PYROSCOPE_AGENT_ROWS]-[COMPLETE]: Pyroscope agent rows landed on `Observability/benchmarks.md#PROFILE_CORRELATION` — `ProfileTracking` toggle policy, `ProfileLabels.Scoped` bounded label frames over `LabelSet.BuildUpon`/`LabelsWrapper.Do`.
[BAGGAGE_PROCESSOR_ROWS]-[COMPLETE]: `AddBaggageActivityProcessor(PromotedBaggage)` rows landed at both provider owners with the allowlist policy row.
[ROLL_ANNOTATION_WIRE]-[COMPLETE]: `RollAnnotationWire` record and per-wave fold landed on `Sandbox/provisioning.md#ROLLOVER_DRAIN`, fanned under `InstrumentFan.RollKind`, `[JsonSerializable]` row on `AppHostWireContext`.
[DUMP_TRIAGE_FOLD]-[COMPLETE]: `DumpTriage` fold landed on `Observability/bundles.md#CAPTURE_PIPELINE` — verified ClrMD members, `SupportArtifact.DumpAnalysis` row, `DumpPolicy.CensusCap`/`TriageRows`/`FrameCap` bounds, and shallow heap-sample semantics.
[SCHEMA_PINNED_MINT]-[COMPLETE]: Schema-pinned mint landed — `TelemetryContributorPort.SchemaUrl` stamped as `MeterOptions.TelemetrySchemaUrl` at `TelemetryIdentity.Mint` and `InstrumentFan.Mount`.
[MACHINE_OBSERVATION_DECODE]-[COMPLETE]: Observation decode rows landed as `MachineLane`/`MachineObservationWire` on `Wire/livewire.md` under `InstrumentFan.ObservationKind`; the crossing rides the receipt stream and the `MachineObservation` domain vocabulary is Fabrication-owned — no eighth port.
[OBSERVABILITY_PAGES_AUTHORED]-[COMPLETE]: Instruments, hooks, and benchmarks pages authored under `.planning/Observability/`; `InstrumentRow` widened to carry unit, description, and bind; `FaultBand` gained the `Hook` and `Benchmark` rows; `BenchmarkReceipt` joined the `AppHostWireContext` roster.
[ASPNET_GRPC_ADMISSIONS]-[COMPLETE]: `OpenTelemetry.Instrumentation.AspNetCore` and `OpenTelemetry.Instrumentation.GrpcNetClient` admitted — csproj rows, README registry rows, `.api` catalogs, and the `SignalGovernance` gRPC-client registration row landed.
[PARTITIONED_LEVEL_ROWS]-[COMPLETE]: keyed `LevelCells` with `Reader` and the tagged `Measurement<long>` multi-measurement projection landed at the kernel owner `libs/csharp/Rasm/.planning/Domain/telemetry.md` `[03]-[INSTRUMENT_MECHANISM]`; `Measurement<T>` catalogued at `libs/csharp/.api/api-diagnostics-metrics.md`.
[CONTRIBUTOR_ROSTER_ARMS]-[COMPLETE]: all ten contributor rows landed in the `[CONTRIBUTED_ARMS]` table at `Observability/instruments.md` `[03]-[RECEIPT_PROJECTION]`.
