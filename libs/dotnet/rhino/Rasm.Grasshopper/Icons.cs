using Eto.Drawing;
using Eto.Forms;
using Grasshopper2.Extensions;
using Grasshopper2.UI.Icon;
using Grasshopper2.UI.Icon.Vector;
using Rasm.Rhino;
using Rasm.Rhino.UI.Assets;
using Rhino.UI;

namespace Rasm.Grasshopper;

// --- [SERVICES] ------------------------------------------------------------------------
internal sealed class SvgGlyphIcon(IPlugInSink sink, string source) : AbstractIcon(IconType.Vector) {
    protected override void DrawInternal(IconContext context) =>
        Callbacks.Run(
            from drawing in IO.lift(() => {
                RectangleF frame = context.Frame.ToInnerSquare();
                using IMatrix transform = context.Context.Graphics.CurrentTransform;
                PointF origin = transform.TransformPoint(frame.TopLeft);
                float density = context.Context.Bitmap is null && context.Context.Control is Control control
                    ? control.ParentWindow.LogicalPixelSize
                    : 1f;
                int pixels = (int)MathF.Ceiling(MathF.Max(
                    PointF.Distance(origin, transform.TransformPoint(frame.TopRight)),
                    PointF.Distance(origin, transform.TransformPoint(frame.BottomLeft))) * density);
                return (Frame: frame, Pixels: new Size(pixels, pixels));
            })
            from _ in drawing.Pixels switch {
                { Width: > 0, Height: > 0 } =>
                    Icons.RenderSvg(source, drawing.Pixels, Some(SpecialColour.ForegroundFirm.Resolve(context.Background, context.Palette)), bitmap => {
                        _ = Optional(context.Filter).Iter(filter => {
                            using BitmapData data = bitmap.Lock();
                            data.SetPixels(data.GetPixels().Select(filter));
                        });
                        context.Context.Graphics.DrawImage(bitmap, drawing.Frame);
                        return unit;
                    }),
                _ => IO.pure(unit),
            }
            select unit,
            new CallbackSite(sink, typeof(SvgGlyphIcon), nameof(DrawInternal)));
}

internal sealed class GlyphIcon : AbstractIcon {
    private readonly IPlugInSink sink;
    private readonly Seq<IIcon> frames;
    private readonly Seq<IIcon> overlays;
    private readonly IconState position;

    public GlyphIcon(IPlugInSink sink, Seq<IIcon> frames, Seq<IIcon> overlays) : base(IconType.Vector) {
        this.sink = sink;
        this.frames = frames;
        this.overlays = overlays;
        position = new("", 0d, frames.Count - 1, 0d);
        AddState(position);
    }

    protected override void DrawInternal(IconContext context) =>
        Callbacks.Run(
            from value in IO.lift(() => position.Value)
            let lower = (int)Math.Floor(value)
            let upper = (float)(value - lower)
            let received = Optional(context.Filter).IfNone(identity)
            let weighted = fun((float weight) => context.WithFilter(compose(received, colour => new Color(colour, colour.A * weight))))
            from _ in IO.lift(() => frames.At(lower).Iter(frame => frame.Draw(weighted(1f - upper))))
            from __ in IO.lift(() => frames.At(lower + 1).Iter(frame => frame.Draw(weighted(upper))))
            from ___ in IO.lift(() => overlays.Iter(overlay => overlay.Draw(context)))
            select unit,
            new CallbackSite(sink, typeof(GlyphIcon), nameof(DrawInternal)));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class CanvasIcons {
    // --- [VECTORS]
    public static IO<IIcon> Vector(IPlugInSink plugIn, IGlyph glyph, int master) =>
        IO.lift(() => Icons.ResourceName(glyph, master) switch {
            var resource => GlyphMissing.Unless(ImageResources.SvgFromResourceId(resource, glyph.GetType().Assembly), resource),
        }).Map(source => (IIcon)new SvgGlyphIcon(plugIn, source));

    // --- [COMPOSITES]
    public static IO<IIcon> Accented(IPlugInSink plugIn, IGlyph glyph, int master, SpecialColour accent) =>
        Vector(plugIn, glyph, master).Map<IIcon>(frame => {
            const double baseline = 21.5d;
            Builder bar = new(24);
            using IDisposable edge = bar.WithEdge(accent, 1f);
            bar.Line(3d, baseline, 21d, baseline);
            return new GlyphIcon(plugIn, Seq(frame), Seq<IIcon>(new VectorIcon(bar)));
        });

    public static IO<IIcon> Crossfade(IPlugInSink plugIn, IterableNE<IGlyph> glyphs, int master) =>
        Callbacks.Each(toSeq(glyphs).Map(glyph => Vector(plugIn, glyph, master)))
            .Map<IIcon>(frames => new GlyphIcon(plugIn, frames, Seq<IIcon>()));
}
