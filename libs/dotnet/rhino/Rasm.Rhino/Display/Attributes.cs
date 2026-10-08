using System.Drawing;
using Rhino.Display;

namespace Rasm.Rhino.Display;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Off", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct ObjectEffectAmount : System.Numerics.IMinMaxValue<ObjectEffectAmount> {
    // --- [LIMITS]
    public static ObjectEffectAmount MinValue { get; } = new(0f);
    public static ObjectEffectAmount MaxValue { get; } = new(1f);

    // --- [FACTORY]
    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct DiagonalHatchWidth : System.Numerics.IMinMaxValue<DiagonalHatchWidth> {
    // --- [LIMITS]
    public static DiagonalHatchWidth MinValue { get; } = new(0f);
    public static DiagonalHatchWidth MaxValue { get; } = new(float.MaxValue);

    // --- [FACTORY]
    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

public sealed record ObjectEffects(Color FadeColor, ObjectEffectAmount FadeAmount, ObjectEffectAmount DitherTransparency, ObjectEffectAmount HatchStrength, DiagonalHatchWidth HatchWidth) {
    public static Fin<ObjectEffects> FromHost(DisplayPipelineAttributes attributes) {
        attributes.GetColorFadeEffect(out Color fadeColor, out float fadeAmount);
        attributes.GetDiagonalHatchEffect(out float hatchStrength, out float hatchWidth);
        return (
                Conversions.Validated<ObjectEffectAmount, float, InvalidRhinoValue>(fadeAmount).ToValidation(),
                Conversions.Validated<ObjectEffectAmount, float, InvalidRhinoValue>(attributes.GetDitherTransparencyEffect()).ToValidation(),
                Conversions.Validated<ObjectEffectAmount, float, InvalidRhinoValue>(hatchStrength).ToValidation(),
                Conversions.Validated<DiagonalHatchWidth, float, InvalidRhinoValue>(hatchWidth).ToValidation())
            .Apply((fade, dither, strength, width) => new ObjectEffects(fadeColor, fade, dither, strength, width))
            .As()
            .ToFin();
    }

    public void Write(DisplayPipelineAttributes attributes) {
        attributes.SetColorFadeEffect(FadeColor, FadeAmount.ToValue());
        attributes.SetDitherTransparencyEffect(DitherTransparency.ToValue());
        attributes.SetDiagonalHatchEffect(HatchStrength.ToValue(), HatchWidth.ToValue());
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record FrameBufferFill {
    // --- [CASES]
    public sealed record DefaultColor() : FrameBufferFill;

    public sealed record SolidColor(Color Color) : FrameBufferFill;

    public sealed record Gradient2Color(Color Top, Color Bottom) : FrameBufferFill;

    public sealed record Gradient4Color(Color TopLeft, Color BottomLeft, Color TopRight, Color BottomRight) : FrameBufferFill;

    public sealed record BackgroundBitmap() : FrameBufferFill;

    public sealed record Renderer() : FrameBufferFill;

    public sealed record Transparent() : FrameBufferFill;

    // --- [ATTRIBUTES]
    public static FrameBufferFill FromHost(DisplayPipelineAttributes attributes) {
        attributes.GetFill(out Color topLeft, out Color bottomLeft, out Color topRight, out Color bottomRight);
        return attributes.FillMode switch {
            DisplayPipelineAttributes.FrameBufferFillMode.DefaultColor => new DefaultColor(),
            DisplayPipelineAttributes.FrameBufferFillMode.SolidColor => new SolidColor(topLeft),
            DisplayPipelineAttributes.FrameBufferFillMode.Gradient2Color => new Gradient2Color(topLeft, bottomLeft),
            DisplayPipelineAttributes.FrameBufferFillMode.Gradient4Color => new Gradient4Color(topLeft, bottomLeft, topRight, bottomRight),
            DisplayPipelineAttributes.FrameBufferFillMode.Bitmap => new BackgroundBitmap(),
            DisplayPipelineAttributes.FrameBufferFillMode.Renderer => new Renderer(),
            DisplayPipelineAttributes.FrameBufferFillMode.Transparent => new Transparent(),
        };
    }

    public void Write(DisplayPipelineAttributes attributes) =>
        Switch(
            attributes,
            defaultColor: static (target, _) => target.FillMode = DisplayPipelineAttributes.FrameBufferFillMode.DefaultColor,
            solidColor: static (target, solid) => target.SetFill(solid.Color),
            gradient2Color: static (target, gradient) => target.SetFill(gradient.Top, gradient.Bottom),
            gradient4Color: static (target, gradient) => target.SetFill(gradient.TopLeft, gradient.BottomLeft, gradient.TopRight, gradient.BottomRight),
            backgroundBitmap: static (target, _) => target.FillMode = DisplayPipelineAttributes.FrameBufferFillMode.Bitmap,
            renderer: static (target, _) => target.FillMode = DisplayPipelineAttributes.FrameBufferFillMode.Renderer,
            transparent: static (target, _) => target.FillMode = DisplayPipelineAttributes.FrameBufferFillMode.Transparent);
}
