# [RASM_FABRICATION_PROGRAM_OPTIMIZATION]

`Optimize` owns one admitted `CutProgram` transformation. `OptimizationIngress` admits one raw ingress or an existing policy, the selected `PassPolicy` rows fold in declaration order, `Post.Interpret` supplies the one modal and spatial trace, and `OptimizationResult` returns the re-keyed program, pass evidence, and exact `Dialect.Emit` image together.

`NodeWalk` is the ONE structural recursion: every pass states its leaf rewrite or its level rewrite and the descent through block, macro, and subprogram bodies lives once, so a new `GNode` case cannot lose an arm in one walker while keeping it in four others. Pattern folding streams `CutProgram.Keys` — the structural `NodeKey` digest `Posting/program#AST` publishes — through a prefix-hash census under a declared occurrence budget, so the census is linear per window length and the rewrite terminates by policy rather than by luck. `StabilityLobes.Recommend` at `Tooling/cuttingdata#CHATTER_STABILITY` supplies the `StablePoint` this page intersects with the dialect's own spindle word and the machine power limit, refusing typed when no point survives.

## [01]-[INDEX]

- [02]-[ADMISSION]: `PassIngress`, `PassPolicy`, `CutContext`, the `OptimizeMap` unit lift, and `Optimize.Admit`.
- [03]-[OPTIMIZATION]: `OptimizePass`, `PassState`, `Optimize.Apply`, and the per-case capability gate.
- [04]-[OBJECTIVE]: accel-limited machine minutes over every motion, not cutting alone.
- [05]-[WALK]: `NodeWalk.Deep` and `NodeWalk.Collect`, the two total structural traversals every pass composes.
- [06]-[PASSES]: stability speed selection, feed adaptation, corner blending, compaction, and pattern folding.

## [02]-[ADMISSION]

- Owner: `OptimizePolicy` is the canonical policy; `CutContext` owns the machine and material envelope every pass prices against; `PassPolicy` is the ONE closed family of selected concerns and `PassIngress` its raw counterpart.
- Law: the selection IS the policy roster. A `Set<OptimizePass>` beside per-pass policy columns made the two able to disagree, and the hand agreement check that guarded ONE of those pairings could not have guarded the rest — the four required policies forced a caller selecting only compaction to supply a cutting model, an engagement map, and a spindle envelope it never reads. `Seq<PassPolicy>` makes the disagreement unrepresentable and the distinctness gate is the only invariant left.
- Law: the machine envelope is CONTEXT, never a pass column. The stability pass prices its power ceiling on the same feed range, spindle range, tool, and nominal engagement the feed pass reads, so a pass reaching into a sibling pass's policy for them was a coupling the split removes.
- Law: controller ranges cross as the package's OWN `ProcessRange`. `Tooling/magazine#TOOL_ASSET` admits the MTConnect `ProcessFeedRate` and `ProcessSpindleSpeed` envelopes once, into `Option<double>` columns with a declared `Resolve`; the raw asset types reaching this page put twelve null-coalescing reads over provider-nullable fields inside admission, which is foreign material crossing an admission boundary unadmitted.
- Law: `OptimizeMap` is the ONE unit lift. A millimetre double becoming a `Length` is a mapping, so it rides the generated mapper and the hand-written constructor fan that spelled every conversion at a call site is the deleted form.
- Cases: `Feed` carries engagement evidence and its minimum fraction; `Smooth` geometry preservation; `Compact` collinear and co-circular tolerances; `Pattern` the pattern window, its occurrence floor, its first label, and its occurrence BUDGET; `Stability` the chatter lobes with the depth they are relative to and the margin floor a point must clear.
- Entry: `Optimize.Apply(CutProgram, OptimizationIngress, OptimizationEgress)` is the only public operation; `OptimizationEgress.Measurement` derives the final codec, termination, and framing with `BlockLimit.Observe`, while `Final` retains `BlockLimit.Enforce`.
- Auto: independent raw-policy failures accumulate through `Validation<Error, _>` before the `Fin<_>` execution rail; each policy admits through its generated `Validate` and the one `Admitted` bridge.
- Receipt: every admitted column carries its `UnitsNet` quantity past admission.
- Packages: `UnitsNet` supplies `Speed`, `RotationalSpeed`, `Length`, `Angle`, `Power`, `Ratio`, and `Duration`; `Process/physics#EQUIPMENT` `ProcessRange` supplies controller bounds; `Riok.Mapperly` owns the unit lift; `LanguageExt.Core` supplies `Validation<Error, _>`, applicative `Apply`, `Fin<_>`, and the equality-keyed `HashMap` carrier `BlockLocus` requires.
- Boundary: raw dimensional doubles, provider range types, and page-local cutting-force equations never cross admission; the ordered `Map` carrier never keys on a `[ComplexValueObject]`, which owns structural equality and no comparer.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------------------------------------------------------
using System;
using System.Linq;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Element.Projection;
using Rasm.Fabrication.Kinematics;
using Rasm.Fabrication.Process;
using Rasm.Fabrication.Tooling;
using Rhino.Geometry;
using Riok.Mapperly.Abstractions;
using Thinktecture;
using UnitsNet;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Posting;

// --- [TYPES] --------------------------------------------------------------------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class OptimizePass {
    // Stability fixes the SPINDLE and feed adaptation derives against it, so the stable point is upstream of every
    // feed the removal-rate pass writes; ordering it after would price a feed on a speed the next pass replaces.
    public static readonly OptimizePass StabilitySpeed = new("stability-speed", 5);
    public static readonly OptimizePass MrrFeed = new("mrr-feed", 10);
    public static readonly OptimizePass CornerSmooth = new("corner-smooth", 20);
    public static readonly OptimizePass Compact = new("compact", 30);
    public static readonly OptimizePass PatternFold = new("pattern-fold", 40);

    // The terminal feed certification. It is never SELECTED — `PassPolicy` carries no case for it — so this row
    // exists to NAME its delta on the ledger, where the stage previously rewrote every feed and published nothing.
    public static readonly OptimizePass Lookahead = new("lookahead", 50);

    public int Order { get; }
}

[ValueObject<int>(KeyMemberName = "Segments")]
public readonly partial struct PatternLength {
    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) =>
        validationError = value > 0 ? null : new ValidationError("optimization:pattern-length");
}

[ComplexValueObject]
public sealed partial class BlockLocus {
    public Seq<int> Path { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Seq<int> path) {
        if (path.IsEmpty || path.Exists(static index => index < 0))
            validationError = new ValidationError("optimization:locus");
    }

    public static Fin<BlockLocus> Admit(Seq<int> path) => Validate(path, out BlockLocus locus).Admitted(locus);
}

[ComplexValueObject]
public sealed partial class EngagementRow {
    public BlockLocus Locus { get; }
    public Length RadialDepth { get; }
    public Length AxialDepth { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError, ref BlockLocus locus, ref Length radialDepth, ref Length axialDepth) {
        if (radialDepth <= Length.Zero || axialDepth <= Length.Zero)
            validationError = new ValidationError("optimization:engagement");
    }

    public static Fin<EngagementRow> Admit(BlockLocus locus, Length radialDepth, Length axialDepth) =>
        Validate(locus, radialDepth, axialDepth, out EngagementRow row).Admitted(row);
}

