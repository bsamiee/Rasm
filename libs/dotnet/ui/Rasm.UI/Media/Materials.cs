using System.Numerics;
using Rasm.UI.Styling;
using SkiaSharp;
using Wacton.Unicolour;

namespace Rasm.UI.Media;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.DefaultWithPartialOverloads)]
public abstract partial record Material {
    // --- [FACTS]
    private static readonly SKSamplingOptions Sampling = new(SKFilterMode.Linear);

    internal virtual Option<ColorValue> BaseColor => None;

    internal virtual bool Timed => false;

    internal virtual bool Pointing => false;

    internal virtual Material Opaque => this;

    internal virtual Seq<GlyphSource> Arts => [];

    internal virtual Option<SKShader> Shader(PaintTarget target) => None;

    private static SKMatrix Placed(SKRect frame, SKSize natural, Option<Matrix3x2> transform) =>
        transform.Match(
            Some: static held => ((SKMatrix44)new Matrix4x4(held)).Matrix,
            None: () => SKMatrix.CreateScale(frame.Width / natural.Width, frame.Height / natural.Height)).PostConcat(SKMatrix.CreateTranslation(frame.Left, frame.Top));

    // --- [CASES]
    public sealed record Solid(ColorValue Color) : Material {
        internal override Option<ColorValue> BaseColor => Some(Color);

        internal override Option<SKShader> Shader(PaintTarget target) => target.Resolve(Color).Map(static color => {
            using SKColorSpace space = SKColorSpace.CreateSrgb();
            return SKShader.CreateColor(color, space);
        });
    }

    public sealed record Gradient(ColorRamp Ramp, GradientShape Shape) : Material {
        internal override Option<SKShader> Shader(PaintTarget target) =>
            Ramp.Dense(target.Resolve, target.Space.GammaIsLinear ? ColourSpace.RgbLinear : ColourSpace.Rgb).Map(dense => Shape.Shader(target.Frame, [.. dense.Map(static stop => stop.Color)], [.. dense.Map(static stop => (float)stop.At)]));
    }

    public sealed record Drawn : Material {
        private readonly RuntimeEffect effect;

        private Drawn(RuntimeEffect effect) => this.effect = effect;

        internal override bool Timed => effect.Timed;

        internal override bool Pointing => effect.Pointing;

        internal override Seq<GlyphSource> Arts => effect.Arts;

        public static Fin<Drawn> Validated(string sksl, HashMap<string, ColorValue> colors = default, HashMap<string, Metric> floats = default, HashMap<string, Material> children = default) =>
            RuntimeEffect.Validated(SKRuntimeEffect.CreateShader, sksl, colors, floats, children).Map(static effect => new Drawn(effect));

        internal override Option<SKShader> Shader(PaintTarget target) =>
            effect.Bound(target, SKMatrix.CreateTranslation(-target.Frame.Left, -target.Frame.Top),
                (program, uniforms, children) => program.ToShader(uniforms, children, SKMatrix.CreateTranslation(target.Frame.Left, target.Frame.Top)));
    }

    public sealed record Glass(bool Clear, Seq<Filter> Backdrop) : Material {
        private static readonly Material Window = new Solid(new ColorValue.Host(Token.Canvas));

        public Option<float> Refraction { get; init; }

        internal override bool Timed => Backdrop.Exists(static filter => filter.Timed);

        internal override bool Pointing => Backdrop.Exists(static filter => filter.Pointing);

        internal override Material Opaque => Window;

        internal override Seq<GlyphSource> Arts => Backdrop.Bind(static filter => filter.Arts);

        internal static float Band(float scale) => 1f / scale;

        internal (SKPoint Side, SKPoint End) Rim(bool dark) =>
            (Clear, dark) switch {
                (false, false) => (new SKPoint(0.8266f, 1.2402f), new SKPoint(0.8822f, 1.1543f)),
                (false, true) => (new SKPoint(0.3867f, 0.4890f), new SKPoint(0.5761f, 0.7615f)),
                (true, false) => (new SKPoint(0.7731f, 1.3750f), new SKPoint(1f, 1f)),
                (true, true) => (new SKPoint(0.6434f, 1.1787f), new SKPoint(1f, 1f)),
            };
    }

    public sealed record Hatch(ColorValue Color, float Angle, float Spacing, float Width) : Material {
        internal SKPathEffect Effect() => SKPathEffect.Create2DLine(Width, SKMatrix.CreateScale(Spacing, Spacing).PostConcat(SKMatrix.CreateRotationDegrees(Angle)));
    }

