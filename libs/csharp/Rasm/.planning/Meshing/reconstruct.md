# [RASM_RECONSTRUCTION_RECONSTRUCT]

`Reconstruction` owns one `Reconstruct` entry over `ReconstructionPolicy`; each policy case builds a `Spatial/fields` scalar field carrying typed reconstruction evidence. `SignedHeatSpine`, `MeshSdf`, and `IsoSurface` own the delegated signed-distance and native-extraction rails, every native callback boundary converted through `Op.Catch` and every failure kept on `Fin`; `IsoContour` is the managed rank-2 iso lane beside the native 3D adapter — the correctness rail the iso family shares with `Meshing/arrangement`'s `BooleanRoute` pattern.

`Spatial/fields` owns the `ScalarField` union and its frozen cases; this page owns the kernels those cases delegate to. `Meshing/mesh` owns the type-keyed `Memoized` solver slot and `SpdMassShift`, so this page declares `BoundarySignedHeatKey`/`ClosedSignedHeatKey` and composes the memo at `SignedHeatSpine.BoundarySolutionOf`/`ClosedSolutionOf`. `Meshing/dec` owns the Crouzeix-Raviart assembly the boundary-source row composes, `Spatial/index` the accelerated winding lane the GWN row composes, `Numerics/matrix` every linear solve, and `Numerics/calculus` the kernel-profile math.

## [01]-[INDEX]

- [02]-[RECONSTRUCTION]: `ReconstructionPolicy` construction discriminant and its one `Reconstruct` entry, the `SignedHeatDiscretization` four-stage spine, `SdfMeshMethod` mesh-SDF dispatch, native `IsoSurface` extraction, and the managed `IsoContour` rank-2 lane.

## [02]-[RECONSTRUCTION]

- Owner: `ReconstructionPolicy` `[Union]` is the one construction discriminant, each case carrying its typed policy payload and deriving its `ReconstructionMode` row; `Reconstruction` is the build/evaluate kernel; `SignedHeatDiscretization` `[Union]` the spine discriminant and `SignedHeatSpine` its one four-stage signed-heat law; `MeshSdf` the mesh-SDF dispatch over `SdfMeshMethod`; `IsoSurface` the native extraction adapter; `TetMeshDomain` the validated tetrahedral domain deriving its full boundary-face topology at admission. Every knob is a validated policy row carrying a preset.
- Cases: `ReconstructionPolicy` cases `RbfCase`/`MlsCase`/`LevinCase`/`ApssCase`/`PoissonCase` select the build kernel; `ReconstructionMode` carries the seven modes with normals, sparse-system, degree, and status columns; `SignedHeatDiscretization` cases `TetFem`/`BoundarySource`/`ClosedVolumeGrid`; `SdfMeshMethod` carries `GeneralizedWindingNumber`/`BoundarySignedHeat`/`ClosedSurfaceSignedHeat`, and the method row IS the mesh-SDF classification; `PoissonBoundary` carries `Neumann` (singular) and `Dirichlet` (definite); `IsoSurfaceStatus` carries four rows, each with its own receipt-evidence predicate. Every single-value policy enum lands one row in the fence.
- Entry: `Reconstruction.Reconstruct` is the one reconstruction entry — the policy case selects the build kernel, admission internalizes per case (finite positions/normals/values, mode-specific guards), and the result carries a frozen `Spatial/fields` case with `ReconstructionReceipt`. `SignedHeatSpine.Solve` routes each `SignedHeatDiscretization` case to its row kernel over the same four-stage law. `MeshSdf.SignedDistanceDetailed` dispatches on `policy.Method` and `MeshSdf.Prewarm` factors and caches the solves without sampling. `IsoSurface.Detailed` returns the classified receipt for every native outcome — admission failures alone fail the rail, consumers gate on `Receipt.Valid`. `IsoContour.Detailed` is the rank-2 managed analogue, gated `grid.Rank is 2`, chaining crossings into oriented `Meshing/intersect` `Chain` loops. No per-mode public factory siblings on the surface.
- Auto: RBF selects interpolation vs approximation by the smoothing row (`≤ ZeroTolerance` → exact kernel-matrix solve; `> 0` → `√smoothing`-diagonal-augmented least squares) — the mode split is a value consequence, not a knob. MLS solves the 4-equation-per-neighbor design (`[1, −offset] · [value; gradient]` rows weighted by `√profile`) and gates on rank ≥ 4 and normal agreement ≥ 0.5 against the weighted normal. Levin runs step one as covariance plane seed (smallest eigenvector, orientation-corrected) then alternates Brent root-finding on the weighted energy derivative along the normal (bracket/accuracy scale-derived from support) with normal re-estimation (at most `NormalMaxIter` inner steps against `NormalTau`, offset/normal convergence at `StepEps`/`NormalStepTol`), gated by the planarity ratio `λ0/λ2 ≤ PlanarityTau`; step two fits the ridge-regularized degree-`PolyDegree` height polynomial in the local tangent frame. APSS fits the algebraic sphere `(hc, hl, hq)` by Pratt normalization, classifies the plane-degenerate branch by `DegeneracyRatio ≤ EpsDegeneracy`, and projects iteratively with `StepDamping` under `ProjTol`. Poisson splats inward normals trilinearly onto the `2^Depth` lattice — the degree-1 discretization, the ONE splat the lattice owns (splat radius `Width`-scaled per cell; density estimate normalized by `SamplesPerNode` with weight floor `max(√ε, Density)`; bounding box grown by `Scale`) — assembles the 7-point Laplacian with one-sided boundary differences, adds `α = 8^Depth · PointWeight` screening outer products per sample when screened, imposes Dirichlet rows when `Boundary.IsDirichlet`, solves definite systems by `CholeskySparse` and singular ones by `SingularSolveDetailed` under `GaugePolicy.PinConstant(interior, GaugeShift.PinZero)` — residual-gated against `Solver.ResidualTolerance` like every other solve on this page — and derives the isovalue `γ` as the density-weighted mean sample indicator. Signed-heat rows specialize the same four stages across tet FEM, boundary CR, and closed volume-grid discretizations, heat time resolving per row from `SignedHeatTime`.
- Receipt: every build, point evaluation, and spine step carries its typed receipt as one `ValidityClaim.All` fold. `ReconstructionReceipt`/`ReconstructionSampleReceipt` ride the RBF/MLS rail with the interpolation verdict on `Mode.Status`; the deep `LevinMlsSampleReceipt`/`ApssSampleReceipt` carry their solver witnesses; `PoissonReceipt` carries the splat-conservation claim; `SignedHeatReceipt`/`VolumeGridReceipt`/`TetSignedHeatReceipt` sit per spine row, `SdfMeshReceipt` on the mesh-SDF rail, `IsoSurfaceReceipt` inside `IsoSurfaceResult`, and `IsoContourReceipt` witnessing the lattice, isovalue, ambiguous-cell census, and open-run count inside `IsoContourResult`.
- Packages: `Rasm.Numerics` `Numerics/matrix` (sparse and dense solves, gauge policy, solve receipts) and `Numerics/calculus` (kernel-profile math, composed never re-minted); `Meshing/mesh` (the `MeshSpace` snapshot, cache memo slots, `TopologyReceipt`) and `Meshing/dec` (CR heat-system assembly, face-field sampling, intrinsic divergence); `Spatial/fields` (`ScalarField` frozen cases as the build product); `Spatial/index` (`Spatial.Apply` over `SpatialQuery.Winding`, the accelerated GWN lane); `Domain/rails` and `Domain/context`; MathNet.Numerics (`RootFinding.Brent` for the Levin energy root); RhinoCommon (`Mesh.CreateFromIsosurface` and the inside/closest/orientation predicates, genuinely Rhino-boundary, never thinned); LanguageExt.Core; BCL (`Interlocked`).
- Growth: a new reconstruction family (partition-of-unity implicits, neural pull) is one `ReconstructionPolicy` case + one `ReconstructionMode` row + one build arm producing a new frozen field case; a new signed-heat discretization (polygon FEM, adaptive octree grid) is one `SignedHeatDiscretization` case + one stage row on the same four-stage spine — never a parallel heat→Poisson pipeline; a new mesh-SDF method is one `SdfMeshMethod` row; a new lattice boundary condition is one `PoissonBoundary` row with its column values; a grid ceiling change is a policy-row edit; a managed 3D iso lane is one `IsoSurfaceRoute` row over the same receipt law — the `BooleanRoute` pattern one rank up from the landed `IsoContour` rail; zero new entry surface.
- Boundary: `SignedHeatSpine` owns one heat→divergence→Poisson→calibrate law. Boundary-source rows reject flipped intrinsic snapshots; closed-grid rows admit only watertight, solid, closed, oriented topology. Lattice-backed samples outside the grid return the positive far value, never a clamp-to-edge fabricated interior. Native evaluator callbacks count failures with `Interlocked`; every linear solve routes through `Numerics/matrix`, and `Op.Catch` converts the native callback boundary.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using LanguageExt;
using Rasm.Domain;
using Rasm.Numerics;
using Rasm.Spatial;
using Rhino;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;
// CS0104 guard: Rhino.Geometry declares Matrix/Dimension homonyms under the dual usings.
using Dimension = Rasm.Numerics.Dimension;

namespace Rasm.Meshing;

// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class ReconstructionStatus {
    public static readonly ReconstructionStatus ExactInterpolation = new(key: 0);
    public static readonly ReconstructionStatus ApproximateSdf     = new(key: 1);
    public static readonly ReconstructionStatus PoissonIndicator   = new(key: 2);
}

// Status is a COLUMN — a pure function of the mode row, never a second bit carried beside Mode on a receipt.
[SmartEnum<int>]
public sealed partial class ReconstructionMode {
    public static readonly ReconstructionMode RbfInterpolation          = new(key: 0, requiresNormals: false, requiresSparseSystem: false, polynomialDegree: 0, status: ReconstructionStatus.ExactInterpolation);
    public static readonly ReconstructionMode RbfApproximation          = new(key: 1, requiresNormals: false, requiresSparseSystem: false, polynomialDegree: 0, status: ReconstructionStatus.ApproximateSdf);
    public static readonly ReconstructionMode MovingLeastSquares        = new(key: 2, requiresNormals: true, requiresSparseSystem: false, polynomialDegree: 1, status: ReconstructionStatus.ApproximateSdf);
    public static readonly ReconstructionMode LevinMovingLeastSquares   = new(key: 3, requiresNormals: true, requiresSparseSystem: false, polynomialDegree: 2, status: ReconstructionStatus.ApproximateSdf);
    public static readonly ReconstructionMode AlgebraicPointSetSurfaces = new(key: 4, requiresNormals: true, requiresSparseSystem: false, polynomialDegree: 2, status: ReconstructionStatus.ApproximateSdf);
    public static readonly ReconstructionMode Poisson                   = new(key: 5, requiresNormals: true, requiresSparseSystem: true, polynomialDegree: 0, status: ReconstructionStatus.PoissonIndicator);
    public static readonly ReconstructionMode ScreenedPoisson           = new(key: 6, requiresNormals: true, requiresSparseSystem: true, polynomialDegree: 0, status: ReconstructionStatus.PoissonIndicator);
    public bool RequiresNormals { get; }
    public bool RequiresSparseSystem { get; }
    public int PolynomialDegree { get; }
    public ReconstructionStatus Status { get; }
}

