using System.Globalization;
using System.Runtime.CompilerServices;
using HarfBuzzSharp;
using ICU4N.Text;
using Singulink.Collections;
using SkiaSharp;
using SkiaSharp.HarfBuzz;
using Topten.RichTextKit;

namespace Rasm.UI.Typography;

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class Shaping : IDisposable {
    // --- [STATE]
    private static readonly float[] WidthPercents = [50f, 62.5f, 75f, 87.5f, 100f, 112.5f, 125f, 150f, 200f];
    private static readonly HashMap<Tag, Func<TextStyle, SKFontVariationAxis, float>> Derived = HashMap<Tag, Func<TextStyle, SKFontVariationAxis, float>>(
        (Tag.Parse("wght"), static (style, _) => style.FontWeight),
        (Tag.Parse("wdth"), static (style, _) => WidthPercents[(int)style.FontWidth - 1]),
        (Tag.Parse("ital"), static (style, axis) => style.FontItalic ? axis.Max : axis.Min),
        (Tag.Parse("slnt"), static (style, axis) => style.FontItalic ? axis.Min : axis.Default),
        (Tag.Parse("opsz"), static (style, _) => style.FontSize));

    private readonly Lock gate = new();
    private readonly ConditionalWeakTable<SKTypeface, Face> faces = [];
    private readonly ConditionalWeakTable<SKTypeface, Font> fonts = [];
    private readonly ConditionalWeakTable<SKTypeface, Tuple<Seq<SKFontVariationPositionCoordinate>, HashMap<Tag, float>, Seq<SKFontVariationAxis>>> variations = [];
    private readonly WeakValueDictionary<(nint Typeface, Seq<SKFontVariationPositionCoordinate> Position, int Palette, Seq<SKFontPaletteOverride> Overrides), SKTypeface> clones = [];
    private readonly ConditionalWeakTable<SKTypeface, SKTypeface> bases = [];
    private readonly Seq<SKTypeface> embedded;

    public Shaping(Seq<SKTypeface> embedded = default) {
        this.embedded = embedded;
        Mapper = new FontMapping(this);
    }

    internal FontMapper Mapper { get; }

    internal HarfBuzzSharp.Buffer Buffer { get; } = new();

    internal BreakIterator Graphemes { get; } = BreakIterator.GetCharacterInstance(CultureInfo.InvariantCulture);

    internal Func<CultureInfo, BreakIterator> LineBreaks { get; } = memo<CultureInfo, BreakIterator>(BreakIterator.GetLineInstance);

    // --- [TYPEFACES]
    internal Option<SKTypeface> Matching(string family, SKFontStyleWeight weight, SKFontStyleWidth width, SKFontStyleSlant slant) =>
        Optional(embedded
            .Filter(face => string.Equals(face.FamilyName, family, StringComparison.OrdinalIgnoreCase))
            .MinBy(face => (face.IsItalic != (slant is not SKFontStyleSlant.Upright), Math.Abs(face.FontWeight - (int)weight), Math.Abs(face.FontWidth - (int)width))));

    internal SKTypeface Instance(SKTypeface based, TextStyle style) {
        Tuple<Seq<SKFontVariationPositionCoordinate>, HashMap<Tag, float>, Seq<SKFontVariationAxis>> metadata;
        lock (gate) metadata = variations.GetOrAdd(based, static face => {
            Seq<SKFontVariationPositionCoordinate> position = toSeq(face.VariationDesignPosition).Strict();
            return Tuple.Create(position, toHashMap(position.Map(static at => ((Tag)(uint)at.Axis, at.Value))), toSeq(face.VariationDesignParameters).Strict());
        });
        (Seq<SKFontVariationPositionCoordinate> original, HashMap<Tag, float> current, Seq<SKFontVariationAxis> parameters) = metadata;
        Seq<SKFontVariationPositionCoordinate> position = parameters.Map(axis => (Tag)(uint)axis.Tag switch {
            var tag => new SKFontVariationPositionCoordinate {
                Axis = axis.Tag,
                Value = float.Clamp((style.Variations.Find(tag) || Derived.Find(tag).Map(derive => derive(style, axis)) || current.Find(tag)).IfNone(axis.Default), axis.Min, axis.Max),
            },
        }).Strict();
        int palette = (style.Dark ? Some(HarfBuzzFace(based)) : None)
            .Bind(static face => Range(0, face.PaletteCount).Find(index => face.GetPaletteFlags(index).HasFlag(OpenTypeColorPaletteFlags.UsableWithDarkBackground))).IfNone(0);
        if (position == original && palette is 0 && style.Palette.IsEmpty) return based;
        lock (gate) {
            (nint, Seq<SKFontVariationPositionCoordinate>, int, Seq<SKFontPaletteOverride>) key = (based.Handle, position, palette, style.Palette);
            if (clones.TryGetValue(key, out SKTypeface? kept)) return kept;
            SKTypeface clone = based.Clone(new SKFontArguments { VariationDesignPosition = [.. position], PaletteIndex = palette, PaletteOverrides = [.. style.Palette] });
            if (ReferenceEquals(clone, based)) return based;
            bases.AddOrUpdate(clone, based);
            clones.Add(key, clone);
            return clone;
        }
    }

    private sealed class FontMapping(Shaping shaping) : FontMapper {
        public override SKTypeface TypefaceFromStyle(IStyle style, bool ignoreFontVariants) {
            TextStyle stated = (TextStyle)style;
            return (stated.Fallback || shaping.Matching(stated.FontFamily, (SKFontStyleWeight)stated.FontWeight, stated.FontWidth, stated.FontItalic ? SKFontStyleSlant.Italic : SKFontStyleSlant.Upright))
                .IfNone(() => base.TypefaceFromStyle(style, ignoreFontVariants));
        }
    }

    // --- [SHAPING]
    internal Font HarfBuzzFont(SKTypeface typeface) {
        lock (gate) return fonts.GetOrAdd(typeface, static (face, shaping) => {
            Font font = shaping.bases.TryGetValue(face, out SKTypeface? based) ? new Font(shaping.HarfBuzzFont(based)) : new Font(shaping.HarfBuzzFace(face));
            font.SetVariations([.. face.VariationDesignPosition.Select(static at => new Variation { Tag = (uint)at.Axis, Value = at.Value })]);
            return font;
        }, this);
    }

    private Face HarfBuzzFace(SKTypeface typeface) {
        lock (gate) return faces.GetOrAdd(typeface, static face => {
            using Blob blob = face.OpenStream(out int index).ToHarfBuzzBlob();
            return new Face(blob, index);
        });
    }

    // --- [LIFETIME]
    public void Dispose() {
        lock (gate) {
            SKTypeface[] held = [.. bases.Select(static entry => entry.Key)];
            foreach (Font font in fonts.Select(static entry => entry.Value)) font.Dispose();
            foreach (Face face in faces.Select(static entry => entry.Value)) face.Dispose();
            foreach (SKTypeface typeface in held) typeface.Dispose();
            clones.Dispose();
            bases.Clear();
            fonts.Clear();
            faces.Clear();
            variations.Clear();
            Buffer.Dispose();
        }
    }
}
