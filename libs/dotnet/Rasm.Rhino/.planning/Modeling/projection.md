# [RASM_RHINO_MODELING_PROJECTION]

`Rasm.Rhino.Modeling` owns hidden-line drafting capture. `ProjectionOp` routes Make2D, silhouette, and draft-curve extraction through `Projections.Build`; `ProjectionFrame` admits camera values, `DrawingLaw` closes native policy, and `Context` supplies tolerance and angle.

## [01]-[INDEX]

- [02]-[PROJECTION_FRAME]: `ProjectionFrame` — the value-only camera family and its `ViewportInfo` rig.
- [03]-[DRAWING_POLICY]: `DrawingFeature`, `DrawingRejoin`, `DrawingThreads`, `SilhouetteKind`, `ProjectionSubject`, `DrawingLaw`, `ProjectionPacing`, `SilhouetteFrame` — flag rows, subjects, pacing, and the silhouette eye.
- [04]-[OPERATION_PIPELINE]: `ProjectionOp` and the `Projections.Build` entry.

## [02]-[PROJECTION_FRAME]

- Owner: `ProjectionFrame` `[Union]` closes the four frame sources — a sampled `CameraSnapshot`, a synthetic `CameraPose` over subject bounds, a drafting-plane plan, and an explicit parallel direction; `Rig` is the one site minting the disposable `ViewportInfo` every host compute consumes.
- Law: the frame is a sampled value, never a live viewport — a `RhinoViewport` or `ViewportLease` never crosses this pipeline; live sampling is the Viewport pipeline's `CameraSnapshot.Take`, and the `Snapshot`/`Pose` cases consume that pipeline's value shapes as the ruled Modeling counter-edge, so a headless drawing and a viewport-true drawing enter one union.
- Law: the seat is verified member by member — `SetCameraLocation(Point3d)`, `SetCameraDirection(Vector3d)`, `SetCameraUp(Vector3d)`, and `SetFrustumNearFar(BoundingBox)` each answer `bool` on `Rhino.DocObjects.ViewportInfo` and every one rides the same short-circuit chain into one confirmation, so a refused seat step is a typed `InvalidResult` and never a discarded return; the projection transition selects `ChangeToParallelProjection`/`ChangeToPerspectiveProjection`/`ChangeToTwoPointPerspectiveProjection` off the pose's `ProjectionKind` row inside that chain, and the bounds overload derives the clip range from the subject so no frustum scalar is hand-derived and the point-and-distance, near-far, and full six-scalar overloads stay unused.
- Law: custody is a CASE, never a nulled local — `Seat` acquires the frame through `Lease.Acquire`, so a refused seat step or a throwing configure body rolls the acquired native back with the cleanup fault aggregated into the primary, and a successful seat hands the caller the detached frame its own `using` scopes. Both the `frame = null` transfer sentinel and its `try`/`finally` delete.
- Law: `Directed` requires its up vector — a local orthogonalization fallback is the killed form the camera page names, so an explicit up crosses the case and the plan case reads the drafting plane's own `YAxis`; `IsValid` proves both direction axes non-degenerate before any arm reaches a seat step.
- Law: the lens seat performs no optics arithmetic — `LensAngle` is the FULL vertical view angle and `ViewportInfo.CameraAngle` holds its HALF (live-proven 13.4957 deg = atan(12/50) at 50mm, identical on `RhinoViewport.CameraAngle`), so the seat halves on write exactly as `Viewport/camera.md` doubles on read; the write is a NAMED side effect so no mutation hides inside a dispatch arm, the 35mm lens length reads back off `Camera35mmLensLength` so the half-frame diagonal is the host's own constant rather than a transcribed literal, and the perspective target distance reads the pose's admitted target, never a magic depth.
- Boundary: screen landing is not this owner — a consumer placing the flattened drawing on a sheet composes `ViewTransforms.Mapping` over `ViewMapping` on the Viewport pipeline; this frame ends at the projection compute.
- Packages: RhinoCommon geometry (`.api/api-rhinocommon-geometry.md` — `HiddenLineDrawing*` `:149-158`, `Silhouette` `:178-188`), RhinoCommon document (`.api/api-rhinocommon-document.md` — `Rhino.DocObjects.ViewportInfo` seat and `Camera35mmLensLength`), kernel `Domain/results` (`Op`, `Lease<T>.Acquire`, `ValidityClaim`, `IValidityEvidence`, `Fin`), `Rasm.Rhino.Viewport` (`CameraSnapshot`, `CameraPose`, `LensAngle`, `ProjectionKind`), `Modeling/curves.md` (`ModelClaim`), `Modeling/solids.md` (`ModelGate`), LanguageExt.Core, Thinktecture.Runtime.Extensions.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Rasm.Domain;
using Rasm.Numerics;
using Rasm.Rhino.Document;
using Rasm.Rhino.Viewport;
using Rhino.DocObjects;
using Rhino.Geometry;

