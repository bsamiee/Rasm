using System.Drawing;
using System.Globalization;
using System.Numerics;
using System.Numerics.Tensors;
using System.Runtime.Intrinsics;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using Emgu.CV;
using Emgu.CV.CvEnum;
using MathNet.Numerics;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Procedural;

namespace Rasm.Imaging.Filtering;

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
[ValidationError<ImagingFault>]
public sealed partial class Iris {
    public int Blades { get; }
    public double Rotation { get; }
    public double Roundness { get; }
    public double Obstruction { get; }
    public double Squeeze { get; }

    public float Circumradius =>
        double.Asin(Roundness * double.Sin(double.Pi / Blades)) switch {
            var beta => (float)double.Sqrt(double.Pi / (Blades / 2d * (double.Sin(double.Tau / Blades) + (Roundness is 0d ? 0d : ((2d * beta) - double.Sin(2d * beta)) / (Roundness * Roundness))))),
        };

    public (double Reach, double Edge) Outline((double X, double Y) q) {
        (double x, double y) = (Squeeze * q.X, q.Y);
        double edge = Boundary(double.Atan2(-y, x), Circumradius);
        return (double.Hypot(x, y) / edge, edge);
    }

    public Func<(double X, double Y), double> Transmission(double radius) {
        (double obstruction, double squeeze, double circumradius) = (Obstruction, Squeeze, Circumradius);
        double margin = double.Hypot(squeeze, 1d) / (2d * circumradius * double.Cos(double.Pi / Blades));
        double[] blades = [.. Enumerable.Range(0, Blades).Select(i => (((-Rotation - (double.Tau * i / Blades)) % double.Tau) + double.Tau) % double.Tau)];
        return point => {
            (double x, double y) = (squeeze * point.X, point.Y);
            double reach = double.Hypot(x, y) / Boundary(double.Atan2(-y, x), circumradius);
            if (reach - margin >= radius || reach + margin <= obstruction * radius)
                return 0d;
            if (reach + margin <= radius && reach - margin >= obstruction * radius)
                return 1d / (1d - obstruction);
            (double left, double right, double top, double bottom) = (x - (squeeze / 2d), x + (squeeze / 2d), y - 0.5d, y + 0.5d);
            double[] angles = [0d, double.Pi / 2d, double.Pi, 3d * double.Pi / 2d, double.Tau, .. blades, Bearing(left, top), Bearing(left, bottom), Bearing(right, top), Bearing(right, bottom)];
            angles.AsSpan().Sort();
            double area = 0d;
            for (int i = 1; i < angles.Length; i++)
                area += Integrate.GaussKronrod(Section, angles[i - 1], angles[i]);
            return area / squeeze;

            double Section(double angle) {
                (double sin, double cos) = double.SinCos(angle);
                (double nearX, double farX) = cos is 0d ? left <= 0d && right >= 0d ? (0d, double.PositiveInfinity) : (double.PositiveInfinity, double.NegativeInfinity)
                    : (double.Min(left / cos, right / cos), double.Max(left / cos, right / cos));
                (double nearY, double farY) = sin is 0d ? top <= 0d && bottom >= 0d ? (0d, double.PositiveInfinity) : (double.PositiveInfinity, double.NegativeInfinity)
                    : (double.Min(top / sin, bottom / sin), double.Max(top / sin, bottom / sin));
                double edge = radius * Boundary(-angle, circumradius);
                (double near, double far) = (double.Max(0d, double.Max(double.Max(nearX, nearY), obstruction * edge)), double.Min(edge, double.Min(farX, farY)));
                return far > near ? (far - near) * (far + near) / (2d * (1d - obstruction)) : 0d;
            }

            static double Bearing(double x, double y) => (double.Atan2(y, x) + double.Tau) % double.Tau;
        };
    }

    public Vector256<double> Weights(double reach, double edge, double radius, double ramp, double fringe) {
        Vector256<double> r = Radii(radius, fringe);
        Vector256<double> distance = Vector256.Create(reach);
        Vector256<double> one = Vector256.Create(1d);
        Vector256<double> obstruction = Vector256.Create(Obstruction) * r;
        Vector256<double> w = Obstruction is 1d ? Vector256<double>.Zero : ramp is 0d
            ? Vector256.ConditionalSelect(Vector256.LessThanOrEqual(distance, r) & Vector256.GreaterThanOrEqual(distance, obstruction), one, Vector256<double>.Zero)
            : Vector256.Create(edge / ramp) switch {
                var slope => Vector256.Min(Vector256.Clamp((slope * (r - distance)) + Vector256.Create(0.5d), Vector256<double>.Zero, one), Vector256.Clamp(one + (slope * (distance - obstruction)), Vector256<double>.Zero, one)),
            };
        w = Vector256.ConditionalSelect(Vector256.GreaterThan(r, Vector256<double>.Zero), w, Vector256<double>.Zero);
        return Vector256.Create(w[0], w[1], w[2], (w[0] + w[1] + w[2]) / 3d);
    }

