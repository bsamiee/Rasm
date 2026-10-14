using System.Numerics;
using Rasm.Imaging.Pixels;

namespace Rasm.Imaging.Filters.Warp;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidWarp>]
public readonly partial struct MirrorCount : IMinMaxValue<MirrorCount> {
    public static MirrorCount MinValue { get; } = new(1);
    public static MirrorCount MaxValue { get; } = new(32);

    static partial void ValidateFactoryArguments(ref InvalidWarp? validationError, ref int value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidWarp();
}

public sealed record Kaleidoscope(MirrorCount Mirrors, SignedAngle Rotation, FramePosition Center, TransformScale Zoom, Sampling Sampling, WrapMode Wrap)
    : IStateRecord<Kaleidoscope, KaleidoscopeParameter, InvalidWarp>, IPixelStage<Kaleidoscope> {
    public static Kaleidoscope Default { get; } = new(MirrorCount.Create(3), SignedAngle.Up, FramePosition.Center, TransformScale.Identity, Sampling.Linear, WrapMode.Mirror);

    public static Option<PixelPass> Pass(Kaleidoscope state, PassContext context) =>
        (state.Center.Point(context.Extent), (float)state.Rotation + (double.Pi / state.Mirrors), double.Tau / state.Mirrors, (double)(float)state.Zoom) switch {
            var (center, axis, period, zoom) => Some<PixelPass>(new PixelPass.Frame(new CoordinateMap.Analytic((points, _, _) => {
                foreach (ref (double X, double Y) point in points) point = (point.X - center.X, point.Y - center.Y) switch {
                    var (x, y) => (double.Hypot(x, y) / zoom, double.SinCos(axis - double.Abs(double.Ieee754Remainder(double.Atan2(-y, x) - axis, period)))) switch {
                        var (radius, (sin, cos)) => (center.X + (radius * cos), center.Y - (radius * sin)),
                    },
                };
            }, state.Sampling, (state.Wrap, state.Wrap)).Apply)),
        };
}

[SmartEnum<string>]
[ValidationError<InvalidWarp>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class KaleidoscopeParameter : IStateParameter<Kaleidoscope> {
    private static readonly (StateParameter<Kaleidoscope> X, StateParameter<Kaleidoscope> Y) Centered =
        FramePosition.Kinds(Lens<Kaleidoscope, FramePosition>.New(static state => state.Center, static center => state => state with { Center = center }));

    public static readonly KaleidoscopeParameter Mirrors = new("mirrors", StateParameter.Bounded<Kaleidoscope, MirrorCount, int, InvalidWarp>(
        Lens<Kaleidoscope, MirrorCount>.New(static state => state.Mirrors, static mirrors => state => state with { Mirrors = mirrors }), new() { Soft = (1, 16) }));
    public static readonly KaleidoscopeParameter Rotation = new("rotation", StateParameter.Bounded<Kaleidoscope, SignedAngle, float, InvalidPixelValue>(
        Lens<Kaleidoscope, SignedAngle>.New(static state => state.Rotation, static rotation => state => state with { Rotation = rotation }), SignedAngle.Presentation));
    public static readonly KaleidoscopeParameter CenterX = new("center-x", Centered.X);
    public static readonly KaleidoscopeParameter CenterY = new("center-y", Centered.Y);
    public static readonly KaleidoscopeParameter Zoom = new("zoom", StateParameter.Bounded<Kaleidoscope, TransformScale, float, InvalidWarp>(
        Lens<Kaleidoscope, TransformScale>.New(static state => state.Zoom, static zoom => state => state with { Zoom = zoom }), TransformScale.Presentation));
    public static readonly KaleidoscopeParameter Sampling = new("sampling", new StateParameter<Kaleidoscope>.Choice<Sampling, InvalidWarp>(
        Lens<Kaleidoscope, Sampling>.New(static state => state.Sampling, static sampling => state => state with { Sampling = sampling })));
    public static readonly KaleidoscopeParameter Wrap = new("wrap", new StateParameter<Kaleidoscope>.Choice<WrapMode, InvalidPixelValue>(
        Lens<Kaleidoscope, WrapMode>.New(static state => state.Wrap, static wrap => state => state with { Wrap = wrap })));

    public StateParameter<Kaleidoscope> Kind { get; }
}