    public sealed record Skin(SKImage Source, SKRectI Center) : Material;

    public sealed record Noise(float Frequency, int Octaves) : Material {
        public float Seed { get; init; }
        public bool Turbulent { get; init; }

        internal override Option<SKShader> Shader(PaintTarget target) {
            using SKShader noise = Turbulent
                ? SKShader.CreatePerlinNoiseTurbulence(Frequency, Frequency, Octaves, Seed)
                : SKShader.CreatePerlinNoiseFractalNoise(Frequency, Frequency, Octaves, Seed);
            return noise.WithLocalMatrix(SKMatrix.CreateTranslation(target.Frame.Left, target.Frame.Top));
        }
    }

    public sealed record Image(SKImage Source) : Material {
        public SKShaderTileMode Tile { get; init; } = SKShaderTileMode.Clamp;
        public Option<Matrix3x2> Transform { get; init; }

        internal override Option<SKShader> Shader(PaintTarget target) => Source.ToShader(Tile, Tile, Sampling, Placed(target.Frame, new SKSize(Source.Width, Source.Height), Transform));
    }

    public sealed record Art(GlyphSource Glyph) : Material {
        public SKShaderTileMode Tile { get; init; } = SKShaderTileMode.Clamp;
        public Option<Matrix3x2> Transform { get; init; }

        internal override Seq<GlyphSource> Arts => [Glyph];

        internal override Option<SKShader> Shader(PaintTarget target) =>
            target.Art(Glyph).Map(picture => picture.ToShader(Tile, Tile, SKFilterMode.Linear, Placed(target.Frame, picture.CullRect.Size, Transform), picture.CullRect));
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GradientShape {
    // --- [SHADER]
    internal abstract SKShader Shader(SKRect frame, SKColorF[] colors, float[] positions);

    private static SKPoint Point(SKRect frame, UnitPoint at) => new(frame.Left + (frame.Width * (float)at.X), frame.Top + (frame.Height * (float)at.Y));

    private static float Reach(SKRect frame, SKPoint from) =>
        Seq(new SKPoint(frame.Left, frame.Top), new SKPoint(frame.Right, frame.Top), new SKPoint(frame.Right, frame.Bottom), new SKPoint(frame.Left, frame.Bottom))
            .Fold(0f, (farthest, corner) => float.Max(farthest, SKPoint.Distance(from, corner)));

    // --- [CASES]
    public sealed record Linear(float Angle) : GradientShape {
        internal override SKShader Shader(SKRect frame, SKColorF[] colors, float[] positions) {
            using SKColorSpace space = SKColorSpace.CreateSrgb();
            (float sin, float cos) = MathF.SinCos(float.DegreesToRadians(Angle));
            float reach = (MathF.Abs(cos * frame.Width) + MathF.Abs(sin * frame.Height)) / 2f;
            SKPoint along = new(cos * reach, sin * reach);
            return SKShader.CreateLinearGradient(new SKPoint(frame.MidX, frame.MidY) - along, new SKPoint(frame.MidX, frame.MidY) + along, colors, space, positions, SKShaderTileMode.Clamp);
        }
    }

    public sealed record Radial(UnitPoint Center) : GradientShape {
        internal override SKShader Shader(SKRect frame, SKColorF[] colors, float[] positions) {
            using SKColorSpace space = SKColorSpace.CreateSrgb();
            SKPoint center = Point(frame, Center);
            return SKShader.CreateRadialGradient(center, Reach(frame, center), colors, space, positions, SKShaderTileMode.Clamp);
        }
    }

    public sealed record Sweep(UnitPoint Center, float Start) : GradientShape {
        internal override SKShader Shader(SKRect frame, SKColorF[] colors, float[] positions) {
            using SKColorSpace space = SKColorSpace.CreateSrgb();
            return SKShader.CreateSweepGradient(Point(frame, Center), colors, space, positions, SKShaderTileMode.Clamp, Start, Start + 360f);
        }
    }

    public sealed record Conical(UnitPoint From, Fraction FromRadius, UnitPoint To, Fraction ToRadius) : GradientShape {
        internal override SKShader Shader(SKRect frame, SKColorF[] colors, float[] positions) {
            using SKColorSpace space = SKColorSpace.CreateSrgb();
            (SKPoint start, SKPoint end) = (Point(frame, From), Point(frame, To));
            return SKShader.CreateTwoPointConicalGradient(start, (float)FromRadius * Reach(frame, start), end, (float)ToRadius * Reach(frame, end), colors, space, positions, SKShaderTileMode.Clamp);
        }
    }
}
