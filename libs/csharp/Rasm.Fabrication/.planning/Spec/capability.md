# [RASM_FABRICATION_CAPABILITY]

`Capability` owns characteristic-scoped process evidence from admission through control-state, distribution, measurement-system, tolerance-stack, history, and plan-gate projection. Variable and attribute studies share one assessment rail, and every report preserves the evidence that makes its verdict reproducible.

`CapabilityIdentity`, `ToleranceChain`, `ProcedureReceipt`, `Stat`, `CapabilityVerdict`, and `FabricationFault` remain the seam owners. `CapabilityReport` is the terminal specification receipt, while `CapabilityHistory` carries its validity-bounded ledger projection into `Gate` and `Achievable`.

## [01]-[INDEX]

- [02]-[CAPABILITY_VOCABULARY]: the index roster and its estimation methods beside the generated SPC chart, rule, and control-constant rows.
- [03]-[DISTRIBUTION_FIT]: the fitted family union, its support-gated moment seeds, and the policy every numeric the fitting lane spends rides.
- [04]-[STUDY_ADMISSION]: study identity, measurement-system evidence, control policy, the attribute cohort, and the chain-bound stackup contributors.
- [05]-[ASSESSMENT]: `Capability.Assess` over either study case, control-limit and violation derivation, the correlated stackup, and the ledger projections.
- [06]-[HISTORY]: the validity-bounded ledger row and the durable slots it rides.

## [02]-[CAPABILITY_VOCABULARY]

- Owner: `CapabilityMetric` owns the index roster and the standard error each index carries; `CapabilityMethod` owns moment and percentile estimation over one `CapabilitySpread`; `CapabilitySide` owns the one sided-index algebra both methods enter; `SpcChart`, `SpcRule`, `SpcRuleClass`, and `ControlConstant` own generated control policy.
- Law: `CapabilityMethod` closes moment and ISO 22514-4 percentile estimation, so the fitted distribution decides the non-normal index instead of decorating the report; the demand itself is the fourth operand, so neither method takes a page constant nor a bare double whose axis is unstated.
- Law: `SpcChart.Admits` grades each rule class per chart, so every chart signals on its own control band while the zone and pattern ladders stay on symmetric equal-variance charts.
- Auto: calibrated `ControlConstant` rows carry the range mean and spread the subgroup limits derive from, and a subgroup past the calibrated roster hands spread to the s-chart rather than extrapolating a d2 that was never published.
- Growth: a capability index is one `CapabilityMetric` row; a control rule is one `SpcRule` row carrying its `SpcRuleClass`.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------------------------------------------------------
using CommunityToolkit.HighPerformance.Buffers;
using CommunityToolkit.HighPerformance.Helpers;
using Foundation.CSharp.Analyzers.Contracts;
using LanguageExt;
using LanguageExt.Common;
using MathNet.Numerics;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.RootFinding;
using NodaTime;
using Rasm.Analysis;
using Rasm.Domain;
using System.Numerics.Tensors;
using UnitsNet;
using Rasm.Fabrication.Joining;
using Rasm.Fabrication.Process;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Spec;

// --- [VOCABULARY] ---------------------------------------------------------------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class CapabilityScale {
    public static readonly CapabilityScale Short = new("short", shortTerm: true);
    public static readonly CapabilityScale Long = new("long", shortTerm: false);

    public bool ShortTerm { get; }

    public double Sigma(CapabilityMoment moment) =>
        double.Max(ShortTerm ? moment.WithinSigma : moment.OverallSigma, double.Epsilon);
}

[SmartEnum<string>]
public sealed partial class CapabilityMethod {
    public static readonly CapabilityMethod Moment = new("moment", static (scale, moment, _, tolerance) =>
        Some(Symmetric(moment.Mean, tolerance.SpreadSigmaSpan * scale.Sigma(moment))));
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

[SmartEnum<string>]
public sealed partial class CapabilityMetric {
    public static readonly CapabilityMetric Cp = Moment("cp", CapabilityScale.Short, CapabilitySide.Bilateral, targetPenalty: false);
    public static readonly CapabilityMetric Cpk = Moment("cpk", CapabilityScale.Short, side: null, targetPenalty: false);
    public static readonly CapabilityMetric Cpu = Moment("cpu", CapabilityScale.Short, CapabilitySide.Upper, targetPenalty: false);
    public static readonly CapabilityMetric Cpl = Moment("cpl", CapabilityScale.Short, CapabilitySide.Lower, targetPenalty: false);
    public static readonly CapabilityMetric Pp = Moment("pp", CapabilityScale.Long, CapabilitySide.Bilateral, targetPenalty: false);
    public static readonly CapabilityMetric Ppk = Moment("ppk", CapabilityScale.Long, side: null, targetPenalty: false);
    public static readonly CapabilityMetric Ppu = Moment("ppu", CapabilityScale.Long, CapabilitySide.Upper, targetPenalty: false);
    public static readonly CapabilityMetric Ppl = Moment("ppl", CapabilityScale.Long, CapabilitySide.Lower, targetPenalty: false);
    public static readonly CapabilityMetric Cpm = Moment("cpm", CapabilityScale.Long, CapabilitySide.Bilateral, targetPenalty: true);
    public static readonly CapabilityMetric PpQuantile = Quantile("pp-q", CapabilitySide.Bilateral);
    public static readonly CapabilityMetric PpkQuantile = Quantile("ppk-q", side: null);
    public static readonly CapabilityMetric PpuQuantile = Quantile("ppu-q", CapabilitySide.Upper);
    public static readonly CapabilityMetric PplQuantile = Quantile("ppl-q", CapabilitySide.Lower);

    public CapabilityMethod Method { get; }
    public CapabilityScale Scale { get; }
    public Option<CapabilitySide> Side { get; }
    public bool TargetPenalty { get; }

