using System.Drawing;
using Rasm.Rhino.Document;
using Rhino.ApplicationSettings;
using Rhino.Collections;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.Input;
using Rhino.Input.Custom;
using Rhino.UI;

namespace Rasm.Rhino.Commands;

// --- [TYPES] ---------------------------------------------------------------------------
public enum ElevatorMode { None = 0, FixedPlane = 1, ConstructionPlane = 2 }

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PointConstraint {
    public sealed record Segment(Point3d From, Point3d To) : PointConstraint;

    public sealed record OnArc(Arc Value) : PointConstraint;

    public sealed record OnCircle(Circle Value) : PointConstraint;

    public sealed record OnPlane(Plane Value, bool AllowElevator) : PointConstraint;

    public sealed record OnSphere(Sphere Value) : PointConstraint;

    public sealed record OnCylinder(Cylinder Value) : PointConstraint;

    public sealed record OnCurve(Curve Value, bool AllowOff) : PointConstraint;

    public sealed record OnSurface(Surface Value, bool AllowOff) : PointConstraint;

    public sealed record OnMesh(Mesh Value, bool AllowOff) : PointConstraint;

    public sealed record OnBrep(Brep Value, int WireDensity, int FaceIndex, bool AllowOff) : PointConstraint;

    public sealed record OnConstructionPlane(bool ThroughBasePoint) : PointConstraint;

    public sealed record OnTargetPlane() : PointConstraint;

    public sealed record OnCPlaneIntersection(Plane Value) : PointConstraint;
}

public sealed record PointPick(Point3d Value, Option<ViewportIdentity> Viewport);

public sealed record WorldPick(PointPick Pick, bool GotDefault, OsnapModes Osnap);

public sealed record WindowPick(System.Drawing.Point Value, Option<ViewportIdentity> Viewport);

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

public sealed record BasePoint(Point3d Origin, bool ShowDistance) {
    public bool DrawLine { get; init; }

    public Option<double> Distance { get; private init; }

    public Fin<BasePoint> WithDistance(double distance) =>
        Limits.AtLeast(0.0).Check(distance, nameof(GetPoint.ConstrainDistanceFromBasePoint)).Map(valid => this with { Distance = Some(valid) });
}

public sealed record PointSettings {
    public Option<BasePoint> Base { get; init; }

    public Option<CursorStyle> Cursor { get; init; }

    public Option<ElevatorMode> Elevator { get; init; }

    public Option<bool> OrthoSnap { get; init; }

    public Option<bool> ObjectSnap { get; init; }

    public Option<bool> FromOption { get; init; }

    public Option<bool> TabMode { get; init; }

    public Option<bool> ConstraintOptions { get; init; }

    public Option<bool> ObjectSnapCursors { get; init; }

    public Option<bool> SnapToCurves { get; init; }

    public Option<(bool Draw, bool Ends)> TangentBar { get; init; }

    public Option<(bool Draw, bool Ends)> PerpBar { get; init; }

    public Option<(bool Draw, bool Reverse)> Arrow { get; init; }

    public Option<bool> NoRedrawOnExit { get; init; }

    public Option<Color> DrawColor { get; init; }

    public Seq<Point3d> SnapPoints { get; init; }

    public Seq<Point3d> ConstructionPoints { get; init; }

    public bool FullFrameRedraw { get; init; }

    public Option<PointConstraint> Constraint { get; init; }
}

public sealed record PointMouseEvent(
    Option<Point3d> Point,
    Option<Line> PickLine,
    System.Drawing.Point WindowPoint,
    Guid ViewportId,
    bool LeftButtonDown,
    bool MiddleButtonDown,
    bool RightButtonDown,
    bool ShiftKeyDown,
    bool ControlKeyDown);

public sealed record PointHandlers {
    public Option<Func<PointMouseEvent, IO<Unit>>> MouseMove { get; init; }

    public Option<Func<GetPoint, PointMouseEvent, IO<Unit>>> MouseDown { get; init; }

    public Option<Func<GetPointDrawEventArgs, IO<Unit>>> DynamicDraw { get; init; }

    public Option<Func<DrawEventArgs, IO<Unit>>> PostDraw { get; init; }
}

public abstract record GetterRequest<T>(string Prompt) {
    public Option<string> PromptDefault { get; init; }

