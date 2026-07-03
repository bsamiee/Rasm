# DOSSIER — Rasm/Geometry — lane: ecosystem-adjacent

Scope: REAL OSS ecosystem deep-dive for the target's ADJACENT/expansion concerns — the KERNEL GATE growth axes (RASM-GENERATION-SPEC `[04]` + `[05]`), the Fabrication scope-expansion axes, and what the corpus is silent on. Candidate table = seed data for brief escalation + roster rows. License gate: OSS + free-for-OSS-commercial admissible; GPL/AGPL/pay-tiered REJECTED. Native C++/managed-binding admissible only where the managed ecosystem lacks the concern.

Corpus read: 18 pages (Meshing 4, Processing 6, Spatial 3, Drawing 2, Numerics 2, Parametric 1); all fully or charter-read. Seam-counterparts read: `libs/.planning/architecture.md`, `RASM-GENERATION-SPEC.md`, `Rasm/Vectors/_ARCHITECTURE.md` + grep of Vectors source, `Directory.Packages.props` (Kernel Geometry + Numerics groups).

## [00]-[DECISIVE CONTEXT — WHAT VECTORS ALREADY OWNS]

The single most consequential fact for this lane: `Rasm.Vectors` (the mature sibling kernel sub-domain, no `.planning`) already owns most of the surface-field substrate the KERNEL GATE names. Verified in source (`libs/csharp/Rasm/Vectors/`):
- `ScalarField.Geodesic` heat-method geodesic distance + `GeodesicWindow`/`GeodesicTracePolicy`/`GeodesicStopKind` geodesic PATH tracing (`Mesh.cs:3447,3672,4006,4033`; `Field.cs:330,1834`).
- `CrossField` / direction fields (`Field.cs:333,617,735`; `Mesh.cs:642,2495`).
- `LogMap` logarithmic/exponential map (`Field.cs:1834,1872,1958`; `Mesh.cs:467,480,3323,3344`; `Extraction.cs:298`) — the core pattern-to-surface primitive.
- `VectorHeat` vector heat method / parallel transport (`Mesh.cs:642,3251,3268,3344`).
- `Isoline` iso-contour extraction (`Extraction.cs:137,163,209,416`).
- `Streamline` field-line tracing (`Flow.cs:98,113,441,646,700`; `Intent.cs:21`).
- `MeanCurvatureFlow` (`Field.cs:331`), `ScalarField.SignedDistanceFromMesh` SDF, `IsoSurfaceDetailed` marching cubes (`Field.cs:451,2009`), `DiscreteCalculus` cotangent-Laplacian DEC (`Spectral.cs`), `VectorCloudMetric.OrientedNormals`/`PrincipalCurvature` (`Cloud.cs`), `FeatureEdge` dihedral classification.

Consequence: **distance fields, geodesic distance+paths, cross fields, log maps, isolines, streamlines, parallel transport, marching-cubes iso-surfacing, SDF, DEC/Laplacian are NOT Geometry silences** — they are Vectors owners that Geometry pages compose (pack.md, flatten.md, decimate.md all consume them). Any silence claim naming these would be false. The TRUE silences are the COMPOSITION OWNERS one level up that turn these primitives into surface-development artifacts — and those are absent from BOTH Geometry and Vectors.

## [01]-[PER-PAGE VERDICTS]

Verdict = capability-completeness-vs-owned-domain through the ecosystem-adjacent lens (what the page should grow / what adjacent capability it lacks). The corpus is world-class on its named concerns; grades are high because the defects are silences and roster coherence, not naive code.

### Parametric/curve.md — VERDICT 7/10
- Deep, correct GShark composition (curve/surface eval/measure/divide/split/reconstruct/intersect over one `Parametric` `[Union]` + `Apply`). Retires the in-house NURBS-Book hand-roll cleanly.
- **Adjacent gap (surface-surface intersection):** owns curve∩curve, curve∩line, curve∩plane (GShark `Intersection.Intersect`, `curve.md:16`) but NO surface∩surface intersection (SSI) → trimmed-surface construction. GShark has no SSI; the page defers surface intersection to Rhino host / discrete `Meshing/intersect`. For a NURBS kernel feeding Generation/Fabrication this is a real hole (trimmed lofts, developable seams).
- **Folder-architecture defect (LOAD-BEARING):** this is the ONLY page in `Parametric/` (156 LOC, one file, one folder). The one-file folder is rot precisely BECAUSE its natural siblings — the surface-development owners (subdivision, developable/unroll, panelization, pattern-mapping) the KERNEL GATE demands — are all absent (see `[02]`, `[03]`, VC-1).
- Owner charter as it SHOULD be: `Parametric/` grows into the surface/curve sub-domain (or is renamed `Surfacing/`) owning curve+surface eval AND the surface-development family; `curve.md` gains a `SurfaceIntersect` op or an explicit SSI-deferred-to-host charter row.

