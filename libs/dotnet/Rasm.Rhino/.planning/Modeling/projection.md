# [RASM_RHINO_MODELING_PROJECTION]

`Rasm.Rhino.Modeling` owns hidden-line drafting capture. One `ProjectionOp` union carries the Make2D hidden-line drawing, per-object silhouette capture, and draft-curve extraction through `Projections.Build`; `ProjectionFrame` admits the sampled camera, synthetic pose, plan, and directional frames as values, `DrawingLaw` closes the parameter surface, and `Context` supplies every tolerance and angle. Occlusion classification is the host engine's; the exact analytic drawing (`View.Apply` onto `DrawingProjection`), section cuts (`IntersectOp.PlaneMesh`), region fill (`ArrangementOp.PlanarOverlay` behind `DrawingProjection.Fill`), and exact orientation (`Predicate.Orient2D`) remain kernel-owned, so this page is the HOST capture altitude under the capture law and never re-derives a visibility algorithm.

## [01]-[INDEX]

- [02]-[PROJECTION_FRAME]: `ProjectionFrame` — the value-only camera family and its `ViewportInfo` rig.
- [03]-[DRAWING_POLICY]: `DrawingFeature`, `DrawingRejoin`, `DrawingThreads`, `SilhouetteKind`, `ProjectionSubject`, `DrawingLaw`, `ProjectionPacing`, `SilhouetteFrame` — flag rows, subjects, pacing, and the silhouette eye.
- [04]-[OPERATION_RAIL]: `ProjectionSlot`, `ProjectionOp`, and the `Projections.Build` entry.

## [02]-[PROJECTION_FRAME]

- Owner: `ProjectionFrame` `[Union]` closes the four frame sources — a sampled `CameraSnapshot`, a synthetic `CameraPose` over subject bounds, a drafting-plane plan, and an explicit parallel direction; `Rig` is the one site minting the disposable `ViewportInfo` every host compute consumes.
- Law: the frame is a sampled value, never a live viewport — a `RhinoViewport` or `ViewportLease` never crosses this rail; live sampling is the Viewport rail's `CameraSnapshot.Take`, and the `Snapshot`/`Pose` cases consume that rail's value shapes as the ruled Modeling counter-edge, so a headless drawing and a viewport-true drawing enter one union.
- Law: the seat is verified member by member — `SetCameraLocation(Point3d)`, `SetCameraDirection(Vector3d)`, `SetCameraUp(Vector3d)`, and `SetFrustumNearFar(BoundingBox)` each answer `bool` on `Rhino.DocObjects.ViewportInfo` and every one rides the same short-circuit chain into one confirmation, so a refused seat step is a typed `InvalidResult` and never a discarded return; the projection transition selects `ChangeToParallelProjection`/`ChangeToPerspectiveProjection`/`ChangeToTwoPointPerspectiveProjection` off the pose's `ProjectionKind` row inside that chain, and the bounds overload derives the clip range from the subject so no frustum scalar is hand-derived and the point-and-distance, near-far, and full six-scalar overloads stay unused.
- Law: custody is a CASE, never a nulled local — `Seat` acquires the frame through `Lease.Acquire`, so a refused seat step or a throwing configure body rolls the acquired native back with the cleanup fault aggregated into the primary, and a successful seat hands the caller the detached frame its own `using` scopes. Both the `frame = null` transfer sentinel and its `try`/`finally` delete.
- Law: `Directed` requires its up vector — a local orthogonalization fallback is the killed form the camera page names, so an explicit up crosses the case and the plan case reads the drafting plane's own `YAxis`; `IsValid` proves both direction axes non-degenerate before any arm reaches a seat step.
- Law: the lens seat performs no optics arithmetic — `LensAngle` is the FULL vertical view angle and `ViewportInfo.CameraAngle` holds its HALF (live-proven 13.4957 deg = atan(12/50) at 50mm, identical on `RhinoViewport.CameraAngle`), so the seat halves on write exactly as `Viewport/camera.md` doubles on read; the write is a NAMED side effect so no mutation hides inside a dispatch arm, the 35mm lens length reads back off `Camera35mmLensLength` so the half-frame diagonal is the host's own constant rather than a transcribed literal, and the perspective target distance reads the pose's admitted target, never a magic depth.
- Boundary: screen landing is not this owner — a consumer placing the flattened drawing on a sheet composes `ViewTransforms.Mapping` over `ViewMapping` on the Viewport rail; this frame ends at the projection compute.
- Packages: RhinoCommon geometry (`.api/api-rhinocommon-geometry.md` — `HiddenLineDrawing*` `:149-158`, `Silhouette` `:178-188`), RhinoCommon document (`.api/api-rhinocommon-document.md` — `Rhino.DocObjects.ViewportInfo` seat and `Camera35mmLensLength`), kernel `Domain/rails` (`Op`, `Lease<T>.Acquire`, `ValidityClaim`, `IValidityEvidence`, `Fin`), `Rasm.Rhino.Viewport` (`CameraSnapshot`, `CameraPose`, `LensAngle`, `ProjectionKind`), `Modeling/curves.md` (`ModelClaim`), `Modeling/solids.md` (`ModelGate`), LanguageExt.Core, Thinktecture.Runtime.Extensions.