    // ISO 22514-4: the percentile method estimates spread from fitted quantiles, so a non-normal fit gates its own rows.
    public Option<double> Of(CapabilityMoment moment, Option<CapabilityDistribution> fitted, CapabilityTolerance tolerance) =>
        from spread in Method.Of(Scale, moment, fitted, tolerance)
        let inflated = TargetPenalty
            ? tolerance.TargetMm
                .Map(target => Math.Sqrt(1.0 + Math.Pow((moment.Mean - target) / Scale.Sigma(moment), 2.0)))
                .Map(correction => spread with { Lower = spread.Lower * correction, Upper = spread.Upper * correction })
                .IfNone(spread)
            : spread
        from index in Side.Match(
            Some: side => side.Index(inflated, tolerance),
            None: () => Closest(inflated, tolerance))
        select index;

    // Bissell: a one-sided index carries the mean-estimation term the bilateral index does not.
    public double StandardError(double value, double sampleSize) =>
        Side == CapabilitySide.Bilateral
            ? Math.Abs(value) / Math.Sqrt(2.0 * double.Max(sampleSize - 1.0, 1.0))
            : Math.Sqrt((1.0 / (9.0 * double.Max(sampleSize, 1.0)))
                + (value * value / (2.0 * double.Max(sampleSize - 1.0, 1.0))));

    private static CapabilityMetric Moment(string key, CapabilityScale scale, CapabilitySide? side, bool targetPenalty) =>
        new(key, CapabilityMethod.Moment, scale, Optional(side), targetPenalty);

    private static CapabilityMetric Quantile(string key, CapabilitySide? side) =>
        new(key, CapabilityMethod.Percentile, CapabilityScale.Long, Optional(side), targetPenalty: false);

    private static Option<double> Closest(CapabilitySpread spread, CapabilityTolerance tolerance) {
        Option<double> lower = CapabilitySide.Lower.Index(spread, tolerance);
        Option<double> upper = CapabilitySide.Upper.Index(spread, tolerance);
        return (from low in lower from high in upper select double.Min(low, high)) | lower | upper;
    }
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

- Owner: `DistributionParameters` closes the fitted continuous families and owns the free-parameter count the criterion charges each for; `DistributionFamily` seeds them by support; `DistributionPolicy` owns every numeric the fitting lane spends.
- Law: distribution selection is PENALIZED. A nested richer family always tracks a sample at least as closely, so Akaike's criterion over the fitted log-likelihood charges each family for its free parameters and the reported supremum stays the evidence a reader compares on; the count is a property of the FAMILY, so selection never reflects over a record's positional arity.
- Law: moment matching is a ROW per family, so a candidate space grows by one declaration and never by editing a seeding body, and a family whose support the sample violates seeds nothing rather than fitting an impossible fit.
- Auto: `Generate.LinearSpacedMap` generates the Student candidate fan and one bracketed `Brent.TryFindRoot` serves every quantile and shape MathNet exposes no closed inverse for.
- Growth: a distribution is one `DistributionParameters` case with one `DistributionFamily` seed row and one free-parameter arm.
- Boundary: a policy value here decides a caller's fit; a page-level constant is a policy column hiding from its own owner.

```csharp signature
// --- [MODELS] -------------------------------------------------------------------------------------------------------------------------------------
// The fitting policy owns EVERY numeric the fitting lane spends: the candidate fan, the Student freedom band, the
// shape bracket and accuracy every root-find runs at, the quantile bracket in standard deviations, and the draw
// seed the fit and spread share. A page-level constant here is a policy value hiding from its own owner.
[ComplexValueObject]
[ValidationError<FabricationFault>]
public sealed partial class DistributionPolicy {
    public int CandidateCount { get; }
    public double StudentFreedomMinimum { get; }
    public double StudentFreedomMaximum { get; }
    public double ShapeLowerBound { get; }
    public double ShapeUpperBound { get; }
    public double RootAccuracy { get; }
    public int RootIterations { get; }
    public double BracketSigma { get; }
    public int FitSeed { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref FabricationFault? validationError,
        ref int candidateCount,
        ref double studentFreedomMinimum,
        ref double studentFreedomMaximum,
        ref double shapeLowerBound,
        ref double shapeUpperBound,
        ref double rootAccuracy,
        ref int rootIterations,
        ref double bracketSigma,
        ref int fitSeed) =>
        validationError = candidateCount >= 2 && studentFreedomMinimum > 2.0 && studentFreedomMaximum > studentFreedomMinimum
            && shapeLowerBound > 0.0 && shapeUpperBound > shapeLowerBound
            && rootAccuracy > 0.0 && rootIterations >= 1 && bracketSigma > 0.0
            ? null
            : Capability.Inadmissible("distribution-policy");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DistributionParameters {
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

    public bool Valid => Switch(
        normal: static value => double.IsFinite(value.Mean) && double.IsFinite(value.Sigma) && value.Sigma > 0.0,
        logNormal: static value => double.IsFinite(value.Mu) && double.IsFinite(value.Sigma) && value.Sigma > 0.0,
        gamma: static value => Positive(value.Shape) && Positive(value.Rate),
        student: static value => double.IsFinite(value.Location) && Positive(value.Scale) && double.IsFinite(value.Freedom) && value.Freedom > 2.0,
        weibull: static value => Positive(value.Shape) && Positive(value.Scale),
        beta: static value => Positive(value.A) && Positive(value.B),
        chiSquared: static value => Positive(value.Freedom),
        exponential: static value => Positive(value.Rate),
        uniform: static value => double.IsFinite(value.Lower) && double.IsFinite(value.Upper) && value.Upper > value.Lower,
        cauchy: static value => double.IsFinite(value.Location) && Positive(value.Scale),
        laplace: static value => double.IsFinite(value.Location) && Positive(value.Scale),
        rayleigh: static value => Positive(value.Scale),
        fisher: static value => Positive(value.D1) && double.IsFinite(value.D2) && value.D2 > 4.0,
        triangular: static value => double.IsFinite(value.Lower) && double.IsFinite(value.Upper) && double.IsFinite(value.Mode) && value.Upper > value.Lower
            && value.Mode >= value.Lower && value.Mode <= value.Upper,
        pareto: static value => Positive(value.Scale) && double.IsFinite(value.Shape) && value.Shape > 1.0,
        inverseGamma: static value => double.IsFinite(value.Shape) && value.Shape > 2.0 && Positive(value.Scale),
        betaScaled: static value => Positive(value.A) && Positive(value.B) && double.IsFinite(value.Lower)
            && double.IsFinite(value.Upper) && value.Upper > value.Lower,
        logistic: static value => double.IsFinite(value.Mean) && Positive(value.Scale));

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

    private static bool Positive(double value) => double.IsFinite(value) && value > 0.0;
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
            policy.CandidateCount, policy.StudentFreedomMinimum, policy.StudentFreedomMaximum,
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
        static (_, _, policy) => new DistributionParameters.Fisher(5.0, double.Max(5.0, policy.StudentFreedomMaximum)));
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
        Support.Admits(moment) ? Seed(moment, sigma, policy).Filter(static row => row.Valid) : Seq<DistributionParameters>();

    private static DistributionFamily One(string key, DistributionSupport support,
        Func<CapabilityMoment, double, DistributionPolicy, DistributionParameters> seed) =>
        new(key, support, (moment, sigma, policy) => Seq(seed(moment, sigma, policy)));

    private static DistributionParameters LogParameters(double mean, double sigma) =>
        LogNormalOf(mean, Math.Sqrt(Math.Log(1.0 + Math.Pow(sigma / mean, 2.0))));

    private static DistributionParameters LogNormalOf(double mean, double logSigma) =>
        new DistributionParameters.LogNormal(Math.Log(mean) - (logSigma * logSigma / 2.0), logSigma);

    private static Option<double> WeibullShape(double coefficient, DistributionPolicy policy) =>
        Brent.TryFindRoot(
            shape => (SpecialFunctions.Gamma(1.0 + (2.0 / shape)) / Math.Pow(SpecialFunctions.Gamma(1.0 + (1.0 / shape)), 2.0))
                - 1.0 - (coefficient * coefficient),
            policy.ShapeLowerBound,
            policy.ShapeUpperBound,
            policy.RootAccuracy,
            policy.RootIterations,
            out double shape)
                ? Some(shape)
                : None;

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
- Boundary: `CapabilityIdentity` carries the `DiameterBand` its study measured, so `Gate` and `Achievable` resolve through one identity and no row authorizes a size it never observed.
- Growth: a study modality is one `CapabilityStudy` case folded by `Assess`.

```csharp signature
// --- [MODELS] -------------------------------------------------------------------------------------------------------------------------------------
[ComplexValueObject]
[ValidationError<FabricationFault>]
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
        ref FabricationFault? validationError,
        ref ProcessKind process,
        ref UInt128 characteristic,
        ref DiameterBand feature,
        ref UInt128 machine,
        ref UInt128 material,
        ref UInt128 tool,
        ref ToolEvidence toolState,
        ref UInt128 setup) =>
        validationError = process is not null && characteristic != 0 && feature is not null
            && machine != 0 && material != 0 && tool != 0 && toolState is not null && setup != 0
            ? null
            : Capability.Inadmissible("identity");
}