namespace Rasm.Rhino.Modeling;

// --- [TYPES] ---------------------------------------------------------------------------
[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ProjectionFrame : IValidityEvidence {
    private ProjectionFrame() { }
    public sealed record Snapshot(CameraSnapshot Value) : ProjectionFrame;
    public sealed record Pose(CameraPose Value, BoundingBox Subject) : ProjectionFrame;
    public sealed record Look(Plane Frame, BoundingBox Subject) : ProjectionFrame;
    public sealed record Directed(Vector3d Direction, Vector3d Up, BoundingBox Subject) : ProjectionFrame;

    public bool IsValid => Switch(
        snapshot: static frame => (ValidityClaim)(frame.Value is not null),
        pose: static frame => ValidityClaim.All(frame.Value is not null, frame.Subject.IsValid),
        look: static frame => ValidityClaim.All(frame.Frame.IsValid, frame.Subject.IsValid),
        directed: static frame => ValidityClaim.All(
            ValidityClaim.Direction(value: frame.Direction), ValidityClaim.Direction(value: frame.Up), frame.Subject.IsValid));

    internal Fin<ViewportInfo> Rig() =>
        Switch(
            context: key,
            snapshot: static (frame) => Seated(
                pose: frame.Value.Pose, subject: frame.Value.Frustum.Bounds),
            pose: static (frame) =>
                from _ in Acceptance.Input(value: frame.Subject)
                from rigged in Seated(pose: frame.Value, subject: frame.Subject)
                select rigged,
            look: static (frame) =>
                from plane in Acceptance.Input(value: frame.Frame)
                from _ in Acceptance.Input(value: frame.Subject)
                from rigged in Parallel(
                    eye: frame.Subject.Center + plane.ZAxis * frame.Subject.Diagonal.Length,
                    direction: -plane.ZAxis, up: plane.YAxis, subject: frame.Subject)
                select rigged,
            directed: static (frame) =>
                from direction in Acceptance.Input(value: frame.Direction)
                from up in Acceptance.Input(value: frame.Up)
                from _ in Acceptance.Input(value: frame.Subject)
                from rigged in Parallel(
                    eye: frame.Subject.Center - direction * frame.Subject.Diagonal.Length,
                    direction: direction, up: up, subject: frame.Subject)
                select rigged);

    private static Fin<ViewportInfo> Seated(CameraPose pose, BoundingBox subject) =>
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
                            targetDistance: seat.Reach, symmetricFrustum: true, lensLength: Lens(frame: seat.Frame, angle: seat.Angle)),
                        twoPoint: static seat => seat.Frame.ChangeToTwoPointPerspectiveProjection(
                            targetDistance: seat.Reach, up: seat.Up, lensLength: Lens(frame: seat.Frame, angle: seat.Angle)))
                    && frame.SetFrustumNearFar(state.Subject);
            });

    private static Fin<ViewportInfo> Parallel(Point3d eye, Vector3d direction, Vector3d up, BoundingBox subject) =>
        Seat(
            state: (Eye: eye, Direction: direction, Up: up, Subject: subject),
            configure: static (state, frame) => frame.SetCameraLocation(state.Eye)
                && frame.SetCameraDirection(state.Direction)
                && frame.SetCameraUp(state.Up)
                && frame.ChangeToParallelProjection(symmetricFrustum: true)
                && frame.SetFrustumNearFar(state.Subject));

    private static Fin<ViewportInfo> Seat<TState>(
        TState state, Func<TState, ViewportInfo, bool> configure) =>
        Lease<ViewportInfo>.Acquire(mint: static () => new ViewportInfo())
            .Bind(lease => Try.lift(() => Admit.Confirm(success: configure(state, lease.Resource))).Run().Bind(static inner => inner)
                .Map(_ => lease.Resource)
                .Rollback(lease.Resource));

    private static double Lens(ViewportInfo frame, LensAngle angle) {
        _ = HostEdge.Side(action: () => frame.CameraAngle = (double)angle / 2.0);
        return frame.Camera35mmLensLength;
    }
}
```

## [03]-[DRAWING_POLICY]

- Owner: `ProjectionSubject` carries one drawing source as an admitted row — leased handle, optional admitted placement, per-subject clip planes, and the occluding grant; `DrawingFeature` closes every `HiddenLineDrawingParameters` flag as a capability vocabulary carrying its own writer column, `SilhouetteKind` closes the host's `[Flags]` silhouette word as a capability vocabulary carrying its bit, `DrawingRejoin` and `DrawingThreads` close the two compute behaviors, `DrawingLaw` fuses them into one value; `ProjectionPacing` is the cancellable-compute policy row; `SilhouetteFrame` closes the three silhouette eye modalities.
- Law: placement is admitted, never raw — a subject motion enters as `TransformSpec` and lowers through `Placement.Build`, so `TransformSpec.PlanarProjection` and `DirectionalProjection` are the projection-transform spellings and a directly constructed host `Transform` bypassing kernel admission is the deleted form.
- Law: no drawing flag travels as a bare bool — the five `HiddenLineDrawingParameters` toggles are `DrawingFeature` rows carrying their own writer column and enter as one `CapabilitySet`, so `Rig` folds `DrawingFeature.Items` once and a new host flag is one row; `AbsoluteTolerance` reads the regime, a tolerance knob beside the law is the deleted form, and the public occluding flag is `OccludingSectionOption` — the same-named list member is host-internal. Rejoin and threading are compute behavior rather than parameter state, so they stay their own two-row `Native` vocabularies; the per-subject occluding grant is NOT — it is one bit of parameter state the host reads off the `AddGeometry` argument with no correlated partner and no host projection column, so it travels as a named `bool` on the subject exactly as the sibling curve and mesh pipelines spell their solitary grants.
- Law: a host `[Flags]` word is a `CapabilitySet`, never a raw enum — `SilhouetteType` is `[Flags]`, so `Enum.IsDefined` answers FALSE for every composite mask a caller legitimately builds and the admission it guarded refused every real request; the vocabulary carries the bit, `Mask` folds it, and the empty set IS the host's `None`.
- Law: every policy value is admitted at CONSTRUCTION — `ProjectionPacing`, `ProjectionSubject`, and `DrawingLaw` run the same fold their `IsValid` reads, so the null-check chain that stood in for admission at the operation gate deletes and the roster's `Admitted` states domain facts alone. NAMED LOSS: the defaulted `Placement`/`Clips` columns must now be supplied at the call; bought back by an invalid subject being unconstructible.
- Law: pacing is ONE value and cancellation ONE spelling — `ProjectionPacing` fuses the token, optional progress, and the thread row, and `Outline` and `Draft` carry a bare `CancellationToken` whose `default` IS the host's own `CancellationToken.None`; an `Option<CancellationToken>` stacks a second absence on a value that already models it. Reporter lowering rides `HostEdge.Slot` at the ONE call that writes it; `ModelRuntime` carries the shared context while this operation-owned value selects host compute behavior.
- Law: every short host overload delegates verbatim to its long form with `null` progress or planes and `CancellationToken.None`, so this page composes the long form alone and no arm branches to pick an overload — an identity `Transform` and an empty plane list are the host's own no-op spellings.
- Packages: RhinoCommon geometry (`.api/api-rhinocommon-geometry.md` — `HiddenLineDrawingParameters` `:149-158` incl. `AddGeometryAndPlanes`, `SetViewport`, `AddClippingPlane`, and the five flag members; `SilhouetteType` `[Flags]` roster `:81`), kernel `Domain/results` (`Op`, `HostEdge.Slot`, `ValidityClaim`, `IValidityEvidence`), kernel `Domain/validation` (`ICapability`, `CapabilitySet`), kernel `Domain/context` (`Context.Absolute`), kernel `Numerics/atoms` (`TransformSpec`, `Placement.Build` — the kernel transform builder, NOT the `Blocks/model.md` `Placement` block-instance union), `Rasm.Rhino.Document` (`GeometryHandle`), `Modeling/curves.md` (`ModelClaim`), Thinktecture.Runtime.Extensions, LanguageExt.Core.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DrawingFeature : ICapability<DrawingFeature> {
    public static readonly DrawingFeature TangentEdges = new(key: "tangent-edges", write: static (p, on) => { p.IncludeTangentEdges = on; return unit; });
    public static readonly DrawingFeature TangentSeams = new(key: "tangent-seams", write: static (p, on) => { p.IncludeTangentSeams = on; return unit; });
    public static readonly DrawingFeature HiddenCurves = new(key: "hidden-curves", write: static (p, on) => { p.IncludeHiddenCurves = on; return unit; });
    public static readonly DrawingFeature Flatten = new(key: "flatten", write: static (p, on) => { p.Flatten = on; return unit; });
    public static readonly DrawingFeature OccludingSections = new(key: "occluding-sections", write: static (p, on) => { p.OccludingSectionOption = on; return unit; });

    [UseDelegateFromConstructor]
    internal partial Unit Write(HiddenLineDrawingParameters parameters, bool enabled);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SilhouetteKind : ICapability<SilhouetteKind> {
    public static readonly SilhouetteKind Projecting = new(key: "projecting", bit: 0x1);
    public static readonly SilhouetteKind TangentProjects = new(key: "tangent-projects", bit: 0x2);
    public static readonly SilhouetteKind Tangent = new(key: "tangent", bit: 0x4);
    public static readonly SilhouetteKind Crease = new(key: "crease", bit: 0x8);
    public static readonly SilhouetteKind Boundary = new(key: "boundary", bit: 0x10);
    public static readonly SilhouetteKind NonSilhouetteCrease = new(key: "non-silhouette-crease", bit: 0x100);
    public static readonly SilhouetteKind NonSilhouetteTangent = new(key: "non-silhouette-tangent", bit: 0x200);
    public static readonly SilhouetteKind NonSilhouetteSeam = new(key: "non-silhouette-seam", bit: 0x400);
    public static readonly SilhouetteKind SectionCut = new(key: "section-cut", bit: 0x1000);
    public static readonly SilhouetteKind MiscellaneousFeature = new(key: "miscellaneous-feature", bit: 0x2000);
    public static readonly SilhouetteKind DraftCurve = new(key: "draft-curve", bit: 0x8000);

    public int Rank => Bit;
    internal int Bit { get; }

    internal static SilhouetteType Native(CapabilitySet<SilhouetteKind> kinds) =>
        (SilhouetteType)kinds.Mask(bit: static kind => kind.Bit);
}

[SmartEnum<int>]
public sealed partial class DrawingRejoin {
    public static readonly DrawingRejoin Raw = new(key: 0, native: false);
    public static readonly DrawingRejoin Compatible = new(key: 1, native: true);

    internal bool Native { get; }
}

[SmartEnum<int>]
public sealed partial class DrawingThreads {
    public static readonly DrawingThreads Serial = new(key: 0, native: false);
    public static readonly DrawingThreads Parallel = new(key: 1, native: true);

    internal bool Native { get; }
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SilhouetteFrame : IValidityEvidence {
    private SilhouetteFrame() { }
    public sealed record Eye(Point3d Value) : SilhouetteFrame;
    public sealed record Along(Vector3d Value) : SilhouetteFrame;
    public sealed record Framed(ProjectionFrame Value) : SilhouetteFrame;

    public bool IsValid => Switch(
        eye: static frame => ValidityClaim.Finite(value: frame.Value),
        along: static frame => ValidityClaim.Direction(value: frame.Value),
        framed: static frame => (ValidityClaim)(frame.Value is { IsValid: true }));
}

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct ProjectionPacing : IValidityEvidence {
    public CancellationToken Cancel { get; }
    public Option<IProgress<double>> Progress { get; }
    public DrawingThreads Threads { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref CancellationToken cancel,
        ref Option<IProgress<double>> progress,
        ref DrawingThreads threads) =>
        validationError = threads is not null
            ? null
            : new ValidationError("Projection pacing requires a declared thread row.");

    public static readonly ProjectionPacing Unpaced = Create(
        cancel: CancellationToken.None, progress: Option<IProgress<double>>.None, threads: DrawingThreads.Parallel);

    public bool IsValid => Threads is not null;
}

[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct ProjectionSubject : IValidityEvidence {
    public GeometryHandle Geometry { get; }
    public bool Occluding { get; }
    public Option<TransformSpec> Placement { get; }
    public Seq<Plane> Clips { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref GeometryHandle geometry,
        ref bool occluding,
        ref Option<TransformSpec> placement,
        ref Seq<Plane> clips) =>
        validationError = Admits(geometry: geometry, clips: clips)
            ? null
            : new ValidationError("A drawing subject requires a live geometry handle and valid clipping planes.");

    public bool IsValid => Admits(geometry: Geometry, clips: Clips);

    private static ValidityClaim Admits(GeometryHandle? geometry, Seq<Plane> clips) =>
        ValidityClaim.All(
            ModelClaim.Handle(handle: geometry),
            ModelClaim.Rows(rows: clips, claim: static clip => (ValidityClaim)clip.IsValid, allowEmpty: true));
}

[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct DrawingLaw : IValidityEvidence {
    public CapabilitySet<DrawingFeature> Features { get; }
    public DrawingRejoin Rejoin { get; }
    public ProjectionPacing Pacing { get; }
    public Seq<Plane> Clips { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref CapabilitySet<DrawingFeature> features,
        ref DrawingRejoin rejoin,
        ref ProjectionPacing pacing,
        ref Seq<Plane> clips) =>
        validationError = Admits(rejoin: rejoin, pacing: pacing, clips: clips)
            ? null
            : new ValidationError("A drawing law requires a declared rejoin row, an admitted pacing row, and valid clipping planes.");

    public bool IsValid => Admits(rejoin: Rejoin, pacing: Pacing, clips: Clips);

    internal Fin<HiddenLineDrawingParameters> Rig(Context domain) {
        DrawingLaw law = this;
        return from clips in ProjectionOp.AdmittedClips(law.Clips)
               from parameters in Try.lift(() => {
                   HiddenLineDrawingParameters parameters = new() { AbsoluteTolerance = domain.Absolute.Value };
                   _ = toSeq(DrawingFeature.Items).Iter(feature =>
                       feature.Write(parameters: parameters, enabled: law.Features.Admits(capability: feature)));
                   foreach (Plane clip in clips) { parameters.AddClippingPlane(plane: clip); }
                   return Fin.Succ(value: parameters);
               }).Run().Bind(static inner => inner)
               select parameters;
    }

    private static ValidityClaim Admits(DrawingRejoin? rejoin, ProjectionPacing pacing, Seq<Plane> clips) =>
        ValidityClaim.All(
            rejoin is not null,
            pacing.IsValid,
            ModelClaim.Rows(rows: clips, claim: static clip => (ValidityClaim)clip.IsValid, allowEmpty: true));
}
```

