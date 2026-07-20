# [RASM_FABRICATION_QUALITY_RECORD]

As-built quality truth enters once through `QualityRecord.Admit(QualitySource)`, remains typed across inspection, material, process, nonconformance, calibration, and declaration evidence, and exits through `QualityReport.Seal(QualityReportRequest)`. `SealedRecord` preserves the full record set, one folded accountability census, cryptographic attestations, and the optional `EgressKind.DigitalProductPassport` identity the traveler composes.

Generated owners reject malformed identifiers, measurements, evidence sets, sampling plans, calibrations, and passports. `QualityRecord.Admit` rejects invalid certificate and declaration relations through typed `RecordRefusal` rows. `EvidenceRef` carries `CharacteristicId` only on its characteristic arm and `EvidenceId` elsewhere, preventing incompatible conversion. `CertType` and `QualityDeclaration` carry evidence and signer obligations by case.

Canonical JSON uses shared Thinktecture and NodaTime converters. `Attest<TBody>` canonicalizes, trust-admits, signs, verifies, and keys reports and passports through one algebra. `ECDsa` signs body, signer, role, credential, and instant; the trust callback binds those claims to the certificate before quorum and receipt verification. Passport attestations bind genealogy, materials, compliance, declarations, lifecycle, sustainability, and report key.

## [01]-[INDEX]

| [INDEX] | [OWNER]          | [CONTRACT]                                              |
| :-----: | :--------------- | :------------------------------------------------------ |
|  [01]   | `QUALITY_RECORD` | admitted quality evidence, attestation, and DPP sealing |

## [02]-[QUALITY_RECORD]

- Owner: `QualitySource` owns raw ingress modality; generated evidence owners own admission; `QualityRecord` owns the closed as-built family; `QualityReportRequest` owns report-versus-passport scope; `QualityReport` owns canonical sealing; `SealedRecord` owns traveler-composable evidence identity.
- Admission: `QualityRecord.Admit` absorbs native `InspectionResult`, residual samples, material certificates, process evidence, nonconformances, calibrations, declarations, and already-admitted records through one `QualitySource` union. Every nested primitive crosses a generated `Validate` seam before becoming reachable from `QualityRecord`, and one `ValidationError?.Admitted` extension is the single bridge from a generated outcome onto `Fin`.
- Cases: `QualityRecord` preserves the frozen `Inspection`, `MillCert`, `WeldInspection`, `Nonconformance`, `Calibration`, and `Conformance` wire names. Each case carries one admitted product owner rather than a positional raw row bag.
- Sampled: `QualitySource.Inspection` and `Residuals` share one `SampledLot` and project their native features into `SampleReading`, so `QualityRecord.Sampled` is the one admission pipeline both traverse. `InspectionEvidence.Features` retains inspection-source identity for traveler links; residual sampling carries no inspection feature. Per-feature `InspectionFeature.ToleranceMm` rules its own window and the lot-wide `PositionTolerance` is the fallback.
- Sampling: `SamplingPlan` carries AQL, `InspectionLevel`, `InspectionSeverity`, sample size, and the acceptance-rejection pair; `InspectionEvidence.LotVerdict` compares observed nonconformities against the severity-shifted acceptance number, and a short sample emits one `Missing` observation against `SamplingPlan.Requirement`.
- Evidence: `CharacteristicRow`, `ChemistryRow`, `MechanicalRow`, `WeldInspectionRow`, and `CalibrationRow` retain their established names and gain quantity, `CoverageInterval` uncertainty, method, equipment, personnel, procedure, acceptance, examiner grade, locus, coverage, environment, traceability, and lifecycle evidence. `Measurement.StandardUncertainty` and `ToleranceRatio` derive from the declared coverage factor.
- Reconciliation: `ProcessEvidence.Unfulfilled` diffs `ProcedureReceipt.Inspections` against the performed `WeldInspectionRow` set on joint, `InspectionFamily`, and achieved coverage; every unmet demand enters the census as a `Missing` observation instead of passing silently.
- Recall: `CalibrationRow` carries the interval `Period` and the `Impacted` record keys measured inside it, so an out-of-tolerance as-found reading is `Complete` only once its downstream impact is enumerated.
- Nonconformance: `NonconformanceEvidence` separates immediate `Correction` from systemic `CorrectiveAction`, carries the `Containment` scope and `Recurrence` link, and admits `Effectiveness` evidence exactly when a corrective action exists.
- Certificates: `CertType.En10204_2_1`, `En10204_2_2`, `En10204_3_1`, and `En10204_3_2` carry exact `EN 10204` result and representative shapes. `Requirements` derives role-only or named-representative quorum from the selected case.
- Declarations: `QualityDeclaration` carries conformity scope, PPAP level and parts, coating system and film thickness, heat-treatment cycle, or special-process procedure and operator. `QualityDeclarationKind` remains the stable projection vocabulary.
- Entry: `public static Fin<QualityRecord> QualityRecord.Admit(QualitySource source)` and `public static Fin<SealedRecord> QualityReport.Seal(QualityReportRequest request)` are the only report-creation entrypoints.
- Fold: `QualityObservation.Outcome` projects every evidence atom to one `EvidenceOutcome` policy value; `EvidenceCensus.Of` folds them into one owner whose admission proves the traced, conforming, accepted-nonconforming, rejected, incomplete, and contradictory buckets partition the row count exactly, and whose `Severity` carries the worst outcome seen.
- Refusal: `RecordRefusal` rows name every distinguishable rejection and lower onto `Op.InvalidResult(detail:)`, so the accumulating `Validation` gates report distinct causes rather than repeating one opaque code.
- Identity: `QualityReport.Canonical` serializes declaration-owned fields through one frozen converter profile. Field growth changes canonical bytes automatically; no field census, interpolation mirror, hash-code projection, or secondary codec exists.
- Attestation: `AttestationRequirement` binds role-only or named-signer obligations. `RecordAttestation` signs and verifies `AttestationPayload(Body, Signer, Role, Credential, SignedAt)` with `ECDsa`, `HashAlgorithmName.SHA384`, and `DSASignatureFormat.Rfc3279DerSequence`; receipts carry credential identity, certificate PEM, and signature bytes without private-key material.
- Passport: `PassportEvidence` carries product identity, connected acyclic genealogy whose sole terminal is that product, material and compliance links, declarations, service and repair history, manufacture and retirement instants, and typed energy, carbon, waste, recycled-content, water, renewable-energy, recyclable-mass, hazardous-substance, repairability, and durability evidence. `Attested<DigitalProductPassport>` binds that payload to the report key, canonical bytes, its own attestations, and `EgressKind.DigitalProductPassport`.
- Seams: `ProcedureReceipt`, `InspectionRequirement`, and qualification rows enter through `ProcessEvidence`; `MaterialSpec` carries mill-certificate grade identity; `CapabilityReport` remains inspection evidence; `TravelerReceiptCorpus.Records` consumes `Seq<SealedRecord>` and derives its singleton digital-product-passport projection from those records.
- Packages: owner atoms (`ContentKey`, `EgressKind`, `FabricationResult`, `InspectionFeature`, `MaterialSpec`), `Joining/procedure` (`ProcedureReceipt`, `InspectionRequirement`, `InspectionFamily`), `Spec/capability` (`CapabilityReport`), `Rasm.Analysis` (`ResidualSample`), `UnitsNet` (`IQuantity`, `Length`, `Ratio`, `Temperature`, `Energy`, `Mass`, `Volume`), `NodaTime` (`Instant`, `Interval`, `Duration`), `QuikGraph` (`AdjacencyGraph`, `Edge`, `IsDirectedAcyclicGraph`, `WeaklyConnectedComponents`, `Sinks`), `NodaTime.Serialization.SystemTextJson`, `Thinktecture.Runtime.Extensions`, `Thinktecture.Runtime.Extensions.Json`, `LanguageExt.Core`, `System.Security.Cryptography`, and `System.Text.Json`.
- Growth: a source is one `QualitySource` case; a record is one `QualityRecord` case; an observation is one `QualityObservation` case; a declaration is one `QualityDeclaration` case; a sustainability metric is one `SustainabilityEvidence` case; a refusal is one `RecordRefusal` row; a signed artifact class is one `Attested<TBody>` instantiation; an egress modality is one `ReportScope` case. Public operation count remains fixed.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Text.Json.Serialization;
using LanguageExt;
using LanguageExt.Common;
using LanguageExt.Traits;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;
using QuikGraph;
using QuikGraph.Algorithms;
using Rasm.Analysis;
using Rasm.Domain;
using Rasm.Fabrication.Joining;
using Rasm.Fabrication.Process;
using Rasm.Fabrication.Spec;
using Thinktecture;
using Thinktecture.Text.Json.Serialization;
using UnitsNet;
using UnitsNet.Units;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Documentation;

// --- [VOCABULARY] ---------------------------------------------------------------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class RecordRefusal {
    public static readonly RecordRefusal Source = new("source-absent");
    public static readonly RecordRefusal Subject = new("subject-unmapped");
    public static readonly RecordRefusal Sampling = new("sampling-plan");
    public static readonly RecordRefusal Window = new("tolerance-window");
    public static readonly RecordRefusal Evidence = new("evidence-inadmissible");
    public static readonly RecordRefusal Declaration = new("declaration-invalid");
    public static readonly RecordRefusal Lineage = new("record-lineage");
    public static readonly RecordRefusal Scope = new("scope-absent");
    public static readonly RecordRefusal Credential = new("credential-incomplete");
    public static readonly RecordRefusal Quorum = new("attestation-quorum");
    public static readonly RecordRefusal Independence = new("signer-independence");
    public static readonly RecordRefusal Canonical = new("canonical-encode");
    public static readonly RecordRefusal Signature = new("signature-unverified");
    public static readonly RecordRefusal SigningKey = new("signing-key-absent");
}

[SmartEnum<string>]
public sealed partial class TraceRelation {
    public static readonly TraceRelation ProducedFrom = new("produced-from");
    public static readonly TraceRelation AssembledInto = new("assembled-into");
    public static readonly TraceRelation CertifiedBy = new("certified-by");
    public static readonly TraceRelation InspectedBy = new("inspected-by");
    public static readonly TraceRelation MeasuredWith = new("measured-with");
    public static readonly TraceRelation DerivedFrom = new("derived-from");
    public static readonly TraceRelation SupersededBy = new("superseded-by");
    public static readonly TraceRelation ReworkedInto = new("reworked-into");
    public static readonly TraceRelation SegregatedAs = new("segregated-as");
}