[SmartEnum<int>]
public sealed partial class PoissonBoundary {
    public static readonly PoissonBoundary Neumann   = new(key: 0, singular: true, exteriorValue: 0.0, isDirichlet: false);
    public static readonly PoissonBoundary Dirichlet = new(key: 1, singular: false, exteriorValue: -0.5, isDirichlet: true);
    public bool Singular { get; }
    public double ExteriorValue { get; }
    public bool IsDirichlet { get; }
}

// SdfMeshMethod IS the mesh-SDF classification — the method row carries it on the receipt.
[SmartEnum<int>]
public sealed partial class SdfMeshMethod {
    public static readonly SdfMeshMethod GeneralizedWindingNumber = new(key: 0);
    public static readonly SdfMeshMethod BoundarySignedHeat      = new(key: 1);
    public static readonly SdfMeshMethod ClosedSurfaceSignedHeat = new(key: 2);
}

[SmartEnum<int>]
public sealed partial class SdfSignConvention {
    public static readonly SdfSignConvention NegativeInsidePositiveOutside = new(key: 0, multiplier: 1.0);
    public static readonly SdfSignConvention PositiveInsideNegativeOutside = new(key: 1, multiplier: -1.0);
    public double Multiplier { get; }
}

[SmartEnum<int>]
public sealed partial class IsoSurfaceStatus {
    public static readonly IsoSurfaceStatus NativeValid = new(key: 0,
        admitsEvidence: static (failures, vertices, faces, naked) => failures == 0 && vertices > 0 && faces > 0 && naked.IsSome);
    public static readonly IsoSurfaceStatus EvaluatorFailure = new(key: 1,
        admitsEvidence: static (failures, _, _, _) => failures > 0);
    public static readonly IsoSurfaceStatus NativeReturnedNull = new(key: 2,
        admitsEvidence: static (failures, vertices, faces, naked) => failures == 0 && vertices == 0 && faces == 0 && naked.IsNone);
    public static readonly IsoSurfaceStatus NativeInvalidMesh = new(key: 3,
        admitsEvidence: static (failures, _, _, naked) => failures == 0 && naked.IsNone);

    [UseDelegateFromConstructor]
    internal partial bool AdmitsEvidence(int failures, int vertices, int faces, Option<int> nakedBoundaryLoops);
}

[SmartEnum<int>] public sealed partial class TetGaugePolicy { public static readonly TetGaugePolicy PinnedFirstBoundary = new(key: 0); }
[SmartEnum<int>] public sealed partial class TetInterpolation { public static readonly TetInterpolation Barycentric = new(key: 0); }
[SmartEnum<int>] public sealed partial class VolumeSolverKind { public static readonly VolumeSolverKind SparseCholeskyPinned = new(key: 0); }
[SmartEnum<int>] public sealed partial class VolumeBoundaryCondition { public static readonly VolumeBoundaryCondition NeumannGaugePinned = new(key: 0); }
// Volume sample reconstruction is the Numerics/atoms LatticeInterpolation row family — a page-local one-row
// interpolation enum was the deleted second trilinear owner.

// THE one construction discriminant: a policy case IS the mode selection; no per-mode factory siblings.
[Union]
public abstract partial record ReconstructionPolicy {
    private ReconstructionPolicy() { }
    public sealed record RbfCase(KernelKind Kernel, PositiveMagnitude Radius, double Smoothing) : ReconstructionPolicy;
    public sealed record MlsCase(KernelKind Kernel, PositiveMagnitude Radius) : ReconstructionPolicy;
    public sealed record LevinCase(LevinMlsPolicy Policy) : ReconstructionPolicy;
    public sealed record ApssCase(ApssPolicy Policy) : ReconstructionPolicy;
    public sealed record PoissonCase(PoissonPolicy Policy) : ReconstructionPolicy;
    public static Fin<ReconstructionPolicy> Rbf(KernelKind kernel, double radius, double smoothing = 0.0, Op? key = null);
    public static Fin<ReconstructionPolicy> Mls(KernelKind kernel, double radius, Op? key = null);
    public static Fin<ReconstructionPolicy> Levin(LevinMlsPolicy policy, Op? key = null);
    public static Fin<ReconstructionPolicy> Apss(ApssPolicy policy, Op? key = null);
    public static Fin<ReconstructionPolicy> Poisson(PoissonPolicy policy, Op? key = null);
    public ReconstructionMode Mode => Switch(
        rbfCase:     static c => c.Smoothing <= EpsilonPolicy.ZeroTolerance ? ReconstructionMode.RbfInterpolation : ReconstructionMode.RbfApproximation,
        mlsCase:     static _ => ReconstructionMode.MovingLeastSquares,
        levinCase:   static _ => ReconstructionMode.LevinMovingLeastSquares,
        apssCase:    static _ => ReconstructionMode.AlgebraicPointSetSurfaces,
        poissonCase: static c => c.Policy.PointWeight > 0.0 ? ReconstructionMode.ScreenedPoisson : ReconstructionMode.Poisson);
}

// THE spine discriminant: each case carries its domain and policy; the spine runs ONE four-stage law over the row.
[Union]
public abstract partial record SignedHeatDiscretization {
    private SignedHeatDiscretization() { }
    public sealed record TetFemCase(TetMeshDomain Domain, TetSignedHeatPolicy Policy) : SignedHeatDiscretization;
    public sealed record BoundarySourceCase(MeshSpace Space, SdfMeshPolicy Policy) : SignedHeatDiscretization;
    public sealed record ClosedVolumeGridCase(MeshSpace Space, SdfMeshPolicy Policy) : SignedHeatDiscretization;
    public static Fin<SignedHeatDiscretization> TetFem(TetMeshDomain domain, TetSignedHeatPolicy? policy = null, Op? key = null);
    public static Fin<SignedHeatDiscretization> BoundarySource(MeshSpace space, SdfMeshPolicy policy, Op? key = null);
    public static Fin<SignedHeatDiscretization> ClosedVolumeGrid(MeshSpace space, SdfMeshPolicy policy, Op? key = null);
}

