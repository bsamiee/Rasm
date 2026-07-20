# [RASM_FABRICATION_BEND_SEQUENCE]

`BendSequence` owns executable press-brake planning from `UnfoldResult` to ordered `BendStep` instructions. Tooling, back-gauge, orientation, support, transformed-panel, tonnage, springback, and clearance evidence govern every accepted search transition; `BrakePolicy` admits the cell geometry, tool catalog, support limits, solver tolerances, search budget, and search cost once.

`BendSequence.Plan`, `FormPolicy`, `ProcessEnvelope.Brake`, `BendStep`, and `FlatPattern.Formed` preserve the forming wire. `PolygonAlgebra.Apply` owns section topology, and `ProcessEnvelope.Brake` remains the machine-capacity seam.

## [01]-[INDEX]

- [01]-[BEND_SEQUENCE]: Generated tooling and method families, physical panel-state evolution, accumulated feasibility evidence, finite best-first sequencing, and executable result projection.

## [02]-[BEND_SEQUENCE]

- Owner: `BendMethod` owns forming physics; `PunchKind` owns punch compatibility; `BrakeTool` owns admitted tool geometry; `SupportRule` owns handling limits; `PartPose` owns rigid orientation; `BrakePolicy` owns cell and search policy; `BendSequence` owns candidate evaluation and sequencing.
- Cases: `BendMethod` carries air, bottom, coin, hem, wipe, and fold behavior with a per-row forming-force law; `PunchKind` carries straight, acute, gooseneck, hemming, wiping, and radius turn windows beside each punch's nose-radius floor; `BrakeRejection` carries every failed admission column.
- Entry: `BendSequence.Plan(UnfoldResult, FormPolicy, ProcessEnvelope.Brake)` is the frozen polymorphic planning entry.
- Auto: Candidate generation spans the admitted tool catalog and physical bend-axis alignments; each bend resolves the tooling its `SheetForm` demands over the policy default; independent gauge, support, sweep-station, and candidate failures accumulate before one accepted transition rotates every descendant panel and enters the best-first frontier; `Brent.TryFindRoot` inverts the cubic elastic-recovery law over the loaded radius; blank weight and descendant closures resolve once for the whole search.
- Receipt: `Seq<BendStep>` preserves the frozen line, angle, radius, `K`, overbend, tonnage, and orientation wire; the search path retains tool, gauge, setup, support, transformed-panel, and clearance evidence until projection; frontier exhaustion alone returns `BendSequenceInfeasible`, and budget exhaustion returns a distinct fault carrying the unfinished frontier.
- Packages: `LanguageExt.Core`, `Thinktecture.Runtime.Extensions`, `MathNet.Numerics`, `UnitsNet`, `RhinoCommon`, the `Geometry2D` owner, and BCL `PriorityQueue<TElement, TPriority>` compose the surface.
- Growth: A method or punch family is one smart-enum row carrying its own force or nose law, a physical tool is catalog data, a setup derives from the live bend axis, and a feasibility dimension is one `BrakeRejection` case with one evidence column.
- Boundary: Forming owns sequence feasibility and evolving part geometry; flat development, machine capacity, polygon topology, process physics, posting text, and artifact identity remain at their canonical owners; punch body profile is `BrakeTool.ForbiddenSections` geometry, so `PunchKind` states only the turn window and nose-radius floor a section cannot. `PriorityQueue<TElement, TPriority>`, its canonical composite priority, structural dominance map, and frontier loop are the statement-kernel exemptions.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------------------------------------------------------
using System.Buffers.Binary;
using System.Text;
using CommunityToolkit.HighPerformance.Buffers;
using LanguageExt;
using LanguageExt.Common;
using MathNet.Numerics.RootFinding;
using Rasm.Domain;
using Rasm.Fabrication.Geometry2D;
using Rasm.Fabrication.Kinematics;
using Rasm.Fabrication.Process;
using Rhino.Geometry;
using Thinktecture;
using UnitsNet;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Forming;

// --- [TYPES] --------------------------------------------------------------------------------------------------------------------------------------
// Bottoming and air bending load a V-die (force scales as `t^2 / V`), coining loads the full contact width
// (force scales as `V`), and hemming flattens over the material thickness — one shared expression cannot carry
// all three, so each row owns its own force law beside its calibration factor.
[SmartEnum<string>]
public sealed partial class BendMethod {
    public static readonly BendMethod Air = new("air", 1.0, 1.0, 1.0,
        static (radius, opening) => Math.Max(radius, opening * 0.16),
        static (stress, thickness, length, opening) => stress * thickness * thickness * length / opening);
    public static readonly BendMethod Bottom = new("bottom", 3.5, 0.55, 0.65,
        static (radius, _) => radius,
        static (stress, thickness, length, opening) => stress * thickness * thickness * length / opening);
    public static readonly BendMethod Coin = new("coin", 7.0, 0.15, 0.2,
        static (radius, _) => radius,
        static (stress, _, length, opening) => stress * length * opening);
    public static readonly BendMethod Hem = new("hem", 5.5, 0.05, 0.25,
        static (radius, _) => radius,
        static (stress, thickness, length, _) => stress * length * thickness);
    public static readonly BendMethod Wipe = new("wipe", 2.5, 0.8, 0.8,
        static (radius, _) => radius,
        static (stress, thickness, length, opening) => stress * thickness * thickness * length / opening);
    public static readonly BendMethod Fold = new("fold", 1.8, 0.9, 0.9,
        static (radius, _) => radius,
        static (stress, thickness, length, opening) => stress * thickness * thickness * length / opening);

    public double TonnageFactor { get; }
    public double PenetrationFactor { get; }
    public double RecoveryFactor { get; }

    [UseDelegateFromConstructor]
    public partial double WorkingRadius(double requestedRadiusMm, double dieOpeningMm);

    [UseDelegateFromConstructor]
    public partial double FormingForce(double flowStressMpa, double thicknessMm, double lengthMm, double dieOpeningMm);
}

// Punch body profile is `BrakeTool.ForbiddenSections` geometry, so a punch row owns only what the section
// cannot state: the turn window it can reach and the forming radius its nose imposes on the work.
[SmartEnum<string>]
public sealed partial class PunchKind {
    public static readonly PunchKind Straight = new("straight", Angle.FromDegrees(180.0), Angle.FromDegrees(0.0), noseRadiusFactor: 0.0);
    public static readonly PunchKind Acute = new("acute", Angle.FromDegrees(180.0), Angle.FromDegrees(20.0), noseRadiusFactor: 0.0);
    public static readonly PunchKind Gooseneck = new("gooseneck", Angle.FromDegrees(180.0), Angle.FromDegrees(0.0), noseRadiusFactor: 0.0);
    public static readonly PunchKind Hemming = new("hemming", Angle.FromDegrees(180.0), Angle.FromDegrees(150.0), noseRadiusFactor: 0.0);
    public static readonly PunchKind Wiping = new("wiping", Angle.FromDegrees(135.0), Angle.FromDegrees(0.0), noseRadiusFactor: 0.0);
    public static readonly PunchKind Radius = new("radius", Angle.FromDegrees(180.0), Angle.FromDegrees(0.0), noseRadiusFactor: 2.0);

