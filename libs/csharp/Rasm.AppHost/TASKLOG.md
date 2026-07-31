# [APPHOST_TASKLOG]

Open and closed work for the runtime spine, distilled from `IDEAS.md`. Each open task carries a status marker and the capability-to-build, packages, integration points/boundaries, and key considerations; one idea spawns one or more tasks across one or more files. Closed cards record already-settled cleanup.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with. `Atomic` flags a minor-scope task so a later session sizes its turn correctly and does not overscope a batch of small items.

## [01]-[OPEN]

<!-- source-only: open task card template:
[ID]-[STATUS]: <ambitious concise thesis — the capability outcome, never the landing motion>.
- Capability: <the higher-order invariant, owner capability, or concept established — concept grain only, never a page path, row list, or member spelling>.
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


[HLC_HEADER_KEY_MINT]-[QUEUED]: Correlation spine mints the HLC carrier-header key contract every cross-runtime decoder proves against.
- Capability: one host-owned spelling authority for the causal-stamp carrier keys — the wire's header vocabulary becomes a minted contract row set, so a decoder proves its mirror at boot instead of hand-mirroring spellings a host rename silently defaults to zeroed stamps.
- Shape: `libs/csharp/Rasm.AppHost/.planning/Observability/telemetry.md` `[03]-[CORRELATION_SPINE]` — the HLC stamp's carrier header keys spelled as contract rows beside the `TraceContext` `traceparent`/`tracestate` fold.
- Unlocks: Python runtime's boot gate proves its `SLOTS` binding against the shared keys before causal admission.
- Anchors: `Correlation`/`TraceContext` on the `[CORRELATION_SPINE]` owner; python `SLOTS` and `CausalFrame.decode` at `libs/python/runtime/.planning/clock/clock.md`; the cross-libs single-writer measure-authority ruling.
- Ripple: `python runtime` `[HLC_HEADER_DRIFT_GATE]` — precedes.
- Atomic: one contract row set on one owner section.

