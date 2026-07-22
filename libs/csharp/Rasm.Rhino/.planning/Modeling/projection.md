# [RASM_RHINO_MODELING_PROJECTION]

`Rasm.Rhino.Modeling` owns hidden-line drafting capture. One `ProjectionOp` union carries the Make2D hidden-line drawing, per-object silhouette capture, and draft-curve extraction through `Projections.Build`; `ProjectionFrame` admits the sampled camera, synthetic pose, plan, and directional frames as values, `DrawingLaw` closes the parameter surface, and `Context` supplies every tolerance and angle. Occlusion classification is the host engine's; the exact analytic drawing (`View.Apply` onto `DrawingProjection`), section cuts (`IntersectOp.PlaneMesh`), region fill (`ArrangementOp.PlanarOverlay` behind `DrawingProjection.Fill`), and exact orientation (`Predicate.Orient2D`) remain kernel-owned, so this page is the HOST capture altitude under the capture law and never re-derives a visibility algorithm.

## [01]-[INDEX]

- [02]-[PROJECTION_FRAME]: `ProjectionFrame` — the value-only camera family and its `ViewportInfo` rig.
- [03]-[DRAWING_POLICY]: `ProjectionSubject`, `DrawingLaw`, `ProjectionPacing`, `SilhouetteFrame` — subjects, flags, pacing, and the silhouette eye.
- [04]-[OPERATION_RAIL]: `ProjectionSlot`, `ProjectionOp`, and the `Projections.Build` entry.
- [05]-[SURFACE_LEDGER]: the page's owner table.

## [02]-[PROJECTION_FRAME]

- Owner: `ProjectionFrame` `[Union]` closes the four frame sources — a sampled `CameraSnapshot`, a synthetic `CameraPose` over subject bounds, a drafting-plane plan, and an explicit parallel direction; `Rig` is the one site minting the disposable `ViewportInfo` every host compute consumes.
- Law: the frame is a sampled value, never a live viewport — a `RhinoViewport` or `ViewportLease` never crosses this rail; live sampling is the Viewport rail's `CameraSnapshot.Take`, and the `Snapshot`/`Pose` cases consume that rail's value shapes as the ruled Modeling counter-edge, so a headless drawing and a viewport-true drawing enter one union.
- Law: the seat is verified member by member — `SetCameraLocation`, `SetCameraDirection`, and `SetCameraUp` each answer `bool` and fold through one confirmation, the projection transition selects `ChangeToParallelProjection`/`ChangeToPerspectiveProjection`/`ChangeToTwoPointPerspectiveProjection` off the pose's `ProjectionKind` row, and `SetFrustumNearFar(BoundingBox)` derives the clip range from the subject bounds, so no frustum scalar is hand-derived.
- Law: `Rig` transfers a detached frame only after every seat step succeeds; one ownership bracket disposes each refused or exceptional frame before the typed fault leaves.
- Law: `Directed` requires its up vector — a local orthogonalization fallback is the killed form the camera page names, so an explicit up crosses the case and the plan case reads the drafting plane's own `YAxis`.
- Law: the lens seat is the one optics inversion — `LensAngle` converts to the 35mm lens length through the named half-frame diagonal, and the perspective target distance reads the pose's admitted target, never a magic depth.
- Boundary: screen landing is not this owner — a consumer placing the flattened drawing on a sheet composes `ViewTransforms.Mapping` over `ViewMapping` on the Viewport rail; this frame ends at the projection compute.

