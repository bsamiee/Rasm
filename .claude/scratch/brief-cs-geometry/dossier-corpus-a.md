# DOSSIER — corpus-a — Rasm/Geometry/.planning FIRST HALF (Drawing, Meshing, Numerics) + owning-package governing docs

Lane: corpus-a. Scope surveyed: subfolders sorted alpha = {Drawing, Meshing, Numerics, Parametric, Processing, Spatial}; FIRST HALF = **Drawing, Meshing, Numerics** (8 pages). Root of `Geometry/.planning/` has NO root-level pages. Owning package = `libs/csharp/Rasm/` (Geometry is a sub-domain alongside Vectors/Analysis/Domain, NOT its own package). Governing docs read in full: `Rasm/ARCHITECTURE.md`, `Rasm/README.md`, `Rasm/TASKLOG.md`, `Rasm/IDEAS.md`, plus `libs/.planning/architecture.md`, `RASM-GENERATION-SPEC.md` (kernel gate), `RASM-COMPONENT-PARADIGM-DECISION.md` (geometry touchpoints), `Directory.Packages.props`, `Rasm.csproj`, `Rasm/.api/` listing + sizes.

Second-half pages (Parametric/curve, Processing/{repair,receipts,solver,decimate,flatten,fit}, Spatial/{index,naming,reconciliation}) are corpus-b; this dossier flags the corpus-a→corpus-b dependencies it depends on but does NOT grade those pages.

---

## FOLDER-ARCHITECTURE VERDICT (top-level)

- **Placement is correct.** `Geometry/.planning/` is the architecture-sanctioned scaffold location (`libs/.planning/architecture.md:66` names it "the canonical instance" of a new sub-domain inside a mature package). Owning-package docs correctly sit at `Rasm/` root. No loose one-file-one-folder combos except **Parametric (1 page, curve.md, 156 LOC)** — the thinnest subfolder, a growth-shaped concern under-populated relative to its gated importance (see VC2).
- **Subfolder granularity is sound** as a mesh-CSG taxonomy (Numerics floor → Meshing lattice → Processing rail → Drawing producers → Spatial acceleration) but is **inverted relative to consumer demand**: the corpus is deep on exact-predicate mesh-CSG and nearly absent on the parametric curve/surface plane both gated consumers need most (VC2).
- **The corpus-a half (Meshing, Drawing) is a NET CONSUMER of the corpus-b half (Spatial, Processing).** Dependency edges crossing the split: intersect→Spatial/index+Processing/repair(MeshEdit); arrangement→Spatial/index(GWN+Overlap)+Processing/repair(BooleanOp+MeshEdit+WeldDuplicates); view→Spatial/index(Ray)+Meshing/intersect; offset→Meshing/{delaunay,arrangement}; pack→Spatial/reconciliation (via a wrong namespace, VC6). This asymmetry means corpus-a's correctness is gated on corpus-b owner shapes being right.

---

## PER-PAGE VERDICTS

### Numerics/predicates.md — VERDICT 9/10 (corpus gold standard)
Charter (as-is, correct): adaptive-precision exact-predicate floor; closed `Predicate` family (Orient2D/3D, InCircle/Sphere direct + OrientLPI/TPI, InCircleLPI/InSphereTPI indirect) over a four-tier `PrecisionTier` ladder (double filter → ddouble 106-bit → Expansion sign-exact → Fraction rational oracle); `Expansion`/`ErrorBound`/`RationalOracle`/`Lpi`/`Tpi`/`NumericsPolicy`.
- STRENGTHS: genuine first-principles Shewchuk expansions (TwoProduct via FMA, fast-expansion-sum), Attene/Cherchi indirect predicates with homogeneous λ² lifts (line 257-302), a FOUR-WAY differential oracle (Expansion / EFloat / Fraction / ERational — four unrelated bignums, no shared representation, line 360-366, 571) with `EContext.WithBlankFlags` inexact self-proof. This is the correctly-hand-rolled kernel: no admitted package provides adaptive exact predicates as a reusable primitive, so line 3's justification ("no admitted external geometry library carries a robustness guarantee") is SOUND — unlike the Delaunay/boolean/offset hand-rolls (VC1).
- Defects (minor): FMA-free RID Dekker-split fallback deferred (predicates.md:571, acknowledged, gates only a fallback row); `Lpi.Denominator`/`Tpi.Denominator` carry a `double` estimate alongside the exact `Expansion Lambda` (predicates.md:522,533) — a `double` in a public-ish struct, tolerable as it is used only for a fast path and the exact sign reads `Lambda`.
- No split/merge/move pressure. This page is the anchor every other page correctly composes.

