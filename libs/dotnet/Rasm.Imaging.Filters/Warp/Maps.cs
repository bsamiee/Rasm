using System.Drawing;
using System.Numerics;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Helpers;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Rasm.Imaging.Pixels;

namespace Rasm.Imaging.Filters.Warp;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>]
[ValidationError<InvalidWarp>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Sampling {
    public static readonly Sampling Nearest = new("nearest", Inter.Nearest, 0, Some<Action<Span<Vector2>>>(static points => { foreach (ref Vector2 point in points) point = Vector2.Round(point, MidpointRounding.ToNegativeInfinity) + new Vector2(0.5f); }));
    public static readonly Sampling Linear = new("linear", Inter.Linear, 1, None);
    public static readonly Sampling Cubic = new("cubic", Inter.Cubic, 2, None);

    internal Inter Interpolation { get; }
    internal int Reach { get; }
    internal Option<Action<Span<Vector2>>> Snap { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CoordinateMap {
    private const int Tile = 1024;
    private static readonly int Pad = Sampling.Items.Max(static sampling => sampling.Reach);

    private CoordinateMap(Sampling sampling, WrapMode wrap) => (Sampling, Wrap) = (sampling, wrap);

    public Sampling Sampling { get; }
    public WrapMode Wrap { get; }

    public Fin<Unit> Apply(PixelFrame frame, IProgress<int> progress) => Run(frame, progress, [(this, Vector4.One)]);

    public PixelFrame Resample(PixelFrame source, PixelExtent extent) => new(Point.Empty, extent, extent, block =>
        Sample(source, new Rectangle(0, 0, extent.Width, extent.Height), extent, [(this, Vector4.One)], block.AsSpan().Cast<float, Vector4>().AsSpan2D(extent.Height, extent.Width)));

    public PixelFrame Bake(PixelExtent extent) {
        Rectangle whole = new(0, 0, extent.Width, extent.Height);
        using Mat points = new(extent.Height, extent.Width, DepthType.Cv32F, 2);
        using Mat coverage = new(extent.Height, extent.Width, DepthType.Cv32F, 1);
        ParallelHelper.For(0, extent.Height, new TraceRows(this, points, coverage, whole, extent, extent, whole));
        return new(Point.Empty, extent, extent, block => {
            Span<Vector4> pixels = block.AsSpan().Cast<float, Vector4>();
            ReadOnlySpan<Vector2> at = points.GetSpan<Vector2>();
            ReadOnlySpan<float> kept = coverage.GetSpan<float>();
            for (int i = 0; i < pixels.Length; i++) pixels[i] = new Vector4(at[i].X / extent.Width, 1f - (at[i].Y / extent.Height), kept[i], kept[i]);
        });
    }

    public static Func<PixelFrame, IProgress<int>, Fin<Unit>> Integrated(Func<float, (CoordinateMap Map, Vector4 Lanes)> subframe) =>
        (frame, progress) => {
            (PixelExtent extent, int diagonal) = (frame.Extent, (int)double.Ceiling(double.Hypot(frame.Extent.Width, frame.Extent.Height)));
            Vector2[][] corners = [.. Enumerable.Range(0, diagonal + 1).Select(step => {
                Vector2[] points = [new(0.5f), new(extent.Width - 0.5f, 0.5f), new(extent.Width - 0.5f, extent.Height - 0.5f), new(0.5f, extent.Height - 0.5f)];
                subframe((float)step / diagonal).Map.Trace(points, stackalloc float[points.Length], extent, extent);
                return points;
            })];
            int steps = int.Clamp((int)float.Ceiling(Enumerable.Range(0, 4).Max(corner => Enumerable.Range(1, diagonal).Sum(step => Vector2.Distance(corners[step - 1][corner], corners[step][corner])))), 1, diagonal);
            return subframe(0f).Map.Run(frame, progress, toSeq(Enumerable.Range(0, steps + 1).Select(step => subframe((float)step / steps))));
        };

    internal abstract void Trace(Span<Vector2> points, Span<float> coverage, PixelExtent output, PixelExtent source);

    internal void Bound(Span<Vector2> points, Span<float> coverage, Rectangle window) {
        if (Sampling.Snap.Case is Action<Span<Vector2>> snap) snap(points);
        for (int i = 0; i < points.Length; i++) {
            ((float x, float across), (float y, float down)) = (Wrap.Fold(points[i].X - window.X, window.Width), Wrap.Fold(points[i].Y - window.Y, window.Height));
            points[i] = new Vector2(x, y);
            coverage[i] *= across * down;
        }
    }

    private Fin<Unit> Run(PixelFrame frame, IProgress<int> progress, Seq<(CoordinateMap Map, Vector4 Lanes)> subframes) {
        Sample(frame, frame.Window, frame.Extent, subframes, frame.View.Span);
        progress.Report(frame.Size.Height);
        return unit;
    }

    private void Sample(PixelFrame source, Rectangle window, PixelExtent output, Seq<(CoordinateMap Map, Vector4 Lanes)> subframes, Span2D<Vector4> destination) {
        BorderType border = Wrap.Map(black: BorderType.Constant, clamp: BorderType.Replicate, periodic: BorderType.Wrap, mirror: BorderType.Reflect);
        using Mat header = source.Header();
        using Mat padded = new();
        CvInvoke.CopyMakeBorder(header, padded, Pad, Pad, Pad, Pad, border);
        foreach (ref Vector4 pixel in padded.GetSpan<Vector4>()) pixel = new Vector4(pixel.AsVector3() * pixel.W, pixel.W);
        using Mat points = new(int.Min(Tile, window.Height), int.Min(Tile, window.Width), DepthType.Cv32F, 2);
        using Mat coverage = new(points.Rows, points.Cols, DepthType.Cv32F, 1);
        using Mat samples = new(points.Rows, points.Cols, DepthType.Cv32F, 4);
        using Mat unused = new();
        Vector4 total = subframes.Fold(Vector4.Zero, static (sum, subframe) => sum + subframe.Lanes);
        destination.Clear();
        foreach ((CoordinateMap map, Vector4 lanes) in subframes) foreach (Rectangle area in Cut(window, new Size(Tile, Tile))) Accumulate(map, lanes / total, area, destination);
        foreach (ref Vector4 pixel in destination) pixel = pixel.W > 0f ? new Vector4(pixel.AsVector3() / pixel.W, pixel.W) : Vector4.Zero;

        void Accumulate(CoordinateMap map, Vector4 lanes, Rectangle area, Span2D<Vector4> destination) {
            using Mat traced = Shaped(points, area.Size);
            using Mat share = Shaped(coverage, area.Size);
            ParallelHelper.For(0, area.Height, new TraceRows(map, traced, share, area, output, source.Extent, source.Window));
            traced.MinMax(out double[] low, out double[] high, out _, out _);
            Rectangle box = Rectangle.Intersect(new Rectangle(Point.Empty, padded.Size), Rectangle.Inflate(Rectangle.FromLTRB((int)low[0] + Pad, (int)low[1] + Pad, (int)high[0] + Pad + 1, (int)high[1] + Pad + 1), map.Sampling.Reach, map.Sampling.Reach));
            if (box.Width >= short.MaxValue || box.Height >= short.MaxValue)
                foreach (Rectangle quarter in Cut(area, new Size((area.Width + 1) / 2, (area.Height + 1) / 2))) Accumulate(map, lanes, quarter, destination);
            else if (CvInvoke.CountNonZero(share) > 0) {
                foreach (ref Vector2 point in traced.GetSpan<Vector2>()) point += new Vector2(Pad - 0.5f) - new Vector2(box.X, box.Y);
                using Mat region = new(padded, box);
                using Mat sampled = Shaped(samples, area.Size);
                CvInvoke.Remap(region, sampled, traced, unused, map.Sampling.Interpolation, border);
                ReadOnlySpan<Vector4> read = sampled.GetSpan<Vector4>();
                ReadOnlySpan<float> weights = share.GetSpan<float>();
                for (int i = 0; i < read.Length; i++) destination[area.Y - window.Y + (i / area.Width), area.X - window.X + (i % area.Width)] += lanes * (weights[i] * read[i]);
            }
        }

        static Mat Shaped(Mat scratch, Size size) => new(size.Height, size.Width, scratch.Depth, scratch.NumberOfChannels, scratch.DataPointer, size.Width * scratch.ElementSize);

        static IEnumerable<Rectangle> Cut(Rectangle area, Size size) =>
            from down in Enumerable.Range(0, (area.Height + size.Height - 1) / size.Height) from across in Enumerable.Range(0, (area.Width + size.Width - 1) / size.Width)
            select Rectangle.Intersect(new Rectangle(area.X + (across * size.Width), area.Y + (down * size.Height), size.Width, size.Height), area);
    }

    public sealed record Analytic(Action<Span<Vector2>, Span<float>, PixelExtent> Kernel, Sampling Sampling, WrapMode Wrap) : CoordinateMap(Sampling, Wrap) {
        internal override void Trace(Span<Vector2> points, Span<float> coverage, PixelExtent output, PixelExtent source) => Kernel(points, coverage, source);
    }

    public sealed record Sampled(PixelFrame St, Sampling Sampling, WrapMode Wrap) : CoordinateMap(Sampling, Wrap) {
        internal override void Trace(Span<Vector2> points, Span<float> coverage, PixelExtent output, PixelExtent source) {
            ReadOnlySpan2D<Vector4> st = St.View.Span;
            Vector2 scale = new Vector2(St.Size.Width, St.Size.Height) / new Vector2(output.Width, output.Height);
            for (int i = 0; i < points.Length; i++)
                (points[i], coverage[i]) = st.Sample(points[i] * scale, (WrapMode.Clamp, WrapMode.Clamp)) switch { var read => (new Vector2(read.X, 1f - read.Y) * new Vector2(source.Width, source.Height), coverage[i] * read.W) };
        }
    }

    public sealed record Composed(CoordinateMap First, CoordinateMap Second) : CoordinateMap(First.Sampling, First.Wrap) {
        internal override void Trace(Span<Vector2> points, Span<float> coverage, PixelExtent output, PixelExtent source) {
            Second.Trace(points, coverage, output, output);
            Second.Bound(points, coverage, new Rectangle(0, 0, output.Width, output.Height));
            First.Trace(points, coverage, output, source);
        }
    }
}

file readonly struct TraceRows(CoordinateMap map, Mat points, Mat coverage, Rectangle area, PixelExtent output, PixelExtent source, Rectangle window) : IAction {
    public void Invoke(int i) {
        Span<Vector2> row = points.GetSpan<Vector2>().Slice(i * area.Width, area.Width);
        Span<float> share = coverage.GetSpan<float>().Slice(i * area.Width, area.Width);
        for (int x = 0; x < row.Length; x++) row[x] = new Vector2(area.X + x + 0.5f, area.Y + i + 0.5f);
        share.Fill(1f);
        map.Trace(row, share, output, source);
        map.Bound(row, share, window);
    }
}
