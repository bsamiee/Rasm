using System.Drawing;
using System.Numerics;
using System.Runtime.Intrinsics;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using Emgu.CV;
using Emgu.CV.CvEnum;
using MathNet.Numerics.Integration;
using Rasm.Imaging.Pixels;

namespace Rasm.Imaging.Warping;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class Sampling {
    public static readonly Sampling Nearest = new("nearest", Inter.Nearest, 0, static coordinate => double.Floor(coordinate) + 0.5d);
    public static readonly Sampling Linear = new("linear", Inter.Linear, 1, static coordinate => coordinate);
    public static readonly Sampling Cubic = new("cubic", Inter.Cubic, 2, static coordinate => coordinate);

    internal Inter Interpolation { get; }
    internal int Reach { get; }

    public double Point(double coordinate, WrapMode wrap, int size) => Snapped(wrap.Tiles ? wrap.Fold(coordinate, size, 0d).Point : coordinate);

    [UseDelegateFromConstructor]
    internal partial double Snapped(double coordinate);
}

public sealed record MapPath(Func<double, Vector4> Gains, Func<(double X, double Y), double, PixelExtent, ((double X, double Y) Point, float Coverage)> At);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CoordinateMap {
    private CoordinateMap(Sampling sampling, (WrapMode Across, WrapMode Down) wrap) => (Sampling, Wrap) = (sampling, wrap);

    public Sampling Sampling { get; }
    public (WrapMode Across, WrapMode Down) Wrap { get; }

    public Fin<Unit> Apply(PixelFrame frame, IProgress<int> progress) => Run(frame, progress, [(this, Vector4.One)]);

    public PixelFrame Resample(PixelFrame source, PixelExtent extent) {
        PixelFrame resampled = new(Point.Empty, extent, extent);
        Sample(source, Point.Empty, extent, extent, [(this, Vector4.One)], resampled.View.Span);
        return resampled;
    }

    public PixelFrame Bake(PixelExtent extent) {
        PixelFrame baked = new(Point.Empty, extent, extent);
        Span2D<Vector4> pixels = baked.View.Span;
        using SpanOwner<(double X, double Y)> points = SpanOwner<(double X, double Y)>.Allocate(extent.Width);
        using SpanOwner<float> coverage = SpanOwner<float>.Allocate(extent.Width);
        for (int row = 0; row < extent.Height; row++) {
            Traced(row, Point.Empty, extent, extent, points.Span, coverage.Span);
            for (int column = 0; column < extent.Width; column++)
                pixels[row, column] = new((float)(points.Span[column].X / extent.Width), (float)(1d - (points.Span[column].Y / extent.Height)), coverage.Span[column], coverage.Span[column]);
        }
        return baked;
    }

    public static Func<PixelFrame, IProgress<int>, Fin<Unit>> Integrated(GaussLegendreRule quadrature, Func<double, (CoordinateMap Map, Vector4 Gains)> subframe, params Func<double, (CoordinateMap Map, Vector4 Gains)>[] additional) =>
        (frame, progress) => Run(frame, progress, toSeq(
            from path in additional.Prepend(subframe)
            from step in Enumerable.Range(0, quadrature.Order)
            let sample = path(quadrature.GetAbscissa(step))
            select (sample.Map, sample.Gains * (float)quadrature.GetWeight(step))));

    public static Func<PixelFrame, IProgress<int>, Fin<Unit>> IntegratedLinear(Func<Func<double, Complex>, Complex> integral, WrapMode wrap, MapPath path, params MapPath[] additional) =>
        (frame, progress) => {
            PixelFrame source = frame.Copy();
            MapPath[] paths = [path, .. additional];
            Vector256<double> total = paths.Aggregate(Vector256<double>.Zero, (sum, sample) => sum + Integrated(t => sample.Gains(t).Wide));
            return new PixelPass.Pointwise((row, column, line) => {
                for (int x = 0; x < row.Length; x++) {
                    (double X, double Y) point = (column + x + 0.5d, frame.Extent.Height - line - 0.5d);
                    Vector256<double> associated = Vector256<double>.Zero;
                    foreach (MapPath sample in paths) {
                        associated += Integrated(t => {
                            ((double X, double Y) at, float coverage) = sample.At(point, t, frame.Extent);
                            Vector256<double> weights = sample.Gains(t).Wide * coverage;
                            return Vector256.ConditionalSelect(Vector256.IsZero(weights), Vector256<double>.Zero, weights * source.Sample(at, (wrap, wrap), Vector256<double>.Zero, Associated));
                        });
                    }
                    row[x] = (associated / total).Narrowed.AsVector4().Unassociated;
                }
            }).Run(frame, progress);

            Vector256<double> Integrated(Func<double, Vector256<double>> function) =>
                (integral(t => function(t) switch { var value => new Complex(value.GetElement(0), value.GetElement(1)) }),
                 integral(t => function(t) switch { var value => new Complex(value.GetElement(2), value.GetElement(3)) })) switch {
                     var (rg, ba) => Vector256.Create(rg.Real, rg.Imaginary, ba.Real, ba.Imaginary),
                 };
        };

    internal abstract void Trace(Span<(double X, double Y)> points, Span<float> coverage, PixelExtent output, PixelExtent source);

    internal void Bound(Span<(double X, double Y)> points, Span<float> coverage, PixelExtent extent) {
        Prepare(points, extent);
        for (int i = 0; i < points.Length; i++) {
            ((double x, double across), (double y, double down)) = (Wrap.Across.Fold(points[i].X, extent.Width, 0d), Wrap.Down.Fold(points[i].Y, extent.Height, 0d));
            points[i] = (x, y);
            coverage[i] *= (float)(across * down);
        }
    }

