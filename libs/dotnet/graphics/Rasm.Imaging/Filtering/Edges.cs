using System.Drawing;
using System.Numerics;
using System.Runtime.Intrinsics;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Rasm.Imaging.Color;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Procedural;
using Rasm.Imaging.Tone;
using SixLabors.ImageSharp.PixelFormats;

namespace Rasm.Imaging.Filtering;

// --- [TYPES] ---------------------------------------------------------------------------
public delegate Fin<Unit> ContourMarks(Mat guide, Rectangle first, Rectangle second, Mat edges);

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class StrokePlacement {
    public static readonly StrokePlacement Center = new("center", static (near, nearWidth, far, farWidth) =>
        near <= far ? ShapeEdge.Strip(near + 0.5f, nearWidth, 1f) : ShapeEdge.Strip(far + 0.5f, farWidth, 1f));
    public static readonly StrokePlacement Inside = new("inside", static (near, nearWidth, far, _) =>
        (float)double.Clamp(nearWidth - near, 0d, 1d) * ShapeEdge.Coverage((near - far) / 2d, 1d));
    public static readonly StrokePlacement Outside = new("outside", static (near, _, far, farWidth) =>
        (float)double.Clamp(farWidth - far, 0d, 1d) * ShapeEdge.Coverage((far - near) / 2d, 1d));

    [UseDelegateFromConstructor]
    public partial float Cover(double near, double nearWidth, double far, double farWidth);
}

public sealed record ContourLine(Seq<(ContourMarks Marks, PixelFrame Guide)> Sources, StrokePlacement Placement, Func<float, double> Width, Vector3 Color);

[SmartEnum<string>]
public abstract partial class EdgeOperator {
    public static readonly EdgeOperator Laplace = new Gradient("laplace", Seq(-1f / 8f, -1f / 8f, -1f / 8f, -1f / 8f, 1f, -1f / 8f, -1f / 8f, -1f / 8f, -1f / 8f));
    public static readonly EdgeOperator Sobel = new Gradient("sobel", Seq(1f, 0f, -1f, 2f, 0f, -2f, 1f, 0f, -1f));
    public static readonly EdgeOperator Prewitt = new Gradient("prewitt", Seq(1f, 0f, -1f, 1f, 0f, -1f, 1f, 0f, -1f));
    public static readonly EdgeOperator Kirsch = new Gradient("kirsch", Seq(5f, -3f, -2f, 5f, -3f, -2f, 5f, -3f, -2f));
    public static readonly EdgeOperator Emboss = new Relief("emboss", Seq(1f, 2f, 1f, 0f, 1f, 0f, -1f, -2f, -1f));

    public Seq<float> Kernel { get; }

    public abstract float Gain { get; }

    private protected Seq<(float X, float Y)> Steps =>
        from axis in Seq<Func<(int Row, int Column), int>>(static cell => cell.Row, static cell => cell.Column)
        from split in Seq(1, 2)
        from side in Seq<Func<int, int, bool>>(static (at, edge) => at < edge, static (at, edge) => at >= edge)
        let lit = Cells.Filter(cell => side(axis(cell), split))
        select (lit.Fold(0f, (sum, cell) => sum + Kernel[(3 * cell.Row) + cell.Column]), lit.Fold(0f, (sum, cell) => sum + Kernel[(3 * cell.Column) + cell.Row]));

    private static Seq<(int Row, int Column)> Cells { get; } = toSeq(from row in Enumerable.Range(0, 3) from column in Enumerable.Range(0, 3) select (row, column));

    public abstract Unit Respond(Mat plane, Mat response);

    private protected Mat Across() => Taps(static (row, column) => (3 * (2 - row)) + column);

    private protected Mat Down() => Taps(static (row, column) => (3 * column) + 2 - row);

    private Mat Taps(Func<int, int, int> cell) {
        Mat taps = new(3, 3, DepthType.Cv32F, 1);
        taps.SetTo([.. Cells.Map(at => Kernel[cell(at.Row, at.Column)])]);
        return taps;
    }

    private sealed class Gradient(string key, Seq<float> kernel) : EdgeOperator(key, kernel) {
        public override float Gain => Steps.Fold(0f, static (gain, step) => float.Max(gain, float.Hypot(step.X, step.Y)));