    public Vector256<double> Integral(double radius, double ramp, double fringe) {
        (Vector256<double> r, double circumradius) = (Radii(radius, fringe), Circumradius);
        double[] areas = [.. Enumerable.Range(0, 3).Select(component => r[component] is 0d || Obstruction is 1d ? 0d
            : Blades / Squeeze * Integrate.GaussKronrod(angle => {
                double boundary = r[component] * Boundary(angle, circumradius);
                double inner = Obstruction * boundary;
                if (ramp is 0d)
                    return (boundary - inner) * (boundary + inner) / 2d;
                (double high, double low) = (boundary + (ramp / 2d), inner - ramp);
                double upper = double.Min(1d, double.Min(high / ramp, (high - low) / (2d * ramp)));
                double width = ramp * upper;
                return low >= 0d ? upper * (high + low) * (high - low - width) / 2d
                    : ((upper * ((high * high) - (high * width) + (width * width / 3d))) - (double.Pow(double.Max(0d, low + width), 3d) / (3d * ramp))) / 2d;
            }, Rotation, Rotation + (double.Tau / Blades)))];
        return Vector256.Create(areas[0], areas[1], areas[2], (areas[0] + areas[1] + areas[2]) / 3d);
    }

    private double Boundary(double angle, double circumradius) {
        (int n, double kappa) = (Blades, Roundness);
        (double s1, double c1) = double.SinCos(double.Pi / n);
        (double sa, double ca) = double.SinCos(double.Ieee754Remainder(angle - Rotation - (double.Pi / n), double.Tau / n));
        double root = double.Sqrt(1d - (kappa * kappa * s1 * s1));
        double delta = root - (kappa * c1);
        return circumradius * ((2d * c1 * root) - (kappa * (1d - (2d * s1 * s1)))) / ((delta * ca) + double.Sqrt(1d - (delta * delta * sa * sa)));
    }

    private static Vector256<double> Radii(double radius, double fringe) =>
        Vector256.Create(radius * (1d - double.Max(fringe, 0d)), radius * (1d - (double.Abs(fringe) / 2d)), radius * (1d + double.Min(fringe, 0d)), 0d);

    static partial void ValidateFactoryArguments(ref ImagingFault? validationError, ref int blades, ref double rotation, ref double roundness, ref double obstruction, ref double squeeze) =>
        validationError = blades >= 3 && double.IsFinite(rotation) && roundness is >= 0d and <= 1d && obstruction is >= 0d and <= 1d && double.IsFinite(squeeze) && squeeze > 0d ? null
            : new ImagingFault(string.Create(CultureInfo.InvariantCulture,
                $"iris of {blades} blades, roundness {roundness}, obstruction {obstruction}, rotation {rotation}, and squeeze {squeeze}"), ImagingReason.Range);

