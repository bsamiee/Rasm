# [RASM_FABRICATION_MOTION]

CAM motion closes the admitted `(ProcessModality, CutStrategy)` cross-product under one `Cam` fold. `EngagementPolicy` carries physics, cutting data, finish demand, contour compensation, axial schedule, hole/seam/infill/thread/lathe law, planar offset, surface sampling, guard and link policy, strategy generators, and `MotionMounts` execution evidence. Machine capability owns rapid rates.

`Cam.Generate` returns `Fin<Seq<CutElement>>`. Contour depths, pocket rings, medial arcs, surface drives, holes, raster fills, and turning programs preserve their independent element boundaries until `Link.Route` inserts travel. Axial passes change Z; radial stepover changes planar geometry.

## [01]-[INDEX]

- [01]-[CAM_MOTION]: owns `EngagementPolicy`, `HoleLaw`, `AxialPass`, `MotionMounts`, seam/hole/lathe policy rows, `MotionRun`, pair-total strategy dispatch, element linking, workholding/guard conditioning, and the machine/cell solve boundary.

## [02]-[CAM_MOTION]

- Owner: `EngagementPolicy` is the `[ComplexValueObject]` policy owner; `EntryPolicy` is the per-variant payload family for tangential arc, ramp, plunge, and helical entry; `SeamPolicy` and `HoleCycle` are constructor-bound behavior rows; `HoleLaw` is the admitted hole geometry every cycle reads; `LathePolicy` keys one `TurnStep` program per admitted turning strategy; `Cam` owns `Solve` and `Generate`. `MotionMounts` carries fixture state and holder evidence in both cases; `Mounted` also carries the clearance channel, spatial index, and machine kinematics. `MotionRun` is the admitted execution carrier built from policy, input, and the carried mounts.
- Cases: every `CutStrategy` row lands in the generated total switch. `HoleCycle` covers spotting, drilling, pecking, chip-breaking, deep-hole, reaming, interpolated boring, counterboring, countersinking, and fine boring. Fine boring emits an `OrientedStop` directive; turning projects spindle, dwell, synchronization, and barrier semantics into `MotionDirective` without falsifying Cartesian moves.
- Entry: `Solve(FabricationPolicy.Cam, FabricationInput)` is the owner-side fold. `Generate(MotionRun)` derives its `(ProcessModality, CutStrategy)` discriminant from the admitted carrier and dispatches elements. Both return `Fin`; generated `Validate` advice admits every policy axis once, independent profile defects accumulate at the closed-boundary gate, and dependent generation aborts.
- Auto: `EngagementPolicy.Resolve` folds the budget case against process modality and returns feed, compensation, and step-down only for matching pairs. `EngagementPolicy.Schedule` derives axial-pass rows from total depth, step ceiling, finish step-down, and allowances. `MotionRun.Of` resolves scallop chord and IT-grade allowance once. `Solve` submits link objective, precedence, home, work offset, and lowering; `Commit` conditions the linked program once through `Workholding.Apply`, then `Guard.Check` accumulates every hazard into one verdict. `MotionMounts.Floor` admits guard and workholding evidence but rejects execution without joint evidence; `Mounted` threads `CurveSkeleton`, `SpatialIndex`, and kinematics through `MotionRun`.
- Receipt: `FabricationResult.Motion` carries atom-safe moves, generated directives, joint rows, seconds, and cell code; reach is asserted only by a machine or cell solve. `CutElement` identity composes `CanonicalWriter` and the single `ContentHash.Of` mint over the counted station stream, so geometrically equal elements at one depth remain distinct.
- Packages: `Process/owner.md` atoms, `Process/family.md`, `Process/physics.md`, `Process/faults.md`, `Tooling/cuttingdata.md`, `Spec/tolerance.md`, `Geometry2D/algebra.md`, `Geometry2D/arcs.md`, `Rasm.Meshing`, `Toolpath/partition.md`, `Toolpath/skeleton.md`, `Toolpath/surface.md`, `Toolpath/turning.md`, `Toolpath/link.md`, `Toolpath/guard.md`, `Fixturing/workholding.md`, `Kinematics/machine.md`, `Kinematics/cell.md`, `LanguageExt.Core`, `Thinktecture.Runtime.Extensions`, `ContentHash.Of`, `RhinoCommon`, BCL inbox.
- Growth: a new strategy is one family row, one modality admission, and one total-switch arm; a modality-divergent body is one arm on that row's `ProcessModality.Switch`. New hole behavior is one `HoleCycle` row and its `HoleLaw` column; a new turning strategy is one `LathePolicy.Programs` key. A new pass class is one row in the `Schedule` generator, never a caller-authored depth roster.
- Boundary: `Cam` never uses pass count as an axial-depth surrogate, never feeds between islands, rings, graph components, native paths, or fill strokes, and never chord-samples a revolution a `Move.Circular` arc states exactly. Fabricated physics, Cartesian coordinates relabeled as joints, and automatic guard lifts stay unrepresentable.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------------------------------------------------------
using System.Globalization;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Domain;
using Rasm.Fabrication.Fixturing;
using Rasm.Fabrication.Geometry2D;
using Rasm.Fabrication.Kinematics;
using Rasm.Fabrication.Process;
using Rasm.Fabrication.Spec;
using Rasm.Fabrication.Tooling;
using Rasm.Meshing;
using Rasm.Numerics;
using Rasm.Spatial;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Toolpath;

// --- [TYPES] --------------------------------------------------------------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record EntryPolicy {
    private EntryPolicy() { }

    public sealed record TangentialArc : EntryPolicy;
    public sealed record Ramp(double LengthMm, double ClearanceMm) : EntryPolicy;
    public sealed record Plunge(double ClearanceMm) : EntryPolicy;
    public sealed record Helix(double RadiusMm, double PitchMm, double ClearanceMm) : EntryPolicy;
}

[SmartEnum<string>]
public sealed partial class ContourCompensation {
    public static readonly ContourCompensation Centerline = new("centerline", static (_, _) => 0.0);
    public static readonly ContourCompensation Inside = new("inside", static (radius, allowance) => -(radius + allowance));
    public static readonly ContourCompensation Outside = new("outside", static (radius, allowance) => radius + allowance);

    [UseDelegateFromConstructor]
    public partial double Signed(double radius, double allowance);
}

[SmartEnum<string>]
public sealed partial class SeamPolicy {
    public static readonly SeamPolicy Nearest = new("nearest", NearestScore);
    public static readonly SeamPolicy Farthest = new("farthest", FarthestScore);
    public static readonly SeamPolicy SharpestConcave = new("sharpest-concave", SharpestConcaveScore);
    public static readonly SeamPolicy Aligned = new("aligned", AlignedScore);
    public static readonly SeamPolicy Distributed = new("distributed", DistributedScore);

    [UseDelegateFromConstructor]
    public partial double Score(Loop perimeter, Point3d reference, int layer, int index);

    private static double NearestScore(Loop perimeter, Point3d reference, int layer, int index) =>
        perimeter.At(index).DistanceTo(reference);

    private static double FarthestScore(Loop perimeter, Point3d reference, int layer, int index) =>
        -perimeter.At(index).DistanceTo(reference);

    private static double SharpestConcaveScore(Loop perimeter, Point3d reference, int layer, int index) {
        Point3d previous = perimeter.At(index - 1);
        Point3d current = perimeter.At(index);
        Point3d next = perimeter.At(index + 1);
        double deflection = Math.PI - Vector3d.VectorAngle(current - previous, next - current);
        return Predicate.Orient2D(previous, current, next) == Sign.Negative
            ? -Math.Abs(deflection)
            : double.PositiveInfinity;
    }

    private static double AlignedScore(Loop perimeter, Point3d reference, int layer, int index) {
        Point3d center = perimeter.Bound().Center;
        Vector3d axis = reference - center;
        Vector3d radial = perimeter.At(index) - center;
        return axis.IsTiny() || radial.IsTiny() ? double.PositiveInfinity : Vector3d.VectorAngle(axis, radial);
    }

    private static double DistributedScore(Loop perimeter, Point3d reference, int layer, int index) {
        int target = (int)Math.Floor((layer * 0.6180339887498948 % 1.0) * perimeter.Count);
        int distance = Math.Abs(index - target);
        return Math.Min(distance, perimeter.Count - distance);
    }
}

[SmartEnum<string>]
public sealed partial class HoleCycle {
    public static readonly HoleCycle Spot = new("spot", minFitRatio: 0.0, maxFitRatio: 1.0, Spotting);
    public static readonly HoleCycle Drill = new("drill", minFitRatio: 0.98, maxFitRatio: 1.02, Drilling);
    public static readonly HoleCycle Peck = new("peck", minFitRatio: 0.98, maxFitRatio: 1.02,
        static (target, law) => Pecks(target, law, target.StepMm, FullRetract));
    public static readonly HoleCycle ChipBreak = new("chip-break", minFitRatio: 0.98, maxFitRatio: 1.02,
        static (target, law) => Pecks(target, law, target.StepMm, PartialRetract));
    public static readonly HoleCycle DeepHole = new("deep-hole", minFitRatio: 0.98, maxFitRatio: 1.02,
        static (target, law) => Pecks(target, law, target.StepMm * law.DeepStepFraction, FullRetract));
    public static readonly HoleCycle Ream = new("ream", minFitRatio: 0.98, maxFitRatio: 1.02, Reaming);
    public static readonly HoleCycle Bore = new("bore", minFitRatio: 0.0, maxFitRatio: 0.95, Boring);
    public static readonly HoleCycle FineBore = new("fine-bore", minFitRatio: 0.0, maxFitRatio: 0.95,
        Boring);
    public static readonly HoleCycle Counterbore = new("counterbore", minFitRatio: 0.0, maxFitRatio: 0.95, Counterboring);
    public static readonly HoleCycle Countersink = new("countersink", minFitRatio: 0.0, maxFitRatio: 0.95, Countersinking);

    public double MinFitRatio { get; }
    public double MaxFitRatio { get; }

