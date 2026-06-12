using Rasm.Vectors;
using DrawingPoint = System.Drawing.Point;
using DrawingRectangle = System.Drawing.Rectangle;

namespace Rasm.Rhino.Camera;

// --- [TYPES] ------------------------------------------------------------------------------
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
        Switch(
            op,
            instant: static (key, value) => Fin.Succ(value: (CameraPath)value),
            matchAspect: static (key, value) => Fin.Succ(value: (CameraPath)value),
            animated: static (key, _) => Fin.Fail<CameraPath>(error: key.InvalidInput()));
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
    public sealed record ViewState(CameraViewState Value, bool UpdateTarget = true) : CameraEdit;
    public sealed record Project(CameraProjection Projection) : CameraEdit;
    public sealed record DetailProjection(CameraProjection Projection, Option<double> FrustumAspect = default) : CameraEdit;
    public sealed record Frustum(CameraFrustum Value, bool UpdateTarget = true) : CameraEdit;
    public sealed record Isometric(IsometricCamera Camera, string Name = "", bool UpdateConstructionPlane = true) : CameraEdit;
    public sealed record ConstructionPlane(global::Rhino.DocObjects.ConstructionPlane Value) : CameraEdit;
    public sealed record Plan(Point3d Origin, Vector3d XDirection, Vector3d YDirection, bool SetConstructionPlane = true) : CameraEdit;
    public sealed record DisplayMode(DisplayModeDescription Value) : CameraEdit;
    public sealed record Clipping(BoundingBox Box) : CameraEdit;
    public sealed record NearFar(CameraSubject Source, double Margin = 1.0) : CameraEdit;
    public sealed record SectionLink(Seq<Guid> PlaneIds, bool Attach) : CameraEdit;
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
    public sealed record StackMove(CameraStackOp Move, Option<global::Rhino.DocObjects.ConstructionPlane> Plane = default) : CameraEdit;
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
            navigateLookAt: static (ctx, value) =>
                from context in Context.Of(doc: ctx.Document).ToFin()
                from frame in CameraFrame.LookAt(location: value.From, target: value.At, context: context, up: value.UpAxis, op: Op.Of(name: nameof(NavigateLookAt)))
                from _ in frame.Apply(viewport: ctx.Viewport, op: Op.Of(name: nameof(NavigateLookAt)))
                select CameraScope.RedrawFor(scope: ctx),
            subjectFrame: static (ctx, value) => from amount in Op.Of(name: nameof(SubjectFrame)).Positive(value: value.Amount)
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
            lens: static (ctx, value) => from millimeters in Op.Of(name: nameof(Lens)).Positive(value: value.Millimeters)
                                             // Camera35mmLensLength is a native no-op under parallel projection — guard so the
                                             // discarded value surfaces as Fin.Fail instead of a silent success + redraw.
                                         from _ in guard(ctx.Viewport.IsPerspectiveProjection || ctx.Viewport.IsTwoPointPerspectiveProjection, Op.Of(name: nameof(Lens)).InvalidInput())
                                         from redraw in Side(scope: ctx, apply: vp => vp.Camera35mmLensLength = millimeters)
                                         select redraw,
            angle: static (ctx, value) => from radians in Op.Of(name: nameof(Angle)).Positive(value: value.Radians)
                                          from redraw in Side(scope: ctx, apply: vp => vp.CameraAngle = radians)
                                          select redraw,
            defined: static (ctx, value) => Result(scope: ctx, apply: vp => vp.SetProjection(projection: value.Projection, viewName: value.Name, updateConstructionPlane: value.UpdateConstructionPlane), Op.Of(name: nameof(Defined))),
            snapshot: static (ctx, value) => ApplyProjection(scope: ctx, apply: (_, _) => Fin.Succ(value: value.Projection), updateTarget: value.UpdateTarget),
            viewState: static (ctx, value) => ApplyViewState(scope: ctx, state: value.Value, updateTarget: value.UpdateTarget),
            project: static (ctx, value) => from _ in value.Projection.Use(viewport: ctx.Viewport, op: Op.Of(name: nameof(Project)))
                                            select CameraScope.RedrawFor(scope: ctx),
            detailProjection: static (ctx, value) =>
                from detail in ctx.Detail.ToFin(Fail: Op.Of(name: nameof(DetailProjection)).MissingContext())
                // BOUNDARY ADAPTER — the projection lock must be restored on BOTH success and failure of the
                // mutation; computing the fallible result before the relock statement keeps the rail short-circuit
                // from leaking an unlocked detail (which would silently drop the page-width scale lock).
                from redraw in UI.RhinoUi.Protect(valid: () => {
                    bool wasLocked = detail.Viewport.LockedProjection;
                    try {
                        detail.Viewport.LockedProjection = false;
                        return from _proj in value.Projection.Use(viewport: detail.Viewport, op: Op.Of(name: nameof(DetailProjection)))
                               from _aspect in value.FrustumAspect.Map(aspect => {
                                   using ViewportInfo info = new(rhinoViewport: detail.Viewport);
                                   info.FrustumAspect = aspect;
                                   return Op.Of(name: nameof(DetailProjection)).Confirm(success: detail.Viewport.SetViewProjection(projection: info, updateTargetLocation: false));
                               }).IfNone(Fin.Succ(value: unit))
                               select (RedrawRequest)new RedrawRequest.DetailCommit();
                    } finally {
                        detail.Viewport.LockedProjection = wasLocked;
                    }
                })
                select redraw,
            frustum: static (ctx, value) => ApplyProjection(scope: ctx, apply: (_, projection) => value.Value.Apply(projection: projection, op: Op.Of(name: nameof(Frustum))), updateTarget: value.UpdateTarget),
            isometric: static (ctx, value) => Result(scope: ctx, apply: vp => vp.SetProjection(projection: value.Camera, viewName: value.Name, updateConstructionPlane: value.UpdateConstructionPlane), Op.Of(name: nameof(Isometric))),
            constructionPlane: static (ctx, value) => from _ in Optional(value.Value).ToFin(Fail: Op.Of(name: nameof(ConstructionPlane)).InvalidInput())
                                                      from redraw in Side(scope: ctx, apply: vp => vp.SetConstructionPlane(cplane: value.Value))
                                                      select redraw,
            plan: static (ctx, value) => from _ in guard(value.Origin.IsValid && value.XDirection.IsValid && value.YDirection.IsValid
                    && value.XDirection.Length > RhinoMath.ZeroTolerance && value.YDirection.Length > RhinoMath.ZeroTolerance, Op.Of(name: nameof(Plan)).InvalidInput())
                                         from done in Result(scope: ctx, apply: vp => vp.SetToPlanView(value.Origin, value.XDirection, value.YDirection, value.SetConstructionPlane), Op.Of(name: nameof(Plan)))
                                         select done,
            displayMode: static (ctx, value) => from valid in Optional(value.Value).ToFin(Fail: Op.Of(name: nameof(DisplayMode)).InvalidInput())
                                                from redraw in Side(scope: ctx, apply: vp => vp.DisplayMode = valid)
                                                select redraw,
            // SetClippingPlanes adjusts frustum near/far distances only — not scene ClippingPlaneSurface objects;
            // use CameraSection for additive scene-level clipping. A camera-behind box yields near <= 0 or far <= near
            // that SetClippingPlanes accepts silently, so read the frustum back and surface a degenerate split as Fin.Fail.
            clipping: static (ctx, value) => from _box in guard(value.Box.IsValid, Op.Of(name: nameof(Clipping)).InvalidInput())
                                             from redraw in Side(scope: ctx, apply: vp => vp.SetClippingPlanes(box: value.Box))
                                             from _frustum in guard(
                                                 ctx.Viewport.GetFrustum(left: out _, right: out _, bottom: out _, top: out _, nearDistance: out double near, farDistance: out double far) && near > 0.0 && far > near,
                                                 Op.Of(name: nameof(Clipping)).InvalidResult())
                                             select redraw,
            nearFar: static (ctx, value) => ApplyProjection(
                scope: ctx,
                apply: (vp, projection) =>
                    from depth in Optional(value.Source).ToFin(Fail: Op.Of(name: nameof(NearFar)).InvalidInput())
                        .Bind(subject => subject.Depth(viewport: vp))
                    let minNear = projection.PerspectiveMinNearDist
                    let minRatio = Math.Max(projection.PerspectiveMinNearOverFar, RhinoMath.ZeroTolerance)
                    let near = Math.Max(depth.Near * value.Margin, minNear)
                    let far = Math.Max(depth.Far * value.Margin, near / minRatio)
                    from _ in Op.Of(name: nameof(NearFar)).Confirm(
                        success: projection.SetFrustumNearFar(
                            nearDistance: near,
                            farDistance: far,
                            minNearDistance: minNear,
                            minNearOverFar: minRatio,
                            targetDistance: projection.TargetDistance(useFrustumCenterFallback: true)))
                    select projection,
                updateTarget: false),
            sectionLink: static (ctx, value) =>
                from _any in guard(!value.PlaneIds.IsEmpty, Op.Of(name: nameof(SectionLink)).InvalidInput())
                from result in Op.Of(name: nameof(SectionLink)).Catch(() => {
                    Seq<ClippingPlaneObject> planes = value.PlaneIds.Distinct()
                        .Map(id => ctx.Document.Objects.FindId(id: id))
                        .Filter(obj => obj is ClippingPlaneObject)
                        .Map(obj => (ClippingPlaneObject)obj);
                    Seq<Guid> attached = toSeq(ctx.Document.Objects.FindClippingPlanesForViewport(viewport: ctx.Viewport))
                        .Map(static plane => plane.Id);
                    return from _planes in guard(!planes.IsEmpty, Op.Of(name: nameof(SectionLink)).MissingContext())
                           from _ in planes.TraverseM(plane =>
                               (value.Attach, attached.Exists(id => id == plane.Id)) switch {
                                   (true, true) or (false, false) => Fin.Succ(value: unit),
                                   (true, false) => Op.Of(name: nameof(SectionLink)).Confirm(success: plane.AddClipViewport(viewport: ctx.Viewport, commit: false)),
                                   (false, true) => Op.Of(name: nameof(SectionLink)).Confirm(success: plane.RemoveClipViewport(viewport: ctx.Viewport, commit: false)),
                               }).As()
                           from __ in planes.TraverseM(plane =>
                               Op.Of(name: nameof(SectionLink)).Confirm(success: plane.CommitChanges())).As()
                           select CameraScope.RedrawFor(scope: ctx);
                })
                select result,
            transform: static (ctx, value) => ApplyProjection(scope: ctx, apply: (_, projection) => Op.Of(name: nameof(Transform)).Confirm(success: projection.TransformCamera(xform: value.Xform)).Map(_ => projection), updateTarget: true),
            zoom: static (ctx, _) => Result(scope: ctx, apply: static vp => vp.ZoomExtents(), Op.Of(name: nameof(Zoom))),
            zoomSelected: static (ctx, _) => Result(scope: ctx, apply: static vp => vp.ZoomExtentsSelected(), Op.Of(name: nameof(ZoomSelected))),
            zoomBox: static (ctx, value) => from _ in guard(value.Bounds.IsValid, Op.Of(name: nameof(ZoomBox)).InvalidInput())
                                            from done in Result(scope: ctx, apply: vp => vp.ZoomBoundingBox(box: value.Bounds), Op.Of(name: nameof(ZoomBox)))
                                            select done,
            zoomWindow: static (ctx, value) => Result(scope: ctx, apply: vp => vp.ZoomWindow(rect: value.Window), Op.Of(name: nameof(ZoomWindow))),
            rotate: static (ctx, value) => WithDirection(scope: ctx, value: value.Axis, op: Op.Of(name: nameof(Rotate)),
                apply: dir => Result(scope: ctx, apply: vp => vp.Rotate(angleRadians: value.Radians, rotationAxis: dir, rotationCenter: value.Center), Op.Of(name: nameof(Rotate)))),
            magnify: static (ctx, value) => from factor in Op.Of(name: nameof(Magnify)).Positive(value: value.Factor)
                                            from redraw in Result(scope: ctx, apply: vp => value.FixedPoint.Case switch {
                                                DrawingPoint p => vp.Magnify(magnificationFactor: factor, mode: value.LensMode, fixedScreenPoint: p),
                                                _ => vp.Magnify(magnificationFactor: factor, mode: value.LensMode),
                                            }, Op.Of(name: nameof(Magnify)))
                                            select redraw,
            keyboard: static (ctx, value) => from amount in Op.Of(name: nameof(Keyboard)).Finite(value: value.Amount)
                                             from redraw in Result(scope: ctx, apply: vp => value.Move.Apply(viewport: vp, leftRight: value.LeftRight, amount: amount), Op.Of(name: nameof(Keyboard)))
                                             select redraw,
            mouse: static (ctx, value) => Result(scope: ctx, apply: vp => value.Move.Apply(viewport: vp, previous: value.PreviousPoint, current: value.CurrentPoint), Op.Of(name: nameof(Mouse))),
            mouseLens: static (ctx, value) => Result(scope: ctx, apply: vp => vp.MouseAdjustLensLength(mousePreviousPoint: value.PreviousPoint, mouseCurrentPoint: value.CurrentPoint, moveTarget: value.MoveTarget), Op.Of(name: nameof(MouseLens))),
            stackMove: static (ctx, value) => Fin.Succ(value: value.Move.Apply(viewport: ctx.Viewport, plane: value.Plane)
                ? CameraScope.RedrawFor(scope: ctx)
                : RedrawRequest.Empty),
            pushView: static (ctx, value) => Result(scope: ctx, apply: vp => vp.PushViewInfo(value.Info, value.IncludeTraceImage), Op.Of(name: nameof(PushView))));

    private static Fin<Point3d> ValidPoint(Point3d value, Op op) =>
        guard(value.IsValid, op.InvalidInput()).ToFin().Map(_ => value);

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

    private static Fin<RedrawRequest> ApplyViewState(CameraScope scope, CameraViewState state, bool updateTarget) =>
        UI.RhinoUi.Protect(valid: () => Op.Of(name: nameof(ViewState)).Catch(() => state.Projection(op: Op.Of(name: nameof(ViewState))).Bind(projection => {
            using (projection) {
                return (from _projection in Op.Of(name: nameof(ViewState)).Confirm(success: scope.Viewport.SetViewProjection(projection: projection, updateTargetLocation: updateTarget))
                        from _dof in Op.Of(name: nameof(ViewState)).Catch(() => {
                            using ViewInfo info = new(scope.Viewport);
                            _ = state.Dof.Write(view: info);
                            return Op.Of(name: nameof(ViewState)).Confirm(success: scope.Viewport.PushViewInfo(info, includeTraceImage: false));
                        })
                        select unit)
                    .Map(_ => CameraScope.RedrawFor(scope: scope));
            }
        })));

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
}

