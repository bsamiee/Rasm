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

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record CameraOp<T>(Func<CameraScope, Fin<T>> Run) {
    public CameraOp<TNext> Map<TNext>(Func<T, TNext> project) =>
        new(Run: scope => Run(arg: scope).Map(project));

    public CameraOp<TNext> Bind<TNext>(Func<T, CameraOp<TNext>> bind) =>
        new(Run: scope => Run(arg: scope).Bind(value => bind(arg: value).Run(arg: scope)));
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
public abstract partial record CameraEdit {
    private static readonly Op ConfirmKey = Op.Of(name: nameof(Confirm));
    private static readonly Op FrustumKey = Op.Of(name: nameof(Frustum));
    private static readonly Op PlanKey = Op.Of(name: nameof(Plan));
    private static readonly Op TransformKey = Op.Of(name: nameof(Transform));
    private static readonly Op ZoomBoxKey = Op.Of(name: nameof(ZoomBox));
    private static readonly Op RotateKey = Op.Of(name: nameof(Rotate));

    private CameraEdit() { }

    public sealed record Action(Action<RhinoViewport> Run) : CameraEdit;
    public sealed record Confirm(Func<RhinoViewport, bool> Run) : CameraEdit;
    public sealed record Frame(CameraFrame Value) : CameraEdit;
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
    public sealed record Keyboard(bool RotateInPlace, bool LeftRight, double Amount) : CameraEdit;
    public sealed record KeyboardDolly(double Amount) : CameraEdit;
    public sealed record Mouse(CameraMouseMove Move, DrawingPoint PreviousPoint, DrawingPoint CurrentPoint) : CameraEdit;
    public sealed record MouseLens(DrawingPoint PreviousPoint, DrawingPoint CurrentPoint, bool MoveTarget) : CameraEdit;

    // Push/Pop/Next/Previous return void per verified IL; PushView returns bool.
    public sealed record Push : CameraEdit;
    public sealed record Pop : CameraEdit;
    public sealed record NextView : CameraEdit;
    public sealed record Previous : CameraEdit;
    public sealed record PushView(ViewInfo Info, bool IncludeTarget = true) : CameraEdit;

    internal Fin<Unit> Apply(CameraScope scope, bool redraw) =>
        Switch(
            (Scope: scope, Redraw: redraw),
            action: static (ctx, edit) => from valid in Optional(edit.Run).ToFin(Fail: Op.Of(name: nameof(Action)).InvalidInput())
                                          from done in Side(ctx, valid)
                                          select done,
            confirm: static (ctx, edit) => from valid in Optional(edit.Run).ToFin(Fail: ConfirmKey.InvalidInput())
                                           from done in Result(ctx, valid, ConfirmKey)
                                           select done,
            frame: static (ctx, edit) => edit.Value.Apply(viewport: ctx.Scope.Viewport, op: Op.Of(name: nameof(Frame))).Bind(_ => RedrawIf(ctx)),
            location: static (ctx, edit) => Side(ctx, vp => vp.SetCameraLocation(cameraLocation: edit.Value, updateTargetLocation: edit.UpdateTarget)),
            target: static (ctx, edit) => Side(ctx, vp => vp.SetCameraTarget(targetLocation: edit.Value, updateCameraLocation: edit.UpdateLocation)),
            direction: static (ctx, edit) => from context in Context.Of(doc: ctx.Scope.Document).ToFin()
                                             from direction in VectorIntent.Direction(value: edit.Value).Project<Vector3d>(context: context, key: Op.Of(name: nameof(Direction)))
                                             from done in Side(ctx, vp => vp.SetCameraDirection(cameraDirection: direction, updateTargetLocation: edit.UpdateTarget))
                                             select done,
            up: static (ctx, edit) => from context in Context.Of(doc: ctx.Scope.Document).ToFin()
                                      from up in VectorIntent.Direction(value: edit.Value).Project<Vector3d>(context: context, key: Op.Of(name: nameof(Up)))
                                      from done in Side(ctx, vp => vp.CameraUp = up)
                                      select done,
            lens: static (ctx, edit) => from _ in guard(edit.Millimeters > 0, Op.Of(name: nameof(Lens)).InvalidInput())
                                        from done in Side(ctx, vp => vp.Camera35mmLensLength = edit.Millimeters)
                                        select done,
            angle: static (ctx, edit) => from _ in guard(edit.Radians > 0, Op.Of(name: nameof(Angle)).InvalidInput())
                                         from done in Side(ctx, vp => vp.CameraAngle = edit.Radians)
                                         select done,
            defined: static (ctx, edit) => Result(ctx, vp => vp.SetProjection(projection: edit.Projection, viewName: edit.Name, updateConstructionPlane: edit.UpdateConstructionPlane), Op.Of(name: nameof(Defined))),
            snapshot: static (ctx, edit) => Result(ctx, vp => vp.SetViewProjection(projection: edit.Projection, updateTargetLocation: edit.UpdateTarget), Op.Of(name: nameof(Snapshot))),
            project: static (ctx, edit) => from _ in edit.Projection.Use(viewport: ctx.Scope.Viewport, op: Op.Of(name: nameof(Project)))
                                           from done in RedrawIf(ctx)
                                           select done,
            frustum: static (ctx, edit) => UI.RhinoUi.Protect(valid: () => {
                using ViewportInfo projection = new(ctx.Scope.Viewport);
                return from authored in edit.Value.Apply(projection: projection, op: FrustumKey)
                       from _ in FrustumKey.Confirm(success: ctx.Scope.Viewport.SetViewProjection(projection: authored, updateTargetLocation: edit.UpdateTarget))
                       from done in RedrawIf(ctx)
                       select done;
            }),
            planePlain: static (ctx, edit) => from _ in guard(edit.Value.IsValid, Op.Of(name: nameof(PlanePlain)).InvalidInput())
                                              from done in Side(ctx, vp => vp.SetConstructionPlane(plane: edit.Value))
                                              select done,
            planeFull: static (ctx, edit) => from _ in Optional(edit.Value).ToFin(Fail: Op.Of(name: nameof(PlaneFull)).InvalidInput())
                                             from done in Side(ctx, vp => vp.SetConstructionPlane(cplane: edit.Value))
                                             select done,
            plan: static (ctx, edit) =>
                from _ in guard(edit.Origin.IsValid && edit.XDirection.IsValid && edit.YDirection.IsValid
                                && edit.XDirection.Length > RhinoMath.ZeroTolerance && edit.YDirection.Length > RhinoMath.ZeroTolerance, PlanKey.InvalidInput())
                from done in Result(ctx, vp => vp.SetToPlanView(edit.Origin, edit.XDirection, edit.YDirection, edit.SetConstructionPlane), PlanKey)
                select done,
            displayMode: static (ctx, edit) => from valid in Optional(edit.Value).ToFin(Fail: Op.Of(name: nameof(DisplayMode)).InvalidInput())
                                               from done in Side(ctx, vp => vp.DisplayMode = valid)
                                               select done,
            clipping: static (ctx, edit) => from _ in guard(edit.Box.IsValid, Op.Of(name: nameof(Clipping)).InvalidInput())
                                            from done in Side(ctx, vp => vp.SetClippingPlanes(box: edit.Box))
                                            select done,
            transform: static (ctx, edit) => UI.RhinoUi.Protect(valid: () => {
                using ViewportInfo projection = new(rhinoViewport: ctx.Scope.Viewport);
                return TransformKey.Confirm(success: projection.TransformCamera(xform: edit.Xform))
                    .Bind(_ => TransformKey.Confirm(success: ctx.Scope.Viewport.SetViewProjection(projection: projection, updateTargetLocation: true)))
                    .Bind(_ => RedrawIf(ctx));
            }),
            zoom: static (ctx, _) => Result(ctx, static vp => vp.ZoomExtents(), Op.Of(name: nameof(Zoom))),
            zoomSelected: static (ctx, _) => Result(ctx, static vp => vp.ZoomExtentsSelected(), Op.Of(name: nameof(ZoomSelected))),
            zoomBox: static (ctx, edit) => from _ in guard(edit.Bounds.IsValid, ZoomBoxKey.InvalidInput())
                                           from done in Result(ctx, vp => vp.ZoomBoundingBox(box: edit.Bounds), ZoomBoxKey)
                                           select done,
            zoomWindow: static (ctx, edit) => Result(ctx, vp => vp.ZoomWindow(rect: edit.Window), Op.Of(name: nameof(ZoomWindow))),
            rotate: static (ctx, edit) => from context in Context.Of(doc: ctx.Scope.Document).ToFin()
                                          from direction in VectorIntent.Direction(value: edit.Axis).Project<Vector3d>(context: context, key: RotateKey)
                                          from done in Result(ctx, vp => vp.Rotate(angleRadians: edit.Radians, rotationAxis: direction, rotationCenter: edit.Center), RotateKey)
                                          select done,
            magnify: static (ctx, edit) => Result(ctx, vp => edit.FixedPoint.Case switch {
                DrawingPoint p => vp.Magnify(magnificationFactor: edit.Factor, mode: edit.LensMode, fixedScreenPoint: p),
                _ => vp.Magnify(magnificationFactor: edit.Factor, mode: edit.LensMode),
            }, Op.Of(name: nameof(Magnify))),
            keyboard: static (ctx, edit) => Result(ctx, vp => edit.RotateInPlace
                ? vp.KeyboardRotate(leftRight: edit.LeftRight, angleRadians: edit.Amount)
                : vp.KeyboardDolly(leftRight: edit.LeftRight, amount: edit.Amount), Op.Of(name: nameof(Keyboard))),
            keyboardDolly: static (ctx, edit) => Result(ctx, vp => vp.KeyboardDollyInOut(amount: edit.Amount), Op.Of(name: nameof(KeyboardDolly))),
            mouse: static (ctx, edit) => Result(ctx, vp => edit.Move.Apply(viewport: vp, previous: edit.PreviousPoint, current: edit.CurrentPoint), Op.Of(name: nameof(Mouse))),
            mouseLens: static (ctx, edit) => Result(ctx, vp => vp.MouseAdjustLensLength(mousePreviousPoint: edit.PreviousPoint, mouseCurrentPoint: edit.CurrentPoint, moveTarget: edit.MoveTarget), Op.Of(name: nameof(MouseLens))),
            push: static (ctx, _) => Side(ctx, static vp => vp.PushViewProjection()),
            pop: static (ctx, _) => Side(ctx, static vp => vp.PopViewProjection()),
            nextView: static (ctx, _) => Side(ctx, static vp => vp.NextViewProjection()),
            previous: static (ctx, _) => Side(ctx, static vp => vp.PreviousViewProjection()),
            pushView: static (ctx, edit) => Result(ctx, vp => vp.PushViewInfo(edit.Info, edit.IncludeTarget), Op.Of(name: nameof(PushView))));

    private static Fin<Unit> RedrawIf((CameraScope Scope, bool Redraw) ctx) =>
        ctx.Redraw ? ctx.Scope.Redraw() : Fin.Succ(value: unit);

    private static Fin<Unit> Side((CameraScope Scope, bool Redraw) ctx, Action<RhinoViewport> apply) {
        apply(obj: ctx.Scope.Viewport);
        return RedrawIf(ctx);
    }

    private static Fin<Unit> Result((CameraScope Scope, bool Redraw) ctx, Func<RhinoViewport, bool> apply, Op op) =>
        from _ in op.Confirm(success: apply(arg: ctx.Scope.Viewport))
        from done in RedrawIf(ctx)
        select done;
}

[Union(SwitchMapStateParameterName = "context")]
public abstract partial record CameraNamedRestore {
    private static readonly Op ApplyKey = Op.Of(name: nameof(CameraNamedRestore));
    private CameraNamedRestore() { }

    public sealed record Direct : CameraNamedRestore;
    public sealed record MatchAspect : CameraNamedRestore;
    public sealed record ConstantSpeed(double UnitsPerFrame, int DelayMilliseconds) : CameraNamedRestore;
    public sealed record ConstantTime(int Frames, int DelayMilliseconds) : CameraNamedRestore;

    internal Fin<Unit> Apply(RhinoDoc document, int index, RhinoViewport viewport) =>
        Switch(
            (Document: document, Index: index, Viewport: viewport),
            direct: static (ctx, _) => ApplyKey.Confirm(success: ctx.Document.NamedViews.Restore(index: ctx.Index, viewport: ctx.Viewport)),
            matchAspect: static (ctx, _) => ApplyKey.Confirm(success: ctx.Document.NamedViews.RestoreWithAspectRatio(index: ctx.Index, viewport: ctx.Viewport)),
            constantSpeed: static (ctx, edit) => ApplyKey.Confirm(success: ctx.Document.NamedViews.RestoreAnimatedConstantSpeed(ctx.Index, ctx.Viewport, edit.UnitsPerFrame, edit.DelayMilliseconds)),
            constantTime: static (ctx, edit) => ApplyKey.Confirm(success: ctx.Document.NamedViews.RestoreAnimatedConstantTime(ctx.Index, ctx.Viewport, edit.Frames, edit.DelayMilliseconds)));
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct CameraCapture(
    DrawingSize Size,
    double Dpi = 96,
    bool DrawBackground = true,
    bool DrawGrid = false,
    bool DrawAxis = false,
    bool DrawLockedObjects = true,
    bool DrawSelectedObjectsOnly = false,
    bool DrawClippingPlanes = true,
    bool DrawLights = true,
    bool DrawWallpaper = true,
    bool UsePrintWidths = false,
    ViewCaptureSettings.ColorMode OutputColor = ViewCaptureSettings.ColorMode.DisplayColor,
    Option<CaptureLayout> Layout = default) {
    private static readonly Op CaptureKey = Op.Of(name: nameof(Capture));

    internal Fin<T> Capture<T>(CameraScope scope, Func<ViewCaptureSettings, Fin<T>> project) {
        CameraCapture self = this;
        return from valid in Optional(project).ToFin(Fail: CaptureKey.InvalidInput())
               from _ in guard(self.Size is { Width: > 0, Height: > 0 } && RhinoMath.IsValidDouble(x: self.Dpi) && self.Dpi > 0.0, CaptureKey.InvalidInput())
               from result in UI.RhinoUi.Protect(valid: () => {
                   using ViewCaptureSettings settings = new(sourceView: scope.View, mediaSize: self.Size, dpi: self.Dpi) {
                       DrawBackground = self.DrawBackground,
                       DrawGrid = self.DrawGrid,
                       DrawAxis = self.DrawAxis,
                       DrawLockedObjects = self.DrawLockedObjects,
                       DrawSelectedObjectsOnly = self.DrawSelectedObjectsOnly,
                       DrawClippingPlanes = self.DrawClippingPlanes,
                       DrawLights = self.DrawLights,
                       DrawWallpaper = self.DrawWallpaper,
                       UsePrintWidths = self.UsePrintWidths,
                       OutputColor = self.OutputColor,
                   };
                   settings.SetViewport(viewport: scope.Viewport);
                   return from layout in self.Layout.Map(active => active.Apply(settings: settings, op: CaptureKey)).IfNone(Fin.Succ(value: unit))
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

    public static CameraOp<T> Query<T>(Func<CameraScope, Fin<T>> query) =>
        new(Run: scope => Optional(query).ToFin(Fail: Op.Of(name: nameof(Query)).InvalidInput()).Bind(valid => valid(arg: scope)));

    public static CameraOp<T> Read<T>(Func<CameraSnapshot, Fin<T>> project) =>
        new(Run: scope => Optional(project).ToFin(Fail: Op.Of(name: nameof(Read)).InvalidInput()).Bind(valid => scope.Snapshot().Bind(snapshot => {
            using (snapshot) { return valid(arg: snapshot); }
        })));

    public static CameraOp<Unit> Change(params CameraEdit[] edits) =>
        Change(edits: edits, redrawEach: true);

    // redrawEach:false brackets the batch in EnableRedraw(false) + try/finally to eliminate flicker even if an edit throws.
    public static CameraOp<Unit> Change(IEnumerable<CameraEdit> edits, bool redrawEach = true) =>
        new(Run: scope =>
            from changes in Optional(edits).ToFin(Fail: Op.Of(name: nameof(Change)).InvalidInput()).Map(static source => toSeq(source))
            from result in redrawEach switch {
                true => changes.TraverseM(edit => edit.Apply(scope: scope, redraw: true)).As().Map(static _ => unit),
                false => UI.RhinoUi.Protect(valid: () => {
                    scope.Document.Views.EnableRedraw(enable: false, redrawDocument: false, redrawLayers: false);
                    try {
                        return changes.TraverseM(edit => edit.Apply(scope: scope, redraw: false)).As().Bind(_ => scope.Redraw());
                    } finally {
                        scope.Document.Views.EnableRedraw(enable: true, redrawDocument: false, redrawLayers: false);
                    }
                }),
            }
            select result);

    public static CameraOp<Commands.DocumentResourceChange> SaveNamed(string name) =>
        new(Run: scope =>
            from valid in Name(value: name, op: SaveNamedKey)
            from index in scope.Document.NamedViews.Add(name: valid, viewportId: scope.Viewport.Id) switch {
                int value when value >= 0 => Fin.Succ(value: value),
                _ => Fin.Fail<int>(error: SaveNamedKey.InvalidResult()),
            }
            select new Commands.DocumentResourceChange(Kind: Commands.DocumentResourceKind.NamedView, Name: valid));

    public static CameraOp<Unit> RestoreNamed(string name) =>
        RestoreNamed(name: name, restore: new CameraNamedRestore.Direct());

    public static CameraOp<Unit> RestoreNamed(string name, CameraNamedRestore restore) =>
        new(Run: scope =>
            from index in NamedIndex(document: scope.Document, name: name, op: RestoreNamedKey)
            from restored in restore.Apply(document: scope.Document, index: index, viewport: scope.Viewport)
            from redraw in scope.Redraw()
            select redraw);

    public static CameraOp<Commands.DocumentResourceChange> RenameNamed(string current, string next) =>
        new(Run: scope =>
            from index in NamedIndex(document: scope.Document, name: current, op: RenameNamedKey)
            from name in Name(value: next, op: RenameNamedKey)
            from renamed in RenameNamedKey.Confirm(success: scope.Document.NamedViews.Rename(index: index, newName: name))
            select new Commands.DocumentResourceChange(Kind: Commands.DocumentResourceKind.NamedView, Name: name));

    public static CameraOp<Commands.DocumentResourceChange> DeleteNamed(string name) =>
        new(Run: scope =>
            from index in NamedIndex(document: scope.Document, name: name, op: DeleteNamedKey)
            from valid in Name(value: name, op: DeleteNamedKey)
            from deleted in DeleteNamedKey.Confirm(success: scope.Document.NamedViews.Delete(index: index))
            select new Commands.DocumentResourceChange(Kind: Commands.DocumentResourceKind.NamedView, Name: valid));

    public static CameraOp<DrawingBitmap> CaptureBitmap(CameraCapture capture) =>
        new(Run: scope => capture.Capture(scope: scope, project: static settings => Optional(ViewCapture.CaptureToBitmap(settings: settings)).ToFin(Fail: Op.Of(name: nameof(CaptureBitmap)).InvalidResult())));

    public static CameraOp<XmlDocument> CaptureSvg(CameraCapture capture) =>
        new(Run: scope => capture.Capture(scope: scope, project: static settings => Optional(ViewCapture.CaptureToSvg(settings: settings)).ToFin(Fail: Op.Of(name: nameof(CaptureSvg)).InvalidResult())));

    private static Fin<int> NamedIndex(RhinoDoc document, string name, Op op) =>
        from valid in Name(value: name, op: op)
        from index in document.NamedViews.FindByName(name: valid) switch {
            int value when value >= 0 => Fin.Succ(value: value),
            _ => Fin.Fail<int>(error: op.MissingContext()),
        }
        select index;

    private static Fin<string> Name(string value, Op op) =>
        op.AcceptText(value: value).MapFail(_ => op.InvalidInput());
}
