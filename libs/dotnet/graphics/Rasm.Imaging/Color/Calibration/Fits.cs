using System.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Factorization;

namespace Rasm.Imaging.Color.Calibration;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ChartTransform(Matrix4x4 Matrix, float Stops) {
    public Fin<ChartTransform> Inverse => Matrix4x4.Invert(Matrix, out Matrix4x4 inverse) ? new ChartTransform(inverse, -Stops) : ImagingFault.Of(ImagingReason.NoInverse, Matrix);
}

public sealed record ChartFit {
    private ChartFit(ChartTransform transform, double condition, Fin<ChartTransform> normalized) => (Transform, Condition, Normalized) = (transform, condition, normalized);

    public ChartTransform Transform { get; }
    public double Condition { get; }
    public Fin<ChartTransform> Normalized { get; }

    public static Fin<ChartFit> Solve(ChartSamples source, ChartSamples target) {
        Svd<double> fitted = Rows(source).Svd();
        return
            from solved in (ImagingFault.Unless(source.Chart == target.Chart, ImagingReason.Mismatch, (source.Chart, target.Chart)), ImagingFault.Unless(fitted.Rank is 3, ImagingReason.Degenerate, fitted.Rank))
                .Apply((_, _) => fitted.Solve(Rows(target))).As()
            let decomposed = solved.Svd()
            from transform in ImagingFault.Unless(decomposed.Rank is 3, ImagingReason.Degenerate, decomposed.Rank).Map(_ => Lowered(solved.ToRowMajorArray()))
            let neutrals = (Source: Neutral(source), Target: Neutral(target))
            select new ChartFit(new(transform, 0f), decomposed.ConditionNumber, neutrals.Source > 0f && neutrals.Target > 0f
                ? new ChartTransform(Matrix4x4.CreateScale(neutrals.Source / neutrals.Target) * transform, float.Log2(neutrals.Target / neutrals.Source))
                : ImagingFault.Of(ImagingReason.Range, neutrals));
    }

    private static Matrix<double> Rows(ChartSamples samples) => Matrix<double>.Build.DenseOfRowArrays(samples.Patches.Map(static patch => new double[] { patch.X, patch.Y, patch.Z }));

    private static float Neutral(ChartSamples samples) => Vector3.Dot(samples.Gamut.Luminance, samples.Patches[samples.Chart.Neutral]);

    private static Matrix4x4 Lowered(double[] m) =>
        new((float)m[0], (float)m[1], (float)m[2], 0f, (float)m[3], (float)m[4], (float)m[5], 0f, (float)m[6], (float)m[7], (float)m[8], 0f, 0f, 0f, 0f, 1f);
}