        public override Unit Respond(Mat plane, Mat response) {
            using Mat across = Across();
            using Mat down = Down();
            using Mat vertical = new(plane.Size, plane.Depth, plane.NumberOfChannels);
            CvInvoke.Filter2D(plane, response, across, new Point(-1, -1), 0d, BorderType.Replicate);
            CvInvoke.Filter2D(plane, vertical, down, new Point(-1, -1), 0d, BorderType.Replicate);
            CvInvoke.Multiply(response, response, response);
            CvInvoke.Multiply(vertical, vertical, vertical);
            CvInvoke.Add(response, vertical, response);
            CvInvoke.Sqrt(response, response);
            return unit;
        }
    }

    private sealed class Relief(string key, Seq<float> kernel) : EdgeOperator(key, kernel) {
        public override float Gain => Steps.Fold(0f, static (gain, step) => float.Max(gain, step.X));

        public override Unit Respond(Mat plane, Mat response) {
            using Mat across = Across();
            using ScalarArray floor = new(0d);
            CvInvoke.Filter2D(plane, response, across, new Point(-1, -1), 0d, BorderType.Replicate);
            CvInvoke.Max(response, floor, response);
            return unit;
        }
    }
}

[SmartEnum<string>]
public sealed partial class EdgeSource {
    public static readonly EdgeSource Color = new("color", 3, separate: true, static (pixel, levels, _) => (levels[0], levels[1], levels[2]) = (pixel.X, pixel.Y, pixel.Z));
    public static readonly EdgeSource Luma = new("luma", 1, separate: false, static (pixel, levels, weights) => levels[0] = LuminanceSamples.Luminance(pixel.X, pixel.Y, pixel.Z, weights));
    public static readonly EdgeSource Hue = new("hue", 2, separate: false, static (pixel, levels, _) =>
        (levels[0], levels[1]) = double.SinCos(Hsy.From(pixel).Hue) switch { var (sine, cosine) => (cosine / double.Tau, sine / double.Tau) });
    public static readonly EdgeSource Saturation = new("saturation", 1, separate: false, static (pixel, levels, _) => levels[0] = RangeAxis.Saturation.Coordinate(Hsy.From(pixel)));
    public static readonly EdgeSource Value = new("value", 1, separate: false, static (pixel, levels, _) => levels[0] = float.Max(float.Max(pixel.X, pixel.Y), pixel.Z));
    public static readonly EdgeSource Alpha = new("alpha", 1, separate: false, static (pixel, levels, _) => levels[0] = pixel.W);

    public int Planes { get; }
    public bool Separate { get; }

