# [RASM_COMPUTE_API_BRIEF_FINITE_ELEMENT]

`BriefFiniteElement.Net` (assembly `BriefFiniteElementNet`, with its shared value/contract floor
`BriefFiniteElementNet.Common` and the `BriefFiniteElementNet.CustomElements` extension) is the managed
3D STRUCTURAL-FRAME finite-element solver: a `Model` of `Node`s and typed `Element`s (the 1D
`BarElement` frame/truss/beam/shaft, the 2D `TriangleElement` CST/DKT plate-shell + custom
`QuadrilaturalElement`, the 3D `TetrahedronElement` + custom `HexahedralElement` solid, plus
`ConcentratedMass`, `ParametricSpring`, and the `MpcElement` family `HingeLink`/`RigidElement_MPC`/
`TelepathyLink`/`VirtualConstraint`), each carrying a `Base1DSection`/`Base2DSection` and a
`BaseMaterial`, loaded by `LoadCase`/`LoadCombination`-keyed `NodalLoad`/`ConcentratedLoad`/
`UniformLoad`/`PartialNonUniformLoad`, constrained by the 6-DOF `Constraint`, assembled into a global
stiffness system and solved through a `BuiltInSolverType` (Cholesky/CG/LU/QR) — every concrete solver
factoring the sparse system through the admitted CSparse owner (`api-csparse`) — into a
`StaticLinearAnalysisResult` of per-`LoadCase` nodal `Displacement`s, `Force` support reactions, and
element internal forces (`BarElement.GetInternalForceAt`/`GetExactInternalForceAt`), with
`CauchyStressTensor`/`BendingStressTensor`/`StrainTensor` recovery. It is the 3D `FrameBackend` arm of
`Analysis/structural#FRAME_BACKEND` (the `Discipline.Structural` arm of the assessment rail) — distinct
from the generalized continuum `Solver/contract#SOLVE_CONTRACT` `SolveLane` that assembles its own
`Bᵀ·D·B` over a `Solver/discretization#DISCRETIZATION_MESH` `DiscreteMesh`: BFE owns the
code-checking-grade element library (exact beam shape functions, member releases, rigid links) the
continuum solver does not, and the structural runner DELEGATES a continuum field solve to the `SolveLane`
rather than BFE re-deriving it. It consumes the `Rasm.Element` `ElementGraph` DIRECTLY (above the seam, no
`IElementProjection`) via `Analysis/structural` — `StructuralAnalysis.Project` folds each member Object
node into a `BarElement` (`graph.AxisOf`→member axis), `graph.SupportsOf`/`MemberSupport`→`Node.Constraints`,
`graph.LoadsOf`/`MemberLoad`+`LoadCombinationSpec`→`LoadCase`/`LoadCombination`, with the M7-resolved seam
`SectionProperties` (Area/Iyy/Izz/J read off the graph — the `Rasm.Materials` projector's `ProfileRef`→VividOrange
one-hop baked on the graph, so Compute admits no VividOrange). It is pure-managed under LGPL-3.0-only (dynamic-linking obligation),
the 3D twin of the 2D `FEALiTE2D` (`api-fealite2d`); both ride CSparse.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `BriefFiniteElement.Net` (+ `BriefFiniteElementNet.Common` + `BriefFiniteElementNet.CustomElements`)
- package: `BriefFiniteElement.Net` (the core solver — assembly `BriefFiniteElementNet`) +
  `BriefFiniteElementNet.CustomElements` (hexahedral/quadrilateral/spring elements) — both direct csproj
  pins under "Structural Solvers"; `BriefFiniteElementNet.Common` (the shared value/contract assembly) is
  the transitive floor both pull (also centrally pinned)
- version: `2.1.2` (all three)
- license: LGPL-3.0-only (`license type="expression"`, `BriefFiniteElementNet/BriefFiniteElementNet`) —
  copyleft with a dynamic-linking obligation: keep all three as REFERENCED assemblies, never IL-merge,
  static-embed, or vendor source into a Rasm assembly (the same posture as the CSparse LGPL dependency)
