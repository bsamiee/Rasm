# [RASM_MATERIALS_API_VIVIDORANGE_INTERACTIONDIAGRAM]

`VividOrange.InteractionDiagram` owns the reinforced-concrete column biaxial capacity-surface engine: from an `IConcreteSection` it computes the closed N-M-M (Force-Moment-Moment / P-M onion) interaction hull the elastic section-property solver does not. Construction runs the full solve eagerly and exposes the hull as `IForceMomentMesh Mesh`, a `UnitsNet`-typed capacity onion a utilisation check folds applied `Force`/`Torque` demands against.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `VividOrange.InteractionDiagram`
- package: `VividOrange.InteractionDiagram` (MIT, MagmaWorks / VividOrange)
- assembly: `VividOrange.InteractionDiagram`
- namespace: `VividOrange.ForceMomentInteraction`, `VividOrange.ForceMomentInteraction.Utility` (`.Utility` types are `internal`)
- asset: runtime library, pure-managed AnyCPU, no native RID; consumer `net10.0` binds the `lib/net8.0` managed asset
- rail: profiles — RC/steel column capacity-surface solver

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: consumer-callable capacity-surface engine

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [CAPABILITY]                              |
| :-----: | :------------------- | :------------ | :---------------------------------------- |
|  [01]   | `InteractionDiagram` | class         | eager RC capacity solve, `Mesh` output    |
|  [02]   | `DiagramSettings`    | class         | mesh refinement + sweep-resolution policy |

[PUBLIC_TYPE_SCOPE]: `.Utility` solver internals — `internal`, not consumer-callable; they carry the fibre-integration mechanism ([04]-[TOPOLOGY]), and a consumer constructs none of them.

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY] | [CAPABILITY]                                                             |
| :-----: | :------------------------- | :------------ | :----------------------------------------------------------------------- |
|  [01]   | `Utility.AnalyticalFace`   | struct        | meshed triangle fibre (`Area`/`Y`/`Z` centroid, mm/mm²)                  |
|  [02]   | `Utility.Meshing`          | class         | static Triangle.NET meshing + MIConvexHull assembly                      |
|  [03]   | `Utility.ConvexHullVertex` | class         | MIConvexHull `IVertex` adapter over `IForceMomentVertex`                 |
|  [04]   | `Utility.ConvexHullFace`   | class         | MIConvexHull `ConvexFace<,>` adapter projecting to `IForceMomentTriFace` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: engine construction — the capacity surface from a concrete section

Construction runs the full solve (`Parallel.For` sweep + hull assembly) and populates `Mesh`; no separate `Solve()` exists, and construction is the expensive operation. Default construction uses 30 steps with 250/200 mm² concrete/rebar face-area caps.

| [INDEX] | [SURFACE]                                                   | [SHAPE]  | [CAPABILITY]        |
| :-----: | :---------------------------------------------------------- | :------- | :------------------ |
|  [01]   | `new InteractionDiagram(IConcreteSection)`                  | ctor     | default solve       |
|  [02]   | `new InteractionDiagram(IConcreteSection, DiagramSettings)` | ctor     | configured solve    |
|  [03]   | `diagram.Mesh`                                              | property | N-M-M capacity hull |

[ENTRYPOINT_SCOPE]: `DiagramSettings` — solver knobs

Defaults are `250 mm²`/`25°` concrete and `200 mm²`/`25°` rebar mesh, `16` rebar polygon segments, `30` steps. `Steps` drives a `Steps²` azimuth × elevation strain-plane sweep, so cost is quadratic in `Steps` and the hull is bounded at `Steps²` vertices.

