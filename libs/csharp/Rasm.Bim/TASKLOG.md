# [RASM_BIM_TASKLOG]

Open and closed work distilled from `IDEAS.md`. `[01]-[OPEN]` carries task cards with `[QUEUED]`, `[ACTIVE]`, or `[BLOCKED]` leaders; `[02]-[CLOSED]` carries `[COMPLETE]` or `[DROPPED]` cards. One idea spawns one or more tasks; each task names the exact sub-domain or file it lands in.

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

[ELEMENT_SET_VIEWPORT_SEAM]-[BLOCKED]: `ElementSet` query algebra reaches the AppUi viewport as a declared `[PROJECTION]` seam.
- Capability: model-query results (`Model/query` `ElementSet`) rendered as AppUi viewport/inspector selections.
- Shape: one `Model/query -> csharp:Rasm.AppUi/<owner> # [PROJECTION]` seam row with the consuming AppUi page fence.
- Unlocks: saved-query overlays, selection-driven dashboards, query-scoped exports.
- Anchors: `Model/query` `ElementSet`; the AppUi `[V9]` growth register carries the reciprocal record.
- Tension: no AppUi page names a consumer today — the seam row re-enters `ARCHITECTURE.md` `[02]-[SEAMS]` only when one does; deferred pressure never rides the ledger.

[SCHEDULE_CHARTS_SEAM]-[BLOCKED]: `ScheduleNetwork` CPM/4D projection reaches the AppUi Charts plane as a declared `[PROJECTION]` seam.
- Capability: 4D construction-sequencing and critical-path dashboards over the `Planning/schedule` domain.
- Shape: one `Planning/schedule -> csharp:Rasm.AppUi/Charts # [PROJECTION]` seam row with the consuming dashboards fence.
- Unlocks: 4D playback tiles, earned-value overlays beside the existing `Planning/cost` receipt row.
- Anchors: `Planning/schedule` `ScheduleNetwork`; the AppUi `[V9]` growth register carries the reciprocal record.
- Tension: no AppUi consuming fence exists today — same re-entry law as `[ELEMENT_SET_VIEWPORT_SEAM]`.

[PROGRESS_COMPARISON_FOLD]-[QUEUED]: Author the progress comparison fold and evidence receipt on the new progress page.
- Capability: capture-epoch occurrences joined to `TaskAssignment` element sets, minting per-task observed completion, variance band, and the unmatched-occurrence residue.
- Shape: `libs/csharp/Rasm.Bim/.planning/Planning/progress.md` gains the comparison fold over `Exchange/reconstruct#RECONSTRUCTION` occurrences, `ConstructionState.At` expectations, and the `Model/query#ELEMENT_SET` join, and the typed evidence receipt.
- Unlocks: `[EARNED_VALUE_ACTUALS_JOIN]` earned-value actuals and reality-capture dashboards.
- Anchors: `IDEAS.md` `[PROGRESS_VERIFICATION]`.

[EARNED_VALUE_ACTUALS_JOIN]-[QUEUED]: Join observed completion into the earned-value actuals read.
- Capability: evidence-backed actuals beside authored `IfcTaskTime.Completion` with a stated precedence law.
- Shape: `libs/csharp/Rasm.Bim/.planning/Planning/cost.md` `[EARNED_VALUE]` gains the observed-completion source row and its precedence over the authored percent and the actual-interval fraction.
- Unlocks: dispute-grade earned value.
- Anchors: `IDEAS.md` `[PROGRESS_VERIFICATION]`.
- Atomic: one source row and precedence law.

[ENERGY_RESULTS_ADMISSION_FOLD]-[QUEUED]: Author the results admission fold on the new energy results page.
- Capability: Compute results receipt admitted onto zone/space quantity rows, re-emittable as Psets, readable by the AppUi report.
- Shape: `libs/csharp/Rasm.Bim/.planning/Energy/results.md` gains the admission fold keyed by the `EnergyArtifact` content key, the zone/space quantity rows over `Model/zones#ZONE_GRAPH`, and the Pset re-emission row through `Semantics/properties#PROPERTY_TEMPLATES`.
- Unlocks: results-aware QA facets and energy dashboards from the model.
- Anchors: `IDEAS.md` `[ENERGY_RESULTS_ANNOTATION]`.

