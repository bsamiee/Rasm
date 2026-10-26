using System.Reflection;
using Eto.Drawing;
using Grasshopper2.UI.Icon;
using Grasshopper2.UI.Icon.Vector;
using Rasm.Grasshopper.UI.Surfaces;
using Rasm.Rhino;
using Rasm.Rhino.UI.Styling;
using Rasm.UI;
using Rhino.UI;
using SkiaSharp;

namespace Rasm.Grasshopper;

// --- [SERVICES] ------------------------------------------------------------------------
internal sealed class GlyphIcon : AbstractIcon, IDisposable {
    private readonly IPlugInSink sink;
    private readonly Seq<Func<SKColorF, Fin<IIcon>>> frames;
    private readonly Seq<IDisposable> owned;
    private readonly IconState position;

    public GlyphIcon(IPlugInSink sink, Seq<(Func<SKColorF, Fin<IIcon>> Shown, Option<IDisposable> Owned)> frames) : base(IconType.Vector) {
        this.sink = sink;
        this.frames = frames.Map(static frame => frame.Shown);
        owned = frames.Bind(static frame => frame.Owned.ToSeq());
        position = new("", 0d, frames.Count - 1, 0d);
        AddState(position);
    }

    protected override void DrawInternal(IconContext context) =>
        Callbacks.Run(
            from value in IO.lift(() => position.Value)
            let lower = (int)Math.Floor(value)
            let upper = (float)(value - lower)
            let under = Conversions.ToSKColorF(context.Background)
            let received = Optional(context.Filter).IfNone(identity)
            let weighted = fun((float weight) => context.WithFilter(compose(received, color => new Color(color, color.A * weight))))
            from _ in frames.At(lower).Traverse(frame => IO.lift(() => frame(under)).Bind(icon => IO.lift(() => icon.Draw(weighted(1f - upper))))).As()
            from __ in frames.At(lower + 1).Traverse(frame => IO.lift(() => frame(under)).Bind(icon => IO.lift(() => icon.Draw(weighted(upper))))).As()
            select unit,
            new CallbackSite(sink, typeof(GlyphIcon), nameof(DrawInternal)));

    public void Dispose() => DisposalOps.Release(owned).Run();
}

internal sealed class PictureIcon(IPlugInSink sink, GlyphSource.Picture art) : AbstractIcon(IconType.Vector), IDisposable {
    private readonly Guest guest = new(sink, IO.pure(unit));
    private readonly Node view = new Node.Glyph(art) { Width = Sizing.Grow.Unit, Height = Sizing.Grow.Unit };

    protected override void DrawInternal(IconContext context) =>
        Callbacks.Run(
            from device in IO.lift(() => Guest.Device(context.Context))
            from painted in guest.Paint(context.Context.Graphics, device, context.Frame, Visibility.Shown, view)
            select unit,
            new CallbackSite(sink, typeof(PictureIcon), nameof(DrawInternal)));

    public void Dispose() => guest.Dispose();
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

    private static IO<(Func<SKColorF, Fin<IIcon>> Shown, Option<IDisposable> Owned)> Frame(IPlugInSink plugIn, GlyphSource glyph) =>
        glyph.Switch(
            plugIn,
            tinted: static (_, held) => IO.lift(() => Parsed((held.Assembly, held.Resource)))
                .Map<(Func<SKColorF, Fin<IIcon>>, Option<IDisposable>)>(static icon => (_ => Fin.Succ(icon), None)),
            paired: static (_, held) => IO.lift(() =>
                from light in Parsed((held.Assembly, held.Light))
                from dark in Parsed((held.Assembly, held.Dark))
                select ((Func<SKColorF, Fin<IIcon>>)(under => Parsed((held.Assembly, held.Resource(under)))), Option<IDisposable>.None)),
            picture: static (sink, held) => IO.lift(() => new PictureIcon(sink, held))
                .Map<(Func<SKColorF, Fin<IIcon>>, Option<IDisposable>)>(static icon => (_ => icon, Some<IDisposable>(icon))));

    // --- [COMPOSITES]
    public static IO<IIcon> Crossfade(IPlugInSink plugIn, IterableNE<GlyphSource> glyphs) =>
        Callbacks.Each(toSeq(glyphs).Map(glyph => Frame(plugIn, glyph)))
            .Map<IIcon>(frames => new GlyphIcon(plugIn, frames));
}
