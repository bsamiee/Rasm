using System.Numerics;
using Rasm.Imaging.Pixels;
using UnitsNet.Units;

namespace Rasm.Imaging.Filters.Warp;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>]
[ValidationError<InvalidWarp>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PanoramaOutput {
    public static readonly PanoramaOutput Rectilinear = new("rectilinear", static (_, lens, extent) => new Projection.Rectilinear(extent, lens));
    public static readonly PanoramaOutput Equirectangular = new("equirectangular", static (_, _, extent) => new Projection.Equirectangular(extent));
    public static readonly PanoramaOutput Cylindrical = new("cylindrical", static (state, _, extent) => new Projection.Cylindrical(extent, ViewWindow.Centered(state.Sweep, extent)));
    public static readonly PanoramaOutput Fisheye = new("fisheye", static (state, _, extent) => new Projection.Fisheye(extent, state.Sweep));
    public static readonly PanoramaOutput Equisolid = new("equisolid", static (state, _, extent) => new Projection.Equisolid(extent, state.Sweep));
    public static readonly PanoramaOutput MirrorBall = new("mirror-ball", static (_, _, extent) => new Projection.MirrorBall(extent));
    public static readonly PanoramaOutput Stereographic = new("stereographic", static (state, _, extent) => new Projection.Stereographic(extent, state.Sweep));
    public static readonly PanoramaOutput Panini = new("panini", static (state, _, extent) => new Projection.Panini(extent, state.Field, state.Distance, state.Compression));

    [UseDelegateFromConstructor]
    public partial Projection Of(Panorama state, Frustum.Perspective lens, PixelExtent extent);
}

public sealed record Panorama(
    PanoramaOutput Output, PanoramaView View, FieldOfView Field, SweepAngle Sweep, PaniniDistance Distance, VerticalCompression Compression, Sampling Sampling, EdgeMode Wrap)
    : IStateRecord<Panorama, PanoramaParameter, InvalidWarp>, IPixelStage<Panorama> {
    public static Panorama Default { get; } = new(
        PanoramaOutput.Rectilinear, default, FieldOfView.SixthTurn, SweepAngle.HalfTurn, PaniniDistance.Panini, VerticalCompression.Off, Sampling.Linear, EdgeMode.Black);

    public CoordinateMap Placed(Camera camera, Frustum.Perspective lens) =>
        Output.Of(this, lens, camera.Extent) switch {
            var output => new CoordinateMap.Analytic(
                Reprojected(new Projection.Rectilinear(camera.Extent, lens), output,
                    Quaternion.Conjugate(output.Upright ? Quaternion.Concatenate(Level(camera), View.Rotation) : View.Rotation)),
                Sampling, Wrap),
        };

    public static Option<PixelPass> Pass(Panorama state, PassContext context) =>
        from camera in context.Camera
        from lens in camera.Frustum.Switch(perspective: static perspective => Some(perspective), parallel: static _ => Option<Frustum.Perspective>.None)
        where state.Output != PanoramaOutput.Rectilinear || state.View != default
        select (PixelPass)new PixelPass.Frame(state.Placed(camera, lens).Apply);

    public static PixelFrame Viewed(PixelFrame source, Projection view, PanoramaView place) =>
        new CoordinateMap.Analytic(
            (points, coverage, extent) => {
                Reprojected(new Projection.Equirectangular(source.Size), view, place.Rotation)(points, coverage, extent);
                foreach (ref Vector2 point in points) point.Y = float.Clamp(point.Y, 0.5f, extent.Height - 0.5f);
            },
            Sampling.Linear, EdgeMode.Periodic).Resample(source, view.Extent);

    private static Action<Span<Vector2>, Span<float>, PixelExtent> Reprojected(Projection input, Projection output, Quaternion turn) =>
        (points, coverage, _) => {
            for (int i = 0; i < points.Length; i++)
                (points[i], coverage[i]) = output.Reprojected(input, turn, points[i]) switch {
                    { IsSome: true } point => ((Vector2)point, coverage[i]),
                    _ => (points[i], 0f),
                };
        };

    private static Quaternion Level(Camera camera) =>
        Vector3.Transform(Vector3.UnitZ, Quaternion.Conjugate(camera.Orientation)) switch {
            var up => Quaternion.CreateFromYawPitchRoll(0f, float.Atan2(-up.Z, float.Hypot(up.X, up.Y)), float.Atan2(up.X, up.Y)),
        };
}

[SmartEnum<string>]
[ValidationError<InvalidWarp>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PanoramaParameter : IStateParameter<Panorama> {
    private static readonly Lens<Panorama, PanoramaView> ViewOf =
        Lens<Panorama, PanoramaView>.New(static state => state.View, static view => state => state with { View = view });
    private static readonly Presentation<FieldOfView, float> Span = new() { Unit = UnitsNet.Quantity.GetUnitInfo(AngleUnit.Radian) };

    public static readonly PanoramaParameter Output = new("output", new StateParameter<Panorama>.Choice<PanoramaOutput, InvalidWarp>(
        Lens<Panorama, PanoramaOutput>.New(static state => state.Output, static output => state => state with { Output = output })));
    public static readonly PanoramaParameter Heading = new("heading", new StateParameter<Panorama>.Bounded<SignedAngle, float, InvalidPixelValue>(
        lens(ViewOf, Lens<PanoramaView, SignedAngle>.New(static view => view.Heading, static heading => view => view with { Heading = heading })), Presentations.Angle));
    public static readonly PanoramaParameter Elevation = new("elevation", new StateParameter<Panorama>.Bounded<Elevation, float, InvalidPixelValue>(
        lens(ViewOf, Lens<PanoramaView, Elevation>.New(static view => view.Elevation, static elevation => view => view with { Elevation = elevation })),
        new() { Unit = UnitsNet.Quantity.GetUnitInfo(AngleUnit.Radian), Origin = (float)Pixels.Elevation.Neutral }));
    public static readonly PanoramaParameter Roll = new("roll", new StateParameter<Panorama>.Bounded<SignedAngle, float, InvalidPixelValue>(
        lens(ViewOf, Lens<PanoramaView, SignedAngle>.New(static view => view.Roll, static roll => view => view with { Roll = roll })), Presentations.Angle));
    public static readonly PanoramaParameter Field = new("field", new StateParameter<Panorama>.Bounded<FieldOfView, float, InvalidPixelValue>(
        Lens<Panorama, FieldOfView>.New(static state => state.Field, static field => state => state with { Field = field }), Span));
    public static readonly PanoramaParameter Sweep = new("sweep", new StateParameter<Panorama>.Bounded<SweepAngle, float, InvalidPixelValue>(
        Lens<Panorama, SweepAngle>.New(static state => state.Sweep, static sweep => state => state with { Sweep = sweep }), new() { Unit = UnitsNet.Quantity.GetUnitInfo(AngleUnit.Radian) }));
    public static readonly PanoramaParameter Distance = new("distance", new StateParameter<Panorama>.Bounded<PaniniDistance, float, InvalidPixelValue>(
        Lens<Panorama, PaniniDistance>.New(static state => state.Distance, static distance => state => state with { Distance = distance }), new() { Soft = (0f, 1f) }));
    public static readonly PanoramaParameter Compression = new("compression", new StateParameter<Panorama>.Bounded<VerticalCompression, float, InvalidPixelValue>(
        Lens<Panorama, VerticalCompression>.New(static state => state.Compression, static compression => state => state with { Compression = compression }), new()));
    public static readonly PanoramaParameter Sampling = new("sampling", new StateParameter<Panorama>.Choice<Sampling, InvalidWarp>(
        Lens<Panorama, Sampling>.New(static state => state.Sampling, static sampling => state => state with { Sampling = sampling })));
    public static readonly PanoramaParameter Wrap = new("wrap", new StateParameter<Panorama>.Choice<EdgeMode, InvalidWarp>(
        Lens<Panorama, EdgeMode>.New(static state => state.Wrap, static wrap => state => state with { Wrap = wrap })));

    public StateParameter<Panorama> Kind { get; }
}

[SmartEnum<string>]
[ValidationError<InvalidWarp>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PolarConversion {
    public static readonly PolarConversion RectangularToPolar = new("rectangular-to-polar", static (point, extent) =>
        ((point - (new Vector2(extent.Width, extent.Height) / 2f)) / int.Min(extent.Width, extent.Height)) switch {
            var q => new Vector2(extent.Width * (float.Pi + float.Atan2(-q.X, q.Y)) / float.Tau, 2f * extent.Height * q.Length()),
        });
    public static readonly PolarConversion PolarToRectangular = new("polar-to-rectangular", static (point, extent) =>
        float.SinCos(float.Tau * point.X / extent.Width) switch {
            var (sin, cos) => (new Vector2(extent.Width, extent.Height) / 2f) + (int.Min(extent.Width, extent.Height) * point.Y / (2f * extent.Height) * new Vector2(sin, -cos)),
        });

    [UseDelegateFromConstructor]
    public partial Vector2 Source(Vector2 point, PixelExtent extent);
}

public sealed record PolarCoordinates(PolarConversion Conversion, Sampling Sampling, EdgeMode Wrap)
    : IStateRecord<PolarCoordinates, PolarCoordinatesParameter, InvalidWarp>, IPixelStage<PolarCoordinates> {
    public static PolarCoordinates Default { get; } = new(PolarConversion.RectangularToPolar, Sampling.Linear, EdgeMode.Black);

    public static Option<PixelPass> Pass(PolarCoordinates state, PassContext context) =>
        Some<PixelPass>(new PixelPass.Frame(new CoordinateMap.Analytic(
            (points, _, _) => {
                foreach (ref Vector2 point in points) point = state.Conversion.Source(point, context.Extent);
            },
            state.Sampling, state.Wrap).Apply));
}

[SmartEnum<string>]
[ValidationError<InvalidWarp>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PolarCoordinatesParameter : IStateParameter<PolarCoordinates> {
    public static readonly PolarCoordinatesParameter Conversion = new("conversion", new StateParameter<PolarCoordinates>.Choice<PolarConversion, InvalidWarp>(
        Lens<PolarCoordinates, PolarConversion>.New(static state => state.Conversion, static conversion => state => state with { Conversion = conversion })));
    public static readonly PolarCoordinatesParameter Sampling = new("sampling", new StateParameter<PolarCoordinates>.Choice<Sampling, InvalidWarp>(
        Lens<PolarCoordinates, Sampling>.New(static state => state.Sampling, static sampling => state => state with { Sampling = sampling })));
    public static readonly PolarCoordinatesParameter Wrap = new("wrap", new StateParameter<PolarCoordinates>.Choice<EdgeMode, InvalidWarp>(
        Lens<PolarCoordinates, EdgeMode>.New(static state => state.Wrap, static wrap => state => state with { Wrap = wrap })));

    public StateParameter<PolarCoordinates> Kind { get; }
}
