using Rasm.Domain;
using Rasm.TestKit;
using Rasm.Vectors;

namespace Rasm.Tests.Vectors;

// --- [CONSTANTS] ----------------------------------------------------------------------------
internal static class SpectralGens {
    public static readonly Op Key = Op.Of(name: "spectral-test");
    public static readonly SpectralBasis Basis = new(
        Eigenvalues: new Arr<double>([0.0, 2.0]),
        Eigenvectors: new Arr<Arr<double>>([
            new Arr<double>([1.0, 1.0, 1.0]),
            new Arr<double>([1.0, 0.0, -1.0]),
        ]));
    public static readonly SpectralBasis OverflowBasis = new(
        Eigenvalues: new Arr<double>([1.0]),
        Eigenvectors: new Arr<Arr<double>>([new Arr<double>([1.0e200, 1.0e200])]));
    public static readonly Gen<double> PositiveEigenvalue = Gen.Double[start: 0.25, finish: 4.0];
    public static PositiveMagnitude Positive(double value) =>
        Spec.SuccValue(Key.AcceptValidated<PositiveMagnitude>(candidate: value), label: "positive");
}

// --- [ALGEBRAIC] ----------------------------------------------------------------------------
public sealed class SpectralBasisLaws {
    [Fact]
    public void BasisValidityRejectsEmptyOrRaggedEigenRails() {
        Assert.True(condition: SpectralGens.Basis.IsValid);
        Assert.False(condition: new SpectralBasis(Eigenvalues: new Arr<double>([]), Eigenvectors: new Arr<Arr<double>>([])).IsValid);
        Assert.False(condition: new SpectralBasis(Eigenvalues: new Arr<double>([1.0]), Eigenvectors: new Arr<Arr<double>>([new Arr<double>([])])).IsValid);
        Assert.False(condition: new SpectralBasis(Eigenvalues: new Arr<double>([1.0, 2.0]), Eigenvectors: new Arr<Arr<double>>([
            new Arr<double>([1.0, 2.0]),
            new Arr<double>([1.0]),
        ])).IsValid);
    }
    [Fact]
    public void TruncationCannotProduceValidEmptyBasis() {
        Assert.Equal(expected: SpectralGens.Basis.Eigenvalues.Count, actual: SpectralGens.Basis.Truncate(k: 0).Eigenvalues.Count);
        Assert.Equal(expected: SpectralGens.Basis.Eigenvalues.Count, actual: SpectralGens.Basis.Truncate(k: -4).Eigenvalues.Count);
        Assert.Equal(expected: 1, actual: SpectralGens.Basis.Truncate(k: 1).Eigenvalues.Count);
    }
}

public sealed class SpectralFilterLaws {
    [Fact]
    public void IdentitySignatureAndPairwiseDistancesUseIndependentSpectralSums() {
        Spec.Succ(SpectralFilter.Identity.Apply(basis: SpectralGens.Basis, sources: Option<Seq<int>>.None, key: SpectralGens.Key), then: values =>
            Spec.SeqEqualWithin(left: toSeq(values.AsIterable()), right: Seq(2.0, 1.0, 2.0), tolerance: 1.0e-12, what: "signature"));
        Spec.Succ(SpectralFilter.Identity.Apply(basis: SpectralGens.Basis, sources: Some(Seq(0)), key: SpectralGens.Key), then: values =>
            Spec.SeqEqualWithin(left: toSeq(values.AsIterable()), right: Seq(0.0, 1.0, 2.0), tolerance: 1.0e-12, what: "pairwise"));
    }
    [Fact]
    public void SourceRailRejectsEmptyOrOutOfRangePairwiseInputs() {
        Spec.FailCategory(SpectralFilter.Identity.Apply(basis: SpectralGens.Basis, sources: Some(Seq<int>()), key: SpectralGens.Key), category: "Input");
        Spec.FailCategory(SpectralFilter.Identity.Apply(basis: SpectralGens.Basis, sources: Some(Seq(3)), key: SpectralGens.Key), category: "Input");
        Spec.FailCategory(SpectralFilter.Identity.Apply(basis: SpectralGens.Basis, sources: Some(Seq(-1)), key: SpectralGens.Key), category: "Input");
        Assert.True(condition: SpectralGens.OverflowBasis.IsValid);
        Spec.FailCategory(SpectralFilter.Identity.Apply(basis: SpectralGens.OverflowBasis, sources: Option<Seq<int>>.None, key: SpectralGens.Key), category: "Result");
    }
    [Fact]
    public void ClosedCompositionsAreAlgebraicAndUnsupportedPairsStayNone() {
        SpectralFilter heatA = SpectralFilter.Heat(time: SpectralGens.Positive(value: 0.25));
        SpectralFilter heatB = SpectralFilter.Heat(time: SpectralGens.Positive(value: 0.75));
        Spec.Some(heatA.Compose(other: heatB), composed =>
            Spec.EqualWithin(left: ((SpectralFilter.HeatCase)composed).Time.Value, right: 1.0, tolerance: 1.0e-12, what: "heat time"));
        Spec.Some(SpectralFilter.Identity.Compose(other: heatA), composed => Assert.Equal(expected: heatA, actual: composed));
        Spec.None(SpectralFilter.Wave(energy: SpectralGens.Positive(value: 1.0), bandwidth: SpectralGens.Positive(value: 0.2)).Compose(other: heatA));
    }
    [Fact]
    public void FilterWeightsMatchIndependentClosedForms() =>
        Spec.ForAll(SpectralGens.PositiveEigenvalue, static lambda => {
            PositiveMagnitude time = SpectralGens.Positive(value: 0.5);
            PositiveMagnitude energy = SpectralGens.Positive(value: 1.5);
            PositiveMagnitude bandwidth = SpectralGens.Positive(value: 0.75);
            double waveRatio = (Math.Log(d: energy.Value) - Math.Log(d: lambda)) / bandwidth.Value;
            Spec.EqualWithin(left: SpectralFilter.Heat(time: time).Weight(eigenvalue: lambda), right: Math.Exp(d: -time.Value * lambda), tolerance: 1.0e-12, what: "heat");
            Spec.EqualWithin(left: SpectralFilter.Diffusion(time: time).Weight(eigenvalue: lambda), right: Math.Exp(d: -2.0 * time.Value * lambda), tolerance: 1.0e-12, what: "diffusion");
            Spec.EqualWithin(left: SpectralFilter.Wave(energy: energy, bandwidth: bandwidth).Weight(eigenvalue: lambda), right: Math.Exp(d: -0.5 * waveRatio * waveRatio), tolerance: 1.0e-12, what: "wave");
            Spec.EqualWithin(left: SpectralFilter.Biharmonic.Weight(eigenvalue: lambda), right: 1.0 / (lambda * lambda), tolerance: 1.0e-12, what: "biharmonic");
            Spec.EqualWithin(left: SpectralFilter.CommuteTime.Weight(eigenvalue: lambda), right: 1.0 / lambda, tolerance: 1.0e-12, what: "commute");
            Spec.EqualWithin(left: SpectralFilter.Identity.Weight(eigenvalue: lambda), right: 1.0, tolerance: 0.0, what: "identity");
        });
}
