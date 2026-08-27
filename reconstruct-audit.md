# 1. Replace the two-case sign roster with the fact the sampler reads

## Location

`libs/dotnet/Rasm/.planning/Meshing/reconstruct.md:24-877` — C# fence anchors `SdfSignConvention` at lines 94-99, `SdfMeshPolicy` at lines 183-192, `TetSignedHeatPolicy` at lines 194-200, `TetSignedHeatSolve` at lines 447-450, and the three distance projections at lines 762-778.

## From

```csharp
[SmartEnum<int>]
public sealed partial class SdfSignConvention {
    public static readonly SdfSignConvention NegativeInsidePositiveOutside = new(key: 0, multiplier: 1.0);
    public static readonly SdfSignConvention PositiveInsideNegativeOutside = new(key: 1, multiplier: -1.0);
    public double Multiplier { get; }
}
```

## To

```csharp
// SdfSignConvention DELETED
```

```csharp
public readonly record struct SdfMeshPolicy(
    SdfMeshMethod Method, bool PositiveInside, Option<VolumeGridPolicy> Grid,
    SignedHeatTime Heat, VolumeSolverPolicy Solver, LatticeInterpolation Interpolation,
    double WindingBetaSquared)
```

```csharp
public readonly record struct TetSignedHeatPolicy(
    SignedHeatTime Heat, VolumeSolverPolicy Solver, bool PositiveInside,
    TetGaugePolicy Gauge, TetInterpolation Interpolation)
```

Replace `TetSignedHeatSolve.SignConvention` with `bool PositiveInside`; change the four raw factory tails from `Option<SdfSignConvention> signConvention = default` to `bool positiveInside = false`. Each distance arm applies the convention without another owner:

```csharp
let oriented = active.PositiveInside ? -distance : distance
select new SdfMeshSample(Distance: oriented, Solve: solve)
```

## Why

The sign domain is intrinsically binary and every consumer immediately turns the selected row into `1.0` or `-1.0`. The roster has no independent payload, lookup, persistence, or dispatch behavior; its `Multiplier` is a numeric restatement of the same bit. A named policy fact preserves both conventions while deleting one module-level type, two authored rows, one generated roster, four `Option` factory carriers, and every multiplier hop.

# 2. Keep reconstruction provenance while deleting its mirrored columns and key

## Location

`libs/dotnet/Rasm/.planning/Meshing/reconstruct.md:24-877` — C# fence anchors `ReconstructionStatus` at lines 46-51 and `ReconstructionMode` at lines 65-78.

## From

```csharp
[SmartEnum<int>]
public sealed partial class ReconstructionStatus {
    public static readonly ReconstructionStatus ExactInterpolation = new(key: 0);
    public static readonly ReconstructionStatus ApproximateSdf = new(key: 1);
    public static readonly ReconstructionStatus PoissonIndicator = new(key: 2);
}
```

```csharp
public static readonly ReconstructionMode RbfInterpolation = new(key: 0, requiresNormals: false, polynomialDegree: 0, status: ReconstructionStatus.ExactInterpolation);
public int PolynomialDegree { get; }
public ReconstructionStatus Status { get; }
```

## To

```csharp
// ReconstructionStatus DELETED
```

```csharp
[SmartEnum]
public sealed partial class ReconstructionMode {
    public static readonly ReconstructionMode RbfInterpolation = new(requiresNormals: false);
    public static readonly ReconstructionMode RbfApproximation = new(requiresNormals: false);
    public static readonly ReconstructionMode MovingLeastSquares = new(requiresNormals: true);
    public static readonly ReconstructionMode LevinMovingLeastSquares = new(requiresNormals: true);
    public static readonly ReconstructionMode AlgebraicPointSetSurfaces = new(requiresNormals: true);
    public static readonly ReconstructionMode Poisson = new(requiresNormals: true);
    public static readonly ReconstructionMode ScreenedPoisson = new(requiresNormals: true);
    public static readonly ReconstructionMode NaturalNeighbor = new(requiresNormals: false);
    public bool RequiresNormals { get; }
}
```

## Why

`ReconstructionMode` is genuine sample provenance and remains on `ReconstructionFit` and `SampleFit`. Its `Status` is only a coarser projection of those eight rows, while `PolynomialDegree` has no read because every fit already records the degree actually used. No consumer reads a numeric mode key. This removes the mirrored module-level status type and both dead columns, and keyless Thinktecture generation removes unearned lookup, parsing, formatting, and conversion surface without erasing the real mode vocabulary.

# 3. Delete the unused policy-to-mode forwarding projection

## Location

`libs/dotnet/Rasm/.planning/Meshing/reconstruct.md:24-877` — C# fence anchor `ReconstructionPolicy.Mode` at lines 134-140.

## From

```csharp
public ReconstructionMode Mode => Switch(
    rbfCase: static c => c.Smoothing <= EpsilonPolicy.ZeroTolerance
        ? ReconstructionMode.RbfInterpolation
        : ReconstructionMode.RbfApproximation,
    mlsCase: static _ => ReconstructionMode.MovingLeastSquares,
    levinCase: static _ => ReconstructionMode.LevinMovingLeastSquares,
    apssCase: static _ => ReconstructionMode.AlgebraicPointSetSurfaces,
    poissonCase: static c => c.Policy.PointWeight > 0.0 ? ReconstructionMode.ScreenedPoisson : ReconstructionMode.Poisson,
    sibsonCase: static _ => ReconstructionMode.NaturalNeighbor);
```

## To

```csharp
// ReconstructionPolicy.Mode DELETED
```

## Why

The complete corpus has no read of this property. `Reconstruct` already selects each build arm through the policy's generated exhaustive `Switch`, and each produced fit records the actual mode after smoothing or point-weight routing. The projection repeats that dispatch without serving admission, evidence, or a consumer. Deleting it removes one public member and seven lines while retaining the mode vocabulary itself. The case-named policy factories remain: their bodiless declarations do not prove they are forwarding-only, so bypassing them with public nested-case constructors would be an unevidenced admission regression.

# 4. Delete the one-field volume-solver wrapper

## Location

`libs/dotnet/Rasm/.planning/Meshing/reconstruct.md:24-877` — C# fence anchors `VolumeSolverKind` at line 116, `VolumeSolverPolicy` at lines 163-170, its policy fields at lines 184-198 and 243-248, its evidence fields at lines 380-384 and 447-450, and its residual reads at lines 576 and 736-737.

## From

```csharp
[SmartEnum<int>]
public sealed partial class VolumeSolverKind {
    public static readonly VolumeSolverKind SparseCholeskyPinned = new(key: 0);
}
```

