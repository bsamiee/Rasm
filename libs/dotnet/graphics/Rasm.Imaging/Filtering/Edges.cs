using System.Drawing;
using System.Numerics;
using CommunityToolkit.HighPerformance.Buffers;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Rasm.Imaging.Pixels;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Convolution;

namespace Rasm.Imaging.Filtering;

// --- [TYPES] ---------------------------------------------------------------------------
public delegate void ContourMarks(Mat guide, Rectangle first, Rectangle second, Mat edges);

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ContourLine(Seq<(ContourMarks Marks, PixelFrame Guide)> Sources, float Alignment, Func<float, float> Width, Vector3 Color);

[SmartEnum<string>]
public sealed partial class EdgeOperator {
    public static readonly EdgeOperator Sobel = new("sobel", Seq(KnownEdgeDetectorKernels.Sobel.KernelX, KnownEdgeDetectorKernels.Sobel.KernelY), strongest: false);
    public static readonly EdgeOperator Scharr = new("scharr", Seq(KnownEdgeDetectorKernels.Scharr.KernelX, KnownEdgeDetectorKernels.Scharr.KernelY), strongest: false);
    public static readonly EdgeOperator Prewitt = new("prewitt", Seq(KnownEdgeDetectorKernels.Prewitt.KernelX, KnownEdgeDetectorKernels.Prewitt.KernelY), strongest: false);
    public static readonly EdgeOperator RobertsCross = new("roberts-cross", Seq(KnownEdgeDetectorKernels.RobertsCross.KernelX, KnownEdgeDetectorKernels.RobertsCross.KernelY), strongest: false);
    public static readonly EdgeOperator Kayyali = new("kayyali", Seq(KnownEdgeDetectorKernels.Kayyali.KernelX, KnownEdgeDetectorKernels.Kayyali.KernelY), strongest: false);
    public static readonly EdgeOperator Kirsch = new("kirsch", Compass(KnownEdgeDetectorKernels.Kirsch), strongest: true);
    public static readonly EdgeOperator Robinson = new("robinson", Compass(KnownEdgeDetectorKernels.Robinson), strongest: true);
    public static readonly EdgeOperator Laplacian3 = new("laplacian-3", Seq(KnownEdgeDetectorKernels.Laplacian3x3.KernelXY), strongest: false);
    public static readonly EdgeOperator Laplacian5 = new("laplacian-5", Seq(KnownEdgeDetectorKernels.Laplacian5x5.KernelXY), strongest: false);
    public static readonly EdgeOperator LaplacianOfGaussian = new("laplacian-of-gaussian", Seq(KnownEdgeDetectorKernels.LaplacianOfGaussian.KernelXY), strongest: false);
    public static readonly EdgeOperator Emboss = new("emboss", Seq(Relief(KnownEdgeDetectorKernels.Sobel.KernelY)), strongest: true);

    private readonly Seq<SixLabors.ImageSharp.DenseMatrix<float>> kernels;
    private readonly bool strongest;

    public Fin<Unit> Detected(PixelFrame frame, Action<Mat, Mat> source, double sigma, Func<Mat, Fin<Unit>> render) => frame.Filter(header => {
        using Mat plane = new();
        using Mat response = new();
        using Mat taps = new();
        using Mat each = new();
        source(header, plane);
        plane.Blur(plane, sigma);
        response.Create(plane.Rows, plane.Cols, plane.Depth, plane.NumberOfChannels);
        response.SetTo(default(MCvScalar));
        foreach (SixLabors.ImageSharp.DenseMatrix<float> kernel in kernels) {
            taps.Create(kernel.Rows, kernel.Columns, DepthType.Cv32F, 1);
            taps.SetTo(kernel.Data);
            CvInvoke.Filter2D(plane, each, taps, new Point(-1, -1), 0d, BorderType.Replicate);
            if (strongest)
                CvInvoke.Max(response, each, response);
            else
                CvInvoke.AccumulateSquare(each, response, mask: null);
        }
        if (!strongest)
            CvInvoke.Sqrt(response, response);
        if (response.NumberOfChannels is 2) {
            using Mat sums = Mat.Ones(1, 2, DepthType.Cv32F, 1);
            CvInvoke.Multiply(response, response, response);
            CvInvoke.Transform(response, plane, sums);
            CvInvoke.Sqrt(plane, response);
        }
        response.ConvertTo(response, DepthType.Cv32F, 1d / kernels.Fold(0f, static (gain, kernel) => float.Max(gain, kernel.Data.Sum(static weight => float.Max(weight, 0f)))));
        return render(response);
    });

