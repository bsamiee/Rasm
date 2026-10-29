using System.Numerics;
using MathNet.Numerics.RootFinding;
using Rasm.Imaging.Color;
using Wacton.Unicolour;

namespace Rasm.Imaging.Tone;

// --- [MODELS] --------------------------------------------------------------------------
public sealed partial record ViewTransform {
    private const int Hues = 360;
    private const int SearchIterations = 100;
    private const float ReferenceLuminance = 100f;

    private static readonly (float C, float Nc, float Background) DimSurround = (0.59f, 0.9f, 20f);
    private static readonly (float Exponent, float Offset, float Scale) ConeResponse = (0.42f, 0.2713f * ReferenceLuminance, 4f * ReferenceLuminance);
    private static readonly (float Contrast, float Flare, float GrayLuminance, float GrayShift, float RoofLow, float RoofHigh, float St2084Peak) Tonescale = (1.15f, 0.04f, 10.013f, 0.14f, 128f, 896f, 10_000f);
    private static readonly (float Compress, float CompressFactor, float Expand, float ExpandFactor, float ExpandThreshold) Chroma = (2.4f, 3.3f, 1.3f, 0.69f, 0.5f);
    private static readonly (float SmoothCusps, float SmoothM, float CuspMidBlend, float FocusGainBlend, float FocusDistance, float FocusScaling, float CompressionThreshold) Mapping = (0.12f, 0.27f, 1.3f, 0.3f, 1.35f, 1.75f, 0.75f);
    private static readonly (double Reach, double Display, double ReachM, double ReachMaximum) Tolerance = (1e-3, 1e-7, 1e-2, 1300d);
    private static readonly (double Minimum, double Maximum, double Step, double Accuracy) HullGamma = (0d, 5d, 0.4d, 1e-5);
    private static readonly Seq<float> HullTests = [0.01f, 0.1f, 0.5f, 0.8f, 0.99f];
    private static readonly Seq<Vector3> Corners = [new(1f, 0f, 0f), new(1f, 1f, 0f), new(0f, 1f, 0f), new(0f, 1f, 1f), new(0f, 0f, 1f), new(1f, 0f, 1f)];

    private static readonly float LuminanceAdaptation = (float)(double.Pow(1d / ((5d * ReferenceLuminance) + 1d), 4d) switch {
        var k4 => (0.2d * k4 * 5d * ReferenceLuminance) + (0.1d * double.Pow(1d - k4, 2d) * double.Cbrt(5d * ReferenceLuminance)),
    });
    private static readonly float WhiteResponse = Compressed(LuminanceAdaptation);
    private static readonly float ModelGamma = DimSurround.C * (1.48f + float.Sqrt(DimSurround.Background / ReferenceLuminance));
    private static readonly Matrix4x4 ConeToAab = (1f / ((2f + 1f + (1f / 20f)) * WhiteResponse), ConeResponse.Scale * 43f * DimSurround.Nc) switch {
        var (a, c) => new(2f * a, c, c / 9f, 0f, a, -12f / 11f * c, c / 9f, 0f, a / 20f, c / 11f, -2f / 9f * c, 0f, 0f, 0f, 0f, 1f),
    };
    private static readonly Configuration Sharpened = new WhitePoint(0.333d, 0.333d) switch {
        var white => new(new RgbConfiguration(new Chromaticity(0.8336d, 0.1735d), new Chromaticity(2.3854d, -1.4659d), new Chromaticity(0.087d, -0.125d), white, identity, identity), new XyzConfiguration(white)),
    };
    private static readonly Func<(Gamut Limit, float Peak), Fin<Rendering>> Rendered = memoUnsafe<(Gamut Limit, float Peak), Fin<Rendering>>(Built);

    public static Fin<ViewTransform> Aces(DisplayDevice device, bool inverse = false) =>
        (new ColorEncoding(Gamut.Acescg, new Transfer.Linear()), new ColorEncoding(device.LimitingGamut, new Transfer.Linear())) switch {
            var (scene, display) => Rendered((device.LimitingGamut, (float)device.Peak.Nits.ToDouble())).Map(rendering => inverse
                ? new ViewTransform(display, [ToneCurve.Graded(color => Inverse(color, rendering))], Some(scene))
                : new ViewTransform(scene, [ToneCurve.Graded(color => Forward(color, rendering))], Some(display))),
        };

