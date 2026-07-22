# [RASM_MATERIALS_API_VIVIDORANGE_FORCEMOMENTINTERACTION]

`VividOrange.ForceMomentInteraction` owns the N-M-M column capacity-hull concrete: `ForceMomentVertex` carries one `(Force N, Torque My, Torque Mz)` point, `ForceMomentMesh` welds those points into the closed capacity onion behind a topology kernel, and `ForceMomentTriFace` weights each facet. This package owns the mesh DATA and topology algebra; the engine produces the vertices, and consumers read the hull through the interface floor. Coordinates stay `UnitsNet` `Force`/`Torque` quantities, so the surface renders as a `VividOrange.Geometry` mesh and round-trips the taxonomy JSON rail.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `VividOrange.ForceMomentInteraction`
- package: `VividOrange.ForceMomentInteraction` (MIT, MagmaWorks / VividOrange)
- assembly: `VividOrange.ForceMomentInteraction`
- namespace: `VividOrange.ForceMomentInteraction`
- asset: pure-managed AnyCPU, no native RID; the `net10.0` consumer binds `lib/net8.0`.
- rail: profiles (RC/steel column capacity surface — concrete implementation)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the N-M-M capacity-hull concrete; every class implements its matching interface floor.

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [CAPABILITY]                     |
| :-----: | :------------------- | :------------ | :------------------------------- |
|  [01]   | `ForceMomentMesh`    | class         | closed N-M-M capacity onion      |
|  [02]   | `ForceMomentVertex`  | class         | capacity point with texture      |
|  [03]   | `ForceMomentTriFace` | class         | lazy centroid and meshing weight |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `ForceMomentVertex` construction — the unit-typed `(N, My, Mz)` capacity point.
- `ForceMomentVertex.(Force, Torque, Torque)` ctors re-express `Z` into `Y.Unit` so My and Mz share one torque unit; the `(double, double, double, ForceUnit, TorqueUnit)` ctor is the engine's raw-scalar form.

| [INDEX] | [SURFACE]                                                          | [SHAPE]  | [CAPABILITY]                 |
| :-----: | :----------------------------------------------------------------- | :------- | :--------------------------- |
|  [01]   | `ForceMomentVertex()`                                              | ctor     | zero `Force`/`Torque` point  |
|  [02]   | `ForceMomentVertex(Force, Torque, Torque)`                         | ctor     | unit-normalized typed point  |
|  [03]   | `ForceMomentVertex(Force, Torque, Torque, ICoordinate)`            | ctor     | typed point with texture     |
|  [04]   | `ForceMomentVertex(double, double, double, ForceUnit, TorqueUnit)` | ctor     | engine scalar-and-unit form  |
|  [05]   | `ForceMomentVertex.X`                                              | property | settable `Force` coordinate  |
|  [06]   | `ForceMomentVertex.Y`                                              | property | settable `Torque` coordinate |
|  [07]   | `ForceMomentVertex.Z`                                              | property | settable `Torque` coordinate |
|  [08]   | `ForceMomentVertex.TextureCoordinate`                              | property | render texture `ICoordinate` |

[ENTRYPOINT_SCOPE]: `ForceMomentMesh` construction and the topology kernel.
- `ForceMomentMesh()` initializes an olive `(128,128,0)` `Brush` and opacity; the populated ctor carries the engine's vertices and faces.

| [INDEX] | [SURFACE]                                                                | [SHAPE]  | [CAPABILITY]                          |
| :-----: | :----------------------------------------------------------------------- | :------- | :------------------------------------ |
|  [01]   | `ForceMomentMesh()`                                                      | ctor     | empty hull                            |
|  [02]   | `ForceMomentMesh(IList<IForceMomentVertex>, IList<IForceMomentTriFace>)` | ctor     | engine-produced hull                  |
|  [03]   | `mesh.AddVertex<C>(Force, Torque, Torque, C)`                            | instance | typed vertex with `C: ICoordinate`    |
|  [04]   | `mesh.AddVertex<V>(V)`                                                   | instance | `ICartesian3d<Force, Torque, Torque>` |
|  [05]   | `mesh.SetIndices(IList<int[]>)`                                          | instance | face and index-triple rebuild         |
|  [06]   | `mesh.GetUniqueEdges()`                                                  | instance | `List<int[]>` undirected edges        |
|  [07]   | `mesh.GetAllEdges()`                                                     | instance | `List<int[]>` directed edges          |
|  [08]   | `mesh.GetOuterEdges()`                                                   | instance | `List<int[]>` single-face edges       |
|  [09]   | `mesh.GetMeshOutlines()`                                                 | instance | `List<List<int>>` boundary loops      |
|  [10]   | `mesh.ReverseFaceDirections()`                                           | instance | outward/inward normal toggle          |
|  [11]   | `mesh.Verticies`                                                         | property | vertex store; spelling retained       |
|  [12]   | `mesh.Faces`                                                             | property | face store                            |
|  [13]   | `mesh.MeshIndices`                                                       | property | index-triple store                    |
|  [14]   | `mesh.Brush`                                                             | property | `VividOrange.Geometry` `IBrush`       |
|  [15]   | `mesh.Opacity`                                                           | property | `double` render opacity               |

