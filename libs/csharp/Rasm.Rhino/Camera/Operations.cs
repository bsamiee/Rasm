using Rasm.Vectors;
using DrawingBitmap = System.Drawing.Bitmap;
using DrawingPoint = System.Drawing.Point;
using DrawingRectangle = System.Drawing.Rectangle;
using XmlDocument = System.Xml.XmlDocument;

namespace Rasm.Rhino.Camera;

// --- [TYPES] ------------------------------------------------------------------------------
[Union(SwitchMapStateParameterName = "scope")]
public abstract partial record CameraEdit {
    private CameraEdit() { }

    public sealed record Native(Func<RhinoViewport, bool> Run) : CameraEdit;
    public sealed record Frame(CameraFrame Value) : CameraEdit;
    public sealed record NavigateLookAt(Point3d From, Point3d At, Option<Vector3d> UpAxis = default) : CameraEdit;
    public sealed record SubjectFrame(CameraSubject Subject, FramePaddingMode Mode, double Amount = CameraDefaults.FramePadding) : CameraEdit;
    public sealed record Location(Point3d Value, bool UpdateTarget = true) : CameraEdit;
    public sealed record Target(Point3d Value, bool UpdateLocation = true) : CameraEdit;
    public sealed record Direction(Vector3d Value, bool UpdateTarget = true) : CameraEdit;
    public sealed record Up(Vector3d Value) : CameraEdit;
    public sealed record Lens(double Millimeters) : CameraEdit;
    public sealed record Angle(double Radians) : CameraEdit;
    public sealed record Defined(DefinedViewportProjection Projection, string Name = "", bool UpdateConstructionPlane = true) : CameraEdit;
    public sealed record Snapshot(ViewportInfo Projection, bool UpdateTarget = true) : CameraEdit;
    public sealed record Project(CameraProjection Projection) : CameraEdit;
    public sealed record Frustum(CameraFrustum Value, bool UpdateTarget = true) : CameraEdit;
    public sealed record Isometric(IsometricCamera Camera, string Name = "", bool UpdateConstructionPlane = true) : CameraEdit;
    public sealed record PlanePlain(Plane Value) : CameraEdit;
    public sealed record PlaneFull(ConstructionPlane Value) : CameraEdit;
    public sealed record Plan(Point3d Origin, Vector3d XDirection, Vector3d YDirection, bool SetConstructionPlane = true) : CameraEdit;
    public sealed record DisplayMode(DisplayModeDescription Value) : CameraEdit;
    public sealed record Clipping(BoundingBox Box) : CameraEdit;
    public sealed record Transform(global::Rhino.Geometry.Transform Xform) : CameraEdit;
    public sealed record Zoom : CameraEdit;
    public sealed record ZoomSelected : CameraEdit;
    public sealed record ZoomBox(BoundingBox Bounds) : CameraEdit;
    public sealed record ZoomWindow(DrawingRectangle Window) : CameraEdit;
    public sealed record Rotate(double Radians, Vector3d Axis, Point3d Center) : CameraEdit;
    public sealed record Magnify(double Factor, bool LensMode, Option<DrawingPoint> FixedPoint = default) : CameraEdit;
    public sealed record Keyboard(CameraKeyboardMove Move, bool LeftRight, double Amount) : CameraEdit;
    public sealed record Mouse(CameraMouseMove Move, DrawingPoint PreviousPoint, DrawingPoint CurrentPoint) : CameraEdit;
    public sealed record MouseLens(DrawingPoint PreviousPoint, DrawingPoint CurrentPoint, bool MoveTarget) : CameraEdit;
    public sealed record StackMove(CameraStackOp Move, Option<ConstructionPlane> Plane = default) : CameraEdit;
    public sealed record PushView(ViewInfo Info, bool IncludeTraceImage = true) : CameraEdit;

    internal Fin<RedrawRequest> Apply(CameraScope scope) => Apply(edit: this, scope: scope);

