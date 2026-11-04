using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using Rasm.Imaging.Color;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Tone;

namespace Rasm.Imaging.Grade;

// --- [MODELS] --------------------------------------------------------------------------
public sealed class ColorRamp {
    private ColorRamp(ImmutableArray<Vector4> samples) => Samples = samples;

    public ImmutableArray<Vector4> Samples { get; }

    public static Fin<ColorRamp> From(ImmutableArray<Vector4> samples) =>
        ImagingFault.Unless(!samples.IsDefaultOrEmpty, ImagingReason.Empty, samples).Map(_ => new ColorRamp(samples));

    public static Fin<ColorRamp> Sampled(int intervals, Func<float, Vector4> sample) =>
        ImagingFault.Unless(intervals > 0 && intervals < System.Array.MaxLength, ImagingReason.Range, intervals)
            .Map(_ => new ColorRamp([.. Enumerable.Range(0, intervals + 1).Select(at => sample(at / (float)intervals))]));

    public Vector4 Sample(float position) =>
        float.IsNaN(position) ? new Vector4(float.NaN)
            : Samples.Length is 1 ? Samples[0]
            : (float.Clamp(position, 0f, 1f) * (Samples.Length - 1)) switch {
                var texel => int.Min((int)texel, Samples.Length - 2) switch {
                    var at => Vector4.Lerp(Samples[at], Samples[at + 1], texel - at),
                },
            };

    public Action<Span<Vector4>> Graded(Gamut gamut) =>
        gamut.Luminance switch {
            var weights => ToneCurve.Graded(color => Sample(float.Cbrt(Vector3.Dot(weights, color))) switch {
                var mapped => Vector3.Lerp(color, mapped.AsVector3(), mapped.W),
            }),
        };
}

public sealed class RampIntegral {
    private readonly ImmutableArray<Vector4> bins;
    private readonly ImmutableArray<Vector256<double>> sums;

    private RampIntegral(ImmutableArray<Vector4> samples) {
        Vector4[] associated = GC.AllocateUninitializedArray<Vector4>(samples.Length);
        PixelFrame.Associated(samples.AsSpan(), associated);
        bins = ImmutableCollectionsMarshal.AsImmutableArray(associated);
        sums = [.. toSeq(bins).Scan(Vector256<double>.Zero, static (sum, bin) => sum + Widen(bin))];
    }

    public static Fin<RampIntegral> From(ImmutableArray<Vector4> samples) =>
        ImagingFault.Unless(!samples.IsDefaultOrEmpty, ImagingReason.Empty, samples).Map(_ => new RampIntegral(samples));

    public static Fin<RampIntegral> Sampled(int bins, Func<float, Vector4> sample) =>
        ImagingFault.Unless(bins > 0 && bins < System.Array.MaxLength, ImagingReason.Range, bins)
            .Map(_ => new RampIntegral([.. Enumerable.Range(0, bins).Select(at => sample((at + 0.5f) / bins))]));

    public Vector4 Mean(double position, double footprint) {
        if (double.IsPositiveInfinity(footprint))
            return Straight((Widen(bins[0]) + Widen(bins[^1])) / 2d);
        if (footprint > 0d && footprint * bins.Length <= 1d)
            return CellMean(position, footprint, periodic: false);
        (double low, double high, double total) = (position - (footprint / 2d), position + (footprint / 2d), footprint * bins.Length);
        return Straight(high <= 0d ? Widen(bins[0]) : low >= 1d ? Widen(bins[^1])
            : (low < 0d ? Widen(bins[0]) * ((double.Min(0d, high) - low) / footprint) : Vector256<double>.Zero)
                + (high > 1d ? Widen(bins[^1]) * ((high - double.Max(low, 1d)) / footprint) : Vector256<double>.Zero)
                + Segment(double.Max(low, 0d) * bins.Length, double.Min(high, 1d) * bins.Length, total));
    }

    public Vector4 PeriodicMean(double position, double footprint) {
        if (double.IsPositiveInfinity(footprint))
            return Average;
        if (footprint > 0d && footprint * bins.Length <= 1d)
            return CellMean(position, footprint, periodic: true);
        (double low, double total, double periods) = (position - (footprint / 2d), footprint * bins.Length, double.Floor(footprint));
        double start = low - double.Floor(low);
        double end = start + (footprint - periods);
        return Straight((sums[^1] * (periods / footprint / bins.Length))
            + Segment(start * bins.Length, double.Min(end, 1d) * bins.Length, total)
            + (end > 1d ? Segment(0d, (end - 1d) * bins.Length, total) : Vector256<double>.Zero));
    }

    private Vector4 Average => Straight(sums[^1] / bins.Length);

    private Vector4 CellMean(double position, double footprint, bool periodic) {
        double texel = (periodic ? position - double.Floor(position) : double.Clamp(position, 0d, 1d)) * bins.Length;
        int last = bins.Length - 1;
        int at = int.Min((int)texel, last);
        (double fraction, double width) = (texel - at, footprint * bins.Length);
        (double before, double after) = (double.Max(0.5d - (fraction / width), 0d), double.Max(0.5d - ((1d - fraction) / width), 0d));
        (int previous, int next) = periodic ? (at is 0 ? last : at - 1, at == last ? 0 : at + 1) : (int.Max(at - 1, 0), int.Min(at + 1, last));
        return Straight((Widen(bins[at]) * (1d - before - after)) + (Widen(bins[previous]) * before) + (Widen(bins[next]) * after));
    }

    private static Vector256<double> Widen(Vector4 value) => Vector256.WidenLower(value.AsVector128().ToVector256());

    private static Vector4 Straight(Vector256<double> value) {
        Vector4 mean = Vector128.Narrow(value.GetLower(), value.GetUpper()).AsVector4();
        PixelFrame.Unassociated(new Span<Vector4>(ref mean));
        return mean;
    }

    private Vector256<double> Segment(double low, double high, double total) {
        if (low >= high)
            return Vector256<double>.Zero;
        (int first, int last) = ((int)double.Floor(low), (int)double.Floor(high));
        return (Widen(bins[first]) * ((double.Min(high, first + 1) - low) / total))
            + (last > first + 1 ? (sums[last] - sums[first + 1]) * (1d / total) : Vector256<double>.Zero)
            + (last > first && high > last ? Widen(bins[last]) * ((high - last) / total) : Vector256<double>.Zero);
    }
}
