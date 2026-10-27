using System.Numerics;
using SkiaSharp;
using Wacton.Unicolour;

namespace Rasm.UI.Styling;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.DefaultWithPartialOverloads)]
public abstract partial record Material {
    // --- [FACTS]
    internal virtual Option<ColorValue> BaseColor => None;

    internal virtual bool Timed => false;

    internal virtual bool Pointing => false;

    internal virtual Material Opaque => this;

    internal virtual Seq<GlyphSource> Arts => [];

    // --- [CASES]
    public sealed record Solid(ColorValue Color) : Material {
        internal override Option<ColorValue> BaseColor => Some(Color);
    }

    public sealed record Gradient(Seq<Stop<ColorValue>> Stops, GradientShape Shape) : Material {
        public ColourSpace Space { get; init; } = ColourSpace.Oklab;
        public HueSpan Hue { get; init; } = HueSpan.Shorter;

        internal Option<SKShader> Shader(SKRect frame, Func<ColorValue, Option<SKColorF>> resolve) =>
            Stops.Traverse(stop => resolve(stop.Color).Map(color => new Stop<SKColorF>(color, stop.At))).As()
                .Map(resolved => ColorRamp.Dense(toSeq(resolved.OrderBy(static stop => stop.At)), Mixed))
                .Map(dense => Shape.Shader(frame, [.. dense.Map(static stop => stop.Color)], [.. dense.Map(static stop => (float)stop.At)]));

        private SKColorF Mixed(RampSegment segment, Fraction at) => segment.Low.Color.ToUnicolour().Mix(segment.High.Color.ToUnicolour(), Space, segment.Progress(at), Hue).ToSKColorF();
    }

    public sealed record Drawn : Material {
        internal const string Size = "size";
        internal const string Clock = "time";
        internal const string Radii = "corners";
        internal const string Pointer = "pointer";

        private static readonly Seq<string> Implicit = [Size, Clock, Radii, Pointer];
        private static readonly SKPoint Away = new(float.PositiveInfinity, float.PositiveInfinity);

        private readonly SKRuntimeEffect effect;
        private readonly HashMap<string, ColorValue> colors;
        private readonly HashMap<string, Metric> floats;
        private readonly HashMap<string, Child> children;

        private Drawn(SKRuntimeEffect effect, HashMap<string, ColorValue> colors, HashMap<string, Metric> floats, HashMap<string, Child> children) {
            this.effect = effect;
            this.colors = colors;
            this.floats = floats;
            this.children = children;
        }

        internal override bool Timed => effect.Uniforms.Contains(Clock, StringComparer.Ordinal);

        internal override bool Pointing => effect.Uniforms.Contains(Pointer, StringComparer.Ordinal);

        public static Fin<Drawn> Validated(string sksl, HashMap<string, ColorValue> colors = default, HashMap<string, Metric> floats = default, HashMap<string, Child> children = default) =>
            SKRuntimeEffect.CreateShader(sksl, out string errors) switch {
                null => new UIFault(errors, UIReason.Refused),
                var compiled => Declared(compiled, colors, floats, children) switch {
                    Fin<Unit>.Fail refused => Released(compiled, refused.Error),
                    _ => new Drawn(compiled, colors, floats, children),
                },
            };

        internal override Seq<GlyphSource> Arts => children.Values.ToSeq().Bind(static child => child.Arts);

        internal Option<SKShader> Shader(SKRect frame, Func<ColorValue, Option<SKColorF>> resolve, Func<Metric, float> metric, Func<Child, Option<SKShader>> child, Corners corners, float clock, Option<SKPoint> pointer) =>
            children.AsIterable().ToSeq().Traverse(pair => child(pair.Value).Map(shader => (Name: pair.Key, Shader: shader))).As().Map(made => {
                using SKRuntimeEffectUniforms uniforms = Uniforms(effect, colors.Map(resolve), floats.Map(metric), frame.Size, clock,
                    [corners.TopLeft, corners.TopRight, corners.BottomRight, corners.BottomLeft], pointer.Map(at => at - frame.Location).IfNone(Away));
                using SKRuntimeEffectChildren bound = new(effect);
                _ = made.Iter(held => bound[held.Name] = held.Shader);
                SKShader shader = effect.ToShader(uniforms, bound, SKMatrix.CreateTranslation(frame.Left, frame.Top));
                _ = made.Iter(static held => held.Shader.Dispose());
                return shader;
            });

        private static Fin<Drawn> Released(SKRuntimeEffect compiled, Error refused) {
            compiled.Dispose();
            return refused;
        }

        private static Fin<Unit> Declared(SKRuntimeEffect compiled, HashMap<string, ColorValue> colors, HashMap<string, Metric> floats, HashMap<string, Child> children) {
            Seq<string> stated = colors.Keys.ToSeq() + floats.Keys.ToSeq();
            HashSet<string> bindable = toHashSet(Implicit + stated);
            Seq<string> stray = stated.Filter(name => !compiled.Uniforms.Contains(name, StringComparer.Ordinal)) + children.Keys.ToSeq().Filter(name => !compiled.Children.Contains(name, StringComparer.Ordinal));
            Seq<string> unbound = toSeq(compiled.Uniforms).Filter(name => !bindable.Contains(name)) + toSeq(compiled.Children).Filter(name => !children.ContainsKey(name));
            return (UIFault.Unless(stray.IsEmpty, UIReason.Missing, stray), UIFault.Unless(unbound.IsEmpty, UIReason.Missing, unbound), Typed(compiled, colors, floats))
                .Apply(static (_, _, _) => unit).As();
        }

