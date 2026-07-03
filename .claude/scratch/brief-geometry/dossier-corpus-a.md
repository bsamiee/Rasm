# DOSSIER — geometry/.planning corpus-a (graph + ifc first-half, + governance + upstream FIT)

Lane: corpus-a. Deep-read FULLY: `graph/{algebra,features,nonmanifold}.md`, `ifc/{analysis,authoring,costing,selector,structural}.md`, geometry `ARCHITECTURE.md`/`README.md`/`TASKLOG.md`/`IDEAS.md`, `RASM-RUNTIME-BRIEF.md`, `RASM-DATA-BRIEF.md`, `libs/.planning/architecture.md`, `RASM-COMPONENT-PARADIGM-DECISION.md#[AMENDMENTS]`. Referenced (not deep): geometry `.api/` tree (38 catalogs), corpus-wide greps over all 18 pages. Second half (`mesh/`, `scan/`) = corpus-b.

Stance: HOSTILE. The graph+ifc pages are genuinely dense, doctrine-fluent, catalogue-grounded page-craft (page scores mostly 8-9). The DEFECTS are structural/graph-level and governance-level, not per-page: an entire orphaned domain, a corpus-wide upstream break, a banned page-grammar surface, an offload-lane split, and 2-3× duplicated analytics/weave mechanisms — none fixed by a per-page cold pass.

---

## [A] PER-PAGE VERDICTS

### graph/algebra.md — 8/10
Charter (as-is, correct): `ComputationalGeometry` `@tagged_union` over compas — network/form-finding(DR|TNA)/numerical/datastructure; one `CASE` subject/ledger/ceiling table; `NUMERICAL` primitive+transform table with per-row RPC route; `solver_proxy` async CM over `compas.rpc.Proxy`; polymorphic single-or-batch `run` + async `bridged`. World-class compas weave; the RESEARCH section catches real upstream bugs (the `vertices_attributes(values=)` broadcast defect at :412, the `Constraint(geometry)` polymorphic-factory correction).
Defects:
- `algebra.md:98-99` — TRUNCATED dangling comment: "...siblings each mint INDEPENDENTLY — not a" then a new sentence begins; the sentence is cut mid-clause. Copy-paste artifact shared with features/nonmanifold.
- `algebra.md:49` — imports `rasm.runtime.content_identity` (drift; runtime `[V4]` renames to `rasm.runtime.identity`). See VC2.
- `algebra.md:100` — `_SOLVER_LIMITER = CapacityLimiter(4)` hand-rolled; `bridged` offloads via `anyio.to_thread.run_sync` (:354,361,405) instead of runtime `LanePolicy.offload`, which mesh/brep,repair,daemon already consume. See VC4.
- `algebra.md:323,409` — Numerical case subject `"numerical-primitive"` (subject overload, VC7); `[03]-[RESEARCH]` tail banned by runtime/data `[V11]` (VC3).
Owner charter as it SHOULD be: unchanged concern; consume runtime `LanePolicy.offload(THREAD)` instead of the local limiter; fold the RESEARCH member-confirmations into `Packages`; rename identity import.

### graph/features.md — 9/10
Charter (correct): `Features` producer minting `FeatureResult`; `FEATURE_OPS` detect/project table (sharp-edge/planar/curvature/boundary as 4 rows); `MARK_PROJECT` 3-arm projection over `MarkSpace`; `ANALYTICS` 11-row networkx table under one `backend=` axis with `mode_guard` data-gating; heavy/light offload split. Best page in the subfolder — the `AnalyticValue` collapse, the `edges_face` incidence read, the `Census.facts` peak/as_scalar projection are exemplary.
Defects:
- `features.md:157-201` — defines `AnalyticValue` @tagged_union (scalar/leaderboard/groups + as_scalar + peak) VERBATIM-identical to `nonmanifold.md:111-161`. See VC5.
- `features.md:132-133` — same TRUNCATED comment ("...each mint INDEPENDENTLY, never a") + `_ANALYTIC_LIMITER = CapacityLimiter(4)` hand-rolled (VC4).
- `features.md:44,505` — content_identity drift (VC2); `[03]-[RESEARCH]` tail (VC3).
Owner charter as it SHOULD be: unchanged; consume a shared graph-analytics `AnalyticValue`/leaderboard owner + runtime offload lane.

