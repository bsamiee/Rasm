# [RASM_COMPUTE_API_FEALITE2D]

`FEALiTE2D` is the 2D planar-frame solver core — the 2D planar `FrameBackend` arm of `Analysis/structural#FRAME_BACKEND`, the planar twin of the 3D `BriefFiniteElement.Net` (`api-brief-finite-element`). It owns the linear matrix-stiffness analysis of plane frame, beam, and truss assemblies: a `Structure` graph of `Node2D` joints and `FrameElement2D`/`SpringElement2D` members carrying `IFrame2DSection` cross-sections over `IMaterial` constitutive data, loaded per `LoadCase`/`LoadCombination`, solved into per-load-case displacement and reaction vectors, then post-processed through `Structure.Results` (`PostProcessor`) into continuous internal-force and displacement diagrams sampled by an `ILinearMesher` over `LinearMeshSegment` stations. Its linear-algebra carriers are `CSparse` types — the global `StructuralStiffnessMatrix` is a `CSparse.Double.SparseMatrix` factored through `CSparse.Double.Factorization`, and every element matrix is a `CSparse.Double.DenseMatrix` — so the 2D and 3D structural-frame lanes ride the ONE shared `api-csparse` factorization owner, never a second linear-algebra rail; `MathNet.Numerics` (`api-mathnet-providers`) is the internal numeric floor only and never crosses the public surface. It is the 2D planar structural-frame backend only: the 3D structural-frame concern is the `BriefFiniteElement.Net` twin, the generalized continuum multi-physics concern is the separate `Solver/contract#SOLVE_CONTRACT` `SolveLane`, and DXF result rendering is the `FEALiTE2D.Plotting` (`api-fealite2d-plotting`) leg.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `FEALiTE2D`
- package: `FEALiTE2D`
- version: `1.1.2` (centrally pinned)
- license: `MIT`
- assembly: `FEALiTE2D`
- namespace roots: `FEALiTE2D.Structure`, `FEALiTE2D.Elements`, `FEALiTE2D.Loads`, `FEALiTE2D.CrossSections`, `FEALiTE2D.Materials`, `FEALiTE2D.Meshing`, `FEALiTE2D.Helper`
- asset: pure-managed AnyCPU IL, multi-target `net8.0` / `netstandard2.0` / `net45` (no native asset, no RID burden); the `net10.0` consumer binds `lib/net8.0/FEALiTE2D.dll`, XML docs shipped
- closure: `CSparse` (the `CSparse.Double.SparseMatrix`/`DenseMatrix` carriers + `CSparse.Double.Factorization` solve — the `api-csparse` owner shared with the 3D `BriefFiniteElement.Net` twin) + `MathNet.Numerics` (`api-mathnet-providers`, the internal numeric floor — never on the public surface); both pure-managed transitive deps, centrally pinned
- rail: `Analysis/structural#FRAME_BACKEND` (the 2D planar `FrameBackend` arm — the planar twin of the 3D `BriefFiniteElement.Net`, both on the shared `api-csparse` owner)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: model root and analysis lifecycle — `FEALiTE2D.Structure`
- rail: solve

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]    | [CAPABILITY]                                                                 |
| :-----: | :----------------------------- | :--------------- | :-------------------------------------------------------------------------- |
|  [01]   | `Structure`                    | model root       | node/element/load-case graph; `Solve()` assembles + factors the global system |
|  [02]   | `PostProcessor`                | result reader    | element internal forces, displacements, and support reactions per case/combination |
|  [03]   | `Assembler`                    | global assembler | builds the `StructuralStiffnessMatrix` and fixed-end load vectors            |
|  [04]   | `Displacement`                 | result vector    | `(Ux, Uy, Rz)` nodal/section displacement with `±` and scalar `*` operators |
|  [05]   | `Force`                        | result vector    | `(Fx, Fy, Mz)` internal force/reaction with `±` and scalar `*` operators    |
|  [06]   | `AnalysisStatus`               | status enum      | `Successful` / `Failure` — the analysis-receipt discriminant                |

