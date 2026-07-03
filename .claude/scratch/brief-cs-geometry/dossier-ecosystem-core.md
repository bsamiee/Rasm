# DOSSIER — Rasm/Geometry — lane: ecosystem-core

Target: `libs/csharp/Rasm/Geometry/.planning/` (18 pages, 6277 LOC md). Stance: HOSTILE. Lane focus: REAL OSS ecosystem deep-dive for the kernel's CORE domain concerns (predicates, NURBS, spatial acceleration, tessellation, arrangement/booleans, intersection, offsetting, decimation, parameterization, fitting, constraint-solving, projection, encoding) graded against BOTH gated consumers (Rasm.Generation KERNEL GATE + Rasm.Fabrication toolpath-grade). Package roster is OPEN; license gate enforced (OSS + free-for-OSS-commercial admissible; pay-tiered/seat/proprietary REJECTED; GPL/AGPL treated as reject-for-robust-core per the corpus's own settled stance).

Verdict up front: this is a genuinely world-class corpus — dense, ADT-collapsed, one-owner-per-concern, exact-arithmetic robust core. It is NOT naive on the APPROACH axis (every owner is a `[Union]`+`Apply` polymorphic surface driven by data tables, zero enumerated-instance sprawl). The ecosystem-core defects are almost entirely at the MANIFEST/ROSTER boundary, not the design: seven admitted kernel packages are composed by ZERO pages (dead weight), the single most important reserved seam (tier-3 mesh-boolean native asset) names no engine, one Fabrication-gate concern (CAM arc-aware curve offset with corner strategies) is uncovered, and the numerics floor rides an unreleased beta. The design pages' "reject external, author from first principles" rulings are, on verification, CORRECT for the robust core (with two imprecise rationales to fix).

---

## [A] CORE DOMAIN CONCERNS (derived from scope mandate + corpus) → categorical-best OSS owner

Each row = one geometry concern the kernel owns, the current disposition, and the ecosystem verdict after web+manifest verification. License gate applied.

