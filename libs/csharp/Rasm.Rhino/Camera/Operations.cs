using System.Runtime.InteropServices;
using Rasm.Vectors;
using DrawingBitmap = System.Drawing.Bitmap;
using DrawingPoint = System.Drawing.Point;
using DrawingRectangle = System.Drawing.Rectangle;
using DrawingSize = System.Drawing.Size;
using XmlDocument = System.Xml.XmlDocument;

namespace Rasm.Rhino.Camera;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class CameraMouseMove {
    public static readonly CameraMouseMove RotateAroundTarget = new(key: 0, apply: static (vp, prev, curr) => vp.MouseRotateAroundTarget(mousePreviousPoint: prev, mouseCurrentPoint: curr));
    public static readonly CameraMouseMove RotateCamera = new(key: 1, apply: static (vp, prev, curr) => vp.MouseRotateCamera(mousePreviousPoint: prev, mouseCurrentPoint: curr));
    public static readonly CameraMouseMove InOutDolly = new(key: 2, apply: static (vp, prev, curr) => vp.MouseInOutDolly(mousePreviousPoint: prev, mouseCurrentPoint: curr));
    public static readonly CameraMouseMove Magnify = new(key: 3, apply: static (vp, prev, curr) => vp.MouseMagnify(mousePreviousPoint: prev, mouseCurrentPoint: curr));
    public static readonly CameraMouseMove Tilt = new(key: 4, apply: static (vp, prev, curr) => vp.MouseTilt(mousePreviousPoint: prev, mouseCurrentPoint: curr));
    public static readonly CameraMouseMove DollyZoom = new(key: 5, apply: static (vp, prev, curr) => vp.MouseDollyZoom(mousePreviousPoint: prev, mouseCurrentPoint: curr));
    public static readonly CameraMouseMove LateralDolly = new(key: 6, apply: static (vp, prev, curr) => vp.MouseLateralDolly(mousePreviousPoint: prev, mouseCurrentPoint: curr));

    private readonly Func<RhinoViewport, DrawingPoint, DrawingPoint, bool> apply;
    internal bool Apply(RhinoViewport viewport, DrawingPoint previous, DrawingPoint current) => apply(arg1: viewport, arg2: previous, arg3: current);
}

[SmartEnum<int>]
public sealed partial class CameraStackOp {
    public static readonly CameraStackOp ViewPush = new(key: 0, apply: static (vp, _) => Op.Side(vp.PushViewProjection) switch { _ => true });
    public static readonly CameraStackOp ViewPop = new(key: 1, apply: static (vp, _) => vp.PopViewProjection());
    public static readonly CameraStackOp ViewNext = new(key: 2, apply: static (vp, _) => {
        bool ignored = vp.NextViewProjection();
        return true;
    });
    public static readonly CameraStackOp ViewPrevious = new(key: 3, apply: static (vp, _) => {
        bool ignored = vp.PreviousViewProjection();
        return true;
    });
    public static readonly CameraStackOp CPlanePush = new(key: 4, apply: static (vp, plane) => plane.Match(
        Some: cplane => Op.Side(() => vp.PushConstructionPlane(cplane: cplane)) switch { _ => true },
        None: () => Op.Side(() => vp.PushConstructionPlane(cplane: vp.GetConstructionPlane())) switch { _ => true }));
    public static readonly CameraStackOp CPlanePop = new(key: 5, apply: static (vp, _) => vp.PopConstructionPlane());
    public static readonly CameraStackOp CPlaneNext = new(key: 6, apply: static (vp, _) => vp.NextConstructionPlane());
    public static readonly CameraStackOp CPlanePrevious = new(key: 7, apply: static (vp, _) => vp.PreviousConstructionPlane());

    private readonly Func<RhinoViewport, Option<ConstructionPlane>, bool> apply;
    internal bool Apply(RhinoViewport viewport, Option<ConstructionPlane> plane) => apply(arg1: viewport, arg2: plane);
}

