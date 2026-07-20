# [PY_GEOMETRY_IDEAS]

Forward pool of higher-order concepts for `geometry`, grounded in the host-free companion role. Each idea is a card — slug leader with the capability, what it unlocks, and the gap or technique it draws on — and spawns one or more tasks in `TASKLOG.md`. `[1]-[OPEN]` holds live concepts; `[2]-[CLOSED]` records dispositions so an idea is never re-litigated.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis>.
- Capability: <higher-order concept, invariant, or owner capability>.
- Shape: <what the idea becomes as a system, product, owner, or feature set(s)>.
- Unlocks: <new branch, package, workflow, proof, user, or agent capability made possible>.
- Anchors: <owners, seams, packages, doctrines, or techniques that make the idea plausible>.
- Tension: <only when an unresolved constraint, boundary, bet, or dependency shapes the idea>.
- Ripple: <origin/counterpart card this entry pairs with across folders, as `pkg` `[SLUG]`; present only on a cross-folder ripple counterpart card>.
-->

[KERNEL_BENCH_FAMILY]-[QUEUED]: kernel macro-benchmarks through the runtime bench runner across the mesh and scan planes.
- Capability: tessellation, repair, boolean, registration, and reconstruction kernels gain repeatable warmup-disciplined latency and throughput benchmarks beside the per-call `rasm.geometry.evidence.duration` histogram.
- Shape: one bench lane per kernel subject riding the runtime `Bench.run` latency/throughput modes; receipts stream through the standing contributor fold; a process-terminal bench run rides the runtime job envelope so the final projection flushes before exit; lands on `libs/python/geometry/.planning/mesh/serve.md`, `libs/python/geometry/.planning/mesh/repair.md`, `libs/python/geometry/.planning/mesh/brep.md`, `libs/python/geometry/.planning/scan/registration.md`, and `libs/python/geometry/.planning/scan/reconstruction.md`.
- Unlocks: kernel-regression evidence across mesh and scan library upgrades without a C# harness round-trip.
- Anchors: runtime `observability/profiles#BENCH` `Bench.run`/`BenchmarkReceipt`; the graduation `EvidenceScope` vocabulary keying subjects; the bench growth law absorbing new subjects with zero runtime edits.
- Tension: HOSTILE worker kernels bench through their entry seam, never in-kernel — the same boundary the mid-operation pulse card records.

[GLB_ARTIFACT_STORE]-[QUEUED]: durable content-addressed tessellation store over the branch object-store substrate.
- Capability: daemon GLB results persist beyond process life under the existing cache-seed content key, so a warm restart, a fleet peer, or a re-serve of an unchanged model never re-tessellates.
- Shape: one spill/read-through band on the daemon cache fold — `obstore` `from_url`-dispatched `ObjectStore` get/put with conditional put on the content key, `universal-pathlib`/`fsspec` carrying the local tier — and a serve read-through row answering `ArtifactSync` from the store before waking the daemon; lands on `libs/python/geometry/.planning/mesh/daemon.md` and `libs/python/geometry/.planning/mesh/serve.md`.
- Unlocks: cross-process and cross-host tessellation reuse; cold-start latency drops to a store read; the C# rail replays unchanged models with zero Python recompute.
- Anchors: daemon cache-seed byte fold already minting the content key; branch `libs/python/.api/obstore.md` typed store constructors, conditional get/put, and fsspec adapter; `libs/python/.api/universal-pathlib.md`; `libs/python/.api/fsspec.md`; runtime `ContentKey`.
- Tension: store identity and credentials are app-composition inputs — the band takes a configured store handle as a parameter, never a process-global default, per the app-neutrality law.

[KERNEL_COST_LEDGER]-[QUEUED]: resource-cost attribution on the evidence weave.
- Capability: every graduation crossing carries its compute cost — cpu-seconds, rss high-water delta, wall time — as receipt facts and a recorded cost measure, tenant-dimensioned with zero producer edits.
- Shape: one `psutil.Process.oneshot` before/after fold inside `evidence_run` beside the existing duration record, cost facts folded onto the crossing receipt and recorded through `Metrics.record` under `domain="geometry"` — tenant baggage rides the `_attributed` fold on the runtime metrics owner; lands on `libs/python/geometry/.planning/graduation.md`.
- Unlocks: per-subject, per-tenant chargeback evidence for the estate cost-attribution rail without a second projection surface.
- Anchors: branch `libs/python/.api/psutil.md` `Process.oneshot`/`cpu_times`/`memory_info`; runtime `observability/metrics.md` tenant fold; the cross-libs `[COST_ATTRIBUTION_BAGGAGE]` card this folder's facts feed.
- Tension: worker-side cost of a HOSTILE kernel is invisible to the parent process handle — the weave attributes parent-observed cost, and worker-floor cost rides the profile card's lane.