[PUBLIC_TYPE_SCOPE]: element graph — `FEALiTE2D.Elements`
- rail: solve

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]      | [CAPABILITY]                                                                  |
| :-----: | :--------------------- | :----------------- | :--------------------------------------------------------------------------- |
|  [01]   | `IElement`             | element contract   | polymorphic member: DOF map, stiffness/transformation `DenseMatrix` set, mesh segments, shape function |
|  [02]   | `FrameElement2D`       | frame member       | axial+flexural beam-column element with `EndRelease` and `IFrame2DSection`    |
|  [03]   | `SpringElement2D`      | spring member      | two-node axial/rotational spring (`K`, `R`) element                           |
|  [04]   | `Node2D`               | joint              | `(X, Y)` joint with `Support`, nodal loads, support-displacement loads, transformation matrix |
|  [05]   | `NodalSupport`         | restraint          | boolean `(Ux, Uy, Rz)` fixity with `RestraintCount`                           |
|  [06]   | `NodalSpringSupport`   | elastic restraint  | `(Kx, Ky, Cz)` elastic support (a `NodalSupport` with a stiffness matrix)     |
|  [07]   | `Frame2DEndRelease`    | release enum       | `NoRelease` / `StartRelease` / `EndRelease` / `FullRelease` member-end hinge  |
|  [08]   | `NodalDegreeOfFreedom` | DOF enum           | `UX` / `UY` / `RZ` per-node degree-of-freedom selector                        |

[PUBLIC_TYPE_SCOPE]: load vocabulary — `FEALiTE2D.Loads`
- rail: solve

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]      | [CAPABILITY]                                                          |
| :-----: | :------------------------ | :----------------- | :------------------------------------------------------------------- |
|  [01]   | `ILoad`                   | element-load contract | per-element load with `LoadDirection`/`LoadCase`; `GetLoadValueAt`/`GetGlobalFixedEndForces` |
|  [02]   | `FramePointLoad`          | element load       | concentrated `(Fx, Fy, Mz)` at distance `L1` along a frame member    |
|  [03]   | `FrameUniformLoad`        | element load       | uniform `(Wx, Wy)` over span `[L1, L2]`                              |
|  [04]   | `FrameTrapezoidalLoad`    | element load       | linearly varying `(Wx1→Wx2, Wy1→Wy2)` over span `[L1, L2]`           |
|  [05]   | `NodalLoad`               | joint load         | concentrated `(Fx, Fy, Mz)` applied at a `Node2D`                    |
|  [06]   | `SupportDisplacementLoad` | imposed settlement | prescribed `(Ux, Uy, Rz)` support displacement                       |
|  [07]   | `LoadCase`                | load grouping      | named case with `LoadCaseType`/`LoadCaseDuration`; `IsLinearCase`    |
|  [08]   | `LoadCombination`         | factored combo     | `Dictionary<LoadCase, double>` factor map (subclass) with a `Label`  |
|  [09]   | `LoadCaseType`            | case enum          | `Dead`/`Live`/`Wind`/`Seismic`/`SelfWeight`/`Accidental`/`Shrinkage` |
|  [10]   | `LoadCaseDuration`        | duration enum      | `Instantaneous`/`ShortTerm`/`MediumTerm`/`LongTerm`/`Permanent`      |
|  [11]   | `LoadDirection`           | frame enum         | `Global` / `Local` load-axis frame                                  |

[PUBLIC_TYPE_SCOPE]: cross-sections and materials — `FEALiTE2D.CrossSections` / `FEALiTE2D.Materials`
- rail: solve

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]       | [CAPABILITY]                                                          |
| :-----: | :------------------------ | :------------------ | :------------------------------------------------------------------- |
|  [01]   | `IFrame2DSection`         | section base        | abstract: `A`, `Ax`/`Ay`, `Ix`/`Iy`, `J`, `MaxHeight`/`MaxWidth`, `Material` |
|  [02]   | `Generic2DSection`        | arbitrary section   | direct `(A, Ax, Ay, Ix, Iy, J, hmax, wmax)` escape hatch            |
|  [03]   | `RectangularSection`      | parametric section  | `(b, t)` solid rectangle                                            |
|  [04]   | `CircularSection`         | parametric section  | `(D)` solid round                                                   |
|  [05]   | `HollowTube`              | parametric section  | `(D, Thickness)` circular hollow                                     |
|  [06]   | `IPESection`              | parametric section  | `(tf, tw, b, h, r)` rolled I-section                                |
|  [07]   | `IMaterial`               | material contract   | constitutive-property contract                                       |
|  [08]   | `GenericIsotropicMaterial`| isotropic material  | `E`, `U` (Poisson), derived `G`, `Alpha` (thermal), `Gama` (density), `MaterialType` |
|  [09]   | `MaterialType`            | material enum       | `Steel`/`Concrete`/`Timber`/`Aluminum`/`Userdefined`                |

