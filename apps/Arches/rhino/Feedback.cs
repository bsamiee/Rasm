using Rasm.Rhino;
using Rasm.Rhino.Display.Feedback;
using Rhino.Display;

namespace Arches;

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class ArchFeedback {
    public static IO<Unit> DrawEnds(DisplayPipeline display, FeedbackAppearance appearance, Line baseline) => Marks.Draw(display, Ends(appearance, baseline));

    public static IO<Unit> Draw(DisplayPipeline display, FeedbackAppearance appearance, Line baseline, Plane frame, Func<Action<IDisposable>, IO<Option<Seq<Curve>>>> candidate) =>
        (Midpoint: baseline.PointAt(0.5), Reach: frame.YAxis * baseline.Length) switch {
            var span => envIO.Bind(environment =>
                DisposalOps.Within((Action<IDisposable> retain) =>
                 from curves in IO.liftVAsync(() => candidate(retain).RunAsync(environment))
                 from drawn in Marks.Draw(display, curves.Match(
                    Some: held => held.Bind(curve => curve is ArcCurve { Arc: var arc }
                            ? Seq<WorldMark>(
                                new RetainedMark.ArcMark(arc, appearance.Curve, appearance.CurveThickness),
                                new RetainedMark.Points(Seq(arc.Center, arc.MidPoint), appearance.ActivePoint, appearance.PointStyle))
                                + Ends(appearance, new Line(arc.StartPoint, arc.EndPoint))
                            : Seq<WorldMark>(new RetainedMark.CurveMark(curve, appearance.Curve, appearance.CurveThickness)))
                        + Seq<WorldMark>(
                            new RetainedMark.LineMark(new Line(span.Midpoint - span.Reach, span.Midpoint + span.Reach), appearance.Construction, appearance.ConstructionThickness),
                            new RetainedMark.Points(Seq(span.Midpoint), appearance.ActivePoint, appearance.PointStyle))
                        + Ends(appearance, baseline),
                    None: () => Ends(appearance, baseline)))
                 select drawn, DisposalOps.Release).Uninterruptible()),
        };

    private static Seq<WorldMark> Ends(FeedbackAppearance appearance, Line baseline) =>
        Seq<WorldMark>(
            new RetainedMark.LineMark(baseline, appearance.Construction, appearance.ConstructionThickness),
            new RetainedMark.Points(Seq(baseline.From, baseline.To), appearance.Construction, appearance.PointStyle));
}
