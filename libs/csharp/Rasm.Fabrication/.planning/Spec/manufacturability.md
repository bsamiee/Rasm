# [RASM_FABRICATION_MANUFACTURABILITY]

`Manufacturability` owns evidence-backed producibility from admitted component geometry and supplied domain observations through parameterized rule evaluation, remediation, process-requirement ranking, assembly precheck, and one terminal `DfmReport`. Missing, insufficient-confidence, or incomparable evidence remains an explicit gate state; no absent lane reads as conforming.

`Analyze`, `Offsetting`, and `Spatial` remain geometry-kernel owners. `Capability.Achievable` owns process-history projection through the qualifying row's own `ItGrade`, `Tolerance.Apply(ToleranceRequest.Effective)` owns material-condition departure and virtual condition, `ProcedureReceipt.Qualified` owns weld-procedure compliance, `ModalityPhysics` owns process physics, and `Kinematics/fleet` owns machine matching. `DfmReport.Routing` crosses the derivation seam as ranked `ProcessKind` evidence.

## [01]-[INDEX]

- [02]-[MANUFACTURABILITY]: `Manufacturability.Assess` admits one `DfmRequest`, composes derived and supplied observations, evaluates parameterized policy rows per process, projects process requirements, ranks viable processes, and emits one `DfmReport`.

## [02]-[MANUFACTURABILITY]

- Owner: `DfmConcern` and `DfmFeature` close domain vocabulary; `DfmMeasure` and `DfmCriterion` close typed comparison; `DfmRule` parameterizes applicability, evidence obligation, minimum confidence, severity, and remediation; `DfmObservation` preserves measured evidence and provenance.
- Provenance: `DfmProvenance` owns the evidence-key namespace and the confidence each derivation route earns, so an analytic measurement, a sampled medial axis, and a ray probe are never equally trusted and no lane spells a key literal.
- Gate: `DfmVerdict.Gates` defers every consequence to `DfmSeverity`, and `DfmPolicy` admission proves each required concern carries a gating rule — the telos that no absent lane reads as conforming holds structurally instead of by outcome-kind override.
- Routing: `RouteObjective` rows carry their own yield-adjusted measurement and weight selector, so `RouteScore` folds `RouteColumn` values and a new routing dimension is one row with no scoring expression re-spelled.
- Cases: removal covers access, draft, undercut, feature depth, corner radius, thread, finish, stock, datum, and inspection constraints; additive covers feature size, wall, overhang, bridge, enclosed volume, escape, support removal, recoater, anisotropy, thermal, and integrity constraints; forming covers bend radius, edge and hole distance, flange, hem, draw, grain, tonnage, springback, and thinning constraints; joining covers access, root gap, throat, heat input, distortion, qualification, inspection, and assembly constraints.
- Entry: `Manufacturability.Assess(DfmRequest)` is the sole cross-modality fold. Geometry, capability, supplied evidence, and assembly allowances join applicatively; kernel failures remain typed `Fin` failures, while producibility failures remain report rows.
- Auto: `DfmPolicy` proves every required concern has a generic or process-specific gating rule; `DfmCriterion.Evaluate` compares unit-bearing, count, ratio, and flag measures; `RouteCandidate.Encloses` derives the envelope concern from the candidate's own work volume, so a mesh-only part is never blocked for want of supplied envelope evidence; approach rows derive removal, joining, and build-orientation evidence; `ProcedureReceipt.Qualified` derives the joined-process qualification concern; `TraverseM`, `Apply`, `Choose`, and `Fold` own collection flow; `Capability.Achievable` and `Tolerance.Apply(ToleranceRequest.Effective)` derive achievable-tolerance and virtual-condition observations.
- Degradation: a degenerate profile, an unresolvable medial axis, or absent history contributes no observation rather than failing the report, so producibility gaps stay report rows and only kernel faults leave the rail.
- Receipt: `DfmVerdict` preserves process, confidence outcome, observation, criterion, locus, and remedy; `RoutingRow` preserves blockers, requirements, and the `RouteScore` column set whose `Worst` names the dominant burden; `StackupPrecheck` preserves required and observed allowance counts with worst-case evidence; `DfmReport` preserves the full decision basis.
- Packages: `Loop.Apply` composes CavalierContours arc-native measurement and sampling; `PolygonAlgebra.Apply` composes Clipper2 topology; `DfmPackageEvidence.Cutter` carries OpenCAMLib cutter-contact evidence against canonical `ToolEvidence`; `DfmPackageEvidence.Voxel` carries PicoGK morphology, membership, ray, and solid-property evidence; UnitsNet owns every physical comparison; Thinktecture and LanguageExt own generated values and the accumulated rail.
- Growth: a concern is one `DfmConcern` seed; a feature is one `DfmFeature` seed; a policy variation is one `DfmRule` row; a process candidate is one `RouteCandidate` row; a routing dimension is one `RouteObjective` row; a derivation route is one `DfmProvenance` row; no entrypoint or verdict case grows.
- Boundary: sidecar OpenCAMLib and PicoGK owners lower native handles into `DfmPackageEvidence.Cutter` and `DfmPackageEvidence.Voxel` before this host-local owner consumes them. Routing ranks process requirements and evidence, while fleet matching, tool selection, support generation, unfolding, joining sequence, correlated stackup, rendering, and persistence remain downstream owners.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------------------------------------------------------
using Foundation.CSharp.Analyzers.Contracts;
using LanguageExt;
using LanguageExt.Common;
using NodaTime;
using Rasm.Analysis;
using Rasm.Domain;
using Rasm.Fabrication.Geometry2D;
using Rasm.Fabrication.Joining;
using Rasm.Fabrication.Process;
using Rasm.Meshing;
using Rasm.Spatial;
using Rhino.Geometry;
using Thinktecture;
using UnitsNet;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Spec;

// --- [VOCABULARY] ---------------------------------------------------------------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class DfmSeverity {
    public static readonly DfmSeverity Advisory = new("advisory", gate: false, penalty: 1.0);
    public static readonly DfmSeverity Warning = new("warning", gate: false, penalty: 3.0);
    public static readonly DfmSeverity Blocker = new("blocker", gate: true, penalty: 10.0);

    public bool Gate { get; }
    public double Penalty { get; }
}

[SmartEnum<string>]
public sealed partial class DfmOutcome {
    public static readonly DfmOutcome Conforming = new("conforming", gate: false);
    public static readonly DfmOutcome Nonconforming = new("nonconforming", gate: true);
    public static readonly DfmOutcome MissingEvidence = new("missing-evidence", gate: true);
    public static readonly DfmOutcome InsufficientConfidence = new("insufficient-confidence", gate: true);
    public static readonly DfmOutcome IncomparableEvidence = new("incomparable-evidence", gate: true);

    public bool Gate { get; }
}

[SmartEnum<string>]
public sealed partial class DfmFeature {
    public static readonly DfmFeature Part = new("part");
    public static readonly DfmFeature Stock = new("stock");
    public static readonly DfmFeature Envelope = new("envelope");
    public static readonly DfmFeature Surface = new("surface");
    public static readonly DfmFeature Wall = new("wall");
    public static readonly DfmFeature Rib = new("rib");
    public static readonly DfmFeature Boss = new("boss");
    public static readonly DfmFeature Hole = new("hole");
    public static readonly DfmFeature Pocket = new("pocket");
    public static readonly DfmFeature Slot = new("slot");
    public static readonly DfmFeature Thread = new("thread");
    public static readonly DfmFeature Datum = new("datum");
    public static readonly DfmFeature Inspection = new("inspection");
    public static readonly DfmFeature Bend = new("bend");
    public static readonly DfmFeature Flange = new("flange");
    public static readonly DfmFeature Hem = new("hem");
    public static readonly DfmFeature Draw = new("draw");
    public static readonly DfmFeature Joint = new("joint");
    public static readonly DfmFeature Overhang = new("overhang");
    public static readonly DfmFeature Bridge = new("bridge");
    public static readonly DfmFeature EnclosedVolume = new("enclosed-volume");
    public static readonly DfmFeature Lattice = new("lattice");
    public static readonly DfmFeature Support = new("support");
    public static readonly DfmFeature Setup = new("setup");
    public static readonly DfmFeature Assembly = new("assembly");
}