// The machine and material envelope EVERY pass prices against — the cutting model, the controller's own resolved
// bounds, the tool, and the setup's nominal engagement. Seating these on the feed pass made the stability pass read
// a sibling pass's policy for its power ceiling, and forced a caller selecting compaction alone to supply all of it.
[ComplexValueObject]
public sealed partial class CutContext {
    public CuttingData Cutting { get; }
    public Speed ProcessFeed { get; }
    public Speed MinimumFeed { get; }
    public Speed MaximumFeed { get; }
    public RotationalSpeed MinimumSpindle { get; }
    public RotationalSpeed MaximumSpindle { get; }
    public RotationalSpeed ProgramSpindle { get; }
    public Length ToolDiameter { get; }
    public int Teeth { get; }
    public Length NominalRadialDepth { get; }
    public Length NominalAxialDepth { get; }
    public Option<Power> SpindlePower { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError, ref CuttingData cutting,
        ref Speed processFeed, ref Speed minimumFeed, ref Speed maximumFeed, ref RotationalSpeed minimumSpindle,
        ref RotationalSpeed maximumSpindle, ref RotationalSpeed programSpindle, ref Length toolDiameter, ref int teeth,
        ref Length nominalRadialDepth, ref Length nominalAxialDepth, ref Option<Power> spindlePower) {
        if (processFeed <= Speed.Zero || minimumFeed <= Speed.Zero || maximumFeed < minimumFeed
            || minimumSpindle <= RotationalSpeed.Zero || maximumSpindle < minimumSpindle
            || programSpindle <= RotationalSpeed.Zero
            || toolDiameter <= Length.Zero || teeth <= 0
            || nominalRadialDepth <= Length.Zero || nominalAxialDepth <= Length.Zero
            || nominalRadialDepth > toolDiameter
            || spindlePower.Exists(static value => value <= Power.Zero))
            validationError = new ValidationError("optimization:context");
    }

    // The controller publishes its bounds through whichever range slots it populates; the budget's own derived rate
    // is the floor a controller that publishes none leaves standing, and every slot is already an `Option` because
    // `Tooling/magazine#TOOL_ASSET` admitted the provider envelope before it reached this page.
    public static Fin<CutContext> Admit(ContextIngress raw) {
        Speed feedFloor = Speed.FromMillimetersPerMinutes(raw.Budget.FeedRate);
        RotationalSpeed spindleFloor = RotationalSpeed.FromRevolutionsPerMinute(raw.Budget.SpindleRpm);
        return Validate(
            raw.Cutting,
            feedFloor,
            Bound(raw.FeedRange.Minimum, Speed.FromMillimetersPerMinutes, feedFloor),
            Bound(raw.FeedRange.Maximum, Speed.FromMillimetersPerMinutes, feedFloor),
            Bound(raw.SpindleRange.Minimum, RotationalSpeed.FromRevolutionsPerMinute, spindleFloor),
            Bound(raw.SpindleRange.Maximum, RotationalSpeed.FromRevolutionsPerMinute, spindleFloor),
            spindleFloor,
            OptimizeMap.Mm(raw.ToolDiameterMm), raw.Teeth,
            OptimizeMap.Mm(raw.NominalRadialDepthMm), OptimizeMap.Mm(raw.NominalAxialDepthMm),
            raw.SpindlePowerWatts.Map(Power.FromWatts),
            out CutContext context).Admitted(context);
    }

    private static TQuantity Bound<TQuantity>(Option<double> published, Func<double, TQuantity> lift, TQuantity floor) =>
        published.Map(lift).IfNone(floor);
}

// One raw case per SELECTED pass. The flattened twenty-three-column ingress it replaces put five concerns in one
// positional list, so an adjacent pair of same-typed depths was one transposition away from a silently wrong cut.
public sealed record ContextIngress(
    ProcessBudget.Subtractive Budget,
    CuttingData Cutting,
    ProcessRange FeedRange,
    ProcessRange SpindleRange,
    int Teeth,
    double ToolDiameterMm,
    double NominalRadialDepthMm,
    double NominalAxialDepthMm,
    Option<double> SpindlePowerWatts);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PassIngress {
    private PassIngress() { }

    public sealed record Stability(
        StabilityLobes Lobes,
        double RequestedDepthMm,
        double MinimumMargin) : PassIngress;
    public sealed record Feed(
        double MinimumEngagementFraction,
        HashMap<BlockLocus, EngagementRow> Engagement) : PassIngress;
    public sealed record Smooth(
        double MaximumDeviationMm,
        double MinimumTurnRadians,
        double MinimumRadiusMm,
        double GeometryToleranceMm) : PassIngress;
    public sealed record Compact(double CollinearToleranceMm, double CocircularToleranceMm) : PassIngress;
    public sealed record Pattern(
        int MinimumLength,
        int MaximumLength,
        int MinimumOccurrences,
        int FirstLabel,
        int OccurrenceBudget) : PassIngress;
}

[ComplexValueObject]
public sealed partial class FeedPolicy {
    public Ratio MinimumEngagement { get; }
    public HashMap<BlockLocus, EngagementRow> Engagement { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError, ref Ratio minimumEngagement,
        ref HashMap<BlockLocus, EngagementRow> engagement) {
        if (minimumEngagement.DecimalFractions is <= 0.0 or > 1.0
            || engagement.AsIterable().Exists(static row => row.Key != row.Value.Locus))
            validationError = new ValidationError("optimization:feed");
    }
}

[ComplexValueObject]
public sealed partial class SmoothPolicy {
    public Length MaximumDeviation { get; }
    public Angle MinimumTurn { get; }
    public Length MinimumRadius { get; }
    public Length GeometryTolerance { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError, ref Length maximumDeviation,
        ref Angle minimumTurn, ref Length minimumRadius, ref Length geometryTolerance) {
        if (maximumDeviation <= Length.Zero || minimumTurn <= Angle.Zero || minimumTurn >= Angle.FromRadians(Math.PI)
            || minimumRadius <= Length.Zero || geometryTolerance <= Length.Zero)
            validationError = new ValidationError("optimization:smooth");
    }
}

[ComplexValueObject]
public sealed partial class CompactPolicy {
    public Length CollinearTolerance { get; }
    public Length CocircularTolerance { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError, ref Length collinearTolerance, ref Length cocircularTolerance) {
        if (collinearTolerance <= Length.Zero || cocircularTolerance <= Length.Zero)
            validationError = new ValidationError("optimization:compact");
    }
}

[ComplexValueObject]
public sealed partial class PatternPolicy {
    public PatternLength MinimumLength { get; }
    public PatternLength MaximumLength { get; }
    public int MinimumOccurrences { get; }
    public int FirstLabel { get; }

    // The rewrite re-enters on its own output, so the budget is what BOUNDS it: a program whose folded body still
    // matches would otherwise fold until the label space or the stack ran out.
    public int OccurrenceBudget { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError, ref PatternLength minimumLength,
        ref PatternLength maximumLength, ref int minimumOccurrences, ref int firstLabel, ref int occurrenceBudget) {
        if (maximumLength.Segments < minimumLength.Segments || minimumOccurrences <= 1
            || firstLabel <= 0 || occurrenceBudget <= 0)
            validationError = new ValidationError("optimization:pattern");
    }
}

// The gate the elected point must clear. It CONSUMES `StabilityLobes` and produces no bands of its own, which is
// why the chatter producer at `Tooling/cuttingdata#CHATTER_STABILITY` keeps the `StabilityPolicy` name.
[ComplexValueObject]
public sealed partial class StabilityGate {
    public StabilityLobes Lobes { get; }
    public Length RequestedDepth { get; }
    public Ratio MinimumMargin { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError, ref StabilityLobes lobes,
        ref Length requestedDepth, ref Ratio minimumMargin) {
        // A margin fraction is a share of the stable band, so it lies inside the unit interval; a zero floor admits
        // a point sitting exactly on a lobe crossing, which is the chatter boundary itself.
        if (requestedDepth <= Length.Zero || minimumMargin.DecimalFractions is <= 0.0 or > 1.0)
            validationError = new ValidationError("optimization:stability");
    }
}