### graph/nonmanifold.md — 7/10
Charter (correct): `TopologyOp` @tagged_union over topologicpy static namespace; `_CONSTRUCT`/`_BOOLEAN`/`_ANALYSIS` tables; `GRAPH_ANALYTIC` dual-graph fold; `topology-graph` the sole graduating subject. The RESEARCH section forced 5 real source corrections (`ConnectedComponents` vs `Connectivity` alias, `MinimumSpanningTree` vs `Tree`, polymorphic `Topology.Vertices`).
Defects:
- `nonmanifold.md:456-457` — STRAY XML TAGS in the file body: literal `</content>` then `</invoke>`. Hard corruption artifact.
- `nonmanifold.md:454` — EMPTY `[04]-[UPSTREAM]` header (dead section, VC3).
- `nonmanifold.md:111-161` — `AnalyticValue` duplicated with features (VC5).
- `nonmanifold.md:98-99` — TRUNCATED comment; `:99` `_ANALYTIC_LIMITER = CapacityLimiter(4)` hand-rolled offload (VC4).
- `nonmanifold.md:42,449` — content_identity drift (VC2); `[03]-[RESEARCH]` tail (VC3).
- Whole page is authored against `topologicpy` which is AGPL-BLOCKED from the default build (page head :5; TASKLOG `[IFC_BYTES_CONSTRUCTOR]`/`[GEO_TOPOLOGICPY_LICENSE_REPLACE]` BLOCKED) — speculative until the AGPL lane or the `cadquery-ocp`/`compas` permissive replacement lands. Legitimate but the entire owner is contingent.
Owner charter as it SHOULD be: unchanged concern; strip stray tags + empty header; consume shared AnalyticValue + offload lane; the permissive-backend replacement (`[GEO_TOPOLOGICPY_LICENSE_REPLACE]`) is the real forward path.

### ifc/analysis.md — 8/10
Charter (correct): `IfcAnalysis` — quantity/pset/IDS/clash/space-program/BCF over ifcopenshell.util/ifctester/ifcclash/bcf; one `AnalysisRow` @tagged_union with `of_*` constructors + total `facts`; `QUERY_SPLIT` table; `_CLASH_RETRY` stamina policy; self-owned `content.analysis` span; selector gate the sole `filter_elements` caller. The IDS structured-verdict fold (real per-element passing fraction, `status is None` exclusion at :303-311) and the space-program `evidence`-poison guard (:265-274) are genuinely sharp.
Defects:
- `analysis.md:121` — `ANALYSIS_SUBJECT = "numerical-primitive"` (subject overload, VC7).
- `analysis.md:416-417` — TWO honestly-flagged UNRESOLVED provider shapes ridden as typed fields: the clash-group key spelling (`clash_set["clash_groups"]` — `cluster` index defaults 0 until "the live run confirms") and `TopicHandler.topic.title`. Illusory-precision risk; verification debt.
- `analysis.md:126` — `_TRACER = get_tracer("geometry.ifc.analysis")` + `run`/`_ok`/`_emit` span weave triplicated with costing/structural (VC6).
- `analysis.md:37,414` — content_identity drift (VC2); `[03]-[RESEARCH]` tail (VC3).
Owner charter as it SHOULD be: unchanged; distinct BIM-evidence subject (not `numerical-primitive`); compose the shared evidence-run span weave; resolve the two provider shapes.

### ifc/authoring.md — 9/10
Charter (correct): `IfcAuthor` — transactional verb script over `ifcopenshell.api.<module>.<action>`; `IfcApiVerb` row table IS the vocabulary; `Capability` Flag; `AuthorPayload` @tagged_union collapsing verbs into argument-shape families; `@transactional`/`@stamped` AOP over one immutable `AuthorCarry` fold; `AuthorReceipt` IS the ReceiptContributor. Exemplary — the `to_kwargs` railed slot-resolve, the `_keyed` per-usecase keyword re-key, the OWNER_NEW/OWNER_UPD two-row split are award-grade. Correctly does NOT graduate (no GeometrySubject, no content_identity import).
Defects:
- `authoring.md:311` — `[03]-[RESEARCH]` tail (VC3).
- `authoring.md:317` — one flagged unresolved usecase-return shape (relating usecases returning relationship-instance vs None) — minor verification debt, `Capability` flag already gates it.
- NOT in the README router (VC8) though present in ARCHITECTURE codemap.
Owner charter as it SHOULD be: unchanged — this is the reference page. The `[GEO_IFC_AUTHORING_PIPELINE]` DAG-campaign idea is the correct forward extension.