[SmartEnum<string>]
public sealed partial class DfmConcern {
    public static readonly DfmConcern GeometryEvidence = Any("geometry-evidence", required: true);
    public static readonly DfmConcern MaterialEvidence = Any("material-evidence", required: true);
    public static readonly DfmConcern ToleranceCapability = Any("tolerance-capability", required: true);
    public static readonly DfmConcern MinimumFeature = Any("minimum-feature", required: true);
    public static readonly DfmConcern MinimumWall = For("minimum-wall", true, ModalityClass.Removal, ModalityClass.Additive, ModalityClass.Formed);
    public static readonly DfmConcern SolidVolume = Any("solid-volume", required: false);
    public static readonly DfmConcern DatumAccess = Any("datum-access", required: true);
    public static readonly DfmConcern InspectionAccess = Any("inspection-access", required: true);
    public static readonly DfmConcern StandardSize = Any("standard-size", required: false);
    public static readonly DfmConcern StockAllowance = For("stock-allowance", true, ModalityClass.Removal, ModalityClass.Formed);
    public static readonly DfmConcern Envelope = Any("envelope", required: true);
    public static readonly DfmConcern Draft = For("draft", true, ModalityClass.Removal);
    public static readonly DfmConcern Undercut = For("undercut", true, ModalityClass.Removal);
    public static readonly DfmConcern ToolAccess = For("tool-access", true, ModalityClass.Removal);
    public static readonly DfmConcern DepthToDiameter = For("depth-to-diameter", true, ModalityClass.Removal);
    public static readonly DfmConcern InternalCorner = For("internal-corner", true, ModalityClass.Removal);
    public static readonly DfmConcern ThreadReach = For("thread-reach", true, ModalityClass.Removal);
    public static readonly DfmConcern SurfaceFinish = For("surface-finish", true, ModalityClass.Removal, ModalityClass.Additive);
    public static readonly DfmConcern Overhang = For("overhang", true, ModalityClass.Additive);
    public static readonly DfmConcern Bridge = For("bridge", true, ModalityClass.Additive);
    public static readonly DfmConcern TrappedVolume = For("trapped-volume", true, ModalityClass.Additive);
    public static readonly DfmConcern EscapeAccess = For("escape-access", true, ModalityClass.Additive);
    public static readonly DfmConcern SupportRemoval = For("support-removal", true, ModalityClass.Additive);
    public static readonly DfmConcern RecoaterClearance = For("recoater-clearance", true, ModalityClass.Additive);
    public static readonly DfmConcern Anisotropy = For("anisotropy", true, ModalityClass.Additive);
    public static readonly DfmConcern ThermalDistortion = For("thermal-distortion", true, ModalityClass.Additive, ModalityClass.Joined);
    public static readonly DfmConcern Integrity = For("integrity", true, ModalityClass.Additive);
    public static readonly DfmConcern BendRadius = For("bend-radius", true, ModalityClass.Formed);
    public static readonly DfmConcern BendEdgeDistance = For("bend-edge-distance", true, ModalityClass.Formed);
    public static readonly DfmConcern BendHoleDistance = For("bend-hole-distance", true, ModalityClass.Formed);
    public static readonly DfmConcern FlangeLength = For("flange-length", true, ModalityClass.Formed);
    public static readonly DfmConcern HemGap = For("hem-gap", true, ModalityClass.Formed);
    public static readonly DfmConcern DrawRatio = For("draw-ratio", true, ModalityClass.Formed);
    public static readonly DfmConcern GrainDirection = For("grain-direction", true, ModalityClass.Formed);
    public static readonly DfmConcern Tonnage = For("tonnage", true, ModalityClass.Formed);
    public static readonly DfmConcern Springback = For("springback", true, ModalityClass.Formed);
    public static readonly DfmConcern Thinning = For("thinning", true, ModalityClass.Formed);
    public static readonly DfmConcern WeldAccess = For("weld-access", true, ModalityClass.Joined);
    public static readonly DfmConcern RootGap = For("root-gap", true, ModalityClass.Joined);
    public static readonly DfmConcern Throat = For("throat", true, ModalityClass.Joined);
    public static readonly DfmConcern HeatInput = For("heat-input", true, ModalityClass.Joined);
    public static readonly DfmConcern ProcedureQualification = For("procedure-qualification", true, ModalityClass.Joined);
    public static readonly DfmConcern JointInspection = For("joint-inspection", true, ModalityClass.Joined);
    public static readonly DfmConcern AssemblyAccess = For("assembly-access", true, ModalityClass.Joined);
    public static readonly DfmConcern AssemblyStackup = Any("assembly-stackup", required: false);

    public Set<ModalityClass> Classes { get; }
    public bool Required { get; }

    public bool AppliesTo(ModalityClass cls) => Classes.Contains(cls);

    private static DfmConcern Any(string key, bool required) =>
        For(key, required, ModalityClass.Removal, ModalityClass.Additive, ModalityClass.Formed, ModalityClass.Joined);

    private static DfmConcern For(string key, bool required, params ModalityClass[] classes) =>
        new(key, toSet(classes), required);
}

[ValueObject<string>]
public readonly partial struct DfmEvidenceKey {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        value = value?.Trim() ?? string.Empty;
        if (value.Length == 0)
            validationError = ValidationError.Create("DfmEvidenceKey cannot be empty.");
    }
}

// Derivation route owns both the evidence-key namespace and the confidence a measurement of that kind carries.
[SmartEnum<string>]
public sealed partial class DfmProvenance {
    public static readonly DfmProvenance Analytic = new("analytic", confidence: 1.0);
    public static readonly DfmProvenance Policy = new("policy", confidence: 1.0);
    public static readonly DfmProvenance Package = new("package", confidence: 1.0);
    public static readonly DfmProvenance History = new("capability-history", confidence: 1.0);
    public static readonly DfmProvenance Qualification = new("qualification", confidence: 1.0);
    public static readonly DfmProvenance Sampled = new("sampled", confidence: 0.9);
    public static readonly DfmProvenance Probed = new("probed", confidence: 0.75);

    public double Confidence { get; }

    public Fin<DfmEvidenceKey> Evidence(DfmConcern concern, Option<ProcessKind> process) =>
        DfmEvidenceKey.Validate(
            process.Match(Some: candidate => $"{Key}:{concern.Key}:{candidate.Key}", None: () => $"{Key}:{concern.Key}"),
            provider: null,
            out DfmEvidenceKey admitted) is { }
                ? Fin.Fail<DfmEvidenceKey>(Manufacturability.DfmOp.InvalidInput())
                : Fin.Succ(admitted);
}

// One weighted, yield-adjusted objective per row: a new routing dimension is a row, and no scoring expression is re-spelled.
[SmartEnum<string>]
public sealed partial class RouteObjective {
    public static readonly RouteObjective Quality = new("quality",
        static (weight, _, lane) => lane.Fold(0.0, static (sum, verdict) => sum
                + (verdict.Outcome == DfmOutcome.Conforming ? 0.0 : verdict.Rule.Severity.Penalty * verdict.Rule.Weight))
            / weight.QualityReference,
        static weight => weight.Quality);
    public static readonly RouteObjective Time = new("time",
        static (weight, candidate, _) => candidate.CycleTime.Seconds / (weight.TimeReference.Seconds * candidate.YieldRate),
        static weight => weight.Time);
    public static readonly RouteObjective Waste = new("waste",
        static (weight, candidate, _) => candidate.Waste.Kilograms / (weight.WasteReference.Kilograms * candidate.YieldRate),
        static weight => weight.Waste);
    public static readonly RouteObjective Energy = new("energy",
        static (weight, candidate, _) => candidate.Energy.Joules / (weight.EnergyReference.Joules * candidate.YieldRate),
        static weight => weight.Energy);
    public static readonly RouteObjective Risk = new("risk",
        static (_, candidate, _) => candidate.Risk,
        static weight => weight.Risk);

    public Func<RouteWeight, RouteCandidate, Seq<DfmVerdict>, double> Measure { get; }
    public Func<RouteWeight, double> Weight { get; }
}

