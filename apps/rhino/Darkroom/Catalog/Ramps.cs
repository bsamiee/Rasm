using System.Numerics;
using System.Runtime.InteropServices;
using Darkroom.Chain;
using Rasm.Imaging.Color;
using Rasm.Imaging.Grade;
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

    public ColorRamp Glow(Gamut gamut) =>
        RampLight.Sampled(position => new Wacton.Unicolour.Temperature(double.Lerp(Cool, Hot, position)).Emission(gamut).RgbLinear switch {
            var rgb => new Vector4((float)rgb.R, (float)rgb.G, (float)rgb.B, 1f),
        });
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class RampLight {
    private const int Intervals = 256;

    public static ColorRamp Of(Seq<RampStop> ramp, PassContext context) =>
        ColorRamp.From(Lights(toSeq(Range(0, Intervals + 1)).Map(at => RampStop.At(ramp, at / (float)Intervals)), context)).ThrowIfFail();

    public static ColorRamp Sampled(Func<float, Vector4> light) => ColorRamp.Sampled(Intervals, light).ThrowIfFail();

    public static RampIntegral Integral(Seq<RampStop> ramp, Option<WaveProfile> profile, PassContext context) {
        const int bins = 4096;
        return RampIntegral.From(Lights(toSeq(Range(0, bins)).Map(bin =>
            RampStop.At(ramp, profile.Match(Some: wave => wave.Level((bin + 0.5f) / bins), None: () => (bin + 0.5f) / bins))), context)).ThrowIfFail();
    }

    public static ImmutableArray<Vector4> Lights(Seq<Unicolour> colors, PassContext context) {
        Vector4[] lights = [.. colors.Map(color => color.ConvertToConfiguration(context.Working.Configuration) switch {
            var held => new Vector4((float)held.RgbLinear.R, (float)held.RgbLinear.G, (float)held.RgbLinear.B, (float)held.Alpha.A),
        })];
        if (context.Formed)
            context.Signal.Apply(lights, inverted: false);
        return ImmutableCollectionsMarshal.AsImmutableArray(lights);
    }

    public static Vector4 Light(Unicolour color, PassContext context) => Lights([color], context)[0];
}

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
