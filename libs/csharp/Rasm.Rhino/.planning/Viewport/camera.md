# [RASM_RHINO_CAMERA]

The camera model owner (`Rasm.Rhino.Viewport`). One vocabulary separates the three altitudes the census-era model fused: kernel pose and intent (`CameraPose` composing the `Rasm.Numerics` `VectorFrame`, projected from `VectorIntent` view rows — never a second frame algebra), a typed host scope lease (`ViewportTarget` resolved through the `DocumentSession` capability rail into a `ViewportLease` whose live `RhinoView`/`RhinoViewport`/`DetailViewObject` handles never escape a borrow), and the native snapshot adapter (`CameraSnapshot` projecting a disposable `ViewportInfo` to values inside one owned lease). Frustum, depth, depth-of-field, construction plane, visibility, and screen transforms are typed host rows read through the lease; main views, page views, and page details are cases of one target union, so a consumer addresses any viewport shape with one request and receives values, evidence records, and `Fin` rails — a live host handle, a retained `ViewportInfo`, or a `System.Drawing` screen carrier past this boundary is the deleted form.

## [01]-[INDEX]

- [02]-[SCOPE_LEASE]: `ViewportTarget` the polymorphic viewport address, `ViewportRef` the resolved row, `ViewportLease` the session-gated borrow surface, and stale-identity detection over `ChangeCounter`.
- [03]-[POSE_MODEL]: `CameraPose` over the kernel `VectorFrame`, `LensState`, `ProjectionKind` classification, and the pose read/write pair on one owner.
- [04]-[HOST_ROWS]: `CameraFrustum`, `DepthProbe`/`VisibilityProbe` polymorphic depth and visibility, `CameraDof` focal-blur row, `CPlaneGrid`, and the `ViewMapping` coordinate-transform rows.
- [05]-[SNAPSHOT]: `CameraSnapshot` — the `ViewportInfo` value adapter with staleness evidence and the restore seam.

## [02]-[SCOPE_LEASE]

- Owner: `ViewportTarget` `[Union]` — the one viewport address: `ActiveCase`, `NamedCase(string)`, `IdCase(Guid)`, `PageCase(Guid)`, `DetailCase(Guid, Guid)`, `EveryCase(bool)`. `ViewportRef` is the resolved row — view, viewport, and the detail object when the address lands inside a layout — `internal` so host handles stay inside the package, and its `Info` member is the ONE disposable-`ViewportInfo` projection: minted, projected, and released inside the borrow, never retained. `ViewportLease` is the borrow surface: resolution runs once per lease under `SessionNeed.Redraw`, the row set is immutable, and every read or edit crosses through `Use`/`UseAll`, which re-checks the session snapshot before touching a handle.
- Entry: `ViewportTarget.Active()` / `Named(name, Op?)` / `Id(id, Op?)` / `Page(pageId, Op?)` / `Detail(pageId, detailId, Op?)` / `Every(includePages)` construct; `ViewportLease.Of(DocumentSession, ViewportTarget, Op?)` resolves; `lease.Use<T>(Func<ViewportRef, Fin<T>>)` borrows the single row — a broadcast lease under `Use` is a typed refusal, never a silent head-pick — and `UseAll<T>` folds every row, so single and broadcast execution are one surface discriminated by the target's arity, never sibling entrypoints.
- Law: resolution names the host lookup members exactly once — `RhinoDoc.Views.ActiveView`, `RhinoDoc.Views.Find`, `RhinoDoc.Views.GetViewList(ViewTypeFilter)`, `RhinoDoc.Views.GetPageViews`, `RhinoPageView.GetDetailViews`, `DetailViewObject.Viewport` — and an address that resolves to nothing is a typed refusal at `Of`, never a null row a consumer branches on; a detail address matches either detail identity (`DetailViewObject.Id` or `DetailViewObject.Viewport.Id`), because commands hand out the object id and viewport APIs the viewport id.
- Law: a detail row carries its `DetailViewObject` beside the viewport because a detail edit is committed through `CommitViewportChanges`, not observed — the commit seam lives on the operations rail, and the lease only proves which rows are details.
- Law: lease identity is generation-stamped — each row captures `RhinoViewport.ChangeCounter` and the session `DocKey` at resolution, and `Stale` reads both against the live handle so a consumer holding a lease across host mutations detects drift as evidence instead of acting on a moved camera.
- Law: every borrow crosses the session capability rail through `HostThread.OnSession` — the HostUi seam whose closure capture carries the typed value while the demand's own result stays the session `DocKey` — so the session's result constraint holds without constraining consumer value types, and no parallel detachment envelope exists beside that seam.
- Law: an addressed row binds `RhinoView.MainViewport` — the viewport the address names — because `RhinoPageView.ActiveViewport` silently returns an active detail; only `ActiveCase` binds `ActiveViewport`, adopting the host's own active semantics, and a detail row binds `DetailViewObject.Viewport`.
- Boundary: the lease holds borrowed host handles under the session's own `Lease<RhinoDoc>` lifetime; it is not `IDisposable` because it owns nothing — disposal pressure on a viewport handle is the host's, and a consumer needing durability re-resolves.

