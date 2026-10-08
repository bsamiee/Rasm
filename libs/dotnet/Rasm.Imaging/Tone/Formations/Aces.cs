using System.Numerics;
using Rasm.Imaging.ColorManagement;
using Rasm.Imaging.Pixels;
using TinyEXR;
using Wacton.Unicolour;

namespace Rasm.Imaging.Tone.Formations;

// --- [MODELS] --------------------------------------------------------------------------
internal sealed record JmhSpace(Matrix4x4 ToCone, Matrix4x4 FromCone);

internal sealed record AcesPeak(
    float Ceiling, float ForwardLimit, float S2, float M2, float LimitJ, float MidJ,
    float Saturation, float SaturationThreshold, float Compression, float ChromaScale,
    float FocusDistance, float BottomGammaInverse, ImmutableArray<float> ReachM);

internal sealed record AcesGamut(AcesPeak Peak, JmhSpace Limit, ImmutableArray<float> Hues, ImmutableArray<Vector3> Cusps, int SearchLow, int SearchHigh);

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class AcesTransform {
    private const int Degrees = 360;
    private const double AdaptingLuminance = 100d;
    private const double Surround = 1.15d;
    private const double ToeFlare = 0.04d;
    private const float LightnessScale = 100f;
    private const float ConeOffset = 27.13f;
    private const float ConeExponent = 0.42f;
    private const float SmoothCusps = 0.12f;

    private static readonly double Adaptation = double.Pow(1d / ((5d * AdaptingLuminance) + 1d), 4d) switch {
        var k4 => (0.2d * k4 * 5d * AdaptingLuminance) + (0.1d * double.Pow(1d - k4, 2d) * double.Cbrt(5d * AdaptingLuminance)),
    };
    private static readonly float LuminanceScale = (float)(Adaptation / (float)Nits.ReferenceWhite);
    private static readonly float ModelGamma = (float)(0.59d * (1.48d + double.Sqrt(20d / (float)Nits.ReferenceWhite)));
    private static readonly float WhiteResponse = Compressed((float)Adaptation);
    private static readonly Matrix4x4 ConeToAab = (1d / ((2d + 1d + (1d / 20d)) * WhiteResponse), 400d * 43d * 0.9d) switch {
        var (achromatic, chromatic) => new Matrix4x4(
            (float)(2d * achromatic), (float)chromatic, (float)(chromatic / 9d), 0f,
            (float)achromatic, (float)(-12d / 11d * chromatic), (float)(chromatic / 9d), 0f,
            (float)(achromatic / 20d), (float)(chromatic / 11d), (float)(-2d / 9d * chromatic), 0f,
            0f, 0f, 0f, 1f),
    };
    private static readonly Matrix4x4 AabToCone = Inverted(ConeToAab);
    private static readonly JmhSpace Reach = Space(Gamut.Acescg);
    private static readonly Func<(Gamut Limit, Nits Peak), AcesGamut> Gamuts = memoUnsafe<(Gamut Limit, Nits Peak), AcesGamut>(Limited);

    internal static PixelPass Formed(ToneMapping _, PassContext context) =>
        context.Display.Map(
            srgb: Gamut.StandardRgb, displayP3: Gamut.DisplayP3, rec1886: Gamut.StandardRgb, rec2020: Gamut.DisplayP3,
            rec2100PqHdr: Gamut.Rec2020, rec2100PqSdr: Gamut.StandardRgb, rec2100HlgHdr: Gamut.Rec2020, rec2100HlgSdr: Gamut.StandardRgb) switch {
                var limit => Gamuts((limit, context.Display.Peak)) switch {
                    var gamut => Formation.Formed(
                        Seq(new LutTable.Affine(context.Working.MatrixTo(Gamut.Acescg), Vector3.Zero).Apply, row => {
                            foreach (ref Vector4 pixel in row)
                                pixel = new Vector4(Rendered(pixel.AsVector3(), gamut), pixel.W);
                        }),
                        new ColorEncoding(limit, TransferCurve.Linear, Nits.ReferenceWhite), context.Display),
                },
            };

    private static Vector3 Rendered(Vector3 scene, AcesGamut gamut) {
        AcesPeak peak = gamut.Peak;
        Vector3 aab = Aab(Vector3.Clamp(scene, Vector3.Zero, new Vector3(peak.ForwardLimit)), Reach);
        Vector3 jmh = Jmh(aab);
        (float sin, float cos) = float.SinCos(float.DegreesToRadians(jmh.Z));
        float reach = float.Lerp(peak.ReachM[(int)jmh.Z + 1], peak.ReachM[(int)jmh.Z + 2], jmh.Z - (int)jmh.Z);
        Vector2 compressed = GamutCompressed(ChromaCompressed(jmh, Toned(aab.X, peak), Norm(cos, sin, peak.ChromaScale), reach, peak), jmh.Z, reach, gamut);
        return Vector3.Clamp(Rgb(Aab(compressed.X, compressed.Y, cos, sin), gamut.Limit), Vector3.Zero, new Vector3(peak.Ceiling));
    }

    private static float Toned(float achromatic, AcesPeak peak) =>
        (Uncompressed(WhiteResponse * float.Abs(achromatic)) / LuminanceScale / (float)Nits.ReferenceWhite) switch {
            var scene => (peak.M2 * float.Pow(scene / (scene + peak.S2), (float)Surround)) switch {
                var flared => float.CopySign(Lightness(float.Max(0f, flared * flared / (flared + (float)ToeFlare)) * (float)Nits.ReferenceWhite), achromatic),
            },
        };

    private static Vector2 ChromaCompressed(Vector3 jmh, float toned, float norm, float reach, AcesPeak peak) {
        float relative = toned / peak.LimitJ;
        float remaining = float.Max(0f, 1f - relative);
        float limit = float.Pow(relative, 1f / ModelGamma) * reach / norm;
        float expanded = limit - Toe(limit - (jmh.Y * float.Pow(toned / jmh.X, 1f / ModelGamma) / norm), limit - 0.001f, remaining * peak.Saturation, float.Sqrt((relative * relative) + peak.SaturationThreshold));
        return new(toned, jmh.Y == 0f ? 0f : Toe(expanded, limit, relative * peak.Compression, remaining) * norm);
    }

    private static Vector2 GamutCompressed(Vector2 jm, float hue, float reach, AcesGamut gamut) =>
        jm switch {
            { X: <= 0f } => Vector2.Zero,
            _ when jm.Y <= 0f || jm.X > gamut.Peak.LimitJ => new(jm.X, 0f),
            _ => Interval(hue, gamut) switch {
                var upper => Mapped(jm, reach, gamut.Peak, Vector3.Lerp(gamut.Cusps[upper - 1], gamut.Cusps[upper], (hue - gamut.Hues[upper - 1]) / (gamut.Hues[upper] - gamut.Hues[upper - 1]))),
            },
        };

    private static Vector2 Mapped(Vector2 jm, float reach, AcesPeak peak, Vector3 cusp) {
        (float source, float slope, float crossing) = Projected(jm.X, jm.Y, cusp, peak);
        float boundary = Boundary(cusp, peak, source, slope, crossing);
        return boundary <= 0f
            ? new(jm.X, 0f)
            : Remapped(jm.Y, boundary, Crossed(source, slope, 1f / ModelGamma, peak.LimitJ, reach, peak.LimitJ)) switch {
                var remapped => new(source + (remapped * slope), remapped),
            };
    }

    private static int Interval(float hue, AcesGamut gamut) {
        int nominal = 1 + (int)hue;
        int low = int.Max(0, nominal + gamut.SearchLow);
        int found = gamut.Hues.AsSpan()[(low + 1)..int.Min(Degrees + 1, nominal + gamut.SearchHigh)].BinarySearch(hue);
        return int.Max(1, low + 1 + int.Max(found, ~found));
    }

    private static (float Source, float Slope, float Crossing) Projected(float j, float m, Vector3 cusp, AcesPeak peak) {
        float focus = float.Lerp(cusp.X, peak.MidJ, float.Min(1f, 1.3f - (cusp.X / peak.LimitJ)));
        float threshold = float.Lerp(cusp.X, peak.LimitJ, 0.3f);
        float gain = peak.LimitJ * peak.FocusDistance * (j > threshold ? float.Pow(float.Log10((peak.LimitJ - threshold) / float.Max(0.0001f, peak.LimitJ - j)), 2f) + 1f : 1f);
        float source = Intersected(j, m, focus, peak.LimitJ, gain);
        return (source, (source < focus ? source : peak.LimitJ - source) * (source - focus) / (focus * gain), Intersected(cusp.X, cusp.Y, focus, peak.LimitJ, gain));
    }

    private static float Intersected(float j, float m, float focus, float limit, float gain) {
        float scaled = m / gain;
        float a = scaled / focus;
        (float b, float c) = j < focus ? (1f - scaled, -j) : (-(1f + scaled + (limit * a)), (limit * scaled) + j);
        float root = float.Sqrt((b * b) - (4f * a * c));
        return -2f * c / (j < focus ? b + root : b - root);
    }

    private static float Boundary(Vector3 cusp, AcesPeak peak, float source, float slope, float crossing) {
        float lower = Crossed(source, slope, peak.BottomGammaInverse, cusp.X, cusp.Y, crossing);
        float upper = Crossed(peak.LimitJ - source, -slope, cusp.Z, peak.LimitJ - cusp.X, cusp.Y, peak.LimitJ - crossing);
        float smoothing = SmoothCusps * cusp.Y;
        return float.Min(lower, upper) - (float.Pow(float.Max(smoothing - float.Abs(lower - upper), 0f) / smoothing, 3f) * smoothing / 6f);
    }

    private static float Crossed(float axis, float slope, float inverseGamma, float jMax, float mMax, float reference) =>
        reference * float.Pow(axis / reference, inverseGamma) * mMax / (jMax - (slope * mMax));

    private static float Remapped(float m, float gamut, float reach) {
        float proportion = float.Max(gamut / reach, 0.75f);
        float threshold = proportion * gamut;
        float scale = (reach - threshold) / (((reach - threshold) / (gamut - threshold)) - 1f);
        float distance = (m - threshold) / scale;
        return m <= threshold || proportion >= 1f ? m : threshold + (scale * distance / (1f + distance));
    }

    private static float Toe(float value, float limit, float shoulder, float floor) {
        float k2 = float.Max(floor, 0.001f);
        float k1 = float.Sqrt((shoulder * shoulder) + (k2 * k2));
        float k3 = (limit + k1) / (limit + k2);
        float b = (k3 * value) - k1;
        return value > limit ? value : 0.5f * (b + float.Sqrt((b * b) + (4f * k2 * k3 * value)));
    }

    private static float Norm(float cos, float sin, float scale) =>
        scale * ((11.34072f * cos) + (16.46899f * ((2f * cos * cos) - 1f)) + (7.88380f * ((4f * cos * cos * cos) - (3f * cos)))
                 + (14.66441f * sin) - (6.37224f * 2f * cos * sin) + (9.19364f * ((3f * sin) - (4f * sin * sin * sin))) + 77.12896f);

    private static Vector3 Jmh(Vector3 aab) =>
        aab.X <= 0f ? Vector3.Zero
        : new(LightnessScale * float.Pow(aab.X, ModelGamma), float.Sqrt((aab.Y * aab.Y) + (aab.Z * aab.Z)),
            float.RadiansToDegrees(float.Atan2(aab.Z, aab.Y)) switch { var hue => hue < 0f ? hue + Degrees : hue });

    private static Vector3 Aab(float j, float m, float cos, float sin) => new(float.Pow(j / LightnessScale, 1f / ModelGamma), m * cos, m * sin);

    private static Vector3 Aab(Vector3 rgb, JmhSpace space) => Vector3.Transform(Compressed(Vector3.Transform(rgb, space.ToCone)), ConeToAab);

    private static Vector3 Rgb(Vector3 aab, JmhSpace space) => Vector3.Transform(Uncompressed(Vector3.Transform(aab, AabToCone)), space.FromCone);

    private static float Lightness(float luminance) => LightnessScale * float.Pow(Compressed(luminance * LuminanceScale) / WhiteResponse, ModelGamma);

    private static float Compressed(float value) =>
        float.Pow(float.Abs(value), ConeExponent) switch { var response => float.CopySign(response / (ConeOffset + response), value) };

    private static Vector3 Compressed(Vector3 value) => new(Compressed(value.X), Compressed(value.Y), Compressed(value.Z));

    private static float Uncompressed(float value) =>
        float.Min(float.Abs(value), 0.99f) switch { var limited => float.CopySign(float.Pow(ConeOffset * limited / (1f - limited), 1f / ConeExponent), value) };

    private static Vector3 Uncompressed(Vector3 value) => new(Uncompressed(value.X), Uncompressed(value.Y), Uncompressed(value.Z));

    private static Matrix4x4 Inverted(Matrix4x4 matrix) {
        _ = Matrix4x4.Invert(matrix, out Matrix4x4 inverse);
        return inverse;
    }

    private static JmhSpace Space(Gamut gamut) {
        RgbConfiguration rgb = gamut.Configuration.Rgb;
        Configuration own = new(rgb, new XyzConfiguration(rgb.WhitePoint));
        Configuration sharpened = new WhitePoint(0.333d, 0.333d) switch {
            var white => new(
                new RgbConfiguration(new Chromaticity(0.8336d, 0.1735d), new Chromaticity(2.3854d, -1.4659d), new Chromaticity(0.087d, -0.125d), white, static linear => linear, static linear => linear),
                new XyzConfiguration(white)),
        };
        (double, double, double) Cone((double, double, double) value) => new Unicolour(sharpened, ColourSpace.Xyz, new Unicolour(own, ColourSpace.RgbLinear, value).Xyz.Tuple).RgbLinear.Tuple;
        (double red, double green, double blue) = Cone((1d, 1d, 1d));
        ColorMatrix3x3 columns = Gamut.Columns(basis => Cone(basis) switch {
            var (r, g, b) => (Adaptation * r / red, Adaptation * g / green, Adaptation * b / blue),
        });
        Matrix4x4 toCone = new(columns.M11, columns.M21, columns.M31, 0f, columns.M12, columns.M22, columns.M32, 0f, columns.M13, columns.M23, columns.M33, 0f, 0f, 0f, 0f, 1f);
        return new(toCone, Inverted(toCone));
    }

    private static AcesPeak Peaked(Nits nits) {
        double peak = (float)nits;
        double ceiling = peak / (float)Nits.ReferenceWhite;
        double hit = double.Lerp(128d, 896d, double.Log(ceiling) / double.Log((float)Nits.MaxValue / (float)Nits.ReferenceWhite));
        double m1 = 0.5d * (ceiling + double.Sqrt(ceiling * (ceiling + (4d * ToeFlare))));
        double scaled = hit / m1;
        double m = m1 / double.Pow(scaled / (scaled + 1d), Surround);
        double grey = 10.013d / (float)Nits.ReferenceWhite * (1d + (double.Log2(ceiling) * 0.14d));
        double lifted = double.Pow(0.5d * (grey + double.Sqrt(grey * (grey + (4d * ToeFlare)))) / m, 1d / Surround);
        double greyWeight = Exposure.MiddleGrey / (-(m1 * lifted) / (lifted - 1d));
        double logPeak = double.Log10(ceiling);
        float limitJ = Lightness((float)peak);
        return new(
            (float)ceiling, (float)(8d * hit), (float)(greyWeight * m1), (float)(m1 / double.Pow(scaled / (scaled + greyWeight), Surround)), limitJ, Lightness((float)(grey * (float)Nits.ReferenceWhite)),
            (float)double.Max(0.2d, 1.3d - (1.3d * 0.69d * logPeak)), (float)(0.5d / peak), (float)(2.4d + (2.4d * 3.3d * logPeak)), (float)(double.Pow(0.03379d * peak, 0.30596d) - 0.45135d),
            (float)(1.35d + (1.35d * 1.75d * logPeak)), (float)(1d / (1.14d + (0.07d * logPeak))),
            Wrapped(toSeq(Range(0, Degrees)).Map(hue => Reached(hue, limitJ)), static (value, _) => value));
    }

    private static float Reached(float hue, float limitJ) {
        (float sin, float cos) = float.SinCos(float.DegreesToRadians(hue));
        bool Outside(float m) => Rgb(Aab(limitJ, m, cos, sin), Reach) is var rgb && (rgb.X < 0f || rgb.Y < 0f || rgb.Z < 0f);
        (float Low, float High) Widened(float low, float high) => high < 1300f && !Outside(high) ? Widened(high, high + 50f) : (low, high);
        float Narrowed(float low, float high) => high - low > 1e-2f ? ((low + high) / 2f) switch { var middle => Outside(middle) ? Narrowed(low, middle) : Narrowed(middle, high) } : high;
        return Widened(0f, 50f) switch { var (low, high) => Narrowed(low, high) };
    }

    private static AcesGamut Limited((Gamut Limit, Nits Peak) key) {
        AcesPeak peak = Peaked(key.Peak);
        JmhSpace limit = Space(key.Limit);
        float limitA = float.Pow(peak.LimitJ / LightnessScale, 1f / ModelGamma);
        Seq<Vector3> corners = Seq(new Vector3(1f, 0f, 0f), new Vector3(1f, 1f, 0f), new Vector3(0f, 1f, 0f), new Vector3(0f, 1f, 1f), new Vector3(0f, 0f, 1f), new Vector3(1f, 0f, 1f));
        float Scaled(Vector3 corner, float lower, float upper) =>
            upper - lower > 1e-3f
                ? ((lower + upper) / 2f) switch { var test => Aab(test * corner, Reach).X switch { var a when a < limitA => Scaled(corner, test, upper), var a when a == limitA => test, _ => Scaled(corner, lower, test) } }
                : upper;
        ImmutableArray<(Vector3 Rgb, Vector3 Jmh)> reached = Cycled(corners.Map(corner => (Scaled(corner, 0f, peak.ForwardLimit) * corner) switch { var rgb => (rgb, Jmh(Aab(rgb, Reach))) }));
        ImmutableArray<(Vector3 Rgb, Vector3 Jmh)> bounded = Cycled(corners.Map(corner => (corner * peak.Ceiling) switch { var rgb => (rgb, Jmh(Aab(rgb, limit))) }));
        Seq<float> nominal = Nominal(toSeq(toSeq(Range(1, 6)).Bind(i => Seq(reached[i].Jmh.Z, bounded[i].Jmh.Z)).Distinct().Order()));
        Seq<Vector3> cusps = nominal.Fold((Cusps: Seq<Vector2>(), Segment: 0, Position: 0f), (state, hue) => Cusp(hue, bounded, limit, state.Segment, state.Position) switch {
            var (cusp, segment, position) => (state.Cusps.Add(cusp * new Vector2(1f, 1f + (0.27f * SmoothCusps))), segment, position),
        }).Cusps.Zip(nominal).Map(pair => new Vector3(pair.First, Hull(pair.First, pair.Second, peak, limit)));
        Seq<int> offsets = nominal.Map(static (hue, index) => index - (int)hue);
        return new(peak, limit, Wrapped(nominal, static (hue, turn) => hue + (Degrees * turn)), Wrapped(cusps, static (cusp, _) => cusp), offsets.Fold(0, int.Min), offsets.Fold(0, int.Max) + 1);
    }

    private static ImmutableArray<(Vector3 Rgb, Vector3 Jmh)> Cycled(Seq<(Vector3 Rgb, Vector3 Jmh)> corners) =>
        Range(0, corners.Count).Fold(0, (least, i) => corners[i].Jmh.Z < corners[least].Jmh.Z ? i : least) switch {
            var least => [.. Range(0, corners.Count + 2).Select(k => corners[(k + corners.Count - 1 + least) % corners.Count] switch {
                var corner => (corner.Rgb, corner.Jmh with { Z = corner.Jmh.Z + (Degrees * (((k + corners.Count - 1) / corners.Count) - 1)) }),
            })],
        };

    private static Seq<float> Nominal(Seq<float> sorted) =>
        sorted.Fold((Counts: Seq<int>(), Last: -1, Floor: sorted.Head.Map(static hue => hue == 0f ? 0 : 1).IfNone(1)), static (state, hue) =>
            int.Min(int.Max((int)float.Round(hue, MidpointRounding.AwayFromZero), state.Floor), Degrees - 1) switch {
                var index when index != state.Last => (state.Counts.Add(index), index, index),
                var index when state.Counts is [.., var before, var last] && before != last - 1 => (state.Counts.Init.Add(last - 1).Add(index), index, index),
                var index => (state.Counts.Add(int.Min(index + 1, Degrees - 1)), index + 1, index + 1),
            }).Counts switch {
                var counts => (Seq(0f) + sorted).Zip(sorted.Add(Degrees)).Zip((Seq(0) + counts).Zip(counts.Add(Degrees))).Bind(static span => Spread(span.First, span.Second)),
            };

    private static Seq<float> Spread((float Low, float High) hues, (int Low, int High) slots) =>
        toSeq(Range(0, slots.High - slots.Low)).Map(i => hues.Low + (i * ((hues.High - hues.Low) / (slots.High - slots.Low))));

    private static (Vector2 Cusp, int Segment, float Position) Cusp(float hue, ImmutableArray<(Vector3 Rgb, Vector3 Jmh)> corners, JmhSpace limit, int segment, float position) {
        int upper = 1 + toSeq(Range(1, corners.Length - 1)).Filter(i => corners[i].Jmh.Z <= hue).Count;
        ((Vector3 lowerRgb, Vector3 lowerJmh), (Vector3 upperRgb, Vector3 upperJmh)) = (corners[upper - 1], corners[upper]);
        Vector3 Sampled(float t) => Jmh(Aab(Vector3.Lerp(lowerRgb, upperRgb, t), limit));
        float Split(float low, float high, float t, float h) => h < lowerJmh.Z || (h < upperJmh.Z && h > hue) ? Bisected(low, t) : Bisected(t, high);
        float Bisected(float low, float high) => high - low > 1e-7f ? ((low + high) / 2f) switch { var t => Split(low, high, t, Sampled(t).Z) } : (low + high) / 2f;
        return lowerJmh.Z == hue
            ? (new Vector2(lowerJmh.X, lowerJmh.Y), segment, position)
            : Bisected(upper == segment ? position : 0f, 1f) switch { var t => Sampled(t) switch { var jmh => (new Vector2(jmh.X, jmh.Y), upper, t) } };
    }

    private static float Hull(Vector2 cusp, float hue, AcesPeak peak, JmhSpace limit) {
        (float sin, float cos) = float.SinCos(float.DegreesToRadians(hue));
        Vector3 point = new(cusp, 0f);
        Seq<(float Source, float Slope, float Crossing)> tests = Seq(0.01f, 0.1f, 0.5f, 0.8f, 0.99f).Map(position => Projected(float.Lerp(cusp.X, peak.LimitJ, position), cusp.Y, point, peak)).Strict();
        bool Fits(float gamma) => tests.ForAll(test => Boundary(point with { Z = 1f / gamma }, peak, test.Source, test.Slope, test.Crossing) switch {
            var m => Rgb(Aab(test.Source + (test.Slope * m), m, cos, sin), limit) is var rgb && (rgb.X > peak.Ceiling || rgb.Y > peak.Ceiling || rgb.Z > peak.Ceiling),
        });
        (float Low, float High) Widened(float low, float high) => high < 5f && !Fits(high) ? Widened(high, high + 0.4f) : (low, high);
        float Narrowed(float low, float high) => high - low > 1e-5f ? ((low + high) / 2f) switch { var middle => Fits(middle) ? Narrowed(low, middle) : Narrowed(middle, high) } : high;
        return 1f / (Widened(0f, 0.4f) switch { var (low, high) => Narrowed(low, high) });
    }

    private static ImmutableArray<T> Wrapped<T>(Seq<T> nominal, Func<T, int, T> turned) =>
        [.. Range(0, Degrees + 3).Select(k => turned(nominal[(k + Degrees - 1) % Degrees], ((k + Degrees - 1) / Degrees) - 1))];
}
