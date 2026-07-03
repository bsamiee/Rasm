# DOSSIER — libs/python/geometry — lane: API-TIERS

Scope covered: BOTH catalog tiers COMPLETE inventory (`libs/python/.api/` 27 substrate files; `libs/python/geometry/.api/` 38 domain files, 3952 LOC), judged mined-vs-unmined against all 18 owning `.planning` pages (read in full), the governance surfaces (`README.md`, `ARCHITECTURE.md`, `IDEAS.md`, `TASKLOG.md`), `pyproject.toml` tags, and the upstream `RASM-RUNTIME-BRIEF.md` / `RASM-DATA-BRIEF.md` + `libs/.planning/architecture.md` + `RASM-COMPONENT-PARADIGM-DECISION.md [AMENDMENTS]`.

Headline: the 12 realized CODE pages are uniformly 9.5–10 (extreme ADT/table-driven dispatch, deep multi-package `.api` mining, correct runtime-seam composition, zero `frozendict`). The naivety is **COVERAGE, not APPROACH** — an entire admitted DOMAIN (energy/environmental, 13 catalogs) has zero page, and ~39% of the geometry `.api` tier (~1555 of 3952 LOC) is dead / illusory / duplicate / misplaced.

---

## [A] PER-CATALOG VERDICT — geometry/.api (38) — mined vs unmined

Legend: MINED = owning page consumes at depth; PARTIAL = consumed, real unmined surface remains; DEAD = no consuming page; DUP = divergent duplicate in another tier; MISPLACED = package owned by another folder tier.

