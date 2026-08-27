# [RASM_NUMERICS_SPECTRAL]

`Rasm.Numerics` spectral owns the mesh-free discrete-exterior-calculus carrier layer and the spectral filter/descriptor algebra with zero mesh coupling, so the pure math floor stands independent of any `Mesh` and every eigenvalue-driven consumer meets one transfer-function and descriptor surface.

`DiscreteCalculus` is the frozen cross-package contract name the `Rasm.Compute` adjoint surface binds; `Meshing/dec` constructs the DEC carriers this page only declares, and `Meshing/mesh` declares `SignpostTransport`, carried here on the `Domain/validation` `Evidence` probe verdict, so each DDG carrier has one declaration site. `SpectralFilter` weights eigenvalues alone, the keyless `[SmartEnum]` policy rosters drive normalization and distance selection beside the `NormalizeScale`/`IncludeZeroModes` policy booleans, and every consumer reads carriers and descriptors from this floor without touching a `Mesh`. Transform-domain machinery — the arena, the taper roster, the tap fold — is `Numerics/transform#SPECTRAL`'s; this page weights an already-solved spectrum and mints no transform.

## [01]-[INDEX]

- [02]-[FILTER_ALGEBRA]: keyless `[SmartEnum]` policy rosters and `SpectralFilter`, the closed transfer-function `[Union]` with its eigenvalue weight law and partial-monoid `Compose`.
- [03]-[DEC_CARRIERS]: `DiscreteCalculus` the frozen adjoint contract, `SpectralBasis`, and the assembly/harmonic census family `Meshing/dec` mints and `Rasm.Compute` consumes.
- [04]-[DESCRIPTOR_ALGEBRA]: descriptor policy, profile, and carrier family and `SpectralKernel` filtered-signature evaluation, normalization, and ranking.

## [02]-[FILTER_ALGEBRA]

- Owner: `DescriptorNormalization` and `DescriptorDistance` are the keyless `[SmartEnum]` policy rosters, the distance row carrying its `[UseDelegateFromConstructor]` compute column over `MathNet.Numerics.Distance` and the normalization row its `Apply` column over `TensorPrimitives`; `SpectralFilter` is the closed `[Union]` whose `Weight(eigenvalue)` is the spectral transfer function and whose `Compose` is a partial monoid — composable pairs fuse, `Identity` is the unit, every other pair is `None` by law.
- Cases: `ExponentialCase(rate)`, `WaveCase(energy, bandwidth)`, `PowerCase(exponent)`, `IdentityCase` — heat, diffusion, and amplification are ONE exponential family, apart by a literal and a sign, so `Diffusion(t)` IS `Heat(2t)` and the three fuse under `Compose` where three sibling cases each answered `None`.
- Entry: the `SpectralFilter.Heat`/`Diffusion`/`Amplify` factories take their magnitudes as `PositiveMagnitude` and land as canonical `Exponential` mints, `Wave` carries its own pair, and `Power` takes the signed exponent whole, so a filter in hand is admitted; `Biharmonic` (λ^−2) and `CommuteTime` (λ^−1) are the same canonical-mint precedent on the power leg; `Evaluate(basis, sources, key, policy)` is the ONE evaluation entry, absence of a policy riding the carrier rather than an arity twin.
- Auto: `Compose` dispatches through the GENERATED `Switch` on the left operand, so a new case is a compile break rather than a silent fall to `None`; `Weight` carries `[MethodImpl(AggressiveInlining)]` for the descriptor kernel's per-eigenpair-per-vertex hot loop; the wave weight floors its bandwidth at `EpsilonPolicy.ZeroTolerance` — the anchor itself, resolved in one hop rather than through a rename shell a sibling type reached across for.
- Law: `Compose`'s NAMED LOSS is right-side exhaustiveness — the generated `Switch` closes the left family and each arm's own `is` test decides its one composable partner, because a total pair fold over four cases is sixteen arms stating one law twelve times.
- Packages: MathNet.Numerics (`Distance` — the twelve metric rows, split across its `Vector<T>` and `double[]` carriers; `CreateVector.DenseOfArray` the lift the `DistanceOperand` pays once), System.Numerics.Tensors (`TensorPrimitives` — the descriptor normalization and the span reductions), `Rasm.Numerics` `atoms.md` (`EpsilonPolicy`, `PositiveMagnitude`), Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new transfer function whose weight is `exp(rate*λ)` is a MINT, not a case; a genuinely new shape is one case, one `Weight` arm, and one `Compose` arm, the kernel and every consumer untouched; a new normalization is one row with its `Apply` column and a new distance one row with its compute column.
- Boundary: filters weight eigenvalues alone — never a mesh, a basis matrix, or a vertex — so the one filter value drives `Meshing/dec` heat scaffolds, `Processing/segment` descriptors, and `Spatial/fields` spectral-distance cases from this floor.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
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
[SmartEnum]
public sealed partial class DescriptorNormalization {
    public static readonly DescriptorNormalization Raw = new(apply: static values => Some(values));
    public static readonly DescriptorNormalization UnitL1 = new(apply: static values => Scaled(values: values, scale: TensorPrimitives.SumOfMagnitudes<double>(values)));
    public static readonly DescriptorNormalization UnitL2 = new(apply: static values => Scaled(values: values, scale: TensorPrimitives.Norm<double>(values)));
    public static readonly DescriptorNormalization ZScore = new(apply: static values => Centered(values: values));

