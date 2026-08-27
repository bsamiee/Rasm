# [RASM_FABRICATION_PROJECTION]

`Hlr.Solve` is the host-local adapter from an admitted fabrication model, boolean source policy, view convention, drafting convention, and characteristic loci to one evidence-complete `ProjectionEvidence`. `Rasm.Drawing.View` retains visibility and silhouette computation, `Rasm.Drawing.Hatching` retains pattern fill so a hatch-rowed view carries its exactly-clipped `HatchResult` with no host round-trip, and `Rasm.Meshing.Arrangement` retains solid composition; APP composition receives `FabricationResult.HiddenLineResult` without a direct library dependency on a UI package.

`ViewKey` is the one view identity every roster row, hatch row, run, and anchor joins on, so a projection never string-compares its own keys. `ProjectionCharacteristic` carries its `FeatureFrame` whole, and `ProjectionAnchor` republishes that frame's layout-free `FrameSymbolRow` stream beside the projected screen locus — a drafting consumer places a feature-control frame from the anchor alone, never by re-opening specification geometry or reconstructing symbol identity. That evidence republishes one `BalloonAnchor` per part per view, so a parts-list leader seats from it alone.

## [01]-[INDEX]

- [02]-[PROJECTION]: `ViewKey`, source and view families, `ProjectionPolicy` admission over the issued sheet, the boolean composition lineage, and the run, anchor, and result shapes `Hlr.Solve` mints.

## [02]-[PROJECTION]

