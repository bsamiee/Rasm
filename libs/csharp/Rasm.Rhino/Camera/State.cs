using System.Runtime.InteropServices;
using Rasm.Vectors;
using DrawingRectangle = System.Drawing.Rectangle;
using DrawingSize = System.Drawing.Size;

namespace Rasm.Rhino.Camera;

// --- [TYPES] ------------------------------------------------------------------------------
public enum CameraMode { Perspective, Parallel, TwoPointPerspective }

[Union(SwitchMapStateParameterName = "viewport")]
public abstract partial record CameraSubject {
    private static readonly Op DepthKey = Op.Of(name: nameof(Depth));
    private static readonly Op VisibleKey = Op.Of(name: nameof(Visible));
    private CameraSubject() { }
    public sealed record AtPoint(Point3d Value) : CameraSubject;
    public sealed record InBounds(BoundingBox Value) : CameraSubject;
    public sealed record InSphere(Sphere Value) : CameraSubject;
    public sealed record FromGeometry(GeometryBase Value) : CameraSubject;

    internal Fin<BoundingBox> BoundsOf(Op op) =>
        this switch {
            AtPoint source when source.Value.IsValid => Fin.Succ(value: new BoundingBox(min: source.Value, max: source.Value)),
            InBounds source when source.Value.IsValid => Fin.Succ(value: source.Value),
            InSphere source when source.Value.IsValid => Fin.Succ(value: source.Value.BoundingBox),
            FromGeometry source => source.Value.BoundsOf(op: op),
            _ => Fin.Fail<BoundingBox>(error: op.InvalidInput()),
        };

    internal Fin<CameraDepth> Depth(RhinoViewport viewport) =>
        Switch(
            viewport,
            atPoint: static (vp, source) => UI.RhinoUi.Protect(valid: () =>
                guard(source.Value.IsValid, DepthKey.InvalidInput()).ToFin().Bind(_ =>
                    vp.GetDepth(point: source.Value, distance: out double d)
                        ? Fin.Succ(value: new CameraDepth(Near: d, Far: d))
                        : Fin.Fail<CameraDepth>(error: DepthKey.InvalidResult()))),
            inBounds: static (vp, source) => UI.RhinoUi.Protect(valid: () =>
                guard(source.Value.IsValid, DepthKey.InvalidInput()).ToFin().Bind(_ =>
                    vp.GetDepth(bbox: source.Value, nearDistance: out double near, farDistance: out double far)
                        ? Fin.Succ(value: new CameraDepth(Near: near, Far: far))
                        : Fin.Fail<CameraDepth>(error: DepthKey.InvalidResult()))),
            inSphere: static (vp, source) => UI.RhinoUi.Protect(valid: () =>
                guard(source.Value.IsValid, DepthKey.InvalidInput()).ToFin().Bind(_ =>
                    vp.GetDepth(sphere: source.Value, nearDistance: out double near, farDistance: out double far)
                        ? Fin.Succ(value: new CameraDepth(Near: near, Far: far))
                        : Fin.Fail<CameraDepth>(error: DepthKey.InvalidResult()))),
            fromGeometry: static (vp, source) =>
                source.Value.BoundsOf(op: DepthKey).Bind(bounds => new InBounds(Value: bounds).Depth(viewport: vp)));

    internal Fin<bool> Visible(RhinoViewport viewport) =>
        Switch(
            viewport,
            atPoint: static (vp, src) => UI.RhinoUi.Protect(valid: () =>
                guard(src.Value.IsValid, VisibleKey.InvalidInput()).ToFin().Map(_ => vp.IsVisible(point: src.Value))),
            inBounds: static (vp, src) => UI.RhinoUi.Protect(valid: () =>
                guard(src.Value.IsValid, VisibleKey.InvalidInput()).ToFin().Map(_ => vp.IsVisible(bbox: src.Value))),
            inSphere: static (vp, src) => UI.RhinoUi.Protect(valid: () =>
                guard(src.Value.IsValid, VisibleKey.InvalidInput()).ToFin().Map(_ => vp.IsVisible(bbox: src.Value.BoundingBox))),
            fromGeometry: static (vp, src) =>
                src.Value.BoundsOf(op: VisibleKey).Bind(bounds =>
                    UI.RhinoUi.Protect(valid: () =>
                        guard(bounds.IsValid, VisibleKey.InvalidInput()).ToFin().Map(_ => vp.IsVisible(bbox: bounds)))));
}