    private static Fin<RedrawRequest> Apply(CameraEdit edit, CameraScope scope) =>
        edit.Switch(
            scope,
            native: static (ctx, value) => from valid in Optional(value.Run).ToFin(Fail: Op.Of(name: nameof(Native)).InvalidInput())
                                           from _ in Op.Of(name: nameof(Native)).Confirm(success: valid(arg: ctx.Viewport))
                                           select CameraScope.RedrawFor(scope: ctx),
            frame: static (ctx, value) => value.Value.Apply(viewport: ctx.Viewport, op: Op.Of(name: nameof(Frame)))
                .Map(_ => CameraScope.RedrawFor(scope: ctx)),
            navigateLookAt: static (ctx, value) => from frame in CameraFrame.LookAt(location: value.From, target: value.At, up: value.UpAxis)
                                                   from _ in frame.Apply(viewport: ctx.Viewport, op: Op.Of(name: nameof(NavigateLookAt)))
                                                   select CameraScope.RedrawFor(scope: ctx),
            subjectFrame: static (ctx, value) => from amount in ValidDouble(value: value.Amount, op: Op.Of(name: nameof(SubjectFrame)), positive: true)
                                                 from bounds in value.Subject.BoundsOf(op: Op.Of(name: nameof(SubjectFrame)))
                                                 let box = value.Mode.Inflate(source: bounds, amount: amount)
                                                 from _ in guard(box.IsValid, Op.Of(name: nameof(SubjectFrame)).InvalidInput())
                                                 from __ in Op.Of(name: nameof(SubjectFrame)).Confirm(success: ctx.Viewport.ZoomBoundingBox(box: box))
                                                 select CameraScope.RedrawFor(scope: ctx),
            location: static (ctx, value) => from point in ValidPoint(value: value.Value, op: Op.Of(name: nameof(Location)))
                                             from redraw in Side(scope: ctx, apply: vp => vp.SetCameraLocation(cameraLocation: point, updateTargetLocation: value.UpdateTarget))
                                             select redraw,
            target: static (ctx, value) => from point in ValidPoint(value: value.Value, op: Op.Of(name: nameof(Target)))
                                           from redraw in Side(scope: ctx, apply: vp => vp.SetCameraTarget(targetLocation: point, updateCameraLocation: value.UpdateLocation))
                                           select redraw,
            direction: static (ctx, value) => WithDirection(scope: ctx, value: value.Value, op: Op.Of(name: nameof(Direction)),
                apply: dir => Side(scope: ctx, apply: vp => vp.SetCameraDirection(cameraDirection: dir, updateTargetLocation: value.UpdateTarget))),
            up: static (ctx, value) => WithDirection(scope: ctx, value: value.Value, op: Op.Of(name: nameof(Up)),
                apply: dir => Side(scope: ctx, apply: vp => vp.CameraUp = dir)),
            lens: static (ctx, value) => from millimeters in ValidDouble(value: value.Millimeters, op: Op.Of(name: nameof(Lens)), positive: true)
                                         from redraw in Side(scope: ctx, apply: vp => vp.Camera35mmLensLength = millimeters)
                                         select redraw,
            angle: static (ctx, value) => from radians in ValidDouble(value: value.Radians, op: Op.Of(name: nameof(Angle)), positive: true)
                                          from redraw in Side(scope: ctx, apply: vp => vp.CameraAngle = radians)
                                          select redraw,
            defined: static (ctx, value) => Result(scope: ctx, apply: vp => vp.SetProjection(projection: value.Projection, viewName: value.Name, updateConstructionPlane: value.UpdateConstructionPlane), Op.Of(name: nameof(Defined))),
            snapshot: static (ctx, value) => ApplyProjection(scope: ctx, apply: (_, _) => Fin.Succ(value: value.Projection), updateTarget: value.UpdateTarget),
            project: static (ctx, value) => from _ in value.Projection.Use(viewport: ctx.Viewport, op: Op.Of(name: nameof(Project)))
                                            select CameraScope.RedrawFor(scope: ctx),
            frustum: static (ctx, value) => ApplyProjection(scope: ctx, apply: (_, projection) => value.Value.Apply(projection: projection, op: Op.Of(name: nameof(Frustum))), updateTarget: value.UpdateTarget),
            isometric: static (ctx, value) => Result(scope: ctx, apply: vp => vp.SetProjection(projection: value.Camera, viewName: value.Name, updateConstructionPlane: value.UpdateConstructionPlane), Op.Of(name: nameof(Isometric))),
            planePlain: static (ctx, value) => from _ in guard(value.Value.IsValid, Op.Of(name: nameof(PlanePlain)).InvalidInput())
                                               from redraw in Side(scope: ctx, apply: vp => vp.SetConstructionPlane(plane: value.Value))
                                               select redraw,
            planeFull: static (ctx, value) => from _ in Optional(value.Value).ToFin(Fail: Op.Of(name: nameof(PlaneFull)).InvalidInput())
                                              from redraw in Side(scope: ctx, apply: vp => vp.SetConstructionPlane(cplane: value.Value))
                                              select redraw,
            plan: static (ctx, value) => from _ in guard(value.Origin.IsValid && value.XDirection.IsValid && value.YDirection.IsValid
                    && value.XDirection.Length > RhinoMath.ZeroTolerance && value.YDirection.Length > RhinoMath.ZeroTolerance, Op.Of(name: nameof(Plan)).InvalidInput())
                                         from done in Result(scope: ctx, apply: vp => vp.SetToPlanView(value.Origin, value.XDirection, value.YDirection, value.SetConstructionPlane), Op.Of(name: nameof(Plan)))
                                         select done,
            displayMode: static (ctx, value) => from valid in Optional(value.Value).ToFin(Fail: Op.Of(name: nameof(DisplayMode)).InvalidInput())
                                                from redraw in Side(scope: ctx, apply: vp => vp.DisplayMode = valid)
                                                select redraw,
            clipping: static (ctx, value) => from _ in guard(value.Box.IsValid, Op.Of(name: nameof(Clipping)).InvalidInput())
                                             from redraw in Side(scope: ctx, apply: vp => vp.SetClippingPlanes(box: value.Box))
                                             select redraw,
            transform: static (ctx, value) => ApplyProjection(scope: ctx, apply: (_, projection) => Op.Of(name: nameof(Transform)).Confirm(success: projection.TransformCamera(xform: value.Xform)).Map(_ => projection), updateTarget: true),
            zoom: static (ctx, _) => Result(scope: ctx, apply: static vp => vp.ZoomExtents(), Op.Of(name: nameof(Zoom))),
            zoomSelected: static (ctx, _) => Result(scope: ctx, apply: static vp => vp.ZoomExtentsSelected(), Op.Of(name: nameof(ZoomSelected))),
            zoomBox: static (ctx, value) => from _ in guard(value.Bounds.IsValid, Op.Of(name: nameof(ZoomBox)).InvalidInput())
                                            from done in Result(scope: ctx, apply: vp => vp.ZoomBoundingBox(box: value.Bounds), Op.Of(name: nameof(ZoomBox)))
                                            select done,
            zoomWindow: static (ctx, value) => Result(scope: ctx, apply: vp => vp.ZoomWindow(rect: value.Window), Op.Of(name: nameof(ZoomWindow))),
            rotate: static (ctx, value) => WithDirection(scope: ctx, value: value.Axis, op: Op.Of(name: nameof(Rotate)),
                apply: dir => Result(scope: ctx, apply: vp => vp.Rotate(angleRadians: value.Radians, rotationAxis: dir, rotationCenter: value.Center), Op.Of(name: nameof(Rotate)))),
            magnify: static (ctx, value) => from factor in ValidDouble(value: value.Factor, op: Op.Of(name: nameof(Magnify)), positive: true)
                                            from redraw in Result(scope: ctx, apply: vp => value.FixedPoint.Case switch {
                                                DrawingPoint p => vp.Magnify(magnificationFactor: factor, mode: value.LensMode, fixedScreenPoint: p),
                                                _ => vp.Magnify(magnificationFactor: factor, mode: value.LensMode),
                                            }, Op.Of(name: nameof(Magnify)))
                                            select redraw,
            keyboard: static (ctx, value) => from amount in ValidDouble(value: value.Amount, op: Op.Of(name: nameof(Keyboard)))
                                             from redraw in Result(scope: ctx, apply: vp => value.Move.Apply(viewport: vp, leftRight: value.LeftRight, amount: amount), Op.Of(name: nameof(Keyboard)))
                                             select redraw,
            mouse: static (ctx, value) => Result(scope: ctx, apply: vp => value.Move.Apply(viewport: vp, previous: value.PreviousPoint, current: value.CurrentPoint), Op.Of(name: nameof(Mouse))),
            mouseLens: static (ctx, value) => Result(scope: ctx, apply: vp => vp.MouseAdjustLensLength(mousePreviousPoint: value.PreviousPoint, mouseCurrentPoint: value.CurrentPoint, moveTarget: value.MoveTarget), Op.Of(name: nameof(MouseLens))),
            stackMove: static (ctx, value) => Fin.Succ(value: value.Move.Apply(viewport: ctx.Viewport, plane: value.Plane)
                ? CameraScope.RedrawFor(scope: ctx)
                : RedrawRequest.Empty),
            pushView: static (ctx, value) => Result(scope: ctx, apply: vp => vp.PushViewInfo(value.Info, value.IncludeTraceImage), Op.Of(name: nameof(PushView))));