[SmartEnum<int>]
public sealed partial class CameraKeyboardMove {
    public static readonly CameraKeyboardMove RotateInPlace = new(key: 0, apply: static (vp, leftRight, amount) => vp.KeyboardRotate(leftRight: leftRight, angleRadians: amount));
    public static readonly CameraKeyboardMove Dolly = new(key: 1, apply: static (vp, leftRight, amount) => vp.KeyboardDolly(leftRight: leftRight, amount: amount));
    public static readonly CameraKeyboardMove DollyInOut = new(key: 2, apply: static (vp, _, amount) => vp.KeyboardDollyInOut(amount: amount));

    private readonly Func<RhinoViewport, bool, double, bool> apply;
    internal bool Apply(RhinoViewport viewport, bool leftRight, double amount) => apply(arg1: viewport, arg2: leftRight, arg3: amount);
}

// --- [MODELS] -----------------------------------------------------------------------------
public readonly record struct CameraChangeReceipt(
    Seq<Commands.DocumentResourceChange> Resources,
    RedrawRequest Redraw) {
    public static CameraChangeReceipt Empty => new(Resources: Seq<Commands.DocumentResourceChange>(), Redraw: RedrawRequest.Empty);
    public bool RedrawRequested => Redraw != RedrawRequest.Empty;
}

public sealed record CameraOp<T>(Func<CameraScope, Fin<CameraOutcome<T>>> Run, bool UiBound = true) {
    internal bool RequiresUiThread() => UiBound;

    public CameraOp<TNext> Map<TNext>(Func<T, TNext> project) =>
        new(Run: scope => Run(arg: scope)
                .Map(outcome => CameraOutcomeCreate.Value(
                    value: project(arg: outcome.Value),
                    redraw: outcome.Redraw,
                    resources: outcome.Resources)),
            UiBound: UiBound);

    public CameraOp<TNext> Bind<TNext>(Func<T, CameraOp<TNext>> bind) =>
        new(Run: scope => Run(arg: scope).Bind(outcome => bind(arg: outcome.Value).Run(arg: scope).Map(next => next with {
            Redraw = outcome.Redraw | next.Redraw,
            Resources = outcome.Resources + next.Resources,
        })), UiBound: UiBound);
}

[Union(SwitchMapStateParameterName = "viewport")]
public abstract partial record CameraProjection {
    private CameraProjection() { }

    public sealed record Parallel(bool SymmetricFrustum = true) : CameraProjection;
    public sealed record Perspective(Option<double> TargetDistance = default, bool SymmetricFrustum = true, double LensLength = 50.0) : CameraProjection;
    public sealed record TwoPointPerspective(double LensLength = 50.0, Option<(Vector3d Up, double TargetDistance)> Target = default) : CameraProjection;
    public sealed record ParallelReflected : CameraProjection;

    internal Fin<Unit> Use(RhinoViewport viewport, Op op) =>
        Optional(viewport).ToFin(Fail: op.InvalidInput()).Bind(active => Switch(
            (Viewport: active, Op: op),
            parallel: static (ctx, p) => ctx.Op.Confirm(success: ctx.Viewport.ChangeToParallelProjection(symmetricFrustum: p.SymmetricFrustum)),
            perspective: static (ctx, p) => ctx.Op.Confirm(success: p.TargetDistance.Case switch {
                double distance => ctx.Viewport.ChangeToPerspectiveProjection(targetDistance: distance, symmetricFrustum: p.SymmetricFrustum, lensLength: p.LensLength),
                _ => ctx.Viewport.ChangeToPerspectiveProjection(symmetricFrustum: p.SymmetricFrustum, lensLength: p.LensLength),
            }),
            twoPointPerspective: static (ctx, p) => ctx.Op.Confirm(success: p.Target.Case switch {
                (Vector3d up, double distance) => ctx.Viewport.ChangeToTwoPointPerspectiveProjection(lensLength: p.LensLength, up: up, targetDistance: distance),
                _ => ctx.Viewport.ChangeToTwoPointPerspectiveProjection(lensLength: p.LensLength),
            }),
            parallelReflected: static (ctx, _) => ctx.Op.Confirm(success: ctx.Viewport.ChangeToParallelReflectedProjection())));
}

[Union(SwitchMapStateParameterName = "context")]
public abstract partial record NamedSave {
    private NamedSave() { }

    public sealed record ViewportId : NamedSave;
    public sealed record Full : NamedSave;

