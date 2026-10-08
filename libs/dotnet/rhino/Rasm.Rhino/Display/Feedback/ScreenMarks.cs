using System.Drawing;
using Rhino.Display;
using Rhino.DocObjects;

namespace Rasm.Rhino.Display.Feedback;

// --- [MODELS] --------------------------------------------------------------------------
[Union]
public abstract partial record ScreenMark {
    public sealed record Segment(PointF From, PointF To, Stroke Stroke) : ScreenMark;

    public sealed record Chain(Seq<Point2f> Points, bool Closed, Stroke Stroke) : ScreenMark;

    public sealed record Box(PointF Center, DrawSize Width, DrawSize Height, DrawSize Radius, Ink Edge, DrawSize EdgeWidth, Ink Fill) : ScreenMark;

    public sealed record Outline(Curve Curve, Stroke Stroke) : ScreenMark;

    public sealed record Caption(
        string Text,
        Ink Color,
        Point2d Origin,
        TextHorizontalAlignment Horizontal,
        TextVerticalAlignment Vertical,
        DrawSize Height,
        global::Rhino.DocObjects.Font Font,
        bool Kerning) : ScreenMark;

    public sealed record Dot(PointF Center, string Text, Ink Fill, Ink TextColor) : ScreenMark;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ScreenMarks {
    // --- [DRAW]
    public static IO<Unit> Draw(DisplayPipeline pipeline, ScreenMark mark) =>
        mark.Switch(
            pipeline,
            segment: static (target, segment) => IO.lift(() => target.Draw2dLine(segment.From, segment.To, Strokes.Pen(segment.Stroke))),
            chain: static (target, chain) => IO.lift(() => target.Draw2dPolyline([.. chain.Points], Strokes.Pen(chain.Stroke), chain.Closed)),
            box: static (target, box) => IO.lift(() => target.DrawRoundedRectangle(box.Center, box.Width, box.Height, box.Radius, box.Edge.Drawn, box.EdgeWidth, box.Fill.Drawn)),
            outline: static (target, outline) => IO.lift(target.Push2dProjection).Bracket(
                Use: _ => IO.lift(() => target.DrawCurve(outline.Curve, Strokes.Pen(outline.Stroke))),
                Fin: _ => IO.lift(target.PopProjection)),
            caption: static (target, caption) => IO.lift(() => target.Draw2dText(
                caption.Text,
                caption.Color.Drawn,
                caption.Origin,
                caption.Horizontal,
                caption.Vertical,
                caption.Height,
                caption.Font.QuartetName,
                caption.Font.Bold,
                caption.Font.Italic,
                caption.Font.Underlined,
                caption.Font.Strikeout,
                caption.Kerning)),
            dot: static (target, dot) => IO.lift(() => target.DrawDot(dot.Center.X, dot.Center.Y, dot.Text, dot.Fill.Drawn, dot.TextColor.Drawn)));

    // --- [MEASURE]
    public static IO<Option<Rectangle>> Measure(DisplayPipeline pipeline, string text, Point2d origin, bool middleJustified, int height, global::Rhino.DocObjects.Font font) =>
        IO.lift(() => Conversions.Present(pipeline.Measure2dText(text, origin, middleJustified, 0.0, height, font.QuartetName)));
}
