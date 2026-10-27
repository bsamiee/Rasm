using System.Numerics;
using HarfBuzzSharp;
using SkiaSharp;
using Topten.RichTextKit;

namespace Rasm.UI.Styling;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Halo(ColorValue Color, float Width, float Blur);

public sealed record TextFace : Monoid<TextFace> {
    public static TextFace Empty { get; } = new();

    public static TextFace Tabular { get; } = new() { Features = Seq(new Feature(Tag.Parse("tnum"))) };

    public Option<string> Family { get; init; }
    public Option<SKFontStyleWeight> Weight { get; init; }
    public Option<SKFontStyleWidth> Width { get; init; }
    public Option<bool> Italic { get; init; }
    public Option<float> LetterSpacing { get; init; }
    public Option<float> LineHeight { get; init; }
    public Option<UnderlineStyle> Underline { get; init; }
    public Option<StrikeThroughStyle> StrikeThrough { get; init; }
    public Option<FontVariant> Variant { get; init; }
    public Option<Seq<Feature>> Features { get; init; }
    public Option<Halo> Halo { get; init; }

    public TextFace Combine(TextFace rhs) =>
        new() {
            Family = rhs.Family | Family,
            Weight = rhs.Weight | Weight,
            Width = rhs.Width | Width,
            Italic = rhs.Italic | Italic,
            LetterSpacing = rhs.LetterSpacing | LetterSpacing,
            LineHeight = rhs.LineHeight | LineHeight,
            Underline = rhs.Underline | Underline,
            StrikeThrough = rhs.StrikeThrough | StrikeThrough,
            Variant = rhs.Variant | Variant,
            Features = rhs.Features | Features,
            Halo = rhs.Halo | Halo,
        };
}

[SmartEnum]
public sealed partial class FilterFunction {
    private static readonly Matrix4x4 Luminance = Rows(new(0.2126f, 0.7152f, 0.0722f), new(0.2126f, 0.7152f, 0.0722f), new(0.2126f, 0.7152f, 0.0722f));
    private static readonly Matrix4x4 Saturation = Rows(new(0.213f, 0.715f, 0.072f), new(0.213f, 0.715f, 0.072f), new(0.213f, 0.715f, 0.072f));
    private static readonly Matrix4x4 Aged = Rows(new(0.393f, 0.769f, 0.189f), new(0.349f, 0.686f, 0.168f), new(0.272f, 0.534f, 0.131f));
    private static readonly Matrix4x4 Quadrature = Rows(new(-0.213f, -0.715f, 0.928f), new(0.143f, 0.140f, -0.283f), new(-0.787f, 0.715f, 0.072f));

    public static readonly FilterFunction Blur = new(NonNegative, static (amount, input) => SKImageFilter.CreateBlur(amount, amount, input));
    public static readonly FilterFunction Grayscale = new(NonNegative, static (amount, input) => Matrixed(Matrix4x4.Lerp(Luminance, Matrix4x4.Identity, 1f - float.Min(amount, 1f)), Vector3.Zero, input));
    public static readonly FilterFunction Saturate = new(NonNegative, static (amount, input) => Matrixed(Matrix4x4.Lerp(Saturation, Matrix4x4.Identity, amount), Vector3.Zero, input));
    public static readonly FilterFunction Brightness = new(NonNegative, static (amount, input) => Matrixed(Matrix4x4.CreateScale(amount), Vector3.Zero, input));
    public static readonly FilterFunction Contrast = new(NonNegative, static (amount, input) => Matrixed(Matrix4x4.CreateScale(amount), new Vector3(0.5f - (0.5f * amount)), input));
    public static readonly FilterFunction HueRotate = new(float.IsFinite, static (amount, input) => Matrixed(Turned(float.DegreesToRadians(amount)), Vector3.Zero, input));
    public static readonly FilterFunction Invert = new(NonNegative,
        static (amount, input) => float.Min(amount, 1f) switch { var held => Matrixed(Matrix4x4.CreateScale(1f - (2f * held)), new Vector3(held), input) });
    public static readonly FilterFunction Sepia = new(NonNegative, static (amount, input) => Matrixed(Matrix4x4.Lerp(Aged, Matrix4x4.Identity, 1f - float.Min(amount, 1f)), Vector3.Zero, input));

    [UseDelegateFromConstructor]
    public partial bool Accepts(float amount);

    [UseDelegateFromConstructor]
    internal partial SKImageFilter Built(float amount, SKImageFilter? input);

    private static bool NonNegative(float amount) => amount >= 0f && float.IsFinite(amount);

    private static Matrix4x4 Rows(Vector3 red, Vector3 green, Vector3 blue) => new(red.X, red.Y, red.Z, 0f, green.X, green.Y, green.Z, 0f, blue.X, blue.Y, blue.Z, 0f, 0f, 0f, 0f, 1f);

    private static Matrix4x4 Turned(float radians) {
        (float sin, float cos) = MathF.SinCos(radians);
        return Matrix4x4.Lerp(Saturation, Matrix4x4.Identity, cos) + (Quadrature * sin);
    }

    private static SKImageFilter Matrixed(Matrix4x4 rows, Vector3 offset, SKImageFilter? input) {
        using SKColorFilter color = SKColorFilter.CreateColorMatrix([
            rows.M11, rows.M12, rows.M13, 0f, offset.X,
            rows.M21, rows.M22, rows.M23, 0f, offset.Y,
            rows.M31, rows.M32, rows.M33, 0f, offset.Z,
            0f, 0f, 0f, 1f, 0f,
        ]);
        return SKImageFilter.CreateColorFilter(color, input);
    }
}

