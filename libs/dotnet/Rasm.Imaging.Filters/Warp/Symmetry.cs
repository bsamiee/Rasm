using System.Numerics;
using Rasm.Imaging.Pixels;

namespace Rasm.Imaging.Filters.Warp;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidWarp>]
public readonly partial struct MirrorCount : IMinMaxValue<MirrorCount> {
    public static MirrorCount MinValue { get; } = new(1);
    public static MirrorCount MaxValue { get; } = new(32);
    public static MirrorCount Square { get; } = new(4);

    static partial void ValidateFactoryArguments(ref InvalidWarp? validationError, ref int value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidWarp();
}

public sealed record Kaleidoscope(MirrorCount Mirrors, SignedAngle Rotation, FramePosition Center, TransformScale Zoom, ShortSideLength RadialOffset, Sampling Sampling, WrapMode Wrap)
    : IStateRecord<Kaleidoscope, KaleidoscopeParameter, InvalidWarp>, IPixelStage<Kaleidoscope> {
    public static Kaleidoscope Default { get; } = new(MirrorCount.Square, SignedAngle.Neutral, FramePosition.Center, TransformScale.Identity, ShortSideLength.Neutral, Sampling.Linear, WrapMode.Mirror);

    public static Option<PixelPass> Pass(Kaleidoscope state, PassContext context) =>
        Some<PixelPass>(new PixelPass.Frame(new CoordinateMap.Analytic(state.Kernel(context.Extent), state.Sampling, state.Wrap).Apply));

    private Action<Span<Vector2>, Span<float>, PixelExtent> Kernel(PixelExtent extent) {
        (Vector2 center, float rotation, float zoom, float offset, float wedge) = (Center.Point(extent), Rotation, Zoom, RadialOffset.Pixels(extent), float.Pi / Mirrors);
        return (points, _, _) => {
            foreach (ref Vector2 point in points) {
                Vector2 d = point - center;
                float t = float.Atan2(-d.Y, d.X) - rotation;
                float folded = rotation + wedge - float.Abs(t - (2f * wedge * float.Floor(t / (2f * wedge))) - wedge);
                point = center + (((d.Length() / zoom) + offset) * Vector2.Transform(Vector2.UnitX, Matrix3x2.CreateRotation(-folded)));
            }
        };
    }
}

[SmartEnum<string>]
[ValidationError<InvalidWarp>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class KaleidoscopeParameter : IStateParameter<Kaleidoscope> {
    private static readonly (StateParameter<Kaleidoscope> X, StateParameter<Kaleidoscope> Y) Centered =
        FramePosition.Kinds(Lens<Kaleidoscope, FramePosition>.New(static state => state.Center, static center => state => state with { Center = center }));

    public static readonly KaleidoscopeParameter Mirrors = new("mirrors", new StateParameter<Kaleidoscope>.Bounded<MirrorCount, int, InvalidWarp>(
        Lens<Kaleidoscope, MirrorCount>.New(static state => state.Mirrors, static mirrors => state => state with { Mirrors = mirrors }), new() { Soft = (1, 16) }));
    public static readonly KaleidoscopeParameter Rotation = new("rotation", new StateParameter<Kaleidoscope>.Bounded<SignedAngle, float, InvalidPixelValue>(
        Lens<Kaleidoscope, SignedAngle>.New(static state => state.Rotation, static rotation => state => state with { Rotation = rotation }), SignedAngle.Presentation));
    public static readonly KaleidoscopeParameter CenterX = new("center-x", Centered.X);
    public static readonly KaleidoscopeParameter CenterY = new("center-y", Centered.Y);
    public static readonly KaleidoscopeParameter Zoom = new("zoom", new StateParameter<Kaleidoscope>.Bounded<TransformScale, float, InvalidWarp>(
        Lens<Kaleidoscope, TransformScale>.New(static state => state.Zoom, static zoom => state => state with { Zoom = zoom }), TransformScale.Presentation));
    public static readonly KaleidoscopeParameter RadialOffset = new("radial-offset", new StateParameter<Kaleidoscope>.Bounded<ShortSideLength, float, InvalidPixelValue>(
        Lens<Kaleidoscope, ShortSideLength>.New(static state => state.RadialOffset, static offset => state => state with { RadialOffset = offset }), ShortSideLength.Presentation));
    public static readonly KaleidoscopeParameter Sampling = new("sampling", new StateParameter<Kaleidoscope>.Choice<Sampling, InvalidWarp>(
        Lens<Kaleidoscope, Sampling>.New(static state => state.Sampling, static sampling => state => state with { Sampling = sampling })));
    public static readonly KaleidoscopeParameter Wrap = new("wrap", new StateParameter<Kaleidoscope>.Choice<WrapMode, InvalidPixelValue>(
        Lens<Kaleidoscope, WrapMode>.New(static state => state.Wrap, static wrap => state => state with { Wrap = wrap })));

    public StateParameter<Kaleidoscope> Kind { get; }
}
