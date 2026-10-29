using System.Numerics;
using System.Numerics.Tensors;
using System.Runtime.InteropServices;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Util;
using Rasm.Imaging.Color;

namespace Rasm.Imaging.Pixels;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record LevelRange(float Low, float High) {
    public bool Clips(Vector4 pixel) => float.Min(pixel.X, float.Min(pixel.Y, pixel.Z)) < Low || float.Max(pixel.X, float.Max(pixel.Y, pixel.Z)) >= High;

    public Fin<(ImmutableArray<int> Below, ImmutableArray<int> Above)> Clipped(PixelFrame frame) =>
        Try.lift(() => {
            using Mat header = frame.Header();
            using Mat channel = new();
            using Mat covered = new();
            using Mat hit = new();
            CvInvoke.ExtractChannel(header, channel, 3);
            using ScalarArray zero = new(0d);
            CvInvoke.Compare(channel, zero, covered, CmpType.GreaterThan);
            return (Counted(Low, CmpType.LessThan), Counted(High, CmpType.GreaterEqual));

            ImmutableArray<int> Counted(float level, CmpType comparison) =>
                [.. toSeq(Range(0, 3)).Map(index => {
                    CvInvoke.ExtractChannel(header, channel, index);
                    using ScalarArray threshold = new(level);
                    CvInvoke.Compare(channel, threshold, hit, comparison);
                    CvInvoke.BitwiseAnd(hit, covered, hit, mask: null);
                    return CvInvoke.CountNonZero(hit);
                })];
        }).Run();
}

[SmartEnum]
public sealed partial class TracePlane {
    public static readonly TracePlane Red = new(0, Vector3.UnitX);
    public static readonly TracePlane Green = new(1, Vector3.UnitY);
    public static readonly TracePlane Blue = new(2, Vector3.UnitZ);
    public static readonly TracePlane Luma = new(3, Vector3.One);

    public int Channel { get; }
    public Vector3 Color { get; }