[SmartEnum<string>]
public sealed partial class TestOrientation {
    public static readonly TestOrientation Longitudinal = new("longitudinal");
    public static readonly TestOrientation Transverse = new("transverse");
    public static readonly TestOrientation ThroughThickness = new("through-thickness");
    public static readonly TestOrientation WeldMetal = new("weld-metal");
    public static readonly TestOrientation FusionLine = new("fusion-line");
    public static readonly TestOrientation HeatAffectedZone = new("heat-affected-zone");
}

[SmartEnum<string>]
public sealed partial class ExaminerGrade {
    public static readonly ExaminerGrade LevelOne = new("level-1", interprets: false, approvesProcedure: false);
    public static readonly ExaminerGrade LevelTwo = new("level-2", interprets: true, approvesProcedure: false);
    public static readonly ExaminerGrade LevelThree = new("level-3", interprets: true, approvesProcedure: true);

    private ExaminerGrade(string key, bool interprets, bool approvesProcedure) : this(key) =>
        (Interprets, ApprovesProcedure) = (interprets, approvesProcedure);

    public bool Interprets { get; }
    public bool ApprovesProcedure { get; }
}

[SmartEnum<string>]
public sealed partial class EvidenceRefKind {
    public static readonly EvidenceRefKind Report = new("report");
    public static readonly EvidenceRefKind Characteristic = new("characteristic");
    public static readonly EvidenceRefKind Product = new("product");
    public static readonly EvidenceRefKind Certificate = new("certificate");
    public static readonly EvidenceRefKind Personnel = new("personnel");
    public static readonly EvidenceRefKind Procedure = new("procedure");
    public static readonly EvidenceRefKind Material = new("material");
    public static readonly EvidenceRefKind Lot = new("lot");
    public static readonly EvidenceRefKind Source = new("source");
    public static readonly EvidenceRefKind Requirement = new("requirement");
}

[SmartEnum<string>]
public sealed partial class CharacteristicClass {
    public static readonly CharacteristicClass Dimension = new("dimension", quantified: true, requiresLocus: true);
    public static readonly CharacteristicClass Geometry = new("geometry", quantified: true, requiresLocus: true);
    public static readonly CharacteristicClass Surface = new("surface", quantified: true, requiresLocus: true);
    public static readonly CharacteristicClass Material = new("material", quantified: true, requiresLocus: false);
    public static readonly CharacteristicClass Process = new("process", quantified: false, requiresLocus: false);
    public static readonly CharacteristicClass Assembly = new("assembly", quantified: true, requiresLocus: true);
    public static readonly CharacteristicClass Functional = new("functional", quantified: true, requiresLocus: false);
    public static readonly CharacteristicClass Visual = new("visual", quantified: false, requiresLocus: true);
    public static readonly CharacteristicClass Documentation = new("documentation", quantified: false, requiresLocus: false);

    private CharacteristicClass(string key, bool quantified, bool requiresLocus) : this(key) =>
        (Quantified, RequiresLocus) = (quantified, requiresLocus);

    public bool Quantified { get; }
    public bool RequiresLocus { get; }
}

[SmartEnum<string>]
public sealed partial class InspectionStage {
    public static readonly InspectionStage Receiving = new("receiving", requiresPrior: false);
    public static readonly InspectionStage FirstArticle = new("first-article", requiresPrior: false);
    public static readonly InspectionStage Setup = new("setup", requiresPrior: false);
    public static readonly InspectionStage InProcess = new("in-process", requiresPrior: false);
    public static readonly InspectionStage Final = new("final", requiresPrior: false);
    public static readonly InspectionStage Reinspection = new("reinspection", requiresPrior: true);
    public static readonly InspectionStage Surveillance = new("surveillance", requiresPrior: true);

    private InspectionStage(string key, bool requiresPrior) : this(key) => RequiresPrior = requiresPrior;

    public bool RequiresPrior { get; }
}

[SmartEnum<string>]
public sealed partial class InspectionLevel {
    public static readonly InspectionLevel Special1 = new("s-1", discrimination: 0.25);
    public static readonly InspectionLevel Special2 = new("s-2", discrimination: 0.4);
    public static readonly InspectionLevel Special3 = new("s-3", discrimination: 0.6);
    public static readonly InspectionLevel Special4 = new("s-4", discrimination: 0.8);
    public static readonly InspectionLevel General1 = new("i", discrimination: 1.0);
    public static readonly InspectionLevel General2 = new("ii", discrimination: 1.5);
    public static readonly InspectionLevel General3 = new("iii", discrimination: 2.0);
    public static readonly InspectionLevel Total = new("100-percent", discrimination: double.PositiveInfinity);

    private InspectionLevel(string key, double discrimination) : this(key) => Discrimination = discrimination;

    public double Discrimination { get; }
    public bool Census => double.IsPositiveInfinity(Discrimination);
}

[SmartEnum<string>]
public sealed partial class InspectionSeverity {
    public static readonly InspectionSeverity Normal = new("normal", sampleFactor: 1.0, acceptanceShift: 0);
    public static readonly InspectionSeverity Tightened = new("tightened", sampleFactor: 1.0, acceptanceShift: -1);
    public static readonly InspectionSeverity Reduced = new("reduced", sampleFactor: 0.4, acceptanceShift: 0);

    private InspectionSeverity(string key, double sampleFactor, int acceptanceShift) : this(key) =>
        (SampleFactor, AcceptanceShift) = (sampleFactor, acceptanceShift);

    public double SampleFactor { get; }
    public int AcceptanceShift { get; }
}

[SmartEnum<string>]
public sealed partial class Disposition {
    public static readonly Disposition Conform = new("conform", conforming: true, accepted: true, terminal: true, requiresAuthority: false);
    public static readonly Disposition UseAsIs = new("use-as-is", conforming: false, accepted: true, terminal: true, requiresAuthority: true);
    public static readonly Disposition Repair = new("repair", conforming: false, accepted: false, terminal: false, requiresAuthority: true);
    public static readonly Disposition Rework = new("rework", conforming: false, accepted: false, terminal: false, requiresAuthority: true);
    public static readonly Disposition ReturnToSupplier = new("return-to-supplier", conforming: false, accepted: false, terminal: true, requiresAuthority: true);
    public static readonly Disposition Reject = new("reject", conforming: false, accepted: false, terminal: true, requiresAuthority: true);
    public static readonly Disposition Scrap = new("scrap", conforming: false, accepted: false, terminal: true, requiresAuthority: true);
    public static readonly Disposition PendingReview = new("pending-review", conforming: false, accepted: false, terminal: false, requiresAuthority: true);

    private Disposition(string key, bool conforming, bool accepted, bool terminal, bool requiresAuthority) : this(key) =>
        (Conforming, Accepted, Terminal, RequiresAuthority) = (conforming, accepted, terminal, requiresAuthority);

    public bool Conforming { get; }
    public bool Accepted { get; }
    public bool Terminal { get; }
    public bool RequiresAuthority { get; }
}

[SmartEnum<string>]
public sealed partial class DecisionRule {
    public static readonly DecisionRule SimpleAcceptance = new("simple-acceptance", guardBandFactor: 0.0);
    public static readonly DecisionRule SharedRisk = new("shared-risk", guardBandFactor: 0.5);
    public static readonly DecisionRule GuardBand = new("guard-band", guardBandFactor: 1.0);

    private DecisionRule(string key, double guardBandFactor) : this(key) => GuardBandFactor = guardBandFactor;

    public double GuardBandFactor { get; }
}

[SmartEnum<string>]
public sealed partial class CoverageInterval {
    public static readonly CoverageInterval Standard = new("k1", factor: 1.0, confidence: 0.6827);
    public static readonly CoverageInterval Nominal95 = new("k1.96", factor: 1.96, confidence: 0.95);
    public static readonly CoverageInterval Expanded = new("k2", factor: 2.0, confidence: 0.9545);
    public static readonly CoverageInterval Critical = new("k3", factor: 3.0, confidence: 0.9973);

    private CoverageInterval(string key, double factor, double confidence) : this(key) =>
        (Factor, Confidence) = (factor, confidence);

    public double Factor { get; }
    public double Confidence { get; }
}

[SmartEnum<string>]
public sealed partial class NdtMethod {
    public static readonly NdtMethod Visual = new("visual", InspectionFamily.Visual, radiationControls: false);
    public static readonly NdtMethod LiquidPenetrant = new("liquid-penetrant", InspectionFamily.Surface, radiationControls: false);
    public static readonly NdtMethod MagneticParticle = new("magnetic-particle", InspectionFamily.Surface, radiationControls: false);
    public static readonly NdtMethod EddyCurrent = new("eddy-current", InspectionFamily.Surface, radiationControls: false);
    public static readonly NdtMethod Thermography = new("thermography", InspectionFamily.Surface, radiationControls: false);
    public static readonly NdtMethod Ultrasonic = new("ultrasonic", InspectionFamily.Volumetric, radiationControls: false);
    public static readonly NdtMethod PhasedArray = new("phased-array", InspectionFamily.Volumetric, radiationControls: false);
    public static readonly NdtMethod TimeOfFlightDiffraction = new("time-of-flight-diffraction", InspectionFamily.Volumetric, radiationControls: false);
    public static readonly NdtMethod Radiographic = new("radiographic", InspectionFamily.Volumetric, radiationControls: true);
    public static readonly NdtMethod Leak = new("leak", InspectionFamily.Volumetric, radiationControls: false);
    public static readonly NdtMethod AcousticEmission = new("acoustic-emission", InspectionFamily.Volumetric, radiationControls: false);
    public static readonly NdtMethod Macrosection = new("macrosection", InspectionFamily.Destructive, radiationControls: false);
    public static readonly NdtMethod Hardness = new("hardness", InspectionFamily.Destructive, radiationControls: false);
    public static readonly NdtMethod BendTest = new("bend-test", InspectionFamily.Destructive, radiationControls: false);

    private NdtMethod(string key, InspectionFamily family, bool radiationControls) : this(key) =>
        (Family, RadiationControls) = (family, radiationControls);

    public InspectionFamily Family { get; }
    public bool RadiationControls { get; }
    public bool Volumetric => Family.Subsurface;
    public bool SurfaceBreaking => Family == InspectionFamily.Surface;
    public bool Destructive => Family.ConsumesPart;
}

[SmartEnum<string>]
public sealed partial class QualityDeclarationKind {
    public static readonly QualityDeclarationKind CertificateOfConformity = new("certificate-of-conformity");
    public static readonly QualityDeclarationKind ProductionPartApproval = new("production-part-approval");
    public static readonly QualityDeclarationKind Coating = new("coating");
    public static readonly QualityDeclarationKind HeatTreatment = new("heat-treatment");
    public static readonly QualityDeclarationKind SpecialProcess = new("special-process");
}

[SmartEnum<int>]
public sealed partial class PpapLevel {
    public static readonly PpapLevel One = new(1);
    public static readonly PpapLevel Two = new(2);
    public static readonly PpapLevel Three = new(3);
    public static readonly PpapLevel Four = new(4);
    public static readonly PpapLevel Five = new(5);
}

