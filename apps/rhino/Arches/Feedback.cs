using Rasm.Rhino;
using Rasm.Rhino.Display;
using Rhino.Display;

namespace Arches;

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class ArchFeedback {
    public static IO<Unit> DrawEnds(DisplayPipeline display, Line baseline) => Marks.Draw(display, Ends(FeedbackAppearance.Of(display), baseline));

    public static IO<Unit> Draw(DisplayPipeline display, ArchSpan span, Option<Func<Action<IDisposable>, IO<Seq<Curve>>>> build) =>
        DisposalOps.Within((Action<IDisposable> retain) =>
            build.Match(
                    Some: held => held(retain).Map(static curves => Some(curves)).Catch(ArchPrompts.Rejected, static _ => IO.pure(Option<Seq<Curve>>.None)),
                    None: static () => IO.pure(Option<Seq<Curve>>.None))
                .Bind(curves => Marks.Draw(display, Figure(FeedbackAppearance.Of(display), span, curves))), DisposalOps.Release);

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