    private void Prepare(Span<(double X, double Y)> points, PixelExtent extent) {
        for (int i = 0; i < points.Length; i++) points[i] = (Sampling.Point(points[i].X, Wrap.Across, extent.Width), Sampling.Point(points[i].Y, Wrap.Down, extent.Height));
    }

    private void Traced(int row, Point origin, PixelExtent output, PixelExtent source, Span<(double X, double Y)> points, Span<float> coverage) {
        for (int column = 0; column < points.Length; column++) points[column] = ((double)origin.X + column + 0.5d, (double)origin.Y + row + 0.5d);
        coverage.Fill(1f);
        Trace(points, coverage, output, source);
        Bound(points, coverage, source);
    }

    private static Vector256<double> Associated(Vector256<double> sum, Vector4 pixel, double weight) =>
        sum + (weight * Vector256.Create((double)pixel.X * pixel.W, (double)pixel.Y * pixel.W, (double)pixel.Z * pixel.W, pixel.W));

    private static Fin<Unit> Run(PixelFrame frame, IProgress<int> progress, Seq<(CoordinateMap Map, Vector4 Gains)> subframes) =>
        frame.Filter(_ => {
            Sample(frame, frame.Origin, frame.Size, frame.Extent, subframes, frame.View.Span);
            progress.Report(frame.Size.Height);
            return unit;
        });

    private static void Sample(PixelFrame original, Point origin, PixelExtent size, PixelExtent output, Seq<(CoordinateMap Map, Vector4 Gains)> subframes, Span2D<Vector4> destination) {
        Vector4 total = subframes.Fold(Vector4.Zero, static (sum, subframe) => sum + subframe.Gains);
        using Mat source = original.Premultiplied();
        using Mat wide = new();
        using Mat padded = new();
        using Mat across = new(size.Height, size.Width, DepthType.Cv32F, 1);
        using Mat down = new(size.Height, size.Width, DepthType.Cv32F, 1);
        using Mat weights = new(size.Height, size.Width, DepthType.Cv32F, 4);
        using Mat sampled = new();
        using Mat sum = Mat.Zeros(size.Height, size.Width, DepthType.Cv32F, 4);
        Span2D<float> columns = across.View<float>();
        Span2D<float> rows = down.View<float>();
        Span2D<Vector4> shares = weights.View<Vector4>();
        using SpanOwner<(double X, double Y)> points = SpanOwner<(double X, double Y)>.Allocate(size.Width);
        using SpanOwner<float> coverage = SpanOwner<float>.Allocate(size.Width);
        foreach ((CoordinateMap map, Vector4 gains) in subframes) {
            int pad = map.Sampling.Reach + 1;
            (double left, double top) = (pad - original.Origin.X - 0.5d, pad - original.Origin.Y - 0.5d);
            CvInvoke.CopyMakeBorder(source, wide, 0, 0, pad, pad, map.Wrap.Across.Border, default);
            CvInvoke.CopyMakeBorder(wide, padded, pad, pad, 0, 0, map.Wrap.Down.Border, default);
            for (int row = 0; row < size.Height; row++) {
                map.Traced(row, origin, output, original.Extent, points.Span, coverage.Span);
                for (int column = 0; column < size.Width; column++)
                    (columns[row, column], rows[row, column], shares[row, column]) = ((float)(points.Span[column].X + left), (float)(points.Span[column].Y + top), gains / total * coverage.Span[column]);
            }
            CvInvoke.Remap(padded, sampled, across, down, map.Sampling.Interpolation, BorderType.Constant, default);
            CvInvoke.Multiply(sampled, weights, sampled);
            CvInvoke.Add(sum, sampled, sum);
        }
        ReadOnlySpan2D<Vector4> associated = sum.View<Vector4>();
        for (int row = 0; row < size.Height; row++) {
            Span<Vector4> written = destination.GetRowSpan(row);
            for (int column = 0; column < size.Width; column++) written[column] = associated[row, column].Unassociated;
        }
    }

    public sealed record Analytic(Action<Span<(double X, double Y)>, Span<float>, PixelExtent> Kernel, Sampling Sampling, (WrapMode Across, WrapMode Down) Wrap) : CoordinateMap(Sampling, Wrap) {
        internal override void Trace(Span<(double X, double Y)> points, Span<float> coverage, PixelExtent output, PixelExtent source) => Kernel(points, coverage, source);
    }

    public sealed record Sampled(PixelFrame Coordinates, Sampling Sampling, (WrapMode Across, WrapMode Down) Wrap) : CoordinateMap(Sampling, Wrap) {
        internal override void Trace(Span<(double X, double Y)> points, Span<float> coverage, PixelExtent output, PixelExtent source) {
            (double X, double Y) = ((double)Coordinates.Extent.Width / output.Width, (double)Coordinates.Extent.Height / output.Height);
            for (int i = 0; i < points.Length; i++)
                (points[i], coverage[i]) = Coordinates.Sample((points[i].X * X, points[i].Y * Y), (WrapMode.Clamp, WrapMode.Clamp), Vector256<double>.Zero,
                    static (sum, pixel, weight) => sum + (weight * pixel.Wide)) switch {
                        var read => ((read.GetElement(0) * source.Width, (1d - read.GetElement(1)) * source.Height), coverage[i] * (float)read.GetElement(3)),
                    };
        }
    }

    public sealed record Composed(CoordinateMap First, CoordinateMap Second) : CoordinateMap(First.Sampling, First.Wrap) {
        internal override void Trace(Span<(double X, double Y)> points, Span<float> coverage, PixelExtent output, PixelExtent source) {
            Second.Trace(points, coverage, output, output);
            Second.Bound(points, coverage, output);
            First.Trace(points, coverage, output, source);
        }
    }
}
