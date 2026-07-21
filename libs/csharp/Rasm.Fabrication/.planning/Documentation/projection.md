# [RASM_FABRICATION_PROJECTION]

`projection.md` owns the host-local adapter from an admitted fabrication model, boolean source policy, view convention, drafting convention, and characteristic loci to one evidence-complete `ProjectionReceipt`. `Rasm.Drawing.View` retains visibility and silhouette computation; `Rasm.Meshing.Arrangement` retains solid composition; APP composition receives `FabricationResult.HiddenLineResult` without creating a direct library dependency on a UI package.

`Fabrication.Run` remains the sole public package entry. `Hlr.Solve` is internal, receives parameterized ingress and egress, and preserves every `ProjectedSegment` field of every requested view through the kernel `DrawingProjection` carrier.

## [01]-[INDEX]

- [01]-[PROJECTION]: `ProjectionPolicy` admits source, view roster, drafting convention, exactness, spatial, and characteristic policy once; `ProjectionSource` generates model and boolean modalities; `ProjectionView` carries authored view identity, direction, and kernel operation; `ProjectionReceipt` carries per-view runs, poses, sheet convention, anchors, composition, and source identity; `Hlr.Solve` lowers the policy through one flat `Fin` rail over an accumulating view traversal.

## [02]-[PROJECTION]

`ProjectionPolicy` is the canonical admission owner. `Length FacetTolerance`, `CreaseDihedralRadians`, `BetaSquared`, `SpatialLeaf`, and `Ratio Scale` validate before any kernel operation; `ProjectionSource.Boolean` requires at least one identified operand; `ProjectionView` rows admit one run per authored key with an independent direction and a valid cut plane on every section; `ProjectionCharacteristic` binds each `FeatureFrameReceipt` to its model locus and optional source-edge provenance. `FabricationPolicy.HiddenLine` carries one `ProjectionPolicy`, so raw scalars and optional boolean rows never enter the operation late.

`ProjectionView` is the author-time view roster and `ViewOp` the solve-time kernel request; the two stay distinct on payload timing, because a policy-authored key and direction carry neither the `Camera` the pose derives nor the `MeshSpace` the boolean fold produces. `Projected` composes silhouette, hidden-line, or outline operation with an authored orientation; `Section` adds its cut plane, so multiple cuts coexist under distinct keys without reminting a kernel operation vocabulary.

`ProjectionSource` collapses projection ingress into one generated case family. `Model` forwards the admitted `FabricationInput.Model`; `Boolean` folds identified `BooleanOperand` rows through `Arrangement.Apply` and accepts only `ArrangementResult.Boolean`. `BooleanComposition` pairs each operand `ContentKey` with its `BooleanReceipt` and survives the fold into `ProjectionReceipt`, so source lineage covers every solid that contributes geometry. Every operand shares one `ArrangementPolicy`, so arity grows as data and never creates another solver or source wrapper.

`ViewConvention.Pose` and `ViewPose.ToCamera` derive each view's camera from its own `ProjectionDir.Forward`, model bounds, and `Context`; anchors project once per authored view and retain that view key. `ViewPolicy` receives the admitted crease, winding, intersection-inflation, and leaf values. Requested views enter one `Validation<Error>` traversal, so an unprojectable view reports beside every other failed view rather than masking them.

`ProjectionReceipt` retains one keyed `ProjectionRun` per requested view — each carrying its `ViewPose`, kernel operation, and complete `DrawingProjection` including `EdgeKind`, `Invisibility`, `Next`, `SourceA`, `SourceB`, and `EdgeHistogram` — plus every boolean-composition receipt. `ProjectionAngle` and `Ratio Scale` carry the drafting convention the sheet consumer places against, so first-angle and third-angle layouts derive from receipt-carried poses. `ProjectionAnchor` projects every characteristic locus through its run's camera and records the run key and depth beside the admitted `ProjectionCharacteristic`; drawing layout places characteristic callouts without re-opening model geometry or reconstructing specification identity.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------------------------------------------------------
using System;
using LanguageExt;
using LanguageExt.Common;
using LanguageExt.Traits;
using Rasm.Domain;
using Rasm.Drawing;
using Rasm.Fabrication.Process;
using Rasm.Fabrication.Spec;
using Rasm.Meshing;
using Rasm.Numerics;
using Rasm.Spatial;
using Rhino.Geometry;
using Thinktecture;
using UnitsNet;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Documentation;

// --- [TYPES] --------------------------------------------------------------------------------------------------------------------------------------
// QuadrantSign is the placement law as data: a third-angle view lands on the side it looks from,
// a first-angle view on the opposite side, so sheet layout folds the column instead of branching.
[SmartEnum<string>]
public sealed partial class ProjectionAngle {
    public static readonly ProjectionAngle First = new("first", quadrantSign: -1);
    public static readonly ProjectionAngle Third = new("third", quadrantSign: 1);

    public int QuadrantSign { get; }
}