```csharp
// --- [TYPES] ------------------------------------------------------------------------------
[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ProjectionFrame {
    private ProjectionFrame() { }
    public sealed record Snapshot(CameraSnapshot Value) : ProjectionFrame;
    public sealed record Pose(CameraPose Value, BoundingBox Subject) : ProjectionFrame;
    public sealed record Look(Plane Frame, BoundingBox Subject) : ProjectionFrame;
    public sealed record Directed(Vector3d Direction, Vector3d Up, BoundingBox Subject) : ProjectionFrame;

    private const double HalfFrameDiagonal = 21.635;

    internal Fin<ViewportInfo> Rig(Op key) =>
        Switch(
            context: key,
            snapshot: static (op, frame) => Seated(
                pose: frame.Value.Pose, subject: frame.Value.Frustum.Bounds, key: op),
            pose: static (op, frame) =>
                from _ in op.AcceptInput(value: frame.Subject)
                from rigged in Seated(pose: frame.Value, subject: frame.Subject, key: op)
                select rigged,
            look: static (op, frame) =>
                from plane in op.AcceptInput(value: frame.Frame)
                from _ in op.AcceptInput(value: frame.Subject)
                from rigged in Parallel(
                    eye: frame.Subject.Center + plane.ZAxis * frame.Subject.Diagonal.Length,
                    direction: -plane.ZAxis, up: plane.YAxis, subject: frame.Subject, key: op)
                select rigged,
            directed: static (op, frame) =>
                from direction in op.AcceptInput(value: frame.Direction)
                from up in op.AcceptInput(value: frame.Up)
                from _ in op.AcceptInput(value: frame.Subject)
                from rigged in Parallel(
                    eye: frame.Subject.Center - direction * frame.Subject.Diagonal.Length,
                    direction: direction, up: up, subject: frame.Subject, key: op)
                select rigged);

    private static Fin<ViewportInfo> Seated(CameraPose pose, BoundingBox subject, Op key) =>
        Seat(
            state: (Pose: pose, Subject: subject),
            configure: static (state, frame) => {
                Plane basis = state.Pose.Frame.Value;
                double reach = basis.Origin.DistanceTo(other: state.Pose.Target);
                return frame.SetCameraLocation(basis.Origin)
                    && frame.SetCameraDirection(basis.ZAxis)
                    && frame.SetCameraUp(basis.YAxis)
                    && state.Pose.Projection.Switch<(ViewportInfo Frame, double Reach, LensAngle Angle, Vector3d Up), bool>(
                        (frame, reach, state.Pose.Angle, basis.YAxis),
                        parallel: static seat => seat.Frame.ChangeToParallelProjection(symmetricFrustum: true),
                        perspective: static seat => seat.Frame.ChangeToPerspectiveProjection(
                            targetDistance: seat.Reach, symmetricFrustum: true, lensLength: Lens(angle: seat.Angle)),
                        twoPoint: static seat => seat.Frame.ChangeToTwoPointPerspectiveProjection(
                            targetDistance: seat.Reach, up: seat.Up, lensLength: Lens(angle: seat.Angle)))
                    && frame.SetFrustumNearFar(state.Subject);
            },
            key: key);

    private static Fin<ViewportInfo> Parallel(Point3d eye, Vector3d direction, Vector3d up, BoundingBox subject, Op key) =>
        Seat(
            state: (Eye: eye, Direction: direction, Up: up, Subject: subject),
            configure: static (state, frame) => frame.SetCameraLocation(state.Eye)
                && frame.SetCameraDirection(state.Direction)
                && frame.SetCameraUp(state.Up)
                && frame.ChangeToParallelProjection(symmetricFrustum: true)
                && frame.SetFrustumNearFar(state.Subject),
            key: key);

    private static Fin<ViewportInfo> Seat<TState>(
        TState state, Func<TState, ViewportInfo, bool> configure, Op key) =>
        key.Catch(() => {
            ViewportInfo? frame = null;
            try {
                frame = new ViewportInfo();
                if (!configure(state, frame)) {
                    return Fin.Fail<ViewportInfo>(error: key.InvalidResult());
                }
                ViewportInfo owned = frame;
                frame = null;
                return Fin.Succ(value: owned);
            } finally {
                frame?.Dispose();
            }
        });

    private static double Lens(LensAngle angle) => HalfFrameDiagonal / Math.Tan((double)angle / 2.0);
}
```

## [03]-[DRAWING_POLICY]

