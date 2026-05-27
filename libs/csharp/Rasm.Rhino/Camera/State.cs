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
    public sealed record Many(Seq<ViewportTarget> Targets) : ViewportTarget;

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
                            _ => CameraScope.FindDetail(document: ctx.Document, id: id)
                                .Map(row => CameraScope.Of(document: ctx.Document, view: row.Page, viewport: row.Detail.Viewport))
                                .ToFin(Fail: ctx.Op.MissingContext()),
                        },
                    _ => Fin.Fail<CameraScope>(error: ctx.Op.InvalidInput()),
                },
            view: static (ctx, target) =>
                from view in Optional(target.Value).ToFin(Fail: ctx.Op.InvalidInput())
                from _ in guard(view.Document?.RuntimeSerialNumber == ctx.Document.RuntimeSerialNumber, ctx.Op.InvalidInput())
                select CameraScope.Of(document: ctx.Document, view: view, viewport: view.ActiveViewport),
            many: static (ctx, target) => Fin.Fail<CameraScope>(error: ctx.Op.InvalidInput()));

    internal Fin<Seq<CameraScope>> ResolveMany(RhinoDoc document, Op op) =>
        this switch {
            Many many => many.Targets.TraverseM(target => target.Resolve(document: document, op: op)).As(),
            _ => Resolve(document: document, op: op).Map(scope => toSeq([scope])),
        };
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct CameraDepth(double Near, double Far);

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
                source.Value.IsValid switch {
                    false => Fin.Fail<CameraDepth>(error: DepthKey.InvalidInput()),
                    true => vp.GetDepth(point: source.Value, distance: out double distance)
                        ? Fin.Succ(value: new CameraDepth(Near: distance, Far: distance))
                        : Fin.Fail<CameraDepth>(error: DepthKey.InvalidResult()),
                }),
            inBounds: static (vp, source) => UI.RhinoUi.Protect(valid: () =>
                source.Value.IsValid switch {
                    false => Fin.Fail<CameraDepth>(error: DepthKey.InvalidInput()),
                    true => vp.GetDepth(bbox: source.Value, nearDistance: out double near, farDistance: out double far)
                        ? Fin.Succ(value: new CameraDepth(Near: near, Far: far))
                        : Fin.Fail<CameraDepth>(error: DepthKey.InvalidResult()),
                }),
            inSphere: static (vp, source) => UI.RhinoUi.Protect(valid: () =>
                source.Value.IsValid switch {
                    false => Fin.Fail<CameraDepth>(error: DepthKey.InvalidInput()),
                    true => vp.GetDepth(sphere: source.Value, nearDistance: out double near, farDistance: out double far)
                        ? Fin.Succ(value: new CameraDepth(Near: near, Far: far))
                        : Fin.Fail<CameraDepth>(error: DepthKey.InvalidResult()),
                }),
            fromGeometry: static (vp, source) =>
                source.Value.BoundsOf(op: DepthKey).Bind(bounds => new InBounds(Value: bounds).Depth(viewport: vp)));

    internal Fin<bool> Visible(RhinoViewport viewport) =>
        Switch(
            viewport,
            atPoint: static (vp, source) => UI.RhinoUi.Protect(valid: () =>
                source.Value.IsValid
                    ? Fin.Succ(value: vp.IsVisible(point: source.Value))
                    : Fin.Fail<bool>(error: VisibleKey.InvalidInput())),
            inBounds: static (vp, source) => UI.RhinoUi.Protect(valid: () =>
                source.Value.IsValid
                    ? Fin.Succ(value: vp.IsVisible(bbox: source.Value))
                    : Fin.Fail<bool>(error: VisibleKey.InvalidInput())),
            inSphere: static (vp, source) => UI.RhinoUi.Protect(valid: () =>
                source.Value.IsValid
                    ? Fin.Succ(value: vp.IsVisible(bbox: source.Value.BoundingBox))
                    : Fin.Fail<bool>(error: VisibleKey.InvalidInput())),
            fromGeometry: static (vp, source) =>
                source.Value.BoundsOf(op: VisibleKey).Bind(bounds => new InBounds(Value: bounds).Visible(viewport: vp)));
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct CameraScope(
    RhinoDoc Document,
    RhinoView View,
    RhinoViewport Viewport,
    Option<DetailViewObject> Detail = default) {
    private static readonly Op WorldToScreenScaleKey = Op.Of(name: nameof(WorldToScreenScale));

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
        return from _ in guard(point.IsValid, WorldToScreenScaleKey.InvalidInput())
               from scale in WorldToScreenScaleKey.Catch(() => UI.RhinoUi.Protect(valid: () =>
                   viewport.GetWorldToScreenScale(pointInFrustum: point, pixelsPerUnit: out double pixels) switch {
                       true when RhinoMath.IsValidDouble(x: pixels) && pixels > 0.0 => Fin.Succ(value: pixels),
                       _ => Fin.Fail<double>(error: WorldToScreenScaleKey.InvalidResult()),
                   }))
               select scale;
    }

    public Fin<Unit> ApplyRedraw(RedrawRequest request) {
        CameraScope self = this;
        return Optional(request).ToFin(Fail: Op.Of(name: nameof(ApplyRedraw)).InvalidInput())
            .Bind(valid => valid.ApplyTo(scope: self));
    }

    internal static RedrawRequest RedrawFor(CameraScope scope) =>
        scope.Detail.IsSome ? new RedrawRequest.DetailCommit() : new RedrawRequest.View();

    public Fin<Unit> Redraw() => ApplyRedraw(request: RedrawFor(scope: this));
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
    private static readonly Op OfKey = Op.Of(name: nameof(CameraSnapshot));
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

    // `ChangeCounter` (uint) advances per viewport mutation; doc serial guards reopen + uint wrap.
    public bool IsStale =>
        Scope.Document.RuntimeSerialNumber != DocumentSerial || Scope.Viewport.ChangeCounter != ChangeSerial;

    public void Dispose() {
        projection.Dispose();
        GC.SuppressFinalize(obj: this);
    }

    internal static Fin<CameraSnapshot> Of(CameraScope scope) {
        RhinoViewport viewport = scope.Viewport;
        return OfKey.Catch(() => {
            // BOUNDARY ADAPTER — ViewportInfo ctor may throw; disposal on exception prevents native leak.
            ViewportInfo? snapshotProjection = null;
            try {
                snapshotProjection = new ViewportInfo(rhinoViewport: viewport);
                ViewportInfo captured = snapshotProjection;
                return from frustum in CameraFrustum.Of(viewport: viewport, op: OfKey)
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
                snapshotProjection?.Dispose();
                throw;
            }
        });
    }
}

file static class DetailIndexCache {
    private const int MaxDocuments = 8;

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
        int skip = merged.Count > MaxDocuments ? merged.Count - MaxDocuments : 0;
        Seq<uint> promoted = toSeq(merged.Skip(count: skip));
        LanguageExt.HashSet<uint> keep = toHashSet(promoted);
        HashMap<uint, Entry> entries = state.Entries
            .AddOrUpdate(key: serial, value: new Entry(PageCount: pageCount, DetailCount: detailCount, UndoSerial: undoSerial, ById: index))
            .Filter((key, _) => keep.Find(key: key).IsSome);
        return (Order: promoted, Entries: entries);
    }
}
