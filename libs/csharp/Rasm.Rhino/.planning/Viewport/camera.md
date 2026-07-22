# [RASM_RHINO_CAMERA]

Camera ownership (`Rasm.Rhino.Viewport`) separates kernel pose and intent, session-scoped native borrows over the Document-owned `ViewportTarget` address, and value-only host projections. `CameraPose` composes `Rasm.Numerics.VectorFrame`; `ViewportLease` retains only `DocumentSession` plus `ViewportTarget`; `CameraSnapshot` disposes `ViewportInfo` before egress. Frustum, depth, depth-of-field, construction plane, detail scale, visibility, and coordinate transforms remain typed rows on one `Fin` rail.

## [01]-[INDEX]

- [02]-[SCOPE_LEASE]: `ViewportBorrowMode` the broadcast redraw-suppression gate and `ViewportLease` the session-gated borrow over the Document-owned `ViewportTarget` address.
- [03]-[POSE_MODEL]: `CameraPose` over the kernel `VectorFrame`, `LensAngle`, `ProjectionKind` classification, and the pose read/write pair on one owner.
- [04]-[HOST_ROWS]: `CameraFrustum`, `DepthProbe`, `VisibilityProbe`, `CameraDof`, `CPlaneGrid`, `DetailLength`, and `ViewMapping`.
- [05]-[SNAPSHOT]: `CameraSnapshot` — the `ViewportInfo` value adapter with staleness evidence and the restore seam.

## [02]-[SCOPE_LEASE]

- Owner: `ViewportLease` is the sole session-gated borrow surface, retaining only the `DocumentSession` plus the Document-owned `ViewportTarget` address; `ViewportBorrowMode` `[SmartEnum<int>]` gates broadcast redraw suppression. Every `Use` resolves and consumes the Document `ViewportRef` rows inside one `HostWork<T>.Session` body run through `HostThread.Run`, so no `RhinoView`, `RhinoViewport`, or `DetailViewObject` survives its borrow.
- Entry: `ViewportTarget.Active` / `Named` / `Id` / `Page` / `Detail` / `Every` mint the durable address on the Document owner; `ViewportLease.Of(DocumentSession, ViewportTarget, Op?)` admits it; `Use<T>` and `UseAll<T>` resolve the current native rows through the Document address resolver during the borrow and reject missing or ambiguous scalar addresses.
- Law: a detail edit is committed through `CommitViewportChanges` on the operations rail, not observed — the lease only proves which rows are details, reading the `DetailViewObject` the Document `ViewportRef` carries.
- Law: durable identity is `DocKey` plus `ViewportTarget`; mutation identity is sampled from `RhinoViewport.ChangeCounter` by the operation that projects the value. A lease never stamps a native instance and therefore cannot become a stale handle cache.
- Law: every borrow crosses the session capability rail through `HostThread.Run` over a `HostWork<T>.Session` — the HostUi seam whose closure capture carries the typed value while the demand's own result stays the session `DocKey` — so the session's result constraint holds without constraining consumer value types, and no parallel detachment envelope exists beside that seam. Broadcast redraw suppression restores the captured state on every exit, retries a rejected restore once, and combines primary and cleanup faults.
- Boundary: the lease owns no host resource and is not `IDisposable`; each use re-resolves the address, executes, and discards every native reference before the host-thread closure returns.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using Rasm.Domain;
using Rasm.Numerics;
using Rasm.Rhino.Document;
using Rasm.Rhino.HostUi;
using System.Runtime.InteropServices;

namespace Rasm.Rhino.Viewport;

// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<int>]
internal sealed partial class ViewportBorrowMode {
    internal static readonly ViewportBorrowMode Observe = new(key: 0, suppress: static _ => false);
    internal static readonly ViewportBorrowMode Mutate = new(key: 1, suppress: static count => count >= 3);

    [UseDelegateFromConstructor]
    internal partial bool Suppress(int count);
}

[SmartEnum<int>]
internal sealed partial class ViewportCardinality {
    internal static readonly ViewportCardinality Scalar = new(
        key: 0,
        admit: static (count, op) => guard(count == 1, op.InvalidInput()).ToFin());
    internal static readonly ViewportCardinality Set = new(
        key: 1,
        admit: static (count, op) => guard(count > 0, op.MissingContext()).ToFin());

    [UseDelegateFromConstructor]
    internal partial Fin<Unit> Admit(int count, Op op);
}