// --- [CONSTANTS] ----------------------------------------------------------------------------
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct SignedHeatTime(Option<PositiveMagnitude> Explicit, PositiveMagnitude Coefficient) {
    public static Fin<SignedHeatTime> Scaled(double coefficient = 1.0, Op? key = null);
    public static Fin<SignedHeatTime> Fixed(double value, Op? key = null);
    // Eager IfNone — a None lambda would capture struct state (CS1673); the fallback multiply is cheap.
    internal double Resolve(double cellSize) =>
        Explicit.Map(static heat => heat.Value).IfNone(noneValue: Coefficient.Value * cellSize * cellSize);
    internal bool IsValid => Coefficient.Value > 0.0 && Explicit.Map(static heat => heat.Value > 0.0).IfNone(noneValue: true);
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct VolumeSolverPolicy(VolumeSolverKind Kind, PositiveMagnitude ResidualTolerance) {
    public const double DefaultRelativeResidualTolerance = 1.0e-8;
    public static Fin<VolumeSolverPolicy> SparseCholesky(double residualTolerance = DefaultRelativeResidualTolerance, Op? key = null);
    internal bool IsValid => Kind is not null && ResidualTolerance.Value > 0.0;
}

// Ceilings are POLICY ROWS: MaxNodes is the VALUE this policy passes as the CellLattice.Of ceiling, KernelSofteningRatio scales the heat-kernel softening.
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct VolumeGridPolicy(
    Option<Dimension> Resolution, Option<PositiveMagnitude> CellSize, PositiveMagnitude Padding,
    Dimension MaxNodes, UnitInterval KernelSofteningRatio) {
    public static Fin<VolumeGridPolicy> ByResolution(int resolution = 16, double padding = 1.0, Op? key = null);
    public static Fin<VolumeGridPolicy> ByCellSize(double cellSize, double padding = 1.0, Op? key = null);
    public static readonly Dimension DefaultMaxNodes = Dimension.Create(value: 1_000_000);
    public static readonly UnitInterval DefaultKernelSofteningRatio = UnitInterval.Create(value: 0.0625);
    internal bool IsValid => Padding.Value > 0.0 && Resolution.IsSome != CellSize.IsSome && MaxNodes.Value > 0;
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct SdfMeshPolicy(
    SdfMeshMethod Method, SdfSignConvention SignConvention, Option<VolumeGridPolicy> Grid,
    SignedHeatTime Heat, VolumeSolverPolicy Solver, LatticeInterpolation Interpolation, VolumeBoundaryCondition BoundaryCondition,
    double WindingBetaSquared) {
    // betaSquared = the SpatialQuery.Winding far-field acceptance ratio (Barill β = 2 default).
    public static Fin<SdfMeshPolicy> GeneralizedWinding(SdfSignConvention? signConvention = null, double betaSquared = 4.0, Op? key = null);
    public static Fin<SdfMeshPolicy> BoundarySignedHeat(SignedHeatTime? heat = null, VolumeSolverPolicy? solver = null, SdfSignConvention? signConvention = null, Op? key = null);
    public static Fin<SdfMeshPolicy> ClosedSignedHeat(VolumeGridPolicy grid, SignedHeatTime? heat = null, VolumeSolverPolicy? solver = null, SdfSignConvention? signConvention = null, Op? key = null);
    internal Fin<SdfMeshPolicy> Admit(Op key);       // grid present IFF ClosedSurfaceSignedHeat; heat/solver validity; WindingBetaSquared finite > 0
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct TetSignedHeatPolicy(
    SignedHeatTime Heat, VolumeSolverPolicy Solver, SdfSignConvention SignConvention, TetGaugePolicy Gauge, TetInterpolation Interpolation) {
    public static Fin<TetSignedHeatPolicy> Of(SignedHeatTime? heat = null, VolumeSolverPolicy? solver = null,
        SdfSignConvention? signConvention = null, TetGaugePolicy? gauge = null, TetInterpolation? interpolation = null, Op? key = null);
    internal Fin<TetSignedHeatPolicy> Admit(Op key);
}

// NormalMaxIter/NormalStepTol bound the inner normal re-estimation loop — no conjugate gradient exists on this rail.
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct LevinMlsPolicy(
    PositiveMagnitude Support, int PolyDegree, double NeglectEps, int MinNeighbors, double BracketFactor,
    int MaxOuterIter, double StepEps, double RootTol, int NormalMaxIter, double NormalStepTol, double PlanarityTau,
    double RidgeLambda, double NormalTau, double ProjEps, bool PlaneThroughPoint, bool OrientNormals, WeightKernelFamily WeightKernel) {
    public static Fin<LevinMlsPolicy> Of(double support, int polyDegree = 2, double neglectEps = 1e-3, int minNeighbors = 6,
        double bracketFactor = 2.0, int maxOuterIter = 16, double stepEps = 1e-4, double rootTol = 1e-6, int normalMaxIter = 32,
        double normalStepTol = 1e-6, double planarityTau = 0.25, double ridgeLambda = 0.0, double normalTau = 0.3, double projEps = 1e-4,
        bool planeThroughPoint = false, bool orientNormals = true, WeightKernelFamily? weightKernel = null, Op? key = null);
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct ApssPolicy(
    PositiveMagnitude Support, WeightKernelFamily WeightKernel, double Beta, double NeglectEps, double EpsDegeneracy,
    double EpsPratt, int ProjMaxIter, double ProjTol, double StepDamping, int MinNeighbors) {
    public static Fin<ApssPolicy> Of(double support, WeightKernelFamily? weightKernel = null, double beta = 1.0,
        double neglectEps = 1e-3, double epsDegeneracy = 1e-6, double epsPratt = 1e-9, int projMaxIter = 16,
        double projTol = 1e-4, double stepDamping = 1.0, int minNeighbors = 6, Op? key = null);
}

// Dense regular lattice: trilinear interpolation IS degree 1, and every lattice solve routes the one VolumeSolverPolicy
// as a direct factorization.
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct PoissonPolicy(
    Dimension Depth, PositiveMagnitude Width, PositiveMagnitude Scale, PositiveMagnitude SamplesPerNode,
    double PointWeight, PoissonBoundary Boundary, VolumeSolverPolicy Solver, Option<PositiveMagnitude> Density) {
    // density <= 0 maps to None — the splat weight floor is then the sqrt-eps floor alone, never a zero sentinel.
    public static Fin<PoissonPolicy> Of(int depth = 6, double width = 1.0, double scale = 1.1, double samplesPerNode = 1.5,
        double pointWeight = 0.0, PoissonBoundary? boundary = null, VolumeSolverPolicy? solver = null,
        double density = 0.0, Op? key = null);
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct IsoSurfacePolicy(Dimension MaxRootSteps, long MaxCells) {
    public static readonly IsoSurfacePolicy Default = new(MaxRootSteps: Dimension.Create(value: 32), MaxCells: 16_777_216L);
}

// --- [MODELS] -------------------------------------------------------------------------------
[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct MlsSample(Point3d Position, Vector3d Normal, double Value);

// Kernel/radius/smoothing are Option — absent for kernel-less modes, never fabricated zeros; the interpolation
// verdict rides Mode.Status. PolynomialDegree is the ACTUAL fitted degree, which a Levin policy may override.
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct ReconstructionReceipt(
    ReconstructionMode Mode, Option<KernelKind> Kernel, Option<double> Radius, Option<double> Smoothing,
    int SampleCount, int CenterCount, int PolynomialDegree, Option<SolveReceipt> Solve) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.CountAtLeast(count: SampleCount, floor: 1), ValidityClaim.CountAtLeast(count: CenterCount, floor: 0),
        ValidityClaim.CountAtLeast(count: PolynomialDegree, floor: 0),
        ValidityClaim.Of(Radius.Map(static r => ValidityClaim.Positive(r).Holds).IfNone(noneValue: true)),
        ValidityClaim.Of(Smoothing.Map(static s => ValidityClaim.Nonnegative(s).Holds).IfNone(noneValue: true)),
        ValidityClaim.Of(Solve.Map(static receipt => receipt.IsValid).IfNone(noneValue: true)));
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct ReconstructionResult(ScalarField Field, ReconstructionReceipt Receipt) {
    internal Fin<TOut> Project<TOut>(Op key) {
        ReconstructionResult self = this;
        return AtomProjection.Rows<ReconstructionResult, TOut>(self: self, key: key,
            ProjectionRow.Of<ReconstructionReceipt>(() => Fin.Succ(self.Receipt)),
            ProjectionRow.Of<ScalarField>(() => Optional(self.Field).ToFin(key.InvalidResult())));
    }
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct ReconstructionSample(double Value, ReconstructionSampleReceipt Receipt) {
    internal Fin<TOut> Project<TOut>(Op key);        // typed rows: receipt | double
}

// Status rides Mode.Status. Kernel/Radius are Option like the build receipt — Levin and APSS weight through
// WeightKernelFamily + Support on their deep receipts, so a fabricated KernelKind here would lie.
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct ReconstructionSampleReceipt(
    ReconstructionMode Mode, Option<KernelKind> Kernel, Option<double> Radius, int SampleCount,
    int NeighborhoodCount, int RejectedWeightCount, double WeightSum, int Rank,
    Option<double> Condition, Option<double> NormalAgreement, Option<double> GradientNorm, Option<SolveReceipt> Solve) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.CountAtLeast(count: SampleCount, floor: 1), ValidityClaim.CountAtLeast(count: NeighborhoodCount, floor: 0),
        ValidityClaim.CountAtLeast(count: RejectedWeightCount, floor: 0), ValidityClaim.CountAtLeast(count: Rank, floor: 0),
        ValidityClaim.Nonnegative(WeightSum),
        ValidityClaim.Of(Radius.Map(static r => ValidityClaim.Positive(r).Holds).IfNone(noneValue: true)),
        ValidityClaim.Of(Solve.Map(static receipt => receipt.IsValid).IfNone(noneValue: true)));
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct LevinMlsSampleReceipt(
    Point3d PlaneOrigin, Vector3d PlaneNormal, Vector3d MlsNormal, double Offset, Vector3d FrameU, Vector3d FrameV,
    int Step1Iterations, bool Step1Converged, int RootIterations, double RootResidual, double SecondDerivative,
    int NormalIterations, double NormalResidual, double Lambda0, double Lambda2, double Planarity,
    int NeighborCount, double WeightSum, double Step1Energy, int PolyDegree, int CoefficientCount,
    double Step2Residual, double Step2Rms, double DesignCondition, int Rank, double GradientMagnitude,
    double NormalAgreement, double ProjDisplacement, double ProjResidual, bool ProjConverged, bool PlaneThroughPoint, SolveReceipt Solve) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.CountAtLeast(count: NeighborCount, floor: 1), ValidityClaim.Positive(WeightSum),
        ValidityClaim.CountAtLeast(count: PolyDegree, floor: 1), ValidityClaim.CountAtLeast(count: CoefficientCount, floor: 1),
        ValidityClaim.CountAtLeast(count: Rank, floor: 1),
        ValidityClaim.Finite(PlaneOrigin), ValidityClaim.Finite(PlaneNormal), ValidityClaim.Finite(Offset),
        ValidityClaim.Finite(RootResidual), ValidityClaim.Finite(Step2Residual), ValidityClaim.Finite(GradientMagnitude),
        ValidityClaim.Evidence(Solve));
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct ApssSampleReceipt(
    double Hc, Vector3d Hl, double Hq, double PrattNormSquared, bool IsPlane, double DegeneracyRatio,
    Point3d Center, double Radius, double MeanCurvature, double FieldValue, double GradientNorm, Vector3d Normal,
    int NeighborCount, double WeightSum, int ProjIterations, double TaubinResidual, double ProjDisplacement) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Positive(PrattNormSquared), ValidityClaim.CountAtLeast(count: NeighborCount, floor: 1),
        ValidityClaim.Positive(WeightSum), ValidityClaim.Finite(FieldValue), ValidityClaim.Nonnegative(DegeneracyRatio),
        ValidityClaim.CountAtLeast(count: ProjIterations, floor: 0));
}

// Origin, spacing, and resolution are the lattice's; sample reconstruction is the LatticeInterpolation.Linear body
// which the lattice-backed ScalarField arm already runs — the second trilinear owner this record carried deleted with it.
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct PoissonGrid(CellLattice Grid, Arr<double> Chi, Arr<double> Density) : IValidityEvidence {
    internal static Fin<PoissonGrid> Of(CellLattice grid, Arr<double> chi, double[] density, Op key);
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.CountExactly(count: Chi.Count, expected: (int)Grid.CellCount),
        ValidityClaim.CountExactly(count: Density.Count, expected: Chi.Count),
        ValidityClaim.Of(holds: Grid.Columns.Value >= 2));
}

// CellLattice IS the census witness — a stored resolution scalar re-asserts what Grid.CellCount derives, and the
// cubic claim it anchored was false the moment the admitted lattice went anisotropic.
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct PoissonReceipt(
    ReconstructionMode Mode, Dimension Depth, CellLattice Grid, int SystemDof, PoissonBoundary Boundary,
    double PointWeight, double Scale, int SampleCount, int ContributionCount, int RejectedCount, int ClampedCount,
    double WeightSum, int LaplacianNonZeros, int ScreeningNonZeros, double RhsNorm, double Isovalue, double IsovalueStdDev,
    double MeanAbsChi, double MaxAbsChi, double GradientEnergy, double ScreeningEnergy, Option<double> DataResidual,
    double GradientResidual, bool UnscreenedEquivalence, Option<GaugeReceipt> Gauge, SolveReceipt Solve) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.CountExactly(count: SystemDof, expected: (int)Grid.CellCount),
        ValidityClaim.CountExactly(count: SystemDof, expected: Solve.Cols.Value),
        ValidityClaim.CountExactly(count: ContributionCount + RejectedCount + ClampedCount, expected: SampleCount),
        ValidityClaim.Nonnegative(WeightSum), ValidityClaim.Finite(Isovalue), ValidityClaim.Nonnegative(GradientEnergy),
        ValidityClaim.Nonnegative(ScreeningEnergy), ValidityClaim.Finite(GradientResidual),
        ValidityClaim.Of(!UnscreenedEquivalence || (ScreeningNonZeros == 0 && PointWeight <= 0.0 && ScreeningEnergy == 0.0 && DataResidual.IsNone)),
        ValidityClaim.Evidence(Solve),
        ValidityClaim.Of(Gauge.Map(static receipt => receipt.IsValid).IfNone(noneValue: true)));
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct SignedHeatReceipt(
    int BoundarySourceVertexCount, int BoundaryEncodedEdgeSourceCount, int BoundaryRejectedPointCount,
    int BoundaryUnmatchedSegmentCount, Option<SolveReceipt> HeatSolve, SolveReceipt PoissonSolve,
    Option<SpectralAssemblyReceipt> EdgeAssembly = default, Option<double> SpdMassShift = default) : IValidityEvidence {
    // Boundary counts floor at ZERO — the closed-grid spine row shares this receipt with no boundary sources.
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.CountAtLeast(count: BoundarySourceVertexCount, floor: 0),
        ValidityClaim.CountAtLeast(count: BoundaryEncodedEdgeSourceCount, floor: 0),
        ValidityClaim.CountAtLeast(count: BoundaryRejectedPointCount, floor: 0),
        ValidityClaim.CountAtLeast(count: BoundaryUnmatchedSegmentCount, floor: 0),
        ValidityClaim.Of(HeatSolve.Map(static receipt => receipt.IsValid).IfNone(noneValue: true)),
        ValidityClaim.Evidence(PoissonSolve),
        ValidityClaim.Of(EdgeAssembly.Map(static receipt => receipt.IsValid).IfNone(noneValue: true)),
        ValidityClaim.Of(SpdMassShift.Map(static shift => ValidityClaim.Positive(shift).Holds).IfNone(noneValue: true)));
}