- Owner: `ProjectionSubject` carries one drawing source as a row — leased handle, optional admitted placement, per-subject clip planes, and the occluding-section flag; `DrawingLaw` closes every `HiddenLineDrawingParameters` flag as one value; `ProjectionPacing` is the cancellable-compute policy row; `SilhouetteFrame` closes the three silhouette eye modalities.
- Law: placement is admitted, never raw — a subject motion enters as `TransformSpec` and lowers through `Placement.Build`, so `TransformSpec.PlanarProjection` and `DirectionalProjection` are the projection-transform spellings and a directly constructed host `Transform` bypassing kernel admission is the deleted form.
- Law: the tag channel is the correspondence — every subject registers under its ordinal, so segment-to-source and point-to-source maps are host evidence read back from `HiddenLineDrawingObject.Tag`, never a geometry re-match.
- Law: flags map one-to-one — `Flatten`, `IncludeTangentEdges`, `IncludeTangentSeams`, `IncludeHiddenCurves`, and `OccludingSectionOption` bake in one member with `AbsoluteTolerance` reading the regime; a tolerance knob beside the law is the deleted form, and the public occluding flag is `OccludingSectionOption` — the same-named list member is host-internal.
- Law: pacing is one optional policy value — `ProjectionPacing` fuses cancellation, progress, and the thread grant, selecting the four-argument `Compute` overload; absent pacing runs the two-argument overload under the law's thread grant, so no signature grows a token tail.

```csharp
// --- [TYPES] ------------------------------------------------------------------------------
[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SilhouetteFrame {
    private SilhouetteFrame() { }
    public sealed record Eye(Point3d Value) : SilhouetteFrame;
    public sealed record Along(Vector3d Value) : SilhouetteFrame;
    public sealed record Framed(ProjectionFrame Value) : SilhouetteFrame;
}

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record ProjectionPacing(CancellationToken Cancel, IProgress<double> Progress, bool Threads = true);

public sealed record ProjectionSubject(
    GeometryHandle Geometry,
    Option<TransformSpec> Placement = default,
    Seq<Plane> Clips = default,
    bool Occluding = false);

public sealed record DrawingLaw(
    bool TangentEdges = true,
    bool TangentSeams = false,
    bool HiddenCurves = true,
    bool Flatten = true,
    bool OccludingSections = false,
    bool Rejoin = true,
    bool Threads = true,
    Seq<Plane> Clips = default,
    Option<ProjectionPacing> Pacing = default) {
    internal Fin<HiddenLineDrawingParameters> Rig(Context domain, Op key) {
        DrawingLaw law = this;
        return from clips in ProjectionOp.AdmittedClips(law.Clips, key)
               from parameters in key.Catch(() => {
                   HiddenLineDrawingParameters parameters = new() {
                       AbsoluteTolerance = domain.Absolute.Value,
                       Flatten = law.Flatten,
                       IncludeTangentEdges = law.TangentEdges,
                       IncludeTangentSeams = law.TangentSeams,
                       IncludeHiddenCurves = law.HiddenCurves,
                       OccludingSectionOption = law.OccludingSections,
                   };
                   foreach (Plane clip in clips) { parameters.AddClippingPlane(plane: clip); }
                   return Fin.Succ(value: parameters);
               })
               select parameters;
    }
}
```

## [04]-[OPERATION_RAIL]

- Owner: `ProjectionSlot` `[SmartEnum<int>]` — the consequence vocabulary over host visibility classes, correspondence streams, and capture products; `ProjectionOp` `[Union]` — the drawing, silhouette, and draft verbs; `Projections` — the one entry folding any operation spread into one `Built<ProjectionSlot>`.
- Law: products detach before the engine dies — every segment curve duplicates out of the disposable `HiddenLineDrawing` onto an owned handle inside the compute window, silhouette curves own directly because `Silhouette.Compute` returns fresh geometry, and the drawing, its `ViewportInfo`, and every mid-fold failure release symmetrically.
- Law: classification is total host evidence — the six-value segment visibility maps onto the six visibility slots with an out-of-roster host value landing on the `Unresolved` floor, `SilhouetteType` crosses as a per-product kind stream, per-side surface fills cross as paired code groups, and scene-silhouette membership rides `IsSceneSilhouette` ordinals, so a consumer partitions the drawing without re-testing geometry.
- Law: correspondence maps survive — the subject ordinal registered at `AddGeometry` returns through `ParentCurve.SourceObject.Tag`, curve and point component indices land as `ComponentRows`, drawing points carry source and visibility streams beside their marks, and `WorldToHiddenLine` plus the host-computed drawing bounds land as frame facts, so sheet placement composes evidence rather than re-projecting.
- Law: clipping planes cross one admission fold — drawing-global, subject-selective, and outline clip sets traverse `AcceptInput` applicatively, and each arm reuses the admitted sequence for every native overload it selects.
- Law: the capture altitudes stay split — `Drawing` is the occlusion-resolved scene, `Outline` is the per-object `Silhouette.Compute` capture whose kind mask and clip set cross as values, and `Draft` is `ComputeDraftCurve` under an explicit pull; a BSP, painter, per-sample occlusion march, or host round-trip re-derivation of any altitude is the kernel view page's enumerated dead form.
- Boundary: geometric curve projection stays the curve rail — `CurveOp.Project` over `ProjectTarget` owns plane, brep, and mesh target projection with index maps; this rail begins where visibility classification does.
- Growth: a new drawing modality is one `ProjectionOp` case with its arm; a new frame source is one `ProjectionFrame` case every verb reads.

