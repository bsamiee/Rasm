using System.Drawing;
using System.Numerics;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Rasm.Imaging.Color;
using Rasm.Imaging.Pixels;
using Wacton.Unicolour;

namespace Rasm.Imaging.Tone;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum]
public sealed partial class LightingKey {
    public static readonly LightingKey HighKey = new(Linear(0.45f), 2f, float.PositiveInfinity);
    public static readonly LightingKey MidKey = new(Linear(0.22f), 3f, 0.42f);
    public static readonly LightingKey LowKey = new(0f, float.PositiveInfinity, float.PositiveInfinity);

    private readonly float minimumMean;
    private readonly float maximumRatio;
    private readonly float maximumSplit;

    public bool Holds(LightingReading reading) => reading.MeanLuminance >= minimumMean && reading.Ratio <= maximumRatio && reading.Split <= maximumSplit;

    private static float Linear(float encoded) {
        Vector4 level = new(new Vector3(encoded), 1f);
        Transfer.Gamma.Srgb.Decode.Apply(new Span<Vector4>(ref level));
        return level.X;
    }
}

public sealed record PaletteEntry(Vector3 Color, Vector2 Position, float Share) {
    private const double MergeDistance = 0.08d;

    public float Chroma => float.Max(Color.X, float.Max(Color.Y, Color.Z)) switch {
        > 0f and var peak => (peak - float.Min(Color.X, float.Min(Color.Y, Color.Z))) / peak,
        _ => 0f,
    };

    public static Fin<(Seq<PaletteEntry> Entries, double Spread)> Extracted(PixelFrame frame, Gamut gamut, int size, Option<Rectangle> region = default) {
        Rectangle window = Rectangle.Intersect(region.IfNone(frame.Window), frame.Window);
        (Vector3 Color, Vector2 Place)[] kept = GC.AllocateUninitializedArray<(Vector3 Color, Vector2 Place)>(window.Width * window.Height);
        (Vector2 extent, int count) = (new Vector2(frame.Extent.Width, frame.Extent.Height), 0);
        for (int y = window.Top; y < window.Bottom; y++) {
            ReadOnlySpan<Vector4> row = frame.View.Span.GetRowSpan(y - frame.Origin.Y).Slice(window.Left - frame.Origin.X, window.Width);
            for (int x = 0; x < row.Length; x++) {
                kept[count] = (row[x].AsVector3(), (new Vector2(window.Left + x, y) + new Vector2(0.5f)) / extent);
                count += row[x].W > 0f && Vector4.AllWhereAllBitsSet(Vector4.IsFinite(row[x])) ? 1 : 0;
            }
        }
        return (ImagingFault.Unless(size > 0, ImagingReason.Range, size), ImagingFault.Unless(count > 0, ImagingReason.Empty, window))
            .Apply((_, _) => Clustered(kept[..count], int.Min(size, count), gamut)).As();
    }

    private static (Seq<PaletteEntry> Entries, double Spread) Clustered((Vector3 Color, Vector2 Place)[] samples, int clusters, Gamut gamut) {
        using Mat data = new(samples.Length, 3, DepthType.Cv32F, 1);
        using Mat labels = new(samples.Length, 1, DepthType.Cv32S, 1);
        using Mat centers = new();
        data.SetTo([.. samples.Select(static sample => sample.Color)]);
        labels.SetTo([.. Enumerable.Range(0, samples.Length).Select(index => (int)((long)index * clusters / samples.Length))]);
        double compactness = CvInvoke.Kmeans(data, clusters, labels, new MCvTermCriteria(100, 1e-6), 1, KMeansInitType.UseInitialLabels, centers);
        Vector3[] means = centers.GetSpan<Vector3>().ToArray();
        return (labels.GetSpan<int>().ToArray().Zip(samples, static (label, sample) => (Label: label, sample.Place))
            .GroupBy(static sample => sample.Label, static sample => sample.Place)
            .OrderByDescending(static group => group.Count()).ThenBy(static group => group.Key)
            .Aggregate(Seq<(PaletteEntry Entry, Unicolour Perceived)>(), (kept, group) => {
                PaletteEntry entry = new(means[group.Key], group.Aggregate(Vector2.Zero, static (sum, place) => sum + place) / group.Count(), (float)group.Count() / samples.Length);
                Vector3 level = entry.Color / float.Max(Vector3.Dot(gamut.Luminance, entry.Color), LuminanceSamples.Floor);
                Unicolour perceived = new Unicolour(gamut.Configuration, ColourSpace.RgbLinear, level.X, level.Y, level.Z).ConvertToConfiguration(Configuration.Default);
                return kept.Map((held, index) => (Distance: held.Perceived.Difference(perceived, DeltaE.Ok), Index: index))
                    .Filter(static near => near.Distance < MergeDistance).AsIterableNE().Map(static near => Enumerable.Min(near))
                    .Match(
                        Some: nearest => kept.Map((held, index) => index == nearest.Index ? (held.Entry with { Share = held.Entry.Share + entry.Share }, held.Perceived) : held),
                        None: () => kept.Add((entry, perceived)));
            }).Map(static held => held.Entry), compactness / samples.Length);
    }
}

