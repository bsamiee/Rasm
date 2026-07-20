# [MATERIALS_IDEAS]

Forward pool of higher-order concepts for the host-neutral materials owner, each grounded in the folder's domain and current platform capability. Open ideas drive the tasks in `TASKLOG.md`; a finished or dropped idea moves to `[2]-[CLOSED]` with a one-line disposition so it is never re-litigated.

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

[DESIGN_BASIS_AXIS]-[QUEUED]: A jurisdiction axis across the capacity rail — EC3/EN 1994 steel and composite, EN 1996 masonry — beside the realized AISC/TMS verdicts.
- Capability: Capacity receipt columns folded under the governing code's resistance and interaction spellings, so the EN-seeded half of the catalogue (IPE/HE/UPN at S355) receives an EC3 verdict instead of an AISC one.
- Shape: A `DesignBasis` policy row selecting per-basis kernels (EC3 §6.3.1 buckling curves with per-class imperfection α, §6.3.2 χLT, γM0/γM1 partial factors over the typed `VividOrange.Standards` `En1993`/`En1994`/`En1996` rows with `NationalAnnex` threaded), the basis a column on the capacity receipt — never sibling per-code capacity surfaces; lands in `libs/csharp/Rasm.Materials/.planning/Component/capacity.md`, `libs/csharp/Rasm.Materials/.planning/Component/steel.md`, and `libs/csharp/Rasm.Materials/.planning/Component/masonry.md`.
- Unlocks: EU-deliverable member verdicts over the already-seeded two-region catalogue; the international sizing product the seed tables promise.
- Anchors: `steel#STEEL_FAMILY` `SteelDesign`/`DesignCapacity`, `capacity#SECTION_CAPACITY` `SectionCapacity`/`CapacityReceipt`, `SteelGrade.YieldMpa` already reading the Table 3.1 registered `f_y` per annex.
- Tension: Realized `SectionCapacity` case names encode the basis (`SteelLrfd`, `MasonryCompression` = TMS) and the utilisation verdict crosses to the `Rasm.Compute/structural#DESIGN_CHECK` consumer — the basis axis is a root-up rename of that closed verdict vocabulary the Compute counterpart must co-sign before the case family re-cuts.

[MATERIALS_SIGNAL_TAP]-[QUEUED]: Telemetry-as-tap over the materials owners — a typed hook-point roster, a folder instrument fan over the receipt stream, and dashboard/alert descriptor rows, zero OTel reference at library altitude.
- Capability: `rasm.materials.<domain>.<point>` typed hook registry (catalogue admission, section solve, capacity check, graph compile, acquisition fit, wire mint, projection) with veto/observe/replay modalities and subscriber-fault isolation onto the banded `ComponentFault`/`MaterialFault`/`ElementFault` rails; a `MaterialsInstrumentFan` projecting the typed receipts — `CapacityReceipt`, acquisition `Provenance`, `WireProvenance`, `ComponentResolution` — into `IMeterFactory`-minted UCUM instruments and `ILogger` banded-fault records, per-app scoped so two hosts never share a meter.
- Shape: one new S2 page `libs/csharp/Rasm.Materials/.planning/Projection/observability.md` — roster rows, the fan arm table over `rasm.materials.*` kinds, `ILatencyContext` checkpoints on the eager catalogue and interaction-diagram constructions, and dashboard/alert descriptor rows as data.
- Unlocks: materials solve/fit/wire activity on estate dashboards without one emit call in domain code; AppHost mounts Materials as a sibling arm beside the AppUi, Compute, and Persistence contributions.
- Anchors: AppHost `InstrumentFan.Mount` contributed-arm law and `HookId` grammar (`libs/csharp/Rasm.AppHost/.planning/Observability/instruments.md`), Compute `ComputeInstrumentFan` folder-fan precedent, branch `.api` `api-diagnostics-metrics.md` `IMeterFactory`, `api-logging-abstractions.md` `ILogger`, `api-extensions-telemetry.md` `ILatencyContext`, `capacity#COMPONENT_FAULT_RAIL` band-2300/2450 telemetry-reader banding.
- Tension: the AppHost mount is the counterpart half of a cross-folder pair — the sibling-fan roster in `instruments.md` names AppUi, Compute, and Persistence today and must name Materials.

