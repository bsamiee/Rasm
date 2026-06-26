# [RASM_MATERIALS_API_VIVIDORANGE_INTERACTIONDIAGRAM]

`VividOrange.InteractionDiagram` is the RC column biaxial Force-Moment-Moment (N-M-M / P-M onion) capacity-SURFACE
ENGINE — the additive reinforced-concrete interaction-diagram capability the elastic-only section-property solver
(`api-vividorange-sections-sectionproperties.md`) does NOT compute. From an `IConcreteSection` (the concrete
profile + EN material + longitudinal rebar layout) it: Triangle.NET-meshes the concrete perimeter and each circular
rebar into `AnalyticalFace` fibres, runs a `Parallel.For` strain-plane sweep over `Steps²` orientations,
integrates the `Pressure.Megapascals × Area` fibre stress over the compression zone for each plane to a (N, My, Mz)
capacity point, and MIConvexHull-assembles the points into the closed `IForceMomentMesh` capacity hull
(`api-vividorange-iforcemomentinteraction.md` / `api-vividorange-forcemomentinteraction.md`). It is the COMPUTATION
that COMPOSES the admitted `VividOrange.Sections` (the `ConcreteSection` + `VividOrange.Sections.Reinforcement`
`Rebar`/`LongitudinalReinforcement`/`Link`) and `VividOrange.Materials` (`AnalysisMaterialFactory.CreateLinearElastic`)
DATA owners. The full RC input path IS reachable from the admitted Materials set — `VividOrange.Sections` ships
`ConcreteSection` and the reinforcement namespace ([01]-[RC_COMPOSITION_PATH]), so this engine is a first-class
composable rail, NOT admission-gated.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `VividOrange.InteractionDiagram`
- package: `VividOrange.InteractionDiagram`
- version: `0.2.1`
- license: MIT (`licenses.nuget.org/MIT` — MagmaWorks / VividOrange taxonomy)
- assembly: `VividOrange.InteractionDiagram`
- namespace: `VividOrange.ForceMomentInteraction`, `.Utility` (the `.Utility` types are `internal` — see below)
- asset: runtime library, pure-managed AnyCPU, NO native RID asset. Multi-TFM `net8.0` / `net7.0` / `net6.0` /
  `net48`; the consumer `net10.0` binds the highest managed asset `lib/net8.0` (`net8.0` is the bound TFM — no
  `net9.0`+ asset, so the `api resolve` primary `net8.0` is the consumed surface).
- rail: profiles (RC/steel column capacity surface — the solver engine)
- ABI floor: a `0.2.x` PRE-1.0 contract — the `InteractionDiagram`/`DiagramSettings` member set may break across a
  minor bump. The transitive closure (centrally transitive-pinned, all MIT/MIT-0, pure-managed AnyCPU, osx-arm64-safe):
  `VividOrange.ForceMomentInteraction` + `VividOrange.IForceMomentInteraction` `0.2.1` (the output mesh),
  `VividOrange.Sections` + `VividOrange.ISections` `0.1.0` (the `IConcreteSection`/reinforcement input),
  `VividOrange.Materials` + `VividOrange.IMaterials` `0.1.0` (`AnalysisMaterialFactory`), `VividOrange.Profiles` /
  `Profiles.Perimeter` / `Geometry` / `ICartesianBase` (`1.8.0`), `UnitsNet` `5.75.0`, plus the COMPUTATIONAL-GEOMETRY
  substrate `MIConvexHull` `1.1.19.1019` (the N-M-M hull) and `Triangle` `0.0.6-Beta3` (the section mesher) — both
  PRIMARILY OWNED by the Rasm KERNEL and consumed here through the same single central pin.

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the capacity-surface engine (the only consumer-callable types)
- rail: profiles

| [INDEX] | [SYMBOL]             | [PACKAGE_ROLE]     | [CAPABILITY]                                                                                  |
| :-----: | :------------------- | :----------------- | :-------------------------------------------------------------------------------------------- |
|  [01]   | `InteractionDiagram` | capacity engine    | constructs from an `IConcreteSection` (+ optional `DiagramSettings`), runs the strain-plane fibre sweep, exposes the `IForceMomentMesh Mesh` capacity hull |
|  [02]   | `DiagramSettings`    | solver settings    | the mesh-refinement + sweep-resolution knobs (`ConcreteMaximumFaceArea`/`ConcreteMinimumAngle`, `RebarMaximumFaceArea`/`RebarMinimumAngle`, `RebarDivisions`, `Steps`) |

