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
    internal Fin<T> Apply(CameraScope scope) => Run(arg: scope);

    public CameraOp<TNext> Map<TNext>(Func<T, TNext> project) =>
        new(Run: scope => Apply(scope: scope).Map(project));

    public CameraOp<TNext> Bind<TNext>(Func<T, CameraOp<TNext>> bind) =>
        new(Run: scope => Apply(scope: scope).Bind(value => bind(arg: value).Apply(scope: scope)));
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct CameraProjection(Func<RhinoViewport, bool> Apply) {
    public static CameraProjection Parallel(bool symmetricFrustum = true) =>
        new(Apply: viewport => viewport.ChangeToParallelProjection(symmetricFrustum: symmetricFrustum));

    public static CameraProjection Perspective(Option<double> targetDistance = default, bool symmetricFrustum = true, double lensLength = 50.0) =>
        new(Apply: viewport => targetDistance.Case switch {
            double distance => viewport.ChangeToPerspectiveProjection(targetDistance: distance, symmetricFrustum: symmetricFrustum, lensLength: lensLength),
            _ => viewport.ChangeToPerspectiveProjection(symmetricFrustum: symmetricFrustum, lensLength: lensLength),
        });

    public static CameraProjection TwoPointPerspective(double lensLength = 50.0, Option<(Vector3d Up, double TargetDistance)> target = default) =>
        new(Apply: viewport => target.Case switch {
            (Vector3d up, double distance) => viewport.ChangeToTwoPointPerspectiveProjection(lensLength: lensLength, up: up, targetDistance: distance),
            _ => viewport.ChangeToTwoPointPerspectiveProjection(lensLength: lensLength),
        });

    public static CameraProjection ParallelReflected { get; } =
        new(Apply: static viewport => viewport.ChangeToParallelReflectedProjection());

    internal Fin<Unit> Use(RhinoViewport viewport, Op op) {
        Func<RhinoViewport, bool> apply = Apply;
        return from validViewport in Optional(viewport).ToFin(Fail: op.InvalidInput())
               from result in op.Confirm(success: apply(arg: validViewport))
               select result;
    }
}

public sealed record CameraEdit {
    private readonly Func<CameraScope, bool, Fin<Unit>> apply;

    private CameraEdit(Func<CameraScope, bool, Fin<Unit>> apply) =>
        this.apply = apply ?? throw new ArgumentNullException(paramName: nameof(apply));

    internal Fin<Unit> Apply(CameraScope scope, bool redraw) =>
        apply(arg1: scope, arg2: redraw);

    public static CameraEdit Native(Action<RhinoViewport> change) =>
        new(apply: (scope, redraw) =>
            Optional(change).ToFin(Fail: Op.Of(name: nameof(Native)).InvalidInput())
                .Map(valid => { valid(obj: scope.Viewport); return unit; })
                .Bind(_ => Redraw(scope: scope, redraw: redraw)));

    public static CameraEdit Native(Func<RhinoViewport, bool> change) =>
        new(apply: (scope, redraw) =>
            from valid in Optional(change).ToFin(Fail: Op.Of(name: nameof(Native)).InvalidInput())
            from _ in Op.Of(name: nameof(Native)).Confirm(success: valid(arg: scope.Viewport))
            from result in Redraw(scope: scope, redraw: redraw)
            select result);

    public static CameraEdit Frame(CameraFrame frame) =>
        new(apply: (scope, redraw) =>
            from _ in frame.Apply(viewport: scope.Viewport, op: Op.Of(name: nameof(Frame)))
            from result in Redraw(scope: scope, redraw: redraw)
            select result);

    public static CameraEdit Location(Point3d value, bool updateTarget = true) =>
        Native(change: viewport => viewport.SetCameraLocation(cameraLocation: value, updateTargetLocation: updateTarget));

    public static CameraEdit Target(Point3d value, bool updateLocation = true) =>
        Native(change: viewport => viewport.SetCameraTarget(targetLocation: value, updateCameraLocation: updateLocation));

    public static CameraEdit Direction(Vector3d value, bool updateTarget = true) =>
        new(apply: (scope, redraw) =>
            from context in Context.Of(doc: scope.Document).ToFin()
            from direction in VectorIntent.Direction(value: value).Project<Vector3d>(context: context, key: Op.Of(name: nameof(Direction)))
            from result in Native(change: viewport => viewport.SetCameraDirection(cameraDirection: direction, updateTargetLocation: updateTarget)).Apply(scope: scope, redraw: redraw)
            select result);

