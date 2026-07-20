# [PY_GEOMETRY_TASKLOG]

Open and closed work for `geometry`, distilled from `IDEAS.md`. Each task card leads with `[ID]-[STATUS]: thesis` and carries `Capability`, `Shape`, `Unlocks`, `Anchors`, and optional `Tension` fields. `[1]-[OPEN]` holds live work; `[2]-[CLOSED]` records finished or dropped tasks.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with. `Atomic` flags a minor-scope task so a later session sizes its turn correctly and does not overscope a batch of small items.

## [01]-[OPEN]

<!-- source-only: open task card template:
[ID]-[STATUS]: <ambitious concise thesis>.
- Capability: <higher-order concept, invariant, or owner capability>.
- Shape: <what the idea becomes as a system, product, owner, or feature set(s)>.
- Unlocks: <new branch, package, workflow, proof, user, or agent capability made possible>.
- Anchors: <owners, seams, packages, doctrines, or techniques that make the idea plausible>.
- Tension: <only when an unresolved constraint, boundary, bet, or dependency shapes the idea>.
- Ripple: <origin/counterpart card this entry pairs with across folders, as `pkg` `[SLUG]`; present only on a cross-folder ripple counterpart card>.
- Atomic: <present only on a minor-scope task; one short phrase naming the small unit so a later session does not overscope its turn>.
-->

[KERNEL_BENCH_LANE]-[QUEUED]: land the kernel bench lane on the mesh pages.
- Capability: runtime `Bench.run` subjects for the tessellate, repair, and boolean kernels with latency and throughput rows on `libs/python/geometry/.planning/mesh/serve.md`, `libs/python/geometry/.planning/mesh/repair.md`, and `libs/python/geometry/.planning/mesh/brep.md`.
- Anchors: runtime `observability/profiles#BENCH`; `mesh/serve#SYNC` tessellation entry; the graduation `EvidenceScope` vocabulary; idea `[KERNEL_BENCH_FAMILY]`.
- Tension: bench at the entry seam only; a process-terminal run rides the runtime job envelope.
- Atomic: bench subjects and card fields, zero new instrument rows.

[SCAN_BENCH_SUBJECTS]-[QUEUED]: extend the bench family onto the scan kernels.
- Capability: `Bench.run` subjects for the registration and reconstruction kernels — global, coarse, fine, and Poisson arms — with cloud-size-parameterized latency rows on `libs/python/geometry/.planning/scan/registration.md` and `libs/python/geometry/.planning/scan/reconstruction.md`.
- Anchors: runtime `observability/profiles#BENCH`; the registration mode vocabulary; the reconstruction `_CONSTRUCT` rows; idea `[KERNEL_BENCH_FAMILY]`.
- Atomic: bench subject rows only, no kernel edits.

[GLB_STORE_SPILL]-[QUEUED]: map the daemon cache fold onto a content-addressed object-store spill.
- Capability: pin the `obstore` members the spill band composes — `from_url` store dispatch, get/put, conditional put, the fsspec adapter — and design the store-key layout off the existing daemon cache-seed content key on `libs/python/geometry/.planning/mesh/daemon.md`.
- Anchors: branch `libs/python/.api/obstore.md`; `libs/python/.api/universal-pathlib.md`; the daemon cache-seed byte fold; idea `[GLB_ARTIFACT_STORE]`.

[GLB_SERVE_READTHROUGH]-[QUEUED]: land the serve read-through row against the artifact store.
- Capability: `ArtifactSync` answers from the store on a content-key hit before waking the daemon, with the store handle threaded as a servicer parameter on `libs/python/geometry/.planning/mesh/serve.md`.
- Anchors: serve streaming fold; runtime `ContentKey`; idea `[GLB_ARTIFACT_STORE]`.
- Atomic: one read-through row and its parameter thread.

[COST_WEAVE_FOLD]-[QUEUED]: fold parent-observed resource cost into `evidence_run`.
- Capability: `psutil.Process.oneshot` before/after capture — `cpu_times`, `memory_info` — folded as cost facts onto the crossing receipt and recorded as a cost measure beside the duration row on `libs/python/geometry/.planning/graduation.md`.
- Anchors: branch `libs/python/.api/psutil.md`; the `_recorded` fold on the weave; runtime `observability/metrics.md` tenant fold; idea `[KERNEL_COST_LEDGER]`.