// --- [SERVICES] -----------------------------------------------------------------------------
public sealed class ViewportLease : IDetachedDocumentResult {
    private readonly DocumentSession session;
    private readonly ViewportTarget target;

    private ViewportLease(DocumentSession session, ViewportTarget target) {
        this.session = session;
        this.target = target;
    }

    public DocKey Key => session.Key;

    internal DocumentSession Session => session;
    internal ViewportTarget Target => target;

    public Fin<Context> Context(Op? key = null) => session.Context(key: key);

    public static Fin<ViewportLease> Of(DocumentSession session, ViewportTarget target, Op? key = null) {
        Op op = key.OrDefault();
        return from owner in Optional(session).ToFin(Fail: op.MissingContext())
               from request in op.Need(value: target)
               select new ViewportLease(session: owner, target: request);
    }

    internal static Fin<ViewportLease> Admit(ViewportLease? lease, Op key) =>
        key.Need(value: lease);

    internal Fin<TOut> Use<TOut>(Func<ViewportRef, Fin<TOut>> borrow, Op key) =>
        Use(borrow: (_, row) => borrow(row), key: key);

    internal Fin<TOut> Use<TOut>(Func<RhinoDoc, ViewportRef, Fin<TOut>> borrow, Op key) =>
        BorrowAll(
            borrow: borrow,
            terminal: static (_, _) => Fin.Succ(unit),
            mode: ViewportBorrowMode.Observe,
            cardinality: ViewportCardinality.Scalar,
            key: key)
            .Bind(outputs => outputs.Head.ToFin(Fail: key.MissingContext()));

    internal Fin<Seq<TOut>> UseAll<TOut>(
        Func<RhinoDoc, ViewportRef, Fin<TOut>> borrow,
        Func<RhinoDoc, int, Fin<Unit>> terminal,
        ViewportBorrowMode mode,
        Op key) => BorrowAll(
            borrow: borrow,
            terminal: terminal,
            mode: mode,
            cardinality: ViewportCardinality.Set,
            key: key);

    private Fin<Seq<TOut>> BorrowAll<TOut>(
        Func<RhinoDoc, ViewportRef, Fin<TOut>> borrow,
        Func<RhinoDoc, int, Fin<Unit>> terminal,
        ViewportBorrowMode mode,
        ViewportCardinality cardinality,
        Op key) =>
        HostThread.Run(
            work: new HostWork<Seq<TOut>>.Session(
                Document: session,
                Needs: [SessionNeed.Redraw],
                Body: document =>
                    from rows in target.Resolve(document: document, key: key)
                    from _ in cardinality.Admit(count: rows.Count, op: key)
                    from outputs in mode.Suppress(count: rows.Count)
                        ? Suppressed(document: document, rows: rows, borrow: borrow, key: key)
                        : rows.TraverseM(row => Capture(document: document, row: row, borrow: borrow, key: key)).As()
                    from __ in terminal(document, rows.Count)
                    select outputs.Strict()),
            key: key);

    private static Fin<Seq<TOut>> Suppressed<TOut>(
        RhinoDoc document,
        Seq<ViewportRef> rows,
        Func<RhinoDoc, ViewportRef, Fin<TOut>> borrow,
        Op key) => key.Catch(() => Fin.Succ(value: document.Views.RedrawEnabled)).Bind(prior => {
            Fin<Seq<TOut>> primary = key.Catch(() => {
                document.Views.EnableRedraw(enable: false, redrawDocument: false, redrawLayers: false);
                return rows.TraverseM(row => Capture(document: document, row: row, borrow: borrow, key: key)).As();
            });
            Fin<Unit> cleanup = RestoreRedraw(document: document, enabled: prior, key: key);
            return cleanup.Match(
                Succ: _ => primary,
                Fail: restore => primary.Match(
                    Succ: _ => Fin.Fail<Seq<TOut>>(error: restore),
                    Fail: failure => Fin.Fail<Seq<TOut>>(error: failure + restore)));
        });

    private static Fin<TOut> Capture<TOut>(
        RhinoDoc document,
        ViewportRef row,
        Func<RhinoDoc, ViewportRef, Fin<TOut>> borrow,
        Op key) => key.Catch(() => borrow(document, row));

