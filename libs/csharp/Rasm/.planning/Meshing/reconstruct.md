# [RASM_RECONSTRUCTION_RECONSTRUCT]

The implicit-reconstruction owner (points/mesh → field → mesh): ONE `Reconstruction.Reconstruct` entry discriminating on a closed `ReconstructionPolicy` union builds the five oriented-sample reconstruction families — RBF interpolation/approximation (kernel-matrix solve, smoothing-augmented least squares), moving least squares (4-equation-per-sample normal-constrained LSQ with SVD rank/condition witnesses), Levin two-step MLS (covariance plane seed, Brent energy-minimizing offset/normal alternation, ridge-regularized polynomial height fit), APSS (Pratt-normalized algebraic sphere fit with Taubin projection), and screened-Poisson indicator on a dense regular lattice (trilinear normal splat, 7-point Laplacian, Dirichlet/screening terms) — each producing a `Spatial/fields` `ScalarField` case plus a typed receipt. The page also owns THE unified signed-heat spine: one four-stage law (heat solve → unit field → divergence → gauge-fixed Poisson → sign calibration) parameterized by a closed `SignedHeatDiscretization` union — P1 tet FEM over a `TetMeshDomain`, boundary-source Crouzeix-Raviart over a mesh surface, closed-surface regular volume grid — reuniting the `SdfMeshPolicy`/`SignedHeatTime`/`VolumeGridPolicy`/`VolumeSolverPolicy` policy vocabulary with its kernels and receipts (the mature policy-in-`Field.cs`/kernel-in-`Mesh.cs` fracture is dead, and the tet spine is a row of the same law, not a parallel implementation). The three mesh-SDF methods (generalized-winding solid angle, boundary signed heat, closed volumetric signed heat) dispatch on the `SdfMeshMethod` row, and the native marching-cubes `IsoSurface` extraction closes the loop back to a mesh with evaluator-failure receipts.

The 58-factory construction spam is dead: `ReconstructionPolicy` is the one construction discriminant (a policy case IS the mode selection — `RbfDetailed`/`MlsDetailed`/`LevinMlsDetailed`/`ApssDetailed`/`PoissonDetailed` sibling factories never re-mint), and `MlsSample` is the one sample carrier (RBF reads `(Position, Value)` and ignores normals per its `ReconstructionMode.RequiresNormals` column). The ~6 vestigial octree-era `PoissonPolicy` knobs (`FullDepth`/`CgDepth`/`KernelDepth`/`Confidence`/`ConfidenceBias`/`LinearFit`/`PrimalGrid`) are DROPPED — dead parameterization the dense-lattice implementation never read; the rebuilt policy parameterizes the real lattice only. Volume-grid ceilings are policy rows (`VolumeGridPolicy.MaxNodes`, `KernelSofteningRatio`; `IsoSurfacePolicy.MaxCells`), never consts. Weight-kernel profile math (`KernelKind`, `WeightKernelFamily`, `KernelProfile`) arrives settled from `Numerics/calculus`; linear algebra routes the `Numerics/matrix` owners exclusively; the `Spatial/fields` `ScalarField` case names (`Rbf`/`Mls`/`LevinMls`/`Apss`/`Poisson`/`TetSignedHeat`/`SignedDistanceFromMesh`) are frozen and delegate their sampling to the kernels here. Receipts ride the `Domain/rails` `[ValidityEvidence]` fold; `Op` stays the explicit value key; failures route `Op` fault factories over `Fin<T>`.

## [01]-[INDEX]

- [02]-[RECONSTRUCTION]: `ReconstructionPolicy` union → one `Reconstruct` entry over RBF/MLS/Levin/APSS/Poisson; point evaluators; the `SignedHeatDiscretization` spine (tet FEM / boundary CR / closed volume grid); `SdfMeshMethod` mesh-SDF dispatch; native `IsoSurface` marching cubes; the reunited policy + receipt vocabulary.

## [02]-[RECONSTRUCTION]

