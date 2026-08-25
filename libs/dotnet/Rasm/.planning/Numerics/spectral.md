# [RASM_NUMERICS_SPECTRAL]

`Rasm.Numerics` spectral owns the mesh-free discrete-exterior-calculus carrier layer and the spectral filter/descriptor algebra with zero mesh coupling, so the pure math floor stands independent of any `Mesh` and every eigenvalue-driven consumer meets one transfer-function and descriptor surface.

`DiscreteCalculus` is the frozen cross-package seam name the `Rasm.Compute` adjoint surface binds; `Meshing/dec` constructs the DEC carriers and receipts this page only declares, and `Meshing/mesh` declares `SignpostTransportReceipt`, carried here on the `Domain/validation` `Evidence` probe receipt, so each DDG receipt has one declaration site. `SpectralFilter` weights eigenvalues alone, the `[SmartEnum<int>]` policy vocabularies drive filter, normalization, and distance selection, and every consumer reads carriers and descriptors from this floor without touching a `Mesh`. Transform-domain machinery — the arena, the taper roster, the tap fold — is `Numerics/transform#SPECTRAL`'s; this page weights an already-solved spectrum and mints no transform.

## [01]-[INDEX]

- [02]-[FILTER_ALGEBRA]: `[SmartEnum<int>]` policy vocabularies and `SpectralFilter`, the closed transfer-function `[Union]` with its eigenvalue weight law and partial-monoid `Compose`.
- [03]-[DEC_CARRIERS]: `DiscreteCalculus` the frozen adjoint seam, `SpectralBasis`, and the assembly/harmonic receipt family `Meshing/dec` mints and `Rasm.Compute` consumes.
- [04]-[DESCRIPTOR_ALGEBRA]: descriptor policy, receipt, and carrier family and `SpectralKernel` filtered-signature evaluation, normalization, and ranking.

## [02]-[FILTER_ALGEBRA]

- Owner: `SpectralAssemblyKind`, `SpectralScaleNormalization`, `SpectralEnergyNormalization`, `SpectralZeroModePolicy`, and `SpectralDistanceKind` are the `[SmartEnum<int>]` policy vocabularies, the distance row carrying its `[UseDelegateFromConstructor]` compute column over `MathNet.Numerics.Distance` and the energy row its `Rescale` column over `TensorPrimitives`; `SpectralFilter` is the closed `[Union]` whose `Weight(eigenvalue)` is the spectral transfer function and whose `Compose` is a partial monoid — composable pairs fuse, `Identity` is the unit, every other pair is `None` by law.
- Cases: `ExponentialCase(rate)`, `WaveCase(energy, bandwidth)`, `PowerCase(exponent)`, `IdentityCase` — heat, diffusion, and amplification are ONE exponential family, apart by a literal and a sign, so `Diffusion(t)` IS `Heat(2t)` and the three fuse under `Compose` where three sibling cases each answered `None`.
- Entry: the `SpectralFilter.Heat`/`Diffusion`/`Amplify` factories take their magnitudes as `PositiveMagnitude` and land as canonical `Exponential` mints, `Wave` carries its own pair, and `Power` takes the signed exponent whole, so a filter in hand is admitted; `Biharmonic` (λ^−2) and `CommuteTime` (λ^−1) are the same canonical-mint precedent on the power leg; `ApplyDetailed(basis, sources, key, policy)` is the ONE evaluation entry, absence of a policy riding the carrier rather than an arity twin.
- Auto: `Compose` dispatches through the GENERATED `Switch` on the left operand, so a new case is a compile break rather than a silent fall to `None`; `Weight` carries `[MethodImpl(AggressiveInlining)]` for the descriptor kernel's per-eigenpair-per-vertex hot loop; the wave weight floors its bandwidth at `EpsilonPolicy.ZeroTolerance` — the anchor itself, resolved in one hop rather than through a rename shell a sibling type reached across for.
- Law: `Compose`'s NAMED LOSS is right-side exhaustiveness — the generated `Switch` closes the left family and each arm's own `is` test decides its one composable partner, because a total pair fold over four cases is sixteen arms stating one law twelve times.
- Receipt: none at this layer — the filter is policy, and evidence lands on the [04] descriptor receipts.
- Packages: MathNet.Numerics (`Distance` — the twelve metric rows, split across its `Vector<T>` and `double[]` carriers; `CreateVector.DenseOfArray` the lift the `Lifted` operand pays once), System.Numerics.Tensors (`TensorPrimitives` — the energy rescale and the span reductions), `Rasm.Numerics` `atoms.md` (`EpsilonPolicy`, `PositiveMagnitude`), Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new transfer function whose weight is `exp(rate*λ)` is a MINT, not a case; a genuinely new shape is one case, one `Weight` arm, and one `Compose` arm, the kernel and every consumer untouched; a new normalization is one row with its `Rescale` column and a new distance one row with its compute column.
- Boundary: filters weight eigenvalues alone — never a mesh, a basis matrix, or a vertex — so the one filter value drives `Meshing/dec` heat scaffolds, `Processing/segment` descriptors, and `Spatial/fields` spectral-distance cases from this floor.

