# [RASM_FABRICATION_PROGRAM_OPTIMIZATION]

`Optimize` owns one admitted `CutProgram` transformation. `OptimizationIngress` admits domain evidence or an existing policy, `OptimizePass` folds the selected concerns in declaration order, `Post.Interpret` supplies the one modal and spatial trace, and `OptimizationResult` returns the re-keyed program, pass evidence, and exact `Dialect.Emit` image together.

`MrrPolicy` composes `ProcessBudget.Subtractive`, `CuttingData`, and `MTConnect` controller ranges into dimensional feed decisions. `SmoothPolicy`, `CompactPolicy`, and `PatternPolicy` parameterize geometry preservation, modal compaction, and repeated-sequence factoring; no pass owns an implicit tolerance, label range, feed floor, or output convention.

## [01]-[INDEX]

- [01]-[ADMISSION]: `OptimizationIngress` selects raw evidence or an admitted policy.
- [02]-[OPTIMIZATION]: `Optimize.Apply` executes the selected generated pass rows and returns one result.
- [03]-[TRACE]: `Post.Interpret` derives effective position, feed, units, distance mode, and nested execution in one fold.
- [04]-[OBJECTIVE]: `MotionDynamics` turns the trace into accel-limited machine minutes over every motion, not cutting alone.
- [05]-[PASSES]: `OptimizationCore` owns feed adaptation, corner blending, compaction, and pattern folding.

## [02]-[ADMISSION]

- Owner: `OptimizePolicy` is the canonical policy; `OptimizationIngress.Raw` is the only raw admission case.
- Cases: `OptimizationIngress.Raw` composes process, cutting, controller, engagement, geometry, compaction, pattern, and kinematic evidence; `OptimizationIngress.Admitted` preserves an already admitted policy.
- Entry: `Optimize.Apply` consumes `CutProgram`, `OptimizationIngress`, and parameterized `OptimizationEgress`; `OptimizationEgress.Measurement` derives the final codec, termination, and framing with `BlockLimit.Observe`, while `Final` retains `BlockLimit.Enforce`.
- Auto: independent raw-policy failures accumulate through `Validation<Error, T>` before the `Fin<T>` execution rail; independent capability refusals accumulate the same way.
- Receipt: `OptimizePolicy` carries typed `UnitsNet` quantities past admission.
- Packages: `UnitsNet` supplies `Speed`, `RotationalSpeed`, `Length`, `Angle`, `Power`, and `Duration` constructors and canonical properties; `ProcessFeedRate.Minimum`/`Nominal`/`Maximum`/`Value` and `ProcessSpindleSpeed.Minimum`/`Nominal`/`Maximum`/`Value` supply controller bounds; `LanguageExt.Core` supplies `Validation<Error, T>`, applicative `Apply`, `Fin<T>`, `Fold`, `Bind`, `Choose`, `Zip`, and the equality-keyed `HashMap` carrier `BlockLocus` requires.
- Growth: one new evidence source extends `OptimizationIngress.Raw` and the existing admission fan-in.
- Boundary: raw dimensional doubles, controller-range copies, and page-local cutting-force equations never cross admission; the ordered `Map` carrier never keys on a `[ComplexValueObject]`, which owns structural equality and no comparer.

## [03]-[OPTIMIZATION]

- Owner: `OptimizePass` carries the declaration order and fold behavior; `OptimizationResult` is the sole egress.
- Cases: `mrr-feed`, `corner-smooth`, `compact`, and `pattern-fold` are generated rows over one `PassState`.
- Entry: `Optimize.Apply(CutProgram, OptimizationIngress, OptimizationEgress)` is the only public operation.
- Auto: baseline and optimized block counts come from `PostImage.PhysicalRecords`; `PassState` threads the interpretation forward so each pass interprets the program it produced exactly once; `Post.Lookahead` derives caps from the recursively interpreted motion trace and rewrites every block, macro, and subprogram locus before final emission.
- Receipt: `OptimizationReceipt` carries baseline and optimized duration, physical records, estimated-engagement count, folded-pattern count, and ordered `PassDelta` evidence.
- Packages: `Dialect.Emit` owns physical rendering; `Post.Interpret`, `Post.Lookahead`, `CutProgram.Of`, `GNode.Switch`, `GNode.Word.P`/`With`/`Without`, and `GParam.Number` own program semantics; `MotionDynamics.RapidFeed`/`LinearFeed`/`ArcFeed`/`Acceleration`/`JunctionFeed` own machine timing; `CuttingData.Evaluate(CutIntent)` supplies dimensional force, power, and removal evidence; `Thinktecture.Runtime.Extensions` generates pass rows and policy owners.
- Growth: one optimization concern adds one `OptimizePass` row and one pure `PassState` fold.
- Boundary: separate `Feeds`, `Delta`, `Blocks`, and renderer-shaped estimators are deleted forms; symbolic `GValue.Variable`/`Expression` motion fails admission because geometry-changing passes cannot preserve unevaluated coordinates by inspection.

## [04]-[TRACE]

`Post.Interpret` is the single semantic walk. Missing axes inherit the cursor, `G90` and `G91` alter coordinate interpretation, `F` remains modal, and every block, macro, repeated subprogram, and canned cycle enters with caller state and returns its final state.

`ProgramEvent.Motion.Cutting` is refined by the subtractive command family before feed rewriting, so `GCommand.Extrude` never enters material-removal logic. Arc rows preserve their admitted `I`/`J` center evidence, while distances and turns use full `Point3d` positions; no absent axis becomes zero and no zero-length span becomes fabricated distance.

## [05]-[OBJECTIVE]

Objective time counts every machine minute: rapids traverse at `MotionDynamics.RapidFeed`, feed motion is bounded by `LinearFeed` and `ArcFeed`, `FeedMode.InverseTime` reads one block as `1/F` minutes, and `GCommand.Dwell` contributes its `P` seconds for every commanded pause, including pierce delay. A cutting-only objective reports compaction and linking gains it never earned.

Each span is accel-limited: `MotionDynamics.JunctionFeed` fixes the shared speed at the turn between consecutive spans, and `Acceleration` bounds the reachable peak, so a fold of short segments cannot show its programmed feed. `MotionSpan` carries length, cruise ceiling, entry speed, and chord direction, and the trapezoid closes each span when its successor fixes the exit.

## [06]-[PASSES]

`mrr-feed` evaluates `CuttingData.Evaluate(CutIntent)` for each subtractive locus, applies radial chip thinning, intersects process, controller, and spindle-power limits, and writes one explicit effective `F`. Missing engagement evidence uses the policy estimator and increments receipt evidence. Tapping and threading cycles are excluded because their feed is bound to pitch and spindle speed.

