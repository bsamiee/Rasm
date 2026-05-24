using System.Runtime.InteropServices;
using DrawingRectangle = System.Drawing.Rectangle;
using DrawingSize = System.Drawing.Size;

namespace Rasm.Rhino.Camera;

// --- [TYPES] ------------------------------------------------------------------------------
public enum CameraMode { Perspective, Parallel, TwoPointPerspective }

// --- [MODELS] -----------------------------------------------------------------------------
[Union(SwitchMapStateParameterName = "context")]
public abstract partial record ViewportTarget {
    private ViewportTarget() { }
    public sealed record Current : ViewportTarget;
    public sealed record Id(Guid Value) : ViewportTarget;
    public sealed record View(RhinoView Value) : ViewportTarget;

    internal Fin<CameraScope> Resolve(RhinoDoc document, Op op) =>
        Switch(
            (Document: document, Op: op),
            current: static (ctx, _) =>
                from view in Optional(ctx.Document.Views.ActiveView).ToFin(Fail: ctx.Op.MissingContext())
                select CameraScope.Of(document: ctx.Document, view: view, viewport: view.ActiveViewport),
            id: static (ctx, target) =>
                target.Value switch {
                    Guid id when id != Guid.Empty =>
                        Optional(ctx.Document.Views.Find(mainViewportId: id)).Case switch {
                            RhinoView view => Fin.Succ(value: CameraScope.Of(document: ctx.Document, view: view, viewport: view.MainViewport)),
                            _ => toSeq(ctx.Document.Views.GetPageViews() ?? [])
                                .Bind(static page => toSeq(page.GetDetailViews()).Map(detail => (Page: page, Detail: detail)))
                                .Find(row => row.Detail.Id == id || row.Detail.Viewport.Id == id)
                                .Map(row => CameraScope.Of(document: ctx.Document, view: row.Page, viewport: row.Detail.Viewport))
                                .ToFin(Fail: ctx.Op.MissingContext()),
                        },
                    _ => Fin.Fail<CameraScope>(error: ctx.Op.InvalidInput()),
                },
            view: static (ctx, target) =>
                from view in Optional(target.Value).ToFin(Fail: ctx.Op.InvalidInput())
                from _ in view.Document?.RuntimeSerialNumber == ctx.Document.RuntimeSerialNumber
                    ? Fin.Succ(value: unit)
                    : Fin.Fail<Unit>(error: ctx.Op.InvalidInput())
                select CameraScope.Of(document: ctx.Document, view: view, viewport: view.ActiveViewport));
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct CameraDepth(double Near, double Far);

[Union(SwitchMapStateParameterName = "viewport")]
public abstract partial record CameraSubject {
    private CameraSubject() { }
    public sealed record AtPoint(Point3d Value) : CameraSubject;
    public sealed record InBounds(BoundingBox Value) : CameraSubject;
    public sealed record InSphere(Sphere Value) : CameraSubject;
    public sealed record FromGeometry(GeometryBase Value) : CameraSubject;

    public static CameraSubject Point(Point3d point) => new AtPoint(Value: point);
    public static CameraSubject Bounds(BoundingBox bounds) => new InBounds(Value: bounds);
    public static CameraSubject Sphere(Sphere sphere) => new InSphere(Value: sphere);
    public static CameraSubject Geometry(GeometryBase geometry) => new FromGeometry(Value: geometry);

    internal Fin<CameraDepth> Depth(RhinoViewport viewport) {
        Op op = Op.Of(name: nameof(Depth));
        return Switch(
            (Viewport: viewport, Op: op),
            atPoint: static (ctx, source) => UI.RhinoUi.Protect(valid: () =>
                source.Value.IsValid switch {
                    false => Fin.Fail<CameraDepth>(error: ctx.Op.InvalidInput()),
                    true => ctx.Viewport.GetDepth(point: source.Value, distance: out double distance)
                        ? Fin.Succ(value: new CameraDepth(Near: distance, Far: distance))
                        : Fin.Fail<CameraDepth>(error: ctx.Op.InvalidResult()),
                }),
            inBounds: static (ctx, source) => UI.RhinoUi.Protect(valid: () =>
                source.Value.IsValid switch {
                    false => Fin.Fail<CameraDepth>(error: ctx.Op.InvalidInput()),
                    true => ctx.Viewport.GetDepth(bbox: source.Value, nearDistance: out double near, farDistance: out double far)
                        ? Fin.Succ(value: new CameraDepth(Near: near, Far: far))
                        : Fin.Fail<CameraDepth>(error: ctx.Op.InvalidResult()),
                }),
            inSphere: static (ctx, source) => UI.RhinoUi.Protect(valid: () =>
                source.Value.IsValid switch {
                    false => Fin.Fail<CameraDepth>(error: ctx.Op.InvalidInput()),
                    true => ctx.Viewport.GetDepth(sphere: source.Value, nearDistance: out double near, farDistance: out double far)
                        ? Fin.Succ(value: new CameraDepth(Near: near, Far: far))
                        : Fin.Fail<CameraDepth>(error: ctx.Op.InvalidResult()),
                }),
            fromGeometry: static (ctx, source) =>
                source.Value.BoundsOf(op: ctx.Op).Bind(bounds => Bounds(bounds: bounds).Depth(viewport: ctx.Viewport)));
    }

