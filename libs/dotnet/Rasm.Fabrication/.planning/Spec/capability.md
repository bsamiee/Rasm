# [RASM_FABRICATION_CAPABILITY]

`Capability` owns characteristic-scoped process evidence from admission through control-state, distribution, measurement-system, tolerance-stack, history, and plan-gate projection. Variable and attribute studies share one assessment pipeline, and every report preserves the evidence that makes its verdict reproducible.

`CapabilityIdentity`, `ToleranceChain`, `ProcedureAssessment`, `Stat`, `CapabilityVerdict`, and `FabricationFault` remain the contract owners. `CapabilityReport` is the terminal specification result, while `CapabilityHistory` carries its validity-bounded ledger projection into `Gate` and `Achievable`.

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

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
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
using Rasm.Element.Projection;
using System.Numerics.Tensors;
using UnitsNet;
using Rasm.Fabrication.Joining;
using Rasm.Fabrication.Process;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Spec;

// --- [VOCABULARY] ----------------------------------------------------------------------
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

    public Option<double> Of(CapabilityMoment moment, Option<CapabilityDistribution> fitted, CapabilityTolerance tolerance) =>
        from spread in Method.Of(Scale, moment, fitted, tolerance)
        let adjusted = Adjust(spread, moment, tolerance)
        from index in Side.Match(
            Some: side => side.Index(adjusted, tolerance),
            None: () => Closest(adjusted, tolerance))
        select index;

    private static CapabilitySpread Taguchi(CapabilitySpread spread, CapabilityMoment moment, CapabilityTolerance tolerance) =>
        tolerance.TargetMm
            .Map(target => Math.Sqrt(1.0 + Math.Pow((moment.Mean - target) / CapabilityScale.Long.Sigma(moment), 2.0)))
            .Map(correction => spread with { Lower = spread.Lower * correction, Upper = spread.Upper * correction })
            .IfNone(spread);

    private static CapabilitySpread Unadjusted(CapabilitySpread spread, CapabilityMoment _, CapabilityTolerance __) => spread;

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

    public double LowerLimit(double center, double band) =>
        Attribute ? double.Max(0.0, center - band) : center - band;

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

```csharp
// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<double>]
public readonly partial struct SigmaSpan {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) =>
        validationError = ValidityClaim.Nonnegative(value) ? null : Capability.Validation("sigma-span");

    public double Of(double sigma) => ToValue() * sigma;
}

[ComplexValueObject]
public sealed partial class SearchBracket {
    public double Lower { get; }
    public double Upper { get; }

    static partial void ValidateFactoryArguments(ref ValidationError? validationError,
        ref double lower, ref double upper) =>
        validationError = ValidityClaim.All(
            ValidityClaim.Positive(lower), ValidityClaim.Ordered(lower, upper), lower < upper)
            ? null
            : Capability.Validation("search-bracket");

    public double Span => Upper - Lower;
}

[ComplexValueObject]
public sealed partial class DistributionPolicy {
    public int CandidateCount { get; }
    public SearchBracket StudentFreedom { get; }
    public SearchBracket Shape { get; }

    public double RootAccuracy { get; }
    public int RootIterations { get; }
    public SigmaSpan Bracket { get; }
    public int FitSeed { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref int candidateCount,
        ref SearchBracket studentFreedom,
        ref SearchBracket shape,
        ref double rootAccuracy,
        ref int rootIterations,
        ref SigmaSpan bracket,
        ref int fitSeed) =>
        validationError = ValidityClaim.All(
            ValidityClaim.CountAtLeast(candidateCount, 2),
            studentFreedom is not null && studentFreedom.Lower > 2.0,
            shape is not null,
            ValidityClaim.Positive(rootAccuracy),
            ValidityClaim.CountAtLeast(rootIterations, 1),
            ValidityClaim.Positive(bracket.ToValue()))
            ? null
            : Capability.Validation("distribution-policy");

    public Option<double> Root(Func<double, double> residual, SearchBracket over) =>
        Brent.TryFindRoot(residual, over.Lower, over.Upper, RootAccuracy, RootIterations, out double root)
            ? Some(root)
            : None;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DistributionParameters : IValidityEvidence {
    private DistributionParameters() { }

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

```csharp
// --- [MODELS] --------------------------------------------------------------------------
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

    public Ratio GrrFraction => Ratio.FromDecimalFractions(Grr.Millimeters / Math.Sqrt(
        (Grr.Millimeters * Grr.Millimeters) + (PartVariation.Millimeters * PartVariation.Millimeters)));

    public int DistinctCategories =>
        (int)Math.Floor(1.41 * PartVariation.Millimeters / double.Max(Grr.Millimeters, double.Epsilon));

    public bool Suitable => GrrFraction <= MaximumGrr && DistinctCategories >= MinimumDistinctCategories;

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
            ValidityClaim.Positive(partVariation.Millimeters),
            ValidityClaim.UnitInterval(maximumGrr.DecimalFractions),
            ValidityClaim.Positive(maximumGrr.DecimalFractions),
            ValidityClaim.CountAtLeast(minimumDistinctCategories, 1))
                ? null
                : Capability.Validation("variable-msa");
}

