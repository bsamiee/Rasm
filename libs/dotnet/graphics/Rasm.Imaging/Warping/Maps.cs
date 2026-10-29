using System.Drawing;
using System.Numerics;
using System.Runtime.Intrinsics;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using MathNet.Numerics.Integration;
using MathNet.Numerics.LinearAlgebra;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Procedural;
using UnitsNet;

namespace Rasm.Imaging.Warping;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class Sampling {
    public static readonly Sampling Nearest = new("nearest", Inter.Nearest, 0);
    public static readonly Sampling Linear = new("linear", Inter.Linear, 1);
    public static readonly Sampling Cubic = new("cubic", Inter.Cubic, 2);
    public static readonly Sampling Lanczos = new("lanczos", Inter.Lanczos4, 4);

    internal Inter Interpolation { get; }
    internal int Reach { get; }

    internal double Snapped(double coordinate) => Interpolation is Inter.Nearest ? double.Floor(coordinate) + 0.5d : coordinate;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CoordinateMap {
    private CoordinateMap(Sampling sampling, (WrapMode Across, WrapMode Down) wrap) => (Sampling, Wrap) = (sampling, wrap);

    public Sampling Sampling { get; }
    public (WrapMode Across, WrapMode Down) Wrap { get; }

    public Fin<Unit> Apply(PixelFrame frame, Option<IProgress<int>> progress) => Run(frame, progress, [(this, Vector4.One)]);

    public Fin<PixelFrame> Resample(PixelFrame source, PixelExtent extent) =>
        Try.lift(() => Sampled(new PixelFrame(Point.Empty, extent, extent), source, [(this, Vector4.One)])).Run();

    public PixelFrame ToSTMap(PixelExtent extent) {
        PixelFrame map = new(Point.Empty, extent, extent);
        map.Rows((row, x, y) => {
            using SpanOwner<Vector2> points = SpanOwner<Vector2>.Allocate(row.Length);
            using SpanOwner<float> coverage = SpanOwner<float>.Allocate(row.Length);
            Traced(x, y, extent, extent, points.Span, coverage.Span);
            for (int column = 0; column < row.Length; column++)
                row[column] = new Vector4(points.Span[column].X / extent.Width, 1f - (points.Span[column].Y / extent.Height), coverage.Span[column], coverage.Span[column]);
        });
        return map;
    }

    public static PixelPass.Frame Integrated(Func<double, (CoordinateMap Map, Vector4 Gains)> path, params Func<double, (CoordinateMap Map, Vector4 Gains)>[] additional) {
        Seq<Func<double, (CoordinateMap Map, Vector4 Gains)>> paths = path.Cons(toSeq(additional));
        return new((frame, progress) => new GaussLegendreRule(0d, 1d, int.Max(1, (int)double.Ceiling(double.Pi / 2d * paths.Max(path => Travel(path, frame.Extent))))) switch {
            var rule => Run(frame, progress, toSeq(
                from each in paths
                from step in Enumerable.Range(0, rule.Order)
                let sample = each(rule.GetAbscissa(step))
                select (sample.Map, sample.Gains * (float)rule.GetWeight(step)))),
        });
    }

    internal abstract void Trace(Span<Vector2> points, Span<float> coverage, PixelExtent output, PixelExtent source);

    internal void Bound(Span<Vector2> points, Span<float> coverage, PixelExtent extent) {
        for (int i = 0; i < points.Length; i++) {
            ((double x, double across), (double y, double down)) = (Folded(points[i].X, Wrap.Across, extent.Width), Folded(points[i].Y, Wrap.Down, extent.Height));
            (points[i], coverage[i]) = (new Vector2((float)x, (float)y), coverage[i] * (float)(across * down));
        }
    }

    private protected virtual void Accumulate(Mat source, PixelFrame original, PixelFrame destination, Vector4 share, Mat sum) {
        (int pad, PixelExtent size) = (Sampling.Reach + 1, destination.Size);
        using Mat wide = new();
        using Mat padded = new();
        using Mat map = new(size.Height, size.Width, DepthType.Cv32F, 2);
        using Mat weights = new(size.Height, size.Width, DepthType.Cv32F, 4);
        using Mat sampled = new();
        CvInvoke.CopyMakeBorder(source, wide, 0, 0, pad, pad, Wrap.Across.Border, default);
        CvInvoke.CopyMakeBorder(wide, padded, pad, pad, 0, 0, Wrap.Down.Border, default);
        Span2D<Vector4> shares = weights.View<Vector4>();
        Vector2 shift = new(pad - original.Origin.X - 0.5f, pad - original.Origin.Y - 0.5f);
        using SpanOwner<float> coverage = SpanOwner<float>.Allocate(size.Width);
        for (int row = 0; row < size.Height; row++) {
            Span<Vector2> traced = map.View<Vector2>().GetRowSpan(row);
            Traced(destination.Origin.X, destination.Origin.Y + row, destination.Extent, original.Extent, traced, coverage.Span);
            for (int column = 0; column < size.Width; column++)
                (traced[column], shares[row, column]) = (traced[column] + shift, share * coverage.Span[column]);
        }
        using Mat empty = new();
        CvInvoke.Remap(padded, sampled, map, empty, Sampling.Interpolation, BorderType.Constant, default);
        CvInvoke.Multiply(sampled, weights, sampled);
        CvInvoke.Add(sum, sampled, sum);
    }

    private (double Point, double Weight) Folded(double coordinate, WrapMode wrap, int size) =>
        wrap.Fold(Sampling.Snapped(wrap.Tiles ? wrap.Fold(coordinate, size, 0d).Point : coordinate), size, 0d);

    private void Traced(int x, int y, PixelExtent output, PixelExtent source, Span<Vector2> points, Span<float> coverage) {
        for (int column = 0; column < points.Length; column++) points[column] = new Vector2(x + column + 0.5f, y + 0.5f);
        coverage.Fill(1f);
        Trace(points, coverage, output, source);
        Bound(points, coverage, source);
    }

    private static double Travel(Func<double, (CoordinateMap Map, Vector4 Gains)> path, PixelExtent extent) {
        Span<Vector2> corners = stackalloc Vector2[12];
        Span<float> coverage = stackalloc float[4];
        double travel = 0d;
        for (int at = 0; at < corners.Length; at++) corners[at] = new Vector2((at & 1) is 0 ? 0.5f : extent.Width - 0.5f, (at & 2) is 0 ? 0.5f : extent.Height - 0.5f);
        for (int at = 0; at < 3; at++) path(at / 2d).Map.Trace(corners.Slice(4 * at, 4), coverage, extent, extent);
        for (int at = 0; at < 4; at++) travel = double.Max(travel, Vector2.Distance(corners[at], corners[at + 4]) + Vector2.Distance(corners[at + 4], corners[at + 8]));
        return travel;
    }

    private static Fin<Unit> Run(PixelFrame frame, Option<IProgress<int>> progress, Seq<(CoordinateMap Map, Vector4 Gains)> subframes) =>
        Try.lift(() => Sampled(frame, frame, subframes)).Run().Map(sampled => progress.Iter(report => report.Report(sampled.Size.Height)));

    private static PixelFrame Sampled(PixelFrame destination, PixelFrame original, Seq<(CoordinateMap Map, Vector4 Gains)> subframes) {
        Vector4 total = subframes.Fold(Vector4.Zero, static (sum, subframe) => sum + subframe.Gains);
        using Mat source = original.Premultiplied();
        using Mat sum = Mat.Zeros(destination.Size.Height, destination.Size.Width, DepthType.Cv32F, 4);
        foreach ((CoordinateMap map, Vector4 gains) in subframes) map.Accumulate(source, original, destination, gains / total, sum);
        Span2D<Vector4> pixels = destination.View.Span;
        sum.View<Vector4>().CopyTo(pixels);
        for (int row = 0; row < pixels.Height; row++) PixelFrame.Unassociated(pixels.GetRowSpan(row));
        return destination;
    }

    public sealed record Analytic(Action<Span<Vector2>, Span<float>, PixelExtent> Kernel, Sampling Sampling, (WrapMode Across, WrapMode Down) Wrap) : CoordinateMap(Sampling, Wrap) {
        internal override void Trace(Span<Vector2> points, Span<float> coverage, PixelExtent output, PixelExtent source) => Kernel(points, coverage, source);
    }

    public sealed record STMap(PixelFrame Coordinates, Sampling Sampling, (WrapMode Across, WrapMode Down) Wrap) : CoordinateMap(Sampling, Wrap) {
        internal override void Trace(Span<Vector2> points, Span<float> coverage, PixelExtent output, PixelExtent source) {
            (Vector2 into, Vector2 size) = (new Vector2(Coordinates.Extent.Width, Coordinates.Extent.Height) / new Vector2(output.Width, output.Height), new Vector2(source.Width, source.Height));
            for (int i = 0; i < points.Length; i++) {
                Vector4 read = Coordinates.Sample(points[i] * into, (WrapMode.Clamp, WrapMode.Clamp));
                (points[i], coverage[i]) = (new Vector2(read.X, 1f - read.Y) * size, coverage[i] * read.W);
            }
        }
    }

    public sealed record Offsets(PixelFrame Plane, Sampling Sampling, (WrapMode Across, WrapMode Down) Wrap) : CoordinateMap(Sampling, Wrap) {
        internal override void Trace(Span<Vector2> points, Span<float> coverage, PixelExtent output, PixelExtent source) {
            for (int i = 0; i < points.Length; i++) {
                Vector4 offset = Plane.Sample(points[i], (WrapMode.Clamp, WrapMode.Clamp));
                (points[i], coverage[i]) = (points[i] + new Vector2(offset.X, offset.Y), coverage[i] * offset.W);
            }
        }
    }

    public sealed record Composed(CoordinateMap First, CoordinateMap Second) : CoordinateMap(First.Sampling, First.Wrap) {
        internal override void Trace(Span<Vector2> points, Span<float> coverage, PixelExtent output, PixelExtent source) {
            Second.Trace(points, coverage, output, output);
            Second.Bound(points, coverage, output);
            First.Trace(points, coverage, output, source);
        }
    }

    public sealed record Planar(Matrix<double> Homography, Sampling Sampling, (WrapMode Across, WrapMode Down) Wrap) : CoordinateMap(Sampling, Wrap) {
        public Planar(Matrix3x2 transform, Sampling sampling, (WrapMode Across, WrapMode Down) wrap)
            : this(Matrix<double>.Build.DenseOfRowMajor(3, 3, [transform.M11, transform.M21, transform.M31, transform.M12, transform.M22, transform.M32, 0d, 0d, 1d]), sampling, wrap) { }

        public static Planar Pinned(FrameQuad quad, PixelExtent source, Sampling sampling, (WrapMode Across, WrapMode Down) wrap) =>
            new(Matrix<double>.Build.DenseOfDiagonalArray([source.Width, source.Height, 1d]) * quad.Homography.Inverse(), sampling, wrap);

        internal override void Trace(Span<Vector2> points, Span<float> coverage, PixelExtent output, PixelExtent source) {
            (Vector3 across, Vector3 down, Vector3 depth) = (Row(0), Row(1), Row(2));
            for (int i = 0; i < points.Length; i++) {
                Vector3 at = new(points[i], 1f);
                (points[i], coverage[i]) = Vector3.Dot(depth, at) switch {
                    > 0f and var w => (new Vector2(Vector3.Dot(across, at), Vector3.Dot(down, at)) / w, coverage[i]),
                    _ => (points[i], 0f),
                };
            }

            Vector3 Row(int row) => new((float)Homography[row, 0], (float)Homography[row, 1], (float)Homography[row, 2]);
        }

        private protected override void Accumulate(Mat source, PixelFrame original, PixelFrame destination, Vector4 share, Mat sum) {
            if (!original.Whole || Wrap.Across != Wrap.Down) {
                base.Accumulate(source, original, destination, share, sum);
                return;
            }
            using Mat sampled = new();
            using ScalarArray gains = new(new MCvScalar(share.X, share.Y, share.Z, share.W));
            using Mat perspective = new(3, 3, DepthType.Cv64F, 1);
            perspective.SetTo((Shift(-0.5d, -0.5d) * Homography * Shift(destination.Origin.X + 0.5d, destination.Origin.Y + 0.5d)).ToRowMajorArray());
            CvInvoke.WarpPerspective(source, sampled, perspective, new Size(destination.Size.Width, destination.Size.Height), Sampling.Interpolation, Warp.InverseMap, Wrap.Across.Border, default);
            CvInvoke.Multiply(sampled, gains, sampled);
            CvInvoke.Add(sum, sampled, sum);

            static Matrix<double> Shift(double x, double y) => Matrix<double>.Build.DenseOfRowMajor(3, 3, [1d, 0d, x, 0d, 1d, y, 0d, 0d, 1d]);
        }
    }

    public sealed record Reprojection(Projection Output, Projection Input, Quaternion Turn, Sampling Sampling, (WrapMode Across, WrapMode Down) Wrap) : CoordinateMap(Sampling, Wrap) {
        public static Option<Reprojection> Panorama(Camera camera, Func<Projection.Rectilinear, Projection> output, PanoramaView view, Sampling sampling, (WrapMode Across, WrapMode Down) wrap) =>
            from rendering in camera.Lens is Projection.Rectilinear rectilinear ? Some(rectilinear) : None
            let projection = output(rendering)
            let up = Vector3.Transform(Vector3.UnitZ, Quaternion.Conjugate(camera.Orientation))
            let level = Quaternion.CreateFromYawPitchRoll(0f, float.Atan2(-up.Z, float.Hypot(up.X, up.Y)), float.Atan2(up.X, up.Y))
            select new Reprojection(projection, rendering, Quaternion.Conjugate(projection.Upright ? view.Rotation * level : view.Rotation), sampling, wrap);

        public static Fin<Projection.Rectilinear> Fitted(Projection.Rectilinear distorted, float alpha) =>
            distorted.Distortion.Match(
                Some: lens => {
                    (int width, int height, Matrix3x2 normalized) = (distorted.Extent.Width, distorted.Extent.Height, distorted.Window.ToWindow(distorted.Extent) * Matrix3x2.CreateScale(1f, -1f));
                    ((Vector2 Low, Vector2 High) left, (Vector2 Low, Vector2 High) right, (Vector2 Low, Vector2 High) top, (Vector2 Low, Vector2 High) bottom) =
                        (Edge(height, static y => new Vector2(0f, y)), Edge(height, y => new Vector2(width, y)), Edge(width, static x => new Vector2(x, 0f)), Edge(width, x => new Vector2(x, height)));
                    Vector2 low = Vector2.Lerp(new Vector2(left.High.X, top.High.Y), Vector2.Min(Vector2.Min(left.Low, right.Low), Vector2.Min(top.Low, bottom.Low)), alpha);
                    Vector2 high = Vector2.Lerp(new Vector2(right.Low.X, bottom.Low.Y), Vector2.Max(Vector2.Max(left.High, right.High), Vector2.Max(top.High, bottom.High)), alpha);
                    return ViewWindow.Validated((low + high) / new Vector2(2f, -2f), high - low).Map(fitted => distorted with { Window = fitted, Distortion = None });

                    (Vector2 Low, Vector2 High) Edge(int count, Func<int, Vector2> at) =>
                        toSeq(Enumerable.Range(0, count + 1)).Map(i => lens.Ideal(Vector2.Transform(at(i), normalized))).Somes()
                            .Fold((Low: new Vector2(float.PositiveInfinity), High: new Vector2(float.NegativeInfinity)), static (bounds, point) => (Vector2.Min(bounds.Low, point), Vector2.Max(bounds.High, point)));
                },
                None: () => distorted);

        public static PixelPass.Frame Dispersed(Projection.Rectilinear output, Projection.Rectilinear input, double dispersion, Sampling sampling, (WrapMode Across, WrapMode Down) wrap) {
            return Integrated(
                t => (Shifted(double.Lerp(-dispersion / 4d, 0d, t)), new Vector4((float)(1d - t), (float)t, 0f, 1f)),
                t => (Shifted(double.Lerp(0d, dispersion / 4d, t)), new Vector4(0f, (float)(1d - t), (float)t, 1f)));

            Reprojection Shifted(double shift) => new(Lensed(output, shift), Lensed(input, shift), Quaternion.Identity, sampling, wrap);

            static Projection.Rectilinear Lensed(Projection.Rectilinear projection, double shift) =>
                projection with { Distortion = projection.Distortion.Map(lens => new LensDistortion(lens.K1 + shift, lens.K2, lens.P1, lens.P2, lens.K3)) };
        }

        internal override void Trace(Span<Vector2> points, Span<float> coverage, PixelExtent output, PixelExtent source) {
            for (int i = 0; i < points.Length; i++) {
                (points[i], float kept) = Output.Reprojected(Input, Turn, points[i]).Map(static point => (point, 1f)).IfNone((points[i], 0f));
                coverage[i] *= kept;
            }
        }
    }

    public sealed record Field(Func<Vector2, Vector2> Displacement, Vector2 Center, Sampling Sampling, (WrapMode Across, WrapMode Down) Wrap) : CoordinateMap(Sampling, Wrap) {
        public static Field Twirl(float angle, ShortSideLength radius, float falloff, int arms, float armOffset, Vector2 center, Sampling sampling, (WrapMode Across, WrapMode Down) wrap) =>
            new(d => Vector2.Transform(d, Matrix3x2.CreateRotation(angle * DistanceField.SmoothStep(0f, 1f, 1f - float.Pow(d.Length() / radius, 1f / falloff)) * (0.5f + (0.5f * float.Cos(arms * (float.Atan2(-d.Y, d.X) - armOffset)))))), center, sampling, wrap);

        public static Field Bulge(float amount, Vector2 axes, ShortSideLength radius, Vector2 center, Sampling sampling, (WrapMode Across, WrapMode Down) wrap) =>
            new(d => (d * axes) switch {
                var v when v.Length() / radius is var u and > 0f and < 1f =>
                    d + (v * (((amount >= 0f ? float.Lerp(u, 2f / float.Pi * float.Asin(u), amount) : float.Lerp(u, float.SinPi(u / 2f), -amount)) / u) - 1f)),
                _ => d,
            }, center, sampling, wrap);

        public static Fin<Field> Wave(
            WaveProfile profile, int count, (ShortSideLength Low, ShortSideLength High) wavelength, (float Low, float High) amplitude, Vector2 amount, float direction, float spread, float shift,
            Option<(Angle Turn, ShortSideLength Radius)> rings, uint field, Vector2 center, Sampling sampling, (WrapMode Across, WrapMode Down) wrap) =>
            ImagingFault.Unless(count >= 1, ImagingReason.Range, count).Map(_ => {
                (Matrix3x2 turn, float reach) = rings.Match(Some: static ring => (Matrix3x2.CreateRotation(-(float)ring.Turn.Radians), (float)ring.Radius), None: static () => (Matrix3x2.CreateRotation(-float.Pi / 2f), float.PositiveInfinity));
                (Vector2 Along, float Length, float Level, float Phase)[] waves = [.. Enumerable.Range(0, count).Select(i =>
                    (CoordinateHash.Uniforms(Vector128<int>.Zero, CoordinateHash.Branch(field, (uint)i)), float.SinCos(direction + (spread * (i - ((count - 1) / 2f)) / int.Max(count - 1, 1)))) switch {
                        var (draw, (sin, cos)) => (new Vector2(cos, -sin), float.Lerp(wavelength.Low, wavelength.High, draw.X), float.Lerp(amplitude.Low, amplitude.High, draw.Y), draw.Z + shift),
                    })];
                return new Field(d => {
                    (float distance, Vector2 sum, Vector2 spoke) = (d.Length(), Vector2.Zero, d == Vector2.Zero ? Vector2.UnitX : Vector2.Normalize(d));
                    foreach ((Vector2 along, float length, float level, float phase) in waves) {
                        Vector2 axis = rings.IsSome ? spoke : along;
                        sum += Vector2.TransformNormal(axis, turn) * level * ((2f * profile.Level((Vector2.Dot(d, axis) / length) + phase)) - 1f);
                    }
                    return d + (amount * sum * float.Max(0f, 1f - (distance / reach)) / count);
                }, center, sampling, wrap);
            });

        public static Field Turbulence(Func<Vector4, uint, float> noise, (ShortSideLength Across, ShortSideLength Down) scale, Vector2 amount, float depth, uint field, Vector2 center, Sampling sampling, (WrapMode Across, WrapMode Down) wrap) =>
            (new Vector2(scale.Across, -(float)scale.Down), CoordinateHash.Branch(field, 0u), CoordinateHash.Branch(field, 1u), CoordinateHash.Branch(field, 2u), CoordinateHash.Branch(field, 3u)) switch {
                var (cell, right, left, down, up) => new(d => new Vector4(d / cell, depth, 0f) switch {
                    var at => d + (amount * new Vector2(noise(at, right) - noise(at, left), noise(at, down) - noise(at, up))),
                }, center, sampling, wrap),
            };

        public static Fin<Field> Kaleidoscope(int count, float rotation, float zoom, Vector2 center, Sampling sampling, (WrapMode Across, WrapMode Down) wrap) =>
            ImagingFault.Unless(count >= 1 && float.IsFinite(rotation) && zoom > 0f && float.IsFinite(zoom), ImagingReason.Range, (count, rotation, zoom)).Map(_ => (rotation + (float.Pi / count), float.Tau / count) switch {
                var (axis, period) => new Field(d => float.SinCos(axis - float.Abs(float.Ieee754Remainder(float.Atan2(-d.Y, d.X) - axis, period))) switch {
                    var (sin, cos) => new Vector2(cos, -sin) * d.Length() / zoom,
                }, center, sampling, wrap),
            });

        internal override void Trace(Span<Vector2> points, Span<float> coverage, PixelExtent output, PixelExtent source) {
            (Vector2 center, float side) = (source.Point(Center), source.ShortSide);
            foreach (ref Vector2 point in points) point = center + (Displacement((point - center) / side) * side);
        }
    }

    public sealed record Polar(bool Inverse, Vector2 Center, Sampling Sampling, (WrapMode Across, WrapMode Down) Wrap) : CoordinateMap(Sampling, Wrap) {
        internal override void Trace(Span<Vector2> points, Span<float> coverage, PixelExtent output, PixelExtent source) {
            (Vector2 center, Vector2 size, float side) = (source.Point(Center), new Vector2(source.Width, source.Height), source.ShortSide);
            foreach (ref Vector2 point in points)
                point = Inverse
                    ? ((point - center) / side) switch { var d => new Vector2((float.Pi + float.Atan2(-d.X, d.Y)) / float.Tau, 2f * d.Length()) * size }
                    : float.SinCos(float.Tau * point.X / size.X) switch { var (sin, cos) => center + (new Vector2(sin, -cos) * side * point.Y / (2f * size.Y)) };
        }
    }
}

[ComplexValueObject(SkipFactoryMethods = true)]
public sealed partial class Pose {
    public Vector2 Offset { get; }
    public float Rotation { get; }
    public Vector2 Scale { get; }
    public Vector2 Skew { get; }
    public Vector2 Pivot { get; }

