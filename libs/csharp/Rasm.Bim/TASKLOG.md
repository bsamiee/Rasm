# [RASM_BIM_TASKLOG]

Open and closed work distilled from `IDEAS.md`. `[01]-[OPEN]` carries task cards with `[QUEUED]`, `[ACTIVE]`, or `[BLOCKED]` leaders; `[02]-[CLOSED]` carries `[COMPLETE]` or `[DROPPED]` cards. One idea spawns one or more tasks; each task names the exact sub-domain or file it lands in.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with. `Atomic` flags a minor-scope task so a later session sizes its turn correctly and does not overscope a batch of small items.

## [01]-[OPEN]

<!-- source-only: open task card template:
[ID]-[STATUS]: <ambitious concise thesis>.
- Capability: <higher-order concept, invariant, or owner capability>.
- Shape: <what the idea becomes as a system, product, owner, or feature set(s)>.
- Unlocks: <new branch, package, workflow, proof, user, or agent capability made possible>.
- Anchors: <owners, seams, packages, doctrines, or techniques that make the idea plausible>.
- Tension: <only when an unresolved constraint, boundary, bet, or dependency shapes the idea>.
- Ripple: <origin/counterpart card this entry pairs with across folders, as `pkg` `[SLUG]`; present only on a cross-folder ripple counterpart card>.
- Atomic: <present only on a minor-scope task; one short phrase naming the small unit so a later session does not overscope its turn>.
-->

[T1]-[BLOCKED]: `ElementSet` query algebra reaches the AppUi viewport as a declared `[PROJECTION]` seam.
- Capability: model-query results (`Model/query` `ElementSet`) rendered as AppUi viewport/inspector selections.
- Shape: one `Model/query -> csharp:Rasm.AppUi/<owner> # [PROJECTION]` seam row with the consuming AppUi page fence.
- Unlocks: saved-query overlays, selection-driven dashboards, query-scoped exports.
- Anchors: `Model/query` `ElementSet`; the AppUi `[V9]` growth register carries the reciprocal record.
- Tension: no AppUi page names a consumer today — the seam row re-enters `ARCHITECTURE.md` `[02]-[SEAMS]` only when one does; deferred pressure never rides the ledger.

[T2]-[BLOCKED]: `ScheduleNetwork` CPM/4D projection reaches the AppUi Charts plane as a declared `[PROJECTION]` seam.
- Capability: 4D construction-sequencing and critical-path dashboards over the `Planning/schedule` domain.
- Shape: one `Planning/schedule -> csharp:Rasm.AppUi/Charts # [PROJECTION]` seam row with the consuming dashboards fence.
- Unlocks: 4D playback tiles, earned-value overlays beside the existing `Planning/cost` receipt row.
- Anchors: `Planning/schedule` `ScheduleNetwork`; the AppUi `[V9]` growth register carries the reciprocal record.
- Tension: no AppUi consuming fence exists today — same re-entry law as `[T1]`.

[T3]-[QUEUED]: Author the hook-point vocabulary and composition-scoped registry on the new observability page.
- Capability: closed point roster keyed `rasm.bim.<domain>.<point>`, a veto/observe/replay modality union, the per-composition registry record, and the subscriber-fault isolation law onto `BimFault`.
- Shape: `libs/csharp/Rasm.Bim/.planning/Model/observability.md` gains a `[HOOK_RAIL]` section with owner/cases/entry/receipt rows and one signature fence; emitting pages cite points by roster row, never a local hook type.
- Unlocks: `[T4]` progress wiring and `[T5]` instrument subscription.
- Anchors: `IDEAS.md` `[BIM_HOOK_RAIL]`; `Model/faults#FAULT_BAND`.

[T4]-[QUEUED]: Wire progress observation points into the import and energy rails.
- Capability: long-running decode and translate operations surface observe-modality progress facts.
- Shape: `libs/csharp/Rasm.Bim/.planning/Exchange/import.md` DWG arm registers `ICadReader.OnProgress` onto the import progress point; `libs/csharp/Rasm.Bim/.planning/Energy/derive.md` translate matrix registers the OpenStudio `ProgressBar` callback onto the energy progress point.
- Unlocks: cancellable imports and UI progress without codec coupling.
- Anchors: `IDEAS.md` `[BIM_HOOK_RAIL]`; `.api/api-acadsharp.md`; `.api/api-openstudio.md`.
- Atomic: two wiring rows and fence touch-ups on landed pages.

[T5]-[QUEUED]: Land the instrument roster and meter owner on the observability page.
- Capability: `rasm.bim.<domain>.<measure>` instrument table — name, kind, UCUM unit, tag set, receipt source — and one meter owner over injected `IMeterFactory`.
- Shape: `libs/csharp/Rasm.Bim/.planning/Model/observability.md` gains the roster, the `IMeterFactory.Create(MeterOptions)` owner fence, and `Meter.CreateHistogram<T>(name, unit, description, tags, advice)` rows with explicit-bound advice fallback.
- Unlocks: dashboard tiles and the AppHost `InstrumentFan` contributed arm.
- Anchors: `IDEAS.md` `[BIM_TELEMETRY_TAP]`; `libs/csharp/.api/api-diagnostics-metrics.md`.

