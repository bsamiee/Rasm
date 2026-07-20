# [RASM_FABRICATION_CAPABILITY]

`Capability` owns characteristic-scoped process evidence from admission through control-state, distribution, measurement-system, tolerance-stack, history, and plan-gate projection. Variable and attribute studies share one assessment rail, and every report preserves the evidence that makes its verdict reproducible.

`CapabilityIdentity`, `ToleranceChain`, `ProcedureReceipt`, `Stat`, `CapabilityVerdict`, and `FabricationFault` remain the seam owners. `CapabilityReport` is the terminal specification receipt, while `CapabilityHistory` carries its validity-bounded ledger projection into `Gate` and `Achievable`.

## [01]-[INDEX]

- [02]-[CAPABILITY]: `Capability.Assess` admits either study case, derives control and capability evidence, and emits one history-projectable report.

## [02]-[CAPABILITY]

- Owner: `CapabilityStudy` closes variable and attribute evidence; `CapabilityTolerance` carries the characteristic demand, control policy, measurement study, optional procedure, and optional factor-model stackup; `CapabilityReport` conserves every derived decision.
- Cases: `DistributionParameters` carries fitted continuous families and `DistributionFamily` seeds them by support; `MeasurementEvidence` carries variable gage and attribute agreement studies; `SpcChart`, `SpcRuleClass`, `SpcRule`, and `ControlConstant` carry generated control policy; `CapabilityMetric` derives sided short- and long-term indices from row data.
- Method: `CapabilityMethod` closes moment and ISO 22514-4 percentile estimation over one `CapabilitySpread`, so the fitted distribution decides the non-normal index instead of decorating the report; `CapabilitySide` owns the one sided-index algebra both methods enter.
- Entry: `Capability.Assess`, `Capability.Gate`, and `Capability.Achievable` parameterize assessment, ledger selection, and tolerance projection without ambient state.
- Auto: `Validation` accumulates independent request and gate faults under distinct errors; `Stat.Of` owns variable moments; `Distance.Pearson` derives the autocorrelation spectrum; `Fit.Line` derives drift; `Generate.LinearSpacedMap` generates Student candidates; one bracketed `Brent.TryFindRoot` serves every quantile the MathNet interfaces expose no generic inverse for; `SpecialFunctions.GammaLn` and `Gamma` own distribution functions; calibrated `ControlConstant` rows and the exact `C4` gamma ratio derive subgroup limits; `Traverse`, `Choose`, and `Fold` own collection flow.
- Signals: `SpcChart.Admits` grades each rule class per chart, so every chart signals on its own control band while zone and pattern ladders stay on symmetric equal-variance charts; band normalization makes a breach honor the configured sigma width and the clamped attribute floor.
- Receipt: `CapabilityReport` carries moment and percentile indices or attribute rates, per-metric confidence intervals, pointwise control limits, rule windows, fitted distribution, effective sample size, measurement evidence, procedure evidence, optional stackup with per-contributor variance shares, control state, and the admitted `CapabilityVerdict`. `FabricationFact.Capability.Of` projects the index rows and violation count onto `rasm.fabrication.capability.index` and `rasm.fabrication.capability.violations` through `Process/telemetry#FACT_PROJECTION` as kind `capability`.
- Packages: MathNet.Numerics owns fitted distributions, roots, regression, correlation, batch sampling, and generated parameter maps; `System.Numerics.Tensors` owns numeric reductions; CommunityToolkit.HighPerformance owns pooled and partitioned trial execution; UnitsNet owns specification lengths, achievable tolerance, and probability ratios; `ToolEvidence` carries MTConnect operating state through the canonical process boundary; Thinktecture and LanguageExt own generated values and the accumulated rail.
- Growth: a capability index is one `CapabilityMetric` row; a distribution is one `DistributionParameters` case with one `DistributionFamily` seed row; a control rule is one `SpcRule` row carrying its `SpcRuleClass`; a study modality is one `CapabilityStudy` case folded by `Assess`.
- Boundary: `CapabilityIdentity` carries the `DiameterBand` its study measured, so `Gate` and `Achievable` resolve through one identity and no row authorizes a size it never observed. `CapabilityHistory` is input-carried evidence; enrollment and persistence remain orchestration effects. `CapabilityReport` never enters `FabricationResult`, and only `CapabilityVerdict` crosses the plan seam.

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
    public static readonly CapabilityMethod Moment = new("moment", static (scale, moment, _, _) =>
        Some(Symmetric(moment.Mean, Capability.SigmaSpan * scale.Sigma(moment))));
    public static readonly CapabilityMethod Percentile = new("percentile", static (_, _, fitted, tail) =>
        from row in fitted
        from spread in Capability.QuantileSpread(row.Parameters, tail)
        select spread);

    public Func<CapabilityScale, CapabilityMoment, Option<CapabilityDistribution>, double, Option<CapabilitySpread>> Of { get; }

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
        from spread in Method.Of(Scale, moment, fitted, tolerance.TailProbabilityValue)
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

    // A limit breach reads the row's own control band, so a configured sigma width never disagrees with a literal zone.
    private static SpcRule Limit(string key) =>
        new(key, SpcRuleClass.Limit, window: 1, static values => values.Exists(static value => Math.Abs(value) > 1.0));

    private static SpcRule Zone(string key, int window, int minimum, double zone) =>
        new(key, SpcRuleClass.Zone, window,
            values => int.Max(values.Count(value => value > zone), values.Count(value => value < -zone)) >= minimum);

    private static SpcRule Pattern(string key, int window, Func<Arr<double>, bool> breach) =>
        new(key, SpcRuleClass.Pattern, window, breach);

    private static Arr<int> Steps(Arr<double> values) =>
        values.Skip(1).Zip(values).Map(static pair => Math.Sign(pair.First - pair.Second)).ToArr();

    private static bool Trending(Arr<int> steps) =>
        steps.ForAll(static step => step > 0) || steps.ForAll(static step => step < 0);

    private static bool Alternating(Arr<int> steps) =>
        steps.ForAll(static step => step != 0)
        && steps.Zip(steps.Skip(1)).ForAll(static pair => pair.First == -pair.Second);
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

