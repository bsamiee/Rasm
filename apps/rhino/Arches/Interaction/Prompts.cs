using Arches.Profiles;
using Rasm.Rhino.Commands;
using Rasm.Rhino.Display;
using Rasm.Rhino.Document;
using Rasm.Rhino.Viewport;
using Rhino;
using Rhino.ApplicationSettings;
using Rhino.Display;
using Rhino.Input.Custom;
using Rhino.UI;

namespace Arches.Interaction;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Prompts {
    // --- [TYPE]
    public static IO<ArchProfile> PickType(string prompt, IterableNE<ArchType> types, Context context) =>
        Getters.GetPoint(
            _ => new PointRequest<ArchProfile>(prompt) { Options = toSeq(types.Map<OptionSpec<ArchProfile>>(type => new OptionSpec<ArchProfile>.Simple(type.Name, None, Hidden: false, type.Run(context)))) },
            onMouseUp: false,
            picked => Anchored(context.Doc, picked.Pick).Bind(start => types.Head.Run(context with { Start = Some(start) })));

    // --- [SPAN]
    public static IO<Span> PickSpan(Context context, bool flip, Func<DisplayPipeline, Span, IO<Unit>> draw) =>
        SpanLoop(context, flip ? Some(value: false) : None, pointed: None, (display, span, _) => draw(display, span)).Map(static picked => picked.Span);

    public static IO<(Span Span, bool Pointed)> PickFoilSpan(Context context, Func<DisplayPipeline, Span, bool, IO<Unit>> draw) =>
        SpanLoop(context, flip: Some(value: false), pointed: Some(value: false), draw);

    private static IO<(Span Span, bool Pointed)> SpanLoop(Context context, Option<bool> flip, Option<bool> pointed, Func<DisplayPipeline, Span, bool, IO<Unit>> draw) =>
        Getters.Loop(
            new SpanState(context.Start, BothSides: false, flip, pointed),
            state => Getters.GetPoint(_ => SpanRequest(context, state, draw), onMouseUp: false, picked => SpanEnd(context, state, picked.Pick)));

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

    private static PointRequest<Next<SpanState, (Span Span, bool Pointed)>> SpanRequest(Context context, SpanState state, Func<DisplayPipeline, Span, bool, IO<Unit>> draw) =>
        state.Start.Match(
            Some: start => new PointRequest<Next<SpanState, (Span Span, bool Pointed)>>(LOC.STR("Second end point")) {
                Settings = new() { Base = Some(new BasePoint(start.Point, ShowDistance: true)) },
                Handlers = DynamicDraw((display, end) => Spanned(context, state, start, end).Match(
                    Succ: span => draw(display, span, state.IsPointed),
                    Fail: _ => ProfileMarks.Ends(display, MirroredStart(state, start.Point, end), end))),
            },
            None: static () => new PointRequest<Next<SpanState, (Span Span, bool Pointed)>>(LOC.STR("First end point")))
        with {
            Options = Toggles(context, state),
        };

    private static IO<Next<SpanState, (Span Span, bool Pointed)>> SpanEnd(Context context, SpanState state, PointPick pick) =>
        state.Start.Match(
            Some: start => IO.lift(Spanned(context, state, start, pick.Value)).Map(span => Next.Done<SpanState, (Span Span, bool Pointed)>((span, state.IsPointed))),
            None: () => Anchored(context.Doc, pick).Map(start => Next.Loop<SpanState, (Span Span, bool Pointed)>(state with { Start = Some(start) })));

    private static Fin<Span> Spanned(Context context, SpanState state, Anchor start, Point3d end) =>
        Span.From(MirroredStart(state, start.Point, end), end, state.Flip.Case is true ? -start.Normal : start.Normal, context.Tolerance);

    private static Point3d MirroredStart(SpanState state, Point3d start, Point3d end) =>
        state.BothSides ? start + (start - end) : start;

    private static IO<Anchor> Anchored(RhinoDoc doc, PointPick pick) =>
        from viewport in IO.lift(pick.Viewport.ToFin(new Missing(nameof(PointPick.Viewport))))
        from row in Viewports.ResolveViewport(doc, new ViewportTarget.Id(viewport.Id))
        from cplane in Cameras.GetConstructionPlane(row.Viewport)
        select new Anchor(pick.Value, cplane.Plane.Normal);

    private sealed record SpanState(Option<Anchor> Start, bool BothSides, Option<bool> Flip, Option<bool> Pointed) {
        public bool IsPointed => Pointed.Case is true;
    }

    // --- [APEX]
    private static readonly string RisePrompt = LOC.STR("Rise");

    public static IO<Point3d> PickApex(RhinoDoc doc, Span span, Limits<double> limits, Func<DisplayPipeline, Point3d, IO<Unit>> draw) =>
        Getters.Loop(
            new ApexState(ByRise: false, Apex: None),
            state => Getters.GetPoint(
                cursor => ApexRequest(doc, span, limits, state, cursor, draw),
                onMouseUp: false,
                picked => IO.pure(Next.Done<ApexState, Point3d>(Candidate(span, limits, state, picked.Pick.Value)))));

