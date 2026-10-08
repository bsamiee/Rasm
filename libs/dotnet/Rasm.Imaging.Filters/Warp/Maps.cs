using System.Drawing;
using System.Numerics;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using CommunityToolkit.HighPerformance.Helpers;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Rasm.Imaging.Pixels;

namespace Rasm.Imaging.Filters.Warp;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>]
[ValidationError<InvalidWarp>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Sampling {
    public static readonly Sampling Nearest = new("nearest", Inter.Nearest, 1, static (points, _, _) => {
        foreach (ref Vector2 point in points) point = Vector2.Round(point, MidpointRounding.ToNegativeInfinity) + new Vector2(0.5f);
    });
    public static readonly Sampling Linear = new("linear", Inter.Linear, 1, static (_, _, _) => { });
    public static readonly Sampling Cubic = new("cubic", Inter.Cubic, 2, static (_, _, _) => { });

    internal Inter Interpolation { get; }
    internal int Reach { get; }
    internal Action<Span<Vector2>, Span<float>, PixelExtent> Snap { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CoordinateMap {
    private const int Tile = 1024;
    internal static readonly int Pad = Sampling.Items.Max(static sampling => sampling.Reach);

    private CoordinateMap(Sampling sampling, WrapMode wrap) => (Sampling, Wrap) = (sampling, wrap);

    public Sampling Sampling { get; }
    public WrapMode Wrap { get; }
    internal BorderType Border => Wrap.Map(black: BorderType.Replicate, clamp: BorderType.Replicate, periodic: BorderType.Wrap, mirror: BorderType.Reflect);

    public CoordinateMap Then(CoordinateMap next) => new Composed(this, next);

    public Fin<Unit> Apply(PixelFrame frame, IProgress<int> progress) => Run(frame, progress, Border, [(this, Vector4.One)]);

    public PixelFrame Resample(PixelFrame source, PixelExtent extent) =>
        new(Point.Empty, extent, extent, block => Sample(source, new Rectangle(0, 0, extent.Width, extent.Height), extent, Border, [(this, Vector4.One)], block.AsMemory().Cast<float, Vector4>().AsMemory2D(extent.Height, extent.Width)));

    public PixelFrame Bake(PixelExtent extent) =>
        new(Point.Empty, extent, extent, block => ParallelHelper.For(0, extent.Height, new BakeRows(this, block.AsMemory().Cast<float, Vector4>().AsMemory2D(extent.Height, extent.Width), extent)));

    public static Func<PixelFrame, IProgress<int>, Fin<Unit>> Integrated(Func<float, (CoordinateMap Map, Vector4 Lanes)> subframe) =>
        (frame, progress) => {
            int diagonal = (int)Math.Ceiling(double.Hypot(frame.Extent.Width, frame.Extent.Height));
            Seq<Vector2[]> corners = toSeq(Enumerable.Range(0, diagonal + 1).Select(step => subframe((float)step / diagonal).Map.Corners(frame.Extent))).Strict();
            int steps = int.Clamp((int)float.Ceiling(Enumerable.Range(0, 4).Max(corner =>
                corners.Zip(corners.Tail).Fold(0f, (path, step) => path + Vector2.Distance(step.First[corner], step.Second[corner])))), 1, diagonal);
            return Run(frame, progress, subframe(0f).Map.Border, toSeq(Enumerable.Range(0, steps + 1).Select(step => subframe((float)step / steps))).Strict());
        };

    internal void Traced(Span<Vector2> points, Span<float> coverage, Point corner, PixelExtent output, PixelExtent source) {
        for (int x = 0; x < points.Length; x++) points[x] = new Vector2(corner.X + x + 0.5f, corner.Y + 0.5f);
        coverage.Fill(1f);
        Trace(points, coverage, output, source);
    }

    internal abstract void Trace(Span<Vector2> points, Span<float> coverage, PixelExtent output, PixelExtent source);

    internal void Folded(Span<Vector2> points, Span<float> coverage, PixelExtent source) {
        for (int i = 0; i < points.Length; i++) {
            ((float x, float across), (float y, float down)) = (Wrap.Fold(points[i].X, source.Width), Wrap.Fold(points[i].Y, source.Height));
            points[i] = new Vector2(x, y);
            coverage[i] = coverage[i] * across * down;
        }
    }

    private Vector2[] Corners(PixelExtent extent) {
        Vector2[] points = [new(0.5f), new(extent.Width - 0.5f, 0.5f), new(extent.Width - 0.5f, extent.Height - 0.5f), new(0.5f, extent.Height - 0.5f)];
        Trace(points, stackalloc float[points.Length], extent, extent);
        return points;
    }

    private static Fin<Unit> Run(PixelFrame frame, IProgress<int> progress, BorderType border, Seq<(CoordinateMap Map, Vector4 Lanes)> subframes) {
        Sample(frame, frame.Window, frame.Extent, border, subframes, frame.View);
        progress.Report(frame.Size.Height);
        return unit;
    }

    private static void Sample(PixelFrame source, Rectangle window, PixelExtent output, BorderType border, Seq<(CoordinateMap Map, Vector4 Lanes)> subframes, Memory2D<Vector4> destination) {
        using Mat header = source.Header();
        using Mat padded = new();
        CvInvoke.CopyMakeBorder(header, padded, Pad, Pad, Pad, Pad, border, default);
        foreach (ref Vector4 pixel in padded.GetSpan<Vector4>()) pixel = new Vector4(pixel.AsVector3() * pixel.W, pixel.W);
        Size tile = new(int.Min(Tile, window.Width), int.Min(Tile, window.Height));
        using Mat points = new(tile.Height, tile.Width, DepthType.Cv32F, 2);
        using Mat coverage = new(tile.Height, tile.Width, DepthType.Cv32F, 1);
        using Mat samples = new(tile.Height, tile.Width, DepthType.Cv32F, 4);
        Tiles tiles = new(source, padded, output, subframes, points, coverage, samples);
        int across = (window.Width + Tile - 1) / Tile;
        for (int index = 0; index < across * ((window.Height + Tile - 1) / Tile); index++) {
            Rectangle area = Rectangle.Intersect(new Rectangle(window.X + (index % across * Tile), window.Y + (index / across * Tile), Tile, Tile), window);
            tiles.Fill(area, destination.Span.Slice(area.Y - window.Y, area.X - window.X, area.Height, area.Width));
        }
    }

    public sealed record Analytic(Action<Span<Vector2>, Span<float>, PixelExtent> Kernel, Sampling Sampling, WrapMode Wrap) : CoordinateMap(Sampling, Wrap) {
        internal override void Trace(Span<Vector2> points, Span<float> coverage, PixelExtent output, PixelExtent source) => Kernel(points, coverage, source);
    }

    public sealed record Sampled(PixelFrame St, Sampling Sampling, WrapMode Wrap) : CoordinateMap(Sampling, Wrap) {
        internal override void Trace(Span<Vector2> points, Span<float> coverage, PixelExtent output, PixelExtent source) {
            ReadOnlySpan2D<Vector4> st = St.View.Span;
            Vector2 scale = new Vector2(St.Size.Width, St.Size.Height) / new Vector2(output.Width, output.Height);
            for (int i = 0; i < points.Length; i++) {
                Vector4 read = st.Sample(points[i] * scale, (WrapMode.Clamp, WrapMode.Clamp));
                (points[i], coverage[i]) = (new Vector2(read.X, 1f - read.Y) * new Vector2(source.Width, source.Height), coverage[i] * read.W);
            }
        }
    }

    public sealed record Composed(CoordinateMap First, CoordinateMap Second) : CoordinateMap(First.Sampling, First.Wrap) {
        internal override void Trace(Span<Vector2> points, Span<float> coverage, PixelExtent output, PixelExtent source) {
            Second.Trace(points, coverage, output, output);
            Second.Sampling.Snap(points, coverage, output);
            Second.Folded(points, coverage, output);
            First.Trace(points, coverage, output, source);
        }
    }
}

file readonly struct Tiles(PixelFrame source, Mat padded, PixelExtent output, Seq<(CoordinateMap Map, Vector4 Lanes)> subframes, Mat points, Mat coverage, Mat samples) {
    private const int Side = short.MaxValue;
    private readonly Vector4 total = subframes.Fold(Vector4.Zero, static (sum, subframe) => sum + subframe.Lanes);

    public void Fill(Rectangle area, Span2D<Vector4> target) {
        target.Clear();
        foreach ((CoordinateMap map, Vector4 lanes) in subframes) Accumulate(map, lanes, area, target);
        foreach (ref Vector4 pixel in target) pixel = pixel.W > 0f ? new Vector4(pixel.AsVector3() / total.AsVector3() * (total.W / pixel.W), pixel.W / total.W) : Vector4.Zero;
    }

    private void Accumulate(CoordinateMap map, Vector4 lanes, Rectangle area, Span2D<Vector4> target) {
        using Mat traced = Shaped(points, area.Size);
        using Mat share = Shaped(coverage, area.Size);
        ParallelHelper.For(0, area.Height, new TraceRows(map, traced, share, area, output, source));
        ReadOnlySpan<Vector2> at = traced.GetSpan<Vector2>();
        ReadOnlySpan<float> weights = share.GetSpan<float>();
        (Vector2 low, Vector2 high) = (new(float.MaxValue), new(float.MinValue));
        for (int i = 0; i < at.Length; i++) (low, high) = weights[i] > 0f ? (Vector2.Min(low, at[i]), Vector2.Max(high, at[i])) : (low, high);
        if (low.X > high.X)
            return;
        Vector2 limit = new(padded.Cols, padded.Rows);
        Vector2 start = Vector2.Clamp(Vector2.Round(low, MidpointRounding.ToNegativeInfinity) - new Vector2(map.Sampling.Reach), Vector2.Zero, limit - Vector2.One);
        Vector2 end = Vector2.Clamp(Vector2.Round(high, MidpointRounding.ToNegativeInfinity) + new Vector2(map.Sampling.Reach + 1), start + Vector2.One, limit);
        Rectangle box = Rectangle.FromLTRB((int)start.X, (int)start.Y, (int)end.X, (int)end.Y);
        if (box.Width >= Side || box.Height >= Side) {
            Size half = new((area.Width + 1) / 2, (area.Height + 1) / 2);
            foreach (Rectangle quarter in Seq(0, 1, 2, 3).Map(k => Rectangle.Intersect(new Rectangle(area.X + (k % 2 * half.Width), area.Y + (k / 2 * half.Height), half.Width, half.Height), area)).Filter(static quarter => quarter.Width > 0 && quarter.Height > 0))
                Accumulate(map, lanes, quarter, target.Slice(quarter.Y - area.Y, quarter.X - area.X, quarter.Height, quarter.Width));
            return;
        }
        using ScalarArray corner = new(new MCvScalar(box.X, box.Y));
        CvInvoke.Subtract(traced, corner, traced);
        using Mat region = new(padded, box);
        using Mat sampled = Shaped(samples, area.Size);
        using Mat empty = new();
        CvInvoke.Remap(region, sampled, traced, empty, map.Sampling.Interpolation, map.Border, default);
        ReadOnlySpan<Vector4> read = sampled.GetSpan<Vector4>();
        for (int i = 0; i < read.Length; i++) target[i / area.Width, i % area.Width] += lanes * (weights[i] * read[i]);
    }

    private static Mat Shaped(Mat scratch, Size size) =>
        new(size.Height, size.Width, scratch.Depth, scratch.NumberOfChannels, scratch.DataPointer, size.Width * scratch.ElementSize);
}

file readonly struct TraceRows(CoordinateMap map, Mat points, Mat coverage, Rectangle area, PixelExtent output, PixelFrame source) : IAction {
    public void Invoke(int i) {
        Span<Vector2> row = points.GetSpan<Vector2>().Slice(i * area.Width, area.Width);
        Span<float> share = coverage.GetSpan<float>().Slice(i * area.Width, area.Width);
        map.Traced(row, share, new Point(area.X, area.Y + i), output, source.Extent);
        map.Folded(row, share, source.Extent);
        foreach (ref Vector2 point in row) point += new Vector2(CoordinateMap.Pad - 0.5f) - new Vector2(source.Origin.X, source.Origin.Y);
    }
}

file readonly struct BakeRows(CoordinateMap map, Memory2D<Vector4> rows, PixelExtent extent) : IAction {
    public void Invoke(int i) {
        using SpanOwner<Vector2> points = SpanOwner<Vector2>.Allocate(extent.Width);
        using SpanOwner<float> coverage = SpanOwner<float>.Allocate(extent.Width);
        map.Traced(points.Span, coverage.Span, new Point(0, i), extent, extent);
        map.Sampling.Snap(points.Span, coverage.Span, extent);
        map.Folded(points.Span, coverage.Span, extent);
        Span<Vector4> row = rows.Span.GetRowSpan(i);
        for (int x = 0; x < row.Length; x++) row[x] = new Vector4(points.Span[x].X / extent.Width, 1f - (points.Span[x].Y / extent.Height), coverage.Span[x], coverage.Span[x]);
    }
}
