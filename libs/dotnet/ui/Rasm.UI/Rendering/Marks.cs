using Rasm.UI.Plotting;
using Rasm.UI.Styling;
using SkiaSharp;

namespace Rasm.UI.Rendering;

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class Marks {
    internal static void Pictured(SKCanvas canvas, Painter.Pass pass, Laid laid, Context context, Node.Image image, SKRect content) {
        RoleStyle style = laid.Style;
        SKRect target = image.Fit.Fitted(content, new SKSize(image.Source.Width, image.Source.Height));
        SKRect device = context.Device.MapRect(target);
        Option<SKColorF> color = image.Source.IsAlphaOnly ? (style.Strokes.Head.Map(static stroke => stroke.Color) | style.Solid).Bind(value => pass.Theme.Washed(value, style, context.Washes, series: image.Series)) : None;
        using SKAutoCanvasRestore restore = new(canvas);
        canvas.ClipRect(content, antialias: true);
        canvas.DrawImage(image.Source, target, image.Rendering.Sampling(MathF.Min(device.Width / image.Source.Width, device.Height / image.Source.Height)), pass.Cache.Imaged(laid, style.Blend, color));
    }

    internal static void Marked(SKCanvas canvas, Painter.Pass pass, Laid laid, Context context, Node.Mark mark, PlotFrame frame) {
        RoleStyle style = laid.Style;
        Pen pen = context.Pen(style);
        if (!style.Fills.IsEmpty) {
            using SKPath interior = Outline.Traced(builder => mark.Figure.Interior(builder, frame, pen));
            _ = style.Fills.Map(static (fill, index) => (Fill: fill, Index: index)).Iter(held => Painter.Filled(new Painter.Filling(pass, laid, context, interior.Bounds, held.Index, style.Blend, mark.Series), held.Fill)
                .Iter(paint => Painter.Covered(canvas, held.Fill, interior.Bounds, clip => clip.ClipPath(interior, antialias: true), () => canvas.DrawPath(interior, paint))));
        }
        Seq<(Stroke Stroke, SKColorF Color, int Index)> edges = style.Strokes.Map((stroke, index) => pass.Theme.Washed(stroke.Color, style, context.Washes, series: mark.Series)
            .Map(color => (Stroke: stroke, Color: color, Index: index))).Somes().Strict();
        if (edges.IsEmpty) return;
        using SKPath edge = Outline.Traced(builder => mark.Figure.Edges(builder, frame, pen));
        _ = edges.Iter(held => canvas.DrawPath(edge, pass.Cache.Edged(laid, held.Index, held.Color, pen.Stroke, held.Stroke.Rhythm(mark.Series.Map(static series => series.Ordinal).IfNone(0u)), style.Blend)));
    }
}
