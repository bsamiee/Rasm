# [RASM_MATERIALS_API_VIVIDORANGE_INTERACTIONDIAGRAM]

`VividOrange.InteractionDiagram` owns the RC column biaxial Force-Moment-Moment (N-M-M / P-M onion) capacity-surface engine, adding the reinforced-concrete interaction-diagram capability the elastic-only section-property solver (`api-vividorange-sections-sectionproperties.md`) does not compute. From an `IConcreteSection` carrying the concrete profile, EN material, and longitudinal rebar layout, the engine Triangle.NET-meshes the concrete perimeter and each circular rebar into `AnalyticalFace` fibres, runs a `Parallel.For` strain-plane sweep over `Steps²` orientations, integrates the `Pressure.Megapascals × Area` fibre stress over each plane's compression zone into an (N, My, Mz) capacity point, and MIConvexHull-assembles the points into the closed `IForceMomentMesh` capacity hull (`api-vividorange-iforcemomentinteraction.md` / `api-vividorange-forcemomentinteraction.md`). The computation composes the admitted `VividOrange.Sections` data owners, including `ConcreteSection` and the `VividOrange.Sections.Reinforcement` `Rebar`/`LongitudinalReinforcement`/`Link` types, with the `VividOrange.Materials` `AnalysisMaterialFactory.CreateLinearElastic` owner. The admitted Materials set reaches the full RC input path because `VividOrange.Sections` ships `ConcreteSection` and the reinforcement namespace ([01]-[RC_COMPOSITION_PATH]), so the engine is a first-class composable rail.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `VividOrange.InteractionDiagram`

- package: `VividOrange.InteractionDiagram`
- license: MIT (`licenses.nuget.org/MIT` — MagmaWorks / VividOrange taxonomy)
- assembly: `VividOrange.InteractionDiagram`
- namespace: `VividOrange.ForceMomentInteraction`, `.Utility` (the `.Utility` types are `internal` — see below)
- asset: runtime library, pure-managed AnyCPU, NO native RID asset. Multi-TFM `net8.0` / `net7.0` / `net6.0` /
  `net48`; the consumer `net10.0` binds the highest managed asset `lib/net8.0` (`net8.0` is the bound TFM — no
  `net9.0`+ asset, so the `api resolve` primary `net8.0` is the consumed surface).
- rail: profiles (RC/steel column capacity surface — the solver engine)
- ABI floor: a `0.2.x` PRE-1.0 contract — the `InteractionDiagram`/`DiagramSettings` member set may break across a minor bump.
- transitive closure: centrally transitive-pinned, all MIT/MIT-0, pure-managed AnyCPU, and osx-arm64-safe.
- output closure: `VividOrange.ForceMomentInteraction` + `VividOrange.IForceMomentInteraction` own the output mesh.
- section closure: `VividOrange.Sections` + `VividOrange.ISections` own the `IConcreteSection` and reinforcement input.
- material closure: `VividOrange.Materials` + `VividOrange.IMaterials` own `AnalysisMaterialFactory`; `VividOrange.Profiles`, `Profiles.Perimeter`, `Geometry`, `ICartesianBase`, and `UnitsNet` complete the data floor.
- geometry closure: the Rasm kernel primarily owns `MIConvexHull` for the N-M-M hull and `Triangle` for section meshing; this engine consumes both through the same central pin.

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the capacity-surface engine (the only consumer-callable types)

- rail: profiles

`InteractionDiagram` accepts an `IConcreteSection` with optional `DiagramSettings`, runs the strain-plane fibre sweep, and exposes the capacity hull through `IForceMomentMesh Mesh`. `DiagramSettings` owns concrete and rebar mesh refinement plus sweep resolution through `ConcreteMaximumFaceArea`, `ConcreteMinimumAngle`, `RebarMaximumFaceArea`, `RebarMinimumAngle`, `RebarDivisions`, and `Steps`.

| [INDEX] | [SYMBOL]             | [KIND] | [CAPABILITY]         |
| :-----: | :------------------- | :----- | :------------------- |
|  [01]   | `InteractionDiagram` | class  | eager capacity solve |
|  [02]   | `DiagramSettings`    | class  | solver policy        |

