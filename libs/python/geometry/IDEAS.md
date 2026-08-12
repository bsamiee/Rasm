# [PY_GEOMETRY_IDEAS]

Geometry ideas extend the standalone host-free geometry and IFC platform; folder tasks carry promoted work.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis — the capability outcome, never the landing motion>.
- Capability: <the higher-order invariant, owner capability, or concept established — concept grain only, never a page path, row list, or member spelling>.
- Shape: <where the work lands and at what grain — repo-relative page with section/row, or a new-page path; the concrete surface, so Capability never names it>.
- Unlocks: <the downstream capability at the consumer grain — a task narrows its parent idea's Unlocks to THIS slice as `IDEAS.md [SLUG] — consequence`; a set-completion card states the completeness bar that is its acceptance contract>.
- Anchors: <owners, seams, packages, catalogs, doctrines, and techniques making the work plausible — anchors, never procedures>.
- Arms: <BLOCKED or gated cards only; the exact observable flipping it actionable — catalog row landing, member query evidence, package admitted>.
- Route: <present only on a probe, research, or member-pin card; the ordered verification path run before any fence lands>.
- Tension: <only when an unresolved constraint, boundary, or bet shapes the work — the genuine bet, never the arming condition Arms carries>.
- Ripple: <counterpart — cross-folder `pkg` `[SLUG]`, same-folder prerequisite `[SLUG]`; load-bearing build order prefixes follows/precedes/mirrors>.
Capability, Shape, Unlocks, and Anchors are required on every open card; statuses closed — `ACTIVE|QUEUED|BLOCKED` open, `COMPLETE|DROPPED` closed; IDs are SEMANTIC UPPERCASE_SNAKE slugs carrying meaning — never numeric (`[0007]`-class NNNN IDs are a defect), for cards AND research tokens alike; a hyphenated slug anywhere is a defect; repo-relative paths only. Design pages carry the terminal `[RESEARCH]` section always — `(none)` marks empty, absence is an error. Ideas state higher-order concepts, never landing-grain tasks.
-->

(none)

## [02]-[CLOSED]

