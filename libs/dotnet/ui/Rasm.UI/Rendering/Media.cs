using System.Drawing;
using Rasm.UI.Styling;
using SkiaSharp;
using Svg.Model;
using Svg.Model.Services;
using Svg.Skia;

namespace Rasm.UI.Rendering;

// --- [SERVICES] ------------------------------------------------------------------------
internal sealed class Media : IDisposable {
    private readonly SkiaModel model = new(new SKSvgSettings());
    private readonly SkiaSvgAssetLoader loader;

    internal Media() => loader = new SkiaSvgAssetLoader(model);

    internal HashMap<Art, SKPicture> Pictures { get; private set; }

    internal static Art Shown(Laid laid, GlyphSource source, SKColorF under) {
        Art shown = source.Shown(under);
        return shown.Tinted
            ? shown
            : shown with { Current = laid.Cascade.Theme.Color(laid.Style.Text, laid.Style).Map(ink => (SKColor)Styles.Legible(laid.Style, under, ink, laid.Cascade.Theme.Contrast.Graphic)) };
    }

    internal Fin<Unit> Retain(Seq<Laid> drawn) {
        Seq<Art> shown = drawn.Choose(static laid => laid.Node.Glyphed.Map(glyph => Shown(laid, glyph, Styles.Grounded(laid.Cascade, laid.Style, laid.Node)))).Distinct().Strict();
        HashSet<Art> kept = toHashSet(shown);
        Seq<(Art Key, Fin<SKPicture> Picture)> loaded = shown.Filter(key => !Pictures.ContainsKey(key)).Map(key => (Key: key, Picture: Recorded(key))).Strict();
        Pictures = Released(Pictures, (key, _) => kept.Contains(key)).AddRange(loaded.Choose(static held => held.Picture.ToOption().Map(picture => (held.Key, picture))));
        return loaded.Traverse(static held => held.Picture).As().Map(static _ => unit);
    }

    public void Dispose() => Pictures = Released(Pictures, static (_, _) => false);

    private Fin<SKPicture> Recorded(Art glyph) {
        using Stream? stream = glyph.Assembly.GetManifestResourceStream(glyph.Resource);
        return from held in Optional(stream).ToFin(new UIFault(glyph.Resource, UIReason.Missing))
               from document in Optional(SvgService.Open(held, new SvgParameters(Entities: null, Css: null, CurrentColor: glyph.Current.Map(static ink => Color.FromArgb(ink.Alpha, ink.Red, ink.Green, ink.Blue)).ToNullable())))
                   .ToFin(new UIFault(glyph.Resource, UIReason.Unreadable))
               from picture in Optional(SKSvg.ToPicture(document, model, loader)).ToFin(new UIFault(glyph.Resource, UIReason.Unreadable))
               select picture;
    }

    private static HashMap<Art, SKPicture> Released(HashMap<Art, SKPicture> held, Func<Art, SKPicture, bool> kept) {
        _ = held.Filter((key, value) => !kept(key, value)).Values.Iter(static picture => picture.Dispose());
        return held.Filter(kept);
    }
}