// The ONE selected-pass family. The case IS the selection, so a pass carries exactly the policy it runs against and
// a roster beside these rows has nothing left to disagree with.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PassPolicy {
    private PassPolicy() { }

    public sealed record Stability(StabilityGate Gate) : PassPolicy;
    public sealed record Feed(FeedPolicy Policy) : PassPolicy;
    public sealed record Smooth(SmoothPolicy Policy) : PassPolicy;
    public sealed record Compact(CompactPolicy Policy) : PassPolicy;
    public sealed record Pattern(PatternPolicy Policy) : PassPolicy;

    public OptimizePass Pass => Switch(
        stability: static _ => OptimizePass.StabilitySpeed,
        feed: static _ => OptimizePass.MrrFeed,
        smooth: static _ => OptimizePass.CornerSmooth,
        compact: static _ => OptimizePass.Compact,
        pattern: static _ => OptimizePass.PatternFold);

    // Each case states its OWN precondition against the program it will rewrite. The prior form was a flat gate
    // list keyed on roster membership, so a new pass added a row to the roster and nothing forced a gate for it.
    internal K<Validation<Error>, Unit> Admits(CutProgram program, bool geometry) => Switch(
        state: (Program: program, Geometry: geometry),
        // A stable point is only actionable where the controller admits a spindle word at all; on a dialect that
        // admits none the elected speed would be recorded and never emitted.
        stability: static (context, _) => AdmissionSlots.Gate(GCommand.Spindle.Admits(context.Program.Dialect),
            FabConcern.Posting, "optimization:spindle-word", FabricationFault.Inadmissible),
        // The feed rewrite reads the cutting model and the engagement evidence alone, so it demands nothing of the
        // program the numeric admission has not already proved.
        feed: static (_, _) => AdmissionSlots.Gate(true, FabConcern.Posting, "optimization:feed-context", FabricationFault.Inadmissible),
        smooth: static (context, _) => AdmissionSlots.Gate(
            context.Geometry && context.Program.Dialect.Arc.Exists(
                static mode => mode == ArcMode.Ijk || mode == ArcMode.Both),
                    FabConcern.Posting, "optimization:arc-representation", FabricationFault.Inadmissible),
        compact: static (context, _) => AdmissionSlots.Gate(context.Geometry,
            FabConcern.Posting, "optimization:geometry-context", FabricationFault.Inadmissible),
        pattern: static (context, _) => AdmissionSlots.Gate(context.Program.Dialect.Subprogram != SubprogramGrammar.None,
            FabConcern.Posting, "optimization:subprogram-grammar", FabricationFault.Inadmissible));

}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record OptimizationIngress {
    private OptimizationIngress() { }

    public sealed record Raw(ContextIngress Context, Seq<PassIngress> Passes, PostPolicy Post) : OptimizationIngress;
    public sealed record Admitted(OptimizePolicy Policy) : OptimizationIngress;
}

[ComplexValueObject]
public sealed partial class OptimizePolicy {
    public CutContext Context { get; }
    public Seq<PassPolicy> Passes { get; }
    public PostPolicy Post { get; }

    [IgnoreMember]
    public MotionDynamics Dynamics => Post.Cut.Dynamics;

    [IgnoreMember]
    public Seq<PassPolicy> Ordered => toSeq(Passes.OrderBy(static row => row.Pass.Order));

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError, ref CutContext context,
        ref Seq<PassPolicy> passes, ref PostPolicy post) {
        // Two rows for one pass would run its rewrite twice against two policies, so distinctness is the only
        // invariant a selection carrying its own policy still needs.
        if (passes.Map(static row => row.Pass).Distinct().Count != passes.Count)
            validationError = new ValidationError("optimization:pass-repeat");
    }
}

[ComplexValueObject]
public sealed partial class OptimizationEgress {
    public EmitPolicy Final { get; }

    [IgnoreMember]
    public EmitPolicy Measurement => EmitPolicy.Create(
        Final.Codec, Final.NewLine, Final.FinalTerminator, Final.Frame, new BlockLimit.Observe());

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref EmitPolicy @final) {
        if (@final.Limit is not BlockLimit.Enforce)
            validationError = new ValidationError("optimization:egress");
    }
}

// The ONE unit lift. A millimetre double becoming a `Length` is a MAPPING, so it generates; the constructor fan
// that spelled every conversion at a call site is the deleted form.
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
internal static partial class OptimizeMap {
    [MapProperty(nameof(PassIngress.Smooth.MaximumDeviationMm), nameof(SmoothPolicy.MaximumDeviation), Use = nameof(Mm))]
    [MapProperty(nameof(PassIngress.Smooth.MinimumTurnRadians), nameof(SmoothPolicy.MinimumTurn), Use = nameof(Rad))]
    [MapProperty(nameof(PassIngress.Smooth.MinimumRadiusMm), nameof(SmoothPolicy.MinimumRadius), Use = nameof(Mm))]
    [MapProperty(nameof(PassIngress.Smooth.GeometryToleranceMm), nameof(SmoothPolicy.GeometryTolerance), Use = nameof(Mm))]
    public static partial SmoothPolicy Smooth(PassIngress.Smooth source);

    [MapProperty(nameof(PassIngress.Compact.CollinearToleranceMm), nameof(CompactPolicy.CollinearTolerance), Use = nameof(Mm))]
    [MapProperty(nameof(PassIngress.Compact.CocircularToleranceMm), nameof(CompactPolicy.CocircularTolerance), Use = nameof(Mm))]
    public static partial CompactPolicy Compact(PassIngress.Compact source);

    [MapProperty(nameof(PassIngress.Pattern.MinimumLength), nameof(PatternPolicy.MinimumLength), Use = nameof(Segments))]
    [MapProperty(nameof(PassIngress.Pattern.MaximumLength), nameof(PatternPolicy.MaximumLength), Use = nameof(Segments))]
    public static partial PatternPolicy Pattern(PassIngress.Pattern source);

    [MapProperty(nameof(PassIngress.Feed.MinimumEngagementFraction), nameof(FeedPolicy.MinimumEngagement), Use = nameof(Fraction))]
    public static partial FeedPolicy Feed(PassIngress.Feed source);

    [MapProperty(nameof(PassIngress.Stability.RequestedDepthMm), nameof(StabilityGate.RequestedDepth), Use = nameof(Mm))]
    [MapProperty(nameof(PassIngress.Stability.MinimumMargin), nameof(StabilityGate.MinimumMargin), Use = nameof(Fraction))]
    public static partial StabilityGate Stability(PassIngress.Stability source);

    [UserMapping]
    internal static Length Mm(double value) => Length.FromMillimeters(value);

    [UserMapping]
    internal static Angle Rad(double value) => Angle.FromRadians(value);

    [UserMapping]
    internal static Ratio Fraction(double value) => Ratio.FromDecimalFractions(value);

    [UserMapping]
    internal static PatternLength Segments(int value) => PatternLength.Create(value);
}
```

## [03]-[OPTIMIZATION]

- Owner: `OptimizePass` carries stage identity and declaration order; `PassState` threads the program, its trace, the elected stable point, and the accumulated deltas; `OptimizationResult` is the sole egress.
- Cases: five selected rows and one terminal certification fold over one `PassState`.
- Entry: `Optimize.Apply(CutProgram, OptimizationIngress, OptimizationEgress)` is the only public operation.
- Law: `PassState.Stable` is what makes the stability and feed passes COMPOSE — the elected point fixes the spindle every later feed derivation prices against and caps the axial depth it engages at. Without that column each pass would price against a policy constant the other pass had already replaced.
- Law: the terminal feed certification is a STAGE with its own delta. `Post.Lookahead` rewrites every block, macro, and subprogram feed after the selected passes settle, and the prior receipt read its objective off the certified program while every delta stopped one stage short — so the minutes the certification itself cost or saved were in the total and in no row.
- Law: `OptimizeLedger` states each endpoint ONCE. Baseline objective is the first stage's own `Before`, optimized objective the certification's own `After`, and estimated engagement and folded patterns are sums over the stages — every one of those was a top-level column restating a `PassDelta` the ledger already carried.
- Auto: baseline and optimized record counts come from `PostImage.PhysicalRecords`, the one census neither delta carries because a per-pass emit would render the whole program once per stage; `PassState` threads the interpretation forward so each stage interprets the program it produced exactly once. An intermediate program costs no content key, because `CutProgram` holds its key lazily.
- Receipt: `OptimizeLedger` carries the ordered stage deltas, the terminal certification delta, the elected point, and the baseline and optimized physical-record counts.
- Growth: one optimization concern adds one `PassPolicy` case, one `OptimizePass` row, and one pure `PassState` fold.
- Boundary: separate `Feeds`, `Delta`, and `Blocks` estimators are deleted forms; symbolic `GValue.Variable`/`Expression` motion fails admission because geometry-changing passes cannot preserve unevaluated coordinates by inspection.

```csharp signature
// --- [MODELS] -------------------------------------------------------------------------------------------------------------------------------------
public sealed record PassDelta(
    OptimizePass Pass,
    Duration Before,
    Duration After,
    int ChangedNodes,
    int EstimatedEngagement,
    int FoldedPatterns);

