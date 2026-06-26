# [RASM_API_LIBTESS]

`LibTessDotNet` is the de-facto C# port of SGI's GLU `libtess2` — the WINDING-RULE polygon
tessellator the kernel composes for the messy-winding polygon-FILL concern that the clean-PSLG
Bowyer-Watson Delaunay (`Meshing/delaunay`) CANNOT own: arbitrary self-intersecting, overlapping,
or holey closed contours combined under a winding rule (`EvenOdd`/`NonZero`/`Positive`/`Negative`/
`AbsGeqTwo`) into a triangle/connected-polygon/boundary-contour mesh, with a `CombineCallback`
emitting attributes for the new vertices created at self-intersections. The DRIVER is the `Tess`
class: `AddContour` one or more contours, then `Tessellate(windingRule, elementType, polySize,
combineCallback, normal)`, then read `Vertices`/`Elements`/`VertexCount`/`ElementCount`. The
package ships TWO assemblies in ONE folder — `LibTessDotNet.dll` (`float`-precision `Vec3`,
namespace `LibTessDotNet`) and `LibTessDotNet.Double.dll` (`double`-precision `Vec3`, namespace
`LibTessDotNet.Double`); the double-precision geometry kernel MUST reference the `.Double`
assembly and the `LibTessDotNet.Double` namespace. Pure-managed `netstandard2.0`, zero
dependencies, AnyCPU, with an `IPool` allocation-reuse hook for the repeated-tessellation hot path.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `LibTessDotNet`
- package: `LibTessDotNet`
- version: `1.1.15`
- license: MIT (`speps`; SGI Free Software License B 2.0 origin; `github.com/speps/LibTessDotNet`; nuspec `<license type="expression">MIT</license>`)
- assembly: SHIPS TWO — `LibTessDotNet.dll` (single precision) AND `LibTessDotNet.Double.dll` (double precision), BOTH under `lib/netstandard2.0/`; the kernel references `LibTessDotNet.Double.dll`
- namespace: `LibTessDotNet` (the float assembly) vs `LibTessDotNet.Double` (the double assembly) — the SAME type names (`Tess`/`Vec3`/`ContourVertex`/`WindingRule`/…) live in BOTH namespaces with differing `Vec3` scalar (`float` vs `double`); a `double`-geometry consumer uses `using LibTessDotNet.Double;`
- target: single `lib/netstandard2.0` per assembly — NO multi-target fallback ambiguity; the `net10.0` consumer binds the `netstandard2.0` asset forward. The "two targets" decision is PRECISION (which DLL), not TFM
- asset: pure-managed runtime library, AnyCPU, NO native runtime and ZERO package dependencies; the algorithm is the libtess2 sweep-line with a `Dict`/`PriorityHeap` event structure
- abi: `netstandard2.0` object-model API — `Vec3` is a 3-component value (`X`/`Y`/`Z`, `double` in the `.Double` assembly), `ContourVertex` carries an opaque `object Data` per input vertex, and the tessellation is a coordinate-space operation (project to the tessellation `normal` plane)
- rail: winding-rule polygon-fill triangulation

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the tessellation driver + its input/output/policy vocabulary
- rail: polygon-fill triangulation

`Tess` is the whole consumer surface — the stateful driver accumulating contours and producing
the mesh. `ContourVertex`/`Vec3` are the input vocabulary, the three enums are the policy
discriminants, and `CombineCallback` is the self-intersection attribute hook. The package
EXPOSES a large internal mesh/`Dict`/`PriorityHeap`/`Geom`/`MeshUtils` plumbing as public (a port
artifact) — NONE of it is consumer surface; the kernel touches ONLY the types in this table.