- assembly: `BriefFiniteElement.Net` ships `BriefFiniteElementNet.dll` + `BriefFiniteElementNet.Common.dll`;
  `BriefFiniteElementNet.CustomElements` ships `BriefFiniteElementNet.CustomElements.dll`;
  `BriefFiniteElementNet.Common` ships `BriefFiniteElementNet.Common.dll` → the `net10.0` consumer binds
  `lib/net6.0` for each (each multi-targets `net6.0`/`netstandard2.0`/`net45`; `net6.0` is the bound
  asset and binds forward under `net10.0`); pure-managed AnyCPU IL, ALC-safe, no native asset
- namespace: `BriefFiniteElementNet` (the `Model`/`Node`/`Element` core + the value types in Common),
  `BriefFiniteElementNet.Elements` (`BarElement`/`TriangleElement`/`TetrahedronElement`/`ConcentratedMass`
  + custom `HexahedralElement`/`QuadrilaturalElement`), `.Sections`, `.Materials`, `.Loads`, `.Solver`,
  `.MpcElements`, `.ElementHelpers`, `.Integration`, `.Geometry`, `BriefFiniteElementNet.Common`
- transitive: `CSparse` (`api-csparse`, the sparse direct-factorization owner every BFE solver factors
  through — `StaticLinearAnalysisResult.Solvers_New` is keyed by CSparse `SparseMatrix`); the BFE 2.1.2
  nuspecs floor `CSparse >= 3.5.0` while the workspace unifies on the central `4.4.0` pin — BFE's SPARSE
  solve path stays binary-clean but its embedded dense `DenseLU` does not (the `[SOLVE_TOPOLOGY]` CSparse
  binding seam); the CustomElements package additionally depends on the core + Common
- scope: managed linear-static 3D structural FEM — model assembly, the frame/plate/shell/solid element
  library, sectioning, material, load cases/combinations, MPC, the direct/iterative solve, and result
  recovery (displacements, reactions, internal forces, stress/strain); NOT a continuum multi-physics
  solver (that is the `Solver/contract#SOLVE_CONTRACT` `SolveLane`), NOT a mesher (the structural graph
  arrives idealized from the `Rasm.Element` `ElementGraph` via `Analysis/structural`), NOT a code-check engine
- rail: `Analysis/structural#FRAME_BACKEND` (the 3D `FrameBackend` arm)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: model, node, and element family (`BriefFiniteElementNet` + `.Elements`)
- rail: solve
- note: `Model` is the assembly root holding `Nodes`/`Elements`/`MpcElements`/`RigidElements`; an
  `Element` is the abstract base the typed elements derive, each carrying its `Section`/`Material`/
  behaviour and contributing a local stiffness matrix the model assembles.

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]      | [RAIL]                                            |
| :-----: | :----------------------------- | :----------------- | :----------------------------------------------- |
|  [01]   | `Model` (sealed)               | model root         | `Nodes`/`Elements`/`MpcElements`/`RigidElements`/`LastResult`/`Trace`/`SettlementLoadCase`; the `Solve*`/`Solve_MPC*` entry; static binary `Save(file, model)`/`Load(file)`/`ToByteArray`/`FromByteArray` |
|  [02]   | `Node`                         | node               | `Location` (`Point`), `Constraints` (`Constraint`), `Loads`/`Settlements`; the displacement/reaction read surface |
|  [03]   | `Element` (abstract) / `ElementCollection` | element base   | `: StructurePart` — local stiffness/mass/damping + global-equivalent nodal loads; the typed-element base |
|  [04]   | `BarElement` (`.Elements`)     | 1D frame element   | beam/column/truss/shaft over `StartNode`/`EndNode`, `Section`/`Material`/`Behavior`, `Start`/`EndReleaseCondition` |
|  [05]   | `TriangleElement` (`.Elements`) | 2D plate-shell    | CST/DKT triangular membrane + bending element over a `Base2DSection` |
|  [06]   | `TetrahedronElement` (`.Elements`) | 3D solid       | 4-node tetrahedral continuum element |
|  [07]   | `ConcentratedMass` (`.Elements`) | mass element     | a lumped nodal mass for dynamic/modal mass assembly |
|  [08]   | `HexahedralElement` / `QuadrilaturalElement` / `ParametricSpring` (CustomElements) | extension elements | the 8-node brick solid, the quad shell/membrane, and the parametric spring |
|  [09]   | `MpcElement` (abstract) + `HingeLink` / `RigidElement_MPC` / `TelepathyLink` / `VirtualConstraint` | multi-point constraint | the MPC family (hinge release, rigid link, slaved-DOF, virtual constraint) |
|  [10]   | `StructureGenerator` (static)  | generator          | parametric test-structure generators (grids/frames) |