    private static Fin<Point3d> ValidPoint(Point3d value, Op op) =>
        value.IsValid ? Fin.Succ(value: value) : Fin.Fail<Point3d>(error: op.InvalidInput());

    private static Fin<double> ValidDouble(double value, Op op, bool positive = false) =>
        (!positive || value > 0.0) && RhinoMath.IsValidDouble(x: value) ? Fin.Succ(value: value) : Fin.Fail<double>(error: op.InvalidInput());

    private static Fin<RedrawRequest> ApplyProjection(
        CameraScope scope,
        Func<RhinoViewport, ViewportInfo, Fin<ViewportInfo>> apply,
        bool updateTarget) =>
        UI.RhinoUi.Protect(valid: () => {
            using ViewportInfo projection = new(rhinoViewport: scope.Viewport);
            return from authored in apply(arg1: scope.Viewport, arg2: projection)
                   from _ in Op.Of(name: nameof(ApplyProjection)).Confirm(success: scope.Viewport.SetViewProjection(projection: authored, updateTargetLocation: updateTarget))
                   select CameraScope.RedrawFor(scope: scope);
        });

    private static Fin<RedrawRequest> Side(CameraScope scope, Action<RhinoViewport> apply) {
        apply(obj: scope.Viewport);
        return Fin.Succ(value: CameraScope.RedrawFor(scope: scope));
    }

    private static Fin<RedrawRequest> Result(CameraScope scope, Func<RhinoViewport, bool> apply, Op op) =>
        from _ in op.Confirm(success: apply(arg: scope.Viewport))
        select CameraScope.RedrawFor(scope: scope);

    private static Fin<RedrawRequest> WithDirection(CameraScope scope, Vector3d value, Op op, Func<Vector3d, Fin<RedrawRequest>> apply) =>
        from context in Context.Of(doc: scope.Document).ToFin()
        from direction in VectorIntent.Direction(value: value).Project<Vector3d>(context: context, key: op)
        from result in apply(arg: direction)
        select result;
}

[Union]
public abstract partial record CameraAim {
    private CameraAim() { }
    public sealed record Preserve : CameraAim;
    public sealed record Direction(Vector3d Value, Option<Vector3d> Up = default) : CameraAim;
    public sealed record LookAt(Point3d Location, Point3d Target, Option<Vector3d> Up = default) : CameraAim;
    public sealed record Plan(Point3d Origin, Vector3d XDirection, Vector3d YDirection, bool SetConstructionPlane = true) : CameraAim;

    internal Fin<Seq<CameraEdit>> Edits(Op op) =>
        this switch {
            Preserve => Fin.Succ(value: Seq<CameraEdit>()),
            Direction aim when aim.Value.IsValid && aim.Value.Length > RhinoMath.ZeroTolerance =>
                Fin.Succ(value: Seq<CameraEdit>(new CameraEdit.Direction(Value: aim.Value))
                    + aim.Up.Map(value => Seq<CameraEdit>(new CameraEdit.Up(Value: value))).IfNone(Seq<CameraEdit>())),
            LookAt aim when aim.Location.IsValid && aim.Target.IsValid =>
                Fin.Succ(value: Seq<CameraEdit>(new CameraEdit.NavigateLookAt(From: aim.Location, At: aim.Target, UpAxis: aim.Up))),
            Plan aim when aim.Origin.IsValid && aim.XDirection.IsValid && aim.YDirection.IsValid =>
                Fin.Succ(value: Seq<CameraEdit>(new CameraEdit.Plan(Origin: aim.Origin, XDirection: aim.XDirection, YDirection: aim.YDirection, SetConstructionPlane: aim.SetConstructionPlane))),
            _ => Fin.Fail<Seq<CameraEdit>>(error: op.InvalidInput()),
        };
}

[SmartEnum<int>]
public sealed partial class CameraKeyboardMove {
    public static readonly CameraKeyboardMove RotateInPlace = new(key: 0, apply: static (vp, leftRight, amount) => vp.KeyboardRotate(leftRight: leftRight, angleRadians: amount));
    public static readonly CameraKeyboardMove Dolly = new(key: 1, apply: static (vp, leftRight, amount) => vp.KeyboardDolly(leftRight: leftRight, amount: amount));
    public static readonly CameraKeyboardMove DollyInOut = new(key: 2, apply: static (vp, _, amount) => vp.KeyboardDollyInOut(amount: amount));

    [UseDelegateFromConstructor] internal partial bool Apply(RhinoViewport viewport, bool leftRight, double amount);
}

