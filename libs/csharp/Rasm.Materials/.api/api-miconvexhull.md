# [RASM_MATERIALS_API_MICONVEXHULL]

`MIConvexHull` is the DesignEngrLab MIT-licensed, pure-managed AnyCPU computational-geometry primitive suite: the dimension-generic incremental convex-hull algorithm (the same quickhull-family beneath-beyond machinery serving any dimension N>=2), its dedicated 2D specialization, the `Triangulation.CreateDelaunay` Delaunay lift of the hull-on-the-paraboloid, and the `VoronoiMesh` dual-graph builder — all over the open `IVertex`/`IVertex2D` vertex contracts so a consumer's OWN vertex/face/cell types ride the algorithm with zero copy. Every entry returns a `ConvexHullCreationResult` carrying a `ConvexHullCreationResultOutcome` discriminant plus an `ErrorMessage`, so feasibility is a TYPED result, not an exception. The package is OWNED by `Rasm` (the geometry KERNEL): `libs/csharp/Rasm/Rasm.csproj` holds the `PackageReference`, and the kernel design pages (`Geometry/.planning/Meshing/delaunay.md`, `Numerics/predicates.md`, `Drawing/`) compose it. `Rasm.Materials` consumes it TRANSITIVELY through `VividOrange.InteractionDiagram`: the N-M-M biaxial column-capacity engine meshes the concrete+rebar section, sweeps strain planes, integrates fibre stress, and `MIConvexHull` assembles the resulting force-moment-moment vertex cloud into the closed 3D capacity onion hull (`IForceMomentMesh`). The Materials folder mints NO direct `MIConvexHull` call of its own — this catalog documents the surface the transitive `VividOrange` capacity engine drives and the kernel owner authors against, so the Materials design never re-mints a hull/Delaunay/Voronoi primitive at the AEC-DOMAIN stratum.

The kernel relationship is COMPOSITIONAL, not a replacement: `Rasm`'s `Geometry/.planning/Meshing/delaunay.md` authors its OWN exact-predicate Bowyer-Watson `Tessellation` (constrained Delaunay over a flat `SimplexStore`, robust to degenerate input via the four-tier `Adaptive.Resolve` precision ladder). `MIConvexHull` is the FLOAT-domain `double`-tolerance fast path and the convex-hull/Voronoi-dual primitive the authored constrained mesher does not own — the two are layered (the authored kernel for robust constrained meshing and arrangement; `MIConvexHull` for the unconstrained hull, the unconstrained Delaunay lift, and the Voronoi dual), never duplicated. Distinct from the in-folder `Triangle` (Triangle.NET — constrained/conforming PSLG Delaunay refinement, `Aliases="TriangleNet"`) and the sibling-folder `SharpVoronoiLib` (Fortune sweep, 2D point-site only): `MIConvexHull` is the only owner of the ND convex hull and the only Delaunay/Voronoi path that lifts through the N-D hull.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `MIConvexHull`
- package: `MIConvexHull`
- version: `1.1.19.1019` (centrally pinned)
- license: `MIT` (DesignEngrLab; `licenseUrl` `github.com/DesignEngrLab/MIConvexHull/blob/master/LICENSE.txt`)
- assembly: `MIConvexHull`
- namespace: `MIConvexHull` (single namespace — the entire surface is flat under it)
- asset: pure-managed AnyCPU IL (no native asset, no RID burden, ALC-safe, osx-arm64-safe)
- frameworks: multi-target `netstandard2.0` + `netstandard1.0` (two `lib/` assets); the `net10.0` consumer binds `lib/netstandard2.0/MIConvexHull.dll` by TFM precedence — the README/manifest "netstandard1.0" note is the NUSPEC dependency-group floor, NOT the bound asset. The two assets carry the identical public surface, so the `[API_TFM_RESOLUTION]` hazard does not change any signature here, but the bound TFM is `netstandard2.0`
- dependencies: NONE on `netstandard2.0` (empty dependency group — zero transitive closure)
- owner: `libs/csharp/Rasm/Rasm.csproj` (geometry KERNEL); `Rasm.Materials` reaches it transitively via `VividOrange.InteractionDiagram`
- rail: materials (transitive — direct calls live in the `Rasm` kernel)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: vertex and face contracts — the open algorithm input/output interfaces
- rail: kernel-substrate

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]      | [CAPABILITY]                                                                                  |
| :-----: | :------------------------ | :----------------- | :------------------------------------------------------------------------------------------- |
|  [01]   | `IVertex`                 | input interface    | the ND vertex contract — one member `double[] Position { get; }`; a consumer's own vertex type implements this to ride the N-D hull/Delaunay/Voronoi |
|  [02]   | `IVertex2D`               | input interface    | the 2D specialization contract — `double X { get; }` / `double Y { get; }`; rides the faster `Create2D` planar hull (no `double[]` allocation per point) |
|  [03]   | `DefaultVertex`           | default vertex     | `IVertex` impl with a settable `double[] Position` — the zero-ceremony ND vertex when the consumer has no own type |
|  [04]   | `DefaultVertex2D`         | default 2D vertex  | readonly `struct` `IVertex2D` impl, `double X`/`Y`, ctors `(double[])` and `(double, double)` — the zero-alloc planar vertex |
|  [05]   | `ConvexFace<TVertex,TFace>` | abstract face    | the hull-facet base: `TVertex[] Vertices`, `TFace[] Adjacency` (neighbour facets across each edge), `double[] Normal` (outward facet normal) — CRTP `TFace` self-type so adjacency is strongly typed |
|  [06]   | `DefaultConvexFace<TVertex>` | default face    | concrete `ConvexFace<TVertex, DefaultConvexFace<TVertex>>` — the zero-ceremony facet when the consumer carries no per-facet payload |

