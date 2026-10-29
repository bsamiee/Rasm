using System.Drawing;
using System.Numerics;
using System.Numerics.Tensors;
using CommunityToolkit.HighPerformance;
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
    public static Iris Circular { get; } = new(3, 0d, 1d, 0d, 1d);

    public int Blades { get; }
    public double Rotation { get; }
    public double Roundness { get; }
    public double Obstruction { get; }
    public double Squeeze { get; }

    public double Circumradius =>
        double.Asin(Roundness * double.SinPi(1d / Blades)) switch {
            var beta => double.Sqrt(double.Pi / (Blades / 2d * (double.Sin(double.Tau / Blades) + (Roundness is 0d ? 0d : ((2d * beta) - double.Sin(2d * beta)) / (Roundness * Roundness))))),
        };

    public (double Reach, double Edge) Outline(Vector2 point) {
        (double x, double y) = (Squeeze * point.X, point.Y);
        double edge = Boundary(double.Atan2(-y, x), Circumradius);
        return (double.Hypot(x, y) / edge, edge);
    }

    public Vector4 Weights(double reach, double edge, double radius, double ramp, double fringe) {
        Vector3 radii = Radii(radius, fringe);
        Vector3 weights = new((float)Weight(radii.X), (float)Weight(radii.Y), (float)Weight(radii.Z));
        return new(weights, (weights.X + weights.Y + weights.Z) / 3f);

        double Weight(double channel) => channel > 0d && Obstruction < 1d ? Profile(reach * edge, channel * edge, Obstruction * channel * edge, ramp) : 0d;
    }

    public Vector4 Integral(double radius, double ramp, double fringe) {
        Vector3 radii = Radii(radius, fringe);
        Vector3 areas = new((float)Area(radii.X), (float)Area(radii.Y), (float)Area(radii.Z));
        return new(areas, (areas.X + areas.Y + areas.Z) / 3f);

        double Area(double channel) => ((channel * Circumradius) + ramp) switch { var half => Mass(channel, ramp, new RectangleF((float)(-half / Squeeze), (float)-half, (float)(2d * half / Squeeze), (float)(2d * half))) };
    }

    public double Mass(double radius, double ramp, RectangleF box) {
        (double left, double top, double right, double bottom) = (Squeeze * box.Left, box.Top, Squeeze * box.Right, box.Bottom);
        (double reach, double circumradius) = (Outline(new Vector2(box.X + (box.Width / 2f), box.Y + (box.Height / 2f))).Reach, Circumradius);
        double slack = ((double.Hypot(right - left, bottom - top) / 2d) + ramp) / (circumradius * double.CosPi(1d / Blades));
        return radius <= 0d || Obstruction is 1d || reach - slack >= radius || reach + slack <= Obstruction * radius ? 0d
            : reach + slack <= radius && reach - slack >= Obstruction * radius ? (double)box.Width * box.Height
            : Sections() / Squeeze;

        double Sections() {
            double[] angles = [0d, double.Pi / 2d, double.Pi, 3d * double.Pi / 2d, double.Tau, Turn(double.Atan2(top, left)), Turn(double.Atan2(bottom, left)), Turn(double.Atan2(top, right)),
                Turn(double.Atan2(bottom, right)), .. Enumerable.Range(0, Blades).Select(blade => Turn(-Rotation - (double.Tau * blade / Blades)))];
            angles.AsSpan().Sort();
            return Enumerable.Range(1, angles.Length - 1).Sum(i => Integrate.GaussKronrod(Section, angles[i - 1], angles[i]));
        }

        double Section(double angle) {
            (double sin, double cos) = double.SinCos(angle);
            ((double nearX, double farX), (double nearY, double farY)) = (Crossing(left, right, cos), Crossing(top, bottom, sin));
            double edge = radius * Boundary(-angle, circumradius);
            (double near, double far) = (double.Max(0d, double.Max(nearX, nearY)), double.Min(edge + ramp, double.Min(farX, farY)));
            return far > near ? Swept(far, edge, Obstruction * edge, ramp) - Swept(near, edge, Obstruction * edge, ramp) : 0d;
        }

        static (double Near, double Far) Crossing(double low, double high, double direction) =>
            direction is 0d ? low <= 0d && high >= 0d ? (0d, double.PositiveInfinity) : (double.PositiveInfinity, double.NegativeInfinity)
                : (double.Min(low / direction, high / direction), double.Max(low / direction, high / direction));

        static double Turn(double angle) => ((angle % double.Tau) + double.Tau) % double.Tau;
    }

    private double Boundary(double angle, double circumradius) {
        (int n, double kappa) = (Blades, Roundness);
        (double s1, double c1) = double.SinCos(double.Pi / n);
        (double sa, double ca) = double.SinCos(double.Ieee754Remainder(angle - Rotation - (double.Pi / n), double.Tau / n));
        double root = double.Sqrt(1d - (kappa * kappa * s1 * s1));
        double delta = root - (kappa * c1);
        return circumradius * ((2d * c1 * root) - (kappa * (1d - (2d * s1 * s1)))) / ((delta * ca) + double.Sqrt(1d - (delta * delta * sa * sa)));
    }

    private static double Profile(double distance, double edge, double inner, double ramp) =>
        ramp is 0d ? distance >= inner && distance <= edge ? 1d : 0d : double.Clamp(double.Min(distance - inner + ramp, edge + (ramp / 2d) - distance) / ramp, 0d, 1d);

    private static double Swept(double distance, double edge, double inner, double ramp) {
        (double low, double high) = (inner - ramp, edge + (ramp / 2d));
        (double middle, double reached) = ((low + high) / 2d, double.Clamp(distance, inner, edge));
        return ramp is 0d ? ((reached * reached) - (inner * inner)) / 2d
            : (Ramp(low) - Ramp(double.Min(low + ramp, middle)) - Ramp(double.Max(high - ramp, middle)) + Ramp(high)) / ramp;

        double Ramp(double start) => double.Max(start, 0d) switch {
            var from => distance > from ? Primitive(distance, start) - Primitive(from, start) : 0d,
        };

        static double Primitive(double at, double start) => at * at * ((at / 3d) - (start / 2d));
    }

    private static Vector3 Radii(double radius, double fringe) =>
        new((float)(radius * (1d - double.Max(fringe, 0d))), (float)(radius * (1d - (double.Abs(fringe) / 2d))), (float)(radius * (1d + double.Min(fringe, 0d))));

    static partial void ValidateFactoryArguments(ref ImagingFault? validationError, ref int blades, ref double rotation, ref double roundness, ref double obstruction, ref double squeeze) =>
        validationError = blades >= 3 && double.IsFinite(rotation) && roundness is >= 0d and <= 1d && obstruction is >= 0d and <= 1d && double.IsFinite(squeeze) && squeeze > 0d ? null
            : ImagingFault.Of(ImagingReason.Range, (blades, rotation, roundness, obstruction, squeeze));

    public static Fin<Iris> Validated(int blades, double rotation, double roundness, double obstruction, double squeeze) =>
        TryCreate(blades, rotation, roundness, obstruction, squeeze, out Iris? iris, out ImagingFault? fault) ? iris : fault;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Bokeh {
    public static Fin<PointSpread> Spread(Iris iris, double fringe, Vector4 radii) =>
        toSeq(Range(0, 4)).Traverse(channel => Gaussian.Taps(radii[channel] * iris.Circumradius / double.Min(iris.Squeeze, 1d)).Map(side => {
            Mat kernel = new(side, side, DepthType.Cv32F, 1);
            Span2D<float> cells = kernel.View<float>();
            for (int y = 0; y < side; y++) {
                for (int x = 0; x < side; x++)
                    cells[y, x] = Tap(iris, fringe, iris.Outline(new Vector2((side / 2) - x, (side / 2) - y)), radii[channel])[channel];
            }
            CvInvoke.Normalize(kernel, kernel, 1d, 0d, NormType.L1);
            return kernel;
        })).As().Map(static kernels => new PointSpread(kernels));

    public static PixelPass Correlated(Iris iris, double fringe, Vector4 radii) =>
        new PixelPass.Frame((frame, progress) =>
            from spread in Spread(iris, fringe, radii)
            from convolved in frame.Filter(header => {
                using (spread) {
                    using Mat light = frame.Premultiplied();
                    spread.Convolve(light, BorderType.Replicate);
                    light.CopyTo(header);
                }
                frame.Rows(static (row, _, _) => PixelFrame.Unassociated(row), progress);
                return unit;
            })
            select convolved);

    public static Action<PixelFrame, Mat> Focal(Matrix3x2 field, DistanceField region, float feather, int power, float cap) =>
        (frame, radii) => {
            Span2D<float> radius = radii.View<float>();
            for (int y = 0; y < radius.Height; y++) {
                for (int x = 0; x < radius.Width; x++)
                    radius[y, x] = cap * float.Pow(float.Clamp(region.Distance(Vector2.Transform(new Vector2(frame.Origin.X + x + 0.5f, frame.Origin.Y + y + 0.5f), field)) / feather, 0f, 1f), power);
            }
        };

    public static Option<Action<PixelFrame, Mat>> Depth(PixelFrame depth, Camera camera, double cap) =>
        from focus in camera.Focus
        from lens in camera.Lens is Projection.Rectilinear rectilinear ? Some(rectilinear) : None
        let distance = focus.FocusDistance.Meters.ToDouble()
        let focal = focus.FocalLength.Meters.ToDouble()
        let scale = focal * lens.Extent.ShortSide / (2d * focus.FNumber * (distance - focal) * float.Min(lens.Window.Size.X, lens.Window.Size.Y))
        select new Action<PixelFrame, Mat>((frame, radii) => {
            using Mat spread = new(frame.Size.Height, frame.Size.Width, DepthType.Cv32F, 1);
            Span2D<float> radius = radii.View<float>();
            Span2D<float> magnitude = spread.View<float>();
            ReadOnlySpan2D<Vector4> distances = depth.View.Span;
            for (int y = 0; y < radius.Height; y++) {
                for (int x = 0; x < radius.Width; x++)
                    magnitude[y, x] = float.Abs(radius[y, x] = (float)double.Clamp(
                        scale * (1d - (distance / distances[int.Clamp(frame.Origin.Y + y - depth.Origin.Y, 0, distances.Height - 1), int.Clamp(frame.Origin.X + x - depth.Origin.X, 0, distances.Width - 1)].X)), -cap, cap));
            }
            spread.Blur(spread, cap / 3d);
            TensorPrimitives.MinMagnitude(radii.GetSpan<float>(), spread.GetSpan<float>(), spread.GetSpan<float>());
            TensorPrimitives.CopySign(spread.GetSpan<float>(), radii.GetSpan<float>(), radii.GetSpan<float>());
        });

    public static PixelPass Gathered(Iris iris, double fringe, Action<PixelFrame, Mat> radii) =>
        new PixelPass.Frame((frame, progress) => {
            using Mat source = frame.Premultiplied();
            using Mat field = Mat.Zeros(frame.Size.Height, frame.Size.Width, DepthType.Cv32F, 1);
            radii(frame, field);
            field.MinMax(out double[] least, out double[] most, out Point[] _, out Point[] _);
            (double circumradius, double near) = (iris.Circumradius / double.Min(iris.Squeeze, 1d), double.Max(0d, -least[0]));
            return Gaussian.Taps(double.Max(near, most[0]) * circumradius).Bind(side => {
                (double Reach, double Edge)[] outlines = [.. from y in Enumerable.Range(0, side) from x in Enumerable.Range(0, side) select iris.Outline(new Vector2((side / 2) - x, (side / 2) - y))];
                return new PixelPass.Positional((row, _, top) => {
                    ReadOnlySpan2D<Vector4> light = source.View<Vector4>();
                    ReadOnlySpan2D<float> radius = field.View<float>();
                    ReadOnlySpan2D<(double Reach, double Edge)> shapes = new(outlines, side, side);
                    int y = top - frame.Origin.Y;
                    for (int x = 0; x < row.Length; x++) {
                        float own = float.Abs(radius[y, x]);
                        int reach = (int)double.Ceiling(double.Max(own, near) * circumradius);
                        (Vector4 sum, Vector4 total) = (Vector4.Zero, Vector4.Zero);
                        for (int dy = -reach; dy <= reach; dy++) {
                            for (int dx = -reach; dx <= reach; dx++) {
                                (int u, int v) = (int.Clamp(x + dx, 0, radius.Width - 1), int.Clamp(y + dy, 0, radius.Height - 1));
                                Vector4 weights = Tap(iris, fringe, shapes[dy + (side / 2), dx + (side / 2)], float.Abs(float.Min(radius[v, u], own)));
                                (sum, total) = (sum + (weights * light[v, u]), total + weights);
                            }
                        }
                        row[x] = sum / total;
                    }
                    PixelFrame.Unassociated(row);
                }).Run(frame, progress);
            });
        });

    private static Vector4 Tap(Iris iris, double fringe, (double Reach, double Edge) outline, double radius) =>
        outline.Reach is 0d ? Vector4.One : iris.Weights(outline.Reach, outline.Edge, radius, 1d, fringe);
}