[KERNEL_BENCH_PROFILE_CORPUS]-[QUEUED]: Deterministic benchmark workload rows and `BenchmarkReceipt` projections over the materials hot kernels, span-profile-correlated operation names included.
- Capability: closed workload rows — `SectionSolver` over the catalogued profile algebra, `InteractionDiagram` strain sweep with fibre integration, thin-QR GGX fit, `MaterialGraph` topological compile and eval, spectral upsample, texture sampling fold, Kubelka-Munk mix — each pinned to catalogue/library inputs so a run is content-keyed reproducible; receipts join the branch `BenchmarkReceipt` family and carry the stable operation names a span-profile correlator labels.
- Shape: one new S2 page `libs/csharp/Rasm.Materials/.planning/Projection/benchmarks.md` — the workload row table, receipt columns (kernel, input key, host-fingerprint slot), and the `BenchmarkKind` projection row on the Materials fan.
- Unlocks: regression-gated kernel performance for the section and appearance engines; Pyroscope flame-graph attribution against named materials operations.
- Anchors: AppHost `Observability/benchmarks.md` `BenchmarkReceipt` and corpus gate, `component#SECTION_SOLVER`, `capacity#SECTION_CAPACITY`, `acquisition#ACQUISITION` thin-QR fit, `graph#MATERIAL_GRAPH` `SourceFirstTopologicalSort`, branch `.api` `api-pyroscope-opentelemetry.md` (correlation composes at the root, never here).
- Tension: corpus-gate registration is the AppHost counterpart's; Materials owns workload rows and receipts only.

[CATALOGUE_ANALYTICS_EGRESS]-[QUEUED]: Seed catalogues and receipt streams as columnar analytics data — one Arrow-shaped schema family per catalogue, store-slot registered, parquet/DuckDB queryable.
- Capability: `ComponentRow` families, `MaterialPropertyCatalogue`, `SustainabilityCatalogue`, `MaterialLibrary` rows, and `CapacityReceipt` streams project onto typed columnar record batches — schema as data, provenance columns carried — so takeoff, carbon, and capacity analytics run as queries instead of object walks, and `python:data` reads the same datasets for assessment round-trips.
- Shape: one new S2 page `libs/csharp/Rasm.Materials/.planning/Projection/analytics.md` — per-catalogue columnar schema rows and the projection folds; writers and store custody stay Persistence-side behind `store.materials.<verb>` slots.
- Unlocks: steel tonnage and carbon dashboards off DuckDB; EPD and property audits as SQL; lake parity for materials data.
- Anchors: Persistence `store.<domain>.<verb>` SlotRegistry law (`Store/observability.md`), Persistence-owned `ParquetSharp.Dataset` and Arrow admissions, `Properties/properties#MATERIAL_PROPERTY_CATALOGUE` registered-row database, `capacity#SECTION_CAPACITY` receipt columns.
- Tension: slot rows and dataset writers are the Persistence counterpart's; Materials owns schema and projection only.

[ASSESSMENT_WIRE_INGESTION]-[QUEUED]: A real Materials end for the declared Assessment wire — typed in-situ assessment and declaration records lowered onto `Published<T>` evidence rows.
- Capability: assessment record vocabulary — in-situ test results, condition grades, dated declarations — admitted with provenance and expiry onto `Published<T>`/`PropertyEvidence` and folded into `MaterialPropertyCatalogue`/`SustainabilityCatalogue` rows as evidence-dated overrides beside the seed rows, so the `[WIRE]: Assessment` edge gains its owning page.
- Shape: one new page `libs/csharp/Rasm.Materials/.planning/Properties/assessment.md` — the record vocabulary, the admission fold, the assessed-over-published resolution law, and the EPD row landing surface `EPD_DATA_INGESTION` arms; earns the Properties folder its second non-eponymous sibling.
- Unlocks: assessed-condition property resolution over published seeds; a concrete landing owner for the EPD ingestion card.
- Anchors: `Properties/properties#MATERIAL_PROPERTY_CATALOGUE` `Published<T>`/`Admit`/`Lookup`, the realized `INormalDistributionUncertainty<T>` band discrimination on `Published<T>.Kind`, NodaTime `LocalDate` expiry, `ARCHITECTURE.md` `[WIRE]: Assessment` edge from `python:data`.
- Tension: wire record schema and transport are the `python:data` peer's to co-sign — the Materials-side vocabulary and fold proceed; the transport binding waits.

