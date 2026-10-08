using System.Drawing;
using Rhino.Display;
using Rhino.DocObjects;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Display.Feedback;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct DrawSize : System.Numerics.IMinMaxValue<DrawSize> {
    public static DrawSize MinValue { get; } = new(0f);
    public static DrawSize MaxValue { get; } = new(float.MaxValue);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct MaterialFactor : System.Numerics.IMinMaxValue<MaterialFactor> {
    public static MaterialFactor MinValue { get; } = new(0.0);
    public static MaterialFactor MaxValue { get; } = new(1.0);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref double value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

[ComplexValueObject]
[ValidationError<InvalidRhinoValue>]
public sealed partial class DashPattern {
    public const int MaxLengths = 8;
    public Seq<DrawSize> Lengths { get; }
    public Option<float> Offset { get; }
    public DrawSize Scale { get; }
    public bool BySegment { get; }
    public bool Autoscale { get; }
    public bool LengthInWorldUnits { get; }

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref Seq<DrawSize> lengths, ref Option<float> offset, ref DrawSize scale, ref bool bySegment, ref bool autoscale, ref bool lengthInWorldUnits) =>
        validationError = lengths.Count is >= 1 and <= MaxLengths ? null : new InvalidRhinoValue();
}

[Union<DrawSize, Tapered>(MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class StrokeWidth {
    public sealed record Tapered(DrawSize Start, DrawSize End, Option<(float Position, DrawSize Thickness)> Waist);
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record Stroke {
    public sealed record Drawn(Ink Color, StrokeWidth Width, CoordinateSystem ThicknessSpace, LineCapStyle Cap, LineJoinStyle Join, Option<DashPattern> Pattern, Option<(Ink Color, DrawSize Thickness)> Halo) : Stroke;
    public sealed record FromLinetype(Linetype Linetype, Ink Color, DrawSize Scale, Option<(Ink Color, DrawSize Thickness)> Halo) : Stroke;
}

public sealed record ShadedFace(Ink Diffuse, Ink Specular, Ink Emission, MaterialFactor Shine, MaterialFactor Transparency);

[Union<Material, Faces>(MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class ShadedMaterial {
    public sealed record Faces(ShadedFace Front, Option<ShadedFace> Back);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Source)]
public static partial class Strokes {
    // --- [PENS]
    public static DisplayPen Pen(Stroke stroke) {
        Point2f absentWaist = new(0.5f, -1f);
        (DisplayPen? pen, Option<Stroke.Drawn> details, Option<(Ink Color, DrawSize Thickness)> halo) = stroke.Switch(
            drawn: static drawn => (Map((drawn.Color, drawn.ThicknessSpace, drawn.Cap, drawn.Join)), Some(drawn), drawn.Halo),
            fromLinetype: static linetype => (DisplayPen.FromLinetype(linetype.Linetype, linetype.Color.Drawn, linetype.Scale), Option<Stroke.Drawn>.None, linetype.Halo));
        _ = details.Iter(drawn => {
            drawn.Width.Switch((Pen: pen, AbsentWaist: absentWaist),
                drawSize: static (state, size) => Map(size, state.Pen),
                tapered: static (state, tapered) => state.Pen.SetTaper(tapered.Start, tapered.End, tapered.Waist.Map(Map).IfNone(state.AbsentWaist)));
            _ = drawn.Pattern.Iter(pattern => {
                pen.SetPattern(pattern.Lengths.Map(Scalar));
                Map((pattern.Offset, pattern.Scale, pattern.BySegment, pattern.Autoscale, pattern.LengthInWorldUnits), pen);
            });
        });
        _ = halo.Iter(outline => Map(outline, pen));
        return pen;
    }

    private static partial DisplayPen Map((Ink Color, CoordinateSystem ThicknessSpace, LineCapStyle CapStyle, LineJoinStyle JoinStyle) source);
    private static partial Point2f Map((float X, DrawSize Y) source);
    private static partial void Map((Ink HaloColor, DrawSize HaloThickness) source, DisplayPen target);
    private static partial void Map((Option<float> PatternOffset, DrawSize PatternScale, bool PatternBySegment, bool PatternAutoscale, bool PatternLengthInWorldUnits) source, DisplayPen target);

    [MapPropertyFromSource(nameof(DisplayPen.Thickness))]
    private static partial void Map(DrawSize source, DisplayPen target);

    // --- [MATERIALS]
    public static IO<Unit> Use(ShadedMaterial material, Action<DisplayMaterial> draw) =>
        IO.lift(() => material.Switch(
                material: static source => (Material: new DisplayMaterial(source), Faces: Option<ShadedMaterial.Faces>.None),
                faces: static faces => (Material: new DisplayMaterial(), Faces: Some(faces))))
            .Bracket(
                Use: held => IO.lift(() => {
                    _ = held.Faces.Iter(faces => Map(faces.Front, held.Material));
                    _ = held.Faces.Bind(static faces => faces.Back).Iter(back => Map((back, true), held.Material));
                    draw(held.Material);
                }),
                Fin: static held => IO.lift(held.Material.Dispose));

    private static partial void Map(ShadedFace source, DisplayMaterial target);
    private static partial void Map((ShadedFace Back, bool IsTwoSided) source, DisplayMaterial target);

    // --- [CONVERSIONS]
    [UserMapping]
    private static Color Drawn(Ink source) => source.Drawn;

    [UserMapping]
    private static float Scalar(DrawSize source) => source;

    [UserMapping]
    private static double Scalar(MaterialFactor source) => source;
}