    private static Vector3 Forward(Vector3 scene, Rendering p) {
        Vector3 jmh = Jmh(Aab(Vector3.Clamp(scene, Vector3.Zero, new Vector3(p.ForwardLimit)), p.Reach.ToCone));
        float toned = Toned(jmh.X, p);
        Vector3 compressed = GamutCompressed(new Vector3(toned, ChromaCompressed(jmh, toned, p, invert: false), jmh.Z), p, invert: false);
        return Vector3.Clamp(Rgb(Aab(compressed), p, p.Limit.FromCone), Vector3.Zero, new Vector3(p.Ceiling));
    }

    private static Vector3 Inverse(Vector3 display, Rendering p) {
        Vector3 toned = GamutCompressed(Jmh(Aab(display, p.Limit.ToCone)), p, invert: true);
        Vector3 jmh = toned with { X = Untoned(toned.X, p) };
        return Rgb(Aab(jmh with { Y = ChromaCompressed(toned with { X = jmh.X }, toned.X, p, invert: true) }), p, p.Reach.FromCone);
    }

    private static float Toned(float lightness, Rendering p) =>
        (LuminanceOf(lightness) / ReferenceLuminance) switch {
            var linear => (p.M2 * float.Pow(float.Max(0f, linear) / (linear + p.S2), Tonescale.Contrast)) switch {
                var f => Lightness(float.Max(0f, f * f / (f + Tonescale.Flare)) * ReferenceLuminance),
            },
        };

    private static float Untoned(float toned, Rendering p) =>
        float.Max(0f, float.Min(p.InverseLimit, LuminanceOf(toned) / ReferenceLuminance)) switch {
            var z => Lightness(p.S2 / (float.Pow(p.M2 / ((z + float.Sqrt(z * ((4f * Tonescale.Flare) + z))) / 2f), 1f / Tonescale.Contrast) - 1f) * ReferenceLuminance),
        };

    private static float ChromaCompressed(Vector3 jmh, float toned, Rendering p, bool invert) {
        float nJ = toned / p.LimitJ;
        float snJ = float.Max(0f, 1f - nJ);
        float norm = Norm(jmh.Z) * p.ChromaScale;
        float limit = float.Pow(nJ, 1f / ModelGamma) * ReachM(jmh.Z, p) / norm;
        float ratio = float.Pow(toned / jmh.X, 1f / ModelGamma);
        (float expansion, float threshold, float compression) = (snJ * p.Saturation, float.Sqrt((nJ * nJ) + p.SaturationThreshold), nJ * p.Compression);
        return jmh.Y is 0f ? 0f
            : invert ? (limit - Toe(limit - Toe(jmh.Y / norm, limit, compression, snJ, invert), limit - 0.001f, expansion, threshold, invert)) * norm / ratio
            : Toe(limit - Toe(limit - (jmh.Y * ratio / norm), limit - 0.001f, expansion, threshold, invert), limit, compression, snJ, invert) * norm;
    }

    private static Vector3 GamutCompressed(Vector3 jmh, Rendering p, bool invert) {
        if (jmh.X <= 0f)
            return new(0f, 0f, jmh.Z);
        if (jmh.Y < 0f || jmh.X > p.LimitJ)
            return jmh with { Y = 0f };
        int upper = int.Clamp(p.HueTable.AsSpan().BinarySearch(jmh.Z) switch { var found => int.Max(found, ~found) }, 1, Hues + 1);
        Vector3 cusp = Vector3.Lerp(p.Cusps[upper - 1], p.Cusps[upper], (jmh.Z - p.HueTable[upper - 1]) / (p.HueTable[upper] - p.HueTable[upper - 1]));
        float approximated = invert && jmh.X > float.Lerp(cusp.X, p.LimitJ, Mapping.FocusGainBlend) ? Mapped(jmh, jmh.X, cusp, p, invert).X : jmh.X;
        return Mapped(jmh, approximated, cusp, p, invert);
    }