    [UseDelegateFromConstructor] internal partial Option<double[]> Apply(double[] values);

    private static Option<double[]> Scaled(double[] values, double scale) {
        if (!double.IsFinite(scale) || scale <= EpsilonPolicy.SqrtEpsilon) { return Option<double[]>.None; }
        double[] destination = new double[values.Length];
        TensorPrimitives.Divide<double>(values, scale, destination);
        return Some(destination);
    }
    private static Option<double[]> Centered(double[] values) {
        double sigma = TensorPrimitives.StdDev<double>(values);
        if (!double.IsFinite(sigma) || sigma <= EpsilonPolicy.SqrtEpsilon) { return Option<double[]>.None; }
        double[] destination = new double[values.Length];
        TensorPrimitives.Subtract<double>(values, TensorPrimitives.Average<double>(values), destination);
        TensorPrimitives.Divide<double>(destination, sigma, destination);
        return Some(destination);
    }
}

[SmartEnum]
public sealed partial class DescriptorDistance {
    public static readonly DescriptorDistance Euclidean = new(compute: static (a, b) => MathNet.Numerics.Distance.Euclidean(a.Dense, b.Dense));
    public static readonly DescriptorDistance Manhattan = new(compute: static (a, b) => MathNet.Numerics.Distance.Manhattan(a.Dense, b.Dense));
    public static readonly DescriptorDistance Cosine = new(compute: static (a, b) => MathNet.Numerics.Distance.Cosine(a.Raw, b.Raw));
    public static readonly DescriptorDistance Chebyshev = new(compute: static (a, b) => MathNet.Numerics.Distance.Chebyshev(a.Dense, b.Dense));
    public static readonly DescriptorDistance Canberra = new(compute: static (a, b) => MathNet.Numerics.Distance.Canberra(a.Raw, b.Raw));
    public static readonly DescriptorDistance Minkowski3 = new(compute: static (a, b) => MathNet.Numerics.Distance.Minkowski(3.0, a.Dense, b.Dense));
    public static readonly DescriptorDistance Hamming = new(compute: static (a, b) => MathNet.Numerics.Distance.Hamming(a.Raw, b.Raw));
    public static readonly DescriptorDistance Jaccard = new(compute: static (a, b) => MathNet.Numerics.Distance.Jaccard(a.Raw, b.Raw));
    public static readonly DescriptorDistance MeanAbsolute = new(compute: static (a, b) => MathNet.Numerics.Distance.MAE(a.Dense, b.Dense));
    public static readonly DescriptorDistance MeanSquared = new(compute: static (a, b) => MathNet.Numerics.Distance.MSE(a.Dense, b.Dense));
    public static readonly DescriptorDistance SumAbsolute = new(compute: static (a, b) => MathNet.Numerics.Distance.SAD(a.Dense, b.Dense));
    public static readonly DescriptorDistance SumSquared = new(compute: static (a, b) => MathNet.Numerics.Distance.SSD(a.Dense, b.Dense));

    [UseDelegateFromConstructor] internal partial double Compute(DistanceOperand a, DistanceOperand b);
}