### Meshing/offset.md — VERDICT 9/10
- Exceptional: Aichholzer-Aurenhammer wavefront over exact `Orient2D`, one `OffsetOp` `[Union]` closing skeleton/weighted-skeleton/medial/Minkowski/polygon-offset. Owns 2D polygon **medial axis** (`offset.md:16` `MedialFrom`).
- **Adjacent gap (3D medial / skeleton):** Fabrication scope names "medial axis" for toolpath geometry; the 2D polygon medial is owned but 3D mesh medial-axis / curve-skeleton is silent (see VC-5). No permissive OSS lib (CGAL GPL); author-kernel over Vectors `MeanCurvatureFlow`.
- **Adjacent gap (toolpath corner strategies):** the wavefront gives mitered offset; Fabrication "offset curves with corner strategies" (round/square/miter joins, end caps) is not a wavefront modality — it lives in Clipper2/CavalierContours in the Fabrication lane. Clarify the kernel-vs-Fabrication offset seam.
- Category slight: `offset` is a 2D-CURVE concern filed under `Meshing/` (which implies 3D mesh). Move pressure → a curve/2D sub-domain.

### Meshing/intersect.md — VERDICT 9/10
- Guigue-Devillers exact tri-tri, BVH-broad-phased mesh∩mesh + plane∩mesh sectioning + exact `OrderKey` chain. `PlaneMesh` produces one section contour (`intersect.md:16`).
- **Adjacent gap (slicing STACK):** Fabrication scope names "slicing/section stacks" for AM. `PlaneMesh` is a SINGLE cut; a slicing owner (parallel-plane stack → per-layer contour tree with inside/outside nesting, adaptive layer height, contour ordering) is silent (see VC-4). Composes `PlaneMesh` but adds the stack/tree/adaptive structure.
- Growth arm `curve-surface`/`swept-volume` intersection noted (`intersect.md:19`) but SSI stays absent (ties to curve.md gap).

### Meshing/arrangement.md — VERDICT 9/10
- Cherchi-Attene exact managed mesh boolean + planar overlay + cell complex; GWN patch classification; composes delaunay/predicates/spatial/healing. Retires float Clipper2 for the robust core.
- **Adjacent gap (UNFILLED reserved native slot — HIGH VALUE):** the page explicitly reserves a tier-3 native-scale asset behind `ArrangementPolicy.ScaleCeiling` (`arrangement.md:16,469` `BeyondManaged`, `NativeAssetMissing`) and leaves it UPSTREAM-BLOCKED. **Manifold (Apache-2.0)** is the categorical-best OSS fit for that exact slot (see VC-2, candidate table).
- **Adjacent gap (convex decomposition):** approximate convex decomposition (ACD) is an arrangement-adjacent concern (nesting, collision, assembly) that no page owns (see VC-6).

### Meshing/delaunay.md — VERDICT 9/10
- Bowyer-Watson CDT 2D + tetrahedralization 3D over exact `InCircle`/`InSphere` + Lpi/Tpi Steiner recovery. Rejects Triangle/TetGen (GPL).
- **Adjacent gap (quality/quad remeshing):** owns triangulation, not isotropic-quality remeshing or field-guided quad remeshing (see VC-7). Growth arms `power/weighted Delaunay`, `alpha-complex` noted (`delaunay.md:19`) but unbuilt.
- Roster coherence: rejects Triangle yet `Triangle 0.0.6-Beta3` sits in the Kernel Geometry package group (see `[04]`, VC-3).