[PUBLIC_TYPE_SCOPE]: `.Utility` solver internals — NOT consumer-callable (`internal`)

- rail: profiles
- gate: every `.Utility` type is `internal` to this assembly.
- machinery: `AnalyticalFace` is the meshed fibre struct, `Meshing` binds the Triangle.NET `GenericMesher().Triangulate` and MIConvexHull `ConvexHull.Create` kernels, and `ConvexHullVertex` / `ConvexHullFace` adapt MIConvexHull `IVertex` / `ConvexFace<,>`.
- boundary: the utility types carry the fibre-integration law rather than consumer entrypoints; consumers construct neither `AnalyticalFace` nor `Meshing.CreateHull`.
- surface: `InteractionDiagram`, `DiagramSettings`, and the `IForceMomentMesh Mesh` output form the consumer-callable surface.

| [INDEX] | [SYMBOL]                   | [KIND]  | [CAPABILITY]           |
| :-----: | :------------------------- | :------ | :--------------------- |
|  [01]   | `Utility.AnalyticalFace`   | struct  | triangle fibre         |
|  [02]   | `Utility.Meshing`          | class   | mesh and hull assembly |
|  [03]   | `Utility.ConvexHullVertex` | adapter | MIConvexHull vertex    |
|  [04]   | `Utility.ConvexHullFace`   | adapter | MIConvexHull face      |

[Utility.AnalyticalFace]:

- Shape: a meshed triangle's `Area`, `Y`, and `Z` centroid in mm/mm².
- Entry: `CreateFromTriangleNetMesh(Mesh)` mints the stress-integration fibre.

[Utility.Meshing]:

- Mesh: `Create(pts, Area maxArea, Angle minAngle, voids)` returns Triangle.NET fibres.
- Hull: `CreateHull(vertices)` returns MIConvexHull faces.

[Utility.ConvexHullVertex]:

- Contract: implements `MIConvexHull.IVertex` around an `IForceMomentVertex` whose `Position` is the [N, My, Mz] scalar array.

[Utility.ConvexHullFace]:

- Contract: derives from `MIConvexHull.ConvexFace<ConvexHullVertex, ConvexHullFace>` and projects through `ToForceMomentTriFace()`.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: engine construction — the capacity surface from a concrete section

- rail: profiles
- construction law: the constructor RUNS the full solve eagerly (`Initialise` + the `Parallel.For` sweep + hull
  assembly) and populates `Mesh`; there is no separate `Solve()` call. Construction is the expensive operation.

The default constructor uses 30 steps with 250/200 mm² concrete/rebar maximum face areas; the settings overload selects explicit mesh refinement and sweep resolution.

| [INDEX] | [SURFACE]                                                                    | [KIND]   | [CAPABILITY]        |
| :-----: | :--------------------------------------------------------------------------- | :------- | :------------------ |
|  [01]   | `new InteractionDiagram(IConcreteSection section)`                           | ctor     | default solve       |
|  [02]   | `new InteractionDiagram(IConcreteSection section, DiagramSettings settings)` | ctor     | configured solve    |
|  [03]   | `diagram.Mesh`                                                               | property | N-M-M capacity hull |

[ENTRYPOINT_SCOPE]: `DiagramSettings` — solver knobs

- rail: profiles
- default law: defaults are `ConcreteMaximumFaceArea = 250 mm²`, `ConcreteMinimumAngle = 25°`,
  `RebarMaximumFaceArea = 200 mm²`, `RebarMinimumAngle = 25°`, `RebarDivisions = 16`, `Steps = 30`. The
  `Steps` knob drives a `Steps²` strain-plane sweep (azimuth × elevation), so cost is quadratic in `Steps`.

Each constructor overload owns one policy granularity.

| [INDEX] | [OVERLOAD]      | [POLICY]            |
| :-----: | :-------------- | :------------------ |
|  [01]   | `default`       | package defaults    |
|  [02]   | `steps`         | sweep resolution    |
|  [03]   | `shared-mesh`   | shared mesh policy  |
|  [04]   | `material-mesh` | per-material policy |

[default]: `new DiagramSettings()`.

[steps]: `new DiagramSettings(int steps)`.

[shared-mesh]: `new DiagramSettings(Area maxArea, Angle minAngle, int steps)` applies one face-area and angle policy to concrete and rebar.