[T6]-[QUEUED]: State the span and attribution law rows.
- Capability: one `ActivitySource` law (scope name, `Op`-derived span names) and the baggage-sourced tenant/model tag law every instrument row carries.
- Shape: two law rows on `libs/csharp/Rasm.Bim/.planning/Model/observability.md` beside the instrument roster.
- Unlocks: span-profile correlation and per-tenant cost attribution at the AppHost composition.
- Anchors: `IDEAS.md` `[BIM_TELEMETRY_TAP]`.
- Atomic: law rows on the landed page.

[T7]-[QUEUED]: Author `BimEvent` and the CloudEvents envelope projection on the new events page.
- Capability: closed `[Union]` domain-fact family with one envelope projection stamping type, source, subject, and the distributed-tracing extension.
- Shape: `libs/csharp/Rasm.Bim/.planning/Exchange/events.md` gains the event union, the `CloudNative.CloudEvents.SystemTextJson` formatter fence, and the `rasm.bim.<domain>.<fact>` type-naming law.
- Unlocks: `[T8]` mint-point wiring and app-tier transport bindings.
- Anchors: `IDEAS.md` `[BIM_EVENT_FABRIC]`.

[T8]-[QUEUED]: Pin the event mint points across the owning pages.
- Capability: every model-mutating fact names its mint row at the owning rail.
- Shape: one mint row each on `libs/csharp/Rasm.Bim/.planning/Review/versioning.md` (commit landed), `libs/csharp/Rasm.Bim/.planning/Review/issues.md` (board mutation), `libs/csharp/Rasm.Bim/.planning/Review/validation.md` (verdict issued), `libs/csharp/Rasm.Bim/.planning/Exchange/export.md` (artifact minted), and `libs/csharp/Rasm.Bim/.planning/Energy/exchange.md` (energy artifact minted).
- Unlocks: event-driven review pipelines and CDE webhooks.
- Anchors: `IDEAS.md` `[BIM_EVENT_FABRIC]`.
- Atomic: one mint row per named page.

[T9]-[QUEUED]: Author the progress comparison fold and evidence receipt on the new progress page.
- Capability: capture-epoch occurrences joined to `TaskAssignment` element sets, minting per-task observed completion, variance band, and the unmatched-occurrence residue.
- Shape: `libs/csharp/Rasm.Bim/.planning/Planning/progress.md` gains the comparison fold over `Exchange/reconstruct#RECONSTRUCTION` occurrences, `ConstructionState.At` expectations, and the `Model/query#ELEMENT_SET` join, and the typed evidence receipt.
- Unlocks: `[T10]` earned-value actuals and reality-capture dashboards.
- Anchors: `IDEAS.md` `[PROGRESS_VERIFICATION]`.

[T10]-[QUEUED]: Join observed completion into the earned-value actuals read.
- Capability: evidence-backed actuals beside authored `IfcTaskTime.Completion` with a stated precedence law.
- Shape: `libs/csharp/Rasm.Bim/.planning/Planning/cost.md` `[EARNED_VALUE]` gains the observed-completion source row and its precedence over the authored percent and the actual-interval fraction.
- Unlocks: dispute-grade earned value.
- Anchors: `IDEAS.md` `[PROGRESS_VERIFICATION]`.
- Atomic: one source row and precedence law.

[T11]-[QUEUED]: Author the results admission fold on the new energy results page.
- Capability: Compute results receipt admitted onto zone/space quantity rows, re-emittable as Psets, readable by the AppUi report.
- Shape: `libs/csharp/Rasm.Bim/.planning/Energy/results.md` gains the admission fold keyed by the `EnergyArtifact` content key, the zone/space quantity rows over `Model/zones#ZONE_GRAPH`, and the Pset re-emission row through `Semantics/properties#PROPERTY_TEMPLATES`.
- Unlocks: results-aware QA facets and energy dashboards from the model.
- Anchors: `IDEAS.md` `[ENERGY_RESULTS_ANNOTATION]`.

[T12]-[QUEUED]: Land the `BimBenchReceipt` family and claim roster on the observability page.
- Capability: typed per-op benchmark claims — import per format, egress, query over graph scales, geo ingest, tessellation — corpus-gated.
- Shape: `libs/csharp/Rasm.Bim/.planning/Model/observability.md` gains the receipt record, the claim roster, and the corpus-gate admission row.
- Unlocks: regression-proof codec changes and push-down comparison evidence.
- Anchors: `IDEAS.md` `[BIM_BENCH_RECEIPTS]`; `csharp:Rasm.AppHost/Observability/benchmarks.md`.