// --- [MODELS] -------------------------------------------------------------------------------------------------------------------------------------
[ComplexValueObject]
public sealed partial class ProjectionCharacteristic {
    public FeatureFrameReceipt Frame { get; }
    public CharacteristicId Id => Frame.Id;
    public Point3d ModelLocus { get; }
    public Option<(ContentKey Source, int Edge)> Provenance { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref FeatureFrameReceipt frame,
        ref Point3d modelLocus,
        ref Option<(ContentKey Source, int Edge)> provenance) {
        if (frame is null || !modelLocus.IsValid
            || provenance.Exists(static value => value.Source is null || value.Edge < 0))
            validationError = new ValidationError("projection:characteristic");
    }
}

// MeshSpace is a readonly record struct whose private factory alone assigns Native, so an
// un-admitted default carries a null Native — the probe every operand gate spells.
public readonly record struct BooleanOperand(MeshSpace Other, BooleanOp Operation, ContentKey Source);

public readonly record struct BooleanComposition(ContentKey Source, BooleanReceipt Receipt);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ProjectionSource {
    private ProjectionSource() { }

    public sealed record Model : ProjectionSource;
    public sealed record Boolean(Seq<BooleanOperand> Operands, ArrangementPolicy Policy) : ProjectionSource;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ProjectionView {
    private ProjectionView() { }

    public sealed record Projected(string Key, ProjectionDir Direction, ViewKind Operation) : ProjectionView;
    public sealed record Section(string Key, ProjectionDir Direction, Plane Cut) : ProjectionView;

    public ViewKind Kind => Switch(
        projected: static value => value.Operation,
        section:    static _ => ViewKind.Section);

    public string Key => Switch(
        projected: static value => value.Key,
        section: static value => value.Key);

    public ProjectionDir Direction => Switch(
        projected: static value => value.Direction,
        section: static value => value.Direction);

    internal ViewOp Lower(MeshSpace model, Camera camera, ViewPolicy policy) => Switch(
        state: (Model: model, Camera: camera, Policy: policy),
        projected: static (state, view) => view.Operation.Switch<ViewOp>(
            silhouette: _ => new ViewOp.Silhouette(state.Model, state.Camera, state.Policy),
            hiddenLine: _ => new ViewOp.HiddenLine(state.Model, state.Camera, state.Policy),
            outline: _ => new ViewOp.Outline(state.Model, state.Camera, state.Policy),
            section: _ => new ViewOp.Section(state.Model, Plane.Unset, state.Camera, state.Policy)),
        section:    static (state, view) => new ViewOp.Section(state.Model, view.Cut, state.Camera, state.Policy));
}

[ComplexValueObject]
public sealed partial class ProjectionPolicy {
    public ProjectionSource Source { get; }
    public Seq<ProjectionView> Views { get; }
    public ViewConvention Convention { get; }
    public ProjectionAngle Angle { get; }
    public Ratio Scale { get; }
    public Length FacetTolerance { get; }
    public double CreaseDihedralRadians { get; }
    public double BetaSquared { get; }
    public int SpatialLeaf { get; }
    public Seq<ProjectionCharacteristic> Characteristics { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref ProjectionSource source,
        ref Seq<ProjectionView> views,
        ref ViewConvention convention,
        ref ProjectionAngle angle,
        ref Ratio scale,
        ref Length facetTolerance,
        ref double creaseDihedralRadians,
        ref double betaSquared,
        ref int spatialLeaf,
        ref Seq<ProjectionCharacteristic> characteristics) {
        bool scalarsValid = facetTolerance > Length.Zero
            && double.IsFinite(scale.DecimalFractions)
            && scale.DecimalFractions > 0.0
            && double.IsFinite(creaseDihedralRadians)
            && creaseDihedralRadians > 0.0
            && creaseDihedralRadians < Math.PI
            && double.IsFinite(betaSquared)
            && betaSquared > 0.0
            && spatialLeaf > 0;
        bool sourceValid = source is not null && source.Switch(
            model: static _ => true,
            boolean: static value => value.Policy is not null
                && !value.Operands.IsEmpty
                && value.Operands.ForAll(static operand => operand.Other.Native is not null
                    && operand.Operation is not null
                    && operand.Source is not null));
        bool viewsValid = !views.IsEmpty
            && views.ForAll(static value => value is not null && value.Switch(
                projected: static view => !string.IsNullOrWhiteSpace(view.Key)
                    && view.Direction.Forward.IsValid && view.Operation is not null && view.Operation != ViewKind.Section,
                section: static view => !string.IsNullOrWhiteSpace(view.Key)
                    && view.Direction.Forward.IsValid && view.Cut.IsValid))
            && views.Map(static value => value.Key).Distinct().Count == views.Count;
        bool characteristicsValid = characteristics.ForAll(static value => value is not null);
        bool anchorsUnique = characteristicsValid && characteristics
            .Map(static value => value.Id)
            .Distinct()
            .Count == characteristics.Count;
        if (!scalarsValid || !sourceValid || !viewsValid || convention is null || angle is null
            || !characteristicsValid || !anchorsUnique)
            validationError = new ValidationError("projection:policy");
    }
}

public sealed record ProjectionAnchor(string View, ProjectionCharacteristic Characteristic, Point3d ScreenLocus, double Depth);

public sealed record ProjectionRun(string Key, ViewPose Pose, ViewKind Operation, DrawingProjection Projection);

public sealed record ProjectionReceipt(
    ProjectionAngle Angle,
    Ratio Scale,
    Seq<ProjectionRun> Runs,
    Seq<ProjectionAnchor> Characteristics,
    Seq<BooleanComposition> Composition,
    Seq<ContentKey> Sources) {
    public Option<DrawingProjection> View(string key) =>
        Runs.Find(run => string.Equals(run.Key, key, StringComparison.Ordinal)).Map(static run => run.Projection);
}

// --- [OPERATIONS] ---------------------------------------------------------------------------------------------------------------------------------
internal static class Hlr {
    static readonly Op HlrOp = Op.Of(name: "fabrication:hidden-line");

    readonly record struct Sourced(MeshSpace Model, Seq<BooleanComposition> Composition);

    internal static Fin<FabricationResult> Solve(
        FabricationPolicy.HiddenLine request,
        FabricationInput input,
        Func<ProjectionReceipt, FabricationResult> egress) =>
        from admitted in input.Model.ToFin(HlrOp.InvalidInput())
        from sourced in Source(admitted, request.Policy.Source)
        from framed in request.Policy.Views
            .Traverse(view => ProjectionLeg(view, sourced.Model, request.Policy))
            .As()
            .ToFin()
        select egress(new ProjectionReceipt(
            request.Policy.Angle,
            request.Policy.Scale,
            framed.Map(static value => value.Run),
            framed.Bind(value => request.Policy.Characteristics.Map(characteristic => new ProjectionAnchor(
                value.Run.Key,
                characteristic,
                value.Camera.Project(characteristic.ModelLocus),
                value.Camera.Depth(characteristic.ModelLocus)))),
            sourced.Composition,
            (input.Sources + sourced.Composition.Map(static value => value.Source))
                .Distinct()
                .OrderBy(static value => value.Kind.Key)
                .ThenBy(static value => value.Digest)
                .ToSeq()));

    static Fin<Sourced> Source(MeshSpace model, ProjectionSource source) =>
        source.Switch(
            state: model,
            model: static (state, _) => Fin.Succ(new Sourced(state, Seq<BooleanComposition>())),
            boolean: static (state, request) => request.Operands.FoldM(
                new Sourced(state, Seq<BooleanComposition>()),
                (current, operand) =>
                    from result in Arrangement.Apply(
                        new ArrangementOp.MeshBoolean(current.Model, operand.Other, operand.Operation, request.Policy),
                        HlrOp)
                    from kept in result.Switch(
                        boolean: static value => Fin.Succ(value),
                        overlay: static _ => Fin.Fail<ArrangementResult.Boolean>(HlrOp.InvalidResult()),
                        complex: static _ => Fin.Fail<ArrangementResult.Boolean>(HlrOp.InvalidResult()))
                    select new Sourced(
                        kept.Solid,
                        current.Composition.Add(new BooleanComposition(operand.Source, kept.Receipt)))));

    static Fin<(ViewPose Pose, Camera Camera, ViewPolicy View)> Lower(
        MeshSpace model,
        ProjectionDir direction,
        ProjectionPolicy policy) =>
        // Fast bounds are exact enough for camera framing: Pose reads only Center and Diagonal to
        // seat the standoff, so an accurate walk buys no framing precision at a full-mesh cost.
        from bounds in Try.lift<BoundingBox>(() => model.Native.GetBoundingBox(accurate: false))
            .Run()
            .MapFail(static error => new GeometryFault.DegenerateInput(Kind.Mesh, -1, error.Message).ToError())
        from _ in guard(bounds.IsValid, HlrOp.InvalidInput()).ToFin()
        from forward in Direction.Of(direction.Forward, model.Tolerance, HlrOp)
        from pose in policy.Convention.Pose(bounds, Some(forward), model.Tolerance, HlrOp)
        from camera in pose.ToCamera(model.Tolerance, HlrOp)
        select (
            pose,
            camera,
            ViewPolicy.Canonical with {
                CreaseDihedralRadians = policy.CreaseDihedralRadians,
                BetaSquared = policy.BetaSquared,
                Narrow = IntersectPolicy.Canonical with { BroadPhaseInflation = policy.FacetTolerance.Millimeters },
                Broad = BuildPolicy.Canonical with { LeafSize = policy.SpatialLeaf },
            });

    static K<Validation<Error>, (ProjectionRun Run, Camera Camera)> ProjectionLeg(
        ProjectionView view,
        MeshSpace model,
        ProjectionPolicy policy) =>
        (from lowered in Lower(model, view.Direction, policy)
         from projection in View.Apply(view.Lower(model, lowered.Camera, lowered.View), HlrOp)
         select (new ProjectionRun(view.Key, lowered.Pose, view.Kind, projection), lowered.Camera))
        .ToValidation();
}
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