```csharp
public readonly record struct VolumeSolverPolicy(VolumeSolverKind Kind, PositiveMagnitude ResidualTolerance) {
    public static Fin<VolumeSolverPolicy> SparseCholesky(Context context, Op? key = null) =>
        key.OrDefault() switch {
            Op op => op.AcceptValidated<PositiveMagnitude>(candidate: SolvePath.SparseLdl.Cap.In(context: Some(context)))
                .Map(static tolerance => new VolumeSolverPolicy(Kind: VolumeSolverKind.SparseCholeskyPinned, ResidualTolerance: tolerance)),
        };
}
```

## To

```csharp
// VolumeSolverKind DELETED
// VolumeSolverPolicy DELETED
```

Replace each `VolumeSolverPolicy Solver` policy/evidence column with `PositiveMagnitude ResidualTolerance`; replace each `Option<VolumeSolverPolicy> solver = default` factory argument with `Option<PositiveMagnitude> residualTolerance = default`. Each raw factory resolves absence directly through the existing generated owner and context rail:

```csharp
from tolerance in residualTolerance.Match(
    Some: static tolerance => Fin.Succ(tolerance),
    None: () => op.AcceptValidated<PositiveMagnitude>(
        candidate: SolvePath.SparseLdl.Cap.In(context: Some(context))))
```

Read `policy.ResidualTolerance.Value` at the three residual gates and carry the same value on `VolumeSolve` and `TetSignedHeatSolve`.

## Why

The kind has one row and the wrapper has only one field after that row disappears. Its factory merely admits the already-generated `PositiveMagnitude` selected by `SolvePath.SparseLdl.Cap.In(context)`, while every consumer immediately drills through `Solver.ResidualTolerance`. Carrying that admitted value directly deletes two module-level types, one smart-enum row, one public factory, one field hop at every solve gate, and the optional wrapper carrier. The actual Cholesky-versus-singular route remains derived from boundary singularity and screening inside `BuildPoisson`; no solver capability is lost.

# 5. Remove tetrahedral selectors whose mathematics has one implementation

## Location

`libs/dotnet/Rasm/.planning/Meshing/reconstruct.md:24-877` — C# fence anchors `TetGaugePolicy`/`TetInterpolation` at lines 114-115, `TetSignedHeatPolicy` at lines 194-200, `TetLocation` at lines 441-444, and `TetSignedHeatSolve` at lines 446-455.

## From

```csharp
[SmartEnum<int>] public sealed partial class TetGaugePolicy { public static readonly TetGaugePolicy PinnedFirstBoundary = new(key: 0); }
[SmartEnum<int>] public sealed partial class TetInterpolation { public static readonly TetInterpolation Barycentric = new(key: 0); }
```

```csharp
SignedHeatTime Heat, VolumeSolverPolicy Solver, SdfSignConvention SignConvention,
TetGaugePolicy Gauge, TetInterpolation Interpolation)
```

```csharp
public readonly record struct TetLocation(TetInterpolation Interpolation, int CellIndex, Arr<double> Barycentric, bool Inside)
```

## To

```csharp
// TetGaugePolicy DELETED
// TetInterpolation DELETED
```

```csharp
SignedHeatTime Heat, PositiveMagnitude ResidualTolerance, bool PositiveInside)
```

```csharp
public readonly record struct TetLocation(int CellIndex, Arr<double> Barycentric, bool Inside)
```

Delete the matching `Gauge` and `Interpolation` fields from `TetSignedHeatSolve` and the two optional factory parameters from `TetSignedHeatPolicy.Of`.

## Why

The tetrahedral row defines one gauge (the first boundary face) and one interpolation law (barycentric coordinates). Neither axis has an alternative implementation or a consumer outside this fence; `TetLocation.Barycentric` is itself the interpolation evidence. The rows are labels for fixed algorithm steps, not policies. This removes two module-level types, two roster members, five repeated fields, two public parameters, and both generated surfaces while retaining the full algorithm.

# 6. Remove the fixed closed-grid boundary-condition label

## Location

`libs/dotnet/Rasm/.planning/Meshing/reconstruct.md:24-877` — C# fence anchors `VolumeBoundaryCondition` at line 117, `SdfMeshPolicy` at lines 183-192, `VolumeSolve` at lines 379-389, and `ClosedSignedHeatKey` at line 480.

## From

```csharp
[SmartEnum<int>]
public sealed partial class VolumeBoundaryCondition {
    public static readonly VolumeBoundaryCondition NeumannGaugePinned = new(key: 0);
}
```

```csharp
LatticeInterpolation Interpolation, VolumeBoundaryCondition BoundaryCondition,
```

## To

```csharp
// VolumeBoundaryCondition DELETED
```

```csharp
LatticeInterpolation Interpolation,
```

Delete `BoundaryCondition` from `VolumeSolve`, the closed-grid cache probe, and their construction sites.

## Why

The closed signed-heat volume algorithm has exactly one boundary treatment and its gauge node is already recorded on `VolumeSolve`. A one-row vocabulary repeated through policy, cache identity, and evidence adds a module type and three fields without selecting behavior. A future genuinely different boundary law would first require another implemented solve arm; it does not justify a dormant selector now.

# 7. Make mesh-SDF policy shape carry the method

## Location

`libs/dotnet/Rasm/.planning/Meshing/reconstruct.md:24-877` — C# fence anchors `SdfMeshMethod` at lines 87-92 and `SdfMeshPolicy` at lines 183-192.

## From

```csharp
[SmartEnum<int>]
public sealed partial class SdfMeshMethod {
    public static readonly SdfMeshMethod GeneralizedWindingNumber = new(key: 0);
    public static readonly SdfMeshMethod BoundarySignedHeat = new(key: 1);
    public static readonly SdfMeshMethod ClosedSurfaceSignedHeat = new(key: 2);
}
```

```csharp
public readonly record struct SdfMeshPolicy(
    SdfMeshMethod Method, SdfSignConvention SignConvention, Option<VolumeGridPolicy> Grid,
    SignedHeatTime Heat, VolumeSolverPolicy Solver, LatticeInterpolation Interpolation,
    double WindingBetaSquared)
```

## To

```csharp
// SdfMeshMethod DELETED
```

