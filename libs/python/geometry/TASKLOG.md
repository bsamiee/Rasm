# [PY_GEOMETRY_TASKLOG]

Open and closed work for `geometry`, distilled from `IDEAS.md`. Each task card leads with `[ID]-[STATUS]: thesis` and carries `Capability`, `Shape`, `Unlocks`, `Anchors`, and optional `Tension` fields. `[1]-[OPEN]` holds live work; `[2]-[CLOSED]` records finished or dropped tasks.

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

[GLB_STORE_SPILL]-[QUEUED]: map the daemon cache fold onto a content-addressed object-store spill.
- Capability: pin the `obstore` members the spill band composes — `from_url` store dispatch, get/put, conditional put, the fsspec adapter — and design the store-key layout off the existing daemon cache-seed content key on `libs/python/geometry/.planning/mesh/daemon.md`.
- Shape: one content-addressed spill band on the daemon cache fold of `libs/python/geometry/.planning/mesh/daemon.md` — `obstore` `from_url` store dispatch, get/put, conditional put on the cache-seed content key, the `fsspec`/`universal-pathlib` local tier.
- Unlocks: IDEAS.md [GLB_ARTIFACT_STORE] — cross-process and cross-host tessellation reuse, cold-start latency dropping to a store read.
- Anchors: branch `libs/python/.api/obstore.md`; `libs/python/.api/universal-pathlib.md`; the daemon cache-seed byte fold; idea `[GLB_ARTIFACT_STORE]`.

[GLB_SERVE_READTHROUGH]-[QUEUED]: land the serve read-through row against the artifact store.
- Capability: serve answers unchanged models from the durable store before waking the daemon, the store handle arriving as a composition parameter.
- Shape: one read-through row on `libs/python/geometry/.planning/mesh/serve.md` where `ArtifactSync` answers from the store on a content-key hit before waking the daemon, the store handle threaded as a servicer parameter.
- Unlocks: IDEAS.md [GLB_ARTIFACT_STORE] — warm-restart and re-serve of an unchanged model answer from the store, the C# rail replaying with zero Python recompute.
- Anchors: serve streaming fold; runtime `ContentKey`; idea `[GLB_ARTIFACT_STORE]`.
- Atomic: one read-through row and its parameter thread.

[GEOREF_MEMBER_PIN]-[QUEUED]: pin the `ifcopenshell.util.geolocation` member spellings.
- Capability: verify the geolocation transform helpers against the installed distribution and repair `libs/python/geometry/.api/ifcopenshell.md` with the confirmed members before any fence lands.
- Shape: verified `ifcopenshell.util.geolocation` transform members pinned into `libs/python/geometry/.api/ifcopenshell.md` against the installed distribution, catalog pin only, no page fences.
- Unlocks: IDEAS.md [IFC_GEOREFERENCE] — confirmed geolocation member spellings gate the georeference extraction and authoring fences before any lands.
- Anchors: installed `ifcopenshell` distribution; the folder catalog repair law; idea `[IFC_GEOREFERENCE]`.
- Atomic: catalog member pin only, no page fences.

[GEOREF_BAND]-[QUEUED]: land the georeference extraction and authoring rows.
- Capability: georeference truth becomes first-class evidence — extraction mints the CRS fact and authoring writes it back into the model.
- Shape: one extraction projection minting CRS, local-to-map transform, and true north on `libs/python/geometry/.planning/ifc/analysis.md`, and one authoring verb row minting and updating `IfcMapConversion`/`IfcProjectedCRS` on `libs/python/geometry/.planning/ifc/authoring.md`.
- Unlocks: IDEAS.md [IFC_GEOREFERENCE] — scan-vs-model in shared map frames, geo-data planes consuming one geometry-minted georeference fact instead of re-deriving it.
- Anchors: pinned geolocation members from `[GEOREF_MEMBER_PIN]`; the authoring verb table; idea `[IFC_GEOREFERENCE]`.

