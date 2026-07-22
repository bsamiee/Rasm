# [RASM_NUMERICS_SPECTRAL]

`Rasm.Numerics` spectral owns the mesh-free discrete-exterior-calculus carrier layer and the spectral filter/descriptor algebra with zero mesh coupling, so the pure math floor stands independent of any `Mesh` and every eigenvalue-driven consumer meets one transfer-function and descriptor surface.

`DiscreteCalculus` is the frozen cross-package seam name the `Rasm.Compute` adjoint surface binds; `Meshing/dec` constructs the DEC carriers and receipts this page only declares, and `Meshing/mesh` declares `SignpostTransportReceipt` as optional carried evidence, so each DDG receipt has one declaration site. `SpectralFilter` weights eigenvalues alone, the `[SmartEnum<int>]` policy vocabularies drive filter, normalization, and distance selection, and every consumer reads carriers and descriptors from this floor without touching a `Mesh`.

## [01]-[INDEX]

- [02]-[FILTER_ALGEBRA]: the `[SmartEnum<int>]` policy vocabularies and `SpectralFilter`, the closed transfer-function `[Union]` with its eigenvalue weight law and partial-monoid `Compose`.
- [03]-[DEC_CARRIERS]: `DiscreteCalculus` the frozen adjoint seam, `SpectralBasis`, and the assembly/harmonic receipt family `Meshing/dec` mints and `Rasm.Compute` consumes.
- [04]-[DESCRIPTOR_ALGEBRA]: the descriptor policy, receipt, and carrier family and `SpectralKernel` filtered-signature evaluation, normalization, and ranking.

## [02]-[FILTER_ALGEBRA]

- Owner: `SpectralAssemblyKind`, `SpectralScaleNormalization`, `SpectralEnergyNormalization`, `SpectralZeroModePolicy`, and `SpectralDistanceKind` are the `[SmartEnum<int>]` policy vocabularies, the distance row carrying its `[UseDelegateFromConstructor]` compute column over `MathNet.Numerics.Distance`; `SpectralFilter` is the closed `[Union]` whose `Weight(eigenvalue)` is the spectral transfer function and whose `Compose` is a partial monoid — composable pairs fuse, `Identity` is the unit, every other pair is `None` by law.
- Cases: `HeatCase(time)`, `WaveCase(energy, bandwidth)`, `BiharmonicCase`, `DiffusionCase(time)`, `CommuteTimeCase`, `IdentityCase`.
- Entry: the `SpectralFilter.Heat`/`Wave`/`Biharmonic`/`Diffusion`/`CommuteTime`/`Identity` factories take their parameters as `PositiveMagnitude`, so a filter in hand is admitted; `ApplyDetailed(basis, sources, policy, key)` evaluates the filtered descriptor through `SpectralKernel`.
- Auto: `Weight` carries `[MethodImpl(AggressiveInlining)]` for the descriptor kernel's per-eigenpair-per-vertex hot loop; the wave weight floors its bandwidth at `SpectralKernel.WaveBandwidthFloor` so a degenerate band never divides to infinity.
- Receipt: none at this layer — the filter is policy, and evidence lands on the [04] descriptor receipts.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new transfer function is one case, one `Weight` arm, and at most one `Compose` pair, the kernel and every consumer untouched; a new normalization or distance is one vocabulary row.
- Boundary: filters weight eigenvalues alone — never a mesh, a basis matrix, or a vertex — so the one filter value drives `Meshing/dec` heat scaffolds, `Processing/segment` descriptors, and `Spatial/fields` spectral-distance cases from this floor.