```csharp
// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class ProjectionSlot {
    public static readonly ProjectionSlot Visible = new(key: 0);
    public static readonly ProjectionSlot Hidden = new(key: 1);
    public static readonly ProjectionSlot Duplicate = new(key: 2);
    public static readonly ProjectionSlot Projecting = new(key: 3);
    public static readonly ProjectionSlot Clipped = new(key: 4);
    public static readonly ProjectionSlot Unresolved = new(key: 5);
    public static readonly ProjectionSlot Sourced = new(key: 6);
    public static readonly ProjectionSlot Kinds = new(key: 7);
    public static readonly ProjectionSlot Fills = new(key: 8);
    public static readonly ProjectionSlot Marks = new(key: 9);
    public static readonly ProjectionSlot Frame = new(key: 10);
    public static readonly ProjectionSlot Outline = new(key: 11);
    public static readonly ProjectionSlot Draft = new(key: 12);
    public static readonly ProjectionSlot Bounds = new(key: 13);

    internal static ProjectionSlot Classify(HiddenLineDrawingSegment.Visibility visibility) =>
        visibility switch {
            HiddenLineDrawingSegment.Visibility.Visible => Visible,
            HiddenLineDrawingSegment.Visibility.Hidden => Hidden,
            HiddenLineDrawingSegment.Visibility.Duplicate => Duplicate,
            HiddenLineDrawingSegment.Visibility.Projecting => Projecting,
            HiddenLineDrawingSegment.Visibility.Clipped => Clipped,
            HiddenLineDrawingSegment.Visibility.Unset or _ => Unresolved,
        };
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ProjectionOp {
    private ProjectionOp() { }
    public sealed record Drawing(Seq<ProjectionSubject> Subjects, ProjectionFrame Frame, DrawingLaw Law) : ProjectionOp;
    public sealed record Outline(
        GeometryHandle Subject, SilhouetteType Kinds, SilhouetteFrame Frame,
        Seq<Plane> Clips = default, Option<CancellationToken> Cancel = default) : ProjectionOp;
    public sealed record Draft(
        GeometryHandle Subject, double Angle, Vector3d Pull,
        Option<CancellationToken> Cancel = default) : ProjectionOp;

    internal Fin<Built<ProjectionSlot>> Apply(Context domain) =>
        Switch(
            context: domain,
            drawing: static (model, edit) => {
                Op op = Op.Of(name: nameof(Drawing));
                return from _ in guard(!edit.Subjects.IsEmpty, op.InvalidInput())
                       from law in Optional(edit.Law).ToFin(Fail: op.InvalidInput())
                       from built in ModelGate.BorrowMany<GeometryBase, Built<ProjectionSlot>>(
                           handles: edit.Subjects.Map(static subject => subject.Geometry), key: op,
                           body: natives =>
                               from parameters in law.Rig(domain: model, key: op)
                               from rigged in edit.Frame.Rig(key: op)
                               from harvested in op.Catch(() => {
                                   using ViewportInfo frame = rigged;
                                   parameters.SetViewport(frame);
                                   return edit.Subjects
                                       .Map((subject, ordinal) => (Subject: subject, Ordinal: ordinal))
                                       .TraverseM(row => Registered(
                                           parameters: parameters, subject: row.Subject, native: natives[row.Ordinal],
                                           ordinal: row.Ordinal, model: model, op: op)).As()
                                       .Bind(_ => Computed(parameters: parameters, law: law, op: op));
                               })
                               select harvested)
                       select built;
            },
            outline: static (model, edit) => {
                Op op = Op.Of(name: nameof(Outline));
                return from clips in AdmittedClips(edit.Clips, op)
                       from built in ModelGate.Borrow<GeometryBase, Built<ProjectionSlot>>(handle: edit.Subject, key: op, body: native =>
                    edit.Frame.Switch(
                        context: (Native: native, Model: model, Edit: edit, Clips: clips, Op: op),
                        eye: static (ctx, frame) => Outlined(ctx,
                            plain: () => Silhouette.Compute(
                                ctx.Native, ctx.Edit.Kinds, frame.Value, ctx.Model.Absolute.Value, ctx.Model.Angle.Value),
                            clipped: () => Silhouette.Compute(
                                ctx.Native, ctx.Edit.Kinds, frame.Value, ctx.Model.Absolute.Value, ctx.Model.Angle.Value,
                                ctx.Clips.AsIterable(), ctx.Edit.Cancel.IfNone(CancellationToken.None))),
                        along: static (ctx, frame) => Outlined(ctx,
                            plain: () => Silhouette.Compute(
                                ctx.Native, ctx.Edit.Kinds, frame.Value, ctx.Model.Absolute.Value, ctx.Model.Angle.Value),
                            clipped: () => Silhouette.Compute(
                                ctx.Native, ctx.Edit.Kinds, frame.Value, ctx.Model.Absolute.Value, ctx.Model.Angle.Value,
                                ctx.Clips.AsIterable(), ctx.Edit.Cancel.IfNone(CancellationToken.None))),
                        framed: static (ctx, frame) =>
                            from rigged in frame.Value.Rig(key: ctx.Op)
                            from built in ctx.Op.Catch(() => {
                                using ViewportInfo viewport = rigged;
                                return Captured(slot: ProjectionSlot.Outline, op: ctx.Op, run: () =>
                                    Silhouette.Compute(
                                        ctx.Native, ctx.Edit.Kinds, viewport,
                                        ctx.Model.Absolute.Value, ctx.Model.Angle.Value,
                                        ctx.Clips.AsIterable(), ctx.Edit.Cancel.IfNone(CancellationToken.None)));
                            })
                            select built)
                       select built;
            },
            draft: static (model, edit) => {
                Op op = Op.Of(name: nameof(Draft));
                return ModelGate.Borrow<GeometryBase, Built<ProjectionSlot>>(handle: edit.Subject, key: op, body: native =>
                    from _ in guard(double.IsFinite(edit.Angle), op.InvalidInput())
                    from pull in op.AcceptInput(value: edit.Pull)
                    from built in Captured(slot: ProjectionSlot.Draft, op: op, run: () =>
                        edit.Cancel.Case switch {
                            CancellationToken cancel => Silhouette.ComputeDraftCurve(
                                native, edit.Angle, pull, model.Absolute.Value, model.Angle.Value, cancel),
                            _ => Silhouette.ComputeDraftCurve(
                                native, edit.Angle, pull, model.Absolute.Value, model.Angle.Value),
                        })
                    select built);
            });

    private static Fin<Unit> Registered(
        HiddenLineDrawingParameters parameters, ProjectionSubject subject, GeometryBase native,
        int ordinal, Context model, Op op) =>
        from motion in subject.Placement
            .Traverse(spec => Placement.Build(spec: spec, context: Some(model), key: op)).As()
        from clips in AdmittedClips(subject.Clips, op)
        from _ in op.Confirm(success: (motion.Case, clips.IsEmpty) switch {
            (Transform placed, true) => parameters.AddGeometry(native, placed, ordinal, subject.Occluding),
            (Transform placed, false) => parameters.AddGeometryAndPlanes(
                native, placed, ordinal, subject.Occluding, [.. clips]),
            (_, true) => parameters.AddGeometry(native, ordinal, subject.Occluding),
            (_, false) => parameters.AddGeometryAndPlanes(native, ordinal, subject.Occluding, [.. clips]),
        })
        select unit;

    private static Fin<Built<ProjectionSlot>> Computed(HiddenLineDrawingParameters parameters, DrawingLaw law, Op op) {
        HiddenLineDrawing? computed = law.Pacing.Case switch {
            ProjectionPacing pacing => HiddenLineDrawing.Compute(parameters, pacing.Threads, pacing.Progress, pacing.Cancel),
            _ => HiddenLineDrawing.Compute(parameters, law.Threads),
        };
        return Optional(computed).ToFin(Fail: op.InvalidResult()).Bind(drawing => {
            using (drawing) {
                if (law.Rejoin) { drawing.RejoinCompatibleVisible(); }
                Seq<HiddenLineDrawingSegment> segments = toSeq(drawing.Segments);
                Seq<HiddenLineDrawingPoint> points = toSeq(drawing.Points);
                BuildReceipt<ProjectionSlot> evidence = Harvested(
                    segments: segments,
                    points: points,
                    frame: drawing.WorldToHiddenLine,
                    bounds: drawing.BoundingBox(includeHidden: law.HiddenCurves));
                return ModelGate.OwnMany(
                        built: segments.Map(static segment => (GeometryBase)segment.CurveGeometry.Duplicate()),
                        key: op, allowEmpty: true)
                    .Map(owned => Built<ProjectionSlot>.Of(operation: op, Products: owned, Evidence: evidence));
            }
        });
    }

    private static BuildReceipt<ProjectionSlot> Harvested(
        Seq<HiddenLineDrawingSegment> segments,
        Seq<HiddenLineDrawingPoint> points,
        Transform frame,
        BoundingBox bounds) {
        Seq<(ProjectionSlot Slot, int Ordinal)> classified = segments
            .Map((segment, ordinal) => (Slot: ProjectionSlot.Classify(visibility: segment.SegmentVisibility), Ordinal: ordinal));
        BuildReceipt<ProjectionSlot> classes = classified
            .Map(static row => row.Slot).Distinct()
            .Fold(BuildReceipt<ProjectionSlot>.Empty, (receipt, slot) => {
                Seq<int> members = classified.Filter(row => row.Slot == slot).Map(static row => row.Ordinal);
                return receipt
                    + BuildReceipt<ProjectionSlot>.Of(slot: slot, body: new BuildBody.Tally(Count: members.Count))
                    + BuildReceipt<ProjectionSlot>.Of(slot: slot, body: new BuildBody.Components(Indices: members));
            });
        Seq<int> outlined = classified.Filter(row => segments[row.Ordinal].IsSceneSilhouette).Map(static row => row.Ordinal);
        return classes
            + BuildReceipt<ProjectionSlot>.Of(slot: ProjectionSlot.Sourced, body: new BuildBody.SourceMap(
                Axis: SourceAxis.Subject, Rows: segments.Map(static segment => (int)segment.ParentCurve.SourceObject.Tag)))
            + BuildReceipt<ProjectionSlot>.Of(slot: ProjectionSlot.Sourced, body: new BuildBody.ComponentRows(
                Indices: segments.Map(static segment => segment.ParentCurve.SourceObjectComponentIndex)))
            + BuildReceipt<ProjectionSlot>.Of(slot: ProjectionSlot.Kinds, body: new BuildBody.SourceMap(
                Axis: SourceAxis.Kind, Rows: segments.Map(static segment => (int)segment.ParentCurve.SilhouetteType)))
            + BuildReceipt<ProjectionSlot>.Of(slot: ProjectionSlot.Fills, body: new BuildBody.SourceGroups(
                Axis: SourceAxis.Kind,
                Groups: segments.Map(static segment => toSeq(segment.CurveSideFills).Map(static fill => (int)fill))))
            + BuildReceipt<ProjectionSlot>.Of(slot: ProjectionSlot.Outline, body: new BuildBody.Components(Indices: outlined))
            + BuildReceipt<ProjectionSlot>.Of(slot: ProjectionSlot.Frame, body: new BuildBody.Motion(Value: frame))
            + BuildReceipt<ProjectionSlot>.Of(slot: ProjectionSlot.Bounds, body: new BuildBody.Bounds(Value: bounds))
            + BuildReceipt<ProjectionSlot>.Of(slot: ProjectionSlot.Marks, body: new BuildBody.Marks(
                Points: points.Map(static point => point.Location)))
            + BuildReceipt<ProjectionSlot>.Of(slot: ProjectionSlot.Marks, body: new BuildBody.SourceMap(
                Axis: SourceAxis.Subject, Rows: points.Map(static point => (int)point.SourceObject.Tag)))
            + BuildReceipt<ProjectionSlot>.Of(slot: ProjectionSlot.Marks, body: new BuildBody.ComponentRows(
                Indices: points.Map(static point => point.SourceObjectComponentIndex)))
            + BuildReceipt<ProjectionSlot>.Of(slot: ProjectionSlot.Marks, body: new BuildBody.SourceMap(
                Axis: SourceAxis.Kind, Rows: points.Map(static point => (int)point.PointVisibility)));
    }

    private static Fin<Built<ProjectionSlot>> Outlined(
        (GeometryBase Native, Context Model, Outline Edit, Seq<Plane> Clips, Op Op) ctx,
        Func<Silhouette[]> plain,
        Func<Silhouette[]> clipped) =>
        Captured(slot: ProjectionSlot.Outline, op: ctx.Op,
            run: ctx.Clips.IsEmpty && ctx.Edit.Cancel.IsNone ? plain : clipped);

    internal static Fin<Seq<Plane>> AdmittedClips(Seq<Plane> clips, Op key) =>
        clips
            .Traverse(clip => key.AcceptInput(value: clip).ToValidation())
            .As()
            .ToFin();

    private static Fin<Built<ProjectionSlot>> Captured(ProjectionSlot slot, Op op, Func<Silhouette[]> run) =>
        op.Catch(() => {
            Seq<Silhouette> captured = toSeq(run() ?? []);
            BuildReceipt<ProjectionSlot> evidence =
                BuildReceipt<ProjectionSlot>.Of(slot: ProjectionSlot.Kinds, body: new BuildBody.SourceMap(
                    Axis: SourceAxis.Kind,
                    Rows: captured.Filter(static outline => outline.Curve is not null)
                        .Map(static outline => (int)outline.SilhouetteType)))
                + BuildReceipt<ProjectionSlot>.Of(slot: slot, body: new BuildBody.ComponentRows(
                    Indices: captured.Filter(static outline => outline.Curve is not null)
                        .Map(static outline => outline.GeometryComponentIndex)));
            return ModelGate.OwnMany(
                    built: captured.Choose(static outline => Optional((GeometryBase?)outline.Curve)),
                    key: op, allowEmpty: true)
                .Map(owned => Built<ProjectionSlot>.Of(operation: op,
                    Products: owned,
                    Evidence: BuildReceipt<ProjectionSlot>.Of(slot: slot, body: new BuildBody.Tally(Count: owned.Count)) + evidence));
        });
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class Projections {
    public static Fin<Built<ProjectionSlot>> Build(Context context, params ReadOnlySpan<ProjectionOp> operations) =>
        ModelGate.Entry(
            context: context,
            operations: operations,
            admit: static (operation, _) => Fin.Succ(operation),
            apply: static (operation, model) => operation.Apply(domain: model));
}
```

