# [RASM_NUMERICS_SPECTRAL]

`Rasm.Numerics` spectral owns the mesh-free discrete-exterior-calculus carrier layer and the spectral filter/descriptor algebra with zero mesh coupling, so the pure math floor stands independent of any `Mesh` and every eigenvalue-driven consumer meets one transfer-function and descriptor surface.

`DiscreteCalculus` is the frozen cross-package contract name the `Rasm.Compute` adjoint surface binds; `Meshing/dec` constructs the DEC carriers this page only declares, and `Meshing/mesh` declares `SignpostTransport`, carried here on the `Domain/validation` `Evidence` probe verdict, so each DDG carrier has one declaration site. `SpectralFilter` weights eigenvalues alone, the keyless `[SmartEnum]` policy rosters drive normalization and distance selection beside the `NormalizeScale`/`IncludeZeroModes` policy booleans, and every consumer reads carriers and descriptors from this floor without touching a `Mesh`. Transform-domain machinery — the arena, the taper roster, the tap fold — is `Numerics/transform#SPECTRAL`'s; this page weights an already-solved spectrum and mints no transform.

## [01]-[INDEX]

- [02]-[FILTER_ALGEBRA]: keyless `[SmartEnum]` policy rosters and `SpectralFilter`, the closed transfer-function `[Union]` with its eigenvalue weight law and partial-monoid `Compose`.
- [03]-[DEC_CARRIERS]: `DiscreteCalculus` the frozen adjoint contract, `SpectralBasis`, and the assembly/harmonic census family `Meshing/dec` mints and `Rasm.Compute` consumes.
- [04]-[DESCRIPTOR_ALGEBRA]: descriptor policy, profile, and carrier family with evaluation on `SpectralFilter` and normalization and ranking on `SpectralDescriptor`.

## [02]-[FILTER_ALGEBRA]

