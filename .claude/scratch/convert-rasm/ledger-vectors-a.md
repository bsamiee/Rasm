# CAPABILITY LEDGER — lane vectors-a

Scope: `libs/csharp/Rasm/Vectors` odd-rank-by-size files + `_ARCHITECTURE.md`. Size-desc rank (`ls -S`): 1 `Mesh.cs`, 3 `Sample.cs`, 5 `Matrix.cs`, 7 `Flow.cs`, 9 `Align.cs`, 11 `Atoms.cs`, 13 `Space.cs`. `Modes.cs` (rank 12 by bytes / tie-13 by LOC, 146 LOC) ledgered defensively as the size-tie sibling — CONFIRM ownership with vectors-b to avoid a double-claim or a gap.

Physical LOC: Mesh 4445, Sample 1428, Matrix 1092, Flow 743, Align 527, Atoms 285, Space 152, Modes 152. Namespace: all `Rasm.Vectors`. Verdict scale weights DESIGN over function (campaign rebuilds design; function is the asset). "Functionally good, structurally weak" holds: algorithm quality is 9-10 throughout; the design scores below are dragged by file-as-kitchen-sink siting, procedural mega-kernels behind thin ADT façades, receipt-validity copy-paste, and `RhinoMath`-as-numeric-primitive strata coupling.

Sub-domain target vocabulary (from `Rasm/ARCHITECTURE.md` codemap + campaign): `Numerics, Spatial, Meshing, Parametric, Processing, Drawing` (+ `Parametric` per campaign brief). "SHOULD live" columns are a MAP, never a ceiling — the architect rules the final cut.

Shared cross-lane owners referenced by every unit (NOT in this lane, do not re-ledger): `Op` = `Domain/Validation.cs` (the error/receipt key rail, threaded positionally into ~every signature); `GeometryKernel` = `Domain/Geometry.cs` (Rhino closest/normal/frame/sample/curvature); `FieldNabla` = shared Vectors validation kernel (vectors-b, `Field.cs`); `SpectralCore`/`SpectralFilter`/`SpectralBasis`/`DiscreteCalculus` = `Spectral.cs` (vectors-b); `ScalarField`/`VectorField` = `Field.cs` (vectors-b); `VectorCloud`/`CloudKernel`/`CloudNeighborhoodPca*`/`CloudCorrespondence*` = `Cloud.cs` (vectors-b); `ExtractionDomain`/`SdfMeshPolicy`/`SdfMeshMethod`/`SignedHeatTime`/`VolumeGridPolicy`/`Volume*Policy`/`SignConvention` = `Extraction.cs` (vectors-b); `VectorIntent.Project<TOut>` = `Intent.cs` (vectors-b) is the SOLE public consumer rail — every internal entry below is reached only through it.

---

## Atoms.cs (285 LOC, cpx 48) — typed vector-algebra primitives + the projection dispatcher

| Unit | file:line | What it does | Q | Bloat / duplication | Rhino coupling (exact) | SHOULD live |
|---|---|---|---|---|---|---|
| `Dimension` `[ValueObject<int>]` | Atoms.cs:6 | `>=1` validated dimension count | 9 | Thinktecture-generated; clean | `RhinoMath.IsValidDouble` (none — int) | Numerics (primitives floor) |
| `PositiveMagnitude` `[ValueObject<double>]` | Atoms.cs:9 | positive-finite scalar | 9 | clean | `RhinoMath.IsValidDouble/ZeroTolerance` | Numerics |
| `UnitInterval` `[ValueObject<double>]` | Atoms.cs:12 | `[0,1]` scalar | 9 | clean | `RhinoMath.IsValidDouble` | Numerics |
| `BoundarySense` `[SmartEnum<int>]` | Atoms.cs:15 | Toward/Away sign carrier | 8 | trivially small | none | Numerics/Atoms |
| `SignedAxis` `[SmartEnum<int>]` | Atoms.cs:22 | 6 cardinal signed axes; `Of(frame)`, `Cardinal(planar)` filter | 8 | dense; good | `Vector3d.XAxis/…`, `Plane` | Atoms |
| `AnglePivot` `[Union]` | Atoms.cs:36 | World/Frame/Normal angle-measure pivot; `Admit`,`Compute` | 8 | clean ADT | `Vector3d.VectorAngle(…,plane/vNormal)`, `Plane` | Atoms |
| `VectorAngle` `[ValueObject<double>]` | Atoms.cs:53 | `[0,2π]`; `Of(a,b,pivot)`, `Project` | 9 | clean | `RhinoMath.TwoPI/IsValidDouble`, `Vector3d` | Atoms |
| `VectorRelation` `[SmartEnum<int>]` | Atoms.cs:70 | Oblique/Parallel/AntiParallel/Perpendicular classifier | 8 | clean | `Vector3d.IsParallelTo/IsPerpendicularTo` | Atoms |
| `Direction` `record struct` | Atoms.cs:90 | unit vector; `Of`,`Reflect`,`Refract`(Snell),`ParallelTransport(Seq<Plane>)`,operators | 9 | flagship primitive; Refract+transport are real algorithms | `Vector3d`,`Transform.Mirror/PlaneToPlane`,`RhinoMath.SqrtEpsilon` | Atoms |
| `VectorSpan` `record struct` | Atoms.cs:128 | anchor+dir+magnitude; `Components(frame)`,`Axis`(Line),`Project` | 9 | clean | `Point3d`,`Line`,`Plane` | Atoms |
| `VectorFrame` `record struct` | Atoms.cs:161 | orthonormal frame; `Of(origin,normal,xHint)`,`Chain`(Bishop→`CloudKernel.BishopChainOf`),`SeedPerpendicular`,`Project`→Plane/Transform | 9 | clean; leans on Cloud for Bishop chain | `Plane`,`Point3d`,`Vector3d`,`Transform.PlaneToPlane` | Atoms |
| `VectorCone` `record struct` | Atoms.cs:187 | apex+axis+halfAngle; `SolidAngle`,`Contains`,`Enclose`(cone-merge),`PartitionBy(sectors)` | 9 | dense; Enclose is a real envelope solve | `Point3d`,`Transform.Rotation`,`RhinoMath.TwoPI` | Atoms |
| `ProjectionRow` + `AtomProjection` `internal static` | Atoms.cs:242 | THE raw→typed-output dispatch rail: `Rows/Self/Value/SelfOrValue/Values/Custom/Raw` type-switch over Vector3d/Direction/Plane/VectorFrame/double/Circle/Point3d/Matrix/SymmetricMatrix/VectorAngle | 9 | genuine canonical owner, MISNAMED as an internal helper buried in Atoms; every projection in every lane file routes through it | 12+ Rhino primitive type-checks | **promote to a named projection owner** (Numerics/core) |