- Owner: `ReconstructionPolicy` `[Union]` the ONE construction discriminant (`Rbf`/`Mls`/`Levin`/`Apss`/`Poisson` cases, each carrying its typed policy payload; the `ReconstructionMode` row derives per case); `Reconstruction` the build/evaluate kernel; `SignedHeatDiscretization` `[Union]` the spine discriminant (`TetFem`/`BoundarySource`/`ClosedVolumeGrid`); `SignedHeatSpine` the one four-stage signed-heat law; `MeshSdf` the mesh-SDF dispatch (`SignedDistanceDetailed`/`Prewarm` over `SdfMeshMethod`); `IsoSurface` the native extraction adapter; `TetMeshDomain` the validated tetrahedral domain (full boundary-face topology derivation at admission); the policy family (`SdfMeshPolicy`, `TetSignedHeatPolicy`, `SignedHeatTime`, `VolumeGridPolicy`, `VolumeSolverPolicy`, `LevinMlsPolicy`, `ApssPolicy`, `PoissonPolicy`, `IsoSurfacePolicy`) — every knob a validated policy row with a preset, per the `TuftedCoverPolicy.Default` pattern.
- Cases: `ReconstructionPolicy` cases `Rbf` · `Mls` · `Levin` · `Apss` · `Poisson` (5); `ReconstructionMode` rows `RbfInterpolation` · `RbfApproximation` · `MovingLeastSquares` · `LevinMovingLeastSquares` · `AlgebraicPointSetSurfaces` · `Poisson` · `ScreenedPoisson` (7, `RequiresNormals`/`RequiresSparseSystem`/`PolynomialDegree` columns); `SignedHeatDiscretization` cases `TetFem` · `BoundarySource` · `ClosedVolumeGrid` (3); `SdfMeshMethod` rows `GeneralizedWindingNumber` · `BoundarySignedHeat` · `ClosedSurfaceSignedHeat` (3, `Status`/`Domain` columns); `SdfSignConvention` rows (2, `Multiplier` column); `PoissonBoundary` rows `Free` · `Dirichlet` · `Neumann` (3, `Singular`/`ExteriorValue`/`IsDirichlet` columns); `ReconstructionStatus` (3); `IsoSurfaceStatus` (4); `TetGaugePolicy` `PinnedFirstBoundary` (1); `TetInterpolation` `Barycentric` (1); `VolumeSolverKind` `SparseCholeskyPinned` (1); `VolumeBoundaryCondition` `NeumannGaugePinned` (1); `VolumeInterpolation` `Trilinear` (1).
- Entry: `public static Fin<ReconstructionResult> Reconstruct(Seq<MlsSample> samples, ReconstructionPolicy policy, Context context, Op? key = null)` — the one reconstruction entry; the policy case selects the build kernel, admission is internalized per case (finite positions/normals/values, mode-specific guards), and the result carries the frozen `Spatial/fields` case plus `ReconstructionReceipt`. `public static Fin<SignedHeatOutcome> Solve(SignedHeatDiscretization discretization, Op? key = null)` on `SignedHeatSpine` — the one signed-heat entry; the case carries its domain and policy, and the union `Switch` routes to the case's row kernel, each realizing the SAME four-stage law over its discretization. `MeshSdf.SignedDistanceDetailed(MeshSpace space, SdfMeshPolicy policy, Point3d sample, Op key)` — the mesh-SDF sample entry dispatching on `policy.Method`; `MeshSdf.Prewarm(MeshSpace space, SdfMeshPolicy policy, Op key)` factors and caches the solves without sampling. `IsoSurface.Detailed(ScalarField field, BoundingBox bounds, int resolution, IsoSurfacePolicy policy, Context context, Op key)` — the extraction entry, returning the classified receipt for EVERY native outcome: admission failures alone fail the rail, consumers gate on `Receipt.Valid` (the mature gated/attempt entry PAIR collapses to this one receipt-honest entry with zero capability loss — the settled extract rail already re-gates). No per-mode public factory siblings anywhere on the surface.
- Auto: RBF selects interpolation vs approximation by the smoothing row (`≤ ZeroTolerance` → exact kernel-matrix solve; `> 0` → `√smoothing`-diagonal-augmented least squares) — the mode split is a value consequence, not a knob. MLS solves the 4-equation-per-neighbor design (`[1, −offset] · [value; gradient]` rows weighted by `√profile`) and gates on rank ≥ 4 plus normal agreement ≥ 0.5 against the weighted normal. Levin runs step one as covariance plane seed (smallest eigenvector, orientation-corrected) then alternates Brent root-finding on the weighted energy derivative along the normal (bracket/accuracy scale-derived from support) with normal re-estimation until `StepEps`, gated by the planarity ratio `λ0/λ2 ≤ PlanarityTau`; step two fits the ridge-regularized degree-`PolyDegree` height polynomial in the local tangent frame. APSS fits the algebraic sphere `(hc, hl, hq)` by Pratt normalization, classifies the plane-degenerate branch by `DegeneracyRatio ≤ EpsDegeneracy`, and projects iteratively with `StepDamping` under `ProjTol`. Poisson splats inward normals trilinearly onto the `2^Depth` lattice (density-normalized), assembles the 7-point Laplacian with one-sided boundary differences, adds `α = 8^Depth · PointWeight` screening outer products per sample when screened, imposes Dirichlet rows when `Boundary.IsDirichlet`, solves definite systems by `CholeskySparse` and singular ones by `SingularSolveDetailed` under `GaugePolicy.PinConstant(interior, GaugeShift.PinZero)`, and derives the isovalue `γ` as the density-weighted mean sample indicator. The signed-heat spine stages per row: TetFem assembles P1 mass/stiffness (per-cell Jacobian-inverse gradients), heat RHS from boundary indicators, solves `(M + tK)` by Cholesky, normalizes per-cell gradients, scatters `−V·(∇φᵢ · ĝ)` divergence, Poisson-solves under `PinConstant(firstBoundary, PinZero)` with lumped mass, and calibrates by boundary-mean shift + interior-mean sign; BoundarySource encodes naked-edge polylines as signed CR edge sources (`Lo→Hi` sign convention, closest-vertex snap with rejection witnesses), heat-solves through the cached CR factor (`Meshing/dec` system via the `Meshing/mesh` memo), samples the CR face field, scatters intrinsic cotangent divergence, Poisson-solves through the cached `(M + SpdMassShift·L)` factor, and shifts by source mean; ClosedVolumeGrid admits watertight-solid-closed-oriented topology only, builds the padded grid under the `MaxNodes` ceiling row, integrates the softened heat kernel `Area·e^{−r/√t}/r` per node over all source triangles (softening `ε = h²·KernelSofteningRatio`), assembles the 7-point FD Poisson, solves under `MeanZeroConstant(GaugeShift.MinZero)`, and calibrates by source-mean shift with interior-point sign flip. Heat time resolves per row from `SignedHeatTime` (`Explicit` or `Coefficient · cellSize²` — cell size is half the mean edge for the surface row, the FD cell for the grid row, the cube-root mean cell volume for the tet row). All solve residuals gate against `VolumeSolverPolicy.ResidualTolerance`. `IsoSurface.Detailed` prewarms mesh-backed fields, evaluates through the native `Mesh.CreateFromIsosurface` parallel callback counting evaluator failures via `Interlocked`, and classifies `IsoSurfaceStatus` from (failures, result) — an invalid extraction is a typed receipt, never a null.
- Receipt: `ReconstructionReceipt` (mode/kernel/radius/smoothing/interpolation/counts/degree/solve) on every build; `ReconstructionSample` + `ReconstructionSampleReceipt` (status, neighborhood/rejection counts, weight sum, rank, condition, normal agreement, gradient norm, solve) on every point evaluation; `LevinMlsSampleReceipt` (plane frame, offset, step-1 iterations/energy/root residual/second derivative, normal iterations/residual, eigen spread, step-2 residual/RMS/condition/rank, projection witnesses) and `ApssSampleReceipt` (algebraic coefficients, Pratt norm, degeneracy ratio, sphere center/radius/mean curvature, field value/gradient, projection iterations/Taubin residual) on the deep evaluators; `PoissonReceipt` (lattice/system witnesses, splat counts, isovalue ± deviation, chi statistics, gradient/screening energies, data/gradient residuals, unscreened-equivalence law, gauge + solve) with its conservation law `Contribution + Rejected + Clamped == SampleCount` on the fold hook; `SignedHeatReceipt` (boundary-source counts, heat + Poisson solves, edge assembly, `SpdMassShift`) and `VolumeGridReceipt` (grid geometry, source/degenerate counts, inside/outside/near-surface partitions, heat time, gauge node, surface shift, solver policy, operator/factor NNZ, residual) and `TetSignedHeatReceipt` (FEM witnesses + heat/Poisson solves + calibration shifts) per spine row; `SdfMeshReceipt` (method/status/domain + `TopologyReceipt` + optional signed-heat/volume-grid receipts) and `SdfMeshSample` on the mesh-SDF rail; `IsoSurfaceReceipt` (native routing, status, grid, root steps, evaluator failures, vertex/face counts, native-fixed tolerance witnesses, mesh preflight) inside `IsoSurfaceResult`. All ride the `[ValidityEvidence]` fold.
- Packages: `Rasm.Vectors` `Numerics/matrix` (`Matrix`, `SymmetricMatrix`, `SparseMatrix`, `CholeskySparse`, `GaugePolicy`/`GaugeShift`, `SolveReceipt`/`SvdResult`/`EigenSolveReceipt`), `Numerics/calculus` (`KernelKind`, `WeightKernelFamily`, `KernelProfile` — the profile math composed, never re-minted), `Meshing/mesh` (`MeshSpace`, cache memo slots, `TopologyReceipt`, `MeshKernel.TopologyDetailed`, `DegenerateAreaFloorOf`), `Meshing/dec` (`BuildCrouzeixRaviartHeatSystemDetailed`, `SampleCrouzeixRaviartFaceField`, `ComputeIntrinsicVertexDivergence`), `Spatial/fields` (`ScalarField` frozen cases as the build product), `Domain/rails` + `Domain/context`, MathNet.Numerics (`RootFinding.Brent.TryFindRoot` — the Levin energy root), RhinoCommon (`Mesh.CreateFromIsosurface`, `Mesh.IsPointInside`/`ClosestPoint`/`SolidOrientation`/`GetNakedEdges`, `Point3dList.ClosestIndex` — genuinely Rhino-boundary, never thinned), LanguageExt.Core, BCL (`Interlocked`).
- Growth: a new reconstruction family (partition-of-unity implicits, neural pull) is one `ReconstructionPolicy` case + one `ReconstructionMode` row + one build arm producing a new frozen field case; a new signed-heat discretization (polygon FEM, adaptive octree grid) is one `SignedHeatDiscretization` case + one stage row on the same four-stage spine — never a parallel heat→Poisson pipeline; a new mesh-SDF method is one `SdfMeshMethod` row; a new lattice boundary condition is one `PoissonBoundary` row with its column values; a grid ceiling change is a policy-row edit; zero new entry surface.
- Boundary: the spine is ONE law — a discretization implementing its own heat→divergence→Poisson→calibrate sequence outside the `SignedHeatSpine` rows is the re-opened Field↔Mesh fracture this page exists to close. The dropped Poisson octree knobs are a standing decision: re-admitting `FullDepth`/`CgDepth`/`KernelDepth`/`Confidence`/`ConfidenceBias`/`LinearFit`/`PrimalGrid` without an octree implementation that reads them is dead parameterization; an octree upgrade re-parameterizes the lattice policy at that time. The boundary-source row REJECTS flipped intrinsic snapshots (sources are encoded against original-mesh edges; the `Unsupported` fault is the honest verdict until CR signpost transfer lands — recorded growth). The closed-grid row admits ONLY watertight-solid-closed-oriented topology (the `TopologyReceipt` conjunction is the gate; a soup mesh routes `InvalidInput`, never a garbage sign field). `PoissonGrid.SampleTrilinear` returns the positive outside value `max(1, spacing·resolution)` beyond the lattice — a clamp-to-edge would fabricate interior values. Native `CreateFromIsosurface` runs its evaluator callback in parallel — the failure counter is `Interlocked`, and the receipt's `FixedTolerance`/`FixedNormalSampleDistance` witness the native evaluator's fixed internals (RhinoCommon-owned values, recorded not chosen). Every linear solve routes the `Numerics/matrix` owners — a raw MathNet/CSparse reach is the named bypass defect. A thrown exception anywhere on the rail is forbidden; `key.Catch` converts the one native extraction callback boundary.