    public Angle MaximumTurn { get; }
    public Angle MinimumTurn { get; }
    public double NoseRadiusFactor { get; }

    public double MinimumNoseRadius(double thicknessMm) => NoseRadiusFactor * thicknessMm;
}

[SmartEnum<string>]
public sealed partial class SupportMode {
    public static readonly SupportMode None = new("none", 0.0);
    public static readonly SupportMode Front = new("front", 1.0);
    public static readonly SupportMode Back = new("back", 1.0);
    public static readonly SupportMode Follower = new("follower", 0.25);
    public static readonly SupportMode Robot = new("robot", 0.1);

    public double HandlingFactor { get; }
}

[ComplexValueObject]
public sealed partial class BrakeTool {
    public string Key { get; }
    public PunchKind Punch { get; }
    public Set<BendMethod> Methods { get; }
    public double DieOpeningMm { get; }
    public double NoseRadiusMm { get; }
    public double SegmentLengthMm { get; }
    public double CapacityKn { get; }
    public Arr<Loop> ForbiddenSections { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref string key,
        ref PunchKind punch,
        ref Set<BendMethod> methods,
        ref double dieOpeningMm,
        ref double noseRadiusMm,
        ref double segmentLengthMm,
        ref double capacityKn,
        ref Arr<Loop> forbiddenSections) =>
        validationError = !string.IsNullOrWhiteSpace(key) && punch is not null && !methods.IsEmpty
            && methods.ForAll(static method => method is not null)
            && double.IsFinite(dieOpeningMm) && dieOpeningMm > 0.0
            && double.IsFinite(noseRadiusMm) && noseRadiusMm >= 0.0
            && double.IsFinite(segmentLengthMm) && segmentLengthMm > 0.0
            && double.IsFinite(capacityKn) && capacityKn > 0.0
            && !forbiddenSections.IsEmpty && forbiddenSections.ForAll(static loop => loop is not null && loop.Closed)
                ? null
                : new ValidationError(message: "Brake tooling must carry compatible methods, physical section geometry, and finite capacity.");
}

[ComplexValueObject]
public sealed partial class SupportRule {
    public SupportMode Mode { get; }
    public double MaximumOverhangMm { get; }
    public double MaximumMomentNmm { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref SupportMode mode,
        ref double maximumOverhangMm,
        ref double maximumMomentNmm) =>
        validationError = mode is not null
            && double.IsFinite(maximumOverhangMm) && maximumOverhangMm >= 0.0
            && double.IsFinite(maximumMomentNmm) && maximumMomentNmm >= 0.0
                ? null
                : new ValidationError(message: "Support limits require one mode and finite non-negative overhang and moment bounds.");
}

[ComplexValueObject]
public sealed partial class BrakePolicy {
    public Arr<BrakeTool> Tools { get; }
    public Arr<Loop> CellForbiddenSections { get; }
    public Arr<SupportRule> Supports { get; }
    public double GaugeResolutionMm { get; }
    public double SectionClearanceMm { get; }
    public int SearchExpansions { get; }
    public int SweepStations { get; }
    public double RootAccuracyDeg { get; }
    public int RootIterations { get; }
    public double MaximumOverbendDeg { get; }
    public double GravityMps2 { get; }
    public double ReorientCost { get; }
    public double ToolChangeCost { get; }
    public double GaugeTravelCost { get; }
    public double HandlingCost { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Arr<BrakeTool> tools,
        ref Arr<Loop> cellForbiddenSections,
        ref Arr<SupportRule> supports,
        ref double gaugeResolutionMm,
        ref double sectionClearanceMm,
        ref int searchExpansions,
        ref int sweepStations,
        ref double rootAccuracyDeg,
        ref int rootIterations,
        ref double maximumOverbendDeg,
        ref double gravityMps2,
        ref double reorientCost,
        ref double toolChangeCost,
        ref double gaugeTravelCost,
        ref double handlingCost) =>
        validationError = !tools.IsEmpty && tools.ForAll(static tool => tool is not null)
            && tools.GroupBy(static tool => tool.Key).ForAll(static group => group.Count() == 1)
            && cellForbiddenSections.ForAll(static loop => loop is not null && loop.Closed)
            && tools.Bind(static tool => tool.ForbiddenSections).Concat(cellForbiddenSections)
                .Map(static loop => loop.Tolerance).Distinct().Count() <= 1
            && !supports.IsEmpty && supports.ForAll(static support => support is not null)
            && double.IsFinite(gaugeResolutionMm) && gaugeResolutionMm > 0.0
            && double.IsFinite(sectionClearanceMm) && sectionClearanceMm >= 0.0
            && searchExpansions > 0 && sweepStations >= 2
            && double.IsFinite(rootAccuracyDeg) && rootAccuracyDeg is > 0.0 and <= 1.0
            && rootIterations > 0 && double.IsFinite(maximumOverbendDeg) && maximumOverbendDeg > 0.0
            && double.IsFinite(gravityMps2) && gravityMps2 > 0.0
            && Seq(reorientCost, toolChangeCost, gaugeTravelCost, handlingCost)
                .ForAll(static value => double.IsFinite(value) && value >= 0.0)
                ? null
                : new ValidationError(message: "Brake policy must carry unique tools, cell solids, support limits, sweep, search, and solver resolution, and non-negative costs.");
}