| Catalog | LOC | Owning page(s) | Verdict | Note |
|---|---|---|---|---|
| cadquery-ocp | 228 | mesh/cad, mesh/brep | 9 MINED | full XCAF+BRep* mined; unmined-by-design: shape-only STEP/IGES readers (rejected), StlAPI (→data), RWGltf read; genuinely unmined: `Geom_*` NURBS curves/surfaces, `BRepBuilderAPI_Sewing`/`NurbsConvert`, BOPAlgo `SetFuzzyValue`/`History`/`SectionEdges` (cad/brep do n-ary fuse but never fuzzy-tol/history), `BRepAlgoAPI_Splitter` |
| trimesh | 172 | repair, spatial, quality, brep, features, deviation, reconstruction | 9 MINED | most-consumed; unmined-by-design: `load`/`export`/`available_formats` (→data MeshPayload), `Scene` transform-graph (→artifacts), `creation` primitives (superseded by OCCT BrepOp), `registration`/`voxel`/`section` (unused) |
| manifold3d | 142 | repair, spatial, quality, brep | 9 MINED | `batch_boolean`/status/Mesh64/min_gap mined; `CrossSection` 2D in brep; unmined: SDF/Minkowski/hull/refine |
| ifcopenshell | 116 | daemon, analysis, costing, structural, authoring, selector | 9 MINED | the spine; util.element/unit/selector/shape + geom.iterator/serializers + api.* + guid + file.traverse/get_inverse mined across 6 pages |
| ifctester | 134 | analysis | 9 MINED | ids.open/validate/specifications/status/passed_entities/failed_entities; unmined tail: `Json(ids).report()` Results graph (analysis:418 names it as sharper residual) |
| ifcclash | 99 | analysis | 9 MINED | Clasher/ClashSettings/ClashSet/ClashSource/clash/smart_group_clashes/ClashResult |
| bcf-client | 131 | analysis | 8 MINED | BcfXml.create_new/add_topic/add_viewpoint_from_point_and_guids; REST API-client surface unmined-by-design |
| lark | 83 | selector | 9 MINED | Lark/Transformer_NonRecursive/v_args/UnexpectedInput.pos_in_stream/get_context |
| ifc5d | 58 | costing | 9 MINED | qto.rules/quantify/edit_qtos mined; `ifc5Dspreadsheet` writers named-but-deferred to data (correct) |
| ifc4d | 55 | costing | 9 MINED | MSProject2Ifc/P62Ifc/Asta2Ifc + .execute() |
| ifcpatch | 55 | costing | 9 MINED | execute/recipes namespace; write→data |
| ifcdiff | 56 | costing | 9 MINED | IfcDiff/diff/change_register/added_elements/deleted_elements |
| sectionproperties | 67 | structural | 9 MINED | Geometry.from_points/create_mesh/Section/calculate_*/get_j/sc/as/s/area |
| compas | 137 | algebra | 9 MINED | datastructures + geometry + transforms + rpc.Proxy; 342-entry `__all__` verified phantom-free |
| compas-dr | 101 | algebra | 9 MINED | numdata/solvers/constraints/loads |
| compas-tna | 118 | algebra | 9 MINED | FormDiagram/ForceDiagram/LoadUpdater/horizontal_numpy/vertical_from_zmax |
| topologicpy | 136 | nonmanifold | 8 MINED | Topology/Graph/Cluster/Dictionary; AGPL opt-in lane |
| open3d | 134 | deviation, registration, reconstruction | 8 MINED | registration + geometry + io + t.geometry.PointCloud |
| pdal | 102 | ingestion | 8 MINED | Pipeline/Filter driver factories |
| pye57 | 89 | ingestion | 8 MINED | E57 direct (columnar bridge does not own E57) |
| small-gicp | 84 | registration | 8 MINED | VGICP/KdTree.batch_knn_search |
| kiss-matcher | 100 | registration | 8 MINED | KISSMatcher global bootstrap (find_spec-gated) |
| **rhino3dm** | 153 | — (deferral-only) | **DEAD/MISPLACED** | data owns the `.3dm` codec (repair:258 CODEC_SEAM; data brief VERIFIED `meshio/trimesh/rhino3dm _BACKEND`); no geometry page imports it |
| **meshio** | 75 | — (deferral-only) | **DEAD/MISPLACED** | data owns FEM codec; geometry defers, never imports |
| **laspy** | 104 | — (data decodes) | **DEAD/MISPLACED** | data owns LAS/LAZ/COPC decode→Arrow (SCAN_COPC_PARTIAL card; data brief VERIFIED laspy/lazrs COPC); ingestion `arrow_las` consumes the decoded pyarrow.Table, not laspy |
| **honeybee-energy** | 144 | — | **DEAD/ILLUSORY** | fully specifies anyio.to_thread+stamina+otel integration law for a consuming owner that does not exist |
| **honeybee-core** | 120 | — | DEAD/ILLUSORY | HBJSON object graph, check_all |
| **honeybee-openstudio** | 80 | — | DEAD/ILLUSORY | in-process OpenStudio/EnergyPlus translator |
| **honeybee-standards** | 49 | — | DEAD/ILLUSORY | defaults data backend |
| **honeybee-energy-standards** | 55 | — | DEAD/ILLUSORY | ASHRAE 90.1 / DOE library |
| **ladybug-core** | 125 | — | DEAD/ILLUSORY | EPW/Wea/Location/Sunpath climate backbone |
| **ladybug-geometry** | 124 | — | DEAD/ILLUSORY | planar/solid primitive substrate |
| **ladybug-comfort** | 99 | — | DEAD/ILLUSORY | PMV/UTCI/PET comfort models |
| **dragonfly-core** | 66 | — | DEAD/ILLUSORY | district 2.5-D massing |
| **dragonfly-energy** | 68 | — | DEAD/ILLUSORY | URBANopt/DES/OpenDSS translation |
| **queenbee** | 121 | — | DEAD + DUP | owner=geometry vs runtime/.api owner=runtime; `[RUNTIME]`-tagged; 257 differing lines |
| **lbt-recipes** | 88 | — | DEAD + DUP | `[RUNTIME]`-tagged; 160 differing lines vs runtime tier |
| **pollination-handlers** | 84 | — | DEAD + DUP | `[RUNTIME]`-tagged; 127 differing lines vs runtime tier |

DEAD/misplaced total: 3 recipe (293) + 10 energy (930) + 3 codec (332) = **~1555 LOC (~39% of the geometry .api tier)**.

## [B] SUBSTRATE TIER (libs/python/.api, 27) — geometry relevance

Geometry's direct substrate consume is **numpy only** (heavy: structural MOMENT_KERNELS/eigh, quality half-edge folds, spatial lexsort, features reductions) — `numpy.md` (branch-owned) is well-mined, no geometry gap. All other substrate (msgspec/beartype/expression/anyio/structlog/opentelemetry/protobuf/grpcio/xxhash) is consumed **through runtime seams**, not directly — correctly runtime/data-owned. `xxhash.md` owner=runtime (geometry reaches it only via `ContentIdentity`). **`networkx.md` owner=data** — the cross-folder defect (see VC4). No geometry-owned catalog is misplaced *into* the substrate tier.