`corner-smooth` runs only after `ProgramTrace` proves absolute `G17` motion with explicit `X`/`Y`/`Z` and the dialect admits center-form arcs, then applies one measured three-word array window, carries the corner's modal words onto the emitted arc, and emits the existing `GNode.Word` arc representation. `Geometry2D/arcs` remains the owner for subsequent arc inspection, offset, and densification.

`compact` uses the same admitted geometry, folds forward-collinear rapid and feed runs and co-circular arc runs while preserving traversed locus, and strips repeated modal `F` and `S` values. A nested body executes under state the parent fold cannot see, so the modal census clears at every non-word node rather than stripping a word the body already changed.

`pattern-fold` selects the maximal positive structural saving after call and definition overhead, replaces occurrences with one `GNode.Subprogram`, and repeats until no profitable sequence remains; generated labels come only from `PatternPolicy`, and `Dialect.Emit` hoists one definition per label regardless of call-site count.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------------------------------------------------------
using System;
using System.Linq;
using LanguageExt;
using LanguageExt.Common;
using MTConnect.Assets.CuttingTools;
using Rasm.Fabrication.Kinematics;
using Rasm.Fabrication.Process;
using Rasm.Fabrication.Tooling;
using Rhino.Geometry;
using Thinktecture;
using UnitsNet;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Posting;

// --- [TYPES] --------------------------------------------------------------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record OptimizationIngress {
    private OptimizationIngress() { }

    public sealed record Raw(
        ProcessBudget.Subtractive Budget,
        CuttingData Cutting,
        ProcessFeedRate FeedRange,
        ProcessSpindleSpeed SpindleRange,
        int Teeth,
        double ToolDiameterMm,
        double EstimatedRadialDepthMm,
        double EstimatedAxialDepthMm,
        double MinimumEngagementFraction,
        Option<double> SpindlePowerWatts,
        HashMap<BlockLocus, EngagementRow> Engagement,
        double MaximumDeviationMm,
        double MinimumTurnRadians,
        double MinimumRadiusMm,
        double GeometryToleranceMm,
        double CollinearToleranceMm,
        double CocircularToleranceMm,
        int MinimumPatternLength,
        int MaximumPatternLength,
        int MinimumPatternOccurrences,
        int FirstPatternLabel,
        Set<OptimizePass> Passes,
        PostPolicy Post) : OptimizationIngress;

    public sealed record Admitted(OptimizePolicy Policy) : OptimizationIngress;
}

[SmartEnum<string>]
public sealed partial class OptimizePass {
    public static readonly OptimizePass MrrFeed = new("mrr-feed", 10, OptimizationCore.MrrFeed);
    public static readonly OptimizePass CornerSmooth = new("corner-smooth", 20, OptimizationCore.Smooth);
    public static readonly OptimizePass Compact = new("compact", 30, OptimizationCore.Compact);
    public static readonly OptimizePass PatternFold = new("pattern-fold", 40, OptimizationCore.PatternFold);

    public int Order { get; }

    [UseDelegateFromConstructor]
    internal partial Fin<PassState> Fold(PassState state, OptimizePolicy policy);
}

[ValueObject<int>(KeyMemberName = "Segments")]
public readonly partial struct PatternLength;

[ComplexValueObject]
public sealed partial class BlockLocus {
    public Seq<int> Path { get; }

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Seq<int> path) =>
        validationError = path.IsEmpty || path.Exists(static index => index < 0)
            ? new ValidationError("optimization:locus") : null;
}

[ComplexValueObject]
public sealed partial class EngagementRow {
    public BlockLocus Locus { get; }
    public Length RadialDepth { get; }
    public Length AxialDepth { get; }

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref BlockLocus locus,
        ref Length radialDepth, ref Length axialDepth) =>
        validationError = locus is null || radialDepth <= Length.Zero || axialDepth <= Length.Zero
            ? new ValidationError("optimization:engagement") : null;
}

[ComplexValueObject]
public sealed partial class MrrPolicy {
    public CuttingData Cutting { get; }
    public Speed ProcessFeed { get; }
    public Speed MinimumFeed { get; }
    public Speed MaximumFeed { get; }
    public RotationalSpeed MinimumSpindle { get; }
    public RotationalSpeed MaximumSpindle { get; }
    public RotationalSpeed ProgramSpindle { get; }
    public Length ToolDiameter { get; }
    public int Teeth { get; }
    public Length EstimatedRadialDepth { get; }
    public Length EstimatedAxialDepth { get; }
    public double MinimumEngagementFraction { get; }
    public Option<Power> SpindlePower { get; }
    public HashMap<BlockLocus, EngagementRow> Engagement { get; }

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref CuttingData cutting,
        ref Speed processFeed, ref Speed minimumFeed, ref Speed maximumFeed, ref RotationalSpeed minimumSpindle,
        ref RotationalSpeed maximumSpindle, ref RotationalSpeed programSpindle, ref Length toolDiameter, ref int teeth,
        ref Length estimatedRadialDepth, ref Length estimatedAxialDepth, ref double minimumEngagementFraction,
        ref Option<Power> spindlePower, ref HashMap<BlockLocus, EngagementRow> engagement) =>
        validationError = cutting is null || processFeed <= Speed.Zero || minimumFeed <= Speed.Zero || maximumFeed < minimumFeed
            || minimumSpindle <= RotationalSpeed.Zero || maximumSpindle < minimumSpindle || programSpindle <= RotationalSpeed.Zero
            || toolDiameter <= Length.Zero || teeth <= 0 || estimatedRadialDepth <= Length.Zero || estimatedAxialDepth <= Length.Zero
            || estimatedRadialDepth > toolDiameter || engagement.AsIterable().Exists(row => row.Key != row.Value.Locus
                || row.Value.RadialDepth > toolDiameter)
            || !double.IsFinite(minimumEngagementFraction) || minimumEngagementFraction is <= 0.0 or > 1.0
            || spindlePower.Exists(static value => value <= Power.Zero)
            ? new ValidationError("optimization:mrr") : null;
}

[ComplexValueObject]
public sealed partial class SmoothPolicy {
    public Length MaximumDeviation { get; }
    public Angle MinimumTurn { get; }
    public Length MinimumRadius { get; }
    public Length GeometryTolerance { get; }

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Length maximumDeviation,
        ref Angle minimumTurn, ref Length minimumRadius, ref Length geometryTolerance) =>
        validationError = maximumDeviation <= Length.Zero || minimumTurn <= Angle.Zero || minimumTurn >= Angle.FromRadians(Math.PI)
            || minimumRadius <= Length.Zero || geometryTolerance <= Length.Zero
            ? new ValidationError("optimization:smooth") : null;
}

[ComplexValueObject]
public sealed partial class CompactPolicy {
    public Length CollinearTolerance { get; }
    public Length CocircularTolerance { get; }

    static partial void ValidateFactoryArguments(ref ValidationError? validationError,
        ref Length collinearTolerance, ref Length cocircularTolerance) =>
        validationError = collinearTolerance <= Length.Zero || cocircularTolerance <= Length.Zero
            ? new ValidationError("optimization:compact") : null;
}