```csharp
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using Rasm.Domain;
using Rasm.Numerics;
using Rasm.Rhino.Document;
using Rasm.Rhino.HostUi;

namespace Rasm.Rhino.Viewport;

// --- [TYPES] --------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ViewportTarget {
    private ViewportTarget() { }
    public sealed record ActiveCase : ViewportTarget;
    public sealed record NamedCase(string Name) : ViewportTarget;
    public sealed record IdCase(Guid ViewportId) : ViewportTarget;
    public sealed record PageCase(Guid PageViewId) : ViewportTarget;
    public sealed record DetailCase(Guid PageViewId, Guid DetailId) : ViewportTarget;
    public sealed record EveryCase(bool IncludePages) : ViewportTarget;

    public static ViewportTarget Active() => new ActiveCase();
    public static ViewportTarget Every(bool includePages = false) => new EveryCase(IncludePages: includePages);
    public static Fin<ViewportTarget> Named(string name, Op? key = null) =>
        key.OrDefault().AcceptText(value: name).Map(static valid => (ViewportTarget)new NamedCase(Name: valid));
    public static Fin<ViewportTarget> Id(Guid viewportId, Op? key = null) =>
        guard(viewportId != Guid.Empty, key.OrDefault().InvalidInput()).ToFin().Map(_ => (ViewportTarget)new IdCase(ViewportId: viewportId));
    public static Fin<ViewportTarget> Page(Guid pageViewId, Op? key = null) =>
        guard(pageViewId != Guid.Empty, key.OrDefault().InvalidInput()).ToFin().Map(_ => (ViewportTarget)new PageCase(PageViewId: pageViewId));
    public static Fin<ViewportTarget> Detail(Guid pageViewId, Guid detailId, Op? key = null) =>
        guard(pageViewId != Guid.Empty && detailId != Guid.Empty, key.OrDefault().InvalidInput()).ToFin()
            .Map(_ => (ViewportTarget)new DetailCase(PageViewId: pageViewId, DetailId: detailId));

    internal Fin<Seq<ViewportRef>> Resolve(RhinoDoc document, Op key) =>
        Switch(
            state: (Document: document, Op: key),
            activeCase: static (ctx, _) =>
                Optional(ctx.Document.Views.ActiveView).ToFin(Fail: ctx.Op.MissingContext())
                    .Map(view => Seq(ViewportRef.OfActive(view: view))),
            namedCase: static (ctx, target) =>
                Optional(ctx.Document.Views.Find(mainViewportName: target.Name, compareCase: false))
                    .ToFin(Fail: ctx.Op.InvalidInput())
                    .Map(view => Seq(ViewportRef.Of(view: view))),
            idCase: static (ctx, target) =>
                Optional(ctx.Document.Views.Find(mainViewportId: target.ViewportId)).ToFin(Fail: ctx.Op.InvalidInput())
                    .Map(view => Seq(ViewportRef.Of(view: view))),
            pageCase: static (ctx, target) =>
                PageOf(document: ctx.Document, pageViewId: target.PageViewId, key: ctx.Op)
                    .Map(page => Seq(ViewportRef.Of(view: page))),
            detailCase: static (ctx, target) =>
                from page in PageOf(document: ctx.Document, pageViewId: target.PageViewId, key: ctx.Op)
                from detail in toSeq(page.GetDetailViews())
                    .Find(row => row.Id == target.DetailId || row.Viewport.Id == target.DetailId)
                    .ToFin(Fail: ctx.Op.InvalidInput())
                select Seq(ViewportRef.OfDetail(view: page, detail: detail)),
            everyCase: static (ctx, target) => Fin.Succ(
                toSeq(ctx.Document.Views.GetViewList(filter: target.IncludePages ? ViewTypeFilter.All : ViewTypeFilter.Model))
                    .Map(static view => ViewportRef.Of(view: view))));

    private static Fin<RhinoPageView> PageOf(RhinoDoc document, Guid pageViewId, Op key) =>
        toSeq(document.Views.GetPageViews()).Find(page => page.MainViewport.Id == pageViewId).ToFin(Fail: key.InvalidInput());
}

// --- [MODELS] -------------------------------------------------------------------------------
internal readonly record struct ViewportRef(RhinoView View, RhinoViewport Viewport, Option<DetailViewObject> Detail, uint ChangeSerial) {
    internal static ViewportRef Of(RhinoView view) =>
        new(View: view, Viewport: view.MainViewport, Detail: Option<DetailViewObject>.None, ChangeSerial: view.MainViewport.ChangeCounter);
    internal static ViewportRef OfActive(RhinoView view) =>
        new(View: view, Viewport: view.ActiveViewport, Detail: Option<DetailViewObject>.None, ChangeSerial: view.ActiveViewport.ChangeCounter);
    internal static ViewportRef OfDetail(RhinoPageView view, DetailViewObject detail) =>
        new(View: view, Viewport: detail.Viewport, Detail: Some(detail), ChangeSerial: detail.Viewport.ChangeCounter);
    internal bool Stale => Viewport.ChangeCounter != ChangeSerial;

    internal Fin<TOut> Info<TOut>(Func<ViewportInfo, Fin<TOut>> project, Op key) =>
        key.Catch(() => new Lease<ViewportInfo>.Owned(Value: new ViewportInfo(Viewport)).Use(project));
}

// --- [SERVICES] -----------------------------------------------------------------------------
public sealed class ViewportLease : IDetachedDocumentResult {
    private readonly DocumentSession session;
    private readonly Seq<ViewportRef> rows;

    private ViewportLease(DocumentSession session, Seq<ViewportRef> rows) {
        this.session = session;
        this.rows = rows;
    }

    public int Count => rows.Count;
    public DocKey Key => session.Key;

    public Fin<Context> Context(Op? key = null) => session.Context(key: key);

    public static Fin<ViewportLease> Of(DocumentSession session, ViewportTarget target, Op? key = null) {
        Op op = key.OrDefault();
        return from owner in Optional(session).ToFin(Fail: op.MissingContext())
               from request in Optional(target).ToFin(Fail: op.InvalidInput())
               from lease in owner.Demand(
                   use: document =>
                       from resolved in request.Resolve(document: document, key: op)
                       from _ in guard(!resolved.IsEmpty, op.InvalidResult())
                       select new ViewportLease(session: owner, rows: resolved),
                   key: op,
                   needs: [SessionNeed.Redraw])
               select lease;
    }

    internal Fin<TOut> Use<TOut>(Func<ViewportRef, Fin<TOut>> borrow, Op key) =>
        guard(rows.Count == 1, key.InvalidInput()).ToFin()
            .Bind(_ => UseAll(borrow: borrow, key: key))
            .Bind(outputs => outputs.Head.ToFin(Fail: key.MissingContext()));

    internal Fin<Seq<TOut>> UseAll<TOut>(Func<ViewportRef, Fin<TOut>> borrow, Op key) {
        Seq<ViewportRef> held = rows;
        return HostThread.OnSession(
            session: session,
            body: _ => held.TraverseM(borrow).As().Map(static outputs => outputs.Strict()),
            op: key,
            needs: [SessionNeed.Redraw]);
    }

    public Seq<bool> Staleness() => rows.Map(static row => row.Stale);
}
```