// Bounds, per-axis census, cell size, node and cell counts all derive from the admitted lattice — the eight stored
// census columns this receipt carried were the drift the migration deleted; only run evidence remains stored.
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct VolumeGridReceipt(
    CellLattice Grid, double Padding, int SourceTriangleCount, int DegenerateTriangleCount, double SourceArea,
    int InsideNodeCount, int OutsideNodeCount, int NearSurfaceNodeCount, int RejectedVectorCount, double HeatTime,
    int GaugeNode, double SurfaceShift, LatticeInterpolation Interpolation, VolumeBoundaryCondition BoundaryCondition,
    VolumeSolverPolicy Solver, int OperatorNonZeros, Option<int> FactorNonZeros, double Residual) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.CountAtLeast(count: (int)Grid.NodeCount, floor: InsideNodeCount + OutsideNodeCount),
        ValidityClaim.Positive(HeatTime), ValidityClaim.Nonnegative(SourceArea),
        ValidityClaim.CountAtLeast(count: SourceTriangleCount, floor: 1), ValidityClaim.Finite(SurfaceShift), ValidityClaim.Finite(Residual));
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct SdfMeshReceipt(
    SdfMeshMethod Method, TopologyReceipt Topology,
    Option<SignedHeatReceipt> SignedHeat, Option<VolumeGridReceipt> VolumeGrid = default) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Of(SignedHeat.Map(static receipt => receipt.IsValid).IfNone(noneValue: true)),
        ValidityClaim.Of(VolumeGrid.Map(static receipt => receipt.IsValid).IfNone(noneValue: true)));
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct SdfMeshSample(double Distance, SdfMeshReceipt Receipt);

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct TetCell(int A, int B, int C, int D) { internal int[] Indices => [A, B, C, D]; }

// Admission derives the FULL boundary topology once: face-count map -> boundary faces (count==1) -> boundary vertices,
// cell volumes, interior count, total volume. Admit() re-derives and cross-checks a carried domain against rebuild.
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct TetMeshDomain(
    Seq<Point3d> Vertices, Seq<TetCell> Cells, Context Context, Arr<double> CellVolumes, Seq<int> BoundaryVertices,
    BoundingBox Bounds, int BoundaryFaceCount, int InteriorVertexCount, double TotalVolume) {
    public static Fin<TetMeshDomain> Of(Seq<Point3d> vertices, Seq<TetCell> cells, Context context, Op? key = null);
    internal Fin<TetMeshDomain> Admit(Op key);
    internal static Fin<TetCellMetric> MetricOf(Point3d[] points, TetCell cell, Op key);   // Jacobian-inverse P1 gradients
}
internal readonly record struct TetCellMetric(double Volume, Vector3d[] Gradients);

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct TetFemReceipt(
    int VertexCount, int CellCount, int BoundaryVertexCount, int BoundaryFaceCount, int InteriorVertexCount,
    int IncidenceCount, double TotalVolume, double MinCellVolume, double MaxCellVolume,
    int MassNonZeros, int StiffnessNonZeros, int HeatOperatorNonZeros, int DivergenceNonZeros, int RejectedGradientCellCount) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.CountExactly(count: BoundaryVertexCount + InteriorVertexCount, expected: VertexCount),
        ValidityClaim.Ordered(lower: MinCellVolume, upper: MaxCellVolume),
        ValidityClaim.CountAtLeast(count: CellCount, floor: 1), ValidityClaim.Positive(TotalVolume),
        ValidityClaim.CountAtLeast(count: RejectedGradientCellCount, floor: 0));
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct TetInterpolationReceipt(TetInterpolation Interpolation, int CellIndex, Arr<double> Barycentric, bool Inside) : IValidityEvidence {
    public bool IsValid => ValidityClaim.Of(!Inside || Barycentric.Count == 4);
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct TetSignedHeatReceipt(
    TetFemReceipt Fem, SignedHeatTime Heat, VolumeSolverPolicy Solver, SdfSignConvention SignConvention,
    TetGaugePolicy Gauge, TetInterpolation Interpolation, int GaugeVertex, double HeatTime,
    double BoundaryShift, double InteriorMean, SolveReceipt HeatSolve, SolveReceipt PoissonSolve) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Evidence(Fem), ValidityClaim.Positive(HeatTime),
        ValidityClaim.Finite(BoundaryShift), ValidityClaim.Finite(InteriorMean),
        ValidityClaim.Evidence(HeatSolve), ValidityClaim.Evidence(PoissonSolve));
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct TetSignedHeatSample(double Value, TetSignedHeatReceipt Receipt, TetInterpolationReceipt Interpolation);

// FixedTolerance/FixedNormalSampleDistance WITNESS the native evaluator's fixed internals — recorded, never chosen.
// Valid is DERIVED from Status — a second stored bit would be a desynchronizable duplicate. Grid IS the admitted
// CellLattice — bounds, per-axis census, cell size, and ceiling all derive from it, so the census-receipt sibling
// record deleted; the four sample-count columns are the evidence only this receipt carries.
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct IsoSurfaceReceipt(
    bool NativeRouted, IsoSurfaceStatus Status, CellLattice Grid, long HexCellCount, long CornerSampleCount,
    long CenterSampleCount, long InitialSampleCount, int MaxRootSteps, bool ParallelCallback,
    int EvaluatorFailures, int VertexCount, int FaceCount, Option<int> NakedBoundaryLoopCount,
    Option<double> FixedTolerance, Option<double> FixedNormalSampleDistance, Option<SdfMeshReceipt> MeshPreflight) : IValidityEvidence {
    public bool Valid => Status is not null && Status.Equals(IsoSurfaceStatus.NativeValid);
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Of(Status is not null && Status.AdmitsEvidence(
            failures: EvaluatorFailures, vertices: VertexCount, faces: FaceCount, nakedBoundaryLoops: NakedBoundaryLoopCount)),
        ValidityClaim.CountAtLeast(count: EvaluatorFailures, floor: 0),
        ValidityClaim.Of(NakedBoundaryLoopCount.Map(static count => count >= 0).IfNone(noneValue: true)),
        ValidityClaim.Of(MeshPreflight.Map(static receipt => receipt.IsValid).IfNone(noneValue: true)));
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct IsoSurfaceResult(Mesh Mesh, IsoSurfaceReceipt Receipt);

// Spine carriers and memo key records for the Meshing/mesh type-keyed Memoized slot, declared beside their kernels.
internal readonly record struct BoundarySignedHeatKey(SignedHeatTime Heat, VolumeSolverPolicy Solver);
internal readonly record struct ClosedSignedHeatKey(VolumeGridPolicy Grid, SignedHeatTime Heat, VolumeSolverPolicy Solver, LatticeInterpolation Interpolation, VolumeBoundaryCondition BoundaryCondition);
internal readonly record struct SignedHeatSolution(Arr<double> Values, SignedHeatReceipt Receipt, TopologyReceipt Topology);
// VolumeGridReceipt IS the domain — the lattice carries extent, census, addressing, and ceiling whole (node count,
// linearization, and node position read Grid.NodeCount / Grid.Linear / Grid.Corner), so the domain wrapper that
// re-carried the lattice beside its own receipt deleted with the local stride arithmetic.
internal readonly record struct ClosedSignedHeatSolution(VolumeGridReceipt Grid, Arr<double> Values, SignedHeatReceipt Receipt, TopologyReceipt Topology);