    public static CameraEdit Up(Vector3d value) =>
        new(apply: (scope, redraw) =>
            from context in Context.Of(doc: scope.Document).ToFin()
            from up in VectorIntent.Direction(value: value).Project<Vector3d>(context: context, key: Op.Of(name: nameof(Up)))
            from result in Native(change: viewport => viewport.CameraUp = up).Apply(scope: scope, redraw: redraw)
            select result);

    public static CameraEdit Lens(double millimeters) =>
        new(apply: (scope, redraw) =>
            from _ in guard(millimeters > 0, Op.Of(name: nameof(Lens)).InvalidInput())
            from result in Native(change: viewport => viewport.Camera35mmLensLength = millimeters).Apply(scope: scope, redraw: redraw)
            select result);

    public static CameraEdit Angle(double radians) =>
        new(apply: (scope, redraw) =>
            from _ in guard(radians > 0, Op.Of(name: nameof(Angle)).InvalidInput())
            from result in Native(change: viewport => viewport.CameraAngle = radians).Apply(scope: scope, redraw: redraw)
            select result);

    public static CameraEdit Projection(DefinedViewportProjection projection, string name = "", bool updateConstructionPlane = true) =>
        Native(change: viewport => viewport.SetProjection(projection: projection, viewName: name, updateConstructionPlane: updateConstructionPlane));

    public static CameraEdit Projection(ViewportInfo projection, bool updateTarget = true) =>
        Native(change: viewport => viewport.SetViewProjection(projection: projection, updateTargetLocation: updateTarget));

    public static CameraEdit Projection(CameraProjection projection) =>
        new(apply: (scope, redraw) =>
            from _ in projection.Use(viewport: scope.Viewport, op: Op.Of(name: nameof(Projection)))
            from result in Redraw(scope: scope, redraw: redraw)
            select result);

    public static CameraEdit Frustum(CameraFrustum frustum, bool updateTarget = true) =>
        new(apply: (scope, redraw) => UI.RhinoUi.Protect(valid: () => {
            Op op = Op.Of(name: nameof(Frustum));
            using ViewportInfo projection = new(scope.Viewport);
            return from authored in frustum.Apply(projection: projection, op: op)
                   from _ in op.Confirm(success: scope.Viewport.SetViewProjection(projection: authored, updateTargetLocation: updateTarget))
                   from result in Redraw(scope: scope, redraw: redraw)
                   select result;
        }));

    public static CameraEdit Plane(Plane plane) =>
        new(apply: (scope, redraw) =>
            from _ in guard(plane.IsValid, Op.Of(name: nameof(Plane)).InvalidInput())
            from result in Native(change: viewport => viewport.SetConstructionPlane(plane: plane)).Apply(scope: scope, redraw: redraw)
            select result);

    public static CameraEdit Zoom() =>
        Native(change: static viewport => viewport.ZoomExtents());

    public static CameraEdit ZoomSelected() =>
        Native(change: static viewport => viewport.ZoomExtentsSelected());

    public static CameraEdit Zoom(BoundingBox bounds) =>
        new(apply: (scope, redraw) =>
            from _ in guard(bounds.IsValid, Op.Of(name: nameof(Zoom)).InvalidInput())
            from result in Native(change: viewport => viewport.ZoomBoundingBox(box: bounds)).Apply(scope: scope, redraw: redraw)
            select result);

    public static CameraEdit Zoom(DrawingRectangle window) =>
        Native(change: viewport => viewport.ZoomWindow(rect: window));

    public static CameraEdit Rotate(double radians, Vector3d axis, Point3d center) =>
        new(apply: (scope, redraw) =>
            from context in Context.Of(doc: scope.Document).ToFin()
            from direction in VectorIntent.Direction(value: axis).Project<Vector3d>(context: context, key: Op.Of(name: nameof(Rotate)))
            from result in Native(change: viewport => viewport.Rotate(angleRadians: radians, rotationAxis: direction, rotationCenter: center)).Apply(scope: scope, redraw: redraw)
            select result);

    public static CameraEdit Magnify(double factor, bool lensMode, Option<DrawingPoint> fixedPoint = default) =>
        Native(change: viewport => fixedPoint.Case switch {
            DrawingPoint point => viewport.Magnify(magnificationFactor: factor, mode: lensMode, fixedScreenPoint: point),
            _ => viewport.Magnify(magnificationFactor: factor, mode: lensMode),
        });

    public static CameraEdit Keyboard(bool rotate, bool leftRight, double amount) =>
        Native(change: viewport => rotate switch {
            true => viewport.KeyboardRotate(leftRight: leftRight, angleRadians: amount),
            false => viewport.KeyboardDolly(leftRight: leftRight, amount: amount),
        });