        private static Fin<Unit> Typed(SKRuntimeEffect compiled, HashMap<string, ColorValue> colors, HashMap<string, Metric> floats) =>
            Try.lift(() => {
                using SKRuntimeEffectUniforms probed = Uniforms(compiled, colors.Map(static _ => Some(new SKColorF())), floats.Map(static _ => 0f), SKSize.Empty, 0f, [0f, 0f, 0f, 0f], SKPoint.Empty);
                return unit;
            }).Run().MapFail(error => new UIFault(string.Join(", ", colors.Keys.Concat(floats.Keys)), UIReason.Refused, Inner: error));

        private static SKRuntimeEffectUniforms Uniforms(SKRuntimeEffect compiled, HashMap<string, Option<SKColorF>> colors, HashMap<string, float> floats, SKSize size, float clock, float[] radii, SKPoint pointer) {
            SKRuntimeEffectUniforms uniforms = new(compiled);
            if (uniforms.Contains(Size)) uniforms[Size] = size;
            if (uniforms.Contains(Clock)) uniforms[Clock] = clock;
            if (uniforms.Contains(Radii)) uniforms[Radii] = radii;
            if (uniforms.Contains(Pointer)) uniforms[Pointer] = pointer;
            _ = colors.Iter((name, color) => color.Iter(held => uniforms[name] = held));
            _ = floats.Iter((name, value) => uniforms[name] = value);
            return uniforms;
        }
    }

    public sealed record Glass(bool Clear, Seq<Filter> Backdrop) : Material {
        private static readonly Material Window = new Solid(new ColorValue.Host(Channel.Fill) { Role = Role.Window });

        public Option<float> Refraction { get; init; }

        internal override Material Opaque => Window;

        internal override Seq<GlyphSource> Arts => Backdrop.Bind(static filter => filter.Arts);
    }

    public sealed record Pattern(Child Source) : Material {
        internal override Seq<GlyphSource> Arts => Source.Arts;

        internal Option<SKShader> Shader(SKRect frame, Func<Material, Option<SKShader>> fill, Func<GlyphSource, Option<SKPicture>> art) =>
            Source.Shader(frame, fill, art).Map(shader => {
                using SKShader owned = shader;
                return owned.WithLocalMatrix(SKMatrix.CreateTranslation(frame.Left, frame.Top));
            });
    }

    public sealed record Hatch(ColorValue Color, float Angle, float Spacing, float Width) : Material {
        internal SKPathEffect Effect() => SKPathEffect.Create2DLine(Width, SKMatrix.CreateScale(Spacing, Spacing).PostConcat(SKMatrix.CreateRotationDegrees(Angle)));
    }

    public sealed record Skin(SKImage Art, SKRectI Center) : Material;
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

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Child {
    // --- [SHADER]
    internal virtual Seq<GlyphSource> Arts => [];

    internal abstract Option<SKShader> Shader(SKRect frame, Func<Material, Option<SKShader>> fill, Func<GlyphSource, Option<SKPicture>> art);

    private static SKMatrix Placed(SKRect frame, SKSize natural, Option<Matrix3x2> transform) =>
        transform.Match(
            Some: static held => ((SKMatrix44)new Matrix4x4(held)).Matrix,
            None: () => SKMatrix.CreateScale(frame.Width / natural.Width, frame.Height / natural.Height));

    // --- [CASES]
    public sealed record Noise(float Frequency, int Octaves) : Child {
        internal override Option<SKShader> Shader(SKRect frame, Func<Material, Option<SKShader>> fill, Func<GlyphSource, Option<SKPicture>> art) =>
            SKShader.CreatePerlinNoiseFractalNoise(Frequency, Frequency, Octaves, 0f);
    }

    public sealed record Image(SKImage Source) : Child {
        public SKShaderTileMode Tile { get; init; } = SKShaderTileMode.Clamp;
        public Option<Matrix3x2> Transform { get; init; }

        internal override Option<SKShader> Shader(SKRect frame, Func<Material, Option<SKShader>> fill, Func<GlyphSource, Option<SKPicture>> art) =>
            Source.ToShader(Tile, Tile, new SKSamplingOptions(SKFilterMode.Linear), Placed(frame, new SKSize(Source.Width, Source.Height), Transform));
    }

    public sealed record Art(GlyphSource Glyph) : Child {
        public SKShaderTileMode Tile { get; init; } = SKShaderTileMode.Clamp;
        public Option<Matrix3x2> Transform { get; init; }

        internal override Seq<GlyphSource> Arts => [Glyph];

        internal override Option<SKShader> Shader(SKRect frame, Func<Material, Option<SKShader>> fill, Func<GlyphSource, Option<SKPicture>> art) =>
            art(Glyph).Map(picture => picture.ToShader(Tile, Tile, SKFilterMode.Linear, Placed(frame, picture.CullRect.Size, Transform), picture.CullRect));
    }

    public sealed record Fill(Material Material) : Child {
        internal override Seq<GlyphSource> Arts => Material.Arts;

        internal override Option<SKShader> Shader(SKRect frame, Func<Material, Option<SKShader>> fill, Func<GlyphSource, Option<SKPicture>> art) =>
            fill(Material).Map(filled => {
                using SKShader owned = filled;
                return owned.WithLocalMatrix(SKMatrix.CreateTranslation(-frame.Left, -frame.Top));
            });
    }
}