// The stage ledger. Its endpoints are the stages' own — the certification always lands, so the optimized objective
// is a column the last stage already published rather than a second measurement beside it.
public sealed record OptimizeLedger(
    Seq<PassDelta> Passes,
    PassDelta Certification,
    Option<StablePoint> Stable,
    int BaselineRecords,
    int OptimizedRecords) {
    public Duration BaselineObjective => Passes.Head.Map(static row => row.Before).IfNone(Certification.Before);
    public Duration OptimizedObjective => Certification.After;
    public Seq<PassDelta> Stages => Passes.Add(Certification);
    public int EstimatedEngagement => Stages.Fold(0, static (sum, row) => sum + row.EstimatedEngagement);
    public int FoldedPatterns => Stages.Fold(0, static (sum, row) => sum + row.FoldedPatterns);
}

public sealed record OptimizationResult(CutProgram Program, PostImage Image, OptimizeLedger Ledger);

internal readonly record struct MotionSpan(double Length, double Cruise, double Entry, double Minutes, Vector3d Direction);

// `Stable` is the elected chatter-free operating point, threaded so the feed pass prices against the speed the
// stability pass actually wrote rather than against the policy value it replaced.
internal sealed record PassState(
    CutProgram Program, ProgramTrace Trace, Option<StablePoint> Stable, Seq<PassDelta> Deltas);

// A pass returns its rewritten nodes AND the evidence it counted while rewriting them, so the receipt column and
// the tree it describes leave one transform together.
internal readonly record struct PassOutcome(Seq<GNode> Nodes, int Estimated, int Patterns) {
    public static PassOutcome Of(Seq<GNode> nodes) => new(nodes, 0, 0);
}

// --- [OPERATIONS] ---------------------------------------------------------------------------------------------------------------------------------
public static partial class Optimize {
    public static Fin<OptimizationResult> Apply(CutProgram program, OptimizationIngress ingress, OptimizationEgress egress) =>
        from policy in Admit(ingress)
        from _numeric in Numeric(program.Nodes)
        from baselineTrace in Post.Interpret(program)
        from _capability in Capability(program, policy, baselineTrace)
        from baselineImage in Dialect.Emit(program, egress.Measurement)
        let initial = new PassState(program, baselineTrace, Option<StablePoint>.None, Seq<PassDelta>())
        from folded in policy.Ordered.Fold(Fin.Succ(initial),
            (state, pass) => state.Bind(current => OptimizationCore.Fold(current, pass, policy)))
        from certified in OptimizationCore.Certify(folded, policy)
        from image in Dialect.Emit(certified.State.Program, egress.Final)
        select new OptimizationResult(certified.State.Program, image, new OptimizeLedger(
            folded.Deltas, certified.Delta, certified.State.Stable,
            baselineImage.PhysicalRecords, image.PhysicalRecords));

    private static Fin<OptimizePolicy> Admit(OptimizationIngress ingress) => ingress.Switch(
        raw: static raw => Admit(raw),
        admitted: static admitted => Fin.Succ(admitted.Policy));

    // The context and every selected pass admit independently, so a caller reads every violated invariant at once
    // rather than the first; the selection is the admitted sequence and no roster is carried beside it.
    private static Fin<OptimizePolicy> Admit(OptimizationIngress.Raw raw) =>
        (CutContext.Admit(raw.Context).ToValidation(),
         raw.Passes.Traverse(Admit).As())
        .Apply(static (context, passes) => (Context: context, Passes: passes))
        .As().ToFin()
        .Bind(admitted => OptimizePolicy.Validate(admitted.Context, admitted.Passes, raw.Post,
            out OptimizePolicy policy).Admitted(policy));

    private static Validation<Error, PassPolicy> Admit(PassIngress ingress) => ingress.Switch(
        stability: static row => StabilityGate.Validate(row.Lobes, OptimizeMap.Mm(row.RequestedDepthMm),
                OptimizeMap.Fraction(row.MinimumMargin), out StabilityGate gate)
            .Admitted(gate).Map<PassPolicy>(static value => new PassPolicy.Stability(value)).ToValidation(),
        feed: static row => FeedPolicy.Validate(OptimizeMap.Fraction(row.MinimumEngagementFraction), row.Engagement,
                out FeedPolicy policy)
            .Admitted(policy).Map<PassPolicy>(static value => new PassPolicy.Feed(value)).ToValidation(),
        smooth: static row => SmoothPolicy.Validate(OptimizeMap.Mm(row.MaximumDeviationMm),
                OptimizeMap.Rad(row.MinimumTurnRadians), OptimizeMap.Mm(row.MinimumRadiusMm),
                OptimizeMap.Mm(row.GeometryToleranceMm), out SmoothPolicy policy)
            .Admitted(policy).Map<PassPolicy>(static value => new PassPolicy.Smooth(value)).ToValidation(),
        compact: static row => CompactPolicy.Validate(OptimizeMap.Mm(row.CollinearToleranceMm),
                OptimizeMap.Mm(row.CocircularToleranceMm), out CompactPolicy policy)
            .Admitted(policy).Map<PassPolicy>(static value => new PassPolicy.Compact(value)).ToValidation(),
        pattern: static row => PatternPolicy.Validate(OptimizeMap.Segments(row.MinimumLength),
                OptimizeMap.Segments(row.MaximumLength), row.MinimumOccurrences, row.FirstLabel,
                row.OccurrenceBudget, out PatternPolicy policy)
            .Admitted(policy).Map<PassPolicy>(static value => new PassPolicy.Pattern(value)).ToValidation());