[PUBLIC_TYPE_SCOPE]: result carriers and the feasibility discriminant
- rail: kernel-substrate
- note: EVERY hull/Delaunay/Voronoi entry returns a result carrier rather than throwing — `Outcome` is the typed feasibility discriminant the consumer matches on; `ConvexHullGenerationException` is internal-to-algorithm and surfaced ONLY as a folded `Outcome`, never escaping the public `Create` surface.

| [INDEX] | [SYMBOL]                              | [TYPE_FAMILY]      | [CAPABILITY]                                                                            |
| :-----: | :------------------------------------ | :----------------- | :------------------------------------------------------------------------------------- |
|  [01]   | `ConvexHullCreationResult<TVertex,TFace>` | ND result      | `ConvexHull<TVertex,TFace> Result`, `ConvexHullCreationResultOutcome Outcome`, `string ErrorMessage` — the ND-hull return |
|  [02]   | `ConvexHullCreationResult<TVertex>`   | 2D result          | `IList<TVertex> Result` (the ordered hull-boundary vertices), `Outcome`, `ErrorMessage` — the 2D-hull return (vertices, NOT faces) |
|  [03]   | `ConvexHullCreationResultOutcome`     | feasibility enum   | `Success`, `DimensionSmallerTwo`, `DimensionTwoWrongMethod`, `NotEnoughVerticesForDimension`, `NonUniformDimension`, `DegenerateData`, `UnknownError` — the seven-case discriminant the consumer folds to a typed domain result |
|  [04]   | `ConvexHullGenerationException`       | exception          | `: Exception` carrying `Error` (an `Outcome`) + `ErrorMessage`; thrown INSIDE the algorithm and CAUGHT by `Create`, which folds it into the result — it does NOT escape the public surface |

