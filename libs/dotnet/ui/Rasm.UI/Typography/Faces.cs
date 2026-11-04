using System.Globalization;
using HarfBuzzSharp;
using Rasm.UI.Controls.Primitives;
using Rasm.UI.Media;
using Rasm.UI.Styling;
using SkiaSharp;
using SuperLinq;
using Topten.RichTextKit;

namespace Rasm.UI.Typography;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Span(string Text) {
    public Option<Key> Role { get; init; }
    public HashSet<Condition> Conditions { get; init; }
    public Option<CultureInfo> Language { get; init; }
    public Option<Part> Part { get; init; }

    public static implicit operator Span(string text) => FromString(text);

    public static Span FromString(string text) => new(text);

    public static Seq<Span> Cut(Seq<Span> runs, Seq<int> offsets) =>
        runs.Zip(runs.Scan(0, static (start, run) => start + run.Text.Length)).Bind(held =>
            toSeq(held.Second.Cons(toSeq(offsets.Filter(at => at > held.Second && at < held.Second + held.First.Text.Length).Distinct().Order())).Add(held.Second + held.First.Text.Length)
                .AsIterable().Window(2, cut => held.First with { Text = held.First.Text[(cut[0] - held.Second)..(cut[1] - held.Second)] })))
            .Filter(static piece => piece.Text.Length > 0);
}

public sealed record Halo(ColorValue Color, float Width, float Blur);

public sealed record TextFace : Monoid<TextFace> {
    public static TextFace Empty { get; } = new();

    public static TextFace Tabular { get; } = new() { Features = HashMap((Tag.Parse("tnum"), 1u)) };

    public Option<string> Family { get; init; }
    public Option<SKFontStyleWeight> Weight { get; init; }
    public Option<SKFontStyleWidth> Width { get; init; }
    public Option<bool> Italic { get; init; }
    public HashMap<Tag, float> Variations { get; init; }
    public Option<float> LetterSpacing { get; init; }
    public Option<float> LineHeight { get; init; }
    public Option<UnderlineStyle> Underline { get; init; }
    public Option<StrikeThroughStyle> StrikeThrough { get; init; }
    public Option<FontVariant> Variant { get; init; }
    public HashMap<Tag, uint> Features { get; init; }
    public HashMap<ushort, ColorValue> Palette { get; init; }
    public Option<ColorValue> Background { get; init; }
    public Option<Halo> Halo { get; init; }
    public Option<float> Stroked { get; init; }
    public Option<char> Replacement { get; init; }
    public Option<TextDirection> Direction { get; init; }

    public TextFace Combine(TextFace rhs) =>
        new() {
            Family = rhs.Family | Family,
            Weight = rhs.Weight | Weight,
            Width = rhs.Width | Width,
            Italic = rhs.Italic | Italic,
            Variations = Variations.Union(rhs.Variations, static (_, _, stated) => stated),
            LetterSpacing = rhs.LetterSpacing | LetterSpacing,
            LineHeight = rhs.LineHeight | LineHeight,
            Underline = rhs.Underline | Underline,
            StrikeThrough = rhs.StrikeThrough | StrikeThrough,
            Variant = rhs.Variant | Variant,
            Features = Features.Union(rhs.Features, static (_, _, stated) => stated),
            Palette = Palette.Union(rhs.Palette, static (_, _, stated) => stated),
            Background = rhs.Background | Background,
            Halo = rhs.Halo | Halo,
            Stroked = rhs.Stroked | Stroked,
            Replacement = rhs.Replacement | Replacement,
            Direction = rhs.Direction | Direction,
        };
}

internal sealed record TextStyle : IStyle {
    public required string FontFamily { get; init; }
    public float FontSize { get; init; }
    public int FontWeight { get; init; }
    public SKFontStyleWidth FontWidth { get; init; }
    public bool FontItalic { get; init; }
    public UnderlineStyle Underline { get; init; }
    public StrikeThroughStyle StrikeThrough { get; init; }
    public float LineHeight { get; init; }
    public SKColorF TextColor { get; init; }
    public SKColorF BackgroundColor { get; init; }
    public SKColorF HaloColor { get; init; }
    SKColor IStyle.TextColor => (SKColor)TextColor;
    SKColor IStyle.BackgroundColor => (SKColor)BackgroundColor;
    SKColor IStyle.HaloColor => (SKColor)HaloColor;
    public float HaloWidth { get; init; }
    public float HaloBlur { get; init; }
    public float LetterSpacing { get; init; }
    public FontVariant FontVariant => FontVariant.Normal;
    public TextDirection TextDirection { get; init; }
    public char ReplacementCharacter { get; init; }
    public Seq<Feature> Features { get; init; }
    public Option<FontVariant> Variant { get; init; }
    public HashMap<Tag, float> Variations { get; init; }
    public Seq<SKFontPaletteOverride> Palette { get; init; }
    public bool Dark { get; init; }
    public Option<SKTypeface> Fallback { get; init; }
    public Option<CultureInfo> Language { get; init; }
    public Option<float> Cell { get; init; }
    public Option<float> Stroked { get; init; }
    public Option<int> Span { get; init; }
}
