# [APPHOST_IDEAS]

Forward pool of higher-order concepts for the runtime spine, each grounded in the folder's domain and current platform capability — some are new sub-domain folders that deepen a thin owner, others bind a concrete admitted package to a settled abstract surface. Open ideas drive the tasks in `TASKLOG.md`; a finished or dropped idea moves to `[2]-[CLOSED]` with a one-line disposition so it is never re-litigated.

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

[CLASSIFICATION_TAXONOMY_OWNER]-[QUEUED]: One owner holds the classification row set the estate redacts on, so three branch-local vocabularies stop sharing one taxonomy name with disjoint rows.
- Capability: Classification collapses to one owned row set whose every row carries its own redaction verdict, so a marker minted at any branch resolves against one fail-closed redactor map rather than three private vocabularies colliding on one taxonomy string.
- Shape: `libs/csharp/Rasm.AppHost/.planning/Observability/telemetry.md:1090` declares `DataClassification`, nine rows, a `[SmartEnum<string>]` carrying `RedactorKind` and projecting `Marker`; `libs/csharp/Rasm.Fabrication/.planning/Process/telemetry.md:653` declares `FabricationClassified`, three framework values under a `"DataClassification"` const; `libs/csharp/Rasm.Rhino/.planning/Objects/authoring.md:74` declares `HostSensitivity`, four framework values under `nameof(DataClassification)`.
- Unlocks: Redaction derives from the row itself at every branch, so a newly classified field annotates and lands its verdict with no app-root map edit, and a row outside the owner's set fails closed instead of crossing unredacted.
- Anchors: `Microsoft.Extensions.Compliance.Classification.DataClassification`/`DataClassificationAttribute` are the framework carriers all three spell; `RedactorKind` is the sole verdict column among them; `HostSensitivity` already spells `HostPath` and `UserContent` onto one `user-content` value, so row identity drifted inside a single declaration.
- Tension: `Rasm.Fabrication` rules its rows VALUE FEDERATION — `(taxonomy, value)` string pairs crossing no type reference — so the collapse question is whether federation stands estate-wide and what then owns the ROW SET all three must agree on, since a value the owner never declares redacts nowhere.

## [02]-[CLOSED]

<!-- source-only: closed idea card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[WIRE_CARRIER_ADAPTERS]-[COMPLETE]: `api-mqtt.md` gained the v5 user-property read scope — `MqttApplicationMessage.UserProperties`, `MqttUserProperty.Name`/`ValueBuffer`, and `MqttUserPropertyExtensions.ReadValueAsString`, the buffer pair the package's own `[Obsolete]` notes point at — so `TraceContext` carries an `MqttApplicationMessage` `Continue` overload beside its builder `Inject` and the `MqttRuntime.Properties` composition delegate deleted with the unknown it existed to defer.
[CORPUS_GATE_INGEST]-[COMPLETE]: `Observability/benchmarks.md` `[05]-[CLAIM_FIELD_MAP]` closes over all nine families — Materials `BenchWorkload` stood already and the two Grasshopper breach rows landed once `CaptureBreach` carried its producing bound — and that page's `[RESEARCH]` section reads `(none)`, so every producer grades through one verdict grammar.
[TELEMETRY_OFFLINE_SPINE]-[COMPLETE]: the durable spine landed as branch-owned TRANSPORT rather than exporter persistence — `OtlpOfflinePolicy.For` opens one `OtlpOfflineQueue` per exported signal at composition and `PersistentOtlpHandler` swaps a durable signal's transport, replaying the tail through the next proved request under the governance queue, arming, and drain bounds.
[TENANT_COST_SPAN_PROMOTION]-[COMPLETE]: `AddBaggageActivityProcessor(PromotedBaggage)` allowlist rows landed at both provider owners with the governance-table policy row.
[FLEET_DEPLOY_ANNOTATIONS]-[COMPLETE]: `RollAnnotationWire` record and per-wave projection fold landed on `Sandbox/provisioning#ROLLOVER_DRAIN`, fanned under `InstrumentFan.RollKind` and joined to the `AppHostWireContext` roster.
[DIAGNOSTIC_HEAP_ANALYSIS]-[COMPLETE]: `DumpTriage.Walk` ClrMD fold and the `SupportArtifact.DumpAnalysis` factory row landed on `Observability/bundles#CAPTURE_PIPELINE` under `DumpPolicy.CensusCap`/`TriageRows`/`FrameCap` bounds with shallow heap-sample semantics.
[MACHINE_OBSERVATION_LANE]-[COMPLETE]: `MachineLane`/`MachineObservationWire` decode lane landed on `Wire/livewire.md` fanned under `InstrumentFan.ObservationKind`; the `MachineObservation` domain vocabulary is Fabrication-owned per its observation card.
[DOMAIN_INSTRUMENT_CATALOG]-[COMPLETE]: Domain-instrument catalog over the receipt fan — landed as `.planning/Observability/instruments.md` with the roster, projection fold, per-ALC provider capsule, and observation rail.
[TYPED_HOOK_RAIL]-[COMPLETE]: Typed hook rail over the existing bus, lifecycle, and receipt seams — landed as `.planning/Observability/hooks.md` with id grammar, modality rows, registry, and fault isolation.
[BENCHMARK_RECEIPT_FAMILY]-[COMPLETE]: Benchmark receipt family and corpus-gate ownership — landed as `.planning/Observability/benchmarks.md` with the gate fold, bundle capture seam, and span-profile correlation.
[PARTITIONED_OBSERVABLE_LEVELS]-[COMPLETE]: realized kernel-side per the substrate-homing collapse — keyed `LevelCells` families with `Reader` project per-key levels as tagged `Measurement<long>` rows through the multi-measurement observe overload at `libs/csharp/Rasm/.planning/Domain/telemetry.md` `[04]-[INSTRUMENT_MECHANISM]`, so a bespoke per-partition gauge is the deleted form.
[CONTRIBUTED_ARM_ROSTER]-[COMPLETE]: the `[CONTRIBUTED_ARMS]` table landed at `Observability/instruments.md` `[03]-[RECEIPT_PROJECTION]` with all ten contributor rows — each domain package's projection, port mint, and custody named.