[StructLayout(LayoutKind.Auto)]
internal readonly record struct DistanceOperand(double[] Raw) {
    internal MathNet.Numerics.LinearAlgebra.Vector<double> Dense { get; } = MathNet.Numerics.LinearAlgebra.CreateVector.DenseOfArray(Raw);
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
    public static SpectralFilter Identity { get; } = new IdentityCase();
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
    internal Fin<SpectralDescriptor> Evaluate(SpectralBasis basis, Option<Seq<int>> sources, Option<SpectralDescriptorPolicy> policy = default) =>
        policy.IfNone(noneValue: SpectralDescriptorPolicy.Raw) switch {
            SpectralDescriptorPolicy active when basis.IsValid && active.IsValid => SpectralKernel.Evaluate(basis: basis, sources: sources, filter: this, policy: active),
            _ => Fin.Fail<SpectralDescriptor>(new KernelFault.InvalidInput()),
        };
}
```

## [03]-[DEC_CARRIERS]

- Owner: `DiscreteCalculus` is the DEC operator bundle — incidence and curl operators, the diagonal Hodge stars, its `SpectralAssembly` census, the `Transport` probe column (`Domain/validation` `Evidence<SignpostTransport>` — a signpost probe that refused keeps its cause distinct from one never run), and the optional `Harmonic` slot — with `Project<TOut>` routing the evidence family through typed `ProjectionRow` rows. `SpectralBasis` is the eigenpair carrier with `Truncate(k)` and the ONE scale-relative `ZeroBand` (`SqrtEpsilon` × spectral radius) the descriptor kernel reuses to classify zero modes, so one threshold declaration carries every consumer with zero drift. `SpectralAssembly`, `HarmonicCensus`, and `HarmonicOneFormBasis` are the assembly and harmonic measurements, their semantic gates scale-relative against `max(1, spectralRadius)` rather than any bare absolute.
- Entry: carriers are constructed by `Meshing/dec` (assembly) and `Meshing/mesh` (caching); this page owns their shape, validity law, and projection, and consumers — the `Rasm.Compute` adjoint surface, `Processing/geodesics`, `Processing/segment`, `Spatial/fields` — read them from here.
- Auto: `DiscreteCalculus.IsValid` cross-couples the stars to the operator shapes, requires strictly positive vertex and face stars, admits edge stars down to a scale-relative negative band, where near-degenerate intrinsic cotan weights legitimately dip below zero within roundoff of the `Star1` scale, and gates harmonic presence on the dimension `2g + max(0, b−1)` derived from `Assembly.Genus` and `Assembly.BoundaryComponentCount` — the one place a composing witness receives that coupling settled.
- Law: `HarmonicCensus` is the ONE owner of the harmonic-decomposition measurements — basis count, edge count, rank, nullity, positive-star-1 count, the closed and co-closed residuals, and the star-1 orthonormality residual — with the expected dimension DERIVED from genus and boundary components and `SvdTolerance` the ONE nullspace threshold, so neither rides a second slot the gate could only reject. A composing carrier CARRIES it (`Meshing/dec`'s `HodgeWitness` nests it as `Option<HarmonicCensus>`) and re-declares no slot of it, because a second copy of one measurement is two independently-filled values with no claim tying them and either may be the wrong one.
- Law: all three carriers fold `ValidityClaim.All` with `IValidityEvidence` registration; the semantic gates carry the harmonic dimension law, the `Rank + Nullity == EdgeCount` partition, and the residual-tolerance ladder. Every field is read by a conjunct, so a slot no gate consumes is false evidence and never lands; a measure one assembly arm cannot take rides `Option` and reads as an absent claim, never a zero the gate mistakes for a measurement — the symmetry residual and its tolerance are ONE optional pair for exactly that reason, since a zeroed pair passed `0 <= 0` for an arm that measured neither.
- Packages: `matrix.md` owners (`SparseMatrix`, `EigenSolution`), System.Numerics.Tensors (`TensorPrimitives.MaxMagnitude`/`Min`/`IsFiniteAll` — the star and eigenvalue folds), LanguageExt.Core, Rasm.Domain (`IValidityEvidence` + `ValidityClaim`).
- Growth: a new DEC operator is one field, one validity coupling, and one `ProjectionRow`; a new assembly witness is one `SpectralAssembly` column.
- Boundary: `DiscreteCalculus` is the `Rasm.Compute` adjoint contract — Compute binds the operator columns and the validity fold, never `Transport`, so the probe column stays kernel-grain; `SignpostTransport` is declared by `Meshing/mesh`, the intrinsic-triangulation owner, and carried here only as probe evidence, so each DDG carrier has exactly one declaration site with this page owning the mesh-free members. DECLARATION and CONSTRUCTION are the split with `Meshing/dec`: no member here emits `D0`, `D1`, or a star, and `dec`'s assembly re-owns no algebra declared here.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
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
public readonly record struct SpectralAssembly(
    int VertexCount, int EdgeCount, int FaceCount, int AdmittedFaceCount, int SkippedDegenerateFaces, int SkippedMissingEdges,
    bool FlippedIntrinsicLifted, int MatrixRows, int MatrixCols, int NonZeros,
    int PositiveStar0Count, int PositiveStar1Count, int PositiveStar2Count,
    double BoundaryCompositionResidual, Option<int> Genus, bool EdgeConnection,
    Option<int> BoundaryEdgeCount = default, int BoundaryComponentCount = 0, Option<int> NonManifoldEdgeCount = default,
    Option<int> EulerCharacteristic = default, Option<int> PositiveMassCount = default,
    Option<(double Residual, double Tolerance)> Symmetry = default, Option<int> FactorNonZeros = default,
    Option<double> BoundaryCompositionTolerance = default) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        VertexCount >= 0 && EdgeCount >= 0 && FaceCount >= 0 && MatrixRows >= 0 && MatrixCols >= 0 && NonZeros >= 0,
        ValidityClaim.Nonnegative(value: BoundaryCompositionResidual),
        AdmittedFaceCount >= 0 && SkippedDegenerateFaces >= 0 && SkippedMissingEdges >= 0
            && (long)AdmittedFaceCount + SkippedDegenerateFaces + SkippedMissingEdges <= FaceCount,
        Symmetry is not { IsSome: true, Case: (double Residual, double Tolerance) measured } || (ValidityClaim.Nonnegative(measured.Residual) && ValidityClaim.Positive(measured.Tolerance) && measured.Residual <= measured.Tolerance),
        BoundaryCompositionTolerance is not { IsSome: true, Case: double tolerance } || (ValidityClaim.Positive(tolerance) && BoundaryCompositionResidual <= tolerance),
        FactorNonZeros.Map(static (int value) => value > 0).IfNone(noneValue: true),
        !FlippedIntrinsicLifted || EdgeConnection,
        BoundaryEdgeCount is not { IsSome: true, Case: int boundaryEdges }
            || (boundaryEdges >= 0 && boundaryEdges <= EdgeCount && (boundaryEdges == 0) == (BoundaryComponentCount == 0) && boundaryEdges >= 3L * BoundaryComponentCount),
        NonManifoldEdgeCount is not { IsSome: true, Case: int nonManifoldEdges } || (nonManifoldEdges >= 0 && nonManifoldEdges <= EdgeCount),
        EulerCharacteristic is not { IsSome: true, Case: int chi } || ((long)chi == (long)VertexCount - EdgeCount + FaceCount
            && NonManifoldEdgeCount.IfNone(noneValue: 0) == 0
            && (Genus is not { IsSome: true, Case: int genus } || chi == 2L - (2L * genus) - BoundaryComponentCount)),
        BoundaryComponentCount >= 0,
        Genus is not { IsSome: true, Case: int heldGenus } || heldGenus >= 0,
        EdgeConnection
            ? MatrixRows == (long)EdgeCount * 2L && MatrixCols == MatrixRows
              && (PositiveMassCount is not { IsSome: true, Case: int positiveMass } || (positiveMass >= 0 && positiveMass <= EdgeCount))
            : PositiveStar0Count >= 0 && PositiveStar0Count <= VertexCount
              && PositiveStar1Count >= 0 && PositiveStar1Count <= EdgeCount
              && PositiveStar2Count >= 0 && PositiveStar2Count <= FaceCount);
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct HarmonicCensus(
    Option<int> Genus, int ConstraintRows, int EdgeCount, int Rank, int Nullity, int BasisCount,
    double SvdTolerance, double EpsRank, double SpectralRadius,
    double MinNullEigenvalue, double MaxNullEigenvalue, double MaxClosedResidual, double MaxCoClosedResidual,
    double Star1OrthonormalResidual, int PositiveStar1Count, EigenSolution<double, Arr<double>> Eigen,
    int BoundaryComponentCount = 0) : IValidityEvidence {
    private const double ResidualSlack = 1.0e3;
    public bool IsValid {
        get {
            int boundaryComponentCount = BoundaryComponentCount;
            long expected = Genus.Map((int genus) => (2L * genus) + Math.Max(0L, (long)boundaryComponentCount - 1L)).IfNone(0L);
            double residualTolerance = Math.Max(val1: SvdTolerance, val2: EpsilonPolicy.SqrtEpsilon * Math.Max(val1: 1.0, val2: SpectralRadius)) * ResidualSlack;
            return ValidityClaim.All(
                EdgeCount >= 0 && Rank >= 0 && Nullity >= 0 && BasisCount >= 0 && ConstraintRows >= 0 && boundaryComponentCount >= 0,
                (long)Rank + Nullity == EdgeCount,
                BasisCount == expected,
                Nullity >= expected,
                PositiveStar1Count >= 0 && PositiveStar1Count <= EdgeCount,
                Genus.Map(static (int genus) => genus >= 0).IfNone(noneValue: true),
                ValidityClaim.Positive(value: SvdTolerance),
                ValidityClaim.Positive(value: EpsRank),
                ValidityClaim.Nonnegative(value: SpectralRadius),
                ValidityClaim.Finite(value: residualTolerance),
                SvdTolerance <= (EpsRank * Math.Max(val1: 1.0, val2: SpectralRadius)) + EpsilonPolicy.SqrtEpsilon,
                ValidityClaim.Finite(value: MinNullEigenvalue) && MinNullEigenvalue >= -EpsilonPolicy.SqrtEpsilon,
                ValidityClaim.Finite(value: MaxNullEigenvalue) && MaxNullEigenvalue >= MinNullEigenvalue - EpsilonPolicy.SqrtEpsilon,
                ValidityClaim.Nonnegative(value: MaxClosedResidual) && MaxClosedResidual <= residualTolerance,
                ValidityClaim.Nonnegative(value: MaxCoClosedResidual) && MaxCoClosedResidual <= residualTolerance,
                ValidityClaim.Nonnegative(value: Star1OrthonormalResidual) && Star1OrthonormalResidual <= residualTolerance,
                ValidityClaim.Evidence(evidence: Some(Eigen)));
        }
    }
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct HarmonicOneFormBasis(Arr<Arr<double>> Forms, HarmonicCensus Census) : IValidityEvidence {
    public bool IsValid {
        get {
            HarmonicCensus census = Census;
            return ValidityClaim.All(
                ValidityClaim.Evidence(evidence: Some(census)),
                ValidityClaim.CountExactly(count: Forms.Count, expected: census.BasisCount),
                Forms.ForAll(form => form.Count == census.EdgeCount && form.ForAll(double.IsFinite)));
        }
    }
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct DiscreteCalculus(SparseMatrix D0, SparseMatrix D1, Arr<double> Star0, Arr<double> Star1, Arr<double> Star2, SpectralAssembly Assembly, Evidence<SignpostTransport> Transport, Option<HarmonicOneFormBasis> Harmonic = default) : IValidityEvidence {
    public bool IsValid {
        get {
            ReadOnlySpan<double> star0 = Star0.AsSpan(), star1 = Star1.AsSpan(), star2 = Star2.AsSpan();
            double star1Scale = star1.IsEmpty ? 0.0 : Math.Abs(value: TensorPrimitives.MaxMagnitude<double>(star1));
            int boundaryComponentCount = Assembly.BoundaryComponentCount;
            long expectedHarmonic = Assembly.Genus.Map((int genus) => (2L * genus) + Math.Max(0L, (long)boundaryComponentCount - 1L)).IfNone(0L);
            return ValidityClaim.All(
                D0.IsValid && D1.IsValid,
                Harmonic.IsSome == (expectedHarmonic > 0),
                Harmonic.Map((HarmonicOneFormBasis basis) => basis.Census.BasisCount == expectedHarmonic).IfNone(noneValue: true),
                ValidityClaim.Evidence(evidence: Some(Assembly)),
                Star0.Count == D0.Cols.Value && Star1.Count == D0.Rows.Value && Star2.Count == D1.Rows.Value,
                TensorPrimitives.IsFiniteAll<double>(star0) && (star0.IsEmpty || TensorPrimitives.Min<double>(star0) > 0.0),
                TensorPrimitives.IsFiniteAll<double>(star1)
                    && (star1.IsEmpty || TensorPrimitives.Min<double>(star1) >= -(EpsilonPolicy.SqrtEpsilon * Math.Max(1.0, star1Scale))),
                TensorPrimitives.IsFiniteAll<double>(star2) && (star2.IsEmpty || TensorPrimitives.Min<double>(star2) > 0.0),
                Transport is { } probe && ValidityClaim.Evidence(evidence: probe.Value()),
                ValidityClaim.Evidence(evidence: Harmonic));
        }
    }
    internal Fin<TOut> Project<TOut>() {
        DiscreteCalculus self = this;
        return ResultProjection.Rows<DiscreteCalculus, TOut>(self: self,
            ProjectionRow.Of<SpectralAssembly>(() => Fin.Succ(self.Assembly)),
            ProjectionRow.Of<SignpostTransport>(() => self.Transport.Switch(
                measured: static row => Fin.Succ(row.Value),
                refused: static row => Fin.Fail<SignpostTransport>(row.Cause),
                absent: _ => Fin.Fail<SignpostTransport>(new KernelFault.InvalidResult()))),
            ProjectionRow.Of<HarmonicOneFormBasis>(() => self.Harmonic.ToFin(new KernelFault.InvalidResult())),
            ProjectionRow.Of<HarmonicCensus>(() => self.Harmonic.Map(static basis => basis.Census).ToFin(new KernelFault.InvalidResult())));
    }
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct SpectralBasis(Arr<double> Eigenvalues, Arr<Arr<double>> Eigenvectors) : IValidityEvidence {
    internal int VertexCount => Eigenvectors.IsEmpty ? 0 : Eigenvectors[index: 0].Count;
    internal double ZeroBand => EpsilonPolicy.SqrtEpsilon * Math.Max(val1: Eigenvalues.IsEmpty ? 0.0 : Math.Abs(value: TensorPrimitives.MaxMagnitude<double>(Eigenvalues.AsSpan())), val2: EpsilonPolicy.ZeroTolerance);
    public bool IsValid {
        get {
            int vertexCount = VertexCount;
            double zeroBand = ZeroBand;
            ReadOnlySpan<double> eigenvalues = Eigenvalues.AsSpan();
            return ValidityClaim.All(
                ValidityClaim.CountAtLeast(count: Eigenvalues.Count, floor: 1),
                ValidityClaim.CountExactly(count: Eigenvectors.Count, expected: Eigenvalues.Count),
                TensorPrimitives.IsFiniteAll<double>(eigenvalues) && TensorPrimitives.Min<double>(eigenvalues) >= -zeroBand,
                Eigenvalues.AsIterable().Zip(Eigenvalues.AsIterable().Skip(1)).All(static ((double First, double Second) pair) => pair.First <= pair.Second),
                Eigenvectors.ForAll(phi => vertexCount > 0 && phi.Count == vertexCount && TensorPrimitives.IsFiniteAll<double>(phi.AsSpan())));
        }
    }
    public SpectralBasis Truncate(int k) =>
        k <= 0 || k >= Eigenvalues.Count ? this : new SpectralBasis(Eigenvalues: new Arr<double>([.. Eigenvalues.AsIterable().Take(k)]), Eigenvectors: new Arr<Arr<double>>([.. Eigenvectors.AsIterable().Take(k)]));
}
```