- Owner: `ViewKey` owns view identity; `ProjectionSource` owns projection ingress; `ProjectionView` owns the authored roster and `ViewOp` the solve-time kernel request; `ProjectionPolicy` owns one-shot admission; `BooleanComposition` owns operand lineage; `ProjectionRun`, `ProjectionAnchor`, `BalloonAnchor`, and `ProjectionEvidence` own the settled evidence; `Hlr` owns the fold.
- Cases: `ProjectionSource.Model` forwards the admitted `FabricationInput.Model` and `.Boolean` folds identified operand rows through `Arrangement.Apply`; `ProjectionView.Projected` composes silhouette, hidden-line, or outline with an authored orientation and `.Section` adds its cut plane, so multiple cuts coexist under distinct keys without reminting a kernel operation vocabulary.
- Law: the operand ordinal is the position an operand took in its OWN boolean leg — the accumulated model enters at `0` and the operand at `1`, which is exactly what `BooleanCensus.ManifoldEvidence.OperandOf(face)` returns. A running count across legs makes every leg past the first attribute to nothing, because no leg ever sees an ordinal above one. Attribution therefore walks the legs in REVERSE: the last leg's provenance describes the final faces, an operand-`1` hit names that leg's source, and an operand-`0` hit falls through to the leg that produced the accumulated model.
- Law: every scalar knob is dimensioned or bounded at admission — `CreaseDihedralRadians` enters as an `Angle`, the spatial leaf as a positive count, and `BetaSquared` as the squared silhouette-tolerance ratio the kernel's own crease test consumes. A bare double angle beside a typed length is the fork this admission deletes.
- Law: the SHEET facts are one column. `PlotPolicy` carries the size, its standard, the ladder-admitted `DrawingScale`, the frame, the line group, the plot style table, and the PDF conformance in one privately-minted value, so the result's `Quadrant` DERIVES off it through `ProjectionAngle.For(standard)` — the kernel's own ISO 128-30 §5 row — and `ScaleLadder.For(standard).Admits(scale)` already ran inside the mint. Scale is read as `Plot.Scale`; an aliasing property beside it would be a rename wrapper over one hop. A local two-row angle table and a free positive `Ratio` were the deleted form: the ratio admitted `1:7.3`, and the local table keyed `first`/`third` where the kernel keys `first-angle`/`third-angle`.
- Law: broad-phase inflation is the OPERAND's, not the policy's. `IntersectPolicy` carries no inflation column and the sweep band reads `Context.For(ToleranceLane.MeshIntersection)` off the operand's own bound context, so the band scales with the model and a caller wanting a wider sweep widens that context — one authority for the tolerance rather than a per-view scalar every hatch and view leg re-spelled.
- Law: an anchor is projected evidence, not a cross product. A characteristic anchors in a view only where its locus projects to a FINITE POSITIVE depth in that view's own camera, so a locus behind the camera or on its plane records no anchor rather than a screen point a sheet would place.
- Law: a balloon anchors at the ARC-LENGTH MIDPOINT of its part's LONGEST visible chain. ISO 6433 §4.4 seats a part reference outside the outlines of the part it names and connects it by a leader terminating against that outline, so the outline stretch a leader wants is the longest one the view draws — the run a neighbouring part's linework crowds least, and the one whose midpoint holds the part at every drawing scale. Chain partition COMPOSES `SuccessorChain.Walk` over the run's own visible set: `ProjectedSegment.Next` indexes within that set, so a per-part filter ahead of the walk strands every ordinal, and a page-local cursor loop beside the kernel's one walk is the deleted twin. Each chain keys on its head's `Part` — a linked run is one source edge's own — length is summed screen distance, ties fall to the lowest first-segment ordinal, and the depth cue INTERPOLATES between the bracketing segment's own endpoint pair rather than reporting an endpoint's.
- Law: a part with no visible edge is UNCALLED, and the verdict is the histogram's — `DrawingProjection.Parts[part].VisibleCount` is the per-part tally the kernel already published, so the fold reads it rather than recounting, and a wholly occluded part records no anchor instead of a leader landing on a neighbour's outline. Chain heads carrying no `Part` — inter-part boundaries and section rows — anchor nothing for the same reason.
- Law: face-grain attribution reads the SEGMENT — `ProjectedSegment.SourceFace` carries the kernel's classifying face ordinal, which on this single-part solve IS the composed model's own face index, so `ProjectionEvidence.Attribute(segment.SourceFace)` resolves per-segment operand lineage with no side table; `SourceA`/`SourceB` stay source-edge vertex indices the chain walk links on, negative wherever a visibility split landed mid-edge, and a `-1` face (an inter-part boundary or section segment) attributes to nothing, the honest answer.
- Entry: `Fabrication.Run` remains the sole public package entry; `Hlr.Solve` is internal, receives parameterized ingress and egress, and preserves every `ProjectedSegment` field of every requested view through the kernel `DrawingProjection` carrier.
- Auto: the policy is CONSUMER-AUTHORED per run — a drafting consumer raises its own `ProjectionView` rows from the camera basis it already holds and its `PlotPolicy` from the sheet it is issuing to, minting the whole row through `PlotPolicy.Issue(size, key)` where it holds only the size — so no view, scale, or sheet convention originates inside this owner and admission validates whatever a consumer raises. Requested views enter one `Validation<Error>` traversal, so an unprojectable view reports beside every other failed view rather than masking them.
- Result: `ProjectionEvidence` carries the issued `PlotPolicy`, publishes its `Quadrant` off that sheet's standard, and retains one keyed `ProjectionRun` per requested view — its `ViewPose`, kernel operation, complete `DrawingProjection` including `EdgeKind`, `Invisibility`, `Next`, `SourceA`, `SourceB`, `Part`, `SourceFace`, the flat and per-part `EdgeHistogram` tallies, and the `Contacts` interference roster, and the `Option<HatchResult>` its hatch row produced — beside every boolean composition, the drafting convention, the anchor stream carrying its symbol rows, and the per-part balloon stream a parts-list leader seats on.
- Packages: `Rasm.Drawing` (`View.Apply`, `Hatching.Apply`, `ViewOp`, `ViewKind`, `ViewPolicy`, `ViewConvention`, `ViewPose`, `Camera`, `DrawingProjection`, `ProjectedSegment`, `EdgeHistogram`, `SuccessorChain`, `HatchOp`, `HatchPlan`, `HatchPolicy`, `HatchResult`, `PlotPolicy`, `DrawingScale`, `ScaleLadder`, `ProjectionAngle`, `SheetStandard`), `Rasm.Meshing` (`Arrangement.Apply`, `ArrangementOp.MeshBoolean`, `ArrangementResult`, `MeshSpace`, `BooleanCensus`, `BooleanCensus.ManifoldEvidence`), `Rasm.Spatial` (`BuildPolicy`, `IntersectPolicy`), `Rasm.Numerics` (`Direction.Of`), `Rasm.Fabrication.Spec` (`FeatureFrame`, `FrameSymbolRow`, `CharacteristicId`).
- Boundary: a boolean returns SHELLS, so a severing operand refuses typed mid-fold rather than silently framing one component of a disconnected model; silhouette, crease, and intersection loci are whole-model reads and projecting the first shell would drop geometry the operand legitimately produced.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
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