Design note: Atoms is the healthiest file — dense value objects, real algorithms (Snell refract, Bishop transport, cone envelope), one polymorphic projection rail. Its ONLY structural sin is that `AtomProjection` (the corpus-wide output dispatcher) hides as an `internal static class` instead of being a first-class owner.

---

## Space.cs (152 LOC, cpx 47) — closest-hit support-geometry projection

| Unit | file:line | What it does | Q | Bloat / duplication | Rhino coupling (exact) | SHOULD live |
|---|---|---|---|---|---|---|
| `SupportProjection` `[SmartEnum<int>]` | Space.cs:6 | 14 closest-hit projections (Closest/Direction/Span/Normal/Distance/Parameter/Uv/Component/MeshPoint/SignedDistance/ContainmentDistance/Tangent/Frame/SignedSpanAway) w/ capability+accepts+projectRaw delegates; `Project<TOut>` gate | 9 | exemplary dense polymorphic surface — one enum owns all 14 modalities, capability-gated | `Point3d`,`Vector3d`,`Plane`,`Line` via `ClosestHit` | Spatial |
| `SupportSpace` `record struct [BoundaryAdapter]` | Space.cs:81 | wraps ANY Rhino geometry OR cloud cluster; `Of`,`Closest`,`SignedDistance`,`ContainmentDistance`, `Admits*` predicates | 9 | dense boundary adapter; the polymorphic `Value:object` + `SourceType` switch is honest | HEAVY: `Brep.IsPointInside`, `Mesh.IsPointInside`, `Plane/Sphere/Box/BoundingBox/GeometryBase`, `GeometryKernel.*` | Spatial |
| `SurfaceSpace` `record struct [BoundaryAdapter]` | Space.cs:135 | wraps Rhino `Surface`; `Of`(validate),`Sample<TOut>(projection,u,v)` | 8 | thin; delegates to SurfaceProjection (Modes.cs) | `Surface` | Spatial or Parametric |

Design note: `SupportProjection` is a model of what the campaign wants — one SmartEnum, 14 capability-gated cases, single `Project<TOut>`. Rhino coupling is intrinsic here (closest-point IS a Rhino query) so it is correctly a `[BoundaryAdapter]` at the edge. Clean split candidate: `SurfaceSpace` naturally belongs with the curve/surface parametric family (Modes.cs), not with the proximity family — they are welded here only by filename.

---

## Modes.cs (152 LOC, cpx 14) — TIE SIBLING (confirm lane ownership) — curve/surface/cone parametric selectors

| Unit | file:line | What it does | Q | Bloat / duplication | Rhino coupling (exact) | SHOULD live |
|---|---|---|---|---|---|---|
| `CurveProjection` `[SmartEnum<int>]` | Modes.cs:5 | 9 curve evaluations (Tangent/Curvature/Frame/PerpendicularFrame/ArcLength/FrameNormal/FrameBinormal/PerpendicularNormal/PerpendicularBinormal); `Project<TOut>` | 9 | dense; `CurveFrameProjection` factory collapses 5 frame cases | `Curve.TangentAt/CurvatureAt/FrameAt/PerpendicularFrameAt/GetLength`, `Interval` | Parametric |
| `SurfaceProjection` `[SmartEnum<int>]` | Modes.cs:44 | 13 surface evaluations (PrincipalCurvatures/Gaussian/Mean/Max+MinOsculatingCircle/Normal/ShapeOperator/Point/Frame/UvFrame/Jacobian/Metric/AreaScale); ShapeOperator→`SymmetricMatrix` | 9 | dense; `Derivatives` factory shares Du/Dv cases | `Surface.CurvatureAt/Evaluate/PointAt`,`SurfaceCurvature`,`Circle`,`Point2d` | Parametric |
| `ConeProjection` `[SmartEnum<int>]` | Modes.cs:121 | HalfAngle/SolidAngle/Axis/Apex of `VectorCone` | 8 | trivial | none | Parametric/Atoms |
| `MotionInterpolation` `[SmartEnum<int>]` | Modes.cs:132 | Linear/Slerp plane interpolation via quaternion | 8 | clean | `Quaternion.Rotation/Slerp/Lerp`,`Plane` | Parametric/Atoms |

---

## Align.cs (527 LOC, cpx 82) — point-cloud ICP registration

| Unit | file:line | What it does | Q | Bloat / duplication | Rhino coupling (exact) | SHOULD live |
|---|---|---|---|---|---|---|
| `AlignmentStopKind`,`AlignmentOptimizerStopKind` `[SmartEnum]` | Align.cs:9,16 | convergence-stop vocabularies | 8 | trivial | none | Processing (or Registration) |
| `AlignKind` `[SmartEnum<int>]` | Align.cs:23 | 6 ICP variants (Point/Plane/Symmetric/Robust/NormalWeightedPointToPlane/Generalized) w/ `NeedsTargetNormals`/`NeedsCovariances` flags + `solveStep` delegate + `AlignDetailed` entry | 9 | flagship polymorphic ICP dispatcher; one enum owns all 6 solves | via kernel | Processing |
| `AlignmentPolicy` `record struct` | Align.cs:60 | MaxIterations/ConvergenceTolerance/RobustScale/CovarianceRidge + `Admit` | 9 | clean; `Default` preset | `RhinoMath` | Processing |
| `AlignmentRobustReceipt`/`AlignmentOptimizerReceipt`/`AlignmentReceipt`/`AlignmentMatch`/`AlignmentStep` records | Align.cs:73-99 | typed ICP receipts + `Project<TOut>`(Transform gated on Converged) | 9 | `AlignmentOptimizerReceipt.IsValid` is a 15-term `RhinoMath.IsValidDouble && >=0` litany — receipt-validity copy-paste | `Transform`,`RhinoMath` | Processing |
| `AlignKernel` `internal static` | Align.cs:110 | `AlignClouds`/`IcpAlign` outer loop (fold over iterations); `SolvePointToPoint`(Umeyama-1991 SVD Procrustes), `SolvePointToPlane`(Chen-Medioni-1992 linearized), `SolveSymmetric`(Rusinkiewicz-2019 oriented-normal-sum), `SolveRobustProcrustes`(MAD-scaled Welsch IRLS), `SolveNormalWeightedPointToPlane`, `SolveGeneralizedIcp`(GICP Mahalanobis precision field + Armijo line search + SE(3) jacobian), `FindCorrespondences`(RTree KNN), cross-covariance SVD, weighted centroid | 9 | world-class ICP; the 6 solvers share `SolveLinearizedRows`/`SolveProcrustes` well; `DeltaMagnitude`/`ComposeRigidTransform` are honest statement kernels | `Point3d`,`Vector3d`,`Transform`,`RTree.PointCloudKNeighbors`,`RhinoMath`; **MathNet direct** (`DenseMatrixD`,`DenseVectorD`,`Evd`,`Cholesky`) at Align.cs:483-497 | Processing |

