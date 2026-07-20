# [MATERIALS_TASKLOG]

Host-neutral materials owner's open and closed work, distilled from `IDEAS.md`. Each task card leads with a status marker — `[QUEUED]`/`[ACTIVE]`/`[BLOCKED]` open, `[COMPLETE]`/`[DROPPED]` closed — and names the exact sub-domain and file it lands in. One idea spawns one or more tasks; a task is scoped guidance, not a full spec.

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

[SIGNAL_TAP_ROSTER]-[QUEUED]: Pin the materials hook-point roster — exact ids, payload types, and modality rows per point.
- Capability: seven points — `rasm.materials.catalogue.admit` (Veto), `rasm.materials.section.solve` (Observe), `rasm.materials.capacity.check` (Observe), `rasm.materials.graph.compile` (Observe), `rasm.materials.acquisition.fit` (Observe/Replay), `rasm.materials.wire.mint` (Observe), `rasm.materials.projection.project` (Veto) — each bound to one closed payload type and registered once at composition.
- Shape: roster rows in `libs/csharp/Rasm.Materials/.planning/Projection/observability.md`.
- Unlocks: `[MATERIALS_SIGNAL_TAP]` roster half; slice-implement discovery gets exact grep-able point names.
- Anchors: AppHost `HookId` grammar and registry uniqueness law, Compute `[HOOK_POINT_ROSTER]` precedent, subscriber-fault isolation onto the band-2300/2450 rails.

[SIGNAL_FAN_ARMS]-[QUEUED]: Pin the `MaterialsInstrumentFan` arm table — typed receipts onto UCUM instruments and banded faults onto structured logs.
- Capability: `CapacityReceipt`, acquisition `Provenance`, `WireProvenance`, and `ComponentResolution` kinds each project one instrument row (`rasm.materials.<domain>.<measure>` UCUM, `IMeterFactory`-minted, per-app scoped); `ComponentFault`/`MaterialFault`/`ElementFault` band onto `ILogger` records; `ILatencyContext` checkpoints wrap the eager catalogue and interaction-diagram constructions.
- Shape: the fan owner and arm table in `libs/csharp/Rasm.Materials/.planning/Projection/observability.md`.
- Unlocks: `[MATERIALS_SIGNAL_TAP]` projection half; the AppHost mount consumes one contributed arm.
- Anchors: AppHost `InstrumentFan.Mount` merge law (duplicate kinds composition-fatal), Compute `ComputeInstrumentFan` pre-envelope precedent, branch `.api` `api-diagnostics-metrics.md`/`api-logging-abstractions.md`/`api-extensions-telemetry.md`.

[SIGNAL_DESCRIPTOR_ROWS]-[QUEUED]: Project the instrument roster into dashboard-and-alert descriptor rows as data.
- Capability: panel and alert-rule descriptors over the Materials instrument names so the IaC compile leg provisions materials dashboards without hand-authored queries.
- Shape: descriptor rows beside the arm table in `libs/csharp/Rasm.Materials/.planning/Projection/observability.md`.
- Unlocks: `[MATERIALS_SIGNAL_TAP]` dashboard half; Compute `[DASHBOARD_ALERT_DESCRIPTOR]` gains a sibling contributor.
- Anchors: ts:iac Foundation-SDK compile leg, Compute descriptor precedent.
- Atomic: descriptor rows over an already-pinned roster.

[BENCH_WORKLOAD_ROWS]-[QUEUED]: Pin the deterministic kernel workload rows and the `BenchmarkReceipt` columns.
- Capability: workload rows for `SectionSolver`, `InteractionDiagram` sweep, thin-QR GGX fit, `MaterialGraph` compile and eval, spectral upsample, texture sampling, and Kubelka-Munk mix, each naming its pinned catalogue/library input and content key; receipt columns carry kernel, input key, and the host-fingerprint slot.
- Shape: the row table and receipt schema in `libs/csharp/Rasm.Materials/.planning/Projection/benchmarks.md`.
- Unlocks: `[KERNEL_BENCH_PROFILE_CORPUS]` Materials half; the AppHost corpus gate registers named workloads.
- Anchors: AppHost `Observability/benchmarks.md` receipt family, `component#SECTION_SOLVER`, `acquisition#ACQUISITION`, `graph#MATERIAL_GRAPH`.

