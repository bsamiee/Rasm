using Rasm.Vectors;
using DrawingBitmap = System.Drawing.Bitmap;
using DrawingPoint = System.Drawing.Point;
using DrawingRectangle = System.Drawing.Rectangle;
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

    [UseDelegateFromConstructor] internal partial bool Apply(RhinoViewport viewport, DrawingPoint previous, DrawingPoint current);
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
public sealed partial class CameraKeyboardMove {
    public static readonly CameraKeyboardMove RotateInPlace = new(key: 0, apply: static (vp, leftRight, amount) => vp.KeyboardRotate(leftRight: leftRight, angleRadians: amount));
    public static readonly CameraKeyboardMove Dolly = new(key: 1, apply: static (vp, leftRight, amount) => vp.KeyboardDolly(leftRight: leftRight, amount: amount));
    public static readonly CameraKeyboardMove DollyInOut = new(key: 2, apply: static (vp, _, amount) => vp.KeyboardDollyInOut(amount: amount));

    [UseDelegateFromConstructor] internal partial bool Apply(RhinoViewport viewport, bool leftRight, double amount);
}

[SmartEnum<int>]
public sealed partial class FramePaddingMode {
    public static readonly FramePaddingMode None = new(0), Symmetric = new(1), Additive = new(2);
}

// --- [MODELS] -----------------------------------------------------------------------------
public readonly record struct CameraChangeReceipt(
    Seq<Commands.DocumentResourceChange> Resources,
    RedrawRequest Redraw) {
    public static CameraChangeReceipt Empty => new(Resources: Seq<Commands.DocumentResourceChange>(), Redraw: RedrawRequest.Empty);
    public bool RedrawRequested => Redraw != RedrawRequest.Empty;
}

public sealed record CameraOp<T>(Func<CameraScope, Fin<CameraOutcome<T>>> Run, bool UiBound = true) {
    public CameraOp<TNext> Map<TNext>(Func<T, TNext> project) =>
        new(Run: scope => Run(arg: scope)
                .Map(outcome => CameraOutcomeCreate.Value(
                    value: project(arg: outcome.Value),
                    redraw: outcome.Redraw,
                    resources: outcome.Resources)),
            UiBound: UiBound);

    // `UiBound: true` is conservative: composed Bind chains can route a non-UI outer into a UI-bound inner.
    // Without this, RhinoCamera.Run skips UI dispatch and the inner mutates viewport off-thread.
    public CameraOp<TNext> Bind<TNext>(Func<T, CameraOp<TNext>> bind) =>
        new(Run: scope => Run(arg: scope).Bind(outcome => bind(arg: outcome.Value).Run(arg: scope).Map(next => next with {
            Redraw = outcome.Redraw | next.Redraw,
            Resources = outcome.Resources + next.Resources,
        })), UiBound: true);

    // Recovery combinators on the Run-returned Fin: Catch reroutes a failed run through a fallback op on the same
    // scope; MapFail rewrites the error without touching the success rail.
    public CameraOp<T> Catch(Func<Error, CameraOp<T>> handle) =>
        new(Run: scope => Run(arg: scope).BindFail(error => handle(arg: error).Run(arg: scope)), UiBound: true);

    public CameraOp<T> MapFail(Func<Error, Error> map) =>
        new(Run: scope => Run(arg: scope).MapFail(map), UiBound: UiBound);
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
                _ = self.ApplyBeforeRestore();
                return valid();
            } finally {
                global::Rhino.ApplicationSettings.ViewSettings.DefinedViewSetCPlane = cplane;
                global::Rhino.ApplicationSettings.ViewSettings.DefinedViewSetProjection = projection;
                global::Rhino.ApplicationSettings.ViewSettings.DefinedViewSetClippingPlanes = clipping;
                global::Rhino.ApplicationSettings.ViewSettings.DefinedViewSetDisplayMode = display;
            }
        }));
    }

    internal Fin<Unit> ApplyBeforeRestore() {
        NamedRestorePolicy self = this;
        return Fin.Succ(value: Op.Side(() => {
            global::Rhino.ApplicationSettings.ViewSettings.DefinedViewSetCPlane = self.CPlane;
            global::Rhino.ApplicationSettings.ViewSettings.DefinedViewSetProjection = self.Projection;
            global::Rhino.ApplicationSettings.ViewSettings.DefinedViewSetClippingPlanes = self.Clipping;
            global::Rhino.ApplicationSettings.ViewSettings.DefinedViewSetDisplayMode = self.Display;
        }));
    }
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
public abstract partial record CameraEdit {
    private CameraEdit() { }