    private static Vector3 Mapped(Vector3 jmh, float approximated, Vector3 cusp, Rendering p, bool invert) {
        (float source, float slope, float crossing) = Projected(jmh.X, jmh.Y, approximated, cusp, p);
        float boundary = Boundary(cusp, source, slope, crossing, p);
        return boundary <= 0f
            ? jmh with { Y = 0f }
            : Remapped(jmh.Y, boundary, Crossed(source, slope, 1f / ModelGamma, p.LimitJ, ReachM(jmh.Z, p), p.LimitJ), invert) switch {
                var m => new(source + (m * slope), m, jmh.Z),
            };
    }

    private static (float Source, float Slope, float Crossing) Projected(float j, float m, float approximated, Vector3 cusp, Rendering p) {
        float focus = float.Lerp(cusp.X, p.MidJ, float.Min(1f, Mapping.CuspMidBlend - (cusp.X / p.LimitJ)));
        float threshold = float.Lerp(cusp.X, p.LimitJ, Mapping.FocusGainBlend);
        float gain = p.LimitJ * p.FocusDistance * (approximated > threshold ? float.Pow(float.Log10((p.LimitJ - threshold) / float.Max(0.0001f, p.LimitJ - approximated)), 2f) + 1f : 1f);
        float source = Intersected(j, m, focus, p.LimitJ, gain);
        return (source, (source < focus ? source : p.LimitJ - source) * (source - focus) / (focus * gain), Intersected(cusp.X, cusp.Y, focus, p.LimitJ, gain));
    }

    private static float Intersected(float j, float m, float focus, float limit, float gain) {
        float scaled = m / gain;
        float a = scaled / focus;
        (float b, float c) = j < focus ? (1f - scaled, -j) : (-(1f + scaled + (limit * a)), (limit * scaled) + j);
        float root = float.Sqrt((b * b) - (4f * a * c));
        return -2f * c / (j < focus ? b + root : b - root);
    }

    private static float Boundary(Vector3 cusp, float source, float slope, float crossing, Rendering p) {
        float lower = Crossed(source, slope, p.LowerHullGammaInverse, cusp.X, cusp.Y, crossing);
        float upper = Crossed(p.LimitJ - source, -slope, cusp.Z, p.LimitJ - cusp.X, cusp.Y, p.LimitJ - crossing);
        float smoothing = Mapping.SmoothCusps * cusp.Y;
        return float.Min(lower, upper) - (float.Pow(float.Max(smoothing - float.Abs(lower - upper), 0f) / smoothing, 3f) * smoothing / 6f);
    }

    private static float Crossed(float axis, float slope, float inverseGamma, float jMax, float mMax, float reference) =>
        reference * float.Pow(axis / reference, inverseGamma) * mMax / (jMax - (slope * mMax));

    private static float Remapped(float m, float gamut, float reach, bool invert) {
        float proportion = float.Max(gamut / reach, Mapping.CompressionThreshold);
        float threshold = proportion * gamut;
        float scale = (reach - threshold) / (((reach - threshold) / (gamut - threshold)) - 1f);
        float distance = (m - threshold) / scale;
        return m <= threshold || proportion >= 1f ? m
            : threshold + (invert ? distance >= 1f ? scale : scale * -(distance / (distance - 1f)) : scale * distance / (1f + distance));
    }

    private static float Toe(float value, float limit, float shoulder, float floor, bool invert) {
        float k2 = float.Max(floor, 0.001f);
        float k1 = float.Sqrt((shoulder * shoulder) + (k2 * k2));
        float k3 = (limit + k1) / (limit + k2);
        float b = (k3 * value) - k1;
        return value > limit ? value
            : invert ? ((value * value) + (k1 * value)) / (k3 * (value + k2))
            : 0.5f * (b + float.Sqrt((b * b) + (4f * k2 * k3 * value)));
    }

    private static float Norm(float hue) =>
        float.SinCos(float.DegreesToRadians(hue)) switch {
            var (sin, cos) => (11.34072f * cos) + (16.46899f * ((2f * cos * cos) - 1f)) + (7.88380f * ((4f * cos * cos * cos) - (3f * cos)))
                + (14.66441f * sin) - (6.37224f * 2f * cos * sin) + (9.19364f * ((3f * sin) - (4f * sin * sin * sin))) + 77.12896f,
        };

    private static float ReachM(float hue, Rendering p) => float.Lerp(p.ReachLimits[(int)hue + 1], p.ReachLimits[(int)hue + 2], hue - (int)hue);