[ANALYTICS_SCHEMA_ROWS]-[QUEUED]: Pin the per-catalogue columnar schema rows and projection folds for the analytics egress.
- Capability: one Arrow-shaped schema per source — `ComponentRow` families, `MaterialPropertyCatalogue`, `SustainabilityCatalogue`, `MaterialLibrary`, `CapacityReceipt` — provenance and evidence columns carried, projection folds total over the registered rows.
- Shape: schema rows and folds in `libs/csharp/Rasm.Materials/.planning/Projection/analytics.md`.
- Unlocks: `[CATALOGUE_ANALYTICS_EGRESS]` Materials half; Persistence slot writers consume typed batches.
- Anchors: Persistence `store.<domain>.<verb>` SlotRegistry law, `Properties/properties#MATERIAL_PROPERTY_CATALOGUE`.

[ASSESSMENT_RECORD_VOCABULARY]-[QUEUED]: Draft the assessment record vocabulary and its `Published<T>` lowering with the assessed-over-published resolution law.
- Capability: in-situ test results, condition grades, and dated declarations admit with provenance and expiry onto `Published<T>`/`PropertyEvidence`, folding into catalogue rows as evidence-dated overrides beside seed rows.
- Shape: the vocabulary and admission fold in `libs/csharp/Rasm.Materials/.planning/Properties/assessment.md`.
- Unlocks: `[ASSESSMENT_WIRE_INGESTION]` core; `[EPD_DATA_INGESTION]` gets its landing surface.
- Anchors: `Properties/properties#MATERIAL_PROPERTY_CATALOGUE` `Published<T>`/`Admit`/`Lookup`, `INormalDistributionUncertainty<T>` band discrimination, NodaTime `LocalDate` expiry.

[BASIS_KERNEL_DISPATCH]-[QUEUED]: Map the realized basis-encoding capacity case names and draft the `DesignBasis` policy row with per-basis kernel dispatch.
- Capability: `DesignBasis` selects EC3/EN 1994/EN 1996 kernels beside the realized AISC/TMS verdicts, the basis a receipt column; the closed verdict vocabulary re-cut is staged for the Compute co-sign.
- Shape: the policy row and dispatch in `libs/csharp/Rasm.Materials/.planning/Component/capacity.md`, EN verdict columns threading `libs/csharp/Rasm.Materials/.planning/Component/steel.md` and `libs/csharp/Rasm.Materials/.planning/Component/masonry.md`.
- Unlocks: `[DESIGN_BASIS_AXIS]` first cut; EU-deliverable verdicts over the EN-seeded catalogue half.
- Anchors: `SteelGrade.YieldMpa` per-annex read, `VividOrange.Standards` `En1993`/`En1994`/`En1996` rows with `NationalAnnex`.

[SDPWS_TABLE_ACQUISITION]-[BLOCKED]: Acquire the SDPWS 4.2A-4.3D tabulated unit shears as admissible published data.
- Capability: per-cell unit shears by panel grade, thickness, nail, and edge spacing seed the lateral capacity case under `SEED_ROW_LAW`.
- Shape: seed rows in `libs/csharp/Rasm.Materials/.planning/Component/panel.md` and the lateral case in `libs/csharp/Rasm.Materials/.planning/Component/capacity.md`.
- Unlocks: `[PANEL_LATERAL_CAPACITY]` seeding.
- Anchors: `panel#PANEL_FAMILY` printed-data precedent.
- Tension: blocker question — are the AWC SDPWS Tables 4.2A-4.3D cells available as published data this corpus may transcribe under `SEED_ROW_LAW`? Route: the AWC SDPWS publication as a live-doc verification target.

