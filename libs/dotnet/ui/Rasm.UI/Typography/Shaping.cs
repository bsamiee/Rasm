using System.Globalization;
using HarfBuzzSharp;
using ICU4N.Text;
using SkiaSharp;
using SkiaSharp.HarfBuzz;
using Topten.RichTextKit;

namespace Rasm.UI.Typography;

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class Shaping : IDisposable {
    private readonly AtomHashMap<SKTypeface, Face> faces = AtomHashMap<SKTypeface, Face>();
    private readonly AtomHashMap<SKTypeface, Font> fonts = AtomHashMap<SKTypeface, Font>();
    private readonly Seq<SKTypeface> embedded;

    public Shaping(Seq<SKTypeface> embedded = default) {
        this.embedded = embedded;
        Mapper = new Varied(this);
    }

    internal FontMapper Mapper { get; }

    internal HarfBuzzSharp.Buffer Buffer { get; } = new();

    internal BreakIterator Graphemes { get; } = BreakIterator.GetCharacterInstance(CultureInfo.InvariantCulture);

    internal Func<CultureInfo, BreakIterator> LineBreaks { get; } = memo<CultureInfo, BreakIterator>(BreakIterator.GetLineInstance);

    internal Option<SKTypeface> Matching(string family, SKFontStyleWeight weight, SKFontStyleWidth width, SKFontStyleSlant slant) =>
        Optional(embedded
            .Filter(face => string.Equals(face.FamilyName, family, StringComparison.OrdinalIgnoreCase))
            .MinBy(face => (face.IsItalic != (slant is not SKFontStyleSlant.Upright), Math.Abs(face.FontWeight - (int)weight), Math.Abs(face.FontWidth - (int)width))));

    internal Font HarfBuzzFont(SKTypeface typeface) {
        Option<Font> parent = Varied.Bases.Find(typeface).Map(HarfBuzzFont);
        return fonts.FindOrAdd(typeface, () => {
            Font font = parent.Map(static based => new Font(based)).IfNone(() => new Font(HarfBuzzFace(typeface)));
            font.SetVariations([.. typeface.VariationDesignPosition.Select(static at => new Variation { Tag = (uint)at.Axis, Value = at.Value })]);
            return font;
        });
    }

    public void Dispose() {
        _ = fonts.Values.Iter(static font => font.Dispose());
        _ = faces.Values.Iter(static face => face.Dispose());
        _ = fonts.Clear();
        _ = faces.Clear();
        Buffer.Dispose();
    }

    private Face HarfBuzzFace(SKTypeface typeface) =>
        faces.FindOrAdd(typeface, () => {
            using Blob blob = typeface.OpenStream(out int index).ToHarfBuzzBlob();
            return new Face(blob, index);
        });

    private int DarkPalette(SKTypeface typeface) =>
        HarfBuzzFace(typeface) switch {
            var face => toSeq(Enumerable.Range(0, face.PaletteCount)).Find(index => face.GetPaletteFlags(index).HasFlag(OpenTypeColorPaletteFlags.UsableWithDarkBackground)).IfNone(0),
        };

    private sealed record CloneKey(SKTypeface Typeface, Seq<SKFontVariationPositionCoordinate> Position, int PaletteIndex, Seq<SKFontPaletteOverride> Palette);

    private sealed class Varied(Shaping shaping) : FontMapper {
        internal static readonly AtomHashMap<SKTypeface, SKTypeface> Bases = AtomHashMap<SKTypeface, SKTypeface>();
        private static readonly AtomHashMap<CloneKey, SKTypeface> Clones = AtomHashMap<CloneKey, SKTypeface>();
        private static readonly float[] WidthPercents = [50f, 62.5f, 75f, 87.5f, 100f, 112.5f, 125f, 150f, 200f];
        private static readonly HashMap<Tag, Func<TextStyle, SKFontVariationAxis, float>> Derived = HashMap<Tag, Func<TextStyle, SKFontVariationAxis, float>>(
            (Tag.Parse("wght"), static (style, _) => style.FontWeight),
            (Tag.Parse("wdth"), static (style, _) => WidthPercents[(int)style.FontWidth - 1]),
            (Tag.Parse("ital"), static (style, axis) => style.FontItalic ? axis.Max : axis.Min),
            (Tag.Parse("slnt"), static (style, axis) => style.FontItalic ? axis.Min : axis.Default),
            (Tag.Parse("opsz"), static (style, _) => style.FontSize));

        public override SKTypeface TypefaceFromStyle(IStyle style, bool ignoreFontVariants) {
            TextStyle stated = (TextStyle)style;
            return Instance((stated.Fallback | shaping.Matching(stated.FontFamily, (SKFontStyleWeight)stated.FontWeight, stated.FontWidth, stated.FontItalic ? SKFontStyleSlant.Italic : SKFontStyleSlant.Upright))
                .IfNone(() => base.TypefaceFromStyle(style, ignoreFontVariants)), stated);
        }

        private SKTypeface Instance(SKTypeface based, TextStyle style) {
            HashMap<Tag, float> current = toHashMap(based.VariationDesignPosition.Select(static at => ((Tag)(uint)at.Axis, at.Value)));
            Seq<SKFontVariationPositionCoordinate> position = toSeq(based.VariationDesignParameters).Map(axis => (Tag)(uint)axis.Tag switch {
                var tag => new SKFontVariationPositionCoordinate {
                    Axis = axis.Tag,
                    Value = float.Clamp((style.Variations.Find(tag) | Derived.Find(tag).Map(derive => derive(style, axis)) | current.Find(tag)).IfNone(axis.Default), axis.Min, axis.Max),
                },
            }).Strict();
            int palette = style.Dark ? shaping.DarkPalette(based) : 0;
            return position == toSeq(based.VariationDesignPosition) && palette is 0 && style.Palette.IsEmpty
                ? based
                : Clones.FindOrAdd(new CloneKey(based, position, palette, style.Palette), () => {
                    SKTypeface clone = based.Clone(new SKFontArguments { VariationDesignPosition = [.. position], PaletteIndex = palette, PaletteOverrides = [.. style.Palette] });
                    _ = Bases.TryAdd(clone, based);
                    return clone;
                });
        }
    }
}
