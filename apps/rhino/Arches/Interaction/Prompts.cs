using System.Diagnostics;
using Arches.Profiles;
using Rasm.Rhino.Commands;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.Display;
using Rhino.Input.Custom;
using Rhino.UI;

namespace Arches.Interaction;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Prompts {
    // --- [TYPE]
    public static IO<ArchProfile> PickType(string prompt, IterableNE<ArchType> types, Context context) =>
        Getters.GetPoint(new PointRequest<ArchType>(prompt) { Options = toSeq(types.Map<OptionSpec<ArchType>>(static type => new OptionSpec<ArchType>.Simple(type.Name, None, Hidden: false) { Key = type })) }, onMouseUp: false, get2D: false)
            .Bind(result => result.Switch(
                (Types: types, Context: context),
                point: static (state, location) => state.Types.Head.Run(state.Context with { Start = Some(location.Value) }),
                point2d: static (_, _) => throw new UnreachableException(),
                option: static (state, chosen) => chosen.Chosen.Run(state.Context),
                number: static (_, _) => throw new UnreachableException(),
                @string: static (_, _) => throw new UnreachableException(),
                color: static (_, _) => throw new UnreachableException(),
                nothing: static (_, _) => throw new UnreachableException(),
                undo: static (_, _) => throw new UnreachableException(),
                timeout: static (_, _) => throw new UnreachableException()));

    // --- [SPAN]
    public static IO<Span> PickSpan(Context context, bool flip, Func<DisplayPipeline, Span, IO<Unit>> draw) =>
        SpanLoop(context, flip ? Some(value: false) : None, pointed: None, (display, span, _) => draw(display, span)).Map(static picked => picked.Span);

    public static IO<(Span Span, bool Pointed)> PickFoilSpan(Context context, Func<DisplayPipeline, Span, bool, IO<Unit>> draw) =>
        SpanLoop(context, flip: Some(value: false), pointed: Some(value: false), draw);

    private static IO<(Span Span, bool Pointed)> SpanLoop(Context context, Option<bool> flip, Option<bool> pointed, Func<DisplayPipeline, Span, bool, IO<Unit>> draw) =>
        Getters.Loop(
            new SpanState(context.Start, BothSides: false, flip, pointed),
            state => Getters.GetPoint(SpanRequest(context, state, draw), onMouseUp: false, get2D: false),
            (state, result) => SpanStep(context, state, result));

    private static Seq<Toggle> Toggles(Context context, SpanState state) =>
        state.Start.IsSome
            ? context.Start.Map(_ => Sides(state)).ToSeq()
              + state.Flip.Map(static _ => new Toggle(LOC.CON("Flip"), static toggled => toggled with { Flip = toggled.Flip.Map(static flip => !flip) })).ToSeq()
              + state.Pointed.Map(static pointed => new Toggle(pointed ? LOC.CON("Rounded") : LOC.CON("Pointed"), static toggled => toggled with { Pointed = toggled.Pointed.Map(static shape => !shape) })).ToSeq()
            : Seq(Sides(state));

    private static Toggle Sides(SpanState state) =>
        new(state.BothSides ? LOC.CON("EndPoints") : LOC.CON("BothSides"), static toggled => toggled with { BothSides = !toggled.BothSides });

    private static PointRequest<Toggle> SpanRequest(Context context, SpanState state, Func<DisplayPipeline, Span, bool, IO<Unit>> draw) =>
        state.Start.Match(
            Some: start => new PointRequest<Toggle>(LOC.STR("Second end point")) {
                Settings = new() { Base = Some(new BasePoint(start, ShowDistance: true)) },
                Handlers = DynamicDraw((display, end) => context.Normal.Bind(normal => SpanFrame(display, MirroredStart(state, start, end), end, Flipped(state, normal), state.IsPointed, draw))),
            },
            None: static () => new PointRequest<Toggle>(LOC.STR("First end point")))
        with {
            Options = Toggles(context, state).Map<OptionSpec<Toggle>>(static toggle => new OptionSpec<Toggle>.Simple(toggle.Name, None, Hidden: false) { Key = toggle }),
        };

