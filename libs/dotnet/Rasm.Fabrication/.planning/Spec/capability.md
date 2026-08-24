# [RASM_FABRICATION_CAPABILITY]

`Capability` owns characteristic-scoped process evidence from admission through control-state, distribution, measurement-system, tolerance-stack, history, and plan-gate projection. Variable and attribute studies share one assessment rail, and every report preserves the evidence that makes its verdict reproducible.

`CapabilityIdentity`, `ToleranceChain`, `ProcedureReceipt`, `Stat`, `CapabilityVerdict`, and `FabricationFault` remain the seam owners. `CapabilityReport` is the terminal specification receipt, while `CapabilityHistory` carries its validity-bounded ledger projection into `Gate` and `Achievable`.

## [01]-[INDEX]

- [02]-[CAPABILITY_VOCABULARY]: the index roster and its estimation methods beside the generated SPC chart, rule, attribute-chart, control-evidence, and control-constant rows.
- [03]-[DISTRIBUTION_FIT]: the fitted family union, its support-gated moment seeds, and the policy every numeric the fitting lane spends rides.
- [04]-[STUDY_ADMISSION]: study identity, measurement-system evidence, control policy, the attribute cohort, and the chain-bound stackup contributors.
- [05]-[ASSESSMENT]: `Capability.Assess` over either study case, control-limit and violation derivation, the correlated stackup, and the ledger projections.
- [06]-[HISTORY]: the validity-bounded ledger row and the durable slots it rides.

## [02]-[CAPABILITY_VOCABULARY]

- Owner: `CapabilityMetric` owns the index roster, the spread adjustment each definition applies, and the standard error each index carries; `CapabilityMethod` owns moment and percentile estimation over one `CapabilitySpread`; `CapabilitySide` owns the one sided-index algebra both methods enter; `ControlEvidence` owns the three findings a controlled process holds; `SpcChart`, `SpcRule`, `SpcRuleClass`, `AttributeChart`, and `ControlConstant` own generated control policy.
- Law: `CapabilityMethod` closes moment and ISO 22514-4 percentile estimation, so the fitted distribution decides the non-normal index instead of decorating the report; the demand itself is the fourth operand, so neither method takes a page constant nor a bare double whose axis is unstated.
- Law: `SpcChart.Admits` grades each rule class per chart, so every chart signals on its own control band while the zone and pattern ladders stay on symmetric equal-variance charts.
- Auto: calibrated `ControlConstant` rows carry the range mean and spread the subgroup limits derive from, and a subgroup past the calibrated roster hands spread to the s-chart rather than extrapolating a d2 that was never published.
- Growth: a capability index is one `CapabilityMetric` row carrying its own scale, side, and spread adjustment; a control rule is one `SpcRule` row carrying its `SpcRuleClass`; an attribute chart is one `AttributeChart` row carrying when it is derivable, how a sample plots on it, and how the cohort interval scales onto it; a control finding is one `ControlEvidence` row.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------------------------------------------------------
using CommunityToolkit.HighPerformance.Buffers;
using CommunityToolkit.HighPerformance.Helpers;
using Foundation.CSharp.Analyzers.Contracts;
using LanguageExt;
using LanguageExt.Common;
using LanguageExt.Traits;
using MathNet.Numerics;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.RootFinding;
using NodaTime;
using Rasm.Analysis;
using Rasm.Domain;
using Rasm.Element.Projection;           // AdmissionSlots — the one accumulating slot algebra every gate lifts through
using System.Numerics.Tensors;
using UnitsNet;
using Rasm.Fabrication.Joining;
using Rasm.Fabrication.Process;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Spec;

// --- [VOCABULARY] ---------------------------------------------------------------------------------------------------------------------------------
// The row reads the moment column it names. A boolean beside a ternary answered one question in two places, and
// a third scale — a between-subgroup component, a pooled estimate — could not be a row there at all.
[SmartEnum<string>]
public sealed partial class CapabilityScale {
    public static readonly CapabilityScale Short = new("short", static moment => moment.WithinSigma);
    public static readonly CapabilityScale Long = new("long", static moment => moment.OverallSigma);

    [UseDelegateFromConstructor]
    private partial double Component(CapabilityMoment moment);

    public double Sigma(CapabilityMoment moment) => double.Max(Component(moment), double.Epsilon);
}

[SmartEnum<string>]
public sealed partial class CapabilityMethod {
    public static readonly CapabilityMethod Moment = new("moment", static (scale, moment, _, tolerance) =>
        Some(Symmetric(moment.Mean, tolerance.SpreadSpan.Of(scale.Sigma(moment)))));
    public static readonly CapabilityMethod Percentile = new("percentile", static (_, _, fitted, tolerance) =>
        from row in fitted
        from spread in Capability.QuantileSpread(row.Parameters, tolerance)
        select spread);

    // The demand itself is the fourth operand: the moment arm reads the index span it declares and the percentile
    // arm the tail it declares, so neither method takes a page constant nor a bare double whose axis is unstated.
    public Func<CapabilityScale, CapabilityMoment, Option<CapabilityDistribution>, CapabilityTolerance, Option<CapabilitySpread>> Of { get; }

    private static CapabilitySpread Symmetric(double center, double half) => new(center, half, half);
}

[SmartEnum<string>]
public sealed partial class CapabilitySide {
    public static readonly CapabilitySide Lower = new("lower");
    public static readonly CapabilitySide Upper = new("upper");
    public static readonly CapabilitySide Bilateral = new("bilateral");

    public Option<double> Index(CapabilitySpread spread, CapabilityTolerance tolerance) =>
        Switch(
            state: (spread, tolerance),
            lower: static state => state.tolerance.LowerSpecMm.Map(lower => (state.spread.Center - lower) / state.spread.Lower),
            upper: static state => state.tolerance.UpperSpecMm.Map(upper => (upper - state.spread.Center) / state.spread.Upper),
            bilateral: static state =>
                from lower in state.tolerance.LowerSpecMm
                from upper in state.tolerance.UpperSpecMm
                select (upper - lower) / (state.spread.Lower + state.spread.Upper));
}

// Each row carries the SPREAD ADJUSTMENT its index definition applies — identity for every published index but
// Taguchi's, which inflates by the off-target term. A boolean naming that one row put the correction's formula at
// the fold instead of at its owner, so a second adjusted index could not be a row and the fold branched per read.
[SmartEnum<string>]
public sealed partial class CapabilityMetric {
    public static readonly CapabilityMetric Cp = Moment("cp", CapabilityScale.Short, CapabilitySide.Bilateral);
    public static readonly CapabilityMetric Cpk = Moment("cpk", CapabilityScale.Short, side: null);
    public static readonly CapabilityMetric Cpu = Moment("cpu", CapabilityScale.Short, CapabilitySide.Upper);
    public static readonly CapabilityMetric Cpl = Moment("cpl", CapabilityScale.Short, CapabilitySide.Lower);
    public static readonly CapabilityMetric Pp = Moment("pp", CapabilityScale.Long, CapabilitySide.Bilateral);
    public static readonly CapabilityMetric Ppk = Moment("ppk", CapabilityScale.Long, side: null);
    public static readonly CapabilityMetric Ppu = Moment("ppu", CapabilityScale.Long, CapabilitySide.Upper);
    public static readonly CapabilityMetric Ppl = Moment("ppl", CapabilityScale.Long, CapabilitySide.Lower);
    // ISO 22514-2 Cpm: the spread widens by the root of one plus the squared standardized off-target displacement,
    // so an on-target process reads its bilateral index and a displaced one is charged for the displacement.
    public static readonly CapabilityMetric Cpm = new("cpm", CapabilityMethod.Moment, CapabilityScale.Long,
        Some(CapabilitySide.Bilateral), Taguchi);
    public static readonly CapabilityMetric PpQuantile = Quantile("pp-q", CapabilitySide.Bilateral);
    public static readonly CapabilityMetric PpkQuantile = Quantile("ppk-q", side: null);
    public static readonly CapabilityMetric PpuQuantile = Quantile("ppu-q", CapabilitySide.Upper);
    public static readonly CapabilityMetric PplQuantile = Quantile("ppl-q", CapabilitySide.Lower);

    public CapabilityMethod Method { get; }
    public CapabilityScale Scale { get; }
    public Option<CapabilitySide> Side { get; }

    [UseDelegateFromConstructor]
    public partial CapabilitySpread Adjust(CapabilitySpread spread, CapabilityMoment moment, CapabilityTolerance tolerance);

    // ISO 22514-4: the percentile method estimates spread from fitted quantiles, so a non-normal fit gates its own rows.
    public Option<double> Of(CapabilityMoment moment, Option<CapabilityDistribution> fitted, CapabilityTolerance tolerance) =>
        from spread in Method.Of(Scale, moment, fitted, tolerance)
        let adjusted = Adjust(spread, moment, tolerance)
        from index in Side.Match(
            Some: side => side.Index(adjusted, tolerance),
            None: () => Closest(adjusted, tolerance))
        select index;

    // A declared target is what the correction reads, so a row demanding one and a study declaring none reports the
    // uncorrected spread rather than a figure derived from a target nobody stated.
    private static CapabilitySpread Taguchi(CapabilitySpread spread, CapabilityMoment moment, CapabilityTolerance tolerance) =>
        tolerance.TargetMm
            .Map(target => Math.Sqrt(1.0 + Math.Pow((moment.Mean - target) / CapabilityScale.Long.Sigma(moment), 2.0)))
            .Map(correction => spread with { Lower = spread.Lower * correction, Upper = spread.Upper * correction })
            .IfNone(spread);

    private static CapabilitySpread Unadjusted(CapabilitySpread spread, CapabilityMoment _, CapabilityTolerance __) => spread;

    // Bissell: a one-sided index carries the mean-estimation term the bilateral index does not.
    public double StandardError(double value, double sampleSize) =>
        Side == CapabilitySide.Bilateral
            ? Math.Abs(value) / Math.Sqrt(2.0 * double.Max(sampleSize - 1.0, 1.0))
            : Math.Sqrt((1.0 / (9.0 * double.Max(sampleSize, 1.0)))
                + (value * value / (2.0 * double.Max(sampleSize - 1.0, 1.0))));

    private static CapabilityMetric Moment(string key, CapabilityScale scale, CapabilitySide? side) =>
        new(key, CapabilityMethod.Moment, scale, Optional(side), Unadjusted);

    private static CapabilityMetric Quantile(string key, CapabilitySide? side) =>
        new(key, CapabilityMethod.Percentile, CapabilityScale.Long, Optional(side), Unadjusted);

    private static Option<double> Closest(CapabilitySpread spread, CapabilityTolerance tolerance) {
        Option<double> lower = CapabilitySide.Lower.Index(spread, tolerance);
        Option<double> upper = CapabilitySide.Upper.Index(spread, tolerance);
        return (from low in lower from high in upper select double.Min(low, high)) | lower | upper;
    }
}

// A controlled process holds THREE independent facts, each bounded by its own `ControlPolicy` column: no rule fired
// on any chart, no autocorrelation lag exceeded its bound, and the drift slope stayed inside its own. One boolean
// conjunction answered all three, so a gate refusing an uncontrolled process could never name WHICH evidence was
// absent and a ledger row published a verdict no consumer could reopen. `Controlled` survives as the derived read
// of the whole set, so every consumer of that projection compiles unchanged while the refusal gains its evidence.
[SmartEnum<string>]
public sealed partial class ControlEvidence : ICapability<ControlEvidence> {
    public static readonly ControlEvidence Stable = new("stable");
    public static readonly ControlEvidence Independent = new("independent");
    public static readonly ControlEvidence Stationary = new("stationary");
}

[SmartEnum<string>]
public sealed partial class SpcRuleClass {
    public static readonly SpcRuleClass Limit = new("limit");
    public static readonly SpcRuleClass Zone = new("zone");
    public static readonly SpcRuleClass Pattern = new("pattern");
}

[SmartEnum<string>]
public sealed partial class SpcChart {
    public static readonly SpcChart Individuals = Western("i", attribute: false);
    public static readonly SpcChart MovingRange = Bounded("mr", attribute: false);
    public static readonly SpcChart XBar = Western("xbar", attribute: false);
    public static readonly SpcChart Range = Bounded("r", attribute: false);
    public static readonly SpcChart Sigma = Bounded("s", attribute: false);
    public static readonly SpcChart Ewma = Bounded("ewma", attribute: false);
    public static readonly SpcChart Cusum = Bounded("cusum", attribute: false);
    public static readonly SpcChart P = Bounded("p", attribute: true);
    public static readonly SpcChart Np = Bounded("np", attribute: true);
    public static readonly SpcChart C = Bounded("c", attribute: true);
    public static readonly SpcChart U = Bounded("u", attribute: true);

    public bool Attribute { get; }
    public Set<SpcRuleClass> Rules { get; }

    public bool Admits(SpcRule rule) => Rules.Contains(rule.Class);

    // A count charts a non-negative quantity, so its lower limit floors at zero where a variables chart's runs
    // negative — the row states its own floor rather than every limit site re-reading the attribute flag.
    public double LowerLimit(double center, double band) =>
        Attribute ? double.Max(0.0, center - band) : center - band;

    // Every chart signals on its own limits; only a symmetric, equal-variance chart admits the zone and pattern ladder.
    private static SpcChart Bounded(string key, bool attribute) => new(key, attribute, Set(SpcRuleClass.Limit));

    private static SpcChart Western(string key, bool attribute) =>
        new(key, attribute, Set(SpcRuleClass.Limit, SpcRuleClass.Zone, SpcRuleClass.Pattern));
}

