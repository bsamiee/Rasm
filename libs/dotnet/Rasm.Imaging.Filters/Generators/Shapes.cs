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
file static class ReferenceFrame {
    public const float Width = 1920f;
    public const float Height = 1080f;
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

[ValueObject<int>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Neutral", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidGenerator>]
public readonly partial struct BranchCount : IMinMaxValue<BranchCount> {
    public static BranchCount MinValue => Neutral;
    public static BranchCount MaxValue { get; } = new(4);
    public static BranchCount Standard { get; } = new(3);

    static partial void ValidateFactoryArguments(ref InvalidGenerator? validationError, ref int value) =>
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

[SmartEnum<string>]
[ValidationError<InvalidGenerator>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FillSide {
    public static readonly FillSide Inside = new("inside", 1f);
    public static readonly FillSide Outside = new("outside", -1f);

    public float Sign { get; }
}

[SmartEnum<string>]
[ValidationError<InvalidGenerator>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ShapeFit {
    public static readonly ShapeFit ShortSide = new("short-side", static _ => Vector2.One);
    public static readonly ShapeFit LongSide = new("long-side", static extent => new Vector2(int.Max(extent.Width, extent.Height)) / extent.ShortSide);

    public static readonly ShapeFit Frame = new("frame", static extent => new Vector2(extent.Width, extent.Height) / int.Min(extent.Width, extent.Height));

    [UseDelegateFromConstructor]
    public partial Vector2 Scale(PixelExtent extent);
}

[SmartEnum<string>]
[ValidationError<InvalidGenerator>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ShapeKind {
    public static readonly ShapeKind Ellipse = new("ellipse", ShapeFields.Ellipse);
    public static readonly ShapeKind Box = new("box", static (shape, half) =>
        (half, shape.Roundness * float.Min(half.X, half.Y)) switch {
            var (extent, radius) => point => ShapeFields.Box(point, extent, radius),
        });
    public static readonly ShapeKind Polygon = new("polygon", ShapeFields.Polygon);
    public static readonly ShapeKind Ring = new("ring", static (shape, half) =>
        (ShapeFields.Ellipse(shape, half), shape.Inset * float.Min(half.X, half.Y) / 2f) switch {
            var (ellipse, halfWidth) => point => float.Abs(ellipse(point) + halfWidth) - halfWidth,
        });
    public static readonly ShapeKind Cross = new("cross", static (shape, half) =>
        (half, (1f - shape.Inset) * float.Min(half.X, half.Y)) switch {
            var (extent, arm) => (shape.Roundness * arm) switch {
                var radius => point => float.Min(ShapeFields.Box(point, extent with { Y = arm }, radius), ShapeFields.Box(point, extent with { X = arm }, radius)),
            },
        });
    public static readonly ShapeKind Heart = new("heart", static (_, half) => (1.63f * half.X) switch {
        var scale => point => ShapeFields.Heart((point / scale) + new Vector2(0f, 0.55f)) * scale,
    });
    public static readonly ShapeKind Triangle = new("triangle", ShapeFields.Triangle);

    [UseDelegateFromConstructor]
    internal partial Func<Vector2, float> Field(ShapePrimitive primitive, Vector2 extent);
}

[SmartEnum<string>]
[ValidationError<InvalidGenerator>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ShapeOperation {
    public static readonly ShapeOperation Union = new("union", static (_, _) => float.Min);
    public static readonly ShapeOperation Difference = new("difference", static (_, _) => static (first, second) => float.Max(first, -second));
    public static readonly ShapeOperation Intersection = new("intersection", static (_, _) => float.Max);
    public static readonly ShapeOperation SmoothUnion = new("smooth-union", static (smoothing, _) => (first, second) =>
        Easing.Saturate(0.5f + (0.5f * (second - first) / smoothing)) switch {
            var h => float.Lerp(second, first, h) - (smoothing * h * (1f - h)),
        });
    public static readonly ShapeOperation Morph = new("morph", static (_, progress) => (first, second) => float.Lerp(first, second, progress));

    [UseDelegateFromConstructor]
    internal partial Func<float, float, float> Combine(ShortSideLength smoothing, Mix progress);
}

[SmartEnum<string>]
[ValidationError<InvalidGenerator>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GradientKind {
    public static readonly GradientKind Linear = new("linear", periodic: false, static extent => point => (0.5f + (point.X / (2f * extent.X)), 1f / (2f * extent.X)));
    public static readonly GradientKind Radial = new("radial", periodic: false, static extent => point => ShapeFields.Level(point, extent, 2f));
    public static readonly GradientKind Angular = new("angular", periodic: true, static _ => static point =>
        ((MathF.Atan2(point.Y, point.X) / float.Tau) + 0.5f, 1f / (float.Tau * point.Length())));
    public static readonly GradientKind Box = new("box", periodic: false, static extent => point =>
        (Vector2.Abs(point) / extent) switch { var reach => reach.X >= reach.Y ? (reach.X, 1f / extent.X) : (reach.Y, 1f / extent.Y) });

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
    ShapeKind Kind, Placement Placement, ShortSideExtent Width, ShortSideExtent Height, AxisFraction Roundness,
    SuperellipseExponent Exponent, SideCount Sides, AxisFraction Inset)
    : IStateRecord<ShapePrimitive, ShapePrimitiveParameter, InvalidGenerator> {
    public static ShapePrimitive Default { get; } = ShortSideExtent.Create(0.5f) switch {
        var extent => new(ShapeKind.Ellipse, Placement.Default, extent, extent, AxisFraction.MinValue,
            SuperellipseExponent.Ellipse, SideCount.Five, AxisFraction.MinValue),
    };
    public ShapeFit Fit { get; init; } = ShapeFit.ShortSide;

    internal (FieldFrame Frame, Func<Vector2, float> Distance) Field(PixelExtent extent) =>
        (Placement.Frame(extent), Kind.Field(this, new Vector2(Width, Height) * Fit.Scale(extent))) switch {
            var (frame, field) => (frame, point => frame.Local(point) switch {
                var local => field(new Vector2(local.X, -local.Y)),
            }),
        };
}

[SmartEnum<string>]
[ValidationError<InvalidGenerator>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ShapePrimitiveParameter : IStateParameter<ShapePrimitive> {
    private static readonly Presentation<ShortSideExtent, float> Extent = new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction), Soft = (0.01f, 2f), Scale = TrackScale.Log };
    private static readonly Presentation<AxisFraction, float> Share = new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction) };

    public static readonly ShapePrimitiveParameter Shape = new("kind", new StateParameter<ShapePrimitive>.Choice<ShapeKind, InvalidGenerator>(
        Lens<ShapePrimitive, ShapeKind>.New(static shape => shape.Kind, static kind => shape => shape with { Kind = kind })));
    public static readonly ShapePrimitiveParameter Placement = new("placement", new StateParameter<ShapePrimitive>.Record<Placement>(
        Lens<ShapePrimitive, Placement>.New(static shape => shape.Placement, static placement => shape => shape with { Placement = placement })));
    public static readonly ShapePrimitiveParameter Width = new("width", new StateParameter<ShapePrimitive>.Bounded<ShortSideExtent, float, InvalidPixelValue>(
        Lens<ShapePrimitive, ShortSideExtent>.New(static shape => shape.Width, static width => shape => shape with { Width = width }), Extent));
    public static readonly ShapePrimitiveParameter Height = new("height", new StateParameter<ShapePrimitive>.Bounded<ShortSideExtent, float, InvalidPixelValue>(
        Lens<ShapePrimitive, ShortSideExtent>.New(static shape => shape.Height, static height => shape => shape with { Height = height }), Extent));
    public static readonly ShapePrimitiveParameter Roundness = new("roundness", new StateParameter<ShapePrimitive>.Bounded<AxisFraction, float, InvalidGrade>(
        Lens<ShapePrimitive, AxisFraction>.New(static shape => shape.Roundness, static roundness => shape => shape with { Roundness = roundness }), Share));
    public static readonly ShapePrimitiveParameter Exponent = new("exponent", new StateParameter<ShapePrimitive>.Bounded<SuperellipseExponent, float, InvalidGenerator>(
        Lens<ShapePrimitive, SuperellipseExponent>.New(static shape => shape.Exponent, static exponent => shape => shape with { Exponent = exponent }),
        new() { Scale = TrackScale.Log, Origin = (float)SuperellipseExponent.Ellipse }));
    public static readonly ShapePrimitiveParameter Sides = new("sides", new StateParameter<ShapePrimitive>.Bounded<SideCount, int, InvalidGenerator>(
        Lens<ShapePrimitive, SideCount>.New(static shape => shape.Sides, static sides => shape => shape with { Sides = sides }), new() { Step = 1 }));
    public static readonly ShapePrimitiveParameter Inset = new("inset", new StateParameter<ShapePrimitive>.Bounded<AxisFraction, float, InvalidGrade>(
        Lens<ShapePrimitive, AxisFraction>.New(static shape => shape.Inset, static inset => shape => shape with { Inset = inset }), Share));
    public static readonly ShapePrimitiveParameter Fit = new("fit", new StateParameter<ShapePrimitive>.Choice<ShapeFit, InvalidGenerator>(
        Lens<ShapePrimitive, ShapeFit>.New(static shape => shape.Fit, static fit => shape => shape with { Fit = fit })));

    public StateParameter<ShapePrimitive> Kind { get; }
}

public sealed record ShapeStyle(Swatch Fill, Swatch Outline, ShortSideLength OutlineWidth, ShortSideLength Feather, ShortSideLength Roughness, FillSide Side)
    : IStateRecord<ShapeStyle, ShapeStyleParameter, InvalidGenerator> {
    public static ShapeStyle Default { get; } = new(Swatch.White, Swatch.Black, ShortSideLength.Neutral, ShortSideLength.Neutral, ShortSideLength.Neutral, FillSide.Inside);
    private static readonly Func<Vector4, uint, float> Gate = (NoiseBasis.Default with {
        Kind = BasisKind.RidgedMultifractal,
        Dimensions = NoiseDimensions.Four,
        Octaves = Octaves.Default with { Detail = FractalDetail.MaxValue, Roughness = AxisFraction.Create(0.9f) },
        Distortion = NoiseDistortion.Create(1.9f),
        FractalGain = Some(FractalGain.Create(74.5f)),
    }).Sampler;
    internal ShapeShading Shading(PixelExtent extent, uint field, Func<Swatch, Vector3> light) =>
        new(light(Fill), light(Outline), extent.ShortSide, OutlineWidth.Pixels(extent), ShapeEdge.Band(Feather, extent), Roughness.Pixels(extent),
            Side.Sign, Gate, field);
}

[SmartEnum<string>]
[ValidationError<InvalidGenerator>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ShapeStyleParameter : IStateParameter<ShapeStyle> {
    public static readonly ShapeStyleParameter Fill = new("fill", new StateParameter<ShapeStyle>.Color(
        Lens<ShapeStyle, Swatch>.New(static style => style.Fill, static value => style => style with { Fill = value })));
    public static readonly ShapeStyleParameter Outline = new("outline", new StateParameter<ShapeStyle>.Color(
        Lens<ShapeStyle, Swatch>.New(static style => style.Outline, static value => style => style with { Outline = value })));
    public static readonly ShapeStyleParameter OutlineWidth = new("outline-width", new StateParameter<ShapeStyle>.Bounded<ShortSideLength, float, InvalidPixelValue>(
        Lens<ShapeStyle, ShortSideLength>.New(static style => style.OutlineWidth, static value => style => style with { OutlineWidth = value }),
        new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction), Soft = (0f, 0.1f) }));
    public static readonly ShapeStyleParameter Feather = new("feather", new StateParameter<ShapeStyle>.Bounded<ShortSideLength, float, InvalidPixelValue>(
        Lens<ShapeStyle, ShortSideLength>.New(static style => style.Feather, static value => style => style with { Feather = value }),
        new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction), Soft = (0f, 0.25f) }));
    public static readonly ShapeStyleParameter Roughness = new("roughness", new StateParameter<ShapeStyle>.Bounded<ShortSideLength, float, InvalidPixelValue>(
        Lens<ShapeStyle, ShortSideLength>.New(static style => style.Roughness, static value => style => style with { Roughness = value }),
        new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction), Soft = (0f, 0.02f) }));
    public static readonly ShapeStyleParameter Side = new("side", new StateParameter<ShapeStyle>.Choice<FillSide, InvalidGenerator>(
        Lens<ShapeStyle, FillSide>.New(static style => style.Side, static side => style => style with { Side = side })));

    public StateParameter<ShapeStyle> Kind { get; }
}

