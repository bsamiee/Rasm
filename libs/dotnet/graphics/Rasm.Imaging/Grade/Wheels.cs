using System.Globalization;
using System.Numerics;
using Rasm.Imaging.Color;
using Rasm.Imaging.Tone;
using Wacton.Unicolour;

namespace Rasm.Imaging.Grade;

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct WheelOffset(float Hue, float Strength, float Luma) {
    public Vector3 Multiplier(Gamut gamut) => Tint(gamut, 0.5f * Strength * new Vector2(float.Cos(Hue), float.Sin(Hue))) * float.Exp2(Luma);

    public static Vector3 Tint(Gamut gamut, Vector2 chroma) =>
        new Unicolour(Configuration.Default, ColourSpace.Oklab, 1d, chroma.X, chroma.Y).ConvertToConfiguration(gamut.Configuration).RgbLinear switch {
            var rgb => new Vector3((float)rgb.R, (float)rgb.G, (float)rgb.B),
        };

    public static Fin<WheelOffset> FromMultiplier(Vector3 multiplier, Gamut gamut, float hue) =>
        new Unicolour(gamut.Configuration, ColourSpace.RgbLinear, multiplier.X, multiplier.Y, multiplier.Z).ConvertToConfiguration(Configuration.Default).Oklab switch {
            { L: <= 0d } => Error.New(ErrorCodes.ValidationFailed, string.Create(CultureInfo.InvariantCulture, $"wheel multiplier {multiplier} must have positive lightness")),
            var lab => new Vector2((float)(lab.A / lab.L), (float)(lab.B / lab.L)) switch {
                var chroma => new WheelOffset(
                    chroma.Length() < 1e-6f ? hue : (float.Atan2(chroma.Y, chroma.X) + float.Tau) % float.Tau, 2f * chroma.Length(), 3f * float.Log2((float)lab.L)),
            },
        };

    public static (Vector3 Low, Vector3 High) ChannelBounds(Gamut gamut) =>
        toSeq(Range(0, 360))
            .Map(static step => float.DegreesToRadians(step))
            .Map(angle => Tint(gamut, 0.5f * new Vector2(float.Cos(angle), float.Sin(angle))))
            .Fold((Low: new Vector3(float.MaxValue), High: new Vector3(float.MinValue)), static (bounds, tint) => (Vector3.Min(bounds.Low, tint), Vector3.Max(bounds.High, tint)));

    public static Action<Span<Vector4>> Graded(
        Gamut working, WheelOffset global, WheelOffset shadows, WheelOffset midtones, WheelOffset highlights, float shadowsEnd, float highlightsStart, float highlightsWidth) {
        ((double R, double G, double B) weights, Vector3 all, Vector3 low, Vector3 middle, Vector3 high) =
            (working.Luminance, global.Multiplier(working), shadows.Multiplier(working), midtones.Multiplier(working), highlights.Multiplier(working));
        return row => {
            foreach (ref Vector4 pixel in row) {
                double luma = LuminanceSamples.Luminance(pixel.X, pixel.Y, pixel.Z, weights);
                (float lifted, float raised) = ((float)Smooth(0d, shadowsEnd, luma), (float)Smooth(highlightsStart, highlightsStart + highlightsWidth, luma));
                pixel = new Vector4(pixel.AsVector3() * all * ((low * (1f - lifted)) + (middle * lifted * (1f - raised)) + (high * lifted * raised)), pixel.W);
            }
        };

        static double Smooth(double low, double high, double y) =>
            double.Clamp((y - low) / (high - low), 0d, 1d) switch {
                var t => t * t * (3d - (2d * t)),
            };
    }
}