## [04]-[DESCRIPTOR_ALGEBRA]

- Owner: `SpectralDescriptorPolicy` is the normalization bundle (`NormalizeScale` × normalization × `IncludeZeroModes` × optional crop) with `Raw` the no-op row, admitted at the `Evaluate` entry beside the basis; `WaveProfile` and `DescriptorProfile` carry the WKS weighting and the descriptor's filter, policy, and counts; `SpectralDescriptor` is the values-and-profile carrier with `Normalize(normalization)` and `Rank(candidates, policy)`; `SpectralRankingPolicy`/`SpectralRank`/`SpectralRanking` carry ranking. `SpectralKernel` is the `internal static` evaluation owner — the dense-buffer filtered-signature kernel, value normalization through the `DescriptorNormalization` `Apply` column, and ranking off the `DescriptorDistance` compute column over one pre-lifted operand; the harmonic eps-rank default stays `Meshing/dec`'s, declared beside the construction that applies it.
- Entry: `filter.Evaluate(basis, sources, policy)` is the evaluation entry; `descriptor.Normalize(normalization)` and `descriptor.Rank(candidates, policy)` the post-processing entries, normalization taking the ONE axis it honours rather than a four-axis bundle it refuses three arms of — one descriptor pipeline, no sibling evaluate/compare surfaces.
- Auto: WKS weights normalize to unit sum with the full `WaveProfile` minted inline, and the wave arm answers its OWN failure rather than the caller re-testing the case a line later; the profile carries no readiness mirror — `Policy`, `Wave`, and `SourceCount` ARE the facts, and `IsValid` ties the cropped prefix, the zero-mode census, and the wave's nonzero count to the policy that produced them; `RankDescriptors` re-normalizes a candidate on the value-normalization axis alone and REFUSES a scale, zero-mode, or crop mismatch, because those axes cannot be reconstructed once the eigenbasis is discarded.
- Law: ranking short-circuits on the first unusable candidate rather than accumulating — a ranking is a JOINT verdict over the whole candidate set, so a partial ordering is a wrong ordering and there is no per-candidate outcome for a `Validation` to carry.
- Exemption: `Evaluate` is a declared statement kernel — a dense per-eigenpair-per-vertex accumulation buffer, where a query fold churns allocations over `n · k` (`· |S|` pairwise) terms. The exemption covers ALLOCATION shape alone: every per-vertex arm rides `TensorPrimitives` over one pooled lane, and the accumulation walks `ki → s → v` so a source ordinate is read once per pair instead of once per vertex.
- Law: `WaveProfile` and `DescriptorProfile` fold `ValidityClaim.All` with semantic gates over the source, eigenpair, and vertex couplings and the WKS unit-sum, whose band scales with the summed term count rather than sitting at a bare absolute.
- Packages: MathNet.Numerics (`Distance` rows over the `DistanceOperand`), System.Numerics.Tensors (`TensorPrimitives.Sum`/`Min`/`Max`/`MaxMagnitude`/`Subtract`/`Multiply`/`MultiplyAdd`/`Sqrt`/`Divide`/`IsFiniteAll` — every reduction, rescale, and accumulation on this cluster), CommunityToolkit.HighPerformance (`MemoryOwner<T>` the accumulation lane), LanguageExt.Core, Thinktecture.Runtime.Extensions.
- Growth: a new signature family is one filter case and policy rows, the kernel loop already generic over the weight function; a new distance is one `DescriptorDistance` row and a new normalization one `DescriptorNormalization` row, the compute and apply columns being the arms.
- Boundary: the kernel is mesh-free, seeing vertex COUNT as its only topology, so it serves tet, grid, and mesh bases identically, while mesh-side basis computation and caching (`SpectralBasisBundle`) are `Meshing/dec`'s.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System;
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
public readonly record struct SpectralDescriptorPolicy(bool NormalizeScale, DescriptorNormalization Normalization, bool IncludeZeroModes, Option<Dimension> CropCount) : IValidityEvidence {
    public static SpectralDescriptorPolicy Raw => new(NormalizeScale: false, Normalization: DescriptorNormalization.Raw, IncludeZeroModes: true, CropCount: None);
    public bool IsValid => ValidityClaim.All(
        Normalization is not null,
        CropCount.Map(static (Dimension count) => count.Value > 0).IfNone(noneValue: true));
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct WaveProfile(
    double FirstNonZeroScale, int NonZeroEigenpairCount, double RawWeightSum, double NormalizedWeightSum,
    double MinLogEigenvalue, double MaxLogEigenvalue) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        NonZeroEigenpairCount > 0,
        ValidityClaim.Positive(value: RawWeightSum),
        Math.Abs(value: NormalizedWeightSum - 1.0) <= EpsilonPolicy.SqrtEpsilon * Math.Max(val1: 1, val2: NonZeroEigenpairCount),
        ValidityClaim.Positive(value: FirstNonZeroScale),
        ValidityClaim.Finite(value: MinLogEigenvalue),
        ValidityClaim.Finite(value: MaxLogEigenvalue) && MinLogEigenvalue <= MaxLogEigenvalue);
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct DescriptorProfile(SpectralFilter Filter, int VertexCount, int EigenpairCount, int SourceCount,
    SpectralDescriptorPolicy Policy, int ZeroModeCount = 0, int CroppedEigenpairCount = 0, Option<WaveProfile> Wave = default) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        Filter is not null && (Filter is SpectralFilter.WaveCase) == Wave.IsSome && VertexCount > 0 && EigenpairCount > 0,
        ZeroModeCount >= 0 && ZeroModeCount <= EigenpairCount && CroppedEigenpairCount > 0
            && CroppedEigenpairCount == Math.Min(
                Policy.CropCount.Map(static (Dimension count) => count.Value).IfNone(EigenpairCount),
                Policy.IncludeZeroModes ? EigenpairCount : EigenpairCount - ZeroModeCount)
            && (!Policy.NormalizeScale || ZeroModeCount < EigenpairCount)
            && (Wave is not { IsSome: true, Case: WaveProfile wave }
                || wave.NonZeroEigenpairCount == (Policy.IncludeZeroModes ? Math.Max(0, CroppedEigenpairCount - ZeroModeCount) : CroppedEigenpairCount)),
        SourceCount >= 0 && SourceCount <= VertexCount,
        ValidityClaim.Evidence(evidence: Some(Policy)),
        ValidityClaim.Evidence(evidence: Wave));
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct SpectralDescriptor(Arr<double> Values, DescriptorProfile Profile) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Evidence(evidence: Some(Profile)),
        ValidityClaim.CountExactly(count: Values.Count, expected: Profile.VertexCount),
        Values.ForAll(double.IsFinite));
    public Fin<SpectralDescriptor> Normalize(DescriptorNormalization normalization) =>
        SpectralKernel.Normalize(descriptor: this, normalization: normalization);
    public Fin<SpectralRanking> Rank(Seq<SpectralDescriptor> candidates, SpectralRankingPolicy policy) =>
        SpectralKernel.RankDescriptors(query: this, candidates: candidates, policy: policy);
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct SpectralRankingPolicy(SpectralDescriptorPolicy Descriptor, DescriptorDistance Distance) : IValidityEvidence {
    public static SpectralRankingPolicy Default => new(Descriptor: SpectralDescriptorPolicy.Raw, Distance: DescriptorDistance.Euclidean);
    public bool IsValid => Distance is not null && ValidityClaim.Evidence(evidence: Some(Descriptor));
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct SpectralRank(int Index, double Distance, SpectralDescriptor Descriptor);

[StructLayout(LayoutKind.Auto)]
public readonly record struct SpectralRanking(SpectralDescriptor Query, Seq<SpectralRank> Items, SpectralRankingPolicy Policy);

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class SpectralKernel {
    internal static Fin<SpectralDescriptor> Evaluate(SpectralBasis basis, Option<Seq<int>> sources, SpectralFilter filter, SpectralDescriptorPolicy policy) {
        int n = basis.VertexCount;
        int[] sourceSet = sources.Map(static values => values.AsIterable().ToArray()).IfNone([]);
        if (n == 0 || (sources.IsSome && sourceSet.Length == 0) || toSet(sourceSet).Count != sourceSet.Length
            || (sourceSet.Length > 0 && (TensorPrimitives.Min<int>(sourceSet) < 0 || TensorPrimitives.Max<int>(sourceSet) >= n))) {
            return Fin.Fail<SpectralDescriptor>(error: new KernelFault.InvalidInput());
        }
        if (!basis.Eigenvectors.ForAll(phi => phi.Count == n)) { return Fin.Fail<SpectralDescriptor>(error: new KernelFault.InvalidResult()); }
        double zeroBand = basis.ZeroBand;
        int zeroModeCount = basis.Eigenvalues.AsIterable().Count(lambda => lambda <= zeroBand);
        Option<double> firstNonZero = basis.Eigenvalues.Find(lambda => lambda > zeroBand);
        bool scaleNormalized = policy.NormalizeScale;
        if (scaleNormalized && firstNonZero.IsNone) { return Fin.Fail<SpectralDescriptor>(error: new KernelFault.InvalidResult()); }
        double scale = firstNonZero.IfNone(1.0);
        int[] eigenIndices = [.. Enumerable.Range(start: 0, count: basis.Eigenvalues.Count)
            .Where((int i) => policy.IncludeZeroModes || basis.Eigenvalues[index: i] > zeroBand)
            .Take(policy.CropCount.Map(static count => count.Value).IfNone(basis.Eigenvalues.Count))];
        if (eigenIndices.Length == 0) { return Fin.Fail<SpectralDescriptor>(error: new KernelFault.InvalidInput()); }
        double[] scaledEigenvalues = [.. eigenIndices.Select(k => scaleNormalized ? basis.Eigenvalues[index: k] / scale : basis.Eigenvalues[index: k])];
        return WeightsOf(filter: filter, eigenvalues: scaledEigenvalues, firstNonZero: firstNonZero,
                zeroBand: scaleNormalized ? zeroBand / scale : zeroBand)
            .Bind(weighted => Accumulated(basis: basis, eigenIndices: eigenIndices, weights: weighted.Weights, sourceSet: sourceSet, vertexCount: n)
                .Bind((double[] values) => Rescaled(values: values, normalization: policy.Normalization))
                .Map(values => new SpectralDescriptor(
                    Values: new Arr<double>(values),
                    Profile: new DescriptorProfile(Filter: filter, VertexCount: n, EigenpairCount: basis.Eigenvalues.Count, SourceCount: sourceSet.Length,
                        Policy: policy, ZeroModeCount: zeroModeCount, CroppedEigenpairCount: eigenIndices.Length, Wave: weighted.Wave))));
    }
    private static Fin<double[]> Accumulated(SpectralBasis basis, int[] eigenIndices, double[] weights, int[] sourceSet, int vertexCount) {
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
        return TensorPrimitives.IsFiniteAll<double>(result) ? Fin.Succ(result) : Fin.Fail<double[]>(error: new KernelFault.InvalidResult());
    }
    internal static Fin<SpectralDescriptor> Normalize(SpectralDescriptor descriptor, DescriptorNormalization normalization) =>
        !descriptor.IsValid || normalization is null ? Fin.Fail<SpectralDescriptor>(new KernelFault.InvalidInput())
        : descriptor.Profile.Policy.Normalization.Equals(normalization) ? Fin.Succ(descriptor)
        : !descriptor.Profile.Policy.Normalization.Equals(DescriptorNormalization.Raw) ? Fin.Fail<SpectralDescriptor>(new KernelFault.InvalidInput())
        : from values in Rescaled(values: [.. descriptor.Values.AsIterable()], normalization: normalization)
          let merged = descriptor.Profile.Policy with { Normalization = normalization }
          select new SpectralDescriptor(Values: new Arr<double>(values), Profile: descriptor.Profile with { Policy = merged });
    internal static Fin<SpectralRanking> RankDescriptors(SpectralDescriptor query, Seq<SpectralDescriptor> candidates, SpectralRankingPolicy policy) =>
        !policy.IsValid || !query.IsValid || candidates.IsEmpty || !candidates.ForAll(static candidate => candidate.IsValid)
            ? Fin.Fail<SpectralRanking>(new KernelFault.InvalidInput())
            : from normalizedQuery in NormalizeForRanking(descriptor: query, policy: policy.Descriptor)
              from normalizedCandidates in candidates.TraverseM(candidate => NormalizeForRanking(descriptor: candidate, policy: policy.Descriptor)).As()
              from ranks in RankNormalized(query: normalizedQuery, candidates: normalizedCandidates, policy: policy)
              select new SpectralRanking(Query: normalizedQuery, Items: ranks, Policy: policy);
    private static Fin<SpectralDescriptor> NormalizeForRanking(SpectralDescriptor descriptor, SpectralDescriptorPolicy policy) =>
        !(descriptor.Profile.Policy with { Normalization = policy.Normalization }).Equals(policy)
            ? Fin.Fail<SpectralDescriptor>(new KernelFault.InvalidInput())
            : descriptor.Profile.Policy.Normalization.Equals(policy.Normalization)
                ? Fin.Succ(descriptor)
                : Normalize(descriptor: descriptor, normalization: policy.Normalization);
    private static Fin<(double[] Weights, Option<WaveProfile> Wave)> WeightsOf(
        SpectralFilter filter, double[] eigenvalues, Option<double> firstNonZero, double zeroBand) =>
        filter is SpectralFilter.WaveCase wave
            ? firstNonZero.Bind((double first) => WaveWeightsOf(wave, eigenvalues, first, zeroBand))
                .Map(static ((double[] Weights, WaveProfile Profile) held) => (held.Weights, Wave: Some(held.Profile))).ToFin(new KernelFault.InvalidResult())
            : ((double[])[.. eigenvalues.Select(filter.Weight)]) switch {
                double[] weights when TensorPrimitives.IsFiniteAll<double>(weights) => Fin.Succ<(double[], Option<WaveProfile>)>((weights, Option<WaveProfile>.None)),
                _ => Fin.Fail<(double[], Option<WaveProfile>)>(new KernelFault.InvalidResult()),
            };
    private static Option<(double[] Weights, WaveProfile Profile)> WaveWeightsOf(
        SpectralFilter.WaveCase wave, double[] eigenvalues, double firstNonZero, double zeroBand) {
        double[] raw = [.. eigenvalues.Select((double lambda) => lambda > zeroBand ? wave.Weight(lambda) : 0.0)];
        double sum = TensorPrimitives.Sum<double>(raw);
        double[] positiveLogs = [.. eigenvalues.Where((double lambda) => lambda > zeroBand).Select(static (double lambda) => Math.Log(d: lambda))];
        if (!double.IsFinite(sum) || sum <= EpsilonPolicy.SqrtEpsilon || positiveLogs.Length == 0) { return Option<(double[], WaveProfile)>.None; }
        double[] normalized = new double[raw.Length];
        TensorPrimitives.Divide<double>(raw, sum, normalized);
        WaveProfile profile = new(
            FirstNonZeroScale: firstNonZero,
            NonZeroEigenpairCount: positiveLogs.Length,
            RawWeightSum: sum,
            NormalizedWeightSum: TensorPrimitives.Sum<double>(normalized),
            MinLogEigenvalue: TensorPrimitives.Min<double>(positiveLogs),
            MaxLogEigenvalue: TensorPrimitives.Max<double>(positiveLogs));
        return profile.IsValid ? Some((normalized, profile)) : Option<(double[], WaveProfile)>.None;
    }
    private static Fin<Seq<SpectralRank>> RankNormalized(SpectralDescriptor query, Seq<SpectralDescriptor> candidates, SpectralRankingPolicy policy) {
        int valueCount = query.Values.Count;
        if (valueCount <= 0 || candidates.Exists(candidate => candidate.Values.Count != valueCount)) { return Fin.Fail<Seq<SpectralRank>>(new KernelFault.InvalidInput()); }
        DistanceOperand queryOperand = new([.. query.Values.AsIterable()]);
        SpectralRank[] ranks = [.. candidates.AsIterable()
            .Select((SpectralDescriptor candidate, int index) => new SpectralRank(Index: index, Distance: policy.Distance.Compute(a: queryOperand, b: new([.. candidate.Values.AsIterable()])), Descriptor: candidate))
            .OrderBy(static rank => rank.Distance).ThenBy(static rank => rank.Index)];
        return System.Array.TrueForAll(ranks, static (SpectralRank rank) => double.IsFinite(rank.Distance)) ? Fin.Succ(toSeq(ranks)) : Fin.Fail<Seq<SpectralRank>>(new KernelFault.InvalidResult());
    }
    private static Fin<double[]> Rescaled(double[] values, DescriptorNormalization normalization) =>
        TensorPrimitives.IsFiniteAll<double>(values)
            ? normalization.Apply(values: values).Filter(static (double[] normalized) => TensorPrimitives.IsFiniteAll<double>(normalized)).ToFin(new KernelFault.InvalidResult())
            : Fin.Fail<double[]>(new KernelFault.InvalidResult());
}
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
