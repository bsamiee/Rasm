using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;
using Rasm.Imaging.Color;
using Rasm.Imaging.Pixels;
using Wacton.Unicolour;

namespace Rasm.Imaging.Tone;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum]
public sealed partial class LightingMood {
    public static readonly LightingMood HighKey = new();
    public static readonly LightingMood Neutral = new();
    public static readonly LightingMood LowKey = new();
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
    private static readonly (float HighKeyMean, float HighKeyRatio, float LowKeyMean, float LowKeyRatio, float LowKeySplit) Thresholds =
        (Linear(0.45f), float.Log2(4f), Linear(0.22f), float.Log2(8f), 0.42f);

    public LightingMood Mood =>
        MeanLuminance > Thresholds.HighKeyMean && Ratio < Thresholds.HighKeyRatio ? LightingMood.HighKey
        : MeanLuminance < Thresholds.LowKeyMean || Ratio > Thresholds.LowKeyRatio || Split > Thresholds.LowKeySplit ? LightingMood.LowKey
        : LightingMood.Neutral;

    public Option<(PaletteEntry Key, PaletteEntry Accent)> Gels =>
        Palette.Take(2).Filter(static entry => entry.Chroma >= 0.18f) switch {
            [var first, var second] => Some(Vector2.Dot(new Vector2(first.Position.X - second.Position.X, second.Position.Y - first.Position.Y), Direction) >= 0f ? (first, second) : (second, first)),
            _ => None,
        };