[PUBLIC_TYPE_SCOPE]: discretization sampling — `FEALiTE2D.Meshing`
- rail: solve

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]      | [CAPABILITY]                                                              |
| :-----: | :------------------ | :----------------- | :---------------------------------------------------------------------- |
|  [01]   | `ILinearMesher`     | mesher contract    | per-element sampling-station policy                                      |
|  [02]   | `LinearMesher`      | linear mesher      | `(NumberSegements, MinDistance)` station generator over each member     |
|  [03]   | `LinearMeshSegment` | diagram segment    | one sampled span carrying `MomentAt`/`ShearAt`/`AxialAt`/`*DisplacementAt`/`SlopeAngleAt` evaluators |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: model assembly and solve — `Structure`
- rail: solve

| [INDEX] | [SURFACE]                                              | [ENTRY_FAMILY]  | [CAPABILITY]                                                        |
| :-----: | :----------------------------------------------------- | :-------------- | :---------------------------------------------------------------- |
|  [01]   | `Structure()`                                          | constructor     | empty model (`Nodes`, `Elements`, `LoadCasesToRun` collections)   |
|  [02]   | `AddNode(Node2D)` / `AddNode(params Node2D[])`         | ingress         | register joints (polymorphic single/variadic)                     |
|  [03]   | `AddElement(IElement, bool addNodes = false)`          | ingress         | register a member, optionally auto-adding its end nodes           |
|  [04]   | `AddElement(IEnumerable<IElement>, bool addNodes)`     | ingress         | bulk member registration                                          |
|  [05]   | `LoadCasesToRun` / `LinearMesher` / `Tolerance`        | configuration   | cases to solve, sampling policy, convergence tolerance            |
|  [06]   | `Solve()`                                              | operation       | assemble + factor + back-substitute every `LoadCase`; sets `AnalysisStatus` |
|  [07]   | `Results` (`PostProcessor`)                            | result accessor | the post-processor bound after a successful solve                 |
|  [08]   | `StructuralStiffnessMatrix` (`CSparse.Double.SparseMatrix`) | matrix accessor | the assembled global stiffness — the `api-csparse` factor input   |
|  [09]   | `DisplacementVectors` / `FixedEndLoadsVectors`         | raw result map  | `Dictionary<LoadCase, double[]>` solution and load vectors        |
|  [10]   | `AnalysisStatus` / `nDOF`                              | receipt facts   | terminal status discriminant and active DOF count                 |

[ENTRYPOINT_SCOPE]: result extraction — `PostProcessor`
- rail: solve

| [INDEX] | [SURFACE]                                                            | [ENTRY_FAMILY] | [CAPABILITY]                                            |
| :-----: | :------------------------------------------------------------------ | :------------- | :----------------------------------------------------- |
|  [01]   | `GetElementInternalForcesAt(IElement, LoadCase, double x)`          | point query    | `Force` at station `x` for a case                       |
|  [02]   | `GetElementInternalForcesAt(IElement, LoadCombination, double x)`   | point query    | `Force` at station `x` for a factored combination       |
|  [03]   | `GetElementInternalForces(IElement, LoadCase)` / `(…, LoadCombination)` | diagram query  | `List<LinearMeshSegment>` full internal-force diagram (per case or factored combination) |
|  [04]   | `GetElementDisplacementAt(IElement, LoadCase, double x)` / `(…, LoadCombination, double x)` | point query    | `Displacement` at station `x` (per case or factored combination) |
|  [05]   | `GetNodeGlobalDisplacement(Node2D, LoadCase)`                       | nodal query    | global joint displacement                               |
|  [06]   | `GetSupportReaction(Node2D, LoadCase)` / `(…, LoadCombination)`     | reaction query | support `Force` reaction                                |
|  [07]   | `GetElementLocalEndDisplacement` / `GetElement*FixedEndForeces`     | raw vectors    | local/global element end vectors (`double[]`)           |

[ENTRYPOINT_SCOPE]: result carriers — `Force` / `Displacement` / `LinearMeshSegment`
- rail: solve

