using System.Drawing;
using System.Numerics;
using Emgu.CV;
using MathNet.Numerics.LinearAlgebra;
using UnitsNet;

namespace Rasm.Imaging.Pixels;

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
[ValidationError<ImagingFault>]
public sealed partial class FrameQuad {
    public (double X, double Y) TopLeft { get; }
    public (double X, double Y) TopRight { get; }
    public (double X, double Y) BottomRight { get; }
    public (double X, double Y) BottomLeft { get; }

    public bool Mirrored => Turn(BottomLeft, TopLeft, TopRight) < 0d;

    public FrameQuad FlippedHorizontal => new(TopRight, TopLeft, BottomLeft, BottomRight);
    public FrameQuad FlippedVertical => new(BottomLeft, BottomRight, TopRight, TopLeft);
    public FrameQuad Turned => new(BottomLeft, TopLeft, TopRight, BottomRight);

    public Matrix<double> Homography {
        get {
            using Mat map = CvInvoke.GetPerspectiveTransform([new(0f, 0f), new(1f, 0f), new(1f, 1f), new(0f, 1f)], [Corner(TopLeft), Corner(TopRight), Corner(BottomRight), Corner(BottomLeft)]);
            double[] cells = new double[9];
            map.CopyTo(cells);
            return Matrix<double>.Build.DenseOfRowMajor(3, 3, cells);
        }
    }

    public static Fin<FrameQuad> Validated((double X, double Y) topLeft, (double X, double Y) topRight, (double X, double Y) bottomRight, (double X, double Y) bottomLeft) =>
        TryCreate(topLeft, topRight, bottomRight, bottomLeft, out FrameQuad? quad, out ImagingFault? fault) ? quad : fault;

    private static PointF Corner((double X, double Y) corner) => new((float)corner.X, (float)corner.Y);

    private static double Turn((double X, double Y) previous, (double X, double Y) corner, (double X, double Y) next) =>
        ((corner.X - previous.X) * (next.Y - corner.Y)) - ((corner.Y - previous.Y) * (next.X - corner.X));

