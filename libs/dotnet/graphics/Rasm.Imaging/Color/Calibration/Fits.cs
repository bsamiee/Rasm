using System.Globalization;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Factorization;
using Rasm.Imaging.Tone;

namespace Rasm.Imaging.Color.Calibration;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ChartTransform(Matrix<double> Matrix, double Stops);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FitTarget {
    public sealed record Reference(Gamut Gamut) : FitTarget;
    public sealed record Sampled(ChartSamples Samples) : FitTarget;
}

public sealed record FitQuality((double Largest, double Middle, double Smallest) SampleValues, (double Largest, double Middle, double Smallest) TransformValues) {
    public double Condition => TransformValues.Largest / TransformValues.Smallest;
    public bool IllConditioned => Condition > 1e4;
}

public sealed record ChartFit {
    private readonly (double Source, double Target) neutrals;

    private ChartFit(Matrix<double> forward, Matrix<double> inverse, FitQuality quality, (double Source, double Target) neutrals) =>
        (Forward, Inverse, Quality, this.neutrals) = (forward, inverse, quality, neutrals);

    public Matrix<double> Forward { get; }
    public Matrix<double> Inverse { get; }
    public FitQuality Quality { get; }

    public Fin<double> Gain =>
        neutrals switch {
            ( > 0d, > 0d) and var (source, target) => target / source,
            var (source, target) => Error.New(string.Create(CultureInfo.InvariantCulture, $"gain needs positive neutral patch luminances, not {source} and {target}")),
        };

    public ChartTransform Transform(FitDirection direction) => direction.Of(this, 1d);
    public Fin<ChartTransform> Normalized(FitDirection direction) => Gain.Map(gain => direction.Of(this, gain));

    public static Fin<ChartFit> Solve(ChartSamples source, FitTarget target) {
        (Matrix<double> rows, double neutral) = Operand(source.Gamut, source.Colors);
        Svd<double> fitted = rows.Svd();
        return
            from targets in (
                    target.Switch<ColorChart, Fin<(Matrix<double> Rows, double Neutral)>>(
                        source.Chart,
                        reference: static (chart, reference) => Operand(reference.Gamut, chart.References(reference.Gamut)),
                        sampled: static (chart, sampled) =>
                            sampled.Samples.Chart == chart ? Operand(sampled.Samples.Gamut, sampled.Samples.Colors) : Error.New($"source chart {chart} differs from target chart {sampled.Samples.Chart}")),
                    fitted.Rank is 3
                        ? Fin.Succ(unit)
                        : Error.New(string.Create(CultureInfo.InvariantCulture, $"chart samples span {fitted.Rank} of three color directions")))
                .Apply(static (operand, _) => operand)
                .As()
            let transform = fitted.Solve(targets.Rows)
            let decomposed = transform.Svd()
            from _ in guard(decomposed.Rank is 3, Error.New(string.Create(CultureInfo.InvariantCulture, $"fitted transform is singular at rank {decomposed.Rank}")))
            select new ChartFit(
                transform.Transpose(), decomposed.Solve(Matrix<double>.Build.DenseIdentity(3)).Transpose(),
                new FitQuality((fitted.S[0], fitted.S[1], fitted.S[2]), (decomposed.S[0], decomposed.S[1], decomposed.S[2])),
                (neutral, targets.Neutral));
    }

    private static (Matrix<double> Rows, double Neutral) Operand(Gamut gamut, HashMap<ChartPatch, (double R, double G, double B)> colors) =>
        (Matrix<double>.Build.DenseOfRowArrays(toSeq(ChartPatch.Items).Map(patch => colors[patch] switch { var (r, g, b) => new[] { r, g, b } })),
         colors[ChartPatch.Neutral5] switch {
             var (r, g, b) => LuminanceSamples.Luminance(r, g, b, gamut.Luminance),
         });
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FitDirection {
    public static readonly FitDirection Forward = new("forward", static (fit, gain) => new(fit.Forward.Divide(gain), Math.Log2(gain)));
    public static readonly FitDirection Inverse = new("inverse", static (fit, gain) => new(fit.Inverse.Multiply(gain), -Math.Log2(gain)));

    [UseDelegateFromConstructor]
    internal partial ChartTransform Of(ChartFit fit, double gain);
}
