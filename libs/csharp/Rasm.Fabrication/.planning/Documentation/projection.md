# [RASM_FABRICATION_PROJECTION]

`Hlr.Solve` is the host-local adapter from an admitted fabrication model, boolean source policy, view convention, drafting convention, and characteristic loci to one evidence-complete `ProjectionReceipt`. `Rasm.Drawing.View` retains visibility and silhouette computation, `Rasm.Drawing.Hatching` retains pattern fill so a hatch-rowed view carries its exactly-clipped `HatchResult` with no host round-trip, and `Rasm.Meshing.Arrangement` retains solid composition; APP composition receives `FabricationResult.HiddenLineResult` without a direct library dependency on a UI package.

`ViewKey` is the one view identity every roster row, hatch row, run, and anchor joins on, so a projection never string-compares its own keys. `ProjectionCharacteristic` carries its `FeatureFrameReceipt` whole, and `ProjectionAnchor` republishes that receipt's layout-free `FrameSymbolRow` stream beside the projected screen locus — a drafting consumer places a feature-control frame from the anchor alone, never by re-opening specification geometry or reconstructing symbol identity.

## [01]-[INDEX]

- [02]-[PROJECTION]: `ViewKey`, `ProjectionAngle`, source and view families, `ProjectionPolicy` admission, the boolean composition lineage, and the run, anchor, and receipt shapes `Hlr.Solve` mints.

## [02]-[PROJECTION]

- Owner: `ViewKey` owns view identity; `ProjectionAngle` owns the sheet-quadrant law; `ProjectionSource` owns projection ingress; `ProjectionView` owns the authored roster and `ViewOp` the solve-time kernel request; `ProjectionPolicy` owns one-shot admission; `BooleanComposition` owns operand lineage; `ProjectionRun`, `ProjectionAnchor`, and `ProjectionReceipt` own the settled evidence; `Hlr` owns the fold.
- Cases: `ProjectionSource.Model` forwards the admitted `FabricationInput.Model` and `.Boolean` folds identified operand rows through `Arrangement.Apply`; `ProjectionView.Projected` composes silhouette, hidden-line, or outline with an authored orientation and `.Section` adds its cut plane, so multiple cuts coexist under distinct keys without reminting a kernel operation vocabulary.
- Law: the operand ordinal is the position an operand took in its OWN boolean leg — the accumulated model enters at `0` and the operand at `1`, which is exactly what `ManifoldProvenance.OperandOf(face)` returns. A running count across legs makes every leg past the first attribute to nothing, because no leg ever sees an ordinal above one. Attribution therefore walks the legs in REVERSE: the last leg's provenance describes the final faces, an operand-`1` hit names that leg's source, and an operand-`0` hit falls through to the leg that produced the accumulated model.
- Law: every scalar knob is dimensioned or bounded at admission — `CreaseDihedralRadians` enters as an `Angle`, the intersection inflation as a `Length`, the spatial leaf as a positive count, and `BetaSquared` as the squared silhouette-tolerance ratio the kernel's own crease test consumes. A bare double angle beside a typed length is the fork this admission deletes.
- Law: an anchor is projected evidence, not a cross product. A characteristic anchors in a view only where its locus projects to a FINITE POSITIVE depth in that view's own camera, so a locus behind the camera or on its plane records no anchor rather than a screen point a sheet would place.
- Law: face-grain attribution reads the SEGMENT — `ProjectedSegment.SourceFace` carries the kernel's classifying face ordinal, which on this single-part solve IS the composed model's own face index, so `ProjectionReceipt.Attribute(segment.SourceFace)` resolves per-segment operand lineage with no side table; `SourceA`/`SourceB` stay source-edge vertex indices the chain walk links on, negative wherever a visibility split landed mid-edge, and a `-1` face (an inter-part seam or section segment) attributes to nothing, the honest answer.
- Entry: `Fabrication.Run` remains the sole public package entry; `Hlr.Solve` is internal, receives parameterized ingress and egress, and preserves every `ProjectedSegment` field of every requested view through the kernel `DrawingProjection` carrier.
- Auto: the policy is CONSUMER-AUTHORED per run — a drafting consumer raises its own `ProjectionView` rows from the camera basis it already holds and its `Ratio Scale` from its sheet scale — so no view or scale value originates inside this owner and admission validates whatever a consumer raises. Requested views enter one `Validation<Error>` traversal, so an unprojectable view reports beside every other failed view rather than masking them.
- Receipt: `ProjectionReceipt` retains one keyed `ProjectionRun` per requested view — its `ViewPose`, kernel operation, complete `DrawingProjection` including `EdgeKind`, `Invisibility`, `Next`, `SourceA`, `SourceB`, `Part`, `SourceFace`, the flat and per-part `EdgeHistogram` tallies, and the `Contacts` interference roster, and the `Option<HatchResult>` its hatch row produced — beside every boolean composition, the drafting convention, and the anchor stream carrying its symbol rows.
- Packages: `Rasm.Drawing` (`View.Apply`, `Hatching.Apply`, `ViewOp`, `ViewKind`, `ViewPolicy`, `ViewConvention`, `ViewPose`, `Camera`, `DrawingProjection`, `HatchOp`, `HatchPlan`, `HatchPolicy`, `HatchResult`), `Rasm.Meshing` (`Arrangement.Apply`, `ArrangementOp.MeshBoolean`, `ArrangementResult`, `MeshSpace`, `BooleanReceipt`, `ManifoldProvenance`), `Rasm.Spatial` (`BuildPolicy`, `IntersectPolicy`), `Rasm.Numerics` (`Direction.Of`), `Rasm.Fabrication.Spec` (`FeatureFrameReceipt`, `FrameSymbolRow`, `CharacteristicId`).
- Boundary: a boolean returns SHELLS, so a severing operand refuses typed mid-fold rather than silently framing one component of a disconnected model; silhouette, crease, and intersection loci are whole-model reads and projecting the first shell would drop geometry the operand legitimately produced.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------------------------------------------------------
using System.Linq;
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
using UnitsNet.Units;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Documentation;