[PUBLIC_TYPE_SCOPE]: section, material, and load family (`.Sections` / `.Materials` / `.Loads`)
- rail: solve
- note: a `BarElement.Section` is a `Base1DSection` (the mechanical-property carrier A/Iy/Iz/J), its
  `Material` a `BaseMaterial`; loads are `LoadCase`/`LoadCombination`-keyed and applied nodally or along
  an element.

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]      | [RAIL]                                            |
| :-----: | :----------------------------- | :----------------- | :----------------------------------------------- |
|  [01]   | `Base1DSection` / `Base2DSection` (abstract) | section base | the 1D cross-section property contract (`GetCrossSectionPropertiesAt` → `_1DCrossSectionGeometricProperties`) / the 2D thickness contract (`GetThicknessAt`) |
|  [02]   | `UniformParametric1DSection`   | parametric 1D section | direct mechanical properties `A`/`Ay`/`Az`/`Iy`/`Iz`/`J`/`Ky`/`Kz` (the AISC-profile-table target) |
|  [03]   | `UniformGeometric1DSection` / `NonUniformGeometric1DSection` | geometric 1D section | a `PointYZ`/`PolygonYz` cross-section outline the props are derived from (uniform / tapered) |
|  [04]   | `NonUniformParametric1DSection` / `UniformParametric2DSection` | tapered / 2D section | a tapered 1D section / a 2D shell thickness |
|  [05]   | `BaseMaterial` (abstract)      | material base      | `GetMaterialPropertiesAt` → `AnisotropicMaterialInfo` |
|  [06]   | `UniformIsotropicMaterial`     | isotropic material | `YoungModulus`/`PoissonRatio`; `CreateFromYoungPoisson`/`CreateFromYoungShear`/`CreateFromShearPoisson` |
|  [07]   | `UniformAnisotropicMaterial`   | anisotropic material | `Ex`/`Ey`/`Ez`/`NuXy`/`NuYz`/`NuZx`/…/`Mu`/`Rho` orthotropic constants |
|  [08]   | `ElementalLoad` (abstract) + `ConcentratedLoad` / `UniformLoad` / `PartialNonUniformLoad` | element load | a point / uniform / partial-trapezoidal load on an element (`Force`/`Vector` + `CoordinationSystem`) |
|  [09]   | `NodalLoad`                    | nodal load         | a `Force` applied at a node under a `LoadCase` |
|  [10]   | `LoadCase` (struct) / `LoadCombination` / `LoadType` | load discriminant | the load-case identity (`LoadType` `Default`/`Dead`/`Live`/`Snow`/`Wind`/`Quake`/`Crane`/`Other`; `LoadCase.DefaultLoadCase`/`LoadCombination.DefaultLoadCombination` back the parameterless result reads) and the factored combination map |

