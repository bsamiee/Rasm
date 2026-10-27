using HarfBuzzSharp;
using SkiaSharp;
using SkiaSharp.HarfBuzz;
using Topten.RichTextKit;

namespace Rasm.UI.Typography;

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class Shaping : IDisposable {
    private sealed record Instance(SKTypeface Typeface, Seq<Variation> Variations);

    private HashMap<Instance, Font> fonts;

    internal HarfBuzzSharp.Buffer Buffer { get; } = new();

    internal StyleManager Faces { get; } = new();

    internal Font Shaped(SKTypeface typeface) {
        Instance instance = new(typeface, toSeq(typeface.VariationDesignPosition).Map(static at => new Variation { Tag = at.Axis, Value = at.Value }).Strict());
        return fonts.Find(instance).IfNone(() => {
            using Blob blob = typeface.OpenStream(out int index).ToHarfBuzzBlob();
            using Face face = new(blob, index);
            Font font = new(face);
            font.SetVariations([.. instance.Variations]);
            fonts = fonts.Add(instance, font);
            return font;
        });
    }

    public void Dispose() {
        _ = fonts.Values.Iter(static font => font.Dispose());
        fonts = [];
        Buffer.Dispose();
    }
}
