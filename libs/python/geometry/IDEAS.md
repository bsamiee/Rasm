# [PY_GEOMETRY_IDEAS]

Forward pool of higher-order concepts for `geometry`, grounded in the host-free companion role. Each idea is a card — slug leader with the capability, what it unlocks, and the gap or technique it draws on — and spawns one or more tasks in `TASKLOG.md`. `[1]-[OPEN]` holds live concepts; `[2]-[CLOSED]` records dispositions so an idea is never re-litigated.

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

[GLB_ARTIFACT_STORE]-[QUEUED]: durable content-addressed tessellation store over the branch object-store substrate.
- Capability: daemon GLB results persist beyond process life under the existing cache-seed content key, so a warm restart, a fleet peer, or a re-serve of an unchanged model never re-tessellates.
- Shape: one spill/read-through band on the daemon cache fold — `obstore` `from_url`-dispatched `ObjectStore` get/put with conditional put on the content key, `universal-pathlib`/`fsspec` carrying the local tier — and a serve read-through row answering `ArtifactSync` from the store before waking the daemon; lands on `libs/python/geometry/.planning/mesh/daemon.md` and `libs/python/geometry/.planning/mesh/serve.md`.
- Unlocks: cross-process and cross-host tessellation reuse; cold-start latency drops to a store read; the C# rail replays unchanged models with zero Python recompute.
- Anchors: daemon cache-seed byte fold already minting the content key; branch `libs/python/.api/obstore.md` typed store constructors, conditional get/put, and fsspec adapter; `libs/python/.api/universal-pathlib.md`; `libs/python/.api/fsspec.md`; runtime `ContentKey`.
- Tension: store identity and credentials are app-composition inputs — the band takes a configured store handle as a parameter, never a process-global default, per the app-neutrality law.

[IFC_GEOREFERENCE]-[QUEUED]: IFC georeference band — map conversion and CRS as first-class evidence.
- Capability: extraction and authoring of `IfcMapConversion`/`IfcProjectedCRS` — CRS name, local-to-map transform, true north — so a model pins to the earth and site-local scan geometry lifts to map coordinates.
- Shape: one extraction projection on the analysis page and one authoring verb row on the authoring script minting and updating georeference entities over `ifcopenshell.util.geolocation`; lands on `libs/python/geometry/.planning/ifc/analysis.md` and `libs/python/geometry/.planning/ifc/authoring.md`.
- Unlocks: scan-vs-model in shared map frames; geo-data planes — python data vector claims, C# Bim geospatial — consume one geometry-minted georeference fact instead of re-deriving it.
- Anchors: `ifcopenshell.util.geolocation` transform helpers; folder `libs/python/geometry/.api/ifcopenshell.md` as the catalog the member pin repairs into; data `spatial/geospatial.md` pyproj CRS plane as the consuming counterpart; the estate geospatial root move.
- Tension: geolocation member spellings are uncataloged in the folder tier — the member-pin task verifies them against the installed distribution before fences land.

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

[DAYLIGHTING_SCENE_DESCRIPTOR]-[QUEUED]: host-minted daylighting scene descriptors decode into the energy simulation plane.
- Capability: the C#-minted content-keyed scene descriptor — `SunState` astronomy, photometric light roster with distribution payloads, GLB shading tessellation — decodes into the building-model admission so radiation, shading, and daylight-autonomy studies run from the live host scene with zero host dependency.
- Shape: a descriptor decode fold ahead of the `translate` pair on `libs/python/geometry/.planning/energy/simulate.md` — sun state onto the climate owner's solar vocabulary at `libs/python/geometry/.planning/energy/climate.md`, light roster onto the model admission at `libs/python/geometry/.planning/energy/model.md`, GLB shading through the standing tessellation rail; daylight recipes ride the existing `RecipeName` row shape, results returning as the page's self-describing frames keyed by the descriptor's content identity.
- Unlocks: closed-loop solar and daylight studies from the live Rhino scene; the estate scene-descriptor vocabulary gains its first consumer.
- Anchors: `energy/simulate.md` recipe binding and frame discipline; `energy/climate.md` solar vocabulary; the content-keyed crossing law and the GLB rail.
- Tension: descriptor schema is the shared wire owner's mint — this plane decodes it and never widens it; tessellation-fidelity policy arrives as a descriptor axis, never a local default.
- Ripple: `libs/.planning` `[DAYLIGHTING_SCENE_DESCRIPTOR]`.

## [02]-[CLOSED]

<!-- source-only: closed idea card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[EVIDENCE_TRACE_LINKS]-[COMPLETE]: landed as the optional W3C composite mapping on `GeometryHandoff.of`/`_trace`/`wire()` and the `evidence_run` `upstream` decode folding one `Link` at `_linked` on `libs/python/geometry/.planning/graduation.md`; deviation's `evaluate` threads the mapping on `libs/python/geometry/.planning/scan/deviation.md`, and compute co-ships `_GeometryWire.trace` on `libs/python/compute/.planning/graduation/handoff.md`.
[KERNEL_BENCH_FAMILY]-[COMPLETE]: landed as graduation `bench_seam`/`bench_terminal` with entry-seam bench folds on `GeometryServe.bench` (`libs/python/geometry/.planning/mesh/serve.md`), repair/brep `benched`, and the cloud-size-keyed `ScanRegistration.bench`/`ScanReconstruction.bench`.
[KERNEL_COST_LEDGER]-[COMPLETE]: landed as the `EvidenceCost` psutil `oneshot` bracket inside `evidence_run` on `libs/python/geometry/.planning/graduation.md` — span facts, `UNIVERSAL_MEASURES` record under `domain="geometry"`, one `rasm.geometry.evidence` cost receipt, and the `_priced` subject rename.
[KERNEL_PROFILE_EVIDENCE]-[DROPPED]: runtime `Kernel.of` already mints `Kernel.name`, and `traced_kernel` passes that exact subject to `Profiles.phase` on `libs/python/runtime/.planning/execution/workers.md`; a geometry `PROFILE_SUBJECTS` registry duplicates the worker owner.
[DASHBOARD_CHARTER]-[COMPLETE]: landed as `MeasureRow`/`UNIVERSAL_MEASURES`/`CHARTER`/`charter_of`/`charter_record` on `libs/python/geometry/.planning/graduation.md`; the ts iac compile-leg decode stays the iac counterpart's deferral.
[PRODUCER_DISTRIBUTIONS]-[COMPLETE]: landed as `charter_record` producing-fold calls on deviation `_distributed`, quality `_metrics_outcome`, and the simulate eui decode; the runtime `INSTRUMENTS` counterpart rows stay the metrics owner's deferral.
[ANALYTIC_FRAME_EGRESS]-[COMPLETE]: landed as the `EvidenceFrame` port on `libs/python/geometry/.planning/graduation.md` with frame rows on deviation, quality, structural, costing, `AnalyticValue.tabled`, and the features board `frame`.
[MID_OPERATION_PULSE]-[COMPLETE]: landed as `GeometryPulse`/`PulseBeat`/`pulse_points` on `libs/python/geometry/.planning/graduation.md` with `pulsed` kernel beats on daemon tessellation, registration solve/edges, and reconstruction clusters over the runtime lane conduit.