[SPAN_SUBJECT_RENAME]-[QUEUED]: refine the weave span name with the graduated subject at close.
- Capability: `Span.update_name` inside `_close` re-names the evidence span `{operation}:{subject}` when the cleared value carries a `GeometrySubject`, so trace search keys on subjects on `libs/python/geometry/.planning/graduation.md`.
- Anchors: `Span.update_name(name)` on `libs/python/.api/opentelemetry-api.md`; the `_close` fold; idea `[KERNEL_COST_LEDGER]`.
- Atomic: one `_close` edit, zero weave-shape change.

[PROFILE_SUBJECT_MAP]-[QUEUED]: map kernel entry seams to profile subjects.
- Capability: one profile-subject row per `KernelTrait.HOSTILE` entry seam — daemon tessellation, spatial batch kinds, quality manifold tier, registration arms — keyed by `EvidenceScope`, naming what the runtime worker-attach counterpart profiles, on `libs/python/geometry/.planning/graduation.md` and `libs/python/geometry/.planning/mesh/daemon.md`.
- Anchors: runtime `observability/profiles.md`; `pyroscope-otel` span correlation; idea `[KERNEL_PROFILE_EVIDENCE]`.

[CHARTER_MEASURE_TABLE]-[QUEUED]: land the subject-keyed measure charter table.
- Capability: one table on `libs/python/geometry/.planning/graduation.md` — measure name under `rasm.geometry.<measure>`, UCUM unit, source receipt field, aggregation — covering deviation, closure, quality, registration-fitness, energy, and bench measures.
- Anchors: wire-law UCUM naming; `GeometrySubject` keys; runtime `INSTRUMENTS` shape; idea `[DASHBOARD_CHARTER]`.

[DISTRIBUTION_ROWS]-[QUEUED]: land one instrument row and one record call per charter measure.
- Capability: runtime `INSTRUMENTS` rows for the charter measures and `Metrics.record` mapping-arm calls at the producing folds on `libs/python/geometry/.planning/scan/deviation.md`, `libs/python/geometry/.planning/mesh/quality.md`, and `libs/python/geometry/.planning/energy/simulate.md`.
- Anchors: `_DOMAIN_SLOT` mapping arm; `[CHARTER_MEASURE_TABLE]` as the row authority; idea `[PRODUCER_DISTRIBUTIONS]`.
- Tension: sequenced after the charter table lands — rows derive from charter data, never ad-hoc picks.

[FRAME_SCHEMA_PORT]-[QUEUED]: mint the subject-keyed frame projection port on the graduation spine.
- Capability: one frame-projection port beside `wire()` — per-subject columnar schema, numpy-backed columns, `GeometrySubject` discriminant — on `libs/python/geometry/.planning/graduation.md`.
- Anchors: `GeometryHandoff` carrier; data-branch arrow plane as the consumer; idea `[ANALYTIC_FRAME_EGRESS]`.

[FRAME_PRODUCER_ROWS]-[QUEUED]: land one frame projection row per receipt family.
- Capability: deviation bands, quality metrics, analytic boards, section properties, and lifecycle rollups each gain a projection row through the frame port on `libs/python/geometry/.planning/scan/deviation.md`, `libs/python/geometry/.planning/mesh/quality.md`, `libs/python/geometry/.planning/graph/analytic.md`, `libs/python/geometry/.planning/ifc/structural.md`, and `libs/python/geometry/.planning/ifc/costing.md`.
- Anchors: `[FRAME_SCHEMA_PORT]` port; the geometry-to-data seam beside `ResultFrame`; idea `[ANALYTIC_FRAME_EGRESS]`.

[GEOREF_MEMBER_PIN]-[QUEUED]: pin the `ifcopenshell.util.geolocation` member spellings.
- Capability: verify the geolocation transform helpers against the installed distribution and repair `libs/python/geometry/.api/ifcopenshell.md` with the confirmed members before any fence lands.
- Anchors: installed `ifcopenshell` distribution; the folder catalog repair law; idea `[IFC_GEOREFERENCE]`.
- Atomic: catalog member pin only, no page fences.