[CONSTITUENT_VOCABULARY_RULING]-[BLOCKED]: Rule which owner mints constituent fraction rows for the mix-design composition.
- Capability: a fraction-tagged mix vocabulary feeding `CompositionAuthor.ConstituentSet` so multi-substance components stop flattening to one `SubstanceId`.
- Shape: the ruling and vocabulary in `libs/csharp/Rasm.Materials/.planning/Projection/component.md` with family data columns in `libs/csharp/Rasm.Materials/.planning/Component/component.md`.
- Unlocks: `[CONSTITUENT_SET_PRODUCER]` vocabulary half.
- Anchors: seam `MaterialComposition.OfConstituentSet` fraction normalization.
- Tension: blocker question — does the seam `MaterialComposition` owner or a Materials-local mix vocabulary mint fraction rows? Route: the `Rasm.Element` composition seam-owner page.

[QTO_MINT_PINS]-[BLOCKED]: Pin the three type-level quantity mint folds and the seam co-sign ask.
- Capability: kg/m off `AreaMm2` and substance density, m²/m off `HeatedPerimeterMm`, m³/m off `AreaMm2`, minted once in `ComponentProjector.ProjectType` beside the detail bag.
- Shape: the mint folds in `libs/csharp/Rasm.Materials/.planning/Projection/component.md`.
- Unlocks: `[TYPE_QUANTITY_RECEIPT]` mint half.
- Anchors: `component#QUANTITY_ROW` `Density` row, `ComputedSection.AreaMm2`/`HeatedPerimeterMm`.
- Tension: blocker question — does the seam admit a quantity property-set row family on `DetailSchema` and a typed density accessor on `MaterialPropertySet`? Route: the `Rasm.Element` seam-owner pages.

[REALIZATION_SCALAR_AUDIT]-[QUEUED]: Audit the realized realization scalars complete against the schedule-derivation demand.
- Capability: `RebarSchedule`/`RebarBend`/`ShapeCodes` cover the BS 8666 shape-code set and `WeldProfile`/`GroovePrep`/`StudRow` cover weld-map and stud-layout derivation, gaps recorded as rows.
- Shape: audit deltas in `libs/csharp/Rasm.Materials/.planning/Component/reinforcement.md` and `libs/csharp/Rasm.Materials/.planning/Component/joint.md`.
- Unlocks: `[FABRICATION_SCHEDULE_WIRE]` Materials-side readiness proof.
- Anchors: `reinforcement#REINFORCEMENT_FAMILY` host-neutral scalar law, `joint#JOINT_FAMILY` receipts.
- Atomic: coverage audit over realized vocabularies.

[IFC_ADMISSION_FOLD_MAP]-[BLOCKED]: Map the family `Of` factories for imported-row admission and the provenance-marking column.
- Capability: reconciled `IfcElementType` data reaches family-dispatched `Component.Of` construction with imported rows provenance-marked beside seeds.
- Shape: the admission fold map in `libs/csharp/Rasm.Materials/.planning/Component/component.md`.
- Unlocks: `[IFC_PRODUCT_LIBRARY_ADMISSION]` Materials half.
- Anchors: `component#CATALOGUE` fail-loud admission, frozen per-family round-trips.
- Tension: blocker question — which `ComponentRow` column carries import provenance, and does the Bim ingest extend to reverse minting? Route: the `Rasm.Bim` ingest pages and its counterpart card.

[EPD_ROW_SHAPE]-[BLOCKED]: Draft the Materials-side EPD row shape — declared unit, module coverage, expiry — on the assessment landing page.
- Capability: EC3/Ökobaudat/EPD-Norge records key to `MaterialId`/component designations with `PropertyEvidence` identity, demoting the authored per-kg bases to declared fallback.
- Shape: the row shape in `libs/csharp/Rasm.Materials/.planning/Properties/assessment.md` with the fallback demotion in `libs/csharp/Rasm.Materials/.planning/Properties/sustainability.md`.
- Unlocks: `[EPD_DATA_INGESTION]` record half.
- Anchors: `Published<T>` evidence rows, `glazing#GLAZING_FAMILY` `GlazingGwp`/`GenericEpd`.
- Tension: blocker question — what record schema and transport does the `python:data` peer carry over the Assessment wire? Route: the `python:data` planning corpus and its counterpart card.


## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

(none)