[ComplexValueObject]
public sealed partial class PatternPolicy {
    public PatternLength MinimumLength { get; }
    public PatternLength MaximumLength { get; }
    public int MinimumOccurrences { get; }
    public int FirstLabel { get; }

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref PatternLength minimumLength,
        ref PatternLength maximumLength, ref int minimumOccurrences, ref int firstLabel) =>
        validationError = minimumLength.Segments <= 0 || maximumLength.Segments < minimumLength.Segments
            || minimumOccurrences <= 1 || firstLabel <= 0
            ? new ValidationError("optimization:pattern") : null;
}

[ComplexValueObject]
public sealed partial class OptimizePolicy {
    public Set<OptimizePass> Passes { get; }
    public MrrPolicy Mrr { get; }
    public SmoothPolicy Smooth { get; }
    public CompactPolicy Compact { get; }
    public PatternPolicy Pattern { get; }
    public PostPolicy Post { get; }

    [IgnoreMember]
    public MotionDynamics Dynamics => Post.Cut.Dynamics;

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Set<OptimizePass> passes,
        ref MrrPolicy mrr, ref SmoothPolicy smooth, ref CompactPolicy compact, ref PatternPolicy pattern, ref PostPolicy post) =>
        validationError = mrr is null || smooth is null || compact is null || pattern is null || post is null
            ? new ValidationError("optimization:policy-owner") : null;
}

[ComplexValueObject]
public sealed partial class OptimizationEgress {
    public EmitPolicy Final { get; }

    [IgnoreMember]
    public EmitPolicy Measurement => EmitPolicy.Create(
        Final.Codec, Final.NewLine, Final.FinalTerminator, Final.Frame, new BlockLimit.Observe());

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref EmitPolicy @final) =>
        validationError = @final is null || @final.Limit is not BlockLimit.Enforce
            ? new ValidationError("optimization:egress") : null;
}

// --- [MODELS] -------------------------------------------------------------------------------------------------------------------------------------
public sealed record PassDelta(
    OptimizePass Pass,
    Duration Before,
    Duration After,
    int ChangedNodes,
    int EstimatedEngagement,
    int FoldedPatterns);

public sealed record OptimizationReceipt(
    Duration BaselineObjective,
    Duration OptimizedObjective,
    int BaselineRecords,
    int OptimizedRecords,
    int EstimatedEngagement,
    int FoldedPatterns,
    Seq<PassDelta> Passes);

public sealed record OptimizationResult(CutProgram Program, PostImage Image, OptimizationReceipt Receipt);

internal readonly record struct MotionSpan(double Length, double Cruise, double Entry, double Minutes, Vector3d Direction);

internal sealed record PassState(CutProgram Program, ProgramTrace Trace, Seq<PassDelta> Deltas);

// --- [OPERATIONS] ---------------------------------------------------------------------------------------------------------------------------------
public static class Optimize {
    public static Fin<OptimizationResult> Apply(CutProgram program, OptimizationIngress ingress, OptimizationEgress egress) {
        ArgumentNullException.ThrowIfNull(program);
        ArgumentNullException.ThrowIfNull(ingress);
        ArgumentNullException.ThrowIfNull(egress);

        return from policy in Admit(ingress)
               from _ in Numeric(program.Nodes)
               from baselineTrace in Post.Interpret(program)
               from __ in Capability(program, policy, baselineTrace)
               from baselineImage in Dialect.Emit(program, egress.Measurement)
               let initial = new PassState(program, baselineTrace, Seq<PassDelta>())
               from folded in toSeq(policy.Passes.OrderBy(static pass => pass.Order))
                   .Fold(Fin.Succ(initial), (state, pass) => state.Bind(current => pass.Fold(current, policy)))
               from looked in Post.Lookahead(folded.Program.Nodes, policy.Dynamics, program.Dialect)
               let certified = CutProgram.Of(looked, program.Dialect)
               from image in Dialect.Emit(certified, egress.Final)
               from optimizedTrace in Post.Interpret(certified)
               let estimated = folded.Deltas.Sum(static delta => delta.EstimatedEngagement)
               let patterns = folded.Deltas.Sum(static delta => delta.FoldedPatterns)
               select new OptimizationResult(certified, image, new OptimizationReceipt(
                   Objective(baselineTrace, policy.Dynamics), Objective(optimizedTrace, policy.Dynamics),
                   baselineImage.PhysicalRecords, image.PhysicalRecords, estimated, patterns, folded.Deltas));
    }

    private static Fin<OptimizePolicy> Admit(OptimizationIngress ingress) => ingress.Switch(
        raw: static raw => Admit(raw),
        admitted: static admitted => Optional(admitted.Policy).ToFin(Error.New("optimization:admitted-policy")));

    private static Fin<OptimizePolicy> Admit(OptimizationIngress.Raw raw) {
        if (raw.Budget is null || raw.Cutting is null || raw.FeedRange is null || raw.SpindleRange is null || raw.Post is null)
            return Fin.Fail<OptimizePolicy>(Error.New("optimization:references"));
        double feedMinimum = raw.FeedRange.Minimum ?? raw.FeedRange.Value ?? raw.FeedRange.Nominal ?? raw.Budget.FeedRate;
        double feedMaximum = raw.FeedRange.Maximum ?? raw.FeedRange.Value ?? raw.FeedRange.Nominal ?? raw.Budget.FeedRate;
        double spindleMinimum = raw.SpindleRange.Minimum ?? raw.SpindleRange.Value ?? raw.SpindleRange.Nominal ?? raw.Budget.SpindleRpm;
        double spindleMaximum = raw.SpindleRange.Maximum ?? raw.SpindleRange.Value ?? raw.SpindleRange.Nominal ?? raw.Budget.SpindleRpm;
        K<Validation<Error>, Unit> dimensions = Gate(Seq(raw.ToolDiameterMm, raw.EstimatedRadialDepthMm,
            raw.EstimatedAxialDepthMm, raw.MaximumDeviationMm, raw.MinimumRadiusMm, raw.GeometryToleranceMm,
            raw.CollinearToleranceMm, raw.CocircularToleranceMm).ForAll(static value => double.IsFinite(value) && value > 0.0),
            "optimization:dimensions");
        K<Validation<Error>, Unit> ranges = Gate(feedMinimum > 0.0 && feedMaximum >= feedMinimum
            && spindleMinimum > 0.0 && spindleMaximum >= spindleMinimum && double.IsFinite(raw.Budget.FeedRate)
            && raw.Budget.FeedRate > 0.0 && double.IsFinite(raw.Budget.SpindleRpm) && raw.Budget.SpindleRpm > 0.0,
            "optimization:controller-range");
        K<Validation<Error>, Unit> topology = Gate(raw.Teeth > 0
            && double.IsFinite(raw.MinimumEngagementFraction) && raw.MinimumEngagementFraction is > 0.0 and <= 1.0
            && double.IsFinite(raw.MinimumTurnRadians) && raw.MinimumTurnRadians is > 0.0 and < Math.PI
            && raw.SpindlePowerWatts.ForAll(static value => double.IsFinite(value) && value > 0.0)
            && raw.MinimumPatternLength > 0 && raw.MaximumPatternLength >= raw.MinimumPatternLength
            && raw.MinimumPatternOccurrences > 1 && raw.FirstPatternLabel > 0, "optimization:policy");
        return (dimensions, ranges, topology).Apply((_, _, _) => Policy(raw,
            feedMinimum, feedMaximum, spindleMinimum, spindleMaximum)).As().ToFin();
    }