[SmartEnum<string>]
public sealed partial class AttestationRole {
    public static readonly AttestationRole Manufacturer = new("manufacturer", independent: false);
    public static readonly AttestationRole ManufacturerAuthorized = new("manufacturer-authorized", independent: false);
    public static readonly AttestationRole Purchaser = new("purchaser", independent: true);
    public static readonly AttestationRole Independent = new("independent", independent: true);
    public static readonly AttestationRole Quality = new("quality", independent: false);
    public static readonly AttestationRole Regulator = new("regulator", independent: true);
    public static readonly AttestationRole WeldingInspector = new("welding-inspector", independent: true);
    public static readonly AttestationRole CalibrationLaboratory = new("calibration-laboratory", independent: true);
    public static readonly AttestationRole MaterialReviewBoard = new("material-review-board", independent: true);
    public static readonly AttestationRole SustainabilityVerifier = new("sustainability-verifier", independent: true);

    private AttestationRole(string key, bool independent) : this(key) => IndependentAuthority = independent;

    public bool IndependentAuthority { get; }
}

[SmartEnum<string>]
public sealed partial class EvidenceOutcome {
    public static readonly EvidenceOutcome Trace = new("trace", rank: 0, measured: 0, traced: 1, conforming: 0, accepted: 0, rejected: 0, incomplete: 0, contradictions: 0);
    public static readonly EvidenceOutcome Conforming = new("conforming", rank: 1, measured: 1, traced: 0, conforming: 1, accepted: 0, rejected: 0, incomplete: 0, contradictions: 0);
    public static readonly EvidenceOutcome Incomplete = new("incomplete", rank: 2, measured: 0, traced: 0, conforming: 0, accepted: 0, rejected: 0, incomplete: 1, contradictions: 0);
    public static readonly EvidenceOutcome AcceptedNonconforming = new("accepted-nonconforming", rank: 3, measured: 1, traced: 0, conforming: 0, accepted: 1, rejected: 0, incomplete: 0, contradictions: 0);
    public static readonly EvidenceOutcome Rejected = new("rejected", rank: 4, measured: 1, traced: 0, conforming: 0, accepted: 0, rejected: 1, incomplete: 0, contradictions: 0);
    public static readonly EvidenceOutcome Contradiction = new("contradiction", rank: 5, measured: 1, traced: 0, conforming: 0, accepted: 0, rejected: 0, incomplete: 0, contradictions: 1);

    private EvidenceOutcome(
        string key,
        int rank,
        int measured,
        int traced,
        int conforming,
        int accepted,
        int rejected,
        int incomplete,
        int contradictions) : this(key) =>
        (Rank, Measured, Traced, Conforming, Accepted, Rejected, Incomplete, Contradictions) =
        (rank, measured, traced, conforming, accepted, rejected, incomplete, contradictions);

    public int Rank { get; }
    public int Measured { get; }
    public int Traced { get; }
    public int Conforming { get; }
    public int Accepted { get; }
    public int Rejected { get; }
    public int Incomplete { get; }
    public int Contradictions { get; }

    internal static EvidenceOutcome Worst(EvidenceOutcome left, EvidenceOutcome right) =>
        left.Rank >= right.Rank ? left : right;
}

// --- [ADMISSION] ----------------------------------------------------------------------------------------------------------------------------------
[ValueObject<string>]
public readonly partial struct HeatNumber {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        value = value?.Trim() ?? string.Empty;
        validationError = value.Length == 0 ? new ValidationError(message: "heat-number") : null;
    }
}

[ValueObject<string>]
public readonly partial struct NonconformanceNumber {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        value = value?.Trim() ?? string.Empty;
        validationError = value.Length == 0 ? new ValidationError(message: "nonconformance-number") : null;
    }
}

[ValueObject<string>]
public readonly partial struct AssetTag {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        value = value?.Trim() ?? string.Empty;
        validationError = value.Length == 0 ? new ValidationError(message: "asset-tag") : null;
    }
}

[ValueObject<string>]
public sealed partial class EvidenceId {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        value = value?.Trim() ?? string.Empty;
        validationError = value.Length == 0 ? new ValidationError(message: "evidence-id") : null;
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record EvidenceRef {
    private EvidenceRef(EvidenceRefKind kind) => Kind = kind;

    public EvidenceRefKind Kind { get; }

    public sealed record Report(EvidenceId Id) : EvidenceRef(EvidenceRefKind.Report);
    public sealed record Characteristic(CharacteristicId Id) : EvidenceRef(EvidenceRefKind.Characteristic);
    public sealed record Product(EvidenceId Id) : EvidenceRef(EvidenceRefKind.Product);
    public sealed record Certificate(EvidenceId Id) : EvidenceRef(EvidenceRefKind.Certificate);
    public sealed record Personnel(EvidenceId Id) : EvidenceRef(EvidenceRefKind.Personnel);
    public sealed record Procedure(EvidenceId Id) : EvidenceRef(EvidenceRefKind.Procedure);
    public sealed record Material(EvidenceId Id) : EvidenceRef(EvidenceRefKind.Material);
    public sealed record Lot(EvidenceId Id) : EvidenceRef(EvidenceRefKind.Lot);
    public sealed record Source(EvidenceId Id) : EvidenceRef(EvidenceRefKind.Source);
    public sealed record Requirement(EvidenceId Id) : EvidenceRef(EvidenceRefKind.Requirement);
}

[ValueObject<Seq<EvidenceRef>>]
public sealed partial class EvidenceLinks {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Seq<EvidenceRef> value) =>
        validationError = value.IsEmpty || value.Exists(static item => item is null) || value.Distinct().Count != value.Count
            ? new ValidationError(message: "evidence-links")
            : null;
}

[ComplexValueObject]
public sealed partial class EvidenceContext {
    public EvidenceRef.Personnel Actor { get; }
    public Option<AssetTag> Equipment { get; }
    public Option<EvidenceRef.Procedure> Procedure { get; }
    public string Method { get; }
    public Option<string> Locus { get; }
    public Instant At { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref EvidenceRef.Personnel actor,
        ref Option<AssetTag> equipment,
        ref Option<EvidenceRef.Procedure> procedure,
        ref string method,
        ref Option<string> locus,
        ref Instant at) {
        method = method?.Trim() ?? string.Empty;
        locus = locus.Map(static value => value?.Trim() ?? string.Empty).Filter(static value => value.Length > 0);
        validationError = actor is null || method.Length == 0 || procedure.Exists(static value => value is null)
            ? new ValidationError(message: "evidence-context")
            : null;
    }
}

[ComplexValueObject]
public sealed partial class Measurement {
    public IQuantity Nominal { get; }
    public IQuantity Observed { get; }
    public IQuantity Lower { get; }
    public IQuantity Upper { get; }
    public IQuantity ExpandedUncertainty { get; }
    public CoverageInterval Coverage { get; }
    public DecisionRule DecisionRule { get; }
    public EvidenceContext Context { get; }
    public double GuardBand => DecisionRule.GuardBandFactor * ExpandedUncertainty.As(Observed.Unit);
    public double StandardUncertainty => ExpandedUncertainty.As(Observed.Unit) / Coverage.Factor;
    public double ToleranceRatio => StandardUncertainty > 0.0
        ? (Upper.As(Observed.Unit) - Lower.As(Observed.Unit)) / (2.0 * ExpandedUncertainty.As(Observed.Unit))
        : double.PositiveInfinity;
    public bool Within =>
        Lower.As(Observed.Unit) + GuardBand <= (double)Observed.Value
        && (double)Observed.Value <= Upper.As(Observed.Unit) - GuardBand;

    internal static Fin<Measurement> Admit(
        IQuantity nominal,
        IQuantity observed,
        IQuantity lower,
        IQuantity upper,
        IQuantity expandedUncertainty,
        CoverageInterval coverage,
        DecisionRule decisionRule,
        EvidenceContext context) =>
        Validate(nominal, observed, lower, upper, expandedUncertainty, coverage, decisionRule, context, out Measurement admitted)
            .Admitted(admitted, RecordRefusal.Window);

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref IQuantity nominal,
        ref IQuantity observed,
        ref IQuantity lower,
        ref IQuantity upper,
        ref IQuantity expandedUncertainty,
        ref CoverageInterval coverage,
        ref DecisionRule decisionRule,
        ref EvidenceContext context) =>
        validationError = nominal is null || observed is null || lower is null || upper is null || expandedUncertainty is null
            || coverage is null || decisionRule is null || context is null
            || nominal.QuantityInfo != observed.QuantityInfo || lower.QuantityInfo != observed.QuantityInfo || upper.QuantityInfo != observed.QuantityInfo
            || expandedUncertainty.QuantityInfo != observed.QuantityInfo
            || !double.IsFinite((double)nominal.Value) || !double.IsFinite((double)observed.Value)
            || !double.IsFinite((double)lower.Value) || !double.IsFinite((double)upper.Value)
            || lower.As(observed.Unit) > upper.As(observed.Unit)
            || !double.IsFinite((double)expandedUncertainty.Value) || expandedUncertainty.As(observed.Unit) < 0.0
            || lower.As(observed.Unit) + decisionRule.GuardBandFactor * expandedUncertainty.As(observed.Unit)
                > upper.As(observed.Unit) - decisionRule.GuardBandFactor * expandedUncertainty.As(observed.Unit)
                ? new ValidationError(message: "measurement")
                : null;
}

[ComplexValueObject]
public sealed partial class CharacteristicSubject {
    public EvidenceRef.Characteristic Characteristic { get; }
    public EvidenceRef.Requirement Requirement { get; }
    public CharacteristicClass Class { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref EvidenceRef.Characteristic characteristic,
        ref EvidenceRef.Requirement requirement,
        ref CharacteristicClass @class) =>
        validationError = characteristic is null || requirement is null || @class is null
            ? new ValidationError(message: "characteristic-subject")
            : null;
}

[ComplexValueObject]
public sealed partial class CharacteristicRow {
    public CharacteristicSubject Subject { get; }
    public Measurement Measurement { get; }
    public Disposition Verdict { get; }

    internal static Fin<CharacteristicRow> Admit(CharacteristicSubject subject, Measurement measurement, Disposition verdict) =>
        Validate(subject, measurement, verdict, out CharacteristicRow admitted).Admitted(admitted, RecordRefusal.Evidence);

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref CharacteristicSubject subject,
        ref Measurement measurement,
        ref Disposition verdict) =>
        validationError = subject is null || measurement is null || verdict is null
            || !subject.Class.Quantified
            || subject.Class.RequiresLocus && measurement.Context.Locus.IsNone
                ? new ValidationError(message: "characteristic-row")
                : null;
}