| [INDEX] | [SYMBOL]                       | [PACKAGE_ROLE]                       | [CAPABILITY]                                                                                          |
| :-----: | :----------------------------- | :----------------------------------- | :--------------------------------------------------------------------------------------------------- |
|  [01]   | `Tess`                         | the stateful tessellation driver     | `AddContour(...)` × N then `Tessellate(...)`; output props `Vertices`/`VertexCount`/`Elements`/`ElementCount`/`Normal`; the `NoEmptyPolygons` flag; `new Tess()` / `new Tess(IPool)` |
|  [02]   | `ContourVertex`                | one input contour vertex             | `struct ContourVertex(Vec3 position, object data=null)` — `Position` + an opaque `object Data` carried through to the output (attribute pass-through) |
|  [03]   | `Vec3`                         | a 3-component coordinate             | `struct` with `double X,Y,Z` (in `.Double`); `Zero`; `Sub`/`Neg`/`Dot`/`Normalize`/`LongAxis` static ops — the tessellation works in this coordinate space |
|  [04]   | `WindingRule` (enum)           | the fill rule                        | `EvenOdd` / `NonZero` / `Positive` / `Negative` / `AbsGeqTwo` — how overlapping contour windings combine into "inside" |
|  [05]   | `ElementType` (enum)           | the output topology                  | `Polygons` (triangle/poly fan, `polySize`-bounded) / `ConnectedPolygons` (with adjacency) / `BoundaryContours` (the outline only) |
|  [06]   | `ContourOrientation` (enum)    | per-contour orientation override     | `Original` / `Clockwise` / `CounterClockwise` — force a contour's winding direction on add |
|  [07]   | `CombineCallback` (delegate)   | the self-intersection vertex hook    | `delegate object CombineCallback(Vec3 position, object[] data, double[] weights)` — emit the `Data` for a vertex newly created where contours cross (interpolate the 4 source `data` by `weights`) |
|  [08]   | `IPool` / `DefaultPool` / `NullPool` | allocation-reuse hook          | `IPool` (`Get`/`Return`/`Register`); pass a `DefaultPool` to `new Tess(IPool)` to reuse the mesh-node allocations across repeated tessellations (the hot-path GC-pressure control) |

## [03]-[TESSELLATION_PIPELINE]

[PIPELINE_SCOPE]: add contours → tessellate → read the mesh
- rail: polygon-fill triangulation

The driver is STATEFUL and three-phase: accumulate contours, run one `Tessellate`, read the
flat output arrays. `Tessellate` defaults to `EvenOdd`/`Polygons`/`polySize=3` (triangles) — the
common fill case. The output is FLAT: `Vertices` is the vertex array, `Elements` is the index
array (`polySize` indices per element, `Tess.Undef`=-1 padding for under-full polygons when
`polySize>3`). A vertex's `Data` is the value carried from its `ContourVertex` (or the
`CombineCallback` result for an intersection vertex), so the consumer recovers per-vertex
attributes by index.

| [INDEX] | [SURFACE]                                                                  | [CALL_SHAPE]   | [CAPABILITY]                                                          |
| :-----: | :------------------------------------------------------------------------- | :------------- | :------------------------------------------------------------------- |
|  [01]   | `Tess.AddContour(ContourVertex[] vertices, ContourOrientation forceOrientation=ContourOrientation.Original)` | instance | accumulate one closed contour (outer boundary or hole); call once PER contour |
|  [02]   | `Tess.AddContour(IList<ContourVertex> vertices, ContourOrientation forceOrientation=Original)` | instance | the `IList` overload — accumulate a contour from any list |
|  [03]   | `Tess.Tessellate(WindingRule windingRule=EvenOdd, ElementType elementType=Polygons, int polySize=3, CombineCallback combineCallback=null, Vec3 normal=default)` | instance | run the sweep — combine the accumulated contours under `windingRule` into `elementType` output with ≤`polySize`-gon faces; `normal` sets the projection plane (default = auto-detect) |
|  [04]   | `Tess.Vertices` (`ContourVertex[]`) / `Tess.VertexCount` (`int`)          | instance       | the output vertex array (each carrying its `Data`) + its count        |
|  [05]   | `Tess.Elements` (`int[]`) / `Tess.ElementCount` (`int`)                   | instance       | the flat index array (`polySize` indices/element, `Tess.Undef`=-1 pad) + the element count |
|  [06]   | `Tess.Normal` (`Vec3`) / `Tess.NoEmptyPolygons` (`bool` field)           | instance       | the computed/forced tessellation-plane normal; suppress zero-area output polygons when set |

## [04]-[IMPLEMENTATION_LAW]

