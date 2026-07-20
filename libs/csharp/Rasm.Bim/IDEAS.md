# [RASM_BIM_IDEAS]

Forward pool of higher-order concepts for the host-neutral BIM-and-exchange engine. `[1]-[OPEN]` holds active ideas as cards; `[2]-[CLOSED]` records a finished or dropped idea with a one-line disposition.

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

[CONNECTION_INTERFACE_GEOMETRY_DECODE]-[QUEUED]: Land the Bim lowering and re-materialization for the seam-landed `Connect.Interface` content key.
- Capability: `IfcConnectionGeometry` and `IfcRelSpaceBoundary2ndLevel` interface surfaces ride the graph as content-keyed typed geometry instead of stranding in `Generic` attributes.
- Shape: the `Projection/relations` projector hashes the interface surface into the blob store and stamps the key on the `Connect` edge (`IfcRelConnectsElements.ConnectionGeometry` and the 2nd-level space-boundary route off `Generic`); the `Projection/egress` re-materializes it; the landed seam decodes the interface key through the one `GeometrySource.ResolveFootprint` leg — never a third port.
- Unlocks: Compute reads connection-interface geometry one-hop by content key; re-exported analysis models keep their boundary surfaces.
- Anchors: `Connect.Interface` landed seam-side (presence-delimited canonical bytes, additive `ConnectWire` field); the egress eccentricity path already reconstitutes `IfcConnectionGeometry` STEP fragments from the ctor-held profiles store — the same lane.
- Ripple: `Rasm.Element` `[CONNECTION_INTERFACE_GEOMETRY_DECODE]`.

[QUANTITY_BAG_GROUP_AXIS]-[QUEUED]: Round-trip the `IfcPhysicalComplexQuantity` `Discrimination`/`Quality`/`Usage` grouping identity once the seam `QuantityBag` carries a group axis.
- Capability: complex-quantity grouping identity survives the dot-path flatten — a formwork quantity grouped by `Discrimination` re-emits with its grouping strings instead of prefix-only reconstruction.
- Shape: the `Projection/semantic` flatten stamps the group-axis rows the seam carrier adds; the `Projection/egress` `RaiseQuantity` rebuilds nested `IfcPhysicalComplexQuantity` children from them.
- Unlocks: value-lossless AND identity-lossless complex-quantity round-trip; QTO consumers select by grouping axis.
- Anchors: `Projection/semantic` `FlattenQuantities` already recurses dot-path keys value-lossless and names the residual; the decompile-verified `Discrimination`/`Quality`/`Usage` public strings.
- Tension: the seam column ripples the counted-bag canonical-bytes injectivity law, the frozen wire, and the `Bake` merge — the seam owner lands first.
- Ripple: `Rasm.Element` `[QUANTITY_BAG_GROUP_AXIS]`.

[BCF_API_RESPONSE_ADMISSION]-[QUEUED]: Close the BCF-API round-trip with the response half of the REST projection.
- Capability: a BCF-API response admits back into the archive-domain family — status/header fold, pagination cursor, and the response-body lowering onto `BcfTopic`/`BcfComment`/`BcfViewpoint` — so a CDE round-trip reads one owner.
- Shape: `Review/issues` gains the response peer of `BcfResource` → `BcfApiRequest`: one response carrier discriminated by resource, the snake-case body admission reusing `BcfApiContext`, the paged-collection fold; execution stays on the Compute transport.
- Unlocks: live CDE topic sync onto `IssueBoard`, server-authored viewpoints landing beside `.bcfzip` imports, conflict-aware `ReviseTopic` round-trips.
- Anchors: `BcfResource`/`BcfApiRequest`/`BcfApiContext` landed; `BcfWireMapper` owns the archive↔wire correspondence the response admission reuses.
- Tension: the transport port and retry/auth policy are Compute's — the response admission consumes returned bytes and never mints a second transport owner.

[BRICK_SYSTEMS_OPERATIONS_OVERLAY]-[BLOCKED]: Compose the admitted `BrickSchema.Net` ontology as the building-systems-operations overlay on `Model/systems`, once the app-platform live-binding leg lands.
- Capability: a Brick `Point`/`Equipment`/`Location` graph overlays the static `SystemTrace` connectivity, mapping IFC `MonitoringSystem` occurrences onto Brick `Fedby`/`PointOf`/`PartOf`/`LocationOf` relations so operations consumers read one semantic systems model.
- Shape: `Model/systems` gains a Brick projector lowering the `DistributionSystem` reach set onto Brick classes; the live-point binding (`BACnetReference`/`BACnetDevice`/`ModbusDevice`) stays a reference the app-platform resolves, never a transport minted here.
- Unlocks: BMS-aware clash and coordination, live-versus-design systems reconciliation, operations-phase handover beyond COBie.
- Anchors: `BrickSchema.Net` admitted (README `[DOMAIN_VOCABULARY]`, `.api/api-brickschema-net.md` — `Aggregation.AggregateByInterval` the interval-rollup read the operations overlay exposes); `Model/systems` owns the static `SystemTrace` connectivity the overlay reads.
- Tension: which app-platform owner lands the live-point binding resolver (`BACnetReference`/`BACnetDevice`/`ModbusDevice` rows) the overlay references? Route: `libs/csharp/Rasm.AppHost/.planning/` capability pages and the AppHost growth register — Bim owns only the static ontology projection, the overlay reads `SystemTrace`, never re-minting a second connectivity store beside it.