    public sealed record Native(Func<RhinoViewport, bool> Run) : CameraEdit;
    public sealed record Frame(CameraFrame Value) : CameraEdit;
    public sealed record NavigateLookAt(Point3d From, Point3d At, Option<Vector3d> UpAxis = default) : CameraEdit;
    public sealed record SubjectFrame(CameraSubject Subject, FramePaddingMode Mode, double Amount = 1.1) : CameraEdit;
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
    internal static Fin<RedrawRequest> Apply(CameraEdit edit, CameraScope scope) =>
        edit.Switch(
            scope,
            native: static (ctx, value) => from valid in Optional(value.Run).ToFin(Fail: Op.Of(name: nameof(CameraEdit.Native)).InvalidInput())
                                           from _ in Op.Of(name: nameof(CameraEdit.Native)).Confirm(success: valid(arg: ctx.Viewport))
                                           select CameraScope.RedrawFor(scope: ctx),
            frame: static (ctx, value) => value.Value.Apply(viewport: ctx.Viewport, op: Op.Of(name: nameof(CameraEdit.Frame)))
                .Map(_ => CameraScope.RedrawFor(scope: ctx)),
            navigateLookAt: static (ctx, value) => from frame in CameraFrame.LookAt(location: value.From, target: value.At, up: value.UpAxis)
                                                   from _ in frame.Apply(viewport: ctx.Viewport, op: Op.Of(name: nameof(CameraEdit.NavigateLookAt)))
                                                   select CameraScope.RedrawFor(scope: ctx),
            subjectFrame: static (ctx, value) => from bounds in value.Subject.BoundsOf(op: Op.Of(name: nameof(CameraEdit.SubjectFrame)))
                                                 let box = InflateBox(source: bounds, mode: value.Mode, amount: value.Amount)
                                                 from _ in guard(box.IsValid, Op.Of(name: nameof(CameraEdit.SubjectFrame)).InvalidInput())
                                                 from __ in Op.Of(name: nameof(CameraEdit.SubjectFrame)).Confirm(success: ctx.Viewport.ZoomBoundingBox(box: box))
                                                 select CameraScope.RedrawFor(scope: ctx),
            location: static (ctx, value) => Side(scope: ctx, apply: vp => vp.SetCameraLocation(cameraLocation: value.Value, updateTargetLocation: value.UpdateTarget)),
            target: static (ctx, value) => Side(scope: ctx, apply: vp => vp.SetCameraTarget(targetLocation: value.Value, updateCameraLocation: value.UpdateLocation)),
            direction: static (ctx, value) => WithDirection(scope: ctx, value: value.Value, op: Op.Of(name: nameof(CameraEdit.Direction)),
                apply: dir => Side(scope: ctx, apply: vp => vp.SetCameraDirection(cameraDirection: dir, updateTargetLocation: value.UpdateTarget))),
            up: static (ctx, value) => WithDirection(scope: ctx, value: value.Value, op: Op.Of(name: nameof(CameraEdit.Up)),
                apply: dir => Side(scope: ctx, apply: vp => vp.CameraUp = dir)),
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
            frustum: static (ctx, value) => ApplyProjection(scope: ctx, apply: (_, projection) => value.Value.Apply(projection: projection, op: Op.Of(name: nameof(CameraEdit.Frustum))), updateTarget: value.UpdateTarget),
            isometric: static (ctx, value) => Result(scope: ctx, apply: vp => vp.SetProjection(projection: value.Camera, viewName: value.Name, updateConstructionPlane: value.UpdateConstructionPlane), Op.Of(name: nameof(CameraEdit.Isometric))),
            planePlain: static (ctx, value) => from _ in guard(value.Value.IsValid, Op.Of(name: nameof(CameraEdit.PlanePlain)).InvalidInput())
                                               from redraw in Side(scope: ctx, apply: vp => vp.SetConstructionPlane(plane: value.Value))
                                               select redraw,
            planeFull: static (ctx, value) => from _ in Optional(value.Value).ToFin(Fail: Op.Of(name: nameof(CameraEdit.PlaneFull)).InvalidInput())
                                              from redraw in Side(scope: ctx, apply: vp => vp.SetConstructionPlane(cplane: value.Value))
                                              select redraw,
            plan: static (ctx, value) => from _ in guard(value.Origin.IsValid && value.XDirection.IsValid && value.YDirection.IsValid
                    && value.XDirection.Length > RhinoMath.ZeroTolerance && value.YDirection.Length > RhinoMath.ZeroTolerance, Op.Of(name: nameof(CameraEdit.Plan)).InvalidInput())
                                         from done in Result(scope: ctx, apply: vp => vp.SetToPlanView(value.Origin, value.XDirection, value.YDirection, value.SetConstructionPlane), Op.Of(name: nameof(CameraEdit.Plan)))
                                         select done,
            displayMode: static (ctx, value) => from valid in Optional(value.Value).ToFin(Fail: Op.Of(name: nameof(CameraEdit.DisplayMode)).InvalidInput())
                                                from redraw in Side(scope: ctx, apply: vp => vp.DisplayMode = valid)
                                                select redraw,
            clipping: static (ctx, value) => from _ in guard(value.Box.IsValid, Op.Of(name: nameof(CameraEdit.Clipping)).InvalidInput())
                                             from redraw in Side(scope: ctx, apply: vp => vp.SetClippingPlanes(box: value.Box))
                                             select redraw,
            transform: static (ctx, value) => ApplyProjection(scope: ctx, apply: (_, projection) => Op.Of(name: nameof(CameraEdit.Transform)).Confirm(success: projection.TransformCamera(xform: value.Xform)).Map(_ => projection), updateTarget: true),
            zoom: static (ctx, _) => Result(scope: ctx, apply: static vp => vp.ZoomExtents(), Op.Of(name: nameof(CameraEdit.Zoom))),
            zoomSelected: static (ctx, _) => Result(scope: ctx, apply: static vp => vp.ZoomExtentsSelected(), Op.Of(name: nameof(CameraEdit.ZoomSelected))),
            zoomBox: static (ctx, value) => from _ in guard(value.Bounds.IsValid, Op.Of(name: nameof(CameraEdit.ZoomBox)).InvalidInput())
                                            from done in Result(scope: ctx, apply: vp => vp.ZoomBoundingBox(box: value.Bounds), Op.Of(name: nameof(CameraEdit.ZoomBox)))
                                            select done,
            zoomWindow: static (ctx, value) => Result(scope: ctx, apply: vp => vp.ZoomWindow(rect: value.Window), Op.Of(name: nameof(CameraEdit.ZoomWindow))),
            rotate: static (ctx, value) => WithDirection(scope: ctx, value: value.Axis, op: Op.Of(name: nameof(CameraEdit.Rotate)),
                apply: dir => Result(scope: ctx, apply: vp => vp.Rotate(angleRadians: value.Radians, rotationAxis: dir, rotationCenter: value.Center), Op.Of(name: nameof(CameraEdit.Rotate)))),
            magnify: static (ctx, value) => Result(scope: ctx, apply: vp => value.FixedPoint.Case switch {
                DrawingPoint p => vp.Magnify(magnificationFactor: value.Factor, mode: value.LensMode, fixedScreenPoint: p),
                _ => vp.Magnify(magnificationFactor: value.Factor, mode: value.LensMode),
            }, Op.Of(name: nameof(CameraEdit.Magnify))),
            keyboard: static (ctx, value) => Result(scope: ctx, apply: vp => value.Move.Apply(viewport: vp, leftRight: value.LeftRight, amount: value.Amount), Op.Of(name: nameof(CameraEdit.Keyboard))),
            mouse: static (ctx, value) => Result(scope: ctx, apply: vp => value.Move.Apply(viewport: vp, previous: value.PreviousPoint, current: value.CurrentPoint), Op.Of(name: nameof(CameraEdit.Mouse))),
            mouseLens: static (ctx, value) => Result(scope: ctx, apply: vp => vp.MouseAdjustLensLength(mousePreviousPoint: value.PreviousPoint, mouseCurrentPoint: value.CurrentPoint, moveTarget: value.MoveTarget), Op.Of(name: nameof(CameraEdit.MouseLens))),
            stackMove: static (ctx, value) => Fin.Succ(value: value.Move.Apply(viewport: ctx.Viewport, plane: value.Plane)
                ? CameraScope.RedrawFor(scope: ctx)
                : RedrawRequest.Empty),
            pushView: static (ctx, value) => Result(scope: ctx, apply: vp => vp.PushViewInfo(value.Info, value.IncludeTraceImage), Op.Of(name: nameof(CameraEdit.PushView))));