## [04]-[OPERATION_PIPELINE]

- Owner: `ProjectionOp` `[Union]` `` closes the drawing, silhouette, and draft verbs; `Projections.Build` folds the admitted spread through `ModelGate` and returns the owned handles directly.
- Law: products detach before the engine dies — every segment curve duplicates out of the disposable `HiddenLineDrawing` onto an owned handle inside the compute window, silhouette curves own directly because `Silhouette.Compute` returns fresh geometry, and the drawing, its `ViewportInfo`, and every mid-fold failure release symmetrically.
- Law: native projections materialize inside their custody window — every map over `HiddenLineDrawingSegment` or `Silhouette` closes with `Strict()` before its native owner releases.
- Law: clipping planes cross one admission fold — drawing-global, subject-selective, and outline clip sets traverse `AcceptInput` applicatively, and each arm hands the admitted sequence to the one host overload it composes; an empty admitted set reaches the host as an empty plane list, which the native reads exactly as the no-planes short form does.
- Law: a missing host capture refuses through `InvalidResult`; an answered empty capture remains a successful empty geometry sequence.
- Law: the capture altitudes stay split — `Drawing` is the occlusion-resolved scene, `Outline` is the per-object `Silhouette.Compute` capture whose kind mask and clip set cross as values, and `Draft` is `ComputeDraftCurve` under an explicit pull; a BSP, painter, per-sample occlusion march, or host round-trip re-derivation of any altitude is the kernel view page's enumerated dead form.
- Law: admission NAMES its axis and closes at compile — `Admitted` dispatches the generated `Switch` into the spine's `ModelClaim.Admits`, so a new verb breaks the compile instead of falling to a silent refusal and a request breaching several constraints reports each one.
- Boundary: geometric curve projection stays the curve pipeline — `CurveOp.Project` over `ProjectTarget` owns plane, brep, and mesh target projection with index maps; this pipeline begins at host hidden-line and silhouette capture.
- Growth: a new drawing modality is one `ProjectionOp` case with its arm; a new frame source is one `ProjectionFrame` case every verb reads.
- Packages: RhinoCommon geometry (`.api/api-rhinocommon-geometry.md` — `HiddenLineDrawing.Compute`/`Segments`/`RejoinCompatibleVisible` `:149-158`, `Silhouette.Compute`/`ComputeDraftCurve` `:178-188`), kernel `Domain/results` (`Op`, `` + generated `SelfOp`, `ValidityClaim`, `Fin`), kernel `Numerics/atoms` (`Placement.Build` — the kernel transform builder, NOT the `Blocks/model.md` `Placement` block-instance union), `Rasm.Rhino.Document` (`GeometryHandle`), `Modeling/curves.md` (`ModelClaim`), `Modeling/solids.md` (`ModelGate`), LanguageExt.Core (`TraverseM`, `Traverse`, `Choose`, `Strict`, `Zip`), Thinktecture.Runtime.Extensions.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ProjectionOp {
    private ProjectionOp() { }
    public sealed record Drawing(Seq<ProjectionSubject> Subjects, ProjectionFrame Frame, DrawingLaw Law) : ProjectionOp;
    public sealed record Outline(
        GeometryHandle Subject, CapabilitySet<SilhouetteKind> Kinds, SilhouetteFrame Frame,
        Seq<Plane> Clips = default, CancellationToken Cancel = default) : ProjectionOp;
    public sealed record Draft(
        GeometryHandle Subject, double Angle, Vector3d Pull,
        CancellationToken Cancel = default) : ProjectionOp;

    internal Fin<ProjectionOp> Admitted() =>
        Switch(
            context: key,
            drawing: static (row) => ModelClaim.Admits(row,
                (nameof(row.Subjects), ModelClaim.Rows(rows: row.Subjects, claim: static subject => (ValidityClaim)subject.IsValid)),
                (nameof(row.Frame), row.Frame is { IsValid: true }),
                (nameof(row.Law), row.Law.IsValid)),
            outline: static (row) => ModelClaim.Admits(row,
                (nameof(row.Subject), ModelClaim.Handle(handle: row.Subject)),
                (nameof(row.Frame), row.Frame is { IsValid: true }),
                (nameof(row.Clips), ModelClaim.Rows(rows: row.Clips, claim: static clip => (ValidityClaim)clip.IsValid, allowEmpty: true))),
            draft: static (row) => ModelClaim.Admits(row,
                (nameof(row.Subject), ModelClaim.Handle(handle: row.Subject)),
                (nameof(row.Angle), ValidityClaim.Finite(value: row.Angle)),
                (nameof(row.Pull), ValidityClaim.Direction(value: row.Pull))));

    internal Fin<Seq<GeometryHandle>> Apply(Context domain) =>
        Switch(
            context: domain,
            drawing: static (model, edit) => {
                 = Drawing.SelfOp;
                DrawingLaw law = edit.Law;
                return ModelGate.BorrowMany<GeometryBase, Seq<GeometryHandle>>(
                    handles: edit.Subjects.Map(static subject => subject.Geometry),
                    body: natives =>
                        from parameters in law.Rig(domain: model, key: op)
                        from rigged in edit.Frame.Rig()
                        from harvested in Try.lift(() => {
                            using ViewportInfo frame = rigged;
                            parameters.SetViewport(frame);
                            return edit.Subjects.Zip(natives)
                                .Map(static (pair, ordinal) => (Subject: pair.First, Native: pair.Second, Ordinal: ordinal))
                                .TraverseM(row => Registered(
                                    parameters: parameters, subject: row.Subject, native: row.Native,
                                    ordinal: row.Ordinal, model: model)).As()
                                .Bind(_ => Computed(parameters: parameters, law: law));
                        }).Run().Bind(static inner => inner)
                        select harvested);
            },
            outline: static (model, edit) => {
                 = Outline.SelfOp;
                return from clips in AdmittedClips(edit.Clips)
                       from built in ModelGate.Borrow<GeometryBase, Seq<GeometryHandle>>(handle: edit.Subject, body: native =>
                    edit.Frame.Switch(
                        context: (Native: native, Model: model, Edit: edit, Clips: clips),
                        eye: static (ctx, frame) => Captured(token: ctx.Edit.Cancel, run: () =>
                            Silhouette.Compute(
                                ctx.Native, SilhouetteKind.Native(kinds: ctx.Edit.Kinds), frame.Value,
                                ctx.Model.Absolute.Value, ctx.Model.Angle.Value,
                                ctx.Clips.AsIterable(), ctx.Edit.Cancel)),
                        along: static (ctx, frame) => Captured(token: ctx.Edit.Cancel, run: () =>
                            Silhouette.Compute(
                                ctx.Native, SilhouetteKind.Native(kinds: ctx.Edit.Kinds), frame.Value,
                                ctx.Model.Absolute.Value, ctx.Model.Angle.Value,
                                ctx.Clips.AsIterable(), ctx.Edit.Cancel)),
                        framed: static (ctx, frame) =>
                            from rigged in frame.Value.Rig()
                            from built in Try.lift(() => {
                                using ViewportInfo viewport = rigged;
                                return Captured(token: ctx.Edit.Cancel, run: () =>
                                    Silhouette.Compute(
                                        ctx.Native, SilhouetteKind.Native(kinds: ctx.Edit.Kinds), viewport,
                                        ctx.Model.Absolute.Value, ctx.Model.Angle.Value,
                                        ctx.Clips.AsIterable(), ctx.Edit.Cancel));
                            }).Run().Bind(static inner => inner)
                            select built))
                       select built;
            },
            draft: static (model, edit) => {
                 = Draft.SelfOp;
                return ModelGate.Borrow<GeometryBase, Seq<GeometryHandle>>(handle: edit.Subject, body: native =>
                    from pull in Acceptance.Input(value: edit.Pull)
                    from built in Captured(token: edit.Cancel, run: () =>
                        Silhouette.ComputeDraftCurve(
                            native, edit.Angle, pull, model.Absolute.Value, model.Angle.Value, edit.Cancel))
                    select built);
            });

    private static Fin<Unit> Registered(
        HiddenLineDrawingParameters parameters, ProjectionSubject subject, GeometryBase native,
        int ordinal, Context model) =>
        from motion in subject.Placement
            .Traverse(spec => Placement.Build(spec: spec, context: Some(model))).As()
        from clips in AdmittedClips(subject.Clips)
        from _ in Admit.Confirm(success: parameters.AddGeometryAndPlanes(
            native, motion.IfNone(noneValue: Transform.Identity), ordinal, subject.Occluding, [.. clips]))
        select unit;

    private static Fin<Seq<GeometryHandle>> Computed(HiddenLineDrawingParameters parameters, DrawingLaw law) {
        HiddenLineDrawing? computed = HiddenLineDrawing.Compute(
            parameters, law.Pacing.Threads.Native, HostEdge.Slot(value: law.Pacing.Progress), law.Pacing.Cancel);
        return Optional(computed).ToFin(Fail: new KernelFault.InvalidResult()).Bind(drawing => {
            using (drawing) {
                if (law.Rejoin.Native) { drawing.RejoinCompatibleVisible(); }
                Seq<HiddenLineDrawingSegment> segments = toSeq(drawing.Segments).Strict();
                return ModelGate.OwnMany(
                    built: segments.Map(static segment => (GeometryBase)segment.CurveGeometry.Duplicate()).Strict(), allowEmpty: true);
            }
        });
    }

    internal static Fin<Seq<Plane>> AdmittedClips(Seq<Plane> clips) =>
        clips
            .Traverse(clip => Acceptance.Input(value: clip).ToValidation())
            .As()
            .ToFin();

    private static Fin<Seq<GeometryHandle>> Captured(CancellationToken token, Func<Silhouette[]> run) =>
        Try.lift(() => Optional(run())
            .ToFin(Fail: new KernelFault.InvalidResult(Detail: Some("the host capture answered nothing")))
            .Bind(captured => ModelGate.OwnMany(
                built: toSeq(captured)
                    .Choose(static outline => Optional(outline.Curve).Map(static curve => (GeometryBase)curve))
                    .Strict(),
                allowEmpty: true))).Run().Bind(static inner => inner);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Projections {
    public static Eff<ModelRuntime, Seq<GeometryHandle>> Build(params ReadOnlySpan<ProjectionOp> operations) {
        Seq<ProjectionOp> captured = toSeq(operations.ToArray());
        return Eff.runtime<ModelRuntime>().Bind(runtime =>
            ModelGate.Entry(
                runtime: runtime,
                operations: captured,
                admit: static (operation, key) => operation.Admitted(),
                apply: static (operation, model) => operation.Apply(domain: model)).ToEff());
    }
}
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
