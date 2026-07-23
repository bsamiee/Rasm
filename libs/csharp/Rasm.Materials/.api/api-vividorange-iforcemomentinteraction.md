# [RASM_MATERIALS_API_VIVIDORANGE_IFORCEMOMENTINTERACTION]

`VividOrange.IForceMomentInteraction` owns the interface floor of the column capacity hull: `IForceMomentMesh`, `IForceMomentVertex`, and `IForceMomentTriFace` specialize a `VividOrange.Geometry` `ICartesianMesh` as an N-M-M capacity onion whose axes carry axial `Force` and two `Torque` `UnitsNet` quantities, so the capacity surface is a unit-typed mesh, not a bag of doubles. No behaviour ships — a Materials RC-capacity owner reads this contract, the concrete `ForceMomentMesh` implementing it.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `VividOrange.IForceMomentInteraction`
- package: `VividOrange.IForceMomentInteraction` (MIT)
- assembly: `VividOrange.IForceMomentInteraction`
- namespace: `VividOrange.ForceMomentInteraction`
- asset: runtime library, pure-managed AnyCPU, no native RID asset; multi-TFM down to `net48`, a `net10.0` consumer binding the `net8.0` asset
- rail: profiles — RC/steel column capacity surface, contract half

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the N-M-M capacity-hull contract, three interfaces specializing the geometry floor. `ICartesianMesh<TVertex, TFace, TCoord, X, Y, Z>` binds `X = Force`, `Y = Torque`, `Z = Torque`, so the surface is a unit-typed mesh and a vertex is one feasible (N, My, Mz) load combination on the capacity boundary.

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [CAPABILITY]                 |
| :-----: | :-------------------- | :------------ | :--------------------------- |
|  [01]   | `IForceMomentMesh`    | interface     | closed N-M-M capacity onion  |
|  [02]   | `IForceMomentVertex`  | interface     | feasible boundary load point |
|  [03]   | `IForceMomentTriFace` | interface     | triangular hull facet        |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the contract properties surfaced from the `VividOrange.ICartesianBase` geometry floor; these interfaces add no member of their own. `Verticies` retains the package spelling, and `Brush`/`Opacity` are the settable render pair while every other property is get-only.

| [INDEX] | [SURFACE]                                                 | [SHAPE]  | [CAPABILITY]              |
| :-----: | :-------------------------------------------------------- | :------- | :------------------------ |
|  [01]   | `IForceMomentMesh.Verticies -> IList<IForceMomentVertex>` | property | capacity-boundary points  |
|  [02]   | `IForceMomentMesh.Faces -> IList<IForceMomentTriFace>`    | property | triangular hull facets    |
|  [03]   | `IForceMomentMesh.MeshIndices -> IList<int[]>`            | property | per-face index triples    |
|  [04]   | `IForceMomentMesh.Brush -> IBrush`                        | property | render brush              |
|  [05]   | `IForceMomentMesh.Opacity -> double`                      | property | render opacity            |
|  [06]   | `IForceMomentVertex.X -> Force`                           | property | axial capacity coordinate |
|  [07]   | `IForceMomentVertex.Y -> Torque`                          | property | My coordinate             |
|  [08]   | `IForceMomentVertex.Z -> Torque`                          | property | Mz coordinate             |
|  [09]   | `IForceMomentVertex.TextureCoordinate -> ICoordinate`     | property | render coordinate         |
|  [10]   | `IForceMomentTriFace.A -> IForceMomentVertex`             | property | facet boundary vertex     |
|  [11]   | `IForceMomentTriFace.B -> IForceMomentVertex`             | property | facet boundary vertex     |
|  [12]   | `IForceMomentTriFace.C -> IForceMomentVertex`             | property | facet boundary vertex     |
|  [13]   | `IForceMomentTriFace.Center -> IForceMomentVertex`        | property | facet centroid            |
|  [14]   | `IForceMomentTriFace.Area -> IQuantity`                   | property | facet meshing weight      |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `IForceMomentMesh` is `ICartesianMesh<IForceMomentVertex, IForceMomentTriFace, ICoordinate, Force, Torque, Torque>`, `IGeometryBase`, and `ITaxonomySerializable`, adding no member beyond the closed specialization.
- `IForceMomentVertex` is both a 3-axis `ICartesian3d<Force, Torque, Torque>` point and a 2-axis `ILocalCartesian2d<Torque, Torque>` projection, so a consumer reads the boundary as a 3D hull or as the P-M moment envelope at one axial level.
- `IForceMomentTriFace` is `ICartesianTriFace<IForceMomentVertex, ICoordinate, Force, Torque, Torque>` carrying `A`/`B`/`C`, `Center`, and `Area`.
- Force-moment space, not length space: `X` carries axial `Force`, `Y` and `Z` carry My and Mz `Torque`; an interior point is a safe N-My-Mz combination, a surface point sits at capacity, an exterior point exceeds biaxial capacity.
- Facet `Area` is a `UnitsNet` `Ratio` — a dimensionless Heron product over the mixed coordinates as raw scalars, a meshing weight never read as a physical quantity.