[PUBLIC_TYPE_SCOPE]: hull / triangulation / voronoi result graphs
- rail: kernel-substrate

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY]      | [CAPABILITY]                                                                            |
| :-----: | :-------------------------------- | :----------------- | :------------------------------------------------------------------------------------- |
|  [01]   | `ConvexHull<TVertex,TFace>`       | ND hull graph      | `IEnumerable<TVertex> Points` (hull-boundary vertices) + `IEnumerable<TFace> Faces` (the facet set, each with `Vertices`/`Adjacency`/`Normal`) — the full ND hull boundary representation |
|  [02]   | `ConvexHull<TVertex>`             | 2D hull graph      | `IEnumerable<TVertex> Points` — the ordered 2D hull boundary (no facet graph; a polygon is its boundary cycle) |
|  [03]   | `ITriangulation<TVertex,TCell>`   | triangulation contract | `IEnumerable<TCell> Cells` — the Delaunay simplex set (triangles in 2D, tetrahedra in 3D, N-simplices in N-D) |
|  [04]   | `DelaunayTriangulation<TVertex,TCell>` | triangulation impl | concrete `ITriangulation` with `IEnumerable<TCell> Cells` + static `Create(IList<TVertex>, double)` |
|  [05]   | `TriangulationCell<TVertex,TCell>` | abstract cell     | `: ConvexFace<TVertex,TCell>` — a Delaunay simplex IS a facet of the lifted hull; inherits `Vertices`/`Adjacency`/`Normal` |
|  [06]   | `DefaultTriangulationCell<TVertex>` | default cell      | concrete `TriangulationCell<TVertex, DefaultTriangulationCell<TVertex>>` — the zero-ceremony Delaunay simplex |
|  [07]   | `VoronoiMesh<TVertex,TCell,TEdge>` | voronoi graph     | `IEnumerable<TCell> Vertices` (the Voronoi VERTICES are the Delaunay CELLS — circumcentres) + `IEnumerable<TEdge> Edges` (the dual graph) |
|  [08]   | `VoronoiEdge<TVertex,TCell>`      | voronoi edge       | `TCell Source` / `TCell Target` — a dual edge connecting two adjacent Delaunay-cell circumcentres; `Equals`/`GetHashCode` for dedup |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: convex hull — `ConvexHull` static facade
- rail: kernel-substrate
- note: the `data == null` guard, the dimension/uniformity checks, and the degenerate-input branch all fold into the returned `Outcome` — no overload throws to the caller. `tolerance` defaults to `1E-10` (the planar-distance / coplanarity epsilon).

| [INDEX] | [SURFACE]                                                                              | [ENTRY_FAMILY] | [CAPABILITY]                                                                 |
| :-----: | :------------------------------------------------------------------------------------- | :------------- | :--------------------------------------------------------------------------- |
|  [01]   | `ConvexHull.Create<TVertex,TFace>(IList<TVertex> data, double tolerance = 1E-10)`      | ND hull (full) | the full strongly-typed ND hull with a custom facet payload `TFace`          |
|  [02]   | `ConvexHull.Create<TVertex>(IList<TVertex> data, double tolerance = 1E-10)`            | ND hull (default face) | the ND hull with `DefaultConvexFace<TVertex>` facets — the common ND entry |
|  [03]   | `ConvexHull.Create(IList<double[]> data, double tolerance = 1E-10)`                    | ND hull (raw)  | the rawest entry: `double[]` coordinate arrays in, `DefaultVertex` hull out  |
|  [04]   | `ConvexHull.Create2D<TVertex>(IList<TVertex> data, double tolerance = 1E-10)`          | 2D hull (typed)| the planar hull over `IVertex2D` — returns the ordered boundary vertex list  |
|  [05]   | `ConvexHull.Create2D(IList<double[]> data, double tolerance = 1E-10)`                  | 2D hull (raw)  | the planar hull over raw `double[]` pairs — `DefaultVertex2D` boundary out    |

[ENTRYPOINT_SCOPE]: Delaunay triangulation — `Triangulation` static facade
- rail: kernel-substrate
- note: the Delaunay triangulation is computed as the lower convex hull of the points lifted onto the paraboloid `z = x^2 + y^2` (N-D generalization); the returned `Cells` are the projected simplices. `PlaneDistanceTolerance` defaults to `1E-10`.

| [INDEX] | [SURFACE]                                                                                          | [ENTRY_FAMILY] | [CAPABILITY]                                                                 |
| :-----: | :------------------------------------------------------------------------------------------------- | :------------- | :--------------------------------------------------------------------------- |
|  [01]   | `Triangulation.CreateDelaunay<TVertex,TFace>(IList<TVertex> data, double PlaneDistanceTolerance = 1E-10)` | delaunay (full) | strongly-typed Delaunay with a custom cell payload `TFace`             |
|  [02]   | `Triangulation.CreateDelaunay<TVertex>(IList<TVertex> data, double PlaneDistanceTolerance = 1E-10)` | delaunay (default) | Delaunay with `DefaultTriangulationCell<TVertex>` cells — the common entry |
|  [03]   | `Triangulation.CreateDelaunay(IList<double[]> data, double PlaneDistanceTolerance = 1E-10)`        | delaunay (raw) | raw `double[]` coordinate Delaunay — `DefaultVertex` cells out                |