[PANEL_LATERAL_CAPACITY]-[BLOCKED]: Wood-structural-panel diaphragm and shear-wall unit shears as published rows feeding a lateral capacity case.
- Capability: SDPWS 4.2/4.3 unit shears by panel grade × thickness × nail × edge spacing — the columns `PanelRow` + `FastenPattern` already select — folded through the one `Check(demand)` rail as a lateral case.
- Shape: A published unit-shear row table beside `SpanRatings` with one `SectionCapacity` case and `Check` arm; the NDS `Cd` axis rides the connector-owned `DurationRow`; lands in `libs/csharp/Rasm.Materials/.planning/Component/panel.md` and `libs/csharp/Rasm.Materials/.planning/Component/capacity.md`.
- Unlocks: Sheathing selection as lateral design — the diaphragm/shear-wall dimension the member and wall rails do not cover.
- Anchors: `panel#PANEL_FAMILY` `FastenPattern`/`SpanRatings` printed-data precedent, `capacity#SECTION_CAPACITY` growth law naming the diaphragm check.
- Tension: SDPWS 4.2A-4.3D tabulated unit shears are per-cell printed data this corpus cannot re-derive — seeding waits on the published tables in hand; an invented cell fails `SEED_ROW_LAW`.

[CONSTITUENT_SET_PRODUCER]-[BLOCKED]: A first producer for the constituent-set composition — mix-design and multi-substance component captures.
- Capability: Fraction-tagged constituent rows derived from family data so a component's material truth stops flattening to one `SubstanceId` when the real product is a weighted composition (concrete mix, faced board, IGU frame-and-glass), and per-constituent carbon decomposes the EPD way.
- Shape: A Materials mix/constituent vocabulary whose rows `CompositionOf` selects into the realized `CompositionAuthor.ConstituentSet`, the fourth seam case finally carrying traffic; lands in `libs/csharp/Rasm.Materials/.planning/Projection/component.md` and `libs/csharp/Rasm.Materials/.planning/Component/component.md`.
- Unlocks: `IfcMaterialConstituentSet` egress for multi-substance components; constituent-resolved carbon accounting.
- Anchors: `Projection/component#COMPOSITION_AUTHOR` `ConstituentSet` (realized, coerce-and-delegate), the seam `MaterialComposition.OfConstituentSet` fraction normalization.
- Tension: No family's canonical composition is constituent-shaped today — the producer needs a mix-design vocabulary decision (which owner mints fraction rows) and EPD-grade constituent fractions, provenance-bound data no current table carries.

[TYPE_QUANTITY_RECEIPT]-[BLOCKED]: Type-level quantity takeoff — linear mass, surface area per length, volume per length authored onto the projected Type subgraph.
- Capability: Deterministic, content-keyed QTO facts (kg/m off `AreaMm2` × substance density, m²/m off `HeatedPerimeterMm`, m³/m off `AreaMm2`) minted once at projection from data already on the node pair — the costing/carbon substrate every BIM type library leads with.
- Shape: `QuantityRow`-minted measures on a Type-owned quantity set the `ComponentProjector.ProjectType` fold authors beside the detail bag; lands in `libs/csharp/Rasm.Materials/.planning/Projection/component.md`.
- Unlocks: Steel tonnage, fire-protection area, and concrete volume as graph reads; cost and carbon before any structural check runs.
- Anchors: `component#QUANTITY_ROW` (`Density` already a mint row), `Projection/component#COMPONENT_PROJECTOR` `SeamSection`, `ComputedSection.AreaMm2`/`HeatedPerimeterMm` crossing the seam per section.
- Tension: A quantity property-set row family is a wire-visible content-key change on the seam `DetailSchema`/property vocabulary the `Rasm.Element` owner must co-sign, and the substance density readback needs a typed accessor on the seam `MaterialPropertySet` — both counterpart surfaces, neither a Materials-local edit.