[ComplexValueObject]
[ValidationError<FabricationFault>]
public sealed partial class VariableMeasurementStudy {
    public double RepeatabilityMm { get; }
    public double ReproducibilityMm { get; }
    public double PartVariationMm { get; }
    public double BiasMm { get; }
    public double LinearityMm { get; }
    public double StabilityMm { get; }
    public double MaximumPercentGrr { get; }
    public int MinimumDistinctCategories { get; }

    public double GrrMm => Math.Sqrt((RepeatabilityMm * RepeatabilityMm) + (ReproducibilityMm * ReproducibilityMm));
    public double PercentGrr => 100.0 * GrrMm / Math.Sqrt((GrrMm * GrrMm) + (PartVariationMm * PartVariationMm));
    public int DistinctCategories => (int)Math.Floor(1.41 * PartVariationMm / double.Max(GrrMm, double.Epsilon));
    public bool Suitable => PercentGrr <= MaximumPercentGrr && DistinctCategories >= MinimumDistinctCategories;

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref FabricationFault? validationError,
        ref double repeatabilityMm,
        ref double reproducibilityMm,
        ref double partVariationMm,
        ref double biasMm,
        ref double linearityMm,
        ref double stabilityMm,
        ref double maximumPercentGrr,
        ref int minimumDistinctCategories) =>
        validationError = Seq(repeatabilityMm, reproducibilityMm, partVariationMm, biasMm, linearityMm, stabilityMm, maximumPercentGrr)
                .ForAll(static value => double.IsFinite(value) && value >= 0.0)
            && partVariationMm > 0.0 && maximumPercentGrr > 0.0 && minimumDistinctCategories >= 1
                ? null
                : Capability.Inadmissible("variable-msa");
}

[ComplexValueObject]
[ValidationError<FabricationFault>]
public sealed partial class AttributeAgreementStudy {
    public double AppraiserAgreement { get; }
    public double StandardAgreement { get; }
    public double Kappa { get; }
    public double FalseAcceptRate { get; }
    public double MissRate { get; }
    public double MinimumAgreement { get; }
    public double MinimumKappa { get; }
    public double MaximumFalseDecisionRate { get; }

