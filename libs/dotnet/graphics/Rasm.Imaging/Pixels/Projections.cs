using System.Drawing;
using System.Numerics;
using Emgu.CV;
using MathNet.Numerics.LinearAlgebra;
using UnitsNet;

namespace Rasm.Imaging.Pixels;

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject(SkipFactoryMethods = true)]
public sealed partial class FrameQuad {
    public Vector2 TopLeft { get; }
    public Vector2 TopRight { get; }
    public Vector2 BottomRight { get; }
    public Vector2 BottomLeft { get; }

    public bool Mirrored => Turn(BottomLeft, TopLeft, TopRight) < 0f;

    public FrameQuad FlippedHorizontal => new(TopRight, TopLeft, BottomLeft, BottomRight);
    public FrameQuad FlippedVertical => new(BottomLeft, BottomRight, TopRight, TopLeft);
    public FrameQuad Turned => new(BottomLeft, TopLeft, TopRight, BottomRight);

    public Matrix<double> Homography {
        get {
            using Mat map = CvInvoke.GetPerspectiveTransform([new(0f, 0f), new(1f, 0f), new(1f, 1f), new(0f, 1f)], [Corner(TopLeft), Corner(TopRight), Corner(BottomRight), Corner(BottomLeft)]);
            return Matrix<double>.Build.DenseOfArray((double[,])map.GetData());
        }
    }

    public Func<Vector2, Option<Vector2>> Mapping() {
        Matrix<double> map = Homography;
        return point =>
            from input in Vector2.AllWhereAllBitsSet(Vector2.IsFinite(point)) ? Some(point) : None
            from w in ((map[2, 0] * input.X) + (map[2, 1] * input.Y) + map[2, 2]) is > 0d and var denominator ? Some(denominator) : None
            let mapped = new Vector2((float)(((map[0, 0] * input.X) + (map[0, 1] * input.Y) + map[0, 2]) / w), (float)(((map[1, 0] * input.X) + (map[1, 1] * input.Y) + map[1, 2]) / w))
            where Vector2.AllWhereAllBitsSet(Vector2.IsFinite(mapped))
            select mapped;
    }

    public static Fin<FrameQuad> Validated(Vector2 topLeft, Vector2 topRight, Vector2 bottomRight, Vector2 bottomLeft) =>
        ImagingFault.Unless((Turn(bottomLeft, topLeft, topRight), Turn(topLeft, topRight, bottomRight), Turn(topRight, bottomRight, bottomLeft), Turn(bottomRight, bottomLeft, topLeft))
            is ( > 0f, > 0f, > 0f, > 0f) or ( < 0f, < 0f, < 0f, < 0f), ImagingReason.Degenerate, (topLeft, topRight, bottomRight, bottomLeft)).Map(_ => new FrameQuad(topLeft, topRight, bottomRight, bottomLeft));

    private static PointF Corner(Vector2 corner) => new(corner.X, corner.Y);