    private static float Linear(float encodedMean) {
        Vector4 mean = new(new Vector3(encodedMean), 1f);
        new Transfer.Srgb().Decode(new Span<Vector4>(ref mean));
        return mean.X;
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Lighting {
    public static Fin<LightingReading> Read(PixelFrame frame, Gamut gamut, int paletteSize) {
        PixelFrame analysis = frame.Downscaled(PixelExtent.Analysis);
        (int width, int height) = (analysis.Size.Width, analysis.Size.Height);
        (double R, double G, double B) luminance = gamut.Luminance;
        double Luma(double red, double green, double blue) => double.Max(LuminanceSamples.Luminance(red, green, blue, luminance), LuminanceSamples.Floor);
        (int Left, int Top, int Right, int Bottom) Inset(double fraction) =>
            ((int)double.Floor(width * fraction), (int)double.Floor(height * fraction)) switch {
                var (x, y) => (x, y, width - x, height - y),
            };
        bool Inside(int index, (int Left, int Top, int Right, int Bottom) box) =>
            int.DivRem(index, width) switch {
                var (y, x) => x >= box.Left && x < box.Right && y >= box.Top && y < box.Bottom,
            };
        (int Left, int Top, int Right, int Bottom) subject = Inset(0.12);
        (long Left, long Top, long Right, long Bottom) window = (
            (long)analysis.Origin.X + subject.Left, (long)analysis.Origin.Y + subject.Top,
            (long)analysis.Origin.X + subject.Right, (long)analysis.Origin.Y + subject.Bottom);
        return paletteSize <= 0 ? Error.New(ErrorCodes.ValidationFailed, string.Create(CultureInfo.InvariantCulture, $"palette size {paletteSize} must be positive"))
            : from all in LuminanceSamples.Sorted(analysis, analysis.Window, rgb => Luma(rgb.X, rgb.Y, rgb.Z),
                (key, pixel) => (Key: key, Index: ((int)(pixel.Y - analysis.Origin.Y) * width) + (int)(pixel.X - analysis.Origin.X)))
              let samples = all.Where(sample => Inside(sample.Index, subject)).ToArray()
              from stops in Metering.Default.Metered([.. samples.Select(static sample => (sample.Key, Weight: 1f))], window)
              select Measured(samples, all, stops);

        LightingReading Measured((double Key, int Index)[] samples, (double Key, int Index)[] all, float stops) {
            Vector3 Color(int index) => MemoryMarshal.Cast<float, Vector4>(analysis.Block)[index].AsVector3();
            Vector2 Point(int index) => int.DivRem(index, width) switch {
                var (row, column) => Position(column, row, width, height),
            };
            (double R, double G, double B) Mean(IEnumerable<int> indices) =>
                indices.Aggregate((R: 0d, G: 0d, B: 0d, Count: 0), (sum, index) => Color(index) switch {
                    var color => (sum.R + color.X, sum.G + color.Y, sum.B + color.Z, sum.Count + 1),
                }) switch {
                    var (r, g, b, count) => (r / count, g / count, b / count),
                };
            Vector3 Rgb((double R, double G, double B) color) => new((float)color.R, (float)color.G, (float)color.B);
            IEnumerable<(double Key, int Index)> Band(float low, float high) =>
                new ArraySegment<(double Key, int Index)>(samples)[LuminanceSamples.Slice(samples.Length, low, high)];
            Vector2 Center(IEnumerable<(double Key, int Index)> band) =>
                band.Aggregate((X: 0d, Y: 0d, Count: 0), (sum, sample) => Point(sample.Index) switch {
                    var point => (sum.X + point.X, sum.Y + point.Y, sum.Count + 1),
                }) switch {
                    var (x, y, count) => new((float)(x / count), (float)(y / count)),
                };
            LuminanceSamples ranks = new([.. samples.Select(static sample => sample.Key)]);
            double At(float rank) => ranks.At(rank);
            float Asymmetry(Func<Vector2, float> axis) =>
                samples.GroupBy(sample => float.Sign(axis(Point(sample.Index))))
                    .ToDictionary(static half => half.Key, static half => half.Average(static sample => sample.Key)) switch {
                        var halves when halves.TryGetValue(1, out double positive) && halves.TryGetValue(-1, out double negative) => (float)((positive - negative) / (positive + negative)),
                        _ => 0f,
                    };
            Vector2 asymmetry = Saturated(new(Asymmetry(static point => point.X), Asymmetry(static point => point.Y)));
            (double Key, int Index)[] highlights = [.. Band(0.55f, 1f)];
            (double momentX, double momentY, double momentWeight) = highlights.Aggregate((X: 0d, Y: 0d, Weight: 0d), (sum, sample) => (Point(sample.Index), Weight: sample.Key - highlights[0].Key) switch {
                var (point, weight) => ((weight * point.X) + sum.X, (weight * point.Y) + sum.Y, weight + sum.Weight),
            });
            Dictionary<int, double> plane = samples.ToDictionary(static sample => sample.Index, static sample => sample.Key);
            (double X, double Y) Slope(int index) {
                (int row, int column) = int.DivRem(index, width);
                (int left, int right, int top, int bottom) = (
                    column > 0 && plane.ContainsKey(index - 1) ? index - 1 : index,
                    column + 1 < width && plane.ContainsKey(index + 1) ? index + 1 : index,
                    row > 0 && plane.ContainsKey(index - width) ? index - width : index,
                    row + 1 < height && plane.ContainsKey(index + width) ? index + width : index);
                return (right == left ? 0d : (plane[right] - plane[left]) / (right - left),
                    bottom == top ? 0d : (plane[top] - plane[bottom]) / ((bottom - top) / width));
            }
            (double gradientX, double gradientY, double gradientWeight, double steepest) = samples.Aggregate((X: 0d, Y: 0d, Weight: 0d, Steepest: 0d), (sum, sample) => Slope(sample.Index) switch {
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
            Option<double> Average(IEnumerable<(double Key, int Index)> values) => values.AsIterableNE().Map(static band => Enumerable.Average(band, static sample => sample.Key));
            bool backlit = (from center in Average(all.Where(sample => Inside(sample.Index, centerBox)))
                            from border in Average(all.Where(sample => !Inside(sample.Index, centerBox)))
                            select border > center * 1.35 && center <= overall * 1.08).Exists(static holds => holds)
                || Average(samples.Where(sample => !Inside(sample.Index, rimBox))).Exists(rim => rim > (core * 1.25) && core < (overall * 1.05));
            int count = int.Min(paletteSize, samples.Length);
            (double R, double G, double B)[] centers = [.. Enumerable.Range(0, count).Select(index =>
                Mean(new ArraySegment<(double Key, int Index)>(samples)[
                    (int)((long)index * samples.Length / count)..(int)((long)(index + 1) * samples.Length / count)]
                    .Select(static sample => sample.Index)))];
            int[] Assigned() => [.. samples.Select(sample => Color(sample.Index) switch {
                var color => centers.Select((center, index) => (
                    Distance: double.Pow(center.R - color.X, 2d) + double.Pow(center.G - color.Y, 2d) + double.Pow(center.B - color.Z, 2d), Index: index)).Min().Index,
            })];
            int[] labels = Assigned();
            IGrouping<int, int>[] groups = [.. Enumerable.Range(0, samples.Length).GroupBy(index => labels[index])];
            while (true) {
                foreach (IGrouping<int, int> group in groups)
                    centers[group.Key] = Mean(group.Select(index => samples[index].Index));
                int[] next = Assigned();
                if (labels.SequenceEqual(next))
                    break;
                labels = next;
                groups = [.. Enumerable.Range(0, samples.Length).GroupBy(index => labels[index])];
            }
            List<(PaletteEntry Entry, Unicolour Color)> palette = [];
            foreach (IGrouping<int, int> group in groups.OrderByDescending(static group => group.Count()).ThenBy(static group => group.Key)) {
                Vector3 color = Rgb(centers[group.Key]);
                Vector2 point = Center(group.Select(index => samples[index]));
                PaletteEntry entry = new(color, new(FrameAxis.Create((point.X + 1f) / 2f), FrameAxis.Create((1f - point.Y) / 2f)), (float)group.Count() / samples.Length);
                double level = Luma(color.X, color.Y, color.Z);
                Unicolour perceptual = new Unicolour(gamut.Configuration, ColourSpace.RgbLinear, color.X / level, color.Y / level, color.Z / level).ConvertToConfiguration(Configuration.Default);
                Option<(double Distance, int Index)> nearest = palette.Select((kept, index) => (Distance: perceptual.Difference(kept.Color, DeltaE.Ok), Index: index)).AsIterableNE().Map(static distances => Enumerable.Min(distances));
                if (nearest.Case is ValueTuple<double, int> { Item1: < 0.08, Item2: var index })
                    palette[index] = (palette[index].Entry with { Share = palette[index].Entry.Share + entry.Share }, palette[index].Color);
                else
                    palette.Add((entry, perceptual));
            }
            (double R, double G, double B) key = Mean(Band(0.85f, 1f).Select(static sample => sample.Index));
            (double R, double G, double B) fill = Mean(Band(0.35f, 0.6f).Select(static sample => sample.Index));
            float range = (float)(double.Log2(At(0.95f)) - double.Log2(At(0.05f)));
            double hardnessScale = steepest + (At(0.9f) - At(0.1f));
            return new LightingReading(
                gamut, Rgb(key), Rgb(fill), Rgb(Mean(Band(0f, 0.15f).Select(static sample => sample.Index))),
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
