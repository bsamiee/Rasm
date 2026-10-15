using System.Drawing;
using System.Numerics;
using System.Runtime.Intrinsics;
using CommunityToolkit.HighPerformance;
using Emgu.CV;
using Emgu.CV.CvEnum;
using MathNet.Numerics.Integration;
using PeterO.Numbers;
using Rasm.Imaging.Filters.Generators;
using Rasm.Imaging.Pixels;

namespace Rasm.Imaging.Filters.Warp;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>]
[ValidationError<InvalidWarp>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Sampling {
    public static readonly Sampling Nearest = new("nearest", Inter.Nearest, 0, Some<Action<Span<(double X, double Y)>>>(static points => { foreach (ref (double X, double Y) point in points) point = (Math.Floor(point.X) + 0.5d, Math.Floor(point.Y) + 0.5d); }));
    public static readonly Sampling Linear = new("linear", Inter.Linear, 1, None);
    public static readonly Sampling Cubic = new("cubic", Inter.Cubic, 2, None);

    internal Inter Interpolation { get; }
    internal int Reach { get; }
    internal Option<Action<Span<(double X, double Y)>>> Snap { get; }

    internal double Point(ERational coordinate, WrapMode wrap, int size) =>
        (wrap.Tiles ? wrap.Fold(coordinate, size).Point : coordinate) switch {
            var point when this == Nearest => ERational.FromEInteger(NoiseFunctions.Split(point).Cell).Add(ERational.FromDouble(0.5d)).ToDouble(),
            var point => point.ToDouble(),
        };
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CoordinateMap {
    private CoordinateMap(Sampling sampling, (WrapMode Across, WrapMode Down) wrap) => (Sampling, Wrap) = (sampling, wrap);

    public Sampling Sampling { get; }
    public (WrapMode Across, WrapMode Down) Wrap { get; }

    public Fin<Unit> Apply(PixelFrame frame, IProgress<int> progress) => Run(frame, progress, [(this, Vector256.Create(1d))]);

    public PixelFrame Resample(PixelFrame source, PixelExtent extent) => new(Point.Empty, extent, extent, block =>
        Sample(source, Point.Empty, extent, extent, [(this, Vector256.Create(1d))], block.AsSpan().Cast<float, Vector4>().AsSpan2D(extent.Height, extent.Width)));

    public PixelFrame Bake(PixelExtent extent) => new(Point.Empty, extent, extent, block => {
        Span2D<Vector4> pixels = block.AsSpan().Cast<float, Vector4>().AsSpan2D(extent.Height, extent.Width);
        (double X, double Y)[] points = new (double X, double Y)[extent.Width];
        float[] coverage = new float[extent.Width];
        for (int row = 0; row < extent.Height; row++) {
            for (int column = 0; column < points.Length; column++) points[column] = (column + 0.5d, row + 0.5d);
            coverage.AsSpan().Fill(1f);
            Trace(points, coverage, extent, extent);
            Bound(points, coverage, extent);
            for (int column = 0; column < points.Length; column++) pixels[row, column] = new((float)(points[column].X / extent.Width), (float)(1d - (points[column].Y / extent.Height)), coverage[column], coverage[column]);
        }
    });

    public static Func<PixelFrame, IProgress<int>, Fin<Unit>> Integrated(GaussLegendreRule quadrature, Func<double, (CoordinateMap Map, Vector4 Lanes)> subframe, params Func<double, (CoordinateMap Map, Vector4 Lanes)>[] additional) =>
        (frame, progress) => Run(frame, progress, toSeq(
            from path in additional.Prepend(subframe)
            from step in Enumerable.Range(0, quadrature.Order)
            let sample = path(quadrature.GetAbscissa(step))
            select (sample.Map, Vector256.Create(sample.Lanes.X, sample.Lanes.Y, sample.Lanes.Z, sample.Lanes.W) * quadrature.GetWeight(step))));

    public static Func<PixelFrame, IProgress<int>, Fin<Unit>> IntegratedLinear(
        Func<Func<double, Complex>, Complex> integral, WrapMode wrap,
        (Func<double, Vector4> Lanes, Func<(double X, double Y), double, PixelExtent, ((double X, double Y) Point, float Coverage)> At) path,
        params (Func<double, Vector4> Lanes, Func<(double X, double Y), double, PixelExtent, ((double X, double Y) Point, float Coverage)> At)[] additional) =>
        (frame, progress) => Try.lift(() => {
            PixelFrame source = frame.Copy();
            (Func<double, Vector4> Lanes, Func<(double X, double Y), double, PixelExtent, ((double X, double Y) Point, float Coverage)> At)[] paths = [path, .. additional];
            Vector256<double> total = paths.Aggregate(Vector256<double>.Zero, (sum, sample) => sum + Integrated(t => Lanes(sample.Lanes(t))));
            _ = Parallel.For(0, frame.Size.Height, row => {
                Span<Vector4> destination = frame.View.Span.GetRowSpan(row);
                for (int column = 0; column < destination.Length; column++) {
                    (double X, double Y) point = ((double)frame.Origin.X + column + 0.5d, (double)frame.Origin.Y + row + 0.5d);
                    Vector256<double> associated = Vector256<double>.Zero;
                    foreach ((Func<double, Vector4> Lanes, Func<(double X, double Y), double, PixelExtent, ((double X, double Y) Point, float Coverage)> At) sample in paths) {
                        associated += Integrated(t => {
                            ((double X, double Y) at, float coverage) = sample.At(point, t, frame.Extent);
                            Vector256<double> weights = Lanes(sample.Lanes(t)) * coverage;
                            Vector256<double> read = source.Sample(at, (wrap, wrap), Vector256<double>.Zero,
                                static (sum, pixel, weight) => sum + (weight * Vector256.Create((double)pixel.X * pixel.W, (double)pixel.Y * pixel.W, (double)pixel.Z * pixel.W, pixel.W)));
                            return Vector256.ConditionalSelect(Vector256.IsZero(weights), Vector256<double>.Zero, weights * read);
                        });
                    }
                    Vector256<double> pixel = associated / total;
                    double alpha = pixel.GetElement(3);
                    destination[column] = alpha > 0d ? new Vector4((float)(pixel.GetElement(0) / alpha), (float)(pixel.GetElement(1) / alpha), (float)(pixel.GetElement(2) / alpha), (float)alpha) : Vector4.Zero;
                }
            });
            progress.Report(frame.Size.Height);
            return unit;

            Vector256<double> Integrated(Func<double, Vector256<double>> function) =>
                (integral(t => function(t) switch { var value => new Complex(value.GetElement(0), value.GetElement(1)) }),
                 integral(t => function(t) switch { var value => new Complex(value.GetElement(2), value.GetElement(3)) })) switch {
                     var (rg, ba) => Vector256.Create(rg.Real, rg.Imaginary, ba.Real, ba.Imaginary),
                 };

            static Vector256<double> Lanes(Vector4 value) => Vector256.Create(value.X, value.Y, value.Z, value.W);
        }).Run();

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
        foreach (ref (double X, double Y) point in points) point = (
            Wrap.Across.Tiles ? Wrap.Across.Fold(point.X, extent.Width, 0d).Point : point.X,
            Wrap.Down.Tiles ? Wrap.Down.Fold(point.Y, extent.Height, 0d).Point : point.Y);
        if (Sampling.Snap.Case is Action<Span<(double X, double Y)>> snap) snap(points);
    }

    private static Fin<Unit> Run(PixelFrame frame, IProgress<int> progress, Seq<(CoordinateMap Map, Vector256<double> Lanes)> subframes) =>
        Try.lift(() => {
            Sample(frame.Copy(), frame.Origin, frame.Size, frame.Extent, subframes, frame.View.Span);
            progress.Report(frame.Size.Height);
            return unit;
        }).Run();

    private static unsafe void Sample(PixelFrame original, Point origin, PixelExtent size, PixelExtent output, Seq<(CoordinateMap Map, Vector256<double> Lanes)> subframes, Span2D<Vector4> destination) {
        Vector256<double> total = subframes.Fold(Vector256<double>.Zero, static (sum, subframe) => sum + subframe.Lanes);
        Vector256<double>[] associated = new Vector256<double>[size.Width];
        for (int row = 0; row < size.Height; row++) {
            associated.AsSpan().Clear();
            foreach ((CoordinateMap map, Vector256<double> lanes) in subframes) {
                int stride = map.Sampling == Sampling.Cubic ? 1 : 1024;
                for (int column = 0; column < size.Width; column += stride)
                    Accumulate(map, lanes / total, row, column, int.Min(stride, size.Width - column), associated);
            }
            Span<Vector4> written = destination.GetRowSpan(row);
            for (int column = 0; column < size.Width; column++) {
                Vector256<double> pixel = associated[column];
                double alpha = pixel.GetElement(3);
                written[column] = alpha > 0d ? new Vector4((float)(pixel.GetElement(0) / alpha), (float)(pixel.GetElement(1) / alpha), (float)(pixel.GetElement(2) / alpha), (float)alpha) : Vector4.Zero;
            }
        }

        void Accumulate(CoordinateMap map, Vector256<double> lanes, int row, int column, int count, Span<Vector256<double>> destination) {
            int reach = map.Sampling.Reach;
            int support = (2 * reach) + 1;
            (double X, double Y)[] traced = new (double X, double Y)[count];
            float[] coverage = new float[count];
            for (int i = 0; i < count; i++) traced[i] = ((double)origin.X + column + i + 0.5d, (double)origin.Y + row + 0.5d);
            coverage.AsSpan().Fill(1f);
            map.Trace(traced, coverage, output, original.Extent);
            map.Prepare(traced, original.Extent);
            using Mat patches = new(support, support * count, DepthType.Cv64F, 4);
            using Mat points = new(1, count, DepthType.Cv32F, 2);
            using Mat sampled = new();
            using Mat unused = new();
            Span<Vector2> at = new((void*)points.DataPointer, count);
            for (int i = 0; i < count; i++) {
                (double x, double y) = (Math.Floor(traced[i].X - 0.5d), Math.Floor(traced[i].Y - 0.5d));
                int inset = int.Max(0, reach - 1);
                at[i] = new((float)(inset + (traced[i].X - 0.5d - x)), (float)(inset + (traced[i].Y - 0.5d - y)));
                Gather(i, x - inset, y - inset);
            }
            if (map.Sampling == Sampling.Cubic) CvInvoke.Resize(patches, sampled, Size.Empty, 0.5d / (at[0].X + 0.5d), 0.5d / (at[0].Y + 0.5d), map.Sampling.Interpolation);
            else {
                using Mat cells = new();
                using Mat fractions = new();
                CvInvoke.ConvertMaps(points, unused, cells, fractions, DepthType.Cv16S, 2, map.Sampling == Sampling.Nearest);
                Span<short> locations = new((void*)cells.DataPointer, 2 * count);
                for (int i = 0; i < count; i++) locations[2 * i] += short.CreateChecked(i * support);
                CvInvoke.Remap(patches, sampled, cells, fractions, map.Sampling.Interpolation, BorderType.Constant);
            }
            ReadOnlySpan<Vector256<double>> read = new((void*)sampled.DataPointer, count);
            for (int i = 0; i < count; i++) {
                Vector256<double> weights = lanes * coverage[i];
                destination[column + i] += Vector256.ConditionalSelect(Vector256.IsZero(weights), Vector256<double>.Zero, weights * read[i]);
            }

            void Gather(int patch, double left, double top) {
                Span<Vector256<double>> pixels = new((void*)patches.DataPointer, support * support * count);
                for (int y = 0; y < support; y++)
                    for (int x = 0; x < support; x++)
                        pixels[(y * support * count) + (patch * support) + x] = original.Sample((left + x + 0.5d, top + y + 0.5d), map.Wrap, Vector256<double>.Zero,
                            static (sum, pixel, weight) => sum + (weight * Vector256.Create((double)pixel.X * pixel.W, (double)pixel.Y * pixel.W, (double)pixel.Z * pixel.W, pixel.W)));
            }
        }
    }

    public sealed record Analytic(Action<Span<(double X, double Y)>, Span<float>, PixelExtent> Kernel, Sampling Sampling, (WrapMode Across, WrapMode Down) Wrap) : CoordinateMap(Sampling, Wrap) {
        internal override void Trace(Span<(double X, double Y)> points, Span<float> coverage, PixelExtent output, PixelExtent source) => Kernel(points, coverage, source);
    }

    public sealed record Sampled(PixelFrame St, Sampling Sampling, (WrapMode Across, WrapMode Down) Wrap) : CoordinateMap(Sampling, Wrap) {
        internal override void Trace(Span<(double X, double Y)> points, Span<float> coverage, PixelExtent output, PixelExtent source) {
            (double X, double Y) = ((double)St.Extent.Width / output.Width, (double)St.Extent.Height / output.Height);
            for (int i = 0; i < points.Length; i++)
                (points[i], coverage[i]) = St.Sample((points[i].X * X, points[i].Y * Y), (WrapMode.Clamp, WrapMode.Clamp), Vector256<double>.Zero,
                    static (sum, pixel, weight) => sum + (weight * Vector256.Create(pixel.X, pixel.Y, pixel.Z, pixel.W))) switch {
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
