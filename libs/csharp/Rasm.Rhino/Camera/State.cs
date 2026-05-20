using System.Runtime.InteropServices;
using DrawingRectangle = System.Drawing.Rectangle;
using DrawingSize = System.Drawing.Size;

namespace Rasm.Rhino.Camera;

[StructLayout(LayoutKind.Auto)]
public readonly record struct CameraScope(
    RhinoDoc Document,
    RhinoView View,
    RhinoViewport Viewport,
    Option<DetailViewObject> Detail = default) {
    public Fin<CameraSnapshot> Snapshot() => CameraSnapshot.Of(scope: this);

    public Fin<Unit> Redraw() {
        RhinoView view = View;
        Option<DetailViewObject> detail = Detail;
        return Rasm.Rhino.UI.RhinoUi.Protect(valid: () =>
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
        return (location.IsValid && target.IsValid && direction.IsValid && direction.Length > RhinoMath.ZeroTolerance) switch {
            true => Fin.Succ(value: ((Func<CameraFrame>)(() => {
                Vector3d unitDirection = direction;
                _ = unitDirection.Unitize();
                Vector3d cameraUp = up.Filter(static value => value.IsValid && value.Length > RhinoMath.ZeroTolerance).IfNone(Vector3d.ZAxis);
                Vector3d x = Vector3d.CrossProduct(unitDirection, cameraUp);
                Vector3d y = Vector3d.CrossProduct(x, unitDirection);
                return new CameraFrame(Location: location, Target: target, Direction: unitDirection, Up: cameraUp, X: x, Y: y, Z: -unitDirection);
            }))()),
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
