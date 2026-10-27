using System.Numerics;
using SkiaSharp;

namespace Rasm.UI.Styling;

// --- [TYPES] ---------------------------------------------------------------------------
internal delegate SKRuntimeEffect? Compiler(string sksl, out string errors);

// --- [MODELS] --------------------------------------------------------------------------
internal sealed record RuntimeEffect {
    private const string Clock = "time";
    private const string Pointer = "pointer";
    private const string Size = "size";
    private const string Radii = "corners";

    private static readonly Seq<string> Implicit = [Size, Clock, Radii, Pointer];
    private static readonly SKPoint Away = new(float.PositiveInfinity, float.PositiveInfinity);

    private RuntimeEffect(SKRuntimeEffect program, HashMap<string, ColorValue> colors, HashMap<string, Metric> floats, HashMap<string, Material> children) =>
        (Program, Colors, Floats, Children) = (program, colors, floats, children);

    internal SKRuntimeEffect Program { get; }
    internal HashMap<string, ColorValue> Colors { get; }
    internal HashMap<string, Metric> Floats { get; }
    internal HashMap<string, Material> Children { get; }

    internal Seq<GlyphSource> Arts => Children.Values.ToSeq().Bind(static child => child.Arts);

    internal bool Timed => Program.Uniforms.Contains(Clock, StringComparer.Ordinal) || Children.Values.Exists(static child => child.Timed);

    internal bool Pointing => Program.Uniforms.Contains(Pointer, StringComparer.Ordinal) || Children.Values.Exists(static child => child.Pointing);

    // --- [VALIDATION]
    internal static Fin<RuntimeEffect> Validated(Compiler compile, string sksl, HashMap<string, ColorValue> colors, HashMap<string, Metric> floats, HashMap<string, Material> children) =>
        compile(sksl, out string errors) switch {
            null => new UIFault(errors, UIReason.Refused),
            var program => Declared(program, colors, floats, children) switch {
                Fin<Unit>.Fail refused => Released(program, refused.Error),
                _ => new RuntimeEffect(program, colors, floats, children),
            },
        };

    private static Fin<RuntimeEffect> Released(SKRuntimeEffect program, Error refused) {
        program.Dispose();
        return refused;
    }

    private static Fin<Unit> Declared(SKRuntimeEffect program, HashMap<string, ColorValue> colors, HashMap<string, Metric> floats, HashMap<string, Material> children) {
        Seq<string> stated = colors.Keys.ToSeq() + floats.Keys.ToSeq();
        HashSet<string> bindable = toHashSet(Implicit + stated);
        Seq<string> stray = stated.Filter(name => !program.Uniforms.Contains(name, StringComparer.Ordinal)) + children.Keys.ToSeq().Filter(name => !program.Children.Contains(name, StringComparer.Ordinal));
        Seq<string> unbound = toSeq(program.Uniforms).Filter(name => !bindable.Contains(name)) + toSeq(program.Children).Filter(name => !children.ContainsKey(name));
        return (UIFault.Unless(stray.IsEmpty, UIReason.Missing, stray), UIFault.Unless(unbound.IsEmpty, UIReason.Missing, unbound), Typed(program, colors, floats))
            .Apply(static (_, _, _) => unit).As();
    }

    private static Fin<Unit> Typed(SKRuntimeEffect program, HashMap<string, ColorValue> colors, HashMap<string, Metric> floats) =>
        Try.lift(() => {
            using SKRuntimeEffectUniforms probed = Uniforms(program, colors.Map(static _ => Some(new SKColorF())), floats.Map(static _ => 0f), SKSize.Empty, 0f, [0f, 0f, 0f, 0f], SKPoint.Empty);
            return unit;
        }).Run().MapFail(error => new UIFault(string.Join(", ", colors.Keys.Concat(floats.Keys)), UIReason.Refused, Inner: error));

    // --- [BINDING]
    internal Option<T> Bound<T>(Func<ColorValue, Option<SKColorF>> resolve, Func<Metric, float> metric, Func<Material, Option<SKShader>> shader, SKRect frame, Corners corners, float clock, Option<SKPoint> pointer,
                                SKMatrix local, Func<SKRuntimeEffect, SKRuntimeEffectUniforms, SKRuntimeEffectChildren, T> build) =>
        Children.AsIterable().ToSeq().Traverse(pair => shader(pair.Value).Map(made => (Name: pair.Key, Shader: made))).As().Map(made => {
            using SKRuntimeEffectUniforms uniforms = Uniforms(Program, Colors.Map(resolve), Floats.Map(metric), frame.Size, clock,
                [corners.TopLeft, corners.TopRight, corners.BottomRight, corners.BottomLeft], pointer.Map(at => at - frame.Location).IfNone(Away));
            using SKRuntimeEffectChildren bound = new(Program);
            Seq<SKShader> placed = made.Map(held => {
                using SKShader owned = held.Shader;
                SKShader moved = owned.WithLocalMatrix(local);
                bound[held.Name] = moved;
                return moved;
            }).Strict();
            T built = build(Program, uniforms, bound);
            _ = placed.Iter(static held => held.Dispose());
            return built;
        });

