using System.Numerics;
using Rasm.Imaging.Pixels;

namespace Rasm.Imaging.Filters.Warp;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PanoramaOutput : ICaptioned {
    public static readonly PanoramaOutput Rectilinear = new("rectilinear", "Rectilinear", static (_, rendering) => rendering);
    public static readonly PanoramaOutput Equirectangular = new("equirectangular", "Equirectangular", static (_, rendering) => new Projection.Equirectangular(rendering.Extent));
    public static readonly PanoramaOutput Cylindrical = new("cylindrical", "Cylindrical", static (state, rendering) => new Projection.Cylindrical(rendering.Extent, ViewWindow.Centered(state.Sweep, rendering.Extent).Raised(state.Height)));
    public static readonly PanoramaOutput Fisheye = new("fisheye", "Fisheye equidistant", static (state, rendering) => new Projection.Fisheye(rendering.Extent, state.Sweep));
    public static readonly PanoramaOutput Equisolid = new("equisolid", "Fisheye equisolid", static (state, rendering) => new Projection.Equisolid(rendering.Extent, state.Sweep, state.FullTurn));
    public static readonly PanoramaOutput MirrorBall = new("mirror-ball", "Mirror ball", static (_, rendering) => new Projection.MirrorBall(rendering.Extent));
    public static readonly PanoramaOutput Stereographic = new("stereographic", "Stereographic", static (state, rendering) => new Projection.Stereographic(rendering.Extent, state.Sweep));
    public static readonly PanoramaOutput Panini = new("panini", "Panini", static (state, rendering) => new Projection.Panini(rendering.Extent, state.Field, state.Distance, state.Compression));

    public string Caption { get; }

    [UseDelegateFromConstructor]
    public partial Projection Of(Panorama state, Projection.Rectilinear rendering);
}