    private static Fin<Unit> Numeric(Seq<GNode> nodes) =>
        NodeWalk.Collect(nodes, Seq<int>(), static (_, node) => node switch {
            GNode.Word word when word.Command.Group == ModalGroup.Motion && !word.Words
                .Filter(static parameter => parameter.Address is 'X' or 'Y' or 'Z' or 'A' or 'B' or 'C' or 'U' or 'V' or 'W'
                    or 'I' or 'J' or 'K' or 'R' or 'F' or 'S')
                .ForAll(static parameter => parameter.Value.Scalar.IsSome) => Seq(unit),
            GNode.CannedCycle cycle when !cycle.SingleBlockWords.ForAll(static parameter => parameter.Value.Scalar.IsSome) => Seq(unit),
            _ => Seq<Unit>(),
        }).IsEmpty
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new FabricationFault.OptimizationRefused("admission", "symbolic-program"));

    // Every selected pass states its own precondition through the generated dispatch; independent refusals
    // accumulate, so a caller reads every unmet demand at once.
    private static Fin<Unit> Capability(CutProgram program, OptimizePolicy policy, ProgramTrace trace) {
        bool geometry = trace.Events.ForAll(static item => item switch {
            ProgramEvent.Motion motion => Seq('X', 'Y', 'Z').ForAll(address => motion.Word.P(address).IsSome),
            ProgramEvent.State state => state.Command != GCommand.Relative && state.Command != GCommand.ArcRelative
                && state.Command != GCommand.PlaneZx && state.Command != GCommand.PlaneYz,
            _ => true,
        });
        return policy.Passes.Traverse(pass => pass.Admits(program, geometry)).As().Map(static _ => unit).ToFin();
    }
}
```

## [04]-[OBJECTIVE]

- Owner: `Optimize.Objective` owns machine minutes over the whole trace.
- Law: objective time counts EVERY machine minute — rapids traverse at `MotionDynamics.RapidFeed`, feed motion is bounded by `LinearFeed` and `ArcFeed`, `FeedMode.InverseTime` reads one block as `1/F` minutes, and `GCommand.Dwell` contributes its `P` seconds for every commanded pause including pierce delay. A cutting-only objective reports compaction and linking gains it never earned.
- Law: each span is accel-limited — `MotionDynamics.JunctionFeed` fixes the shared speed at the turn between consecutive spans, and `Acceleration` bounds the reachable peak, so a fold of short segments cannot show its programmed feed. The junction speed is derived ONCE per advance: computing it twice from the same operands let the entry speed a span recorded and the elapsed time its predecessor was charged disagree.
- Receipt: `MotionSpan` carries length, cruise ceiling, entry speed, and chord direction, and the trapezoid closes each span when its successor fixes the exit; the last span decelerates to rest.
- Boundary: arc rows preserve their admitted `I`/`J` centre evidence, distances use full `Point3d` positions, no absent axis becomes zero, and no zero-length span becomes fabricated distance.

```csharp signature
// --- [OBJECTIVE] ----------------------------------------------------------------------------------------------------------------------------------
public static partial class Optimize {
    internal static Duration Objective(ProgramTrace trace, MotionDynamics dynamics) {
        (GCommand Plane, Option<MotionSpan> Pending, double Minutes) folded = trace.Events.Fold(
            (Plane: GCommand.PlaneXy, Pending: Option<MotionSpan>.None, Minutes: 0.0),
            (state, item) => item switch {
                ProgramEvent.State { Command: var command }
                    when command == GCommand.PlaneXy || command == GCommand.PlaneZx || command == GCommand.PlaneYz =>
                        (command, state.Pending, state.Minutes),
                ProgramEvent.State { Word: var word } when word.Command == GCommand.Dwell =>
                    (state.Plane, state.Pending, state.Minutes + (word.P('P').IfNone(0.0) / 60.0)),
                ProgramEvent.Motion motion when Span(motion, state.Plane, dynamics) is { Length: > 0.0 } span =>
                    Advance(state, span, dynamics),
                _ => state,
            });
        return Duration.FromMinutes(folded.Minutes + folded.Pending.Map(span => Elapsed(span, 0.0, dynamics)).IfNone(0.0));
    }

    // The junction speed is ONE derivation: it is both the entry the new span records and the exit its predecessor
    // is charged against, so deriving it twice let the two disagree on the same turn.
    private static (GCommand Plane, Option<MotionSpan> Pending, double Minutes) Advance(
        (GCommand Plane, Option<MotionSpan> Pending, double Minutes) state, MotionSpan span, MotionDynamics dynamics) {
        double junction = Junction(state.Pending, span, dynamics);
        return (state.Plane, Some(span with { Entry = junction }),
            state.Minutes + state.Pending.Map(pending => Elapsed(pending, junction, dynamics)).IfNone(0.0));
    }

    private static double Junction(Option<MotionSpan> pending, MotionSpan span, MotionDynamics dynamics) => pending
        .Map(previous => Math.Min(Math.Min(previous.Cruise, span.Cruise),
            dynamics.JunctionFeed(Vector3d.VectorAngle(previous.Direction, span.Direction))))
        .IfNone(0.0);

    // Exemption: the trapezoid closure is a numeric kernel — the ramp pair, the covered distance, and the cruise
    // remainder are one solution over the same peak, and splitting them re-solves it.
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
        return motion.Word.P('R').Match(
            Some: signedRadius => {
                double radius = Math.Abs(signedRadius);
                double chord = new Vector2d(ToU - FromU, ToV - FromV).Length;
                double minor = 2.0 * Math.Asin(Math.Clamp(chord / (2.0 * Math.Max(radius, double.Epsilon)), 0.0, 1.0));
                return Helical(radius * (signedRadius < 0.0 ? Math.Tau - minor : minor), ToW - FromW);
            },
            None: () => motion.Arc.Match(
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
                None: static () => 0.0));
    }

    private static double Helical(double planar, double rise) => Math.Sqrt((planar * planar) + (rise * rise));

    private static (double U, double V, double W) Project(Point3d point, GCommand plane) => plane == GCommand.PlaneZx
        ? (point.Z, point.X, point.Y)
        : plane == GCommand.PlaneYz ? (point.Y, point.Z, point.X) : (point.X, point.Y, point.Z);
}
```

## [05]-[WALK]

- Owner: `NodeWalk` owns the ONE structural recursion over a `GNode` sequence.
- Law: the descent through block, macro, and subprogram bodies lives HERE. Five parallel walkers each re-enumerated every node case, and two of them had already lost their directive arm — a label census that skipped directives and an engagement census that skipped them too, both compiling clean while silently under-reporting. One walker means a new case is one arm.
- Cases: `Deep` REWRITES on the rail — a leaf rewrite over words and cycles, and a level rewrite over each body sequence after its children settle; `Collect` FOLDS — one projection per node into an accumulated sequence.
- Auto: both entries thread the structural `BlockLocus`, so a pass keyed on engagement or a label reads the same path the trace publishes.
- Boundary: `NodeWalk` decides no domain question — it descends and re-seats bodies, and every rewrite is the caller's.

```csharp signature
// --- [WALK] ---------------------------------------------------------------------------------------------------------------------------------------
internal static class NodeWalk {
    // The ONE rewrite descent. `leaf` answers for a word or a cycle; `level` re-seats each body once its children
    // have settled, so a pass needing sequence context states it exactly once.
    internal static Fin<Seq<GNode>> Deep(
        Seq<GNode> nodes,
        Seq<int> prefix,
        Func<BlockLocus, GNode, Fin<GNode>> leaf,
        Func<Seq<GNode>, Fin<Seq<GNode>>> level) =>
        nodes.Map((node, index) => (Node: node, Locus: prefix.Add(index)))
            .Traverse(row => row.Node.Switch(
                state: (Locus: row.Locus, Leaf: leaf, Level: level),
                block: static (context, value) => Deep(value.Body.ToSeq(), context.Locus, context.Leaf, context.Level)
                    .Bind(context.Level).Map<GNode>(body => value with { Body = body.ToArr() }),
                word: static (context, value) => Seat(context.Locus, value, context.Leaf),
                cannedCycle: static (context, value) => Seat(context.Locus, value, context.Leaf),
                coordinateFrame: static (_, value) => Fin.Succ<GNode>(value),
                macro: static (context, value) => Deep(value.Body.ToSeq(), context.Locus, context.Leaf, context.Level)
                    .Bind(context.Level).Map<GNode>(body => value with { Body = body.ToArr() }),
                subprogram: static (context, value) => Deep(value.Body.ToSeq(), context.Locus, context.Leaf, context.Level)
                    .Bind(context.Level).Map<GNode>(body => value with { Body = body.ToArr() }),
                additiveLayer: static (_, value) => Fin.Succ<GNode>(value),
                nc1: static (_, value) => Fin.Succ<GNode>(value),
                directive: static (_, value) => Fin.Succ<GNode>(value)))
            .As()
            .Bind(level);

