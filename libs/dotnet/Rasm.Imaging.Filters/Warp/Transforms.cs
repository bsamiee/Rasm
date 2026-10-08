using System.Numerics;
using Rasm.Imaging.Filters.Generators;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Pixels;
using UnitsNet.Units;

namespace Rasm.Imaging.Filters.Warp;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidWarp>]
public readonly partial struct TransformScale : IMinMaxValue<TransformScale> {
    public static TransformScale MaxValue { get; } = new(1 << 15);
    public static TransformScale MinValue { get; } = new(1f / MaxValue._value);
    public static TransformScale Identity { get; } = new(1f);
    public static Presentation<TransformScale, float> Presentation { get; } = new() { Soft = (1f / 16f, 16f), Scale = TrackScale.Log, Origin = (float)Identity };

    static partial void ValidateFactoryArguments(ref InvalidWarp? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidWarp();
}

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Neutral", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidWarp>]
public readonly partial struct SkewAngle : IMinMaxValue<SkewAngle> {
    public static SkewAngle MaxValue { get; } = new(float.BitDecrement(float.Pi) / 2f);
    public static SkewAngle MinValue { get; } = new(-MaxValue._value);
    public static Presentation<SkewAngle, float> Presentation { get; } = new() { Unit = UnitsNet.Quantity.GetUnitInfo(AngleUnit.Radian), Soft = float.Atan(3f) switch { var reach => (-reach, reach) }, Origin = (float)Neutral };

    static partial void ValidateFactoryArguments(ref InvalidWarp? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidWarp();
}

internal readonly record struct Pose(Vector2 Offset, float Rotation, Vector2 Scale, Vector2 Skew, Vector2 Pivot) {
    public Pose Partial(float share) => new(Offset * share, Rotation * share, Vector2.Exp(share * Vector2.Log(Scale)), Skew * share, Pivot);

    public Matrix3x2 Inverse =>
        Matrix3x2.CreateTranslation(-Offset) * Matrix3x2.CreateRotation(Rotation, Pivot) * Matrix3x2.CreateSkew(0f, -Skew.Y, Pivot) * Matrix3x2.CreateSkew(-Skew.X, 0f, Pivot) * Matrix3x2.CreateScale(Vector2.One / Scale, Pivot);
}

public sealed record AffineTransform(
    ShortSideOffset OffsetX, ShortSideOffset OffsetY, SignedAngle Rotation, TransformScale ScaleX, TransformScale ScaleY, SkewAngle SkewX, SkewAngle SkewY, FramePosition Pivot,
    ShortSideOffset TravelX, ShortSideOffset TravelY, SignedAngle Turn, TransformScale Zoom, Shake Shake, ShutterTime Shutter, AxisFraction ShutterPosition, Sampling Sampling, WrapMode Wrap)
    : IStateRecord<AffineTransform, AffineTransformParameter, InvalidWarp>, IPixelStage<AffineTransform> {
    public static AffineTransform Default { get; } = new(
        ShortSideOffset.Neutral, ShortSideOffset.Neutral, SignedAngle.Neutral, TransformScale.Identity, TransformScale.Identity, SkewAngle.Neutral, SkewAngle.Neutral, FramePosition.Center,
        ShortSideOffset.Neutral, ShortSideOffset.Neutral, SignedAngle.Neutral, TransformScale.Identity, Shake.Default, ShutterTime.Instant, AxisFraction.Half, Sampling.Linear, WrapMode.Black);

    private bool Moving => (TravelX, TravelY, Turn, Zoom) != (Default.TravelX, Default.TravelY, Default.Turn, Default.Zoom) || (Shutter != ShutterTime.Instant && !Shake.Still);

    public static Option<PixelPass> Pass(AffineTransform state, PassContext context) =>
        state.Path(context) switch {
            var path when state.Moving => Some<PixelPass>(new PixelPass.Frame(CoordinateMap.Integrated(share => (state.Mapped(path(share)), Vector4.One)))),
            var path => path(0f) is { IsIdentity: false } still ? Some<PixelPass>(new PixelPass.Frame(state.Mapped(still).Apply)) : None,
        };

    private Func<float, Matrix3x2> Path(PassContext context) {
        (PixelExtent extent, Vector2 pivot) = (context.Extent, Pivot.Point(context.Extent));
        Matrix3x2 placed = new Pose(new(OffsetX.Pixels(extent), OffsetY.Pixels(extent)), Rotation, new(ScaleX, ScaleY), new(SkewX, SkewY), pivot).Inverse;
        Pose motion = new(new(TravelX.Pixels(extent), TravelY.Pixels(extent)), Turn, new(Zoom), Vector2.Zero, pivot);
        float opened = Shake.Hold.Held(Shake.Timing.At(context)) - (Shutter * ShutterPosition);
        return share => motion.Partial(share - ShutterPosition).Inverse * Shake.Posed(extent, opened + (Shutter * share)).Inverse * placed;
    }

    private CoordinateMap.Analytic Mapped(Matrix3x2 inverse) => new((points, _, _) => {
        foreach (ref Vector2 point in points) point = Vector2.Transform(point, inverse);
    }, Sampling, Wrap);
}

[SmartEnum<string>]
[ValidationError<InvalidWarp>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class AffineTransformParameter : IStateParameter<AffineTransform> {
    private static readonly (StateParameter<AffineTransform> X, StateParameter<AffineTransform> Y) Pivoted = FramePosition.Kinds(Lens<AffineTransform, FramePosition>.New(static state => state.Pivot, static pivot => state => state with { Pivot = pivot }));

    public static readonly AffineTransformParameter OffsetX = new("offset-x", new StateParameter<AffineTransform>.Bounded<ShortSideOffset, float, InvalidPixelValue>(Lens<AffineTransform, ShortSideOffset>.New(static state => state.OffsetX, static offset => state => state with { OffsetX = offset }), ShortSideOffset.Presentation));
    public static readonly AffineTransformParameter OffsetY = new("offset-y", new StateParameter<AffineTransform>.Bounded<ShortSideOffset, float, InvalidPixelValue>(Lens<AffineTransform, ShortSideOffset>.New(static state => state.OffsetY, static offset => state => state with { OffsetY = offset }), ShortSideOffset.Presentation));
    public static readonly AffineTransformParameter Rotation = new("rotation", new StateParameter<AffineTransform>.Bounded<SignedAngle, float, InvalidPixelValue>(Lens<AffineTransform, SignedAngle>.New(static state => state.Rotation, static rotation => state => state with { Rotation = rotation }), SignedAngle.Presentation));
    public static readonly AffineTransformParameter ScaleX = new("scale-x", new StateParameter<AffineTransform>.Bounded<TransformScale, float, InvalidWarp>(Lens<AffineTransform, TransformScale>.New(static state => state.ScaleX, static scale => state => state with { ScaleX = scale }), TransformScale.Presentation));
    public static readonly AffineTransformParameter ScaleY = new("scale-y", new StateParameter<AffineTransform>.Bounded<TransformScale, float, InvalidWarp>(Lens<AffineTransform, TransformScale>.New(static state => state.ScaleY, static scale => state => state with { ScaleY = scale }), TransformScale.Presentation));
    public static readonly AffineTransformParameter SkewX = new("skew-x", new StateParameter<AffineTransform>.Bounded<SkewAngle, float, InvalidWarp>(Lens<AffineTransform, SkewAngle>.New(static state => state.SkewX, static skew => state => state with { SkewX = skew }), SkewAngle.Presentation));
    public static readonly AffineTransformParameter SkewY = new("skew-y", new StateParameter<AffineTransform>.Bounded<SkewAngle, float, InvalidWarp>(Lens<AffineTransform, SkewAngle>.New(static state => state.SkewY, static skew => state => state with { SkewY = skew }), SkewAngle.Presentation));
    public static readonly AffineTransformParameter PivotX = new("pivot-x", Pivoted.X);
    public static readonly AffineTransformParameter PivotY = new("pivot-y", Pivoted.Y);
    public static readonly AffineTransformParameter TravelX = new("travel-x", new StateParameter<AffineTransform>.Bounded<ShortSideOffset, float, InvalidPixelValue>(Lens<AffineTransform, ShortSideOffset>.New(static state => state.TravelX, static travel => state => state with { TravelX = travel }), ShortSideOffset.Presentation));
    public static readonly AffineTransformParameter TravelY = new("travel-y", new StateParameter<AffineTransform>.Bounded<ShortSideOffset, float, InvalidPixelValue>(Lens<AffineTransform, ShortSideOffset>.New(static state => state.TravelY, static travel => state => state with { TravelY = travel }), ShortSideOffset.Presentation));
    public static readonly AffineTransformParameter Turn = new("turn", new StateParameter<AffineTransform>.Bounded<SignedAngle, float, InvalidPixelValue>(Lens<AffineTransform, SignedAngle>.New(static state => state.Turn, static turn => state => state with { Turn = turn }), SignedAngle.Presentation));
    public static readonly AffineTransformParameter Zoom = new("zoom", new StateParameter<AffineTransform>.Bounded<TransformScale, float, InvalidWarp>(Lens<AffineTransform, TransformScale>.New(static state => state.Zoom, static zoom => state => state with { Zoom = zoom }), TransformScale.Presentation));
    public static readonly AffineTransformParameter Shake = new("shake", new StateParameter<AffineTransform>.Record<Shake>(Lens<AffineTransform, Shake>.New(static state => state.Shake, static shake => state => state with { Shake = shake })));
    public static readonly AffineTransformParameter Shutter = new("shutter", new StateParameter<AffineTransform>.Bounded<ShutterTime, float, InvalidGenerator>(Lens<AffineTransform, ShutterTime>.New(static state => state.Shutter, static shutter => state => state with { Shutter = shutter }), ShutterTime.Presentation));
    public static readonly AffineTransformParameter ShutterPosition = new("shutter-position", new StateParameter<AffineTransform>.Bounded<AxisFraction, float, InvalidGrade>(Lens<AffineTransform, AxisFraction>.New(static state => state.ShutterPosition, static position => state => state with { ShutterPosition = position }), new()));
    public static readonly AffineTransformParameter Sampling = new("sampling", new StateParameter<AffineTransform>.Choice<Sampling, InvalidWarp>(Lens<AffineTransform, Sampling>.New(static state => state.Sampling, static sampling => state => state with { Sampling = sampling })));
    public static readonly AffineTransformParameter Wrap = new("wrap", new StateParameter<AffineTransform>.Choice<WrapMode, InvalidPixelValue>(Lens<AffineTransform, WrapMode>.New(static state => state.Wrap, static wrap => state => state with { Wrap = wrap })));

    public StateParameter<AffineTransform> Kind { get; }
}

public sealed record CornerPin(FramePosition TopLeft, FramePosition TopRight, FramePosition BottomRight, FramePosition BottomLeft, Sampling Sampling, WrapMode Wrap)
    : IStateRecord<CornerPin, CornerPinParameter, InvalidWarp>, IPixelStage<CornerPin> {
    public static CornerPin Default { get; } = new(FramePosition.TopLeft, FramePosition.TopRight, FramePosition.BottomRight, FramePosition.BottomLeft, Sampling.Linear, WrapMode.Black);

    public static Option<PixelPass> Pass(CornerPin state, PassContext context) =>
        state with { Sampling = Default.Sampling, Wrap = Default.Wrap } == Default
        || FrameQuad.Validate(state.TopLeft.Point(context.Extent), state.TopRight.Point(context.Extent), state.BottomRight.Point(context.Extent), state.BottomLeft.Point(context.Extent), out FrameQuad? quad) is not null
            ? None
            : quad!.Homography.Inverse switch {
                var inverse => Some<PixelPass>(new PixelPass.Frame(new CoordinateMap.Analytic((points, coverage, source) => {
                    for (int i = 0; i < points.Length; i++)
                        (points[i], coverage[i]) = inverse.Apply(points[i]) switch { { IsSome: true } uv => ((Vector2)uv * new Vector2(source.Width, source.Height), coverage[i]), _ => (points[i], 0f) };
                }, state.Sampling, state.Wrap).Apply)),
            };
}

[SmartEnum<string>]
[ValidationError<InvalidWarp>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CornerPinParameter : IStateParameter<CornerPin> {
    private static readonly (StateParameter<CornerPin> X, StateParameter<CornerPin> Y) TopLeftAxes = FramePosition.Kinds(Lens<CornerPin, FramePosition>.New(static state => state.TopLeft, static corner => state => state with { TopLeft = corner }));
    private static readonly (StateParameter<CornerPin> X, StateParameter<CornerPin> Y) TopRightAxes = FramePosition.Kinds(Lens<CornerPin, FramePosition>.New(static state => state.TopRight, static corner => state => state with { TopRight = corner }));
    private static readonly (StateParameter<CornerPin> X, StateParameter<CornerPin> Y) BottomRightAxes = FramePosition.Kinds(Lens<CornerPin, FramePosition>.New(static state => state.BottomRight, static corner => state => state with { BottomRight = corner }));
    private static readonly (StateParameter<CornerPin> X, StateParameter<CornerPin> Y) BottomLeftAxes = FramePosition.Kinds(Lens<CornerPin, FramePosition>.New(static state => state.BottomLeft, static corner => state => state with { BottomLeft = corner }));

    public static readonly CornerPinParameter TopLeftX = new("top-left-x", TopLeftAxes.X);
    public static readonly CornerPinParameter TopLeftY = new("top-left-y", TopLeftAxes.Y);
    public static readonly CornerPinParameter TopRightX = new("top-right-x", TopRightAxes.X);
    public static readonly CornerPinParameter TopRightY = new("top-right-y", TopRightAxes.Y);
    public static readonly CornerPinParameter BottomRightX = new("bottom-right-x", BottomRightAxes.X);
    public static readonly CornerPinParameter BottomRightY = new("bottom-right-y", BottomRightAxes.Y);
    public static readonly CornerPinParameter BottomLeftX = new("bottom-left-x", BottomLeftAxes.X);
    public static readonly CornerPinParameter BottomLeftY = new("bottom-left-y", BottomLeftAxes.Y);
    public static readonly CornerPinParameter Sampling = new("sampling", new StateParameter<CornerPin>.Choice<Sampling, InvalidWarp>(Lens<CornerPin, Sampling>.New(static state => state.Sampling, static sampling => state => state with { Sampling = sampling })));
    public static readonly CornerPinParameter Wrap = new("wrap", new StateParameter<CornerPin>.Choice<WrapMode, InvalidPixelValue>(Lens<CornerPin, WrapMode>.New(static state => state.Wrap, static wrap => state => state with { Wrap = wrap })));

    public StateParameter<CornerPin> Kind { get; }
}

public sealed record Shake(ShortSideLength AmplitudeX, ShortSideLength AmplitudeY, SignedAngle Rotation, TransformScale Zoom, Frequency Rate, Octaves Octaves, Seed Seed, Timing Timing, Hold Hold)
    : IStateRecord<Shake, ShakeParameter, InvalidWarp> {
    public static Shake Default { get; } =
        new(ShortSideLength.Neutral, ShortSideLength.Neutral, SignedAngle.Neutral, TransformScale.Identity, Frequency.Create(5f), Octaves.Default, Seed.MinValue, Timing.Default, Hold.MinValue);

    internal bool Still => (AmplitudeX, AmplitudeY, Rotation, Zoom) == (Default.AmplitudeX, Default.AmplitudeY, Default.Rotation, Default.Zoom);

    internal Pose Posed(PixelExtent extent, float time) {
        (uint field, Vector4 at) = (CoordinateHash.Field(NoiseStream.Shake, Seed, 0u), new Vector4(time * Rate, 0f, 0f, 0f));
        float Channel(uint axis) => NoiseDimensions.One.Fbm(at, Octaves, CoordinateHash.Branch(field, axis));
        return new(new(AmplitudeX.Pixels(extent) * Channel(0u), AmplitudeY.Pixels(extent) * Channel(1u)), Rotation * Channel(2u), new(float.Pow(Zoom, Channel(3u))), Vector2.Zero, FramePosition.Center.Point(extent));
    }
}

[SmartEnum<string>]
[ValidationError<InvalidWarp>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ShakeParameter : IStateParameter<Shake> {
    private static readonly Presentation<ShortSideLength, float> Amplitude = ShortSideLength.Presentation with { Soft = (0f, 0.1f) };

    public static readonly ShakeParameter AmplitudeX = new("amplitude-x", new StateParameter<Shake>.Bounded<ShortSideLength, float, InvalidPixelValue>(Lens<Shake, ShortSideLength>.New(static state => state.AmplitudeX, static amplitude => state => state with { AmplitudeX = amplitude }), Amplitude));
    public static readonly ShakeParameter AmplitudeY = new("amplitude-y", new StateParameter<Shake>.Bounded<ShortSideLength, float, InvalidPixelValue>(Lens<Shake, ShortSideLength>.New(static state => state.AmplitudeY, static amplitude => state => state with { AmplitudeY = amplitude }), Amplitude));
    public static readonly ShakeParameter Rotation = new("rotation", new StateParameter<Shake>.Bounded<SignedAngle, float, InvalidPixelValue>(Lens<Shake, SignedAngle>.New(static state => state.Rotation, static rotation => state => state with { Rotation = rotation }), SignedAngle.Presentation with { Soft = (0f, float.Pi) }));
    public static readonly ShakeParameter Zoom = new("zoom", new StateParameter<Shake>.Bounded<TransformScale, float, InvalidWarp>(Lens<Shake, TransformScale>.New(static state => state.Zoom, static zoom => state => state with { Zoom = zoom }), TransformScale.Presentation with { Soft = (1f, 2f) }));
    public static readonly ShakeParameter Rate = new("rate", new StateParameter<Shake>.Bounded<Frequency, float, InvalidGenerator>(Lens<Shake, Frequency>.New(static state => state.Rate, static rate => state => state with { Rate = rate }), Frequency.Presentation with { Soft = (0.1f, 20f), Scale = TrackScale.Log }));
    public static readonly ShakeParameter Octaves = new("octaves", new StateParameter<Shake>.Record<Octaves>(Lens<Shake, Octaves>.New(static state => state.Octaves, static octaves => state => state with { Octaves = octaves })));
    public static readonly ShakeParameter Seed = new("seed", new StateParameter<Shake>.Bounded<Seed, int, InvalidGenerator>(Lens<Shake, Seed>.New(static state => state.Seed, static seed => state => state with { Seed = seed }), Generators.Seed.Presentation));
    public static readonly ShakeParameter Timing = new("timing", new StateParameter<Shake>.Record<Timing>(Lens<Shake, Timing>.New(static state => state.Timing, static timing => state => state with { Timing = timing })));
    public static readonly ShakeParameter Hold = new("hold", new StateParameter<Shake>.Bounded<Hold, float, InvalidGenerator>(Lens<Shake, Hold>.New(static state => state.Hold, static hold => state => state with { Hold = hold }), Generators.Hold.Presentation));

    public StateParameter<Shake> Kind { get; }
}