[NONRIGID_ARM]-[QUEUED]: land the non-rigid registration arm and the deformation split.
- Capability: registration distinguishes construction deviation from structural deformation through a per-point deformation field.
- Shape: one `probreg` CPD/FilterReg arm on the registration mode vocabulary returning a deformation-field carrier on `libs/python/geometry/.planning/scan/registration.md`, and a deviation projection splitting rigid residual from deformation magnitude on `libs/python/geometry/.planning/scan/deviation.md`.
- Unlocks: IDEAS.md [NONRIGID_DEFORMATION_TRACK] — monitoring-grade evidence (settlement, deflection, bowing) from repeat scans, completing the registration family beside the global, coarse, and fine arms.
- Anchors: the `Cloud` array carrier crossing worker seams; the registration session shape; the admitted root-manifest `probreg` row; idea `[NONRIGID_DEFORMATION_TRACK]`.
- Tension: `probreg` rides an interpreter marker in the root manifest, so the arm's fences stay floor-gated like every native-gated provider.

[FPS_DOWNSAMPLE_ROW]-[QUEUED]: land the farthest-point downsample policy row on ingestion.
- Capability: geometry-uniform downsampling bounds the non-rigid arm's point budget at ingestion.
- Shape: one `PointCloud.farthest_point_down_sample` policy row on the ingestion filter-stage tables of `libs/python/geometry/.planning/scan/ingestion.md`, geometry-uniform downsample bounding the non-rigid arm's point budget.
- Unlocks: IDEAS.md [NONRIGID_DEFORMATION_TRACK] — a bounded point budget for the non-rigid registration arm through geometry-uniform downsampling.
- Anchors: `libs/python/geometry/.api/open3d.md` member row; the ingestion filter-stage tables; idea `[NONRIGID_DEFORMATION_TRACK]`.
- Atomic: one policy row on the ingestion stage table.

[TABULAR_ROUNDTRIP_VERBS]-[QUEUED]: land the spreadsheet exchange verb pair on the lifecycle owner.
- Capability: lifecycle tables round-trip to estimator spreadsheets and back through the authoring rail.
- Shape: one `ifccsv` export/re-import verb pair on the lifecycle verb table of `libs/python/geometry/.planning/ifc/costing.md`, selector-grammar scoped, the re-import writing attribute and Pset edits back through the authoring rail.
- Unlocks: IDEAS.md [LIFECYCLE_TABULAR_EXCHANGE] — estimator and scheduler workflows without a BIM authoring tool in the loop, completing the ifcopenshell exchange family beside `ifcdiff`, `ifcpatch`, `ifc4d`, and `ifc5d`.
- Anchors: the admitted root-manifest `ifccsv` row; `ifc/selector` grammar; the costing partition vocabulary; idea `[LIFECYCLE_TABULAR_EXCHANGE]`.
- Atomic: one verb pair on the lifecycle verb table.

[ENERGY_GRAPH_TYPED_FAULTS]-[QUEUED]: energy and graph refusals join the typed fault vocabulary.
- Capability: every domain refusal crosses the converting fence as a structured fault whose kwargs survive into the boundary fault.
- Shape: `EnergyFault` and `GraphFault` structured cases replace the bare `ValueError` f-strings on `libs/python/geometry/.planning/energy/model.md`, `energy/district.md`, `energy/simulate.md`, and `graph/analytic.md`, matching the `BrepFault`/`QualityFault`/`RepairFault`/`DeviationFault` shape.
- Unlocks: the typed-refusal ruling realized; the mesh pages' fence comment stops being contradicted by peers.
- Anchors: the typed-refusal ruling at `libs/python/geometry/RULINGS.md`; the mesh fault classes; the `evidence_run` converting fence.