    private static float Turn(Vector2 previous, Vector2 corner, Vector2 next) => ((corner.X - previous.X) * (next.Y - corner.Y)) - ((corner.Y - previous.Y) * (next.X - corner.X));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Projection {
    private static readonly Vector2 Flip = new(1f, -1f);

    private Projection(PixelExtent extent, bool upright, (WrapMode Across, WrapMode Down) wrap) => (Extent, Upright, Wrap) = (extent, upright, wrap);

    private Projection(PixelExtent extent, bool upright = false) : this(extent, upright, (WrapMode.Black, WrapMode.Black)) { }

    public PixelExtent Extent { get; }
    public bool Upright { get; }
    public (WrapMode Across, WrapMode Down) Wrap { get; }

    public Option<(Vector3 Origin, Vector3 Direction)> Ray(Vector2 pixel) =>
        Vector2.AllWhereAllBitsSet(Vector2.IsFinite(pixel)) && Direction(pixel) is { IsSome: true } direction ? (Origin(pixel), (Vector3)direction) : None;

    public Option<(Vector2 Point, float Depth)> Pixel(Vector3 view) =>
        Vector3.AllWhereAllBitsSet(Vector3.IsFinite(view)) && view != Vector3.Zero && Located(view) is { IsSome: true } point ? ((Vector2)point, Depth(view)) : None;

    public Option<Vector3> Point(Vector2 pixel, float depth) => Ray(pixel).Map(ray => ray.Origin + (ray.Direction * (depth / Depth(ray.Direction))));

    public Option<Vector2> Reprojected(Projection target, Quaternion turn, Vector2 pixel) =>
        Vector2.AllWhereAllBitsSet(Vector2.IsFinite(pixel)) && Direction(pixel) is { IsSome: true } direction ? target.Located(Vector3.Transform((Vector3)direction, turn)) : None;

    private protected abstract Option<Vector3> Direction(Vector2 pixel);
    private protected abstract Option<Vector2> Located(Vector3 view);
    private protected virtual Vector3 Origin(Vector2 pixel) => Vector3.Zero;
    private protected virtual float Depth(Vector3 view) => view.Length();

    private Vector2 Centered(Vector2 pixel) => (pixel - Extent.Point(new Vector2(0.5f))) * Flip / Extent.Width;
    private Vector2 Uncentered(Vector2 centered) => (centered * Flip * Extent.Width) + Extent.Point(new Vector2(0.5f));
    private float Circle => (float)Extent.ShortSide / Extent.Width;

    private Option<Vector3> Radial(Vector2 pixel, double cut, double scale, Func<double, double, double> angle) =>
        Centered(pixel) switch {
            var centered => angle(centered.Length(), scale) switch {
                var theta when theta <= cut => (double.SinCos(double.Atan2(centered.Y, centered.X)), double.SinCos(theta)) switch {
                    var (azimuth, inclination) => Some(new Vector3((float)(azimuth.Cos * inclination.Sin), (float)(azimuth.Sin * inclination.Sin), (float)-inclination.Cos)),
                },
                _ => None,
            },
        };

    private Option<Vector2> RadialPoint(Vector3 view, double cut, double scale, Func<double, double, double> radius) =>
        double.Atan2(double.Hypot(view.X, view.Y), -view.Z) switch {
            var theta when theta <= cut => (double.SinCos(double.Atan2(view.Y, view.X)), radius(theta, scale)) switch {
                var (azimuth, reach) => Some(Uncentered(new Vector2((float)(azimuth.Cos * reach), (float)(azimuth.Sin * reach)))),
            },
            _ => None,
        };

    public sealed record Rectilinear(PixelExtent Extent, ViewWindow Window, Option<LensDistortion> Distortion = default) : Projection(Extent) {
        public Option<float> Illumination(Vector2 pixel) => Ray(pixel).Map(static ray => float.Pow(ray.Direction.Z, 4f));

        private protected override Option<Vector3> Direction(Vector2 pixel) =>
            Vector2.Transform(pixel, Window.ToWindow(Extent)) switch {
                var plane => (Distortion.Case is LensDistortion lens ? lens.Ideal(plane * Flip).Map(static ideal => ideal * Flip) : plane).Map(static ideal => Vector3.Normalize(new Vector3(ideal, -1f))),
            };

        private protected override Option<Vector2> Located(Vector3 view) =>
            -view.Z > 0f && new Vector2(view.X, view.Y) / -view.Z is var ideal
                && (Distortion.Case is LensDistortion lens ? lens.Distorted(ideal * Flip).Map(static distorted => distorted * Flip) : ideal) is { IsSome: true } plane
                ? Vector2.Transform((Vector2)plane, Window.FromWindow(Extent))
                : None;

        private protected override float Depth(Vector3 view) => -view.Z;
    }

    public sealed record Orthographic(PixelExtent Extent, ViewWindow Window) : Projection(Extent) {
        private protected override Option<Vector3> Direction(Vector2 pixel) => -Vector3.UnitZ;
        private protected override Option<Vector2> Located(Vector3 view) => -view.Z > 0f ? Some(Vector2.Transform(new Vector2(view.X, view.Y), Window.FromWindow(Extent))) : None;
        private protected override Vector3 Origin(Vector2 pixel) => new(Vector2.Transform(pixel, Window.ToWindow(Extent)), 0f);
        private protected override float Depth(Vector3 view) => -view.Z;
    }

    public sealed record Equirectangular(PixelExtent Extent) : Projection(Extent, upright: true, (WrapMode.Periodic, WrapMode.Clamp)) {
        private protected override Option<Vector3> Direction(Vector2 pixel) =>
            (double.SinCos(((pixel.X % Extent.Width / Extent.Width) - 0.5d) * double.Tau), double.SinCos((0.5d - (pixel.Y % (2d * Extent.Height) / Extent.Height)) * double.Pi)) switch {
                var (longitude, latitude) => Some(new Vector3((float)(longitude.Sin * latitude.Cos), (float)latitude.Sin, (float)(-longitude.Cos * latitude.Cos))),
            };

        private protected override Option<Vector2> Located(Vector3 view) =>
            Some(new Vector2((float)(((double.Atan2(view.X, -view.Z) / double.Tau) + 0.5d) * Extent.Width), (float)((0.5d - double.Atan2Pi(view.Y, double.Hypot(view.X, view.Z))) * Extent.Height)));
    }

    public sealed record Cylindrical(PixelExtent Extent, ViewWindow Window) : Projection(Extent, upright: true) {
        private protected override Option<Vector3> Direction(Vector2 pixel) =>
            Vector2.Transform(pixel, Window.ToWindow(Extent)) switch {
                var plane => (double.SinCos(plane.X), double.SinCos(double.Atan(plane.Y))) switch {
                    var (across, latitude) => Some(new Vector3((float)(across.Sin * latitude.Cos), (float)latitude.Sin, (float)(-across.Cos * latitude.Cos))),
                },
            };

        private protected override Option<Vector2> Located(Vector3 view) =>
            double.Hypot(view.X, view.Z) switch {
                > 0d and var radius => Some(Vector2.Transform(new Vector2((float)double.Atan2(view.X, -view.Z), (float)(view.Y / radius)), Window.FromWindow(Extent))),
                _ => None,
            };
    }

    public sealed record Equidistant(PixelExtent Extent, Angle Sweep) : Projection(Extent) {
        private protected override Option<Vector3> Direction(Vector2 pixel) => Radial(pixel, Sweep.Radians.ToDouble() / 2d, Sweep.Radians.ToDouble() / Circle, static (radius, scale) => radius * scale);
        private protected override Option<Vector2> Located(Vector3 view) => RadialPoint(view, Sweep.Radians.ToDouble() / 2d, Sweep.Radians.ToDouble() / Circle, static (angle, scale) => angle / scale);
    }

    public sealed record Equisolid(PixelExtent Extent, Angle Sweep, double FocalRatio) : Projection(Extent) {
        public static Equisolid MirrorBall(PixelExtent extent) => new(extent, Angle.FromRadians(double.Tau), (double)extent.ShortSide / extent.Width / 4d);

        private protected override Option<Vector3> Direction(Vector2 pixel) => Radial(pixel, Sweep.Radians.ToDouble() / 2d, 2d * FocalRatio, static (radius, scale) => 2d * double.Asin(radius / scale));
        private protected override Option<Vector2> Located(Vector3 view) => RadialPoint(view, Sweep.Radians.ToDouble() / 2d, 2d * FocalRatio, static (angle, scale) => scale * double.Sin(angle / 2d));
    }

    public sealed record Stereographic(PixelExtent Extent, Angle Sweep) : Projection(Extent) {
        private protected override Option<Vector3> Direction(Vector2 pixel) => Radial(pixel, double.Pi, 2d * double.Tan(Sweep.Radians.ToDouble() / 4d), static (radius, scale) => 2d * double.Atan(radius * scale));
        private protected override Option<Vector2> Located(Vector3 view) => RadialPoint(view, double.BitDecrement(double.Pi), 2d * double.Tan(Sweep.Radians.ToDouble() / 4d), static (angle, scale) => double.Tan(angle / 2d) / scale);
    }

    public sealed record Panini(PixelExtent Extent, Angle Field, float Distance, float Compression) : Projection(Extent, upright: true) {
        private double Gain => double.SinCos(Field.Radians.ToDouble() / 2d) switch { var (sin, cos) => (Distance + cos) / (2d * sin) };

        private protected override Option<Vector3> Direction(Vector2 pixel) =>
            Centered(pixel) switch {
                var centered => (centered.X * Distance / double.Hypot(Gain, centered.X)) switch {
                    var reach when double.Abs(reach) <= 1d => double.SinCos(double.Atan2(centered.X, Gain) + double.Asin(reach)) switch {
                        var (sin, cos) when Covers(cos) => Some(Vector3.Normalize(new Vector3((float)sin, (float)(centered.Y * (Distance + cos) / (Gain * Vertical(cos))), (float)-cos))),
                        _ => None,
                    },
                    _ => None,
                },
            };

        private protected override Option<Vector2> Located(Vector3 view) =>
            double.Hypot(view.X, view.Z) switch {
                > 0d and var radius when -view.Z / radius is var cos && Covers(cos) => (Gain / (Distance + cos)) switch {
                    var gain => Some(Uncentered(new Vector2((float)(view.X / radius * gain), (float)(view.Y / radius * Vertical(cos) * gain)))),
                },
                _ => None,
            };

        private bool Covers(double cos) => Distance + cos > 0d && (Compression is 0f || cos > 0d);

        private double Vertical(double cos) => 1d - Compression + (Compression / cos);
    }
}

public sealed record PanoramaView(Angle Heading, Angle Elevation, Angle Roll) {
    public Quaternion Rotation => Quaternion.CreateFromYawPitchRoll(-(float)Heading.Radians.ToDouble(), (float)Elevation.Radians.ToDouble(), (float)Roll.Radians.ToDouble());

    public static Option<PanoramaView> Facing(Vector3 left, Vector3 right) =>
        from normal in Vector3.AllWhereAllBitsSet(Vector3.IsFinite(left) & Vector3.IsFinite(right)) && Vector3.Cross(left, right) is var cross && cross != Vector3.Zero ? Some(cross) : None
        let forward = Vector3.Normalize(left) + Vector3.Normalize(right)
        let across = Vector3.Cross(normal, forward)
        let heading = double.Atan2(forward.X, -forward.Z)
        let elevation = double.Atan2(forward.Y, double.Hypot(forward.X, forward.Z))
        let yaw = double.SinCos(heading)
        let pitch = double.SinCos(elevation)
        select new PanoramaView(Angle.FromRadians(heading), Angle.FromRadians(elevation),
            Angle.FromRadians(double.Atan2((-across.X * yaw.Sin * pitch.Sin) + (across.Y * pitch.Cos) + (across.Z * yaw.Cos * pitch.Sin), (across.X * yaw.Cos) + (across.Z * yaw.Sin))));
}