### Numerics/faults.md — VERDICT 8/10
Charter (as-is, correct): one `GeometryFault [Union]`, band 2400, sub-banded by cluster on a 4-wide stride (2400 spatial … 2444 encoding), `Code`/`ToError` lowering into the LanguageExt `Error` rail; below the AEC `MaterialFault` 2450 boundary.
- STRENGTHS: correct band discipline confirmed against the federation (`RASM-COMPONENT-PARADIGM-DECISION.md:144` pins `FaultBand.Geometry = 2400` as a mirror of the kernel literal); one closed family, no per-cluster sibling unions; `ComparerAccessors.StringOrdinal` named once.
- Defects: **stringly-typed majority** — 10 of 17 cases carry only `string Detail` (DegenerateInput, IndexMismatch, UnrepairableMesh, NativeAssetMissing, DegenerateOffset, DegenerateArrangement, IntersectionFault, ParameterizationFault, ProjectionFault, EncodingFault-partial), contradicting the page's own boundary rhetoric "each case is typed to its failure with its real payload … a generic erasing carrier is the deleted form" (faults.md:20). The typed cases (OverConstrained, SingularSystem, FitFault, DecimationFault, SkeletonStalled — faults.md:46-57) prove the richer shape is achievable; the string cases are under-typed carriers (VC9).
- Band headroom is nearly exhausted: 12 clusters occupy 2400-2447, leaving only 2448-2449 (faults.md:19 acknowledges "an outright federation re-plan rather than a 13th cluster once the century fills"). A curve/surface plane expansion (VC2) would need new fault clusters with no room — a real constraint the brief must weigh.

### Meshing/delaunay.md — VERDICT 7/10
Charter (as-is): author-kernel constrained Delaunay/tetrahedralization over one `Tessellation [Union]` on a flat `SimplexStore`, Bowyer-Watson insertion driven by exact InCircle/InSphere, constraint recovery by predicate-guarded flips + Steiner via indirect predicates.
- STRENGTHS: clean polymorphic collapse (one union, two kinds differ only in arity+predicate dimension); Insert/Cavity/Locate/ExitFace bodies are real and predicate-exact; correct composition of the predicates floor.
- Defects:
  - **Core algorithm bodies are signature-only stubs** (delaunay.md:192-198): `Seed`, `Retriangulate` (the cavity-star cone + neighbour patch — the crux of Bowyer-Watson), `RecoverOne`, `StripSuper`, `LastLive`, `TouchesSuper`, `AddSimplexFaces` are declared `;` with no body. The page marks these "signature-fixed transcription targets" (delaunay.md:247) — legitimate for a design page, but the hardest 50% of the algorithm is deferred. Maturity is signature-complete, NOT body-complete.
  - **`Triangle/TetGen are GPL and rejected`** (delaunay.md:3) is factually wrong and roster-contradicting: Triangle.NET (the admitted `Triangle` package, `Rasm.csproj:30` aliased `TriangleNet`, 161-line .api) is MIT-licensed, not GPL (the original C Triangle by Shewchuk is non-commercial; the .NET port is MIT). Also `MIConvexHull` (admitted, Delaunay/Voronoi) is unmentioned. (VC1)
  - `SimplexStore` called "hash-friendly immutable record" (delaunay.md:5) but has mutable `int[]/bool[]` + `Stack<int> FreeList` + in-place `Spawn`/`Kill` (delaunay.md:66-74) (VC5).