[SmartEnum<string>]
public sealed partial class SpcRule {
    public static readonly SpcRule BeyondLimits = Limit("beyond-limits");
    public static readonly SpcRule TwoOfThreeBeyondTwoSigma = Zone("two-of-three-2s", window: 3, minimum: 2, zone: 2.0);
    public static readonly SpcRule FourOfFiveBeyondOneSigma = Zone("four-of-five-1s", window: 5, minimum: 4, zone: 1.0);
    public static readonly SpcRule EightOnOneSide = Pattern("eight-one-side", window: 8,
        static values => values.ForAll(static value => value > 0.0) || values.ForAll(static value => value < 0.0));
    public static readonly SpcRule SixTrending = Pattern("six-trending", window: 6,
        static values => Trending(Steps(values)));
    public static readonly SpcRule FourteenAlternating = Pattern("fourteen-alternating", window: 14,
        static values => Alternating(Steps(values)));
    public static readonly SpcRule FifteenWithinOneSigma = Pattern("fifteen-within-1s", window: 15,
        static values => values.ForAll(static value => Math.Abs(value) < 1.0));
    public static readonly SpcRule EightOutsideOneSigma = Pattern("eight-outside-1s", window: 8,
        static values => values.ForAll(static value => Math.Abs(value) > 1.0));

    public SpcRuleClass Class { get; }
    public int Window { get; }
    public Func<Arr<double>, bool> Breach { get; }

    // Limit breaches read the row's own control band, so a configured sigma width never disagrees with a literal zone.
    private static SpcRule Limit(string key) =>
        new(key, SpcRuleClass.Limit, window: 1, static values => values.Exists(static value => Math.Abs(value) > 1.0));

    private static SpcRule Zone(string key, int window, int minimum, double zone) =>
        new(key, SpcRuleClass.Zone, window,
            values => int.Max(values.Count(value => value > zone), values.Count(value => value < -zone)) >= minimum);

    private static SpcRule Pattern(string key, int window, Func<Arr<double>, bool> breach) =>
        new(key, SpcRuleClass.Pattern, window, breach);

    private static Arr<int> Steps(Arr<double> values) {
        Seq<double> walk = toSeq(values);
        return walk.Skip(1).Zip(walk, static (next, prior) => Math.Sign(next - prior)).ToArr();
    }

    private static bool Trending(Arr<int> steps) =>
        steps.ForAll(static step => step > 0) || steps.ForAll(static step => step < 0);

    private static bool Alternating(Arr<int> steps) {
        Seq<int> walk = toSeq(steps);
        return walk.ForAll(static step => step != 0)
            && walk.Zip(walk.Skip(1), static (first, second) => first == -second).ForAll(identity);
    }
}

[SmartEnum<int>]
public sealed partial class ControlConstant {
    public static readonly ControlConstant N2 = new(2, 1.128, 0.853);
    public static readonly ControlConstant N3 = new(3, 1.693, 0.888);
    public static readonly ControlConstant N4 = new(4, 2.059, 0.880);
    public static readonly ControlConstant N5 = new(5, 2.326, 0.864);
    public static readonly ControlConstant N6 = new(6, 2.534, 0.848);
    public static readonly ControlConstant N7 = new(7, 2.704, 0.833);
    public static readonly ControlConstant N8 = new(8, 2.847, 0.820);
    public static readonly ControlConstant N9 = new(9, 2.970, 0.808);
    public static readonly ControlConstant N10 = new(10, 3.078, 0.797);

    public double RangeMean { get; }
    public double RangeSigma { get; }

    // Range charts are calibrated only where a d2/d3 row exists; beyond it the s-chart owns spread.
    public static int SmallestSubgroup => Items.Min(static row => row.Key);
    public static int LargestSubgroup => Items.Max(static row => row.Key);

    public static ControlConstant Nearest(int subgroupSize) =>
        Get(int.Clamp(subgroupSize, SmallestSubgroup, LargestSubgroup));
}
```

## [03]-[DISTRIBUTION_FIT]

- Owner: `DistributionParameters` closes the fitted continuous families and owns the free-parameter count the criterion charges each for; `DistributionFamily` seeds them by support; `SearchBracket` owns the one positive ordered interval every bracketed search runs over; `DistributionPolicy` owns every numeric the fitting lane spends and the ONE root-find that spends them.
- Law: distribution selection is PENALIZED. A nested richer family always tracks a sample at least as closely, so Akaike's criterion over the fitted log-likelihood charges each family for its free parameters and the reported supremum stays the evidence a reader compares on; the count is a property of the FAMILY, so selection never reflects over a record's positional arity.
- Law: moment matching is a ROW per family, so a candidate space grows by one declaration and never by editing a seeding body, and a family whose support the sample violates seeds nothing rather than fitting an impossible fit.
- Auto: `Generate.LinearSpacedMap` generates the Student candidate fan and one bracketed `Brent.TryFindRoot` serves every quantile and shape MathNet exposes no closed inverse for.
- Growth: a distribution is one `DistributionParameters` case with one `DistributionFamily` seed row and one free-parameter arm.
- Boundary: a policy value here decides a caller's fit; a page-level constant is a policy column hiding from its own owner.

```csharp signature
// --- [MODELS] -------------------------------------------------------------------------------------------------------------------------------------
// A COUNT of process standard deviations — the one axis five bare doubles across two policies and one demand were
// each spending under a name that spelled it: a control band's half-width, a CUSUM slack and its decision interval,
// a quantile bracket's reach, and the index span a moment method spreads to. Every one of them was separately
// admitted, separately guarded, and could be handed a millimetre or a probability by a caller reading the name
// rather than the axis. The carrier states the axis once, so the member names drop the suffix that was standing in
// for a type, and `Of` is the one place a span becomes a magnitude. A zero span is admissible because a CUSUM with
// no slack is a real configuration; an owner needing a strictly positive span demands it at its own admission.
[ValueObject<double>]
public readonly partial struct SigmaSpan {
    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) =>
        validationError = ValidityClaim.Nonnegative(value) ? null : Capability.Validation("sigma-span");

    public double Of(double sigma) => ToValue() * sigma;
}

// A positive ordered interval, admitted ONCE. The fitting policy carried two of them as four bare doubles under two
// hand-repeated ordering clauses — the Student freedom band and the shape bracket — and every root-find then spread
// the same pair back across two positional arguments. One owner states the invariant, and the interval it names is
// what a search runs over rather than a pair a call site re-assembles.
[ComplexValueObject]
public sealed partial class SearchBracket {
    public double Lower { get; }
    public double Upper { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError,
        ref double lower, ref double upper) =>
        validationError = ValidityClaim.All(
            ValidityClaim.Positive(lower), ValidityClaim.Ordered(lower, upper), lower < upper)
            ? null
            : Capability.Validation("search-bracket");

    public double Span => Upper - Lower;
}

// The fitting policy owns EVERY numeric the fitting lane spends: the candidate fan, the Student freedom band, the
// shape bracket and the budget every root-find runs at, the quantile bracket the axis carrier now names, and the
// draw seed the fit and spread share. A page-level constant here is a policy value hiding from its own owner.
[ComplexValueObject]
public sealed partial class DistributionPolicy {
    public int CandidateCount { get; }
    public SearchBracket StudentFreedom { get; }
    public SearchBracket Shape { get; }

    // The residual magnitude Brent converges to — a difference of two cumulative probabilities on the quantile
    // path and of two gamma ratios on the shape path, so it is dimensionless by construction and carries no axis.
    public double RootAccuracy { get; }
    public int RootIterations { get; }
    public SigmaSpan Bracket { get; }
    public int FitSeed { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref int candidateCount,
        ref SearchBracket studentFreedom,
        ref SearchBracket shape,
        ref double rootAccuracy,
        ref int rootIterations,
        ref SigmaSpan bracket,
        ref int fitSeed) =>
        // The Student band's own floor is the family's: a t below two degrees of freedom has no finite variance,
        // so a moment-matched scale off it is a number the fit cannot mean.
        validationError = ValidityClaim.All(
            ValidityClaim.CountAtLeast(candidateCount, 2),
            studentFreedom is not null && studentFreedom.Lower > 2.0,
            shape is not null,
            ValidityClaim.Positive(rootAccuracy),
            ValidityClaim.CountAtLeast(rootIterations, 1),
            ValidityClaim.Positive(bracket.ToValue()))
            ? null
            : Capability.Validation("distribution-policy");

    // The ONE bracketed root-find on the page. Two call sites spread bracket, accuracy, and iteration budget across
    // five positional arguments and each re-spelled the `out`-parameter-to-`Option` lift; the budget is this policy's
    // own fact, so the bracket is the only thing a caller still names.
    public Option<double> Root(Func<double, double> residual, SearchBracket over) =>
        Brent.TryFindRoot(residual, over.Lower, over.Upper, RootAccuracy, RootIterations, out double root)
            ? Some(root)
            : None;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DistributionParameters : IValidityEvidence {
    private DistributionParameters() { }

    // The free-parameter count Akaike's criterion charges each family for. It is a property of the FAMILY, so the
    // selection never reflects over a record's positional arity or hard-codes a count at the selecting fold.
    public int FreeParameters => Switch(
        normal: static _ => 2,
        logNormal: static _ => 2,
        gamma: static _ => 2,
        student: static _ => 3,
        weibull: static _ => 2,
        beta: static _ => 2,
        chiSquared: static _ => 1,
        exponential: static _ => 1,
        uniform: static _ => 2,
        cauchy: static _ => 2,
        laplace: static _ => 2,
        rayleigh: static _ => 1,
        fisher: static _ => 2,
        triangular: static _ => 3,
        pareto: static _ => 2,
        inverseGamma: static _ => 2,
        betaScaled: static _ => 4,
        logistic: static _ => 2);

    public sealed record Normal(double Mean, double Sigma) : DistributionParameters;
    public sealed record LogNormal(double Mu, double Sigma) : DistributionParameters;
    public sealed record Gamma(double Shape, double Rate) : DistributionParameters;
    public sealed record Student(double Location, double Scale, double Freedom) : DistributionParameters;
    public sealed record Weibull(double Shape, double Scale) : DistributionParameters;
    public sealed record Beta(double A, double B) : DistributionParameters;
    public sealed record ChiSquared(double Freedom) : DistributionParameters;
    public sealed record Exponential(double Rate) : DistributionParameters;
    public sealed record Uniform(double Lower, double Upper) : DistributionParameters;
    public sealed record Cauchy(double Location, double Scale) : DistributionParameters;
    public sealed record Laplace(double Location, double Scale) : DistributionParameters;
    public sealed record Rayleigh(double Scale) : DistributionParameters;
    public sealed record Fisher(double D1, double D2) : DistributionParameters;
    public sealed record Triangular(double Lower, double Upper, double Mode) : DistributionParameters;
    public sealed record Pareto(double Scale, double Shape) : DistributionParameters;
    public sealed record InverseGamma(double Shape, double Scale) : DistributionParameters;
    public sealed record BetaScaled(double A, double B, double Lower, double Upper) : DistributionParameters;
    public sealed record Logistic(double Mean, double Scale) : DistributionParameters;

    // Each arm states the support ITS family requires, through the kernel claim rows: the page-local `Positive`
    // helper that stood here was `ValidityClaim.Positive` under a second name, and a family whose free parameter
    // has a hard floor — a Student below two degrees of freedom, a Fisher below four, a Pareto below one — names
    // that floor as an ordering rather than as a bare comparison a reader has to recognize as a support test.
    public bool IsValid => Switch(
        normal: static value => ValidityClaim.All(ValidityClaim.Finite(value.Mean), ValidityClaim.Positive(value.Sigma)),
        logNormal: static value => ValidityClaim.All(ValidityClaim.Finite(value.Mu), ValidityClaim.Positive(value.Sigma)),
        gamma: static value => ValidityClaim.All(ValidityClaim.Positive(value.Shape), ValidityClaim.Positive(value.Rate)),
        student: static value => ValidityClaim.All(ValidityClaim.Finite(value.Location),
            ValidityClaim.Positive(value.Scale), ValidityClaim.Ordered(2.0, value.Freedom), value.Freedom > 2.0),
        weibull: static value => ValidityClaim.All(ValidityClaim.Positive(value.Shape), ValidityClaim.Positive(value.Scale)),
        beta: static value => ValidityClaim.All(ValidityClaim.Positive(value.A), ValidityClaim.Positive(value.B)),
        chiSquared: static value => ValidityClaim.Positive(value.Freedom),
        exponential: static value => ValidityClaim.Positive(value.Rate),
        uniform: static value => ValidityClaim.All(ValidityClaim.Ordered(value.Lower, value.Upper), value.Upper > value.Lower),
        cauchy: static value => ValidityClaim.All(ValidityClaim.Finite(value.Location), ValidityClaim.Positive(value.Scale)),
        laplace: static value => ValidityClaim.All(ValidityClaim.Finite(value.Location), ValidityClaim.Positive(value.Scale)),
        rayleigh: static value => ValidityClaim.Positive(value.Scale),
        fisher: static value => ValidityClaim.All(ValidityClaim.Positive(value.D1),
            ValidityClaim.Ordered(4.0, value.D2), value.D2 > 4.0),
        triangular: static value => ValidityClaim.All(ValidityClaim.Ordered(value.Lower, value.Upper),
            value.Upper > value.Lower, ValidityClaim.Ordered(value.Lower, value.Mode),
            ValidityClaim.Ordered(value.Mode, value.Upper)),
        pareto: static value => ValidityClaim.All(ValidityClaim.Positive(value.Scale),
            ValidityClaim.Ordered(1.0, value.Shape), value.Shape > 1.0),
        inverseGamma: static value => ValidityClaim.All(ValidityClaim.Ordered(2.0, value.Shape),
            value.Shape > 2.0, ValidityClaim.Positive(value.Scale)),
        betaScaled: static value => ValidityClaim.All(ValidityClaim.Positive(value.A), ValidityClaim.Positive(value.B),
            ValidityClaim.Ordered(value.Lower, value.Upper), value.Upper > value.Lower),
        logistic: static value => ValidityClaim.All(ValidityClaim.Finite(value.Mean), ValidityClaim.Positive(value.Scale)));

    public bool FiniteMoments => this switch {
        Cauchy => false,
        Pareto value => value.Shape > 2.0,
        _ => true,
    };

    public IContinuousDistribution Create(Random random) => Switch(
        state: random,
        normal: static (rng, value) => new MathNet.Numerics.Distributions.Normal(value.Mean, value.Sigma, rng),
        logNormal: static (rng, value) => new MathNet.Numerics.Distributions.LogNormal(value.Mu, value.Sigma, rng),
        gamma: static (rng, value) => new MathNet.Numerics.Distributions.Gamma(value.Shape, value.Rate, rng),
        student: static (rng, value) => new StudentT(value.Location, value.Scale, value.Freedom, rng),
        weibull: static (rng, value) => new MathNet.Numerics.Distributions.Weibull(value.Shape, value.Scale, rng),
        beta: static (rng, value) => new MathNet.Numerics.Distributions.Beta(value.A, value.B, rng),
        chiSquared: static (rng, value) => new MathNet.Numerics.Distributions.ChiSquared(value.Freedom, rng),
        exponential: static (rng, value) => new MathNet.Numerics.Distributions.Exponential(value.Rate, rng),
        uniform: static (rng, value) => new ContinuousUniform(value.Lower, value.Upper, rng),
        cauchy: static (rng, value) => new MathNet.Numerics.Distributions.Cauchy(value.Location, value.Scale, rng),
        laplace: static (rng, value) => new MathNet.Numerics.Distributions.Laplace(value.Location, value.Scale, rng),
        rayleigh: static (rng, value) => new MathNet.Numerics.Distributions.Rayleigh(value.Scale, rng),
        fisher: static (rng, value) => new FisherSnedecor(value.D1, value.D2, rng),
        triangular: static (rng, value) => new MathNet.Numerics.Distributions.Triangular(value.Lower, value.Upper, value.Mode, rng),
        pareto: static (rng, value) => new MathNet.Numerics.Distributions.Pareto(value.Scale, value.Shape, rng),
        inverseGamma: static (rng, value) => new MathNet.Numerics.Distributions.InverseGamma(value.Shape, value.Scale, rng),
        betaScaled: static (rng, value) => new MathNet.Numerics.Distributions.BetaScaled(value.A, value.B, value.Lower, value.Upper, rng),
        logistic: static (rng, value) => new MathNet.Numerics.Distributions.Logistic(value.Mean, value.Scale, rng));
}

[SmartEnum<string>]
public sealed partial class DistributionSupport {
    public static readonly DistributionSupport Real = new("real", static _ => true);
    public static readonly DistributionSupport Positive = new("positive", static moment => moment.Minimum > 0.0);
    public static readonly DistributionSupport UnitInterval = new("unit-interval",
        static moment => moment.Minimum >= 0.0 && moment.Maximum <= 1.0);
    public static readonly DistributionSupport Bounded = new("bounded",
        static moment => moment.Maximum > moment.Minimum);

