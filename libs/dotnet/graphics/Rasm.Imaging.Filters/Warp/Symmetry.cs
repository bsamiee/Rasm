using System.Numerics;
using Rasm.Imaging.Pixels;

namespace Rasm.Imaging.Filters.Warp;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct MirrorCount : IMinMaxValue<MirrorCount> {
    public static MirrorCount MinValue { get; } = new(1);
    public static MirrorCount MaxValue { get; } = new(32);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) => validationError = Admission.Refusal(value, MinValue, MaxValue);
}

public sealed record Kaleidoscope(MirrorCount Mirrors, SignedAngle Rotation, FramePosition Center, TransformScale Zoom, Sampling Sampling, WrapMode Wrap)
    : IStateRecord<Kaleidoscope, KaleidoscopeParameter>, IPixelStage<Kaleidoscope> {
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
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class KaleidoscopeParameter : IStateParameter<Kaleidoscope> {
    public static readonly KaleidoscopeParameter Mirrors = new("mirrors", StateParameter.Bounded(Lens<Kaleidoscope, MirrorCount>.New(static state => state.Mirrors, static mirrors => state => state with { Mirrors = mirrors }), new Presentation<int> { Soft = (1, 16) }),
        new("Mirrors", "Mirror lines through the center, drawing twice as many wedges"));
    public static readonly KaleidoscopeParameter Rotation = new("rotation", StateParameter.Bounded(Lens<Kaleidoscope, SignedAngle>.New(static state => state.Rotation, static rotation => state => state with { Rotation = rotation })),
        new("Angle", "Turn of the first mirror line, counterclockwise"));
    public static readonly KaleidoscopeParameter Center = new("center", new StateParameter<Kaleidoscope>.Record<FramePosition>(Lens<Kaleidoscope, FramePosition>.New(static state => state.Center, static center => state => state with { Center = center })),
        new("Center", "Point the warp centers on"));
    public static readonly KaleidoscopeParameter Zoom = new("zoom", StateParameter.Bounded(Lens<Kaleidoscope, TransformScale>.New(static state => state.Zoom, static zoom => state => state with { Zoom = zoom })),
        new("Zoom", "Magnification of the wedges' source about the center"));
    public static readonly KaleidoscopeParameter Sampling = new("sampling", new StateParameter<Kaleidoscope>.Choice<Sampling>(Lens<Kaleidoscope, Sampling>.New(static state => state.Sampling, static sampling => state => state with { Sampling = sampling })),
        new("Interpolation", "Filter reading the source between its pixels"));
    public static readonly KaleidoscopeParameter Wrap = new("wrap", new StateParameter<Kaleidoscope>.Choice<WrapMode>(Lens<Kaleidoscope, WrapMode>.New(static state => state.Wrap, static wrap => state => state with { Wrap = wrap })),
        new("Extension", "Source read past the frame's edge"));

    public StateParameter<Kaleidoscope> Kind { get; }
    public ParameterText Text { get; }
}
