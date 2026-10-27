using System.Drawing;
using System.Globalization;
using System.Numerics;
using System.Runtime.Intrinsics;
using CommunityToolkit.HighPerformance.Buffers;
using Emgu.CV;
using Rasm.Imaging.Color;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Pixels;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Quantization;

namespace Rasm.Imaging.Filtering;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class SortMetric {
    public static readonly SortMetric Luma = new("luma", static (color, weights) => Vector256.Dot(weights, color.Wide));
    public static readonly SortMetric Hue = new("hue", static (color, _) => RangeAxis.Hue.Coordinate(Hsy.HueOf(color)));
    public static readonly SortMetric Saturation = new("saturation", static (color, _) => RangeAxis.Saturation.Coordinate(Hsy.SaturationOf(color, Hsy.LumaOf(color))));

    [UseDelegateFromConstructor]
    public partial double Level(Vector4 color, Vector256<double> weights);
}

[SmartEnum<string>]
public sealed partial class SortDirection {
    public static readonly SortDirection Right = new("right", order: 1, transposed: false);
    public static readonly SortDirection Left = new("left", order: -1, transposed: false);
    public static readonly SortDirection Down = new("down", order: 1, transposed: true);
    public static readonly SortDirection Up = new("up", order: -1, transposed: true);

    public int Order { get; }
    public bool Transposed { get; }

    public PixelPass Sorted(SortMetric metric, double low, double high, Gamut gamut) {
        Vector256<double> luminance = gamut.Luminance;
        PixelPass.Pointwise rows = new((row, _, _) => {
            using SpanOwner<double> owner = SpanOwner<double>.Allocate(row.Length);
            Span<double> keys = owner.Span;
            for (int x = 0; x < row.Length; x++)
                keys[x] = metric.Level(row[x], luminance);
            for (int x = 0, start = 0; x <= row.Length; x++) {
                if (x < row.Length && row[x].W > 0f && (low <= high ? keys[x] >= low && keys[x] <= high : keys[x] >= low || keys[x] <= high))
                    continue;
                keys[start..x].Sort(row[start..x], (level, other) => Order * level.CompareTo(other));
                start = x + 1;
            }
        });
        return Transposed ? new PixelPass.Frame(Columns) : rows;

        Fin<Unit> Columns(PixelFrame frame, IProgress<int> progress) => frame.Filter(header => {
            PixelFrame transposed = new(Point.Empty, frame.Size.Transposed, frame.Size.Transposed);
            using Mat lines = transposed.Header();
            CvInvoke.Transpose(header, lines);
            return rows.Run(transposed, new Progress<int>()).Map(_ => {
                CvInvoke.Transpose(lines, header);
                progress.Report(frame.Size.Height);
                return unit;
            });
        });
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Reduction {
    public static Fin<Unit> Quantized(PixelFrame frame, IQuantizer quantizer, IProgress<int> progress) =>
        frame.Mutate(image => image.Mutate(context => context.Quantize(quantizer))).Map(_ => {
            progress.Report(frame.Size.Height);
            return unit;
        });

    public static Fin<Action<Span<Vector4>, long, long>> Posterized(int levels, float strength, float retainHue, double cell, int height) =>
        levels >= 2 && cell > 0d
            ? Fin.Succ<Action<Span<Vector4>, long, long>>((row, column, line) => {
                int v = Ordered((2L * (height - line)) - 1L, cell);
                for (int x = 0; x < row.Length; x++) {
                    int b = Ordered((2L * (column + x)) + 1L, cell) ^ v;
                    int rank = ((b & 1) << 5) | ((v & 1) << 4) | ((b & 2) << 2) | ((v & 2) << 1) | ((b & 4) >> 1) | ((v & 4) >> 2);
                    row[x] = Leveled(row[x], levels - 1, strength * (((rank + 0.5d) / 64d) - 0.5d), retainHue);
                }
            })
            : new ImagingFault(string.Create(CultureInfo.InvariantCulture, $"posterize of {levels} levels and dither cell {cell}"), ImagingReason.Range);

    private static int Ordered(long doubled, double cell) => (int)((long)double.Floor(doubled / 2d / cell) & 7);

    private static Vector4 Leveled(Vector4 pixel, double steps, double offset, float retainHue) =>
        (Vector256.Floor((Vector256.Clamp(Vector256.Create(pixel.X, pixel.Y, pixel.Z, 0d), Vector256<double>.Zero, Vector256.Create(1d)) * steps) + Vector256.Create(0.5d + offset)) / steps) switch {
            var codes => new Vector4((float)codes.GetElement(0), (float)codes.GetElement(1), (float)codes.GetElement(2), pixel.W) switch {
                var level when retainHue > 0f => Hsy.LumaOf(level) switch {
                    var luma => new Vector4(Vector3.Lerp(level.AsVector3(), Hsy.ToRgb(Hsy.HueOf(pixel), Hsy.SaturationOf(level, luma), luma, pixel.W).AsVector3(), retainHue), pixel.W),
                },
                var level => level,
            },
        };
}
