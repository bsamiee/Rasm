# [LEDGER_ECOSYSTEM] — Rasm kernel .NET package research

Scope: the converting kernel (`libs/csharp/Rasm`) — the eight kernel domains SIMD/vectorized math, tensor/array substrates, computational-geometry primitives, spatial indexing, robust predicates, mesh/half-edge structures, sparse/dense numerics, spectral/graph algorithms. Read-only research; the roster delta is a proposal for the conversion blueprint, not applied here.

Gate: OSS or free-for-OSS-commercial only, never pay-tiered. Every candidate verified on the nuget MCP (feed reality) plus two web sources (repo + license/docs). Kernel constraint: pure-managed AnyCPU on **osx-arm64** — native and dual-commercial packages are disqualified for the kernel even when categorical-best (they route to `Rasm.Compute`, which already admits native deps).

Verdict vocabulary: KEEP (current pick is categorical-best), KEEP+ (keep with a flagged caveat), DROP (dead/superseded/unusable, remove), REPLACE (swap for a named package), ADD (admit for an uncovered concern), WATCH (strong candidate not yet admissible — unpackaged or stale package), REJECT (evaluated, not admitted).

## [01]-[CURRENT_KERNEL_ROSTER]

`Rasm.csproj` (folder-declared) plus the shared substrate rows `Rasm` inherits from `libs/csharp/.api/`. Versions from `Directory.Packages.props`, all re-verified latest on the feed this pass.

| Package | Roster ver | Latest (feed) | Domain | License |
| --- | --- | --- | --- | --- |
| CSparse | 4.4.0 | 4.4.0 (2026-06) | sparse numerics | LGPL-2.1+ |
| MathNet.Numerics | 6.0.0-beta2 | 6.0.0-beta2 (2025-03) | dense numerics | MIT |
| MathNet.Numerics.Providers.MKL | 6.0.0-beta2 | 6.0.0-beta2 | native LA provider | MIT wrap / Intel ISSL |
| MathNet.Numerics.Providers.OpenBLAS | 6.0.0-beta2 | 6.0.0-beta2 | native LA provider | MIT wrap / BSD-3 |
| ExtendedNumerics.BigRational | 3000.0.2.132 | — | exact rational | MIT |
| PeterO.Numbers | 1.8.2 | 1.8.2 (2021-07) | arbitrary-precision | Unlicense (PD) |
| TYoshimura.DoubleDouble | 5.0.8 | 5.0.8 (2026-05) | 106-bit precision | MIT |
| System.Numerics.Tensors | 10.0.9 | 10.0.9 / 11.0-preview | SIMD + tensor | MIT |
| System.IO.Hashing | 10.0.9 | 10.0.9 | content hash | MIT |
| GShark | 2.3.1 | 2.3.1 (2023-08) | NURBS | MIT |
| LibTessDotNet | 1.1.15 | 1.1.15 (2021-03) | tessellation | SGI-B 2.0 |
| Supercluster.KDTree.Net | 1.0.22 | 1.0.22 (2025-08) | 3D k-NN | MIT |
| Clipper2 | 2.0.0 | 2.0.0 (2025-12) | polygon boolean/offset | Boost-1.0 |
| CavalierContours | 1.0.0 | 1.0.0 (2026-06) | arc polyline offset | MIT |
| SharpVoronoiLib | 1.2.0 | 1.2.0 (2026-02) | Fortune Voronoi | MIT |
| MIConvexHull | 1.1.19.1019 | 1.1.19.1019 (2019-10) | n-D hull/Delaunay/Voronoi | MIT |
| Triangle | 0.0.6-Beta3 | 0.0.6-Beta3 (2016-05) | quality Delaunay mesh | Shewchuk-encumbered |
| geometry3Sharp | 1.0.324 | 1.0.324 (2019-03) | DMesh3 half-edge mesh | Boost-1.0 |
| QuikGraph (substrate) | 2.5.0 | — | graph structures/algos | Ms-PL |
| CommunityToolkit.HighPerformance (substrate) | 8.4.2 | — | SIMD spans/memory | MIT |

## [02]-[DOMAIN_VERDICTS]

