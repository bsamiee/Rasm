using System.Runtime.InteropServices;
using DrawingBitmap = System.Drawing.Bitmap;
using DrawingPoint = System.Drawing.Point;
using DrawingRectangle = System.Drawing.Rectangle;
using DrawingSize = System.Drawing.Size;
using XmlDocument = System.Xml.XmlDocument;

namespace Rasm.Rhino.Camera;

// --- [TYPES] ------------------------------------------------------------------------------
public enum CameraMouseMove { RotateAroundTarget, RotateCamera, InOutDolly, Magnify, Tilt, DollyZoom, LateralDolly, }

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record CameraOp<T> {
    private readonly Func<CameraScope, Fin<T>> run;

    public CameraOp(Func<CameraScope, Fin<T>> run) =>
        this.run = run ?? throw new ArgumentNullException(paramName: nameof(run));

    internal Fin<T> Apply(CameraScope scope) =>
        run(arg: scope);

    public CameraOp<TNext> Map<TNext>(Func<T, TNext> project) =>
        new(run: scope => Optional(project).ToFin(Fail: Op.Of(name: nameof(Map)).InvalidInput()).Bind(valid => Apply(scope: scope).Map(valid)));

    public CameraOp<TNext> Bind<TNext>(Func<T, CameraOp<TNext>> bind) =>
        new(run: scope => Optional(bind).ToFin(Fail: Op.Of(name: nameof(Bind)).InvalidInput()).Bind(valid => Apply(scope: scope).Bind(value => Optional(valid(arg: value)).ToFin(Fail: Op.Of(name: nameof(Bind)).InvalidResult()).Bind(next => next.Apply(scope: scope)))));
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
               from valid in Optional(apply).ToFin(Fail: op.InvalidInput())
               from result in RhinoCamera.UnitResult(success: valid(arg: validViewport), op: op)
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
            from valid in Optional(change).ToFin(Fail: Op.Of(name: nameof(Native)).InvalidInput())
            from _ in Fin.Succ(value: ((Func<Unit>)(() => { valid(obj: scope.Viewport); return unit; }))())
            from result in Redraw(scope: scope, redraw: redraw)
            select result);

    public static CameraEdit Native(Func<RhinoViewport, bool> change) =>
        new(apply: (scope, redraw) =>
            from valid in Optional(change).ToFin(Fail: Op.Of(name: nameof(Native)).InvalidInput())
            from _ in RhinoCamera.UnitResult(success: valid(arg: scope.Viewport), op: Op.Of(name: nameof(Native)))
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
        Native(change: viewport => viewport.SetCameraDirection(cameraDirection: value, updateTargetLocation: updateTarget));

    public static CameraEdit Up(Vector3d value) =>
        Native(change: viewport => viewport.CameraUp = value);

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
        new(apply: (scope, redraw) => Rasm.Rhino.UI.RhinoUi.Protect(valid: () => {
            using ViewportInfo projection = new(scope.Viewport);
            return from authored in frustum.Apply(projection: projection, op: Op.Of(name: nameof(Frustum)))
                   from _ in RhinoCamera.UnitResult(success: scope.Viewport.SetViewProjection(projection: authored, updateTargetLocation: updateTarget), op: Op.Of(name: nameof(Frustum)))
                   from result in Redraw(scope: scope, redraw: redraw)
                   select result;
        }));

    public static CameraEdit Plane(global::Rhino.Geometry.Plane plane) =>
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
        Native(change: viewport => viewport.Rotate(angleRadians: radians, rotationAxis: axis, rotationCenter: center));

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
        Native(change: viewport => move switch {
            CameraMouseMove.RotateAroundTarget => viewport.MouseRotateAroundTarget(mousePreviousPoint: previous, mouseCurrentPoint: current),
            CameraMouseMove.RotateCamera => viewport.MouseRotateCamera(mousePreviousPoint: previous, mouseCurrentPoint: current),
            CameraMouseMove.InOutDolly => viewport.MouseInOutDolly(mousePreviousPoint: previous, mouseCurrentPoint: current),
            CameraMouseMove.Magnify => viewport.MouseMagnify(mousePreviousPoint: previous, mouseCurrentPoint: current),
            CameraMouseMove.Tilt => viewport.MouseTilt(mousePreviousPoint: previous, mouseCurrentPoint: current),
            CameraMouseMove.DollyZoom => viewport.MouseDollyZoom(mousePreviousPoint: previous, mouseCurrentPoint: current),
            CameraMouseMove.LateralDolly => viewport.MouseLateralDolly(mousePreviousPoint: previous, mouseCurrentPoint: current),
            _ => false,
        });

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
        redraw switch {
            true => scope.Redraw(),
            false => Fin.Succ(value: unit),
        };
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct CameraNamedRestore(Func<RhinoDoc, int, RhinoViewport, Fin<Unit>> Run) {
    public static CameraNamedRestore Direct { get; } =
        new(Run: static (document, index, viewport) => RhinoCamera.UnitResult(success: document.NamedViews.Restore(index: index, viewport: viewport), op: Op.Of(name: nameof(Direct))));

    public static CameraNamedRestore MatchAspect { get; } =
        new(Run: static (document, index, viewport) => RhinoCamera.UnitResult(success: document.NamedViews.RestoreWithAspectRatio(index: index, viewport: viewport), op: Op.Of(name: nameof(MatchAspect))));

    public static CameraNamedRestore ConstantSpeed(double unitsPerFrame, int delayMilliseconds) =>
        new(Run: (document, index, viewport) => RhinoCamera.UnitResult(success: document.NamedViews.RestoreAnimatedConstantSpeed(index, viewport, unitsPerFrame, delayMilliseconds), op: Op.Of(name: nameof(ConstantSpeed))));

    public static CameraNamedRestore ConstantTime(int frames, int delayMilliseconds) =>
        new(Run: (document, index, viewport) => RhinoCamera.UnitResult(success: document.NamedViews.RestoreAnimatedConstantTime(index, viewport, frames, delayMilliseconds), op: Op.Of(name: nameof(ConstantTime))));

    internal Fin<Unit> Apply(RhinoDoc document, int index, RhinoViewport viewport) =>
        (Optional(Run) | Some(Direct.Run)).ToFin(Fail: Op.Of(name: nameof(CameraNamedRestore)).InvalidInput()).Bind(run => run(arg1: document, arg2: index, arg3: viewport));
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
        DrawingSize size = Size;
        double dpi = Dpi;
        ViewCaptureSettings.ViewAreaMapping area = Area;
        bool drawBackground = DrawBackground;
        bool drawGrid = DrawGrid;
        bool drawAxis = DrawAxis;
        bool drawLockedObjects = DrawLockedObjects;
        bool drawSelectedObjectsOnly = DrawSelectedObjectsOnly;
        bool drawClippingPlanes = DrawClippingPlanes;
        bool drawLights = DrawLights;
        bool drawWallpaper = DrawWallpaper;
        bool usePrintWidths = UsePrintWidths;
        bool applyDisplayModeThicknessScales = ApplyDisplayModeThicknessScales;
        ViewCaptureSettings.ColorMode outputColor = OutputColor;
        Option<DrawingRectangle> crop = Crop;
        Option<(Point2d A, Point2d B)> window = Window;
        return from valid in Optional(project).ToFin(Fail: Op.Of(name: nameof(Capture)).InvalidInput())
               from result in Rasm.Rhino.UI.RhinoUi.Protect(valid: () => {
                   ViewCaptureSettings settings = new(sourceView: scope.View, mediaSize: size, dpi: dpi) {
                       ViewArea = area,
                       DrawBackground = drawBackground,
                       DrawGrid = drawGrid,
                       DrawAxis = drawAxis,
                       DrawLockedObjects = drawLockedObjects,
                       DrawSelectedObjectsOnly = drawSelectedObjectsOnly,
                       DrawClippingPlanes = drawClippingPlanes,
                       DrawLights = drawLights,
                       DrawWallpaper = drawWallpaper,
                       UsePrintWidths = usePrintWidths,
                       ApplyDisplayModeThicknessScales = applyDisplayModeThicknessScales,
                       OutputColor = outputColor,
                   };
                   settings.SetViewport(viewport: scope.Viewport);
                   _ = crop.Iter(rectangle => settings.SetLayout(size, rectangle));
                   _ = window.Iter(points => settings.SetWindowRect(screenPoint1: points.A, screenPoint2: points.B));
                   // BOUNDARY ADAPTER - ViewCaptureSettings owns native print info and must always be released.
                   try {
                       return settings.IsValid switch {
                           true => valid(arg: settings),
                           false => Fin.Fail<T>(error: Op.Of(name: nameof(CameraCapture)).InvalidResult()),
                       };
                   } finally { settings.Dispose(); }
               })
               select result;
    }
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class CameraOps {
    public static CameraOp<T> Query<T>(Func<CameraScope, Fin<T>> query) =>
        new(run: scope => Optional(query).ToFin(Fail: Op.Of(name: nameof(Query)).InvalidInput()).Bind(valid => valid(arg: scope)));

    public static CameraOp<T> Read<T>(Func<CameraSnapshot, Fin<T>> project) =>
        new(run: scope => Optional(project).ToFin(Fail: Op.Of(name: nameof(Read)).InvalidInput()).Bind(valid => scope.Snapshot().Bind(snapshot => {
            // BOUNDARY ADAPTER - snapshot owns native ViewportInfo only for the projection callback.
            try { return valid(arg: snapshot); } finally { snapshot.Dispose(); }
        })));

    public static CameraOp<Unit> Change(params CameraEdit[] edits) =>
        Change(edits: edits, redrawEach: true);

    public static CameraOp<Unit> Change(IEnumerable<CameraEdit> edits, bool redrawEach = true) =>
        new(run: scope =>
            from changes in Optional(edits).ToFin(Fail: Op.Of(name: nameof(Change)).InvalidInput()).Map(static source => toSeq(source))
            from applied in changes.TraverseM(edit => edit.Apply(scope: scope, redraw: redrawEach)).As()
            from redraw in redrawEach switch {
                true => Fin.Succ(value: unit),
                false => scope.Redraw(),
            }
            select redraw);

    public static CameraOp<Rasm.Rhino.Commands.DocumentResourceChange> SaveNamed(string name) =>
        new(run: scope =>
            from valid in Name(value: name, op: Op.Of(name: nameof(SaveNamed)))
            from index in scope.Document.NamedViews.Add(name: valid, viewportId: scope.Viewport.Id) switch {
                int value when value >= 0 => Fin.Succ(value: value),
                _ => Fin.Fail<int>(error: Op.Of(name: nameof(SaveNamed)).InvalidResult()),
            }
            select new Rasm.Rhino.Commands.DocumentResourceChange(Kind: Rasm.Rhino.Commands.DocumentResourceKind.View, Name: valid));

    public static CameraOp<Unit> RestoreNamed(string name) =>
        RestoreNamed(name: name, restore: CameraNamedRestore.Direct);

    public static CameraOp<Unit> RestoreNamed(string name, CameraNamedRestore restore) =>
        new(run: scope =>
            from index in NamedIndex(document: scope.Document, name: name, op: Op.Of(name: nameof(RestoreNamed)))
            from restored in restore.Apply(document: scope.Document, index: index, viewport: scope.Viewport)
            from redraw in scope.Redraw()
            select redraw);

    public static CameraOp<Rasm.Rhino.Commands.DocumentResourceChange> RenameNamed(string current, string next) =>
        new(run: scope =>
            from index in NamedIndex(document: scope.Document, name: current, op: Op.Of(name: nameof(RenameNamed)))
            from name in Name(value: next, op: Op.Of(name: nameof(RenameNamed)))
            from renamed in RhinoCamera.UnitResult(success: scope.Document.NamedViews.Rename(index: index, newName: name), op: Op.Of(name: nameof(RenameNamed)))
            select new Rasm.Rhino.Commands.DocumentResourceChange(Kind: Rasm.Rhino.Commands.DocumentResourceKind.View, Name: name));

    public static CameraOp<Rasm.Rhino.Commands.DocumentResourceChange> DeleteNamed(string name) =>
        new(run: scope =>
            from index in NamedIndex(document: scope.Document, name: name, op: Op.Of(name: nameof(DeleteNamed)))
            from valid in Name(value: name, op: Op.Of(name: nameof(DeleteNamed)))
            from deleted in RhinoCamera.UnitResult(success: scope.Document.NamedViews.Delete(index: index), op: Op.Of(name: nameof(DeleteNamed)))
            select new Rasm.Rhino.Commands.DocumentResourceChange(Kind: Rasm.Rhino.Commands.DocumentResourceKind.View, Name: valid));

    public static CameraOp<DrawingBitmap> CaptureBitmap(CameraCapture capture) =>
        new(run: scope => capture.Capture(scope: scope, project: static settings => Optional(ViewCapture.CaptureToBitmap(settings: settings)).ToFin(Fail: Op.Of(name: nameof(CaptureBitmap)).InvalidResult())));

    public static CameraOp<XmlDocument> CaptureSvg(CameraCapture capture) =>
        new(run: scope => capture.Capture(scope: scope, project: static settings => Optional(ViewCapture.CaptureToSvg(settings: settings)).ToFin(Fail: Op.Of(name: nameof(CaptureSvg)).InvalidResult())));

    private static Fin<int> NamedIndex(RhinoDoc document, string name, Op op) =>
        from valid in Name(value: name, op: op)
        from index in document.NamedViews.FindByName(name: valid) switch {
            int value when value >= 0 => Fin.Succ(value: value),
            _ => Fin.Fail<int>(error: op.MissingContext()),
        }
        select index;

    private static Fin<string> Name(string value, Op op) =>
        string.IsNullOrWhiteSpace(value: value) switch {
            false => Fin.Succ(value: value.Trim()),
            true => Fin.Fail<string>(error: op.InvalidInput()),
        };
}