// --- [MEASUREMENT] --------------------------------------------------------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DfmMeasure {
    private DfmMeasure() { }

    public sealed record Quantity(IQuantity Value) : DfmMeasure;
    public sealed record Ratio(double Value) : DfmMeasure;
    public sealed record Count(int Value) : DfmMeasure;
    public sealed record Flag(bool Value) : DfmMeasure;

    public bool IsValid => Switch(
        quantity: static quantity => quantity.Value is not null && double.IsFinite(quantity.Value.Value),
        ratio: static ratio => double.IsFinite(ratio.Value),
        count: static count => count.Value >= 0,
        flag: static _ => true);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DfmCriterion {
    private DfmCriterion() { }

    public sealed record Minimum(DfmMeasure Bound) : DfmCriterion;
    public sealed record Maximum(DfmMeasure Bound) : DfmCriterion;
    public sealed record Band(DfmMeasure Lower, DfmMeasure Upper) : DfmCriterion;
    public sealed record Required() : DfmCriterion;
    public sealed record Forbidden() : DfmCriterion;

    public bool IsValid => Switch(
        minimum: static criterion => criterion.Bound is not null && criterion.Bound.IsValid,
        maximum: static criterion => criterion.Bound is not null && criterion.Bound.IsValid,
        band: static criterion => criterion.Lower is not null && criterion.Lower.IsValid
            && criterion.Upper is not null && criterion.Upper.IsValid
            && Compare(criterion.Lower, criterion.Upper).Exists(static order => order <= 0),
        required: static _ => true,
        forbidden: static _ => true);

    public Option<bool> Evaluate(DfmMeasure measured) =>
        Switch(
            state: measured,
            minimum: static (value, criterion) => Compare(value, criterion.Bound).Map(order => order >= 0),
            maximum: static (value, criterion) => Compare(value, criterion.Bound).Map(order => order <= 0),
            band: static (value, criterion) =>
                from lower in Compare(value, criterion.Lower)
                from upper in Compare(value, criterion.Upper)
                select lower >= 0 && upper <= 0,
            required: static (value, _) => value is DfmMeasure.Flag flag ? Some(flag.Value) : None,
            forbidden: static (value, _) => value is DfmMeasure.Flag flag ? Some(!flag.Value) : None);

    private static Option<int> Compare(DfmMeasure left, DfmMeasure right) =>
        (left, right) switch {
            (DfmMeasure.Quantity a, DfmMeasure.Quantity b) when a.Value is not null && b.Value is not null
                && a.Value.QuantityInfo.Name == b.Value.QuantityInfo.Name
                && a.Value.QuantityInfo.BaseDimensions == b.Value.QuantityInfo.BaseDimensions =>
                Some(a.Value.As(UnitSystem.SI).CompareTo(b.Value.As(UnitSystem.SI))),
            (DfmMeasure.Ratio a, DfmMeasure.Ratio b) => Some(a.Value.CompareTo(b.Value)),
            (DfmMeasure.Count a, DfmMeasure.Count b) => Some(a.Value.CompareTo(b.Value)),
            (DfmMeasure.Flag a, DfmMeasure.Flag b) => Some(a.Value.CompareTo(b.Value)),
            _ => None,
        };
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DfmLocus {
    private DfmLocus() { }

    public sealed record AtPoint(Point3d Point) : DfmLocus;
    public sealed record AtEdge(Edge3 Edge) : DfmLocus;
    public sealed record AtFace(int Face) : DfmLocus;
    public sealed record AtRegion(BoundingBox Bounds) : DfmLocus;
    public sealed record AtVolume(BoundingBox Bounds) : DfmLocus;
    public sealed record AtFeature(DfmFeature Feature, UInt128 Key) : DfmLocus;
    public sealed record AtDatum(UInt128 Key) : DfmLocus;
    public sealed record AtLayer(int Layer) : DfmLocus;
    public sealed record AtJoint(int Joint) : DfmLocus;
    public sealed record AtSetup(int Setup) : DfmLocus;
    public sealed record AtProcess(ProcessKind Process) : DfmLocus;
    public sealed record Global() : DfmLocus;

    public bool IsValid => Switch(
        atPoint: static locus => locus.Point.IsValid,
        atEdge: static locus => locus.Edge.A.IsValid && locus.Edge.B.IsValid && locus.Edge.A.DistanceTo(locus.Edge.B) > 0.0,
        atFace: static locus => locus.Face >= 0,
        atRegion: static locus => locus.Bounds.IsValid,
        atVolume: static locus => locus.Bounds.IsValid,
        atFeature: static locus => locus.Feature is not null && locus.Key != 0,
        atDatum: static locus => locus.Key != 0,
        atLayer: static locus => locus.Layer >= 0,
        atJoint: static locus => locus.Joint >= 0,
        atSetup: static locus => locus.Setup >= 0,
        atProcess: static locus => locus.Process is not null,
        global: static _ => true);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DfmRemedy {
    private DfmRemedy() { }

    public sealed record Adjust(DfmFeature Feature, DfmCriterion Target) : DfmRemedy;
    public sealed record Reorient(Vector3d Direction) : DfmRemedy;
    public sealed record ChangeProcess(Set<ProcessKind> Candidates) : DfmRemedy;
    public sealed record Split(DfmLocus Locus) : DfmRemedy;
    public sealed record AddAccess(DfmFeature Feature) : DfmRemedy;
    public sealed record Qualify(DfmConcern Concern) : DfmRemedy;
    public sealed record Review(DfmConcern Concern) : DfmRemedy;

    public bool IsValid => Switch(
        adjust: static remedy => remedy.Feature is not null && remedy.Target is not null && remedy.Target.IsValid,
        reorient: static remedy => remedy.Direction.IsValid && remedy.Direction.Length > 0.0,
        changeProcess: static remedy => !remedy.Candidates.IsEmpty && remedy.Candidates.ForAll(static process => process is not null),
        split: static remedy => remedy.Locus is not null && remedy.Locus.IsValid,
        addAccess: static remedy => remedy.Feature is not null,
        qualify: static remedy => remedy.Concern is not null,
        review: static remedy => remedy.Concern is not null);
}

// --- [MODELS] -------------------------------------------------------------------------------------------------------------------------------------
[ComplexValueObject]
public sealed partial class DfmRule {
    public DfmConcern Concern { get; }
    public Set<DfmFeature> Features { get; }
    public Set<ModalityClass> Classes { get; }
    public DfmCriterion Criterion { get; }
    public DfmSeverity Severity { get; }
    public Option<ProcessKind> Process { get; }
    public DfmRemedy Remedy { get; }
    public double Weight { get; }
    public double MinimumConfidence { get; }
    public bool EvidenceRequired { get; }

    public bool AppliesTo(RouteCandidate candidate) => candidate is not null
        && Classes.Contains(candidate.Process.Modality.Class)
        && Process.ForAll(selected => selected == candidate.Process)
        && Features.Exists(candidate.Features.Contains);

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref DfmConcern concern,
        ref Set<DfmFeature> features,
        ref Set<ModalityClass> classes,
        ref DfmCriterion criterion,
        ref DfmSeverity severity,
        ref Option<ProcessKind> process,
        ref DfmRemedy remedy,
        ref double weight,
        ref double minimumConfidence,
        ref bool evidenceRequired) {
        if (concern is null || criterion is null || !criterion.IsValid || severity is null || remedy is null || !remedy.IsValid
            || features.IsEmpty || features.Exists(static feature => feature is null)
            || classes.IsEmpty || classes.Exists(cls => cls is null || !concern.Classes.Contains(cls))
            || process.Exists(candidate => !classes.Contains(candidate.Modality.Class))
            || !double.IsFinite(weight) || weight <= 0.0 || !double.IsFinite(minimumConfidence) || minimumConfidence is < 0.0 or > 1.0)
            validationError = ValidationError.Create("DfmRule requires feature and class coverage plus positive finite weight.");
    }
}

[ComplexValueObject]
public sealed partial class DfmObservation {
    public DfmConcern Concern { get; }
    public DfmFeature Feature { get; }
    public DfmMeasure Measure { get; }
    public Option<DfmCriterion> Criterion { get; }
    public DfmLocus Locus { get; }
    public Option<ProcessKind> Process { get; }
    public DfmEvidenceKey Evidence { get; }
    public DfmProvenance Provenance { get; }
    public double Confidence => Provenance.Confidence;
    public Instant At { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref DfmConcern concern,
        ref DfmFeature feature,
        ref DfmMeasure measure,
        ref Option<DfmCriterion> criterion,
        ref DfmLocus locus,
        ref Option<ProcessKind> process,
        ref DfmEvidenceKey evidence,
        ref DfmProvenance provenance,
        ref Instant at) {
        bool evidenceOwned = provenance is not null && concern is not null
            && !process.Exists(static value => value is null)
            && provenance.Evidence(concern, process).Map(expected => expected == evidence).IfFail(false);
        if (concern is null || feature is null || measure is null || locus is null || !locus.IsValid || !measure.IsValid
            || criterion.Exists(static value => value is null || !value.IsValid)
            || process.Exists(static value => value is null) || evidence == default || !evidenceOwned || at == default)
            validationError = ValidationError.Create("DfmObservation requires provenance-owned evidence and an observation instant.");
    }
}

[ComplexValueObject]
public sealed partial class RouteWeight {
    public double Quality { get; }
    public double Time { get; }
    public double Waste { get; }
    public double Energy { get; }
    public double Risk { get; }
    public double QualityReference { get; }
    public Duration TimeReference { get; }
    public Mass WasteReference { get; }
    public Energy EnergyReference { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref double quality,
        ref double time,
        ref double waste,
        ref double energy,
        ref double risk,
        ref double qualityReference,
        ref Duration timeReference,
        ref Mass wasteReference,
        ref Energy energyReference) {
        Seq<double> weights = Seq(quality, time, waste, energy, risk);
        if (weights.Exists(static value => !double.IsFinite(value) || value < 0.0)
            || weights.Fold(0.0, static (sum, value) => sum + value) <= 0.0
            || !double.IsFinite(qualityReference) || qualityReference <= 0.0
            || !double.IsFinite(timeReference.Seconds) || timeReference.Seconds <= 0.0
            || !double.IsFinite(wasteReference.Kilograms) || wasteReference.Kilograms <= 0.0
            || !double.IsFinite(energyReference.Joules) || energyReference.Joules <= 0.0)
            validationError = ValidationError.Create("RouteWeight requires nonnegative weights and positive normalization references.");
    }
}

[ComplexValueObject]
public sealed partial class RouteCandidate {
    public ProcessKind Process { get; }
    public ModalityPhysics Physics { get; }
    public CapabilityIdentity Capability { get; }
    public bool MaterialCompatible { get; }
    public Set<DfmFeature> Features { get; }
    public Arr<Vector3d> Approaches { get; }
    public BoundingBox WorkEnvelope { get; }
    public Mass MassCapacity { get; }
    public Duration CycleTime { get; }
    public Mass Waste { get; }
    public Energy Energy { get; }
    public double Risk { get; }
    public double YieldRate { get; }

    // Envelope conformance is a candidate fact, so the required Envelope concern derives instead of waiting on supplied evidence.
    public bool Encloses(BoundingBox part) =>
        part.IsValid && WorkEnvelope.Contains(part.Min) && WorkEnvelope.Contains(part.Max);

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref ProcessKind process,
        ref ModalityPhysics physics,
        ref CapabilityIdentity capability,
        ref bool materialCompatible,
        ref Set<DfmFeature> features,
        ref Arr<Vector3d> approaches,
        ref BoundingBox workEnvelope,
        ref Mass massCapacity,
        ref Duration cycleTime,
        ref Mass waste,
        ref Energy energy,
        ref double risk,
        ref double yieldRate) {
        bool congruent = process is not null && physics is not null && physics.Switch(
            state: process.Physics,
            subtractive: static (kind, _) => kind == PhysicsKind.Subtractive,
            thermal: static (kind, _) => kind == PhysicsKind.Thermal,
            abrasive: static (kind, _) => kind == PhysicsKind.Abrasive,
            fff: static (kind, _) => kind == PhysicsKind.Fff,
            deposition: static (kind, _) => kind == PhysicsKind.Deposition,
            joining: static (kind, _) => kind == PhysicsKind.Joining,
            erosion: static (kind, _) => kind == PhysicsKind.Erosion,
            resin: static (kind, _) => kind == PhysicsKind.Resin,
            powder: static (kind, _) => kind == PhysicsKind.Powder,
            forming: static (kind, _) => kind == PhysicsKind.Forming);
        if (!congruent || capability is null || capability.Process != process || capability.ToolState is null
            || features.IsEmpty || features.Exists(static feature => feature is null)
            || approaches.IsEmpty || approaches.Exists(static vector => !vector.IsValid || vector.Length <= 0.0)
            || !workEnvelope.IsValid || workEnvelope.Volume <= 0.0
            || !double.IsFinite(massCapacity.Kilograms) || massCapacity.Kilograms <= 0.0
            || !double.IsFinite(cycleTime.Seconds) || cycleTime.Seconds < 0.0
            || !double.IsFinite(waste.Kilograms) || waste.Kilograms < 0.0
            || !double.IsFinite(energy.Joules) || energy.Joules < 0.0
            || !double.IsFinite(risk) || risk is < 0.0 or > 1.0 || !double.IsFinite(yieldRate) || yieldRate is <= 0.0 or > 1.0)
            validationError = ValidationError.Create("RouteCandidate requires supported features, approaches, a positive work envelope and mass capacity, bounded risk and yield, and nonnegative burdens.");
    }
}