- Charter as SHOULD be: unchanged concept; either land the stub bodies or explicitly cull the admitted Triangle/MIConvexHull (they cannot compose the exact floor — float-domain — so cull is the honest resolution).

### Meshing/intersect.md — VERDICT 8/10
Charter (as-is, correct): predicate-exact crossing lattice over one `IntersectOp [Union]` (6 cases), exact Orient3D straddle + Guigue-Devillers tri-tri, `Crossing{Site, Expansion OrderKey, Fraction RationalKey}`, BVH-broad-phased chain assembly.
- STRENGTHS: exact OrderKey + RationalKey tie-break (intersect.md:213-216) is the right robustness shape; **genuinely collapses the 3 inline crossing tests** scattered in repair/offset/arrangement onto one owner (intersect.md:388 [EXACT_CROSSING_COLLAPSE]) — a real duplication kill. Guigue-Devillers is correctly hand-rolled (no admitted package owns exact tri-tri).
- Defects:
  - `SegmentSegment`/`LineLine` is implicitly 2D (sets Z=0, intersect.md:293) but the union type does not encode the dimensional restriction — a caller can pass 3D lines and get a silent XY projection.
  - Depends on `Rasm.Geometry.Healing.MeshEdit.OfMesh` for triangle-soup extraction (intersect.md:327) — the soup adapter is mis-placed in Processing/repair (VC8).
  - Line 3 "No external geometry library is admitted" contradicts roster (geometry3Sharp owns mesh boolean/intersection; VC1) — though exact tri-tri itself is legitimately hand-rolled.

### Meshing/offset.md — VERDICT 5/10 (largest claim-vs-delivery gap)
Charter (as-is): straight-skeleton/weighted/medial/Minkowski/offset over one `OffsetOp [Union]` on a `WavefrontStore`, Aichholzer-Aurenhammer event queue driven by exact Orient2D.
- STRENGTHS: the wavefront `Propagate`/`Collapse`/`Divide` event machinery is real and exact-turn-guarded; `weighted` collapsed as a policy column (`OffsetPolicy.EdgeSpeed`) not a sibling class — correct.
- Defects (severe):
  - **MEDIAL-AXIS Voronoi-dual claim is UNWIRED/dead.** `MedialFrom` builds `Tessellation.Build(...)` then DISCARDS it: `.Map(_ => new MedialAxis(trace.Graph.Nodes, toSeq(kept), toSeq(bisectors)))` (offset.md:325-326). The medial axis is just the trimmed straight skeleton. The prose (offset.md:16, 478 [MEDIAL_AXIS]) claims it "reconciles against the constrained-Delaunay Voronoi dual so the medial axis is the EXACT bisector locus" — but **delaunay.md exposes NO Voronoi-dual surface** (its `Tessellation` union exposes only Build/Insert/Recover/ToMesh). Phantom seam: offset composes a capability delaunay does not provide, and the code proves it by discarding the build. (VC4)
  - Rejects `Clipper offsetting … float-epsilon and rejected` (offset.md:3) and CGAL — but `Clipper2` + `CavalierContours` (arc-aware offset) are both admitted (Rasm.csproj:25-26, .api-cataloged) (VC1). Clipper2's boolean core is integer-EXACT, not float-epsilon — the rejection reasoning is imprecise.
  - Owner sentence lists only `skeleton/medial/minkowski/offset` (offset.md:13) — omits `weighted` (present in enum + Cases). Stale prose.
  - `WavefrontStore` sizing: `Prev/Next/Dead/SpawnTime/Origin` allocated `2*n` (offset.md:195) but `Spawn` grows `Count` past `2n` on repeated splits → index-overflow risk; "immutable record" mislabel (VC5).
- Charter as SHOULD be: either wire a real Voronoi-dual accessor on delaunay (or SharpVoronoiLib, admitted, 153-line .api, which owns Fortune Voronoi + Delaunay dual — the exact package for this) OR delete the medial/Voronoi claim and ship straight-skeleton-only medial. The current page claims capability it does not deliver.