[Union(SwitchMapStateParameterName = "context")]
public abstract partial record ViewportTarget {
    private ViewportTarget() { }
    public sealed record Current : ViewportTarget;
    public sealed record Id(Guid Value) : ViewportTarget;
    public sealed record View(RhinoView Value) : ViewportTarget;
    public sealed record Many(Seq<ViewportTarget> Targets) : ViewportTarget;

    internal Fin<Seq<CameraScope>> Resolve(RhinoDoc document, Op op) =>
        Switch(
            (Document: document, Op: op),
            current: static (ctx, _) =>
                from view in Optional(ctx.Document.Views.ActiveView).ToFin(Fail: ctx.Op.MissingContext())
                select Seq(CameraScope.Of(document: ctx.Document, view: view, viewport: view.ActiveViewport)),
            id: static (ctx, target) =>
                target.Value switch {
                    Guid id when id != Guid.Empty =>
                        Optional(ctx.Document.Views.Find(mainViewportId: id)).Case switch {
                            RhinoView view => Fin.Succ(value: Seq(CameraScope.Of(document: ctx.Document, view: view, viewport: view.MainViewport))),
                            _ => CameraScope.FindDetail(document: ctx.Document, id: id)
                                .Map(row => CameraScope.Of(document: ctx.Document, view: row.Page, viewport: row.Detail.Viewport))
                                .Map(scope => Seq(scope))
                                .ToFin(Fail: ctx.Op.MissingContext()),
                        },
                    _ => Fin.Fail<Seq<CameraScope>>(error: ctx.Op.InvalidInput()),
                },
            view: static (ctx, target) =>
                from view in Optional(target.Value).ToFin(Fail: ctx.Op.InvalidInput())
                from _ in guard(view.Document?.RuntimeSerialNumber == ctx.Document.RuntimeSerialNumber, ctx.Op.InvalidInput())
                select Seq(CameraScope.Of(document: ctx.Document, view: view, viewport: view.ActiveViewport)),
            many: static (ctx, target) => target.Targets.TraverseM(target => target.Resolve(document: ctx.Document, op: ctx.Op)).As()
                .Map(static scopes => scopes.Bind(static item => item)));
}

// Native GetFramePlaneCorners orders corners (0,1,2,3) = (BottomLeft, BottomRight, TopLeft, TopRight)
// and returns Point3d[0] when camera or frustum is invalid — surfaced here as Fin.Fail.
[Union]
public abstract partial record ProjectionSource {
    private ProjectionSource() { }

    public sealed record Live(RhinoViewport Viewport) : ProjectionSource;
    public sealed record Captured(ViewportInfo Projection) : ProjectionSource;

    internal Fin<T> With<T>(Func<ViewportInfo, Fin<T>> project, Op op) =>
        this switch {
            Live live => op.Catch(() => {
                using ViewportInfo p = new(rhinoViewport: live.Viewport);
                return project(arg: p);
            }),
            Captured cap => op.Catch(() => project(arg: cap.Projection)),
            _ => Fin.Fail<T>(error: op.InvalidInput()),
        };

    internal static Fin<(Point3d BottomLeft, Point3d BottomRight, Point3d TopLeft, Point3d TopRight)> FrustumRect(ProjectionSource source, double depth, Op op) =>
        source.With(
            project: p => p.GetFramePlaneCorners(depth: depth) is { Length: 4 } c
                ? Fin.Succ(value: (BottomLeft: c[0], BottomRight: c[1], TopLeft: c[2], TopRight: c[3]))
                : Fin.Fail<(Point3d, Point3d, Point3d, Point3d)>(error: op.InvalidResult()),
            op: op);

    internal static Fin<System.Drawing.PointF> ScreenPoint(ProjectionSource source, Point3d point, Op op) =>
        source.With(
            project: p => CameraScope.ProjectScreen(
                xform: p.GetXform(sourceSystem: CoordinateSystem.World, destinationSystem: CoordinateSystem.Screen),
                point: point,
                op: op),
            op: op);
}

// --- [MODELS] -----------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct CameraDepth(double Near, double Far);