[ComplexValueObject]
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

[ComplexValueObject]
public sealed partial class ControlPolicy {
    public int SubgroupSize { get; }
    public int MinimumObservations { get; }

    public SigmaSpan Band { get; }
    public SigmaSpan CusumSlack { get; }
    public SigmaSpan CusumDecision { get; }
    public Ratio EwmaWeight { get; }
    public int MaximumLag { get; }

    public Ratio MaximumAutocorrelation { get; }
    public Length MaximumDrift { get; }

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

public readonly record struct AttributeBounds(double ProportionLower, double ProportionUpper,
    double CountLower, double CountUpper);

[SmartEnum<string>]
public sealed partial class AttributeChart {
    public static readonly AttributeChart Proportion = new("p", SpcChart.P, static _ => true,
        static (sample, cohort) => (
            (double)sample.Nonconforming / sample.Inspected, cohort.P,
            Math.Sqrt(cohort.P * (1.0 - cohort.P) / sample.Inspected)),
        static (cohort, bounds, tail) => (cohort.P, bounds.ProportionLower, bounds.ProportionUpper, tail));
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

[ComplexValueObject]
public sealed partial class StackContributor {
    public string Term { get; }
    public double BiasMm { get; }
    public Arr<double> FactorLoadings { get; }
    public Option<DistributionParameters> Fitted { get; }

    public double IndependentLoading => Math.Sqrt(1.0 - FactorLoadings.Fold(0.0, static (sum, value) => sum + (value * value)));

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref string term,
        ref double biasMm,
        ref Arr<double> factorLoadings,
        ref Option<DistributionParameters> fitted) {
        term = term?.Trim() ?? string.Empty;
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

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref ToleranceChain chain,
        ref Seq<StackContributor> contributors,
        ref int trials,
        ref Ratio tailProbability,
        ref int randomSeed) =>
        validationError = ValidityClaim.All(
            chain is not null,
            ValidityClaim.CountAtLeast(contributors.Count, 1),
            ValidityClaim.CountAtLeast(trials, 2),
            tailProbability.DecimalFractions is > 0.0 and < 0.5,
            ValidityClaim.Ordered(1.0, trials * tailProbability.DecimalFractions),
            ValidityClaim.CountExactly(contributors.Map(static row => row.Term).Distinct().Count, contributors.Count),
            toSet(contributors.Map(static row => row.Term))
                == toSet(toSeq(chain.Terms).Map(static row => row.Key)),
            contributors.Map(static row => row.FactorLoadings.Count).Distinct().Count <= 1)
                ? null
                : Capability.Validation("stack-policy");

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

    public SigmaSpan SpreadSpan { get; }
    public DistributionPolicy Distribution { get; }
    public ControlPolicy Control { get; }
    public MeasurementEvidence Measurement { get; }
    public Option<ProcedureAssessment> Procedure { get; }
    public Option<StackupPolicy> Stackup { get; }
    public Instant At { get; }