internal readonly record struct ShapeShading(Vector3 Fill, Vector3 Outline, float Pixels, float Width, float Band, float Reach, float Sign, Func<Vector4, uint, float> Edge, uint Field) {
    public Vector4 Pixel(float distance, Vector2 local) {
        (float d, float band) = (Sign * distance * Pixels, Reach + (Band / 2f));
        float edge = float.Abs(d) <= band || float.Abs(d - Width) <= band
            ? d + (Reach * ((2f * Easing.Saturate(Edge(new Vector4(2.5f * new Vector2(local.X, -local.Y), 0f, 0f), Field))) - 1f))
            : d;
        float fill = ShapeEdge.Coverage(edge, Band);
        float outline = ShapeEdge.Coverage(edge - Width, Band) - fill;
        return (fill + outline) switch { var cover when cover > 0f => new Vector4(((fill * Fill) + (outline * Outline)) / cover, cover), _ => Vector4.Zero };
    }
}

public sealed record Shapes(
    ShapePrimitive First, Option<ShapeOperation> Operation, ShapePrimitive Second, ShortSideLength Smoothing, Mix Progress, ShapeStyle Style,
    GeneratedLayer Layer, Seed Seed)
    : IStateRecord<Shapes, ShapesParameter, InvalidGenerator>, IPixelStage<Shapes> {
    public static Shapes Default { get; } = new(
        ShapePrimitive.Default, None,
        ShapePrimitive.Default with { Kind = ShapeKind.Box, Roundness = AxisFraction.Create(0.3f / 2f / ShapePrimitive.Default.Width) },
        ShortSideLength.Neutral, Mix.MinValue, ShapeStyle.Default, GeneratedLayer.Default, Seed.MinValue);

    public static Shapes FilmGate { get; } = (Padding: 40f, Radius: 40f, Blur: 10f, Intensity: 6f) switch {
        var (padding, radius, blur, intensity) => Default with {
            First = Default.First with {
                Kind = ShapeKind.Box,
                Width = Valid.Value(ShortSideExtent.Validate(((ReferenceFrame.Width / 2f) - padding) / ReferenceFrame.Height, provider: null, out ShortSideExtent width), width),
                Height = Valid.Value(ShortSideExtent.Validate(((ReferenceFrame.Height / 2f) - padding) / ReferenceFrame.Height, provider: null, out ShortSideExtent height), height),
                Roundness = Valid.Value(AxisFraction.Validate(radius / ((ReferenceFrame.Height / 2f) - padding), provider: null, out AxisFraction roundness), roundness),
            },
            Style = new(Swatch.Black, Swatch.Black, ShortSideLength.Neutral,
                Valid.Value(ShortSideLength.Validate(blur / 3f * MathF.Sqrt(float.Tau) / ReferenceFrame.Height, provider: null, out ShortSideLength feather), feather),
                Valid.Value(ShortSideLength.Validate(intensity / ReferenceFrame.Height, provider: null, out ShortSideLength roughness), roughness),
                FillSide.Outside),
        },
    };

    public static Option<PixelPass> Pass(Shapes state, PassContext context) =>
        Some<PixelPass>(new PixelPass.Pointwise(state.Layer.Kernel(Fill(state, context.Extent))));

    public static PixelFrame Matte(Shapes state, PixelFrame frame) =>
        Fill(state, frame.Extent) switch {
            var fill => new(frame.Origin, frame.Size, frame.Extent, block => {
                Span<Vector4> pixels = MemoryMarshal.Cast<float, Vector4>(block.AsSpan());
                for (int i = 0; i < frame.Size.Height; i++)
                    fill(pixels.Slice(i * frame.Size.Width, frame.Size.Width), frame.Origin.X, frame.Line(i));
            }),
        };

    private static Action<Span<Vector4>, int, int> Fill(Shapes state, PixelExtent extent) =>
        (state.First.Field(extent), state.Style.Shading(extent, CoordinateHash.Field(NoiseStream.Shape, state.Seed, 0u), static swatch => swatch.Display.AsVector3())) switch {
            var (first, shading) => ShapeEdge.Draw((first.Frame, state.Operation.Match(
                Some: operation => (state.Second.Field(extent).Distance, operation.Combine(state.Smoothing, state.Progress)) switch {
                    var (second, combine) => point => combine(first.Distance(point), second(point)),
                },
                None: () => first.Distance)), (_, distance, local) => shading.Pixel(distance, local)),
        };
}

