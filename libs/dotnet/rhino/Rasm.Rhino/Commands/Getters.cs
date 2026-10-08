using System.Drawing;
using Rasm.Rhino.Document;
using Rasm.Rhino.Document.Notation;
using Rasm.Rhino.Events;
using Rhino;
using Rhino.ApplicationSettings;
using Rhino.Collections;
using Rhino.Commands;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.Input;
using Rhino.Input.Custom;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Commands;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record PointPick(Point3d Point, LengthUnit Unit, Option<(ViewportTarget Viewport, Plane ConstructionPlane)> View);

public sealed record WorldPick(PointPick Pick, bool GotDefault, OsnapModes Osnap, Option<Point3d> Base, Option<ObjRef> On) {
    public Option<Length> FromBase => Base.Map(start => Quantities.From(Pick.Point.DistanceTo(start), Pick.Unit));
}

public sealed record WindowPick(System.Drawing.Point Point, Option<(ViewportTarget Viewport, Plane ConstructionPlane)> View);

public sealed record TransformPick(WorldPick Pick, Option<Transform> Xform);

public sealed record Entered<TValue>(TValue Value, bool GotDefault);

public sealed record TransformObjectState(Seq<Guid> Objects, Seq<Guid> Grips, Seq<Guid> GripOwners, Option<BoundingBox> Extent);

