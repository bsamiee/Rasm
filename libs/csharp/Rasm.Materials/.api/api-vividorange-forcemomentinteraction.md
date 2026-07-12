# [RASM_MATERIALS_API_VIVIDORANGE_FORCEMOMENTINTERACTION]

`VividOrange.ForceMomentInteraction` is verified-transitive through direct package `VividOrange.InteractionDiagram` and is the concrete half of the column biaxial capacity hull — the `ForceMomentMesh` / `ForceMomentVertex` / `ForceMomentTriFace` classes that implement the `api-vividorange-iforcemomentinteraction.md` floor and that the `InteractionDiagram` engine (`api-vividorange-interactiondiagram.md`) CONSTRUCTS as its output. A `ForceMomentVertex` is one feasible (axial-`Force` N, moment-`Torque` My, moment-`Torque` Mz) point on a column's capacity boundary; a `ForceMomentMesh` is the closed N-M-M ONION assembled from those points with a full mesh-topology kernel (unique-edge extraction, outer-edge / outline tracing, face-direction reversal) so the capacity surface is a renderable, editable `VividOrange.Geometry` `ICartesianMesh`. This package owns the mesh DATA + topology algebra; the strain-plane fibre-integration that PRODUCES the vertices lives in `InteractionDiagram`, and the read contract a consumer holds is the interface floor. The mesh's `Force`/`Torque` fields round-trip through the `VividOrange.Serialization` `ITaxonomySerializable` JSON pipeline (`api-vividorange-serialization.md`) over the `UnitsNet` Json.NET converters (`api-vividorange-serialization.md` [TRANSITIVE_UNITSNET_JSONNET]).

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `VividOrange.ForceMomentInteraction`

- package: `VividOrange.ForceMomentInteraction`
- license: MIT (`licenses.nuget.org/MIT` — MagmaWorks / VividOrange taxonomy)
- assembly: `VividOrange.ForceMomentInteraction`
- namespace: `VividOrange.ForceMomentInteraction`
- asset: runtime library, pure-managed AnyCPU, NO native RID asset. Multi-TFM `net8.0` / `net7.0` / `net6.0` /
  `net48`; the consumer `net10.0` binds the highest managed asset `lib/net8.0` (`net8.0` is the bound TFM — no
  `net9.0`+ asset, so the `api resolve` primary `net8.0` is the consumed surface).
- rail: profiles (RC/steel column capacity surface — concrete implementation)
- ABI floor: a `0.2.x` PRE-1.0 contract. The interface floor is `VividOrange.IForceMomentInteraction`; the
  geometry floor (`ICartesianMesh`/`ICartesianVertex`/`ICartesianTriFace`/`ICartesian3d`/`ILocalCartesian2d`/
  `ICoordinate`/`IBrush`/`Coordinate`/`Brush`) is `VividOrange.ICartesianBase`; `Force`/`Torque`/`Ratio` are
  `UnitsNet`; `ITaxonomySerializable` is the `VividOrange.ISerialization` floor — all centrally pinned
  (central transitive pinning forces explicit rows).

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the N-M-M capacity-hull concrete; every class implements its matching `api-vividorange-iforcemomentinteraction.md` floor. `ForceMomentMesh` carries vertices, triangular faces, index topology, render brush, and the topology kernel. `ForceMomentVertex.X`, `.Y`, and `.Z` carry `Force`, `Torque`, and `Torque`; `TextureCoordinate` carries `ICoordinate`. `ForceMomentTriFace` binds A/B/C and lazily computes its mean `Center` and Heron `Ratio` `Area`.

- rail: profiles

| [INDEX] | [SYMBOL]             | [PACKAGE_ROLE] | [CAPABILITY]                     |
| :-----: | :------------------- | :------------- | :------------------------------- |
|  [01]   | `ForceMomentMesh`    | mutable hull   | closed N-M-M capacity onion      |
|  [02]   | `ForceMomentVertex`  | hull vertex    | capacity point with texture      |
|  [03]   | `ForceMomentTriFace` | hull facet     | lazy centroid and meshing weight |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `ForceMomentVertex` construction — the unit-typed (N, My, Mz) capacity point

- rail: profiles
- normalization law: the `(Force x, Torque y, Torque z)` ctors RE-EXPRESS `z` into `y.Unit` so My and Mz share one
  torque unit; the `(double, double, double, ForceUnit, TorqueUnit)` ctor is the raw-scalar form the engine uses
  (`new ForceMomentVertex(N, My, Mz, ForceUnit.Kilonewton, TorqueUnit.KilonewtonMeter)`).
- property law: `X`, `Y`, and `Z` are settable capacity coordinates, and `TextureCoordinate` is the render coordinate.