[SmartEnum<int>]
public sealed partial class CameraMouseMove {
    public static readonly CameraMouseMove RotateAroundTarget = new(key: 0, apply: static (vp, prev, curr) => vp.MouseRotateAroundTarget(mousePreviousPoint: prev, mouseCurrentPoint: curr));
    public static readonly CameraMouseMove RotateCamera = new(key: 1, apply: static (vp, prev, curr) => vp.MouseRotateCamera(mousePreviousPoint: prev, mouseCurrentPoint: curr));
    public static readonly CameraMouseMove InOutDolly = new(key: 2, apply: static (vp, prev, curr) => vp.MouseInOutDolly(mousePreviousPoint: prev, mouseCurrentPoint: curr));
    public static readonly CameraMouseMove Magnify = new(key: 3, apply: static (vp, prev, curr) => vp.MouseMagnify(mousePreviousPoint: prev, mouseCurrentPoint: curr));
    public static readonly CameraMouseMove Tilt = new(key: 4, apply: static (vp, prev, curr) => vp.MouseTilt(mousePreviousPoint: prev, mouseCurrentPoint: curr));
    public static readonly CameraMouseMove DollyZoom = new(key: 5, apply: static (vp, prev, curr) => vp.MouseDollyZoom(mousePreviousPoint: prev, mouseCurrentPoint: curr));
    public static readonly CameraMouseMove LateralDolly = new(key: 6, apply: static (vp, prev, curr) => vp.MouseLateralDolly(mousePreviousPoint: prev, mouseCurrentPoint: curr));

    [UseDelegateFromConstructor] internal partial bool Apply(RhinoViewport viewport, DrawingPoint previous, DrawingPoint current);
}

[Union(SwitchMapStateParameterName = "context")]
public abstract partial record CameraPath {
    private CameraPath() { }

    public sealed record Instant : CameraPath;
    public sealed record MatchAspect : CameraPath;
    public sealed record Animated(CameraRestoreMode Mode, double Amount, int DelayMilliseconds) : CameraPath;

    internal Fin<RedrawRequest> Restore(CameraScope scope, int index) =>
        Switch(
            (Scope: scope, Index: index),
            instant: static (ctx, _) => Op.Of(name: nameof(Restore)).Confirm(success: ctx.Scope.Document.NamedViews.Restore(index: ctx.Index, viewport: ctx.Scope.Viewport))
                .Map(_ => CameraScope.RedrawFor(scope: ctx.Scope)),
            matchAspect: static (ctx, _) => Op.Of(name: nameof(Restore)).Confirm(success: ctx.Scope.Document.NamedViews.RestoreWithAspectRatio(index: ctx.Index, viewport: ctx.Scope.Viewport))
                .Map(_ => CameraScope.RedrawFor(scope: ctx.Scope)),
            animated: static (ctx, path) =>
                from mode in Optional(path.Mode).ToFin(Fail: Op.Of(name: nameof(Restore)).InvalidInput())
                from restored in Op.Of(name: nameof(Restore)).Confirm(success: mode.Restore(ctx.Scope.Document.NamedViews, ctx.Index, ctx.Scope.Viewport, path.Amount, path.DelayMilliseconds))
                select (RedrawRequest)new RedrawRequest.Deferred());

    internal Fin<CameraPath> RequireSynchronous(Op op) =>
        this switch {
            Instant or MatchAspect => Fin.Succ(value: this),
            _ => Fin.Fail<CameraPath>(error: op.InvalidInput()),
        };
}

[Union(SwitchMapStateParameterName = "scope")]
public abstract partial record CameraPick {
    private static readonly Op ResolveKey = Op.Of(name: nameof(CameraPick));
    private CameraPick() { }

    public sealed record Screen(double ClientX, double ClientY, double Depth = 0.0) : CameraPick;
    public sealed record Subject(CameraSubject Value) : CameraPick;

    internal Fin<Point3d> Resolve(CameraScope scope) =>
        Switch(
            scope,
            screen: static (ctx, pick) => UI.RhinoUi.Protect(valid: () =>
                ctx.FrustumLine(screenX: pick.ClientX, screenY: pick.ClientY)
                    .Map(line => pick.Depth == 0.0 ? line.From : line.PointAt(t: pick.Depth))),
            subject: static (ctx, pick) => pick.Value switch {
                CameraSubject.AtPoint source when source.Value.IsValid => Fin.Succ(value: source.Value),
                CameraSubject.AtPoint => Fin.Fail<Point3d>(error: ResolveKey.InvalidInput()),
                CameraSubject value => value.BoundsOf(op: ResolveKey).Map(bounds => bounds.Center),
            });
}

[Union(SwitchMapStateParameterName = "viewport")]
public abstract partial record CameraProjection {
    private CameraProjection() { }

    public sealed record Parallel(bool SymmetricFrustum = true) : CameraProjection;
    public sealed record Perspective(Option<double> TargetDistance = default, bool SymmetricFrustum = true, double LensLength = CameraDefaults.LensLength) : CameraProjection;
    public sealed record TwoPointPerspective(double LensLength = CameraDefaults.LensLength, Option<(Vector3d Up, double TargetDistance)> Target = default) : CameraProjection;
    public sealed record ParallelReflected : CameraProjection;

    internal Fin<Unit> Use(RhinoViewport viewport, Op op) =>
        Optional(viewport).ToFin(Fail: op.InvalidInput()).Bind(active => Switch(
            (Viewport: active, Op: op),
            parallel: static (ctx, p) => ctx.Op.Confirm(success: ctx.Viewport.ChangeToParallelProjection(symmetricFrustum: p.SymmetricFrustum)),
            perspective: static (ctx, p) => ctx.Op.Confirm(success: ctx.Viewport.ChangeToPerspectiveProjection(
                targetDistance: p.TargetDistance.IfNone(RhinoMath.UnsetValue),
                symmetricFrustum: p.SymmetricFrustum,
                lensLength: p.LensLength)),
            twoPointPerspective: static (ctx, p) => ctx.Op.Confirm(success: ctx.Viewport.ChangeToTwoPointPerspectiveProjection(
                lensLength: p.LensLength,
                up: p.Target.Map(static item => item.Up).IfNone(Vector3d.Zero),
                targetDistance: p.Target.Map(static item => item.TargetDistance).IfNone(RhinoMath.UnsetValue))),
            parallelReflected: static (ctx, _) => ctx.Op.Confirm(success: ctx.Viewport.ChangeToParallelReflectedProjection())));
}