    // The ONE fold descent. Every census — labels, estimated engagement, symbolic residue — reads this, so a case
    // a census must answer for cannot be missing from one walker and present in another.
    internal static Seq<T> Collect<T>(Seq<GNode> nodes, Seq<int> prefix, Func<BlockLocus, GNode, Seq<T>> pick) =>
        nodes.Map((node, index) => (Node: node, Locus: prefix.Add(index)))
            .Bind(row => pick(Locus(row.Locus), row.Node) + row.Node.Switch(
                state: (Locus: row.Locus, Pick: pick),
                block: static (context, value) => Collect(value.Body.ToSeq(), context.Locus, context.Pick),
                word: static (_, _) => Seq<T>(),
                cannedCycle: static (_, _) => Seq<T>(),
                coordinateFrame: static (_, _) => Seq<T>(),
                macro: static (context, value) => Collect(value.Body.ToSeq(), context.Locus, context.Pick),
                subprogram: static (context, value) => Collect(value.Body.ToSeq(), context.Locus, context.Pick),
                additiveLayer: static (_, _) => Seq<T>(),
                nc1: static (_, _) => Seq<T>(),
                directive: static (_, _) => Seq<T>()));

    internal static Fin<Seq<GNode>> Level(Seq<GNode> nodes) => Fin.Succ(nodes);

    internal static Fin<GNode> Keep(BlockLocus locus, GNode node) => Fin.Succ(node);

    private static Fin<GNode> Seat(Seq<int> prefix, GNode node, Func<BlockLocus, GNode, Fin<GNode>> leaf) =>
        leaf(Locus(prefix), node);

    // The path is non-empty and index-derived by construction, so the admitted locus is total here and the
    // refusal arm names a caller that walked from an empty prefix.
    private static BlockLocus Locus(Seq<int> path) => BlockLocus.Create(path);
}
```

## [06]-[PASSES]

- Owner: `OptimizationCore` owns stability speed selection, feed adaptation, corner blending, compaction, pattern folding, and the terminal feed certification.
- Law: `Fold` is the ONE pass dispatch. Each `PassPolicy` case names its own transform through the generated switch, so a new case cannot compile without one and a delegate column on the pass roster cannot drift from the case it was declared beside.
- Law: `stability` intersects the `StabilityLobes.Recommend` point with the controller's own spindle operating envelope AND the machine power ceiling, and REFUSES `OptimizationRefused` when no point survives — a pass that silently kept the programmed speed would post a program the chatter model already rejected. The elected point rides `PassState.Stable`, so `feed` prices its rate against the speed that was actually written and caps its axial engagement at the stable depth.
- Law: `pattern` streams the structural `NodeKey` digest, never node bodies. A window's identity is a PREFIX HASH over that key stream, so the census is linear per window length and two windows compare in constant time; comparing bodies made every candidate a subtree walk. The rewrite re-enters on its own output under a declared OCCURRENCE BUDGET, so it terminates by policy.
- Law: every pass transform reads the nodes it is HANDED. A transform that closed over the incoming program and ignored its argument ran against the pre-pass tree whatever the fold passed it.
- Cases: `feed` evaluates `CuttingData.Evaluate(CutIntent)` for each subtractive locus, applies radial chip thinning, intersects process, controller, and spindle-power limits, and writes one explicit effective `F`; tapping and threading cycles are excluded because their feed is bound to pitch and spindle speed. `smooth` runs only after the trace proves absolute `G17` motion with explicit `X`/`Y`/`Z` and the dialect admits centre-form arcs. `compact` folds forward-collinear rapid and feed runs and co-circular arc runs while preserving traversed locus, and strips repeated modal `F` and `S` values. `Certify` is the terminal stage every run takes, so its rewrite publishes a delta like any other.
- Auto: a nested body executes under state the parent fold cannot see, so the modal census clears at every non-word node rather than stripping a word the body already changed; a locus with no measured engagement reads the context's nominal depths and increments the ledger's estimated count.
- Receipt: generated labels come only from the pattern policy, and `Dialect.Emit` hoists one definition per label regardless of call-site count.
- Exemption: `PatternCensus` is the measured hash kernel — the prefix array and the per-window read are one linear pass whose intermediate array IS the algorithm.
- Boundary: `Geometry2D/arcs` remains the owner for subsequent arc inspection, offset, and densification.

```csharp signature
// --- [PASSES] -------------------------------------------------------------------------------------------------------------------------------------
internal static class OptimizationCore {
    // Tapping and threading feed is bound to pitch and spindle speed, so adaptation never reaches them.
    private static readonly Set<GCommand> AdaptiveCycles = Set(GCommand.Drill, GCommand.DrillDwell, GCommand.Peck, GCommand.Bore);

    // The ONE pass dispatch: the case that carries the policy is the case that names the transform, so a selected
    // pass and the body it runs cannot be paired wrongly at the roster.
    internal static Fin<PassState> Fold(PassState state, PassPolicy pass, OptimizePolicy policy) => pass.Switch(
        state: (State: state, Policy: policy),
        stability: static (context, row) => Stability(context.State, row.Gate, context.Policy),
        feed: static (context, row) => MrrFeed(context.State, row.Policy, context.Policy),
        smooth: static (context, row) => Smooth(context.State, row.Policy, context.Policy),
        compact: static (context, row) => Compact(context.State, row.Policy, context.Policy),
        pattern: static (context, row) => PatternFold(context.State, row.Policy, context.Policy));

    // The elected chatter-free point, intersected with what the controller can actually command. A recommendation
    // outside the spindle envelope or above the power ceiling is not an operating point, so the pass refuses rather
    // than recording a speed the machine would clamp away.
    private static Fin<PassState> Stability(PassState state, StabilityGate gate, OptimizePolicy policy) =>
        gate.Lobes.Recommend(gate.RequestedDepth.Millimeters)
            .Filter(point => point.MarginFraction >= gate.MinimumMargin.DecimalFractions)
            .Filter(point => point.SpindleRpm >= policy.Context.MinimumSpindle.RevolutionsPerMinute
                && point.SpindleRpm <= policy.Context.MaximumSpindle.RevolutionsPerMinute)
            .Filter(point => Powered(point, policy.Context))
            .ToFin(new FabricationFault.OptimizationRefused(OptimizePass.StabilitySpeed.Key, "no-stable-point"))
            .Bind(point => Rewrite(state, OptimizePass.StabilitySpeed, policy,
                nodes => NodeWalk.Deep(nodes, Seq<int>(),
                    (_, node) => Fin.Succ(node is GNode.Word word && word.Command.Group == ModalGroup.Spindle
                        ? word.With('S', point.SpindleRpm)
                        : node),
                    NodeWalk.Level).Map(PassOutcome.Of),
                stable: Some(point)));

    // Cutting power rises with the spindle the point elects, so the ceiling is checked against the demand at that
    // speed rather than against the programmed one the pass is replacing.
    private static bool Powered(StablePoint point, CutContext context) => context.SpindlePower.ForAll(ceiling =>
        context.Cutting.Evaluate(Intent(context, OptimizeMap.Mm(point.DepthMm), context.NominalRadialDepth,
                context.Cutting.Feed, RotationalSpeed.FromRevolutionsPerMinute(point.SpindleRpm),
                context.ProcessFeed))
            .Map(load => load.Power <= ceiling)
            .IfFail(false));

    private static Fin<PassState> MrrFeed(PassState state, FeedPolicy feed, OptimizePolicy policy) => Rewrite(
        state, OptimizePass.MrrFeed, policy,
        nodes => NodeWalk.Deep(
            nodes,
            Seq<int>(),
            (locus, node) => node switch {
                GNode.Word word when Subtractive(word) =>
                    Rate(locus, feed, policy.Context, state.Stable).Map<GNode>(rate => word.With('F', rate.MillimetersPerMinutes)),
                GNode.CannedCycle cycle when CycleSubtractive(cycle) =>
                    Rate(locus, feed, policy.Context, state.Stable).Map<GNode>(rate => CycleFeed(cycle, rate)),
                _ => Fin.Succ(node),
            },
            NodeWalk.Level).Map(nodes => new PassOutcome(nodes, Estimated(nodes, feed), Patterns: 0)),
        stable: state.Stable);