[ENTRYPOINT_SCOPE]: Voronoi diagram — `Triangulation.CreateVoronoi` and `VoronoiMesh` static facades (dual entries)
- rail: kernel-substrate
- note: the Voronoi diagram is the geometric DUAL of the Delaunay triangulation — `VoronoiMesh.Vertices` ARE the Delaunay cells (their circumcentres are the Voronoi vertices), `VoronoiMesh.Edges` connect adjacent cells. `Triangulation.CreateVoronoi` and `VoronoiMesh.Create` are MIRROR facades with identical overload shapes.

| [INDEX] | [SURFACE]                                                                                                            | [ENTRY_FAMILY] | [CAPABILITY]                                                       |
| :-----: | :----------------------------------------------------------------------------------------------------------------- | :------------- | :---------------------------------------------------------------- |
|  [01]   | `Triangulation.CreateVoronoi<TVertex,TCell,TEdge>(IList<TVertex> data, double PlaneDistanceTolerance = 1E-10)`      | voronoi (full) | full custom cell+edge Voronoi mesh                                |
|  [02]   | `Triangulation.CreateVoronoi<TVertex,TCell>(IList<TVertex>, double)`                                                | voronoi        | custom cell, default `VoronoiEdge` dual edges                     |
|  [03]   | `Triangulation.CreateVoronoi<TVertex>(IList<TVertex>, double)`                                                      | voronoi (default) | default cell + default edge Voronoi mesh                       |
|  [04]   | `Triangulation.CreateVoronoi(IList<double[]>, double)`                                                              | voronoi (raw)  | raw `double[]` Voronoi over `DefaultVertex`                        |
|  [05]   | `VoronoiMesh.Create<TVertex,TCell,TEdge>(IList<TVertex>, double)` (+ the same `<TVertex,TCell>` / `<TVertex>` / raw overloads) | voronoi (mirror) | the `VoronoiMesh`-rooted twin of the four `CreateVoronoi` entries |

## [04]-[IMPLEMENTATION_LAW]

[ALGORITHM_TOPOLOGY]:
- The hull is the beneath-beyond / quickhull family generalized to N dimensions; the SAME algorithm core serves every `Create<...>` overload, discriminating only on dimension (read from `Position.Length`) and the 2D fast-path split. `Create2D` is a SEPARATE planar algorithm (`ConvexHull2DAlgorithm`), NOT the ND core at dimension 2 — calling `Create` (ND) on 2D data returns `Outcome.DimensionTwoWrongMethod`, and calling `Create2D` on >2D data is undefined; route planar input through `Create2D`/`CreateDelaunay` and ND input through `Create`.
- Delaunay = lower convex hull on the lifting paraboloid; Voronoi = the Delaunay dual. All three share the ONE hull core, so a consumer needing both the Delaunay mesh and its Voronoi dual computes the hull once conceptually but calls the two facades separately (no shared-state reuse handle is exposed — the cost is the second lift).
- `tolerance`/`PlaneDistanceTolerance` (`1E-10` default) is the coplanarity/cospherical epsilon in `double`: points within it of a facet plane are treated as ON it. This is a FLOATING-POINT robustness knob, NOT exact arithmetic — near-degenerate input (cocircular/cospherical clusters, collinear runs) can return `Outcome.DegenerateData` or a topologically-imperfect mesh. For EXACT-predicate robustness the consumer routes through the `Rasm` kernel's `Geometry/.planning/Meshing/delaunay.md` four-tier `Adaptive.Resolve` Bowyer-Watson instead; `MIConvexHull` is the fast `double` path where the input is known well-conditioned.