## [03]-[POSE_MODEL]

- Owner: `CameraPose` — the kernel-composed pose: one `VectorFrame` whose `Value` plane carries the camera location at its origin, the view direction on its Z axis, and the camera right on its X axis — up re-derives at the host seat from direction and the live `CameraUp`, never a fourth pose field — plus the target point, the lens angle in radians, and the classified `ProjectionKind`. `LensState` `[ValueObject<double>]` admits the half-lens angle into `(0, π)`. `ProjectionKind` `[SmartEnum<int>]` names the host projection classes — `Parallel`, `Perspective`, `TwoPoint`, `ParallelReflected` — folded from the viewport's three projection predicates in one read; the host exposes no reflected read predicate, so `Classify` reads a reflected viewport as `Parallel` and the `ParallelReflected` row serves synthetic poses and the kernel `ViewProjectionIntent` lowering.
- Entry: `CameraPose.Read(ViewportLease, Op?)` projects the live camera through the lease; `CameraPose.Of(VectorFrame, Point3d, LensState, ProjectionKind, Op?)` admits a synthetic pose (the lowering target of the kernel `VectorIntent` view rows); `Write(ViewportLease, Op?)` composes the one internal `SeatOn` triplet — `SetCameraLocations`, `SetCameraDirection`, the `CameraAngle` setter — the single host write seat the operations rail's pose-carrying arms compose too, so no sibling pose writer exists.
- Law: the frame is read through `RhinoViewport.GetCameraFrame(frame: out Plane)` and admitted through `VectorFrame.Of` — a second local frame construction beside the kernel owner is the killed census defect; an up-vector fallback resolves through `ViewportInfo.CalculateCameraUpDirection(location:, direction:, angle:)`, never a hand-rolled orthogonalization.
- Law: the pose write orders direction before angle and refuses `updateTargetLocation` on the direction write so the admitted target survives the seat; the write returns the post-write `ChangeCounter` so callers observe the edit landed.
- Law: architectural view conventions are NOT pose recipes here — `Rasm.Processing` `VectorIntent.View` computes the convention pose from a subject bounds through the kernel `ViewConvention` rows, and this owner only admits and seats the projected `ViewPose`; a bounds-relative multiplier or elevation constant in this package is the killed `Architecture.cs` form.
- Boundary: reading and writing cross the same lease; a pose is a value, so two reads of a mutated viewport differ by construction and no cached pose masquerades as live state.

