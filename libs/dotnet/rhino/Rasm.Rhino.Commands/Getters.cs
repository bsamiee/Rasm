using System.Drawing;
using Rasm.Rhino.Document;
using Rhino.ApplicationSettings;
using Rhino.Collections;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.Input;
using Rhino.Input.Custom;

namespace Rasm.Rhino.Commands;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record PointPick(Point3d Value, Option<(ViewportIdentity Viewport, Plane ConstructionPlane)> View);

public sealed record WorldPick(PointPick Pick, bool GotDefault, OsnapModes Osnap);

public sealed record WindowPick(System.Drawing.Point Value, Option<(ViewportIdentity Viewport, Plane ConstructionPlane)> View);

public sealed record TransformPick(WorldPick Pick, Option<Transform> Xform);

public sealed record Entered<TValue>(TValue Value, bool GotDefault);

public sealed record Accepts<T> {
    public Option<IO<T>> Nothing { get; init; }

    public Option<IO<T>> Undo { get; init; }

    public Option<Func<string, IO<T>>> String { get; init; }

    public Option<Func<Color, IO<T>>> Color { get; init; }

    public Option<(bool Zero, Func<double, IO<T>> Then)> Number { get; init; }

    public Option<Func<PointPick, IO<T>>> Point { get; init; }

