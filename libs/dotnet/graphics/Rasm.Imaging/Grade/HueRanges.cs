using System.Numerics;
using System.Runtime.Intrinsics;
using Rasm.Imaging.Color;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Procedural;

namespace Rasm.Imaging.Grade;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Hsy(double Hue, double Saturation, double Luma) {
    private const double Knee = 0.15d;
    private const double LowGain = 5d;
    private const double Scale = 1.4d;
    private const double MinLuma = 0.001d;
    private const double MaxLuma = 0.01d;

    private static readonly Vector256<double> LumaWeights = Gamut.StandardRgb.Luminance;

    public static double SaturationPeak { get; } = Scale * (1d + (Vector256.Sum(LumaWeights) / double.Min(double.Min(LumaWeights[0], LumaWeights[1]), LumaWeights[2]))) / 3d;

    public static Hsy From(Vector4 color) => LumaOf(color) switch { var luma => new(HueOf(color), SaturationOf(color, luma), luma) };

    public static Vector3 Saturated(double hue) => HueRgb(hue) switch { var (red, green, blue) => new((float)red, (float)green, (float)blue) };

    public Vector4 ToRgb(float alpha) => ToRgb(Hue, Saturation, Luma, alpha);

    internal static double LumaOf(Vector4 color) => Luminance(color.X, color.Y, color.Z);

    internal static double SaturationOf(Vector4 color, double luma) => (double.Abs(color.X - luma) + double.Abs(color.Y - luma) + double.Abs(color.Z - luma)) * SaturationGain(luma);

    internal static double HueOf(Vector4 color) {
        (double red, double green, double blue) = (color.X, color.Y, color.Z);
        (double max, double min) = (double.Max(double.Max(red, green), blue), double.Min(double.Min(red, green), blue));
        double turn = (min == max ? 0d
            : red == max ? 1d + ((green - blue) / (max - min))
            : green == max ? 3d + ((blue - red) / (max - min))
            : 5d + ((red - green) / (max - min))) / 6d;
        return (turn - double.Floor(turn)) * double.Tau;
    }

    internal static Vector4 ToRgb(double hue, double saturation, double luma, float alpha) {
        (double red, double green, double blue) = HueRgb(hue);
        double mean = Luminance(red, green, blue);
        (red, green, blue) = (red - mean, green - mean, blue - mean);
        double gain = saturation / SaturationGain(luma) / (double.Abs(red) + double.Abs(green) + double.Abs(blue));
        return new((float)(luma + (gain * red)), (float)(luma + (gain * green)), (float)(luma + (gain * blue)), alpha);
    }

    private static double Luminance(double red, double green, double blue) =>
        Vector256.Dot(LumaWeights, Vector256.Create(red, green, blue, 0d)) / Vector256.Sum(LumaWeights);

    private static double SaturationGain(double luma) =>
        luma switch {
            <= MinLuma => Scale * LowGain,
            >= MaxLuma => Scale / (Knee + (3d * luma)),
            _ => ((luma - MinLuma) / (MaxLuma - MinLuma)) switch { var blend => Scale * (((1d - blend) * LowGain) + (blend / (Knee + (3d * luma)))) },
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

public sealed record RangeBand(float Center, float Width, float Softness) {
    public float Membership(float distance) =>
        (distance - (Width / 2f)) switch {
            <= 0f => 1f,
            var outside when outside >= Softness => 0f,
            var outside => 1f - ShapeEdge.Smooth(outside / Softness),
        };
}

public sealed record HueRange(RangeBand Hue, RangeBand Saturation, RangeBand Luma) {
    public static HueRange Open { get; } = new RangeBand(0.5f, 1f, 0f) switch { var open => new HueRange(open, open, open) };

    public float Membership(Hsy hsy) => Membership(hsy.Hue, hsy.Saturation, hsy.Luma);

    public Action<Span<Vector4>> Graded(float shift, float saturation, float stops) {
        float gain = float.Exp2(stops);
        return row => {
            foreach (ref Vector4 pixel in row) {
                double luma = Hsy.LumaOf(pixel);
                (double hue, double chroma) = (Hsy.HueOf(pixel), Hsy.SaturationOf(pixel, luma));
                Vector3 graded = shift is 0f && saturation is 1f ? pixel.AsVector3() * gain : Hsy.ToRgb(hue + shift, chroma * saturation, luma, pixel.W).AsVector3() * gain;
                pixel = new Vector4(Vector3.Lerp(pixel.AsVector3(), graded, Membership(hue, chroma, luma)), pixel.W);
            }
        };
    }

    public Action<Span<Vector4>> Isolated(Gamut gamut) {
        Vector256<double> luminance = gamut.Luminance;
        return row => {
            foreach (ref Vector4 pixel in row) {
                float membership = Membership(pixel);
                float gray = (float)((1d - membership) * Vector256.Dot(luminance, pixel.Wide));
                pixel = new Vector4(new Vector3(gray) + (membership * pixel.AsVector3()), pixel.W);
            }
        };
    }

    public float Membership(Vector4 pixel) => Hsy.LumaOf(pixel) switch { var luma => Membership(Hsy.HueOf(pixel), Hsy.SaturationOf(pixel, luma), luma) };

    private float Membership(double hue, double saturation, double luma) =>
        RangeAxis.Hue.Membership(Hue, hue) * RangeAxis.Saturation.Membership(Saturation, saturation) * RangeAxis.Luma.Membership(Luma, CurveAxis.ToStops(luma));
}

[SmartEnum]
public sealed partial class RangeAxis {
    public static readonly RangeAxis Hue = new(InputAxis.Hue, static (hsy, value) => hsy with { Hue = value });
    public static readonly RangeAxis Saturation = new(InputAxis.Saturation, static (hsy, value) => hsy with { Saturation = value });
    public static readonly RangeAxis Luma = new(InputAxis.Stops, static (hsy, value) => hsy with { Luma = CurveAxis.FromStops(value) });

    public InputAxis Input { get; }

    [UseDelegateFromConstructor]
    private partial Hsy Write(Hsy hsy, double value);

    public float Coordinate(Hsy hsy) => Coordinate(Input.Read(hsy));

    public float Distance(float coordinate, float center) =>
        (coordinate - center) switch { var d => Input.Axis.Periodic ? float.Abs(d + 0.5f - float.Floor(d + 0.5f) - 0.5f) : float.Abs(d) };

    public Hsy Replaced(Hsy hsy, float coordinate) => Input.Axis.Span switch { var (low, high) => Write(hsy, low + (coordinate * (high - low))) };

    public float Membership(RangeBand band, Hsy hsy) => Membership(band, Input.Read(hsy));

    internal float Coordinate(double input) =>
        Input.Axis.Span switch {
            var (low, high) => ((input - low) / (high - low)) switch { var u => (float)(Input.Axis.Periodic ? u - double.Floor(u) : double.Clamp(u, 0d, 1d)) },
        };

    internal float Membership(RangeBand band, double input) => band.Membership(Distance(Coordinate(input), band.Center));
}