    private static PointRequest<Next<ApexState, Point3d>> ApexRequest(RhinoDoc doc, Span span, Limits<double> limits, ApexState state, IO<Option<Point3d>> cursor, Func<DisplayPipeline, Point3d, IO<Unit>> draw) =>
        (state.ByRise
            ? new PointRequest<Next<ApexState, Point3d>>(RisePrompt) {
                Accept = new() {
                    Nothing = IO.lift(state.Apex.Map(Next.Done<ApexState, Point3d>).ToFin(new NothingEntered())),
                    Number = (Zero: true, Then: fun((double rise) => TypedRise(doc, span, limits, rise).Map(apex => Next.Loop<ApexState, Point3d>(state with { Apex = Some(apex) })))),
                },
            }
            : new PointRequest<Next<ApexState, Point3d>>(LOC.STR("Apex point")) {
                Options = Seq<OptionSpec<Next<ApexState, Point3d>>>(new OptionSpec<Next<ApexState, Point3d>>.Simple(
                    LOC.CON("Rise"),
                    None,
                    Hidden: false,
                    cursor.Map(point => Next.Loop<ApexState, Point3d>(new ApexState(ByRise: true, Apex: point.Map(at => span.Raised(at, limits))))))),
            })
        with {
            Handlers = DynamicDraw((display, point) => draw(display, Candidate(span, limits, state, point))),
        };

    private static Point3d Candidate(Span span, Limits<double> limits, ApexState state, Point3d point) =>
        state.Apex.IfNone(() => span.Raised(point, limits));

    private static IO<Point3d> TypedRise(RhinoDoc doc, Span span, Limits<double> limits, double rise) =>
        IO.lift(() => limits.Check(Math.Abs(rise), RisePrompt, bound => new ModelDistance(doc, bound)).Map(_ => span.Midpoint + (span.Frame.YAxis * rise)));

    private sealed record ApexState(bool ByRise, Option<Point3d> Apex);

    // --- [FOILS]
    private static readonly string FoilCountPrompt = LOC.STR("Foil count");

    public static IO<int> PickFoilCount(Span span, int count, Func<DisplayPipeline, int, IO<Unit>> draw) =>
        Getters.Loop(count, current => Getters.GetPoint(_ => FoilCountRequest(span, current, draw), onMouseUp: false, _ => IO.pure(Next.Done<int, int>(current))));

    private static PointRequest<Next<int, int>> FoilCountRequest(Span span, int count, Func<DisplayPipeline, int, IO<Unit>> draw) =>
        new(FoilCountPrompt) {
            Accept = new() {
                Nothing = IO.pure(Next.Done<int, int>(count)),
                Number = (Zero: true, Then: fun(static (double typed) => IO.lift(Multifoil.FoilCount.Check((int)typed, FoilCountPrompt)).Map(static valid => Next.Loop<int, int>(valid)))),
            },
            Options = Seq<OptionSpec<Next<int, int>>>(new OptionSpec<Next<int, int>>.Integer(LOC.CON("FoilCount"), count, Multifoil.FoilCount, None, static chosen => IO.pure(Next.Loop<int, int>(chosen)))),
            Settings = new() { Base = Some(new BasePoint(span.Start, ShowDistance: true)) },
            Handlers = DynamicDraw((display, _) => draw(display, count)),
        };

    // --- [HANDLERS]
    private static PointHandlers DynamicDraw(Func<DisplayPipeline, Point3d, IO<Unit>> draw) =>
        new() { DynamicDraw = Some<Func<GetPointDrawEventArgs, IO<Unit>>>(e => draw(e.Display, e.CurrentPoint)) };
}

public static class ProfileMarks {
    public static IO<Unit> Profile(DisplayPipeline display, Span span, Fin<ArchProfile> built) =>
        built.Match(
            Succ: profile => profile.Switch(
                (Display: display, profile.Span),
                arcs: static (state, arcs) => Marks.DrawWorld(state.Display, arcs.Parts.Bind(static arc => ArcMarks(arc)) + SpanMarks(state.Span)),
                parabolic: static (state, parabolic) => Curved(state.Display, parabolic.ToCurve(), state.Span),
                elliptical: static (state, elliptical) => Curved(state.Display, elliptical.ToCurve(), state.Span)),
            Fail: _ => Ends(display, span.Start, span.End));

    public static IO<Unit> Ends(DisplayPipeline display, Point3d start, Point3d end) =>
        Marks.DrawWorld(display, EndPoints(start, end));

    private static IO<Unit> Curved(DisplayPipeline display, IO<Curve> curve, Span span) =>
        Disposal.Using(curve, drawn => Marks.DrawWorld(display, Seq<WorldMark>(new WorldMark.CurveMark(drawn, AppearanceSettings.FeedbackColor)) + SpanMarks(span)))
            .IfFail(_ => Ends(display, span.Start, span.End));

    private static Seq<WorldMark> ArcMarks(Arc arc) =>
        Seq<WorldMark>(new WorldMark.ArcMark(arc, AppearanceSettings.FeedbackColor), new WorldMark.Points(Seq(arc.Center, arc.MidPoint), SmartTrackSettings.ActivePointColor, None))
        + EndPoints(arc.StartPoint, arc.EndPoint);

    private static Seq<WorldMark> SpanMarks(Span span) =>
        Seq<WorldMark>(new WorldMark.LineMark(span.CenterLine, AppearanceSettings.CrosshairColor), new WorldMark.Points(Seq(span.Midpoint), SmartTrackSettings.ActivePointColor, None))
        + EndPoints(span.Start, span.End);

    private static Seq<WorldMark> EndPoints(Point3d start, Point3d end) =>
        Seq<WorldMark>(new WorldMark.LineMark(new Line(start, end), AppearanceSettings.CrosshairColor), new WorldMark.Points(Seq(start, end), AppearanceSettings.CrosshairColor, None));
}