    [UseDelegateFromConstructor]
    public partial Fin<Seq<Move>> Expand(HoleTarget target, HoleLaw law);

    public bool Fits(double cutterDiameterMm, double holeDiameterMm) =>
        holeDiameterMm > 0.0
        && cutterDiameterMm / holeDiameterMm is var ratio
        && ratio >= MinFitRatio && ratio <= MaxFitRatio;

    private static Fin<Seq<Move>> Spotting(HoleTarget target, HoleLaw law) =>
        Fin.Succ(Seq(
            (Move)new Move.Rapid(target.Clear(law)),
            new Move.Linear(target.At(Math.Min(law.Through, target.StepMm * law.SpotDepthFraction)), target.Feed),
            new Move.Rapid(target.Clear(law))));

    private static Fin<Seq<Move>> Drilling(HoleTarget target, HoleLaw law) =>
        Fin.Succ(Seq(
            (Move)new Move.Rapid(target.Clear(law)),
            new Move.Linear(target.At(law.Through), target.Feed),
            new Move.Rapid(target.Clear(law))));

    private static Fin<Seq<Move>> Reaming(HoleTarget target, HoleLaw law) =>
        Fin.Succ(Seq(
            (Move)new Move.Rapid(target.Clear(law)),
            new Move.Linear(target.At(law.Through), target.Feed * law.ReamFeedFraction),
            new Move.Linear(target.Clear(law), target.Feed * law.ReamRetractFraction)));

    private static Fin<Seq<Move>> Boring(HoleTarget target, HoleLaw law) =>
        Interpolated(target, law, target.Radius - target.CutterRadiusMm, law.Through);

    private static Fin<Seq<Move>> Counterboring(HoleTarget target, HoleLaw law) =>
        Interpolated(target, law, (law.CounterDiameterMm * 0.5) - target.CutterRadiusMm, law.RecessDepthMm);

    private static Fin<Seq<Move>> Countersinking(HoleTarget target, HoleLaw law) =>
        law.SinkDepth(target.DiameterMm) is var depth && depth > 0.0 && depth <= law.Through
            ? Fin.Succ(Seq(
                (Move)new Move.Rapid(target.Clear(law)),
                new Move.Linear(target.At(depth), target.Feed),
                new Move.Rapid(target.Clear(law))))
            : Fin.Fail<Seq<Move>>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "hole:countersink:included-angle").ToError());

    // Quarter revolutions ride `Move.Circular` exactly; chord sampling governs linear approximation alone.
    private static Fin<Seq<Move>> Interpolated(HoleTarget target, HoleLaw law, double radius, double depth) =>
        radius <= 0.0 || depth <= 0.0 || target.StepMm <= 0.0 || !double.IsFinite(radius) || !double.IsFinite(depth)
            ? Fin.Fail<Seq<Move>>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "hole:interpolated-clearance").ToError())
            : Count(depth, target.StepMm, (int.MaxValue - 1) / 4, "hole:interpolated-passes").Map(turns => Cam.Helix(
                target.Top,
                radius,
                depth,
                turns,
                law.ClearanceMm,
                target.Feed));

    private static Fin<Seq<Move>> Pecks(
        HoleTarget target,
        HoleLaw law,
        double step,
        Func<HoleTarget, HoleLaw, double, double> retractAt) =>
        Count(law.Through, step, int.MaxValue, "hole:peck-passes").Map(passes =>
            Seq<Move>(new Move.Rapid(target.Clear(law))).Concat(Range(1, passes).Bind(index =>
                Math.Min(law.Through, index * step) is var depth
                    ? Seq(
                        (Move)new Move.Linear(target.At(depth), target.Feed),
                        new Move.Rapid(target.At(retractAt(target, law, depth))))
                    : Seq<Move>())));

    private static Fin<int> Count(double extent, double step, int ceiling, string slot) {
        double count = Math.Ceiling(extent / step);
        return double.IsFinite(count) && count >= 1.0 && count <= ceiling
            ? Fin.Succ(checked((int)count))
            : Fin.Fail<int>(new GeometryFault.DegenerateInput(Kind.Curve, -1, slot).ToError());
    }

    private static double FullRetract(HoleTarget target, HoleLaw law, double depth) =>
        -law.ClearanceMm;

    private static double PartialRetract(HoleTarget target, HoleLaw law, double depth) =>
        Math.Max(0.0, depth - (target.StepMm * law.RetractFraction));
}

// --- [MODELS] -------------------------------------------------------------------------------------------------------------------------------------
public readonly record struct HoleTarget(Point3d Top, double DiameterMm, double StepMm, double CutterRadiusMm, double Feed) {
    public double Radius => DiameterMm * 0.5;

    public Point3d At(double depth) => new(Top.X, Top.Y, Top.Z - depth);

    public Point3d Clear(HoleLaw law) => At(-law.ClearanceMm);
}

[ComplexValueObject]
public sealed partial class HoleLaw {
    public double DepthMm { get; }
    public double BreakthroughMm { get; }
    public double ClearanceMm { get; }
    public double RecessDepthMm { get; }
    public double CounterDiameterMm { get; }
    public double IncludedAngleDeg { get; }
    public double RetractFraction { get; }
    public double SpotDepthFraction { get; }
    public double DeepStepFraction { get; }
    public double ReamFeedFraction { get; }
    public double ReamRetractFraction { get; }

    public double Through => DepthMm + BreakthroughMm;

    public double SinkDepth(double diameterMm) =>
        (CounterDiameterMm - diameterMm) * 0.5 / Math.Tan(IncludedAngleDeg * Math.PI / 360.0);

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref double depthMm,
        ref double breakthroughMm,
        ref double clearanceMm,
        ref double recessDepthMm,
        ref double counterDiameterMm,
        ref double includedAngleDeg,
        ref double retractFraction,
        ref double spotDepthFraction,
        ref double deepStepFraction,
        ref double reamFeedFraction,
        ref double reamRetractFraction) =>
        validationError = depthMm > 0.0 && double.IsFinite(depthMm)
            && breakthroughMm >= 0.0 && double.IsFinite(breakthroughMm)
            && clearanceMm >= 0.0 && double.IsFinite(clearanceMm)
            && recessDepthMm >= 0.0 && double.IsFinite(recessDepthMm)
            && counterDiameterMm > 0.0 && double.IsFinite(counterDiameterMm)
            && includedAngleDeg is > 0.0 and < 180.0
            && retractFraction is >= 0.0 and <= 1.0
            && spotDepthFraction is > 0.0 and <= 1.0
            && deepStepFraction is > 0.0 and < 1.0
            && reamFeedFraction is > 0.0 and <= 1.0
            && reamRetractFraction is > 0.0 and <= 1.0
                ? null
                : new ValidationError("hole-law:invalid");
}

public readonly record struct AxialPass(double DepthMm, double RadialAllowanceMm, double FloorAllowanceMm, double FeedScale);

[ComplexValueObject]
public sealed partial class LathePolicy {
    public TurnStock Stock { get; }
    public TurnInsert Insert { get; }
    public SpindleMode Spindle { get; }
    public TurnPolicy Motion { get; }
    public HashMap<CutStrategy, Seq<TurnStep>> Programs { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref TurnStock stock,
        ref TurnInsert insert,
        ref SpindleMode spindle,
        ref TurnPolicy motion,
        ref HashMap<CutStrategy, Seq<TurnStep>> programs) =>
        validationError = programs.IsEmpty || programs.Exists(static row => row.Value.IsEmpty)
            ? new ValidationError("lathe-policy:empty-program")
            : null;