[ComplexValueObject]
public sealed partial class ToleranceDemand {
    public FeatureControl Frame { get; }
    public CapabilityIdentity Capability { get; }
    public Length Departure { get; }
    public DfmLocus Locus { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref FeatureControl frame,
        ref CapabilityIdentity capability,
        ref Length departure,
        ref DfmLocus locus) =>
        validationError = frame is not null && capability is not null
            && double.IsFinite(departure.Millimeters) && departure.Millimeters >= 0.0
            && locus is not null && locus.IsValid
                ? null
                : ValidationError.Create("ToleranceDemand requires one process-congruent capability identity and finite departure.");
}

[ComplexValueObject]
public sealed partial class AssemblyAllowance {
    public string Term { get; }
    public DfmLocus Locus { get; }
    public Length Negative { get; }
    public Length Positive { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref string term,
        ref DfmLocus locus,
        ref Length negative,
        ref Length positive) {
        term = term?.Trim() ?? string.Empty;
        if (term.Length == 0 || locus is null || !locus.IsValid
            || !double.IsFinite(negative.Millimeters) || !double.IsFinite(positive.Millimeters)
            || negative.Millimeters > 0.0 || positive.Millimeters < 0.0)
            validationError = ValidationError.Create("AssemblyAllowance requires an ordered signed interval.");
    }
}

[ComplexValueObject]
public sealed partial class DfmPolicy {
    public Seq<DfmRule> Rules { get; }
    public Seq<RouteCandidate> Candidates { get; }
    public RouteWeight RouteWeight { get; }
    public Length ProbeReach { get; }
    public Length ArcTolerance { get; }
    public Option<ToleranceChain> AssemblyChain { get; }
    public Instant At { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Seq<DfmRule> rules,
        ref Seq<RouteCandidate> candidates,
        ref RouteWeight routeWeight,
        ref Length probeReach,
        ref Length arcTolerance,
        ref Option<ToleranceChain> assemblyChain,
        ref Instant at) {
        bool ownersValid = rules.ForAll(static rule => rule is not null) && candidates.ForAll(static row => row is not null);
        bool rulesReachable = ownersValid && rules.ForAll(rule => rule.Classes.ForAll(cls =>
            candidates.Exists(row => row.Process.Modality.Class == cls && rule.AppliesTo(row))));
        // A required concern's rule must gate, so DfmVerdict can defer every consequence to severity without weakening the telos.
        bool requiredCovered = ownersValid && candidates.ForAll(row => toSeq(DfmConcern.Items)
            .Filter(concern => concern.Required && concern.AppliesTo(row.Process.Modality.Class))
            .ForAll(concern => rules.Exists(rule => rule.Concern == concern
                && rule.Severity.Gate
                && rule.AppliesTo(row))));
        if (routeWeight is null || rules.IsEmpty || candidates.IsEmpty || !double.IsFinite(probeReach.Millimeters)
            || probeReach.Millimeters <= 0.0 || !double.IsFinite(arcTolerance.Millimeters)
            || arcTolerance.Millimeters <= 0.0 || at == default
            || candidates.GroupBy(static row => row.Process).Exists(static group => group.Count() != 1)
            || !rulesReachable || !requiredCovered)
            validationError = ValidationError.Create("DfmPolicy requires unique process evidence, complete required-rule coverage, positive reach, and an assessment instant.");
    }
}

