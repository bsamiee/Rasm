using System.Numerics;
using System.Runtime.InteropServices;
using PeterO.Numbers;
using Rasm.Imaging.Color;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Tone;
using UnitsNet;
using UnitsNet.Units;

namespace Rasm.Imaging.Filters.Generators;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct SuperellipseExponent : IMinMaxValue<SuperellipseExponent> {
    public static SuperellipseExponent MinValue { get; } = new(1f);
    public static SuperellipseExponent MaxValue { get; } = new(8f);
    public static SuperellipseExponent Ellipse { get; } = new(2f);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct SideCount : IMinMaxValue<SideCount> {
    public static SideCount MinValue { get; } = new(3);
    public static SideCount MaxValue { get; } = new(32);
    public static SideCount Five { get; } = new(5);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct SubdivisionDepth : IMinMaxValue<SubdivisionDepth> {
    public static SubdivisionDepth MinValue { get; } = new(1);
    public static SubdivisionDepth MaxValue { get; } = new(5);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<int>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Neutral", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct BranchCount : IMinMaxValue<BranchCount> {
    public static BranchCount MinValue => Neutral;
    public static BranchCount MaxValue { get; } = new(7);
    public static BranchCount Standard { get; } = new(3);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct ForkSpread : IMinMaxValue<ForkSpread> {
    public static ForkSpread MinValue { get; } = new(0f);
    public static ForkSpread MaxValue { get; } = new(float.Pi / 3f);
    public static ForkSpread Standard { get; } = new(0.5f);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

internal readonly record struct FieldSample(EFloat Distance, EFloat Width);

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ShapeFit : ICaptioned {
    public static readonly ShapeFit ShortSide = new("short-side", "Short side", static _ => (1d, 1d));
    public static readonly ShapeFit LongSide = new("long-side", "Long side", static extent => ((double)int.Max(extent.Width, extent.Height) / extent.ShortSide) switch { var scale => (scale, scale) });

    public string Caption { get; }

    [UseDelegateFromConstructor]
    public partial (double X, double Y) Scale(PixelExtent extent);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ShapeKind : ICaptioned {
    public static readonly ShapeKind Ellipse = new("ellipse", "Ellipse", static (shape, half, math) => (point => ShapeFields.Ellipse(point, half, (float)shape.Exponent, math), 2 * EFloat.Min(half.X, half.Y)));
    public static readonly ShapeKind Box = new("box", "Box", static (shape, half, math) => EFloat.Min(half.X, half.Y) switch {
        var small => (point => ShapeFields.Box(point, half, (float)shape.Roundness * small, math), 2 * small),
    });
    public static readonly ShapeKind Polygon = new("polygon", "Polygon", static (shape, half, math) => {
        double wedge = double.Pi / shape.Sides;
        EFloat radius = (float)shape.Roundness * half.X;
        (double sine, double cosine) = double.SinCos(wedge);
        (EFloat depth, EFloat reach) = ((EFloat)cosine * (1 - EFloat.FromSingle(shape.Inset)), half.X - radius);
        ((EFloat X, EFloat Y) outer, (EFloat X, EFloat Y) inner) = ((0, reach), (reach * depth * sine, reach * depth * cosine));
        return (point => double.SinCos(double.Abs(double.Ieee754Remainder(double.Atan2(point.X.ToDouble(), point.Y.ToDouble()), 2d * wedge))) switch {
            var (sin, cos) => ShapeFields.Length(point, math) switch { var length => (X: length * sin, Y: length * cos) } switch {
                var folded => ShapeFields.Offset(folded, outer, inner, math).Offset switch { var offset => ShapeFields.Length(offset, math).CopySign(ShapeFields.Side(folded, outer, inner)) - radius },
            },
        }, half.X * (1 + depth));
    });
    public static readonly ShapeKind Ring = new("ring", "Ring", static (shape, half, math) => ((float)shape.Inset * EFloat.Min(half.X, half.Y)) switch {
        var band => (point => (ShapeFields.Ellipse(point, half, (float)shape.Exponent, math) + (band * 0.5d)).Abs() - (band * 0.5d), band),
    });
    public static readonly ShapeKind Cross = new("cross", "Cross", static (shape, half, math) => ((1 - EFloat.FromSingle(shape.Inset)) * EFloat.Min(half.X, half.Y)) switch {
        var arm => ((float)shape.Roundness * arm) switch { var radius => (point => EFloat.Min(ShapeFields.Box(point, (half.X, arm), radius, math), ShapeFields.Box(point, (arm, half.Y), radius, math)), 2 * arm) },
    });
    public static readonly ShapeKind Heart = new("heart", "Heart", static (_, half, math) => (Center: (X: (EFloat)0.25d, Y: (EFloat)0.75d), Radius: EFloat.FromInt32(2).Sqrt(math) * 0.25d) switch {
        var lobe => (half.X.Divide(lobe.Center.X + lobe.Radius, math), (lobe.Center.Y + lobe.Radius) * 0.5d) switch {
            var (scale, middle) => (point => (X: point.X.Abs().Divide(scale, math), Y: point.Y.Divide(scale, math) + middle) switch {
                var q when (q.X + q.Y).CompareTo(EFloat.One) > 0 => scale * (ShapeFields.Length((q.X - lobe.Center.X, q.Y - lobe.Center.Y), math) - lobe.Radius),
                var q => (0.5d * EFloat.Max(q.X + q.Y, 0)) switch { var diagonal => scale * EFloat.Min(ShapeFields.Length((q.X, q.Y - 1), math), ShapeFields.Length((q.X - diagonal, q.Y - diagonal), math)) * (q.X - q.Y).Sign },
            }, 2 * lobe.Radius * scale),
        },
    });
    public static readonly ShapeKind Triangle = new("triangle", "Triangle", static (shape, half, math) => {
        ((EFloat X, EFloat Y) a, (EFloat X, EFloat Y) b, (EFloat X, EFloat Y) c) = ((-half.X, -half.Y), (half.X, -half.Y), ((float)shape.Apex * half.X, half.Y));
        (EFloat opposite, EFloat across, EFloat under) = (ShapeFields.Length((b.X - c.X, b.Y - c.Y), math), ShapeFields.Length((c.X - a.X, c.Y - a.Y), math), ShapeFields.Length((a.X - b.X, a.Y - b.Y), math));
        (EFloat perimeter, EFloat twiceArea) = (opposite + across + under, 4 * half.X * half.Y);
        (EFloat X, EFloat Y) = (((opposite * a.X) + (across * b.X) + (under * c.X)).Divide(perimeter, math), ((opposite * a.Y) + (across * b.Y) + (under * c.Y)).Divide(perimeter, math));
        Func<(EFloat X, EFloat Y), (EFloat X, EFloat Y)> inward = vertex => (X + ((vertex.X - X) * (1 - EFloat.FromSingle(shape.Roundness))), Y + ((vertex.Y - Y) * (1 - EFloat.FromSingle(shape.Roundness))));
        ((EFloat X, EFloat Y) p, (EFloat X, EFloat Y) q, (EFloat X, EFloat Y) r, EFloat radius) = (inward(a), inward(b), inward(c), ((float)shape.Roundness * twiceArea).Divide(perimeter, math));
        return (point => (Edge(point, p, q), Edge(point, q, r), Edge(point, r, p)) switch {
            var (first, second, third) => (EFloat.Min(first.Distance, EFloat.Min(second.Distance, third.Distance)) * (EFloat.Min(first.Side, EFloat.Min(second.Side, third.Side)).Sign > 0 ? -1 : 1)) - radius,
        }, twiceArea.Divide(EFloat.Max(under, EFloat.Max(opposite, across)), math));

        (EFloat Distance, EFloat Side) Edge((EFloat X, EFloat Y) point, (EFloat X, EFloat Y) start, (EFloat X, EFloat Y) end) =>
            ShapeFields.Offset(point, start, end, math).Offset switch { var offset => (ShapeFields.Length(offset, math), ShapeFields.Side(point, start, end)) };
    });

    public string Caption { get; }

    [UseDelegateFromConstructor]
    internal partial (Func<(EFloat X, EFloat Y), EFloat> Distance, EFloat Width) Field(ShapePrimitive primitive, (EFloat X, EFloat Y) half, EContext math);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ShapeOperation : ICaptioned {
    public static readonly ShapeOperation Union = new("union", "Union", static (first, second, smoothing, _, math) => new(Smooth(first.Distance, second.Distance, smoothing, math), EFloat.Max(first.Width, second.Width)));
    public static readonly ShapeOperation Difference = new("difference", "Difference", static (first, second, smoothing, _, math) => new(-Smooth(-first.Distance, second.Distance, smoothing, math), first.Width));
    public static readonly ShapeOperation Intersection = new("intersection", "Intersection", static (first, second, smoothing, _, math) => new(-Smooth(-first.Distance, -second.Distance, smoothing, math), EFloat.Min(first.Width, second.Width)));
    public static readonly ShapeOperation Morph = new("morph", "Morph", static (first, second, _, progress, _) => progress switch {
        0f => first,
        1f => second,
        _ => new(first.Width.IsZero || second.Width.IsZero ? EFloat.PositiveInfinity : first.Distance + ((second.Distance - first.Distance) * progress), first.Width + ((second.Width - first.Width) * progress)),
    });

    public string Caption { get; }

    [UseDelegateFromConstructor]
    internal partial FieldSample Join(FieldSample first, FieldSample second, EFloat smoothing, float progress, EContext math);

    private static EFloat Smooth(EFloat first, EFloat second, EFloat smoothing, EContext math) =>
        (EFloat.Min(first, second), smoothing - (first - second).Abs()) switch {
            (var low, var overlap) when overlap.Sign > 0 => low - (overlap * overlap).Divide(4 * smoothing, math),
            (var low, _) => low,
        };
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GradientKind : ICaptioned {
    public static readonly GradientKind Linear = new("linear", "Linear", periodic: false, static extent => point => (0.5d + (point.X / (2d * extent.X)), 1d / (2d * extent.X)));
    public static readonly GradientKind Radial = new("radial", "Radial", periodic: false, static extent => point => double.Hypot(point.X / extent.X, point.Y / extent.Y) switch {
        var level => (level, level > 0d ? double.Hypot(point.X / (extent.X * extent.X * level), point.Y / (extent.Y * extent.Y * level)) : 1d / extent.Y),
    });
    public static readonly GradientKind Angular = new("angular", "Angular", periodic: true, static _ => static point => ((double.Atan2(point.Y, point.X) / double.Tau) + 0.5d, 1d / (double.Tau * double.Hypot(point.X, point.Y))));
    public static readonly GradientKind Box = new("box", "Box", periodic: false, static extent => point => (X: double.Abs(point.X) / extent.X, Y: double.Abs(point.Y) / extent.Y) switch { var reach => reach.X >= reach.Y ? (reach.X, 1d / extent.X) : (reach.Y, 1d / extent.Y) });

    public string Caption { get; }
    internal bool Periodic { get; }

    [UseDelegateFromConstructor]
    internal partial Func<(double X, double Y), (double Position, double Slope)> Parameter((double X, double Y) extent);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GridPattern : ICaptioned {
    public static readonly GridPattern Lines = new("lines", "Lines");
    public static readonly GridPattern Checker = new("checker", "Checker");
    public static readonly GridPattern Brick = new("brick", "Brick");

    public string Caption { get; }
}

public sealed record ShapePrimitive(
    ShapeKind Kind, Placement Placement, ShortSideLength Width, ShortSideLength Height, AxisFraction Roundness, SuperellipseExponent Exponent, SideCount Sides, AxisFraction Inset,
    SignedFraction Apex)
    : IStateRecord<ShapePrimitive, ShapePrimitiveParameter> {
    public static ShapePrimitive Default { get; } = ShortSideLength.Create(0.5f) switch {
        var extent => new(ShapeKind.Ellipse, Placement.Default, extent, extent, AxisFraction.MinValue, SuperellipseExponent.Ellipse, SideCount.Five, AxisFraction.MinValue, SignedFraction.Neutral),
    };
    public ShapeFit Fit { get; init; } = ShapeFit.ShortSide;

    internal (FieldFrame Frame, Func<long, long, FieldSample> Sample) Field(PixelExtent extent, EContext math) =>
        (Placement.Frame(extent), Fit.Scale(extent)) switch {
            var (frame, _) when Width == ShortSideLength.Neutral || Height == ShortSideLength.Neutral => (frame, static (_, _) => new(double.PositiveInfinity, 0d)),
            var (frame, scale) => Kind.Field(this, (EFloat.FromSingle(Width) * scale.X, EFloat.FromSingle(Height) * scale.Y), math) switch {
                var (distance, width) => (frame, (column, line) => frame.Upward(column, line) switch { var (x, y) => new(distance((x, y)), width) }),
            },
        };
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ShapePrimitiveParameter : IStateParameter<ShapePrimitive> {
    private static readonly Presentation<float> Extent = ShortSideLength.Presentation with { Soft = (0.01f, 2f) };

    public static readonly ShapePrimitiveParameter Kind = new("kind", new StateParameter<ShapePrimitive>.Choice<ShapeKind>(Lens<ShapePrimitive, ShapeKind>.New(static shape => shape.Kind, static kind => shape => shape with { Kind = kind })),
        new("Shape", "Figure the distance field draws"));
    public static readonly ShapePrimitiveParameter Placement = new("placement", new StateParameter<ShapePrimitive>.Record<Placement>(Lens<ShapePrimitive, Placement>.New(static shape => shape.Placement, static placement => shape => shape with { Placement = placement })),
        new("Placement", "Position and turn of the figure"));
    public static readonly ShapePrimitiveParameter Width = new("width", StateParameter.Bounded(Lens<ShapePrimitive, ShortSideLength>.New(static shape => shape.Width, static width => shape => shape with { Width = width }), Extent),
        new("Width", "Half width as a share of the selected frame side"));
    public static readonly ShapePrimitiveParameter Height = new("height", StateParameter.Bounded(Lens<ShapePrimitive, ShortSideLength>.New(static shape => shape.Height, static height => shape => shape with { Height = height }), Extent),
        new("Height", "Half height as a share of the selected frame side"));
    public static readonly ShapePrimitiveParameter Roundness = new("roundness", StateParameter.Bounded(Lens<ShapePrimitive, AxisFraction>.New(static shape => shape.Roundness, static roundness => shape => shape with { Roundness = roundness })),
        new("Roundness", "Corner radius as a share of the shorter half extent"));
    public static readonly ShapePrimitiveParameter Exponent = new("exponent", StateParameter.Bounded(Lens<ShapePrimitive, SuperellipseExponent>.New(static shape => shape.Exponent, static exponent => shape => shape with { Exponent = exponent }), new Presentation<float> { Scale = TrackScale.Log, Origin = (float)SuperellipseExponent.Ellipse }),
        new("Exponent", "Superellipse exponent, 1 a diamond, 2 an ellipse, above 2 a squircle"));
    public static readonly ShapePrimitiveParameter Sides = new("sides", StateParameter.Bounded(Lens<ShapePrimitive, SideCount>.New(static shape => shape.Sides, static sides => shape => shape with { Sides = sides }), new Presentation<int> { Step = 1 }),
        new("Sides", "Polygon side count"));
    public static readonly ShapePrimitiveParameter Inset = new("inset", StateParameter.Bounded(Lens<ShapePrimitive, AxisFraction>.New(static shape => shape.Inset, static inset => shape => shape with { Inset = inset })),
        new("Inset", "Star inset of a polygon, the band of a ring, the arm width of a cross"));
    public static readonly ShapePrimitiveParameter Apex = new("apex", StateParameter.Bounded(Lens<ShapePrimitive, SignedFraction>.New(static shape => shape.Apex, static apex => shape => shape with { Apex = apex }), SignedFraction.Presentation with { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction) }),
        new("Apex", "Horizontal position of the triangle's top vertex between its left and right edges"));
    public static readonly ShapePrimitiveParameter Fit = new("fit", new StateParameter<ShapePrimitive>.Choice<ShapeFit>(Lens<ShapePrimitive, ShapeFit>.New(static shape => shape.Fit, static fit => shape => shape with { Fit = fit })),
        new("Fit", "Frame side the shape dimensions follow"));

    StateParameter<ShapePrimitive> IStateParameter<ShapePrimitive>.Kind => Parameter;

    private StateParameter<ShapePrimitive> Parameter { get; }
    public ParameterText Text { get; }
}

public sealed record ShapeStyle(
    Gated<Swatch> Fill, Swatch Outline, ShortSideLength OutlineWidth, ShortSideLength OutlineOffset, ShortSideLength Feather, ShortSideLength Roughness, bool Inverted)
    : IStateRecord<ShapeStyle, ShapeStyleParameter> {
    public static ShapeStyle Default { get; } = new(new(Enabled: true, Swatch.White), Swatch.Black, ShortSideLength.Neutral, ShortSideLength.Neutral, ShortSideLength.Neutral, ShortSideLength.Neutral, Inverted: false);
    private static readonly NoiseBasis Gate = NoiseBasis.RidgedMultifractal.Default with {
        Dimensions = NoiseDimensions.Four,
        Octaves = new(FractalDetail.MaxValue, AxisFraction.Create(0.9f), FractalLacunarity.Standard),
        Distortion = NoiseDistortion.Create(1.9f),
        Gain = NoiseGain.Create(74.5f),
    };

    internal Func<FieldSample, (double X, double Y), Vector4> Shading(PassContext context, uint field, EContext math) {
        Func<Swatch, Vector3> light = context.Signal == TransferCurve.Linear ? swatch => swatch.SceneLight(context.Working) : static swatch => swatch.Display.AsVector3();
        (Vector4 fill, Vector3 outline, EFloat pixels, EFloat band) = (Fill.Active.Map(swatch => new Vector4(light(swatch), 1f)).IfNone(Vector4.Zero), light(Outline), context.Extent.ShortSide, ShapeEdge.Band(Feather, context.Extent));
        (EFloat offset, EFloat width, EFloat reach) = (EFloat.FromSingle(OutlineOffset) * pixels, EFloat.FromSingle(OutlineWidth) * pixels, EFloat.FromSingle(Roughness) * pixels);
        (EFloat near, EFloat far) = (-reach - (band * 0.5d), offset + width + reach + (band * 0.5d));
        Func<LatticePoint, uint, NoiseSample<LatticePoint>> gate = Gate.Sampler;
        Func<EFloat, (double X, double Y), EFloat> rough = reach.IsZero ? static (distance, _) => distance
            : (distance, local) => distance.CompareTo(near) < 0 || distance.CompareTo(far) > 0 ? distance : distance + (reach * ((2f * Easing.Saturate(gate(new(ERational.FromDouble(local.X).Multiply(ERational.FromDouble(2.5d)), ERational.FromDouble(local.Y).Multiply(ERational.FromDouble(2.5d)), ERational.Zero, ERational.Zero), field).Value)) - 1f));
        return (sample, local) => (Inverted ? new FieldSample(-sample.Distance, double.PositiveInfinity) : sample) switch {
            var (distance, narrowest) => (rough(distance * pixels, local), narrowest * pixels) switch {
                var (edge, span) => ((fill.W * Grown(edge, span, 0)).ToDouble(), (Grown(edge, span, offset + width) - Grown(edge, span, offset)).ToDouble()) switch {
                    var (covered, ring) when covered + ring > 0d => new Vector4((float)(((covered * fill.X) + (ring * outline.X)) / (covered + ring)),
                        (float)(((covered * fill.Y) + (ring * outline.Y)) / (covered + ring)), (float)(((covered * fill.Z) + (ring * outline.Z)) / (covered + ring)), (float)(covered + ring)),
                    _ => Vector4.Zero,
                },
            },
        };

        EFloat Grown(EFloat distance, EFloat span, EFloat growth) => EFloat.Min(
            EFloat.Max(0, EFloat.Min(1, 0.5d - (distance - growth).Divide(band, math))), (span + (2 * growth)).Divide(band, math));
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ShapeStyleParameter : IStateParameter<ShapeStyle> {
    private static readonly Presentation<float> Ring = ShortSideLength.Presentation with { Soft = (0f, 0.1f) };

    public static readonly ShapeStyleParameter Fill = new("fill", new StateParameter<ShapeStyle>.OptionalColor(Lens<ShapeStyle, Gated<Swatch>>.New(static style => style.Fill, static value => style => style with { Fill = value })),
        new("Fill", "Color inside the boundary, off drawing the outline alone"));
    public static readonly ShapeStyleParameter Outline = new("outline", new StateParameter<ShapeStyle>.Color(Lens<ShapeStyle, Swatch>.New(static style => style.Outline, static value => style => style with { Outline = value })),
        new("Outline", "Color of the band around the boundary"));
    public static readonly ShapeStyleParameter OutlineWidth = new("outline-width", StateParameter.Bounded(Lens<ShapeStyle, ShortSideLength>.New(static style => style.OutlineWidth, static value => style => style with { OutlineWidth = value }), Ring),
        new("Outline width", "Width of the outline band as a share of the frame's short side"));
    public static readonly ShapeStyleParameter OutlineOffset = new("outline-offset", StateParameter.Bounded(Lens<ShapeStyle, ShortSideLength>.New(static style => style.OutlineOffset, static value => style => style with { OutlineOffset = value }), Ring),
        new("Outline offset", "Distance of the outline from the boundary as a share of the frame's short side"));
    public static readonly ShapeStyleParameter Feather = new("feather", StateParameter.Bounded(Lens<ShapeStyle, ShortSideLength>.New(static style => style.Feather, static value => style => style with { Feather = value }), ShortSideLength.Presentation with { Soft = (0f, 0.25f) }),
        new("Feather", "Edge softness as a share of the frame's short side, 0% one pixel wide"));
    public static readonly ShapeStyleParameter Roughness = new("roughness", StateParameter.Bounded(Lens<ShapeStyle, ShortSideLength>.New(static style => style.Roughness, static value => style => style with { Roughness = value }), ShortSideLength.Presentation with { Soft = (0f, 0.02f) }),
        new("Rough edge", "Noise displacement of the edge as a share of the frame's short side"));
    public static readonly ShapeStyleParameter Inverted = new("inverted", new StateParameter<ShapeStyle>.Toggle(Lens<ShapeStyle, bool>.New(static style => style.Inverted, static inverted => style => style with { Inverted = inverted })),
        new("Invert", "Fills outside the boundary"));

    public StateParameter<ShapeStyle> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record Shapes(
    ShapePrimitive First, Option<ShapeOperation> Operation, ShapePrimitive Second, ShortSideLength Smoothing, AxisFraction Progress, ShapeStyle Style, GeneratedLayer Layer, Seed Seed)
    : IStateRecord<Shapes, ShapesParameter>, IPixelStage<Shapes> {
    public static Shapes Default { get; } = new(
        ShapePrimitive.Default, None, ShapePrimitive.Default with { Kind = ShapeKind.Box, Roundness = AxisFraction.Create(0.3f / 2f / ShapePrimitive.Default.Width) },
        ShortSideLength.Neutral, AxisFraction.MinValue, ShapeStyle.Default, GeneratedLayer.Default, Seed.MinValue);

    public static Option<PixelPass> Pass(Shapes state, PassContext context) => Some(state.Layer.Pass(Fill(state, context)));

    public static PixelFrame Matte(Shapes state, PassContext context, PixelFrame frame) =>
        Fill(state, context) switch {
            var fill => new(frame.Origin, frame.Size, frame.Extent, block => {
                Span<Vector4> pixels = MemoryMarshal.Cast<float, Vector4>(block.AsSpan());
                for (int i = 0; i < frame.Size.Height; i++)
                    fill(pixels.Slice(i * frame.Size.Width, frame.Size.Width), frame.Origin.X, frame.Line(i));
            }),
        };

    private static Action<Span<Vector4>, long, long> Fill(Shapes state, PassContext context) {
        EContext math = ShapeFields.Precision(context.Extent);
        (FieldFrame frame, Func<long, long, FieldSample> first) = state.First.Field(context.Extent, math);
        Func<long, long, FieldSample> field = state.Operation.Match(
            Some: operation => (state.Second.Field(context.Extent, math).Sample, EFloat.FromSingle(state.Smoothing), (float)state.Progress) switch {
                var (second, smoothing, progress) => (column, line) => operation.Join(first(column, line), second(column, line), smoothing, progress, math),
            },
            None: () => first);
        Func<FieldSample, (double X, double Y), Vector4> shade = state.Style.Shading(context, CoordinateHash.Field(NoiseStream.Shape, state.Seed, 0u), math);
        return (row, column, line) => {
            for (int i = 0; i < row.Length; i++)
                row[i] = shade(field(column + i, line), frame.Upward(column + i, line));
        };
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ShapesParameter : IStateParameter<Shapes> {
    public static readonly ShapesParameter First = new("first", new StateParameter<Shapes>.Record<ShapePrimitive>(Lens<Shapes, ShapePrimitive>.New(static shape => shape.First, static first => shape => shape with { First = first })),
        new("First", "First figure of the combination"));
    public static readonly ShapesParameter Second = new("second", new StateParameter<Shapes>.Record<ShapePrimitive>(Lens<Shapes, ShapePrimitive>.New(static shape => shape.Second, static second => shape => shape with { Second = second })),
        new("Second", "Second figure of the combination"));
    public static readonly ShapesParameter Operation = new("operation", new StateParameter<Shapes>.OptionalChoice<ShapeOperation>(Lens<Shapes, Option<ShapeOperation>>.New(static shape => shape.Operation, static operation => shape => shape with { Operation = operation }), "None"),
        new("Operation", "Combination of the first shape with the second"));
    public static readonly ShapesParameter Smoothing = new("smoothing", StateParameter.Bounded(Lens<Shapes, ShortSideLength>.New(static shape => shape.Smoothing, static smoothing => shape => shape with { Smoothing = smoothing }), ShortSideLength.Presentation with { Soft = (0f, 0.25f) }),
        new("Smoothing", "Blend width of the combination as a share of the frame's short side"));
    public static readonly ShapesParameter Progress = new("progress", StateParameter.Bounded(Lens<Shapes, AxisFraction>.New(static shape => shape.Progress, static progress => shape => shape with { Progress = progress })),
        new("Progress", "Morph from the first shape at 0% to the second at 100%"));
    public static readonly ShapesParameter Style = new("style", new StateParameter<Shapes>.Record<ShapeStyle>(Lens<Shapes, ShapeStyle>.New(static shape => shape.Style, static style => shape => shape with { Style = style })),
        new("Style", "Fill and boundary of the generated figure"));
    public static readonly ShapesParameter Layer = new("layer", new StateParameter<Shapes>.Record<GeneratedLayer>(Lens<Shapes, GeneratedLayer>.New(static shape => shape.Layer, static layer => shape => shape with { Layer = layer })),
        new("Layer", "Composition of the generated layer over the frame"));
    public static readonly ShapesParameter Seed = new("seed", StateParameter.Bounded(Lens<Shapes, Seed>.New(static shape => shape.Seed, static seed => shape => shape with { Seed = seed })),
        new("Seed", "Number selecting the field's pattern"));

    public StateParameter<Shapes> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record Lightning(
    Placement Placement, ShortSideLength Length, ShortSideLength Thickness, SubdivisionDepth Depth, AxisFraction Displacement, BranchCount Branches, AxisFraction Reach,
    ForkSpread Spread, AxisFraction Taper, Frequency Speed, ShapeStyle Style, GeneratedLayer Layer, Seed Seed, Timing Timing, Hold Hold)
    : IStateRecord<Lightning, LightningParameter>, IPixelStage<Lightning> {
    public static Lightning Default { get; } = Hold.Create(0.05f) switch {
        var hold => new(
            Placement.Default, ShortSideLength.Create(1f / 2f), ShortSideLength.Create(0.01f / (5f - (4.5f * 0.5f)) * ReferenceFrame.GreaterSide), SubdivisionDepth.MaxValue, AxisFraction.Half,
            BranchCount.Standard, AxisFraction.Create(0.4f), ForkSpread.Standard, AxisFraction.Half, Frequency.Create(1f / hold), ShapeStyle.Default,
            GeneratedLayer.Added with { Exposure = Exposure.Create(3f) }, Seed.MinValue, Timing.Default, hold),
    };

    public static Option<PixelPass> Pass(Lightning state, PassContext context) {
        (uint field, float clock, double chord) = (CoordinateHash.Field(NoiseStream.Lightning, state.Seed, 0u), 2f * state.Speed * state.Hold.Held(state.Timing.At(context)), 2d * state.Length);
        Func<int, int, int, float> noise = (x, y, z) => 0.5f + (0.5f * NoiseDimensions.Four.Gradient(new Vector4((2f * x) + 0.5f, (2f * y) + 0.5f, (2f * z) + 0.5f, clock), field));
        BoltPiece[] channel = BoltPiece.Path((0d, state.Length), (0d, -1d), chord, SubdivisionDepth.MaxValue, state, k => noise(k, 0, 0), _ => state.Thickness / 2d);
        BoltPiece[] pieces = [.. channel, .. Enumerable.Range(1, state.Branches).SelectMany(i => (BitOperations.Log2((uint)i), (i % 2 is 0 ? 1f : -1f) * state.Spread * (0.5f + noise(i, 0, 1))) switch {
            var (level, turn) => BoltPiece.Path(
                channel[((2 * (i - (1 << level))) + 1) * channel.Length / (2 << level)].Start, double.SinCos(turn) switch { var (sin, cos) => (sin, -cos) },
                state.Reach * chord * (0.5f + noise(i, 0, 2)), levels: 3, state, j => noise(j, i, 0), along => (double)state.Taper * state.Thickness * (1d - along) / 2d),
        })];
        EContext math = ShapeFields.Precision(context.Extent);
        Func<FieldSample, (double X, double Y), Vector4> shade = state.Style.Shading(context, CoordinateHash.Branch(field, 0u), math);
        return Some(state.Layer.Pass(state.Placement.Frame(context.Extent).Fill(point => {
            FieldSample nearest = new(float.PositiveInfinity, 0f);
            foreach (BoltPiece piece in pieces)
                nearest = piece.Sample(point, math) switch { var next => next.Distance.CompareTo(nearest.Distance) < 0 ? next : nearest };
            return shade(nearest, point);
        })));
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LightningParameter : IStateParameter<Lightning> {
    public static readonly LightningParameter Placement = new("placement", new StateParameter<Lightning>.Record<Placement>(Lens<Lightning, Placement>.New(static bolt => bolt.Placement, static placement => bolt => bolt with { Placement = placement })),
        new("Placement", "Position and turn of the field"));
    public static readonly LightningParameter Length = new("length", StateParameter.Bounded(Lens<Lightning, ShortSideLength>.New(static bolt => bolt.Length, static length => bolt => bolt with { Length = length })),
        new("Length", "Half the stroke length as a share of the frame's short side"));
    public static readonly LightningParameter Thickness = new("thickness", StateParameter.Bounded(Lens<Lightning, ShortSideLength>.New(static bolt => bolt.Thickness, static thickness => bolt => bolt with { Thickness = thickness }), ShortSideLength.Presentation with { Soft = (0f, 0.02f) }),
        new("Thickness", "Width of the stroke as a share of the frame's short side"));
    public static readonly LightningParameter Depth = new("depth", StateParameter.Bounded(Lens<Lightning, SubdivisionDepth>.New(static bolt => bolt.Depth, static depth => bolt => bolt with { Depth = depth }), new Presentation<int> { Step = 1 }),
        new("Detail", "Subdivisions of each displaced segment"));
    public static readonly LightningParameter Displacement = new("displacement", StateParameter.Bounded(Lens<Lightning, AxisFraction>.New(static bolt => bolt.Displacement, static displacement => bolt => bolt with { Displacement = displacement })),
        new("Displacement", "Share of each segment's length the stroke wanders by"));
    public static readonly LightningParameter Branches = new("branches", StateParameter.Bounded(Lens<Lightning, BranchCount>.New(static bolt => bolt.Branches, static branches => bolt => bolt with { Branches = branches }), new Presentation<int> { Step = 1 }),
        new("Branches", "Forks from the stroke's midpoints"));
    public static readonly LightningParameter Reach = new("reach", StateParameter.Bounded(Lens<Lightning, AxisFraction>.New(static bolt => bolt.Reach, static reach => bolt => bolt with { Reach = reach })),
        new("Reach", "Share of the stroke length the branches reach"));
    public static readonly LightningParameter Spread = new("spread", StateParameter.Bounded(Lens<Lightning, ForkSpread>.New(static bolt => bolt.Spread, static spread => bolt => bolt with { Spread = spread }), new Presentation<float> { Unit = Quantity.GetUnitInfo(AngleUnit.Radian) }),
        new("Spread", "Angle by which the branches turn from the stroke"));
    public static readonly LightningParameter Taper = new("taper", StateParameter.Bounded(Lens<Lightning, AxisFraction>.New(static bolt => bolt.Taper, static taper => bolt => bolt with { Taper = taper })),
        new("Taper", "Width of each branch against the stroke before narrowing to its tip"));
    public static readonly LightningParameter Speed = new("speed", StateParameter.Bounded(Lens<Lightning, Frequency>.New(static bolt => bolt.Speed, static speed => bolt => bolt with { Speed = speed })),
        new("Speed", "Noise units per second the bolt changes through"));
    public static readonly LightningParameter Style = new("style", new StateParameter<Lightning>.Record<ShapeStyle>(Lens<Lightning, ShapeStyle>.New(static bolt => bolt.Style, static style => bolt => bolt with { Style = style })),
        new("Style", "Fill and boundary of the generated figure"));
    public static readonly LightningParameter Layer = new("layer", new StateParameter<Lightning>.Record<GeneratedLayer>(Lens<Lightning, GeneratedLayer>.New(static bolt => bolt.Layer, static layer => bolt => bolt with { Layer = layer })),
        new("Layer", "Composition of the generated layer over the frame"));
    public static readonly LightningParameter Seed = new("seed", StateParameter.Bounded(Lens<Lightning, Seed>.New(static bolt => bolt.Seed, static seed => bolt => bolt with { Seed = seed })),
        new("Seed", "Number selecting the field's pattern"));
    public static readonly LightningParameter Timing = new("timing", new StateParameter<Lightning>.Record<Timing>(Lens<Lightning, Timing>.New(static bolt => bolt.Timing, static timing => bolt => bolt with { Timing = timing })),
        new("Timing", "Stored time and rate of recorded frame time"));
    public static readonly LightningParameter Hold = new("hold", StateParameter.Bounded(Lens<Lightning, Hold>.New(static bolt => bolt.Hold, static hold => bolt => bolt with { Hold = hold })),
        new("Hold", "Seconds each drawn pattern holds before the next"));

    public StateParameter<Lightning> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record Gradient(GradientKind Kind, Placement Placement, ShortSideExtent Width, ShortSideExtent Height, Ramp Colors, GeneratedLayer Layer)
    : IStateRecord<Gradient, GradientParameter>, IPixelStage<Gradient> {
    public static Gradient Default { get; } = ShortSideExtent.Create(1f / 2f) switch {
        var extent => new(GradientKind.Linear, Placement.Default, extent, extent, Ramp.Grayscale, GeneratedLayer.Default),
    };

    public static Option<PixelPass> Pass(Gradient state, PassContext context) {
        Func<(double X, double Y), (double Position, double Slope)> parameter = state.Kind.Parameter((state.Width, state.Height));
        RampFootprint footprint = RampFootprint.Of((state.Colors, None));
        return Some(state.Layer.Pass(state.Placement.Frame(context.Extent).Fill(point => {
            (double position, double slope) = parameter(point);
            return footprint.Mean(position, slope / context.Extent.ShortSide, state.Kind.Periodic);
        })));
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GradientParameter : IStateParameter<Gradient> {
    private static readonly Presentation<float> Extent = ShortSideExtent.Presentation with { Soft = (0.01f, 4f) };

    public static readonly GradientParameter Kind = new("kind", new StateParameter<Gradient>.Choice<GradientKind>(Lens<Gradient, GradientKind>.New(static gradient => gradient.Kind, static kind => gradient => gradient with { Kind = kind })),
        new("Type", "Field the ramp runs over"));
    public static readonly GradientParameter Placement = new("placement", new StateParameter<Gradient>.Record<Placement>(Lens<Gradient, Placement>.New(static gradient => gradient.Placement, static placement => gradient => gradient with { Placement = placement })),
        new("Placement", "Position and turn of the field"));
    public static readonly GradientParameter Width = new("width", StateParameter.Bounded(Lens<Gradient, ShortSideExtent>.New(static gradient => gradient.Width, static width => gradient => gradient with { Width = width }), Extent),
        new("Width", "Half width the ramp spans as a share of the frame's short side"));
    public static readonly GradientParameter Height = new("height", StateParameter.Bounded(Lens<Gradient, ShortSideExtent>.New(static gradient => gradient.Height, static height => gradient => gradient with { Height = height }), Extent),
        new("Height", "Half height the ramp spans as a share of the frame's short side"));
    public static readonly GradientParameter Colors = new("colors", new StateParameter<Gradient>.Gradient(Lens<Gradient, Ramp>.New(static gradient => gradient.Colors, static colors => gradient => gradient with { Colors = colors })),
        new("Colors", "Display colors and coverage along the ramp"));
    public static readonly GradientParameter Layer = new("layer", new StateParameter<Gradient>.Record<GeneratedLayer>(Lens<Gradient, GeneratedLayer>.New(static gradient => gradient.Layer, static layer => gradient => gradient with { Layer = layer })),
        new("Layer", "Composition of the generated layer over the frame"));

    StateParameter<Gradient> IStateParameter<Gradient>.Kind => Parameter;

    private StateParameter<Gradient> Parameter { get; }
    public ParameterText Text { get; }
}

public sealed record CornerGradient(Swatch TopLeft, Swatch TopRight, Swatch BottomRight, Swatch BottomLeft, GeneratedLayer Layer)
    : IStateRecord<CornerGradient, CornerGradientParameter>, IPixelStage<CornerGradient> {
    public static CornerGradient Default { get; } = new(Swatch.White, Swatch.Black, Swatch.White, Swatch.Black, GeneratedLayer.Default);

    public static Option<PixelPass> Pass(CornerGradient state, PassContext context) {
        (Vector3 topLeft, Vector3 topRight, Vector3 bottomRight, Vector3 bottomLeft) =
            (state.TopLeft.Display.AsVector3(), state.TopRight.Display.AsVector3(), state.BottomRight.Display.AsVector3(), state.BottomLeft.Display.AsVector3());
        return Some(state.Layer.Pass((row, column, line) => {
            float down = (float)RampInterpolation.SmootherRgb.Weight(double.Clamp((context.Extent.Height - line - 0.5d) / context.Extent.Height, 0d, 1d));
            for (int i = 0; i < row.Length; i++) {
                float across = (float)RampInterpolation.SmootherRgb.Weight(double.Clamp((column + i + 0.5d) / context.Extent.Width, 0d, 1d));
                row[i] = new Vector4(Vector3.Lerp(Vector3.Lerp(topLeft, topRight, across), Vector3.Lerp(bottomLeft, bottomRight, across), down), 1f);
            }
        }));
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CornerGradientParameter : IStateParameter<CornerGradient> {
    public static readonly CornerGradientParameter TopLeft = new("top-left", new StateParameter<CornerGradient>.Color(Lens<CornerGradient, Swatch>.New(static corners => corners.TopLeft, static color => corners => corners with { TopLeft = color })),
        new("Top left", "Display color at the top left corner"));
    public static readonly CornerGradientParameter TopRight = new("top-right", new StateParameter<CornerGradient>.Color(Lens<CornerGradient, Swatch>.New(static corners => corners.TopRight, static color => corners => corners with { TopRight = color })),
        new("Top right", "Display color at the top right corner"));
    public static readonly CornerGradientParameter BottomRight = new("bottom-right", new StateParameter<CornerGradient>.Color(Lens<CornerGradient, Swatch>.New(static corners => corners.BottomRight, static color => corners => corners with { BottomRight = color })),
        new("Bottom right", "Display color at the bottom right corner"));
    public static readonly CornerGradientParameter BottomLeft = new("bottom-left", new StateParameter<CornerGradient>.Color(Lens<CornerGradient, Swatch>.New(static corners => corners.BottomLeft, static color => corners => corners with { BottomLeft = color })),
        new("Bottom left", "Display color at the bottom left corner"));
    public static readonly CornerGradientParameter Layer = new("layer", new StateParameter<CornerGradient>.Record<GeneratedLayer>(Lens<CornerGradient, GeneratedLayer>.New(static corners => corners.Layer, static layer => corners => corners with { Layer = layer })),
        new("Layer", "Composition of the generated layer over the frame"));

    public StateParameter<CornerGradient> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record Grid(
    GridPattern Pattern, Placement Placement, ShortSideExtent Cell, PositiveMagnitude Aspect, AxisFraction Offset, ShortSideLength LineWidth,
    ShortSideLength Feather, Swatch LineColor, Swatch CellColor, Swatch AlternateColor, GeneratedLayer Layer, Seed Seed)
    : IStateRecord<Grid, GridParameter>, IPixelStage<Grid> {
    public static Grid Default { get; } = (Size: 3f, Thickness: 0.2f, Factor: 0.1f) switch {
        var (size, thickness, factor) => new(
            GridPattern.Lines, Placement.Default, ShortSideExtent.Create(1f / size), PositiveMagnitude.Unit, AxisFraction.MinValue,
            ShortSideLength.Create(2f * (0.02f + (0.18f * thickness)) * (0.1f + (0.4f * factor)) / size), ShortSideLength.Neutral,
            Swatch.White, Swatch.Black, Swatch.White, GeneratedLayer.Default, Seed.MinValue),
    };

    public static Option<PixelPass> Pass(Grid state, PassContext context) {
        Func<(double X, double Y), Vector4> shade = state.Pattern.Switch(
            (Lattice: new GridLattice(state, context.Extent), Cell: state.CellColor.Display.AsVector3(), Alternate: state.AlternateColor.Display.AsVector3(),
                Field: CoordinateHash.Field(NoiseStream.Grid, state.Seed, 0u)),
            lines: static at => at.Lattice.Lines,
            checker: static at => at.Lattice.Cells((i, j) => i.Add(j).IsEven ? at.Cell : at.Alternate),
            brick: static at => at.Lattice.Cells((i, j) => Vector3.Lerp(at.Cell, at.Alternate, CoordinateHash.Uniforms(i, j, at.Field).X)));
        return Some(state.Layer.Pass(state.Placement.Frame(context.Extent).Fill(shade)));
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GridParameter : IStateParameter<Grid> {
    private static readonly Presentation<float> Line = ShortSideLength.Presentation with { Soft = (0f, 0.05f) };

    public static readonly GridParameter Pattern = new("pattern", new StateParameter<Grid>.Choice<GridPattern>(Lens<Grid, GridPattern>.New(static grid => grid.Pattern, static pattern => grid => grid with { Pattern = pattern })),
        new("Pattern", "Lines alone or filled cells"));
    public static readonly GridParameter Placement = new("placement", new StateParameter<Grid>.Record<Placement>(Lens<Grid, Placement>.New(static grid => grid.Placement, static placement => grid => grid with { Placement = placement })),
        new("Placement", "Position and turn of the field"));
    public static readonly GridParameter Cell = new("cell", StateParameter.Bounded(Lens<Grid, ShortSideExtent>.New(static grid => grid.Cell, static cell => grid => grid with { Cell = cell }), ShortSideExtent.Presentation with { Soft = (0.01f, 1f) }),
        new("Cell", "Cell height as a share of the frame's short side"));
    public static readonly GridParameter Aspect = new("aspect", StateParameter.Bounded(Lens<Grid, PositiveMagnitude>.New(static grid => grid.Aspect, static aspect => grid => grid with { Aspect = aspect }), Tracks.Aspect),
        new("Aspect", "Cell width over cell height"));
    public static readonly GridParameter Offset = new("offset", StateParameter.Bounded(Lens<Grid, AxisFraction>.New(static grid => grid.Offset, static offset => grid => grid with { Offset = offset })),
        new("Brick offset", "Share of a cell every second brick row shifts by"));
    public static readonly GridParameter LineWidth = new("line-width", StateParameter.Bounded(Lens<Grid, ShortSideLength>.New(static grid => grid.LineWidth, static width => grid => grid with { LineWidth = width }), Line),
        new("Line width", "Line width as a share of the frame's short side"));
    public static readonly GridParameter Feather = new("feather", StateParameter.Bounded(Lens<Grid, ShortSideLength>.New(static grid => grid.Feather, static feather => grid => grid with { Feather = feather }), Line),
        new("Feather", "Edge softness as a share of the frame's short side, 0% one pixel wide"));
    public static readonly GridParameter LineColor = new("line-color", new StateParameter<Grid>.Color(Lens<Grid, Swatch>.New(static grid => grid.LineColor, static color => grid => grid with { LineColor = color })),
        new("Line color", "Display color of the lines"));
    public static readonly GridParameter CellColor = new("cell-color", new StateParameter<Grid>.Color(Lens<Grid, Swatch>.New(static grid => grid.CellColor, static color => grid => grid with { CellColor = color })),
        new("Cell color", "Display color of the cells"));
    public static readonly GridParameter AlternateColor = new("alternate-color", new StateParameter<Grid>.Color(Lens<Grid, Swatch>.New(static grid => grid.AlternateColor, static color => grid => grid with { AlternateColor = color })),
        new("Alternate color", "Display color of every other checker cell and the far end of the brick tints"));
    public static readonly GridParameter Layer = new("layer", new StateParameter<Grid>.Record<GeneratedLayer>(Lens<Grid, GeneratedLayer>.New(static grid => grid.Layer, static layer => grid => grid with { Layer = layer })),
        new("Layer", "Composition of the generated layer over the frame"));
    public static readonly GridParameter Seed = new("seed", StateParameter.Bounded(Lens<Grid, Seed>.New(static grid => grid.Seed, static seed => grid => grid with { Seed = seed })),
        new("Seed", "Number selecting the field's pattern"));

    public StateParameter<Grid> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record Stripes(
    WaveProfile Profile, Placement Placement, ShortSideExtent Wavelength, SignedAngle Bend, AxisFraction Phase, Frequency Speed, Ramp Colors, GeneratedLayer Layer, Timing Timing)
    : IStateRecord<Stripes, StripesParameter>, IPixelStage<Stripes> {
    public static Stripes Default { get; } = new(
        WaveProfile.Sine, Placement.Default, ShortSideExtent.Create(float.Tau / 20f), SignedAngle.Neutral, AxisFraction.MinValue, Frequency.Neutral, Ramp.Grayscale,
        GeneratedLayer.Default, Timing.Default);

    public static Option<PixelPass> Pass(Stripes state, PassContext context) {
        (RampFootprint footprint, (double sin, double cos), ERational start, ERational wavelength) = (RampFootprint.Of((state.Colors, Some(state.Profile))), double.SinCos(state.Bend), ERational.FromSingle(state.Phase).Add(ERational.FromSingle(state.Speed).Multiply(ERational.FromSingle(state.Timing.At(context)))), ERational.FromSingle(state.Wavelength));
        ERational width = ERational.One.Divide(wavelength.Multiply(context.Extent.ShortSide));
        return Some(state.Layer.Pass(state.Placement.Frame(context.Extent).Fill(point =>
            footprint.Mean(ERational.FromDouble(point.Y).Multiply(ERational.FromDouble(cos)).Add(ERational.FromDouble(double.Abs(point.X)).Multiply(ERational.FromDouble(sin))).Divide(wavelength).Add(start), width, periodic: true))));
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class StripesParameter : IStateParameter<Stripes> {
    public static readonly StripesParameter Profile = new("profile", new StateParameter<Stripes>.Choice<WaveProfile>(Lens<Stripes, WaveProfile>.New(static stripes => stripes.Profile, static profile => stripes => stripes with { Profile = profile })),
        new("Profile", "Level across each stripe period"));
    public static readonly StripesParameter Placement = new("placement", new StateParameter<Stripes>.Record<Placement>(Lens<Stripes, Placement>.New(static stripes => stripes.Placement, static placement => stripes => stripes with { Placement = placement })),
        new("Placement", "Position and turn of the field"));
    public static readonly StripesParameter Wavelength = new("wavelength", StateParameter.Bounded(Lens<Stripes, ShortSideExtent>.New(static stripes => stripes.Wavelength, static wavelength => stripes => stripes with { Wavelength = wavelength }), ShortSideExtent.Presentation with { Soft = (0.01f, 1f) }),
        new("Wavelength", "Period of the stripes as a share of the frame's short side"));
    public static readonly StripesParameter Bend = new("bend", StateParameter.Bounded(Lens<Stripes, SignedAngle>.New(static stripes => stripes.Bend, static bend => stripes => stripes with { Bend = bend })),
        new("Bend", "Fold of the stripes about the center line, 0 straight bands"));
    public static readonly StripesParameter Phase = new("phase", StateParameter.Bounded(Lens<Stripes, AxisFraction>.New(static stripes => stripes.Phase, static phase => stripes => stripes with { Phase = phase })),
        new("Phase", "Share of a period the stripes shift by"));
    public static readonly StripesParameter Speed = new("speed", StateParameter.Bounded(Lens<Stripes, Frequency>.New(static stripes => stripes.Speed, static speed => stripes => stripes with { Speed = speed }), Frequency.Presentation with { Soft = (-4f, 4f) }),
        new("Speed", "Periods per second the stripes scroll"));
    public static readonly StripesParameter Colors = new("colors", new StateParameter<Stripes>.Gradient(Lens<Stripes, Ramp>.New(static stripes => stripes.Colors, static colors => stripes => stripes with { Colors = colors })),
        new("Colors", "Display colors and coverage across one period"));
    public static readonly StripesParameter Layer = new("layer", new StateParameter<Stripes>.Record<GeneratedLayer>(Lens<Stripes, GeneratedLayer>.New(static stripes => stripes.Layer, static layer => stripes => stripes with { Layer = layer })),
        new("Layer", "Composition of the generated layer over the frame"));
    public static readonly StripesParameter Timing = new("timing", new StateParameter<Stripes>.Record<Timing>(Lens<Stripes, Timing>.New(static stripes => stripes.Timing, static timing => stripes => stripes with { Timing = timing })),
        new("Timing", "Stored time and rate of recorded frame time"));

    public StateParameter<Stripes> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record Letterbox(Gated<PositiveMagnitude> Aspect, ShortSideLength Inset, ShortSideLength Radius, ShapeStyle Style, GeneratedLayer Layer, Seed Seed)
    : IStateRecord<Letterbox, LetterboxParameter>, IPixelStage<Letterbox> {
    public static Letterbox Default { get; } = new(
        new(Enabled: true, PositiveMagnitude.Create(2.39f)), ShortSideLength.Neutral, ShortSideLength.Neutral, ShapeStyle.Default with { Fill = new(Enabled: true, Swatch.Black), Inverted = true }, GeneratedLayer.Default, Seed.MinValue);

    public static Letterbox FilmGate { get; } = (Padding: 40f, Radius: 40f, Blur: 10f, Intensity: 6f) switch {
        var (padding, radius, blur, intensity) => Default with {
            Aspect = Default.Aspect with { Enabled = false },
            Inset = ShortSideLength.Create(padding / ReferenceFrame.Height),
            Radius = ShortSideLength.Create(radius / ReferenceFrame.Height),
            Style = Default.Style with { Feather = ShortSideLength.Create(blur / 3f * float.Sqrt(float.Tau) / ReferenceFrame.Height), Roughness = ShortSideLength.Create(intensity / ReferenceFrame.Height) },
        },
    };

    public static Option<PixelPass> Pass(Letterbox state, PassContext context) {
        EContext math = ShapeFields.Precision(context.Extent);
        (double X, double Y) frame = ((double)context.Extent.Width / context.Extent.ShortSide, (double)context.Extent.Height / context.Extent.ShortSide);
        (double X, double Y) = state.Aspect.Active.Map(aspect => aspect >= frame.X / frame.Y ? (frame.X, frame.X / aspect) : (frame.Y * aspect, frame.Y)).IfNone(frame) switch { var (x, y) => ((x / 2d) - state.Inset, (y / 2d) - state.Inset) };
        double small = double.Min(X, Y);
        double radius = double.Min(state.Radius, small);
        Func<FieldSample, (double X, double Y), Vector4> shade = state.Style.Shading(context, CoordinateHash.Field(NoiseStream.Shape, state.Seed, 0u), math);
        return Some(state.Layer.Pass(Placement.Default.Frame(context.Extent).Fill(point =>
            shade(small <= 0d ? new(double.PositiveInfinity, 0d) : new(ShapeFields.Box(point, (X, Y), radius, math), 2d * small), point))));
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LetterboxParameter : IStateParameter<Letterbox> {
    private static readonly Presentation<float> Margin = ShortSideLength.Presentation with { Soft = (0f, 0.1f * ReferenceFrame.GreaterSide) };

    public static readonly LetterboxParameter Aspect = new("aspect", StateParameter.OptionalBounded(Lens<Letterbox, Gated<PositiveMagnitude>>.New(static bars => bars.Aspect, static aspect => bars => bars with { Aspect = aspect }), Tracks.Aspect),
        new("Aspect", "Width over height of the window the bars leave, off following the full frame"));
    public static readonly LetterboxParameter Inset = new("inset", StateParameter.Bounded(Lens<Letterbox, ShortSideLength>.New(static bars => bars.Inset, static inset => bars => bars with { Inset = inset }), Margin),
        new("Inset", "Margin inside the window as a share of the frame's short side"));
    public static readonly LetterboxParameter Radius = new("radius", StateParameter.Bounded(Lens<Letterbox, ShortSideLength>.New(static bars => bars.Radius, static radius => bars => bars with { Radius = radius }), Margin),
        new("Roundness", "Radius of the window's corners as a share of the frame's short side"));
    public static readonly LetterboxParameter Style = new("style", new StateParameter<Letterbox>.Record<ShapeStyle>(Lens<Letterbox, ShapeStyle>.New(static bars => bars.Style, static style => bars => bars with { Style = style })),
        new("Style", "Fill and boundary of the generated figure"));
    public static readonly LetterboxParameter Layer = new("layer", new StateParameter<Letterbox>.Record<GeneratedLayer>(Lens<Letterbox, GeneratedLayer>.New(static bars => bars.Layer, static layer => bars => bars with { Layer = layer })),
        new("Layer", "Composition of the generated layer over the frame"));
    public static readonly LetterboxParameter Seed = new("seed", StateParameter.Bounded(Lens<Letterbox, Seed>.New(static bars => bars.Seed, static seed => bars => bars with { Seed = seed })),
        new("Seed", "Number selecting the field's pattern"));

    public StateParameter<Letterbox> Kind { get; }
    public ParameterText Text { get; }
}

file sealed record GridLattice(ERational Height, ERational Span, ERational Shift, double Width, double Band, Vector3 Line) {
    public GridLattice(Grid state, PixelExtent extent)
        : this(ERational.FromSingle(state.Cell), ERational.FromSingle(state.Cell).Multiply(ERational.FromSingle(state.Aspect)), ERational.FromSingle(state.Offset), state.LineWidth, ShapeEdge.Band(state.Feather, extent) / extent.ShortSide, state.LineColor.Display.AsVector3()) { }

    public Vector4 Lines((double X, double Y) point) => new(Line, ShapeEdge.Strip(Locate(point).Side, Width, Band));

    public Func<(double X, double Y), Vector4> Cells(Func<EInteger, EInteger, Vector3> color) => point => Locate(point) switch {
        var (i, j, side, acrossI, acrossJ) => new Vector4(Vector3.Lerp(Vector3.Lerp(color(i, j), color(acrossI, acrossJ), ShapeEdge.Coverage(side, Band)), Line, ShapeEdge.Strip(side, Width, Band)), 1f),
    };

    private (EInteger I, EInteger J, double Side, EInteger AcrossI, EInteger AcrossJ) Locate((double X, double Y) point) {
        ERational across = ERational.FromDouble(point.X);
        (EInteger j, ERational vertical) = NoiseFunctions.Split(ERational.FromDouble(point.Y).Divide(Height));
        (EInteger i, ERational horizontal) = NoiseFunctions.Split(across.Divide(Span).Add(Offset(j)));
        (double x, double y, double right, double bottom) = (horizontal.Multiply(Span).ToDouble(), vertical.Multiply(Height).ToDouble(), ERational.One.Subtract(horizontal).Multiply(Span).ToDouble(), ERational.One.Subtract(vertical).Multiply(Height).ToDouble());
        double side = double.Min(double.Min(x, right), double.Min(y, bottom));
        return side == x ? (i, j, side, i.Subtract(1), j) : side == right ? (i, j, side, i.Add(1), j) : side == y ? (i, j, side, Column(across, j.Subtract(1)), j.Subtract(1)) : (i, j, side, Column(across, j.Add(1)), j.Add(1));
    }

    private EInteger Column(ERational x, EInteger j) => NoiseFunctions.Split(x.Divide(Span).Add(Offset(j))).Cell;

    private ERational Offset(EInteger j) => j.IsEven ? Shift : ERational.Zero;
}

file readonly record struct BoltPiece((double X, double Y) Start, (double X, double Y) End, double From, double To) {
    public FieldSample Sample((double X, double Y) point, EContext math) =>
        ShapeFields.Offset((point.X, point.Y), (Start.X, Start.Y), (End.X, End.Y), math) switch {
            var (offset, along) =>
            (EFloat.FromDouble(From) + ((EFloat.FromDouble(To) - From) * along)) switch { var radius => new(ShapeFields.Length(offset, math) - radius, 2 * radius) },
        };

    public static BoltPiece[] Path((double X, double Y) start, (double X, double Y) direction, double length, int levels, Lightning bolt, Func<int, float> draw, Func<double, double> radius) {
        int segments = 1 << levels;
        double[] offsets = new double[segments + 1];
        for (int level = 1; level <= levels; level++)
            for (int step = segments >> level, k = step; k < segments; k += 2 * step)
                offsets[k] = ((offsets[k - step] + offsets[k + step]) / 2d) + (level <= bolt.Depth ? bolt.Displacement * (2d * step * length / segments) * ((2f * draw(k)) - 1f) : 0d);
        (double X, double Y)[] vertices = [.. offsets.Select((offset, k) => (start.X + (k * length / segments * direction.X) - (offset * direction.Y), start.Y + (k * length / segments * direction.Y) + (offset * direction.X)))];
        return [.. Enumerable.Range(0, segments).Select(k => new BoltPiece(vertices[k], vertices[k + 1], radius(k / (double)segments), radius((k + 1d) / segments)))];
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class ShapeEdge {
    public static double Band(ShortSideLength feather, PixelExtent extent, double scale = 1d) => double.Max(feather.Pixels(extent) * scale, 1d);

    public static float Coverage(double distance, double band) => Easing.Saturate((float)(0.5d - (distance / band)));

    public static float Strip(double distance, double width, double band) =>
        (double.Min(width, band), double.Max(width, band)) switch {
            var (small, large) => (float)(double.Max(0d, double.Min(small, (small / 2d) - (double.Abs(distance) - (large / 2d)))) / band),
        };
}

internal static class ShapeFields {
    public static EContext Precision(PixelExtent extent) => EContext.Binary64.WithUnlimitedExponents().WithPrecision(
        double.ILogB((double)float.MaxValue * int.Max(extent.Width, extent.Height)) - double.ILogB((double)float.Epsilon / extent.ShortSide) + EContext.Binary64.Precision.ToInt32Checked());

    public static EFloat Length((EFloat X, EFloat Y) point, EContext math) => ((point.X * point.X) + (point.Y * point.Y)).Sqrt(math);

    public static (EFloat Level, EFloat Slope) Level((EFloat X, EFloat Y) point, (EFloat X, EFloat Y) extent, EFloat exponent, EContext math) {
        EFloat length = Length(point, math);
        (EFloat x, EFloat y) = length.Sign > 0 ? (point.X.Divide(length, math).Abs().Divide(extent.X, math), point.Y.Divide(length, math).Abs().Divide(extent.Y, math)) : (0, EFloat.One.Divide(extent.Y, math));
        EFloat largest = EFloat.Max(x, y);
        EFloat level = largest * (x.Divide(largest, math).Pow(exponent, math) + y.Divide(largest, math).Pow(exponent, math)).Pow(EFloat.One.Divide(exponent, math), math);
        return (length * level, exponent.CompareTo(EFloat.One) is 0
            ? Length((EFloat.One.Divide(extent.X, math), EFloat.One.Divide(extent.Y, math)), math)
            : Length((x.Divide(level, math).Pow(exponent - 1, math).Divide(extent.X, math), y.Divide(level, math).Pow(exponent - 1, math).Divide(extent.Y, math)), math));
    }

    public static EFloat Ellipse((EFloat X, EFloat Y) point, (EFloat X, EFloat Y) extent, EFloat exponent, EContext math) => Level(point, extent, exponent, math) switch { var (level, slope) => (level - 1).Divide(slope, math) };

    public static EFloat Box((EFloat X, EFloat Y) point, (EFloat X, EFloat Y) extent, EFloat radius, EContext math) =>
        (X: point.X.Abs() - extent.X + radius, Y: point.Y.Abs() - extent.Y + radius) switch {
            var corner => Length((EFloat.Max(corner.X, 0), EFloat.Max(corner.Y, 0)), math) + EFloat.Min(EFloat.Max(corner.X, corner.Y), 0) - radius,
        };

    public static ((EFloat X, EFloat Y) Offset, EFloat Along) Offset((EFloat X, EFloat Y) point, (EFloat X, EFloat Y) start, (EFloat X, EFloat Y) end, EContext math) =>
        (Edge: (X: end.X - start.X, Y: end.Y - start.Y), Reach: (X: point.X - start.X, Y: point.Y - start.Y)) switch {
            var (edge, reach) => ((edge.X * edge.X) + (edge.Y * edge.Y)) switch { var squared when squared.IsZero => EFloat.Zero, var squared => EFloat.Max(0, EFloat.Min(1, ((reach.X * edge.X) + (reach.Y * edge.Y)).Divide(squared, math))) } switch { var along => ((reach.X - (edge.X * along), reach.Y - (edge.Y * along)), along) },
        };

    public static EFloat Side((EFloat X, EFloat Y) point, (EFloat X, EFloat Y) start, (EFloat X, EFloat Y) end) =>
        ((end.X - start.X) * (point.Y - start.Y)) - ((end.Y - start.Y) * (point.X - start.X));
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

    public Vector4 Mean(double position, double footprint, bool periodic) =>
        double.IsPositiveInfinity(footprint) ? Color(sums[Bins] / Bins) : Mean(ERational.FromDouble(position), ERational.FromDouble(footprint), periodic);

    public Vector4 Mean(ERational position, ERational footprint, bool periodic) {
        (ERational low, ERational high, ERational total) = (position.Subtract(footprint.Divide(2)), position.Add(footprint.Divide(2)), footprint.Multiply(Bins));
        Vector4 mean;
        if (periodic) {
            (EInteger periods, ERational remainder) = NoiseFunctions.Split(footprint);
            ERational start = NoiseFunctions.Split(low).Local;
            ERational end = start.Add(remainder);
            mean = (sums[Bins] * ERational.FromEInteger(periods).Divide(total).ToSingle())
                + Segment(start.Multiply(Bins), ERational.Min(end, ERational.One).Multiply(Bins), total)
                + (end.CompareTo(ERational.One) > 0 ? Segment(ERational.Zero, end.Subtract(ERational.One).Multiply(Bins), total) : Vector4.Zero);
        } else {
            mean = (low.Sign < 0 ? bins[0] * ERational.Min(ERational.Zero, high).Subtract(low).Divide(footprint).ToSingle() : Vector4.Zero)
                + (high.CompareTo(ERational.One) > 0 ? bins[Bins - 1] * high.Subtract(ERational.Max(low, ERational.One)).Divide(footprint).ToSingle() : Vector4.Zero)
                + Segment(ERational.Max(low, ERational.Zero).Multiply(Bins), ERational.Min(high, ERational.One).Multiply(Bins), total);
        }
        return Color(mean);
    }

    private static Vector4 Color(Vector4 mean) => mean switch {
        var color when color.W > 0f => new Vector4(color.AsVector3() / color.W, color.W),
        _ => Vector4.Zero,
    };

    private Vector4 Segment(ERational low, ERational high, ERational total) {
        if (low.CompareTo(high) >= 0) return Vector4.Zero;
        (int first, int last) = (NoiseFunctions.Split(low).Cell.ToInt32Checked(), NoiseFunctions.Split(high).Cell.ToInt32Checked());
        ERational next = ERational.FromInt32(first + 1);
        return (bins[first] * ERational.Min(high, next).Subtract(low).Divide(total).ToSingle())
            + (last > first + 1 ? (sums[last] - sums[first + 1]) * ERational.One.Divide(total).ToSingle() : Vector4.Zero)
            + (last > first && high.CompareTo(last) > 0 ? bins[last] * high.Subtract(last).Divide(total).ToSingle() : Vector4.Zero);
    }
}