    private static IO<Unit> SpanFrame(DisplayPipeline display, Point3d start, Point3d end, Vector3d normal, bool pointed, Func<DisplayPipeline, Span, bool, IO<Unit>> draw) =>
        Span.From(start, end, normal).Match(Succ: span => draw(display, span, pointed), Fail: _ => ProfileMarks.Ends(display, start, end));

    private static IO<Next<SpanState, (Span Span, bool Pointed)>> SpanStep(Context context, SpanState state, PointResult<Toggle> result) =>
        result.Switch(
            (Context: context, State: state),
            point: static (picked, end) => picked.State.Start.Match(
                Some: start =>
                    from normal in picked.Context.Normal
                    from span in IO.lift(Span.From(MirroredStart(picked.State, start, end.Value), end.Value, Flipped(picked.State, normal)))
                    select Next.Done<SpanState, (Span Span, bool Pointed)>((span, picked.State.IsPointed)),
                None: () => IO.pure(Next.Loop<SpanState, (Span Span, bool Pointed)>(picked.State with { Start = Some(end.Value) }))),
            point2d: static (_, _) => throw new UnreachableException(),
            option: static (picked, chosen) => IO.pure(Next.Loop<SpanState, (Span Span, bool Pointed)>(chosen.Chosen.Next(picked.State))),
            number: static (_, _) => throw new UnreachableException(),
            @string: static (_, _) => throw new UnreachableException(),
            color: static (_, _) => throw new UnreachableException(),
            nothing: static (_, _) => throw new UnreachableException(),
            undo: static (_, _) => throw new UnreachableException(),
            timeout: static (_, _) => throw new UnreachableException());

    private static Point3d MirroredStart(SpanState state, Point3d start, Point3d end) =>
        state.BothSides ? start + (start - end) : start;

    private static Vector3d Flipped(SpanState state, Vector3d normal) =>
        state.Flip.Case is true ? -normal : normal;

    private sealed record SpanState(Option<Point3d> Start, bool BothSides, Option<bool> Flip, Option<bool> Pointed) {
        public bool IsPointed => Pointed.Case is true;
    }

    private sealed record Toggle(LocalizeStringPair Name, Func<SpanState, SpanState> Next);

    // --- [APEX]
    private static readonly string RisePrompt = LOC.STR("Rise");

    public static IO<Point3d> PickApex(RhinoDoc doc, Span span, Limits<double> limits, Func<DisplayPipeline, Point3d, IO<Unit>> draw) =>
        from cursor in IO.lift(static () => Atom(Option<Point3d>.None))
        from apex in Getters.Loop(
            new ApexState(ByRise: false, Apex: None),
            state => Getters.GetPoint(ApexRequest(span, limits, state, cursor, draw), onMouseUp: false, get2D: false),
            (state, result) => ApexStep(doc, span, limits, cursor, state, result))
        select apex;

    private static PointRequest<Unit> ApexRequest(Span span, Limits<double> limits, ApexState state, Atom<Option<Point3d>> cursor, Func<DisplayPipeline, Point3d, IO<Unit>> draw) =>
        (state.ByRise
            ? new PointRequest<Unit>(RisePrompt) { Accept = AcceptNumber }
            : new PointRequest<Unit>(LOC.STR("Apex point")) {
                Options = Seq<OptionSpec<Unit>>(new OptionSpec<Unit>.Simple(LOC.CON("Rise"), None, Hidden: false) { Key = unit }),
            })
        with {
            Handlers = DynamicDraw((display, point) =>
                from apex in IO.pure(Candidate(span, limits, state, point))
                from held in cursor.SwapIO(_ => Some(apex))
                from drawn in draw(display, apex)
                select drawn),
        };