    internal static Fin<Unit> Commit(NamedSave source, CameraScope scope, string name, Op op) =>
        source.Switch(
            (Scope: scope, Name: name, Op: op),
            viewportId: static (ctx, _) => ctx.Op.Confirm(success: ctx.Scope.Document.NamedViews.Add(name: ctx.Name, viewportId: ctx.Scope.Viewport.Id) >= 0),
            full: static (ctx, _) => {
                using ViewInfo view = new(ctx.Scope.Viewport);
                view.Name = ctx.Name;
                return ctx.Op.Confirm(success: ctx.Scope.Document.NamedViews.Add(view: view) >= 0);
            });
}

public readonly record struct NamedRestorePolicy(bool Clipping = true, bool Display = true) {
    internal void ApplyBeforeRestore() {
        NamedRestorePolicy policy = this;
        Action apply = policy switch {
            (Clipping: true, Display: true) => static () => {
                global::Rhino.ApplicationSettings.ViewSettings.DefinedViewSetClippingPlanes = true;
                global::Rhino.ApplicationSettings.ViewSettings.DefinedViewSetDisplayMode = true;
            }
            ,
            (Clipping: true, Display: false) => static () =>
                global::Rhino.ApplicationSettings.ViewSettings.DefinedViewSetClippingPlanes = true,
            (Clipping: false, Display: true) => static () =>
                global::Rhino.ApplicationSettings.ViewSettings.DefinedViewSetDisplayMode = true,
            _ => static () => { }
            ,
        };
        apply();
    }
}

[Union(SwitchMapStateParameterName = "context")]
public abstract partial record CameraPath {
    private CameraPath() { }

    public sealed record Instant : CameraPath;
    public sealed record MatchAspect : CameraPath;
    public sealed record ConstantSpeed(double UnitsPerFrame, int DelayMilliseconds) : CameraPath;
    public sealed record ConstantTime(int Frames, int DelayMilliseconds) : CameraPath;

    internal Fin<RedrawRequest> Restore(CameraScope scope, int index) =>
        Switch(
            (Scope: scope, Index: index),
            instant: static (ctx, _) => Op.Of(name: nameof(Restore)).Confirm(success: ctx.Scope.Document.NamedViews.Restore(index: ctx.Index, viewport: ctx.Scope.Viewport))
                .Map(_ => CameraScope.RedrawFor(scope: ctx.Scope)),
            matchAspect: static (ctx, _) => Op.Of(name: nameof(Restore)).Confirm(success: ctx.Scope.Document.NamedViews.RestoreWithAspectRatio(index: ctx.Index, viewport: ctx.Scope.Viewport))
                .Map(_ => CameraScope.RedrawFor(scope: ctx.Scope)),
            constantSpeed: static (ctx, path) => Op.Of(name: nameof(Restore)).Confirm(success: ctx.Scope.Document.NamedViews.RestoreAnimatedConstantSpeed(ctx.Index, ctx.Scope.Viewport, path.UnitsPerFrame, path.DelayMilliseconds))
                .Map(_ => (RedrawRequest)new RedrawRequest.Deferred()),
            constantTime: static (ctx, path) => Op.Of(name: nameof(Restore)).Confirm(success: ctx.Scope.Document.NamedViews.RestoreAnimatedConstantTime(ctx.Index, ctx.Scope.Viewport, path.Frames, path.DelayMilliseconds))
                .Map(_ => (RedrawRequest)new RedrawRequest.Deferred()));
}

[Union(SwitchMapStateParameterName = "scope")]
public abstract partial record CameraEdit {
    private CameraEdit() { }

    public sealed record Native(Func<RhinoViewport, bool> Run) : CameraEdit;
    public sealed record Frame(CameraFrame Value) : CameraEdit;
    public sealed record NavigateLookAt(Point3d From, Point3d At, Option<Vector3d> UpAxis = default) : CameraEdit;
    public sealed record SubjectFrame(CameraSubject Subject, double Padding = 1.1) : CameraEdit;
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

    internal Fin<RedrawRequest> Apply(CameraScope scope) => CameraEditKernel.Apply(edit: this, scope: scope);
}