```csharp
[Union]
public abstract partial record SdfMeshPolicy {
    private SdfMeshPolicy() { }
    public sealed record WindingCase(bool PositiveInside, double BetaSquared) : SdfMeshPolicy;
    public sealed record BoundaryCase(bool PositiveInside, SignedHeatTime Heat, PositiveMagnitude ResidualTolerance) : SdfMeshPolicy;
    public sealed record ClosedCase(bool PositiveInside, VolumeGridPolicy Grid, SignedHeatTime Heat,
        PositiveMagnitude ResidualTolerance, LatticeInterpolation Interpolation) : SdfMeshPolicy;
}
```

Keep the three existing raw-input factories on this owner, returning their corresponding cases after admission. Retain `internal Fin<SdfMeshPolicy> Admit(Op key)` as the one canonical check for policies supplied through public case constructors, but make its generated `Switch` validate only the active case. Narrow `SignedHeatDiscretization.BoundarySourceCase` to `SdfMeshPolicy.BoundaryCase` and `ClosedVolumeGridCase` to `SdfMeshPolicy.ClosedCase`; narrow the corresponding `SignedHeatSpine` methods the same way. `WindingFieldOf`, `SolveOf`, and each dispatch arm take the exact generated case they consume rather than the union root.

## Why

The current product type admits nonsensical combinations: winding policies carry unused heat/solver/interpolation fields, boundary heat carries an absent grid, and closed heat relies on a later `Option.ToFin`. A generated union makes the method recoverable from payload shape, deletes the module-level method roster and optional grid, narrows admission to the fields that exist on the active case, and makes every generated `Switch` exhaustive. This is the exact algebraic-data-type replacement for a tag plus nullable-shaped payload matrix.

# 8. Make mesh-SDF evidence carry the same closed shape

## Location

`libs/dotnet/Rasm/.planning/Meshing/reconstruct.md:24-877` — C# fence anchors `SdfSolve` at lines 391-398 and `MeshSdf.SignedDistanceDetailed` at lines 761-779.

## From

```csharp
public readonly record struct SdfSolve(
    SdfMeshMethod Method, Topology Topology,
    Option<SignedHeatSolve> SignedHeat, Option<VolumeSolve> Volume = default) : IValidityEvidence
```

## To

```csharp
[Union]
public abstract partial record SdfSolve : IValidityEvidence {
    private SdfSolve() { }
    public sealed record WindingCase(Topology Topology) : SdfSolve {
        public override bool IsValid => true;
    }
    public sealed record BoundaryCase(Topology Topology, SignedHeatSolve Heat) : SdfSolve {
        public override bool IsValid => Heat.IsValid;
    }
    public sealed record ClosedCase(Topology Topology, SignedHeatSolve Heat, VolumeSolve Volume) : SdfSolve {
        public override bool IsValid => Heat.IsValid && Volume.IsValid;
    }
    public abstract bool IsValid { get; }
}
```

`MeshSdf` exhaustively switches on `SdfMeshPolicy` and constructs the matching evidence case in each policy arm.

## Why

`Method`, `SignedHeat`, and `Volume` re-encode the same three-case matrix with optional fields and allow contradictory evidence. The union removes two option carriers and the duplicated method discriminant while preserving the public `SdfSolve` type consumed by `Spatial/fields.md`. Validity becomes case-local and requires no absent-is-valid clauses.

# 9. Return the scalar-field owner directly from reconstruction

## Location

`libs/dotnet/Rasm/.planning/Meshing/reconstruct.md:24-877` — C# fence anchors `ReconstructionResult` at lines 277-285 and `Reconstruction.Reconstruct`/`BuildRbf`/`AdmitAndSeat`/`BuildPoisson` at lines 493-585.

## From

```csharp
public readonly record struct ReconstructionResult(ScalarField Field, ReconstructionFit Fit) {
    internal Fin<TOut> Project<TOut>(Op key) {
        ReconstructionResult self = this;
        return ResultProjection.Rows<ReconstructionResult, TOut>(self: self, key: key,
            ProjectionRow.Of<ReconstructionFit>(() => Fin.Succ(self.Fit)),
            ProjectionRow.Of<ScalarField>(() => Optional(self.Field).ToFin(key.InvalidResult())));
    }
}
```

```csharp
public static Fin<ReconstructionResult> Reconstruct(Seq<MlsSample> samples, ReconstructionPolicy policy, Context context, Op? key = null) =>
```

## To

```csharp
// ReconstructionResult DELETED
```

```csharp
public static Fin<ScalarField> Reconstruct(Seq<MlsSample> samples, ReconstructionPolicy policy, Context context, Op? key = null) =>
```

Change the three private builders to `Fin<ScalarField>` and return their `ScalarField.*Case` directly. While seating MLS, match the owning case shape in `Spatial/fields.md`: pass `Kernel: c.Policy.Kernel` and `Radius: c.Policy.Radius`, not the nonexistent `Policy:` argument.

## Why

Every constructed field case already carries the exact fit evidence it needs: RBF/MLS/Levin/APSS/Sibson carry `ReconstructionFit`, while Poisson carries the stronger `PoissonSolve`. `ReconstructionResult` duplicates that evidence, adds an unused projection method, and forces callers through a shell to reach the actual algebra owner. Returning `ScalarField` removes one module-level type and one member while preserving all field and evidence capability.

# 10. Inline the RBF fit after deleting its result shell

## Location

`libs/dotnet/Rasm/.planning/Meshing/reconstruct.md:24-877` — C# fence anchors the two `FitOf(...)` calls and helper at lines 548-555.

## From

```csharp
select new ReconstructionResult(
    Field: new ScalarField.RbfCase(Samples: samples, Kernel: kernel, Radius: radius, Coefficients: coefficients,
        Fit: FitOf(mode: exact ? ReconstructionMode.RbfInterpolation : ReconstructionMode.RbfApproximation, kernel: kernel, radius: radius.Value, smoothing: smoothing, n: n, degree: tail is 0 ? 0 : tail is 1 ? 0 : 1, solve: solve)),
    Fit: FitOf(mode: exact ? ReconstructionMode.RbfInterpolation : ReconstructionMode.RbfApproximation, kernel: kernel, radius: radius.Value, smoothing: smoothing, n: n, degree: tail is 0 ? 0 : tail is 1 ? 0 : 1, solve: solve));
```

```csharp
private static ReconstructionFit FitOf(ReconstructionMode mode, KernelKind kernel, double radius, double smoothing, int n, int degree, LinearSolution solve) =>
    new(Mode: mode, Kernel: Some(kernel), Radius: Some(radius), Smoothing: Some(smoothing),
        SampleCount: n, CenterCount: n, PolynomialDegree: degree, Solve: Some(solve));
```

## To