    public static CameraEdit KeyboardDolly(double amount) =>
        Native(change: viewport => viewport.KeyboardDollyInOut(amount: amount));

    public static CameraEdit Mouse(CameraMouseMove move, DrawingPoint previous, DrawingPoint current) =>
        Native(change: viewport => move.Apply(viewport: viewport, previous: previous, current: current));

    public static CameraEdit MouseLens(DrawingPoint previous, DrawingPoint current, bool moveTarget) =>
        Native(change: viewport => viewport.MouseAdjustLensLength(mousePreviousPoint: previous, mouseCurrentPoint: current, moveTarget: moveTarget));

    public static CameraEdit Push() =>
        Native(change: static viewport => viewport.PushViewProjection());

    public static CameraEdit Pop() =>
        Native(change: static viewport => viewport.PopViewProjection());

    public static CameraEdit Next() =>
        Native(change: static viewport => viewport.NextViewProjection());

    public static CameraEdit Previous() =>
        Native(change: static viewport => viewport.PreviousViewProjection());

    public static CameraEdit Push(ViewInfo info, bool includeTarget = true) =>
        Native(change: viewport => viewport.PushViewInfo(info, includeTarget));

    private static Fin<Unit> Redraw(CameraScope scope, bool redraw) =>
        redraw ? scope.Redraw() : Fin.Succ(value: unit);
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct CameraNamedRestore(Func<RhinoDoc, int, RhinoViewport, Fin<Unit>> Run) {
    public static CameraNamedRestore Direct { get; } =
        Of(op: Op.Of(name: nameof(Direct)), apply: static (d, i, v) => d.NamedViews.Restore(index: i, viewport: v));

    public static CameraNamedRestore MatchAspect { get; } =
        Of(op: Op.Of(name: nameof(MatchAspect)), apply: static (d, i, v) => d.NamedViews.RestoreWithAspectRatio(index: i, viewport: v));

    public static CameraNamedRestore ConstantSpeed(double unitsPerFrame, int delayMilliseconds) =>
        Of(op: Op.Of(name: nameof(ConstantSpeed)), apply: (d, i, v) => d.NamedViews.RestoreAnimatedConstantSpeed(i, v, unitsPerFrame, delayMilliseconds));

    public static CameraNamedRestore ConstantTime(int frames, int delayMilliseconds) =>
        Of(op: Op.Of(name: nameof(ConstantTime)), apply: (d, i, v) => d.NamedViews.RestoreAnimatedConstantTime(i, v, frames, delayMilliseconds));

    internal Fin<Unit> Apply(RhinoDoc document, int index, RhinoViewport viewport) =>
        (Optional(Run) | Some(Direct.Run)).ToFin(Fail: Op.Of(name: nameof(CameraNamedRestore)).InvalidInput()).Bind(run => run(arg1: document, arg2: index, arg3: viewport));

    private static CameraNamedRestore Of(Op op, Func<RhinoDoc, int, RhinoViewport, bool> apply) =>
        new(Run: (doc, idx, vp) => op.Confirm(success: apply(arg1: doc, arg2: idx, arg3: vp)));
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct CameraCapture(
    DrawingSize Size,
    double Dpi = 96,
    ViewCaptureSettings.ViewAreaMapping Area = ViewCaptureSettings.ViewAreaMapping.View,
    bool DrawBackground = true,
    bool DrawGrid = false,
    bool DrawAxis = false,
    bool DrawLockedObjects = true,
    bool DrawSelectedObjectsOnly = false,
    bool DrawClippingPlanes = true,
    bool DrawLights = true,
    bool DrawWallpaper = true,
    bool UsePrintWidths = false,
    bool ApplyDisplayModeThicknessScales = false,
    ViewCaptureSettings.ColorMode OutputColor = ViewCaptureSettings.ColorMode.DisplayColor,
    Option<DrawingRectangle> Crop = default,
    Option<(Point2d A, Point2d B)> Window = default) {
    internal Fin<T> Capture<T>(CameraScope scope, Func<ViewCaptureSettings, Fin<T>> project) {
        CameraCapture self = this;
        return from valid in Optional(project).ToFin(Fail: Op.Of(name: nameof(Capture)).InvalidInput())
               from result in UI.RhinoUi.Protect(valid: () => {
                   using ViewCaptureSettings settings = new(sourceView: scope.View, mediaSize: self.Size, dpi: self.Dpi) {
                       ViewArea = self.Area,
                       DrawBackground = self.DrawBackground,
                       DrawGrid = self.DrawGrid,
                       DrawAxis = self.DrawAxis,
                       DrawLockedObjects = self.DrawLockedObjects,
                       DrawSelectedObjectsOnly = self.DrawSelectedObjectsOnly,
                       DrawClippingPlanes = self.DrawClippingPlanes,
                       DrawLights = self.DrawLights,
                       DrawWallpaper = self.DrawWallpaper,
                       UsePrintWidths = self.UsePrintWidths,
                       ApplyDisplayModeThicknessScales = self.ApplyDisplayModeThicknessScales,
                       OutputColor = self.OutputColor,
                   };
                   settings.SetViewport(viewport: scope.Viewport);
                   _ = self.Crop.Iter(rectangle => settings.SetLayout(self.Size, rectangle));
                   _ = self.Window.Iter(points => settings.SetWindowRect(screenPoint1: points.A, screenPoint2: points.B));
                   return settings.IsValid switch {
                       true => valid(arg: settings),
                       false => Fin.Fail<T>(error: Op.Of(name: nameof(CameraCapture)).InvalidResult()),
                   };
               })
               select result;
    }
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class CameraOps {
    public static CameraOp<T> Query<T>(Func<CameraScope, Fin<T>> query) =>
        new(Run: scope => Optional(query).ToFin(Fail: Op.Of(name: nameof(Query)).InvalidInput()).Bind(valid => valid(arg: scope)));

    public static CameraOp<T> Read<T>(Func<CameraSnapshot, Fin<T>> project) =>
        new(Run: scope => Optional(project).ToFin(Fail: Op.Of(name: nameof(Read)).InvalidInput()).Bind(valid => scope.Snapshot().Bind(snapshot => {
            using (snapshot) { return valid(arg: snapshot); }
        })));

    public static CameraOp<Unit> Change(params CameraEdit[] edits) =>
        Change(edits: edits, redrawEach: true);

    public static CameraOp<Unit> Change(IEnumerable<CameraEdit> edits, bool redrawEach = true) =>
        new(Run: scope =>
            from changes in Optional(edits).ToFin(Fail: Op.Of(name: nameof(Change)).InvalidInput()).Map(static source => toSeq(source))
            from applied in changes.TraverseM(edit => edit.Apply(scope: scope, redraw: redrawEach)).As()
            from redraw in redrawEach ? Fin.Succ(value: unit) : scope.Redraw()
            select redraw);

    public static CameraOp<Commands.DocumentResourceChange> SaveNamed(string name) =>
        new(Run: scope =>
            from valid in Name(value: name, op: Op.Of(name: nameof(SaveNamed)))
            from index in scope.Document.NamedViews.Add(name: valid, viewportId: scope.Viewport.Id) switch {
                int value when value >= 0 => Fin.Succ(value: value),
                _ => Fin.Fail<int>(error: Op.Of(name: nameof(SaveNamed)).InvalidResult()),
            }
            select new Commands.DocumentResourceChange(Kind: Commands.DocumentResourceKind.NamedView, Name: valid));

    public static CameraOp<Unit> RestoreNamed(string name) =>
        RestoreNamed(name: name, restore: CameraNamedRestore.Direct);

    public static CameraOp<Unit> RestoreNamed(string name, CameraNamedRestore restore) =>
        new(Run: scope =>
            from index in NamedIndex(document: scope.Document, name: name, op: Op.Of(name: nameof(RestoreNamed)))
            from restored in restore.Apply(document: scope.Document, index: index, viewport: scope.Viewport)
            from redraw in scope.Redraw()
            select redraw);

    public static CameraOp<Commands.DocumentResourceChange> RenameNamed(string current, string next) =>
        new(Run: scope =>
            from index in NamedIndex(document: scope.Document, name: current, op: Op.Of(name: nameof(RenameNamed)))
            from name in Name(value: next, op: Op.Of(name: nameof(RenameNamed)))
            from renamed in Op.Of(name: nameof(RenameNamed)).Confirm(success: scope.Document.NamedViews.Rename(index: index, newName: name))
            select new Commands.DocumentResourceChange(Kind: Commands.DocumentResourceKind.NamedView, Name: name));

    public static CameraOp<Commands.DocumentResourceChange> DeleteNamed(string name) =>
        new(Run: scope =>
            from index in NamedIndex(document: scope.Document, name: name, op: Op.Of(name: nameof(DeleteNamed)))
            from valid in Name(value: name, op: Op.Of(name: nameof(DeleteNamed)))
            from deleted in Op.Of(name: nameof(DeleteNamed)).Confirm(success: scope.Document.NamedViews.Delete(index: index))
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