| [INDEX] | [SURFACE]                                                                    | [CALL_SHAPE] | [CAPABILITY]                 |
| :-----: | :--------------------------------------------------------------------------- | :----------- | :--------------------------- |
|  [01]   | `new ForceMomentVertex()`                                                    | constructor  | zero `Force`/`Torque` point  |
|  [02]   | `new ForceMomentVertex(Force x, Torque y, Torque z)`                         | constructor  | unit-normalized typed point  |
|  [03]   | `new ForceMomentVertex(Force x, Torque y, Torque z, ICoordinate)`            | constructor  | typed point with texture     |
|  [04]   | `new ForceMomentVertex(double x, double y, double z, ForceUnit, TorqueUnit)` | constructor  | engine scalar-and-unit form  |
|  [05]   | `ForceMomentVertex.X`                                                        | property     | settable `Force` coordinate  |
|  [06]   | `ForceMomentVertex.Y`                                                        | property     | settable `Torque` coordinate |
|  [07]   | `ForceMomentVertex.Z`                                                        | property     | settable `Torque` coordinate |
|  [08]   | `ForceMomentVertex.TextureCoordinate`                                        | property     | render texture `ICoordinate` |

[ENTRYPOINT_SCOPE]: `ForceMomentMesh` construction + the topology kernel

- rail: profiles
- note: the engine constructs `new ForceMomentMesh(vertices, faces)` from the hull; a consumer that holds the mesh
  drives the topology kernel below to extract edges/outlines for rendering or to flip winding for a viewer.
- construction law: the empty form initializes an olive `(128,128,0)` brush and opacity; the populated form carries the engine's vertices and faces.
- topology law: `SetIndices` rebuilds `Faces` and `MeshIndices` from triples and throws when any triple length differs from three. Edge queries return directed, de-duplicated, boundary, or ordered-loop index collections; winding reversal swaps indices zero and two.

| [INDEX] | [SURFACE]                                                                    | [KIND]          | [CAPABILITY]                          |
| :-----: | :--------------------------------------------------------------------------- | :-------------- | :------------------------------------ |
|  [01]   | `new ForceMomentMesh()`                                                      | constructor     | empty hull                            |
|  [02]   | `new ForceMomentMesh(IList<IForceMomentVertex>, IList<IForceMomentTriFace>)` | constructor     | engine-produced hull                  |
|  [03]   | `mesh.AddVertex<C>(Force x, Torque y, Torque z, C coordinate)`               | generic mutator | typed vertex with `C: ICoordinate`    |
|  [04]   | `mesh.AddVertex<V>(V vertex)`                                                | generic mutator | `ICartesian3d<Force, Torque, Torque>` |
|  [05]   | `mesh.SetIndices(IList<int[]>)`                                              | mutator         | face and index-triple rebuild         |
|  [06]   | `mesh.GetUniqueEdges()`                                                      | topology query  | `List<int[]>` undirected edges        |
|  [07]   | `mesh.GetAllEdges()`                                                         | topology query  | `List<int[]>` directed edges          |
|  [08]   | `mesh.GetOuterEdges()`                                                       | topology query  | `List<int[]>` single-face edges       |
|  [09]   | `mesh.GetMeshOutlines()`                                                     | topology query  | `List<List<int>>` boundary loops      |
|  [10]   | `mesh.ReverseFaceDirections()`                                               | mutator         | outward/inward normal toggle          |
|  [11]   | `mesh.Verticies`                                                             | property        | vertex store; spelling retained       |
|  [12]   | `mesh.Faces`                                                                 | property        | face store                            |
|  [13]   | `mesh.MeshIndices`                                                           | property        | index-triple store                    |
|  [14]   | `mesh.Brush`                                                                 | property        | `VividOrange.Geometry` `IBrush`       |
|  [15]   | `mesh.Opacity`                                                               | property        | `double` render opacity               |

[ENTRYPOINT_SCOPE]: `ForceMomentTriFace` — the lazily computed facet

- rail: profiles
- facet law: A/B/C are read-only boundary vertices. `Center` memoizes their coordinate mean, and `Area` memoizes their Heron-formula `UnitsNet` `Ratio` meshing weight.

| [INDEX] | [SURFACE]                                            | [CALL_SHAPE]  | [CAPABILITY]                |
| :-----: | :--------------------------------------------------- | :------------ | :-------------------------- |
|  [01]   | `new ForceMomentTriFace(IForceMomentVertex a, b, c)` | constructor   | triangular facet            |
|  [02]   | `face.A`                                             | property      | read-only boundary vertex   |
|  [03]   | `face.B`                                             | property      | read-only boundary vertex   |
|  [04]   | `face.C`                                             | property      | read-only boundary vertex   |
|  [05]   | `face.Center`                                        | lazy property | memoized coordinate mean    |
|  [06]   | `face.Area`                                          | lazy property | memoized Heron `Ratio` area |