    public Func<CapabilityMoment, bool> Admits { get; }
}

// Moment matching is a row per family, so a candidate space grows by one declaration and never by editing a seeding body.
[SmartEnum<string>]
public sealed partial class DistributionFamily {
    public static readonly DistributionFamily Normal = One("normal", DistributionSupport.Real,
        static (moment, sigma, _) => new DistributionParameters.Normal(moment.Mean, sigma));
    public static readonly DistributionFamily Cauchy = One("cauchy", DistributionSupport.Real,
        static (moment, sigma, _) => new DistributionParameters.Cauchy(moment.Mean, sigma));
    public static readonly DistributionFamily Laplace = One("laplace", DistributionSupport.Real,
        static (moment, sigma, _) => new DistributionParameters.Laplace(moment.Mean, sigma / Math.Sqrt(2.0)));
    public static readonly DistributionFamily Logistic = One("logistic", DistributionSupport.Real,
        static (moment, sigma, _) => new DistributionParameters.Logistic(moment.Mean, sigma * Math.Sqrt(3.0) / Math.PI));
    public static readonly DistributionFamily Uniform = One("uniform", DistributionSupport.Real,
        static (moment, sigma, _) => new DistributionParameters.Uniform(
            moment.Mean - (Math.Sqrt(3.0) * sigma), moment.Mean + (Math.Sqrt(3.0) * sigma)));
    public static readonly DistributionFamily Triangular = One("triangular", DistributionSupport.Real,
        static (moment, sigma, _) => new DistributionParameters.Triangular(
            moment.Mean - (Math.Sqrt(6.0) * sigma), moment.Mean + (Math.Sqrt(6.0) * sigma), moment.Mean));
    public static readonly DistributionFamily Student = new("student", DistributionSupport.Real,
        static (moment, sigma, policy) => toSeq(Generate.LinearSpacedMap(
            policy.CandidateCount, policy.StudentFreedom.Lower, policy.StudentFreedom.Upper,
            freedom => (DistributionParameters)new DistributionParameters.Student(
                moment.Mean, sigma * Math.Sqrt((freedom - 2.0) / freedom), freedom))));
    public static readonly DistributionFamily LogNormal = One("log-normal", DistributionSupport.Positive,
        static (moment, sigma, _) => LogParameters(moment.Mean, sigma));
    public static readonly DistributionFamily Gamma = One("gamma", DistributionSupport.Positive,
        static (moment, sigma, _) => new DistributionParameters.Gamma(
            Math.Pow(moment.Mean / sigma, 2.0), moment.Mean / (sigma * sigma)));
    public static readonly DistributionFamily Exponential = One("exponential", DistributionSupport.Positive,
        static (moment, _, _) => new DistributionParameters.Exponential(1.0 / moment.Mean));
    public static readonly DistributionFamily Rayleigh = One("rayleigh", DistributionSupport.Positive,
        static (moment, _, _) => new DistributionParameters.Rayleigh(
            double.Max(moment.Mean / Math.Sqrt(Math.PI / 2.0), double.Epsilon)));
    public static readonly DistributionFamily Pareto = One("pareto", DistributionSupport.Positive,
        static (moment, _, _) => new DistributionParameters.Pareto(moment.Minimum,
            double.Max(1.01, moment.Mean / double.Max(moment.Mean - moment.Minimum, double.Epsilon))));
    public static readonly DistributionFamily InverseGamma = One("inverse-gamma", DistributionSupport.Positive,
        static (moment, sigma, _) => new DistributionParameters.InverseGamma(
            2.0 + Math.Pow(moment.Mean / sigma, 2.0), moment.Mean * (1.0 + Math.Pow(moment.Mean / sigma, 2.0))));
    public static readonly DistributionFamily ChiSquared = One("chi-squared", DistributionSupport.Positive,
        static (moment, _, _) => new DistributionParameters.ChiSquared(double.Max(moment.Mean, double.Epsilon)));
    public static readonly DistributionFamily Fisher = One("fisher", DistributionSupport.Positive,
        static (_, _, policy) => new DistributionParameters.Fisher(5.0, double.Max(5.0, policy.StudentFreedom.Upper)));
    public static readonly DistributionFamily Weibull = new("weibull", DistributionSupport.Positive,
        static (moment, sigma, policy) => WeibullShape(sigma / moment.Mean, policy)
            .Map(shape => (DistributionParameters)new DistributionParameters.Weibull(
                shape, moment.Mean / SpecialFunctions.Gamma(1.0 + (1.0 / shape))))
            .ToSeq());
    public static readonly DistributionFamily BetaScaled = new("beta-scaled", DistributionSupport.Bounded,
        static (moment, sigma, _) => Shape(moment.Minimum, moment.Maximum, moment.Mean, sigma)
            .Map(shape => (DistributionParameters)new DistributionParameters.BetaScaled(
                shape.A, shape.B, moment.Minimum, moment.Maximum))
            .ToSeq());
    public static readonly DistributionFamily Beta = new("beta", DistributionSupport.UnitInterval,
        static (moment, sigma, _) => Shape(0.0, 1.0, moment.Mean, sigma)
            .Map(shape => (DistributionParameters)new DistributionParameters.Beta(shape.A, shape.B))
            .ToSeq());

    public DistributionSupport Support { get; }
    public Func<CapabilityMoment, double, DistributionPolicy, Seq<DistributionParameters>> Seed { get; }

    public Seq<DistributionParameters> Candidates(CapabilityMoment moment, double sigma, DistributionPolicy policy) =>
        Support.Admits(moment) ? Seed(moment, sigma, policy).Filter(static row => row.IsValid) : Seq<DistributionParameters>();

    private static DistributionFamily One(string key, DistributionSupport support,
        Func<CapabilityMoment, double, DistributionPolicy, DistributionParameters> seed) =>
        new(key, support, (moment, sigma, policy) => Seq(seed(moment, sigma, policy)));

    private static DistributionParameters LogParameters(double mean, double sigma) =>
        LogNormalOf(mean, Math.Sqrt(Math.Log(1.0 + Math.Pow(sigma / mean, 2.0))));

    private static DistributionParameters LogNormalOf(double mean, double logSigma) =>
        new DistributionParameters.LogNormal(Math.Log(mean) - (logSigma * logSigma / 2.0), logSigma);

    private static Option<double> WeibullShape(double coefficient, DistributionPolicy policy) =>
        policy.Root(
            shape => (SpecialFunctions.Gamma(1.0 + (2.0 / shape)) / Math.Pow(SpecialFunctions.Gamma(1.0 + (1.0 / shape)), 2.0))
                - 1.0 - (coefficient * coefficient),
            policy.Shape);

    private static Option<(double A, double B)> Shape(double lower, double upper, double mean, double sigma) {
        double width = upper - lower;
        double normalizedMean = (mean - lower) / width;
        double normalizedVariance = Math.Pow(sigma / width, 2.0);
        double concentration = (normalizedMean * (1.0 - normalizedMean) / normalizedVariance) - 1.0;
        return width > 0.0 && normalizedMean is > 0.0 and < 1.0 && normalizedVariance > 0.0 && concentration > 0.0
            ? Some((normalizedMean * concentration, (1.0 - normalizedMean) * concentration))
            : None;
    }
}
```

## [04]-[STUDY_ADMISSION]

- Owner: `CapabilityStudy` closes variable and attribute evidence; `CapabilityTolerance` carries the characteristic demand, control policy, measurement study, optional procedure, and optional stackup; `MeasurementEvidence` carries variable gage and attribute agreement studies; `StackContributor` and `StackupPolicy` carry the stochastic half of a declared tolerance chain.
- Law: the attribute cohort folds ONCE per study and threads through limits and rows as a NAMED record, so no column is addressed by tuple position and no second pass re-derives sums already in hand.
- Law: the contributor roster is a BIJECTION onto the chain's terms, and a contributor carries only what the term cannot — the systematic offset, the shared-factor loadings, and an optional measured fit overriding the term's declared family.
- Law: every policy and study numeric carries its AXIS. `SigmaSpan` is the count of process standard deviations five columns were each spelling with a name suffix — a control band, a CUSUM slack and decision interval, a quantile bracket, and the index span a moment method spreads to; a gage variation is a `Length`, a proportion is a `Ratio`, and a bare double survives only where the quantity is genuinely dimensionless, stated at its own member.
- Law: admission composes the kernel claim vocabulary. `ValidityClaim.All` over `Positive`, `Nonnegative`, `UnitInterval`, `Ordered`, and `CountAtLeast` is what each generated hook spells, so a bound is never re-derived and a folder-local scalar predicate has no site here.
- Boundary: `CapabilityIdentity` carries the `DiameterBand` its study measured, so `Gate` and `Achievable` resolve through one identity and no row authorizes a size it never observed.
- Growth: a study modality is one `CapabilityStudy` case folded by `Assess`.

```csharp signature
// --- [MODELS] -------------------------------------------------------------------------------------------------------------------------------------
[ComplexValueObject]
public sealed partial class CapabilityIdentity {
    public ProcessKind Process { get; }
    public UInt128 Characteristic { get; }
    public DiameterBand Feature { get; }
    public UInt128 Machine { get; }
    public UInt128 Material { get; }
    public UInt128 Tool { get; }
    public ToolEvidence ToolState { get; }
    public UInt128 Setup { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref ProcessKind process,
        ref UInt128 characteristic,
        ref DiameterBand feature,
        ref UInt128 machine,
        ref UInt128 material,
        ref UInt128 tool,
        ref ToolEvidence toolState,
        ref UInt128 setup) =>
        validationError = ValidityClaim.All(
            process is not null, characteristic != 0, feature is not null,
            machine != 0, material != 0, tool != 0, toolState is not null, setup != 0)
            ? null
            : Capability.Validation("identity");
}

[ComplexValueObject]
// Six of these columns are gage VARIATIONS in the characteristic's own unit and one is a proportion of total
// variation; all seven stood as bare doubles under an `Mm` suffix that told a caller what the type would not, so a
// micrometre reading, a percentage, and a millimetre reading were one shape. `GrrFraction` is a `Ratio` because the
// study's own demand is one, and the hundred that scaled it was a presentation choice inside a comparison.
public sealed partial class VariableMeasurementStudy {
    public Length Repeatability { get; }
    public Length Reproducibility { get; }
    public Length PartVariation { get; }
    public Length Bias { get; }
    public Length Linearity { get; }
    public Length Stability { get; }
    public Ratio MaximumGrr { get; }
    public int MinimumDistinctCategories { get; }

    public Length Grr => Length.FromMillimeters(Math.Sqrt(
        (Repeatability.Millimeters * Repeatability.Millimeters)
        + (Reproducibility.Millimeters * Reproducibility.Millimeters)));

    // The AIAG %GRR against total variation, carried as the fraction it is: the study's demand is a fraction, so
    // one comparison reads one axis and a demand stated in percent converts at its own boundary.
    public Ratio GrrFraction => Ratio.FromDecimalFractions(Grr.Millimeters / Math.Sqrt(
        (Grr.Millimeters * Grr.Millimeters) + (PartVariation.Millimeters * PartVariation.Millimeters)));

    public int DistinctCategories =>
        (int)Math.Floor(1.41 * PartVariation.Millimeters / double.Max(Grr.Millimeters, double.Epsilon));

    public bool Suitable => GrrFraction <= MaximumGrr && DistinctCategories >= MinimumDistinctCategories;

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Length repeatability,
        ref Length reproducibility,
        ref Length partVariation,
        ref Length bias,
        ref Length linearity,
        ref Length stability,
        ref Ratio maximumGrr,
        ref int minimumDistinctCategories) =>
        validationError = ValidityClaim.All(
            Seq(repeatability, reproducibility, bias, linearity, stability)
                .ForAll(static value => ValidityClaim.Nonnegative(value.Millimeters)),
            // Part variation divides the ratio and floors the category count, so a zero here is not a small study
            // but an unmeasurable one.
            ValidityClaim.Positive(partVariation.Millimeters),
            ValidityClaim.UnitInterval(maximumGrr.DecimalFractions),
            ValidityClaim.Positive(maximumGrr.DecimalFractions),
            ValidityClaim.CountAtLeast(minimumDistinctCategories, 1))
                ? null
                : Capability.Validation("variable-msa");
}