    private static Fin<PassState> Smooth(PassState state, SmoothPolicy smooth, OptimizePolicy policy) => Rewrite(
        state, OptimizePass.CornerSmooth, policy,
        nodes => NodeWalk.Deep(nodes, Seq<int>(), NodeWalk.Keep,
            level => Fin.Succ(Blended(level, smooth, state.Program.Dialect))).Map(PassOutcome.Of),
        stable: state.Stable);

    private static Fin<PassState> Compact(PassState state, CompactPolicy compact, OptimizePolicy policy) => Rewrite(
        state, OptimizePass.Compact, policy,
        nodes => NodeWalk.Deep(nodes, Seq<int>(), NodeWalk.Keep,
            level => Fin.Succ(Compacted(level, compact, state.Program.Dialect))).Map(PassOutcome.Of),
        stable: state.Stable);

    // The transform reads the nodes it is HANDED and returns its own evidence with them; closing over the incoming
    // program made the fold run against the pre-pass tree, and smuggling the fold count out through a cell put the
    // receipt's evidence off the rail the nodes travelled on.
    private static Fin<PassState> PatternFold(PassState state, PatternPolicy pattern, OptimizePolicy policy) => Rewrite(
        state, OptimizePass.PatternFold, policy,
        nodes => Fin.Succ(Folded(nodes, pattern, NodeKey.Grid(state.Program.Dialect),
            pattern.FirstLabel, pattern.OccurrenceBudget)),
        stable: state.Stable);

    // The terminal stage every run takes. `Post.Lookahead` re-caps every block, macro, and subprogram feed against
    // the machine's own dynamics after the selected passes settle, so it rides the same stage body and publishes
    // the minutes it cost or saved instead of moving them silently into the total. It hands its delta BACK rather
    // than leaving the ledger to pull the last row off a sequence, so no caller carries a refusal for a row the
    // stage always writes.
    internal static Fin<(PassState State, PassDelta Delta)> Certify(PassState state, OptimizePolicy policy) =>
        Staged(state, OptimizePass.Lookahead, policy,
            nodes => Post.Lookahead(nodes, policy.Dynamics).Map(PassOutcome.Of),
            stable: state.Stable);

    private static Fin<PassState> Rewrite(PassState state, OptimizePass pass, OptimizePolicy policy,
        Func<Seq<GNode>, Fin<PassOutcome>> transform, Option<StablePoint> stable) =>
        Staged(state, pass, policy, transform, stable).Map(static staged => staged.State);

    // Incoming trace is the prior stage's result, so one interpretation per stage proves both ends of its delta,
    // and the stage's own counted evidence rides the same rail its nodes do.
    private static Fin<(PassState State, PassDelta Delta)> Staged(PassState state, OptimizePass pass,
        OptimizePolicy policy, Func<Seq<GNode>, Fin<PassOutcome>> transform, Option<StablePoint> stable) =>
        from outcome in transform(state.Program.Nodes)
        let program = CutProgram.Of(outcome.Nodes, state.Program.Dialect)
        from trace in Post.Interpret(program)
        let delta = new PassDelta(pass,
            Optimize.Objective(state.Trace, policy.Dynamics), Optimize.Objective(trace, policy.Dynamics),
            Changed(state.Program.Keys, program.Keys), outcome.Estimated, outcome.Patterns)
        select (new PassState(program, trace, stable, state.Deltas.Add(delta)), delta);

    private static Fin<Speed> Rate(BlockLocus locus, FeedPolicy feed, CutContext context, Option<StablePoint> stable) {
        EngagementRow engagement = feed.Engagement.Find(locus).IfNone(
            EngagementRow.Create(locus, context.NominalRadialDepth, context.NominalAxialDepth));
        // The elected stable point caps BOTH axes it fixes: the spindle the pass wrote and the depth that speed is
        // chatter-free at. Pricing against the context nominal would quote a feed for a cut the program no longer runs.
        double spindle = stable.Map(static point => point.SpindleRpm).IfNone(context.ProgramSpindle.RevolutionsPerMinute);
        Length axial = stable
            .Map(point => Length.FromMillimeters(Math.Min(engagement.AxialDepth.Millimeters, point.DepthMm)))
            .IfNone(engagement.AxialDepth);
        double bounded = Math.Clamp(spindle,
            context.MinimumSpindle.RevolutionsPerMinute, context.MaximumSpindle.RevolutionsPerMinute);
        double fraction = Math.Clamp(engagement.RadialDepth.Millimeters / context.ToolDiameter.Millimeters,
            feed.MinimumEngagement.DecimalFractions, 1.0);
        double thinning = fraction >= 0.5 ? 1.0 : 1.0 / Math.Sqrt(1.0 - Math.Pow(1.0 - (2.0 * fraction), 2.0));
        (double Feed, double Chip) basis = context.Cutting.FeedBasis.Switch(
            state: (Feed: context.Cutting.Feed, Process: context.ProcessFeed.MillimetersPerMinutes, Teeth: context.Teeth, Spindle: bounded),
            perTooth: static value => (value.Feed * value.Teeth * value.Spindle, value.Feed),
            perRevolution: static value => (value.Feed * value.Spindle, value.Feed / value.Teeth),
            linearPerMinute: static value => (value.Feed, value.Feed / (value.Teeth * value.Spindle)),
            surfaceRatio: static value => (value.Process, value.Process / (value.Teeth * value.Spindle)));
        double proposed = Math.Min(context.ProcessFeed.MillimetersPerMinutes, basis.Feed * thinning);
        return from load in context.Cutting.Evaluate(Intent(context, axial, engagement.RadialDepth,
                   basis.Chip, RotationalSpeed.FromRevolutionsPerMinute(bounded), Speed.FromMillimetersPerMinutes(proposed)))
               let powered = context.SpindlePower
                   .Map(ceiling => load.Power > ceiling ? proposed * ceiling.Watts / load.Power.Watts : proposed)
                   .IfNone(proposed)
               from rate in powered >= context.MinimumFeed.MillimetersPerMinutes
                   ? Fin.Succ(Speed.FromMillimetersPerMinutes(Math.Min(powered, context.MaximumFeed.MillimetersPerMinutes)))
                   : Fin.Fail<Speed>(new FabricationFault.OptimizationRefused(OptimizePass.MrrFeed.Key, "feed-envelope"))
               select rate;
    }

    // The intent is built by NAMED slot. Its chip-thickness and chip-width columns are adjacent same-typed lengths,
    // and the engaged edge length IS the axial depth for a peripheral cut, so positional construction put two
    // legitimately equal arguments side by side with nothing marking which was which.
    private static CutIntent Intent(
        CutContext context, Length axial, Length radial, double chipMm, RotationalSpeed spindle, Speed feed) =>
        CutIntent.Create(
            chipThickness: Length.FromMillimeters(chipMm),
            chipWidth: axial,
            axialDepth: axial,
            radialDepth: radial,
            diameter: context.ToolDiameter,
            teeth: context.Teeth,
            spindle: spindle,
            feed: feed);

    private static Seq<GNode> Blended(Seq<GNode> nodes, SmoothPolicy policy, PostDialect dialect) {
        GNode[] source = nodes.ToArray();
        return toSeq(Enumerable.Range(0, source.Length)).Bind(index => Blend(source, index, policy, dialect));
    }

    // Exemption: the corner blend is a geometric kernel — the tangent trim, the arc radius, and the admission
    // verdict all read one three-word window, and splitting them re-derives the window.
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

