using System.Numerics;
using Rasm.Imaging.Pixels;

namespace Rasm.Imaging.Filters.Warp;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>]
[ValidationError<InvalidWarp>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PanoramaOutput {
    public static readonly PanoramaOutput Rectilinear = new("rectilinear", static (_, lens, extent) => new Projection.Rectilinear(extent, lens));
    public static readonly PanoramaOutput Equirectangular = new("equirectangular", static (_, _, extent) => new Projection.Equirectangular(extent));
    public static readonly PanoramaOutput Cylindrical = new("cylindrical", static (state, _, extent) => new Projection.Cylindrical(extent, ViewWindow.Centered(state.Sweep, extent).Raised(state.Height)));
    public static readonly PanoramaOutput Fisheye = new("fisheye", static (state, _, extent) => new Projection.Fisheye(extent, state.Sweep));
    public static readonly PanoramaOutput Equisolid = new("equisolid", static (state, _, extent) => new Projection.Equisolid(extent, state.Sweep, state.FullTurn));
    public static readonly PanoramaOutput MirrorBall = new("mirror-ball", static (_, _, extent) => new Projection.MirrorBall(extent));
    public static readonly PanoramaOutput Stereographic = new("stereographic", static (state, _, extent) => new Projection.Stereographic(extent, state.Sweep));
    public static readonly PanoramaOutput Panini = new("panini", static (state, _, extent) => new Projection.Panini(extent, state.Field, state.Distance, state.Compression));

    [UseDelegateFromConstructor]
    public partial Projection Of(Panorama state, Frustum.Perspective lens, PixelExtent extent);
}

public sealed record Panorama(
    PanoramaOutput Output, PanoramaView View, FieldOfView Field, SweepAngle Sweep, FullTurnRadius FullTurn, CylinderHeight Height,
    PaniniDistance Distance, VerticalCompression Compression, Sampling Sampling, WrapMode Wrap)
    : IStateRecord<Panorama, PanoramaParameter, InvalidWarp>, IPixelStage<Panorama> {
    public static Panorama Default { get; } = new(
        PanoramaOutput.Rectilinear, PanoramaView.Default, FieldOfView.SixthTurn, SweepAngle.HalfTurn, FullTurnRadius.Standard, CylinderHeight.Neutral,
        PaniniDistance.Panini, VerticalCompression.Off, Sampling.Linear, WrapMode.Black);

    public Option<CoordinateMap.Analytic> Placed(Camera camera) =>
        from lens in camera.Frustum.Switch(perspective: static perspective => Some(perspective), parallel: static _ => Option<Frustum.Perspective>.None)
        let output = Output.Of(this, lens, camera.Extent)
        let up = Vector3.Transform(Vector3.UnitZ, Quaternion.Conjugate(camera.Orientation))
        let level = Quaternion.CreateFromYawPitchRoll(0f, float.Atan2(-up.Z, float.Hypot(up.X, up.Y)), float.Atan2(up.X, up.Y))
        select Reprojected(new Projection.Rectilinear(camera.Extent, lens), output,
            Quaternion.Conjugate(output.Upright ? Quaternion.Concatenate(level, View.Rotation) : View.Rotation), Sampling, (Wrap, Wrap));

    public static Option<PixelPass> Pass(Panorama state, PassContext context) =>
        from camera in context.Camera
        where state.Output != PanoramaOutput.Rectilinear || state.View != PanoramaView.Default
        from map in state.Placed(camera)
        select (PixelPass)new PixelPass.Frame(map.Apply);

    public static PixelFrame Viewed(PixelFrame frame, Projection source, Projection view, PanoramaView place) =>
        Reprojected(source, view, place.Rotation, Sampling.Linear, source.Wrap).Resample(frame, view.Extent);

    private static CoordinateMap.Analytic Reprojected(Projection input, Projection output, Quaternion turn, Sampling sampling, (WrapMode Across, WrapMode Down) wrap) =>
        new((points, coverage, extent) => {
            for (int i = 0; i < points.Length; i++) {
                (points[i], coverage[i]) = output.Reprojected(input, turn, points[i]) switch { { IsSome: true } point => ((Vector2)point, coverage[i]), _ => (points[i], 0f) };
                (points[i].Y, coverage[i]) = wrap.Down.Fold(points[i].Y, extent.Height) switch { (var row, var kept) => (row, coverage[i] * kept) };
            }
        }, sampling, wrap.Across);
}

[SmartEnum<string>]
[ValidationError<InvalidWarp>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PanoramaParameter : IStateParameter<Panorama> {
    public static readonly PanoramaParameter Output = new("output", new StateParameter<Panorama>.Choice<PanoramaOutput, InvalidWarp>(
        Lens<Panorama, PanoramaOutput>.New(static state => state.Output, static output => state => state with { Output = output })));
    public static readonly PanoramaParameter View = new("view", new StateParameter<Panorama>.Record<PanoramaView>(
        Lens<Panorama, PanoramaView>.New(static state => state.View, static view => state => state with { View = view })));
    public static readonly PanoramaParameter Field = new("field", new StateParameter<Panorama>.Bounded<FieldOfView, float, InvalidPixelValue>(
        Lens<Panorama, FieldOfView>.New(static state => state.Field, static field => state => state with { Field = field }), FieldOfView.Presentation));
    public static readonly PanoramaParameter Sweep = new("sweep", new StateParameter<Panorama>.Bounded<SweepAngle, float, InvalidPixelValue>(
        Lens<Panorama, SweepAngle>.New(static state => state.Sweep, static sweep => state => state with { Sweep = sweep }), SweepAngle.Presentation));
    public static readonly PanoramaParameter FullTurn = new("full-turn", new StateParameter<Panorama>.Bounded<FullTurnRadius, float, InvalidPixelValue>(
        Lens<Panorama, FullTurnRadius>.New(static state => state.FullTurn, static turn => state => state with { FullTurn = turn }),
        new() { Soft = (2f * 0.01f / 12f, 2f * 15f / 12f), Scale = TrackScale.Log }));
    public static readonly PanoramaParameter Height = new("height", new StateParameter<Panorama>.Bounded<CylinderHeight, float, InvalidPixelValue>(
        Lens<Panorama, CylinderHeight>.New(static state => state.Height, static height => state => state with { Height = height }), new()));
    public static readonly PanoramaParameter Distance = new("distance", new StateParameter<Panorama>.Bounded<PaniniDistance, float, InvalidPixelValue>(
        Lens<Panorama, PaniniDistance>.New(static state => state.Distance, static distance => state => state with { Distance = distance }), new() { Soft = (0f, 1f) }));
    public static readonly PanoramaParameter Compression = new("compression", new StateParameter<Panorama>.Bounded<VerticalCompression, float, InvalidPixelValue>(
        Lens<Panorama, VerticalCompression>.New(static state => state.Compression, static compression => state => state with { Compression = compression }), new()));
    public static readonly PanoramaParameter Sampling = new("sampling", new StateParameter<Panorama>.Choice<Sampling, InvalidWarp>(
        Lens<Panorama, Sampling>.New(static state => state.Sampling, static sampling => state => state with { Sampling = sampling })));
    public static readonly PanoramaParameter Wrap = new("wrap", new StateParameter<Panorama>.Choice<WrapMode, InvalidPixelValue>(
        Lens<Panorama, WrapMode>.New(static state => state.Wrap, static wrap => state => state with { Wrap = wrap })));

    public StateParameter<Panorama> Kind { get; }
}

[SmartEnum<string>]
[ValidationError<InvalidWarp>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PolarConversion {
    public static readonly PolarConversion RectangularToPolar = new("rectangular-to-polar", static (point, extent) =>
        ((point - FramePosition.Center.Point(extent)) / extent.ShortSide) switch {
            var q => new Vector2(extent.Width * (float.Pi + float.Atan2(-q.X, q.Y)) / float.Tau, 2f * extent.Height * q.Length()),
        });
    public static readonly PolarConversion PolarToRectangular = new("polar-to-rectangular", static (point, extent) =>
        float.SinCos(float.Tau * point.X / extent.Width) switch {
            var (sin, cos) => FramePosition.Center.Point(extent) + (extent.ShortSide * point.Y / (2f * extent.Height) * new Vector2(sin, -cos)),
        });

    [UseDelegateFromConstructor]
    public partial Vector2 Source(Vector2 point, PixelExtent extent);
}

public sealed record PolarCoordinates(PolarConversion Conversion, Sampling Sampling, WrapMode Wrap)
    : IStateRecord<PolarCoordinates, PolarCoordinatesParameter, InvalidWarp>, IPixelStage<PolarCoordinates> {
    public static PolarCoordinates Default { get; } = new(PolarConversion.RectangularToPolar, Sampling.Linear, WrapMode.Black);

    public static Option<PixelPass> Pass(PolarCoordinates state, PassContext context) =>
        Some<PixelPass>(new PixelPass.Frame(new CoordinateMap.Analytic((points, _, extent) => {
            foreach (ref Vector2 point in points) point = state.Conversion.Source(point, extent);
        }, state.Sampling, state.Wrap).Apply));
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
    public static readonly PolarCoordinatesParameter Wrap = new("wrap", new StateParameter<PolarCoordinates>.Choice<WrapMode, InvalidPixelValue>(
        Lens<PolarCoordinates, WrapMode>.New(static state => state.Wrap, static wrap => state => state with { Wrap = wrap })));

    public StateParameter<PolarCoordinates> Kind { get; }
}