[ComplexValueObject]
public sealed partial class ChemistryRow {
    public string Element { get; }
    public Ratio Observed { get; }
    public Ratio Lower { get; }
    public Ratio Upper { get; }
    public EvidenceContext Context { get; }
    public Disposition Verdict { get; }
    public bool Within => Lower <= Observed && Observed <= Upper;

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref string element,
        ref Ratio observed,
        ref Ratio lower,
        ref Ratio upper,
        ref EvidenceContext context,
        ref Disposition verdict) {
        element = element?.Trim() ?? string.Empty;
        validationError = element.Length == 0 || lower > upper || !QualityReport.ValidFraction(observed)
            || !QualityReport.ValidFraction(lower) || !QualityReport.ValidFraction(upper)
            || context is null || verdict is null
                ? new ValidationError(message: "chemistry-row")
                : null;
    }
}

[ComplexValueObject]
public sealed partial class CategoricalEvidence {
    public EvidenceRef.Characteristic Characteristic { get; }
    public CharacteristicClass Class { get; }
    public Seq<string> Admitted { get; }
    public string Observed { get; }
    public EvidenceContext Context { get; }
    public bool Within => Admitted.Exists(value => string.Equals(value, Observed, StringComparison.OrdinalIgnoreCase));

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref EvidenceRef.Characteristic characteristic,
        ref CharacteristicClass @class,
        ref Seq<string> admitted,
        ref string observed,
        ref EvidenceContext context) {
        observed = observed?.Trim() ?? string.Empty;
        validationError = characteristic is null || @class is null || context is null
            || @class.Quantified || admitted.IsEmpty || admitted.Exists(static value => string.IsNullOrWhiteSpace(value))
            || admitted.Map(static value => value.ToUpperInvariant()).Distinct().Count != admitted.Count || observed.Length == 0
            || @class.RequiresLocus && context.Locus.IsNone
                ? new ValidationError(message: "categorical-evidence")
                : null;
    }
}

[ComplexValueObject]
public sealed partial class TraceEvidence {
    public EvidenceRef Subject { get; }
    public EvidenceRef Source { get; }
    public TraceRelation Relation { get; }
    public EvidenceContext Context { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref EvidenceRef subject,
        ref EvidenceRef source,
        ref TraceRelation relation,
        ref EvidenceContext context) =>
        validationError = subject is null || source is null || relation is null || context is null || subject == source
            ? new ValidationError(message: "trace-evidence")
            : null;
}

// --- [EVIDENCE] -----------------------------------------------------------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record QualityObservation {
    private QualityObservation() { }

    public sealed record Characteristic(CharacteristicRow Row) : QualityObservation;
    public sealed record Chemistry(ChemistryRow Row) : QualityObservation;
    public sealed record Categorical(CategoricalEvidence Row, Disposition Verdict) : QualityObservation;
    public sealed record Ndt(WeldInspectionRow Row) : QualityObservation;
    public sealed record Calibration(CalibrationRow Row) : QualityObservation;
    public sealed record Trace(TraceEvidence Row) : QualityObservation;
    public sealed record Missing(EvidenceRef.Requirement Requirement, EvidenceContext Context) : QualityObservation;

    public EvidenceOutcome Outcome => Switch(
        characteristic: static value => Classify(value.Row.Measurement.Within, value.Row.Verdict),
        chemistry: static value => Classify(value.Row.Within, value.Row.Verdict),
        categorical: static value => Classify(value.Row.Within, value.Verdict),
        ndt: static value => value.Row.Complete
            ? EvidenceOutcome.Worst(value.Row.Findings.Outcome, Classify(within: true, verdict: value.Row.Verdict))
            : EvidenceOutcome.Incomplete,
        calibration: static value => value.Row.Complete ? Classify(value.Row.Within, value.Row.Verdict) : EvidenceOutcome.Incomplete,
        trace: static _ => EvidenceOutcome.Trace,
        missing: static _ => EvidenceOutcome.Incomplete);

    private static EvidenceOutcome Classify(bool within, Disposition verdict) => (within, verdict.Conforming, verdict.Accepted) switch {
        (true, true, _) => EvidenceOutcome.Conforming,
        (false, true, _) => EvidenceOutcome.Contradiction,
        (_, _, true) => EvidenceOutcome.AcceptedNonconforming,
        _ => EvidenceOutcome.Rejected,
    };
}

[ValueObject<Seq<QualityObservation>>]
public sealed partial class EvidenceSet {
    public bool HasContradiction => Outcome == EvidenceOutcome.Contradiction;
    public EvidenceOutcome Outcome => ToValue().Fold(
        EvidenceOutcome.Trace,
        static (state, observation) => EvidenceOutcome.Worst(state, observation.Outcome));

    internal static Fin<EvidenceSet> Admit(Seq<QualityObservation> observations) =>
        Validate(observations, out EvidenceSet admitted).Admitted(admitted, RecordRefusal.Evidence);

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Seq<QualityObservation> value) =>
        validationError = value.IsEmpty || value.Exists(static observation => observation is null)
            ? new ValidationError(message: "evidence-set")
            : null;
}

[ComplexValueObject]
public sealed partial class EvidenceCensus {
    public int Rows { get; }
    public int Measured { get; }
    public int Traced { get; }
    public int Conforming { get; }
    public int AcceptedNonconforming { get; }
    public int Rejected { get; }
    public int Incomplete { get; }
    public int Contradictions { get; }
    public EvidenceOutcome Severity { get; }

    internal static EvidenceCensus Of(Seq<QualityObservation> observations) =>
        observations.Map(static observation => observation.Outcome).Fold(
            Create(0, 0, 0, 0, 0, 0, 0, 0, EvidenceOutcome.Trace),
            static (state, outcome) => Create(
                state.Rows + 1,
                state.Measured + outcome.Measured,
                state.Traced + outcome.Traced,
                state.Conforming + outcome.Conforming,
                state.AcceptedNonconforming + outcome.Accepted,
                state.Rejected + outcome.Rejected,
                state.Incomplete + outcome.Incomplete,
                state.Contradictions + outcome.Contradictions,
                EvidenceOutcome.Worst(state.Severity, outcome)));

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref int rows,
        ref int measured,
        ref int traced,
        ref int conforming,
        ref int acceptedNonconforming,
        ref int rejected,
        ref int incomplete,
        ref int contradictions,
        ref EvidenceOutcome severity) =>
        validationError = severity is null || rows < 0 || measured < 0 || traced < 0 || conforming < 0
            || acceptedNonconforming < 0 || rejected < 0 || incomplete < 0 || contradictions < 0
            || traced + conforming + acceptedNonconforming + rejected + incomplete + contradictions != rows
            || measured != conforming + acceptedNonconforming + rejected + contradictions
                ? new ValidationError(message: "evidence-census")
                : null;
}

[ComplexValueObject]
public sealed partial class MechanicalRow {
    public TestOrientation Orientation { get; }
    public EvidenceRef.Requirement Standard { get; }
    public Option<Temperature> TestTemperature { get; }
    public EvidenceSet Properties { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref TestOrientation orientation,
        ref EvidenceRef.Requirement standard,
        ref Option<Temperature> testTemperature,
        ref EvidenceSet properties) =>
        validationError = orientation is null || standard is null || properties is null
            ? new ValidationError(message: "mechanical-row")
            : null;
}

[ComplexValueObject]
public sealed partial class MaterialResults {
    public Seq<ChemistryRow> Chemistry { get; }
    public Seq<MechanicalRow> Mechanicals { get; }
    public Seq<QualityObservation> Observations =>
        Chemistry.Map(static row => (QualityObservation)new QualityObservation.Chemistry(row))
        + Mechanicals.Bind(static row => row.Properties.ToValue());

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Seq<ChemistryRow> chemistry,
        ref Seq<MechanicalRow> mechanicals) =>
        validationError = chemistry.IsEmpty && mechanicals.IsEmpty
            || chemistry.Exists(static row => row is null) || mechanicals.Exists(static row => row is null)
                ? new ValidationError(message: "material-results")
                : null;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CertType {
    private CertType() { }

    public sealed record En10204_2_1(EvidenceRef.Source Issuer, QualityDeclaration.Conformity Declaration) : CertType;
    public sealed record En10204_2_2(EvidenceRef.Source Issuer, MaterialResults Results) : CertType;
    public sealed record En10204_3_1(
        EvidenceRef.Source Issuer,
        MaterialResults Results,
        EvidenceRef.Personnel ManufacturerRepresentative) : CertType;
    public sealed record En10204_3_2(
        EvidenceRef.Source Issuer,
        MaterialResults Results,
        EvidenceRef.Personnel ManufacturerRepresentative,
        EvidenceRef.Personnel IndependentRepresentative) : CertType;

    public Seq<QualityObservation> Observations => Switch(
        en10204_2_1: static value => value.Declaration.Observations,
        en10204_2_2: static value => value.Results.Observations,
        en10204_3_1: static value => value.Results.Observations,
        en10204_3_2: static value => value.Results.Observations);

    public Seq<AttestationRequirement> Requirements => Switch(
        en10204_2_1: static value => value.Declaration.Requirements
            + Seq<AttestationRequirement>(new AttestationRequirement.Role(AttestationRole.Manufacturer)),
        en10204_2_2: static _ => Seq<AttestationRequirement>(new AttestationRequirement.Role(AttestationRole.Manufacturer)),
        en10204_3_1: static value => Seq<AttestationRequirement>(
            new AttestationRequirement.Signer(value.ManufacturerRepresentative, AttestationRole.ManufacturerAuthorized)),
        en10204_3_2: static value => Seq<AttestationRequirement>(
            new AttestationRequirement.Signer(value.ManufacturerRepresentative, AttestationRole.ManufacturerAuthorized),
            new AttestationRequirement.Signer(value.IndependentRepresentative, AttestationRole.Independent)));

    public bool Valid => Switch(
        en10204_2_1: static value => value.Issuer is not null && value.Declaration is not null && value.Declaration.Valid,
        en10204_2_2: static value => value.Issuer is not null && value.Results is not null,
        en10204_3_1: static value => value.Issuer is not null && value.Results is not null
            && value.ManufacturerRepresentative is not null,
        en10204_3_2: static value => value.Issuer is not null && value.Results is not null
            && value.ManufacturerRepresentative is not null && value.IndependentRepresentative is not null
            && value.ManufacturerRepresentative != value.IndependentRepresentative);
}

