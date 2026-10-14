using Rasm.Rhino;
using Rasm.Rhino.Display;
using Rhino.Display;

namespace Arches;

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class ArchFeedback {
    private static readonly FeedbackAppearance Appearance = new();

    public static IO<Unit> DrawEnds(DisplayPipeline display, Line baseline) => Marks.Draw(display, Ends(baseline));

    public static IO<Unit> Draw(DisplayPipeline display, Line baseline, Plane frame, Func<Action<IDisposable>, IO<Option<Seq<Curve>>>> candidate) =>
        (Midpoint: baseline.PointAt(0.5), Reach: frame.YAxis * baseline.Length) switch {
            var span => envIO.Bind(environment =>
                DisposalOps.Within((Action<IDisposable> retain) =>
                 from curves in IO.liftVAsync(() => candidate(retain).RunAsync(environment))
                 from drawn in Marks.Draw(display, Figure(baseline, span, curves))
                 select drawn, DisposalOps.Release).Uninterruptible()),
        };

    private static Seq<WorldMark> Figure(Line baseline, (Point3d Midpoint, Vector3d Reach) span, Option<Seq<Curve>> curves) =>
        curves.Match(
            Some: held => held.Bind(static curve => curve is ArcCurve { Arc: var arc }
                    ? Seq<WorldMark>(
                        new RetainedMark.ArcMark(arc, Appearance.Curve, Appearance.CurveThickness),
                        new RetainedMark.Points(Seq(arc.Center, arc.MidPoint), Appearance.ActivePoint, Appearance.PointStyle))
                        + Ends(new Line(arc.StartPoint, arc.EndPoint))
                    : Seq<WorldMark>(new RetainedMark.CurveMark(curve, Appearance.Curve, Appearance.CurveThickness)))
                + Seq<WorldMark>(
                    new RetainedMark.LineMark(new Line(span.Midpoint - span.Reach, span.Midpoint + span.Reach), Appearance.Construction, Appearance.ConstructionThickness),
                    new RetainedMark.Points(Seq(span.Midpoint), Appearance.ActivePoint, Appearance.PointStyle))
                + Ends(baseline),
            None: () => Ends(baseline));

    private static Seq<WorldMark> Ends(Line baseline) =>
        Seq<WorldMark>(
            new RetainedMark.LineMark(baseline, Appearance.Construction, Appearance.ConstructionThickness),
            new RetainedMark.Points(Seq(baseline.From, baseline.To), Appearance.Construction, Appearance.PointStyle));
}