### Meshing/arrangement.md — VERDICT 7/10 (most integrated; subtle robustness gap)
Charter (as-is): fully-managed exact mesh/polygon arrangement (Cherchi-Attene) over one `Arrangement [Union]` (MeshBoolean/PlanarOverlay/CellComplex), surface subdivision + GWN classification + BooleanOp keep + weld; retires the tier-3 native CSG gate.
- STRENGTHS: the deepest composition in the corpus — predicates + delaunay substrate + Spatial GWN + Spatial Overlap + Healing weld, all through settled rails; `Arrangers` FrozenDictionary data-table dispatch (arrangement.md:147); exact `RationalProjection` crossing-endpoint ordering (arrangement.md:237); correctly frames the boolean as SURFACE subdivision not convex-hull tetrahedralization.
- Defects:
  - **Constructed-crossing exactness is overstated.** `PlaneCrossPoint` computes the crossing as a ROUNDED `double` (arrangement.md:258-262), interns it, and feeds it as an explicit `Constraint.Segment(int u, int v)` over a `Point3d[]` into Tessellation (arrangement.md:286-290). The LPI/TPI implicit-point exactness (claimed arrangement.md:270,467) only covers Steiner points the tessellation creates INTERNALLY — NOT the arrangement's own operand crossings, which are already rounded before entering the substrate. The interface `Constraint.Segment` cannot carry an implicit point, so the headline exact-boolean robustness has a rounding gap precisely at the operand-crossing interface. (VC7)
  - `ArrangementStore` "immutable SoA record" (arrangement.md:488) but `internal void Write` mutates in place (arrangement.md:87-91) (VC5).
  - Composes `BooleanOp` (Union/Difference/Intersection) from `Processing/repair#HEALING` (arrangement.md:14,18) — the boolean discriminant is mis-placed in the heal page; it belongs with the boolean OWNER (this page) (VC8).
  - Retires the admitted `Clipper2` (arrangement.md:470) (VC1).
- Charter as SHOULD be: unchanged concept (this is the right owner for the CSG plane); hoist `BooleanOp` here; either thread implicit crossings through a richer `Constraint` case or drop the exact claim to "exact-sign-classified, rounded-vertex" honesty.

### Drawing/view.md — VERDICT 4/10 (strongest prose, weakest delivery — illusory depth)
Charter (as-is): predicate-exact HLR/silhouette/section/outline over one `ViewOp [Union]`, Newell-Newell-Sancha BSP painter, Appel quantitative-invisibility, exact Orient3D silhouette locus; Section delegates to one IntersectOp.PlaneMesh.
- STRENGTHS: exact silhouette locus (`FacesOppose` via Orient3D, view.md:235-241) is genuinely real and correct; Section correctly collapsed to one `IntersectOp.PlaneMesh.Apply` (view.md:349-353, no fourth crossing test); crease lift from Vectors FeatureEdge.
- Defects (severe — the two headline algorithms are non-functional):
  - **The Newell-Newell-Sancha BSP is ENTIRELY DEAD.** `Paint` takes `BspNode bsp` as a parameter (view.md:302) but the body **never references `bsp`** — it iterates `edges` and calls `Clip(... index ...)` (BVH), never the partition. `Partition`/`Split`/`BspNode`/`Spawn` (view.md:263-300, ~40 LOC) are built and discarded. The page's central thesis ("back-to-front BSP face ordering, authored from first principles over a flat BspNode partition store", view.md:3) is decorative. (VC3)
  - **Visibility is naive uniform ray-sampling, NOT Appel QI.** `Clip`/`OccludedAt` (view.md:320-346) sample the segment every `SampleStep=0.5` screen units and cast a BVH ray per sample. This re-introduces exactly the sampling-tolerance non-robustness the page claims to eliminate (an occluder edge between two samples is missed). "Appel quantitative-invisibility count" (view.md:16,435) is claimed but not implemented. (VC3)
  - `DrawingProjection.ToPolylines` groups by `Edge.Key` and concatenates ALL segments of a kind into one polyline (view.md:130-135) regardless of connectivity — produces garbage polylines.