Design note: functionally elite (6 literature-grade ICP variants behind one SmartEnum). Structural drag: `AlignKernel` reaches PAST the `Matrix`/`SymmetricMatrix` owners straight into raw MathNet (`GicpPrecisionOf`, `RebuildFromSpectrum` use `LinearMatrix`/`Evd`/`Cholesky` directly) — a second linear-algebra access path bypassing the file's own canonical `Matrix` rail. Should route through Matrix.cs owners or the coupling should be ruled.

---

## Flow.cs (743 LOC, cpx 156) — RK streamline integration over vector fields

| Unit | file:line | What it does | Q | Bloat / duplication | Rhino coupling (exact) | SHOULD live |
|---|---|---|---|---|---|---|
| `IntegratorKind` `[SmartEnum<int>]` | Flow.cs:6 | 9 Butcher tableaus (Euler/Heun/Midpoint/Ralston/RK4/RK38/BogackiShampine/CashKarp/DormandPrince) via `Fixed`/`Adaptive` factories | 10 | exemplary data-driven vocabulary | `RhinoMath` (validity only) | Processing/Numerics |
| `FieldIntegrator` `[Union]` | Flow.cs:50 | Fixed/Adaptive; `Step`(RK stage compute + adaptive PI error control + dense output) | 9 | clean ADT | `Point3d`,`Vector3d`,`RhinoMath.ZeroTolerance` | Processing |
| `Termination` `[Union]` | Flow.cs:155 | 6 stops (StepCount/ArcLength/MagnitudeFloor/CrossSurface/RegionThreshold/LoopDetected); `Evaluate` w/ bisection + dense-output root event localization | 9 | dense; event bracketing is real | `Point3d`,`Vector3d`; `SupportSpace`/`ScalarField` refs | Processing |
| `StreamlineStopKind`/`TraceEventKind`/`TraceEventStatus`/`TraceEventLocalizationKind` `[SmartEnum]` | Flow.cs:128-153 | event vocabularies | 8 | trivial | none | Processing |
| `ButcherTableau` `record struct` | Flow.cs:292 | coupling/abscissae/weights/embedded; `IsValid`(order-condition MOMENT checks), `MomentReceipt`, dense-output | 10 | genuinely deep — validates RK order conditions numerically | `RhinoMath` | Numerics |
| `ButcherMomentReceipt`/`DenseOutputReceipt`/`TraceEvent`/`StreamlineTrace` records | Flow.cs:343-453 | typed receipts w/ deep `IsValid` gates | 9 | more `RhinoMath.IsValidDouble && >=0` litanies (StreamlineTrace.IsValid ~15 terms) | `Point3d`,`RhinoMath` | Processing |
| `DenseOutputCoefficientFamily` `[SmartEnum<int>]` | Flow.cs:353 | GenericMomentFit/DormandPrinceShampine/BogackiShampine continuous-extension coefficient tables + Horner eval | 10 | specialized interpolants w/ exact rational tables | `RhinoMath` | Numerics |
| `ButcherDenseOutput` `internal static` | Flow.cs:456 | dense-output receipt derivation; moment design/preimage/correction least-squares (via `Matrix.LeastSquaresDetailed`) | 9 | correctly routes through Matrix owner | `RhinoMath` | Processing |
| `DenseOutputState`/`StreamlineStep`/`StreamlineState` records | Flow.cs:605-673 | immutable integration state w/ `Accept`/`Reject`/`Step` folds | 8 | clean | `Point3d`,`Vector3d` | Processing |
| `FlowKernel` `internal static` | Flow.cs:675 | `Trace<TOut>`(streamline tracer over VectorField+integrator+termination), `ProjectTrace`→Seq<Point3d>/Polyline/Curve, `TraceState` loop, `AdvanceState` | 9 | flagship entry; `Combine(coeffs,vectors)` DUPLICATED at Flow.cs:122 (FieldIntegrator) and Flow.cs:633 (DenseOutputState) | `Point3d`,`Polyline`,`Curve.ToPolylineCurve`,`RhinoMath`; hardcoded `MaxIterations=100000` const | Processing |

Design note: the Butcher/dense-output machinery (IntegratorKind, ButcherTableau, DenseOutputCoefficientFamily, ButcherDenseOutput) is PURE numerics with zero geometric content but is welded to `Point3d`/`Vector3d`/`RhinoMath`. The tableau+order-condition validation belongs at a host-neutral Numerics floor; only `FlowKernel`/`FieldIntegrator.Step` (which sample a `VectorField` at `Point3d`) are geometry-coupled.

---

## Matrix.cs (1092 LOC, cpx 262) — dense + sparse linear algebra kernel

