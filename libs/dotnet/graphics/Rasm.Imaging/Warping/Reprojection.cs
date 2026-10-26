using System.Numerics;
using Rasm.Imaging.Pixels;

namespace Rasm.Imaging.Warping;

// --- [MODELS] --------------------------------------------------------------------------
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
