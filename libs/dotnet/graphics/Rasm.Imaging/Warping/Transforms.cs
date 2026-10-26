using System.Globalization;
using System.Numerics;
using MathNet.Numerics.Integration;
using MathNet.Numerics.LinearAlgebra;
using PeterO.Numbers;
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

    static partial void ValidateFactoryArguments(ref ImagingFault? validationError, ref int mirrors, ref double rotation, ref FramePosition center, ref double zoom) =>
        validationError = mirrors >= 1 && double.IsFinite(rotation) && double.IsFinite(zoom) && zoom > 0d ? null
            : new ImagingFault(string.Create(CultureInfo.InvariantCulture, $"Mirror fold of {mirrors} mirrors must have at least 1, finite rotation {rotation} and positive zoom {zoom}"));
}

public readonly record struct Affine(Matrix<double> Linear, (ERational X, ERational Y) Shift) {
    public static Affine Identity { get; } = new(Matrix<double>.Build.DenseIdentity(2), (ERational.Zero, ERational.Zero));

    public bool Still => Linear.Equals(Identity.Linear) && Shift.X.IsZero && Shift.Y.IsZero;

    public Affine Then(Affine next) =>
        new(Linear * next.Linear, (
            (Shift.X * ERational.FromDouble(next.Linear[0, 0])) + (Shift.Y * ERational.FromDouble(next.Linear[1, 0])) + next.Shift.X,
            (Shift.X * ERational.FromDouble(next.Linear[0, 1])) + (Shift.Y * ERational.FromDouble(next.Linear[1, 1])) + next.Shift.Y));

    public CoordinateMap.Analytic Map(Sampling sampling, WrapMode wrap) {
        ((ERational xx, ERational xy, ERational yx, ERational yy), (ERational X, ERational Y) shift) =
            ((ERational.FromDouble(Linear[0, 0]), ERational.FromDouble(Linear[0, 1]), ERational.FromDouble(Linear[1, 0]), ERational.FromDouble(Linear[1, 1])), Shift);
        return new((points, _, extent) => {
            foreach (ref (double X, double Y) point in points) {
                (ERational x, ERational y) = (ERational.FromDouble(point.X), ERational.FromDouble(point.Y));
                point = (sampling.Point((x * xx) + (y * yx) + shift.X, wrap, extent.Width), sampling.Point((x * xy) + (y * yy) + shift.Y, wrap, extent.Height));
            }
        }, sampling, (wrap, wrap));
    }

    public static PixelPass.Frame Swept(Func<double, Affine> path, Sampling sampling, WrapMode wrap) =>
        new(CoordinateMap.Integrated(new GaussLegendreRule(0d, 1d, 1 << 6), share => (path(share).Map(sampling, wrap), Vector4.One)));
}

public readonly record struct Pose((ERational X, ERational Y) Offset, double Rotation, (double X, double Y) Scale, (double X, double Y) Skew, (double X, double Y) Pivot) {
    public Pose Partial(double share) => ERational.FromDouble(share) switch {
        var part => new((Offset.X * part, Offset.Y * part), Rotation * share, (double.Exp(share * double.Log(Scale.X)), double.Exp(share * double.Log(Scale.Y))), (Skew.X * share, Skew.Y * share), Pivot),
    };

    public Affine Inverse => double.SinCos(Rotation) switch {
        var (sin, cos) => (Matrix<double>.Build.Dense(2, 2, [cos, -sin, sin, cos])
            * Matrix<double>.Build.Dense(2, 2, [1d, 0d, -double.Tan(Skew.Y), 1d])
            * Matrix<double>.Build.Dense(2, 2, [1d, -double.Tan(Skew.X), 0d, 1d])
            * Matrix<double>.Build.Dense(2, 2, [1d / Scale.X, 0d, 0d, 1d / Scale.Y])) switch {
                var linear => (ERational.FromDouble(Pivot.X), ERational.FromDouble(Pivot.Y)) switch {
                    var (x, y) => new Affine(linear, (x - ((x + Offset.X) * ERational.FromDouble(linear[0, 0])) - ((y + Offset.Y) * ERational.FromDouble(linear[1, 0])),
                        y - ((x + Offset.X) * ERational.FromDouble(linear[0, 1])) - ((y + Offset.Y) * ERational.FromDouble(linear[1, 1])))),
                },
            },
    };
}

public sealed record ShakePath((ShortSideLength X, ShortSideLength Y) Amplitude, double Rotation, double Zoom, double Rate, Octaves Octaves, uint Field) {
    public Pose Posed(PixelExtent extent, double time) {
        Vector4 at = new((float)(time * Rate), 0f, 0f, 0f);
        double zoom = double.Pow(Zoom, Channel(3u));
        return new((ERational.FromSingle(Amplitude.X).Multiply(extent.ShortSide) * ERational.FromSingle(Channel(0u)), ERational.FromSingle(Amplitude.Y).Multiply(extent.ShortSide) * ERational.FromSingle(Channel(1u))),
            Rotation * Channel(2u), (zoom, zoom), (0d, 0d), FramePosition.Center.Point(extent));

        float Channel(uint axis) => NoiseDimensions.One.Fbm(at, Octaves, CoordinateHash.Branch(Field, axis));
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class QuadWarp {
    extension(FrameQuad quad) {
        public CoordinateMap.Analytic Pinned(Sampling sampling, (WrapMode Across, WrapMode Down) wrap) =>
            quad.Homography.Inverse switch {
                var inverse => new((points, coverage, source) => {
                    for (int i = 0; i < points.Length; i++) {
                        float weight = coverage[i];
                        (points[i], coverage[i]) = inverse.Apply(points[i]).Map(uv => ((uv.X * source.Width, uv.Y * source.Height), weight)).IfNone((points[i], 0f));
                    }
                }, sampling, wrap),
            };
    }
}
