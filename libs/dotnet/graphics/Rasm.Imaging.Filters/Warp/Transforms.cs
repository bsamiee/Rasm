using System.Numerics;
using MathNet.Numerics.Integration;
using MathNet.Numerics.LinearAlgebra;
using PeterO.Numbers;
using Rasm.Imaging.Filters.Generators;
using Rasm.Imaging.Pixels;
using UnitsNet.Units;

namespace Rasm.Imaging.Filters.Warp;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct TransformScale : IPresented<TransformScale, float> {
    public static TransformScale MaxValue { get; } = new(1 << 15);
    public static TransformScale MinValue { get; } = new(1f / MaxValue._value);
    public static TransformScale Identity { get; } = new(1f);
    public static Presentation<float> Presentation { get; } = new() { Soft = (1f / 16f, 16f), Scale = TrackScale.Log, Origin = (float)Identity };

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Neutral", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct SkewAngle : IPresented<SkewAngle, float> {
    public static SkewAngle MaxValue { get; } = new(float.BitDecrement(float.Pi) / 2f);
    public static SkewAngle MinValue { get; } = new(-MaxValue._value);
    public static Presentation<float> Presentation { get; } = new() { Unit = UnitsNet.Quantity.GetUnitInfo(AngleUnit.Radian), Soft = float.Atan(3f) switch { var reach => (-reach, reach) }, Origin = (float)Neutral };

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
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
    : IStateRecord<AffineTransform, AffineTransformParameter>, IPixelStage<AffineTransform> {
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
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class AffineTransformParameter : IStateParameter<AffineTransform> {
    public static readonly AffineTransformParameter OffsetX = new("offset-x", StateParameter.Bounded(Lens<AffineTransform, ShortSideOffset>.New(static state => state.OffsetX, static offset => state => state with { OffsetX = offset })),
        new("X", "Horizontal move in frame short sides"));
    public static readonly AffineTransformParameter OffsetY = new("offset-y", StateParameter.Bounded(Lens<AffineTransform, ShortSideOffset>.New(static state => state.OffsetY, static offset => state => state with { OffsetY = offset })),
        new("Y", "Downward move in frame short sides"));
    public static readonly AffineTransformParameter Rotation = new("rotation", StateParameter.Bounded(Lens<AffineTransform, SignedAngle>.New(static state => state.Rotation, static rotation => state => state with { Rotation = rotation })),
        new("Angle", "Counterclockwise turn about pivot"));
    public static readonly AffineTransformParameter ScaleX = new("scale-x", StateParameter.Bounded(Lens<AffineTransform, TransformScale>.New(static state => state.ScaleX, static scale => state => state with { ScaleX = scale })),
        new("Scale X", "Horizontal scale about pivot"));
    public static readonly AffineTransformParameter ScaleY = new("scale-y", StateParameter.Bounded(Lens<AffineTransform, TransformScale>.New(static state => state.ScaleY, static scale => state => state with { ScaleY = scale })),
        new("Scale Y", "Vertical scale about pivot"));
    public static readonly AffineTransformParameter SkewX = new("skew-x", StateParameter.Bounded(Lens<AffineTransform, SkewAngle>.New(static state => state.SkewX, static skew => state => state with { SkewX = skew })),
        new("Skew X", "Horizontal shear about pivot"));
    public static readonly AffineTransformParameter SkewY = new("skew-y", StateParameter.Bounded(Lens<AffineTransform, SkewAngle>.New(static state => state.SkewY, static skew => state => state with { SkewY = skew })),
        new("Skew Y", "Vertical shear about pivot"));
    public static readonly AffineTransformParameter Pivot = new("pivot", new StateParameter<AffineTransform>.Record<FramePosition>(Lens<AffineTransform, FramePosition>.New(static state => state.Pivot, static pivot => state => state with { Pivot = pivot })),
        new("Pivot", "Point the frame turns, scales, and shears about"));
    public static readonly AffineTransformParameter TravelX = new("travel-x", StateParameter.Bounded(Lens<AffineTransform, ShortSideOffset>.New(static state => state.TravelX, static travel => state => state with { TravelX = travel })),
        new("Travel X", "Horizontal move across exposure, blurring along it"));
    public static readonly AffineTransformParameter TravelY = new("travel-y", StateParameter.Bounded(Lens<AffineTransform, ShortSideOffset>.New(static state => state.TravelY, static travel => state => state with { TravelY = travel })),
        new("Travel Y", "Downward move across exposure, blurring along it"));
    public static readonly AffineTransformParameter Turn = new("turn", StateParameter.Bounded(Lens<AffineTransform, SignedAngle>.New(static state => state.Turn, static turn => state => state with { Turn = turn })),
        new("Spin", "Counterclockwise turn about pivot across exposure"));
    public static readonly AffineTransformParameter Zoom = new("zoom", StateParameter.Bounded(Lens<AffineTransform, TransformScale>.New(static state => state.Zoom, static zoom => state => state with { Zoom = zoom })),
        new("Zoom", "Scale about pivot at exposure end"));
    public static readonly AffineTransformParameter Shake = new("shake", new StateParameter<AffineTransform>.Record<Shake>(Lens<AffineTransform, Shake>.New(static state => state.Shake, static shake => state => state with { Shake = shake })),
        new("Shake", "Seeded camera motion over time"));
    public static readonly AffineTransformParameter Shutter = new("shutter", StateParameter.Bounded(Lens<AffineTransform, ShutterTime>.New(static state => state.Shutter, static shutter => state => state with { Shutter = shutter })),
        new("Shutter", "Seconds the exposure samples along its motion"));
    public static readonly AffineTransformParameter ShutterPosition = new("shutter-position", StateParameter.Bounded(Lens<AffineTransform, AxisFraction>.New(static state => state.ShutterPosition, static position => state => state with { ShutterPosition = position })),
        new("Shutter position", "Exposure share before current time, 0% opening and 100% closing now"));
    public static readonly AffineTransformParameter Sampling = new("sampling", new StateParameter<AffineTransform>.Choice<Sampling>(Lens<AffineTransform, Sampling>.New(static state => state.Sampling, static sampling => state => state with { Sampling = sampling })),
        new("Interpolation", "Filter between source pixels"));
    public static readonly AffineTransformParameter Wrap = new("wrap", new StateParameter<AffineTransform>.Choice<WrapMode>(Lens<AffineTransform, WrapMode>.New(static state => state.Wrap, static wrap => state => state with { Wrap = wrap })),
        new("Extension", "Sampling past frame edges"));

    public StateParameter<AffineTransform> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record CornerPin(FramePosition TopLeft, FramePosition TopRight, FramePosition BottomRight, FramePosition BottomLeft, Sampling Sampling, WrapMode Wrap)
    : IStateRecord<CornerPin, CornerPinParameter>, IPixelStage<CornerPin> {
    public static CornerPin Default { get; } = new(FramePosition.TopLeft, FramePosition.TopRight, FramePosition.BottomRight, FramePosition.BottomLeft, Sampling.Linear, WrapMode.Black);

    public static Option<PixelPass> Pass(CornerPin state, PassContext context) =>
        state with { Sampling = Default.Sampling, Wrap = Default.Wrap } == Default
            ? None
            : ValidationResults.ToFin(FrameQuad.Validate(state.TopLeft.Point(context.Extent), state.TopRight.Point(context.Extent), state.BottomRight.Point(context.Extent), state.BottomLeft.Point(context.Extent), out FrameQuad? quad), quad)
                .Map(static admitted => admitted.Homography.Inverse)
                .Match(
                    Succ: inverse => Some<PixelPass>(new PixelPass.Frame(new CoordinateMap.Analytic((points, coverage, source) => {
                        for (int i = 0; i < points.Length; i++) {
                            float weight = coverage[i];
                            (points[i], coverage[i]) = inverse.Apply(points[i]).Map(uv => ((uv.X * source.Width, uv.Y * source.Height), weight)).IfNone((points[i], 0f));
                        }
                    }, state.Sampling, (state.Wrap, state.Wrap)).Apply)),
                    Fail: static error => Some<PixelPass>(new PixelPass.Frame((_, _) => error)));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CornerPinParameter : IStateParameter<CornerPin> {
    public static readonly CornerPinParameter TopLeft = new("top-left", new StateParameter<CornerPin>.Record<FramePosition>(Lens<CornerPin, FramePosition>.New(static state => state.TopLeft, static corner => state => state with { TopLeft = corner })),
        new("Upper left", "Upper left corner position in frame"));
    public static readonly CornerPinParameter TopRight = new("top-right", new StateParameter<CornerPin>.Record<FramePosition>(Lens<CornerPin, FramePosition>.New(static state => state.TopRight, static corner => state => state with { TopRight = corner })),
        new("Upper right", "Upper right corner position in frame"));
    public static readonly CornerPinParameter BottomRight = new("bottom-right", new StateParameter<CornerPin>.Record<FramePosition>(Lens<CornerPin, FramePosition>.New(static state => state.BottomRight, static corner => state => state with { BottomRight = corner })),
        new("Lower right", "Lower right corner position in frame"));
    public static readonly CornerPinParameter BottomLeft = new("bottom-left", new StateParameter<CornerPin>.Record<FramePosition>(Lens<CornerPin, FramePosition>.New(static state => state.BottomLeft, static corner => state => state with { BottomLeft = corner })),
        new("Lower left", "Lower left corner position in frame"));
    public static readonly CornerPinParameter Sampling = new("sampling", new StateParameter<CornerPin>.Choice<Sampling>(Lens<CornerPin, Sampling>.New(static state => state.Sampling, static sampling => state => state with { Sampling = sampling })),
        new("Interpolation", "Filter between source pixels"));
    public static readonly CornerPinParameter Wrap = new("wrap", new StateParameter<CornerPin>.Choice<WrapMode>(Lens<CornerPin, WrapMode>.New(static state => state.Wrap, static wrap => state => state with { Wrap = wrap })),
        new("Extension", "Sampling past frame edges"));

    public StateParameter<CornerPin> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record Shake(ShortSideLength AmplitudeX, ShortSideLength AmplitudeY, SignedAngle Rotation, TransformScale Zoom, Frequency Rate, Octaves Octaves, Seed Seed, Timing Timing, Hold Hold)
    : IStateRecord<Shake, ShakeParameter> {
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
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ShakeParameter : IStateParameter<Shake> {
    private static readonly Presentation<float> Amplitude = ShortSideLength.Presentation with { Soft = (0f, 0.1f) };

    public static readonly ShakeParameter AmplitudeX = new("amplitude-x", StateParameter.Bounded(Lens<Shake, ShortSideLength>.New(static state => state.AmplitudeX, static amplitude => state => state with { AmplitudeX = amplitude }), Amplitude),
        new("Amplitude X", "Largest horizontal shake in frame short sides"));
    public static readonly ShakeParameter AmplitudeY = new("amplitude-y", StateParameter.Bounded(Lens<Shake, ShortSideLength>.New(static state => state.AmplitudeY, static amplitude => state => state with { AmplitudeY = amplitude }), Amplitude),
        new("Amplitude Y", "Largest vertical shake in frame short sides"));
    public static readonly ShakeParameter Rotation = new("rotation", StateParameter.Bounded(Lens<Shake, SignedAngle>.New(static state => state.Rotation, static rotation => state => state with { Rotation = rotation }), SignedAngle.Presentation with { Soft = (0f, float.Pi) }),
        new("Rotation", "Largest shake turn"));
    public static readonly ShakeParameter Zoom = new("zoom", StateParameter.Bounded(Lens<Shake, TransformScale>.New(static state => state.Zoom, static zoom => state => state with { Zoom = zoom }), TransformScale.Presentation with { Soft = (1f, 2f) }),
        new("Zoom", "Largest shake zoom factor, in and out alike"));
    public static readonly ShakeParameter Rate = new("rate", StateParameter.Bounded(Lens<Shake, Frequency>.New(static state => state.Rate, static rate => state => state with { Rate = rate }), Frequency.Presentation with { Soft = (0.1f, 20f), Scale = TrackScale.Linear }),
        new("Frequency", "Shakes per second"));
    public static readonly ShakeParameter Octaves = new("octaves", new StateParameter<Shake>.Record<Octaves>(Lens<Shake, Octaves>.New(static state => state.Octaves, static octaves => state => state with { Octaves = octaves })),
        new("Octaves", "Shake detail across scales"));
    public static readonly ShakeParameter Seed = new("seed", StateParameter.Bounded(Lens<Shake, Seed>.New(static state => state.Seed, static seed => state => state with { Seed = seed })),
        new("Seed", "Number selecting pattern"));
    public static readonly ShakeParameter Timing = new("timing", new StateParameter<Shake>.Record<Timing>(Lens<Shake, Timing>.New(static state => state.Timing, static timing => state => state with { Timing = timing })),
        new("Timing", "Stored time and rate of recorded frame time"));
    public static readonly ShakeParameter Hold = new("hold", StateParameter.Bounded(Lens<Shake, Hold>.New(static state => state.Hold, static hold => state => state with { Hold = hold })),
        new("Hold", "Seconds between pattern redraws"));

    public StateParameter<Shake> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record ShakeWarp(Shake Motion, ShutterTime Shutter, AxisFraction ShutterPosition, Sampling Sampling, WrapMode Wrap)
    : IStateRecord<ShakeWarp, ShakeWarpParameter>, IPixelStage<ShakeWarp> {
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
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ShakeWarpParameter : IStateParameter<ShakeWarp> {
    public static readonly ShakeWarpParameter Motion = new("motion", new StateParameter<ShakeWarp>.Record<Shake>(Lens<ShakeWarp, Shake>.New(static state => state.Motion, static motion => state => state with { Motion = motion })),
        new("Shake", "Seeded camera motion over time"));
    public static readonly ShakeWarpParameter Shutter = new("shutter", StateParameter.Bounded(Lens<ShakeWarp, ShutterTime>.New(static state => state.Shutter, static shutter => state => state with { Shutter = shutter })),
        new("Shutter", "Seconds the exposure samples along its motion"));
    public static readonly ShakeWarpParameter ShutterPosition = new("shutter-position", StateParameter.Bounded(Lens<ShakeWarp, AxisFraction>.New(static state => state.ShutterPosition, static position => state => state with { ShutterPosition = position })),
        new("Shutter position", "Exposure share before current time, 0% opening and 100% closing now"));
    public static readonly ShakeWarpParameter Sampling = new("sampling", new StateParameter<ShakeWarp>.Choice<Sampling>(Lens<ShakeWarp, Sampling>.New(static state => state.Sampling, static sampling => state => state with { Sampling = sampling })),
        new("Interpolation", "Filter between source pixels"));
    public static readonly ShakeWarpParameter Wrap = new("wrap", new StateParameter<ShakeWarp>.Choice<WrapMode>(Lens<ShakeWarp, WrapMode>.New(static state => state.Wrap, static wrap => state => state with { Wrap = wrap })),
        new("Extension", "Sampling past frame edges"));

    public StateParameter<ShakeWarp> Kind { get; }
    public ParameterText Text { get; }
}