Ranked categorical-best per concern. `binding` = the surface the kernel composes.

### SIMD / vectorized math
| Rank | Package | Verdict | Binding | ID / ver |
| --- | --- | --- | --- | --- |
| 1 | System.Numerics.Tensors | KEEP | `TensorPrimitives` SIMD span math (dot/norm/axpy/reduce) | `System.Numerics.Tensors@10.0.9` |
| 1 | CommunityToolkit.HighPerformance | KEEP | `Span2D`/`Memory2D`, SIMD helpers | `@8.4.2` |
| — | Silk.NET.Maths `@2.23.0`, Hexa.NET.Math `@2.0.8` | REJECT | generic `Vector<T>`/`Matrix<T>` SIMD — redundant with the existing tensor substrate + Rhino `Point3d`/`Vector3d`; adding fragments the vector surface | — |

`TensorPrimitives` is the modern .NET SIMD floor; nothing packaged beats it for span-wise geometry reduces. No change.

### Tensor / array substrates
| Rank | Package | Verdict | Binding | ID / ver |
| --- | --- | --- | --- | --- |
| 1 | System.Numerics.Tensors | KEEP | `Tensor<T>`/`TensorSpan<T>` (.NET 9+), the managed dense array substrate | `@10.0.9` |
| — | TorchSharp / libtorch-cpu | REJECT (kernel) | native tensor engine — already sited in `Rasm.Compute`, never the pure-managed kernel | — |

Stay on the 10.x stable line for net10; 11.0 is a .NET-11 preview.

### Computational-geometry primitives
| Rank | Package | Verdict | Binding | ID / ver |
| --- | --- | --- | --- | --- |
| 1 | Clipper2 | KEEP | integer-exact 2D polygon union/intersect/diff/XOR + parallel offset | `Clipper2@2.0.0` |
| 1 | MIConvexHull | KEEP+ | n-D QuickHull + Delaunay/Voronoi (2019 but latest & unmatched managed) | `MIConvexHull@1.1.19.1019` |
| 1 | CavalierContours | KEEP | arc-aware (bulge) polyline offset + closed-polyline boolean | `CavalierContours@1.0.0` |
| 1 | SharpVoronoiLib | KEEP | Fortune point-site Voronoi, edge clip, Delaunay dual, Lloyd | `SharpVoronoiLib@1.2.0` |
| 1 | LibTessDotNet | KEEP | winding-rule polygon fill tessellation (EvenOdd/NonZero/…) | `LibTessDotNet@1.1.15` |
| — | Triangle | **DROP / REPLACE** | Shewchuk-license-encumbered (below); quality-mesh refinement | `Triangle@0.0.6-Beta3` |
| — | geometry3Sharp | **DROP** | archived 2019; see mesh domain | `geometry3Sharp@1.0.324` |
| watch | TVGL | WATCH | MIT CGAL-like C# (hull/slice/voxel/boolean); **NuGet stale 2019** vs active GitHub (1221 commits, 2026-04) — admit only if repackaged, and it carries a C++ leg | `TVGL@1.0.19.213` |

The permissive cluster (Clipper2/MIConvexHull/CavalierContours/SharpVoronoiLib/LibTessDotNet) is categorical-best and stays intact.

### Spatial indexing
| Rank | Package | Verdict | Binding | ID / ver |
| --- | --- | --- | --- | --- |
| 1 | Supercluster.KDTree.Net | KEEP+ | array-backed 3D exact k-NN + radius search | `Supercluster.KDTree.Net@1.0.22` |
| — | hand-rolled SAH-BVH + Morton octree | KEEP | broad-phase `SpatialIndex` (design authors this; no packaged 3D BVH worth vendoring) | — |
| — | NetTopologySuite indexes | REJECT (kernel) | `STRtree`/`KdTree`/`HPRtree`/`Quadtree` are **2D-only** — cannot serve the 3D k-NN need; NTS already sits in the BIM tier for 2D GIS | `NetTopologySuite@2.6.0` |
| — | KdTree `@1.4.1` (2018), GeometRi | REJECT | KdTree stale; GeometRi is tolerance-based, not robust | — |

