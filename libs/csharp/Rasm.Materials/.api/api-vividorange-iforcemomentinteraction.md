# [RASM_MATERIALS_API_VIVIDORANGE_IFORCEMOMENTINTERACTION]

`VividOrange.IForceMomentInteraction` is the INTERFACE FLOOR of the column biaxial capacity hull — the
`IForceMomentMesh` / `IForceMomentVertex` / `IForceMomentTriFace` contract that the `InteractionDiagram` engine
EMITS (`api-vividorange-interactiondiagram.md`) and the `ForceMomentMesh` concrete implements
(`api-vividorange-forcemomentinteraction.md`). It is the typed shape of the N-M-M (axial-Force × moment-Torque ×
moment-Torque) capacity ONION: a `VividOrange.Geometry` `ICartesianMesh` whose three axes are NOT three lengths but
`Force` (axial N), `Torque` (My), `Torque` (Mz) `UnitsNet` quantities — so a column capacity surface is a
first-class unit-typed geometry mesh, not a bag of doubles. This package carries NO behaviour; it is the contract
the Materials RC-capacity owner reads at the wire and the concrete in `api-vividorange-forcemomentinteraction.md`
fulfils. The mesh's `ITaxonomySerializable` marker is the seam to the `VividOrange.Serialization` JSON pipeline
(`api-vividorange-serialization.md`), whose `UnitsNet` quantity fields round-trip through
`api-unitsnet-serialization-jsonnet.md`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `VividOrange.IForceMomentInteraction`
- package: `VividOrange.IForceMomentInteraction`
- version: `0.2.1`
- license: MIT (`licenses.nuget.org/MIT` — MagmaWorks / VividOrange taxonomy)
- assembly: `VividOrange.IForceMomentInteraction`
- namespace: `VividOrange.ForceMomentInteraction` (the interface set namespaces under the WORD-form, not the
  `I`-prefixed package id)
- asset: runtime library, pure-managed AnyCPU, NO native RID asset. Multi-TFM `net8.0` / `net7.0` / `net6.0` /
  `net48`; the consumer `net10.0` binds the highest managed asset `lib/net8.0` (no `net9.0`+ asset ships — `net8.0`
  IS the bound TFM, so the `api resolve` primary `net8.0` is the consumed surface, not an under-resolved fallback).
- rail: profiles (RC/steel column capacity surface — contract half)
- ABI floor: a `0.2.x` PRE-1.0 contract — the `IForceMoment*` member set and the `ICartesianMesh<…>` generic arity
  may break across a minor bump. The geometry floor (`ICartesianMesh`/`ICartesianVertex`/`ICartesianTriFace`/
  `ICartesian3d`/`ILocalCartesian2d`/`ICoordinate`/`IGeometryBase`) lives in `VividOrange.ICartesianBase` `1.8.0`
  (centrally pinned); `Force`/`Torque` are `UnitsNet` `5.75.0`; `ITaxonomySerializable` is the `VividOrange.ISerialization`
  floor (pinned). All transitively forced explicit by central transitive pinning.

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the N-M-M capacity-hull contract (interface floor)
- rail: profiles
- shape law: the three mesh axes are `Force` (axial), `Torque` (My), `Torque` (Mz) — the `ICartesianMesh<TVertex,
  TFace, TCoord, X, Y, Z>` generic binds `X = Force`, `Y = Torque`, `Z = Torque`, so the capacity surface is a
  unit-typed mesh and a vertex is one feasible (N, My, Mz) load combination on the capacity boundary.