| Unit | file:line | What it does | Q | Bloat / duplication | Rhino coupling (exact) | SHOULD live |
|---|---|---|---|---|---|---|
| `EigenSolvePath`/`EigenSolveStop`/`MatrixNormKind`/`SolvePath`/`SolveStop`/`GaugeSolverKind`/`GaugeShift` `[SmartEnum]` | Matrix.cs:18-86 | solver-path + stop + norm-kind vocabularies | 8-9 | clean data | `RhinoMath` | Numerics |
| `GaugePolicy` `[Union]` | Matrix.cs:88 | Pin/MeanZeroDeflation/LagrangeKKT singular-system gauge fixing + `PinConstant`/`MeanZeroConstant`/`KktConstant` factories | 9 | clean ADT + `Switch` dispatch | none | Numerics |
| `Matrix` `record struct [BoundaryAdapter]` | Matrix.cs:120 | dense matrix; `Of`/`Identity`/`Transpose`/`Multiply`/`Inverse`/`PseudoInverse`/`DecomposeEigen`/`Lu`/`Qr`/`Svd`/`Norm`/`Trace`/`Determinant`/`Spectral`/`Solve`/`LeastSquares`/`Rank`/`At`/`With` | 9 | dense façade over MathNet; row-major `Arr<double>` storage | `RhinoMath.IsValidDouble` as finiteness primitive | Numerics |
| `SymmetricMatrix` `record struct [BoundaryAdapter]` `: IValidityEvidence` | Matrix.cs:164 | packed-upper symmetric; `ToDense`/`DecomposeEigen`/`DecomposeCholesky`/`At`/`With` | 9 | **the ONLY receipt/model that uses the `IValidityEvidence` interface** — proof the shared-validity pattern exists but is unapplied to ~40 other receipts | `RhinoMath` | Numerics |
| `SparseMatrix` `record struct [BoundaryAdapter]` | Matrix.cs:189 | CSR; `FromTriplets`/`Multiply`/`ToDense`/`Solve`/`SingularSolve`/`SolveIndefinite`/`SmallestEigenpairs` + CSR invariant checks (`RowPointersAreMonotone`/`RowColumnsAreStrict`) | 9 | dense; invariant checks are honest | `RhinoMath` | Numerics |
| `SparseHermitian` `record struct [BoundaryAdapter]` | Matrix.cs:221 | complex Hermitian CSR; `FromTriplets`/`Multiply`/`SmallestEigenpairs`; upper-store + conjugate reconstruct | 9 | clean | `System.Numerics.Complex`,`RhinoMath` | Numerics |
| `SolveReceipt`/`EigenSolveReceipt<T,V>`/`GaugeReceipt`/`SvdResult`/`LuResult`/`QrResult`/`CholeskyResult` records | Matrix.cs:239-346 | typed decomposition receipts w/ `IsUsable`/`IsValid` gates | 9 | `GaugeReceipt.IsValid` = ~18-term litany; `SolveReceipt.IsUsable` ~12 terms — receipt-validity copy-paste | `RhinoMath`; `MathNet.*.Factorization.LU/Cholesky` held in LuResult/CholeskyResult | Numerics |
| `CholeskySparse` `sealed record [BoundaryAdapter]` | Matrix.cs:348 | CSparse.NET SPD Cholesky (AMD ordering) w/ `Lock`-guarded `Solve`; `Of`/`SolveDetailed`/`FactorNonZeros` | 9 | thread-safe factor cache; the memoized SPD substrate the whole DDG kernel rides | `CSparse.Double.Factorization.SparseCholesky`,`RhinoMath` | Numerics |
| `MatrixKernel` `internal static` | Matrix.cs:384 | full MathNet+CSparse bridge: `ToMathNet`/`FromMathNet`, dense `Svd`/`Lu`/`Qr`/`Cholesky`/`SymmetricEigen`/`GeneralEigen`, dense solves, sparse assembly+matvec, `SparseSolve`(BiCgStab + MathNet-direct fallback w/ typed receipts), `SparseLuSolve`(CSparse indefinite), `SingularGaugeSolve`(pin/deflation/KKT + M-orthogonal projection + Tikhonov shift + `GaugeShift` post-shift), `GeneralizedEigenpairsDetailed`(Cholesky congruence), `Lobpcg`+`LobpcgHermitian`(Knyazev-2001 w/ Rayleigh-Ritz, MGS reorthonormalization, Jacobi precond, splitmix64 PRNG basis, survivor-column deflation) | 10 | world-class; "local LOBPCG WITHOUT hidden dense fallback" is a real correctness stance; splitmix64 PRNG (`NextSignedUnit` Matrix.cs:1017) DUPLICATES the hash pattern in Sample.cs `CandidateOrderKey`/`UnitInterval` | `RhinoMath.IsValidDouble/SqrtEpsilon/ZeroTolerance` used pervasively as finiteness/epsilon primitives in a PURE-numeric kernel with zero geometry; `#pragma warning disable CA1031` at :810 for MathNet bare-Exception Cholesky | Numerics |

Design note: the single cleanest strata violation in the lane — a 1092-LOC dense/sparse/complex linear-algebra kernel (MathNet + CSparse) with ZERO geometric content, welded to RhinoCommon solely through `RhinoMath.IsValidDouble` (= `double.IsFinite`) and `RhinoMath.SqrtEpsilon`/`ZeroTolerance` (owned epsilon constants). This belongs at a host-neutral Numerics floor.

---

## Sample.cs (1428 LOC, cpx 271) — point sampling / distribution kernel

| Unit | file:line | What it does | Q | Bloat / duplication | Rhino coupling (exact) | SHOULD live |
|---|---|---|---|---|---|---|
| `DworkSamplingDomain`/`SampleAlgorithmKind`/`SampleDomainStatus`/`SampleStopKind`/`PowerCcvtGauge`/`PowerCcvtStopKind` `[SmartEnum]` | Sample.cs:6-59 | sampling vocabularies; `PowerCcvtGauge.Policy` composes `GaugePolicy` (item-1 reuse, no parallel solve) | 8-9 | clean | `RhinoMath` | Processing |
| `SampleKind` `[Union]` | Sample.cs:61 | 11 cases (Explicit/PoissonDisk/Farthest/Optimize/Lloyd/Capacity/Weighted/ScalarDensity/Adaptive/SampleElimination/DworkVariableDensity/PowerCcvt) + factories + `Admit` + `Evaluate` + `Project<TOut>` (→Seq<Point3d>/VectorCloud/PointCloud/SampleReceipt) | 9 | flagship polymorphic sampler; BUT `PowerCcvtCase` ctor takes **22 params** (Sample.cs:74) + `PowerCcvt` factory 24-arg (Sample.cs:133) = raw knob-spam, the ANTITHESIS of a dense internalized entry | `Point3d`,`RhinoMath` | Processing |
| `DworkReceipt`/`PowerCellFragmentFacts`/`PowerCcvtReceipt`/`SampleAlgorithmReceipt`/`SampleReceipt` records | Sample.cs:225-283 | typed sampling receipts; `PowerCcvtReceipt` has ~40 fields + ~35-term `IsValid` | 9 | receipt-validity copy-paste at extreme scale | `RhinoMath` | Processing |
| `SampleKernel` `internal static` (dispatch) | Sample.cs:292 | `Sample` dispatch over support/mesh/cloud/candidate domains; `SampleAdmitted`; `SampleOnCandidates` (11-arm switch → per-algorithm selection) | 9 | correct polymorphic dispatch | `Point3d`,`Mesh.ClosestMeshPoint`,`MeshPoint`,`AreaMassProperties.Compute` | Processing |
| `PowerCcvtRun` `sealed class` (de Goes BNOT) | Sample.cs:431 | continuous power-CCVT: gauge-fixed ENFORCE-CAPACITY concave Newton (composes `SingularSolveDetailed`+`GaugePolicy.MeanZeroConstant`) → two-phase site motion (Lloyd `q←b` + Armijo transport-energy gradient ascent) → `Schedule.recurs`+`Atom`+`IO.lift` convergence → regularity break → surface lift | 10 | extraordinary depth (de Goes-2012 BNOT); honest `IO.lift`/`Atom` LanguageExt-native outer loops, NOT raw counters; consumes `RestrictedPowerDiagram` from Mesh.cs | `Point3d`,`Vector3d`,`Plane.FitPlaneToPoints`,`Mesh.ClosestMeshPoint`,`AreaMassProperties`,`BoundingBox`,`RhinoMath` | Processing (+ Meshing for power-cell dep) |
| `DworkMeshRun` `sealed class` + `DworkVariableDensitySelection` | Sample.cs:932-1039,1040 | Dwork variable-density Poisson-disk on mesh surface + on candidate cloud; spatial-hash annulus band `[r,2r]`, deterministic seed | 10 | dense; own spatial-hash grid (`DworkCell`) | `Mesh`,`MeshFace`,`MeshPoint`,`Vector3d`,`BoundingBox`,`Point3dList` | Processing |
| `PoissonDiskSelection`/`FarthestIndices`/`FpoSample`/`RelaxationSample`/`CapacityCvtSelection`/`DensitySelection`/`PrioritySelection`/`SampleElimination` | Sample.cs:1226-1387,899,1278 | Bridson active-list Poisson, farthest-point + FP-optimize, Lloyd/capacity relaxation, weighted priority density, Yuksel-2015 weighted sample elimination | 10 | complete classical sampler suite; `MeanSpacingOf`/hexagonal-packing-reference spacing recomputed in ~3 sites | `Point3d`,`PointCloud.ClosestPoint`,`BoundingBox`,`RhinoMath` | Processing |
| `CandidateOrderKey`/`UnitInterval` `static` (PRNG) | Sample.cs:1332,1343 | deterministic splitmix64-style point hash → order key + unit interval | 9 | DUPLICATES the splitmix64 PRNG in Matrix.cs `NextSignedUnit`; both hand-roll the same mixing constants | `Point3d`,`BitConverter`,`RhinoMath` | Numerics (shared PRNG owner) |