    private static SKRuntimeEffectUniforms Uniforms(SKRuntimeEffect program, HashMap<string, Option<SKColorF>> colors, HashMap<string, float> floats, SKSize size, float clock, float[] radii, SKPoint pointer) {
        SKRuntimeEffectUniforms uniforms = new(program);
        if (uniforms.Contains(Size)) uniforms[Size] = size;
        if (uniforms.Contains(Clock)) uniforms[Clock] = clock;
        if (uniforms.Contains(Radii)) uniforms[Radii] = radii;
        if (uniforms.Contains(Pointer)) uniforms[Pointer] = pointer;
        _ = colors.Iter((name, color) => color.Iter(held => uniforms[name] = held));
        _ = floats.Iter((name, value) => uniforms[name] = value);
        return uniforms;
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.DefaultWithPartialOverloads)]
public abstract partial record Material {
    // --- [FACTS]
    private static readonly SKSamplingOptions Sampling = new(SKFilterMode.Linear);

    internal virtual Option<ColorValue> BaseColor => None;

    internal virtual bool Timed => false;

    internal virtual bool Pointing => false;

    internal virtual Material Opaque => this;

    internal virtual Seq<GlyphSource> Arts => [];

    private static SKMatrix Placed(SKRect frame, SKSize natural, Option<Matrix3x2> transform) =>
        transform.Match(
            Some: static held => ((SKMatrix44)new Matrix4x4(held)).Matrix,
            None: () => SKMatrix.CreateScale(frame.Width / natural.Width, frame.Height / natural.Height)).PostConcat(SKMatrix.CreateTranslation(frame.Left, frame.Top));

    // --- [CASES]
    public sealed record Solid(ColorValue Color) : Material {
        internal override Option<ColorValue> BaseColor => Some(Color);
    }

    public sealed record Gradient(ColorRamp Ramp, GradientShape Shape) : Material {
        internal Option<SKShader> Shader(SKRect frame, Func<ColorValue, Option<SKColorF>> resolve) =>
            Ramp.Dense(resolve).Map(dense => Shape.Shader(frame, [.. dense.Map(static stop => stop.Color)], [.. dense.Map(static stop => (float)stop.At)]));
    }

    public sealed record Drawn : Material {
        private readonly RuntimeEffect effect;

        private Drawn(RuntimeEffect effect) => this.effect = effect;

        internal override bool Timed => effect.Timed;

        internal override bool Pointing => effect.Pointing;

        internal override Seq<GlyphSource> Arts => effect.Arts;

        public static Fin<Drawn> Validated(string sksl, HashMap<string, ColorValue> colors = default, HashMap<string, Metric> floats = default, HashMap<string, Material> children = default) =>
            RuntimeEffect.Validated(SKRuntimeEffect.CreateShader, sksl, colors, floats, children).Map(static effect => new Drawn(effect));

        internal Option<SKShader> Shader(SKRect frame, Func<ColorValue, Option<SKColorF>> resolve, Func<Metric, float> metric, Func<Material, Option<SKShader>> child, Corners corners, float clock, Option<SKPoint> pointer) =>
            effect.Bound(resolve, metric, child, frame, corners, clock, pointer, SKMatrix.CreateTranslation(-frame.Left, -frame.Top),
                (program, uniforms, children) => program.ToShader(uniforms, children, SKMatrix.CreateTranslation(frame.Left, frame.Top)));
    }

    public sealed record Glass(bool Clear, Seq<Filter> Backdrop) : Material {
        private static readonly Material Window = new Solid(new ColorValue.Host(Channel.Fill) { Role = Role.Window });

        public Option<float> Refraction { get; init; }

        internal override bool Timed => Backdrop.Exists(static filter => filter.Timed);

        internal override bool Pointing => Backdrop.Exists(static filter => filter.Pointing);

        internal override Material Opaque => Window;

        internal override Seq<GlyphSource> Arts => Backdrop.Bind(static filter => filter.Arts);
    }