| [INDEX] | [SURFACE]                                                | [ENTRY_FAMILY]  | [CAPABILITY]                                              |
| :-----: | :------------------------------------------------------- | :-------------- | :------------------------------------------------------- |
|  [01]   | `Force.{Fx, Fy, Mz}` / `Displacement.{Ux, Uy, Rz}`      | components      | scalar internal-force / displacement components          |
|  [02]   | `ToVector()` / `FromVector(double[])`                    | conversion      | `double[]` round-trip for the matrix lane                |
|  [03]   | `operator +` / `operator -` / `operator *` (scalar)     | algebra         | superposition + load-factor scaling of result vectors    |
|  [04]   | `LinearMeshSegment.MomentAt/ShearAt/AxialAt(double x)`   | diagram eval    | continuous internal-force interpolation within a segment |
|  [05]   | `LinearMeshSegment.{Vertical,Axial}DisplacementAt(x)` / `SlopeAngleAt(x)` | diagram eval | continuous deflected-shape interpolation                 |

## [04]-[IMPLEMENTATION_LAW]

[ANALYSIS_PROFILE]:
- model root: `Structure` — the node/element/load-case graph; `Solve()` is the one terminal operation, `AnalysisStatus` (`Successful`/`Failure`) the terminal discriminant
- linear-algebra carrier: `CSparse.Double.SparseMatrix` for the global `StructuralStiffnessMatrix`; `CSparse.Double.DenseMatrix` for every element `LocalStiffnessMatrix`/`GlobalStiffnessMatrix`/`TransformationMatrix`/`LocalCoordinateSystemMatrix` — never a MathNet matrix on the public surface
- element polymorphism: `IElement` is the member contract; `FrameElement2D` (beam-column) and `SpringElement2D` (spring) are the two realized arms, both carrying the same DOF/stiffness/mesh surface
- receipt facts: `AnalysisStatus`, `nDOF`, the per-`LoadCase` `Force`/`Displacement`/reaction set, and the `LinearMeshSegment` diagram sampling are the structural-analysis receipt evidence

[SOLVE_AND_FACTOR]:
- `Structure.Solve()` assembles the global system through `Assembler`, factors `StructuralStiffnessMatrix` (a `CSparse.Double.SparseMatrix`) through a CSparse `ISparseFactorization<double>`, and back-substitutes each `LoadCase` into `DisplacementVectors` via `ISolver<double>.Solve`. The factorization is the `api-csparse` `SparseCholesky` rail with a `SparseQR` fallback for the rank-deficient / non-SPD case — the FE solve and the kernel's own sparse-factor lane share ONE owner; never stage a second dense inverse.
- A singular/ill-conditioned system terminates with `AnalysisStatus.Failure` rather than throwing; the discretization lane reads the status discriminant, not an exception, and lowers `Failure` into the typed solver-receipt rail.
- `Tolerance` governs convergence/zero-pivot detection; `nDOF` reports the active (unrestrained) degree-of-freedom count the factor operates on.

[DISCRETIZATION_SAMPLING]:
- `ILinearMesher`/`LinearMesher` sets the per-element station count (`NumberSegements`) and minimum station spacing (`MinDistance`); `FrameElement2D.AdditionalMeshPoints` injects load-discontinuity stations so point loads and span breaks land on a sample.
- `PostProcessor.GetElementInternalForces(element, case)` returns the `LinearMeshSegment` list — the discretized M/V/N and deflection diagram. Each segment's `MomentAt`/`ShearAt`/`AxialAt`/`*DisplacementAt` evaluators interpolate WITHIN the segment, so the Solver discretization lane samples a continuous field without re-solving.
- Superposition for a `LoadCombination` folds per-case `Force`/`Displacement` through the `operator +`/`operator *` algebra (`Force.FromVector`/`ToVector` bridges the matrix lane), never a re-assembly per combination.