    static partial void ValidateFactoryArguments(ref ImagingFault? validationError, ref (double X, double Y) topLeft, ref (double X, double Y) topRight, ref (double X, double Y) bottomRight, ref (double X, double Y) bottomLeft) =>
        validationError = (Turn(bottomLeft, topLeft, topRight), Turn(topLeft, topRight, bottomRight), Turn(topRight, bottomRight, bottomLeft), Turn(bottomRight, bottomLeft, topLeft))
                is ( > 0d, > 0d, > 0d, > 0d) or ( < 0d, < 0d, < 0d, < 0d) ? null : new ImagingFault("Frame quad corners must be convex");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Projection {
    private Projection(PixelExtent extent) => Extent = extent;

    public PixelExtent Extent { get; }
    public virtual bool Upright => false;
    public virtual (WrapMode Across, WrapMode Down) Wrap => (WrapMode.Black, WrapMode.Black);

    public Option<Vector3> Ray((double X, double Y) point) => double.IsFinite(point.X) && double.IsFinite(point.Y) ? Direction(point) : None;
    public Option<(double X, double Y)> Pixel(Vector3 ray) =>
        float.IsFinite(ray.X) && float.IsFinite(ray.Y) && float.IsFinite(ray.Z) && ray != Vector3.Zero ? FramePoint(ray) : None;

    private protected abstract Option<Vector3> Direction((double X, double Y) point);
    private protected abstract Option<(double X, double Y)> FramePoint(Vector3 ray);

    public Option<(double X, double Y)> Reprojected(Projection target, Quaternion turn, (double X, double Y) point) =>
        Ray(point).Bind(ray => target.Pixel(Vector3.Transform(ray, turn)));

    private double Circle => (double)Extent.ShortSide / Extent.Width;

    private (double X, double Y) Centered((double X, double Y) point) => ((point.X / Extent.Width) - 0.5d, ((Extent.Height / 2d) - point.Y) / Extent.Width);
    private (double X, double Y) Uncentered((double X, double Y) centered) => ((centered.X + 0.5d) * Extent.Width, (Extent.Height / 2d) - (centered.Y * Extent.Width));

    private Option<Vector3> Radial((double X, double Y) point, double cut, double scale, Func<double, double, double> angle) =>
        Centered(point) switch {
            var centered => angle(double.Hypot(centered.X, centered.Y), scale) switch {
                var theta when theta <= cut => (double.SinCos(double.Atan2(centered.Y, centered.X)), double.SinCos(theta)) switch {
                    var (azimuth, inclination) => Some(new Vector3((float)(azimuth.Cos * inclination.Sin), (float)(azimuth.Sin * inclination.Sin), (float)-inclination.Cos)),
                },
                _ => None,
            },
        };
    private Option<(double X, double Y)> RadialPoint(Vector3 ray, double cut, double scale, Func<double, double, double> radius) =>
        double.Atan2(double.Hypot(ray.X, ray.Y), -ray.Z) switch {
            var theta when theta <= cut => (double.SinCos(double.Atan2(ray.Y, ray.X)), radius(theta, scale)) switch {
                var (azimuth, reach) => Some(Uncentered((azimuth.Cos * reach, azimuth.Sin * reach))),
            },
            _ => None,
        };

    private static double SolidAngle(double radius, double reach) => 2d * double.Asin(radius / reach);
    private static double SolidRadius(double angle, double reach) => reach * double.Sin(angle / 2d);

    public sealed record Rectilinear(PixelExtent Extent, Frustum.Perspective Frustum) : Projection(Extent) {
        private protected override Option<Vector3> Direction((double X, double Y) point) => Some(Frustum.Ray(point, Extent).Direction);
        private protected override Option<(double X, double Y)> FramePoint(Vector3 ray) => Frustum.Locate(ray, Extent).Map(static located => located.Point);
    }

    public sealed record Equirectangular(PixelExtent Extent) : Projection(Extent) {
        public override bool Upright => true;
        public override (WrapMode Across, WrapMode Down) Wrap => (WrapMode.Periodic, WrapMode.Clamp);

        private protected override Option<Vector3> Direction((double X, double Y) point) =>
            (double.SinCos(((point.X % Extent.Width / Extent.Width) - 0.5d) * double.Tau), double.SinCos((0.5d - (point.Y % (2d * Extent.Height) / Extent.Height)) * double.Pi)) switch {
                var (longitude, latitude) => Some(new Vector3((float)(longitude.Sin * latitude.Cos), (float)latitude.Sin, (float)(-longitude.Cos * latitude.Cos))),
            };

        private protected override Option<(double X, double Y)> FramePoint(Vector3 ray) =>
            Some((
                ((double.Atan2(ray.X, -ray.Z) / double.Tau) + 0.5d) * Extent.Width,
                (0.5d - double.Atan2Pi(ray.Y, double.Hypot(ray.X, ray.Z))) * Extent.Height));
    }

    public sealed record Cylindrical(PixelExtent Extent, ViewWindow Window) : Projection(Extent) {
        public override bool Upright => true;

        private protected override Option<Vector3> Direction((double X, double Y) point) =>
            from window in Some(Window.WindowPoint(point.X, point.Y, Extent))
            from longitude in double.IsFinite(window.X) ? Some(window.X) : None
            select (double.SinCos(longitude), double.SinCos(double.Atan(window.Y))) switch {
                var (across, latitude) => new Vector3((float)(across.Sin * latitude.Cos), (float)latitude.Sin, (float)(-across.Cos * latitude.Cos)),
            };

        private protected override Option<(double X, double Y)> FramePoint(Vector3 ray) =>
            double.Hypot(ray.X, ray.Z) switch {
                > 0d and var radius => Window.FramePoint((double.Atan2(ray.X, -ray.Z), ray.Y / radius), Extent),
                _ => None,
            };
    }

    public sealed record Fisheye(PixelExtent Extent, float Sweep) : Projection(Extent) {
        private protected override Option<Vector3> Direction((double X, double Y) point) => Radial(point, Sweep / 2d, Sweep / Circle, static (radius, scale) => radius * scale);
        private protected override Option<(double X, double Y)> FramePoint(Vector3 ray) => RadialPoint(ray, Sweep / 2d, Sweep / Circle, static (angle, scale) => angle / scale);
    }

    public sealed record Equisolid(PixelExtent Extent, float Sweep, float FullTurn) : Projection(Extent) {
        private protected override Option<Vector3> Direction((double X, double Y) point) => Radial(point, Sweep / 2d, Circle * FullTurn / 2d, SolidAngle);
        private protected override Option<(double X, double Y)> FramePoint(Vector3 ray) => RadialPoint(ray, Sweep / 2d, Circle * FullTurn / 2d, SolidRadius);
    }

    public sealed record MirrorBall(PixelExtent Extent) : Projection(Extent) {
        private protected override Option<Vector3> Direction((double X, double Y) point) => Radial(point, double.Pi, Circle / 2d, SolidAngle);
        private protected override Option<(double X, double Y)> FramePoint(Vector3 ray) => RadialPoint(ray, double.Pi, Circle / 2d, SolidRadius);
    }

    public sealed record Stereographic(PixelExtent Extent, float Sweep) : Projection(Extent) {
        private protected override Option<Vector3> Direction((double X, double Y) point) =>
            from centered in Some(Centered(point))
            let inverse = 1d / (2d * double.Tan(Sweep / 4d))
            let magnitude = double.Max(inverse, double.Max(double.Abs(centered.X), double.Abs(centered.Y)))
            let local = (X: centered.X / magnitude, Y: centered.Y / magnitude, Z: inverse / magnitude)
            let planar = (local.X * local.X) + (local.Y * local.Y)
            let axial = local.Z * local.Z
            let denominator = planar + axial
            select new Vector3((float)(2d * local.X * local.Z / denominator), (float)(2d * local.Y * local.Z / denominator), (float)((planar - axial) / denominator));

        private protected override Option<(double X, double Y)> FramePoint(Vector3 ray) =>
            from planar in Some(double.Hypot(ray.X, ray.Y))
            where planar > 0d || ray.Z < 0f
            let length = double.Hypot(planar, ray.Z)
            let radius = ray.Z > 0f ? (length + ray.Z) / planar : planar / (length - ray.Z)
            let reach = radius / (2d * double.Tan(Sweep / 4d))
            let azimuth = double.SinCos(double.Atan2(ray.Y, ray.X))
            select Uncentered((azimuth.Cos * reach, azimuth.Sin * reach));
    }

    public sealed record Panini(PixelExtent Extent, float Field, float Distance, float Compression) : Projection(Extent) {
        public override bool Upright => true;

        private protected override Option<Vector3> Direction((double X, double Y) point) =>
            from centered in Some(Centered(point))
            let distance = (double)Distance
            let factor = 2d * double.Sin(Field / 2d) / (distance + double.Cos(Field / 2d))
            from cos in distance is 0d
                ? Some(1d / factor / double.Hypot(centered.X, 1d / factor))
                : (from u in Some(double.Abs(centered.X * factor))
                   let delta = double.FusedMultiplyAdd(-distance, u, 1d)
                   where delta > 0d
                   let t = distance * u
                   let remainder = delta * (1d + t)
                   select remainder / (double.Sqrt(remainder + (u * u)) + (u * t)))
            let vertical = centered.Y * (cos / (((1d - Compression) * cos) + Compression))
            let depth = distance is 0d ? -1d / factor : -cos / (factor * (distance + cos))
            let scale = double.Max(double.Abs(centered.X), double.Max(double.Abs(vertical), double.Abs(depth)))
            select Vector3.Normalize(new Vector3((float)(centered.X / scale), (float)(vertical / scale), (float)(depth / scale)));

        private protected override Option<(double X, double Y)> FramePoint(Vector3 ray) =>
            double.Hypot(ray.X, ray.Z) switch {
                var radius when -ray.Z > 0f => (double.SinCos(Field / 2d), -ray.Z / radius) switch {
                    var (edge, cos) => ((Distance + edge.Cos) / (Distance + cos) / (2d * edge.Sin)) switch {
                        var gain => Some(Uncentered((ray.X / radius * gain, ray.Y / radius * (1d - Compression + (Compression / cos)) * gain))),
                    },
                },
                _ => None,
            };
    }
}

public sealed record PanoramaView(Angle Heading, Angle Elevation, Angle Roll) {
    public Quaternion Rotation => Quaternion.CreateFromYawPitchRoll(-(float)Heading.Radians, (float)Elevation.Radians, (float)Roll.Radians);

