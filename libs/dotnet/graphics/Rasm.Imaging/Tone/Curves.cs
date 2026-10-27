using System.Numerics;
using System.Runtime.InteropServices;
using CommunityToolkit.HighPerformance.Buffers;
using Rasm.Imaging.Color;
using Rasm.Imaging.Pixels;
using TinyEXR;
using UnitsNet;
using Wacton.Unicolour;

namespace Rasm.Imaging.Tone;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class DisplayDevice {
    private static readonly Luminance Standard = Luminance.FromNits(DynamicRange.Standard.MaxLuminance);
    private static readonly Luminance High = Luminance.FromNits(DynamicRange.High.MaxLuminance);

    public static readonly DisplayDevice Srgb = new("srgb", new ColorEncoding(Gamut.StandardRgb, new Transfer.Srgb()), Standard, Gamut.StandardRgb);
    public static readonly DisplayDevice DisplayP3 = new("display-p3", new ColorEncoding(Gamut.DisplayP3, new Transfer.Srgb()), Standard, Gamut.DisplayP3);
    public static readonly DisplayDevice Rec1886 = new("rec1886", new ColorEncoding(Gamut.StandardRgb, Transfer.Gamma.Rec1886), Standard, Gamut.StandardRgb);
    public static readonly DisplayDevice Rec2020 = new("rec2020", new ColorEncoding(Gamut.Rec2020, Transfer.Gamma.Rec1886), Standard, Gamut.DisplayP3);
    public static readonly DisplayDevice Rec2100PqHdr = new("rec2100-pq-hdr", new ColorEncoding(Gamut.Rec2020, Transfer.Pq.High), High, Gamut.Rec2020);
    public static readonly DisplayDevice Rec2100PqSdr = new("rec2100-pq-sdr", new ColorEncoding(Gamut.Rec2020, Transfer.Pq.High), Standard, Gamut.StandardRgb);
    public static readonly DisplayDevice Rec2100HlgHdr = new("rec2100-hlg-hdr", new ColorEncoding(Gamut.Rec2020, Transfer.Hlg.High), High, Gamut.Rec2020);
    public static readonly DisplayDevice Rec2100HlgSdr = new("rec2100-hlg-sdr", new ColorEncoding(Gamut.Rec2020, Transfer.Hlg.High), Standard, Gamut.StandardRgb);

    public ColorEncoding Encoding { get; }
    public Luminance Peak { get; }
    public Gamut LimitingGamut { get; }

    public Action<Span<Vector4>> Formed(Gamut working, Gamut scene, Seq<Action<Span<Vector4>>> steps, ColorEncoding signal) {
        Action<Span<Vector4>>[] chain = [.. LutTables.Between(working, scene).Map<Action<Span<Vector4>>>(static step => step.Apply), .. steps, .. signal.To(Encoding)];
        return row => {
            foreach (Action<Span<Vector4>> step in chain)
                step(row);
        };
    }
}

