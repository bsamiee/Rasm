using System.Globalization;
using System.Numerics;
using System.Runtime.Intrinsics;
using CommunityToolkit.HighPerformance;
using Emgu.CV;
using Emgu.CV.CvEnum;
using MathNet.Numerics.Integration;
using MathNet.Numerics.LinearAlgebra;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Procedural;

namespace Rasm.Imaging.Warping;

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
[ValidationError<ImagingFault>]
public sealed partial class MirrorFold {
    public int Mirrors { get; }
    public double Rotation { get; }
    public FramePosition Center { get; }
    public double Zoom { get; }

    public CoordinateMap.Analytic Map(Sampling sampling, (WrapMode Across, WrapMode Down) wrap) {
        (double axis, double period, double zoom, FramePosition center) = (Rotation + (double.Pi / Mirrors), double.Tau / Mirrors, Zoom, Center);
        return new((points, _, extent) => {
            (double x, double y) = center.Point(extent);
            foreach (ref (double X, double Y) point in points) point = (point.X - x, point.Y - y) switch {
                var (across, down) => (double.Hypot(across, down) / zoom, double.SinCos(axis - double.Abs(double.Ieee754Remainder(double.Atan2(-down, across) - axis, period)))),
            } switch {
                var (radius, (sin, cos)) => (x + (radius * cos), y - (radius * sin)),
            };
        }, sampling, wrap);
    }

    public static Fin<MirrorFold> Validated(int mirrors, double rotation, FramePosition center, double zoom) =>
        TryCreate(mirrors, rotation, center, zoom, out MirrorFold? fold, out ImagingFault? fault) ? fold : fault;

    static partial void ValidateFactoryArguments(ref ImagingFault? validationError, ref int mirrors, ref double rotation, ref FramePosition center, ref double zoom) =>
        validationError = mirrors >= 1 && double.IsFinite(rotation) && double.IsFinite(zoom) && zoom > 0d ? null
            : new ImagingFault(string.Create(CultureInfo.InvariantCulture, $"mirror fold of {mirrors} mirrors, rotation {rotation}, and zoom {zoom}"), ImagingReason.Range);
}

public sealed record Affine(Matrix<double> Linear, (double X, double Y) Shift) {
    public static Affine Identity { get; } = new(Matrix<double>.Build.DenseIdentity(2), (0d, 0d));

    public bool Still => Linear.Equals(Identity.Linear) && Shift is (0d, 0d);

    public Affine Then(Affine next) =>
        new(Linear * next.Linear, (
            (Shift.X * next.Linear[0, 0]) + (Shift.Y * next.Linear[1, 0]) + next.Shift.X,
            (Shift.X * next.Linear[0, 1]) + (Shift.Y * next.Linear[1, 1]) + next.Shift.Y));

    public CoordinateMap.Analytic Map(Sampling sampling, WrapMode wrap) {
        ((double xx, double xy, double yx, double yy), (double X, double Y) shift) = ((Linear[0, 0], Linear[0, 1], Linear[1, 0], Linear[1, 1]), Shift);
        return new((points, _, extent) => {
            foreach (ref (double X, double Y) point in points)
                point = (sampling.Point((point.X * xx) + (point.Y * yx) + shift.X, wrap, extent.Width), sampling.Point((point.X * xy) + (point.Y * yy) + shift.Y, wrap, extent.Height));
        }, sampling, (wrap, wrap));
    }

    public static PixelPass.Frame Swept(Func<double, Affine> path, Sampling sampling, WrapMode wrap) =>
        new(CoordinateMap.Integrated(new GaussLegendreRule(0d, 1d, 1 << 6), share => (path(share).Map(sampling, wrap), Vector4.One)));
}

public sealed record Pose((double X, double Y) Offset, double Rotation, (double X, double Y) Scale, (double X, double Y) Skew, (double X, double Y) Pivot) {
    public Pose Partial(double share) =>
        new((Offset.X * share, Offset.Y * share), Rotation * share, (double.Exp(share * double.Log(Scale.X)), double.Exp(share * double.Log(Scale.Y))), (Skew.X * share, Skew.Y * share), Pivot);

    public Affine Inverse => double.SinCos(Rotation) switch {
        var (sin, cos) => (Matrix<double>.Build.Dense(2, 2, [cos, -sin, sin, cos])
            * Matrix<double>.Build.Dense(2, 2, [1d, 0d, -double.Tan(Skew.Y), 1d])
            * Matrix<double>.Build.Dense(2, 2, [1d, -double.Tan(Skew.X), 0d, 1d])
            * Matrix<double>.Build.Dense(2, 2, [1d / Scale.X, 0d, 0d, 1d / Scale.Y])) switch {
                var linear => Pivot switch {
                    var (x, y) => new Affine(linear, (x - ((x + Offset.X) * linear[0, 0]) - ((y + Offset.Y) * linear[1, 0]),
                        y - ((x + Offset.X) * linear[0, 1]) - ((y + Offset.Y) * linear[1, 1]))),
                },
            },
    };
}