## [04]-[IMPLEMENTATION_LAW]

[HULL_ALGEBRA]:

- vertex root: `ForceMomentVertex` — a `(Force X, Torque Y, Torque Z)` carrier; the `(Force, Torque, Torque)` ctors
  normalize `Z` into `Y.Unit` so the two moment axes are unit-consistent. `AddVertex<V>` is polymorphic over any
  `ICartesian3d<Force, Torque, Torque>`, so the engine's vertices and a user-supplied point append identically.
- face root: `ForceMomentTriFace` — three vertices with lazy `Center` (coordinate mean) and lazy `Area` (Heron over
  the mixed Force/Torque coordinates as raw scalars, carried as `Ratio`); both memoize on first read.
- mesh root: `ForceMomentMesh` — `Verticies` + `Faces` + `MeshIndices` with the topology kernel: `SetIndices` builds
  faces from triples (rejecting non-triangular input), `GetUniqueEdges`/`GetAllEdges`/`GetOuterEdges` extract the
  edge sets, `GetMeshOutlines` traces the boundary loops, `ReverseFaceDirections` flips winding. This is the
  renderable-mesh surface a viewer consumes.

[TOPOLOGY_CONTRACT]:

- `GetOuterEdges` returns edges referenced by exactly ONE face — for a CLOSED capacity onion this is empty; on an
  open/clipped hull it is the boundary. `GetMeshOutlines` walks those outer edges into ordered vertex loops (the
  silhouette curves a 2D section-view or a P-M curve slice reads).
- `SetIndices(triples)` is the index-driven rebuild: it clears and reconstructs `Faces` from `Verticies[triple[i]]`
  and throws `ArgumentException` on any triple whose length ≠ 3, so a malformed topology fails fast at admission.
- `ReverseFaceDirections` swaps index 0 ↔ 2 of every triple — the outward-normal toggle a renderer needs when the
  engine emits the hull with inward winding.

[LOCAL_ADMISSION]:

- A Materials capacity owner holds the hull through the `IForceMomentMesh` floor (`api-vividorange-iforcemomentinteraction.md`),
  not this concrete class — the engine returns the floor type. This catalog's concrete surface is read when the
  owner MUTATES the mesh for rendering (brush/opacity, `ReverseFaceDirections`, `GetMeshOutlines`) or builds a hull
  directly from supplied vertices via `AddVertex`/`SetIndices`.
- The `Force`/`Torque` coordinates are `UnitsNet` quantities (`api-unitsnet.md`) and never reduced to `double` in an
  interior signature; the facet `Area` `Ratio` is a meshing weight, not a structural quantity.

[STACK]:

- engine seam: `InteractionDiagram` (`api-vividorange-interactiondiagram.md`) constructs `new ForceMomentVertex(N, My, Mz, ForceUnit.Kilonewton, TorqueUnit.KilonewtonMeter)` per strain-plane sample and `new ForceMomentMesh(vertices, faces)` from the MIConvexHull result — this package is the engine's output DATA, the engine is the producer.
- contract seam: every class implements its `api-vividorange-iforcemomentinteraction.md` floor interface, so a
  consumer depends on the floor and this concrete is swappable (a decimated/clipped hull can re-implement it).
- units seam: `Force`/`Torque` are the in-folder `UnitsNet` quantities (`api-unitsnet.md`) — a capacity vertex and a
  measured applied-load demand are the SAME types, so a utilisation check is one unit-typed comparison and a fold
  over the hull's coordinates uses `UnitMath.Max<…>`/`Min<…>`, never a raw `double` reduce.
- geometry seam: `ForceMomentMesh` is a `VividOrange.Geometry` `ICartesianMesh`/`IGeometryBase` carrying an `IBrush`
  (`VividOrange.ICartesianBase` floor) — it renders through the SAME geometry/brush vocabulary as the rest of
  the VividOrange geometry stack, so the capacity onion is a first-class viewer mesh, not a custom shape.
- wire seam: the `ITaxonomySerializable` marker routes the mesh through `VividOrange.Serialization`
  (`api-vividorange-serialization.md`); the `Force`/`Torque` vertex fields serialize via the `UnitsNet` Json.NET
  converters (`api-vividorange-serialization.md` [TRANSITIVE_UNITSNET_JSONNET]) as canonical SI scalar plus unit token for a TS/Python peer.

[TRANSITIVE_CONVEX_HULL]:

- direct package: `VividOrange.InteractionDiagram` brings `MIConvexHull` and drives the capacity-hull construction.

