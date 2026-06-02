using System.Runtime.InteropServices;
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

    public Point3d Location => Frame.Origin;
    public Vector3d Direction => -Frame.ZAxis;
    public Vector3d Up => Frame.YAxis;

    public static Fin<CameraFrame> Of(RhinoViewport viewport) =>
        Optional(viewport).ToFin(Fail: OfKey.InvalidInput())
            .Bind(valid => valid.GetCameraFrame(frame: out Plane plane) switch {
                true => Fin.Succ(value: new CameraFrame(Frame: plane, Target: valid.CameraTarget)),
                false => Fin.Fail<CameraFrame>(error: OfKey.InvalidResult()),
            });

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

    internal Fin<ViewportInfo> Apply(ViewportInfo projection, Op op) {
        CameraFrustum self = this;
        return from valid in Optional(projection).ToFin(Fail: op.InvalidInput())
               from _ in op.Confirm(success: valid.SetFrustum(left: self.Left, right: self.Right, bottom: self.Bottom, top: self.Top, nearDistance: self.Near, farDistance: self.Far))
               select valid;
    }
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

    private Fin<T> Probe<T>(Func<RhinoViewport, Fin<T>> project) {
        RhinoViewport viewport = Viewport;
        return UI.RhinoUi.Protect(valid: () => project(arg: viewport));
    }

    public Fin<Transform> CoordinateTransform(CoordinateSystem sourceSystem, CoordinateSystem destinationSystem) =>
        Probe(project: vp => {
            using ViewportInfo projection = new(rhinoViewport: vp);
            Transform transform = projection.GetXform(sourceSystem: sourceSystem, destinationSystem: destinationSystem);
            return Op.Of(name: nameof(CoordinateTransform)).AcceptValue(value: transform);
        });

    public Fin<Line> FrustumLine(double screenX, double screenY) =>
        Probe(project: vp => vp.GetFrustumLine(screenX: screenX, screenY: screenY, worldLine: out Line line) switch {
            true when line.IsValid && line != Line.Unset => Fin.Succ(value: line),
            _ => Fin.Fail<Line>(error: Op.Of(name: nameof(FrustumLine)).InvalidResult()),
        });

    // Native GetFramePlaneCorners orders corners (0,1,2,3) = (BottomLeft, BottomRight, TopLeft, TopRight)
    // and returns Point3d[0] when camera or frustum is invalid — surfaced here as Fin.Fail.
    public Fin<(Point3d BottomLeft, Point3d BottomRight, Point3d TopLeft, Point3d TopRight)> FrustumRect(double depth) =>
        Probe(project: vp => Op.Of(name: nameof(FrustumRect)).Catch(() => {
            using ViewportInfo projection = new(rhinoViewport: vp);
            Point3d[] corners = projection.GetFramePlaneCorners(depth: depth);
            return corners.Length == 4
                ? Fin.Succ(value: (BottomLeft: corners[0], BottomRight: corners[1], TopLeft: corners[2], TopRight: corners[3]))
                : Fin.Fail<(Point3d, Point3d, Point3d, Point3d)>(error: Op.Of(name: nameof(FrustumRect)).InvalidResult());
        }));

    public Fin<CameraDepth> Depth(CameraSubject source) {
        RhinoViewport viewport = Viewport;
        return Optional(source).ToFin(Fail: Op.Of(name: nameof(Depth)).InvalidInput()).Bind(valid => valid.Depth(viewport: viewport));
    }

    public Fin<bool> Visible(CameraSubject source) {
        RhinoViewport viewport = Viewport;
        return Optional(source).ToFin(Fail: Op.Of(name: nameof(Visible)).InvalidInput()).Bind(valid => valid.Visible(viewport: viewport));
    }

    public Fin<double> TargetDistance(bool useFrustumCenterFallback = true) =>
        Probe(project: vp => {
            using ViewportInfo projection = new(rhinoViewport: vp);
            double distance = projection.TargetDistance(useFrustumCenterFallback: useFrustumCenterFallback);
            return Op.Of(name: nameof(TargetDistance)).AcceptValue(value: distance);
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
               from screen in self.Probe(project: vp => ScreenPointKey.Catch(() => {
                   using ViewportInfo projection = new(rhinoViewport: vp);
                   Transform xform = projection.GetXform(sourceSystem: CoordinateSystem.World, destinationSystem: CoordinateSystem.Screen);
                   return ProjectScreen(xform: xform, point: point, op: ScreenPointKey);
               }))
               select screen;
    }

    public Fin<Unit> ApplyRedraw(RedrawRequest request) {
        CameraScope self = this;
        return Optional(request).ToFin(Fail: Op.Of(name: nameof(ApplyRedraw)).InvalidInput())
            .Bind(valid => valid.ApplyTo(scope: self));
    }

    internal static RedrawRequest RedrawFor(CameraScope scope) =>
        scope.Detail.IsSome ? new RedrawRequest.DetailCommit() : new RedrawRequest.View();

    public Fin<Unit> Redraw() => ApplyRedraw(request: RedrawFor(scope: this));

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
    public Fin<System.Drawing.PointF> ScreenPoint(Point3d point) {
        ViewportInfo captured = Projection;
        Op op = Op.Of(name: nameof(ScreenPoint));
        return from _ in guard(point.IsValid, op.InvalidInput())
               from screen in op.Catch(() => {
                   Transform xform = captured.GetXform(sourceSystem: CoordinateSystem.World, destinationSystem: CoordinateSystem.Screen);
                   return CameraScope.ProjectScreen(xform: xform, point: point, op: op);
               })
               select screen;
    }

    public Fin<CameraDepth> Depth(CameraSubject source) {
        CameraFrame frame = Frame;
        Op op = Op.Of(name: nameof(Depth));
        return Optional(source).ToFin(Fail: op.InvalidInput())
            .Bind(valid => valid.BoundsOf(op: op))
            .Bind(bounds => guard(bounds.IsValid, op.InvalidResult()).ToFin().Map(_ => DepthOf(frame: frame, bounds: bounds)))
            // Far <= 0 means the whole box sits behind the camera plane — no valid clipping range,
            // matching the live CameraSubject.Depth failure semantics for the behind-camera case.
            .Bind(depth => guard(depth.Far > 0.0, op.InvalidResult()).ToFin().Map(_ => depth));
    }
    public Fin<bool> IsVisible(BoundingBox box) {
        ViewportInfo captured = Projection;
        Op op = Op.Of(name: nameof(IsVisible));
        return from _ in guard(box.IsValid, op.InvalidInput())
               from result in op.Catch(() => {
                   // Inward-pointing frustum planes (GetPlane 0-5): a corner inside the truncated
                   // pyramid satisfies DistanceTo >= 0 against all six. Strict 6-plane test avoids the
                   // perspective false positives the AABB-of-frustum produced in the pyramid corners.
                   Plane[] planes = [
                       captured.FrustumNearPlane,
                       captured.FrustumFarPlane,
                       captured.FrustumLeftPlane,
                       captured.FrustumRightPlane,
                       captured.FrustumBottomPlane,
                       captured.FrustumTopPlane,
                   ];
                   return Fin.Succ(value:
                       planes.All(static plane => plane.IsValid) &&
                       box.GetCorners().Any(corner => planes.All(plane => plane.DistanceTo(testPoint: corner) >= 0.0)));
               })
               select result;
    }

    // Reuses the owned Projection ViewportInfo — zero allocation. Corner order matches CameraScope.FrustumRect.
    public Fin<(Point3d BottomLeft, Point3d BottomRight, Point3d TopLeft, Point3d TopRight)> FrustumRect(double depth) {
        ViewportInfo captured = Projection;
        Op op = Op.Of(name: nameof(FrustumRect));
        return op.Catch(() => {
            Point3d[] corners = captured.GetFramePlaneCorners(depth: depth);
            return corners.Length == 4
                ? Fin.Succ(value: (BottomLeft: corners[0], BottomRight: corners[1], TopLeft: corners[2], TopRight: corners[3]))
                : Fin.Fail<(Point3d, Point3d, Point3d, Point3d)>(error: op.InvalidResult());
        });
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

    private static CameraDepth DepthOf(CameraFrame frame, BoundingBox bounds) {
        (double near, double far) = toSeq(bounds.GetCorners())
            .Map(corner => (corner - frame.Location) * frame.Direction)
            .Fold((Near: double.MaxValue, Far: double.MinValue), static (acc, depth) => (Near: Math.Min(val1: acc.Near, val2: depth), Far: Math.Max(val1: acc.Far, val2: depth)));
        return new CameraDepth(Near: near, Far: far);
    }

    public void Dispose() {
        Projection.Dispose();
        GC.SuppressFinalize(obj: this);
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
                        mode: viewport switch {
                            { IsTwoPointPerspectiveProjection: true } => CameraMode.TwoPointPerspective,
                            { IsPerspectiveProjection: true } => CameraMode.Perspective,
                            _ => CameraMode.Parallel,
                        },
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
}
