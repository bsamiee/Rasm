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

[FETCH_COORDINATE_FOLD]-[QUEUED]: the artifact fetch seam addresses its body by the bare content coordinate the request itself carries, with no single-field envelope standing between the rpc and the digest it names.
- Capability: an rpc binds the payload it carries, so a request naming one coordinate exposes that coordinate directly and every peer reads one spelling of it rather than an envelope name and a field name that must agree.
- Shape: `libs/python/geometry/.planning/mesh/serve.md` `[02]-[SERVE]` — the `GeometryServe.fetch` override and the repository custody call it drives; the refusal coordinate the absent-artifact row publishes moves with it.
- Unlocks: `IDEAS.md` `[GLB_SERVE_READTHROUGH]` — the serve edge reads the requested digest in one hop, so the streamed-frame proof and the absent-artifact refusal both key on the same value the caller sent.
- Anchors: the corpus `rasm.contracts.artifact.v1` `FetchRequest` definition and its reserved former field names; `rasm.contracts.artifact.fetch_responses` as the one frame projection; the repository custody entry at `mesh/daemon#DAEMON`; `docs/stacks/python/transport.md` `[GENERATED_VOCABULARY]` on generated classes as the only shape.
- Atomic: one override body, one refusal coordinate, and the card its predecessor vacated.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[SCENE_DESCRIPTOR_DECODE]-[COMPLETE]: decode fold landed at `energy/simulate#SIMULATE` — `SkySource`, `SceneLighting`, `SceneContext`, `SceneReceipt`, and `Simulation.scene` over `_derived_sun`/`_sky`/`_graded`/`_shades`/`_lighting`, with `EnergyFault` gaining `authored_sun` and `shading_fidelity` at `energy/climate`.
[GEOREF_BAND]-[COMPLETE]: extraction landed as `IfcAnalysis.georeference` minting the eight-field `GeoreferenceFact` (roster verified arm-for-arm against the data decoder, `crs:unnamed` covering the IFC2X3 pset path that reaches the fold with no `IfcProjectedCRS`), authoring as the four georeference `AuthorVerb` rows with `edit_true_north` excluded as `GEOREF_EDIT`'s own `coordinate_operation` keys; the seam edge mirrored onto the `ARCHITECTURE.md` sibling map from the `Ifc` node.
[NONRIGID_ARM]-[COMPLETE]: `NONRIGID` mode row, sealed `DeformationField` carrier, parent-side `find_spec` gate onto `BoundaryFault(import_=)`, and the deviation `DeformationSplit` landed; the floor-gating tension resolved as stated law, and the seeding ruling gained its one stated pre-pose exception because `probreg` publishes no init-transform slot.
[TABULAR_ROUNDTRIP_VERBS]-[COMPLETE]: the `EXPORT`/`IMPORT` pair landed on the lifecycle phase vocabulary — the catalog's single-EXCHANGE pre-spec split in two because the arms diverge on admission shape, mutation posture, transaction fence, and residual source; re-import runs inside the authoring transaction fence and `BLIND_KEYS` makes the written-cell census exact.
[GLB_STORE_SPILL]-[COMPLETE]: injected `ObjectStoreLane` holds two write-once objects per unit, reads through before the kernel, and carries generated `Spill` on the crossing receipt.
[GLB_SERVE_READTHROUGH]-[COMPLETE]: `GeometryServe.fetch` streams from the one repository through the shared artifact helper; only typed absence reaches the unknown-artifact fault, resident bytes are rehashed against the requested SHA-256 before success, and no raw body, eager frame block, or second ring truth exists.
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