    private static Fin<Unit> RestoreRedraw(RhinoDoc document, bool enabled, Op key) =>
        Restore(document: document, enabled: enabled, key: key).BindFail(primary =>
            Restore(document: document, enabled: enabled, key: key).Match(
                Succ: _ => Fin.Fail<Unit>(error: primary),
                Fail: retry => Fin.Fail<Unit>(error: primary + retry)));

    private static Fin<Unit> Restore(RhinoDoc document, bool enabled, Op key) => key.Catch(() => {
        document.Views.EnableRedraw(enable: enabled, redrawDocument: false, redrawLayers: false);
        return Fin.Succ(value: unit);
    });
}
```

## [03]-[POSE_MODEL]

- Owner: `CameraPose` composes `VectorFrame`, target, `LensAngle`, and the observable `ProjectionKind` rows `Parallel`, `Perspective`, and `TwoPoint`. RhinoCommon exposes no reflected read predicate, so reflected projection remains an explicit `ProjectionChange.ReflectedCase` command and never masquerades as readable pose state.
- Entry: `CameraPose.Read(ViewportLease, Op?)` projects the live camera through the lease; `CameraPose.Of(VectorFrame, Point3d, LensAngle, ProjectionKind, Op?)` admits a synthetic pose; `CameraPose.Admit` is the shared outer storage seam consumed by writes and tracks; `Write(ViewportLease, Op?)` enters `Cameras.Apply`, which proves the requested `ProjectionKind` already matches the live viewport and composes the one internal `SeatOn` triplet. Projection transitions remain explicit `CameraOp.ProjectionCase` operations because RhinoCommon exposes no reflected read predicate and its perspective transition consumes a lens-length contract distinct from `CameraAngle`.
- Law: the frame is read through `RhinoViewport.GetCameraFrame(frame: out Plane)` and admitted through `VectorFrame.Of` — a second local frame construction beside the kernel owner is the killed census defect; an up-vector fallback resolves through `ViewportInfo.CalculateCameraUpDirection(location:, direction:, angle:)`, never a hand-rolled orthogonalization.
- Law: the pose write orders direction before angle and refuses `updateTargetLocation` on the direction write so the admitted target survives the seat; a mismatched projection is a typed refusal rather than a pose that silently omits one declared field, and the write returns the post-write `ChangeCounter`.
- Law: architectural view conventions are NOT pose recipes here — `Rasm.Drawing` `ViewConvention.Pose` computes the convention pose from a subject bounds through the kernel catalog rows, and this owner only admits and seats the projected `ViewPose`; a bounds-relative multiplier or elevation constant in this package is the killed `Architecture.cs` form.
- Boundary: reading and writing cross the same lease; a pose is a value, so two reads of a mutated viewport differ by construction and no cached pose masquerades as live state.

```csharp
// --- [TYPES] --------------------------------------------------------------------------------
[ValueObject<double>]
public readonly partial struct LensAngle {
    [BoundaryAdapter]
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

    internal static ProjectionKind Classify(RhinoViewport viewport) =>
        (viewport.IsPerspectiveProjection, viewport.IsTwoPointPerspectiveProjection, viewport.IsParallelProjection) switch {
            (_, true, _) => TwoPoint,
            (true, false, _) => Perspective,
            _ => Parallel,
        };

    internal bool Accepts(RhinoViewport viewport) => this == Classify(viewport: viewport);
}