    private static OptimizePolicy Policy(OptimizationIngress.Raw raw,
        double feedMinimum, double feedMaximum, double spindleMinimum, double spindleMaximum) =>
        OptimizePolicy.Create(
            raw.Passes,
            MrrPolicy.Create(raw.Cutting, Speed.FromMillimetersPerMinute(raw.Budget.FeedRate),
                Speed.FromMillimetersPerMinute(feedMinimum), Speed.FromMillimetersPerMinute(feedMaximum),
                RotationalSpeed.FromRevolutionsPerMinute(spindleMinimum), RotationalSpeed.FromRevolutionsPerMinute(spindleMaximum),
                RotationalSpeed.FromRevolutionsPerMinute(raw.Budget.SpindleRpm),
                Length.FromMillimeters(raw.ToolDiameterMm), raw.Teeth, Length.FromMillimeters(raw.EstimatedRadialDepthMm),
                Length.FromMillimeters(raw.EstimatedAxialDepthMm), raw.MinimumEngagementFraction,
                raw.SpindlePowerWatts.Map(Power.FromWatts), raw.Engagement),
            SmoothPolicy.Create(Length.FromMillimeters(raw.MaximumDeviationMm), Angle.FromRadians(raw.MinimumTurnRadians),
                Length.FromMillimeters(raw.MinimumRadiusMm), Length.FromMillimeters(raw.GeometryToleranceMm)),
            CompactPolicy.Create(Length.FromMillimeters(raw.CollinearToleranceMm), Length.FromMillimeters(raw.CocircularToleranceMm)),
            PatternPolicy.Create(PatternLength.Create(raw.MinimumPatternLength), PatternLength.Create(raw.MaximumPatternLength),
                raw.MinimumPatternOccurrences, raw.FirstPatternLabel), raw.Post);

    private static K<Validation<Error>, Unit> Gate(bool valid, string locus) => valid
        ? Validation<Error, Unit>.Success(unit)
        : Validation<Error, Unit>.Fail(Error.New(locus));

    private static Fin<Unit> Numeric(Seq<GNode> nodes) => nodes.ForAll(NumericNode)
        ? Fin.Succ(unit)
        : Fin.Fail<Unit>(Error.New("optimization:symbolic-program"));

    // Every selected pass states its own precondition against the program; independent refusals accumulate.
    private static Fin<Unit> Capability(CutProgram program, OptimizePolicy policy, ProgramTrace trace) {
        bool geometry = trace.Events.ForAll(static item => item switch {
            ProgramEvent.Motion motion => Seq('X', 'Y', 'Z').ForAll(address => motion.Word.P(address).IsSome),
            ProgramEvent.State state => state.Command != GCommand.Relative && state.Command != GCommand.ArcRelative
                && state.Command != GCommand.PlaneZx && state.Command != GCommand.PlaneYz,
            _ => true,
        });
        return (Gate(!policy.Passes.Contains(OptimizePass.PatternFold) || program.Dialect.Subprogram != SubprogramGrammar.None,
                "optimization:subprogram-grammar"),
            Gate(!policy.Passes.Contains(OptimizePass.CornerSmooth) && !policy.Passes.Contains(OptimizePass.Compact) || geometry,
                "optimization:geometry-context"),
            Gate(!policy.Passes.Contains(OptimizePass.CornerSmooth)
                || program.Dialect.Arc.Exists(static mode => mode == ArcMode.Ijk || mode == ArcMode.Both),
                "optimization:arc-representation"))
            .Apply(static (_, _, _) => unit).As().ToFin();
    }

    private static bool NumericNode(GNode node) => node.Switch(
        block: static block => block.Body.ForAll(NumericNode),
        word: static word => word.Command.Group != ModalGroup.Motion || word.Words
            .Filter(static parameter => parameter.Address is 'X' or 'Y' or 'Z' or 'A' or 'B' or 'C' or 'U' or 'V' or 'W'
                or 'I' or 'J' or 'K' or 'R' or 'F' or 'S')
            .ForAll(static parameter => parameter.Value.Scalar.IsSome),
        cannedCycle: static cycle => cycle.SingleBlockWords.ForAll(static parameter => parameter.Value.Scalar.IsSome),
        coordinateFrame: static _ => true,
        macro: static macro => macro.Body.ForAll(NumericNode),
        subprogram: static subprogram => subprogram.Body.ForAll(NumericNode),
        additiveLayer: static _ => true,
        nc1: static _ => true,
        directive: static _ => true);

    // Machine minutes: every span closes against the junction speed its successor fixes, so the last span decelerates
    // to rest and no pass can report a gain the machine cannot deliver.
    internal static Duration Objective(ProgramTrace trace, MotionDynamics dynamics) {
        (GCommand Plane, Option<MotionSpan> Pending, double Minutes) folded = trace.Events.Fold(
            (Plane: GCommand.PlaneXy, Pending: Option<MotionSpan>.None, Minutes: 0.0),
            (state, item) => item switch {
                ProgramEvent.State { Command: var command }
                    when command == GCommand.PlaneXy || command == GCommand.PlaneZx || command == GCommand.PlaneYz =>
                        (command, state.Pending, state.Minutes),
                ProgramEvent.State { Word: var word }
                    when word.Command == GCommand.Dwell =>
                        (state.Plane, state.Pending, state.Minutes + (word.P('P').IfNone(0.0) / 60.0)),
                ProgramEvent.Motion motion when Span(motion, state.Plane, dynamics) is { Length: > 0.0 } span =>
                    (state.Plane, Some(span with { Entry = Junction(state.Pending, span, dynamics) }),
                        state.Minutes + state.Pending.Map(pending => Elapsed(pending, Junction(state.Pending, span, dynamics), dynamics))
                            .IfNone(0.0)),
                _ => state,
            });
        return Duration.FromMinutes(folded.Minutes + folded.Pending.Map(span => Elapsed(span, 0.0, dynamics)).IfNone(0.0));
    }