### Processing/flatten.md — VERDICT 9/10
- Harmonic/LSCM/ARAP/BFF UV flattening over the Vectors DEC cotangent-Laplacian + one cached Cholesky factor. World-class.
- **Adjacent gap (developable unroll):** Fabrication "unroll" + KERNEL GATE "developable" is adjacent but distinct — ARAP/BFF give low-distortion conformal maps, not guaranteed-isometric developable-strip unrolling (sheet metal / plywood / fabric). Growth arm `seamless-direction-field global parameterization` noted (`flatten.md:19`) — that arm plus a developable owner is the surface-development seam (see VC-1).
- The `ChartAtlas` is the exact carrier a panelization/pattern-mapping owner would consume; that consumer is silent.

### Processing/fit.md — VERDICT 8/10
- Schnabel efficient-RANSAC + MLESAC/PROSAC + geometric-orthogonal-distance LM over plane/sphere/cylinder/cone/torus/line, analytic Jacobians. Strong.
- **Adjacent gap (freeform surface fit):** only ANALYTIC primitives; B-spline/NURBS surface fitting from a cloud (feeding curve.md reconstruction on the surface side) is absent — growth arm names paraboloid/ellipsoid (`fit.md:19`) but not freeform.
- Registration/ICP correspondence is referenced (`fit.md:16`) but likely owned by Vectors `Align.cs` — confirm, avoid a second ICP owner.

### Processing/decimate.md — VERDICT 9/10
- Garland-Heckbert QEM + progressive/voxel-remesh/feature-preserve + one-sided Hausdorff + reversible vsplit stream, 106-bit `ddouble` quadrics. Composes Vectors SDF/iso-surfacer + curvature + features.
- **Adjacent gap (isotropic/quality remeshing):** decimation goes COARSE; `VoxelRemesh` is SDF-resample (topology-changing); there is no isotropic-quality remesher (Botsch-Kobbelt incremental: split/collapse/flip/tangential-relax to uniform edge length preserving genus) nor field-guided quad remesh (see VC-7). The "remesh" concern is under-owned (voxel only).

### Processing/repair.md — VERDICT 9/10
- Closed `HealOp` family (weld/degenerate/gap/manifold/self-intersect/orient) + boolean-via-arrangement. Composes predicates/spatial/arrangement. Well-scoped; boolean correctly delegated.
- No major adjacent gap. `SelfIntersectResolve` inline crossing test flagged by intersect.md for collapse onto `Intersection.Apply` (`intersect.md:388`) — an internal collapse, not a silence.

### Processing/solver.md — VERDICT 8/10
- Author-kernel 2D/3D parametric-sketch LM constraint solver, 9-constraint closed `[Union]`, analytic partials, witness-DOF. Rejects SolveSpace/NeoGeoSolver (GPL). Excellent for CAD sketching.
- **Adjacent gap (form-finding / physical relaxation):** this is a CAD-SKETCH solver. Generation/architectural form-finding (dynamic relaxation, force-density, projective/position-based dynamics for gridshells and membranes) is a DIFFERENT optimization concern the same LM machinery does not cover. Borderline kernel scope; flag for Generation review — a shell/gridshell campaign will want it.

### Spatial/index.md — VERDICT 9/10
- BVH(SAH)/octree/agglomerative + Supercluster kd-tree PointCloud leaf + GWN winding query, one `NodeStore` SoA, Compute-seam acceleration projection. Comprehensive; no adjacent gap.

### Spatial/naming.md — VERDICT 8/10
- Topological naming (`TopoName` lineage + `Track` re-anchor by signature). Governance page, well-scoped; scales by consuming `RebuildLog` hints.

### Spatial/reconciliation.md — VERDICT 8/10
- `CanonicalTopology` + `NamingHash` bridge to Persistence `GeometryHash` + `ONE_WIRE_FIXTURE_CORPUS`. Governance; well-scoped, orthogonal reference/content axes.

### Numerics/predicates.md — VERDICT 10/10
- Four-tier adaptive exact-predicate ladder (double→ddouble→Expansion→Fraction) + direct + implicit Lpi/Tpi. The robustness floor every page rides. No gap; the campaign's crown jewel and the reason external geometry libs are rejected.