[ComplexValueObject]
public sealed partial class WeldInspectionRow {
    public int Joint { get; }
    public NdtMethod Method { get; }
    public Ratio Coverage { get; }
    public Ratio RequiredCoverage { get; }
    public EvidenceRef.Procedure Procedure { get; }
    public EvidenceRef.Requirement Acceptance { get; }
    public EvidenceRef.Personnel Examiner { get; }
    public ExaminerGrade Grade { get; }
    public EvidenceSet Findings { get; }
    public Disposition Verdict { get; }
    public Instant At { get; }
    public bool Complete => Coverage >= RequiredCoverage;

    internal bool Satisfies(InspectionRequirement demand) =>
        demand.Joint == Joint && demand.Family == Method.Family
        && Coverage.As(RatioUnit.DecimalFraction) >= demand.Coverage;

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref int joint,
        ref NdtMethod method,
        ref Ratio coverage,
        ref Ratio requiredCoverage,
        ref EvidenceRef.Procedure procedure,
        ref EvidenceRef.Requirement acceptance,
        ref EvidenceRef.Personnel examiner,
        ref ExaminerGrade grade,
        ref EvidenceSet findings,
        ref Disposition verdict,
        ref Instant at) =>
        validationError = joint < 0 || method is null
            || !QualityReport.ValidFraction(coverage)
            || !QualityReport.ValidFraction(requiredCoverage, positive: true)
            || procedure is null || acceptance is null || examiner is null || grade is null
            || findings is null || verdict is null || !grade.Interprets
                ? new ValidationError(message: "weld-inspection-row")
                : null;
}

[ComplexValueObject]
public sealed partial class CalibrationRow {
    public AssetTag Asset { get; }
    public EvidenceRef.Procedure Procedure { get; }
    public IQuantity AsFoundError { get; }
    public IQuantity AllowedError { get; }
    public Option<IQuantity> AsLeftError { get; }
    public IQuantity ExpandedUncertainty { get; }
    public CoverageInterval Coverage { get; }
    public DecisionRule DecisionRule { get; }
    public ContentKey StandardCertificate { get; }
    public EvidenceContext Context { get; }
    public Option<Temperature> AmbientTemperature { get; }
    public Option<Ratio> AmbientHumidity { get; }
    public Interval Period { get; }
    public Seq<ContentKey> Impacted { get; }
    public Instant DueAt { get; }
    public Disposition Verdict { get; }
    public IQuantity EffectiveError => AsLeftError.IfNone(AsFoundError);
    public double TestUncertaintyRatio => ExpandedUncertainty.As(AllowedError.Unit) > 0.0
        ? Math.Abs((double)AllowedError.Value) / ExpandedUncertainty.As(AllowedError.Unit)
        : double.PositiveInfinity;
    public bool AsFoundWithin => Bounded(AsFoundError);
    public bool Within => Bounded(EffectiveError);
    public bool Complete => Within && (AsFoundWithin || !Impacted.IsEmpty);

    private bool Bounded(IQuantity error) => Math.Abs(error.As(AllowedError.Unit))
        + DecisionRule.GuardBandFactor * ExpandedUncertainty.As(AllowedError.Unit) <= Math.Abs((double)AllowedError.Value);

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref AssetTag asset,
        ref EvidenceRef.Procedure procedure,
        ref IQuantity asFoundError,
        ref IQuantity allowedError,
        ref Option<IQuantity> asLeftError,
        ref IQuantity expandedUncertainty,
        ref CoverageInterval coverage,
        ref DecisionRule decisionRule,
        ref ContentKey standardCertificate,
        ref EvidenceContext context,
        ref Option<Temperature> ambientTemperature,
        ref Option<Ratio> ambientHumidity,
        ref Interval period,
        ref Seq<ContentKey> impacted,
        ref Instant dueAt,
        ref Disposition verdict) =>
        validationError = procedure is null || asFoundError is null || allowedError is null
            || asFoundError.QuantityInfo != allowedError.QuantityInfo
            || asLeftError.Exists(value => value is null || value.QuantityInfo != allowedError.QuantityInfo)
            || expandedUncertainty is null || expandedUncertainty.QuantityInfo != allowedError.QuantityInfo
            || !double.IsFinite((double)asFoundError.Value) || !double.IsFinite((double)allowedError.Value)
            || asLeftError.Exists(static value => !double.IsFinite((double)value.Value))
            || !double.IsFinite((double)expandedUncertainty.Value) || expandedUncertainty.As(allowedError.Unit) < 0.0
            || coverage is null || decisionRule is null || (double)allowedError.Value <= 0.0
            || standardCertificate is null || context is null || dueAt <= context.At || verdict is null
            || ambientHumidity.Exists(static value => !QualityReport.ValidFraction(value))
            || !period.HasStart || !period.HasEnd || period.End != context.At
            || impacted.Exists(static key => key is null) || impacted.Distinct().Count != impacted.Count
                ? new ValidationError(message: "calibration-row")
                : null;
}

// --- [DECLARATIONS] -------------------------------------------------------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record QualityDeclaration {
    private QualityDeclaration() { }

    public sealed record Conformity(EvidenceRef.Certificate Certificate, EvidenceLinks Scope, EvidenceSet Evidence) : QualityDeclaration;
    public sealed record ProductionPartApproval(
        EvidenceRef.Certificate Submission,
        PpapLevel Level,
        EvidenceLinks Parts,
        EvidenceSet Evidence) : QualityDeclaration;
    public sealed record Coating(
        EvidenceRef.Certificate Certificate,
        EvidenceRef.Requirement System,
        Length DryFilmThickness,
        EvidenceRef.Requirement SurfacePreparation,
        EvidenceSet Evidence) : QualityDeclaration;
    public sealed record HeatTreatment(
        EvidenceRef.Certificate Certificate,
        EvidenceRef.Requirement Cycle,
        Temperature Soak,
        NodaTime.Duration Dwell,
        EvidenceRef.Requirement Cooling,
        EvidenceSet Evidence) : QualityDeclaration;
    public sealed record SpecialProcess(
        EvidenceRef.Certificate Certificate,
        EvidenceRef.Procedure Procedure,
        EvidenceRef.Personnel Operator,
        EvidenceRef.Requirement Process,
        EvidenceSet Evidence) : QualityDeclaration;

    public QualityDeclarationKind Kind => Switch(
        conformity: static _ => QualityDeclarationKind.CertificateOfConformity,
        productionPartApproval: static _ => QualityDeclarationKind.ProductionPartApproval,
        coating: static _ => QualityDeclarationKind.Coating,
        heatTreatment: static _ => QualityDeclarationKind.HeatTreatment,
        specialProcess: static _ => QualityDeclarationKind.SpecialProcess);

    public Seq<QualityObservation> Observations => Switch(
        conformity: static value => value.Evidence.ToValue(),
        productionPartApproval: static value => value.Evidence.ToValue(),
        coating: static value => value.Evidence.ToValue(),
        heatTreatment: static value => value.Evidence.ToValue(),
        specialProcess: static value => value.Evidence.ToValue());

    public Seq<AttestationRequirement> Requirements => Switch(
        conformity: static _ => Seq<AttestationRequirement>(new AttestationRequirement.Role(AttestationRole.Manufacturer)),
        productionPartApproval: static _ => Seq<AttestationRequirement>(
            new AttestationRequirement.Role(AttestationRole.Manufacturer),
            new AttestationRequirement.Role(AttestationRole.Purchaser)),
        coating: static _ => Seq<AttestationRequirement>(new AttestationRequirement.Role(AttestationRole.ManufacturerAuthorized)),
        heatTreatment: static _ => Seq<AttestationRequirement>(new AttestationRequirement.Role(AttestationRole.ManufacturerAuthorized)),
        specialProcess: static value => Seq<AttestationRequirement>(
            new AttestationRequirement.Signer(value.Operator, AttestationRole.ManufacturerAuthorized)));

    public bool Valid => Switch(
        conformity: static value => value.Certificate is not null && value.Scope is not null && value.Evidence is not null,
        productionPartApproval: static value => value.Submission is not null && value.Level is not null
            && value.Parts is not null && value.Evidence is not null,
        coating: static value => value.Certificate is not null && value.System is not null
            && value.DryFilmThickness > Length.Zero && value.SurfacePreparation is not null && value.Evidence is not null,
        heatTreatment: static value => value.Certificate is not null && value.Cycle is not null && value.Dwell > NodaTime.Duration.Zero
            && value.Cooling is not null && value.Evidence is not null,
        specialProcess: static value => value.Certificate is not null && value.Procedure is not null
            && value.Operator is not null && value.Process is not null && value.Evidence is not null);
}

// --- [RECORDS] ------------------------------------------------------------------------------------------------------------------------------------
[ComplexValueObject]
public sealed partial class SamplingPlan {
    public EvidenceRef.Requirement Requirement { get; }
    public Ratio AcceptanceQuality { get; }
    public InspectionLevel Level { get; }
    public InspectionSeverity Severity { get; }
    public int SampleSize { get; }
    public int Accept { get; }
    public int Reject { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref EvidenceRef.Requirement requirement,
        ref Ratio acceptanceQuality,
        ref InspectionLevel level,
        ref InspectionSeverity severity,
        ref int sampleSize,
        ref int accept,
        ref int reject) =>
        validationError = requirement is null || level is null || severity is null
            || !QualityReport.ValidFraction(acceptanceQuality)
            || sampleSize < 1 || accept < 0 || reject != accept + 1 || accept >= sampleSize
            || level.Census && accept != 0
                ? new ValidationError(message: "sampling-plan")
                : null;
}

[ComplexValueObject]
public sealed partial class InspectionEvidence {
    public EvidenceRef.Report Report { get; }
    public EvidenceRef.Product Product { get; }
    public InspectionStage Stage { get; }
    public SamplingPlan Plan { get; }
    public int LotSize { get; }
    public Seq<CharacteristicRow> Characteristics { get; }
    public Seq<InspectionFeature> Features { get; }
    public Option<CapabilityReport> Capability { get; }
    public Option<ContentKey> Prior { get; }
    public Instant SampledAt { get; }
    public int Nonconforming => Characteristics.Filter(static row => !row.Measurement.Within).Count;
    public Disposition LotVerdict => Nonconforming <= Plan.Accept + Plan.Severity.AcceptanceShift
        ? Disposition.Conform
        : Disposition.PendingReview;
    public Seq<QualityObservation> Observations =>
        Characteristics.Map(static row => (QualityObservation)new QualityObservation.Characteristic(row))
        + (Characteristics.Count < Plan.SampleSize
            ? Characteristics.Take(1).Map(row => (QualityObservation)new QualityObservation.Missing(Plan.Requirement, row.Measurement.Context))
            : Seq<QualityObservation>());

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref EvidenceRef.Report report,
        ref EvidenceRef.Product product,
        ref InspectionStage stage,
        ref SamplingPlan plan,
        ref int lotSize,
        ref Seq<CharacteristicRow> characteristics,
        ref Seq<InspectionFeature> features,
        ref Option<CapabilityReport> capability,
        ref Option<ContentKey> prior,
        ref Instant sampledAt) =>
        validationError = report is null || product is null || stage is null || plan is null
            || lotSize < plan.SampleSize || characteristics.IsEmpty
            || characteristics.Count > plan.SampleSize
            || characteristics.Exists(static row => row is null)
            || characteristics.Map(static row => row.Subject.Characteristic).Distinct().Count != characteristics.Count
            || features.Exists(static feature => feature is null)
            || (!features.IsEmpty && features.Count != characteristics.Count)
            || features.Distinct().Count != features.Count
            || plan.Level.Census && plan.SampleSize != lotSize
            || stage.RequiresPrior && prior.IsNone
                ? new ValidationError(message: "inspection-evidence")
                : null;
}

