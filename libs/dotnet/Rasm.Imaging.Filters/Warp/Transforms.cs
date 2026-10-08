using System.Numerics;
using Rasm.Imaging.Filters.Generators;
using Rasm.Imaging.Pixels;
using UnitsNet.Units;

namespace Rasm.Imaging.Filters.Warp;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidWarp>]
public readonly partial struct TransformScale : IMinMaxValue<TransformScale> {
    public static TransformScale MinValue { get; } = new(1f / (1 << 15));
    public static TransformScale MaxValue { get; } = new(1 << 15);
    public static TransformScale Identity { get; } = new(1f);

    static partial void ValidateFactoryArguments(ref InvalidWarp? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidWarp();
}

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Neutral", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidWarp>]
public readonly partial struct SkewAngle : IMinMaxValue<SkewAngle> {
    public static SkewAngle MinValue { get; } = new(-float.BitDecrement(float.Pi) / 2f);
    public static SkewAngle MaxValue { get; } = new(float.BitDecrement(float.Pi) / 2f);

    static partial void ValidateFactoryArguments(ref InvalidWarp? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidWarp();
}

internal readonly record struct Pose(Vector2 Offset, float Rotation, Vector2 Scale, Vector2 Skew, Vector2 Pivot) {
    public Pose Partial(float share) => new(Offset * share, Rotation * share, new Vector2(float.Pow(Scale.X, share), float.Pow(Scale.Y, share)), Skew * share, Pivot);

    public Matrix3x2 Inverse =>
        Matrix3x2.CreateTranslation(-(Pivot + Offset))
        * Matrix3x2.CreateRotation(Rotation)
        * Matrix3x2.CreateSkew(0f, -Skew.Y)
        * Matrix3x2.CreateSkew(-Skew.X, 0f)
        * Matrix3x2.CreateScale(Vector2.One / Scale)
        * Matrix3x2.CreateTranslation(Pivot);

    public static CoordinateMap.Analytic Moved(Matrix3x2 inverse, Sampling sampling, EdgeMode wrap) =>
        new((points, _, _) => {
            foreach (ref Vector2 point in points) point = Vector2.Transform(point, inverse);
        }, sampling, wrap);
}

public sealed record AffineTransform(
    ShortSideOffset OffsetX, ShortSideOffset OffsetY, SignedAngle Rotation, TransformScale ScaleX, TransformScale ScaleY, SkewAngle SkewX, SkewAngle SkewY,
    FramePosition Pivot, ShortSideOffset TravelX, ShortSideOffset TravelY, SignedAngle Turn, TransformScale Zoom, ShutterSamples Samples, Sampling Sampling, EdgeMode Wrap)
    : IStateRecord<AffineTransform, AffineTransformParameter, InvalidWarp>, IPixelStage<AffineTransform> {
    public static AffineTransform Default { get; } = new(
        ShortSideOffset.Neutral, ShortSideOffset.Neutral, SignedAngle.Neutral, TransformScale.Identity, TransformScale.Identity, SkewAngle.Neutral, SkewAngle.Neutral,
        FramePosition.Center, ShortSideOffset.Neutral, ShortSideOffset.Neutral, SignedAngle.Neutral, TransformScale.Identity, ShutterSamples.MinValue, Sampling.Linear, EdgeMode.Black);

    public static Option<PixelPass> Pass(AffineTransform state, PassContext context) =>
        (state.Placement(context.Extent).Inverse, state.Motion(context.Extent)) switch {
            ( { IsIdentity: true }, { Inverse.IsIdentity: true }) => None,
            (var placed, { Inverse.IsIdentity: true }) => Some<PixelPass>(new PixelPass.Frame(Pose.Moved(placed, state.Sampling, state.Wrap).Apply)),
            (var placed, var motion) => Some<PixelPass>(new PixelPass.Frame(CoordinateMap.Integrated(
                state.Samples.Count(context.Extent),
                share => (Pose.Moved(motion.Partial(share).Inverse * placed, state.Sampling, state.Wrap), Vector4.One)))),
        };

    private Pose Placement(PixelExtent extent) =>
        new(new Vector2(OffsetX.Pixels(extent), OffsetY.Pixels(extent)), Rotation, new Vector2(ScaleX, ScaleY), new Vector2(SkewX, SkewY), Pivot.Point(extent));

    private Pose Motion(PixelExtent extent) =>
        new(new Vector2(TravelX.Pixels(extent), TravelY.Pixels(extent)), Turn, new Vector2(Zoom), Vector2.Zero, Pivot.Point(extent));
}

[SmartEnum<string>]
[ValidationError<InvalidWarp>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class AffineTransformParameter : IStateParameter<AffineTransform> {
    private static readonly (StateParameter<AffineTransform> X, StateParameter<AffineTransform> Y) Pivoted =
        FramePosition.Kinds(Lens<AffineTransform, FramePosition>.New(static state => state.Pivot, static pivot => state => state with { Pivot = pivot }));
    private static readonly Presentation<SkewAngle, float> Shear =
        new() { Unit = UnitsNet.Quantity.GetUnitInfo(AngleUnit.Radian), Soft = float.Atan(3f) switch { var reach => (-reach, reach) }, Origin = (float)SkewAngle.Neutral };

    public static readonly AffineTransformParameter OffsetX = new("offset-x", new StateParameter<AffineTransform>.Bounded<ShortSideOffset, float, InvalidPixelValue>(
        Lens<AffineTransform, ShortSideOffset>.New(static state => state.OffsetX, static offset => state => state with { OffsetX = offset }), Presentations.Travel));
    public static readonly AffineTransformParameter OffsetY = new("offset-y", new StateParameter<AffineTransform>.Bounded<ShortSideOffset, float, InvalidPixelValue>(
        Lens<AffineTransform, ShortSideOffset>.New(static state => state.OffsetY, static offset => state => state with { OffsetY = offset }), Presentations.Travel));
    public static readonly AffineTransformParameter Rotation = new("rotation", new StateParameter<AffineTransform>.Bounded<SignedAngle, float, InvalidPixelValue>(
        Lens<AffineTransform, SignedAngle>.New(static state => state.Rotation, static rotation => state => state with { Rotation = rotation }), Presentations.Angle));
    public static readonly AffineTransformParameter ScaleX = new("scale-x", new StateParameter<AffineTransform>.Bounded<TransformScale, float, InvalidWarp>(
        Lens<AffineTransform, TransformScale>.New(static state => state.ScaleX, static scale => state => state with { ScaleX = scale }), Presentations.Scale));
    public static readonly AffineTransformParameter ScaleY = new("scale-y", new StateParameter<AffineTransform>.Bounded<TransformScale, float, InvalidWarp>(
        Lens<AffineTransform, TransformScale>.New(static state => state.ScaleY, static scale => state => state with { ScaleY = scale }), Presentations.Scale));
    public static readonly AffineTransformParameter SkewX = new("skew-x", new StateParameter<AffineTransform>.Bounded<SkewAngle, float, InvalidWarp>(
        Lens<AffineTransform, SkewAngle>.New(static state => state.SkewX, static skew => state => state with { SkewX = skew }), Shear));
    public static readonly AffineTransformParameter SkewY = new("skew-y", new StateParameter<AffineTransform>.Bounded<SkewAngle, float, InvalidWarp>(
        Lens<AffineTransform, SkewAngle>.New(static state => state.SkewY, static skew => state => state with { SkewY = skew }), Shear));
    public static readonly AffineTransformParameter PivotX = new("pivot-x", Pivoted.X);
    public static readonly AffineTransformParameter PivotY = new("pivot-y", Pivoted.Y);
    public static readonly AffineTransformParameter TravelX = new("travel-x", new StateParameter<AffineTransform>.Bounded<ShortSideOffset, float, InvalidPixelValue>(
        Lens<AffineTransform, ShortSideOffset>.New(static state => state.TravelX, static travel => state => state with { TravelX = travel }), Presentations.Travel));
    public static readonly AffineTransformParameter TravelY = new("travel-y", new StateParameter<AffineTransform>.Bounded<ShortSideOffset, float, InvalidPixelValue>(
        Lens<AffineTransform, ShortSideOffset>.New(static state => state.TravelY, static travel => state => state with { TravelY = travel }), Presentations.Travel));
    public static readonly AffineTransformParameter Turn = new("turn", new StateParameter<AffineTransform>.Bounded<SignedAngle, float, InvalidPixelValue>(
        Lens<AffineTransform, SignedAngle>.New(static state => state.Turn, static turn => state => state with { Turn = turn }), Presentations.Angle));
    public static readonly AffineTransformParameter Zoom = new("zoom", new StateParameter<AffineTransform>.Bounded<TransformScale, float, InvalidWarp>(
        Lens<AffineTransform, TransformScale>.New(static state => state.Zoom, static zoom => state => state with { Zoom = zoom }), Presentations.Scale));
    public static readonly AffineTransformParameter Samples = new("samples", new StateParameter<AffineTransform>.Bounded<ShutterSamples, int, InvalidWarp>(
        Lens<AffineTransform, ShutterSamples>.New(static state => state.Samples, static samples => state => state with { Samples = samples }), Presentations.Samples));
    public static readonly AffineTransformParameter Sampling = new("sampling", new StateParameter<AffineTransform>.Choice<Sampling, InvalidWarp>(
        Lens<AffineTransform, Sampling>.New(static state => state.Sampling, static sampling => state => state with { Sampling = sampling })));
    public static readonly AffineTransformParameter Wrap = new("wrap", new StateParameter<AffineTransform>.Choice<EdgeMode, InvalidWarp>(
        Lens<AffineTransform, EdgeMode>.New(static state => state.Wrap, static wrap => state => state with { Wrap = wrap })));

    public StateParameter<AffineTransform> Kind { get; }
}

public sealed record CornerPin(FramePosition TopLeft, FramePosition TopRight, FramePosition BottomRight, FramePosition BottomLeft, Sampling Sampling, EdgeMode Wrap)
    : IStateRecord<CornerPin, CornerPinParameter, InvalidWarp>, IPixelStage<CornerPin> {
    public static CornerPin Default { get; } = new(FramePosition.TopLeft, FramePosition.TopRight, FramePosition.BottomRight, FramePosition.BottomLeft, Sampling.Linear, EdgeMode.Black);

    public static Option<PixelPass> Pass(CornerPin state, PassContext context) =>
        state with { Sampling = Default.Sampling, Wrap = Default.Wrap } == Default
        || FrameQuad.Validate(state.TopLeft.Point(context.Extent), state.TopRight.Point(context.Extent), state.BottomRight.Point(context.Extent), state.BottomLeft.Point(context.Extent), out FrameQuad? quad) is not null
            ? None
            : Some<PixelPass>(new PixelPass.Frame(new CoordinateMap.Analytic(Pinned(quad!.Homography.Inverse, context.Extent), state.Sampling, state.Wrap).Apply));

    private static Action<Span<Vector2>, Span<float>, PixelExtent> Pinned(Homography inverse, PixelExtent extent) {
        Vector2 size = new(extent.Width, extent.Height);
        return (points, coverage, _) => {
            for (int i = 0; i < points.Length; i++)
                (points[i], coverage[i]) = inverse.Apply(points[i]) switch {
                    { IsSome: true } uv => ((Vector2)uv * size, coverage[i]),
                    _ => (points[i], 0f),
                };
        };
    }
}

[SmartEnum<string>]
[ValidationError<InvalidWarp>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CornerPinParameter : IStateParameter<CornerPin> {
    private static readonly (StateParameter<CornerPin> X, StateParameter<CornerPin> Y) TopLeftAxes =
        FramePosition.Kinds(Lens<CornerPin, FramePosition>.New(static state => state.TopLeft, static corner => state => state with { TopLeft = corner }));
    private static readonly (StateParameter<CornerPin> X, StateParameter<CornerPin> Y) TopRightAxes =
        FramePosition.Kinds(Lens<CornerPin, FramePosition>.New(static state => state.TopRight, static corner => state => state with { TopRight = corner }));
    private static readonly (StateParameter<CornerPin> X, StateParameter<CornerPin> Y) BottomRightAxes =
        FramePosition.Kinds(Lens<CornerPin, FramePosition>.New(static state => state.BottomRight, static corner => state => state with { BottomRight = corner }));
    private static readonly (StateParameter<CornerPin> X, StateParameter<CornerPin> Y) BottomLeftAxes =
        FramePosition.Kinds(Lens<CornerPin, FramePosition>.New(static state => state.BottomLeft, static corner => state => state with { BottomLeft = corner }));

    public static readonly CornerPinParameter TopLeftX = new("top-left-x", TopLeftAxes.X);
    public static readonly CornerPinParameter TopLeftY = new("top-left-y", TopLeftAxes.Y);
    public static readonly CornerPinParameter TopRightX = new("top-right-x", TopRightAxes.X);
    public static readonly CornerPinParameter TopRightY = new("top-right-y", TopRightAxes.Y);
    public static readonly CornerPinParameter BottomRightX = new("bottom-right-x", BottomRightAxes.X);
    public static readonly CornerPinParameter BottomRightY = new("bottom-right-y", BottomRightAxes.Y);
    public static readonly CornerPinParameter BottomLeftX = new("bottom-left-x", BottomLeftAxes.X);
    public static readonly CornerPinParameter BottomLeftY = new("bottom-left-y", BottomLeftAxes.Y);
    public static readonly CornerPinParameter Sampling = new("sampling", new StateParameter<CornerPin>.Choice<Sampling, InvalidWarp>(
        Lens<CornerPin, Sampling>.New(static state => state.Sampling, static sampling => state => state with { Sampling = sampling })));
    public static readonly CornerPinParameter Wrap = new("wrap", new StateParameter<CornerPin>.Choice<EdgeMode, InvalidWarp>(
        Lens<CornerPin, EdgeMode>.New(static state => state.Wrap, static wrap => state => state with { Wrap = wrap })));

    public StateParameter<CornerPin> Kind { get; }
}

public sealed record Shake(
    ShortSideLength AmplitudeX, ShortSideLength AmplitudeY, SignedAngle Rotation, TransformScale Zoom, Frequency Rate, Octaves Octaves,
    Seed Seed, Timing Timing, ShutterTime Shutter, ShutterSamples Samples, Sampling Sampling, EdgeMode Wrap)
    : IStateRecord<Shake, ShakeParameter, InvalidWarp>, IPixelStage<Shake> {
    public static Shake Default { get; } = new(
        ShortSideLength.Neutral, ShortSideLength.Neutral, SignedAngle.Neutral, TransformScale.Identity, Frequency.Wiggle, Octaves.Standard,
        Seed.MinValue, Timing.Standard, ShutterTime.Instant, ShutterSamples.MinValue, Sampling.Linear, EdgeMode.Periodic);

    public static Option<PixelPass> Pass(Shake state, PassContext context) =>
        (state.AmplitudeX, state.AmplitudeY, state.Rotation, state.Zoom) == (ShortSideLength.Neutral, ShortSideLength.Neutral, SignedAngle.Neutral, TransformScale.Identity)
            ? None
            : Some<PixelPass>(new PixelPass.Frame(state.Kernel(context)));

    private Func<PixelFrame, IProgress<int>, Fin<Unit>> Kernel(PassContext context) {
        float time = Timing.At(context);
        return Shutter == ShutterTime.Instant
            ? Pose.Moved(Posed(context.Extent, time).Inverse, Sampling, Wrap).Apply
            : CoordinateMap.Integrated(
                Samples.Count(context.Extent),
                share => (Pose.Moved(Posed(context.Extent, time - (Shutter * (1f - share))).Inverse, Sampling, Wrap), Vector4.One));
    }

    private Pose Posed(PixelExtent extent, float time) {
        uint field = CoordinateHash.Field(NoiseStream.Shake, Seed, 0u);
        Vector4 at = new(time * Rate, 0f, 0f, 0f);
        Vector4 shake = new(
            NoiseDimensions.One.Fbm(at, Octaves, CoordinateHash.Branch(field, 0u)), NoiseDimensions.One.Fbm(at, Octaves, CoordinateHash.Branch(field, 1u)),
            NoiseDimensions.One.Fbm(at, Octaves, CoordinateHash.Branch(field, 2u)), NoiseDimensions.One.Fbm(at, Octaves, CoordinateHash.Branch(field, 3u)));
        return new(
            int.Min(extent.Width, extent.Height) * new Vector2(AmplitudeX * shake.X, AmplitudeY * shake.Y), Rotation * shake.Z,
            new Vector2(float.Pow(Zoom, shake.W)), Vector2.Zero, new Vector2(extent.Width, extent.Height) / 2f);
    }
}

[SmartEnum<string>]
[ValidationError<InvalidWarp>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ShakeParameter : IStateParameter<Shake> {
    private static readonly (StateParameter<Shake> Clock, StateParameter<Shake> Pace) Time =
        Timing.Kinds(Lens<Shake, Timing>.New(static state => state.Timing, static timing => state => state with { Timing = timing }));
    private static readonly (StateParameter<Shake> Detail, StateParameter<Shake> Roughness, StateParameter<Shake> Lacunarity) Fractal =
        Octaves.Kinds(Lens<Shake, Octaves>.New(static state => state.Octaves, static octaves => state => state with { Octaves = octaves }));
    private static readonly Presentation<ShortSideLength, float> Amplitude = new() { Unit = UnitsNet.Quantity.GetUnitInfo(RatioUnit.DecimalFraction), Soft = (0f, 0.1f) };

    public static readonly ShakeParameter AmplitudeX = new("amplitude-x", new StateParameter<Shake>.Bounded<ShortSideLength, float, InvalidPixelValue>(
        Lens<Shake, ShortSideLength>.New(static state => state.AmplitudeX, static amplitude => state => state with { AmplitudeX = amplitude }), Amplitude));
    public static readonly ShakeParameter AmplitudeY = new("amplitude-y", new StateParameter<Shake>.Bounded<ShortSideLength, float, InvalidPixelValue>(
        Lens<Shake, ShortSideLength>.New(static state => state.AmplitudeY, static amplitude => state => state with { AmplitudeY = amplitude }), Amplitude));
    public static readonly ShakeParameter Rotation = new("rotation", new StateParameter<Shake>.Bounded<SignedAngle, float, InvalidPixelValue>(
        Lens<Shake, SignedAngle>.New(static state => state.Rotation, static rotation => state => state with { Rotation = rotation }), Presentations.HalfTurn));
    public static readonly ShakeParameter Zoom = new("zoom", new StateParameter<Shake>.Bounded<TransformScale, float, InvalidWarp>(
        Lens<Shake, TransformScale>.New(static state => state.Zoom, static zoom => state => state with { Zoom = zoom }),
        new() { Soft = (1f, 2f), Scale = TrackScale.Log, Origin = (float)TransformScale.Identity }));
    public static readonly ShakeParameter Rate = new("rate", new StateParameter<Shake>.Bounded<Frequency, float, InvalidGenerator>(
        Lens<Shake, Frequency>.New(static state => state.Rate, static rate => state => state with { Rate = rate }),
        Frequency.Presentation with { Soft = (0.1f, 20f), Scale = TrackScale.Log }));
    public static readonly ShakeParameter Detail = new("detail", Fractal.Detail);
    public static readonly ShakeParameter Roughness = new("roughness", Fractal.Roughness);
    public static readonly ShakeParameter Lacunarity = new("lacunarity", Fractal.Lacunarity);
    public static readonly ShakeParameter Seed = new("seed", new StateParameter<Shake>.Bounded<Seed, int, InvalidGenerator>(
        Lens<Shake, Seed>.New(static state => state.Seed, static seed => state => state with { Seed = seed }), Generators.Seed.Presentation));
    public static readonly ShakeParameter Clock = new("clock", Time.Clock);
    public static readonly ShakeParameter Pace = new("pace", Time.Pace);
    public static readonly ShakeParameter Shutter = new("shutter", new StateParameter<Shake>.Bounded<ShutterTime, float, InvalidGenerator>(
        Lens<Shake, ShutterTime>.New(static state => state.Shutter, static shutter => state => state with { Shutter = shutter }),
        ShutterTime.Presentation));
    public static readonly ShakeParameter Samples = new("samples", new StateParameter<Shake>.Bounded<ShutterSamples, int, InvalidWarp>(
        Lens<Shake, ShutterSamples>.New(static state => state.Samples, static samples => state => state with { Samples = samples }), Presentations.Samples));
    public static readonly ShakeParameter Sampling = new("sampling", new StateParameter<Shake>.Choice<Sampling, InvalidWarp>(
        Lens<Shake, Sampling>.New(static state => state.Sampling, static sampling => state => state with { Sampling = sampling })));
    public static readonly ShakeParameter Wrap = new("wrap", new StateParameter<Shake>.Choice<EdgeMode, InvalidWarp>(
        Lens<Shake, EdgeMode>.New(static state => state.Wrap, static wrap => state => state with { Wrap = wrap })));

    public StateParameter<Shake> Kind { get; }
}