    public sealed record Hatch(ColorValue Color, float Angle, float Spacing, float Width) : Material {
        internal SKPathEffect Effect() => SKPathEffect.Create2DLine(Width, SKMatrix.CreateScale(Spacing, Spacing).PostConcat(SKMatrix.CreateRotationDegrees(Angle)));
    }

    public sealed record Skin(SKImage Source, SKRectI Center) : Material;

    public sealed record Noise(float Frequency, int Octaves) : Material {
        public float Seed { get; init; }
        public bool Turbulent { get; init; }

        internal SKShader Shader(SKRect frame) {
            using SKShader noise = Turbulent
                ? SKShader.CreatePerlinNoiseTurbulence(Frequency, Frequency, Octaves, Seed)
                : SKShader.CreatePerlinNoiseFractalNoise(Frequency, Frequency, Octaves, Seed);
            return noise.WithLocalMatrix(SKMatrix.CreateTranslation(frame.Left, frame.Top));
        }
    }

    public sealed record Image(SKImage Source) : Material {
        public SKShaderTileMode Tile { get; init; } = SKShaderTileMode.Clamp;
        public Option<Matrix3x2> Transform { get; init; }

        internal SKShader Shader(SKRect frame) => Source.ToShader(Tile, Tile, Sampling, Placed(frame, new SKSize(Source.Width, Source.Height), Transform));
    }

    public sealed record Art(GlyphSource Glyph) : Material {
        public SKShaderTileMode Tile { get; init; } = SKShaderTileMode.Clamp;
        public Option<Matrix3x2> Transform { get; init; }

        internal override Seq<GlyphSource> Arts => [Glyph];

        internal Option<SKShader> Shader(SKRect frame, Func<GlyphSource, Option<SKPicture>> art) =>
            art(Glyph).Map(picture => picture.ToShader(Tile, Tile, SKFilterMode.Linear, Placed(frame, picture.CullRect.Size, Transform), picture.CullRect));
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GradientShape {
    // --- [SHADER]
    private static readonly SKColorSpace Srgb = SKColorSpace.CreateSrgb();

    internal abstract SKShader Shader(SKRect frame, SKColorF[] colors, float[] positions);

    private static SKPoint Point(SKRect frame, UnitPoint at) => new(frame.Left + (frame.Width * (float)at.X), frame.Top + (frame.Height * (float)at.Y));

    private static float Reach(SKRect frame, SKPoint from) =>
        Seq(new SKPoint(frame.Left, frame.Top), new SKPoint(frame.Right, frame.Top), new SKPoint(frame.Right, frame.Bottom), new SKPoint(frame.Left, frame.Bottom))
            .Fold(0f, (farthest, corner) => float.Max(farthest, SKPoint.Distance(from, corner)));

    // --- [CASES]
    public sealed record Linear(float Angle) : GradientShape {
        internal override SKShader Shader(SKRect frame, SKColorF[] colors, float[] positions) {
            (float sin, float cos) = MathF.SinCos(float.DegreesToRadians(Angle));
            float reach = (MathF.Abs(cos * frame.Width) + MathF.Abs(sin * frame.Height)) / 2f;
            SKPoint along = new(cos * reach, sin * reach);
            return SKShader.CreateLinearGradient(new SKPoint(frame.MidX, frame.MidY) - along, new SKPoint(frame.MidX, frame.MidY) + along, colors, Srgb, positions, SKShaderTileMode.Clamp);
        }
    }

    public sealed record Radial(UnitPoint Center) : GradientShape {
        internal override SKShader Shader(SKRect frame, SKColorF[] colors, float[] positions) =>
            Point(frame, Center) switch {
                var center => SKShader.CreateRadialGradient(center, Reach(frame, center), colors, Srgb, positions, SKShaderTileMode.Clamp),
            };
    }

    public sealed record Sweep(UnitPoint Center, float Start) : GradientShape {
        internal override SKShader Shader(SKRect frame, SKColorF[] colors, float[] positions) =>
            SKShader.CreateSweepGradient(Point(frame, Center), colors, Srgb, positions, SKShaderTileMode.Clamp, Start, Start + 360f);
    }

    public sealed record Conical(UnitPoint From, Fraction FromRadius, UnitPoint To, Fraction ToRadius) : GradientShape {
        internal override SKShader Shader(SKRect frame, SKColorF[] colors, float[] positions) {
            (SKPoint start, SKPoint end) = (Point(frame, From), Point(frame, To));
            return SKShader.CreateTwoPointConicalGradient(start, (float)FromRadius * Reach(frame, start), end, (float)ToRadius * Reach(frame, end), colors, Srgb, positions, SKShaderTileMode.Clamp);
        }
    }
}

