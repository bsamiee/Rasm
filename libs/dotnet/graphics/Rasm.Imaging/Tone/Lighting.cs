using System.Globalization;
using System.Numerics;
using CommunityToolkit.HighPerformance;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Rasm.Imaging.Color;
using Rasm.Imaging.Pixels;
using Wacton.Unicolour;

namespace Rasm.Imaging.Tone;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum]
public sealed partial class LightingMood {
    public static readonly LightingMood HighKey = new(static reading => reading.MeanLuminance > HighKeyMean && reading.Ratio < float.Log2(4f));
    public static readonly LightingMood LowKey = new(static reading =>
        !HighKey.Holds(reading) && (reading.MeanLuminance < LowKeyMean || reading.Ratio > float.Log2(8f) || reading.Split > 0.42f));
    public static readonly LightingMood Neutral = new(static reading => !HighKey.Holds(reading) && !LowKey.Holds(reading));

    private static readonly float HighKeyMean = Linear(0.45f);
    private static readonly float LowKeyMean = Linear(0.22f);

    [UseDelegateFromConstructor]
    public partial bool Holds(LightingReading reading);

    private static float Linear(float encodedMean) {
        Vector4 mean = new(new Vector3(encodedMean), 1f);
        new Transfer.Srgb().Decode(new Span<Vector4>(ref mean));
        return mean.X;
    }
}

public sealed record PaletteEntry(Vector3 Color, FramePosition Position, float Share) {
    public double Chroma =>
        float.Max(Color.X, float.Max(Color.Y, Color.Z)) switch {
            > 0f and var peak => ((double)peak - float.Min(Color.X, float.Min(Color.Y, Color.Z))) / peak,
            _ => 0f,
        };
}

