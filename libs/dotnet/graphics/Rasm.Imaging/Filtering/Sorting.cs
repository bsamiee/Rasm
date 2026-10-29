using System.Drawing;
using System.Numerics;
using CommunityToolkit.HighPerformance.Buffers;
using Emgu.CV;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Pixels;

namespace Rasm.Imaging.Filtering;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class SortMetric {
    public static readonly SortMetric Hue = new("hue", Vector3.UnitX);
    public static readonly SortMetric Saturation = new("saturation", Vector3.UnitY);
    public static readonly SortMetric Luma = new("luma", Vector3.UnitZ);

    internal Vector3 Axis { get; }
}

[SmartEnum<string>]
public sealed partial class SortDirection {
    public static readonly SortDirection Right = new("right", descending: false, transposed: false);
    public static readonly SortDirection Left = new("left", descending: true, transposed: false);
    public static readonly SortDirection Down = new("down", descending: false, transposed: true);
    public static readonly SortDirection Up = new("up", descending: true, transposed: true);

    private readonly bool descending;
    private readonly bool transposed;

    public PixelPass Sorted(SortMetric metric, GradingStyle style, Vector3 luminance, float low, float high) {
        PixelPass.Point rows = new(row => {
            using SpanOwner<float> owner = SpanOwner<float>.Allocate(row.Length);
            Span<float> keys = owner.Span;
            for (int x = 0; x < row.Length; x++)
                keys[x] = Vector3.Dot(metric.Axis, style.ToHsy(row[x].AsVector3()) with { Z = Vector3.Dot(luminance, row[x].AsVector3()) });
            for (int x = 0, start = 0; x <= row.Length; x++) {
                if (x < row.Length && row[x].W > 0f && (low <= high ? keys[x] >= low && keys[x] <= high : keys[x] >= low || keys[x] <= high))
                    continue;
                keys[start..x].Sort(row[start..x]);
                if (descending)
                    row[start..x].Reverse();
                start = x + 1;
            }
        });
        return transposed ? new PixelPass.Frame(Columns) : rows;

        Fin<Unit> Columns(PixelFrame frame, Option<IProgress<int>> progress) => frame.Filter(header => {
            PixelFrame lines = new(Point.Empty, frame.Size.Transposed, frame.Size.Transposed);
            using Mat view = lines.Header();
            CvInvoke.Transpose(header, view);
            return rows.Run(lines).Map(_ => {
                CvInvoke.Transpose(view, header);
                return progress.Iter(sink => sink.Report(frame.Size.Height));
            });
        });
    }
}