```csharp contract
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using LanguageExt;
using Rasm.Domain;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Vectors;

// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class ReconstructionMode {
    public static readonly ReconstructionMode RbfInterpolation         = new(key: 0, requiresNormals: false, requiresSparseSystem: false, polynomialDegree: 0);
    public static readonly ReconstructionMode RbfApproximation         = new(key: 1, requiresNormals: false, requiresSparseSystem: false, polynomialDegree: 0);
    public static readonly ReconstructionMode MovingLeastSquares       = new(key: 2, requiresNormals: true, requiresSparseSystem: false, polynomialDegree: 1);
    public static readonly ReconstructionMode LevinMovingLeastSquares  = new(key: 3, requiresNormals: true, requiresSparseSystem: false, polynomialDegree: 2);
    public static readonly ReconstructionMode AlgebraicPointSetSurfaces = new(key: 4, requiresNormals: true, requiresSparseSystem: false, polynomialDegree: 2);
    public static readonly ReconstructionMode Poisson                  = new(key: 5, requiresNormals: true, requiresSparseSystem: true, polynomialDegree: 0);
    public static readonly ReconstructionMode ScreenedPoisson          = new(key: 6, requiresNormals: true, requiresSparseSystem: true, polynomialDegree: 0);
    public bool RequiresNormals { get; }
    public bool RequiresSparseSystem { get; }
    public int PolynomialDegree { get; }
}

[SmartEnum<int>]
public sealed partial class ReconstructionStatus {
    public static readonly ReconstructionStatus ExactInterpolation = new(key: 0);
    public static readonly ReconstructionStatus ApproximateSdf     = new(key: 1);
    public static readonly ReconstructionStatus PoissonIndicator   = new(key: 2);
}

[SmartEnum<int>]
public sealed partial class PoissonBoundary {
    public static readonly PoissonBoundary Free      = new(key: 0, singular: true, exteriorValue: 0.0, isDirichlet: false);
    public static readonly PoissonBoundary Dirichlet = new(key: 1, singular: false, exteriorValue: -0.5, isDirichlet: true);
    public static readonly PoissonBoundary Neumann   = new(key: 2, singular: true, exteriorValue: 0.0, isDirichlet: false);
    public bool Singular { get; }
    public double ExteriorValue { get; }
    public bool IsDirichlet { get; }
}

[SmartEnum<int>]
public sealed partial class SdfMeshMethod {
    public static readonly SdfMeshMethod GeneralizedWindingNumber = new(key: 0, status: SdfMeshStatus.ApproximateSignClosestDistance, domain: SdfMeshDomain.SurfaceMesh);
    public static readonly SdfMeshMethod BoundarySignedHeat      = new(key: 1, status: SdfMeshStatus.BoundarySourceSignedHeat, domain: SdfMeshDomain.BoundarySource);
    public static readonly SdfMeshMethod ClosedSurfaceSignedHeat = new(key: 2, status: SdfMeshStatus.ClosedSurfaceSignedHeat, domain: SdfMeshDomain.VolumeGrid);
    public SdfMeshStatus Status { get; }
    public SdfMeshDomain Domain { get; }
}

[SmartEnum<int>]
public sealed partial class SdfMeshStatus {
    public static readonly SdfMeshStatus ApproximateSignClosestDistance = new(key: 0);
    public static readonly SdfMeshStatus BoundarySourceSignedHeat       = new(key: 1);
    public static readonly SdfMeshStatus ClosedSurfaceSignedHeat        = new(key: 2);
}

[SmartEnum<int>]
public sealed partial class SdfMeshDomain {
    public static readonly SdfMeshDomain SurfaceMesh   = new(key: 0);
    public static readonly SdfMeshDomain BoundarySource = new(key: 1);
    public static readonly SdfMeshDomain VolumeGrid    = new(key: 2);
}

[SmartEnum<int>]
public sealed partial class SdfSignConvention {
    public static readonly SdfSignConvention NegativeInsidePositiveOutside = new(key: 0, multiplier: 1.0);
    public static readonly SdfSignConvention PositiveInsideNegativeOutside = new(key: 1, multiplier: -1.0);
    public double Multiplier { get; }
}

[SmartEnum<int>]
public sealed partial class IsoSurfaceStatus {
    public static readonly IsoSurfaceStatus NativeValid       = new(key: 0);
    public static readonly IsoSurfaceStatus EvaluatorFailure  = new(key: 1);
    public static readonly IsoSurfaceStatus NativeReturnedNull = new(key: 2);
    public static readonly IsoSurfaceStatus NativeInvalidMesh  = new(key: 3);
}

[SmartEnum<int>] public sealed partial class TetGaugePolicy { public static readonly TetGaugePolicy PinnedFirstBoundary = new(key: 0); }
[SmartEnum<int>] public sealed partial class TetInterpolation { public static readonly TetInterpolation Barycentric = new(key: 0); }
[SmartEnum<int>] public sealed partial class VolumeSolverKind { public static readonly VolumeSolverKind SparseCholeskyPinned = new(key: 0); }
[SmartEnum<int>] public sealed partial class VolumeBoundaryCondition { public static readonly VolumeBoundaryCondition NeumannGaugePinned = new(key: 0); }
[SmartEnum<int>] public sealed partial class VolumeInterpolation { public static readonly VolumeInterpolation Trilinear = new(key: 0); }

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
        rbfCase:     static c => c.Smoothing <= RhinoMath.ZeroTolerance ? ReconstructionMode.RbfInterpolation : ReconstructionMode.RbfApproximation,
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

// Ceilings are POLICY ROWS: MaxNodes bounds the node lattice, KernelSofteningRatio scales the heat-kernel softening.
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
    SignedHeatTime Heat, VolumeSolverPolicy Solver, VolumeInterpolation Interpolation, VolumeBoundaryCondition BoundaryCondition) {
    public static Fin<SdfMeshPolicy> GeneralizedWinding(SdfSignConvention? signConvention = null, Op? key = null);
    public static Fin<SdfMeshPolicy> BoundarySignedHeat(SignedHeatTime? heat = null, VolumeSolverPolicy? solver = null, SdfSignConvention? signConvention = null, Op? key = null);
    public static Fin<SdfMeshPolicy> ClosedSignedHeat(VolumeGridPolicy grid, SignedHeatTime? heat = null, VolumeSolverPolicy? solver = null, SdfSignConvention? signConvention = null, Op? key = null);
    internal Fin<SdfMeshPolicy> Admit(Op key);       // grid present IFF ClosedSurfaceSignedHeat; heat/solver validity
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct TetSignedHeatPolicy(
    SignedHeatTime Heat, VolumeSolverPolicy Solver, SdfSignConvention SignConvention, TetGaugePolicy Gauge, TetInterpolation Interpolation) {
    public static Fin<TetSignedHeatPolicy> Of(SignedHeatTime? heat = null, VolumeSolverPolicy? solver = null,
        SdfSignConvention? signConvention = null, TetGaugePolicy? gauge = null, TetInterpolation? interpolation = null, Op? key = null);
    internal Fin<TetSignedHeatPolicy> Admit(Op key);
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct LevinMlsPolicy(
    PositiveMagnitude Support, int PolyDegree, double NeglectEps, int MinNeighbors, double BracketFactor,
    int MaxOuterIter, double StepEps, double RootTol, int CgMaxIter, double CgTol, double PlanarityTau,
    double RidgeLambda, double NormalTau, double ProjEps, bool PlaneThroughPoint, bool OrientNormals, WeightKernelFamily WeightKernel) {
    public static Fin<LevinMlsPolicy> Of(double support, int polyDegree = 2, double neglectEps = 1e-3, int minNeighbors = 6,
        double bracketFactor = 2.0, int maxOuterIter = 16, double stepEps = 1e-4, double rootTol = 1e-6, int cgMaxIter = 32,
        double cgTol = 1e-6, double planarityTau = 0.25, double ridgeLambda = 0.0, double normalTau = 0.3, double projEps = 1e-4,
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

// REBUILT for the dense regular lattice. The octree-era knobs (FullDepth/CgDepth/KernelDepth/Confidence/ConfidenceBias/
// LinearFit/PrimalGrid) are DROPPED — dead parameterization the lattice implementation never read.
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct PoissonPolicy(
    Dimension Depth, PositiveMagnitude Width, PositiveMagnitude Scale, PositiveMagnitude SamplesPerNode,
    double PointWeight, int Degree, PoissonBoundary Boundary, Dimension Iters, PositiveMagnitude SolverTolerance, PositiveMagnitude Density) {
    public static Fin<PoissonPolicy> Of(int depth = 6, double width = 1.0, double scale = 1.1, double samplesPerNode = 1.5,
        double pointWeight = 0.0, int degree = 2, PoissonBoundary? boundary = null, int iters = 8,
        double solverTolerance = 0.0, double density = 0.0, Op? key = null);
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct IsoSurfacePolicy(Dimension MaxRootSteps, long MaxCells) {
    public static readonly IsoSurfacePolicy Default = new(MaxRootSteps: Dimension.Create(value: 32), MaxCells: 16_777_216L);
}

// --- [MODELS] -------------------------------------------------------------------------------
[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct MlsSample(Point3d Position, Vector3d Normal, double Value);

[ValidityEvidence, BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly partial record struct ReconstructionReceipt(
    ReconstructionMode Mode, KernelKind Kernel, double Radius, double Smoothing, bool Interpolation,
    int SampleCount, int CenterCount, int PolynomialDegree, Option<SolveReceipt> Solve) : IValidityEvidence;

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

[ValidityEvidence, BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly partial record struct ReconstructionSampleReceipt(
    ReconstructionMode Mode, ReconstructionStatus Status, KernelKind Kernel, double Radius, int SampleCount,
    int NeighborhoodCount, int RejectedWeightCount, double WeightSum, int Rank,
    Option<double> Condition, Option<double> NormalAgreement, Option<double> GradientNorm, Option<SolveReceipt> Solve) : IValidityEvidence;

[ValidityEvidence, BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly partial record struct LevinMlsSampleReceipt(
    Point3d PlaneOrigin, Vector3d PlaneNormal, Vector3d MlsNormal, double Offset, Vector3d FrameU, Vector3d FrameV,
    int Step1Iterations, bool Step1Converged, int RootIterations, double RootResidual, double SecondDerivative,
    int NormalIterations, double NormalResidual, double Lambda0, double Lambda2, double Planarity,
    int NeighborCount, double WeightSum, double Step1Energy, int PolyDegree, int CoefficientCount,
    double Step2Residual, double Step2Rms, double DesignCondition, int Rank, double GradientMagnitude,
    double NormalAgreement, double ProjDisplacement, double ProjResidual, bool ProjConverged, bool PlaneThroughPoint, SolveReceipt Solve) : IValidityEvidence {
    private bool ValidityGate() => NeighborCount >= 1 && WeightSum > 0.0 && PolyDegree >= 1 && CoefficientCount >= 1 && Rank >= 1 && Solve.IsValid;
}

[ValidityEvidence, BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly partial record struct ApssSampleReceipt(
    double Hc, Vector3d Hl, double Hq, double PrattNormSquared, bool IsPlane, double DegeneracyRatio,
    Point3d Center, double Radius, double MeanCurvature, double FieldValue, double GradientNorm, Vector3d Normal,
    int NeighborCount, double WeightSum, int ProjIterations, double TaubinResidual, double ProjDisplacement) : IValidityEvidence {
    private bool ValidityGate() => PrattNormSquared > 0.0 && NeighborCount >= 1 && WeightSum > 0.0;
}

[ValidityEvidence, BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly partial record struct PoissonGrid(Point3d Origin, double Spacing, int Resolution, Arr<double> Chi, Arr<double> Density) : IValidityEvidence {
    internal static Fin<PoissonGrid> Of(Point3d origin, double spacing, int resolution, Arr<double> chi, double[] density, Op key);
    private bool ValidityGate() => Chi.Count == Resolution * Resolution * Resolution && Density.Count == Chi.Count && Resolution >= 2 && Spacing > 0.0;
    // Outside the lattice returns the positive far value max(1, Spacing*Resolution) — clamp-to-edge would fabricate interiors.
    internal double SampleTrilinear(Point3d point);
}

[ValidityEvidence, BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly partial record struct PoissonReceipt(
    ReconstructionMode Mode, Dimension Depth, int GridResolution, int SystemDof, int Degree, PoissonBoundary Boundary,
    double PointWeight, double Scale, int SampleCount, int ContributionCount, int RejectedCount, int ClampedCount,
    double WeightSum, int LaplacianNonZeros, int ScreeningNonZeros, double RhsNorm, double Isovalue, double IsovalueStdDev,
    double MeanAbsChi, double MaxAbsChi, double GradientEnergy, double ScreeningEnergy, Option<double> DataResidual,
    double GradientResidual, bool UnscreenedEquivalence, Option<GaugeReceipt> Gauge, SolveReceipt Solve) : IValidityEvidence {
    private bool ValidityGate() =>
        SystemDof == GridResolution * GridResolution * GridResolution && SystemDof == Solve.Cols.Value
        && Degree is >= 1 and <= 2 && ContributionCount + RejectedCount + ClampedCount == SampleCount
        && (!UnscreenedEquivalence || (ScreeningNonZeros == 0 && PointWeight <= 0.0 && ScreeningEnergy == 0.0 && DataResidual.IsNone))
        && Solve.IsValid && Gauge.Map(static receipt => receipt.IsValid).IfNone(noneValue: true);
}

[ValidityEvidence, BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly partial record struct SignedHeatReceipt(
    int BoundarySourceVertexCount, int BoundaryEncodedEdgeSourceCount, int BoundaryRejectedPointCount,
    int BoundaryUnmatchedSegmentCount, Option<SolveReceipt> HeatSolve, SolveReceipt PoissonSolve,
    Option<SpectralAssemblyReceipt> EdgeAssembly = default, Option<double> SpdMassShift = default) : IValidityEvidence;

[ValidityEvidence, BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly partial record struct VolumeGridReceipt(
    BoundingBox Bounds, int Resolution, int XNodes, int YNodes, int ZNodes, double CellSize, double Padding,
    int NodeCount, int CellCount, int SourceTriangleCount, int DegenerateTriangleCount, double SourceArea,
    int InsideNodeCount, int OutsideNodeCount, int NearSurfaceNodeCount, int RejectedVectorCount, double HeatTime,
    int GaugeNode, double SurfaceShift, VolumeInterpolation Interpolation, VolumeBoundaryCondition BoundaryCondition,
    VolumeSolverPolicy Solver, int OperatorNonZeros, Option<int> FactorNonZeros, double Residual) : IValidityEvidence {
    private bool ValidityGate() => NodeCount == XNodes * YNodes * ZNodes && InsideNodeCount + OutsideNodeCount <= NodeCount;
}

[ValidityEvidence, BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly partial record struct SdfMeshReceipt(
    SdfMeshMethod Method, SdfMeshStatus Status, SdfMeshDomain Domain, TopologyReceipt Topology,
    Option<SignedHeatReceipt> SignedHeat, Option<VolumeGridReceipt> VolumeGrid = default) : IValidityEvidence {
    private bool ValidityGate() =>
        Method.Status.Equals(Status) && Method.Domain.Equals(Domain)
        && SignedHeat.Map(static receipt => receipt.IsValid).IfNone(noneValue: true)
        && VolumeGrid.Map(static receipt => receipt.IsValid).IfNone(noneValue: true);
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

[ValidityEvidence, BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly partial record struct TetFemReceipt(
    int VertexCount, int CellCount, int BoundaryVertexCount, int BoundaryFaceCount, int InteriorVertexCount,
    int IncidenceCount, double TotalVolume, double MinCellVolume, double MaxCellVolume,
    int MassNonZeros, int StiffnessNonZeros, int HeatOperatorNonZeros, int DivergenceNonZeros, int RejectedGradientCellCount) : IValidityEvidence {
    private bool ValidityGate() => BoundaryVertexCount + InteriorVertexCount == VertexCount && MaxCellVolume >= MinCellVolume;
}

[ValidityEvidence, BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly partial record struct TetInterpolationReceipt(TetInterpolation Interpolation, int CellIndex, Arr<double> Barycentric, bool Inside) : IValidityEvidence {
    private bool ValidityGate() => !Inside || Barycentric.Count == 4;
}

[ValidityEvidence, BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly partial record struct TetSignedHeatReceipt(
    TetFemReceipt Fem, SignedHeatTime Heat, VolumeSolverPolicy Solver, SdfSignConvention SignConvention,
    TetGaugePolicy Gauge, TetInterpolation Interpolation, int GaugeVertex, double HeatTime,
    double BoundaryShift, double InteriorMean, SolveReceipt HeatSolve, SolveReceipt PoissonSolve) : IValidityEvidence {
    private bool ValidityGate() => Fem.IsValid && HeatTime > 0.0 && HeatSolve.IsValid && PoissonSolve.IsValid;
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct TetSignedHeatSample(double Value, TetSignedHeatReceipt Receipt, TetInterpolationReceipt Interpolation);

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct IsoSurfaceGrid(
    BoundingBox Bounds, int Resolution, long XCells, long YCells, long ZCells, double CellSize,
    long HexCellCount, long CornerSampleCount, long CenterSampleCount, long InitialSampleCount, long MaxCells);

// FixedTolerance/FixedNormalSampleDistance WITNESS the native evaluator's fixed internals — recorded, never chosen.
// Valid is DERIVED from Status — a second stored bit would be a desynchronizable duplicate.
[ValidityEvidence, BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly partial record struct IsoSurfaceReceipt(
    bool NativeRouted, IsoSurfaceStatus Status, IsoSurfaceGrid Grid, int MaxRootSteps, bool ParallelCallback,
    int EvaluatorFailures, int VertexCount, int FaceCount,
    Option<double> FixedTolerance, Option<double> FixedNormalSampleDistance, Option<SdfMeshReceipt> MeshPreflight) : IValidityEvidence {
    public bool Valid => Status.Equals(IsoSurfaceStatus.NativeValid);
    private bool ValidityGate() => !Valid || (EvaluatorFailures == 0 && VertexCount > 0 && FaceCount > 0);
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct IsoSurfaceResult(Mesh Mesh, IsoSurfaceReceipt Receipt);

// Spine carriers + cache keys — declared beside their kernels; the Meshing/mesh cache memoizes by these keys.
internal readonly record struct BoundarySignedHeatKey(SignedHeatTime Heat, VolumeSolverPolicy Solver);
internal readonly record struct ClosedSignedHeatKey(VolumeGridPolicy Grid, SignedHeatTime Heat, VolumeSolverPolicy Solver, VolumeInterpolation Interpolation, VolumeBoundaryCondition BoundaryCondition);
internal readonly record struct SignedHeatSolution(Arr<double> Values, SignedHeatReceipt Receipt, TopologyReceipt Topology);
internal readonly record struct ClosedSignedHeatSolution(VolumeGridDomain Domain, Arr<double> Values, SignedHeatReceipt Receipt, TopologyReceipt Topology);
internal readonly record struct VolumeGridDomain(BoundingBox Bounds, int Resolution, int XCells, int YCells, int ZCells, double CellSize, double Padding, VolumeGridReceipt Receipt) {
    internal int XNodes => XCells + 1; internal int YNodes => YCells + 1; internal int ZNodes => ZCells + 1;
    internal int NodeCount => XNodes * YNodes * ZNodes; internal int CellCount => XCells * YCells * ZCells;
    internal int Index(int x, int y, int z) => x + (XNodes * (y + (YNodes * z)));
    internal Point3d PointAt(int x, int y, int z);
}

[Union]
public abstract partial record SignedHeatOutcome {
    private SignedHeatOutcome() { }
    public sealed record SurfaceCase(SignedHeatSolution Solution) : SignedHeatOutcome;
    public sealed record VolumeCase(ClosedSignedHeatSolution Solution) : SignedHeatOutcome;
    public sealed record TetCase(Arr<double> Values, TetSignedHeatReceipt Receipt) : SignedHeatOutcome;
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class Reconstruction {
    // THE one reconstruction entry: the policy case selects the kernel; admission internalized per case.
    public static Fin<ReconstructionResult> Reconstruct(Seq<MlsSample> samples, ReconstructionPolicy policy, Context context, Op? key = null) =>
        key.OrDefault() switch {
            Op op => policy.Switch(
                rbfCase:     c => BuildRbf(samples: samples.Map(static s => (s.Position, s.Value)), kernel: c.Kernel, radius: c.Radius, smoothing: c.Smoothing, key: op),
                mlsCase:     c => BuildMls(samples: samples, kernel: c.Kernel, radius: c.Radius, context: context, key: op),
                levinCase:   c => BuildLevin(samples: samples, policy: c.Policy, context: context, key: op),
                apssCase:    c => BuildApss(samples: samples, policy: c.Policy, context: context, key: op),
                poissonCase: c => BuildPoisson(samples: samples, policy: c.Policy, context: context, key: op)),
        };
    // RBF: interpolation solves the kernel matrix exactly; approximation augments sqrt(smoothing) diagonal rows into LSQ.
    private static Fin<ReconstructionResult> BuildRbf(Seq<(Point3d Position, double Value)> samples, KernelKind kernel, PositiveMagnitude radius, double smoothing, Op key);
    private static Fin<ReconstructionResult> BuildMls(Seq<MlsSample> samples, KernelKind kernel, PositiveMagnitude radius, Context context, Op key);
    private static Fin<ReconstructionResult> BuildLevin(Seq<MlsSample> samples, LevinMlsPolicy policy, Context context, Op key);
    private static Fin<ReconstructionResult> BuildApss(Seq<MlsSample> samples, ApssPolicy policy, Context context, Op key);
    // Poisson: trilinear inward-normal splat -> 7-pt Laplacian (+screening/Dirichlet per policy) -> definite Cholesky
    // or PinConstant singular solve -> density-weighted isovalue gamma. The lattice triple loops are the named kernel.
    private static Fin<ReconstructionResult> BuildPoisson(Seq<MlsSample> samples, PoissonPolicy policy, Context context, Op key);

    // Point evaluators the frozen Spatial/fields cases delegate to (case names frozen: Rbf/Mls/LevinMls/Apss/Poisson).
    internal static Fin<double> EvaluateRbf(Seq<(Point3d Position, double Value)> samples, KernelKind kernel, double radius, Arr<double> coefficients, Point3d sample, Op key);
    internal static Fin<ReconstructionSample> EvaluateMls(Seq<MlsSample> samples, KernelKind kernel, double radius, Point3d sample, Context context, Op key);
    internal static Fin<(ReconstructionSample Sample, LevinMlsSampleReceipt Levin)> EvaluateLevinMls(Seq<MlsSample> samples, LevinMlsPolicy policy, Point3d sample, Context context, Op key);
    internal static Fin<(ReconstructionSample Sample, ApssSampleReceipt Apss)> EvaluateApss(Seq<MlsSample> samples, ApssPolicy policy, Point3d sample, Context context, Op key);
    // Shared neighborhood collection: neglect radius = support*sqrt(ln(1/eps)), weight floor from context.Relative.
    private static Fin<Neighbor[]> CollectNeighborhood(Seq<MlsSample> samples, Point3d sample, double support, WeightKernelFamily kernel, double neglectEps, int minNeighbors, Context context, Op key);
    private readonly record struct Neighbor(MlsSample Sample, Vector3d Offset, double Distance, double Weight);
}

// THE unified signed-heat spine: heat solve -> unit field -> divergence -> gauge-fixed Poisson -> sign calibration.
// One law, three discretization rows; heat time resolves per row from SignedHeatTime against the row's cell size.
public static class SignedHeatSpine {
    public static Fin<SignedHeatOutcome> Solve(SignedHeatDiscretization discretization, Op? key = null) =>
        key.OrDefault() switch {
            Op op => discretization.Switch(
                tetFemCase:         c => SolveTetSignedHeat(domain: c.Domain, policy: c.Policy, key: op)
                                             .Map(solved => (SignedHeatOutcome)new SignedHeatOutcome.TetCase(Values: solved.Values, Receipt: solved.Receipt)),
                boundarySourceCase: c => c.Space.Cache.SignedHeatDetailed(policy: c.Policy, key: op)
                                             .Map(solution => (SignedHeatOutcome)new SignedHeatOutcome.SurfaceCase(Solution: solution)),
                closedVolumeGridCase: c => c.Space.Cache.ClosedSignedHeatDetailed(policy: c.Policy, key: op)
                                             .Map(solution => (SignedHeatOutcome)new SignedHeatOutcome.VolumeCase(Solution: solution))),
        };

    // ROW 1 — P1 tet FEM: (M + tK) heat, per-cell unit gradients, -V*(grad(phi_i) . g) divergence, PinConstant Poisson,
    // boundary-mean shift + interior-mean sign calibration.
    internal static Fin<(Arr<double> Values, TetSignedHeatReceipt Receipt)> SolveTetSignedHeat(TetMeshDomain domain, TetSignedHeatPolicy policy, Op key);
    internal static Fin<TetSignedHeatSample> SampleTetSignedHeat(TetMeshDomain domain, TetSignedHeatPolicy policy, Arr<double> values, Point3d sample, Context context, Op key);

    // ROW 2 — boundary-source CR surface: naked-edge polylines encoded as signed CR edge sources (Lo->Hi sign, closest-
    // vertex snap with rejection witnesses); heat through the cached CR factor; face field -> intrinsic cotangent
    // divergence -> cached (M + SpdMassShift*L) Poisson -> source-mean shift. REJECTS flipped intrinsic (Unsupported).
    internal static Fin<SignedHeatSolution> ComputeSignedHeatDetailed(MeshSpace space, SdfMeshPolicy policy, Op key) {
        double h = space.Cache.MeanEdgeLength;
        if (h <= RhinoMath.ZeroTolerance) return Fin.Fail<SignedHeatSolution>(key.InvalidResult());
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
    private static Fin<VolumeGridDomain> VolumeGridDomainOf(BoundingBox source, VolumeGridPolicy grid, Op key);   // overflow-checked against grid.MaxNodes
    internal static Fin<double> InterpolateVolumeGrid(VolumeGridDomain domain, Arr<double> values, Point3d sample, Op key);
    internal readonly record struct BoundarySignedHeatSource(Arr<double> Rhs, Seq<int> SourceVertices, int EncodedEdgeSourceCount, int RejectedBoundaryPointCount, int UnmatchedBoundarySegmentCount);
}

// Mesh-SDF dispatch: the SdfMeshMethod row's generated Switch is exhaustive — no when-Equals chain, no dead
// fallback arm; a fourth method row fails to compile until it names its arm. Prewarm factors and caches without sampling.
public static class MeshSdf {
    public static Fin<SdfMeshSample> SignedDistanceDetailed(MeshSpace space, SdfMeshPolicy policy, Point3d sample, Op key) =>
        policy.Admit(key: key).Bind(active => active.Method.Switch(
            generalizedWindingNumber: () =>
                from distance in GeneralizedWindingDistance(space: space, sample: sample, key: key)
                from receipt in ReceiptOf(space: space, policy: active, signedHeat: Option<SignedHeatReceipt>.None)
                select new SdfMeshSample(Distance: active.SignConvention.Multiplier * distance, Receipt: receipt),
            boundarySignedHeat: () =>
                from solution in space.Cache.SignedHeatDetailed(policy: active, key: key)
                from signed in InterpolateOnMesh(space: space, sample: sample, perVertex: solution.Values, key: key)
                from receipt in ReceiptOf(space: space, policy: active, signedHeat: Some(solution.Receipt), topology: Some(solution.Topology))
                select new SdfMeshSample(Distance: active.SignConvention.Multiplier * signed, Receipt: receipt),
            closedSurfaceSignedHeat: () =>
                from solution in space.Cache.ClosedSignedHeatDetailed(policy: active, key: key)
                from signed in SignedHeatSpine.InterpolateVolumeGrid(domain: solution.Domain, values: solution.Values, sample: sample, key: key)
                from receipt in ReceiptOf(space: space, policy: active, signedHeat: Some(solution.Receipt), topology: Some(solution.Topology), volumeGrid: Some(solution.Domain.Receipt))
                select new SdfMeshSample(Distance: active.SignConvention.Multiplier * signed, Receipt: receipt)));
    public static Fin<SdfMeshReceipt> Prewarm(MeshSpace space, SdfMeshPolicy policy, Op key);
    private static Fin<SdfMeshReceipt> ReceiptOf(MeshSpace space, SdfMeshPolicy policy, Option<SignedHeatReceipt> signedHeat, Option<TopologyReceipt> topology = default, Option<VolumeGridReceipt> volumeGrid = default);
    // Solid-angle generalized winding: |w| > 0.5 -> inside -> negative closest distance. Quad faces split into two fans.
    private static Fin<double> GeneralizedWindingDistance(MeshSpace space, Point3d sample, Op key);
    private static Fin<double> SolidAngleWindingNumber(Mesh mesh, Point3d sample, Op key);
    private static double TriangleSolidAngle(Point3d a, Point3d b, Point3d c, Point3d sample) {
        Vector3d va = a - sample; Vector3d vb = b - sample; Vector3d vc = c - sample;
        double la = va.Length; double lb = vb.Length; double lc = vc.Length;
        if (la <= RhinoMath.ZeroTolerance || lb <= RhinoMath.ZeroTolerance || lc <= RhinoMath.ZeroTolerance) return 0.0;
        double det = Vector3d.CrossProduct(a: va, b: vb) * vc;
        double denom = (la * lb * lc) + ((va * vb) * lc) + ((vb * vc) * la) + ((vc * va) * lb);
        return 2.0 * Math.Atan2(y: det, x: denom);
    }
    private static Fin<double> InterpolateOnMesh(MeshSpace space, Point3d sample, Arr<double> perVertex, Op key);   // barycentric at ClosestMeshPoint
}

// Native marching-cubes extraction: prewarm mesh-backed fields, count evaluator failures via Interlocked, classify
// status from (failures, result); key.Catch converts the one native callback boundary.
public static class IsoSurface {
    public static Fin<IsoSurfaceResult> Detailed(ScalarField field, BoundingBox bounds, int resolution, IsoSurfacePolicy policy, Context context, Op key) =>
        AdmitGrid(bounds: bounds, resolution: resolution, policy: policy, key: key)
            .Bind(grid => PreflightOf(field: field, context: context, key: key)
            .Bind(preflight => key.Catch(() => {
                int failures = 0;
                // Increment only — assigning the returned count back would race the parallel callback (lost update).
                double EvaluateIso(Point3d point) =>
                    field.SampleScalar(sample: point, context: context, key: key).Match(
                        Succ: static value => value,
                        Fail: _ => { _ = Interlocked.Increment(location: ref failures); return double.NaN; });
                Mesh? result = Mesh.CreateFromIsosurface(scalarFieldEvaluator: EvaluateIso, box: bounds, resolution: resolution, RootFindingMaxSteps: policy.MaxRootSteps.Value);
                IsoSurfaceStatus status = (failures, result) switch {
                    ( > 0, _) => IsoSurfaceStatus.EvaluatorFailure,
                    (_, null) => IsoSurfaceStatus.NativeReturnedNull,
                    (_, { IsValid: true }) => IsoSurfaceStatus.NativeValid,
                    _ => IsoSurfaceStatus.NativeInvalidMesh,
                };
                return Fin.Succ(new IsoSurfaceResult(
                    Mesh: result ?? new Mesh(),
                    Receipt: new IsoSurfaceReceipt(NativeRouted: true, Status: status, Grid: grid, MaxRootSteps: policy.MaxRootSteps.Value,
                        ParallelCallback: true, EvaluatorFailures: failures,
                        VertexCount: result?.Vertices.Count ?? 0, FaceCount: result?.Faces.Count ?? 0,
                        FixedTolerance: Some(0.001), FixedNormalSampleDistance: Some(1.0e-5), MeshPreflight: preflight)));
            })));
    // Every classified native outcome RETURNS its receipt — a terminal Valid gate here would strip the failure
    // evidence the extract rail inspects; admission failures alone fail the rail, consumers gate on Receipt.Valid.
    private static Fin<IsoSurfaceGrid> AdmitGrid(BoundingBox bounds, int resolution, IsoSurfacePolicy policy, Op key);   // cell census against policy.MaxCells
    private static Fin<Option<SdfMeshReceipt>> PreflightOf(ScalarField field, Context context, Op key);                  // MeshSdf.Prewarm for mesh-backed fields
}
```