[SmartEnum<string>]
[ValidationError<InvalidGenerator>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ShapesParameter : IStateParameter<Shapes> {
    public static readonly ShapesParameter First = new("first", new StateParameter<Shapes>.Record<ShapePrimitive>(
        Lens<Shapes, ShapePrimitive>.New(static shape => shape.First, static first => shape => shape with { First = first })));
    public static readonly ShapesParameter Second = new("second", new StateParameter<Shapes>.Record<ShapePrimitive>(
        Lens<Shapes, ShapePrimitive>.New(static shape => shape.Second, static second => shape => shape with { Second = second })));
    public static readonly ShapesParameter Operation = new("operation", new StateParameter<Shapes>.OptionalChoice<ShapeOperation, InvalidGenerator>(
        Lens<Shapes, Option<ShapeOperation>>.New(static shape => shape.Operation, static operation => shape => shape with { Operation = operation })));
    public static readonly ShapesParameter Smoothing = new("smoothing", new StateParameter<Shapes>.Bounded<ShortSideLength, float, InvalidPixelValue>(
        Lens<Shapes, ShortSideLength>.New(static shape => shape.Smoothing, static smoothing => shape => shape with { Smoothing = smoothing }),
        new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction), Soft = (0f, 0.25f) }));
    public static readonly ShapesParameter Progress = new("progress", new StateParameter<Shapes>.Bounded<Mix, float, InvalidGrade>(
        Lens<Shapes, Mix>.New(static shape => shape.Progress, static progress => shape => shape with { Progress = progress }), new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction) }));
    public static readonly ShapesParameter Style = new("style", new StateParameter<Shapes>.Record<ShapeStyle>(
        Lens<Shapes, ShapeStyle>.New(static shape => shape.Style, static style => shape => shape with { Style = style })));
    public static readonly ShapesParameter Layer = new("layer", new StateParameter<Shapes>.Record<GeneratedLayer>(
        Lens<Shapes, GeneratedLayer>.New(static shape => shape.Layer, static layer => shape => shape with { Layer = layer })));
    public static readonly ShapesParameter Seed = new("seed", new StateParameter<Shapes>.Bounded<Seed, int, InvalidGenerator>(
        Lens<Shapes, Seed>.New(static shape => shape.Seed, static seed => shape => shape with { Seed = seed }), Generators.Seed.Presentation));

    public StateParameter<Shapes> Kind { get; }
}