[Union]
public abstract partial record SignedHeatOutcome {
    private SignedHeatOutcome() { }
    public sealed record SurfaceCase(SignedHeatSolution Solution) : SignedHeatOutcome;
    public sealed record VolumeCase(ClosedSignedHeatSolution Solution) : SignedHeatOutcome;
    public sealed record TetCase(Arr<double> Values, TetSignedHeatReceipt Receipt) : SignedHeatOutcome;
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class Reconstruction {
    public static Fin<ReconstructionResult> Reconstruct(Seq<MlsSample> samples, ReconstructionPolicy policy, Context context, Op? key = null) =>
        key.OrDefault() switch {
            Op op => policy.Switch(
                rbfCase:     c => BuildRbf(samples: samples.Map(static s => (s.Position, s.Value)), kernel: c.Kernel, radius: c.Radius, smoothing: c.Smoothing, key: op),
                mlsCase:     c => BuildMls(samples: samples, kernel: c.Kernel, radius: c.Radius, context: context, key: op),
                levinCase:   c => BuildLevin(samples: samples, policy: c.Policy, context: context, key: op),
                apssCase:    c => BuildApss(samples: samples, policy: c.Policy, context: context, key: op),
                poissonCase: c => BuildPoisson(samples: samples, policy: c.Policy, context: context, key: op)),
        };
    // RBF (RK-1): centers ARE the samples. Exact interpolation solves Φc = v; approximation stacks √smoothing·I under
    // Φ against a zero tail — the Tikhonov normal equations without forming ΦᵀΦ. A conditionally-positive-definite
    // kernel row (PolynomialOrder > 0) appends the reproduction tail [Φ P; Pᵀ 0], constants for order 1, affine for 2.
    private static Fin<ReconstructionResult> BuildRbf(Seq<(Point3d Position, double Value)> samples, KernelKind kernel, PositiveMagnitude radius, double smoothing, Op key) {
        (Point3d Position, double Value)[] points = [.. samples];
        int n = points.Length, tail = kernel.PolynomialOrder switch { 0 => 0, 1 => 1, _ => 4 };
        bool exact = smoothing <= EpsilonPolicy.ZeroTolerance;
        int rows = exact ? n + tail : (2 * n) + tail, cols = n + tail;
        double[] design = new double[rows * cols];
        double[] rhs = new double[rows];
        for (int i = 0; i < n; i++) {
            rhs[i] = points[i].Value;
            for (int j = 0; j < n; j++) design[(i * cols) + j] = kernel.Weight(distance: points[i].Position.DistanceTo(other: points[j].Position), radius: radius.Value);
            for (int t = 0; t < tail; t++) {
                double basis = t switch { 0 => 1.0, 1 => points[i].Position.X, 2 => points[i].Position.Y, _ => points[i].Position.Z };
                design[(i * cols) + n + t] = basis;
                if (exact) design[((n + t) * cols) + i] = basis;
            }
            if (!exact) design[((n + i) * cols) + i] = Math.Sqrt(d: smoothing);
        }
        if (!exact) for (int t = 0; t < tail; t++) for (int i = 0; i < n; i++) design[(((2 * n) + t) * cols) + i] = t switch { 0 => 1.0, 1 => points[i].Position.X, 2 => points[i].Position.Y, _ => points[i].Position.Z };
        return from _ in guard(n >= 1, key.InvalidInput()).ToFin()
               from rowDim in key.AcceptValidated<Dimension>(candidate: rows)
               from colDim in key.AcceptValidated<Dimension>(candidate: cols)
               from matrix in Matrix.Of(rows: rowDim, cols: colDim, entries: toArr(design), key: key)
               from solve in exact ? matrix.SolveDetailed(rhs: toArr(rhs), key: key) : matrix.LeastSquaresDetailed(rhs: toArr(rhs), key: key)
               let coefficients = toArr(solve.Solution.Take(count: n))
               select new ReconstructionResult(
                   Field: new ScalarField.RbfCase(Samples: samples, Kernel: kernel, Radius: radius, Coefficients: coefficients,
                       Receipt: ReceiptOf(mode: exact ? ReconstructionMode.RbfInterpolation : ReconstructionMode.RbfApproximation, kernel: kernel, radius: radius.Value, smoothing: smoothing, n: n, degree: tail is 0 ? 0 : tail is 1 ? 0 : 1, solve: solve)),
                   Receipt: ReceiptOf(mode: exact ? ReconstructionMode.RbfInterpolation : ReconstructionMode.RbfApproximation, kernel: kernel, radius: radius.Value, smoothing: smoothing, n: n, degree: tail is 0 ? 0 : tail is 1 ? 0 : 1, solve: solve));
    }
    private static ReconstructionReceipt ReceiptOf(ReconstructionMode mode, KernelKind kernel, double radius, double smoothing, int n, int degree, SolveReceipt solve) =>
        new(Mode: mode, Kernel: Some(kernel), Radius: Some(radius), Smoothing: Some(smoothing),
            SampleCount: n, CenterCount: n, PolynomialDegree: degree, Solve: Some(solve));

    // MLS (RK-3): BUILD IS ADMISSION ONLY — MLS is evaluated, never fitted. Every sample gates finite with a valid
    // normal, the rank floor is four (the value + gradient design's unknown count), and the per-sample work is EvaluateMls.
    private static Fin<ReconstructionResult> BuildMls(Seq<MlsSample> samples, KernelKind kernel, PositiveMagnitude radius, Context context, Op key) =>
        from _ in guard(samples.Count >= 4 && samples.ForAll(static s => s.Position.IsValid && s.Normal.IsValid && double.IsFinite(s.Value)), key.InvalidInput()).ToFin()
        let receipt = new ReconstructionReceipt(Mode: ReconstructionMode.MovingLeastSquares, Kernel: Some(kernel), Radius: Some(radius.Value),
            Smoothing: Option<double>.None, SampleCount: samples.Count, CenterCount: samples.Count, PolynomialDegree: 1, Solve: Option<SolveReceipt>.None)
        select new ReconstructionResult(Field: new ScalarField.MlsCase(Samples: samples, Kernel: kernel, Radius: radius, Receipt: receipt), Receipt: receipt);
    private static Fin<ReconstructionResult> BuildLevin(Seq<MlsSample> samples, LevinMlsPolicy policy, Context context, Op key) =>
        from _ in guard(samples.Count >= policy.MinNeighbors && samples.ForAll(static s => s.Position.IsValid && s.Normal.IsValid && double.IsFinite(s.Value)), key.InvalidInput()).ToFin()
        let receipt = new ReconstructionReceipt(Mode: ReconstructionMode.LevinMovingLeastSquares, Kernel: Option<KernelKind>.None, Radius: Some(policy.Support.Value),
            Smoothing: Option<double>.None, SampleCount: samples.Count, CenterCount: samples.Count, PolynomialDegree: policy.PolyDegree, Solve: Option<SolveReceipt>.None)
        select new ReconstructionResult(Field: new ScalarField.LevinMlsCase(Samples: samples, Policy: policy, Receipt: receipt), Receipt: receipt);
    private static Fin<ReconstructionResult> BuildApss(Seq<MlsSample> samples, ApssPolicy policy, Context context, Op key) =>
        from _ in guard(samples.Count >= policy.MinNeighbors && samples.ForAll(static s => s.Position.IsValid && s.Normal.IsValid && double.IsFinite(s.Value)), key.InvalidInput()).ToFin()
        let receipt = new ReconstructionReceipt(Mode: ReconstructionMode.AlgebraicPointSetSurfaces, Kernel: Option<KernelKind>.None, Radius: Some(policy.Support.Value),
            Smoothing: Option<double>.None, SampleCount: samples.Count, CenterCount: samples.Count, PolynomialDegree: 2, Solve: Option<SolveReceipt>.None)
        select new ReconstructionResult(Field: new ScalarField.ApssCase(Samples: samples, Policy: policy, Receipt: receipt), Receipt: receipt);

    // Poisson (RK-7): the lattice body at degree 1. (a) grow the sample box by Scale and admit the 2^Depth lattice
    // through CellLattice.Of — the ceiling derives from Depth, so an over-deep request refuses at admission; (b)
    // TRILINEAR-splat each inward normal onto the eight surrounding nodes with the Width-scaled radius, density
    // normalized by SamplesPerNode over the sqrt-eps weight floor, counting Contribution/Rejected/Clamped so the
    // receipt census holds by construction; (c) assemble the 7-point Laplacian by FromTriplets with ONE-SIDED
    // boundary differences; (d) PointWeight > 0 adds α = 8^Depth·PointWeight screening outer products per sample;
    // (e) a Dirichlet boundary imposes its explicit rows; (f) singular routes through SingularSolveDetailed under
    // GaugePolicy.PinConstant, definite through CholeskySparse — both residual-gated on Solver.ResidualTolerance;
    // (g) γ = the density-weighted mean sample indicator with IsovalueStdDev off the same pass, and
    // UnscreenedEquivalence is set from the receipt's own four-fact conjunction, never a fifth flag.
    private static Fin<ReconstructionResult> BuildPoisson(Seq<MlsSample> samples, PoissonPolicy policy, Context context, Op key) {
        int resolution = 1 << policy.Depth.Value;
        return from bounds in key.AcceptValue(value: GrownBounds(samples: samples, scale: policy.Scale.Value))
               from cell in key.AcceptValidated<PositiveMagnitude>(candidate: bounds.Diagonal.MaximumCoordinate / resolution)
               from grid in CellLattice.Of(bounds: bounds, cell: cell, ceiling: (long)resolution * resolution * resolution, key: key)
               from splat in key.AcceptValue(value: SplatNormals(samples: samples, grid: grid, policy: policy))
               from laplacian in AssembleLaplacian(grid: grid, policy: policy, splat: splat, key: key)
               from solve in policy.Boundary.IsDirichlet || policy.PointWeight > 0.0
                   ? CholeskySparse.Of(symmetric: laplacian.System, key: key).Bind(factor => factor.SolveDetailed(rhs: laplacian.Rhs, key: key))
                   : laplacian.System.SingularSolveDetailed(rhs: laplacian.Rhs, gauge: GaugePolicy.PinConstant(index: 0, shift: GaugeShift.PinZero), context: context, key: key)
               from _ in guard(solve.Residual <= policy.Solver.ResidualTolerance.Value, key.InvalidResult()).ToFin()
               from gamma in key.AcceptValue(value: IsovalueOf(samples: samples, grid: grid, chi: solve.Solution, splat: splat))
               from poissonGrid in PoissonGrid.Of(grid: grid, chi: solve.Solution, density: splat.Density, key: key)
               let receipt = PoissonReceiptOf(policy: policy, grid: grid, splat: splat, laplacian: laplacian, solve: solve, gamma: gamma, sampleCount: samples.Count)
               select new ReconstructionResult(
                   Field: new ScalarField.PoissonCase(Grid: poissonGrid, Gamma: gamma.Value, Receipt: receipt),
                   Receipt: new ReconstructionReceipt(Mode: policy.PointWeight > 0.0 ? ReconstructionMode.ScreenedPoisson : ReconstructionMode.Poisson,
                       Kernel: Option<KernelKind>.None, Radius: Option<double>.None, Smoothing: Option<double>.None,
                       SampleCount: samples.Count, CenterCount: (int)grid.CellCount, PolynomialDegree: 1, Solve: Some(solve)));
    }
    private static BoundingBox GrownBounds(Seq<MlsSample> samples, double scale);
    // Named-kernel triple loops: splat, stencil assembly, and isovalue each walk the lattice census once.
    private static (double[] Vector, double[] Density, int ContributionCount, int RejectedCount, int ClampedCount, double WeightSum) SplatNormals(Seq<MlsSample> samples, CellLattice grid, PoissonPolicy policy);
    private static Fin<(SparseMatrix System, Arr<double> Rhs, int LaplacianNonZeros, int ScreeningNonZeros)> AssembleLaplacian(CellLattice grid, PoissonPolicy policy, (double[] Vector, double[] Density, int ContributionCount, int RejectedCount, int ClampedCount, double WeightSum) splat, Op key);
    private static (double Value, double StdDev) IsovalueOf(Seq<MlsSample> samples, CellLattice grid, Arr<double> chi, (double[] Vector, double[] Density, int ContributionCount, int RejectedCount, int ClampedCount, double WeightSum) splat);
    private static PoissonReceipt PoissonReceiptOf(PoissonPolicy policy, CellLattice grid, (double[] Vector, double[] Density, int ContributionCount, int RejectedCount, int ClampedCount, double WeightSum) splat, (SparseMatrix System, Arr<double> Rhs, int LaplacianNonZeros, int ScreeningNonZeros) laplacian, SolveReceipt solve, (double Value, double StdDev) gamma, int sampleCount);

    // RK-2 — total point fold: compact rows contribute exact zeros past q > 1, so no neighborhood cull exists.
    internal static Fin<double> EvaluateRbf(Seq<(Point3d Position, double Value)> samples, KernelKind kernel, double radius, Arr<double> coefficients, Point3d sample, Op key) =>
        from _ in guard(coefficients.Count == samples.Count, key.InvalidInput()).ToFin()
        from value in key.AcceptValue(value: samples.Map((index, s) => coefficients[index] * kernel.Weight(distance: sample.DistanceTo(other: s.Position), radius: radius)).Sum())
        select value;
    // RK-4 — the four-equation-per-neighbor linear design: for each neighbor with offset d = pᵢ − sample and
    // w = √profile, one value row w·[1, −d] and three gradient rows w·[0, eⱼ] against w·nᵢ·eⱼ; solved 4n×4 by
    // LeastSquaresDetailed, gated Rank >= 4 and NormalAgreement = ∇f̂ · n̄ >= 0.5.
    internal static Fin<ReconstructionSample> EvaluateMls(Seq<MlsSample> samples, KernelKind kernel, double radius, Point3d sample, Context context, Op key) {
        return from hood in CollectNeighborhood(samples: samples, sample: sample, support: radius, kernel: WeightKernelFamily.WendlandC2, neglectEps: 1e-3, minNeighbors: 4, context: context, key: key)
               from design in DesignOf(hood: hood, key: key)
               from solve in design.Matrix.LeastSquaresDetailed(rhs: design.Rhs, key: key)
               from _ in guard(solve.FullRank.IfNone(noneValue: true), key.InvalidResult()).ToFin()
               let gradient = new Vector3d(x: solve.Solution[1], y: solve.Solution[2], z: solve.Solution[3])
               let agreement = AgreementOf(gradient: gradient, hood: hood)
               from __ in guard(agreement >= 0.5, key.InvalidResult()).ToFin()
               select new ReconstructionSample(Value: solve.Solution[0],
                   Receipt: new ReconstructionSampleReceipt(Mode: ReconstructionMode.MovingLeastSquares, Kernel: Some(kernel), Radius: Some(radius),
                       SampleCount: samples.Count, NeighborhoodCount: hood.Length, RejectedWeightCount: samples.Count - hood.Length,
                       WeightSum: hood.Sum(static n => n.Weight), Rank: 4,
                       Condition: Option<double>.None, NormalAgreement: Some(agreement), GradientNorm: Some(gradient.Length), Solve: Some(solve)));
        static Fin<(Matrix Matrix, Arr<double> Rhs)> DesignOf(Neighbor[] hood, Op key);
        static double AgreementOf(Vector3d gradient, Neighbor[] hood);
    }
    // RK-5 — two-step Levin. Step one converges the local reference plane, step two fits the height polynomial;
    // every LevinMlsSampleReceipt column is a direct read of the run — no column is asserted.
    internal static Fin<(ReconstructionSample Sample, LevinMlsSampleReceipt Levin)> EvaluateLevinMls(Seq<MlsSample> samples, LevinMlsPolicy policy, Point3d sample, Context context, Op key) =>
        from hood in CollectNeighborhood(samples: samples, sample: sample, support: policy.Support.Value, kernel: policy.WeightKernel,
            neglectEps: policy.NeglectEps, minNeighbors: policy.MinNeighbors, context: context, key: key)
        from plane in ConvergeLevinPlane(hood: hood, sample: sample, policy: policy, key: key)
        from _ in guard(plane.Planarity <= policy.PlanarityTau, key.InvalidResult()).ToFin()
        from fit in FitLevinHeight(hood: hood, plane: plane, policy: policy, key: key)
        let receipt = new LevinMlsSampleReceipt(
            PlaneOrigin: plane.Origin, PlaneNormal: plane.Normal, MlsNormal: plane.MlsNormal, Offset: plane.Offset,
            FrameU: plane.FrameU, FrameV: plane.FrameV, Step1Iterations: plane.Step1Iterations, Step1Converged: plane.Step1Converged,
            RootIterations: plane.RootIterations, RootResidual: plane.RootResidual, SecondDerivative: plane.SecondDerivative,
            NormalIterations: plane.NormalIterations, NormalResidual: plane.NormalResidual,
            Lambda0: plane.Lambda0, Lambda2: plane.Lambda2, Planarity: plane.Planarity,
            NeighborCount: hood.Length, WeightSum: hood.Sum(static n => n.Weight), Step1Energy: plane.Step1Energy,
            PolyDegree: policy.PolyDegree, CoefficientCount: fit.CoefficientCount, Step2Residual: fit.Step2Residual,
            Step2Rms: fit.Step2Rms, DesignCondition: fit.DesignCondition, Rank: fit.Rank,
            GradientMagnitude: fit.Gradient.Length, NormalAgreement: fit.NormalAgreement,
            ProjDisplacement: fit.ProjDisplacement, ProjResidual: fit.ProjResidual, ProjConverged: fit.ProjConverged,
            PlaneThroughPoint: policy.PlaneThroughPoint, Solve: fit.Solve)
        select (new ReconstructionSample(Value: fit.Value,
                Receipt: new ReconstructionSampleReceipt(Mode: ReconstructionMode.LevinMovingLeastSquares, Kernel: Option<KernelKind>.None,
                    Radius: Some(policy.Support.Value), SampleCount: samples.Count, NeighborhoodCount: hood.Length,
                    RejectedWeightCount: samples.Count - hood.Length, WeightSum: hood.Sum(static n => n.Weight), Rank: fit.Rank,
                    Condition: Some(fit.DesignCondition), NormalAgreement: Some(fit.NormalAgreement),
                    GradientNorm: Some(fit.Gradient.Length), Solve: Some(fit.Solve))),
            receipt);
    // STEP ONE kernel — weighted-covariance seed: SymmetricMatrix.Of over the packed weighted moments,
    // DecomposeEigenDetailed, plane normal = smallest eigenvector orientation-corrected against the weighted normal, the
    // λ0/λ2 planarity ratio read off the same spectrum. Then alternate RootFinding.Brent on the stationarity
    // d/dt Σ wᵢ(t)·((pᵢ − (sample + t·n))·n)² over the bracket ±BracketFactor·Support at RootTol with inner normal
    // re-estimation (at most NormalMaxIter steps at NormalStepTol against NormalTau), outer convergence at StepEps
    // under the MaxOuterIter budget; Step1Energy is the converged weighted energy, SecondDerivative the bracketing
    // curvature witness, and PlaneThroughPoint pins the origin to the sample instead of the converged offset.
    private static Fin<LevinPlane> ConvergeLevinPlane(Neighbor[] hood, Point3d sample, LevinMlsPolicy policy, Op key);
    private readonly record struct LevinPlane(
        Point3d Origin, Vector3d Normal, Vector3d MlsNormal, double Offset, Vector3d FrameU, Vector3d FrameV,
        int Step1Iterations, bool Step1Converged, int RootIterations, double RootResidual, double SecondDerivative,
        int NormalIterations, double NormalResidual, double Lambda0, double Lambda2, double Planarity, double Step1Energy);
    // STEP TWO kernel — the degree-PolyDegree bivariate height fit in the (FrameU, FrameV) frame at the converged
    // origin: monomial design rows per neighbor weighted by √profile, RidgeLambda on the diagonal, one
    // LeastSquaresDetailed solve; value and gradient read off the coefficients at the sample's frame coordinates,
    // NormalAgreement = fitted normal · weighted normal, and the iterative projection under ProjEps closes
    // ProjDisplacement/ProjResidual/ProjConverged. CoefficientCount = (d+1)(d+2)/2 for degree d.
    private static Fin<LevinHeight> FitLevinHeight(Neighbor[] hood, LevinPlane plane, LevinMlsPolicy policy, Op key);
    private readonly record struct LevinHeight(
        double Value, Vector3d Gradient, int CoefficientCount, double Step2Residual, double Step2Rms,
        double DesignCondition, int Rank, double NormalAgreement, double ProjDisplacement, double ProjResidual,
        bool ProjConverged, SolveReceipt Solve);
    // RK-6 — Pratt-normalized algebraic sphere over the shared neighborhood fold; the closed-form moments make the
    // fit one pass, and the plane-degenerate branch is a value consequence of DegeneracyRatio, never a knob.
    internal static Fin<(ReconstructionSample Sample, ApssSampleReceipt Apss)> EvaluateApss(Seq<MlsSample> samples, ApssPolicy policy, Point3d sample, Context context, Op key) =>
        from hood in CollectNeighborhood(samples: samples, sample: sample, support: policy.Support.Value, kernel: policy.WeightKernel,
            neglectEps: policy.NeglectEps, minNeighbors: policy.MinNeighbors, context: context, key: key)
        from fit in AlgebraicSphereOf(hood: hood, support: policy.Support.Value, epsDegeneracy: policy.EpsDegeneracy, epsPratt: policy.EpsPratt, key: key)
        from projected in key.AcceptValue(value: ProjectApss(fit: fit, sample: sample, policy: policy))
        let gradient = fit.IsPlane ? fit.Hl : fit.Hl + (2.0 * fit.Hq * (Vector3d)sample)
        let value = fit.Hc + (fit.Hl * (Vector3d)sample) + (fit.Hq * ((Vector3d)sample * (Vector3d)sample))
        let receipt = new ApssSampleReceipt(
            Hc: fit.Hc, Hl: fit.Hl, Hq: fit.Hq, PrattNormSquared: fit.PrattNormSquared, IsPlane: fit.IsPlane,
            DegeneracyRatio: fit.DegeneracyRatio, Center: fit.Center, Radius: fit.Radius, MeanCurvature: fit.MeanCurvature,
            FieldValue: value, GradientNorm: gradient.Length, Normal: gradient.Length > EpsilonPolicy.ZeroTolerance ? gradient / gradient.Length : fit.Hl,
            NeighborCount: hood.Length, WeightSum: hood.Sum(static n => n.Weight),
            ProjIterations: projected.Iterations, TaubinResidual: projected.TaubinResidual, ProjDisplacement: projected.Displacement)
        select (new ReconstructionSample(Value: value,
                Receipt: new ReconstructionSampleReceipt(Mode: ReconstructionMode.AlgebraicPointSetSurfaces, Kernel: Option<KernelKind>.None,
                    Radius: Some(policy.Support.Value), SampleCount: samples.Count, NeighborhoodCount: hood.Length,
                    RejectedWeightCount: samples.Count - hood.Length, WeightSum: hood.Sum(static n => n.Weight), Rank: 4,
                    Condition: Option<double>.None, NormalAgreement: Option<double>.None,
                    GradientNorm: Some(gradient.Length), Solve: Option<SolveReceipt>.None)),
            receipt);
    // Weighted moments Sw, Σw·p, Σw·n, Σw·(p·n), Σw·|p|² give hq = 0.5·(Σw(p·n) − Σwp·Σwn/Σw)/(Σw|p|² − |Σwp|²/Σw),
    // hl = (Σwn − 2hq·Σwp)/Σw, hc = −(hl·Σwp + hq·Σw|p|²)/Σw; PrattNormSquared = |hl|² − 4·hc·hq gated > epsPratt;
    // DegeneracyRatio = |hq|·support/|hl| ≤ epsDegeneracy selects the PLANE branch (Center/Radius unset, MeanCurvature 0),
    // else Center = −hl/(2hq), Radius = √PrattNormSquared/(2|hq|), MeanCurvature = 2hq/√PrattNormSquared.
    private static Fin<AlgebraicSphere> AlgebraicSphereOf(Neighbor[] hood, double support, double epsDegeneracy, double epsPratt, Op key);
    private readonly record struct AlgebraicSphere(
        double Hc, Vector3d Hl, double Hq, double PrattNormSquared, bool IsPlane, double DegeneracyRatio,
        Point3d Center, double Radius, double MeanCurvature);
    // Damped sphere projection: p ← p − StepDamping·f(p)·∇f(p)/|∇f(p)|² until |f| ≤ ProjTol within ProjMaxIter;
    // TaubinResidual = f(sample)/|∇f(sample)| — the first-order distance the receipt witnesses beside the walk.
    private static (int Iterations, double Displacement, double TaubinResidual) ProjectApss(AlgebraicSphere fit, Point3d sample, ApssPolicy policy);
    // RK-8 — THE one neighborhood fold RK-4..RK-6 share; a per-kernel cull is the deleted form. Neglect radius
    // = support·√(ln(1/eps)); weights below context.Relative drop; a survivor count under minNeighbors fails typed.
    private static Fin<Neighbor[]> CollectNeighborhood(Seq<MlsSample> samples, Point3d sample, double support, WeightKernelFamily kernel, double neglectEps, int minNeighbors, Context context, Op key) {
        double neglect = support * Math.Sqrt(d: Math.Log(d: 1.0 / neglectEps));
        Neighbor[] survivors = [.. samples
            .Map(s => (Sample: s, Offset: s.Position - sample))
            .Filter(pair => pair.Offset.Length <= neglect)
            .Map(pair => new Neighbor(Sample: pair.Sample, Offset: pair.Offset, Distance: pair.Offset.Length,
                Weight: kernel.Weight(distance: pair.Offset.Length, support: support)))
            .Filter(n => n.Weight > context.Relative.Value)];
        return survivors.Length >= minNeighbors ? Fin.Succ(survivors) : Fin.Fail<Neighbor[]>(error: key.InvalidInput());
    }
    private readonly record struct Neighbor(MlsSample Sample, Vector3d Offset, double Distance, double Weight);
}

// THE unified signed-heat spine: heat diffusion -> unit-gradient divergence -> gauge-fixed Poisson -> sign calibration.
// One law, three discretization rows; heat time resolves per row from SignedHeatTime against the row's cell size.
public static class SignedHeatSpine {
    public static Fin<SignedHeatOutcome> Solve(SignedHeatDiscretization discretization, Op? key = null) =>
        key.OrDefault() switch {
            Op op => discretization.Switch(
                tetFemCase:         c => SolveTetSignedHeat(domain: c.Domain, policy: c.Policy, key: op)
                                             .Map(solved => (SignedHeatOutcome)new SignedHeatOutcome.TetCase(Values: solved.Values, Receipt: solved.Receipt)),
                boundarySourceCase: c => BoundarySolutionOf(space: c.Space, policy: c.Policy, key: op)
                                             .Map(solution => (SignedHeatOutcome)new SignedHeatOutcome.SurfaceCase(Solution: solution)),
                closedVolumeGridCase: c => ClosedSolutionOf(space: c.Space, policy: c.Policy, key: op)
                                             .Map(solution => (SignedHeatOutcome)new SignedHeatOutcome.VolumeCase(Solution: solution))),
        };

