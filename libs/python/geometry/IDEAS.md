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

[IFC_GEOREFERENCE]-[QUEUED]: IFC georeference band — map conversion and CRS as first-class evidence.
- Capability: extraction and authoring of `IfcMapConversion`/`IfcProjectedCRS` — CRS name, local-to-map transform, true north — so a model pins to the earth and site-local scan geometry lifts to map coordinates.
- Shape: one extraction projection on the analysis page and one authoring verb row on the authoring script minting and updating georeference entities over `ifcopenshell.util.geolocation`; lands on `libs/python/geometry/.planning/ifc/analysis.md` and `libs/python/geometry/.planning/ifc/authoring.md`.
- Unlocks: scan-vs-model in shared map frames; geo-data planes — python data vector claims, C# Bim geospatial — consume one geometry-minted georeference fact instead of re-deriving it.
- Anchors: `ifcopenshell.util.geolocation` transform helpers; folder `libs/python/geometry/.api/ifcopenshell.md` as the catalog the member pin repairs into; data `spatial/geospatial.md` pyproj CRS plane as the consuming counterpart; the estate geospatial root move.
- Tension: `get_helmert_transformation_parameters` collapses `IfcMapConversion`, `IfcMapConversionScaled`, `IfcRigidOperation`, and the IFC2X3 pset fallback onto ONE nine-field transform and answers absence rather than raising, so the band's real bet is that an ungeoreferenced model is the identity crossing and not a refusal — a scan-to-map lift over that identity silently reports site-local coordinates as map coordinates.

[NONRIGID_DEFORMATION_TRACK]-[QUEUED]: probabilistic non-rigid registration for deformation-aware scan verification.
- Capability: CPD/FilterReg non-rigid alignment yields a per-point deformation field, so scan-vs-model verification distinguishes construction deviation from structural deformation instead of reporting one rigid residual.
- Shape: one non-rigid arm on the registration mode vocabulary returning a deformation-field carrier, and a deviation projection splitting rigid residual from deformation magnitude; lands on `libs/python/geometry/.planning/scan/registration.md` and `libs/python/geometry/.planning/scan/deviation.md`.
- Unlocks: monitoring-grade evidence — settlement, deflection, bowing — from repeat scans; completes the registration family beside the global `kiss-matcher`, coarse `open3d`, and fine `small-gicp` arms.
- Anchors: `probreg` CPD/FilterReg/SVR over the standing `Cloud` array carrier; the multi-scale registration session shape; `REGISTRATION_TRANSFORM`/`SCAN_DEVIATION` subjects absorbing the new evidence.
- Tension: `probreg` rides an interpreter marker in the root manifest — the non-rigid fences stay floor-gated like every native-gated provider.

[LIFECYCLE_TABULAR_EXCHANGE]-[QUEUED]: spreadsheet round-trip completing the lifecycle exchange set.
- Capability: quantity, cost, and schedule tables round-trip IFC to CSV/ODS/XLSX and back — export for estimator review, re-import of edited attribute and Pset values — beside the ifc5d/ifc4d rollups.
- Shape: one exchange verb pair on the lifecycle owner over `ifccsv` with the selector grammar scoping exported elements; lands on `libs/python/geometry/.planning/ifc/costing.md`.
- Unlocks: estimator and scheduler workflows without a BIM authoring tool in the loop; completes the ifcopenshell exchange family — `ifcdiff`, `ifcpatch`, `ifc4d`, `ifc5d` — per the set-completion law.
- Anchors: the admitted root-manifest `ifccsv` row; `libs/python/geometry/.planning/ifc/selector.md` grammar as the scoping input; the costing partition vocabulary.

[IFC_CENTERLINE_PROFILE]-[QUEUED]: the centre-line profile family joins the section-property roster through an offset ring constructor.
- Capability: `IfcCenterLineProfileDef` — an `IfcArbitraryOpenProfileDef` centreline offset to its `Thickness` — evaluates to a closed ring pair, so the one IFC4 profile family the landed roster still refuses by name gains section properties.
- Shape: one polyline-offset ring constructor beside the parametric sampler rows on `libs/python/geometry/.planning/ifc/structural.md`, carrying the offset algebra and its degenerate-self-intersection bound.
- Unlocks: cold-formed and plate profiles graded on the same `SECTION_PROPERTY` rail as the parametric and arbitrary families; the profile roster closes over the IFC4 schema set.
- Anchors: the `PROFILE_SAMPLERS` row grammar and `_sample`'s named-refusal fall-through; `sectionproperties` `CompoundGeometry.from_points`; the structural Growth line naming the family.
- Tension: a centreline whose offset exceeds the local turning radius self-intersects — the constructor must refuse that bound by name rather than emit a bowtie ring.