public sealed record ShakePath((ShortSideLength X, ShortSideLength Y) Amplitude, double Rotation, double Zoom, double Rate, Octaves Octaves, uint Field) {
    public Pose Posed(PixelExtent extent, double time) {
        Vector256<double> at = Vector256.Create(time * Rate, 0d, 0d, 0d);
        double zoom = double.Pow(Zoom, Channel(3u));
        return new((Amplitude.X.Pixels(extent) * Channel(0u), Amplitude.Y.Pixels(extent) * Channel(1u)), Rotation * Channel(2u), (zoom, zoom), (0d, 0d), FramePosition.Center.Point(extent));

        float Channel(uint axis) => NoiseDimensions.One.Fbm(at, Octaves, CoordinateHash.Branch(Field, axis));
    }
}

[SmartEnum<string>]
public sealed partial class PolarConversion {
    public static readonly PolarConversion RectangularToPolar = new("rectangular-to-polar", static (point, extent) =>
        FramePosition.Center.Point(extent) switch {
            var center => ((point.X - center.X) / extent.ShortSide, (point.Y - center.Y) / extent.ShortSide) switch {
                var (x, y) => (extent.Width * (double.Pi + double.Atan2(-x, y)) / double.Tau, 2d * extent.Height * double.Hypot(x, y)),
            },
        });
    public static readonly PolarConversion PolarToRectangular = new("polar-to-rectangular", static (point, extent) =>
        (double.SinCos(double.Tau * point.X / extent.Width), FramePosition.Center.Point(extent), extent.ShortSide * point.Y / (2d * extent.Height)) switch {
            var ((sin, cos), center, radius) => (center.X + (radius * sin), center.Y - (radius * cos)),
        });

    public CoordinateMap.Analytic Map(Sampling sampling, (WrapMode Across, WrapMode Down) wrap) =>
        new((points, _, extent) => {
            foreach (ref (double X, double Y) point in points) point = Source(point, extent);
        }, sampling, wrap);

    [UseDelegateFromConstructor]
    public partial (double X, double Y) Source((double X, double Y) point, PixelExtent extent);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class QuadWarp {
    extension(FrameQuad quad) {
        public PixelPass.Frame Pinned(Sampling sampling, WrapMode wrap) =>
            new((frame, progress) => frame.Filter(target => {
                (double left, double top) = (frame.Origin.X + 0.5d, frame.Origin.Y + 0.5d);
                Matrix<double> map = Matrix<double>.Build.DenseOfRowMajor(3, 3, [frame.Extent.Width, 0d, -left, 0d, frame.Extent.Height, -top, 0d, 0d, 1d])
                    * quad.Homography.Inverse()
                    * Matrix<double>.Build.DenseOfRowMajor(3, 3, [1d, 0d, left, 0d, 1d, top, 0d, 0d, 1d]);
                using Mat source = CoordinateMap.Premultiplied(frame);
                using Mat perspective = new(3, 3, DepthType.Cv64F, 1);
                perspective.SetTo(map.ToRowMajorArray());
                CvInvoke.WarpPerspective(source, target, perspective, source.Size, sampling.Interpolation, Warp.InverseMap, wrap.Border, default);
                Span2D<Vector4> pixels = target.View<Vector4>();
                for (int row = 0; row < pixels.Height; row++) {
                    for (int column = 0; column < pixels.Width; column++)
                        pixels[row, column] = (map[2, 0] * column) + (map[2, 1] * row) + map[2, 2] > 0d ? CoordinateMap.Unassociated(pixels[row, column]) : Vector4.Zero;
                }
                progress.Report(frame.Size.Height);
                return unit;
            }));
    }
}

public static class Reprojection {
    extension(Projection output) {
        public CoordinateMap.Analytic From(Projection input, Quaternion turn, Sampling sampling, (WrapMode Across, WrapMode Down) wrap) =>
            new((points, coverage, _) => {
                for (int i = 0; i < points.Length; i++) {
                    float weight = coverage[i];
                    (points[i], coverage[i]) = output.Reprojected(input, turn, points[i]).Map(point => (point, weight)).IfNone((points[i], 0f));
                }
            }, sampling, wrap);

        public PixelFrame Viewed(PixelFrame frame, Projection source, PanoramaView place) =>
            output.From(source, place.Rotation, Sampling.Linear, source.Wrap).Resample(frame, output.Extent);
    }

    extension(Camera camera) {
        public Option<CoordinateMap.Analytic> Panorama(Func<Projection.Rectilinear, Projection> output, PanoramaView view, Sampling sampling, (WrapMode Across, WrapMode Down) wrap) =>
            from rendering in camera.Frustum.Switch(camera.Extent,
                perspective: static (extent, lens) => Some(new Projection.Rectilinear(extent, lens)), parallel: static (_, _) => Option<Projection.Rectilinear>.None)
            let projection = output(rendering)
            let up = Vector3.Transform(Vector3.UnitZ, Quaternion.Conjugate(camera.Orientation))
            let level = Quaternion.CreateFromYawPitchRoll(0f, float.Atan2(-up.Z, float.Hypot(up.X, up.Y)), float.Atan2(up.X, up.Y))
            select projection.From(rendering, Quaternion.Conjugate(projection.Upright ? view.Rotation * level : view.Rotation), sampling, wrap);
    }
}