public sealed record Lightning(
    NoiseBasis Basis, ShortSideExtent Wavelength, Placement Placement, ShortSideLength Length, ShortSideLength Thickness, ShortSideLength Amplitude,
    BranchCount Branches, ShortSideLength Reach, ShapeStyle Style, GeneratedLayer Layer, Seed Seed, Timing Timing, Hold Hold)
    : IStateRecord<Lightning, LightningParameter, InvalidGenerator>, IPixelStage<Lightning> {
    public static Lightning Default { get; } = new(
        NoiseBasis.Default with { Octaves = Octaves.Default with { Detail = FractalDetail.Create(4f), Roughness = AxisFraction.MaxValue }, Distortion = NoiseDistortion.Create(0.65f) },
        Valid.Value(ShortSideExtent.Validate(1f / 1.3f, provider: null, out ShortSideExtent wavelength), wavelength), Placement.Default,
        Valid.Value(ShortSideLength.Validate(1f / 2f, provider: null, out ShortSideLength length), length),
        Valid.Value(ShortSideLength.Validate(0.01f / (5f - (4.5f * 0.5f)), provider: null, out ShortSideLength thickness), thickness),
        Valid.Value(ShortSideLength.Validate(0.155f, provider: null, out ShortSideLength amplitude), amplitude), BranchCount.Standard,
        Valid.Value(ShortSideLength.Validate(0.7f, provider: null, out ShortSideLength reach), reach),
        new(Swatch.White, Swatch.White, ShortSideLength.Neutral, ShortSideLength.Neutral, ShortSideLength.Neutral, FillSide.Inside),
        GeneratedLayer.Added with { Exposure = Valid.Value(Exposure.Validate(3f, provider: null, out Exposure exposure), exposure) },
        Seed.MinValue, Timing.Standard, Hold.Bolt);

    public static Option<PixelPass> Pass(Lightning state, PassContext context) =>
        Some<PixelPass>(new PixelPass.Pointwise(state.Layer.Kernel(Fill(state, context))));

    private static Action<Span<Vector4>, int, int> Fill(Lightning state, PassContext context) {
        FieldFrame frame = state.Placement.Frame(context.Extent);
        uint field = CoordinateHash.Field(NoiseStream.Lightning, state.Seed, state.Hold.Period(state.Timing.At(context)));
        ShapeShading shading = state.Style.Shading(context.Extent, CoordinateHash.Branch(field, 3u), swatch => swatch.SceneLight(context.Working));
        (Func<Vector4, uint, float> basis, uint across, uint down) = (state.Basis.Sampler, CoordinateHash.Branch(field, 0u), CoordinateHash.Branch(field, 1u));
        (Vector2 top, Vector2 bottom) = (new(0f, state.Length), new(0f, -state.Length));
        (float wavelength, float amplitude, float thickness) = (state.Wavelength, state.Amplitude, state.Thickness);
        const float lead = 0.7f;
        (Vector2 Root, Vector2 Tip)[] arms = [.. Seq<(float Root, float Limit)>((1f / 2f, lead), (1f / 4f, 0.53f), (3f / 4f, 0.27f), (5f / 8f, 0.7f)).Take(state.Branches)
            .Map((branch, k) => Vector2.Lerp(top, bottom, branch.Root) switch {
                var root => (root, root + (state.Reach * branch.Limit / lead
                    * ((2f * NoiseFunctions.White(new Vector4(k, 0f, 0f, 0f), CoordinateHash.Branch(field, 2u)).AsVector2()) - Vector2.One))),
            })];
        return (row, column, line) => {
            for (int i = 0; i < row.Length; i++) {
                Vector2 local = frame.Local(frame.Point(column + i, line));
                Vector2 p = new(local.X, -local.Y);
                Vector4 at = new(p / wavelength, 0f, 0f);
                Vector2 bent = p + (amplitude * new Vector2((2f * basis(at, across)) - 1f, (2f * basis(at, down)) - 1f));
                float distance = Segment.Offset(bent, top, bottom).Offset.Length() - thickness;
                foreach ((Vector2 root, Vector2 tip) in arms)
                    distance = Segment.Offset(bent, root, tip) switch { var (offset, along) => float.Min(distance, offset.Length() - (thickness * (1f - along))) };
                row[i] = shading.Pixel(distance, local);
            }
        };
    }
}

[SmartEnum<string>]
[ValidationError<InvalidGenerator>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LightningParameter : IStateParameter<Lightning> {
    private static readonly (StateParameter<Lightning> Clock, StateParameter<Lightning> Pace) Time =
        Timing.Kinds(Lens<Lightning, Timing>.New(static state => state.Timing, static timing => state => state with { Timing = timing }));

    public static readonly LightningParameter Basis = new("basis", new StateParameter<Lightning>.Record<NoiseBasis>(
        Lens<Lightning, NoiseBasis>.New(static state => state.Basis, static basis => state => state with { Basis = basis })));
    public static readonly LightningParameter Wavelength = new("wavelength", new StateParameter<Lightning>.Bounded<ShortSideExtent, float, InvalidPixelValue>(
        Lens<Lightning, ShortSideExtent>.New(static bolt => bolt.Wavelength, static wavelength => bolt => bolt with { Wavelength = wavelength }),
        new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction), Soft = (0.01f, 2f), Scale = TrackScale.Log }));
    public static readonly LightningParameter Placement = new("placement", new StateParameter<Lightning>.Record<Placement>(
        Lens<Lightning, Placement>.New(static bolt => bolt.Placement, static placement => bolt => bolt with { Placement = placement })));
    public static readonly LightningParameter Length = new("length", new StateParameter<Lightning>.Bounded<ShortSideLength, float, InvalidPixelValue>(
        Lens<Lightning, ShortSideLength>.New(static bolt => bolt.Length, static length => bolt => bolt with { Length = length }),
        new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction), Soft = (0f, 1f) }));
    public static readonly LightningParameter Thickness = new("thickness", new StateParameter<Lightning>.Bounded<ShortSideLength, float, InvalidPixelValue>(
        Lens<Lightning, ShortSideLength>.New(static bolt => bolt.Thickness, static thickness => bolt => bolt with { Thickness = thickness }),
        new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction), Soft = (0f, 0.02f) }));
    public static readonly LightningParameter Amplitude = new("amplitude", new StateParameter<Lightning>.Bounded<ShortSideLength, float, InvalidPixelValue>(
        Lens<Lightning, ShortSideLength>.New(static bolt => bolt.Amplitude, static amplitude => bolt => bolt with { Amplitude = amplitude }),
        new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction), Soft = (0f, 0.5f) }));
    public static readonly LightningParameter Branches = new("branches", new StateParameter<Lightning>.Bounded<BranchCount, int, InvalidGenerator>(
        Lens<Lightning, BranchCount>.New(static bolt => bolt.Branches, static branches => bolt => bolt with { Branches = branches }), new() { Step = 1 }));
    public static readonly LightningParameter Reach = new("reach", new StateParameter<Lightning>.Bounded<ShortSideLength, float, InvalidPixelValue>(
        Lens<Lightning, ShortSideLength>.New(static bolt => bolt.Reach, static reach => bolt => bolt with { Reach = reach }),
        new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction), Soft = (0f, 1f) }));
    public static readonly LightningParameter Style = new("style", new StateParameter<Lightning>.Record<ShapeStyle>(
        Lens<Lightning, ShapeStyle>.New(static bolt => bolt.Style, static style => bolt => bolt with { Style = style })));
    public static readonly LightningParameter Layer = new("layer", new StateParameter<Lightning>.Record<GeneratedLayer>(
        Lens<Lightning, GeneratedLayer>.New(static bolt => bolt.Layer, static layer => bolt => bolt with { Layer = layer })));
    public static readonly LightningParameter Seed = new("seed", new StateParameter<Lightning>.Bounded<Seed, int, InvalidGenerator>(
        Lens<Lightning, Seed>.New(static bolt => bolt.Seed, static seed => bolt => bolt with { Seed = seed }), Generators.Seed.Presentation));
    public static readonly LightningParameter Clock = new("clock", Time.Clock);
    public static readonly LightningParameter Pace = new("pace", Time.Pace);
    public static readonly LightningParameter Hold = new("hold", new StateParameter<Lightning>.Bounded<Hold, float, InvalidGenerator>(
        Lens<Lightning, Hold>.New(static bolt => bolt.Hold, static hold => bolt => bolt with { Hold = hold }),
        Generators.Hold.Presentation));

    public StateParameter<Lightning> Kind { get; }
}