```csharp
let degree = tail is 0 ? 0 : tail is 1 ? 0 : 1
let fit = new ReconstructionFit(
    Mode: exact ? ReconstructionMode.RbfInterpolation : ReconstructionMode.RbfApproximation,
    Kernel: Some(kernel), Radius: Some(radius.Value), Smoothing: Some(smoothing),
    SampleCount: n, CenterCount: n, PolynomialDegree: degree, Solve: Some(solve))
select (ScalarField)new ScalarField.RbfCase(
    Samples: samples, Kernel: kernel, Radius: radius, Coefficients: coefficients, Fit: fit);
```

```csharp
// Reconstruction.FitOf DELETED
```

## Why

Deleting the result shell reduces two identical calls to one construction site. Keeping a private seven-parameter forwarding helper after that collapse creates a one-call member and hides no independent algorithm. The local `fit` also proves the field receives the same witness that was formerly duplicated in the outer shell.

# 11. Keep the RBF basis projection inside its only kernel

## Location

`libs/dotnet/Rasm/.planning/Meshing/reconstruct.md:24-877` — C# fence anchor `Reconstruction.BuildRbf` and its `Basis` helper at lines 515-556.

## From

```csharp
double basis = Basis(t, points[i].Position);
```

```csharp
for (int i = 0; i < n; i++) tailRow[i] = Basis(t, points[i].Position);
```

```csharp
private static double Basis(int term, Point3d at) =>
    term switch { 0 => 1.0, 1 => at.X, 2 => at.Y, _ => at.Z };
```

## To

Move the same expression into `BuildRbf` as a block-local static function, before its first read:

```csharp
static double Basis(int term, Point3d at) =>
    term switch { 0 => 1.0, 1 => at.X, 2 => at.Y, _ => at.Z };
```

```csharp
// Reconstruction.Basis DELETED
```

## Why

The polynomial basis is read twice, but both reads are inside one RBF construction body and no other reconstruction arm can consume it. Keeping it as a class member widens the module's symbol surface for operation-local mechanics. A static local preserves the shared expression and allocation-free calls while removing one private module member.

# 12. Inline the one-call Sibson tolerance projection

## Location

`libs/dotnet/Rasm/.planning/Meshing/reconstruct.md:24-877` — C# fence anchors `SibsonPolicy.Resolve` at line 231 and its sole call at lines 508-509.

## From

```csharp
internal PositiveMagnitude Resolve(Context context) =>
    Tolerance.IfNone(noneValue: PositiveMagnitude.Create(value: context.Absolute.Value));
```

```csharp
tolerance: c.Policy.Resolve(context: context).Value,
```

## To

```csharp
// SibsonPolicy.Resolve DELETED
```

```csharp
tolerance: c.Policy.Tolerance.IfNone(
    noneValue: PositiveMagnitude.Create(value: context.Absolute.Value)).Value,
```

## Why

The projection has one caller, is one expression, and introduces no admission or reusable algebra. Inlining it removes one module member and keeps the default visibly derived from the run context at the only place the Voronoi owner consumes it.

# 13. Use structural tuples as the two memo probes

## Location

`libs/dotnet/Rasm/.planning/Meshing/reconstruct.md:24-877` — C# fence anchors `BoundarySignedHeatKey`/`ClosedSignedHeatKey` at lines 479-480 and their sole constructions at lines 710-718.

## From

```csharp
internal readonly record struct BoundarySignedHeatKey(SignedHeatTime Heat, VolumeSolverPolicy Solver);
internal readonly record struct ClosedSignedHeatKey(
    VolumeGridPolicy Grid, SignedHeatTime Heat, VolumeSolverPolicy Solver,
    LatticeInterpolation Interpolation, VolumeBoundaryCondition BoundaryCondition);
```

```csharp
space.Cache.Memoized(probe: new BoundarySignedHeatKey(Heat: policy.Heat, Solver: policy.Solver),
```

## To

```csharp
// BoundarySignedHeatKey DELETED
// ClosedSignedHeatKey DELETED
```

```csharp
space.Cache.Memoized(probe: (policy.Heat, policy.ResidualTolerance),
```

Use `(policy.Grid, policy.Heat, policy.ResidualTolerance, policy.Interpolation)` for the closed probe.

## Why

Each record exists only once as a `Memoized` probe and has no behavior, admission, or domain identity beyond the structural values already supplied. `MeshCache.Memoized` keys a slot by `(typeof(TKey), typeof(T))`; these arity-two and arity-four tuple key types are distinct, and their boundary/closed result types are distinct after the next move. The tuples therefore preserve value equality and slot separation while removing two module-level types without adding a wrapper or helper.

# 14. Let signed-heat outcome cases own their payloads

## Location

`libs/dotnet/Rasm/.planning/Meshing/reconstruct.md:24-877` — C# fence anchors `SignedHeatSolution`, `ClosedSignedHeatSolution`, and `SignedHeatOutcome` at lines 481-490.

## From

```csharp
internal readonly record struct SignedHeatSolution(Arr<double> Values, SignedHeatSolve Solve, Topology Topology);
internal readonly record struct ClosedSignedHeatSolution(VolumeSolve Volume, Arr<double> Values, SignedHeatSolve Solve, Topology Topology);
```

```csharp
public sealed record SurfaceCase(SignedHeatSolution Solution) : SignedHeatOutcome;
public sealed record VolumeCase(ClosedSignedHeatSolution Solution) : SignedHeatOutcome;
```

## To

```csharp
// SignedHeatSolution DELETED
// ClosedSignedHeatSolution DELETED
```

```csharp
public sealed record SurfaceCase(Arr<double> Values, SignedHeatSolve Solve, Topology Topology) : SignedHeatOutcome;
public sealed record VolumeCase(Arr<double> Values, SignedHeatSolve Solve, Topology Topology, VolumeSolve Volume) : SignedHeatOutcome;
```

Return these case types from the corresponding cached `BoundarySolutionOf` and `ClosedSolutionOf` methods, and have `Solve` upcast them to `SignedHeatOutcome` without reconstructing a second shell.

## Why

The two internal records have exactly one purpose: payload a matching nested union case. That creates a case-to-shell-to-values hop. Seating the fields directly on the generated cases removes both module-level records, preserves cache result typing, and lets `MeshSdf` read the same named fields with one fewer indirection.

# 15. Make the admitted output the iso-surface classification

## Location

`libs/dotnet/Rasm/.planning/Meshing/reconstruct.md:24-877` — C# fence anchors `IsoSurfaceStatus` at lines 101-112, `IsoSurfaceRun`/`IsoSurfaceResult` at lines 460-477, and `IsoSurface.Detailed` at lines 805-847.