    [UseDelegateFromConstructor]
    public partial void Read(Vector4 pixel, Span<double> levels, (double R, double G, double B) weights);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class EdgeResponse {
    public static Fin<Unit> Detected(PixelFrame frame, EdgeOperator kernel, EdgeSource source, double softening, Gamut gamut, Func<Mat, Fin<Unit>> render) => frame.Filter(_ => {
        (int width, int planes, (double R, double G, double B) luminance) = (frame.Size.Width, source.Planes, gamut.Luminance);
        using Mat plane = new(frame.Size.Height, width, DepthType.Cv64F, planes);
        using Mat response = new(frame.Size.Height, width, DepthType.Cv64F, planes);
        using Mat squares = new();
        return new PixelPass.Pointwise((row, _, line) => {
            Span2D<double> levels = plane.View<double>().GetRowSpan(frame.RowIndex(line)).AsSpan2D(width, planes);
            for (int x = 0; x < row.Length; x++)
                source.Read(row[x], levels.GetRowSpan(x), luminance);
        }).Run(frame, new Progress<int>())
            .Bind(_ => plane.Blur(plane, softening))
            .Map(_ => kernel.Respond(plane, response))
            .Bind(_ => {
                if (!source.Separate) {
                    using Mat sums = Mat.Ones(1, planes, DepthType.Cv32F, 1);
                    CvInvoke.Multiply(response, response, response);
                    CvInvoke.Transform(response, squares, sums);
                    CvInvoke.Sqrt(squares, response);
                }
                return render(response);
            });
    });

    public static Fin<Unit> Magnitudes(PixelFrame frame, Mat edges, IProgress<int> progress) =>
        new PixelPass.Pointwise((row, _, line) => {
            int channels = edges.NumberOfChannels;
            ReadOnlySpan2D<double> responses = edges.View<double>().GetRowSpan(frame.RowIndex(line)).AsSpan2D(row.Length, channels);
            for (int x = 0; x < row.Length; x++)
                row[x] = new Vector4(channels is 1 ? new Vector3((float)responses[x, 0]) : new Vector3((float)responses[x, 0], (float)responses[x, 1], (float)responses[x, 2]), row[x].W);
        }).Run(frame, progress);

    public static Fin<Unit> Lines(
        PixelFrame frame, Mat edges, double threshold, double width, Vector3 line, Vector3 under, float floor, BlendingMode mode, Gamut gamut, IProgress<int> progress) {
        using Mat value = new();
        using Mat mask = new();
        using Mat reach = new();
        using ScalarArray cut = new(threshold);
        if (edges.NumberOfChannels is 3) {
            using Mat weights = new(1, 3, DepthType.Cv64F, 1);
            weights.SetTo([gamut.Luminance.R, gamut.Luminance.G, gamut.Luminance.B]);
            CvInvoke.Transform(edges, value, weights);
        } else
            CvInvoke.ExtractChannel(edges, value, 0);
        CvInvoke.Compare(value, cut, mask, CmpType.GreaterThan);
        CvInvoke.BitwiseNot(mask, mask);
        CvInvoke.DistanceTransform(mask, reach, labels: null, DistType.L2, 5, DistLabelType.Pixel);
        return new PixelPass.Pointwise((row, _, at) => {
            using SpanOwner<Vector4> layer = SpanOwner<Vector4>.Allocate(row.Length);
            using SpanOwner<float> cover = SpanOwner<float>.Allocate(row.Length);
            ReadOnlySpan<float> distances = reach.View<float>().GetRowSpan(frame.RowIndex(at));
            for (int x = 0; x < row.Length; x++) {
                float coverage = ShapeEdge.Strip(distances[x] + 0.5f, width, 1f);
                layer.Span[x] = new Vector4(Vector3.Lerp(under, line, coverage), 1f);
                cover.Span[x] = float.Max(coverage, floor);
            }
            mode.Composite(row, layer.Span, cover.Span, PixelAlphaRepresentation.Unassociated, gamut);
        }).Run(frame, progress);
    }
}

public static class GuideEdges {
    public static ContourMarks Silhouette(Frustum frustum, double step) =>
        (guide, first, second, edges) => {
            using Mat z = new();
            using Mat curvature = new(guide.Size, DepthType.Cv64F, 1);
            using Mat change = new();
            using Mat marks = new();
            using Mat taps = new(1 + (2 * (second.Y - first.Y)), 1 + (2 * (second.X - first.X)), DepthType.Cv32F, 1);
            using ScalarArray zero = new(0d);
            using ScalarArray ratio = new(step);
            taps.SetTo([1f, -2f, 1f]);
            CvInvoke.ExtractChannel(guide, z, 0);
            z.ConvertTo(z, DepthType.Cv64F);
            frustum.Switch(z,
                perspective: static (depth, _) => {
                    using ScalarArray one = new(1d);
                    CvInvoke.Divide(one, depth, depth);
                },
                parallel: static (depth, _) => {
                    using ScalarArray infinity = new(float.PositiveInfinity);
                    using Mat background = new();
                    CvInvoke.Compare(depth, infinity, background, CmpType.Equal);
                    depth.SetTo(new MCvScalar(0d), background);
                });
            CvInvoke.Filter2D(z, curvature, taps, new Point(-1, -1), 0d, BorderType.Replicate);
            CvInvoke.AbsDiff(curvature, zero, change);
            CvInvoke.Multiply(z, ratio, z);
            CvInvoke.Compare(change, z, marks, CmpType.GreaterThan);
            using Mat leading = new(marks, first);
            using Mat trailing = new(marks, second);
            CvInvoke.BitwiseAnd(leading, trailing, edges);
            return unit;
        };

    public static ContourMarks Crease(double angle) =>
        (guide, firstBox, secondBox, edges) => {
            using Mat first = new(guide, firstBox);
            using Mat second = new(guide, secondBox);
            using Mat near = new();
            using Mat far = new();
            using Mat rgb = ChannelMask.Rgb();
            using Mat product = new();
            using Mat dot = new();
            using Mat lengths = new();
            using Mat partner = new();
            using ScalarArray bend = new(double.Cos(angle));
            first.ConvertTo(near, DepthType.Cv64F);
            second.ConvertTo(far, DepthType.Cv64F);
            foreach ((Mat left, Mat right, Mat sum) in Seq((near, far, dot), (near, near, lengths), (far, far, partner))) {
                CvInvoke.Multiply(left, right, product);
                CvInvoke.Transform(product, sum, rgb);
            }
            CvInvoke.Multiply(lengths, partner, lengths);
            CvInvoke.Sqrt(lengths, lengths);
            CvInvoke.Multiply(lengths, bend, lengths);
            CvInvoke.Compare(dot, lengths, edges, CmpType.LessThan);
            return unit;
        };

