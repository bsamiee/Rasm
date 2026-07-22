# [MATERIALS_IDEAS]

Forward pool of higher-order concepts for the host-neutral materials owner, each grounded in the folder's domain and current platform capability. Open ideas drive the tasks in `TASKLOG.md`; a finished or dropped idea moves to `[2]-[CLOSED]` with a one-line disposition so it is never re-litigated.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis — the capability outcome, never the landing motion>.
- Capability: <the higher-order invariant, owner capability, or concept established — altitude only, never a page path, row list, or member spelling>.
- Shape: <where the work lands and at what grain — repo-relative page with section/row, or a new-page path; the concrete surface, so Capability never names it>.
- Unlocks: <the downstream capability at the consumer grain — a task narrows its parent idea's Unlocks to THIS slice as `IDEAS.md [SLUG] — consequence`; a set-completion card states the completeness bar that is its acceptance contract>.
- Anchors: <owners, seams, packages, catalogs, doctrines, and techniques making the work plausible — anchors, never procedures>.
- Arms: <present only on a BLOCKED or gated card; the exact observable that flips it actionable — a catalog row landing, a member query returning evidence, a package admitted>.
- Route: <present only on a probe, research, or member-pin card; the ordered verification path run before any fence lands>.
- Tension: <only when an unresolved constraint, boundary, or bet shapes the work — the genuine bet, never the arming condition Arms carries>.
- Ripple: <counterpart card — cross-folder as `pkg` `[SLUG]` or a same-folder prerequisite `[SLUG]`, prefixed follows/precedes/mirrors when build order is load-bearing>.
Capability, Shape, Unlocks, and Anchors are required on every open card; statuses closed — `ACTIVE|QUEUED|BLOCKED` open, `COMPLETE|DROPPED` closed; IDs are SEMANTIC UPPERCASE_SNAKE slugs carrying meaning — never numeric (`[0007]`-class NNNN IDs are a defect), for cards AND research tokens alike; a hyphenated slug anywhere is a defect; repo-relative paths only. Design pages carry the terminal `[RESEARCH]` section always — `(none)` marks empty, absence is an error. Ideas state higher-order concepts, never landing-grain tasks.
-->

[DESIGN_BASIS_AXIS]-[QUEUED]: A jurisdiction axis across the capacity rail — EC3/EN 1994 steel and composite, EN 1996 masonry — beside the realized AISC/TMS verdicts.
- Capability: Capacity receipt columns folded under the governing code's resistance and interaction spellings, so the EN-seeded half of the catalogue (IPE/HE/UPN at S355) receives an EC3 verdict instead of an AISC one.
- Shape: A `DesignBasis` policy row selecting per-basis kernels (EC3 §6.3.1 buckling curves with per-class imperfection α, §6.3.2 χLT, γM0/γM1 partial factors over the typed `VividOrange.Standards` `En1993`/`En1994`/`En1996` rows with `NationalAnnex` threaded); lands in `libs/csharp/Rasm.Materials/.planning/Component/capacity.md`, `libs/csharp/Rasm.Materials/.planning/Component/steel.md`, and `libs/csharp/Rasm.Materials/.planning/Component/masonry.md`.
- Unlocks: EU-deliverable member verdicts over the already-seeded two-region catalogue; the international sizing product the seed tables promise.
- Anchors: `steel#STEEL_FAMILY` `SteelDesign`/`DesignCapacity`, `capacity#SECTION_CAPACITY` `SectionCapacity`/`CapacityReceipt`, `SteelGrade.YieldMpa` already reading the Table 3.1 registered `f_y` per annex; the folder `RULINGS.md` basis-column row.
- Tension: the basis-tagged re-cut of the closed verdict vocabulary lands only with the `Rasm.Compute/structural#DESIGN_CHECK` co-sign.

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
- Anchors: `Projection/component#COMPOSITION_AUTHOR` `ConstituentSet` (realized, coerce-and-delegate), the seam `MaterialComposition.OfConstituentSet` fraction normalization owning the algebra — Materials mints instance rows at its own seam.
- Arms: a provenance-bound constituent-fraction data source lands — family mix-design columns or EPD-grade ingestion rows.
- Tension: no family's canonical composition is constituent-shaped today — the fraction data itself is the bet, not the vocabulary custody.

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

[KERNEL_BENCH_PROFILE_CORPUS]-[BLOCKED]: Materials kernel claims grade through the shared AppHost corpus gate.
- Capability: content-bound benchmark workload rows join the corpus gate as receipt-bearing claims.
- Shape: `BenchKernel`/`BenchInput`/`BenchWorkload` rows hold in `libs/csharp/Rasm.Materials/.planning/Projection/benchmarks.md`; the receipt composition lands on arming.
- Unlocks: Materials kernel performance truth reaches the estate benchmark plane.
- Anchors: `Projection/benchmarks.md` `[02]-[WORKLOAD_ROWS]`, `[03]-[GATE_COMPOSITION]`; the `Rasm.AppHost` project reference — this card's gate composition is its sole compile consumer, and the ref holds under the branch up-reference ruling while the card arms.
- Arms: `libs/csharp/Rasm.Materials/.api/api-rasm-apphost.md` or `libs/csharp/.api/api-rasm-apphost.md` admits every member named by `[APPHOST_BENCHMARK_CATALOG]`, including the current receipt corpus slot.

[CATALOGUE_ANALYTICS_EGRESS]-[BLOCKED]: Catalogue truth lands on the analytics plane as typed dataset rows.
- Capability: composite-key dataset schemas with parameterized projection folds carry the catalogue onto the analytics egress.
- Shape: schemas and folds hold in `libs/csharp/Rasm.Materials/.planning/Projection/analytics.md`.
- Unlocks: catalogue analytics for costing, carbon, and coverage reads over the durable plane.
- Anchors: `Projection/analytics.md` `[02]-[COLUMN_VOCABULARY]`, `[03]-[SCHEMA_ROWS]`, `[04]-[PROJECTION_FOLDS]`.
- Arms: `libs/csharp/.api/api-unitsnet.md` admits every dimensioned selector named by `[UNITSNET_ANALYTICS_SELECTORS]`.

## [02]-[CLOSED]

<!-- source-only: closed idea card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[CMU_SUBTYPE_CARRIER]-[COMPLETE]: Ruled a realization-bag row — `CmuSeed.Rows` seeds the `DetailSchema.ProfileSubtype` token off `CmuPhysics.IfcSubtypeOf` (the family widened to `DetailLane.Realization`), the `Rasm.Bim` egress profile lane resolves the subtype from the carried row, never a `CmuRow` seed column and never a cross-package call.
[MATERIALS_SIGNAL_TAP]-[COMPLETE]: landed as kernel composition on `Projection/observability.md` — `MaterialsFact` family, the seven-point `MaterialsHooks` rail over the kernel capsule, `MaterialsInstruments` roster with the contributor port, `MaterialsLatency` checkpoint ledger, and `MaterialsDescriptors` rows; the fixed-severity log projection and the descriptor iac decode row stay open on `TASKLOG.md` `[SIGNAL_FAN_ARMS]`/`[SIGNAL_DESCRIPTOR_ROWS]`.