[BIM_HOOK_RAIL]-[QUEUED]: Mint the `rasm.bim.<domain>.<point>` typed hook registry so every long-running Bim rail exposes veto/observe/replay points with zero emit calls in domain code.
- Capability: composition-scoped hook points over the import/export codecs, the semantic projection, the legality gate, the review verdicts, and the energy translators — modalities veto/observe/replay, subscriber faults isolated onto the `BimFault` rail, telemetry-as-tap so observers subscribe to domain facts.
- Shape: `libs/csharp/Rasm.Bim/.planning/Model/observability.md` (new page) owns a closed point roster keyed `rasm.bim.<domain>.<point>`, a modality union, and the registry record an app composes PER INSTANCE — no process-global registry, so two apps built on the library never fight over hook slots; progress points wrap `ICadReader.OnProgress` (ACadSharp DWG decode) and the OpenStudio translator `ProgressBar` callback as observe-modality facts on the import and energy rails.
- Unlocks: cancellable long imports, UI progress without codec coupling, replayable review pipelines, `[BIM_TELEMETRY_TAP]` as a registry subscriber.
- Anchors: `Model/faults#FAULT_BAND` `BimFault` the subscriber-fault isolation target; `.api/api-acadsharp.md` `ICadReader.OnProgress`; `.api/api-openstudio.md` `ProgressBar`; `csharp:Rasm.AppHost/Observability/instruments.md` the branch registry pattern the point roster mirrors.

[BIM_TELEMETRY_TAP]-[QUEUED]: Project every Bim receipt and fault onto `rasm.bim.<domain>.<measure>` instruments through one meter owner carrying zero OTel reference.
- Capability: receipts stay billing truth; instruments are the lossy dashboard channel — import/export durations and byte folds, projection node/edge counts, legality rejects banded by `Category`, review verdict tallies, energy exchange counts — an `ActivitySource` span law attributing traces and span-profiles to Bim operations, and a baggage-sourced tenant/model tag law attributing every instrument row.
- Shape: `libs/csharp/Rasm.Bim/.planning/Model/observability.md` gains the instrument roster (name, kind, UCUM unit, tag set, receipt source) and one meter owner constructed over injected `IMeterFactory.Create(MeterOptions)` — histograms through `Meter.CreateHistogram<T>(name, unit, description, tags, advice)` with explicit-bound advice fallback, scope the package id; the owner subscribes to `[BIM_HOOK_RAIL]` points, never an emit call inside a projector; spans mint through one `ActivitySource` with the kernel `Op` as the span-name source.
- Unlocks: Bim tiles on estate dashboards, span-profile correlation at the AppHost Pyroscope composition, per-tenant/model cost attribution over BIM workloads.
- Anchors: `libs/csharp/.api/api-diagnostics-metrics.md` `IMeterFactory.Create(MeterOptions)` + `Meter.CreateHistogram<T>` advice member; `Model/faults#FAULT_BAND` `error.Category()` telemetry banding; typed receipts (`EnergyReceipt`, `TessellationOutcome`, `EarnedValueReport`, `ModelHealth`) as fact sources; `csharp:Rasm.AppHost/Observability/instruments.md` `InstrumentFan` merging the contributed arm.
- Tension: library altitude forbids any OTel package reference — BCL `System.Diagnostics.Metrics` + `ActivitySource` only; SDK composition, exporters, and exemplars stay AppHost's.

[BIM_EVENT_FABRIC]-[QUEUED]: Mint the `BimEvent` domain-fact union and its CloudEvents envelope so model mutations travel any transport with trace continuity.
- Capability: commit landed, issue-board mutation, validation verdict, export artifact minted, energy artifact minted — one closed event family carrying content keys and GlobalId sets, never payload bytes; envelope projection stamps the CloudEvents distributed-tracing extension so W3C context rides brokers end to end.
- Shape: `libs/csharp/Rasm.Bim/.planning/Exchange/events.md` (new page) owns the `[Union]` `BimEvent` and the envelope projection over `CloudNative.CloudEvents` + `CloudNative.CloudEvents.SystemTextJson` — type `rasm.bim.<domain>.<fact>`, source the service instance, subject the content key; transport bindings (Kafka/MQTT/NATS) stay app-tier composition, Bim owns payload and envelope only.
- Unlocks: CDE webhooks, cross-runtime model-sync notification to the Python and TypeScript peers, outbox rows the Persistence tier stores, event-driven review pipelines.
- Anchors: `Review/versioning#VERSION_GRAPH` `CommitKey`; `Review/issues#BCF_ARCHIVE` board mutations; `Exchange/export#EXPORT_RAIL` artifact receipts; `Energy/exchange#ENERGY_EXCHANGE` content keys; CloudEvents packages admitted in the central manifest.

