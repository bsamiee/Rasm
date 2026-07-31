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

[EMISSION_QUANTITY_ADMISSION]-[QUEUED]: Gate an emission admission on `PhotometricQuantity.CanonicalIsRadiance` so a flux magnitude stops landing in a radiance column.
- Capability: the flux-versus-radiance split the band declares governs the row write, so a lumen or watt figure refuses the emission columns instead of being written as if it were `W/(sr·m²)`.
- Shape: the `WithEmission` write at `libs/csharp/Rasm.Materials/.planning/Appearance/photometric.md` `[02]-[PHOTOMETRIC]`, reading `EmissionInput.Source.CanonicalIsRadiance` before the row construction.
- Unlocks: `IDEAS.md [PHOTOMETRIC_RECEIPT]` — an emission magnitude whose unit the row can be trusted to carry.
- Anchors: the `PhotometricQuantity` `canonicalIsRadiance` column rows, `UnitEvidence.RadiometricSi`, `MaterialParameters.EmissionLuminance` as the radiance-shaped column, the `graph#MATERIAL_GRAPH` normalized-chromaticity emission law.
- Tension: normalizing flux to radiance needs an emitter area and solid angle no appearance row carries, so the honest reader is a REFUSAL rather than a conversion — and the alternative, deleting the column, cements the mis-typed write it names.
- Atomic: one gate at one write site.

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

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[GRAPH_ALGEBRA_NODE_PRODUCER]-[COMPLETE]: the producer landed on the graph owner itself — `graph#MATERIAL_GRAPH` carries the `GraphEdit` authoring request union (`Node`/`Seat`/`Route` — three cases disjoint by refusal contract), the `ShadeChannel` sink-port roster with each row's read and re-seat delegates, and the one `MaterialGraph.Author(Seq<GraphEdit>, Op)` fold beside `Ports(int)`, so a layered or masked appearance is a sequence of edits over `MaterialGraph.Default` that Compiles, shades, and lowers through the SAME frozen-order rail. The arity and answerability sweeps `Compile` once inlined collapsed into one shared `Admit(node, known)` predicate both admissions read against their own known-set, so surface count fell while the producer gained its gate; the `Raster/set#SET_BIND` `Program` arm now LOWERS through the fold (`GraphEdit.Seat` per covered slot + `MaterialGraph.PortOf` resolving the normal-frame port, the hand-wired node list and literal port ids deleted).
[EMISSION_READOUT_CONSUMERS]-[COMPLETE]: the fork resolved to WIRE COLUMNS against deletion — the corpus census found no reader in C#, python `transport/shapes`, or TS `interchange/codec`, but the sibling `acquisition#ACQUISITION` receipt carries the same evidence class to the wire and to the python peer and cites this payload as its precedent, so the defect was the row TRUNCATING the receipt, not the receipt existing. `MaterialParameters.EmissionProvenance` now carries the whole `EmissionInput`, `interchange#MATERIAL_WIRE` appends the nullable `WireEmission` receipt at `[Key(7)]` (the frozen `EmissionUnit`/`EmissionValue` pair untouched at 5-6) with its `EmissionInput`→wire transcription generated under the same RMG completeness gate, and `CanonicalIsRadiance` re-cards as `[EMISSION_QUANTITY_ADMISSION]` because its honest reader is a refusal, not a conversion. `Photometric.WithEmission` writes the whole payload (the `EmissionInput` construction normalizing the emission colour to unit-Y with exposure moved onto the energy channel), and both peer decode rows are landed — python `EmissionWire` + trailing `emission` mirror row, TS `_EmissionVector` + the `Material` `emission` option field.
[ASSESSMENT_RECORD_VOCABULARY]-[COMPLETE]: `Properties/assessment.md` landed whole — `AssessmentModality`/`ConditionGrade`/`AssessedProperty`/`EpdRow`, the `AssessmentRecord` union, `AssessmentAdmission.Admit` onto the shared `Published<T>`, and `AssessmentResolution.Resolve` carrying the assessed-over-published law.
[EPD_ROW_SHAPE]-[COMPLETE]: the blocker named a missing landing surface, which now exists — `EpdRow` carries issuer, registration, declared unit, module-coverage census, and expiry on `Properties/assessment.md`; the peer transport binding stays a research row on the page with its `python:data` route, never a card blocker.
[GPU_PERIOD_ARM_TRUTH]-[COMPLETE]: landed as the edit itself rather than a note — `fn simplex` dropped its `period: i32`, the three `wrap(i, period)` calls are bare lattice reads matching `simplex3`'s seed-only form, and `basisAt` dispatches `simplex(x, y, seed)`; the parameter's absence IS the statement.
[SHADESPAN_RESEARCH_CLOSE]-[COMPLETE]: the debt deleted itself — `graph#MATERIAL_GRAPH` landed `ShadeSpan(ReadOnlySpan<ShadePoint>, MaterialParameters, Span<PortValue>, Span<SurfaceShade>, Op)` with `ScratchWidth`/`OperandWidth` the two compile-resolved rentals, `press#TEXTURE_PRESS` binds `ScratchWidth` at its band rental, and `press.md` `[05]-[RESEARCH]` carries only `[GPU_GRAPH_LOWERING]`.
[BIM_SUMMARY_ARITY]-[COMPLETE]: the repair landed in-wave — `Rasm.Bim/Semantics/appearance#APPEARANCE_PROJECTION` calls the eight-parameter seam factory positionally with no tolerance argument, and its research section records the frozen arity as carried law.
[TEXTURE_WIRE_CORPUS_ENTRIES]-[COMPLETE]: `tests/contracts/MANIFEST.md` carries `[02.17]-[TEXTURE_SET_BY_KEY]` and `[02.19]-[MATERIAL_WIRE]` at DESIGN-PIN beside the REAL `[02.18]-[ASSET_SET_MANIFEST]`, all over the shared vocabulary fragment, so the `IAppearanceWire.CorpusBorne` census answers on disk.
[PEER_TEXTURE_SET_DECODE]-[COMPLETE]: both peer ends landed in-wave — the TypeScript census and landing rows at `libs/typescript/core/.planning/interchange/codec.md` and the python `PROTO_VOCABULARY` row at `libs/python/runtime/.planning/transport/shapes.md`; the remaining descriptor-source debt (`Graph/element.proto` and the suite service vocabulary, plus the unwired `buf breaking` gate) is owned by the `tests/contracts/MANIFEST.md` `[02.9]` blocker row and is not re-carded here.
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