```csharp
// --- [TYPES] --------------------------------------------------------------------------------
[ValueObject<double>]
public readonly partial struct LensState {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) {
        validationError = double.IsFinite(value) && value > 0.0 && value < Math.PI
            ? validationError
            : new ValidationError("lens angle requires a finite radian value in (0, PI)");
    }
}

[SmartEnum<int>]
public sealed partial class ProjectionKind {
    public static readonly ProjectionKind Parallel = new(key: 0);
    public static readonly ProjectionKind Perspective = new(key: 1);
    public static readonly ProjectionKind TwoPoint = new(key: 2);
    public static readonly ProjectionKind ParallelReflected = new(key: 3);

    internal static ProjectionKind Classify(RhinoViewport viewport) =>
        (viewport.IsPerspectiveProjection, viewport.IsTwoPointPerspectiveProjection, viewport.IsParallelProjection) switch {
            (_, true, _) => TwoPoint,
            (true, false, _) => Perspective,
            _ => Parallel,
        };
}

// --- [MODELS] -------------------------------------------------------------------------------
public readonly record struct CameraPose(VectorFrame Frame, Point3d Target, LensState Lens, ProjectionKind Projection) {
    public static Fin<CameraPose> Of(VectorFrame frame, Point3d target, LensState lens, ProjectionKind projection, Op? key = null) {
        Op op = key.OrDefault();
        return from validTarget in op.AcceptValue(value: target)
               from kind in Optional(projection).ToFin(Fail: op.InvalidInput())
               select new CameraPose(Frame: frame, Target: validTarget, Lens: lens, Projection: kind);
    }

    public static Fin<CameraPose> Read(ViewportLease lease, Op? key = null) {
        Op op = key.OrDefault();
        return from context in lease.Context(key: op)
               from pose in lease.Use(borrow: row => ReadRow(row: row, context: context, key: op), key: op)
               select pose;
    }

    internal static Fin<CameraPose> ReadRow(ViewportRef row, Context context, Op key) =>
        row.Viewport.GetCameraFrame(frame: out Plane plane)
            ? (from admitted in VectorFrame.Of(origin: plane.Origin, normal: -plane.ZAxis, xHint: Some(plane.XAxis), context: context, key: key)
               from target in key.AcceptValue(value: row.Viewport.CameraTarget)
               from lens in key.AcceptValidated<LensState>(candidate: row.Viewport.CameraAngle)
               select new CameraPose(Frame: admitted, Target: target, Lens: lens, Projection: ProjectionKind.Classify(viewport: row.Viewport)))
            : Fin.Fail<CameraPose>(key.InvalidResult());

    public Fin<uint> Write(ViewportLease lease, Op? key = null) {
        Op op = key.OrDefault();
        CameraPose self = this;
        return lease.Use(borrow: row => op.Catch(() => {
            _ = self.SeatOn(viewport: row.Viewport);
            return Fin.Succ(value: row.Viewport.ChangeCounter);
        }), key: op);
    }

    internal Unit SeatOn(RhinoViewport viewport) {
        _ = Seat(viewport: viewport, target: Target, location: Frame.Value.Origin, direction: Frame.Value.ZAxis);
        viewport.CameraAngle = (double)Lens;
        return unit;
    }

    // The ONE host camera-write triplet core; every pose-carrying arm — full poses here, kernel ViewPose lowering on the operations rail — seats through it.
    internal static Unit Seat(RhinoViewport viewport, Point3d target, Point3d location, Vector3d direction) {
        viewport.SetCameraLocations(targetLocation: target, cameraLocation: location);
        viewport.SetCameraDirection(cameraDirection: direction, updateTargetLocation: false);
        return unit;
    }
}
```