## From

```csharp
[SmartEnum<int>]
public sealed partial class IsoSurfaceStatus {
    public static readonly IsoSurfaceStatus NativeValid = new(key: 0,
        admitsEvidence: static (failures, vertices, faces, naked) => failures == 0 && vertices > 0 && faces > 0 && naked.IsSome);
    public static readonly IsoSurfaceStatus EvaluatorFailure = new(key: 1,
        admitsEvidence: static (failures, _, _, _) => failures > 0);
    public static readonly IsoSurfaceStatus NativeInvalidMesh = new(key: 2,
        admitsEvidence: static (failures, _, _, naked) => failures == 0 && naked.IsNone);
    [UseDelegateFromConstructor]
    internal partial bool AdmitsEvidence(int failures, int vertices, int faces, Option<int> nakedBoundaryLoops);
}
```

```csharp
public readonly record struct IsoSurfaceRun(
    bool NativeRouted, IsoSurfaceStatus Status, CellLattice Grid, long HexCellCount, Int128 CornerSampleCount,
    int MaxRootSteps, double IsoValue, bool NativeNormalsDiscarded,
    bool ParallelCallback, int EvaluatorFailures, int VertexCount, int FaceCount, Option<int> NakedBoundaryLoopCount,
    Option<double> FixedTolerance, Option<double> FixedNormalSampleDistance, Option<SdfSolve> MeshPreflight) : IValidityEvidence
```

```csharp
public readonly record struct IsoSurfaceResult(Option<MeshSpace> Space, IsoSurfaceRun Run);
```

```csharp
public static Fin<IsoSurfaceResult> Detailed(ScalarField field, CellLattice grid, IsoSurfacePolicy policy, Context context, Op? key = null)
```

## To

```csharp
public readonly record struct IsoSurfaceRun(
    Option<MeshSpace> Space, CellLattice Grid, long HexCellCount, Int128 CornerSampleCount,
    int MaxRootSteps, double IsoValue, bool NativeNormalsDiscarded,
    bool ParallelCallback, int EvaluatorFailures, int VertexCount, int FaceCount, Option<int> NakedBoundaryLoopCount,
    Option<double> FixedTolerance, Option<double> FixedNormalSampleDistance, Option<SdfSolve> MeshPreflight) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.CountAtLeast(EvaluatorFailures, 0),
        ValidityClaim.CountAtLeast(VertexCount, 0),
        ValidityClaim.CountAtLeast(FaceCount, 0),
        ValidityClaim.Finite(IsoValue),
        NakedBoundaryLoopCount.Map(static count => count >= 0).IfNone(true),
        MeshPreflight.Map(static witness => witness.IsValid).IfNone(true),
        Space.IsSome
            ? EvaluatorFailures == 0 && VertexCount > 0 && FaceCount > 0 && NakedBoundaryLoopCount.IsSome
            : EvaluatorFailures > 0 || NakedBoundaryLoopCount.IsNone);
}
```

```csharp
// IsoSurfaceStatus DELETED
// IsoSurfaceResult DELETED
// IsoSurface.Detailed.status DELETED
```

```csharp
public static Fin<IsoSurfaceRun> Detailed(ScalarField field, CellLattice grid, IsoSurfacePolicy policy, Context context, Op? key = null)
```

## Why

`IsoSurfaceResult` is a behaviorless two-field wrapper used only to pair output presence with the run. Once the native branch preserves `MeshSpace.Of` failures, `Space` is also the authoritative classification: callback failure never exposes a space, invalid native output has no space, and only an admitted mesh is present. The status roster re-derives those carried facts through a generated delegate. Seating `Space` on the run and validating the joint evidence deletes two module-level types, three rows, one generated delegate, one forwarding hop, and the construction switch without losing success gating or failure diagnosis.

## Ripples

- `libs/dotnet/Rasm/.planning/Processing/extract.md:493-503`: construct the `CellLattice` that the target entry actually requires, project from the returned run directly, replace `result.Run` with `run`, gate success on `run.Space.IsSome`, and replace the nonexistent `result.Mesh` read with `run.Space.ToFin(...).Map(static space => space.DuplicateNative())` where a host `Mesh` is requested; keep `run.EvaluatorFailures` as the failure diagnostic.
- `libs/dotnet/Rasm/.planning/Processing/decimate.md:568-574`: construct the `CellLattice` from the measured bounds and `VoxelResolution`, then bind `run.Space.ToFin(key.InvalidResult())` after `IsoSurface.Detailed`; do not read the nonexistent `result.Mesh` member.

# 16. Remove fixed and derived fields from iso evidence

## Location

`libs/dotnet/Rasm/.planning/Meshing/reconstruct.md:24-877` — C# fence anchors `IsoSurfaceRun` at lines 460-475, the native constants at lines 805-807, and run construction at lines 835-843.

## From

```csharp
bool NativeRouted, IsoSurfaceStatus Status, CellLattice Grid, long HexCellCount, Int128 CornerSampleCount,
int MaxRootSteps, double IsoValue, bool NativeNormalsDiscarded,
bool ParallelCallback, int EvaluatorFailures, int VertexCount, int FaceCount, Option<int> NakedBoundaryLoopCount,
Option<double> FixedTolerance, Option<double> FixedNormalSampleDistance, Option<SdfSolve> MeshPreflight
```

```csharp
private const double NativeFixedNormalSampleDistance = 1.0e-5;
```

## To

```csharp
Option<MeshSpace> Space, CellLattice Grid,
int MaxRootSteps, double IsoValue, int EvaluatorFailures, int VertexCount, int FaceCount,
Option<int> NakedBoundaryLoopCount, double FixedTolerance, Option<SdfSolve> MeshPreflight
```

Delete the `HexCellCount == Grid.CellCount`, `CornerSampleCount == Grid.NodeCount`, and `NativeNormalsDiscarded` validity clauses. Tighten the retained fixed-tolerance clause to `ValidityClaim.Positive(FixedTolerance)` now that the value is not optional.

```csharp
// IsoSurface.NativeFixedNormalSampleDistance DELETED
```

## Why

This owner has one native route, Rhino's callback is always parallel, normals are always rebuilt, grid cell/node counts are derivable from `Grid`, and the native normal-sample distance has no consumer. Those are implementation constants, not per-run evidence. `FixedTolerance` is retained as a non-optional value because `Processing/extract` genuinely reports it; the existing `Option<double>` falsely admits absence on a route where the Rhino API fixes it.

## Ripples