## [05]-[SURFACE_LEDGER]

| [INDEX] | [CONCERN]              | [OWNER]             | [FORM]                                                 | [ENTRY]                          |
| :-----: | :--------------------- | :------------------ | :----------------------------------------------------- | :------------------------------- |
|  [01]   | projection frame       | `ProjectionFrame`   | snapshot / pose / plan / directed value frames         | `ProjectionOp` frame payloads    |
|  [02]   | drawing sources        | `ProjectionSubject` | leased handle with placement / clip / occluding rows   | `ProjectionOp.Drawing`           |
|  [03]   | drawing flags          | `DrawingLaw`        | whole parameter surface as one regime-toleranced value | `Rig` inside the drawing arm     |
|  [04]   | cancellable compute    | `ProjectionPacing`  | cancel + progress + thread grant as one policy row     | `DrawingLaw.Pacing`              |
|  [05]   | silhouette eye         | `SilhouetteFrame`   | perspective point / parallel direction / rigged frame  | `ProjectionOp.Outline`           |
|  [06]   | classified consequence | `ProjectionSlot`    | visibility, correspondence, frame, and bounds facts    | `Built<ProjectionSlot>.Evidence` |
|  [07]   | projection verbs       | `ProjectionOp`      | one flat `[Union]` under total generated dispatch      | `Projections.Build`              |

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