    public Option<(int Milliseconds, IO<T> Then)> Timeout { get; init; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PointHandler {
    internal abstract IO<IDisposable> Attach(GetPoint getter);

    public sealed record MouseMove(Func<GetPointMouseEventArgs, IO<Unit>> Handle) : PointHandler {
        internal override IO<IDisposable> Attach(GetPoint getter) =>
            Events.Attach(h => getter.MouseMove += h, h => getter.MouseMove -= h, Answers.Handler(Handle, ErrorOps.Report));
    }

    public sealed record MouseDown(Func<GetPointMouseEventArgs, IO<Unit>> Handle) : PointHandler {
        internal override IO<IDisposable> Attach(GetPoint getter) =>
            Events.Attach(h => getter.MouseDown += h, h => getter.MouseDown -= h, Answers.Handler(Handle, ErrorOps.Report));
    }

    public sealed record DynamicDraw(Func<GetPointDrawEventArgs, IO<Unit>> Handle) : PointHandler {
        internal override IO<IDisposable> Attach(GetPoint getter) =>
            Events.Attach(h => getter.DynamicDraw += h, h => getter.DynamicDraw -= h, Answers.Handler(Handle, ErrorOps.Report));
    }

    /// <summary>Draws after the scene objects during the get, where a transform get raises the stage only under a full-frame redraw.</summary>
    public sealed record PostDraw(Func<DrawEventArgs, IO<Unit>> Handle) : PointHandler {
        internal override IO<IDisposable> Attach(GetPoint getter) =>
            IO.lift(() => getter.FullFrameRedrawDuringGet |= getter is GetTransform)
                .Bind(_ => Events.Attach(h => getter.PostDrawObjects += h, h => getter.PostDrawObjects -= h, Answers.Handler(Handle, ErrorOps.Report)));
    }
}

public record GetterRequest<TGetter, T>(string Prompt) where TGetter : GetBaseClass {
    public Accepts<T> Accept { get; init; } = new();

    public Seq<OptionSpec<T>> Options { get; init; }

    public Func<TGetter, IO<Unit>> Configure { get; init; } = static _ => IO.pure(unit);
}

public sealed record PointRequest<T>(string Prompt) : GetterRequest<GetPoint, T>(Prompt) {
    public Seq<PointHandler> Handlers { get; init; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SelectionEnd {
    public sealed record OnEnter() : SelectionEnd;

    public sealed record AtMinimum() : SelectionEnd;

    public sealed record AtCount : SelectionEnd {
        private AtCount(int maximum) => Maximum = maximum;

        public static SelectionEnd Single { get; } = new AtCount(1);

        public int Maximum { get; }

        public static Fin<SelectionEnd> Create(int maximum) =>
            Limits.AtLeast(1).Check(maximum, nameof(AtCount)).Map<SelectionEnd>(static valid => new AtCount(valid));
    }
}

public sealed record ObjectRequest<T>(string Prompt) : GetterRequest<GetObject, T>(Prompt) {
    public int Minimum { get; private init; } = 1;

    public SelectionEnd End { get; init; } = SelectionEnd.AtCount.Single;

    public Fin<ObjectRequest<T>> WithMinimum(int minimum) =>
        Limits.AtLeast(0).Check(minimum, nameof(Minimum)).Map(valid => this with { Minimum = valid });
}

public sealed record TransformObjectState(Seq<Guid> Objects, Seq<Guid> Grips, Seq<Guid> GripOwners, Option<BoundingBox> Extent);

// --- [SERVICES] ------------------------------------------------------------------------
internal sealed class CallbackGetTransform(Func<RhinoViewport, Point3d, Transform> calculate) : GetTransform {
    public override Transform CalculateTransform(RhinoViewport viewport, Point3d point) => calculate(viewport, point);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Getters {
    // --- [GETTERS]
    public static IO<T> GetPoint<T>(PointRequest<T> request, bool onMouseUp, Func<WorldPick, IO<T>> picked) =>
        Pointed(static () => new GetPoint(), request, getter => IO.lift(() => getter.Get(onMouseUp, get2DPoint: false)), getter => Seq((GetResult.Point, World(getter).Bind(picked))));

    public static IO<T> GetWindowPoint<T>(PointRequest<T> request, bool onMouseUp, Func<WindowPick, IO<T>> picked) =>
        Pointed(
            static () => new GetPoint(),
            request,
            getter => IO.lift(() => getter.Get(onMouseUp, get2DPoint: true)),
            getter => Seq((GetResult.Point2d, IO.lift(() => new WindowPick(getter.Point2d(), View(getter))).Bind(picked))));

    public static IO<T> GetTransform<T>(PointRequest<T> request, TransformObjectList objects, Func<RhinoViewport, Point3d, Transform> calculate, Func<TransformPick, IO<T>> picked) =>
        Pointed(
            () => new CallbackGetTransform(calculate),
            request,
            getter => IO.lift(() => getter.AddTransformObjects(objects)).Bind(_ => IO.lift(getter.GetXform)),
            getter => Seq((GetResult.Point, World(getter).Map(pick => new TransformPick(pick, Answers.Found(getter.HaveTransform, getter.Transform))).Bind(picked))));

    public static IO<T> GetObjects<T>(ObjectRequest<T> request, Func<Seq<ObjRef>, IO<T>> selected) =>
        Run(
            static () => new GetObject(),
            request,
            getter => Selected(getter, request),
            getter => Seq((GetResult.Object, DisposalOps.Using(IO.lift(() => toSeq(getter.Objects())), selected))));

    public static IO<T> GetNumber<T>(GetterRequest<GetNumber, T> request, Func<Entered<double>, IO<T>> entered) =>
        Run(
            static () => new GetNumber(),
            request,
            static getter => IO.lift(() => getter.Get()),
            getter => Seq((GetResult.Number, IO.lift(() => new Entered<double>(getter.Number(), getter.GotDefault())).Bind(entered))));

    public static IO<T> GetInteger<T>(GetterRequest<GetInteger, T> request, Func<Entered<int>, IO<T>> entered) =>
        Run(
            static () => new GetInteger(),
            request,
            static getter => IO.lift(() => getter.Get()),
            getter => Seq((GetResult.Number, IO.lift(() => new Entered<int>(getter.Number(), getter.GotDefault())).Bind(entered))));

    public static IO<T> GetString<T>(GetterRequest<GetString, T> request, bool literal, Func<Entered<string>, IO<T>> entered) =>
        Run(
            static () => new GetString(),
            request,
            getter => IO.lift(() => literal ? getter.GetLiteralString() : getter.Get()),
            getter => Seq((GetResult.String, IO.lift(() => new Entered<string>(getter.StringResult(), getter.GotDefault())).Bind(entered))));

    public static IO<T> GetOption<T>(GetterRequest<GetOption, T> request) =>
        Run(static () => new GetOption(), request, static getter => IO.lift(() => getter.Get()), static _ => Seq<(GetResult Result, IO<T> Then)>());

    public static IO<TValue> Loop<TState, TValue>(TState initial, Func<TState, IO<Next<TState, TValue>>> step) =>
        Monad.recur(
                initial,
                state => step(state)
                    .IfFail(error => error.IsType<LimitViolation>()
                        ? IO.lift(() => ErrorOps.Report(error)).Map(_ => Next.Loop<TState, TValue>(state))
                        : IO.fail<Next<TState, TValue>>(error)))
            .As();

    private static IO<T> Pointed<TGetter, T>(
        Func<TGetter> create,
        PointRequest<T> request,
        Func<TGetter, IO<GetResult>> get,
        Func<TGetter, Seq<(GetResult Result, IO<T> Then)>> own) where TGetter : GetPoint =>
        Run(create, request, getter => DisposalOps.Using(Events.AttachAll(request.Handlers.Map(handler => handler.Attach(getter)), ErrorOps.Report), _ => get(getter)), own);

    private static IO<T> Run<TBase, TGetter, T>(
        Func<TGetter> create,
        GetterRequest<TBase, T> request,
        Func<TGetter, IO<GetResult>> get,
        Func<TGetter, Seq<(GetResult Result, IO<T> Then)>> own)
        where TBase : GetBaseClass
        where TGetter : TBase =>
        DisposalOps.Using(create, getter =>
            from prompted in IO.lift(() => getter.SetCommandPrompt(request.Prompt))
            let accepted = Accepted(getter, request.Accept)
            let routes = own(getter) + accepted.Map(static row => (row.Result, row.Then))
            from enabled in accepted.TraverseM(static row => row.Enable).As()
            from configured in request.Configure(getter)
            from answer in CommandOptions.Using(
                getter,
                request.Options,
                chosen => get(getter).Bind(result => toMapTry(routes.Add((GetResult.Option, chosen))).Find(result).IfNone(IO.fail<T>(Ended(result)))))
            select answer);

    // --- [TRANSFORM_OBJECTS]
    public static IO<TransformObjectList> TransformObjects(Seq<ObjRef> references, bool feedback) =>
        Listed(list => Invalid.Unless(!references.IsEmpty, nameof(references)).Map(_ => references.Iter(list.Add)), feedback);

    public static IO<TransformObjectList> TransformObjects(ObjectRequest<TransformObjectList> request, bool allowGrips, bool feedback) =>
        Run(
            static () => new GetObject(),
            request,
            getter => Selected(getter, request),
            getter => Seq((GetResult.Object, Listed(list => Missing.Unless(list.AddObjects(getter, allowGrips) > 0, nameof(TransformObjectList.AddObjects)), feedback))));

    public static IO<Unit> UpdateFeedback(TransformObjectList list, Transform xform) =>
        IO.lift(() => Refused.Unless(list.UpdateDisplayFeedbackTransform(xform), nameof(TransformObjectList.UpdateDisplayFeedbackTransform)));

    public static IO<TransformObjectState> Snapshot(TransformObjectList list, bool grips) =>
        IO.lift(() => new TransformObjectState(
            toSeq(list.ObjectArray()).Map(static subject => subject.Id).Strict(),
            toSeq(list.GripArray()).Map(static grip => grip.Id).Strict(),
            toSeq(list.GripOwnerArray()).Map(static owner => owner.Id).Strict(),
            Some(list.GetBoundingBox(regularObjects: true, grips)).Filter(static box => box.IsValid)));

    private static IO<TransformObjectList> Listed(Func<TransformObjectList, Fin<Unit>> fill, bool feedback) =>
        from list in IO.lift(() => new TransformObjectList { DisplayFeedbackEnabled = feedback })
        from filled in DisposalOps.OnFailure(IO.lift(() => fill(list)), IO.lift(list.Dispose))
        select list;

    private static IO<GetResult> Selected<T>(GetObject getter, ObjectRequest<T> request) =>
        IO.lift(() => getter.GetMultiple(request.Minimum, request.End.Switch(onEnter: static _ => 0, atMinimum: static _ => -1, atCount: static count => count.Maximum)));

    // --- [RESULTS]
    private static Seq<(GetResult Result, IO<Unit> Enable, IO<T> Then)> Accepted<T>(GetBaseClass getter, Accepts<T> accept) =>
        Seq(
            accept.Nothing.Map(then => (GetResult.Nothing, IO.lift(() => getter.AcceptNothing(enable: true)), then)),
            accept.Undo.Map(then => (GetResult.Undo, IO.lift(() => getter.AcceptUndo(enable: true)), then)),
            accept.String.Map(then => (GetResult.String, IO.lift(() => getter.AcceptString(enable: true)), IO.lift(getter.StringResult).Bind(then))),
            accept.Color.Map(then => (GetResult.Color, IO.lift(() => getter.AcceptColor(enable: true)), IO.lift(getter.Color).Bind(then))),
            accept.Number.Map(number => (GetResult.Number, IO.lift(() => getter.AcceptNumber(enable: true, number.Zero)), IO.lift(getter.Number).Bind(number.Then))),
            accept.Point.Map(then => (GetResult.Point, IO.lift(() => getter.AcceptPoint(enable: true)), Pick(getter).Bind(then))),
            accept.Timeout.Map(wait => (GetResult.Timeout, IO.lift(() => getter.SetWaitDuration(wait.Milliseconds)), wait.Then)))
            .Somes();

    private static Error Ended(GetResult result) =>
        result switch {
            GetResult.Cancel => new Canceled(),
            GetResult.ExitRhino => new ExitRequested(),
            _ => new UnexpectedGetResult(result),
        };

    private static IO<PointPick> Pick(GetBaseClass getter) =>
        IO.lift(() => new PointPick(getter.Point(), View(getter)));

    private static IO<WorldPick> World(GetPoint getter) =>
        Pick(getter).Map(pick => new WorldPick(pick, getter.GotDefault(), getter.OsnapEventType));

    private static Option<(ViewportIdentity Viewport, Plane ConstructionPlane)> View(GetBaseClass getter) =>
        Optional(getter.View()).Map(static view => (Viewports.Identity(view, view.ActiveViewport), view.ActiveViewport.ConstructionPlane()));
}