- `libs/dotnet/Rasm/.planning/Processing/extract.md:499-502`: set `route: ExtractionRoute.Native` and pass `ExtractionTolerance.RhinoFixed(run.FixedTolerance)` directly.

# 17. Preserve native mesh admission failures instead of erasing them

## Location

`libs/dotnet/Rasm/.planning/Meshing/reconstruct.md:24-877` — C# fence anchors the native `Mesh.CreateFromIsosurface` block at lines 819-835.

## From

```csharp
Mesh? result = Mesh.CreateFromIsosurface(
    scalarFieldEvaluator: EvaluateIso, box: indexBox, resolution: resolution,
    RootFindingMaxSteps: policy.MaxRootSteps.Value);
_ = result?.Transform(xform: grid.IndexToWorld);
_ = result?.RebuildNormals();
```

```csharp
Option<MeshSpace> space = Optional(result)
    .Filter(static mesh => mesh.IsValid)
    .Bind(mesh => MeshSpace.Of(source: new MeshSource.Native(Value: mesh), context: context, key: op).ToOption());
```

## To

```csharp
Mesh result = Mesh.CreateFromIsosurface(
    scalarFieldEvaluator: EvaluateIso, box: indexBox, resolution: resolution,
    RootFindingMaxSteps: policy.MaxRootSteps.Value);
_ = result.Transform(xform: grid.IndexToWorld);
_ = result.RebuildNormals();
```

```csharp
return failures == 0 && result.IsValid
    ? MeshSpace.Of(source: new MeshSource.Native(Value: result), context: context, key: op)
        .Map(space => Finish(Some(space)))
    : Fin.Succ(Finish(Option<MeshSpace>.None));
```

`Finish` is a block-local function that constructs the collapsed `IsoSurfaceRun` from the already-computed output and counts; it must not become a module member.

## Why

The folder Rhino catalogue verifies that `Mesh.CreateFromIsosurface` has one non-null exit: an empty extraction is a non-null invalid mesh. Nullable calls and `Optional(result)` therefore model an impossible state. More importantly, `.ToOption()` currently converts a failure from the canonical `MeshSpace` admission boundary into an invalid-native-mesh status. The branch preserves a real admission fault on `Fin`, refuses to expose a mesh produced after any evaluator failure, and retains absence for a genuinely invalid native extraction.

# 18. Inline the complete one-arm winding-distance pipeline

## Location

`libs/dotnet/Rasm/.planning/Meshing/reconstruct.md:24-877` — C# fence anchors the generalized-winding dispatch arm at lines 765-768 and its two private helpers at lines 784-792.

## From

```csharp
generalizedWindingNumber: () =>
    from distance in GeneralizedWindingDistance(space: space, policy: active, sample: sample, key: op)
    from solve in SolveOf(space: space, policy: active, signedHeat: Option<SignedHeatSolve>.None)
    select new SdfMeshSample(Distance: active.SignConvention.Multiplier * distance, Solve: solve),
```

```csharp
private static Fin<double> GeneralizedWindingDistance(MeshSpace space, SdfMeshPolicy policy, Point3d sample, Op key) =>
    from field in WindingFieldOf(space: space, samples: [sample], policy: policy, key: key)
    from winding in field.Length == 1 ? Fin.Succ(field[0]) : Fin.Fail<double>(key.InvalidResult())
    from distance in ClosestDistanceOf(space: space, sample: sample, key: key)
    select Math.Abs(value: winding) > 0.5 ? -distance : distance;
private static Fin<double> ClosestDistanceOf(MeshSpace space, Point3d sample, Op key) =>
    space.Native.ClosestPoint(testPoint: sample, pointOnMesh: out Point3d hit, maximumDistance: 0.0) < 0
        ? Fin.Fail<double>(key.InvalidResult())
        : Fin.Succ(sample.DistanceTo(other: hit));
```

## To

```csharp
windingCase: active =>
    from field in WindingFieldOf(space: space, samples: [sample], policy: active, key: op)
    from winding in field.Length == 1 ? Fin.Succ(field[0]) : Fin.Fail<double>(op.InvalidResult())
    from hit in space.Native.ClosestPoint(
            testPoint: sample, pointOnMesh: out Point3d closest, maximumDistance: 0.0) >= 0
        ? Fin.Succ(closest)
        : Fin.Fail<Point3d>(op.InvalidResult())
    let distance = sample.DistanceTo(hit)
    let signed = Math.Abs(winding) > 0.5 ? -distance : distance
    from solve in SolveOf(space: space, policy: active)
    select new SdfMeshSample(Distance: active.PositiveInside ? -signed : signed, Solve: solve),
```

```csharp
// MeshSdf.GeneralizedWindingDistance DELETED
// MeshSdf.ClosestDistanceOf DELETED
```

## Why

Both private helpers exist for this single exhaustive policy arm. The outer helper sequences one-sample winding, closest-point distance, and sign; the inner helper only lifts Rhino's `out` value. Seating that pipeline directly in the `WindingCase` arm removes two module members and two resolution hops while keeping each refusal on `Fin`. `WindingFieldOf` remains because it is the reusable batch kernel and prewarm path, not a one-call extraction.

# 19. Inline the one-arm surface interpolation gate

## Location

`libs/dotnet/Rasm/.planning/Meshing/reconstruct.md:24-877` — C# fence anchors the boundary signed-heat dispatch arm at lines 769-773 and `MeshSdf.InterpolateOnMesh` at lines 794-801.

## From

```csharp
boundarySignedHeat: () =>
    from solution in SignedHeatSpine.BoundarySolutionOf(space: space, policy: active, key: op)
    from signed in InterpolateOnMesh(space: space, sample: sample, perVertex: solution.Values, key: op)
    from solve in SolveOf(space: space, policy: active, signedHeat: Some(solution.Solve), topology: Some(solution.Topology))
    select new SdfMeshSample(Distance: active.SignConvention.Multiplier * signed, Solve: solve),
```

```csharp
private static Fin<double> InterpolateOnMesh(MeshSpace space, Point3d sample, Arr<double> perVertex, Op key) =>
    Optional(space.Native.ClosestMeshPoint(testPoint: sample, maximumDistance: 0.0))
        .Filter(static point => point.FaceIndex >= 0)
        .ToFin(key.InvalidResult())
        .Bind(point => CornerSlotsOf(face: space.Native.Faces[index: point.FaceIndex], triangle: point.Triangle)
            .ToFin(key.Unsupported(inputType: typeof(MeshPoint), outputType: typeof(double)))
            .Bind(slots => guard(slots.ForAll(slot => slot.Vertex >= 0 && slot.Vertex < perVertex.Count), key.InvalidResult()).ToFin()
                .Map(_ => slots.Fold(0.0, (sum, slot) => sum + (point.T[slot.Slot] * perVertex[index: slot.Vertex])))));
```