[PUBLIC_TYPE_SCOPE]: value types, vocabulary, solvers, and results (`BriefFiniteElementNet.Common` + core + `.Solver`)
- rail: solve
- note: the Common assembly owns the shared value structs and the solver contracts; the concrete
  solvers in `.Solver` factor the assembled sparse system through CSparse.

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]      | [RAIL]                                            |
| :-----: | :----------------------------- | :----------------- | :----------------------------------------------- |
|  [01]   | `Constraint` (struct) + `Constraints` (static) | 6-DOF restraint | per-DOF `DofConstraint` (`Fixed`/`Released`); presets `Fixed`(encastre)/`Released`(fully free)/`MovementFixed`(pinned)/`RotationFixed`/`FixedDX`…`FixedRZ` (no separate `Free` — `Released` is the all-released support) |
|  [02]   | `Displacement` (struct)        | nodal displacement | `Dx`/`Dy`/`Dz`/`Rx`/`Ry`/`Rz`; `Zero`, `FromVector`/`ToVector`, `GetComponent(DoF)` |
|  [03]   | `Force` (struct)               | force/moment       | `Fx`/`Fy`/`Fz`/`Mx`/`My`/`Mz`; `FromVector`/`ToVector`, `GetComponent(DoF)` |
|  [04]   | `Point` / `Vector` / `IsoPoint` / `PointYZ` (structs) + `PolygonYz` (sealed class) | geometry value | the node location, a direction vector, an iso-parametric point, the section-outline point, and the section-outline polygon (`PolygonYz` is a reference type, not a value struct) |
|  [05]   | `DoF` / `DofConstraint` / `LoadDirection` / `CoordinationSystem` / `MassFormulation` | enum vocabulary | the DOF axis (`Dx`…`Rz`), fixity, load direction (`X`/`Y`/`Z`), local/global frame, lumped/consistent mass |
|  [06]   | `BarElementBehaviour` (`[Flags]`) + `BarElementBehaviours` (static) | bar behaviour | `Truss`/`BeamY*`/`BeamZ*`/`Shaft` flags; presets `FullFrame`/`FullBeam`/`Truss` (Euler-Bernoulli or Timoshenko) |
|  [07]   | `PlaneElementBehaviour` (`[Flags]`) + `PlaneElementBehaviours` (static) | plate behaviour | `ThinPlate`/`Membrane`/`DrillingDof`; preset `FullThinShell` |
|  [08]   | `BuiltInSolverType` (enum)      | solver selector    | `CholeskyDecomposition`/`ConjugateGradient`/`Lu`/`Qr` |
|  [09]   | `ISolver` / `ISolverFactory` (Common) + `IPreconditioner<T>` + `CholeskySolver` / `LuSolver` / `QRSolver` / iterative `PCG` (`: IterativeSolver`) + `CholeskySolverFactory` / `LuSolverFactory` / `QrSolverFactory` / `ConjugateGradientFactory` + `SSOR` / `IdentityPreconditioner<T>` (`.Solver`) | solver family | the `ISolver`/`ISolverFactory` contracts (Common), the concrete direct/iterative solvers, the `ISolverFactory` impls the `Solve(ISolverFactory)` seam consumes, and the `IPreconditioner<T>` set `PCG` rides (`SSOR` is a preconditioner, not a solver) — each factoring through CSparse |
|  [10]   | `StaticLinearAnalysisResult`   | result carrier     | per-`LoadCase` `Displacements`/`Forces`/`SupportReactions`/`ElementForces` (`Dictionary<LoadCase, double[]>`) |
|  [11]   | `CauchyStressTensor` / `BendingStressTensor` / `StrainTensor` / `GeneralStressTensor` (structs) | stress/strain | the element stress/strain recovery tensors |
|  [12]   | `Matrix` (`: DenseMatrix`) / `SolverConfiguration` | numeric carrier | the dense element-matrix type and the solve configuration (`SolverFactory` (`ISolverFactory`) / `LoadCases`) |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: model assembly (from the `ElementGraph`)
- rail: solve
- note: the canonical assembly maps the `Rasm.Element` `ElementGraph` (read directly via `Analysis/structural`)
  onto a BFE `Model` — each member Object node a `BarElement` (`graph.AxisOf`), each `MemberSupport`
  (`graph.SupportsOf`) a `Node.Constraints`, each `MemberLoad`+`LoadCombinationSpec` (`graph.LoadsOf`) a
  `LoadCase`/`LoadCombination` — section/material from the M7-resolved seam `SectionProperties`/`MaterialPropertySet.Mechanical` baked on the graph.