// --- [TYPES] --------------------------------------------------------------------------------------------------------------------------------------
// One view identity for the roster, the hatch rows, the runs, and the anchors, so a join is an equality on an
// admitted owner rather than an ordinal string comparison every site has to spell identically.
[ValueObject<string>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
[ValidationError<FabricationFault>]
public readonly partial struct ViewKey {
    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref FabricationFault? validationError, ref string value) {
        value = value.Trim();
        if (!Witness.Keyed(value))
            validationError = new FabricationFault.PolicyInadmissible(FabConcern.Documentation, "view-key");
    }
}

// QuadrantSign is the placement law as data: a third-angle view lands on the side it looks from, a first-angle view
// on the opposite side, so sheet layout folds the column instead of branching. The fold is the APP drafting owner's —
// the same claimant that places this page's `FrameSymbolRow` compartments — because this package projects views and
// seats no sheet; publishing the sign is what keeps that owner from re-deriving the convention off the angle key.
[SmartEnum<string>]
public sealed partial class ProjectionAngle {
    public static readonly ProjectionAngle First = new("first", quadrantSign: -1);
    public static readonly ProjectionAngle Third = new("third", quadrantSign: 1);

    public int QuadrantSign { get; }
}

// --- [MODELS] -------------------------------------------------------------------------------------------------------------------------------------
[ComplexValueObject]
[ValidationError<FabricationFault>]
public sealed partial class ProjectionCharacteristic {
    public FeatureFrameReceipt Frame { get; }
    public Point3d ModelLocus { get; }
    public Option<(ContentKey Source, int Edge)> Provenance { get; }

    public CharacteristicId Id => Frame.Id;

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref FabricationFault? validationError,
        ref FeatureFrameReceipt frame,
        ref Point3d modelLocus,
        ref Option<(ContentKey Source, int Edge)> provenance) {
        if (!modelLocus.IsValid || provenance.Exists(static value => value.Edge < 0))
            validationError = new FabricationFault.PolicyInadmissible(FabConcern.Documentation, "projection:characteristic");
    }

    public static Fin<ProjectionCharacteristic> Admit(
        FeatureFrameReceipt frame,
        Point3d modelLocus,
        Option<(ContentKey Source, int Edge)> provenance) =>
        Validate(frame, modelLocus, provenance, out ProjectionCharacteristic characteristic).Admitted(characteristic);
}

// MeshSpace is a readonly record struct whose private factory alone assigns Native, so an
// un-admitted default carries a null Native — the probe every operand gate spells.
public readonly record struct BooleanOperand(MeshSpace Other, BooleanOp Operation, ContentKey Source);

