using System.Numerics;
using CommunityToolkit.HighPerformance.Buffers;
using Emgu.CV;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Tone;

namespace Rasm.Imaging.Filtering;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class SortMetric {
    public static readonly SortMetric Luma = new("luma", static (color, weights) => LuminanceSamples.Luminance(color.X, color.Y, color.Z, weights));
    public static readonly SortMetric Hue = new("hue", static (color, _) => RangeAxis.Hue.Coordinate(Hsy.From(color)));
    public static readonly SortMetric Saturation = new("saturation", static (color, _) => RangeAxis.Saturation.Coordinate(Hsy.From(color)));

    [UseDelegateFromConstructor]
    public partial double Level(Vector4 color, (double R, double G, double B) weights);
}

[SmartEnum<string>]
public sealed partial class SortDirection {
    public static readonly SortDirection Right = new("right", order: 1, transposed: false);
    public static readonly SortDirection Left = new("left", order: -1, transposed: false);
    public static readonly SortDirection Down = new("down", order: 1, transposed: true);
    public static readonly SortDirection Up = new("up", order: -1, transposed: true);

    public int Order { get; }
    public bool Transposed { get; }

    public PixelPass Sorted(SortMetric metric, double low, double high, (double R, double G, double B) luminance) {
        int order = Order;
        PixelPass.Pointwise rows = new((row, _, _) => {
            using SpanOwner<double> owner = SpanOwner<double>.Allocate(row.Length);
            Span<double> keys = owner.Span;
            for (int x = 0; x < row.Length; x++)
                keys[x] = metric.Level(row[x], luminance);
            for (int x = 0, start = 0; x <= row.Length; x++) {
                if (x < row.Length && row[x].W > 0f && (low <= high ? keys[x] >= low && keys[x] <= high : keys[x] >= low || keys[x] <= high))
                    continue;
                keys[start..x].Sort(row[start..x], (level, other) => order * level.CompareTo(other));
                start = x + 1;
            }
        });
        return Transposed ? new PixelPass.Frame(Columns) : rows;

        unsafe Fin<Unit> Columns(PixelFrame frame, IProgress<int> progress) => frame.Filter(header => {
            using Mat lines = new();
            CvInvoke.Transpose(header, lines);
            Span<Vector4> pixels = new(lines.DataPointer.ToPointer(), lines.Rows * lines.Cols);
            for (int y = 0; y < lines.Rows; y++)
                rows.Row(pixels.Slice(y * lines.Cols, lines.Cols), 0, y);
            CvInvoke.Transpose(lines, header);
            progress.Report(frame.Size.Height);
            return unit;
        });
    }
}
