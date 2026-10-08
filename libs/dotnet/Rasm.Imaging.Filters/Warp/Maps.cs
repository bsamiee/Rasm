using System.Drawing;
using System.Numerics;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using CommunityToolkit.HighPerformance.Helpers;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Rasm.Imaging.Filters.Generators;
using Rasm.Imaging.Pixels;
using UnitsNet.Units;

namespace Rasm.Imaging.Filters.Warp;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>]
[ValidationError<InvalidWarp>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Sampling {
    public static readonly Sampling Nearest = new("nearest", Inter.Nearest, 1, Some<Action<Span<Vector2>, Span<float>, PixelExtent>>(static (points, _, _) => {
        foreach (ref Vector2 point in points) point = Vector2.Round(point, MidpointRounding.ToNegativeInfinity) + new Vector2(0.5f);
    }));
    public static readonly Sampling Linear = new("linear", Inter.Linear, 1, None);
    public static readonly Sampling Cubic = new("cubic", Inter.Cubic, 2, None);

    internal Inter Interpolation { get; }
    internal int Reach { get; }
    internal Option<Action<Span<Vector2>, Span<float>, PixelExtent>> Snap { get; }
}

[SmartEnum<string>]
[ValidationError<InvalidWarp>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class EdgeMode {
    private static readonly Action<Span<Vector2>, Span<float>, PixelExtent> Periodically = static (points, _, source) => {
        Vector2 size = new(source.Width, source.Height);
        foreach (ref Vector2 point in points) point -= size * Vector2.Round(point / size, MidpointRounding.ToNegativeInfinity);
    };
    private static readonly Action<Span<Vector2>, Span<float>, PixelExtent> Mirrored = static (points, _, source) => {
        Vector2 span = new(2f * source.Width, 2f * source.Height);
        foreach (ref Vector2 point in points) {
            Vector2 folded = point - (span * Vector2.Round(point / span, MidpointRounding.ToNegativeInfinity));
            point = Vector2.Min(folded, span - folded);
        }
    };

    public static readonly EdgeMode Black = new("black", BorderType.Constant, static (points, coverage, source) => {
        Vector2 size = new(source.Width, source.Height);
        for (int i = 0; i < points.Length; i++) {
            Vector2 kept = Vector2.Clamp(points[i] + new Vector2(0.5f), Vector2.Zero, Vector2.One) * Vector2.Clamp(size + new Vector2(0.5f) - points[i], Vector2.Zero, Vector2.One);
            (coverage[i], points[i]) = (coverage[i] * kept.X * kept.Y, Vector2.Clamp(points[i], new Vector2(0.5f), size - new Vector2(0.5f)));
        }
    }, None);
    public static readonly EdgeMode Clamp = new("clamp", BorderType.Replicate, static (points, _, source) => {
        foreach (ref Vector2 point in points) point = Vector2.Clamp(point, new Vector2(0.5f), new Vector2(source.Width - 0.5f, source.Height - 0.5f));
    }, None);
    public static readonly EdgeMode Periodic = new("periodic", BorderType.Wrap, Periodically, Some(Periodically));
    public static readonly EdgeMode Mirror = new("mirror", BorderType.Reflect, Mirrored, Some(Mirrored));

    internal BorderType Border { get; }
    internal Action<Span<Vector2>, Span<float>, PixelExtent> Fold { get; }
    internal Option<Action<Span<Vector2>, Span<float>, PixelExtent>> Period { get; }
}

[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidWarp>]
public readonly partial struct ShutterSamples : IMinMaxValue<ShutterSamples> {
    public static ShutterSamples MinValue { get; } = new(1);
    public static ShutterSamples MaxValue { get; } = new(29);

    public int Count(PixelExtent frame) => int.Min(1 << _value, (int)Math.Ceiling(double.Hypot(frame.Width, frame.Height)));

    static partial void ValidateFactoryArguments(ref InvalidWarp? validationError, ref int value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidWarp();
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CoordinateMap {
    private const int Tile = 1024;
    internal static readonly int Pad = Sampling.Items.Max(static sampling => sampling.Reach);

    public abstract Sampling Sampling { get; }
    public abstract EdgeMode Wrap { get; }

    public CoordinateMap Then(CoordinateMap next) => new Composed(this, next);

    public Fin<Unit> Apply(PixelFrame frame, IProgress<int> progress) => Run(frame, progress, Wrap.Border, [(this, Vector4.One)]);

    public PixelFrame Resample(PixelFrame source, PixelExtent extent) =>
        new(Point.Empty, extent, extent, block => Sample(source, new Rectangle(0, 0, extent.Width, extent.Height), extent, Wrap.Border, [(this, Vector4.One)], block.AsMemory().Cast<float, Vector4>().AsMemory2D(extent.Height, extent.Width)));

    public PixelFrame Bake(PixelExtent extent) =>
        new(Point.Empty, extent, extent, block => ParallelHelper.For(0, extent.Height, new BakeRows(this, block.AsMemory().Cast<float, Vector4>(), extent)));

    public static Func<PixelFrame, IProgress<int>, Fin<Unit>> Integrated(int steps, Func<float, (CoordinateMap Map, Vector4 Lanes)> subframe) =>
        (subframe(0f), toSeq(Enumerable.Range(1, steps).Select(step => subframe((float)step / steps))).Strict()) switch {
            var (start, rest) => (frame, progress) => Run(frame, progress, start.Map.Wrap.Border, start.Cons(rest)),
        };

    internal void Traced(Span<Vector2> points, Span<float> coverage, Point corner, PixelExtent output, PixelExtent source) {
        for (int x = 0; x < points.Length; x++) points[x] = new Vector2(corner.X + x + 0.5f, corner.Y + 0.5f);
        coverage.Fill(1f);
        Trace(points, coverage, output, source);
    }

    internal abstract void Trace(Span<Vector2> points, Span<float> coverage, PixelExtent output, PixelExtent source);

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
        using Mat axis = new(tile.Height, tile.Width, DepthType.Cv32F, 1);
        using Mat mask = new(tile.Height, tile.Width, DepthType.Cv8U, 1);
        Tiles tiles = new(source, padded, output, window, destination, points, coverage, samples, axis, mask);
        Vector4 total = subframes.Fold(Vector4.Zero, static (sum, subframe) => sum + subframe.Lanes);
        int across = (window.Width + Tile - 1) / Tile;
        for (int index = 0; index < across * ((window.Height + Tile - 1) / Tile); index++) {
            Rectangle area = Rectangle.Intersect(new Rectangle(window.X + (index % across * Tile), window.Y + (index / across * Tile), Tile, Tile), window);
            tiles.Clear(area);
            foreach ((CoordinateMap map, Vector4 lanes) in subframes) tiles.Accumulate(map, lanes, area);
            tiles.Resolve(area, total);
        }
    }

    public sealed record Analytic(Action<Span<Vector2>, Span<float>, PixelExtent> Kernel, Sampling Sampling, EdgeMode Wrap) : CoordinateMap {
        public override Sampling Sampling { get; } = Sampling;
        public override EdgeMode Wrap { get; } = Wrap;

        internal override void Trace(Span<Vector2> points, Span<float> coverage, PixelExtent output, PixelExtent source) => Kernel(points, coverage, source);
    }

    public sealed record Sampled(PixelFrame St, Sampling Sampling, EdgeMode Wrap) : CoordinateMap {
        public override Sampling Sampling { get; } = Sampling;
        public override EdgeMode Wrap { get; } = Wrap;

        internal override void Trace(Span<Vector2> points, Span<float> coverage, PixelExtent output, PixelExtent source) {
            ReadOnlySpan2D<Vector4> st = St.View.Span;
            Vector2 scale = new Vector2(St.Size.Width, St.Size.Height) / new Vector2(output.Width, output.Height);
            for (int i = 0; i < points.Length; i++) {
                Vector4 read = st.Sample(points[i] * scale, (WrapMode.Clamp, WrapMode.Clamp));
                (points[i], coverage[i]) = (new Vector2(read.X, 1f - read.Y) * new Vector2(source.Width, source.Height), coverage[i] * read.W);
            }
        }
    }

    public sealed record Composed(CoordinateMap First, CoordinateMap Second) : CoordinateMap {
        public override Sampling Sampling => First.Sampling;
        public override EdgeMode Wrap => First.Wrap;

        internal override void Trace(Span<Vector2> points, Span<float> coverage, PixelExtent output, PixelExtent source) {
            Second.Trace(points, coverage, output, output);
            if (Second.Sampling.Snap.Case is Action<Span<Vector2>, Span<float>, PixelExtent> snap) snap(points, coverage, output);
            Second.Wrap.Fold(points, coverage, output);
            First.Trace(points, coverage, output, source);
        }
    }
}

file readonly struct Tiles(PixelFrame source, Mat padded, PixelExtent output, Rectangle window, Memory2D<Vector4> destination, Mat points, Mat coverage, Mat samples, Mat axis, Mat mask) {
    private const int Side = short.MaxValue;

    public void Clear(Rectangle area) {
        for (int y = 0; y < area.Height; y++) Rows(area, y).Clear();
    }

    public void Accumulate(CoordinateMap map, Vector4 lanes, Rectangle area) {
        using Mat traced = Shaped(points, area.Size);
        using Mat share = Shaped(coverage, area.Size);
        ParallelHelper.For(0, area.Height, new TraceRows(map, traced, share, area, output, source));
        if (Spread(share, mask: null).High <= 0d)
            return;
        using Mat plane = Shaped(axis, area.Size);
        using Mat covered = Shaped(mask, area.Size);
        using ScalarArray none = new(0d);
        CvInvoke.Compare(share, none, covered, CmpType.GreaterThan);
        CvInvoke.ExtractChannel(traced, plane, 0);
        (double left, double right) = Spread(plane, covered);
        CvInvoke.ExtractChannel(traced, plane, 1);
        (double top, double bottom) = Spread(plane, covered);
        (int x, int y) = ((int)double.Clamp(Math.Floor(left) - map.Sampling.Reach, 0d, padded.Cols - 1), (int)double.Clamp(Math.Floor(top) - map.Sampling.Reach, 0d, padded.Rows - 1));
        Rectangle box = Rectangle.FromLTRB(x, y,
            (int)double.Clamp(Math.Floor(right) + map.Sampling.Reach + 1d, x + 1, padded.Cols), (int)double.Clamp(Math.Floor(bottom) + map.Sampling.Reach + 1d, y + 1, padded.Rows));
        if (box.Width >= Side || box.Height >= Side) {
            Size half = new((area.Width + 1) / 2, (area.Height + 1) / 2);
            foreach (Rectangle quarter in Seq(new Rectangle(area.Location, half), Rectangle.FromLTRB(area.X + half.Width, area.Y, area.Right, area.Y + half.Height),
                Rectangle.FromLTRB(area.X, area.Y + half.Height, area.X + half.Width, area.Bottom), Rectangle.FromLTRB(area.X + half.Width, area.Y + half.Height, area.Right, area.Bottom)).Filter(static quarter => !quarter.IsEmpty))
                Accumulate(map, lanes, quarter);
            return;
        }
        using ScalarArray corner = new(new MCvScalar(box.X, box.Y));
        CvInvoke.Subtract(traced, corner, traced);
        using Mat region = new(padded, box);
        using Mat sampled = Shaped(samples, area.Size);
        using Mat empty = new();
        CvInvoke.Remap(region, sampled, traced, empty, map.Sampling.Interpolation, map.Wrap.Border, default);
        ReadOnlySpan<Vector4> read = sampled.GetSpan<Vector4>();
        ReadOnlySpan<float> weights = share.GetSpan<float>();
        for (int row = 0; row < area.Height; row++) {
            Span<Vector4> target = Rows(area, row);
            for (int column = 0; column < area.Width; column++) target[column] += lanes * (weights[(row * area.Width) + column] * read[(row * area.Width) + column]);
        }
    }

    public void Resolve(Rectangle area, Vector4 total) {
        for (int y = 0; y < area.Height; y++) {
            foreach (ref Vector4 pixel in Rows(area, y)) {
                Vector4 mean = pixel / total;
                pixel = mean.W > 0f ? new Vector4(mean.AsVector3() / mean.W, mean.W) : Vector4.Zero;
            }
        }
    }

    private Span<Vector4> Rows(Rectangle area, int y) =>
        destination.Span.GetRowSpan(area.Y + y - window.Y).Slice(area.X - window.X, area.Width);

    private static Mat Shaped(Mat scratch, Size size) =>
        new(size.Height, size.Width, scratch.Depth, scratch.NumberOfChannels, scratch.DataPointer, size.Width * scratch.ElementSize);

    private static (double Low, double High) Spread(Mat plane, Mat? mask) {
        (double low, double high, Point lowAt, Point highAt) = (0d, 0d, Point.Empty, Point.Empty);
        CvInvoke.MinMaxLoc(plane, ref low, ref high, ref lowAt, ref highAt, mask);
        return (low, high);
    }
}

file readonly struct TraceRows(CoordinateMap map, Mat points, Mat coverage, Rectangle area, PixelExtent output, PixelFrame source) : IAction {
    public void Invoke(int i) {
        Span<Vector2> row = points.GetSpan<Vector2>().Slice(i * area.Width, area.Width);
        Span<float> share = coverage.GetSpan<float>().Slice(i * area.Width, area.Width);
        map.Traced(row, share, new Point(area.X, area.Y + i), output, source.Extent);
        if (map.Wrap.Period.Case is Action<Span<Vector2>, Span<float>, PixelExtent> period) period(row, share, source.Extent);
        Vector2 shift = new Vector2(CoordinateMap.Pad - 0.5f) - new Vector2(source.Origin.X, source.Origin.Y);
        foreach (ref Vector2 point in row) point += shift;
    }
}

file readonly struct BakeRows(CoordinateMap map, Memory<Vector4> rows, PixelExtent extent) : IAction {
    public void Invoke(int i) {
        using SpanOwner<Vector2> points = SpanOwner<Vector2>.Allocate(extent.Width);
        using SpanOwner<float> coverage = SpanOwner<float>.Allocate(extent.Width);
        Span<Vector2> at = points.Span;
        Span<float> share = coverage.Span;
        map.Traced(at, share, new Point(0, i), extent, extent);
        if (map.Sampling.Snap.Case is Action<Span<Vector2>, Span<float>, PixelExtent> snap) snap(at, share, extent);
        map.Wrap.Fold(at, share, extent);
        Span<Vector4> row = rows.Span.Slice(i * extent.Width, extent.Width);
        for (int x = 0; x < row.Length; x++) row[x] = new Vector4(at[x].X / extent.Width, 1f - (at[x].Y / extent.Height), share[x], share[x]);
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class Presentations {
    public static Presentation<SignedAngle, float> Angle { get; } = new() { Unit = UnitsNet.Quantity.GetUnitInfo(AngleUnit.Radian), Origin = (float)SignedAngle.Neutral };
    public static Presentation<SignedAngle, float> HalfTurn { get; } = new() { Unit = UnitsNet.Quantity.GetUnitInfo(AngleUnit.Radian), Soft = (0f, float.Pi) };
    public static Presentation<ShortSideOffset, float> Shift { get; } = new() { Unit = UnitsNet.Quantity.GetUnitInfo(RatioUnit.DecimalFraction), Soft = (-0.1f, 0.1f), Origin = (float)ShortSideOffset.Neutral };
    public static Presentation<ShortSideOffset, float> Travel { get; } = new() { Unit = UnitsNet.Quantity.GetUnitInfo(RatioUnit.DecimalFraction), Soft = (-1f, 1f), Origin = (float)ShortSideOffset.Neutral };
    public static Presentation<ShortSideLength, float> Radius { get; } = new() { Unit = UnitsNet.Quantity.GetUnitInfo(RatioUnit.DecimalFraction), Soft = (0f, 1f) };
    public static Presentation<TransformScale, float> Scale { get; } = new() { Soft = (1f / 16f, 16f), Scale = TrackScale.Log, Origin = (float)TransformScale.Identity };
    public static Presentation<Frequency, float> Rate { get; } = Frequency.Presentation with { Soft = (-2f, 2f) };
    public static Presentation<ShutterSamples, int> Samples { get; } = new() { Step = 1, Soft = (1, 8) };
}