    // THE one memo composition point per cached row, keyed by the row's key record. SignConvention and Method stay
    // OUTSIDE the keys — the solution is sign-agnostic (the multiplier applies at sampling), so both conventions share one solve.
    internal static Fin<SignedHeatSolution> BoundarySolutionOf(MeshSpace space, SdfMeshPolicy policy, Op key) =>
        space.Cache.Memoized(probe: new BoundarySignedHeatKey(Heat: policy.Heat, Solver: policy.Solver),
            compute: () => ComputeSignedHeatDetailed(space: space, policy: policy, key: key));
    internal static Fin<ClosedSignedHeatSolution> ClosedSolutionOf(MeshSpace space, SdfMeshPolicy policy, Op key) =>
        policy.Grid.ToFin(key.InvalidInput()).Bind(grid =>
            space.Cache.Memoized(
                probe: new ClosedSignedHeatKey(Grid: grid, Heat: policy.Heat, Solver: policy.Solver,
                    Interpolation: policy.Interpolation, BoundaryCondition: policy.BoundaryCondition),
                compute: () => ComputeClosedSignedHeatDetailed(space: space, policy: policy, key: key)));

    // ROW 1 — P1 tet FEM: (M + tK) heat, per-cell unit gradients, -V*(grad(phi_i) . g) divergence, PinConstant Poisson,
    // boundary-mean shift + interior-mean sign calibration.
    internal static Fin<(Arr<double> Values, TetSignedHeatReceipt Receipt)> SolveTetSignedHeat(TetMeshDomain domain, TetSignedHeatPolicy policy, Op key);
    internal static Fin<TetSignedHeatSample> SampleTetSignedHeat(TetMeshDomain domain, TetSignedHeatPolicy policy, Arr<double> values, Point3d sample, Context context, Op key);

