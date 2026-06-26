# [RASM_MATERIALS_API_VIVIDORANGE_FORCEMOMENTINTERACTION]

`VividOrange.ForceMomentInteraction` is the CONCRETE half of the column biaxial capacity hull — the
`ForceMomentMesh` / `ForceMomentVertex` / `ForceMomentTriFace` classes that implement the
`api-vividorange-iforcemomentinteraction.md` floor and that the `InteractionDiagram` engine
(`api-vividorange-interactiondiagram.md`) CONSTRUCTS as its output. A `ForceMomentVertex` is one feasible
(axial-`Force` N, moment-`Torque` My, moment-`Torque` Mz) point on a column's capacity boundary; a `ForceMomentMesh`
is the closed N-M-M ONION assembled from those points with a full mesh-topology kernel (unique-edge extraction,
outer-edge / outline tracing, face-direction reversal) so the capacity surface is a renderable, editable
`VividOrange.Geometry` `ICartesianMesh`. This package owns the mesh DATA + topology algebra; the strain-plane
fibre-integration that PRODUCES the vertices lives in `InteractionDiagram`, and the read contract a consumer holds
is the interface floor. The mesh's `Force`/`Torque` fields round-trip through the `VividOrange.Serialization`
`ITaxonomySerializable` JSON pipeline (`api-vividorange-serialization.md`) over the `UnitsNet` Json.NET converters
(`api-unitsnet-serialization-jsonnet.md`).

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `VividOrange.ForceMomentInteraction`
- package: `VividOrange.ForceMomentInteraction`
- version: `0.2.1`
- license: MIT (`licenses.nuget.org/MIT` — MagmaWorks / VividOrange taxonomy)
- assembly: `VividOrange.ForceMomentInteraction`
- namespace: `VividOrange.ForceMomentInteraction`
- asset: runtime library, pure-managed AnyCPU, NO native RID asset. Multi-TFM `net8.0` / `net7.0` / `net6.0` /
  `net48`; the consumer `net10.0` binds the highest managed asset `lib/net8.0` (`net8.0` is the bound TFM — no
  `net9.0`+ asset, so the `api resolve` primary `net8.0` is the consumed surface).
- rail: profiles (RC/steel column capacity surface — concrete implementation)
- ABI floor: a `0.2.x` PRE-1.0 contract. The interface floor is `VividOrange.IForceMomentInteraction` `0.2.1`; the
  geometry floor (`ICartesianMesh`/`ICartesianVertex`/`ICartesianTriFace`/`ICartesian3d`/`ILocalCartesian2d`/
  `ICoordinate`/`IBrush`/`Coordinate`/`Brush`) is `VividOrange.ICartesianBase` `1.8.0`; `Force`/`Torque`/`Ratio` are
  `UnitsNet` `5.75.0`; `ITaxonomySerializable` is the `VividOrange.ISerialization` floor — all centrally pinned
  (central transitive pinning forces explicit rows).

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the N-M-M capacity-hull concrete (implements the `api-vividorange-iforcemomentinteraction.md` floor)
- rail: profiles

| [INDEX] | [SYMBOL]             | [PACKAGE_ROLE]   | [CAPABILITY]                                                                                  |
| :-----: | :------------------- | :--------------- | :-------------------------------------------------------------------------------------------- |
|  [01]   | `ForceMomentMesh`    | hull (mutable)   | `: IForceMomentMesh` — the closed N-M-M capacity onion: vertices + tri-faces + index topology + render brush, with the topology kernel below |
|  [02]   | `ForceMomentVertex`  | hull vertex      | `: IForceMomentVertex` — one (N, My, Mz) capacity-boundary point; `X:Force`, `Y:Torque`, `Z:Torque`, plus a texture `ICoordinate` |
|  [03]   | `ForceMomentTriFace` | hull facet       | `: IForceMomentTriFace` — one triangular facet (A/B/C) with lazily computed `Center` (mean) and `Area` (Heron, `Ratio`) |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `ForceMomentVertex` construction — the unit-typed (N, My, Mz) capacity point
- rail: profiles
- normalization law: the `(Force x, Torque y, Torque z)` ctors RE-EXPRESS `z` into `y.Unit` so My and Mz share one
  torque unit; the `(double, double, double, ForceUnit, TorqueUnit)` ctor is the raw-scalar form the engine uses
  (`new ForceMomentVertex(N, My, Mz, ForceUnit.Kilonewton, TorqueUnit.KilonewtonMeter)`).