    public Accepts<T> Accept { get; init; } = new();

    public Seq<OptionSpec<T>> Options { get; init; }

    public bool EnterWhenDone { get; init; }

    public Option<bool> Transparent { get; init; }
}

public sealed record PointRequest<T>(string Prompt) : GetterRequest<T>(Prompt) {
    public Option<Point3d> Default { get; init; }

    public PointSettings Settings { get; init; } = new();

    public PointHandlers Handlers { get; init; } = new();
}

public sealed record ValueRequest<T, TValue>(string Prompt) : GetterRequest<T>(Prompt) {
    public Option<TValue> Default { get; init; }
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

public sealed record ObjectSettings {
    public int Minimum { get; private init; } = 1;

    public SelectionEnd End { get; init; } = SelectionEnd.AtCount.Single;

    public ObjectType Filter { get; init; } = ObjectType.AnyObject;

    public Option<GeometryAttributeFilter> Attributes { get; init; }

    public Option<GetObjectGeometryFilter> Custom { get; init; }

    public Option<(bool Enabled, bool IgnoreUnacceptable)> PreSelect { get; init; }

    public Option<bool> PostSelect { get; init; }

    public Option<bool> SelPrevious { get; init; }

    public Option<bool> Highlight { get; init; }

    public Option<bool> IgnoreGrips { get; init; }

    public Option<bool> EnablePressEnterWhenDonePrompt { get; init; }

    public Option<bool> SubObjectSelect { get; init; }

    public Option<bool> GroupSelect { get; init; }