public sealed record Gradient(GradientKind Kind, Placement Placement, ShortSideExtent Width, ShortSideExtent Height, Ramp Colors, GeneratedLayer Layer)
    : IStateRecord<Gradient, GradientParameter, InvalidGenerator>, IPixelStage<Gradient> {
    public static Gradient Default { get; } = Valid.Value(ShortSideExtent.Validate(1f / 2f, provider: null, out ShortSideExtent half), half) switch {
        var extent => new(GradientKind.Linear, Placement.Default, extent, extent, Ramp.Grayscale, GeneratedLayer.Default),
    };

    public static Option<PixelPass> Pass(Gradient state, PassContext context) =>
        (state.Placement.Frame(context.Extent), state.Kind.Parameter(new Vector2(state.Width, state.Height)),
            RampFootprint.Of((state.Colors, None)), (float)int.Min(context.Extent.Width, context.Extent.Height)) switch {
                var (frame, parameter, footprint, pixels) => Some<PixelPass>(new PixelPass.Pointwise(state.Layer.Kernel((row, column, line) => {
                    for (int i = 0; i < row.Length; i++) {
                        Vector2 local = frame.Local(frame.Point(column + i, line));
                        row[i] = parameter(new Vector2(local.X, -local.Y)) switch {
                            var (position, slope) => footprint.Mean(position, slope / pixels, state.Kind.Periodic),
                        };
                    }
                }))),
            };
}

[SmartEnum<string>]
[ValidationError<InvalidGenerator>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GradientParameter : IStateParameter<Gradient> {
    private static readonly Presentation<ShortSideExtent, float> Extent = new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction), Soft = (0.01f, 4f), Scale = TrackScale.Log };

    public static readonly GradientParameter Kind = new("kind", new StateParameter<Gradient>.Choice<GradientKind, InvalidGenerator>(
        Lens<Gradient, GradientKind>.New(static gradient => gradient.Kind, static kind => gradient => gradient with { Kind = kind })));
    public static readonly GradientParameter Placement = new("placement", new StateParameter<Gradient>.Record<Placement>(
        Lens<Gradient, Placement>.New(static gradient => gradient.Placement, static placement => gradient => gradient with { Placement = placement })));
    public static readonly GradientParameter Width = new("width", new StateParameter<Gradient>.Bounded<ShortSideExtent, float, InvalidPixelValue>(
        Lens<Gradient, ShortSideExtent>.New(static gradient => gradient.Width, static width => gradient => gradient with { Width = width }), Extent));
    public static readonly GradientParameter Height = new("height", new StateParameter<Gradient>.Bounded<ShortSideExtent, float, InvalidPixelValue>(
        Lens<Gradient, ShortSideExtent>.New(static gradient => gradient.Height, static height => gradient => gradient with { Height = height }), Extent));
    public static readonly GradientParameter Colors = new("colors", new StateParameter<Gradient>.Gradient(
        Lens<Gradient, Ramp>.New(static gradient => gradient.Colors, static colors => gradient => gradient with { Colors = colors })));
    public static readonly GradientParameter Layer = new("layer", new StateParameter<Gradient>.Record<GeneratedLayer>(
        Lens<Gradient, GeneratedLayer>.New(static gradient => gradient.Layer, static layer => gradient => gradient with { Layer = layer })));

    StateParameter<Gradient> IStateParameter<Gradient>.Kind => Parameter;

    private StateParameter<Gradient> Parameter { get; }
}

public sealed record CornerGradient(Swatch TopLeft, Swatch TopRight, Swatch BottomRight, Swatch BottomLeft, GeneratedLayer Layer)
    : IStateRecord<CornerGradient, CornerGradientParameter, InvalidGenerator>, IPixelStage<CornerGradient> {
    public static CornerGradient Default { get; } = new(Swatch.White, Swatch.Black, Swatch.White, Swatch.Black, GeneratedLayer.Default);

    public static Option<PixelPass> Pass(CornerGradient state, PassContext context) =>
        Some<PixelPass>(new PixelPass.Pointwise(state.Layer.Kernel(Fill(state, context.Extent))));

    private static Action<Span<Vector4>, int, int> Fill(CornerGradient state, PixelExtent extent) {
        (Vector3 topLeft, Vector3 topRight, Vector3 bottomRight, Vector3 bottomLeft) =
            (state.TopLeft.Display.AsVector3(), state.TopRight.Display.AsVector3(), state.BottomRight.Display.AsVector3(), state.BottomLeft.Display.AsVector3());
        (float width, float height) = (extent.Width, extent.Height);
        return (row, column, line) => {
            float down = Easing.SmootherStep((height - line - 0.5f) / height);
            (Vector3 top, Vector3 bottom) = ((1f - down) * topLeft, down * bottomLeft);
            (Vector3 topSpan, Vector3 bottomSpan) = (((1f - down) * topRight) - top, (down * bottomRight) - bottom);
            for (int i = 0; i < row.Length; i++)
                row[i] = new Vector4(top + bottom + (Easing.SmootherStep((column + i + 0.5f) / width) * (topSpan + bottomSpan)), 1f);
        };
    }
}