[KERNEL_PROFILE_EVIDENCE]-[QUEUED]: worker-floor continuous profiling correlated to the evidence spans.
- Capability: CPU flame evidence for the compiled kernels — tessellation, booleans, registration, reconstruction — at the floor where the folder's cycles burn: inside pool workers no parent-side profiler observes.
- Shape: per-kernel profile subjects keyed by the `EvidenceScope` vocabulary, worker-floor agent attach owned by the runtime lane spine, span-profile correlation so a slow `evidence_run` span clicks through to its worker flame; lands on `libs/python/geometry/.planning/graduation.md` and `libs/python/geometry/.planning/mesh/daemon.md`.
- Unlocks: kernel hot-path answers — which OCCT, manifold, or open3d call burns — from production traces, never local reruns.
- Anchors: runtime `observability/profiles.md` profile rail with its `pyroscope-otel` span link; `pyroscope-io` as the agent source the correlation requires; `KernelTrait.HOSTILE` entry seams as the attach points.
- Tension: agent attach is a lane-spine concern — geometry names subjects and seams, runtime owns worker bootstrap, so the counterpart card lands there.

[DASHBOARD_CHARTER]-[QUEUED]: geometry measure charter as typed data the estate dashboard compile leg consumes.
- Capability: one charter table naming every dashboard-worthy geometry measure — deviation magnitude, closure residual, mesh genus/aspect, registration fitness, EUI, bench latency — with unit, source receipt field, and aggregation, so instrument rows and dashboards derive from one owner.
- Shape: a charter table on the graduation page keyed by `GeometrySubject`, each row naming measure, UCUM unit, receipt field, and aggregation; the TS iac Foundation-SDK leg compiles geometry dashboards and alert rules from these rows; lands on `libs/python/geometry/.planning/graduation.md`.
- Unlocks: unblocks `[PRODUCER_DISTRIBUTIONS]`; geometry dashboards and alerts derive from charter data, never hand-picked measures.
- Anchors: wire-law `rasm.<domain>.<measure>` UCUM naming; runtime `observability/metrics.md` `INSTRUMENTS` table the rows project into; ts iac observe compile leg as the consuming counterpart.

[PRODUCER_DISTRIBUTIONS]-[QUEUED]: per-producer geometry distributions beside the one evidence-duration instrument.
- Capability: deviation magnitude, mesh genus/aspect, and EUI cross from receipt-only facts to backend histograms.
- Shape: one runtime `INSTRUMENTS` row per charter measure and one `Metrics.record` mapping-arm call at the producing fold — no second projection rail; sequenced after `[DASHBOARD_CHARTER]` names the measures; lands on `libs/python/geometry/.planning/graduation.md` with record calls on `libs/python/geometry/.planning/scan/deviation.md`, `libs/python/geometry/.planning/mesh/quality.md`, and `libs/python/geometry/.planning/energy/simulate.md`.
- Unlocks: geometry-domain dashboards aggregate distributions without receipt post-processing.
- Anchors: the `rasm.geometry.evidence.duration` row and the `_DOMAIN_SLOT` mapping arm on the runtime metrics owner; `[DASHBOARD_CHARTER]` as the measure authority.

[ANALYTIC_FRAME_EGRESS]-[QUEUED]: columnar frame egress for every receipt family onto the data seam.
- Capability: deviation bands, quality metrics, analytic boards, section properties, and lifecycle rollups project as columnar frames the data branch's analytics plane aggregates — energy `ResultFrame` stops being the only tabular crossing.
- Shape: one frame-projection port on the graduation spine — subject-keyed schema per receipt family, numpy-backed columns — and one projection row per producing page, frames crossing the standing geometry-to-data seam beside `ResultFrame`; lands on `libs/python/geometry/.planning/graduation.md`, `libs/python/geometry/.planning/scan/deviation.md`, `libs/python/geometry/.planning/mesh/quality.md`, `libs/python/geometry/.planning/graph/analytic.md`, `libs/python/geometry/.planning/ifc/structural.md`, and `libs/python/geometry/.planning/ifc/costing.md`.
- Unlocks: estate-wide analytics — the data branch's duckdb and lake tiers — over geometry evidence without bespoke receipt parsing.
- Anchors: ARCHITECTURE geometry-to-data `ResultFrame`/`Trimesh` seam edges; data branch arrow/duckdb analytic plane; `GeometrySubject` as the frame discriminant.

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
- Tension: assumes the `probreg` admission lands through the serialized admission lane.