## [04]-[HOST_ROWS]

- Owner: `CameraFrustum` — the six frustum planes plus aspect and the frustum bounding box, one read through `GetFrustum`/`FrustumAspect`/`GetFrustumBoundingBox`. `DepthProbe` `[Union]` — polymorphic depth: `AtPoint(Point3d)` through the single-distance overload, `OfBounds(BoundingBox)` and `OfSphere(Sphere)` through the near/far pair — one `Read` folding the three host overloads. `VisibilityProbe` `[Union]` — `Point`/`Bounds`/`Geometry` visibility through the `IsVisible` overload family. `CameraDof` — the full focal-blur row (`FocalBlurMode`/`Distance`/`Aperture`/`Jitter`/`SampleCount`) read from and written to a `ViewInfo`. `CPlaneGrid` — the construction-plane row from `GetConstructionPlane`, carrying plane, grid spacing, and snap spacing as values. `ViewMapping` `[SmartEnum<int>]` — the coordinate-transform rows (`WorldToScreen`, `WorldToClip`, `WorldToCamera`, `ScreenToWorld`, `ClipToWorld`, `CameraToWorld`) whose `(Source, Destination)` columns feed one `ViewportInfo.GetXform` read; the screen-ray inverse rides the same owner as `FrustumLineAt` over `GetFrustumLine`.
- Law: every row is a value projection through the lease — `DepthExtent`, `CameraFrustum`, and `CPlaneGrid` carry doubles, `Plane`, and `BoundingBox` values; the killed forms are the census screen carriers (`System.Drawing.Point`/`Rectangle` as camera state) and depth reads scattered per call site.
- Law: `ViewMapping` is the ONE world/screen/clip/camera correspondence — forward and inverse directions are rows of one owner, and a consumer needing pixels-per-unit reads `GetWorldToScreenScale` through `PixelScale`, never a re-derived projection ratio; the transform reads through a `ViewportInfo.GetXform` snapshot because that member returns `Transform.Unset` on failure where the live `RhinoViewport.GetTransform` returns `Identity` and makes refusal invisible to `IsValid`.
- Boundary: depth-of-field lives on `ViewInfo` (named-view state), not the live viewport — `CameraDof.Read`/`Write` take the `ViewInfo` the render and named-view rails hold, and the write is host mutation gated by the operations rail.