| [INDEX] | [SURFACE]                                                          | [ENTRY_FAMILY]  | [RAIL]                                            |
| :-----: | :---------------------------------------------------------------- | :-------------- | :----------------------------------------------- |
|  [01]   | `new Model()` → `model.Nodes.Add(new Node(x, y, z))`              | construct       | the model root and its node set |
|  [02]   | `new BarElement(Node n1, Node n2) { Section = …, Material = …, Behavior = BarElementBehaviours.FullFrame }` | author element | a frame member; `model.Elements.Add(bar)` |
|  [03]   | `node.Constraints = Constraints.Fixed` / `Constraints.MovementFixed` / a `new Constraint(dx, dy, dz, rx, ry, rz)` | constrain | apply the 6-DOF support (from `SupportRestraint`) |
|  [04]   | `new UniformParametric1DSection(a, iy, iz, j)` / `(a, iy, iz)` / `(a)` | section | the direct mechanical-property section (AISC A/Iy/Iz/J) |
|  [05]   | `UniformIsotropicMaterial.CreateFromYoungPoisson(E, nu)` / `new UniformIsotropicMaterial(E, nu)` | material | the isotropic material (steel/concrete E, ν) |
|  [06]   | `node.Loads.Add(new NodalLoad(force, loadCase))` / `bar.Loads.Add(new UniformLoad(loadCase, direction, magnitude, CoordinationSystem.Global))` | load | nodal and element loads under a `LoadCase` |

[ENTRYPOINT_SCOPE]: solve and solver selection
- rail: solve
- note: `Solve*` assembles the global stiffness, factors through the selected solver (CSparse-backed),
  and populates `LastResult`; `Solve(ISolverFactory)` is the seam to route through the shared Compute
  `Tensor/factor#SPARSE_SOLVE` CSparse owner.

| [INDEX] | [SURFACE]                                                          | [ENTRY_FAMILY]  | [RAIL]                                            |
| :-----: | :---------------------------------------------------------------- | :-------------- | :----------------------------------------------- |
|  [01]   | `model.Solve()`                                                   | solve           | assemble + factor + solve every load case (default Cholesky) |
|  [02]   | `model.Solve(BuiltInSolverType.CholeskyDecomposition)` / `.ConjugateGradient` / `.Lu` / `.Qr` | solve | solve with an explicit built-in solver kind |
|  [03]   | `model.Solve(ISolverFactory factory)`                            | solve           | solve through a custom solver factory (the CSparse-sharing seam) |
|  [04]   | `model.Solve(SolverConfiguration config)` / `model.Solve(params LoadCase[] cases)` | solve | configured solve / scope to specific load cases |
|  [05]   | `model.Solve_MPC(SolverConfiguration)` / `Solve_MPC(params LoadCase[])`  | solve (MPC)     | the master-slave-constraint solve path (with `MpcElement`s) |

[ENTRYPOINT_SCOPE]: result recovery (the structural receipt)
- rail: solve
- note: results are read off the nodes and elements per `LoadCase`/`LoadCombination` — nodal
  displacements and support reactions, and element internal forces/displacements along the span.