Design note: the single richest algorithm file in the lane (BNOT optimal transport, 4 Poisson variants, Yuksel elimination, Dwork variable-density). The `PowerCcvtCase` 22-param constructor is the textbook violation of the CLAUDE.md "dense entry, no knob spam, internalized intelligence" mandate — these must collapse into a small preset set + one advanced override on rebuild.

---

## Mesh.cs (4445 LOC, cpx 1259) — the giant: DDG mesh-operator kernel

The single largest structural liability in the conversion. ONE file + ONE `MeshKernel` static class jams ~30 distinct algorithm families. Ledgered by family (per-member is the wrong granularity for a map at this scale). Section anchors verified via `rg`.

### Public TYPES/MODELS (vocabularies, policies, receipts)

| Unit | file:line | What it does | Q | Bloat / notes | Rhino coupling | SHOULD live |
|---|---|---|---|---|---|---|
| `MeshDescriptor` `[Union]` (single `SpectralCase`) | Mesh.cs:9 | HKS/WKS spectral descriptors via `SpectralFilter` | 9 | intentionally one case (not full ShapeDNA) | `SpectralFilter` (vectors-b) | Processing |
| `MeshLaplacian` `[SmartEnum]` | Mesh.cs:18 | Cotangent/IntrinsicDelaunay/TuftedIntrinsic selector → `cache.Select` | 9 | clean | via cache | Meshing |
| `MeshFeatureAlgorithm`/`MeshFeatureKind`/`MeshSamplingSpectrumAlgorithm`/`MeshSegmentationAlgorithm`/`MeshSegmentationStatus`/`SdfMeshDomain`/`SdfMeshStatus`/`TangentLogMapAlgorithm`/`GeodesicStopKind`/`SignpostEncoding`/`SignpostGauge`/`QuadGuideInfluence`/`QuadPreserveEdges`/`RemeshStatus`/`PowerDensityPolicy` `[SmartEnum]` | Mesh.cs:26-180,673 | ~15 algorithm/status/encoding vocabularies | 8 | correct data-driven vocabularies; but ~15 tiny enums in one file signals the file spans ~15 concerns | none/minimal | split w/ their owning algorithm |
| `MeshSegmentation` `[Union]` | Mesh.cs:44 | 6 cases (ScalarThreshold/ScalarBands/SeededRegionGrow/DescriptorClusters/Watershed/NormalizedCut) + factories | 9 | dense ADT; factory validation inline | `RhinoMath` | Processing |
| `QuadTarget`/`RemeshKind` `[Union]` | Mesh.cs:104,121 | quad/simplify remesh targets + factories | 9 | clean | `Curve`,`ReduceMeshParameters`,`QuadRemeshSymmetryAxis` | Processing |
| `MeshSpace` `record struct [BoundaryAdapter]` | Mesh.cs:184 | validated mesh snapshot (`DuplicateMesh`); `Cache`→`LaplacianCache.For`, `Laplacian`, `DuplicateNative` | 9 | the mesh boundary handle; snapshots defensively | `Mesh.IsValid/DuplicateMesh` | Meshing |
| Policies: `TuftedCoverPolicy`/`SignpostPolicy`/`GeodesicTracePolicy`/`WindowPropagationPolicy`/`MeshFeaturePolicy` records | Mesh.cs:240-365 | validated algorithm policies w/ `Default` presets + `Of` admission | 9 | good — knobs live in policy records (contrast SampleKind) | `RhinoMath` | with owning algorithm |
| ~25 receipts: `TopologyReceipt`,`TuftedLaplacianReceipt`(~40 fields),`SparseLaplacian`,`SpectralBasisBundle`,`DescriptorReceipt`,`DescriptorResult`,`MeshSamplingSpectrumReceipt`,`FeatureEdge`,`FeatureReceipt`,`FlattenReceipt`,`FlattenResult`,`MeshSegmentationReceipt`,`MeshSegmentationResult`,`RemeshReceipt`,`RemeshResult`,`SignedHeatReceipt`,`VolumeGridReceipt`,`SdfMeshReceipt`,`SdfMeshSample`,`CommonSubdivision`,`SignpostTransportReceipt`,`HodgeDecompositionReceipt`,`TangentLogMapReceipt`,`TangentLogMapResult`,`TuftedBaseFaces` | Mesh.cs:203-480 | typed evidence carriers w/ deep `IsValid` gates + `Project<TOut>` rows | 9 | the receipt-validity swarm at its worst: `TuftedLaplacianReceipt.IsValid` (~35 terms), `HodgeDecompositionReceipt.IsValid`, `CommonSubdivision.IsValid` (partition-of-unity gate) — ALL hand-rolled `RhinoMath.IsValidDouble && >=0` | `Mesh`,`Point2d`,`BoundingBox`,`RhinoMath` | with owning algorithm |
| `PowerFacet`/`PowerCell`/`RestrictedPowerReceipt`/`RestrictedPowerDiagram` | Mesh.cs:654-716 | Laguerre power-diagram-restricted-to-mesh; consumed ONLY by Sample.cs `PowerCcvtRun` | 10 | correct cross-file coupling but siting is arbitrary (defined in Mesh, used in Sample) | `Point3d`,`RhinoMath` | Meshing (shared owner) |