    // ROW 2 — boundary-source CR surface: naked-edge polylines encoded as signed CR edge sources (Lo->Hi sign, closest-
    // vertex snap with rejection witnesses); heat through the cached CR factor; face field -> intrinsic cotangent
    // divergence -> cached (L + SpdMassShift*M) regularized Poisson -> source-mean shift. REJECTS flipped intrinsic (Unsupported).
    internal static Fin<SignedHeatSolution> ComputeSignedHeatDetailed(MeshSpace space, SdfMeshPolicy policy, Op key) {
        double h = space.Cache.MeanEdgeLength;
        if (h <= EpsilonPolicy.ZeroTolerance) return Fin.Fail<SignedHeatSolution>(key.InvalidResult());
        double t = policy.Heat.Resolve(cellSize: 0.5 * h);
        return from imesh in space.Cache.IntrinsicMeshSnapshot(key: key)
               from _ in guard(!imesh.HasFlips, key.Unsupported(geometryType: typeof(MeshKernel.IntrinsicMesh), outputType: typeof(SignedHeatSolution)))
               from admitted in AdmitBoundarySignedHeat(space: space, imesh: imesh, key: key)
               from heatFactor in space.Cache.EdgeConnectionCholeskyDetailed(time: t, key: key)
               from heatSolve in heatFactor.Factor.SolveDetailed(rhs: admitted.Source.Rhs, key: key)
               let faceField = DecAssembly.SampleCrouzeixRaviartFaceField(mesh: space.Native, imesh: imesh, stacked: heatSolve.Solution)
               let divergence = DecAssembly.ComputeIntrinsicVertexDivergence(mesh: space.Native, imesh: imesh, faceFields: faceField)
               from poissonFactor in space.Cache.Cholesky(key: key)
               from poissonSolve in poissonFactor.SolveDetailed(rhs: divergence, key: key)
               from residuals in heatSolve.Residual <= policy.Solver.ResidualTolerance.Value && poissonSolve.Residual <= policy.Solver.ResidualTolerance.Value
                   ? Fin.Succ(unit) : Fin.Fail<Unit>(key.InvalidResult())
               from shifted in ShiftSignedHeat(phi: poissonSolve.Solution, sourceVertices: admitted.Source.SourceVertices, vertexCount: space.Native.Vertices.Count, key: key)
               select new SignedHeatSolution(
                   Values: shifted,
                   Receipt: new SignedHeatReceipt(
                       BoundarySourceVertexCount: admitted.Source.SourceVertices.Count,
                       BoundaryEncodedEdgeSourceCount: admitted.Source.EncodedEdgeSourceCount,
                       BoundaryRejectedPointCount: admitted.Source.RejectedBoundaryPointCount,
                       BoundaryUnmatchedSegmentCount: admitted.Source.UnmatchedBoundarySegmentCount,
                       HeatSolve: Some(heatSolve), PoissonSolve: poissonSolve,
                       EdgeAssembly: Some(heatFactor.Receipt), SpdMassShift: Some(space.Cache.SpdMassShift)),
                   Topology: admitted.Topology);
    }
    private static Fin<(TopologyReceipt Topology, BoundarySignedHeatSource Source)> AdmitBoundarySignedHeat(MeshSpace space, MeshKernel.IntrinsicMesh imesh, Op key);
    private static Fin<Arr<double>> ShiftSignedHeat(Arr<double> phi, Seq<int> sourceVertices, int vertexCount, Op key);