### ifc/costing.md — 8/10
Charter (correct): `IfcLifecycle` — 5D/4D over ifc5d/ifc4d/ifcpatch/ifcdiff; `LifecyclePhase` quantity/cost/schedule/patch/diff; `LifecycleRow` @tagged_union; generic `_token[E: StrEnum]` rail-validated vocabulary parse; `PHASE_DELIMITER` table; `DiffChange.of_register` marker fold; self-owned `content.lifecycle` span. The `population`-divided drift ledger (avoids changed-over-changed always-1.0 at :419-423) and the rule-driven `ifc5d.qto.quantify` take-off are strong.
Defects:
- `costing.md:439` — EMPTY `[04]-[UPSTREAM]` header (dead trailing section, last line of file, VC3).
- `costing.md:157` — `LIFECYCLE_SUBJECT = "numerical-primitive"` (VC7).
- `costing.md:168` — span weave triplicated (VC6).
- `costing.md:430,431` — schedule `.file`/`.xml`/`.work_plan` slot semantics + patch-output shapes flagged "live-run confirms" (verification debt).
- `costing.md:35,426` — content_identity drift (VC2); `[03]-[RESEARCH]` tail (VC3).
Owner charter as it SHOULD be: unchanged concern; distinct lifecycle subject; shared span weave; delete empty `[04]` or populate it.

### ifc/selector.md — 9/10
Charter (correct): `IfcSelector` — one lark EBNF grammar faithful to `filter_elements_grammar`; `Facet` @tagged_union collapsing 11 upstream facets onto 4 cases; `SelectorComparison` value object; `SelectorQuery` IS the ReceiptContributor; polymorphic `parse(str|Iterable[str])`; `@cache` engine; round-trip `filter_string`. Cleanest page in the corpus — the fabricated-grammar demolition (:318, deleting `@`-decomposition / leading-`/` / `classification.X=Y` / bare-`*`) and the `_emit_token` re-quote round-trip are exemplary. Reconciles the analysis `IDS_SELECTOR_GRAMMAR` residual (:323). Correctly does NOT graduate (no content_identity).
Defects:
- `selector.md:316` — `[03]-[RESEARCH]` tail (VC3) — the ONLY defect; its `[SELECTOR_FILTER_GRAMMAR]`/`[FACET_ROW_ALGEBRA]` bodies are load-bearing and must fold into `Packages`/.api, not delete.
- NOT in the README router (VC8) though present in ARCHITECTURE codemap.
Owner charter as it SHOULD be: unchanged — reference page.

### ifc/structural.md — 9/10
Charter (correct): `IfcStructural` — numpy Green's-theorem `MOMENT_KERNELS` section-integral spine + two gated tiers (IFC_ENTITY topology, WARPING sectionproperties FE) folded into one `Enrichment` @tagged_union; `PROFILE_SAMPLERS` parametric-subtype table + arbitrary-closed fall-through feeding one shape-agnostic contour integral; `@railed` effect.result dispatch; `beartype.vale.Is` ClosedRing refinement; `_FE_RETRY`; self-owned `content.section` span; includes a mermaid flow. Largest (452 LOC) and most sophisticated. The centroid-relative `_extreme_fibers` (not half-bbox), the FE-void closed-facet-loop carving, the `fe-area` convergence residual are award-grade.
Defects:
- `structural.md:74` — `STRUCTURAL_SUBJECT = "numerical-primitive"` (VC7).
- `structural.md:78` — span weave triplicated (VC6).
- `structural.md:518,526` — `[03]-[RESEARCH]` tail + `[04]-[UPSTREAM]` (populated, but same appendix class, VC3).
- `structural.md:42` — content_identity drift (VC2).
- Card-bullet ordering: `[02]` leads "Selector gate:" then "Entry:" — analysis/costing lead "Owner:"/"Cases:". Minor inconsistency across the ifc pages.
Owner charter as it SHOULD be: unchanged — reference page; distinct section-property subject; shared span weave.

---

## [B] GOVERNANCE PAGES