    internal Fin<bool> Visible(RhinoViewport viewport) {
        Op op = Op.Of(name: nameof(Visible));
        return Switch(
            (Viewport: viewport, Op: op),
            atPoint: static (ctx, source) => UI.RhinoUi.Protect(valid: () =>
                source.Value.IsValid
                    ? Fin.Succ(value: ctx.Viewport.IsVisible(point: source.Value))
                    : Fin.Fail<bool>(error: ctx.Op.InvalidInput())),
            inBounds: static (ctx, source) => UI.RhinoUi.Protect(valid: () =>
                source.Value.IsValid
                    ? Fin.Succ(value: ctx.Viewport.IsVisible(bbox: source.Value))
                    : Fin.Fail<bool>(error: ctx.Op.InvalidInput())),
            inSphere: static (ctx, source) => UI.RhinoUi.Protect(valid: () =>
                source.Value.IsValid
                    ? Fin.Succ(value: ctx.Viewport.IsVisible(bbox: source.Value.BoundingBox))
                    : Fin.Fail<bool>(error: ctx.Op.InvalidInput())),
            fromGeometry: static (ctx, source) =>
                source.Value.BoundsOf(op: ctx.Op).Bind(bounds => Bounds(bounds: bounds).Visible(viewport: ctx.Viewport)));
    }
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct CameraScope(
    RhinoDoc Document,
    RhinoView View,
    RhinoViewport Viewport,
    Option<DetailViewObject> Detail = default) {
    internal static CameraScope Of(RhinoDoc document, RhinoView view, RhinoViewport viewport) =>
        new(
            Document: document,
            View: view,
            Viewport: viewport,
            Detail: view is RhinoPageView page
                ? toSeq(page.GetDetailViews()).Find(detail => detail.Id == viewport.Id || detail.Viewport.Id == viewport.Id)
                : Option<DetailViewObject>.None);

    public Fin<CameraSnapshot> Snapshot() => CameraSnapshot.Of(scope: this);

    public Fin<Transform> CoordinateTransform(CoordinateSystem sourceSystem, CoordinateSystem destinationSystem) {
        RhinoViewport viewport = Viewport;
        return UI.RhinoUi.Protect(valid: () => {
            using ViewportInfo projection = new(rhinoViewport: viewport);
            Transform transform = projection.GetXform(sourceSystem: sourceSystem, destinationSystem: destinationSystem);
            return transform.IsValid
                ? Fin.Succ(value: transform)
                : Fin.Fail<Transform>(error: Op.Of(name: nameof(CoordinateTransform)).InvalidResult());
        });
    }

    public Fin<Line> FrustumLine(double screenX, double screenY) {
        RhinoViewport viewport = Viewport;
        return UI.RhinoUi.Protect(valid: () => viewport.GetFrustumLine(screenX: screenX, screenY: screenY, worldLine: out Line line) switch {
            true when line.IsValid && line != Line.Unset => Fin.Succ(value: line),
            _ => Fin.Fail<Line>(error: Op.Of(name: nameof(FrustumLine)).InvalidResult()),
        });
    }

    public Fin<CameraDepth> Depth(CameraSubject source) {
        RhinoViewport viewport = Viewport;
        return Optional(source).ToFin(Fail: Op.Of(name: nameof(Depth)).InvalidInput()).Bind(valid => valid.Depth(viewport: viewport));
    }

    public Fin<bool> Visible(CameraSubject source) {
        RhinoViewport viewport = Viewport;
        return Optional(source).ToFin(Fail: Op.Of(name: nameof(Visible)).InvalidInput()).Bind(valid => valid.Visible(viewport: viewport));
    }

    public Fin<double> TargetDistance(bool useFrustumCenterFallback = true) {
        RhinoViewport viewport = Viewport;
        return UI.RhinoUi.Protect(valid: () => {
            using ViewportInfo projection = new(rhinoViewport: viewport);
            double distance = projection.TargetDistance(useFrustumCenterFallback: useFrustumCenterFallback);
            return RhinoMath.IsValidDouble(x: distance)
                ? Fin.Succ(value: distance)
                : Fin.Fail<double>(error: Op.Of(name: nameof(TargetDistance)).InvalidResult());
        });
    }

    public Fin<double> WorldToScreenScale(Point3d point) {
        RhinoViewport viewport = Viewport;
        Op op = Op.Of(name: nameof(WorldToScreenScale));
        return from _ in guard(point.IsValid, op.InvalidInput())
               from scale in op.Catch(() => UI.RhinoUi.Protect(valid: () =>
                   viewport.GetWorldToScreenScale(pointInFrustum: point, pixelsPerUnit: out double pixels) switch {
                       true when RhinoMath.IsValidDouble(x: pixels) && pixels > 0.0 => Fin.Succ(value: pixels),
                       _ => Fin.Fail<double>(error: op.InvalidResult()),
                   }))
               select scale;
    }

    public Fin<Unit> Redraw() {
        RhinoView view = View;
        Option<DetailViewObject> detail = Detail;
        return UI.RhinoUi.Protect(valid: () =>
            detail.Case switch {
                DetailViewObject value when !value.CommitViewportChanges() => Fin.Fail<Unit>(error: Op.Of(name: nameof(Redraw)).InvalidResult()),
                _ => Fin.Succ(value: unit).Map(_ => { view.Redraw(); return unit; }),
            });
    }
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct CameraFrame(Plane Frame, Point3d Target) {
    public Point3d Location => Frame.Origin;
    public Vector3d Direction => -Frame.ZAxis;
    public Vector3d Up => Frame.YAxis;

    public static Fin<CameraFrame> Of(RhinoViewport viewport) =>
        Optional(viewport).ToFin(Fail: Op.Of(name: nameof(Of)).InvalidInput())
            .Bind(valid => valid.GetCameraFrame(frame: out Plane plane) switch {
                true => Fin.Succ(value: new CameraFrame(Frame: plane, Target: valid.CameraTarget)),
                false => Fin.Fail<CameraFrame>(error: Op.Of(name: nameof(Of)).InvalidResult()),
            });

    public static Fin<CameraFrame> LookAt(Point3d location, Point3d target, Option<Vector3d> up = default) =>
        from direction in (location.IsValid, target.IsValid, target - location) switch {
            (true, true, Vector3d d) when d.Length > RhinoMath.ZeroTolerance => Fin.Succ(value: d),
            _ => Fin.Fail<Vector3d>(error: Op.Of(name: nameof(LookAt)).InvalidInput()),
        }
        let resolved = up.Filter(static value => value.IsValid && value.Length > RhinoMath.ZeroTolerance)
                         .IfNone(() => ViewportInfo.CalculateCameraUpDirection(location: location, direction: direction, angle: 0.0))
        let plane = new Plane(origin: location, xDirection: Vector3d.CrossProduct(direction, resolved), yDirection: resolved)
        from valid in plane.IsValid switch {
            true => Fin.Succ(value: plane),
            false => Fin.Fail<Plane>(error: Op.Of(name: nameof(LookAt)).InvalidResult()),
        }
        select new CameraFrame(Frame: valid, Target: target);

    internal Fin<Unit> Apply(RhinoViewport viewport, Op op) {
        Plane frame = Frame;
        Point3d target = Target;
        return Optional(viewport).ToFin(Fail: op.InvalidInput()).Map(valid => {
            valid.SetCameraLocations(targetLocation: target, cameraLocation: frame.Origin);
            valid.SetCameraDirection(cameraDirection: -frame.ZAxis, updateTargetLocation: true);
            valid.CameraUp = frame.YAxis;
            return unit;
        });
    }
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct CameraFrustum(
    double Left,
    double Right,
    double Bottom,
    double Top,
    double Near,
    double Far,
    double Aspect,
    BoundingBox Bounds) {
    public static Fin<CameraFrustum> Of(RhinoViewport viewport, Op op) =>
        Optional(viewport).ToFin(Fail: op.InvalidInput()).Bind(valid =>
            valid.GetFrustum(left: out double left, right: out double right, bottom: out double bottom, top: out double top, nearDistance: out double near, farDistance: out double far) switch {
                true => Fin.Succ(value: new CameraFrustum(Left: left, Right: right, Bottom: bottom, Top: top, Near: near, Far: far, Aspect: valid.FrustumAspect, Bounds: valid.GetFrustumBoundingBox())),
                false => Fin.Fail<CameraFrustum>(error: op.InvalidResult()),
            });

    internal Fin<ViewportInfo> Apply(ViewportInfo projection, Op op) {
        CameraFrustum self = this;
        return from valid in Optional(projection).ToFin(Fail: op.InvalidInput())
               from _ in op.Confirm(success: valid.SetFrustum(left: self.Left, right: self.Right, bottom: self.Bottom, top: self.Top, nearDistance: self.Near, farDistance: self.Far))
               select valid;
    }
}

public sealed record CameraSnapshot : IDisposable {
    private readonly ViewportInfo projection;

    private CameraSnapshot(
        CameraScope scope,
        CameraFrame frame,
        CameraFrustum frustum,
        ViewportInfo projection,
        Plane constructionPlane,
        DisplayModeDescription displayMode,
        DrawingRectangle screenPort,
        DrawingSize size,
        bool lockedProjection,
        CameraMode mode,
        double lensLength,
        double cameraAngle,
        uint documentSerial,
        uint changeSerial) {
        Scope = scope;
        Frame = frame;
        Frustum = frustum;
        this.projection = projection;
        Projection = projection;
        ConstructionPlane = constructionPlane;
        DisplayMode = displayMode;
        ScreenPort = screenPort;
        Size = size;
        LockedProjection = lockedProjection;
        Mode = mode;
        LensLength = lensLength;
        CameraAngle = cameraAngle;
        DocumentSerial = documentSerial;
        ChangeSerial = changeSerial;
    }

    public CameraScope Scope { get; }
    public CameraFrame Frame { get; }
    public CameraFrustum Frustum { get; }
    public ViewportInfo Projection { get; }
    public Plane ConstructionPlane { get; }
    public DisplayModeDescription DisplayMode { get; }
    public DrawingRectangle ScreenPort { get; }
    public DrawingSize Size { get; }
    public bool LockedProjection { get; }
    public CameraMode Mode { get; }
    public double LensLength { get; }
    public double CameraAngle { get; }
    public uint DocumentSerial { get; }
    public uint ChangeSerial { get; }

    // `ChangeCounter` advances on every viewport mutation — cheapest staleness probe is uint equality.
    // `DocumentSerial` guard catches doc-close/reopen and `ChangeCounter` uint wraparound.
    public bool IsStale =>
        Scope.Document.RuntimeSerialNumber != DocumentSerial || Scope.Viewport.ChangeCounter != ChangeSerial;

    public void Dispose() {
        projection.Dispose();
        GC.SuppressFinalize(obj: this);
    }

    internal static Fin<CameraSnapshot> Of(CameraScope scope) {
        Op op = Op.Of(name: nameof(CameraSnapshot));
        RhinoViewport viewport = scope.Viewport;
        return op.Catch(() => {
            ViewportInfo? projection = null;
            try {
                projection = new ViewportInfo(rhinoViewport: viewport);
                ViewportInfo captured = projection;
                return from frustum in CameraFrustum.Of(viewport: viewport, op: op)
                       from frame in CameraFrame.Of(viewport: viewport)
                       select new CameraSnapshot(
                           scope: scope,
                           frame: frame,
                           frustum: frustum,
                           projection: captured,
                           constructionPlane: viewport.ConstructionPlane(),
                           displayMode: viewport.DisplayMode,
                           screenPort: captured.ScreenPort,
                           size: viewport.Size,
                           lockedProjection: viewport.LockedProjection,
                           mode: viewport switch {
                               { IsTwoPointPerspectiveProjection: true } => CameraMode.TwoPointPerspective,
                               { IsPerspectiveProjection: true } => CameraMode.Perspective,
                               _ => CameraMode.Parallel,
                           },
                           lensLength: viewport.Camera35mmLensLength,
                           cameraAngle: viewport.CameraAngle,
                           documentSerial: scope.Document.RuntimeSerialNumber,
                           changeSerial: viewport.ChangeCounter);
            } catch {
                projection?.Dispose();
                throw;
            }
        });
    }
}