[FABRICATION_SCHEDULE_WIRE]-[BLOCKED]: Shop-deliverable schedules derived from the realization vocabularies — bar bending schedules, weld maps, stud layouts.
- Capability: Per-pour/per-member aggregation of the realized per-component scalars (BS 8666 shape codes, ACI/EN bend receipts, weld prep and stud spacing) into the contract documents a fabricator is paid against.
- Shape: Materials owns the typed scalars (realized — `RebarSchedule`/`RebarBend`/`ShapeCodes`, `WeldProfile`/`GroovePrep`/`StudRow`); `Rasm.Fabrication` owns the derivation into schedules over the projected `DetailSchema.Realization` bags and its registered `IElementProjection` row; the Materials audit lands in `libs/csharp/Rasm.Materials/.planning/Component/reinforcement.md` and `libs/csharp/Rasm.Materials/.planning/Component/joint.md`.
- Unlocks: Fabrication-deliverable pipeline — realization detail as shop documents, not graph metadata.
- Anchors: `reinforcement#REINFORCEMENT_FAMILY` host-neutral scalar law, `joint#JOINT_FAMILY` receipts, `Projection/component#COMPONENT_PROJECTOR` detail-bag authoring.
- Tension: Schedule derivation is `Rasm.Fabrication`'s process-derivation surface — a cross-package build this folder cannot land; the Materials end is realized and waiting on the Fabrication counterpart card.

[IFC_PRODUCT_LIBRARY_ADMISSION]-[BLOCKED]: Reverse type path — admitting an ingested IFC product library into `ComponentRow` candidates.
- Capability: A manufacturer's IFC content (curtain-wall system, proprietary deck, hanger range) minting `Component` rows through the railed `Of` factories, so authored seeds and admitted imports share one `ComponentId` space.
- Shape: An admission fold from reconciled `IfcElementType` data onto family-dispatched `Component.Of` construction, imported rows provenance-marked beside the seed rows; lands in `libs/csharp/Rasm.Materials/.planning/Component/component.md`.
- Unlocks: Two-way materials library — vendor BIM content as first-class catalogue rows.
- Anchors: Per-family round-trips already frozen (`SteelClass.IfcSubtype` `ProfileSet`, the `IfcMaterialLayerSet` ply round-trip, the `DetailSchema` bag round-trips), `component#CATALOGUE` fail-loud admission.
- Tension: Declared `[PORT]: IIfcTypeReconciler` reconciles ingested types AGAINST Materials-minted ids only — the reverse mint is a Bim-side ingest extension with a provenance-marking decision on `ComponentRow` the `Rasm.Bim` counterpart card must co-sign.

[EPD_DATA_INGESTION]-[BLOCKED]: Database-backed environmental vectors replacing the authored generic-EPD constants.
- Capability: EC3/Ökobaudat/EPD-Norge records per actual product — declared units, module coverage, expiry — keyed to `MaterialId`/component designations, the authored per-kg bases and fixed A4-D fractions demoted to the declared fallback.
- Shape: A peer-ingested EPD row shape on the Materials side with `PropertyEvidence` the per-row identity carrier (its `Option<LocalDate>` expiry axis already realized), the ingestion arriving over the declared `[WIRE]: Assessment` edge; lands in `libs/csharp/Rasm.Materials/.planning/Properties/assessment.md` and `libs/csharp/Rasm.Materials/.planning/Properties/sustainability.md`.
- Unlocks: Reportable whole-life carbon — audited submissions over evidence-dated records instead of illustrative constants.
- Anchors: `glazing#GLAZING_FAMILY` `GlazingGwp`/`GenericEpd`, the Properties sustainability rows, the `ARCHITECTURE` `[WIRE]: Assessment` edge.
- Tension: Wire record schema and transport are the `python:data` peer's to co-sign via its counterpart card, and EPD records are external provenance-bound data — no Materials-local table can seed them.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

[CMU_SUBTYPE_CARRIER]-[COMPLETE]: Ruled a realization-bag row — `CmuSeed.Rows` seeds the `DetailSchema.ProfileSubtype` token off `CmuPhysics.IfcSubtypeOf` (the family widened to `DetailLane.Realization`), the `Rasm.Bim` egress profile lane resolves the subtype from the carried row, never a `CmuRow` seed column and never a cross-package call.