- Charter as SHOULD be: keep the exact silhouette locus; the visibility engine needs a GROUND-UP rebuild (either a real BSP painter that Paint consumes, or an exact analytic Appel QI that tracks invisibility count only at silhouette crossings). Current page cannot be polished — the advertised engine does not exist.

### Drawing/pack.md — VERDICT 7/10
Charter (as-is): canonical geometry-encoding over one `PackOp [Union]` (PointCloud/MeshPatch/VoxelGrid/BrepPatch), 8-row `EncodingChannel` lattice, contiguous SoA `ReadOnlyMemory<float>` payload, content-hash-keyed round-trip witness; producer for Compute EncodedTensor + AppHost GeometryPacking.
- STRENGTHS: clean channel-lattice-as-data (Readers FrozenDictionary, pack.md:159); correctly composes Vectors ScalarField (Geodesic/MeanCurvatureFlow/SignedDistance) + VectorCloudMetric.OrientedNormals rather than re-deriving; the residency-seam (Compute reads the arena as ReadOnlyTensorSpan without a copy) is a genuinely good cross-package contract.
- Defects:
  - **NAMESPACE-LAW VIOLATION.** `using Rasm.Geometry.Topology;` (pack.md:33) + `Rasm.Geometry.Topology.CanonicalTopology`/`NamingHashOps` (pack.md:224-226, 33). ARCHITECTURE.md:64 EXPLICITLY forbids a `Rasm.Geometry.Topology` namespace ("the robust-core mints no `Topology` namespace … the naming↔hash reconciliation live under `Rasm.Geometry.Spatial`") to prevent collision with `Rasm.Domain.Topology`. Every other page routes `Spatial/reconciliation`. pack.md reintroduces the exact collision the law forbids. (VC6)
  - **Float16/Unorm8 residency-savings claim is illusory.** `EncodedStore.Payload` is always `float[]` (pack.md:126-129); `ChannelDtype.Pack` returns a `float` (quantized-and-widened-back, pack.md:50-56) and `Unpack` is identity. So Dtype only affects round-trip ERROR, never stored SIZE. The claim "a `Half` halves residency" (pack.md:13, 463) is false — a Float16 channel occupies the same 4 bytes/elt as Float32.
  - Depends on `ScalarField.SampleDetailed` described as "the public analogue of the landed `SampleSdfDetailed`, the settled-contract seam the Vectors source-pass exposes" (pack.md:459) — i.e. a Vectors member that may NOT yet exist. Unverified cross-package member (phantom risk; assay/api over the restored Vectors assembly would confirm).
  - Attributes `VectorCloudMetric.OrientedNormals` to `Processing/fit#FITTING` (pack.md:3) but it is a Vectors capability (run through VectorIntent.Cloud). Minor mis-attribution.

---

## CROSS-CUTTING FINDINGS

### Dead carriers / roster-vs-doctrine (the dominant finding)
Nine geometry packages are admitted (`Rasm.csproj:19-30`, centralized `Directory.Packages.props:70-78`), each with a substantive `.api` catalog (Triangle 161, GShark 185, SharpVoronoiLib 153, MIConvexHull 136, geometry3Sharp 114, CavalierContours 109, Clipper2 106, kdtree 110, libtess 99 — ~1,177 lines of API evidence). **ZERO first-half pages compose ANY of them.** The three grep hits (arrangement/offset/view) are rejection-mentions ("retires the float Clipper2") or the common noun "triangle." README claims LibTessDotNet "backs the Drawing/view + Drawing/pack fill leg" (README:44) — but neither view.md nor pack.md reference it (view = HLR/silhouette, pack = channel encoding; neither does polygon fill). The roster is carried, cataloged, restored, and unused.