[ComplexValueObject]
// Every column is a proportion, so every column is a `Ratio`. Kappa is the one whose band runs to minus one —
// perfect disagreement is a real reading and clamping it to the unit interval would report a systematically
// inverted appraiser as merely unhelpful — so it and its demand carry the chance-corrected band explicitly.
public sealed partial class AttributeAgreementStudy {
    public Ratio AppraiserAgreement { get; }
    public Ratio StandardAgreement { get; }
    public Ratio Kappa { get; }
    public Ratio FalseAcceptRate { get; }
    public Ratio MissRate { get; }
    public Ratio MinimumAgreement { get; }
    public Ratio MinimumKappa { get; }
    public Ratio MaximumFalseDecisionRate { get; }

    public bool Suitable => AppraiserAgreement >= MinimumAgreement && StandardAgreement >= MinimumAgreement
        && Kappa >= MinimumKappa
        && Ratio.FromDecimalFractions(double.Max(FalseAcceptRate.DecimalFractions, MissRate.DecimalFractions))
            <= MaximumFalseDecisionRate;

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Ratio appraiserAgreement,
        ref Ratio standardAgreement,
        ref Ratio kappa,
        ref Ratio falseAcceptRate,
        ref Ratio missRate,
        ref Ratio minimumAgreement,
        ref Ratio minimumKappa,
        ref Ratio maximumFalseDecisionRate) =>
        validationError = ValidityClaim.All(
            Seq(appraiserAgreement, standardAgreement, falseAcceptRate, missRate, minimumAgreement,
                    maximumFalseDecisionRate)
                .ForAll(static value => ValidityClaim.UnitInterval(value.DecimalFractions)),
            Seq(kappa, minimumKappa).ForAll(static value =>
                ValidityClaim.Ordered(-1.0, value.DecimalFractions)
                && ValidityClaim.Ordered(value.DecimalFractions, 1.0)))
                ? null
                : Capability.Validation("attribute-msa");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MeasurementEvidence {
    private MeasurementEvidence() { }

    public sealed record Variable(VariableMeasurementStudy Study) : MeasurementEvidence;
    public sealed record Attribute(AttributeAgreementStudy Study) : MeasurementEvidence;

    public bool Suitable => Switch(
        variable: static evidence => evidence.Study.Suitable,
        attribute: static evidence => evidence.Study.Suitable);
}

// Every bound here now carries its own axis. Three columns were counts of process sigma, two were probabilities
// spelled as bare doubles, and one was a length spelled as a bare double — six numerics a caller could transpose
// with no diagnostic, under names that were doing the carrier's job with a suffix.
[ComplexValueObject]
public sealed partial class ControlPolicy {
    public int SubgroupSize { get; }
    public int MinimumObservations { get; }

    // The control band's half-width, and the CUSUM slack and decision interval, all read in process sigmas.
    public SigmaSpan Band { get; }
    public SigmaSpan CusumSlack { get; }
    public SigmaSpan CusumDecision { get; }
    public Ratio EwmaWeight { get; }
    public int MaximumLag { get; }

    // Bounds on the ABSOLUTE reading: a correlation at any admitted lag, and the per-sample drift slope the
    // stationarity finding measures — so a negative slope of equal magnitude fails the same bound a positive does.
    public Ratio MaximumAutocorrelation { get; }
    public Length MaximumDrift { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref int subgroupSize,
        ref int minimumObservations,
        ref SigmaSpan band,
        ref SigmaSpan cusumSlack,
        ref SigmaSpan cusumDecision,
        ref Ratio ewmaWeight,
        ref int maximumLag,
        ref Ratio maximumAutocorrelation,
        ref Length maximumDrift) =>
        // A subgroup study needs at least two subgroups' worth of observations, and an individuals study at least
        // two points, so the floor is the subgroup size against a hard two rather than either alone.
        validationError = ValidityClaim.All(
            ValidityClaim.CountAtLeast(subgroupSize, 1),
            ValidityClaim.CountAtLeast(minimumObservations, int.Max(2, subgroupSize)),
            ValidityClaim.Positive(band.ToValue()),
            ValidityClaim.Positive(cusumDecision.ToValue()),
            ValidityClaim.UnitInterval(ewmaWeight.DecimalFractions),
            ValidityClaim.Positive(ewmaWeight.DecimalFractions),
            ValidityClaim.CountAtLeast(maximumLag, 1),
            ValidityClaim.UnitInterval(maximumAutocorrelation.DecimalFractions),
            ValidityClaim.Nonnegative(maximumDrift.Millimeters))
                ? null
                : Capability.Validation("control-policy");
}

[ComplexValueObject]
public sealed partial class AttributeSample {
    public int Inspected { get; }
    public int Nonconforming { get; }
    public int Defects { get; }
    public int Opportunities { get; }
    public Instant At { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref int inspected,
        ref int nonconforming,
        ref int defects,
        ref int opportunities,
        ref Instant at) =>
        validationError = ValidityClaim.All(
            ValidityClaim.CountAtLeast(inspected, 1),
            ValidityClaim.CountAtLeast(nonconforming, 0),
            ValidityClaim.CountAtLeast(inspected, nonconforming),
            ValidityClaim.CountAtLeast(defects, 0),
            ValidityClaim.CountAtLeast(opportunities, defects),
            ValidityClaim.CountAtLeast(opportunities, inspected),
            at != default)
            ? null
            : Capability.Validation("attribute-sample");
}

// The attribute cohort every attribute limit and every attribute row reads: the four sums, the three rates they
// derive, and the two fixed-column facts. It is folded ONCE per study and threaded — a second fold over the same
// samples is one more full pass whose only product is the answer already in hand, under a nine-slot tuple no
// reader can name a column of.
public sealed record AttributeCohort(
    long Inspected,
    long Nonconforming,
    long Opportunities,
    long Defects,
    int Samples,
    bool FixedInspected,
    bool FixedOpportunities) {
    public double P => (double)Nonconforming / Inspected;
    public double U => (double)Defects / Opportunities;
    public double C => (double)Defects / Samples;
    public double MeanInspected => (double)Inspected / Samples;
    public double MeanOpportunities => (double)Opportunities / Samples;

    public static AttributeCohort Of(Seq<AttributeSample> samples) => new(
        samples.Fold(0L, static (sum, row) => sum + row.Inspected),
        samples.Fold(0L, static (sum, row) => sum + row.Nonconforming),
        samples.Fold(0L, static (sum, row) => sum + row.Opportunities),
        samples.Fold(0L, static (sum, row) => sum + row.Defects),
        samples.Count,
        samples.Map(static sample => sample.Inspected).Distinct().Count == 1,
        samples.Map(static sample => sample.Opportunities).Distinct().Count == 1);
}

// The interval bounds one cohort earns ONCE per study: the Clopper-Pearson proportion pair over the nonconforming
// count and the exact Poisson pair over the defect count, each read by the two charts that share its basis.
public readonly record struct AttributeBounds(double ProportionLower, double ProportionUpper,
    double CountLower, double CountUpper);

// One row per attribute chart, carrying WHEN it is derivable, how a sample plots on it, and how the cohort's own
// interval scales onto it. The limit fold and the capability fold were two hand-written four-arm literal blocks
// over the same four charts, each re-testing the cohort's fixed-column bools and each free to disagree with the
// other about a chart's centre; both are now one fold over this roster and a fifth attribute chart is one row.
[SmartEnum<string>]
public sealed partial class AttributeChart {
    public static readonly AttributeChart Proportion = new("p", SpcChart.P, static _ => true,
        static (sample, cohort) => (
            (double)sample.Nonconforming / sample.Inspected, cohort.P,
            Math.Sqrt(cohort.P * (1.0 - cohort.P) / sample.Inspected)),
        static (cohort, bounds, tail) => (cohort.P, bounds.ProportionLower, bounds.ProportionUpper, tail));
    // A count of nonconforming UNITS is comparable across samples only where every sample inspected the same
    // number, so the row states that demand rather than a consuming fold re-reading the cohort's own column.
    public static readonly AttributeChart Count = new("np", SpcChart.Np, static cohort => cohort.FixedInspected,
        static (sample, cohort) => (
            sample.Nonconforming, cohort.P * sample.Inspected,
            Math.Sqrt(sample.Inspected * cohort.P * (1.0 - cohort.P))),
        static (cohort, bounds, tail) => (
            cohort.P * cohort.MeanInspected,
            bounds.ProportionLower * cohort.MeanInspected,
            bounds.ProportionUpper * cohort.MeanInspected,
            tail * cohort.MeanInspected));
    public static readonly AttributeChart Defects = new("c", SpcChart.C, static cohort => cohort.FixedOpportunities,
        static (sample, cohort) => (sample.Defects, cohort.C, Math.Sqrt(cohort.C)),
        static (cohort, bounds, tail) => (
            cohort.U * cohort.MeanOpportunities,
            bounds.CountLower / cohort.Samples,
            bounds.CountUpper / cohort.Samples,
            tail * cohort.MeanOpportunities));
    public static readonly AttributeChart DefectRate = new("u", SpcChart.U, static _ => true,
        static (sample, cohort) => (
            (double)sample.Defects / sample.Opportunities, cohort.U,
            Math.Sqrt(cohort.U / sample.Opportunities)),
        static (cohort, bounds, tail) => (
            cohort.U, bounds.CountLower / cohort.Opportunities, bounds.CountUpper / cohort.Opportunities, tail));

    public SpcChart Chart { get; }

    [UseDelegateFromConstructor]
    public partial bool Derivable(AttributeCohort cohort);
    [UseDelegateFromConstructor]
    public partial (double Value, double Center, double Sigma) Plot(AttributeSample sample, AttributeCohort cohort);
    [UseDelegateFromConstructor]
    public partial (double Estimate, double Lower, double Upper, double Demanded) Band(
        AttributeCohort cohort, AttributeBounds bounds, double tail);

    public static Seq<AttributeChart> For(AttributeCohort cohort) => toSeq(Items).Filter(row => row.Derivable(cohort));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CapabilityStudy {
    private CapabilityStudy() { }

    public sealed record Variables(Seq<ResidualSample> Samples) : CapabilityStudy;
    public sealed record Attributes(Seq<AttributeSample> Samples) : CapabilityStudy;
}

// The STOCHASTIC half of one chain term — the part the analytic term cannot state. Sensitivity and spread are the
// term's own declarations, so re-carrying them here is what let a simulation run on a roster the chain never saw:
// the contributor names its term, adds the systematic offset and the shared-factor loadings, and optionally
// overrides the term's declared family with a MEASURED fit.
[ComplexValueObject]
public sealed partial class StackContributor {
    public string Term { get; }
    public double BiasMm { get; }
    public Arr<double> FactorLoadings { get; }
    public Option<DistributionParameters> Fitted { get; }

    public double IndependentLoading => Math.Sqrt(1.0 - FactorLoadings.Fold(0.0, static (sum, value) => sum + (value * value)));

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref string term,
        ref double biasMm,
        ref Arr<double> factorLoadings,
        ref Option<DistributionParameters> fitted) {
        term = term?.Trim() ?? string.Empty;
        // The loadings are direction cosines onto the shared factors, so their squared sum is the share of the
        // term's variance the common factors explain and cannot exceed the whole of it.
        validationError = ValidityClaim.All(
            Witness.Keyed(term),
            ValidityClaim.Finite(biasMm),
            fitted.ForAll(static row => row.IsValid && row.FiniteMoments),
            factorLoadings.ForAll(static value => ValidityClaim.Finite(value)),
            ValidityClaim.UnitInterval(factorLoadings.Fold(0.0, static (sum, value) => sum + (value * value))))
                ? null
                : Capability.Validation("stack-contributor");
    }
}

[ComplexValueObject]
public sealed partial class StackupPolicy {
    public ToleranceChain Chain { get; }
    public Seq<StackContributor> Contributors { get; }
    public int Trials { get; }
    public Ratio TailProbability { get; }
    public int RandomSeed { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref ToleranceChain chain,
        ref Seq<StackContributor> contributors,
        ref int trials,
        ref Ratio tailProbability,
        ref int randomSeed) =>
        // The contributor roster is a BIJECTION onto the chain's terms. An extra contributor models a term nobody
        // declared and a missing one drops a term the analytic bound still counts, so the two readings the receipt
        // publishes would describe different stacks under one seed.
        validationError = ValidityClaim.All(
            chain is not null,
            ValidityClaim.CountAtLeast(contributors.Count, 1),
            ValidityClaim.CountAtLeast(trials, 2),
            tailProbability.DecimalFractions is > 0.0 and < 0.5,
            // A tail the trial count cannot resolve to one draw is a percentile read off nothing.
            ValidityClaim.Ordered(1.0, trials * tailProbability.DecimalFractions),
            ValidityClaim.CountExactly(contributors.Map(static row => row.Term).Distinct().Count, contributors.Count),
            toSet(contributors.Map(static row => row.Term))
                == toSet(toSeq(chain.Terms).Map(static row => row.Key)),
            contributors.Map(static row => row.FactorLoadings.Count).Distinct().Count <= 1)
                ? null
                : Capability.Validation("stack-policy");

    // The chain term each contributor models, in the contributor's own order — resolved once at the fold rather
    // than searched per trial. The admitted bijection is what makes this positionally aligned with `Contributors`,
    // so the simulation indexes both by the same ordinal and never re-searches a key it already proved.
    public Seq<ToleranceTerm> Terms => Contributors.Choose(row =>
        toSeq(Chain.Terms).Find(term => string.Equals(term.Key, row.Term, StringComparison.Ordinal)));
}