// --- [ADMISSION] ----------------------------------------------------------------------------------------------------------------------------------
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
        validationError = process is not null && characteristic != 0 && feature is not null
            && machine != 0 && material != 0 && tool != 0 && toolState is not null && setup != 0
            ? null
            : new ValidationError(message: "capability:identity");
}

[ComplexValueObject]
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
        ref ValidationError? validationError,
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
                : new ValidationError(message: "capability:variable-msa");
}

[ComplexValueObject]
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
        ref ValidationError? validationError,
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
                : new ValidationError(message: "capability:attribute-msa");
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
public sealed partial class DistributionPolicy {
    public int CandidateCount { get; }
    public double StudentFreedomMinimum { get; }
    public double StudentFreedomMaximum { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref int candidateCount,
        ref double studentFreedomMinimum,
        ref double studentFreedomMaximum) =>
        validationError = candidateCount >= 2 && studentFreedomMinimum > 2.0 && studentFreedomMaximum > studentFreedomMinimum
            ? null
            : new ValidationError(message: "capability:distribution-policy");
}

[ComplexValueObject]
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
        ref ValidationError? validationError,
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
                : new ValidationError(message: "capability:control-policy");
}

[ComplexValueObject]
public sealed partial class AttributeSample {
    public int Inspected { get; }
    public int Nonconforming { get; }
    public int Defects { get; }
    public int Opportunities { get; }
    public Instant At { get; }