    public static Fin<Iris> Validated(int blades, double rotation, double roundness, double obstruction, double squeeze) =>
        TryCreate(blades, rotation, roundness, obstruction, squeeze, out Iris? iris, out ImagingFault? fault) ? iris : fault;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Bokeh {
    public static Func<Mat, Mat, PixelFrame, Fin<Unit>> Uniform(Iris iris, double fringe, double radius) =>
        (light, _, _) => Correlate(light, iris, fringe, Vector256.Create(radius));

    public static PixelPass.Frame Axial(Iris iris, ShortSideLength radius) =>
        Gathered(iris, 0d, (light, _, frame) => Correlate(light, iris, 0d, Vector256.Create(0.5d, 0d, 1d, 0d) * radius.Pixels(frame.Extent)));

    public static Fin<Unit> Correlate(Mat image, Iris iris, double fringe, Vector256<double> radii) =>
        toSeq(Range(0, image.NumberOfChannels)).Traverse(channel => PixelSampling.KernelTaps<int>(radii[channel] * iris.Circumradius).Map(side => (Channel: channel, Side: side))).As().Map(kernels => {
            foreach ((int channel, int side) in kernels) {
                using Mat plane = new();
                using Mat kernel = new(side, side, DepthType.Cv32F, 1);
                Span2D<float> cells = kernel.View<float>();
                for (int y = 0; y < side; y++) {
                    for (int x = 0; x < side; x++)
                        cells[y, x] = (float)Tap(iris, fringe, iris.Outline(((side / 2) - x, (side / 2) - y)), radii[channel])[channel];
                }
                CvInvoke.Normalize(kernel, kernel, 1d, 0d, NormType.L1);
                CvInvoke.ExtractChannel(image, plane, channel);
                CvInvoke.Filter2D(plane, plane, kernel, new Point(-1, -1), 0d, BorderType.Replicate);
                CvInvoke.InsertChannel(plane, image, channel);
            }
            return unit;
        });

    public static Func<Mat, Mat, PixelFrame, Fin<Unit>> Focal(FieldFrame field, double sharp, double feather, double aspect, int power, double cap) =>
        (_, radii, frame) => {
            Span2D<float> radius = radii.View<float>();
            for (int y = 0; y < radius.Height; y++) {
                for (int x = 0; x < radius.Width; x++) {
                    (double u, double v) = field.Local(((double)frame.Origin.X + x + 0.5d, (double)frame.Origin.Y + y + 0.5d));
                    radius[y, x] = (float)(cap * double.Pow(double.Clamp((double.Hypot(aspect * u, v) - sharp) / feather, 0d, 1d), power));
                }
            }
            return unit;
        };

    public static Option<Func<Mat, Mat, PixelFrame, Fin<Unit>>> Depth(PixelFrame depth, Camera camera, double fStop, double cap, PixelExtent extent) =>
        from lens in camera.Lens
        from window in camera.Frustum.Switch(perspective: static frustum => Some(frustum.Window), parallel: static _ => Option<ViewWindow>.None)
        let focus = lens.FocusDistance.Meters.ToDouble()
        let focal = lens.FocalLength.Meters.ToDouble()
        let k = focal * extent.ShortSide / (2d * fStop * (focus - focal))
        let span = double.Min(window.Width, window.Height)
        select new Func<Mat, Mat, PixelFrame, Fin<Unit>>((_, radii, frame) => {
            using Mat magnitude = new(frame.Size.Height, frame.Size.Width, DepthType.Cv32F, 1);
            Span2D<float> radius = radii.View<float>();
            Span2D<float> blurred = magnitude.View<float>();
            ReadOnlySpan2D<Vector4> distance = depth.View.Span;
            for (int y = 0; y < radius.Height; y++) {
                for (int x = 0; x < radius.Width; x++) {
                    (int u, int v) = ((int)long.Clamp((long)frame.Origin.X + x - depth.Origin.X, 0, distance.Width - 1), (int)long.Clamp((long)frame.Origin.Y + y - depth.Origin.Y, 0, distance.Height - 1));
                    (radius[y, x], blurred[y, x]) = (float)double.Clamp(k * (1d - (focus / distance[v, u].X)) / span, -cap, cap) switch { var r => (r, float.Abs(r)) };
                }
            }
            return magnitude.Blur(magnitude, cap).Map(_ => {
                Span<float> values = radii.GetSpan<float>();
                using SpanOwner<float> limits = SpanOwner<float>.Allocate(values.Length);
                TensorPrimitives.Abs(values, limits.Span);
                TensorPrimitives.Min(magnitude.GetSpan<float>(), limits.Span, limits.Span);
                TensorPrimitives.CopySign(limits.Span, values, values);
                return unit;
            });
        });

    public static PixelPass.Frame Gathered(Iris iris, double fringe, Func<Mat, Mat, PixelFrame, Fin<Unit>> field) =>
        new((frame, progress) => frame.Filter(_ => {
            using Mat source = frame.Premultiplied();
            using Mat radii = Mat.Zeros(frame.Size.Height, frame.Size.Width, DepthType.Cv32F, 1);
            return field(source, radii, frame).Bind(_ => {
                radii.MinMax(out double[] least, out double[] most, out Point[] _, out Point[] _);
                (float circumradius, float near) = (iris.Circumradius, float.Max(0f, -(float)least[0]));
                int side = Window(float.Max(near, (float)most[0]) * circumradius);
                (double Reach, double Edge)[] outlines = [.. from y in Enumerable.Range(0, side) from x in Enumerable.Range(0, side) select iris.Outline(((side / 2) - x, (side / 2) - y))];
                return new PixelPass.Pointwise((row, _, line) => {
                    ReadOnlySpan2D<Vector4> light = source.View<Vector4>();
                    ReadOnlySpan2D<float> radius = radii.View<float>();
                    ReadOnlySpan2D<(double Reach, double Edge)> shapes = new(outlines, side, side);
                    int y = frame.RowIndex(line);
                    for (int x = 0; x < row.Length; x++) {
                        float own = float.Abs(radius[y, x]);
                        int reach = Window(float.Max(own, near) * circumradius) / 2;
                        (Vector256<double> sum, Vector256<double> total) = (Vector256<double>.Zero, Vector256<double>.Zero);
                        for (int dy = -reach; dy <= reach; dy++) {
                            for (int dx = -reach; dx <= reach; dx++) {
                                (int u, int v) = (int.Clamp(x + dx, 0, radius.Width - 1), int.Clamp(y + dy, 0, radius.Height - 1));
                                Vector256<double> weights = Tap(iris, fringe, shapes[dy + (side / 2), dx + (side / 2)], float.Abs(float.Min(radius[v, u], own)));
                                Vector4 pixel = light[v, u];
                                (sum, total) = (sum + (weights * pixel.Wide), total + weights);
                            }
                        }
                        row[x] = (sum / total).Narrowed.AsVector4().Unassociated;
                    }
                }).Run(frame, progress);
            });
        }));

    private static Vector256<double> Tap(Iris iris, double fringe, (double Reach, double Edge) outline, double radius) =>
        outline.Reach is 0d ? Vector256.Create(1d) : iris.Weights(outline.Reach, outline.Edge, radius, 1d, fringe);

    private static int Window(float reach) => (2 * (int)float.Ceiling(reach)) + 1;
}
