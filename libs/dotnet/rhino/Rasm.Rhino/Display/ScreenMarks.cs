using Rhino.Display;
using Rhino.DocObjects;

namespace Rasm.Rhino.Display;

// --- [MODELS] --------------------------------------------------------------------------
[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record ScreenMark {
    public sealed record Segment(System.Drawing.PointF From, System.Drawing.PointF To, Func<DisplayPen> Pen) : ScreenMark;
    public sealed record Chain(Seq<Point2f> Points, bool Closed, Func<DisplayPen> Pen) : ScreenMark;
    public sealed record Box(System.Drawing.PointF Center, float Width, float Height, float Radius, Func<System.Drawing.Color> Edge, float EdgeWidth, Func<System.Drawing.Color> Fill) : ScreenMark;
    public sealed record Outline(Curve Curve, Func<DisplayPen> Pen) : ScreenMark;
    public sealed record Caption(string Text, Func<System.Drawing.Color> Color, Point2d Origin, TextHorizontalAlignment Horizontal, TextVerticalAlignment Vertical, float Height, Font Font, bool Kerning) : ScreenMark;
    public sealed record Dot(System.Drawing.PointF Center, string Text, Func<System.Drawing.Color> Fill, Func<System.Drawing.Color> TextColor) : ScreenMark;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ScreenMarks {
    // --- [DRAW]
    public static IO<Unit> Draw(DisplayPipeline pipeline, ScreenMark mark) =>
        mark.Switch(pipeline,
            segment: static (target, segment) => IO.lift(() => target.Draw2dLine(segment.From, segment.To, segment.Pen())),
            chain: static (target, chain) => IO.lift(() => target.Draw2dPolyline([.. chain.Points], chain.Pen(), chain.Closed)),
            box: static (target, box) => IO.lift(() => target.DrawRoundedRectangle(box.Center, box.Width, box.Height, box.Radius, box.Edge(), box.EdgeWidth, box.Fill())),
            outline: static (target, outline) => Conduits.WithState(target, Seq<RenderState>(new RenderState.Projection2d()),
                IO.lift(() => target.DrawCurve(outline.Curve, outline.Pen()))),
            caption: static (target, caption) => IO.lift(() => target.Draw2dText(caption.Text, caption.Color(), caption.Origin, caption.Horizontal, caption.Vertical, caption.Height, caption.Font.QuartetName, caption.Font.Bold, caption.Font.Italic, caption.Font.Underlined, caption.Font.Strikeout, caption.Kerning)),
            dot: static (target, dot) => IO.lift(() => target.DrawDot(dot.Center.X, dot.Center.Y, dot.Text, dot.Fill(), dot.TextColor())));

    // --- [MEASURE]
    public static IO<Option<System.Drawing.Rectangle>> Measure(DisplayPipeline pipeline, string text, Point2d origin, bool middleJustified, int height, Font font) =>
        IO.lift(() => Conversions.Present(pipeline.Measure2dText(text, origin, middleJustified, rotationRadians: 0, height, font.QuartetName)));
}