    public bool Suitable => AppraiserAgreement >= MinimumAgreement && StandardAgreement >= MinimumAgreement
        && Kappa >= MinimumKappa && double.Max(FalseAcceptRate, MissRate) <= MaximumFalseDecisionRate;

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref FabricationFault? validationError,
        ref double appraiserAgreement,
        ref double standardAgreement,
        ref double kappa,
        ref double falseAcceptRate,
        ref double missRate,
        ref double minimumAgreement,
        ref double minimumKappa,
        ref double maximumFalseDecisionRate) =>
        validationError = Seq(appraiserAgreement, standardAgreement, falseAcceptRate, missRate, minimumAgreement, maximumFalseDecisionRate)
                .ForAll(static value => double.IsFinite(value) && value is >= 0.0 and <= 1.0)
            && double.IsFinite(kappa) && double.IsFinite(minimumKappa) && kappa is >= -1.0 and <= 1.0 && minimumKappa is >= -1.0 and <= 1.0
                ? null
                : Capability.Inadmissible("attribute-msa");
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
[ValidationError<FabricationFault>]
public sealed partial class ControlPolicy {
    public int SubgroupSize { get; }
    public int MinimumObservations { get; }
    public double SigmaWidth { get; }
    public double EwmaWeight { get; }
    public double CusumSlackSigma { get; }
    public double CusumDecisionSigma { get; }
    public int MaximumAutocorrelationLag { get; }
    public double MaximumAbsoluteAutocorrelation { get; }
    public double MaximumAbsoluteDriftPerSample { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref FabricationFault? validationError,
        ref int subgroupSize,
        ref int minimumObservations,
        ref double sigmaWidth,
        ref double ewmaWeight,
        ref double cusumSlackSigma,
        ref double cusumDecisionSigma,
        ref int maximumAutocorrelationLag,
        ref double maximumAbsoluteAutocorrelation,
        ref double maximumAbsoluteDriftPerSample) =>
        validationError = subgroupSize >= 1 && minimumObservations >= int.Max(2, subgroupSize) && sigmaWidth > 0.0 && ewmaWeight is > 0.0 and <= 1.0
            && cusumSlackSigma >= 0.0 && cusumDecisionSigma > 0.0 && maximumAutocorrelationLag >= 1
            && maximumAbsoluteAutocorrelation is >= 0.0 and <= 1.0
            && double.IsFinite(maximumAbsoluteDriftPerSample) && maximumAbsoluteDriftPerSample >= 0.0
                ? null
                : Capability.Inadmissible("control-policy");
}

[ComplexValueObject]
[ValidationError<FabricationFault>]
public sealed partial class AttributeSample {
    public int Inspected { get; }
    public int Nonconforming { get; }
    public int Defects { get; }
    public int Opportunities { get; }
    public Instant At { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref FabricationFault? validationError,
        ref int inspected,
        ref int nonconforming,
        ref int defects,
        ref int opportunities,
        ref Instant at) =>
        validationError = inspected > 0 && nonconforming is >= 0 && nonconforming <= inspected
            && defects >= 0 && defects <= opportunities && opportunities >= inspected && at != default
            ? null
            : Capability.Inadmissible("attribute-sample");
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
[ValidationError<FabricationFault>]
public sealed partial class StackContributor {
    public string Term { get; }
    public double BiasMm { get; }
    public Arr<double> FactorLoadings { get; }
    public Option<DistributionParameters> Fitted { get; }

    public double IndependentLoading => Math.Sqrt(1.0 - FactorLoadings.Fold(0.0, static (sum, value) => sum + (value * value)));

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref FabricationFault? validationError,
        ref string term,
        ref double biasMm,
        ref Arr<double> factorLoadings,
        ref Option<DistributionParameters> fitted) {
        term = term?.Trim() ?? string.Empty;
        validationError = term.Length > 0 && double.IsFinite(biasMm)
            && fitted.ForAll(static row => row.Valid && row.FiniteMoments)
            && factorLoadings.ForAll(static value => double.IsFinite(value))
            && factorLoadings.Fold(0.0, static (sum, value) => sum + (value * value)) <= 1.0
                ? null
                : Capability.Inadmissible("stack-contributor");
    }
}

[ComplexValueObject]
[ValidationError<FabricationFault>]
public sealed partial class StackupPolicy {
    public ToleranceChain Chain { get; }
    public Seq<StackContributor> Contributors { get; }
    public int Trials { get; }
    public Ratio TailProbability { get; }
    public int RandomSeed { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref FabricationFault? validationError,
        ref ToleranceChain chain,
        ref Seq<StackContributor> contributors,
        ref int trials,
        ref Ratio tailProbability,
        ref int randomSeed) =>
        // The contributor roster is a BIJECTION onto the chain's terms. An extra contributor models a term nobody
        // declared and a missing one drops a term the analytic bound still counts, so the two readings the receipt
        // publishes would describe different stacks under one seed.
        validationError = chain is not null && !contributors.IsEmpty && trials >= 2
            && tailProbability.DecimalFractions is > 0.0 and < 0.5
            && trials * tailProbability.DecimalFractions >= 1.0
            && contributors.Map(static row => row.Term).Distinct().Count == contributors.Count
            && toSet(contributors.Map(static row => row.Term))
                == toSet(toSeq(chain.Terms).Map(static row => row.Key))
            && contributors.Map(static row => row.FactorLoadings.Count).Distinct().Count <= 1
                ? null
                : Capability.Inadmissible("stack-policy");

    // The chain term each contributor models, in the contributor's own order — resolved once at the fold rather
    // than searched per trial. The admitted bijection is what makes this positionally aligned with `Contributors`,
    // so the simulation indexes both by the same ordinal and never re-searches a key it already proved.
    public Seq<ToleranceTerm> Terms => Contributors.Choose(row =>
        toSeq(Chain.Terms).Find(term => string.Equals(term.Key, row.Term, StringComparison.Ordinal)));
}