    // ROW 3 — closed-surface regular grid: watertight-solid-closed-oriented gate; padded grid under the MaxNodes
    // policy ceiling; softened heat kernel Area*e^(-r/sqrt(t))/r per node over all source triangles (the hot triple
    // loop is the named kernel); 7-pt FD Poisson under MeanZeroConstant(MinZero); source-mean + interior-sign calibrate.
    internal static Fin<ClosedSignedHeatSolution> ComputeClosedSignedHeatDetailed(MeshSpace space, SdfMeshPolicy policy, Op key);
    private static Fin<VolumeGridReceipt> VolumeGridOf(BoundingBox source, VolumeGridPolicy grid, Op key);   // grid.MaxNodes passes as the CellLattice.Of ceiling — the one budget gate
    internal static Fin<double> InterpolateVolumeGrid(VolumeGridReceipt grid, Arr<double> values, Point3d sample, Op key);
    internal readonly record struct BoundarySignedHeatSource(Arr<double> Rhs, Seq<int> SourceVertices, int EncodedEdgeSourceCount, int RejectedBoundaryPointCount, int UnmatchedBoundarySegmentCount);
}

// Mesh-SDF dispatch: the SdfMeshMethod row's generated Switch is exhaustive — no when-Equals chain, no dead
// fallback arm; a fourth method row fails to compile until it names its arm. Prewarm factors and caches without sampling.
public static class MeshSdf {
    public static Fin<SdfMeshSample> SignedDistanceDetailed(MeshSpace space, SdfMeshPolicy policy, Point3d sample, Op? key = null) =>
        key.OrDefault() switch {
            Op op => policy.Admit(key: op).Bind(active => active.Method.Switch(
                generalizedWindingNumber: () =>
                    from distance in GeneralizedWindingDistance(space: space, policy: active, sample: sample, key: op)
                    from receipt in ReceiptOf(space: space, policy: active, signedHeat: Option<SignedHeatReceipt>.None)
                    select new SdfMeshSample(Distance: active.SignConvention.Multiplier * distance, Receipt: receipt),
                boundarySignedHeat: () =>
                    from solution in SignedHeatSpine.BoundarySolutionOf(space: space, policy: active, key: op)
                    from signed in InterpolateOnMesh(space: space, sample: sample, perVertex: solution.Values, key: op)
                    from receipt in ReceiptOf(space: space, policy: active, signedHeat: Some(solution.Receipt), topology: Some(solution.Topology))
                    select new SdfMeshSample(Distance: active.SignConvention.Multiplier * signed, Receipt: receipt),
                closedSurfaceSignedHeat: () =>
                    from solution in SignedHeatSpine.ClosedSolutionOf(space: space, policy: active, key: op)
                    from signed in SignedHeatSpine.InterpolateVolumeGrid(grid: solution.Grid, values: solution.Values, sample: sample, key: op)
                    from receipt in ReceiptOf(space: space, policy: active, signedHeat: Some(solution.Receipt), topology: Some(solution.Topology), volumeGrid: Some(solution.Grid))
                    select new SdfMeshSample(Distance: active.SignConvention.Multiplier * signed, Receipt: receipt))),
        };
    public static Fin<SdfMeshReceipt> Prewarm(MeshSpace space, SdfMeshPolicy policy, Op? key = null);
    private static Fin<SdfMeshReceipt> ReceiptOf(MeshSpace space, SdfMeshPolicy policy, Option<SignedHeatReceipt> signedHeat, Option<TopologyReceipt> topology = default, Option<VolumeGridReceipt> volumeGrid = default);
    // GWN rides Spatial/index's accelerated Winding query, the ONE distance-field lane: TriangleSoup's per-triangle
    // boxes build ONE Bvh per MeshSpace (Spatial.Apply over SpatialOp.Build(SpatialKind.Bvh, boxes, BuildPolicy.Canonical),
    // memoized under WindingIndexKey), then every probe batch is ONE Spatial.Apply over SpatialOp.Query with
    // SpatialQuery.Winding(samples, triangles, policy.WindingBetaSquared), read SpatialAnswer.Result → QueryResult.Field.
    // |w| > 0.5 -> inside -> negative native-ClosestPoint distance; SpatialIndex owns the solid-angle descent, so a local walker is deleted.
    internal readonly record struct WindingIndexKey;
    internal static Fin<double[]> WindingFieldOf(MeshSpace space, Point3d[] samples, SdfMeshPolicy policy, Op key);
    private static Fin<double> GeneralizedWindingDistance(MeshSpace space, SdfMeshPolicy policy, Point3d sample, Op key);   // singleton batch over WindingFieldOf
    // Kernels.QuadDiagonal splits every quad; Boxes[i] bounds half-open Triangles[3i..3i+3).
    private static (BoundingBox[] Boxes, Point3d[] Triangles) TriangleSoup(MeshSpace space);
    private static Fin<double> InterpolateOnMesh(MeshSpace space, Point3d sample, Arr<double> perVertex, Op key);   // barycentric at ClosestMeshPoint
}

// Native marching-cubes: prewarm mesh-backed fields, count evaluator failures via Interlocked, and the Op.Catch funnel converts the one native callback boundary.
public static class IsoSurface {
    // RhinoCommon-owned evaluator internals — RECORDED as witnesses on the receipt, never chosen here.
    private const double NativeFixedTolerance = 0.001;
    private const double NativeFixedNormalSampleDistance = 1.0e-5;
    // CellLattice IS the sweep request: an admitted CellLattice expresses a rotated, sheared, or per-axis-anisotropic
    // sweep no (bounds, int resolution) pair can spell, and its Of ceiling replaced the local cell-census gate. The
    // native evaluator samples INDEX space at the max-axis census (its one-resolution limitation), the evaluator maps
    // each native point through IndexToWorld, and the emitted mesh transforms back to world — the receipt's lattice
    // witnesses the true request either way.
    public static Fin<IsoSurfaceResult> Detailed(ScalarField field, CellLattice grid, IsoSurfacePolicy policy, Context context, Op? key = null) {
        Op op = key.OrDefault();
        return PreflightOf(field: field, context: context, key: op)
            .Bind(preflight => op.Catch(() => {
                int failures = 0;
                int resolution = Math.Max(val1: grid.Columns.Value, val2: Math.Max(val1: grid.Rows.Value, val2: grid.Layers.Value));
                BoundingBox indexBox = new(min: Point3d.Origin, max: new Point3d(x: grid.Columns.Value, y: grid.Rows.Value, z: grid.Layers.Value));
                // Increment only — assigning the returned count back would race the parallel callback (lost update).
                double EvaluateIso(Point3d point) =>
                    field.SampleScalar(sample: grid.IndexToWorld * point, context: context, key: op).Match(
                        Succ: static value => value,
                        Fail: _ => { _ = Interlocked.Increment(location: ref failures); return double.NaN; });
                Mesh? result = Mesh.CreateFromIsosurface(scalarFieldEvaluator: EvaluateIso, box: indexBox, resolution: resolution, RootFindingMaxSteps: policy.MaxRootSteps.Value);
                _ = result?.Transform(xform: grid.IndexToWorld);
                Option<int> nakedBoundaryLoops = result switch {
                    { IsValid: true, IsClosed: true } => Some(0),
                    { IsValid: true } mesh => Optional(mesh.GetNakedEdges()).Map(static loops => loops.Length),
                    _ => Option<int>.None,
                };
                IsoSurfaceStatus status = (failures, result) switch {
                    ( > 0, _) => IsoSurfaceStatus.EvaluatorFailure,
                    (_, null) => IsoSurfaceStatus.NativeReturnedNull,
                    (_, { IsValid: true }) when nakedBoundaryLoops.IsSome => IsoSurfaceStatus.NativeValid,
                    _ => IsoSurfaceStatus.NativeInvalidMesh,
                };
                return Fin.Succ(new IsoSurfaceResult(
                    Mesh: result ?? new Mesh(),
                    Receipt: new IsoSurfaceReceipt(NativeRouted: true, Status: status, Grid: grid,
                        HexCellCount: grid.CellCount, CornerSampleCount: grid.NodeCount,
                        CenterSampleCount: grid.CellCount, InitialSampleCount: grid.NodeCount + grid.CellCount,
                        MaxRootSteps: policy.MaxRootSteps.Value,
                        ParallelCallback: true, EvaluatorFailures: failures,
                        VertexCount: result?.Vertices.Count ?? 0, FaceCount: result?.Faces.Count ?? 0,
                        NakedBoundaryLoopCount: nakedBoundaryLoops,
                        FixedTolerance: Some(NativeFixedTolerance), FixedNormalSampleDistance: Some(NativeFixedNormalSampleDistance), MeshPreflight: preflight)));
            }));
    }
    // Every classified native outcome RETURNS its receipt — a terminal Valid gate here would strip the failure
    // evidence the extract rail inspects; admission failures alone fail the rail, consumers gate on Receipt.Valid.
    private static Fin<Option<SdfMeshReceipt>> PreflightOf(ScalarField field, Context context, Op key);                  // MeshSdf.Prewarm for mesh-backed fields
}

// MANAGED 2D iso extraction — the IsoSurface analogue one rank down, gated grid.Rank is 2: the managed correctness
// rail the native adapter lacks, restoring the arrangement.md BooleanRoute pattern for the iso family. Per cell the
// four corner samples classify the 16-entry case table, the saddle ambiguity resolves by the exact bilinear-centre
// sign (never a fixed convention), each crossing interpolates linearly along its edge, and segments chain into
// oriented loops through the SAME Meshing/intersect Chain carrier PlanarOverlay emits — outer CCW, holes CW.
public static class IsoContour {
    public static Fin<IsoContourResult> Detailed(ScalarField field, CellLattice grid, IsoContourPolicy policy, Context context, Op? key = null) {
        Op op = key.OrDefault();
        return from _ in guard(grid.Rank is 2, op.InvalidInput()).ToFin()
               from corners in CornerFieldOf(field: field, grid: grid, context: context, key: op)
               from cells in op.AcceptValue(value: ClassifyCells(corners: corners, grid: grid, isovalue: policy.IsoValue))
               from chains in ChainSegments(cells: cells, grid: grid, context: context, key: op)
               select new IsoContourResult(
                   Loops: chains.Loops,
                   Receipt: new IsoContourReceipt(Grid: grid, IsoValue: policy.IsoValue,
                       CellCount: grid.CellCount, AmbiguousCellCount: cells.AmbiguousCount, OpenRunCount: chains.OpenRuns));
    }
    // Corner samples at lattice NODES — (Columns+1)x(Rows+1) taps through the one SampleScalar rail, first failure
    // carrying its node coordinate; the census rode the lattice admission, so no second budget gate exists here.
    private static Fin<double[]> CornerFieldOf(ScalarField field, CellLattice grid, Context context, Op key);
    // Marching-squares classification: per cell the 4-bit corner sign word indexes the 16-entry segment table; words
    // 5 and 10 resolve by the exact bilinear-centre sign, counted on AmbiguousCount.
    private static (Seq<(Point3d A, Point3d B)> Segments, int AmbiguousCount) ClassifyCells(double[] corners, CellLattice grid, double isovalue);
    // Segment chaining into oriented Chain loops: shared-endpoint stitch under context.Absolute, outer CCW holes CW
    // by signed-area orientation; a run that cannot close (field crosses the lattice border) counts as an open run
    // and emits Closed: false rather than a fabricated closure.
    private static Fin<(Seq<Chain> Loops, int OpenRuns)> ChainSegments((Seq<(Point3d A, Point3d B)> Segments, int AmbiguousCount) cells, CellLattice grid, Context context, Op key);
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct IsoContourPolicy(double IsoValue) {
    public static readonly IsoContourPolicy Default = new(IsoValue: 0.0);
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct IsoContourReceipt(
    CellLattice Grid, double IsoValue, long CellCount, int AmbiguousCellCount, int OpenRunCount) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Finite(IsoValue), ValidityClaim.CountAtLeast(count: AmbiguousCellCount, floor: 0),
        ValidityClaim.CountAtLeast(count: OpenRunCount, floor: 0));
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct IsoContourResult(Seq<Chain> Loops, IsoContourReceipt Receipt);
```

```mermaid
---
config:
  layout: elk
  flowchart:
    curve: linear
    padding: 25
---
flowchart LR
    accTitle: Reconstruction dispatch
    accDescr: Reconstruction policies produce scalar fields, signed heat shares one spine, and native extraction returns typed evidence.
    MlsSample -->|one entry, policy-case dispatch| Reconstruct
    Reconstruct -->|Rbf / Mls / Levin / Apss| ScalarField
    Reconstruct -->|splat + 7pt Laplacian + gauge solve| PoissonGrid
    SignedHeatDiscretization -->|heat -> unit-gradient divergence -> Poisson -> calibrate| SignedHeatSpine
    SignedHeatSpine -->|TetFem row| TetSignedHeatReceipt
    SignedHeatSpine -->|BoundarySource row via dec CR + mesh cache| SignedHeatSolution
    SignedHeatSpine -->|ClosedVolumeGrid row| VolumeGridReceipt
    SdfMeshMethod -->|GWN / boundary / closed dispatch| MeshSdf
    MeshSdf -->|distance + receipt| SdfMeshSample
    ScalarField -->|native marching cubes| IsoSurface
    IsoSurface -->|evaluator-failure witness| IsoSurfaceReceipt
    ScalarField -->|managed rank-2 marching squares| IsoContour
    IsoContour -->|oriented Chain loops| IsoContourReceipt
    Reconstruct -.->|degenerate / residual breach| Op
```

## [03]-[DENSITY_BAR]

Each `[RAIL]` cell names the one return rail its owner exposes; the per-axis collapse kind rides the indexed notes below.

| [INDEX] | [AXIS_CONCERN]    | [OWNER]                                               | [RAIL]                                        | [CASES] |
| :-----: | :---------------- | :---------------------------------------------------- | :-------------------------------------------- | :-----: |
|  [01]   | Construction      | `ReconstructionPolicy` → `Reconstruction.Reconstruct` | `Reconstruct → Fin<ReconstructionResult>`     |    5    |
|  [02]   | Mode vocabulary   | `ReconstructionMode`                                  | discriminant                                  |    7    |
|  [03]   | Signed-heat spine | `SignedHeatDiscretization` → `SignedHeatSpine.Solve`  | `Solve → Fin<SignedHeatOutcome>`              |    3    |
|  [04]   | Mesh SDF          | `SdfMeshPolicy` → `MeshSdf`                           | `SignedDistanceDetailed → Fin<SdfMeshSample>` |    3    |
|  [05]   | Tet domain        | `TetMeshDomain`                                       | `Of → Fin<TetMeshDomain>`                     |    1    |
|  [06]   | Volume grid       | `VolumeGridPolicy` / `VolumeGridReceipt`              | `VolumeGridOf → Fin<VolumeGridReceipt>`       |    —    |
|  [07]   | Iso extraction    | `IsoSurface`                                          | `Detailed → Fin<IsoSurfaceResult>`            |    4    |
|  [08]   | Iso contouring    | `IsoContour`                                          | `Detailed → Fin<IsoContourResult>`            |    —    |
|  [09]   | Policy family     | `SignedHeatTime` … `PoissonPolicy`                    | `Of → Fin<policy>` per record                 |    —    |

- [01]-[CONSTRUCTION]: `[Union]` policy discriminant, one entry, per-case admission.
- [02]-[MODE_VOCABULARY]: `[SmartEnum<int>]` with normals/sparse/degree/status columns.
- [03]-[SIGNED_HEAT_SPINE]: `[Union]` rows over ONE four-stage law.
- [04]-[MESH_SDF]: method-row dispatch, cache-backed solves.
- [05]-[TET_DOMAIN]: validated domain, boundary topology derived at admission.
- [06]-[VOLUME_GRID]: resolution-xor-cellsize + ceiling policy rows.
- [07]-[ISO_EXTRACTION]: native marching-cubes adapter, failure-classified receipt.
- [08]-[ISO_CONTOURING]: managed rank-2 marching squares, exact saddle resolution, `Chain` egress.
- [09]-[POLICY_FAMILY]: validated policy records with presets over the one `VolumeSolverPolicy` solve gate.

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

- [SIGNED_HEAT_SPINE]-[OPEN]: Which transcription-complete row bodies close the P1 tet FEM, boundary-source CR, and closed-volume-grid stages under one heat-to-Poisson law?; verify time derivation, source proximity, sign calibration, residual gates, and every solve receipt.
- [MESH_SDF_AND_ISO]-[OPEN]: Which transcription-complete mesh-SDF helper bodies close cached winding, signed-heat projection, and native isosurface preflight without admitting approximate predicates?; verify evaluator-failure counting and receipt invalidation against `Mesh.CreateFromIsosurface` and `Interlocked`.