public sealed record LightingReading(
    Gamut Gamut, Vector3 KeyColor, Vector3 FillColor, Vector3 AmbientColor,
    float Ratio, float Range, float MeteredExposure, double MeanLuminance,
    Vector2 Direction, float Hardness, float Split, bool Backlit,
    Option<Wacton.Unicolour.Temperature> KeyWhite, Seq<PaletteEntry> Palette) {
    public LightingMood Mood => LightingMood.Items.First(mood => mood.Holds(this));

    public Option<(PaletteEntry Key, PaletteEntry Accent)> Gels =>
        Palette.Take(2).Filter(static entry => entry.Chroma >= 0.18f) switch {
            [var first, var second] => Some(Vector2.Dot(new Vector2(first.Position.X - second.Position.X, second.Position.Y - first.Position.Y), Direction) >= 0f ? (first, second) : (second, first)),
            _ => None,
        };
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Lighting {
    public static Fin<LightingReading> Read(PixelFrame frame, Gamut gamut, int paletteSize) {
        PixelFrame analysis = frame.Downscaled(PixelExtent.Analysis);
        (int width, int height) = (analysis.Size.Width, analysis.Size.Height);
        (double R, double G, double B) luminance = gamut.Luminance;
        double Luma(double red, double green, double blue) => double.Max(Gamut.RelativeLuminance(red, green, blue, luminance), LuminanceSamples.Floor);
        (int Left, int Top, int Right, int Bottom) Inset(double fraction) =>
            ((int)double.Floor(width * fraction), (int)double.Floor(height * fraction)) switch {
                var (x, y) => (x, y, width - x, height - y),
            };
        bool Inside((double Key, int Y, int X) sample, (int Left, int Top, int Right, int Bottom) box) =>
            sample.X >= box.Left && sample.X < box.Right && sample.Y >= box.Top && sample.Y < box.Bottom;
        (int Left, int Top, int Right, int Bottom) subject = Inset(0.12);
        (long Left, long Top, long Right, long Bottom) window = (
            (long)analysis.Origin.X + subject.Left, (long)analysis.Origin.Y + subject.Top,
            (long)analysis.Origin.X + subject.Right, (long)analysis.Origin.Y + subject.Bottom);
        return paletteSize <= 0 ? new ImagingFault(string.Create(CultureInfo.InvariantCulture, $"palette size {paletteSize}"), ImagingReason.Range)
            : from all in LuminanceSamples.Sorted(analysis, analysis.Window, rgb => Luma(rgb.X, rgb.Y, rgb.Z),
                (key, pixel) => (Key: key, Y: (int)(pixel.Y - analysis.Origin.Y), X: (int)(pixel.X - analysis.Origin.X)))
              let samples = all.Where(sample => Inside(sample, subject)).ToArray()
              from stops in Metering.Default.Metered([.. samples.Select(static sample => (sample.Key, Weight: 1f))], window)
              select Measured(samples, all, stops);

        LightingReading Measured((double Key, int Y, int X)[] samples, (double Key, int Y, int X)[] all, float stops) {
            Memory2D<Vector4> view = analysis.View;
            Vector3 Color((double Key, int Y, int X) sample) => view.Span[sample.Y, sample.X].AsVector3();
            Vector2 Point((double Key, int Y, int X) sample) => Position(sample.X, sample.Y, width, height);
            (double R, double G, double B) Mean(IEnumerable<(double Key, int Y, int X)> band) =>
                band.Aggregate((R: 0d, G: 0d, B: 0d, Count: 0), (sum, sample) => Color(sample) switch {
                    var color => (sum.R + color.X, sum.G + color.Y, sum.B + color.Z, sum.Count + 1),
                }) switch {
                    var (r, g, b, count) => (r / count, g / count, b / count),
                };
            Vector3 Rgb((double R, double G, double B) color) => new((float)color.R, (float)color.G, (float)color.B);
            ArraySegment<(double Key, int Y, int X)> Band(float low, float high) => LuminanceSamples.Slice(samples, low, high);
            Vector2 Center(IEnumerable<(double Key, int Y, int X)> band) =>
                band.Aggregate((X: 0d, Y: 0d, Count: 0), (sum, sample) => Point(sample) switch {
                    var point => (sum.X + point.X, sum.Y + point.Y, sum.Count + 1),
                }) switch {
                    var (x, y, count) => new((float)(x / count), (float)(y / count)),
                };
            LuminanceSamples ranks = new([.. samples.Select(static sample => sample.Key)]);
            double At(float rank) => ranks.At(rank);
            float Asymmetry(Func<Vector2, float> axis) =>
                samples.GroupBy(sample => float.Sign(axis(Point(sample))))
                    .ToDictionary(static half => half.Key, static half => half.Average(static sample => sample.Key)) switch {
                        var halves when halves.TryGetValue(1, out double positive) && halves.TryGetValue(-1, out double negative) => (float)((positive - negative) / (positive + negative)),
                        _ => 0f,
                    };
            Vector2 asymmetry = Saturated(new(Asymmetry(static point => point.X), Asymmetry(static point => point.Y)));
            ArraySegment<(double Key, int Y, int X)> highlights = Band(0.55f, 1f);
            (double momentX, double momentY, double momentWeight) = highlights.Aggregate((X: 0d, Y: 0d, Weight: 0d), (sum, sample) => (Point(sample), Weight: sample.Key - highlights[0].Key) switch {
                var (point, weight) => ((weight * point.X) + sum.X, (weight * point.Y) + sum.Y, weight + sum.Weight),
            });
            Dictionary<(int Y, int X), double> plane = samples.ToDictionary(static sample => (sample.Y, sample.X), static sample => sample.Key);
            (int Y, int X) Near((double Key, int Y, int X) sample, int down, int across) =>
                plane.ContainsKey((sample.Y + down, sample.X + across)) ? (sample.Y + down, sample.X + across) : (sample.Y, sample.X);
            (double X, double Y) Slope((double Key, int Y, int X) sample) =>
                (Near(sample, 0, -1), Near(sample, 0, 1), Near(sample, -1, 0), Near(sample, 1, 0)) switch {
                    var (left, right, top, bottom) => (
                        right.X == left.X ? 0d : (plane[right] - plane[left]) / (right.X - left.X),
                        bottom.Y == top.Y ? 0d : (plane[top] - plane[bottom]) / (bottom.Y - top.Y)),
                };
            (double gradientX, double gradientY, double gradientWeight, double steepest) = samples.Aggregate((X: 0d, Y: 0d, Weight: 0d, Steepest: 0d), (sum, sample) => Slope(sample) switch {
                var (x, y) => double.Hypot(x, y) switch {
                    var magnitude => (sum.X + (magnitude * x), sum.Y + (magnitude * y), sum.Weight + (magnitude * magnitude), double.Max(sum.Steepest, double.Max(double.Abs(x), double.Abs(y)))),
                },
            });
            Vector2 direction = (0.42f * asymmetry)
                + (0.28f * Saturated(1.25f * (Center(Band(0.78f, 1f)) - Center(Band(0f, 0.22f)))))
                + (0.18f * (momentWeight > 0d ? Saturated(new((float)(momentX / momentWeight), (float)(momentY / momentWeight))) : Vector2.Zero))
                + (0.12f * (gradientWeight > 0d ? new Vector2((float)(gradientX / gradientWeight), (float)(gradientY / gradientWeight)) : Vector2.Zero));
            (int Left, int Top, int Right, int Bottom) centerBox = Inset(0.25);
            (int Left, int Top, int Right, int Bottom) rimBox = Inset(0.18);
            double overall = all.Average(static sample => sample.Key);
            double core = samples.Average(static sample => sample.Key);
            Option<double> Average(IEnumerable<(double Key, int Y, int X)> values) => values.AsIterableNE().Map(static band => Enumerable.Average(band, static sample => sample.Key));
            bool backlit = (from center in Average(all.Where(sample => Inside(sample, centerBox)))
                            from border in Average(all.Where(sample => !Inside(sample, centerBox)))
                            select border > center * 1.35 && center <= overall * 1.08).Exists(static holds => holds)
                || Average(samples.Where(sample => !Inside(sample, rimBox))).Exists(rim => rim > (core * 1.25) && core < (overall * 1.05));
            int count = int.Min(paletteSize, samples.Length);
            using Mat data = new(samples.Length, 3, DepthType.Cv32F, 1);
            using Mat labels = new(samples.Length, 1, DepthType.Cv32S, 1);
            using Mat centers = new();
            data.SetTo([.. samples.Select(Color)]);
            labels.SetTo([.. Enumerable.Range(0, samples.Length).Select(index => (int)((long)index * count / samples.Length))]);
            _ = CvInvoke.Kmeans(data, count, labels, new MCvTermCriteria(100, 1e-6), 1, KMeansInitType.UseInitialLabels, centers);
            Vector3[] colors = centers.GetSpan<Vector3>().ToArray();
            List<(PaletteEntry Entry, Unicolour Color)> palette = [];
            foreach (IGrouping<int, (double Key, int Y, int X)> group in labels.GetSpan<int>().ToArray().Zip(samples)
                .GroupBy(static pair => pair.First, static pair => pair.Second)
                .OrderByDescending(static group => group.Count()).ThenBy(static group => group.Key)) {
                Vector3 color = colors[group.Key];
                Vector2 point = Center(group);
                PaletteEntry entry = new(color, new((point.X + 1f) / 2f, (1f - point.Y) / 2f), (float)group.Count() / samples.Length);
                double level = Luma(color.X, color.Y, color.Z);
                Unicolour perceptual = new Unicolour(gamut.Configuration, ColourSpace.RgbLinear, color.X / level, color.Y / level, color.Z / level).ConvertToConfiguration(Configuration.Default);
                Option<(double Distance, int Index)> nearest = palette.Select((kept, index) => (Distance: perceptual.Difference(kept.Color, DeltaE.Ok), Index: index)).AsIterableNE().Map(static distances => Enumerable.Min(distances));
                if (nearest.Case is ValueTuple<double, int> { Item1: < 0.08, Item2: var index })
                    palette[index] = (palette[index].Entry with { Share = palette[index].Entry.Share + entry.Share }, palette[index].Color);
                else
                    palette.Add((entry, perceptual));
            }
            (double R, double G, double B) key = Mean(Band(0.85f, 1f));
            (double R, double G, double B) fill = Mean(Band(0.35f, 0.6f));
            float range = (float)(double.Log2(At(0.95f)) - double.Log2(At(0.05f)));
            double hardnessScale = steepest + (At(0.9f) - At(0.1f));
            return new LightingReading(
                gamut, Rgb(key), Rgb(fill), Rgb(Mean(Band(0f, 0.15f))),
                (float)(double.Log2(Luma(key.R, key.G, key.B)) - double.Log2(Luma(fill.R, fill.G, fill.B))), range, stops, overall,
                direction, hardnessScale > 0d ? (float)(steepest / hardnessScale) : 0f,
                asymmetry.Length() * float.Min(1f, range / float.Log2(6f)), backlit,
                Adaptation.Correlate(gamut, Rgb(key)).ToOption(), toSeq(palette.Select(static kept => kept.Entry)));
        }
    }

    private static Vector2 Position(int column, int row, int width, int height) =>
        new((((2f * column) + 1f) / width) - 1f, 1f - (((2f * row) + 1f) / height));

    private static Vector2 Saturated(Vector2 cue) => cue * float.Min(1f, 1f / cue.Length());
}