| [INDEX] | [SURFACE]                                                 | [SHAPE]  | [CAPABILITY]                             |
| :-----: | :-------------------------------------------------------- | :------- | :--------------------------------------- |
|  [01]   | `new DiagramSettings()`                                   | ctor     | package defaults                         |
|  [02]   | `new DiagramSettings(int)`                                | ctor     | sweep resolution only                    |
|  [03]   | `new DiagramSettings(Area, Angle, int)`                   | ctor     | one mesh policy for concrete + rebar     |
|  [04]   | `new DiagramSettings(Area, Angle, Area, Angle, int, int)` | ctor     | full per-material mesh control           |
|  [05]   | `.ConcreteMaximumFaceArea`                                | property | concrete face-area cap (`UnitsNet.Area`) |
|  [06]   | `.RebarMaximumFaceArea`                                   | property | rebar face-area cap (`UnitsNet.Area`)    |
|  [07]   | `.ConcreteMinimumAngle`                                   | property | concrete angle floor (`UnitsNet.Angle`)  |
|  [08]   | `.RebarMinimumAngle`                                      | property | rebar angle floor (`UnitsNet.Angle`)     |
|  [09]   | `.RebarDivisions`                                         | property | rebar polygon segments (`int`)           |
|  [10]   | `.Steps`                                                  | property | strain-plane sweep resolution (`int`)    |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Construction meshes the concrete `Perimeter` and each rebar (a `RebarDivisions`-segment circle, voided in the concrete mesh) into `AnalyticalFace` fibres via Triangle.NET, builds `LinearElasticMaterial` strengths through `AnalysisMaterialFactory.CreateLinearElastic`, then `Parallel.For` sweeps `Steps²` strain-plane orientations, integrating `σ·A` over each compression zone into one `(N, My, Mz)` `ForceMomentVertex`; `Meshing.CreateHull` closes the de-duplicated points into `Mesh`.
- Output is a RIGID-PLASTIC stress-block capacity hull (uniform `fcd`/`fyd`, no strain-limit curvature cap) — the design CAPACITY envelope, not a moment-curvature analysis.
- Materials must be EN grades: the fibre strength read is `IEnConcreteMaterial`/`IEnRebarMaterial` specific, and every capacity coordinate stays a `UnitsNet` `Force`/`Torque` quantity — no interior carries the hull as raw `double`.

[STACKING]:
- `VividOrange.Sections`(`.api/api-vividorange-sections.md`): the `IConcreteSection` input is `ConcreteSection` + the `VividOrange.Sections.Reinforcement` `Rebar`/`LongitudinalReinforcement`/`Link` layout; the engine reads `section.Profile`, `section.Material`, and each `section.Rebars` entry's `Rebar.Diameter`/`Rebar.Material`.
- `VividOrange.Materials`(`.api/api-vividorange-materials.md`): `AnalysisMaterialFactory.CreateLinearElastic` over the section's `IEnConcreteMaterial`/`IEnRebarMaterial` supplies the `fcd`/`fyd` strengths the fibre integral folds.
- `VividOrange.IForceMomentInteraction`(`.api/api-vividorange-iforcemomentinteraction.md`) / `VividOrange.ForceMomentInteraction`(`.api/api-vividorange-forcemomentinteraction.md`): `Mesh` is the `IForceMomentMesh` floor, concrete `ForceMomentMesh` — a `VividOrange.Geometry` `ICartesianMesh` whose axes are `UnitsNet` `Force`/`Torque` (`libs/csharp/.api/api-unitsnet.md`).
- `VividOrange.Serialization`(`.api/api-vividorange-serialization.md`): `Mesh` is `ITaxonomySerializable`, round-tripping to a TS/Python peer as an SI scalar with its unit token over the `UnitsNet` Json.NET converters.
- Within-lib: `Triangle` (Triangle.NET constrained-Delaunay `GenericMesher().Triangulate`) meshes the section into fibres and `MIConvexHull` (`ConvexHull.Create<ConvexHullVertex, ConvexHullFace>(…, 1e-10)`) closes the N-M-M points — both kernel-owned geometry primitives consumed through the central pin, encapsulated `internal`.

[LOCAL_ADMISSION]:
- A Materials RC/steel column-capacity owner builds an `IConcreteSection` from the admitted Profiles/Sections/Materials owners, constructs `new InteractionDiagram(section, settings)` once, and reads `diagram.Mesh` as the `IForceMomentMesh` capacity hull; the RC input reaches from the admitted `VividOrange.Sections` set with no further admission.

[RAIL_LAW]:
- Package: `VividOrange.InteractionDiagram` (MIT, pure-managed AnyCPU, consumer `net10.0` binds `net8.0`)
- Owns: the RC column biaxial N-M-M capacity-surface engine — Triangle.NET fibre meshing, the `Parallel.For` strain-plane stress sweep, MIConvexHull assembly of the `(N, My, Mz)` points into the closed `IForceMomentMesh` hull, configured by `DiagramSettings` — the additive RC capability the elastic section-property solver does not compute.
- Accept: an `IConcreteSection` (concrete profile + EN material + longitudinal rebar) from the admitted `VividOrange.Sections`/`Materials`/`Profiles` owners, solved once at construction and read as the `IForceMomentMesh` hull with `UnitsNet` `Force`/`Torque` coordinates.
- Reject: calling the `internal` `.Utility` meshing kernel directly; re-solving per query instead of caching `Mesh`; reducing a capacity coordinate to raw `double`; feeding a non-EN material whose strength the `IEnConcreteMaterial`/`IEnRebarMaterial` cast cannot read.