public sealed record Panorama(PanoramaOutput Output, PanoramaView View, FieldOfView Field, SweepAngle Sweep, FullTurnRadius FullTurn, CylinderHeight Height, Magnitude Distance, AxisFraction Compression, Sampling Sampling, WrapMode Wrap)
    : IStateRecord<Panorama, PanoramaParameter>, IPixelStage<Panorama> {
    public static Panorama Default { get; } = new(PanoramaOutput.Rectilinear, PanoramaView.Default, FieldOfView.SixthTurn, SweepAngle.HalfTurn, FullTurnRadius.Standard, CylinderHeight.Neutral, Magnitude.Unit, AxisFraction.MinValue, Sampling.Linear, WrapMode.Black);

    public Option<CoordinateMap.Analytic> Placed(Camera camera) =>
        from rendering in camera.Frustum.Switch(camera.Extent,
            perspective: static (extent, lens) => Some(new Projection.Rectilinear(extent, lens)), parallel: static (_, _) => Option<Projection.Rectilinear>.None)
        let output = Output.Of(this, rendering)
        let up = Vector3.Transform(Vector3.UnitZ, Quaternion.Conjugate(camera.Orientation))
        let level = Quaternion.CreateFromYawPitchRoll(0f, float.Atan2(-up.Z, float.Hypot(up.X, up.Y)), float.Atan2(up.X, up.Y))
        select Reprojected(rendering, output, Quaternion.Conjugate(output.Upright ? View.Rotation * level : View.Rotation), Sampling, (Wrap, Wrap));

    public static Option<PixelPass> Pass(Panorama state, PassContext context) =>
        from map in context.Camera.Bind(state.Placed)
        where state.Output != PanoramaOutput.Rectilinear || state.View != PanoramaView.Default
        select (PixelPass)new PixelPass.Frame(map.Apply);

    public static PixelFrame Viewed(PixelFrame frame, Projection source, Projection view, PanoramaView place) =>
        Reprojected(source, view, place.Rotation, Sampling.Linear, source.Wrap).Resample(frame, view.Extent);

    private static CoordinateMap.Analytic Reprojected(Projection input, Projection output, Quaternion turn, Sampling sampling, (WrapMode Across, WrapMode Down) wrap) =>
        new((points, coverage, _) => {
            for (int i = 0; i < points.Length; i++) {
                float weight = coverage[i];
                (points[i], coverage[i]) = output.Reprojected(input, turn, points[i]).Map(point => (point, weight)).IfNone((points[i], 0f));
            }
        }, sampling, wrap);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PanoramaParameter : IStateParameter<Panorama> {
    public static readonly PanoramaParameter Output = new("output", new StateParameter<Panorama>.Choice<PanoramaOutput>(Lens<Panorama, PanoramaOutput>.New(static state => state.Output, static output => state => state with { Output = output })),
        new("Projection", "Projection the rendered view is placed into"));
    public static readonly PanoramaParameter View = new("view", new StateParameter<Panorama>.Record<PanoramaView>(Lens<Panorama, PanoramaView>.New(static state => state.View, static view => state => state with { View = view })),
        new("View", "Rendered view orientation"));
    public static readonly PanoramaParameter Field = new("field", StateParameter.Bounded(Lens<Panorama, FieldOfView>.New(static state => state.Field, static field => state => state with { Field = field })),
        new("Field of view", "Horizontal angle Panini output spans across frame"));
    public static readonly PanoramaParameter Sweep = new("sweep", StateParameter.Bounded(Lens<Panorama, SweepAngle>.New(static state => state.Sweep, static sweep => state => state with { Sweep = sweep })),
        new("Sweep", "Angle cylindrical, fisheye, or stereographic output spans across frame"));
    public static readonly PanoramaParameter FullTurn = new("full-turn", StateParameter.Bounded(Lens<Panorama, FullTurnRadius>.New(static state => state.FullTurn, static fullTurn => state => state with { FullTurn = fullTurn }), new Presentation<float> { Soft = (2f * 0.01f / 12f, 2f * 15f / 12f), Scale = TrackScale.Log }),
        new("Full turn radius", "Equisolid full-turn radius as a share of the inscribed circle"));
    public static readonly PanoramaParameter Height = new("height", StateParameter.Bounded(Lens<Panorama, CylinderHeight>.New(static state => state.Height, static height => state => state with { Height = height }), new Presentation<float> { Soft = (-10f, 10f) }),
        new("Height", "Cylindrical viewing window vertical offset"));
    public static readonly PanoramaParameter Distance = new("distance", StateParameter.Bounded(Lens<Panorama, Magnitude>.New(static state => state.Distance, static distance => state => state with { Distance = distance }), new Presentation<float> { Soft = (0f, 1f) }),
        new("Distance", "Panini distance, 0 rectilinear and 1 Panini"));
    public static readonly PanoramaParameter Compression = new("compression", StateParameter.Bounded(Lens<Panorama, AxisFraction>.New(static state => state.Compression, static compression => state => state with { Compression = compression })),
        new("Compression", "Panini compression straightening verticals"));
    public static readonly PanoramaParameter Sampling = new("sampling", new StateParameter<Panorama>.Choice<Sampling>(Lens<Panorama, Sampling>.New(static state => state.Sampling, static sampling => state => state with { Sampling = sampling })),
        new("Interpolation", "Filter between source pixels"));
    public static readonly PanoramaParameter Wrap = new("wrap", new StateParameter<Panorama>.Choice<WrapMode>(Lens<Panorama, WrapMode>.New(static state => state.Wrap, static wrap => state => state with { Wrap = wrap })),
        new("Extension", "Sampling past frame edges"));

    public StateParameter<Panorama> Kind { get; }
    public ParameterText Text { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PolarConversion : ICaptioned {
    public static readonly PolarConversion RectangularToPolar = new("rectangular-to-polar", static (point, extent) =>
        FramePosition.Center.Point(extent) switch { var center => ((point.X - center.X) / extent.ShortSide, (point.Y - center.Y) / extent.ShortSide) switch { var (x, y) => (extent.Width * (double.Pi + double.Atan2(-x, y)) / double.Tau, 2d * extent.Height * double.Hypot(x, y)) } });
    public static readonly PolarConversion PolarToRectangular = new("polar-to-rectangular", static (point, extent) =>
        (double.SinCos(double.Tau * point.X / extent.Width), FramePosition.Center.Point(extent), extent.ShortSide * point.Y / (2d * extent.Height)) switch { var ((sin, cos), center, radius) => (center.X + (radius * sin), center.Y - (radius * cos)) });

    [UseDelegateFromConstructor]
    public partial (double X, double Y) Source((double X, double Y) point, PixelExtent extent);
}

public sealed record PolarCoordinates(PolarConversion Conversion, Sampling Sampling, WrapMode Wrap)
    : IStateRecord<PolarCoordinates, PolarCoordinatesParameter>, IPixelStage<PolarCoordinates> {
    public static PolarCoordinates Default { get; } = new(PolarConversion.RectangularToPolar, Sampling.Linear, WrapMode.Black);

    public static Option<PixelPass> Pass(PolarCoordinates state, PassContext context) =>
        Some<PixelPass>(new PixelPass.Frame(new CoordinateMap.Analytic((points, _, extent) => {
            foreach (ref (double X, double Y) point in points) point = state.Conversion.Source(point, extent);
        }, state.Sampling, (state.Wrap, state.Wrap)).Apply));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PolarCoordinatesParameter : IStateParameter<PolarCoordinates> {
    public static readonly PolarCoordinatesParameter Conversion = new("conversion", new StateParameter<PolarCoordinates>.Choice<PolarConversion>(Lens<PolarCoordinates, PolarConversion>.New(static state => state.Conversion, static conversion => state => state with { Conversion = conversion })),
        new("Conversion", "Direction the frame converts between rows and rings"));
    public static readonly PolarCoordinatesParameter Sampling = new("sampling", new StateParameter<PolarCoordinates>.Choice<Sampling>(Lens<PolarCoordinates, Sampling>.New(static state => state.Sampling, static sampling => state => state with { Sampling = sampling })),
        new("Interpolation", "Filter between source pixels"));
    public static readonly PolarCoordinatesParameter Wrap = new("wrap", new StateParameter<PolarCoordinates>.Choice<WrapMode>(Lens<PolarCoordinates, WrapMode>.New(static state => state.Wrap, static wrap => state => state with { Wrap = wrap })),
        new("Extension", "Sampling past frame edges"));

    public StateParameter<PolarCoordinates> Kind { get; }
    public ParameterText Text { get; }
}
