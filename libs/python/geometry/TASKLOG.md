# [PY_GEOMETRY_TASKLOG]

Open and closed work for `geometry`, distilled from `IDEAS.md`. Each task card leads with `[ID]-[STATUS]: thesis` and carries `Capability`, `Shape`, `Unlocks`, `Anchors`, and optional `Tension` fields. `[1]-[OPEN]` holds live work; `[2]-[CLOSED]` records finished or dropped tasks.

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

[GEOREF_BAND]-[QUEUED]: land the georeference extraction and authoring rows.
- Capability: georeference truth becomes first-class evidence — extraction mints the CRS fact and authoring writes it back into the model.
- Shape: one extraction projection minting CRS, local-to-map transform, and true north on `libs/python/geometry/.planning/ifc/analysis.md`, and one authoring verb row minting and updating `IfcMapConversion`/`IfcProjectedCRS` on `libs/python/geometry/.planning/ifc/authoring.md`.
- Unlocks: IDEAS.md [IFC_GEOREFERENCE] — scan-vs-model in shared map frames, geo-data planes consuming one geometry-minted georeference fact instead of re-deriving it.
- Anchors: the pinned `util.geolocation` rows at `libs/python/geometry/.api/ifcopenshell.md` — `get_helmert_transformation_parameters` the one extraction seam over every coordinate-operation subtype, the `auto_*`/manual pairs, `get_crs`, and the north projections; the authoring verb table; idea `[IFC_GEOREFERENCE]`.

[NONRIGID_ARM]-[QUEUED]: land the non-rigid registration arm and the deformation split.
- Capability: registration distinguishes construction deviation from structural deformation through a per-point deformation field.
- Shape: one `probreg` CPD/FilterReg arm on the registration mode vocabulary returning a deformation-field carrier on `libs/python/geometry/.planning/scan/registration.md`, and a deviation projection splitting rigid residual from deformation magnitude on `libs/python/geometry/.planning/scan/deviation.md`.
- Unlocks: IDEAS.md [NONRIGID_DEFORMATION_TRACK] — monitoring-grade evidence (settlement, deflection, bowing) from repeat scans, completing the registration family beside the global, coarse, and fine arms.
- Anchors: the `Cloud` array carrier crossing worker seams; the registration session shape and its `_seeded` provider-slot seeding; the landed `IngestStage.FARTHEST_POINT` budget bound; the admitted root-manifest `probreg` row; idea `[NONRIGID_DEFORMATION_TRACK]`.
- Tension: `probreg` rides an interpreter marker in the root manifest, so the arm's fences stay floor-gated like every native-gated provider.

[TABULAR_ROUNDTRIP_VERBS]-[QUEUED]: land the spreadsheet exchange verb pair on the lifecycle owner.
- Capability: lifecycle tables round-trip to estimator spreadsheets and back through the authoring rail.
- Shape: one `ifccsv` export/re-import verb pair on the lifecycle verb table of `libs/python/geometry/.planning/ifc/costing.md`, selector-grammar scoped, the re-import writing attribute and Pset edits back through the authoring rail.
- Unlocks: IDEAS.md [LIFECYCLE_TABULAR_EXCHANGE] — estimator and scheduler workflows without a BIM authoring tool in the loop, completing the ifcopenshell exchange family beside `ifcdiff`, `ifcpatch`, `ifc4d`, and `ifc5d`.
- Anchors: the admitted root-manifest `ifccsv` row; `ifc/selector` grammar; the costing partition vocabulary; idea `[LIFECYCLE_TABULAR_EXCHANGE]`.
- Atomic: one verb pair on the lifecycle verb table.

