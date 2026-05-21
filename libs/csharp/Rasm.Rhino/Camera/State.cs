using System.Runtime.InteropServices;
using DrawingRectangle = System.Drawing.Rectangle;
using DrawingSize = System.Drawing.Size;

namespace Rasm.Rhino.Camera;

// --- [MODELS] -----------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct CameraDepth(double Near, double Far);

[StructLayout(LayoutKind.Auto)]
public readonly record struct CameraSubject(
    Func<RhinoViewport, Fin<CameraDepth>> Depth,
    Func<RhinoViewport, Fin<bool>> Visible) {
    public static CameraSubject Point(Point3d point) =>
        new(
            Depth: viewport => UI.RhinoUi.Protect(valid: () => point.IsValid switch {
                true => viewport.GetDepth(point: point, distance: out double distance) switch {
                    true => Fin.Succ(value: new CameraDepth(Near: distance, Far: distance)),
                    false => Fin.Fail<CameraDepth>(error: Op.Of(name: nameof(Point)).InvalidResult()),
                },
                false => Fin.Fail<CameraDepth>(error: Op.Of(name: nameof(Point)).InvalidInput()),
            }),
            Visible: viewport => UI.RhinoUi.Protect(valid: () => point.IsValid switch {
                true => Fin.Succ(value: viewport.IsVisible(point: point)),
                false => Fin.Fail<bool>(error: Op.Of(name: nameof(Point)).InvalidInput()),
            }));

    public static CameraSubject Bounds(BoundingBox bounds) =>
        new(
            Depth: viewport => UI.RhinoUi.Protect(valid: () => bounds.IsValid switch {
                true => viewport.GetDepth(bbox: bounds, nearDistance: out double near, farDistance: out double far) switch {
                    true => Fin.Succ(value: new CameraDepth(Near: near, Far: far)),
                    false => Fin.Fail<CameraDepth>(error: Op.Of(name: nameof(Bounds)).InvalidResult()),
                },
                false => Fin.Fail<CameraDepth>(error: Op.Of(name: nameof(Bounds)).InvalidInput()),
            }),
            Visible: viewport => UI.RhinoUi.Protect(valid: () => bounds.IsValid switch {
                true => Fin.Succ(value: viewport.IsVisible(bbox: bounds)),
                false => Fin.Fail<bool>(error: Op.Of(name: nameof(Bounds)).InvalidInput()),
            }));

    public static CameraSubject Sphere(Sphere sphere) =>
        new(
            Depth: viewport => UI.RhinoUi.Protect(valid: () => sphere.IsValid switch {
                true => viewport.GetDepth(sphere: sphere, nearDistance: out double near, farDistance: out double far) switch {
                    true => Fin.Succ(value: new CameraDepth(Near: near, Far: far)),
                    false => Fin.Fail<CameraDepth>(error: Op.Of(name: nameof(Sphere)).InvalidResult()),
                },
                false => Fin.Fail<CameraDepth>(error: Op.Of(name: nameof(Sphere)).InvalidInput()),
            }),
            Visible: viewport => Bounds(bounds: sphere.BoundingBox).Visible(arg: viewport));

    public static CameraSubject Geometry(GeometryBase geometry) =>
        Optional(geometry).Map(valid => Bounds(bounds: valid.GetBoundingBox(accurate: true))).IfNone(new CameraSubject(
            Depth: static _ => Fin.Fail<CameraDepth>(error: Op.Of(name: nameof(Geometry)).InvalidInput()),
            Visible: static _ => Fin.Fail<bool>(error: Op.Of(name: nameof(Geometry)).InvalidInput())));
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
                _ => Fin.Succ(value: ((Func<Unit>)(() => { view.Redraw(); return unit; }))()),
            });
    }

}