    private static Vector3 Jmh(Vector3 aab) =>
        aab.X <= 0f ? Vector3.Zero
        : new(ReferenceLuminance * float.Pow(aab.X, ModelGamma), float.Sqrt((aab.Y * aab.Y) + (aab.Z * aab.Z)),
            float.RadiansToDegrees(float.Atan2(aab.Z, aab.Y)) switch { var hue => hue < 0f ? hue + Hues : hue });

    private static Vector3 Aab(Vector3 jmh) =>
        float.SinCos(float.DegreesToRadians(jmh.Z)) switch { var (sin, cos) => new(float.Pow(jmh.X / ReferenceLuminance, 1f / ModelGamma), jmh.Y * cos, jmh.Y * sin) };

    private static Vector3 Aab(Vector3 rgb, Matrix4x4 toCone) =>
        Vector3.Transform(Vector3.Transform(rgb, toCone) switch { var cone => new Vector3(Compressed(cone.X), Compressed(cone.Y), Compressed(cone.Z)) }, ConeToAab);

    private static Vector3 Rgb(Vector3 aab, Rendering p, Matrix4x4 fromCone) =>
        Vector3.Transform(Vector3.Transform(aab, p.AabToCone) switch { var cone => new Vector3(Uncompressed(cone.X), Uncompressed(cone.Y), Uncompressed(cone.Z)) }, fromCone);

    private static float Lightness(float luminance) =>
        float.CopySign(ReferenceLuminance * float.Pow(Compressed(float.Abs(luminance) * LuminanceAdaptation / ReferenceLuminance) / WhiteResponse, ModelGamma), luminance);

    private static float LuminanceOf(float lightness) =>
        Uncompressed(WhiteResponse * float.Pow(float.Abs(lightness) / ReferenceLuminance, 1f / ModelGamma)) * ReferenceLuminance / LuminanceAdaptation;

    private static float Compressed(float value) =>
        float.Pow(float.Abs(value), ConeResponse.Exponent) switch { var response => float.CopySign(response / (ConeResponse.Offset + response), value) };

    private static float Uncompressed(float value) =>
        float.Min(float.Abs(value), 0.99f) switch { var limited => float.CopySign(float.Pow(ConeResponse.Offset * limited / (1f - limited), 1f / ConeResponse.Exponent), value) };

    private static (Matrix4x4 ToCone, Matrix4x4 FromCone) Cones(Gamut gamut) {
        Configuration own = new(gamut.Rgb, new XyzConfiguration(gamut.Rgb.WhitePoint));
        static Vector3 Converted(Configuration from, Configuration to, Vector3 rgb) =>
            new Unicolour(to, ColourSpace.Xyz, new Unicolour(from, ColourSpace.RgbLinear, rgb.X, rgb.Y, rgb.Z).Xyz.Tuple).RgbLinear.Tuple switch {
                var (red, green, blue) => new((float)red, (float)green, (float)blue),
            };
        Vector3 white = Converted(own, Sharpened, Vector3.One);
        return (Gamut.Columns(rgb => Converted(own, Sharpened, rgb) * LuminanceAdaptation / white), Gamut.Columns(cone => Converted(Sharpened, own, cone * white / LuminanceAdaptation)));
    }

    private static Fin<float> Root(Func<double, double> margin, double lower, double upper, double accuracy) =>
        margin(upper) >= 0d ? (float)upper
        : Brent.TryFindRoot(margin, lower, upper, accuracy, SearchIterations, out double root) ? (float)root
        : ImagingFault.Of(ImagingReason.Range, (lower, upper));

    private static Fin<Matrix4x4> Inverted(Matrix4x4 matrix) => Matrix4x4.Invert(matrix, out Matrix4x4 inverse) ? inverse : ImagingFault.Of(ImagingReason.Degenerate, matrix);

