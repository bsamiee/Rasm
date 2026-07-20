# [RASM_FABRICATION_WELD_PROCEDURE]

`Procedure.Assess` evaluates one admitted procedure request against code-profiled WPS/PQR rules, assigned-welder ranges, validity and continuity intervals, and inspection-scope policy. Essential variables are profile data over family, modality, and dimension parameters; `VariableKey` is the correspondence key every rule and demand map carries, so a variable resolves by identity rather than by whole-row structure.

Qualification mismatch is a decision, never an admission failure: welder status, continuity, WPS validity, and every range comparison land as `ComplianceRow` evidence, while admission owns only structural well-formedness. `ProcedureDecision` preserves qualified and unqualified receipts with WPS/PQR identity, welder evidence, quantity-preserving comparisons, derived inspection requirements, and an ordered structural-diff surface for revision and audit consumers.

## [01]-[INDEX]

- [02]-[QUALIFICATION_PROFILE]: variable generation, dimensional correspondence, WPS/PQR evidence, welder continuity, and request admission.
- [03]-[ASSESSMENT_FOLD]: accumulated pairing, total qualification evaluation, inspection derivation, receipt diff, and decision projection.

## [02]-[QUALIFICATION_PROFILE]

- Owner: `QualificationProfile` owns procedure and personnel test sets with the governing-code variable registry; `EssentialVariable` carries key, family, modality, admitted quantity dimension, source scopes, essentiality, and parameterized applicability as one admitted value.
- Key: `VariableKey` is the keyed identity every rule and demand map indexes on, so a profile edition that revises a variable row still resolves its rules and values.
- Cases: `QualificationValue` distinguishes demanded evidence, context exclusion, and permitted nonessential omission; `QualificationValue.Qualify` is the one total pairing over that family, so `Qualification` admits no rule-shape fallback arm.
- Dimension: a quantity variable carries its `QuantityInfo`, and admission proves demand, range low, and range high share it, so evaluation compares scalars that are already dimensionally paired.
- Requirement: `VariableRequirement` distinguishes evidence-bearing and nonessential variables; `ApplicabilityLaw` carries conditional essentiality, and `ContextExcluded` remains distinct from permitted `EvidenceOmitted` receipt rows.
- Evidence: `PqrEvidence` owns specimen-specific procedure tests; `WelderRegistry` resolves assigned `WelderQualification` ranges, test evidence, status, and activity-derived continuity.
- Entry: `Procedure.Assess` accepts only `ProcedureRequest`; `Wps`, `WeldDemand`, assignments, inspection context, and assessment time enter through that generated aggregate gate.
- Packages: `Thinktecture.Runtime.Extensions` owns admitted values and closed dispatch; `UnitsNet` owns physical dimensions and registry identity; `NodaTime` owns validity; `LanguageExt.Core` owns accumulated assessment; `Generator.Equals` owns ordered receipt equality and member diffs.
- Growth: governing-code breadth is profile data, so one variable row or inspection rule extends a regime without a checker method, named field, or new public surface.
- Boundary: every qualification verdict — expired continuity, suspended status, out-of-range value — remains a domain decision; only missing, duplicate, dimensionally incompatible, or malformed evidence fails request admission.
- Exemption: generated admission hooks are definition-time boundary statements; `Procedure.Receipt` is the measured evidence-projection fold.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------------------------------------------------------
using System.Globalization;
using System.Runtime.InteropServices;
using Generator.Equals;
using LanguageExt;
using LanguageExt.Common;
using NodaTime;
using Rasm.Fabrication.Fixturing;
using Rasm.Fabrication.Process;
using Thinktecture;
using UnitsNet;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Joining;

// --- [TYPES] --------------------------------------------------------------------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class VariableFamily {
    public static readonly VariableFamily Validity = new("validity");
    public static readonly VariableFamily Process = new("process");
    public static readonly VariableFamily Joint = new("joint");
    public static readonly VariableFamily Consumable = new("consumable");
    public static readonly VariableFamily Position = new("position");
    public static readonly VariableFamily Electrical = new("electrical");
    public static readonly VariableFamily Material = new("material");
    public static readonly VariableFamily Dimension = new("dimension");
    public static readonly VariableFamily Thermal = new("thermal");
    public static readonly VariableFamily Technique = new("technique");
    public static readonly VariableFamily Inspection = new("inspection");
    public static readonly VariableFamily Personnel = new("personnel");
}

[SmartEnum<string>]
public sealed partial class VariableModality {
    public static readonly VariableModality Quantity = new("quantity");
    public static readonly VariableModality Categorical = new("categorical");
    public static readonly VariableModality Boolean = new("boolean");
    public static readonly VariableModality Temporal = new("temporal");
}

[ValueObject<string>]
public readonly partial struct VariableKey {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) =>
        validationError = string.IsNullOrWhiteSpace(value) ? new ValidationError(message: "variable-key") : null;
}

[SmartEnum<string>]
public sealed partial class ApplicabilityMode {
    public static readonly ApplicabilityMode All = new(
        "all",
        static (tokens, context) => tokens.ForAll(context.Contains));
    public static readonly ApplicabilityMode Any = new(
        "any",
        static (tokens, context) => tokens.Exists(context.Contains));
    public static readonly ApplicabilityMode None = new(
        "none",
        static (tokens, context) => !tokens.Exists(context.Contains));

    [UseDelegateFromConstructor]
    public partial bool Matches(Set<string> tokens, Set<string> context);
}

[SmartEnum<string>]
public sealed partial class VariableRequirement {
    public static readonly VariableRequirement Essential = new("essential", evidenceRequired: true);
    public static readonly VariableRequirement Nonessential = new("nonessential", evidenceRequired: false);

    public bool EvidenceRequired { get; }
}

