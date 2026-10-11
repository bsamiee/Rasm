using System.Numerics;
using MathNet.Numerics.Integration;
using MathNet.Numerics.LinearAlgebra;
using PeterO.Numbers;
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
    public static Presentation<float> Presentation { get; } = new() { Soft = (1f / 16f, 16f), Scale = TrackScale.Log, Origin = (float)Identity };

    static partial void ValidateFactoryArguments(ref InvalidWarp? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidWarp();
}

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Neutral", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidWarp>]
public readonly partial struct SkewAngle : IMinMaxValue<SkewAngle> {
    public static SkewAngle MaxValue { get; } = new(float.BitDecrement(float.Pi) / 2f);
    public static SkewAngle MinValue { get; } = new(-MaxValue._value);
    public static Presentation<float> Presentation { get; } = new() { Unit = UnitsNet.Quantity.GetUnitInfo(AngleUnit.Radian), Soft = float.Atan(3f) switch { var reach => (-reach, reach) }, Origin = (float)Neutral };

    static partial void ValidateFactoryArguments(ref InvalidWarp? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidWarp();
}

internal readonly record struct Pose((ERational X, ERational Y) Offset, double Rotation, (double X, double Y) Scale, (double X, double Y) Skew, (double X, double Y) Pivot) {
    public Pose Partial(double share) => ERational.FromDouble(share) switch {
        var part => new((Offset.X * part, Offset.Y * part), Rotation * share, (double.Exp(share * double.Log(Scale.X)), double.Exp(share * double.Log(Scale.Y))), (Skew.X * share, Skew.Y * share), Pivot),
    };

    public (Matrix<double> Linear, (ERational X, ERational Y) Shift) Inverse => double.SinCos(Rotation) switch {
        var (sin, cos) => (Matrix<double>.Build.Dense(2, 2, [cos, -sin, sin, cos])
            * Matrix<double>.Build.Dense(2, 2, [1d, 0d, -double.Tan(Skew.Y), 1d])
            * Matrix<double>.Build.Dense(2, 2, [1d, -double.Tan(Skew.X), 0d, 1d])
            * Matrix<double>.Build.Dense(2, 2, [1d / Scale.X, 0d, 0d, 1d / Scale.Y])) switch {
                var linear => (ERational.FromDouble(Pivot.X), ERational.FromDouble(Pivot.Y)) switch {
                    var (x, y) => (linear, (x - ((x + Offset.X) * ERational.FromDouble(linear[0, 0])) - ((y + Offset.Y) * ERational.FromDouble(linear[1, 0])),
                        y - ((x + Offset.X) * ERational.FromDouble(linear[0, 1])) - ((y + Offset.Y) * ERational.FromDouble(linear[1, 1])))),
                },
            },
    };
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
            var path when state.Moving => Some<PixelPass>(new PixelPass.Frame(CoordinateMap.Integrated(new GaussLegendreRule(0d, 1d, 1 << 6), share => (state.Mapped(path(share)), Vector4.One)))),
            var path => path(0d) switch { var still => still.Linear.Equals(Matrix<double>.Build.DenseIdentity(2)) && still.Shift.X.IsZero && still.Shift.Y.IsZero ? None : Some<PixelPass>(new PixelPass.Frame(state.Mapped(still).Apply)) },
        };

    private Func<double, (Matrix<double> Linear, (ERational X, ERational Y) Shift)> Path(PassContext context) {
        (PixelExtent extent, (double X, double Y) pivot) = (context.Extent, Pivot.Point(context.Extent));
        (Matrix<double> Linear, (ERational X, ERational Y) Shift) placed = new Pose((ERational.FromSingle((float)OffsetX).Multiply(extent.ShortSide), ERational.FromSingle((float)OffsetY).Multiply(extent.ShortSide)), (float)Rotation, ((float)ScaleX, (float)ScaleY), ((float)SkewX, (float)SkewY), pivot).Inverse;
        Pose motion = new((ERational.FromSingle((float)TravelX).Multiply(extent.ShortSide), ERational.FromSingle((float)TravelY).Multiply(extent.ShortSide)), (float)Turn, ((float)Zoom, (float)Zoom), (0d, 0d), pivot);
        (double duration, double position) = ((float)Shutter, (float)ShutterPosition);
        double opened = Shake.Hold.Held(Shake.Timing.At(context)) - (duration * position);
        return share => new[] { motion.Partial(share - position).Inverse, Shake.Posed(extent, opened + (duration * share)).Inverse, placed }
            .Aggregate(static (first, second) => (first.Linear * second.Linear,
                ((first.Shift.X * ERational.FromDouble(second.Linear[0, 0])) + (first.Shift.Y * ERational.FromDouble(second.Linear[1, 0])) + second.Shift.X,
                 (first.Shift.X * ERational.FromDouble(second.Linear[0, 1])) + (first.Shift.Y * ERational.FromDouble(second.Linear[1, 1])) + second.Shift.Y)));
    }

    private CoordinateMap.Analytic Mapped((Matrix<double> Linear, (ERational X, ERational Y) Shift) inverse) {
        (ERational xx, ERational xy, ERational yx, ERational yy) = (ERational.FromDouble(inverse.Linear[0, 0]), ERational.FromDouble(inverse.Linear[0, 1]), ERational.FromDouble(inverse.Linear[1, 0]), ERational.FromDouble(inverse.Linear[1, 1]));
        return new((points, _, extent) => {
            foreach (ref (double X, double Y) point in points) {
                (ERational x, ERational y) = (ERational.FromDouble(point.X), ERational.FromDouble(point.Y));
                (ERational across, ERational down) = ((x * xx) + (y * yx) + inverse.Shift.X, (x * xy) + (y * yy) + inverse.Shift.Y);
                point = (Sampling.Point(across, Wrap, extent.Width), Sampling.Point(down, Wrap, extent.Height));
            }
        }, Sampling, (Wrap, Wrap));
    }
}