```csharp
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System.Numerics.Tensors;
using Rasm.Domain;
using Rasm.Meshing;

namespace Rasm.Numerics;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class SpectralAssemblyKind {
    public static readonly SpectralAssemblyKind Dec = new(key: 0);
    public static readonly SpectralAssemblyKind EdgeConnection = new(key: 1);
}

[SmartEnum<int>]
public sealed partial class SpectralScaleNormalization {
    public static readonly SpectralScaleNormalization Raw = new(key: 0);
    public static readonly SpectralScaleNormalization FirstNonZeroEigenvalue = new(key: 1);
}

[SmartEnum<int>]
public sealed partial class SpectralEnergyNormalization {
    public static readonly SpectralEnergyNormalization Raw = new(key: 0);
    public static readonly SpectralEnergyNormalization UnitL1 = new(key: 1);
    public static readonly SpectralEnergyNormalization UnitL2 = new(key: 2);
    public static readonly SpectralEnergyNormalization ZScore = new(key: 3);
}

[SmartEnum<int>]
public sealed partial class SpectralZeroModePolicy {
    public static readonly SpectralZeroModePolicy Keep = new(key: 0);
    public static readonly SpectralZeroModePolicy Drop = new(key: 1);
}

[SmartEnum<int>]
public sealed partial class SpectralDistanceKind {
    public static readonly SpectralDistanceKind Euclidean = new(key: 0, compute: static (a, b) => MathNet.Numerics.Distance.Euclidean(a, b));
    public static readonly SpectralDistanceKind Manhattan = new(key: 1, compute: static (a, b) => MathNet.Numerics.Distance.Manhattan(a, b));
    public static readonly SpectralDistanceKind Cosine = new(key: 2, compute: static (a, b) => MathNet.Numerics.Distance.Cosine(a, b));
    [UseDelegateFromConstructor] internal partial double Compute(double[] a, double[] b);
}

[Union]
public abstract partial record SpectralFilter {
    public sealed record HeatCase(PositiveMagnitude Time) : SpectralFilter;
    public sealed record WaveCase(PositiveMagnitude Energy, PositiveMagnitude Bandwidth) : SpectralFilter;
    public sealed record BiharmonicCase : SpectralFilter;
    public sealed record DiffusionCase(PositiveMagnitude Time) : SpectralFilter;
    public sealed record CommuteTimeCase : SpectralFilter;
    public sealed record IdentityCase : SpectralFilter;
    private SpectralFilter() { }
    public static SpectralFilter Heat(PositiveMagnitude time) => new HeatCase(Time: time);
    public static SpectralFilter Wave(PositiveMagnitude energy, PositiveMagnitude bandwidth) => new WaveCase(Energy: energy, Bandwidth: bandwidth);
    public static SpectralFilter Biharmonic => new BiharmonicCase();
    public static SpectralFilter Diffusion(PositiveMagnitude time) => new DiffusionCase(Time: time);
    public static SpectralFilter CommuteTime => new CommuteTimeCase();
    public static SpectralFilter Identity => new IdentityCase();
    // Partial monoid: composable pairs fuse, Identity is the unit, every other pair is None.
    public Option<SpectralFilter> Compose(SpectralFilter other) =>
        (this, other) switch {
            (HeatCase a, HeatCase b) => Positive(value: a.Time.Value + b.Time.Value).Map(static time => Heat(time: time)),
            (DiffusionCase a, DiffusionCase b) => Positive(value: a.Time.Value + b.Time.Value).Map(static time => Diffusion(time: time)),
            (IdentityCase, _) when other is not null => Some(other),
            (_, IdentityCase) => Some(this),
            _ => Option<SpectralFilter>.None,
        };
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal double Weight(double eigenvalue) => Switch(
        state: eigenvalue,
        heatCase: static (lambda, c) => Math.Exp(d: -c.Time.Value * lambda),
        waveCase: static (lambda, c) => RawWaveWeight(eigenvalue: lambda, energy: c.Energy.Value, bandwidth: c.Bandwidth.Value),
        biharmonicCase: static (lambda, _) => lambda > EpsilonPolicy.SqrtEpsilon ? 1.0 / (lambda * lambda) : 0.0,
        diffusionCase: static (lambda, c) => Math.Exp(d: -2.0 * c.Time.Value * lambda),
        commuteTimeCase: static (lambda, _) => lambda > EpsilonPolicy.SqrtEpsilon ? 1.0 / lambda : 0.0,
        identityCase: static (_, _) => 1.0);
    internal Fin<SpectralDescriptor> ApplyDetailed(SpectralBasis basis, Option<Seq<int>> sources, Op key) =>
        ApplyDetailed(basis: basis, sources: sources, policy: SpectralDescriptorPolicy.Raw, key: key);
    internal Fin<SpectralDescriptor> ApplyDetailed(SpectralBasis basis, Option<Seq<int>> sources, SpectralDescriptorPolicy policy, Op key) =>
        Optional(this).ToFin(key.InvalidInput())
            .Bind(filter => guard(basis.IsValid, key.InvalidInput()).ToFin()
            .Bind(_ => SpectralDescriptorPolicy.Admit(policy: policy, key: key))
            .Bind(activePolicy => SpectralKernel.EvaluateFilteredDetailed(basis: basis, sources: sources, filter: filter, policy: activePolicy, key: key)));
    private static double RawWaveWeight(double eigenvalue, double energy, double bandwidth) =>
        eigenvalue < EpsilonPolicy.SqrtEpsilon ? 0.0 : ((Math.Log(d: energy) - Math.Log(d: eigenvalue)) / Math.Max(val1: bandwidth, val2: SpectralKernel.WaveBandwidthFloor)) switch { double ratio => Math.Exp(d: -0.5 * ratio * ratio) };
    private static Option<PositiveMagnitude> Positive(double value) =>
        PositiveMagnitude.TryCreate(value: value, obj: out PositiveMagnitude magnitude) ? Some(magnitude) : Option<PositiveMagnitude>.None;
}
```

## [03]-[DEC_CARRIERS]