### Prose-vs-fence splits (systemic; class of illusory depth)
Four pages claim cross-page compositions or algorithms the fences do not deliver: view.md BSP dead + sampled-not-QI (VC3), offset.md Voronoi-dual discarded (VC4), arrangement.md rounded-crossing-not-implicit (VC7), pack.md residency-not-halved. Plus ARCHITECTURE.md:3 "admits no external geometry library" vs README's 9-package roster.

### Duplication
- Triangle-soup extraction (`MeshEdit.OfMesh` → `(Point3d[], (int,int,int)[])`) re-derived in intersect.md:326, view.md:386, arrangement.md:352 — three near-identical `Soup(MeshSpace)` helpers. Belongs to one owner.
- intersect.md correctly documents (and collapses) the 3 inline crossing tests (repair/offset/arrangement) — a duplication kill already scoped; verify the offset/arrangement source-pass actually re-routes to `Intersection.Apply` (offset.md still spells its own `SegmentsCross` at line 440-444; arrangement its own `EdgeCrossings` at 242-247 — the collapse is claimed but NOT yet applied in these pages).

### Concern mis-placement (move-pressure)
- `BooleanOp` (Union/Difference/Intersection) lives in Processing/repair#HEALING, consumed as core vocab by arrangement (the actual boolean owner). Hoist to arrangement. (VC8)
- Mesh↔triangle-soup adapter lives in Processing/repair (Healing `MeshEdit`), consumed by Meshing+Drawing. Hoist to a Meshing- or Vectors-level owner. (VC8)

### Hardcoding-vs-generator
Mostly clean — kinds/channels/dtypes/tiers are all SmartEnum row-data with policy columns (correct generator shape). Rosters-as-data is honored. Residual literals: `SampleStep=0.5`, `OcclusionBias=1e-7` (view ViewPolicy), `BetaSquared=4.0`/`InteriorOffset=1e-7` (arrangement) — all parameterized on policy records (acceptable). No enumerated-instance-where-generator-belongs defects found in corpus-a.

### Dead typed carriers
`BspNode` + entire Partition/Split apparatus (view.md) — built, never read (VC3). `Tessellation` build result in offset MedialFrom — built, discarded (VC4). `Lpi.Denominator`/`Tpi.Denominator` double fields — carried, only the exact `Lambda` is load-bearing.

### Unwired seams
- offset → delaunay Voronoi-dual: delaunay exposes no such surface (VC4).
- pack → `Rasm.Geometry.Topology` reconciliation namespace that violates the law (VC6).
- pack → `ScalarField.SampleDetailed` unverified Vectors member.
- The corpus-a→corpus-b edges (Spatial GWN `SpatialQuery.Winding`+`QueryResult.Scalar`, Spatial `SpatialQuery.Ray`+`QueryResult.RayHit`, Spatial `Overlap`+`QueryResult.Pairs`, Processing `Kernels.WeldDuplicates`+`MeshEdit`+`BooleanOp`) are all assumed-shaped by corpus-a pages; corpus-b must confirm these owner surfaces exist exactly as consumed.

### Unmined capability (catalog anchors)
The admitted `.api` catalogs describe capability the corpus needs but hand-rolls or lacks:
- `api-gshark.md` (185): NurbsCurve/Surface Evaluate + derivatives, Intersection (curve-curve, curve-surface), Fitting (interp/approx/least-squares), Sampling (adaptive), Analyze (curvature/length/closest-point), Modify/Optimization Newton — **the exact curve/surface plane the KERNEL GATE requires (VC2), unused.**
- `api-sharpvoronoilib.md` (153): Fortune Voronoi + Delaunay dual + Lloyd — **the exact medial-axis Voronoi dual offset.md claims but discards (VC4), unused.**
- `api-clipper2.md` (106) / `api-cavaliercontours.md` (109): 2D boolean + parallel/arc offset — the Fabrication toolpath-offset-with-corner-strategy concern, hand-rolled/absent.
- `api-geometry3sharp.md` (114): DMesh3 boolean/remesh/repair — overlaps arrangement + Processing/repair.
- `api-kdtree.md` (110): flat kd-tree k-NN — the point-NN leaf (Processing/fit consumer, corpus-b).