[CONNECTION_KEY_LOWERING]-[QUEUED]: Pin the connection-interface content-key lowering and re-materialization ends.
- Capability: `IfcConnectionGeometry` and 2nd-level space-boundary surfaces ride the `Connect` edge as content-keyed typed geometry.
- Shape: `libs/csharp/Rasm.Bim/.planning/Projection/relations.md` hashes the interface surface into the blob store and stamps the key on the `Connect` edge; `libs/csharp/Rasm.Bim/.planning/Projection/egress.md` re-materializes through the ctor-held profiles-store lane.
- Unlocks: one-hop Compute reads and boundary-preserving re-export.
- Anchors: `IDEAS.md` `[CONNECTION_INTERFACE_GEOMETRY_DECODE]`.

[QUANTITY_GROUP_AXIS_ENDS]-[QUEUED]: Pin the complex-quantity group-axis flatten and raise ends.
- Capability: `Discrimination`/`Quality`/`Usage` grouping identity survives the dot-path flatten and re-emits nested.
- Shape: `libs/csharp/Rasm.Bim/.planning/Projection/semantic.md` `FlattenQuantities` stamps the group-axis rows; `libs/csharp/Rasm.Bim/.planning/Projection/egress.md` `RaiseQuantity` rebuilds nested `IfcPhysicalComplexQuantity` children.
- Unlocks: identity-lossless QTO round-trip.
- Anchors: `IDEAS.md` `[QUANTITY_BAG_GROUP_AXIS]`.
- Tension: seam `QuantityBag` group-axis column lands first — the `Rasm.Element` counterpart owns the wire and `Bake` merge ripple.

[BCF_RESPONSE_CARRIER]-[QUEUED]: Author the BCF-API response carrier and paged-collection fold.
- Capability: a BCF-API response admits back into the archive-domain family — status/header fold, pagination cursor, body lowering onto `BcfTopic`/`BcfComment`/`BcfViewpoint`.
- Shape: `libs/csharp/Rasm.Bim/.planning/Review/issues.md` gains the response peer of `BcfApiRequest` discriminated by resource, the snake-case body admission over `BcfApiContext`, and the paged-collection fold; execution stays on the Compute transport.
- Unlocks: live CDE topic sync onto `IssueBoard`.
- Anchors: `IDEAS.md` `[BCF_API_RESPONSE_ADMISSION]`; `BcfWireMapper` archive-wire correspondence.

[BRICK_SYSTEMS_PROJECTOR]-[BLOCKED]: Land the Brick systems-operations projector once the live-binding owner is named.
- Capability: `DistributionSystem` reach sets lowered onto Brick `Point`/`Equipment`/`Location` classes with `Fedby`/`PointOf`/`PartOf`/`LocationOf` relations; interval-rollup reads ride `Aggregation.AggregateByInterval`.
- Shape: `libs/csharp/Rasm.Bim/.planning/Model/systems.md` gains the Brick projector beside `SystemTrace`, reading connectivity, never re-minting a second store.
- Unlocks: BMS-aware coordination and operations-phase handover beyond COBie.
- Anchors: `IDEAS.md` `[BRICK_SYSTEMS_OPERATIONS_OVERLAY]`; `.api/api-brickschema-net.md`.
- Tension: which app-platform owner lands the live-point binding resolver (`BACnetReference`/`ModbusDevice` rows)? Route: `libs/csharp/Rasm.AppHost/.planning/` capability pages and the AppHost growth register.