### Internal owners + operation families (`MeshKernel`, `LaplacianCache`, `IntrinsicMesh`)

| Family (section) | file:line | What it does | Q | Rhino coupling | SHOULD live |
|---|---|---|---|---|---|
| `LaplacianCache` `sealed class` | Mesh.cs:528 | per-mesh-snapshot memoization (`ConditionalWeakTable` keyed by Mesh identity + `Atom<HashMap>` `Memo<K,T>`) of cotangent/IDT/tufted Laplacians, scalar/connection/edge Cholesky factors, spectral bases, intrinsic snapshots, geodesic/MCF/crossfield/Hodge/vectorheat/signed-heat caches; `SpdMassShift` mesh-scale Tikhonov | 10 | the flagship memoization substrate the entire DDG kernel rides; subtle GC/identity + `Lock` semantics MUST survive verbatim | `Mesh`,`RhinoMath`,`ConditionalWeakTable` | Meshing (as a service) |
| `IntrinsicMesh` `sealed class` + `IntrinsicEdge` | Mesh.cs:1147,1153 | post-IDT-flip frozen intrinsic triangulation: normal coordinates, edge index, face-edge map, Heron face areas, first-incident edge; `FromMesh`/`Freeze`/`EdgeAt`/`IndexOfEdge` | 10 | INTERNAL to Vectors; the `MeshAdjointSnapshot` public handle (per `_ARCHITECTURE`) wraps it for Rasm.Compute | `Mesh`,`MeshFace`,`Point3d`,`RhinoMath` | Meshing |
| POWER_CELLS: `RestrictedPowerCells` | Mesh.cs:855 | Laguerre diagram restricted to mesh: Sutherland-Hodgman radical clip + FIFO incident-pair frontier + shoelace area/first-moment; origin-shifted weighted sites | 10 | named statement-kernel exemption (honest) | `Mesh`,`BoundingBox`,`Point3d`,`Vector3d` | Meshing |
| COTANGENT_ASSEMBLY: `AssembleCotangent`, `LaplacianTriplets` | Mesh.cs:1107,1109 | cotangent stiffness + consistent/lumped mass assembly | 9 | | `Mesh`,`MeshFace` | Meshing |
| IDT_AND_NONMANIFOLD: `BuildIntrinsicMesh`,`FrozenIntrinsicFor`,`FromMesh`,`FlipToDelaunay`,`AssembleCotangentFromIntrinsic`,`AssembleTuftedCotangentFromIntrinsic`,`TuftedCoverMesh.Construct` | Mesh.cs:1135-1459 | intrinsic-Delaunay edge flipping + Sharp-Crane tufted-intrinsic cover assembly | 10 | | `Mesh`,`RhinoMath` | Meshing |
| NORMAL_COORDINATES (FLIP-N) | Mesh.cs:1347 | intrinsic edge-flip normal-coordinate bookkeeping (Gillespie-Sharp-Crane) | 10 | | | Meshing |
| SELECTION/SPD_PIN: `LaplacianOf`,`AssembleMassStiffnessSystem` | Mesh.cs:1803,1813 | Laplacian selection + `(M + tL)` SPD system assembly | 9 | | | Meshing |
| SPECTRAL_BASIS: `ComputeSpectralBasisDetailed` | Mesh.cs:1823 | generalized eigenbasis (via Matrix LOBPCG/congruence) | 9 | | | Meshing/Processing |
| METRICS: `TopologyDetailed`,`DetectFeatureEdgesDetailed`,`ParameterizeFlattenDetailed` | Mesh.cs:1853,1875,1961 | topology (Euler/genus/manifold), dihedral+curvature feature edges, LSCM-style unwrap/flatten | 9 | `Mesh.GetNakedEdges/SolidOrientation/Unwrap`,`AreaMassProperties` | Processing |
| HEAT_METHOD: `HeatGeodesicAt` | Mesh.cs:2028 | Crane-Weischedel-Ovsjanikov heat-method geodesic distance | 10 | | Processing |
| MEAN_CURVATURE_FLOW: `MeanCurvatureMagnitudeAt` | Mesh.cs:2078 | implicit MCF | 9 | | Processing |
| TANGENT_FRAMES/SIGNPOST_ANGLES: `SignpostTransportReceiptOf` | Mesh.cs:2113,2138 | Sharp-Crane signpost + normal-coordinate intrinsic transport | 10 | | Meshing |
| COMMON_SUBDIVISION: overlay(M,T) | Mesh.cs:2298 | common-subdivision overlay + interpolation matrices (partition-of-unity gated) | 10 | | Meshing |
| CROSS_FIELD: `CrossFieldAt`,`BuildConnectionLaplacianRealSystem` | Mesh.cs:2489,2466 | Knöppel-Crane globally-optimal direction fields (cone/constraint variants) | 10 | `System.Numerics.Complex` | Processing |
| REMESH: `ApplyRemeshDetailed` | Mesh.cs:2618,2619 | QuadRemesh + ReduceMesh (Rhino native) | 9 | `Mesh.QuadRemesh/Reduce`,`ReduceMeshParameters` | Processing |
| DESCRIPTORS: `DescribeShape`/`DescribeSpectralShape`/`SpectralDistanceAt` | Mesh.cs:2677-2701 | HKS/WKS spectral shape descriptors + spectral distance | 9 | | Processing |
| SEGMENTATION: `Segment<TOut>` | Mesh.cs:2707,2708 | 6-algorithm dispatch incl. normalized-cut spectral clustering + watershed basins | 10 | | Processing |
| MESH_SPECTRUM_SAMPLING: `ValidateSamplingSpectrum` | Mesh.cs:3059,3060 | low-frequency spectral blue-noise validation of sample sets (feeds Sample.cs) | 9 | | Processing |
| HODGE_DECOMPOSITION: `HodgeProjected` | Mesh.cs:3107,3125 | harmonic/exact/coexact Hodge split w/ genus-aware harmonic basis | 10 | | Processing |
| VECTOR_HEAT: `VectorHeatAt` | Mesh.cs:3235,3237 | Sharp-Soliman-Crane vector heat method (parallel transport of tangent data) | 10 | | Processing |
| GEODESIC_TANGENT/TANGENT_LOG_MAP: `GeodesicTangentAt`,`TangentLogMapAt`,`ExactExpMapAt` | Mesh.cs:3303-3925 | log map (VectorHeatApproximate / ExactStraightestExp / ExactWindowPropagation) | 10 | | Processing |
| WINDOW_PROPAGATION: (MMP) | Mesh.cs:3431 | Surazhsky-Surazhsky exact MMP geodesic windows | 10 | | Processing |
| BACKTRACE_GEODESIC_BVP / STRAIGHTEST_GEODESIC_EXP | Mesh.cs:3635,3918 | geodesic BVP backtrace + straightest-geodesic exp tracing | 10 | | Processing |
| STRIPE_PATTERN: `StripeAt` | Mesh.cs:4132,4134 | Knöppel-Crane stripe patterns | 9 | | Processing |
| SDF_FROM_MESH: `SignedDistanceFromMeshDetailed`,`PrewarmSignedDistanceEvaluator`,`ComputeSignedHeatDetailed`,`ComputeClosedSignedHeatDetailed` | Mesh.cs:4151-4232 | 3 SDF methods: GWN solid-angle winding (Mesh.cs:4177), boundary-source signed heat (CR edge sources, unflipped-only), closed-surface volumetric signed-heat (regular-grid Poisson SHM, Mesh.cs:4232) | 10 | `Mesh.IsPointInside/SolidOrientation/GetNakedEdges/ClosestPoint`,`BoundingBox`,`Point3dList`; hardcoded `VolumeGridMaxNodes=1_000_000`,`AspectRatioCeiling=11.5` | Processing (+ Meshing for grid) |