[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct PickCount : System.Numerics.IMinMaxValue<PickCount> {
    public static PickCount MinValue { get; } = new(1);
    public static PickCount MaxValue { get; } = new(int.MaxValue);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref int value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

[Union]
public abstract partial record SelectionEnd {
    public sealed record OnEnter : SelectionEnd;
    public sealed record AtMinimum : SelectionEnd;
    public sealed record AtCount(PickCount Maximum) : SelectionEnd;
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record NumberEntry<T> {
    private NumberEntry(bool zero) => Zero = zero;
    public bool Zero { get; }
    public sealed record Plain(bool Zero, Func<double, IO<T>> Then) : NumberEntry<T>(Zero);
    public sealed record Distance(bool Zero, Func<Length, IO<T>> Then) : NumberEntry<T>(Zero);
}

public sealed record Accepts<T> {
    public Option<(Option<string> Shown, IO<T> Then)> Nothing { get; init; }
    public Option<IO<T>> Undo { get; init; }
    public Option<Func<string, IO<T>>> String { get; init; }
    public Option<(Option<Color> Default, Func<Color, IO<T>> Then)> Color { get; init; }
    public Option<NumberEntry<T>> Number { get; init; }
    public Option<Func<PointPick, IO<T>>> Point { get; init; }
    public Option<(Duration Wait, IO<T> Then)> Timeout { get; init; }
    public Option<Func<object, IO<T>>> Message { get; init; }
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record PointHandler {
    public sealed record MouseMove(Func<GetPointMouseEventArgs, IO<Unit>> Handle) : PointHandler;
    public sealed record MouseDown(Func<GetPointMouseEventArgs, IO<Unit>> Handle) : PointHandler;
    public sealed record DynamicDraw(Func<GetPointDrawEventArgs, IO<Unit>> Handle) : PointHandler;
    public sealed record PostDraw(Func<DrawEventArgs, IO<Unit>> Handle) : PointHandler;

    internal IO<IDisposable> Attach(GetPoint getter, CallbackSite site) =>
        Switch((Getter: getter, Site: site),
            mouseMove: static (at, row) => Subscriptions.Attach(h => at.Getter.MouseMove += h, h => at.Getter.MouseMove -= h, Callbacks.Handler(Posting(row.Handle), at.Site)),
            mouseDown: static (at, row) => Subscriptions.Attach(h => at.Getter.MouseDown += h, h => at.Getter.MouseDown -= h, Callbacks.Handler(Posting(row.Handle), at.Site)),
            dynamicDraw: static (at, row) => Subscriptions.Attach(h => at.Getter.DynamicDraw += h, h => at.Getter.DynamicDraw -= h, Callbacks.Handler(Posting(row.Handle), at.Site)),
            postDraw: static (at, row) => Subscriptions.Attach(h => at.Getter.PostDrawObjects += h, h => at.Getter.PostDrawObjects -= h, Callbacks.Handler(Posting(row.Handle), at.Site)));

    private static Func<TArgs, IO<Unit>> Posting<TArgs>(Func<TArgs, IO<Unit>> handle) =>
        args => IO.pure(args).Bind(handle).IfFail(static error => IO.lift(() => GetBaseClass.PostCustomMessage(error)));
}

public record GetterRequest<TGetter, T>(string Prompt, CallbackSite Site) where TGetter : GetBaseClass {
    public Accepts<T> Accept { get; init; } = new();
    public Seq<OptionSpec<T>> Options { get; init; }
    public Func<TGetter, IO<Unit>> Configure { get; init; } = static _ => IO.pure(unit);
}

public sealed record PointRequest<T>(string Prompt, CallbackSite Site) : GetterRequest<GetPoint, T>(Prompt, Site) {
    public Option<Point3d> Default { get; init; }
    public bool OnMouseUp { get; init; }
    public Seq<PointHandler> Handlers { get; init; }
    internal bool Released => OnMouseUp || Handlers.Exists(static handler => handler is PointHandler.MouseDown);
    internal bool FullFrame => Handlers.Exists(static handler => handler is PointHandler.PostDraw);
}

public sealed record EntryRequest<TGetter, TValue, T>(string Prompt, CallbackSite Site) : GetterRequest<TGetter, T>(Prompt, Site) where TGetter : GetBaseClass {
    public Option<TValue> Default { get; init; }
}

public sealed record ObjectRequest<T>(string Prompt, CallbackSite Site) : GetterRequest<GetObject, T>(Prompt, Site) {
    public Option<PickCount> Minimum { get; init; } = PickCount.MinValue;
    public SelectionEnd End { get; init; } = PickCount.MinValue;
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record StageStep<TStage, TValue> {
    internal static IO<Next<(TStage Current, Seq<TStage> Earlier), TValue>> Transition(
        (TStage Current, Seq<TStage> Earlier) trail,
        Func<TStage, Accepts<StageStep<TStage, TValue>>, IO<StageStep<TStage, TValue>>> prompt) =>
        from step in prompt(trail.Current, new Accepts<StageStep<TStage, TValue>> {
            Undo = trail.Earlier.Head.Map(earlier => IO.pure<StageStep<TStage, TValue>>(new Back(earlier, trail.Earlier.Tail))),
        })
        from next in step.Switch(
            trail,
            advance: static (at, advance) => IO.pure(Next.Loop<(TStage Current, Seq<TStage> Earlier), TValue>((advance.Stage, at.Current.Cons(at.Earlier)))),
            back: static (_, back) => IO.pure(Next.Loop<(TStage Current, Seq<TStage> Earlier), TValue>((back.Stage, back.Earlier))),
            retry: static (at, retry) => IO.lift(() => RhinoApp.WriteLine(ErrorOps.Localize(retry.Rejection)))
                .Map(_ => Next.Loop<(TStage Current, Seq<TStage> Earlier), TValue>(at)),
            done: static (_, done) => IO.pure(Next.Done<(TStage Current, Seq<TStage> Earlier), TValue>(done.Value)))
        select next;

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
        calculate(viewport, new PointPick(point, Getters.Space(Optional(View()), document), Some(Getters.Located(viewport))));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
public static partial class Getters {
    // --- [ROUTES]
    private static IO<T> Run<TBase, TGetter, T>(
        Func<TGetter> create,
        GetterRequest<TBase, T> request,
        RhinoDoc doc,
        Func<TGetter, IO<GetResult>> get,
        Func<TGetter, Seq<(GetResult Result, IO<T> Then)>> own)
        where TBase : GetBaseClass
        where TGetter : TBase =>
        (from getter in use(create)
         from prompted in IO.lift(() => getter.SetCommandPrompt(RowText.Localize(request.Prompt, table: Some<object>(request.Site.Sink)).Local))
         let accepted = Accepted(getter, request, doc)
         let routes = own(getter) + accepted.Map(static route => (route.Result, route.Then))
         from enabled in accepted.TraverseM(static route => route.Enable).As()
         from configured in request.Configure(getter)
         from answer in CommandOptions.Using(
             getter, request.Site.Sink, request.Options,
             chosen => Routed(routes.Add((GetResult.Option, chosen)), get(getter), request.Site.Member))
         select answer).Bracket();

    private static IO<T> Routed<T>(Seq<(GetResult Result, IO<T> Then)> routes, IO<GetResult> get, string member) =>
        from result in get
        from answer in routes.Find(route => route.Result == result).Map(static route => route.Then).IfNone(() => IO.fail<T>(result switch {
            GetResult.Cancel or GetResult.Timeout => Errors.Cancelled,
            GetResult.Nothing => new Ended(member, Result.Nothing),
            GetResult.ExitRhino => new Ended(member, Result.ExitRhino),
            _ => new UnexpectedGetResult(result),
        }))
        select answer;

    private static Seq<(GetResult Result, IO<Unit> Enable, IO<T> Then)> Accepted<TBase, T>(GetBaseClass getter, GetterRequest<TBase, T> request, RhinoDoc doc)
        where TBase : GetBaseClass =>
        Seq(
            request.Accept.Nothing.Map(nothing => (
                GetResult.Nothing,
                IO.lift(() => {
                    getter.AcceptNothing(enable: true);
                    _ = nothing.Shown.Iter(shown => getter.SetCommandPromptDefault(RowText.Localize(shown, table: Some<object>(request.Site.Sink)).Local));
                }),
                nothing.Then)),
            request.Accept.Undo.Map(then => (GetResult.Undo, IO.lift(() => getter.AcceptUndo(enable: true)), then)),
            request.Accept.String.Map(then => (GetResult.String, IO.lift(() => getter.AcceptString(enable: true)), IO.lift(getter.StringResult).Bind(then))),
            request.Accept.Color.Map(color => (
                GetResult.Color,
                IO.lift(() => {
                    getter.AcceptColor(enable: true);
                    _ = color.Default.Iter(getter.SetDefaultColor);
                }),
                IO.lift(getter.Color).Bind(color.Then))),
            request.Accept.Number.Map(number => (GetResult.Number, IO.lift(() => getter.AcceptNumber(enable: true, number.Zero)), Numbered(getter, number, doc))),
            request.Accept.Point.Map(then => (GetResult.Point, IO.lift(() => getter.AcceptPoint(enable: true)), Pick(getter, doc).Bind(then))),
            request.Accept.Timeout.Map(timeout => (
                GetResult.Timeout,
                from milliseconds in IO.lift(Conversions.Whole(timeout.Wait, Duration.FromMilliseconds(1)))
                from set in IO.lift(() => getter.SetWaitDuration(milliseconds))
                select set,
                timeout.Then)),
            request.Accept.Message.Map(message => (GetResult.CustomMessage, IO.lift(() => getter.AcceptCustomMessage(enable: true)), Posted(getter, Some(message)))))
            .Somes();

    private static IO<T> Numbered<T>(GetBaseClass getter, NumberEntry<T> number, RhinoDoc doc) =>
        number.Switch(
            (Getter: getter, Doc: doc),
            plain: static (at, plain) => IO.lift(at.Getter.Number).Bind(plain.Then),
            distance: static (at, distance) => IO.lift(() => Quantities.From(at.Getter.Number(), Space(Optional(at.Getter.View()), at.Doc))).Bind(distance.Then));

    private static IO<T> Posted<T>(GetBaseClass getter, Option<Func<object, IO<T>>> message) =>
        from value in IO.lift(() => Missing.Unless(getter.CustomMessage(), nameof(GetBaseClass.CustomMessage)))
        from answer in value switch {
            Error error => IO.fail<T>(error),
            _ => message.Map(then => then(value)).IfNone(static () => IO.fail<T>(new Missing(nameof(Accepts<>.Message)))),
        }
        select answer;

    // --- [POINTS]
    public static IO<T> Point<T>(RhinoDoc doc, PointRequest<T> request, Func<WorldPick, IO<T>> picked) =>
        Pointed(static () => new GetPoint(), doc, request, getter => getter.Get(request.Released, get2DPoint: false), getter => (GetResult.Point, World(getter, doc, picked)));

    public static IO<T> WindowPoint<T>(RhinoDoc doc, PointRequest<T> request, Func<WindowPick, IO<T>> picked) =>
        Pointed(
            static () => new GetPoint(),
            doc,
            request,
            getter => getter.Get(request.Released, get2DPoint: true),
            getter => (GetResult.Point2d, IO.lift(() => new WindowPick(getter.Point2d(), Optional(getter.View()).Map(static view => Located(view.ActiveViewport)))).Bind(picked)));

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
            getter => (GetResult.Point, World(getter, doc, pick => picked(new TransformPick(pick, Callbacks.Found(getter.HaveTransform, getter.Transform))))));

    public static LengthUnit Space(Option<RhinoView> view, RhinoDoc doc) =>
        view.Exists(static found => found is RhinoPageView { ActiveDetail: null }) ? doc.PageUnits : doc.ModelUnits;

    private static IO<T> Pointed<TGetter, T>(
        Func<TGetter> create, RhinoDoc doc, PointRequest<T> request, Func<TGetter, GetResult> get, Func<TGetter, (GetResult Result, IO<T> Then)> answered)
        where TGetter : GetPoint =>
        Run(
            create,
            request,
            doc,
            getter => DisposalOps.AcquireAll(request.Handlers.Map(handler => handler.Attach(getter, request.Site)), DisposalOps.Release)
                .Bracket(
                    Use: _ => IO.lift(() => {
                        getter.AcceptCustomMessage(!request.Handlers.IsEmpty || request.Accept.Message.IsSome);
                        getter.FullFrameRedrawDuringGet |= getter is GetTransform && request.FullFrame;
                        if (request.Default.Case is Point3d point)
                            getter.SetDefaultPoint(point);
                        return get(getter);
                    }),
                    Fin: DisposalOps.Release),
            getter => request.Handlers.IsEmpty
                ? Seq(answered(getter))
                : Seq(answered(getter), (GetResult.CustomMessage, Posted(getter, request.Accept.Message))));

    private static IO<T> World<T>(GetPoint getter, RhinoDoc doc, Func<WorldPick, IO<T>> picked) =>
        IO.lift(() => Optional(getter.PointOnObject())).Bracket(
            Use: reference => Pick(getter, doc).Map(pick => new WorldPick(pick, getter.GotDefault(), getter.OsnapEventType, Callbacks.Found(getter.TryGetBasePoint(out Point3d start), start), reference)).Bind(picked),
            Fin: static reference => DisposalOps.Release(reference.ToSeq()));

    private static IO<PointPick> Pick(GetBaseClass getter, RhinoDoc doc) =>
        IO.lift(() => Optional(getter.View())).Map(view => new PointPick(getter.Point(), Space(view, doc), view.Map(static found => Located(found.ActiveViewport))));

    internal static (ViewportTarget Viewport, Plane ConstructionPlane) Located(RhinoViewport viewport) =>
        (new ViewportTarget.Id(viewport.Id), viewport.ConstructionPlane());

    // --- [OBJECTS]
    public static IO<T> Objects<T>(RhinoDoc doc, ObjectRequest<T> request, Func<Seq<ObjRef>, IO<T>> selected) =>
        Run(
            static () => new GetObject(),
            request,
            doc,
            getter => Selected(getter, request),
            getter => Seq((GetResult.Object, IO.lift(() => toSeq(getter.Objects())).Bracket(Use: selected, Fin: DisposalOps.Release))));

    private static IO<GetResult> Selected<T>(GetObject getter, ObjectRequest<T> request) =>
        IO.lift(() => {
            getter.AcceptEnterWhenDone(request.End.Map(onEnter: true, atMinimum: false, atCount: false));
            return getter.GetMultiple(
                request.Minimum.Map(static minimum => (int)minimum).IfNone(0),
                request.End.Switch(onEnter: static _ => 0, atMinimum: static _ => -1, atCount: static count => (int)count.Maximum));
        });

    // --- [ENTRY]
    public static IO<T> Number<TValue, TRaw, TError, T>(RhinoDoc doc, EntryRequest<GetNumber, TValue, T> request, Func<Entered<TValue>, IO<T>> entered)
        where TValue : IObjectFactory<TValue, TRaw, TError>, IConvertible<TRaw>, System.Numerics.IMinMaxValue<TValue>
        where TRaw : struct, System.Numerics.IFloatingPointIeee754<TRaw>
        where TError : Error, IValidationError<TError> =>
        Run(
            static () => new GetNumber(),
            request,
            doc,
            getter => IO.lift(() => {
                getter.SetLowerLimit(double.CreateChecked(TValue.MinValue.ToValue()), strictlyGreaterThan: false);
                getter.SetUpperLimit(double.CreateChecked(TValue.MaxValue.ToValue()), strictlyLessThan: false);
                _ = request.Default.Iter(value => getter.SetDefaultNumber(double.CreateChecked(value.ToValue())));
                return getter.Get();
            }),
            getter => Seq((GetResult.Number,
                IO.lift(() => Conversions.Validated<TValue, TRaw, TError>(TRaw.CreateChecked(getter.Number())).Map(value => new Entered<TValue>(value, getter.GotDefault()))).Bind(entered))));

    public static IO<T> Integer<TValue, TRaw, TError, T>(RhinoDoc doc, EntryRequest<GetInteger, TValue, T> request, Func<Entered<TValue>, IO<T>> entered)
        where TValue : IObjectFactory<TValue, TRaw, TError>, IConvertible<TRaw>, System.Numerics.IMinMaxValue<TValue>
        where TRaw : struct, System.Numerics.IBinaryInteger<TRaw>
        where TError : Error, IValidationError<TError> =>
        from values in IO.lift(() => Callbacks.Thrown<OverflowException, (int Lower, int Upper, Option<int> Default)>(
            () => (int.CreateChecked(TValue.MinValue.ToValue()), int.CreateChecked(TValue.MaxValue.ToValue()), request.Default.Map(static value => int.CreateChecked(value.ToValue()))),
            nameof(int.CreateChecked)))
        from answer in Run(
            static () => new GetInteger(),
            request,
            doc,
            getter => IO.lift(() => {
                getter.SetLowerLimit(values.Lower, strictlyGreaterThan: false);
                getter.SetUpperLimit(values.Upper, strictlyLessThan: false);
                _ = values.Default.Iter(getter.SetDefaultInteger);
                return getter.Get();
            }),
            getter => Seq((GetResult.Number,
                IO.lift(() => Conversions.Validated<TValue, TRaw, TError>(TRaw.CreateChecked(getter.Number())).Map(value => new Entered<TValue>(value, getter.GotDefault()))).Bind(entered))))
        select answer;

    public static IO<T> Distance<TValue, TError, T>(RhinoDoc doc, EntryRequest<GetNumber, TValue, T> request, Func<Entered<TValue>, IO<T>> entered)
        where TValue : IObjectFactory<TValue, Length, TError>, IConvertible<Length>, System.Numerics.IMinMaxValue<TValue>
        where TError : Error, IValidationError<TError> =>
        from space in IO.lift(() => Space(Optional(doc.Views.ActiveView), doc))
        from answer in Run(
            static () => new GetNumber(),
            request,
            doc,
            getter => IO.lift(() => {
                getter.SetLowerLimit(Quantities.As(TValue.MinValue.ToValue(), space), strictlyGreaterThan: false);
                getter.SetUpperLimit(Quantities.As(TValue.MaxValue.ToValue(), space), strictlyLessThan: false);
                _ = request.Default.Iter(value => getter.SetDefaultNumber(Quantities.As(value.ToValue(), space)));
                return getter.Get();
            }),
            getter => Seq((GetResult.Number,
                IO.lift(() => Conversions.Validated<TValue, Length, TError>(Quantities.From(getter.Number(), space)).Map(value => new Entered<TValue>(value, getter.GotDefault()))).Bind(entered))))
        select answer;

    public static IO<T> Text<T>(RhinoDoc doc, EntryRequest<GetString, string, T> request, bool literal, Func<Entered<string>, IO<T>> entered) =>
        Run(
            static () => new GetString(),
            request,
            doc,
            getter => IO.lift(() => {
                _ = request.Default.Iter(getter.SetDefaultString);
                return literal ? getter.GetLiteralString() : getter.Get();
            }),
            getter => Seq((GetResult.String, IO.lift(() => new Entered<string>(getter.StringResult(), getter.GotDefault())).Bind(entered))));

    public static IO<T> Choice<T>(RhinoDoc doc, GetterRequest<GetOption, T> request) =>
        Run(static () => new GetOption(), request, doc, static getter => IO.lift(getter.Get), static _ => Seq<(GetResult Result, IO<T> Then)>());

    // --- [DRAG]
    public static IO<TransformObjectList> DragList(Seq<ObjRef> references, bool feedback) =>
        Listed(list => references.Iter(list.Add), feedback);

    public static IO<TransformObjectList> DragList(RhinoDoc doc, ObjectRequest<TransformObjectList> request, bool allowGrips, bool feedback) =>
        Run(
            static () => new GetObject(),
            request,
            doc,
            getter => Selected(getter, request),
            getter => Seq((GetResult.Object, Listed(list => Missing.Unless(list.AddObjects(getter, allowGrips) > 0, nameof(TransformObjectList.AddObjects)), feedback))));

    public static IO<Unit> Dragged(TransformObjectList list, Transform xform) =>
        IO.lift(() => Refused.Unless(list.UpdateDisplayFeedbackTransform(xform), nameof(TransformObjectList.UpdateDisplayFeedbackTransform)));

    public static IO<TransformObjectState> Snapshot(TransformObjectList list) =>
        IO.lift(() => new TransformObjectState(
            toSeq(list.ObjectArray()).Map(static subject => subject.Id).Strict(),
            toSeq(list.GripArray()).Map(static grip => grip.Id).Strict(),
            toSeq(list.GripOwnerArray()).Map(static owner => owner.Id).Strict(),
            Conversions.Present(list.GetBoundingBox(regularObjects: true, grips: true))));

    private static IO<TransformObjectList> Listed(Func<TransformObjectList, Fin<Unit>> fill, bool feedback) =>
        from list in IO.lift(() => CreateList(feedback))
        from filled in DisposalOps.OnFailure(IO.lift(() => fill(list)), IO.lift(list.Dispose))
        select list;

    // --- [STAGES]
    public static IO<TValue> Stages<TStage, TValue>(TStage entry, Func<TStage, Accepts<StageStep<TStage, TValue>>, IO<StageStep<TStage, TValue>>> prompt) =>
        Monad.recur<IO, (TStage Current, Seq<TStage> Earlier), TValue>(
                (entry, Seq<TStage>()), trail => StageStep<TStage, TValue>.Transition(trail, prompt))
            .As();

    // --- [AWAIT]
    public static IO<T> Await<T>(RhinoDoc doc, string prompt, Option<string> progress, Func<IProgress<double>, IO<T>> work) =>
        (from getter in use(() => CreateWaiter((progress.IsSome, progress)))
         from shown in IO.lift(() => getter.SetCommandPrompt(prompt))
         from running in IO.lift(() => Task.Run(() => {
             using EnvIO environment = EnvIO.New(token: getter.Token);
             return Try.lift(() => work(getter.Progress).Run(environment)).Run();
         }, getter.Token))
         from waited in IO.lift(() => Conversions.FromResult(getter.Wait(running, doc), nameof(GetCancel.Wait)))
         from value in IO.liftAsync(() => running).Bind(IO.lift)
         select value).Bracket();

    // --- [MAPPING]
    [MapPropertyFromSource(nameof(TransformObjectList.DisplayFeedbackEnabled))]
    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    private static partial TransformObjectList CreateList(bool feedback);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    private static partial GetCancel CreateWaiter((bool ProgressReporting, Option<string> ProgressMessage) settings);
}
