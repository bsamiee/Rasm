using Arches.Profiles;
using Rhino;
using Rhino.ApplicationSettings;
using Rhino.Display;
using Rhino.UI;

namespace Arches.Interaction;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ProfileMarks {
    public static IO<Unit> Profile(DisplayPipeline display, Span span, Fin<ArchProfile> built) =>
        built.Match(
            Succ: profile => profile.Switch(
                (Display: display, profile.Span),
                arcs: static (state, arcs) => Marks.DrawWorld(state.Display, arcs.Parts.Bind(static arc => ArcMarks(arc)) + SpanMarks(state.Span)),
                parabolic: static (state, parabolic) => Curved(state.Display, parabolic.Curve, state.Span),
                elliptical: static (state, elliptical) => Curved(state.Display, elliptical.Curve, state.Span)),
            Fail: _ => Ends(display, span.Start, span.End));

    public static IO<Unit> Ends(DisplayPipeline display, Point3d start, Point3d end) =>
        Marks.DrawWorld(display, EndPoints(start, end));

    private static IO<Unit> Curved(DisplayPipeline display, IO<NurbsCurve> curve, Span span) =>
        DisposalOps.Using(curve, drawn => Marks.DrawWorld(display, Seq<RetainedMark>(new RetainedMark.CurveMark(drawn, AppearanceSettings.FeedbackColor)) + SpanMarks(span)))
            .IfFail(_ => Ends(display, span.Start, span.End));

    private static Seq<RetainedMark> ArcMarks(Arc arc) =>
        Seq<RetainedMark>(new RetainedMark.ArcMark(arc, AppearanceSettings.FeedbackColor), new RetainedMark.Points(Seq(arc.Center, arc.MidPoint), SmartTrackSettings.ActivePointColor))
        + EndPoints(arc.StartPoint, arc.EndPoint);

    private static Seq<RetainedMark> SpanMarks(Span span) =>
        Seq<RetainedMark>(new RetainedMark.LineMark(span.CenterLine, AppearanceSettings.CrosshairColor), new RetainedMark.Points(Seq(span.Midpoint), SmartTrackSettings.ActivePointColor))
        + EndPoints(span.Start, span.End);

    private static Seq<RetainedMark> EndPoints(Point3d start, Point3d end) =>
        Seq<RetainedMark>(new RetainedMark.LineMark(new Line(start, end), AppearanceSettings.CrosshairColor), new RetainedMark.Points(Seq(start, end), AppearanceSettings.CrosshairColor));
}

public static class Prompts {
    // --- [TYPE]
    public static IO<ArchProfile> PickType(string prompt, IterableNE<ArchType> types, Context context) =>
        Getters.GetPoint(
            new PointRequest<ArchProfile>(prompt) { Options = toSeq(types.Map<OptionSpec<ArchProfile>>(type => new OptionSpec<ArchProfile>.Simple(type.Name, None, Hidden: false, type.Run(context)))) },
            onMouseUp: false,
            picked => IO.lift(Anchored(picked.Pick)).Bind(start => types.Head.Run(context with { Start = Some(start) })));

    // --- [SPAN]
    public static IO<Span> PickSpan(Context context, bool flip, Func<DisplayPipeline, Span, IO<Unit>> draw) =>
        SpanLoop(context, flip ? Some(value: false) : None, pointed: None, (display, span, _) => draw(display, span)).Map(static picked => picked.Span);

    public static IO<(Span Span, bool Pointed)> PickFoilSpan(Context context, Func<DisplayPipeline, Span, bool, IO<Unit>> draw) =>
        SpanLoop(context, flip: Some(value: false), pointed: Some(value: false), draw);

    private static IO<(Span Span, bool Pointed)> SpanLoop(Context context, Option<bool> flip, Option<bool> pointed, Func<DisplayPipeline, Span, bool, IO<Unit>> draw) =>
        Getters.Loop(
            new SpanState(context.Start, BothSides: false, flip, pointed),
            state => Getters.GetPoint(
                (state.Start.Case is Anchor start
                    ? new PointRequest<Next<SpanState, (Span Span, bool Pointed)>>(LOC.STR("Second end point")) {
                        Configure = getter => IO.lift(() => getter.SetBasePoint(start.Point, showDistanceInStatusBar: true)),
                        Handlers = [
                            new PointHandler.DynamicDraw(args => Spanned(context, state, start, args.CurrentPoint).Match(
                                Succ: span => draw(args.Display, span, state.IsPointed),
                                Fail: _ => ProfileMarks.Ends(args.Display, MirroredStart(state, start.Point, args.CurrentPoint), args.CurrentPoint))),
                        ],
                    }
                    : new PointRequest<Next<SpanState, (Span Span, bool Pointed)>>(LOC.STR("First end point")))
                with {
                    Options = Toggles(context, state),
                },
                onMouseUp: false,
                picked => state.Start.Case is Anchor anchor
                    ? IO.lift(Spanned(context, state, anchor, picked.Pick.Value)).Map(span => Next.Done<SpanState, (Span Span, bool Pointed)>((span, state.IsPointed)))
                    : IO.lift(Anchored(picked.Pick)).Map(first => Next.Loop<SpanState, (Span Span, bool Pointed)>(state with { Start = Some(first) }))));