---

## [C] CROSS-CUTTING FINDINGS

**C1 — Energy band is an unwired domain (illusory capability).** 13 catalogs (~1223 LOC), 10 `[GEOMETRY]`-tagged + 3 `[RUNTIME]`-tagged packages, README `[01]` prose charter ("the out-of-process AGPL Ladybug Tools energy/environmental companion band") + `[02]-[ENERGY]` roster (README 11 mentions). ABSENT from: ARCHITECTURE.md codemap+seams (`rg` energy → zero), README `[01]-[ROUTER]` (pages [01]–[13], none energy), IDEAS.md, TASKLOG.md, `[03]-[COMPANION_LANES]`. `rg -i 'ladybug|honeybee|dragonfly|queenbee|comfort|energyplus|openstudio|hbjson|epw' .planning/` → ZERO (one `costing.md` "recipe" hit = ifcpatch, not Ladybug). Worse than data's "impact plane illusory": honeybee-energy.md:126-135 fully authors the consuming owner's rail composition (anyio.to_thread offload + stamina retry + otel span + numpy/xarray result decode) for an owner with no page.

**C2 — Recipe trio: divergent duplicate catalogs across two tiers.** queenbee/lbt-recipes/pollination-handlers cataloged in BOTH `runtime/.api` (owner=runtime, "the runtime recipe owner composes", per runtime brief V3 `execution/recipe.md`) AND `geometry/.api` (owner=geometry, "the geometry energy rail"), divergent by 257/160/127 lines. pyproject tags all three `[RUNTIME]` (pyproject:140-142). Runtime is canonical; the geometry-tier copies are misplaced duplicates for the nonexistent geometry energy owner.

**C3 — Mesh-file codec catalogs are data-owned, dead in geometry.** rhino3dm(153)/meshio(75)/laspy(104) `[GEOMETRY]`-tagged + README-listed + ARCHITECTURE companion-lane ("runtime lane carries … rhino3dm, laspy"), but every page defers the codec to data (`repair.md:258` CODEC_SEAM: "the `.3dm` leg over `rhino3dm.File3dm`, the FEM leg over `meshio` … is NOT this owner: the data `MeshPayload` owner holds the three-engine `trimesh`/`meshio`/`rhino3dm` codec"; TASKLOG `SCAN_COPC_PARTIAL` cedes LAS decode to data). `trimesh` is the legit-dual exception (data codec + geometry in-memory ops). rhino3dm/meshio/laspy have no geometry import.

**C4 — networkx analytics ownership contradiction (cross-folder).** `graph/features.md` is the repo's heaviest networkx-analysis consumer — 11 algorithm families (`features.md:397-408`: connected/weakly/strongly_components, betweenness/degree/closeness/eigenvector/pagerank, minimum_spanning_tree, simple_cycles, louvain_communities). It cites `data/.api/networkx.md` as owner-of-record (`features.md:508`) and geometry has NO networkx.md. Data brief `[04]` + V12: networkx relocates branch→`data/.api/`, and "the analysis surface is permanently DECLINED — rustworkx owns analysis". The single owning catalog (data) scopes OUT exactly what geometry's heaviest consumer mines.