[SmartEnum<int>]
public sealed partial class CameraRestoreMode {
    public static readonly CameraRestoreMode
        Speed = new(
            key: 0,
            restore: static (views, index, viewport, amount, delay) =>
                amount > 0.0 && views.RestoreAnimatedConstantSpeed(index, viewport, amount, delay)),
        Time = new(
            key: 1,
            restore: static (views, index, viewport, amount, delay) =>
                amount > 0.0 && Math.Abs(amount - Math.Round(amount, MidpointRounding.ToEven)) <= RhinoMath.ZeroTolerance
                && Math.Round(amount, MidpointRounding.ToEven) <= int.MaxValue
                && views.RestoreAnimatedConstantTime(index, viewport, (int)Math.Round(amount, MidpointRounding.ToEven), delay));

    [UseDelegateFromConstructor]
    internal partial bool Restore(global::Rhino.DocObjects.Tables.NamedViewTable views, int index, RhinoViewport viewport, double amount, int delayMilliseconds);
}

[SmartEnum<int>]
public sealed partial class CameraStackOp {
    public static readonly CameraStackOp ViewPush = new(key: 0, apply: static (vp, _) => Op.Side(vp.PushViewProjection) switch { _ => true });
    public static readonly CameraStackOp ViewPop = new(key: 1, apply: static (vp, _) => vp.PopViewProjection());
    public static readonly CameraStackOp ViewNext = new(key: 2, apply: static (vp, _) => vp.NextViewProjection());
    public static readonly CameraStackOp ViewPrevious = new(key: 3, apply: static (vp, _) => vp.PreviousViewProjection());
    public static readonly CameraStackOp CPlanePush = new(key: 4, apply: static (vp, plane) =>
        Op.Side(() => vp.PushConstructionPlane(cplane: plane.Case switch {
            ConstructionPlane cplane => cplane,
            _ => vp.GetConstructionPlane(),
        })) switch { _ => true });
    public static readonly CameraStackOp CPlanePop = new(key: 5, apply: static (vp, _) => vp.PopConstructionPlane());
    public static readonly CameraStackOp CPlaneNext = new(key: 6, apply: static (vp, _) => vp.NextConstructionPlane());
    public static readonly CameraStackOp CPlanePrevious = new(key: 7, apply: static (vp, _) => vp.PreviousConstructionPlane());

    [UseDelegateFromConstructor] internal partial bool Apply(RhinoViewport viewport, Option<ConstructionPlane> plane);
}

[SmartEnum<int>]
public sealed partial class FramePaddingMode {
    public static readonly FramePaddingMode
        None = new(key: 0, inflate: static (source, _) => source),
        Symmetric = new(key: 1, inflate: static (source, amount) => Diagonal(source: source, factor: (amount - 1.0) * 0.5)),
        Additive = new(key: 2, inflate: static (source, amount) => Diagonal(source: source, factor: amount));

    [UseDelegateFromConstructor] internal partial BoundingBox Inflate(BoundingBox source, double amount);

    private static BoundingBox Diagonal(BoundingBox source, double factor) {
        BoundingBox box = source;
        box.Inflate(xAmount: box.Diagonal.X * factor, yAmount: box.Diagonal.Y * factor, zAmount: box.Diagonal.Z * factor);
        return box;
    }
}

// --- [MODELS] -----------------------------------------------------------------------------
public readonly record struct CameraChangeReceipt(
    Seq<Commands.DocumentResourceChange> Resources,
    RedrawRequest Redraw) {
    public static CameraChangeReceipt Empty => new(Resources: Seq<Commands.DocumentResourceChange>(), Redraw: RedrawRequest.Empty);
    public bool RedrawRequested => Redraw != RedrawRequest.Empty;

    public static CameraChangeReceipt Of(Seq<Commands.DocumentResourceChange> resources, RedrawRequest redraw) =>
        new(Resources: resources, Redraw: redraw);
    public static CameraChangeReceipt WithRedraw(RedrawRequest redraw) =>
        Of(resources: Seq<Commands.DocumentResourceChange>(), redraw: redraw);
    public static CameraChangeReceipt operator +(CameraChangeReceipt left, CameraChangeReceipt right) =>
        Of(resources: left.Resources + right.Resources, redraw: left.Redraw | right.Redraw);
}

public readonly record struct CameraSection(
    Plane Plane,
    Option<double> Depth = default,
    Option<string> Name = default,
    Seq<Guid> ClipObjects = default,
    Seq<int> ClipLayers = default,
    bool ExclusionList = false,
    Option<SectionStyle> Style = default) {
    internal Fin<Commands.DocumentResourceChange> Apply(CameraScope scope, Op op) {
        Plane plane = Plane;
        Option<double> depth = Depth;
        Option<string> name = Name;
        Seq<Guid> clipObjects = ClipObjects;
        Seq<int> clipLayers = ClipLayers;
        bool exclusionList = ExclusionList;
        Option<SectionStyle> style = Style;
        return from _plane in guard(plane.IsValid, op.InvalidInput())
               from validDepth in depth.Map(value => RhinoMath.IsValidDouble(x: value) && value > 0.0 ? Fin.Succ(value: Some(value)) : Fin.Fail<Option<double>>(error: op.InvalidInput()))
                   .IfNone(Fin.Succ(value: Option<double>.None))
               from result in op.Catch(() => {
                   using ClippingPlaneSurface surface = new(plane: plane);
                   _ = validDepth.Iter(value => {
                       surface.PlaneDepth = value;
                       surface.PlaneDepthEnabled = true;
                   });
                   _ = Op.SideWhen(!clipObjects.IsEmpty || !clipLayers.IsEmpty, () => {
                       surface.ParticipationListsEnabled = true;
                       surface.SetClipParticipation(objectIds: clipObjects.AsIterable(), layerIndices: clipLayers.AsIterable(), isExclusionList: exclusionList);
                   });
                   ObjectAttributes attributes = scope.Document.CreateDefaultAttributes();
                   _ = name.Iter(value => attributes.Name = value);
                   _ = style.Iter(value => attributes.SetCustomSectionStyle(sectionStyle: value));
                   Guid id = scope.Document.Objects.AddClippingPlaneSurface(clippingPlane: surface, attributes: attributes, history: null, reference: false);
                   return id != Guid.Empty && scope.Document.Objects.FindId(id: id) is ClippingPlaneObject section
                       ? op.Confirm(success: section.AddClipViewport(viewport: scope.Viewport, commit: true))
                           .Map(_ => Commands.DocumentResourceKind.Object.Change(name: name.IfNone(id.ToString(format: "D"))))
                       : Fin.Fail<Commands.DocumentResourceChange>(error: op.InvalidResult());
               })
               select result;
    }
}