    private static Seq<SixLabors.ImageSharp.DenseMatrix<float>> Compass(EdgeDetectorCompassKernel kernel) =>
        Seq(kernel.North, kernel.NorthWest, kernel.West, kernel.SouthWest, kernel.South, kernel.SouthEast, kernel.East, kernel.NorthEast);

    private static SixLabors.ImageSharp.DenseMatrix<float> Relief(SixLabors.ImageSharp.DenseMatrix<float> gradient) {
        SixLabors.ImageSharp.DenseMatrix<float> kernel = new(gradient.Columns, gradient.Rows, gradient.Span);
        kernel[gradient.Rows / 2, gradient.Columns / 2]++;
        return kernel;
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class EdgeSource {
    public static Action<Mat, Mat> Color { get; } = static (frame, plane) => CvInvoke.CvtColor(frame, plane, ColorConversion.Rgba2Rgb);
    public static Action<Mat, Mat> Hue { get; } = static (frame, plane) => {
        using Mat angle = new();
        using Mat radius = new(frame.Size, DepthType.Cv32F, 1);
        using Mat across = new();
        using Mat down = new();
        Hsv(frame, angle, 0);
        radius.SetTo(new MCvScalar(1d / double.Tau));
        CvInvoke.PolarToCart(radius, angle, across, down, angleInDegrees: true);
        using VectorOfMat parts = new(across, down);
        CvInvoke.Merge(parts, plane);
    };
    public static Action<Mat, Mat> Saturation { get; } = static (frame, plane) => Hsv(frame, plane, 1);
    public static Action<Mat, Mat> Value { get; } = static (frame, plane) => Hsv(frame, plane, 2);
    public static Action<Mat, Mat> Alpha { get; } = static (frame, plane) => CvInvoke.ExtractChannel(frame, plane, 3);

    public static Action<Mat, Mat> Luma(Vector3 luminance) => (frame, plane) => {
        using Mat weights = new(1, 4, DepthType.Cv32F, 1);
        weights.SetTo([luminance.X, luminance.Y, luminance.Z, 0f]);
        CvInvoke.Transform(frame, plane, weights);
    };

    private static void Hsv(Mat frame, Mat plane, int channel) {
        using Mat hsv = new();
        CvInvoke.CvtColor(frame, hsv, ColorConversion.Rgb2Hsv);
        CvInvoke.ExtractChannel(hsv, plane, channel);
    }
}

public static class Contours {
    public static Fin<Func<float, float>> Width(double pixels, Option<double> world, Option<Camera> camera, Option<(double Near, Func<float, float> Scale)> taper) =>
        world.Match(
            meters => meters is 0d || pixels is 0d
                ? Fin.Succ<Func<float, float>>(static _ => 0f)
                : from view in camera.ToFin(ImagingFault.Of(ImagingReason.Missing, camera))
                  from step in Footprint(view.Lens).ToFin(ImagingFault.Of(ImagingReason.Degenerate, view.Lens))
                  select (Func<float, float>)(depth => (float)double.Min(meters / (step.Origin + (depth * step.Direction)).Length(), pixels)),
            () => Fin.Succ<Func<float, float>>(_ => (float)pixels))
        .Map(width => taper.Match(near => depth => width(depth) * near.Scale((float)double.Clamp(near.Near / depth, 0d, 1d)), () => width));

    public static Fin<Unit> Magnitudes(PixelFrame frame, Mat edges) => frame.Filter(header => {
        CvInvoke.MixChannels(edges, header, [.. Enumerable.Range(0, 3).SelectMany(channel => new[] { channel % edges.NumberOfChannels, channel })]);
        return unit;
    });

    public static Fin<Unit> Lines(PixelFrame frame, Mat edges, double threshold, float width, Vector3 line, Vector3 under, float floor, BlendingMode mode, Option<IProgress<int>> progress = default) {
        using Mat channels = edges.Reshape(1, edges.Rows * edges.Cols);
        using Mat strongest = new();
        using Mat mask = new();
        using Mat reach = new();
        using ScalarArray cut = new(threshold);
        CvInvoke.Reduce(channels, strongest, ReduceDimension.SingleCol, ReduceType.ReduceMax);
        using Mat plane = strongest.Reshape(1, edges.Rows);
        CvInvoke.Compare(plane, cut, mask, CmpType.LessEqual);
        CvInvoke.DistanceTransform(mask, reach, labels: null, DistType.L2, 5, DistLabelType.Pixel);
        return new PixelPass.Positional((row, _, y) => {
            using SpanOwner<Vector4> layer = SpanOwner<Vector4>.Allocate(row.Length);
            using SpanOwner<float> cover = SpanOwner<float>.Allocate(row.Length);
            ReadOnlySpan<float> distances = reach.View<float>().GetRowSpan(y - frame.Origin.Y);
            for (int x = 0; x < row.Length; x++) {
                float coverage = float.Clamp((width / 2f) - distances[x], 0f, 1f);
                (layer.Span[x], cover.Span[x]) = (new Vector4(Vector3.Lerp(under, line, coverage), 1f), float.Max(coverage, floor));
            }
            mode.Composite(row, layer.Span, cover.Span);
        }).Run(frame, progress);
    }

    public static Fin<Unit> Outlined(PixelFrame frame, PixelFrame depth, Seq<ContourLine> lines, Option<Vector4> paper, BlendingMode mode, Option<IProgress<int>> progress = default) =>
        frame.Filter(header => {
            using Mat range = depth.Header();
            using Mat z = new();
            using Mat unclaimed = new(range.Size, DepthType.Cv8U, 1);
            unclaimed.SetTo(new MCvScalar(byte.MaxValue));
            CvInvoke.ExtractChannel(range, z, 0);
            (Vector3 Color, float[] Cover)[] strokes = [.. lines.Filter(static line => !line.Sources.IsEmpty).Map(line => Claimed(line, z, unclaimed))];
            float[] opaque = GC.AllocateUninitializedArray<float>(frame.Size.Width);
            opaque.AsSpan().Fill(1f);
            if (paper.ToSpan() is [var ground])
                header.SetTo(new MCvScalar(ground.X, ground.Y, ground.Z, ground.W));
            return new PixelPass.Positional((row, _, y) => {
                using SpanOwner<Vector4> layer = SpanOwner<Vector4>.Allocate(row.Length, AllocationMode.Clear);
                foreach ((Vector3 color, float[] cover) in strokes) {
                    ReadOnlySpan<float> covered = cover.AsSpan((y - frame.Origin.Y) * row.Length, row.Length);
                    for (int x = 0; x < row.Length; x++)
                        layer.Span[x] += (1f - layer.Span[x].W) * covered[x] * new Vector4(color, 1f);
                }
                PixelFrame.Unassociated(layer.Span);
                mode.Composite(row, layer.Span, opaque);
            }).Run(frame, progress);
        });

    public static ContourMarks Silhouette(Projection lens, double step) =>
        (guide, first, second, edges) => {
            using Mat z = new();
            using Mat curvature = new();
            using Mat marks = new();
            CvInvoke.ExtractChannel(guide, z, 0);
            z.ConvertTo(z, DepthType.Cv64F);
            if (lens is Projection.Orthographic) {
                using ScalarArray infinity = new(double.PositiveInfinity);
                CvInvoke.Compare(z, infinity, marks, CmpType.Equal);
                z.SetTo(default(MCvScalar), marks);
            } else
                CvInvoke.Pow(z, -1d, z);
            CvInvoke.Sobel(z, curvature, DepthType.Cv64F, 2 * (second.X - first.X), 2 * (second.Y - first.Y), 1, 1d, 0d, BorderType.Replicate);
            CvInvoke.Multiply(curvature, curvature, curvature);
            CvInvoke.Multiply(z, z, z, step * step);
            CvInvoke.Compare(curvature, z, marks, CmpType.GreaterThan);
            using Mat leading = new(marks, first);
            using Mat trailing = new(marks, second);
            CvInvoke.BitwiseAnd(leading, trailing, edges);
        };

    public static ContourMarks Crease(double angle) =>
        (guide, firstBox, secondBox, edges) => {
            using Mat first = new(guide, firstBox);
            using Mat second = new(guide, secondBox);
            using Mat rgb = new(1, 4, DepthType.Cv32F, 1);
            using Mat dot = new();
            using Mat lengths = new();
            using Mat partner = new();
            using Mat product = new();
            rgb.SetTo([1f, 1f, 1f, 0f]);
            foreach ((Mat left, Mat right, Mat sum) in Seq((first, second, dot), (first, first, lengths), (second, second, partner))) {
                CvInvoke.Multiply(left, right, product);
                CvInvoke.Transform(product, sum, rgb);
            }
            CvInvoke.Multiply(lengths, partner, lengths);
            CvInvoke.Sqrt(lengths, lengths);
            lengths.ConvertTo(lengths, DepthType.Cv32F, double.Cos(angle));
            CvInvoke.Compare(dot, lengths, edges, CmpType.LessThan);
        };

    public static void Identity(Mat guide, Rectangle firstBox, Rectangle secondBox, Mat edges) {
        using Mat first = new(guide, firstBox);
        using Mat second = new(guide, secondBox);
        using Mat leading = new();
        using Mat trailing = new();
        CvInvoke.ExtractChannel(first, leading, 0);
        CvInvoke.ExtractChannel(second, trailing, 0);
        CvInvoke.Compare(leading, trailing, edges, CmpType.NotEqual);
    }

    private static Option<(Vector3 Origin, Vector3 Direction)> Footprint(Projection lens) =>
        lens.Extent.Point(new Vector2(0.5f)) switch {
            var center =>
                from here in lens.Point(center, 0f)
                from ahead in lens.Point(center, 1f)
                from next in lens.Point(center + Vector2.UnitX, 0f)
                from beyond in lens.Point(center + Vector2.UnitX, 1f)
                select (next - here, beyond - next - (ahead - here)),
        };

    private static (Vector3 Color, float[] Cover) Claimed(ContourLine line, Mat z, Mat unclaimed) {
        using Mat near = Mat.Zeros(z.Rows, z.Cols, DepthType.Cv8U, 1);
        using Mat far = Mat.Zeros(z.Rows, z.Cols, DepthType.Cv8U, 1);
        using Mat contour = new(z.Size, DepthType.Cv32F, 1);
        contour.SetTo(new MCvScalar(float.PositiveInfinity));
        foreach ((ContourMarks marks, PixelFrame guide) in line.Sources)
            Marked(marks, guide, z, near, far, contour);
        CvInvoke.BitwiseAnd(near, unclaimed, near);
        CvInvoke.BitwiseAnd(far, unclaimed, far);
        unclaimed.SetTo(default(MCvScalar), near);
        unclaimed.SetTo(default(MCvScalar), far);
        ((float[] Distance, int[] Label, float[] Width) inner, (float[] Distance, int[] Label, float[] Width) outer) = (Reached(near, contour, line.Width), Reached(far, contour, line.Width));
        float[] cover = GC.AllocateUninitializedArray<float>(inner.Distance.Length);
        for (int pixel = 0; pixel < cover.Length; pixel++)
            cover[pixel] = float.Clamp(inner.Distance[pixel] <= outer.Distance[pixel]
                ? (inner.Width[inner.Label[pixel]] * (1f - line.Alignment) / 2f) - inner.Distance[pixel]
                : (outer.Width[outer.Label[pixel]] * (1f + line.Alignment) / 2f) - outer.Distance[pixel], 0f, 1f);
        return (line.Color, cover);
    }

    private static void Marked(ContourMarks mark, PixelFrame guide, Mat z, Mat near, Mat far, Mat contour) {
        using Mat header = guide.Header();
        using Mat edges = new();
        using Mat side = new();
        using Mat nearer = new();
        using Mat lowered = new();
        foreach ((Rectangle first, Rectangle second) in Seq(
                     (new Rectangle(0, 0, z.Cols - 1, z.Rows), new Rectangle(1, 0, z.Cols - 1, z.Rows)),
                     (new Rectangle(0, 0, z.Cols, z.Rows - 1), new Rectangle(0, 1, z.Cols, z.Rows - 1))).Filter(static pair => pair.Item1.Width > 0 && pair.Item1.Height > 0)) {
            using Mat firstDepth = new(z, first);
            using Mat secondDepth = new(z, second);
            mark(header, first, second, edges);
            CvInvoke.Min(firstDepth, secondDepth, nearer);
            foreach ((Rectangle box, Mat own, Mat partner) in Seq((first, firstDepth, secondDepth), (second, secondDepth, firstDepth))) {
                foreach ((CmpType test, Mat mask) in Seq((CmpType.LessThan, near), (CmpType.GreaterEqual, far))) {
                    using Mat marks = new(mask, box);
                    CvInvoke.Compare(own, partner, side, test);
                    CvInvoke.BitwiseAnd(edges, side, side);
                    CvInvoke.BitwiseOr(marks, side, marks);
                }
                using Mat depth = new(contour, box);
                CvInvoke.Min(depth, nearer, lowered);
                lowered.CopyTo(depth, edges);
            }
        }
    }

    private static (float[] Distance, int[] Label, float[] Width) Reached(Mat mask, Mat contour, Func<float, float> width) {
        using Mat reach = new();
        using Mat labels = new();
        (float[] distance, int[] label, float[] widths) = (new float[mask.Rows * mask.Cols], new int[mask.Rows * mask.Cols], new float[CvInvoke.CountNonZero(mask) + 1]);
        CvInvoke.BitwiseNot(mask, mask);
        CvInvoke.DistanceTransform(mask, reach, labels, DistType.L2, 5, DistLabelType.Pixel);
        reach.CopyTo(distance);
        labels.CopyTo(label);
        ReadOnlySpan<byte> marks = mask.GetSpan<byte>();
        ReadOnlySpan<float> depths = contour.GetSpan<float>();
        for (int pixel = 0, next = 1; pixel < marks.Length; pixel++) {
            if (marks[pixel] is 0)
                widths[next++] = width(depths[pixel]);
        }
        return (distance, label, widths);
    }
}