- Owner: `DiscreteCalculus` is the DEC operator bundle — incidence and curl operators, the diagonal Hodge stars, its `SpectralAssemblyReceipt`, and the optional `Transport` and `Harmonic` evidence slots — with `Project<TOut>` routing the evidence family through typed `ProjectionRow` rows. `SpectralBasis` is the eigenpair carrier with `Truncate(k)` and the ONE scale-relative `ZeroBand` (`SqrtEpsilon` × spectral radius) the descriptor kernel reuses to classify zero modes, so one threshold declaration carries every consumer with zero drift. `SpectralAssemblyReceipt`, `HarmonicOneFormReceipt`, and `HarmonicOneFormBasis` are the assembly and harmonic evidence, their semantic gates scale-relative against `max(1, spectralRadius)` rather than any bare absolute.
- Entry: carriers are constructed by `Meshing/dec` (assembly) and `Meshing/mesh` (caching); this page owns their shape, validity law, and projection, and consumers — the `Rasm.Compute` adjoint surface, `Processing/geodesics`, `Processing/segment`, `Spatial/fields` — read them from here.
- Auto: `DiscreteCalculus.IsValid` cross-couples the stars to the operator shapes, requires strictly positive vertex and face stars, and admits edge stars down to a scale-relative negative band, where near-degenerate intrinsic cotan weights legitimately dip below zero within roundoff of the `Star1` scale.
- Receipt: all three receipts fold `ValidityClaim.All` with `IValidityEvidence` registration; the semantic gates carry the harmonic dimension law, the `Rank + Nullity == EdgeCount` partition, and the residual-tolerance ladder.
- Packages: `matrix.md` owners (`SparseMatrix`, `EigenSolveReceipt`), LanguageExt.Core, Rasm.Domain (`IValidityEvidence` + `ValidityClaim`, `Op`).
- Growth: a new DEC operator is one field, one validity coupling, and one `ProjectionRow`; a new assembly witness is one receipt field.
- Boundary: `DiscreteCalculus` is the `Rasm.Compute` adjoint seam; `SignpostTransportReceipt` is declared by `Meshing/mesh`, the intrinsic-triangulation owner, and carried here only as optional evidence, so each DDG receipt has exactly one declaration site with this page owning the mesh-free members.