    public static Fin<Unit> Identity(Mat guide, Rectangle firstBox, Rectangle secondBox, Mat edges) {
        using Mat first = new(guide, firstBox);
        using Mat second = new(guide, secondBox);
        using Mat leading = new();
        using Mat trailing = new();
        CvInvoke.ExtractChannel(first, leading, 0);
        CvInvoke.ExtractChannel(second, trailing, 0);
        CvInvoke.Compare(leading, trailing, edges, CmpType.NotEqual);
        return unit;
    }
}

public static class Contours {
    public static Fin<Func<float, double>> Width(double pixels, Option<double> world, Option<Camera> camera, PixelExtent extent, Option<(double Near, Func<float, float> Scale)> taper) =>
        world.Match(
            meters => meters is 0d || pixels is 0d ? Fin.Succ<Func<float, double>>(static _ => 0d) : camera.ToFin(new ImagingFault("world line width", ImagingReason.Uncamera)).Map(view =>
                view.Frustum.Switch(
                    (Frame: pixels, Span: meters * extent.Width / view.Frustum.Window.Width),
                    perspective: static (scale, _) => (Func<float, double>)(depth => double.Min(scale.Span / depth, scale.Frame)),
                    parallel: static (scale, _) => _ => double.Min(scale.Span, scale.Frame))),
            () => Fin.Succ<Func<float, double>>(_ => pixels)).Map(width => taper.Match(
                near => depth => width(depth) * near.Scale((float)double.Clamp(near.Near / depth, 0d, 1d)),
                () => width));

    public static Fin<Unit> Outlined(
        PixelFrame frame, PixelFrame depth, Seq<ContourLine> lines, Option<Vector4> paper, BlendingMode mode, Gamut gamut, IProgress<int> progress) => frame.Filter(_ => {
            Seq<ContourLine> drawn = lines.Filter(static line => !line.Sources.IsEmpty);
            using Mat range = depth.Header();
            using Mat z = new();
            using Mat cover = Mat.Zeros(frame.Size.Height, frame.Size.Width, DepthType.Cv32F, int.Max(drawn.Count, 1));
            using Mat unclaimed = new(frame.Size.Height, frame.Size.Width, DepthType.Cv8U, 1);
            unclaimed.SetTo(new MCvScalar(byte.MaxValue));
            CvInvoke.ExtractChannel(range, z, 0);
            Vector3[] colors = [.. drawn.Map(static line => line.Color)];
            return drawn.Map(static (line, channel) => (Line: line, Channel: channel))
                .TraverseM(entry => Covered(z, entry.Line, entry.Channel, cover, unclaimed, frame, progress))
                .As()
                .Bind(_ => new PixelPass.Pointwise((row, _, line) => {
                    using SpanOwner<Vector4> layer = SpanOwner<Vector4>.Allocate(row.Length);
                    using SpanOwner<float> weights = SpanOwner<float>.Allocate(row.Length);
                    weights.Span.Fill(1f);
                    ReadOnlySpan2D<float> covers = cover.View<float>().GetRowSpan(frame.RowIndex(line))
                        .AsSpan2D(row.Length, cover.NumberOfChannels);
                    foreach (Vector4 ground in paper.ToSeq())
                        row.Fill(ground);
                    for (int x = 0; x < row.Length; x++) {
                        (Vector3 color, float weight) = (Vector3.Zero, 0f);
                        for (int k = 0; k < colors.Length; k++)
                            (color, weight) = (covers[x, k] * (1f - weight)) switch { var c => (color + (c * colors[k]), weight + c) };
                        layer.Span[x] = new Vector4(color, weight);
                    }
                    mode.Composite(row, layer.Span, weights.Span, PixelAlphaRepresentation.Unassociated, gamut, PixelAlphaRepresentation.Associated);
                }).Run(frame, progress));
        });