public sealed record CurveApplication(float PreserveHue, float Purity, Vector3 Inset, Vector3 Rotation) {
    public static CurveApplication Default { get; } = new(1f, 0f, Vector3.Zero, Vector3.Zero);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ToneCurves {
    private static readonly Action<Span<Vector4>> Desaturated = static row => {
        foreach (ref Vector4 pixel in row) {
            double average = double.Max(((double)pixel.X + pixel.Y + pixel.Z) / 3d, 0d);
            double least = float.Min(pixel.X, float.Min(pixel.Y, pixel.Z));
            double amount = least < 0d ? -least / (average - least) : 0d;
            pixel = new Vector4((float)double.Lerp(pixel.X, average, amount), (float)double.Lerp(pixel.Y, average, amount), (float)double.Lerp(pixel.Z, average, amount), pixel.W);
        }
    };

    private static readonly Action<Span<Vector4>> Neutral = static row => {
        const float fresnel = 0.04f;
        const float specular = 0.8f - fresnel;
        const float desaturation = 0.15f;
        foreach (ref Vector4 pixel in row) {
            float least = float.Min(pixel.X, float.Min(pixel.Y, pixel.Z));
            Vector3 shifted = pixel.AsVector3() - new Vector3(least < 2f * fresnel ? least - (least * least / (4f * fresnel)) : fresnel);
            float peak = float.Max(shifted.X, float.Max(shifted.Y, shifted.Z));
            float compressed = 1f - ((1f - specular) * (1f - specular) / (peak + (1f - specular) - specular));
            pixel = peak < specular
                ? new Vector4(shifted, pixel.W)
                : new Vector4(Vector3.Lerp(shifted * (compressed / peak), new Vector3(compressed), 1f - (1f / ((desaturation * (peak - compressed)) + 1f))), pixel.W);
        }
    };

    public static Vector3 PreserveHue(Vector3 input, Vector3 mapped, float preserve) {
        (int min, int mid, int max) = input.X >= input.Y
            ? input.Y > input.Z ? (2, 1, 0) : input.Z > input.X ? (1, 0, 2) : input.Z > input.Y ? (1, 2, 0) : (2, 1, 0)
            : input.X >= input.Z ? (2, 0, 1) : input.Z > input.Y ? (0, 1, 2) : (0, 2, 1);
        float hue = preserve;
        float chroma = input[max] - input[min];
        float midscale = chroma is not 0f ? (input[mid] - input[min]) / chroma : 0f;
        float naive = ((1f - hue) * mapped[mid]) + (hue * (mapped[min] + ((mapped[max] - mapped[min]) * midscale)));
        float blend = input[min] + input[mid] is not 0f ? 2f * input[min] / (input[min] + input[mid]) : 0f;
        float target = (blend * (mapped.X + mapped.Y + mapped.Z)) + ((1f - blend) * (mapped[min] + naive + mapped[max]));
        (float low, float middle, float high) = naive <= mapped[mid]
            ? ((((1f - hue) * mapped[mid]) + (hue * ((midscale * mapped[max]) + ((1f - midscale) * (target - mapped[max]))))) / (1f + (hue * (1f - midscale)))) switch {
                var corrected => (target - mapped[max] - corrected, corrected, mapped[max]),
            }
            : ((((1f - hue) * mapped[mid]) + (hue * ((mapped[min] * (1f - midscale)) + (midscale * (target - mapped[min]))))) / (1f + (hue * midscale))) switch {
                var corrected => (mapped[min], corrected, target - mapped[min] - corrected),
            };
        return new(Placed(0), Placed(1), Placed(2));

        float Placed(int channel) => channel == min ? low : channel == mid ? middle : high;
    }

    public static Func<float, float> Sigmoid(float contrast, float skew, float black, Luminance peak) {
        const double gray = Exposure.MiddleGray;
        double ceiling = peak.Nits.ToDouble() / DynamicRange.Standard.WhiteLuminance;
        double bend = Math.Pow(5d, -skew);
        double power = contrast * (1d - gray) / (bend * (1d - Math.Pow(gray / ceiling, 1d / bend)));
        double towardGray = Math.Pow(ceiling / gray, 1d / bend) - 1d;
        double floor = Math.Pow(black, 1d / bend);
        double lifted = Math.Pow(towardGray * floor, 1d / power);
        double fog = gray * lifted / (Math.Pow(1d - floor, 1d / power) - lifted);
        (float whiteTarget, float paperExposure, float filmFog, float contrastPower, float skewPower) =
            ((float)ceiling, (float)(Math.Pow(fog + gray, power) * towardGray), (float)fog, (float)power, (float)bend);
        return value => whiteTarget * MathF.Pow(1f + (paperExposure * MathF.Pow(filmFog + float.MaxNumber(value, 0f), -contrastPower)), -skewPower);
    }

    public static Func<float, float> Piecewise(float toeStrength, float toeLength, float shoulderAngle, float gamma, float shoulderStrength, float shoulderLength) {
        double toeX = 0.5d * Math.Pow(toeLength, 2.2d);
        double toeY = (1d - toeStrength) * toeX;
        double shoulderX = toeX + ((1d - shoulderLength) * (1d - toeY));
        double white = toeX + (1d - toeY) + double.Exp2(shoulderStrength) - 1d;
        double end = white * (1d + (2d * shoulderAngle * shoulderStrength));
        double ceiling = Math.Pow(1d + (0.5d * shoulderAngle * shoulderStrength), gamma);
        (double toe, double shoulder) = (Math.Max(1e-5d, Math.Pow(toeY, gamma)), Math.Max(1e-5d, Math.Pow(toeY + shoulderX - toeX, gamma)));
        double shoulderPower = gamma * Math.Pow(shoulder, 1d - (1d / gamma)) * (end - shoulderX) / (ceiling - shoulder);
        (float toeEnd, float toeValue, float toePower, float intercept) = ((float)toeX, (float)toe, (float)(gamma * toeX * Math.Pow(toe, -1d / gamma)), (float)(toeY - toeX));
        (float shoulderStart, float shoulderValue, float shoulderSlope, float shoulderEnd, float top) = ((float)shoulderX, (float)shoulder, (float)shoulderPower, (float)end, (float)ceiling);
        float scale = (float)(1d / (ceiling - ((ceiling - shoulder) * Math.Pow((end - white) / (end - shoulderX), shoulderPower))));
        return value =>
            scale * (float.MaxNumber(value, 0f) switch {
                var x when x < toeEnd => toeValue * MathF.Pow(x / toeEnd, toePower),
                var x when x < shoulderStart => MathF.Pow(x + intercept, gamma),
                var x when x < shoulderEnd => top - ((top - shoulderValue) * MathF.Pow((shoulderEnd - x) / (shoulderEnd - shoulderStart), shoulderSlope)),
                _ => top,
            });
    }

    public static Func<float, float> Logarithmic(float bias, float white, Luminance peak) {
        (float whiteTarget, float normalizer, float inverseWhite, float power) =
            ((float)(peak.Nits.ToDouble() / DynamicRange.Standard.WhiteLuminance), float.Log10P1(white), 1f / white, (float)(Math.Log(bias) / Math.Log(0.5d)));
        return value => float.MaxNumber(value, 0f) switch {
            var x => whiteTarget * float.LogP1(x) / (normalizer * MathF.Log(2f + (8f * MathF.Pow(x * inverseWhite, power)))),
        };
    }

    public static Action<Span<Vector4>> Curved(Gamut working, DisplayDevice device, Gamut basis, Action<Span<Vector4>> curve) =>
        device.Formed(working, basis, [Desaturated, curve], new ColorEncoding(basis, new Transfer.Linear()));

    public static Action<Span<Vector4>> Applied(Gamut working, DisplayDevice device, Gamut basis, CurveApplication application, Func<float, float> curve) =>
        device.Formed(working, basis, [
            Desaturated,
            new LutTable.Affine(Gamut.Between(Adjusted(basis, application, 1f), basis.Configuration)).Apply,
            row => {
                foreach (ref Vector4 pixel in row)
                    pixel = new Vector4(PreserveHue(pixel.AsVector3(), new Vector3(curve(pixel.X), curve(pixel.Y), curve(pixel.Z)), application.PreserveHue), pixel.W);
            },
            new LutTable.Affine(Gamut.Between(basis.Configuration, Adjusted(basis, application, application.Purity))).Apply,
        ], new ColorEncoding(basis, new Transfer.Linear()));

    public static Action<Span<Vector4>> Mapped(Gamut working, DisplayDevice device, ToneMapOperator operation, ToneMapParameters parameters) =>
        Curved(working, device, working, row => {
            Span<float> components = MemoryMarshal.Cast<Vector4, float>(row);
            ImageProcessing.ToneMap(components, components, 4, operation, parameters);
        });

    public static Action<Span<Vector4>> PbrNeutral(Gamut working, DisplayDevice device) => Curved(working, device, Gamut.StandardRgb, Neutral);

    public static Fin<Action<Span<Vector4>>> Photoreceptor(Gamut working, DisplayDevice device, PixelFrame frame, float intensity, Option<float> contrast, float light, float chromatic) =>
        LuminanceSamples.Gather(frame, working).Map(samples => Curved(working, device, working, Adapted(frame, working.Luminance, samples.LogStatistics, intensity, contrast, light, chromatic)));

    private static Action<Span<Vector4>> Adapted(
        PixelFrame frame, (double R, double G, double B) weights, (double Mean, double Maximum, double Minimum) logs, float intensity, Option<float> contrast, float light, float chromatic) {
        (double red, double green, double blue, long count) = (0d, 0d, 0d, 0L);
        using SpanOwner<Vector4> scratch = SpanOwner<Vector4>.Allocate(frame.Size.Width);
        for (int y = 0; y < frame.Size.Height; y++) {
            ReadOnlySpan<Vector4> row = frame.View.Span.GetRowSpan(y);
            row.CopyTo(scratch.Span);
            Desaturated(scratch.Span);
            for (int x = 0; x < row.Length; x++)
                (red, green, blue, count) = row[x].W > 0f && double.IsFinite(Gamut.RelativeLuminance(row[x].X, row[x].Y, row[x].Z, weights))
                    ? (red + scratch.Span[x].X, green + scratch.Span[x].Y, blue + scratch.Span[x].Z, count + 1L) : (red, green, blue, count);
        }
        (double r, double g, double b) = (red / count, green / count, blue / count);
        double mean = Gamut.RelativeLuminance(r, g, b, weights);
        double scale = double.Exp(-intensity);
        double power = contrast.Match(
            Some: static manual => (double)manual,
            None: () => logs.Maximum == logs.Minimum ? 1d : 0.3d + (0.7d * double.Pow((logs.Maximum - logs.Mean) / (logs.Maximum - logs.Minimum), 1.4d)));
        (double globalR, double globalG, double globalB) = (double.Lerp(mean, r, chromatic), double.Lerp(mean, g, chromatic), double.Lerp(mean, b, chromatic));
        return row => {
            foreach (ref Vector4 pixel in row) {
                double luminance = Gamut.RelativeLuminance(pixel.X, pixel.Y, pixel.Z, weights);
                pixel = new Vector4(Mapped(pixel.X, globalR, luminance), Mapped(pixel.Y, globalG, luminance), Mapped(pixel.Z, globalB, luminance), pixel.W);
            }
        };

        float Mapped(double channel, double average, double luminance) =>
            (channel + double.Pow(scale * double.Lerp(average, double.Lerp(luminance, channel, chromatic), light), power)) switch {
                0d => 0f,
                var total => (float)(channel / total),
            };
    }

    private static Configuration Adjusted(Gamut basis, CurveApplication application, float share) =>
        basis.Configuration.Rgb switch {
            var rgb => new(new RgbConfiguration(
                Moved(rgb, rgb.ChromaticityR, 1d - (share * application.Inset.X), application.Rotation.X),
                Moved(rgb, rgb.ChromaticityG, 1d - (share * application.Inset.Y), application.Rotation.Y),
                Moved(rgb, rgb.ChromaticityB, 1d - (share * application.Inset.Z), application.Rotation.Z),
                rgb.WhitePoint, static linear => linear, static linear => linear)),
        };

    private static Chromaticity Moved(RgbConfiguration rgb, Chromaticity primary, double scale, double rotation) {
        Chromaticity white = rgb.WhitePoint.Chromaticity;
        (double sin, double cos) = Math.SinCos(Angle(rgb, primary) + rotation);
        double edge = Seq((From: rgb.ChromaticityR, To: rgb.ChromaticityG), (From: rgb.ChromaticityG, To: rgb.ChromaticityB), (From: rgb.ChromaticityB, To: rgb.ChromaticityR))
            .Map(side => (((white.X - side.From.X) * (side.From.Y - side.To.Y)) - ((side.From.X - side.To.X) * (white.Y - side.From.Y)))
                / ((sin * (side.From.X - side.To.X)) - (cos * (side.From.Y - side.To.Y))))
            .Filter(static reach => reach >= 0d)
            .Fold(double.PositiveInfinity, Math.Min);
        return new(white.X + (scale * edge * cos), white.Y + (scale * edge * sin));
    }

    private static double Angle(RgbConfiguration rgb, Chromaticity primary) =>
        Math.Atan2(primary.Y - rgb.WhitePoint.Chromaticity.Y, primary.X - rgb.WhitePoint.Chromaticity.X);
}