[ComplexValueObject]
[ValidationError<FabricationFault>]
public sealed partial class CapabilityTolerance {
    public CapabilityIdentity Identity { get; }
    public ItGrade Grade { get; }
    public Option<Length> LowerSpec { get; }
    public Option<Length> UpperSpec { get; }
    public Option<Length> Target { get; }
    public Ratio TailProbability { get; }
    public Ratio Confidence { get; }

    // The index convention this study reports on: the half-span, in process sigmas, the moment method spreads to.
    public double SpreadSigmaSpan { get; }
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
        ref FabricationFault? validationError,
        ref CapabilityIdentity identity,
        ref ItGrade grade,
        ref Option<Length> lowerSpec,
        ref Option<Length> upperSpec,
        ref Option<Length> target,
        ref Ratio tailProbability,
        ref Ratio confidence,
        ref double spreadSigmaSpan,
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
        validationError = identity is not null && grade.Number >= 1 && identity.Feature == grade.Diameter
            && (lowerSpec.IsSome || upperSpec.IsSome) && finite && ordered && centered
            && double.IsFinite(spreadSigmaSpan) && spreadSigmaSpan > 0.0
            && tailProbability.DecimalFractions is > 0.0 and < 0.5 && confidence.DecimalFractions is > 0.0 and < 1.0
            && distribution is not null && control is not null && measurement is not null && procedure.ForAll(static value => value is not null)
            && stackup.ForAll(static value => value is not null) && at != default
                ? null
                : Capability.Inadmissible("tolerance");
    }
}
```

## [05]-[ASSESSMENT]

- Owner: `Capability` owns admission, the two study folds, control-limit derivation, violation merging, the correlated stackup, and the ledger projections; `CapabilityReport` conserves every derived decision.
- Law: every gate refusal carries its OWN discriminant. The kernel `InvalidInput`/`InvalidResult` mints take no detail slot, so eight gates lowering onto them are eight refusals a caller cannot tell apart; each answers on the fabrication band under a declared locus, and `Inadmissible` is the one mint both a generated hook and a fold gate read.
- Law: `StackupReceipt.Pass` is a VERDICT, not a fault. A stack exceeding its bound is exactly the answer the study was run to obtain and its contribution ranking is the evidence naming the term worth tightening, so the receipt returns and the consuming gate decides what the exceedance means.
- Law: contribution shares are CORRELATED shares. The simulation loads every contributor on the same shared factors, so a term's share is its covariance with the assembled response over the response variance — an independence fraction under a correlated model hands a shared factor's spread to whichever term carries the largest loading.
- Law: one out-of-control EPISODE is one violation. A run longer than a rule's window breaches at every offset inside it, so overlapping and adjacent breach windows merge into the maximal span they cover and the excursion is the worst standardized point in that span.
- Law: `Achievable` returns the qualifying row's band beside the evidence that earned it — grade, index, and effective sample size — so a consumer grading confidence reads the support behind the projection rather than assigning a constant trust to the word history.
- Entry: `Capability.Assess`, `Capability.Gate`, and `Capability.Achievable` parameterize assessment, ledger selection, and tolerance projection without ambient state; `Assess` takes the trailing `FabricationTap?` the run spine hands it, so the fact fires where the receipt settles and every estimation fold stays tap-free.
- Auto: `Validation` accumulates independent request and gate faults under distinct errors; `Stat.Of` owns variable moments; `Distance.Pearson` derives the autocorrelation spectrum; `Fit.Line` derives drift; `SpecialFunctions.GammaLn` and `Gamma` own distribution functions; `Traverse`, `Choose`, and `Fold` own collection flow.
- Receipt: `CapabilityReport` carries moment and percentile indices or attribute rates, per-metric confidence intervals, pointwise control limits, merged rule windows, the fitted distribution, effective sample size, measurement and procedure evidence, the optional stackup with both analytic readings and its covariance shares, control state, and the admitted `CapabilityVerdict`. `FabricationFact.Capability.Of` projects the index rows and violation count onto `rasm.fabrication.capability.index` and `rasm.fabrication.capability.violations` through `Process/telemetry#FACT_PROJECTION` as kind `capability`.
- Packages: MathNet.Numerics owns fitted distributions, roots, regression, correlation, and batch sampling; `System.Numerics.Tensors` owns numeric reductions; CommunityToolkit.HighPerformance owns pooled and partitioned trial execution; UnitsNet owns specification lengths, achievable tolerance, and probability ratios; `ToolEvidence` carries MTConnect operating state decoded at `Tooling/magazine`; Thinktecture and LanguageExt own generated values and the accumulated rail.
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

public sealed record CapabilityRow(CapabilityMetric Metric, double Value, double Demanded, bool Pass);

public sealed record CapabilityInterval(CapabilityMetric Metric, double Lower, double Upper, double Confidence);

public sealed record AttributeCapabilityRow(SpcChart Chart, double Estimate, double Lower, double Upper, double Demanded, bool Pass);

public sealed record SpcLimitRow(SpcChart Chart, int Index, Instant At, double Value, double Center, double Sigma, double Lower, double Upper);

public sealed record SpcViolation(SpcChart Chart, SpcRule Rule, int Start, int End, double Excursion);

public sealed record DriftRow(double Intercept, double Slope);

public sealed record AutocorrelationRow(int Lag, double Correlation);

public sealed record CapabilityDependence(Seq<AutocorrelationRow> Lags, double EffectiveSampleSize);

// Keyed by the chain TERM, so the simulated share and the analytic share on `ChainReceipt.Contributions` rank the
// same names and a documentation-plane row reads either without a second key space.
public sealed record StackContribution(string Term, double Share, double SigmaMm, double TighteningFactor);