    public Fin<Seq<TurnStep>> Steps(CutStrategy strategy) =>
        Programs.Find(strategy).ToFin(
            new GeometryFault.DegenerateInput(Kind.Curve, -1, $"lathe-policy:unprogrammed:{strategy.Key}").ToError());
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MotionMounts(Fixture Fixture, FixtureState State, HolderState Holder) {
    private MotionMounts() { }

    public sealed record Floor(Fixture GuardFixture, FixtureState CuttingState, HolderState GuardHolder)
        : MotionMounts(GuardFixture, CuttingState, GuardHolder);
    public sealed record Mounted(
        Fixture MountedFixture,
        FixtureState CuttingState,
        HolderState GuardHolder,
        Option<CurveSkeleton> Channel,
        Option<SpatialIndex> Index,
        Option<MachineKinematics> Kinematics) : MotionMounts(MountedFixture, CuttingState, GuardHolder);
}

[ComplexValueObject]
public sealed partial class EngagementPolicy {
    public ProcessBudget Budget { get; }
    public Option<CuttingData> Cutting { get; }
    public double TargetAngle { get; }
    public double MaxAxialDepth { get; }
    public double AxialDepthMm { get; }
    public double FinishStepDownMm { get; }
    public double FinishAllowanceMm { get; }
    public double FloorAllowanceMm { get; }
    public double FinishFeedFraction { get; }
    public RaTarget Finish { get; }
    public ItGrade FinishGrade { get; }
    public ContourCompensation Contour { get; }
    public EntryPolicy Entry { get; }
    public CutSense Sense { get; }
    public HoleCycle HoleCycle { get; }
    public HoleLaw Hole { get; }
    public SeamPolicy Seam { get; }
    public Point3d SeamReference { get; }
    public PartitionStrategy Infill { get; }
    public WalkStrategy Walk { get; }
    public double InfillAngleDeg { get; }
    public double InfillAngleAdvanceDeg { get; }
    public Rasm.Fabrication.Geometry2D.OffsetPolicy PlanarOffset { get; }
    public double ThreadPitchMm { get; }
    public double PencilContactAngleDeg { get; }
    public SurfaceSampling Sampling { get; }
    public WaterlineMode Waterline { get; }
    public GuardPolicy Guard { get; }
    public Seq<GuardProbe> Probes { get; }
    public LinkPolicyRaw Link { get; }
    public LinkObjective LinkObjective { get; }
    public Arr<OrderConstraint> LinkPrecedence { get; }
    public Point3d Home { get; }
    public string WorkOffset { get; }
    public HashMap<CutStrategy, Func<MotionRun, Fin<Seq<CutElement>>>> Generators { get; }
    public Option<Func<MeshSpace, SurfaceLayoutKind, double, Fin<Seq<SurfaceDrive>>>> Layout { get; }
    public MotionMounts Mounts { get; }
    public LathePolicy Turning { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref ProcessBudget budget,
        ref Option<CuttingData> cutting,
        ref double targetAngle,
        ref double maxAxialDepth,
        ref double axialDepthMm,
        ref double finishStepDownMm,
        ref double finishAllowanceMm,
        ref double floorAllowanceMm,
        ref double finishFeedFraction,
        ref RaTarget finish,
        ref ItGrade finishGrade,
        ref ContourCompensation contour,
        ref EntryPolicy entry,
        ref CutSense sense,
        ref HoleCycle holeCycle,
        ref HoleLaw hole,
        ref SeamPolicy seam,
        ref Point3d seamReference,
        ref PartitionStrategy infill,
        ref WalkStrategy walk,
        ref double infillAngleDeg,
        ref double infillAngleAdvanceDeg,
        ref Rasm.Fabrication.Geometry2D.OffsetPolicy planarOffset,
        ref double threadPitchMm,
        ref double pencilContactAngleDeg,
        ref SurfaceSampling sampling,
        ref WaterlineMode waterline,
        ref GuardPolicy guard,
        ref Seq<GuardProbe> probes,
        ref LinkPolicyRaw link,
        ref LinkObjective linkObjective,
        ref Arr<OrderConstraint> linkPrecedence,
        ref Point3d home,
        ref string workOffset,
        ref HashMap<CutStrategy, Func<MotionRun, Fin<Seq<CutElement>>>> generators,
        ref Option<Func<MeshSpace, SurfaceLayoutKind, double, Fin<Seq<SurfaceDrive>>>> layout,
        ref MotionMounts mounts,
        ref LathePolicy turning) {
        Seq<string> axes = Seq(
            (Ok: targetAngle is > 0.0 and <= 180.0 && double.IsFinite(targetAngle), Axis: "target-angle"),
            (Ok: maxAxialDepth > 0.0 && double.IsFinite(maxAxialDepth), Axis: "axial-step"),
            (Ok: axialDepthMm > 0.0 && double.IsFinite(axialDepthMm), Axis: "axial-depth"),
            (Ok: finishStepDownMm >= 0.0 && finishStepDownMm <= axialDepthMm, Axis: "finish-step"),
            (Ok: finishAllowanceMm >= 0.0 && double.IsFinite(finishAllowanceMm), Axis: "finish-allowance"),
            (Ok: floorAllowanceMm >= 0.0 && double.IsFinite(floorAllowanceMm), Axis: "floor-allowance"),
            (Ok: finishFeedFraction is > 0.0 and <= 1.0, Axis: "finish-feed"),
            (Ok: Valid(entry), Axis: "entry"),
            (Ok: seamReference.IsValid, Axis: "seam-reference"),
            (Ok: walk is not null, Axis: "walk-strategy"),
            (Ok: infillAngleDeg is >= 0.0 and < 180.0, Axis: "infill-angle"),
            (Ok: infillAngleAdvanceDeg is >= 0.0 and < 180.0, Axis: "infill-advance"),
            (Ok: threadPitchMm > 0.0 && double.IsFinite(threadPitchMm), Axis: "thread-pitch"),
            (Ok: pencilContactAngleDeg is >= 0.0 and <= 90.0 && double.IsFinite(pencilContactAngleDeg), Axis: "pencil-angle"),
            (Ok: home.IsValid, Axis: "home"),
            (Ok: !string.IsNullOrWhiteSpace(workOffset), Axis: "work-offset"))
            .Filter(static row => !row.Ok)
            .Map(static row => row.Axis);
        validationError = axes.IsEmpty
            ? null
            : new ValidationError($"engagement:{string.Join('|', axes)}");
    }

    private static bool Valid(EntryPolicy entry) =>
        entry.Switch(
            tangentialArc: static _ => true,
            ramp: static row => row.LengthMm > 0.0 && row.ClearanceMm >= 0.0
                && double.IsFinite(row.LengthMm) && double.IsFinite(row.ClearanceMm),
            plunge: static row => row.ClearanceMm >= 0.0 && double.IsFinite(row.ClearanceMm),
            helix: static row => row.RadiusMm > 0.0 && row.PitchMm > 0.0 && row.ClearanceMm >= 0.0
                && double.IsFinite(row.RadiusMm) && double.IsFinite(row.PitchMm) && double.IsFinite(row.ClearanceMm));

    // Rough levels descend to the finish stock line; the terminal row alone clears the floor at the scaled finishing feed.
    public Seq<AxialPass> Schedule(double stepDown, double allowanceMm) {
        double step = Math.Min(MaxAxialDepth, stepDown > 0.0 ? stepDown : MaxAxialDepth);
        double rough = Math.Max(0.0, AxialDepthMm - FinishStepDownMm);
        return Range(1, Math.Max(1, (int)Math.Ceiling(rough / step)))
            .Map(level => new AxialPass(
                Math.Min(rough, level * step),
                allowanceMm + FinishAllowanceMm,
                FloorAllowanceMm,
                FeedScale: 1.0))
            .ToSeq()
            .Filter(static pass => pass.DepthMm > 0.0)
            .Add(new AxialPass(AxialDepthMm, allowanceMm, FloorAllowanceMm: 0.0, FinishFeedFraction));
    }

    public Fin<(double Feed, double Compensation, double StepDown)> Resolve(ProcessModality modality, CutterForm cutter) =>
        Budget.Switch(
            state: (Modality: modality, Cutter: cutter),
            subtractive: static (state, budget) => state.Modality == ProcessModality.Subtractive
                ? Admit(budget.FeedRate, state.Cutter.Diameter * 0.5, budget.DepthOfCut)
                : Mismatch(state.Modality, "subtractive"),
            turning: static (state, budget) => state.Modality == ProcessModality.Subtractive
                ? Admit(budget.FeedPerRevolution, budget.NoseRadius, budget.DepthOfCut)
                : Mismatch(state.Modality, "turning"),
            thermal: static (state, budget) => state.Modality == ProcessModality.Thermal
                ? Admit(budget.CutSpeed, budget.KerfWidth * 0.5, 0.0)
                : Mismatch(state.Modality, "thermal"),
            abrasive: static (state, budget) => state.Modality == ProcessModality.Abrasive
                ? Admit(budget.TraverseSpeed, state.Cutter.Diameter * 0.5, 0.0)
                : Mismatch(state.Modality, "abrasive"),
            fff: static (state, budget) => state.Modality == ProcessModality.Additive
                ? Admit(budget.PrintSpeed, budget.ExtrusionWidth * 0.5, budget.LayerHeight)
                : Mismatch(state.Modality, "fff"),
            deposition: static (state, _) => Mismatch(state.Modality, "deposition:travel-speed-absent"),
            joining: static (state, budget) => state.Modality == ProcessModality.Joined
                ? Admit(budget.TravelSpeed, 0.0, 0.0)
                : Mismatch(state.Modality, "joining"),
            erosion: static (state, budget) => state.Modality == ProcessModality.Erosion
                ? Admit(budget.WireFeed, state.Cutter.Diameter * 0.5, 0.0)
                : Mismatch(state.Modality, "erosion"),
            resin: static (state, _) => Mismatch(state.Modality, "resin-noncontinuous"),
            powder: static (state, budget) => state.Modality == ProcessModality.Additive
                ? Admit(budget.ScanSpeed, budget.HatchSpacing * 0.5, 0.0)
                : Mismatch(state.Modality, "powder"),
            formed: static (state, _) => Mismatch(state.Modality, "formed-non-cam"));

    private static Fin<(double Feed, double Compensation, double StepDown)> Admit(double feed, double compensation, double stepDown) =>
        feed > 0.0 && compensation >= 0.0 && stepDown >= 0.0
        && double.IsFinite(feed) && double.IsFinite(compensation) && double.IsFinite(stepDown)
            ? Fin.Succ((feed, compensation, stepDown))
            : Fin.Fail<(double, double, double)>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "engagement:resolved-physics").ToError());

    private static Fin<(double Feed, double Compensation, double StepDown)> Mismatch(ProcessModality modality, string budget) =>
        Fin.Fail<(double, double, double)>(new GeometryFault.DegenerateInput(Kind.Curve, -1, $"engagement:{modality.Key}:{budget}").ToError());
}

public sealed record MotionRun(
    FabricationPolicy.Cam Policy,
    FabricationInput Input,
    Fixture Fixture,
    FixtureState State,
    GuardStock Stock,
    Option<MachineKinematics> Kinematics,
    double Feed,
    double Compensation,
    double StepDown,
    double Chord,
    double Allowance) {
    public (ProcessModality Modality, CutStrategy Strategy) Pair => (Input.Process.Modality, Policy.Strategy);

    public Seq<AxialPass> Schedule => Policy.Engagement.Schedule(StepDown, Allowance);

    public static Fin<MotionRun> Of(FabricationPolicy.Cam policy, FabricationInput input) =>
        from physics in policy.Engagement.Resolve(input.Process.Modality, policy.Cutter)
        from scallop in Tolerance.Apply(new ToleranceRequest.Scallop(policy.Engagement.Finish, policy.Cutter))
        from chord in scallop is ToleranceReceipt.Scallop receipt
            ? Fin.Succ(receipt.StepMm)
            : Fin.Fail<double>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "cam:scallop-receipt").ToError())
        from tolerance in Tolerance.Apply(new ToleranceRequest.Allowance(policy.Engagement.FinishGrade))
        from allowance in tolerance is ToleranceReceipt.Allowance grade
            ? Fin.Succ(grade.Millimeters)
            : Fin.Fail<double>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "cam:allowance-receipt").ToError())
        let mounts = policy.Engagement.Mounts.Switch(
            floor: static row => (row.Fixture, row.State, row.Holder,
                Channel: Option<CurveSkeleton>.None, Index: Option<SpatialIndex>.None,
                Kinematics: Option<MachineKinematics>.None),
            mounted: static row => (row.Fixture, row.State, row.Holder, row.Channel, row.Index, row.Kinematics))
        from stock in GuardStock.Validate(
            input.Profiles.ToSeq(),
            input.Keepouts.ToSeq(),
            mounts.Fixture.Zones,
            input.Snapshots,
            policy.Cutter,
            mounts.Holder,
            mounts.Channel,
            mounts.Index,
            new ProbeRoute.Reference(),
            out GuardStock guardStock) is { } stockError
                ? Fin.Fail<GuardStock>(new GeometryFault.DegenerateInput(Kind.Curve, -1, stockError.Message).ToError())
                : Fin.Succ(guardStock)
        select new MotionRun(
                policy,
                input,
                mounts.Fixture,
                mounts.State,
                stock,
                mounts.Kinematics,
                physics.Feed,
                physics.Compensation,
                physics.StepDown > 0.0 ? physics.StepDown : policy.Engagement.MaxAxialDepth,
                chord,
                allowance);
}