[VALUE_PROFILE]:
- representation: `Tess` is a STATEFUL driver — contours accumulate across `AddContour` calls and one `Tessellate` consumes them; the output is the flat `Vertices`/`Elements` index-mesh, NOT an object graph. `Vec3` is a 3-component coordinate value (`double X,Y,Z` in the `.Double` assembly). `ContourVertex` pairs a `Vec3 Position` with an opaque `object Data` carried verbatim to the output.
- precision dichotomy: the package ships `LibTessDotNet.dll` (`float` `Vec3`) and `LibTessDotNet.Double.dll` (`double` `Vec3`) as SEPARATE assemblies in the SAME `netstandard2.0` folder, with identical type names in the `LibTessDotNet` vs `LibTessDotNet.Double` namespaces. This is the ONLY consequential variant — a double-precision geometry kernel binds the `.Double` assembly; a `using LibTessDotNet;` against the float assembly silently halves precision.
- output shape: `Elements` packs `polySize` indices per element with `Tess.Undef`(=-1) padding when a polygon has fewer than `polySize` vertices (relevant only for `polySize>3`); at the default `polySize=3` every element is a full triangle. `BoundaryContours` returns the OUTLINE (start-index + count per contour), not filled faces.
- combine semantics: where input contours self-intersect or overlap, the tessellator creates a NEW vertex at the crossing and invokes `CombineCallback(position, object[] data, double[] weights)` — the consumer interpolates the (up to four) source `data` values by `weights` to produce the new vertex's `Data`; returning `null` is valid when no attribute carry is needed. The README/manifest name `VertexCombine` is WRONG — the delegate is `CombineCallback`.

[LOCAL_ADMISSION]:
- LibTessDotNet owns the MESSY-WINDING polygon-FILL triangulation: self-intersecting/overlapping/holey closed contours resolved by a winding rule into a filled mesh. This is DISTINCT from the kernel's clean-PSLG Bowyer-Watson Delaunay (`Meshing/delaunay`), which requires a non-self-intersecting planar straight-line graph and produces a quality Delaunay triangulation — LibTess produces a FILL (not a Delaunay) of arbitrary winding.
- it backs the `Drawing/view` + `Drawing/pack` FILL leg — turning a GShark `PolyLine`/`Polygon` boundary (or a nested outer+hole contour set) into renderable/packable triangles. The kernel feeds `ContourVertex[]` (mapping `Rasm.Vectors` → `Vec3` at the boundary), carries its own per-vertex index/payload as `ContourVertex.Data`, and recovers it from the output `Vertices[i].Data`.
- ALWAYS reference `LibTessDotNet.Double` (the `double` assembly) — the geometry kernel is double-precision; the `float` assembly is for memory-bound graphics, not CAD geometry.
- for the repeated-tessellation hot path (per-frame fill, many small polygons), construct `new Tess(new DefaultPool())` ONCE and reuse it across `Reset`-bounded tessellations to amortize the mesh-node allocations rather than allocating a fresh `Tess` per call.