    // Symmetric reproduces the legacy (padding-1)/2 diagonal inflate; Additive scales the diagonal by an absolute
    // amount; None passes the bounds through untouched.
    private static BoundingBox InflateBox(BoundingBox source, FramePaddingMode mode, double amount) =>
        mode == FramePaddingMode.Symmetric ? Diagonal(source: source, factor: (amount - 1.0) * 0.5)
        : mode == FramePaddingMode.Additive ? Diagonal(source: source, factor: amount)
        : source;

    private static BoundingBox Diagonal(BoundingBox source, double factor) {
        BoundingBox box = source;
        box.Inflate(xAmount: box.Diagonal.X * factor, yAmount: box.Diagonal.Y * factor, zAmount: box.Diagonal.Z * factor);
        return box;
    }

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

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class CameraOps {
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
                    .Map(redraws => redraws.Fold(RedrawRequest.Empty, static (left, right) => left | right))
                    .Map(redraw => CameraOutcomeCreate.Value(
                        value: CameraChangeReceipt.Empty with { Redraw = redraw },
                        redraw: redraw)),
                false => UI.RhinoUi.Protect(valid: () => {
                    scope.Document.Views.EnableRedraw(enable: false, redrawDocument: false, redrawLayers: false);
                    try {
                        return changes.TraverseM(edit => edit.Apply(scope: scope)).As()
                            .Map(redraws => redraws.Fold(RedrawRequest.Empty, static (left, right) => left | right) | CameraScope.RedrawFor(scope: scope))
                            .Map(redraw => CameraOutcomeCreate.Value(
                                value: CameraChangeReceipt.Empty with { Redraw = redraw },
                                redraw: redraw));
                    } finally {
                        scope.Document.Views.EnableRedraw(enable: true, redrawDocument: false, redrawLayers: false);
                    }
                }),
            }
            select outcome,
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
            select CameraOutcomeCreate.Value(value: frame),
            UiBound: false);

    public static CameraOp<Unit> RestoreNamed(string name, CameraPath? path = null, NamedRestorePolicy? restore = null) =>
        new(Run: scope =>
            from index in NamedIndex(document: scope.Document, name: name, op: Op.Of(name: nameof(RestoreNamed)))
            from redraw in (restore ?? NamedRestorePolicy.Default).Use(run: () => (path ?? new CameraPath.Instant()).Restore(scope: scope, index: index))
            select CameraOutcomeCreate.Value(value: unit, redraw: redraw));

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

    public static CameraOp<DrawingBitmap> CaptureBitmap(CaptureRecipe recipe = default) =>
        new(Run: scope => recipe.Render(
                view: scope.View,
                viewport: Some(value: scope.Viewport),
                fallbackDpi: CaptureRecipe.ScreenDpi,
                fallbackDecor: CaptureRecipe.ScreenDecor,
                rewrite: static (decor, _) => decor,
                project: static settings => Optional(ViewCapture.CaptureToBitmap(settings: settings)).ToFin(Fail: Op.Of(name: nameof(CaptureBitmap)).InvalidResult()),
                op: Op.Of(name: nameof(CaptureBitmap)))
            .Map(bitmap => CameraOutcomeCreate.Value(value: bitmap, redraw: RedrawRequest.Empty)));

    public static CameraOp<XmlDocument> CaptureSvg(CaptureRecipe recipe = default) =>
        new(Run: scope => recipe.Render(
                view: scope.View,
                viewport: Some(value: scope.Viewport),
                fallbackDpi: CaptureRecipe.ScreenDpi,
                fallbackDecor: CaptureRecipe.ScreenDecor,
                rewrite: static (decor, _) => decor,
                project: static settings => Optional(ViewCapture.CaptureToSvg(settings: settings)).ToFin(Fail: Op.Of(name: nameof(CaptureSvg)).InvalidResult()),
                op: Op.Of(name: nameof(CaptureSvg)))
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
        return CameraOutcomeCreate.Resource(value: change, change: change);
    }
}