    private static Seq<OptionSpec<Next<SpanState, (Span Span, bool Pointed)>>> Toggles(Context context, SpanState state) =>
        state.Start.IsSome
            ? context.Start.Map(_ => Sides(state)).ToSeq()
              + state.Flip.Map(flip => Toggle(LOC.CON("Flip"), flip, LOC.CON("No"), LOC.CON("Yes"), value => state with { Flip = Some(value) })).ToSeq()
              + state.Pointed.Map(pointed => Toggle(LOC.CON("Shape"), pointed, LOC.CON("Rounded"), LOC.CON("Pointed"), value => state with { Pointed = Some(value) })).ToSeq()
            : Seq(Sides(state));

    private static OptionSpec<Next<SpanState, (Span Span, bool Pointed)>> Sides(SpanState state) =>
        Toggle(LOC.CON("BothSides"), state.BothSides, LOC.CON("No"), LOC.CON("Yes"), value => state with { BothSides = value });

    private static OptionSpec<Next<SpanState, (Span Span, bool Pointed)>> Toggle(LocalizeStringPair name, bool initial, LocalizeStringPair off, LocalizeStringPair on, Func<bool, SpanState> next) =>
        new OptionSpec<Next<SpanState, (Span Span, bool Pointed)>>.Toggle(name, initial, off, on, value => IO.pure(Next.Loop<SpanState, (Span Span, bool Pointed)>(next(value))));

    private static Fin<Span> Spanned(Context context, SpanState state, Anchor start, Point3d end) =>
        Span.From(MirroredStart(state, start.Point, end), end, state.Flip.Case is true ? -start.Normal : start.Normal, context.Tolerance);

    private static Point3d MirroredStart(SpanState state, Point3d start, Point3d end) =>
        state.BothSides ? start + (start - end) : start;

    private static Fin<Anchor> Anchored(PointPick pick) =>
        pick.View.Map(view => new Anchor(pick.Value, view.ConstructionPlane.Normal)).ToFin(new Missing(nameof(PointPick.View)));

    private sealed record SpanState(Option<Anchor> Start, bool BothSides, Option<bool> Flip, Option<bool> Pointed) {
        public bool IsPointed => Pointed.Case is true;
    }

    // --- [APEX]
    private static readonly string RisePrompt = LOC.STR("Rise");

    public static IO<Point3d> PickApex(RhinoDoc doc, Span span, Limits<double> limits, Func<DisplayPipeline, Point3d, IO<Unit>> draw) =>
        from cursor in IO.lift(static () => Atom(Option<Point3d>.None))
        from apex in Getters.Loop(
            new ApexState(ByRise: false, Apex: None),
            state => Getters.GetPoint(
                (state.ByRise
                    ? new PointRequest<Next<ApexState, Point3d>>(RisePrompt) {
                        Accept = new() {
                            Nothing = IO.lift(state.Apex.Map(Next.Done<ApexState, Point3d>).ToFin(new NothingEntered())),
                            Number = (Zero: true, Then: fun((double rise) => IO.lift(limits.Check(Math.Abs(rise), RisePrompt, bound => $"{doc.FormatNumber(bound)}"))
                                .Map(_ => Next.Loop<ApexState, Point3d>(state with { Apex = Some(span.Midpoint + (span.Frame.YAxis * rise)) })))),
                        },
                    }
                    : new PointRequest<Next<ApexState, Point3d>>(LOC.STR("Apex point")) {
                        Options = [
                            new OptionSpec<Next<ApexState, Point3d>>.Simple(
                                LOC.CON("Rise"),
                                None,
                                Hidden: false,
                                cursor.ValueIO.Map(point => Next.Loop<ApexState, Point3d>(new ApexState(ByRise: true, Apex: point.Map(at => span.Raised(at, limits)))))),
                        ],
                    })
                with {
                    Handlers = [new PointHandler.DynamicDraw(args => cursor.SwapIO(_ => Some(args.CurrentPoint)).Bind(_ => draw(args.Display, Candidate(span, limits, state, args.CurrentPoint))))],
                },
                onMouseUp: false,
                picked => IO.pure(Next.Done<ApexState, Point3d>(Candidate(span, limits, state, picked.Pick.Value)))))
        select apex;

    private static Point3d Candidate(Span span, Limits<double> limits, ApexState state, Point3d point) =>
        state.Apex.IfNone(() => span.Raised(point, limits));

    private sealed record ApexState(bool ByRise, Option<Point3d> Apex);

    // --- [FOILS]
    private static readonly string FoilCountPrompt = LOC.STR("Foil count");

    public static IO<int> PickFoilCount(Span span, int count, Func<DisplayPipeline, int, IO<Unit>> draw) =>
        Getters.Loop(
            count,
            current => Getters.GetPoint(
                new PointRequest<Next<int, int>>(FoilCountPrompt) {
                    Accept = new() {
                        Nothing = IO.pure(Next.Done<int, int>(current)),
                        Number = (Zero: true, Then: fun(static (double typed) => IO.lift(Multifoil.FoilCount.Check((int)typed, FoilCountPrompt)).Map(static valid => Next.Loop<int, int>(valid)))),
                    },
                    Options = [new OptionSpec<Next<int, int>>.Integer(LOC.CON("FoilCount"), current, Some(Multifoil.MinimumFoilCount), None, None, static chosen => IO.pure(Next.Loop<int, int>(chosen)))],
                    Configure = getter => IO.lift(() => getter.SetBasePoint(span.Start, showDistanceInStatusBar: true)),
                    Handlers = [new PointHandler.DynamicDraw(args => draw(args.Display, current))],
                },
                onMouseUp: false,
                _ => IO.pure(Next.Done<int, int>(current))));
}
