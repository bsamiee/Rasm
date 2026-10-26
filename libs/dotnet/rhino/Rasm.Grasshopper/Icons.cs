using System.Reflection;
using Eto.Drawing;
using Grasshopper2.Extensions;
using Grasshopper2.UI.Icon;
using Grasshopper2.UI.Icon.Vector;
using Rasm.Grasshopper.UI.Surfaces;
using Rasm.Rhino;
using Rasm.Rhino.UI.Styling;
using Rasm.UI;
using Rhino.UI;

namespace Rasm.Grasshopper;

// --- [SERVICES] ------------------------------------------------------------------------
internal sealed class GlyphIcon : AbstractIcon {
    private readonly IPlugInSink sink;
    private readonly Seq<(IIcon Light, IIcon Dark)> frames;
    private readonly IconState position;

    public GlyphIcon(IPlugInSink sink, Seq<(IIcon Light, IIcon Dark)> frames) : base(IconType.Vector) {
        this.sink = sink;
        this.frames = frames;
        position = new("", 0d, frames.Count - 1, 0d);
        AddState(position);
    }

    protected override void DrawInternal(IconContext context) =>
        Callbacks.Run(
            from value in IO.lift(() => position.Value)
            let lower = (int)Math.Floor(value)
            let upper = (float)(value - lower)
            let dark = context.Background.IsDarkBackground()
            let shown = frames.Map(frame => dark ? frame.Dark : frame.Light)
            let received = Optional(context.Filter).IfNone(identity)
            let weighted = fun((float weight) => context.WithFilter(compose(received, color => new Color(color, color.A * weight))))
            from _ in IO.lift(() => shown.At(lower).Iter(frame => frame.Draw(weighted(1f - upper))))
            from __ in IO.lift(() => shown.At(lower + 1).Iter(frame => frame.Draw(weighted(upper))))
            select unit,
            new CallbackSite(sink, typeof(GlyphIcon), nameof(DrawInternal)));
}

internal sealed class PictureIcon(IPlugInSink sink, GlyphSource.Picture art) : AbstractIcon(IconType.Vector), IDisposable {
    private static readonly Sizing Grown = new Sizing.Grow(Factor.Create(1f));
    private readonly Guest guest = new(sink, IO.pure(unit));
    private readonly Node view = new Node.Glyph(art) { Width = Grown, Height = Grown };

    protected override void DrawInternal(IconContext context) =>
        Callbacks.Run(
            from device in IO.lift(() => Guest.Device(context.Context))
            from painted in guest.Paint(context.Context.Graphics, device, context.Frame, Visibility.Shown, view)
            select unit,
            new CallbackSite(sink, typeof(PictureIcon), nameof(DrawInternal)));

    public void Dispose() {
        throw new NotImplementedException();
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class CanvasIcons {
    // --- [VECTORS]
    private static readonly Func<(Assembly Assembly, string Key), Fin<IIcon>> Parsed =
        memoUnsafe(static ((Assembly Assembly, string Key) resource) =>
            Refused.UnlessFound(ImageResources.SvgFromResourceId(resource.Key, resource.Assembly), nameof(ImageResources.SvgFromResourceId))
                .Map(static svg => (IIcon)SvgIcon.FromXml(svg)));

    public static IO<IIcon> Vector(IPlugInSink plugIn, GlyphSource glyph) =>
        Frame(plugIn, glyph).Map<IIcon>(frame => new GlyphIcon(plugIn, Seq(frame)));

    internal static IO<Option<IIcon>> PlugIn(IPlugInSink plugIn) =>
        Icons.PlugInImage(plugIn).Bind(found => found.Traverse(glyph => Vector(plugIn, glyph)).As());

    private static IO<(IIcon Light, IIcon Dark)> Frame(IPlugInSink plugIn, GlyphSource glyph) =>
        glyph.Switch(
            plugIn,
            tinted: static (_, held) => IO.lift(() => Parsed((held.Assembly, held.Resource))).Map(static icon => (icon, icon)),
            paired: static (_, held) => IO.lift(() =>
                from light in Parsed((held.Assembly, held.Light))
                from dark in Parsed((held.Assembly, held.Dark))
                select (light, dark)),
            picture: static (sink, held) => IO.lift(() => (IIcon)new PictureIcon(sink, held)).Map(static icon => (icon, icon)));

    // --- [COMPOSITES]
    public static IO<IIcon> Crossfade(IPlugInSink plugIn, IterableNE<GlyphSource> glyphs) =>
        Callbacks.Each(toSeq(glyphs).Map(glyph => Frame(plugIn, glyph)))
            .Map<IIcon>(frames => new GlyphIcon(plugIn, frames));
}