[ComplexValueObject]
public sealed partial class MaterialCertificate {
    public EvidenceRef.Report Report { get; }
    public MaterialSpec Grade { get; }
    public HeatNumber Heat { get; }
    public Seq<EvidenceRef.Lot> Lots { get; }
    public CertType Cert { get; }
    public Instant IssuedAt { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref EvidenceRef.Report report,
        ref MaterialSpec grade,
        ref HeatNumber heat,
        ref Seq<EvidenceRef.Lot> lots,
        ref CertType cert,
        ref Instant issuedAt) =>
        validationError = report is null || grade is null || lots.IsEmpty
            || lots.Exists(static lot => lot is null) || lots.Distinct().Count != lots.Count
            || cert is null || !cert.Valid
                ? new ValidationError(message: "material-certificate")
                : null;
}

[ComplexValueObject]
public sealed partial class ProcessEvidence {
    public EvidenceRef.Report Report { get; }
    public EvidenceRef.Product Product { get; }
    public ProcedureReceipt Procedure { get; }
    public Seq<WeldInspectionRow> Inspections { get; }
    public EvidenceSet Execution { get; }
    public EvidenceContext Context { get; }
    public Option<ContentKey> Prior { get; }

    public Seq<InspectionRequirement> Unfulfilled =>
        Procedure.Inspections.Filter(demand => !Inspections.Exists(row => row.Satisfies(demand)));

    public Seq<QualityObservation> Observations =>
        Execution.ToValue()
        + Inspections.Map(static row => (QualityObservation)new QualityObservation.Ndt(row))
        + Unfulfilled.Map(demand => (QualityObservation)new QualityObservation.Missing(
            new EvidenceRef.Requirement(EvidenceId.Create(demand.Acceptance)), Context));

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref EvidenceRef.Report report,
        ref EvidenceRef.Product product,
        ref ProcedureReceipt procedure,
        ref Seq<WeldInspectionRow> inspections,
        ref EvidenceSet execution,
        ref EvidenceContext context,
        ref Option<ContentKey> prior) =>
        validationError = report is null || product is null || procedure is null || !procedure.Qualified
            || inspections.IsEmpty || inspections.Exists(static row => row is null) || execution is null || context is null
            || inspections.Map(static row => (row.Joint, row.Method)).Distinct().Count != inspections.Count
            || procedure.Inspections.Exists(static demand => string.IsNullOrWhiteSpace(demand.Acceptance))
                ? new ValidationError(message: "process-evidence")
                : null;
}