[GEOREF_BAND]-[QUEUED]: land the georeference extraction and authoring rows.
- Capability: extraction projection minting CRS, local-to-map transform, and true north on `libs/python/geometry/.planning/ifc/analysis.md`; authoring verb row minting and updating `IfcMapConversion`/`IfcProjectedCRS` on `libs/python/geometry/.planning/ifc/authoring.md`.
- Anchors: pinned geolocation members from `[GEOREF_MEMBER_PIN]`; the authoring verb table; idea `[IFC_GEOREFERENCE]`.

[NONRIGID_ARM]-[QUEUED]: land the non-rigid registration arm and the deformation split.
- Capability: `probreg` CPD/FilterReg arm on the registration mode vocabulary returning a deformation-field carrier on `libs/python/geometry/.planning/scan/registration.md`; deviation projection splitting rigid residual from deformation magnitude on `libs/python/geometry/.planning/scan/deviation.md`.
- Anchors: the `Cloud` array carrier crossing worker seams; the registration session shape; idea `[NONRIGID_DEFORMATION_TRACK]`.
- Tension: assumes the `probreg` admission lands.

[FPS_DOWNSAMPLE_ROW]-[QUEUED]: land the farthest-point downsample policy row on ingestion.
- Capability: `PointCloud.farthest_point_down_sample` as a geometry-uniform downsample row bounding the non-rigid arm's point budget on `libs/python/geometry/.planning/scan/ingestion.md`.
- Anchors: `libs/python/geometry/.api/open3d.md` member row; the ingestion filter-stage tables; idea `[NONRIGID_DEFORMATION_TRACK]`.
- Atomic: one policy row on the ingestion stage table.

[TABULAR_ROUNDTRIP_VERBS]-[QUEUED]: land the spreadsheet exchange verb pair on the lifecycle owner.
- Capability: `ifccsv` export and re-import verbs on `libs/python/geometry/.planning/ifc/costing.md`, selector-grammar scoped, with the re-import writing attribute and Pset edits back through the authoring rail.
- Anchors: `ifccsv` round-trip; `ifc/selector` grammar; the costing partition vocabulary; idea `[LIFECYCLE_TABULAR_EXCHANGE]`.
- Tension: assumes the `ifccsv` admission lands.
- Atomic: one verb pair on the lifecycle verb table.

[TRACE_LINK_WIRE_PROBE]-[BLOCKED]: answer the trace-link wire-widening question.
- Capability: verdict on widening `GeometryHandoff.wire()` with a W3C `traceparent` field and the compute co-ship that decodes it, unblocking `[EVIDENCE_TRACE_LINKS]`.
- Anchors: compute `rasm.compute.graduation.handoff` decode owner; cross-libs `[UNIFIED_SIGNAL_FABRIC]` frozen-wire ruling; idea `[EVIDENCE_TRACE_LINKS]`.
- Tension: blocker — the crossing is frozen wire data; resolution route is the compute handoff page and the cross-libs ruling, never a unilateral geometry widen.

[PULSE_DRAIN_PROBE]-[BLOCKED]: answer the parent-side streaming-drain question.
- Capability: verdict on whether the runtime lane spine surfaces a mid-run drain — `pebble` map-iterator or pipe conduit — a worker kernel can pulse through, unblocking `[MID_OPERATION_PULSE]`.
- Anchors: runtime `execution/lanes.md` lane surface; branch `libs/python/.api/pebble.md` member truth; idea `[MID_OPERATION_PULSE]`.
- Tension: blocker — every candidate loop runs inside a HOSTILE worker kernel; without a parent-side drain no live span or hook registry is reachable mid-operation.

[SCENE_DESCRIPTOR_DECODE]-[QUEUED]: land the descriptor decode fold on the energy plane.
- Capability: decode rows mapping descriptor bands to their owners — `SunState` onto the climate solar vocabulary, light roster onto the model admission, GLB shading through the tessellation rail — with the daylight `RecipeName` rows joining the simulate shape.
- Shape: rows on `libs/python/geometry/.planning/energy/simulate.md`, `libs/python/geometry/.planning/energy/climate.md`, and `libs/python/geometry/.planning/energy/model.md`.
- Anchors: idea `[DAYLIGHTING_SCENE_DESCRIPTOR]`; the estate `[SCENE_DESCRIPTOR_SCHEMA]` schema pin as the field authority.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

(none)
