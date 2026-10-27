using System.Drawing;
using Rasm.Rhino.Document.Units;
using Rasm.Rhino.Events;
using Rasm.Rhino.Viewport;
using Rhino;
using Rhino.ApplicationSettings;
using Rhino.Collections;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.Input;
using Rhino.Input.Custom;
using Rhino.PlugIns;

namespace Rasm.Rhino.Commands;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ViewFrame(ViewportTarget Viewport, Plane ConstructionPlane);

public sealed record PointPick(Point3d Point, LengthUnit Unit, Option<ViewFrame> View);

public sealed record WorldPick(PointPick Pick, bool GotDefault, OsnapModes Osnap, Option<Point3d> Base, Option<ObjRef> On) {
    public Option<Length> FromBase => Base.Map(start => Quantities.From(Pick.Point.DistanceTo(start), Pick.Unit));
}

public sealed record WindowPick(System.Drawing.Point Point, Option<ViewFrame> View);

public sealed record TransformPick(WorldPick Pick, Option<Transform> Xform);

public sealed record Entered<TValue>(TValue Value, bool GotDefault);

public sealed record TransformObjectState(Seq<Guid> Objects, Seq<Guid> Grips, Seq<Guid> GripOwners, Option<BoundingBox> Extent);

[Union]
public abstract partial record SelectionEnd {
    public sealed record OnEnter : SelectionEnd;
    public sealed record AtMinimum : SelectionEnd;
    public sealed record AtCount(int Maximum) : SelectionEnd;
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record NumberEntry<T> {
    private NumberEntry(bool zero) => Zero = zero;
    public bool Zero { get; }
    public sealed record Plain(bool Zero, Func<double, IO<T>> Then) : NumberEntry<T>(Zero);
    public sealed record Distance(bool Zero, Func<Length, IO<T>> Then) : NumberEntry<T>(Zero);
}

public sealed record NothingEntry<T>(Option<string> Shown, IO<T> Then);

public sealed record ColorEntry<T>(Option<Color> Default, Func<Color, IO<T>> Then);

public sealed record TimeoutEntry<T>(Duration Wait, IO<T> Then);

public sealed record Accepts<T> {
    public Option<NothingEntry<T>> Nothing { get; init; }
    public Option<IO<T>> Undo { get; init; }
    public Option<Func<string, IO<T>>> String { get; init; }
    public Option<ColorEntry<T>> Color { get; init; }
    public Option<NumberEntry<T>> Number { get; init; }
    public Option<Func<PointPick, IO<T>>> Point { get; init; }
    public Option<TimeoutEntry<T>> Timeout { get; init; }
    public Option<Func<object, IO<T>>> Message { get; init; }
}

public sealed record Route<T>(GetResult Result, IO<T> Then) {
    public IO<Unit> Enable { get; init; } = IO.pure(unit);
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record PointHandler {
    public sealed record MouseMove(Func<GetPointMouseEventArgs, IO<Unit>> Handle) : PointHandler;
    public sealed record MouseDown(Func<GetPointMouseEventArgs, IO<Unit>> Handle) : PointHandler;
    public sealed record DynamicDraw(Func<GetPointDrawEventArgs, IO<Unit>> Handle) : PointHandler;
    public sealed record PostDraw(Func<DrawEventArgs, IO<Unit>> Handle) : PointHandler;

    internal IO<Unit> Attach(GetPoint getter, PlugIn owner) =>
        Switch((Getter: getter, Owner: owner),
            mouseMove: static (at, row) => Subscriptions.During<GetPointMouseEventArgs>(h => at.Getter.MouseMove += h, h => at.Getter.MouseMove -= h).Inline(Posting(row.Handle), at.Owner),
            mouseDown: static (at, row) => Subscriptions.During<GetPointMouseEventArgs>(h => at.Getter.MouseDown += h, h => at.Getter.MouseDown -= h).Inline(Posting(row.Handle), at.Owner),
            dynamicDraw: static (at, row) => Subscriptions.During<GetPointDrawEventArgs>(h => at.Getter.DynamicDraw += h, h => at.Getter.DynamicDraw -= h).Inline(Posting(row.Handle), at.Owner),
            postDraw: static (at, row) => Subscriptions.During<DrawEventArgs>(h => at.Getter.PostDrawObjects += h, h => at.Getter.PostDrawObjects -= h).Inline(Posting(row.Handle), at.Owner));

    private static Func<TArgs, IO<Unit>> Posting<TArgs>(Func<TArgs, IO<Unit>> handle) =>
        args => IO.pure(args).Bind(handle).IfFail(static error => IO.lift(() => GetBaseClass.PostCustomMessage(error)));
}

public record GetterRequest<TGetter, T>(string Prompt, PlugIn PlugIn) where TGetter : GetBaseClass {
    public Accepts<T> Accept { get; init; } = new();
    public Seq<OptionSpec<T>> Options { get; init; }
    public Seq<Func<TGetter, IO<Unit>>> Configure { get; init; }
}

public sealed record PointRequest<T>(string Prompt, PlugIn PlugIn) : GetterRequest<GetPoint, T>(Prompt, PlugIn) {
    public Option<Point3d> Default { get; init; }
    public bool OnMouseUp { get; init; }
    public Seq<PointHandler> Handlers { get; init; }
    internal bool Released => OnMouseUp || Handlers.Exists(static handler => handler is PointHandler.MouseDown);
    internal bool FullFrame => Handlers.Exists(static handler => handler is PointHandler.PostDraw);
}

public sealed record EntryRequest<TGetter, TValue, T>(string Prompt, PlugIn PlugIn) : GetterRequest<TGetter, T>(Prompt, PlugIn) where TGetter : GetBaseClass {
    public Option<TValue> Default { get; init; }
}

public sealed record ObjectRequest<T>(string Prompt, PlugIn PlugIn) : GetterRequest<GetObject, T>(Prompt, PlugIn) {
    public Option<int> Minimum { get; init; } = 1;
    public SelectionEnd End { get; init; } = new SelectionEnd.AtCount(1);
}

public sealed record StageTrail<TStage>(TStage Current, Seq<TStage> Earlier);

public sealed record StageKit<TStage, TValue>(
    Func<TStage, IO<StageStep<TStage, TValue>>> Advance,
    Func<Error, IO<StageStep<TStage, TValue>>> Retry,
    Func<TValue, IO<StageStep<TStage, TValue>>> Done,
    Accepts<StageStep<TStage, TValue>> Accept);

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record StageStep<TStage, TValue> {
    internal static IO<Next<StageTrail<TStage>, TValue>> Transition(StageTrail<TStage> trail, Func<TStage, StageKit<TStage, TValue>, IO<StageStep<TStage, TValue>>> prompt) =>
        from step in prompt(trail.Current, Kit(trail)).Catch(Getters.Retried, static error => IO.pure<StageStep<TStage, TValue>>(new Retry(error)))
        from next in step.Switch(
            trail,
            advance: static (at, advance) => IO.pure(Next.Loop<StageTrail<TStage>, TValue>(new StageTrail<TStage>(advance.Stage, at.Current.Cons(at.Earlier)))),
            back: static (_, back) => IO.pure(Next.Loop<StageTrail<TStage>, TValue>(new StageTrail<TStage>(back.Stage, back.Earlier))),
            retry: static (at, retry) => IO.lift(() => RhinoApp.WriteLine(ErrorOps.Messages(retry.Rejection))).Map(_ => Next.Loop<StageTrail<TStage>, TValue>(at)),
            done: static (_, done) => IO.pure(Next.Done<StageTrail<TStage>, TValue>(done.Value)))
        select next;

    private static StageKit<TStage, TValue> Kit(StageTrail<TStage> trail) =>
        new(
            static stage => IO.pure<StageStep<TStage, TValue>>(new Advance(stage)),
            static error => IO.pure<StageStep<TStage, TValue>>(new Retry(error)),
            static value => IO.pure<StageStep<TStage, TValue>>(new Done(value)),
            new Accepts<StageStep<TStage, TValue>> { Undo = trail.Earlier.Head.Map(earlier => IO.pure<StageStep<TStage, TValue>>(new Back(earlier, trail.Earlier.Tail))) });

    public sealed record Advance(TStage Stage) : StageStep<TStage, TValue>;

    public sealed record Back : StageStep<TStage, TValue> {
        internal Back(TStage stage, Seq<TStage> earlier) => (Stage, Earlier) = (stage, earlier);

        public TStage Stage { get; }

        public Seq<TStage> Earlier { get; }
    }

    public sealed record Retry(Error Rejection) : StageStep<TStage, TValue>;

    public sealed record Done(TValue Value) : StageStep<TStage, TValue>;
}

// --- [SERVICES] ------------------------------------------------------------------------
internal sealed class CallbackGetTransform(RhinoDoc document, Func<RhinoViewport, PointPick, Transform> calculate) : GetTransform {
    public override Transform CalculateTransform(RhinoViewport viewport, Point3d point) =>
        calculate(viewport, new PointPick(point, Viewports.Units(document, Some(viewport)), Some(Getters.Located(viewport))));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Getters {
    // --- [ROUTES]
    public static bool Retried(Error error) =>
        error.IsExpected && !ErrorOps.Cancellation(error) && error.AsIterable().ForAll(static leaf => leaf is not RhinoFault { Reason.Ending.IsSome: true });

    private static IO<T> Run<TBase, TGetter, T>(Func<TGetter> create, GetterRequest<TBase, T> request, RhinoDoc doc, Func<TGetter, IO<GetResult>> get, Func<TGetter, Seq<Route<T>>> own)
        where TBase : GetBaseClass
        where TGetter : TBase =>
        (from getter in use(create)
         from prompted in IO.lift(() => getter.SetCommandPrompt(RowText.Localize(request.Prompt, request.PlugIn).Local))
         let accepted = Accepted(getter, request, doc)
         from enabled in accepted.TraverseM(static route => route.Enable).As()
         from configured in request.Configure.TraverseM(configure => configure(getter)).As()
         from result in CommandOptions.Using(getter, request.PlugIn, request.Options, chosen => Routed(own(getter) + accepted + Seq(new Route<T>(GetResult.Option, chosen)), get(getter), request.Prompt))
         select result).Bracket();

    private static IO<T> Routed<T>(Seq<Route<T>> routes, IO<GetResult> get, string prompt) =>
        from result in get
        from answer in routes.Find(route => route.Result == result).Map(static route => route.Then).IfNone(() => IO.fail<T>(result switch {
            GetResult.Cancel or GetResult.Timeout => Errors.Cancelled,
            GetResult.Nothing => new RhinoFault(prompt, RhinoReason.Nothing),
            GetResult.ExitRhino => new RhinoFault(prompt, RhinoReason.Exited),
            _ => new RhinoFault(prompt, RhinoReason.Declined, Inner: Error.New(result.ToString())),
        }))
        select answer;

    private static Seq<Route<T>> Accepted<TBase, T>(GetBaseClass getter, GetterRequest<TBase, T> request, RhinoDoc doc) where TBase : GetBaseClass =>
        Seq(
            request.Accept.Nothing.Map(nothing => new Route<T>(GetResult.Nothing, nothing.Then) {
                Enable = IO.lift(() => {
                    getter.AcceptNothing(enable: true);
                    _ = nothing.Shown.Iter(shown => getter.SetCommandPromptDefault(RowText.Localize(shown, request.PlugIn).Local));
                }),
            }),
            request.Accept.Undo.Map(then => new Route<T>(GetResult.Undo, then) { Enable = IO.lift(() => getter.AcceptUndo(enable: true)) }),
            request.Accept.String.Map(then => new Route<T>(GetResult.String, IO.lift(getter.StringResult).Bind(then)) { Enable = IO.lift(() => getter.AcceptString(enable: true)) }),
            request.Accept.Color.Map(color => new Route<T>(GetResult.Color, IO.lift(getter.Color).Bind(color.Then)) {
                Enable = IO.lift(() => {
                    getter.AcceptColor(enable: true);
                    _ = color.Default.Iter(getter.SetDefaultColor);
                }),
            }),
            request.Accept.Number.Map(number => new Route<T>(GetResult.Number, Numbered(getter, number, doc)) { Enable = IO.lift(() => getter.AcceptNumber(enable: true, number.Zero)) }),
            request.Accept.Point.Map(then => new Route<T>(GetResult.Point, Pick(getter, doc).Bind(then)) { Enable = IO.lift(() => getter.AcceptPoint(enable: true)) }),
            request.Accept.Timeout.Map(timeout => new Route<T>(GetResult.Timeout, timeout.Then) {
                Enable = IO.lift(Conversions.Whole(timeout.Wait, Duration.FromMilliseconds(1))).Bind(milliseconds => IO.lift(() => getter.SetWaitDuration(milliseconds))),
            }),
            request.Accept.Message.Map(message => new Route<T>(GetResult.CustomMessage, Posted(getter, Some(message))) { Enable = IO.lift(() => getter.AcceptCustomMessage(enable: true)) }))
            .Somes();

    private static IO<T> Numbered<T>(GetBaseClass getter, NumberEntry<T> number, RhinoDoc doc) =>
        number.Switch(
            (Getter: getter, Doc: doc),
            plain: static (at, plain) => IO.lift(at.Getter.Number).Bind(plain.Then),
            distance: static (at, distance) => IO.lift(() => Quantities.From(at.Getter.Number(), Viewports.Units(at.Doc, Optional(at.Getter.View()).Map(static view => view.ActiveViewport)))).Bind(distance.Then));

    private static IO<T> Posted<T>(GetBaseClass getter, Option<Func<object, IO<T>>> message) =>
        from value in IO.lift(() => Conversions.Found(getter.CustomMessage()))
        from answer in value switch {
            Error error => IO.fail<T>(error),
            _ => IO.lift(Conversions.Found(message)).Bind(then => then(value)),
        }
        select answer;

    // --- [POINTS]
    public static IO<T> Point<T>(RhinoDoc doc, PointRequest<T> request, Func<WorldPick, IO<T>> picked) =>
        Pointed(static () => new GetPoint(), doc, request, getter => getter.Get(request.Released), getter => new Route<T>(GetResult.Point, World(getter, doc, picked)));

    public static IO<T> WindowPoint<T>(RhinoDoc doc, PointRequest<T> request, Func<WindowPick, IO<T>> picked) =>
        Pointed(
            static () => new GetPoint(),
            doc,
            request,
            getter => getter.Get(request.Released, get2DPoint: true),
            getter => new Route<T>(GetResult.Point2d, IO.lift(() => new WindowPick(getter.Point2d(), Optional(getter.View()).Map(static view => Located(view.ActiveViewport)))).Bind(picked)));

    public static IO<T> Transformed<T>(
        RhinoDoc doc, PointRequest<T> request, TransformObjectList objects, Func<RhinoViewport, PointPick, Transform> calculate, Func<TransformPick, IO<T>> picked) =>
        Pointed(
            () => new CallbackGetTransform(doc, calculate),
            doc,
            request,
            getter => {
                getter.AddTransformObjects(objects);
                return getter.GetXform();
            },
            getter => new Route<T>(GetResult.Point, World(getter, doc, pick => picked(new TransformPick(pick, Conversions.Found(getter.HaveTransform, getter.Transform))))));

    private static IO<T> Pointed<TGetter, T>(Func<TGetter> create, RhinoDoc doc, PointRequest<T> request, Func<TGetter, GetResult> get, Func<TGetter, Route<T>> answered)
        where TGetter : GetPoint =>
        Run(
            create,
            request,
            doc,
            getter =>
                from handlers in request.Handlers.TraverseM(handler => handler.Attach(getter, request.PlugIn)).As()
                from result in IO.lift(() => {
                    getter.AcceptCustomMessage(!request.Handlers.IsEmpty || request.Accept.Message.IsSome);
                    getter.FullFrameRedrawDuringGet |= request.FullFrame;
                    if (request.Default.Case is Point3d point)
                        getter.SetDefaultPoint(point);
                    return get(getter);
                })
                select result,
            getter => request.Handlers.IsEmpty
                ? Seq(answered(getter))
                : Seq(answered(getter), new Route<T>(GetResult.CustomMessage, Posted(getter, request.Accept.Message))));

    private static IO<T> World<T>(GetPoint getter, RhinoDoc doc, Func<WorldPick, IO<T>> picked) =>
        from reference in Products.Held(getter.PointOnObject())
        from pick in Pick(getter, doc)
        from value in picked(new WorldPick(pick, getter.GotDefault(), getter.OsnapEventType, Conversions.Found(getter.TryGetBasePoint(out Point3d start), start), reference))
        select value;

    private static IO<PointPick> Pick(GetBaseClass getter, RhinoDoc doc) =>
        IO.lift(() => Optional(getter.View()).Map(static view => view.ActiveViewport)).Map(viewport => new PointPick(getter.Point(), Viewports.Units(doc, viewport), viewport.Map(Located)));

    internal static ViewFrame Located(RhinoViewport viewport) => new(new ViewportTarget.Id(viewport.Id), viewport.ConstructionPlane());

    // --- [OBJECTS]
    public static IO<T> Objects<T>(RhinoDoc doc, ObjectRequest<T> request, Func<Seq<ObjRef>, IO<T>> selected) =>
        Run(
            static () => new GetObject(),
            request,
            doc,
            getter => Selected(getter, request),
            getter => Seq(new Route<T>(GetResult.Object, Products.Made(getter.Objects()).Bind(selected))));

    private static IO<GetResult> Selected<T>(GetObject getter, ObjectRequest<T> request) =>
        IO.lift(() => {
            getter.AcceptEnterWhenDone(request.End.Map(onEnter: true, atMinimum: false, atCount: false));
            return getter.GetMultiple(
                request.Minimum.IfNone(0),
                request.End.Switch(onEnter: static _ => 0, atMinimum: static _ => -1, atCount: static count => count.Maximum));
        });

    // --- [ENTRY]
    public static IO<T> Number<T>(RhinoDoc doc, EntryRequest<GetNumber, double, T> request, Func<Entered<double>, IO<T>> entered) =>
        Number(doc, request, identity, identity, entered);

    public static IO<T> Integer<T>(RhinoDoc doc, EntryRequest<GetInteger, int, T> request, Func<Entered<int>, IO<T>> entered) =>
        Run(
            static () => new GetInteger(),
            request,
            doc,
            getter => IO.lift(() => {
                _ = request.Default.Iter(getter.SetDefaultInteger);
                return getter.Get();
            }),
            getter => Seq(new Route<T>(GetResult.Number, IO.lift(() => new Entered<int>(getter.Number(), getter.GotDefault())).Bind(entered))));

    public static IO<T> Distance<T>(RhinoDoc doc, EntryRequest<GetNumber, Length, T> request, Func<Entered<Length>, IO<T>> entered) =>
        IO.lift(() => Viewports.Units(doc, Optional(doc.Views.ActiveView).Map(static view => view.ActiveViewport))).Bind(space =>
            Number(doc, request, value => Quantities.As(value, space), number => Quantities.From(number, space), entered));

    public static IO<T> Text<T>(RhinoDoc doc, EntryRequest<GetString, string, T> request, bool literal, Func<Entered<string>, IO<T>> entered) =>
        Run(
            static () => new GetString(),
            request,
            doc,
            getter => IO.lift(() => {
                _ = request.Default.Iter(getter.SetDefaultString);
                return literal ? getter.GetLiteralString() : getter.Get();
            }),
            getter => Seq(new Route<T>(GetResult.String, IO.lift(() => new Entered<string>(getter.StringResult(), getter.GotDefault())).Bind(entered))));

    public static IO<T> Choice<T>(RhinoDoc doc, GetterRequest<GetOption, T> request) =>
        Run(static () => new GetOption(), request, doc, static getter => IO.lift(getter.Get), static _ => Seq<Route<T>>());

    private static IO<T> Number<TValue, T>(
        RhinoDoc doc, EntryRequest<GetNumber, TValue, T> request, Func<TValue, double> shown, Func<double, TValue> read, Func<Entered<TValue>, IO<T>> entered) =>
        Run(
            static () => new GetNumber(),
            request,
            doc,
            getter => IO.lift(() => {
                _ = request.Default.Iter(value => getter.SetDefaultNumber(shown(value)));
                return getter.Get();
            }),
            getter => Seq(new Route<T>(GetResult.Number, IO.lift(() => new Entered<TValue>(read(getter.Number()), getter.GotDefault())).Bind(entered))));

    // --- [DRAG]
    public static IO<T> DragList<T>(Seq<ObjRef> references, bool feedback, Func<TransformObjectList, IO<T>> body) =>
        Listed(list => {
            _ = references.Iter(list.Add);
            return unit;
        }, feedback, body);

    public static IO<T> DragList<T>(RhinoDoc doc, ObjectRequest<T> request, bool allowGrips, bool feedback, Func<TransformObjectList, IO<T>> body) =>
        Run(
            static () => new GetObject(),
            request,
            doc,
            getter => Selected(getter, request),
            getter => Seq(new Route<T>(GetResult.Object, Listed(list => RhinoFault.Unless(list.AddObjects(getter, allowGrips) > 0), feedback, body))));

    public static IO<Unit> Dragged(TransformObjectList list, Transform xform) =>
        IO.lift(() => RhinoFault.Unless(list.UpdateDisplayFeedbackTransform(xform)));

    public static IO<TransformObjectState> Snapshot(TransformObjectList list) =>
        IO.lift(() => new TransformObjectState(
            toSeq(list.ObjectArray()).Map(static subject => subject.Id).Strict(),
            toSeq(list.GripArray()).Map(static grip => grip.Id).Strict(),
            toSeq(list.GripOwnerArray()).Map(static owner => owner.Id).Strict(),
            Conversions.Present(list.GetBoundingBox(regularObjects: true, grips: true))));

    private static IO<T> Listed<T>(Func<TransformObjectList, Fin<Unit>> fill, bool feedback, Func<TransformObjectList, IO<T>> body) =>
        (from list in use(() => new TransformObjectList { DisplayFeedbackEnabled = feedback })
         from filled in IO.lift(() => fill(list))
         from value in body(list)
         select value).Bracket();

    // --- [STAGES]
    public static IO<TValue> Stages<TStage, TValue>(TStage entry, Func<TStage, StageKit<TStage, TValue>, IO<StageStep<TStage, TValue>>> prompt) =>
        Monad.recur(new StageTrail<TStage>(entry, []), trail => StageStep<TStage, TValue>.Transition(trail, prompt)).As();

    // --- [AWAIT]
    public static IO<T> Await<T>(RhinoDoc doc, string prompt, Option<string> progress, Func<IProgress<double>, IO<T>> work) =>
        (from getter in use(() => new GetCancel { ProgressReporting = progress.IsSome, ProgressMessage = Conversions.Unset(progress) })
         from answer in IO.liftAsync(async () => {
             getter.SetCommandPrompt(prompt);
             Task<Fin<T>> running = Task.Run(() => Callbacks.Settled(work(getter.Progress), getter.Token), getter.Token);
             Fin<Unit> waited = Results.Ended(getter.Wait(running, doc));
             return waited is Fin<Unit>.Fail(var error) ? error : await running.ConfigureAwait(false);
         })
         from value in IO.lift(answer)
         select value).Bracket();
}