    private static Fin<Rendering> Built((Gamut Limit, float Peak) key) =>
        from aabToCone in Inverted(ConeToAab)
        let ceiling = key.Peak / ReferenceLuminance
        let hit = float.Lerp(Tonescale.RoofLow, Tonescale.RoofHigh, float.Log(ceiling) / float.Log(Tonescale.St2084Peak / ReferenceLuminance))
        let m1 = 0.5f * (ceiling + float.Sqrt(ceiling * (ceiling + (4f * Tonescale.Flare))))
        let gray = Tonescale.GrayLuminance / ReferenceLuminance * (1f + (float.Log2(ceiling) * Tonescale.GrayShift))
        let lifted = float.Pow(0.5f * (gray + float.Sqrt(gray * (gray + (4f * Tonescale.Flare)))) / (m1 / float.Pow(hit / m1 / ((hit / m1) + 1f), Tonescale.Contrast)), 1f / Tonescale.Contrast)
        let weight = Metering.MiddleGray / (-(m1 * lifted) / (lifted - 1f))
        let u2 = float.Pow(hit / m1 / ((hit / m1) + weight), Tonescale.Contrast)
        let logPeak = float.Log10(ceiling)
        let peaked = new Rendering(
            Cones(Gamut.Acescg), Cones(key.Limit), aabToCone, ceiling, 8f * hit, ceiling / u2, weight * m1, m1 / u2, Lightness(key.Peak), Lightness(gray * ReferenceLuminance),
            float.Max(0.2f, Chroma.Expand - (Chroma.Expand * Chroma.ExpandFactor * logPeak)), Chroma.ExpandThreshold / key.Peak, Chroma.Compress + (Chroma.Compress * Chroma.CompressFactor * logPeak),
            float.Pow(0.03379f * key.Peak, 0.30596f) - 0.45135f, Mapping.FocusDistance + (Mapping.FocusDistance * Mapping.FocusScaling * logPeak), 1f / (1.14f + (0.07f * logPeak)))
        from reach in toSeq(Range(0, Hues)).Traverse(hue => Reached(hue, peaked)).As()
        let rendering = peaked with { ReachLimits = Wrapped(reach, static (m, _) => m) }
        from scales in Corners.Traverse(corner => Root(s => float.Pow(rendering.LimitJ / ReferenceLuminance, 1f / ModelGamma) - Aab((float)s * corner, rendering.Reach.ToCone).X, 0d, rendering.ForwardLimit, Tolerance.Reach)).As()
        let reached = Cycled(Corners.Zip(scales).Map(pair => (pair.First * pair.Second) switch { var rgb => (rgb, Jmh(Aab(rgb, rendering.Reach.ToCone))) }))
        let bounded = Cycled(Corners.Map(corner => (corner * ceiling) switch { var rgb => (rgb, Jmh(Aab(rgb, rendering.Limit.ToCone))) }))
        let nominal = Nominal(toSeq(toSeq(Range(1, Corners.Count)).Bind(i => Seq(reached[i].Jmh.Z, bounded[i].Jmh.Z)).Distinct().Order()))
        from cusps in nominal.Traverse(hue => Cusp(hue, bounded, rendering)).As()
        from hulls in cusps.Zip(nominal).Traverse(pair => Hull(pair.First, pair.Second, rendering)).As()
        select rendering with {
            HueTable = Wrapped(nominal, static (hue, turn) => hue + (Hues * turn)),
            Cusps = Wrapped(cusps.Zip(hulls).Map(static pair => new Vector3(pair.First, pair.Second)), static (cusp, _) => cusp),
        };

    private static Fin<float> Reached(float hue, Rendering p) =>
        float.SinCos(float.DegreesToRadians(hue)) switch {
            var (sin, cos) => Root(m => Rgb(new Vector3(float.Pow(p.LimitJ / ReferenceLuminance, 1f / ModelGamma), (float)m * cos, (float)m * sin), p, p.Reach.FromCone) switch {
                var rgb => float.Min(rgb.X, float.Min(rgb.Y, rgb.Z)),
            }, 0d, Tolerance.ReachMaximum, Tolerance.ReachM),
        };

    private static Fin<Vector2> Cusp(float hue, ImmutableArray<(Vector3 Rgb, Vector3 Jmh)> corners, Rendering p) {
        int upper = 1 + toSeq(Range(1, corners.Length - 1)).Filter(i => corners[i].Jmh.Z <= hue).Count;
        ((Vector3 lowerRgb, Vector3 lowerJmh), (Vector3 upperRgb, _)) = (corners[upper - 1], corners[upper]);
        Vector3 Sampled(double t) => Jmh(Aab(Vector3.Lerp(lowerRgb, upperRgb, (float)t), p.Limit.ToCone));
        Vector2 smoothed = new(1f, 1f + (Mapping.SmoothM * Mapping.SmoothCusps));
        return lowerJmh.Z == hue
            ? new Vector2(lowerJmh.X, lowerJmh.Y) * smoothed
            : Root(t => float.Ieee754Remainder(hue - Sampled(t).Z, Hues), 0d, 1d, Tolerance.Display).Map(t => Sampled(t) switch { var jmh => new Vector2(jmh.X, jmh.Y) * smoothed });
    }