// --- [TYPES] ---------------------------------------------------------------------------
[ValueObject<string>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public readonly partial struct ViewKey {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        value = value.Trim();
        if (!Witness.Keyed(value))
            validationError = new ValidationError("view-key");
    }
}

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
public sealed partial class ProjectionCharacteristic {
    public FeatureFrame Frame { get; }
    public Point3d ModelLocus { get; }
    public Option<(ContentKey Source, int Edge)> Provenance { get; }

    public CharacteristicId Id => Frame.Id;

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref FeatureFrame frame,
        ref Point3d modelLocus,
        ref Option<(ContentKey Source, int Edge)> provenance) {
        if (!modelLocus.IsValid || provenance.Exists(static value => value.Edge < 0))
            validationError = new ValidationError("projection:characteristic");
    }

    public static Fin<ProjectionCharacteristic> Admit(
        FeatureFrame frame,
        Point3d modelLocus,
        Option<(ContentKey Source, int Edge)> provenance) =>
        Validate(frame, modelLocus, provenance, out ProjectionCharacteristic characteristic).Admitted(characteristic);
}

public readonly record struct BooleanOperand(MeshSpace Other, BooleanOp Operation, ContentKey Source);

public readonly record struct BooleanComposition(ContentKey Source, BooleanCensus Census) {
    public const int OperandSlot = 1;

    public Option<int> OperandOf(int face) => Census.Native.Bind(evidence => evidence.OperandOf(face));
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

    internal Fin<ViewOp> Lower(MeshSpace model, Camera camera, ViewPolicy policy) => Switch(
        state: (Parts: Seq(ViewSubject.Of(model)), Camera: camera, Policy: policy),
        projected: static (state, view) => view.Operation.Switch(
            silhouette: _ => Fin.Succ<ViewOp>(new ViewOp.Silhouette(state.Parts, state.Camera, state.Policy)),
            hiddenLine: _ => Fin.Succ<ViewOp>(new ViewOp.HiddenLine(state.Parts, state.Camera, state.Policy)),
            outline: _ => Fin.Succ<ViewOp>(new ViewOp.Outline(state.Parts, state.Camera, state.Policy)),
            section: _ => Fin.Fail<ViewOp>(new KernelFault.InvalidValue("projection", $"projection:section-without-plane:{view.Key.Value}"))),
        section: static (state, view) => Fin.Succ<ViewOp>(
            new ViewOp.Section(state.Parts, view.Cut, state.Camera, state.Policy)));
}

[ComplexValueObject]
public sealed partial class ProjectionPolicy {
    public ProjectionSource Source { get; }
    public Seq<ProjectionView> Views { get; }
    public ViewConvention Convention { get; }

    public PlotPolicy Plot { get; }

    public Angle CreaseDihedral { get; }
    public double BetaSquared { get; }