<!-- source-only: closed idea card template:
[DAYLIGHTING_SCENE_DESCRIPTOR]-[COMPLETE]: arms fired — `[SCENE_DESCRIPTOR_SCHEMA]` closed as the `rasm.scene.v1` family, so `energy/simulate#SIMULATE` decodes the pinned descriptor into shade meshes, a point-in-time sky, and an authority-ranked light roster, grading declared fidelity against honeybee's own converted tolerance floor.
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[IFC_GEOREFERENCE]-[COMPLETE]: landed as the eight-field `GeoreferenceFact` producer `IfcAnalysis.georeference` on `libs/python/geometry/.planning/ifc/analysis.md` — absence crosses as `Ok(Nothing)`, never an identity transform, and a non-uniform `factor_*` triple or unnamed CRS refuses by name — with the four georeference `AuthorVerb` rows (`GEOREF_ADD`/`GEOREF_EDIT`/`GEOREF_REMOVE`/`WCS`) on `ifc/authoring.md` and the `[SHAPE]: GeoreferenceFact` seam edge re-pointed from `Scan` to `Ifc` on `ARCHITECTURE.md`; the identity-crossing tension resolved to typed absence, so no lift reads site-local coordinates as map coordinates, and the roster verified arm-for-arm against the data decoder.
[NONRIGID_DEFORMATION_TRACK]-[COMPLETE]: landed as the `NONRIGID` `RegistrationMode` row with `NonRigidEngine` policy coordinates (CPD `tf_type_name="nonrigid"`, FilterReg `objective_type`) and the sealed `DeformationField` carrier on `scan/registration.md`, split at `DeformationSplit` on `scan/deviation.md`; `probreg` publishes no seed slot, so the arm pre-poses once at its own admission — the one stated exception on the folder seeding ruling.
[LIFECYCLE_TABULAR_EXCHANGE]-[COMPLETE]: landed as the `EXPORT`/`IMPORT` phase pair on `ifc/costing.md` — selector-gated column resolve with no handle across the seam, re-import inside the authoring transaction fence, `BLIND_KEYS` naming the provider's count/material write-back hole — with `.api/ifccsv.md` repaired to source-verified truth; the exchange family closes beside `ifcdiff`, `ifcpatch`, `ifc4d`, and `ifc5d`.
[IFC_CENTERLINE_PROFILE]-[COMPLETE]: landed as the `_centered`/`_centerline` offset-ring arm on `ifc/structural.md` — miter-joined ±`Thickness`/2 offset with `centreline-reversal` and `centreline-offset-self-intersects` refused by name — beside the widened `_curve` ordering `IfcLineIndex` runs and tessellating `IfcArcIndex` arcs under `CIRCLE_SEGMENTS`, so the profile roster closes over the IFC4 schema set.
[GLB_ARTIFACT_STORE]-[COMPLETE]: landed as the injected `ObjectStoreLane` durable tier on `libs/python/geometry/.planning/mesh/daemon.md` — two write-once objects per unit under one `spill_path` derivation, a read-through ahead of the kernel whose every failure folds to absence, `SpillOutcome` on the crossing receipt, and `_phase` deriving replay provenance across both reuse tiers — with the serve-side `ArtifactSyncService` read-through and its unknown-artifact collapse on `libs/python/geometry/.planning/mesh/serve.md`.
[EVIDENCE_TRACE_LINKS]-[COMPLETE]: landed as the optional W3C composite mapping on `GeometryHandoff.of`/`_trace`/`wire()` and the `evidence_run` `upstream` decode folding one `Link` at `_linked` on `libs/python/geometry/.planning/graduation.md`; deviation's `evaluate` threads the mapping on `libs/python/geometry/.planning/scan/deviation.md`, and compute co-ships `_GeometryWire.trace` on `libs/python/compute/.planning/graduation/handoff.md`.
[KERNEL_BENCH_FAMILY]-[COMPLETE]: landed as graduation `bench_seam`/`bench_terminal` with entry-seam bench folds on `GeometryServe.bench` (`libs/python/geometry/.planning/mesh/serve.md`), repair/brep `benched`, and the cloud-size-keyed `ScanRegistration.bench`/`ScanReconstruction.bench`.
[KERNEL_COST_LEDGER]-[COMPLETE]: landed as the `EvidenceCost` ledger over the runtime `Cost` substrate bracket inside `evidence_run` on `libs/python/geometry/.planning/graduation.md` — span facts carrying cpu, rss, io, and switches, `UNIVERSAL_MEASURES` record under `domain="geometry"`, one `rasm.geometry.evidence` cost receipt, and the `_priced` subject rename.
[KERNEL_PROFILE_EVIDENCE]-[DROPPED]: runtime `Kernel.of` already mints `Kernel.name`, and `traced_kernel` passes that exact subject to `Profiles.phase` on `libs/python/runtime/.planning/execution/workers.md`; a geometry `PROFILE_SUBJECTS` registry duplicates the worker owner.
[DASHBOARD_CHARTER]-[COMPLETE]: landed as `MeasureRow`/`UNIVERSAL_MEASURES`/`CHARTER`/`charter_of`/`charter_record` on `libs/python/geometry/.planning/graduation.md`; the ts iac compile-leg decode stays the iac counterpart's deferral.
[PRODUCER_DISTRIBUTIONS]-[COMPLETE]: landed as `charter_record` producing-fold calls on deviation `_distributed`, quality `_metrics_outcome`, the simulate eui decode, registration `_distributed`, and structural `_distributed`, so no charter row is orphaned; the runtime `INSTRUMENTS` counterpart rows stay the metrics owner's ripple, proven at composition by the geometry `registered` census gate.
[ANALYTIC_FRAME_EGRESS]-[COMPLETE]: landed as the `EvidenceFrame` port with its `crossing()` Arrow fold on `libs/python/geometry/.planning/graduation.md`, with frame rows on deviation, quality, structural, costing, analysis, and all three graph producers — features and nonmanifold off `AnalyticValue.tabled`, algebra off its own census row.
[MID_OPERATION_PULSE]-[COMPLETE]: landed as `GeometryPulse`/`PulseBeat` registered through the folder's one `registered(composition)` composition leg on `libs/python/geometry/.planning/graduation.md`, with `pulsed` kernel beats on daemon tessellation, registration solve/edges, and reconstruction clusters over the runtime lane conduit.