    public static (
        long Inspected,
        long Nonconforming,
        long Opportunities,
        long Defects,
        double P,
        double U,
        double C,
        bool FixedInspected,
        bool FixedOpportunities) Cohort(Seq<AttributeSample> samples) {
        long inspected = samples.Fold(0L, static (sum, row) => sum + row.Inspected);
        long nonconforming = samples.Fold(0L, static (sum, row) => sum + row.Nonconforming);
        long opportunities = samples.Fold(0L, static (sum, row) => sum + row.Opportunities);
        long defects = samples.Fold(0L, static (sum, row) => sum + row.Defects);
        return (
            inspected,
            nonconforming,
            opportunities,
            defects,
            (double)nonconforming / inspected,
            (double)defects / opportunities,
            (double)defects / samples.Count,
            samples.Map(static sample => sample.Inspected).Distinct().Count == 1,
            samples.Map(static sample => sample.Opportunities).Distinct().Count == 1);
    }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref int inspected,
        ref int nonconforming,
        ref int defects,
        ref int opportunities,
        ref Instant at) =>
        validationError = inspected > 0 && nonconforming is >= 0 && nonconforming <= inspected
            && defects >= 0 && defects <= opportunities && opportunities >= inspected && at != default
            ? null
            : new ValidationError(message: "capability:attribute-sample");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CapabilityStudy {
    private CapabilityStudy() { }

    public sealed record Variables(Seq<ResidualSample> Samples) : CapabilityStudy;
    public sealed record Attributes(Seq<AttributeSample> Samples) : CapabilityStudy;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DistributionParameters {
    private DistributionParameters() { }

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
        static (moment, sigma, _) => WeibullShape(sigma / moment.Mean)
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

    private static Option<double> WeibullShape(double coefficient) =>
        Brent.TryFindRoot(
            shape => (SpecialFunctions.Gamma(1.0 + (2.0 / shape)) / Math.Pow(SpecialFunctions.Gamma(1.0 + (1.0 / shape)), 2.0))
                - 1.0 - (coefficient * coefficient),
            Capability.ShapeLowerBound,
            Capability.ShapeUpperBound,
            Capability.RootAccuracy,
            Capability.RootIterations,
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

[ComplexValueObject]
public sealed partial class StackContributor {
    public UInt128 Key { get; }
    public DistributionParameters Distribution { get; }
    public double BiasMm { get; }
    public double ScaleMm { get; }
    public double Sensitivity { get; }
    public Arr<double> FactorLoadings { get; }

    public double IndependentLoading => Math.Sqrt(1.0 - FactorLoadings.Fold(0.0, static (sum, value) => sum + (value * value)));

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref UInt128 key,
        ref DistributionParameters distribution,
        ref double biasMm,
        ref double scaleMm,
        ref double sensitivity,
        ref Arr<double> factorLoadings) =>
        validationError = key != 0 && distribution is not null && distribution.Valid && distribution.FiniteMoments
            && double.IsFinite(biasMm) && double.IsFinite(scaleMm) && scaleMm > 0.0
            && double.IsFinite(sensitivity) && sensitivity != 0.0 && factorLoadings.ForAll(static value => double.IsFinite(value))
            && factorLoadings.Fold(0.0, static (sum, value) => sum + (value * value)) <= 1.0
                ? null
                : new ValidationError(message: "capability:stack-contributor");
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
        validationError = chain is not null && !contributors.IsEmpty && trials >= 2
            && tailProbability.DecimalFractions is > 0.0 and < 0.5
            && trials * tailProbability.DecimalFractions >= 1.0
            && contributors.Map(static row => row.Key).Distinct().Count == contributors.Count
            && contributors.Map(static row => row.FactorLoadings.Count).Distinct().Count <= 1
                ? null
                : new ValidationError(message: "capability:stack-policy");
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
            && tailProbability.DecimalFractions is > 0.0 and < 0.5 && confidence.DecimalFractions is > 0.0 and < 1.0
            && distribution is not null && control is not null && measurement is not null && procedure.ForAll(static value => value is not null)
            && stackup.ForAll(static value => value is not null) && at != default
                ? null
                : new ValidationError(message: "capability:tolerance");
    }
}

// --- [RECEIPTS] -----------------------------------------------------------------------------------------------------------------------------------
public sealed record CapabilitySeries(
    Arr<double> ResidualMm,
    Seq<Arr<double>> Groups,
    Arr<double> Means,
    Arr<double> Ranges,
    Arr<double> Sigmas);

public sealed record CapabilityMoment(double Mean, double WithinSigma, double OverallSigma, double Minimum, double Maximum);
public sealed record CapabilitySpread(double Center, double Lower, double Upper);
public sealed record CapabilityDistribution(DistributionParameters Parameters, double FitError);
public sealed record CapabilityRow(CapabilityMetric Metric, double Value, double Demanded, bool Pass);
public sealed record CapabilityInterval(CapabilityMetric Metric, double Lower, double Upper, double Confidence);
public sealed record AttributeCapabilityRow(SpcChart Chart, double Estimate, double Lower, double Upper, double Demanded, bool Pass);
public sealed record SpcLimitRow(SpcChart Chart, int Index, Instant At, double Value, double Center, double Sigma, double Lower, double Upper);
public sealed record SpcViolation(SpcChart Chart, SpcRule Rule, int Start, int End, double Excursion);
public sealed record DriftRow(double Intercept, double Slope);
public sealed record AutocorrelationRow(int Lag, double Correlation);
public sealed record CapabilityDependence(Seq<AutocorrelationRow> Lags, double EffectiveSampleSize);

public sealed record StackContribution(UInt128 Key, double Share, double SigmaMm, double TighteningFactor);

public sealed record StackupReceipt(
    ToleranceChain Chain,
    double MeanMm,
    double SigmaMm,
    double TailMm,
    double RssMm,
    double WorstCaseMm,
    int RandomSeed,
    int FactorCount,
    Seq<StackContribution> Contributions,
    bool Pass) {
    public Option<StackContribution> Dominant => Contributions.OrderByDescending(static row => row.Share).Head;
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
    internal const double SigmaSpan = 3.0;
    internal const double ShapeLowerBound = 0.1;
    internal const double ShapeUpperBound = 100.0;
    internal const double RootAccuracy = 1e-10;
    internal const int RootIterations = 100;
    internal const double BracketSigma = 12.0;
    internal const int FitSeed = 0;

    internal static readonly Op CapabilityOp = Op.Of(name: "fabrication:capability");
    private static readonly Error UncontrolledProcess = CapabilityOp.InvalidResult();
    private static readonly Error UnqualifiedProcedure = CapabilityOp.InvalidResult();
    private static readonly Error UnsuitableMeasurement = CapabilityOp.InvalidResult();
    private static readonly Error MissingHistory = CapabilityOp.InvalidInput();
    private static readonly Error StudyMismatch = CapabilityOp.InvalidInput();
    private static readonly Error UnderpoweredStudy = CapabilityOp.InvalidInput();
    private static readonly Error StackupUnsupported = CapabilityOp.InvalidInput();
    private static readonly Error ProcedureNotYetIssued = CapabilityOp.InvalidInput();

    public static Fin<CapabilityReport> Assess(CapabilityStudy study, CapabilityTolerance tolerance) =>
        from _ in Admit(study, tolerance)
        from report in study.Switch(
            state: tolerance,
            variables: static (demand, evidence) => Variables(evidence.Samples, demand),
            attributes: static (demand, evidence) => Attributes(evidence.Samples, demand))
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
                            FabricationFault.CapabilityShortfall(identity.Process, row.Cpk, row.DemandedCpk)))
                    .Apply((_, _, _, _) => verdict)
                    .As()
                    .ToFin()));

    // CapabilityHistory selection returns the qualifying row's measured band, so caller-supplied size cannot borrow foreign evidence.
    public static Option<Length> Achievable(CapabilityIdentity identity, Instant at, Seq<CapabilityHistory> history) =>
        history.Filter(row => row.Identity == identity && row.ValidFrom <= at && at < row.ValidUntil && row.Controlled
                && row.ProcedureQualified && row.MeasurementSystemSuitable && row.Cpk >= row.DemandedCpk)
            .OrderBy(static row => row.Grade.Number)
            .Head
            .Map(static row => Length.FromMillimeters(row.Grade.ToleranceMillimeters));

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
        from _ in guard(!samples.IsEmpty, CapabilityOp.InvalidInput()).ToFin()
        let limits = AttributeLimits(samples, tolerance)
        let violations = Violations(limits)
        from rows in AttributeRows(samples, tolerance)
        let equivalentCpk = rows.Filter(static row => row.Chart == SpcChart.P || row.Chart == SpcChart.U)
            .Map(static row => MathNet.Numerics.Distributions.Normal.InvCDF(
            0.0,
            1.0,
            double.Clamp(1.0 - row.Upper, double.Epsilon, 1.0 - double.Epsilon)) / 3.0).Min()
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
            new CapabilityDependence(Seq<AutocorrelationRow>(), samples.Count),
            new DriftRow(0.0, 0.0),
            tolerance.Measurement,
            tolerance.Procedure,
            None,
            violations.IsEmpty,
            verdict,
            tolerance.At);

    private static Fin<CapabilitySeries> Series(Seq<ResidualSample> samples, int subgroupSize) =>
        from _1 in guard(!samples.IsEmpty && subgroupSize >= 1 && subgroupSize <= samples.Count, CapabilityOp.InvalidInput()).ToFin()
        from _2 in guard(subgroupSize == 1 ? samples.Count >= 2 : samples.Count % subgroupSize == 0, CapabilityOp.InvalidInput()).ToFin()
        let residual = samples.Map(static sample => sample.Distance).ToArr()
        let groups = subgroupSize == 1
            ? residual.Map(static value => Arr.create(value)).ToSeq()
            : toSeq(Enumerable.Range(0, residual.Count / subgroupSize)).Map(index => residual.Skip(index * subgroupSize).Take(subgroupSize).ToArr())
        select new CapabilitySeries(
            residual,
            groups,
            groups.Map(static group => group.Average()).ToArr(),
            subgroupSize == 1
                ? residual.Skip(1).Zip(residual).Map(static pair => Math.Abs(pair.First - pair.Second)).ToArr()
                : groups.Map(static group => group.Max() - group.Min()).ToArr(),
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
                ? series.Ranges.Average() / ControlConstant.Get(2).RangeMean
                : series.Sigmas.Average() / C4(tolerance.Control.SubgroupSize),
            Math.Sqrt(accepted.Variance),
            accepted.Minimum,
            accepted.Maximum);

    private static CapabilityDistribution FitDistribution(Arr<double> values, CapabilityMoment moment, DistributionPolicy policy) =>
        toSeq(DistributionFamily.Items)
            .Bind(family => family.Candidates(moment, double.Max(moment.OverallSigma, double.Epsilon), policy))
            .Map(parameters => new CapabilityDistribution(parameters, FitError(parameters, values)))
            .OrderBy(static candidate => candidate.FitError)
            .Head
            .IfNone(new CapabilityDistribution(
                new DistributionParameters.Normal(moment.Mean, double.Max(moment.OverallSigma, double.Epsilon)),
                double.PositiveInfinity));

    // Kolmogorov-Smirnov supremum against the mid-rank plotting position; the seeded generator never enters a CDF read.
    private static double FitError(DistributionParameters parameters, Arr<double> values) =>
        Supremum(parameters.Create(new Random(FitSeed)), values.Order().ToArr());

    private static double Supremum(IContinuousDistribution fitted, Arr<double> ordered) =>
        ordered.Map((value, index) => Math.Abs(fitted.CumulativeDistribution(value) - ((index + 0.5) / ordered.Count))).Max();

    // No MathNet interface exposes a generic inverse CDF, so one bracketed root-find serves every admitted family.
    internal static Option<double> Quantile(IContinuousDistribution distribution, double probability) =>
        double.Max(distribution.StdDev, double.Epsilon) is var spread
            && double.Max(distribution.Minimum, distribution.Mean - (BracketSigma * spread)) is var lower
            && double.Min(distribution.Maximum, distribution.Mean + (BracketSigma * spread)) is var upper
            && double.IsFinite(lower) && double.IsFinite(upper) && upper > lower
            && Brent.TryFindRoot(value => distribution.CumulativeDistribution(value) - probability,
                lower, upper, RootAccuracy, RootIterations, out double root)
                    ? Some(root)
                    : None;

    internal static Option<CapabilitySpread> QuantileSpread(DistributionParameters parameters, double tail) =>
        parameters.Valid && parameters.FiniteMoments
            ? Spread(parameters.Create(new Random(FitSeed)), tail)
            : None;

    private static Option<CapabilitySpread> Spread(IContinuousDistribution fitted, double tail) =>
        from median in Quantile(fitted, 0.5)
        from low in Quantile(fitted, tail)
        from high in Quantile(fitted, 1.0 - tail)
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
        double rangeCenter = series.Ranges.Average();
        double rangeSigma = SampleSigma(series.Ranges);
        double rangeLower = double.Max(0.0,
            rangeCenter * (rangeConstant.RangeMean - (width * rangeConstant.RangeSigma)) / rangeConstant.RangeMean);
        double rangeUpper = rangeCenter * (rangeConstant.RangeMean + (width * rangeConstant.RangeSigma)) / rangeConstant.RangeMean;
        Seq<SpcLimitRow> spread = subgroupSize <= ControlConstant.LargestSubgroup
            ? series.Ranges.Map((value, index) => new SpcLimitRow(
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
            + ewma.Map((value, index) => Point(
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

    private static Seq<SpcLimitRow> AttributeLimits(Seq<AttributeSample> samples, CapabilityTolerance tolerance) {
        var cohort = AttributeSample.Cohort(samples);
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

    private static Fin<Seq<AttributeCapabilityRow>> AttributeRows(Seq<AttributeSample> samples, CapabilityTolerance tolerance) {
        var cohort = AttributeSample.Cohort(samples);
        double alpha = 1.0 - tolerance.ConfidenceValue;
        double meanInspected = (double)cohort.Inspected / samples.Count;
        double meanOpportunities = (double)cohort.Opportunities / samples.Count;
        return from pLower in BetaQuantile(cohort.Nonconforming + 0.5, cohort.Inspected - cohort.Nonconforming + 0.5, alpha / 2.0)
               from pUpper in BetaQuantile(cohort.Nonconforming + 0.5, cohort.Inspected - cohort.Nonconforming + 0.5, 1.0 - (alpha / 2.0))
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
                           ? Some(new AttributeCapabilityRow(SpcChart.C, cohort.U * meanOpportunities, countLower / samples.Count, countUpper / samples.Count,
                               tolerance.TailProbabilityValue * meanOpportunities, countUpper / cohort.Opportunities <= tolerance.TailProbabilityValue))
                           : None,
                       Some(new AttributeCapabilityRow(SpcChart.U, cohort.U, countLower / cohort.Opportunities, countUpper / cohort.Opportunities,
                           tolerance.TailProbabilityValue, countUpper / cohort.Opportunities <= tolerance.TailProbabilityValue)))
                   .Bind(identity);
    }

    private static Seq<SpcViolation> Violations(Seq<SpcLimitRow> limits) =>
        from group in toSeq(limits.GroupBy(static row => row.Chart))
        let points = toSeq(group.OrderBy(static row => row.Index))
        let banded = points.Map(Banded).ToArr()
        let zoned = points.Map(Zoned).ToArr()
        from rule in toSeq(SpcRule.Items).Filter(group.Key.Admits)
        let series = rule.Class == SpcRuleClass.Limit ? banded : zoned
        from start in toSeq(Enumerable.Range(0, int.Max(0, series.Count - rule.Window + 1)))
        let window = series.Skip(start).Take(rule.Window).ToArr()
        where rule.Breach(window)
        select new SpcViolation(group.Key, rule, start, start + rule.Window - 1, window.Map(Math.Abs).Max());

    // Band normalization crosses +/-1 exactly at the row's own limit, so a configured sigma width and a clamped attribute floor both hold.
    private static double Banded(SpcLimitRow row) =>
        row.Value >= row.Center
            ? (row.Value - row.Center) / double.Max(row.Upper - row.Center, double.Epsilon)
            : -((row.Center - row.Value) / double.Max(row.Center - row.Lower, double.Epsilon));

    private static double Zoned(SpcLimitRow row) => (row.Value - row.Center) / double.Max(row.Sigma, double.Epsilon);

    private static Seq<SpcLimitRow> SigmaLimits(Arr<double> sigmas, double c4, double width, Instant at) =>
        SigmaBand(sigmas, sigmas.Average(), width * Math.Sqrt(1.0 - (c4 * c4)) / c4, at);

    private static Seq<SpcLimitRow> SigmaBand(Arr<double> sigmas, double center, double band, Instant at) =>
        sigmas.Map((value, index) => new SpcLimitRow(
            SpcChart.Sigma,
            index,
            at,
            value,
            center,
            SampleSigma(sigmas),
            double.Max(0.0, center * (1.0 - band)),
            center * (1.0 + band))).ToSeq();

    private static Fin<StackupReceipt> Stackup(StackupPolicy policy) {
        Random random = new(policy.RandomSeed);
        int factors = policy.Contributors.Head.Map(static row => row.FactorLoadings.Count).IfNone(0);
        Arr<IContinuousDistribution> distributions = policy.Contributors.Map(row => row.Distribution.Create(random)).ToArr();
        MathNet.Numerics.Distributions.Normal standard = new(0.0, 1.0, random);
        double[][] independent = policy.Contributors.Map((row, index) => {
            double[] samples = new double[policy.Trials];
            distributions[index].Samples(samples);
            TensorPrimitives.Subtract(samples, distributions[index].Mean, samples);
            TensorPrimitives.Divide(samples, double.Max(distributions[index].StdDev, double.Epsilon), samples);
            return samples;
        }).ToArray();
        double[][] shared = toSeq(Enumerable.Range(0, factors)).Map(_ => {
            double[] samples = new double[policy.Trials];
            standard.Samples(samples);
            return samples;
        }).ToArray();
        using MemoryOwner<double> owner = MemoryOwner<double>.Allocate(policy.Trials);
        ArraySegment<double> destination = owner.DangerousGetArray();
        StackupAction action = new(policy, independent, shared, destination.Array!, destination.Offset);
        ParallelHelper.For<StackupAction>(0, policy.Trials, in action);
        Span<double> trials = owner.Span[..policy.Trials];
        double mean = TensorPrimitives.Average(trials);
        double sigma = TensorPrimitives.StdDev(trials);
        trials.Sort();
        double probability = policy.TailProbability.DecimalFractions;
        double tail = double.Max(Math.Abs(trials[(int)Math.Floor((policy.Trials - 1) * probability)]),
            Math.Abs(trials[(int)Math.Ceiling((policy.Trials - 1) * (1.0 - probability))]));
        double[] sensitive = policy.Contributors.Map(static row => row.ScaleMm * row.Sensitivity).ToArray();
        double variance = double.Max(TensorPrimitives.SumOfSquares<double>(sensitive), double.Epsilon);
        double rss = Math.Sqrt(variance);
        double worst = policy.Contributors.Fold(0.0, static (sum, row) =>
            sum + Math.Abs(row.BiasMm * row.Sensitivity) + (SigmaSpan * row.ScaleMm * Math.Abs(row.Sensitivity)));
        StackupReceipt receipt = new(
            policy.Chain,
            mean,
            sigma,
            tail,
            rss,
            worst,
            policy.RandomSeed,
            factors,
            Contributions(policy, sensitive, variance, tail),
            tail <= policy.Chain.BoundMm);
        return receipt.Pass
            ? Fin.Succ(receipt)
            : Fin.Fail<StackupReceipt>(FabricationFault.StackupExceeded(
                new FaultSubject.Specification(policy.Chain.Source), tail, policy.Chain.BoundMm));
    }

    // Variance share plus the scale factor that brings the simulated tail inside the bound names the term worth tightening.
    private static Seq<StackContribution> Contributions(StackupPolicy policy, double[] sensitive, double variance, double tail) =>
        policy.Contributors.Map((row, index) =>
            new StackContribution(
                row.Key,
                sensitive[index] * sensitive[index] / variance,
                Math.Abs(sensitive[index]),
                tail <= policy.Chain.BoundMm ? 1.0 : policy.Chain.BoundMm / double.Max(tail, double.Epsilon)));

    private readonly struct StackupAction(
        StackupPolicy policy,
        double[][] independent,
        double[][] shared,
        double[] destination,
        int offset) : IAction {
        public void Invoke(int index) =>
            destination[offset + index] = policy.Contributors.Map((row, contributor) => {
                double common = row.FactorLoadings.Map((loading, factor) => loading * shared[factor][index]).Sum();
                double standardized = common + (row.IndependentLoading * independent[contributor][index]);
                return row.Sensitivity * (row.BiasMm + (row.ScaleMm * standardized));
            }).Sum();
    }

    // ProcedureReceipt.Qualified is the owner's own compliance verdict over every row; a joined process without one is unqualified.
    private static bool ProcedureQualified(ProcessKind process, Option<ProcedureReceipt> procedure) =>
        procedure.Match(
            Some: receipt => receipt.Process == process && receipt.Qualified,
            None: () => process.Modality.Class != ModalityClass.Joined);

    private static Seq<SpcLimitRow> Points(SpcChart chart, Arr<double> values, double center, double sigma, Instant at, double width) =>
        values.Map((value, index) => Point(chart, index, at, value, center, sigma, width)).ToSeq();

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
        double mean = values.Average();
        return values.Count < 2
            ? 0.0
            : Math.Sqrt(values.Fold(0.0, (sum, value) => sum + Math.Pow(value - mean, 2.0)) / (values.Count - 1));
    }

    private static double C4(int subgroupSize) =>
        Math.Exp(SpecialFunctions.GammaLn(subgroupSize / 2.0) - SpecialFunctions.GammaLn((subgroupSize - 1.0) / 2.0))
        / Math.Sqrt((subgroupSize - 1.0) / 2.0);

    private static Fin<double> BetaQuantile(double a, double b, double probability) =>
        Quantile(new MathNet.Numerics.Distributions.Beta(a, b), probability).ToFin(CapabilityOp.InvalidResult());

    private static Fin<double> Finite(double value) =>
        double.IsFinite(value)
            ? Fin.Succ(value)
            : Fin.Fail<double>(CapabilityOp.InvalidResult());

    private static DriftRow Drift(Arr<double> values) {
        double[] x = Generate.LinearSpaced(values.Count, 0.0, values.Count - 1.0);
        (double intercept, double slope) = Fit.Line(x, values.ToArray());
        return new DriftRow(intercept, slope);
    }
}

// --- [HISTORY] ------------------------------------------------------------------------------------------------------------------------------------
[ComplexValueObject]
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
        ref ValidationError? validationError,
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
                : new ValidationError(message: "capability:history");

    public static Fin<CapabilityHistory> From(CapabilityReport report, Instant validUntil) =>
        CapabilityHistory.Validate(
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
            out CapabilityHistory history) is { }
                ? Fin.Fail<CapabilityHistory>(Capability.CapabilityOp.InvalidInput())
                : Fin.Succ(history);

    // Grade name and diameter band both discriminate; the allowance factor is downstream policy and never selects evidence.
    public static Option<CapabilityHistory> Of(
        CapabilityIdentity identity,
        ItGrade grade,
        Instant at,
        Seq<CapabilityHistory> history) =>
        history.Filter(row => row.Identity == identity && row.Grade.Name == grade.Name
                && row.Grade.Diameter == grade.Diameter && row.ValidFrom <= at && at < row.ValidUntil)
            .OrderByDescending(static row => row.ValidFrom)
            .Head;
}
```
