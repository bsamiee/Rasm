# [RASM_RHINO_CAMERA]

Camera ownership (`Rasm.Rhino.Viewport`) separates kernel pose and intent, session-scoped native borrows over the Document-owned `ViewportTarget` address, and value-only host projections. `CameraPose` composes `Rasm.Numerics.VectorFrame`; `ViewportLease` retains only `DocumentSession` plus `ViewportTarget`; `CameraSnapshot` disposes `ViewportInfo` before egress. Frustum, depth, visibility, depth-of-field, construction plane, detail scale, and coordinate transforms remain typed rows on one `Fin` carrier, and every read enters through ONE lease query member rather than a per-row admission preamble.

## [01]-[INDEX]

- [02]-[SCOPE_LEASE]: `ViewportBorrowMode`, `ViewportCardinality`, and `ViewportLease` — the session-gated borrow with its one query member, redraw bracket, and redriven restore.
- [03]-[POSE_MODEL]: `CameraPose` over the kernel `VectorFrame`, `LensAngle`, the `ProjectionKind` rows, and `CameraSeat` — the host-handle classification and seat owner behind the pose read/write pair.
- [04]-[HOST_ROWS]: `SpatialProbe`, `CameraFrustum`, `CameraDof`, `CPlaneState` over the Persistence-seated grid parts, `DetailLength`, and `ViewMapping`.
- [05]-[SNAPSHOT]: `CameraSnapshot` — the `ViewportInfo` value adapter with typed staleness evidence and the restore member.

## [02]-[SCOPE_LEASE]

- Owner: `ViewportLease` is the sole session-gated borrow surface, retaining only the `DocumentSession` plus the Document-owned `ViewportTarget` address; `ViewportBorrowMode` `[SmartEnum<int>]` gates broadcast redraw suppression; `ViewportCardinality` admits the resolved row count. Every borrow resolves and consumes the Document `ViewportRef` rows inside one session demand marshalled through the kernel dispatch, so no `RhinoView`, `RhinoViewport`, or `DetailViewObject` survives its borrow.
- Entry: `ViewportTarget.Active` / `Named` / `Id` / `Page` / `Detail` / `Every` mint the durable address on the Document owner; `ViewportLease.Of(DocumentSession, ViewportTarget, Op?)` admits it; `Read<T>(project, key?)` is the ONE scalar query member — self-admitting, observe-moded, resolving exactly one row — that every host-row projection on this page composes, so the eleven per-row `Admit`-then-`Use` preambles the prior page spelled collapse to one member; `Use<T>` is one name with two arities, the scalar observe borrow and the set borrow carrying its mode and terminal — the cardinality is value-borne inside the one `Borrow` body and the return SHAPE is the discriminant, exactly the kernel marshal's own arity law. NAMED LOSS: the `UseAll` name; witness — `Viewport/operations.md`'s broadcast apply composes `Use(borrow, mode, terminal)` and compiles unchanged but for the name.
- Law: a detail edit is committed through `CommitViewportChanges` on the operations pipeline, not observed — the lease only proves which rows are details, reading the `DetailViewObject` the Document `ViewportRef` carries.
- Law: durable identity is `DocKey` plus `ViewportTarget`; mutation identity is sampled from `RhinoViewport.ChangeCounter` by the operation that projects the value. A lease never stamps a native instance and therefore cannot become a stale handle cache.
- Law: every borrow crosses the kernel dispatch on the interactive lane and proves `SessionNeed.Redraw` inside the same window — `UiThread.Run(new UiDispatch<T>.Blocking(...), DispatchLane.Interactive)` wraps the session demand, so the crossing is gauged against the interactive frame budget and the demand serializes the host call; the retired command-thread marshal (`HostThread.Run` over `HostWork<T>.Session`) survives only at the shell, whose axis is the command queue and not this borrow.
- Law: broadcast redraw suppression is a BRACKET — the acquisition captures the prior redraw state and disables, the use traverses the rows, and the restore runs from the bracket's own final arm — and the restore RE-DRIVES once through the kernel redrive owner (`Redrive.Run(RedrivePolicy.Of(Schedule.recurs(1), 1), restore)`), its residual fault APPENDING to the primary through the one aggregation fold. A hand-spelled retry literal and a `.Match` ladder re-spelling cleanup beside the fold are the deleted forms.
- Boundary: the lease owns no host resource and is not `IDisposable`; each use re-resolves the address, executes, and discards every native reference before the marshalled closure returns.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Rasm.Domain;
using Rasm.Interaction;
using Rasm.Numerics;
using Rasm.Rhino.Document;
using Rasm.Rhino.Persistence;
using System.Runtime.InteropServices;

