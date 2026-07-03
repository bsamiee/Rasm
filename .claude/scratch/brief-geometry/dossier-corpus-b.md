# [DOSSIER_GEOMETRY_CORPUS_B] — mesh/ + scan/ (second-half subfolders)

Scope: `libs/python/geometry/.planning/{mesh,scan}` deep-read (10 pages, 2430 LOC); `graph/`+`ifc/` skimmed on seam-cross + grep census. Governance (`README.md`, `ARCHITECTURE.md`) read full. Upstream law consumed: `RASM-RUNTIME-BRIEF.md`, `RASM-DATA-BRIEF.md`, `libs/.planning/architecture.md`, `RASM-COMPONENT-PARADIGM-DECISION.md [AMENDMENTS]`.

VERDICT (folder half): page-level dense, graph-level and governance-level broken — the same disease runtime/data carry. The mesh worker tier (brep/repair/spatial/quality) is genuinely 8-9-grade FP/ROP with real `.api`-anchored depth; the scan tier is authored to the same prose bar but runs its multi-second kernels SYNCHRONOUSLY with a dead offload seam; the load-bearing `daemon` has no wire-egress fence at all; a whole ENERGY plane is illusory; and every identity import breaks the instant runtime lands. Mesh worker family and scan family diverge into two incompatible receipt/span idioms. Corpus is NOT world-class as-is.

---

## PER-PAGE VERDICTS (deep-read: mesh + scan)