[STACKING]:
- `VividOrange.InteractionDiagram`(`.api/api-vividorange-interactiondiagram.md`): `InteractionDiagram.Mesh` returns this floor, so a capacity owner composes `new InteractionDiagram(section).Mesh` and reads the hull through `IForceMomentMesh` with no dependency on the concrete assembly.
- `VividOrange.ForceMomentInteraction`(`.api/api-vividorange-forcemomentinteraction.md`): `ForceMomentMesh`/`ForceMomentVertex`/`ForceMomentTriFace` are the only shipping implementations; a consumer holds the floor, so a decimated Materials-owned hull re-implements it.
- `UnitsNet`(`libs/csharp/.api/api-unitsnet.md`): the hull `Force`/`Torque` axes are the same quantities as a measured applied-load demand, so `demand vs capacity` is one unit-typed comparison and `UnitMath.Max`/`Min` fold over the coordinates without a raw reduce.
- `VividOrange.Sections.SectionProperties`(`.api/api-vividorange-sections-sectionproperties.md`): the hull is a `VividOrange.Geometry` `ICartesianMesh`/`IGeometryBase`, the same geometry contract the section-property `ILocalPoint2d`/`ILocalDomain2d` returns, so capacity hull and section geometry share one vocabulary across the Profiles rail.
- `VividOrange.Serialization`(`.api/api-vividorange-serialization.md`): the `ITaxonomySerializable` marker routes the hull through the JSON pipeline, its `Force`/`Torque` fields serializing as a `UnitsNet` SI scalar and unit token for a TS/Python peer.
- Within Materials: an RC/steel capacity owner folds a capacity-check over `mesh.Verticies`/`mesh.Faces`, reading the typed `Force`/`Torque` coordinates and never a raw `double`.

[LOCAL_ADMISSION]:
- A Materials capacity owner reads the hull through `IForceMomentMesh`, the engine's return type, folding a capacity-check over `mesh.Verticies`/`mesh.Faces`.
- `Force`/`Torque` coordinates map onto the canonical Materials load/capacity quantity types at the boundary, never reduced to `double` in an interior signature.

[RAIL_LAW]:
- Package: `VividOrange.IForceMomentInteraction`
- Owns: the N-M-M capacity-hull interface floor — `IForceMomentMesh`/`IForceMomentVertex`/`IForceMomentTriFace` as a `VividOrange.Geometry` `ICartesianMesh` with `Force`/`Torque`/`Torque` axes and the `ITaxonomySerializable` JSON marker; contract only, no behaviour.
- Accept: a column capacity surface read through `IForceMomentMesh`, its `Force`/`Torque` coordinates mapped onto the Materials capacity owner at the boundary.
- Reject: reading the hull through the concrete `ForceMomentMesh` instead of this floor; reducing a `Force`/`Torque` coordinate to `double` in an interior signature; treating the facet `Area` `Ratio` as a physical quantity.