[RESULT_FOLD]:
- EVERY entry returns a `ConvexHullCreationResult*` — match on `Outcome` and read `Result` only on `Success`. The kernel owner folds the seven-case `ConvexHullCreationResultOutcome` to its typed `Fin`/`Validation` geometry-failure rail: `Success` -> the hull/mesh; `NotEnoughVerticesForDimension`/`DegenerateData`/`NonUniformDimension`/`DimensionSmallerTwo`/`DimensionTwoWrongMethod` -> distinct typed failure cases (each carries the `ErrorMessage` for the diagnostic); `UnknownError` -> the catch-all with the inner exception message. The internal `ConvexHullGenerationException` is NEVER caught at the call site — it is already folded into `Outcome`/`ErrorMessage` by `Create`.
- The ND result exposes BOTH `Points` (boundary vertices) and `Faces` (the facet graph with `Adjacency`/`Normal`); the 2D result exposes ONLY the ordered boundary `Result` vertex list (a 2D hull has no facet graph — its boundary is the polygon cycle). Consumers reading facet normals/adjacency MUST use the ND `Create`/`Create<TVertex,TFace>` path even for embedded-3D planar data, never `Create2D`.

[STACKING_LAW]:
- VividOrange.InteractionDiagram (the Materials transitive consumer): the N-M-M capacity engine produces a cloud of force-moment-moment points (one per integrated strain plane); it feeds them as `IVertex` (`double[] {N, My, Mz}`) to `ConvexHull.Create` and reads `Faces`/`Points` to weld the closed 3D capacity ONION (`IForceMomentMesh`/`IForceMomentVertex`/`IForceMomentTriFace`). The `Faces[].Normal` is the per-facet capacity-surface gradient. This is the ONLY `MIConvexHull` consumption the `Rasm.Materials` design composes — the AEC-DOMAIN folder NEVER calls `MIConvexHull` directly; it consumes the welded `IForceMomentMesh` the capacity engine returns.
- `Rasm` kernel (the direct owner): `MIConvexHull` is the unconstrained-hull + Voronoi-dual primitive layered BESIDE the authored exact `Tessellation` (constrained Delaunay) and `Arrangement` (mesh boolean). The kernel `Drawing/` fill leg and the unconstrained-point-set Delaunay/Voronoi concerns compose `MIConvexHull`; the constrained-PSLG path stays on the authored Bowyer-Watson + `Triangle` (Triangle.NET). A consumer's own kernel vertex (carrying an exact-coordinate payload) implements `IVertex` so the SAME point type rides both the `double` `MIConvexHull` fast path and the exact authored mesher.
- Disjoint from `Triangle` and `SharpVoronoiLib`: `Triangle` (Triangle.NET) owns CONSTRAINED/conforming Delaunay over a PSLG (segments + holes + quality refinement) — `MIConvexHull` owns only the UNCONSTRAINED point-set Delaunay. `SharpVoronoiLib` owns the 2D point-site Fortune Voronoi with border clipping + Lloyd relaxation — `MIConvexHull` owns the N-D Voronoi-as-Delaunay-dual with no border clipping. A consumer needing a bounded/clipped 2D Voronoi uses `SharpVoronoiLib`; one needing the N-D dual or the hull-lift Delaunay uses `MIConvexHull`.

[RAIL_LAW]:
- Package: `MIConvexHull` (assembly `MIConvexHull`)
- Owns: the dimension-generic convex hull (`Create`/`Create2D`), the hull-lift unconstrained Delaunay (`Triangulation.CreateDelaunay`), and the Delaunay-dual Voronoi mesh (`CreateVoronoi`/`VoronoiMesh.Create`), all over the open `IVertex`/`IVertex2D` contracts returning typed `ConvexHullCreationResult` carriers in the `double` domain at a coplanarity tolerance
- Accept: the kernel's unconstrained-hull / Voronoi-dual / fast-`double`-Delaunay concern with a consumer-owned vertex type implementing `IVertex`/`IVertex2D`; the transitive `VividOrange.InteractionDiagram` N-M-M capacity-onion hull build
- Reject: any direct `MIConvexHull` call minted in the `Rasm.Materials` AEC-DOMAIN folder (it consumes the welded `IForceMomentMesh`, never the raw hull); using `MIConvexHull` where the input is near-degenerate and exact robustness is required (route to the kernel `Adaptive.Resolve` Bowyer-Watson); duplicating the constrained-PSLG Delaunay (`Triangle` owns it) or the clipped 2D point-site Voronoi (`SharpVoronoiLib` owns it); calling the ND `Create` on planar data (returns `DimensionTwoWrongMethod` — use `Create2D`); and catching `ConvexHullGenerationException` at a call site (it never escapes — match `Outcome` instead)