```mermaid
flowchart LR
    MlsSample -->|one entry, policy-case dispatch| Reconstruct
    Reconstruct -->|Rbf / Mls / Levin / Apss| ScalarField
    Reconstruct -->|splat + 7pt Laplacian + gauge solve| PoissonGrid
    SignedHeatDiscretization -->|heat -> field -> divergence -> Poisson -> calibrate| SignedHeatSpine
    SignedHeatSpine -->|TetFem row| TetSignedHeatReceipt
    SignedHeatSpine -->|BoundarySource row via dec CR + mesh cache| SignedHeatSolution
    SignedHeatSpine -->|ClosedVolumeGrid row| VolumeGridReceipt
    SdfMeshMethod -->|GWN / boundary / closed dispatch| MeshSdf
    MeshSdf -->|distance + receipt| SdfMeshSample
    ScalarField -->|native marching cubes| IsoSurface
    IsoSurface -->|evaluator-failure witness| IsoSurfaceReceipt
    Reconstruct -.->|degenerate / residual breach| Op
```

## [03]-[DENSITY_BAR]

| [INDEX] | [AXIS/CONCERN]        | [OWNER]                                  | [KIND]                                                              | [RAIL]                                          | [CASES] |
| :-----: | :-------------------- | :---------------------------------------- | :------------------------------------------------------------------ | :----------------------------------------------- | :-----: |
|  [01]   | Construction          | `ReconstructionPolicy` → `Reconstruction.Reconstruct` | `[Union]` policy discriminant, one entry, per-case admission | `Reconstruct → Fin<ReconstructionResult>`        |    5    |
|  [02]   | Mode vocabulary       | `ReconstructionMode`                      | `[SmartEnum<int>]` with normals/sparse/degree columns                | discriminant                                     |    7    |
|  [03]   | Signed-heat spine     | `SignedHeatDiscretization` → `SignedHeatSpine.Solve` | `[Union]` rows over ONE four-stage law                    | `Solve → Fin<SignedHeatOutcome>`                 |    3    |
|  [04]   | Mesh SDF              | `SdfMeshPolicy` → `MeshSdf`               | method-row dispatch, cache-backed solves                             | `SignedDistanceDetailed → Fin<SdfMeshSample>`    |    3    |
|  [05]   | Tet domain            | `TetMeshDomain`                           | validated domain, boundary topology derived at admission             | `Of → Fin<TetMeshDomain>`                        |    1    |
|  [06]   | Volume grid           | `VolumeGridPolicy` / `VolumeGridDomain`   | resolution-xor-cellsize + ceiling policy rows                        | `VolumeGridDomainOf → Fin<VolumeGridDomain>`     |    —    |
|  [07]   | Iso extraction        | `IsoSurface`                              | native marching-cubes adapter, failure-classified receipt            | `Detailed → Fin<IsoSurfaceResult>`               |    4    |
|  [08]   | Policy family         | `SignedHeatTime` … `PoissonPolicy`        | validated policy records with presets; dead octree knobs dropped     | `Of → Fin<policy>` per record                    |    —    |