```csharp
// --- [MODELS] -----------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct SpectralAssemblyReceipt(
    int VertexCount, int EdgeCount, int FaceCount, int AdmittedFaceCount, int SkippedDegenerateFaces, int SkippedMissingEdges,
    bool FlippedIntrinsicRejected, int MatrixRows, int MatrixCols, int NonZeros,
    int PositiveStar0Count, int PositiveStar1Count, int PositiveStar2Count,
    double BoundaryCompositionResidual, Option<int> Genus, int HarmonicDimension, SpectralAssemblyKind Kind,
    int BoundaryEdgeCount = 0, int BoundaryComponentCount = 0, int NonManifoldEdgeCount = 0, int EulerCharacteristic = 0,
    bool TopologyEulerValidated = false, int ComponentCount = 1, int PositiveMassCount = 0,
    double SymmetryResidual = 0.0, double SymmetryTolerance = 0.0, Option<int> FactorNonZeros = default,
    double BoundaryCompositionTolerance = 0.0) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Of(VertexCount >= 0 && EdgeCount >= 0 && FaceCount >= 0 && MatrixRows >= 0 && MatrixCols >= 0 && NonZeros >= 0),
        ValidityClaim.Nonnegative(value: BoundaryCompositionResidual),
        ValidityClaim.Nonnegative(value: SymmetryResidual),
        ValidityClaim.Of(AdmittedFaceCount + SkippedDegenerateFaces + SkippedMissingEdges <= FaceCount),
        ValidityClaim.Of(SymmetryResidual <= SymmetryTolerance),
        // Tolerance 0 = witness-only (the minting assembly gated the residual itself); positive enforces.
        ValidityClaim.Of(BoundaryCompositionTolerance <= 0.0 || BoundaryCompositionResidual <= BoundaryCompositionTolerance),
        ValidityClaim.Of(FactorNonZeros.Map(static value => value > 0).IfNone(noneValue: true)),
        ValidityClaim.Of(Kind is not null
            && (Kind.Equals(SpectralAssemblyKind.EdgeConnection)
                ? ComponentCount == 2 && MatrixRows == EdgeCount * ComponentCount && MatrixCols == MatrixRows && PositiveMassCount <= EdgeCount
                : ComponentCount == 1 && PositiveStar0Count <= VertexCount && PositiveStar1Count <= EdgeCount && PositiveStar2Count <= FaceCount
                  && (Genus is { IsSome: true, Case: int genus }
                      ? genus >= 0 && HarmonicDimension == (2 * genus) + Math.Max(0, BoundaryComponentCount - 1)
                      : HarmonicDimension == 0))));
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct HarmonicOneFormReceipt(
    Option<int> Genus, int ExpectedDimension, int ConstraintRows, int EdgeCount, int Rank, int Nullity, int BasisCount,
    double SvdTolerance, double EpsRank, double SpectralRadius, double NullspaceThreshold,
    double MinNullEigenvalue, double MaxNullEigenvalue, double MaxClosedResidual, double MaxCoClosedResidual,
    double Star1OrthonormalResidual, int PositiveStar1Count, EigenSolveReceipt<double, Arr<double>> Eigen,
    int BoundaryComponentCount = 0) : IValidityEvidence {
    public bool IsValid {
        get {
            int expected = ExpectedDimension;
            int boundaryComponentCount = BoundaryComponentCount;
            // Residual gate is operator-scale-relative: SvdTolerance and the sqrt-machineEps floor carry
            // max(1, spectralRadius); 1e3 is dimensionless rounding slack above the eigenvalue tolerance, never a bare absolute.
            double residualTolerance = Math.Max(val1: SvdTolerance, val2: EpsilonPolicy.SqrtEpsilon * Math.Max(val1: 1.0, val2: SpectralRadius)) * 1.0e3;
            return ValidityClaim.All(
                ValidityClaim.Of(EdgeCount >= 0 && Rank >= 0 && Nullity >= 0 && BasisCount >= 0 && ConstraintRows >= 0),
                ValidityClaim.Of(Rank + Nullity == EdgeCount),
                ValidityClaim.CountExactly(count: BasisCount, expected: expected),
                ValidityClaim.Of(Nullity >= expected),
                ValidityClaim.Of(PositiveStar1Count <= EdgeCount),
                ValidityClaim.Of(Genus.Map(genus => expected == (2 * genus) + Math.Max(0, boundaryComponentCount - 1)).IfNone(expected == 0)),
                ValidityClaim.Positive(value: SvdTolerance),
                ValidityClaim.Positive(value: EpsRank),
                ValidityClaim.Of(Math.Abs(value: SvdTolerance - NullspaceThreshold) <= EpsilonPolicy.SqrtEpsilon * Math.Max(val1: 1.0, val2: NullspaceThreshold)),
                ValidityClaim.Of(NullspaceThreshold <= (EpsRank * Math.Max(val1: 1.0, val2: SpectralRadius)) + EpsilonPolicy.SqrtEpsilon),
                ValidityClaim.Of(MinNullEigenvalue >= -EpsilonPolicy.SqrtEpsilon),
                ValidityClaim.Of(MaxNullEigenvalue >= MinNullEigenvalue - EpsilonPolicy.SqrtEpsilon),
                ValidityClaim.Of(MaxClosedResidual <= residualTolerance),
                ValidityClaim.Of(MaxCoClosedResidual <= residualTolerance),
                ValidityClaim.Of(Star1OrthonormalResidual <= residualTolerance),
                ValidityClaim.Evidence(Eigen));
        }
    }
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct HarmonicOneFormBasis(Arr<Arr<double>> Forms, HarmonicOneFormReceipt Receipt) {
    public bool IsValid {
        get {
            HarmonicOneFormReceipt receipt = Receipt;
            return receipt.IsValid && Forms.Count == receipt.BasisCount && Forms.ForAll(form => form.Count == receipt.EdgeCount && form.ForAll(double.IsFinite));
        }
    }
}

// THE Rasm.Compute adjoint seam — name, fields, and projection rows are the cross-package contract; Transport evidence is declared by Meshing/mesh and carried here.
[StructLayout(LayoutKind.Auto)]
public readonly record struct DiscreteCalculus(SparseMatrix D0, SparseMatrix D1, Arr<double> Star0, Arr<double> Star1, Arr<double> Star2, SpectralAssemblyReceipt Receipt, Option<SignpostTransportReceipt> Transport = default, Option<HarmonicOneFormBasis> Harmonic = default) {
    public bool IsValid =>
        D0.IsValid && D1.IsValid && Receipt.IsValid
        && Star0.Count == D0.Cols.Value && Star1.Count == D0.Rows.Value && Star2.Count == D1.Rows.Value
        && Star0.ForAll(static v => double.IsFinite(v) && v > 0.0)
        && Star1.Fold(0.0, static (max, v) => Math.Max(max, Math.Abs(v))) is double star1Scale
        && Star1.ForAll(v => double.IsFinite(v) && v >= -(EpsilonPolicy.SqrtEpsilon * Math.Max(1.0, star1Scale)))
        && Star2.ForAll(static v => double.IsFinite(v) && v > 0.0)
        && Transport.Map(static receipt => receipt.IsValid).IfNone(noneValue: true)
        && Harmonic.Map(static basis => basis.IsValid).IfNone(noneValue: true);
    internal Fin<TOut> Project<TOut>(Op key) {
        DiscreteCalculus self = this;
        return AtomProjection.Rows<DiscreteCalculus, TOut>(self: self, key: key,
            ProjectionRow.Of<SpectralAssemblyReceipt>(() => Fin.Succ(self.Receipt)),
            ProjectionRow.Of<SignpostTransportReceipt>(() => self.Transport.ToFin(key.InvalidResult())),
            ProjectionRow.Of<HarmonicOneFormBasis>(() => self.Harmonic.ToFin(key.InvalidResult())),
            ProjectionRow.Of<HarmonicOneFormReceipt>(() => self.Harmonic.Map(static basis => basis.Receipt).ToFin(key.InvalidResult())));
    }
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct SpectralBasis(Arr<double> Eigenvalues, Arr<Arr<double>> Eigenvectors) {
    internal int VertexCount => Eigenvectors.IsEmpty ? 0 : Eigenvectors[index: 0].Count;
    internal double SpectralRadius => Eigenvalues.Fold(initialState: 0.0, f: static (max, lambda) => Math.Max(val1: max, val2: Math.Abs(value: lambda)));
    // THE one zero band, scale-relative to the spectral radius: eigen error grows with operator scale, so
    // an absolute SqrtEpsilon band misclassifies every mode of an mm-unit spectrum as zero.
    internal double ZeroBand => EpsilonPolicy.SqrtEpsilon * Math.Max(val1: SpectralRadius, val2: EpsilonPolicy.ZeroTolerance);
    public bool IsValid {
        get {
            int vertexCount = VertexCount;
            double zeroBand = ZeroBand;
            return Eigenvalues.Count > 0
                && Eigenvalues.Count == Eigenvectors.Count
                && Eigenvalues.ForAll(lambda => double.IsFinite(lambda) && lambda >= -zeroBand)
                && Eigenvectors.ForAll(phi => vertexCount > 0 && phi.Count == vertexCount && phi.ForAll(double.IsFinite));
        }
    }
    public SpectralBasis Truncate(int k) =>
        k <= 0 || k >= Eigenvalues.Count ? this : new SpectralBasis(Eigenvalues: new Arr<double>([.. Eigenvalues.AsIterable().Take(k)]), Eigenvectors: new Arr<Arr<double>>([.. Eigenvectors.AsIterable().Take(k)]));
}
```

