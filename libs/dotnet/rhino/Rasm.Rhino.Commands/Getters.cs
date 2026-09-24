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

public sealed record AcceptPolicy {
    public bool Nothing { get; init; }

    public bool Undo { get; init; }

    public bool EnterWhenDone { get; init; }

    public bool String { get; init; }

    public bool Color { get; init; }

    public Option<bool> NumberAcceptZero { get; init; }

    public Option<bool> Point { get; init; }

    public Option<bool> Transparent { get; init; }

    public Option<int> WaitMilliseconds { get; init; }
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

    public AcceptPolicy Accept { get; init; } = new();

    public Seq<OptionSpec<T>> Options { get; init; }
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

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PointResult<T> {
    public sealed record Point(Point3d Value, Option<ViewportIdentity> Viewport, bool GotDefault, OsnapModes Osnap) : PointResult<T>;

    public sealed record Point2d(System.Drawing.Point Value, Option<ViewportIdentity> Viewport) : PointResult<T>;

    public sealed record Option(OptionSelection Selection, T Chosen) : PointResult<T>;

    public sealed record Number(double Value) : PointResult<T>;

    public sealed record String(string Value) : PointResult<T>;

    public sealed record Color(System.Drawing.Color Value) : PointResult<T>;

    public sealed record Nothing() : PointResult<T>;

    public sealed record Undo() : PointResult<T>;

    public sealed record Timeout() : PointResult<T>;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TransformResult<T> {
    public sealed record Point(Point3d Value, Option<ViewportIdentity> Viewport, bool GotDefault, OsnapModes Osnap) : TransformResult<T>;

    public sealed record Option(OptionSelection Selection, T Chosen) : TransformResult<T>;

    public sealed record Number(double Value) : TransformResult<T>;

    public sealed record String(string Value) : TransformResult<T>;

    public sealed record Color(System.Drawing.Color Value) : TransformResult<T>;

    public sealed record Nothing() : TransformResult<T>;

    public sealed record Undo() : TransformResult<T>;

    public sealed record Timeout() : TransformResult<T>;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ObjectResult<T> {
    public sealed record Objects(Seq<ObjRef> References) : ObjectResult<T>;

    public sealed record Point(Point3d Value, Option<ViewportIdentity> Viewport) : ObjectResult<T>;

    public sealed record Option(OptionSelection Selection, T Chosen) : ObjectResult<T>;

    public sealed record Number(double Value) : ObjectResult<T>;

    public sealed record String(string Value) : ObjectResult<T>;

    public sealed record Color(System.Drawing.Color Value) : ObjectResult<T>;

    public sealed record Nothing() : ObjectResult<T>;

    public sealed record Undo() : ObjectResult<T>;

    public sealed record Timeout() : ObjectResult<T>;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record NumberResult<T, TNumber> where TNumber : struct, System.Numerics.INumber<TNumber> {
    public sealed record Number(TNumber Value, bool GotDefault) : NumberResult<T, TNumber>;

    public sealed record Point(Point3d Value, Option<ViewportIdentity> Viewport) : NumberResult<T, TNumber>;

    public sealed record Option(OptionSelection Selection, T Chosen) : NumberResult<T, TNumber>;

    public sealed record String(string Value) : NumberResult<T, TNumber>;

    public sealed record Color(System.Drawing.Color Value) : NumberResult<T, TNumber>;

    public sealed record Nothing() : NumberResult<T, TNumber>;

    public sealed record Undo() : NumberResult<T, TNumber>;

    public sealed record Timeout() : NumberResult<T, TNumber>;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record StringResult<T> {
    public sealed record String(string Value, bool GotDefault) : StringResult<T>;

    public sealed record Point(Point3d Value, Option<ViewportIdentity> Viewport) : StringResult<T>;

    public sealed record Option(OptionSelection Selection, T Chosen) : StringResult<T>;

    public sealed record Number(double Value) : StringResult<T>;

    public sealed record Color(System.Drawing.Color Value) : StringResult<T>;

    public sealed record Nothing() : StringResult<T>;

    public sealed record Undo() : StringResult<T>;

    public sealed record Timeout() : StringResult<T>;
}

public sealed record TransformObjectState(Seq<Guid> Objects, Seq<Guid> Grips, Seq<Guid> GripOwners, Option<BoundingBox> Extent);

// --- [SERVICES] ------------------------------------------------------------------------
internal sealed class CallbackGetTransform(Func<RhinoViewport, Point3d, Transform> calculate) : GetTransform {
    public override Transform CalculateTransform(RhinoViewport viewport, Point3d point) => calculate(viewport, point);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Getters {
    // --- [GETTERS]
    public static IO<PointResult<T>> GetPoint<T>(PointRequest<T> request, bool onMouseUp, bool get2D) =>
        Disposal.Using(static () => new GetPoint(), getter =>
            from primed in IO.lift(() => Prime(getter, request))
            from defaulted in IO.lift(() => request.Default.Iter(getter.SetDefaultPoint))
            from result in Disposal.Using(CommandOptions.Register(getter, request.Options), holders =>
                from applied in IO.lift(() => Apply(getter, request.Settings, request.Handlers))
                from read in Disposal.Using(Subscribe(getter, request.Handlers), _ => Get(getter, holders, onMouseUp, get2D))
                select read)
            select result);

    public static IO<ObjectResult<T>> GetObjects<T>(ObjectRequest<T> request) =>
        Disposal.Using(static () => new GetObject(), getter =>
            from primed in IO.lift(() => Prime(getter, request))
            from result in Disposal.Using(CommandOptions.Register(getter, request.Options), holders =>
                from getResult in IO.lift(() => getter.GetMultiple(request.Settings.Minimum, Configure(getter, request.Settings)))
                from objects in ReadObjects(getter, holders, getResult)
                select objects)
            select result);

    public static IO<NumberResult<T, double>> GetNumber<T>(ValueRequest<T, double> request, Limits<double> limits) =>
        Value(
            static () => new GetNumber(),
            request,
            static (getter, value) => getter.SetDefaultNumber(value),
            getter => {
                _ = limits.Lower.Iter(lower => getter.SetLowerLimit(lower.Value, lower.Exclusive));
                _ = limits.Upper.Iter(upper => getter.SetUpperLimit(upper.Value, upper.Exclusive));
            },
            static getter => getter.Get(),
            static (getter, holders, result) => ReadNumber(getter, holders, result, static number => number.Number()));

    public static IO<NumberResult<T, int>> GetInteger<T>(ValueRequest<T, int> request, Limits<int> limits) =>
        Value(
            static () => new GetInteger(),
            request,
            static (getter, value) => getter.SetDefaultInteger(value),
            getter => {
                _ = limits.Lower.Iter(lower => getter.SetLowerLimit(lower.Value, lower.Exclusive));
                _ = limits.Upper.Iter(upper => getter.SetUpperLimit(upper.Value, upper.Exclusive));
            },
            static getter => getter.Get(),
            static (getter, holders, result) => ReadNumber(getter, holders, result, static integer => integer.Number()));

    public static IO<StringResult<T>> GetString<T>(ValueRequest<T, string> request, bool literal) =>
        Value(static () => new GetString(), request, static (getter, value) => getter.SetDefaultString(value), static _ => { }, getter => literal ? getter.GetLiteralString() : getter.Get(), ReadString);

    public static IO<(TransformResult<T> Result, Option<Transform> Xform)> GetTransform<T>(PointRequest<T> request, TransformObjectList objects, Func<RhinoViewport, Point3d, Transform> calculate) =>
        Disposal.Using(() => new CallbackGetTransform(calculate), getter =>
            from primed in IO.lift(() => Prime(getter, request))
            from defaulted in IO.lift(() => request.Default.Iter(getter.SetDefaultPoint))
            from result in Disposal.Using(CommandOptions.Register(getter, request.Options), holders =>
                from applied in IO.lift(() => Apply(getter, request.Settings, request.Handlers))
                from added in IO.lift(() => getter.AddTransformObjects(objects))
                from getResult in Disposal.Using(Subscribe(getter, request.Handlers), _ => IO.lift(getter.GetXform))
                from read in ReadTransform(getter, holders, getResult)
                from xform in IO.lift(() => getter.HaveTransform ? Some(getter.Transform) : Option<Transform>.None)
                select (read, xform))
            select result);

    private static IO<TResult> Value<TGetter, T, TValue, TResult>(
        Func<TGetter> create,
        ValueRequest<T, TValue> request,
        Action<TGetter, TValue> setDefault,
        Action<TGetter> configure,
        Func<TGetter, GetResult> get,
        Func<TGetter, OptionHolders<T>, GetResult, IO<TResult>> read) where TGetter : GetBaseClass =>
        Disposal.Using(create, getter =>
            from primed in IO.lift(() => Prime(getter, request))
            from defaulted in IO.lift(() => request.Default.Iter(value => setDefault(getter, value)))
            from result in Disposal.Using(CommandOptions.Register(getter, request.Options), holders =>
                from configured in IO.lift(() => configure(getter))
                from getResult in IO.lift(() => get(getter))
                from answer in read(getter, holders, getResult)
                select answer)
            select result);

    private static IO<PointResult<T>> Get<T>(GetPoint getter, OptionHolders<T> holders, bool onMouseUp, bool get2D) =>
        from getResult in IO.lift(() => getter.Get(onMouseUp, get2D))
        from result in ReadPoint(getter, holders, getResult)
        select result;

    // --- [LOOPS]
    public static IO<TValue> Loop<TState, TResult, TValue>(TState initial, Func<TState, IO<TResult>> ask, Func<TState, TResult, IO<Next<TState, TValue>>> step) =>
        Monad.recur(
                initial,
                state => ask(state)
                    .Bind(result => step(state, result))
                    .IfFail(error => error.IsType<LimitViolation>()
                        ? IO.lift(() => ErrorOps.Report(error)).Map(_ => Next.Loop<TState, TValue>(state))
                        : IO.fail<Next<TState, TValue>>(error)))
            .As();

    // --- [TRANSFORM_OBJECTS]
    public static IO<TransformObjectList> TransformObjects(Seq<ObjRef> references, bool feedback) =>
        from list in IO.lift(() => {
            TransformObjectList list = new();
            _ = references.Iter(reference => list.Add(reference));
            return list;
        })
        from counted in GeometryOps.OnFailure(IO.lift(() => Invalid.Unless(list.Count > 0, nameof(TransformObjectList.Count))), IO.lift(list.Dispose))
        from fed in IO.lift(() => list.DisplayFeedbackEnabled = feedback)
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
        getter.AcceptNothing(request.Accept.Nothing);
        getter.AcceptUndo(request.Accept.Undo);
        getter.AcceptEnterWhenDone(request.Accept.EnterWhenDone);
        getter.AcceptString(request.Accept.String);
        getter.AcceptColor(request.Accept.Color);
        _ = request.Accept.NumberAcceptZero.Iter(acceptZero => getter.AcceptNumber(enable: true, acceptZero));
        _ = request.Accept.Point.Iter(getter.AcceptPoint);
        _ = request.Accept.Transparent.Iter(getter.EnableTransparentCommands);
        _ = request.Accept.WaitMilliseconds.Iter(getter.SetWaitDuration);
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
        _ = Written(getter, settings);
        return settings.End.Switch(onEnter: static _ => 0, atMinimum: static _ => -1, atCount: static count => count.Maximum);
    }

    private static Unit Written(GetObject getter, ObjectSettings settings) {
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
        return unit;
    }

    private static IO<IDisposable> Subscribe(GetPoint getter, PointHandlers handlers) =>
        Events.AttachAll(Seq(
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
    private static IO<PointResult<T>> ReadPoint<T>(GetPoint getter, OptionHolders<T> holders, GetResult result) =>
        result switch {
            GetResult.Point => IO.lift(() => (PointResult<T>)new PointResult<T>.Point(getter.Point(), Viewport(getter), getter.GotDefault(), getter.OsnapEventType)),
            GetResult.Point2d => IO.lift(() => (PointResult<T>)new PointResult<T>.Point2d(getter.Point2d(), Viewport(getter))),
            GetResult.Option => CommandOptions.Selected(getter, holders).Map(static selected => (PointResult<T>)new PointResult<T>.Option(selected.Selection, selected.Key)),
            GetResult.Number => IO.lift(() => (PointResult<T>)new PointResult<T>.Number(getter.Number())),
            GetResult.String => IO.lift(() => (PointResult<T>)new PointResult<T>.String(getter.StringResult())),
            GetResult.Color => IO.lift(() => (PointResult<T>)new PointResult<T>.Color(getter.Color())),
            GetResult.Nothing => IO.pure<PointResult<T>>(new PointResult<T>.Nothing()),
            GetResult.Undo => IO.pure<PointResult<T>>(new PointResult<T>.Undo()),
            GetResult.Timeout => IO.pure<PointResult<T>>(new PointResult<T>.Timeout()),
            _ => IO.fail<PointResult<T>>(Ended(result)),
        };

    private static IO<TransformResult<T>> ReadTransform<T>(GetTransform getter, OptionHolders<T> holders, GetResult result) =>
        result switch {
            GetResult.Point => IO.lift(() => (TransformResult<T>)new TransformResult<T>.Point(getter.Point(), Viewport(getter), getter.GotDefault(), getter.OsnapEventType)),
            GetResult.Option => CommandOptions.Selected(getter, holders).Map(static selected => (TransformResult<T>)new TransformResult<T>.Option(selected.Selection, selected.Key)),
            GetResult.Number => IO.lift(() => (TransformResult<T>)new TransformResult<T>.Number(getter.Number())),
            GetResult.String => IO.lift(() => (TransformResult<T>)new TransformResult<T>.String(getter.StringResult())),
            GetResult.Color => IO.lift(() => (TransformResult<T>)new TransformResult<T>.Color(getter.Color())),
            GetResult.Nothing => IO.pure<TransformResult<T>>(new TransformResult<T>.Nothing()),
            GetResult.Undo => IO.pure<TransformResult<T>>(new TransformResult<T>.Undo()),
            GetResult.Timeout => IO.pure<TransformResult<T>>(new TransformResult<T>.Timeout()),
            _ => IO.fail<TransformResult<T>>(Ended(result)),
        };

    private static IO<ObjectResult<T>> ReadObjects<T>(GetObject getter, OptionHolders<T> holders, GetResult result) =>
        result switch {
            GetResult.Object => IO.lift(() => (ObjectResult<T>)new ObjectResult<T>.Objects(toSeq(getter.Objects()))),
            GetResult.Point => IO.lift(() => (ObjectResult<T>)new ObjectResult<T>.Point(getter.Point(), Viewport(getter))),
            GetResult.Option => CommandOptions.Selected(getter, holders).Map(static selected => (ObjectResult<T>)new ObjectResult<T>.Option(selected.Selection, selected.Key)),
            GetResult.Number => IO.lift(() => (ObjectResult<T>)new ObjectResult<T>.Number(getter.Number())),
            GetResult.String => IO.lift(() => (ObjectResult<T>)new ObjectResult<T>.String(getter.StringResult())),
            GetResult.Color => IO.lift(() => (ObjectResult<T>)new ObjectResult<T>.Color(getter.Color())),
            GetResult.Nothing => IO.pure<ObjectResult<T>>(new ObjectResult<T>.Nothing()),
            GetResult.Undo => IO.pure<ObjectResult<T>>(new ObjectResult<T>.Undo()),
            GetResult.Timeout => IO.pure<ObjectResult<T>>(new ObjectResult<T>.Timeout()),
            _ => IO.fail<ObjectResult<T>>(Ended(result)),
        };

    private static IO<NumberResult<T, TNumber>> ReadNumber<TGetter, T, TNumber>(TGetter getter, OptionHolders<T> holders, GetResult result, Func<TGetter, TNumber> number)
        where TGetter : GetBaseClass
        where TNumber : struct, System.Numerics.INumber<TNumber> =>
        result switch {
            GetResult.Number => IO.lift(() => (NumberResult<T, TNumber>)new NumberResult<T, TNumber>.Number(number(getter), getter.GotDefault())),
            GetResult.Point => IO.lift(() => (NumberResult<T, TNumber>)new NumberResult<T, TNumber>.Point(getter.Point(), Viewport(getter))),
            GetResult.Option => CommandOptions.Selected(getter, holders).Map(static selected => (NumberResult<T, TNumber>)new NumberResult<T, TNumber>.Option(selected.Selection, selected.Key)),
            GetResult.String => IO.lift(() => (NumberResult<T, TNumber>)new NumberResult<T, TNumber>.String(getter.StringResult())),
            GetResult.Color => IO.lift(() => (NumberResult<T, TNumber>)new NumberResult<T, TNumber>.Color(getter.Color())),
            GetResult.Nothing => IO.pure<NumberResult<T, TNumber>>(new NumberResult<T, TNumber>.Nothing()),
            GetResult.Undo => IO.pure<NumberResult<T, TNumber>>(new NumberResult<T, TNumber>.Undo()),
            GetResult.Timeout => IO.pure<NumberResult<T, TNumber>>(new NumberResult<T, TNumber>.Timeout()),
            _ => IO.fail<NumberResult<T, TNumber>>(Ended(result)),
        };

    private static IO<StringResult<T>> ReadString<T>(GetString getter, OptionHolders<T> holders, GetResult result) =>
        result switch {
            GetResult.String => IO.lift(() => (StringResult<T>)new StringResult<T>.String(getter.StringResult(), getter.GotDefault())),
            GetResult.Point => IO.lift(() => (StringResult<T>)new StringResult<T>.Point(getter.Point(), Viewport(getter))),
            GetResult.Option => CommandOptions.Selected(getter, holders).Map(static selected => (StringResult<T>)new StringResult<T>.Option(selected.Selection, selected.Key)),
            GetResult.Number => IO.lift(() => (StringResult<T>)new StringResult<T>.Number(getter.Number())),
            GetResult.Color => IO.lift(() => (StringResult<T>)new StringResult<T>.Color(getter.Color())),
            GetResult.Nothing => IO.pure<StringResult<T>>(new StringResult<T>.Nothing()),
            GetResult.Undo => IO.pure<StringResult<T>>(new StringResult<T>.Undo()),
            GetResult.Timeout => IO.pure<StringResult<T>>(new StringResult<T>.Timeout()),
            _ => IO.fail<StringResult<T>>(Ended(result)),
        };

    private static Error Ended(GetResult result) =>
        result switch {
            GetResult.Cancel => new Canceled(),
            GetResult.ExitRhino => new ExitRequested(),
            _ => new UnexpectedGetResult(result),
        };

    private static Option<ViewportIdentity> Viewport(GetBaseClass getter) =>
        Optional(getter.View()).Map(static view => Viewports.Identity(view, view.ActiveViewport));
}