    private static IO<Next<ApexState, Point3d>> ApexStep(RhinoDoc doc, Span span, Limits<double> limits, Atom<Option<Point3d>> cursor, ApexState state, PointResult<Unit> result) =>
        result.Switch(
            (Doc: doc, Span: span, Limits: limits, Cursor: cursor, State: state),
            point: static (picked, apex) => IO.pure(Next.Done<ApexState, Point3d>(Candidate(picked.Span, picked.Limits, picked.State, apex.Value))),
            point2d: static (_, _) => throw new UnreachableException(),
            option: static (picked, _) => picked.State.ByRise
                ? throw new UnreachableException()
                : picked.Cursor.ValueIO.Map(static frozen => Next.Loop<ApexState, Point3d>(new ApexState(ByRise: true, Apex: frozen))),
            number: static (picked, rise) => picked.State.ByRise
                ? TypedRise(picked.Doc, picked.Span, picked.Limits, rise.Value).Map(apex => Next.Loop<ApexState, Point3d>(picked.State with { Apex = Some(apex) }))
                : throw new UnreachableException(),
            @string: static (_, _) => throw new UnreachableException(),
            color: static (_, _) => throw new UnreachableException(),
            nothing: static (picked, _) => picked.State.ByRise
                ? IO.lift(() => picked.State.Apex.Map(static apex => Next.Done<ApexState, Point3d>(apex)).ToFin(new NothingEntered()))
                : throw new UnreachableException(),
            undo: static (_, _) => throw new UnreachableException(),
            timeout: static (_, _) => throw new UnreachableException());

    private static Point3d Candidate(Span span, Limits<double> limits, ApexState state, Point3d point) =>
        state.Apex.IfNone(() => span.Raised(point, limits));

    private static IO<Point3d> TypedRise(RhinoDoc doc, Span span, Limits<double> limits, double rise) =>
        IO.lift(() => limits.Check(Math.Abs(rise), RisePrompt, bound => new ModelDistance(doc, bound)))
            .Map(_ => span.Raised(span.Midpoint + (span.Perpendicular * rise), limits));

    private sealed record ApexState(bool ByRise, Option<Point3d> Apex);

    // --- [FOILS]
    private static readonly string FoilCountPrompt = LOC.STR("Foil count");

    public static IO<int> PickFoilCount(Span span, int count, Func<DisplayPipeline, int, IO<Unit>> draw) =>
        Getters.Loop(count, current => Getters.GetPoint(FoilCountRequest(span, current, draw), onMouseUp: false, get2D: false), FoilCountStep);

    private static PointRequest<Unit> FoilCountRequest(Span span, int count, Func<DisplayPipeline, int, IO<Unit>> draw) =>
        new(FoilCountPrompt) {
            Accept = AcceptNumber,
            Options = Seq<OptionSpec<Unit>>(new OptionSpec<Unit>.Integer(LOC.CON("FoilCount"), count, Multifoil.FoilCount, None) { Key = unit }),
            Settings = new() { Base = Some(new BasePoint(span.Start, ShowDistance: true)) },
            Handlers = DynamicDraw((display, _) => draw(display, count)),
        };

    private static IO<Next<int, int>> FoilCountStep(int count, PointResult<Unit> result) =>
        result.Switch(
            count,
            point: static (current, _) => IO.pure(Next.Done<int, int>(current)),
            point2d: static (_, _) => throw new UnreachableException(),
            option: static (_, chosen) => IO.lift(() => Next.Loop<int, int>(ChosenCount(chosen.Selection))),
            number: static (_, typed) => IO.lift(() => Multifoil.FoilCount.Check((int)typed.Value, FoilCountPrompt)).Map(static valid => Next.Loop<int, int>(valid)),
            @string: static (_, _) => throw new UnreachableException(),
            color: static (_, _) => throw new UnreachableException(),
            nothing: static (current, _) => IO.pure(Next.Done<int, int>(current)),
            undo: static (_, _) => throw new UnreachableException(),
            timeout: static (_, _) => throw new UnreachableException());

    private static int ChosenCount(OptionSelection selection) =>
        selection.CurrentValue.Match(
            Some: static current => current.Switch(
                toggle: static _ => throw new UnreachableException(),
                number: static _ => throw new UnreachableException(),
                integer: static integer => integer.Value,
                @string: static _ => throw new UnreachableException(),
                color: static _ => throw new UnreachableException(),
                list: static _ => throw new UnreachableException()),
            None: static () => throw new UnreachableException());

    // --- [REQUESTS]
    private static AcceptPolicy AcceptNumber => new() { Nothing = true, NumberAcceptZero = Some(value: true) };

    private static PointHandlers DynamicDraw(Func<DisplayPipeline, Point3d, IO<Unit>> draw) =>
        new() { DynamicDraw = Some<Func<GetPointDrawEventArgs, IO<Unit>>>(e => draw(e.Display, e.CurrentPoint)) };
}