[ENTRYPOINT_SCOPE]: `ForceMomentTriFace` — the lazily computed facet; `A`/`B`/`C` are read-only and `Center`/`Area` memoize on first read.

| [INDEX] | [SURFACE]                                                                        | [SHAPE]  | [CAPABILITY]                |
| :-----: | :------------------------------------------------------------------------------- | :------- | :-------------------------- |
|  [01]   | `ForceMomentTriFace(IForceMomentVertex, IForceMomentVertex, IForceMomentVertex)` | ctor     | triangular facet            |
|  [02]   | `face.A`                                                                         | property | read-only boundary vertex   |
|  [03]   | `face.B`                                                                         | property | read-only boundary vertex   |
|  [04]   | `face.C`                                                                         | property | read-only boundary vertex   |
|  [05]   | `face.Center`                                                                    | property | memoized coordinate mean    |
|  [06]   | `face.Area`                                                                      | property | memoized Heron `Ratio` area |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- A `ForceMomentVertex` is one feasible `(Force N, Torque My, Torque Mz)` point on a column's capacity boundary; a `ForceMomentMesh` is the closed N-M-M onion welded from those points, and every coordinate stays a `UnitsNet` quantity end to end.
- Both `(Force, Torque, Torque)` ctors re-express `Z` into `Y.Unit`, so both moment axes carry one torque unit; `AddVertex<V>` appends any `ICartesian3d<Force, Torque, Torque>`, so an engine vertex and a supplied point enter identically.
- `SetIndices` rebuilds faces from index triples and rejects any triple of length ≠ 3, so a malformed topology fails at admission.
- `GetOuterEdges` returns edges bordered by exactly one face — empty for a closed onion, the boundary for an open or clipped hull; `GetMeshOutlines` walks those edges into the ordered vertex loops a 2D section or P-M slice reads, and `ReverseFaceDirections` toggles the outward normal when the engine emits inward winding.

[STACKING]:
- `api-vividorange-interactiondiagram.md`: this package is the engine's output data — the engine constructs `ForceMomentVertex(N, My, Mz, ForceUnit.Kilonewton, TorqueUnit.KilonewtonMeter)` per strain-plane sample and `ForceMomentMesh(vertices, faces)` from the welded hull.
- `api-vividorange-iforcemomentinteraction.md`: every class implements its `IForceMomentMesh`/`IForceMomentVertex`/`IForceMomentTriFace` floor, so a consumer binds the floor and a decimated or clipped hull re-implements it.
- `libs/csharp/.api/api-unitsnet.md`: a capacity vertex and a measured applied-load demand are the same `Force`/`Torque` quantities, so a utilisation check is one unit-typed comparison and a hull fold runs `UnitMath.Max`/`Min`, never a `double` reduce.
- `api-vividorange-serialization.md`: `ITaxonomySerializable` routes the mesh through the taxonomy JSON pipeline, and the `Force`/`Torque` fields serialize via the `UnitsNet` Json.NET converters as SI scalar with a unit token for a TS or Python peer.
- `libs/csharp/Rasm/.api/api-miconvexhull.md`: `InteractionDiagram` brings the transitive `MIConvexHull` dependency and welds `(N, My, Mz)` points into this closed-onion hull; the AEC-DOMAIN folder consumes the welded `IForceMomentMesh`, never the raw hull.
- `VividOrange.ICartesianBase`: `ForceMomentMesh` is a `VividOrange.Geometry` `ICartesianMesh`/`IGeometryBase` carrying an `IBrush`, so the capacity onion renders through the same geometry and brush vocabulary as every VividOrange mesh — a first-class viewer mesh, not a custom shape.
- within-lib: a Materials rendering owner drives the topology kernel on a held mesh — `GetMeshOutlines` for the silhouette a 2D section reads, `ReverseFaceDirections` to face the viewer, `Brush`/`Opacity` for display — while capacity math folds the `Force`/`Torque` coordinates through `UnitMath`.

[LOCAL_ADMISSION]:
- A Materials capacity owner binds the hull through the `IForceMomentMesh` floor, never this concrete — the engine returns the floor type; this concrete surface is read only to mutate a held mesh for rendering or to build a hull from supplied vertices via `AddVertex`/`SetIndices`.
- `Force`/`Torque` coordinates never reduce to `double` in an interior signature; the facet `Area` `Ratio` is a meshing weight, not a structural quantity.

[RAIL_LAW]:
- Package: `VividOrange.ForceMomentInteraction`
- Owns: the N-M-M capacity-hull concrete — `ForceMomentMesh` (unique/outer/outline edge extraction and winding-reversal topology kernel), `ForceMomentVertex` (unit-typed `(N, My, Mz)` point with unit-normalizing ctors and polymorphic `AddVertex`), `ForceMomentTriFace` (lazy `Center`/`Area`) — all `Force`/`Torque`-typed and `ITaxonomySerializable`.
- Accept: a capacity hull built by the engine and read through `IForceMomentMesh`, or a hull mutated for rendering through this concrete; `Force`/`Torque` coordinates carried as `UnitsNet` quantities at the boundary.
- Reject: reducing a `Force`/`Torque` capacity coordinate to `double` in an interior signature; treating the facet `Area` `Ratio` as a physical area; a topology built from non-triangular index triples (`SetIndices` rejects it).