[ComplexValueObject]
public sealed partial class CapabilityTolerance {
    public CapabilityIdentity Identity { get; }
    public ItGrade Grade { get; }
    public Option<Length> LowerSpec { get; }
    public Option<Length> UpperSpec { get; }
    public Option<Length> Target { get; }
    public Ratio TailProbability { get; }
    public Ratio Confidence { get; }

    // The index convention this study reports on: the half-span, in process sigmas, the moment method spreads to.
    public SigmaSpan SpreadSpan { get; }
    public DistributionPolicy Distribution { get; }
    public ControlPolicy Control { get; }
    public MeasurementEvidence Measurement { get; }
    public Option<ProcedureReceipt> Procedure { get; }
    public Option<StackupPolicy> Stackup { get; }
    public Instant At { get; }

    public Option<double> LowerSpecMm => LowerSpec.Map(static value => value.Millimeters);
    public Option<double> UpperSpecMm => UpperSpec.Map(static value => value.Millimeters);
    public Option<double> TargetMm => Target.Map(static value => value.Millimeters);
    public double TailProbabilityValue => TailProbability.DecimalFractions;
    public double ConfidenceValue => Confidence.DecimalFractions;
    public double DemandedCpk => MathNet.Numerics.Distributions.Normal.InvCDF(0.0, 1.0, 1.0 - TailProbabilityValue) / 3.0;

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref CapabilityIdentity identity,
        ref ItGrade grade,
        ref Option<Length> lowerSpec,
        ref Option<Length> upperSpec,
        ref Option<Length> target,
        ref Ratio tailProbability,
        ref Ratio confidence,
        ref SigmaSpan spreadSpan,
        ref DistributionPolicy distribution,
        ref ControlPolicy control,
        ref MeasurementEvidence measurement,
        ref Option<ProcedureReceipt> procedure,
        ref Option<StackupPolicy> stackup,
        ref Instant at) {
        bool finite = lowerSpec.ForAll(static value => double.IsFinite(value.Millimeters))
            && upperSpec.ForAll(static value => double.IsFinite(value.Millimeters))
            && target.ForAll(static value => double.IsFinite(value.Millimeters));
        bool ordered = lowerSpec.Bind(lower => upperSpec.Map(upper => lower < upper)).IfNone(true);
        bool centered = target.ForAll(value => lowerSpec.ForAll(lower => lower <= value) && upperSpec.ForAll(upper => value <= upper));
        validationError = ValidityClaim.All(
            identity is not null,
            ValidityClaim.CountAtLeast(grade.Number, 1),
            identity.Feature == grade.Diameter,
            lowerSpec.IsSome || upperSpec.IsSome,
            finite, ordered, centered,
            ValidityClaim.Positive(spreadSpan.ToValue()),
            tailProbability.DecimalFractions is > 0.0 and < 0.5,
            confidence.DecimalFractions is > 0.0 and < 1.0,
            distribution is not null, control is not null, measurement is not null,
            procedure.ForAll(static value => value is not null),
            stackup.ForAll(static value => value is not null),
            at != default)
                ? null
                : Capability.Validation("tolerance");
    }
}
```

## [05]-[ASSESSMENT]

- Owner: `Capability` owns admission, the two study folds, control-limit derivation, violation merging, the correlated stackup, and the ledger projections; `CapabilityReport` conserves every derived decision.
- Law: every gate refusal carries its OWN discriminant. The kernel `InvalidInput`/`InvalidResult` mints take no detail slot, so eight gates lowering onto them are eight refusals a caller cannot tell apart; each answers on the fabrication band under a declared locus, and `Inadmissible` is the one mint both a generated hook and a fold gate read.
- Law: `StackupVerdict` is a VERDICT, not a fault, and it is a CASE rather than a flag. A stack exceeding its bound is exactly the answer the study was run to obtain and its contribution ranking is the evidence naming the term worth tightening, so the assessment returns and the consuming gate decides what the exceedance means — with the margin or the overrun on the case, since a boolean made every consumer re-derive the number it acts on.
- Law: a settled study reports its control state as EVIDENCE, not as a word. Rule stability, serial independence, and stationarity are three independently bounded findings with three different remedies, so they ride a `CapabilitySet<ControlEvidence>` whose whole-set read keeps the `Controlled` projection every consumer already spelled, while the gate's refusal names the rows the process missed. The same posture binds the attestation pair: both demands compose the typed `Require` twin, where `Some` IS the refusal carrying its own missing set, so a bare-label refusal has no site here.
- Law: moments, order statistics, and the sample deviation are the KERNEL's — `Stat<Scalar>` over the value-trait carrier and `Distribution<Scalar>` for exact percentiles over a bounded materialized sample — and every normalizer is named at its call site. The simulation's tail reads through that owner rather than sorting its own trial buffer, which is what let a covariance fold correlate a SORTED response against factor draws still in trial order.
- Law: contribution shares are CORRELATED shares. The simulation loads every contributor on the same shared factors, so a term's share is its covariance with the assembled response over the response variance — an independence fraction under a correlated model hands a shared factor's spread to whichever term carries the largest loading.
- Law: one out-of-control EPISODE is one violation. A run longer than a rule's window breaches at every offset inside it, so overlapping and adjacent breach windows merge into the maximal span they cover and the excursion is the worst standardized point in that span.
- Law: `Achievable` returns the qualifying row's band beside the evidence that earned it — grade, index, and effective sample size — so a consumer grading confidence reads the support behind the projection rather than assigning a constant trust to the word history.
- Entry: `Capability.Assess`, `Capability.Gate`, and `Capability.Achievable` parameterize assessment, ledger selection, and tolerance projection without ambient state; `Assess` takes the trailing `FabricationTap?` the run spine hands it, so the fact fires where the receipt settles and every estimation fold stays tap-free.
- Auto: `Validation` accumulates independent request and gate faults under distinct errors; `Stat<Scalar>` owns variable moments and `Distribution<Scalar>` the simulation tail; `Distance.Pearson` derives the autocorrelation spectrum; `Fit.Line` derives drift; `SpecialFunctions.GammaLn` and `Gamma` own distribution functions; `Traverse`, `Choose`, and `Fold` own collection flow.
- Receipt: `CapabilityReport` carries moment and percentile indices or attribute rates, per-metric confidence intervals, pointwise control limits, merged rule windows, the fitted distribution, effective sample size, measurement and procedure evidence, the optional stackup assessment with both analytic evaluations and its covariance shares, the control-evidence set, and the admitted `CapabilityVerdict`. `FabricationFact.Capability.Of` projects the index rows and violation count onto `rasm.fabrication.capability.index` and `rasm.fabrication.capability.violations` through `Process/telemetry#FACT_PROJECTION` as kind `capability`.
- Packages: MathNet.Numerics owns fitted distributions, roots, regression, correlation, and batch sampling; `Rasm.Domain` owns `Stat<TCarrier>`, `Distribution<TCarrier>`, `MomentNormalizer`, `QuantileRule`, the `Tolerance` band carrier, and the `CapabilitySet`/`ICapability` axis; `Rasm.Element` owns the `AdmissionSlots` gate and accumulate fold; `System.Numerics.Tensors` owns numeric reductions; CommunityToolkit.HighPerformance owns pooled and partitioned trial execution; UnitsNet owns specification lengths, achievable tolerance, and probability ratios; `ToolEvidence` carries MTConnect operating state decoded at `Tooling/magazine`; Thinktecture and LanguageExt own generated values and the accumulated rail.
- Boundary: `CapabilityReport` never enters `FabricationResult`, and only `CapabilityVerdict` crosses the plan seam.