// --- [MODELS] -------------------------------------------------------------------------------
public readonly record struct CameraPose(VectorFrame Frame, Point3d Target, LensAngle Angle, ProjectionKind Projection) {
    public static Fin<CameraPose> Of(VectorFrame frame, Point3d target, LensAngle angle, ProjectionKind projection, Op? key = null) =>
        Admit(pose: new CameraPose(Frame: frame, Target: target, Angle: angle, Projection: projection), key: key.OrDefault());

    internal static Fin<CameraPose> Admit(CameraPose pose, Op key) =>
        from _frame in guard(pose.Frame.Value.IsValid, key.InvalidInput()).ToFin()
        from target in key.AcceptValue(value: pose.Target)
        from angle in key.AcceptValidated<LensAngle>(candidate: (double)pose.Angle)
        from projection in key.Need(value: pose.Projection)
        select pose with { Target = target, Angle = angle, Projection = projection };

    public static Fin<CameraPose> Read(ViewportLease lease, Op? key = null) {
        Op op = key.OrDefault();
        return ViewportLease.Admit(lease: lease, key: op).Bind(owner => owner.Use(
            borrow: (document, row) => Rasm.Domain.Context.Of(doc: document).ToFin()
                .Bind(context => ReadRow(row: row, context: context, key: op)),
            key: op));
    }

    internal static Fin<CameraPose> ReadRow(ViewportRef row, Context context, Op key) => key.Catch(() =>
        row.Viewport.GetCameraFrame(frame: out Plane plane)
            ? (from admitted in VectorFrame.Of(origin: plane.Origin, normal: -plane.ZAxis, xHint: Some(plane.XAxis), context: context, key: key)
               from target in key.AcceptValue(value: row.Viewport.CameraTarget)
               from angle in key.AcceptValidated<LensAngle>(candidate: row.Viewport.CameraAngle)
               select new CameraPose(Frame: admitted, Target: target, Angle: angle, Projection: ProjectionKind.Classify(viewport: row.Viewport)))
            : Fin.Fail<CameraPose>(key.InvalidResult()));

    public Fin<uint> Write(ViewportLease lease, Op? key = null) {
        Op op = key.OrDefault();
        return from owner in ViewportLease.Admit(lease: lease, key: op)
               from operation in CameraOp.Pose(pose: this, key: op)
               from receipt in Cameras.Apply(
                   session: owner.Session,
                   target: owner.Target,
                   operation: operation,
                   key: op)
               from serial in receipt.Switch(
                   immediateCase: row => row.Serials.Last.ToFin(Fail: op.InvalidResult()),
                   motionCase: _ => Fin.Fail<(uint Before, uint After)>(op.InvalidResult()))
               select serial.After;
    }

    internal Unit SeatOn(RhinoViewport viewport) {
        _ = Seat(viewport: viewport, target: Target, location: Frame.Value.Origin, direction: Frame.Value.ZAxis);
        viewport.CameraAngle = (double)Angle;
        return unit;
    }

    internal static Unit Seat(RhinoViewport viewport, Point3d target, Point3d location, Vector3d direction) {
        viewport.SetCameraLocations(targetLocation: target, cameraLocation: location);
        viewport.SetCameraDirection(cameraDirection: direction, updateTargetLocation: false);
        return unit;
    }
}
```

## [04]-[HOST_ROWS]

- Owner: `CameraFrustum` owns six planes, aspect, and bounds; `DepthProbe` and `VisibilityProbe` own polymorphic spatial evidence; `CameraDof` owns focal-blur state and `CameraDofField` the ordered setter vocabulary; `CPlaneGrid` owns plane, spacing, visibility, depth, frequency, and axis/line colors; `DetailLength` owns paper/model conversion through `TryGetPaperLength` and `TryGetModelLength`; `ViewMapping` owns the coordinate-transform rows projected by `ViewportInfo.GetXform`.
- Law: every row is a value projection through the lease — `DepthExtent`, `CameraFrustum`, and `CPlaneGrid` carry doubles, `Plane`, and `BoundingBox` values; the killed forms are the census screen carriers (`System.Drawing.Point`/`Rectangle` as camera state) and depth reads scattered per call site.
- Law: `ViewMapping` is the ONE world/screen/clip/camera correspondence — one admitted `(Source, Destination)` pair generates the complete directional space, and a consumer needing pixels-per-unit reads `GetWorldToScreenScale` through `PixelScale`, never a re-derived projection ratio; the transform reads through a `ViewportInfo.GetXform` snapshot because that member returns `Transform.Unset` on failure where the live `RhinoViewport.GetTransform` returns `Identity` and makes refusal invisible to `IsValid`.
- Boundary: depth-of-field lives on `ViewInfo` (named-view state), not the live viewport — `CameraDof.Read`/`Write` take the `ViewInfo` the render and named-view rails hold, and the write is host mutation gated by the operations rail. `Write` captures all focal-blur fields before mutation, applies the ordered field rows fail-fast, and restores the complete prior state through one compensation path when any setter fails.

```csharp signature
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
        return ViewportLease.Admit(lease: lease, key: op).Bind(owner => owner.Use(borrow: row => self.Switch(
            (Viewport: row.Viewport, Op: op),
            atPoint: static (ctx, probe) => Extent(hit: ctx.Viewport.GetDepth(point: probe.Value, distance: out double at), near: at, far: at, key: ctx.Op),
            ofBounds: static (ctx, probe) => Extent(hit: ctx.Viewport.GetDepth(bbox: probe.Value, nearDistance: out double near, farDistance: out double far), near: near, far: far, key: ctx.Op),
            ofSphere: static (ctx, probe) => Extent(hit: ctx.Viewport.GetDepth(sphere: probe.Value, nearDistance: out double near, farDistance: out double far), near: near, far: far, key: ctx.Op)),
            key: op));
    }

    private static Fin<DepthExtent> Extent(bool hit, double near, double far, Op key) =>
        hit ? DepthExtent.Of(near: near, far: far, key: key) : Fin.Fail<DepthExtent>(key.InvalidResult());
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record VisibilityProbe {
    private VisibilityProbe() { }
    public sealed record PointCase(Point3d Value) : VisibilityProbe;
    public sealed record BoundsCase(BoundingBox Value) : VisibilityProbe;
    public sealed record GeometryBoundsCase(GeometryBase Value) : VisibilityProbe;

    public Fin<bool> Read(ViewportLease lease, Op? key = null) {
        Op op = key.OrDefault();
        VisibilityProbe self = this;
        return ViewportLease.Admit(lease: lease, key: op).Bind(owner => owner.Use(borrow: row => self.Switch(
            (Viewport: row.Viewport, Op: op),
            pointCase: static (ctx, probe) => guard(probe.Value.IsValid, ctx.Op.InvalidInput()).ToFin().Map(_ => ctx.Viewport.IsVisible(point: probe.Value)),
            boundsCase: static (ctx, probe) => guard(probe.Value.IsValid, ctx.Op.InvalidInput()).ToFin().Map(_ => ctx.Viewport.IsVisible(bbox: probe.Value)),
            geometryBoundsCase: static (ctx, probe) =>
                from geometry in ctx.Op.Need(value: probe.Value)
                from _ in guard(geometry.IsValid, ctx.Op.InvalidInput()).ToFin()
                from bounds in ctx.Op.Catch(() => Fin.Succ(geometry.GetBoundingBox(accurate: false)))
                from __ in guard(bounds.IsValid, ctx.Op.InvalidInput()).ToFin()
                select ctx.Viewport.IsVisible(bbox: bounds)),
            key: op));
    }
}

