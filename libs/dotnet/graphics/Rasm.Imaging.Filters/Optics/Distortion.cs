using System.Numerics;
using System.Runtime.Intrinsics;
using MathNet.Numerics;
using Rasm.Imaging.Filters.Generators;
using Rasm.Imaging.Filters.Warp;
using Rasm.Imaging.Pixels;

namespace Rasm.Imaging.Filters.Optics;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Neutral", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct Distortion : IPresented<Distortion, float> {
    public static Distortion MinValue { get; } = new(-0.999f);
    public static Distortion MaxValue { get; } = new(1f);
    public static Presentation<float> Presentation { get; } = new() { Origin = (float)Neutral };

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LensFraming : ICaptioned {
    public static readonly LensFraming Edges = new("edges", static _ => 1d);
    public static readonly LensFraming Corners = new("corners", static extent => (double.Pow(extent.Width, 2d) + double.Pow(extent.Height, 2d)) / double.Pow(int.Max(extent.Width, extent.Height), 2d));

    public double Scale(PixelExtent extent, double distortion) => 1d / (1d + ((distortion > 0d ? Reach(extent) : 1d) * distortion));

    [UseDelegateFromConstructor]
    private partial double Reach(PixelExtent extent);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LensDirection : ICaptioned {
    public static readonly LensDirection Distort = new("distort", static (distortion, scale, radius) =>
        (4d * distortion * scale * scale * radius) switch { var bound => (2d * scale / (1d + double.Sqrt(double.Max(0d, 1d - bound))), bound > 1d ? 0f : 1f) });
    public static readonly LensDirection Undistort = new("undistort", static (distortion, scale, radius) =>
        (distortion * radius) switch { var bound => bound > -1d ? (1d / ((1d + bound) * scale), bound > 1d ? 0f : 1f) : (0d, 0f) });

    [UseDelegateFromConstructor]
    internal partial (double Factor, float Weight) Radial(double distortion, double scale, double radius);
}

public sealed record LensDistortion(Distortion Distortion, AxisFraction Dispersion, LensFraming Framing, LensDirection Direction, WrapMode Wrap)
    : IStateRecord<LensDistortion, LensDistortionParameter>, IPixelStage<LensDistortion> {
    public static LensDistortion Default { get; } = new(Distortion.Create(0.05f), AxisFraction.Create(0.05f), LensFraming.Edges, LensDirection.Distort, WrapMode.Clamp);

    public CoordinateMap Placed => Step(Distortion);

    public static Option<PixelPass> Pass(LensDistortion state, PassContext context) =>
        state.Distortion == Distortion.Neutral && state.Dispersion == AxisFraction.MinValue ? None
            : Some<PixelPass>(new PixelPass.Frame(
                state.Dispersion == AxisFraction.MinValue ? state.Placed.Apply
                    : Vector3.Clamp(new(state.Distortion + (state.Dispersion / 4f), state.Distortion, state.Distortion - (state.Dispersion / 4f)), new(Distortion.MinValue), new(Distortion.MaxValue)) switch {
                        var d => CoordinateMap.IntegratedLinear(static integral => ContourIntegrate.GaussKronrod(integral, 0d, 1d), state.Wrap,
                            (static t => new Vector4((float)(1d - t), (float)t, 0f, 1f), (point, t, extent) => state.Mapped(double.Lerp(d.X, d.Y, t), point, extent)),
                            (static t => new Vector4(0f, (float)(1d - t), (float)t, 1f), (point, t, extent) => state.Mapped(double.Lerp(d.Y, d.Z, t), point, extent))),
                    }));

    private CoordinateMap.Analytic Step(float distortion) =>
        new((points, coverage, extent) => {
            for (int i = 0; i < points.Length; i++)
                (points[i], coverage[i]) = Mapped(distortion, points[i], extent) switch { var (point, weight) => (point, weight * coverage[i]) };
        }, Sampling.Linear, (Wrap, Wrap));

    private ((double X, double Y) Point, float Coverage) Mapped(double distortion, (double X, double Y) point, PixelExtent extent) {
        (double x, double y) = (extent.Width / 2d, extent.Height / 2d);
        (float red, float blue) = (float.Min(Distortion + (Dispersion / 4f), Distortion.MaxValue), float.Max(Distortion - (Dispersion / 4f), Distortion.MinValue));
        double half = double.Max(x, y);
        (double scale, double radius) = (Framing.Scale(extent, red), double.Pow((point.X - x) / half, 2d) + double.Pow((point.Y - y) / half, 2d));
        double factor = Direction.Radial(distortion, scale, radius).Factor;
        return ((x + (factor * (point.X - x)), y + (factor * (point.Y - y))), Direction.Radial(red, scale, radius).Weight * Direction.Radial(blue, scale, radius).Weight);
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LensDistortionParameter : IStateParameter<LensDistortion> {
    public static readonly LensDistortionParameter Distortion = new("distortion", StateParameter.Bounded(Lens<LensDistortion, Distortion>.New(static lens => lens.Distortion, static distortion => lens => lens with { Distortion = distortion })),
        new("Distortion", "Bends straight lines away from or toward frame center"));
    public static readonly LensDistortionParameter Dispersion = new("dispersion", StateParameter.Bounded(Lens<LensDistortion, AxisFraction>.New(static lens => lens.Dispersion, static dispersion => lens => lens with { Dispersion = dispersion })),
        new("Dispersion", "Spreads red and blue over continuous spectrum, more toward edges"));
    public static readonly LensDistortionParameter Framing = new("framing", new StateParameter<LensDistortion>.Choice<LensFraming>(Lens<LensDistortion, LensFraming>.New(static lens => lens.Framing, static framing => lens => lens with { Framing = framing })),
        new("Fit", "Scales warped frame to fill edges or corners"));
    public static readonly LensDistortionParameter Direction = new("direction", new StateParameter<LensDistortion>.Choice<LensDirection>(Lens<LensDistortion, LensDirection>.New(static lens => lens.Direction, static direction => lens => lens with { Direction = direction })),
        new("Direction", "Applies or removes lens warp"));
    public static readonly LensDistortionParameter Wrap = new("wrap", new StateParameter<LensDistortion>.Choice<WrapMode>(Lens<LensDistortion, WrapMode>.New(static lens => lens.Wrap, static wrap => lens => lens with { Wrap = wrap })),
        new("Wrap", "Source sampling past frame edges"));

    public StateParameter<LensDistortion> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record AxialAberration(ShortSideLength Radius, Iris Iris)
    : IStateRecord<AxialAberration, AxialAberrationParameter>, IPixelStage<AxialAberration> {
    public static AxialAberration Default { get; } = new(ShortSideLength.Create(20f / ReferenceFrame.Height), Defocus.Default.Iris);

    public static Option<PixelPass> Pass(AxialAberration state, PassContext context) =>
        state.Radius == ShortSideLength.Neutral ? None : Some<PixelPass>(Defocus.Gathered(state.Iris, SignedFraction.Neutral, (light, _, _) =>
            Defocus.Correlate(light, state.Iris, SignedFraction.Neutral, Vector256.Create(0.5d, 0d, 1d, 0d) * state.Radius.Pixels(context.Extent))));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class AxialAberrationParameter : IStateParameter<AxialAberration> {
    public static readonly AxialAberrationParameter Radius = new("radius", StateParameter.Bounded(Lens<AxialAberration, ShortSideLength>.New(static aberration => aberration.Radius, static radius => aberration => aberration with { Radius = radius }), ShortSideLength.Presentation with { Soft = (ShortSideLength.MinValue, 80f / ReferenceFrame.Height) }),
        new("Radius", "Blue defocus radius against green, red at half"));
    public static readonly AxialAberrationParameter Iris = new("iris", new StateParameter<AxialAberration>.Record<Iris>(Lens<AxialAberration, Iris>.New(static aberration => aberration.Iris, static iris => aberration => aberration with { Iris = iris })),
        new("Aperture", "Lens opening shaping bokeh and diffraction spikes"));

    public StateParameter<AxialAberration> Kind { get; }
    public ParameterText Text { get; }
}
