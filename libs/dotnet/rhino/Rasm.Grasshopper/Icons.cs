using System.Reflection;
using Eto.Drawing;
using Grasshopper2.Extensions;
using Grasshopper2.UI.Icon;
using Grasshopper2.UI.Icon.Vector;
using Rasm.Rhino;
using Rasm.Rhino.UI.Styling;
using Rhino.Resources;
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

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class CanvasIcons {
    // --- [VECTORS]
    private static readonly Seq<string> DarkSuffixes = Seq("_darkmode", "_auto-darkmode");

    private static readonly Func<(Assembly Assembly, string Key), Fin<(IIcon Light, IIcon Dark)>> Parsed =
        memoUnsafe(static ((Assembly Assembly, string Key) resource) =>
            from svg in Refused.UnlessFound(ImageResources.SvgFromResourceId(resource.Key, resource.Assembly), nameof(ImageResources.SvgFromResourceId))
            let light = (IIcon)SvgIcon.FromXml(svg)
            let dark = DarkSuffixes
                .Choose(suffix => Optional(ImageResources.SvgFromResourceId(Path.ChangeExtension(resource.Key, extension: null) + suffix + Path.GetExtension(resource.Key), resource.Assembly)))
                .Head.Map(static xml => (IIcon)SvgIcon.FromXml(xml))
            select (light, dark.IfNone(light)));

    public static IO<IIcon> Vector(IPlugInSink plugIn, IGlyph glyph) => Vector(plugIn, glyph.Assets, glyph.Key);

    internal static IO<Option<IIcon>> PlugIn(IPlugInSink plugIn) =>
        Icons.PlugInImage(plugIn, (assets, key) => Vector(plugIn, assets, key)).Bind(static found => found.Traverse(static icon => icon).As());

    private static IO<IIcon> Vector(IPlugInSink plugIn, Assets assets, string key) =>
        Frame(assets, key).Map<IIcon>(frame => new GlyphIcon(plugIn, Seq(frame)));

    private static IO<(IIcon Light, IIcon Dark)> Frame(Assets assets, string key) =>
        IO.lift(() => Parsed((assets.Assembly, key)));

    // --- [COMPOSITES]
    public static IO<IIcon> Crossfade(IPlugInSink plugIn, IterableNE<IGlyph> glyphs) =>
        Callbacks.Each(toSeq(glyphs).Map(static glyph => Frame(glyph.Assets, glyph.Key)))
            .Map<IIcon>(frames => new GlyphIcon(plugIn, frames));
}