[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct ViewMapping {
    public CoordinateSystem Source { get; }
    public CoordinateSystem Destination { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref CoordinateSystem source,
        ref CoordinateSystem destination) {
        validationError = Enum.IsDefined(value: source) && Enum.IsDefined(value: destination)
            ? validationError
            : new ValidationError(message: "coordinate system is invalid");
    }
}

// --- [MODELS] -------------------------------------------------------------------------------
[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct DepthExtent {
    public double Near { get; }
    public double Far { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double near, ref double far) {
        validationError = double.IsFinite(near) && double.IsFinite(far) && near <= far
            ? validationError
            : new ValidationError(message: "depth extent is invalid");
    }

    internal static Fin<DepthExtent> Of(double near, double far, Op key) =>
        key.Catch(() => Fin.Succ(Create(near: near, far: far)));
}

[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct CameraFrustum {
    public double Left { get; }
    public double Right { get; }
    public double Bottom { get; }
    public double Top { get; }
    public double Near { get; }
    public double Far { get; }
    public double Aspect { get; }
    public BoundingBox Bounds { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref double left,
        ref double right,
        ref double bottom,
        ref double top,
        ref double near,
        ref double far,
        ref double aspect,
        ref BoundingBox bounds) {
        validationError = left < right && bottom < top && near < far && aspect > 0.0 && bounds.IsValid
            && new[] { left, right, bottom, top, near, far, aspect }.All(double.IsFinite)
                ? validationError
                : new ValidationError(message: "camera frustum is invalid");
    }

    public static Fin<CameraFrustum> Read(ViewportLease lease, Op? key = null) {
        Op op = key.OrDefault();
        return ViewportLease.Admit(lease: lease, key: op)
            .Bind(owner => owner.Use(borrow: row => ReadRow(row: row, key: op), key: op));
    }

    internal static Fin<CameraFrustum> ReadRow(ViewportRef row, Op key) =>
        row.Viewport.GetFrustum(left: out double left, right: out double right, bottom: out double bottom, top: out double top, nearDistance: out double near, farDistance: out double far)
            ? key.Catch(() => Fin.Succ(Create(
                left: left,
                right: right,
                bottom: bottom,
                top: top,
                near: near,
                far: far,
                aspect: row.Viewport.FrustumAspect,
                bounds: row.Viewport.GetFrustumBoundingBox())))
            : Fin.Fail<CameraFrustum>(key.InvalidResult());
}

[SmartEnum<int>]
internal sealed partial class CameraDofField {
    internal static readonly CameraDofField Mode = new(
        key: 0, set: static (target, value) => target.FocalBlurMode = value.Mode);
    internal static readonly CameraDofField Distance = new(
        key: 1, set: static (target, value) => target.FocalBlurDistance = value.Distance);
    internal static readonly CameraDofField Aperture = new(
        key: 2, set: static (target, value) => target.FocalBlurAperture = value.Aperture);
    internal static readonly CameraDofField Jitter = new(
        key: 3, set: static (target, value) => target.FocalBlurJitter = value.Jitter);
    internal static readonly CameraDofField SampleCount = new(
        key: 4, set: static (target, value) => target.FocalBlurSampleCount = value.SampleCount);

    [UseDelegateFromConstructor]
    internal partial void Set(ViewInfo target, CameraDof value);
}

[ComplexValueObject]
public sealed partial class CameraDof {
    public ViewInfoFocalBlurModes Mode { get; }
    public double Distance { get; }
    public double Aperture { get; }
    public double Jitter { get; }
    public uint SampleCount { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref ViewInfoFocalBlurModes mode,
        ref double distance,
        ref double aperture,
        ref double jitter,
        ref uint sampleCount) {
        validationError = Enum.IsDefined(value: mode) && sampleCount >= 1u
            && new[] { distance, aperture, jitter }.All(static value => double.IsFinite(value) && value >= 0.0)
                ? validationError
                : new ValidationError(message: "camera depth of field is invalid");
    }

    public static Fin<CameraDof> Of(
        ViewInfoFocalBlurModes mode,
        double distance,
        double aperture,
        double jitter,
        uint sampleCount,
        Op? key = null) =>
        key.OrDefault().Catch(() => Fin.Succ(Create(
            mode: mode,
            distance: distance,
            aperture: aperture,
            jitter: jitter,
            sampleCount: sampleCount)));

    public static Fin<CameraDof> Read(ViewInfo view, Op? key = null) {
        Op op = key.OrDefault();
        return op.Need(value: view).Bind(source => op.Catch(() => Of(
            mode: source.FocalBlurMode,
            distance: source.FocalBlurDistance,
            aperture: source.FocalBlurAperture,
            jitter: source.FocalBlurJitter,
            sampleCount: source.FocalBlurSampleCount,
            key: op)));
    }

    public Fin<Unit> Write(ViewInfo view, Op? key = null) {
        Op op = key.OrDefault();
        CameraDof self = this;
        return op.Need(value: view).Bind(target => Read(view: target, key: op).Bind(prior =>
            Apply(target: target, value: self, key: op).BindFail(primary =>
                Restore(target: target, value: prior, key: op).Match(
                    Succ: _ => Fin.Fail<Unit>(error: primary),
                    Fail: cleanup => Fin.Fail<Unit>(error: primary + cleanup)))));
    }

    private static Fin<Unit> Apply(ViewInfo target, CameraDof value, Op key) =>
        toSeq(CameraDofField.Items)
            .TraverseM(field => Set(field: field, target: target, value: value, key: key))
            .As()
            .Map(static _ => unit);

    private static Fin<Unit> Restore(ViewInfo target, CameraDof value, Op key) =>
        toSeq(CameraDofField.Items)
            .Traverse(field => Set(field: field, target: target, value: value, key: key).ToValidation())
            .As()
            .ToFin()
            .Map(static _ => unit);

    private static Fin<Unit> Set(CameraDofField field, ViewInfo target, CameraDof value, Op key) =>
        key.Catch(() => {
            field.Set(target: target, value: value);
            return Fin.Succ(value: unit);
        });
}

public readonly record struct CPlaneGrid(
    Option<string> Name,
    Plane Plane,
    double GridSpacing,
    double SnapSpacing,
    int GridLineCount,
    int ThickLineFrequency,
    bool ShowGrid,
    bool ShowAxes,
    bool ShowZAxis,
    bool DepthBuffered,
    System.Drawing.Color ThinLineColor,
    System.Drawing.Color ThickLineColor,
    System.Drawing.Color GridXColor,
    System.Drawing.Color GridYColor,
    System.Drawing.Color GridZColor) {
    public static Fin<CPlaneGrid> Read(ViewportLease lease, Op? key = null) {
        Op op = key.OrDefault();
        return ViewportLease.Admit(lease: lease, key: op).Bind(owner => owner.Use(borrow: row => op.Catch(() => {
            DocObjects.ConstructionPlane cplane = row.Viewport.GetConstructionPlane();
            return Fin.Succ(new CPlaneGrid(
                Name: Optional(cplane.Name).Filter(static value => value.Length > 0),
                Plane: cplane.Plane,
                GridSpacing: cplane.GridSpacing,
                SnapSpacing: cplane.SnapSpacing,
                GridLineCount: cplane.GridLineCount,
                ThickLineFrequency: cplane.ThickLineFrequency,
                ShowGrid: cplane.ShowGrid,
                ShowAxes: cplane.ShowAxes,
                ShowZAxis: cplane.ShowZAxis,
                DepthBuffered: cplane.DepthBuffered,
                ThinLineColor: cplane.ThinLineColor,
                ThickLineColor: cplane.ThickLineColor,
                GridXColor: cplane.GridXColor,
                GridYColor: cplane.GridYColor,
                GridZColor: cplane.GridZColor));
        }), key: op));
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DetailLength {
    private DetailLength() { }
    public sealed record PaperCase(DetailMagnitude Value) : DetailLength;
    public sealed record ModelCase(DetailMagnitude Value) : DetailLength;

    public static Fin<DetailLength> Paper(double value, Op? key = null) =>
        key.OrDefault().AcceptValidated<DetailMagnitude>(candidate: value)
            .Map(static admitted => (DetailLength)new PaperCase(Value: admitted));

    public static Fin<DetailLength> Model(double value, Op? key = null) =>
        key.OrDefault().AcceptValidated<DetailMagnitude>(candidate: value)
            .Map(static admitted => (DetailLength)new ModelCase(Value: admitted));

    public Fin<DetailLength> Convert(ViewportLease lease, Op? key = null) {
        Op op = key.OrDefault();
        DetailLength self = this;
        return ViewportLease.Admit(lease: lease, key: op).Bind(owner => owner.Use(
            borrow: row => row.Detail.ToFin(Fail: op.InvalidInput()).Bind(detail => self.Switch(
                (Detail: detail, Op: op),
                paperCase: static (ctx, length) => ctx.Detail.TryGetModelLength(paperLength: (double)length.Value, modelLength: out double value)
                    ? Model(value: value, key: ctx.Op)
                    : Fin.Fail<DetailLength>(ctx.Op.InvalidResult()),
                modelCase: static (ctx, length) => ctx.Detail.TryGetPaperLength(modelLength: (double)length.Value, paperLength: out double value)
                    ? Paper(value: value, key: ctx.Op)
                    : Fin.Fail<DetailLength>(ctx.Op.InvalidResult()))),
            key: op));
    }
}

[ValueObject<double>]
public readonly partial struct DetailMagnitude {
    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) {
        validationError = double.IsFinite(value) && value >= 0.0
            ? validationError
            : new ValidationError(message: "detail length is invalid");
    }
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class ViewTransforms {
    extension(ViewportLease lease) {
        public Fin<Transform> Mapping(ViewMapping mapping, Op? key = null) {
            Op op = key.OrDefault();
            return ViewportLease.Admit(lease: lease, key: op).Bind(owner => owner.Use(borrow: row => row.Info(project: info =>
                info.GetXform(sourceSystem: mapping.Source, destinationSystem: mapping.Destination) is { IsValid: true } transform
                    ? Fin.Succ(transform)
                    : Fin.Fail<Transform>(op.InvalidResult()), key: op), key: op));
        }

        public Fin<double> PixelScale(Point3d at, Op? key = null) {
            Op op = key.OrDefault();
            return ViewportLease.Admit(lease: lease, key: op).Bind(owner => owner.Use(borrow: row =>
                row.Viewport.GetWorldToScreenScale(pointInFrustum: at, pixelsPerUnit: out double ppu) && double.IsFinite(ppu) && ppu > 0.0
                    ? Fin.Succ(ppu)
                    : Fin.Fail<double>(op.InvalidResult()),
                key: op));
        }

        public Fin<Line> FrustumLineAt(double screenX, double screenY, Op? key = null) {
            Op op = key.OrDefault();
            return ViewportLease.Admit(lease: lease, key: op).Bind(owner => owner.Use(borrow: row =>
                row.Viewport.GetFrustumLine(screenX: screenX, screenY: screenY, worldLine: out Line line)
                    ? Fin.Succ(line)
                    : Fin.Fail<Line>(op.InvalidResult()),
                key: op));
        }
    }
}
```

## [05]-[SNAPSHOT]

- Owner: `CameraSnapshot` is the `ViewportInfo` value adapter: pose, frustum, both frame-plane quads, and the identity pair (`DocKey`, `ChangeCounter`) that makes staleness a fact. Every disposable `ViewportInfo` is minted, projected, and released inside one borrow.
- Entry: `CameraSnapshot.Take(ViewportLease, Op?)` captures pose, frustum, quad, and serial under ONE borrow, so the stamped `ChangeCounter` names exactly the state the values project — a per-value borrow can stamp a serial that postdates the pose it certifies; `Restore(ViewportLease, Op?)` replays the stored pose through `CameraPose.Write` after proving the document identity; `Stale(ViewportLease)` compares BOTH identity axes, because a reopened document can alias a stored counter while the `DocKey` cannot.
- Law: frame-plane corners read through `ViewportInfo.GetFramePlaneCorners(depth:)` in host order `(BottomLeft, BottomRight, TopLeft, TopRight)` and travel as a typed quad, so downstream capture and draw code consumes named corners instead of index arithmetic.
- Law: snapshot values feed three consumers with one shape — the operations rail's view stack, the capture specification's window mapping, and the motion compiler's keyframe sampling — so a per-consumer snapshot variant is the collapsed form.
- Boundary: `Restore` is a host mutation and enters the operations rail through `CameraPose.Write`; the snapshot owner never seats a native viewport directly.

```csharp
// --- [MODELS] -------------------------------------------------------------------------------
public readonly record struct FramePlaneQuad(Point3d BottomLeft, Point3d BottomRight, Point3d TopLeft, Point3d TopRight);

public sealed record CameraSnapshot(
    CameraPose Pose,
    CameraFrustum Frustum,
    FramePlaneQuad NearQuad,
    FramePlaneQuad FarQuad,
    DocKey Document,
    uint ChangeSerial) {

    public static Fin<CameraSnapshot> Take(ViewportLease lease, Op? key = null) {
        Op op = key.OrDefault();
        return ViewportLease.Admit(lease: lease, key: op).Bind(owner => owner.Use(
            borrow: (document, row) =>
                from context in Rasm.Domain.Context.Of(doc: document).ToFin()
                from pose in CameraPose.ReadRow(row: row, context: context, key: op)
                from frustum in CameraFrustum.ReadRow(row: row, key: op)
                from quads in row.Info(project: info =>
                    from near in Quad(info.GetFramePlaneCorners(depth: frustum.Near), op)
                    from far in Quad(info.GetFramePlaneCorners(depth: frustum.Far), op)
                    select (Near: near, Far: far), key: op)
                select new CameraSnapshot(Pose: pose, Frustum: frustum, NearQuad: quads.Near, FarQuad: quads.Far, Document: owner.Key, ChangeSerial: row.Viewport.ChangeCounter),
            key: op));
    }

    public Fin<Unit> Restore(ViewportLease lease, Op? key = null) {
        Op op = key.OrDefault();
        return ViewportLease.Admit(lease: lease, key: op)
            .Bind(owner => guard(owner.Key == Document, op.InvalidInput()).ToFin()
                .Bind(_ => Pose.Write(lease: owner, key: op)))
            .Map(static _ => unit);
    }

    public Fin<bool> Stale(ViewportLease lease, Op? key = null) {
        Op op = key.OrDefault();
        CameraSnapshot self = this;
        return ViewportLease.Admit(lease: lease, key: op).Bind(owner => owner.Use(
            borrow: row => Fin.Succ(owner.Key != self.Document || row.Viewport.ChangeCounter != self.ChangeSerial),
            key: op));
    }

    private static Fin<FramePlaneQuad> Quad(Point3d[]? corners, Op key) => corners is { Length: 4 }
        ? Fin.Succ(new FramePlaneQuad(BottomLeft: corners[0], BottomRight: corners[1], TopLeft: corners[2], TopRight: corners[3]))
        : Fin.Fail<FramePlaneQuad>(key.InvalidResult());
}
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
