using System.Drawing;
using Rasm.Rhino.Document;
using Rhino.Display;
using Rhino.DocObjects;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Display;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record PatternPolicy(Seq<float> Lengths, float Offset, float Scale, bool BySegment, bool Autoscale, bool LengthInWorldUnits);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record StrokeWidth {
    public sealed record Uniform(float Thickness) : StrokeWidth;

    public sealed record Tapered(float Start, float End, Point2f TaperPoint) : StrokeWidth;
}

[ComplexValueObject]
[ValidationError<ValidationFailure>]
public sealed partial class Stroke {
    public const int MaximumPatternLength = 8;

    public Color Color { get; }

    public StrokeWidth Width { get; }

    public CoordinateSystem ThicknessSpace { get; }

    public LineCapStyle CapStyle { get; }

    public LineJoinStyle JoinStyle { get; }

    public Option<PatternPolicy> Pattern { get; }

    public Color HaloColor { get; }

    public float HaloThickness { get; }

    public static Fin<Stroke> From(
        Color color,
        StrokeWidth width,
        CoordinateSystem thicknessSpace,
        LineCapStyle capStyle,
        LineJoinStyle joinStyle,
        Option<PatternPolicy> pattern,
        Color haloColor,
        float haloThickness) =>
        Validate(color, width, thicknessSpace, capStyle, joinStyle, pattern, haloColor, haloThickness, out Stroke? stroke) is { } error ? error : stroke!;

    static partial void ValidateFactoryArguments(
        ref ValidationFailure? validationError,
        ref Color color,
        ref StrokeWidth width,
        ref CoordinateSystem thicknessSpace,
        ref LineCapStyle capStyle,
        ref LineJoinStyle joinStyle,
        ref Option<PatternPolicy> pattern,
        ref Color haloColor,
        ref float haloThickness) =>
        validationError = Answers.FirstInvalid(
            (width.Switch(
                uniform: static uniform => !Positive(uniform.Thickness),
                tapered: static tapered => !Positive(tapered.Start) || !Positive(tapered.End) || tapered.TaperPoint is not { X: >= 0f and <= 1f, Y: >= 0f }), nameof(Width)),
            (thicknessSpace is not (CoordinateSystem.World or CoordinateSystem.Screen), nameof(ThicknessSpace)),
            (pattern.Exists(static policy => policy.Lengths.IsEmpty || (policy.Lengths.Count > MaximumPatternLength) || policy.Lengths.Exists(static entry => !Positive(entry))), nameof(Pattern)),
            (haloThickness < 0f, nameof(HaloThickness)));

    private static bool Positive(float value) =>
        float.IsFinite(value) && (value > 0f);
}

[ComplexValueObject]
[ValidationError<ValidationFailure>]
public sealed partial class ShadedFace {
    public Color Diffuse { get; }

    public Color Specular { get; }

    public Color Emission { get; }

    public double Shine { get; }

    public double Transparency { get; }

    public static Fin<ShadedFace> From(Color diffuse, Color specular, Color emission, double shine, double transparency) =>
        Validate(diffuse, specular, emission, shine, transparency, out ShadedFace? face) is { } error ? error : face!;

    static partial void ValidateFactoryArguments(
        ref ValidationFailure? validationError,
        ref Color diffuse,
        ref Color specular,
        ref Color emission,
        ref double shine,
        ref double transparency) =>
        validationError = Limits.AtLeast(0.0).Violated(shine, nameof(Shine)) ?? Limits.AtLeast(0.0).AtMost(1.0).Violated(transparency, nameof(Transparency));
}

public sealed record ShadedMaterial(ShadedFace Front, Option<ShadedFace> Back);

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
internal static partial class StrokeMapper {
    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    [MapperIgnoreSource(nameof(Stroke.Width), Justification = "DisplayPen.Thickness or DisplayPen.SetTaper per StrokeWidth case")]
    [MapperIgnoreSource(nameof(Stroke.Pattern), Justification = "DisplayPen.SetPattern and the PatternPolicy update")]
    internal static partial DisplayPen ToPen(Stroke stroke);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    [MapperIgnoreSource(nameof(PatternPolicy.Lengths), Justification = "DisplayPen.SetPattern")]
    [MapProperty(nameof(PatternPolicy.Offset), nameof(DisplayPen.PatternOffset))]
    [MapProperty(nameof(PatternPolicy.Scale), nameof(DisplayPen.PatternScale))]
    [MapProperty(nameof(PatternPolicy.BySegment), nameof(DisplayPen.PatternBySegment))]
    [MapProperty(nameof(PatternPolicy.Autoscale), nameof(DisplayPen.PatternAutoscale))]
    [MapProperty(nameof(PatternPolicy.LengthInWorldUnits), nameof(DisplayPen.PatternLengthInWorldUnits))]
    internal static partial void Update(PatternPolicy pattern, DisplayPen pen);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    internal static partial DisplayMaterial ToMaterial(ShadedFace front);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    internal static partial void Update((ShadedFace Back, bool IsTwoSided) face, DisplayMaterial material);
}

public static class Strokes {
    // --- [PENS]
    public static DisplayPen ToPen(Stroke stroke) {
        DisplayPen pen = StrokeMapper.ToPen(stroke);
        stroke.Width.Switch(
            pen,
            uniform: static (target, uniform) => target.Thickness = uniform.Thickness,
            tapered: static (target, tapered) => target.SetTaper(tapered.Start, tapered.End, tapered.TaperPoint));
        _ = stroke.Pattern.Iter(pattern => {
            pen.SetPattern(pattern.Lengths);
            StrokeMapper.Update(pattern, pen);
        });
        return pen;
    }

    // --- [MATERIALS]
    public static IO<Unit> Use(ShadedMaterial material, Action<DisplayMaterial> draw) =>
        DisposalOps.Using(() => {
            DisplayMaterial shaded = StrokeMapper.ToMaterial(material.Front);
            _ = material.Back.Iter(back => StrokeMapper.Update((Back: back, IsTwoSided: true), shaded));
            return shaded;
        }, shaded => IO.lift(() => draw(shaded)));
}