    public Fin<ObjectSettings> WithMinimum(int minimum) =>
        Limits.AtLeast(0).Check(minimum, nameof(Minimum)).Map(valid => this with { Minimum = valid });
}

public sealed record ObjectRequest<T>(string Prompt) : GetterRequest<T>(Prompt) {
    public ObjectSettings Settings { get; init; } = new();
}

public sealed record TransformObjectState(Seq<Guid> Objects, Seq<Guid> Grips, Seq<Guid> GripOwners, Option<BoundingBox> Extent);

// --- [SERVICES] ------------------------------------------------------------------------
internal sealed class CallbackGetTransform(Func<RhinoViewport, Point3d, Transform> calculate) : GetTransform {
    public override Transform CalculateTransform(RhinoViewport viewport, Point3d point) => calculate(viewport, point);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Getters {
    // --- [GETTERS]
    public static IO<T> GetPoint<T>(Func<IO<Option<Point3d>>, PointRequest<T>> request, bool onMouseUp, Func<WorldPick, IO<T>> picked) =>
        Pointed(static () => new GetPoint(), request, getter => IO.lift(() => getter.Get(onMouseUp, get2DPoint: false)), getter => Seq((GetResult.Point, World(getter).Bind(picked))));

    public static IO<T> GetWindowPoint<T>(Func<IO<Option<Point3d>>, PointRequest<T>> request, bool onMouseUp, Func<WindowPick, IO<T>> picked) =>
        Pointed(
            static () => new GetPoint(),
            request,
            getter => IO.lift(() => getter.Get(onMouseUp, get2DPoint: true)),
            getter => Seq((GetResult.Point2d, IO.lift(() => new WindowPick(getter.Point2d(), Viewport(getter))).Bind(picked))));

    public static IO<T> GetTransform<T>(Func<IO<Option<Point3d>>, PointRequest<T>> request, TransformObjectList objects, Func<RhinoViewport, Point3d, Transform> calculate, Func<TransformPick, IO<T>> picked) =>
        Pointed(
            () => new CallbackGetTransform(calculate),
            request,
            getter => IO.lift(() => getter.AddTransformObjects(objects)).Bind(_ => IO.lift(getter.GetXform)),
            getter => Seq((GetResult.Point, World(getter).Map(pick => new TransformPick(pick, getter.HaveTransform ? Some(getter.Transform) : Option<Transform>.None)).Bind(picked))));

    public static IO<T> GetObjects<T>(ObjectRequest<T> request, Func<Seq<ObjRef>, IO<T>> selected) =>
        Run(
            static () => new GetObject(),
            request,
            static _ => IO.pure(unit),
            getter => IO.lift(() => getter.GetMultiple(request.Settings.Minimum, Configure(getter, request.Settings))),
            getter => Seq((GetResult.Object, IO.lift(() => toSeq(getter.Objects())).Bind(selected))));

    public static IO<T> GetNumber<T>(ValueRequest<T, double> request, Limits<double> limits, Func<Entered<double>, IO<T>> entered) =>
        Valued(
            static () => new GetNumber(),
            request,
            getter => {
                _ = request.Default.Iter(getter.SetDefaultNumber);
                _ = limits.Lower.Iter(lower => getter.SetLowerLimit(lower.Value, lower.Exclusive));
                _ = limits.Upper.Iter(upper => getter.SetUpperLimit(upper.Value, upper.Exclusive));
            },
            static getter => getter.Get(),
            GetResult.Number,
            static getter => getter.Number(),
            entered);

    public static IO<T> GetInteger<T>(ValueRequest<T, int> request, Limits<int> limits, Func<Entered<int>, IO<T>> entered) =>
        Valued(
            static () => new GetInteger(),
            request,
            getter => {
                _ = request.Default.Iter(getter.SetDefaultInteger);
                _ = limits.Lower.Iter(lower => getter.SetLowerLimit(lower.Value, lower.Exclusive));
                _ = limits.Upper.Iter(upper => getter.SetUpperLimit(upper.Value, upper.Exclusive));
            },
            static getter => getter.Get(),
            GetResult.Number,
            static getter => getter.Number(),
            entered);

    public static IO<T> GetString<T>(ValueRequest<T, string> request, bool literal, Func<Entered<string>, IO<T>> entered) =>
        Valued(
            static () => new GetString(),
            request,
            getter => _ = request.Default.Iter(getter.SetDefaultString),
            getter => literal ? getter.GetLiteralString() : getter.Get(),
            GetResult.String,
            static getter => getter.StringResult(),
            entered);

    private static IO<T> Pointed<TGetter, T>(
        Func<TGetter> create,
        Func<IO<Option<Point3d>>, PointRequest<T>> build,
        Func<TGetter, IO<GetResult>> get,
        Func<TGetter, Seq<(GetResult Result, IO<T> Then)>> own) where TGetter : GetPoint =>
        from cursor in IO.lift(static () => Atom(Option<Point3d>.None))
        let request = build(cursor.ValueIO)
        from answer in Run(
            create,
            request,
            getter =>
                from defaulted in IO.lift(() => request.Default.Iter(getter.SetDefaultPoint))
                from applied in IO.lift(() => Apply(getter, request.Settings, request.Handlers))
                select applied,
            getter => Disposal.Using(Subscribe(getter, request.Handlers, cursor), _ => get(getter)),
            own)
        select answer;

    private static IO<T> Valued<TGetter, T, TValue>(
        Func<TGetter> create,
        ValueRequest<T, TValue> request,
        Action<TGetter> configure,
        Func<TGetter, GetResult> get,
        GetResult own,
        Func<TGetter, TValue> read,
        Func<Entered<TValue>, IO<T>> entered) where TGetter : GetBaseClass =>
        Run(
            create,
            request,
            getter => IO.lift(() => configure(getter)),
            getter => IO.lift(() => get(getter)),
            getter => Seq((own, IO.lift(() => new Entered<TValue>(read(getter), getter.GotDefault())).Bind(entered))));

    private static IO<T> Run<TGetter, T>(
        Func<TGetter> create,
        GetterRequest<T> request,
        Func<TGetter, IO<Unit>> configure,
        Func<TGetter, IO<GetResult>> get,
        Func<TGetter, Seq<(GetResult Result, IO<T> Then)>> own) where TGetter : GetBaseClass =>
        Disposal.Using(create, getter =>
            from primed in IO.lift(() => Prime(getter, request))
            from configured in configure(getter)
            from answer in Disposal.Bracketed(
                CommandOptions.Register(getter, request.Options),
                static registered => Disposal.Release(registered.Holders),
                registered => get(getter).Bind(result => Read(getter, request.Accept, registered.Bound, result, own(getter))))
            select answer);

    // --- [LOOPS]
    public static IO<TValue> Loop<TState, TValue>(TState initial, Func<TState, IO<Next<TState, TValue>>> step) =>
        Monad.recur(
                initial,
                state => step(state)
                    .IfFail(error => error.IsType<LimitViolation>()
                        ? IO.lift(() => ErrorOps.Report(error)).Map(_ => Next.Loop<TState, TValue>(state))
                        : IO.fail<Next<TState, TValue>>(error)))
            .As();

    // --- [TRANSFORM_OBJECTS]
    public static IO<TransformObjectList> TransformObjects(Seq<ObjRef> references, bool feedback) =>
        from present in IO.lift(() => Answers.NonEmpty(references, nameof(TransformObjectList.Add)))
        from list in IO.lift(() => {
            TransformObjectList list = new() { DisplayFeedbackEnabled = feedback };
            _ = present.Iter(reference => list.Add(reference));
            return list;
        })
        select list;

    public static IO<TransformObjectList> TransformObjects(string prompt, ObjectType filter, Option<(bool Enabled, bool IgnoreUnacceptable)> preSelect, Option<bool> postSelect, bool allowGrips, bool feedback) =>
        Disposal.Using(static () => new GetObject(), getter =>
            from configured in IO.lift(() => {
                getter.SetCommandPrompt(prompt);
                getter.GeometryFilter = filter;
                _ = preSelect.Iter(pre => getter.EnablePreSelect(pre.Enabled, pre.IgnoreUnacceptable));
                _ = postSelect.Iter(getter.EnablePostSelect);
            })
            from list in IO.lift(static () => new TransformObjectList())
            from added in GeometryOps.OnFailure(
                IO.lift(() => list.AddObjects(getter, allowGrips) > 0
                    ? Fin.Succ(unit)
                    : Answers.FromResult(getter.CommandResult(), nameof(TransformObjectList.AddObjects)).Bind(static _ => Fin.Fail<Unit>(new NothingEntered()))),
                IO.lift(list.Dispose))
            from fed in IO.lift(() => list.DisplayFeedbackEnabled = feedback)
            select list);

    public static IO<Unit> UpdateFeedback(TransformObjectList list, Transform xform) =>
        IO.lift(() => Refused.Unless(list.UpdateDisplayFeedbackTransform(xform), nameof(TransformObjectList.UpdateDisplayFeedbackTransform)));

    public static IO<TransformObjectState> Snapshot(TransformObjectList list, bool grips) =>
        IO.lift(() => new TransformObjectState(
            toSeq(list.ObjectArray()).Map(static subject => subject.Id).Strict(),
            toSeq(list.GripArray()).Map(static grip => grip.Id).Strict(),
            toSeq(list.GripOwnerArray()).Map(static owner => owner.Id).Strict(),
            Some(list.GetBoundingBox(regularObjects: true, grips)).Filter(static box => box.IsValid)));

    // --- [PREPARATION]
    private static Unit Prime<T>(GetBaseClass getter, GetterRequest<T> request) {
        getter.SetCommandPrompt(request.Prompt);
        _ = request.PromptDefault.Iter(getter.SetCommandPromptDefault);
        getter.AcceptNothing(request.Accept.Nothing.IsSome);
        getter.AcceptUndo(request.Accept.Undo.IsSome);
        getter.AcceptEnterWhenDone(request.EnterWhenDone);
        getter.AcceptString(request.Accept.String.IsSome);
        getter.AcceptColor(request.Accept.Color.IsSome);
        _ = request.Accept.Number.Iter(number => getter.AcceptNumber(enable: true, number.Zero));
        _ = request.Accept.Point.Iter(_ => getter.AcceptPoint(enable: true));
        _ = request.Accept.Timeout.Iter(wait => getter.SetWaitDuration(wait.Milliseconds));
        _ = request.Transparent.Iter(getter.EnableTransparentCommands);
        return unit;
    }

    private static Fin<Unit> Apply(GetPoint getter, PointSettings settings, PointHandlers handlers) {
        _ = settings.Base.Iter(point => {
            getter.SetBasePoint(point.Origin, point.ShowDistance);
            if (point.DrawLine)
                getter.DrawLineFromPoint(point.Origin, point.ShowDistance);
            _ = point.Distance.Iter(getter.ConstrainDistanceFromBasePoint);
        });
        _ = settings.Cursor.Iter(getter.SetCursor);
        _ = settings.Elevator.Iter(mode => getter.PermitElevatorMode((int)mode));
        _ = settings.OrthoSnap.Iter(getter.PermitOrthoSnap);
        _ = settings.ObjectSnap.Iter(getter.PermitObjectSnap);
        _ = settings.FromOption.Iter(getter.PermitFromOption);
        _ = settings.TabMode.Iter(getter.PermitTabMode);
        _ = settings.ConstraintOptions.Iter(getter.PermitConstraintOptions);
        _ = settings.ObjectSnapCursors.Iter(getter.EnableObjectSnapCursors);
        _ = settings.SnapToCurves.Iter(getter.EnableSnapToCurves);
        _ = settings.TangentBar.Iter(bar => getter.EnableCurveSnapTangentBar(bar.Draw, bar.Ends));
        _ = settings.PerpBar.Iter(bar => getter.EnableCurveSnapPerpBar(bar.Draw, bar.Ends));
        _ = settings.Arrow.Iter(arrow => getter.EnableCurveSnapArrow(arrow.Draw, arrow.Reverse));
        _ = settings.NoRedrawOnExit.Iter(getter.EnableNoRedrawOnExit);
        _ = settings.DrawColor.Iter(color => getter.DynamicDrawColor = color);
        if (!settings.SnapPoints.IsEmpty)
            _ = getter.AddSnapPoints([.. settings.SnapPoints]);
        if (!settings.ConstructionPoints.IsEmpty)
            _ = getter.AddConstructionPoints([.. settings.ConstructionPoints]);
        getter.FullFrameRedrawDuringGet = settings.FullFrameRedraw || handlers.PostDraw.IsSome;
        return Refused.Unless(
            settings.Constraint.ForAll(constraint => constraint.Switch(
                getter,
                segment: static (target, segment) => target.Constrain(segment.From, segment.To),
                onArc: static (target, arc) => target.Constrain(arc.Value),
                onCircle: static (target, circle) => target.Constrain(circle.Value),
                onPlane: static (target, plane) => target.Constrain(plane.Value, plane.AllowElevator),
                onSphere: static (target, sphere) => target.Constrain(sphere.Value),
                onCylinder: static (target, cylinder) => target.Constrain(cylinder.Value),
                onCurve: static (target, curve) => target.Constrain(curve.Value, curve.AllowOff),
                onSurface: static (target, surface) => target.Constrain(surface.Value, surface.AllowOff),
                onMesh: static (target, mesh) => target.Constrain(mesh.Value, mesh.AllowOff),
                onBrep: static (target, brep) => target.Constrain(brep.Value, brep.WireDensity, brep.FaceIndex, brep.AllowOff),
                onConstructionPlane: static (target, plane) => target.ConstrainToConstructionPlane(plane.ThroughBasePoint),
                onTargetPlane: static (target, _) => {
                    target.ConstrainToTargetPlane();
                    return true;
                },
                onCPlaneIntersection: static (target, plane) => target.ConstrainToVirtualCPlaneIntersection(plane.Value))),
            nameof(getter.Constrain));
    }

    private static int Configure(GetObject getter, ObjectSettings settings) {
        getter.GeometryFilter = settings.Filter;
        _ = settings.Attributes.Iter(filter => getter.GeometryAttributeFilter = filter);
        _ = settings.Custom.Iter(getter.SetCustomGeometryFilter);
        _ = settings.PreSelect.Iter(preSelect => getter.EnablePreSelect(preSelect.Enabled, preSelect.IgnoreUnacceptable));
        _ = settings.PostSelect.Iter(getter.EnablePostSelect);
        _ = settings.SelPrevious.Iter(getter.EnableSelPrevious);
        _ = settings.Highlight.Iter(getter.EnableHighlight);
        _ = settings.IgnoreGrips.Iter(getter.EnableIgnoreGrips);
        _ = settings.EnablePressEnterWhenDonePrompt.Iter(getter.EnablePressEnterWhenDonePrompt);
        _ = settings.SubObjectSelect.Iter(subObject => getter.SubObjectSelect = subObject);
        _ = settings.GroupSelect.Iter(group => getter.GroupSelect = group);
        return settings.End.Switch(onEnter: static _ => 0, atMinimum: static _ => -1, atCount: static count => count.Maximum);
    }

    private static IO<IDisposable> Subscribe(GetPoint getter, PointHandlers handlers, Atom<Option<Point3d>> cursor) =>
        Events.AttachAll(Seq(
            Attached<GetPointDrawEventArgs, Point3d>(
                Some<Func<Point3d, IO<Unit>>>(point => cursor.SwapIO(_ => Some(point)).Map(static _ => unit)),
                static args => args.CurrentPoint,
                h => getter.DynamicDraw += h,
                h => getter.DynamicDraw -= h),
            Attached<GetPointMouseEventArgs, PointMouseEvent>(handlers.MouseMove, MouseEvent, h => getter.MouseMove += h, h => getter.MouseMove -= h),
            Attached<GetPointMouseEventArgs, PointMouseEvent>(handlers.MouseDown.Map(down => fun((PointMouseEvent mouse) => down(getter, mouse))), MouseEvent, h => getter.MouseDown += h, h => getter.MouseDown -= h),
            Attached<GetPointDrawEventArgs, GetPointDrawEventArgs>(handlers.DynamicDraw, static args => args, h => getter.DynamicDraw += h, h => getter.DynamicDraw -= h),
            Attached<DrawEventArgs, DrawEventArgs>(handlers.PostDraw, static args => args, h => getter.PostDrawObjects += h, h => getter.PostDrawObjects -= h))
            .Somes());

    private static Option<IO<IDisposable>> Attached<TArgs, TValue>(
        Option<Func<TValue, IO<Unit>>> handler,
        Func<TArgs, TValue> project,
        Action<EventHandler<TArgs>> subscribe,
        Action<EventHandler<TArgs>> unsubscribe) =>
        handler.Map(handle => Events.Attach(subscribe, unsubscribe, Answers.Handler<TArgs>(args => handle(project(args)), ErrorOps.Report)));

    private static PointMouseEvent MouseEvent(GetPointMouseEventArgs args) =>
        new(
            Some(args.Point).Filter(static point => point.IsValid),
            Answers.Found(args.Viewport.GetFrustumLine(args.WindowPoint.X, args.WindowPoint.Y, out Line pickLine), pickLine),
            args.WindowPoint,
            args.Viewport.Id,
            args.LeftButtonDown,
            args.MiddleButtonDown,
            args.RightButtonDown,
            args.ShiftKeyDown,
            args.ControlKeyDown);

    // --- [RESULTS]
    private static IO<T> Read<T>(GetBaseClass getter, Accepts<T> accept, Map<int, Func<CommandLineOption, IO<T>>> bound, GetResult result, Seq<(GetResult Result, IO<T> Then)> own) =>
        toMapTry(own + Accepted(getter, accept, bound)).Find(result).IfNone(() => IO.fail<T>(Ended(result)));

    private static Seq<(GetResult Result, IO<T> Then)> Accepted<T>(GetBaseClass getter, Accepts<T> accept, Map<int, Func<CommandLineOption, IO<T>>> bound) =>
        Seq(
            accept.Nothing.Map(static then => (GetResult.Nothing, then)),
            accept.Undo.Map(static then => (GetResult.Undo, then)),
            accept.String.Map(then => (GetResult.String, IO.lift(getter.StringResult).Bind(then))),
            accept.Color.Map(then => (GetResult.Color, IO.lift(getter.Color).Bind(then))),
            accept.Number.Map(number => (GetResult.Number, IO.lift(getter.Number).Bind(number.Then))),
            accept.Point.Map(then => (GetResult.Point, Pick(getter).Bind(then))),
            accept.Timeout.Map(static wait => (GetResult.Timeout, wait.Then)),
            Some((GetResult.Option, CommandOptions.Chosen(getter, bound))))
            .Somes();

    private static Error Ended(GetResult result) =>
        result switch {
            GetResult.Cancel => new Canceled(),
            GetResult.ExitRhino => new ExitRequested(),
            _ => new UnexpectedGetResult(result),
        };

    private static IO<PointPick> Pick(GetBaseClass getter) =>
        IO.lift(() => new PointPick(getter.Point(), Viewport(getter)));

    private static IO<WorldPick> World(GetPoint getter) =>
        Pick(getter).Map(pick => new WorldPick(pick, getter.GotDefault(), getter.OsnapEventType));

    private static Option<ViewportIdentity> Viewport(GetBaseClass getter) =>
        Optional(getter.View()).Map(static view => Viewports.Identity(view, view.ActiveViewport));
}