---

## VERDICT CANDIDATES (campaign-defining, strongest first)

**VC1 — ROBUST-CORE vs ECOSYSTEM-PACKAGE BOUNDARY IS UNRESOLVED AND CONTRADICTORY.** 9 admitted geometry packages (all .csproj-referenced, .api-cataloged 99-185 lines) are consumed by zero first-half pages; the corpus hand-rolls every concern and explicitly rejects Triangle (delaunay.md:3, factually wrong — Triangle.NET is MIT) / Clipper2 (offset.md:3, arrangement.md:470) / CGAL. ARCHITECTURE.md:3 ("admits no external geometry library") contradicts README [COMPUTATIONAL_GEOMETRY]. Intra-roster redundancy: Triangle + MIConvexHull + SharpVoronoiLib all do Delaunay/Voronoi; Clipper2 + CavalierContours both do 2D offset — violating "one categorical-best owner per concern." The brief MUST rule: cull the float-domain packages that cannot compose the exact floor (honest, given the robust-core doctrine) OR wire them as the explicit non-exact/scale-fallback lane — and reconcile ARCHITECTURE vs README.

**VC2 — KERNEL-GATE COVERAGE INVERSION (the strongest folder-architecture ruling).** Generation stand-up is GATED (RASM-GENERATION-SPEC.md:179, 226) on curve/surface operations: arc-length stations/frames on arbitrary curves, surface isolines/geodesics/offsets, region tessellation/subdivision, developable/panelization, pattern-to-surface mapping — and the spec is explicit that "a Vectors/Geometry rebuild campaign PRECEDES Generation." The corpus is deep on exact-predicate mesh-CSG (predicates/delaunay/arrangement/intersect/offset-2D/HLR) and NEARLY ABSENT on the gated curve/surface plane: no stations/frames owner, no surface isolines/geodesics owner (Geodesic exists only as a consumed Vectors ScalarField), offset is 2D-polygon not surface offset, no panelization, no pattern-mapping; Parametric/curve.md is the thinnest subfolder (156 LOC). GShark (the admitted pure-managed NURBS engine, 185-line .api) — the package most directly serving the gate — is unused. Fabrication's toolpath-grade needs (offset curves with corner strategies, slicing/section STACKS, developable/unroll, adaptive clearing) are equally thin (offset is straight-skeleton; intersect gives single-plane section, not stacks). The corpus's investment is misaligned with BOTH gated consumers. The brief must rebalance toward the curve/surface/panelization plane.

**VC3 — view.md's ADVERTISED VISIBILITY ENGINE DOES NOT EXIST.** The Newell-Newell-Sancha BSP (page thesis, view.md:3) is built (Partition/Split/BspNode, view.md:263-300) and never consumed — `Paint` takes `bsp` (view.md:302) but the body never references it; visibility is naive uniform ray-sampling (Clip/OccludedAt, view.md:320-346), not the claimed Appel quantitative-invisibility. Highest-order illusory depth in the corpus. Verdict: view.md's HLR arm requires ground-up rebuild, not polish (exact silhouette locus is the only salvageable part).

**VC4 — offset.md's MEDIAL-AXIS EXACTNESS IS UNWIRED.** MedialFrom builds Tessellation then discards it (`.Map(_ => ...)`, offset.md:325-326); the "constrained-Delaunay Voronoi dual → exact bisector locus" claim (offset.md:16,478) is unbacked and delaunay.md exposes no Voronoi-dual surface. Medial axis is just the trimmed straight skeleton. SharpVoronoiLib (admitted, 153-line .api) owns exactly this and is unused (ties to VC1). Fix: wire a real Voronoi dual (delaunay accessor or the admitted package) or delete the claim.