---

## CROSS-CUTTING FINDINGS

### Entry-point rot
- No unifying Vectors entry exists in-lane. Every capability exposes an `internal static Fin<T> XxxAt(MeshSpace,…,Op key)` (Mesh) / `Xx.Project<TOut>` (Atoms/Space/Modes/Sample/Align) / `FlowKernel.Trace<TOut>` — all reached ONLY through `VectorIntent.Project<TOut>` in `Intent.cs` (vectors-b). Each of my 8 files is a silo stitched together off-lane. The new sub-domain partition needs ONE dense polymorphic entry PER sub-domain, not N internal statics behind a cross-lane dispatcher.
- `SampleKind.PowerCcvt` (22-param ctor / 24-arg factory, Sample.cs:74,133) is the flagrant anti-pattern vs. the CLAUDE.md "dense entry, no knob spam, internalized intelligence" mandate. Contrast the Mesh policy records (`TuftedCoverPolicy`/`SignpostPolicy` etc.) which correctly encapsulate knobs behind `Default` presets — the good pattern already exists in the same lane; Sample must adopt it.

### Parallel rails
- TWO linear-algebra access paths: the canonical `Matrix`/`SymmetricMatrix` owners (Matrix.cs) AND direct raw-MathNet reach in `AlignKernel.GicpPrecisionOf`/`RebuildFromSpectrum` (Align.cs:481-501, `DenseMatrixD`/`Evd`/`Cholesky`). Align bypasses its own file's canonical matrix rail.
- TWO splitmix64 PRNGs: `MatrixKernel.NextSignedUnit` (Matrix.cs:1017, LOBPCG basis) and `SampleKernel.CandidateOrderKey`/`UnitInterval` (Sample.cs:1332,1343) hand-roll the same mixing constants independently. One deterministic-hash owner should serve both.
- `Combine(coefficients, vectors)` DUPLICATED verbatim inside Flow.cs (`FieldIntegrator` :122 and `DenseOutputState` :633).
- Hexagonal-packing reference-spacing (`sqrt(2·area/(sqrt3·n))`) recomputed in ~3 Sample.cs sites (`MeanSpacingOf` :606, `NormalizedPoissonRadiusOf` :816, `LloydPhase` fallback).

### Receipt-validity swarm (the largest single collapse target)
~40 `readonly record struct …Receipt` across the lane each hand-roll `IsValid`/`IsUsable` as a 10-40-term `RhinoMath.IsValidDouble(x) && x >= 0.0 && …` conjunction (worst: `TuftedLaplacianReceipt` ~35 terms Mesh.cs:214, `PowerCcvtReceipt` ~35 terms Sample.cs:246, `GaugeReceipt` ~18 terms Matrix.cs:284). The shared-validity interface `IValidityEvidence` EXISTS but is applied to exactly ONE type (`SymmetricMatrix`, Matrix.cs:164). A generated `[ValidityEvidence]` aspect (or a finite-nonnegative field-fold combinator) collapses hundreds of LOC and centralizes the invariant — high-value, low-risk collapse.

### Hardcoding
- Scattered `private const` magic caps that are really policy values: `FlowKernel.MaxIterations=100000` (Flow.cs:676), `MeshKernel.AspectRatioCeiling=11.5`/`VolumeGridMaxNodes=1_000_000`/`NativeAdaptiveScale=100.0` (Mesh.cs:719-724), `MatrixKernel.BiCgStabDivergenceFactor=1e3`/`KktPivotTolerance` (Matrix.cs:387), `AlignKernel.MadToSigma=1.4826`/`LineSearchBudget=8` (Align.cs:112). Mixed discipline: SampleKind over-parameterizes (22 knobs) while these under-parameterize (hardcoded ceilings). Ceilings should be policy rows.
- Contrast the GOOD discipline: nearly all epsilon/tolerance thresholds ARE scale-derived (mesh bbox diagonal / mean edge), never bare literals (`PowerClipPolicy`, `SpdMassShift`). Preserve this on rebuild.

