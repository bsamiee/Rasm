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
    public static SpectralDescriptor Descriptor(params double[] values) =>
        new(Values: new Arr<double>(values), Receipt: new SpectralDescriptorReceipt(Filter: SpectralFilter.Identity, VertexCount: values.Length, EigenpairCount: values.Length, SourceCount: 0, ComparisonReady: false, Pairwise: false, EnergyNormalized: false, BandwidthNormalized: false, Policy: SpectralDescriptorPolicy.Raw));
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
        Assert.False(condition: new SpectralBasis(Eigenvalues: new Arr<double>([double.NaN]), Eigenvectors: new Arr<Arr<double>>([new Arr<double>([1.0])])).IsValid);
        Assert.False(condition: new SpectralBasis(Eigenvalues: new Arr<double>([-Rhino.RhinoMath.SqrtEpsilon * 2.0]), Eigenvectors: new Arr<Arr<double>>([new Arr<double>([1.0])])).IsValid);
        Assert.True(condition: new SpectralBasis(Eigenvalues: new Arr<double>([-Rhino.RhinoMath.SqrtEpsilon * 0.5]), Eigenvectors: new Arr<Arr<double>>([new Arr<double>([1.0])])).IsValid);
    }
    [Fact]
    public void TruncationPreservesPrefixAndCannotProduceValidEmptyBasis() {
        Assert.Equal(expected: SpectralGens.Basis.Eigenvalues.Count, actual: SpectralGens.Basis.Truncate(k: 0).Eigenvalues.Count);
        Assert.Equal(expected: SpectralGens.Basis.Eigenvalues.Count, actual: SpectralGens.Basis.Truncate(k: -4).Eigenvalues.Count);
        Assert.Equal(expected: 1, actual: SpectralGens.Basis.Truncate(k: 1).Eigenvalues.Count);
        Assert.True(condition: SpectralGens.Basis.Truncate(k: 1).IsValid);
        Assert.Equal(expected: SpectralGens.Basis.Eigenvalues[0], actual: SpectralGens.Basis.Truncate(k: 1).Eigenvalues[0]);
        Assert.Equal(expected: SpectralGens.Basis.Eigenvalues.Count, actual: SpectralGens.Basis.Truncate(k: 99).Eigenvalues.Count);
    }
}