```csharp signature
// --- [RECEIPTS] -----------------------------------------------------------------------------------------------------------------------------------
public sealed record CapabilitySeries(
    Arr<double> ResidualMm,
    Seq<Arr<double>> Groups,
    Arr<double> Means,
    Arr<double> Ranges,
    Arr<double> Sigmas);

public sealed record CapabilityMoment(double Mean, double WithinSigma, double OverallSigma, double Minimum, double Maximum);

public sealed record CapabilitySpread(double Center, double Lower, double Upper);

// FitError is the reported goodness figure; Akaike is what SELECTS. A richer family always tracks a sample at
// least as closely as the one it nests, so selection charges for parameters and the reported supremum stays the
// evidence a reader compares families on.
public sealed record CapabilityDistribution(DistributionParameters Parameters, double FitError, double Akaike);

// Conformance is the row's own read of its two published columns, never a third column beside them: a stored
// verdict is a hand-kept mirror of a comparison the reader can run, and the attribute count chart proved the risk
// by publishing a pass derived from a normalization its own bounds did not print.
public sealed record CapabilityRow(CapabilityMetric Metric, double Value, double Demanded) {
    public bool Pass => Value >= Demanded;
}

public sealed record CapabilityInterval(CapabilityMetric Metric, double Lower, double Upper, double Confidence);

public sealed record AttributeCapabilityRow(SpcChart Chart, double Estimate, double Lower, double Upper, double Demanded) {
    public bool Pass => Upper <= Demanded;
}

public sealed record SpcLimitRow(SpcChart Chart, int Index, Instant At, double Value, double Center, double Sigma, double Lower, double Upper);

public sealed record SpcViolation(SpcChart Chart, SpcRule Rule, int Start, int End, double Excursion);

public sealed record DriftRow(double Intercept, double Slope);

public sealed record AutocorrelationRow(int Lag, double Correlation);

public sealed record CapabilityDependence(Seq<AutocorrelationRow> Lags, double EffectiveSampleSize);

// Keyed by the chain TERM, so the simulated share and the analytic share on `ChainEvidence.Contributions` rank the
// same names and a documentation-plane row reads either without a second key space.
public sealed record StackContribution(string Term, double Share, double SigmaMm, double TighteningFactor);

// The history projection a downstream demand consumes: the achievable band, the grade and index that qualified
// it, and the effective sample size the projection rests on.
public sealed record AchievableTolerance(Length Width, ItGrade Grade, double Cpk, double EffectiveSampleSize);

// The bound verdict as a CASE, carrying the number a consumer acts on. A boolean said only that the stack cleared
// and made a caller re-derive the margin or the overrun from columns it had to correlate itself; an exceeded stack
// names its overrun beside the term whose share dominates it, which is the evidence the fault band's own ruling
// says refusing this study would have destroyed.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record StackupVerdict {
    private StackupVerdict() { }

    public sealed record Within(double MarginMm) : StackupVerdict;
    public sealed record Exceeded(double OverrunMm, Option<StackContribution> Dominant) : StackupVerdict;

    public bool Pass => this is Within;

    // ONE construction law, so the simulation and any re-reading consumer cannot disagree on where the bound sits.
    public static StackupVerdict Of(double tailMm, double boundMm, Option<StackContribution> dominant) =>
        tailMm <= boundMm ? new Within(boundMm - tailMm) : new Exceeded(tailMm - boundMm, dominant);
}

// BOTH readings of one stack: `Analytic` is the chain's own closed-form combination under its declared method and
// `Arithmetic` the worst-case bound over the same terms, beside the correlated simulation's moments, tail, and
// covariance shares. A consumer comparing the statistical answer against the arithmetic one reads one settled
// study rather than re-evaluating a fold of its own. It carries no content key, band, or stamp of its own — the
// key and the stamp are the analytic evaluation's, which is why this is an assessment and not a receipt.
public sealed record StackupAssessment(
    Receipt<ChainEvidence> Analytic,
    Receipt<ChainEvidence> Arithmetic,
    double MeanMm,
    double SigmaMm,
    double TailMm,
    int RandomSeed,
    int FactorCount,
    Seq<StackContribution> Contributions,
    StackupVerdict Verdict) {
    public ContentKey Source => Analytic.Key;
    public double BoundMm => Analytic.Evidence.BoundMm;

    public Option<StackContribution> Dominant => Contributions.Fold(Option<StackContribution>.None,
        static (best, row) => best.Filter(held => held.Share >= row.Share).IfNone(row));
}

public sealed record CapabilityReport(
    CapabilityIdentity Identity,
    ItGrade Grade,
    Seq<CapabilityRow> Rows,
    Seq<CapabilityInterval> Intervals,
    Seq<AttributeCapabilityRow> Attributes,
    Seq<SpcLimitRow> Limits,
    Seq<SpcViolation> Violations,
    Option<CapabilityDistribution> Distribution,
    CapabilityDependence Dependence,
    DriftRow Drift,
    MeasurementEvidence Measurement,
    Option<ProcedureReceipt> Procedure,
    Option<StackupAssessment> Stackup,
    CapabilitySet<ControlEvidence> Control,
    CapabilityVerdict Verdict,
    Instant At) {
    // The whole-set read every prior consumer of the boolean column already spelled, kept so a preimage frame and
    // a ledger projection compile unchanged while the refusal below gains the rows the boolean could not name.
    public bool Controlled => Control.AdmitsAll(CapabilitySet<ControlEvidence>.All);
}

// --- [OPERATIONS] ---------------------------------------------------------------------------------------------------------------------------------
public static class Capability {
    // Stackup lanes: the kernel draw owner keys a stream by (seed, lanes), so each contributor and each shared
    // factor draws from its own stream instead of taking a slice of one. Two disjoint lane heads keep the two
    // populations apart, and the ordinal beneath each is the row's own index — so a contributor added, removed,
    // or reordered leaves every other row's draw byte-identical and the seed the receipt publishes replays a
    // partial re-run, not only a re-run of the same roster in the same order.
    internal const long ContributorLane = 0L;
    internal const long SharedFactorLane = 1L;

    internal static readonly Op CapabilityOp = Op.Of(name: "fabrication:capability");

    // Every gate refusal names ITS OWN condition. The kernel `InvalidInput`/`InvalidResult` mints carry no detail
    // slot, so eight gates lowering onto them are eight refusals a caller cannot tell apart; each row here answers
    // on the fabrication band under a declared locus, which is the same `detail:` discipline the record-refusal
    // rail already spells and is what makes a failed study actionable rather than merely failed.
    // One refusal mint for the page; generated admission uses the default validation bridge separately.
    internal static ValidationError Validation(string locus) => new($"capability:{locus}");

    internal static FabricationFault Inadmissible(string locus) =>
        FabricationFault.Inadmissible(FabConcern.Spec, $"capability:{locus}");

    internal static Error Refusal(string locus) => Inadmissible(locus);

    private static readonly Error MissingHistory = Refusal("missing-history");
    private static readonly Error StudyMismatch = Refusal("study-identity-mismatch");
    private static readonly Error UnderpoweredStudy = Refusal("underpowered-study");
    private static readonly Error StackupUnsupported = Refusal("stackup-unsupported");
    private static readonly Error ProcedureNotYetIssued = Refusal("procedure-not-yet-issued");

    // The fact fires where the receipt SETTLES, which is here: the tap is a trailing parameter defaulting to the
    // silent port, so a headless assessment emits into unit with no branch and a composed runtime projects the
    // index rows and violation count without a metering call inside the estimation folds.
    public static Fin<CapabilityReport> Assess(
        CapabilityStudy study,
        CapabilityTolerance tolerance,
        FabricationTap? tap = null) =>
        from _ in Admit(study, tolerance)
        from report in study.Switch(
            state: tolerance,
            variables: static (demand, evidence) => Variables(evidence.Samples, demand),
            attributes: static (demand, evidence) => Attributes(evidence.Samples, demand))
        let _fact = (tap ?? FabricationTap.Silent).Fire(FabricationFact.Capability.Of(report))
        select report;

    // Every rejection reason accumulates, so a caller learns control, procedure, and measurement state in one verdict.
    public static Fin<CapabilityVerdict> Gate(CapabilityIdentity identity, ItGrade grade, Instant at, Seq<CapabilityHistory> history) =>
        CapabilityHistory.Of(identity, grade, at, history)
            .ToFin(MissingHistory)
            .Bind(row => CapabilityVerdict
                .Admit(row.Cpk, row.DemandedCpk, row.Attested)
                // The shortfall fault names a SHORTFALL, so it reads the index half alone. `CapabilityVerdict.Pass`
                // is the TOTAL demand — index and attestation together — and gating on it here made an attested-short
                // process and an unattested-capable process mint the same `CapabilityShortfall(cpk, demanded)`, a
                // fault whose own two numbers refute it. The attestation and control halves already refuse through
                // `Demanded`, each carrying the rows it missed, so this gate keeps the one question it can answer.
                .Bind(verdict => (Demanded(row),
                        Check(row.Cpk >= row.DemandedCpk,
                            new FabricationFault.CapabilityShortfall(identity.Process, row.Cpk, row.DemandedCpk)))
                    .Apply((_, _) => verdict)
                    .As()
                    .ToFin()));

    // Both capability demands ride the TYPED `Require` twin, where `Some` IS the refusal and the fault it mints
    // already carries the rows the holder missed. Three bare-label gates stood here — one per capability plus a
    // whole-set control bool — and none could tell a caller WHICH evidence was absent; the refusals now name their
    // own missing rows, and an empty run of them accumulates to the pass without a fault minted on the way.
    private static K<Validation<Error>, Unit> Demanded(CapabilityHistory row) =>
        AdmissionSlots.Accumulate(Seq(
                row.Control.AdmitsAll(CapabilitySet<ControlEvidence>.All)
                    ? Option<Error>.None
                    : Some<Error>(Inadmissible($"uncontrolled-process:{row.Control.Missing(CapabilitySet<ControlEvidence>.All).Wire}")),
                row.Attested.AdmitsAll(CapabilitySet<CapabilityAttestation>.All)
                    ? Option<Error>.None
                    : Some<Error>(Inadmissible($"unattested-process:{row.Attested.Missing(CapabilitySet<CapabilityAttestation>.All).Wire}")))
            .Somes()
            .Map(static refusal => AdmissionSlots.Gate(holds: false, refusal)));

    // CapabilityHistory selection returns the qualifying row's measured band BESIDE the evidence behind it, so a
    // consumer grading its own confidence reads the effective sample size that earned the projection rather than
    // assigning a constant trust to the word history.
    public static Option<AchievableTolerance> Achievable(
        CapabilityIdentity identity, Instant at, Seq<CapabilityHistory> history) =>
        history.Filter(row => row.Identity == identity && row.Covers(at) && row.Qualifies)
            .Fold(Option<CapabilityHistory>.None, static (best, row) =>
                best.Filter(held => held.Grade.Number <= row.Grade.Number).IfNone(row))
            .Map(static row => new AchievableTolerance(
                Length.FromMillimeters(row.Grade.ToleranceMillimeters),
                row.Grade,
                row.Cpk,
                row.EffectiveSampleSize));

    private static Fin<CapabilityReport> Variables(Seq<ResidualSample> samples, CapabilityTolerance tolerance) =>
        from series in Series(samples, tolerance.Control.SubgroupSize)
        from moment in Moment(series, tolerance)
        let fitted = FitDistribution(series.ResidualMm, moment, tolerance.Distribution)
        from stackup in tolerance.Stackup.Traverse(policy => Stackup(policy, tolerance.At)).As()
        let rows = Rows(moment, Some(fitted), tolerance)
        let dependence = Dependence(series.ResidualMm, tolerance.Control.MaximumLag)
        let limits = VariableLimits(series, moment, tolerance)
        let violations = Violations(limits)
        let drift = Drift(series.ResidualMm)
        // Three INDEPENDENT bounds, three rows: a run that signalled and a run that drifted are different findings
        // with different remedies, and one conjunction reported them under one word.
        let control = Held(
            (ControlEvidence.Stable, violations.IsEmpty),
            (ControlEvidence.Independent, dependence.Lags.ForAll(row =>
                Math.Abs(row.Correlation) <= tolerance.Control.MaximumAutocorrelation.DecimalFractions)),
            (ControlEvidence.Stationary, Math.Abs(drift.Slope) <= tolerance.Control.MaximumDrift.Millimeters))
        let procedureQualified = ProcedureQualified(tolerance.Identity.Process, tolerance.Procedure)
        let cpk = rows.Find(static row => row.Metric == CapabilityMetric.Cpk).Map(static row => row.Value).IfNone(0.0)
        from verdict in CapabilityVerdict.Admit(
            cpk,
            tolerance.DemandedCpk,
            Attested(procedureQualified, tolerance.Measurement.Suitable))
        select new CapabilityReport(
            tolerance.Identity,
            tolerance.Grade,
            rows,
            Intervals(rows, dependence, tolerance.ConfidenceValue),
            Seq<AttributeCapabilityRow>(),
            limits,
            violations,
            Some(fitted),
            dependence,
            drift,
            tolerance.Measurement,
            tolerance.Procedure,
            stackup,
            control,
            verdict,
            tolerance.At);

    private static Fin<CapabilityReport> Attributes(Seq<AttributeSample> samples, CapabilityTolerance tolerance) =>
        from _ in guard(!samples.IsEmpty, Refusal("empty-attribute-study")).ToFin()
        let cohort = AttributeCohort.Of(samples)
        let limits = AttributeLimits(samples, cohort, tolerance)
        let violations = Violations(limits)
        from rows in AttributeRows(cohort, tolerance)
        let equivalentCpk = rows.Filter(static row => row.Chart == SpcChart.P || row.Chart == SpcChart.U)
            .Min(static row => MathNet.Numerics.Distributions.Normal.InvCDF(
            0.0,
            1.0,
            double.Clamp(1.0 - row.Upper, double.Epsilon, 1.0 - double.Epsilon)) / 3.0)
        let procedureQualified = ProcedureQualified(tolerance.Identity.Process, tolerance.Procedure)
        from verdict in CapabilityVerdict.Admit(
            double.Max(0.0, equivalentCpk),
            tolerance.DemandedCpk,
            Attested(procedureQualified, tolerance.Measurement.Suitable))
        select new CapabilityReport(
            tolerance.Identity,
            tolerance.Grade,
            Seq<CapabilityRow>(),
            Seq<CapabilityInterval>(),
            rows,
            limits,
            violations,
            None,
            new CapabilityDependence(Seq<AutocorrelationRow>(), cohort.Samples),
            new DriftRow(0.0, 0.0),
            tolerance.Measurement,
            tolerance.Procedure,
            None,
            // An attribute study measures no serial dependence and fits no drift line, so the two rows it cannot
            // observe are HELD rather than absent: absence here would report an attribute cohort as uncontrolled
            // for want of evidence its own modality never produces.
            Held(
                (ControlEvidence.Stable, violations.IsEmpty),
                (ControlEvidence.Independent, true),
                (ControlEvidence.Stationary, true)),
            verdict,
            tolerance.At);

    private static Fin<CapabilitySeries> Series(Seq<ResidualSample> samples, int subgroupSize) =>
        from _1 in guard(!samples.IsEmpty && subgroupSize >= 1 && subgroupSize <= samples.Count, Refusal("subgroup-size")).ToFin()
        from _2 in guard(subgroupSize == 1 ? samples.Count >= 2 : samples.Count % subgroupSize == 0, Refusal("subgroup-partition")).ToFin()
        let residual = samples.Map(static sample => sample.Distance).ToArr()
        let walk = toSeq(residual)
        let groups = subgroupSize == 1
            ? residual.Map(static value => Arr.create(value)).ToSeq()
            : toSeq(Enumerable.Range(0, residual.Count / subgroupSize)).Map(index => residual.Skip(index * subgroupSize).Take(subgroupSize).ToArr())
        select new CapabilitySeries(
            residual,
            groups,
            groups.Map(Mean).ToArr(),
            subgroupSize == 1
                ? walk.Skip(1).Zip(walk, static (next, prior) => Math.Abs(next - prior)).ToArr()
                : groups.Map(static group => group.Max(double.NegativeInfinity) - group.Min(double.PositiveInfinity)).ToArr(),
            groups.Map(SampleSigma).ToArr());

    // The kernel moment owner over the kernel scalar carrier. `Stat<Scalar>` is the spelling a bare-measure reader
    // takes — `double` cannot stand on the value-trait axis the fold constrains on — and its band context carries
    // an ADMITTED `Tolerance`, so the spec half-band crosses the seam through the kernel carrier on its own lane
    // rather than as a bare magnitude the context had no band to gate. A mean displaced past its nearer limit
    // yields no half-band at all and the receipt stays context-free: an out-of-spec process is what a capability
    // study exists to report, so it must not refuse, and a negative magnitude stamped as a band is what did.
    private static Fin<CapabilityMoment> Moment(CapabilitySeries series, CapabilityTolerance tolerance) =>
        from stat in Stat<Scalar>.Of(
            series.ResidualMm.ToSeq().Map(static value => (Scalar)value), CapabilityOp)
        let banded = Tolerance.Of(ToleranceLane.Deviation, SpecHalfBand(tolerance, stat.Mean), CapabilityOp)
            .Match(Succ: static band => StatContext.Band(band), Fail: static _ => StatContext.None)
        from accepted in CapabilityOp.AcceptValue(value: stat with { Context = banded })
        select new CapabilityMoment(
            accepted.Mean,
            // The within-subgroup component is the range or sigma bar over its own calibrated constant; the
            // overall component is the receipt's own unbiased deviation, stated at its normalizer because a
            // capability index that inherited the biased estimate had no site at which a reader would notice.
            tolerance.Control.SubgroupSize == 1
                ? Mean(series.Ranges) / ControlConstant.Get(2).RangeMean
                : Mean(series.Sigmas) / C4(tolerance.Control.SubgroupSize),
            accepted.Deviation(MomentNormalizer.Sample),
            accepted.Minimum.To(),
            accepted.Maximum.To());

    // Selection is PENALIZED, never raw goodness-of-fit: a Student-t always tracks a sample at least as closely as
    // the normal it nests, so a bare supremum elects the richer family every time and its heavier tail then biases
    // every percentile index the report publishes. Akaike's criterion over the fitted log-likelihood charges each
    // family for the parameters it spends, so the normal wins unless the sample genuinely pays for the extra one.
    private static CapabilityDistribution FitDistribution(Arr<double> values, CapabilityMoment moment, DistributionPolicy policy) =>
        toSeq(DistributionFamily.Items)
            .Bind(family => family.Candidates(moment, double.Max(moment.OverallSigma, double.Epsilon), policy))
            .Map(parameters => Assessed(parameters, values, policy))
            .Fold(Option<CapabilityDistribution>.None, static (best, candidate) =>
                best.Filter(held => held.Akaike <= candidate.Akaike).IfNone(candidate))
            .IfNone(new CapabilityDistribution(
                new DistributionParameters.Normal(moment.Mean, double.Max(moment.OverallSigma, double.Epsilon)),
                double.PositiveInfinity,
                double.PositiveInfinity));

    // Both figures ride ONE fitted instance, so the reported supremum and the selecting criterion can never
    // describe different parameterizations of the same family.
    private static CapabilityDistribution Assessed(
        DistributionParameters parameters, Arr<double> values, DistributionPolicy policy) {
        IContinuousDistribution fitted = parameters.Create(Deterministic.Source(seed: policy.FitSeed));
        return new CapabilityDistribution(parameters, Supremum(fitted, toSeq(values.Order())), Akaike(fitted, parameters, values));
    }

    // AIC = 2k − 2·ln L over the fitted density. A zero or non-finite density at any observation makes the sample
    // impossible under that family, so the criterion is infinite and the family loses outright.
    private static double Akaike(IContinuousDistribution fitted, DistributionParameters parameters, Arr<double> values) {
        double logLikelihood = toSeq(values).Fold(0.0, (sum, value) => sum + Math.Log(fitted.Density(value)));
        return double.IsFinite(logLikelihood)
            ? (2.0 * parameters.FreeParameters) - (2.0 * logLikelihood)
            : double.PositiveInfinity;
    }

    // Kolmogorov-Smirnov supremum against the mid-rank plotting position; the seeded generator never enters a CDF read.
    // MathNet's constructor demands a System.Random, so the kernel draw owner ADAPTS into it rather than forking a
    // second stream — one seed space across every fit, sample, and spread this page draws.
    private static double Supremum(IContinuousDistribution fitted, Seq<double> ordered) =>
        ordered.Map((value, index) => Math.Abs(fitted.CumulativeDistribution(value) - ((index + 0.5) / ordered.Count))).Max(0.0);

    // No MathNet interface exposes a generic inverse CDF, so one bracketed root-find serves every admitted family.
    // The bracket is the distribution's own support clipped to the policy's sigma reach, admitted through the same
    // owner the shape search runs over — a support that cannot bracket refuses at the interval rather than at Brent.
    internal static Option<double> Quantile(
        IContinuousDistribution distribution, double probability, DistributionPolicy policy) =>
        double.Max(distribution.StdDev, double.Epsilon) is var spread
            && SearchBracket.Validate(
                double.Max(distribution.Minimum, distribution.Mean - policy.Bracket.Of(spread)),
                double.Min(distribution.Maximum, distribution.Mean + policy.Bracket.Of(spread)),
                out SearchBracket? support) is null
                ? policy.Root(value => distribution.CumulativeDistribution(value) - probability, support!)
                : None;

    internal static Option<CapabilitySpread> QuantileSpread(DistributionParameters parameters, CapabilityTolerance tolerance) =>
        parameters.IsValid && parameters.FiniteMoments
            ? Spread(
                parameters.Create(Deterministic.Source(seed: tolerance.Distribution.FitSeed)),
                tolerance.TailProbabilityValue,
                tolerance.Distribution)
            : None;

    private static Option<CapabilitySpread> Spread(IContinuousDistribution fitted, double tail, DistributionPolicy policy) =>
        from median in Quantile(fitted, 0.5, policy)
        from low in Quantile(fitted, tail, policy)
        from high in Quantile(fitted, 1.0 - tail, policy)
        where median - low > 0.0 && high - median > 0.0
        select new CapabilitySpread(median, median - low, high - median);

    private static Seq<CapabilityRow> Rows(CapabilityMoment moment, Option<CapabilityDistribution> fitted, CapabilityTolerance tolerance) =>
        toSeq(CapabilityMetric.Items).Choose(metric => metric.Of(moment, fitted, tolerance)
            .Map(value => new CapabilityRow(metric, value, tolerance.DemandedCpk)));

    private static Seq<CapabilityInterval> Intervals(Seq<CapabilityRow> rows, CapabilityDependence dependence, double confidence) =>
        from row in rows
        let half = MathNet.Numerics.Distributions.Normal.InvCDF(0.0, 1.0, (1.0 + confidence) / 2.0)
            * row.Metric.StandardError(row.Value, dependence.EffectiveSampleSize)
        select new CapabilityInterval(row.Metric, row.Value - half, row.Value + half, confidence);

    private static CapabilityDependence Dependence(Arr<double> values, int maximumLag) {
        int upper = int.Min(maximumLag, values.Count / 4);
        Seq<AutocorrelationRow> lags = toSeq(Enumerable.Range(1, upper)).Map(lag =>
            new AutocorrelationRow(lag, 1.0 - Distance.Pearson(values.SkipLast(lag), values.Skip(lag))));
        double penalty = lags.Fold(1.0, static (sum, row) => sum + (2.0 * row.Correlation));
        return new CapabilityDependence(lags, double.Clamp(values.Count / double.Max(penalty, 1.0), 2.0, values.Count));
    }

    private static Seq<SpcLimitRow> VariableLimits(CapabilitySeries series, CapabilityMoment moment, CapabilityTolerance tolerance) {
        int subgroupSize = tolerance.Control.SubgroupSize;
        double width = tolerance.Control.Band.ToValue();
        double weight = tolerance.Control.EwmaWeight.DecimalFractions;
        double slack = tolerance.Control.CusumSlack.ToValue();
        double meanSigma = moment.WithinSigma / Math.Sqrt(subgroupSize);
        ControlConstant rangeConstant = ControlConstant.Nearest(subgroupSize);
        double rangeCenter = Mean(series.Ranges);
        double rangeSigma = SampleSigma(series.Ranges);
        double rangeLower = double.Max(0.0,
            rangeCenter * (rangeConstant.RangeMean - (width * rangeConstant.RangeSigma)) / rangeConstant.RangeMean);
        double rangeUpper = rangeCenter * (rangeConstant.RangeMean + (width * rangeConstant.RangeSigma)) / rangeConstant.RangeMean;
        Seq<SpcLimitRow> spread = subgroupSize <= ControlConstant.LargestSubgroup
            ? toSeq(series.Ranges).Map((value, index) => new SpcLimitRow(
                subgroupSize == 1 ? SpcChart.MovingRange : SpcChart.Range,
                index,
                tolerance.At,
                value,
                rangeCenter,
                rangeSigma,
                rangeLower,
                rangeUpper)).ToSeq()
            : SigmaLimits(series.Sigmas, C4(subgroupSize), width, tolerance.At);
        Seq<SpcLimitRow> primary = subgroupSize == 1
            ? Points(SpcChart.Individuals, series.ResidualMm, moment.Mean, moment.WithinSigma, tolerance.At, width) + spread
            : Points(SpcChart.XBar, series.Means, moment.Mean, meanSigma, tolerance.At, width) + spread;
        (double _, Arr<double> ewma) = series.ResidualMm.Fold(
            (moment.Mean, Arr<double>.Empty),
            (state, value) => {
                double next = (weight * value) + ((1.0 - weight) * state.Item1);
                return (next, state.Item2.Add(next));
            });
        (double positive, double negative, Arr<double> cusum) = series.ResidualMm.Fold(
            (0.0, 0.0, Arr<double>.Empty),
            (state, value) => {
                double standardized = (value - moment.Mean) / double.Max(moment.WithinSigma, double.Epsilon);
                double nextPositive = double.Max(0.0, state.Item1 + standardized - slack);
                double nextNegative = double.Min(0.0, state.Item2 + standardized + slack);
                double signed = nextPositive >= -nextNegative ? nextPositive : nextNegative;
                return (nextPositive, nextNegative, state.Item3.Add(signed));
            });
        return primary
            + toSeq(ewma).Map((value, index) => Point(
                SpcChart.Ewma,
                index,
                tolerance.At,
                value,
                moment.Mean,
                moment.WithinSigma * Math.Sqrt(
                    weight / (2.0 - weight)
                    * (1.0 - Math.Pow(1.0 - weight, 2.0 * (index + 1)))),
                width)).ToSeq()
            + Points(SpcChart.Cusum, cusum, 0.0, tolerance.Control.CusumDecision.ToValue(), tolerance.At, width: 1.0);
    }

    // Every sample plots on every chart the cohort supports, so the fan is the roster crossed with the samples and
    // no chart's centre or spread is spelled at the fold that reads it.
    private static Seq<SpcLimitRow> AttributeLimits(
        Seq<AttributeSample> samples, AttributeCohort cohort, CapabilityTolerance tolerance) =>
        from row in AttributeChart.For(cohort)
        from indexed in samples.Map(static (sample, index) => (Sample: sample, Index: index))
        let plot = row.Plot(indexed.Sample, cohort)
        select Point(row.Chart, indexed.Index, indexed.Sample.At, plot.Value, plot.Center, plot.Sigma,
            tolerance.Control.Band.ToValue());

    // The interval is the COHORT's, earned once: a Clopper-Pearson pair over the nonconforming count and an exact
    // Poisson pair over the defect count, each scaled onto the charts that share its basis by the chart's own row.
    // Conformance is the row's own read of its published bound against its published demand, so the verdict cannot
    // compare a figure the row never printed — which is what a stored pass column let the count chart do.
    private static Fin<Seq<AttributeCapabilityRow>> AttributeRows(AttributeCohort cohort, CapabilityTolerance tolerance) {
        double alpha = 1.0 - tolerance.ConfidenceValue;
        double shape = cohort.Nonconforming + 0.5;
        double complement = cohort.Inspected - cohort.Nonconforming + 0.5;
        return from proportionLower in BetaQuantile(shape, complement, alpha / 2.0, tolerance.Distribution)
               from proportionUpper in BetaQuantile(shape, complement, 1.0 - (alpha / 2.0), tolerance.Distribution)
               from countLower in Finite(cohort.Defects == 0 ? 0.0
                   : MathNet.Numerics.Distributions.Gamma.InvCDF(cohort.Defects, 1.0, alpha / 2.0))
               from countUpper in Finite(MathNet.Numerics.Distributions.Gamma.InvCDF(
                   cohort.Defects + 1.0, 1.0, 1.0 - (alpha / 2.0)))
               let bounds = new AttributeBounds(proportionLower, proportionUpper, countLower, countUpper)
               select AttributeChart.For(cohort).Map(row =>
                   row.Band(cohort, bounds, tolerance.TailProbabilityValue) switch {
                       var band => new AttributeCapabilityRow(row.Chart, band.Estimate, band.Lower, band.Upper, band.Demanded),
                   });
    }

    // One EXCURSION, one violation. A run longer than a rule's window breaches at every offset inside it, so
    // emitting a row per window inflates a single out-of-control episode into `run - window + 1` rows and every
    // count a report publishes off them. Overlapping and adjacent breach windows merge into the maximal span
    // they cover, and the excursion is the worst standardized point inside that span.
    private static Seq<SpcViolation> Violations(Seq<SpcLimitRow> limits) =>
        from group in toSeq(limits.GroupBy(static row => row.Chart))
        let points = toSeq(group.OrderBy(static row => row.Index))
        let banded = points.Map(Banded).ToArr()
        let zoned = points.Map(Zoned).ToArr()
        from rule in toSeq(SpcRule.Items).Filter(group.Key.Admits)
        let series = rule.Class == SpcRuleClass.Limit ? banded : zoned
        from span in Merged(
            toSeq(Enumerable.Range(0, int.Max(0, series.Count - rule.Window + 1)))
                .Filter(start => rule.Breach(series.Skip(start).Take(rule.Window).ToArr()))
                .Map(start => (Start: start, End: start + rule.Window - 1)))
        select new SpcViolation(
            group.Key,
            rule,
            span.Start,
            span.End,
            series.Skip(span.Start).Take(span.End - span.Start + 1).Map(Math.Abs).Max(0.0));

    private static Seq<(int Start, int End)> Merged(Seq<(int Start, int End)> windows) =>
        windows.Fold(Seq<(int Start, int End)>(), static (held, window) =>
            held.Last.Filter(prior => window.Start <= prior.End + 1).Match(
                Some: prior => held.Init.Add((prior.Start, int.Max(prior.End, window.End))),
                None: () => held.Add(window)));

    // Band normalization crosses +/-1 exactly at the row's own limit, so a configured sigma width and a clamped attribute floor both hold.
    private static double Banded(SpcLimitRow row) =>
        row.Value >= row.Center
            ? (row.Value - row.Center) / double.Max(row.Upper - row.Center, double.Epsilon)
            : -((row.Center - row.Value) / double.Max(row.Center - row.Lower, double.Epsilon));

    private static double Zoned(SpcLimitRow row) => (row.Value - row.Center) / double.Max(row.Sigma, double.Epsilon);

    private static Seq<SpcLimitRow> SigmaLimits(Arr<double> sigmas, double c4, double width, Instant at) =>
        SigmaBand(sigmas, Mean(sigmas),
            width * Math.Sqrt(1.0 - (c4 * c4)) / c4, at);

    private static Seq<SpcLimitRow> SigmaBand(Arr<double> sigmas, double center, double band, Instant at) =>
        toSeq(sigmas).Map((value, index) => new SpcLimitRow(
            SpcChart.Sigma,
            index,
            at,
            value,
            center,
            SampleSigma(sigmas),
            double.Max(0.0, center * (1.0 - band)),
            center * (1.0 + band))).ToSeq();

    private static Fin<StackupAssessment> Stackup(StackupPolicy policy, Instant stamped) {
        // `StackupAssessment.RandomSeed` is published as replay evidence, and only the branch's one splitmix64 owner
        // makes that evidence hold across runtimes — a `System.Random` mint forks the draw the receipt claims, and
        // MathNet reaches the bulk-fill virtuals the owner overrides whole rather than the base compat stream. The
        // seed keys one LANE per row rather than one shared stream, so the evidence is per-row reproducible.
        int factors = policy.Contributors.Head.Map(static row => row.FactorLoadings.Count).IfNone(0);
        // The spread each row spends is its TERM's, and the family is the term's declared process distribution
        // unless the contributor carries a measured fit that overrides it — so a stack cannot be simulated at a
        // sigma the analytic bound never saw.
        Arr<double> spread = policy.Terms.Map(static term => term.StatisticalHalfRangeMm).ToArr();
        Arr<IContinuousDistribution> distributions = policy.Contributors.Map((row, index) =>
            row.Fitted.Match(
                Some: fitted => fitted.Create(Deterministic.Source(policy.RandomSeed, ContributorLane, index)),
                None: () => policy.Terms[index].Distribution.Seeded(
                    Deterministic.Source(policy.RandomSeed, ContributorLane, index)))).ToArr();
        double[][] independent = policy.Contributors.Map((_, index) => {
            double[] samples = new double[policy.Trials];
            distributions[index].Samples(samples);
            TensorPrimitives.Subtract(samples, distributions[index].Mean, samples);
            TensorPrimitives.Divide(samples, double.Max(distributions[index].StdDev, double.Epsilon), samples);
            return samples;
        }).ToArray();
        double[][] shared = toSeq(Enumerable.Range(0, factors)).Map(factor => {
            double[] samples = new double[policy.Trials];
            new MathNet.Numerics.Distributions.Normal(0.0, 1.0,
                Deterministic.Source(policy.RandomSeed, SharedFactorLane, factor)).Samples(samples);
            return samples;
        }).ToArray();
        using MemoryOwner<double> owner = MemoryOwner<double>.Allocate(policy.Trials);
        ArraySegment<double> destination = owner.DangerousGetArray();
        StackupAction action = new(policy, spread, independent, shared, destination.Array!, destination.Offset);
        ParallelHelper.For<StackupAction>(0, policy.Trials, in action);
        Span<double> trials = owner.Span[..policy.Trials];
        double probability = policy.TailProbability.DecimalFractions;
        // The kernel exact-order-statistic owner answers the moments and BOTH tail percentiles from one admitted
        // sample under a DECLARED quantile convention. The hand fold that stood here averaged, took a population
        // deviation, sorted the buffer IN PLACE, and then indexed one percentile by floor and the other by ceiling
        // — two conventions on one reading — and the covariance fold that followed correlated the response in its
        // SORTED order against factor draws still in trial order, so every published share was a correlation
        // between two unrelated permutations. Reading through the owner keeps the response buffer intact.
        return from spreadStat in Distribution<Scalar>.Of(
                   toSeq(trials.ToArray().Map(static value => (Scalar)value)),
                   Seq(probability, 1.0 - probability), CapabilityOp, Some(QuantileRule.Interpolated))
               let mean = spreadStat.Summary.Mean
               let tail = spreadStat.Percentiles.Fold(0.0,
                   static (worst, row) => double.Max(worst, Math.Abs(row.Value.To())))
               // The simulation is CORRELATED — every contributor loads the same shared factors — so the share a
               // term owns is its covariance with the assembled response, not its independent variance fraction.
               // An independence share under a correlated model attributes a shared factor's spread to whichever
               // term happens to carry the largest loading and understates every term that moves with it.
               let covariance = Covariances(policy, spread, independent, shared, trials, mean)
               let variance = double.Max(TensorPrimitives.Sum<double>(covariance), double.Epsilon)
               let contributions = Contributions(policy, spread, covariance, variance, tail)
               // The analytic readings are the CHAIN's own, evaluated over the same terms and stamped where this
               // study settles: the declared method beside the arithmetic bound. A worst-case fold re-spelled here
               // would be a second algebra over one term roster.
               let analytic = policy.Chain.Evaluate(stamped)
               // The bound answer is a VERDICT, not a fault. A stack that exceeds its bound is exactly what the
               // study was run to obtain, and its contribution ranking is the evidence naming the term worth
               // tightening — refusing here destroys that evidence and forces every consumer to re-simulate to see
               // it. The consuming gate decides what an exceeded bound means for ITS decision.
               select new StackupAssessment(
                   analytic,
                   policy.Chain.Evaluate(StackMethod.WorstCase, stamped),
                   mean,
                   spreadStat.Summary.Deviation(MomentNormalizer.Sample),
                   tail,
                   policy.RandomSeed,
                   factors,
                   contributions,
                   StackupVerdict.Of(tail, analytic.Evidence.BoundMm,
                       contributions.Fold(Option<StackContribution>.None,
                           static (best, row) => best.Filter(held => held.Share >= row.Share).IfNone(row))));
    }

    // Each term's share is its own COVARIANCE with the assembled response over the response variance, so a set of
    // terms loading one shared factor divides that factor's spread among them rather than assigning it to one.
    private static double[] Covariances(
        StackupPolicy policy,
        Arr<double> spread,
        double[][] independent,
        double[][] shared,
        Span<double> trials,
        double mean) =>
        policy.Contributors.Map((row, index) => {
            double loading = row.FactorLoadings.Fold(0.0, static (sum, value) => sum + (value * value));
            double own = Math.Sqrt(double.Max(1.0 - loading, 0.0));
            double scale = spread[index];
            double sum = 0.0;
            for (int trial = 0; trial < policy.Trials; trial++) {
                double term = own * independent[index][trial];
                for (int factor = 0; factor < shared.Length; factor++)
                    term += row.FactorLoadings[factor] * shared[factor][trial];
                sum += scale * term * (trials[trial] - mean);
            }
            return sum / policy.Trials;
        }).ToArray();

    // Covariance share plus the scale factor that brings the simulated tail inside the bound names the term worth tightening.
    private static Seq<StackContribution> Contributions(
        StackupPolicy policy, Arr<double> spread, double[] covariance, double variance, double tail) =>
        policy.Contributors.Map((row, index) =>
            new StackContribution(
                row.Term,
                covariance[index] / variance,
                Math.Abs(spread[index]),
                tail <= policy.Chain.BoundMm ? 1.0 : policy.Chain.BoundMm / double.Max(tail, double.Epsilon)));

    private readonly struct StackupAction(
        StackupPolicy policy,
        Arr<double> spread,
        double[][] independent,
        double[][] shared,
        double[] destination,
        int offset) : IAction {
        public void Invoke(int index) =>
            destination[offset + index] = policy.Contributors.Map((row, contributor) => {
                double common = toSeq(row.FactorLoadings).Map((loading, factor) => loading * shared[factor][index])
                    .Fold(0.0, static (sum, value) => sum + value);
                double standardized = common + (row.IndependentLoading * independent[contributor][index]);
                return row.BiasMm + (spread[contributor] * standardized);
            }).Fold(0.0, static (sum, value) => sum + value);
    }

    // ONE boundary where measured booleans become a capability set, generic in the vocabulary so the attestation
    // pair and the control triad share it and no owner above this line sees a loose boolean again. A row enters on
    // its own evidence, so a third attestation or a fourth control fact is one roster row plus one reading.
    private static CapabilitySet<TCapability> Held<TCapability>(
        params ReadOnlySpan<(TCapability Row, bool Observed)> evidence)
        where TCapability : notnull, ICapability<TCapability> =>
        evidence.ToArray().AsIterable()
            .Filter(static row => row.Observed)
            .Fold(CapabilitySet<TCapability>.None, static (held, row) => held.With(row.Row));

    private static CapabilitySet<CapabilityAttestation> Attested(bool procedure, bool measurement) =>
        Held((CapabilityAttestation.Procedure, procedure), (CapabilityAttestation.Measurement, measurement));

    // ProcedureReceipt.Qualified is the owner's own compliance verdict over every row; a joined process without one is unqualified.
    private static bool ProcedureQualified(ProcessKind process, Option<ProcedureReceipt> procedure) =>
        procedure.Match(
            Some: receipt => receipt.Process == process && receipt.Qualified,
            None: () => process.Modality.Class != ModalityClass.Joined);

    private static Seq<SpcLimitRow> Points(SpcChart chart, Arr<double> values, double center, double sigma, Instant at, double width) =>
        toSeq(values).Map((value, index) => Point(chart, index, at, value, center, sigma, width));

    private static SpcLimitRow Point(SpcChart chart, int index, Instant at, double value, double center, double sigma, double width) =>
        new(chart, index, at, value, center, sigma, chart.LowerLimit(center, width * sigma), center + (width * sigma));

    private static Fin<Unit> Admit(CapabilityStudy study, CapabilityTolerance tolerance) =>
        (Check(study.Switch(
                state: tolerance.Measurement,
                variables: static (measurement, _) => measurement is MeasurementEvidence.Variable,
                attributes: static (measurement, _) => measurement is MeasurementEvidence.Attribute), StudyMismatch),
            Check(study.Switch(
                state: tolerance.Control.MinimumObservations,
                variables: static (minimum, evidence) => evidence.Samples.Count >= minimum,
                attributes: static (minimum, evidence) => evidence.Samples.Count >= minimum), UnderpoweredStudy),
            Check(study is CapabilityStudy.Variables || tolerance.Stackup.IsNone, StackupUnsupported),
            Check(tolerance.Procedure.ForAll(receipt => receipt.At <= tolerance.At), ProcedureNotYetIssued))
            .Apply(static (_, _, _, _) => unit)
            .As()
            .ToFin();

    // One error per gate keeps accumulation informative; a shared error collapses every fault into one indistinguishable row.
    private static K<Validation<Error>, Unit> Check(bool condition, Error fault) =>
        guard(condition, fault).ToValidation();

    private static double SpecHalfBand(CapabilityTolerance tolerance, double mean) =>
        tolerance.LowerSpecMm.Map(lower => mean - lower)
            .Bind(lower => tolerance.UpperSpecMm.Map(upper => double.Min(lower, upper - mean)))
            .IfNone(tolerance.LowerSpecMm.Map(lower => mean - lower)
                .IfNone(tolerance.UpperSpecMm.Map(upper => upper - mean).IfNone(0.0)));

    // The kernel span leg over a contiguous plane: one vectorized centred two-pass, the unbiased normalizer named
    // at the site. The hand loop it replaces re-derived the mean, squared through `Math.Pow`, and answered a
    // fabricated `0.0` at one observation — a spread no sample measured, which the receipt's own finiteness screen
    // now reports as absence instead.
    private static double SampleSigma(Arr<double> values) =>
        Stat<Scalar>.Of(values.AsSpan(), CapabilityOp)
            .Map(static stat => stat.Deviation(MomentNormalizer.Sample))
            .Match(Succ: static deviation => double.IsFinite(deviation) ? deviation : 0.0, Fail: static _ => 0.0);

    private static double Mean(Arr<double> values) => TensorPrimitives.Average<double>(values.AsSpan());

    private static double C4(int subgroupSize) =>
        Math.Exp(SpecialFunctions.GammaLn(subgroupSize / 2.0) - SpecialFunctions.GammaLn((subgroupSize - 1.0) / 2.0))
        / Math.Sqrt((subgroupSize - 1.0) / 2.0);

    private static Fin<double> BetaQuantile(double a, double b, double probability, DistributionPolicy policy) =>
        Quantile(new MathNet.Numerics.Distributions.Beta(a, b), probability, policy).ToFin(Refusal("beta-quantile"));

    private static Fin<double> Finite(double value) =>
        double.IsFinite(value)
            ? Fin.Succ(value)
            : Fin.Fail<double>(Refusal("non-finite-estimate"));

    private static DriftRow Drift(Arr<double> values) {
        double[] x = Generate.LinearSpaced(values.Count, 0.0, values.Count - 1.0);
        (double intercept, double slope) = Fit.Line(x, values.ToArray());
        return new DriftRow(intercept, slope);
    }
}
```

