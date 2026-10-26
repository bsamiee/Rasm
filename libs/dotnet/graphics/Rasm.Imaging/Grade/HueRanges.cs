using System.Numerics;
using Rasm.Imaging.Color;
using Rasm.Imaging.Grade.Curves;
using Rasm.Imaging.Tone;

namespace Rasm.Imaging.Grade;

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct Hsy(double Hue, double Saturation, double Luma) {
    private const double Knee = 0.15d;
    private const double LowGain = 5d;
    private const double Scale = 1.4d;
    private const double MinLuma = 0.001d;
    private const double MaxLuma = 0.01d;

    private static readonly (double R, double G, double B) LumaWeights = Gamut.StandardRgb.Luminance;

    public static double SaturationPeak =>
        LumaWeights switch {
            var weights => Scale * (1d + ((weights.R + weights.G + weights.B) / double.Min(double.Min(weights.R, weights.G), weights.B))) / 3d,
        };

    public static Hsy From(Vector4 color) {
        (double red, double green, double blue) = (color.X, color.Y, color.Z);
        double luma = Luminance(red, green, blue);
        double distance = double.Abs(red - luma) + double.Abs(green - luma) + double.Abs(blue - luma);
        double max = double.Max(double.Max(red, green), blue);
        double min = double.Min(double.Min(red, green), blue);
        double sextant = min == max ? 0d
            : red == max ? 1d + ((green - blue) / (max - min))
            : green == max ? 3d + ((blue - red) / (max - min))
            : 5d + ((red - green) / (max - min));
        double turn = sextant / 6d;
        return new((turn - double.Floor(turn)) * double.Tau, distance * SaturationGain(luma), luma);
    }

    public static Vector3 Saturated(double hue) =>
        HueRgb(hue) switch {
            var (red, green, blue) => new((float)red, (float)green, (float)blue),
        };

    public Vector4 ToRgb(float alpha) {
        (double red, double green, double blue) = HueRgb(Hue);
        double luma = Luminance(red, green, blue);
        (red, green, blue) = (red - luma, green - luma, blue - luma);
        double gain = Saturation / SaturationGain(Luma) / (double.Abs(red) + double.Abs(green) + double.Abs(blue));
        return new((float)(Luma + (gain * red)), (float)(Luma + (gain * green)), (float)(Luma + (gain * blue)), alpha);
    }

    private static double Luminance(double red, double green, double blue) =>
        LuminanceSamples.Luminance(red, green, blue, LumaWeights) / (LumaWeights.R + LumaWeights.G + LumaWeights.B);

    private static double SaturationGain(double luma) =>
        luma switch {
            <= MinLuma => Scale * LowGain,
            >= MaxLuma => Scale / (Knee + (3d * luma)),
            _ => ((luma - MinLuma) / (MaxLuma - MinLuma)) switch {
                var blend => Scale * (((1d - blend) * LowGain) + (blend / (Knee + (3d * luma)))),
            },
        };

    private static (double Red, double Green, double Blue) HueRgb(double hue) =>
        ((hue % double.Tau / double.Tau) - (1d / 6d)) switch {
            var turn => ((turn - double.Floor(turn)) * 6d) switch {
                < 1d and var sextant => (1d, sextant, 0d),
                < 2d and var sextant => (2d - sextant, 1d, 0d),
                < 3d and var sextant => (0d, 1d, sextant - 2d),
                < 4d and var sextant => (0d, 4d - sextant, 1d),
                < 5d and var sextant => (sextant - 4d, 0d, 1d),
                var sextant => (1d, 0d, 6d - sextant),
            },
        };
}

public readonly record struct RangeBand(float Center, float Width, float Softness) {
    public float Membership(float distance) =>
        (distance - (Width / 2f)) switch {
            <= 0f => 1f,
            var outside when outside >= Softness => 0f,
            var outside => (outside / Softness) switch { var t => 1f - (t * t * (3f - (2f * t))) },
        };
}

public sealed record HueRange(RangeBand Hue, RangeBand Saturation, RangeBand Luma) {
    public static HueRange Open { get; } = new RangeBand(0.5f, 1f, 0f) switch { var open => new HueRange(open, open, open) };

    public float Membership(Hsy hsy) =>
        RangeAxis.Hue.Membership(Hue, hsy) * RangeAxis.Saturation.Membership(Saturation, hsy) * RangeAxis.Luma.Membership(Luma, hsy);

    public void Graded(Span<Vector4> row, float shift, float saturation, float stops) {
        float gain = float.Exp2(stops);
        foreach (ref Vector4 pixel in row) {
            Hsy hsy = Hsy.From(pixel);
            Vector3 graded = shift is 0f && saturation is 1f
                ? pixel.AsVector3() * gain
                : (hsy with { Hue = hsy.Hue + shift, Saturation = hsy.Saturation * saturation }).ToRgb(pixel.W).AsVector3() * gain;
            pixel = new Vector4(Vector3.Lerp(pixel.AsVector3(), graded, Membership(hsy)), pixel.W);
        }
    }

    public void Isolated(Span<Vector4> row, (double R, double G, double B) luminance) {
        foreach (ref Vector4 pixel in row) {
            float membership = Membership(Hsy.From(pixel));
            double gray = (1d - membership) * LuminanceSamples.Luminance(pixel.X, pixel.Y, pixel.Z, luminance);
            pixel = membership is 1f
                ? pixel
                : new Vector4((float)(gray + (membership * (double)pixel.X)), (float)(gray + (membership * (double)pixel.Y)), (float)(gray + (membership * (double)pixel.Z)), pixel.W);
        }
    }
}

[SmartEnum]
public sealed partial class RangeAxis {
    public static readonly RangeAxis Hue = new(InputAxis.Hue, static (hsy, value) => hsy with { Hue = value });
    public static readonly RangeAxis Saturation = new(InputAxis.Saturation, static (hsy, value) => hsy with { Saturation = value });
    public static readonly RangeAxis Luma = new(InputAxis.Stops, static (hsy, value) => hsy with { Luma = CurveAxis.FromStops(value) });

    public InputAxis Input { get; }

    [UseDelegateFromConstructor]
    private partial Hsy Write(Hsy hsy, double value);

    public float Coordinate(Hsy hsy) =>
        Input.Axis.Span switch {
            var (low, high) => ((Input.Read(hsy) - low) / (high - low)) switch { var u => (float)(Input.Axis.Periodic ? u - double.Floor(u) : double.Clamp(u, 0d, 1d)) },
        };

    public float Distance(float coordinate, float center) =>
        (coordinate - center) switch { var d => Input.Axis.Periodic ? float.Abs(d + 0.5f - float.Floor(d + 0.5f) - 0.5f) : float.Abs(d) };

    public Hsy Replaced(Hsy hsy, float coordinate) =>
        Input.Axis.Span switch { var (low, high) => Write(hsy, low + (coordinate * (high - low))) };

    public float Membership(RangeBand band, Hsy hsy) => band.Membership(Distance(Coordinate(hsy), band.Center));
}