| [INDEX] | [SURFACE]                                                          | [ENTRY_FAMILY]  | [RAIL]                                            |
| :-----: | :---------------------------------------------------------------- | :-------------- | :----------------------------------------------- |
|  [01]   | `node.GetNodalDisplacement(LoadCase)` / `(LoadCombination)` → `Displacement` | read | the solved nodal displacement |
|  [02]   | `node.GetSupportReaction(LoadCase)` / `(LoadCombination)` → `Force` | read | the support reaction at a constrained node |
|  [03]   | `bar.GetInternalForceAt(double xi, LoadCase)` / `(LoadCombination)` → `Force` | read | the approximate internal force at a span fraction |
|  [04]   | `bar.GetExactInternalForceAt(double xi, LoadCase)` → `Force`      | read            | the exact (shape-function) internal force at a span fraction |
|  [05]   | `bar.GetInternalDisplacementAt(double xi, LoadCase)` / `GetExactInternalDisplacementAt(…)` → `Displacement` | read | the deflected-shape displacement along the span |
|  [06]   | `model.LastResult.Displacements` / `.SupportReactions` / `.ElementForces` (`Dictionary<LoadCase, double[]>`) | read | the raw per-load-case result vectors |

## [04]-[IMPLEMENTATION_LAW]

[MODEL_TOPOLOGY]:
- a `Model` holds `Nodes` (each a `Node` with a `Point` `Location`, a 6-DOF `Constraint`, and nodal
  `Loads`/`Settlements`) and `Elements` (each an `Element` carrying its `Section`/`Material`/behaviour
  and contributing a local stiffness/mass/damping matrix the assembler reduces by the constraint map)
- the DOF order is fixed `(Dx, Dy, Dz, Rx, Ry, Rz)` — `Constraint`/`Displacement`/`Force` all index it
  through `GetComponent(DoF)`, and `Constraints.MovementFixed` is the pinned support, `Constraints.Fixed`
  the encastre; the seam `MemberSupport` 6-DOF (from `graph.SupportsOf`) maps directly onto a `Constraint`