### Numerics/faults.md — VERDICT 9/10
- Consolidated `GeometryFault` band-2400 `[Union]`, one sub-band per owner (2400/2420/2424/2428/2432/2436/2440/2444). Governance; scales by row — a new adjacent owner (subdivision/develop/panelize/slice/skeleton) is one new sub-band.

### Drawing/view.md — VERDICT 9/10
- Newell-BSP painter + Appel quantitative-invisibility + exact `Orient3D` silhouette locus; section delegated to `IntersectOp.PlaneMesh`. HLR/silhouette/section/outline over one `ViewOp`. No adjacent gap.

### Drawing/pack.md — VERDICT 9/10
- Eight-channel `EncodingChannel` SoA float payload (position/normal/color/curvature/geodesic/intensity/occupancy/weight) composing Vectors geodesic/curvature/SDF fields; round-trip-witnessed. Comprehensive; correctly composes the owned Vectors fields.

### Processing/receipts.md — VERDICT 8/10
- `RebuildReceipt` typed per-op heal evidence + `RebuildLog` re-anchor seed. Governance; well-scoped.

## [02]-[CROSS-CUTTING FINDINGS]

### Unmined capability (the surface-development composition layer)
The KERNEL GATE (`RASM-GENERATION-SPEC.md:179,226`) names, as kernel operations Generation composes: "stations/frames, isolines/geodesics/offsets, tessellation/subdivision, panelization, pattern-to-surface mapping." Cross-referenced against BOTH Geometry AND Vectors:
- stations/frames ✓ curve.md; isolines ✓ Vectors `Isoline`; geodesics ✓ Vectors `Geodesic`; distance/curvature fields ✓ Vectors `ScalarField`; cross fields ✓ Vectors `CrossField`; log map ✓ Vectors `LogMap`; tessellation ✓ delaunay.md.
- **subdivision — SILENT** (no Catmull-Clark/Loop/Doo-Sabin in Geometry or Vectors; grep confirms zero `Subdivid`/`CatmullClark`/`LoopSubdiv`).
- **panelization — SILENT** (no owner mapping a discrete panel grid onto a surface region via cross-field + parameterization).
- **pattern-to-surface mapping — SILENT AT OWNER LEVEL** (the primitive `LogMap` exists in Vectors, but no owner composes LogMap + a 2D symmetry-group tiling + placement into a surface-mapped instance stream — the Generation SPEC `[05]` PATTERN/TILING + SURFACE-DEVELOPMENT planes' exact input).
- **developable / unroll — SILENT** (flatten UV-param is adjacent but not guaranteed-isometric developable-strip unroll).
This quartet is the single largest unmined-capability finding: the PRIMITIVES are owned, the COMPOSITION OWNERS are absent, and Generation stand-up is GATED on them by the SPEC's own KERNEL GATE law. Catalog anchors: compose Vectors `CrossField`/`LogMap`/`Geodesic`/`Isoline` + flatten `ChartAtlas` + delaunay `Build`.

### Roster ↔ corpus coherence defect (hardcoding-vs-reality)
`Directory.Packages.props` "Kernel Geometry" group (`:69-78`): CavalierContours, Clipper2, geometry3Sharp, GShark, libtess, MIConvexHull, SharpVoronoiLib, Supercluster.KDTree, Triangle. The pages CONTRADICT most of this:
- delaunay.md: "No external geometry library is admitted (Triangle/TetGen are GPL and rejected)" — author-kernels CDT; **Triangle is a roster phantom in the kernel.**
- offset.md: "Clipper offsetting is float-epsilon and rejected for the robust core"; author-kernels Voronoi-dual medial — **SharpVoronoiLib superseded.**
- arrangement.md: "retiring the float Clipper2 the fabrication lane carries" — **Clipper2/CavalierContours belong to the Fabrication lane, not the kernel.**
- Genuinely composed by the pages: **GShark** (curve.md), **Supercluster.KDTree.Net** (index.md), and the Numerics group (**CSparse, MathNet.Numerics + MKL/OpenBLAS, TYoshimura.DoubleDouble, ExtendedNumerics.BigRational, PeterO.Numbers**). MIConvexHull/libtess/geometry3Sharp have no live composition in any read page.
- **geometry3Sharp 1.0.324 (roster) is ABANDONED** (GitHub README "A Short Note about the future of geometry3Sharp (updated Jan 2026)"); the maintained Boost successor is **geometry4Sharp** (NuGet 1.0.0). Since the kernel author-kernels its mesh processing, geometry3Sharp is a removal candidate, not a replacement — unless a managed mesh-toolkit substrate is wanted, in which case geometry4Sharp supersedes it.
This is a package-lane finding surfaced here because it directly shapes the roster rows: prune the superseded kernel packages (or relocate to Fabrication), keep GShark + Supercluster + numerics.

### Concern mixing / folder taxonomy (folder-architecture axis)
- `Processing/` groups five heterogeneous concerns by loose verb-category: `fit` (RANSAC), `flatten` (parameterization), `decimate` (simplification), `repair` (healing), `solver` (constraints). Each has a distinct namespace (`Fitting`/`Parameterization`/`Simplification`/`Healing`/`Constraints`). The folder is a grab-bag, not a coherent sub-domain.
- `Meshing/` mixes 3D-mesh concerns (arrangement/intersect/delaunay) with a 2D-CURVE concern (offset → skeleton/medial/Minkowski/polygon-offset).
- `Parametric/` is a ONE-FILE folder (curve.md) — the clearest folder-architecture rot: it is a stub whose intended siblings (the surface-development quartet) are the campaign's biggest silences. There is NO home in the current taxonomy for subdivision/developable/panelization/pattern-mapping.
- Verdict: the taxonomy is not conducive to the growth axis. A concern-coherent re-partition should create an explicit surface-development home (grow `Parametric/`→`Surfacing/` or add `Development/`) and consider splitting `Processing/`.

### Duplication, dead carriers, unwired seams (ecosystem-lens)
- No dead typed carriers found in-lane; receipts are typed-per-op and consumed. The corpus is disciplined here.
- Unwired seam (forward, healthy): `arrangement.md:478` names the Fabrication HLR/nesting `PlanarOverlay` consumers as consume-when-realized — correct, not a defect.
- The three inline crossing tests (repair/offset/arrangement) collapsing onto `Intersection.Apply` (`intersect.md:388`) is an internal densification already recorded — not an ecosystem gap.

## [03]-[OSS CANDIDATE TABLE — seed data for roster rows + brief escalation]

License gate applied. "Managed path" = author-kernel-from-first-principles (the corpus philosophy, over owned Vectors substrate) unless a permissive engine wins on scale. "Native" = admit only where managed ecosystem lacks the concern; behind a reserved scale gate.

| # | Adjacent concern | Gate origin | Best OSS engine | License | Status/currency | .NET path | Verdict |
|---|---|---|---|---|---|---|---|
| 1 | Mesh boolean at scale | arrangement.md reserved tier-3 slot | **Manifold** (elalish) v3.5.2 | Apache-2.0 | active (push 2026-06-27, 90 contrib) | `ManifoldNET` NuGet 1.0.7-**alpha**; also mclaager/weianweigan bindings; C API for P/Invoke | ADMIT as the reserved native scale companion behind `ScaleCeiling`; binding is alpha → P/Invoke the C API or vendor the binding |
| 2 | Subdivision surfaces | KERNEL GATE "subdivision" | **OpenSubdiv** (Pixar) | TOST (permissive, patent grant) | active, C++/GPU | no official .NET binding | Managed author-kernel Catmull-Clark/Loop (topological rules, small) is the default; OpenSubdiv only if limit-surface/GPU eval at scale is demanded |
| 3 | Quad remesh / cross-field guidance | KERNEL GATE "panelization"; VC-7 | **QuadriFlow** (hjwdzh) / **libigl** comb+cross-field | QuadriFlow BSD; libigl **MPL-2.0** | QuadriFlow stable; libigl active | C++; no .NET | Managed author-kernel over Vectors `CrossField` (already owned) is the default; algorithm refs = QuadriFlow, libigl MPL2. **Instant Meshes / NeurCross = AGPL/"Other" → verify-or-reject** |
| 4 | Approximate convex decomposition | arrangement/nesting-adjacent; VC-6 | **CoACD** (SIGGRAPH'22) / **V-HACD** | CoACD MIT; V-HACD BSD-3 | CoACD active; V-HACD active (2025-09) | `VHacdSharp` (BSD-3, C#, stale 2019, not on NuGet); Unity VHACD fork Apache-2.0 w/ C# | Native P/Invoke to V-HACD (BSD) or CoACD (MIT); both clear the gate. No maintained NuGet → vendor/build |
| 5 | Developable strip / unroll | KERNEL GATE + Fabrication "unroll" | **RectifyingStripPatterns** (SIGGRAPH'23), **Dev2PQ** (algorithm) | RectifyingStrip **MIT**; Dev2PQ paper | research code | C++; no .NET | Managed author-kernel over flatten `ChartAtlas` + Vectors `CrossField`/curvature. **SDQ_meshes = GPL-3 → REJECTED.** No productized lib → author-kernel |
| 6 | 3D medial axis / curve skeleton | Fabrication "medial axis" (3D); VC-5 | CGAL `Surface_mesh_skeletonization` (MCF) | **GPL/commercial → REJECTED** | CGAL active | none permissive | Managed author-kernel: mean-curvature-flow skeletonization composes Vectors `MeanCurvatureFlow` (already owned). No permissive engine exists — author-kernel is mandatory |
| 7 | NURBS SSI / trimmed BREP | curve.md/intersect.md SSI gap | **OpenCASCADE** (OCCT) 7.x | LGPL-2.1+exception | active | OCCSharp (Gitee), OCCT3D wrapper, OCC.NET; **LNLib** LGPL-2.1 C#/C++ NURBS-Book | LGPL is borderline for OSS (dynamic-link OK); heavy, overlaps Rhino host. LIKELY REJECT (meets Rhino at wire); note LNLib as a NURBS-Book algorithm ref |
| 8 | Managed mesh toolkit (if wanted) | roster coherence (geometry3Sharp dead) | **geometry4Sharp** (NewWheelTech) | Boost-1.0 | maintained fork; NuGet 1.0.0 | native C# | REPLACE dead geometry3Sharp 1.0.324 IF any managed toolkit is composed; else PRUNE both (kernel author-kernels its mesh ops) |
| — | Slicing/section stack (AM) | Fabrication "slicing/section stacks"; VC-4 | (Cura/Slic3r cores = **AGPL → REJECTED**) | — | — | — | Managed author-kernel over `IntersectOp.PlaneMesh` + Clipper2 (Fabrication lane). No permissive slicer core → author-kernel |
| — | Half-edge mesh substrate | infra | **Plankton** (meshmash) | **LGPL-3** borderline | active | C# | Not needed — Vectors owns native `Mesh` + `MeshEdit`; noted for completeness |

Rejected-by-license (named so the brief does not re-propose): CGAL (GPL/commercial), Instant Meshes (license "Other"/GPL-family — verify), NeurCross (AGPL-3), SDQ_meshes (GPL-3), CuraEngine/Slic3r AM slicer cores (AGPL), SISL NURBS (AGPL/commercial). GShark stays the composed NURBS engine; the exact-predicate + author-kernel philosophy means most silences are filled by NEW managed pages, not packages — the ecosystem's value here is algorithm references + two native scale companions (Manifold, optionally V-HACD/CoACD).

## [04]-[VERDICT CANDIDATES — campaign-defining structural rulings]

**VC-1 (STRONGEST) — The surface-development quartet is the campaign's defining silence, and `Parametric/` is a one-file stub because of it.** Subdivision, developable/unroll, panelization, and pattern-to-surface MAPPING owners are absent from BOTH Geometry and Vectors, yet the RASM-GENERATION-SPEC KERNEL GATE (`RASM-GENERATION-SPEC.md:179,226`) names all four as kernel operations Generation composes, and `[05]` makes SURFACE-DEVELOPMENT + PATTERN/TILING the engine's core planes. The enabling PRIMITIVES are already owned (Vectors `CrossField`/`LogMap`/`Geodesic`/`Isoline`, flatten `ChartAtlas`, delaunay `Build`) — only the composition owners are missing. Ruling: create a surface-development home (grow `Parametric/`→`Surfacing/` or add `Development/`) with `Subdivision`, `Develop` (unroll), `Panelize`, `PatternMap` owners; this is the KERNEL GATE that unblocks the entire Rasm.Generation campaign. Evidence: `Parametric/curve.md` sole page; grep of Geometry+Vectors returns zero subdivision/develop/panelize/unroll owners.

**VC-2 — arrangement.md's reserved tier-3 native boolean slot is NAMED-BUT-UNFILLED; Manifold is the categorical-best fit.** `arrangement.md:16,469` reserves `ArrangementPolicy.ScaleCeiling`/`BeyondManaged`/`NativeAssetMissing` for "the future performance/scale path the managed exact arrangement does not cover" and leaves it UPSTREAM-BLOCKED. Manifold (Apache-2.0, v3.5.2, active, guaranteed-manifold robust boolean, `ManifoldNET`/C-API) is exactly that engine and clears the license gate. Ruling: admit Manifold as the native scale companion behind the existing gate (binding is alpha → P/Invoke the C API); the design page already has the seam.

**VC-3 — Roster/corpus coherence defect: the "Kernel Geometry" package group is contradicted by the author-kernel pages.** `Directory.Packages.props:69-78` pins Triangle/Clipper2/CavalierContours/geometry3Sharp/SharpVoronoiLib/MIConvexHull/libtess, but delaunay.md rejects Triangle (GPL), offset.md rejects Clipper (float), arrangement.md retires Clipper2 to the Fabrication lane, and delaunay author-kernels the Voronoi dual. Only GShark + Supercluster.KDTree + the Numerics group are composed by read pages; geometry3Sharp 1.0.324 is additionally abandoned (Jan-2026), successor geometry4Sharp (Boost, NuGet 1.0.0). Ruling: prune superseded kernel packages (relocate Clipper2/CavalierContours to Fabrication where the pages say they belong); the kernel roster is GShark + Supercluster + numerics only.

**VC-4 — Slicing / section STACK for additive manufacturing is silent (only single-plane sectioning owned).** `intersect.md:16` `PlaneMesh` produces ONE contour; Fabrication scope names "slicing/section stacks." No owner composes PlaneMesh into an adaptive layer stack with a per-layer contour-nesting tree and layer-height policy. Ruling: a `Slicing` owner (Fabrication-AM-gated) composing `IntersectOp.PlaneMesh` + the inside/outside contour tree; no permissive slicer core exists (Cura/Slic3r AGPL) → author-kernel.

**VC-5 — 3D medial-axis / curve-skeleton is silent (only 2D polygon medial owned).** offset.md owns 2D medial via straight skeleton (`offset.md:16`); Fabrication scope names "medial axis" for toolpath geometry, which for 3D solids means the mesh medial-axis/curve-skeleton. CGAL's skeletonization is GPL (rejected); no permissive engine exists. Ruling: author-kernel mean-curvature-flow skeletonization composing the already-owned Vectors `MeanCurvatureFlow` — mandatory managed path.

**VC-6 — Approximate convex decomposition (ACD) is an unowned arrangement/nesting-adjacent concern.** No page owns ACD, which feeds Fabrication nesting/packing, Generation assembly-collision, and physics proxies. CoACD (MIT) and V-HACD (BSD-3) are permissive native engines. Ruling: admit as an arrangement-adjacent owner (native P/Invoke to CoACD/V-HACD, or managed author-kernel); clears the license gate cleanly.

**VC-7 — Remeshing is under-owned: decimation (coarse) + SDF voxel-remesh exist, but no isotropic-quality or field-guided quad remesh.** decimate.md `VoxelRemesh` is SDF-resample (topology-changing); there is no Botsch-Kobbelt isotropic remesher (uniform edge length, genus-preserving) nor cross-field quad remesh — both feed panelization (VC-1) and Fabrication mesh prep. Ruling: a `Remesh` owner composing Vectors `CrossField` (quad) + the DEC (isotropic); algorithm refs QuadriFlow (BSD)/libigl (MPL2); Instant Meshes is license-"Other"/AGPL-adjacent → rejected.

**VC-8 — Folder taxonomy is not conducive to the growth axis.** `Processing/` is a five-concern grab-bag, `offset` (2D curve) sits under `Meshing/` (3D), and `Parametric/` is a one-file stub with no sibling home for surface-development. Ruling: re-partition toward concern-coherent sub-domains with an explicit surface-development home; this folder-architecture ruling is the structural precondition for landing VC-1/VC-4/VC-5/VC-6/VC-7 without scattering them into mismatched folders.
