using SkiaSharp;

namespace Rasm.UI.Styling;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.DefaultWithPartialOverloads)]
public abstract partial record Material {
    public virtual bool Timed => false;

    internal abstract Option<ColorValue> BaseColor { get; }

    internal virtual Option<Backdrop> AsBackdrop => None;

    internal virtual Option<float> Blur => None;

    internal Material Opaque => BaseColor.Map(static color => (Material)new Solid(color)).IfNone(this);

    public sealed record Solid(ColorValue Color) : Material {
        internal override Option<ColorValue> BaseColor => Some(Color);
    }

    public sealed record Gradient(Seq<Stop<ColorValue>> Stops, float Angle) : Material {
        private static readonly SKColorSpace Srgb = SKColorSpace.CreateSrgb();

        internal override Option<ColorValue> BaseColor => None;

        internal SKShader Shader(SKRect frame, Seq<SKColorF> colors) {
            (float sin, float cos) = MathF.SinCos(float.DegreesToRadians(Angle));
            float reach = (MathF.Abs(cos * frame.Width) + MathF.Abs(sin * frame.Height)) / 2f;
            SKPoint center = new(frame.MidX, frame.MidY);
            return SKShader.CreateLinearGradient(
                new SKPoint(center.X - (cos * reach), center.Y - (sin * reach)),
                new SKPoint(center.X + (cos * reach), center.Y + (sin * reach)),
                [.. colors],
                Srgb,
                [.. Stops.Map(static stop => (float)stop.At)],
                SKShaderTileMode.Clamp);
        }
    }

    public sealed record Drawn(SKRuntimeEffect Effect, HashMap<string, ColorValue> Uniforms) : Material {
        internal const string Size = "size";
        internal const string Clock = "time";

        public override bool Timed => Effect.Uniforms.Contains(Clock, StringComparer.Ordinal);

        internal override Option<ColorValue> BaseColor => None;

        internal SKShader Shader(HashMap<string, Option<SKColorF>> colors, SKRect frame, float clock) {
            using SKRuntimeEffectUniforms uniforms = new(Effect);
            using SKRuntimeEffectChildren children = new(Effect);
            if (uniforms.Contains(Size)) uniforms[Size] = frame.Size;
            if (uniforms.Contains(Clock)) uniforms[Clock] = clock;
            _ = colors.Filter((name, _) => uniforms.Contains(name)).Iter((name, color) => color.Iter(held => uniforms[name] = held));
            return Effect.ToShader(uniforms, children, SKMatrix.CreateTranslation(frame.Left, frame.Top));
        }
    }

    public sealed record Backdrop(bool Clear, ColorValue Fallback) : Material {
        public Option<ColorValue> Tint { get; init; }

        internal override Option<ColorValue> BaseColor => Some(Fallback);

        internal override Option<Backdrop> AsBackdrop => Some(this);
    }

    public sealed record Blurred(float Sigma, ColorValue Tint) : Material {
        internal override Option<ColorValue> BaseColor => Some(Tint);

        internal override Option<float> Blur => Some(Sigma);
    }
}
