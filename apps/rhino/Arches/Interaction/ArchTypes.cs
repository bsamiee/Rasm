using Arches.Profiles;
using Rasm.Rhino.Display;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.ApplicationSettings;
using Rhino.Display;
using Rhino.UI;

namespace Arches.Interaction;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Context(RhinoDoc Doc, IO<Vector3d> Normal, Option<Point3d> Start);

public sealed record ArchType(LocalizeStringPair Name, Func<Context, IO<ArchProfile>> Run);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ArchTypes {
    private const int InitialFoilCount = 5;

    public static ArchType TwoPoint(LocalizeStringPair name, Func<Span, Fin<ArchProfile>> build) =>
        new(name, context =>
            from span in Prompts.PickSpan(context, flip: true, (display, picked) => ProfileMarks.Profile(display, picked, build(picked)))
            from profile in IO.lift(build(span))
            select profile);

    public static ArchType ThreePoint(LocalizeStringPair name, Func<Span, Point3d, Fin<ArchProfile>> build, Func<Span, Fin<Limits<double>>> rise) =>
        new(name, context =>
            from span in Prompts.PickSpan(context, flip: false, static (display, picked) => ProfileMarks.Ends(display, picked.Start, picked.End))
            from limits in IO.lift(rise(span))
            from apex in Prompts.PickApex(context.Doc, span, limits, (display, candidate) => ProfileMarks.Profile(display, span, build(span, candidate)))
            from profile in IO.lift(build(span, apex))
            select profile);

    public static ArchType Foil(LocalizeStringPair name, Func<Span, bool, Fin<ArchProfile>> build) =>
        new(name, context =>
            from picked in Prompts.PickFoilSpan(context, (display, span, pointed) => ProfileMarks.Profile(display, span, build(span, pointed)))
            from profile in IO.lift(build(picked.Span, picked.Pointed))
            select profile);

    public static ArchType FoilCount(LocalizeStringPair name, Func<Span, bool, int, Fin<ArchProfile>> build) =>
        new(name, context =>
            from picked in Prompts.PickFoilSpan(context, (display, span, pointed) => ProfileMarks.Profile(display, span, build(span, pointed, InitialFoilCount)))
            from count in Prompts.PickFoilCount(picked.Span, InitialFoilCount, (display, candidate) => ProfileMarks.Profile(display, picked.Span, build(picked.Span, picked.Pointed, candidate)))
            from profile in IO.lift(build(picked.Span, picked.Pointed, count))
            select profile);
}

public static class ProfileMarks {
    public static IO<Unit> Profile(DisplayPipeline display, Span span, Fin<ArchProfile> built) =>
        built.Match(
            Succ: profile => profile.Switch(
                display,
                arcs: static (pipeline, arcs) => Marks.DrawWorld(pipeline, arcs.Parts.Bind(static arc => ArcMarks(arc)) + SpanMarks(arcs.Span)),
                parabolic: static (pipeline, parabolic) => Curved(pipeline, parabolic.ToCurve(), parabolic.Span),
                elliptical: static (pipeline, elliptical) => Curved(pipeline, elliptical.ToCurve(), elliptical.Span)),
            Fail: _ => Ends(display, span.Start, span.End));

    public static IO<Unit> Ends(DisplayPipeline display, Point3d start, Point3d end) =>
        Marks.DrawWorld(display, EndPoints(start, end));

    private static IO<Unit> Curved(DisplayPipeline display, IO<Curve> curve, Span span) =>
        curve
            .Map(built => Disposal.Using(IO.pure(built), drawn => Marks.DrawWorld(display, Seq<WorldMark>(new WorldMark.CurveMark(drawn, AppearanceSettings.FeedbackColor)) + SpanMarks(span))))
            .IfFail(_ => Ends(display, span.Start, span.End))
            .Flatten();

    private static Seq<WorldMark> ArcMarks(Arc arc) =>
        Seq<WorldMark>(new WorldMark.ArcMark(arc, AppearanceSettings.FeedbackColor), new WorldMark.Points(Seq(arc.Center, arc.MidPoint), SmartTrackSettings.ActivePointColor, None))
        + EndPoints(arc.StartPoint, arc.EndPoint);

    private static Seq<WorldMark> SpanMarks(Span span) =>
        Seq<WorldMark>(new WorldMark.LineMark(span.CenterLine, AppearanceSettings.CrosshairColor), new WorldMark.Points(Seq(span.Midpoint), SmartTrackSettings.ActivePointColor, None))
        + EndPoints(span.Start, span.End);

    private static Seq<WorldMark> EndPoints(Point3d start, Point3d end) =>
        Seq<WorldMark>(new WorldMark.LineMark(new Line(start, end), AppearanceSettings.CrosshairColor), new WorldMark.Points(Seq(start, end), AppearanceSettings.CrosshairColor, None));
}
