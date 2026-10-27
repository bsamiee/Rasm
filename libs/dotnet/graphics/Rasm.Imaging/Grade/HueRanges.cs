using System.Numerics;
using Rasm.Imaging.Color;
using Rasm.Imaging.Procedural;

namespace Rasm.Imaging.Grade;

// --- [MODELS] --------------------------------------------------------------------------
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

    public float Membership((double Hue, double Saturation, double Luma) hsy) =>
        RangeAxis.Hue.Membership(Hue, hsy) * RangeAxis.Saturation.Membership(Saturation, hsy) * RangeAxis.Luma.Membership(Luma, hsy);

    public void Graded(Span<Vector4> row, float shift, float saturation, float stops) {
        float gain = float.Exp2(stops);
        foreach (ref Vector4 pixel in row) {
            (double Hue, double Saturation, double Luma) hsy = Hsy.From(pixel);
            Vector3 graded = shift is 0f && saturation is 1f
                ? pixel.AsVector3() * gain
                : Hsy.ToRgb((hsy.Hue + shift, hsy.Saturation * saturation, hsy.Luma), pixel.W).AsVector3() * gain;
            pixel = new Vector4(Vector3.Lerp(pixel.AsVector3(), graded, Membership(hsy)), pixel.W);
        }
    }

    public void Isolated(Span<Vector4> row, Gamut gamut) {
        (double R, double G, double B) luminance = gamut.Luminance;
        foreach (ref Vector4 pixel in row) {
            float membership = Membership(Hsy.From(pixel));
            double gray = (1d - membership) * Gamut.RelativeLuminance(pixel.X, pixel.Y, pixel.Z, luminance);
            pixel = membership is 1f
                ? pixel
                : new Vector4((float)(gray + (membership * (double)pixel.X)), (float)(gray + (membership * (double)pixel.Y)), (float)(gray + (membership * (double)pixel.Z)), pixel.W);
        }
    }
}

[SmartEnum]
public sealed partial class RangeAxis {
    public static readonly RangeAxis Hue = new(InputAxis.Hue, static (hsy, value) => (value, hsy.Saturation, hsy.Luma));
    public static readonly RangeAxis Saturation = new(InputAxis.Saturation, static (hsy, value) => (hsy.Hue, value, hsy.Luma));
    public static readonly RangeAxis Luma = new(InputAxis.Stops, static (hsy, value) => (hsy.Hue, hsy.Saturation, CurveAxis.FromStops(value)));

    public InputAxis Input { get; }

    [UseDelegateFromConstructor]
    private partial (double Hue, double Saturation, double Luma) Write((double Hue, double Saturation, double Luma) hsy, double value);

    public float Coordinate((double Hue, double Saturation, double Luma) hsy) =>
        Input.Axis.Span switch {
            var (low, high) => ((Input.Read(hsy) - low) / (high - low)) switch { var u => (float)(Input.Axis.Periodic ? u - double.Floor(u) : double.Clamp(u, 0d, 1d)) },
        };

    public float Distance(float coordinate, float center) =>
        (coordinate - center) switch { var d => Input.Axis.Periodic ? float.Abs(d + 0.5f - float.Floor(d + 0.5f) - 0.5f) : float.Abs(d) };

    public (double Hue, double Saturation, double Luma) Replaced((double Hue, double Saturation, double Luma) hsy, float coordinate) =>
        Input.Axis.Span switch { var (low, high) => Write(hsy, low + (coordinate * (high - low))) };

    public float Membership(RangeBand band, (double Hue, double Saturation, double Luma) hsy) => band.Membership(Distance(Coordinate(hsy), band.Center));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Hsy {
    private const double Knee = 0.15d;
    private const double LowGain = 5d;
    private const double Scale = 1.4d;
    private const double MinLuma = 0.001d;
    private const double MaxLuma = 0.01d;

    private static readonly (double R, double G, double B) LumaWeights = Gamut.StandardRgb.Luminance;

    public static double SaturationPeak { get; } = Scale * (1d + ((LumaWeights.R + LumaWeights.G + LumaWeights.B) / double.Min(double.Min(LumaWeights.R, LumaWeights.G), LumaWeights.B))) / 3d;

    public static (double Hue, double Saturation, double Luma) From(Vector4 color) {
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
        return ((turn - double.Floor(turn)) * double.Tau, distance * SaturationGain(luma), luma);
    }

    public static Vector3 Saturated(double hue) =>
        HueRgb(hue) switch {
            var (red, green, blue) => new((float)red, (float)green, (float)blue),
        };

    public static Vector4 ToRgb((double Hue, double Saturation, double Luma) hsy, float alpha) {
        (double red, double green, double blue) = HueRgb(hsy.Hue);
        double luma = Luminance(red, green, blue);
        (red, green, blue) = (red - luma, green - luma, blue - luma);
        double gain = hsy.Saturation / SaturationGain(hsy.Luma) / (double.Abs(red) + double.Abs(green) + double.Abs(blue));
        return new((float)(hsy.Luma + (gain * red)), (float)(hsy.Luma + (gain * green)), (float)(hsy.Luma + (gain * blue)), alpha);
    }

    private static double Luminance(double red, double green, double blue) =>
        Gamut.RelativeLuminance(red, green, blue, LumaWeights) / (LumaWeights.R + LumaWeights.G + LumaWeights.B);

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