    public int SpatialLeaf { get; }
    public Map<ViewKey, HatchPlan> Hatching { get; }
    public Seq<ProjectionCharacteristic> Characteristics { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref ProjectionSource source,
        ref Seq<ProjectionView> views,
        ref ViewConvention convention,
        ref PlotPolicy plot,
        ref Angle creaseDihedral,
        ref double betaSquared,
        ref int spatialLeaf,
        ref Map<ViewKey, HatchPlan> hatching,
        ref Seq<ProjectionCharacteristic> characteristics) {
        Set<ViewKey> roster = toSet(views.Map(static value => value.Key));
        if (!plot.IsValid
            || !double.IsFinite(creaseDihedral.Radians) || creaseDihedral.Radians is <= 0.0 or >= Math.PI
            || !ValidityClaim.Positive(betaSquared).Holds || spatialLeaf <= 0
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
            validationError = new ValidationError("projection:policy");
    }

    public static Fin<ProjectionPolicy> Admit(
        ProjectionSource source,
        Seq<ProjectionView> views,
        ViewConvention convention,
        PlotPolicy plot,
        Angle creaseDihedral,
        double betaSquared,
        int spatialLeaf,
        Map<ViewKey, HatchPlan> hatching,
        Seq<ProjectionCharacteristic> characteristics) =>
        Validate(source, views, convention, plot, creaseDihedral, betaSquared, spatialLeaf,
            hatching, characteristics, out ProjectionPolicy policy).Admitted(policy);
}

public sealed record ProjectionAnchor(
    ViewKey View,
    ProjectionCharacteristic Characteristic,
    Point3d ScreenLocus,
    double Depth) {
    public Seq<FrameSymbolRow> Symbols => Characteristic.Frame.Annotation;
}

public sealed record BalloonAnchor(
    ViewKey View,
    int Part,
    Point3d ScreenLocus,
    double Depth,
    double RunLength,
    int Segments);

public sealed record ProjectionRun(
    ViewKey Key,
    ViewPose Pose,
    ViewKind Operation,
    DrawingProjection Projection,
    Option<HatchResult> Hatch = default);

public sealed record ProjectionEvidence(
    PlotPolicy Plot,
    Seq<ProjectionRun> Runs,
    Seq<ProjectionAnchor> Characteristics,
    Seq<BalloonAnchor> Balloons,
    Seq<BooleanComposition> Composition,
    Seq<ContentKey> Sources) {
    public ProjectionAngle Quadrant => ProjectionAngle.For(Plot.Size.Standard);

    public Option<DrawingProjection> View(ViewKey key) =>
        Runs.Find(run => run.Key == key).Map(static run => run.Projection);

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

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class Hlr {
    private static readonly Op HlrOp = Op.Of(name: "fabrication:hidden-line");

    private readonly record struct Sourced(MeshSpace Model, Seq<BooleanComposition> Composition);

    internal static Fin<FabricationResult> Solve(
        FabricationPolicy.HiddenLine request,
        FabricationInput input,
        Func<ProjectionEvidence, FabricationResult> egress) =>
        from admitted in input.Model.ToFin(HlrOp.InvalidInput())
        from sourced in Source(admitted, request.Policy.Source)
        from framed in request.Policy.Views
            .Traverse(view => ProjectionLeg(view, sourced.Model, request.Policy))
            .As()
            .ToFin()
        select egress(new ProjectionEvidence(
            request.Policy.Plot,
            framed.Map(static value => value.Run),
            framed.Bind(value => Anchors(value.Run.Key, value.Camera, request.Policy.Characteristics)),
            framed.Bind(static value => Balloons(value.Run)),
            sourced.Composition,
            toSeq((input.Sources + sourced.Composition.Map(static value => value.Source))
                .Distinct()
                .OrderBy(static value => value.Kind.Key)
                .ThenBy(static value => value.Digest))));

    private static Seq<ProjectionAnchor> Anchors(
        ViewKey key,
        Camera camera,
        Seq<ProjectionCharacteristic> characteristics) =>
        characteristics.Bind(characteristic => {
            double depth = camera.Depth(characteristic.ModelLocus);
            return ValidityClaim.Positive(depth) ? Seq(new ProjectionAnchor(key, characteristic, camera.Project(characteristic.ModelLocus), depth))
                : Seq<ProjectionAnchor>();
        });

    private static Seq<BalloonAnchor> Balloons(ProjectionRun run) => toSeq(
        SuccessorChain.Walk(run.Projection.Visible, static row => row.Next)
            .Bind(chain => Called(run.Projection, chain[0])
                .Map(part => (
                    Part: part,
                    Head: chain[0],
                    Chain: chain,
                    Run: Drawn(run.Projection.Visible, chain)))
                .ToSeq())
            .Fold(
                Map<int, (int Head, Seq<int> Chain, double Run)>.Empty,
                static (held, row) => held
                    .Find(row.Part)
                    .Exists(seated => seated.Run > row.Run || (seated.Run >= row.Run && seated.Head < row.Head))
                        ? held
                        : held.AddOrUpdate(row.Part, (row.Head, row.Chain, row.Run)))
            .Map((part, seated) => Anchor(run.Key, part, run.Projection.Visible, seated.Chain, seated.Run))
            .Values);

    private static Option<int> Called(DrawingProjection projection, int head) =>
        projection.Visible[head].Part.Filter(part =>
            part >= 0 && part < projection.Parts.Count && projection.Parts[part].VisibleCount > 0);

    private static double Drawn(Seq<ProjectedSegment> rows, Seq<int> chain) =>
        chain.Sum(index => rows[index].ScreenA.DistanceTo(rows[index].ScreenB));

    private static BalloonAnchor Anchor(ViewKey key, int part, Seq<ProjectedSegment> rows, Seq<int> chain, double run) {
        double half = run * 0.5;
        (Point3d Locus, double Depth) seat = chain.Fold(
            (Travelled: 0.0, Seat: Seat(rows[chain[0]], 0.0)),
            (state, index) => {
                ProjectedSegment row = rows[index];
                double span = row.ScreenA.DistanceTo(row.ScreenB);
                return state.Travelled >= half
                    ? state
                    : (state.Travelled + span,
                        Seat(row, span > 0.0 ? Math.Min((half - state.Travelled) / span, 1.0) : 0.0));
            }).Seat;
        return new BalloonAnchor(key, part, seat.Locus, seat.Depth, run, chain.Count);
    }

    private static (Point3d Locus, double Depth) Seat(ProjectedSegment row, double at) => (
        new Point3d(
            row.ScreenA.X + ((row.ScreenB.X - row.ScreenA.X) * at),
            row.ScreenA.Y + ((row.ScreenB.Y - row.ScreenA.Y) * at),
            row.ScreenA.Z + ((row.ScreenB.Z - row.ScreenA.Z) * at)),
        row.Depth.A + ((row.Depth.B - row.Depth.A) * at));

    private static Fin<Sourced> Source(MeshSpace model, ProjectionSource source) => source.Switch(
        state: model,
        model: static (state, _) => Fin.Succ(new Sourced(state, Seq<BooleanComposition>())),
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
                        Kind.Mesh, None, "projection:severed-operand"))
                select new Sourced(
                    solid,
                    current.Composition.Add(new BooleanComposition(operand.Source, kept.Result)))).As());

    private static Fin<(ViewPose Pose, Camera Camera, ViewPolicy View)> Lower(
        MeshSpace model,
        ProjectionDir direction,
        ProjectionPolicy policy) =>
        from bounds in HlrOp.Catch(() => Fin.Succ(model.Native.GetBoundingBox(accurate: false)))
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
                Narrow = IntersectPolicy.Canonical,
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
             .TraverseM(plan => Hatching.Apply(
                 new HatchOp.Projection(projection, plan, HatchLane(policy)),
                 HlrOp))
             .As()
         select (new ProjectionRun(view.Key, lowered.Pose, view.Kind, projection, hatch), lowered.Camera))
        .ToValidation();

    private static HatchPolicy HatchLane(ProjectionPolicy policy) =>
        HatchPolicy.Canonical with {
            Broad = BuildPolicy.Canonical with { LeafSize = policy.SpatialLeaf },
        };
}
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