    public static Option<(float Heading, float Elevation, float Roll)> Facing(Vector3 left, Vector3 right) =>
        from normal in Vector3.AllWhereAllBitsSet(Vector3.IsFinite(left) & Vector3.IsFinite(right)) ? Some(Vector3.Cross(left, right)) : None
        where normal != Vector3.Zero
        let forward = Vector3.Normalize(left) + Vector3.Normalize(right)
        let across = Vector3.Cross(normal, forward)
        let heading = double.Atan2(forward.X, -forward.Z)
        let elevation = double.Atan2(forward.Y, double.Hypot(forward.X, forward.Z))
        let yaw = double.SinCos(heading)
        let pitch = double.SinCos(elevation)
        select ((float)heading, (float)elevation, (float)double.Atan2((-across.X * yaw.Sin * pitch.Sin) + (across.Y * pitch.Cos) + (across.Z * yaw.Cos * pitch.Sin), (across.X * yaw.Cos) + (across.Z * yaw.Sin)));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Homography {
    extension(Matrix<double> map) {
        public Option<(double X, double Y)> Apply((double X, double Y) point) =>
            from input in double.IsFinite(point.X) && double.IsFinite(point.Y) ? Some(point) : None
            let denominator = (map[2, 0] * input.X) + (map[2, 1] * input.Y) + map[2, 2]
            from w in denominator > 0d ? Some(denominator) : None
            let x = ((map[0, 0] * input.X) + (map[0, 1] * input.Y) + map[0, 2]) / w
            let y = ((map[1, 0] * input.X) + (map[1, 1] * input.Y) + map[1, 2]) / w
            from result in double.IsFinite(x) && double.IsFinite(y) ? Some((x, y)) : None
            select result;
    }
}