[DAYLIGHTING_SCENE_DESCRIPTOR]-[BLOCKED]: capture-produced daylighting scene descriptors decode into the energy simulation plane.
- Capability: the capture-produced content-keyed scene descriptor — `SunState` astronomy, photometric light roster with distribution payloads, GLB shading tessellation — decodes into the building-model admission so radiation, shading, and daylight-autonomy studies run from the live host scene with zero host dependency.
- Shape: a descriptor decode fold ahead of the `translate` pair on `libs/python/geometry/.planning/energy/simulate.md` — sun state onto the climate owner's solar vocabulary at `libs/python/geometry/.planning/energy/climate.md`, light roster onto the model admission at `libs/python/geometry/.planning/energy/model.md`, GLB shading through the standing tessellation rail; daylight recipes ride the existing `RecipeName` row shape, results returning as the page's self-describing frames keyed by the descriptor's content identity.
- Unlocks: closed-loop solar and daylight studies from the live Rhino scene; the estate scene-descriptor vocabulary gains its first consumer.
- Anchors: `energy/simulate.md` recipe binding and frame discipline; `energy/climate.md` solar vocabulary; the content-keyed crossing law and the GLB rail.
- Arms: the descriptor schema lands in the shared wire vocabulary at `libs/.planning` `[SCENE_DESCRIPTOR_SCHEMA]`; until it does this plane has no field authority to decode against and every column would be a local invention the schema then contradicts.
- Tension: descriptor schema is the shared wire owner's mint — this plane decodes it and never widens it; tessellation-fidelity policy arrives as a descriptor axis, never a local default.
- Ripple: `libs/.planning` `[DAYLIGHTING_SCENE_DESCRIPTOR]`.

## [02]-[CLOSED]

<!-- source-only: closed idea card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[GLB_ARTIFACT_STORE]-[COMPLETE]: landed as the injected `ObjectStoreLane` durable tier on `libs/python/geometry/.planning/mesh/daemon.md` — two write-once objects per unit under one `spill_path` derivation, a read-through ahead of the kernel whose every failure folds to absence, `SpillOutcome` on the crossing receipt, and `_phase` deriving replay provenance across both reuse tiers — with the serve-side `ArtifactSync` read-through and its unknown-artifact collapse on `libs/python/geometry/.planning/mesh/serve.md`.
[EVIDENCE_TRACE_LINKS]-[COMPLETE]: landed as the optional W3C composite mapping on `GeometryHandoff.of`/`_trace`/`wire()` and the `evidence_run` `upstream` decode folding one `Link` at `_linked` on `libs/python/geometry/.planning/graduation.md`; deviation's `evaluate` threads the mapping on `libs/python/geometry/.planning/scan/deviation.md`, and compute co-ships `_GeometryWire.trace` on `libs/python/compute/.planning/graduation/handoff.md`.
[KERNEL_BENCH_FAMILY]-[COMPLETE]: landed as graduation `bench_seam`/`bench_terminal` with entry-seam bench folds on `GeometryServe.bench` (`libs/python/geometry/.planning/mesh/serve.md`), repair/brep `benched`, and the cloud-size-keyed `ScanRegistration.bench`/`ScanReconstruction.bench`.
[KERNEL_COST_LEDGER]-[COMPLETE]: landed as the `EvidenceCost` ledger over the runtime `Cost` substrate bracket inside `evidence_run` on `libs/python/geometry/.planning/graduation.md` — span facts carrying cpu, rss, io, and switches, `UNIVERSAL_MEASURES` record under `domain="geometry"`, one `rasm.geometry.evidence` cost receipt, and the `_priced` subject rename.
[KERNEL_PROFILE_EVIDENCE]-[DROPPED]: runtime `Kernel.of` already mints `Kernel.name`, and `traced_kernel` passes that exact subject to `Profiles.phase` on `libs/python/runtime/.planning/execution/workers.md`; a geometry `PROFILE_SUBJECTS` registry duplicates the worker owner.
[DASHBOARD_CHARTER]-[COMPLETE]: landed as `MeasureRow`/`UNIVERSAL_MEASURES`/`CHARTER`/`charter_of`/`charter_record` on `libs/python/geometry/.planning/graduation.md`; the ts iac compile-leg decode stays the iac counterpart's deferral.
[PRODUCER_DISTRIBUTIONS]-[COMPLETE]: landed as `charter_record` producing-fold calls on deviation `_distributed`, quality `_metrics_outcome`, the simulate eui decode, registration `_distributed`, and structural `_distributed`, so no charter row is orphaned; the runtime `INSTRUMENTS` counterpart rows stay the metrics owner's ripple, proven at composition by the geometry `registered` census gate.
[ANALYTIC_FRAME_EGRESS]-[COMPLETE]: landed as the `EvidenceFrame` port with its `crossing()` Arrow fold on `libs/python/geometry/.planning/graduation.md`, with frame rows on deviation, quality, structural, costing, analysis, and all three graph producers — features and nonmanifold off `AnalyticValue.tabled`, algebra off its own census row.
[MID_OPERATION_PULSE]-[COMPLETE]: landed as `GeometryPulse`/`PulseBeat` registered through the folder's one `registered(composition)` composition leg on `libs/python/geometry/.planning/graduation.md`, with `pulsed` kernel beats on daemon tessellation, registration solve/edges, and reconstruction clusters over the runtime lane conduit.