[PUBLIC_TYPE_SCOPE]: `.Utility` solver internals — NOT consumer-callable (`internal`)
- rail: profiles
- gate: every `.Utility` type is `internal` to this assembly — `AnalyticalFace` (the meshed fibre struct),
  `Meshing` (the Triangle.NET `GenericMesher().Triangulate` + MIConvexHull `ConvexHull.Create` kernel),
  `ConvexHullVertex` / `ConvexHullFace` (the MIConvexHull `IVertex` / `ConvexFace<,>` adapters). They are the
  engine's machinery, documented here for the fibre-integration LAW, NOT as an entrypoint. A consumer NEVER
  constructs an `AnalyticalFace` or calls `Meshing.CreateHull`; the only surface is `InteractionDiagram` +
  `DiagramSettings` + the `IForceMomentMesh Mesh` output.

| [INDEX] | [SYMBOL]                    | [PACKAGE_ROLE]        | [CAPABILITY]                                                                  |
| :-----: | :-------------------------- | :-------------------- | :--------------------------------------------------------------------------- |
|  [01]   | `Utility.AnalyticalFace`    | fibre struct (internal) | a meshed triangle's `Area`/`Y`/`Z` (centroid) in mm/mm² — the stress-integration fibre; `CreateFromTriangleNetMesh(Mesh)` |
|  [02]   | `Utility.Meshing`           | mesher (internal)     | `Create(pts, Area maxArea, Angle minAngle, voids)` -> Triangle.NET fibres; `CreateHull(vertices)` -> MIConvexHull faces |
|  [03]   | `Utility.ConvexHullVertex`  | hull adapter (internal) | `: MIConvexHull.IVertex` wrapping an `IForceMomentVertex` (Position = [N, My, Mz] scalars) |
|  [04]   | `Utility.ConvexHullFace`    | hull adapter (internal) | `: MIConvexHull.ConvexFace<ConvexHullVertex, ConvexHullFace>` -> `ToForceMomentTriFace()` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: engine construction — the capacity surface from a concrete section
- rail: profiles
- construction law: the constructor RUNS the full solve eagerly (`Initialise` + the `Parallel.For` sweep + hull
  assembly) and populates `Mesh`; there is no separate `Solve()` call. Construction is the expensive operation.