| [INDEX] | [SURFACE]                                                          | [CALL_SHAPE]   | [CAPABILITY]                                                                    |
| :-----: | :----------------------------------------------------------------- | :------------- | :----------------------------------------------------------------------------- |
|  [01]   | `new ForceMomentVertex()`                                          | constructor    | the zero vertex (`Force.Zero`, `Torque.Zero`, `Torque.Zero`)                    |
|  [02]   | `new ForceMomentVertex(Force x, Torque y, Torque z)`              | constructor    | a typed (N, My, Mz) point; `z` re-expressed into `y.Unit`                       |
|  [03]   | `new ForceMomentVertex(Force x, Torque y, Torque z, ICoordinate)` | constructor    | as [02] plus an explicit texture coordinate                                    |
|  [04]   | `new ForceMomentVertex(double x, double y, double z, ForceUnit, TorqueUnit)` | constructor | the raw-scalar + unit-enum form (the engine's vertex builder)              |
|  [05]   | `ForceMomentVertex.X` / `.Y` / `.Z`                               | property       | the `Force` / `Torque` / `Torque` capacity coordinates (settable)              |
|  [06]   | `ForceMomentVertex.TextureCoordinate`                            | property       | the render texture `ICoordinate`                                               |

[ENTRYPOINT_SCOPE]: `ForceMomentMesh` construction + the topology kernel
- rail: profiles
- note: the engine constructs `new ForceMomentMesh(vertices, faces)` from the hull; a consumer that holds the mesh
  drives the topology kernel below to extract edges/outlines for rendering or to flip winding for a viewer.

| [INDEX] | [SURFACE]                                                       | [CALL_SHAPE]   | [CAPABILITY]                                                                    |
| :-----: | :------------------------------------------------------------- | :------------- | :----------------------------------------------------------------------------- |
|  [01]   | `new ForceMomentMesh()`                                        | constructor    | an empty hull (default olive brush `(128,128,0)`, opacity 1.0)                  |
|  [02]   | `new ForceMomentMesh(IList<IForceMomentVertex>, IList<IForceMomentTriFace>)` | constructor | the hull from the engine's vertices + faces (the produced form)         |
|  [03]   | `mesh.AddVertex<C>(Force x, Torque y, Torque z, C coordinate)` | mutator (generic) | append a typed (N, My, Mz) vertex with a texture coordinate (`C : ICoordinate`) |
|  [04]   | `mesh.AddVertex<V>(V vertex)`                                  | mutator (generic) | append from any `V : ICartesian3d<Force, Torque, Torque>` (reads X/Y/Z)      |
|  [05]   | `mesh.SetIndices(IList<int[]>)`                                | mutator        | rebuild `Faces` + `MeshIndices` from index triples (throws if any triple ≠ 3)  |
|  [06]   | `mesh.GetUniqueEdges()`                                        | topology query | `List<int[]>` — the de-duplicated edge set (undirected)                         |
|  [07]   | `mesh.GetAllEdges()`                                           | topology query | `List<int[]>` — every directed face edge                                       |
|  [08]   | `mesh.GetOuterEdges()`                                         | topology query | `List<int[]>` — the boundary edges (edges referenced by exactly one face)       |
|  [09]   | `mesh.GetMeshOutlines()`                                       | topology query | `List<List<int>>` — the ordered boundary loops traced from the outer edges      |
|  [10]   | `mesh.ReverseFaceDirections()`                                | mutator        | flip every face winding (swap index 0 and 2) — outward/inward normal toggle     |
|  [11]   | `mesh.Verticies` / `.Faces` / `.MeshIndices`                  | property       | the `IList` vertex / face / index-triple stores (NOTE spelling `Verticies`)     |
|  [12]   | `mesh.Brush` / `.Opacity`                                     | property       | the `VividOrange.Geometry` `IBrush` render colour and `double` opacity          |

[ENTRYPOINT_SCOPE]: `ForceMomentTriFace` — the lazily computed facet
- rail: profiles

| [INDEX] | [SURFACE]                                                       | [CALL_SHAPE]   | [CAPABILITY]                                                                    |
| :-----: | :------------------------------------------------------------- | :------------- | :----------------------------------------------------------------------------- |
|  [01]   | `new ForceMomentTriFace(IForceMomentVertex a, b, c)`          | constructor    | a facet from three capacity-boundary vertices                                   |
|  [02]   | `face.A` / `.B` / `.C`                                         | property       | the three boundary vertices (read-only)                                        |
|  [03]   | `face.Center`                                                  | property (lazy)| the facet centroid — mean of A/B/C `X`/`Y`/`Z`, memoized                        |
|  [04]   | `face.Area`                                                    | property (lazy)| the Heron-formula facet area as `UnitsNet` `Ratio` (a meshing weight, memoized) |

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
- engine seam: `InteractionDiagram` (`api-vividorange-interactiondiagram.md`) constructs `new ForceMomentVertex(N,
  My, Mz, ForceUnit.Kilonewton, TorqueUnit.KilonewtonMeter)` per strain-plane sample and `new ForceMomentMesh(vertices,
  faces)` from the MIConvexHull result — this package is the engine's output DATA, the engine is the producer.
- contract seam: every class implements its `api-vividorange-iforcemomentinteraction.md` floor interface, so a
  consumer depends on the floor and this concrete is swappable (a decimated/clipped hull could re-implement it).
- units seam: `Force`/`Torque` are the in-folder `UnitsNet` quantities (`api-unitsnet.md`) — a capacity vertex and a
  measured applied-load demand are the SAME types, so a utilisation check is one unit-typed comparison and a fold
  over the hull's coordinates uses `UnitMath.Max<…>`/`Min<…>`, never a raw `double` reduce.
- geometry seam: `ForceMomentMesh` is a `VividOrange.Geometry` `ICartesianMesh`/`IGeometryBase` carrying an `IBrush`
  (`VividOrange.ICartesianBase` `1.8.0` floor) — it renders through the SAME geometry/brush vocabulary as the rest of
  the VividOrange geometry stack, so the capacity onion is a first-class viewer mesh, not a custom shape.
- wire seam: the `ITaxonomySerializable` marker routes the mesh through `VividOrange.Serialization`
  (`api-vividorange-serialization.md`); the `Force`/`Torque` vertex fields serialize via the `UnitsNet` Json.NET
  converters (`api-unitsnet-serialization-jsonnet.md`) as canonical SI scalar plus unit token for a TS/Python peer.

[RAIL_LAW]:
- Package: `VividOrange.ForceMomentInteraction` `0.2.1` (MIT, pure-managed AnyCPU, `net10.0` binds `net8.0`, PRE-1.0 contract)
- Owns: the N-M-M capacity-hull CONCRETE — `ForceMomentMesh` (with the unique/outer/outline edge-extraction +
  winding-reversal topology kernel), `ForceMomentVertex` (the unit-typed (N, My, Mz) point with unit-normalizing
  ctors and polymorphic `AddVertex`), `ForceMomentTriFace` (lazy `Center`/`Area`) — all `Force`/`Torque`-typed and
  `ITaxonomySerializable`.
- Accept: a capacity hull constructed by the engine and read through `IForceMomentMesh`, or a hull mutated for
  rendering through this concrete; the `Force`/`Torque` coordinates carried as `UnitsNet` quantities at the boundary
- Reject: reducing a `Force`/`Torque` capacity coordinate to `double` in an interior signature; treating the facet
  `Area` `Ratio` as a physical area; a topology built from non-triangular index triples (`SetIndices` rejects it)