    public ReadOnlyMemory2D<float> Plane(ReadOnlyMemory2D<float> reading) =>
        (reading.Height / Items.Count) switch { var height => reading.Slice(Channel * height, 0, height, reading.Width) };
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Scope {
    public virtual Fin<ReadOnlyMemory2D<float>> Read(PixelFrame frame) =>
        Try.lift(() => {
            using Mat header = frame.Header();
            using Mat rgb = new();
            using Mat alpha = new();
            using Mat covered = new();
            CvInvoke.CvtColor(header, rgb, ColorConversion.Bgra2Bgr);
            CvInvoke.ExtractChannel(header, alpha, 3);
            using ScalarArray zero = new(0d);
            CvInvoke.Compare(alpha, zero, covered, CmpType.GreaterThan);
            return Counted(rgb, covered);
        }).Run();

    private protected abstract ReadOnlyMemory2D<float> Counted(Mat rgb, Mat covered);

    private static Matrix4x4 Levels(Vector3 luminance) => new(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, luminance.X, luminance.Y, luminance.Z, 0f);

    private static Mat Rows(Matrix4x4 rows, int first, int count) {
        Mat map = new(count, 4, DepthType.Cv32F, 1);
        map.SetTo(MemoryMarshal.CreateReadOnlySpan(ref rows.M11, 16).Slice(4 * first, 4 * count).ToArray());
        return map;
    }

    private static Mat Transformed(Mat rgb, Matrix4x4 rows, int first, int count) {
        using Mat map = Rows(rows, first, count);
        Mat transformed = new();
        CvInvoke.Transform(rgb, transformed, map);
        return transformed;
    }

    private static ReadOnlyMemory2D<float> Binned(VectorOfMat images, Mat covered, int[] sizes, float[] ranges, params ReadOnlySpan<int[]> planes) {
        int cells = sizes.Length is 1 ? sizes[0] : sizes[0] * sizes[1];
        float[] counts = new float[planes.Length * cells];
        for (int index = 0; index < planes.Length; index++) {
            using Mat histogram = new();
            CvInvoke.CalcHist(images, planes[index], covered, histogram, sizes, ranges, accumulate: false);
            histogram.GetSpan<float>().CopyTo(counts.AsSpan(index * cells));
        }
        return new(counts, planes.Length * cells / sizes[^1], sizes[^1]);
    }

    public sealed record Histogram(int BinsPerStop, int TopStop, Vector3 Luminance) : Scope {
        public int Bins => (BinsPerStop * (TopStop + 1)) + 1;

        public Seq<float> Ticks => toSeq(Range(1, 4)).Map(static quarter => quarter / 4f) + toSeq(Range(1, TopStop)).Map(static stop => float.Exp2(stop));

        public Option<int> Bin(float level) {
            Span<float> bin = [level];
            Mapped(bin, stackalloc float[1]);
            return float.IsNaN(bin[0]) ? None : (int)bin[0];
        }

        public float LowerBound(int bin) => bin < BinsPerStop ? (float)bin / BinsPerStop : float.Exp2((float)(bin - BinsPerStop) / BinsPerStop);

        public static Option<double> Cumulative(ReadOnlySpan<float> plane, int bin) =>
            TensorPrimitives.Sum(plane) is > 0f and var covered ? Some((double)TensorPrimitives.Sum(plane[..(bin + 1)]) / covered) : None;

        private protected override ReadOnlyMemory2D<float> Counted(Mat rgb, Mat covered) {
            using Mat levels = Transformed(rgb, Levels(Luminance), 0, 4);
            using SpanOwner<float> scratch = SpanOwner<float>.Allocate(levels.Rows * levels.Cols * 4);
            Mapped(levels.GetSpan<float>(), scratch.Span);
            CvInvoke.PatchNaNs(levels, -1d);
            using VectorOfMat images = new(levels);
            return Binned(images, covered, [Bins], [0f, Bins], [.. TracePlane.Items.Select(static plane => new[] { plane.Channel })]);
        }

        private void Mapped(Span<float> levels, Span<float> scratch) {
            TensorPrimitives.Max(levels, 1f, scratch);
            TensorPrimitives.Log2(scratch, scratch);
            TensorPrimitives.Min(levels, 1f, levels);
            TensorPrimitives.Add(levels, scratch, levels);
            TensorPrimitives.Multiply(levels, BinsPerStop, levels);
            TensorPrimitives.Max(levels, 0f, levels);
            TensorPrimitives.Min(levels, Bins - 1, levels);
        }
    }

    public sealed record Waveform(PixelExtent Plot, Vector3 Luminance) : Scope {
        private protected override ReadOnlyMemory2D<float> Counted(Mat rgb, Mat covered) {
            (int columns, int rows) = (int.Min(Plot.Width, rgb.Cols), Plot.Height);
            using Mat levels = Transformed(rgb, Levels(Luminance), 0, 4);
            Span<float> samples = levels.GetSpan<float>();
            TensorPrimitives.Multiply(samples, rows, samples);
            TensorPrimitives.Max(samples, 0f, samples);
            TensorPrimitives.Min(samples, float.BitDecrement(rows), samples);
            CvInvoke.PatchNaNs(levels, -1d);
            using Mat line = new(1, rgb.Cols, DepthType.Cv32F, 1);
            Span<float> across = line.GetSpan<float>();
            for (int x = 0; x < across.Length; x++)
                across[x] = (long)x * columns / rgb.Cols;
            using Mat positions = new();
            CvInvoke.Repeat(line, rgb.Rows, 1, positions);
            using VectorOfMat images = new(levels, positions);
            return Binned(images, covered, [rows, columns], [0f, rows, 0f, columns], [.. TracePlane.Items.Select(static plane => new[] { plane.Channel, 4 })]);
        }
    }

    public sealed record Vectorscope(PixelExtent Plot, Matrix4x4 ToYpbpr, float Magnification) : Scope {
        public float Reach => 0.5f / Magnification;

        private protected override ReadOnlyMemory2D<float> Counted(Mat rgb, Mat covered) {
            using Mat ycc = Transformed(rgb, Matrix4x4.Transpose(ToYpbpr), 0, 3);
            CvInvoke.PatchNaNs(ycc, -2d * Reach);
            using VectorOfMat images = new(ycc);
            return Binned(images, covered, [Plot.Height, Plot.Width], [-Reach, Reach, -Reach, Reach], [2, 1]);
        }
    }

    public sealed record Chromaticity(PixelExtent Plot, Matrix4x4 ToXyz, Transfer Encoding) : Scope {
        public override Fin<ReadOnlyMemory2D<float>> Read(PixelFrame frame) =>
            frame.Copy() switch { var decoded => new PixelPass.Point(Encoding.Decode.Apply).Run(decoded).Bind(_ => base.Read(decoded)) };

        private protected override ReadOnlyMemory2D<float> Counted(Mat rgb, Mat covered) {
            Matrix4x4 rows = Matrix4x4.Transpose(ToXyz) switch {
                var xyz => xyz with { M41 = xyz.M11 + xyz.M21 + xyz.M31, M42 = xyz.M12 + xyz.M22 + xyz.M32, M43 = xyz.M13 + xyz.M23 + xyz.M33, M44 = xyz.M14 + xyz.M24 + xyz.M34 },
            };
            using Mat map = Rows(rows, 0, 4);
            using Mat xy = new();
            using Mat sum = Transformed(rgb, rows, 3, 1);
            using Mat lit = new();
            CvInvoke.PerspectiveTransform(rgb, xy, map);
            using ScalarArray zero = new(0d);
            CvInvoke.Compare(sum, zero, lit, CmpType.GreaterThan);
            CvInvoke.BitwiseAnd(lit, covered, lit, mask: null);
            using VectorOfMat images = new(xy);
            return Binned(images, lit, [Plot.Height, Plot.Width], [0f, 1f, 0f, 1f], [1, 0]);
        }
    }
}

public sealed record FrameComparison(double Psnr, Option<double> Nrmse, PixelFrame Difference) {
    public static Fin<FrameComparison> Of(PixelFrame reference, PixelFrame test) =>
        ImagingFault.Unless(reference.Size == test.Size, ImagingReason.Mismatch, (reference.Size, test.Size)).Bind(_ => Try.lift(() => {
            using Mat expected = Color(reference);
            using Mat actual = Color(test);
            using Mat delta = new();
            PixelFrame difference = new(test.Origin, test.Size, test.Extent);
            using Mat written = difference.Header();
            CvInvoke.AbsDiff(expected, actual, delta);
            CvInvoke.CvtColor(delta, written, ColorConversion.Bgr2Bgra);
            double error = double.Pow(CvInvoke.Norm(expected, actual, NormType.L2, mask: null), 2d);
            double top = double.Max(1d, double.Max(Peak(expected), Peak(actual)));
            return new FrameComparison(
                10d * double.Log10(top * top * 3d * test.Size.Width * test.Size.Height / error),
                CvInvoke.Norm(expected, NormType.L2, mask: null) is > 0d ? Some(CvInvoke.Norm(actual, expected, NormType.RelativeL2, mask: null)) : None,
                difference);
        }).Run());

    private static Mat Color(PixelFrame frame) {
        using Mat header = frame.Header();
        Mat color = new();
        CvInvoke.CvtColor(header, color, ColorConversion.Bgra2Bgr);
        return color;
    }

    private static double Peak(Mat color) {
        using Mat flat = color.Reshape(1);
        CvInvoke.MinMaxIdx(flat, out _, out double most, minIdx: null, maxIdx: null, mask: null);
        return most;
    }
}