// --- [OPERATIONS] ---------------------------------------------------------------------------------------------------------------------------------
public static class Cam {
    public static Fin<FabricationResult> Solve(FabricationPolicy.Cam policy, FabricationInput input) =>
        from _ in input.Process.Modality.Admits(policy.Strategy)
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(FabricationFault.InadmissiblePair(
                new RelationFault.ModalityStrategy(input.Process.Modality, policy.Strategy)).ToError())
        from __ in ClosedGate(policy.Strategy, input)
        from run in MotionRun.Of(policy, input)
        from elements in Generate(run)
        from keepouts in input.Keepouts.Map((loop, index) => Keepout.Admit(
            $"input:{index}",
            loop,
            new KeepoutExtent.Bounded(loop.Bound().Min.Z, loop.Bound().Max.Z),
            FormattableString.Invariant($"{policy.Engagement.Link.ToleranceMm} mm"))).TraverseM(identity).As()
        from linked in Link.Route(
            new LinkDemand(
                policy.Engagement.Home,
                elements.ToArr(),
                keepouts.ToArr(),
                policy.Engagement.LinkPrecedence,
                policy.Engagement.Link,
                policy.Engagement.LinkObjective,
                (points, kind) => Lower(run, points, kind),
                static moves => Fin.Succ(moves)),
            static route => (route.Moves, route.Directives.Add(route.SpecializedDirective)))
        from solved in Commit(run, linked.Moves, linked.Directives)
        select (FabricationResult)solved;

    public static Fin<Seq<CutElement>> Generate(MotionRun run) =>
        !run.Pair.Modality.Admits(run.Pair.Strategy)
            ? Fin.Fail<Seq<CutElement>>(FabricationFault.InadmissiblePair(
                new RelationFault.ModalityStrategy(run.Pair.Modality, run.Pair.Strategy)).ToError())
            : run.Pair.Strategy.Switch(
            boundaryPass: _ => Contour(run),
            pocketClear:  _ => Pocket(run),
            peck:         _ => Holes(run, run.Policy.Engagement.HoleCycle),
            adaptive:     _ => Adaptive(run),
            radialSweep:  _ => Turn(run),
            plungeDwell:  _ => run.Pair.Modality.Switch(
                subtractive: _ => Turn(run),
                erosion:     _ => Sink(run),
                thermal:     _ => Inadmissible(run),
                abrasive:    _ => Inadmissible(run),
                additive:    _ => Inadmissible(run),
                formed:      _ => Inadmissible(run),
                joined:      _ => Inadmissible(run)),
            helical:      _ => Helical(run, run.StepDown),
            threadMill:   _ => Helical(run, run.Policy.Engagement.ThreadPitchMm),
            layerWalk:    _ => Layer(run),
            waterline:    _ => Surface(run, policy => new SurfaceStrategy.Waterline(
                policy,
                run.Schedule.Map(static pass => -pass.DepthMm).ToArr(),
                run.Policy.Engagement.Waterline)),
            scallop:      _ => Surface(run, policy => new SurfaceStrategy.Scallop(policy, new SurfaceLayoutKind.PlanarRaster())),
            pencil:       _ => Surface(run, policy => new SurfaceStrategy.Pencil(
                policy,
                new SurfaceLayoutKind.PlanarRaster(),
                run.Policy.Engagement.PencilContactAngleDeg)),
            rest:         _ => Rest(run),
            threePlusTwo: _ => Surface(run, policy => new SurfaceStrategy.ThreePlusTwo(policy, new SurfaceLayoutKind.PlanarRaster(), Arr(run.Input.View))),
            swarf:        _ =>
                from key in Key("morph")
                from elements in Surface(run, policy => new SurfaceStrategy.Swarf(
                    policy,
                    new SurfaceLayoutKind.Kernel(key),
                    run.Input.View,
                    run.Policy.Pass.StepOver))
                select elements,
            drillCycle:   _ => Holes(run, HoleCycle.Drill),
            boreCycle:    _ => Holes(run, HoleCycle.Bore),
            reamCycle:    _ => Holes(run, HoleCycle.Ream),
            face:         _ => Surface(run, policy => new SurfaceStrategy.FiberSlice(
                policy,
                new SurfaceLayoutKind.PlanarRaster())),
            slot:         _ => Adaptive(run),
            trochoidal:   _ => Extend(run),
            raster:       _ => run.Pair.Modality.Switch(
                subtractive: _ => Surface(run, policy => new SurfaceStrategy.Scallop(
                    policy,
                    new SurfaceLayoutKind.PlanarRaster())),
                thermal:     _ => Fill(run, layer: 0),
                additive:    _ => Fill(run, layer: 0),
                abrasive:    _ => Inadmissible(run),
                erosion:     _ => Inadmissible(run),
                formed:      _ => Inadmissible(run),
                joined:      _ => Inadmissible(run)),
            spiral:       _ => Kernel(run, "spiral"),
            morph:        _ => Kernel(run, "morph"),
            geodesic:     _ => Kernel(run, "geodesic"),
            rotary:       _ =>
                from key in Key("rotary")
                from elements in Surface(run, policy => new SurfaceStrategy.ThreePlusTwo(
                    policy,
                    new SurfaceLayoutKind.Kernel(key),
                    Arr(run.Input.View)))
                select elements,
            fiveAxisContour: _ =>
                from key in Key("contour")
                from elements in Surface(run, policy => new SurfaceStrategy.Swarf(
                    policy,
                    new SurfaceLayoutKind.Kernel(key),
                    run.Input.View,
                    run.Policy.Pass.StepOver))
                select elements,
            layerContour: _ => Layer(run),
            layerInfill:  _ => Layer(run),
            support:      _ => Extend(run),
            seam:         _ => Trace(run),
            spot:         _ => run.Pair.Modality.Switch(
                subtractive: _ => Holes(run, HoleCycle.Spot),
                joined:      _ => Tack(run),
                thermal:     _ => Inadmissible(run),
                abrasive:    _ => Inadmissible(run),
                erosion:     _ => Inadmissible(run),
                additive:    _ => Inadmissible(run),
                formed:      _ => Inadmissible(run)),
            form:         _ => Extend(run));

    private static Fin<Seq<CutElement>> Inadmissible(MotionRun run) =>
        Fin.Fail<Seq<CutElement>>(FabricationFault.InadmissiblePair(
            new RelationFault.ModalityStrategy(run.Pair.Modality, run.Pair.Strategy)).ToError());

    private static Fin<Unit> ClosedGate(CutStrategy strategy, FabricationInput input) =>
        !DemandsClosed(strategy)
            ? Fin.Succ(unit)
            : toSeq(input.Profiles)
                .Map((loop, index) => (Index: index, loop.Closed))
                .Filter(static row => !row.Closed) is var open && open.IsEmpty
                ? Fin.Succ(unit)
                : Fin.Fail<Unit>(Error.Many([.. open.Map(row => FabricationFault.OpenLoop(FabConcern.Toolpath, row.Index).ToError())]));

    private static bool DemandsClosed(CutStrategy strategy) =>
        strategy.Switch(
            boundaryPass: static _ => true,
            pocketClear:  static _ => true,
            peck:         static _ => true,
            adaptive:     static _ => true,
            radialSweep:  static _ => false,
            plungeDwell:  static _ => false,
            helical:      static _ => true,
            threadMill:   static _ => true,
            layerWalk:    static _ => true,
            waterline:    static _ => false,
            scallop:      static _ => false,
            pencil:       static _ => false,
            rest:         static _ => false,
            threePlusTwo: static _ => false,
            swarf:        static _ => false,
            drillCycle:   static _ => true,
            boreCycle:    static _ => true,
            reamCycle:    static _ => true,
            face:         static _ => true,
            slot:         static _ => true,
            trochoidal:   static _ => true,
            raster:       static _ => false,
            spiral:       static _ => false,
            morph:        static _ => false,
            geodesic:     static _ => false,
            rotary:       static _ => false,
            fiveAxisContour: static _ => false,
            layerContour: static _ => true,
            layerInfill:  static _ => true,
            support:      static _ => false,
            seam:         static _ => false,
            spot:         static _ => true,
            form:         static _ => false);