```csharp
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
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
                            targetDistance: seat.Reach, symmetricFrustum: true, lensLength: Lens(frame: seat.Frame, angle: seat.Angle)),
                        twoPoint: static seat => seat.Frame.ChangeToTwoPointPerspectiveProjection(
                            targetDistance: seat.Reach, up: seat.Up, lensLength: Lens(frame: seat.Frame, angle: seat.Angle)))
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
        Lease<ViewportInfo>.Acquire(mint: static () => new ViewportInfo(), key: key)
            .Bind(lease => key
                .Catch(() => key.Confirm(success: configure(state, lease.Resource)))
                .Map(_ => lease.Resource)
                .Rollback(lease.Resource));

    private static double Lens(ViewportInfo frame, LensAngle angle) {
        _ = Op.Side(action: () => frame.CameraAngle = (double)angle / 2.0);
        return frame.Camera35mmLensLength;
    }
}
```

## [03]-[DRAWING_POLICY]

- Owner: `ProjectionSubject` carries one drawing source as an admitted row — leased handle, optional admitted placement, per-subject clip planes, and the occluding grant; `DrawingFeature` closes every `HiddenLineDrawingParameters` flag as a capability vocabulary carrying its own writer column, `SilhouetteKind` closes the host's `[Flags]` silhouette word as a capability vocabulary carrying its bit, `DrawingRejoin` and `DrawingThreads` close the two compute behaviors, `DrawingLaw` fuses them into one value; `ProjectionPacing` is the cancellable-compute policy row; `SilhouetteFrame` closes the three silhouette eye modalities.
- Law: placement is admitted, never raw — a subject motion enters as `TransformSpec` and lowers through `Placement.Build`, so `TransformSpec.PlanarProjection` and `DirectionalProjection` are the projection-transform spellings and a directly constructed host `Transform` bypassing kernel admission is the deleted form.
- Law: the tag channel is the correspondence — every subject registers under its ordinal, so segment-to-source and point-to-source maps are host evidence read back from `HiddenLineDrawingObject.Tag`, never a geometry re-match.
- Law: no drawing flag travels as a bare bool — the five `HiddenLineDrawingParameters` toggles are `DrawingFeature` rows carrying their own writer column and enter as one `CapabilitySet`, so `Rig` folds `DrawingFeature.Items` once and a new host flag is one row; `AbsoluteTolerance` reads the regime, a tolerance knob beside the law is the deleted form, and the public occluding flag is `OccludingSectionOption` — the same-named list member is host-internal. Rejoin and threading are compute behavior rather than parameter state, so they stay their own two-row `Native` vocabularies; the per-subject occluding grant is NOT — it is one bit of parameter state the host reads off the `AddGeometry` argument with no correlated partner and no host projection column, so it travels as a named `bool` on the subject exactly as the sibling curve and mesh rails spell their solitary grants.
- Law: a host `[Flags]` word is a `CapabilitySet`, never a raw enum — `SilhouetteType` is `[Flags]`, so `Enum.IsDefined` answers FALSE for every composite mask a caller legitimately builds and the admission it guarded refused every real request; the vocabulary carries the bit, `Mask` folds it, and the empty set IS the host's `None`.
- Law: every policy value is admitted at CONSTRUCTION — `ProjectionPacing`, `ProjectionSubject`, and `DrawingLaw` run the same fold their `IsValid` reads, so the null-check chain that stood in for admission at the operation gate deletes and the roster's `Admitted` states domain facts alone. NAMED LOSS: the defaulted `Placement`/`Clips` columns must now be supplied at the call; bought back by an invalid subject being unconstructible.
- Law: pacing is ONE value and cancellation ONE spelling — `ProjectionPacing` fuses the token, optional progress, and the thread row, and `Outline` and `Draft` carry a bare `CancellationToken` whose `default` IS the host's own `CancellationToken.None`; an `Option<CancellationToken>` stacks a second absence on a value that already models it. Reporter lowering rides `Op.ToHostSlot` at the ONE call that writes it; `ModelRuntime` still carries the shared context and timeline into the entry, while this operation-owned pacing value selects the host compute behavior.
- Law: every short host overload delegates verbatim to its long form with `null` progress or planes and `CancellationToken.None`, so this page composes the long form alone and no arm branches to pick an overload — an identity `Transform` and an empty plane list are the host's own no-op spellings.
- Packages: RhinoCommon geometry (`.api/api-rhinocommon-geometry.md` — `HiddenLineDrawingParameters` `:149-158` incl. `AddGeometryAndPlanes`, `SetViewport`, `AddClippingPlane`, and the five flag members; `SilhouetteType` `[Flags]` roster `:81`), kernel `Domain/rails` (`Op`, `Op.ToHostSlot`, `ValidityClaim`, `IValidityEvidence`), kernel `Domain/validation` (`ICapability`, `CapabilitySet`), kernel `Domain/context` (`Context.Absolute`), kernel `Numerics/atoms` (`TransformSpec`, `Placement.Build` — the kernel transform builder, NOT the `Blocks/model.md` `Placement` block-instance union), `Rasm.Rhino.Document` (`GeometryHandle`), `Modeling/curves.md` (`ModelClaim`), Thinktecture.Runtime.Extensions, LanguageExt.Core.

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

    internal Fin<HiddenLineDrawingParameters> Rig(Context domain, Op key) {
        DrawingLaw law = this;
        return from clips in ProjectionOp.AdmittedClips(law.Clips, key)
               from parameters in key.Catch(() => {
                   HiddenLineDrawingParameters parameters = new() { AbsoluteTolerance = domain.Absolute.Value };
                   _ = toSeq(DrawingFeature.Items).Iter(feature =>
                       feature.Write(parameters: parameters, enabled: law.Features.Admits(capability: feature)));
                   foreach (Plane clip in clips) { parameters.AddClippingPlane(plane: clip); }
                   return Fin.Succ(value: parameters);
               })
               select parameters;
    }

    private static ValidityClaim Admits(DrawingRejoin? rejoin, ProjectionPacing pacing, Seq<Plane> clips) =>
        ValidityClaim.All(
            rejoin is not null,
            pacing.IsValid,
            ModelClaim.Rows(rows: clips, claim: static clip => (ValidityClaim)clip.IsValid, allowEmpty: true));
}
```

## [04]-[OPERATION_RAIL]

- Owner: `ProjectionSlot` `[SmartEnum<int>]` — the consequence vocabulary over host visibility classes, correspondence streams, and capture products, each visibility row carrying the host ordinal it answers; `ProjectionOp` `[Union]` `[GenerateUnionOps]` — the drawing, silhouette, and draft verbs, each case carrying its generated `SelfOp`; `Projections` — the one entry folding any operation spread into one `Built<ProjectionSlot>`.
- Law: products detach before the engine dies — every segment curve duplicates out of the disposable `HiddenLineDrawing` onto an owned handle inside the compute window, silhouette curves own directly because `Silhouette.Compute` returns fresh geometry, and the drawing, its `ViewportInfo`, and every mid-fold failure release symmetrically.
- Law: evidence forces before the engine dies too — `Seq` projection is lazy, so every harvest read over a `HiddenLineDrawingSegment`, `HiddenLineDrawingPoint`, or `Silhouette` closes with `Strict()` inside the `using` scope that owns the native; an unforced map escaping into `BuildReceipt` reads freed memory at the consumer's first enumeration, and the detached-product law alone does not cover it because evidence carries no handle to detach.
- Law: classification is a DECLARED correspondence — each visibility slot carries the host ordinal it answers, `Classify` is one frozen-index read, and an out-of-roster host value lands on the named `Unresolved` floor, so a new host visibility member is one column edit rather than a hand `switch` arm a reader must find.
- Law: the class harvest is one pass — the classified rows group once and each group folds its tally and its member ordinals together, so the receipt is not rebuilt by re-scanning the whole segment spread per distinct class.
- Law: correspondence maps survive — the subject ordinal registered at `AddGeometry` returns through `ParentCurve.SourceObject.Tag`, curve and point component indices land as `ComponentRows`, drawing points carry source and visibility streams beside their marks, and `WorldToHiddenLine` and the host-computed drawing bounds land as frame facts, so sheet placement composes evidence rather than re-projecting. Subject spread and borrow spread are equal-length by construction, so they zip rather than index each other.
- Law: clipping planes cross one admission fold — drawing-global, subject-selective, and outline clip sets traverse `AcceptInput` applicatively, and each arm hands the admitted sequence to the one host overload it composes; an empty admitted set reaches the host as an empty plane list, which the native reads exactly as the no-planes short form does.
- Law: a host capture that answered NOTHING lands no product and no fact — the silhouette spread rides `ModelFact.Answered`, so an absent capture and an empty one stay two readings instead of collapsing on a coalesced empty.
- Law: the capture altitudes stay split — `Drawing` is the occlusion-resolved scene, `Outline` is the per-object `Silhouette.Compute` capture whose kind mask and clip set cross as values, and `Draft` is `ComputeDraftCurve` under an explicit pull; a BSP, painter, per-sample occlusion march, or host round-trip re-derivation of any altitude is the kernel view page's enumerated dead form.
- Law: admission NAMES its axis and closes at compile — `Admitted` dispatches the generated `Switch` into the spine's `ModelClaim.Admits`, so a new verb breaks the compile instead of falling to a silent refusal and a request breaching several constraints reports each one.
- Boundary: geometric curve projection stays the curve rail — `CurveOp.Project` over `ProjectTarget` owns plane, brep, and mesh target projection with index maps; this rail begins where visibility classification does.
- Growth: a new drawing modality is one `ProjectionOp` case with its arm; a new frame source is one `ProjectionFrame` case every verb reads.
- Packages: RhinoCommon geometry (`.api/api-rhinocommon-geometry.md` — `HiddenLineDrawing.Compute`/`Segments`/`Points`/`WorldToHiddenLine`/`BoundingBox`/`RejoinCompatibleVisible` `:149-158`, `HiddenLineDrawingSegment.Visibility` roster `:78`, `Silhouette.Compute`/`ComputeDraftCurve` `:178-188`), kernel `Domain/rails` (`Op`, `[GenerateUnionOps]` + generated `SelfOp`, `ValidityClaim`, `Fin`), kernel `Numerics/atoms` (`Placement.Build` — the kernel transform builder, NOT the `Blocks/model.md` `Placement` block-instance union), `Rasm.Rhino.Document` (`GeometryHandle`), `Modeling/curves.md` (`ModelClaim`, `ModelFact`), `Modeling/solids.md` (`ModelGate`, `Built<TSlot>`, `BuildReceipt<TSlot>`, `BuildBody`, `SourceAxis`), LanguageExt.Core (`TraverseM`, `Traverse`, `Choose`, `Strict`, `Zip`), Thinktecture.Runtime.Extensions.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class ProjectionSlot {
    public static readonly ProjectionSlot Visible = new(key: 0, visibility: HiddenLineDrawingSegment.Visibility.Visible);
    public static readonly ProjectionSlot Hidden = new(key: 1, visibility: HiddenLineDrawingSegment.Visibility.Hidden);
    public static readonly ProjectionSlot Duplicate = new(key: 2, visibility: HiddenLineDrawingSegment.Visibility.Duplicate);
    public static readonly ProjectionSlot Projecting = new(key: 3, visibility: HiddenLineDrawingSegment.Visibility.Projecting);
    public static readonly ProjectionSlot Clipped = new(key: 4, visibility: HiddenLineDrawingSegment.Visibility.Clipped);
    public static readonly ProjectionSlot Unresolved = new(key: 5, visibility: HiddenLineDrawingSegment.Visibility.Unset);
    public static readonly ProjectionSlot Sourced = new(key: 6, visibility: None);
    public static readonly ProjectionSlot Kinds = new(key: 7, visibility: None);
    public static readonly ProjectionSlot Fills = new(key: 8, visibility: None);
    public static readonly ProjectionSlot Marks = new(key: 9, visibility: None);
    public static readonly ProjectionSlot Frame = new(key: 10, visibility: None);
    public static readonly ProjectionSlot Outline = new(key: 11, visibility: None);
    public static readonly ProjectionSlot Draft = new(key: 12, visibility: None);
    public static readonly ProjectionSlot Bounds = new(key: 13, visibility: None);

    internal Option<HiddenLineDrawingSegment.Visibility> Visibility { get; }

    internal static ProjectionSlot Classify(HiddenLineDrawingSegment.Visibility visibility) =>
        Optional(ByVisibility.Value.GetValueOrDefault(key: visibility)).IfNone(noneValue: Unresolved);

    private static readonly Lazy<FrozenDictionary<HiddenLineDrawingSegment.Visibility, ProjectionSlot>> ByVisibility =
        new(static () => toSeq(Items)
            .Choose(static row => row.Visibility.Map(visibility => (Visibility: visibility, Row: row)))
            .ToFrozenDictionary(static pair => pair.Visibility, static pair => pair.Row));
}

[GenerateUnionOps]
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

    internal Fin<ProjectionOp> Admitted(Op key) =>
        Switch(
            context: key,
            drawing: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Subjects), ModelClaim.Rows(rows: row.Subjects, claim: static subject => (ValidityClaim)subject.IsValid)),
                (nameof(row.Frame), row.Frame is { IsValid: true }),
                (nameof(row.Law), row.Law.IsValid)),
            outline: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Subject), ModelClaim.Handle(handle: row.Subject)),
                (nameof(row.Frame), row.Frame is { IsValid: true }),
                (nameof(row.Clips), ModelClaim.Rows(rows: row.Clips, claim: static clip => (ValidityClaim)clip.IsValid, allowEmpty: true))),
            draft: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Subject), ModelClaim.Handle(handle: row.Subject)),
                (nameof(row.Angle), ValidityClaim.Finite(value: row.Angle)),
                (nameof(row.Pull), ValidityClaim.Direction(value: row.Pull))));

    internal Fin<Built<ProjectionSlot>> Apply(Context domain) =>
        Switch(
            context: domain,
            drawing: static (model, edit) => {
                Op op = Drawing.SelfOp;
                DrawingLaw law = edit.Law;
                return ModelGate.BorrowMany<GeometryBase, Built<ProjectionSlot>>(
                    handles: edit.Subjects.Map(static subject => subject.Geometry), key: op,
                    body: natives =>
                        from parameters in law.Rig(domain: model, key: op)
                        from rigged in edit.Frame.Rig(key: op)
                        from harvested in op.Catch(() => {
                            using ViewportInfo frame = rigged;
                            parameters.SetViewport(frame);
                            return edit.Subjects.Zip(natives)
                                .Map(static (pair, ordinal) => (Subject: pair.First, Native: pair.Second, Ordinal: ordinal))
                                .TraverseM(row => Registered(
                                    parameters: parameters, subject: row.Subject, native: row.Native,
                                    ordinal: row.Ordinal, model: model, op: op)).As()
                                .Bind(_ => Computed(parameters: parameters, law: law, op: op));
                        }, token: law.Pacing.Cancel)
                        select harvested);
            },
            outline: static (model, edit) => {
                Op op = Outline.SelfOp;
                return from clips in AdmittedClips(edit.Clips, op)
                       from built in ModelGate.Borrow<GeometryBase, Built<ProjectionSlot>>(handle: edit.Subject, key: op, body: native =>
                    edit.Frame.Switch(
                        context: (Native: native, Model: model, Edit: edit, Clips: clips, Op: op),
                        eye: static (ctx, frame) => Captured(slot: ProjectionSlot.Outline, op: ctx.Op, token: ctx.Edit.Cancel, run: () =>
                            Silhouette.Compute(
                                ctx.Native, SilhouetteKind.Native(kinds: ctx.Edit.Kinds), frame.Value,
                                ctx.Model.Absolute.Value, ctx.Model.Angle.Value,
                                ctx.Clips.AsIterable(), ctx.Edit.Cancel)),
                        along: static (ctx, frame) => Captured(slot: ProjectionSlot.Outline, op: ctx.Op, token: ctx.Edit.Cancel, run: () =>
                            Silhouette.Compute(
                                ctx.Native, SilhouetteKind.Native(kinds: ctx.Edit.Kinds), frame.Value,
                                ctx.Model.Absolute.Value, ctx.Model.Angle.Value,
                                ctx.Clips.AsIterable(), ctx.Edit.Cancel)),
                        framed: static (ctx, frame) =>
                            from rigged in frame.Value.Rig(key: ctx.Op)
                            from built in ctx.Op.Catch(() => {
                                using ViewportInfo viewport = rigged;
                                return Captured(slot: ProjectionSlot.Outline, op: ctx.Op, token: ctx.Edit.Cancel, run: () =>
                                    Silhouette.Compute(
                                        ctx.Native, SilhouetteKind.Native(kinds: ctx.Edit.Kinds), viewport,
                                        ctx.Model.Absolute.Value, ctx.Model.Angle.Value,
                                        ctx.Clips.AsIterable(), ctx.Edit.Cancel));
                            })
                            select built))
                       select built;
            },
            draft: static (model, edit) => {
                Op op = Draft.SelfOp;
                return ModelGate.Borrow<GeometryBase, Built<ProjectionSlot>>(handle: edit.Subject, key: op, body: native =>
                    from pull in op.AcceptInput(value: edit.Pull)
                    from built in Captured(slot: ProjectionSlot.Draft, op: op, token: edit.Cancel, run: () =>
                        Silhouette.ComputeDraftCurve(
                            native, edit.Angle, pull, model.Absolute.Value, model.Angle.Value, edit.Cancel))
                    select built);
            });

    private static Fin<Unit> Registered(
        HiddenLineDrawingParameters parameters, ProjectionSubject subject, GeometryBase native,
        int ordinal, Context model, Op op) =>
        from motion in subject.Placement
            .Traverse(spec => Placement.Build(spec: spec, context: Some(model), key: op)).As()
        from clips in AdmittedClips(subject.Clips, op)
        from _ in op.Confirm(success: parameters.AddGeometryAndPlanes(
            native, motion.IfNone(noneValue: Transform.Identity), ordinal, subject.Occluding, [.. clips]))
        select unit;

    private static Fin<Built<ProjectionSlot>> Computed(HiddenLineDrawingParameters parameters, DrawingLaw law, Op op) {
        HiddenLineDrawing? computed = HiddenLineDrawing.Compute(
            parameters, law.Pacing.Threads.Native, Op.ToHostSlot(value: law.Pacing.Progress), law.Pacing.Cancel);
        return Optional(computed).ToFin(Fail: op.InvalidResult()).Bind(drawing => {
            using (drawing) {
                if (law.Rejoin.Native) { drawing.RejoinCompatibleVisible(); }
                Seq<HiddenLineDrawingSegment> segments = toSeq(drawing.Segments).Strict();
                Seq<HiddenLineDrawingPoint> points = toSeq(drawing.Points).Strict();
                BuildReceipt<ProjectionSlot> evidence = Harvested(
                    segments: segments,
                    points: points,
                    frame: drawing.WorldToHiddenLine,
                    bounds: drawing.BoundingBox(includeHidden: law.Features.Admits(capability: DrawingFeature.HiddenCurves)));
                return ModelGate.OwnMany(
                        built: segments.Map(static segment => (GeometryBase)segment.CurveGeometry.Duplicate()).Strict(),
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
        Seq<(ProjectionSlot Slot, int Ordinal, bool Outline)> classified = segments
            .Map(static (segment, ordinal) => (
                Slot: ProjectionSlot.Classify(visibility: segment.SegmentVisibility),
                Ordinal: ordinal,
                Outline: segment.IsSceneSilhouette))
            .Strict();
        BuildReceipt<ProjectionSlot> classes = toSeq(classified.GroupBy(static row => row.Slot))
            .Fold(BuildReceipt<ProjectionSlot>.Empty, static (receipt, group) => {
                Seq<int> members = toSeq(group).Map(static row => row.Ordinal).Strict();
                return receipt
                    + BuildReceipt<ProjectionSlot>.Of(slot: group.Key, body: new BuildBody.Tally(Count: members.Count))
                    + BuildReceipt<ProjectionSlot>.Of(slot: group.Key, body: new BuildBody.Components(Indices: members));
            });
        Seq<int> outlined = classified.Filter(static row => row.Outline).Map(static row => row.Ordinal).Strict();
        return classes
            + BuildReceipt<ProjectionSlot>.Of(slot: ProjectionSlot.Sourced, body: new BuildBody.SourceMap(
                Axis: SourceAxis.Subject,
                Rows: segments.Map(static segment => (int)segment.ParentCurve.SourceObject.Tag).Strict()))
            + BuildReceipt<ProjectionSlot>.Of(slot: ProjectionSlot.Sourced, body: new BuildBody.ComponentRows(
                Indices: segments.Map(static segment => segment.ParentCurve.SourceObjectComponentIndex).Strict()))
            + BuildReceipt<ProjectionSlot>.Of(slot: ProjectionSlot.Kinds, body: new BuildBody.SourceMap(
                Axis: SourceAxis.Kind,
                Rows: segments.Map(static segment => (int)segment.ParentCurve.SilhouetteType).Strict()))
            + BuildReceipt<ProjectionSlot>.Of(slot: ProjectionSlot.Fills, body: new BuildBody.SourceGroups(
                Axis: SourceAxis.Kind,
                Groups: segments.Map(static segment => toSeq(segment.CurveSideFills).Map(static fill => (int)fill).Strict()).Strict()))
            + BuildReceipt<ProjectionSlot>.Of(slot: ProjectionSlot.Outline, body: new BuildBody.Components(Indices: outlined))
            + BuildReceipt<ProjectionSlot>.Of(slot: ProjectionSlot.Frame, body: new BuildBody.Motion(Value: frame))
            + BuildReceipt<ProjectionSlot>.Of(slot: ProjectionSlot.Bounds, body: new BuildBody.Bounds(Value: bounds))
            + BuildReceipt<ProjectionSlot>.Of(slot: ProjectionSlot.Marks, body: new BuildBody.Marks(
                Points: points.Map(static point => point.Location).Strict()))
            + BuildReceipt<ProjectionSlot>.Of(slot: ProjectionSlot.Marks, body: new BuildBody.SourceMap(
                Axis: SourceAxis.Subject, Rows: points.Map(static point => (int)point.SourceObject.Tag).Strict()))
            + BuildReceipt<ProjectionSlot>.Of(slot: ProjectionSlot.Marks, body: new BuildBody.ComponentRows(
                Indices: points.Map(static point => point.SourceObjectComponentIndex).Strict()))
            + BuildReceipt<ProjectionSlot>.Of(slot: ProjectionSlot.Marks, body: new BuildBody.SourceMap(
                Axis: SourceAxis.Kind, Rows: points.Map(static point => (int)point.PointVisibility).Strict()));
    }

    internal static Fin<Seq<Plane>> AdmittedClips(Seq<Plane> clips, Op key) =>
        clips
            .Traverse(clip => key.AcceptInput(value: clip).ToValidation())
            .As()
            .ToFin();

    private static Fin<Built<ProjectionSlot>> Captured(
        ProjectionSlot slot, Op op, CancellationToken token, Func<Silhouette[]> run) =>
        op.Catch(() => ModelFact.Answered(channel: run()).Match(
            Some: captured => {
                Seq<(int Kind, ComponentIndex Component, Curve Curve)> rows = captured
                    .Choose(static outline => Optional(outline.Curve).Map(curve =>
                        (Kind: (int)outline.SilhouetteType, Component: outline.GeometryComponentIndex, Curve: curve)))
                    .Strict();
                return ModelGate.OwnMany(
                        built: rows.Map(static row => (GeometryBase)row.Curve), key: op, allowEmpty: true)
                    .Map(owned => Built<ProjectionSlot>.Of(operation: op,
                        Products: owned,
                        Evidence: BuildReceipt<ProjectionSlot>.Of(slot: slot, body: new BuildBody.Tally(Count: owned.Count))
                            + BuildReceipt<ProjectionSlot>.Of(slot: ProjectionSlot.Kinds, body: new BuildBody.SourceMap(
                                Axis: SourceAxis.Kind, Rows: rows.Map(static row => row.Kind)))
                            + BuildReceipt<ProjectionSlot>.Of(slot: slot, body: new BuildBody.ComponentRows(
                                Indices: rows.Map(static row => row.Component)))));
            },
            None: () => Fin.Fail<Built<ProjectionSlot>>(error: op.InvalidResult(detail: "the host capture answered nothing"))),
            token: token);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Projections {
    public static Fin<Built<ProjectionSlot>> Build(ModelRuntime runtime, params ReadOnlySpan<ProjectionOp> operations) =>
        ModelGate.Entry(
            runtime: runtime,
            operations: operations,
            admit: static (operation, key) => operation.Admitted(key: key),
            apply: static (operation, model) => operation.Apply(domain: model));
}
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