| [INDEX] | [SYMBOL]               | [PACKAGE_ROLE]      | [CAPABILITY]                                                                                  |
| :-----: | :--------------------- | :------------------ | :-------------------------------------------------------------------------------------------- |
|  [01]   | `IForceMomentMesh`     | hull contract       | `: ICartesianMesh<IForceMomentVertex, IForceMomentTriFace, ICoordinate, Force, Torque, Torque>, IGeometryBase, ITaxonomySerializable` — the closed N-M-M capacity onion (vertices + tri-faces + index topology) |
|  [02]   | `IForceMomentVertex`   | hull vertex         | `: ICartesianVertex<ICoordinate, Force, Torque, Torque>, ICartesian3d<Force, Torque, Torque>, ILocalCartesian2d<Torque, Torque>, IGeometryBase, ITaxonomySerializable` — one (N, My, Mz) point on the capacity boundary; `X:Force`, `Y:Torque`, `Z:Torque` |
|  [03]   | `IForceMomentTriFace`  | hull facet          | `: ICartesianTriFace<IForceMomentVertex, ICoordinate, Force, Torque, Torque>, ICartesianFace<…>, IGeometryBase, ITaxonomySerializable` — one triangular facet of the hull (A/B/C vertices + center + area) |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the contract members a consumer reads (inherited from the `ICartesianMesh`/`ICartesianVertex` floor)
- rail: profiles
- note: these members are DEFINED on the `VividOrange.ICartesianBase` generic floor and SURFACED through the
  `IForceMoment*` specializations; the concrete getters live in `api-vividorange-forcemomentinteraction.md`. A
  Materials capacity owner reads the mesh through this contract and never depends on the concrete class.