### Strata violations
- `Matrix.cs` (1092 LOC) and Flow.cs's Butcher machinery (`IntegratorKind`/`ButcherTableau`/`DenseOutputCoefficientFamily`/`ButcherDenseOutput`) are PURE numerics (MathNet/CSparse/RK order conditions) with ZERO geometric content, welded to RhinoCommon only through `RhinoMath.IsValidDouble` (= `double.IsFinite`) and `RhinoMath.SqrtEpsilon`/`ZeroTolerance` (owned epsilon constants). Per architecture law these belong at a host-neutral Numerics floor; the Rhino dependency is gratuitous.
- The `Op` error rail (`Domain/Validation.cs`) is threaded POSITIONALLY into ~every signature in all 8 files. Legitimate ROP, but the ubiquity is the strongest signal in the lane that `Op` wants to be an ambient effect/reader context (LanguageExt `Eff`) per docs/stacks/csharp, not a hand-threaded parameter.

---

## STRONGEST STRUCTURAL RULINGS THE ARCHITECT MUST MAKE

1. **SPLIT `Mesh.cs` (4445 LOC / cpx 1259) — the campaign's #1 liability.** The file boundary is "touches a Mesh," not a concept; ONE `MeshKernel` static owns ~30 algorithm families. Rule the exact page cut: (a) Meshing = `MeshSpace`+`LaplacianCache`+`IntrinsicMesh`+cotangent/IDT/tufted assembly+topology+`RestrictedPowerDiagram`+signpost/common-subdivision; (b) Processing = the ~15 DDG solvers (heat, MCF, cross-field, Hodge, vector-heat, geodesics MMP/exp/log, stripe, descriptors, segmentation, flatten, remesh, SDF/signed-heat). Each becomes a page with ONE dense entry, NEVER a shared mega-static.

2. **RESOLVE the `RhinoMath`-as-numeric-primitive strata violation.** `Matrix.cs` + the Butcher/dense-output machinery are host-neutral numerics welded to RhinoCommon only via `RhinoMath.IsValidDouble`/`SqrtEpsilon`/`ZeroTolerance`. Rule: site the linear-algebra + ODE-tableau kernels at a host-neutral Numerics floor (`double.IsFinite` + owned epsilon constants), OR ratify the Rhino floor explicitly. This is the cleanest strata decision in the lane.

3. **COLLAPSE the ~40-receipt validity swarm.** Rule the mechanism: a source-gen `[ValidityEvidence]` aspect over field metadata, or a shared finite-nonnegative fold combinator, applied to every `…Receipt.IsValid`/`IsUsable`. `IValidityEvidence` already exists (1 use) — extend it corpus-wide. Hundreds of LOC of copy-paste conjunctions collapse.

4. **RULE the per-sub-domain entry contract + kill knob-spam.** Today each concept is an `internal static …At(…, Op key)` reached only through `Intent.Project` (vectors-b). Rule ONE canonical polymorphic entry per new sub-domain and collapse the `SampleKind.PowerCcvt` 22-param constructor into preset + advanced-override policy records (the `TuftedCoverPolicy.Default` pattern already models this in-lane). Requires cross-lane coordination on where the `Intent` dispatch re-homes.

5. **RULE `Op key` threading (corpus-wide).** Positional `Op` in ~every signature: keep explicit ROP threading, or lift to an ambient LanguageExt `Eff`/reader per docs/stacks/csharp. Affects every file in both lanes; must be decided ONCE before rebuild.

6. **PROMOTE `AtomProjection` (Atoms.cs:244) to a named projection owner.** It is THE raw→typed-output dispatch rail used by every projection in every lane file but hides as an `internal static class`. Site it as the explicit projection owner in the Numerics/core sub-domain.

7. **HOME Align / Flow / Sample (527/743/1428 LOC, cohesive algorithm families with no natural slot in {Numerics,Spatial,Meshing,Parametric,Processing,Drawing}).** Rule: fold ICP-registration + streamline-integration + point-sampling into Processing, OR mint dedicated sub-domains (Registration/Flow/Sampling). They are large and self-contained enough to warrant their own pages either way.

8. **PRESERVE the power-diagram ↔ BNOT-sampler coupling across the split.** `RestrictedPowerDiagram`/`PowerCell`/`PowerFacet` (Mesh.cs:654-716) are consumed EXCLUSIVELY by `Sample.cs` `PowerCcvtRun`. When Mesh.cs splits, rule where the Laguerre-diagram family lands (Meshing shared owner) so the sole cross-file consumer edge survives.

9. **RULE `LaplacianCache` as a Meshing-owned service + preserve its identity/GC/concurrency semantics VERBATIM.** `ConditionalWeakTable` keyed by `Mesh` reference identity + `Atom<HashMap>` memos + `Lock`-guarded CSparse solves. This is the stateful memoization substrate the ENTIRE DDG kernel rides; a naive rebuild that loses the weak-table keying or the per-kernel success-only memo semantics silently breaks caching correctness. Rule its re-home shape before any Mesh split.

10. **RATIFY the honest statement-kernel exemptions as sited `[OPERATIONS]` kernels, not hidden bodies.** Sample.cs + Mesh.cs carry many "NAMED STATEMENT-KERNEL EXEMPTION" hot loops (Sutherland-Hodgman clip, MMP windows, volume-grid triple-loop, Gram-Schmidt, BNOT Newton). These are legitimate; rule that the rebuild sites each as an explicit typed-receipt-returning algorithm kernel in its owning page (mostly already true) so the split preserves the exemption boundaries rather than re-hiding them in a new mega-static.

## ZERO-CAPABILITY-LOSS CHECKLIST (for the coverage-verify gate)
Flagship capabilities that MUST land in a page fence or be dropped with an explicit standing reason: 6 ICP variants (Umeyama/Chen-Medioni/Rusinkiewicz/robust-Welsch/normal-weighted/GICP); 9 RK integrators + adaptive PI control + dense output (3 families); LOBPCG (real+Hermitian) with no hidden dense fallback; generalized-eigen Cholesky congruence; 3-mode singular gauge solve (pin/deflation/KKT); 11 sampling algorithms incl. de Goes BNOT continuous power-CCVT, Yuksel elimination, Dwork variable-density, Bridson Poisson; cotangent/IDT/tufted-intrinsic Laplacians; intrinsic-Delaunay flipping + normal coordinates + signpost transport + common subdivision; heat-method geodesics; MCF; Knöppel cross-fields + stripes; Hodge decomposition; Sharp-Soliman-Crane vector heat; 3 log-map algorithms (approx/exact-exp/MMP-window); MMP exact geodesics; 3 mesh-SDF methods (GWN winding / boundary signed-heat / closed volumetric signed-heat); spectral descriptors (HKS/WKS); 6 segmentation algorithms; LSCM flatten; QuadRemesh/Reduce; restricted power diagrams; 14 support projections; 9 curve + 13 surface parametric projections; the 12 Atoms value objects (incl. Snell refract, Bishop transport, cone envelope).
