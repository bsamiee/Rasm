using System.Numerics;
using CommunityToolkit.HighPerformance.Buffers;
using Darkroom.Chain;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Procedural;
using UnitsNet;

namespace Darkroom.Catalog;

// --- [CONSTANTS] -----------------------------------------------------------------------

internal static class ReferenceFrame {
    public const float Height = 1080f;
    public const float GreaterSide = 1920f / Height;
}

// --- [MODELS] --------------------------------------------------------------------------

[ValueObject<Duration>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<DarkroomFault>]
internal sealed partial class Hold {
    private static readonly Duration Shortest = Duration.FromMilliseconds(1);
    private static readonly Duration Longest = Duration.FromSeconds(8);

    public static Hold MinValue { get; } = new(Shortest);

    public Int128 Period(Duration clock) =>
        Int128.DivRem(clock.ToInt128Nanoseconds(), _value.ToInt128Nanoseconds()) switch {
            var (period, remainder) => remainder < 0 ? period - 1 : period,
        };

    public Duration Held(Duration clock) => Duration.FromNanoseconds(_value.ToInt128Nanoseconds() * Period(clock));

    public static Fin<Hold> Validated(Duration value) => TryCreate(value, out Hold? hold, out DarkroomFault? fault) ? hold : fault;

    static partial void ValidateFactoryArguments(ref DarkroomFault? validationError, ref Duration value) =>
        validationError = value >= Shortest && value <= Longest ? null
            : new DarkroomFault(value, DarkroomReason.Invalid, Expression: "hold");
}

internal sealed record Timing(Duration Clock, float Pace) {
    private const long Wrap = (1 << 14) * NodaConstants.NanosecondsPerSecond;

    public static Timing Default { get; } = new(Duration.Zero, 1f);

    public Duration At(PassContext context) =>
        Duration.FromNanoseconds((Clock + context.Time.Map(time => time * Pace).IfNone(Duration.Zero)).ToInt128Nanoseconds() % Wrap);
}

internal sealed record FieldPlacement(Vector2 Center, Angle Rotation) {
    public static FieldPlacement Default { get; } = new(new Vector2(0.5f), Angle.Zero);

    public Matrix3x2 Frame(PixelExtent extent) => DistanceField.Frame(extent, extent.Point(Center), Rotation);

    public DistanceField Placed(DistanceField field, PixelExtent extent) =>
        field.Placed((extent.Point(Center) - extent.Point(Default.Center)) * new Vector2(1f, -1f) / extent.ShortSide, Rotation);
}

internal sealed record GeneratedLayer(BlendingMode Mode, float Exposure) {
    public static GeneratedLayer Default { get; } = new(BlendingMode.Normal, 0f);
    public static GeneratedLayer Added { get; } = Default with { Mode = BlendingMode.Add };

    public Fin<Option<PixelPass>> Pass(Fin<Action<Span<Vector4>, int, int>> fill) => fill.Map(layer => Some<PixelPass>(new PixelPass.Positional(Layered(layer, float.Exp2(Exposure), Mode))));

    public Fin<Unit> Run(Fin<Action<Span<Vector4>, int, int>> fill, PixelFrame frame, Option<IProgress<int>> progress) =>
        fill.Bind(layer => new PixelPass.Positional(Layered(layer, float.Exp2(Exposure), Mode)).Run(frame, progress));

    private static Action<Span<Vector4>, int, int> Layered(Action<Span<Vector4>, int, int> fill, float gain, BlendingMode mode) =>
        (row, x, y) => {
            using SpanOwner<Vector4> layer = SpanOwner<Vector4>.Allocate(row.Length);
            using SpanOwner<float> weights = SpanOwner<float>.Allocate(row.Length);
            fill(layer.Span, x, y);
            for (int i = 0; i < row.Length; i++)
                (weights.Span[i], layer.Span[i]) = (layer.Span[i].W, new Vector4(layer.Span[i].AsVector3() * gain, 1f));
            if (mode == BlendingMode.Normal)
                mode.Composite(row, layer.Span, weights.Span);
            else {
                mode.Mixed(row, layer.Span, weights.Span);
                layer.Span.CopyTo(row);
            }
        };
}

// --- [OPERATIONS] ----------------------------------------------------------------------

internal static class OptionalLengths {
    extension(Option<ShortSideLength> length) {
        public float Share => length.Map(static held => (float)held).IfNone(0f);

        public double Pixels(PixelExtent extent) => length.Map(held => held.Pixels(extent)).IfNone(0d);
    }
}
