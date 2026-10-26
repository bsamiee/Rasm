using System.Globalization;
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

[SmartEnum<string>]
public sealed partial class FilterFunction {
    private static readonly Vector3 Luminance = new(0.2126f, 0.7152f, 0.0722f);
    private static readonly Vector3 Saturation = new(0.213f, 0.715f, 0.072f);
    private static readonly (Vector3 Red, Vector3 Green, Vector3 Blue) Aged = (new(0.393f, 0.769f, 0.189f), new(0.349f, 0.686f, 0.168f), new(0.272f, 0.534f, 0.131f));
    private static readonly (Vector3 Red, Vector3 Green, Vector3 Blue) Quadrature = (new(-0.213f, -0.715f, 0.928f), new(0.143f, 0.140f, -0.283f), new(-0.787f, 0.715f, 0.072f));

    public static readonly FilterFunction Blur = new("blur", NonNegative, static (amount, input) => SKImageFilter.CreateBlur(amount, amount, input));
    public static readonly FilterFunction Grayscale = new("grayscale", NonNegative,
        static (amount, input) => Matrixed(Toward((Luminance, Luminance, Luminance), 1f - float.Min(amount, 1f)), Vector3.Zero, input));
    public static readonly FilterFunction Saturate = new("saturate", NonNegative, static (amount, input) => Matrixed(Toward((Saturation, Saturation, Saturation), amount), Vector3.Zero, input));
    public static readonly FilterFunction Brightness = new("brightness", NonNegative, static (amount, input) => Matrixed(Scaled(amount), Vector3.Zero, input));
    public static readonly FilterFunction Contrast = new("contrast", NonNegative, static (amount, input) => Matrixed(Scaled(amount), new Vector3(0.5f - (0.5f * amount)), input));
    public static readonly FilterFunction HueRotate = new("hue-rotate", float.IsFinite, static (amount, input) => Matrixed(Turned(float.DegreesToRadians(amount)), Vector3.Zero, input));
    public static readonly FilterFunction Invert = new("invert", NonNegative,
        static (amount, input) => float.Min(amount, 1f) switch { var held => Matrixed(Scaled(1f - (2f * held)), new Vector3(held), input) });
    public static readonly FilterFunction Sepia = new("sepia", NonNegative, static (amount, input) => Matrixed(Toward(Aged, 1f - float.Min(amount, 1f)), Vector3.Zero, input));

    [UseDelegateFromConstructor]
    public partial bool Accepts(float amount);

    [UseDelegateFromConstructor]
    internal partial SKImageFilter Built(float amount, SKImageFilter? input);

    private static bool NonNegative(float amount) => amount >= 0f && float.IsFinite(amount);

    private static (Vector3 Red, Vector3 Green, Vector3 Blue) Toward((Vector3 Red, Vector3 Green, Vector3 Blue) target, float kept) =>
        (Vector3.Lerp(target.Red, Vector3.UnitX, kept), Vector3.Lerp(target.Green, Vector3.UnitY, kept), Vector3.Lerp(target.Blue, Vector3.UnitZ, kept));

    private static (Vector3 Red, Vector3 Green, Vector3 Blue) Scaled(float slope) => (Vector3.UnitX * slope, Vector3.UnitY * slope, Vector3.UnitZ * slope);

    private static (Vector3 Red, Vector3 Green, Vector3 Blue) Turned(float radians) =>
        MathF.SinCos(radians) switch {
            var (sin, cos) => (
                Vector3.Lerp(Saturation, Vector3.UnitX, cos) + (Quadrature.Red * sin),
                Vector3.Lerp(Saturation, Vector3.UnitY, cos) + (Quadrature.Green * sin),
                Vector3.Lerp(Saturation, Vector3.UnitZ, cos) + (Quadrature.Blue * sin)),
        };

    private static SKImageFilter Matrixed((Vector3 Red, Vector3 Green, Vector3 Blue) rows, Vector3 offset, SKImageFilter? input) {
        using SKColorFilter color = SKColorFilter.CreateColorMatrix([
            rows.Red.X, rows.Red.Y, rows.Red.Z, 0f, offset.X,
            rows.Green.X, rows.Green.Y, rows.Green.Z, 0f, offset.Y,
            rows.Blue.X, rows.Blue.Y, rows.Blue.Z, 0f, offset.Z,
            0f, 0f, 0f, 1f, 0f,
        ]);
        return SKImageFilter.CreateColorFilter(color, input);
    }
}

[ComplexValueObject]
[ValidationError<Invalid>]
public sealed partial class Filter {
    public FilterFunction Function { get; }
    public float Amount { get; }

    static partial void ValidateFactoryArguments(ref Invalid? validationError, ref FilterFunction function, ref float amount) =>
        validationError = function.Accepts(amount) ? null : new Invalid(string.Create(CultureInfo.InvariantCulture, $"{function.Key}() takes no amount {amount}"));

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
    internal Material Opaqued =>
        SwitchPartially(
            @default: static held => held,
            backdrop: static backdrop => new Solid(backdrop.Fallback),
            blurred: static blurred => new Solid(blurred.Tint));

    public sealed record Solid(ColorValue Color) : Material;

    public sealed record Gradient(Seq<Stop<ColorValue>> Stops, float Angle) : Material;

    public sealed record Drawn(SKRuntimeEffect Effect, HashMap<string, ColorValue> Uniforms) : Material {
        internal const string Size = "size";
        internal const string Clock = "time";

        public bool Timed => Effect.Uniforms.Contains(Clock, StringComparer.Ordinal);
    }

    public sealed record Backdrop(bool Clear, ColorValue Fallback) : Material {
        public Option<ColorValue> Tint { get; init; }
    }

    public sealed record Blurred(float Sigma, ColorValue Tint) : Material;
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