[StructLayout(LayoutKind.Auto)]
public readonly record struct CameraDof(ViewInfoFocalBlurModes Mode, double Distance, double Aperture, double Jitter, uint SampleCount) {
    internal static CameraDof Read(ViewInfo view) =>
        new(Mode: view.FocalBlurMode, Distance: view.FocalBlurDistance, Aperture: view.FocalBlurAperture, Jitter: view.FocalBlurJitter, SampleCount: view.FocalBlurSampleCount);

    internal Unit Write(ViewInfo view) {
        view.FocalBlurMode = Mode;
        view.FocalBlurDistance = Distance;
        view.FocalBlurAperture = Aperture;
        view.FocalBlurJitter = Jitter;
        view.FocalBlurSampleCount = SampleCount;
        return unit;
    }
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct CameraFrame(Plane Frame, Point3d Target) {
    private static readonly Op OfKey = Op.Of(name: nameof(Of));
    private static readonly Op LookAtKey = Op.Of(name: nameof(LookAt));

    public static Fin<CameraFrame> Of(RhinoViewport viewport) =>
        Optional(viewport).ToFin(Fail: OfKey.InvalidInput())
            .Bind(valid => valid.GetCameraFrame(frame: out Plane plane) switch {
                true => Fin.Succ(value: new CameraFrame(Frame: plane, Target: valid.CameraTarget)),
                false => Fin.Fail<CameraFrame>(error: OfKey.InvalidResult()),
            });

    internal static Fin<CameraFrame> Of(ViewportInfo projection, Op op) =>
        from valid in Optional(projection).ToFin(Fail: op.InvalidInput())
        let direction = valid.CameraDirection
        let distance = valid.TargetDistance(useFrustumCenterFallback: true)
        from _valid in guard(
            valid.CameraLocation.IsValid && direction.IsValid && direction.Length > RhinoMath.ZeroTolerance
            && RhinoMath.IsValidDouble(x: distance) && distance > RhinoMath.ZeroTolerance,
            op.InvalidResult()).ToFin()
        from frame in LookAt(location: valid.CameraLocation, target: valid.CameraLocation + (direction * distance), up: Some(valid.CameraUp))
            .MapFail(_ => op.InvalidResult())
        select frame;

    public static Fin<CameraFrame> LookAt(Point3d location, Point3d target, Option<Vector3d> up = default) =>
        from direction in (location.IsValid, target.IsValid, target - location) switch {
            (true, true, Vector3d d) when d.Length > RhinoMath.ZeroTolerance => Fin.Succ(value: d),
            _ => Fin.Fail<Vector3d>(error: LookAtKey.InvalidInput()),
        }
        let resolved = up.Filter(static value => value.IsValid && value.Length > RhinoMath.ZeroTolerance)
                         .IfNone(() => ViewportInfo.CalculateCameraUpDirection(location: location, direction: direction, angle: 0.0))
        let plane = new Plane(origin: location, xDirection: Vector3d.CrossProduct(direction, resolved), yDirection: resolved)
        from _ in guard(plane.IsValid, LookAtKey.InvalidResult())
        select new CameraFrame(Frame: plane, Target: target);

    internal static Fin<CameraFrame> LookAt(Point3d location, Point3d target, Context context, Option<Vector3d> up, Op op) {
        Vector3d look = target - location;
        Option<Vector3d> xHint = up.Map(value => Vector3d.CrossProduct(a: look, b: value));
        return from _valid in guard(location.IsValid && target.IsValid && look.IsValid && look.Length > RhinoMath.ZeroTolerance, op.InvalidInput()).ToFin()
               from frame in VectorFrame.Of(origin: location, normal: -look, xHint: xHint, context: context, key: op)
               select new CameraFrame(Frame: frame.Value, Target: target);
    }

    public Point3d Location => Frame.Origin;
    public Vector3d Direction => -Frame.ZAxis;
    public Vector3d Up => Frame.YAxis;

    internal Fin<Unit> Apply(RhinoViewport viewport, Op op) {
        Plane frame = Frame;
        Point3d target = Target;
        return Optional(viewport).ToFin(Fail: op.InvalidInput()).Map(valid => {
            valid.SetCameraLocations(targetLocation: target, cameraLocation: frame.Origin);
            valid.SetCameraDirection(cameraDirection: -frame.ZAxis, updateTargetLocation: false);
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

    internal static Fin<CameraFrustum> Of(ViewportInfo projection, Op op) =>
        Optional(projection).ToFin(Fail: op.InvalidInput()).Bind(valid =>
            valid.GetFrustum(left: out double left, right: out double right, bottom: out double bottom, top: out double top, nearDistance: out double near, farDistance: out double far) switch {
                true => op.Catch(() => {
                    Point3d[] nearCorners = valid.GetFramePlaneCorners(depth: near);
                    Point3d[] farCorners = valid.GetFramePlaneCorners(depth: far);
                    BoundingBox bounds = nearCorners.Length == 4 && farCorners.Length == 4
                        ? new BoundingBox(points: nearCorners.Concat(farCorners))
                        : BoundingBox.Empty;
                    return bounds.IsValid
                        ? Fin.Succ(value: new CameraFrustum(Left: left, Right: right, Bottom: bottom, Top: top, Near: near, Far: far, Aspect: valid.FrustumAspect, Bounds: bounds))
                        : Fin.Fail<CameraFrustum>(error: op.InvalidResult());
                }),
                false => Fin.Fail<CameraFrustum>(error: op.InvalidResult()),
            });

    internal Fin<ViewportInfo> Apply(ViewportInfo projection, Op op) {
        CameraFrustum self = this;
        return from valid in Optional(projection).ToFin(Fail: op.InvalidInput())
               from _ in op.Confirm(success: valid.SetFrustum(left: self.Left, right: self.Right, bottom: self.Bottom, top: self.Top, nearDistance: self.Near, farDistance: self.Far))
               select valid;
    }
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct CameraViewState(
    CameraFrame Frame,
    CameraFrustum Frustum,
    CameraMode Mode,
    double LensLength,
    double CameraAngle,
    CameraDof Dof,
    DrawingRectangle ScreenPort,
    (double X, double Y, double Z) ViewScale,
    CameraLock Lock,
    Option<Plane> ConstructionPlane) {
    public double TargetDistance => Frame.Location.DistanceTo(other: Frame.Target);

    internal Fin<ViewportInfo> Projection(Op op) {
        CameraViewState self = this;
        return op.Catch(() => {
            ViewportInfo projection = new();
            bool handedOff = false;
            try {
                Fin<ViewportInfo> state =
                    from _screen in op.Confirm(success: projection.SetScreenPort(self.ScreenPort))
                    from _mode in self.Mode switch {
                        CameraMode.TwoPointPerspective => op.Confirm(success: projection.ChangeToTwoPointPerspectiveProjection(lensLength: self.LensLength, up: self.Frame.Up, targetDistance: self.TargetDistance)),
                        CameraMode.Perspective => op.Confirm(success: projection.ChangeToPerspectiveProjection(targetDistance: self.TargetDistance, symmetricFrustum: false, lensLength: self.LensLength)),
                        _ => op.Confirm(success: projection.ChangeToParallelProjection(symmetricFrustum: false)),
                    }
                    from _location in op.Confirm(success: projection.SetCameraLocation(self.Frame.Location))
                    from _direction in op.Confirm(success: projection.SetCameraDirection(self.Frame.Direction))
                    from _up in op.Confirm(success: projection.SetCameraUp(self.Frame.Up))
                    from _frustum in self.Frustum.Apply(projection: projection, op: op)
                    from _angle in Fin.Succ(value: Op.Side(() => projection.CameraAngle = self.CameraAngle))
                    from _scale in Fin.Succ(value: Op.Side(() => projection.SetViewScale(scaleX: self.ViewScale.X, scaleY: self.ViewScale.Y, scaleZ: self.ViewScale.Z)))
                    from _lock in self.Lock.Apply(projection: projection, op: op)
                    select projection;
                return state.BiBind(
                    Succ: value => {
                        handedOff = true;
                        return Fin.Succ(value: value);
                    },
                    Fail: error => Fin.Fail<ViewportInfo>(error: error));
            } finally {
                if (!handedOff) {
                    projection.Dispose();
                }
            }
        });
    }

    internal static Fin<CameraViewState> Of(RhinoViewport viewport, Op op) =>
        from active in Optional(viewport).ToFin(Fail: op.InvalidInput())
        from state in op.Catch(() => {
            using ViewportInfo projection = new(rhinoViewport: active);
            return Of(projection: projection, op: op);
        })
        from dof in op.Catch(() => {
            using ViewInfo info = new(active);
            return Fin.Succ(value: CameraDof.Read(view: info));
        })
        select state with { Dof = dof, ConstructionPlane = Some(value: active.ConstructionPlane()) };

    internal static Fin<CameraViewState> Of(ViewportInfo projection, Op op) =>
        Optional(projection).ToFin(Fail: op.InvalidInput()).Bind(active => op.Catch(() => {
            using ViewportInfo copy = new(active);
            return from frame in CameraFrame.Of(projection: copy, op: op)
                   from frustum in CameraFrustum.Of(projection: copy, op: op)
                   select new CameraViewState(
                       Frame: frame,
                       Frustum: frustum,
                       Mode: ModeFrom(isTwoPoint: copy.IsTwoPointPerspectiveProjection, isPerspective: copy.IsPerspectiveProjection),
                       LensLength: copy.Camera35mmLensLength,
                       CameraAngle: copy.CameraAngle,
                       Dof: default,
                       ScreenPort: copy.ScreenPort,
                       ViewScale: ViewScaleOf(projection: copy),
                       Lock: CameraLock.Of(projection: copy),
                       ConstructionPlane: Option<Plane>.None);
        }));

    internal static Fin<CameraViewState> Of(ViewInfo view, Op op) =>
        from active in Optional(view).ToFin(Fail: op.InvalidInput())
        from state in op.Catch(() => {
            using ViewportInfo projection = new(active.Viewport);
            return Of(projection: projection, op: op);
        })
        select state with { Dof = CameraDof.Read(view: active) };

    internal static CameraMode ModeFrom(bool isTwoPoint, bool isPerspective) =>
        (isTwoPoint, isPerspective) switch {
            (true, _) => CameraMode.TwoPointPerspective,
            (_, true) => CameraMode.Perspective,
            _ => CameraMode.Parallel,
        };

    private static (double X, double Y, double Z) ViewScaleOf(ViewportInfo projection) =>
        projection.GetViewScale() switch {
            [double x, double y, double z, ..] => (x, y, z),
            _ => (1.0, 1.0, 1.0),
        };
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct CameraScope(
    RhinoDoc Document,
    RhinoView View,
    RhinoViewport Viewport,
    Option<DetailViewObject> Detail = default) {
    private static readonly Op WorldToScreenScaleKey = Op.Of(name: nameof(WorldToScreenScale));
    private static readonly Op ScreenPointKey = Op.Of(name: nameof(ScreenPoint));

    internal static CameraScope Of(RhinoDoc document, RhinoView view, RhinoViewport viewport) =>
        new(
            Document: document,
            View: view,
            Viewport: viewport,
            Detail: view is RhinoPageView page
                ? FindDetail(page: page, id: viewport.Id).Map(row => row.Detail)
                : Option<DetailViewObject>.None);

    internal static Option<(RhinoPageView Page, DetailViewObject Detail)> FindDetail(RhinoDoc document, Guid id) =>
        DetailIndexCache.Find(document: document, id: id);

    internal static Option<(RhinoPageView Page, DetailViewObject Detail)> FindDetail(RhinoPageView page, Guid id) =>
        DetailIndexCache.Find(document: page.Document, id: id)
            .Filter(row => ReferenceEquals(objA: row.Page, objB: page));

    public Fin<CameraSnapshot> Snapshot() => CameraSnapshot.Of(scope: this);

    public Fin<Transform> CoordinateTransform(CoordinateSystem sourceSystem, CoordinateSystem destinationSystem) =>
        Probe(project: vp => {
            using ViewportInfo projection = new(rhinoViewport: vp);
            Transform transform = projection.GetXform(sourceSystem: sourceSystem, destinationSystem: destinationSystem);
            return Op.Of().AcceptValue(value: transform);
        });

    public Fin<Line> FrustumLine(double screenX, double screenY) =>
        Probe(project: vp => vp.GetFrustumLine(screenX: screenX, screenY: screenY, worldLine: out Line line) switch {
            true when line.IsValid && line != Line.Unset => Fin.Succ(value: line),
            _ => Fin.Fail<Line>(error: Op.Of().InvalidResult()),
        });

    public Fin<(Point3d BottomLeft, Point3d BottomRight, Point3d TopLeft, Point3d TopRight)> FrustumRect(double depth) =>
        Probe(project: vp => ProjectionSource.FrustumRect(source: new ProjectionSource.Live(Viewport: vp), depth: depth, op: Op.Of()));

    public Fin<CameraDepth> Depth(CameraSubject source) {
        RhinoViewport viewport = Viewport;
        return Optional(source).ToFin(Fail: Op.Of().InvalidInput()).Bind(valid => valid.Depth(viewport: viewport));
    }

    public Fin<bool> Visible(CameraSubject source) {
        RhinoViewport viewport = Viewport;
        return Optional(source).ToFin(Fail: Op.Of().InvalidInput()).Bind(valid => valid.Visible(viewport: viewport));
    }

    public Fin<double> TargetDistance(bool useFrustumCenterFallback = true) =>
        Probe(project: vp => {
            using ViewportInfo projection = new(rhinoViewport: vp);
            double distance = projection.TargetDistance(useFrustumCenterFallback: useFrustumCenterFallback);
            return Op.Of().AcceptValue(value: distance);
        });

    public Fin<double> WorldToScreenScale(Point3d point) {
        CameraScope self = this;
        return from _ in guard(point.IsValid, WorldToScreenScaleKey.InvalidInput())
               from scale in self.Probe(project: vp => WorldToScreenScaleKey.Catch(() =>
                   vp.GetWorldToScreenScale(pointInFrustum: point, pixelsPerUnit: out double pixels) switch {
                       true when RhinoMath.IsValidDouble(x: pixels) && pixels > 0.0 => Fin.Succ(value: pixels),
                       _ => Fin.Fail<double>(error: WorldToScreenScaleKey.InvalidResult()),
                   }))
               select scale;
    }
    public Fin<System.Drawing.PointF> ScreenPoint(Point3d point) {
        CameraScope self = this;
        return from _ in guard(point.IsValid, ScreenPointKey.InvalidInput())
               from screen in self.Probe(project: vp => ProjectionSource.ScreenPoint(source: new ProjectionSource.Live(Viewport: vp), point: point, op: ScreenPointKey))
               select screen;
    }

    public Fin<Unit> ApplyRedraw(RedrawRequest request) {
        CameraScope self = this;
        return Optional(request).ToFin(Fail: Op.Of().InvalidInput())
            .Bind(valid => valid.ApplyTo(scope: self));
    }

    public Fin<Unit> Redraw() => ApplyRedraw(request: RedrawFor(scope: this));

    internal static RedrawRequest RedrawFor(CameraScope scope) =>
        scope.Detail.IsSome ? new RedrawRequest.DetailCommit() : new RedrawRequest.View();

    private Fin<T> Probe<T>(Func<RhinoViewport, Fin<T>> project) {
        RhinoViewport viewport = Viewport;
        return UI.RhinoUi.Protect(valid: () => project(arg: viewport));
    }

    internal static Fin<System.Drawing.PointF> ProjectScreen(Transform xform, Point3d point, Op op) {
        Point3d projected = xform * point;
        return xform.IsValid && projected.IsValid
            ? Fin.Succ(value: new System.Drawing.PointF((float)projected.X, (float)projected.Y))
            : Fin.Fail<System.Drawing.PointF>(error: op.InvalidResult());
    }

    private static class DetailIndexCache {
        private readonly record struct Row(RhinoPageView Page, DetailViewObject Detail);

        private readonly record struct Entry(int PageCount, int DetailCount, uint UndoSerial, HashMap<Guid, Row> ById);

        private static readonly Atom<(Seq<uint> Order, HashMap<uint, Entry> Entries)> Cell =
            Atom(value: (Order: Seq<uint>(), Entries: HashMap<uint, Entry>()));

        internal static Option<(RhinoPageView Page, DetailViewObject Detail)> Find(RhinoDoc document, Guid id) {
            uint serial = document.RuntimeSerialNumber;
            (int pageCount, int detailCount, uint undoSerial) = Stamp(document: document);
            (_, HashMap<uint, Entry> entries) = Cell.Value;
            return entries.Find(key: serial) is { IsSome: true, Case: Entry hit }
                && hit.PageCount == pageCount
                && hit.DetailCount == detailCount
                && hit.UndoSerial == undoSerial
                ? hit.ById.Find(key: id).Map(row => (row.Page, row.Detail))
                : InsertAndFind(document: document, serial: serial, pageCount: pageCount, detailCount: detailCount, undoSerial: undoSerial, id: id);
        }

        private static (int PageCount, int DetailCount, uint UndoSerial) Stamp(RhinoDoc document) =>
            (
                PageCount: document.Views.PageViewCount,
                DetailCount: toSeq(document.Views.GetPageViews() ?? [])
                    .Fold(0, static (count, page) => count + (page.GetDetailViews()?.Length ?? 0)),
                UndoSerial: document.NextUndoRecordSerialNumber);

        private static HashMap<Guid, Row> Build(RhinoDoc document) =>
            toSeq(document.Views.GetPageViews() ?? [])
                .Bind(static page => toSeq(page.GetDetailViews()).Map(detail => (Page: page, Detail: detail)))
                .Fold(
                    initialState: HashMap<Guid, Row>(),
                    f: static (map, row) => {
                        Row value = new(Page: row.Page, Detail: row.Detail);
                        return map
                            .AddOrUpdate(key: row.Detail.Id, value: value)
                            .AddOrUpdate(key: row.Detail.Viewport.Id, value: value);
                    });

        private static (Seq<uint> Order, HashMap<uint, Entry> Entries) Touch(
            (Seq<uint> Order, HashMap<uint, Entry> Entries) state,
            uint serial,
            int pageCount,
            int detailCount,
            uint undoSerial,
            HashMap<Guid, Row> index) {
            Seq<uint> merged = state.Order.Filter(h => h != serial) + Seq(serial);
            Seq<uint> promoted = toSeq(merged.Skip(count: Math.Max(merged.Count - CameraDefaults.DetailCacheDocuments, 0)));
            LanguageExt.HashSet<uint> keep = toHashSet(promoted);
            HashMap<uint, Entry> entries = state.Entries
                .AddOrUpdate(key: serial, value: new Entry(PageCount: pageCount, DetailCount: detailCount, UndoSerial: undoSerial, ById: index))
                .Filter((key, _) => keep.Find(key: key).IsSome);
            return (Order: promoted, Entries: entries);
        }

        private static Option<(RhinoPageView Page, DetailViewObject Detail)> InsertAndFind(
            RhinoDoc document,
            uint serial,
            int pageCount,
            int detailCount,
            uint undoSerial,
            Guid id) {
            HashMap<Guid, Row> index = Build(document: document);
            _ = Cell.Swap(f: state => Touch(state: state, serial: serial, pageCount: pageCount, detailCount: detailCount, undoSerial: undoSerial, index: index));
            return index.Find(key: id).Map(row => (row.Page, row.Detail));
        }
    }
}

public sealed record CameraSnapshot : IDisposable {
    private static readonly Op OfKey = Op.Of(name: nameof(CameraSnapshot));

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
        CameraDof dof,
        uint documentSerial,
        uint changeSerial) {
        Scope = scope;
        Frame = frame;
        Frustum = frustum;
        Projection = projection;
        ConstructionPlane = constructionPlane;
        DisplayMode = displayMode;
        ScreenPort = screenPort;
        Size = size;
        LockedProjection = lockedProjection;
        Mode = mode;
        LensLength = lensLength;
        CameraAngle = cameraAngle;
        Dof = dof;
        DocumentSerial = documentSerial;
        ChangeSerial = changeSerial;
    }

    internal static Fin<CameraSnapshot> Of(CameraScope scope) {
        RhinoViewport viewport = scope.Viewport;
        return OfKey.Catch(() => {
            ViewportInfo captured = new(rhinoViewport: viewport);
            using ViewInfo info = new(viewport);
            CameraDof dof = CameraDof.Read(view: info);
            return (from frustum in CameraFrustum.Of(viewport: viewport, op: OfKey)
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
                        mode: CameraViewState.ModeFrom(isTwoPoint: viewport.IsTwoPointPerspectiveProjection, isPerspective: viewport.IsPerspectiveProjection),
                        lensLength: viewport.Camera35mmLensLength,
                        cameraAngle: viewport.CameraAngle,
                        dof: dof,
                        documentSerial: scope.Document.RuntimeSerialNumber,
                        changeSerial: viewport.ChangeCounter))
                .BindFail(error => {
                    captured.Dispose();
                    return Fin.Fail<CameraSnapshot>(error: error);
                });
        });
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
    public CameraDof Dof { get; }
    public uint DocumentSerial { get; }
    public uint ChangeSerial { get; }
    public bool IsStale =>
        Scope.Document.RuntimeSerialNumber != DocumentSerial || Scope.Viewport.ChangeCounter != ChangeSerial;

    public Fin<CameraDepth> Depth(CameraSubject source) {
        CameraFrame frame = Frame;
        Op op = Op.Of();
        return Optional(source).ToFin(Fail: op.InvalidInput())
            .Bind(valid => valid.BoundsOf(op: op))
            .Bind(bounds => guard(bounds.IsValid, op.InvalidResult()).ToFin().Map(_ => toSeq(bounds.GetCorners())
                .Map(corner => (corner - frame.Location) * frame.Direction)
                .Fold((Near: double.MaxValue, Far: double.MinValue), static (acc, depth) => (Near: Math.Min(val1: acc.Near, val2: depth), Far: Math.Max(val1: acc.Far, val2: depth)))))
            // Far <= 0 means the whole box sits behind the camera plane — no valid clipping range,
            // matching the live CameraSubject.Depth failure semantics for the behind-camera case.
            .Bind(depth => guard(depth.Far > 0.0, op.InvalidResult()).ToFin().Map(_ => new CameraDepth(Near: depth.Near, Far: depth.Far)));
    }

    // Reuses the owned Projection ViewportInfo — zero allocation. Corner order matches CameraScope.FrustumRect.
    public Fin<(Point3d BottomLeft, Point3d BottomRight, Point3d TopLeft, Point3d TopRight)> FrustumRect(double depth) =>
        ProjectionSource.FrustumRect(source: new ProjectionSource.Captured(Projection: Projection), depth: depth, op: Op.Of());

    public Fin<bool> IsVisible(BoundingBox box) {
        ViewportInfo captured = Projection;
        Op op = Op.Of();
        return from _ in guard(box.IsValid, op.InvalidInput())
               from result in op.Catch(() => {
                   // FrustumNearPlane's normal points OUT of the frustum (toward the camera) while Far and the
                   // four sides point INTO it; a corner inside the truncated pyramid is therefore on near's
                   // negative side and the other five planes' positive side. The strict 6-plane test avoids the
                   // perspective false positives the AABB-of-frustum produced in the pyramid corners.
                   Plane near = captured.FrustumNearPlane;
                   Plane[] inward = [
                       captured.FrustumFarPlane,
                       captured.FrustumLeftPlane,
                       captured.FrustumRightPlane,
                       captured.FrustumBottomPlane,
                       captured.FrustumTopPlane,
                   ];
                   return Fin.Succ(value:
                       near.IsValid && inward.All(static plane => plane.IsValid) &&
                       box.GetCorners().Any(corner =>
                           near.DistanceTo(testPoint: corner) <= 0.0
                           && inward.All(plane => plane.DistanceTo(testPoint: corner) >= 0.0)));
               })
               select result;
    }

    public Fin<System.Drawing.PointF> ScreenPoint(Point3d point) {
        ViewportInfo captured = Projection;
        Op op = Op.Of(name: nameof(ScreenPoint));
        return from _ in guard(point.IsValid, op.InvalidInput())
               from screen in ProjectionSource.ScreenPoint(source: new ProjectionSource.Captured(Projection: captured), point: point, op: op)
               select screen;
    }

    public void Dispose() {
        Projection.Dispose();
        GC.SuppressFinalize(obj: this);
    }

    internal Fin<RedrawRequest> Restore(bool updateTarget = true) {
        CameraSnapshot self = this;
        Op op = Op.Of(name: nameof(Restore));
        return UI.RhinoUi.Protect(valid: () =>
            from _ in guard(self.Scope.Document.RuntimeSerialNumber == self.DocumentSerial, op.InvalidInput())
                // A live scale-locked detail viewport silently no-ops CRhinoViewport_SetVP; clear the lock
                // first, then the final Op.Side restores the snapshotted lock state exactly.
            from _unlock in Fin.Succ(value: Op.Side(() => self.Scope.Viewport.LockedProjection = false))
            from restored in op.Confirm(success: self.Scope.Viewport.SetViewProjection(projection: self.Projection, updateTargetLocation: updateTarget))
            from applied in Fin.Succ(value: Op.Side(() => {
                self.Scope.Viewport.SetConstructionPlane(plane: self.ConstructionPlane);
                self.Scope.Viewport.DisplayMode = self.DisplayMode;
                self.Scope.Viewport.LockedProjection = self.LockedProjection;
            }))
            select CameraScope.RedrawFor(scope: self.Scope));
    }
}