[TYPE_CANDIDATE_EXPORT]-[QUEUED]: Project reconciled type objects into Materials candidate rows — the reverse type-minting export off the IFC ingest.
- Capability: reconciled `IfcElementType` data — identity, property sets, classification — projects into a typed candidate export the Materials `ComponentRow` railed `Of` factories admit, provenance-marked as imported-library rows; the provenance-marking decision co-signs with the Materials owner.
- Shape: one projection member on `libs/csharp/Rasm.Bim/.planning/Exchange/import.md` at the type-object reconciliation end, emitting candidate rows keyed by source-library identity; admission folds stay Materials-side.
- Unlocks: an ingested manufacturer IFC library seeds the Materials catalogue instead of dying at occurrence projection; the Materials `[IFC_ADMISSION_FOLD_MAP]` route gains its ingest-side surface.
- Anchors: the import type-object reconciliation, the `Projection/semantic` property flatten, the Materials railed `Of` factory family; the durable-store leg is derived lineage — `Rasm.Persistence` `Version/provenance.md` `ProvKind.Import` attributes imported entities off the changefeed, so no store provenance column mints.
- Ripple: `Rasm.Materials` `[IFC_PRODUCT_LIBRARY_ADMISSION]`.

[READER_ROWS_RECONCILE]-[QUEUED]: Ingest reader rows compose the owner-provided key space.
- Capability: every row the connection and structural ingests stamp resolves to an Element-declared static or the owner-blessed reader-local category; the ingest keeps its enrichment payload with zero open-mint spellings.
- Shape: `libs/csharp/Rasm.Bim/.planning/Semantics/connection.md` reader-row block and the structural ingest's inline `PropertyName.Create` spellings re-anchor to the Element-provided statics and category.
- Unlocks: the `Rasm.Compute` structural reads and the Bim writer provably share one key space.
- Anchors: `libs/csharp/.planning/RULINGS.md` seam-bag custody row; `Rasm.Element` `Properties/property.md` provision band.
- Ripple: follows `Rasm.Element` `[DETAIL_SCHEMA_READER_PROVISION]`; mirrors `Rasm.Compute` `[STRUCTURAL_ROW_STATICS]`.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[SEAM_REGISTRY_RECONCILE]-[COMPLETE]: seam registry re-anchored to page-owned spellings — the phantom `GeoFeatureWkb`/`GeoFeatureWire` rows re-cut to `GeoWire` (Data, Core, Ui, and the Persistence geo-store edge), the pageless `BcfTopicWire`-to-core edge deleted, `ModelDiff` verified page-owned as the cross-runtime wire; the `typescript:core` `[BIM_CENSUS_RECONCILE]` census now reads current mints.
[HOOK_RAIL_ROSTER]-[COMPLETE]: `Model/observability#HOOK_RAIL` owns the point roster and the per-composition `BimHooks` registry record over the kernel point capsule; modality rows and subscriber-fault isolation arrive settled from the kernel, faults parking as `IsolatedFault` rows on the composition evidence cell.
[PROGRESS_POINT_WIRING]-[COMPLETE]: progress points wired — `Exchange/import#IMPORT_RAIL` `AcadReader.Read` fires `rasm.bim.exchange.progress` off `ICadReader.OnProgress`, `Energy/derive#TRANSLATE_MATRIX` `TranslateProgress` fires `rasm.bim.energy.progress` off `onPercentageUpdated`.
[INSTRUMENT_ROSTER_MOUNT]-[COMPLETE]: `Model/observability#TELEMETRY_TAP` owns the instrument roster as kernel `InstrumentRow` declarations with `Buckets` advice bounds, mounted through the kernel identity mint (`BimTelemetry.Mount`) or materialized by an app fan through the contributor port.
[SPAN_ATTRIBUTION_LAW]-[COMPLETE]: span and attribution law rows landed on `Model/observability#TELEMETRY_TAP` — one `ActivitySource`, `Op`-derived span names, `rasm.tenant`/`rasm.model` baggage-sourced tags.
[EVENT_ENVELOPE_PROJECTION]-[COMPLETE]: `Exchange/events#EVENTS` owns `BimEvent`, the `BimEnvelope` projection over `JsonEventFormatter` with the traceparent/tracestate extension rows, and the `rasm.bim.<domain>.<fact>` type law.
[EVENT_MINT_ROWS]-[COMPLETE]: mint rows pinned — versioning `CommitLanded`, issues `IssueMutated`, validation `VerdictIssued`, export `ArtifactMinted`, energy exchange `EnergyMinted`.
[BENCH_RECEIPT_ROSTER]-[COMPLETE]: `Model/observability#BENCH_RECEIPTS` owns the `BimBenchClaim` roster and `BimBenchReceipt` record under the AppHost corpus-gate admission row.