    private static double Junction(Option<MotionSpan> pending, MotionSpan span, MotionDynamics dynamics) => pending
        .Map(previous => Math.Min(Math.Min(previous.Cruise, span.Cruise),
            dynamics.JunctionFeed(Vector3d.VectorAngle(previous.Direction, span.Direction))))
        .IfNone(0.0);

    private static double Elapsed(MotionSpan span, double exit, MotionDynamics dynamics) {
        if (span.Minutes > 0.0)
            return span.Minutes;
        double accel = Math.Max(dynamics.Acceleration, double.Epsilon);
        double cruise = span.Cruise / 60.0;
        double entry = Math.Min(span.Entry / 60.0, cruise);
        double leave = Math.Min(exit / 60.0, cruise);
        double peak = Math.Min(cruise, Math.Sqrt(((2.0 * accel * span.Length) + (entry * entry) + (leave * leave)) / 2.0));
        double ramps = (Math.Max(peak - entry, 0.0) + Math.Max(peak - leave, 0.0)) / accel;
        double covered = (((peak * peak) - (entry * entry)) + ((peak * peak) - (leave * leave))) / (2.0 * accel);
        return (ramps + (Math.Max(span.Length - covered, 0.0) / Math.Max(peak, double.Epsilon))) / 60.0;
    }

    private static MotionSpan Span(ProgramEvent.Motion motion, GCommand plane, MotionDynamics dynamics) {
        bool arc = motion.Word.Command == GCommand.ArcCw || motion.Word.Command == GCommand.ArcCcw;
        double length = arc ? ArcLength(motion, plane) : motion.From.DistanceTo(motion.To);
        double programmed = motion.Word.Command == GCommand.Rapid
            ? dynamics.RapidFeed
            : Math.Min(motion.Feed > 0.0 ? motion.Feed : dynamics.LinearFeed, arc ? dynamics.ArcFeed : dynamics.LinearFeed);
        Vector3d direction = motion.To - motion.From;
        _ = direction.Unitize();
        return new MotionSpan(
            length,
            programmed,
            0.0,
            motion.Mode == FeedMode.InverseTime && motion.Feed > 0.0 ? 1.0 / motion.Feed : 0.0,
            direction);
    }

    private static double ArcLength(ProgramEvent.Motion motion, GCommand plane) {
        (double FromU, double FromV, double FromW) = Project(motion.From, plane);
        (double ToU, double ToV, double ToW) = Project(motion.To, plane);
        Option<double> radiusWord = motion.Word.P('R');
        if (radiusWord.IsSome) {
            double signedRadius = radiusWord.IfNone(0.0);
            double radius = Math.Abs(signedRadius);
            double chord = new Vector2d(ToU - FromU, ToV - FromV).Length;
            double minor = 2.0 * Math.Asin(Math.Clamp(chord / (2.0 * Math.Max(radius, double.Epsilon)), 0.0, 1.0));
            double sweep = signedRadius < 0.0 ? Math.Tau - minor : minor;
            return Helical(radius * sweep, ToW - FromW);
        }
        return motion.Arc.Match(
            Some: resolved => {
                (double CenterU, double CenterV, _) = Project(resolved.Center, plane);
                double centerRadius = new Vector2d(FromU - CenterU, FromV - CenterV).Length;
                double start = Math.Atan2(FromV - CenterV, FromU - CenterU);
                double end = Math.Atan2(ToV - CenterV, ToU - CenterU);
                double turn = resolved.Sense == RotationSense.Clockwise
                    ? (start - end + Math.Tau) % Math.Tau
                    : (end - start + Math.Tau) % Math.Tau;
                return Helical(centerRadius * turn, ToW - FromW);
            },
            None: static () => 0.0);
    }

    private static double Helical(double planar, double rise) => Math.Sqrt((planar * planar) + (rise * rise));

    private static (double U, double V, double W) Project(Point3d point, GCommand plane) => plane == GCommand.PlaneZx
        ? (point.Z, point.X, point.Y)
        : plane == GCommand.PlaneYz ? (point.Y, point.Z, point.X) : (point.X, point.Y, point.Z);

}

internal static class OptimizationCore {
    // Tapping and threading feed is bound to pitch and spindle speed, so adaptation never reaches them.
    private static readonly Set<GCommand> AdaptiveCycles = Set(GCommand.Drill, GCommand.DrillDwell, GCommand.Peck, GCommand.Bore);

    internal static Fin<PassState> MrrFeed(PassState state, OptimizePolicy policy) => Rewrite(
        state, OptimizePass.MrrFeed, policy,
        nodes => Deep(
            nodes,
            Seq<int>(),
            (locus, word) => Subtractive(word)
                ? Feed(locus, policy.Mrr).Map(feed => word.With('F', feed.MillimetersPerMinute))
                : Fin.Succ(word),
            (locus, cycle) => CycleSubtractive(cycle)
                ? Feed(locus, policy.Mrr).Map(feed => CycleFeed(cycle, feed))
                : Fin.Succ(cycle)),
        Estimated(state.Program.Nodes, Seq<int>(), policy.Mrr),
        0);

    internal static Fin<PassState> Smooth(PassState state, OptimizePolicy policy) => Rewrite(
        state, OptimizePass.CornerSmooth, policy,
        nodes => Fin.Succ(SmoothNodes(nodes, policy.Smooth, state.Program.Dialect)), 0, 0);

    internal static Fin<PassState> Compact(PassState state, OptimizePolicy policy) => Rewrite(
        state, OptimizePass.Compact, policy,
        nodes => Fin.Succ(CompactNodes(nodes, policy.Compact, state.Program.Dialect)), 0, 0);

    internal static Fin<PassState> PatternFold(PassState state, OptimizePolicy policy) {
        (Seq<GNode> Nodes, int Count) folded = FoldPatterns(state.Program.Nodes, policy.Pattern, policy.Pattern.FirstLabel);
        return Rewrite(state, OptimizePass.PatternFold, policy, _ => Fin.Succ(folded.Nodes), 0, folded.Count);
    }

    // Incoming trace is the prior pass's result, so one interpretation per pass proves both ends of its delta.
    private static Fin<PassState> Rewrite(PassState state, OptimizePass pass, OptimizePolicy policy,
        Func<Seq<GNode>, Fin<Seq<GNode>>> transform, int estimated, int patterns) =>
        from nodes in transform(state.Program.Nodes)
        let program = CutProgram.Of(nodes, state.Program.Dialect)
        from trace in Post.Interpret(program)
        select new PassState(program, trace, state.Deltas.Add(new PassDelta(pass,
            Optimize.Objective(state.Trace, policy.Dynamics), Optimize.Objective(trace, policy.Dynamics),
            Changed(state.Program.Nodes, nodes), estimated, patterns)));