public sealed record LightingReading(
    Gamut Gamut, Vector3 KeyColor, Vector3 FillColor, Vector3 AmbientColor,
    float Ratio, float Range, float MeteredExposure, float MeanLuminance,
    Vector2 Direction, float Hardness, float Split, bool Backlit,
    Option<Wacton.Unicolour.Temperature> KeyWhite, Seq<PaletteEntry> Palette) {
    private static readonly (float Subject, float Center, float Rim) Insets = (0.12f, 0.25f, 0.18f);

    public LightingKey Key => LightingKey.Items.First(key => key.Holds(this));

    public Option<(PaletteEntry Key, PaletteEntry Accent)> Gels =>
        Palette.Take(2).Filter(static entry => entry.Chroma >= 0.18f) switch {
            [var first, var second] => Some(Vector2.Dot(new Vector2(first.Position.X - second.Position.X, second.Position.Y - first.Position.Y), Direction) >= 0f ? (first, second) : (second, first)),
            _ => None,
        };

    public static Fin<LightingReading> Read(PixelFrame frame, Gamut gamut, PixelExtent analysis, int paletteSize) {
        PixelFrame sample = frame.Downscaled(analysis);
        static Metering Inset(float share) => new(0f, 1f, 0f, new Vector2(share), new Vector2(1f - share));
        Rectangle subject = Inset(Insets.Subject).Window(sample);
        return (LuminanceSamples.Of(LuminanceSamples.Plane(sample, subject, gamut.Luminance), subject), Inset(Insets.Subject).Metered(sample, gamut), PaletteEntry.Extracted(sample, gamut, paletteSize, subject))
            .Apply((ranks, stops, palette) => Measured(sample, gamut, ranks, stops, palette.Entries, subject, Inset(Insets.Center).Window(sample), Inset(Insets.Rim).Window(sample))).As();
    }

    private static LightingReading Measured(PixelFrame sample, Gamut gamut, LuminanceSamples ranks, float stops, Seq<PaletteEntry> palette, Rectangle subject, Rectangle center, Rectangle rim) {
        (int width, int height) = (sample.Size.Width, sample.Size.Height);
        using Mat header = sample.Header();
        using Mat weights = new(1, 4, DepthType.Cv32F, 1);
        using Mat plane = new();
        using Mat valid = new();
        using Mat within = Mat.Zeros(height, width, DepthType.Cv8U, 1);
        using Mat gx = new();
        using Mat gy = new();
        using Mat magnitude = new();
        using Mat angle = new();
        using Mat product = new();
        using ScalarArray floor = new(LuminanceSamples.Floor);
        using ScalarArray lowest = new(new MCvScalar(-float.MaxValue, -float.MaxValue, -float.MaxValue, float.Epsilon));
        using ScalarArray highest = new(new MCvScalar(float.MaxValue, float.MaxValue, float.MaxValue, float.MaxValue));
        weights.SetTo([gamut.Luminance.X, gamut.Luminance.Y, gamut.Luminance.Z, 0f]);
        CvInvoke.Transform(header, plane, weights);
        CvInvoke.Max(plane, floor, plane);
        CvInvoke.InRange(header, lowest, highest, valid);
        CvInvoke.Rectangle(within, subject, new MCvScalar(byte.MaxValue), thickness: -1);
        CvInvoke.BitwiseAnd(within, valid, within);
        CvInvoke.Sobel(plane, gx, DepthType.Cv32F, 1, 0, 1, 0.5d, 0d, BorderType.Replicate);
        CvInvoke.Sobel(plane, gy, DepthType.Cv32F, 0, 1, 1, -0.5d, 0d, BorderType.Replicate);
        CvInvoke.CartToPolar(gx, gy, magnitude, angle);
        CvInvoke.Multiply(magnitude, magnitude, product);
        double energy = CvInvoke.Mean(product, within).V0;
        float Level(float rank) => float.Max((float)ranks.At(rank), LuminanceSamples.Floor);
        (double Sum, int Count) Total(Rectangle region) {
            using Mat area = new(plane, region);
            using Mat mask = new(valid, region);
            int count = CvInvoke.CountNonZero(mask);
            return (CvInvoke.Mean(area, mask).V0 * count, count);
        }
        static Option<double> Mean((double Sum, int Count) total) => total.Count > 0 ? total.Sum / total.Count : None;
        (Vector3 Color, Vector2 Center) Band(float low, float high, bool weighted = false) {
            using Mat band = new();
            using Mat lifted = Mat.Zeros(height, width, DepthType.Cv32F, 1);
            using ScalarArray bottom = new(Level(low));
            using ScalarArray top = new(Level(high));
            CvInvoke.InRange(plane, bottom, top, band);
            CvInvoke.BitwiseAnd(band, within, band);
            CvInvoke.Subtract(plane, bottom, lifted, band);
            (MCvScalar mean, Moments moments) = (CvInvoke.Mean(header, band), weighted ? CvInvoke.Moments(lifted) : CvInvoke.Moments(band, binaryImage: true));
            return (new((float)mean.V0, (float)mean.V1, (float)mean.V2), moments.M00 > 0d ? Position((moments.M10 / moments.M00) + 0.5d, (moments.M01 / moments.M00) + 0.5d, width, height) : Vector2.Zero);
        }
        float Asymmetry(Rectangle positive, Rectangle negative) =>
            (from above in Mean(Total(Rectangle.Intersect(positive, subject))) from below in Mean(Total(Rectangle.Intersect(negative, subject))) select (float)((above - below) / (above + below))).IfNone(0f);
        double Moment(Mat component) {
            CvInvoke.Multiply(magnitude, component, product);
            return energy > 0d ? CvInvoke.Mean(product, within).V0 / energy : 0d;
        }
        double Extreme(Mat component) {
            (double low, double high, Point least, Point most) = (0d, 0d, Point.Empty, Point.Empty);
            CvInvoke.MinMaxLoc(component, ref low, ref high, ref least, ref most, within);
            return double.Max(-low, high);
        }
        Vector2 asymmetry = Saturated(new(
            Asymmetry(Rectangle.FromLTRB((width + 1) / 2, 0, width, height), Rectangle.FromLTRB(0, 0, width / 2, height)),
            Asymmetry(Rectangle.FromLTRB(0, 0, width, height / 2), Rectangle.FromLTRB(0, (height + 1) / 2, width, height))));
        (Vector3 key, Vector3 fill) = (Band(0.85f, 1f).Color, Band(0.35f, 0.6f).Color);
        Vector2 direction = (0.42f * asymmetry) + (0.28f * Saturated(1.25f * (Band(0.78f, 1f).Center - Band(0f, 0.22f).Center)))
            + (0.18f * Saturated(Band(0.55f, 1f, weighted: true).Center)) + (0.12f * new Vector2((float)Moment(gx), (float)Moment(gy)));
        ((double Sum, int Count) all, (double Sum, int Count) core, (double Sum, int Count) inner, (double rimSum, int rimCount)) = (Total(sample.Window), Total(subject), Total(center), Total(rim));
        (double overall, double kernel, double steepest) = (Mean(all).IfNone(0d), Mean(core).IfNone(0d), double.Max(Extreme(gx), Extreme(gy)));
        bool backlit = (from inside in Mean(inner) from border in Mean((all.Sum - inner.Sum, all.Count - inner.Count)) select border > inside * 1.35d && inside <= overall * 1.08d).Exists(static holds => holds)
            || Mean((core.Sum - rimSum, core.Count - rimCount)).Exists(edge => edge > kernel * 1.25d && kernel < overall * 1.05d);
        float Luma(Vector3 color) => float.Max(Vector3.Dot(gamut.Luminance, color), LuminanceSamples.Floor);
        (float range, double scale) = (float.Log2(Level(0.95f)) - float.Log2(Level(0.05f)), steepest + (Level(0.9f) - Level(0.1f)));
        return new LightingReading(
            gamut, key, fill, Band(0f, 0.15f).Color, float.Log2(Luma(key)) - float.Log2(Luma(fill)), range, stops, (float)overall,
            direction, scale > 0d ? (float)(steepest / scale) : 0f, asymmetry.Length() * float.Min(1f, range / float.Log2(6f)), backlit,
            gamut.Temperature(key).ToOption(), palette);
    }

    private static Vector2 Position(double x, double y, int width, int height) => new((float)((2d * x / width) - 1d), (float)(1d - (2d * y / height)));

    private static Vector2 Saturated(Vector2 cue) => cue * float.Min(1f, 1f / cue.Length());
}