[SmartEnum<string>]
[ValidationError<InvalidGenerator>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CornerGradientParameter : IStateParameter<CornerGradient> {
    public static readonly CornerGradientParameter TopLeft = new("top-left", new StateParameter<CornerGradient>.Color(
        Lens<CornerGradient, Swatch>.New(static corners => corners.TopLeft, static color => corners => corners with { TopLeft = color })));
    public static readonly CornerGradientParameter TopRight = new("top-right", new StateParameter<CornerGradient>.Color(
        Lens<CornerGradient, Swatch>.New(static corners => corners.TopRight, static color => corners => corners with { TopRight = color })));
    public static readonly CornerGradientParameter BottomRight = new("bottom-right", new StateParameter<CornerGradient>.Color(
        Lens<CornerGradient, Swatch>.New(static corners => corners.BottomRight, static color => corners => corners with { BottomRight = color })));
    public static readonly CornerGradientParameter BottomLeft = new("bottom-left", new StateParameter<CornerGradient>.Color(
        Lens<CornerGradient, Swatch>.New(static corners => corners.BottomLeft, static color => corners => corners with { BottomLeft = color })));
    public static readonly CornerGradientParameter Layer = new("layer", new StateParameter<CornerGradient>.Record<GeneratedLayer>(
        Lens<CornerGradient, GeneratedLayer>.New(static corners => corners.Layer, static layer => corners => corners with { Layer = layer })));

    public StateParameter<CornerGradient> Kind { get; }
}

public sealed record Grid(
    GridPattern Pattern, Placement Placement, ShortSideExtent Cell, AspectRatio Aspect, AxisFraction Offset, ShortSideLength LineWidth,
    ShortSideLength Feather, Swatch LineColor, Swatch CellColor, Swatch AlternateColor, GeneratedLayer Layer, Seed Seed)
    : IStateRecord<Grid, GridParameter, InvalidGenerator>, IPixelStage<Grid> {
    public static Grid Default { get; } = (Size: 3f, Thickness: 0.2f, Factor: 0.1f) switch {
        var (size, thickness, factor) => new(
            GridPattern.Lines, Placement.Default, Valid.Value(ShortSideExtent.Validate(1f / size, provider: null, out ShortSideExtent cell), cell), AspectRatio.Square, AxisFraction.Half,
            Valid.Value(ShortSideLength.Validate(2f * (0.02f + (0.18f * thickness)) * (0.1f + (0.4f * factor)) / size, provider: null, out ShortSideLength width), width),
            ShortSideLength.Neutral, Swatch.White, Swatch.Black, Swatch.White, GeneratedLayer.Default, Seed.MinValue),
    };

    public static Option<PixelPass> Pass(Grid state, PassContext context) =>
        Some<PixelPass>(new PixelPass.Pointwise(state.Layer.Kernel(state.Pattern.Switch(
            (Grid: state, context.Extent),
            lines: static at => GridLattice.Of(at.Grid, at.Extent, AxisFraction.MinValue).Lines(),
            checker: static at => (at.Grid.CellColor.Display.AsVector3(), at.Grid.AlternateColor.Display.AsVector3()) switch {
                var (cell, alternate) => GridLattice.Of(at.Grid, at.Extent, AxisFraction.MinValue).Cells((i, j) => ((i + j) & 1) == 0 ? cell : alternate),
            },
            brick: static at => (at.Grid.CellColor.Display.AsVector3(), at.Grid.AlternateColor.Display.AsVector3(), CoordinateHash.Field(NoiseStream.Grid, at.Grid.Seed, 0u)) switch {
                var (cell, alternate, field) => GridLattice.Of(at.Grid, at.Extent, at.Grid.Offset)
                    .Cells((i, j) => Vector3.Lerp(cell, alternate, NoiseFunctions.White(new Vector4(i, j, 0f, 0f), field).X)),
            }))));
}

[SmartEnum<string>]
[ValidationError<InvalidGenerator>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GridParameter : IStateParameter<Grid> {
    private static readonly Presentation<ShortSideLength, float> Line = new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction), Soft = (0f, 0.05f) };

    public static readonly GridParameter Pattern = new("pattern", new StateParameter<Grid>.Choice<GridPattern, InvalidGenerator>(
        Lens<Grid, GridPattern>.New(static grid => grid.Pattern, static pattern => grid => grid with { Pattern = pattern })));
    public static readonly GridParameter Placement = new("placement", new StateParameter<Grid>.Record<Placement>(
        Lens<Grid, Placement>.New(static grid => grid.Placement, static placement => grid => grid with { Placement = placement })));
    public static readonly GridParameter Cell = new("cell", new StateParameter<Grid>.Bounded<ShortSideExtent, float, InvalidPixelValue>(
        Lens<Grid, ShortSideExtent>.New(static grid => grid.Cell, static cell => grid => grid with { Cell = cell }),
        new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction), Soft = (0.01f, 1f), Scale = TrackScale.Log }));
    public static readonly GridParameter Aspect = new("aspect", new StateParameter<Grid>.Bounded<AspectRatio, float, InvalidGenerator>(
        Lens<Grid, AspectRatio>.New(static grid => grid.Aspect, static aspect => grid => grid with { Aspect = aspect }), AspectRatio.Presentation));
    public static readonly GridParameter Offset = new("offset", new StateParameter<Grid>.Bounded<AxisFraction, float, InvalidGrade>(
        Lens<Grid, AxisFraction>.New(static grid => grid.Offset, static offset => grid => grid with { Offset = offset }), new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction) }));
    public static readonly GridParameter LineWidth = new("line-width", new StateParameter<Grid>.Bounded<ShortSideLength, float, InvalidPixelValue>(
        Lens<Grid, ShortSideLength>.New(static grid => grid.LineWidth, static width => grid => grid with { LineWidth = width }), Line));
    public static readonly GridParameter Feather = new("feather", new StateParameter<Grid>.Bounded<ShortSideLength, float, InvalidPixelValue>(
        Lens<Grid, ShortSideLength>.New(static grid => grid.Feather, static feather => grid => grid with { Feather = feather }), Line));
    public static readonly GridParameter LineColor = new("line-color", new StateParameter<Grid>.Color(
        Lens<Grid, Swatch>.New(static grid => grid.LineColor, static color => grid => grid with { LineColor = color })));
    public static readonly GridParameter CellColor = new("cell-color", new StateParameter<Grid>.Color(
        Lens<Grid, Swatch>.New(static grid => grid.CellColor, static color => grid => grid with { CellColor = color })));
    public static readonly GridParameter AlternateColor = new("alternate-color", new StateParameter<Grid>.Color(
        Lens<Grid, Swatch>.New(static grid => grid.AlternateColor, static color => grid => grid with { AlternateColor = color })));
    public static readonly GridParameter Layer = new("layer", new StateParameter<Grid>.Record<GeneratedLayer>(
        Lens<Grid, GeneratedLayer>.New(static grid => grid.Layer, static layer => grid => grid with { Layer = layer })));
    public static readonly GridParameter Seed = new("seed", new StateParameter<Grid>.Bounded<Seed, int, InvalidGenerator>(
        Lens<Grid, Seed>.New(static grid => grid.Seed, static seed => grid => grid with { Seed = seed }), Generators.Seed.Presentation));

    public StateParameter<Grid> Kind { get; }
}

public sealed record Stripes(
    WaveProfile Profile, Placement Placement, ShortSideExtent Wavelength, SignedAngle Bend, AxisFraction Phase, Frequency Speed, Ramp Colors,
    GeneratedLayer Layer, Timing Timing)
    : IStateRecord<Stripes, StripesParameter, InvalidGenerator>, IPixelStage<Stripes> {
    public static Stripes Default { get; } = new(
        WaveProfile.Sine, Placement.Default, Valid.Value(ShortSideExtent.Validate(float.Tau / 20f, provider: null, out ShortSideExtent wavelength), wavelength),
        SignedAngle.Neutral, AxisFraction.MinValue, Frequency.Neutral, Ramp.Grayscale, GeneratedLayer.Default, Timing.Standard);

    public static Option<PixelPass> Pass(Stripes state, PassContext context) =>
        Some<PixelPass>(new PixelPass.Pointwise(state.Layer.Kernel(Fill(state, context))));

    private static Action<Span<Vector4>, int, int> Fill(Stripes state, PassContext context) {
        FieldFrame frame = state.Placement.Frame(context.Extent);
        RampFootprint footprint = RampFootprint.Of((state.Colors, Some(state.Profile)));
        (float sin, float cos) = MathF.SinCos(state.Bend);
        (float wavelength, float start) = (state.Wavelength, state.Phase + (state.Speed * state.Timing.At(context)));
        float span = 1f / (wavelength * context.Extent.ShortSide);
        return (row, column, line) => {
            for (int i = 0; i < row.Length; i++) {
                Vector2 local = frame.Local(frame.Point(column + i, line));
                row[i] = footprint.Mean((((-local.Y * cos) + (float.Abs(local.X) * sin)) / wavelength) + start, span, periodic: true);
            }
        };
    }
}