```csharp signature
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using System;
using System.Numerics.Tensors;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LanguageExt;
using Rasm.Domain;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Numerics;

// --- [TYPES] ---------------------------------------------------------------------------
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
    public static readonly SpectralEnergyNormalization Raw = new(key: 0, rescale: static values => Some(values));
    public static readonly SpectralEnergyNormalization UnitL1 = new(key: 1, rescale: static values => Scaled(values: values, scale: TensorPrimitives.SumOfMagnitudes<double>(values)));
    public static readonly SpectralEnergyNormalization UnitL2 = new(key: 2, rescale: static values => Scaled(values: values, scale: TensorPrimitives.Norm<double>(values)));
    public static readonly SpectralEnergyNormalization ZScore = new(key: 3, rescale: static values => Centered(values: values));

    [UseDelegateFromConstructor] internal partial Option<double[]> Rescale(double[] values);

    private static Option<double[]> Scaled(double[] values, double scale) {
        if (scale <= EpsilonPolicy.SqrtEpsilon) { return Option<double[]>.None; }
        double[] destination = new double[values.Length];
        TensorPrimitives.Divide<double>(values, scale, destination);
        return Some(destination);
    }
    private static Option<double[]> Centered(double[] values) {
        double sigma = TensorPrimitives.StdDev<double>(values);
        if (sigma <= EpsilonPolicy.SqrtEpsilon) { return Option<double[]>.None; }
        double[] destination = new double[values.Length];
        TensorPrimitives.Subtract<double>(values, TensorPrimitives.Average<double>(values), destination);
        TensorPrimitives.Divide<double>(destination, sigma, destination);
        return Some(destination);
    }
}

[SmartEnum<int>]
public sealed partial class SpectralZeroModePolicy {
    public static readonly SpectralZeroModePolicy Keep = new(key: 0);
    public static readonly SpectralZeroModePolicy Drop = new(key: 1);
}

[SmartEnum<int>]
public sealed partial class SpectralDistanceKind {
    public static readonly SpectralDistanceKind Euclidean = new(key: 0, compute: static (a, b) => MathNet.Numerics.Distance.Euclidean(a.Dense, b.Dense));
    public static readonly SpectralDistanceKind Manhattan = new(key: 1, compute: static (a, b) => MathNet.Numerics.Distance.Manhattan(a.Dense, b.Dense));
    public static readonly SpectralDistanceKind Cosine = new(key: 2, compute: static (a, b) => MathNet.Numerics.Distance.Cosine(a.Raw, b.Raw));
    public static readonly SpectralDistanceKind Chebyshev = new(key: 3, compute: static (a, b) => MathNet.Numerics.Distance.Chebyshev(a.Dense, b.Dense));
    public static readonly SpectralDistanceKind Canberra = new(key: 4, compute: static (a, b) => MathNet.Numerics.Distance.Canberra(a.Raw, b.Raw));
    public static readonly SpectralDistanceKind Minkowski3 = new(key: 5, compute: static (a, b) => MathNet.Numerics.Distance.Minkowski(3.0, a.Dense, b.Dense));
    public static readonly SpectralDistanceKind Hamming = new(key: 6, compute: static (a, b) => MathNet.Numerics.Distance.Hamming(a.Raw, b.Raw));
    public static readonly SpectralDistanceKind Jaccard = new(key: 7, compute: static (a, b) => MathNet.Numerics.Distance.Jaccard(a.Raw, b.Raw));
    public static readonly SpectralDistanceKind MeanAbsolute = new(key: 8, compute: static (a, b) => MathNet.Numerics.Distance.MAE(a.Dense, b.Dense));
    public static readonly SpectralDistanceKind MeanSquared = new(key: 9, compute: static (a, b) => MathNet.Numerics.Distance.MSE(a.Dense, b.Dense));
    public static readonly SpectralDistanceKind SumAbsolute = new(key: 10, compute: static (a, b) => MathNet.Numerics.Distance.SAD(a.Dense, b.Dense));
    public static readonly SpectralDistanceKind SumSquared = new(key: 11, compute: static (a, b) => MathNet.Numerics.Distance.SSD(a.Dense, b.Dense));

    [UseDelegateFromConstructor] internal partial double Compute(Lifted a, Lifted b);
}

[StructLayout(LayoutKind.Auto)]
internal readonly record struct Lifted(double[] Raw, MathNet.Numerics.LinearAlgebra.Vector<double> Dense) {
    internal static Lifted Of(double[] values) => new(Raw: values, Dense: MathNet.Numerics.LinearAlgebra.CreateVector.DenseOfArray(values));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SpectralFilter {
    public sealed record ExponentialCase(double Rate) : SpectralFilter;
    public sealed record WaveCase(PositiveMagnitude Energy, PositiveMagnitude Bandwidth) : SpectralFilter;
    public sealed record PowerCase(double Exponent) : SpectralFilter;
    public sealed record IdentityCase : SpectralFilter;
    private SpectralFilter() { }
    public static SpectralFilter Exponential(double rate) => rate == 0.0 ? Identity : new ExponentialCase(Rate: rate);
    public static SpectralFilter Heat(PositiveMagnitude time) => Exponential(rate: -time.Value);
    public static SpectralFilter Wave(PositiveMagnitude energy, PositiveMagnitude bandwidth) => new WaveCase(Energy: energy, Bandwidth: bandwidth);
    public static SpectralFilter Diffusion(PositiveMagnitude time) => Exponential(rate: -2.0 * time.Value);
    public static SpectralFilter Power(double exponent) => exponent == 0.0 ? Identity : new PowerCase(Exponent: exponent);
    public static SpectralFilter Amplify(PositiveMagnitude rate) => Exponential(rate: rate.Value);
    public static SpectralFilter Biharmonic => Power(exponent: -2.0);
    public static SpectralFilter CommuteTime => Power(exponent: -1.0);
    public static SpectralFilter Identity => new IdentityCase();
    public Option<SpectralFilter> Compose(SpectralFilter other) =>
        other is IdentityCase ? Some(this) : Switch(
            state: other,
            exponentialCase: static (o, c) => o is ExponentialCase b ? Some(Exponential(rate: c.Rate + b.Rate)) : Option<SpectralFilter>.None,
            waveCase: static (_, _) => Option<SpectralFilter>.None,
            powerCase: static (o, c) => o is PowerCase b ? Some(Power(exponent: c.Exponent + b.Exponent)) : Option<SpectralFilter>.None,
            identityCase: static (o, _) => Some(o));
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double Weight(double eigenvalue) => Switch(
        state: eigenvalue,
        exponentialCase: static (lambda, c) => Math.Exp(d: c.Rate * lambda),
        waveCase: static (lambda, c) => lambda < EpsilonPolicy.SqrtEpsilon
            ? 0.0
            : ((Math.Log(d: c.Energy.Value) - Math.Log(d: lambda)) / Math.Max(val1: c.Bandwidth.Value, val2: EpsilonPolicy.ZeroTolerance)) switch {
                double ratio => Math.Exp(d: -0.5 * ratio * ratio),
            },
        powerCase: static (lambda, c) => lambda > EpsilonPolicy.SqrtEpsilon ? Math.Pow(x: lambda, y: c.Exponent) : 0.0,
        identityCase: static (_, _) => 1.0);
    internal Fin<SpectralDescriptor> ApplyDetailed(SpectralBasis basis, Option<Seq<int>> sources, Op key, Option<SpectralDescriptorPolicy> policy = default) =>
        from _ in guard(basis.IsValid, key.InvalidInput()).ToFin()
        from active in SpectralDescriptorPolicy.Admit(policy: policy.IfNone(noneValue: SpectralDescriptorPolicy.Raw), key: key)
        from descriptor in SpectralKernel.EvaluateFilteredDetailed(basis: basis, sources: sources, filter: this, policy: active, key: key)
        select descriptor;
}
```