namespace Rasm.Rhino.Viewport;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<int>]
internal sealed partial class ViewportBorrowMode {
    internal const int BroadcastFloor = 3;

    internal static readonly ViewportBorrowMode Observe = new(key: 0, suppress: static _ => false);
    internal static readonly ViewportBorrowMode Mutate = new(key: 1, suppress: static count => count >= BroadcastFloor);

    [UseDelegateFromConstructor]
    internal partial bool Suppress(int count);
}

[SmartEnum<int>]
internal sealed partial class ViewportCardinality {
    internal static readonly ViewportCardinality Scalar = new(
        key: 0,
        admit: static (count, op) => guard(count == 1, new KernelFault.InvalidInput()).ToFin());
    internal static readonly ViewportCardinality Set = new(
        key: 1,
        admit: static (count, op) => guard(count > 0, new KernelFault.MissingContext()).ToFin());

    [UseDelegateFromConstructor]
    internal partial Fin<Unit> Admit(int count);
}

// --- [SERVICES] ------------------------------------------------------------------------
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

    public Fin<Context> Context() => session.Context();

    public static Fin<ViewportLease> Of(DocumentSession session, ViewportTarget target) {
        return from owner in Optional(session).ToFin(Fail: new KernelFault.MissingContext())
               from request in Admit.Need(value: target)
               select new ViewportLease(session: owner, target: request);
    }

    internal static Fin<ViewportLease> Admit(ViewportLease? lease) =>
        Admit.Need(value: lease);

    public Fin<TOut> Read<TOut>(Func<ViewportRef, Fin<TOut>> project) {
        return Use(borrow: (_, row) => project(row));
    }

    internal Fin<TOut> Use<TOut>(Func<RhinoDoc, ViewportRef, Fin<TOut>> borrow) =>
        Borrow(
            borrow: borrow,
            terminal: static (_, _) => Fin.Succ(unit),
            mode: ViewportBorrowMode.Observe,
            cardinality: ViewportCardinality.Scalar)
            .Bind(outputs => outputs.Head.ToFin(Fail: new KernelFault.MissingContext()));

    internal Fin<Seq<TOut>> Use<TOut>(
        Func<RhinoDoc, ViewportRef, Fin<TOut>> borrow,
        Func<RhinoDoc, int, Fin<Unit>> terminal,
        ViewportBorrowMode mode) => Borrow(
            borrow: borrow,
            terminal: terminal,
            mode: mode,
            cardinality: ViewportCardinality.Set);

    private Fin<Seq<TOut>> Borrow<TOut>(
        Func<RhinoDoc, ViewportRef, Fin<TOut>> borrow,
        Func<RhinoDoc, int, Fin<Unit>> terminal,
        ViewportBorrowMode mode,
        ViewportCardinality cardinality) =>
        UiThread.Run(
            new UiDispatch<Seq<TOut>>.Blocking(() => session.Demand(
                use: document =>
                    from rows in target.Resolve(document: document, key: key)
                    from _ in cardinality.Admit(count: rows.Count, op: key)
                    from outputs in mode.Suppress(count: rows.Count)
                        ? Suppressed(document: document, rows: rows, borrow: borrow)
                        : rows.TraverseM(row => Capture(document: document, row: row, borrow: borrow)).As()
                    from __ in terminal(document, rows.Count)
                    select outputs.Strict(),
                needs: [SessionNeed.Redraw])),
            DispatchLane.Interactive);

    private static Fin<Seq<TOut>> Suppressed<TOut>(
        RhinoDoc document,
        Seq<ViewportRef> rows,
        Func<RhinoDoc, ViewportRef, Fin<TOut>> borrow) =>
        IO.lift(() => Try.lift(() => {
                bool prior = document.Views.RedrawEnabled;
                document.Views.EnableRedraw(enable: false, redrawDocument: false, redrawLayers: false);
                return Fin.Succ(value: prior);
            }).Run().Bind(static inner => inner))
            .Bracket(
                Use: prior => IO.lift(() =>
                    rows.TraverseM(row => Capture(document: document, row: row, borrow: borrow)).As()),
                Fin: prior => Redrive.Run(
                    policy: RedrivePolicy.Of(law: Schedule.recurs(1), bound: 1),
                    work: IO.lift(() => Try.lift(() => {
                        document.Views.EnableRedraw(enable: prior, redrawDocument: false, redrawLayers: false);
                        return Fin.Succ(value: unit);
                    }).Run().Bind(static inner => inner))))
            .Run().As();

    private static Fin<TOut> Capture<TOut>(
        RhinoDoc document,
        ViewportRef row,
        Func<RhinoDoc, ViewportRef, Fin<TOut>> borrow) => Try.lift(() => borrow(document, row)).Run().Bind(static inner => inner);
}
```

## [03]-[POSE_MODEL]

- Owner: `CameraPose` composes `VectorFrame`, target, `LensAngle`, and the observable `ProjectionKind` rows `Parallel`, `Perspective`, and `TwoPoint`; `CameraSeat` owns every member that takes a host handle — projection classification, the projection gate, and the seat triplet. RhinoCommon exposes no reflected read predicate, so reflected projection remains an explicit `ProjectionChange.ReflectedCase` command and never masquerades as readable pose state.
- Entry: `CameraPose.Read(ViewportLease, Op?)` projects the live camera through the lease query; `CameraPose.Of(...)` admits a synthetic pose; `Write(ViewportLease, Op?)` enters `Cameras.Apply`, which proves the requested `ProjectionKind` through `CameraSeat.Accepts` and composes the one `CameraSeat.Seat` triplet.
- Law: `CameraSeat.Accepts` answers the RESULT and its refusal CARRIES the live classification — `Fin<Unit>` whose failure detail names `Classify(viewport)` — because a bare `false` discards the one discriminant the caller needs to correct the request (`DISCARDED_DISCRIMINANT`); `Viewport/operations.md:738` is the consumer that reads it.
- Law: `CameraPose` and `ProjectionKind` cross to `Rasm.Rhino.Modeling` as VALUE-ONLY shapes, so neither carries a member taking a `RhinoViewport` — `CameraSeat` is where every such member lives.
- Law: the frame is read through `RhinoViewport.GetCameraFrame(frame: out Plane)` and admitted through `VectorFrame.Of` — a second local frame construction beside the kernel owner is the killed census defect; an up-vector fallback resolves through `ViewportInfo.CalculateCameraUpDirection(location:, direction:, angle:)`, never a hand-rolled orthogonalization.
- Law: the pose write orders direction before angle and refuses `updateTargetLocation` on the direction write so the admitted target survives the seat; a mismatched projection is a typed refusal rather than a pose that silently omits one declared field, and the write returns the post-write `ChangeCounter`.
- Law: `LensAngle` carries the FULL vertical view angle in radians and BOTH host carriers — `RhinoViewport.CameraAngle` and `ViewportInfo.CameraAngle` — hold its HALF, live-proven at 13.4957 deg = atan(12/50) for a 50mm lens and identical on the two carriers. The read doubles, the seat halves, and a 1:1 crossing at either arm halves or doubles the field of view silently; `Modeling/projection.md` writes the same half onto its own frame.
- Law: architectural view conventions are NOT pose recipes here — `Rasm.Drawing` `ViewConvention.Pose` computes the convention pose from a subject bounds through the kernel catalog rows, and this owner only admits and seats the projected `ViewPose`.
- Boundary: reading and writing cross the same lease; a pose is a value, so two reads of a mutated viewport differ by construction and no cached pose masquerades as live state.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[ValueObject<double>]
[ValidationError]
public readonly partial struct LensAngle {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) {
        validationError = double.IsFinite(value) && value > 0.0 && value < Math.PI
            ? validationError
            : new ValidationError(string.Join(" | ", new object?[] { nameof(LensAngle), value, "a finite radian value in (0, PI)" }));
    }
}

[SmartEnum<int>]
public sealed partial class ProjectionKind {
    public static readonly ProjectionKind Parallel = new(key: 0);
    public static readonly ProjectionKind Perspective = new(key: 1);
    public static readonly ProjectionKind TwoPoint = new(key: 2);
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct CameraPose(VectorFrame Frame, Point3d Target, LensAngle Angle, ProjectionKind Projection) {
    public static Fin<CameraPose> Of(VectorFrame frame, Point3d target, LensAngle angle, ProjectionKind projection) =>
        Admit(pose: new CameraPose(Frame: frame, Target: target, Angle: angle, Projection: projection), key: key.OrDefault());

    internal static Fin<CameraPose> Admit(CameraPose pose) =>
        from _frame in guard(pose.Frame.Value.IsValid, new KernelFault.InvalidInput()).ToFin()
        from target in Acceptance.Value(value: pose.Target)
        from angle in FactoryBridge.Accept<LensAngle>(candidate: (double)pose.Angle)
        from projection in Admit.Need(value: pose.Projection)
        select pose with { Target = target, Angle = angle, Projection = projection };

    public static Fin<CameraPose> Read(ViewportLease lease) {
        return ViewportLease.Admit(lease: lease).Bind(owner => owner.Use(
            borrow: (document, row) => Rasm.Domain.Context.Of(doc: document).ToFin()
                .Bind(context => ReadRow(row: row, context: context))));
    }

    internal static Fin<CameraPose> ReadRow(ViewportRef row, Context context) => Try.lift(() =>
        row.Viewport.GetCameraFrame(frame: out Plane plane)
            ? (from admitted in VectorFrame.Of(origin: plane.Origin, normal: -plane.ZAxis, xHint: Some(plane.XAxis), context: context)
               from target in Acceptance.Value(value: row.Viewport.CameraTarget)
               from angle in FactoryBridge.Accept<LensAngle>(candidate: 2.0 * row.Viewport.CameraAngle)
               select new CameraPose(Frame: admitted, Target: target, Angle: angle, Projection: CameraSeat.Classify(viewport: row.Viewport)))
            : Fin.Fail<CameraPose>(new KernelFault.InvalidResult())).Run().Bind(static inner => inner);

    public Fin<uint> Write(ViewportLease lease) {
        return from owner in ViewportLease.Admit(lease: lease)
               from operation in CameraOp.Pose(pose: this)
               from outcome in Cameras.Apply(
                   session: owner.Session,
                   target: owner.Target,
                   operation: operation)
               from serial in outcome.Serials.Last.ToFin(Fail: new KernelFault.InvalidResult())
               select serial.After;
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class CameraSeat {
    internal static ProjectionKind Classify(RhinoViewport viewport) =>
        (viewport.IsPerspectiveProjection, viewport.IsTwoPointPerspectiveProjection, viewport.IsParallelProjection) switch {
            (_, true, _) => ProjectionKind.TwoPoint,
            (true, false, _) => ProjectionKind.Perspective,
            _ => ProjectionKind.Parallel,
        };

    internal static Fin<Unit> Accepts(ProjectionKind projection, RhinoViewport viewport) =>
        Classify(viewport: viewport) is var live && live == projection
            ? Fin.Succ(value: unit)
            : Fin.Fail<Unit>(new KernelFault.InvalidInput(Axis: Some(live.Key.ToString())));

    internal static Unit Seat(RhinoViewport viewport, CameraPose pose) {
        _ = Seat(viewport: viewport, target: pose.Target, location: pose.Frame.Value.Origin, direction: pose.Frame.Value.ZAxis);
        viewport.CameraAngle = (double)pose.Angle / 2.0;
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

- Owner: `SpatialProbe` — ONE probe union with two verbs, `Depth` and `Visible`, replacing the former probe twins whose case rosters overlapped on every geometric shape; `CameraFrustum` owns six planes, aspect, and bounds; `CameraDof` owns focal-blur state and `CameraDofField` the ordered setter vocabulary; `CPlaneState` owns the LIVE construction-plane read COMPOSED from the Persistence-seated value parts — `CPlaneGrid` (spacing, snap, count, frequency, and the `CapabilitySet<CPlaneTrait>` visibility axes) and `CPlanePalette` (the five inks) seat at `Persistence/presets.md`, the persisted owner at the lowest stratum both reach (E-R33), and this page adds only the name and the live read; `DetailLength` owns paper/model conversion; `ViewMapping` owns the coordinate-transform rows.
- Entry: every row's read is ONE composition of the lease query member — `lease.Read(row => ...)` — so the eleven admission preambles the prior page spelled per row are gone and a new host row costs its projection alone.
- Law: `SpatialProbe` is one shape vocabulary because the two verbs read the SAME four geometric cases — a point, a box, a sphere, and a geometry's bounds — and the verb, not the shape, was the twins' only discriminant; each verb lowers the cases onto the host members that answer it, deriving a bounds where the host verb lacks the direct arity, so the shape roster grows once for both questions.
- Law: every row is an ADMITTED value projection — `DepthExtent`, `CameraFrustum`, `CameraDof`, and the composed `CPlaneState` reach the caller on `Fin` through generated `Validate` stamped `[ValidationError]`, so a refusal carries the typed viewport fault and no `Try.lift(Create).Run().Bind(static inner => inner)` exception funnel survives where an admission belongs; finiteness folds ride `ValidityClaim.All`/`Finite` over spans, never hand `new[]{...}.All` ladders.
- Law: `CPlaneState` names the LIVE viewport read and Persistence's `CPlaneGrid`/`CPlanePalette` name the STORED parts — one value vocabulary, two custody points, and the five host screen colours cross into the kernel contract through `PerceptualColor.OfHost` at this one read (S12), accumulating so one refusal names every rejected channel.
- Law: `ViewMapping` is the ONE world/screen/clip/camera correspondence — one admitted `(Source, Destination)` pair generates the complete directional space, and a consumer needing pixels-per-unit reads `GetWorldToScreenScale` through `PixelScale`, never a re-derived projection ratio; the transform reads through a `ViewportInfo.GetXform` snapshot because that member returns `Transform.Unset` on failure where the live `RhinoViewport.GetTransform` returns `Identity` and makes refusal invisible to `IsValid`.
- Boundary: depth-of-field lives on `ViewInfo` (named-view state), not the live viewport — `CameraDof.Read`/`Write` take the `ViewInfo` the render and named-view pipelines hold, and the write is host mutation gated by the operations pipeline. `Write` captures all focal-blur fields before mutation, applies the ordered field rows fail-fast, and restores the complete prior state through one compensation path when any setter fails; the sample-count invariant is mode-conditional so an unconfigured view (`ViewInfoFocalBlurModes.None`, zero samples) reads back cleanly and that capture stays reachable on the first write.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SpatialProbe {
    private SpatialProbe() { }
    public sealed record AtPoint(Point3d Value) : SpatialProbe;
    public sealed record OfBounds(BoundingBox Value) : SpatialProbe;
    public sealed record OfSphere(Sphere Value) : SpatialProbe;
    public sealed record OfGeometry(GeometryBase Value) : SpatialProbe;

    public Fin<DepthExtent> Depth(ViewportLease lease) {
        SpatialProbe self = this;
        return ViewportLease.Admit(lease: lease).Bind(owner => owner.Read(project: row => self.Switch(
            row,
            atPoint: static (ctx, probe) => Extent(hit: ctx.GetDepth(point: probe.Value, distance: out double at), near: at, far: at),
            ofBounds: static (ctx, probe) => Extent(hit: ctx.GetDepth(bbox: probe.Value, nearDistance: out double near, farDistance: out double far), near: near, far: far),
            ofSphere: static (ctx, probe) => Extent(hit: ctx.GetDepth(sphere: probe.Value, nearDistance: out double near, farDistance: out double far), near: near, far: far),
            ofGeometry: static (ctx, probe) => Bounds(geometry: probe.Value, key: ctx.Op)
                .Bind(bounds => Extent(hit: ctx.GetDepth(bbox: bounds, nearDistance: out double near, farDistance: out double far), near: near, far: far)))));
    }

    public Fin<bool> Visible(ViewportLease lease) {
        SpatialProbe self = this;
        return ViewportLease.Admit(lease: lease).Bind(owner => owner.Read(project: row => self.Switch(
            row,
            atPoint: static (ctx, probe) => guard(probe.Value.IsValid, new KernelFault.InvalidInput()).ToFin().Map(_ => ctx.IsVisible(point: probe.Value)),
            ofBounds: static (ctx, probe) => guard(probe.Value.IsValid, new KernelFault.InvalidInput()).ToFin().Map(_ => ctx.IsVisible(bbox: probe.Value)),
            ofSphere: static (ctx, probe) => guard(probe.Value.IsValid, new KernelFault.InvalidInput()).ToFin().Map(_ => ctx.IsVisible(bbox: probe.Value.BoundingBox)),
            ofGeometry: static (ctx, probe) => Bounds(geometry: probe.Value, key: ctx.Op).Map(bounds => ctx.IsVisible(bbox: bounds)))));
    }

    private static Fin<BoundingBox> Bounds(GeometryBase geometry) =>
        from held in Admit.Need(value: geometry)
        from _ in guard(held.IsValid, new KernelFault.InvalidInput())
        from bounds in Try.lift(() => Fin.Succ(held.GetBoundingBox(accurate: false))).Run().Bind(static inner => inner)
        from __ in guard(bounds.IsValid, new KernelFault.InvalidInput())
        select bounds;

    private static Fin<DepthExtent> Extent(bool hit, double near, double far) =>
        hit
            ? FactoryBridge.Accept<DepthExtent>(fault: DepthExtent.Validate(near, far, out DepthExtent admitted), admitted: admitted)
            : Fin.Fail<DepthExtent>(new KernelFault.InvalidResult());
}

[ComplexValueObject]
[ValidationError]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct ViewMapping {
    public CoordinateSystem Source { get; }
    public CoordinateSystem Destination { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref CoordinateSystem source,
        ref CoordinateSystem destination) {
        validationError = Enum.IsDefined(value: source) && Enum.IsDefined(value: destination)
            ? validationError
            : new ValidationError(string.Join(" | ", new object?[] { nameof(ViewMapping), "two defined coordinate systems" }));
    }
}

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
[ValidationError]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct DepthExtent {
    public double Near { get; }
    public double Far { get; }

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double near, ref double far) {
        validationError = ValidityClaim.All(ValidityClaim.Finite([near, far]), near <= far)
            ? validationError
            : new ValidationError(string.Join(" | ", new object?[] { nameof(DepthExtent), far - near, "finite near at or before far" }));
    }
}

[ComplexValueObject]
[ValidationError]
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
        validationError = ValidityClaim.All(
            ValidityClaim.Finite([left, right, bottom, top, near, far, aspect]),
            left < right, bottom < top, near < far, aspect > 0.0, bounds.IsValid)
                ? validationError
                : new ValidationError(string.Join(" | ", new object?[] { nameof(CameraFrustum), "finite ordered planes, positive aspect, valid bounds" }));
    }

    public static Fin<CameraFrustum> Read(ViewportLease lease) {
        return ViewportLease.Admit(lease: lease)
            .Bind(owner => owner.Read(project: row => ReadRow(row: row, key: op)));
    }

    internal static Fin<CameraFrustum> ReadRow(ViewportRef row) =>
        row.Viewport.GetFrustum(left: out double left, right: out double right, bottom: out double bottom, top: out double top, nearDistance: out double near, farDistance: out double far)
            ? FactoryBridge.Accept<CameraFrustum>(
                fault: Validate(left, right, bottom, top, near, far, row.Viewport.FrustumAspect, row.Viewport.GetFrustumBoundingBox(), out CameraFrustum admitted),
                admitted: admitted)
            : Fin.Fail<CameraFrustum>(new KernelFault.InvalidResult());
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
[ValidationError]
public sealed partial class CameraDof {
    public ViewInfoFocalBlurModes Mode { get; }
    public double Distance { get; }
    public double Aperture { get; }
    public double Jitter { get; }
    public uint SampleCount { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref ViewInfoFocalBlurModes mode,
        ref double distance,
        ref double aperture,
        ref double jitter,
        ref uint sampleCount) {
        validationError = Enum.IsDefined(value: mode)
            && (mode is ViewInfoFocalBlurModes.None || sampleCount >= 1u)
            && ValidityClaim.All(ValidityClaim.Finite([distance, aperture, jitter]), distance >= 0.0, aperture >= 0.0, jitter >= 0.0)
                ? validationError
                : new ValidationError(string.Join(" | ", new object?[] { nameof(CameraDof), "a defined mode, nonnegative finite blur figures, and a positive sample count under any configured mode" }));
    }

    public static Fin<CameraDof> Of(
        ViewInfoFocalBlurModes mode,
        double distance,
        double aperture,
        double jitter,
        uint sampleCount) =>
        key.OrDefault().AcceptValidated<CameraDof>(
            fault: Validate(mode, distance, aperture, jitter, sampleCount, out CameraDof admitted),
            admitted: admitted);

    public static Fin<CameraDof> Read(ViewInfo view) {
        return Admit.Need(value: view).Bind(source => Try.lift(() => Of(
            mode: source.FocalBlurMode,
            distance: source.FocalBlurDistance,
            aperture: source.FocalBlurAperture,
            jitter: source.FocalBlurJitter,
            sampleCount: source.FocalBlurSampleCount)).Run().Bind(static inner => inner));
    }

    public Fin<Unit> Write(ViewInfo view) {
        CameraDof self = this;
        return Admit.Need(value: view).Bind(target => Read(view: target).Bind(prior =>
            Apply(target: target, value: self)
                .Rollback(release: () => Restore(target, prior))));
    }

    private static Fin<Unit> Apply(ViewInfo target, CameraDof value) =>
        toSeq(CameraDofField.Items)
            .TraverseM(field => Set(field: field, target: target, value: value))
            .As()
            .Map(static _ => unit);

    private static Fin<Unit> Restore(ViewInfo target, CameraDof value) =>
        toSeq(CameraDofField.Items)
            .Traverse(field => Set(field: field, target: target, value: value).ToValidation())
            .As()
            .ToFin()
            .Map(static _ => unit);

    private static Fin<Unit> Set(CameraDofField field, ViewInfo target, CameraDof value) =>
        Try.lift(() => {
            field.Set(target: target, value: value);
            return Fin.Succ(value: unit);
        }).Run().Bind(static inner => inner);
}

public sealed record CPlaneState(Option<string> Name, Plane Plane, CPlaneGrid Grid, CPlanePalette Palette) {
    public static Fin<CPlaneState> Read(ViewportLease lease) {
        return ViewportLease.Admit(lease: lease).Bind(owner => owner.Read(
            project: row => Try.lift(() => Fin.Succ(row.Viewport.GetConstructionPlane())).Run().Bind(static inner => inner)
                .Bind(cplane => Admitted(cplane: cplane, key: op))));
    }

    private static Fin<CPlaneState> Admitted(DocObjects.ConstructionPlane cplane) =>
        (CPlaneGrid.Read(source: cplane).ToValidation(),
         CPlanePalette.Read(source: cplane).ToValidation())
            .Apply(static (grid, palette) => (Grid: grid, Palette: palette))
            .As()
            .ToFin()
            .Bind(held => guard(cplane.Plane.IsValid, new KernelFault.InvalidInput()).ToFin().Map(_ => new CPlaneState(
                Name: HostEdge.Text(cplane.Name),
                Plane: cplane.Plane,
                Grid: held.Grid,
                Palette: held.Palette)));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DetailLength {
    private DetailLength() { }
    public sealed record PaperCase(DetailMagnitude Value) : DetailLength;
    public sealed record ModelCase(DetailMagnitude Value) : DetailLength;

    public static Fin<DetailLength> Paper(double value) =>
        key.OrDefault().AcceptValidated<DetailMagnitude>(candidate: value)
            .Map(static admitted => (DetailLength)new PaperCase(Value: admitted));

    public static Fin<DetailLength> Model(double value) =>
        key.OrDefault().AcceptValidated<DetailMagnitude>(candidate: value)
            .Map(static admitted => (DetailLength)new ModelCase(Value: admitted));

    public Fin<DetailLength> Convert(ViewportLease lease) {
        DetailLength self = this;
        return ViewportLease.Admit(lease: lease).Bind(owner => owner.Read(
            project: row => row.Detail.ToFin(Fail: new KernelFault.InvalidInput()).Bind(detail => self.Switch(
                detail,
                paperCase: static (ctx, length) => ctx.TryGetModelLength(paperLength: (double)length.Value, modelLength: out double value)
                    ? Model(value: value, key: ctx.Op)
                    : Fin.Fail<DetailLength>(new KernelFault.InvalidResult()),
                modelCase: static (ctx, length) => ctx.TryGetPaperLength(modelLength: (double)length.Value, paperLength: out double value)
                    ? Paper(value: value, key: ctx.Op)
                    : Fin.Fail<DetailLength>(new KernelFault.InvalidResult())))));
    }
}

[ValueObject<double>]
[ValidationError]
public readonly partial struct DetailMagnitude {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) {
        validationError = double.IsFinite(value) && value >= 0.0
            ? validationError
            : new ValidationError(string.Join(" | ", new object?[] { nameof(DetailMagnitude), value, "a finite nonnegative length" }));
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ViewTransforms {
    extension(ViewportLease lease) {
        public Fin<Transform> Mapping(ViewMapping mapping) {
            return ViewportLease.Admit(lease: lease).Bind(owner => owner.Read(project: row => row.Info(project: info =>
                info.GetXform(sourceSystem: mapping.Source, destinationSystem: mapping.Destination) is { IsValid: true } transform
                    ? Fin.Succ(transform)
                    : Fin.Fail<Transform>(new KernelFault.InvalidResult()), key: op)));
        }

        public Fin<double> PixelScale(Point3d at) {
            return ViewportLease.Admit(lease: lease).Bind(owner => owner.Read(project: row =>
                row.Viewport.GetWorldToScreenScale(pointInFrustum: at, pixelsPerUnit: out double ppu) && double.IsFinite(ppu) && ppu > 0.0
                    ? Fin.Succ(ppu)
                    : Fin.Fail<double>(new KernelFault.InvalidResult())));
        }

        public Fin<Line> FrustumLineAt(double screenX, double screenY) {
            return ViewportLease.Admit(lease: lease).Bind(owner => owner.Read(project: row =>
                row.Viewport.GetFrustumLine(screenX: screenX, screenY: screenY, worldLine: out Line line)
                    ? Fin.Succ(line)
                    : Fin.Fail<Line>(new KernelFault.InvalidResult())));
        }
    }
}
```

