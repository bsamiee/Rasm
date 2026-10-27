using System.Globalization;
using HarfBuzzSharp;
using ICU4N.Text;
using SkiaSharp;
using SkiaSharp.HarfBuzz;
using Topten.RichTextKit;

namespace Rasm.UI.Typography;

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class Shaping(Seq<SKTypeface> embedded = default) : IDisposable {
    private readonly AtomHashMap<SKTypeface, Font> fonts = AtomHashMap<SKTypeface, Font>();

    internal Seq<SKTypeface> Embedded { get; } = embedded;

    internal FontMapper Mapper { get; } = new Varied(embedded);

    internal HarfBuzzSharp.Buffer Buffer { get; } = new();

    internal BreakIterator Graphemes { get; } = BreakIterator.GetCharacterInstance(CultureInfo.InvariantCulture);

    internal Font HarfBuzzFont(SKTypeface typeface) =>
        fonts.FindOrAdd(typeface, () => {
            using Blob blob = typeface.OpenStream(out int index).ToHarfBuzzBlob();
            using Face face = new(blob, index);
            Font font = new(face);
            font.SetVariations([.. typeface.VariationDesignPosition.Select(static at => new Variation { Tag = (uint)at.Axis, Value = at.Value })]);
            return font;
        });

    public void Dispose() {
        _ = fonts.Values.Iter(static font => font.Dispose());
        _ = fonts.Clear();
        Buffer.Dispose();
    }

    private sealed record Instance(SKTypeface Typeface, Seq<SKFontVariationPositionCoordinate> Position, Seq<SKFontPaletteOverride> Palette);

    private sealed class Varied(Seq<SKTypeface> embedded) : FontMapper {
        private static readonly AtomHashMap<Instance, SKTypeface> Clones = AtomHashMap<Instance, SKTypeface>();
        private static readonly float[] Widths = [50f, 62.5f, 75f, 87.5f, 100f, 112.5f, 125f, 150f, 200f];
        private static readonly HashMap<Tag, Func<TextStyle, SKFontVariationAxis, float>> Derived = HashMap<Tag, Func<TextStyle, SKFontVariationAxis, float>>(
            (Tag.Parse("wght"), static (style, _) => style.FontWeight),
            (Tag.Parse("wdth"), static (style, _) => Widths[(int)style.FontWidth - 1]),
            (Tag.Parse("ital"), static (style, axis) => style.FontItalic ? axis.Max : axis.Min),
            (Tag.Parse("slnt"), static (style, axis) => style.FontItalic ? axis.Min : axis.Default),
            (Tag.Parse("opsz"), static (style, _) => style.FontSize));

        public override SKTypeface TypefaceFromStyle(IStyle style, bool ignoreFontVariants) {
            TextStyle stated = (TextStyle)style;
            return Instanced((stated.Matched | Optional(embedded
                    .Filter(face => string.Equals(face.FamilyName, stated.FontFamily, StringComparison.OrdinalIgnoreCase))
                    .MinBy(face => (face.IsItalic != stated.FontItalic, Math.Abs(face.FontWeight - stated.FontWeight)))))
                .IfNone(() => base.TypefaceFromStyle(style, ignoreFontVariants)), stated);
        }

        private static SKTypeface Instanced(SKTypeface based, TextStyle style) {
            HashMap<Tag, float> current = toHashMap(based.VariationDesignPosition.Select(static at => ((Tag)(uint)at.Axis, at.Value)));
            Seq<SKFontVariationPositionCoordinate> position = toSeq(based.VariationDesignParameters).Map(axis => (Tag)(uint)axis.Tag switch {
                var tag => new SKFontVariationPositionCoordinate {
                    Axis = axis.Tag,
                    Value = float.Clamp((style.Variations.Find(tag) | Derived.Find(tag).Map(derive => derive(style, axis)) | current.Find(tag)).IfNone(axis.Default), axis.Min, axis.Max),
                },
            }).Strict();
            return position == toSeq(based.VariationDesignPosition) && style.Palette.IsEmpty
                ? based
                : Clones.FindOrAdd(new Instance(based, position, style.Palette), () => based.Clone(new SKFontArguments { VariationDesignPosition = [.. position], PaletteOverrides = [.. style.Palette] }));
        }
    }
}
