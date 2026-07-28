# [MATERIALS_TASKLOG]

Host-neutral materials owner's open and closed work, distilled from `IDEAS.md`. Each task card leads with a status marker — `[QUEUED]`/`[ACTIVE]`/`[BLOCKED]` open, `[COMPLETE]`/`[DROPPED]` closed — and names the exact sub-domain and file it lands in. One idea spawns one or more tasks; a task is scoped guidance, not a full spec.

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
- Arms: arm when the AWC SDPWS 4.2A-4.3D unit-shear cells stand as published data this corpus may transcribe under `SEED_ROW_LAW`.

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
- Arms: arm when `component#CATALOGUE` settles the `ComponentRow` column carrying import provenance and the `Rasm.Bim` ingest counterpart declares whether it mints in reverse.

[EPD_ROW_SHAPE]-[BLOCKED]: Draft the Materials-side EPD row shape — declared unit, module coverage, expiry — on the assessment landing page.
- Capability: EC3/Ökobaudat/EPD-Norge records key to `MaterialId`/component designations with `PropertyEvidence` identity, demoting the authored per-kg bases to declared fallback.
- Shape: the row shape in `libs/csharp/Rasm.Materials/.planning/Properties/assessment.md` with the fallback demotion in `libs/csharp/Rasm.Materials/.planning/Properties/sustainability.md`.
- Unlocks: `[EPD_DATA_INGESTION]` record half.
- Anchors: `Published<T>` evidence rows, `glazing#GLAZING_FAMILY` `GlazingGwp`/`GenericEpd`.
- Arms: arm when the `python:data` peer declares the record schema and transport its Assessment wire carries.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[SIGNAL_DESCRIPTOR_ROWS]-[COMPLETE]: the ingest counterpart landed — `materials.catalogue` seats on the deploy tuple at `libs/typescript/iac/.planning/operate/observe.md` `_PACKS`, and the key now rides inside `MaterialsDescriptors.Pack` as its own first column, so producer and consumer hold one spelling by construction.
[QTO_MINT_PINS]-[COMPLETE]: the blocker question answers yes on both counts — the seam admits the `DetailSchema.Takeoff` row family and `MaterialPropertySet` now carries the `Density` accessor — and `TypeTakeoff` mints the set at `Projection/component#COMPONENT_PROJECTOR`, with `BakeSection` widened so one ref resolution serves both the SI seam projection and the mm-basis takeoff and `SeedType` collapsed onto one bag fold. Partiality is two-tiered: a section-free component mints no set, a stiffness-free substance drops the mass row alone.
[ANALYTICS_SCHEMA_ROWS]-[COMPLETE]: the five declarations, their provenance columns, and the parameterized folds landed at `Projection/analytics.md`, and the blocker was a defect at both ends — the UnitsNet selectors resolve live (`Density.KilogramsPerCubicMeter`, `ThermalConductivity.WattsPerMeterKelvin`, `SpecificEntropy.JoulesPerKilogramKelvin`, `HeatTransferCoefficient.WattsPerSquareMeterKelvin`, each a `public double` SI accessor), and the Persistence store rows were never the gate because `Query/columnar#ANALYTICS_RESIDENCE` already owns the admission gate every producer crosses.
[BENCH_WORKLOAD_ROWS]-[COMPLETE]: `Projection/benchmarks.md` `[03]-[GATE_COMPOSITION]` seats `MaterialsBench.Fresh` over `BenchmarkReceipt.Of` and `MaterialsBench.Gate` traversing the eight-row corpus through `BenchmarkGate.Gate`, harness and claim residence injected as functions.
[README_APPHOST_ROW_RECUT]-[DROPPED]: no registry row exists to re-cut — a sibling package never cards as a folder domain package, so the coupling truth landed on the two leads instead: the `ARCHITECTURE.md` lead names the benchmark up-reference and its one compile consumer, and the `README.md` lead attributes the signal plane to the kernel capsule.
[RESEARCH_ROUTE_TIER_RECUT]-[COMPLETE]: both research routes resolved against their landed owner pages rather than re-tiering, and `libs/.planning/README.md` `[API_TIERS]` now forecloses a catalogue named for a corpus package, so a member crossing a folder boundary routes to that owner's design page and never to a catalogue tier.
[PANEL_VOCABULARY_RENAME]-[COMPLETE]: no rename was needed — the descriptor row collapsed onto the kernel `PanelSpec`/`BoardPack` carrier, so `Projection/observability#BOARD_PACK` declares no panel type at all and `Component/panel.md` holds the board vocabulary uncontested.
[CONSTITUENT_VOCABULARY_RULING]-[DROPPED]: the blocker question is carried law — the Element seam owns the constituent-fraction algebra (`MaterialComposition.OfConstituentSet` normalization on the seam owner page) and each producer mints instance rows at its own seam, so a Materials-local custody ruling re-litigates the settled split; the producer work continues on IDEAS `[CONSTITUENT_SET_PRODUCER]`.
[SIGNAL_TAP_ROSTER]-[COMPLETE]: the seven-point `MaterialsHooks` composition over the kernel capsule landed at `Projection/observability.md` `[03]-[HOOK_RAIL]` — the AppHost-registry composition dissolved into kernel capsule composition, the app root admitting the `Rasm.Materials` scope by name.
[SIGNAL_FAN_ARMS]-[COMPLETE]: `MaterialsLog` landed at `Projection/observability.md` `[05]-[EVIDENCE_RECORDS]` — the branch logging-abstractions catalogue carries the `LogLevel` roster, so the fixed warning severity is declaration data on two banded partials (a rail-side `Logged` aspect and an isolated-evidence `Drain`), and the per-fault-family verb family the blocked card assumed never mints.