[IFC_HEAD_FENCE_ALIGN]-[QUEUED]: every ifc capsule shares the one dispatch fence.
- Capability: peer capsules fence identically — the contract guard sits on the dispatch seam and the public head stays bare.
- Shape: the plain `@beartype` on `run` in `libs/python/geometry/.planning/ifc/analysis.md` is deleted, leaving `@beartype(conf=FAULT_CONF)` on `_dispatch` as structural and costing already hold.
- Unlocks: the capsule-fencing ruling realized; one fence idiom across the ifc plane.
- Anchors: the capsule-fencing ruling at `libs/python/geometry/RULINGS.md`; the peer `_dispatch` fences.
- Atomic: one decorator deletion.

[COST_BRACKET_COMPOSE]-[QUEUED]: evidence costing composes the substrate bracket.
- Capability: geometry samples process cost through the one runtime owner instead of a folder-local psutil bracket.
- Shape: `_sampled` and the `EvidenceCost.of` delta on `libs/python/geometry/.planning/graduation.md` compose runtime `Cost.sampled`/`Cost.delta`; the wall-clock `perf_counter` timing and the `rasm.geometry.evidence.*` charter names stay folder-local.
- Unlocks: one honest-RSS band and one sampling fix point; the folder `_PROCESS` handle dies.
- Anchors: runtime `Cost` on `libs/python/runtime/.planning/observability/receipts.md`; the substrate-bracket ruling at `libs/python/.planning/RULINGS.md`.
- Atomic: one bracket substitution on one page.

[CHARTER_UNIT_SINGLE_WRITER]-[QUEUED]: measure units stay single-writer at the instrument owner.
- Capability: a measure's UCUM unit is authored once at the runtime instrument row, the geometry charter carrying only the aggregation and source-field vocabulary it uniquely owns.
- Shape: `MeasureRow` on `libs/python/geometry/.planning/graduation.md` drops its `unit` column; charter consumers derive the unit from the runtime `INSTRUMENTS` row by measure name.
- Unlocks: the estate single-writer measure law holds on the unit axis; the forced dual edit dies.
- Anchors: `InstrumentSpec.unit` on `libs/python/runtime/.planning/observability/metrics.md`; the geometry charter growth law.
- Ripple: follows `runtime` `[GEOMETRY_MEASURE_CHARTER]`.
- Atomic: one column drop and one derivation.

[ARCH_FLOOR_SENTENCE]-[QUEUED]: the codemap floor sentence matches the verb pages' caller-floor law.
- Capability: the architecture codemap states the worker lanes carry only the genuinely long native phases, matching every verb page's caller-floor default.
- Shape: one sentence tightening on `libs/python/geometry/ARCHITECTURE.md` — "the IFC core" narrows to the long native IFC phases behind function-local gates.
- Unlocks: an isolated codemap read no longer implies whole-core offload the verb pages refuse.
- Anchors: the caller-floor ruling at `libs/python/geometry/RULINGS.md`; the runtime-lane/worker-lane split sentence on the codemap.
- Atomic: one sentence on one index page.

[GMSH_REGISTRY_ALIGN]-[BLOCKED]: the gmsh registry tag flips when the compute generation arm lands.
- Capability: the folder registry reflects gmsh's branch role truthfully — deferred for geometry consumption, admitted at the branch for compute generation.
- Shape: one README registry row touch on `libs/python/geometry/README.md` re-annotating the gmsh row against the landed compute arm.
- Unlocks: registry truth for the shared mesher across the two charters.
- Anchors: the root-manifest `gmsh` row; the compute generation-arm card the flip follows.
- Arms: the compute `generate` route lands on `libs/python/compute/.planning/solvers/mesh.md`.
- Ripple: mirrors `compute` `[GMSH_GENERATE_ARM]`.
- Atomic: one registry row annotation.