## To

```csharp
boundaryCase: active =>
    from solution in SignedHeatSpine.BoundarySolutionOf(space: space, policy: active, key: op)
    from point in Optional(space.Native.ClosestMeshPoint(testPoint: sample, maximumDistance: 0.0))
        .Filter(static candidate => candidate.FaceIndex >= 0).ToFin(op.InvalidResult())
    from slots in CornerSlotsOf(face: space.Native.Faces[index: point.FaceIndex], triangle: point.Triangle)
        .ToFin(op.Unsupported(inputType: typeof(MeshPoint), outputType: typeof(double)))
    from _ in guard(slots.ForAll(slot => slot.Vertex >= 0 && slot.Vertex < solution.Values.Count), op.InvalidResult())
    let signed = slots.Fold(0.0, (sum, slot) => sum + (point.T[slot.Slot] * solution.Values[index: slot.Vertex]))
    from solve in SolveOf(space: space, policy: active, signedHeat: solution.Solve, topology: solution.Topology)
    select new SdfMeshSample(Distance: active.PositiveInside ? -signed : signed, Solve: solve),
```

```csharp
// MeshSdf.InterpolateOnMesh DELETED
```

## Why

This helper is the complete body of one boundary-policy arm: locate one mesh point, admit its corner slots, and fold the interpolant. Seating that query in the arm removes one module member and the nested `Bind` layers while preserving every failure on `Fin`. `CornerSlotsOf` remains because its bodiless declaration does not expose a forwarding implementation that can safely be erased.

# 20. Compose every query guard without a redundant carrier lift

## Location

`libs/dotnet/Rasm/.planning/Meshing/reconstruct.md:24-877` — C# fence anchors the query guards in `BuildRbf` at line 540, `AdmitAndSeat` at lines 560-561, `BuildPoisson` at line 576, `EvaluateRbf` at line 593, `EvaluateMls` at lines 601 and 604, `EvaluateLevinMls` at line 617, `EvaluateSibson` at lines 675 and 677, `ComputeSignedHeatDetailed` at lines 736-737, and `IsoContour.Detailed` at line 852.

## From

```csharp
from _ in guard(n >= 1, key.InvalidInput()).ToFin()
```

The same redundant `.ToFin()` suffix appears on the other nine ordinary query guards named in `Location`. The signed-heat gate hand-builds both carrier arms:

```csharp
from residuals in heatSolve.Residual <= policy.Solver.ResidualTolerance.Value && poissonSolve.Residual <= policy.Solver.ResidualTolerance.Value
    ? Fin.Succ(unit) : Fin.Fail<Unit>(key.InvalidResult())
```

## To

```csharp
from _ in guard(n >= 1, key.InvalidInput())
```

Apply the same suffix deletion to the other ordinary query guards. Replace the hand-built residual branch with the same direct carrier form:

```csharp
from _ in guard(
    heatSolve.Residual <= policy.ResidualTolerance.Value && poissonSolve.Residual <= policy.ResidualTolerance.Value,
    key.InvalidResult())
```

## Why

LanguageExt's verified `Fin.SelectMany(Func<Unit, Guard<Error,Unit>>)` overload composes `guard` directly as a `from` clause. Calling `.ToFin()` first needlessly lowers the same gate into a concrete carrier, while constructing `Fin.Succ(unit)` and `Fin.Fail<Unit>` by hand duplicates that lowering and names an unused binding. One operator form across every query is shorter and removes eleven manual carrier lowerings without changing refusal order.

# 21. Collapse the two payloadless Levin axes into named boolean facts

## Location

`libs/dotnet/Rasm/.planning/Meshing/reconstruct.md:24-877` — C# fence anchors `PlaneSeat`/`NormalPosture` at lines 53-63, `LevinMlsPolicy` at lines 202-211, and the `LevinFit.Seat` field at lines 307-313.

## From

```csharp
[SmartEnum<int>]
public sealed partial class PlaneSeat {
    public static readonly PlaneSeat Centroid = new(key: 0);
    public static readonly PlaneSeat ThroughPoint = new(key: 1);
}
[SmartEnum<int>]
public sealed partial class NormalPosture {
    public static readonly NormalPosture Raw = new(key: 0);
    public static readonly NormalPosture Oriented = new(key: 1);
}
```

```csharp
double RidgeLambda, Tolerance NormalTau, Tolerance ProjEps,
PlaneSeat Seat, NormalPosture Normals, WeightKernel WeightKernel
```

## To

```csharp
// PlaneSeat DELETED
// NormalPosture DELETED
```

```csharp
double RidgeLambda, Tolerance NormalTau, Tolerance ProjEps,
bool ThroughPoint, bool OrientNormals, WeightKernel WeightKernel
```

Change the raw factory tail to `bool throughPoint = false, bool orientNormals = true`, read those facts directly in `ConvergeLevinPlane`, and replace `LevinFit.Seat` with `bool ThroughPoint`.

## Why

Each owner is a two-case closed family with no payload, delegate, lookup, persistence, or consumer outside this fence. Each is immediately consumed as one binary algorithm fact. Named boolean columns preserve both real Levin choices while deleting two module-level types, four authored rows, two generated rosters, two `Option` factory carriers, and the option-default resolution that only reconstitutes those booleans. This follows the stack law for payloadless two-case families; it does not remove either algorithm branch.

# 22. Collapse the Poisson boundary label into the one fact the solver reads

## Location

`libs/dotnet/Rasm/.planning/Meshing/reconstruct.md:24-877` — C# fence anchors `PoissonBoundary` at lines 80-85, `PoissonPolicy` at lines 242-249, `PoissonSolve` at lines 343-359, and the solve route at lines 573-575.

## From

```csharp
[SmartEnum<int>]
public sealed partial class PoissonBoundary {
    public static readonly PoissonBoundary Neumann = new(key: 0, singular: true);
    public static readonly PoissonBoundary Dirichlet = new(key: 1, singular: false);
    public bool Singular { get; }
}
```

```csharp
from solve in policy.Boundary.Singular && policy.PointWeight <= 0.0
```

## To

```csharp
// PoissonBoundary DELETED
```

```csharp
from solve in !policy.Dirichlet && policy.PointWeight <= 0.0
```