// The kernel attributes to its OWN leg-local operand ordinals and knows nothing of content identity, so this row is
// the lineage carrier. Ordinal is fixed at one — the accumulated model is always operand zero — because a running
// count would exceed every ordinal the provenance can return and strand every leg past the first. The row carries no
// leg index: attribution walks the sequence in reverse, so position IS the leg and a stored copy of it is a second
// truth about the same order.
public readonly record struct BooleanComposition(ContentKey Source, BooleanReceipt Receipt) {
    public const int OperandSlot = 1;

    // The join reaches only the NATIVE route: `Receipt.Source` is populated off the engine's run windows and is
    // `None` on every managed fold under the arrangement's scale ceiling, so absence is the honest answer there.
    public Option<int> OperandOf(int face) => Receipt.Source.Bind(provenance => provenance.OperandOf(face));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ProjectionSource {
    private ProjectionSource() { }

    public sealed record Model : ProjectionSource;
    public sealed record Boolean(Seq<BooleanOperand> Operands, ArrangementPolicy Policy) : ProjectionSource;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ProjectionView {
    private ProjectionView() { }

    public sealed record Projected(ViewKey Key, ProjectionDir Direction, ViewKind Operation) : ProjectionView;
    public sealed record Section(ViewKey Key, ProjectionDir Direction, Plane Cut) : ProjectionView;

    public ViewKind Kind => Switch(
        projected: static value => value.Operation,
        section: static _ => ViewKind.Section);

    public ViewKey Key => Switch(
        projected: static value => value.Key,
        section: static value => value.Key);

    public ProjectionDir Direction => Switch(
        projected: static value => value.Direction,
        section: static value => value.Direction);

    // A projected row carrying the section kind has no cut plane to lower, and admission already refuses it; the
    // arm answers a typed refusal so no plane is forged for a case the roster cannot hold.
    // The composed solid enters as a roster of one — the kernel's union solve is the same walk either way,
    // and a future multi-part traveler view is roster data here, never a second lowering.
    internal Fin<ViewOp> Lower(MeshSpace model, Camera camera, ViewPolicy policy) => Switch(
        state: (Parts: Seq(ViewSubject.Of(model)), Camera: camera, Policy: policy),
        projected: static (state, view) => view.Operation.Switch(
            silhouette: _ => Fin.Succ<ViewOp>(new ViewOp.Silhouette(state.Parts, state.Camera, state.Policy)),
            hiddenLine: _ => Fin.Succ<ViewOp>(new ViewOp.HiddenLine(state.Parts, state.Camera, state.Policy)),
            outline: _ => Fin.Succ<ViewOp>(new ViewOp.Outline(state.Parts, state.Camera, state.Policy)),
            section: _ => Fin.Fail<ViewOp>(new FabricationFault.PolicyInadmissible(
                FabConcern.Documentation, $"projection:section-without-plane:{view.Key.Value}"))),
        section: static (state, view) => Fin.Succ<ViewOp>(
            new ViewOp.Section(state.Parts, view.Cut, state.Camera, state.Policy)));
}

[ComplexValueObject]
[ValidationError<FabricationFault>]
public sealed partial class ProjectionPolicy {
    public ProjectionSource Source { get; }
    public Seq<ProjectionView> Views { get; }
    public ViewConvention Convention { get; }
    public ProjectionAngle Angle { get; }
    public Ratio Scale { get; }
    public Length FacetTolerance { get; }

    // Crease is an ANGLE and the silhouette tolerance a squared ratio the kernel's own crease test consumes; both
    // enter typed, so a caller cannot hand degrees where radians are read or a raw ratio where its square is.
    public Angle CreaseDihedral { get; }
    public double BetaSquared { get; }

    public int SpatialLeaf { get; }
    public Map<ViewKey, HatchPlan> Hatching { get; }
    public Seq<ProjectionCharacteristic> Characteristics { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref FabricationFault? validationError,
        ref ProjectionSource source,
        ref Seq<ProjectionView> views,
        ref ViewConvention convention,
        ref ProjectionAngle angle,
        ref Ratio scale,
        ref Length facetTolerance,
        ref Angle creaseDihedral,
        ref double betaSquared,
        ref int spatialLeaf,
        ref Map<ViewKey, HatchPlan> hatching,
        ref Seq<ProjectionCharacteristic> characteristics) {
        Set<ViewKey> roster = toSet(views.Map(static value => value.Key));
        if (!Witness.Positive(facetTolerance.Millimeters)
            || !Witness.Positive(scale.DecimalFractions)
            || !double.IsFinite(creaseDihedral.Radians) || creaseDihedral.Radians is <= 0.0 or >= Math.PI
            || !Witness.Positive(betaSquared) || spatialLeaf <= 0
            || !source.Switch(
                model: static _ => true,
                boolean: static value => !value.Operands.IsEmpty
                    && value.Operands.ForAll(static operand => operand.Other.Native is not null))
            || views.IsEmpty
            || roster.Count != views.Count
            || views.Exists(static value => !value.Direction.Forward.IsValid)
            || views.Exists(static value => value is ProjectionView.Projected { Operation: var kind }
                && kind == ViewKind.Section)
            || views.Exists(static value => value is ProjectionView.Section { Cut: var cut } && !cut.IsValid)
            || hatching.Exists(row => !roster.Contains(row.Key) || !row.Value.IsValid)
            || characteristics.Map(static value => value.Id).Distinct().Count != characteristics.Count)
            validationError = new FabricationFault.PolicyInadmissible(FabConcern.Documentation, "projection:policy");
    }

    public static Fin<ProjectionPolicy> Admit(
        ProjectionSource source,
        Seq<ProjectionView> views,
        ViewConvention convention,
        ProjectionAngle angle,
        Ratio scale,
        Length facetTolerance,
        Angle creaseDihedral,
        double betaSquared,
        int spatialLeaf,
        Map<ViewKey, HatchPlan> hatching,
        Seq<ProjectionCharacteristic> characteristics) =>
        Validate(source, views, convention, angle, scale, facetTolerance, creaseDihedral, betaSquared, spatialLeaf,
            hatching, characteristics, out ProjectionPolicy policy).Admitted(policy);
}

// The anchor republishes the frame's own LAYOUT-FREE symbol rows beside the screen locus, so a drafting consumer
// places a feature-control frame from this row alone and never re-opens specification geometry to rebuild it.
public sealed record ProjectionAnchor(
    ViewKey View,
    ProjectionCharacteristic Characteristic,
    Point3d ScreenLocus,
    double Depth) {
    public Seq<FrameSymbolRow> Symbols => Characteristic.Frame.Annotation;
}

public sealed record ProjectionRun(
    ViewKey Key,
    ViewPose Pose,
    ViewKind Operation,
    DrawingProjection Projection,
    Option<HatchResult> Hatch = default);

public sealed record ProjectionReceipt(
    ProjectionAngle Angle,
    Ratio Scale,
    Seq<ProjectionRun> Runs,
    Seq<ProjectionAnchor> Characteristics,
    Seq<BooleanComposition> Composition,
    Seq<ContentKey> Sources) {
    public Option<DrawingProjection> View(ViewKey key) =>
        Runs.Find(run => run.Key == key).Map(static run => run.Projection);

    // Attribution walks the legs in REVERSE because the last boolean's provenance describes the final faces: an
    // operand-one hit names that leg's own source, an operand-zero hit falls through to the leg that produced the
    // accumulated model, and exhaustion is the honest answer for a managed run or a face the fold generated.
    public Option<ContentKey> Attribute(int face) => Composition
        .Rev()
        .Fold(
            (Resolved: Option<ContentKey>.None, Seeking: true),
            (state, row) => !state.Seeking
                ? state
                : row.OperandOf(face).Match(
                    Some: operand => operand == BooleanComposition.OperandSlot
                        ? (Some(row.Source), false)
                        : (state.Resolved, true),
                    None: () => (state.Resolved, false)))
        .Resolved;
}

// --- [OPERATIONS] ---------------------------------------------------------------------------------------------------------------------------------
internal static class Hlr {
    private static readonly Op HlrOp = Op.Of(name: "fabrication:hidden-line");

    private readonly record struct Sourced(MeshSpace Model, Seq<BooleanComposition> Composition);

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
            framed.Bind(value => Anchors(value.Run.Key, value.Camera, request.Policy.Characteristics)),
            sourced.Composition,
            toSeq((input.Sources + sourced.Composition.Map(static value => value.Source))
                .Distinct()
                .OrderBy(static value => value.Kind.Key)
                .ThenBy(static value => value.Digest))));

    // A locus behind the camera or on its plane has no place on a sheet, so the depth gate is what decides whether
    // a characteristic anchors in THIS view rather than a cross product that places every callout everywhere.
    private static Seq<ProjectionAnchor> Anchors(
        ViewKey key,
        Camera camera,
        Seq<ProjectionCharacteristic> characteristics) =>
        characteristics.Bind(characteristic => {
            double depth = camera.Depth(characteristic.ModelLocus);
            return Witness.Positive(depth)
                ? Seq(new ProjectionAnchor(key, characteristic, camera.Project(characteristic.ModelLocus), depth))
                : Seq<ProjectionAnchor>();
        });

    private static Fin<Sourced> Source(MeshSpace model, ProjectionSource source) => source.Switch(
        state: model,
        model: static (state, _) => Fin.Succ(new Sourced(state, Seq<BooleanComposition>())),
        // `FoldM` returns the erased `K<M, S>`; the carrier is named at the call and re-anchored by `.As()`,
        // so the arm answers the `Fin<Sourced>` its sibling arm does.
        boolean: static (state, request) => request.Operands.FoldM<Fin, Sourced>(
            new Sourced(state, Seq<BooleanComposition>()),
            (current, operand) =>
                from result in Arrangement.Apply(
                    new ArrangementOp.MeshBoolean(current.Model, operand.Other, operand.Operation, request.Policy),
                    HlrOp)
                from kept in result.Switch(
                    boolean: static value => Fin.Succ(value),
                    overlay: static _ => Fin.Fail<ArrangementResult.Boolean>(HlrOp.InvalidResult()),
                    complex: static _ => Fin.Fail<ArrangementResult.Boolean>(HlrOp.InvalidResult()))
                from solid in kept.Shells.Count == 1
                    ? kept.Shells.Head.ToFin(HlrOp.InvalidResult())
                    : Fin.Fail<MeshSpace>(new GeometryFault.DegenerateInput(
                        Kind.Mesh, None, "projection:severed-operand").ToError())
                select new Sourced(
                    solid,
                    current.Composition.Add(new BooleanComposition(operand.Source, kept.Receipt)))).As());

    private static Fin<(ViewPose Pose, Camera Camera, ViewPolicy View)> Lower(
        MeshSpace model,
        ProjectionDir direction,
        ProjectionPolicy policy) =>
        // Fast bounds are exact enough for camera framing: Pose reads only Center and Diagonal to
        // seat the standoff, so an accurate walk buys no framing precision at a full-mesh cost.
        from bounds in Try.lift<BoundingBox>(() => model.Native.GetBoundingBox(accurate: false))
            .Run()
            .MapFail(static error => new GeometryFault.DegenerateInput(Kind.Mesh, None, error.Message).ToError())
        from _ in guard(bounds.IsValid, HlrOp.InvalidInput()).ToFin()
        from forward in Direction.Of(direction.Forward, model.Tolerance, HlrOp)
        from pose in policy.Convention.Pose(bounds, Some(forward), model.Tolerance, HlrOp)
        from camera in pose.ToCamera(model.Tolerance, HlrOp)
        select (
            pose,
            camera,
            ViewPolicy.Canonical with {
                CreaseDihedralRadians = policy.CreaseDihedral.As(AngleUnit.Radian),
                BetaSquared = policy.BetaSquared,
                Narrow = IntersectPolicy.Canonical with { BroadPhaseInflation = policy.FacetTolerance.Millimeters },
                Broad = BuildPolicy.Canonical with { LeafSize = policy.SpatialLeaf },
            });

    private static K<Validation<Error>, (ProjectionRun Run, Camera Camera)> ProjectionLeg(
        ProjectionView view,
        MeshSpace model,
        ProjectionPolicy policy) =>
        (from lowered in Lower(model, view.Direction, policy)
         from operation in view.Lower(model, lowered.Camera, lowered.View)
         from projection in View.Apply(operation, HlrOp)
         from hatch in policy.Hatching
             .Find(view.Key)
             .Match(
                 Some: plan => Hatching
                     .Apply(new HatchOp.Projection(projection, plan, HatchLane(policy)), HlrOp)
                     .Map(static wire => Optional(wire)),
                 None: static () => Fin.Succ(Option<HatchResult>.None))
         select (new ProjectionRun(view.Key, lowered.Pose, view.Kind, projection, hatch), lowered.Camera))
        .ToValidation();

    // Hatch lanes ride the SAME admitted exactness rows Lower binds for the view solve.
    private static HatchPolicy HatchLane(ProjectionPolicy policy) =>
        HatchPolicy.Canonical with {
            Narrow = IntersectPolicy.Canonical with { BroadPhaseInflation = policy.FacetTolerance.Millimeters },
            Broad = BuildPolicy.Canonical with { LeafSize = policy.SpatialLeaf },
        };
}
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
