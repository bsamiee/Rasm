using System.Drawing;
using Rasm.Rhino.Document;
using Rhino.Display;

namespace Rasm.Rhino.Display;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FrameBufferFill {
    public sealed record DefaultColor() : FrameBufferFill;

    public sealed record SolidColor(Color Color) : FrameBufferFill;

    public sealed record Gradient2Color(Color Top, Color Bottom) : FrameBufferFill;

    public sealed record Gradient4Color(Color TopLeft, Color BottomLeft, Color TopRight, Color BottomRight) : FrameBufferFill;

    public sealed record Bitmap() : FrameBufferFill;

    public sealed record Renderer() : FrameBufferFill;

    public sealed record Transparent() : FrameBufferFill;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class DisplayAttributes {
    public static IO<Unit> Write(DisplayPipelineAttributes target, FrameBufferFill fill) =>
        IO.lift(() => fill.Switch(
            target,
            defaultColor: static (attributes, _) => attributes.FillMode = DisplayPipelineAttributes.FrameBufferFillMode.DefaultColor,
            solidColor: static (attributes, solid) => {
                attributes.FillMode = DisplayPipelineAttributes.FrameBufferFillMode.SolidColor;
                attributes.SetFill(solid.Color);
            },
            gradient2Color: static (attributes, gradient) => {
                attributes.FillMode = DisplayPipelineAttributes.FrameBufferFillMode.Gradient2Color;
                attributes.SetFill(gradient.Top, gradient.Bottom);
            },
            gradient4Color: static (attributes, gradient) => {
                attributes.FillMode = DisplayPipelineAttributes.FrameBufferFillMode.Gradient4Color;
                attributes.SetFill(gradient.TopLeft, gradient.BottomLeft, gradient.TopRight, gradient.BottomRight);
            },
            bitmap: static (attributes, _) => attributes.FillMode = DisplayPipelineAttributes.FrameBufferFillMode.Bitmap,
            renderer: static (attributes, _) => attributes.FillMode = DisplayPipelineAttributes.FrameBufferFillMode.Renderer,
            transparent: static (attributes, _) => attributes.FillMode = DisplayPipelineAttributes.FrameBufferFillMode.Transparent));

    public static IO<FrameBufferFill> ReadFill(DisplayPipelineAttributes source) =>
        IO.lift<FrameBufferFill>(() => {
            source.GetFill(out Color topLeft, out Color bottomLeft, out Color topRight, out Color bottomRight);
            return source.FillMode switch {
                DisplayPipelineAttributes.FrameBufferFillMode.DefaultColor => new FrameBufferFill.DefaultColor(),
                DisplayPipelineAttributes.FrameBufferFillMode.SolidColor => new FrameBufferFill.SolidColor(topLeft),
                DisplayPipelineAttributes.FrameBufferFillMode.Gradient2Color => new FrameBufferFill.Gradient2Color(topLeft, bottomLeft),
                DisplayPipelineAttributes.FrameBufferFillMode.Gradient4Color => new FrameBufferFill.Gradient4Color(topLeft, bottomLeft, topRight, bottomRight),
                DisplayPipelineAttributes.FrameBufferFillMode.Bitmap => new FrameBufferFill.Bitmap(),
                DisplayPipelineAttributes.FrameBufferFillMode.Renderer => new FrameBufferFill.Renderer(),
                DisplayPipelineAttributes.FrameBufferFillMode.Transparent => new FrameBufferFill.Transparent(),
                _ => new Invalid(nameof(DisplayPipelineAttributes.FillMode)),
            };
        });
}