- Owner: `DescriptorNormalization` and `DescriptorDistance` are the keyless `[SmartEnum]` policy rosters, the distance row carrying its `[UseDelegateFromConstructor]` compute column over the retained continuous `MathNet.Numerics.Distance` metrics and the normalization row its `Apply` column over `TensorPrimitives`; `SpectralFilter` is the closed `[Union]` whose `Weight(eigenvalue)` is the spectral transfer function and whose `Compose` is a partial monoid — composable pairs fuse, `Identity` is the unit, every other pair is `None` by law.
- Cases: `ExponentialCase(rate)`, `WaveCase(energy, bandwidth)`, `PowerCase(exponent)`, `IdentityCase` — heat, diffusion, and amplification are ONE exponential family apart only by an admitted scalar, while inverse and inverse-square filters are power values; like-shaped exponentials and powers fuse under `Compose`.
- Entry: `Exponential` and `Power` take admitted signed `Scalar` values and canonicalize zero to `Identity`; callers construct `WaveCase` directly from its admitted energy and bandwidth; `Evaluate(basis, sources, policy)` is the ONE evaluation entry, absence of a policy riding the carrier rather than an arity twin.
- Auto: `Compose` dispatches through the GENERATED `Switch` on the left operand, so a new case is a compile break rather than a silent fall to `None`; `Weight` carries `[MethodImpl(AggressiveInlining)]` for `Evaluate`'s per-eigenpair-per-vertex hot loop; logarithmic and negative-power domains use mathematical positivity, while each consumer owns its spectrum-relative zero classification.
- Law: `Compose`'s NAMED LOSS is right-side exhaustiveness — the generated `Switch` closes the left family and each arm's own `is` test decides its one composable partner, because a total pair fold over four cases is sixteen arms stating one law twelve times.
- Packages: MathNet.Numerics (`Distance` — the six continuous metric rows split across its `Vector<T>` and `double[]` carriers; `CreateVector.DenseOfArray` lifts each descriptor once), System.Numerics.Tensors (`TensorPrimitives` — the descriptor normalization and the span reductions), `Rasm.Numerics` `atoms.md` (`EpsilonPolicy`, `PositiveMagnitude`), Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new transfer function whose weight is `exp(rate*λ)` is a MINT, not a case; a genuinely new shape is one case, one `Weight` arm, and one `Compose` arm, with `Evaluate` and every consumer untouched; a new normalization is one row with its `Apply` column and a new distance one row with its compute column.
- Boundary: filters weight eigenvalues alone — never a mesh, a basis matrix, or a vertex — so the one filter value drives `Meshing/dec` heat scaffolds, `Processing/segment` descriptors, and `Spatial/fields` spectral-distance cases from this floor.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System;
using System.Linq;
using System.Numerics.Tensors;
using System.Runtime.CompilerServices;
using CommunityToolkit.HighPerformance.Buffers;
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
    public static readonly DescriptorNormalization ZScore = new(apply: static values => {
        double sigma = TensorPrimitives.StdDev<double>(values);
        if (!double.IsFinite(sigma) || sigma <= EpsilonPolicy.SqrtEpsilon) return Option<double[]>.None;
        double[] destination = new double[values.Length];
        TensorPrimitives.Subtract<double>(values, TensorPrimitives.Average<double>(values), destination);
        TensorPrimitives.Divide<double>(destination, sigma, destination);
        return Some(destination);
    });

    [UseDelegateFromConstructor] internal partial Option<double[]> Apply(double[] values);

    private static Option<double[]> Scaled(double[] values, double scale) {
        if (!double.IsFinite(scale) || scale <= EpsilonPolicy.SqrtEpsilon) { return Option<double[]>.None; }
        double[] destination = new double[values.Length];
        TensorPrimitives.Divide<double>(values, scale, destination);
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

    [UseDelegateFromConstructor] internal partial double Compute(
        (double[] Raw, MathNet.Numerics.LinearAlgebra.Vector<double> Dense) a,
        (double[] Raw, MathNet.Numerics.LinearAlgebra.Vector<double> Dense) b);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SpectralFilter {
    public sealed record ExponentialCase(Scalar Rate) : SpectralFilter;
    public sealed record WaveCase(PositiveMagnitude Energy, PositiveMagnitude Bandwidth) : SpectralFilter;
    public sealed record PowerCase(Scalar Exponent) : SpectralFilter;
    public sealed record IdentityCase : SpectralFilter;
    private SpectralFilter() { }
    public static SpectralFilter Exponential(Scalar rate) => rate.To() == 0.0 ? Identity : new ExponentialCase(Rate: rate);
    public static SpectralFilter Power(Scalar exponent) => exponent.To() == 0.0 ? Identity : new PowerCase(Exponent: exponent);
    public static SpectralFilter Identity { get; } = new IdentityCase();
    public Option<SpectralFilter> Compose(SpectralFilter other) =>
        other is IdentityCase ? Some(this) : Switch(
            state: other,
            exponentialCase: static (o, c) => o is ExponentialCase b ? Scalar.From(c.Rate.To() + b.Rate.To()).ToOption().Map(Exponential) : Option<SpectralFilter>.None,
            waveCase: static (_, _) => Option<SpectralFilter>.None,
            powerCase: static (o, c) => o is PowerCase b ? Scalar.From(c.Exponent.To() + b.Exponent.To()).ToOption().Map(Power) : Option<SpectralFilter>.None,
            identityCase: static (o, _) => Some(o));
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double Weight(double eigenvalue) => Switch(
        state: eigenvalue,
        exponentialCase: static (lambda, c) => Math.Exp(d: c.Rate.To() * lambda),
        waveCase: static (lambda, c) => lambda <= 0.0
            ? 0.0
            : ((Math.Log(d: c.Energy.Value) - Math.Log(d: lambda)) / c.Bandwidth.Value) switch {
                double ratio => Math.Exp(d: -0.5 * ratio * ratio),
            },
        powerCase: static (lambda, c) => lambda > 0.0 ? Math.Pow(x: lambda, y: c.Exponent.To()) : 0.0,
        identityCase: static (_, _) => 1.0);
    internal Fin<SpectralDescriptor> Evaluate(SpectralBasis basis, Option<Seq<int>> sources, Option<SpectralDescriptorPolicy> policy = default) {
        SpectralDescriptorPolicy active = policy.IfNone(noneValue: SpectralDescriptorPolicy.Raw);
        if (!basis.IsValid || !active.IsValid) return Fin.Fail<SpectralDescriptor>(new KernelFault.InvalidInput());
        int n = basis.VertexCount;
        int[] sourceSet = sources.Map(static values => values.AsIterable().ToArray()).IfNone([]);
        if ((sources.IsSome && sourceSet.Length == 0) || toSet(sourceSet).Count != sourceSet.Length
            || (sourceSet.Length > 0 && (TensorPrimitives.Min<int>(sourceSet) < 0 || TensorPrimitives.Max<int>(sourceSet) >= n)))
            return Fin.Fail<SpectralDescriptor>(new KernelFault.InvalidInput());
        double zeroBand = basis.ZeroBand;
        int zeroModeCount = basis.Eigenvalues.AsIterable().Count(lambda => lambda <= zeroBand);
        Option<double> firstNonZero = basis.Eigenvalues.Find(lambda => lambda > zeroBand);
        bool scaleNormalized = active.NormalizeScale;
        if (scaleNormalized && firstNonZero.IsNone) return Fin.Fail<SpectralDescriptor>(new KernelFault.InvalidResult());
        double scale = firstNonZero.IfNone(1.0);
        int[] eigenIndices = [.. Enumerable.Range(start: 0, count: basis.Eigenvalues.Count)
            .Where((int i) => active.IncludeZeroModes || basis.Eigenvalues[index: i] > zeroBand)
            .Take(active.CropCount.Map(static count => count.Value).IfNone(basis.Eigenvalues.Count))];
        if (eigenIndices.Length == 0) return Fin.Fail<SpectralDescriptor>(new KernelFault.InvalidInput());
        double[] scaledEigenvalues = [.. eigenIndices.Select(k => scaleNormalized ? basis.Eigenvalues[index: k] / scale : basis.Eigenvalues[index: k])];
        return WeightsOf(filter: this, eigenvalues: scaledEigenvalues, firstNonZero: firstNonZero,
                zeroBand: scaleNormalized ? zeroBand / scale : zeroBand)
            .Bind(weights => Accumulated(basis: basis, eigenIndices: eigenIndices, weights: weights, sourceSet: sourceSet, vertexCount: n)
                .Bind(values => active.Normalization.Apply(values)
                    .Filter(static normalized => TensorPrimitives.IsFiniteAll<double>(normalized))
                    .ToFin(new KernelFault.InvalidResult()))
                .Map(values => new SpectralDescriptor(
                    Values: new Arr<double>(values),
                    Profile: new DescriptorProfile(Filter: this, VertexCount: n, EigenpairCount: basis.Eigenvalues.Count,
                        SourceCount: sourceSet.Length, Policy: active, ZeroModeCount: zeroModeCount))));

        static Fin<double[]> Accumulated(SpectralBasis basis, int[] eigenIndices, double[] weights, int[] sourceSet, int vertexCount) {
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
            return TensorPrimitives.IsFiniteAll<double>(result)
                ? Fin.Succ(result)
                : Fin.Fail<double[]>(new KernelFault.InvalidResult());
        }

        static Fin<double[]> WeightsOf(SpectralFilter filter, double[] eigenvalues, Option<double> firstNonZero, double zeroBand) =>
            filter is WaveCase wave
                ? firstNonZero.Bind(_ => WaveWeightsOf(wave, eigenvalues, zeroBand)).ToFin(new KernelFault.InvalidResult())
                : ((double[])[.. eigenvalues.Select(filter.Weight)]) switch {
                    double[] weights when TensorPrimitives.IsFiniteAll<double>(weights) => Fin.Succ(weights),
                    _ => Fin.Fail<double[]>(new KernelFault.InvalidResult()),
                };

        static Option<double[]> WaveWeightsOf(WaveCase wave, double[] eigenvalues, double zeroBand) {
            double[] raw = [.. eigenvalues.Select((double lambda) => lambda > zeroBand ? wave.Weight(lambda) : 0.0)];
            double sum = TensorPrimitives.Sum<double>(raw);
            if (!TensorPrimitives.IsFiniteAll<double>(raw) || !double.IsFinite(sum) || sum <= EpsilonPolicy.SqrtEpsilon)
                return Option<double[]>.None;
            double[] normalized = new double[raw.Length];
            TensorPrimitives.Divide<double>(raw, sum, normalized);
            return Some(normalized);
        }
    }
}
```

## [03]-[DEC_CARRIERS]

- Owner: `DiscreteCalculus` is the DEC operator bundle — incidence and curl operators, the diagonal Hodge stars, its exterior-calculus `DiscreteOperatorAssembly` case, the `Transport` probe column (`Domain/validation` `Evidence<SignpostTransport>` — a signpost probe that refused keeps its cause distinct from one never run), and the optional `Harmonic` slot. `SpectralBasis` is the eigenpair carrier with the ONE scale-relative `ZeroBand` (`SqrtEpsilon` × spectral radius) the descriptor evaluation reuses to classify zero modes, so one threshold declaration carries every consumer with zero drift. `DiscreteOperatorAssembly`, `HarmonicCensus`, and `HarmonicOneFormBasis` are the assembly and harmonic measurements, their semantic gates scale-relative against `max(1, spectralRadius)` rather than any bare absolute.
- Entry: carriers are constructed by `Meshing/dec` (assembly) and `Meshing/mesh` (caching); this page owns their shape and validity law, and consumers read named fields or fold the existing `Option` and `Evidence` carriers directly.
- Auto: `DiscreteCalculus.IsValid` cross-couples the stars to the operator shapes, requires strictly positive vertex and face stars, admits edge stars down to a scale-relative negative band, where near-degenerate intrinsic cotan weights legitimately dip below zero within roundoff of the `Star1` scale, and gates harmonic presence on the dimension `2g + max(0, b−1)` derived from `Assembly.Genus` and `Assembly.BoundaryComponentCount` — the one place a composing witness receives that coupling settled.
- Law: `HarmonicCensus` is the ONE owner of intrinsic harmonic-decomposition measurements — basis count, edge count, rank, nullity, positive-star-1 count, the retained SVD threshold, spectral radius, eigenvalue interval, and residuals. Topology-derived dimension agreement belongs to `DiscreteCalculus`, where the exterior-calculus assembly evidence is available; provider payload and rank-policy input do not survive after their measurements are projected. A composing carrier CARRIES the census and re-declares no slot of it.
- Law: all three carriers fold `ValidityClaim.All` with `IValidityEvidence` registration; the semantic gates carry the harmonic dimension law, the `Rank + Nullity == EdgeCount` partition, and the residual-tolerance ladder. Every field is read by a conjunct, so a slot no gate consumes is false evidence and never lands; measurements exclusive to one assembly path live only on that generated case, so no option or selector flag can form a mixed payload.
- Packages: `matrix.md` owner `SparseMatrix`, System.Numerics.Tensors (`TensorPrimitives.MaxMagnitude`/`Min`/`IsFiniteAll` — the star and eigenvalue folds), LanguageExt.Core, Rasm.Domain (`IValidityEvidence` + `ValidityClaim`).
- Growth: a genuinely distinct discrete-operator assembly is one `DiscreteOperatorAssembly` case with its irreducible evidence and validity arm; a new DEC operator is one named field and one validity coupling.
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
using SparseMatrix = CSparse.Storage.CompressedColumnStorage<double>;

namespace Rasm.Numerics;

// --- [MODELS] --------------------------------------------------------------------------
[Thinktecture.Union(ConversionFromValue = Thinktecture.ConversionOperatorsGeneration.None)]
public abstract partial record DiscreteOperatorAssembly : IValidityEvidence {
    public sealed record ExteriorCalculusCase(
        int FaceCount, int AdmittedFaceCount, int SkippedDegenerateFaces, int SkippedMissingEdges,
        double BoundaryCompositionResidual, double BoundaryCompositionTolerance,
        Option<int> Genus, int BoundaryComponentCount) : DiscreteOperatorAssembly;
    public sealed record ConnectionLaplacianCase(
        int EdgeCount, int FaceCount, int AdmittedFaceCount, int SkippedDegenerateFaces, int SkippedMissingEdges,
        bool FlippedIntrinsicLifted, double SymmetryResidual, double SymmetryTolerance,
        int PositiveMassCount) : DiscreteOperatorAssembly;
    private DiscreteOperatorAssembly() { }
    public bool IsValid => Switch(
        exteriorCalculusCase: static c => ValidityClaim.All(
            c.FaceCount >= 0 && c.AdmittedFaceCount >= 0 && c.SkippedDegenerateFaces >= 0 && c.SkippedMissingEdges >= 0,
            (long)c.AdmittedFaceCount + c.SkippedDegenerateFaces + c.SkippedMissingEdges <= c.FaceCount,
            ValidityClaim.Nonnegative(c.BoundaryCompositionResidual),
            ValidityClaim.Positive(c.BoundaryCompositionTolerance),
            c.BoundaryCompositionResidual <= c.BoundaryCompositionTolerance,
            c.BoundaryComponentCount >= 0,
            c.Genus.Map(static genus => genus >= 0).IfNone(true)),
        connectionLaplacianCase: static c => ValidityClaim.All(
            c.EdgeCount >= 0 && c.FaceCount >= 0 && c.AdmittedFaceCount >= 0 && c.SkippedDegenerateFaces >= 0 && c.SkippedMissingEdges >= 0,
            (long)c.AdmittedFaceCount + c.SkippedDegenerateFaces + c.SkippedMissingEdges <= c.FaceCount,
            ValidityClaim.Nonnegative(c.SymmetryResidual), ValidityClaim.Positive(c.SymmetryTolerance),
            c.SymmetryResidual <= c.SymmetryTolerance,
            c.PositiveMassCount >= 0 && c.PositiveMassCount <= c.EdgeCount));
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct HarmonicCensus(
    int ConstraintRows, int EdgeCount, int Rank, int Nullity, int BasisCount,
    double SvdTolerance, double SpectralRadius,
    double MinNullEigenvalue, double MaxNullEigenvalue, double MaxClosedResidual, double MaxCoClosedResidual,
    double Star1OrthonormalResidual, int PositiveStar1Count) : IValidityEvidence {
    private const double ResidualSlack = 1.0e3;
    public bool IsValid {
        get {
            double residualTolerance = Math.Max(SvdTolerance, EpsilonPolicy.SqrtEpsilon * Math.Max(1.0, SpectralRadius)) * ResidualSlack;
            return ValidityClaim.All(
                EdgeCount >= 0 && Rank >= 0 && Nullity >= 0 && BasisCount >= 0 && ConstraintRows >= 0,
                (long)Rank + Nullity == EdgeCount,
                BasisCount <= Nullity,
                PositiveStar1Count >= 0 && PositiveStar1Count <= EdgeCount,
                ValidityClaim.Positive(SvdTolerance),
                ValidityClaim.Nonnegative(SpectralRadius),
                ValidityClaim.Finite(residualTolerance),
                ValidityClaim.Finite(MinNullEigenvalue) && MinNullEigenvalue >= -EpsilonPolicy.SqrtEpsilon,
                ValidityClaim.Finite(MaxNullEigenvalue) && MaxNullEigenvalue >= MinNullEigenvalue - EpsilonPolicy.SqrtEpsilon,
                ValidityClaim.Nonnegative(MaxClosedResidual) && MaxClosedResidual <= residualTolerance,
                ValidityClaim.Nonnegative(MaxCoClosedResidual) && MaxCoClosedResidual <= residualTolerance,
                ValidityClaim.Nonnegative(Star1OrthonormalResidual) && Star1OrthonormalResidual <= residualTolerance);
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
public readonly record struct DiscreteCalculus(SparseMatrix D0, SparseMatrix D1, Arr<double> Star0, Arr<double> Star1, Arr<double> Star2, DiscreteOperatorAssembly.ExteriorCalculusCase Assembly, Evidence<SignpostTransport> Transport, Option<HarmonicOneFormBasis> Harmonic = default) : IValidityEvidence {
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
                Star0.Count == D0.ColumnCount && Star1.Count == D0.RowCount && Star2.Count == D1.RowCount,
                TensorPrimitives.IsFiniteAll<double>(star0) && (star0.IsEmpty || TensorPrimitives.Min<double>(star0) > 0.0),
                TensorPrimitives.IsFiniteAll<double>(star1)
                    && (star1.IsEmpty || TensorPrimitives.Min<double>(star1) >= -(EpsilonPolicy.SqrtEpsilon * Math.Max(1.0, star1Scale))),
                TensorPrimitives.IsFiniteAll<double>(star2) && (star2.IsEmpty || TensorPrimitives.Min<double>(star2) > 0.0),
                Transport is { } probe && ValidityClaim.Evidence(evidence: probe.Value()),
                ValidityClaim.Evidence(evidence: Harmonic));
        }
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
}
```

## [04]-[DESCRIPTOR_ALGEBRA]

- Owner: `SpectralDescriptorPolicy` is the normalization bundle (`NormalizeScale` × normalization × `IncludeZeroModes` × optional crop) with `Raw` the no-op row, admitted at `SpectralFilter.Evaluate` beside the basis; `DescriptorProfile` carries the descriptor's filter, policy, and counts; `SpectralDescriptor` owns normalization and ranking over its values and provenance.
- Entry: `filter.Evaluate(basis, sources, policy)` is the evaluation entry; `descriptor.Normalize(normalization)` and `descriptor.Rank(candidates, normalization, distance)` are the post-processing entries, ranking accepting only the two caller choices it can apply while provenance remains on each descriptor.
- Auto: WKS weights normalize directly to unit sum and the wave arm answers its OWN failure; no diagnostic payload or log-eigenvalue allocation survives the normalized weights. `DescriptorProfile` retains only provenance needed to establish comparability.
- Law: ranking short-circuits on the first unusable or provenance-incompatible candidate rather than accumulating — filters, scale normalization, zero-mode inclusion, and crop policy must match before only the value-normalization axis is applied; a ranking is a JOINT verdict over the whole candidate set.
- Exemption: `Evaluate` is a declared statement kernel — a dense per-eigenpair-per-vertex accumulation buffer, where a query fold churns allocations over `n · k` (`· |S|` pairwise) terms. The exemption covers ALLOCATION shape alone: every per-vertex arm rides `TensorPrimitives` over one pooled lane, and the accumulation walks `ki → s → v` so a source ordinate is read once per pair instead of once per vertex.
- Law: `DescriptorProfile` folds `ValidityClaim.All` over the retained source, eigenpair, vertex, policy, and zero-mode couplings; wave normalization is admitted before construction and is not stored as duplicate diagnostics.
- Packages: MathNet.Numerics (`Distance` rows over one method-local raw/vector tuple per descriptor), System.Numerics.Tensors (`TensorPrimitives.Sum`/`Min`/`Max`/`MaxMagnitude`/`Subtract`/`Multiply`/`MultiplyAdd`/`Sqrt`/`Divide`/`IsFiniteAll` — every reduction, rescale, and accumulation on this cluster), CommunityToolkit.HighPerformance (`MemoryOwner<T>` the evaluation lane), LanguageExt.Core, Thinktecture.Runtime.Extensions.
- Growth: a new signature family is one filter case and policy rows, with `Evaluate` already generic over the weight function; a new distance is one `DescriptorDistance` row and a new normalization one `DescriptorNormalization` row.
- Boundary: evaluation is mesh-free, seeing vertex COUNT as its only topology, so it serves tet, grid, and mesh bases identically, while mesh-side basis computation and caching (`SpectralBasisBundle`) are `Meshing/dec`'s.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System;
using System.Linq;
using System.Numerics.Tensors;
using System.Runtime.InteropServices;
using LanguageExt;
using Rasm.Domain;
using static LanguageExt.Prelude;

namespace Rasm.Numerics;

// --- [MODELS] --------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct SpectralDescriptorPolicy(bool NormalizeScale, DescriptorNormalization Normalization, bool IncludeZeroModes, Option<Dimension> CropCount) : IValidityEvidence {
    public static SpectralDescriptorPolicy Raw => new(NormalizeScale: false, Normalization: DescriptorNormalization.Raw, IncludeZeroModes: true, CropCount: None);
    public bool IsValid => Normalization is not null;
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct DescriptorProfile(SpectralFilter Filter, int VertexCount, int EigenpairCount, int SourceCount,
    SpectralDescriptorPolicy Policy, int ZeroModeCount = 0) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        Filter is not null && VertexCount > 0 && EigenpairCount > 0,
        ZeroModeCount >= 0 && ZeroModeCount <= EigenpairCount
            && (Policy.IncludeZeroModes || ZeroModeCount < EigenpairCount)
            && (!Policy.NormalizeScale || ZeroModeCount < EigenpairCount),
        SourceCount >= 0 && SourceCount <= VertexCount,
        ValidityClaim.Evidence(evidence: Some(Policy)));
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct SpectralDescriptor(Arr<double> Values, DescriptorProfile Profile) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Evidence(evidence: Some(Profile)),
        ValidityClaim.CountExactly(count: Values.Count, expected: Profile.VertexCount),
        Values.ForAll(double.IsFinite));
    public Fin<SpectralDescriptor> Normalize(DescriptorNormalization normalization) =>
        !IsValid || normalization is null ? Fin.Fail<SpectralDescriptor>(new KernelFault.InvalidInput())
        : Profile.Policy.Normalization.Equals(normalization) ? Fin.Succ(this)
        : !Profile.Policy.Normalization.Equals(DescriptorNormalization.Raw) ? Fin.Fail<SpectralDescriptor>(new KernelFault.InvalidInput())
        : from values in normalization.Apply([.. Values.AsIterable()])
              .Filter(static normalized => TensorPrimitives.IsFiniteAll<double>(normalized))
              .ToFin(new KernelFault.InvalidResult())
          let merged = Profile.Policy with { Normalization = normalization }
          select new SpectralDescriptor(Values: new Arr<double>(values), Profile: Profile with { Policy = merged });

    public Fin<Seq<(int Index, double Distance)>> Rank(
        Seq<SpectralDescriptor> candidates, DescriptorNormalization normalization, DescriptorDistance distance) {
        SpectralDescriptor query = this;
        return !query.IsValid || normalization is null || distance is null || candidates.IsEmpty
                || !candidates.ForAll(candidate => candidate.IsValid
                    && candidate.Profile.Filter.Equals(query.Profile.Filter)
                    && (candidate.Profile.Policy with { Normalization = DescriptorNormalization.Raw })
                        .Equals(query.Profile.Policy with { Normalization = DescriptorNormalization.Raw }))
            ? Fin.Fail<Seq<(int Index, double Distance)>>(new KernelFault.InvalidInput())
            : from normalizedQuery in NormalizeForRanking(descriptor: query, normalization: normalization)
              from normalizedCandidates in candidates.TraverseM(candidate => NormalizeForRanking(descriptor: candidate, normalization: normalization)).As()
              from ranks in RankNormalized(query: normalizedQuery, candidates: normalizedCandidates, distance: distance)
              select ranks;

        static Fin<SpectralDescriptor> NormalizeForRanking(
            SpectralDescriptor descriptor, DescriptorNormalization normalization) =>
            descriptor.Profile.Policy.Normalization.Equals(normalization)
                ? Fin.Succ(descriptor)
                : descriptor.Normalize(normalization);

        static Fin<Seq<(int Index, double Distance)>> RankNormalized(
            SpectralDescriptor query, Seq<SpectralDescriptor> candidates, DescriptorDistance distance) {
            int valueCount = query.Values.Count;
            if (valueCount <= 0 || candidates.Exists(candidate => candidate.Values.Count != valueCount))
                return Fin.Fail<Seq<(int Index, double Distance)>>(new KernelFault.InvalidInput());
            double[] queryRaw = [.. query.Values.AsIterable()];
            var queryOperand = (Raw: queryRaw, Dense: MathNet.Numerics.LinearAlgebra.CreateVector.DenseOfArray(queryRaw));
            (int Index, double Distance)[] ranks = [.. candidates.AsIterable()
                .Select((SpectralDescriptor candidate, int index) => {
                    double[] raw = [.. candidate.Values.AsIterable()];
                    var operand = (Raw: raw, Dense: MathNet.Numerics.LinearAlgebra.CreateVector.DenseOfArray(raw));
                    return (Index: index, Distance: distance.Compute(a: queryOperand, b: operand));
                })
                .OrderBy(static rank => rank.Distance).ThenBy(static rank => rank.Index)];
            return System.Array.TrueForAll(ranks, static rank => double.IsFinite(rank.Distance))
                ? Fin.Succ(toSeq(ranks))
                : Fin.Fail<Seq<(int Index, double Distance)>>(new KernelFault.InvalidResult());
        }
    }
}
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