3D exact k-NN has no better packaged option than Supercluster; the broad-phase stays hand-rolled per design.

### Robust predicates
| Rank | Package | Verdict | Binding | ID / ver |
| --- | --- | --- | --- | --- |
| 1 | hand-rolled `Adaptive.Resolve` ladder | KEEP | `double`→`ddouble`→`Expansion`→`Fraction` + Attene indirect predicates (Orient/InCircle/InSphere + LPI/TPI) | — |
| 1 | TYoshimura.DoubleDouble | KEEP | 106-bit FMA `TwoProduct`/`TwoSum` middle tier | `TYoshimura.DoubleDouble@5.0.8` |
| 1 | ExtendedNumerics.BigRational | KEEP | exact-rational oracle + total ordering key | `@3000.0.2.132` |
| 1 | PeterO.Numbers | KEEP | independent `EFloat` exact adjudicator + directed-rounding brackets | `PeterO.Numbers@1.8.2` |
| — | modios/robust-predicates, govert/RobustGeometry.NET | REJECT | GitHub-only (neither on NuGet); base Shewchuk predicates only — **no indirect-predicate package exists in .NET** | — |

VALIDATION: the exact-predicate hand-roll is categorical-best. No .NET package implements Attene indirect (implicit-point) predicates; the two Shewchuk C# ports are unpackaged and cover only the four base tests. The precision trio is the correct substrate.

### Mesh / half-edge structures
| Rank | Package | Verdict | Binding | ID / ver |
| --- | --- | --- | --- | --- |
| 1 | hand-rolled flat `SimplexStore` / intrinsic mesh | KEEP | the design authors its own half-edge-free flat mesh over exact predicates | — |
| — | geometry3Sharp (DMesh3) | **DROP** | ARCHIVED by gradientspace; NuGet frozen at 1.0.324 (2019); the successor is unpackaged | `geometry3Sharp@1.0.324` |
| watch | geometry4Sharp | WATCH | rms80's own successor fork (BSL-1.0, active 2025–26, DMesh3/Remesher/Reducer/MeshSDG) — **no NuGet yet**; admit on first package | github: JustinKidder/geometry4Sharp |
| — | Plankton | REJECT | n-gonal half-edge but LGPL-3.0 + dead (2017) + Rhino-coupled | github: Dan-Piker/Plankton |
| — | MeshLib | **DISQUALIFIED** | pay-tiered (free non-commercial only, commercial license required) **and** native C++ — fails both the license gate and pure-managed | `MeshLib@3.1.3.249` |

The DMesh3 substrate is dead weight — no compile consumer should depend on it. Drop it; the kernel already authors its mesh substrate. Track geometry4Sharp as the eventual admissible successor.

### Sparse / dense numerics
| Rank | Package | Verdict | Binding | ID / ver |
| --- | --- | --- | --- | --- |
| 1 | CSparse | KEEP | sparse LU/Cholesky/LDL'/QR + AMD ordering, Span solve (categorical-best managed direct) | `CSparse@4.4.0` |
| 1 | MathNet.Numerics | KEEP | dense decompositions, LU/QR, BiCGStab, LOBPCG primitives | `MathNet.Numerics@6.0.0-beta2` |
| — | MathNet.Numerics.Providers.OpenBLAS | KEEP+ | native BLAS accel — **arm64-capable**, opt-in over managed | `@6.0.0-beta2` |
| — | MathNet.Numerics.Providers.MKL | **DROP (osx-arm64)** | Intel MKL is **x86-64 only — cannot load on Apple Silicon**; dead weight on the kernel's stated target (keep only behind an x64-CI condition) | `@6.0.0-beta2` |
| watch | CSparse.Extensions | WATCH | wo80's managed iterative solvers + dense direct factorizations — **GitHub-only, unpackaged** | github: wo80/csparse-extensions |
| — | CSparse.Interop (CHOLMOD/UMFPACK/SuperLU/PARDISO/METIS/ARPACK/Spectra/FEAST) | REJECT (kernel) | categorical-best large-sparse, but **native** — route through `Rasm.Compute`, never the pure-managed kernel | github: wo80/csparse-interop |

