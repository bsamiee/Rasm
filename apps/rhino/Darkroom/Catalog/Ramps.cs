using System.Numerics;
using System.Runtime.InteropServices;
using Darkroom.Chain;
using Rasm.Imaging.Color;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Procedural;
using Wacton.Unicolour;

namespace Darkroom.Catalog;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record RampStop(Unicolour Color, float Position) {
    public bool Eased { get; init; }

    public static Unicolour At(Seq<RampStop> ramp, float position) =>
        (ramp.Filter(stop => stop.Position <= position).Last.IfNone(ramp[0]), ramp.Find(stop => stop.Position >= position).IfNone(ramp[^1])) switch {
            var (low, high) when high.Position > low.Position => ((position - low.Position) / (high.Position - low.Position)) switch {
                var progress => low.Color.Mix(high.Color, ColourSpace.RgbLinear, high.Eased ? DistanceField.SmoothStep(0f, 1f, progress) : progress),
            },
            var (low, _) => low.Color,
        };
}

public sealed record Blackbody(double Cool, double Hot) {
    public static Blackbody Default { get; } = new(800d, 2000d);

    public RampLight Glow(Gamut gamut) =>
        RampLight.Sampled(position => new Wacton.Unicolour.Temperature(double.Lerp(Cool, Hot, position)).Emission(gamut).RgbLinear switch {
            var rgb => new Vector4((float)rgb.R, (float)rgb.G, (float)rgb.B, 1f),
        });
}

public sealed class RampLight {
    private const int Intervals = 256;

    private RampLight(ImmutableArray<Vector4> texels) => Texels = texels;

    public ImmutableArray<Vector4> Texels { get; }

    public static RampLight Of(Seq<RampStop> ramp, PassContext context) => new(Lights(Positions.Map(position => RampStop.At(ramp, position)), context));

    public static RampLight Sampled(Func<float, Vector4> light) => new([.. Positions.Map(light)]);

    public static ImmutableArray<Vector4> Lights(Seq<Unicolour> colors, PassContext context) {
        Vector4[] lights = [.. colors.Map(color => color.ConvertToConfiguration(context.Working.Configuration) switch {
            var held => new Vector4((float)held.RgbLinear.R, (float)held.RgbLinear.G, (float)held.RgbLinear.B, (float)held.Alpha.A),
        })];
        if (context.Formed)
            context.Signal.Apply(lights, inverted: false);
        return ImmutableCollectionsMarshal.AsImmutableArray(lights);
    }

    public static Vector4 Light(Unicolour color, PassContext context) => Lights([color], context)[0];

    public Vector4 Sample(float position) =>
        float.IsNaN(position) ? new Vector4(float.NaN)
            : (float.Clamp(position, 0f, 1f) * Intervals) switch { var texel => int.Min((int)texel, Intervals - 1) switch { var at => Vector4.Lerp(Texels[at], Texels[at + 1], texel - at) } };

    private static Seq<float> Positions => toSeq(Range(0, Intervals + 1)).Map(static texel => texel / (float)Intervals);
}

public sealed class RampFootprint {
    private const int Bins = 4096;

    private readonly Arr<Vector4> bins;
    private readonly Arr<Vector4> sums;

    private RampFootprint(Arr<Vector4> bins) =>
        (this.bins, sums) = (bins, [.. toSeq(bins).Scan((X: 0d, Y: 0d, Z: 0d, W: 0d), static (sum, bin) => (sum.X + bin.X, sum.Y + bin.Y, sum.Z + bin.Z, sum.W + bin.W))
            .Map(static sum => new Vector4((float)sum.X, (float)sum.Y, (float)sum.Z, (float)sum.W))]);

    public static RampFootprint Of(Seq<RampStop> ramp, Option<WaveProfile> profile, PassContext context) =>
        new([.. RampLight.Lights(toSeq(Range(0, Bins)).Map(bin => RampStop.At(ramp, profile.Match(Some: wave => wave.Level((bin + 0.5f) / Bins), None: () => (bin + 0.5f) / Bins))), context)
            .Select(static light => new Vector4(light.AsVector3() * light.W, light.W))]);

    public Vector4 Mean(double position, double footprint, bool periodic) {
        if (double.IsPositiveInfinity(footprint))
            return Straight(sums[Bins] / Bins);
        (double low, double high, double total) = (position - (footprint / 2d), position + (footprint / 2d), footprint * Bins);
        if (periodic) {
            double periods = double.Floor(footprint);
            double start = low - double.Floor(low);
            double end = start + (footprint - periods);
            return Straight((sums[Bins] * (float)(periods / total))
                + Segment(start * Bins, double.Min(end, 1d) * Bins, total)
                + (end > 1d ? Segment(0d, (end - 1d) * Bins, total) : Vector4.Zero));
        }
        return Straight((low < 0d ? bins[0] * (float)((double.Min(0d, high) - low) / footprint) : Vector4.Zero)
            + (high > 1d ? bins[Bins - 1] * (float)((high - double.Max(low, 1d)) / footprint) : Vector4.Zero)
            + Segment(double.Max(low, 0d) * Bins, double.Min(high, 1d) * Bins, total));
    }

    private static Vector4 Straight(Vector4 mean) {
        PixelFrame.Unassociated(new Span<Vector4>(ref mean));
        return mean;
    }

    private Vector4 Segment(double low, double high, double total) {
        if (low >= high)
            return Vector4.Zero;
        (int first, int last) = ((int)double.Floor(low), (int)double.Floor(high));
        return (bins[first] * (float)((double.Min(high, first + 1) - low) / total))
            + (last > first + 1 ? (sums[last] - sums[first + 1]) * (float)(1d / total) : Vector4.Zero)
            + (last > first && high > last ? bins[last] * (float)((high - last) / total) : Vector4.Zero);
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Presets {
    public static Seq<RampStop> Grayscale { get; } = [new(new Unicolour(ColourSpace.Rgb, 0d, 0d, 0d), 0f), new(new Unicolour(ColourSpace.Rgb, 1d, 1d, 1d), 1f)];
    public static Seq<RampStop> White { get; } = [new(new Unicolour(ColourSpace.Rgb, 1d, 1d, 1d), 0f)];

    public static float Map(float value, (float Low, float High) edges) => float.Clamp((value - edges.Low) / (edges.High - edges.Low), 0f, 1f);

    public static Seq<RampStop> Sampled(Func<float, Vector4> light, params ReadOnlySpan<float> positions) =>
        [.. toSeq(positions.ToArray()).Map(position => light(position) switch { var held => new RampStop(new Unicolour(ColourSpace.RgbLinear, held.X, held.Y, held.Z, held.W), position) })];

    public static Seq<RampStop> Threshold(Vector3 light, float low, float high, bool eased = false) =>
        [new(new Unicolour(ColourSpace.RgbLinear, light.X, light.Y, light.Z, 0d), low), new(new Unicolour(ColourSpace.RgbLinear, light.X, light.Y, light.Z, 1d), high) { Eased = eased }];

    public static Seq<RampStop> Solid(Vector3 light) => Sampled(_ => new Vector4(light, 1f), 0f);
}