## [05]-[SNAPSHOT]

- Owner: `CameraSnapshot` is the `ViewportInfo` value adapter: pose, frustum, both frame-plane quads, and the identity pair (`DocKey`, `ChangeCounter`) that makes staleness a fact; `Staleness` is the typed verdict the compare answers.
- Entry: `CameraSnapshot.Take(ViewportLease, Op?)` captures pose, frustum, quad, and serial under ONE borrow through the lease query, so the stamped `ChangeCounter` names exactly the state the values project; `Restore(ViewportLease, Op?)` replays the stored pose through `CameraPose.Write` after proving the document identity; `Stale(ViewportLease)` answers `Fin<Staleness>`.
- Law: staleness is a UNION, never a bool — `Fresh`, `Reopened` (the `DocKey` moved: a reopened document can alias a stored counter), and `Mutated` (the counter moved under the same document) are three verdicts a caller recovers from differently, and the bool collapsed the two stale causes a restore must tell apart.
- Law: frame-plane corners read through `ViewportInfo.GetFramePlaneCorners(depth:)` in host order `(BottomLeft, BottomRight, TopLeft, TopRight)` and travel as a typed quad, so downstream capture and draw code consumes named corners instead of index arithmetic.
- Law: snapshot values feed three consumers with one shape — the operations pipeline's view stack, the capture specification's window mapping, and the motion drive's keyframe seeding — so a per-consumer snapshot variant is the collapsed form.
- Boundary: `Restore` is a host mutation and enters the operations pipeline through `CameraPose.Write`; the snapshot owner never seats a native viewport directly.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Staleness {
    private Staleness() { }
    public sealed record Fresh : Staleness;
    public sealed record Reopened : Staleness;
    public sealed record Mutated : Staleness;
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct FramePlaneQuad(Point3d BottomLeft, Point3d BottomRight, Point3d TopLeft, Point3d TopRight);

public sealed record CameraSnapshot(
    CameraPose Pose,
    CameraFrustum Frustum,
    FramePlaneQuad NearQuad,
    FramePlaneQuad FarQuad,
    DocKey Document,
    uint ChangeSerial) {

    public static Fin<CameraSnapshot> Take(ViewportLease lease) {
        return ViewportLease.Admit(lease: lease).Bind(owner => owner.Use(
            borrow: (document, row) =>
                from context in Rasm.Domain.Context.Of(doc: document).ToFin()
                from pose in CameraPose.ReadRow(row: row, context: context)
                from frustum in CameraFrustum.ReadRow(row: row, key: op)
                from quads in row.Info(project: info =>
                    from near in Quad(info.GetFramePlaneCorners(depth: frustum.Near))
                    from far in Quad(info.GetFramePlaneCorners(depth: frustum.Far))
                    select (Near: near, Far: far), key: op)
                select new CameraSnapshot(Pose: pose, Frustum: frustum, NearQuad: quads.Near, FarQuad: quads.Far, Document: owner.Key, ChangeSerial: row.Viewport.ChangeCounter)));
    }

    public Fin<Unit> Restore(ViewportLease lease) {
        return ViewportLease.Admit(lease: lease)
            .Bind(owner => guard(owner.Key == Document, new KernelFault.InvalidInput()).ToFin()
                .Bind(_ => Pose.Write(lease: owner)))
            .Map(static _ => unit);
    }

    public Fin<Staleness> Stale(ViewportLease lease) {
        CameraSnapshot self = this;
        return ViewportLease.Admit(lease: lease).Bind(owner => owner.Read(
            project: row => Fin.Succ<Staleness>(
                owner.Key != self.Document ? new Staleness.Reopened()
                : row.Viewport.ChangeCounter != self.ChangeSerial ? new Staleness.Mutated()
                : new Staleness.Fresh())));
    }

    private static Fin<FramePlaneQuad> Quad(Point3d[]? corners) => corners is { Length: 4 }
        ? Fin.Succ(new FramePlaneQuad(BottomLeft: corners[0], BottomRight: corners[1], TopLeft: corners[2], TopRight: corners[3]))
        : Fin.Fail<FramePlaneQuad>(new KernelFault.InvalidResult());
}
```

- Packages: `RhinoCommon` (`Rasm.Rhino/.api/api-rhinocommon-display.md` — `RhinoViewport` camera members, viewport queries); `Thinktecture.Runtime.Extensions` (`libs/dotnet/.api/api-thinktecture-runtime-extensions.md` — `[SmartEnum]` cardinality/projection rows, `[ComplexValueObject]`/`[ValueObject]` frames); kernel `Domain/results` (`ViewportLease.Read` scalar boundary, `IO.Bracket` suppression) + `Persistence/presets` (`CPlaneGrid`/`CPlanePalette` composed per the folder CPlane-seat ruling).

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