[StructLayout(LayoutKind.Auto)]
public readonly record struct CameraFrame(
    Point3d Location,
    Point3d Target,
    Vector3d Direction,
    Vector3d Up,
    Vector3d X,
    Vector3d Y,
    Vector3d Z) {
    public static CameraFrame Of(RhinoViewport viewport) {
        ArgumentNullException.ThrowIfNull(argument: viewport);
        return new(
            Location: viewport.CameraLocation,
            Target: viewport.CameraTarget,
            Direction: viewport.CameraDirection,
            Up: viewport.CameraUp,
            X: viewport.CameraX,
            Y: viewport.CameraY,
            Z: viewport.CameraZ);
    }

    public static Fin<CameraFrame> LookAt(Point3d location, Point3d target, Option<Vector3d> up = default) {
        Vector3d direction = target - location;
        Vector3d unitDirection = direction;
        _ = unitDirection.Unitize();
        Vector3d fallbackUp = Math.Abs(Vector3d.Multiply(unitDirection, Vector3d.ZAxis)) switch {
            double dot when dot > 1.0 - RhinoMath.ZeroTolerance => Vector3d.YAxis,
            _ => Vector3d.ZAxis,
        };
        Vector3d sourceUp = up.Filter(static value => value.IsValid && value.Length > RhinoMath.ZeroTolerance).IfNone(fallbackUp);
        _ = sourceUp.Unitize();
        Vector3d x = Vector3d.CrossProduct(sourceUp, unitDirection);
        _ = x.Unitize();
        Vector3d y = Vector3d.CrossProduct(unitDirection, x);
        _ = y.Unitize();
        return (location.IsValid && target.IsValid && direction.IsValid && direction.Length > RhinoMath.ZeroTolerance && x.IsValid && x.Length > RhinoMath.ZeroTolerance && y.IsValid && y.Length > RhinoMath.ZeroTolerance) switch {
            true => Fin.Succ(value: ((Func<CameraFrame>)(() => new CameraFrame(Location: location, Target: target, Direction: unitDirection, Up: y, X: x, Y: y, Z: -unitDirection)))()),
            false => Fin.Fail<CameraFrame>(error: Op.Of(name: nameof(LookAt)).InvalidInput()),
        };
    }

    internal Fin<Unit> Apply(RhinoViewport viewport, Op op) {
        Point3d location = Location;
        Point3d target = Target;
        Vector3d direction = Direction;
        Vector3d up = Up;
        return Optional(viewport).ToFin(Fail: op.InvalidInput()).Map(valid => {
            valid.SetCameraLocations(targetLocation: target, cameraLocation: location);
            valid.SetCameraDirection(cameraDirection: direction, updateTargetLocation: true);
            valid.CameraUp = up;
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
        double left = Left;
        double right = Right;
        double bottom = Bottom;
        double top = Top;
        double near = Near;
        double far = Far;
        return from valid in Optional(projection).ToFin(Fail: op.InvalidInput())
               from _ in RhinoCamera.UnitResult(success: valid.SetFrustum(left: left, right: right, bottom: bottom, top: top, nearDistance: near, farDistance: far), op: op)
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
        bool perspective,
        bool parallel,
        bool twoPointPerspective,
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
        IsPerspective = perspective;
        IsParallel = parallel;
        IsTwoPointPerspective = twoPointPerspective;
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
    public bool IsPerspective { get; }
    public bool IsParallel { get; }
    public bool IsTwoPointPerspective { get; }
    public double LensLength { get; }
    public double CameraAngle { get; }
    public uint ChangeSerial { get; }

    public void Dispose() {
        projection.Dispose();
        GC.SuppressFinalize(obj: this);
    }

    internal static Fin<CameraSnapshot> Of(CameraScope scope) {
        Op op = Op.Of(name: nameof(CameraSnapshot));
        return CameraFrustum.Of(viewport: scope.Viewport, op: op).Map(frustum => ((Func<CameraSnapshot>)(() => {
            ViewportInfo nativeProjection = new(scope.Viewport);
            return new CameraSnapshot(
                scope: scope,
                frame: CameraFrame.Of(viewport: scope.Viewport),
                frustum: frustum,
                projection: nativeProjection,
                constructionPlane: scope.Viewport.ConstructionPlane(),
                displayMode: scope.Viewport.DisplayMode,
                screenPort: nativeProjection.ScreenPort,
                size: scope.Viewport.Size,
                lockedProjection: scope.Viewport.LockedProjection,
                perspective: scope.Viewport.IsPerspectiveProjection,
                parallel: scope.Viewport.IsParallelProjection,
                twoPointPerspective: scope.Viewport.IsTwoPointPerspectiveProjection,
                lensLength: scope.Viewport.Camera35mmLensLength,
                cameraAngle: scope.Viewport.CameraAngle,
                changeSerial: scope.Viewport.ChangeCounter);
        }))());
    }
}