[SCENE_DESCRIPTOR_DECODE]-[QUEUED]: land the descriptor decode fold on the energy plane.
- Capability: decode rows mapping descriptor bands to their owners — `SunState` onto the climate solar vocabulary, light roster onto the model admission, GLB shading through the tessellation rail — with the daylight `RecipeName` rows joining the simulate shape.
- Shape: rows on `libs/python/geometry/.planning/energy/simulate.md`, `libs/python/geometry/.planning/energy/climate.md`, and `libs/python/geometry/.planning/energy/model.md`.
- Unlocks: IDEAS.md [DAYLIGHTING_SCENE_DESCRIPTOR] — closed-loop solar and daylight studies from the live Rhino scene, the estate scene-descriptor vocabulary gaining its first consumer.
- Anchors: idea `[DAYLIGHTING_SCENE_DESCRIPTOR]`; the estate `[SCENE_DESCRIPTOR_SCHEMA]` schema pin as the field authority.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[KERNEL_BENCH_LANE]-[COMPLETE]: landed as `GeometryServe.bench` over the whole `_tessellate` entry (`rasm.geometry.mesh.serve.tessellate`) with repair and brep `benched` folds over their `apply` crossings, every row riding graduation `bench_seam` with zero geometry instrument rows.
[SCAN_BENCH_SUBJECTS]-[COMPLETE]: landed as `ScanRegistration.bench` and `ScanReconstruction.bench` keying `bench_seam` by mode/method and source point count (`...<mode>.p<points>`), whole-crossing rounds only.
[COST_WEAVE_FOLD]-[COMPLETE]: landed as the `EvidenceCost` `psutil` `oneshot` bracket (`_sampled`/`EvidenceCost.of`) closed by `_priced` inside `evidence_run` on `libs/python/geometry/.planning/graduation.md` — span facts, charter record, and one cost receipt.
[SPAN_SUBJECT_RENAME]-[COMPLETE]: landed as the `span.update_name(f"{operation}:{subject.value}")` arm inside graduation `_priced` on the cleared `GeometrySubject`-carrying value.
[PROFILE_SUBJECT_MAP]-[DROPPED]: runtime `Kernel.of` and `traced_kernel` already project `Kernel.name` through `Profiles.phase` on `libs/python/runtime/.planning/execution/workers.md`; no geometry subject map is admitted.
[CHARTER_MEASURE_TABLE]-[COMPLETE]: landed as `MeasureRow`/`UNIVERSAL_MEASURES`/`CHARTER` with `charter_of`/`charter_record` on `libs/python/geometry/.planning/graduation.md`, UCUM units and aggregations per row.
[DISTRIBUTION_ROWS]-[COMPLETE]: geometry record calls landed — deviation `_distributed`, quality `_metrics_outcome`, simulate eui `charter_record` — all deriving spellings from the charter; the runtime `INSTRUMENTS` counterpart rows stay the metrics owner's deferral.
[FRAME_SCHEMA_PORT]-[COMPLETE]: landed as the subject-keyed, content-keyed, numpy-backed `EvidenceFrame` carrier beside `wire()` on `libs/python/geometry/.planning/graduation.md`.
[FRAME_PRODUCER_ROWS]-[COMPLETE]: landed as `frame` rows on deviation (`DeviationResult.frame`), quality (`QualityMetrics.frame`), structural (`SectionReceipt.frame`), costing (`LifecycleReceipt.frame`), the analytic `tabled` projection, and the features board `frame` composing it.
[TRACE_LINK_WIRE_PROBE]-[COMPLETE]: verdict admitted — `GeometryHandoff.of`/`_trace`/`wire()` mint the optional `traceparent`/`tracestate`/baggage mapping on `libs/python/geometry/.planning/graduation.md`, and `_GeometryWire.trace`/`_linked` decode it on `libs/python/compute/.planning/graduation/handoff.md`; `[EVIDENCE_TRACE_LINKS]` closes against both fences.
[PULSE_DRAIN_PROBE]-[COMPLETE]: verdict landed on `libs/python/runtime/.planning/execution/lanes.md` — `LanePolicy.pulses` owns the spawn-context manager queue, structured `drain` custody starts and closes the actor, `anyio.from_thread.run_sync` relays into the single-consumer `Hooks.fire` fold, and `pulsed` stays lossy; `[MID_OPERATION_PULSE]` closed against the landed `GeometryPulse` rows and kernel beats.
