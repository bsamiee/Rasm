using Rasm.RhinoCommon.Commands;
using Rasm.RhinoCommon.Display;
using Rhino.Display;

namespace Arches;

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class ArchFeedback {
    public static IO<Unit> DrawEnds(DisplayPipeline display, Line baseline) => Marks.Draw(display, Ends(FeedbackAppearance.Of(display), baseline));

    public static IO<Unit> Draw(DisplayPipeline display, ArchSpan span, Option<IO<Seq<Curve>>> build) =>
        (from curves in build.Traverse(static held => held).As().Catch(Getters.Retried, static _ => IO.pure(Option<Seq<Curve>>.None))
         from drawn in Marks.Draw(display, Figure(FeedbackAppearance.Of(display), span, curves))
         select drawn).Bracket();

    private static Seq<RetainedMark> Figure(FeedbackAppearance look, ArchSpan span, Option<Seq<Curve>> curves) =>
        curves.Match(
            Some: held => held.Bind(curve => curve is ArcCurve { Arc: var arc }
                    ? Seq<RetainedMark>(
                        new RetainedMark.ArcMark(arc, look.Curve, look.Thickness),
                        new RetainedMark.Points(Seq(arc.Center, arc.MidPoint), look.ActivePoint, None))
                        + Ends(look, new Line(arc.StartPoint, arc.EndPoint))
                    : Seq<RetainedMark>(new RetainedMark.CurveMark(curve, look.Curve, look.Thickness)))
                + Seq<RetainedMark>(
                    new RetainedMark.LineMark(new Line(span.Apex(-span.Baseline.Length), span.Apex(span.Baseline.Length)), look.Construction, look.Thickness),
                    new RetainedMark.Points(Seq(span.Midpoint), look.ActivePoint, None))
                + Ends(look, span.Baseline),
            None: () => Ends(look, span.Baseline));

    private static Seq<RetainedMark> Ends(FeedbackAppearance look, Line baseline) =>
        Seq<RetainedMark>(
            new RetainedMark.LineMark(baseline, look.Construction, look.Thickness),
            new RetainedMark.Points(Seq(baseline.From, baseline.To), look.Construction, None));
}
