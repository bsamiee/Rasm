# [MATERIALS_TASKLOG]

Host-neutral materials owner's open and closed work, distilled from `IDEAS.md`. Each task card leads with a status marker — `[QUEUED]`/`[ACTIVE]`/`[BLOCKED]` open, `[COMPLETE]`/`[DROPPED]` closed — and names the exact sub-domain and file it lands in. One idea spawns one or more tasks; a task is scoped guidance, not a full spec.

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

[PANEL_VOCABULARY_RENAME]-[QUEUED]: Dashboard descriptor vocabulary sheds the board-family names.
- Capability: one `PanelKind`/`PanelRow` meaning per package — the board-type catalogue keeps the domain names and the dashboard descriptors carry their own, so a same-named `[SmartEnum]` pair never splits one spelling across two concepts.
- Shape: `libs/csharp/Rasm.Materials/.planning/Projection/observability.md` dashboard descriptor block — the `PanelKind`/`PanelRow` re-declarations rename to dash-scoped names, the IaC `_PACKS` decode counterpart in the same pass.
- Unlocks: the one-canonical-name law holds package-wide; `Component/panel.md` keeps sole custody of the board vocabulary.
- Anchors: `Component/panel.md` `PanelKind` board roster; the semantic-consistency naming law.
- Atomic: one rename pair with its decode counterpart.

[README_APPHOST_ROW_RECUT]-[QUEUED]: README's AppHost row states the benchmark-gate consumer and its deferred arming.
- Capability: the registry row carries the live coupling truth — the benchmark gate as sole compile consumer, deferred until armed, observability surfaces attributed to the kernel capsule — so the up-reference is never cut as dead.
- Shape: `libs/csharp/Rasm.Materials/README.md` `[02]` `Rasm.AppHost` row re-cut; the `libs/csharp/Rasm.Materials/ARCHITECTURE.md` lead acknowledges the benchmark up-reference exception in the same pass.
- Unlocks: the branch benchmark-peer ruling reads consistently at the folder registry.
- Anchors: branch `RULINGS.md` benchmark-peer row; IDEAS `[KERNEL_BENCH_PROFILE_CORPUS]` sole-consumer anchor.
- Atomic: one row and one lead clause.

[RESEARCH_ROUTE_TIER_RECUT]-[QUEUED]: Blocked research routes point at the substrate catalog tier alone.
- Capability: member-verification routes name the tier the homing law admits, so a route never sends a session to a folder catalog the law forbids minting.
- Shape: `libs/csharp/Rasm.Materials/.planning/Projection/benchmarks.md` `[APPHOST_BENCHMARK_CATALOG]` route and `libs/csharp/Rasm.Materials/.planning/Projection/observability.md` `[LOGLEVEL_WARNING_MEMBER]` route — the folder-tier first hop deletes, the branch-tier catalog stays the sole hop.
- Unlocks: routes align with the substrate-catalog homing law.
- Anchors: `libs/csharp/.api/` substrate tier; the no-folder-stub law.
- Atomic: two route lines.

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

[SIGNAL_FAN_ARMS]-[BLOCKED]: Fixed-severity log projection settles beside the landed instrument tap and checkpoint ledger.
- Capability: the four banded `[LoggerMessage]` partials declare their fixed warning severity from a catalogued member, completing the fault-log half of the Materials signal tap.
- Shape: the log projection on `libs/csharp/Rasm.Materials/.planning/Projection/observability.md` `[05]-[FAULT_LOG]` beside the settled `MaterialsLatency` checkpoints.
- Unlocks: IDEAS.md [MATERIALS_SIGNAL_TAP] — the last unsettled fault-log band closes.
- Anchors: `Projection/observability.md` `[05]-[FAULT_LOG]`; the settled `[04]-[INSTRUMENT_TAP]` kernel rows, level cells, and contributor port.
- Arms: a routed logging-abstractions catalogue admits the exact `LogLevel` warning member named by the page's `[LOGLEVEL_WARNING_MEMBER]` research row.

[SIGNAL_DESCRIPTOR_ROWS]-[BLOCKED]: Materials' descriptor pack decodes on the deploy-plane board compiler.
- Capability: the landed `MaterialsDescriptors` panel and alert rows reach the board plane through the producer-pack ingest; drives from IDEAS `[MATERIALS_SIGNAL_TAP]`.
- Shape: rows settled at `libs/csharp/Rasm.Materials/.planning/Projection/observability.md` `[06]-[DESCRIPTOR_ROWS]`; the decode counterpart is one `_PACKS` row on `libs/typescript/iac/.planning/operate/observe.md`.
- Unlocks: IDEAS.md [MATERIALS_SIGNAL_TAP] — Materials evidence renders on the shared board plane.
- Anchors: `Projection/observability.md` `[06]-[DESCRIPTOR_ROWS]`; `libs/typescript/iac/.planning/operate/observe.md` `_PACKS` ingest.
- Arms: `libs/typescript/iac/.planning/operate/observe.md` carries the Materials `_PACKS` provenance row.

[BENCH_WORKLOAD_ROWS]-[BLOCKED]: Eight workload rows resolve injected content keys with `CaseOf` carrying each key.
- Capability: the workload corpus grades through the shared AppHost gate; drives from IDEAS `[KERNEL_BENCH_PROFILE_CORPUS]`.
- Shape: rows hold in `libs/csharp/Rasm.Materials/.planning/Projection/benchmarks.md`.
- Unlocks: IDEAS.md [KERNEL_BENCH_PROFILE_CORPUS] — the receipt-bearing claim composition lands.
- Anchors: `Projection/benchmarks.md` `[02]-[WORKLOAD_ROWS]`.
- Arms: either routed Rasm.AppHost API catalogue admits the current corpus-bearing receipt and gate members named by `[APPHOST_BENCHMARK_CATALOG]`.

[ANALYTICS_SCHEMA_ROWS]-[BLOCKED]: Five composite-key schemas, provenance columns, and parameterized folds land as the analytics egress.
- Capability: catalogue analytics rows land typed on the durable plane; drives from IDEAS `[CATALOGUE_ANALYTICS_EGRESS]`.
- Shape: schemas and folds hold in `libs/csharp/Rasm.Materials/.planning/Projection/analytics.md`.
- Unlocks: IDEAS.md [CATALOGUE_ANALYTICS_EGRESS] — the analytics egress lands.
- Anchors: `Projection/analytics.md` `[03]-[SCHEMA_ROWS]`, `[04]-[PROJECTION_FOLDS]`.
- Arms: either routed UnitsNet API catalogue admits the four exact dimensioned selectors named by `[UNITSNET_ANALYTICS_SELECTORS]` and `libs/csharp/Rasm.Persistence/.planning/Store/observability.md` carries the store rows.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[CONSTITUENT_VOCABULARY_RULING]-[DROPPED]: the blocker question is carried law — the Element seam owns the constituent-fraction algebra (`MaterialComposition.OfConstituentSet` normalization on the seam owner page) and each producer mints instance rows at its own seam, so a Materials-local custody ruling re-litigates the settled split; the producer work continues on IDEAS `[CONSTITUENT_SET_PRODUCER]`.
[SIGNAL_TAP_ROSTER]-[COMPLETE]: the seven-point `MaterialsHooks` composition over the kernel capsule landed at `Projection/observability.md` `[03]-[HOOK_RAIL]` — the AppHost-registry composition dissolved into kernel capsule composition, the app root admitting the `Rasm.Materials` scope by name.