CSparse (LGPL-2.1+) is the only copyleft in the kernel; LGPL permits commercial use under dynamic linking, so it clears the gate. The MKL provider is a genuine osx-arm64 defect to rule at conversion.

### Spectral / graph algorithms
| Rank | Package | Verdict | Binding | ID / ver |
| --- | --- | --- | --- | --- |
| 1 | QuikGraph | KEEP | generic graph structures + BFS/DFS/A*/shortest-path/MST/max-flow | `QuikGraph@2.5.0` |
| 1 | hand-rolled DEC / Laplacian / spectral | KEEP | `DiscreteCalculus` d0/d1/stars, `SpectralBasis`, `SpectralFilter`, heat/vector-heat — no package owns discrete-exterior-calculus in .NET | — |
| opt | Kemsekov.GraphSharp | WATCH / optional ADD | MIT advanced graph algos (clique/SCC condensation, adapters for QuikGraph+Satsuma); marginal for the kernel's DEC needs, last publish 2024-02 | `Kemsekov.GraphSharp@3.1.2` |
| — | METIS graph partitioning / ARPACK-Spectra eigensolvers | REJECT (kernel) | categorical-best partition/eigensolve, **native** via CSparse.Interop — route to Compute | — |
| — | GraphSharp `@1.1.0` (2012 WPF), Unchase.Satsuma | REJECT | wrong `GraphSharp` (layout, dead); Satsuma unpackaged under that id | — |

QuikGraph is the packaged managed best; DEC/spectral stays authored. Large-sparse eigensolve + partitioning are a native gap owned by Compute, not the kernel.

## [03]-[ROSTER_DELTA]

Net proposal for the conversion blueprint (kernel-scoped only):

- DROP `geometry3Sharp@1.0.324` — archived/dead DMesh3; no consumer should carry it; kernel authors its own mesh substrate.
- DROP `Triangle@0.0.6-Beta3` — 2016; Shewchuk-license non-commercial encumbrance; the author-kernel Bowyer-Watson CDT covers it. If FEA-grade quality meshing (Ruppert/Chew angle-bounded, Steiner) is required, author it on the existing exact-predicate Delaunay owner rather than admit an encumbered package. `Speckle.Triangle@1.0.0` is the only maintained packaging but inherits the same license ambiguity — not a clean substitute.
- DROP-or-condition `MathNet.Numerics.Providers.MKL@6.0.0-beta2` — Intel MKL has no arm64 build; non-functional on osx-arm64. Keep OpenBLAS as the sole native accel provider (or gate MKL to x64 CI only).
- KEEP everything else — the numerics/exact-precision ladder, the permissive comp-geo cluster, GShark, Supercluster, System.Numerics.Tensors, QuikGraph, CommunityToolkit are each categorical-best or unmatched.
- No net ADD is admissible today. The only forward candidates are WATCH: `geometry4Sharp` (mesh, admit on first NuGet), `CSparse.Extensions` (managed iterative solvers, admit on first NuGet), `TVGL` (comp-geo, admit if repackaged current), `Kemsekov.GraphSharp` (optional graph enrichment).
- Native categorical-bests (CSparse.Interop → CHOLMOD/UMFPACK/METIS/ARPACK/Spectra, TVGL/MeshLib native legs) are explicitly NOT kernel material; they belong to `Rasm.Compute`.

## [04]-[STRUCTURAL_VALIDATION]

The "author from first principles, additively compose only where a package beats a hand-roll" design law survives adversarial ecosystem attack:

- Robust predicates, DEC/spectral, BVH/octree broad-phase, constrained Delaunay, mesh boolean/arrangement, QEM decimation, LSCM/ARAP/BFF, MLESAC fit, LM solver — every one either has **no packaged .NET owner** or only license-encumbered / native / dead options. The hand-roll is not naivety; it is the categorical-best where the ecosystem is empty.
- The additive compositions the design already names (GShark NURBS-MIT, Supercluster k-NN, LibTessDotNet fill) are confirmed the right picks for their concerns.
- The two roster liabilities are legacy admissions predating the design (geometry3Sharp dead, Triangle license-risk) plus the osx-arm64 MKL-provider mismatch — all three cleanly removable without capability loss.