[SmartEnum<string>]
[ValidationError<InvalidGenerator>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class StripesParameter : IStateParameter<Stripes> {
    private static readonly (StateParameter<Stripes> Clock, StateParameter<Stripes> Pace) Time =
        Timing.Kinds(Lens<Stripes, Timing>.New(static state => state.Timing, static timing => state => state with { Timing = timing }));

    public static readonly StripesParameter Profile = new("profile", new StateParameter<Stripes>.Choice<WaveProfile, InvalidGenerator>(
        Lens<Stripes, WaveProfile>.New(static stripes => stripes.Profile, static profile => stripes => stripes with { Profile = profile })));
    public static readonly StripesParameter Placement = new("placement", new StateParameter<Stripes>.Record<Placement>(
        Lens<Stripes, Placement>.New(static stripes => stripes.Placement, static placement => stripes => stripes with { Placement = placement })));
    public static readonly StripesParameter Wavelength = new("wavelength", new StateParameter<Stripes>.Bounded<ShortSideExtent, float, InvalidPixelValue>(
        Lens<Stripes, ShortSideExtent>.New(static stripes => stripes.Wavelength, static wavelength => stripes => stripes with { Wavelength = wavelength }),
        new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction), Soft = (0.01f, 1f), Scale = TrackScale.Log }));
    public static readonly StripesParameter Bend = new("bend", new StateParameter<Stripes>.Bounded<SignedAngle, float, InvalidPixelValue>(
        Lens<Stripes, SignedAngle>.New(static stripes => stripes.Bend, static bend => stripes => stripes with { Bend = bend }),
        new() { Unit = Quantity.GetUnitInfo(AngleUnit.Radian), Origin = (float)SignedAngle.Neutral }));
    public static readonly StripesParameter Phase = new("phase", new StateParameter<Stripes>.Bounded<AxisFraction, float, InvalidGrade>(
        Lens<Stripes, AxisFraction>.New(static stripes => stripes.Phase, static phase => stripes => stripes with { Phase = phase }), new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction) }));
    public static readonly StripesParameter Speed = new("speed", new StateParameter<Stripes>.Bounded<Frequency, float, InvalidGenerator>(
        Lens<Stripes, Frequency>.New(static stripes => stripes.Speed, static speed => stripes => stripes with { Speed = speed }),
        Frequency.Presentation with { Soft = (-4f, 4f) }));
    public static readonly StripesParameter Colors = new("colors", new StateParameter<Stripes>.Gradient(
        Lens<Stripes, Ramp>.New(static stripes => stripes.Colors, static colors => stripes => stripes with { Colors = colors })));
    public static readonly StripesParameter Layer = new("layer", new StateParameter<Stripes>.Record<GeneratedLayer>(
        Lens<Stripes, GeneratedLayer>.New(static stripes => stripes.Layer, static layer => stripes => stripes with { Layer = layer })));
    public static readonly StripesParameter Clock = new("clock", Time.Clock);
    public static readonly StripesParameter Pace = new("pace", Time.Pace);

    public StateParameter<Stripes> Kind { get; }
}

public sealed record Letterbox(AspectRatio Aspect, Swatch Color, GeneratedLayer Layer)
    : IStateRecord<Letterbox, LetterboxParameter, InvalidGenerator>, IPixelStage<Letterbox> {
    public static Letterbox Default { get; } = new(AspectRatio.Scope, Swatch.Black, GeneratedLayer.Default);

    public static Option<PixelPass> Pass(Letterbox state, PassContext context) =>
        Some<PixelPass>(new PixelPass.Pointwise(state.Layer.Kernel(Fill(state, context.Extent))));

    private static Action<Span<Vector4>, int, int> Fill(Letterbox state, PixelExtent extent) {
        FieldFrame frame = Placement.Default.Frame(extent);
        (float width, float height, float aspect, float pixels) = (extent.Width, extent.Height, state.Aspect, extent.ShortSide);
        Vector2 half = (aspect >= width / height ? new Vector2(width, width / aspect) : new Vector2(height * aspect, height)) / (2f * pixels);
        (Vector3 color, float band) = (state.Color.Display.AsVector3(), ShapeEdge.Band(ShortSideLength.Neutral, extent));
        return (row, column, line) => {
            for (int i = 0; i < row.Length; i++)
                row[i] = new Vector4(color, ShapeEdge.Coverage(FillSide.Outside.Sign * ShapeFields.Box(frame.Local(frame.Point(column + i, line)), half, 0f) * pixels, band));
        };
    }
}