    private static Fin<Speed> Feed(BlockLocus locus, MrrPolicy policy) {
        EngagementRow engagement = policy.Engagement.Find(locus).IfNone(
            EngagementRow.Create(locus, policy.EstimatedRadialDepth, policy.EstimatedAxialDepth));
        double fraction = Math.Clamp(engagement.RadialDepth.Millimeters / policy.ToolDiameter.Millimeters,
            policy.MinimumEngagementFraction, 1.0);
        double thinning = fraction >= 0.5 ? 1.0 : 1.0 / Math.Sqrt(1.0 - Math.Pow(1.0 - (2.0 * fraction), 2.0));
        double spindle = Math.Clamp(policy.ProgramSpindle.RevolutionsPerMinute,
            policy.MinimumSpindle.RevolutionsPerMinute, policy.MaximumSpindle.RevolutionsPerMinute);
        (double Feed, double Chip) basis = policy.Cutting.FeedBasis.Switch(
            state: (Feed: policy.Cutting.Feed, Process: policy.ProcessFeed.MillimetersPerMinute, Teeth: policy.Teeth, Spindle: spindle),
            perTooth: static value => (value.Feed * value.Teeth * value.Spindle, value.Feed),
            perRevolution: static value => (value.Feed * value.Spindle, value.Feed / value.Teeth),
            linearPerMinute: static value => (value.Feed, value.Feed / (value.Teeth * value.Spindle)),
            surfaceRatio: static value => (value.Process, value.Process / (value.Teeth * value.Spindle)));
        double proposed = Math.Min(policy.ProcessFeed.MillimetersPerMinute, basis.Feed * thinning);
        CutIntent intent = CutIntent.Create(
            Length.FromMillimeters(basis.Chip), engagement.AxialDepth, engagement.AxialDepth,
            engagement.RadialDepth, policy.ToolDiameter, policy.Teeth,
            RotationalSpeed.FromRevolutionsPerMinute(spindle), Speed.FromMillimetersPerMinute(proposed));
        return from load in policy.Cutting.Evaluate(intent)
               let powered = policy.SpindlePower
                   .Map(ceiling => load.Power > ceiling ? proposed * ceiling.Watts / load.Power.Watts : proposed)
                   .IfNone(proposed)
               from feed in powered >= policy.MinimumFeed.MillimetersPerMinute
                   ? Fin.Succ(Speed.FromMillimetersPerMinute(Math.Min(powered, policy.MaximumFeed.MillimetersPerMinute)))
                   : Fin.Fail<Speed>(Error.New("optimization:feed-envelope"))
               select feed;
    }

    private static Fin<Seq<GNode>> Deep(
        Seq<GNode> nodes,
        Seq<int> prefix,
        Func<BlockLocus, GNode.Word, Fin<GNode.Word>> rewrite,
        Func<BlockLocus, GNode.CannedCycle, Fin<GNode.CannedCycle>> cycleRewrite) =>
        nodes.Map((node, index) => (Node: node, Locus: prefix.Add(index))).Traverse(row => row.Node.Switch(
            state: (Locus: row.Locus, Rewrite: rewrite, CycleRewrite: cycleRewrite),
            block: static (context, block) => Deep(block.Body.ToSeq(), context.Locus, context.Rewrite, context.CycleRewrite)
                .Map<GNode>(body => block with { Body = body.ToArr() }),
            word: static (context, word) => context.Rewrite(BlockLocus.Create(context.Locus), word).Map<GNode>(static value => value),
            cannedCycle: static (context, cycle) => context.CycleRewrite(BlockLocus.Create(context.Locus), cycle).Map<GNode>(static value => value),
            coordinateFrame: static (_, frame) => Fin.Succ<GNode>(frame),
            macro: static (context, macro) => Deep(macro.Body.ToSeq(), context.Locus, context.Rewrite, context.CycleRewrite)
                .Map<GNode>(body => macro with { Body = body.ToArr() }),
            subprogram: static (context, subprogram) => Deep(subprogram.Body.ToSeq(), context.Locus, context.Rewrite, context.CycleRewrite)
                .Map<GNode>(body => subprogram with { Body = body.ToArr() }),
            additiveLayer: static (_, layer) => Fin.Succ<GNode>(layer),
            nc1: static (_, nc1) => Fin.Succ<GNode>(nc1),
            directive: static (_, directive) => Fin.Succ<GNode>(directive))).As();

    private static Seq<GNode> SmoothNodes(Seq<GNode> nodes, SmoothPolicy policy, PostDialect dialect) {
        GNode[] source = nodes.Map(node => node.Switch(
            block: block => block with { Body = SmoothNodes(block.Body.ToSeq(), policy, dialect).ToArr() },
            word: static word => (GNode)word,
            cannedCycle: static cycle => cycle,
            coordinateFrame: static frame => frame,
            macro: macro => macro with { Body = SmoothNodes(macro.Body.ToSeq(), policy, dialect).ToArr() },
            subprogram: subprogram => subprogram with { Body = SmoothNodes(subprogram.Body.ToSeq(), policy, dialect).ToArr() },
            additiveLayer: static layer => layer,
            nc1: static nc1 => nc1,
            directive: static directive => directive)).ToArray();
        return toSeq(Enumerable.Range(0, source.Length)).Bind(index => Blend(source, index, policy, dialect));
    }

    private static Seq<GNode> Blend(GNode[] source, int index, SmoothPolicy policy, PostDialect dialect) {
        if (index == 0 || index + 1 >= source.Length || source[index] is not GNode.Word corner
            || source[index - 1] is not GNode.Word incoming || source[index + 1] is not GNode.Word outgoing
            || incoming.Command != GCommand.Feed || corner.Command != GCommand.Feed || outgoing.Command != GCommand.Feed)
            return Seq(source[index]);
        Point3d start = Point(incoming, Point3d.Origin);
        Point3d vertex = Point(corner, start);
        Point3d end = Point(outgoing, vertex);
        Vector3d first = vertex - start;
        Vector3d second = end - vertex;
        if (first.Length <= policy.GeometryTolerance.Millimeters || second.Length <= policy.GeometryTolerance.Millimeters
            || Math.Abs(first.Z) > policy.GeometryTolerance.Millimeters || Math.Abs(second.Z) > policy.GeometryTolerance.Millimeters)
            return Seq(source[index]);
        double turn = Vector3d.VectorAngle(first, second);
        if (turn < policy.MinimumTurn.Radians)
            return Seq(source[index]);
        double half = 0.5 * (Math.PI - turn);
        double radius = policy.MaximumDeviation.Millimeters * Math.Sin(half) / (1.0 - Math.Sin(half));
        double trim = radius / Math.Tan(half);
        if (radius < policy.MinimumRadius.Millimeters || trim >= Math.Min(first.Length, second.Length))
            return Seq(source[index]);
        first.Unitize();
        second.Unitize();
        Point3d tangentIn = vertex - (first * trim);
        Point3d tangentOut = vertex + (second * trim);
        double orientation = Vector3d.CrossProduct(first, second).Z;
        Point3d center = tangentIn + (Vector3d.CrossProduct(Vector3d.ZAxis, first) * (orientation < 0.0 ? -radius : radius));
        GNode.Word line = corner.With('X', tangentIn.X).With('Y', tangentIn.Y).With('Z', tangentIn.Z);
        GNode.Word arc = Modal(new GNode.Word(
            orientation < 0.0 ? GCommand.ArcCw : GCommand.ArcCcw,
            Arr(GParam.Number('X', tangentOut.X, corner.SourceUnits), GParam.Number('Y', tangentOut.Y, corner.SourceUnits),
                GParam.Number('Z', tangentOut.Z, corner.SourceUnits), GParam.Number('I', center.X - tangentIn.X, corner.SourceUnits),
                GParam.Number('J', center.Y - tangentIn.Y, corner.SourceUnits)), corner.Mode), corner, dialect);
        return Seq<GNode>(line, arc);
    }