[STACKING_LAW]:
- vs the kernel Delaunay (`Meshing/delaunay`, kernel-authored): the two are DISJOINT triangulation owners — LibTess fills arbitrary-winding contours (no quality guarantee, handles self-intersection), the Bowyer-Watson Delaunay meshes a clean PSLG with a quality/angle guarantee. A boundary that is guaranteed simple + needs quality → Delaunay; a boundary that may self-intersect/overlap/have holes + needs a fill → LibTess. Never feed a self-intersecting contour to the Delaunay mesher nor expect a quality mesh from LibTess.
- vs GShark (`api-gshark`): GShark evaluates the boundary curve (`NurbsBase` → `Divide`/`AdaptiveSample` to a `Point3` polyline); LibTess FILLS that polyline. The stack is evaluate→sample→tessellate: GShark `PolyLine`/`Polygon` boundary → sampled `Point3[]` → `ContourVertex[]` → `Tess.Tessellate` → index mesh. GShark never fills; LibTess never evaluates curves.
- vs Clipper2 (`api-clipper2`): Clipper2 owns 2D polygon BOOLEAN/OFFSET (the exact integer-coordinate clip producing new polygon boundaries) and ALSO ships an ear-clip triangulator (`Clipper.Triangulate(Paths64, out Paths64, useDelaunay=true)` → `TriangulateResult`). The stack is clip/offset (Clipper2) → tessellate: boolean the regions first, then fill the result. Clipper2's `FillRule` (`EvenOdd`/`NonZero`/`Positive`/`Negative`) maps directly onto LibTess's `WindingRule`. The triangulation-owner split is by INPUT and rule: `Clipper.Triangulate` is the integer-exact ear-clip with optional Delaunay edge-flip on an ALREADY-RESOLVED simple `Paths64` (it REJECTS a self-intersecting input as `pathsIntersect`); LibTess is the float winding-rule COMBINE that resolves arbitrary self-intersecting/overlapping/holey contours in one pass (no quality, no exactness); `Triangle` (`api-triangle`) is the constrained-DELAUNAY quality mesher on a clean PSLG. Messy self-intersecting input → LibTess; integer-exact ear-clip of a clean boolean result → `Clipper.Triangulate`; FEM-grade angle/area-bounded mesh → `Triangle`.
- vs Triangle (`api-triangle`): `Triangle.NET` is a 2D constrained-DELAUNAY quality MESHER on a clean planar straight-line graph (angle/area bounds, conforming Delaunay, Steiner refinement) — a non-self-intersecting boundary needing a FEM-grade mesh; LibTess is the winding-rule FILL that handles self-intersecting/overlapping input with NO quality guarantee. They are the messy-fill vs clean-quality-mesh legs of the polygon-triangulation seam (the third owner is `Clipper.Triangulate`'s integer ear-clip). A guaranteed-simple boundary needing quality → Triangle; an arbitrary-winding boundary needing a fill → LibTess; never feed a self-intersecting contour to Triangle nor expect an angle-bounded mesh from LibTess.
- vs MIConvexHull (`api-miconvexhull`): MIConvexHull's `Triangulation.CreateDelaunay` is a POINT-cloud Delaunay (connectivity of scattered points); LibTess is a CONTOUR fill (winding of closed boundaries). Different inputs (points vs contours) — not interchangeable.

[RAIL_LAW]:
- Package: `LibTessDotNet` (the geometry kernel binds assembly `LibTessDotNet.Double.dll`, namespace `LibTessDotNet.Double`)
- Owns: the winding-rule polygon-fill tessellator — the `Tess` stateful driver (`AddContour(ContourVertex[]/IList, ContourOrientation)` accumulation then `Tessellate(WindingRule, ElementType, polySize, CombineCallback, Vec3 normal)`), the flat `Vertices`/`VertexCount`/`Elements`/`ElementCount` index-mesh output, the `WindingRule`/`ElementType`/`ContourOrientation` policy enums, the `ContourVertex`/`Vec3` input vocabulary with per-vertex `object Data` pass-through, the `CombineCallback` self-intersection attribute hook, and the `IPool`/`DefaultPool` allocation-reuse hot-path control.
- Accept: filling arbitrary self-intersecting/overlapping/holey closed contours under a winding rule into a triangle/connected-polygon/boundary mesh; a GShark or Clipper2 polygon boundary sampled to `ContourVertex[]` (mapping `Rasm.Vectors` → `Vec3` at the boundary) with the `Rasm` payload carried as `ContourVertex.Data`; the `.Double` assembly for double-precision geometry; a pooled `Tess` for the repeated-fill hot path.
- Reject: referencing `LibTessDotNet.dll` / `using LibTessDotNet;` (the `float` assembly) in the double-precision geometry kernel — bind `LibTessDotNet.Double`; calling the self-intersection hook `VertexCombine` (it is `CombineCallback`); feeding a self-intersecting contour to the kernel's clean-PSLG Delaunay mesher or expecting a quality/angle-bounded mesh from LibTess (it is a FILL, not a Delaunay); using LibTess where the input is an already-simple `Paths64` wanting the integer-exact ear-clip (that is `Clipper.Triangulate`, `api-clipper2`) or where an angle/area-bounded FEM mesh is needed (that is `Triangle`, `api-triangle`) — LibTess is the messy-WINDING fill leg of the three-owner polygon-triangulation seam; touching the port's public-but-internal mesh/`Dict`/`PriorityHeap`/`Geom`/`MeshUtils` plumbing as if it were consumer API; tessellating a point cloud (that is MIConvexHull's Delaunay — LibTess fills CONTOURS).
