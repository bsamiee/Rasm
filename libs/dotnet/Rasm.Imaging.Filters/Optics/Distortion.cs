using System.Numerics;
using Emgu.CV;
using Rasm.Imaging.Filters.Warp;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Pixels;
using UnitsNet;
using UnitsNet.Units;

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

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Neutral", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidOptics>]
public readonly partial struct Dispersion : IMinMaxValue<Dispersion> {
    public static Dispersion MinValue => Neutral;
    public static Dispersion MaxValue { get; } = new(1f);

    static partial void ValidateFactoryArguments(ref InvalidOptics? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidOptics();
}

[SmartEnum<string>]
[ValidationError<InvalidOptics>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LensFraming {
    public static readonly LensFraming Edges = new("edges", reach: 1f);
    public static readonly LensFraming Corners = new("corners", reach: 2f);

    private readonly float _reach;

    public float Scale(float widest) => 1f / (1f + ((widest > 0f ? _reach : 1f) * widest));
}

public sealed record LensDistortion(Distortion Distortion, Dispersion Dispersion, LensFraming Framing)
    : IStateRecord<LensDistortion, LensDistortionParameter, InvalidOptics>, IPixelStage<LensDistortion> {
    public static LensDistortion Default { get; } = new(Distortion.Neutral, Dispersion.Neutral, LensFraming.Edges);

    private Vector3 Distortions =>
        Vector3.Clamp(new(Distortion + (Dispersion / 4f), Distortion, Distortion - (Dispersion / 4f)), new(Distortion.MinValue), new(Distortion.MaxValue));

    private Vector3 Coefficients => 4f * Distortions;

    public static Option<PixelPass> Pass(LensDistortion state, PassContext context) =>
        (state.Distortion == Distortion.Neutral, state.Dispersion == Dispersion.Neutral) switch {
            (true, true) => None,
            (_, true) => Some<PixelPass>(new PixelPass.Frame(state.Placed(context.Extent).Apply)),
            _ => Some<PixelPass>(new PixelPass.Frame(CoordinateMap.Integrated(state.Steps(context.Extent), state.Subframe(context.Extent)))),
        };

    public CoordinateMap.Analytic Placed(PixelExtent extent) => Step(Coefficients.Y, extent);

    private CoordinateMap.Analytic Step(float k, PixelExtent extent) {
        (float scale, float bound) = (Framing.Scale(Distortions.X), Coefficients.X);
        Vector2 center = new Vector2(extent.Width, extent.Height) / 2f;
        float reach = 2f * scale / extent.Width;
        return new((points, coverage, _) => {
            for (int i = 0; i < points.Length; i++) {
                Vector2 offset = points[i] - center;
                float r2 = reach * reach * offset.LengthSquared();
                (points[i], coverage[i]) = (center + (2f * scale * Spread(k, r2) * offset), bound * r2 > 1f ? 0f : coverage[i]);
            }
        }, Sampling.Linear, EdgeMode.Black);
    }

    private int Steps(PixelExtent extent) {
        Vector3 k = Coefficients;
        float corner = Framing.Scale(Distortions.X) * new Vector2(extent.Width, extent.Height).Length() / extent.Width;
        float r2 = k.X > 0f ? float.Min(corner * corner, 1f / k.X) : corner * corner;
        (float red, float green, float blue) = (Spread(k.X, r2), Spread(k.Y, r2), Spread(k.Z, r2));
        return 2 * (int)((extent.Width * float.Sqrt(r2) * float.Max(red - green, green - blue)) + 1f);
    }

    private Func<float, (CoordinateMap Map, Vector4 Lanes)> Subframe(PixelExtent extent) {
        Vector3 k = Coefficients;
        return tau => (2f * tau) switch {
            <= 1f and var t => (Step(float.Lerp(k.X, k.Y, t), extent), (t == 0f ? 0.5f : 1f) * new Vector4(1f - t, t, 0f, 1f)),
            var t => (Step(float.Lerp(k.Y, k.Z, t - 1f), extent), (t == 2f ? 0.5f : 1f) * new Vector4(0f, 2f - t, t - 1f, 1f)),
        };
    }

    private static float Spread(float k, float r2) => 1f / (1f + float.Sqrt(float.Max(0f, 1f - (k * r2))));
}

[SmartEnum<string>]
[ValidationError<InvalidOptics>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LensDistortionParameter : IStateParameter<LensDistortion> {
    public static readonly LensDistortionParameter Distortion = new(
        "distortion",
        new StateParameter<LensDistortion>.Bounded<Distortion, float, InvalidOptics>(
            Lens<LensDistortion, Distortion>.New(static lens => lens.Distortion, static distortion => lens => lens with { Distortion = distortion }),
            new() { Origin = (float)Optics.Distortion.Neutral }));
    public static readonly LensDistortionParameter Dispersion = new(
        "dispersion",
        new StateParameter<LensDistortion>.Bounded<Dispersion, float, InvalidOptics>(
            Lens<LensDistortion, Dispersion>.New(static lens => lens.Dispersion, static dispersion => lens => lens with { Dispersion = dispersion }), new()));
    public static readonly LensDistortionParameter Framing = new(
        "framing",
        new StateParameter<LensDistortion>.Choice<LensFraming, InvalidOptics>(
            Lens<LensDistortion, LensFraming>.New(static lens => lens.Framing, static framing => lens => lens with { Framing = framing })));

    public StateParameter<LensDistortion> Kind { get; }
}

public sealed record AxialAberration(ShortSideLength Radius, Iris Iris)
    : IStateRecord<AxialAberration, AxialAberrationParameter, InvalidOptics>, IPixelStage<AxialAberration> {
    public static AxialAberration Default { get; } = new(ShortSideLength.Create(20f / 1080f), Iris.Hexagon with { Roundness = AxisFraction.MaxValue });

    public static Option<PixelPass> Pass(AxialAberration state, PassContext context) =>
        state.Radius == ShortSideLength.Neutral
            ? None
            : Some<PixelPass>(new PixelPass.Frame((frame, progress) => Kernel(state.Iris, state.Radius.Pixels(context.Extent), frame, progress)));

    private static Fin<Unit> Kernel(Iris iris, float radius, PixelFrame frame, IProgress<int> progress) {
        using Mat header = frame.Header();
        using Mat plane = new();
        foreach ((int lane, float blur) in (ReadOnlySpan<(int, float)>)[(0, radius / 2f), (2, radius)]) {
            CvInvoke.ExtractChannel(header, plane, lane);
            Defocus.Correlate(plane, iris, blur, lane);
            CvInvoke.InsertChannel(plane, header, lane);
        }
        progress.Report(frame.Size.Height);
        return unit;
    }
}

[SmartEnum<string>]
[ValidationError<InvalidOptics>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class AxialAberrationParameter : IStateParameter<AxialAberration> {
    private static readonly (StateParameter<AxialAberration> Blades, StateParameter<AxialAberration> Rotation, StateParameter<AxialAberration> Roundness, StateParameter<AxialAberration> Obstruction, StateParameter<AxialAberration> Squeeze)
        Outline = Iris.Kinds(Lens<AxialAberration, Iris>.New(static aberration => aberration.Iris, static iris => aberration => aberration with { Iris = iris }));

    public static readonly AxialAberrationParameter Radius = new(
        "radius",
        new StateParameter<AxialAberration>.Bounded<ShortSideLength, float, InvalidPixelValue>(
            Lens<AxialAberration, ShortSideLength>.New(static aberration => aberration.Radius, static radius => aberration => aberration with { Radius = radius }),
            new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction), Soft = (0f, 80f / 1080f) }));
    public static readonly AxialAberrationParameter Blades = new("blades", Outline.Blades);
    public static readonly AxialAberrationParameter Rotation = new("rotation", Outline.Rotation);
    public static readonly AxialAberrationParameter Roundness = new("roundness", Outline.Roundness);
    public static readonly AxialAberrationParameter Obstruction = new("obstruction", Outline.Obstruction);
    public static readonly AxialAberrationParameter Squeeze = new("squeeze", Outline.Squeeze);

    public StateParameter<AxialAberration> Kind { get; }
}
