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

    internal static Art Resolved(LayoutNode layout, GlyphSource source, SKColorF under) {
        Art shown = source.Resolved(under);
        return shown.Tinted
            ? shown
            : shown with { Current = layout.Cascade.Theme.Color(layout.Style.Text, layout.Style).Map(ink => (SKColor)Styles.Legible(layout.Style, under, ink, layout.Cascade.Theme.Contrast.Graphic)) };
    }

    internal Fin<Unit> Retain(Seq<LayoutNode> drawn) {
        Seq<Art> shown = drawn.Choose(static layout => layout.Node.GlyphArt.Map(glyph => Resolved(layout, glyph, Styles.Background(layout.Cascade, layout.Style, layout.Node)))).Distinct().Strict();
        HashSet<Art> kept = toHashSet(shown);
        Seq<(Art Key, Fin<SKPicture> Picture)> loaded = shown.Filter(key => !Pictures.ContainsKey(key)).Map(key => (Key: key, Picture: Loaded(key))).Strict();
        Pictures = Released(Pictures, (key, _) => kept.Contains(key)).AddRange(loaded.Choose(static held => held.Picture.ToOption().Map(picture => (held.Key, picture))));
        return loaded.Traverse(static held => held.Picture).As().Map(static _ => unit);
    }

    public void Dispose() => Pictures = Released(Pictures, static (_, _) => false);

    private Fin<SKPicture> Loaded(Art glyph) {
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