[ComplexValueObject(SkipFactoryMethods = true, SkipToString = true)]
public sealed partial class Filter {
    public FilterFunction Function { get; }
    public float Amount { get; }

    public static Fin<Filter> Validated(FilterFunction function, float amount) => UIFault.Unless(function.Accepts(amount), UIReason.Range).Map(_ => new Filter(function, amount));

    internal static Option<SKImageFilter> Chained(Seq<Filter> filters) =>
        filters.Fold(Option<SKImageFilter>.None, static (input, filter) => input.Match(
            Some: inner => {
                using SKImageFilter held = inner;
                return Some(filter.Function.Built(filter.Amount, held));
            },
            None: () => Some(filter.Function.Built(filter.Amount, input: null))));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.DefaultWithPartialOverloads)]
public abstract partial record Material {
    internal abstract Option<ColorValue> Under { get; }

    internal Material Opaqued => Under.Map(static color => (Material)new Solid(color)).IfNone(this);

    public sealed record Solid(ColorValue Color) : Material {
        internal override Option<ColorValue> Under => Some(Color);
    }

    public sealed record Gradient(Seq<Stop<ColorValue>> Stops, float Angle) : Material {
        private static readonly SKColorSpace Srgb = SKColorSpace.CreateSrgb();

        internal override Option<ColorValue> Under => None;

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

        public bool Timed => Effect.Uniforms.Contains(Clock, StringComparer.Ordinal);

        internal override Option<ColorValue> Under => None;

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

        internal override Option<ColorValue> Under => Some(Fallback);
    }

    public sealed record Blurred(float Sigma, ColorValue Tint) : Material {
        internal override Option<ColorValue> Under => Some(Tint);
    }
}

public sealed record Stroke(ColorValue Color, Seq<Seq<float>> Dash = default) {
    public Seq<float> Rhythm(uint ordinal) => Dash.IsEmpty ? [] : Dash[(int)(ordinal % (uint)Dash.Count)];
}

public sealed record Shadow(ColorValue Color, float Blur) {
    public float X { get; init; }
    public float Y { get; init; }
    public float Spread { get; init; }
    public bool Inside { get; init; }

    internal (float Outset, Shadow Graph) Outlined => Inside ? (0f, this) : (Spread, this with { Spread = 0f });

    internal SKImageFilter Filtered(SKColorF color) {
        float grown = Inside ? -Spread : Spread;
        using SKImageFilter? morphed = grown switch { > 0f => SKImageFilter.CreateDilate(grown, grown), < 0f => SKImageFilter.CreateErode(-grown, -grown), _ => null };
        using SKImageFilter moved = SKImageFilter.CreateOffset(X, Y, morphed);
        using SKImageFilter blurred = SKImageFilter.CreateBlur(Blur, Blur, moved);
        using SKColorFilter inked = SKColorFilter.CreateColorMatrix([
            0f, 0f, 0f, 0f, color.Red,
            0f, 0f, 0f, 0f, color.Green,
            0f, 0f, 0f, 0f, color.Blue,
            0f, 0f, 0f, Inside ? -color.Alpha : color.Alpha, Inside ? color.Alpha : 0f,
        ]);
        if (!Inside) return SKImageFilter.CreateColorFilter(inked, blurred);
        using SKImageFilter hollow = SKImageFilter.CreateColorFilter(inked, blurred);
        return SKImageFilter.CreateBlendMode(SKBlendMode.SrcIn, background: null, foreground: hollow);
    }
}

public sealed record Look : Monoid<Look> {
    public static Look Empty { get; } = new();

    public static Look Bare { get; } = new() { Fills = Seq<Material>(), Strokes = Seq<Stroke>() };

    public Option<Seq<Material>> Fills { get; init; }
    public Option<Seq<Stroke>> Strokes { get; init; }
    public Option<ColorValue> Text { get; init; }
    public TextFace Face { get; init; } = TextFace.Empty;
    public Option<Seq<Shadow>> Shadows { get; init; }
    public Option<Seq<Filter>> Filters { get; init; }
    public Option<Seq<ColorValue>> Palette { get; init; }
    public HashMap<Interaction, ColorValue> Washes { get; init; }
    public Option<SKBlendMode> Blend { get; init; }
    public Option<TimingCurve> Motion { get; init; }
    public HashMap<Condition, Look> When { get; init; }
    public HashMap<Role, Look> Within { get; init; }

    public Look Combine(Look rhs) =>
        new() {
            Fills = rhs.Fills | Fills,
            Strokes = rhs.Strokes | Strokes,
            Text = rhs.Text | Text,
            Face = Face.Combine(rhs.Face),
            Shadows = rhs.Shadows | Shadows,
            Filters = rhs.Filters | Filters,
            Palette = rhs.Palette | Palette,
            Washes = Washes.Union(rhs.Washes, Merge: static (_, _, over) => over),
            Blend = rhs.Blend | Blend,
            Motion = rhs.Motion | Motion,
            When = When.Union(rhs.When, Merge: static (_, under, over) => under.Combine(over)),
            Within = Within.Union(rhs.Within, Merge: static (_, under, over) => under.Combine(over)),
        };
}