### ARCHITECTURE.md — 7/10
Rich: codemap (scan/ifc/mesh/graph), 24-row seam ledger (content-key/tessellation/graduation/data seams), companion-lane band. Defects:
- Codemap includes `graph/features`, `ifc/authoring`, `mesh/{brep,quality,spatial}` (all 5 realized) — but the README router does NOT (VC8). Codemap and router disagree.
- NO energy node anywhere in the codemap, yet README headline (line 3) and `[ENERGY]` package group (13 packages) claim the domain (VC1).
- Seam ledger records the graduation/content-key/data seams but NOT: the intra-geometry offload-lane consumption (graph hand-rolls vs mesh consumes `LanePolicy.offload`), nor the identity-rename migration. Ledger blind to the two corpus-wide structural facts.

### README.md — 5/10
`[01]-[ROUTER]` lists `[01]`–`[13]`, stopping at ALGEBRA — 5 realized pages unrouted (features, authoring, brep, quality, spatial). `[02]-[DOMAIN_PACKAGES] [ENERGY]` (lines 59-72) enumerates 13 Ladybug/Honeybee/Dragonfly packages (each with a 12-28KB `.api` catalog) and line 3 headlines "the out-of-process AGPL Ladybug Tools energy/environmental companion band" — with ZERO owning design pages and ZERO IDEAS/TASKLOG cards. Largest prose-vs-fence split in the folder (VC1). Router staleness (VC8).

### TASKLOG.md — 6/10
Well-reasoned BLOCKED cards (topologicpy AGPL dual-gate, gmsh GPL exclusion, sectionproperties warping). Gaps:
- `[GEOMETRY_CPU_OFFLOAD]` (QUEUED) migrates `registration/daemon/ingestion/reconstruction/repair` to `LanePolicy.offload` but OMITS the three graph pages (algebra/features/nonmanifold), which each hand-roll `to_thread.run_sync`+`CapacityLimiter(4)` (VC4). The task's Shape/Anchors under-cover the corpus.
- NO identity-rename task (VC2), NO `[RESEARCH]`-purge task (VC3), NO energy-band task (VC1).

### IDEAS.md — 7/10
Genuine forward pool (authoring DAG campaign, pointcloud instance labelling, parametric layout, clash/QTO rollup, topologicpy license replace). Gaps: no energy idea despite 13 energy packages (VC1); no identity-migration idea.

---

## [C] CROSS-CUTTING

**Duplication.**
- `AnalyticValue` @tagged_union authored verbatim in features.md:157-201 + nonmanifold.md:111-161 (scalar/leaderboard/groups + as_scalar + peak identical); `_leaders`/`_ranked` leaderboard ranking + census-peak fold duplicated across all three graph pages. Pages defend "each mint INDEPENDENTLY" (the truncated comment). Latent shared graph-analytics substrate owner authored 2-3×. (VC5)
- The three graduating IFC owners (analysis/costing/structural) triplicate the entire span weave: `_TRACER=get_tracer("geometry.ifc.X")` (126/168/78) + `run` (content.X span → `boundary(...).bind(identity).map(_emit).map(lambda r:_ok(span,r))`) + identical static `_ok(span,r)` / `@receipted(_REDACTION) _emit(r)` + `_REDACTION=Redaction(classified=Map.empty())`. Six identical structural elements ×3. (VC6)
- Three independent `CapacityLimiter(4)` (algebra:100, features:133, nonmanifold:99) + identical truncated comment (algebra:98, features:132, nonmanifold:98). (VC4)

**Concern mixing / subject vocabulary.** `GeometrySubject "numerical-primitive"` carries four semantically distinct concerns: IFC compliance/clash/BCF verdicts (analysis), 5D/4D lifecycle (costing), section integrals (structural), AND compas numerical primitives (algebra Numerical case, :323). Ledger keys differentiate at the ceiling so no correctness bug, but the subject is coarse. Compute-owned vocabulary (`rasm.compute.graduation.handoff`). (VC7)

**Hardcoding-vs-generator.** Rosters are correctly generative (MOMENT_KERNELS, PROFILE_SAMPLERS, FEATURE_OPS, ANALYTICS, GRAPH_ANALYTIC, IFC_API_VERBS, _CONSTRUCT/_BOOLEAN/_ANALYSIS, NUMERICAL, QUERY_SPLIT, PHASE_DELIMITER — all one-row-to-extend). No enumerated-instance-where-generator-belongs defects found in graph/ifc. The `CapacityLimiter(4)` literal ×3 is the one hardcoded value that should be a single runtime-owned band bound (VC4). The section/analytics tables are the folder's genuine strength — preserve.