    private static Fin<FabricationResult.Motion> Commit(MotionRun run, Seq<Move> linked, Seq<MotionDirective> directives) =>
        from conditioned in Condition(run, linked)
        let home = run.Policy.Engagement.Home
        from guarded in Guard(run, conditioned, home)
        from solved in run.Input.Cell.Match(
            Some: cell =>
                from receipt in RobotProgram.Run(cell, guarded, new CellProgramRequest.Motion(run.Policy.Cell))
                from motion in receipt is CellProgramReceipt.Motion completed
                    ? Fin.Succ(completed.Result)
                    : Fin.Fail<FabricationResult.Motion>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "cam:cell-motion-receipt").ToError())
                select motion,
            None: () => run.Kinematics.Match(
                Some: kinematics => MachineTool.Solve(kinematics, guarded).Map(static solution => solution.Motion),
                None: () => Fin.Fail<FabricationResult.Motion>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "cam:motion-evidence-unavailable").ToError())))
        select solved with {
            Directives = directives,
            Subjects = (run.Input.Sources + run.Input.ParentRuns).Distinct(),
        };

    private static Fin<Seq<Move>> Condition(MotionRun run, Seq<Move> moves) =>
        Workholding.Apply(new WorkholdingOp.Condition(run.Fixture, run.State, moves)).Bind(result =>
            result is WorkholdingResult.Conditioned conditioned
                ? Fin.Succ(conditioned.Moves)
                : Fin.Fail<Seq<Move>>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "cam:workholding-receipt").ToError()));

    // Every move is guarded so one verdict reports the whole program's hazards; aborting on the first hides the rest.
    private static Fin<Seq<Move>> Guard(MotionRun run, Seq<Move> moves, Point3d home) =>
        moves.Fold(
            (Cursor: home, Faults: Seq<Error>()),
            (state, move) => (Target(move), state.Faults.Concat(Guarded(run, state.Cursor, move))))
        is var walked && walked.Faults.IsEmpty
            ? Fin.Succ(moves)
            : Fin.Fail<Seq<Move>>(Error.Many([.. walked.Faults]));

    private static Fin<Seq<Move>> Lower(MotionRun run, Seq<Point3d> points, RetractKind kind) =>
        points.Count < 2
            ? Fin.Fail<Seq<Move>>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "cam:link-route").ToError())
            : Fin.Succ(points.Tail.Map((point, index) =>
                kind == RetractKind.Ramp || kind == RetractKind.ControlledDescent && index == points.Count - 2
                    ? (Move)new Move.Linear(point, run.Policy.Engagement.Link.PlungeMmPerMin)
                    : new Move.Rapid(point)));

    private static Seq<Error> Guarded(MotionRun run, Point3d cursor, Move move) =>
        (from part in GuardPart.Validate(cursor, run.Input.Profiles.ToSeq(), out GuardPart guardPart) is { } partError
            ? Fin.Fail<GuardPart>(new GeometryFault.DegenerateInput(Kind.Curve, -1, partError.Message).ToError())
            : Fin.Succ(guardPart)
        from request in GuardRequest.Validate(
            move,
            part,
            run.Stock,
            run.Fixture,
            run.State,
            run.Policy.Engagement.Guard,
            run.Policy.Engagement.Probes,
            out GuardRequest guardRequest) is { } requestError
                ? Fin.Fail<GuardRequest>(new GeometryFault.DegenerateInput(Kind.Curve, -1, requestError.Message).ToError())
                : Fin.Succ(guardRequest)
        from receipt in Guard.Check(request)
        select receipt.Hazards.Map(hazard => hazard.Switch(
            gouge: row => FabricationFault.Gouge(row.Surface, run.Policy.Cutter).ToError(),
            fixed: static row => new GeometryFault.DegenerateInput(Kind.Curve, -1, $"cam:guard:fixed:{row.Obstacle.Operation}:{row.Obstacle.Element}").ToError(),
            keepout: static row => new GeometryFault.DegenerateInput(Kind.Curve, -1, $"cam:guard:keepout:{row.Surface.X:R}:{row.Surface.Y:R}:{row.Surface.Z:R}").ToError(),
            stock: static _ => new GeometryFault.DegenerateInput(Kind.Curve, -1, "cam:guard:stock").ToError(),
            channel: static row => new GeometryFault.DegenerateInput(Kind.Curve, -1, $"cam:guard:channel:{row.RequiredMm:R}").ToError(),
            voxel: static row => new GeometryFault.DegenerateInput(Kind.Curve, -1, $"cam:guard:voxel:{row.Contact.Obstacle.Key}").ToError(),
            robot: static row => new GeometryFault.DegenerateInput(Kind.Curve, -1, $"cam:guard:robot:{row.Contact.CollisionTarget}").ToError())))
        .Match(Succ: static hazards => hazards, Fail: static error => Seq(error));

    private static Fin<Seq<CutElement>> Contour(MotionRun run) =>
        toSeq(run.Input.Profiles).Traverse(loop =>
            run.Schedule.Traverse(pass => ContourPass(run, loop, pass)).Map(static passes => passes.Bind(identity)))
        .Map(static profiles => profiles.Bind(identity));

    private static Fin<Seq<CutElement>> ContourPass(MotionRun run, Loop loop, AxialPass pass) =>
        run.Policy.Engagement.Contour.Signed(run.Compensation, pass.RadialAllowanceMm) is var delta
        && pass.DepthMm - pass.FloorAllowanceMm is var cut
        && run.Feed * pass.FeedScale is var feed
            ? from offsets in delta == 0.0
                ? Fin.Succ(Seq(loop.AsCcw()))
                : Offset(Seq(loop.AsCcw()), delta, run.Policy.Engagement.PlanarOffset)
              from elements in offsets.IsEmpty
                ? Fin.Fail<Seq<CutElement>>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "cam:contour-inaccessible").ToError())
                : offsets.Traverse(ring =>
                    from conditioned in Entry(
                        run.Policy.Engagement.Entry,
                        ring,
                        feed,
                        cut,
                        Math.Max(run.Compensation * 0.5, run.Chord),
                        delta < 0.0 ? MaterialSide.Inside : MaterialSide.Outside)
                    from element in Element(run, conditioned.Lead
                        .Concat(AtDepth(Perimeter(run, ring, feed, layer: 0), cut))
                        .Concat(conditioned.Exit))
                    select element)
              select elements
            : Fin.Fail<Seq<CutElement>>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "cam:contour-pass").ToError());

    private static Fin<(Seq<Move> Lead, Seq<Move> Exit)> Entry(
        EntryPolicy policy,
        Loop ring,
        double feed,
        double depth,
        double leadRadius,
        MaterialSide side) =>
        policy.Switch<Fin<(Seq<Move> Lead, Seq<Move> Exit)>>(
            tangentialArc: _ =>
                from trace in ArcAlgebra.Apply(new ArcOp.Lead(
                    ring,
                    Station: 0.0,
                    feed,
                    new LeadShape.Tangent(leadRadius, Math.PI / 2.0),
                    side))
                from motion in trace is ArcTrace.Motion moved
                    ? Fin.Succ(moved.Receipt)
                    : Fin.Fail<MotionReceipt>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "cam:lead-receipt").ToError())
                select (AtDepth(motion.Moves, depth), Seq<Move>()),
            ramp: row => RampEntry(ring, feed, depth, row),
            plunge: row => Fin.Succ((
                Seq<Move>(
                    new Move.Rapid(AtZ(ring.At(0), ring.At(0).Z + row.ClearanceMm)),
                    new Move.Linear(AtZ(ring.At(0), ring.At(0).Z - depth), feed)),
                Seq<Move>())),
            helix: row => HelixEntry(ring, feed, depth, row));

    private static Fin<(Seq<Move> Lead, Seq<Move> Exit)> RampEntry(
        Loop ring,
        double feed,
        double depth,
        EntryPolicy.Ramp policy) {
        Vector3d tangent = ring.At(1) - ring.At(0);
        if (!tangent.Unitize())
            return Fin.Fail<(Seq<Move>, Seq<Move>)>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "cam:ramp-entry").ToError());
        Point3d start = ring.At(0) - (tangent * policy.LengthMm);
        return Fin.Succ((
            Seq<Move>(
                new Move.Rapid(AtZ(start, start.Z + policy.ClearanceMm)),
                new Move.Linear(AtZ(ring.At(0), ring.At(0).Z - depth), feed)),
            Seq<Move>()));
    }

    private static Fin<(Seq<Move> Lead, Seq<Move> Exit)> HelixEntry(
        Loop ring,
        double feed,
        double depth,
        EntryPolicy.Helix policy) {
        Point3d center = ring.Bound().Center;
        Seq<Move> descent = Helix(
            center,
            policy.RadiusMm,
            depth,
            Math.Max(1, (int)Math.Ceiling(depth / policy.PitchMm)),
            policy.ClearanceMm,
            feed);
        return descent.Exists(move => !ring.Covers(Target(move)))
            ? Fin.Fail<(Seq<Move>, Seq<Move>)>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "cam:helix-entry-outside").ToError())
            : Fin.Succ((descent, Seq<Move>()));
    }

    // One helical owner serves entry, thread milling, and interpolated boring; quarter arcs are exact on `Move.Circular`.
    internal static Seq<Move> Helix(
        Point3d center,
        double radius,
        double depth,
        int turns,
        double clearanceMm,
        double feed) =>
        Range(0, (turns * 4) + 1).Map(quarter =>
            new Point3d(
                center.X + (radius * Math.Cos(quarter * Math.PI * 0.5)),
                center.Y + (radius * Math.Sin(quarter * Math.PI * 0.5)),
                center.Z - Math.Min(depth, depth * quarter / (turns * 4.0))))
        .ToSeq() is var stations
            ? Seq<Move>(new Move.Rapid(AtZ(stations.Head, stations.Head.Z + clearanceMm))).Concat(
                stations.Tail.Map(station => (Move)new Move.Circular(
                    station,
                    feed,
                    new ArcCenter(new Point3d(center.X, center.Y, station.Z), RotationSense.Counterclockwise))))
            : Seq<Move>();

    private static Fin<Seq<CutElement>> Pocket(MotionRun run) =>
        run.Schedule.Traverse(pass =>
            toSeq(run.Input.Profiles).Traverse(profile =>
                Offset(
                    Seq(profile.AsCcw()),
                    -(run.Compensation + pass.RadialAllowanceMm),
                    run.Policy.Engagement.PlanarOffset).Bind(accessible =>
                        accessible.IsEmpty
                            ? Fin.Fail<Seq<CutElement>>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "cam:pocket-inaccessible").ToError())
                            : accessible.Traverse(region => Seed(
                                PartitionStrategy.PocketRegion,
                                region,
                                new PartitionProjection.Regions()).Bind(receipt =>
                                    receipt.Regions.Traverse(cell =>
                                        Rings(cell, run.Policy.Pass.StepOver, run.Policy.Engagement.PlanarOffset)
                                            .Bind(rings => rings.Traverse(ring => Element(
                                                run,
                                                AtDepth(
                                                    Perimeter(run, ring, run.Feed * pass.FeedScale, layer: 0),
                                                    pass.DepthMm - pass.FloorAllowanceMm)))))
                                    .Map(static cells => cells.Bind(identity))))
                            .Map(static regions => regions.Bind(identity))))
            .Map(static profiles => profiles.Bind(identity)))
        .Map(static passes => passes.Bind(identity));

    private static Fin<Seq<Loop>> Rings(
        Loop region,
        double stepOver,
        Rasm.Fabrication.Geometry2D.OffsetPolicy offset) =>
        stepOver <= 0.0
            ? Fin.Fail<Seq<Loop>>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "cam:pocket-stepover").ToError())
            : Range(0, RingCap(region, stepOver)).Fold(
                Fin.Succ((Rings: Seq(region), Frontier: Seq(region))),
                (state, _) => state.Bind(current => current.Frontier.IsEmpty
                    ? Fin.Succ(current)
                    : Offset(current.Frontier, -stepOver, offset)
                        .Map(next => (current.Rings.Concat(next), next))))
              .Map(static state => state.Rings);

    private static int RingCap(Loop region, double stepOver) =>
        (int)Math.Ceiling(region.Bound().Diagonal.Length / stepOver) + 1;

    // Cycle admission spans both directions: the tool must fit the measured bore and the bore must admit the tool.
    private static Fin<Seq<CutElement>> Holes(MotionRun run, HoleCycle cycle) =>
        run.StepDown <= 0.0
            ? Fin.Fail<Seq<CutElement>>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "cam:hole-stepdown").ToError())
            : Tops(run).Bind(targets => targets.Traverse(target =>
                !cycle.Fits(run.Policy.Cutter.Diameter, target.DiameterMm)
                    ? Fin.Fail<CutElement>(new GeometryFault.DegenerateInput(Kind.Curve, -1,
                        FormattableString.Invariant(
                            $"cam:hole-fit:{cycle.Key}:{run.Policy.Cutter.Diameter:R}:{target.DiameterMm:R}")).ToError())
                    : cycle.Expand(target, run.Policy.Engagement.Hole).Bind(moves => Element(
                        run,
                        moves,
                        cycle == HoleCycle.FineBore
                            ? Seq<MotionDirective>(new MotionDirective.OrientedStop(
                                moves.Count - 2,
                                new Vector3d(target.Radius - target.CutterRadiusMm, 0.0, run.Policy.Engagement.Hole.ClearanceMm)))
                            : Seq<MotionDirective>()))));

    private static Fin<Seq<HoleTarget>> Tops(MotionRun run) =>
        toSeq(run.Input.Profiles).Traverse(loop => HoleTop(run, loop)).As().Bind(measured =>
            run.Input.Model.Match(
                Some: model =>
                    from policy in SurfacePolicy(run)
                    from receipt in SurfacePath.Sample(
                        new SurfaceStrategy.DrillFamily(policy, measured.Map(static target => target.Top).ToArr()),
                        model,
                        run.Policy.Cutter)
                    from dropped in receipt.Elements
                        .Bind(static element => element.Variants)
                        .Bind(static variant => variant.Moves)
                        .Map(Target) is var tops && tops.Count == measured.Count
                            ? Fin.Succ(tops)
                            : Fin.Fail<Seq<Point3d>>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "cam:hole-drop-census").ToError())
                    select measured.Zip(dropped).Map(static row => row.Item1 with { Top = row.Item2 }),
                None: () => Fin.Succ(measured)));

    private static Fin<HoleTarget> HoleTop(MotionRun run, Loop loop) {
        Point3d center = loop.Bound().Center;
        Seq<double> radii = toSeq(loop.Vertices).Map(point => point.DistanceTo(center));
        (double Min, double Max) range = radii.Fold(
            (Min: double.PositiveInfinity, Max: 0.0),
            static (bounds, value) => (Math.Min(bounds.Min, value), Math.Max(bounds.Max, value)));
        return loop.Closed && radii.Count >= 3 && range.Min > 0.0 && range.Max - range.Min <= run.Chord
            ? Fin.Succ(new HoleTarget(
                center,
                range.Min + range.Max,
                Math.Min(run.Policy.Engagement.MaxAxialDepth, run.StepDown),
                run.Compensation,
                run.Feed))
            : Fin.Fail<HoleTarget>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "cam:hole-profile").ToError());
    }

    private static Fin<Seq<CutElement>> Sink(MotionRun run) =>
        toSeq(run.Input.Profiles).Traverse(loop =>
            HoleCycle.Peck.Expand(
                new HoleTarget(loop.Bound().Center, run.Compensation * 2.0, run.StepDown, run.Compensation, run.Feed),
                run.Policy.Engagement.Hole)
            .Bind(moves => Element(run, moves)));

    private static Fin<Seq<CutElement>> Tack(MotionRun run) =>
        toSeq(run.Input.Profiles).Traverse(loop =>
            loop.Bound().Center is var station
                ? Element(run, Seq(
                    (Move)new Move.Rapid(AtZ(station, station.Z + run.Policy.Engagement.Hole.ClearanceMm)),
                    new Move.Linear(station, run.Feed),
                    new Move.Rapid(AtZ(station, station.Z + run.Policy.Engagement.Hole.ClearanceMm))))
                : Fin.Fail<CutElement>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "cam:tack-station").ToError()));

    private static Fin<Seq<CutElement>> Fill(MotionRun run, int layer) =>
        toSeq(run.Input.Profiles).Traverse(loop =>
            from raster in Raster(run, loop, run.Policy.Pass.StepOver, layer)
            from partition in Seed(run.Policy.Engagement.Infill, loop, new PartitionProjection.Classify(raster))
            from elements in partition.Inside.Traverse(edge => Element(run, Seq(
                (Move)new Move.Rapid(edge.A),
                new Move.Linear(edge.B, run.Feed))))
            select elements)
        .Map(static profiles => profiles.Bind(identity));

    private static Fin<Seq<CutElement>> Adaptive(MotionRun run) =>
        toSeq(run.Input.Profiles).Traverse(loop =>
            from stock in ArcForest.Admit(Seq(loop), loop.Tolerance, loop.Plane).ToFin()
            from result in Offsetting.Apply(new OffsetOp.Medial(Ring(loop), Rasm.Meshing.OffsetPolicy.Canonical))
            from receipt in result is OffsetResult.Axis axis
                    ? SkeletonDemand.Admit(
                        stock,
                        axis.Medial,
                        run.Policy.Cutter,
                        run.Policy.Engagement,
                        run.Policy.Engagement.Sense,
                        run.Policy.Engagement.Walk).Bind(Skeleton.Walk)
                    : Fin.Fail<SkeletonReceipt>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "cam:medial-result").ToError())
            from elements in run.Schedule
                            .Traverse(pass => receipt.Elements.Traverse(element =>
                                AtDepth(element, pass.DepthMm - pass.FloorAllowanceMm)))
                            .Map(static passes => passes.Bind(identity))
            select elements)
        .Map(static profiles => profiles.Bind(identity));

    private static Fin<Seq<CutElement>> Turn(MotionRun run) =>
        from profile in toSeq(run.Input.Profiles).HeadOrNone().ToFin(new GeometryFault.DegenerateInput(Kind.Curve, -1, "cam:turn-profile").ToError())
        from cutting in run.Policy.Engagement.Cutting.ToFin(new GeometryFault.DegenerateInput(Kind.Curve, -1, "cam:turn-cutting-data").ToError())
        from _ in cutting.FeedBasis == FeedBasis.PerRevolution
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new GeometryFault.DegenerateInput(Kind.Curve, -1, $"cam:turn-feed-basis:{cutting.FeedBasis.Key}").ToError())
        from budget in run.Policy.Engagement.Budget is ProcessBudget.Turning turning
            ? Fin.Succ(turning)
            : Fin.Fail<ProcessBudget.Turning>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "cam:turn-budget").ToError())
        let policy = run.Policy.Engagement.Turning
        from demand in TurnDemand.Validate(
            profile,
            policy.Stock,
            policy.Insert,
            policy.Spindle,
            cutting,
            budget,
            policy.Motion,
            out TurnDemand turnDemand) is { } demandError
                ? Fin.Fail<TurnDemand>(new GeometryFault.DegenerateInput(Kind.Curve, -1, demandError.Message).ToError())
                : Fin.Succ(turnDemand)
        from steps in policy.Steps(run.Pair.Strategy)
        from request in TurnRequest.Validate(
            demand,
            steps,
            out TurnRequest turnRequest) is { } requestError
                ? Fin.Fail<TurnRequest>(new GeometryFault.DegenerateInput(Kind.Curve, -1, requestError.Message).ToError())
                : Fin.Succ(turnRequest)
        from program in Turning.Generate(request)
        from elements in program.Passes.Map((pass, index) => (pass, index)).Traverse(item => Element(
            run,
            item.pass.Moves,
            Directives(item.pass).Concat(item.index == 0
                ? program.Barriers.Map(static barrier => (MotionDirective)new MotionDirective.ChannelBarrier(
                    barrier.Step,
                    barrier.Channel.Key,
                    barrier.WaitFor.Map(static token => token.Value),
                    barrier.Signal.Map(static token => token.Value)))
                : Seq<MotionDirective>())))
        select elements;

    private static Seq<MotionDirective> Directives(TurnPass pass) {
        Seq<MotionDirective> executable = pass.Directives.Choose(static directive => directive switch {
            LatheDirective.Spindle row => Some<MotionDirective>(new MotionDirective.Spindle(
                row.Mode is SpindleMode.ConstantSurface ? SpindleControl.ConstantSurface : SpindleControl.ConstantRpm,
                row.SurfaceMetersPerMinute,
                row.ResolvedRpm)),
            LatheDirective.Dwell row => Some<MotionDirective>(new MotionDirective.Dwell(row.AfterMove, row.Revolutions)),
            LatheDirective.Synchronize row => Some<MotionDirective>(new MotionDirective.Synchronize(
                row.FromMove,
                row.ToMove,
                row.Rpm,
                row.Lead,
                row.Hand == ThreadHand.Right ? RotationSense.Clockwise : RotationSense.Counterclockwise)),
            _ => None,
        });
        Seq<SpecializedToolpathRow> evidence = pass.Directives.Choose(static directive => directive switch {
            LatheDirective.ThreadGeometry row => Some<SpecializedToolpathRow>(new SpecializedToolpathRow.TurningThread(
                row.Form.Key, row.LoadFlankDeg, row.ClearanceFlankDeg, row.CrestFlat, row.RootFlat,
                row.CrestRadius, row.RootRadius, row.Side.Key)),
            LatheDirective.AxialShape row => Some<SpecializedToolpathRow>(new SpecializedToolpathRow.TurningAxial(
                row.FromMove, row.ToMove, row.Kind.Key, row.Diameter, row.Depth, row.TipAngleDeg)),
            LatheDirective.TapShape row => Some<SpecializedToolpathRow>(new SpecializedToolpathRow.TurningTap(
                row.FromMove, row.ToMove, row.Diameter, row.Depth, row.Pitch, row.Form.Key, row.Hand.Key)),
            LatheDirective.Knurl row => Some<SpecializedToolpathRow>(new SpecializedToolpathRow.TurningKnurl(
                row.FromMove, row.ToMove, row.Pattern.Key, row.Pressure)),
            LatheDirective.Handoff row => Some<SpecializedToolpathRow>(new SpecializedToolpathRow.TurningHandoff(
                "handoff", row.From.Key, row.To.Key, row.GripPlane, row.GripLength, row.PullDistance)),
            LatheDirective.CutoffHandoff row => Some<SpecializedToolpathRow>(new SpecializedToolpathRow.TurningHandoff(
                "cutoff-handoff", row.From.Key, row.To.Key, row.GripPlane, row.GripLength, row.PullDistance)),
            _ => None,
        });
        return evidence.IsEmpty ? executable : executable.Add(new MotionDirective.Specialized(
            pass.Moves.IsEmpty ? -1 : pass.Moves.Count - 1,
            new SpecializedToolpathEnvelope(SpecializedToolpathKind.Turning, evidence, 0.0)));
    }

    private static Fin<Seq<CutElement>> Helical(MotionRun run, double lead) =>
        lead <= 0.0
            ? Fin.Fail<Seq<CutElement>>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "cam:helix-lead").ToError())
            : toSeq(run.Input.Profiles).Traverse(loop => {
                Point3d center = loop.Bound().Center;
                double radius = loop.Vertices.Min(point => point.DistanceTo(center)) - run.Compensation;
                int turns = Math.Max(1, run.Policy.Pass.Passes);
                return radius <= 0.0 || !double.IsFinite(radius)
                    ? Fin.Fail<CutElement>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "cam:helix-radius").ToError())
                    : Element(run, Helix(center, radius, lead * turns, turns, run.Policy.Engagement.Hole.ClearanceMm, run.Feed));
            });

    private static Fin<Seq<CutElement>> Layer(MotionRun run) =>
        run.StepDown <= 0.0 || run.Policy.Pass.StepOver <= 0.0 || !double.IsFinite(run.Policy.Pass.StepOver)
            ? Fin.Fail<Seq<CutElement>>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "cam:layer-geometry").ToError())
            : Range(0, Math.Max(1, run.Policy.Pass.Passes)).Traverse(layer =>
              toSeq(run.Input.Profiles).Traverse(loop =>
                from raster in Raster(run, loop, run.Policy.Pass.StepOver, layer)
                from partition in Seed(
                    run.Policy.Engagement.Infill,
                    loop,
                    new PartitionProjection.Classify(raster))
                from elements in PartitionElements(run, loop, partition, layer)
                select elements))
              .Map(static layers => layers.Bind(identity).Bind(identity));

    private static Fin<Seq<CutElement>> PartitionElements(
        MotionRun run,
        Loop loop,
        PartitionReceipt partition,
        int layer) {
        int seam = SeamIndex(run, loop, layer);
        Seq<Move> perimeter = Range(0, loop.Count + 1)
            .Map(index => LayerMove(loop.At(seam + index), layer, run.StepDown, run.Feed))
            .ToSeq();
        return from perimeterElement in Element(run, perimeter)
               from fill in partition.Inside.Traverse(edge => Element(run, Seq(
                   LayerMove(edge.A, layer, run.StepDown, run.Feed),
                   LayerMove(edge.B, layer, run.StepDown, run.Feed))))
               let contour = run.Pair.Strategy == CutStrategy.LayerInfill
                   ? Seq<CutElement>()
                   : Seq(perimeterElement)
               let infill = run.Pair.Strategy == CutStrategy.LayerContour
                   ? Seq<CutElement>()
                   : fill
               select contour.Concat(infill);
    }

    private static Move LayerMove(Point3d point, int layer, double height, double feed) =>
        new Move.Linear(AtZ(point, point.Z + (layer * height)), feed);

    // Per-layer angle advance breaks the inter-layer bond anisotropy a fixed raster direction bakes into the part.
    private static Fin<Seq<Edge3>> Raster(MotionRun run, Loop loop, double spacing, int layer) {
        if (!double.IsFinite(spacing) || spacing <= 0.0)
            return Fin.Fail<Seq<Edge3>>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "cam:raster-spacing").ToError());
        BoundingBox box = loop.Bound();
        Point3d pivot = box.Center;
        double angle = (run.Policy.Engagement.InfillAngleDeg
            + (layer * run.Policy.Engagement.InfillAngleAdvanceDeg)) * Math.PI / 180.0;
        double reach = box.Diagonal.Length * 0.5;
        int rows = Math.Max(1, (int)Math.Ceiling(box.Diagonal.Length / spacing)) + 1;
        Seq<Edge3> drives = Range(0, rows).Map(index => {
            double offset = -reach + (index * spacing);
            Point3d origin = new(
                pivot.X - (offset * Math.Sin(angle)),
                pivot.Y + (offset * Math.Cos(angle)),
                box.Min.Z);
            Vector3d along = new(Math.Cos(angle) * reach, Math.Sin(angle) * reach, 0.0);
            return index % 2 == 0
                ? new Edge3(origin - along, origin + along)
                : new Edge3(origin + along, origin - along);
        }).ToSeq();
        return PolygonAlgebra.Apply(new PolygonOp.ClipOpen(Seq(drives), Seq(loop), PolygonFill.NonZero))
            .Bind(trace => trace is PolygonTrace.SplitRuns split
                ? Fin.Succ(split.Inside.Bind(identity))
                : Fin.Fail<Seq<Edge3>>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "cam:raster-trace").ToError()));
    }

    private static Fin<Seq<Loop>> Offset(
        Seq<Loop> paths,
        double distance,
        Rasm.Fabrication.Geometry2D.OffsetPolicy policy) =>
        PolygonAlgebra.Apply(new PolygonOp.Offset(paths, new OffsetField.Uniform(distance), policy))
            .Bind(trace => trace is PolygonTrace.Regions regions
                ? Fin.Succ(regions.Result.Nodes.Map(static node => node.Boundary))
                : Fin.Fail<Seq<Loop>>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "cam:offset-trace").ToError()));

    private static Fin<PartitionReceipt> Seed(
        PartitionStrategy strategy,
        Loop boundary,
        PartitionProjection projection) =>
        PartitionRequest.Validate(strategy, boundary, projection, out PartitionRequest request) is { } error
            ? Fin.Fail<PartitionReceipt>(new GeometryFault.DegenerateInput(Kind.Curve, -1, error.Message).ToError())
            : Partition.Seed(request);

    private static Fin<Seq<CutElement>> Rest(MotionRun run) =>
        run.Input.Residual.ToFin(new GeometryFault.DegenerateInput(Kind.Curve, -1, "cam:rest-residual").ToError())
            .Bind(residual => Surface(run, policy => new SurfaceStrategy.Rest(policy, new SurfaceLayoutKind.PlanarRaster(), residual)));

    private static Fin<Seq<CutElement>> Kernel(MotionRun run, string key) =>
        from admitted in Key(key)
        from elements in Surface(run, policy => new SurfaceStrategy.Scallop(
            policy,
            new SurfaceLayoutKind.Kernel(admitted)))
        select elements;

    private static Fin<Seq<CutElement>> Trace(MotionRun run) =>
        toSeq(run.Input.Profiles).Traverse(loop =>
            Element(run, Perimeter(run, loop, run.Feed, layer: 0)));

    private static Fin<Seq<CutElement>> Extend(MotionRun run) =>
        run.Policy.Engagement.Generators.Find(run.Pair.Strategy)
            .ToFin(new GeometryFault.DegenerateInput(Kind.Curve, -1, $"cam:generator:{run.Pair.Strategy.Key}").ToError())
            .Bind(generator => generator(run));

    private static Fin<Seq<CutElement>> Surface(MotionRun run, Func<SurfacePolicy, SurfaceStrategy> strategy) =>
        from model in run.Input.Model.ToFin(new GeometryFault.DegenerateInput(Kind.Curve, -1, "cam:surface-model").ToError())
        from policy in SurfacePolicy(run)
        from receipt in SurfacePath.Sample(strategy(policy), model, run.Policy.Cutter)
        select receipt.Elements;

    private static Fin<SurfacePolicy> SurfacePolicy(MotionRun run) =>
        SurfacePolicy.Validate(
            run.Policy.Engagement,
            run.Policy.Engagement.Layout,
            out SurfacePolicy policy) is { } error
                ? Fin.Fail<SurfacePolicy>(new GeometryFault.DegenerateInput(Kind.Curve, -1, error.Message).ToError())
                : Fin.Succ(policy);

    private static Fin<SurfaceLayoutKey> Key(string raw) =>
        SurfaceLayoutKey.Validate(raw, provider: null, out SurfaceLayoutKey key) is { } error
            ? Fin.Fail<SurfaceLayoutKey>(new GeometryFault.DegenerateInput(Kind.Curve, -1, error.Message).ToError())
            : Fin.Succ(key);

    private static int SeamIndex(MotionRun run, Loop loop, int layer) =>
        Range(0, loop.Count).Fold(
            (Index: 0, Score: double.PositiveInfinity),
            (best, index) =>
                run.Policy.Engagement.Seam.Score(loop, run.Policy.Engagement.SeamReference, layer, index)
                    is var score && score < best.Score
                    ? (index, score)
                    : best).Index;

    private static Seq<Move> Perimeter(MotionRun run, Loop ring, double feed, int layer) {
        Loop ccw = ring.AsCcw();
        int seam = SeamIndex(run, ccw, layer);
        int sense = run.Policy.Engagement.Sense == CutSense.Climb ? 1 : -1;
        return Range(0, ccw.Count + 1)
            .Map(index => (Move)new Move.Linear(ccw.At(seam + (sense * index)), feed))
            .ToSeq();
    }

    private static Seq<Move> AtDepth(Seq<Move> moves, double depth) =>
        moves.Map(move => move.Switch(
            state: depth,
            rapid: static (value, row) => (Move)new Move.Rapid(AtZ(row.Target, row.Target.Z - value)),
            linear: static (value, row) => new Move.Linear(AtZ(row.Target, row.Target.Z - value), row.Feed),
            circular: static (value, row) => new Move.Circular(
                AtZ(row.Target, row.Target.Z - value),
                row.Feed,
                new ArcCenter(AtZ(row.Arc.Center, row.Arc.Center.Z - value), row.Arc.Sense))));

    private static Fin<CutElement> AtDepth(CutElement element, double depth) =>
        CutElement.Admit(
            element.Key,
            element.ToolKey,
            element.WorkOffset,
            AtDepth(element.Entry, depth));

    private static EntryFamily AtDepth(EntryFamily entry, double depth) =>
        entry.Switch<EntryFamily>(
            fixed: row => new EntryFamily.Fixed(AtDepth(row.Variant, depth)),
            reversible: row => new EntryFamily.Reversible(
                AtDepth(row.Forward, depth),
                AtDepth(row.Reverse, depth)),
            cyclic: row => new EntryFamily.Cyclic(
                row.Boundary,
                row.Samples,
                point => row.AtPoint(point).Map(variant => AtDepth(variant, depth))));

    private static ElementVariant AtDepth(ElementVariant variant, double depth) =>
        variant with {
            Entry = AtZ(variant.Entry, variant.Entry.Z - depth),
            Exit = AtZ(variant.Exit, variant.Exit.Z - depth),
            Moves = AtDepth(variant.Moves, depth),
        };

    // Endpoint-and-count keys collide across identical rings; the framed digest covers every station the element owns.
    private static Fin<CutElement> Element(MotionRun run, Seq<Move> moves, Seq<MotionDirective> directives = default) =>
        moves.IsEmpty
            ? Fin.Fail<CutElement>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "cam:element-empty").ToError())
            : moves.Head is var first && moves.Last.IfNone(first) is var last
            && ElementKey(run.Pair.Strategy, moves) is var key
            ? CutElement.Admit(
                key,
                run.Policy.Cutter.Evidence.Map(static evidence => evidence.ToolId).IfNone(run.Policy.Cutter.Family.Key),
                run.Policy.Engagement.WorkOffset,
                new EntryFamily.Fixed(new ElementVariant(
                    key,
                    Target(first),
                    Target(last),
                    moves,
                    RotationPenalty: Rotation(moves),
                    ThermalExposure: ThermalExposure(run.Pair.Modality, moves),
                    Pierces: Pierces(moves),
                    Directives: directives)))
            : Fin.Fail<CutElement>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "cam:element-key").ToError());

    private static string ElementKey(CutStrategy strategy, Seq<Move> moves) {
        CanonicalWriter writer = new CanonicalWriter(0.0)
            .Ordinal(moves.Count)
            .String(strategy.Key);
        foreach (Move move in moves) {
            switch (move) {
                case Move.Rapid row:
                    Write(writer, 0, row.Target, 0.0, Point3d.Origin, 0.0);
                    break;
                case Move.Linear row:
                    Write(writer, 1, row.Target, row.Feed, Point3d.Origin, 0.0);
                    break;
                case Move.Circular row:
                    Write(writer, 2, row.Target, row.Feed, row.Arc.Center,
                        row.Arc.Sense == RotationSense.Clockwise ? -1.0 : 1.0);
                    break;
            }
        }
        return ContentHash.Of(writer.ToBytes().Span).ToString("x32", CultureInfo.InvariantCulture);
    }

    private static Unit Write(CanonicalWriter writer, int kind, Point3d target, double feed, Point3d center, double sense) {
        writer.Ordinal(kind)
            .Double(target.X)
            .Double(target.Y)
            .Double(target.Z)
            .Double(feed)
            .Double(center.X)
            .Double(center.Y)
            .Double(center.Z)
            .Double(sense);
        return unit;
    }

    private static double Rotation(Seq<Move> moves) => moves.Map(Target).Zip(moves.Map(Target).Skip(1))
        .Zip(moves.Map(Target).Skip(2))
        .Sum(row => Vector3d.VectorAngle(row.First.Second - row.First.First, row.Second - row.First.Second));

    private static double ThermalExposure(ProcessModality modality, Seq<Move> moves) =>
        modality == ProcessModality.Thermal
            ? moves.Zip(moves.Skip(1)).Sum(pair => pair.Second.Switch(
                rapid: static _ => 0.0,
                linear: row => pair.First is { } prior ? Target(prior).DistanceTo(row.Target) / row.Feed : 0.0,
                circular: row => CircularExposure(Target(pair.First), row)))
            : 0.0;

    private static double CircularExposure(Point3d from, Move.Circular move) {
        Vector3d start = from - move.Arc.Center;
        Vector3d end = move.Target - move.Arc.Center;
        start.Z = 0.0;
        end.Z = 0.0;
        double minor = Vector3d.VectorAngle(start, end);
        double cross = Vector3d.CrossProduct(start, end).Z;
        bool counterclockwise = move.Arc.Sense == RotationSense.Counterclockwise;
        double sweep = counterclockwise == cross >= 0.0 ? minor : 2.0 * Math.PI - minor;
        return end.Length * sweep / move.Feed;
    }

    private static int Pierces(Seq<Move> moves) => moves.Zip(moves.Skip(1))
        .Count(static pair => pair.First is Move.Rapid && pair.Second is not Move.Rapid);

    private static Point3d Target(Move move) =>
        move.Switch(
            rapid: static row => row.Target,
            linear: static row => row.Target,
            circular: static row => row.Target);

    private static Point3d AtZ(Point3d point, double z) => new(point.X, point.Y, z);

    private static Polyline Ring(Loop loop) {
        Loop ccw = loop.AsCcw();
        return new Polyline(ccw.Vertices.Add(ccw.Vertices[0]));
    }
}
```

```mermaid
---
config:
  theme: base
  look: classic
  layout: elk
  flowchart:
    curve: linear
    padding: 25
  themeVariables:
    darkMode: true
    fontFamily: "SF Mono, Menlo, Cascadia Mono, Segoe UI Mono, Consolas, monospace"
    useGradient: false
    dropShadow: "none"
    background: "#282A36"
    primaryColor: "#44475A"
    primaryTextColor: "#F8F8F2"
    primaryBorderColor: "#BD93F9"
    lineColor: "#FF79C6"
    textColor: "#F8F8F2"
    edgeLabelBackground: "#21222C"
    labelBackgroundColor: "#21222C"
  themeCSS: ".nodeLabel{font-size:13px;font-weight:500}.edgeLabel{font-size:12px;font-weight:500}.cluster-label .nodeLabel{font-size:13.5px;font-weight:700;letter-spacing:.08em}.edge-thickness-normal{stroke-width:2px}.edge-thickness-thick{stroke-width:3px}.edge-pattern-dashed,.edge-pattern-dotted{stroke-width:1.5px;stroke-dasharray:4 6}.node rect,.node circle,.node polygon,.node path,.node .outer-path{stroke-width:1.5px;filter:none!important}.cluster rect{stroke-width:1px!important;stroke-dasharray:5 4!important;filter:none!important}.marker path{transform:scale(.8);transform-origin:5px 5px}.marker circle{transform:scale(.48);transform-origin:5px 5px}.edgeLabel rect{transform-box:fill-box;transform-origin:center;transform:scale(1.1,1.2)}"