## [04]-[DESCRIPTOR_ALGEBRA]

- Owner: `SpectralDescriptorPolicy` is the normalization bundle (scale × energy × zero-mode × optional crop) with `Raw` the no-op row and `Admit` the gate; `SpectralWaveReceipt` and `SpectralDescriptorReceipt` carry the WKS and descriptor evidence; `SpectralDescriptor` is the values and receipt carrier with `Normalize(policy)` and `Rank(candidates, policy)`; `SpectralRankingPolicy`/`SpectralRank`/`SpectralRanking` carry ranking. `SpectralKernel` is the `internal static` evaluation owner — the dense-buffer filtered-signature kernel, energy normalization over `TensorPrimitives`, and ranking off the `SpectralDistanceKind` compute column — and declares `WaveBandwidthFloor`; the harmonic eps-rank default stays `Meshing/dec`'s, declared beside the construction that applies it.
- Entry: `filter.ApplyDetailed(basis, sources, policy, key)` is the evaluation entry; `descriptor.Normalize(policy)` and `descriptor.Rank(candidates, policy)` the post-processing entries — one descriptor pipeline, no sibling evaluate/compare surfaces.
- Auto: WKS weights normalize to unit sum with the full `SpectralWaveReceipt` minted inline; `ComparisonReady` derives from policy and wave evidence so a raw HKS never ranks against a normalized WKS; `RankDescriptors` re-normalizes every candidate to the query policy before measuring.
- Receipt: `SpectralWaveReceipt` and `SpectralDescriptorReceipt` fold `ValidityClaim.All` with semantic gates over the source, eigenpair, and vertex couplings and the WKS unit-sum.
- Packages: MathNet.Numerics (`Distance.Euclidean/Manhattan/Cosine`), System.Numerics.Tensors (`TensorPrimitives.SumOfMagnitudes`/`Norm`/`IsFiniteAll`), LanguageExt.Core, Thinktecture.Runtime.Extensions.
- Growth: a new signature family is one filter case and policy rows, the kernel loop already generic over the weight function; a new distance is one `SpectralDistanceKind` row, the compute column IS the arm.
- Boundary: `EvaluateFilteredDetailed`'s dense `double[]` loop is the named statement-kernel exemption; the kernel is mesh-free, seeing vertex COUNT as its only topology, so it serves tet, grid, and mesh bases identically, while mesh-side basis computation and caching (`SpectralBasisBundle`) are `Meshing/dec`'s.

