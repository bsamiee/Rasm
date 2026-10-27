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
    public Option<(Feature Feature, OpenTypeMetricsTag Size, OpenTypeMetricsTag Offset, float Direction)> Variant { get; init; }
    public HashMap<Tag, float> Variations { get; init; }
    public Seq<SKFontPaletteOverride> Palette { get; init; }
    public bool Dark { get; init; }
    public Option<SKTypeface> Fallback { get; init; }
    public Option<CultureInfo> Language { get; init; }
    public Option<float> Cell { get; init; }
    public Option<float> Stroked { get; init; }
    public Option<(int Index, Part Part)> Span { get; init; }
}

internal sealed class Typeset(Shaping shaping, Node.Text text, TextStyle style, float width, Option<(TextRange Range, SKColor Color)> selection, TextBlock lines, bool clipped) {
    private readonly Lazy<Option<float>> capHeight = new(() => toSeq(lines.FontRuns).Head.Bind(run =>
        shaping.HarfBuzzFont(run.Typeface).OpenTypeMetrics.TryGetPosition(OpenTypeMetricsTag.CapHeight, out int units) ? Some(units * run.Style.FontSize / run.Typeface.UnitsPerEm) : None));

    internal Node.Text Text { get; } = text;
    internal TextStyle Style { get; } = style;
    internal float Width { get; } = width;
    internal Option<(TextRange Range, SKColor Color)> Selection { get; } = selection;
    internal TextBlock Lines { get; } = lines;
    internal SKSize Size { get; } = new(Typesetting.Widest(lines), lines.MeasuredHeight);
    internal bool Truncated { get; } = lines.Truncated || clipped;

    internal SKRect Overhang => Lines.MeasuredOverhang switch {
        var over => over with { Right = float.Max(0f, over.Right + (Lines.MaxWidth ?? Lines.MeasuredWidth) - Size.Width) },
    };

    internal Option<float> At(Baseline baseline) =>
        from line in toSeq(Lines.Lines).At(baseline.Line.GetOffset(Lines.Lines.Count))
        from rise in (float)baseline.Rise switch {
            0f => Some(0f),
            var share => capHeight.Value.Map(cap => cap * share),
        }
        select line.YCoord + line.BaseLine - rise;

    internal Option<(int Index, Part Part)> PartAt(SKPoint local) =>
        Lines.HitTest(local.X, local.Y).OverCodePointIndex switch {
            < 0 => None,
            var index => ((TextStyle)Lines.GetStyleAtOffset(index + 1)).Span,
        };
}