[ComplexValueObject]
public sealed partial class ApplicabilityLaw {
    public ApplicabilityMode Mode { get; }
    public Set<string> Tokens { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref ApplicabilityMode mode,
        ref Set<string> tokens) =>
        validationError = mode is null || tokens.IsEmpty
            || tokens.Exists(static token => string.IsNullOrWhiteSpace(token))
            ? new ValidationError(message: "applicability-law")
            : null;

    public bool Matches(Set<string> context) => Mode.Matches(Tokens, context);
}

[ComplexValueObject]
public sealed partial class EssentialVariable {
    public VariableKey Key { get; }
    public VariableFamily Family { get; }
    public VariableModality Modality { get; }
    public Option<QuantityInfo> Quantity { get; }
    public Set<QualificationSource> Sources { get; }
    public VariableRequirement Requirement { get; }
    public Option<ApplicabilityLaw> Applicability { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref VariableKey key,
        ref VariableFamily family,
        ref VariableModality modality,
        ref Option<QuantityInfo> quantity,
        ref Set<QualificationSource> sources,
        ref VariableRequirement requirement,
        ref Option<ApplicabilityLaw> applicability) =>
        validationError = key == default || family is null || modality is null || sources.IsEmpty
            || requirement is null
            || quantity.IsSome != (modality == VariableModality.Quantity)
            || quantity.Exists(static info => info is null)
            || sources.Exists(static source => source is null)
            || applicability.Exists(static law => law is null)
            ? new ValidationError(message: "essential-variable")
            : null;
}