[SmartEnum<string>]
[ValidationError<InvalidWarp>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class AffineTransformParameter : IStateParameter<AffineTransform> {
    private static readonly (StateParameter<AffineTransform> X, StateParameter<AffineTransform> Y) Pivoted = FramePosition.Kinds(Lens<AffineTransform, FramePosition>.New(static state => state.Pivot, static pivot => state => state with { Pivot = pivot }));

    public static readonly AffineTransformParameter OffsetX = new("offset-x", StateParameter.Bounded<AffineTransform, ShortSideOffset, float, InvalidPixelValue>(Lens<AffineTransform, ShortSideOffset>.New(static state => state.OffsetX, static offset => state => state with { OffsetX = offset }), ShortSideOffset.Presentation));
    public static readonly AffineTransformParameter OffsetY = new("offset-y", StateParameter.Bounded<AffineTransform, ShortSideOffset, float, InvalidPixelValue>(Lens<AffineTransform, ShortSideOffset>.New(static state => state.OffsetY, static offset => state => state with { OffsetY = offset }), ShortSideOffset.Presentation));
    public static readonly AffineTransformParameter Rotation = new("rotation", StateParameter.Bounded<AffineTransform, SignedAngle, float, InvalidPixelValue>(Lens<AffineTransform, SignedAngle>.New(static state => state.Rotation, static rotation => state => state with { Rotation = rotation }), SignedAngle.Presentation));
    public static readonly AffineTransformParameter ScaleX = new("scale-x", StateParameter.Bounded<AffineTransform, TransformScale, float, InvalidWarp>(Lens<AffineTransform, TransformScale>.New(static state => state.ScaleX, static scale => state => state with { ScaleX = scale }), TransformScale.Presentation));
    public static readonly AffineTransformParameter ScaleY = new("scale-y", StateParameter.Bounded<AffineTransform, TransformScale, float, InvalidWarp>(Lens<AffineTransform, TransformScale>.New(static state => state.ScaleY, static scale => state => state with { ScaleY = scale }), TransformScale.Presentation));
    public static readonly AffineTransformParameter SkewX = new("skew-x", StateParameter.Bounded<AffineTransform, SkewAngle, float, InvalidWarp>(Lens<AffineTransform, SkewAngle>.New(static state => state.SkewX, static skew => state => state with { SkewX = skew }), SkewAngle.Presentation));
    public static readonly AffineTransformParameter SkewY = new("skew-y", StateParameter.Bounded<AffineTransform, SkewAngle, float, InvalidWarp>(Lens<AffineTransform, SkewAngle>.New(static state => state.SkewY, static skew => state => state with { SkewY = skew }), SkewAngle.Presentation));
    public static readonly AffineTransformParameter PivotX = new("pivot-x", Pivoted.X);
    public static readonly AffineTransformParameter PivotY = new("pivot-y", Pivoted.Y);
    public static readonly AffineTransformParameter TravelX = new("travel-x", StateParameter.Bounded<AffineTransform, ShortSideOffset, float, InvalidPixelValue>(Lens<AffineTransform, ShortSideOffset>.New(static state => state.TravelX, static travel => state => state with { TravelX = travel }), ShortSideOffset.Presentation));
    public static readonly AffineTransformParameter TravelY = new("travel-y", StateParameter.Bounded<AffineTransform, ShortSideOffset, float, InvalidPixelValue>(Lens<AffineTransform, ShortSideOffset>.New(static state => state.TravelY, static travel => state => state with { TravelY = travel }), ShortSideOffset.Presentation));
    public static readonly AffineTransformParameter Turn = new("turn", StateParameter.Bounded<AffineTransform, SignedAngle, float, InvalidPixelValue>(Lens<AffineTransform, SignedAngle>.New(static state => state.Turn, static turn => state => state with { Turn = turn }), SignedAngle.Presentation));
    public static readonly AffineTransformParameter Zoom = new("zoom", StateParameter.Bounded<AffineTransform, TransformScale, float, InvalidWarp>(Lens<AffineTransform, TransformScale>.New(static state => state.Zoom, static zoom => state => state with { Zoom = zoom }), TransformScale.Presentation));
    public static readonly AffineTransformParameter Shake = new("shake", new StateParameter<AffineTransform>.Record<Shake>(Lens<AffineTransform, Shake>.New(static state => state.Shake, static shake => state => state with { Shake = shake })));
    public static readonly AffineTransformParameter Shutter = new("shutter", StateParameter.Bounded<AffineTransform, ShutterTime, float, InvalidGenerator>(Lens<AffineTransform, ShutterTime>.New(static state => state.Shutter, static shutter => state => state with { Shutter = shutter }), ShutterTime.Presentation));
    public static readonly AffineTransformParameter ShutterPosition = new("shutter-position", StateParameter.Bounded<AffineTransform, AxisFraction, float, InvalidGrade>(Lens<AffineTransform, AxisFraction>.New(static state => state.ShutterPosition, static position => state => state with { ShutterPosition = position }), new()));
    public static readonly AffineTransformParameter Sampling = new("sampling", new StateParameter<AffineTransform>.Choice<Sampling, InvalidWarp>(Lens<AffineTransform, Sampling>.New(static state => state.Sampling, static sampling => state => state with { Sampling = sampling })));
    public static readonly AffineTransformParameter Wrap = new("wrap", new StateParameter<AffineTransform>.Choice<WrapMode, InvalidPixelValue>(Lens<AffineTransform, WrapMode>.New(static state => state.Wrap, static wrap => state => state with { Wrap = wrap })));

    public StateParameter<AffineTransform> Kind { get; }
}

public sealed record CornerPin(FramePosition TopLeft, FramePosition TopRight, FramePosition BottomRight, FramePosition BottomLeft, Sampling Sampling, WrapMode Wrap)
    : IStateRecord<CornerPin, CornerPinParameter, InvalidWarp>, IPixelStage<CornerPin> {
    public static CornerPin Default { get; } = new(FramePosition.TopLeft, FramePosition.TopRight, FramePosition.BottomRight, FramePosition.BottomLeft, Sampling.Linear, WrapMode.Black);

    public static Option<PixelPass> Pass(CornerPin state, PassContext context) =>
        state with { Sampling = Default.Sampling, Wrap = Default.Wrap } == Default
            ? None
            : FrameQuad.Validate(state.TopLeft.Point(context.Extent), state.TopRight.Point(context.Extent), state.BottomRight.Point(context.Extent), state.BottomLeft.Point(context.Extent), out FrameQuad? quad) switch {
                { } error => Some<PixelPass>(new PixelPass.Frame((_, _) => error)),
                null => quad!.Homography.Inverse switch {
                    var inverse => Some<PixelPass>(new PixelPass.Frame(new CoordinateMap.Analytic((points, coverage, source) => {
                        for (int i = 0; i < points.Length; i++) {
                            float weight = coverage[i];
                            (points[i], coverage[i]) = inverse.Apply(points[i]).Map(uv => ((uv.X * source.Width, uv.Y * source.Height), weight)).IfNone((points[i], 0f));
                        }
                    }, state.Sampling, (state.Wrap, state.Wrap)).Apply)),
                },
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

    internal Pose Posed(PixelExtent extent, double time) {
        (uint field, Vector4 at) = (CoordinateHash.Field(NoiseStream.Shake, Seed, 0u), new Vector4((float)(time * (float)Rate), 0f, 0f, 0f));
        float Channel(uint axis) => NoiseDimensions.One.Fbm(at, Octaves, CoordinateHash.Branch(field, axis));
        double zoom = double.Pow((float)Zoom, Channel(3u));
        return new((ERational.FromSingle((float)AmplitudeX).Multiply(extent.ShortSide) * ERational.FromSingle(Channel(0u)), ERational.FromSingle((float)AmplitudeY).Multiply(extent.ShortSide) * ERational.FromSingle(Channel(1u))), (double)(float)Rotation * Channel(2u), (zoom, zoom), (0d, 0d), FramePosition.Center.Point(extent));
    }
}

[SmartEnum<string>]
[ValidationError<InvalidWarp>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ShakeParameter : IStateParameter<Shake> {
    private static readonly Presentation<float> Amplitude = ShortSideLength.Presentation with { Soft = (0f, 0.1f) };

    public static readonly ShakeParameter AmplitudeX = new("amplitude-x", StateParameter.Bounded<Shake, ShortSideLength, float, InvalidPixelValue>(Lens<Shake, ShortSideLength>.New(static state => state.AmplitudeX, static amplitude => state => state with { AmplitudeX = amplitude }), Amplitude));
    public static readonly ShakeParameter AmplitudeY = new("amplitude-y", StateParameter.Bounded<Shake, ShortSideLength, float, InvalidPixelValue>(Lens<Shake, ShortSideLength>.New(static state => state.AmplitudeY, static amplitude => state => state with { AmplitudeY = amplitude }), Amplitude));
    public static readonly ShakeParameter Rotation = new("rotation", StateParameter.Bounded<Shake, SignedAngle, float, InvalidPixelValue>(Lens<Shake, SignedAngle>.New(static state => state.Rotation, static rotation => state => state with { Rotation = rotation }), SignedAngle.Presentation with { Soft = (0f, float.Pi) }));
    public static readonly ShakeParameter Zoom = new("zoom", StateParameter.Bounded<Shake, TransformScale, float, InvalidWarp>(Lens<Shake, TransformScale>.New(static state => state.Zoom, static zoom => state => state with { Zoom = zoom }), TransformScale.Presentation with { Soft = (1f, 2f) }));
    public static readonly ShakeParameter Rate = new("rate", StateParameter.Bounded<Shake, Frequency, float, InvalidGenerator>(Lens<Shake, Frequency>.New(static state => state.Rate, static rate => state => state with { Rate = rate }), Frequency.Presentation with { Soft = (0.1f, 20f), Scale = TrackScale.Linear }));
    public static readonly ShakeParameter Octaves = new("octaves", new StateParameter<Shake>.Record<Octaves>(Lens<Shake, Octaves>.New(static state => state.Octaves, static octaves => state => state with { Octaves = octaves })));
    public static readonly ShakeParameter Seed = new("seed", StateParameter.Bounded<Shake, Seed, int, InvalidGenerator>(Lens<Shake, Seed>.New(static state => state.Seed, static seed => state => state with { Seed = seed }), Generators.Seed.Presentation));
    public static readonly ShakeParameter Timing = new("timing", new StateParameter<Shake>.Record<Timing>(Lens<Shake, Timing>.New(static state => state.Timing, static timing => state => state with { Timing = timing })));
    public static readonly ShakeParameter Hold = new("hold", StateParameter.Bounded<Shake, Hold, float, InvalidGenerator>(Lens<Shake, Hold>.New(static state => state.Hold, static hold => state => state with { Hold = hold }), Generators.Hold.Presentation));

    public StateParameter<Shake> Kind { get; }
}

public sealed record ShakeWarp(Shake Motion, ShutterTime Shutter, AxisFraction ShutterPosition, Sampling Sampling, WrapMode Wrap)
    : IStateRecord<ShakeWarp, ShakeWarpParameter, InvalidWarp>, IPixelStage<ShakeWarp> {
    public static ShakeWarp Default { get; } = new(Shake.Default, ShutterTime.Instant, AxisFraction.Half, Sampling.Linear, WrapMode.Periodic);

    public static Option<PixelPass> Pass(ShakeWarp state, PassContext context) =>
        AffineTransform.Pass(AffineTransform.Default with {
            Shake = state.Motion,
            Shutter = state.Shutter,
            ShutterPosition = state.ShutterPosition,
            Sampling = state.Sampling,
            Wrap = state.Wrap,
        }, context);
}

[SmartEnum<string>]
[ValidationError<InvalidWarp>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ShakeWarpParameter : IStateParameter<ShakeWarp> {
    public static readonly ShakeWarpParameter Motion = new("motion", new StateParameter<ShakeWarp>.Record<Shake>(
        Lens<ShakeWarp, Shake>.New(static state => state.Motion, static motion => state => state with { Motion = motion })));
    public static readonly ShakeWarpParameter Shutter = new("shutter", StateParameter.Bounded<ShakeWarp, ShutterTime, float, InvalidGenerator>(
        Lens<ShakeWarp, ShutterTime>.New(static state => state.Shutter, static shutter => state => state with { Shutter = shutter }), ShutterTime.Presentation));
    public static readonly ShakeWarpParameter ShutterPosition = new("shutter-position", StateParameter.Bounded<ShakeWarp, AxisFraction, float, InvalidGrade>(
        Lens<ShakeWarp, AxisFraction>.New(static state => state.ShutterPosition, static position => state => state with { ShutterPosition = position }), new()));
    public static readonly ShakeWarpParameter Sampling = new("sampling", new StateParameter<ShakeWarp>.Choice<Sampling, InvalidWarp>(
        Lens<ShakeWarp, Sampling>.New(static state => state.Sampling, static sampling => state => state with { Sampling = sampling })));
    public static readonly ShakeWarpParameter Wrap = new("wrap", new StateParameter<ShakeWarp>.Choice<WrapMode, InvalidPixelValue>(
        Lens<ShakeWarp, WrapMode>.New(static state => state.Wrap, static wrap => state => state with { Wrap = wrap })));

    public StateParameter<ShakeWarp> Kind { get; }
}