Replace `PoissonPolicy.Boundary` with `bool Dirichlet`, change its raw factory argument to `bool dirichlet = false`, and replace `PoissonSolve.Boundary` with the same named fact.

## Why

The roster has exactly two payloadless rows and one consumer, which immediately reads the `Singular` complement to choose pinned singular solve versus definite solve. `Dirichlet` is the actual policy fact: `false` retains the default Neumann/gauge path and `true` selects the Dirichlet row imposition. This deletes one module-level type, two row members, a generated roster, and an `Option<PoissonBoundary>` factory carrier while retaining both boundary algorithms and their evidence.

# 23. Return reconstruction sample evidence as named tuples

## Location

`libs/dotnet/Rasm/.planning/Meshing/reconstruct.md:24-877` — C# fence anchors `ReconstructionSample` at lines 287-290 and the four internal evaluation returns at lines 596-684.

## From

```csharp
public readonly record struct ReconstructionSample(double Value, SampleFit Fit) {
    internal Fin<TOut> Project<TOut>(Op key);
}
```

```csharp
internal static Fin<ReconstructionSample> EvaluateMls(
    Seq<MlsSample> samples, MlsPolicy policy, Point3d sample, Context context, Op key) {
internal static Fin<(ReconstructionSample Sample, LevinFit Levin)> EvaluateLevinMls(
    Seq<MlsSample> samples, LevinMlsPolicy policy, Point3d sample, Context context, Op key) =>
internal static Fin<(ReconstructionSample Sample, ApssFit Apss)> EvaluateApss(
    Seq<MlsSample> samples, ApssPolicy policy, Point3d sample, Context context, Op key) =>
internal static Fin<ReconstructionSample> EvaluateSibson(
    NaturalNeighborField field, Arr<double> values, Point3d sample, Op key) =>
```

## To

```csharp
// ReconstructionSample DELETED
```

```csharp
internal static Fin<(double Value, SampleFit Fit)> EvaluateMls(
    Seq<MlsSample> samples, MlsPolicy policy, Point3d sample, Context context, Op key) {
internal static Fin<(double Value, SampleFit Fit, LevinFit Levin)> EvaluateLevinMls(
    Seq<MlsSample> samples, LevinMlsPolicy policy, Point3d sample, Context context, Op key) =>
internal static Fin<(double Value, SampleFit Fit, ApssFit Apss)> EvaluateApss(
    Seq<MlsSample> samples, ApssPolicy policy, Point3d sample, Context context, Op key) =>
internal static Fin<(double Value, SampleFit Fit)> EvaluateSibson(
    NaturalNeighborField field, Arr<double> values, Point3d sample, Op key) =>
```

Construct target-typed named tuples directly at the four existing `select` sites. Levin and APSS flatten the current nested `Sample` tuple into three named elements; MLS and Sibson retain the same `Value` and `Fit` names.

## Why

All four producers are internal to `Reconstruction`, and the complete `libs/dotnet` corpus has no consumer of the wrapper or its bodiless generic `Project` member. The real capability is the value-plus-evidence pair, which a named tuple carries without a public module type or a projection indirection. Flattening the two richer returns also removes the `result.Sample.Value`/`result.Sample.Fit` hop while retaining the Levin/APSS evidence as a distinct named element.

# 24. Admit the contour isovalue at the entry instead of wrapping it in a policy type

## Location

`libs/dotnet/Rasm/.planning/Meshing/reconstruct.md:24-877` — C# fence anchors `IsoContour.Detailed` at lines 849-863 and `IsoContourPolicy` at lines 865-868.

## From

```csharp
public static Fin<IsoContourResult> Detailed(
    ScalarField field, CellLattice grid, IsoContourPolicy policy, Context context, Op? key = null) {
```

```csharp
[StructLayout(LayoutKind.Auto)]
public readonly record struct IsoContourPolicy(double IsoValue) {
    public static readonly IsoContourPolicy Default = new(IsoValue: 0.0);
}
```

## To

```csharp
public static Fin<IsoContourResult> Detailed(
    ScalarField field, CellLattice grid, double isoValue, Context context, Op? key = null) {
    Op op = key.OrDefault();
    return from iso in op.AcceptValue(value: isoValue)
           from _ in guard(grid.Rank is 2, op.InvalidInput())
           from corners in CornerFieldOf(field: field, grid: grid, context: context, key: op)
           from cells in op.AcceptValue(value: ClassifyCells(corners: corners, grid: grid, isovalue: iso))
           from chains in ChainSegments(cells: cells, grid: grid, context: context, key: op)
           select new IsoContourResult(
               Loops: chains.Loops, IsoValue: iso,
               AmbiguousCellCount: cells.AmbiguousCount, OpenRunCount: chains.OpenRuns);
}
```

```csharp
// IsoContourPolicy DELETED
```

## Why

`IsoContourPolicy` wraps one unvalidated scalar, its `Default` has no consumer, and `Detailed` immediately unwraps the value three times. The iso level is operation input, not a reusable policy family. Admitting the scalar once at the entry removes one module-level type, one static member, and the wrapper construction while strengthening the current path, which can otherwise return an invalid result carrying a non-finite isovalue.

## Ripples

- `libs/dotnet/Rasm/.planning/Processing/extract.md:232`: pass `isoValue: level` directly to `IsoContour.Detailed` and remove `new IsoContourPolicy(...)`.

# 25. Return the internal tetrahedral sample as a named tuple

## Location

`libs/dotnet/Rasm/.planning/Meshing/reconstruct.md:24-877` — C# fence anchors `TetSignedHeatSample` at lines 457-458 and its sole producer declaration at line 721.

## From

```csharp
public readonly record struct TetSignedHeatSample(double Value, TetSignedHeatSolve Solve, TetLocation Location);
```

```csharp
internal static Fin<TetSignedHeatSample> SampleTetSignedHeat(
    TetMeshDomain domain, TetSignedHeatPolicy policy, Arr<double> values,
    Point3d sample, Context context, Op key);
```

## To

```csharp
// TetSignedHeatSample DELETED
```

```csharp
internal static Fin<(double Value, TetSignedHeatSolve Solve, TetLocation Location)> SampleTetSignedHeat(
    TetMeshDomain domain, TetSignedHeatPolicy policy, Arr<double> values,
    Point3d sample, Context context, Op key);
```

## Why

The sole producer is internal, and no `libs/dotnet` consumer names `TetSignedHeatSample`; the public record only packages three named values for that internal operation. A named tuple preserves the exact value, solve witness, and location evidence while deleting one public module-level type and its generated record surface. The sampling algorithm remains real and unchanged.