[ComplexValueObject]
public sealed partial class QualificationProfile {
    public string Code { get; }
    public string Edition { get; }
    public ProcessKind Process { get; }
    public Seq<EssentialVariable> Variables { get; }
    public Set<TestKind> RequiredProcedureTests { get; }
    public Set<TestKind> RequiredPersonnelTests { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref string code,
        ref string edition,
        ref ProcessKind process,
        ref Seq<EssentialVariable> variables,
        ref Set<TestKind> requiredProcedureTests,
        ref Set<TestKind> requiredPersonnelTests) =>
        validationError = string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(edition) || process is null
            || process.Modality.Class != ModalityClass.Joined || variables.IsEmpty
            || variables.Exists(static variable => variable is null)
            || variables.Map(static variable => variable.Key).Distinct().Count != variables.Count
            || variables.Filter(static variable => variable.Family == VariableFamily.Validity).Count != 1
            || variables.Filter(static variable => variable.Family == VariableFamily.Validity).Exists(variable =>
                variable.Modality != VariableModality.Temporal || !variable.Requirement.EvidenceRequired
                || !variable.Sources.Contains(QualificationSource.Procedure)
                || !variable.Sources.Contains(QualificationSource.Welder))
            || requiredProcedureTests.IsEmpty || requiredPersonnelTests.IsEmpty
            ? new ValidationError(message: "qualification-profile")
            : null;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record QualificationValue {
    private QualificationValue() { }

    public sealed record Quantity(IQuantity Value) : QualificationValue;
    public sealed record Categorical(string Value) : QualificationValue;
    public sealed record Boolean(bool Value) : QualificationValue;
    public sealed record Temporal(Instant Value) : QualificationValue;
    public sealed record ContextExcluded : QualificationValue;
    public sealed record EvidenceOmitted : QualificationValue;

    public Option<VariableModality> Modality => Switch(
        quantity: static _ => Some(VariableModality.Quantity),
        categorical: static _ => Some(VariableModality.Categorical),
        boolean: static _ => Some(VariableModality.Boolean),
        temporal: static _ => Some(VariableModality.Temporal),
        contextExcluded: static _ => Option<VariableModality>.None,
        evidenceOmitted: static _ => Option<VariableModality>.None);

    public bool Valid => Switch(
        quantity: static value => value.Value is not null,
        categorical: static value => !string.IsNullOrWhiteSpace(value.Value),
        boolean: static _ => true,
        temporal: static _ => true,
        contextExcluded: static _ => true,
        evidenceOmitted: static _ => true);

    // One total dispatch over the value family pairs the rule; admission already proved modality and dimension agreement.
    public Option<Qualification> Qualify(EssentialVariable variable, QualificationRule rule) => Switch(
        state: (Variable: variable, Rule: rule),
        quantity: static (state, value) => state.Rule.Required is QualificationRule.QuantityRange range
            ? Some<Qualification>(new Qualification.Quantity(state.Variable, value.Value, range.Low, range.High))
            : Option<Qualification>.None,
        categorical: static (state, value) => state.Rule.Required is QualificationRule.CategoricalSet admitted
            ? Some<Qualification>(new Qualification.Categorical(state.Variable, value.Value, admitted.Admitted))
            : Option<Qualification>.None,
        boolean: static (state, value) => state.Rule.Required is QualificationRule.Boolean required
            ? Some<Qualification>(new Qualification.Boolean(state.Variable, value.Value, required.Required))
            : Option<Qualification>.None,
        temporal: static (state, value) => state.Rule.Required is QualificationRule.ActiveInterval window
            ? Some<Qualification>(new Qualification.Temporal(state.Variable, value.Value, window.Interval))
            : Option<Qualification>.None,
        contextExcluded: static (state, _) => Some<Qualification>(new Qualification.ContextExcluded(state.Variable)),
        evidenceOmitted: static (state, _) => Some<Qualification>(new Qualification.EvidenceOmitted(state.Variable)));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record QualificationRule {
    private QualificationRule() { }

    public sealed record QuantityRange(IQuantity Low, IQuantity High) : QualificationRule;
    public sealed record CategoricalSet(Set<string> Admitted) : QualificationRule;
    public sealed record Boolean(bool Required) : QualificationRule;
    public sealed record ActiveInterval(Interval Interval) : QualificationRule;
    public sealed record Optional(QualificationRule Present) : QualificationRule;

    public VariableModality Modality => Switch(
        quantityRange: static _ => VariableModality.Quantity,
        categoricalSet: static _ => VariableModality.Categorical,
        boolean: static _ => VariableModality.Boolean,
        activeInterval: static _ => VariableModality.Temporal,
        optional: static value => value.Present.Modality);

    public QualificationRule Required => this is Optional optional ? optional.Present : this;

    public bool Accepts(EssentialVariable variable) =>
        variable is not null && variable.Modality == Modality && Switch(
            state: variable,
            quantityRange: static (row, value) => row.Quantity.Exists(info =>
                info == value.Low.QuantityInfo && info == value.High.QuantityInfo),
            categoricalSet: static (_, _) => true,
            boolean: static (_, _) => true,
            activeInterval: static (_, _) => true,
            optional: static (row, value) => value.Present.Accepts(row));

    public bool Valid => Switch(
        quantityRange: static value => value.Low is not null && value.High is not null
            && value.Low.QuantityInfo == value.High.QuantityInfo
            && value.Low.As(value.High.Unit) <= (double)value.High.Value,
        categoricalSet: static value => !value.Admitted.IsEmpty
            && value.Admitted.ForAll(static item => !string.IsNullOrWhiteSpace(item)),
        boolean: static _ => true,
        activeInterval: static value => value.Interval.HasStart && value.Interval.HasEnd
            && value.Interval.Start < value.Interval.End,
        optional: static value => value.Present is not null && value.Present is not Optional && value.Present.Valid);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
internal abstract partial record Qualification {
    private Qualification() { }

    public sealed record Quantity(EssentialVariable Variable, IQuantity Demanded, IQuantity Low, IQuantity High) : Qualification;
    public sealed record Categorical(EssentialVariable Variable, string Demanded, Set<string> Admitted) : Qualification;
    public sealed record Boolean(EssentialVariable Variable, bool Demanded, bool Required) : Qualification;
    public sealed record Temporal(EssentialVariable Variable, Instant Demanded, Interval Interval) : Qualification;
    public sealed record ContextExcluded(EssentialVariable Variable) : Qualification;
    public sealed record EvidenceOmitted(EssentialVariable Variable) : Qualification;

    public ComplianceRow Evaluate(int joint, QualificationSource source) => Switch(
        state: (Joint: joint, Source: source),
        quantity: static (state, value) => new ComplianceRow.Quantity(
            state.Joint,
            state.Source,
            value.Variable,
            value.Demanded,
            value.Low,
            value.High,
            Comparable(value.Demanded, value.Low, value.High)),
        categorical: static (state, value) => new ComplianceRow.Categorical(
            state.Joint,
            state.Source,
            value.Variable,
            value.Demanded,
            value.Admitted,
            value.Admitted.Contains(value.Demanded)),
        boolean: static (state, value) => new ComplianceRow.Boolean(
            state.Joint,
            state.Source,
            value.Variable,
            value.Demanded,
            value.Required,
            value.Demanded == value.Required),
        temporal: static (state, value) => new ComplianceRow.Temporal(
            state.Joint,
            state.Source,
            value.Variable,
            value.Demanded,
            value.Interval,
            value.Interval.Contains(value.Demanded)),
        contextExcluded: static (state, value) => new ComplianceRow.ContextExcluded(state.Joint, state.Source, value.Variable),
        evidenceOmitted: static (state, value) => new ComplianceRow.EvidenceOmitted(state.Joint, state.Source, value.Variable));

    // EssentialVariable.Quantity fixes the family at admission, so the comparison is scalar-only.
    private static bool Comparable(IQuantity demanded, IQuantity low, IQuantity high) =>
        demanded.As(low.Unit) >= (double)low.Value && demanded.As(high.Unit) <= (double)high.Value;
}

[SmartEnum<string>]
public sealed partial class QualificationSource {
    public static readonly QualificationSource Procedure = new("procedure");
    public static readonly QualificationSource Welder = new("welder");
}

[SmartEnum<string>]
public sealed partial class TestKind {
    public static readonly TestKind Tensile = new("tensile", destructive: true, minimumSpecimens: 2);
    public static readonly TestKind GuidedBend = new("guided-bend", destructive: true, minimumSpecimens: 4);
    public static readonly TestKind Impact = new("impact", destructive: true, minimumSpecimens: 3);
    public static readonly TestKind Macro = new("macro", destructive: true, minimumSpecimens: 1);
    public static readonly TestKind Hardness = new("hardness", destructive: false, minimumSpecimens: 1);
    public static readonly TestKind Nondestructive = new("nondestructive", destructive: false, minimumSpecimens: 1);

    public bool Destructive { get; }
    public int MinimumSpecimens { get; }

    public bool Admits(Seq<QualificationTest> tests) =>
        tests.Count(test => test.Kind == this) >= MinimumSpecimens;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record QualificationTest {
    private QualificationTest() { }

    public sealed record Quantity(
        TestKind Test,
        string Specimen,
        IQuantity Result,
        Option<IQuantity> Minimum,
        Option<IQuantity> Maximum) : QualificationTest;
    public sealed record Bend(string Specimen, Length Discontinuity, Length Limit) : QualificationTest;
    public sealed record Examination(TestKind Test, string Specimen, string Method, string Acceptance, bool Passed) : QualificationTest;

    public TestKind Kind => Switch(
        quantity: static value => value.Test,
        bend: static _ => TestKind.GuidedBend,
        examination: static value => value.Test);

    public string Specimen => Switch(
        quantity: static value => value.Specimen,
        bend: static value => value.Specimen,
        examination: static value => value.Specimen);

    public bool Passed => Switch(
        quantity: static value => value.Minimum.ForAll(minimum => value.Result.As(minimum.Unit) >= (double)minimum.Value)
            && value.Maximum.ForAll(maximum => value.Result.As(maximum.Unit) <= (double)maximum.Value),
        bend: static value => value.Discontinuity <= value.Limit,
        examination: static value => value.Passed);

    public bool Valid => Switch(
        quantity: static value => value.Test is not null && !string.IsNullOrWhiteSpace(value.Specimen)
            && value.Result is not null && (value.Minimum.IsSome || value.Maximum.IsSome)
            && value.Minimum.ForAll(minimum => minimum is not null && minimum.QuantityInfo == value.Result.QuantityInfo)
            && value.Maximum.ForAll(maximum => maximum is not null && maximum.QuantityInfo == value.Result.QuantityInfo)
            && value.Minimum.ForAll(minimum => value.Maximum.ForAll(maximum =>
                minimum.As(maximum.Unit) <= (double)maximum.Value)),
        bend: static value => !string.IsNullOrWhiteSpace(value.Specimen)
            && value.Discontinuity >= Length.Zero && value.Limit >= Length.Zero,
        examination: static value => value.Test is not null && !string.IsNullOrWhiteSpace(value.Specimen)
            && !string.IsNullOrWhiteSpace(value.Method)
            && !string.IsNullOrWhiteSpace(value.Acceptance));
}

[ComplexValueObject]
public sealed partial class PqrEvidence {
    public string Id { get; }
    public string Coupon { get; }
    public Instant QualifiedAt { get; }
    public Seq<QualificationTest> Tests { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref string id,
        ref string coupon,
        ref Instant qualifiedAt,
        ref Seq<QualificationTest> tests) =>
        validationError = string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(coupon) || tests.IsEmpty
            || tests.Exists(static test => test is null || !test.Valid)
            || tests.Map(static test => (test.Kind, test.Specimen)).Distinct().Count != tests.Count
            || tests.Exists(static test => !test.Passed)
            ? new ValidationError(message: "pqr-evidence")
            : null;
}

[ComplexValueObject]
public sealed partial class Wps {
    public string Id { get; }
    public int Revision { get; }
    public Interval Validity { get; }
    public QualificationProfile Profile { get; }
    public PqrEvidence Pqr { get; }
    public Map<VariableKey, QualificationRule> Rules { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref string id,
        ref int revision,
        ref Interval validity,
        ref QualificationProfile profile,
        ref PqrEvidence pqr,
        ref Map<VariableKey, QualificationRule> rules) =>
        validationError = string.IsNullOrWhiteSpace(id) || revision <= 0 || !validity.HasStart || !validity.HasEnd
            || validity.Start >= validity.End || profile is null || pqr is null
            || pqr.QualifiedAt > validity.Start
            || !profile.RequiredProcedureTests.ForAll(kind => kind.Admits(pqr.Tests))
            || profile.Variables.Exists(variable => variable.Family != VariableFamily.Validity
                && variable.Sources.Contains(QualificationSource.Procedure)
                && !rules.Find(variable.Key).Exists(rule => rule.Valid && rule.Accepts(variable)))
            || rules.Keys.Exists(key => !profile.Variables.Exists(variable => variable.Key == key
                && variable.Family != VariableFamily.Validity
                && variable.Sources.Contains(QualificationSource.Procedure)))
            ? new ValidationError(message: "wps")
            : null;
}

[SmartEnum<string>]
public sealed partial class QualificationStatus {
    public static readonly QualificationStatus Active = new("active", admits: true, recoverable: true);
    public static readonly QualificationStatus Suspended = new("suspended", admits: false, recoverable: true);
    public static readonly QualificationStatus Revoked = new("revoked", admits: false, recoverable: false);

    public bool Admits { get; }
    public bool Recoverable { get; }
}

[ComplexValueObject]
public sealed partial class WelderQualification {
    public string Welder { get; }
    public string Record { get; }
    public Instant QualifiedAt { get; }
    public Instant LastActivity { get; }
    public NodaTime.Duration ContinuityPeriod { get; }
    public QualificationStatus Status { get; }
    public Map<VariableKey, QualificationRule> Rules { get; }
    public Seq<QualificationTest> Tests { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref string welder,
        ref string record,
        ref Instant qualifiedAt,
        ref Instant lastActivity,
        ref NodaTime.Duration continuityPeriod,
        ref QualificationStatus status,
        ref Map<VariableKey, QualificationRule> rules,
        ref Seq<QualificationTest> tests) =>
        validationError = string.IsNullOrWhiteSpace(welder) || string.IsNullOrWhiteSpace(record)
            || lastActivity < qualifiedAt || continuityPeriod <= NodaTime.Duration.Zero || status is null
            || rules.IsEmpty || rules.Values.Exists(static rule => rule is null || !rule.Valid) || tests.IsEmpty
            || tests.Exists(static test => test is null || !test.Valid || !test.Passed)
            || tests.Map(static test => (test.Kind, test.Specimen)).Distinct().Count != tests.Count
            ? new ValidationError(message: "welder-qualification")
            : null;

    public Interval Continuity => new(QualifiedAt, LastActivity + ContinuityPeriod);
}

[ComplexValueObject]
public sealed partial class WelderRegistry {
    public Map<string, WelderQualification> Qualifications { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Map<string, WelderQualification> qualifications) =>
        validationError = qualifications.IsEmpty || qualifications.Keys.Exists(static key => string.IsNullOrWhiteSpace(key))
            || qualifications.Keys.Exists(key => !qualifications.Find(key)
                .Exists(qualification => qualification is not null && qualification.Welder == key))
            ? new ValidationError(message: "welder-registry")
            : null;

    public Option<WelderQualification> Find(string welder) => Qualifications.Find(welder);
}

[SmartEnum<string>]
public sealed partial class InspectionFamily {
    public static readonly InspectionFamily Visual = new("visual", subsurface: false, consumesPart: false, hydrogenDelay: false);
    public static readonly InspectionFamily Surface = new("surface", subsurface: false, consumesPart: false, hydrogenDelay: true);
    public static readonly InspectionFamily Volumetric = new("volumetric", subsurface: true, consumesPart: false, hydrogenDelay: true);
    public static readonly InspectionFamily Destructive = new("destructive", subsurface: true, consumesPart: true, hydrogenDelay: false);

    public bool Subsurface { get; }
    public bool ConsumesPart { get; }
    public bool HydrogenDelay { get; }
}

[SmartEnum<string>]
public sealed partial class InspectionSampling {
    public static readonly InspectionSampling JointCount = new(
        "joint-count",
        static extent => extent is InspectionExtent.Joints);
    public static readonly InspectionSampling WeldLength = new(
        "weld-length",
        static extent => extent is InspectionExtent.Linear);
    public static readonly InspectionSampling SurfaceArea = new(
        "surface-area",
        static extent => extent is InspectionExtent.Areal);
    public static readonly InspectionSampling Volume = new(
        "volume",
        static extent => extent is InspectionExtent.Volumetric);

    [UseDelegateFromConstructor]
    public partial bool Accepts(InspectionExtent extent);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record InspectionExtent {
    private InspectionExtent() { }

    public sealed record Joints(int Count) : InspectionExtent;
    public sealed record Linear(Length Value) : InspectionExtent;
    public sealed record Areal(Area Value) : InspectionExtent;
    public sealed record Volumetric(Volume Value) : InspectionExtent;

    public bool Valid => Switch(
        joints: static value => value.Count > 0,
        linear: static value => double.IsFinite(value.Value.Millimeters) && value.Value > Length.Zero,
        areal: static value => double.IsFinite(value.Value.SquareMillimeters) && value.Value > Area.Zero,
        volumetric: static value => double.IsFinite(value.Value.CubicMillimeters) && value.Value > UnitsNet.Volume.Zero);

    public InspectionExtent Sample(Ratio coverage) => Switch(
        state: coverage.DecimalFractions,
        joints: static (fraction, value) => new Joints(Math.Max(1, (int)Math.Ceiling(value.Count * fraction))),
        linear: static (fraction, value) => new Linear(value.Value * fraction),
        areal: static (fraction, value) => new Areal(value.Value * fraction),
        volumetric: static (fraction, value) => new Volumetric(value.Value * fraction));
}

[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct InspectionBasis {
    public JoinClass JointClass { get; }
    public string ExecutionClass { get; }
    public string StressCategory { get; }
    public bool FatigueCritical { get; }
    public Length Thickness { get; }
    public Map<InspectionSampling, InspectionExtent> Populations { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref JoinClass jointClass,
        ref string executionClass,
        ref string stressCategory,
        ref bool fatigueCritical,
        ref Length thickness,
        ref Map<InspectionSampling, InspectionExtent> populations) =>
        validationError = jointClass is null || string.IsNullOrWhiteSpace(executionClass)
            || string.IsNullOrWhiteSpace(stressCategory) || !double.IsFinite(thickness.Millimeters)
            || thickness <= Length.Zero || populations.IsEmpty
            || populations.Keys.Exists(sampling => sampling is null
                || !populations.Find(sampling).Exists(extent => extent is not null && extent.Valid && sampling.Accepts(extent)))
            ? new ValidationError(message: "inspection-basis")
            : null;
}

[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct InspectionRule {
    public string ExecutionClass { get; }
    public Option<JoinClass> JointClass { get; }
    public Option<string> StressCategory { get; }
    public InspectionFamily Family { get; }
    public InspectionSampling Sampling { get; }
    public Ratio Coverage { get; }
    public string Acceptance { get; }
    public bool FatigueOnly { get; }
    public Length MinimumThickness { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref string executionClass,
        ref Option<JoinClass> jointClass,
        ref Option<string> stressCategory,
        ref InspectionFamily family,
        ref InspectionSampling sampling,
        ref Ratio coverage,
        ref string acceptance,
        ref bool fatigueOnly,
        ref Length minimumThickness) =>
        validationError = string.IsNullOrWhiteSpace(executionClass) || family is null || sampling is null
            || stressCategory.Exists(static category => string.IsNullOrWhiteSpace(category))
            || !double.IsFinite(coverage.DecimalFractions) || coverage <= Ratio.Zero || coverage > Ratio.FromPercent(100)
            || string.IsNullOrWhiteSpace(acceptance) || !double.IsFinite(minimumThickness.Millimeters)
            || minimumThickness < Length.Zero
            ? new ValidationError(message: "inspection-rule")
            : null;

    public bool Applies(InspectionBasis basis) =>
        ExecutionClass == basis.ExecutionClass
        && JointClass.ForAll(jointClass => jointClass == basis.JointClass)
        && StressCategory.ForAll(category => category == basis.StressCategory)
        && (!FatigueOnly || basis.FatigueCritical)
        && basis.Thickness >= MinimumThickness
        && basis.Populations.ContainsKey(Sampling);

    public Option<InspectionRequirement> Require(int joint, InspectionBasis basis) => Applies(basis)
        ? basis.Populations.Find(Sampling).Map(population => new InspectionRequirement(
                joint,
                Family,
                Sampling,
                Coverage,
                population,
                population.Sample(Coverage),
                Acceptance,
                basis))
        : Option<InspectionRequirement>.None;
}

[ComplexValueObject]
public sealed partial class InspectionPolicy {
    public Seq<InspectionRule> Rules { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Seq<InspectionRule> rules) =>
        validationError = rules.IsEmpty || rules.Exists(static rule => rule == default)
            ? new ValidationError(message: "inspection-policy")
            : null;

    public bool Covers(InspectionBasis basis) => Rules.Exists(rule => rule.Applies(basis));
}

// --- [MODELS] -------------------------------------------------------------------------------------------------------------------------------------
[ComplexValueObject]
public sealed partial class WeldDemand {
    public int Joint { get; }
    public Map<VariableKey, QualificationValue> Values { get; }
    public Set<string> Context { get; }
    public InspectionBasis Inspection { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref int joint,
        ref Map<VariableKey, QualificationValue> values,
        ref Set<string> context,
        ref InspectionBasis inspection) =>
        validationError = joint < 0 || values.IsEmpty || inspection == default
            || context.Exists(static token => string.IsNullOrWhiteSpace(token))
            || values.Keys.Exists(key => key == default
                || !values.Find(key).Exists(value => value is not null && value.Valid))
            || values.Values.Exists(static value => value is QualificationValue.ContextExcluded or QualificationValue.EvidenceOmitted)
            ? new ValidationError(message: "weld-demand")
            : null;

    // Modality and dimension bind to the profile, which the demand alone does not carry; ProcedureRequest owns that gate.
    public bool Corresponds(EssentialVariable variable) =>
        Values.Find(variable.Key).ForAll(value => value.Modality.Exists(modality => modality == variable.Modality)
            && (value is not QualificationValue.Quantity demanded
                || variable.Quantity.Exists(info => info == demanded.Value.QuantityInfo)));
}

[ComplexValueObject]
public sealed partial class ProcedureRequest {
    public Seq<WeldDemand> Demands { get; }
    public Wps Wps { get; }
    public Map<int, string> Assignments { get; }
    public WelderRegistry Welders { get; }
    public InspectionPolicy Inspections { get; }
    public Instant At { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Seq<WeldDemand> demands,
        ref Wps wps,
        ref Map<int, string> assignments,
        ref WelderRegistry welders,
        ref InspectionPolicy inspections,
        ref Instant at) =>
        validationError = demands.IsEmpty || demands.Exists(static demand => demand is null)
            || demands.Map(static demand => demand.Joint).Distinct().Count != demands.Count
            || wps is null || welders is null || inspections is null
            || demands.Exists(demand => !assignments.ContainsKey(demand.Joint))
            || assignments.Keys.Exists(joint => !demands.Exists(demand => demand.Joint == joint))
            || assignments.Values.Exists(welder => !welders.Find(welder).IsSome)
            || demands.Exists(demand => !inspections.Covers(demand.Inspection))
            || demands.Exists(demand => wps.Profile.Variables.Exists(variable =>
                variable.Requirement.EvidenceRequired && variable.Family != VariableFamily.Validity
                && variable.Applicability.ForAll(law => law.Matches(demand.Context))
                && !demand.Values.ContainsKey(variable.Key)))
            || demands.Exists(demand => demand.Values.Keys.Exists(key =>
                !wps.Profile.Variables.Exists(variable => variable.Key == key)))
            || demands.Exists(demand => wps.Profile.Variables.Exists(variable => !demand.Corresponds(variable)
                || (variable.Applicability.Exists(law => !law.Matches(demand.Context))
                    && demand.Values.ContainsKey(variable.Key))))
            || assignments.Values.Exists(welder => !welders.Find(welder).Exists(assignment =>
                wps.Profile.RequiredPersonnelTests.ForAll(kind => kind.Admits(assignment.Tests))
                && wps.Profile.Variables.ForAll(variable => variable.Family == VariableFamily.Validity
                    || !variable.Sources.Contains(QualificationSource.Welder)
                    || assignment.Rules.Find(variable.Key).Exists(rule => rule is not null && rule.Valid && rule.Accepts(variable)))
                && assignment.Rules.Keys.ForAll(key => wps.Profile.Variables.Exists(variable => variable.Key == key
                    && variable.Family != VariableFamily.Validity
                    && variable.Sources.Contains(QualificationSource.Welder)))))
            ? new ValidationError(message: "procedure-request")
            : null;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ComplianceRow(
    int Joint,
    QualificationSource Source,
    bool Passed,
    Option<VariableKey> Subject,
    FaultSubject.Qualification Fault,
    double FaultScalar,
    string FaultEvidence) {
    public sealed record Quantity(
        int Joint,
        QualificationSource Source,
        EssentialVariable Variable,
        IQuantity Demanded,
        IQuantity Low,
        IQuantity High,
        bool Pass) : ComplianceRow(
            Joint, Source, Pass, Some(Variable.Key), new FaultSubject.Qualification(Variable.Key.Value),
            (double)Demanded.Value, Demanded.ToString(CultureInfo.InvariantCulture));

    public sealed record Categorical(
        int Joint,
        QualificationSource Source,
        EssentialVariable Variable,
        string Demanded,
        Set<string> Admitted,
        bool Pass) : ComplianceRow(
            Joint, Source, Pass, Some(Variable.Key), new FaultSubject.Qualification(Variable.Key.Value), 0.0, Demanded);

    public sealed record Boolean(
        int Joint,
        QualificationSource Source,
        EssentialVariable Variable,
        bool Demanded,
        bool Required,
        bool Pass) : ComplianceRow(
            Joint, Source, Pass, Some(Variable.Key), new FaultSubject.Qualification(Variable.Key.Value),
            Demanded ? 1.0 : 0.0, Demanded.ToString(CultureInfo.InvariantCulture));

    public sealed record Temporal(
        int Joint,
        QualificationSource Source,
        EssentialVariable Variable,
        Instant Demanded,
        Interval Interval,
        bool Pass) : ComplianceRow(
            Joint, Source, Pass, Some(Variable.Key), new FaultSubject.Qualification(Variable.Key.Value),
            Demanded.ToUnixTimeTicks(), Demanded.ToUnixTimeTicks().ToString(CultureInfo.InvariantCulture));

    public sealed record ContextExcluded(
        int Joint,
        QualificationSource Source,
        EssentialVariable Variable) : ComplianceRow(
            Joint, Source, true, Some(Variable.Key), new FaultSubject.Qualification(Variable.Key.Value),
            0.0, nameof(QualificationValue.ContextExcluded));

    public sealed record EvidenceOmitted(
        int Joint,
        QualificationSource Source,
        EssentialVariable Variable) : ComplianceRow(
            Joint, Source, true, Some(Variable.Key), new FaultSubject.Qualification(Variable.Key.Value),
            0.0, nameof(QualificationValue.EvidenceOmitted));

    public sealed record Standing(
        int Joint,
        QualificationSource Source,
        string Welder,
        QualificationStatus Held,
        Interval Continuity,
        Instant At) : ComplianceRow(
            Joint, Source, Held.Admits && Continuity.Contains(At), Option<VariableKey>.None,
            new FaultSubject.Qualification(Welder), At.ToUnixTimeTicks(), Held.Key);
}

public sealed record InspectionRequirement(
    int Joint,
    InspectionFamily Family,
    InspectionSampling Sampling,
    Ratio Coverage,
    InspectionExtent Population,
    InspectionExtent Sample,
    string Acceptance,
    InspectionBasis Basis);

public sealed record QualificationRecord(
    QualificationSource Source,
    Option<int> Joint,
    string Subject,
    string Record,
    Interval Validity,
    Option<QualificationStatus> Status,
    Seq<QualificationTest> Tests);

[Equatable]
public sealed partial record ProcedureReceipt(
    string WpsId,
    int Revision,
    string PqrId,
    ProcessKind Process,
    [property: OrderedEquality] Seq<ComplianceRow> Rows,
    [property: OrderedEquality] Seq<InspectionRequirement> Inspections,
    [property: OrderedEquality] Seq<QualificationRecord> Qualifications,
    bool Qualified,
    Instant At) {
    [IgnoreEquality]
    public Seq<string> Welders => Qualifications
        .Filter(static record => record.Source == QualificationSource.Welder)
        .Map(static record => record.Subject)
        .Distinct()
        .ToSeq();

    public Seq<Inequality> Diff(ProcedureReceipt prior) =>
        toSeq(EqualityComparer.Default.Inequalities(prior, this));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ProcedureDecision {
    private ProcedureDecision() { }

    public sealed record Qualified(ProcedureReceipt Receipt) : ProcedureDecision;
    public sealed record Unqualified(ProcedureReceipt Receipt, Seq<ComplianceRow> Failures) : ProcedureDecision;

    public Fin<ProcedureReceipt> Require() => Switch(
        qualified: static decision => Fin.Succ(decision.Receipt),
        unqualified: static decision => decision.Failures.HeadOrNone()
            .ToFin(Error.New("weld-procedure:empty-failure-set"))
            .Bind(first => Fin.Fail<ProcedureReceipt>(decision.Failures.Tail.Fold(
                Failure(first),
                static (combined, row) => combined + Failure(row))));

    private static Error Failure(ComplianceRow row) =>
        new FabricationFault.WpsUnqualified(row.Fault, row.FaultScalar)
        + Error.New($"weld-procedure:evidence:{row.FaultEvidence}");
}

// --- [OPERATIONS] ---------------------------------------------------------------------------------------------------------------------------------
public static class Procedure {
    public static Fin<ProcedureDecision> Assess(ProcedureRequest request) =>
        Optional(request).ToFin(Error.New("procedure-request:null"))
            .Bind(AssessAll)
            .Map(Decide);

    private static Fin<ProcedureReceipt> AssessAll(ProcedureRequest request) =>
        request.Demands.Map(demand => AssessDemand(request, demand).ToValidation())
            .Traverse(identity)
            .As()
            .ToFin()
            .Map(rows => Receipt(request, rows));

    // Welder standing is evidence, not admission: a suspended or lapsed welder yields an unqualified receipt, never a fault.
    private static Fin<Seq<ComplianceRow>> AssessDemand(ProcedureRequest request, WeldDemand demand) =>
        from welderId in request.Assignments.Find(demand.Joint)
            .ToFin(Error.New($"weld-procedure:welder:{demand.Joint}"))
        from welder in request.Welders.Find(welderId)
            .ToFin(Error.New($"weld-procedure:qualification:{welderId}"))
        from rows in (
            Scope(
                demand,
                request.Wps.Profile.Variables.Filter(variable =>
                    variable.Sources.Contains(QualificationSource.Procedure)),
                request.Wps.Rules,
                request.Wps.Validity,
                request.At,
                QualificationSource.Procedure).ToValidation(),
            Scope(
                demand,
                request.Wps.Profile.Variables.Filter(variable =>
                    variable.Sources.Contains(QualificationSource.Welder)),
                welder.Rules,
                welder.Continuity,
                request.At,
                QualificationSource.Welder).ToValidation())
            .Apply(static (procedure, person) => procedure + person)
            .As()
            .ToFin()
        select rows.Add(new ComplianceRow.Standing(
            demand.Joint,
            QualificationSource.Welder,
            welder.Welder,
            welder.Status,
            welder.Continuity,
            request.At));

    private static Fin<Seq<ComplianceRow>> Scope(
        WeldDemand demand,
        Seq<EssentialVariable> variables,
        Map<VariableKey, QualificationRule> rules,
        Interval validity,
        Instant at,
        QualificationSource source) =>
        variables.Map(variable => Admit(
                demand.Joint,
                variable,
                DemandValue(demand, variable, at),
                variable.Family == VariableFamily.Validity
                    ? Some<QualificationRule>(new QualificationRule.ActiveInterval(validity))
                    : rules.Find(variable.Key),
                source)
            .ToValidation())
            .Traverse(identity)
            .As()
            .ToFin();

    private static Option<QualificationValue> DemandValue(WeldDemand demand, EssentialVariable variable, Instant at) =>
        variable.Family == VariableFamily.Validity
            ? Some<QualificationValue>(new QualificationValue.Temporal(at))
            : variable.Applicability.Exists(law => !law.Matches(demand.Context))
                ? Some<QualificationValue>(new QualificationValue.ContextExcluded())
                : demand.Values.Find(variable.Key).Match(
                    Some: static value => Some(value),
                    None: () => variable.Requirement.EvidenceRequired
                        ? None
                        : Some<QualificationValue>(new QualificationValue.EvidenceOmitted()));

    private static Fin<ComplianceRow> Admit(
        int joint,
        EssentialVariable variable,
        Option<QualificationValue> value,
        Option<QualificationRule> rule,
        QualificationSource source) =>
        from valueRow in value.ToFin(Error.New($"weld-procedure:value:{variable.Key}"))
        from ruleRow in rule.ToFin(Error.New($"weld-procedure:rule:{variable.Key}"))
        from qualification in valueRow.Qualify(variable, ruleRow)
            .ToFin(Error.New($"weld-procedure:modality:{variable.Key}"))
        select qualification.Evaluate(joint, source);

    private static ProcedureReceipt Receipt(ProcedureRequest request, Seq<Seq<ComplianceRow>> blocks) {
        Seq<ComplianceRow> rows = blocks.Bind(identity);
        Seq<InspectionRequirement> inspections = request.Demands.Bind(demand => Inspect(request.Inspections, demand));
        Seq<QualificationRecord> qualifications = Seq(
                new QualificationRecord(
                    QualificationSource.Procedure,
                    Option<int>.None,
                    request.Wps.Id,
                    request.Wps.Pqr.Id,
                    request.Wps.Validity,
                    Option<QualificationStatus>.None,
                    request.Wps.Pqr.Tests))
            + request.Assignments.Keys.Choose(joint => request.Assignments.Find(joint)
                .Bind(request.Welders.Find)
                .Map(qualification => new QualificationRecord(
                    QualificationSource.Welder,
                    Some(joint),
                    qualification.Welder,
                    qualification.Record,
                    qualification.Continuity,
                    Some(qualification.Status),
                    qualification.Tests)));
        return new ProcedureReceipt(
            request.Wps.Id,
            request.Wps.Revision,
            request.Wps.Pqr.Id,
            request.Wps.Profile.Process,
            rows,
            inspections,
            qualifications,
            rows.ForAll(static row => row.Passed),
            request.At);
    }

    // Overlapping rules collapse only when family, sampling, and acceptance semantics agree.
    private static Seq<InspectionRequirement> Inspect(InspectionPolicy policy, WeldDemand demand) =>
        policy.Rules.Choose(rule => rule.Require(demand.Joint, demand.Inspection))
            .Fold(
                HashMap<(InspectionFamily Family, InspectionSampling Sampling, string Acceptance), InspectionRequirement>(),
                static (held, row) => held.Find((row.Family, row.Sampling, row.Acceptance))
                    .Exists(prior => prior.Coverage >= row.Coverage)
                        ? held
                        : held.SetItem((row.Family, row.Sampling, row.Acceptance), row))
            .Values
            .OrderBy(static row => row.Family.Key)
            .ThenBy(static row => row.Sampling.Key)
            .ThenBy(static row => row.Acceptance, StringComparer.Ordinal)
            .ToSeq();

    private static ProcedureDecision Decide(ProcedureReceipt receipt) =>
        receipt.Rows.Filter(static row => !row.Passed) switch {
            { IsEmpty: true } => new ProcedureDecision.Qualified(receipt),
            { } failures => new ProcedureDecision.Unqualified(receipt, failures),
        };
}
```

## [03]-[ASSESSMENT_FOLD]

- Owner: `Procedure` pairs each admitted demand with WPS and welder rules, accumulates malformed evidence, evaluates canonical cases, derives inspection scope, and mints one decision receipt.
- Correspondence: `EssentialVariable.Modality` and `EssentialVariable.Quantity` gate every map at admission, so `QualificationValue.Qualify` is total over the value family and assessment carries no rule-shape fallback arm.
- Accumulation: admitted assignments are closed before assessment; independent value, rule, and applicability conflicts traverse on `Validation<Error, A>` before the result returns to `Fin`.
- Qualification: WPS/PQR tests, procedure ranges, welder ranges, WPS validity, continuity, and welder standing all contribute evidence; mismatch remains in `ProcedureDecision.Unqualified` with every row preserved.
- Projection: `ComplianceRow` carries verdict, variable identity, fault subject, scalar, and invariant evidence in its base constructor, so every new case supplies the complete projection or fails to compile.
- Inspection: each `InspectionBasis.Populations` row pairs a sampling modality with its dimensional population; policy rows derive visual, surface, volumetric, or destructive coverage with a typed sampled extent, and overlapping rules collapse to the widest coverage per family and sampling.
- Receipt: `ProcedureReceipt` carries ordered comparisons, scoped inspection requirements with their originating basis, PQR tests, per-joint personnel records, status, continuity, and welder identity; `EqualityComparer.Default.Inequalities` supplies revision and audit diffs under declared ordered collection semantics.
- Boundary: `Require` aggregates every mismatch for aborting consumers, while receipt-first consumers retain the domain decision and complete evidence.