    public Option<double> LowerSpecMm => LowerSpec.Map(static value => value.Millimeters);
    public Option<double> UpperSpecMm => UpperSpec.Map(static value => value.Millimeters);
    public Option<double> TargetMm => Target.Map(static value => value.Millimeters);
    public double TailProbabilityValue => TailProbability.DecimalFractions;
    public double ConfidenceValue => Confidence.DecimalFractions;
    public double DemandedCpk => MathNet.Numerics.Distributions.Normal.InvCDF(0.0, 1.0, 1.0 - TailProbabilityValue) / 3.0;

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
        ref Option<ProcedureAssessment> procedure,
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
- Entry: `Capability.Assess`, `Capability.Gate`, and `Capability.Achievable` parameterize assessment, ledger selection, and tolerance projection without ambient state; `Assess` takes the trailing optional instrument set the run spine hands it, so observation stays at the settled result and every estimation fold stays observation-free.
- Auto: `Validation` accumulates independent request and gate faults under distinct errors; `Stat<Scalar>` owns variable moments and `Distribution<Scalar>` the simulation tail; `Distance.Pearson` derives the autocorrelation spectrum; `Fit.Line` derives drift; `SpecialFunctions.GammaLn` and `Gamma` own distribution functions; `Traverse`, `Choose`, and `Fold` own collection flow.
- Result: `CapabilityReport` carries moment and percentile indices or attribute rates, per-metric confidence intervals, pointwise control limits, merged rule windows, the fitted distribution, effective sample size, measurement and procedure evidence, the optional stackup assessment with both analytic evaluations and its covariance shares, the control-evidence set, and the admitted `CapabilityVerdict`. `Capability.Assess` writes index rows, the study verdict, and violation count through the mounted `FabricationInstruments` set.
- Packages: MathNet.Numerics owns fitted distributions, roots, regression, correlation, and batch sampling; `Rasm.Domain` owns `Stat<TCarrier>`, `Distribution<TCarrier>`, `MomentNormalizer`, `QuantileRule`, the `Tolerance` band carrier, and the `CapabilitySet`/`ICapability` axis; `Rasm.Element` owns the `AdmissionSlots` gate and accumulate fold; `System.Numerics.Tensors` owns numeric reductions; CommunityToolkit.HighPerformance owns pooled and partitioned trial execution; UnitsNet owns specification lengths, achievable tolerance, and probability ratios; `ToolEvidence` carries MTConnect operating state decoded at `Tooling/magazine`; Thinktecture and LanguageExt own generated values and the accumulated `Validation`.
- Boundary: `CapabilityReport` never enters `FabricationResult`, and only `CapabilityVerdict` crosses the plan boundary.

```csharp
public sealed record CapabilitySeries(
    Arr<double> ResidualMm,
    Seq<Arr<double>> Groups,
    Arr<double> Means,
    Arr<double> Ranges,
    Arr<double> Sigmas);

public sealed record CapabilityMoment(double Mean, double WithinSigma, double OverallSigma, double Minimum, double Maximum);

public sealed record CapabilitySpread(double Center, double Lower, double Upper);

public sealed record CapabilityDistribution(DistributionParameters Parameters, double FitError, double Akaike);

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

public sealed record StackContribution(string Term, double Share, double SigmaMm, double TighteningFactor);

public sealed record AchievableTolerance(Length Width, ItGrade Grade, double Cpk, double EffectiveSampleSize);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record StackupVerdict {
    private StackupVerdict() { }

    public sealed record Within(double MarginMm) : StackupVerdict;
    public sealed record Exceeded(double OverrunMm, Option<StackContribution> Dominant) : StackupVerdict;

    public bool Pass => this is Within;

    public static StackupVerdict Of(double tailMm, double boundMm, Option<StackContribution> dominant) =>
        tailMm <= boundMm ? new Within(boundMm - tailMm) : new Exceeded(tailMm - boundMm, dominant);
}

public sealed record StackupAssessment(
    ChainEvidence Analytic,
    ChainEvidence Arithmetic,
    double MeanMm,
    double SigmaMm,
    double TailMm,
    int RandomSeed,
    int FactorCount,
    Seq<StackContribution> Contributions,
    StackupVerdict Verdict) {
    public ContentKey Source => Analytic.Key;
    public double BoundMm => Analytic.BoundMm;

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
    Option<ProcedureAssessment> Procedure,
    Option<StackupAssessment> Stackup,
    CapabilitySet<ControlEvidence> Control,
    CapabilityVerdict Verdict,
    Instant At) {
    public bool Controlled => Control.AdmitsAll(CapabilitySet<ControlEvidence>.All);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Capability {
    internal const long ContributorLane = 0L;
    internal const long SharedFactorLane = 1L;

    internal static readonly Op CapabilityOp = Op.Of(name: "fabrication:capability");

    internal static ValidationError Validation(string locus) => new($"capability:{locus}");

    internal static FabricationFault Inadmissible(string locus) =>
        FabricationFault.Inadmissible(FabConcern.Spec, $"capability:{locus}");

    internal static Error Refusal(string locus) => Inadmissible(locus);

    private static readonly Error MissingHistory = Refusal("missing-history");
    private static readonly Error StudyMismatch = Refusal("study-identity-mismatch");
    private static readonly Error UnderpoweredStudy = Refusal("underpowered-study");
    private static readonly Error StackupUnsupported = Refusal("stackup-unsupported");
    private static readonly Error ProcedureNotYetIssued = Refusal("procedure-not-yet-issued");

    public static Fin<CapabilityReport> Assess(
        CapabilityStudy study,
        CapabilityTolerance tolerance,
        Option<InstrumentSet> set = default) =>
        from _ in Admit(study, tolerance)
        from report in study.Switch(
            state: tolerance,
            variables: static (demand, evidence) => Variables(evidence.Samples, demand),
            attributes: static (demand, evidence) => Attributes(evidence.Samples, demand))
        from _indices in report.Rows
            .TraverseM(row => set.Write(FabricationInstruments.CapabilityIndex, row.Value,
                (FabricationInstruments.MetricSlot, row.Metric.Key))).As()
        from _study in set.Write(FabricationInstruments.CapabilityStudies, 1d,
            (FabricationInstruments.VerdictSlot, report.Violations.IsEmpty
                ? FabricationInstruments.Pass
                : FabricationInstruments.Fail))
        from _violations in set.Write(FabricationInstruments.CapabilityViolations, report.Violations.Count)
        select report;

    public static Fin<CapabilityVerdict> Gate(CapabilityIdentity identity, ItGrade grade, Instant at, Seq<CapabilityHistory> history) =>
        CapabilityHistory.Of(identity, grade, at, history)
            .ToFin(MissingHistory)
            .Bind(row => CapabilityVerdict
                .Admit(row.Cpk, row.DemandedCpk, row.Attested)
                .Bind(verdict => (Demanded(row),
                        Check(row.Cpk >= row.DemandedCpk,
                            new FabricationFault.CapabilityShortfall(identity.Process, row.Cpk, row.DemandedCpk)))
                    .Apply((_, _) => verdict)
                    .As()
                    .ToFin()));

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

    private static Fin<CapabilityMoment> Moment(CapabilitySeries series, CapabilityTolerance tolerance) =>
        from stat in Stat<Scalar>.Of(
            series.ResidualMm.ToSeq().Map(static value => (Scalar)value), CapabilityOp)
        let banded = Tolerance.Of(ToleranceLane.Deviation, SpecHalfBand(tolerance, stat.Mean), CapabilityOp)
            .Match(Succ: static band => StatContext.Band(band), Fail: static _ => StatContext.None)
        from accepted in CapabilityOp.AcceptValue(value: stat with { Context = banded })
        select new CapabilityMoment(
            accepted.Mean,
            tolerance.Control.SubgroupSize == 1
                ? Mean(series.Ranges) / ControlConstant.Get(2).RangeMean
                : Mean(series.Sigmas) / C4(tolerance.Control.SubgroupSize),
            accepted.Deviation(MomentNormalizer.Sample),
            accepted.Minimum.To(),
            accepted.Maximum.To());

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

    private static CapabilityDistribution Assessed(
        DistributionParameters parameters, Arr<double> values, DistributionPolicy policy) {
        IContinuousDistribution fitted = parameters.Create(Deterministic.Source(seed: policy.FitSeed));
        return new CapabilityDistribution(parameters, Supremum(fitted, toSeq(values.Order())), Akaike(fitted, parameters, values));
    }

    private static double Akaike(IContinuousDistribution fitted, DistributionParameters parameters, Arr<double> values) {
        double logLikelihood = toSeq(values).Fold(0.0, (sum, value) => sum + Math.Log(fitted.Density(value)));
        return double.IsFinite(logLikelihood)
            ? (2.0 * parameters.FreeParameters) - (2.0 * logLikelihood)
            : double.PositiveInfinity;
    }

    private static double Supremum(IContinuousDistribution fitted, Seq<double> ordered) =>
        ordered.Map((value, index) => Math.Abs(fitted.CumulativeDistribution(value) - ((index + 0.5) / ordered.Count))).Max(0.0);

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

    private static Seq<SpcLimitRow> AttributeLimits(
        Seq<AttributeSample> samples, AttributeCohort cohort, CapabilityTolerance tolerance) =>
        from row in AttributeChart.For(cohort)
        from indexed in samples.Map(static (sample, index) => (Sample: sample, Index: index))
        let plot = row.Plot(indexed.Sample, cohort)
        select Point(row.Chart, indexed.Index, indexed.Sample.At, plot.Value, plot.Center, plot.Sigma,
            tolerance.Control.Band.ToValue());

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
        int factors = policy.Contributors.Head.Map(static row => row.FactorLoadings.Count).IfNone(0);
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
        return from spreadStat in Distribution<Scalar>.Of(
                   toSeq(trials.ToArray().Map(static value => (Scalar)value)),
                   Seq(probability, 1.0 - probability), CapabilityOp, Some(QuantileRule.Interpolated))
               let mean = spreadStat.Summary.Mean
               let tail = spreadStat.Percentiles.Fold(0.0,
                   static (worst, row) => double.Max(worst, Math.Abs(row.Value.To())))
               let covariance = Covariances(policy, spread, independent, shared, trials, mean)
               let variance = double.Max(TensorPrimitives.Sum<double>(covariance), double.Epsilon)
               let contributions = Contributions(policy, spread, covariance, variance, tail)
               let analytic = policy.Chain.Evaluate()
               select new StackupAssessment(
                   analytic,
                   policy.Chain.Evaluate(StackMethod.WorstCase),
                   mean,
                   spreadStat.Summary.Deviation(MomentNormalizer.Sample),
                   tail,
                   policy.RandomSeed,
                   factors,
                   contributions,
                   StackupVerdict.Of(tail, analytic.BoundMm,
                       contributions.Fold(Option<StackContribution>.None,
                           static (best, row) => best.Filter(held => held.Share >= row.Share).IfNone(row))));
    }

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

    private static CapabilitySet<TCapability> Held<TCapability>(
        params ReadOnlySpan<(TCapability Row, bool Observed)> evidence)
        where TCapability : notnull, ICapability<TCapability> =>
        evidence.ToArray().AsIterable()
            .Filter(static row => row.Observed)
            .Fold(CapabilitySet<TCapability>.None, static (held, row) => held.With(row.Row));

    private static CapabilitySet<CapabilityAttestation> Attested(bool procedure, bool measurement) =>
        Held((CapabilityAttestation.Procedure, procedure), (CapabilityAttestation.Measurement, measurement));

    private static bool ProcedureQualified(ProcessKind process, Option<ProcedureAssessment> procedure) =>
        procedure.Match(
            Some: result => result.Process == process && result.Qualified,
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
            Check(tolerance.Procedure.ForAll(result => result.At <= tolerance.At), ProcedureNotYetIssued))
            .Apply(static (_, _, _, _) => unit)
            .As()
            .ToFin();

    private static K<Validation<Error>, Unit> Check(bool condition, Error fault) =>
        guard(condition, fault).ToValidation();

    private static double SpecHalfBand(CapabilityTolerance tolerance, double mean) =>
        tolerance.LowerSpecMm.Map(lower => mean - lower)
            .Bind(lower => tolerance.UpperSpecMm.Map(upper => double.Min(lower, upper - mean)))
            .IfNone(tolerance.LowerSpecMm.Map(lower => mean - lower)
                .IfNone(tolerance.UpperSpecMm.Map(upper => upper - mean).IfNone(0.0)));

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

```csharp
// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
public sealed partial class CapabilityHistory {
    public CapabilityIdentity Identity { get; }
    public ItGrade Grade { get; }
    public double Cpk { get; }
    public double DemandedCpk { get; }

    public CapabilitySet<ControlEvidence> Control { get; }
    public CapabilitySet<CapabilityAttestation> Attested { get; }
    public bool Controlled => Control.AdmitsAll(CapabilitySet<ControlEvidence>.All);
    public double EffectiveSampleSize { get; }
    public Instant ValidFrom { get; }
    public Instant ValidUntil { get; }

    public bool Covers(Instant at) => ValidFrom <= at && at < ValidUntil;

    public bool Qualifies => Controlled
        && Attested.AdmitsAll(CapabilitySet<CapabilityAttestation>.All)
        && Cpk >= DemandedCpk;

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

public static class CapabilitySlots {
    public const string Enroll = "store.fabrication.capability.enroll";
    public const string History = "store.fabrication.capability.history";
}
```

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