`Reconstruct`, `SignedHeatSpine.Solve`, `ComputeSignedHeatDetailed`, `MeshSdf.SignedDistanceDetailed`, `TriangleSolidAngle`, and `IsoSurface.Detailed` are transcription-complete; the build kernels (`BuildRbf`/`BuildMls`/`BuildLevin`/`BuildApss`/`BuildPoisson`), the deep evaluators, the tet FEM assembly, and the volume-grid row are signature-fixed with their bodies the algorithms the `[04]` contracts specify — each invariant they compute is a gated receipt field (Pratt norm positivity, splat conservation, residual tolerances, watertight admission), so a body cannot silently weaken.

## [04]-[RESEARCH]

- [RECONSTRUCTION_KERNELS] — RBF: symmetric kernel-matrix interpolation (exact when smoothing ≤ `ZeroTolerance`; Tikhonov-style `√smoothing` diagonal augmentation into least squares otherwise), evaluation as `Σ wᵢ·φ(|x − xᵢ|)` through the settled `KernelKind.Profile`. MLS: per-neighbor 4-equation design (`value` row + three `gradient = normal` rows, `√profile` weighted), rank ≥ 4 and normal-agreement ≥ 0.5 gates, SVD condition witnessed. Levin (two-step MLS): covariance plane seed → Brent root of the weighted energy derivative along the normal (`bracket = BracketFactor·support`, `accuracy = RootTol·support`, positive second derivative required) alternated with normal re-estimation until `StepEps`, then ridge-regularized (`RidgeLambda`) degree-`PolyDegree` height fit in the local tangent frame; the planarity gate `λ0/λ2 ≤ PlanarityTau` rejects non-surface neighborhoods. APSS (Guennebaud-Gross): weighted algebraic sphere `(hc, hl, hq)` under Pratt normalization `|hl|² − 4·hc·hq = 1`, plane branch below `EpsDegeneracy`, damped iterative projection under `ProjTol` with the Taubin residual witnessed. The law-matrix (`ReconstructionLaws`, CsCheck under `testing-cs`) asserts: RBF interpolation reproduces sample values within solve residual; MLS/Levin/APSS reproduce the signed-distance sign of oriented-plane fixtures; every evaluator's receipt counts satisfy `Neighborhood + Rejected == SampleCount`-band laws; Poisson's `γ`-shifted indicator is negative at interior probes of closed fixtures.
- [SCREENED_POISSON] — Kazhdan screened-Poisson on the dense `2^Depth` regular lattice (no octree — the octree-era knobs are dropped dead parameterization): trilinear inward-normal splat with density normalization (`weightFloor = max(√ε, Density)`), 7-point Laplacian with one-sided boundary stencils, screening term `α·B_i·B_j` outer products (`α = 8^Depth · PointWeight`), Dirichlet row imposition per `PoissonBoundary.IsDirichlet` with the exterior value folded into the RHS, definite path through `CholeskySparse`, singular path through `SingularSolveDetailed` under `PinConstant(interiorIndex, PinZero)`. `UnscreenedEquivalence` is a receipt LAW: an unscreened run must witness zero screening NNZ/energy and no data residual — the equivalence of `Poisson` and `ScreenedPoisson(PointWeight: 0)` is checkable, not assumed.
- [SIGNED_HEAT_SPINE] — the unified signed-heat method (heat diffusion → normalized field → divergence → Poisson → calibration) over three discretizations of ONE law: P1 tet FEM (per-cell Jacobian-inverse gradients; mass `V·(1+δᵢⱼ)/20`; heat RHS from boundary-membership mass rows; `PinConstant(firstBoundary, PinZero)` gauge; boundary-mean shift and interior-mean sign), boundary-source CR surface (signed edge-source encoding over naked-edge polylines with snap-rejection witnesses; the `Meshing/dec` CR factor through the `Meshing/mesh` memo; intrinsic cotangent divergence; the cached `(M + SpdMassShift·L)` Poisson; source-mean shift), and closed-surface volume grid (watertight admission conjunction; softened kernel `Area·e^{−r/√t}/r` with `ε = h²·KernelSofteningRatio`; 7-point FD Poisson under `MeanZeroConstant(MinZero)`; source-mean shift + interior sign flip). Heat time is `SignedHeatTime.Resolve` against the row's characteristic cell size. The law-matrix asserts: zero level set within tolerance of the source set, sign consistency at interior/exterior probes, residual gates against `VolumeSolverPolicy`, and the spine-row receipts carrying every stage's solve.
- [MESH_SDF_AND_ISO] — generalized winding (Jacobson et al. solid-angle sum; `|w| > 0.5` classifies inside; distance from native `ClosestPoint`; robust to open seams where `IsPointInside` is not), boundary and closed signed heat as spine projections, and native `Mesh.CreateFromIsosurface` extraction with the parallel evaluator callback failure-counted through `Interlocked` and the `(failures, result)` status classification — an `EvaluatorFailure` receipt with a non-null mesh is still invalid by law, because a NaN-poisoned lattice produces plausible-looking garbage. `Spatial/fields` `SignedDistanceFromMesh`/`TetSignedHeat` cases and the `ScalarField.SampleSdfDetailed` rail delegate here; `Processing/sample` surface seeding and settled `Meshing/intersect` never consume these approximate distances for predicate decisions — the robust-core altitude split stands.

## [05]-[CROSS_PAGE_SEAMS]

- `Spatial/fields` owns the `ScalarField` union and its frozen case names; this page owns the kernels those cases delegate to and the `ReconstructionResult` build product — the field algebra is never re-taught here, and the SDF-primitive rail (`SdfKind`, `SdfReceipt`, `SdfSample`) stays that page's own.
- `Meshing/mesh` owns the `SignedHeatDetailed`/`ClosedSignedHeatDetailed`/`EdgeConnectionCholeskyDetailed`/`ScalarHeatCholesky`/`Cholesky` memo slots and `SpdMassShift`; this page declares the keys, solutions, kernels, and receipts those slots memoize — the cache stays the one memo service.
- `Meshing/dec` owns the CR system assembly, face-field sampling, and intrinsic divergence the boundary-source row composes; `Numerics/calculus` owns the kernel-profile math; `Numerics/matrix` owns every solve; `Processing/extract` consumes `IsoSurface.Detailed` for its `IsoSurface` extraction case.
