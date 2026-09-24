using System.Drawing;
using LanguageExt.UnsafeValueAccess;
using Rasm.Rhino.Document;
using Rhino.Display;
using Rhino.DocObjects;

namespace Rasm.Rhino.Display;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record PatternPolicy(Seq<float> Lengths, float Offset, float Scale, bool BySegment, bool Autoscale, bool LengthInWorldUnits);

public sealed record Halo(Color Color, float Thickness);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record StrokeWidth {
    public sealed record Uniform(float Thickness) : StrokeWidth;

    public sealed record Tapered(float Start, float End, Point2f TaperPoint) : StrokeWidth;
}

[ComplexValueObject]
[ValidationError<ValidationFailure>]
public sealed partial class Stroke {
    // --- [LIMITS]
    public const int MaximumPatternLength = 8;

    // --- [PROPERTIES]
    public Color Color { get; }

    public StrokeWidth Width { get; }

    public CoordinateSystem Space { get; }

    public LineCapStyle Cap { get; }

    public LineJoinStyle Join { get; }

    public Option<PatternPolicy> Pattern { get; }

    public Option<Halo> Halo { get; }

    // --- [VALIDATION]
    public static Fin<Stroke> From(Color color, StrokeWidth width, CoordinateSystem space, LineCapStyle cap, LineJoinStyle join, Option<PatternPolicy> pattern, Option<Halo> halo) =>
        Validate(color, width, space, cap, join, pattern, halo, out Stroke? stroke) is { } error ? error : stroke!;

    static partial void ValidateFactoryArguments(
        ref ValidationFailure? validationError,
        ref Color color,
        ref StrokeWidth width,
        ref CoordinateSystem space,
        ref LineCapStyle cap,
        ref LineJoinStyle join,
        ref Option<PatternPolicy> pattern,
        ref Option<Halo> halo) =>
        validationError = Seq(
                (Failed: width.Switch(
                    uniform: static uniform => !Positive(uniform.Thickness),
                    tapered: static tapered => !Positive(tapered.Start) || !Positive(tapered.End) || tapered.TaperPoint is not { X: >= 0f and <= 1f, Y: >= 0f }), Member: nameof(Width)),
                (Failed: space is not (CoordinateSystem.World or CoordinateSystem.Screen), Member: nameof(Space)),
                (Failed: pattern.Exists(static policy => policy.Lengths.IsEmpty || (policy.Lengths.Count > MaximumPatternLength) || policy.Lengths.Exists(static entry => !Positive(entry))), Member: nameof(Pattern)),
                (Failed: halo.Exists(static glow => glow.Thickness < 0f), Member: nameof(Halo)))
            .Find(static rule => rule.Failed)
            .Map(static rule => new Invalid(rule.Member)).ValueUnsafe();

    private static bool Positive(float value) =>
        float.IsFinite(value) && (value > 0f);
}

[ComplexValueObject]
[ValidationError<ValidationFailure>]
public sealed partial class ShadedFace {
    public Color Diffuse { get; }

    public Color Specular { get; }

    public Color Ambient { get; }

    public Color Emission { get; }

    public double Shine { get; }

    public double Transparency { get; }

    public static Fin<ShadedFace> From(Color diffuse, Color specular, Color ambient, Color emission, double shine, double transparency) =>
        Validate(diffuse, specular, ambient, emission, shine, transparency, out ShadedFace? face) is { } error ? error : face!;

    static partial void ValidateFactoryArguments(
        ref ValidationFailure? validationError,
        ref Color diffuse,
        ref Color specular,
        ref Color ambient,
        ref Color emission,
        ref double shine,
        ref double transparency) =>
        validationError = Seq(
                (Failed: !double.IsFinite(shine) || (shine < 0.0), Member: nameof(Shine)),
                (Failed: !double.IsFinite(transparency) || (transparency < 0.0) || (transparency > 1.0), Member: nameof(Transparency)))
            .Find(static rule => rule.Failed)
            .Map(static rule => new Invalid(rule.Member)).ValueUnsafe();
}

public sealed record ShadedMaterial(ShadedFace Front, Option<ShadedFace> Back);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Strokes {
    // --- [PENS]
    public static DisplayPen ToPen(Stroke stroke) {
        DisplayPen pen = new() {
            Color = stroke.Color,
            ThicknessSpace = stroke.Space,
            CapStyle = stroke.Cap,
            JoinStyle = stroke.Join,
        };
        stroke.Width.Switch(
            pen,
            uniform: static (target, uniform) => target.Thickness = uniform.Thickness,
            tapered: static (target, tapered) => target.SetTaper(tapered.Start, tapered.End, tapered.TaperPoint));
        _ = stroke.Halo.Iter(halo => {
            pen.HaloColor = halo.Color;
            pen.HaloThickness = halo.Thickness;
        });
        _ = stroke.Pattern.Iter(pattern => {
            pen.SetPattern(pattern.Lengths);
            pen.PatternOffset = pattern.Offset;
            pen.PatternScale = pattern.Scale;
            pen.PatternBySegment = pattern.BySegment;
            pen.PatternAutoscale = pattern.Autoscale;
            pen.PatternLengthInWorldUnits = pattern.LengthInWorldUnits;
        });
        return pen;
    }

    // --- [MATERIALS]
    public static IO<TValue> Use<TValue>(ShadedMaterial material, Func<DisplayMaterial, IO<TValue>> body) =>
        Disposal.Using(() => {
            DisplayMaterial shaded = new(material.Front.Diffuse, material.Front.Specular, material.Front.Ambient, material.Front.Emission, material.Front.Shine, material.Front.Transparency);
            _ = material.Back.Iter(back => {
                shaded.IsTwoSided = true;
                shaded.BackDiffuse = back.Diffuse;
                shaded.BackSpecular = back.Specular;
                shaded.BackEmission = back.Emission;
                shaded.BackShine = back.Shine;
                shaded.BackTransparency = back.Transparency;
            });
            return shaded;
        }, body);
}