    private static Seq<GNode> Compacted(Seq<GNode> nodes, CompactPolicy policy, PostDialect dialect) =>
        StripModal(nodes.Fold(
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

    // The rewrite re-enters on its own output, so the budget bounds it: each accepted fold spends one, and a
    // program whose folded body still matches stops at the declared allowance rather than at a stack limit.
    private static PassOutcome Folded(
        Seq<GNode> nodes, PatternPolicy policy, double grid, int label, int budget) {
        if (budget <= 0)
            return PassOutcome.Of(nodes);
        Seq<UInt128> keys = NodeKey.Stream(nodes, grid);
        int available = NextLabel(nodes, label);
        return PatternCensus.Best(keys, policy).Match(
            Some: row => {
                Arr<GNode> body = nodes.Skip(row.Start).Take(row.Length).ToArr();
                Set<int> starts = toSet(row.Occurrences);
                Seq<GNode> rewritten = nodes.Map(static (node, index) => (Node: node, Index: index)).Bind(entry =>
                    starts.Contains(entry.Index) ? Seq<GNode>(new GNode.Subprogram(available, 1, body))
                    : starts.Exists(start => entry.Index > start && entry.Index < start + row.Length) ? Seq<GNode>()
                    : Seq(entry.Node));
                PassOutcome repeated = Folded(rewritten, policy, grid, checked(available + 1), budget - 1);
                return repeated with { Patterns = repeated.Patterns + 1 };
            },
            None: () => PassOutcome.Of(nodes));
    }

    private static int NextLabel(Seq<GNode> nodes, int candidate) =>
        NodeWalk.Collect(nodes, Seq<int>(), static (_, node) =>
                node is GNode.Subprogram subprogram ? Seq(subprogram.Label) : Seq<int>())
            .Exists(label => label == candidate)
                ? NextLabel(nodes, checked(candidate + 1))
                : candidate;

    private static Point3d Point(GNode.Word word, Point3d prior) => new(
        word.P('X').IfNone(prior.X), word.P('Y').IfNone(prior.Y), word.P('Z').IfNone(prior.Z));

    private static GNode.Word Carry(GNode.Word survivor, GNode.Word merged) =>
        Seq('X', 'Y', 'Z', 'F', 'S').Fold(survivor,
            (current, address) => merged.P(address).Match(
                Some: value => current.With(address, value),
                None: () => current));

    private static bool Subtractive(GNode.Word word) => word.Command.Role == MotionRole.Cutting;

    private static bool CycleSubtractive(GNode.CannedCycle cycle) => AdaptiveCycles.Contains(cycle.Command);

    private static GNode.CannedCycle CycleFeed(GNode.CannedCycle cycle, Speed feed) => cycle with {
        SingleBlockWords = cycle.SingleBlockWords
            .Filter(static parameter => parameter.Address != 'F')
            .Add(GParam.Number('F', feed.MillimetersPerMinutes, ProgramUnits.Metric)),
        ExpandedMoves = cycle.ExpandedMoves.Map(move => move.Switch(
            rapid: static rapid => (Move)rapid,
            linear: linear => linear with { Feed = feed.MillimetersPerMinutes },
            circular: circular => circular with { Feed = feed.MillimetersPerMinutes })),
    };

    private static int Estimated(Seq<GNode> nodes, FeedPolicy policy) =>
        NodeWalk.Collect(nodes, Seq<int>(), (locus, node) => node switch {
            GNode.Word word when Subtractive(word) && !policy.Engagement.ContainsKey(locus) => Seq(1),
            GNode.CannedCycle cycle when CycleSubtractive(cycle) && !policy.Engagement.ContainsKey(locus) =>
                Seq(Math.Max(1, cycle.Repeats)),
            _ => Seq<int>(),
        }).Fold(0, static (sum, count) => sum + count);

    // Changed nodes read the KEY streams, so a subtree that moved without changing compares in constant time and a
    // structural diff never re-walks a body.
    private static int Changed(Seq<UInt128> before, Seq<UInt128> after) =>
        before.Zip(after).Count(static pair => pair.Item1 != pair.Item2) + Math.Abs(before.Count - after.Count);
}

// The pattern census over the structural key stream. Exemption: the prefix array is a measured hash kernel — one
// linear pass builds it and every window reads it in constant time, so the array IS the algorithm.
internal static class PatternCensus {
    // FNV-1a's 64-bit offset basis and prime. The polynomial needs one base and one seed; borrowing a named pair
    // keeps the census free of a tuning constant of its own.
    private const ulong Seed = 14695981039346656037UL;
    private const ulong Prime = 1099511628211UL;

    internal readonly record struct Candidate(int Start, int Length, Seq<int> Occurrences);

    internal static Option<Candidate> Best(Seq<UInt128> keys, PatternPolicy policy) {
        ulong[] prefix = Prefix(keys);
        ulong[] powers = Powers(keys.Count);
        return toSeq(Enumerable
            .Range(policy.MinimumLength.Segments,
                Math.Max(0, policy.MaximumLength.Segments - policy.MinimumLength.Segments + 1))
            .SelectMany(length => Windows(keys, prefix, powers, length, policy)))
            .Fold(Option<Candidate>.None, static (best, row) => best
                .Filter(held => (Saving(held.Occurrences.Count, held.Length), held.Length)
                    .CompareTo((Saving(row.Occurrences.Count, row.Length), row.Length)) >= 0)
                .IfNone(row));
    }

    // One bucket per window hash, then one key-slice verification per bucket member — the hash makes the census
    // linear and the verification is what keeps a collision from folding two different bodies onto one label.
    private static IEnumerable<Candidate> Windows(
        Seq<UInt128> keys, ulong[] prefix, ulong[] powers, int length, PatternPolicy policy) =>
        Enumerable.Range(0, Math.Max(0, keys.Count - length + 1))
            .GroupBy(start => Window(prefix, powers, start, length))
            .Select(group => toSeq(group))
            .Where(group => group.Count >= policy.MinimumOccurrences)
            .Select(group => new Candidate(group.Head.IfNone(0), length,
                Disjoint(keys, group, group.Head.IfNone(0), length)))
            .Where(row => row.Occurrences.Count >= policy.MinimumOccurrences && Saving(row.Occurrences.Count, row.Length) > 0);

    private static Seq<int> Disjoint(Seq<UInt128> keys, Seq<int> starts, int seed, int length) =>
        starts
            .Filter(start => Enumerable.Range(0, length).All(offset => keys[seed + offset] == keys[start + offset]))
            .Fold((Starts: Seq<int>(), End: -1), (state, index) => index >= state.End
                ? (state.Starts.Add(index), index + length)
                : state).Starts;

    private static ulong Window(ulong[] prefix, ulong[] powers, int start, int length) =>
        prefix[start + length] - (prefix[start] * powers[length]);

    private static ulong[] Prefix(Seq<UInt128> keys) {
        ulong[] rows = new ulong[keys.Count + 1];
        rows[0] = Seed;
        for (int index = 0; index < keys.Count; index++)
            rows[index + 1] = (rows[index] * Prime) + Lane(keys[index]);
        return rows;
    }

    private static ulong[] Powers(int count) {
        ulong[] rows = new ulong[count + 1];
        rows[0] = 1UL;
        for (int index = 1; index <= count; index++)
            rows[index] = rows[index - 1] * Prime;
        return rows;
    }

    // The digest is a hundred and twenty-eight bits and the polynomial runs on sixty-four, so both halves fold in;
    // taking one half alone would collide every pair of nodes agreeing on it.
    private static ulong Lane(UInt128 key) => ContentHash.Half(key, 0) ^ ContentHash.Half(key, 1);

    // Occurrences replaced by one call each, one hoisted definition, and its two framing records.
    private static int Saving(int occurrences, int length) => ((occurrences - 1) * length) - occurrences - 2;
}
```

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
