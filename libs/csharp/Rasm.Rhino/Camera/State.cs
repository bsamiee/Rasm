using System.Runtime.InteropServices;
using DrawingRectangle = System.Drawing.Rectangle;
using DrawingSize = System.Drawing.Size;

namespace Rasm.Rhino.Camera;

// --- [TYPES] ------------------------------------------------------------------------------
public enum CameraMode { Perspective, Parallel, TwoPointPerspective }

// --- [MODELS] -----------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct CameraDepth(double Near, double Far);

[StructLayout(LayoutKind.Auto)]
public readonly record struct CameraSubject(
    Func<RhinoViewport, Fin<CameraDepth>> Depth,
    Func<RhinoViewport, Fin<bool>> Visible) {
    public static CameraSubject Point(Point3d point) {
        Op op = Op.Of(name: nameof(Point));
        return new(
            Depth: Guarded(value: point, ready: static p => p.IsValid, op: op, project: (vp, p) =>
                vp.GetDepth(point: p, distance: out double d) switch {
                    true => Fin.Succ(value: new CameraDepth(Near: d, Far: d)),
                    false => Fin.Fail<CameraDepth>(error: op.InvalidResult()),
                }),
            Visible: Guarded(value: point, ready: static p => p.IsValid, op: op, project: static (vp, p) => Fin.Succ(value: vp.IsVisible(point: p))));
    }

    public static CameraSubject Bounds(BoundingBox bounds) {
        Op op = Op.Of(name: nameof(Bounds));
        return new(
            Depth: Guarded(value: bounds, ready: static b => b.IsValid, op: op, project: (vp, b) =>
                vp.GetDepth(bbox: b, nearDistance: out double near, farDistance: out double far) switch {
                    true => Fin.Succ(value: new CameraDepth(Near: near, Far: far)),
                    false => Fin.Fail<CameraDepth>(error: op.InvalidResult()),
                }),
            Visible: Guarded(value: bounds, ready: static b => b.IsValid, op: op, project: static (vp, b) => Fin.Succ(value: vp.IsVisible(bbox: b))));
    }

    public static CameraSubject Sphere(Sphere sphere) {
        Op op = Op.Of(name: nameof(Sphere));
        return new(
            Depth: Guarded(value: sphere, ready: static s => s.IsValid, op: op, project: (vp, s) =>
                vp.GetDepth(sphere: s, nearDistance: out double near, farDistance: out double far) switch {
                    true => Fin.Succ(value: new CameraDepth(Near: near, Far: far)),
                    false => Fin.Fail<CameraDepth>(error: op.InvalidResult()),
                }),
            Visible: Guarded(value: sphere.BoundingBox, ready: static b => b.IsValid, op: op, project: static (vp, b) => Fin.Succ(value: vp.IsVisible(bbox: b))));
    }

    public static CameraSubject Geometry(GeometryBase geometry) =>
        Optional(geometry).Map(static valid => Bounds(bounds: valid.GetBoundingBox(accurate: true))).IfNone(new CameraSubject(
            Depth: static _ => Fin.Fail<CameraDepth>(error: Op.Of(name: nameof(Geometry)).InvalidInput()),
            Visible: static _ => Fin.Fail<bool>(error: Op.Of(name: nameof(Geometry)).InvalidInput())));

    private static Func<RhinoViewport, Fin<T>> Guarded<TInput, T>(TInput value, Func<TInput, bool> ready, Op op, Func<RhinoViewport, TInput, Fin<T>> project) =>
        viewport => UI.RhinoUi.Protect(valid: () => ready(arg: value) switch {
            true => project(arg1: viewport, arg2: value),
            false => Fin.Fail<T>(error: op.InvalidInput()),
        });
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct CameraScope(
    RhinoDoc Document,
    RhinoView View,
    RhinoViewport Viewport,
    Option<DetailViewObject> Detail = default) {
    public Fin<CameraSnapshot> Snapshot() => CameraSnapshot.Of(scope: this);

    public Fin<Transform> CoordinateTransform(CoordinateSystem sourceSystem, CoordinateSystem destinationSystem) {
        RhinoViewport viewport = Viewport;
        return UI.RhinoUi.Protect(valid: () => Fin.Succ(value: viewport.GetTransform(sourceSystem: sourceSystem, destinationSystem: destinationSystem)));
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
        return Optional(source.Depth).ToFin(Fail: Op.Of(name: nameof(Depth)).InvalidInput()).Bind(valid => valid(arg: viewport));
    }

    public Fin<bool> Visible(CameraSubject source) {
        RhinoViewport viewport = Viewport;
        return Optional(source.Visible).ToFin(Fail: Op.Of(name: nameof(Visible)).InvalidInput()).Bind(valid => valid(arg: viewport));
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
    public uint ChangeSerial { get; }

    public void Dispose() {
        projection.Dispose();
        GC.SuppressFinalize(obj: this);
    }

    internal static Fin<CameraSnapshot> Of(CameraScope scope) {
        Op op = Op.Of(name: nameof(CameraSnapshot));
        RhinoViewport viewport = scope.Viewport;
        return from frustum in CameraFrustum.Of(viewport: viewport, op: op)
               from frame in CameraFrame.Of(viewport: viewport)
               let projection = new ViewportInfo(rhinoViewport: viewport)
               select new CameraSnapshot(
                   scope: scope,
                   frame: frame,
                   frustum: frustum,
                   projection: projection,
                   constructionPlane: viewport.ConstructionPlane(),
                   displayMode: viewport.DisplayMode,
                   screenPort: projection.ScreenPort,
                   size: viewport.Size,
                   lockedProjection: viewport.LockedProjection,
                   mode: viewport switch {
                       { IsTwoPointPerspectiveProjection: true } => CameraMode.TwoPointPerspective,
                       { IsPerspectiveProjection: true } => CameraMode.Perspective,
                       _ => CameraMode.Parallel,
                   },
                   lensLength: viewport.Camera35mmLensLength,
                   cameraAngle: viewport.CameraAngle,
                   changeSerial: viewport.ChangeCounter);
    }
}