```csharp
// --- [TYPES] --------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DepthProbe {
    private DepthProbe() { }
    public sealed record AtPoint(Point3d Value) : DepthProbe;
    public sealed record OfBounds(BoundingBox Value) : DepthProbe;
    public sealed record OfSphere(Sphere Value) : DepthProbe;

    public Fin<DepthExtent> Read(ViewportLease lease, Op? key = null) {
        Op op = key.OrDefault();
        DepthProbe self = this;
        return lease.Use(borrow: row => self.Switch(
            state: (Viewport: row.Viewport, Op: op),
            atPoint: static (ctx, probe) => ctx.Viewport.GetDepth(point: probe.Value, distance: out double at)
                ? Fin.Succ(new DepthExtent(Near: at, Far: at))
                : Fin.Fail<DepthExtent>(ctx.Op.InvalidResult()),
            ofBounds: static (ctx, probe) => ctx.Viewport.GetDepth(bbox: probe.Value, nearDistance: out double near, farDistance: out double far)
                ? Fin.Succ(new DepthExtent(Near: near, Far: far))
                : Fin.Fail<DepthExtent>(ctx.Op.InvalidResult()),
            ofSphere: static (ctx, probe) => ctx.Viewport.GetDepth(sphere: probe.Value, nearDistance: out double near, farDistance: out double far)
                ? Fin.Succ(new DepthExtent(Near: near, Far: far))
                : Fin.Fail<DepthExtent>(ctx.Op.InvalidResult())),
            key: op);
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record VisibilityProbe {
    private VisibilityProbe() { }
    public sealed record PointCase(Point3d Value) : VisibilityProbe;
    public sealed record BoundsCase(BoundingBox Value) : VisibilityProbe;
    public sealed record GeometryCase(GeometryBase Value) : VisibilityProbe;

    public Fin<bool> Read(ViewportLease lease, Op? key = null) {
        Op op = key.OrDefault();
        VisibilityProbe self = this;
        return lease.Use(borrow: row => self.Switch(
            state: (Viewport: row.Viewport, Op: op),
            pointCase: static (ctx, probe) => guard(probe.Value.IsValid, ctx.Op.InvalidInput()).ToFin()
                .Map(_ => ctx.Viewport.IsVisible(point: probe.Value)),
            boundsCase: static (ctx, probe) => guard(probe.Value.IsValid, ctx.Op.InvalidInput()).ToFin()
                .Map(_ => ctx.Viewport.IsVisible(bbox: probe.Value)),
            geometryCase: static (ctx, probe) => Optional(probe.Value).ToFin(Fail: ctx.Op.InvalidInput())
                .Map(geometry => ctx.Viewport.IsVisible(bbox: geometry.GetBoundingBox(accurate: false)))),
            key: op);
    }
}

[SmartEnum<int>]
public sealed partial class ViewMapping {
    public static readonly ViewMapping WorldToScreen = new(key: 0, source: CoordinateSystem.World, destination: CoordinateSystem.Screen);
    public static readonly ViewMapping WorldToClip = new(key: 1, source: CoordinateSystem.World, destination: CoordinateSystem.Clip);
    public static readonly ViewMapping WorldToCamera = new(key: 2, source: CoordinateSystem.World, destination: CoordinateSystem.Camera);
    public static readonly ViewMapping ScreenToWorld = new(key: 3, source: CoordinateSystem.Screen, destination: CoordinateSystem.World);
    public static readonly ViewMapping ClipToWorld = new(key: 4, source: CoordinateSystem.Clip, destination: CoordinateSystem.World);
    public static readonly ViewMapping CameraToWorld = new(key: 5, source: CoordinateSystem.Camera, destination: CoordinateSystem.World);

    public CoordinateSystem Source { get; }
    public CoordinateSystem Destination { get; }
}

// --- [MODELS] -------------------------------------------------------------------------------
public readonly record struct DepthExtent(double Near, double Far);

public readonly record struct CameraFrustum(double Left, double Right, double Bottom, double Top, double Near, double Far, double Aspect, BoundingBox Bounds) {
    public static Fin<CameraFrustum> Read(ViewportLease lease, Op? key = null) {
        Op op = key.OrDefault();
        return lease.Use(borrow: row => ReadRow(row: row, key: op), key: op);
    }

    internal static Fin<CameraFrustum> ReadRow(ViewportRef row, Op key) =>
        row.Viewport.GetFrustum(left: out double left, right: out double right, bottom: out double bottom, top: out double top, nearDistance: out double near, farDistance: out double far)
            ? Fin.Succ(new CameraFrustum(Left: left, Right: right, Bottom: bottom, Top: top, Near: near, Far: far, Aspect: row.Viewport.FrustumAspect, Bounds: row.Viewport.GetFrustumBoundingBox()))
            : Fin.Fail<CameraFrustum>(key.InvalidResult());
}

public readonly record struct CameraDof(ViewInfoFocalBlurModes Mode, double Distance, double Aperture, double Jitter, uint SampleCount) {
    public static CameraDof Read(ViewInfo view) =>
        new(Mode: view.FocalBlurMode, Distance: view.FocalBlurDistance, Aperture: view.FocalBlurAperture, Jitter: view.FocalBlurJitter, SampleCount: view.FocalBlurSampleCount);

    public Unit Write(ViewInfo view) {
        view.FocalBlurMode = Mode;
        view.FocalBlurDistance = Distance;
        view.FocalBlurAperture = Aperture;
        view.FocalBlurJitter = Jitter;
        view.FocalBlurSampleCount = SampleCount;
        return unit;
    }
}

public readonly record struct CPlaneGrid(Plane Plane, double GridSpacing, double SnapSpacing, int GridLineCount) {
    public static Fin<CPlaneGrid> Read(ViewportLease lease, Op? key = null) {
        Op op = key.OrDefault();
        return lease.Use(borrow: row => op.Catch(() => {
            DocObjects.ConstructionPlane cplane = row.Viewport.GetConstructionPlane();
            return Fin.Succ(new CPlaneGrid(Plane: cplane.Plane, GridSpacing: cplane.GridSpacing, SnapSpacing: cplane.SnapSpacing, GridLineCount: cplane.GridLineCount));
        }), key: op);
    }
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class ViewTransforms {
    extension(ViewportLease lease) {
        public Fin<Transform> Mapping(ViewMapping mapping, Op? key = null) {
            Op op = key.OrDefault();
            return lease.Use(borrow: row => row.Info(project: info =>
                info.GetXform(sourceSystem: mapping.Source, destinationSystem: mapping.Destination) is { IsValid: true } transform
                    ? Fin.Succ(transform)
                    : Fin.Fail<Transform>(op.InvalidResult()), key: op), key: op);
        }

        public Fin<double> PixelScale(Point3d at, Op? key = null) {
            Op op = key.OrDefault();
            return lease.Use(borrow: row =>
                row.Viewport.GetWorldToScreenScale(pointInFrustum: at, pixelsPerUnit: out double ppu) && double.IsFinite(ppu) && ppu > 0.0
                    ? Fin.Succ(ppu)
                    : Fin.Fail<double>(op.InvalidResult()),
                key: op);
        }

        public Fin<Line> FrustumLineAt(double screenX, double screenY, Op? key = null) {
            Op op = key.OrDefault();
            return lease.Use(borrow: row =>
                row.Viewport.GetFrustumLine(screenX: screenX, screenY: screenY, worldLine: out Line line)
                    ? Fin.Succ(line)
                    : Fin.Fail<Line>(op.InvalidResult()),
                key: op);
        }
    }
}
```