    // An explicit-retention control repeats every modal word, so the produced arc carries the corner's own feed and speed
    // while keeping its own tangent-out endpoint.
    private static GNode.Word Modal(GNode.Word arc, GNode.Word corner, PostDialect dialect) => dialect.Retention == WordRetention.Modal
        ? arc
        : Seq('F', 'S').Fold(arc, (current, address) => corner.P(address).Match(
            Some: value => current.With(address, value),
            None: () => current));

    private static Seq<GNode> CompactNodes(Seq<GNode> nodes, CompactPolicy policy, PostDialect dialect) =>
        StripModal(nodes.Map(node => node.Switch(
            block: block => block with { Body = CompactNodes(block.Body.ToSeq(), policy, dialect).ToArr() },
            word: static word => (GNode)word,
            cannedCycle: static cycle => cycle,
            coordinateFrame: static frame => frame,
            macro: macro => macro with { Body = CompactNodes(macro.Body.ToSeq(), policy, dialect).ToArr() },
            subprogram: subprogram => subprogram with { Body = CompactNodes(subprogram.Body.ToSeq(), policy, dialect).ToArr() },
            additiveLayer: static layer => layer,
            nc1: static nc1 => nc1,
            directive: static directive => directive)).Fold(
            (Rows: Seq<GNode>(), Start: Point3d.Origin, Cursor: Point3d.Origin),
            (state, node) => Merge(state, node, policy)).Rows, dialect).Rows;

    // Start is the locus entering the surviving row and Cursor the locus leaving it, so a merged row keeps its true span.
    private static (Seq<GNode> Rows, Point3d Start, Point3d Cursor) Merge(
        (Seq<GNode> Rows, Point3d Start, Point3d Cursor) state, GNode node, CompactPolicy policy) {
        if (state.Rows.Last.Case is not GNode.Word previous || node is not GNode.Word current)
            return (state.Rows.Add(node), state.Cursor,
                node is GNode.Word word ? Point(word, state.Cursor) : state.Cursor);
        Point3d end = Point(current, state.Cursor);
        Vector3d first = state.Cursor - state.Start;
        Vector3d second = end - state.Cursor;
        bool linear = current.Command == previous.Command
            && (current.Command == GCommand.Feed || current.Command == GCommand.Rapid)
            && Vector3d.CrossProduct(first, second).Length <= policy.CollinearTolerance.Millimeters
            && first * second > 0.0 && previous.P('F') == current.P('F');
        bool cocircular = (previous.Command == GCommand.ArcCw || previous.Command == GCommand.ArcCcw)
            && current.Command == previous.Command && previous.P('F') == current.P('F')
            && new Point3d(state.Start.X + previous.P('I').IfNone(0.0), state.Start.Y + previous.P('J').IfNone(0.0), state.Start.Z)
                .DistanceTo(new Point3d(state.Cursor.X + current.P('I').IfNone(0.0), state.Cursor.Y + current.P('J').IfNone(0.0), state.Cursor.Z))
                <= policy.CocircularTolerance.Millimeters;
        return linear || cocircular
            ? (state.Rows.Init.Add(Carry(previous, current)), state.Start, end)
            : (state.Rows.Add(node), state.Cursor, end);
    }

    // A nested body runs under state this fold cannot read, so the census clears rather than stripping a stale repeat.
    private static (Seq<GNode> Rows, HashMap<char, double> Modal) StripModal(Seq<GNode> rows, PostDialect dialect) =>
        dialect.Retention != WordRetention.Modal
            ? (rows, HashMap<char, double>())
            : rows.Fold((Rows: Seq<GNode>(), Modal: HashMap<char, double>()), static (fold, node) => node is GNode.Word word
                ? StripWord(fold, word)
                : (fold.Rows.Add(node), HashMap<char, double>()));

    private static (Seq<GNode> Rows, HashMap<char, double> Modal) StripWord(
        (Seq<GNode> Rows, HashMap<char, double> Modal) state, GNode.Word word) {
        (GNode.Word Word, HashMap<char, double> Modal) stripped = word.Words
            .Filter(static parameter => parameter.Address is 'F' or 'S')
            .Choose(static parameter => parameter.Value.Scalar.Map(value => (parameter.Address, Value: value)))
            .Fold((Word: word, Modal: state.Modal), static (fold, parameter) => fold.Modal.Find(parameter.Address).Match(
                Some: value => value == parameter.Value
                    ? (fold.Word.Without(parameter.Address), fold.Modal)
                    : (fold.Word, fold.Modal.AddOrUpdate(parameter.Address, parameter.Value)),
                None: () => (fold.Word, fold.Modal.Add(parameter.Address, parameter.Value))));
        return (state.Rows.Add(stripped.Word), stripped.Modal);
    }