public readonly record struct CameraShot(
    CameraSubject Subject,
    CameraProjection Projection,
    CameraAim? Aim = null,
    FramePaddingMode? Padding = null,
    double PaddingAmount = CameraDefaults.FramePadding,
    Option<double> Lens = default,
    Option<DisplayModeDescription> Display = default,
    Option<BoundingBox> ViewClipping = default,
    Seq<CameraSection> Sections = default,
    Seq<CameraEdit> Edits = default) {
    internal Fin<Seq<CameraEdit>> Compile(Op op) {
        CameraSubject subjectSource = Subject;
        CameraProjection projectionSource = Projection;
        CameraAim aimSource = Aim ?? new CameraAim.Preserve();
        FramePaddingMode padding = Padding ?? FramePaddingMode.Symmetric;
        double paddingAmount = PaddingAmount;
        Option<double> lens = Lens;
        Option<DisplayModeDescription> display = Display;
        Option<BoundingBox> viewClipping = ViewClipping;
        Seq<CameraEdit> edits = Edits;
        return Optional(subjectSource).ToFin(Fail: op.InvalidInput()).Bind(subject =>
            Optional(projectionSource).ToFin(Fail: op.InvalidInput()).Bind(projection =>
                aimSource.Edits(op: op).Map(aim => Seq<CameraEdit>(new CameraEdit.Project(Projection: projection))
            + aim
            + lens.Map(value => Seq<CameraEdit>(new CameraEdit.Lens(Millimeters: value))).IfNone(Seq<CameraEdit>())
            + Seq<CameraEdit>(new CameraEdit.SubjectFrame(Subject: subject, Mode: padding, Amount: paddingAmount))
            + display.Map(value => Seq<CameraEdit>(new CameraEdit.DisplayMode(Value: value))).IfNone(Seq<CameraEdit>())
            + viewClipping.Map(value => Seq<CameraEdit>(new CameraEdit.Clipping(Box: value))).IfNone(Seq<CameraEdit>())
            + edits)));
    }
}

public sealed record CameraOp<T>(Func<CameraScope, Fin<CameraOutcome<T>>> Run, bool UiBound = true) {
    public CameraOp<TNext> Map<TNext>(Func<T, TNext> project) =>
        new(Run: scope => Run(arg: scope)
                .Map(outcome => CameraOutcome<TNext>.Create(
                    value: project(arg: outcome.Value),
                    redraw: outcome.Redraw,
                    resources: outcome.Resources)),
            UiBound: UiBound);
    public CameraOp<TNext> Bind<TNext>(Func<T, CameraOp<TNext>> bind) =>
        new(Run: scope => Run(arg: scope).Bind(outcome => bind(arg: outcome.Value).Run(arg: scope).Map(next => next with {
            Redraw = outcome.Redraw | next.Redraw,
            Resources = outcome.Resources + next.Resources,
        })), UiBound: true);
    public CameraOp<T> Catch(Func<Error, CameraOp<T>> handle) =>
        new(Run: scope => Run(arg: scope).BindFail(error => handle(arg: error).Run(arg: scope)), UiBound: true);

    public CameraOp<T> MapFail(Func<Error, Error> map) =>
        new(Run: scope => Run(arg: scope).MapFail(map), UiBound: UiBound);
}