[ComplexValueObject]
public sealed partial class DfmRequest {
    public AdmittedComponent Component { get; }
    public DfmPolicy Policy { get; }
    public Seq<DfmObservation> Observations { get; }
    public Seq<DfmPackageEvidence> PackageEvidence { get; }
    public Seq<ToleranceDemand> Tolerances { get; }
    public Seq<CapabilityHistory> CapabilityHistory { get; }
    public Seq<ProcedureReceipt> Procedures { get; }
    public Seq<AssemblyAllowance> Allowances { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref AdmittedComponent component,
        ref DfmPolicy policy,
        ref Seq<DfmObservation> observations,
        ref Seq<DfmPackageEvidence> packageEvidence,
        ref Seq<ToleranceDemand> tolerances,
        ref Seq<CapabilityHistory> capabilityHistory,
        ref Seq<ProcedureReceipt> procedures,
        ref Seq<AssemblyAllowance> allowances) {
        bool packagesValid = packageEvidence.ForAll(static row => row is not null && row.IsValid);
        if (component is null || policy is null || observations.Exists(static row => row is null)
            || !packagesValid || tolerances.Exists(static row => row is null)
            || capabilityHistory.Exists(static row => row is null) || procedures.Exists(static row => row is null)
            || allowances.Exists(static row => row is null))
            validationError = ValidationError.Create("DfmRequest requires admitted owners and non-null evidence rows.");
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DfmPackageEvidence {
    private DfmPackageEvidence() { }

    public sealed record Cutter(
        ProcessKind Process,
        ToolEvidence Tool,
        DfmLocus Locus,
        Length CutterDiameter,
        Length CornerRadius,
        Length CuttingLength,
        Length RequiredReach,
        bool ContactFree,
        Instant At) : DfmPackageEvidence;

    public sealed record Voxel(
        Length MinimumWall,
        Length MinimumGap,
        int TrappedVolumes,
        bool EscapeReachable,
        bool SupportRemovable,
        Volume SolidVolume,
        BoundingBox Bounds,
        Instant At) : DfmPackageEvidence;

    public bool IsValid => Switch(
        cutter: static receipt => receipt.Process is not null && receipt.Tool is not null
            && receipt.Locus is not null && receipt.Locus.IsValid
            && double.IsFinite(receipt.CutterDiameter.Millimeters) && receipt.CutterDiameter > Length.Zero
            && double.IsFinite(receipt.CornerRadius.Millimeters) && receipt.CornerRadius >= Length.Zero
            && double.IsFinite(receipt.CuttingLength.Millimeters) && receipt.CuttingLength > Length.Zero
            && double.IsFinite(receipt.RequiredReach.Millimeters) && receipt.RequiredReach >= Length.Zero
            && receipt.At != default,
        voxel: static receipt => double.IsFinite(receipt.MinimumWall.Millimeters) && receipt.MinimumWall >= Length.Zero
            && double.IsFinite(receipt.MinimumGap.Millimeters) && receipt.MinimumGap >= Length.Zero
            && receipt.TrappedVolumes >= 0
            && double.IsFinite(receipt.SolidVolume.As(VolumeUnit.CubicMillimeter)) && receipt.SolidVolume > Volume.Zero
            && receipt.Bounds.IsValid && receipt.At != default);
}

public sealed record DfmVerdict(
    ProcessKind Process,
    DfmRule Rule,
    DfmOutcome Outcome,
    Option<DfmObservation> Observation,
    DfmCriterion Criterion,
    DfmLocus Locus,
    DfmRemedy Remedy) {
    // Severity is the single gate authority; policy admission proves every required concern carries a gating rule.
    public bool Gates => Outcome.Gate && Rule.Severity.Gate;
}

public sealed record ProcessRequirement(DfmConcern Concern, DfmFeature Feature, DfmCriterion Criterion, DfmLocus Locus);

public sealed record RouteColumn(RouteObjective Objective, double Normalized, double Weight) {
    public double Weighted => Normalized * Weight;
}

public sealed record RouteScore(Seq<RouteColumn> Columns) {
    public double Total => Columns.Fold(0.0, static (sum, column) => sum + column.Weighted);
    public Option<RouteColumn> Worst => Columns.OrderByDescending(static column => column.Weighted).Head;
}

public sealed record RoutingRow(
    ProcessKind Process,
    bool Viable,
    Seq<DfmConcern> Blockers,
    Seq<ProcessRequirement> Requirements,
    RouteScore Score);

public sealed record StackupPrecheck(
    ToleranceChain Chain,
    Length Negative,
    Length Positive,
    int RequiredAllowances,
    int ObservedAllowances,
    Seq<string> MissingTerms,
    Seq<string> UnexpectedTerms,
    bool Pass);

public sealed record DfmReport(
    UInt128 ComponentKey,
    Seq<DfmObservation> Observations,
    Seq<DfmVerdict> Verdicts,
    Seq<RoutingRow> Rows,
    Option<StackupPrecheck> Stackup,
    Instant At) {
    public Seq<ProcessKind> Routing => Rows.Filter(static row => row.Viable).OrderBy(static row => row.Score.Total).Map(static row => row.Process);
    public bool Feasible(ModalityClass cls) => Rows.Exists(row => row.Process.Modality.Class == cls && row.Viable);
}

// --- [BOUNDARIES] ---------------------------------------------------------------------------------------------------------------------------------
public static class ApproachVector {
    extension(Vector3d approach) {
        // Unitize mutates its receiver, so every probe direction enters through one detached unit projection.
        public Vector3d Unit => approach.IsValid && approach.Length > 0.0
            ? approach / approach.Length
            : Vector3d.Unset;
    }
}

// --- [OPERATIONS] ---------------------------------------------------------------------------------------------------------------------------------
public static class Manufacturability {
    internal static readonly Op DfmOp = Op.Of(name: "fabrication:manufacturability");
    private static readonly Error ObservationAfterAssessment = DfmOp.InvalidInput();
    private static readonly Error ObservationOffRoster = DfmOp.InvalidInput();
    private static readonly Error PackageEvidenceUnmatched = DfmOp.InvalidInput();
    private static readonly Error ToleranceIdentityUnmatched = DfmOp.InvalidInput();
    private static readonly Error ProcedureAfterAssessment = DfmOp.InvalidInput();
    private static readonly Error ProcedureOffRoster = DfmOp.InvalidInput();
    private static readonly Error AllowanceWithoutChain = DfmOp.InvalidInput();
    private static readonly Error AllowanceInterval = DfmOp.InvalidInput();

    public static Fin<DfmReport> Assess(DfmRequest request) =>
        from _ in Admit(request)
        from evidence in (Accumulate(Probe(request.Component, request.Policy)),
                Accumulate(PackageEvidence(request.PackageEvidence)),
                Accumulate(ToleranceEvidence(request)),
                Accumulate(ProcedureEvidence(request)))
            .Apply((derived, package, tolerance, procedure) =>
                request.Observations + derived + package + tolerance + procedure)
            .As()
            .ToFin()
        let verdicts = Evaluate(request.Policy.Rules, request.Policy.Candidates, evidence, request.Policy.At)
        let stackup = Precheck(request.Policy.AssemblyChain, request.Allowances)
        select new DfmReport(
            request.Component.RepresentationKey,
            evidence,
            verdicts,
            Route(request.Component, request.Policy, verdicts, stackup),
            stackup,
            request.Policy.At);

    private static Fin<Unit> Admit(DfmRequest request) =>
        (Check(request.Observations.ForAll(row => row.At <= request.Policy.At), ObservationAfterAssessment),
            Check(request.Observations.ForAll(row => row.Process.ForAll(candidate =>
                request.Policy.Candidates.Exists(process => process.Process == candidate))), ObservationOffRoster),
            Check(request.PackageEvidence.ForAll(row => row.Switch(
                cutter: value => value.At <= request.Policy.At
                    && request.Policy.Candidates.Exists(process => process.Process == value.Process
                        && process.Capability.ToolState == value.Tool),
                voxel: value => value.At <= request.Policy.At)), PackageEvidenceUnmatched),
            Check(request.Tolerances.ForAll(row => request.Policy.Candidates.Exists(process => process.Process == row.Capability.Process
                && process.Capability == row.Capability)), ToleranceIdentityUnmatched),
            Check(request.Procedures.ForAll(row => row.At <= request.Policy.At), ProcedureAfterAssessment),
            Check(request.Procedures.ForAll(row => request.Policy.Candidates.Exists(candidate =>
                candidate.Process == row.Process && candidate.Process.Modality.Class == ModalityClass.Joined)), ProcedureOffRoster),
            Check(request.Policy.AssemblyChain.IsSome || request.Allowances.IsEmpty, AllowanceWithoutChain),
            Check(request.Allowances.ForAll(static row => row.Negative <= row.Positive), AllowanceInterval))
            .Apply(static (_, _, _, _, _, _, _, _) => unit)
            .As()
            .ToFin();

    private static Seq<DfmVerdict> Evaluate(
        Seq<DfmRule> rules,
        Seq<RouteCandidate> candidates,
        Seq<DfmObservation> observations,
        Instant at) =>
        rules.Bind(rule => candidates
            .Filter(rule.AppliesTo)
            .Bind(candidate => {
                ProcessKind process = candidate.Process;
                Seq<DfmObservation> matching = observations.Filter(row => row.Concern == rule.Concern
                    && rule.Features.Contains(row.Feature)
                    && candidate.Features.Contains(row.Feature)
                    && row.Process.ForAll(selected => selected == process)
                    && row.At <= at);
                if (matching.IsEmpty)
                    return rule.EvidenceRequired || rule.Concern.Required
                        ? Seq(new DfmVerdict(process, rule, DfmOutcome.MissingEvidence, None, rule.Criterion, new DfmLocus.AtProcess(process), rule.Remedy))
                        : Seq<DfmVerdict>();
                Seq<DfmObservation> current = toSeq(matching.GroupBy(static row => (row.Evidence, row.Locus, row.Process)))
                    .Choose(group => toSeq(group.OrderByDescending(static row => row.At)).Head);
                return current.Map(observation => {
                    DfmCriterion criterion = observation.Criterion.IfNone(rule.Criterion);
                    DfmOutcome outcome = observation.Confidence < rule.MinimumConfidence
                        ? DfmOutcome.InsufficientConfidence
                        : criterion.Evaluate(observation.Measure).Match(
                            Some: passed => passed ? DfmOutcome.Conforming : DfmOutcome.Nonconforming,
                            None: static () => DfmOutcome.IncomparableEvidence);
                    return new DfmVerdict(process, rule, outcome, Some(observation), criterion, observation.Locus, rule.Remedy);
                });
            }));

    private static Seq<RoutingRow> Route(
        AdmittedComponent component,
        DfmPolicy policy,
        Seq<DfmVerdict> verdicts,
        Option<StackupPrecheck> stackup) =>
        policy.Candidates.Map(candidate => {
            Seq<DfmVerdict> lane = verdicts.Filter(verdict => verdict.Process == candidate.Process);
            Seq<DfmConcern> unsupported = lane.Choose(verdict => verdict.Observation
                .Filter(observation => !candidate.Features.Contains(observation.Feature))
                .Map(_ => verdict.Rule.Concern));
            Seq<DfmConcern> assembly = stackup.Exists(static row => !row.Pass)
                ? Seq(DfmConcern.AssemblyStackup)
                : Seq<DfmConcern>();
            Seq<DfmConcern> blockers = (lane.Filter(static verdict => verdict.Gates).Map(static verdict => verdict.Rule.Concern)
                + unsupported + assembly).Distinct();
            bool admitted = GeometryAdmits(component, candidate.Process.Modality.Class);
            RouteScore score = new(toSeq(RouteObjective.Items).Map(objective => new RouteColumn(
                objective,
                objective.Measure(policy.RouteWeight, candidate, lane),
                objective.Weight(policy.RouteWeight))));
            Seq<ProcessRequirement> requirements = lane.Filter(static verdict => verdict.Outcome != DfmOutcome.Conforming)
                .Map(verdict => new ProcessRequirement(
                    verdict.Rule.Concern,
                    verdict.Observation.Map(static row => row.Feature).IfNone(verdict.Rule.Features.Head.IfNone(DfmFeature.Part)),
                    verdict.Criterion,
                    verdict.Locus));
            return new RoutingRow(candidate.Process, admitted && blockers.IsEmpty, blockers, requirements, score);
        });

    private static Fin<Seq<DfmObservation>> Probe(AdmittedComponent component, DfmPolicy policy) =>
        (Accumulate(PolicyEvidence(component, policy)),
            Accumulate(ProfileEvidence(component.Profiles.ToSeq(), policy)),
            Accumulate(WallEvidence(component, policy)),
            Accumulate(RemovalEvidence(component, policy)),
            Accumulate(FormingEvidence(component, policy)),
            Accumulate(JoiningEvidence(component, policy)),
            Accumulate(AdditiveEvidence(component, policy)))
            .Apply(static (policyRows, profile, wall, removal, forming, joining, additive) =>
                policyRows + profile + wall + removal + forming + joining + additive)
            .As()
            .ToFin();

    // Achievable projects the qualifying history row's own grade, and the frame's virtual condition gates the mating boundary.
    private static Fin<Seq<DfmObservation>> ToleranceEvidence(DfmRequest request) =>
        request.Tolerances.TraverseM(row =>
            Tolerance.Apply(new ToleranceRequest.Effective(row.Frame, row.Departure.Millimeters)).Bind(receipt =>
                receipt is ToleranceReceipt.Effective effective
                    ? Capability.Achievable(row.Capability, request.Policy.At, request.CapabilityHistory)
                        .Map(achievable => Seq(
                            Observe(
                                DfmConcern.ToleranceCapability,
                                DfmFeature.Datum,
                                new DfmMeasure.Quantity(achievable),
                                row.Locus,
                                request.Policy.At,
                                DfmProvenance.History,
                                Some(row.Capability.Process),
                                Some<DfmCriterion>(new DfmCriterion.Maximum(
                                    new DfmMeasure.Quantity(Length.FromMillimeters(effective.WidthMm))))))
                            + effective.VirtualConditionMm.Map(boundary => Observe(
                                DfmConcern.ToleranceCapability,
                                DfmFeature.Datum,
                                new DfmMeasure.Quantity(Length.FromMillimeters(boundary)),
                                row.Locus,
                                request.Policy.At,
                                DfmProvenance.History,
                                Some(row.Capability.Process),
                                Some<DfmCriterion>(new DfmCriterion.Maximum(
                                    new DfmMeasure.Quantity(Length.FromMillimeters(effective.WidthMm) + row.Departure))))).ToSeq())
                        .IfNone(Seq<Fin<DfmObservation>>())
                        .TraverseM(identity)
                        .As()
                    : Fin.Fail<Seq<DfmObservation>>(DfmOp.InvalidResult())))
            .As()
            .Map(static rows => rows.Bind(identity));

    private static Fin<Seq<DfmObservation>> ProcedureEvidence(DfmRequest request) =>
        request.Procedures.TraverseM(receipt => request.Policy.Candidates
                .Find(candidate => candidate.Process == receipt.Process
                    && candidate.Process.Modality.Class == ModalityClass.Joined)
                .ToFin(ProcedureOffRoster)
                .Bind(candidate => Seq(
                    Observe(
                        DfmConcern.ProcedureQualification,
                        DfmFeature.Joint,
                        new DfmMeasure.Flag(receipt.Qualified),
                        new DfmLocus.AtProcess(candidate.Process),
                        request.Policy.At,
                        DfmProvenance.Qualification,
                        Some(candidate.Process)),
                    Observe(
                        DfmConcern.JointInspection,
                        DfmFeature.Inspection,
                        new DfmMeasure.Count(receipt.Inspections.Count),
                        new DfmLocus.AtProcess(candidate.Process),
                        request.Policy.At,
                        DfmProvenance.Qualification,
                        Some(candidate.Process)))
                    .TraverseM(identity)
                    .As()))
            .As()
            .Map(static rows => rows.Bind(identity));

    private static Fin<Seq<DfmObservation>> PackageEvidence(Seq<DfmPackageEvidence> evidence) =>
        evidence.TraverseM(row => row.Switch(
                cutter: CutterEvidence,
                voxel: VoxelEvidence))
            .As()
            .Map(static rows => rows.Bind(identity));

    private static Fin<Seq<DfmObservation>> ProfileEvidence(Seq<Loop> profiles, DfmPolicy policy) =>
        profiles.TraverseM(profile => ArcProfileEvidence(profile, policy)).As().Map(static rows => rows.Bind(identity));

    private static Fin<Seq<DfmObservation>> PolicyEvidence(AdmittedComponent component, DfmPolicy policy) =>
        PolicyRows(component, policy, Bounds(component));

    private static Fin<Seq<DfmObservation>> PolicyRows(AdmittedComponent component, DfmPolicy policy, BoundingBox part) =>
        policy.Candidates.Bind(candidate => Seq(
                Observe(
                    DfmConcern.GeometryEvidence,
                    DfmFeature.Part,
                    new DfmMeasure.Flag(GeometryAdmits(component, candidate.Process.Modality.Class)),
                    new DfmLocus.AtProcess(candidate.Process),
                    policy.At,
                    DfmProvenance.Policy,
                    Some(candidate.Process)),
                Observe(
                    DfmConcern.MaterialEvidence,
                    DfmFeature.Part,
                    new DfmMeasure.Flag(candidate.MaterialCompatible),
                    new DfmLocus.AtProcess(candidate.Process),
                    policy.At,
                    DfmProvenance.Policy,
                    Some(candidate.Process)),
                Observe(
                    DfmConcern.Envelope,
                    DfmFeature.Envelope,
                    new DfmMeasure.Flag(candidate.Encloses(part)),
                    new DfmLocus.AtRegion(part),
                    policy.At,
                    DfmProvenance.Policy,
                    Some(candidate.Process))))
            .TraverseM(identity)
            .As();

    // A degenerate profile yields no observation, never a failed report: absence lands as the rule's own missing-evidence gate.
    private static Fin<Seq<DfmObservation>> ArcProfileEvidence(Loop loop, DfmPolicy policy) =>
        (Accumulate(loop.Apply(new ProfileOp.Measure())),
                Accumulate(PolygonAlgebra.Apply(new PolygonOp.Inspect(Seq(loop), new PolygonQuery.Topology(PolygonFill.NonZero)))))
            .Apply(static (measurement, topology) => (measurement, topology))
            .As()
            .ToFin()
            .Bind(result => result is (ProfileResult.Measure measure, PolygonTrace.Regions regions) && !regions.Result.Nodes.IsEmpty
                ? Observe(
                    DfmConcern.StandardSize,
                    DfmFeature.Part,
                    new DfmMeasure.Quantity(Area.FromSquareMillimeters(Math.Abs(measure.SignedArea.SquareMillimeters))),
                    new DfmLocus.AtRegion(loop.Bound()),
                    policy.At,
                    DfmProvenance.Analytic).Map(static observation => Seq(observation))
                : Fin.Succ(Seq<DfmObservation>()));

    private static Fin<Seq<DfmObservation>> CutterEvidence(DfmPackageEvidence.Cutter receipt) => Seq(
        Observe(DfmConcern.ToolAccess, DfmFeature.Pocket, new DfmMeasure.Flag(receipt.ContactFree), receipt.Locus, receipt.At,
            DfmProvenance.Package, Some(receipt.Process)),
        Observe(DfmConcern.InternalCorner, DfmFeature.Pocket, new DfmMeasure.Quantity(receipt.CornerRadius), receipt.Locus, receipt.At,
            DfmProvenance.Package, Some(receipt.Process)),
        Observe(DfmConcern.DepthToDiameter, DfmFeature.Pocket,
            new DfmMeasure.Ratio(receipt.RequiredReach.Millimeters / receipt.CutterDiameter.Millimeters), receipt.Locus, receipt.At,
            DfmProvenance.Package, Some(receipt.Process)),
        Observe(DfmConcern.ThreadReach, DfmFeature.Thread,
            new DfmMeasure.Flag(receipt.CuttingLength >= receipt.RequiredReach), receipt.Locus, receipt.At,
            DfmProvenance.Package, Some(receipt.Process)))
        .TraverseM(identity)
        .As();

    private static Fin<Seq<DfmObservation>> VoxelEvidence(DfmPackageEvidence.Voxel receipt) => Seq(
        Observe(DfmConcern.MinimumWall, DfmFeature.Wall, new DfmMeasure.Quantity(receipt.MinimumWall),
            new DfmLocus.AtVolume(receipt.Bounds), receipt.At, DfmProvenance.Package),
        Observe(DfmConcern.MinimumFeature, DfmFeature.EnclosedVolume, new DfmMeasure.Quantity(receipt.MinimumGap),
            new DfmLocus.AtVolume(receipt.Bounds), receipt.At, DfmProvenance.Package),
        Observe(DfmConcern.SolidVolume, DfmFeature.Envelope, new DfmMeasure.Quantity(receipt.SolidVolume),
            new DfmLocus.AtVolume(receipt.Bounds), receipt.At, DfmProvenance.Package),
        Observe(DfmConcern.TrappedVolume, DfmFeature.EnclosedVolume, new DfmMeasure.Count(receipt.TrappedVolumes),
            new DfmLocus.AtVolume(receipt.Bounds), receipt.At, DfmProvenance.Package),
        Observe(DfmConcern.EscapeAccess, DfmFeature.EnclosedVolume, new DfmMeasure.Flag(receipt.EscapeReachable),
            new DfmLocus.AtVolume(receipt.Bounds), receipt.At, DfmProvenance.Package),
        Observe(DfmConcern.SupportRemoval, DfmFeature.Support, new DfmMeasure.Flag(receipt.SupportRemovable),
            new DfmLocus.AtVolume(receipt.Bounds), receipt.At, DfmProvenance.Package))
        .TraverseM(identity)
        .As();

    private static Option<StackupPrecheck> Precheck(Option<ToleranceChain> chain, Seq<AssemblyAllowance> allowances) =>
        chain.Map(owner => {
            Length negative = Length.FromMillimeters(allowances.Fold(0.0, static (sum, row) => sum + row.Negative.Millimeters));
            Length positive = Length.FromMillimeters(allowances.Fold(0.0, static (sum, row) => sum + row.Positive.Millimeters));
            Seq<string> required = owner.Terms.ToSeq().Map(static row => row.Key);
            Seq<string> observed = allowances.Map(static row => row.Term);
            Seq<string> missing = required.Filter(term => !observed.Contains(term));
            Seq<string> unexpected = observed.Filter(term => !required.Contains(term));
            bool complete = allowances.Count == owner.Terms.Count && missing.IsEmpty && unexpected.IsEmpty;
            return new StackupPrecheck(
                owner,
                negative,
                positive,
                owner.Terms.Count,
                allowances.Count,
                missing,
                unexpected,
                complete && double.Max(Math.Abs(negative.Millimeters), Math.Abs(positive.Millimeters)) <= owner.BoundMm);
        });

    // A profile the medial-axis lane cannot resolve contributes nothing; the wall rule's own gate reports the absence.
    private static Fin<Seq<DfmObservation>> WallEvidence(AdmittedComponent component, DfmPolicy policy) =>
        component.Profiles.ToSeq().TraverseM(loop =>
            ToPolyline(loop, policy.ArcTolerance).Bind(polyline =>
                Offsetting.Apply(new OffsetOp.Medial(polyline, OffsetPolicy.Canonical))
                    .Bind(result => result is OffsetResult.Axis axis
                        ? axis.Medial.Nodes.TraverseM(node => Observe(
                            DfmConcern.MinimumWall,
                            DfmFeature.Wall,
                            new DfmMeasure.Quantity(Length.FromMillimeters(2.0 * node.Radius)),
                            new DfmLocus.AtPoint(node.At),
                            policy.At,
                            DfmProvenance.Sampled)).As()
                        : Fin.Succ(Seq<DfmObservation>()))))
            .As()
            .Map(static rows => rows.Bind(identity));

    private static Fin<Seq<DfmObservation>> RemovalEvidence(AdmittedComponent component, DfmPolicy policy) =>
        policy.Candidates.Exists(static process => process.Process.Modality.Class == ModalityClass.Removal)
            ? (Accumulate(DraftEvidence(component, policy)),
                    Accumulate(AccessEvidence(component, policy)),
                    Accumulate(component.Profiles.ToSeq().TraverseM(loop => CornerEvidence(loop, policy.At)).As()
                        .Map(static rows => rows.Bind(identity))))
                .Apply(static (draft, access, corner) => draft + access + corner)
                .As()
                .ToFin()
            : Fin.Succ(Seq<DfmObservation>());

    private static Fin<Seq<DfmObservation>> DraftEvidence(AdmittedComponent component, DfmPolicy policy) =>
        component.Mesh.Match(
            None: static () => Fin.Succ(Seq<DfmObservation>()),
            Some: mesh => BottomFaces(mesh).Bind(resting => FaceNormals(mesh)
                .Filter(row => !resting.Contains(row.Face))
                .Bind(row => policy.Candidates
                    .Filter(static process => process.Process.Modality.Class == ModalityClass.Removal)
                    .Map(process => (row, process)))
                .TraverseM(pair => DraftOf(pair.row, pair.process, policy.At)).As()));

    // A degenerate normal or approach yields a non-finite angle, so the guard is live rather than decorative.
    private static Fin<DfmObservation> DraftOf((int Face, Vector3d Normal) face, RouteCandidate candidate, Instant at) =>
        candidate.Approaches.Map(approach => 90.0 - (Vector3d.VectorAngle(face.Normal, approach.Unit) * (180.0 / Math.PI))).Max()
                is var draft && double.IsFinite(draft)
            ? Observe(
                draft < 0.0 ? DfmConcern.Undercut : DfmConcern.Draft,
                DfmFeature.Surface,
                new DfmMeasure.Quantity(Angle.FromDegrees(draft)),
                new DfmLocus.AtFace(face.Face),
                at,
                DfmProvenance.Analytic,
                Some(candidate.Process))
            : Fin.Fail<DfmObservation>(DfmOp.InvalidResult());

    private static Fin<Seq<DfmObservation>> AccessEvidence(AdmittedComponent component, DfmPolicy policy) =>
        component.Mesh.Match(
            None: static () => Fin.Succ(Seq<DfmObservation>()),
            Some: mesh => BvhOf(mesh).Bind(index => component.Profiles.ToSeq()
                .Bind(static loop => loop.AsCcw().Vertices.ToSeq())
                .Bind(point => policy.Candidates.Filter(static process => process.Process.Modality.Class == ModalityClass.Removal)
                    .Bind(process => process.Approaches.Map(approach => (point, process.Process, approach))))
                .TraverseM(probe => RayHitT(
                        index,
                        new Ray3d(probe.point + (probe.approach.Unit * policy.ArcTolerance.Millimeters), probe.approach.Unit),
                        policy.ProbeReach.Millimeters)
                    .Bind(hit => Observe(
                        DfmConcern.ToolAccess,
                        DfmFeature.Pocket,
                        new DfmMeasure.Flag(hit.IsNone),
                        new DfmLocus.AtPoint(probe.point),
                        policy.At,
                        DfmProvenance.Probed,
                        Some(probe.Process)))).As()));

    private static Fin<Seq<DfmObservation>> CornerEvidence(Loop loop, Instant at) {
        Loop ccw = loop.AsCcw();
        Seq<Fin<DfmObservation>> sharp = toSeq(Enumerable.Range(0, ccw.Count)).Choose(index => {
            Point3d previous = ccw.At((index + ccw.Count - 1) % ccw.Count);
            Point3d current = ccw.At(index);
            Point3d next = ccw.At((index + 1) % ccw.Count);
            double cross = ((current.X - previous.X) * (next.Y - current.Y)) - ((current.Y - previous.Y) * (next.X - current.X));
            return cross < 0.0 && ccw.BulgeAt((index + ccw.Count - 1) % ccw.Count) == 0.0 && ccw.BulgeAt(index) == 0.0
                ? Some(Observe(DfmConcern.InternalCorner, DfmFeature.Pocket, new DfmMeasure.Quantity(Length.Zero),
                    new DfmLocus.AtPoint(current), at, DfmProvenance.Analytic))
                : None;
        });
        Seq<Fin<DfmObservation>> curved = BulgeRadii(ccw).Choose(row => ccw.BulgeAt(row.Index) < 0.0
            ? Some(Observe(DfmConcern.InternalCorner, DfmFeature.Pocket,
                new DfmMeasure.Quantity(Length.FromMillimeters(row.Radius)), new DfmLocus.AtEdge(row.Span), at, DfmProvenance.Analytic))
            : None);
        return (sharp + curved).TraverseM(identity).As();
    }

    private static Fin<Seq<DfmObservation>> FormingEvidence(AdmittedComponent component, DfmPolicy policy) =>
        policy.Candidates.Exists(static process => process.Process.Modality.Class == ModalityClass.Formed)
            ? component.SheetThicknessMm.Match(
                Some: thickness => component.Profiles.ToSeq().Bind(BulgeRadii).TraverseM(row => Observe(
                    DfmConcern.BendRadius,
                    DfmFeature.Bend,
                    new DfmMeasure.Ratio(row.Radius / thickness),
                    new DfmLocus.AtEdge(row.Span),
                    policy.At,
                    DfmProvenance.Analytic)).As(),
                None: static () => Fin.Succ(Seq<DfmObservation>()))
            : Fin.Succ(Seq<DfmObservation>());

    private static Fin<Seq<DfmObservation>> JoiningEvidence(AdmittedComponent component, DfmPolicy policy) =>
        policy.Candidates.Exists(static process => process.Process.Modality.Class == ModalityClass.Joined)
            ? component.Mesh.Match(
                None: static () => Fin.Succ(Seq<DfmObservation>()),
                Some: mesh => BvhOf(mesh).Bind(index => policy.Candidates
                    .Filter(static process => process.Process.Modality.Class == ModalityClass.Joined)
                    .Bind(process => toSeq(component.Connections).Map((connection, joint) => (connection, joint, process)))
                    .TraverseM(row => ConeClear(index, row.connection.At, row.process.Approaches, policy.ProbeReach, policy.ArcTolerance)
                        .Bind(clear => Observe(
                            DfmConcern.WeldAccess,
                            DfmFeature.Joint,
                            new DfmMeasure.Flag(clear),
                            new DfmLocus.AtJoint(row.joint),
                            policy.At,
                            DfmProvenance.Probed,
                            Some(row.process.Process))))
                    .As()))
            : Fin.Succ(Seq<DfmObservation>());

    private static Fin<Seq<DfmObservation>> AdditiveEvidence(AdmittedComponent component, DfmPolicy policy) =>
        policy.Candidates.Exists(static process => process.Process.Modality.Class == ModalityClass.Additive)
            ? component.Mesh.Match(
                Some: mesh => (Accumulate(OverhangEvidence(mesh, policy)), Accumulate(IntegrityEvidence(mesh, policy.At)))
                    .Apply(static (overhang, integrity) => overhang + integrity)
                    .As()
                    .ToFin(),
                None: static () => Fin.Succ(Seq<DfmObservation>()))
            : Fin.Succ(Seq<DfmObservation>());

    // Build direction is chosen per candidate as the approach maximizing the worst face angle, then every face reports against it.
    private static Fin<Seq<DfmObservation>> OverhangEvidence(MeshSpace mesh, DfmPolicy policy) =>
        OverhangRows(FaceNormals(mesh), policy);

    private static Fin<Seq<DfmObservation>> OverhangRows(Seq<(int Face, Vector3d Normal)> faces, DfmPolicy policy) =>
        guard(!faces.IsEmpty, DfmOp.InvalidInput()).ToFin().Bind(_ => policy.Candidates
                .Filter(static candidate => candidate.Process.Modality.Class == ModalityClass.Additive)
                .Bind(candidate => candidate.Approaches
                    .Map(approach => (Direction: approach.Unit,
                        Worst: faces.Map(row => Vector3d.VectorAngle(row.Normal, -approach.Unit)).Min()))
                    .OrderByDescending(static row => row.Worst)
                    .Head
                    .ToSeq()
                    .Bind(build => faces.Map(row => Observe(
                        DfmConcern.Overhang,
                        DfmFeature.Overhang,
                        new DfmMeasure.Quantity(Angle.FromRadians(Vector3d.VectorAngle(row.Normal, -build.Direction))),
                        new DfmLocus.AtFace(row.Face),
                        policy.At,
                        DfmProvenance.Analytic,
                        Some(candidate.Process)))))
            .TraverseM(identity)
            .As());

    private static Fin<Seq<DfmObservation>> IntegrityEvidence(MeshSpace mesh, Instant at) =>
        Analyze.Run<Mesh, MeshSample>(new AnalysisQuery.MeshesCase(Meshes.Defects), mesh.Native)
            .ToFin()
            .Bind(samples => samples.TraverseM(sample => Observe(
                DfmConcern.Integrity,
                DfmFeature.Part,
                new DfmMeasure.Count(sample.Value),
                new DfmLocus.Global(),
                at,
                DfmProvenance.Analytic)).As());

    // Provenance mints the evidence key and carries the confidence its derivation route earns; no caller spells either.
    private static Fin<DfmObservation> Observe(
        DfmConcern concern,
        DfmFeature feature,
        DfmMeasure measure,
        DfmLocus locus,
        Instant at,
        DfmProvenance provenance,
        Option<ProcessKind> process = default,
        Option<DfmCriterion> criterion = default) =>
        from evidence in provenance.Evidence(concern, process)
        from observation in Admitted(concern, feature, measure, criterion, locus, process, evidence, provenance, at)
        select observation;

    private static Fin<DfmObservation> Admitted(
        DfmConcern concern,
        DfmFeature feature,
        DfmMeasure measure,
        Option<DfmCriterion> criterion,
        DfmLocus locus,
        Option<ProcessKind> process,
        DfmEvidenceKey evidence,
        DfmProvenance provenance,
        Instant at) =>
        DfmObservation.Validate(concern, feature, measure, criterion, locus, process, evidence, provenance, at,
            out DfmObservation observation) is { }
                ? Fin.Fail<DfmObservation>(DfmOp.InvalidInput())
                : Fin.Succ(observation);

    // Total over the modality family: a new ModalityClass row breaks this dispatch at compile time rather than admitting silently.
    private static bool GeometryAdmits(AdmittedComponent component, ModalityClass modality) =>
        modality.Switch(
            state: component,
            removal: static part => part.Mesh.IsSome || !part.Profiles.IsEmpty,
            additive: static part => part.Mesh.IsSome,
            formed: static part => part.SheetThicknessMm.IsSome && !part.Profiles.IsEmpty,
            joined: static part => part.Mesh.IsSome && !part.Connections.IsEmpty);

    private static BoundingBox Bounds(AdmittedComponent component) =>
        component.Mesh.Match(
            Some: static mesh => mesh.Native.GetBoundingBox(accurate: true),
            None: () => component.Profiles.ToSeq().Fold(BoundingBox.Empty, static (box, loop) => {
                box.Union(loop.Bound());
                return box;
            }));

    private static Fin<bool> ConeClear(
        SpatialIndex index,
        Edge3 at,
        Arr<Vector3d> approaches,
        Length reach,
        Length clearance) =>
        at.A.DistanceTo(at.B) > 0.0
            ? approaches.TraverseM(approach => {
                    Vector3d direction = approach.Unit;
                    Point3d midpoint = at.A + ((at.B - at.A) * 0.5);
                    Point3d origin = midpoint + (direction * clearance.Millimeters);
                    return RayHitT(index, new Ray3d(origin, direction), reach.Millimeters).Map(static hit => hit.IsNone);
                })
                .Map(static answers => answers.Exists(identity))
                .As()
            : Fin.Fail<bool>(DfmOp.InvalidInput());

    private static Fin<Polyline> ToPolyline(Loop loop, Length tolerance) {
        double path = loop.Length();
        int count = int.Max(loop.Spans, (int)Math.Ceiling(path / tolerance.Millimeters));
        return toSeq(Enumerable.Range(0, count)).TraverseM(index =>
                loop.Apply(new ProfileOp.Sample(Length.FromMillimeters(path * index / count)))
                    .Bind(result => result is ProfileResult.Sampled sample
                        ? Fin.Succ(sample.Point)
                        : Fin.Fail<Point3d>(DfmOp.InvalidResult())))
            .As()
            .Map(points => {
                Polyline ring = new(points);
                if (ring.Count > 0)
                    ring.Add(ring[0]);
                return ring;
            });
    }

    private static Fin<Set<int>> BottomFaces(MeshSpace mesh) =>
        Analyze.Run<Mesh, int>(new AnalysisQuery.FacesCase(Faces.Bottom()), mesh.Native).ToFin().Map(static faces => toSet(faces));

    private static Seq<(int Face, Vector3d Normal)> FaceNormals(MeshSpace mesh) {
        Mesh native = mesh.Native.DuplicateMesh();
        native.FaceNormals.ComputeFaceNormals();
        return toSeq(Enumerable.Range(0, native.Faces.Count)).Map(index => (index, (Vector3d)native.FaceNormals[index]));
    }

    private static Fin<SpatialIndex> BvhOf(MeshSpace mesh) =>
        Spatial.Apply(new SpatialOp.Build(SpatialKind.Bvh, FaceBoxes(mesh.Native), BuildPolicy.Canonical), DfmOp)
            .Bind(static answer => answer is SpatialAnswer.Index built
                ? Fin.Succ(built.Value)
                : Fin.Fail<SpatialIndex>(DfmOp.InvalidResult()));

    private static Fin<Option<double>> RayHitT(SpatialIndex index, Ray3d ray, double maxT) =>
        Spatial.Apply(new SpatialOp.Query(index, new SpatialQuery.Ray(ray, maxT)), DfmOp)
            .Bind(static answer => answer switch {
                SpatialAnswer.Result { Value: QueryResult.RayHit { Id.IsSome: true } hit } => Fin.Succ(Some(hit.T)),
                SpatialAnswer.Result { Value: QueryResult.RayHit } => Fin.Succ(Option<double>.None),
                _ => Fin.Fail<Option<double>>(DfmOp.InvalidResult()),
            });

    private static BoundingBox[] FaceBoxes(Mesh native) =>
        Enumerable.Range(0, native.Faces.Count).Select(index => {
            MeshFace face = native.Faces[index];
            BoundingBox box = BoundingBox.Empty;
            box.Union(native.Vertices[face.A]);
            box.Union(native.Vertices[face.B]);
            box.Union(native.Vertices[face.C]);
            if (face.IsQuad)
                box.Union(native.Vertices[face.D]);
            return box;
        }).ToArray();

    private static Seq<(int Index, Edge3 Span, double Radius)> BulgeRadii(Loop loop) =>
        toSeq(Enumerable.Range(0, loop.Count)).Choose(index => {
            double bulge = Math.Abs(loop.BulgeAt(index));
            if (bulge == 0.0)
                return None;
            double chord = loop.At(index).DistanceTo(loop.At(index + 1));
            return Some((index, new Edge3(loop.At(index), loop.At(index + 1)), chord * (1.0 + (bulge * bulge)) / (4.0 * bulge)));
        });

    // One error per gate keeps accumulation informative; a shared error collapses every fault into one indistinguishable row.
    private static K<Validation<Error>, Unit> Check(bool condition, Error fault) =>
        guard(condition, fault).ToValidation();

    private static K<Validation<Error>, T> Accumulate<T>(Fin<T> effect) =>
        effect.ToValidation();
}
```