## [05]-[SNAPSHOT]

- Owner: `CameraSnapshot` — the `ViewportInfo` value adapter: the pose, frustum, projection kind, lens, frame-plane corners at a chosen depth, and the identity pair (`DocKey`, `ChangeCounter`) that makes staleness a fact. Every disposable `ViewportInfo` rides the row's `Info` projection — minted, projected, and released inside one borrow — so no disposable snapshot survives; the census-era public `ViewportInfo` retention is dead.
- Entry: `CameraSnapshot.Take(ViewportLease, Op?)` captures pose, frustum, quad, and serial under ONE borrow, so the stamped `ChangeCounter` names exactly the state the values project — a per-value borrow can stamp a serial that postdates the pose it certifies; `Restore(ViewportLease, Op?)` replays the stored pose through the one `CameraPose.Write` seat after proving the document identity — the one snapshot round-trip; `Stale(ViewportLease)` compares BOTH identity axes, because a reopened document can alias a stored counter while the `DocKey` cannot.
- Law: frame-plane corners read through `ViewportInfo.GetFramePlaneCorners(depth:)` in host order `(BottomLeft, BottomRight, TopLeft, TopRight)` and travel as a typed quad, so downstream capture and draw code consumes named corners instead of index arithmetic.
- Law: snapshot values feed three consumers with one shape — the operations rail's view stack, the capture specification's window mapping, and the motion compiler's keyframe sampling — so a per-consumer snapshot variant is the collapsed form.
- Boundary: `Restore` is a host mutation and rides the operations rail's UI-thread and redraw policy when composed there; the direct member here exists for the render rail, whose `ViewInfo` handoff is its own seam.