## [03]-[DEC_CARRIERS]

- Owner: `DiscreteCalculus` is the DEC operator bundle — incidence and curl operators, the diagonal Hodge stars, its `SpectralAssemblyReceipt`, the `Transport` probe column (`Domain/validation` `Evidence<SignpostTransportReceipt>` — a signpost probe that refused keeps its cause distinct from one never run), and the optional `Harmonic` slot — with `Project<TOut>` routing the evidence family through typed `ProjectionRow` rows. `SpectralBasis` is the eigenpair carrier with `Truncate(k)` and the ONE scale-relative `ZeroBand` (`SqrtEpsilon` × spectral radius) the descriptor kernel reuses to classify zero modes, so one threshold declaration carries every consumer with zero drift. `SpectralAssemblyReceipt`, `HarmonicOneFormReceipt`, and `HarmonicOneFormBasis` are the assembly and harmonic evidence, their semantic gates scale-relative against `max(1, spectralRadius)` rather than any bare absolute.
- Entry: carriers are constructed by `Meshing/dec` (assembly) and `Meshing/mesh` (caching); this page owns their shape, validity law, and projection, and consumers — the `Rasm.Compute` adjoint surface, `Processing/geodesics`, `Processing/segment`, `Spatial/fields` — read them from here.
- Auto: `DiscreteCalculus.IsValid` cross-couples the stars to the operator shapes, requires strictly positive vertex and face stars, and admits edge stars down to a scale-relative negative band, where near-degenerate intrinsic cotan weights legitimately dip below zero within roundoff of the `Star1` scale.
- Law: `HarmonicOneFormReceipt` is the ONE owner of the harmonic-decomposition measurements — expected dimension, basis count, edge count, rank, nullity, positive-star-1 count, the closed and co-closed residuals, and the star-1 orthonormality residual. A composing receipt CARRIES it (`Meshing/dec`'s `HodgeDecompositionReceipt` nests it as `Option<HarmonicOneFormReceipt>`) and re-declares no slot of it, because a second copy of one measurement is two independently-filled values with no claim tying them and either may be the wrong one.
- Receipt: all three receipts fold `ValidityClaim.All` with `IValidityEvidence` registration; the semantic gates carry the harmonic dimension law, the `Rank + Nullity == EdgeCount` partition, and the residual-tolerance ladder. Every field is read by a conjunct, so a slot no gate consumes is false evidence and never lands; a measure one assembly arm cannot take rides `Option` and reads as an absent claim, never a zero the gate mistakes for a measurement — the symmetry residual and its tolerance are ONE optional pair for exactly that reason, since a zeroed pair passed `0 <= 0` for an arm that measured neither.
- Packages: `matrix.md` owners (`SparseMatrix`, `EigenSolveReceipt`), System.Numerics.Tensors (`TensorPrimitives.MaxMagnitude`/`Min`/`IsFiniteAll` — the star and eigenvalue folds), LanguageExt.Core, Rasm.Domain (`IValidityEvidence` + `ValidityClaim`, `Op`).
- Growth: a new DEC operator is one field, one validity coupling, and one `ProjectionRow`; a new assembly witness is one receipt field.
- Boundary: `DiscreteCalculus` is the `Rasm.Compute` adjoint seam — Compute binds the operator columns and the validity fold, never `Transport`, so the probe column stays kernel-grain; `SignpostTransportReceipt` is declared by `Meshing/mesh`, the intrinsic-triangulation owner, and carried here only as probe evidence, so each DDG receipt has exactly one declaration site with this page owning the mesh-free members. DECLARATION and CONSTRUCTION are the split with `Meshing/dec`: no member here emits `D0`, `D1`, or a star, and `dec`'s assembly re-owns no algebra declared here.

```csharp signature
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using System;
using System.Numerics.Tensors;
using System.Runtime.InteropServices;
using LanguageExt;
using Rasm.Domain;
using Rasm.Meshing;
using static LanguageExt.Prelude;

namespace Rasm.Numerics;

// --- [MODELS] --------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct SpectralAssemblyReceipt(
    int VertexCount, int EdgeCount, int FaceCount, int AdmittedFaceCount, int SkippedDegenerateFaces, int SkippedMissingEdges,
    bool FlippedIntrinsicLifted, int MatrixRows, int MatrixCols, int NonZeros,
    int PositiveStar0Count, int PositiveStar1Count, int PositiveStar2Count,
    double BoundaryCompositionResidual, Option<int> Genus, int HarmonicDimension, SpectralAssemblyKind Kind,
    Option<int> BoundaryEdgeCount = default, int BoundaryComponentCount = 0, Option<int> NonManifoldEdgeCount = default,
    Option<int> EulerCharacteristic = default, int ComponentCount = 1, Option<int> PositiveMassCount = default,
    Option<(double Residual, double Tolerance)> Symmetry = default, Option<int> FactorNonZeros = default,
    double BoundaryCompositionTolerance = 0.0) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        VertexCount >= 0 && EdgeCount >= 0 && FaceCount >= 0 && MatrixRows >= 0 && MatrixCols >= 0 && NonZeros >= 0,
        ValidityClaim.Nonnegative(value: BoundaryCompositionResidual),
        AdmittedFaceCount + SkippedDegenerateFaces + SkippedMissingEdges <= FaceCount,
        Symmetry.Map(static measured => measured.Residual >= 0.0 && measured.Residual <= measured.Tolerance).IfNone(noneValue: true),
        BoundaryCompositionTolerance <= 0.0 || BoundaryCompositionResidual <= BoundaryCompositionTolerance,
        FactorNonZeros.Map(static value => value > 0).IfNone(noneValue: true),
        !FlippedIntrinsicLifted || Kind.Equals(SpectralAssemblyKind.EdgeConnection),
        BoundaryEdgeCount.Map(count =>
            count >= 0 && count <= EdgeCount && (count == 0) == (BoundaryComponentCount == 0) && count >= 3 * BoundaryComponentCount).IfNone(noneValue: true),
        NonManifoldEdgeCount.Map(count => count >= 0 && count <= EdgeCount).IfNone(noneValue: true),
        EulerCharacteristic.Map(chi =>
            chi == VertexCount - EdgeCount + FaceCount
            && NonManifoldEdgeCount.IfNone(noneValue: 0) == 0
            && Genus.Map(genus => chi == 2 - (2 * genus) - BoundaryComponentCount).IfNone(noneValue: true)).IfNone(noneValue: true),
        Kind.Equals(SpectralAssemblyKind.EdgeConnection)
            ? ComponentCount == 2 && MatrixRows == EdgeCount * ComponentCount && MatrixCols == MatrixRows
              && PositiveMassCount.Map(count => count >= 0 && count <= EdgeCount).IfNone(noneValue: true)
            : ComponentCount == 1 && PositiveStar0Count <= VertexCount && PositiveStar1Count <= EdgeCount && PositiveStar2Count <= FaceCount
              && (Genus is { IsSome: true, Case: int genus }
                  ? genus >= 0 && HarmonicDimension == (2 * genus) + Math.Max(0, BoundaryComponentCount - 1)
                  : HarmonicDimension == 0));
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct HarmonicOneFormReceipt(
    Option<int> Genus, int ExpectedDimension, int ConstraintRows, int EdgeCount, int Rank, int Nullity, int BasisCount,
    double SvdTolerance, double EpsRank, double SpectralRadius, double NullspaceThreshold,
    double MinNullEigenvalue, double MaxNullEigenvalue, double MaxClosedResidual, double MaxCoClosedResidual,
    double Star1OrthonormalResidual, int PositiveStar1Count, EigenSolveReceipt<double, Arr<double>> Eigen,
    int BoundaryComponentCount = 0) : IValidityEvidence {
    private const double ResidualSlack = 1.0e3;
    public bool IsValid {
        get {
            int expected = ExpectedDimension;
            int boundaryComponentCount = BoundaryComponentCount;
            double residualTolerance = Math.Max(val1: SvdTolerance, val2: EpsilonPolicy.SqrtEpsilon * Math.Max(val1: 1.0, val2: SpectralRadius)) * ResidualSlack;
            return ValidityClaim.All(
                EdgeCount >= 0 && Rank >= 0 && Nullity >= 0 && BasisCount >= 0 && ConstraintRows >= 0,
                Rank + Nullity == EdgeCount,
                ValidityClaim.CountExactly(count: BasisCount, expected: expected),
                Nullity >= expected,
                PositiveStar1Count <= EdgeCount,
                Genus.Map(genus => expected == (2 * genus) + Math.Max(0, boundaryComponentCount - 1)).IfNone(expected == 0),
                ValidityClaim.Positive(value: SvdTolerance),
                ValidityClaim.Positive(value: EpsRank),
                Math.Abs(value: SvdTolerance - NullspaceThreshold) <= EpsilonPolicy.SqrtEpsilon * Math.Max(val1: 1.0, val2: NullspaceThreshold),
                NullspaceThreshold <= (EpsRank * Math.Max(val1: 1.0, val2: SpectralRadius)) + EpsilonPolicy.SqrtEpsilon,
                MinNullEigenvalue >= -EpsilonPolicy.SqrtEpsilon,
                MaxNullEigenvalue >= MinNullEigenvalue - EpsilonPolicy.SqrtEpsilon,
                MaxClosedResidual <= residualTolerance,
                MaxCoClosedResidual <= residualTolerance,
                Star1OrthonormalResidual <= residualTolerance,
                ValidityClaim.Evidence(evidence: Some(Eigen)));
        }
    }
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct HarmonicOneFormBasis(Arr<Arr<double>> Forms, HarmonicOneFormReceipt Receipt) : IValidityEvidence {
    public bool IsValid {
        get {
            HarmonicOneFormReceipt receipt = Receipt;
            return ValidityClaim.All(
                ValidityClaim.Evidence(evidence: Some(receipt)),
                ValidityClaim.CountExactly(count: Forms.Count, expected: receipt.BasisCount),
                Forms.ForAll(form => form.Count == receipt.EdgeCount && form.ForAll(double.IsFinite)));
        }
    }
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct DiscreteCalculus(SparseMatrix D0, SparseMatrix D1, Arr<double> Star0, Arr<double> Star1, Arr<double> Star2, SpectralAssemblyReceipt Receipt, Evidence<SignpostTransportReceipt> Transport, Option<HarmonicOneFormBasis> Harmonic = default) : IValidityEvidence {
    public bool IsValid {
        get {
            ReadOnlySpan<double> star0 = Star0.AsSpan(), star1 = Star1.AsSpan(), star2 = Star2.AsSpan();
            double star1Scale = star1.IsEmpty ? 0.0 : Math.Abs(value: TensorPrimitives.MaxMagnitude<double>(star1));
            return ValidityClaim.All(
                D0.IsValid && D1.IsValid,
                ValidityClaim.Evidence(evidence: Some(Receipt)),
                Star0.Count == D0.Cols.Value && Star1.Count == D0.Rows.Value && Star2.Count == D1.Rows.Value,
                TensorPrimitives.IsFiniteAll<double>(star0) && (star0.IsEmpty || TensorPrimitives.Min<double>(star0) > 0.0),
                TensorPrimitives.IsFiniteAll<double>(star1)
                    && (star1.IsEmpty || TensorPrimitives.Min<double>(star1) >= -(EpsilonPolicy.SqrtEpsilon * Math.Max(1.0, star1Scale))),
                TensorPrimitives.IsFiniteAll<double>(star2) && (star2.IsEmpty || TensorPrimitives.Min<double>(star2) > 0.0),
                Transport is { } probe && ValidityClaim.Evidence(evidence: probe.Value()),
                ValidityClaim.Evidence(evidence: Harmonic));
        }
    }
    internal Fin<TOut> Project<TOut>(Op key) {
        DiscreteCalculus self = this;
        return AtomProjection.Rows<DiscreteCalculus, TOut>(self: self, key: key,
            ProjectionRow.Of<SpectralAssemblyReceipt>(() => Fin.Succ(self.Receipt)),
            ProjectionRow.Of<SignpostTransportReceipt>(() => self.Transport.Switch(
                measured: static row => Fin.Succ(row.Value),
                refused: static row => Fin.Fail<SignpostTransportReceipt>(row.Cause),
                absent: _ => Fin.Fail<SignpostTransportReceipt>(key.InvalidResult()))),
            ProjectionRow.Of<HarmonicOneFormBasis>(() => self.Harmonic.ToFin(key.InvalidResult())),
            ProjectionRow.Of<HarmonicOneFormReceipt>(() => self.Harmonic.Map(static basis => basis.Receipt).ToFin(key.InvalidResult())));
    }
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct SpectralBasis(Arr<double> Eigenvalues, Arr<Arr<double>> Eigenvectors) : IValidityEvidence {
    internal int VertexCount => Eigenvectors.IsEmpty ? 0 : Eigenvectors[index: 0].Count;
    internal double SpectralRadius => Eigenvalues.IsEmpty ? 0.0 : Math.Abs(value: TensorPrimitives.MaxMagnitude<double>(Eigenvalues.AsSpan()));
    internal double ZeroBand => EpsilonPolicy.SqrtEpsilon * Math.Max(val1: SpectralRadius, val2: EpsilonPolicy.ZeroTolerance);
    public bool IsValid {
        get {
            int vertexCount = VertexCount;
            double zeroBand = ZeroBand;
            ReadOnlySpan<double> eigenvalues = Eigenvalues.AsSpan();
            return ValidityClaim.All(
                ValidityClaim.CountAtLeast(count: Eigenvalues.Count, floor: 1),
                ValidityClaim.CountExactly(count: Eigenvectors.Count, expected: Eigenvalues.Count),
                TensorPrimitives.IsFiniteAll<double>(eigenvalues) && TensorPrimitives.Min<double>(eigenvalues) >= -zeroBand,
                Eigenvectors.ForAll(phi => vertexCount > 0 && phi.Count == vertexCount && TensorPrimitives.IsFiniteAll<double>(phi.AsSpan())));
        }
    }
    public SpectralBasis Truncate(int k) =>
        k <= 0 || k >= Eigenvalues.Count ? this : new SpectralBasis(Eigenvalues: new Arr<double>([.. Eigenvalues.AsIterable().Take(k)]), Eigenvectors: new Arr<Arr<double>>([.. Eigenvectors.AsIterable().Take(k)]));
}
```

## [04]-[DESCRIPTOR_ALGEBRA]

- Owner: `SpectralDescriptorPolicy` is the normalization bundle (scale × energy × zero-mode × optional crop) with `Raw` the no-op row and `Admit` the gate; `SpectralWaveReceipt` and `SpectralDescriptorReceipt` carry the WKS and descriptor evidence; `SpectralDescriptor` is the values and receipt carrier with `Normalize(energy, key)` and `Rank(candidates, policy, key)`; `SpectralRankingPolicy`/`SpectralRank`/`SpectralRanking` carry ranking. `SpectralKernel` is the `internal static` evaluation owner — the dense-buffer filtered-signature kernel, energy normalization through the `SpectralEnergyNormalization` rescale column, and ranking off the `SpectralDistanceKind` compute column over one pre-lifted operand; the harmonic eps-rank default stays `Meshing/dec`'s, declared beside the construction that applies it.
- Entry: `filter.ApplyDetailed(basis, sources, key, policy)` is the evaluation entry; `descriptor.Normalize(energy, key)` and `descriptor.Rank(candidates, policy, key)` the post-processing entries, normalization taking the ONE axis it honours rather than a four-axis bundle it refuses three arms of — one descriptor pipeline, no sibling evaluate/compare surfaces.
- Auto: WKS weights normalize to unit sum with the full `SpectralWaveReceipt` minted inline, and the wave arm answers its OWN failure rather than the caller re-testing the case a line later; `ComparisonReady`, `Pairwise`, `EnergyNormalized`, and `ScaleNormalized` DERIVE from the receipt's own `Policy`, `Wave`, and `SourceCount`, so no stored mirror can disagree with the value it copied; `RankDescriptors` re-normalizes every candidate to the query policy before measuring.
- Law: ranking short-circuits on the first unusable candidate rather than accumulating — a ranking is a JOINT verdict over the whole candidate set, so a partial ordering is a wrong ordering and there is no per-candidate outcome for a `Validation` to carry.
- Exemption: `EvaluateFilteredDetailed` is a declared statement kernel — a dense per-eigenpair-per-vertex accumulation buffer, where a query fold churns allocations over `n · k` (`· |S|` pairwise) terms. The exemption covers ALLOCATION shape alone: every per-vertex arm rides `TensorPrimitives` over one pooled lane, and the accumulation walks `ki → s → v` so a source ordinate is read once per pair instead of once per vertex.
- Receipt: `SpectralWaveReceipt` and `SpectralDescriptorReceipt` fold `ValidityClaim.All` with semantic gates over the source, eigenpair, and vertex couplings and the WKS unit-sum, whose band scales with the summed term count rather than sitting at a bare absolute.
- Packages: MathNet.Numerics (`Distance` rows over the pre-lifted `Lifted` operand), System.Numerics.Tensors (`TensorPrimitives.Sum`/`Min`/`Max`/`MaxMagnitude`/`Subtract`/`Multiply`/`MultiplyAdd`/`Sqrt`/`Divide`/`IsFiniteAll` — every reduction, rescale, and accumulation on this cluster), CommunityToolkit.HighPerformance (`MemoryOwner<T>` the accumulation lane), LanguageExt.Core, Thinktecture.Runtime.Extensions.
- Growth: a new signature family is one filter case and policy rows, the kernel loop already generic over the weight function; a new distance is one `SpectralDistanceKind` row and a new energy normalization one `SpectralEnergyNormalization` row, the compute and rescale columns being the arms.
- Boundary: the kernel is mesh-free, seeing vertex COUNT as its only topology, so it serves tet, grid, and mesh bases identically, while mesh-side basis computation and caching (`SpectralBasisBundle`) are `Meshing/dec`'s.

```csharp signature
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics.Tensors;
using System.Runtime.InteropServices;
using CommunityToolkit.HighPerformance.Buffers;
using LanguageExt;
using Rasm.Domain;
using static LanguageExt.Prelude;

namespace Rasm.Numerics;

// --- [MODELS] --------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct SpectralDescriptorPolicy(SpectralScaleNormalization ScaleNormalization, SpectralEnergyNormalization EnergyNormalization, SpectralZeroModePolicy ZeroModePolicy, Option<Dimension> CropCount) : IValidityEvidence {
    public static SpectralDescriptorPolicy Raw => new(ScaleNormalization: SpectralScaleNormalization.Raw, EnergyNormalization: SpectralEnergyNormalization.Raw, ZeroModePolicy: SpectralZeroModePolicy.Keep, CropCount: None);
    public bool IsValid => ValidityClaim.All(
        ScaleNormalization is not null && EnergyNormalization is not null && ZeroModePolicy is not null,
        CropCount.Map(static count => count.Value > 0).IfNone(noneValue: true));
    internal bool IsRaw => Equals(Raw);
    internal static Fin<SpectralDescriptorPolicy> Admit(SpectralDescriptorPolicy policy, Op key) =>
        guard(policy.IsValid, key.InvalidInput()).ToFin().Map(_ => policy);
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct SpectralWaveReceipt(double Energy, double Bandwidth, Option<double> FirstNonZeroScale, int ZeroModeCount, int CroppedEigenpairCount, int NonZeroEigenpairCount, double RawWeightSum, double NormalizedWeightSum, Option<double> MinLogEigenvalue, Option<double> MaxLogEigenvalue) : IValidityEvidence {
    public bool IsValid {
        get {
            Option<double> maxLogEigenvalue = MaxLogEigenvalue;
            return ValidityClaim.All(
                ValidityClaim.Positive(value: Energy),
                ValidityClaim.Positive(value: Bandwidth),
                ZeroModeCount >= 0 && CroppedEigenpairCount >= NonZeroEigenpairCount && NonZeroEigenpairCount > 0,
                ValidityClaim.Positive(value: RawWeightSum),
                Math.Abs(value: NormalizedWeightSum - 1.0) <= EpsilonPolicy.SqrtEpsilon * Math.Max(val1: 1, val2: CroppedEigenpairCount),
                FirstNonZeroScale.Map(static first => first > 0.0).IfNone(noneValue: true),
                MinLogEigenvalue.Map(min => maxLogEigenvalue.Map(max => min <= max).IfNone(noneValue: true)).IfNone(noneValue: true));
        }
    }
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct SpectralDescriptorReceipt(SpectralFilter Filter, int VertexCount, int EigenpairCount, int SourceCount,
    SpectralDescriptorPolicy Policy, int ZeroModeCount = 0, int CroppedEigenpairCount = 0, Option<SpectralWaveReceipt> Wave = default) : IValidityEvidence {
    public bool Pairwise => SourceCount > 0;
    public bool EnergyNormalized => !Policy.EnergyNormalization.Equals(SpectralEnergyNormalization.Raw);
    public bool ScaleNormalized => !Policy.ScaleNormalization.Equals(SpectralScaleNormalization.Raw);
    public bool ComparisonReady => !Policy.IsRaw || Wave.IsSome;
    public bool IsValid => ValidityClaim.All(
        VertexCount > 0 && EigenpairCount > 0,
        CroppedEigenpairCount > 0 && CroppedEigenpairCount <= EigenpairCount,
        ZeroModeCount >= 0 && ZeroModeCount <= EigenpairCount,
        SourceCount >= 0 && SourceCount <= VertexCount,
        ValidityClaim.Evidence(evidence: Some(Policy)),
        ValidityClaim.Evidence(evidence: Wave));
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct SpectralDescriptor(Arr<double> Values, SpectralDescriptorReceipt Receipt) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Evidence(evidence: Some(Receipt)),
        ValidityClaim.CountExactly(count: Values.Count, expected: Receipt.VertexCount),
        Values.ForAll(double.IsFinite));
    public Fin<SpectralDescriptor> Normalize(SpectralEnergyNormalization energy, Op key) =>
        SpectralKernel.NormalizeDescriptor(descriptor: this, energy: energy, key: key);
    public Fin<SpectralRanking> Rank(Seq<SpectralDescriptor> candidates, SpectralRankingPolicy policy, Op key) =>
        SpectralKernel.RankDescriptors(query: this, candidates: candidates, policy: policy, key: key);
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct SpectralRankingPolicy(SpectralDescriptorPolicy Descriptor, SpectralDistanceKind Distance) : IValidityEvidence {
    public static SpectralRankingPolicy Default => new(Descriptor: SpectralDescriptorPolicy.Raw, Distance: SpectralDistanceKind.Euclidean);
    public bool IsValid => ValidityClaim.Evidence(evidence: Some(Descriptor));
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct SpectralRank(int Index, double Distance, SpectralDescriptor Descriptor);

[StructLayout(LayoutKind.Auto)]
public readonly record struct SpectralRanking(SpectralDescriptor Query, Seq<SpectralRank> Items, SpectralRankingPolicy Policy);

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class SpectralKernel {
    internal static Fin<SpectralDescriptor> EvaluateFilteredDetailed(SpectralBasis basis, Option<Seq<int>> sources, SpectralFilter filter, SpectralDescriptorPolicy policy, Op key) {
        int n = basis.VertexCount;
        int[] sourceSet = sources.Map(static values => values.AsIterable().ToArray()).IfNone([]);
        if (n == 0 || (sources.IsSome && sourceSet.Length == 0) || toSet(sourceSet).Count != sourceSet.Length
            || (sourceSet.Length > 0 && (TensorPrimitives.Min<int>(sourceSet) < 0 || TensorPrimitives.Max<int>(sourceSet) >= n))) {
            return Fin.Fail<SpectralDescriptor>(error: key.InvalidInput());
        }
        if (!basis.Eigenvectors.ForAll(phi => phi.Count == n)) { return Fin.Fail<SpectralDescriptor>(error: key.InvalidResult()); }
        double zeroBand = basis.ZeroBand;
        int zeroModeCount = basis.Eigenvalues.AsIterable().Count(lambda => lambda <= zeroBand);
        Option<double> firstNonZero = basis.Eigenvalues.Find(lambda => lambda > zeroBand);
        bool scaleNormalized = policy.ScaleNormalization.Equals(SpectralScaleNormalization.FirstNonZeroEigenvalue);
        if (scaleNormalized && firstNonZero.IsNone) { return Fin.Fail<SpectralDescriptor>(error: key.InvalidResult()); }
        double scale = firstNonZero.IfNone(1.0);
        int[] eigenIndices = [.. Enumerable.Range(start: 0, count: basis.Eigenvalues.Count)
            .Where(i => policy.ZeroModePolicy.Equals(SpectralZeroModePolicy.Keep) || basis.Eigenvalues[index: i] > zeroBand)
            .Take(policy.CropCount.Map(static count => count.Value).IfNone(basis.Eigenvalues.Count))];
        if (eigenIndices.Length == 0) { return Fin.Fail<SpectralDescriptor>(error: key.InvalidInput()); }
        double[] scaledEigenvalues = [.. eigenIndices.Select(k => scaleNormalized ? basis.Eigenvalues[index: k] / scale : basis.Eigenvalues[index: k])];
        return WeightsOf(filter: filter, eigenvalues: scaledEigenvalues, firstNonZero: firstNonZero,
                zeroBand: scaleNormalized ? zeroBand / scale : zeroBand, zeroModeCount: zeroModeCount, croppedCount: eigenIndices.Length, key: key)
            .Bind(weighted => Accumulated(basis: basis, eigenIndices: eigenIndices, weights: weighted.Weights, sourceSet: sourceSet, vertexCount: n, key: key)
                .Bind(values => NormalizeValues(values: values, policy: policy, key: key))
                .Map(values => new SpectralDescriptor(
                    Values: new Arr<double>(values),
                    Receipt: new SpectralDescriptorReceipt(Filter: filter, VertexCount: n, EigenpairCount: basis.Eigenvalues.Count, SourceCount: sourceSet.Length,
                        Policy: policy, ZeroModeCount: zeroModeCount, CroppedEigenpairCount: eigenIndices.Length, Wave: weighted.Wave))));
    }
    private static Fin<double[]> Accumulated(SpectralBasis basis, int[] eigenIndices, double[] weights, int[] sourceSet, int vertexCount, Op key) {
        double[] result = new double[vertexCount];
        using MemoryOwner<double> scratch = MemoryOwner<double>.Allocate(size: vertexCount);
        Span<double> lane = scratch.Span;
        for (int ki = 0; ki < eigenIndices.Length; ki++) {
            ReadOnlySpan<double> phi = basis.Eigenvectors[index: eigenIndices[ki]].AsSpan();
            double w = weights[ki];
            if (sourceSet.Length == 0) {
                TensorPrimitives.Multiply<double>(phi, phi, lane);
                TensorPrimitives.MultiplyAdd<double>(lane, w, result, result);
                continue;
            }
            for (int s = 0; s < sourceSet.Length; s++) {
                double phiSource = phi[sourceSet[s]];
                TensorPrimitives.Subtract<double>(phi, phiSource, lane);
                TensorPrimitives.Multiply<double>(lane, lane, lane);
                TensorPrimitives.MultiplyAdd<double>(lane, w, result, result);
            }
        }
        if (sourceSet.Length > 0) {
            TensorPrimitives.Multiply<double>(result, 1.0 / sourceSet.Length, result);
            TensorPrimitives.Max<double>(result, 0.0, result);
            TensorPrimitives.Sqrt<double>(result, result);
        }
        return TensorPrimitives.IsFiniteAll<double>(result) ? Fin.Succ(result) : Fin.Fail<double[]>(error: key.InvalidResult());
    }
    internal static Fin<SpectralDescriptor> NormalizeDescriptor(SpectralDescriptor descriptor, SpectralEnergyNormalization energy, Op key) =>
        from valid in guard(descriptor.IsValid, key.InvalidInput()).ToFin()
        from values in Rescaled(values: [.. descriptor.Values.AsIterable()], energy: energy, key: key)
        let merged = descriptor.Receipt.Policy with { EnergyNormalization = energy }
        select new SpectralDescriptor(Values: new Arr<double>(values), Receipt: descriptor.Receipt with { Policy = merged });
    internal static Fin<SpectralRanking> RankDescriptors(SpectralDescriptor query, Seq<SpectralDescriptor> candidates, SpectralRankingPolicy policy, Op key) =>
        !policy.IsValid || !query.IsValid || candidates.IsEmpty || !candidates.ForAll(static candidate => candidate.IsValid)
            ? Fin.Fail<SpectralRanking>(key.InvalidInput())
            : from normalizedQuery in NormalizeForRanking(descriptor: query, policy: policy.Descriptor, key: key)
              from normalizedCandidates in candidates.TraverseM(candidate => NormalizeForRanking(descriptor: candidate, policy: policy.Descriptor, key: key)).As()
              from ranks in RankNormalized(query: normalizedQuery, candidates: normalizedCandidates, policy: policy, key: key)
              select new SpectralRanking(Query: normalizedQuery, Items: ranks, Policy: policy);
    private static Fin<SpectralDescriptor> NormalizeForRanking(SpectralDescriptor descriptor, SpectralDescriptorPolicy policy, Op key) =>
        descriptor.Receipt.ComparisonReady && descriptor.Receipt.Policy.Equals(policy)
            ? Fin.Succ(descriptor)
            : NormalizeDescriptor(descriptor: descriptor, energy: policy.EnergyNormalization, key: key);
    private static Fin<(double[] Weights, Option<SpectralWaveReceipt> Wave)> WeightsOf(SpectralFilter filter, double[] eigenvalues, double firstNonZero, double zeroBand, int zeroModeCount, int croppedCount, Op key) =>
        filter is SpectralFilter.WaveCase wave
            ? WaveWeightsOf(wave: wave, eigenvalues: eigenvalues, firstNonZero: firstNonZero, zeroBand: zeroBand, zeroModeCount: zeroModeCount, croppedCount: croppedCount)
                .Map(static held => (held.Weights, Wave: Some(held.Receipt)))
                .ToFin(key.InvalidResult())
            : (double[])[.. eigenvalues.Select(filter.Weight)] is var weights && TensorPrimitives.IsFiniteAll<double>(weights)
                ? Fin.Succ<(double[], Option<SpectralWaveReceipt>)>((weights, Option<SpectralWaveReceipt>.None))
                : Fin.Fail<(double[], Option<SpectralWaveReceipt>)>(key.InvalidResult());
    private static Option<(double[] Weights, SpectralWaveReceipt Receipt)> WaveWeightsOf(SpectralFilter.WaveCase wave, double[] eigenvalues, double firstNonZero, double zeroBand, int zeroModeCount, int croppedCount) {
        double[] raw = [.. eigenvalues.Select(wave.Weight)];
        double sum = TensorPrimitives.Sum<double>(raw);
        if (!double.IsFinite(sum) || sum <= EpsilonPolicy.SqrtEpsilon) { return Option<(double[], SpectralWaveReceipt)>.None; }
        double[] normalized = new double[raw.Length];
        TensorPrimitives.Divide<double>(raw, sum, normalized);
        double[] positiveLogs = [.. eigenvalues.Where(lambda => lambda > zeroBand).Select(static lambda => Math.Log(d: lambda))];
        SpectralWaveReceipt receipt = new(
            Energy: wave.Energy.Value,
            Bandwidth: Math.Max(val1: wave.Bandwidth.Value, val2: EpsilonPolicy.ZeroTolerance),
            FirstNonZeroScale: firstNonZero > 0.0 ? Some(firstNonZero) : Option<double>.None,
            ZeroModeCount: zeroModeCount,
            CroppedEigenpairCount: croppedCount,
            NonZeroEigenpairCount: positiveLogs.Length,
            RawWeightSum: sum,
            NormalizedWeightSum: TensorPrimitives.Sum<double>(normalized),
            MinLogEigenvalue: positiveLogs.Length == 0 ? Option<double>.None : Some(TensorPrimitives.Min<double>(positiveLogs)),
            MaxLogEigenvalue: positiveLogs.Length == 0 ? Option<double>.None : Some(TensorPrimitives.Max<double>(positiveLogs)));
        return receipt.IsValid ? Some((normalized, receipt)) : Option<(double[], SpectralWaveReceipt)>.None;
    }
    private static Fin<Seq<SpectralRank>> RankNormalized(SpectralDescriptor query, Seq<SpectralDescriptor> candidates, SpectralRankingPolicy policy, Op key) {
        int valueCount = query.Values.Count;
        if (valueCount <= 0 || candidates.Exists(candidate => candidate.Values.Count != valueCount)) { return Fin.Fail<Seq<SpectralRank>>(key.InvalidInput()); }
        Lifted lifted = Lifted.Of([.. query.Values.AsIterable()]);
        SpectralRank[] ranks = [.. candidates.AsIterable()
            .Select((candidate, index) => new SpectralRank(Index: index, Distance: policy.Distance.Compute(a: lifted, b: Lifted.Of([.. candidate.Values.AsIterable()])), Descriptor: candidate))
            .OrderBy(static rank => rank.Distance).ThenBy(static rank => rank.Index)];
        return TensorPrimitives.IsFiniteAll<double>([.. ranks.Select(static rank => rank.Distance)]) ? Fin.Succ(toSeq(ranks)) : Fin.Fail<Seq<SpectralRank>>(key.InvalidResult());
    }
    private static Fin<double[]> NormalizeValues(double[] values, SpectralDescriptorPolicy policy, Op key) =>
        Rescaled(values: values, energy: policy.EnergyNormalization, key: key);
    private static Fin<double[]> Rescaled(double[] values, SpectralEnergyNormalization energy, Op key) =>
        TensorPrimitives.IsFiniteAll<double>(values)
            ? energy.Rescale(values: values).ToFin(key.InvalidResult())
            : Fin.Fail<double[]>(key.InvalidResult());
}
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