    private static Fin<Unit> Covered(Mat z, ContourLine line, int channel, Mat cover, Mat unclaimed, PixelFrame frame, IProgress<int> progress) {
        (int width, int height, int classes) = (z.Cols, z.Rows, cover.NumberOfChannels);
        using Mat near = Mat.Zeros(height, width, DepthType.Cv8U, 1);
        using Mat far = Mat.Zeros(height, width, DepthType.Cv8U, 1);
        using Mat contour = new(height, width, DepthType.Cv32F, 1);
        using Mat nearReach = new();
        using Mat farReach = new();
        using Mat nearLabels = new();
        using Mat farLabels = new();
        contour.SetTo(new MCvScalar(float.PositiveInfinity));
        return line.Sources.TraverseM(source => Marked(source.Marks, source.Guide, z, near, far, contour)).As().Bind(_ => {
            CvInvoke.BitwiseAnd(near, unclaimed, near);
            CvInvoke.BitwiseAnd(far, unclaimed, far);
            using Mat claimed = new();
            CvInvoke.BitwiseOr(near, far, claimed);
            CvInvoke.BitwiseNot(claimed, unclaimed);
            using Mat nearWidths = Reached(near, contour, line.Width, nearReach, nearLabels);
            using Mat farWidths = Reached(far, contour, line.Width, farReach, farLabels);
            return new PixelPass.Pointwise((_, _, at) => {
                int row = frame.RowIndex(at);
                ReadOnlySpan<float> nearDistances = nearReach.View<float>().GetRowSpan(row);
                ReadOnlySpan<float> farDistances = farReach.View<float>().GetRowSpan(row);
                ReadOnlySpan<int> nearLines = nearLabels.View<int>().GetRowSpan(row);
                ReadOnlySpan<int> farLines = farLabels.View<int>().GetRowSpan(row);
                ReadOnlySpan<double> nearStrokes = nearWidths.GetSpan<double>();
                ReadOnlySpan<double> farStrokes = farWidths.GetSpan<double>();
                Span2D<float> covers = cover.View<float>().GetRowSpan(row).AsSpan2D(width, classes);
                for (int x = 0; x < width; x++)
                    covers[x, channel] = line.Placement.Cover(nearDistances[x], nearStrokes[nearLines[x]], farDistances[x], farStrokes[farLines[x]]);
            }).Run(frame, progress);
        });
    }

    private static Fin<Unit> Marked(ContourMarks mark, PixelFrame guide, Mat z, Mat near, Mat far, Mat contour) {
        using Mat header = guide.Header();
        using Mat edges = new();
        using Mat side = new();
        using Mat nearer = new();
        using Mat lowered = new();
        return Seq(
                (new Rectangle(0, 0, z.Cols - 1, z.Rows), new Rectangle(1, 0, z.Cols - 1, z.Rows)),
                (new Rectangle(0, 0, z.Cols, z.Rows - 1), new Rectangle(0, 1, z.Cols, z.Rows - 1))).Filter(static pair => pair.Item1.Width > 0 && pair.Item1.Height > 0).TraverseM(pair => {
                    (Rectangle first, Rectangle second) = pair;
                    using Mat firstDepth = new(z, first);
                    using Mat secondDepth = new(z, second);
                    return mark(header, first, second, edges).Map(_ => {
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
                        return unit;
                    });
                }).As().Map(static _ => unit);
    }

    private static Mat Reached(Mat mask, Mat contour, Func<float, double> stroke, Mat reach, Mat labels) {
        Mat widths = Mat.Zeros(1, CvInvoke.CountNonZero(mask) + 1, DepthType.Cv64F, 1);
        CvInvoke.BitwiseNot(mask, mask);
        CvInvoke.DistanceTransform(mask, reach, labels, DistType.L2, 5, DistLabelType.Pixel);
        Span<double> table = widths.GetSpan<double>();
        ReadOnlySpan<byte> marks = mask.GetSpan<byte>();
        ReadOnlySpan<float> depths = contour.GetSpan<float>();
        for (int pixel = 0, label = 1; pixel < marks.Length; pixel++) {
            if (marks[pixel] is 0)
                table[label++] = stroke(depths[pixel]);
        }
        return widths;
    }
}

internal static class ChannelMask {
    public static Mat Rgb() {
        Mat sums = new(1, 4, DepthType.Cv32F, 1);
        sums.SetTo([1f, 1f, 1f, 0f]);
        return sums;
    }
}