    public Matrix3x2 Inverse =>
        Matrix3x2.CreateTranslation(-Offset) * Matrix3x2.CreateRotation(Rotation, Pivot) * Matrix3x2.CreateSkew(0f, -Skew.Y, Pivot) * Matrix3x2.CreateSkew(-Skew.X, 0f, Pivot) * Matrix3x2.CreateScale(Vector2.One / Scale, Pivot);

    public Pose Partial(float share) => new(Offset * share, Rotation * share, new Vector2(float.Pow(Scale.X, share), float.Pow(Scale.Y, share)), Skew * share, Pivot);

    public static Fin<Pose> Validated(Vector2 offset, float rotation, Vector2 scale, Vector2 skew, Vector2 pivot) =>
        ImagingFault.Unless(Vector2.AllWhereAllBitsSet(Vector2.GreaterThan(scale, Vector2.Zero) & Vector2.LessThan(Vector2.Abs(skew), new Vector2(float.Pi / 2f))), ImagingReason.Range, (scale, skew)).Map(_ => new Pose(offset, rotation, scale, skew, pivot));

    public static Fin<Pose> Shaken(PixelExtent extent, float time, float rate, Vector2 amplitude, float rotation, float zoom, Octaves octaves, uint field) {
        return Validated(amplitude * extent.ShortSide * new Vector2(Channel(0u), Channel(1u)), rotation * Channel(2u), new Vector2(float.Pow(zoom, Channel(3u))), Vector2.Zero, extent.Point(new Vector2(0.5f)));

        float Channel(uint axis) => Fractal.Fbm.Sample(NoiseDimensions.One, new Vector4(time * rate, 0f, 0f, 0f), octaves, 0f, 0f, CoordinateHash.Branch(field, axis));
    }
}