file static class CameraEditKernel {
    private static readonly Op NativeKey = Op.Of(name: "CameraEdit.Native");
    private static readonly Op FrustumKey = Op.Of(name: "CameraEdit.Frustum");
    private static readonly Op PlanKey = Op.Of(name: "CameraEdit.Plan");
    private static readonly Op TransformKey = Op.Of(name: "CameraEdit.Transform");
    private static readonly Op ZoomBoxKey = Op.Of(name: "CameraEdit.ZoomBox");
    private static readonly Op RotateKey = Op.Of(name: "CameraEdit.Rotate");
    private static readonly Op SubjectFrameKey = Op.Of(name: "CameraEdit.SubjectFrame");
    private static readonly Op ApplyProjectionKey = Op.Of(name: "CameraEdit.ApplyProjection");

    internal static Fin<RedrawRequest> Apply(CameraEdit edit, CameraScope scope) =>
        edit.Switch(
            scope,
            native: static (ctx, value) => from valid in Optional(value.Run).ToFin(Fail: NativeKey.InvalidInput())
                                           from _ in NativeKey.Confirm(success: valid(arg: ctx.Viewport))
                                           select CameraScope.RedrawFor(scope: ctx),
            frame: static (ctx, value) => value.Value.Apply(viewport: ctx.Viewport, op: Op.Of(name: nameof(CameraEdit.Frame)))
                .Map(_ => CameraScope.RedrawFor(scope: ctx)),
            navigateLookAt: static (ctx, value) => from frame in CameraFrame.LookAt(location: value.From, target: value.At, up: value.UpAxis)
                                                   from _ in frame.Apply(viewport: ctx.Viewport, op: Op.Of(name: nameof(CameraEdit.NavigateLookAt)))
                                                   select CameraScope.RedrawFor(scope: ctx),
            subjectFrame: static (ctx, value) => from bounds in value.Subject.BoundsOf(op: SubjectFrameKey)
                                                 let box = bounds.IsValid
                                                     ? new BoundingBox(
                                                         min: bounds.Min - new Vector3d((bounds.Max.X - bounds.Min.X) * (value.Padding - 1.0) * 0.5, (bounds.Max.Y - bounds.Min.Y) * (value.Padding - 1.0) * 0.5, (bounds.Max.Z - bounds.Min.Z) * (value.Padding - 1.0) * 0.5),
                                                         max: bounds.Max + new Vector3d((bounds.Max.X - bounds.Min.X) * (value.Padding - 1.0) * 0.5, (bounds.Max.Y - bounds.Min.Y) * (value.Padding - 1.0) * 0.5, (bounds.Max.Z - bounds.Min.Z) * (value.Padding - 1.0) * 0.5))
                                                     : BoundingBox.Empty
                                                 from _ in guard(box.IsValid, SubjectFrameKey.InvalidInput())
                                                 from __ in SubjectFrameKey.Confirm(success: ctx.Viewport.ZoomBoundingBox(box: box))
                                                 select CameraScope.RedrawFor(scope: ctx),
            location: static (ctx, value) => Side(scope: ctx, apply: vp => vp.SetCameraLocation(cameraLocation: value.Value, updateTargetLocation: value.UpdateTarget)),
            target: static (ctx, value) => Side(scope: ctx, apply: vp => vp.SetCameraTarget(targetLocation: value.Value, updateCameraLocation: value.UpdateLocation)),
            direction: static (ctx, value) => from context in Context.Of(doc: ctx.Document).ToFin()
                                              from direction in VectorIntent.Direction(value: value.Value).Project<Vector3d>(context: context, key: Op.Of(name: nameof(CameraEdit.Direction)))
                                              from redraw in Side(scope: ctx, apply: vp => vp.SetCameraDirection(cameraDirection: direction, updateTargetLocation: value.UpdateTarget))
                                              select redraw,
            up: static (ctx, value) => from context in Context.Of(doc: ctx.Document).ToFin()
                                       from up in VectorIntent.Direction(value: value.Value).Project<Vector3d>(context: context, key: Op.Of(name: nameof(CameraEdit.Up)))
                                       from redraw in Side(scope: ctx, apply: vp => vp.CameraUp = up)
                                       select redraw,
            lens: static (ctx, value) => from _ in guard(value.Millimeters > 0, Op.Of(name: nameof(CameraEdit.Lens)).InvalidInput())
                                         from redraw in Side(scope: ctx, apply: vp => vp.Camera35mmLensLength = value.Millimeters)
                                         select redraw,
            angle: static (ctx, value) => from _ in guard(value.Radians > 0, Op.Of(name: nameof(CameraEdit.Angle)).InvalidInput())
                                          from redraw in Side(scope: ctx, apply: vp => vp.CameraAngle = value.Radians)
                                          select redraw,
            defined: static (ctx, value) => Result(scope: ctx, apply: vp => vp.SetProjection(projection: value.Projection, viewName: value.Name, updateConstructionPlane: value.UpdateConstructionPlane), Op.Of(name: nameof(CameraEdit.Defined))),
            snapshot: static (ctx, value) => ApplyProjection(scope: ctx, apply: (_, _) => Fin.Succ(value: value.Projection), updateTarget: value.UpdateTarget),
            project: static (ctx, value) => from _ in value.Projection.Use(viewport: ctx.Viewport, op: Op.Of(name: nameof(CameraEdit.Project)))
                                            select CameraScope.RedrawFor(scope: ctx),
            frustum: static (ctx, value) => ApplyProjection(scope: ctx, apply: (_, projection) => value.Value.Apply(projection: projection, op: FrustumKey), updateTarget: value.UpdateTarget),
            isometric: static (ctx, value) => Result(scope: ctx, apply: vp => vp.SetProjection(projection: value.Camera, viewName: value.Name, updateConstructionPlane: value.UpdateConstructionPlane), Op.Of(name: nameof(CameraEdit.Isometric))),
            planePlain: static (ctx, value) => from _ in guard(value.Value.IsValid, Op.Of(name: nameof(CameraEdit.PlanePlain)).InvalidInput())
                                               from redraw in Side(scope: ctx, apply: vp => vp.SetConstructionPlane(plane: value.Value))
                                               select redraw,
            planeFull: static (ctx, value) => from _ in Optional(value.Value).ToFin(Fail: Op.Of(name: nameof(CameraEdit.PlaneFull)).InvalidInput())
                                              from redraw in Side(scope: ctx, apply: vp => vp.SetConstructionPlane(cplane: value.Value))
                                              select redraw,
            plan: static (ctx, value) => from _ in guard(value.Origin.IsValid && value.XDirection.IsValid && value.YDirection.IsValid
                    && value.XDirection.Length > RhinoMath.ZeroTolerance && value.YDirection.Length > RhinoMath.ZeroTolerance, PlanKey.InvalidInput())
                                         from done in Result(scope: ctx, apply: vp => vp.SetToPlanView(value.Origin, value.XDirection, value.YDirection, value.SetConstructionPlane), PlanKey)
                                         select done,
            displayMode: static (ctx, value) => from valid in Optional(value.Value).ToFin(Fail: Op.Of(name: nameof(CameraEdit.DisplayMode)).InvalidInput())
                                                from redraw in Side(scope: ctx, apply: vp => vp.DisplayMode = valid)
                                                select redraw,
            clipping: static (ctx, value) => from _ in guard(value.Box.IsValid, Op.Of(name: nameof(CameraEdit.Clipping)).InvalidInput())
                                             from redraw in Side(scope: ctx, apply: vp => vp.SetClippingPlanes(box: value.Box))
                                             select redraw,
            transform: static (ctx, value) => ApplyProjection(scope: ctx, apply: (_, projection) => TransformKey.Confirm(success: projection.TransformCamera(xform: value.Xform)).Map(_ => projection), updateTarget: true),
            zoom: static (ctx, _) => Result(scope: ctx, apply: static vp => vp.ZoomExtents(), Op.Of(name: nameof(CameraEdit.Zoom))),
            zoomSelected: static (ctx, _) => Result(scope: ctx, apply: static vp => vp.ZoomExtentsSelected(), Op.Of(name: nameof(CameraEdit.ZoomSelected))),
            zoomBox: static (ctx, value) => from _ in guard(value.Bounds.IsValid, ZoomBoxKey.InvalidInput())
                                            from done in Result(scope: ctx, apply: vp => vp.ZoomBoundingBox(box: value.Bounds), ZoomBoxKey)
                                            select done,
            zoomWindow: static (ctx, value) => Result(scope: ctx, apply: vp => vp.ZoomWindow(rect: value.Window), Op.Of(name: nameof(CameraEdit.ZoomWindow))),
            rotate: static (ctx, value) => from context in Context.Of(doc: ctx.Document).ToFin()
                                           from direction in VectorIntent.Direction(value: value.Axis).Project<Vector3d>(context: context, key: RotateKey)
                                           from done in Result(scope: ctx, apply: vp => vp.Rotate(angleRadians: value.Radians, rotationAxis: direction, rotationCenter: value.Center), RotateKey)
                                           select done,
            magnify: static (ctx, value) => Result(scope: ctx, apply: vp => value.FixedPoint.Case switch {
                DrawingPoint p => vp.Magnify(magnificationFactor: value.Factor, mode: value.LensMode, fixedScreenPoint: p),
                _ => vp.Magnify(magnificationFactor: value.Factor, mode: value.LensMode),
            }, Op.Of(name: nameof(CameraEdit.Magnify))),
            keyboard: static (ctx, value) => Result(scope: ctx, apply: vp => value.Move.Apply(viewport: vp, leftRight: value.LeftRight, amount: value.Amount), Op.Of(name: nameof(CameraEdit.Keyboard))),
            mouse: static (ctx, value) => Result(scope: ctx, apply: vp => value.Move.Apply(viewport: vp, previous: value.PreviousPoint, current: value.CurrentPoint), Op.Of(name: nameof(CameraEdit.Mouse))),
            mouseLens: static (ctx, value) => Result(scope: ctx, apply: vp => vp.MouseAdjustLensLength(mousePreviousPoint: value.PreviousPoint, mouseCurrentPoint: value.CurrentPoint, moveTarget: value.MoveTarget), Op.Of(name: nameof(CameraEdit.MouseLens))),
            stackMove: static (ctx, value) => Result(scope: ctx, apply: vp => value.Move.Apply(viewport: vp, plane: value.Plane), Op.Of(name: nameof(CameraEdit.StackMove))),
            pushView: static (ctx, value) => Result(scope: ctx, apply: vp => vp.PushViewInfo(value.Info, value.IncludeTraceImage), Op.Of(name: nameof(CameraEdit.PushView))));

    private static Fin<RedrawRequest> ApplyProjection(
        CameraScope scope,
        Func<RhinoViewport, ViewportInfo, Fin<ViewportInfo>> apply,
        bool updateTarget) =>
        UI.RhinoUi.Protect(valid: () => {
            using ViewportInfo projection = new(rhinoViewport: scope.Viewport);
            return from authored in apply(arg1: scope.Viewport, arg2: projection)
                   from _ in ApplyProjectionKey.Confirm(success: scope.Viewport.SetViewProjection(projection: authored, updateTargetLocation: updateTarget))
                   select CameraScope.RedrawFor(scope: scope);
        });

    private static Fin<RedrawRequest> Side(CameraScope scope, Action<RhinoViewport> apply) {
        apply(obj: scope.Viewport);
        return Fin.Succ(value: CameraScope.RedrawFor(scope: scope));
    }

    private static Fin<RedrawRequest> Result(CameraScope scope, Func<RhinoViewport, bool> apply, Op op) =>
        from _ in op.Confirm(success: apply(arg: scope.Viewport))
        select CameraScope.RedrawFor(scope: scope);
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
                pick.Depth switch {
                    0.0 => Optional(ctx.Viewport.ClientToWorld(clientPoint: new Point2d(pick.ClientX, pick.ClientY)))
                        .Filter(static line => line.IsValid)
                        .ToFin(Fail: ResolveKey.InvalidResult())
                        .Map(static line => line.From),
                    _ => ctx.FrustumLine(screenX: pick.ClientX, screenY: pick.ClientY)
                        .Map(line => line.PointAt(t: pick.Depth)),
                }),
            subject: static (ctx, pick) => pick.Value switch {
                CameraSubject.AtPoint source when source.Value.IsValid => Fin.Succ(value: source.Value),
                CameraSubject.InBounds source when source.Value.IsValid => Fin.Succ(value: source.Value.Center),
                CameraSubject.InSphere source when source.Value.IsValid => Fin.Succ(value: source.Value.Center),
                CameraSubject.FromGeometry source => source.Value.BoundsOf(op: ResolveKey).Map(bounds => bounds.Center),
                _ => Fin.Fail<Point3d>(error: ResolveKey.InvalidInput()),
            });
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct CameraCapture(
    DrawingSize Size,
    double Dpi = 96,
    CaptureDecor Decor = default) {
    private static readonly Op CaptureKey = Op.Of(name: nameof(Capture));

    internal Fin<T> Capture<T>(CameraScope scope, Func<ViewCaptureSettings, Fin<T>> project) {
        CameraCapture self = this;
        return from valid in Optional(project).ToFin(Fail: CaptureKey.InvalidInput())
               from _ in guard(self.Size is { Width: > 0, Height: > 0 } && RhinoMath.IsValidDouble(x: self.Dpi) && self.Dpi > 0.0, CaptureKey.InvalidInput())
               from result in UI.RhinoUi.Protect(valid: () => {
                   using ViewCaptureSettings settings = new(sourceView: scope.View, mediaSize: self.Size, dpi: self.Dpi);
                   settings.SetViewport(viewport: scope.Viewport);
                   return from configured in self.Decor.Apply(settings: settings, op: CaptureKey)
                          from __ in guard(settings.IsValid, CaptureKey.InvalidResult())
                          from projected in valid(arg: settings)
                          select projected;
               })
               select result;
    }
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class CameraOps {
    private static readonly Op SaveNamedKey = Op.Of(name: nameof(SaveNamed));
    private static readonly Op RestoreNamedKey = Op.Of(name: nameof(RestoreNamed));
    private static readonly Op RenameNamedKey = Op.Of(name: nameof(RenameNamed));
    private static readonly Op DeleteNamedKey = Op.Of(name: nameof(DeleteNamed));

    public static CameraOp<Seq<string>> ListNamed() =>
        new(
            Run: scope => Op.Of(name: nameof(ListNamed)).Catch(() =>
                Fin.Succ(value: CameraOutcomeCreate.Value(value: toSeq(scope.Document.NamedViews).Map(static view => view.Name)))),
            UiBound: false);

    public static CameraOp<T> Query<T>(Func<CameraScope, Fin<T>> query) =>
        new(
            Run: scope => Optional(query).ToFin(Fail: Op.Of(name: nameof(Query)).InvalidInput())
                .Bind(valid => valid(arg: scope).Map(value => CameraOutcomeCreate.Value(value: value))),
            UiBound: false);

    public static CameraOp<T> Read<T>(Func<CameraSnapshot, Fin<T>> project) =>
        new(
            Run: scope => Optional(project).ToFin(Fail: Op.Of(name: nameof(Read)).InvalidInput())
                .Bind(valid => scope.Snapshot().Bind(snapshot => {
                    using (snapshot) { return valid(arg: snapshot).Map(value => CameraOutcomeCreate.Value(value: value)); }
                })),
            UiBound: false);

    public static CameraOp<Point3d> Resolve(CameraPick pick) =>
        new(Run: scope => pick.Resolve(scope: scope).Map(point => CameraOutcomeCreate.Value(value: point)));

    public static CameraOp<CameraChangeReceipt> Change(params CameraEdit[] edits) =>
        Change(edits: edits, redrawEach: true);

    public static CameraOp<CameraChangeReceipt> Change(IEnumerable<CameraEdit> edits, bool redrawEach = true) =>
        new(Run: scope =>
            from changes in Optional(edits).ToFin(Fail: Op.Of(name: nameof(Change)).InvalidInput()).Map(static source => toSeq(source))
            from outcome in redrawEach switch {
                true => changes.TraverseM(edit => edit.Apply(scope: scope)).As()
                    .Map(redraws => redraws.Fold(RedrawRequest.Empty, (left, right) => left | right))
                    .Map(redraw => CameraOutcomeCreate.Value(
                        value: CameraChangeReceipt.Empty with { Redraw = redraw },
                        redraw: redraw)),
                false => UI.RhinoUi.Protect(valid: () => {
                    scope.Document.Views.EnableRedraw(enable: false, redrawDocument: false, redrawLayers: false);
                    try {
                        return changes.TraverseM(edit => edit.Apply(scope: scope)).As()
                            .Map(redraws => redraws.Fold(RedrawRequest.Empty, (left, right) => left | right))
                            .Map(batch => CameraOutcomeCreate.Value(
                                value: CameraChangeReceipt.Empty with { Redraw = batch | CameraScope.RedrawFor(scope: scope) },
                                redraw: batch | CameraScope.RedrawFor(scope: scope)));
                    } finally {
                        scope.Document.Views.EnableRedraw(enable: true, redrawDocument: false, redrawLayers: false);
                    }
                }),
            }
            select outcome,
            UiBound: true);

    public static CameraOp<Commands.DocumentResourceChange> SaveNamed(string name, NamedSave? save = null) =>
        new(Run: scope =>
            from valid in Name(value: name, op: SaveNamedKey)
            from source in Fin.Succ(value: save ?? new NamedSave.Full())
            from _ in NamedSave.Commit(source: source, scope: scope, name: valid, op: SaveNamedKey)
            select NamedResource(name: valid),
            UiBound: true);

    public static CameraOp<Unit> RestoreNamed(string name, CameraPath? path = null, NamedRestorePolicy restore = default) =>
        new(Run: scope =>
            from index in NamedIndex(document: scope.Document, name: name, op: RestoreNamedKey)
            from _ in Fin.Succ(value: Op.Side(() => restore.ApplyBeforeRestore()))
            from redraw in (path ?? new CameraPath.Instant()).Restore(scope: scope, index: index)
            select CameraOutcomeCreate.Value(value: unit, redraw: redraw));

    public static CameraOp<Commands.DocumentResourceChange> RenameNamed(string current, string next) =>
        new(Run: scope =>
            from index in NamedIndex(document: scope.Document, name: current, op: RenameNamedKey)
            from name in Name(value: next, op: RenameNamedKey)
            from _ in RenameNamedKey.Confirm(success: scope.Document.NamedViews.Rename(index: index, newName: name))
            select NamedResource(name: name));

    public static CameraOp<Commands.DocumentResourceChange> DeleteNamed(string name) =>
        new(Run: scope =>
            from index in NamedIndex(document: scope.Document, name: name, op: DeleteNamedKey)
            from valid in Name(value: name, op: DeleteNamedKey)
            from _ in DeleteNamedKey.Confirm(success: scope.Document.NamedViews.Delete(index: index))
            select NamedResource(name: valid));

    public static CameraOp<DrawingBitmap> CaptureBitmap(CameraCapture capture) =>
        new(Run: scope => capture.Capture(scope: scope, project: static settings => Optional(ViewCapture.CaptureToBitmap(settings: settings)).ToFin(Fail: Op.Of(name: nameof(CaptureBitmap)).InvalidResult()))
            .Map(bitmap => CameraOutcomeCreate.Value(value: bitmap, redraw: RedrawRequest.Empty)));

    public static CameraOp<XmlDocument> CaptureSvg(CameraCapture capture) =>
        new(Run: scope => capture.Capture(scope: scope, project: static settings => Optional(ViewCapture.CaptureToSvg(settings: settings)).ToFin(Fail: Op.Of(name: nameof(CaptureSvg)).InvalidResult()))
            .Map(document => CameraOutcomeCreate.Value(value: document, redraw: RedrawRequest.Empty)));

    private static Fin<int> NamedIndex(RhinoDoc document, string name, Op op) =>
        from valid in Name(value: name, op: op)
        from index in document.NamedViews.FindByName(name: valid) switch {
            int value when value >= 0 => Fin.Succ(value: value),
            _ => Fin.Fail<int>(error: op.MissingContext()),
        }
        select index;

    private static Fin<string> Name(string value, Op op) =>
        op.AcceptText(value: value).MapFail(_ => op.InvalidInput());

    private static CameraOutcome<Commands.DocumentResourceChange> NamedResource(string name) {
        Commands.DocumentResourceChange change = new(Kind: Commands.DocumentResourceKind.NamedView, Name: name);
        return CameraOutcomeCreate.Resource(value: change, change: change);
    }
}