[Union(SwitchMapStateParameterName = "op")]
public abstract partial record CameraAim {
    private CameraAim() { }
    public sealed record Preserve : CameraAim;
    public sealed record Direction(Vector3d Value, Option<Vector3d> Up = default) : CameraAim;
    public sealed record LookAt(Point3d Location, Point3d Target, Option<Vector3d> Up = default) : CameraAim;
    public sealed record CurveDrive(Curve Path, double S, Option<Curve> LookTarget = default, bool ArcLength = true) : CameraAim;

    internal Fin<Seq<CameraEdit>> Edits(Op op) =>
        Switch(
            op,
            preserve: static (_, _) => Fin.Succ(value: Seq<CameraEdit>()),
            direction: static (key, aim) =>
                guard(aim.Value.IsValid && aim.Value.Length > RhinoMath.ZeroTolerance, key.InvalidInput()).ToFin()
                    .Map(_ => Seq<CameraEdit>(new CameraEdit.Direction(Value: aim.Value))
                        + aim.Up.Map(v => (CameraEdit)new CameraEdit.Up(Value: v)).ToSeq()),
            lookAt: static (key, aim) =>
                guard(aim.Location.IsValid && aim.Target.IsValid, key.InvalidInput()).ToFin()
                    .Map(_ => Seq<CameraEdit>(new CameraEdit.NavigateLookAt(From: aim.Location, At: aim.Target, UpAxis: aim.Up))),
            // PerpendicularFrameAt yields a zero-twist frame; FrameAt twists at high curvature. ArcLength reparameterizes
            // S through NormalizedLengthParameter so equal-S samples are equal-distance along the path.
            curveDrive: static (key, aim) =>
                from _path in guard(aim.Path is { IsValid: true }, key.InvalidInput()).ToFin()
                from edits in key.Catch(() => {
                    double t = aim.ArcLength && aim.Path.NormalizedLengthParameter(s: aim.S, t: out double param)
                        ? param
                        : aim.S;
                    return aim.Path.PerpendicularFrameAt(t: t, plane: out Plane frame) && frame.IsValid
                        ? Fin.Succ(value: aim.LookTarget.Case switch {
                            Curve look => Seq<CameraEdit>(new CameraEdit.NavigateLookAt(
                                From: frame.Origin,
                                At: look.PointAt(t: t),
                                UpAxis: Some(value: frame.YAxis))),
                            _ => Seq<CameraEdit>(
                                new CameraEdit.Direction(Value: -frame.ZAxis, UpdateTarget: true),
                                new CameraEdit.Location(Value: frame.Origin, UpdateTarget: false)),
                        })
                        : Fin.Fail<Seq<CameraEdit>>(error: key.InvalidResult());
                })
                select edits);
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

public sealed record CameraOp<T>(Func<CameraScope, Fin<CameraOutcome<T>>> Run, bool UiBound = true) {
    public CameraOp<TNext> Map<TNext>(Func<T, TNext> project) =>
        new(Run: scope => Run(arg: scope)
                .Map(outcome => CameraOutcome<TNext>.Create(
                    value: project(arg: outcome.Value),
                    redraw: outcome.Redraw,
                    resources: outcome.Resources)),
            UiBound: UiBound);