```csharp
// --- [MODELS] -------------------------------------------------------------------------------
public readonly record struct FramePlaneQuad(Point3d BottomLeft, Point3d BottomRight, Point3d TopLeft, Point3d TopRight);

public sealed record CameraSnapshot(
    CameraPose Pose,
    CameraFrustum Frustum,
    FramePlaneQuad NearQuad,
    DocKey Document,
    uint ChangeSerial) {

    public static Fin<CameraSnapshot> Take(ViewportLease lease, Op? key = null) {
        Op op = key.OrDefault();
        return from context in lease.Context(key: op)
               from snapshot in lease.Use(borrow: row =>
                   from pose in CameraPose.ReadRow(row: row, context: context, key: op)
                   from frustum in CameraFrustum.ReadRow(row: row, key: op)
                   from quad in row.Info(project: info => info.GetFramePlaneCorners(depth: frustum.Near) is { Length: 4 } corners
                       ? Fin.Succ(new FramePlaneQuad(BottomLeft: corners[0], BottomRight: corners[1], TopLeft: corners[2], TopRight: corners[3]))
                       : Fin.Fail<FramePlaneQuad>(op.InvalidResult()), key: op)
                   select new CameraSnapshot(Pose: pose, Frustum: frustum, NearQuad: quad, Document: lease.Key, ChangeSerial: row.Viewport.ChangeCounter),
                   key: op)
               select snapshot;
    }

    public Fin<Unit> Restore(ViewportLease lease, Op? key = null) {
        Op op = key.OrDefault();
        CameraSnapshot self = this;
        return from _ in guard(lease.Key == self.Document, op.InvalidInput()).ToFin()
               from applied in self.Pose.Write(lease: lease, key: op)
               select unit;
    }

    public Fin<bool> Stale(ViewportLease lease, Op? key = null) {
        Op op = key.OrDefault();
        CameraSnapshot self = this;
        return lease.Use(borrow: row => Fin.Succ(lease.Key != self.Document || row.Viewport.ChangeCounter != self.ChangeSerial), key: op);
    }
}
```
