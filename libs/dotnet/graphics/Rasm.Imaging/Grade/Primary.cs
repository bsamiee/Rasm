using System.Numerics;
using Rasm.Imaging.Color;
using Rasm.Imaging.Tone;
using Wacton.Unicolour;

namespace Rasm.Imaging.Grade;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record GradingRgbm(float Red, float Green, float Blue, float Master) {
    internal Vector3 Sum => new Vector3(Red, Green, Blue) + new Vector3(Master);
    internal Vector3 Product => new Vector3(Red, Green, Blue) * Master;

    public static GradingRgbm FromWheel(Gamut gamut, float hue, float strength, float luma) =>
        new Unicolour(Configuration.Default, ColourSpace.Oklab, 1d, 0.5d * strength * float.Cos(hue), 0.5d * strength * float.Sin(hue)).ConvertToConfiguration(gamut.Configuration).RgbLinear switch { var tint => new((float)tint.R, (float)tint.G, (float)tint.B, float.Exp2(luma)) };

    public Fin<(float Hue, float Strength, float Luma)> ToWheel(Gamut gamut, float hue) =>
        new Unicolour(gamut.Configuration, ColourSpace.RgbLinear, Product.X, Product.Y, Product.Z).ConvertToConfiguration(Configuration.Default).Oklab switch {
            { L: <= 0d } lab => ImagingFault.Of(ImagingReason.Range, lab.L),
            var lab => new Vector2((float)(lab.A / lab.L), (float)(lab.B / lab.L)) switch {
                var chroma => (chroma.Length() < 1e-6f ? hue : (float.Atan2(chroma.Y, chroma.X) + float.Tau) % float.Tau, 2f * chroma.Length(), 3f * float.Log2((float)lab.L)),
            },
        };

    public static (Vector3 Low, Vector3 High) WheelBounds(Gamut gamut) =>
        toSeq(Range(0, 360))
            .Map(static step => float.DegreesToRadians(step))
            .Map(angle => FromWheel(gamut, angle, 1f, 0f).Product)
            .Fold((Low: new Vector3(float.MaxValue), High: new Vector3(float.MinValue)), static (bounds, tint) => (Vector3.Min(bounds.Low, tint), Vector3.Max(bounds.High, tint)));
}

public sealed record GradingPrimary(
    GradingRgbm Brightness, GradingRgbm Contrast, GradingRgbm Gamma, GradingRgbm Offset, GradingRgbm Exposure, GradingRgbm Lift, GradingRgbm Gain,
    float Saturation, float Vibrance, float Pivot, float PivotBlack, float PivotWhite, float ClampBlack, float ClampWhite) {
    public static GradingPrimary Neutral(GradingStyle style) =>
        (new GradingRgbm(0f, 0f, 0f, 0f), new GradingRgbm(1f, 1f, 1f, 1f)) switch {
            var (zero, one) => new(zero, one, one, zero, zero, zero, one, 1f, 0f, style.Map(log: -0.2f, linear: Metering.MiddleGray, video: Metering.MiddleGray), 0f, 1f, float.NegativeInfinity, float.PositiveInfinity),
        };

    public Option<Action<Span<Vector4>>> Graded(GradingStyle style) {
        static Vector3 Nonzero(Vector3 value) => Vector3.ConditionalSelect(Vector3.IsZero(value), Vector3.One, value);
        float span = float.Max(PivotWhite - PivotBlack, 0.01f);
        (Vector3 offset, float anchor, Vector3 scale, float black, float white, Vector3 power) = style.Map(
            log: (Brightness.Sum * 6.25f / 1023f, 0.5f + (Pivot * 0.5f), Contrast.Product, PivotBlack, PivotBlack + span, Vector3.One / Vector3.Max(Gamma.Product, new(0.01f))),
            linear: (Offset.Sum, 0f, Vector3.Exp(Exposure.Sum * float.Log(2f)), 0f, Metering.MiddleGray * float.Exp2(Pivot), Vector3.Max(Contrast.Product, new(0.01f))),
            video: (Offset.Sum + Lift.Sum, PivotBlack, new Vector3(span) / Nonzero((new Vector3(PivotWhite) / Nonzero(Gain.Product)) + Lift.Sum - new Vector3(PivotBlack)),
                PivotBlack, PivotBlack + span, Vector3.One / Vector3.Max(Gamma.Product, new(0.01f))));
        (Vector3 luma, float range) = (Gamut.StandardRgb.Luminance, white - black);
        return this with { Pivot = Neutral(style).Pivot, PivotBlack = 0f, PivotWhite = 1f } == Neutral(style) ? None : Some(ToneCurve.Graded(Corrected));

        Vector3 Corrected(Vector3 color) {
            Vector3 shifted = ((color + offset - new Vector3(anchor)) * scale) + new Vector3(anchor - black);
            Vector3 rgb = power == Vector3.One ? shifted + new Vector3(black) : (Vector3.CopySign(Vector3.Exp(Vector3.Log(Vector3.Abs(shifted) / range) * power), shifted) * range) + new Vector3(black);
            (float high, float low) = (float.Max(rgb.X, float.Max(rgb.Y, rgb.Z)), float.Max(float.Min(rgb.X, float.Min(rgb.Y, rgb.Z)), 0f));
            float spread = high > 0f ? 1f - (low / high) : 1f;
            float chroma = Saturation * (1f + (Vibrance * (Vibrance > 0f ? (1f - spread) * (1f - spread) : spread * spread * 0.5f)));
            return Vector3.Clamp(Vector3.Lerp(new Vector3(Vector3.Dot(luma, rgb)), rgb, chroma), new Vector3(ClampBlack), new Vector3(ClampWhite));
        }
    }
}

public sealed record GradientMap(ReadOnlyMemory<Vector4> Stops) {
    public Action<Span<Vector4>> Graded(Gamut gamut) {
        (float top, Vector3 luminance, ReadOnlyMemory<Vector4> stops) = (Stops.Length - 1, gamut.Luminance, Stops);
        return row => {
            ReadOnlySpan<Vector4> table = stops.Span;
            foreach (ref Vector4 pixel in row) {
                float position = float.Clamp(float.Cbrt(Vector3.Dot(luminance, pixel.AsVector3())), 0f, 1f) * top;
                int low = int.Clamp((int)float.MaxNumber(position, 0f), 0, int.Max(table.Length - 2, 0));
                Vector4 mapped = Vector4.Lerp(table[low], table[int.Min(low + 1, table.Length - 1)], position - low);
                pixel = new Vector4(Vector3.Lerp(pixel.AsVector3(), mapped.AsVector3(), mapped.W), pixel.W);
            }
        };
    }
}