[VIEW_TAGKEYS_FROM_ROWS]-[QUEUED]: Signal governance derives every metric view's tag-key set from the declared instrument rows it governs.
- Capability: view shaping stops re-spelling a dimension vocabulary the instrument declaration already carries, so a tag rename lands once at its owner and a view can never cap a dimension no arm stamps.
- Shape: `libs/csharp/Rasm.AppHost/.planning/Observability/telemetry.md` `[05]-[SIGNAL_GOVERNANCE]` — `Views` folds contributor-port rows into per-instrument `MetricStreamConfiguration` values instead of holding a hand-spelled `TagKeys` roster beside the wildcard cap.
- Unlocks: every contributed roster inherits governed cardinality on admission, so a sibling package's new dimension caps without a governance edit.
- Anchors: `InstrumentSpec.Dimensions` and `InstrumentSet.Rows` on the kernel capsule; `HostInstruments` slot consts at `Observability/instruments.md` `[02]-[INSTRUMENT_CATALOG]`; `MetricStreamConfiguration.TagKeys`/`CardinalityLimit` at `libs/csharp/.api/api-opentelemetry.md`.
- Tension: `Govern` builds providers before any meter mints, so the fold reads the contributor ports rather than a mounted `InstrumentSet`, and the wildcard cap stays the floor for foreign rows carrying no declaration.
- Atomic: one fold on one governance owner.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[GOVERNANCE_DECORATOR_ORDER]-[COMPLETE]: `ModelGovernance.Compose` re-seated its chain so decorator ORDER carries the policy — routing moved onto the catalogued `ConfigureOptions` decorator ABOVE `UseDistributedCache` (a rewrite below it left `GetCacheKey` hashing an un-routed `ModelId`, colliding every route on one entry and mis-crediting `CachedInputTokenCount`), `GoverningChatClient` collapsed to redaction alone below the cache so a replay is never re-tokenized by the non-idempotent HMAC redactor, `CacheKeyAdditionalValues` took the governance epoch and redaction key generation, and the embedding arm landed as a second `Compose` overload so `SemanticDiscovery` consumes a traced, cached generator.
[WINDOW_BOUNDED_HISTORY]-[COMPLETE]: `ModelRoute` gained its `Window` column and `ReasoningPolicy`/`GovernanceRuntime` their `WindowShare`, with `WindowReducer` measuring the live conversation through the one composition-built `TiktokenTokenizer` and delegating the overflow head to the shipped `SummarizingChatReducer` at the measured retention — the reducer's own knobs are MESSAGE counts no route window expresses, so the measurement is the branch's and the summarization the package's.
[BROKERED_INVOKER_HOOK]-[COMPLETE]: `FunctionInvokingChatClient.FunctionInvoker` bound as `BrokeredInvoker.Invoke`, closing the `[TOOL_RECEIPT_CARRIER]` research row — the hook is a public settable property on the same configure lambda, so the row's premise that `FunctionInvocationServices` being `protected` forces a construction-time collector is moot, and the seam asserts the brokered function's `CommandReceipt` return rather than leaving a receiptless turn for `AsMacro` to blame.
[MODAL_IMAGE_COLLAPSE]-[COMPLETE]: the image modality collapsed onto the one governed pipeline — `UseImageGeneration` weaves under the `ModalKind.Image` gate and `ModalIntake.Intent` seats a `HostedImageGenerationTool`, deleting `ModalClient`, its `[Union]`, and the direct `IImageGenerator.GenerateAsync` draw that touched no span, cache, redaction, or broker while the page claimed it rode the meter.
[WASM_CAPSULE_MATERIALIZED]-[COMPLETE]: `Sandbox/isolation.md` gained the wasm vehicle it described — `WasmCapsule` beside `Option<CompanionPeer> Child`, `SandboxRows.Capsule` compiling `Module.FromBytes` over the admitted `PluginArtifact.Component` and folding the grant scope onto `Linker.DefineFunction` rows and the WASI pre-open set, `EpochPacer` advancing the engine counter so `SetEpochDeadline` is enforceable, `TrapDisposition` projecting `TrapCode.Interrupt`/`OutOfFuel` onto one quota vocabulary, and `QuotaControl.Observed` reading the `Store.Fuel` delta the enforcement fold took as an argument no caller had a source for.
[PLUGIN_INSTRUMENT_RULES]-[COMPLETE]: `PluginTelemetryHost.Open` folds `InstrumentRule` suppression rows through `AddMetrics(services, builder => …)` under `MeterScope.Local`, so a plugin ALC drops a cardinality-hostile stream without editing the contributor roster every host reads, and the enablement plane is named distinct from the view-shaping plane.
[REDACTION_SET_CLOSURE]-[COMPLETE]: `RedactionRegistration` derives every producible classification set — singletons and `Union` composites under a `RedactorKind.Rank` strongest-wins fold — because lookup keys on whole-set equality and an unregistered composite falls to the erasing fallback as silent data loss; `RedactorKind.None` rows now bind `NullRedactor` EXPLICITLY (unbound they erased every operational dimension), the `Unknown` row declares the never-reviewed arm, and `RedactedText` lands `TryRedact`/`AppendRedacted` at the egress read seam.
[LOG_VOLUME_PLANE]-[COMPLETE]: `Observability/telemetry#LOG_PROJECTION` gained the two planes the rule rows cannot express — `SpineSampler : LoggingSampler` deriving its audit exclusion from the `FaultBand` EVENT-kind registry and thinning the floor against the live `DegradationLevel`, and `IncidentBuffers` seating the global ring beside a RESOLVED `PerRequestLogBuffer` whose activation verb ships with the unadmitted ASP.NET middleware, flushed on a failed outcome.
[BENCH_SPREAD_COLUMN]-[COMPLETE]: `BenchMeasurement`/`BenchmarkReceipt` gained `Duration Iqr` off `SortedArrayStatistics.InterquartileRange(sorted)` — one extra O(1) read on the already-sorted sample, evidence rather than a gate input, so a run whose median holds while its spread doubles stops reading as unchanged.
[MQTT_RECEIVE_ARM]-[COMPLETE]: `TraceContext.Continue(ActivitySource, MqttApplicationMessage, string, TenantAdoption)` landed at `Observability/telemetry#CORRELATION_SPINE` with an ordinal-matched getter decoding `MqttUserProperty.ValueBuffer` through `ReadValueAsString`; `Wire/livewire.md` `MqttRuntime` lost its `Properties` delegate and the receive pump calls the overload directly, consumer-kinded.
[CLAIM_FAMILY_PROJECTIONS]-[COMPLETE]: the Materials `BenchWorkload` row stood at `#CLAIM_FIELD_MAP` `[06]` already, and the Grasshopper half landed as rows `[08]` `BudgetBreach` and `[09]` `CaptureBreach` under the single-sample projection law once `Platform/capture.md` carried the producing bound.
[PROFILE_SAMPLE_CAPTURE_ARM]-[COMPLETE]: `ProfileCapture.Bind` landed at `Observability/benchmarks.md` `[04]-[PROFILE_CORRELATION]` over the verified `SampleProfilerTraceEventParser.ThreadSample`/`.ThreadStackWalk` pair and `ClrThreadStackWalkTraceData.FrameCount`/`.InstructionPointer`, with `ProfileCapturePolicy` bounds, the `ProfileFrameForm` symbolization posture, and the `ProfileCaptureReceipt` disposition counts.
[OFFLINE_RETRY_BINDING]-[COMPLETE]: realized ahead of the card as the branch-owned durable OTLP transport — `OtlpOfflinePolicy`/`OtlpOfflineQueue`/`PersistentOtlpHandler` on `Observability/telemetry.md` open one queue per exported signal and swap that signal's transport, under the AppHost `RULINGS.md` transport-not-exporter-persistence row.
[CONSUMPTION_AXIS_SPELLING]-[COMPLETE]: the six-axis roster landed at all three branch minters with identical closed-axis vocabularies and one common open-axis descriptor shape; refusal is one axis/value/reason grammar everywhere, and the corpus entry's roster blocker is discharged.
[RESOURCE_DETECTOR_ROWS]-[COMPLETE]: Five contrib detector rows landed as `ResourceIdentity.Compose` on `Observability/telemetry#SIGNAL_GOVERNANCE` — host/os/process/runtime always-on, container gated on the OCI-vehicle rows.
[HTTP_TELEMETRY_ROWS]-[COMPLETE]: `AddHttpClientLatencyTelemetry` and `AddExtendedHttpClientLogging` rows landed in `SignalGovernance.EnrichContext` with the four `*DataClasses` taxonomy maps.
[PYROSCOPE_AGENT_ROWS]-[COMPLETE]: Pyroscope agent rows landed on `Observability/benchmarks#PROFILE_CORRELATION` — `ProfileTracking` toggle policy, `ProfileLabels.Scoped` bounded label frames over `LabelSet.BuildUpon`/`LabelsWrapper.Do`.
[BAGGAGE_PROCESSOR_ROWS]-[COMPLETE]: `AddBaggageActivityProcessor(PromotedBaggage)` rows landed at both provider owners with the allowlist policy row.
[ROLL_ANNOTATION_WIRE]-[COMPLETE]: `RollAnnotationWire` record and per-wave fold landed on `Sandbox/provisioning#ROLLOVER_DRAIN`, fanned under `InstrumentFan.RollKind`, `[JsonSerializable]` row on `AppHostWireContext`.
[DUMP_TRIAGE_FOLD]-[COMPLETE]: `DumpTriage` fold landed on `Observability/bundles#CAPTURE_PIPELINE` — verified ClrMD members, `SupportArtifact.DumpAnalysis` row, `DumpPolicy.CensusCap`/`TriageRows`/`FrameCap` bounds, and shallow heap-sample semantics.
[SCHEMA_PINNED_MINT]-[COMPLETE]: Schema-pinned mint landed — `TelemetryContributorPort.SchemaUrl` stamped as `MeterOptions.TelemetrySchemaUrl` at `TelemetryIdentity.Mint` and `InstrumentFan.Mount`.
[MACHINE_OBSERVATION_DECODE]-[COMPLETE]: Observation decode rows landed as `MachineLane`/`MachineObservationWire` on `Wire/livewire.md` under `InstrumentFan.ObservationKind`; the crossing rides the receipt stream and the `MachineObservation` domain vocabulary is Fabrication-owned — no eighth port.
[OBSERVABILITY_PAGES_AUTHORED]-[COMPLETE]: Instruments, hooks, and benchmarks pages authored under `.planning/Observability/`; every `HostInstruments` row declares its unit and description on the kernel `InstrumentSpec`, whose `InstrumentKind` x `MeasureForm` pair derives the bind; `FaultBand` gained the `Hook` and `Benchmark` rows; `BenchmarkReceipt` joined the `AppHostWireContext` roster.
[ASPNET_GRPC_ADMISSIONS]-[COMPLETE]: `OpenTelemetry.Instrumentation.AspNetCore` and `OpenTelemetry.Instrumentation.GrpcNetClient` admitted — csproj rows, README registry rows, `.api` catalogs, and the `SignalGovernance` gRPC-client registration row landed.
[PARTITIONED_LEVEL_ROWS]-[COMPLETE]: keyed `LevelCells` with `Reader` and the tagged `Measurement<long>` multi-measurement projection landed at the kernel owner `libs/csharp/Rasm/.planning/Domain/telemetry.md` `[04]-[INSTRUMENT_MECHANISM]`; `Measurement<T>` catalogued at `libs/csharp/.api/api-diagnostics-metrics.md`.
[CONTRIBUTOR_ROSTER_ARMS]-[COMPLETE]: all ten contributor rows landed in the `[CONTRIBUTED_ARMS]` table at `Observability/instruments.md` `[03]-[RECEIPT_PROJECTION]`.
[PORT_CARRIED_BOARD_PACK]-[COMPLETE]: `TelemetryContributorPort` gained `Option<BoardPack> Board` and its `Admit` fold at the kernel capsule, so `InstrumentFan.Mount` returns `Fin<ReceiptFan>` proving every contributed pack over the set it binds; the AppHost, Materials, Fabrication, and Compute ports carry their packs, the composition-root and registry-mount legs that named `HostInstruments.Board` by hand are gone, and objective-name distinctness folded into `BoardPack.Admit` so the Compute descriptor probe retired.