[SmartEnum<string>]
[ValidationError<InvalidGenerator>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LetterboxParameter : IStateParameter<Letterbox> {
    public static readonly LetterboxParameter Aspect = new("aspect", new StateParameter<Letterbox>.Bounded<AspectRatio, float, InvalidGenerator>(
        Lens<Letterbox, AspectRatio>.New(static bars => bars.Aspect, static aspect => bars => bars with { Aspect = aspect }), AspectRatio.Presentation));
    public static readonly LetterboxParameter Color = new("color", new StateParameter<Letterbox>.Color(
        Lens<Letterbox, Swatch>.New(static bars => bars.Color, static color => bars => bars with { Color = color })));
    public static readonly LetterboxParameter Layer = new("layer", new StateParameter<Letterbox>.Record<GeneratedLayer>(
        Lens<Letterbox, GeneratedLayer>.New(static bars => bars.Layer, static layer => bars => bars with { Layer = layer })));

    public StateParameter<Letterbox> Kind { get; }
}

file readonly record struct GridLattice(FieldFrame Frame, float Height, float Span, float Shift, float Pixels, float Width, float Band, Vector3 Line) {
    public static GridLattice Of(Grid state, PixelExtent extent, AxisFraction offset) =>
        new(state.Placement.Frame(extent), state.Cell, state.Cell * state.Aspect, offset * state.Cell * state.Aspect, extent.ShortSide,
            state.LineWidth.Pixels(extent), ShapeEdge.Band(state.Feather, extent), state.LineColor.Display.AsVector3());

    public Action<Span<Vector4>, int, int> Lines() {
        GridLattice lattice = this;
        return (row, column, line) => {
            for (int i = 0; i < row.Length; i++)
                row[i] = new Vector4(lattice.Line, ShapeEdge.Strip(lattice.Locate(column + i, line).Side, lattice.Width, lattice.Band));
        };
    }

    public Action<Span<Vector4>, int, int> Cells(Func<int, int, Vector3> color) {
        GridLattice lattice = this;
        return (row, column, line) => {
            for (int i = 0; i < row.Length; i++) {
                (int across, int up, float side, int nextAcross, int nextUp) = lattice.Locate(column + i, line);
                Vector3 under = Vector3.Lerp(color(across, up), color(nextAcross, nextUp), ShapeEdge.Coverage(side, lattice.Band));
                row[i] = new Vector4(Vector3.Lerp(under, lattice.Line, ShapeEdge.Strip(side, lattice.Width, lattice.Band)), 1f);
            }
        };
    }

    private (int I, int J, float Side, int AcrossI, int AcrossJ) Locate(int column, int line) {
        Vector2 local = Frame.Local(Frame.Point(column, line));
        Vector2 point = new(local.X, -local.Y);
        int j = (int)MathF.Floor(point.Y / Height);
        int i = Column(point.X, j);
        (float x, float y) = (point.X + Offset(j) - (i * Span), point.Y - (j * Height));
        float side = float.Min(float.Min(x, Span - x), float.Min(y, Height - y));
        return side == x ? (i, j, side * Pixels, i - 1, j)
            : side == Span - x ? (i, j, side * Pixels, i + 1, j)
            : side == y ? (i, j, side * Pixels, Column(point.X, j - 1), j - 1)
            : (i, j, side * Pixels, Column(point.X, j + 1), j + 1);
    }

    private int Column(float x, int j) => (int)MathF.Floor((x + Offset(j)) / Span);

    private float Offset(int j) => (j & 1) == 0 ? Shift : 0f;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class ShapeEdge {
    public static Action<Span<Vector4>, int, int> Draw(
        (FieldFrame Frame, Func<Vector2, float> Distance) field, Func<Vector4, float, Vector2, Vector4> shade) =>
        (row, column, line) => {
            for (int i = 0; i < row.Length; i++) {
                Vector2 point = field.Frame.Point(column + i, line);
                row[i] = shade(row[i], field.Distance(point), field.Frame.Local(point));
            }
        };

    public static float Band(ShortSideLength feather, PixelExtent extent, float scale = 1f) => float.Max(feather.Pixels(extent) * scale, 1f);

    public static float Coverage(float distance, float band) => Easing.Saturate(0.5f - (distance / band));

    public static float Strip(float distance, float width, float band) =>
        Coverage(distance - (width / 2f), band) - Coverage(distance + (width / 2f), band);
}

file static class ShapeFields {
    public static Func<Vector2, float> Ellipse(ShapePrimitive shape, Vector2 extent) =>
        (extent, (float)shape.Exponent) switch {
            var (half, exponent) => point => Level(point, half, exponent) switch { var (level, slope) => (level - 1f) / slope },
        };

    public static (float Level, float Slope) Level(Vector2 point, Vector2 extent, float exponent) {
        float length = point.Length();
        Vector2 toward = Vector2.Abs(length > 0f ? point / length : Vector2.UnitY);
        float level = MathF.Pow(MathF.Pow(toward.X / extent.X, exponent) + MathF.Pow(toward.Y / extent.Y, exponent), 1f / exponent);
        Vector2 normalized = toward / extent / level;
        Vector2 slope = new(MathF.Pow(normalized.X, exponent - 1f) / extent.X, MathF.Pow(normalized.Y, exponent - 1f) / extent.Y);
        return (length * level, slope.Length());
    }

    public static float Box(Vector2 point, Vector2 extent, float radius) =>
        (Vector2.Abs(point) - extent + new Vector2(radius)) switch {
            var corner => Vector2.Max(corner, Vector2.Zero).Length() + float.Min(float.Max(corner.X, corner.Y), 0f) - radius,
        };

    public static Func<Vector2, float> Polygon(ShapePrimitive shape, Vector2 extent) {
        (float wedge, float radius) = (float.Pi / shape.Sides, shape.Roundness * extent.X);
        (float sine, float cosine) = MathF.SinCos(wedge);
        float reach = extent.X - radius;
        (Vector2 outer, Vector2 inner) = (new(0f, reach), reach * cosine * (1f - shape.Inset) * new Vector2(sine, cosine));
        return point => {
            float turn = MathF.Atan2(point.X, point.Y) + wedge;
            (float sin, float cos) = MathF.SinCos(float.Abs(turn - (2f * wedge * MathF.Floor(turn / (2f * wedge))) - wedge));
            Vector2 folded = point.Length() * new Vector2(sin, cos);
            float distance = Segment.Offset(folded, outer, inner).Offset.Length();
            return (Segment.Side(folded, outer, inner) < 0f ? -distance : distance) - radius;
        };
    }

    public static float Heart(Vector2 point) => new Vector2(float.Abs(point.X), point.Y) switch {
        var q when q.X + q.Y > 1f => Vector2.Distance(q, new Vector2(0.25f, 0.75f)) - (MathF.Sqrt(2f) / 4f),
        var q => MathF.Sqrt(float.Min(Vector2.DistanceSquared(q, Vector2.UnitY), Vector2.DistanceSquared(q, 0.5f * float.Max(q.X + q.Y, 0f) * Vector2.One)))
            * float.Sign(q.X - q.Y),
    };

    public static Func<Vector2, float> Triangle(ShapePrimitive shape, Vector2 extent) =>
        (extent, shape.Roundness * float.Min(extent.X, extent.Y)) switch {
            var (size, radius) => (size - new Vector2(radius)) switch {
                var half => (new Vector2(-half.X, -half.Y), new Vector2(half.X, -half.Y), new Vector2(0f, half.Y)) switch {
                    var (a, b, c) => point => Vector2.Min(Vector2.Min(Edge(point, a, b), Edge(point, b, c)), Edge(point, c, a)) switch {
                        var nearest => (MathF.Sqrt(nearest.X) * (nearest.Y > 0f ? -1f : 1f)) - radius,
                    },
                },
            },
        };

    private static Vector2 Edge(Vector2 point, Vector2 start, Vector2 end) =>
        new(Segment.Offset(point, start, end).Offset.LengthSquared(), Segment.Side(point, start, end));
}

file static class Segment {
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
        Func<float, float> position = profile.Match(Some: static wave => (Func<float, float>)(at => wave.Level(float.Tau * at)), None: static () => static at => at);
        Vector4[] encoded = [.. Enumerable.Range(0, Bins).Select(k => table.Sample(position((k + 0.5f) / Bins)))];
        TransferCurve.Srgb.Encode(encoded, Nits.ReferenceWhite);
        bins = [.. encoded.Select(static color => new Vector4(color.AsVector3() * color.W, color.W))];
        sums = [.. toSeq(bins).Scan((X: 0d, Y: 0d, Z: 0d, W: 0d), static (sum, bin) => (sum.X + bin.X, sum.Y + bin.Y, sum.Z + bin.Z, sum.W + bin.W))
            .Map(static sum => new Vector4((float)sum.X, (float)sum.Y, (float)sum.Z, (float)sum.W))];
    }

    public Vector4 Mean(float position, float footprint, bool periodic) =>
        (periodic && float.IsPositiveInfinity(footprint) ? sums[Bins] / Bins : Spanned(position, footprint, periodic)) switch {
            var mean when mean.W > 0f => new Vector4(mean.AsVector3() / mean.W, mean.W),
            _ => Vector4.Zero,
        };

    private Vector4 Spanned(float position, float footprint, bool periodic) {
        double low = (position - (footprint / 2d)) * Bins;
        double start = periodic ? low - (Math.Floor(low / Bins) * Bins) : low;
        double end = start + (footprint * Bins);
        return end == start ? bins[(int)double.Clamp(start, 0, Bins - 1)] : (Integral(end, periodic) - Integral(start, periodic)) / (footprint * Bins);
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