**C5 — content_identity module-name drift (breaks on runtime V4).** 12/12 code pages import `rasm.runtime.content_identity` (ContentIdentity/ContentKey/IdentityPolicy/CANONICAL_POLICY). Runtime brief V4 deletes that spelling for `rasm.runtime.identity`. Every geometry prelude is unimportable the moment runtime lands. Mechanical 12-site rename (mirrors data brief V3's 22-site sweep).

**C6 — compute-graduation forward-import (geometry hard-imports its downstream).** 12/12 code pages `from rasm.compute.graduation.handoff import GeometrySubject` (+ GraduationReceipt/HandoffAxis on ifc/graph/scan pages). compute is the LAST folder (runtime→data→geometry→compute); geometry is unimportable until compute lands, and the ARCHITECTURE `⇄` seam (`graph/algebra ⇄ python:compute/graduation`) means compute also consumes geometry — folder-cycle risk. No compute brief exists yet. The literal is a decode-only string union; currently a deep import.

**C7 — banned [RESEARCH]/[UPSTREAM] tails on all 18 pages.** All 18 carry `[03]-[RESEARCH]`; 11 also carry `[04]-[UPSTREAM]`. `brep.md:423-426`, `spatial.md:288-291` are EMPTY stub headers (delete). analysis/selector/structural/costing RESEARCH tails are LOAD-BEARING member/arity confirmations → purge-and-fold into Packages block or `.api` catalog per runtime/data V11; empty ones delete.

**C8 — unadmitted native spatial backends (domain gaps).** `spatial.md` Bounds + CORE-clearance arms depend on `rtree` (triangles_tree R-tree) + `python-fcl`/`fcl` (CollisionManager.min_distance_single) + `embreex` (ray accel), find_spec-gated (`spatial.md:124,246`), but NONE is in pyproject and NONE has an `.api` catalog. The page admits "the manifest does not declare and no package ships, so they resolve only on the worker companion" (`spatial.md:3`) — self-hand-waved; those arms are illusory without admission. Contrast the disciplined `gmsh` deferral (TASKLOG `GMSH_BUILD_EXCLUSION_MANDATE`: no catalog until a consuming fence exists) — gmsh is deferred *without* a consuming fence; rtree/fcl HAVE consuming fences yet no admission.

**C9 — hardcoding vs generator: clean.** Realized pages are table-driven throughout (READERS/_APPLY, _PRIMITIVES/_BOOLEANS/_FEATURES/_JOINS, MOMENT_KERNELS/PROFILE_SAMPLERS, FEATURE_OPS/MARK_PROJECT/ANALYTICS/CASE, NUMERICAL/DATASTRUCTURE/_FORM, QUERY_SPLIT/PHASE_DELIMITER, _CONDITION/_OPTYPES). No enumerated-instance-where-a-generator-belongs defect in the code pages. The one code-fence bug: `quality.md:344` `area = exact.area…` is dedented out of its method (fence defect, not api-tiers).

**C10 — dead typed carriers / concern mixing: none material in realized pages.** Receipts are uniformly `ReceiptContributor`-on-the-carrier with leaf `.fact()` projection; no parallel receipt rails; no `Codec`/`load`/`export` arms leaking the data seam. Concern separation is crisp (repair=condition/boolean, spatial=query, quality=metrology, brep=exact-kernel, cad=STEP hop, daemon=tessellation). `nonmanifold.md` imports `topologicpy`+`ifcopenshell` module-top (vs the function-local worker-lane discipline every other page holds) — minor base-purity drift in the AGPL opt-in lane.

**C11 — unmined capability with catalog anchors (the folder never demands).**
- cadquery-ocp `Geom_*` NURBS curves/surfaces (`cadquery-ocp.md:61-71`), `BRepBuilderAPI_Sewing`/`NurbsConvert` (`:84,86`), BOPAlgo `SetFuzzyValue`/`SetGlue`/`History`/`SectionEdges` (`:227-228`) — no NURBS-eval or fuzzy-boolean-tolerance owner.
- ifctester `Json(ids).report()` Results graph `percent_checks_pass`/`total_applicable_fail` (analysis:418 flags it as a sharper unmined residual).
- trimesh `registration`/`voxel`/`section`/`split` entrypoints (`trimesh.md:128,150`) — unused (open3d owns registration; no voxel owner).
- The entire 13-catalog energy surface — 0% mined.

---

## [D] UPSTREAM WATERFALL CANDIDATES (recorded, NOT applied — read-only lane)

Framed as consumer pressure with geometry named; the author leg should apply these surgically to the upstream briefs.

- **W1 — RUNTIME lanes/offload (V5 escalation row).** Geometry `mesh/daemon` demands `LanePolicy.offload(kernel, *args, retry=RetryClass)` — the offload leg wrapping the PEP-734 `interpreter_run_sync` in `resilience#guard(RetryClass.OCCT)` so a transient `BrokenWorkerInterpreter` cold-start retries on the offload leg without a fourth `Admit` case (`daemon.md:199`, `[KEYED_ADMISSION]:220`). Also the no-pickle `anyio.to_interpreter.run_sync` path (TASKLOG `GEOMETRY_CPU_OFFLOAD`). Add geometry/mesh + graph/algebra as named consumers of the `retry=` offload keyword.
- **W2 — RUNTIME resilience (V5 POLICY table).** `RetryClass.OCCT` + the `anyio.BrokenWorkerInterpreter`-keyed occt POLICY row (V5 already keeps the "occt" row): name geometry `daemon`/`cad` as the demanding consumers and export the row/member (`daemon.md:220`, `cad.md:20`). algebra additionally binds `RetryClass.RPC`/`guarded` for `compas.rpc.Proxy` bring-up (`algebra.md:354`) — name it too.
- **W3 — RUNTIME identity (V7).** Geometry `mesh` demands the runtime identity owner's `IdentityPolicy`/`ContentIdentity.of` carry (a) a tessellation-knob `spec` (deflection/angle_tolerance/tolerance) folded `.17g`-formatted into the XxHash128 seed so a coarse vs fine pass keys distinctly (`daemon.md:15`; cad/brep read `policy.deflection`/`angle_tolerance`), and (b) the seed-zero (`seed=Some(0)`) GLB **wire-representation-key** path distinct from the policy-folded **re-tessellation cache-key**, byte-aligned to the C# `Rasm.Element/Graph RepresentationContentHash` (`daemon.md:223` `[REPRESENTATION_KEY]`). If V7's `IdentityPolicy` is content-policy-only, this is a named migration pressure: either the identity owner carries the tessellation spec, or geometry owns a `TessellationPolicy` composing it — pin the seam.

DATA brief needs no geometry-driven capability edit — geometry consumes data at the seam (MeshPayload cell-block, Arrow point-record bridge, mesh-file codec) as a downstream reader; the data brief already scopes those (VERIFIED-AT-DEPTH mesh `_BACKEND`, laspy COPC). The networkx contradiction (C4) is a data `[04]`-row scoping edit candidate: the networkx catalog must serve geometry's analysis consume, not blanket-decline it — but this is a catalog-ownership reconciliation the geometry brief flags, not a capability the data brief lacks.

---

## [E] VERDICT CANDIDATES (strongest, campaign-defining)

**VC1 — ENERGY BAND IS AN ILLUSORY DOMAIN; realize-or-expel (the defining ruling).** 13 catalogs (~1223 LOC), 10 `[GEOMETRY]`+3 `[RUNTIME]` packages, README prose+roster, but zero page/codemap/router/card. Rule floor: AUTHOR an `energy/` sub-domain (climate over ladybug-core, comfort over ladybug-comfort, building-energy over honeybee-core/energy/openstudio/standards, district over dragonfly-core/energy — HBJSON/DataCollection/result evidence across the wire, EnergyPlus/OpenStudio subprocess through runtime's recipe rail + LanePolicy.offload) — mirroring runtime brief V3 authoring the recipe owner and data brief V4 realizing impact. Expel arm: strike the 10 geometry packages + catalogs + README `[01]`/`[02]-[ENERGY]`. Given the depth, README charter, and "planned consumers are real design pressure," realize is the ruled default — but it roughly doubles the folder's sub-domain count and must be scoped as its own leg with its own seam ledger (energy ⇄ runtime/recipe, energy → data for HBJSON/result frames, energy → artifacts for viz). Evidence: `rg` energy → zero pages; honeybee-energy.md:126-135 authors the nonexistent owner's rail.

**VC2 — STRIKE THE GEOMETRY-TIER RECIPE-TRIO DUPLICATES.** queenbee/lbt-recipes/pollination-handlers are `[RUNTIME]`-tagged and owned by runtime brief V3 (`execution/recipe.md`); the geometry-tier catalogs (owner=geometry, divergent 257/160/127 lines) are misplaced. Rule: delete geometry/.api/{queenbee,lbt-recipes,pollination-handlers}.md; if VC1 realizes energy, the energy pages consume runtime's recipe rail (RecipeExecution) at the seam, never re-catalog the trio. Evidence: pyproject:140-142 `[RUNTIME]`; runtime brief E3.

**VC3 — MESH-FILE CODEC CATALOGS BELONG TO DATA; strike geometry's rhino3dm/meshio/laspy.** The `.3dm`/FEM/LAS codec is data's `spatial/mesh` three-engine `_BACKEND`; geometry uses in-memory `trimesh` only and consumes data's `MeshPayload`/Arrow bridge at the seam. Rule: strike geometry/.api/{rhino3dm,meshio,laspy}.md + retag/remove from pyproject `[GEOMETRY]` + README MESH_CAD/SCAN + ARCHITECTURE companion-lane (drop "rhino3dm, laspy" from the runtime lane); trimesh stays (legit-dual). Evidence: repair.md:258 CODEC_SEAM; SCAN_COPC_PARTIAL; data brief VERIFIED `meshio/trimesh/rhino3dm _BACKEND` + laspy COPC.

**VC4 — RECONCILE networkx OWNERSHIP FOR THE ANALYSIS SURFACE geometry mines.** features.md (11 nx-analysis families) cites data/.api/networkx.md, which data brief V12 scopes to codec-only + "analysis permanently DECLINED to rustworkx." Rule: either (a) author `geometry/.api/networkx.md` documenting the centrality/community/connectivity/spanning/cycle analysis surface geometry owns as consumer (the branch-law exception: a domain catalogue lives with its consuming folder), OR (b) the single networkx catalog carries an explicit geometry-analysis section (dual-consumer owner line), OR (c) migrate features.md analytics to rustworkx (data's mandate). Ruled default: (a) — geometry is the analysis consumer of record; data owns codec. This is a bidirectional-ripple edit to the data campaign's networkx scoping. Evidence: features.md:508; data brief `[04]` networkx row + V12.

**VC5 — CORPUS-WIDE content_identity → identity RENAME + COMPUTE-GRADUATION SEAM PIN.** (a) 12-site `rasm.runtime.content_identity` → `rasm.runtime.identity` (runtime V4). (b) Pin `rasm.compute.graduation.handoff` (GeometrySubject/HandoffAxis/GraduationReceipt) as a decode-only boundary contract compute owns and geometry consumes — the geometry brief states the forward-dependency + a note that no compute brief exists yet, so geometry must not deep-couple the compute interior (the literal is a string union geometry can type-alias at the seam). Both are graph-level "unimportable as designed" defects (the exact class runtime/data briefs rank #1). Evidence: 12/12 pages both imports; ARCHITECTURE `⇄` compute seam.

**VC6 — PURGE-AND-FOLD the [RESEARCH]/[UPSTREAM] tails on all 18 pages.** Empty stub headers (brep, spatial) delete; load-bearing member confirmations (analysis:414-423, selector:316-324, structural:518-529, costing, features:505-514, algebra:409-415) fold into the owning Packages block or the domain `.api` catalog (where member truth lives). Runtime/data V11 precedent at geometry scale. Evidence: `rg '## \[0.\]-\[RESEARCH|UPSTREAM\]'` → 18 RESEARCH + 11 UPSTREAM headers.

**VC7 — ADMIT-OR-NARROW the spatial native backends (rtree/python-fcl).** spatial.md Bounds + CORE-clearance depend on unadmitted, uncatalogued rtree/python-fcl/embreex (find_spec-gated, self-admitted "manifest does not declare"). Rule: admit rtree + python-fcl to pyproject (worker-lane tag) with `.api` stubs and the consuming-fence discipline, OR narrow spatial to the manifold3d-only clearance path + drop the Bounds arm until a real backend lands (embreex stays a transparent in-process trimesh accel, no admission). Evidence: spatial.md:3,124,246; no catalog for any of the three.

**VC8 — COLLAPSE THE THREE GRAPH `_LIMITER` DECLARATIONS (minor structural).** features `_ANALYTIC_LIMITER`, algebra `_SOLVER_LIMITER`, nonmanifold `_ANALYTIC_LIMITER` each independently mint a `CapacityLimiter(4)` with near-identical prose ("the per-owner 4-slot PATTERN … each mint INDEPENDENTLY, never a …" — the comment is even truncated mid-sentence at features.md:132 and algebra.md:98). Either the 4-slot worker-pool pattern is a shared `graph`-level constant, or the "never a shared" rationale must be completed and justified. Evidence: features.md:133, algebra.md:100, both with dangling comments.