// The history projection a downstream demand consumes: the achievable band, the grade and index that qualified
// it, and the effective sample size the projection rests on.
public sealed record AchievableTolerance(Length Width, ItGrade Grade, double Cpk, double EffectiveSampleSize);

// BOTH readings of one stack: `Analytic` is the chain's own closed-form combination under its declared method and
// `Arithmetic` the worst-case bound over the same terms, beside the correlated simulation's moments, tail, and
// covariance shares. A consumer comparing the statistical answer against the arithmetic one reads one receipt
// rather than re-evaluating a fold of its own.
public sealed record StackupReceipt(
    ChainReceipt Analytic,
    ChainReceipt Arithmetic,
    double MeanMm,
    double SigmaMm,
    double TailMm,
    int RandomSeed,
    int FactorCount,
    Seq<StackContribution> Contributions,
    bool Pass) {
    public ContentKey Source => Analytic.Source;
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
    Option<ProcedureReceipt> Procedure,
    Option<StackupReceipt> Stackup,
    bool Controlled,
    CapabilityVerdict Verdict,
    Instant At);

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
    // ONE refusal mint for the page. `Inadmissible` is what a `[ValidationError<FabricationFault>]` owner's hook
    // seats in its slot, and `Refusal` is the same value read as the rail's `Error` — the band derives `Expected`,
    // so a generated admission and a fold gate answer under one taxonomy and one locus spelling.
    internal static FabricationFault Inadmissible(string locus) =>
        new FabricationFault.PolicyInadmissible(FabConcern.Spec, $"capability:{locus}");

    internal static Error Refusal(string locus) => Inadmissible(locus);

    private static readonly Error UncontrolledProcess = Refusal("uncontrolled-process");
    private static readonly Error UnqualifiedProcedure = Refusal("unqualified-procedure");
    private static readonly Error UnsuitableMeasurement = Refusal("unsuitable-measurement");
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
                .Admit(row.Cpk, row.DemandedCpk, grade.Number, row.ProcedureQualified, row.MeasurementSystemSuitable)
                .Bind(verdict => (Check(row.Controlled, UncontrolledProcess),
                        Check(row.ProcedureQualified, UnqualifiedProcedure),
                        Check(row.MeasurementSystemSuitable, UnsuitableMeasurement),
                        Check(verdict.Pass && row.Cpk >= row.DemandedCpk,
                            new FabricationFault.CapabilityShortfall(identity.Process, row.Cpk, row.DemandedCpk).ToError()))
                    .Apply((_, _, _, _) => verdict)
                    .As()
                    .ToFin()));

    // CapabilityHistory selection returns the qualifying row's measured band BESIDE the evidence behind it, so a
    // consumer grading its own confidence reads the effective sample size that earned the projection rather than
    // assigning a constant trust to the word history.
    public static Option<AchievableTolerance> Achievable(
        CapabilityIdentity identity, Instant at, Seq<CapabilityHistory> history) =>
        history.Filter(row => row.Identity == identity && row.ValidFrom <= at && at < row.ValidUntil && row.Controlled
                && row.ProcedureQualified && row.MeasurementSystemSuitable && row.Cpk >= row.DemandedCpk)
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
        from stackup in tolerance.Stackup.Traverse(Stackup).As()
        let rows = Rows(moment, Some(fitted), tolerance)
        let dependence = Dependence(series.ResidualMm, tolerance.Control.MaximumAutocorrelationLag)
        let limits = VariableLimits(series, moment, tolerance)
        let violations = Violations(limits)
        let drift = Drift(series.ResidualMm)
        let controlled = violations.IsEmpty
            && dependence.Lags.ForAll(row => Math.Abs(row.Correlation) <= tolerance.Control.MaximumAbsoluteAutocorrelation)
            && Math.Abs(drift.Slope) <= tolerance.Control.MaximumAbsoluteDriftPerSample
        let procedureQualified = ProcedureQualified(tolerance.Identity.Process, tolerance.Procedure)
        let cpk = rows.Find(static row => row.Metric == CapabilityMetric.Cpk).Map(static row => row.Value).IfNone(0.0)
        from verdict in CapabilityVerdict.Admit(
            cpk,
            tolerance.DemandedCpk,
            tolerance.Grade.Number,
            procedureQualified,
            tolerance.Measurement.Suitable)
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
            controlled,
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
            tolerance.Grade.Number,
            procedureQualified,
            tolerance.Measurement.Suitable)
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
            violations.IsEmpty,
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
            groups.Map(static group => group.Fold(0.0, static (sum, value) => sum + value) / group.Count).ToArr(),
            subgroupSize == 1
                ? walk.Skip(1).Zip(walk, static (next, prior) => Math.Abs(next - prior)).ToArr()
                : groups.Map(static group => group.Max(double.NegativeInfinity) - group.Min(double.PositiveInfinity)).ToArr(),
            groups.Map(SampleSigma).ToArr());

    private static Fin<CapabilityMoment> Moment(CapabilitySeries series, CapabilityTolerance tolerance) =>
        from stat in Stat.Of(series.ResidualMm.ToSeq(), Op.Of(name: "capability:residual"))
        let stamped = stat with {
            Context = StatContext.Tolerance(
                SpecHalfBand(tolerance, stat.Mean),
                stat.Minimum,
                stat.Maximum),
        }
        from accepted in CapabilityOp.AcceptValue(value: stamped)
        select new CapabilityMoment(
            accepted.Mean,
            tolerance.Control.SubgroupSize == 1
                ? (series.Ranges.Fold(0.0, static (sum, value) => sum + value) / series.Ranges.Count)
                    / ControlConstant.Get(2).RangeMean
                : (series.Sigmas.Fold(0.0, static (sum, value) => sum + value) / series.Sigmas.Count)
                    / C4(tolerance.Control.SubgroupSize),
            Math.Sqrt(accepted.Variance),
            accepted.Minimum,
            accepted.Maximum);

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
    internal static Option<double> Quantile(
        IContinuousDistribution distribution, double probability, DistributionPolicy policy) =>
        double.Max(distribution.StdDev, double.Epsilon) is var spread
            && double.Max(distribution.Minimum, distribution.Mean - (policy.BracketSigma * spread)) is var lower
            && double.Min(distribution.Maximum, distribution.Mean + (policy.BracketSigma * spread)) is var upper
            && double.IsFinite(lower) && double.IsFinite(upper) && upper > lower
            && Brent.TryFindRoot(value => distribution.CumulativeDistribution(value) - probability,
                lower, upper, policy.RootAccuracy, policy.RootIterations, out double root)
                    ? Some(root)
                    : None;

    internal static Option<CapabilitySpread> QuantileSpread(DistributionParameters parameters, CapabilityTolerance tolerance) =>
        parameters.Valid && parameters.FiniteMoments
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
            .Map(value => new CapabilityRow(metric, value, tolerance.DemandedCpk, value >= tolerance.DemandedCpk)));

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
        double width = tolerance.Control.SigmaWidth;
        double meanSigma = moment.WithinSigma / Math.Sqrt(subgroupSize);
        ControlConstant rangeConstant = ControlConstant.Nearest(subgroupSize);
        double rangeCenter = series.Ranges.Fold(0.0, static (sum, value) => sum + value) / series.Ranges.Count;
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
                double next = (tolerance.Control.EwmaWeight * value) + ((1.0 - tolerance.Control.EwmaWeight) * state.Item1);
                return (next, state.Item2.Add(next));
            });
        (double positive, double negative, Arr<double> cusum) = series.ResidualMm.Fold(
            (0.0, 0.0, Arr<double>.Empty),
            (state, value) => {
                double standardized = (value - moment.Mean) / double.Max(moment.WithinSigma, double.Epsilon);
                double nextPositive = double.Max(0.0, state.Item1 + standardized - tolerance.Control.CusumSlackSigma);
                double nextNegative = double.Min(0.0, state.Item2 + standardized + tolerance.Control.CusumSlackSigma);
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
                    tolerance.Control.EwmaWeight / (2.0 - tolerance.Control.EwmaWeight)
                    * (1.0 - Math.Pow(1.0 - tolerance.Control.EwmaWeight, 2.0 * (index + 1)))),
                width)).ToSeq()
            + Points(SpcChart.Cusum, cusum, 0.0, tolerance.Control.CusumDecisionSigma, tolerance.At, width: 1.0);
    }

    private static Seq<SpcLimitRow> AttributeLimits(
        Seq<AttributeSample> samples, AttributeCohort cohort, CapabilityTolerance tolerance) {
        double width = tolerance.Control.SigmaWidth;
        return samples.Map((sample, index) => Seq(
                Some(Point(SpcChart.P, index, sample.At, (double)sample.Nonconforming / sample.Inspected, cohort.P,
                    Math.Sqrt(cohort.P * (1.0 - cohort.P) / sample.Inspected), width)),
                cohort.FixedInspected
                    ? Some(Point(SpcChart.Np, index, sample.At, sample.Nonconforming, cohort.P * sample.Inspected,
                        Math.Sqrt(sample.Inspected * cohort.P * (1.0 - cohort.P)), width))
                    : None,
                cohort.FixedOpportunities
                    ? Some(Point(SpcChart.C, index, sample.At, sample.Defects, cohort.C, Math.Sqrt(cohort.C), width))
                    : None,
                Some(Point(SpcChart.U, index, sample.At, (double)sample.Defects / sample.Opportunities, cohort.U,
                    Math.Sqrt(cohort.U / sample.Opportunities), width))))
            .Bind(identity);
    }

    private static Fin<Seq<AttributeCapabilityRow>> AttributeRows(AttributeCohort cohort, CapabilityTolerance tolerance) {
        double alpha = 1.0 - tolerance.ConfidenceValue;
        double meanInspected = cohort.MeanInspected;
        double meanOpportunities = cohort.MeanOpportunities;
        return from pLower in BetaQuantile(cohort.Nonconforming + 0.5, cohort.Inspected - cohort.Nonconforming + 0.5,
                   alpha / 2.0, tolerance.Distribution)
               from pUpper in BetaQuantile(cohort.Nonconforming + 0.5, cohort.Inspected - cohort.Nonconforming + 0.5,
                   1.0 - (alpha / 2.0), tolerance.Distribution)
               from countLower in Finite(cohort.Defects == 0 ? 0.0 : MathNet.Numerics.Distributions.Gamma.InvCDF(cohort.Defects, 1.0, alpha / 2.0))
               from countUpper in Finite(MathNet.Numerics.Distributions.Gamma.InvCDF(cohort.Defects + 1.0, 1.0, 1.0 - (alpha / 2.0)))
               select Seq(
                       Some(new AttributeCapabilityRow(SpcChart.P, cohort.P, pLower, pUpper, tolerance.TailProbabilityValue,
                           pUpper <= tolerance.TailProbabilityValue)),
                       cohort.FixedInspected
                           ? Some(new AttributeCapabilityRow(SpcChart.Np, cohort.P * meanInspected, pLower * meanInspected, pUpper * meanInspected,
                               tolerance.TailProbabilityValue * meanInspected, pUpper <= tolerance.TailProbabilityValue))
                           : None,
                       cohort.FixedOpportunities
                           ? Some(new AttributeCapabilityRow(SpcChart.C, cohort.U * meanOpportunities,
                               countLower / cohort.Samples, countUpper / cohort.Samples,
                               tolerance.TailProbabilityValue * meanOpportunities, countUpper / cohort.Opportunities <= tolerance.TailProbabilityValue))
                           : None,
                       Some(new AttributeCapabilityRow(SpcChart.U, cohort.U, countLower / cohort.Opportunities, countUpper / cohort.Opportunities,
                           tolerance.TailProbabilityValue, countUpper / cohort.Opportunities <= tolerance.TailProbabilityValue)))
                   .Bind(identity);
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
        SigmaBand(sigmas, sigmas.Fold(0.0, static (sum, value) => sum + value) / sigmas.Count,
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

    private static Fin<StackupReceipt> Stackup(StackupPolicy policy) {
        // `StackupReceipt.RandomSeed` is published as replay evidence, and only the branch's one splitmix64 owner
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
        double mean = TensorPrimitives.Average(trials);
        double sigma = TensorPrimitives.StdDev(trials);
        trials.Sort();
        double probability = policy.TailProbability.DecimalFractions;
        double tail = double.Max(Math.Abs(trials[(int)Math.Floor((policy.Trials - 1) * probability)]),
            Math.Abs(trials[(int)Math.Ceiling((policy.Trials - 1) * (1.0 - probability))]));
        // The simulation is CORRELATED — every contributor loads the same shared factors — so the share a term
        // owns is its covariance with the assembled response, not its independent variance fraction. An
        // independence share under a correlated model attributes a shared factor's spread to whichever term
        // happens to carry the largest loading and understates every term that moves with it.
        double[] covariance = Covariances(policy, spread, independent, shared, trials, mean);
        double variance = double.Max(TensorPrimitives.Sum<double>(covariance), double.Epsilon);
        // The analytic readings are the CHAIN's own, evaluated over the same terms: the declared method beside the
        // arithmetic bound. A worst-case fold re-spelled here would be a second algebra over one term roster.
        ChainReceipt analytic = policy.Chain.Evaluate();
        StackupReceipt receipt = new(
            analytic,
            policy.Chain.Evaluate(StackMethod.WorstCase),
            mean,
            sigma,
            tail,
            policy.RandomSeed,
            factors,
            Contributions(policy, spread, covariance, variance, tail),
            tail <= analytic.BoundMm);
        // Pass is a VERDICT, not a fault. A stack that exceeds its bound is exactly the answer the study was run
        // to obtain, and its contribution ranking is the evidence naming the term worth tightening — refusing the
        // receipt destroys that evidence and forces every consumer to re-run the simulation to see it. The
        // consuming gate decides what an exceeded bound means for ITS decision.
        return Fin.Succ(receipt);
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

    // ProcedureReceipt.Qualified is the owner's own compliance verdict over every row; a joined process without one is unqualified.
    private static bool ProcedureQualified(ProcessKind process, Option<ProcedureReceipt> procedure) =>
        procedure.Match(
            Some: receipt => receipt.Process == process && receipt.Qualified,
            None: () => process.Modality.Class != ModalityClass.Joined);

    private static Seq<SpcLimitRow> Points(SpcChart chart, Arr<double> values, double center, double sigma, Instant at, double width) =>
        toSeq(values).Map((value, index) => Point(chart, index, at, value, center, sigma, width));

    private static SpcLimitRow Point(SpcChart chart, int index, Instant at, double value, double center, double sigma, double width) =>
        new(chart, index, at, value, center, sigma, chart.Attribute ? double.Max(0.0, center - (width * sigma)) : center - (width * sigma), center + (width * sigma));

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

    private static double SampleSigma(Arr<double> values) {
        if (values.Count < 2)
            return 0.0;
        double mean = values.Fold(0.0, static (sum, value) => sum + value) / values.Count;
        return Math.Sqrt(values.Fold(0.0, (sum, value) => sum + Math.Pow(value - mean, 2.0)) / (values.Count - 1));
    }

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
[ValidationError<FabricationFault>]
public sealed partial class CapabilityHistory {
    public CapabilityIdentity Identity { get; }
    public ItGrade Grade { get; }
    public double Cpk { get; }
    public double DemandedCpk { get; }
    public bool Controlled { get; }
    public bool ProcedureQualified { get; }
    public bool MeasurementSystemSuitable { get; }
    public double EffectiveSampleSize { get; }
    public Instant ValidFrom { get; }
    public Instant ValidUntil { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref FabricationFault? validationError,
        ref CapabilityIdentity identity,
        ref ItGrade grade,
        ref double cpk,
        ref double demandedCpk,
        ref bool controlled,
        ref bool procedureQualified,
        ref bool measurementSystemSuitable,
        ref double effectiveSampleSize,
        ref Instant validFrom,
        ref Instant validUntil) =>
        validationError = identity is not null && grade is not null && grade.Number >= 1
            && identity.Feature == grade.Diameter && double.IsFinite(cpk)
            && double.IsFinite(demandedCpk) && demandedCpk > 0.0 && double.IsFinite(effectiveSampleSize) && effectiveSampleSize >= 2.0
            && validFrom != default && validUntil > validFrom
                ? null
                : Capability.Inadmissible("history");

    public static Fin<CapabilityHistory> From(CapabilityReport report, Instant validUntil) =>
        Validate(
            report.Identity,
            report.Grade,
            report.Rows.Find(static row => row.Metric == CapabilityMetric.Cpk).Map(static row => row.Value).IfNone(report.Verdict.Cpk),
            report.Verdict.DemandedCpk,
            report.Controlled,
            report.Verdict.ProcedureQualified,
            report.Verdict.MeasurementSystemSuitable,
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
                && row.Grade.Diameter == grade.Diameter && row.ValidFrom <= at && at < row.ValidUntil)
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
