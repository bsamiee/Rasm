using System.Globalization;
using HarfBuzzSharp;
using Rasm.UI.Styling;
using SkiaSharp;
using Topten.RichTextKit;

namespace Rasm.UI.Typography;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Halo(ColorValue Color, float Width, float Blur);

public sealed record TextFace : Monoid<TextFace> {
    public static TextFace Empty { get; } = new();

    public static TextFace Tabular { get; } = new() { Features = Seq(new Feature(Tag.Parse("tnum"))) };

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
    public Option<Seq<Feature>> Features { get; init; }
    public Option<Seq<ColorValue>> Palette { get; init; }
    public Option<ColorValue> Background { get; init; }
    public Option<Halo> Halo { get; init; }
    public Option<float> Stroked { get; init; }
    public Option<char> Replacement { get; init; }
    public Option<TextDirection> Direction { get; init; }
    public bool Mono { get; init; }

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
            Features = rhs.Features | Features,
            Palette = rhs.Palette | Palette,
            Background = rhs.Background | Background,
            Halo = rhs.Halo | Halo,
            Stroked = rhs.Stroked | Stroked,
            Replacement = rhs.Replacement | Replacement,
            Direction = rhs.Direction | Direction,
            Mono = Mono || rhs.Mono,
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
    public SKColor TextColor { get; init; }
    public SKColor BackgroundColor { get; init; }
    public SKColor HaloColor { get; init; }
    public float HaloWidth { get; init; }
    public float HaloBlur { get; init; }
    public float LetterSpacing { get; init; }
    public FontVariant FontVariant => FontVariant.Normal;
    public TextDirection TextDirection { get; init; }
    public char ReplacementCharacter { get; init; }
    public Seq<Feature> Features { get; init; }
    public HashMap<Tag, float> Variations { get; init; }
    public Seq<SKFontPaletteOverride> Palette { get; init; }
    public Option<SKTypeface> Matched { get; init; }
    public Option<CultureInfo> Language { get; init; }
    public Option<float> Cell { get; init; }
    public Option<float> Stroked { get; init; }
}

internal sealed class Typeset(TextBlock lines, bool clipped, Func<SKColor, TextBlock> tinted) {
    public TextBlock Lines { get; } = lines;
    public SKSize Size { get; } = new(Widest(lines), lines.MeasuredHeight);
    public bool Truncated { get; } = lines.Truncated || clipped;
    public Option<float> Baseline { get; } = toSeq(lines.Lines).Head.Map(static line => line.YCoord + line.BaseLine);
    public Option<float> LastBaseline { get; } = toSeq(lines.Lines).Last.Map(static line => line.YCoord + line.BaseLine);
    public Option<float> CapHeight => Metrics.Map(static metrics => metrics.CapHeight);
    public Option<float> XHeight => Metrics.Map(static metrics => metrics.XHeight);

    private Option<SKFontMetrics> Metrics { get; } = toSeq(lines.FontRuns).Head.Map(static run => {
        using SKFont font = new(run.Typeface, run.Style.FontSize);
        return font.Metrics;
    });

    public Func<SKColor, TextBlock> Tinted { get; } = tinted;

    internal static float Widest(TextBlock lines) => toSeq(lines.Lines).Fold(0f, static (widest, line) => float.Max(widest, Advance(line)));

    internal static float Advance(TextLine line) =>
        toSeq(line.Runs).Filter(static run => run.RunKind is not FontRunKind.TrailingWhitespace).Fold(0f, static (sum, run) => sum + run.Width);
}