[SCENE_DESCRIPTOR_DECODE]-[BLOCKED]: land the descriptor decode fold on the energy plane.
- Capability: decode rows mapping descriptor bands to their owners — `SunState` onto the climate solar vocabulary, light roster onto the model admission, GLB shading through the tessellation rail — the daylight `RecipeName` rows riding the landed `RunSpec.recipe` axis.
- Shape: rows on `libs/python/geometry/.planning/energy/simulate.md`, `libs/python/geometry/.planning/energy/climate.md`, and `libs/python/geometry/.planning/energy/model.md`.
- Unlocks: IDEAS.md [DAYLIGHTING_SCENE_DESCRIPTOR] — closed-loop solar and daylight studies from the live Rhino scene, the estate scene-descriptor vocabulary gaining its first consumer.
- Anchors: idea `[DAYLIGHTING_SCENE_DESCRIPTOR]`; the estate `[SCENE_DESCRIPTOR_SCHEMA]` schema pin as the field authority.
- Arms: `[SCENE_DESCRIPTOR_SCHEMA]` lands the descriptor shape in the shared wire vocabulary at `libs/.planning`; before it does, every decoded column is a local invention the schema then contradicts.
- Ripple: follows `libs/.planning` `[SCENE_DESCRIPTOR_SCHEMA]`; mirrors IDEAS.md `[DAYLIGHTING_SCENE_DESCRIPTOR]`.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[GLB_STORE_SPILL]-[COMPLETE]: the daemon cache fold gained the injected `ObjectStoreLane` spill — two write-once objects per unit under one `spill_path` derivation, the read-through ahead of the kernel, and `SpillOutcome` riding the crossing's own receipt row.
[GLB_SERVE_READTHROUGH]-[COMPLETE]: `GeometryServe.sync` answers ring first and the durable tier second, a refused, absent, or unbound tier all reaching one unknown-artifact fault; `_frames` re-shaped onto `(artifact_id, octets)` so the store leg asserts no producer literal.
[GEOREF_MEMBER_PIN]-[COMPLETE]: the `util.geolocation` namespace is pinned into `libs/python/geometry/.api/ifcopenshell.md` from the installed distribution source — the `HelmertTransformation` nine-field extraction, the `auto_*`/manual pairs, the north and angle projections — with the absence-is-identity law on its topology row.
[FPS_DOWNSAMPLE_ROW]-[COMPLETE]: `IngestStage.FARTHEST_POINT` lands as a `_CARRIER` fold rather than a `pdal` row, the stage vocabulary partitioning into pipe and carrier tables and the floor gate widening onto the stage axis.
[ENERGY_GRAPH_TYPED_FAULTS]-[COMPLETE]: `EnergyFault` seats at the band's tier-0 owner and `GraphFault` at the graph substrate; all six energy coordinate-string raises and the analytic negative-cap raise carry their facts as kwargs the converting fence lifts whole.
[CHARTER_UNIT_SINGLE_WRITER]-[DROPPED]: refuted — `MeasureRow.unit` is not a second AUTHORING site but the assertion `_diverged` PROVES against the runtime census, so dropping the column deletes the one arm that catches a series exported against a descriptor scaled by a thousand; the estate single-writer law governs authority, and a proved dual declaration is a checksum rather than a second writer.
[ARCH_FLOOR_SENTENCE]-[COMPLETE]: the codemap engine-lane sentence narrows to the genuinely long native IFC phases behind function-local gates, matching every verb page's caller-floor default.
[GMSH_REGISTRY_ALIGN]-[COMPLETE]: the compute `MeshExchange` generation arm landed, so the README row re-annotates gmsh as compute-owned at the branch with this folder consuming the meshes rather than the kernel.
[GEOMETRY_BENCH_CORPUS]-[COMPLETE]: `CORPUS` rosters one graded row per bench seam over the `BenchBand` ceiling table and `graded` composes the runtime `Bench.graded` entry; `bench_subject` collapses eight hand-spelled subject f-strings into one derivation both the roster and every producing page read.
[IFC_HEAD_FENCE_ALIGN]-[COMPLETE]: the bare `@beartype` on `IfcAnalysis.run` is deleted; `@beartype(conf=FAULT_CONF)` on `_dispatch` is the one fence across all three ifc capsules.
[KERNEL_BENCH_LANE]-[COMPLETE]: the mesh serve entry benches whole-crossing under graduation's own bench seam, with no geometry instrument row.
[SCAN_BENCH_SUBJECTS]-[COMPLETE]: registration and reconstruction bench whole-crossing rounds keyed by mode and source point count.
[COST_WEAVE_FOLD]-[COMPLETE]: the evidence cost ledger closes inside `evidence_run` and emits one cost receipt.
[COST_BRACKET_COMPOSE]-[COMPLETE]: the ledger composes the runtime cost substrate, so this folder holds no sampling bracket of its own.
[SPAN_SUBJECT_RENAME]-[COMPLETE]: the evidence span renames onto its cleared subject at close-out.
[PROFILE_SUBJECT_MAP]-[DROPPED]: the runtime kernel already projects its own phase names; no geometry subject map is admitted.
[CHARTER_MEASURE_TABLE]-[COMPLETE]: the charter rows every measure with its UCUM unit and aggregation.
[DISTRIBUTION_ROWS]-[COMPLETE]: every charter row has a producing fold deriving its spelling from the charter.
[CHARTER_CENSUS_GATE]-[COMPLETE]: the install leg proves every charter row's whole descriptor against the runtime census before any registration.
[FRAME_SCHEMA_PORT]-[COMPLETE]: the content-keyed evidence frame seals its columns read-only and admits through a rail at its producer.
[FRAME_PRODUCER_ROWS]-[COMPLETE]: every evidence producer carries its own frame projection.
[TRACE_LINK_WIRE_PROBE]-[COMPLETE]: the optional W3C carrier mints on the handoff and decodes at the compute mirror; `[EVIDENCE_TRACE_LINKS]` closes against both fences.
[PULSE_DRAIN_PROBE]-[COMPLETE]: the runtime lane owns the pulse queue and its drain custody, and delivery stays lossy; `[MID_OPERATION_PULSE]` closes against the landed pulse rows.