    private static Fin<float> Hull(Vector2 cusp, float hue, Rendering p) {
        (float sin, float cos) = float.SinCos(float.DegreesToRadians(hue));
        Vector3 point = new(cusp, 0f);
        Seq<(float Source, float Slope, float Crossing)> tests = HullTests.Map(position => float.Lerp(cusp.X, p.LimitJ, position) switch { var j => Projected(j, cusp.Y, j, point, p) }).Strict();
        double Margin(double gamma) => tests.Max(test => Boundary(point with { Z = (float)(1d / gamma) }, test.Source, test.Slope, test.Crossing, p) switch {
            var m => Rgb(Aab(new Vector3(test.Source + (test.Slope * m), m, hue)), p, p.Limit.FromCone) switch { var rgb => 1f - (float.Max(rgb.X, float.Max(rgb.Y, rgb.Z)) / p.Ceiling) },
        });
        return Root(Margin, HullGamma.Minimum + HullGamma.Accuracy, HullGamma.Step * double.Ceiling(HullGamma.Maximum / HullGamma.Step), HullGamma.Accuracy).Map(static gamma => 1f / gamma);
    }

    private static ImmutableArray<(Vector3 Rgb, Vector3 Jmh)> Cycled(Seq<(Vector3 Rgb, Vector3 Jmh)> corners) {
        int least = Range(0, corners.Count).Fold(0, (lowest, i) => corners[i].Jmh.Z < corners[lowest].Jmh.Z ? i : lowest);
        return [.. Range(0, corners.Count + 2).Select(k => corners[(k + corners.Count - 1 + least) % corners.Count] switch {
            var corner => (corner.Rgb, corner.Jmh with { Z = corner.Jmh.Z + (Hues * (((k + corners.Count - 1) / corners.Count) - 1)) }),
        })];
    }

    private static Seq<float> Nominal(Seq<float> sorted) =>
        sorted.Fold((Counts: Seq<int>(), Last: -1, Floor: sorted.Head.Map(static hue => hue is 0f ? 0 : 1).IfNone(1)), static (state, hue) =>
            int.Min(int.Max((int)float.Round(hue, MidpointRounding.AwayFromZero), state.Floor), Hues - 1) switch {
                var index when index != state.Last => (state.Counts.Add(index), index, index),
                var index when state.Counts is [.., var before, var last] && before != last - 1 => (state.Counts.Init.Add(last - 1).Add(index), index, index),
                var index => (state.Counts.Add(int.Min(index + 1, Hues - 1)), index + 1, index + 1),
            }).Counts switch {
                var counts =>
                    from span in (Seq(0f) + sorted).Zip(sorted.Add(Hues)).Zip((Seq(0) + counts).Zip(counts.Add(Hues)))
                    from i in toSeq(Range(0, span.Second.Second - span.Second.First))
                    select span.First.First + (i * ((span.First.Second - span.First.First) / (span.Second.Second - span.Second.First))),
            };

    private static ImmutableArray<T> Wrapped<T>(Seq<T> nominal, Func<T, int, T> turned) =>
        [.. Range(0, Hues + 3).Select(k => turned(nominal[(k + Hues - 1) % Hues], ((k + Hues - 1) / Hues) - 1))];

    private sealed record Rendering(
        (Matrix4x4 ToCone, Matrix4x4 FromCone) Reach, (Matrix4x4 ToCone, Matrix4x4 FromCone) Limit, Matrix4x4 AabToCone,
        float Ceiling, float ForwardLimit, float InverseLimit, float S2, float M2, float LimitJ, float MidJ,
        float Saturation, float SaturationThreshold, float Compression, float ChromaScale, float FocusDistance, float LowerHullGammaInverse) {
        public ImmutableArray<float> ReachLimits { get; init; } = [];
        public ImmutableArray<float> HueTable { get; init; } = [];
        public ImmutableArray<Vector3> Cusps { get; init; } = [];
    }
}