[PROGRESS_VERIFICATION]-[QUEUED]: Close the 4D loop — scan-derived physical progress verifies the schedule and feeds earned value with observed actuals.
- Capability: a reconstructed point-cloud epoch compares against the `ConstructionState` expectation at the capture instant — observed installed elements against planned — minting a typed progress-evidence receipt per task with observed completion, variance band, and the unmatched-occurrence residue.
- Shape: `libs/csharp/Rasm.Bim/.planning/Planning/progress.md` (new page) owns the comparison fold joining `Exchange/reconstruct#RECONSTRUCTION` occurrences to `Planning/schedule#SCHEDULE` `TaskAssignment` element sets through the `Model/query#ELEMENT_SET` predicate algebra; observed completion feeds the `Planning/cost#EARNED_VALUE` fold as the actuals source beside authored `IfcTaskTime.Completion`.
- Unlocks: reality-capture progress dashboards, evidence-backed earned value, dispute-grade progress records keyed by the capture content key.
- Anchors: `ConstructionState.At` phase reads (`Completed` the finished-by read); `ConstructionTask.PercentComplete` fallback law; scan-to-BIM occurrences carrying per-occurrence fit receipts.

[ENERGY_RESULTS_ANNOTATION]-[QUEUED]: Land simulation results back on the model — Compute-read EnergyPlus outputs annotate zones and spaces as typed result quantities.
- Capability: Compute's typed energy-results receipt (keyed by the `EnergyArtifact` content key) admits onto `Model/zones` and spatial nodes as result quantities — annual and peak loads, comfort hours, EUI — re-emittable as Psets through the standing properties authority, so results survive re-export instead of dying in a run directory.
- Shape: `libs/csharp/Rasm.Bim/.planning/Energy/results.md` (new page) owns the admission fold from the Compute receipt onto zone/space quantity rows and the reverse read the AppUi report consumes; `SqlFile` decode stays Compute's per the standing simulation ruling — Bim consumes the receipt, never the SQLite.
- Unlocks: results-aware model QA (IDS facets over result thresholds), energy dashboards read from the model, results round-trip into IFC Psets.
- Anchors: `Energy/exchange#ENERGY_EXCHANGE` content-keyed artifacts; `Semantics/properties#PROPERTY_TEMPLATES` Pset authority; `Model/zones#ZONE_GRAPH` overlay; `.api/api-openstudio.md` results-seam row naming `SqlFile` as Compute's reader.

[BIM_BENCH_RECEIPTS]-[QUEUED]: Stand a `BimBenchReceipt` family so codec, projection, and query performance claims are typed evidence gated by the estate corpus.
- Capability: per-operation benchmark claims — import decode per format, egress re-author, `ElementSet.Query` over graph scales, geospatial vector/raster ingest, tessellation round-trip — each a typed receipt carrying op, corpus fingerprint, and duration/allocation distributions, admitted through the corpus gate rather than prose numbers.
- Shape: `libs/csharp/Rasm.Bim/.planning/Model/observability.md` gains the receipt family and the per-op claim roster; harness wiring rides the estate BenchmarkDotNet corpus gate at the tests estate, never a per-folder runner.
- Unlocks: regression-proof codec changes, size-scaled query-planning evidence, `StorePlan` push-down against in-process fold comparisons.
- Anchors: `csharp:Rasm.AppHost/Observability/benchmarks.md` `BenchmarkReceipt` family shape; import/export/query receipts already typed.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

[TEMPLATE_AUDIT_VALIDATION_TIER]-[COMPLETE]: Ruled — `Review/validation` widened to the two-tier QA owner: `ModelHealth`/`ModelFinding` compose the `TemplateFinding` stream as the baseline tier beneath authored IDS, the case the tier discriminant, one verdict surface for `Rasm.AppUi` and the review pipeline.

[UNIT_SCHEME_BIM_COUNTERPART]-[COMPLETE]: `HeaderWire.unit_scheme = 7` is landed on the seam `Graph/wire` with both Mapper legs, and the Bim ingest (`UnitsOf`) and egress (`DeclareUnits`) ends read the field — the `Rasm.Element` counterpart card closed with it.
