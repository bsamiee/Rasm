using System.Numerics;
using System.Runtime.InteropServices;
using Rasm.Imaging.ColorManagement;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Tone;
using UnitsNet;
using UnitsNet.Units;

namespace Rasm.Imaging.Filters.Generators;

// --- [CONSTANTS] -----------------------------------------------------------------------
file static class Tracks {
    public static Presentation<AxisFraction, float> Share { get; } = new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction) };
}

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidGenerator>]
public readonly partial struct SuperellipseExponent : IMinMaxValue<SuperellipseExponent> {
    public static SuperellipseExponent MinValue { get; } = new(1f);
    public static SuperellipseExponent MaxValue { get; } = new(8f);
    public static SuperellipseExponent Ellipse { get; } = new(2f);

    static partial void ValidateFactoryArguments(ref InvalidGenerator? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidGenerator();
}

[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidGenerator>]
public readonly partial struct SideCount : IMinMaxValue<SideCount> {
    public static SideCount MinValue { get; } = new(3);
    public static SideCount MaxValue { get; } = new(32);
    public static SideCount Five { get; } = new(5);

    static partial void ValidateFactoryArguments(ref InvalidGenerator? validationError, ref int value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidGenerator();
}

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Neutral", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidGenerator>]
public readonly partial struct ApexShift : IMinMaxValue<ApexShift> {
    public static ApexShift MinValue { get; } = new(-1f);
    public static ApexShift MaxValue { get; } = new(1f);

    static partial void ValidateFactoryArguments(ref InvalidGenerator? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidGenerator();
}

[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidGenerator>]
public readonly partial struct SubdivisionDepth : IMinMaxValue<SubdivisionDepth> {
    public static SubdivisionDepth MinValue { get; } = new(1);
    public static SubdivisionDepth MaxValue { get; } = new(5);

    static partial void ValidateFactoryArguments(ref InvalidGenerator? validationError, ref int value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidGenerator();
}

[ValueObject<int>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Neutral", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidGenerator>]
public readonly partial struct BranchCount : IMinMaxValue<BranchCount> {
    public static BranchCount MinValue => Neutral;
    public static BranchCount MaxValue { get; } = new(7);
    public static BranchCount Standard { get; } = new(3);

    static partial void ValidateFactoryArguments(ref InvalidGenerator? validationError, ref int value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidGenerator();
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidGenerator>]
public readonly partial struct ForkSpread : IMinMaxValue<ForkSpread> {
    public static ForkSpread MinValue { get; } = new(0f);
    public static ForkSpread MaxValue { get; } = new(float.Pi / 3f);
    public static ForkSpread Standard { get; } = new(0.5f);

    static partial void ValidateFactoryArguments(ref InvalidGenerator? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidGenerator();
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidGenerator>]
public readonly partial struct AspectRatio : IMinMaxValue<AspectRatio> {
    public static AspectRatio MinValue { get; } = new(0.25f);
    public static AspectRatio MaxValue { get; } = new(4f);
    public static AspectRatio Square { get; } = new(1f);
    public static AspectRatio Scope { get; } = new(2.39f);
    public static Presentation<AspectRatio, float> Presentation { get; } = new() { Scale = TrackScale.Log, Origin = (float)Square };

    static partial void ValidateFactoryArguments(ref InvalidGenerator? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidGenerator();
}

internal readonly record struct FieldSample(float Distance, float Width);

[SmartEnum<string>]
[ValidationError<InvalidGenerator>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ShapeFit {
    public static readonly ShapeFit ShortSide = new("short-side", static _ => Vector2.One);
    public static readonly ShapeFit LongSide = new("long-side", static extent => new Vector2(int.Max(extent.Width, extent.Height)) / extent.ShortSide);

    [UseDelegateFromConstructor]
    public partial Vector2 Scale(PixelExtent extent);
}

[SmartEnum<string>]
[ValidationError<InvalidGenerator>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ShapeKind {
    public static readonly ShapeKind Ellipse = new("ellipse", static (shape, half) => (point => ShapeFields.Ellipse(point, half, shape.Exponent), 2f * float.Min(half.X, half.Y)));
    public static readonly ShapeKind Box = new("box", static (shape, half) => float.Min(half.X, half.Y) switch {
        var small => (point => ShapeFields.Box(point, half, shape.Roundness * small), 2f * small),
    });
    public static readonly ShapeKind Polygon = new("polygon", static (shape, half) => {
        (float wedge, float radius) = (float.Pi / shape.Sides, shape.Roundness * half.X);
        (float sine, float cosine) = float.SinCos(wedge);
        (float depth, float reach) = (cosine * (1f - shape.Inset), half.X - radius);
        (Vector2 outer, Vector2 inner) = (new(0f, reach), reach * depth * new Vector2(sine, cosine));
        return (point => (point.Length() * float.SinCos(float.Abs(float.Ieee754Remainder(float.Atan2(point.X, point.Y), 2f * wedge))) switch { var (sin, cos) => new Vector2(sin, cos) }) switch {
            var folded => float.CopySign(ShapeFields.Offset(folded, outer, inner).Offset.Length(), ShapeFields.Side(folded, outer, inner)) - radius,
        }, half.X * (1f + depth));
    });
    public static readonly ShapeKind Ring = new("ring", static (shape, half) => (shape.Inset * float.Min(half.X, half.Y)) switch {
        var band => (point => float.Abs(ShapeFields.Ellipse(point, half, shape.Exponent) + (band / 2f)) - (band / 2f), band),
    });
    public static readonly ShapeKind Cross = new("cross", static (shape, half) => ((1f - shape.Inset) * float.Min(half.X, half.Y)) switch {
        var arm => (shape.Roundness * arm) switch { var radius => (point => float.Min(ShapeFields.Box(point, half with { Y = arm }, radius), ShapeFields.Box(point, half with { X = arm }, radius)), 2f * arm) },
    });
    public static readonly ShapeKind Heart = new("heart", static (_, half) => (Center: new Vector2(0.25f, 0.75f), Radius: float.Sqrt(2f) / 4f) switch {
        var lobe => (half.X / (lobe.Center.X + lobe.Radius), new Vector2(0f, (lobe.Center.Y + lobe.Radius) / 2f)) switch {
            var (scale, middle) => (point => ((Vector2.Abs(point) with { Y = point.Y } / scale) + middle) switch {
                var q when q.X + q.Y > 1f => scale * (Vector2.Distance(q, lobe.Center) - lobe.Radius),
                var q => scale * float.Sqrt(float.Min(Vector2.DistanceSquared(q, Vector2.UnitY), Vector2.DistanceSquared(q, 0.5f * float.Max(q.X + q.Y, 0f) * Vector2.One))) * float.Sign(q.X - q.Y),
            }, 2f * lobe.Radius * scale),
        },
    });
    public static readonly ShapeKind Triangle = new("triangle", static (shape, half) => {
        (Vector2 a, Vector2 b, Vector2 c) = (-half, half with { Y = -half.Y }, half with { X = shape.Apex * half.X });
        (float opposite, float across, float under) = (Vector2.Distance(b, c), Vector2.Distance(c, a), Vector2.Distance(a, b));
        (float perimeter, float twiceArea) = (opposite + across + under, 4f * half.X * half.Y);
        Vector2 center = ((opposite * a) + (across * b) + (under * c)) / perimeter;
        Func<Vector2, Vector2> inward = vertex => Vector2.Lerp(center, vertex, 1f - shape.Roundness);
        (Vector2 p, Vector2 q, Vector2 r, float radius) = (inward(a), inward(b), inward(c), shape.Roundness * twiceArea / perimeter);
        return (point => Vector2.Min(Vector2.Min(Edge(point, p, q), Edge(point, q, r)), Edge(point, r, p)) switch {
            var nearest => (float.Sqrt(nearest.X) * (nearest.Y > 0f ? -1f : 1f)) - radius,
        }, twiceArea / float.Max(under, float.Max(opposite, across)));

        static Vector2 Edge(Vector2 point, Vector2 start, Vector2 end) => new(ShapeFields.Offset(point, start, end).Offset.LengthSquared(), ShapeFields.Side(point, start, end));
    });

    [UseDelegateFromConstructor]
    internal partial (Func<Vector2, float> Distance, float Width) Field(ShapePrimitive primitive, Vector2 half);
}

[SmartEnum<string>]
[ValidationError<InvalidGenerator>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ShapeOperation {
    public static readonly ShapeOperation Union = new("union", static (first, second, smoothing, _) => new(Smooth(first.Distance, second.Distance, smoothing), float.Max(first.Width, second.Width)));
    public static readonly ShapeOperation Difference = new("difference", static (first, second, smoothing, _) => new(-Smooth(-first.Distance, second.Distance, smoothing), first.Width));
    public static readonly ShapeOperation Intersection = new("intersection", static (first, second, smoothing, _) => new(-Smooth(-first.Distance, -second.Distance, smoothing), float.Min(first.Width, second.Width)));
    public static readonly ShapeOperation Morph = new("morph", static (first, second, _, progress) => new(float.Lerp(first.Distance, second.Distance, progress), float.Lerp(first.Width, second.Width, progress)));

    [UseDelegateFromConstructor]
    internal partial FieldSample Join(FieldSample first, FieldSample second, float smoothing, float progress);

    private static float Smooth(float first, float second, float smoothing) =>
        (float.Min(first, second), smoothing - float.Abs(first - second)) switch {
            (var low, > 0f and var overlap) => low - (overlap * overlap / (4f * smoothing)),
            (var low, _) => low,
        };
}

[SmartEnum<string>]
[ValidationError<InvalidGenerator>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GradientKind {
    public static readonly GradientKind Linear = new("linear", periodic: false, static extent => point => (0.5f + (point.X / (2f * extent.X)), 1f / (2f * extent.X)));
    public static readonly GradientKind Radial = new("radial", periodic: false, static extent => point => ShapeFields.Level(point, extent, SuperellipseExponent.Ellipse));
    public static readonly GradientKind Angular = new("angular", periodic: true, static _ => static point => ((float.Atan2(point.Y, point.X) / float.Tau) + 0.5f, 1f / (float.Tau * point.Length())));
    public static readonly GradientKind Box = new("box", periodic: false, static extent => point => (Vector2.Abs(point) / extent) switch { var reach => reach.X >= reach.Y ? (reach.X, 1f / extent.X) : (reach.Y, 1f / extent.Y) });

    internal bool Periodic { get; }

    [UseDelegateFromConstructor]
    internal partial Func<Vector2, (float Position, float Slope)> Parameter(Vector2 extent);
}

[SmartEnum<string>]
[ValidationError<InvalidGenerator>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GridPattern {
    public static readonly GridPattern Lines = new("lines");
    public static readonly GridPattern Checker = new("checker");
    public static readonly GridPattern Brick = new("brick");
}

public sealed record ShapePrimitive(
    ShapeKind Kind, Placement Placement, ShortSideExtent Width, ShortSideExtent Height, AxisFraction Roundness, SuperellipseExponent Exponent, SideCount Sides, AxisFraction Inset,
    ApexShift Apex)
    : IStateRecord<ShapePrimitive, ShapePrimitiveParameter, InvalidGenerator> {
    public static ShapePrimitive Default { get; } = ShortSideExtent.Create(0.5f) switch {
        var extent => new(ShapeKind.Ellipse, Placement.Default, extent, extent, AxisFraction.MinValue, SuperellipseExponent.Ellipse, SideCount.Five, AxisFraction.MinValue, ApexShift.Neutral),
    };
    public ShapeFit Fit { get; init; } = ShapeFit.ShortSide;

    internal (FieldFrame Frame, Func<int, int, FieldSample> Sample) Field(PixelExtent extent) =>
        (Placement.Frame(extent), Kind.Field(this, new Vector2(Width, Height) * Fit.Scale(extent))) switch {
            var (frame, (distance, width)) => (frame, (column, line) => new(distance(frame.Upward(column, line)), width)),
        };
}

[SmartEnum<string>]
[ValidationError<InvalidGenerator>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ShapePrimitiveParameter : IStateParameter<ShapePrimitive> {
    private static readonly Presentation<ShortSideExtent, float> Extent = ShortSideExtent.Presentation with { Soft = (0.01f, 2f) };

    public static readonly ShapePrimitiveParameter Kind = new("kind", new StateParameter<ShapePrimitive>.Choice<ShapeKind, InvalidGenerator>(Lens<ShapePrimitive, ShapeKind>.New(static shape => shape.Kind, static kind => shape => shape with { Kind = kind })));
    public static readonly ShapePrimitiveParameter Placement = new("placement", new StateParameter<ShapePrimitive>.Record<Placement>(Lens<ShapePrimitive, Placement>.New(static shape => shape.Placement, static placement => shape => shape with { Placement = placement })));
    public static readonly ShapePrimitiveParameter Width = new("width", new StateParameter<ShapePrimitive>.Bounded<ShortSideExtent, float, InvalidPixelValue>(Lens<ShapePrimitive, ShortSideExtent>.New(static shape => shape.Width, static width => shape => shape with { Width = width }), Extent));
    public static readonly ShapePrimitiveParameter Height = new("height", new StateParameter<ShapePrimitive>.Bounded<ShortSideExtent, float, InvalidPixelValue>(Lens<ShapePrimitive, ShortSideExtent>.New(static shape => shape.Height, static height => shape => shape with { Height = height }), Extent));
    public static readonly ShapePrimitiveParameter Roundness = new("roundness", new StateParameter<ShapePrimitive>.Bounded<AxisFraction, float, InvalidGrade>(Lens<ShapePrimitive, AxisFraction>.New(static shape => shape.Roundness, static roundness => shape => shape with { Roundness = roundness }), Tracks.Share));
    public static readonly ShapePrimitiveParameter Exponent = new("exponent", new StateParameter<ShapePrimitive>.Bounded<SuperellipseExponent, float, InvalidGenerator>(Lens<ShapePrimitive, SuperellipseExponent>.New(static shape => shape.Exponent, static exponent => shape => shape with { Exponent = exponent }), new() { Scale = TrackScale.Log, Origin = (float)SuperellipseExponent.Ellipse }));
    public static readonly ShapePrimitiveParameter Sides = new("sides", new StateParameter<ShapePrimitive>.Bounded<SideCount, int, InvalidGenerator>(Lens<ShapePrimitive, SideCount>.New(static shape => shape.Sides, static sides => shape => shape with { Sides = sides }), new() { Step = 1 }));
    public static readonly ShapePrimitiveParameter Inset = new("inset", new StateParameter<ShapePrimitive>.Bounded<AxisFraction, float, InvalidGrade>(Lens<ShapePrimitive, AxisFraction>.New(static shape => shape.Inset, static inset => shape => shape with { Inset = inset }), Tracks.Share));
    public static readonly ShapePrimitiveParameter Apex = new("apex", new StateParameter<ShapePrimitive>.Bounded<ApexShift, float, InvalidGenerator>(Lens<ShapePrimitive, ApexShift>.New(static shape => shape.Apex, static apex => shape => shape with { Apex = apex }), new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction), Origin = (float)ApexShift.Neutral }));
    public static readonly ShapePrimitiveParameter Fit = new("fit", new StateParameter<ShapePrimitive>.Choice<ShapeFit, InvalidGenerator>(Lens<ShapePrimitive, ShapeFit>.New(static shape => shape.Fit, static fit => shape => shape with { Fit = fit })));

    StateParameter<ShapePrimitive> IStateParameter<ShapePrimitive>.Kind => Parameter;

    private StateParameter<ShapePrimitive> Parameter { get; }
}

public sealed record ShapeStyle(
    Gated<Swatch> Fill, Swatch Outline, ShortSideLength OutlineWidth, ShortSideLength OutlineOffset, ShortSideLength Feather, ShortSideLength Roughness, bool Inverted)
    : IStateRecord<ShapeStyle, ShapeStyleParameter, InvalidGenerator> {
    public static ShapeStyle Default { get; } = new(new(Enabled: true, Swatch.White), Swatch.Black, ShortSideLength.Neutral, ShortSideLength.Neutral, ShortSideLength.Neutral, ShortSideLength.Neutral, Inverted: false);
    private static readonly Func<Vector4, uint, NoiseSample> Gate = (NoiseBasis.Default with {
        Kind = BasisKind.RidgedMultifractal,
        Dimensions = NoiseDimensions.Four,
        Octaves = new(FractalDetail.MaxValue, AxisFraction.Create(0.9f), FractalLacunarity.Standard),
        Distortion = NoiseDistortion.Create(1.9f),
        FractalGain = FractalGain.Create(74.5f),
    }).Sampler;

    internal Func<FieldSample, Vector2, Vector4> Shading(PassContext context, uint field) {
        Func<Swatch, Vector3> light = context.Signal == TransferCurve.Linear ? swatch => swatch.SceneLight(context.Working) : static swatch => swatch.Display.AsVector3();
        (Vector4 fill, Vector3 outline, float pixels, float band) = (Fill.Active.Map(swatch => new Vector4(light(swatch), 1f)).IfNone(Vector4.Zero), light(Outline), context.Extent.ShortSide, ShapeEdge.Band(Feather, context.Extent));
        (float offset, float width, float reach) = (OutlineOffset.Pixels(context.Extent), OutlineWidth.Pixels(context.Extent), Roughness.Pixels(context.Extent));
        (float near, float far) = (-reach - (band / 2f), offset + width + reach + (band / 2f));
        Func<float, Vector2, float> rough = reach == 0f ? static (distance, _) => distance
            : (distance, local) => distance < near || distance > far ? distance : distance + (reach * ((2f * Easing.Saturate(Gate(new Vector4(2.5f * local, 0f, 0f), field).Value)) - 1f));
        return (sample, local) => (Inverted ? new FieldSample(-sample.Distance, float.PositiveInfinity) : sample) switch {
            var (distance, narrowest) => (rough(distance * pixels, local), narrowest * pixels) switch {
                var (edge, span) => (fill.W * Grown(edge, span, 0f), Grown(edge, span, offset + width) - Grown(edge, span, offset)) switch {
                    var (covered, ring) when covered + ring > 0f => new Vector4(((covered * fill.AsVector3()) + (ring * outline)) / (covered + ring), covered + ring),
                    _ => Vector4.Zero,
                },
            },
        };

        float Grown(float distance, float span, float growth) => float.Min(ShapeEdge.Coverage(distance - growth, band), (span + (2f * growth)) / band);
    }
}

[SmartEnum<string>]
[ValidationError<InvalidGenerator>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ShapeStyleParameter : IStateParameter<ShapeStyle> {
    private static readonly Presentation<ShortSideLength, float> Ring = ShortSideLength.Presentation with { Soft = (0f, 0.1f) };

    public static readonly ShapeStyleParameter Fill = new("fill", new StateParameter<ShapeStyle>.OptionalColor(Lens<ShapeStyle, Gated<Swatch>>.New(static style => style.Fill, static value => style => style with { Fill = value })));
    public static readonly ShapeStyleParameter Outline = new("outline", new StateParameter<ShapeStyle>.Color(Lens<ShapeStyle, Swatch>.New(static style => style.Outline, static value => style => style with { Outline = value })));
    public static readonly ShapeStyleParameter OutlineWidth = new("outline-width", new StateParameter<ShapeStyle>.Bounded<ShortSideLength, float, InvalidPixelValue>(Lens<ShapeStyle, ShortSideLength>.New(static style => style.OutlineWidth, static value => style => style with { OutlineWidth = value }), Ring));
    public static readonly ShapeStyleParameter OutlineOffset = new("outline-offset", new StateParameter<ShapeStyle>.Bounded<ShortSideLength, float, InvalidPixelValue>(Lens<ShapeStyle, ShortSideLength>.New(static style => style.OutlineOffset, static value => style => style with { OutlineOffset = value }), Ring));
    public static readonly ShapeStyleParameter Feather = new("feather", new StateParameter<ShapeStyle>.Bounded<ShortSideLength, float, InvalidPixelValue>(Lens<ShapeStyle, ShortSideLength>.New(static style => style.Feather, static value => style => style with { Feather = value }), ShortSideLength.Presentation with { Soft = (0f, 0.25f) }));
    public static readonly ShapeStyleParameter Roughness = new("roughness", new StateParameter<ShapeStyle>.Bounded<ShortSideLength, float, InvalidPixelValue>(Lens<ShapeStyle, ShortSideLength>.New(static style => style.Roughness, static value => style => style with { Roughness = value }), ShortSideLength.Presentation with { Soft = (0f, 0.02f) }));
    public static readonly ShapeStyleParameter Inverted = new("inverted", new StateParameter<ShapeStyle>.Toggle(Lens<ShapeStyle, bool>.New(static style => style.Inverted, static inverted => style => style with { Inverted = inverted })));

    public StateParameter<ShapeStyle> Kind { get; }
}

public sealed record Shapes(
    ShapePrimitive First, Option<ShapeOperation> Operation, ShapePrimitive Second, ShortSideLength Smoothing, Mix Progress, ShapeStyle Style, GeneratedLayer Layer, Seed Seed)
    : IStateRecord<Shapes, ShapesParameter, InvalidGenerator>, IPixelStage<Shapes> {
    public static Shapes Default { get; } = new(
        ShapePrimitive.Default, None, ShapePrimitive.Default with { Kind = ShapeKind.Box, Roundness = AxisFraction.Create(0.3f / 2f / ShapePrimitive.Default.Width) },
        ShortSideLength.Neutral, Mix.MinValue, ShapeStyle.Default, GeneratedLayer.Default, Seed.MinValue);

    public static Option<PixelPass> Pass(Shapes state, PassContext context) => Some(state.Layer.Pass(Fill(state, context)));

    public static PixelFrame Matte(Shapes state, PassContext context, PixelFrame frame) =>
        Fill(state, context) switch {
            var fill => new(frame.Origin, frame.Size, frame.Extent, block => {
                Span<Vector4> pixels = MemoryMarshal.Cast<float, Vector4>(block.AsSpan());
                for (int i = 0; i < frame.Size.Height; i++)
                    fill(pixels.Slice(i * frame.Size.Width, frame.Size.Width), frame.Origin.X, frame.Line(i));
            }),
        };

    private static Action<Span<Vector4>, int, int> Fill(Shapes state, PassContext context) {
        (FieldFrame frame, Func<int, int, FieldSample> first) = state.First.Field(context.Extent);
        Func<int, int, FieldSample> field = state.Operation.Match(
            Some: operation => (state.Second.Field(context.Extent).Sample, (float)state.Smoothing, (float)state.Progress) switch {
                var (second, smoothing, progress) => (column, line) => operation.Join(first(column, line), second(column, line), smoothing, progress),
            },
            None: () => first);
        Func<FieldSample, Vector2, Vector4> shade = state.Style.Shading(context, CoordinateHash.Field(NoiseStream.Shape, state.Seed, 0u));
        return (row, column, line) => {
            for (int i = 0; i < row.Length; i++)
                row[i] = shade(field(column + i, line), frame.Upward(column + i, line));
        };
    }
}

[SmartEnum<string>]
[ValidationError<InvalidGenerator>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ShapesParameter : IStateParameter<Shapes> {
    public static readonly ShapesParameter First = new("first", new StateParameter<Shapes>.Record<ShapePrimitive>(Lens<Shapes, ShapePrimitive>.New(static shape => shape.First, static first => shape => shape with { First = first })));
    public static readonly ShapesParameter Second = new("second", new StateParameter<Shapes>.Record<ShapePrimitive>(Lens<Shapes, ShapePrimitive>.New(static shape => shape.Second, static second => shape => shape with { Second = second })));
    public static readonly ShapesParameter Operation = new("operation", new StateParameter<Shapes>.OptionalChoice<ShapeOperation, InvalidGenerator>(Lens<Shapes, Option<ShapeOperation>>.New(static shape => shape.Operation, static operation => shape => shape with { Operation = operation })));
    public static readonly ShapesParameter Smoothing = new("smoothing", new StateParameter<Shapes>.Bounded<ShortSideLength, float, InvalidPixelValue>(Lens<Shapes, ShortSideLength>.New(static shape => shape.Smoothing, static smoothing => shape => shape with { Smoothing = smoothing }), ShortSideLength.Presentation with { Soft = (0f, 0.25f) }));
    public static readonly ShapesParameter Progress = new("progress", new StateParameter<Shapes>.Bounded<Mix, float, InvalidGrade>(Lens<Shapes, Mix>.New(static shape => shape.Progress, static progress => shape => shape with { Progress = progress }), new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction) }));
    public static readonly ShapesParameter Style = new("style", new StateParameter<Shapes>.Record<ShapeStyle>(Lens<Shapes, ShapeStyle>.New(static shape => shape.Style, static style => shape => shape with { Style = style })));
    public static readonly ShapesParameter Layer = new("layer", new StateParameter<Shapes>.Record<GeneratedLayer>(Lens<Shapes, GeneratedLayer>.New(static shape => shape.Layer, static layer => shape => shape with { Layer = layer })));
    public static readonly ShapesParameter Seed = new("seed", new StateParameter<Shapes>.Bounded<Seed, int, InvalidGenerator>(Lens<Shapes, Seed>.New(static shape => shape.Seed, static seed => shape => shape with { Seed = seed }), Generators.Seed.Presentation));

    public StateParameter<Shapes> Kind { get; }
}

public sealed record Lightning(
    Placement Placement, ShortSideLength Length, ShortSideLength Thickness, SubdivisionDepth Depth, AxisFraction Displacement, BranchCount Branches, AxisFraction Reach,
    ForkSpread Spread, AxisFraction Taper, Frequency Speed, ShapeStyle Style, GeneratedLayer Layer, Seed Seed, Timing Timing, Hold Hold)
    : IStateRecord<Lightning, LightningParameter, InvalidGenerator>, IPixelStage<Lightning> {
    public static Lightning Default { get; } = Hold.Create(0.05f) switch {
        var hold => new(
            Placement.Default, ShortSideLength.Create(1f / 2f), ShortSideLength.Create(0.01f / (5f - (4.5f * 0.5f)) * ReferenceFrame.GreaterSide), SubdivisionDepth.MaxValue, AxisFraction.Half,
            BranchCount.Standard, AxisFraction.Create(0.4f), ForkSpread.Standard, AxisFraction.Half, Frequency.Create(1f / hold), ShapeStyle.Default,
            GeneratedLayer.Added with { Exposure = Exposure.Create(3f) }, Seed.MinValue, Timing.Default, hold),
    };

    public static Option<PixelPass> Pass(Lightning state, PassContext context) {
        (uint field, float clock, float chord) = (CoordinateHash.Field(NoiseStream.Lightning, state.Seed, 0u), 2f * state.Speed * state.Hold.Held(state.Timing.At(context)), 2f * state.Length);
        Func<int, int, int, float> noise = (x, y, z) => 0.5f + (0.5f * NoiseDimensions.Four.Gradient(new Vector4((2f * x) + 0.5f, (2f * y) + 0.5f, (2f * z) + 0.5f, clock), field));
        BoltPiece[] channel = BoltPiece.Path(new(0f, state.Length), -Vector2.UnitY, chord, SubdivisionDepth.MaxValue, state, k => noise(k, 0, 0), _ => state.Thickness / 2f);
        BoltPiece[] pieces = [.. channel, .. Enumerable.Range(1, state.Branches).SelectMany(i => (BitOperations.Log2((uint)i), (i % 2 == 0 ? 1f : -1f) * state.Spread * (0.5f + noise(i, 0, 1))) switch {
            var (level, turn) => BoltPiece.Path(
                channel[((2 * (i - (1 << level))) + 1) * channel.Length / (2 << level)].Start, Vector2.Transform(-Vector2.UnitY, Matrix3x2.CreateRotation(turn)),
                state.Reach * chord * (0.5f + noise(i, 0, 2)), levels: 3, state, j => noise(j, i, 0), along => state.Taper * state.Thickness * (1f - along) / 2f),
        })];
        Func<FieldSample, Vector2, Vector4> shade = state.Style.Shading(context, CoordinateHash.Branch(field, 0u));
        return Some(state.Layer.Pass(state.Placement.Frame(context.Extent).Fill(point => {
            FieldSample nearest = new(float.PositiveInfinity, 0f);
            foreach (BoltPiece piece in pieces)
                nearest = piece.Sample(point) switch { var next => next.Distance < nearest.Distance ? next : nearest };
            return shade(nearest, point);
        })));
    }
}

[SmartEnum<string>]
[ValidationError<InvalidGenerator>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LightningParameter : IStateParameter<Lightning> {
    public static readonly LightningParameter Placement = new("placement", new StateParameter<Lightning>.Record<Placement>(Lens<Lightning, Placement>.New(static bolt => bolt.Placement, static placement => bolt => bolt with { Placement = placement })));
    public static readonly LightningParameter Length = new("length", new StateParameter<Lightning>.Bounded<ShortSideLength, float, InvalidPixelValue>(Lens<Lightning, ShortSideLength>.New(static bolt => bolt.Length, static length => bolt => bolt with { Length = length }), ShortSideLength.Presentation));
    public static readonly LightningParameter Thickness = new("thickness", new StateParameter<Lightning>.Bounded<ShortSideLength, float, InvalidPixelValue>(Lens<Lightning, ShortSideLength>.New(static bolt => bolt.Thickness, static thickness => bolt => bolt with { Thickness = thickness }), ShortSideLength.Presentation with { Soft = (0f, 0.02f) }));
    public static readonly LightningParameter Depth = new("depth", new StateParameter<Lightning>.Bounded<SubdivisionDepth, int, InvalidGenerator>(Lens<Lightning, SubdivisionDepth>.New(static bolt => bolt.Depth, static depth => bolt => bolt with { Depth = depth }), new() { Step = 1 }));
    public static readonly LightningParameter Displacement = new("displacement", new StateParameter<Lightning>.Bounded<AxisFraction, float, InvalidGrade>(Lens<Lightning, AxisFraction>.New(static bolt => bolt.Displacement, static displacement => bolt => bolt with { Displacement = displacement }), Tracks.Share));
    public static readonly LightningParameter Branches = new("branches", new StateParameter<Lightning>.Bounded<BranchCount, int, InvalidGenerator>(Lens<Lightning, BranchCount>.New(static bolt => bolt.Branches, static branches => bolt => bolt with { Branches = branches }), new() { Step = 1 }));
    public static readonly LightningParameter Reach = new("reach", new StateParameter<Lightning>.Bounded<AxisFraction, float, InvalidGrade>(Lens<Lightning, AxisFraction>.New(static bolt => bolt.Reach, static reach => bolt => bolt with { Reach = reach }), Tracks.Share));
    public static readonly LightningParameter Spread = new("spread", new StateParameter<Lightning>.Bounded<ForkSpread, float, InvalidGenerator>(Lens<Lightning, ForkSpread>.New(static bolt => bolt.Spread, static spread => bolt => bolt with { Spread = spread }), new() { Unit = Quantity.GetUnitInfo(AngleUnit.Radian) }));
    public static readonly LightningParameter Taper = new("taper", new StateParameter<Lightning>.Bounded<AxisFraction, float, InvalidGrade>(Lens<Lightning, AxisFraction>.New(static bolt => bolt.Taper, static taper => bolt => bolt with { Taper = taper }), Tracks.Share));
    public static readonly LightningParameter Speed = new("speed", new StateParameter<Lightning>.Bounded<Frequency, float, InvalidGenerator>(Lens<Lightning, Frequency>.New(static bolt => bolt.Speed, static speed => bolt => bolt with { Speed = speed }), Frequency.Presentation));
    public static readonly LightningParameter Style = new("style", new StateParameter<Lightning>.Record<ShapeStyle>(Lens<Lightning, ShapeStyle>.New(static bolt => bolt.Style, static style => bolt => bolt with { Style = style })));
    public static readonly LightningParameter Layer = new("layer", new StateParameter<Lightning>.Record<GeneratedLayer>(Lens<Lightning, GeneratedLayer>.New(static bolt => bolt.Layer, static layer => bolt => bolt with { Layer = layer })));
    public static readonly LightningParameter Seed = new("seed", new StateParameter<Lightning>.Bounded<Seed, int, InvalidGenerator>(Lens<Lightning, Seed>.New(static bolt => bolt.Seed, static seed => bolt => bolt with { Seed = seed }), Generators.Seed.Presentation));
    public static readonly LightningParameter Timing = new("timing", new StateParameter<Lightning>.Record<Timing>(Lens<Lightning, Timing>.New(static bolt => bolt.Timing, static timing => bolt => bolt with { Timing = timing })));
    public static readonly LightningParameter Hold = new("hold", new StateParameter<Lightning>.Bounded<Hold, float, InvalidGenerator>(Lens<Lightning, Hold>.New(static bolt => bolt.Hold, static hold => bolt => bolt with { Hold = hold }), Generators.Hold.Presentation));

    public StateParameter<Lightning> Kind { get; }
}

public sealed record Gradient(GradientKind Kind, Placement Placement, ShortSideExtent Width, ShortSideExtent Height, Ramp Colors, GeneratedLayer Layer)
    : IStateRecord<Gradient, GradientParameter, InvalidGenerator>, IPixelStage<Gradient> {
    public static Gradient Default { get; } = ShortSideExtent.Create(1f / 2f) switch {
        var extent => new(GradientKind.Linear, Placement.Default, extent, extent, Ramp.Grayscale, GeneratedLayer.Default),
    };

    public static Option<PixelPass> Pass(Gradient state, PassContext context) =>
        (state.Kind.Parameter(new Vector2(state.Width, state.Height)), RampFootprint.Of((state.Colors, None)), (float)context.Extent.ShortSide) switch {
            var (parameter, footprint, pixels) => Some(state.Layer.Pass(state.Placement.Frame(context.Extent).Fill(point =>
                parameter(point) switch { var (position, slope) => footprint.Mean(position, slope / pixels, state.Kind.Periodic) }))),
        };
}

[SmartEnum<string>]
[ValidationError<InvalidGenerator>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GradientParameter : IStateParameter<Gradient> {
    private static readonly Presentation<ShortSideExtent, float> Extent = ShortSideExtent.Presentation with { Soft = (0.01f, 4f) };

    public static readonly GradientParameter Kind = new("kind", new StateParameter<Gradient>.Choice<GradientKind, InvalidGenerator>(Lens<Gradient, GradientKind>.New(static gradient => gradient.Kind, static kind => gradient => gradient with { Kind = kind })));
    public static readonly GradientParameter Placement = new("placement", new StateParameter<Gradient>.Record<Placement>(Lens<Gradient, Placement>.New(static gradient => gradient.Placement, static placement => gradient => gradient with { Placement = placement })));
    public static readonly GradientParameter Width = new("width", new StateParameter<Gradient>.Bounded<ShortSideExtent, float, InvalidPixelValue>(Lens<Gradient, ShortSideExtent>.New(static gradient => gradient.Width, static width => gradient => gradient with { Width = width }), Extent));
    public static readonly GradientParameter Height = new("height", new StateParameter<Gradient>.Bounded<ShortSideExtent, float, InvalidPixelValue>(Lens<Gradient, ShortSideExtent>.New(static gradient => gradient.Height, static height => gradient => gradient with { Height = height }), Extent));
    public static readonly GradientParameter Colors = new("colors", new StateParameter<Gradient>.Gradient(Lens<Gradient, Ramp>.New(static gradient => gradient.Colors, static colors => gradient => gradient with { Colors = colors })));
    public static readonly GradientParameter Layer = new("layer", new StateParameter<Gradient>.Record<GeneratedLayer>(Lens<Gradient, GeneratedLayer>.New(static gradient => gradient.Layer, static layer => gradient => gradient with { Layer = layer })));

    StateParameter<Gradient> IStateParameter<Gradient>.Kind => Parameter;

    private StateParameter<Gradient> Parameter { get; }
}

public sealed record CornerGradient(Swatch TopLeft, Swatch TopRight, Swatch BottomRight, Swatch BottomLeft, GeneratedLayer Layer)
    : IStateRecord<CornerGradient, CornerGradientParameter, InvalidGenerator>, IPixelStage<CornerGradient> {
    public static CornerGradient Default { get; } = new(Swatch.White, Swatch.Black, Swatch.White, Swatch.Black, GeneratedLayer.Default);

    public static Option<PixelPass> Pass(CornerGradient state, PassContext context) {
        (Vector3 topLeft, Vector3 topRight, Vector3 bottomRight, Vector3 bottomLeft) =
            (state.TopLeft.Display.AsVector3(), state.TopRight.Display.AsVector3(), state.BottomRight.Display.AsVector3(), state.BottomLeft.Display.AsVector3());
        Vector2 share = new Vector2(context.Extent.ShortSide) / new Vector2(context.Extent.Width, -context.Extent.Height);
        return Some(state.Layer.Pass(Placement.Default.Frame(context.Extent).Fill(point => (Vector2.Create(0.5f) + (point * share)) switch {
            var at => ((float)RampInterpolation.SmootherRgb.Weight(at.X), (float)RampInterpolation.SmootherRgb.Weight(at.Y)) switch {
                var (across, down) => new Vector4(Vector3.Lerp(Vector3.Lerp(topLeft, topRight, across), Vector3.Lerp(bottomLeft, bottomRight, across), down), 1f),
            },
        })));
    }
}

[SmartEnum<string>]
[ValidationError<InvalidGenerator>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CornerGradientParameter : IStateParameter<CornerGradient> {
    public static readonly CornerGradientParameter TopLeft = new("top-left", new StateParameter<CornerGradient>.Color(Lens<CornerGradient, Swatch>.New(static corners => corners.TopLeft, static color => corners => corners with { TopLeft = color })));
    public static readonly CornerGradientParameter TopRight = new("top-right", new StateParameter<CornerGradient>.Color(Lens<CornerGradient, Swatch>.New(static corners => corners.TopRight, static color => corners => corners with { TopRight = color })));
    public static readonly CornerGradientParameter BottomRight = new("bottom-right", new StateParameter<CornerGradient>.Color(Lens<CornerGradient, Swatch>.New(static corners => corners.BottomRight, static color => corners => corners with { BottomRight = color })));
    public static readonly CornerGradientParameter BottomLeft = new("bottom-left", new StateParameter<CornerGradient>.Color(Lens<CornerGradient, Swatch>.New(static corners => corners.BottomLeft, static color => corners => corners with { BottomLeft = color })));
    public static readonly CornerGradientParameter Layer = new("layer", new StateParameter<CornerGradient>.Record<GeneratedLayer>(Lens<CornerGradient, GeneratedLayer>.New(static corners => corners.Layer, static layer => corners => corners with { Layer = layer })));

    public StateParameter<CornerGradient> Kind { get; }
}

public sealed record Grid(
    GridPattern Pattern, Placement Placement, ShortSideExtent Cell, AspectRatio Aspect, AxisFraction Offset, ShortSideLength LineWidth,
    ShortSideLength Feather, Swatch LineColor, Swatch CellColor, Swatch AlternateColor, GeneratedLayer Layer, Seed Seed)
    : IStateRecord<Grid, GridParameter, InvalidGenerator>, IPixelStage<Grid> {
    public static Grid Default { get; } = (Size: 3f, Thickness: 0.2f, Factor: 0.1f) switch {
        var (size, thickness, factor) => new(
            GridPattern.Lines, Placement.Default, ShortSideExtent.Create(1f / size), AspectRatio.Square, AxisFraction.MinValue,
            ShortSideLength.Create(2f * (0.02f + (0.18f * thickness)) * (0.1f + (0.4f * factor)) / size), ShortSideLength.Neutral,
            Swatch.White, Swatch.Black, Swatch.White, GeneratedLayer.Default, Seed.MinValue),
    };

    public static Option<PixelPass> Pass(Grid state, PassContext context) =>
        Some(state.Layer.Pass(state.Placement.Frame(context.Extent).Fill(state.Pattern.Switch(
            (Lattice: new GridLattice(state, context.Extent), Cell: state.CellColor.Display.AsVector3(), Alternate: state.AlternateColor.Display.AsVector3(),
                Field: CoordinateHash.Field(NoiseStream.Grid, state.Seed, 0u)),
            lines: static at => at.Lattice.Lines,
            checker: static at => at.Lattice.Cells((i, j) => ((i + j) & 1) == 0 ? at.Cell : at.Alternate),
            brick: static at => at.Lattice.Cells((i, j) => Vector3.Lerp(at.Cell, at.Alternate, NoiseFunctions.White(new Vector4(i, j, 0f, 0f), at.Field).X))))));
}

[SmartEnum<string>]
[ValidationError<InvalidGenerator>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GridParameter : IStateParameter<Grid> {
    private static readonly Presentation<ShortSideLength, float> Line = ShortSideLength.Presentation with { Soft = (0f, 0.05f) };

    public static readonly GridParameter Pattern = new("pattern", new StateParameter<Grid>.Choice<GridPattern, InvalidGenerator>(Lens<Grid, GridPattern>.New(static grid => grid.Pattern, static pattern => grid => grid with { Pattern = pattern })));
    public static readonly GridParameter Placement = new("placement", new StateParameter<Grid>.Record<Placement>(Lens<Grid, Placement>.New(static grid => grid.Placement, static placement => grid => grid with { Placement = placement })));
    public static readonly GridParameter Cell = new("cell", new StateParameter<Grid>.Bounded<ShortSideExtent, float, InvalidPixelValue>(Lens<Grid, ShortSideExtent>.New(static grid => grid.Cell, static cell => grid => grid with { Cell = cell }), ShortSideExtent.Presentation with { Soft = (0.01f, 1f) }));
    public static readonly GridParameter Aspect = new("aspect", new StateParameter<Grid>.Bounded<AspectRatio, float, InvalidGenerator>(Lens<Grid, AspectRatio>.New(static grid => grid.Aspect, static aspect => grid => grid with { Aspect = aspect }), AspectRatio.Presentation));
    public static readonly GridParameter Offset = new("offset", new StateParameter<Grid>.Bounded<AxisFraction, float, InvalidGrade>(Lens<Grid, AxisFraction>.New(static grid => grid.Offset, static offset => grid => grid with { Offset = offset }), Tracks.Share));
    public static readonly GridParameter LineWidth = new("line-width", new StateParameter<Grid>.Bounded<ShortSideLength, float, InvalidPixelValue>(Lens<Grid, ShortSideLength>.New(static grid => grid.LineWidth, static width => grid => grid with { LineWidth = width }), Line));
    public static readonly GridParameter Feather = new("feather", new StateParameter<Grid>.Bounded<ShortSideLength, float, InvalidPixelValue>(Lens<Grid, ShortSideLength>.New(static grid => grid.Feather, static feather => grid => grid with { Feather = feather }), Line));
    public static readonly GridParameter LineColor = new("line-color", new StateParameter<Grid>.Color(Lens<Grid, Swatch>.New(static grid => grid.LineColor, static color => grid => grid with { LineColor = color })));
    public static readonly GridParameter CellColor = new("cell-color", new StateParameter<Grid>.Color(Lens<Grid, Swatch>.New(static grid => grid.CellColor, static color => grid => grid with { CellColor = color })));
    public static readonly GridParameter AlternateColor = new("alternate-color", new StateParameter<Grid>.Color(Lens<Grid, Swatch>.New(static grid => grid.AlternateColor, static color => grid => grid with { AlternateColor = color })));
    public static readonly GridParameter Layer = new("layer", new StateParameter<Grid>.Record<GeneratedLayer>(Lens<Grid, GeneratedLayer>.New(static grid => grid.Layer, static layer => grid => grid with { Layer = layer })));
    public static readonly GridParameter Seed = new("seed", new StateParameter<Grid>.Bounded<Seed, int, InvalidGenerator>(Lens<Grid, Seed>.New(static grid => grid.Seed, static seed => grid => grid with { Seed = seed }), Generators.Seed.Presentation));

    public StateParameter<Grid> Kind { get; }
}

public sealed record Stripes(
    WaveProfile Profile, Placement Placement, ShortSideExtent Wavelength, SignedAngle Bend, AxisFraction Phase, Frequency Speed, Ramp Colors, GeneratedLayer Layer, Timing Timing)
    : IStateRecord<Stripes, StripesParameter, InvalidGenerator>, IPixelStage<Stripes> {
    public static Stripes Default { get; } = new(
        WaveProfile.Sine, Placement.Default, ShortSideExtent.Create(float.Tau / 20f), SignedAngle.Neutral, AxisFraction.MinValue, Frequency.Neutral, Ramp.Grayscale,
        GeneratedLayer.Default, Timing.Default);

    public static Option<PixelPass> Pass(Stripes state, PassContext context) =>
        (RampFootprint.Of((state.Colors, Some(state.Profile))), float.SinCos(state.Bend), (float)((state.Phase + ((double)state.Speed * state.Timing.At(context))) % 1d)) switch {
            var (footprint, (sin, cos), start) => Some(state.Layer.Pass(state.Placement.Frame(context.Extent).Fill(point =>
                footprint.Mean((((point.Y * cos) + (float.Abs(point.X) * sin)) / state.Wavelength) + start, 1f / state.Wavelength.Pixels(context.Extent), periodic: true)))),
        };
}

[SmartEnum<string>]
[ValidationError<InvalidGenerator>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class StripesParameter : IStateParameter<Stripes> {
    public static readonly StripesParameter Profile = new("profile", new StateParameter<Stripes>.Choice<WaveProfile, InvalidGenerator>(Lens<Stripes, WaveProfile>.New(static stripes => stripes.Profile, static profile => stripes => stripes with { Profile = profile })));
    public static readonly StripesParameter Placement = new("placement", new StateParameter<Stripes>.Record<Placement>(Lens<Stripes, Placement>.New(static stripes => stripes.Placement, static placement => stripes => stripes with { Placement = placement })));
    public static readonly StripesParameter Wavelength = new("wavelength", new StateParameter<Stripes>.Bounded<ShortSideExtent, float, InvalidPixelValue>(Lens<Stripes, ShortSideExtent>.New(static stripes => stripes.Wavelength, static wavelength => stripes => stripes with { Wavelength = wavelength }), ShortSideExtent.Presentation with { Soft = (0.01f, 1f) }));
    public static readonly StripesParameter Bend = new("bend", new StateParameter<Stripes>.Bounded<SignedAngle, float, InvalidPixelValue>(Lens<Stripes, SignedAngle>.New(static stripes => stripes.Bend, static bend => stripes => stripes with { Bend = bend }), SignedAngle.Presentation));
    public static readonly StripesParameter Phase = new("phase", new StateParameter<Stripes>.Bounded<AxisFraction, float, InvalidGrade>(Lens<Stripes, AxisFraction>.New(static stripes => stripes.Phase, static phase => stripes => stripes with { Phase = phase }), Tracks.Share));
    public static readonly StripesParameter Speed = new("speed", new StateParameter<Stripes>.Bounded<Frequency, float, InvalidGenerator>(Lens<Stripes, Frequency>.New(static stripes => stripes.Speed, static speed => stripes => stripes with { Speed = speed }), Frequency.Presentation with { Soft = (-4f, 4f) }));
    public static readonly StripesParameter Colors = new("colors", new StateParameter<Stripes>.Gradient(Lens<Stripes, Ramp>.New(static stripes => stripes.Colors, static colors => stripes => stripes with { Colors = colors })));
    public static readonly StripesParameter Layer = new("layer", new StateParameter<Stripes>.Record<GeneratedLayer>(Lens<Stripes, GeneratedLayer>.New(static stripes => stripes.Layer, static layer => stripes => stripes with { Layer = layer })));
    public static readonly StripesParameter Timing = new("timing", new StateParameter<Stripes>.Record<Timing>(Lens<Stripes, Timing>.New(static stripes => stripes.Timing, static timing => stripes => stripes with { Timing = timing })));

    public StateParameter<Stripes> Kind { get; }
}

public sealed record Letterbox(Gated<AspectRatio> Aspect, ShortSideLength Inset, ShortSideLength Radius, ShapeStyle Style, GeneratedLayer Layer, Seed Seed)
    : IStateRecord<Letterbox, LetterboxParameter, InvalidGenerator>, IPixelStage<Letterbox> {
    public static Letterbox Default { get; } = new(
        new(Enabled: true, AspectRatio.Scope), ShortSideLength.Neutral, ShortSideLength.Neutral, ShapeStyle.Default with { Fill = new(Enabled: true, Swatch.Black), Inverted = true }, GeneratedLayer.Default, Seed.MinValue);

    public static Letterbox FilmGate { get; } = (Padding: 40f, Radius: 40f, Blur: 10f, Intensity: 6f) switch {
        var (padding, radius, blur, intensity) => Default with {
            Aspect = Default.Aspect with { Enabled = false },
            Inset = ShortSideLength.Create(padding / ReferenceFrame.Height),
            Radius = ShortSideLength.Create(radius / ReferenceFrame.Height),
            Style = Default.Style with { Feather = ShortSideLength.Create(blur / 3f * float.Sqrt(float.Tau) / ReferenceFrame.Height), Roughness = ShortSideLength.Create(intensity / ReferenceFrame.Height) },
        },
    };

    public static Option<PixelPass> Pass(Letterbox state, PassContext context) {
        Vector2 frame = new Vector2(context.Extent.Width, context.Extent.Height) / context.Extent.ShortSide;
        Vector2 half = (state.Aspect.Active.Map(aspect => aspect >= frame.X / frame.Y ? new Vector2(frame.X, frame.X / aspect) : new Vector2(frame.Y * aspect, frame.Y)).IfNone(frame) / 2f) - new Vector2(state.Inset);
        Func<FieldSample, Vector2, Vector4> shade = state.Style.Shading(context, CoordinateHash.Field(NoiseStream.Shape, state.Seed, 0u));
        return Some(state.Layer.Pass(Placement.Default.Frame(context.Extent).Fill(point => shade(new(ShapeFields.Box(point, half, state.Radius), 2f * float.Min(half.X, half.Y)), point))));
    }
}

[SmartEnum<string>]
[ValidationError<InvalidGenerator>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LetterboxParameter : IStateParameter<Letterbox> {
    private static readonly Presentation<ShortSideLength, float> Margin = ShortSideLength.Presentation with { Soft = (0f, 0.1f * ReferenceFrame.GreaterSide) };

    public static readonly LetterboxParameter Aspect = new("aspect", new StateParameter<Letterbox>.OptionalBounded<AspectRatio, float, InvalidGenerator>(Lens<Letterbox, Gated<AspectRatio>>.New(static bars => bars.Aspect, static aspect => bars => bars with { Aspect = aspect }), AspectRatio.Presentation));
    public static readonly LetterboxParameter Inset = new("inset", new StateParameter<Letterbox>.Bounded<ShortSideLength, float, InvalidPixelValue>(Lens<Letterbox, ShortSideLength>.New(static bars => bars.Inset, static inset => bars => bars with { Inset = inset }), Margin));
    public static readonly LetterboxParameter Radius = new("radius", new StateParameter<Letterbox>.Bounded<ShortSideLength, float, InvalidPixelValue>(Lens<Letterbox, ShortSideLength>.New(static bars => bars.Radius, static radius => bars => bars with { Radius = radius }), Margin));
    public static readonly LetterboxParameter Style = new("style", new StateParameter<Letterbox>.Record<ShapeStyle>(Lens<Letterbox, ShapeStyle>.New(static bars => bars.Style, static style => bars => bars with { Style = style })));
    public static readonly LetterboxParameter Layer = new("layer", new StateParameter<Letterbox>.Record<GeneratedLayer>(Lens<Letterbox, GeneratedLayer>.New(static bars => bars.Layer, static layer => bars => bars with { Layer = layer })));
    public static readonly LetterboxParameter Seed = new("seed", new StateParameter<Letterbox>.Bounded<Seed, int, InvalidGenerator>(Lens<Letterbox, Seed>.New(static bars => bars.Seed, static seed => bars => bars with { Seed = seed }), Generators.Seed.Presentation));

    public StateParameter<Letterbox> Kind { get; }
}

file sealed record GridLattice(float Height, float Span, float Shift, float Width, float Band, Vector3 Line) {
    public GridLattice(Grid state, PixelExtent extent)
        : this(state.Cell, state.Cell * state.Aspect, state.Offset * state.Cell * state.Aspect, state.LineWidth, ShapeEdge.Band(state.Feather, extent) / extent.ShortSide, state.LineColor.Display.AsVector3()) { }

    public Vector4 Lines(Vector2 point) => new(Line, ShapeEdge.Strip(Locate(point).Side, Width, Band));

    public Func<Vector2, Vector4> Cells(Func<int, int, Vector3> color) => point => Locate(point) switch {
        var (i, j, side, acrossI, acrossJ) => new Vector4(Vector3.Lerp(Vector3.Lerp(color(i, j), color(acrossI, acrossJ), ShapeEdge.Coverage(side, Band)), Line, ShapeEdge.Strip(side, Width, Band)), 1f),
    };

    private (int I, int J, float Side, int AcrossI, int AcrossJ) Locate(Vector2 point) {
        int j = (int)float.Floor(point.Y / Height);
        int i = Column(point.X, j);
        (float x, float y) = (point.X + Offset(j) - (i * Span), point.Y - (j * Height));
        float side = float.Min(float.Min(x, Span - x), float.Min(y, Height - y));
        return side == x ? (i, j, side, i - 1, j) : side == Span - x ? (i, j, side, i + 1, j) : side == y ? (i, j, side, Column(point.X, j - 1), j - 1) : (i, j, side, Column(point.X, j + 1), j + 1);
    }

    private int Column(float x, int j) => (int)float.Floor((x + Offset(j)) / Span);

    private float Offset(int j) => (j & 1) == 0 ? Shift : 0f;
}

file readonly record struct BoltPiece(Vector2 Start, Vector2 End, float From, float To) {
    public FieldSample Sample(Vector2 point) =>
        ShapeFields.Offset(point, Start, End) switch { var (offset, along) => float.Lerp(From, To, along) switch { var radius => new(offset.Length() - radius, 2f * radius) } };

    public static BoltPiece[] Path(Vector2 start, Vector2 direction, float length, int levels, Lightning bolt, Func<int, float> draw, Func<float, float> radius) {
        int segments = 1 << levels;
        float[] offsets = new float[segments + 1];
        for (int level = 1; level <= levels; level++)
            for (int step = segments >> level, k = step; k < segments; k += 2 * step)
                offsets[k] = ((offsets[k - step] + offsets[k + step]) / 2f) + (level <= bolt.Depth ? bolt.Displacement * (2f * step * length / segments) * ((2f * draw(k)) - 1f) : 0f);
        Vector2[] vertices = [.. offsets.Select((offset, k) => start + (k * length / segments * direction) + (offset * new Vector2(-direction.Y, direction.X)))];
        return [.. Enumerable.Range(0, segments).Select(k => new BoltPiece(vertices[k], vertices[k + 1], radius(k / (float)segments), radius((k + 1f) / segments)))];
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class ShapeEdge {
    public static float Band(ShortSideLength feather, PixelExtent extent, float scale = 1f) => float.Max(feather.Pixels(extent) * scale, 1f);

    public static float Coverage(float distance, float band) => Easing.Saturate(0.5f - (distance / band));

    public static float Strip(float distance, float width, float band) =>
        Coverage(distance - (width / 2f), band) - Coverage(distance + (width / 2f), band);
}

file static class ShapeFields {
    public static (float Level, float Slope) Level(Vector2 point, Vector2 extent, float exponent) {
        float length = point.Length();
        Vector2 toward = Vector2.Abs(length > 0f ? point / length : Vector2.UnitY);
        float level = float.Pow(float.Pow(toward.X / extent.X, exponent) + float.Pow(toward.Y / extent.Y, exponent), 1f / exponent);
        Vector2 normalized = toward / extent / level;
        return (length * level, new Vector2(float.Pow(normalized.X, exponent - 1f) / extent.X, float.Pow(normalized.Y, exponent - 1f) / extent.Y).Length());
    }

    public static float Ellipse(Vector2 point, Vector2 extent, float exponent) => Level(point, extent, exponent) switch { var (level, slope) => (level - 1f) / slope };

    public static float Box(Vector2 point, Vector2 extent, float radius) =>
        (Vector2.Abs(point) - extent + new Vector2(radius)) switch {
            var corner => Vector2.Max(corner, Vector2.Zero).Length() + float.Min(float.Max(corner.X, corner.Y), 0f) - radius,
        };

    public static (Vector2 Offset, float Along) Offset(Vector2 point, Vector2 start, Vector2 end) =>
        (end - start, point - start) switch {
            var (edge, reach) => Easing.Saturate(Vector2.Dot(reach, edge) / edge.LengthSquared()) switch { var along => (reach - (edge * along), along) },
        };

    public static float Side(Vector2 point, Vector2 start, Vector2 end) =>
        (end - start, point - start) switch { var (edge, reach) => (edge.X * reach.Y) - (edge.Y * reach.X) };
}

file sealed class RampFootprint {
    private const int Bins = 4096;

    public static readonly Func<(Ramp Ramp, Option<WaveProfile> Profile), RampFootprint> Of =
        memo(static ((Ramp Ramp, Option<WaveProfile> Profile) key) => new RampFootprint(key.Ramp.Tabulate(Gamut.StandardRgb), key.Profile));

    private readonly Arr<Vector4> bins;
    private readonly Arr<Vector4> sums;

    private RampFootprint(RampTable table, Option<WaveProfile> profile) {
        Vector4[] encoded = [.. Enumerable.Range(0, Bins).Select(k => table.Sample(profile.Match(Some: wave => wave.Level((k + 0.5f) / Bins), None: () => (k + 0.5f) / Bins)))];
        TransferCurve.Srgb.Encode(encoded, Nits.ReferenceWhite);
        bins = [.. encoded.Select(static color => new Vector4(color.AsVector3() * color.W, color.W))];
        sums = [.. toSeq(bins).Scan((X: 0d, Y: 0d, Z: 0d, W: 0d), static (sum, bin) => (sum.X + bin.X, sum.Y + bin.Y, sum.Z + bin.Z, sum.W + bin.W))
            .Map(static sum => new Vector4((float)sum.X, (float)sum.Y, (float)sum.Z, (float)sum.W))];
    }

    public Vector4 Mean(float position, float footprint, bool periodic) {
        double low = (position - (footprint / 2d)) * Bins;
        double start = periodic ? low - (Math.Floor(low / Bins) * Bins) : low;
        return (float.IsPositiveInfinity(footprint) ? sums[Bins] / Bins : (Integral(start + (footprint * Bins), periodic) - Integral(start, periodic)) / (footprint * Bins)) switch {
            var mean when mean.W > 0f => new Vector4(mean.AsVector3() / mean.W, mean.W),
            _ => Vector4.Zero,
        };
    }

    private Vector4 Integral(double at, bool periodic) =>
        (periodic ? Math.Floor(at / Bins) : 0d) switch {
            var periods => (at - (periods * Bins)) switch {
                var local => (int)double.Clamp(local, 0, Bins - 1) switch {
                    var index => ((float)periods * sums[Bins]) + sums[index] + ((float)(local - index) * bins[index]),
                },
            },
        };
}