---
flowchart LR
  accTitle: Fabrication motion algebra
  accDescr: One engagement carrier resolves process physics, generates independently routable cutting elements, conditions every linked move, guards it fail closed, and lowers the result against admitted machine capability.
  Policy["EngagementPolicy full carrier"] --> Resolve["budget × modality Resolve"]
  Input["FabricationInput"] --> Run["MotionRun.Of"]
  Resolve --> Run
  Run --> Generate["total Cam.Generate"]
  Conditioning["EntryPolicy · ContourCompensation · IT allowance"] --> Generate
  Generate --> Elements["Seq&lt;CutElement&gt; preserving every island/path/span"]
  Elements --> Link["LinkDemand · carried objective · precedence · policy"]
  Link --> Workholding["Condition every move"]
  Workholding --> Guard["Guard.Check fail closed"]
  Guard --> Solve["RobotProgram · MachineTool · typed unmounted rejection"]
  Solve --> Motion["FabricationResult.Motion"]
  classDef primary fill:#44475A,stroke:#FF79C6,color:#F8F8F2
  classDef boundary fill:#282A36,stroke:#BD93F9,color:#F8F8F2
  classDef external fill:#8BE9FDBF,stroke:#8BE9FD,color:#282A36
  classDef data fill:#FFB86CBF,stroke:#FFB86C,color:#282A36
  class Policy,Input,Conditioning boundary
  class Resolve,Run,Generate,Link,Workholding,Guard primary
  class Elements,Motion data
  class Solve external
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