public readonly record struct NamedRestorePolicy(bool CPlane = true, bool Projection = true, bool Clipping = true, bool Display = true) {
    public static NamedRestorePolicy Default { get; } = new(CPlane: true, Projection: true, Clipping: true, Display: true);

    internal Fin<T> Use<T>(Func<Fin<T>> run) {
        NamedRestorePolicy self = this;
        Op op = Op.Of(name: nameof(NamedRestorePolicy));
        return Optional(run).ToFin(Fail: op.InvalidInput()).Bind(valid => op.Catch(() => {
            bool cplane = global::Rhino.ApplicationSettings.ViewSettings.DefinedViewSetCPlane;
            bool projection = global::Rhino.ApplicationSettings.ViewSettings.DefinedViewSetProjection;
            bool clipping = global::Rhino.ApplicationSettings.ViewSettings.DefinedViewSetClippingPlanes;
            bool display = global::Rhino.ApplicationSettings.ViewSettings.DefinedViewSetDisplayMode;
            try {
                global::Rhino.ApplicationSettings.ViewSettings.DefinedViewSetCPlane = self.CPlane;
                global::Rhino.ApplicationSettings.ViewSettings.DefinedViewSetProjection = self.Projection;
                global::Rhino.ApplicationSettings.ViewSettings.DefinedViewSetClippingPlanes = self.Clipping;
                global::Rhino.ApplicationSettings.ViewSettings.DefinedViewSetDisplayMode = self.Display;
                return valid();
            } finally {
                global::Rhino.ApplicationSettings.ViewSettings.DefinedViewSetCPlane = cplane;
                global::Rhino.ApplicationSettings.ViewSettings.DefinedViewSetProjection = projection;
                global::Rhino.ApplicationSettings.ViewSettings.DefinedViewSetClippingPlanes = clipping;
                global::Rhino.ApplicationSettings.ViewSettings.DefinedViewSetDisplayMode = display;
            }
        }));
    }
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class CameraOps {
    public static CameraOp<Seq<string>> ListNamed() =>
        new(
            Run: scope => Op.Of(name: nameof(ListNamed)).Catch(() =>
                Fin.Succ(value: CameraOutcome<Seq<string>>.Create(value: toSeq(scope.Document.NamedViews).Map(static view => view.Name)))),
            UiBound: false);

    public static CameraOp<T> Query<T>(Func<CameraScope, Fin<T>> query) =>
        new(
            Run: scope => Optional(query).ToFin(Fail: Op.Of(name: nameof(Query)).InvalidInput())
                .Bind(valid => valid(arg: scope).Map(value => CameraOutcome<T>.Create(value: value))),
            UiBound: false);

    public static CameraOp<T> Read<T>(Func<CameraSnapshot, Fin<T>> project) =>
        new(
            Run: scope => Optional(project).ToFin(Fail: Op.Of(name: nameof(Read)).InvalidInput())
                .Bind(valid => scope.Snapshot().Bind(snapshot => {
                    using (snapshot) { return valid(arg: snapshot).Map(value => CameraOutcome<T>.Create(value: value)); }
                })),
            UiBound: false);

    public static CameraOp<Point3d> Resolve(CameraPick pick) =>
        new(Run: scope => pick.Resolve(scope: scope).Map(point => CameraOutcome<Point3d>.Create(value: point)));

    public static CameraOp<CameraChangeReceipt> Change(params CameraEdit[] edits) =>
        Change(edits: edits, redrawEach: true);

    public static CameraOp<CameraChangeReceipt> Change(IEnumerable<CameraEdit> edits, bool redrawEach = true) =>
        new(Run: scope =>
            from changes in Optional(edits).ToFin(Fail: Op.Of(name: nameof(Change)).InvalidInput()).Map(static source => toSeq(source))
            from outcome in redrawEach switch {
                true => changes.TraverseM(edit => edit.Apply(scope: scope)).As()
                    .Map(redraws => redraws.Fold(RedrawRequest.Empty, static (left, right) => left | right))
                    .Map(redraw => CameraOutcome<CameraChangeReceipt>.Create(
                        value: CameraChangeReceipt.WithRedraw(redraw: redraw),
                        redraw: redraw)),
                false => UI.RhinoUi.Protect(valid: () => {
                    scope.Document.Views.EnableRedraw(enable: false, redrawDocument: false, redrawLayers: false);
                    try {
                        return changes.TraverseM(edit => edit.Apply(scope: scope)).As()
                            .Map(redraws => redraws.Fold(RedrawRequest.Empty, static (left, right) => left | right) | CameraScope.RedrawFor(scope: scope))
                            .Map(redraw => CameraOutcome<CameraChangeReceipt>.Create(
                                value: CameraChangeReceipt.WithRedraw(redraw: redraw),
                                redraw: redraw));
                    } finally {
                        scope.Document.Views.EnableRedraw(enable: true, redrawDocument: false, redrawLayers: false);
                    }
                }),
            }
            select outcome,
            UiBound: true);

    public static CameraOp<CameraChangeReceipt> Shot(CameraShot shot, bool redrawEach = false) =>
        new(Run: scope =>
            from edits in shot.Compile(op: Op.Of(name: nameof(Shot)))
            from changed in Change(edits: edits.AsIterable(), redrawEach: redrawEach).Run(arg: scope)
            from sections in shot.Sections.TraverseM(section => section.Apply(scope: scope, op: Op.Of(name: nameof(Shot)))).As()
            let resources = changed.Resources + sections
            let receipt = changed.Value + CameraChangeReceipt.Of(resources: sections, redraw: CameraScope.RedrawFor(scope: scope))
            select CameraOutcome<CameraChangeReceipt>.Create(value: receipt, redraw: receipt.Redraw, resources: resources),
            UiBound: true);

    public static CameraOp<CameraChangeReceipt> Section(params CameraSection[] sections) =>
        new(Run: scope =>
            from admitted in Optional(sections).ToFin(Fail: Op.Of(name: nameof(Section)).InvalidInput()).Map(static source => toSeq(source))
            from _any in guard(!admitted.IsEmpty, Op.Of(name: nameof(Section)).InvalidInput())
            from resources in admitted.TraverseM(section => section.Apply(scope: scope, op: Op.Of(name: nameof(Section)))).As()
            let receipt = CameraChangeReceipt.Of(resources: resources, redraw: CameraScope.RedrawFor(scope: scope))
            select CameraOutcome<CameraChangeReceipt>.Create(value: receipt, redraw: receipt.Redraw, resources: resources),
            UiBound: true);

    public static CameraOp<Commands.DocumentResourceChange> SaveNamed(string name, bool fullView = true, Option<CameraDof> dof = default) =>
        new(Run: scope =>
            from valid in Name(value: name, op: Op.Of(name: nameof(SaveNamed)))
            from _ in CommitNamed(scope: scope, name: valid, full: fullView, dof: dof, op: Op.Of(name: nameof(SaveNamed)))
            select NamedResource(name: valid),
            UiBound: true);

    public static CameraOp<CameraFrame> ReadNamed(string name) =>
        new(Run: scope =>
            from index in NamedIndex(document: scope.Document, name: name, op: Op.Of(name: nameof(ReadNamed)))
            from frame in Op.Of(name: nameof(ReadNamed)).Catch(() => {
                using ViewInfo view = scope.Document.NamedViews[index];
                using ViewportInfo projection = new(view.Viewport);
                Plane plane = new(origin: projection.CameraLocation, xDirection: Vector3d.CrossProduct(projection.CameraDirection, projection.CameraUp), yDirection: projection.CameraUp);
                return plane.IsValid
                    ? Fin.Succ(value: new CameraFrame(Frame: plane, Target: projection.TargetPoint))
                    : Fin.Fail<CameraFrame>(error: Op.Of(name: nameof(ReadNamed)).InvalidResult());
            })
            select CameraOutcome<CameraFrame>.Create(value: frame),
            UiBound: false);

    public static CameraOp<Unit> RestoreNamed(string name, CameraPath? path = null, NamedRestorePolicy? restore = null) =>
        new(Run: scope =>
            from index in NamedIndex(document: scope.Document, name: name, op: Op.Of(name: nameof(RestoreNamed)))
            from redraw in (restore ?? NamedRestorePolicy.Default).Use(run: () => (path ?? new CameraPath.Instant()).Restore(scope: scope, index: index))
            select CameraOutcome<Unit>.Create(value: unit, redraw: redraw));

    public static CameraOp<TCapture> RestoreThen<TCapture>(string name, CameraOp<TCapture> capture, CameraPath? path = null, NamedRestorePolicy? restore = null) =>
        new(Run: scope =>
            from validCapture in Optional(capture).ToFin(Fail: Op.Of(name: nameof(RestoreThen)).InvalidInput())
            from snapshot in scope.Snapshot()
            from captured in Op.Of(name: nameof(RestoreThen)).Catch(() => {
                using (snapshot) {
                    Fin<CameraOutcome<TCapture>> run =
                        from restored in RestoreNamed(name: name, path: path, restore: restore).Run(arg: scope)
                        from _commit in restored.Redraw.ApplyTo(scope: scope)
                        from result in validCapture.Run(arg: scope)
                        select result with { Redraw = restored.Redraw | result.Redraw, Resources = restored.Resources + result.Resources };
                    return run.BiBind(
                        Succ: result => snapshot.Restore().Map(reset => result with { Redraw = result.Redraw | reset }),
                        Fail: error => snapshot.Restore().BiBind(
                            Succ: _ => Fin.Fail<CameraOutcome<TCapture>>(error: error),
                            Fail: reset => Fin.Fail<CameraOutcome<TCapture>>(error: reset)));
                }
            })
            select captured);

    public static CameraOp<Commands.DocumentResourceChange> RenameNamed(string current, string next) =>
        new(Run: scope =>
            from index in NamedIndex(document: scope.Document, name: current, op: Op.Of(name: nameof(RenameNamed)))
            from name in Name(value: next, op: Op.Of(name: nameof(RenameNamed)))
            from _ in Op.Of(name: nameof(RenameNamed)).Confirm(success: scope.Document.NamedViews.Rename(index: index, newName: name))
            select NamedResource(name: name));

    public static CameraOp<Commands.DocumentResourceChange> DeleteNamed(string name) =>
        new(Run: scope =>
            from index in NamedIndex(document: scope.Document, name: name, op: Op.Of(name: nameof(DeleteNamed)))
            from valid in Name(value: name, op: Op.Of(name: nameof(DeleteNamed)))
            from _ in Op.Of(name: nameof(DeleteNamed)).Confirm(success: scope.Document.NamedViews.Delete(index: index))
            select NamedResource(name: valid));

    private static CameraOp<DrawingBitmap> CaptureBitmap(CaptureRecipe recipe = default) =>
        Capture(recipe: recipe, name: nameof(CaptureBitmap), render: static settings => ViewCapture.CaptureToBitmap(settings: settings));

    private static CameraOp<XmlDocument> CaptureSvg(CaptureRecipe recipe = default) =>
        Capture(recipe: recipe, name: nameof(CaptureSvg), render: static settings => ViewCapture.CaptureToSvg(settings: settings));

    public static CameraOp<CaptureResult> CaptureFrame(CaptureFormat format, CaptureRecipe recipe = default) =>
        format switch {
            CaptureFormat.Bitmap => CaptureBitmap(recipe: recipe).Map(value => (CaptureResult)new CaptureResult.Bitmap(Value: value)),
            CaptureFormat.Svg => CaptureSvg(recipe: recipe).Map(value => (CaptureResult)new CaptureResult.Svg(Value: value)),
            _ => new CameraOp<CaptureResult>(Run: _ => Fin.Fail<CameraOutcome<CaptureResult>>(error: Op.Of(name: nameof(CaptureFrame)).InvalidInput())),
        };

    private static CameraOp<T> Capture<T>(CaptureRecipe recipe, string name, Func<ViewCaptureSettings, T?> render) where T : class =>
        new(Run: scope => recipe.WithPolicy(
                fallbackDpi: CaptureRecipe.ScreenDpi,
                fallbackDecor: CaptureRecipe.ScreenDecor,
                rewrite: static (decor, _) => decor).Render(
                view: scope.View,
                viewport: Some(value: scope.Viewport),
                project: settings => Optional(render(arg: settings)).ToFin(Fail: Op.Of(name: name).InvalidResult()),
                op: Op.Of(name: name))
            .Map(value => CameraOutcome<T>.Create(value: value, redraw: RedrawRequest.Empty)));

    private static Fin<int> NamedIndex(RhinoDoc document, string name, Op op) =>
        from valid in Name(value: name, op: op)
        from index in document.NamedViews.FindByName(name: valid) switch {
            int value when value >= 0 => Fin.Succ(value: value),
            _ => Fin.Fail<int>(error: op.MissingContext()),
        }
        select index;

    private static Fin<string> Name(string value, Op op) =>
        op.AcceptText(value: value).MapFail(_ => op.InvalidInput());

    private static Fin<Unit> CommitNamed(CameraScope scope, string name, bool full, Option<CameraDof> dof, Op op) =>
        full switch {
            true => op.Catch(() => {
                using ViewInfo view = new(scope.Viewport);
                view.Name = name;
                _ = dof.Iter(value => value.Write(view: view));
                return op.Confirm(success: scope.Document.NamedViews.Add(view: view) >= 0);
            }),
            false => op.Confirm(success: scope.Document.NamedViews.Add(name: name, viewportId: scope.Viewport.Id) >= 0),
        };

    private static CameraOutcome<Commands.DocumentResourceChange> NamedResource(string name) {
        Commands.DocumentResourceChange change = new(Kind: Commands.DocumentResourceKind.NamedView, Name: name);
        return CameraOutcome<Commands.DocumentResourceChange>.WithResource(value: change, change: change);
    }
}
