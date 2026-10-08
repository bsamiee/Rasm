using Rhino.Display;
using Rhino.DocObjects;

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

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Global", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct PatternScale : System.Numerics.IMinMaxValue<PatternScale> {
    public static PatternScale MinValue { get; } = new(0f);
    public static PatternScale MaxValue { get; } = new(float.MaxValue);

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

    public PatternScale Scale { get; }

    public bool BySegment { get; }

    public bool Autoscale { get; }

    public bool LengthInWorldUnits { get; }

    static partial void ValidateFactoryArguments(
        ref InvalidRhinoValue? validationError,
        ref Seq<DrawSize> lengths,
        ref Option<float> offset,
        ref PatternScale scale,
        ref bool bySegment,
        ref bool autoscale,
        ref bool lengthInWorldUnits) =>
        validationError = lengths.Count is >= 1 and <= MaxLengths ? null : new InvalidRhinoValue();
}

[Union]
public abstract partial record StrokeWidth {
    public sealed record Uniform(DrawSize Thickness) : StrokeWidth;

    public sealed record Tapered(DrawSize Start, DrawSize End, Option<(float Position, DrawSize Thickness)> Waist) : StrokeWidth;
}

[Union]
public abstract partial record Stroke {
    private Stroke(Ink color, Option<(Ink Color, DrawSize Thickness)> halo) => (Color, Halo) = (color, halo);

    public Ink Color { get; }

    public Option<(Ink Color, DrawSize Thickness)> Halo { get; }

    public sealed record Drawn(
        Ink Color,
        StrokeWidth Width,
        CoordinateSystem ThicknessSpace,
        LineCapStyle Cap,
        LineJoinStyle Join,
        Option<DashPattern> Pattern,
        Option<(Ink Color, DrawSize Thickness)> Halo) : Stroke(Color, Halo);

    public sealed record FromLinetype(Linetype Linetype, Ink Color, PatternScale Scale, Option<(Ink Color, DrawSize Thickness)> Halo) : Stroke(Color, Halo);
}

public sealed record ShadedFace(Ink Diffuse, Ink Specular, Ink Emission, MaterialFactor Shine, MaterialFactor Transparency);

[Union]
public abstract partial record ShadedMaterial {
    public sealed record Faces(ShadedFace Front, Option<ShadedFace> Back) : ShadedMaterial;

    public sealed record DocumentMaterial(Material Material) : ShadedMaterial;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Strokes {
    // --- [PENS]
    public static DisplayPen Pen(Stroke stroke) {
        DisplayPen pen = stroke.Switch(
            drawn: DrawnPen,
            fromLinetype: static linetype => DisplayPen.FromLinetype(linetype.Linetype, linetype.Color.Drawn, linetype.Scale));
        _ = stroke.Halo.Iter(halo => {
            pen.HaloColor = halo.Color.Drawn;
            pen.HaloThickness = halo.Thickness;
        });
        return pen;
    }

    private static DisplayPen DrawnPen(Stroke.Drawn drawn) {
        DisplayPen pen = new() { Color = drawn.Color.Drawn, ThicknessSpace = drawn.ThicknessSpace, CapStyle = drawn.Cap, JoinStyle = drawn.Join };
        drawn.Width.Switch(
            pen,
            uniform: static (target, uniform) => target.Thickness = uniform.Thickness,
            tapered: static (target, tapered) => target.SetTaper(
                tapered.Start,
                tapered.End,
                tapered.Waist.Match(Some: static waist => new Point2f(waist.Position, waist.Thickness), None: static () => new Point2f(0.5f, -1f))));
        _ = drawn.Pattern.Iter(pattern => {
            pen.SetPattern(pattern.Lengths.Map(static length => length.ToValue()));
            pen.PatternOffset = Conversions.Unset(pattern.Offset);
            pen.PatternScale = pattern.Scale;
            pen.PatternBySegment = pattern.BySegment;
            pen.PatternAutoscale = pattern.Autoscale;
            pen.PatternLengthInWorldUnits = pattern.LengthInWorldUnits;
        });
        return pen;
    }

    // --- [MATERIALS]
    public static IO<Unit> Use(ShadedMaterial material, Action<DisplayMaterial> draw) =>
        use(() => material.Switch(faces: Faced, documentMaterial: static document => new DisplayMaterial(document.Material)))
            .Bind(shaded => IO.lift(() => draw(shaded)))
            .Bracket();

    private static DisplayMaterial Faced(ShadedMaterial.Faces faces) {
        ShadedFace front = faces.Front;
        DisplayMaterial shaded = new(front.Diffuse.Drawn, front.Transparency) { Specular = front.Specular.Drawn, Emission = front.Emission.Drawn, Shine = front.Shine };
        _ = faces.Back.Iter(back => {
            shaded.IsTwoSided = true;
            shaded.BackDiffuse = back.Diffuse.Drawn;
            shaded.BackSpecular = back.Specular.Drawn;
            shaded.BackEmission = back.Emission.Drawn;
            shaded.BackShine = back.Shine;
            shaded.BackTransparency = back.Transparency;
        });
        return shaded;
    }
}