## [06]-[HISTORY]

- Owner: `CapabilityHistory` owns the validity-bounded ledger row `Gate` and `Achievable` select on; `CapabilitySlots` names the durable shop-state streams that ledger rides.
- Law: grade NAME and diameter band both discriminate a history row; the allowance factor is downstream policy and never selects evidence.
- Boundary: `CapabilityHistory` is input-carried evidence — enrollment and persistence remain orchestration effects riding the `store.fabrication.capability.<verb>` streams on the Persistence slot registry, so history-backed gates survive restart while this page stays effect-free.

```csharp signature
// --- [MODELS] -------------------------------------------------------------------------------------------------------------------------------------
[ComplexValueObject]
public sealed partial class CapabilityHistory {
    public CapabilityIdentity Identity { get; }
    public ItGrade Grade { get; }
    public double Cpk { get; }
    public double DemandedCpk { get; }

    // Ledger rows carry BOTH capability sets whole, so a gate reading history and a gate reading a fresh verdict
    // answer through one algebra, a refusal names the rows the row actually missed, and a third attestation or a
    // fourth control fact reaches the ledger with no column here. `Controlled` stays as the whole-set read every
    // prior consumer of the boolean column already spelled.
    public CapabilitySet<ControlEvidence> Control { get; }
    public CapabilitySet<CapabilityAttestation> Attested { get; }
    public bool Controlled => Control.AdmitsAll(CapabilitySet<ControlEvidence>.All);
    public double EffectiveSampleSize { get; }
    public Instant ValidFrom { get; }
    public Instant ValidUntil { get; }

    // The validity window is a HALF-OPEN interval and the qualification is the WHOLE demand — control, attestation,
    // and index together. Each stood re-spelled at the projection site, where a fourth control fact or a third
    // attestation would have been added to the ledger and silently missed by the filter; the row answers both, so
    // `Achievable` states which rows it wants rather than re-deriving what makes one authoritative.
    public bool Covers(Instant at) => ValidFrom <= at && at < ValidUntil;

    public bool Qualifies => Controlled
        && Attested.AdmitsAll(CapabilitySet<CapabilityAttestation>.All)
        && Cpk >= DemandedCpk;

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref CapabilityIdentity identity,
        ref ItGrade grade,
        ref double cpk,
        ref double demandedCpk,
        ref CapabilitySet<ControlEvidence> control,
        ref CapabilitySet<CapabilityAttestation> attested,
        ref double effectiveSampleSize,
        ref Instant validFrom,
        ref Instant validUntil) =>
        validationError = ValidityClaim.All(
            identity is not null, grade is not null,
            ValidityClaim.CountAtLeast(grade.Number, 1),
            identity.Feature == grade.Diameter,
            ValidityClaim.Finite(cpk),
            ValidityClaim.Positive(demandedCpk),
            ValidityClaim.Ordered(2.0, effectiveSampleSize),
            validFrom != default,
            // The window compares as INSTANTS, never through a tick projection: NodaTime's tick count exceeds the
            // exactly-representable double range, so a scalar claim row would order two nearby instants by rounding.
            validUntil > validFrom)
                ? null
                : Capability.Validation("history");

    public static Fin<CapabilityHistory> From(CapabilityReport report, Instant validUntil) =>
        Validate(
            report.Identity,
            report.Grade,
            report.Rows.Find(static row => row.Metric == CapabilityMetric.Cpk).Map(static row => row.Value).IfNone(report.Verdict.Cpk),
            report.Verdict.DemandedCpk,
            report.Control,
            report.Verdict.Attested,
            report.Dependence.EffectiveSampleSize,
            report.At,
            validUntil,
            out CapabilityHistory history).Admitted(history);

    // Grade name and diameter band both discriminate; the allowance factor is downstream policy and never selects evidence.
    public static Option<CapabilityHistory> Of(
        CapabilityIdentity identity,
        ItGrade grade,
        Instant at,
        Seq<CapabilityHistory> history) =>
        history.Filter(row => row.Identity == identity && row.Grade.Name == grade.Name
                && row.Grade.Diameter == grade.Diameter && row.Covers(at))
            .Fold(Option<CapabilityHistory>.None, static (best, row) =>
                best.Filter(held => held.ValidFrom >= row.ValidFrom).IfNone(row));
}

// Durable shop-state seam: capability history persists as slot-registered streams — the enroll slot carries
// each sealed `CapabilityReport` verdict projection, the history slot the validity-bounded `CapabilityHistory`
// ledger re-admitted into `Gate` and `Achievable` at composition. Enrollment stays an orchestration effect;
// spellings are value federation onto the Persistence slot registry's contributed span, and no Persistence type
// crosses this boundary.
public static class CapabilitySlots {
    public const string Enroll = "store.fabrication.capability.enroll";
    public const string History = "store.fabrication.capability.history";
}
```

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
