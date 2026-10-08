using System.Numerics;
using Emgu.CV;
using Rasm.Imaging.Filters.Generators;
using Rasm.Imaging.Filters.Warp;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Pixels;

namespace Rasm.Imaging.Filters.Optics;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Neutral", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidOptics>]
public readonly partial struct Distortion : IMinMaxValue<Distortion> {
    public static Distortion MinValue { get; } = new(-0.999f);
    public static Distortion MaxValue { get; } = new(1f);

    static partial void ValidateFactoryArguments(ref InvalidOptics? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidOptics();
}

[SmartEnum<string>]
[ValidationError<InvalidOptics>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LensFraming {
    public static readonly LensFraming Edges = new("edges", static _ => 1f);
    public static readonly LensFraming Corners = new("corners", static extent => (new Vector2(extent.Width, extent.Height) / int.Max(extent.Width, extent.Height)).LengthSquared());

    public float Scale(PixelExtent extent, float distortion) => 1f / (1f + ((distortion > 0f ? Reach(extent) : 1f) * distortion));

    [UseDelegateFromConstructor]
    private partial float Reach(PixelExtent extent);
}

[SmartEnum<string>]
[ValidationError<InvalidOptics>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LensDirection {
    public static readonly LensDirection Distort = new("distort", static (distortion, scale, radius) =>
        (4f * distortion * scale * scale * radius) switch { var bound => (2f * scale / (1f + float.Sqrt(float.Max(0f, 1f - bound))), bound > 1f ? 0f : 1f) });
    public static readonly LensDirection Undistort = new("undistort", static (distortion, scale, radius) =>
        (distortion * radius) switch { var bound => bound > -1f ? (1f / ((1f + bound) * scale), bound > 1f ? 0f : 1f) : (0f, 0f) });

    [UseDelegateFromConstructor]
    internal partial (float Factor, float Weight) Radial(float distortion, float scale, float radius);
}

public sealed record LensDistortion(Distortion Distortion, AxisFraction Dispersion, LensFraming Framing, LensDirection Direction, WrapMode Wrap)
    : IStateRecord<LensDistortion, LensDistortionParameter, InvalidOptics>, IPixelStage<LensDistortion> {
    public static LensDistortion Default { get; } = new(Distortion.Create(0.05f), AxisFraction.Create(0.05f), LensFraming.Edges, LensDirection.Distort, WrapMode.Clamp);

    public CoordinateMap Placed => Subframe(0.5f).Map;

    public static Option<PixelPass> Pass(LensDistortion state, PassContext context) =>
        state.Distortion == Distortion.Neutral && state.Dispersion == AxisFraction.MinValue ? None
            : Some<PixelPass>(new PixelPass.Frame(state.Dispersion == AxisFraction.MinValue ? state.Placed.Apply : CoordinateMap.Integrated(state.Subframe)));

    private (CoordinateMap Map, Vector4 Lanes) Subframe(float tau) =>
        (Vector3.Clamp(new(Distortion + (Dispersion / 4f), Distortion, Distortion - (Dispersion / 4f)), new(Distortion.MinValue), new(Distortion.MaxValue)), 2f * tau) switch {
            (var d, <= 1f and var t) => (Step(float.Lerp(d.X, d.Y, t)), (t == 0f ? 0.5f : 1f) * new Vector4(1f - t, t, 0f, 1f)),
            (var d, var t) => (Step(float.Lerp(d.Y, d.Z, t - 1f)), (t == 2f ? 0.5f : 1f) * new Vector4(0f, 2f - t, t - 1f, 1f)),
        };

    private CoordinateMap.Analytic Step(float distortion) =>
        new((points, coverage, extent) => {
            (Vector2 center, float scale) = (new Vector2(extent.Width, extent.Height) / 2f, Framing.Scale(extent, Distortion));
            for (int i = 0; i < points.Length; i++)
                (points[i], coverage[i]) = Direction.Radial(distortion, scale, ((points[i] - center) / float.Max(center.X, center.Y)).LengthSquared()) switch {
                    var (factor, weight) => (center + (factor * (points[i] - center)), weight * coverage[i]),
                };
        }, Sampling.Linear, Wrap);
}

[SmartEnum<string>]
[ValidationError<InvalidOptics>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LensDistortionParameter : IStateParameter<LensDistortion> {
    public static readonly LensDistortionParameter Distortion = new("distortion", new StateParameter<LensDistortion>.Bounded<Distortion, float, InvalidOptics>(Lens<LensDistortion, Distortion>.New(static lens => lens.Distortion, static distortion => lens => lens with { Distortion = distortion }), new() { Origin = (float)Optics.Distortion.Neutral }));
    public static readonly LensDistortionParameter Dispersion = new("dispersion", new StateParameter<LensDistortion>.Bounded<AxisFraction, float, InvalidGrade>(Lens<LensDistortion, AxisFraction>.New(static lens => lens.Dispersion, static dispersion => lens => lens with { Dispersion = dispersion }), new()));
    public static readonly LensDistortionParameter Framing = new("framing", new StateParameter<LensDistortion>.Choice<LensFraming, InvalidOptics>(Lens<LensDistortion, LensFraming>.New(static lens => lens.Framing, static framing => lens => lens with { Framing = framing })));
    public static readonly LensDistortionParameter Direction = new("direction", new StateParameter<LensDistortion>.Choice<LensDirection, InvalidOptics>(Lens<LensDistortion, LensDirection>.New(static lens => lens.Direction, static direction => lens => lens with { Direction = direction })));
    public static readonly LensDistortionParameter Wrap = new("wrap", new StateParameter<LensDistortion>.Choice<WrapMode, InvalidPixelValue>(Lens<LensDistortion, WrapMode>.New(static lens => lens.Wrap, static wrap => lens => lens with { Wrap = wrap })));

    public StateParameter<LensDistortion> Kind { get; }
}

public sealed record AxialAberration(ShortSideLength Radius, Iris Iris)
    : IStateRecord<AxialAberration, AxialAberrationParameter, InvalidOptics>, IPixelStage<AxialAberration> {
    public static AxialAberration Default { get; } = new(ShortSideLength.Create(20f / ReferenceFrame.Height), Defocus.Default.Iris);

    public static Option<PixelPass> Pass(AxialAberration state, PassContext context) =>
        state.Radius == ShortSideLength.Neutral ? None : Some<PixelPass>(new PixelPass.Frame((frame, progress) => {
            using Mat header = frame.Header();
            Defocus.Correlate(header, state.Iris, state.Radius.Pixels(context.Extent) * new Vector4(0.5f, 0f, 1f, 0f));
            progress.Report(frame.Size.Height);
            return unit;
        }));
}

[SmartEnum<string>]
[ValidationError<InvalidOptics>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class AxialAberrationParameter : IStateParameter<AxialAberration> {
    private static readonly (StateParameter<AxialAberration> Blades, StateParameter<AxialAberration> Rotation, StateParameter<AxialAberration> Roundness, StateParameter<AxialAberration> Obstruction, StateParameter<AxialAberration> Squeeze)
        Outline = Iris.Kinds(Lens<AxialAberration, Iris>.New(static aberration => aberration.Iris, static iris => aberration => aberration with { Iris = iris }));

    public static readonly AxialAberrationParameter Radius = new("radius", new StateParameter<AxialAberration>.Bounded<ShortSideLength, float, InvalidPixelValue>(Lens<AxialAberration, ShortSideLength>.New(static aberration => aberration.Radius, static radius => aberration => aberration with { Radius = radius }), ShortSideLength.Presentation with { Soft = (ShortSideLength.MinValue, 80f / ReferenceFrame.Height) }));
    public static readonly AxialAberrationParameter Blades = new("blades", Outline.Blades);
    public static readonly AxialAberrationParameter Rotation = new("rotation", Outline.Rotation);
    public static readonly AxialAberrationParameter Roundness = new("roundness", Outline.Roundness);
    public static readonly AxialAberrationParameter Obstruction = new("obstruction", Outline.Obstruction);
    public static readonly AxialAberrationParameter Squeeze = new("squeeze", Outline.Squeeze);

    public StateParameter<AxialAberration> Kind { get; }
}