- a `BarElement.Behavior` is a `BarElementBehaviour` flag set — `BarElementBehaviours.FullFrame`
  (truss + biaxial Euler-Bernoulli bending + torsion) is the general frame member, `Truss` an axial-only
  member, the `*Timoshenko` variants adding shear deformation; member-end releases ride `Start`/
  `EndReleaseCondition` (a pinned end is a released rotational DOF), so the seam member end-fixity
  (the member's connectivity read from the graph) maps onto behaviour + releases
- a `UniformParametric1DSection(A, Iy, Iz, J)` is the direct mechanical-property section the M7-resolved seam
  `SectionProperties` (Area/Iyy/Izz/J) feeds; `UniformGeometric1DSection` derives the props from a `PolygonYz` outline instead

[SOLVE_TOPOLOGY]:
- `Solve*` assembles the global stiffness from the element local matrices, applies the constraint/MPC
  reduction, and factors the reduced sparse system through the selected `BuiltInSolverType` — every
  concrete solver (`CholeskySolver`/`LuSolver`/`QRSolver`/iterative `PCG`) factors a CSparse system
  (`StaticLinearAnalysisResult.Solvers_New` is keyed by the CSparse `SparseMatrix`), so BFE's linear
  algebra is the same `api-csparse` sparse-factorization owner the Compute numeric lane holds
- the factorization is cached per constraint-map (re-solving additional load cases reuses the factor);
  `Solve(ISolverFactory)` is the injection seam to route through a Rasm-owned CSparse factory rather than
  BFE's built-in one, sharing one factorization owner across the structural and continuum lanes
- CSparse binding seam (version skew): the BFE 2.1.2 nuspecs (core + Common + CustomElements) floor
  `CSparse` at `>= 3.5.0`; the workspace unifies on the central `CSparse 4.4.0` pin (NuGet floor semantics
  + central package management resolve 4.4.0 at restore). BFE's SPARSE path is binary-clean — its
  `CholeskySolver`/`LuSolver`/`QRSolver` wrap CSparse's OWN `SparseCholesky`/`SparseLU`/`SparseQR`
  (`CholeskySolver` → `SparseCholesky.Create(A, ColumnOrdering.MinimumDegreeAtPlusA)`), all present in
  4.4.0. The DENSE path is NOT: `BriefFiniteElementNet.Common` embeds a private
  `CSparse.Double.Factorization.DenseLU : ISolver<double>` compiled against the 3.5-era one-method
  `ISolver<T>` (`Solve(double[], double[])` only); CSparse 4.x widened `ISolver<T>` with an abstract
  `Solve(ReadOnlySpan<double>, Span<double>)` carrying no default interface method, so loading `DenseLU`
  fails interface mapping and the CLR throws `TypeLoadException`. The `DenseColumnMajorStorage<double>`
  `Determinant()`/`Inverse()`/`Solve(double[])` extensions route through `DenseLU` and sit on the
  isoparametric Jacobian-determinant integration path of the 2D/3D `TriangleElement`/`TetrahedronElement`/
  `HexahedralElement`/`QuadrilaturalElement`, so any continuum-element solve breaks under 4.4.0 — confine
  BFE to the sparse-factored `BarElement` frame solve (continuum problems are the `SolveLane`'s), inject a
  Rasm CSparse-4.x `ISolverFactory` via `Solve(ISolverFactory)`, and verify the chosen path loads under 4.4.0
- `Solve_MPC` is the master-slave path for the `MpcElement` family (rigid links, hinges, slaved DOF);
  the `StaticLinearAnalysisResult` carries per-`LoadCase` displacement/reaction/element-force vectors

[STACKING] — the structural-frame lane of the Compute solver:
- `BriefFiniteElement.Net` is the 3D `FrameBackend` arm of `Analysis/structural#FRAME_BACKEND` (the
  `Discipline.Structural` assessment runner): BFE owns the code-checking-grade structural element library
  (exact beam shape functions, member releases, rigid links, plate/shell formulations) the continuum solver
  does not derive, while the generalized `Solver/contract#SOLVE_CONTRACT` `SolveLane` assembles a continuum
  `Bᵀ·D·B` over a `Solver/discretization#DISCRETIZATION_MESH` `DiscreteMesh` — the structural runner
  delegates a continuum field solve to the `SolveLane` rather than BFE re-deriving it, and a hand-rolled
  frame assembler beside BFE is the rejected form
- with the `Rasm.Element` `ElementGraph` (via `Analysis/structural`): the concrete `ElementGraph` (read
  DIRECTLY above the seam, no `IElementProjection`) is the source graph — `StructuralAnalysis.Project` folds
  each member Object node into a `BarElement` (`graph.AxisOf`→member axis), a bounding-surface node into a
  `TriangleElement`/`QuadrilaturalElement`, a point connection into a `Node`,
  `graph.SupportsOf`/`MemberSupport`(6-DOF)→`Node.Constraints`, the seam member end-fixity→behaviour+releases,
  `graph.LoadsOf`/`MemberLoad`+`LoadCombinationSpec`→`LoadCase`+`LoadCombination`; the `StaticLinearAnalysisResult`
  is the structural receipt keyed back by the seam graph content address (`Runtime/codecs#CONTENT_ADDRESSING`)
  so a re-solve runs only on a changed graph
- with `CSparse` (`api-csparse`/`Tensor/factor#SPARSE_SOLVE`): BFE's sparse solvers factor the assembled
  stiffness through CSparse — the same sparse direct-factorization owner — so the structural and continuum
  lanes share one numeric factorization owner; routing BFE's `Solve(ISolverFactory)` through a Rasm CSparse
  factory is the canonical sharing seam, and is also the version-skew mitigation: BFE 2.1.2 binds CSparse
  only at the `>= 3.5.0` floor and its embedded dense `DenseLU` is binary-incompatible with the unified
  `4.4.0` pin (`[SOLVE_TOPOLOGY]`), so BFE stays on the clean sparse-factored frame path and continuum
  elements route to the `SolveLane`
- with section/material sources: `UniformParametric1DSection(A, Iy, Iz, J)` is fed by the M7-resolved seam
  `SectionProperties` (Area/Iyy/Izz/J read off the graph — the `Rasm.Materials` projector's `ProfileRef`→VividOrange
  one-hop baked on the graph, so Compute admits no VividOrange), `UniformIsotropicMaterial(E, ν)` by the seam
  `MaterialPropertySet.Mechanical` — the section/material values are the graph's, never re-typed in the solver
- with `FEALiTE2D` (`api-fealite2d`): the 2D planar twin — `FEALiTE2D` solves planar frames, `BFE` the
  full 3D model; both ride CSparse, and the 2D result renders through `FEALiTE2D.Plotting`
  (`api-fealite2d-plotting`) while the 3D result feeds the AppUi/export rails by content key

[LOCAL_ADMISSION]:
- the structural-frame FEM solve is `BriefFiniteElement.Net`, assembled from the `Rasm.Element` `ElementGraph`
  (via `Analysis/structural`) and solved through a CSparse-backed `BuiltInSolverType` — a hand-rolled
  stiffness assembler or a re-derived beam element beside BFE is the rejected form
- the section/material/load/constraint values map from the seam graph (the M7-resolved `SectionProperties`,
  the seam `MaterialPropertySet.Mechanical`, `graph.LoadsOf`, `graph.SupportsOf`); a re-typed section or
  material in the solver is the rejected form
- the linear solve routes through CSparse (`api-csparse`), shared with the continuum lane via
  `Solve(ISolverFactory)`; a second sparse-factorization owner beside CSparse is the rejected form
- BFE is the structural-frame lane only — continuum multi-physics is the `Solver/contract#SOLVE_CONTRACT`
  `SolveLane`, meshing is `Solver/discretization#DISCRETIZATION_MESH`, code-checking is downstream; none
  is sought from BFE

[RAIL_LAW]:
- Package: `BriefFiniteElement.Net` + `BriefFiniteElementNet.CustomElements` + `BriefFiniteElementNet.Common`
  (2.1.2, LGPL-3.0-only dynamic-linking, pure-managed `lib/net6.0` AnyCPU IL binding forward under net10;
  transitive `CSparse` the solvers factor through)
- Owns: the managed linear-static 3D structural-frame FEM — the `Model`/`Node`/`Element` assembly, the
  `BarElement`/`TriangleElement`/`TetrahedronElement`/`HexahedralElement`/`QuadrilaturalElement`/MPC
  element library, sectioning/material/load-case/combination/6-DOF-constraint inputs, the Cholesky/CG/LU/QR
  solve, and the `StaticLinearAnalysisResult` displacement/reaction/internal-force/stress recovery
- Accept: the 3D `FrameBackend` arm of `Analysis/structural#FRAME_BACKEND` assembled from the `Rasm.Element`
  `ElementGraph` (read directly via `Analysis/structural`), section/material from the M7-resolved seam
  `SectionProperties`/`MaterialPropertySet.Mechanical` baked on the graph, the linear solve factored through
  the shared `CSparse` (`api-csparse`/`Tensor/factor#SPARSE_SOLVE`) owner via `Solve(ISolverFactory)`, the
  result fed to the AppUi/export rails by content key
- Reject: a hand-rolled frame/stiffness assembler or re-derived element beside BFE; a second
  sparse-factorization owner beside CSparse; a re-typed section/material in the solver instead of mapping
  the seam graph; treating BFE as the continuum/multi-physics solver (that is the `SolveLane`) or as a
  mesher (the graph arrives idealized); IL-merging, static-embedding, or vendoring the LGPL source rather
  than referencing the binaries; binding the `net45`/`netstandard2.0` asset (net10 binds `net6.0`)
