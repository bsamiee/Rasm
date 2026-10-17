using Rasm.Rhino;
using Rasm.Rhino.Display;
using Rasm.Rhino.Document.Units;
using Rasm.Rhino.Modeling.Curves;
using Rhino.Display;

namespace Arches;

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class ArchFeedback {
    private static readonly FeedbackAppearance Appearance = new();

    public static IO<Unit> DrawEnds(DisplayPipeline display, Line baseline) => Marks.Draw(display, Ends(baseline));

    public static IO<Unit> Draw(DisplayPipeline display, ArchSpan span, Option<ArchForm> form, Tolerances tolerances) =>
        DisposalOps.Within((Action<IDisposable> retain) =>
            form.Match(
                    Some: held => CurveConstruction.CreateArch(span, held, tolerances, retain).Map(static curves => Some(curves)).Catch(ArchPrompts.Rejected, static _ => IO.pure(Option<Seq<Curve>>.None)),
                    None: static () => IO.pure(Option<Seq<Curve>>.None))
                .Bind(curves => Marks.Draw(display, Figure(span, curves))), DisposalOps.Release);

    private static Seq<WorldMark> Figure(ArchSpan span, Option<Seq<Curve>> curves) =>
        curves.Match(
            Some: held => held.Bind(static curve => curve is ArcCurve { Arc: var arc }
                    ? Seq<WorldMark>(
                        new RetainedMark.ArcMark(arc, Appearance.Curve, Appearance.CurveThickness),
                        new RetainedMark.Points(Seq(arc.Center, arc.MidPoint), Appearance.ActivePoint, Appearance.PointStyle))
                        + Ends(new Line(arc.StartPoint, arc.EndPoint))
                    : Seq<WorldMark>(new RetainedMark.CurveMark(curve, Appearance.Curve, Appearance.CurveThickness)))
                + Seq<WorldMark>(
                    new RetainedMark.LineMark(new Line(span.Apex(-span.Baseline.Length), span.Apex(span.Baseline.Length)), Appearance.Construction, Appearance.ConstructionThickness),
                    new RetainedMark.Points(Seq(span.Midpoint), Appearance.ActivePoint, Appearance.PointStyle))
                + Ends(span.Baseline),
            None: () => Ends(span.Baseline));

    private static Seq<WorldMark> Ends(Line baseline) =>
        Seq<WorldMark>(
            new RetainedMark.LineMark(baseline, Appearance.Construction, Appearance.ConstructionThickness),
            new RetainedMark.Points(Seq(baseline.From, baseline.To), Appearance.Construction, Appearance.PointStyle));
}