[STACKING] — the 2D structural-frame lane of the Compute solver:
- `FEALiTE2D` is the 2D planar `FrameBackend` arm of `Analysis/structural#FRAME_BACKEND` — the planar twin of the 3D `BriefFiniteElement.Net`; the generalized continuum multi-physics concern is the separate `Solver/contract#SOLVE_CONTRACT` `SolveLane` (which assembles its own `Bᵀ·D·B` over a `Solver/discretization#DISCRETIZATION_MESH` `DiscreteMesh`), and a hand-rolled planar-frame assembler beside this core is the rejected form
- with `BriefFiniteElement.Net` (`api-brief-finite-element`): the 3D twin — `FEALiTE2D` solves planar frames, `BFE` the full 3D model; both factor through the SAME `api-csparse` sparse owner, so the 2D and 3D structural lanes share ONE factorization owner, never two
- with `FEALiTE2D.Plotting` (`api-fealite2d-plotting`): the result-visualization leg — the plotter reads this core's `Structure.Results` (`PostProcessor`) `GetElementInternalForces` `LinearMeshSegment` diagrams and renders the NFD/SFD/BMD/deflection panels to DXF; the 3D twin has no DXF plotter (its result feeds the AppUi/export rails by content key)
- with `CSparse` (`api-csparse`/`Tensor/factor#SPARSE_SOLVE`) and `MathNet.Numerics` (`api-mathnet-providers`): CSparse is the public-surface sparse-factorization owner (`StructuralStiffnessMatrix` a `SparseMatrix`, every element matrix a `DenseMatrix`); MathNet is the internal numeric floor only — never a MathNet type on the public surface, never a second linear-algebra rail
- with the `Rasm.Element` `ElementGraph` (via `Analysis/structural`): the concrete graph (read DIRECTLY above the seam, no `IElementProjection`) the `Structure` is assembled from — `StructuralAnalysis.Project`/`SolvePlanar` folds each planar member Object node into a `FrameElement2D` (`graph.AxisOf(member, geometry)`→axis resolved one-hop by content key through the seam `GeometrySource` port, `graph.SupportsOf`/`MemberSupport`→`NodalSupport`, `graph.LoadsOf`/`MemberLoad`→`FrameUniformLoad`), the section scalars the M7-resolved seam `SectionProperties` baked on the graph by the `Rasm.Materials` projector (Compute admits no VividOrange); the per-`LoadCase` `Force`/`Displacement`/reaction set is the 2D structural receipt keyed by the seam graph content address (`Runtime/codecs#CONTENT_ADDRESSING`)

[LOCAL_ADMISSION]:
- Build sections through the parametric `RectangularSection`/`CircularSection`/`HollowTube`/`IPESection` constructors over a `GenericIsotropicMaterial`; reserve `Generic2DSection` for the M7-resolved seam `SectionProperties` whose `(A, Ax, Ay, Ix, Iy, J)` the `Rasm.Materials` projector resolved once (the `ProfileRef`→`VividOrange.Sections.SectionProperties` one-hop baked on the graph) and the runner reads off the graph as SI scalars — never hand-key a steel section that the section-property owner already computes.
- `GenericIsotropicMaterial.G` is derived `0.5·E/(1+U)`; set `E` and `U`, never an inconsistent shear modulus.
- Loads attach to `LoadCase`es; only `LoadCasesToRun` are solved. A `LoadCombination` is a factor map (`Dictionary<LoadCase, double>`), so a combination is data, not a re-modelled load set.
- Boundary-map at the `Force`/`Displacement`/`LinearMeshSegment` seam: these carry raw `double` components, NOT `UnitsNet` quantities — the Solver lane re-attaches `UnitsNet.Force`/`Length`/`Pressure` units (`api-unitsnet`) at the receipt edge, and the section/material scalars must be supplied in one consistent unit system.

[RAIL_LAW]:
- Package: `FEALiTE2D`
- Owns: 2D plane-frame / beam / truss linear matrix-stiffness analysis — model assembly, sparse solve, and continuous internal-force/displacement/reaction post-processing
- Accept: the 2D planar `FrameBackend` arm of `Analysis/structural#FRAME_BACKEND` (the planar twin of the 3D `BriefFiniteElement.Net`, both factoring through the shared `api-csparse` owner) — `Structure` graphs of `Node2D` + `FrameElement2D`/`SpringElement2D` over `IFrame2DSection`/`IMaterial`, loaded per `LoadCase`/`LoadCombination`; the `CSparse` matrix carriers and `api-csparse` factorization (`MathNet.Numerics` the internal floor only); `LinearMesher` sampling over `LinearMeshSegment` feeding the Solver lane; `AnalysisStatus`/`Force`/`Displacement` as structural-analysis receipt facts; `FEALiTE2D.Plotting` (`api-fealite2d-plotting`) for DXF diagram artifacts; `UnitsNet` (`api-unitsnet`) unit re-attachment at the receipt edge
- Reject: a second matrix/linear-algebra rail beside `CSparse` (the matrices are CSparse types; `MathNet.Numerics` stays internal); exception-driven failure handling (read `AnalysisStatus.Failure`); 3D structural finite elements (the `BriefFiniteElement.Net` 3D twin (`api-brief-finite-element`) owns the 3D structural-frame lane, and the generalized continuum multi-physics solve is the separate `SolveLane`); hand-keyed section scalars where the section-property owner computes them; passing raw-`double` `Force`/`Displacement` past the receipt edge without unit re-attachment