[LIFECYCLE_TABULAR_EXCHANGE]-[QUEUED]: spreadsheet round-trip completing the lifecycle exchange set.
- Capability: quantity, cost, and schedule tables round-trip IFC to CSV/ODS/XLSX and back — export for estimator review, re-import of edited attribute and Pset values — beside the ifc5d/ifc4d rollups.
- Shape: one exchange verb pair on the lifecycle owner over `ifccsv` with the selector grammar scoping exported elements; lands on `libs/python/geometry/.planning/ifc/costing.md`.
- Unlocks: estimator and scheduler workflows without a BIM authoring tool in the loop; completes the ifcopenshell exchange family — `ifcdiff`, `ifcpatch`, `ifc4d`, `ifc5d` — per the set-completion law.
- Anchors: `ifccsv` IFC-spreadsheet round-trip; `libs/python/geometry/.planning/ifc/selector.md` grammar as the scoping input; the costing partition vocabulary.
- Tension: assumes the `ifccsv` admission lands through the serialized admission lane.

[EVIDENCE_TRACE_LINKS]-[BLOCKED]: cross-producer trace links over the content-keyed crossings.
- Capability: `Span.add_link` joins a consumer span — deviation's reference-GLB read, reconstruction's transform compose — to the upstream producer's trace.
- Shape: an optional serialized W3C `traceparent` string beside the `ContentKey` on `GeometryHandoff.wire()` (absent = no link), decoded to a `SpanContext` and folded as a `Link` at `evidence_run` span open; lands on `libs/python/geometry/.planning/graduation.md` and `libs/python/geometry/.planning/scan/deviation.md`.
- Unlocks: backend metric-to-trace-to-upstream-trace click-through across the scan plane's producer chain.
- Anchors: the `evidence_run` weave on the graduation spine; `Link`/`Span.add_link` on the branch `opentelemetry-api` catalogue.
- Tension: blocker — does the compute `rasm.compute.graduation.handoff` decode admit a widened `wire()` carrying a W3C `traceparent` field, and which co-ship lands both ends; resolution route: the compute graduation handoff owner page and the cross-libs `[UNIFIED_SIGNAL_FABRIC]` frozen-wire ruling.

[MID_OPERATION_PULSE]-[BLOCKED]: hook-rail pulse points for the long-running producers.
- Capability: `rasm.geometry.<domain>.<point>` OBSERVE rows on the runtime `Hooks` registry stream tessellation, convergence, and simulation-phase facts mid-operation.
- Shape: one graduation-owned pulse surface firing `Hooks.fire` and `Span.add_event` on the weave's live span; producers compose it, never a per-page registry; lands on `libs/python/geometry/.planning/graduation.md`.
- Unlocks: live progress taps for the C# rail and the TS viewer without polling receipts.
- Anchors: the runtime `Hooks` registry and its telemetry taps; `Span.add_event` on the branch `opentelemetry-api` catalogue.
- Tension: blocker — does the runtime lane spine expose a parent-side streaming drain a worker kernel can pulse through mid-run, a `pebble` map-iterator or pipe conduit; resolution route: runtime `execution/lanes.md` lane surface and the branch `libs/python/.api/pebble.md` member truth.

[DAYLIGHTING_SCENE_DESCRIPTOR]-[QUEUED]: host-minted daylighting scene descriptors decode into the energy simulation plane.
- Capability: the C#-minted content-keyed scene descriptor — `SunState` astronomy, photometric light roster with distribution payloads, GLB shading tessellation — decodes into the building-model admission so radiation, shading, and daylight-autonomy studies run from the live host scene with zero host dependency.
- Shape: a descriptor decode fold ahead of the `translate` pair on `libs/python/geometry/.planning/energy/simulate.md` — sun state onto the climate owner's solar vocabulary at `libs/python/geometry/.planning/energy/climate.md`, light roster onto the model admission at `libs/python/geometry/.planning/energy/model.md`, GLB shading through the standing tessellation rail; daylight recipes ride the existing `RecipeName` row shape, results returning as the page's self-describing frames keyed by the descriptor's content identity.
- Unlocks: closed-loop solar and daylight studies from the live Rhino scene; the estate scene-descriptor vocabulary gains its first consumer.
- Anchors: `energy/simulate.md` recipe binding and frame discipline; `energy/climate.md` solar vocabulary; the content-keyed crossing law and the GLB rail.
- Tension: descriptor schema is the shared wire owner's mint — this plane decodes it and never widens it; tessellation-fidelity policy arrives as a descriptor axis, never a local default.
- Ripple: `libs/.planning` `[DAYLIGHTING_SCENE_DESCRIPTOR]`.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

(none)