[T13]-[QUEUED]: Pin the connection-interface content-key lowering and re-materialization ends.
- Capability: `IfcConnectionGeometry` and 2nd-level space-boundary surfaces ride the `Connect` edge as content-keyed typed geometry.
- Shape: `libs/csharp/Rasm.Bim/.planning/Projection/relations.md` hashes the interface surface into the blob store and stamps the key on the `Connect` edge; `libs/csharp/Rasm.Bim/.planning/Projection/egress.md` re-materializes through the ctor-held profiles-store lane.
- Unlocks: one-hop Compute reads and boundary-preserving re-export.
- Anchors: `IDEAS.md` `[CONNECTION_INTERFACE_GEOMETRY_DECODE]`.

[T14]-[QUEUED]: Pin the complex-quantity group-axis flatten and raise ends.
- Capability: `Discrimination`/`Quality`/`Usage` grouping identity survives the dot-path flatten and re-emits nested.
- Shape: `libs/csharp/Rasm.Bim/.planning/Projection/semantic.md` `FlattenQuantities` stamps the group-axis rows; `libs/csharp/Rasm.Bim/.planning/Projection/egress.md` `RaiseQuantity` rebuilds nested `IfcPhysicalComplexQuantity` children.
- Unlocks: identity-lossless QTO round-trip.
- Anchors: `IDEAS.md` `[QUANTITY_BAG_GROUP_AXIS]`.
- Tension: seam `QuantityBag` group-axis column lands first — the `Rasm.Element` counterpart owns the wire and `Bake` merge ripple.

[T15]-[QUEUED]: Author the BCF-API response carrier and paged-collection fold.
- Capability: a BCF-API response admits back into the archive-domain family — status/header fold, pagination cursor, body lowering onto `BcfTopic`/`BcfComment`/`BcfViewpoint`.
- Shape: `libs/csharp/Rasm.Bim/.planning/Review/issues.md` gains the response peer of `BcfApiRequest` discriminated by resource, the snake-case body admission over `BcfApiContext`, and the paged-collection fold; execution stays on the Compute transport.
- Unlocks: live CDE topic sync onto `IssueBoard`.
- Anchors: `IDEAS.md` `[BCF_API_RESPONSE_ADMISSION]`; `BcfWireMapper` archive-wire correspondence.

[T16]-[BLOCKED]: Land the Brick systems-operations projector once the live-binding owner is named.
- Capability: `DistributionSystem` reach sets lowered onto Brick `Point`/`Equipment`/`Location` classes with `Fedby`/`PointOf`/`PartOf`/`LocationOf` relations; interval-rollup reads ride `Aggregation.AggregateByInterval`.
- Shape: `libs/csharp/Rasm.Bim/.planning/Model/systems.md` gains the Brick projector beside `SystemTrace`, reading connectivity, never re-minting a second store.
- Unlocks: BMS-aware coordination and operations-phase handover beyond COBie.
- Anchors: `IDEAS.md` `[BRICK_SYSTEMS_OPERATIONS_OVERLAY]`; `.api/api-brickschema-net.md`.
- Tension: which app-platform owner lands the live-point binding resolver (`BACnetReference`/`ModbusDevice` rows)? Route: `libs/csharp/Rasm.AppHost/.planning/` capability pages and the AppHost growth register.

[T17]-[QUEUED]: Project reconciled type objects into Materials candidate rows — the reverse type-minting export off the IFC ingest.
- Capability: reconciled `IfcElementType` data — identity, property sets, classification — projects into a typed candidate export the Materials `ComponentRow` railed `Of` factories admit, provenance-marked as imported-library rows; the provenance-marking decision co-signs with the Materials owner.
- Shape: one projection member on `libs/csharp/Rasm.Bim/.planning/Exchange/import.md` at the type-object reconciliation end, emitting candidate rows keyed by source-library identity; admission folds stay Materials-side.
- Unlocks: an ingested manufacturer IFC library seeds the Materials catalogue instead of dying at occurrence projection; the Materials `[IFC_ADMISSION_FOLD_MAP]` route gains its ingest-side surface.
- Anchors: the import type-object reconciliation, the `Projection/semantic` property flatten, the Materials railed `Of` factory family.
- Ripple: `Rasm.Materials` `[IFC_PRODUCT_LIBRARY_ADMISSION]`.

[T18]-[QUEUED]: Reconcile the seam registry to the page-owned wire spellings — the interchange census counterpart.
- Capability: `ARCHITECTURE.md` seam rows for the IFC interchange wire, the kind-discriminated model diff, and the BCF wire re-anchor to the spellings the Exchange and Review pages own, so the frozen names carry one authority and the TypeScript core wire census decodes current mints.
- Shape: seam-registry rows on `libs/csharp/Rasm.Bim/ARCHITECTURE.md` aligned against `libs/csharp/Rasm.Bim/.planning/Exchange/export.md`, `libs/csharp/Rasm.Bim/.planning/Exchange/import.md`, and the Review pages; page spellings win, registry follows.
- Unlocks: unblocks the core codec-registry reconciliation — census rows and landing classes re-anchor to settled owners.
- Anchors: the seam-registry disagreement the core census names; the Exchange and Review wire clusters.
- Ripple: `typescript:core` `[0004]`.
- Atomic: registry-row alignment, no wire redesign.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

(none)