```csharp
// --- [MODELS] -----------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct SpectralDescriptorPolicy(SpectralScaleNormalization ScaleNormalization, SpectralEnergyNormalization EnergyNormalization, SpectralZeroModePolicy ZeroModePolicy, Option<Dimension> CropCount) {
    public static SpectralDescriptorPolicy Raw => new(ScaleNormalization: SpectralScaleNormalization.Raw, EnergyNormalization: SpectralEnergyNormalization.Raw, ZeroModePolicy: SpectralZeroModePolicy.Keep, CropCount: None);
    internal bool IsValid => ScaleNormalization is not null && EnergyNormalization is not null && ZeroModePolicy is not null && CropCount.Map(static count => count.Value > 0).IfNone(noneValue: true);
    internal bool IsRaw => ScaleNormalization.Equals(SpectralScaleNormalization.Raw) && EnergyNormalization.Equals(SpectralEnergyNormalization.Raw) && ZeroModePolicy.Equals(SpectralZeroModePolicy.Keep) && CropCount.IsNone;
    internal bool IsValueOnly => ScaleNormalization.Equals(SpectralScaleNormalization.Raw) && ZeroModePolicy.Equals(SpectralZeroModePolicy.Keep) && CropCount.IsNone;
    internal static Fin<SpectralDescriptorPolicy> Admit(SpectralDescriptorPolicy policy, Op key) =>
        guard(policy.IsValid, key.InvalidInput()).ToFin().Map(_ => policy);
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct SpectralWaveReceipt(double Energy, double Bandwidth, Option<double> FirstNonZeroScale, int ZeroModeCount, int CroppedEigenpairCount, int NonZeroEigenpairCount, double RawWeightSum, double NormalizedWeightSum, Option<double> MinLogEigenvalue, Option<double> MaxLogEigenvalue, bool WksNormalized) : IValidityEvidence {
    public bool IsValid {
        get {
            Option<double> maxLogEigenvalue = MaxLogEigenvalue;
            return ValidityClaim.All(
                ValidityClaim.Positive(value: Energy),
                ValidityClaim.Positive(value: Bandwidth),
                ValidityClaim.Of(ZeroModeCount >= 0 && CroppedEigenpairCount >= NonZeroEigenpairCount && NonZeroEigenpairCount > 0),
                ValidityClaim.Positive(value: RawWeightSum),
                ValidityClaim.Of(!WksNormalized || Math.Abs(value: NormalizedWeightSum - 1.0) <= 1.0e-9),
                ValidityClaim.Of(FirstNonZeroScale.Map(static first => first > 0.0).IfNone(noneValue: true)),
                ValidityClaim.Of(MinLogEigenvalue.Map(min => maxLogEigenvalue.Map(max => min <= max).IfNone(noneValue: true)).IfNone(noneValue: true)));
        }
    }
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct SpectralDescriptorReceipt(SpectralFilter Filter, int VertexCount, int EigenpairCount, int SourceCount, bool ComparisonReady, bool Pairwise, bool EnergyNormalized, bool ScaleNormalized, SpectralDescriptorPolicy Policy = default, int ZeroModeCount = 0, int CroppedEigenpairCount = 0, Option<SpectralWaveReceipt> Wave = default) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Of(Filter is not null),
        ValidityClaim.Of(VertexCount > 0 && EigenpairCount > 0),
        ValidityClaim.Of(CroppedEigenpairCount > 0 && CroppedEigenpairCount <= EigenpairCount),
        ValidityClaim.Of(ZeroModeCount >= 0 && ZeroModeCount <= EigenpairCount),
        ValidityClaim.Of(SourceCount >= 0 && SourceCount <= VertexCount),
        ValidityClaim.Of(!Pairwise || SourceCount > 0),
        ValidityClaim.Of(Policy.IsValid),
        ValidityClaim.Of(!ComparisonReady || !Policy.IsRaw || Wave.IsSome),
        ValidityClaim.Of(Wave.Map(static wave => wave.IsValid).IfNone(noneValue: true)));
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct SpectralDescriptor(Arr<double> Values, SpectralDescriptorReceipt Receipt) {
    public bool IsValid => Receipt.IsValid && Values.Count == Receipt.VertexCount && Values.ForAll(double.IsFinite);
    public Fin<SpectralDescriptor> Normalize(SpectralDescriptorPolicy policy, Op? key = null) =>
        SpectralKernel.NormalizeDescriptor(descriptor: this, policy: policy, key: key.OrDefault());
    public Fin<SpectralRanking> Rank(Seq<SpectralDescriptor> candidates, SpectralRankingPolicy policy, Op? key = null) =>
        SpectralKernel.RankDescriptors(query: this, candidates: candidates, policy: policy, key: key.OrDefault());
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct SpectralRankingPolicy(SpectralDescriptorPolicy Descriptor, SpectralDistanceKind Distance) {
    public static SpectralRankingPolicy Default => new(Descriptor: SpectralDescriptorPolicy.Raw, Distance: SpectralDistanceKind.Euclidean);
    internal bool IsValid => Descriptor.IsValid && Distance is not null;
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct SpectralRank(int Index, double Distance, SpectralDescriptor Descriptor);

[StructLayout(LayoutKind.Auto)]
public readonly record struct SpectralRanking(SpectralDescriptor Query, Seq<SpectralRank> Items, SpectralRankingPolicy Policy);

// --- [OPERATIONS] -------------------------------------------------------------------------
internal static class SpectralKernel {
    internal const double WaveBandwidthFloor = 1e-9;
    // STATEMENT-KERNEL EXEMPTION — dense per-eigenpair-per-vertex accumulation buffer; a Seq fold churns allocations over n * k (* |S| pairwise) terms.
    internal static Fin<SpectralDescriptor> EvaluateFilteredDetailed(SpectralBasis basis, Option<Seq<int>> sources, SpectralFilter filter, SpectralDescriptorPolicy policy, Op key) {
        int n = basis.VertexCount;
        int[] sourceSet = sources is { IsSome: true, Case: Seq<int> values } ? [.. values.AsIterable()] : [];
        if (n == 0 || (sources.IsSome && sourceSet.Length == 0) || sourceSet.Any(s => s < 0 || s >= n) || sourceSet.Distinct().Count() != sourceSet.Length)
            return Fin.Fail<SpectralDescriptor>(error: key.InvalidInput());
        if (!basis.Eigenvectors.ForAll(phi => phi.Count == n)) return Fin.Fail<SpectralDescriptor>(error: key.InvalidResult());
        double zeroBand = basis.ZeroBand;
        int zeroModeCount = basis.Eigenvalues.AsIterable().Count(lambda => lambda <= zeroBand);
        double firstNonZero = basis.Eigenvalues.AsIterable().FirstOrDefault(lambda => lambda > zeroBand);
        if (policy.ScaleNormalization.Equals(SpectralScaleNormalization.FirstNonZeroEigenvalue) && firstNonZero <= zeroBand) return Fin.Fail<SpectralDescriptor>(error: key.InvalidResult());
        int[] eigenIndices = [.. Enumerable.Range(start: 0, count: basis.Eigenvalues.Count)
            .Where(i => policy.ZeroModePolicy.Equals(SpectralZeroModePolicy.Keep) || basis.Eigenvalues[index: i] > zeroBand)
            .Take(policy.CropCount.Map(static count => count.Value).IfNone(basis.Eigenvalues.Count))];
        if (eigenIndices.Length == 0) return Fin.Fail<SpectralDescriptor>(error: key.InvalidInput());
        bool scaleNormalized = policy.ScaleNormalization.Equals(SpectralScaleNormalization.FirstNonZeroEigenvalue);
        double[] scaledEigenvalues = [.. eigenIndices.Select(k => scaleNormalized ? basis.Eigenvalues[index: k] / firstNonZero : basis.Eigenvalues[index: k])];
        (double[] weights, Option<SpectralWaveReceipt> wave) = WeightsOf(filter: filter, eigenvalues: scaledEigenvalues, firstNonZero: firstNonZero, zeroBand: scaleNormalized ? zeroBand / firstNonZero : zeroBand, zeroModeCount: zeroModeCount, croppedCount: eigenIndices.Length);
        if (weights.Any(static weight => !double.IsFinite(weight)) || (filter is SpectralFilter.WaveCase && wave.IsNone)) return Fin.Fail<SpectralDescriptor>(key.InvalidResult());
        bool isPairwise = sourceSet.Length > 0;
        double normFactor = isPairwise ? 1.0 / sourceSet.Length : 1.0;
        double[] result = new double[n];
        for (int ki = 0; ki < eigenIndices.Length; ki++) {
            int k = eigenIndices[ki];
            double w = weights[ki];
            Arr<double> phi = basis.Eigenvectors[index: k];
            if (isPairwise)
                for (int v = 0; v < n; v++) {
                    double phiV = phi[index: v];
                    for (int s = 0; s < sourceSet.Length; s++) {
                        double delta = phiV - phi[index: sourceSet[s]];
                        result[v] += w * delta * delta;
                    }
                }
            if (!isPairwise)
                for (int v = 0; v < n; v++) result[v] += w * phi[index: v] * phi[index: v];
        }
        if (isPairwise) for (int v = 0; v < n; v++) result[v] = Math.Sqrt(d: Math.Max(val1: 0.0, val2: result[v] * normFactor));
        return NormalizeValues(values: result, policy: policy, key: key).Map(values => new SpectralDescriptor(Values: new Arr<double>(values), Receipt: new SpectralDescriptorReceipt(Filter: filter, VertexCount: n, EigenpairCount: basis.Eigenvalues.Count, SourceCount: sourceSet.Length, ComparisonReady: !policy.IsRaw || wave.IsSome, Pairwise: isPairwise, EnergyNormalized: !policy.EnergyNormalization.Equals(SpectralEnergyNormalization.Raw), ScaleNormalized: !policy.ScaleNormalization.Equals(SpectralScaleNormalization.Raw), Policy: policy, ZeroModeCount: zeroModeCount, CroppedEigenpairCount: eigenIndices.Length, Wave: wave)));
    }
    internal static Fin<SpectralDescriptor> NormalizeDescriptor(SpectralDescriptor descriptor, SpectralDescriptorPolicy policy, Op key) =>
        from valid in guard(descriptor.IsValid, key.InvalidInput()).ToFin()
        from activePolicy in SpectralDescriptorPolicy.Admit(policy: policy, key: key)
        from _ in activePolicy.IsValueOnly
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(key.Unsupported(geometryType: typeof(SpectralDescriptor), outputType: typeof(SpectralDescriptorPolicy)))
        from values in NormalizeValues(values: [.. descriptor.Values.AsIterable()], policy: activePolicy, key: key)
        // Energy is the ONLY re-normalizable axis: merge it into the evaluation policy so the receipt keeps its scale/zero-mode/crop provenance.
        let mergedPolicy = descriptor.Receipt.Policy with { EnergyNormalization = activePolicy.EnergyNormalization }
        let receipt = descriptor.Receipt with { ComparisonReady = !mergedPolicy.IsRaw || descriptor.Receipt.Wave.IsSome, EnergyNormalized = !activePolicy.EnergyNormalization.Equals(SpectralEnergyNormalization.Raw), Policy = mergedPolicy }
        select new SpectralDescriptor(Values: new Arr<double>(values), Receipt: receipt);
    internal static Fin<SpectralRanking> RankDescriptors(SpectralDescriptor query, Seq<SpectralDescriptor> candidates, SpectralRankingPolicy policy, Op key) =>
        !policy.IsValid || !query.IsValid || candidates.IsEmpty || !candidates.ForAll(static candidate => candidate.IsValid)
            ? Fin.Fail<SpectralRanking>(key.InvalidInput())
            : from normalizedQuery in NormalizeForRanking(descriptor: query, policy: policy.Descriptor, key: key)
              from normalizedCandidates in candidates.TraverseM(candidate => NormalizeForRanking(descriptor: candidate, policy: policy.Descriptor, key: key)).As()
              from ranks in RankNormalized(query: normalizedQuery, candidates: normalizedCandidates, policy: policy, key: key)
              select new SpectralRanking(Query: normalizedQuery, Items: ranks, Policy: policy);
    private static Fin<SpectralDescriptor> NormalizeForRanking(SpectralDescriptor descriptor, SpectralDescriptorPolicy policy, Op key) =>
        descriptor.Receipt.ComparisonReady && SamePolicy(left: descriptor.Receipt.Policy, right: policy)
            ? Fin.Succ(descriptor)
            : NormalizeDescriptor(descriptor: descriptor, policy: policy, key: key);
    private static bool SamePolicy(SpectralDescriptorPolicy left, SpectralDescriptorPolicy right) =>
        left.ScaleNormalization.Equals(right.ScaleNormalization)
        && left.EnergyNormalization.Equals(right.EnergyNormalization)
        && left.ZeroModePolicy.Equals(right.ZeroModePolicy)
        && left.CropCount.Match(
            Some: l => right.CropCount.Match(Some: r => l.Value == r.Value, None: static () => false),
            None: () => right.CropCount.IsNone);
    // zeroBand arrives in the SAME units as the eigenvalue array (scaled by 1/firstNonZero when the policy scale-normalizes), so zero-mode classification is one law across raw and scaled spectra.
    private static (double[] Weights, Option<SpectralWaveReceipt> Wave) WeightsOf(SpectralFilter filter, double[] eigenvalues, double firstNonZero, double zeroBand, int zeroModeCount, int croppedCount) =>
        filter is SpectralFilter.WaveCase wave
            ? WaveWeightsOf(wave: wave, eigenvalues: eigenvalues, firstNonZero: firstNonZero, zeroBand: zeroBand, zeroModeCount: zeroModeCount, croppedCount: croppedCount)
            : ([.. eigenvalues.Select(filter.Weight)], Option<SpectralWaveReceipt>.None);
    private static (double[] Weights, Option<SpectralWaveReceipt> Wave) WaveWeightsOf(SpectralFilter.WaveCase wave, double[] eigenvalues, double firstNonZero, double zeroBand, int zeroModeCount, int croppedCount) {
        double[] raw = [.. eigenvalues.Select(wave.Weight)];
        double sum = raw.Sum();
        if (!double.IsFinite(sum) || sum <= EpsilonPolicy.SqrtEpsilon) return (raw, Option<SpectralWaveReceipt>.None);
        double[] normalized = [.. raw.Select(weight => weight / sum)];
        double[] positiveLogs = [.. eigenvalues.Where(lambda => lambda > zeroBand).Select(static lambda => Math.Log(d: lambda))];
        SpectralWaveReceipt receipt = new(
            Energy: wave.Energy.Value,
            Bandwidth: Math.Max(val1: wave.Bandwidth.Value, val2: WaveBandwidthFloor),
            FirstNonZeroScale: firstNonZero > 0.0 ? Some(firstNonZero) : Option<double>.None,
            ZeroModeCount: zeroModeCount,
            CroppedEigenpairCount: croppedCount,
            NonZeroEigenpairCount: positiveLogs.Length,
            RawWeightSum: sum,
            NormalizedWeightSum: normalized.Sum(),
            MinLogEigenvalue: positiveLogs.Length == 0 ? Option<double>.None : Some(positiveLogs.Min()),
            MaxLogEigenvalue: positiveLogs.Length == 0 ? Option<double>.None : Some(positiveLogs.Max()),
            WksNormalized: true);
        return (normalized, receipt.IsValid ? Some(receipt) : Option<SpectralWaveReceipt>.None);
    }
    private static Fin<Seq<SpectralRank>> RankNormalized(SpectralDescriptor query, Seq<SpectralDescriptor> candidates, SpectralRankingPolicy policy, Op key) {
        int valueCount = query.Values.Count;
        if (valueCount <= 0 || candidates.Exists(candidate => candidate.Values.Count != valueCount)) return Fin.Fail<Seq<SpectralRank>>(key.InvalidInput());
        double[] queryValues = [.. query.Values.AsIterable()];
        SpectralRank[] ranks = [.. candidates.AsIterable().Select((candidate, index) => new SpectralRank(Index: index, Distance: policy.Distance.Compute(a: queryValues, b: [.. candidate.Values.AsIterable()]), Descriptor: candidate)).OrderBy(static rank => rank.Distance).ThenBy(static rank => rank.Index)];
        return ranks.All(static rank => double.IsFinite(rank.Distance)) ? Fin.Succ(toSeq(ranks)) : Fin.Fail<Seq<SpectralRank>>(key.InvalidResult());
    }
    private static Fin<double[]> NormalizeValues(double[] values, SpectralDescriptorPolicy policy, Op key) {
        if (!TensorPrimitives.IsFiniteAll<double>(values.AsSpan())) return Fin.Fail<double[]>(key.InvalidResult());
        if (policy.EnergyNormalization.Equals(SpectralEnergyNormalization.Raw)) return Fin.Succ(values);
        double[] result = [.. values];
        if (policy.EnergyNormalization.Equals(SpectralEnergyNormalization.UnitL1) || policy.EnergyNormalization.Equals(SpectralEnergyNormalization.UnitL2))
            return (policy.EnergyNormalization.Equals(SpectralEnergyNormalization.UnitL1) ? TensorPrimitives.SumOfMagnitudes<double>(result) : TensorPrimitives.Norm<double>(result)) switch {
                double scale when scale > EpsilonPolicy.SqrtEpsilon => Fin.Succ<double[]>([.. result.Select(value => value / scale)]),
                _ => Fin.Fail<double[]>(key.InvalidResult()),
            };
        double mean = result.Average();
        double variance = result.Sum(value => (value - mean) * (value - mean)) / result.Length;
        double sigma = Math.Sqrt(d: variance);
        return sigma > EpsilonPolicy.SqrtEpsilon ? Fin.Succ<double[]>([.. result.Select(value => (value - mean) / sigma)]) : Fin.Fail<double[]>(key.InvalidResult());
    }
}
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