[ComplexValueObject]
public sealed partial class PartPose {
    public Transform Placement { get; }
    public BendOrientation Orientation { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Transform placement,
        ref BendOrientation orientation) {
        Point3d origin = placement * Point3d.Origin;
        Vector3d x = (placement * new Point3d(1.0, 0.0, 0.0)) - origin;
        Vector3d y = (placement * new Point3d(0.0, 1.0, 0.0)) - origin;
        Vector3d z = (placement * new Point3d(0.0, 0.0, 1.0)) - origin;
        double tolerance = Math.Sqrt(double.BitIncrement(1.0) - 1.0);
        validationError = orientation is not null && origin.IsValid && x.IsValid && y.IsValid && z.IsValid
            && Math.Abs((x * x) - 1.0) <= tolerance && Math.Abs((y * y) - 1.0) <= tolerance && Math.Abs((z * z) - 1.0) <= tolerance
            && Math.Abs(x * y) <= tolerance && Math.Abs(x * z) <= tolerance && Math.Abs(y * z) <= tolerance
            && Math.Abs((Vector3d.CrossProduct(x, y) * z) - 1.0) <= tolerance
            ? null
            : new ValidationError(message: "Part pose must carry a finite rigid placement.");
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record BrakeRejection {
    private BrakeRejection() { }

    public sealed record Dependency(int Bend, Set<int> Missing) : BrakeRejection;
    public sealed record Direction(int Bend, PartPose Pose) : BrakeRejection;
    public sealed record Tool(int Bend, string ToolKey) : BrakeRejection;
    public sealed record Bed(int Bend, double RequiredMm, double AvailableMm) : BrakeRejection;
    public sealed record Tonnage(int Bend, double RequiredKn, double AvailableKn) : BrakeRejection;
    public sealed record GaugeMissing(int Bend) : BrakeRejection;
    public sealed record Gauge(int Bend, double RequiredMm, double TravelMm) : BrakeRejection;
    public sealed record Height(int Bend, double RequiredMm, double OpenMm) : BrakeRejection;
    public sealed record Flange(int Bend, double AvailableMm, double RequiredMm) : BrakeRejection;
    public sealed record Springback(int Bend, string ToolKey) : BrakeRejection;
    public sealed record Collision(int Bend, string ToolKey) : BrakeRejection;
    public sealed record Support(int Bend, double OverhangMm, double MomentNmm) : BrakeRejection;
    public sealed record Evaluation(int Bend, string ToolKey, Error Fault) : BrakeRejection;
}

// --- [OPERATIONS] ---------------------------------------------------------------------------------------------------------------------------------
public static class BendSequence {
    public static Fin<Seq<BendStep>> Plan(UnfoldResult unfold, FormPolicy policy, ProcessEnvelope.Brake envelope) =>
        unfold is null || policy is null || envelope is null || unfold.Flat.IsEmpty || unfold.Bends.IsEmpty
            ? Fin.Fail<Seq<BendStep>>(new GeometryFault.DegenerateInput(Kind.Brep, -1, "bend-sequence:input").ToError())
            : from _ in ValidEnvelope(envelope)
              from context in Prepare(unfold, policy.Brake)
              from result in Search(context, unfold, policy, envelope)
              select result;

    // `PriorityQueue`, the dominance map, and the expansion counters are the named statement kernel: a best-first
    // frontier cannot fold without materializing every unexpanded state, and the recursive spelling grows one
    // stack frame per expansion. Frontier exhaustion alone proves infeasibility; budget exhaustion is a distinct
    // fault carrying the unfinished frontier, because a computational limit never proves a physical one.
    private static Fin<Seq<BendStep>> Search(
        BrakeContext context,
        UnfoldResult unfold,
        FormPolicy policy,
        ProcessEnvelope.Brake envelope) {
        PriorityQueue<BrakeState, FrontierPriority> frontier = new(FrontierPriorityComparer.Instance);
        BrakeState start = BrakeState.Start(unfold.Evidence.Panels.Map(static panel => panel.Panel).Distinct().ToSeq());
        frontier.Enqueue(start, Priority(start, policy.Brake));
        Dictionary<SearchKey, double> best = [];
        int expanded = 0;
        int rejected = 0;
        while (frontier.TryDequeue(out BrakeState? state, out _) && state is not null) {
            if (state.Done.Count == unfold.Bends.Count)
                return Fin.Succ(state.Path.Map(static row => row.Step));
            SearchKey key = Key(state);
            if (best.TryGetValue(key, out double prior) && prior <= state.Cost)
                continue;
            if (expanded >= policy.Brake.SearchExpansions)
                return Fin.Fail<Seq<BendStep>>(new FabricationFault.BendSearchBudgetExceeded(expanded, frontier.Count).ToError());
            best[key] = state.Cost;
            expanded++;
            Fin<Seq<BrakeCandidate>> expansion =
                from rows in Candidates(state, unfold, policy)
                select rows.Map(row => Evaluate(
                    state, row.Bend, row.Tool, row.Pose, context, unfold, policy, envelope)
                    .Match(
                        Succ: identity,
                        Fail: error => new BrakeCandidate.Rejected(Seq<BrakeRejection>(
                            new BrakeRejection.Evaluation(row.Bend.Index, row.Tool.Key, error)))));
            if (expansion.IsFail)
                return expansion.Map(static _ => Seq<BendStep>());
            Seq<BrakeCandidate> evaluated = expansion.IfFail(Seq<BrakeCandidate>());
            rejected += evaluated.Fold(0, static (count, candidate) => count + candidate.Switch(
                accepted: static _ => 0,
                rejected: static row => row.Rejections.Count));
            evaluated.Bind(static candidate => candidate.Switch(
                    accepted: static row => Seq(row.State),
                    rejected: static _ => Seq<BrakeState>()))
                .Iter(next => frontier.Enqueue(next, Priority(next, policy.Brake)));
        }
        return Fin.Fail<Seq<BendStep>>(new FabricationFault.BendSequenceInfeasible(rejected, expanded).ToError());
    }

    // Blank weight and the per-bend descendant closure are state-invariant, so they resolve once instead of per
    // candidate: the inner loop evaluates them across every state, tool, and pose the frontier ever expands.
    private static Fin<BrakeContext> Prepare(UnfoldResult unfold, BrakePolicy policy) =>
        from measures in unfold.Evidence.Topology.Nodes
            .Traverse(node => node.Boundary.Apply(new ProfileOp.Measure())
                .Bind(static result => result is ProfileResult.Measure measure
                    ? Fin.Succ(Math.Abs(measure.SignedArea.SquareMillimeters))
                    : Fin.Fail<double>(new GeometryFault.DegenerateInput(Kind.Brep, -1, "bend-sequence:mass-result").ToError()))
                .ToValidation()).As().ToFin()
        let netArea = unfold.Evidence.Topology.Nodes.Zip(measures)
            .Fold(0.0, static (area, row) => area + (row.First.IsHole ? -row.Second : row.Second))
        let volumeM3 = Volume.FromCubicMillimeters(netArea * unfold.ThicknessMm).CubicMeters
        select new BrakeContext(
            volumeM3 * unfold.Material.Thermal.DensityKgM3 * policy.GravityMps2,
            unfold.Bends.Fold(
                HashMap<int, Set<int>>(),
                (held, bend) => held.Add(bend.Child, Descendants(unfold.Bends, Set(bend.Child)))));

    private static Fin<Seq<(BendLine Bend, BrakeTool Tool, PartPose Pose)>> Candidates(
        BrakeState state,
        UnfoldResult unfold,
        FormPolicy policy) =>
        unfold.Bends
            .Filter(bend => !state.Done.Contains(bend.Index))
            .Traverse(bend => Poses(state, bend)
                .Map(poses => poses.Bind(pose => policy.Brake.Tools.ToSeq()
                    .Map(tool => (Bend: bend, Tool: tool, Pose: pose))))
                .ToValidation())
            .As()
            .Map(static groups => groups.Bind(static group => group))
            .ToFin();

    private static Fin<BrakeCandidate> Evaluate(
        BrakeState state,
        BendLine bend,
        BrakeTool tool,
        PartPose pose,
        BrakeContext context,
        UnfoldResult unfold,
        FormPolicy policy,
        ProcessEnvelope.Brake envelope) => Overbend(bend, tool, unfold.Forming, policy)
        .Map(overbend => EvaluateResolved(state, bend, tool, pose, context, unfold, policy, envelope, overbend))
        .IfNone(Fin.Succ<BrakeCandidate>(new BrakeCandidate.Rejected(
            Seq<BrakeRejection>(new BrakeRejection.Springback(bend.Index, tool.Key)))));

    private static Fin<BrakeCandidate> EvaluateResolved(
        BrakeState state,
        BendLine bend,
        BrakeTool tool,
        PartPose pose,
        BrakeContext context,
        UnfoldResult unfold,
        FormPolicy policy,
        ProcessEnvelope.Brake envelope,
        double overbend) =>
        from evidence in (
            GaugeOf(state, bend, pose, unfold).ToValidation(),
            SupportOf(state, bend, pose, context, unfold, policy.Brake).ToValidation(),
            SweepOf(
                state,
                bend,
                pose,
                overbend,
                context,
                unfold,
                policy.Brake,
                tool.ForbiddenSections.ToSeq().Concat(policy.Brake.CellForbiddenSections)).ToValidation())
            .Apply(static (gauge, support, sweep) => (Gauge: gauge, Support: support, Sweep: sweep))
            .As()
            .ToFin()
        let gaugeContact = evidence.Gauge
        let support = evidence.Support
        let sweep = evidence.Sweep
        from flange in FlangeWidth(state, bend, pose, unfold)
        let length = bend.Line.A.DistanceTo(bend.Line.B)
        let method = Tooling(bend, policy).Method
        let workingRadius = WorkingRadius(method, tool, bend, unfold.ThicknessMm)
        let tonnage = Tonnage(unfold.Forming, unfold.ThicknessMm, length, tool.DieOpeningMm, method)
        let minimumFlange = MinimumFlange(tool, unfold.ThicknessMm, workingRadius, method)
        let capacity = Math.Min(envelope.CapacityKn, tool.CapacityKn)
        let missing = bend.Prerequisites.Filter(prerequisite => !state.Done.Contains(prerequisite))
        let supportDemand = support.Demand
        let rejections = Seq<Option<BrakeRejection>>(
                missing.IsEmpty ? None : Some<BrakeRejection>(new BrakeRejection.Dependency(bend.Index, missing)),
                DirectionFormable(bend, pose) ? None : Some<BrakeRejection>(new BrakeRejection.Direction(bend.Index, pose)),
                Compatible(tool, policy, bend, length) ? None : Some<BrakeRejection>(new BrakeRejection.Tool(bend.Index, tool.Key)),
                length <= envelope.BedLengthMm ? None : Some<BrakeRejection>(new BrakeRejection.Bed(bend.Index, length, envelope.BedLengthMm)),
                tonnage <= capacity ? None : Some<BrakeRejection>(new BrakeRejection.Tonnage(bend.Index, tonnage, capacity)),
                gaugeContact.IsNone
                    ? Some<BrakeRejection>(new BrakeRejection.GaugeMissing(bend.Index))
                    : gaugeContact.Filter(row => row.XMm > envelope.GaugeTravelMm)
                        .Map<BrakeRejection>(row => new BrakeRejection.Gauge(bend.Index, row.XMm, envelope.GaugeTravelMm)),
                sweep.MaximumHeightMm <= envelope.OpenHeightMm ? None : Some<BrakeRejection>(new BrakeRejection.Height(bend.Index, sweep.MaximumHeightMm, envelope.OpenHeightMm)),
                flange >= minimumFlange ? None : Some<BrakeRejection>(new BrakeRejection.Flange(bend.Index, flange, minimumFlange)),
                sweep.MinimumClearanceMm.ForAll(clearance => clearance > policy.Brake.SectionClearanceMm)
                    ? None
                    : Some<BrakeRejection>(new BrakeRejection.Collision(bend.Index, tool.Key)),
                support.Accepted.IsSome ? None : Some<BrakeRejection>(new BrakeRejection.Support(bend.Index, supportDemand.OverhangMm, supportDemand.MomentNmm)))
            .Choose(identity)
            .ToSeq()
        let ready = (gaugeContact, support.Accepted)
            .Apply(static (gauge, mode) => (Gauge: gauge, Mode: mode))
            .As()
        from accepted in rejections.IsEmpty
            ? from row in ready.ToFin(new GeometryFault.DegenerateInput(Kind.Brep, -1, "bend-sequence:accepted-evidence").ToError())
              from next in Advance(state, bend, tool, pose, row.Gauge, row.Mode, overbend, tonnage, sweep, context, unfold, policy, envelope)
              select Some(next)
            : Fin.Succ(Option<BrakeState>.None)
        select accepted.Map<BrakeCandidate>(static value => new BrakeCandidate.Accepted(value))
            .IfNone(new BrakeCandidate.Rejected(rejections));

    private static Fin<BrakeState> Advance(
        BrakeState state,
        BendLine bend,
        BrakeTool tool,
        PartPose pose,
        GaugeStop gauge,
        SupportMode support,
        double overbend,
        double tonnage,
        SweepWitness sweep,
        BrakeContext context,
        UnfoldResult unfold,
        FormPolicy policy,
        ProcessEnvelope.Brake envelope) {
        Set<int> descendants = context.Below(bend.Child);
        return from active in Model(state, bend.Parent, bend.Line)
        let axis = (active.B - active.A) / active.A.DistanceTo(active.B)
        let rotation = Transform.Rotation(
            Angle.FromDegrees(bend.AngleDeg + Math.CopySign(overbend, bend.AngleDeg)).Radians, axis, active.A)
        let transforms = state.PanelTransforms.Fold(
            HashMap<int, Transform>(),
            (held, row) => held.Add(row.Key, descendants.Contains(row.Key) ? rotation * row.Value : row.Value))
        let length = bend.Line.A.DistanceTo(bend.Line.B)
        let witness = new BrakeWitness(
            Math.Min(envelope.CapacityKn, tool.CapacityKn) - tonnage,
            tool.SegmentLengthMm - length,
            envelope.GaugeTravelMm - gauge.XMm,
            envelope.OpenHeightMm - sweep.MaximumHeightMm,
            sweep.MinimumClearanceMm,
            transforms.OrderBy(static row => row.Key).Map(static row => row.Value).ToArr())
        let step = new BendStep(
            state.Path.Count + 1,
            bend.Line,
            bend.AngleDeg,
            WorkingRadius(Tooling(bend, policy).Method, tool, bend, unfold.ThicknessMm),
            bend.K,
            overbend,
            tonnage,
            pose.Orientation)
        let cost = state.Cost
            + (state.Setup == pose ? 0.0 : policy.Brake.ReorientCost)
            + (state.ToolKey == tool.Key ? 0.0 : policy.Brake.ToolChangeCost)
            + (Math.Abs(state.GaugeX - gauge.XMm) * policy.Brake.GaugeTravelCost)
            + (support.HandlingFactor * policy.Brake.HandlingCost)
        select new BrakeState(
            state.Done.Add(bend.Index),
            transforms,
            pose,
            tool.Key,
            gauge.XMm,
            state.Path.Add(new PlannedBend(step, bend.Index, tool.Key, gauge, pose, support, witness)),
            cost);
    }

    private static Fin<Option<GaugeStop>> GaugeOf(BrakeState state, BendLine bend, PartPose pose, UnfoldResult unfold) {
        Fin<Seq<Point3d>> points = unfold.Evidence.Panels.Traverse(panel => panel.Boundary.Vertices
                .Traverse(point => World(state, panel.Panel, pose, point).ToValidation()).As().ToFin()
                .ToValidation()).As().ToFin()
            .Map(static rows => rows.Bind(static row => row));
        return from resolved in points
        from active in Active(state, bend, pose)
        let axis = active.B - active.A
        let reach = Vector3d.CrossProduct(axis, Vector3d.ZAxis)
        from _ in axis.Unitize() && reach.Unitize() && !resolved.IsEmpty
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new GeometryFault.DegenerateInput(Kind.Brep, -1, "bend-sequence:gauge").ToError())
        select resolved.Map(point => (
                Point: point,
                X: (point - active.A) * reach,
                R: (point - active.A) * axis,
                Z: point.Z))
            .Filter(static row => row.X >= 0.0)
            .OrderByDescending(static row => row.X)
            .HeadOrNone()
            .Map(static row => new GaugeStop(row.X, row.R, row.Z, row.Point));
    }

    private static Fin<SupportEvidence> SupportOf(
        BrakeState state,
        BendLine bend,
        PartPose pose,
        BrakeContext context,
        UnfoldResult unfold,
        BrakePolicy policy) {
        return from pointRows in unfold.Evidence.Panels.Traverse(panel => panel.Boundary.Vertices
                .Traverse(point => World(state, panel.Panel, pose, point).ToValidation()).As().ToFin()
                .ToValidation()).As().ToFin()
               let points = pointRows.Bind(static row => row)
               from active in Active(state, bend, pose)
               let overhang = points.Map(point => point.DistanceTo(Closest(active, point))).Fold(0.0, Math.Max)
               let moment = context.WeightNewtons * overhang
               let evidence = policy.Supports
            .Filter(row => overhang <= row.MaximumOverhangMm && moment <= row.MaximumMomentNmm)
            .OrderBy(static row => row.Mode.HandlingFactor)
            .ThenBy(static row => row.Mode.Key, StringComparer.Ordinal)
            .HeadOrNone()
            .Map<SupportEvidence>(row => new SupportEvidence.Supported(row.Mode, overhang, moment))
            .IfNone(new SupportEvidence.Unsupported(overhang, moment))
               select evidence;
    }

    private static Fin<SweepWitness> SweepOf(
        BrakeState state,
        BendLine bend,
        PartPose pose,
        double overbend,
        BrakeContext context,
        UnfoldResult unfold,
        BrakePolicy policy,
        Seq<Loop> forbidden) {
        Set<int> descendants = context.Below(bend.Child);
        double command = bend.AngleDeg + Math.CopySign(overbend, bend.AngleDeg);
        return from active in Model(state, bend.Parent, bend.Line)
        let axis = active.B - active.A
        from witness in axis.Unitize()
            ? toSeq(Enumerable.Range(0, policy.SweepStations)).Traverse(station => {
                    double fraction = station / (double)(policy.SweepStations - 1);
                    Transform sweep = Transform.Rotation(Angle.FromDegrees(command * fraction).Radians, axis, active.A);
                    return (from sections in MaterialSections(state, bend, pose, unfold, descendants, sweep)
                            from clearance in Clearance(sections, forbidden)
                            from height in CandidateHeight(state, pose, unfold, descendants, sweep)
                            select new SweepWitness(clearance, height))
                        .ToValidation();
                }).As().ToFin().Bind(static witnesses => (
                        witnesses.Map(static row => row.MaximumHeightMm).Head,
                        Some(witnesses.Map(static row => row.MinimumClearanceMm).Choose(identity).ToSeq()))
                    .Apply(static (seed, clearances) => new SweepWitness(
                        clearances.Head.Map(low => clearances.Fold(low, Math.Min)),
                        witnesses.Map(static row => row.MaximumHeightMm).Fold(seed, Math.Max)))
                    .As()
                    .ToFin(new GeometryFault.DegenerateInput(Kind.Brep, -1, "bend-sequence:sweep-stations").ToError()))
            : Fin.Fail<SweepWitness>(new GeometryFault.DegenerateInput(Kind.Brep, -1, "bend-sequence:sweep-axis").ToError())
        select witness;
    }

    private static Fin<Seq<Loop>> MaterialSections(
        BrakeState state,
        BendLine bend,
        PartPose pose,
        UnfoldResult unfold,
        Set<int> descendants,
        Transform sweep) =>
        from active in Active(state, bend, pose)
        from frame in SectionOf(active)
        from panels in unfold.Evidence.Panels.Traverse(panel => {
                Fin<Seq<Loop>> sections = from resolved in panel.Boundary.Vertices
                    .Traverse(point => CandidateWorld(state, panel.Panel, pose, point, descendants, sweep)
                        .Map(frame.Project).ToValidation()).As().ToFin()
                    let projected = resolved.ToArr()
                    let edges = toSeq(Enumerable.Range(0, projected.Count))
                        .Map(index => (A: projected[index], B: projected[(index + 1) % projected.Count]))
                    let signedArea = edges.Fold(0.0, static (area, edge) =>
                        area + ((edge.A.X * edge.B.Y) - (edge.B.X * edge.A.Y))) / 2.0
                    let tolerance = unfold.Flat[0].Tolerance
                    from bars in edges
                    .Filter(edge => edge.A.DistanceTo(edge.B) > tolerance.Absolute.Value)
                    .Traverse(edge => SectionBar(edge.A, edge.B, unfold.ThicknessMm, tolerance).ToValidation()).As().ToFin()
                    from face in Math.Abs(signedArea) <= tolerance.Absolute.Value * tolerance.Absolute.Value
                        ? Fin.Succ(Seq<Loop>())
                        : Loop.Admit(projected, closed: true, Arr<double>(), tolerance).Map(Seq)
                    select face.Concat(bars);
                return sections.ToValidation();
            }).As().ToFin()
        select panels.Bind(static panel => panel);

    private static Fin<Loop> SectionBar(Point3d a, Point3d b, double thicknessMm, Context tolerance) {
        Vector3d along = b - a;
        if (!along.Unitize())
            return Fin.Fail<Loop>(new GeometryFault.DegenerateInput(Kind.Brep, -1, "bend-sequence:section-edge").ToError());
        Vector3d normal = new(-along.Y, along.X, 0.0);
        Vector3d half = normal * thicknessMm / 2.0;
        return Loop.Admit(Arr(a - half, b - half, b + half, a + half), closed: true, Arr<double>(), tolerance);
    }

    private static Fin<Option<double>> Clearance(Seq<Loop> material, Seq<Loop> forbidden) =>
        forbidden.IsEmpty
            ? Fin.Succ(Option<double>.None)
            : from trace in PolygonAlgebra.Apply(new PolygonOp.Boolean(material, forbidden, PolygonBoolean.Intersection, PolygonFill.NonZero))
              from clear in trace is PolygonTrace.Regions regions && regions.Result.Nodes.IsEmpty
                ? from materialToForbidden in BoundaryClearance(material, forbidden)
                  from forbiddenToMaterial in BoundaryClearance(forbidden, material)
                  select Math.Min(materialToForbidden, forbiddenToMaterial)
                : Fin.Succ(0.0)
              select Some(clear);

    private static Fin<double> BoundaryClearance(Seq<Loop> source, Seq<Loop> target) =>
        source.Bind(static loop => loop.Vertices)
            .Traverse(point => target.Traverse(loop => loop.Apply(new ProfileOp.Closest(point))
                    .Bind(static result => result is ProfileResult.Closest closest
                        ? Fin.Succ(closest.Value.Distance)
                        : Fin.Fail<double>(new GeometryFault.DegenerateInput(Kind.Brep, -1, "bend-sequence:clearance-result").ToError()))
                    .ToValidation()).As()
                .Map(static distances => distances.Head.Map(low => distances.Fold(low, Math.Min)))).As()
            .ToFin()
            .Map(static rows => rows.Choose(identity).ToSeq())
            .Bind(static nearest => nearest.Head
                .Map(low => nearest.Fold(low, Math.Min))
                .ToFin(new GeometryFault.DegenerateInput(Kind.Brep, -1, "bend-sequence:clearance-empty").ToError()));

    // Elastic recovery follows loaded radius, not commanded angle: with the neutral-fibre arc conserved,
    // Loaded radius falls as the command opens, so the recovered angle is `command * (4r^3 - 3r + 1)` over the
    // normalized elastic index `r = R * yield / (E * t)`. That relation is cubic in a reciprocal of the command and
    // has no closed inverse, which is what puts the bracketed root find on the rail.
    private static Option<double> Overbend(BendLine bend, BrakeTool tool, ProcessBudget.Formed forming, FormPolicy policy) {
        double target = Math.Abs(bend.AngleDeg);
        double fibre = bend.K * policy.ThicknessMm;
        double arc = Angle.FromDegrees(target).Radians
            * (WorkingRadius(Tooling(bend, policy).Method, tool, bend, policy.ThicknessMm) + fibre);
        double elastic = Tooling(bend, policy).Method.RecoveryFactor * forming.SpringbackRatio
            * policy.Material.Mechanical.YieldStrengthMpa
            / (policy.Material.Mechanical.ElasticModulusMpa * policy.ThicknessMm);
        return Brent.TryFindRoot(
            command => Recovered(command) - target,
            target,
            target + policy.Brake.MaximumOverbendDeg,
            policy.Brake.RootAccuracyDeg,
            policy.Brake.RootIterations,
            out double commanded)
                ? Some(commanded - target)
                : None;

        double Recovered(double commandDeg) {
            double index = elastic * ((arc / Angle.FromDegrees(commandDeg).Radians) - fibre);
            return commandDeg * ((4.0 * index * index * index) - (3.0 * index) + 1.0);
        }
    }

    // A line form whose geometry demands dedicated tooling overrides the policy default, so one part mixes hemmed,
    // curled, and ordinary bends without a second policy.
    private static (BendMethod Method, PunchKind Punch) Tooling(BendLine bend, FormPolicy policy) =>
        bend.Form.Tooling.IfNone((policy.Method, policy.Punch));

    private static bool Compatible(BrakeTool tool, FormPolicy policy, BendLine bend, double lengthMm) =>
        Tooling(bend, policy) is var demand
            && tool.Punch == demand.Punch && tool.Methods.Contains(demand.Method) && tool.SegmentLengthMm >= lengthMm
            && Math.Abs(bend.AngleDeg) >= tool.Punch.MinimumTurn.Degrees
            && Math.Abs(bend.AngleDeg) <= tool.Punch.MaximumTurn.Degrees
            && policy.DieWidthFactor.ForAll(factor => Math.Abs(tool.DieOpeningMm - (factor * policy.ThicknessMm)) <= tool.DieOpeningMm / factor);

    private static double WorkingRadius(BendMethod method, BrakeTool tool, BendLine bend, double thicknessMm) =>
        Math.Max(
            Math.Max(tool.NoseRadiusMm, tool.Punch.MinimumNoseRadius(thicknessMm)),
            method.WorkingRadius(bend.InsideRadiusMm, tool.DieOpeningMm));

    private static bool DirectionFormable(BendLine bend, PartPose pose) =>
        pose.Orientation == BendOrientation.Flipped ? bend.AngleDeg < 0.0 : bend.AngleDeg > 0.0;

    private static double Tonnage(
        ProcessBudget.Formed forming,
        double thicknessMm,
        double lengthMm,
        double dieOpeningMm,
        BendMethod method) =>
        Force.FromNewtons(method.TonnageFactor
            * method.FormingForce(forming.FlowStressMpa, thicknessMm, lengthMm, dieOpeningMm)).Kilonewtons;

    private static double MinimumFlange(BrakeTool tool, double thicknessMm, double radiusMm, BendMethod method) =>
        (tool.DieOpeningMm * method.PenetrationFactor) + radiusMm + thicknessMm;

    private static Fin<double> FlangeWidth(BrakeState state, BendLine bend, PartPose pose, UnfoldResult unfold) =>
        from active in Active(state, bend, pose)
        from distances in unfold.Evidence.Panels.Filter(panel => panel.Panel == bend.Child)
            .Bind(static panel => panel.Boundary.Vertices)
            .Traverse(point => World(state, bend.Child, pose, point)
                .Map(world => world.DistanceTo(Closest(active, world))).ToValidation()).As().ToFin()
        select distances.Fold(0.0, Math.Max);

    private static Fin<double> CandidateHeight(
        BrakeState state,
        PartPose pose,
        UnfoldResult unfold,
        Set<int> descendants,
        Transform sweep) {
        return from rows in unfold.Evidence.Panels.Traverse(panel => panel.Boundary.Vertices
                .Traverse(point => CandidateWorld(state, panel.Panel, pose, point, descendants, sweep)
                    .Map(static world => world.Z).ToValidation()).As().ToFin()
                .ToValidation()).As().ToFin()
        let z = rows.Bind(static row => row)
        from height in z.Head
            .Map(seed => z.Fold(seed, Math.Max) - z.Fold(seed, Math.Min))
            .ToFin(new GeometryFault.DegenerateInput(Kind.Brep, -1, "bend-sequence:height").ToError())
        select height;
    }

    private static Fin<SectionFrame> SectionOf(Edge3 bend) {
        Vector3d axis = bend.B - bend.A;
        Vector3d radial = Vector3d.CrossProduct(axis, Vector3d.ZAxis);
        return axis.Unitize() && radial.Unitize()
            ? Fin.Succ(new SectionFrame(bend.A, radial))
            : Fin.Fail<SectionFrame>(new GeometryFault.DegenerateInput(Kind.Brep, -1, "bend-sequence:section-frame").ToError());
    }

    private static Fin<Point3d> World(BrakeState state, int panel, PartPose pose, Point3d point) =>
        Model(state, panel, point).Map(model => pose.Placement * model);

    private static Fin<Point3d> CandidateWorld(
        BrakeState state,
        int panel,
        PartPose pose,
        Point3d point,
        Set<int> descendants,
        Transform sweep) =>
        Model(state, panel, point)
            .Map(model => pose.Placement * (descendants.Contains(panel) ? sweep * model : model));

    private static Fin<Point3d> Model(BrakeState state, int panel, Point3d point) =>
        state.PanelTransforms.Find(panel)
            .Map(transform => transform * point)
            .ToFin(new GeometryFault.DegenerateInput(Kind.Brep, -1, $"bend-sequence:panel-transform:{panel}").ToError());

    private static Fin<Edge3> Model(BrakeState state, int panel, Edge3 edge) =>
        (Model(state, panel, edge.A), Model(state, panel, edge.B))
            .Apply(static (a, b) => new Edge3(a, b)).As();

    private static Fin<Edge3> Active(BrakeState state, BendLine bend, PartPose pose) =>
        (World(state, bend.Parent, pose, bend.Line.A), World(state, bend.Parent, pose, bend.Line.B))
            .Apply(static (a, b) => new Edge3(a, b)).As();

    private static Fin<Seq<PartPose>> Poses(BrakeState state, BendLine bend) {
        return from active in Model(state, bend.Parent, bend.Line)
        from poses in Poses(active)
        select poses;
    }

    private static Fin<Seq<PartPose>> Poses(Edge3 active) {
        Vector3d axis = active.B - active.A;
        Vector3d turn = Vector3d.CrossProduct(axis, Vector3d.XAxis);
        double angle = Vector3d.VectorAngle(axis, Vector3d.XAxis);
        if (!axis.Unitize() || !double.IsFinite(angle))
            return Fin.Fail<Seq<PartPose>>(new GeometryFault.DegenerateInput(Kind.Brep, -1, "bend-sequence:pose-axis").ToError());
        Transform alignment = angle <= Math.Sqrt(double.BitIncrement(1.0) - 1.0)
            ? Transform.Identity
            : turn.Unitize()
                ? Transform.Rotation(angle, turn, active.A)
                : Transform.Rotation(Math.PI, Vector3d.ZAxis, active.A);
        Transform aligned = Transform.Translation(new Vector3d(-active.A.X, -active.A.Y, -active.A.Z)) * alignment;
        Transform flipped = Transform.Rotation(Math.PI, Vector3d.XAxis, Point3d.Origin) * aligned;
        return (Pose(aligned, BendOrientation.AsIs), Pose(flipped, BendOrientation.Flipped))
            .Apply(static (asIs, inverted) => Seq(asIs, inverted))
            .As();
    }

    private static Fin<PartPose> Pose(Transform placement, BendOrientation orientation) =>
        PartPose.Validate(placement, orientation, out PartPose pose) is { } error
            ? Fin.Fail<PartPose>(new GeometryFault.DegenerateInput(Kind.Brep, -1, error.Message).ToError())
            : Fin.Succ(pose);

    private static Point3d Closest(Edge3 edge, Point3d point) =>
        new Line(edge.A, edge.B).ClosestPoint(point, limitToFiniteSegment: true);

    private static Set<int> Descendants(Seq<BendLine> bends, Set<int> seed) {
        Set<int> closure = seed.Union(bends.Filter(bend => seed.Contains(bend.Parent)).Map(static bend => bend.Child).ToSet());
        return closure.Count > seed.Count ? Descendants(bends, closure) : closure;
    }

    // Dominance keys every state component consumed by future feasibility and cost without frontier rounding.
    private static SearchKey Key(BrakeState state) =>
        new(
            state.Done,
            state.Setup.Orientation.Key,
            Exact(-1, state.Setup.Placement),
            state.ToolKey,
            state.GaugeX,
            new SearchGeometry(state.PanelTransforms
                .OrderBy(static row => row.Key)
                .Map(static row => Exact(row.Key, row.Value))
                .ToSeq()));

    private static ExactTransform Exact(int panel, Transform value) => new(panel, value);

    private static QuantizedTransform Quantized(
        int panel,
        Transform transform,
        double translationResolutionMm,
        double angularResolution) {
        Point3d origin = transform * Point3d.Origin;
        return new QuantizedTransform(
            panel,
            Vector(transform * new Point3d(1.0, 0.0, 0.0) - origin, angularResolution),
            Vector(transform * new Point3d(0.0, 1.0, 0.0) - origin, angularResolution),
            Vector(transform * new Point3d(0.0, 0.0, 1.0) - origin, angularResolution),
            new QuantizedVector(
                (long)Math.Round(origin.X / translationResolutionMm),
                (long)Math.Round(origin.Y / translationResolutionMm),
                (long)Math.Round(origin.Z / translationResolutionMm)));

        static QuantizedVector Vector(Vector3d value, double resolution) => new(
            (long)Math.Round(value.X / resolution),
            (long)Math.Round(value.Y / resolution),
            (long)Math.Round(value.Z / resolution));
    }

    private static FrontierPriority Priority(BrakeState state, BrakePolicy policy) {
        double angularResolution = Math.Sin(Angle.FromDegrees(policy.RootAccuracyDeg).Radians);
        using ArrayPoolBufferWriter<byte> writer = new();
        Write(writer, state.Done.Count);
        state.Done.Order().Iter(value => Write(writer, value));
        Write(writer, state.Setup.Orientation.Key);
        Write(writer, Quantized(-1, state.Setup.Placement, policy.GaugeResolutionMm, angularResolution));
        Write(writer, state.ToolKey);
        Write(writer, (long)Math.Round(state.GaugeX / policy.GaugeResolutionMm));
        Seq<QuantizedTransform> geometry = state.PanelTransforms
            .OrderBy(static row => row.Key)
            .Map(row => Quantized(row.Key, row.Value, policy.GaugeResolutionMm, angularResolution))
            .ToSeq();
        Write(writer, geometry.Count);
        geometry.Iter(row => Write(writer, row));
        Write(writer, state.Path.Count);
        state.Path.Iter(row => {
            Write(writer, row.BendIndex);
            Write(writer, row.ToolKey);
            Write(writer, row.Gauge.XMm); Write(writer, row.Gauge.RMm); Write(writer, row.Gauge.ZMm); Write(writer, row.Gauge.Contact);
            Write(writer, row.Setup.Orientation.Key);
            Write(writer, Quantized(-1, row.Setup.Placement, policy.GaugeResolutionMm, angularResolution));
            Write(writer, row.Support.Key);
            Write(writer, row.Witness.TonnageMarginKn); Write(writer, row.Witness.ToolLengthMarginMm);
            Write(writer, row.Witness.GaugeMarginMm); Write(writer, row.Witness.HeightMarginMm);
            Write(writer, row.Witness.MinimumClearanceMm.IsSome ? 1 : 0);
            row.Witness.MinimumClearanceMm.Iter(value => Write(writer, value));
            Write(writer, row.Witness.PanelTransforms.Count);
            toSeq(Enumerable.Range(0, row.Witness.PanelTransforms.Count)).Iter(index => Write(
                writer,
                Quantized(index, row.Witness.PanelTransforms[index], policy.GaugeResolutionMm, angularResolution)));
            Write(writer, row.Step.Order); Write(writer, row.Step.Line.A); Write(writer, row.Step.Line.B);
            Write(writer, row.Step.AngleDeg); Write(writer, row.Step.RadiusMm); Write(writer, row.Step.KFactor);
            Write(writer, row.Step.OverbendDeg); Write(writer, row.Step.TonnageKn); Write(writer, row.Step.Orientation.Key);
        });
        return new FrontierPriority(state.Cost, writer.WrittenSpan.ToArray());
    }

    private static void Write(ArrayPoolBufferWriter<byte> writer, QuantizedTransform value) {
        Write(writer, value.Panel);
        Write(writer, value.X); Write(writer, value.Y); Write(writer, value.Z); Write(writer, value.Origin);
    }

    private static void Write(ArrayPoolBufferWriter<byte> writer, QuantizedVector value) {
        Write(writer, value.X); Write(writer, value.Y); Write(writer, value.Z);
    }

    private static void Write(ArrayPoolBufferWriter<byte> writer, Point3d value) {
        Write(writer, value.X); Write(writer, value.Y); Write(writer, value.Z);
    }

    private static void Write(ArrayPoolBufferWriter<byte> writer, int value) {
        BinaryPrimitives.WriteInt32LittleEndian(writer.GetSpan(sizeof(int)), value);
        writer.Advance(sizeof(int));
    }

    private static void Write(ArrayPoolBufferWriter<byte> writer, long value) {
        BinaryPrimitives.WriteInt64LittleEndian(writer.GetSpan(sizeof(long)), value);
        writer.Advance(sizeof(long));
    }

    private static void Write(ArrayPoolBufferWriter<byte> writer, double value) {
        BinaryPrimitives.WriteDoubleLittleEndian(writer.GetSpan(sizeof(double)), value);
        writer.Advance(sizeof(double));
    }

    private static void Write(ArrayPoolBufferWriter<byte> writer, string value) {
        int count = Encoding.UTF8.GetByteCount(value);
        Write(writer, count);
        Span<byte> target = writer.GetSpan(count);
        writer.Advance(Encoding.UTF8.GetBytes(value, target));
    }

    private static Fin<Unit> ValidEnvelope(ProcessEnvelope.Brake envelope) =>
        Seq(envelope.CapacityKn, envelope.GaugeTravelMm, envelope.OpenHeightMm, envelope.BedLengthMm)
            .ForAll(static value => double.IsFinite(value) && value > 0.0)
                ? Fin.Succ(unit)
                : Fin.Fail<Unit>(new GeometryFault.DegenerateInput(Kind.Brep, -1, "bend-sequence:envelope").ToError());

    [Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
    private abstract partial record BrakeCandidate {
        private BrakeCandidate() { }

        public sealed record Accepted(BrakeState State) : BrakeCandidate;
        public sealed record Rejected(Seq<BrakeRejection> Rejections) : BrakeCandidate;
    }
    private readonly record struct GaugeStop(double XMm, double RMm, double ZMm, Point3d Contact);
    private sealed record BrakeWitness(
        double TonnageMarginKn,
        double ToolLengthMarginMm,
        double GaugeMarginMm,
        double HeightMarginMm,
        Option<double> MinimumClearanceMm,
        Arr<Transform> PanelTransforms);
    private sealed record PlannedBend(
        BendStep Step,
        int BendIndex,
        string ToolKey,
        GaugeStop Gauge,
        PartPose Setup,
        SupportMode Support,
        BrakeWitness Witness);
    private readonly record struct SweepWitness(Option<double> MinimumClearanceMm, double MaximumHeightMm);
    private readonly record struct SectionFrame(Point3d Origin, Vector3d Radial) {
        public Point3d Project(Point3d point) => new((point - Origin) * Radial, point.Z, 0.0);
    }
    [Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
    private abstract partial record SupportEvidence {
        private SupportEvidence() { }

        public sealed record Supported(SupportMode Mode, double OverhangMm, double MomentNmm) : SupportEvidence;
        public sealed record Unsupported(double OverhangMm, double MomentNmm) : SupportEvidence;

        public Option<SupportMode> Accepted => Switch(
            supported: static row => Some(row.Mode),
            unsupported: static _ => None);

        public (double OverhangMm, double MomentNmm) Demand => Switch(
            supported: static row => (row.OverhangMm, row.MomentNmm),
            unsupported: static row => (row.OverhangMm, row.MomentNmm));
    }
    private readonly record struct QuantizedVector(long X, long Y, long Z);
    private readonly record struct QuantizedTransform(
        int Panel,
        QuantizedVector X,
        QuantizedVector Y,
        QuantizedVector Z,
        QuantizedVector Origin);
    private sealed record ExactTransform(int Panel, Transform Value);
    private sealed record SearchGeometry(Seq<ExactTransform> Rows);
    private sealed record SearchKey(
        Set<int> Done,
        string Orientation,
        ExactTransform Setup,
        string ToolKey,
        double GaugeX,
        SearchGeometry Geometry);
    private readonly record struct FrontierPriority(double Cost, byte[] Canonical);

    private sealed class FrontierPriorityComparer : IComparer<FrontierPriority> {
        public static readonly FrontierPriorityComparer Instance = new();

        public int Compare(FrontierPriority left, FrontierPriority right) {
            int order = left.Cost.CompareTo(right.Cost);
            return order != 0 ? order : left.Canonical.AsSpan().SequenceCompareTo(right.Canonical);
        }
    }

    private sealed record BrakeContext(double WeightNewtons, HashMap<int, Set<int>> Descendants) {
        public Set<int> Below(int child) => Descendants.Find(child).IfNone(Set(child));
    }

    private sealed record BrakeState(
        Set<int> Done,
        HashMap<int, Transform> PanelTransforms,
        PartPose Setup,
        string ToolKey,
        double GaugeX,
        Seq<PlannedBend> Path,
        double Cost) {
        public static BrakeState Start(Seq<int> panels) => new(
            Set<int>(),
            panels.Map(static panel => (panel, Transform.Identity)).ToHashMap(),
            OriginPose(),
            string.Empty,
            0.0,
            Seq<PlannedBend>(),
            0.0);

        private static PartPose OriginPose() => PartPose.Create(Transform.Identity, BendOrientation.AsIs);
    }
}
```