[STACKING_LAW]:

[VividOrange.InteractionDiagram]: the Materials transitive consumer.

- Input: one force-moment-moment point per integrated strain plane, carried to `ConvexHull.Create` as `IVertex` with `double[] {N, My, Mz}`.
- Hull: `Faces` and `Points` weld the closed 3D capacity ONION as `IForceMomentMesh` / `IForceMomentVertex` / `IForceMomentTriFace`.
- Gradient: `Faces[].Normal` carries the per-facet capacity-surface gradient.
- Boundary: this is the ONLY `MIConvexHull` consumption the `Rasm.Materials` design composes; the AEC-DOMAIN folder consumes the capacity engine's welded `IForceMomentMesh` and NEVER calls `MIConvexHull` directly.

- `Rasm` kernel (the direct owner): `MIConvexHull` is the unconstrained-hull + Voronoi-dual primitive layered BESIDE the authored exact `Tessellation` (constrained Delaunay) and `Arrangement` (mesh boolean). The kernel `Drawing/` fill leg and the unconstrained-point-set Delaunay/Voronoi concerns compose `MIConvexHull`; the constrained-PSLG path stays on the authored Bowyer-Watson + `Triangle` (Triangle.NET). A consumer's own kernel vertex (carrying an exact-coordinate payload) implements `IVertex` so the SAME point type rides both the `double` `MIConvexHull` fast path and the exact authored mesher.

[CONCERN_PARTITION]: `Triangle`, `MIConvexHull`, and `SharpVoronoiLib` remain disjoint.

- `Triangle`: CONSTRAINED/conforming Delaunay over a PSLG with segments, holes, and quality refinement.
- `MIConvexHull`: UNCONSTRAINED point-set Delaunay and N-D Voronoi-as-Delaunay-dual without border clipping.
- `SharpVoronoiLib`: 2D point-site Fortune Voronoi with border clipping and Lloyd relaxation.
- Route: bounded or clipped 2D Voronoi uses `SharpVoronoiLib`; the N-D dual or hull-lift Delaunay uses `MIConvexHull`.

[RAIL_LAW]:

- Package: `MIConvexHull` (assembly `MIConvexHull`)
- Owns: the dimension-generic convex hull (`Create`/`Create2D`), the hull-lift unconstrained Delaunay (`Triangulation.CreateDelaunay`), and the Delaunay-dual Voronoi mesh (`CreateVoronoi`/`VoronoiMesh.Create`), all over the open `IVertex`/`IVertex2D` contracts returning typed `ConvexHullCreationResult` carriers in the `double` domain at a coplanarity tolerance
- Accept: the kernel's unconstrained-hull / Voronoi-dual / fast-`double`-Delaunay concern with a consumer-owned vertex type implementing `IVertex`/`IVertex2D`; the transitive `VividOrange.InteractionDiagram` N-M-M capacity-onion hull build
- Reject: any direct `MIConvexHull` call minted in the `Rasm.Materials` AEC-DOMAIN folder (it consumes the welded `IForceMomentMesh`, never the raw hull); using `MIConvexHull` where the input is near-degenerate and exact robustness is required (route to the kernel `Adaptive.Resolve` Bowyer-Watson); duplicating the constrained-PSLG Delaunay (`Triangle` owns it) or the clipped 2D point-site Voronoi (`SharpVoronoiLib` owns it); calling the ND `Create` on planar data (returns `DimensionTwoWrongMethod` — use `Create2D`); and catching `ConvexHullGenerationException` at a call site (it never escapes — match `Outcome` instead)

[RAIL_LAW]:

- Package: `VividOrange.ForceMomentInteraction` (MIT, pure-managed AnyCPU, `net10.0` binds `net8.0`, PRE-1.0 contract)
- Owns: the N-M-M capacity-hull CONCRETE — `ForceMomentMesh` (with the unique/outer/outline edge-extraction +
  winding-reversal topology kernel), `ForceMomentVertex` (the unit-typed (N, My, Mz) point with unit-normalizing
  ctors and polymorphic `AddVertex`), `ForceMomentTriFace` (lazy `Center`/`Area`) — all `Force`/`Torque`-typed and
  `ITaxonomySerializable`.
- Accept: a capacity hull constructed by the engine and read through `IForceMomentMesh`, or a hull mutated for
  rendering through this concrete; the `Force`/`Torque` coordinates carried as `UnitsNet` quantities at the boundary
- Reject: reducing a `Force`/`Torque` capacity coordinate to `double` in an interior signature; treating the facet
  `Area` `Ratio` as a physical area; a topology built from non-triangular index triples (`SetIndices` rejects it)