    private static (Seq<GNode> Nodes, int Count) FoldPatterns(Seq<GNode> nodes, PatternPolicy policy, int label) {
        (Seq<GNode> Nested, int NestedCount) nested = nodes.Fold(
            (Nodes: Seq<GNode>(), Count: 0),
            (state, node) => node.Switch(
                state: state,
                block: (current, block) => {
                    (Seq<GNode> Nodes, int Count) body = FoldPatterns(block.Body.ToSeq(), policy, label + current.Count);
                    return (current.Nodes.Add(block with { Body = body.Nodes.ToArr() }), current.Count + body.Count);
                },
                word: static (current, word) => (current.Nodes.Add(word), current.Count),
                cannedCycle: static (current, cycle) => (current.Nodes.Add(cycle), current.Count),
                coordinateFrame: static (current, frame) => (current.Nodes.Add(frame), current.Count),
                macro: (current, macro) => {
                    (Seq<GNode> Nodes, int Count) body = FoldPatterns(macro.Body.ToSeq(), policy, label + current.Count);
                    return (current.Nodes.Add(macro with { Body = body.Nodes.ToArr() }), current.Count + body.Count);
                },
                subprogram: (current, subprogram) => {
                    (Seq<GNode> Nodes, int Count) body = FoldPatterns(subprogram.Body.ToSeq(), policy, label + current.Count);
                    return (current.Nodes.Add(subprogram with { Body = body.Nodes.ToArr() }), current.Count + body.Count);
                },
                additiveLayer: static (current, layer) => (current.Nodes.Add(layer), current.Count),
                nc1: static (current, nc1) => (current.Nodes.Add(nc1), current.Count),
                directive: static (current, directive) => (current.Nodes.Add(directive), current.Count)));
        int available = NextLabel(nested.Nested, checked(label + nested.NestedCount));
        Option<(int Start, int Length, Seq<int> Occurrences)> candidate = Enumerable
            .Range(policy.MinimumLength.Segments, policy.MaximumLength.Segments - policy.MinimumLength.Segments + 1)
            .SelectMany(length => Enumerable.Range(0, Math.Max(0, nested.Nested.Count - length + 1)).Select(start => (start, length)))
            .Select(row => (Start: row.start, Length: row.length, Occurrences: Occurrences(nested.Nested, row.start, row.length)))
            .Where(row => row.Occurrences.Count >= policy.MinimumOccurrences && Saving(row.Occurrences.Count, row.Length) > 0)
            .OrderByDescending(row => Saving(row.Occurrences.Count, row.Length))
            .ThenByDescending(row => row.Length)
            .HeadOrNone();
        return candidate.Match(
            Some: row => {
                Arr<GNode> body = nested.Nested.Skip(row.Start).Take(row.Length).ToArr();
                Set<int> starts = row.Occurrences.ToSet();
                Seq<GNode> rewritten = nested.Nested.Map(static (node, index) => (Node: node, Index: index)).Bind(entry =>
                    starts.Contains(entry.Index) ? Seq<GNode>(new GNode.Subprogram(available, 1, body))
                    : starts.Exists(start => entry.Index > start && entry.Index < start + row.Length) ? Seq<GNode>()
                    : Seq(entry.Node));
                (Seq<GNode> Nested, int Count) repeated = FoldPatterns(rewritten, policy, checked(available + 1));
                return (repeated.Nested, nested.NestedCount + repeated.Count + 1);
            },
            None: () => (nested.Nested, nested.NestedCount));
    }

    // Occurrences replaced by one call each, one hoisted definition, and its two framing records.
    private static int Saving(int occurrences, int length) => ((occurrences - 1) * length) - occurrences - 2;

    private static int NextLabel(Seq<GNode> nodes, int candidate) =>
        Labels(nodes).Exists(label => label == candidate) ? NextLabel(nodes, checked(candidate + 1)) : candidate;

    private static Seq<int> Labels(Seq<GNode> nodes) => nodes.Bind(node => node.Switch(
        block: static block => Labels(block.Body.ToSeq()),
        word: static _ => Seq<int>(),
        cannedCycle: static _ => Seq<int>(),
        coordinateFrame: static _ => Seq<int>(),
        macro: static macro => Labels(macro.Body.ToSeq()),
        subprogram: static subprogram => Seq(subprogram.Label).Concat(Labels(subprogram.Body.ToSeq())),
        additiveLayer: static _ => Seq<int>(),
        nc1: static _ => Seq<int>()));

    private static Seq<int> Occurrences(Seq<GNode> nodes, int seed, int length) {
        Seq<GNode> pattern = nodes.Skip(seed).Take(length);
        return toSeq(Enumerable.Range(0, Math.Max(0, nodes.Count - length + 1)))
            .Filter(index => nodes.Skip(index).Take(length).SequenceEqual(pattern))
            .Fold((Starts: Seq<int>(), End: -1), (state, index) => index >= state.End
                ? (state.Starts.Add(index), index + length) : state).Starts;
    }

    private static Point3d Point(GNode.Word word, Point3d prior) => new(
        word.P('X').IfNone(prior.X), word.P('Y').IfNone(prior.Y), word.P('Z').IfNone(prior.Z));

    private static GNode.Word Carry(GNode.Word survivor, GNode.Word merged) =>
        Seq('X', 'Y', 'Z', 'F', 'S').Fold(survivor,
            (current, address) => merged.P(address).Match(
                Some: value => current.With(address, value),
                None: () => current));

    private static bool Subtractive(GNode.Word word) => word.Command.Role == MotionRole.Cutting;

    private static bool CycleSubtractive(GNode.CannedCycle cycle) => AdaptiveCycles.Contains(cycle.Command);

    private static GNode.CannedCycle CycleFeed(GNode.CannedCycle cycle, Speed feed) {
        Arr<GParam> words = cycle.SingleBlockWords
            .Filter(static parameter => parameter.Address != 'F')
            .Add(GParam.Number('F', feed.MillimetersPerMinute, ProgramUnits.Metric));
        Seq<Move> moves = cycle.ExpandedMoves.Map(move => move.Switch(
            rapid: static rapid => (Move)rapid,
            linear: linear => linear with { Feed = feed.MillimetersPerMinute },
            circular: circular => circular with { Feed = feed.MillimetersPerMinute }));
        return cycle with { SingleBlockWords = words, ExpandedMoves = moves };
    }

    private static int Estimated(Seq<GNode> nodes, Seq<int> prefix, MrrPolicy policy) =>
        nodes.Map((node, index) => (Node: node, Locus: prefix.Add(index))).Sum(row => row.Node.Switch(
            block: block => Estimated(block.Body.ToSeq(), row.Locus, policy),
            word: word => Subtractive(word) && !policy.Engagement.ContainsKey(BlockLocus.Create(row.Locus)) ? 1 : 0,
            cannedCycle: cycle => CycleSubtractive(cycle) && !policy.Engagement.ContainsKey(BlockLocus.Create(row.Locus))
                ? Math.Max(1, cycle.Repeats) : 0,
            coordinateFrame: static _ => 0,
            macro: macro => Estimated(macro.Body.ToSeq(), row.Locus, policy),
            subprogram: subprogram => Estimated(subprogram.Body.ToSeq(), row.Locus, policy),
            additiveLayer: static _ => 0,
            nc1: static _ => 0));

    private static int Changed(Seq<GNode> before, Seq<GNode> after) =>
        before.Zip(after, static (left, right) => Changed(left, right)).Sum() + Math.Abs(before.Count - after.Count);

    private static int Changed(GNode before, GNode after) => (before, after) switch {
        (GNode.Block left, GNode.Block right) when left.Frame == right.Frame => Changed(left.Body.ToSeq(), right.Body.ToSeq()),
        (GNode.Macro left, GNode.Macro right) when left.Slots == right.Slots => Changed(left.Body.ToSeq(), right.Body.ToSeq()),
        (GNode.Subprogram left, GNode.Subprogram right) when left.Label == right.Label && left.Repeats == right.Repeats =>
            Changed(left.Body.ToSeq(), right.Body.ToSeq()),
        _ => before == after ? 0 : 1,
    };
}
```

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