public sealed class SpectralFilterLaws {
    [Fact]
    public void IdentitySignatureAndPairwiseDistancesUseIndependentSpectralSums() {
        Spec.Succ(SpectralFilter.Identity.ApplyDetailed(basis: SpectralGens.Basis, sources: Option<Seq<int>>.None, key: SpectralGens.Key), then: descriptor => {
            Spec.Equal(left: toSeq(descriptor.Values.AsIterable()), right: Seq(2.0, 1.0, 2.0), tolerance: 1.0e-12, what: "signature");
            Assert.False(condition: descriptor.Receipt.ComparisonReady);
            Assert.False(condition: descriptor.Receipt.EnergyNormalized);
            Assert.False(condition: descriptor.Receipt.BandwidthNormalized);
        });
        Spec.Succ(SpectralFilter.Identity.ApplyDetailed(basis: SpectralGens.Basis, sources: Some(Seq(0)), key: SpectralGens.Key), then: descriptor => {
            Spec.Equal(left: toSeq(descriptor.Values.AsIterable()), right: Seq(0.0, 1.0, 2.0), tolerance: 1.0e-12, what: "pairwise");
            Assert.True(condition: descriptor.Receipt.Pairwise);
            Assert.Equal(expected: 1, actual: descriptor.Receipt.SourceCount);
        });
    }
    [Fact]
    public void DescriptorPolicyNormalizesZeroModesAndEnergyForComparison() {
        SpectralDescriptorPolicy policy = new(
            ScaleNormalization: SpectralScaleNormalization.FirstNonZeroEigenvalue,
            EnergyNormalization: SpectralEnergyNormalization.UnitL2,
            ZeroModePolicy: SpectralZeroModePolicy.Drop,
            CropCount: Some(Dimension.Create(value: 1)));
        Spec.Succ(SpectralFilter.Identity.ApplyDetailed(basis: SpectralGens.Basis, sources: Option<Seq<int>>.None, policy: policy, key: SpectralGens.Key), descriptor => {
            double invRoot2 = 1.0 / Math.Sqrt(d: 2.0);
            Spec.Equal(left: toSeq(descriptor.Values.AsIterable()), right: Seq(invRoot2, 0.0, invRoot2), tolerance: 1.0e-12, what: "normalized identity");
            Assert.True(condition: descriptor.Receipt.ComparisonReady);
            Assert.True(condition: descriptor.Receipt.EnergyNormalized);
            Assert.True(condition: descriptor.Receipt.BandwidthNormalized);
            Assert.True(condition: descriptor.Receipt.Wave.IsNone);
            Assert.Equal(expected: 1, actual: descriptor.Receipt.ZeroModeCount);
            Assert.Equal(expected: 1, actual: descriptor.Receipt.CroppedEigenpairCount);
            Assert.Equal(expected: policy.ScaleNormalization, actual: descriptor.Receipt.Policy.ScaleNormalization);
            Assert.Equal(expected: policy.EnergyNormalization, actual: descriptor.Receipt.Policy.EnergyNormalization);
            Assert.Equal(expected: policy.ZeroModePolicy, actual: descriptor.Receipt.Policy.ZeroModePolicy);
            Spec.Some(policy.CropCount, expected => Spec.Some(descriptor.Receipt.Policy.CropCount, actual => Assert.Equal(expected: expected.Value, actual: actual.Value)));
        });
        Spec.FailCategory(SpectralFilter.Identity.ApplyDetailed(basis: SpectralGens.Basis, sources: Option<Seq<int>>.None, policy: default, key: SpectralGens.Key), category: "Input");
    }
    [Fact]
    public void RankingUsesPolicyDistanceAndOriginalOrderTieBreak() {
        SpectralDescriptor query = SpectralGens.Descriptor(1.0, 0.0, 0.0);
        Seq<SpectralDescriptor> candidates = Seq(SpectralGens.Descriptor(0.0, 1.0, 0.0), SpectralGens.Descriptor(1.0, 0.0, 0.0), SpectralGens.Descriptor(1.0, 0.0, 0.0));
        SpectralRankingPolicy policy = new(Descriptor: new SpectralDescriptorPolicy(ScaleNormalization: SpectralScaleNormalization.Raw, EnergyNormalization: SpectralEnergyNormalization.UnitL2, ZeroModePolicy: SpectralZeroModePolicy.Keep, CropCount: Option<Dimension>.None), Distance: SpectralDistanceKind.Euclidean, TieBreak: SpectralTieBreak.InputOrder);
        Spec.Succ(query.Rank(candidates: candidates, policy: policy, key: SpectralGens.Key), ranking => {
            Assert.Equal(expected: 3, actual: ranking.Items.Count);
            Assert.Equal(expected: 1, actual: ranking.Items[0].Index);
            Assert.Equal(expected: 2, actual: ranking.Items[1].Index);
            Assert.Equal(expected: 0, actual: ranking.Items[2].Index);
            Assert.True(condition: ranking.Query.Receipt.ComparisonReady);
        });
        Spec.FailCategory(query.Normalize(policy: policy.Descriptor with { ScaleNormalization = SpectralScaleNormalization.FirstNonZeroEigenvalue }, key: SpectralGens.Key), category: "Unsupported");
    }
    [Fact]
    public void SourceRailRejectsEmptyOrOutOfRangePairwiseInputs() {
        Spec.FailCategory(SpectralFilter.Identity.ApplyDetailed(basis: SpectralGens.Basis, sources: Some(Seq<int>()), key: SpectralGens.Key), category: "Input");
        Spec.FailCategory(SpectralFilter.Identity.ApplyDetailed(basis: SpectralGens.Basis, sources: Some(Seq(0, 0)), key: SpectralGens.Key), category: "Input");
        Spec.FailCategory(SpectralFilter.Identity.ApplyDetailed(basis: SpectralGens.Basis, sources: Some(Seq(3)), key: SpectralGens.Key), category: "Input");
        Spec.FailCategory(SpectralFilter.Identity.ApplyDetailed(basis: SpectralGens.Basis, sources: Some(Seq(-1)), key: SpectralGens.Key), category: "Input");
        Assert.True(condition: SpectralGens.OverflowBasis.IsValid);
        Spec.FailCategory(SpectralFilter.Identity.ApplyDetailed(basis: SpectralGens.OverflowBasis, sources: Option<Seq<int>>.None, key: SpectralGens.Key), category: "Result");
    }
    [Fact]
    public void ClosedCompositionsAreAlgebraicAndUnsupportedPairsStayNone() {
        SpectralFilter heatA = SpectralFilter.Heat(time: SpectralGens.Positive(value: 0.25));
        SpectralFilter heatB = SpectralFilter.Heat(time: SpectralGens.Positive(value: 0.75));
        SpectralFilter heatC = SpectralFilter.Heat(time: SpectralGens.Positive(value: 0.5));
        Spec.Some(heatA.Compose(other: heatB), composed =>
            Spec.Equal(left: ((SpectralFilter.HeatCase)composed).Time.Value, right: 1.0, tolerance: 1.0e-12, what: "heat time"));
        Option<SpectralFilter> left = heatA.Compose(other: heatB).Bind(composed => composed.Compose(other: heatC));
        Spec.Some(heatB.Compose(other: heatC), bc => Spec.Some(left, l => Spec.Some(heatA.Compose(other: bc), r => Assert.Equal(expected: l, actual: r))));
        Spec.Some(SpectralFilter.Diffusion(time: SpectralGens.Positive(value: 0.25)).Compose(other: SpectralFilter.Diffusion(time: SpectralGens.Positive(value: 0.75))), composed =>
            Spec.Equal(left: ((SpectralFilter.DiffusionCase)composed).Time.Value, right: 1.0, tolerance: 1.0e-12, what: "diffusion time"));
        Spec.Some(SpectralFilter.Identity.Compose(other: heatA), composed => Assert.Equal(expected: heatA, actual: composed));
        Spec.Some(heatA.Compose(other: SpectralFilter.Identity), composed => Assert.Equal(expected: heatA, actual: composed));
        Spec.None(SpectralFilter.Wave(energy: SpectralGens.Positive(value: 1.0), bandwidth: SpectralGens.Positive(value: 0.2)).Compose(other: heatA));
        Spec.None(SpectralFilter.Biharmonic.Compose(other: heatA));
    }
    [Fact]
    public void FilterWeightsMatchIndependentClosedForms() {
        Spec.ForAll(SpectralGens.PositiveEigenvalue, static lambda => {
            PositiveMagnitude time = SpectralGens.Positive(value: 0.5);
            PositiveMagnitude energy = SpectralGens.Positive(value: 1.5);
            PositiveMagnitude bandwidth = SpectralGens.Positive(value: 0.75);
            double waveRatio = (Math.Log(d: energy.Value) - Math.Log(d: lambda)) / bandwidth.Value;
            Spec.Equal(left: SpectralFilter.Heat(time: time).Weight(eigenvalue: lambda), right: Math.Exp(d: -time.Value * lambda), tolerance: 1.0e-12, what: "heat");
            Spec.Equal(left: SpectralFilter.Diffusion(time: time).Weight(eigenvalue: lambda), right: Math.Exp(d: -2.0 * time.Value * lambda), tolerance: 1.0e-12, what: "diffusion");
            Spec.Equal(left: SpectralFilter.Wave(energy: energy, bandwidth: bandwidth).Weight(eigenvalue: lambda), right: Math.Exp(d: -0.5 * waveRatio * waveRatio), tolerance: 1.0e-12, what: "wave");
            Spec.Equal(left: SpectralFilter.Biharmonic.Weight(eigenvalue: lambda), right: 1.0 / (lambda * lambda), tolerance: 1.0e-12, what: "biharmonic");
            Spec.Equal(left: SpectralFilter.CommuteTime.Weight(eigenvalue: lambda), right: 1.0 / lambda, tolerance: 1.0e-12, what: "commute");
            Spec.Equal(left: SpectralFilter.Identity.Weight(eigenvalue: lambda), right: 1.0, tolerance: 0.0, what: "identity");
        });
        PositiveMagnitude energy = SpectralGens.Positive(value: 2.0);
        PositiveMagnitude bandwidth = SpectralGens.Positive(value: 0.75);
        Spec.Succ(SpectralFilter.Wave(energy: energy, bandwidth: bandwidth).ApplyDetailed(basis: SpectralGens.Basis, sources: Option<Seq<int>>.None, key: SpectralGens.Key), descriptor => {
            Assert.False(condition: descriptor.Receipt.BandwidthNormalized);
            Spec.Some(descriptor.Receipt.Wave, wave => {
                Assert.True(condition: wave.WksNormalized);
                Assert.Equal(expected: 1, actual: wave.ZeroModeCount);
                Assert.Equal(expected: 1, actual: wave.NonZeroEigenpairCount);
                Spec.Equal(left: wave.NormalizedWeightSum, right: 1.0, tolerance: 1.0e-12, what: "wks normalized sum");
                Spec.Some(wave.FirstNonZeroScale, scale => Spec.Equal(left: scale, right: 2.0, tolerance: 0.0, what: "first nonzero scale"));
            });
        });
    }
}