[material-mesh]: `new DiagramSettings(Area maxConcreteArea, Angle minConcreteAngle, Area maxRebarArea, Angle minRebarAngle, int rebarDivisions, int steps)` exposes full per-material control.

The property surface carries one solver setting per row. `Steps` drives a quadratic `Steps²` sweep and bounds the hull at `Steps²` vertices; `RebarDivisions` defaults to 16 polygon segments per circular rebar.

| [INDEX] | [SURFACE]                  | [TYPE]           | [CAPABILITY]           |
| :-----: | :------------------------- | :--------------- | :--------------------- |
|  [01]   | `.ConcreteMaximumFaceArea` | `UnitsNet.Area`  | concrete face-area cap |
|  [02]   | `.RebarMaximumFaceArea`    | `UnitsNet.Area`  | rebar face-area cap    |
|  [03]   | `.ConcreteMinimumAngle`    | `UnitsNet.Angle` | concrete angle floor   |
|  [04]   | `.RebarMinimumAngle`       | `UnitsNet.Angle` | rebar angle floor      |
|  [05]   | `.RebarDivisions`          | `int`            | rebar polygon segments |
|  [06]   | `.Steps`                   | `int`            | sweep resolution       |

## [04]-[IMPLEMENTATION_LAW]

[RC_COMPOSITION_PATH]:

- The `IConcreteSection` input AND its reinforcement payload ARE reachable from the admitted Materials set — this
  engine is NOT admission-gated (this CORRECTS the `api-vividorange-sections-sectionproperties.md` `[ADMISSION_GATE]`
  note, which predates the `VividOrange.Sections` admission and wrongly claims the reinforcement floor is unrestored).
- `IConcreteSection: ISection` (with `IList<ILongitudinalReinforcement> Rebars`) lives in the admitted
  `VividOrange.ISections` floor; the `ConcreteSection` concrete (ctors
  `(IProfile, IMaterial, ILink, Length cover, IList<ILongitudinalReinforcement> rebars)` + `AddRebarLayer(IReinforcementLayer)`)
  and the full `VividOrange.Sections.Reinforcement` namespace (`Rebar`, `LongitudinalReinforcement`, `Link`,
  `FaceReinforcementLayer`/`PerimeterReinforcementLayer`, `ReinforcementLayoutByCount`/`BySpacing`,
  `MinimumReinforcementSpacing`) ship in the admitted `VividOrange.Sections` assembly
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
- sweep plane: `Parallel.For(0, Steps)` over azimuth × inner `Steps` elevation builds the unit strain-plane normal `(cos·cos, cos·sin/zLen, sin/yLen)`.
- sweep integral: each plane folds every concrete fibre on the compression side (`n·x > 0`) and every rebar fibre in compression or tension, with steel taking both signs. The integral accumulates `N += ±σ·A/1000`, `My += (z−z̄)·σ·A/1e6`, and `Mz += (y−ȳ)·σ·A/1e6`, where `σ = material.Strength.Megapascals`, then emits one `ForceMomentVertex(N, My, Mz, ForceUnit.Kilonewton, TorqueUnit.KilonewtonMeter)`.
- hull: the `Steps²` vertices are de-duplicated (`Distinct()`) and assembled into the closed onion by
  `Meshing.CreateHull` → MIConvexHull `ConvexHull.Create<ConvexHullVertex, ConvexHullFace>(…, 1e-10)`; the result is
  `new ForceMomentMesh(vertices, faces)` exposed as `Mesh`.
- This is a RIGID-PLASTIC stress-block capacity surface (uniform `fcd`/`fyd` over the compression/tension zones, no
  strain-limit curvature cap), so it is the design CAPACITY hull, not a moment-curvature analysis.

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
  (`api-vividorange-serialization.md`) over the `UnitsNet` Json.NET converters (`api-vividorange-serialization.md` [TRANSITIVE_UNITSNET_JSONNET]),
  so a computed capacity hull serializes to a TS/Python peer as canonical SI scalar plus unit token.

[RAIL_LAW]:

- Package: `VividOrange.InteractionDiagram` (MIT, pure-managed AnyCPU, `net10.0` binds `net8.0`, PRE-1.0 contract)
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