**VC5 — SoA STORES ARE MISLABELED "IMMUTABLE" WHILE reconciliation CONTENT-ADDRESSES THEM.** SimplexStore (delaunay), WavefrontStore (offset), IntersectStore (intersect), ArrangementStore (arrangement), BspNode (view) are all `sealed record` with mutable arrays + internal in-place Write/Spawn/Kill, yet each is prose-labeled "hash-friendly immutable record" that Spatial/reconciliation#NAMING_HASH content-addresses. Systemic across the corpus-a plane; content-address timing/identity is undefined for a store mutated in place. The brief must settle the store-mutability + content-hash contract once, for all Meshing/Drawing stores.

**VC6 — pack.md VIOLATES THE NAMESPACE_LAW.** `using Rasm.Geometry.Topology;` + `Rasm.Geometry.Topology.CanonicalTopology`/`NamingHashOps` (pack.md:33,224-226) reintroduce the exact `Topology` namespace ARCHITECTURE.md:64 forbids (collision with `Rasm.Domain.Topology`); every other page routes `Rasm.Geometry.Spatial`. Either pack.md is wrong or reconciliation.md declares the wrong namespace — the brief must fix the reconciliation-owner namespace consistently (corpus-b confirms reconciliation.md's actual declaration).

**VC7 — arrangement.md's EXACT-BOOLEAN ROBUSTNESS HAS A ROUNDING GAP AT THE OPERAND-CROSSING INTERFACE.** Constructed crossings are rounded doubles (PlaneCrossPoint, arrangement.md:258-262) fed as explicit `Constraint.Segment(int,int)` over a `Point3d[]` (arrangement.md:286-290); the LPI/TPI implicit-point exactness (claimed arrangement.md:270) only covers tessellation-internal Steiner points, not the arrangement's pre-rounded operand crossings — the `Constraint.Segment` interface cannot carry an implicit point. The headline exact-arithmetic boolean is exact-SIGN-classified but rounded-VERTEX. Fix: richer `Constraint`/`Crossing` case carrying the implicit point, or honest re-scoping of the exactness claim.

**VC8 — CONCERN MIS-PLACEMENT: BooleanOp + mesh-soup adapter live in Processing/repair (Healing).** `BooleanOp` (Union/Difference/Intersection) is consumed as core vocab by arrangement (the boolean owner) but declared in Processing/repair#HEALING; the mesh↔triangle-soup adapter (`MeshEdit.OfMesh`/`Soup`) is re-derived in intersect/view/arrangement from the Healing owner. Move-pressure: hoist BooleanOp to arrangement; hoist soup-extraction to a shared Meshing/Vectors owner. (Requires corpus-b Processing/repair confirmation.)

**VC9 (secondary) — faults.md UNDER-TYPES THE MAJORITY OF ITS UNION.** 10 of 17 GeometryFault cases carry only `string Detail`, contradicting the page's own "typed to its failure with its real payload" boundary (faults.md:20). The 5 richly-typed cases prove the shape; the string cases should carry structured payloads. Also: band headroom is 2 codes (2448-2449) — a curve/surface plane expansion (VC2) has no fault-cluster room without a federation re-plan.

---

## KEY NUMBERS
- First-half design pages deep-read: 8 (predicates 575, faults 102, delaunay 254, intersect 390, offset 481, arrangement 492, view 438, pack 466 markdown lines).
- Governing/context docs read: Rasm/{ARCHITECTURE, README, TASKLOG, IDEAS}, libs/.planning/architecture.md, RASM-GENERATION-SPEC.md (gate), RASM-COMPONENT-PARADIGM-DECISION.md (geo touchpoints), Directory.Packages.props, Rasm.csproj, Rasm/.api/ (18 catalogs).
- Admitted-but-unused-in-corpus-a geometry packages: 9 (GShark, Supercluster.KDTree, LibTessDotNet, MIConvexHull, Clipper2, CavalierContours, SharpVoronoiLib, Triangle, geometry3Sharp).
- Verdict spread: predicates 9, faults 8, intersect 8, delaunay 7, arrangement 7, pack 7, offset 5, view 4.