**Dead carriers / structural defects.** nonmanifold.md:456-457 stray `</content></invoke>` tags; nonmanifold.md:454 + costing.md:439 empty `[04]-[UPSTREAM]` headers; all 18 pages carry `[03]-[RESEARCH]` (11 also `[04]-[UPSTREAM]`) — banned appendix class (VC3).

**Unwired seams.** (a) Graph offload never consumes the `LanePolicy.offload` lane the mesh subfolder consumes — a seam split INSIDE the folder (VC4). (b) The energy band (13 packages, 13 catalogs, 6 ARCHITECTURE seams' worth of implied capability) is wired to nothing (VC1). (c) content_identity rename unrecorded in ARCHITECTURE/TASKLOG (VC2).

**Unmined capability with catalog anchors.**
- Energy: `.api/{ladybug-core,ladybug-comfort,ladybug-geometry,honeybee-core,honeybee-energy,honeybee-energy-standards,honeybee-standards,honeybee-openstudio,dragonfly-core,dragonfly-energy,queenbee,lbt-recipes,pollination-handlers}.md` — 13 dense catalogs backing ZERO fences. Either the largest capability delta available (stand up `energy/`) or 13 catalogs of dead weight (VC1).
- Runtime `LanePolicy.offload` isolation-modality axis (runtime `[V5]`) — graph pages could compose the THREAD modality instead of hand-rolling (VC4).
- Runtime `Keyed[T]`/`StagePlan` content-addressed cache (runtime `[04]` TASKLOG absorption confirms the seam) — correctly targeted by `[GEO_IFC_AUTHORING_PIPELINE]` idea; not yet a fence.

---

## [D] UPSTREAM FIT + MIGRATION PRESSURE + WATERFALL-RIPPLE DEMANDS

**Clean FIT (preserve — do NOT touch):** The geometry↔upstream seam discipline is genuinely strong and consistent with both briefs' "decode-not-re-mint" law. Graph/ifc consume `ContentKey` as an opaque runtime-minted evidence key passed to `graduates(evidence_key)`; they never author a canonical-writer mirror — consistent with data `[SEAM_AND_RAIL_LAW]` ("data feeds canonical bytes and never authors a canonical-writer mirror") and the `[AMENDMENTS]` item-1 count-prefix law (C#-owned writer; py/ts mirrors "do not exist yet", `PY_WIRE_ALIGNMENT` queued). The `Receipt.of(owner, (phase, subject, facts))` 2-arg contract, `ReceiptContributor`, `@receipted`, `boundary`/`traversed`/`Disposition`/`railed`/`FAULT_CONF`, `guarded(RetryClass.RPC)` are all consumed by symbol from the runtime owners the runtime brief preserves. The `GraduationReceipt.graduates(HandoffAxis(geometry=...))` rail is compute-owned and correctly consumed at the seam. The C#-fit (Rasm.Bim IFC semantic authority vs Python offline evaluation meeting at the wire) matches `libs/.planning/architecture.md#[04]-[GEOMETRY_FLOW]` exactly — geometry is a peer producer, not a Rasm consumer.

**MIGRATION PRESSURE 1 (both briefs, load-bearing):** Runtime `[V4]`/`[V7]` rule the canonical module name `rasm.runtime.identity`; data `[V3](a)` records the `content_identity → identity` rename as a first-class corpus-wide rebuild motion (22 sites). Geometry has the identical drift in 12 pages (`content_identity` at algebra:49, features:44, nonmanifold:42, analysis:37, costing:35, structural:42, + mesh/scan) — including the daemon content-key MINTER (daemon.md:36 imports `CANONICAL_POLICY, ContentIdentity, ContentKey, IdentityPolicy`). Geometry brief MUST carry the rename motion corpus-wide (mirror data `[V3](a)`). (VC2)

**MIGRATION PRESSURE 2 (runtime `[V5]`):** `LanePolicy.offload` gains an isolation-modality axis (interpreter|process|thread). mesh/brep,repair,daemon already consume it (subinterpreter); graph pages hand-roll `to_thread.run_sync`+`CapacityLimiter(4)`. When the axis lands, graph should consume `offload(THREAD)`. (VC4)

**MIGRATION PRESSURE 3 (page grammar):** Runtime `[V11]` + data `[V11]` BAN the `[RESEARCH]` appendix folder-wide (purge-and-fold load-bearing anchors into Packages/.api). Geometry is next in the runtime→data→geometry→compute sequence; all 18 pages carry it. (VC3)

**WATERFALL-RIPPLE DEMAND (recorded for the brief author to apply; NOT applied here per read-only lane):**
- INTO `RASM-RUNTIME-BRIEF.md` `[V5]` (or `[04]` package-pressure): `[V5]` bounds only the PROCESS offload modality (`WORKER_BAND` = process-wide native-worker bound) and names no thread-modality bound or geometry consumer. Geometry's graph analytics owners (compas.rpc/networkx/topologicpy — GIL-releasing native cores) + scan registration ICP + mesh daemon OCCT are the demanding consumers of a BOUNDED THREAD modality; their stated rationale is "not draining anyio's default 40-token pool," and they currently hand-roll 3 uncoordinated `CapacityLimiter(4)`. Surgical owner-extension: pin the offload THREAD modality with its own process-wide thread-band bound, geometry named the demanding consumer that collapses its per-owner limiters onto the one lane. (Consumer-pressure clause, not a rewrite.) This is the FIT counterpart to VC4.
- Toward COMPUTE (brief not in my upstream set — record only): the `GeometrySubject` union's `numerical-primitive` is overloaded across 4 geometry owners; compute's `HandoffAxis`/`GeometrySubject` needs a differentiated BIM-evidence subject family. (VC7)

**Data-side seams (confirmed, corpus-b territory):** data `spatial/mesh` owns `MeshPayload` cell-block topology (geometry mesh/repair consumes at the mirrored `[SHAPE]` seam — TASKLOG `[MESH_TOPOLOGY_SHAPE]`); data decodes geometry scan Arrow point records (data `INGRESS_LEDGER`; geometry scan/ingestion produces); data leaves pdal for COPC via `laspy.copc` while geometry keeps its full pdal filter-graph (TASKLOG `[SCAN_COPC_PARTIAL]`). All three are boundary contracts, cleanly recorded — verify against corpus-b's mesh/scan deep-read.

---

## [E] STRONGEST VERDICT CANDIDATES (campaign-defining, evidence-first)

**VC1 — The energy band is an orphaned domain: stand it up or strike it.** README headline (README.md:3) + `[ENERGY]` 13-package group (README.md:59-72) + 13 `.api` catalogs (`.api/{ladybug-*,honeybee-*,dragonfly-*,queenbee,lbt-recipes,pollination-handlers}.md`, 12-28KB each) claim a full climate/HBJSON-building-energy/district-massing/thermal-comfort companion band with ZERO owning design pages, ZERO codemap nodes, ZERO IDEAS/TASKLOG cards. COVERAGE naivety at domain scale — an entire owned domain absent from the fence. Ruling: EITHER mint an `energy/` subfolder (owner pages consuming the 13 catalogs: climate/EPW+Sunpath, HBJSON building-energy+EnergyPlus/OpenStudio, dragonfly district massing, ladybug-comfort maps) OR strike the README headline + `[ENERGY]` group + 13 catalogs. Single largest prose-vs-fence split; the largest available capability delta or the largest dead-weight strike.

**VC2 — Corpus-wide identity-module rename migration, unrecorded.** 12 geometry pages import `rasm.runtime.content_identity` (algebra:49, features:44, nonmanifold:42, analysis:37, costing:35, structural:42, daemon:36 [the content-key minter], brep:58, cad:51, deviation:33, reconstruction:37, registration:31). Runtime `[V4]`/`[V7]` rename this to `rasm.runtime.identity`; data `[V3](a)` already carries the rename as a first-class rebuild motion (22 sites). Geometry has NO task/idea for it. Ruling: the geometry campaign carries the `content_identity → identity` rename corpus-wide as a mechanical leg-1 reconcile motion (mirror data `[V3](a)`). Load-bearing: it breaks the daemon content-key seam the moment runtime lands.

**VC3 — Purge the `[RESEARCH]`/`[UPSTREAM]` appendix tails (folder-wide page grammar).** All 18 pages carry `[03]-[RESEARCH]`; 11 also carry `[04]-[UPSTREAM]` (2 EMPTY: costing.md:439, nonmanifold.md:454). Runtime `[V11]` + data `[V11]` BAN the appendix folder-wide and rule purge-and-fold (load-bearing member confirmations fold into `Packages`/.api; freshness/provenance framing deletes). The geometry RESEARCH bodies ARE heavily load-bearing (algebra's compas-dr arity corrections, selector's grammar-parity proof, structural's numpy/sectionproperties member confirmations) — fold, never delete-blind. Additionally strike nonmanifold.md:456-457 stray `</content></invoke>` tags and the 2 empty `[04]` headers. Rule the `[04]-[UPSTREAM]` seam-ledger keep-or-fold explicitly.

**VC4 — Consolidate graph analytics onto the runtime offload lane the mesh subfolder already consumes.** mesh/brep:13, mesh/repair:7,17, mesh/daemon:14 consume `LanePolicy.offload` (subinterpreter). graph/algebra:100, graph/features:133, graph/nonmanifold:99 hand-roll `anyio.to_thread.run_sync` + 3 independent `CapacityLimiter(4)` with an identical TRUNCATED comment (algebra:98, features:132, nonmanifold:98). TASKLOG `[GEOMETRY_CPU_OFFLOAD]` migrates mesh/scan kernels but OMITS the graph owners. Ruling: graph analytics consume `LanePolicy.offload(THREAD)` (runtime `[V5]` isolation-modality axis); extend `[GEOMETRY_CPU_OFFLOAD]` Shape/Anchors to name the three graph owners. DUPLICATE MECHANISM + UNWIRED-SEAM + task-coverage gap. Carries the runtime `[V5]` waterfall-ripple (bounded thread-band, geometry the named consumer) in [D].

**VC5 — Consolidate a graph-analytics substrate owner.** `AnalyticValue` @tagged_union (scalar/leaderboard/groups + as_scalar + peak) is authored VERBATIM in features.md:157-201 and nonmanifold.md:111-161; `_leaders`/`_ranked` ranking + the census-peak Option fold duplicate across all three graph pages. The pages explicitly defend independent minting (the truncated comment). Ruling: the graph subfolder has a latent shared analytics owner (AnalyticValue carrier + leaderboard ranking + the bounded offload of VC4) the three producer pages (compas-adjacency / trimesh-networkx-projection / topologicpy-dual-graph) compose — DEAD-CARRIER duplication resolved by one owner, not three verbatim copies.

**VC6 — Collapse the triplicated IFC evidence span-weave.** analysis/costing/structural each hand-author the identical `_TRACER=get_tracer("geometry.ifc.X")` (126/168/78) + `run` (content.X span → `boundary(...).bind(identity).map(_emit).map(_ok)`) + static `_ok`/`_emit` + `_REDACTION`. Runtime `[V7]` rules the analogous "one span-fold core" collapse (`key`/`of` forking the tracing mechanism = the named defect). Ruling: one composed `evidence_run(tracer_scope, subject, dispatch)` weave the three owners parameterize by span name — DUPLICATE MECHANISM. (Selector/authoring use boundary+@receipted without the span; the triplication is specifically the three graduating owners.)

**VC7 — Differentiate the `numerical-primitive` GeometrySubject.** Four owners cross the graduation rail under one subject: analysis:121 (BIM compliance/clash/BCF), costing:157 (5D/4D lifecycle), structural:74 (section integrals), algebra:323 (compas numerical primitives). Semantically distinct evidence classes collapsed onto one coarse subject; the compute `HandoffAxis`/`GeometrySubject` union needs a BIM-evidence subject family (e.g. bim-compliance / bim-lifecycle / section-property) distinct from the compas numeric primitive. COMPUTE-ward consumer-pressure ripple (compute brief not in scope) — record as a named demand.

**VC8 — Redraw the README router + governance to the full realized page set.** README `[01]-[ROUTER]` lists `[01]`–`[13]` (stops at ALGEBRA); disk + ARCHITECTURE codemap carry 18 pages — features/authoring/brep/quality/spatial unrouted; energy pages (if VC1 mints them) additionally. Codemap/router disagree; both blind to the offload split and identity rename. GOVERNANCE drift (same class runtime `[V4]`/data `[V12]`). Ruling: router + package groups + seam ledger redraw to the exact realized (and energy-decided) module set in the same motion as VC1-VC4.
