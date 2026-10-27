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

public sealed record TextStyle(IStyle Style, Seq<Feature> Features);

public sealed record Typeset(TextBlock Lines, SKSize Size, bool Clipped) {
    public bool Truncated => Lines.Truncated || Clipped;
    public Option<float> Baseline => toSeq(Lines.Lines).Head.Map(static line => line.YCoord + line.BaseLine);
}