| [INDEX] | [SURFACE]                                                       | [CALL_SHAPE]   | [CAPABILITY]                                                                    |
| :-----: | :------------------------------------------------------------- | :------------- | :----------------------------------------------------------------------------- |
|  [01]   | `new InteractionDiagram(IConcreteSection section)`            | constructor    | solve with default `DiagramSettings` (30 steps, 250/200 mm² max face area)      |
|  [02]   | `new InteractionDiagram(IConcreteSection section, DiagramSettings settings)` | constructor | solve with explicit mesh-refinement + sweep-resolution knobs                |
|  [03]   | `diagram.Mesh`                                                | property       | `IForceMomentMesh` — the closed N-M-M capacity hull (the engine's output)       |

[ENTRYPOINT_SCOPE]: `DiagramSettings` — solver knobs
- rail: profiles
- default law: defaults are `ConcreteMaximumFaceArea = 250 mm²`, `ConcreteMinimumAngle = 25°`,
  `RebarMaximumFaceArea = 200 mm²`, `RebarMinimumAngle = 25°`, `RebarDivisions = 16`, `Steps = 30`. The
  `Steps` knob drives a `Steps²` strain-plane sweep (azimuth × elevation), so cost is quadratic in `Steps`.

| [INDEX] | [SURFACE]                                                       | [CALL_SHAPE]   | [CAPABILITY]                                                                    |
| :-----: | :------------------------------------------------------------- | :------------- | :----------------------------------------------------------------------------- |
|  [01]   | `new DiagramSettings()`                                        | constructor    | the default knobs                                                              |
|  [02]   | `new DiagramSettings(int steps)`                              | constructor    | override only the sweep resolution                                             |
|  [03]   | `new DiagramSettings(Area maxArea, Angle minAngle, int steps)`| constructor    | one face-area/angle for both concrete + rebar, plus steps                      |
|  [04]   | `new DiagramSettings(Area maxConcreteArea, Angle minConcreteAngle, Area maxRebarArea, Angle minRebarAngle, int rebarDivisions, int steps)` | constructor | full per-material control |
|  [05]   | `.ConcreteMaximumFaceArea` / `.RebarMaximumFaceArea`         | property       | `UnitsNet.Area` — the Triangle.NET max-triangle-area refinement per material    |
|  [06]   | `.ConcreteMinimumAngle` / `.RebarMinimumAngle`               | property       | `UnitsNet.Angle` — the Triangle.NET min-angle quality constraint per material   |
|  [07]   | `.RebarDivisions`                                            | property       | `int` — the polygon-segment count approximating each circular rebar (default 16)|
|  [08]   | `.Steps`                                                     | property       | `int` — the strain-plane sweep resolution; the hull has up to `Steps²` vertices |

## [04]-[IMPLEMENTATION_LAW]

[RC_COMPOSITION_PATH]:
- The `IConcreteSection` input AND its reinforcement payload ARE reachable from the admitted Materials set — this
  engine is NOT admission-gated (this CORRECTS the `api-vividorange-sections-sectionproperties.md` `[ADMISSION_GATE]`
  note, which predates the `VividOrange.Sections` admission and wrongly claims the reinforcement floor is unrestored).
- `IConcreteSection : ISection` (with `IList<ILongitudinalReinforcement> Rebars`) lives in the admitted
  `VividOrange.ISections` `0.1.0` floor; the `ConcreteSection` concrete (ctors
  `(IProfile, IMaterial, ILink, Length cover, IList<ILongitudinalReinforcement> rebars)` + `AddRebarLayer(IReinforcementLayer)`)
  and the full `VividOrange.Sections.Reinforcement` namespace (`Rebar`, `LongitudinalReinforcement`, `Link`,
  `FaceReinforcementLayer`/`PerimeterReinforcementLayer`, `ReinforcementLayoutByCount`/`BySpacing`,
  `MinimumReinforcementSpacing`) ship in the admitted `VividOrange.Sections` `0.1.0` assembly
  (`api-vividorange-sections.md`). The `EN` material grades come from the admitted `VividOrange.Materials`
  (`api-vividorange-materials.md`). So a Materials RC-capacity owner builds the `IConcreteSection` from admitted
  packages and feeds it to this engine with no further admission.
- The engine reads `section.Profile` (the concrete outline), `section.Material` (cast to `IEnConcreteMaterial`),
  `section.Rebars` (each `ILongitudinalReinforcement` → `r.Rebar.Diameter` / `r.Rebar.Material` cast to
  `IEnRebarMaterial`). Materials must be EN grades — the strength read is `IEnConcreteMaterial`/`IEnRebarMaterial`
  specific.

[FIBRE_INTEGRATION_CONTRACT]:
- `Initialise`: meshes the concrete `Perimeter(section.Profile)` and each rebar (a `Circle(rebar.Diameter)` polygon
  of `RebarDivisions` segments) into `AnalyticalFace` fibres via Triangle.NET (`GenericMesher().Triangulate` with
  the per-material `ConcreteMaximumFaceArea`/`MinimumAngle` quality constraints), with rebar contours as voids in the
  concrete mesh; resolves the section bounds + barycenter; builds `LinearElasticMaterial` strengths via
  `AnalysisMaterialFactory.CreateLinearElastic` for concrete + each rebar.
- sweep: `Parallel.For(0, Steps)` over azimuth × inner `Steps` elevation builds a unit strain-plane normal
  `(cos·cos, cos·sin/zLen, sin/yLen)`; for each plane it folds every concrete fibre on the COMPRESSION side
  (`n·x > 0`) and every rebar fibre (compression OR tension, with steel taking both signs) accumulating
  `N += ±σ·A/1000`, `My += (z−z̄)·σ·A/1e6`, `Mz += (y−ȳ)·σ·A/1e6` where `σ = material.Strength.Megapascals`. Each
  plane yields one `ForceMomentVertex(N, My, Mz, ForceUnit.Kilonewton, TorqueUnit.KilonewtonMeter)`.
- hull: the `Steps²` vertices are de-duplicated (`Distinct()`) and assembled into the closed onion by
  `Meshing.CreateHull` → MIConvexHull `ConvexHull.Create<ConvexHullVertex, ConvexHullFace>(…, 1e-10)`; the result is
  `new ForceMomentMesh(vertices, faces)` exposed as `Mesh`.
- This is a RIGID-PLASTIC stress-block capacity surface (uniform `fcd`/`fyd` over the compression/tension zones, no
  strain-compatibility curvature limit), so it is the design CAPACITY hull, not a moment-curvature analysis.

[INTERNAL_GATE]:
- `AnalyticalFace`, `Meshing`, `ConvexHullVertex`, `ConvexHullFace` are `internal` — a consumer CANNOT call
  `Meshing.Create`/`CreateHull` or read an `AnalyticalFace`. The ONLY public surface is `InteractionDiagram` +
  `DiagramSettings` + `Mesh`. The Triangle.NET / MIConvexHull substrate is encapsulated; a design page composes the
  capacity hull through the constructor, never the meshing kernel.

[LOCAL_ADMISSION]:
- A Materials RC/steel column-capacity owner admits this engine through the boundary that computes a column's biaxial
  capacity surface: build an `IConcreteSection` from the admitted Profiles/Sections/Materials owners, choose
  `DiagramSettings` (sweep resolution + mesh refinement), construct `new InteractionDiagram(section, settings)`, and
  read `diagram.Mesh` as the `IForceMomentMesh` capacity hull through its floor (`api-vividorange-iforcemomentinteraction.md`).
- The hull's `Force`/`Torque` coordinates are `UnitsNet` quantities and a utilisation check folds applied
  `Force`/`Torque` demands against the hull as one unit-typed comparison; no interior signature carries the hull as
  raw `double`. The constructor is the expensive eager solve — a design page constructs once per section/settings and
  caches the `Mesh`, never re-solves per query.

[STACK]:
- section seam: the `IConcreteSection` input is the admitted `VividOrange.Sections` `ConcreteSection` +
  `VividOrange.Sections.Reinforcement` (`api-vividorange-sections.md`) — the engine is the COMPUTATION over the
  reinforced-section DATA, meeting it at the `IConcreteSection` contract ([01]-[RC_COMPOSITION_PATH]).
- material seam: `AnalysisMaterialFactory.CreateLinearElastic` over the section's `IEnConcreteMaterial` /
  `IEnRebarMaterial` (the admitted `VividOrange.Materials` EN grades, `api-vividorange-materials.md`) supplies the
  `fcd`/`fyd` strengths the fibre integral uses — the Materials grade DATA and this capacity COMPUTATION meet at
  `AnalysisMaterialFactory`.
- property seam: this engine is the ADDITIVE RC capability the elastic `SectionProperties` solver
  (`api-vividorange-sections-sectionproperties.md`) does not compute — `SectionProperties` gives elastic
  `Area`/`MomentOfInertia` over any `IProfile`, this gives the ULTIMATE biaxial capacity hull over an
  `IConcreteSection`; one Materials Profiles rail owns both the elastic section properties and the RC capacity surface.
- output seam: `Mesh` is the `IForceMomentMesh` floor (`api-vividorange-iforcemomentinteraction.md`), concrete
  `ForceMomentMesh` (`api-vividorange-forcemomentinteraction.md`) — a renderable `VividOrange.Geometry` `ICartesianMesh`
  whose axes are `Force`/`Torque` `UnitsNet` quantities (`api-unitsnet.md`).
- substrate seam: `Triangle` (Triangle.NET constrained-Delaunay) meshes the section into fibres and `MIConvexHull`
  closes the N-M-M points into the onion — both KERNEL-owned computational-geometry primitives consumed through the
  single central pin, encapsulated `internal` so the design page never touches them directly.
- wire seam: the `Mesh` is `ITaxonomySerializable` — it round-trips through `VividOrange.Serialization`
  (`api-vividorange-serialization.md`) over the `UnitsNet` Json.NET converters (`api-unitsnet-serialization-jsonnet.md`),
  so a computed capacity hull serializes to a TS/Python peer as canonical SI scalar plus unit token.

[RAIL_LAW]:
- Package: `VividOrange.InteractionDiagram` `0.2.1` (MIT, pure-managed AnyCPU, `net10.0` binds `net8.0`, PRE-1.0 contract)
- Owns: the RC column biaxial N-M-M capacity-surface ENGINE — Triangle.NET section meshing into `AnalyticalFace`
  fibres, the `Parallel.For` strain-plane fibre-stress sweep, and MIConvexHull assembly of the (N, My, Mz) points
  into the closed `IForceMomentMesh` hull, configured by `DiagramSettings` — the additive RC capability the elastic
  `SectionProperties` solver does not compute.
- Accept: an `IConcreteSection` (concrete profile + EN material + longitudinal rebar) built from the admitted
  `VividOrange.Sections`/`Materials`/`Profiles` owners, solved once at construction and read as the `IForceMomentMesh`
  capacity hull through its floor; the `Force`/`Torque` coordinates carried as `UnitsNet` quantities at the boundary
- Reject: treating the engine as admission-gated (the RC input IS reachable, [01]-[RC_COMPOSITION_PATH]); calling
  the `internal` `.Utility` meshing kernel directly; re-solving per query instead of caching `Mesh`; reducing a
  `Force`/`Torque` capacity coordinate to raw `double`; feeding a non-EN material whose strength the `IEnConcreteMaterial`/
  `IEnRebarMaterial` cast cannot read