[ComplexValueObject]
public sealed partial class NonconformanceEvidence {
    public EvidenceRef.Product Product { get; }
    public NonconformanceNumber Number { get; }
    public EvidenceRef.Source Source { get; }
    public int AffectedQuantity { get; }
    public EvidenceLinks Containment { get; }
    public string RootCause { get; }
    public string Correction { get; }
    public Option<string> CorrectiveAction { get; }
    public EvidenceSet Verification { get; }
    public Option<EvidenceSet> Effectiveness { get; }
    public Option<NonconformanceNumber> Recurrence { get; }
    public Seq<ContentKey> Evidence { get; }
    public Disposition Verdict { get; }
    public EvidenceRef.Personnel Authority { get; }
    public Instant OpenedAt { get; }
    public Option<Instant> ClosedAt { get; }
    public Seq<QualityObservation> Observations =>
        Verification.ToValue() + Effectiveness.Map(static set => set.ToValue()).IfNone(Seq<QualityObservation>());

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref EvidenceRef.Product product,
        ref NonconformanceNumber number,
        ref EvidenceRef.Source source,
        ref int affectedQuantity,
        ref EvidenceLinks containment,
        ref string rootCause,
        ref string correction,
        ref Option<string> correctiveAction,
        ref EvidenceSet verification,
        ref Option<EvidenceSet> effectiveness,
        ref Option<NonconformanceNumber> recurrence,
        ref Seq<ContentKey> evidence,
        ref Disposition verdict,
        ref EvidenceRef.Personnel authority,
        ref Instant openedAt,
        ref Option<Instant> closedAt) {
        rootCause = rootCause?.Trim() ?? string.Empty;
        correction = correction?.Trim() ?? string.Empty;
        correctiveAction = correctiveAction.Map(static value => value?.Trim() ?? string.Empty).Filter(static value => value.Length > 0);
        validationError = product is null || source is null || containment is null
            || affectedQuantity < 1 || rootCause.Length == 0 || correction.Length == 0 || verification is null || evidence.IsEmpty
            || evidence.Exists(static key => key is null) || verdict is null || authority is null
            || closedAt.Exists(value => value < openedAt)
            || verdict.Terminal != closedAt.IsSome
            || recurrence.IsSome && correctiveAction.IsNone
            || effectiveness.IsSome != correctiveAction.IsSome
                ? new ValidationError(message: "nonconformance-evidence")
                : null;
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record QualityRecord {
    private QualityRecord() { }

    public sealed record Inspection : QualityRecord {
        internal Inspection(InspectionEvidence evidence) => Evidence = evidence;
        public InspectionEvidence Evidence { get; }
    }

    public sealed record MillCert : QualityRecord {
        internal MillCert(MaterialCertificate evidence) => Evidence = evidence;
        public MaterialCertificate Evidence { get; }
    }

    public sealed record WeldInspection : QualityRecord {
        internal WeldInspection(ProcessEvidence evidence) => Evidence = evidence;
        public ProcessEvidence Evidence { get; }
    }

    public sealed record Nonconformance : QualityRecord {
        internal Nonconformance(NonconformanceEvidence evidence) => Evidence = evidence;
        public NonconformanceEvidence Evidence { get; }
    }

    public sealed record Calibration : QualityRecord {
        internal Calibration(CalibrationRow evidence) => Evidence = evidence;
        public CalibrationRow Evidence { get; }
    }

    public sealed record Conformance : QualityRecord {
        internal Conformance(QualityDeclaration declaration, Seq<ContentKey> records, Instant issuedAt) =>
            (Declaration, Records, IssuedAt) = (declaration, records, issuedAt);

        public QualityDeclaration Declaration { get; }
        public Seq<ContentKey> Records { get; }
        public Instant IssuedAt { get; }
    }

    public Seq<QualityObservation> Observations => Switch(
        inspection: static value => value.Evidence.Observations,
        millCert: static value => value.Evidence.Cert.Observations,
        weldInspection: static value => value.Evidence.Observations,
        nonconformance: static value => value.Evidence.Observations,
        calibration: static value => Seq<QualityObservation>(new QualityObservation.Calibration(value.Evidence)),
        conformance: static value => value.Declaration.Observations);

    public Seq<AttestationRequirement> Requirements => Switch(
        inspection: static _ => Seq<AttestationRequirement>(new AttestationRequirement.Role(AttestationRole.Quality)),
        millCert: static value => value.Evidence.Cert.Requirements,
        weldInspection: static _ => Seq<AttestationRequirement>(
            new AttestationRequirement.Role(AttestationRole.ManufacturerAuthorized),
            new AttestationRequirement.Role(AttestationRole.WeldingInspector)),
        nonconformance: static value => Seq<AttestationRequirement>(
            new AttestationRequirement.Signer(value.Evidence.Authority, AttestationRole.MaterialReviewBoard)),
        calibration: static value => Seq<AttestationRequirement>(
            new AttestationRequirement.Signer(value.Evidence.Context.Actor, AttestationRole.CalibrationLaboratory)),
        conformance: static value => value.Declaration.Requirements);

    public Seq<InspectionFeature> InspectionFeatures => Switch(
        inspection: static value => value.Evidence.Features,
        millCert: static _ => Seq<InspectionFeature>(),
        weldInspection: static _ => Seq<InspectionFeature>(),
        nonconformance: static _ => Seq<InspectionFeature>(),
        calibration: static _ => Seq<InspectionFeature>(),
        conformance: static _ => Seq<InspectionFeature>());

    public static Fin<QualityRecord> Admit(QualitySource source) =>
        from admitted in QualityReport.RecordOp.Need(source)
        from record in admitted.Switch(
            inspection: static value => Sampled(value.Lot, value.Readings, value.Measured.Features),
            residuals: static value => Sampled(value.Lot, value.Readings, Seq<InspectionFeature>()),
            procedure: static value => QualityReport.RecordOp.Need(value.Evidence).Map(static evidence => (QualityRecord)new WeldInspection(evidence)),
            material: static value => QualityReport.RecordOp.Need(value.Evidence).Map(static evidence => (QualityRecord)new MillCert(evidence)),
            nonconformance: static value => QualityReport.RecordOp.Need(value.Evidence).Map(static evidence => (QualityRecord)new Nonconformance(evidence)),
            calibration: static value => QualityReport.RecordOp.Need(value.Evidence).Map(static evidence => (QualityRecord)new Calibration(evidence)),
            declaration: static value =>
                from declaration in QualityReport.RecordOp.Need(value.Declaration)
                from _ in guard(declaration.Valid, QualityReport.Refused(RecordRefusal.Declaration)).ToFin()
                from _lineage in guard(
                    !value.Records.IsEmpty && value.Records.ForAll(static key => key is not null)
                    && value.Records.Distinct().Count == value.Records.Count,
                    QualityReport.Refused(RecordRefusal.Lineage)).ToFin()
                select (QualityRecord)new Conformance(declaration, value.Records, value.IssuedAt),
            record: static value => QualityReport.RecordOp.Need(value.Value))
        select record;

    private static Fin<QualityRecord> Sampled(
        SampledLot lot,
        Seq<SampleReading> readings,
        Seq<InspectionFeature> features) =>
        from _ in (
            QualityReport.Gate(!readings.IsEmpty && readings.Count <= lot.Plan.SampleSize, RecordRefusal.Sampling),
            QualityReport.Gate(readings.ForAll(reading => lot.Subjects.Find(reading.Index).IsSome), RecordRefusal.Subject),
            QualityReport.Gate(readings.Map(static reading => reading.Index).Distinct().Count == readings.Count, RecordRefusal.Subject))
            .Apply(static (_, _, _) => unit)
            .As()
            .ToFin()
        from rows in readings.Map(reading =>
                from subject in lot.Subjects.Find(reading.Index).ToFin(QualityReport.Refused(RecordRefusal.Subject))
                from measurement in Measurement.Admit(
                    reading.Nominal,
                    reading.Observed,
                    reading.Lower,
                    reading.Upper,
                    reading.ExpandedUncertainty,
                    lot.Coverage,
                    lot.DecisionRule,
                    lot.Context)
                from row in CharacteristicRow.Admit(subject, measurement, lot.Mrb.Find(reading.Index).IfNone(Disposition.Conform))
                select row)
            .Traverse(identity)
            .As()
        from evidence in InspectionEvidence.Validate(
                lot.Report,
                lot.Product,
                lot.Stage,
                lot.Plan,
                lot.LotSize,
                rows,
                features,
                lot.Capability,
                lot.Prior,
                lot.Context.At,
                out InspectionEvidence admitted)
            .Admitted(admitted, RecordRefusal.Evidence)
        select (QualityRecord)new Inspection(evidence);
}

public sealed record SampleReading(
    int Index,
    IQuantity Nominal,
    IQuantity Observed,
    IQuantity Lower,
    IQuantity Upper,
    IQuantity ExpandedUncertainty);

[ComplexValueObject]
public sealed partial class SampledLot {
    public EvidenceRef.Report Report { get; }
    public EvidenceRef.Product Product { get; }
    public InspectionStage Stage { get; }
    public SamplingPlan Plan { get; }
    public int LotSize { get; }
    public Map<int, CharacteristicSubject> Subjects { get; }
    public Map<int, Disposition> Mrb { get; }
    public EvidenceContext Context { get; }
    public CoverageInterval Coverage { get; }
    public DecisionRule DecisionRule { get; }
    public Option<CapabilityReport> Capability { get; }
    public Option<ContentKey> Prior { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref EvidenceRef.Report report,
        ref EvidenceRef.Product product,
        ref InspectionStage stage,
        ref SamplingPlan plan,
        ref int lotSize,
        ref Map<int, CharacteristicSubject> subjects,
        ref Map<int, Disposition> mrb,
        ref EvidenceContext context,
        ref CoverageInterval coverage,
        ref DecisionRule decisionRule,
        ref Option<CapabilityReport> capability,
        ref Option<ContentKey> prior) =>
        validationError = report is null || product is null || stage is null || plan is null || context is null
            || coverage is null || decisionRule is null
            || lotSize < plan.SampleSize || subjects.IsEmpty
            || subjects.Values.Exists(static subject => subject is null)
            || !mrb.Keys.ForAll(key => subjects.ContainsKey(key))
            || stage.RequiresPrior && prior.IsNone
                ? new ValidationError(message: "sampled-lot")
                : null;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record QualitySource {
    private QualitySource() { }

    public sealed record Inspection(
        FabricationResult.InspectionResult Measured,
        Length PositionTolerance,
        SampledLot Lot) : QualitySource {
        internal Seq<SampleReading> Readings => Measured.Features.Map((feature, index) => new SampleReading(
            index,
            Length.Zero,
            new Length(feature.DeviationMm, LengthUnit.Millimeter),
            Length.Zero,
            feature.ToleranceMm.Map(static value => new Length(value, LengthUnit.Millimeter)).IfNone(PositionTolerance),
            new Length(feature.UncertaintyMm, LengthUnit.Millimeter)));
    }

    public sealed record Residuals(
        Seq<ResidualSample> Samples,
        Length Uncertainty,
        SampledLot Lot) : QualitySource {
        internal Seq<SampleReading> Readings => Samples.Map(sample => new SampleReading(
            sample.Index,
            Length.Zero,
            new Length(sample.Distance, LengthUnit.Millimeter),
            new Length(-sample.Tolerance, LengthUnit.Millimeter),
            new Length(sample.Tolerance, LengthUnit.Millimeter),
            Uncertainty));
    }

    public sealed record Procedure(ProcessEvidence Evidence) : QualitySource;
    public sealed record Material(MaterialCertificate Evidence) : QualitySource;
    public sealed record Nonconformance(NonconformanceEvidence Evidence) : QualitySource;
    public sealed record Calibration(CalibrationRow Evidence) : QualitySource;
    public sealed record Declaration(QualityDeclaration Declaration, Seq<ContentKey> Records, Instant IssuedAt) : QualitySource;
    public sealed record Record(QualityRecord Value) : QualitySource;
}

// --- [PASSPORT] -----------------------------------------------------------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SustainabilityEvidence {
    private SustainabilityEvidence() { }

    public sealed record EnergyUse(Energy Value, EvidenceRef.Source Source, Interval Period) : SustainabilityEvidence;
    public sealed record Carbon(Mass Value, EvidenceRef.Source Source, Interval Period) : SustainabilityEvidence;
    public sealed record Waste(Mass Value, EvidenceRef.Source Source, Interval Period) : SustainabilityEvidence;
    public sealed record RecycledContent(Ratio Value, EvidenceRef.Source Source, Interval Period) : SustainabilityEvidence;
    public sealed record WaterUse(Volume Value, EvidenceRef.Source Source, Interval Period) : SustainabilityEvidence;
    public sealed record RenewableEnergy(Ratio Value, EvidenceRef.Source Source, Interval Period) : SustainabilityEvidence;
    public sealed record RecyclableMass(Mass Value, EvidenceRef.Source Source, Interval Period) : SustainabilityEvidence;
    public sealed record HazardousSubstance(
        EvidenceRef.Material Substance,
        Mass Value,
        EvidenceRef.Source Source,
        Interval Period) : SustainabilityEvidence;
    public sealed record Repairability(Ratio Value, EvidenceRef.Source Source, Interval Period) : SustainabilityEvidence;
    public sealed record Durability(NodaTime.Duration Value, EvidenceRef.Source Source, Interval Period) : SustainabilityEvidence;

    public bool Valid => Switch(
        energyUse: static value => value.Value >= Energy.Zero && ValidSource(value.Source, value.Period),
        carbon: static value => value.Value >= Mass.Zero && ValidSource(value.Source, value.Period),
        waste: static value => value.Value >= Mass.Zero && ValidSource(value.Source, value.Period),
        recycledContent: static value => QualityReport.ValidFraction(value.Value)
            && ValidSource(value.Source, value.Period),
        waterUse: static value => value.Value >= Volume.Zero && ValidSource(value.Source, value.Period),
        renewableEnergy: static value => QualityReport.ValidFraction(value.Value)
            && ValidSource(value.Source, value.Period),
        recyclableMass: static value => value.Value >= Mass.Zero && ValidSource(value.Source, value.Period),
        hazardousSubstance: static value => value.Substance is not null && value.Value >= Mass.Zero
            && ValidSource(value.Source, value.Period),
        repairability: static value => QualityReport.ValidFraction(value.Value)
            && ValidSource(value.Source, value.Period),
        durability: static value => value.Value > NodaTime.Duration.Zero && ValidSource(value.Source, value.Period));

    private static bool ValidSource(EvidenceRef.Source source, Interval period) =>
        source is not null && period.HasStart && period.HasEnd && period.Start < period.End;
}

[ComplexValueObject]
public sealed partial class GenealogyLink {
    public EvidenceRef Parent { get; }
    public EvidenceRef Child { get; }
    public TraceRelation Relation { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref EvidenceRef parent,
        ref EvidenceRef child,
        ref TraceRelation relation) =>
        validationError = parent is null || child is null || relation is null || parent == child
            ? new ValidationError(message: "genealogy-link")
            : null;
}

[ComplexValueObject]
public sealed partial class PassportEvidence {
    public EvidenceRef.Product Product { get; }
    public Seq<GenealogyLink> Genealogy { get; }
    public EvidenceLinks Materials { get; }
    public EvidenceLinks Compliance { get; }
    public Seq<QualityDeclaration> Declarations { get; }
    public Seq<SustainabilityEvidence> Sustainability { get; }
    public Seq<ContentKey> ServiceHistory { get; }
    public Seq<ContentKey> RepairHistory { get; }
    public Instant ManufacturedAt { get; }
    public Option<Instant> RetiredAt { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref EvidenceRef.Product product,
        ref Seq<GenealogyLink> genealogy,
        ref EvidenceLinks materials,
        ref EvidenceLinks compliance,
        ref Seq<QualityDeclaration> declarations,
        ref Seq<SustainabilityEvidence> sustainability,
        ref Seq<ContentKey> serviceHistory,
        ref Seq<ContentKey> repairHistory,
        ref Instant manufacturedAt,
        ref Option<Instant> retiredAt) =>
        validationError = product is null || genealogy.IsEmpty || genealogy.Exists(static value => value is null)
            || materials is null || !materials.ToValue().ForAll(static value => value is EvidenceRef.Material or EvidenceRef.Lot)
            || compliance is null || !compliance.ToValue().ForAll(static value => value is EvidenceRef.Certificate or EvidenceRef.Requirement)
            || declarations.IsEmpty || declarations.Exists(static value => value is null || !value.Valid)
            || sustainability.IsEmpty || sustainability.Exists(static value => value is null || !value.Valid)
            || serviceHistory.Exists(static value => value is null) || repairHistory.Exists(static value => value is null)
            || serviceHistory.Distinct().Count != serviceHistory.Count || repairHistory.Distinct().Count != repairHistory.Count
            || retiredAt.Exists(value => value <= manufacturedAt)
            || genealogy.Map(static value => (value.Parent, value.Child)).Distinct().Count != genealogy.Count
            || !ValidGenealogy(product, genealogy)
                ? new ValidationError(message: "passport-evidence")
                : null;

    private static bool ValidGenealogy(EvidenceRef.Product product, Seq<GenealogyLink> genealogy) {
        AdjacencyGraph<EvidenceRef, Edge<EvidenceRef>> graph = new(allowParallelEdges: false);
        genealogy.Iter(link => graph.AddVerticesAndEdge(new Edge<EvidenceRef>(link.Parent, link.Child)));
        Dictionary<EvidenceRef, int> components = new();
        Seq<EvidenceRef> sinks = toSeq(graph.Sinks());
        return graph.IsDirectedAcyclicGraph()
            && graph.WeaklyConnectedComponents(components) == 1
            && sinks.Count == 1
            && sinks[0] == product;
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ReportScope {
    private ReportScope() { }

    public sealed record Records : ReportScope;
    public sealed record Passport(PassportEvidence Evidence) : ReportScope;
}

// --- [ATTESTATION] --------------------------------------------------------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AttestationRequirement {
    private AttestationRequirement() { }

    public sealed record Role(AttestationRole Value) : AttestationRequirement;
    public sealed record Signer(EvidenceRef.Personnel Identity, AttestationRole Role) : AttestationRequirement;

    internal bool SatisfiedBy(Seq<AttestationCredential> credentials) => Switch(
        state: credentials,
        role: static (values, requirement) => requirement.Value is not null
            && values.Exists(credential => credential.Role == requirement.Value),
        signer: static (values, requirement) => requirement.Identity is not null && requirement.Role is not null
            && values.Exists(credential => credential.Role == requirement.Role && credential.Signer == requirement.Identity));
}

[BoundaryAdapter]
public sealed record AttestationCredential(
    EvidenceRef.Personnel Signer,
    AttestationRole Role,
    EvidenceRef.Certificate Credential,
    X509Certificate2 Certificate);

public sealed record AttestationPayload(
    ReadOnlyMemory<byte> Body,
    EvidenceRef.Personnel Signer,
    AttestationRole Role,
    EvidenceRef.Certificate Credential,
    Instant SignedAt);

public sealed record RecordAttestation(
    EvidenceRef.Personnel Signer,
    AttestationRole Role,
    EvidenceRef.Certificate Credential,
    string CertificatePem,
    ReadOnlyMemory<byte> Signature,
    Instant SignedAt) {
    public Fin<Unit> Verify(
        ReadOnlyMemory<byte> canonicalBody,
        Func<EvidenceRef.Personnel, AttestationRole, EvidenceRef.Certificate, X509Certificate2, Fin<Unit>> trust) =>
        from preimage in QualityReport.Canonical(new AttestationPayload(canonicalBody, Signer, Role, Credential, SignedAt))
        from verified in Try.lift(() => {
            using X509Certificate2 certificate = X509Certificate2.CreateFromPem(CertificatePem);
            using ECDsa? key = certificate.GetECDsaPublicKey();
            return from _ in trust(Signer, Role, Credential, certificate)
                   from __ in guard(key is not null && key.VerifyData(
                       preimage.Span,
                       Signature.Span,
                       HashAlgorithmName.SHA384,
                       DSASignatureFormat.Rfc3279DerSequence), QualityReport.Refused(RecordRefusal.Signature)).ToFin()
                   select unit;
        }).Run().MapFail(static _ => QualityReport.Refused(RecordRefusal.Signature))
        from result in verified
        select unit;
}

// --- [PROJECTIONS] --------------------------------------------------------------------------------------------------------------------------------
[BoundaryAdapter]
public sealed record QualityReportRequest(
    ReportScope Scope,
    Seq<QualityRecord> Records,
    Seq<AttestationCredential> Signers,
    Func<EvidenceRef.Personnel, AttestationRole, EvidenceRef.Certificate, X509Certificate2, Fin<Unit>> Trust,
    Instant SealedAt);

public sealed record QualityReportBody(Seq<QualityRecord> Records, Instant SealedAt);

public sealed record DigitalProductPassport(
    PassportEvidence Evidence,
    ContentKey QualityRecord);

public sealed record Attested<TBody>(
    TBody Body,
    ReadOnlyMemory<byte> Canonical,
    ContentKey Key,
    Seq<RecordAttestation> Attestations);

public sealed record SealedRecord(
    Attested<QualityReportBody> Report,
    Option<Attested<DigitalProductPassport>> Passport,
    EvidenceCensus Census) {
    public ContentKey Key => Report.Key;
    public Seq<QualityRecord> Records => Report.Body.Records;
    public Seq<RecordAttestation> Attestations => Report.Attestations;
    public Option<ContentKey> DigitalProductPassport => Passport.Map(static artifact => artifact.Key);
}

// --- [OPERATIONS] ---------------------------------------------------------------------------------------------------------------------------------
public static class QualityReport {
    internal static readonly Op RecordOp = Op.Of(name: "fabrication:quality-record");

    internal static readonly JsonSerializerOptions CanonicalJson =
        new JsonSerializerOptions(JsonSerializerDefaults.General) {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            Converters = { new QuantityJsonConverter(), new ThinktectureJsonConverterFactory() },
        }.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);

    private sealed class QuantityJsonConverter : JsonConverter<IQuantity> {
        private const string QuantityProperty = "quantity";
        private const string UnitProperty = "unit";
        private const string ValueProperty = "value";

        public override IQuantity Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            using JsonDocument document = JsonDocument.ParseValue(ref reader);
            JsonElement root = document.RootElement;
            string quantity = root.GetProperty(QuantityProperty).GetString()
                ?? throw new JsonException(message: "quantity-name");
            string unit = root.GetProperty(UnitProperty).GetString()
                ?? throw new JsonException(message: "quantity-unit");
            return Quantity.From(root.GetProperty(ValueProperty).GetDouble(), quantity, unit);
        }

        public override void Write(Utf8JsonWriter writer, IQuantity value, JsonSerializerOptions options) {
            ArgumentNullException.ThrowIfNull(writer);
            ArgumentNullException.ThrowIfNull(value);
            writer.WriteStartObject();
            writer.WriteString(QuantityProperty, value.QuantityInfo.Name);
            writer.WriteString(UnitProperty, value.Unit.ToString());
            writer.WriteNumber(ValueProperty, (double)value.Value);
            writer.WriteEndObject();
        }
    }

    public static Fin<SealedRecord> Seal(QualityReportRequest request) =>
        from admitted in RecordOp.Need(request)
        from _request in (
            Gate(!admitted.Records.IsEmpty && admitted.Records.ForAll(static record => record is not null), RecordRefusal.Source),
            Gate(admitted.Scope is not null, RecordRefusal.Scope),
            Gate(admitted.Signers.ForAll(static signer => signer is not null && signer.Signer is not null
                && signer.Role is not null && signer.Credential is not null && signer.Certificate is not null)
                && admitted.Trust is not null, RecordRefusal.Credential))
            .Apply(static (_, _, _) => unit)
            .As()
            .ToFin()
        from _trusted in admitted.Signers.Traverse(signer => admitted.Trust(
            signer.Signer, signer.Role, signer.Credential, signer.Certificate)).As()
        from _quorum in (
            Gate(admitted.Signers.Map(static signer => (signer.Signer, signer.Role)).Distinct().Count == admitted.Signers.Count,
                RecordRefusal.Credential),
            Gate(!admitted.Signers.Exists(independent => independent.Role.IndependentAuthority
                && admitted.Signers.Exists(authority => !authority.Role.IndependentAuthority
                    && authority.Signer == independent.Signer)), RecordRefusal.Independence),
            Gate(Required(admitted).ForAll(requirement => requirement.SatisfiedBy(admitted.Signers)), RecordRefusal.Quorum))
            .Apply(static (_, _, _) => unit)
            .As()
            .ToFin()
        from report in Attest(
            new QualityReportBody(admitted.Records, admitted.SealedAt),
            EgressKind.QualityRecord,
            admitted.Signers,
            admitted.Trust,
            admitted.SealedAt)
        from passport in admitted.Scope.Switch(
            records: static _ => Fin.Succ(Option<Attested<DigitalProductPassport>>.None),
            passport: value => Attest(
                    new DigitalProductPassport(value.Evidence, report.Key),
                    EgressKind.DigitalProductPassport,
                    admitted.Signers,
                    admitted.Trust,
                    admitted.SealedAt)
                .Map(static artifact => Some(artifact)))
        select new SealedRecord(
            report,
            passport,
            EvidenceCensus.Of(admitted.Records.Bind(static record => record.Observations)));

    private static Seq<AttestationRequirement> Required(QualityReportRequest request) => (
        request.Records.Bind(static record => record.Requirements)
        + request.Scope.Switch(
            records: static _ => Seq<AttestationRequirement>(),
            passport: static value => value.Evidence.Declarations.Bind(static declaration => declaration.Requirements)
                + Seq<AttestationRequirement>(new AttestationRequirement.Role(AttestationRole.SustainabilityVerifier))))
        .Distinct()
        .ToSeq();

    private static Fin<Attested<TBody>> Attest<TBody>(
        TBody body,
        EgressKind kind,
        Seq<AttestationCredential> signers,
        Func<EvidenceRef.Personnel, AttestationRole, EvidenceRef.Certificate, X509Certificate2, Fin<Unit>> trust,
        Instant sealedAt) =>
        from canonical in Canonical(body)
        from attestations in signers.Traverse(credential => Sign(canonical, credential, sealedAt)).As()
        from _verified in attestations.Traverse(attestation => attestation.Verify(canonical, trust)).As()
        select new Attested<TBody>(body, canonical, ContentKey.Of(kind, canonical.Span), attestations);

    private static Fin<RecordAttestation> Sign(
        ReadOnlyMemory<byte> canonicalBody,
        AttestationCredential credential,
        Instant signedAt) =>
        from preimage in Canonical(new AttestationPayload(
            canonicalBody,
            credential.Signer,
            credential.Role,
            credential.Credential,
            signedAt))
        from signature in Try.lift(() => {
            using ECDsa? key = credential.Certificate.GetECDsaPrivateKey();
            return key is null
                ? Option<byte[]>.None
                : Some(key.SignData(preimage.Span, HashAlgorithmName.SHA384, DSASignatureFormat.Rfc3279DerSequence));
        }).Run().MapFail(static _ => Refused(RecordRefusal.SigningKey))
        from bytes in signature.ToFin(Refused(RecordRefusal.SigningKey))
        select new RecordAttestation(
            credential.Signer,
            credential.Role,
            credential.Credential,
            credential.Certificate.ExportCertificatePem(),
            bytes,
            signedAt);

    internal static Fin<ReadOnlyMemory<byte>> Canonical<T>(T value) =>
        Try.lift(() => (ReadOnlyMemory<byte>)JsonSerializer.SerializeToUtf8Bytes(value, CanonicalJson))
            .Run()
            .MapFail(static _ => Refused(RecordRefusal.Canonical));

    internal static bool ValidFraction(Ratio value, bool positive = false) {
        double fraction = value.As(RatioUnit.DecimalFraction);
        return double.IsFinite(fraction) && (positive ? fraction > 0.0 : fraction >= 0.0) && fraction <= 1.0;
    }

    internal static Error Refused(RecordRefusal reason) => RecordOp.InvalidResult(detail: reason.Key);

    internal static K<Validation<Error>, Unit> Gate(bool condition, RecordRefusal reason) =>
        guard(condition, Refused(reason)).ToFin().ToValidation();

    extension(ValidationError? error) {
        internal Fin<TValue> Admitted<TValue>(TValue candidate, RecordRefusal reason) =>
            error is null ? Fin.Succ(candidate) : Fin.Fail<TValue>(Refused(reason));
    }
}
```