| # | Concern | Current owner / disposition | Categorical-best OSS (verified) | License | Maturity signal | Binding surface | Verdict |
|---|---|---|---|---|---|---|---|
| 1 | Robust exact predicates (orient/incircle/insphere + indirect) | HAND-ROLLED `Predicate`/`Expansion` (predicates.md) over TYoshimura.DoubleDouble + ExtendedNumerics.BigRational + PeterO.Numbers | `govert/RobustGeometry.NET` (Shewchuk C# port) is the ONLY .NET precursor | MIT | 90★, ~2012, NOT on NuGet, DIRECT predicates only (no Attene indirect), FPU-caveat | source-port | **HAND-ROLL WINS.** No maintained/NuGet/indirect-complete lib exists. Corpus kernel is strictly superior (4-tier ladder + LPI/TPI + 4-way oracle). Fix the overstated claim (§C). |
| 2 | Host-neutral NURBS curve/surface | GShark 2.3.1 (curve.md) | GShark (`GSharker/G-Shark`); SISL; Verb | GShark MIT; **SISL AGPL-3.0 (REJECTED)**; Verb MIT (no C#/NuGet) | GShark 233★, netstandard2.0, moderate maintenance; SISL active but AGPL | managed | **GShark STAYS.** Categorical-best managed host-neutral option; SISL rejected by gate; Verb not .NET. Flag netstandard2.0/velocity risk (§B curve). |
| 3 | Broad-phase acceleration (BVH/octree/kNN/GWN) | HAND-ROLLED `SpatialIndex` (index.md) + Supercluster.KDTree.Net leaf | Supercluster.KDTree.Net for the point leaf only | permissive | 1.0.22, lightly maintained fork; build-once static | managed | **HAND-ROLL WINS** for BVH/octree/agglomerative/GWN (no OSS peer). kd-tree leaf: Supercluster adequate but lightly maintained; acceptable. |
| 4 | Constrained Delaunay tri/tet | HAND-ROLLED Bowyer-Watson `Tessellation` (delaunay.md) | Triangle.NET; TetGen | **Triangle.NET is MIT** (Woltering port), TetGen AGPL | Triangle.NET maintained; but pinned pkg `Triangle` 0.0.6-Beta3 orphaned | managed | **HAND-ROLL WINS** (exact InCircle + LPI/TPI Steiner; no OSS peer does exact CDT+CDT3D). But the page's rejection rationale ("Triangle GPL") is FACTUALLY WRONG — Triangle.NET is MIT (§C). |
| 5 | **Mesh boolean / CSG (3D, manufacturing tolerance)** | HAND-ROLLED exact `Arrangement` (arrangement.md) for common cases; **tier-3 native asset RESERVED BUT UNNAMED** | **Manifold (`elalish/manifold`)** — the categorical-best; MCUT | **Manifold Apache-2.0**; **MCUT LGPL-3.0/commercial (gray, weaker)** | Manifold 2104★, v3.5.2 (push 2026-06-27, 90 contributors) — reference-grade guaranteed-manifold; C# binding `ManifoldNET` alpha/stale (9★, v1.0.7-alpha 2024-08, Apache-2.0) | native (C FFI, P/Invoke); RID assets | **ADMIT MANIFOLD as the named tier-3 engine.** Strongest ecosystem finding. Binding path is the open decision: alpha `ManifoldNET` vs thin in-house P/Invoke over `manifoldc`. §C VC-1. |
| 6 | Discrete exact intersection (seg/tri/mesh/plane) | HAND-ROLLED `IntersectOp` (intersect.md) | none (Guigue-Devillers exact is bespoke) | — | — | managed | **HAND-ROLL WINS.** Correctly collapses 3 scattered inline crossing tests onto one owner. |
| 7 | Straight skeleton / medial axis / Minkowski offset | HAND-ROLLED wavefront `OffsetOp` (offset.md) | CGAL SK (GPL, REJECTED); StraightSkeletonNet / Sutro / Briganti (float) | CGAL GPL; the .NET ports MIT but **float-epsilon** | permissive ports exist but non-robust | managed | **HAND-ROLL WINS for robust core.** offset.md's "CGAL GPL rejected" is CORRECT. All permissive .NET SK are the float-epsilon form the page rejects. |
| 8 | **CAM arc-aware curve offset (corner strategies: round/miter/bevel/square joins, end types)** | **UNCOVERED** — offset.md gives mitered polygon offset only (skeleton wavefront); no arc/round-join tool-radius offset | **CavalierContours.NET** (arc-bulge offset+boolean, generic-math); Clipper2 (JoinType Miter/Square/Bevel/Round + EndType) | **CavalierContours ISC; Clipper2 Boost** | CavalierContours .NET 10, `IFloatingPointIeee754<T>`, `StaticAABB2DIndex`; Clipper2 mature | managed | **COVERAGE GAP vs Fabrication gate.** CavalierContours is admitted-but-ORPHANED yet is the RIGHT engine (true arcs, not polygonized). §C VC-3. |
| 9 | Mesh decimation / LOD (QEM) | HAND-ROLLED Garland-Heckbert `SimplifyOp` (decimate.md) at 106-bit + exact Orient3D gate | meshoptimizer (`Alimer.Bindings.MeshOptimizer`, ADMITTED in Compute label) | MIT | meshoptimizer battle-tested; Alimer binding present | native | **HAND-ROLL WINS** (exact gate + vsplit + Hausdorff meshoptimizer lacks). But meshoptimizer is admitted centrally and unused by Geometry — a real fast-path/meshlet seam to Compute (§B decimate). |
| 10 | Mesh parameterization / UV flatten (LSCM/ARAP/BFF) | HAND-ROLLED `ParamOp` (flatten.md) over Vectors DEC + CSparse/MathNet | libigl (C++, MPL2) | MPL2 (no C# binding) | libigl reference-grade C++ only | native | **HAND-ROLL WINS** (composes Vectors cotangent-Laplacian; no managed peer). Exceptional page. |
| 11 | Primitive fitting / registration (RANSAC/ICP) | HAND-ROLLED `FitOp` (fit.md) MLESAC/PROSAC + LM | none managed (open3d is Python) | — | — | managed | **HAND-ROLL WINS.** But no convex-hull owner (MIConvexHull orphaned) — hull is a missing primitive (§B fit). |
| 12 | Geometric constraint solver (2D/3D sketch) | HAND-ROLLED LM `ConstraintSolver` (solver.md) | SolveSpace (GPL, REJECTED); NeoGeoSolver | GPL / rejected | — | — | **HAND-ROLL WINS.** solver.md's GPL rejection correct. |
| 13 | Distance fields / SDF / iso-surface | **OWNED BY `Rasm.Vectors.ScalarField`** (composed by decimate/pack/flatten) | SdfKit (C# SDF→mesh); MillSimSharp (SDF+voxel CNC) | MIT | SdfKit/MillSimSharp exist for CAM SIM (Fabrication concern) | managed | **CORRECT: distance fields are a Vectors owner, Geometry composes.** Fabrication "distance fields" gate met at the seam; CAM sim (MillSimSharp-class) is a Fabrication owner, not kernel. |
| 14 | Numerics linear-algebra floor | MathNet.Numerics **6.0.0-beta2** (+MKL/OpenBLAS providers), CSparse 4.4.0 | MathNet 5.0.0 stable | MIT | **6.0 has NO stable release** (issue #1147 open 2026-02, only prereleases); 5.0.0 stable since 2022 | managed | **PIN RISK.** Whole kernel LA floor rides an unreleased beta, contradicting DEPENDENCY_POLICY "newest stable". §C VC-4. |
| 15 | Isotropic / quad-dominant remeshing | voxel-remesh only (SDF iso, decimate.md); no field-aligned quad/isotropic | QuadriFlow, Instant Meshes (C++) | mixed/research | no clean C# binding | native | **MINOR COVERAGE GAP** vs Generation panelization. No easy package answer; defer or native. §B decimate. |

Rejected candidates by license gate (recorded so they are not re-proposed): SISL (AGPL-3.0), CGAL straight-skeleton (GPL), SolveSpace (GPL), TetGen (AGPL), Triangle original C (non-commercial). MCUT (LGPL/commercial) is admissible-but-weaker than Apache-2.0 Manifold for the same concern → not selected.

---

## [B] PER-PAGE VERDICTS

Format: verdict/10 · defects (file:line) · split/merge/move pressure · owner charter as it SHOULD be. Grades are ecosystem-core-weighted (package fit, coverage vs both consumers, hand-roll-vs-compose adjudication).

### Numerics/predicates.md — 9/10
The corpus's single strongest asset: adaptive four-tier ladder (`double`→`ddouble`→`Expansion`→`Fraction`), Attene/Cherchi indirect `Lpi`/`Tpi` predicates, four-way differential oracle (`Expansion`/`EFloat`/`Fraction`/`ERational`, no two sharing a bignum). Composes DoubleDouble+BigRational+PeterO to their intended power — textbook "internalize external capability behind a focused owner."
- Defect (predicates.md:3): "authored from first principles because no admitted external geometry library carries a robustness guarantee" is OVERSTATED. `govert/RobustGeometry.NET` (MIT) is a real Shewchuk C# port. Precise claim: no MAINTAINED, NuGet-published, indirect-predicate-complete .NET predicate library exists — the hand-roll remains correct, but the rationale should cite the precursor and its gaps (direct-only, no NuGet, unmaintained since ~2012) rather than assert absence.
- Defect (predicates.md:411-421, 505): `Expansion.TwoSum` uses raw `double` a+b/x-a EFTs; the well-known .NET hazard is x87 80-bit extended-precision defeating Shewchuk arithmetic (RobustGeometry.NET's own wiki documents this). On the target RIDs (osx-arm64 NEON, RyuJIT SSE2/AVX) double arithmetic is strict IEEE-754 64-bit so it is safe, and `TwoProduct` rides FMA — but the page never states the RID assumption that makes `TwoSum` safe. Add a one-line RID/strictfp invariant to `NumericsPolicy`.
- Split/merge: none. Owner is optimally dense.
- Charter as-is: keep verbatim; add the precursor citation + the strict-IEEE-754-double RID invariant as the two robustness caveats.

### Numerics/faults.md — 8/10
Clean single `GeometryFault` `[Union]` band 2400, sub-banded 4-wide per cluster, `ToError()` lowering. Correct one-owner collapse of per-cluster fault families.
- Defect (faults.md:19): band nearly full — 12 clusters consume 2400-2447, only 2448-2449 headroom; a 13th cluster forces "an outright federation re-plan." Real but distant; note as a scaling constraint, not a defect to fix now.
- No ecosystem dependency beyond Thinktecture/LanguageExt (correct).
- Charter as-is: keep; add the band-exhaustion escalation as an explicit IDEAS/blocker rather than inline prose.

### Parametric/curve.md — 8/10
Clean GShark `NurbsBase`/`NurbsSurface` wrap at the instance surface, `Parametric`/`ParametricOp`/`EvalResult` ADT, boundary marshal to `Rasm.Vectors`, correct runtime-not-capability split vs RhinoCommon.
- Ecosystem note (curve.md:18): GShark 2.3.1 is netstandard2.0 and moderately maintained (233★, open issues 39). It is the categorical-best MANAGED host-neutral NURBS for .NET — SISL is AGPL (rejected), Verb has no C#/NuGet, OpenNURBS/Rhino3dm is the host-coupled path already present transitively. No replacement; flag GShark velocity/netstandard2.0 as a watch item, and record SISL's AGPL rejection so it is never re-proposed.
- Coverage vs Fabrication: `NurbsBase.Offset(distance, plane)` is named in Growth (curve.md:19) but curve offset for TOOLPATHS needs corner strategies GShark's planar offset does not own — that concern belongs with §8 (CavalierContours), not here. Keep this page parametric-only.
- Folder note: Parametric/ is a one-file folder (see §D). Justified — NURBS is a distinct growth axis (surface trimming, sweeps, degree ops).

### Spatial/index.md — 9/10
Exceptional. BVH(SAH)/octree(Morton)/agglomerative(PLOC) + kd-tree(Supercluster) leaf + Barill/Jacobson fast generalized-winding, all over ONE SoA `NodeStore`, one `Query` fold, frozen `CLASH_GOLDEN` byte fixture. This is the load-bearing hub every Meshing/Processing page composes.
- Ecosystem note (index.md:18): Supercluster.KDTree.Net 1.0.22 is a lightly-maintained fork (Eric Regina lineage), build-once static — which exactly matches the page's "build-once immutable, a point-set change rebuilds" usage (index.md:117). Adequate; a `Supercluster.KDTree.Net8` net-native variant exists if a TFM bump is wanted. No categorical replacement.
- No defects material to ecosystem lane. Charter as-is: keep.

### Spatial/naming.md — 8/10 · Spatial/reconciliation.md — 8/10
Persistent topological naming (`TopoName` lineage, `Track` re-anchor by position-free signature) and the naming↔hash reconciliation to the Persistence `GeometryHash`. Pure managed, composes only the kernel `Domain.ContentHash` (one hasher) — zero external geometry deps, correct.
- No ecosystem findings. These are boundary-contract pages; their fit is to Persistence/Element, verified consistent with architecture.md §04 GEOMETRY_FLOW (Geometry is source-of-truth, others consume). Charter as-is.

### Meshing/delaunay.md — 8/10
Exceptional exact CDT/CDT3D (Bowyer-Watson, exact InCircle/InSphere cavity, LPI/TPI Steiner recovery).
- Defect (delaunay.md:3): "No external geometry library is admitted (Triangle/TetGen are GPL and rejected)" — FACTUALLY WRONG for Triangle. The admitted NuGet `Triangle` 0.0.6-Beta3 (csproj alias `TriangleNet`) is Christian Woltering's **Triangle.NET, which is MIT-licensed**, NOT GPL. TetGen is AGPL (correct). The hand-roll is still right (no OSS lib does exact CDT+CDT3D with indirect-predicate Steiner recovery), but the stated reason is false and must be corrected to the real one: "no OSS triangulator provides exact-arithmetic constrained Delaunay with implicit-point Steiner recovery; Triangle.NET (MIT) is float-epsilon, TetGen is AGPL."
- Move pressure: this false rejection is coupled to the orphaned `Triangle` package (§D) — fixing one fixes both.
- Charter as-is: keep the kernel; correct the rejection rationale; drop the `Triangle` package + its `.api` (dead).

### Meshing/arrangement.md — 8/10
Exceptional managed exact mesh boolean (Cherchi-Attene surface arrangement: per-face in-plane CDT re-triangulation on exact crossings + GWN patch classification + regularized keep + weld). Correctly retires the naive convex-hull-tetrahedralization CSG.
- Defect (arrangement.md:3, 61, 469): the "tier-3 native asset" reserved for the scale path names NO concrete engine; `ArrangementPolicy.ScaleCeiling: int.MaxValue` so the gate is dead code with no target. This is the single most important unwired ecosystem seam. **Manifold (Apache-2.0, elalish/manifold, v3.5.2, guaranteed-manifold, C FFI) is the categorical-best engine** and must be named. §C VC-1.
- Coverage vs Fabrication: "mesh booleans/repair at manufacturing tolerance" — the managed exact arrangement is correct-at-any-scale but O(n²)-ish narrow-phase past broad-phase; the reserved native path is exactly where Manifold's parallel guaranteed-manifold boolean earns its place for production toolpath-grade watertight solids.
- Charter as-is: keep the managed body as the default; set `ScaleCeiling` to a finite value and route the reserved branch to a `Rasm.Fabrication`-or-kernel Manifold P/Invoke owner (native RID asset), gated behind a golden-boolean fixture per the page's own admission law (arrangement.md:469).

### Meshing/intersect.md — 8/10
Exceptional. Guigue-Devillers exact tri-tri, exact `OrderKey` chain ordering, BVH-broad-phased. Correctly collapses the three scattered inline crossing tests (repair `TriangleCrossPoint`, offset `SegmentsCross`, arrangement face-subdivision) onto one owner (intersect.md:3). No ecosystem dep beyond the kernel. Charter as-is.

### Meshing/offset.md — 8/10
Exceptional robust straight-skeleton/medial/Minkowski hand-roll; "CGAL GPL, Clipper float-epsilon rejected" is CORRECT for the robust core (offset.md:3).
- Defect (offset.md, whole page): MISSES the CAM arc-aware curve-offset-with-corner-strategies concern the Fabrication toolpath gate names explicitly ("offset curves with corner strategies"). The straight-skeleton yields MITERED offsets only; tool-radius compensation needs round/bevel/square joins + open-path end types with TRUE arcs (not polygonized). This is a genuine coverage gap. §C VC-3.
- Merge/move: the fix is NOT a new page — either an `Offset` arm here that composes CavalierContours (arc-bulge, ISC) for the toolpath modality, or Fabrication owns the CAM offset composing this kernel's medial for corner clearance. Prefer: kernel exposes robust skeleton/medial/Minkowski (as-is) + admits CavalierContours (ISC) as the arc-offset engine for the toolpath modality, since it is already in the manifest (orphaned) and is arc-exact.
- Charter as-is: keep the robust wavefront; add the arc-offset toolpath concern as a composed engine, not a hand-roll.

### Processing/repair.md — 8/10
Strong heal rail (`HealOp` union, exact-predicate weld/collapse/manifold/self-intersect kernels). Boolean row now delegates to arrangement (repair.md:434 BOOLEAN_NATIVE_ASSET), which delegates to the unnamed tier-3 → the Manifold gap (§C VC-1) resolves this too. Charter as-is.

### Processing/decimate.md — 8/10
Exceptional QEM (Garland-Heckbert at 106-bit ddouble, exact Orient3D collapse gate, reversible vsplit, directed Hausdorff, SDF voxel-remesh). Composes Vectors curvature/features/SDF + MathNet Cholesky.
- Ecosystem seam (decimate.md:17, and Directory.Packages.props:91 `Alimer.Bindings.MeshOptimizer` in the Compute label): meshoptimizer (MIT, battle-tested) is admitted centrally but NOT composed by Geometry. The hand-roll wins (meshoptimizer lacks the exact gate, vsplit, Hausdorff), but decimate.md names "meshlet-residency consumers" (decimate.md:3, 17) — meshlet generation + vertex-cache/overdraw optimization is meshoptimizer's domain and belongs to Compute. Verify the split is intentional (Geometry = exact QEM; Compute = meshoptimizer meshlet/encode) and record it so meshoptimizer is not mistaken for dead weight or duplicated.
- Coverage gap (minor): no isotropic/quad-dominant remeshing (only SDF voxel-remesh). No clean C# package (QuadriFlow/Instant Meshes are C++). Defer; note for Generation panelization.
- Charter as-is: keep; annotate the meshoptimizer/Compute seam.

### Processing/flatten.md — 9/10
Exceptional. LSCM (Lévy)/ARAP (Liu)/BFF (Sawhney-Crane) + Harmonic over ONE cached CholeskySparse factor, composing the Vectors DEC cotangent-Laplacian at its PUBLIC `MeshAdjointSnapshot` boundary handle (not re-assembling a Laplacian) and CSparse/MathNet through the Vectors wrappers. Textbook capability-composition; directly feeds the Fabrication unroll/nesting gate ("developable/unroll surfaces") via `DistortionReceipt`+`UvIsland`. No ecosystem dep to change. Charter as-is.

### Processing/fit.md — 8/10
Exceptional efficient-RANSAC (Schnabel-Wahl-Klein) + MLESAC/PROSAC + LM refine, composing Spatial BVH + Vectors cloud normals + MathNet. Correctly reuses the solver.md LM λ-ladder rather than a second NLS loop.
- Ecosystem note: no convex-hull owner anywhere in the corpus; `MIConvexHull` is orphaned (§D). Convex hull is a core CG primitive both consumers touch (stock/adaptive-clearing envelopes for Fabrication; bounding for Generation) — but fit.md does not need it. See §B/§D for the hull disposition.
- Charter as-is: keep.

### Processing/solver.md — 8/10
Strong author-kernel LM constraint solver (9-constraint `[Union]`, analytic Jacobian, DOF witness analysis, MathNet Cholesky). GPL rejection (SolveSpace/NeoGeoSolver) correct. Charter as-is.

### Processing/receipts.md — 8/10
Clean typed `RebuildReceipt` union (one case per HealOp), `ManifoldStatus` projected from Vectors `TopologyReceipt` (no second manifold computation). No ecosystem dep. Charter as-is.

### Drawing/view.md — 8/10
Strong predicate-exact hidden-line/silhouette/section/outline (Appel quantitative-invisibility + Newell-Newell-Sancha BSP), section delegated to intersect.md `PlaneMesh`. Correctly coexists with (does not thin) Rhino Make2D per architecture.md §03. No ecosystem dep. Charter as-is.

### Drawing/pack.md — 8/10
Strong 8-channel SoA geometry encoding over `System.Numerics.Tensors` residency seam, composing Vectors ScalarField (geodesic/curvature/SDF) + round-trip witness keyed to the canonical hash. Correctly the kernel producer the Compute tensor lane wraps. No ecosystem dep. Charter as-is.

---

## [C] CROSS-CUTTING FINDINGS

### C1 — DEAD PACKAGES: seven admitted kernel packages composed by ZERO design pages (the dominant ecosystem defect)
Grep of every `Packages:` line + full-corpus reference scan: the authoritative COMPOSED external set for Geometry is exactly `{GShark, Supercluster.KDTree.Net, CSparse, MathNet.Numerics (+MKL/OpenBLAS), PeterO.Numbers, TYoshimura.DoubleDouble, ExtendedNumerics.BigRational, System.Numerics.Tensors, System.IO.Hashing, Thinktecture, LanguageExt}`. The following are pinned in `Directory.Packages.props` "Kernel Geometry" (lines 69-79) AND referenced in `Rasm.csproj` (lines 18-31) but composed NOWHERE:

| Package | props line | csproj line | .api catalog | Disposition |
|---|---|---|---|---|
| LibTessDotNet | 74 | 20 | api-libtess.md | DEAD — tessellation superseded by hand-rolled delaunay. Remove. |
| CavalierContours | 70 | 25 | api-cavaliercontours.md | **NOT dead — mis-scoped.** ISC arc-aware offset is the §8 toolpath-offset engine. Promote to a real owner (offset.md toolpath arm / Fabrication), do not delete. |
| Clipper2 | 71 | 26 | api-clipper2.md | Mis-scoped to KERNEL. arrangement.md:14 says PlanarOverlay "retires the float Clipper2 the Fabrication NFP/silhouette lane carries" → Clipper2 is a FABRICATION dep. Move out of kernel csproj. |
| geometry3Sharp | 72 | 27 | api-geometry3sharp.md | DEAD + ABANDONED upstream (gradientspace README: unmaintained, recommends geometry4Sharp). Remove; if ever wanted, successor is geometry4Sharp (Boost-1.0, maintained, 282★). |
| MIConvexHull | 75 | 28 | api-miconvexhull.md | DEAD — no convex-hull owner exists. Either author a hull owner (fold into delaunay: hull = lower convex hull of the lifted paraboloid, a Bowyer-Watson byproduct) OR remove. Convex hull is a real CG gap both consumers touch. |
| SharpVoronoiLib | 76 | 29 | api-sharpvoronoilib.md | DEAD — Voronoi superseded by the tessellation dual (offset.md medial reconciles against the CDT Voronoi dual, delaunay.md). Remove. |
| Triangle | 78 | 30 | api-triangle.md | DEAD + rejection-rationale-wrong (Triangle.NET is MIT not GPL, delaunay.md:3). Remove; correct the rationale. |

Impact: 7 orphaned packages + 7 orphaned `.api` catalogs under `libs/csharp/Rasm/.api/`. Net move: delete 5 (LibTessDotNet, geometry3Sharp, MIConvexHull-or-own-hull, SharpVoronoiLib, Triangle) from the kernel group + their `.api`; RE-SCOPE 1 (Clipper2 → Fabrication); PROMOTE 1 (CavalierContours → real owner). This is the single largest ecosystem-core cleanup and a campaign-defining structural ruling.

### C2 — Duplication / concern-mixing: essentially NONE (corpus strength)
The corpus is exemplary here. intersect.md explicitly collapses the three scattered inline crossing tests. arrangement/offset/repair/decimate/flatten all COMPOSE sibling owners (Predicate, SpatialIndex, Tessellation, WeldDuplicates, Vectors ScalarField/DEC) rather than re-mint. No duplicate mechanism found. The ONLY latent duplication risk is the meshoptimizer-vs-hand-QEM and CavalierContours-vs-skeleton-offset boundaries, both resolved by the split-ownership rulings above (Geometry=exact, Compute/Fabrication=fast/CAM).

### C3 — Hardcoding vs generator: clean
Every owner is a data-table-driven polymorphic surface (`FrozenDictionary` builder/reader tables, `[SmartEnum]` row vocabularies, `[Union]`+`Apply` folds). Rosters (ArchProfile rows in the Generation spec, EncodingChannel lattice, etc.) are seed DATA, not enumerated instances. No enumerated-instance-where-generator-belongs defect in the geometry corpus. Policies are `record` rows with `Canonical` defaults, parameterized, not magic literals (the one class of literals — error-bound coefficients in `NumericsPolicy`:383-398 — are derived-once mathematical constants, correct).

### C4 — Dead typed carriers: none material
Every receipt/result union has a real consumer named at the seam (DecimationReceipt→Compute tiles, ChartAtlas→Fabrication unroll, BooleanReceipt→Bim, DrawingProjection→Fabrication/AppUi sheets, EncodedGeometry→Compute tensor). No dead carrier.

### C5 — Unwired seams
- **arrangement.md tier-3 native asset → no engine** (arrangement.md:61,469). The gate exists, the target is null. Manifold fills it. (§C VC-1)
- **repair.md Boolean → arrangement → tier-3** chain terminates at the same null (repair.md:434). Resolved by VC-1.
- **decimate.md meshoptimizer/Compute meshlet seam** — admitted package, unstated split. Annotate.
- **fit.md → convex hull** — no hull owner to compose. Resolve by owning hull or removing MIConvexHull.
- All OTHER cross-page seams are correctly wired (ALIGN-noted, consume-when-realized), verified in the CROSS_PAGE_SEAMS sections.

### C6 — Unmined admitted capability (catalog anchors)
- `Alimer.Bindings.MeshOptimizer` (props:91) — meshoptimizer simplify/meshlet/vertex-cache/overdraw/encode. Geometry correctly leaves it to Compute; confirm and document, do not mine into Geometry (would duplicate the exact QEM).
- `System.Numerics.Tensors` — mined correctly by pack.md (residency SoA). TensorPrimitives SIMD reductions could further accelerate the fit.md full-cloud MLESAC reduce and decimate.md Hausdorff sampling (both are element-wise reduces over `double[]`); currently plain loops. Minor optimization, not a defect.
- CSparse (props:60) — mined by flatten.md (sparse Cholesky via Vectors wrappers). Its `SparseQR`/`SparseLU` are unmined; solver.md/fit.md use MathNet dense Cholesky on the normal equations, which is correct for small dense sketch/fit systems. No gap.
- The three admitted arbitrary-precision libraries (DoubleDouble, BigRational, PeterO) are mined to the hilt by predicates.md — exemplary.

### C7 — Package-quality / version risks
- **MathNet.Numerics 6.0.0-beta2** (props:62-64) — the entire kernel LA floor (decimate/flatten/fit/solver) rides an UNRELEASED beta; upstream issue #1147 (open 2026-02) confirms no stable 6.0.0 and no ETA; 5.0.0 is the last stable (2022). Contradicts DEPENDENCY_POLICY "assume newest stable, pin only when incompatible." §C VC-4.
- **Triangle 0.0.6-Beta3** (props:78) — beta AND dead (remove).
- **GShark 2.3.1** — netstandard2.0, moderate maintenance velocity; watch item, no action.
- **ManifoldNET binding** — alpha (v1.0.7-alpha, stale 2024-11); the Manifold CORE is Apache-2.0/active/reference-grade, but the C# binding is the risk surface — decide alpha-binding vs in-house P/Invoke over the stable `manifoldc` C FFI. §C VC-1.

### C8 — Folder-architecture verdict — 8/10 (good, growth-conducive)
Six sub-domain folders mirroring the eventual source tree: `Numerics/`(2), `Parametric/`(1), `Spatial/`(3), `Meshing/`(4), `Processing/`(6), `Drawing/`(2). Logical decomposition by concern altitude (Numerics floor → Parametric/Spatial primitives → Meshing algebra → Processing pipelines → Drawing output). No flat sprawl, no loose one-file-one-folder combos except:
- `Parametric/` holds only `curve.md` — a one-file folder. JUSTIFIED as a growth axis (surface trimming/sweep/loft, degree ops, curve networks will land here) and it is a genuinely distinct concern (host-neutral NURBS) from Spatial/Meshing. Keep, but it is the one folder to watch for either growth or a merge into a `Curves/` peer of Meshing.
- `Numerics/` (predicates+faults) — faults.md is the domain-wide fault union, arguably a root-level concern, but co-locating it with predicates (the lowest floor) is defensible. Keep.
Growth headroom is good: new concerns land as a new page in the right folder or a new folder peer (e.g., a future `Fields/` if distance-field ownership ever migrates from Vectors, or `Curves/` if Parametric grows). No restructure needed.

---

## [D] VERDICT CANDIDATES (campaign-defining structural rulings, evidence-first)

**VC-1 — NAME MANIFOLD (Apache-2.0) as the tier-3 mesh-boolean native engine.** arrangement.md:3/61/469 and repair.md:434 reserve a "tier-3 native asset" for the scale/production path but name no engine, and `ScaleCeiling: int.MaxValue` makes the gate dead. Manifold (`elalish/manifold`, Apache-2.0, 2104★, v3.5.2 pushed 2026-06-27, guaranteed-manifold boolean, C FFI `manifoldc`) is the categorical-best and the only Apache-licensed guaranteed-manifold engine; MCUT is LGPL/commercial (weaker, gray). This is the direct answer to the Fabrication "mesh booleans/repair at manufacturing tolerance" gate for production watertight solids. Binding decision is the open sub-question: the community `ManifoldNET` binding is Apache-2.0 but alpha/stale (v1.0.7-alpha, 2024-11), so the ruling is "admit Manifold; author a thin in-house P/Invoke over the stable `manifoldc` C FFI as a HOST-BOUNDARY-adjacent native asset OR pin ManifoldNET with eyes open," gated behind the page's own golden-boolean fixture law. Central manifest add + `.api` catalog required.

**VC-2 — PURGE THE SEVEN ORPHANED KERNEL PACKAGES; correct two false rejection rationales.** Seven packages (`Directory.Packages.props`:70-78, `Rasm.csproj`:20-30) + their seven `.api` catalogs are composed by zero pages. Delete 5 (LibTessDotNet, geometry3Sharp [abandoned upstream], SharpVoronoiLib, Triangle, MIConvexHull-unless-hull-owned), RE-SCOPE Clipper2 to Fabrication (arrangement.md:14 confirms it is the Fabrication NFP dep), PROMOTE CavalierContours (VC-3). Simultaneously correct delaunay.md:3 ("Triangle GPL" → Triangle.NET is MIT; real reason is float-epsilon + no exact CDT3D) and predicates.md:3 ("no library carries a robustness guarantee" → no maintained/NuGet/indirect-complete library; RobustGeometry.NET is a direct-only unmaintained MIT precursor). This is the largest single ecosystem cleanup and removes the corpus's only real illusory-depth signal (an `.api` tier documenting packages the design rejects).

**VC-3 — CLOSE THE CAM ARC-OFFSET COVERAGE GAP with CavalierContours (ISC), not a hand-roll.** The Fabrication toolpath gate names "offset curves with corner strategies" explicitly; offset.md provides only MITERED polygon offset (straight-skeleton wavefront) and Minkowski — no round/bevel/square joins, no open-path end types, no true-arc tool-radius compensation. CavalierContours.NET (ISC, .NET 10, `IFloatingPointIeee754<T>`, arc-bulge vertices — TRUE arcs not polygonized, boolean + `StaticAABB2DIndex`) is the categorical-best and is ALREADY in the manifest (orphaned). Ruling: keep offset.md's robust skeleton/medial/Minkowski as the exact-core, and add a toolpath arc-offset modality that composes CavalierContours (either an `Offset` arm here or a Fabrication owner), moving CavalierContours from dead weight to a real owner. This is the cleanest resolution of the offset.md-vs-Fabrication tension: robust-core stays hand-rolled-exact, CAM offset composes the arc-exact permissive engine.

**VC-4 — RESOLVE THE MathNet.Numerics 6.0.0-beta2 BETA FLOOR.** The whole kernel linear-algebra floor (decimate/flatten/fit/solver, `Directory.Packages.props`:62-64) rides an unreleased prerelease; MathNet issue #1147 (open 2026-02) confirms no stable 6.0.0 and no ETA, 5.0.0 stable since 2022. This directly contradicts DEPENDENCY_POLICY "assume newest stable, pin only when incompatible." Ruling: either downgrade to 5.0.0 stable (battle-tested, API-compatible for the Cholesky/SVD/EVD surface the corpus uses) or accept 6.0.0-beta2 with an explicit recorded rationale + a tracking blocker to bump to stable on release. A silent beta pin across the numerics floor is the risk.

**VC-5 — HAND-ROLL RULINGS ARE VALIDATED; make the rationales precise, not absolute.** Every "reject external, author from first principles" ruling verifies as CORRECT for the robust core: predicates (no maintained indirect-predicate lib), exact CDT (no OSS exact CDT+CDT3D), exact arrangement (Manifold is the SCALE path, not the exact-core replacement), robust straight-skeleton (CGAL GPL, all permissive .NET SK are float), LM constraints (SolveSpace GPL). The corpus's robustness-first + permissive-license stance is coherent and defensible. The only fixes are the two imprecise rationales (VC-2) and one missing RID/strict-IEEE-754 invariant on `NumericsPolicy` (predicates.md TwoSum safety). No hand-roll should be replaced by an external library for the exact core; externals enter only as (a) the reserved SCALE native asset (Manifold, VC-1) and (b) the CAM arc-offset modality (CavalierContours, VC-3).

**VC-6 — OWN CONVEX HULL or drop MIConvexHull.** No convex-hull owner exists in the 18-page corpus, yet convex hull is a core CG primitive both consumers touch (Fabrication: stock/adaptive-clearing envelope, minimum bounding; Generation: bounding/containment). `MIConvexHull` is admitted-but-orphaned. Ruling: either author a `hull` concern (naturally folds into delaunay.md — the convex hull is the lower convex hull of the paraboloid-lifted points, a direct Bowyer-Watson byproduct, so it is a `Tessellation` projection not a new package) OR remove MIConvexHull as dead weight. Preferred: fold hull into the delaunay owner (denser, no new package, exact via the same InCircle floor), and remove MIConvexHull.

**VC-7 — DOCUMENT THE MESHOPTIMIZER / DISTANCE-FIELD OWNERSHIP SPLITS as settled seams.** Two admitted capabilities sit adjacent to Geometry but are correctly owned elsewhere, and the corpus should state this to prevent future duplication or dead-weight mislabeling: (a) meshoptimizer (`Alimer.Bindings.MeshOptimizer`, props:91) owns meshlet/vertex-cache/overdraw/encode in COMPUTE — Geometry's exact QEM (decimate.md) is a distinct concern, not a duplicate; (b) distance fields / SDF / iso-surface are owned by `Rasm.Vectors.ScalarField` (composed by decimate/pack/flatten) — the Fabrication "distance fields" gate is met at that seam, and CAM material-removal simulation (MillSimSharp-class, SDF+voxel) is a FABRICATION owner, not kernel Geometry. Both splits are architecturally correct; recording them closes the "unmined admitted capability" question honestly.

---

## [E] KEY EVIDENCE ANCHORS (for the author agent)
- Composed-set proof: every `Packages:` line grepped; orphans have zero composing pages (whole-corpus reference scan).
- Manifold: elalish/manifold Apache-2.0, v3.5.2 (2026-06-27), C FFI; ManifoldNET Apache-2.0 alpha v1.0.7-alpha (weianweigan, stale 2024-11).
- geometry3Sharp abandoned (gradientspace README recommends geometry4Sharp Boost-1.0, 282★).
- RobustGeometry.NET MIT Shewchuk port, direct-only, no NuGet, ~2012, FPU-precision caveat.
- Triangle.NET MIT (delaunay.md:3 "GPL" is wrong); TetGen AGPL; SISL AGPL; CGAL SK GPL; SolveSpace GPL; MCUT LGPL/commercial — all license-verified.
- CavalierContours.NET ISC, .NET 10, arc-bulge true-arc offset+boolean, `StaticAABB2DIndex`, generic math.
- Clipper2 JoinType{Miter,Square,Bevel,Round}+EndType{Polygon,Joined,Butt,Square,Round} = the CAM corner-strategy vocabulary offset.md lacks; Boost license.
- MathNet 6.0 no stable (issue #1147 open 2026-02); 5.0.0 stable 2022.
- Manifest anchors: `Directory.Packages.props` Kernel Numerics 59-67 (MathNet beta 62-64), Kernel Geometry 69-79; `Rasm.csproj` Geometry 18-22 + Computational Geometry 24-31; MeshOptimizer props:91.