### mesh/daemon.md — VERDICT 6/10 (9-grade internals, unwired charter)
The load-bearing cross-boundary owner; keyed-admission cache short-circuit + PEP-734 offload are excellent. But its CHARTER (per-element GLB → C# ArtifactSync) has zero fence.
- **DEFECT [unwired core]** `daemon.md:175` `tessellate` returns `self`; `:213` `contribute` yields receipts carrying only `content_key.hex`/counts (`:80-84`), NEVER the GLB bytes. The `TessellationResult.glb` lands only in the lane's in-memory `self._cache` (`:172,180`). No public accessor, no servicer, no serve registration exists anywhere in the corpus (grep: ServerHost/ComputeService/ArtifactSync appear ONLY as prose at `:3,20,222` + `cad.md:22`). The GLB is computed, cached, and stranded — the C# side can never retrieve it. The whole `[TRANSPORT]`/`[WIRE]` seam (`ARCH:42,43,55`) is prose-only.
- **DEFECT [import drift]** `daemon.md:36` `from rasm.runtime.content_identity import ...` — runtime `[V4]` DELETES this spelling for `rasm.runtime.identity`.
- **DEFECT [stale anchor]** `daemon.md:223` anchors `evidence/identity#SEED_REPRODUCTION` `GLB_BY_KEY`; runtime `[V7]` moves `SeedReproduction`/corpus fold to NEW `evidence/reproduction.md`.
- **DEFECT [base-tier coupling]** `daemon.md:15,92-95` reads `IdentityPolicy.deflection`/`.angle_tolerance`/`.spec` off runtime's host-free stratum-zero identity policy — geometry mesh knobs coupled into the base (or phantom fields).
- **DEFECT [upstream demand]** `daemon.md:199` `offload(kernel,*args, retry=RetryClass.OCCT)` + `:220` demands lanes `offload` gain a `retry=` keyword and an `anyio.BrokenWorkerInterpreter`-keyed OCCT POLICY row — runtime brief exposes neither the keyword nor the target binding (waterfall, below).
- **DEFECT [banned tail]** `daemon.md:217` `## [03]-[RESEARCH]`.
- OWNER CHARTER AS IT SHOULD BE: `TessellationDaemon` owns keyed source→GLB tessellation and EXPOSES `RuntimeRail[Block[TessellationResult]]` (GLB bytes reachable); a NEW `mesh/serve.md` (geometry servicer) binds it behind ComputeService.Tessellate and streams `TessellationResult.glb` over the runtime ArtifactSync leg. Tessellation deflection/angle move to a geometry `TessellationPolicy` folded into a content SEED; runtime `ContentIdentity.of` receives canonical bytes only.

### mesh/cad.md — VERDICT 8/10
Clean OCCT XCAF bridge; `READERS`/`_APPLY` dispatch tables, `BridgeView` overload, `boundary`-fenced `BridgeFault`, `.api`-verified members. Genuinely dense.
- **DEFECT** `cad.md:51` `content_identity` import drift.
- **DEFECT** `cad.md:51` reads `IdentityPolicy.deflection`/`.angle_tolerance` (coupling, shared with daemon/brep).
- **DEFECT** `cad.md:255,260` `[03]-[RESEARCH]` + `[04]-[UPSTREAM]` double appendix.
- CHARTER OK: STEP/IGES→GLB hop composed by daemon `cad` arm; contributor-free `tessellate(...,view)`. Sound.

### mesh/brep.md — VERDICT 8/10
Five-case B-rep union; OCCT command-pattern folded once via `OcctBuilder` Protocol; `manifold3d.CrossSection` 2D weave; n-ary BOPAlgo. Strong.
- **DEFECT** `brep.md:58` `content_identity` drift; `:417` reads `policy.deflection`/`angle_tolerance` (coupling).
- **DEFECT** `brep.md:57` geometry→compute forward import (`GeometrySubject` from `rasm.compute.graduation.handoff`).
- **DEFECT** `brep.md:250` free-function `async apply(op, lane)` — owner-shape inconsistent with the capsule siblings (daemon/spatial/quality).
- **DEFECT** `brep.md:423,426` `[RESEARCH]`+`[UPSTREAM]`.
- CHARTER OK (2D CrossSection correctly distinct from repair's 3D Manifold, `:16`).

### mesh/repair.md — VERDICT 8/10
Canonical `manifold3d.Manifold` 3D-CSG owner; `Condition`/`Boolean` union; step-set parameterization over `RepairStep`. Clean.
- **DEFECT [duplicate mechanism]** `repair.md:199` `_to_manifold` (Mesh/Mesh64-by-uint32-ceiling) is spelled IDENTICALLY at `spatial.md:176` and `quality.md:206` (`_topology_kernel`). Three copies despite repair's charter as "the canonical owner of the manifold3d.Manifold kernel" (`:2`).
- **DEFECT** `repair.md:35` geometry→compute import; `:250,260` `[RESEARCH]`+`[UPSTREAM]`.
- (No `content_identity` import — repair doesn't key content, correct.)
- CHARTER: promote `_to_manifold` to a repair-owned public surface (or a shared `mesh/_manifold` leaf) the spatial/quality kernels compose.

### mesh/spatial.md — VERDICT 8.5/10 (cleanest of the half)
Capability-aware `_route`; `SpatialQuery`/`SpatialResult` case-mirror; offload-vs-sync never forks `_dispatch`; `_nearest_hits` lexsort reduction is real depth.
- **DEFECT [duplicate]** `spatial.md:176` `_to_manifold` (see repair).
- **DEFECT** `spatial.md:288,291` `[RESEARCH]`+`[UPSTREAM]` (BOTH EMPTY — pure dead headers, the runtime-`[V11]` empty-tail class).
- Minor: prose "both backends native worker" (`:11,218`) overstates — `_offload` membership is capability-computed (`:246`), so clearance runs sync when `manifold3d` is in-process. No `content_identity` import (correct). Sound owner.

### mesh/quality.md — VERDICT 6.5/10 (fence bug + name collision)
Dense metrology + exact-topology enrichment tier; the half-edge `np.unique` incidence fold (`:335-341`) is genuine depth.
- **DEFECT [FENCE WON'T PARSE]** `quality.md:344` `area = exact.area if exact else float(mesh.area)` is dedented to column 0 inside the 8-space `_metrics` body (`:342-343,345` correctly indented) — a hard `IndentationError`. Concrete correctness bug.
- **DEFECT [name collision]** `quality.md:229` `class MeshQuality(ReceiptContributor)` (the metrics SERVICE) collides with `reconstruction.md:103 class MeshQuality(Struct)` (a leaf value object). Two types, one canonical name.
- **DEFECT [duplicate]** `quality.md:206` `_topology_kernel` re-spells the Mesh/Mesh64 build (see repair/spatial).
- **DEFECT** `quality.md:358,363` `[RESEARCH]`+`[UPSTREAM]`. No `content_identity` import (correct).
- CHARTER: fix indent; rename one `MeshQuality`; the closure-metric fold reconstruction re-implements belongs here (below).

### scan/ingestion.md — VERDICT 6.5/10
Sophisticated pdal filter-graph: `_STAGE`/`_FILTER` injected-`Filter`-class threading, `ScanSource` union, `Block.unfold`-free `Option` absent-graph fold. Real depth in the table design.
- **DEFECT [dead lane + sync blocking]** `ingestion.md:144` `def ingest` (SYNC); `:142` declares `lane: LanePolicy | None = None` but the fence NEVER composes `offload` — `_execute` (`:233-239`) runs `pipeline.execute()` (multi-second SMRF/voxel) on the event loop. The exact "lane accepted yet never composed" deleted form `quality.md:17` forbids; contradicts own prose (`:3,16`).
- **DEFECT [inline span]** `ingestion.md:153` hand-opens `_TRACER.start_as_current_span("geometry.ingest")` — violates runtime's owned `measured` aspect ("never inline span opens beside them").
- Minor hardcode: `range_limits: str = "Z[0:30]"` (`:94`) magic crop default (policy-overridable).
- **DEFECT** `ingestion.md:271,275` `[RESEARCH]`+`[UPSTREAM]`. (No `content_identity` import — ingestion keys nothing.)
- CHARTER: `async def ingest` + `await lane.offload(_dispatch, source)`; compose runtime `measured` not inline span.

### scan/registration.md — VERDICT 6.5/10
Rich N-cloud PEP-646 session; 5 modes (kiss-matcher/open3d-FGR/multiscale/colored/VGICP/multiway); `_from_tensor` projector; measured two-key graduation. Dense.
- **DEFECT [dead lane + sync]** `registration.md:166` `def register` (SYNC); `:164` dead `lane`; `_dispatch` (`:184`) runs `small_gicp.align`/`multi_scale_icp`/`kiss_matcher` on the calling thread. No fence offload.
- **DEFECT [inline span]** `registration.md:171` inline `start_as_current_span`.
- **DEFECT [import drift]** `registration.md:31` `content_identity`; `:35` geometry→compute import.
- Minor hardcode: `_RMSE_CEILING=0.01`/`_MISFIT_CEILING=0.7` (`:64-65`) graduation bars as module Finals (policy-worthy).
- **DEFECT** `registration.md:330,340` `[RESEARCH]`+`[UPSTREAM]`. Stale sub-domain name `mesh-utility` (`:3`) — no such sub-domain in the codemap.

### scan/deviation.md — VERDICT 6.5/10 (central AEC value)
The construction-verification owner (scan-vs-model signed band). `DeviationBand.fold` one-pass reduction, `Block.unfold` RANSAC peel, amortized `ProximityQuery`, `SignedField` finiteness fence, two-key graduation. Genuinely strong domain content.
- **DEFECT [dead lane + sync]** `deviation.md:235` `def evaluate` (SYNC); `:233` dead `lane`; `_dispatch` (`:256`) runs `signed_distance`/`segment_plane` synchronously.
- **DEFECT [inline span]** `deviation.md:243`.
- **DEFECT [drift]** `deviation.md:33` `content_identity`; `:37` geometry→compute import.
- Minor: `_TOLERANCE_CEILING`/`_WORKING_TOLERANCE`/`_FRACTION_CEILING`/`_UP_AXIS`/`_FLAT_AXIS` module Finals (`:70-75`) — verticality/tolerance policy as hardcode (mirrored onto `DeviationPolicy`, so partly parameterized).
- **DEFECT** `deviation.md:311,316` `[RESEARCH]`+`[04]-[CROSS_FOLDER]`.

### scan/reconstruction.md — VERDICT 6/10
`_CONSTRUCT` method table (Poisson/ball-pivoting/alpha), `_trim_poisson` density fence, cluster split. Table design is clean.
- **DEFECT [name collision + duplicate concern]** `reconstruction.md:103` mints `class MeshQuality(Struct)` colliding with `quality.md:229`, AND `MeshQuality.fold` (`:114-123`) re-computes watertight/winding/euler/is_volume/body_count/volume/area/counts — the exact manifold-closure concern `mesh/quality#Metrics`→`QualityMetrics` owns (`quality.md:74-93`). Duplicate mechanism under a colliding name; also a second component-count path (`trimesh.body_count` vs quality's `manifold3d.decompose()`).
- **DEFECT [dead lane + sync]** `reconstruction.md:233` `def reconstruct` (SYNC); `:231` dead `lane`; `_dispatch` (`:253`) folds `build(part)` (Poisson/ball-pivoting) synchronously.
- **DEFECT [inline span]** `reconstruction.md:234`; **[drift]** `:37` `content_identity`, `:41` geometry→compute import.
- **DEFECT** `reconstruction.md:278,285` `[RESEARCH]`+`[UPSTREAM]`.
- CHARTER: reconstruction composes `mesh/quality#Metrics` (or a shared closure-fold leaf); no second `MeshQuality`.

---

## CROSS-CUTTING FINDINGS

### Duplication
- **`_to_manifold` × 3** — `repair.md:199`, `spatial.md:176`, `quality.md:206`(`_topology_kernel`) each re-spell the `Mesh`/`Mesh64`-by-uint32-ceiling `Manifold` build. Repair charters itself the canonical manifold3d owner (`repair.md:2`) yet the conversion is copied into two siblings.
- **Closure-metric fold × 2** — `reconstruction.md MeshQuality.fold` duplicates `quality.md QualityMetrics` (watertight/euler/volume/area/counts), two component-count strategies.
- **Cross-cutting receipt/span scaffold × 3 idioms** (see candidate 3).

### Concern mixing / parallel rails where one owner belongs
- Three receipt/span idioms: scan+ifc inline-`start_as_current_span` + receipt-struct `@receipted _emit` + `span_facts`/`facts` (7 pages); mesh free-`apply` + module-level `@receipted _emit` + `fact()` (brep/repair); mesh capsule `_route`/`_fold`/`_arm` (daemon/spatial/quality). No shared owner; the inline-span variant violates runtime's `measured` aspect.
- Owner-shape split: free-function `apply(op,lane)` (brep/repair) vs stateful capsule (daemon/spatial/quality) vs frozen-Struct service (all scan) — three shapes for one "offload a geometry op, emit a receipt" concern.

### Hardcoding vs generator
- Table discipline is strong where present (`_STAGE`/`_FILTER`/`READERS`/`_CONSTRUCT`/`_PRIMITIVES`/`_BOOLEANS` are genuine seed-data rows — adding a case is one row). GENERATOR is respected in the worker tier.
- Hardcode residue: `ingestion.md:94` `"Z[0:30]"`; `deviation.md:70-75` + `registration.md:64-65` graduation/verticality ceilings as module Finals (partly mirrored to Policy, so soft).

### Dead carriers / unwired seams
- **daemon GLB egress** — computed+cached, never exposed/transported (candidate 1).
- **scan lane** — declared on all 4 scan owners, composed by none (candidate 2).
- **ENERGY plane** — 13 packages + README prose + `.api` catalogs, zero fences (candidate 4).
- **recipe-binding seam** — README (`:70-72`) says geometry binds `RecipeInterface`/`Job` and hands execution to runtime `[V3]` recipe owner; wired in zero fences.
- **empty appendix headers** — `spatial.md:288,291` both empty (runtime-`[V11]` dead-tail class).

### Unmined capability (catalog anchors)
- `libs/python/geometry/.api/{honeybee-core,honeybee-energy,honeybee-openstudio,ladybug-core,ladybug-comfort,dragonfly-core,dragonfly-energy,queenbee,lbt-recipes,pollination-handlers}.md` — 13 richly-authored catalogs backing NO fence (the hollow plane).
- `sectionproperties.md` — README `[MESH_CAD]` charters "warping/plastic/shear enrichment row"; consumed only by ifc/structural (first half), not mesh.
- `gmsh` — README `[MESH_CAD]` "Deferred" with no consuming page (honest deferral, acceptable).

### FIT credits (sound, preserve)
- **GLB content-key parity** — `daemon.md:223` reproduces the seed-zero (`Some(0)`) `XxHash128` over GLB bytes byte-identical to C# `RepresentationContentHash`/`GeometryHash`, consistent with architecture `[07]` tri-runtime bit-identical content-identity and `[FROZEN_INVARIANTS]` seed-zero-one-hasher. A reproduce-under-parity, never a drift re-mint. (ARCH:41 prose "decode the seam key" is loose — it is reproduce, not decode — but semantics correct.)
- **data seam** — `scan → data/spatial/mesh#POINTCLOUD` pyarrow bridge (`ingestion.md:12`) and `mesh ← data/spatial MeshPayload` codec deferral (repair/brep/spatial/quality Boundary) correctly consume data `spatial/mesh` (`rasm.data.spatial.mesh`), matching data-brief codemap. No re-owned LAS decode. Sound.
- **count-prefix amendment** — geometry keys GLB blobs (not PropertySet/QuantitySet bags), so `RASM-COMPONENT-PARADIGM-DECISION [AMENDMENTS]` item-1 count-prefix does not bind here; no violation.

---

## VERDICT CANDIDATES (campaign-defining, evidence-first)

1. **DAEMON WIRE-EGRESS IS UNWIRED** — the load-bearing owner cannot hand its GLB to C#. `tessellate` returns `self` (`daemon.md:175`); `contribute` yields receipts without bytes (`:213,80-84`); GLB stranded in `self._cache` (`:172,180`); zero servicer/serve-registration fences corpus-wide (ComputeService/ArtifactSync are prose only, `:3,20`). RULING: `TessellationDaemon` must expose the GLB results, and a NEW geometry servicer/serve page must bind it to the runtime ArtifactSync/ComputeService leg; the `[TRANSPORT]`/`[WIRE]` seams get a real fence or die.

2. **SCAN FAMILY DEAD-LANE + SYNC BLOCKING** — all 4 scan owners declare `lane: LanePolicy | None = None` and compose it in ZERO fences; entries are sync `def` (`ingestion.md:144`, `registration.md:166`, `deviation.md:235`, `reconstruction.md:233`), running pdal/ICP/RANSAC/Poisson on the event loop. The exact deleted form `quality.md:17` names; contradicts own prose. RULING: adopt the mesh worker shape — `async def` + `await lane.offload(_dispatch, ...)`; the offloaded kernels are module-level and picklable already.

3. **THREE DIVERGENT RECEIPT/SPAN IDIOMS — ONE OWNER BELONGS** — inline `start_as_current_span` on 7 pages (`scan/*` + `ifc/{analysis,costing,structural}`) directly violates runtime's owned `measured` aspect; the mesh tier uses lane-stitched context + module/method `@receipted`; three owner shapes (free-fn / capsule / frozen-Struct). RULING: one shared measured-span+receipt-emit shape composing runtime `measured`/`@receipted`; no inline span opens; converge the owner shape.

4. **ENERGY HOLLOW PLANE** — README charters "the out-of-process AGPL Ladybug Tools energy/environmental companion band" (`README.md:3`) with 13 `[ENERGY]` packages (`:59-72`) and 10 `.api` catalogs, but ZERO design pages, codemap nodes, router routes, or fences (grep: zero honeybee/ladybug/dragonfly/queenbee/recipe hits in `.planning/`). The recipe-binding seam to runtime `[V3]` is prose-only (`:70-72`). Identical to data impact-hollow-plane / runtime recipe-illusion. RULING: build the energy sub-domain (climate/building/district/comfort pages composing runtime's `execution/recipe` rail + the data-brief HBJSON/DataCollection wire) OR strike the 13 packages + prose + catalogs. This is the folder's largest coverage naivety.

5. **CONTENT_IDENTITY IMPORT DRIFT (folder-wide)** — 12 code pages import `rasm.runtime.content_identity` (grep [1]) — the exact spelling runtime `[V4]` DELETES for `rasm.runtime.identity`; `daemon.md:223` anchors `evidence/identity#SEED_REPRODUCTION` which runtime `[V7]` re-homes to `evidence/reproduction.md`. Every identity-consuming page breaks on runtime landing. RULING: rename all 12 sites to `rasm.runtime.identity`; retarget the reproduction anchor. (data E5 parity.)

6. **IDENTITYPOLICY TESSELLATION COUPLING** — geometry reads `IdentityPolicy.deflection`/`.angle_tolerance`/`.spec` (`daemon.md:15,92-95`; `cad.md:51`; `brep.md:417`) and the seam ledger bakes "deflection/tolerance policy-seed" into the content key (`ARCH:40`). Runtime is host-free stratum-zero and knows no meshing. RULING: geometry owns a `TessellationPolicy` (deflection/angle/mesher knobs) folded into canonical bytes; runtime `ContentIdentity.of` receives the seed only; runtime `IdentityPolicy` stays a generic policy carrier. (Waterfall pressure on runtime, below.)

7. **MESHQUALITY NAME COLLISION + DUPLICATE CLOSURE FOLD** — `quality.md:229 MeshQuality` (metrics service) vs `reconstruction.md:103 MeshQuality` (leaf value object); reconstruction re-computes the closure metrics `quality#Metrics`→`QualityMetrics` owns. RULING: one canonical `MeshQuality`; reconstruction composes the metrics owner (or a shared closure-fold leaf); one component-count strategy.

8. **GEOMETRYSUBJECT OWNERSHIP INVERSION** — `GeometrySubject` (the vocabulary of geometry-PRODUCED subjects: registration-transform/reconstructed-mesh/scan-deviation/mesh-algebra) is homed in COMPUTE, forcing 11 geometry pages to import `rasm.compute.graduation.handoff` (grep [4]) — the producer importing its own vocabulary from the consumer, and a forward edge onto compute which is sequenced AFTER geometry (runtime→data→geometry→compute). RULING: `GeometrySubject` moves to geometry (compute's `HandoffAxis` imports it, matching build order) or to a shared graduation-contract tier; compute keeps only the multi-domain `HandoffAxis` hub + `GraduationReceipt` admission fold.

9. **BANNED APPENDIX TAILS (folder-wide, double-layer)** — every one of 18 pages carries `## [03]-[RESEARCH]` and most a second `## [04]-[UPSTREAM]`/`[CROSS_FOLDER]` (grep [5]) — the provenance/checklist tail runtime `[V11]`/data `[V11]` purged, here doubled. Some are empty (`spatial.md:288,291`). RULING: fold load-bearing member confirmations into `Packages`/`.api`; delete the rest and both appendix layers.

10. **README ROUTER STALE BY 5 PAGES** — router lists 13 of 18 pages; missing `ifc/authoring`, `mesh/brep`, `mesh/spatial`, `mesh/quality`, `graph/features` (the entire mesh enrichment tier + two more), plus zero energy routes. RULING: router + codemap grow to the realized page set (and to the energy plane once ruled).

---

## WATERFALL RIPPLE PRESSURES (recorded, not applied — read-only survey)

Geometry demands upstream capabilities the finalized briefs lack; the author agent applies these surgically:

- **→ `RASM-RUNTIME-BRIEF.md` `[V5]`/lanes** (demanding consumer: `mesh/daemon`): `execution/lanes#offload` must gain a `retry: RetryClass` keyword wrapping the `interpreter_run_sync` leg in `resilience#guard`, and the `RetryClass.OCCT` POLICY row must be keyed on `anyio.BrokenWorkerInterpreter` (subinterpreter cold-start). Runtime `[V5]` currently only neutralizes the OCCT COMMENT while keeping the row as data — the keyword + target binding are the missing surface. Add as a lanes/resilience capability row, not a rewrite.
- **→ `RASM-RUNTIME-BRIEF.md` `[V7]`/identity** (demanding consumer: geometry daemon/cad/brep): pin `IdentityPolicy` as a generic content-seed carrier (NOT tessellation-aware); the deflection/angle knobs are geometry's `TessellationPolicy`, crossing as canonical bytes into `ContentIdentity.of`. A one-clause boundary note that `IdentityPolicy.spec` is an opaque seed, never a domain-knob record.
- **→ `RASM-RUNTIME-BRIEF.md` `[V3]`/recipe** (demanding consumer: geometry ENERGY plane): the `execution/recipe` owner is the execution target geometry's energy sub-domain will compose (geometry binds `RecipeInterface`/`Job`, runtime runs the queenbee-local Luigi DAG) — record geometry energy as a named `python:` consumer of the recipe rail so `[V3]`'s expulsion arm cannot fire.