| [INDEX] | [SURFACE]                         | [CALL_SHAPE]   | [CAPABILITY]                                                                                  |
| :-----: | :-------------------------------- | :------------- | :-------------------------------------------------------------------------------------------- |
|  [01]   | `IForceMomentMesh.Verticies`      | property       | `IList<IForceMomentVertex>` — the (N, My, Mz) capacity-boundary points (NOTE the package's spelling `Verticies`) |
|  [02]   | `IForceMomentMesh.Faces`          | property       | `IList<IForceMomentTriFace>` — the triangular hull facets                                     |
|  [03]   | `IForceMomentMesh.MeshIndices`    | property       | `IList<int[]>` — the per-face vertex-index triples (the mesh topology)                        |
|  [04]   | `IForceMomentVertex.X`            | property       | `UnitsNet.Force` — the axial-load coordinate N (compression/tension capacity at this point)   |
|  [05]   | `IForceMomentVertex.Y` / `.Z`     | property       | `UnitsNet.Torque` — the My / Mz moment coordinates of the capacity point                      |
|  [06]   | `IForceMomentTriFace.A` / `.B` / `.C` | property   | `IForceMomentVertex` — the facet's three boundary vertices                                    |
|  [07]   | `IForceMomentTriFace.Center`      | property       | `IForceMomentVertex` — the facet centroid (mean of A/B/C coordinates)                         |
|  [08]   | `IForceMomentTriFace.Area`        | property       | `IQuantity` — the facet area (a Heron-formula scalar carried as `UnitsNet` `Ratio`)           |

## [04]-[IMPLEMENTATION_LAW]

[CONTRACT_ALGEBRA]:
- floor root: `VividOrange.ICartesianBase` `ICartesianMesh<TVertex, TFace, TCoord, X, Y, Z>` — the generic
  unit-typed mesh contract; this package binds `(IForceMomentVertex, IForceMomentTriFace, ICoordinate, Force,
  Torque, Torque)` and adds NO new members, only the closed specialization.
- vertex root: `IForceMomentVertex` — a 3-axis `ICartesian3d<Force, Torque, Torque>` point AND a 2-axis
  `ILocalCartesian2d<Torque, Torque>` projection (the My-Mz moment plane at a fixed axial level), so a consumer can
  read the capacity boundary either as a 3D hull or as the per-axial-level moment envelope.
- serialization root: `ITaxonomySerializable` — the marker the `VividOrange.Serialization` `ITaxonomySerializable`
  JSON pipeline (`api-vividorange-serialization.md`) dispatches on; the `Force`/`Torque` fields round-trip through
  the `UnitsNet` Json.NET converters (`api-unitsnet-serialization-jsonnet.md`).

[AXIS_SEMANTICS]:
- The mesh is NOT a geometric solid in length-space. `X` is `Force` (kN axial), `Y`/`Z` are `Torque` (kNm
  moments). A point inside the hull is a SAFE (N, My, Mz) load combination; a point on the surface is at capacity;
  a point outside exceeds the section's biaxial capacity. The `ILocalCartesian2d<Torque, Torque>` view is the
  P-M interaction curve at the vertex's axial level.
- The `Area` of a facet is a `UnitsNet` `Ratio` (a dimensionless Heron product over the mixed Force/Torque
  coordinates treated as raw scalars) — it is a MESHING weight, not a physical area, and a consumer never reads it
  as a structural quantity.

[LOCAL_ADMISSION]:
- A Materials RC/steel column-capacity owner reads the hull THROUGH this interface floor (`IForceMomentMesh`),
  never the `ForceMomentMesh` concrete — the engine returns `IForceMomentMesh`, and a capacity-check folds over
  `mesh.Verticies` / `mesh.Faces` reading the typed `Force`/`Torque` coordinates.
- The hull is admitted at the boundary that projects a column section's capacity surface; the `Force`/`Torque`
  coordinates map onto the canonical Materials load/capacity owner's quantity types at the edge, never reduced to
  `double` in an interior signature.

[STACK]:
- engine seam: `InteractionDiagram.Mesh` is `IForceMomentMesh` (`api-vividorange-interactiondiagram.md`) — the
  contract is the engine's OUTPUT type, so a Materials capacity owner composes `new InteractionDiagram(section).Mesh`
  and reads it through this floor with no dependency on the concrete assembly.
- concrete seam: `ForceMomentMesh` / `ForceMomentVertex` / `ForceMomentTriFace`
  (`api-vividorange-forcemomentinteraction.md`) are the only shipping implementations; the engine constructs them
  but a consumer holds the interface, so a Materials-owned alternative mesh (e.g. a decimated hull) could implement
  the same floor.
- units seam: `Force`/`Torque` are the in-folder `UnitsNet` quantities (`api-unitsnet.md`) — the capacity hull and
  a measured applied-load demand are the SAME `Force`/`Torque` types, so a utilisation check `demand vs capacity`
  is one unit-typed comparison, and `UnitMath.Max<…>`/`Min<…>` fold over the hull's coordinates without a raw reduce.
- geometry seam: the mesh is a `VividOrange.Geometry` `ICartesianMesh`/`IGeometryBase` (`VividOrange.ICartesianBase`
  `1.8.0` floor) — the SAME geometry contract family as the section-property `ILocalPoint2d`/`ILocalDomain2d` returns
  (`api-vividorange-sections-sectionproperties.md`), so the capacity hull and the section geometry share one
  geometry vocabulary across the Profiles rail.
- wire seam: the `ITaxonomySerializable` marker routes the hull through the `VividOrange.Serialization` JSON pipeline
  (`api-vividorange-serialization.md`); the `Force`/`Torque` vertex fields serialize as `UnitsNet` quantities via
  `api-unitsnet-serialization-jsonnet.md`, so a computed capacity surface round-trips to a TS/Python peer as the
  canonical SI scalar plus unit token.

[RAIL_LAW]:
- Package: `VividOrange.IForceMomentInteraction` `0.2.1` (MIT, pure-managed AnyCPU, `net10.0` binds `net8.0`, PRE-1.0 contract)
- Owns: the N-M-M capacity-hull INTERFACE FLOOR — `IForceMomentMesh`/`IForceMomentVertex`/`IForceMomentTriFace` as
  a `VividOrange.Geometry` `ICartesianMesh` whose axes are `Force`/`Torque`/`Torque` `UnitsNet` quantities, with the
  `ITaxonomySerializable` JSON marker. NO behaviour — the contract only.
- Accept: a column capacity surface read through `IForceMomentMesh` (the engine's output type), the `Force`/`Torque`
  coordinates carried as `UnitsNet` quantities and mapped onto the canonical Materials capacity owner at the boundary
- Reject: reading the hull through the concrete `ForceMomentMesh` class instead of this floor; reducing a
  `Force`/`Torque` capacity coordinate to a raw `double` in an interior signature; treating the facet `Area` `Ratio`
  as a physical quantity