    public CameraOp<T> MapFail(Func<Error, Error> map) =>
        new(Run: scope => Run(arg: scope).MapFail(map), UiBound: UiBound);

    public CameraOp<TNext> Bind<TNext>(Func<T, CameraOp<TNext>> bind) =>
        new(Run: scope => Run(arg: scope).Bind(outcome => bind(arg: outcome.Value).Run(arg: scope).Map(next => next with {
            Redraw = outcome.Redraw | next.Redraw,
            Resources = outcome.Resources + next.Resources,
        })), UiBound: true);

    public CameraOp<T> Catch(Func<Error, CameraOp<T>> handle) =>
        new(Run: scope => Run(arg: scope).BindFail(error => handle(arg: error).Run(arg: scope)), UiBound: true);
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
               from validDepth in depth.Map(value => guard(RhinoMath.IsValidDouble(x: value) && value > 0.0, op.InvalidInput()).ToFin().Map(_ => Some(value)))
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

public readonly record struct CameraSectionView(
    Guid PlaneId,
    double Offset = 0.0,
    bool Attach = true,
    FramePaddingMode? Padding = null) {
    internal Fin<Seq<CameraEdit>> Edits(CameraScope scope, Op op) {
        Guid planeId = PlaneId;
        double offset = Offset;
        bool attach = Attach;
        FramePaddingMode? padding = Padding;
        return from geom in Optional(scope.Document.Objects.FindId(id: planeId) as ClippingPlaneObject)
                   .Bind(static clip => Optional(clip.ClippingPlaneGeometry))
                   .ToFin(Fail: op.MissingContext())
               let plane = geom.Plane
               let bounds = geom.GetBoundingBox(accurate: true)
               from _valid in guard(plane.IsValid && RhinoMath.IsValidDouble(x: offset), op.InvalidInput()).ToFin()
               let origin = plane.Origin + (plane.ZAxis * offset)
               select Seq<CameraEdit>(
                   new CameraEdit.Plan(Origin: origin, XDirection: plane.XAxis, YDirection: plane.YAxis, SetConstructionPlane: true),
                   new CameraEdit.Project(Projection: new CameraProjection.Parallel(SymmetricFrustum: true)),
                   new CameraEdit.SectionLink(PlaneIds: Seq(planeId), Attach: attach))
                   + ((padding, bounds.IsValid) switch {
                       (FramePaddingMode mode, true) => Seq<CameraEdit>(new CameraEdit.SubjectFrame(Subject: new CameraSubject.InBounds(Value: bounds), Mode: mode)),
                       _ => Seq<CameraEdit>(new CameraEdit.Zoom()),
                   });
    }
}

public readonly record struct CameraShot(
    CameraSubject Subject,
    CameraProjection Projection,
    Option<CameraAim> Aim = default,
    FramePaddingMode? Padding = null,
    double PaddingAmount = CameraDefaults.FramePadding,
    Option<double> Lens = default,
    Option<DisplayModeDescription> Display = default,
    Option<BoundingBox> ViewClipping = default,
    Seq<CameraSection> Sections = default,
    Seq<CameraEdit> Edits = default) {
    internal Fin<Seq<CameraEdit>> Compile(Op op) {
        CameraSubject subject = Subject;
        CameraProjection projection = Projection;
        CameraAim aim = Aim.IfNone(() => new CameraAim.Preserve());
        FramePaddingMode padding = Padding ?? FramePaddingMode.Symmetric;
        double paddingAmount = PaddingAmount;
        Option<double> lens = Lens;
        Option<DisplayModeDescription> display = Display;
        Option<BoundingBox> viewClipping = ViewClipping;
        Seq<CameraEdit> extra = Edits;
        return
            from validSubject in Optional(subject).ToFin(Fail: op.InvalidInput())
            from validProjection in Optional(projection).ToFin(Fail: op.InvalidInput())
            from aimEdits in aim.Edits(op: op)
            select Seq<CameraEdit>(new CameraEdit.Project(Projection: validProjection))
                + aimEdits
                + lens.Map(v => (CameraEdit)new CameraEdit.Lens(Millimeters: v)).ToSeq()
                + Seq<CameraEdit>(new CameraEdit.SubjectFrame(Subject: validSubject, Mode: padding, Amount: paddingAmount))
                + display.Map(v => (CameraEdit)new CameraEdit.DisplayMode(Value: v)).ToSeq()
                + viewClipping.Map(v => (CameraEdit)new CameraEdit.Clipping(Box: v)).ToSeq()
                + extra;
    }
}

public readonly record struct NamedRestorePolicy(bool CPlane = true, bool Projection = true, bool Clipping = true, bool Display = true) {
    public static NamedRestorePolicy Default { get; } = new(CPlane: true, Projection: true, Clipping: true, Display: true);

    internal Fin<T> Use<T>(Func<Fin<T>> run) {
        NamedRestorePolicy self = this;
        Op op = Op.Of(name: nameof(NamedRestorePolicy));
        return Optional(run).ToFin(Fail: op.InvalidInput()).Bind(valid => {
            (bool cplane, bool proj, bool clip, bool disp) = (
                global::Rhino.ApplicationSettings.ViewSettings.DefinedViewSetCPlane,
                global::Rhino.ApplicationSettings.ViewSettings.DefinedViewSetProjection,
                global::Rhino.ApplicationSettings.ViewSettings.DefinedViewSetClippingPlanes,
                global::Rhino.ApplicationSettings.ViewSettings.DefinedViewSetDisplayMode);
            global::Rhino.ApplicationSettings.ViewSettings.DefinedViewSetCPlane = self.CPlane;
            global::Rhino.ApplicationSettings.ViewSettings.DefinedViewSetProjection = self.Projection;
            global::Rhino.ApplicationSettings.ViewSettings.DefinedViewSetClippingPlanes = self.Clipping;
            global::Rhino.ApplicationSettings.ViewSettings.DefinedViewSetDisplayMode = self.Display;
            try {
                return op.Catch(valid);
            } finally {
                global::Rhino.ApplicationSettings.ViewSettings.DefinedViewSetCPlane = cplane;
                global::Rhino.ApplicationSettings.ViewSettings.DefinedViewSetProjection = proj;
                global::Rhino.ApplicationSettings.ViewSettings.DefinedViewSetClippingPlanes = clip;
                global::Rhino.ApplicationSettings.ViewSettings.DefinedViewSetDisplayMode = disp;
            }
        });
    }
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class CameraOps {
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
            from changes in Optional(edits).ToFin(Fail: Op.Of(name: nameof(Change)).InvalidInput())
                                           .Map(static source => toSeq(source))
            from outcome in redrawEach
                ? changes.TraverseM(edit => edit.Apply(scope: scope)).As()
                    .Map(redraws => redraws.Fold(RedrawRequest.Empty, static (l, r) => l | r))
                    .Map(redraw => CameraOutcome<CameraChangeReceipt>.Create(
                        value: CameraChangeReceipt.WithRedraw(redraw: redraw),
                        redraw: redraw))
                : UI.RhinoUi.Protect(valid: () => {
                    _ = Op.Side(() => scope.Document.Views.EnableRedraw(enable: false, redrawDocument: false, redrawLayers: false));
                    Fin<CameraOutcome<CameraChangeReceipt>> result = changes.TraverseM(edit => edit.Apply(scope: scope)).As()
                        .Map(redraws => redraws.Fold(RedrawRequest.Empty, static (l, r) => l | r) | CameraScope.RedrawFor(scope: scope))
                        .Map(redraw => CameraOutcome<CameraChangeReceipt>.Create(
                            value: CameraChangeReceipt.WithRedraw(redraw: redraw),
                            redraw: redraw));
                    _ = Op.Side(() => scope.Document.Views.EnableRedraw(enable: true, redrawDocument: false, redrawLayers: false));
                    return result;
                })
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

    public static CameraOp<CameraChangeReceipt> ArchitecturalShot(ArchitecturalStyle style, CameraSubject subject, Option<Vector3d> lockedUp = default) =>
        new(Run: scope =>
            from edits in style.Build(subject: subject, lockedUp: lockedUp)
            from outcome in Change(edits: edits.AsIterable()).Run(arg: scope)
            select outcome,
            UiBound: true);

    public static CameraOp<CameraChangeReceipt> Section(params CameraSection[] sections) =>
        new(Run: scope =>
            from admitted in Optional(sections).ToFin(Fail: Op.Of(name: nameof(Section)).InvalidInput()).Map(static source => toSeq(source))
            from _any in guard(!admitted.IsEmpty, Op.Of(name: nameof(Section)).InvalidInput())
            from resources in admitted.TraverseM(section => section.Apply(scope: scope, op: Op.Of(name: nameof(Section)))).As()
            let receipt = CameraChangeReceipt.Of(resources: resources, redraw: CameraScope.RedrawFor(scope: scope))
            select CameraOutcome<CameraChangeReceipt>.Create(value: receipt, redraw: receipt.Redraw, resources: resources),
            UiBound: true);

    public static CameraOp<CameraChangeReceipt> LinkSections(Seq<Guid> planeIds, bool attach) =>
        Change(edits: [new CameraEdit.SectionLink(PlaneIds: planeIds, Attach: attach)]);

    public static CameraOp<CameraChangeReceipt> ApplyFrames(Seq<MotionFrame> frames, bool redrawEach = false) =>
        new(
            Run: scope =>
                from _any in guard(!frames.IsEmpty, Op.Of(name: nameof(ApplyFrames)).InvalidInput()).ToFin()
                from receipts in frames.TraverseM(frame =>
                    Change(edits: frame.Edits.AsIterable(), redrawEach: redrawEach).Run(arg: scope)).As()
                let merged = receipts.Fold(
                    CameraOutcome<CameraChangeReceipt>.Create(value: CameraChangeReceipt.Empty),
                    static (acc, next) => CameraOutcome<CameraChangeReceipt>.Create(
                        value: acc.Value + next.Value,
                        redraw: acc.Redraw | next.Redraw,
                        resources: acc.Resources + next.Resources))
                select merged,
            UiBound: true);

    public static CameraOp<CameraChangeReceipt> DriveAlongPath(
        Curve path,
        Seq<double> samples,
        Option<Curve> lookTarget = default,
        bool arcLength = true) =>
        new(Run: scope =>
            from _valid in guard(path is { IsValid: true } && !samples.IsEmpty, Op.Of(name: nameof(DriveAlongPath)).InvalidInput()).ToFin()
            let lens = scope.Viewport.Camera35mmLensLength
            from steps in samples.TraverseM(sample => Op.Of(name: nameof(DriveAlongPath)).Catch(() =>
                arcLength
                    ? Fin.Succ(value: sample)
                    : Fin.Succ(value: path.Domain.NormalizedParameterAt(sample)))).As()
            from profile in MotionProfile.Of(frameCount: samples.Count, easing: UI.Easing.Linear, lensStart: Some(lens), lensEnd: Some(lens), samples: steps)
            from frames in AlongBishopPath(eyePath: path, targetPath: lookTarget, profile: profile, arcLength: arcLength).Run(arg: scope)
            from applied in ApplyFrames(frames: frames.Value, redrawEach: true).Run(arg: scope)
            select applied,
            UiBound: true);

    public static CameraOp<Seq<string>> ListNamed() =>
        new(
            Run: scope => Op.Of(name: nameof(ListNamed)).Catch(() =>
                Fin.Succ(value: CameraOutcome<Seq<string>>.Create(value: toSeq(scope.Document.NamedViews).Map(static view => view.Name)))),
            UiBound: false);

    public static CameraOp<Commands.DocumentResourceChange> SaveNamed(string name, bool fullView = true, Option<CameraDof> dof = default) =>
        new(Run: scope =>
            from valid in Name(value: name, op: Op.Of(name: nameof(SaveNamed)))
            from _ in fullView switch {
                true => Op.Of(name: nameof(SaveNamed)).Catch(() => {
                    using ViewInfo view = new(scope.Viewport);
                    view.Name = valid;
                    _ = dof.Iter(value => value.Write(view: view));
                    return Op.Of(name: nameof(SaveNamed)).Confirm(success: scope.Document.NamedViews.Add(view: view) >= 0);
                }),
                false => Op.Of(name: nameof(SaveNamed)).Confirm(success: scope.Document.NamedViews.Add(name: valid, viewportId: scope.Viewport.Id) >= 0),
            }
            select NamedResource(name: valid),
            UiBound: true);

    public static CameraOp<CameraViewState> ReadNamedState(string name) =>
        new(Run: scope =>
            from index in NamedIndex(document: scope.Document, name: name, op: Op.Of(name: nameof(ReadNamedState)))
            from state in Op.Of(name: nameof(ReadNamedState)).Catch(() => {
                using ViewInfo view = scope.Document.NamedViews[index];
                return CameraViewState.Of(view: view, op: Op.Of(name: nameof(ReadNamedState)));
            })
            select CameraOutcome<CameraViewState>.Create(value: state),
            UiBound: false);

    public static CameraOp<CameraChangeReceipt> ApplyNamedState(string name, bool updateTarget = true) =>
        ReadNamedState(name: name).Bind(state =>
            Change(new CameraEdit.ViewState(Value: state, UpdateTarget: updateTarget)));

    public static CameraOp<CameraFrame> ReadNamed(string name) =>
        ReadNamedState(name: name).Map(static state => state.Frame);

    public static CameraOp<Unit> RestoreNamed(
        string name,
        Option<CameraPath> path = default,
        Option<NamedRestorePolicy> restore = default) =>
        new(Run: scope =>
            from index in NamedIndex(document: scope.Document, name: name, op: Op.Of(name: nameof(RestoreNamed)))
            from redraw in restore.IfNone(NamedRestorePolicy.Default).Use(
                run: () => path.IfNone(() => new CameraPath.Instant()).Restore(scope: scope, index: index))
            select CameraOutcome<Unit>.Create(value: unit, redraw: redraw));

    public static CameraOp<TCapture> RestoreThen<TCapture>(
        string name,
        CameraOp<TCapture> capture,
        Option<CameraPath> path = default,
        Option<NamedRestorePolicy> restore = default) =>
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

    public static CameraOp<CaptureResult> CaptureFrame(CaptureFormat format, CaptureRecipe recipe = default) =>
        new(Run: scope => recipe.WithPolicy(
                fallbackDpi: CaptureRecipe.DefaultScreenDpi,
                fallbackDecor: CaptureRecipe.DefaultScreenDecor,
                rewrite: static (decor, _) => decor).Render(
                view: scope.View,
                viewport: Some(value: scope.Viewport),
                project: settings => CaptureCodec.Of(format: format).Render(settings: settings, op: Op.Of(name: nameof(CaptureFrame))),
                op: Op.Of(name: nameof(CaptureFrame)))
            .Map(value => CameraOutcome<CaptureResult>.Create(value: value, redraw: RedrawRequest.Empty)));

    // Pure motion-frame producer between two camera positions: orientation via Rasm/Vectors Slerp,
    // eye/target/lens via LERP. Each MotionFrame carries the eased pose + interpolated lens; the caller
    // pumps frame.Edits through CameraOps.Change at its own cadence, avoiding the blocking native
    // UI-thread animation of CameraPath.Animated — a different concern (named-view restore).
    public static CameraOp<Seq<MotionFrame>> Interpolate(
        CameraFrame start,
        double startLens,
        CameraFrame end,
        double endLens,
        int frames,
        Option<UI.Easing> easing = default) =>
        new(
            Run: scope =>
                from context in Context.Of(doc: scope.Document).ToFin()
                let key = Op.Of(name: nameof(Interpolate))
                from _frames in guard(frames >= 2, key.InvalidInput()).ToFin()
                from _lens in guard(RhinoMath.IsValidDouble(x: startLens) && RhinoMath.IsValidDouble(x: endLens), key.InvalidInput()).ToFin()
                from _planes in guard(start.Frame.IsValid && end.Frame.IsValid, key.InvalidInput()).ToFin()
                let mode = easing.IfNone(UI.Easing.Linear)
                // orientation rides Rasm/Vectors Slerp (shortest-arc geodesic, long-arc negation owned by MotionInterpolation);
                // eye/target/lens LERP straight, so re-anchor each interpolated plane origin to the linear eye path.
                from sequence in toSeq(Enumerable.Range(start: 0, count: frames))
                    .Map(i => mode.Apply(t: (double)i / (frames - 1)))
                    .TraverseM(t =>
                        from intent in VectorIntent.Pose(start.Frame, to: end.Frame, t: t, mode: MotionInterpolation.Slerp, key: key)
                        from plane in intent.Project<Plane>(context: context, key: key)
                        let location = start.Location + ((end.Location - start.Location) * t)
                        let target = start.Target + ((end.Target - start.Target) * t)
                        let lens = startLens + ((endLens - startLens) * t)
                        select new MotionFrame(Frame: new CameraFrame(Frame: plane with { Origin = location }, Target: target), Lens: lens)).As()
                select CameraOutcome<Seq<MotionFrame>>.Create(value: sequence),
            UiBound: false);

    public static CameraOp<Seq<MotionFrame>> Eased(CameraFrame start, CameraFrame end, MotionProfile profile) =>
        Interpolate(
            start: start,
            startLens: profile.LensStart.IfNone(CameraDefaults.LensLength),
            end: end,
            endLens: profile.LensEnd.IfNone(CameraDefaults.LensLength),
            frames: profile.FrameCount,
            easing: Some(value: profile.Easing));

    // Eye samples ride the curve by eased normalized length; each frame aims at the fixed target via
    // CameraFrame.LookAt (guards both endpoints — a degenerate sample fails the rail). Lens LERPs across frames.
    public static CameraOp<Seq<MotionFrame>> AlongPath(Curve eye, Point3d aim, MotionProfile profile) =>
        new(Run: scope =>
            // FrameCount >= 2 guards the (FrameCount - 1) divisor + non-negative Enumerable.Range count for raw
            // positional profiles; MotionProfile.Of carries the same invariant for factory-constructed callers.
            from context in Context.Of(doc: scope.Document).ToFin()
            from _valid in guard(eye is { IsValid: true } && profile.FrameCount >= 2, Op.Of(name: nameof(AlongPath)).InvalidInput()).ToFin()
            let lensStart = profile.LensStart.IfNone(CameraDefaults.LensLength)
            let lensEnd = profile.LensEnd.IfNone(CameraDefaults.LensLength)
            from frames in toSeq(Enumerable.Range(start: 0, count: profile.FrameCount))
                .Map(i => profile.Easing.Apply(t: (double)i / (profile.FrameCount - 1)))
                .TraverseM(step =>
                    CameraFrame.LookAt(location: eye.PointAtNormalizedLength(length: step), target: aim, context: context, up: Option<Vector3d>.None, op: Op.Of(name: nameof(AlongPath)))
                        .Map(frame => new MotionFrame(Frame: frame, Lens: lensStart + ((lensEnd - lensStart) * step)))).As()
            select CameraOutcome<Seq<MotionFrame>>.Create(value: frames),
            UiBound: false);

    public static CameraOp<Seq<MotionFrame>> AlongBishopPath(
        Curve eyePath,
        Option<Curve> targetPath,
        MotionProfile profile,
        Option<Vector3d> initialUp = default,
        bool arcLength = true) =>
        new(Run: scope =>
            from context in Context.Of(doc: scope.Document).ToFin()
            let key = Op.Of(name: nameof(AlongBishopPath))
            from _valid in guard(eyePath is { IsValid: true } && profile.FrameCount >= 2, key.InvalidInput()).ToFin()
            from steps in profile.Steps(op: key)
            let points = steps.Map(step => arcLength ? eyePath.PointAtNormalizedLength(length: step) : eyePath.PointAt(t: eyePath.Domain.ParameterAt(normalizedParameter: step)))
            from _points in guard(points.Count >= 2 && points.ForAll(static point => point.IsValid), key.InvalidInput()).ToFin()
            let firstTangent = points[1] - points[0]
            let fallbackUp = Math.Abs(Vector3d.ZAxis * firstTangent) / Math.Max(firstTangent.Length, RhinoMath.ZeroTolerance) < 0.95 ? Vector3d.ZAxis : Vector3d.XAxis
            from normal in Direction.Of(value: initialUp.Filter(static up => up.IsValid && up.Length > RhinoMath.ZeroTolerance).IfNone(fallbackUp), context: context, key: key)
            from chain in VectorFrame.Chain(points: points, initialNormal: normal, closed: false, context: context, key: key)
            let lensStart = profile.LensStart.IfNone(CameraDefaults.LensLength)
            let lensEnd = profile.LensEnd.IfNone(CameraDefaults.LensLength)
            from frames in toSeq(Enumerable.Range(start: 0, count: chain.Count))
                .TraverseM(index => {
                    Plane plane = chain[index].Value;
                    double step = steps[index];
                    Point3d target = targetPath.Map(curve => arcLength ? curve.PointAtNormalizedLength(length: step) : curve.PointAt(t: curve.Domain.ParameterAt(normalizedParameter: step))).IfNone(plane.Origin + plane.ZAxis);
                    return CameraFrame.LookAt(location: plane.Origin, target: target, context: context, up: Some(plane.YAxis), op: key)
                        .Map(frame => new MotionFrame(Frame: frame, Lens: lensStart + ((lensEnd - lensStart) * step)));
                }).As()
            select CameraOutcome<Seq<MotionFrame>>.Create(value: frames),
            UiBound: false);

    public static CameraOp<CameraChangeReceipt> ViewFromSection(Guid sectionPlaneId) =>
        new(Run: scope =>
            from edits in new CameraSectionView(PlaneId: sectionPlaneId).Edits(scope: scope, op: Op.Of(name: nameof(ViewFromSection)))
            from outcome in Change(edits: edits.AsIterable()).Run(arg: scope)
            select outcome,
            UiBound: true);

    // No ViewFromClipping factory: a clipping plane IS a section plane (both resolve a ClippingPlaneObject),
    // so ViewFromSection already covers it — a separate factory would fork the rail for no new behavior.
    public static CameraOp<CameraChangeReceipt> ViewFromDetail(Guid detailId) =>
        new(Run: scope =>
            from located in CameraScope.FindDetail(document: scope.Document, id: detailId)
                .ToFin(Fail: Op.Of(name: nameof(ViewFromDetail)).MissingContext())
            from frame in CameraFrame.Of(viewport: located.Detail.Viewport)
            from outcome in Change(edits: [
                new CameraEdit.Plan(Origin: frame.Frame.Origin, XDirection: frame.Frame.XAxis, YDirection: frame.Frame.YAxis, SetConstructionPlane: true),
                new CameraEdit.Project(Projection: new CameraProjection.Parallel(SymmetricFrustum: true)),
                new CameraEdit.Zoom(),
            ]).Run(arg: scope)
            select outcome,
            UiBound: true);

    private static Fin<string> Name(string value, Op op) =>
        op.AcceptText(value: value).MapFail(_ => op.InvalidInput());

    private static Fin<int> NamedIndex(RhinoDoc document, string name, Op op) =>
        from valid in Name(value: name, op: op)
        from index in document.NamedViews.FindByName(name: valid) switch {
            int value when value >= 0 => Fin.Succ(value: value),
            _ => Fin.Fail<int>(error: op.MissingContext()),
        }
        select index;

    private static CameraOutcome<Commands.DocumentResourceChange> NamedResource(string name) {
        Commands.DocumentResourceChange change = new(Kind: Commands.DocumentResourceKind.NamedView, Name: name);
        return CameraOutcome<Commands.DocumentResourceChange>.WithResource(value: change, change: change);
    }
}
