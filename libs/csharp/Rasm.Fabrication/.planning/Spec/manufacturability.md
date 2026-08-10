# [RASM_FABRICATION_MANUFACTURABILITY]

`Manufacturability` owns evidence-backed producibility from admitted component geometry and supplied domain observations through parameterized rule evaluation, remediation, process-requirement ranking, assembly precheck, and one terminal `DfmReport`. Missing, insufficient-confidence, or incomparable evidence remains an explicit gate state; no absent lane reads as conforming.

`Analyze`, `Offsetting`, and `Spatial` remain geometry-kernel owners and `MeshSpace` the mesh subject every kernel query takes. `Capability.Achievable` owns process-history projection through the qualifying row's own `ItGrade` and effective sample size, `Tolerance.Apply(ToleranceRequest.Effective)` owns material-condition departure and virtual condition, `ToleranceChain.Evaluate` owns the stackup algebra this page composes rather than forks, `ProcedureReceipt.Qualified` owns weld-procedure compliance, `ModalityPhysics` owns process physics, and `Kinematics/fleet` owns machine matching. `DfmReport.Routing` crosses the derivation seam as ranked `ProcessKind` evidence.

## [01]-[INDEX]

- [02]-[DFM_VOCABULARY]: severity, outcome, feature, and concern rows; the derivation-route census and the resolution algebra that grades every reading; the weighted routing objectives.
- [03]-[EVIDENCE_MODELS]: typed measure and criterion, locus, remedy, rule, observation, route candidate, routing weights, policy, request, and the sidecar package receipts.
- [04]-[ASSESSMENT]: `Manufacturability.Assess`, the derived-evidence folds over one mesh-facts scratch, verdict evaluation, route ranking, and the stackup precheck composing `ToleranceChain.Evaluate`.

## [02]-[DFM_VOCABULARY]

- Owner: `DfmSeverity` owns gating and penalty; `DfmOutcome` owns the five evaluation states; `DfmFeature` and `DfmConcern` close the domain vocabulary; `DfmProvenance` owns the derivation-route census and the exactness each route admits; `DfmResolution` owns the confidence algebra; `RouteObjective` owns the weighted routing columns.
- Law: confidence is what a reading's OWN resolution earns, never a constant the route carries. The measure divided by the step it was resolved at is the count of independent resolution elements behind it, one element is no evidence and an exact derivation has no step at all — so a wall resolved at one sample and a wall resolved at a hundred can never report the same trust, and `DfmRule.MinimumConfidence` becomes a demand on resolution rather than a demand on which lane happened to answer.
- Law: a derivation route declares whether it CAN be exact, so a sampled, probed, packaged, or projected reading handing an exact resolution fails admission instead of laundering its own approximation.
- Law: `DfmConcern` carries one row per producibility question with the modality classes it applies to and whether it gates; `DfmVerdict.Gates` defers every consequence to `DfmSeverity`, and `DfmPolicy` admission proves each required concern carries a gating rule — the invariant that no absent lane reads as conforming holds structurally instead of by outcome-kind override.
- Law: `RouteObjective` rows carry their own yield-adjusted measurement and weight selector, EVERY column dividing by its own `RouteWeight` reference so the column reaches the fold dimensionless and comparable, and `RouteScore.Total` is the weighted burden where LOWER is better — the one ranking polarity `MachineMatch.Score` and `CellPlacementCandidate.Score` also carry, so `Worst` names the dominant burden on every surface and a new routing dimension is one row with no scoring expression re-spelled.
- Growth: a concern is one `DfmConcern` seed; a feature is one `DfmFeature` seed; a derivation route is one `DfmProvenance` row declaring its exactness; a routing dimension is one `RouteObjective` row beside its reference column.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------------------------------------------------------
using System.Linq;
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

    // The quality objective sums penalties against `RouteWeight.QualityReference`, so the ratio between rows is the
    // only fact these values carry: an advisory is one unit of burden, a warning three, a blocker ten.
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
    public static readonly DfmConcern AssemblyStackup = Any("assembly-stackup", required: true);

    public Set<ModalityClass> Classes { get; }
    public bool Required { get; }

    public bool AppliesTo(ModalityClass cls) => Classes.Contains(cls);

    private static DfmConcern Any(string key, bool required) =>
        For(key, required, ModalityClass.Removal, ModalityClass.Additive, ModalityClass.Formed, ModalityClass.Joined);

    private static DfmConcern For(string key, bool required, params ModalityClass[] classes) =>
        new(key, toSet(classes), required);
}

// The resolution BEHIND one reading. Confidence is the fraction of the measure the route's own step does not
// account for, so a reading spanning one resolution element carries no confidence at all and one spanning many
// approaches the exact limit. A route-constant confidence is the hollowed form: it grades the LANE and never the
// reading, so it reports identical trust for a wall sampled once and a wall sampled a hundred times.
public readonly record struct DfmResolution(double Measured, double Step) {
    // The exact derivation has no step: a closed form over admitted geometry, a caller's own declaration, a
    // receipt's own verdict. `Measured` is the unit magnitude such a reading resolves at.
    public static readonly DfmResolution Exact = new(1.0, 0.0);

    public static DfmResolution Of(double measured, double step) => new(measured, step);

    // A counted route resolves in WHOLE elements — approach directions, prior samples — so its step is one element.
    public static DfmResolution Counted(int elements) => new(elements, 1.0);

    public bool IsExact => Step == 0.0;

    public double Confidence => IsExact
        ? 1.0
        : 1.0 - (1.0 / double.Max(1.0, Math.Abs(Measured) / Step));

    public bool Valid => double.IsFinite(Measured) && double.IsFinite(Step) && Step >= 0.0
        && (IsExact || Math.Abs(Measured) > 0.0);
}

// Derivation route: the evidence-key namespace and the exactness the route is ALLOWED to claim. The confidence a
// reading earns rides its own resolution, so this row never carries a number.
[SmartEnum<string>]
public sealed partial class DfmProvenance {
    public static readonly DfmProvenance Analytic = new("analytic", exact: true);
    public static readonly DfmProvenance Policy = new("policy", exact: true);
    public static readonly DfmProvenance Qualification = new("qualification", exact: true);
    public static readonly DfmProvenance Package = new("package", exact: false);
    public static readonly DfmProvenance History = new("capability-history", exact: false);
    public static readonly DfmProvenance Sampled = new("sampled", exact: false);
    public static readonly DfmProvenance Probed = new("probed", exact: false);

    public bool Exact { get; }

    // A discretized route handing an exact resolution would launder its own approximation, and an exact derivation
    // handing a step would understate a reading that has none, so the correspondence is admission law.
    public bool Admits(DfmResolution resolution) => resolution.Valid && Exact == resolution.IsExact;
}

// The evidence identity is the CLOSED PRODUCT of route, concern, and process — three generated owners — so it
// compares, groups, and orders by value with no interpolation, no admission, and no per-observation mint. A
// rendered key exists only where a refusal locus needs one, and it renders once at that refusal.
public readonly record struct DfmEvidenceKey(DfmProvenance Route, DfmConcern Concern, Option<ProcessKind> Process) {
    public string Locus => Process.Match(
        Some: process => $"{Route.Key}:{Concern.Key}:{process.Key}",
        None: () => $"{Route.Key}:{Concern.Key}");
}

// One weighted, yield-adjusted objective per row: every column divides by its own reference, so the fold sums
// comparable dimensionless burdens and a new routing dimension is a row beside its reference.
[SmartEnum<string>]
public sealed partial class RouteObjective {
    public static readonly RouteObjective Quality = new("quality",
        static (weight, candidate, lane) => lane.Fold(0.0, static (sum, verdict) => sum
                + (verdict.Outcome == DfmOutcome.Conforming ? 0.0 : verdict.Rule.Severity.Penalty * verdict.Rule.Weight))
            / (weight.QualityReference * candidate.YieldRate),
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
    // Risk is a probability, so its reference is the risk a route is willing to carry rather than a unit: the
    // column reaching the fold un-referenced was the one row whose burden could not be compared with its peers.
    public static readonly RouteObjective Risk = new("risk",
        static (weight, candidate, _) => candidate.Risk / (weight.RiskReference * candidate.YieldRate),
        static weight => weight.Risk);

    public Func<RouteWeight, RouteCandidate, Seq<DfmVerdict>, double> Measure { get; }
    public Func<RouteWeight, double> Weight { get; }
}
```

## [03]-[EVIDENCE_MODELS]

- Owner: `DfmMeasure` and `DfmCriterion` close typed comparison; `DfmLocus` closes the addressing family; `DfmRemedy` closes remediation; `DfmRule` parameterizes applicability, evidence obligation, minimum confidence, severity, and remedy; `DfmObservation` preserves one measured reading with its route and resolution; `RouteCandidate` carries one process alternative; `RouteWeight` carries the routing references; `DfmPolicy`, `DfmRequest`, and `DfmPackageEvidence` carry the admitted request.
- Cases: removal covers access, draft, undercut, feature depth, corner radius, thread, finish, stock, datum, and inspection constraints; additive covers feature size, wall, overhang, bridge, enclosed volume, escape, support removal, recoater, anisotropy, thermal, and integrity constraints; forming covers bend radius, edge and hole distance, flange, hem, draw, grain, tonnage, springback, and thinning constraints; joining covers access, root gap, throat, heat input, distortion, qualification, inspection, and assembly constraints.
- Law: `DfmLocus` addresses a POINT, an EDGE, a FACE, a BOUNDED REGION, a keyed feature, a datum, a layer, a joint, a setup, a process, or the whole part — a second bounded case distinguished only by the word volume carried the same payload under a second name and is the deleted form.
- Law: resting faces and drafted faces read ONE normal roster and partition it — the resting test is strictly downward so a vertical wall stays in the drafted census, and the draft demand measures the COMPLEMENT of the excluded set rather than the excluded set itself.
- Law: `RouteCandidate` admission UNITIZES its approach directions, so every consumer reads a unit direction by construction and no probe re-derives one or guards a sentinel the derivation could return.
- Law: a sidecar package receipt states the RESOLUTION it measured at — a voxel edge, a cutter-contact step — so the confidence its observations earn is the sidecar's own discretization rather than a constant this page assigns to the word package.
- Auto: `DfmPolicy` proves every required concern has a generic or process-specific gating rule and every rule reaches at least one candidate; `DfmCriterion.Evaluate` compares unit-bearing, count, ratio, and flag measures; `RouteCandidate.Encloses` derives the `DfmConcern.Envelope` verdict from the candidate's own work volume, so a mesh-only part is never blocked for want of supplied operating envelope evidence.
- Packages: `Loop.Apply` composes CavalierContours arc-native measurement and sampling; `PolygonAlgebra.Apply` composes Clipper2 topology; `DfmPackageEvidence.Cutter` carries OpenCAMLib cutter-contact evidence against canonical `ToolEvidence`; `DfmPackageEvidence.Voxel` carries PicoGK morphology, membership, ray, and solid-property evidence; UnitsNet owns every physical comparison; Thinktecture and LanguageExt own generated values and the accumulated rail.
- Growth: a policy variation is one `DfmRule` row; a process candidate is one `RouteCandidate` row; a sidecar family is one `DfmPackageEvidence` case carrying its own resolution column.
- Boundary: sidecar OpenCAMLib and PicoGK owners lower native handles into `DfmPackageEvidence` before this host-local owner consumes them; every owner refuses onto `FabricationFault` under `FabConcern.Spec`.

```csharp signature
// --- [MEASUREMENT] --------------------------------------------------------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DfmMeasure {
    private DfmMeasure() { }

    public sealed record Quantity(IQuantity Value) : DfmMeasure;
    public sealed record Ratio(double Value) : DfmMeasure;
    public sealed record Count(int Value) : DfmMeasure;
    public sealed record Flag(bool Value) : DfmMeasure;

    public bool IsValid => Switch(
        quantity: static quantity => double.IsFinite((double)quantity.Value.Value),
        ratio: static ratio => double.IsFinite(ratio.Value),
        count: static count => count.Value >= 0,
        flag: static _ => true);

    // The magnitude a resolution grades against; a flag resolves as one whole element and never as a length.
    public double Magnitude => Switch(
        quantity: static quantity => Math.Abs((double)quantity.Value.Value),
        ratio: static ratio => Math.Abs(ratio.Value),
        count: static count => count.Value,
        flag: static _ => 1.0);
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
        minimum: static criterion => criterion.Bound.IsValid,
        maximum: static criterion => criterion.Bound.IsValid,
        band: static criterion => criterion.Lower.IsValid && criterion.Upper.IsValid
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

    // Two readings compare only inside one quantity family at one dimension; anything else is incomparable
    // evidence, which is a REPORTED outcome rather than a refusal.
    private static Option<int> Compare(DfmMeasure left, DfmMeasure right) =>
        (left, right) switch {
            (DfmMeasure.Quantity a, DfmMeasure.Quantity b)
                when a.Value.QuantityInfo.Name == b.Value.QuantityInfo.Name
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
    public sealed record AtBounds(BoundingBox Bounds) : DfmLocus;
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
        atBounds: static locus => locus.Bounds.IsValid,
        atFeature: static locus => locus.Key != 0,
        atDatum: static locus => locus.Key != 0,
        atLayer: static locus => locus.Layer >= 0,
        atJoint: static locus => locus.Joint >= 0,
        atSetup: static locus => locus.Setup >= 0,
        atProcess: static _ => true,
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
        adjust: static remedy => remedy.Target.IsValid,
        reorient: static remedy => remedy.Direction.IsValid && remedy.Direction.Length > 0.0,
        changeProcess: static remedy => !remedy.Candidates.IsEmpty,
        split: static remedy => remedy.Locus.IsValid,
        addAccess: static _ => true,
        qualify: static _ => true,
        review: static _ => true);
}

// --- [MODELS] -------------------------------------------------------------------------------------------------------------------------------------
[ComplexValueObject]
[ValidationError<FabricationFault>]
public sealed partial class DfmRule {
    public DfmConcern Concern { get; }
    public Set<DfmFeature> Features { get; }
    public Set<ModalityClass> Classes { get; }
    public DfmCriterion Criterion { get; }
    public DfmSeverity Severity { get; }
    public Option<ProcessKind> Process { get; }
    public DfmRemedy Remedy { get; }
    public double Weight { get; }

    // A demand on RESOLUTION: a rule asking 0.9 asks for ten independent resolution elements behind the reading,
    // which a route declares by its own step rather than by which lane answered.
    public double MinimumConfidence { get; }

    public bool EvidenceRequired { get; }

    public bool AppliesTo(RouteCandidate candidate) =>
        Classes.Contains(candidate.Process.Modality.Class)
        && Process.ForAll(selected => selected == candidate.Process)
        && Features.Exists(candidate.Features.Contains);

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref FabricationFault? validationError,
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
        if (!criterion.IsValid || !remedy.IsValid
            || features.IsEmpty || classes.IsEmpty
            || classes.Exists(cls => !concern.Classes.Contains(cls))
            || process.Exists(candidate => !classes.Contains(candidate.Modality.Class))
            || !double.IsFinite(weight) || weight <= 0.0
            || !double.IsFinite(minimumConfidence) || minimumConfidence is < 0.0 or > 1.0)
            validationError = Manufacturability.Refusal("rule");
    }
}

[ComplexValueObject]
[ValidationError<FabricationFault>]
public sealed partial class DfmObservation {
    public DfmConcern Concern { get; }
    public DfmFeature Feature { get; }
    public DfmMeasure Measure { get; }
    public Option<DfmCriterion> Criterion { get; }
    public DfmLocus Locus { get; }
    public Option<ProcessKind> Process { get; }
    public DfmProvenance Provenance { get; }
    public DfmResolution Resolution { get; }
    public Instant At { get; }

    // Identity and trust both DERIVE: the key is the closed product this row already carries, and the confidence
    // is what its own resolution earned. Neither is a stored column a producer could contradict.
    public DfmEvidenceKey Evidence => new(Provenance, Concern, Process);
    public double Confidence => Resolution.Confidence;

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref FabricationFault? validationError,
        ref DfmConcern concern,
        ref DfmFeature feature,
        ref DfmMeasure measure,
        ref Option<DfmCriterion> criterion,
        ref DfmLocus locus,
        ref Option<ProcessKind> process,
        ref DfmProvenance provenance,
        ref DfmResolution resolution,
        ref Instant at) {
        if (!measure.IsValid || !locus.IsValid
            || criterion.Exists(static value => !value.IsValid)
            || !provenance.Admits(resolution) || at == default)
            validationError = Manufacturability.Refusal("observation");
    }
}

[ComplexValueObject]
[ValidationError<FabricationFault>]
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

    // The failure probability a route is willing to carry; every burden column divides by its own reference, so
    // risk stops being the one column that reaches the fold in its own units.
    public double RiskReference { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref FabricationFault? validationError,
        ref double quality,
        ref double time,
        ref double waste,
        ref double energy,
        ref double risk,
        ref double qualityReference,
        ref Duration timeReference,
        ref Mass wasteReference,
        ref Energy energyReference,
        ref double riskReference) {
        Seq<double> weights = Seq(quality, time, waste, energy, risk);
        if (weights.Exists(static value => !double.IsFinite(value) || value < 0.0)
            || weights.Fold(0.0, static (sum, value) => sum + value) <= 0.0
            || !Witness.Positive(qualityReference) || !Witness.Positive(timeReference.Seconds)
            || !Witness.Positive(wasteReference.Kilograms) || !Witness.Positive(energyReference.Joules)
            || !Witness.Positive(riskReference) || riskReference > 1.0)
            validationError = Manufacturability.Refusal("route-weight");
    }
}

[ComplexValueObject]
[ValidationError<FabricationFault>]
public sealed partial class RouteCandidate {
    public ProcessKind Process { get; }
    public ModalityPhysics Physics { get; }
    public CapabilityIdentity Capability { get; }
    public bool MaterialCompatible { get; }
    public Set<DfmFeature> Features { get; }

    // UNIT directions by admission: every probe, draft, and build-orientation lane reads these directly.
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

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref FabricationFault? validationError,
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
        bool directed = approaches.ForAll(static vector => vector.IsValid && vector.Length > 0.0);
        // The normalization is the admission, so `Vector3d.Unset` never leaves this owner and no consumer guards it.
        if (directed)
            approaches = approaches.Map(static vector => vector / vector.Length);
        bool congruent = physics.Switch(
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
        if (!congruent || capability.Process != process
            || features.IsEmpty || approaches.IsEmpty || !directed
            || !workEnvelope.IsValid || workEnvelope.Volume <= 0.0
            || !Witness.Positive(massCapacity.Kilograms)
            || !double.IsFinite(cycleTime.Seconds) || cycleTime.Seconds < 0.0
            || !double.IsFinite(waste.Kilograms) || waste.Kilograms < 0.0
            || !double.IsFinite(energy.Joules) || energy.Joules < 0.0
            || !double.IsFinite(risk) || risk is < 0.0 or > 1.0
            || !double.IsFinite(yieldRate) || yieldRate is <= 0.0 or > 1.0)
            validationError = Manufacturability.Refusal("route-candidate");
    }
}

[ComplexValueObject]
[ValidationError<FabricationFault>]
public sealed partial class ToleranceDemand {
    public FeatureControl Frame { get; }
    public CapabilityIdentity Capability { get; }
    public Length Departure { get; }
    public DfmLocus Locus { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref FabricationFault? validationError,
        ref FeatureControl frame,
        ref CapabilityIdentity capability,
        ref Length departure,
        ref DfmLocus locus) {
        if (!double.IsFinite(departure.Millimeters) || departure.Millimeters < 0.0 || !locus.IsValid)
            validationError = Manufacturability.Refusal("tolerance-demand");
    }
}

[ComplexValueObject]
[ValidationError<FabricationFault>]
public sealed partial class AssemblyAllowance {
    public string Term { get; }
    public DfmLocus Locus { get; }
    public Length Negative { get; }
    public Length Positive { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref FabricationFault? validationError,
        ref string term,
        ref DfmLocus locus,
        ref Length negative,
        ref Length positive) {
        term = term.Trim();
        if (!Witness.Keyed(term) || !locus.IsValid
            || !double.IsFinite(negative.Millimeters) || !double.IsFinite(positive.Millimeters)
            || negative.Millimeters > 0.0 || positive.Millimeters < 0.0)
            validationError = Manufacturability.Refusal("assembly-allowance");
    }
}

[ComplexValueObject]
[ValidationError<FabricationFault>]
public sealed partial class DfmPolicy {
    public Seq<DfmRule> Rules { get; }
    public Seq<RouteCandidate> Candidates { get; }
    public RouteWeight RouteWeight { get; }
    public Length ProbeReach { get; }

    // The chord step every flattened profile and every probe origin resolves at: it is the STEP a sampled or
    // probed reading grades against, so one policy value decides both the geometry and the confidence it earns.
    public Length ArcTolerance { get; }

    public Option<ToleranceChain> AssemblyChain { get; }
    public Instant At { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref FabricationFault? validationError,
        ref Seq<DfmRule> rules,
        ref Seq<RouteCandidate> candidates,
        ref RouteWeight routeWeight,
        ref Length probeReach,
        ref Length arcTolerance,
        ref Option<ToleranceChain> assemblyChain,
        ref Instant at) {
        bool rulesReachable = rules.ForAll(rule => rule.Classes.ForAll(cls =>
            candidates.Exists(row => row.Process.Modality.Class == cls && rule.AppliesTo(row))));
        // A required concern's rule must gate, so DfmVerdict defers every consequence to severity without weakening that invariant.
        bool requiredCovered = candidates.ForAll(row => toSeq(DfmConcern.Items)
            .Filter(concern => concern.Required && concern.AppliesTo(row.Process.Modality.Class))
            .ForAll(concern => rules.Exists(rule => rule.Concern == concern && rule.Severity.Gate && rule.AppliesTo(row))));
        if (rules.IsEmpty || candidates.IsEmpty
            || !Witness.Positive(probeReach.Millimeters) || !Witness.Positive(arcTolerance.Millimeters)
            || at == default
            || candidates.Map(static row => row.Process).Distinct().Count != candidates.Count
            || !rulesReachable || !requiredCovered)
            validationError = Manufacturability.Refusal("policy");
    }
}

[ComplexValueObject]
[ValidationError<FabricationFault>]
public sealed partial class DfmRequest {
    public AdmittedComponent Component { get; }
    public DfmPolicy Policy { get; }
    public Seq<DfmObservation> Observations { get; }
    public Seq<DfmPackageEvidence> PackageEvidence { get; }
    public Seq<ToleranceDemand> Tolerances { get; }
    public Seq<CapabilityHistory> CapabilityHistory { get; }
    public Seq<ProcedureReceipt> Procedures { get; }
    public Seq<AssemblyAllowance> Allowances { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref FabricationFault? validationError,
        ref AdmittedComponent component,
        ref DfmPolicy policy,
        ref Seq<DfmObservation> observations,
        ref Seq<DfmPackageEvidence> packageEvidence,
        ref Seq<ToleranceDemand> tolerances,
        ref Seq<CapabilityHistory> capabilityHistory,
        ref Seq<ProcedureReceipt> procedures,
        ref Seq<AssemblyAllowance> allowances) {
        if (packageEvidence.Exists(static row => !row.IsValid))
            validationError = Manufacturability.Refusal("request");
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DfmPackageEvidence {
    private DfmPackageEvidence() { }

    // `Resolution` is the sidecar's OWN step — the cutter-contact sampling pitch, the voxel edge — so every
    // observation this receipt yields grades against the discretization that produced it.
    public sealed record Cutter(
        ProcessKind Process,
        ToolEvidence Tool,
        DfmLocus Locus,
        Length CutterDiameter,
        Length CornerRadius,
        Length CuttingLength,
        Length RequiredReach,
        bool ContactFree,
        Length Resolution,
        Instant At) : DfmPackageEvidence;

    public sealed record Voxel(
        Length MinimumWall,
        Length MinimumGap,
        int TrappedVolumes,
        bool EscapeReachable,
        bool SupportRemovable,
        Volume SolidVolume,
        BoundingBox Bounds,
        Length Resolution,
        Instant At) : DfmPackageEvidence;

    public Length Step => Switch(
        cutter: static receipt => receipt.Resolution,
        voxel: static receipt => receipt.Resolution);

    public Instant At => Switch(
        cutter: static receipt => receipt.At,
        voxel: static receipt => receipt.At);

    public bool IsValid => Switch(
        cutter: static receipt => receipt.Locus.IsValid
            && Witness.Positive(receipt.CutterDiameter.Millimeters)
            && double.IsFinite(receipt.CornerRadius.Millimeters) && receipt.CornerRadius >= Length.Zero
            && Witness.Positive(receipt.CuttingLength.Millimeters)
            && double.IsFinite(receipt.RequiredReach.Millimeters) && receipt.RequiredReach >= Length.Zero
            && Witness.Positive(receipt.Resolution.Millimeters) && receipt.At != default,
        voxel: static receipt => double.IsFinite(receipt.MinimumWall.Millimeters) && receipt.MinimumWall >= Length.Zero
            && double.IsFinite(receipt.MinimumGap.Millimeters) && receipt.MinimumGap >= Length.Zero
            && receipt.TrappedVolumes >= 0
            && Witness.Positive(receipt.SolidVolume.As(VolumeUnit.CubicMillimeter))
            && receipt.Bounds.IsValid && Witness.Positive(receipt.Resolution.Millimeters) && receipt.At != default);
}
```

## [04]-[ASSESSMENT]

- Owner: `Manufacturability.Assess` owns the cross-modality fold; `DfmVerdict` owns one rule-against-evidence decision; `RoutingRow` and `RouteScore` own ranking; `StackupPrecheck` owns the assembly-allowance verdict; `DfmReport` owns the terminal receipt.
- Law: the stackup precheck COMPOSES `ToleranceChain.Evaluate` — the chain's own method, its ranked contributions, and its bound verdict — and adds only what this page owns: whether the supplied allowances cover the chain's terms and whether their accumulated interval clears the same bound. A local worst-case fold here would be a third stackup algebra disagreeing with the two that already answer.
- Law: mesh-derived evidence reads ONE scratch per assessment. Face normals have no kernel query — the `Faces` family decomposes BREP faces, whose index space is not a mesh's — so one native copy is taken, its normals computed once, and the draft, resting, overhang, access, and joint lanes all read those rows and one spatial index; bounds and defect samples ride the kernel queries with the `MeshSpace` subject and its own copy.
- Law: resting faces derive from the SAME normal rows the draft census reads, so the excluded set and the measured set share one index space — a face selection recovered from a second decomposition indexes a different topology and silently excludes the wrong faces.
- Law: every gate refusal carries its OWN discriminant. The kernel `InvalidInput`/`InvalidResult` mints take no detail slot, so gates lowering onto them are refusals a caller cannot tell apart; each answers on the fabrication band under a declared locus.
- Law: a degenerate profile, an unresolvable medial axis, or absent history contributes no observation rather than failing the report, so producibility gaps stay report rows and only kernel faults leave the rail.
- Exemption: `MeshFacts.Built`, `Manufacturability.CornerEvidence`, `Manufacturability.ToPolyline`, and `MeshFacts.FaceBoxes` are statement kernels — one native copy, one index walk, one sampling loop; every other body on this cluster is expression-shaped.
- Entry: `Manufacturability.Assess(DfmRequest)` is the sole cross-modality fold. Geometry, capability, supplied evidence, and assembly allowances join applicatively; kernel failures remain typed `Fin` failures, while producibility failures remain report rows.
- Receipt: `DfmVerdict` preserves process, confidence outcome, observation, criterion, locus, and remedy; `RoutingRow` preserves blockers, requirements, and the `RouteScore` column set whose `Worst` names the dominant burden; `StackupPrecheck` preserves the chain receipt beside the allowance census; `DfmReport` preserves the full decision basis.
- Boundary: routing ranks process requirements and evidence, while fleet matching, tool selection, support generation, unfolding, joining sequence, correlated stackup simulation, rendering, and persistence remain downstream owners.

```csharp signature
// --- [RECEIPTS] -----------------------------------------------------------------------------------------------------------------------------------
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
    public Option<RouteColumn> Worst => Columns.Fold(Option<RouteColumn>.None,
        static (best, column) => best.Filter(held => held.Weighted >= column.Weighted).IfNone(column));
}

public sealed record RoutingRow(
    ProcessKind Process,
    bool Viable,
    Seq<DfmConcern> Blockers,
    Seq<ProcessRequirement> Requirements,
    RouteScore Score);

// The chain's own receipt beside the allowance census this page owns: the method, the ranked contributions, and
// the bound verdict all arrive from `ToleranceChain.Evaluate`, so a failed precheck names the dominating term
// without a second simulation and without a second worst-case fold.
public sealed record StackupPrecheck(
    ChainReceipt Chain,
    Length Negative,
    Length Positive,
    int RequiredAllowances,
    int ObservedAllowances,
    Seq<string> MissingTerms,
    Seq<string> UnexpectedTerms) {
    public bool Complete => MissingTerms.IsEmpty && UnexpectedTerms.IsEmpty
        && ObservedAllowances == RequiredAllowances;

    public bool Pass => Complete && Chain.Conforming
        && double.Max(Math.Abs(Negative.Millimeters), Math.Abs(Positive.Millimeters)) <= Chain.BoundMm;

    public Option<(string Term, double Share)> Dominant => Chain.Dominant;
}

public sealed record DfmReport(
    UInt128 ComponentKey,
    Seq<DfmObservation> Observations,
    Seq<DfmVerdict> Verdicts,
    Seq<RoutingRow> Rows,
    Option<StackupPrecheck> Stackup,
    Instant At) {
    public Seq<ProcessKind> Routing => toSeq(Rows.Filter(static row => row.Viable)
        .OrderBy(static row => row.Score.Total).Select(static row => row.Process));
    public bool Feasible(ModalityClass cls) => Rows.Exists(row => row.Process.Modality.Class == cls && row.Viable);
}

// --- [OPERATIONS] ---------------------------------------------------------------------------------------------------------------------------------
public static class Manufacturability {
    internal static readonly Op DfmOp = Op.Of(name: "fabrication:manufacturability");

    // Every refusal names ITS OWN condition on the fabrication band; a shared kernel mint collapses distinct
    // faults into one indistinguishable row a caller cannot act on.
    internal static FabricationFault Refusal(string locus) =>
        new FabricationFault.PolicyInadmissible(FabConcern.Spec, $"manufacturability:{locus}");

    private static readonly Error ObservationAfterAssessment = Refusal("observation-after-assessment");
    private static readonly Error ObservationOffRoster = Refusal("observation-off-roster");
    private static readonly Error PackageEvidenceUnmatched = Refusal("package-evidence-unmatched");
    private static readonly Error ToleranceIdentityUnmatched = Refusal("tolerance-identity-unmatched");
    private static readonly Error ProcedureAfterAssessment = Refusal("procedure-after-assessment");
    private static readonly Error ProcedureOffRoster = Refusal("procedure-off-roster");
    private static readonly Error AllowanceWithoutChain = Refusal("allowance-without-chain");
    private static readonly Error AllowanceInterval = Refusal("allowance-interval");
    internal static readonly Error MeshBounds = Refusal("mesh-bounds");
    internal static readonly Error MeshIndex = Refusal("mesh-index");
    private static readonly Error MeshDefects = Refusal("mesh-defects");
    private static readonly Error ProfileSample = Refusal("profile-sample");
    private static readonly Error ProbeRay = Refusal("probe-ray");
    private static readonly Error DegenerateApproach = Refusal("degenerate-approach");
    private static readonly Error DegenerateJoint = Refusal("degenerate-joint");
    private static readonly Error EffectiveTolerance = Refusal("effective-tolerance");

    public static Fin<DfmReport> Assess(DfmRequest request) =>
        from _ in Admit(request)
        from facts in MeshFacts.Of(request.Component, request.Policy)
        from evidence in (Accumulate(Derived(request, facts)),
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
            Check(request.PackageEvidence.ForAll(row => row.At <= request.Policy.At && row.Switch(
                cutter: value => request.Policy.Candidates.Exists(process => process.Process == value.Process
                    && process.Capability.ToolState == value.Tool),
                voxel: static _ => true)), PackageEvidenceUnmatched),
            Check(request.Tolerances.ForAll(row => request.Policy.Candidates.Exists(process =>
                process.Capability == row.Capability)), ToleranceIdentityUnmatched),
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
                        ? Seq(new DfmVerdict(process, rule, DfmOutcome.MissingEvidence, None, rule.Criterion,
                            new DfmLocus.AtProcess(process), rule.Remedy))
                        : Seq<DfmVerdict>();
                // One reading per evidence identity and locus: the latest observation of a repeated measurement
                // supersedes its predecessors rather than voting beside them.
                Seq<DfmObservation> current = toSeq(matching.GroupBy(static row => (row.Evidence, row.Locus)))
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
            return new RoutingRow(
                candidate.Process,
                GeometryAdmits(component, candidate.Process.Modality.Class) && blockers.IsEmpty,
                blockers,
                requirements,
                score);
        });

    // The allowance census this page owns, over the chain receipt the chain owner derived. `Complete` is the
    // coverage half and `Pass` folds it with the chain's own verdict, so neither half re-derives the other.
    private static Option<StackupPrecheck> Precheck(Option<ToleranceChain> chain, Seq<AssemblyAllowance> allowances) =>
        chain.Map(owner => {
            Seq<string> required = owner.Terms.ToSeq().Map(static row => row.Key);
            Seq<string> observed = allowances.Map(static row => row.Term);
            return new StackupPrecheck(
                owner.Evaluate(),
                Length.FromMillimeters(allowances.Fold(0.0, static (sum, row) => sum + row.Negative.Millimeters)),
                Length.FromMillimeters(allowances.Fold(0.0, static (sum, row) => sum + row.Positive.Millimeters)),
                required.Count,
                allowances.Count,
                required.Filter(term => !observed.Contains(term)),
                observed.Filter(term => !required.Contains(term)));
        });

    // --- [DERIVED_EVIDENCE]
    private static Fin<Seq<DfmObservation>> Derived(DfmRequest request, Option<MeshFacts> facts) =>
        (Accumulate(PolicyRows(request.Component, request.Policy, facts)),
            Accumulate(ProfileEvidence(request.Component.Profiles.ToSeq(), request.Policy)),
            Accumulate(WallEvidence(request.Component, request.Policy)),
            Accumulate(RemovalEvidence(request.Component, request.Policy, facts)),
            Accumulate(FormingEvidence(request.Component, request.Policy)),
            Accumulate(JoiningEvidence(request.Component, request.Policy, facts)),
            Accumulate(AdditiveEvidence(request.Policy, facts)))
            .Apply(static (policyRows, profile, wall, removal, forming, joining, additive) =>
                policyRows + profile + wall + removal + forming + joining + additive)
            .As()
            .ToFin();

    private static Fin<Seq<DfmObservation>> PolicyRows(AdmittedComponent component, DfmPolicy policy, Option<MeshFacts> facts) =>
        Bounds(component, facts).Bind(part => policy.Candidates.Bind(candidate => Seq(
                Observe(
                    DfmConcern.GeometryEvidence,
                    DfmFeature.Part,
                    new DfmMeasure.Flag(GeometryAdmits(component, candidate.Process.Modality.Class)),
                    new DfmLocus.AtProcess(candidate.Process),
                    policy.At,
                    DfmProvenance.Policy,
                    DfmResolution.Exact,
                    Some(candidate.Process)),
                Observe(
                    DfmConcern.MaterialEvidence,
                    DfmFeature.Part,
                    new DfmMeasure.Flag(candidate.MaterialCompatible),
                    new DfmLocus.AtProcess(candidate.Process),
                    policy.At,
                    DfmProvenance.Policy,
                    DfmResolution.Exact,
                    Some(candidate.Process)),
                Observe(
                    DfmConcern.Envelope,
                    DfmFeature.Envelope,
                    new DfmMeasure.Flag(candidate.Encloses(part)),
                    new DfmLocus.AtBounds(part),
                    policy.At,
                    DfmProvenance.Policy,
                    DfmResolution.Exact,
                    Some(candidate.Process))))
            .TraverseM(identity)
            .As());

    // Achievable projects the qualifying history row's own grade and sample count, so the tolerance observation
    // grades on the evidence behind it; the frame's virtual condition gates the mating boundary.
    private static Fin<Seq<DfmObservation>> ToleranceEvidence(DfmRequest request) =>
        request.Tolerances.TraverseM(row =>
            Tolerance.Apply(new ToleranceRequest.Effective(row.Frame, row.Departure.Millimeters)).Bind(receipt =>
                receipt is ToleranceReceipt.Effective effective
                    ? Capability.Achievable(row.Capability, request.Policy.At, request.CapabilityHistory)
                        .Map(achievable => Seq(
                            Observe(
                                DfmConcern.ToleranceCapability,
                                DfmFeature.Datum,
                                new DfmMeasure.Quantity(achievable.Width),
                                row.Locus,
                                request.Policy.At,
                                DfmProvenance.History,
                                DfmResolution.Counted((int)achievable.EffectiveSampleSize),
                                Some(row.Capability.Process),
                                Some<DfmCriterion>(new DfmCriterion.Maximum(
                                    new DfmMeasure.Quantity(Length.FromMillimeters(effective.WidthMm)))))
                            + effective.VirtualConditionMm.Map(boundary => Observe(
                                DfmConcern.ToleranceCapability,
                                DfmFeature.Datum,
                                new DfmMeasure.Quantity(Length.FromMillimeters(boundary)),
                                row.Locus,
                                request.Policy.At,
                                DfmProvenance.History,
                                DfmResolution.Counted((int)achievable.EffectiveSampleSize),
                                Some(row.Capability.Process),
                                Some<DfmCriterion>(new DfmCriterion.Maximum(
                                    new DfmMeasure.Quantity(Length.FromMillimeters(effective.WidthMm) + row.Departure))))).ToSeq())
                        .IfNone(Seq<Fin<DfmObservation>>())
                        .TraverseM(identity)
                        .As()
                    : Fin.Fail<Seq<DfmObservation>>(EffectiveTolerance)))
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
                        DfmResolution.Exact,
                        Some(candidate.Process)),
                    Observe(
                        DfmConcern.JointInspection,
                        DfmFeature.Inspection,
                        new DfmMeasure.Count(receipt.Inspections.Count),
                        new DfmLocus.AtProcess(candidate.Process),
                        request.Policy.At,
                        DfmProvenance.Qualification,
                        DfmResolution.Exact,
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

    // A degenerate profile yields no observation, never a failed report: absence lands as the rule's own missing-evidence gate.
    private static Fin<Seq<DfmObservation>> ArcProfileEvidence(Loop loop, DfmPolicy policy) =>
        (Accumulate(loop.Apply(new ProfileOp.Measure())),
                Accumulate(PolygonAlgebra.Apply(new PolygonOp.Topology(Seq(loop), PolygonFill.NonZero))))
            .Apply(static (measurement, topology) => (measurement, topology))
            .As()
            .ToFin()
            .Bind(result => result is (ProfileResult.Measure measure, PolygonTrace.Regions regions) && !regions.Result.Nodes.IsEmpty
                ? Observe(
                    DfmConcern.StandardSize,
                    DfmFeature.Part,
                    new DfmMeasure.Quantity(Area.FromSquareMillimeters(Math.Abs(measure.SignedArea.SquareMillimeters))),
                    new DfmLocus.AtBounds(loop.Bound()),
                    policy.At,
                    DfmProvenance.Analytic,
                    DfmResolution.Exact).Map(static observation => Seq(observation))
                : Fin.Succ(Seq<DfmObservation>()));

    private static Fin<Seq<DfmObservation>> CutterEvidence(DfmPackageEvidence.Cutter receipt) => Seq(
        Observe(DfmConcern.ToolAccess, DfmFeature.Pocket, new DfmMeasure.Flag(receipt.ContactFree), receipt.Locus, receipt.At,
            DfmProvenance.Package, Step(receipt.CutterDiameter, receipt.Resolution), Some(receipt.Process)),
        Observe(DfmConcern.InternalCorner, DfmFeature.Pocket, new DfmMeasure.Quantity(receipt.CornerRadius), receipt.Locus, receipt.At,
            DfmProvenance.Package, Step(receipt.CornerRadius, receipt.Resolution), Some(receipt.Process)),
        Observe(DfmConcern.DepthToDiameter, DfmFeature.Pocket,
            new DfmMeasure.Ratio(receipt.RequiredReach.Millimeters / receipt.CutterDiameter.Millimeters), receipt.Locus, receipt.At,
            DfmProvenance.Package, Step(receipt.RequiredReach, receipt.Resolution), Some(receipt.Process)),
        Observe(DfmConcern.ThreadReach, DfmFeature.Thread,
            new DfmMeasure.Flag(receipt.CuttingLength >= receipt.RequiredReach), receipt.Locus, receipt.At,
            DfmProvenance.Package, Step(receipt.CuttingLength, receipt.Resolution), Some(receipt.Process)))
        .TraverseM(identity)
        .As();

    private static Fin<Seq<DfmObservation>> VoxelEvidence(DfmPackageEvidence.Voxel receipt) => Seq(
        Observe(DfmConcern.MinimumWall, DfmFeature.Wall, new DfmMeasure.Quantity(receipt.MinimumWall),
            new DfmLocus.AtBounds(receipt.Bounds), receipt.At, DfmProvenance.Package, Step(receipt.MinimumWall, receipt.Resolution)),
        Observe(DfmConcern.MinimumFeature, DfmFeature.EnclosedVolume, new DfmMeasure.Quantity(receipt.MinimumGap),
            new DfmLocus.AtBounds(receipt.Bounds), receipt.At, DfmProvenance.Package, Step(receipt.MinimumGap, receipt.Resolution)),
        Observe(DfmConcern.SolidVolume, DfmFeature.Envelope, new DfmMeasure.Quantity(receipt.SolidVolume),
            new DfmLocus.AtBounds(receipt.Bounds), receipt.At, DfmProvenance.Package,
            // A volume resolves in CUBED voxels, so its element count is the cube of its linear one.
            DfmResolution.Of(receipt.SolidVolume.As(VolumeUnit.CubicMillimeter),
                Math.Pow(receipt.Resolution.Millimeters, 3.0))),
        Observe(DfmConcern.TrappedVolume, DfmFeature.EnclosedVolume, new DfmMeasure.Count(receipt.TrappedVolumes),
            new DfmLocus.AtBounds(receipt.Bounds), receipt.At, DfmProvenance.Package,
            Step(receipt.Bounds.Diagonal.Length, receipt.Resolution)),
        Observe(DfmConcern.EscapeAccess, DfmFeature.EnclosedVolume, new DfmMeasure.Flag(receipt.EscapeReachable),
            new DfmLocus.AtBounds(receipt.Bounds), receipt.At, DfmProvenance.Package,
            Step(receipt.MinimumGap, receipt.Resolution)),
        Observe(DfmConcern.SupportRemoval, DfmFeature.Support, new DfmMeasure.Flag(receipt.SupportRemovable),
            new DfmLocus.AtBounds(receipt.Bounds), receipt.At, DfmProvenance.Package,
            Step(receipt.MinimumGap, receipt.Resolution)))
        .TraverseM(identity)
        .As();

    // A profile the medial-axis lane cannot resolve contributes nothing; the wall rule's own gate reports the
    // absence, and each node grades against the chord step its polyline was flattened at.
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
                            DfmProvenance.Sampled,
                            Step(Length.FromMillimeters(2.0 * node.Radius), policy.ArcTolerance))).As()
                        : Fin.Succ(Seq<DfmObservation>()))))
            .As()
            .Map(static rows => rows.Bind(identity));

    private static Fin<Seq<DfmObservation>> RemovalEvidence(
        AdmittedComponent component, DfmPolicy policy, Option<MeshFacts> facts) =>
        policy.Candidates.Exists(static process => process.Process.Modality.Class == ModalityClass.Removal)
            ? (Accumulate(DraftEvidence(policy, facts)),
                    Accumulate(AccessEvidence(component, policy, facts)),
                    Accumulate(component.Profiles.ToSeq().TraverseM(loop => CornerEvidence(loop, policy.At)).As()
                        .Map(static rows => rows.Bind(identity))))
                .Apply(static (draft, access, corner) => draft + access + corner)
                .As()
                .ToFin()
            : Fin.Succ(Seq<DfmObservation>());

    // Resting faces and drafted faces read the SAME normal rows, so the excluded set indexes the measured set —
    // and the measured set is what REMAINS. Censusing the standing faces themselves demands draft of exactly the
    // faces the part sits on and asks none of the walls that need it.
    private static Fin<Seq<DfmObservation>> DraftEvidence(DfmPolicy policy, Option<MeshFacts> facts) =>
        facts.Match(
            None: static () => Fin.Succ(Seq<DfmObservation>()),
            Some: mesh => mesh.Drafted
                .Bind(row => policy.Candidates
                    .Filter(static process => process.Process.Modality.Class == ModalityClass.Removal)
                    .Map(process => (row, process)))
                .TraverseM(pair => DraftOf(pair.row, pair.process, policy.At)).As());

    // A degenerate normal yields a non-finite angle, so the guard is live rather than decorative.
    private static Fin<DfmObservation> DraftOf((int Face, Vector3d Normal) face, RouteCandidate candidate, Instant at) =>
        candidate.Approaches.Max(approach => 90.0 - (Vector3d.VectorAngle(face.Normal, approach) * (180.0 / Math.PI)))
                is var draft && double.IsFinite(draft)
            ? Observe(
                draft < 0.0 ? DfmConcern.Undercut : DfmConcern.Draft,
                DfmFeature.Surface,
                new DfmMeasure.Quantity(Angle.FromDegrees(draft)),
                new DfmLocus.AtFace(face.Face),
                at,
                DfmProvenance.Analytic,
                DfmResolution.Exact,
                Some(candidate.Process))
            : Fin.Fail<DfmObservation>(DegenerateApproach);

    // Access grades on the DIRECTIONS tested: one ray is an unsupported claim, and the count is what the rule's
    // minimum confidence reads.
    private static Fin<Seq<DfmObservation>> AccessEvidence(
        AdmittedComponent component, DfmPolicy policy, Option<MeshFacts> facts) =>
        facts.Match(
            None: static () => Fin.Succ(Seq<DfmObservation>()),
            Some: mesh => component.Profiles.ToSeq()
                .Bind(static loop => loop.AsCcw().Vertices.ToSeq())
                .Bind(point => policy.Candidates.Filter(static process => process.Process.Modality.Class == ModalityClass.Removal)
                    .Map(process => (point, process)))
                .TraverseM(probe => probe.process.Approaches
                    .TraverseM(approach => RayHitT(
                        mesh.Index,
                        new Ray3d(probe.point + (approach * policy.ArcTolerance.Millimeters), approach),
                        policy.ProbeReach.Millimeters))
                    .As()
                    .Bind(hits => Observe(
                        DfmConcern.ToolAccess,
                        DfmFeature.Pocket,
                        new DfmMeasure.Flag(hits.Exists(static hit => hit.IsNone)),
                        new DfmLocus.AtPoint(probe.point),
                        policy.At,
                        DfmProvenance.Probed,
                        DfmResolution.Counted(probe.process.Approaches.Count),
                        Some(probe.process.Process))))
                .As());

    private static Fin<Seq<DfmObservation>> CornerEvidence(Loop loop, Instant at) {
        Loop ccw = loop.AsCcw();
        Seq<Fin<DfmObservation>> sharp = toSeq(Enumerable.Range(0, ccw.Count)).Choose(index => {
            Point3d previous = ccw.At((index + ccw.Count - 1) % ccw.Count);
            Point3d current = ccw.At(index);
            Point3d next = ccw.At((index + 1) % ccw.Count);
            double cross = ((current.X - previous.X) * (next.Y - current.Y)) - ((current.Y - previous.Y) * (next.X - current.X));
            return cross < 0.0 && ccw.BulgeAt((index + ccw.Count - 1) % ccw.Count) == 0.0 && ccw.BulgeAt(index) == 0.0
                ? Some(Observe(DfmConcern.InternalCorner, DfmFeature.Pocket, new DfmMeasure.Quantity(Length.Zero),
                    new DfmLocus.AtPoint(current), at, DfmProvenance.Analytic, DfmResolution.Exact))
                : None;
        });
        Seq<Fin<DfmObservation>> curved = BulgeRadii(ccw).Choose(row => ccw.BulgeAt(row.Index) < 0.0
            ? Some(Observe(DfmConcern.InternalCorner, DfmFeature.Pocket,
                new DfmMeasure.Quantity(Length.FromMillimeters(row.Radius)), new DfmLocus.AtEdge(row.Span), at,
                DfmProvenance.Analytic, DfmResolution.Exact))
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
                    DfmProvenance.Analytic,
                    DfmResolution.Exact)).As(),
                None: static () => Fin.Succ(Seq<DfmObservation>()))
            : Fin.Succ(Seq<DfmObservation>());

    private static Fin<Seq<DfmObservation>> JoiningEvidence(
        AdmittedComponent component, DfmPolicy policy, Option<MeshFacts> facts) =>
        policy.Candidates.Exists(static process => process.Process.Modality.Class == ModalityClass.Joined)
            ? facts.Match(
                None: static () => Fin.Succ(Seq<DfmObservation>()),
                Some: mesh => policy.Candidates
                    .Filter(static process => process.Process.Modality.Class == ModalityClass.Joined)
                    .Bind(process => toSeq(component.Connections).Map((connection, joint) => (connection, joint, process)))
                    .TraverseM(row => ConeClear(mesh.Index, row.connection.At, row.process.Approaches, policy)
                        .Bind(clear => Observe(
                            DfmConcern.WeldAccess,
                            DfmFeature.Joint,
                            new DfmMeasure.Flag(clear),
                            new DfmLocus.AtJoint(row.joint),
                            policy.At,
                            DfmProvenance.Probed,
                            DfmResolution.Counted(row.process.Approaches.Count),
                            Some(row.process.Process))))
                    .As())
            : Fin.Succ(Seq<DfmObservation>());

    // Build direction is chosen per candidate as the approach maximizing the worst face angle, then every face
    // reports against it; the reading is exact once the direction is fixed.
    private static Fin<Seq<DfmObservation>> AdditiveEvidence(DfmPolicy policy, Option<MeshFacts> facts) =>
        policy.Candidates.Exists(static process => process.Process.Modality.Class == ModalityClass.Additive)
            ? facts.Match(
                None: static () => Fin.Succ(Seq<DfmObservation>()),
                Some: mesh => (Accumulate(OverhangEvidence(mesh, policy)), Accumulate(IntegrityEvidence(mesh, policy.At)))
                    .Apply(static (overhang, integrity) => overhang + integrity)
                    .As()
                    .ToFin())
            : Fin.Succ(Seq<DfmObservation>());

    private static Fin<Seq<DfmObservation>> OverhangEvidence(MeshFacts mesh, DfmPolicy policy) =>
        policy.Candidates
            .Filter(static candidate => candidate.Process.Modality.Class == ModalityClass.Additive)
            .Bind(candidate => candidate.Approaches
                .Map(approach => (Direction: approach,
                    Worst: mesh.Faces.Min(row => Vector3d.VectorAngle(row.Normal, -approach))))
                .Fold(Option<(Vector3d Direction, double Worst)>.None, static (best, row) =>
                    best.Filter(held => held.Worst >= row.Worst).IfNone(row))
                .ToSeq()
                .Bind(build => mesh.Faces.Map(row => Observe(
                    DfmConcern.Overhang,
                    DfmFeature.Overhang,
                    new DfmMeasure.Quantity(Angle.FromRadians(Vector3d.VectorAngle(row.Normal, -build.Direction))),
                    new DfmLocus.AtFace(row.Face),
                    policy.At,
                    DfmProvenance.Analytic,
                    DfmResolution.Exact,
                    Some(candidate.Process)))))
            .TraverseM(identity)
            .As();

    private static Fin<Seq<DfmObservation>> IntegrityEvidence(MeshFacts mesh, Instant at) =>
        Analyze.Run<Mesh, MeshSample>(AnalysisQuery.MeshPointSpatial(Meshes.Defects), mesh.Native)
            .ToFin()
            .MapFail(static _ => MeshDefects)
            .Bind(samples => samples.TraverseM(sample => Observe(
                DfmConcern.Integrity,
                DfmFeature.Part,
                new DfmMeasure.Count(sample.Value),
                new DfmLocus.Global(),
                at,
                DfmProvenance.Analytic,
                DfmResolution.Exact)).As());

    // --- [DERIVATION_SUPPORT]
    // One reading, one route, one resolution: the key derives from the row and the confidence from the step, so a
    // caller spells neither and no site mints an evidence identity.
    private static Fin<DfmObservation> Observe(
        DfmConcern concern,
        DfmFeature feature,
        DfmMeasure measure,
        DfmLocus locus,
        Instant at,
        DfmProvenance provenance,
        DfmResolution resolution,
        Option<ProcessKind> process = default,
        Option<DfmCriterion> criterion = default) =>
        DfmObservation.Validate(concern, feature, measure, criterion, locus, process, provenance, resolution, at,
            out DfmObservation observation).Admitted(observation);

    private static DfmResolution Step(Length measured, Length step) =>
        DfmResolution.Of(measured.Millimeters, step.Millimeters);

    private static DfmResolution Step(double measuredMm, Length step) =>
        DfmResolution.Of(measuredMm, step.Millimeters);

    // Total over the modality family: a new ModalityClass row breaks this dispatch at compile time rather than admitting silently.
    private static bool GeometryAdmits(AdmittedComponent component, ModalityClass modality) =>
        modality.Switch(
            state: component,
            removal: static part => part.Mesh.IsSome || !part.Profiles.IsEmpty,
            additive: static part => part.Mesh.IsSome,
            formed: static part => part.SheetThicknessMm.IsSome && !part.Profiles.IsEmpty,
            joined: static part => part.Mesh.IsSome && !part.Connections.IsEmpty);

    // Bounds ride the kernel query with the `MeshSpace` subject; a profile-only component folds its own loop bounds.
    private static Fin<BoundingBox> Bounds(AdmittedComponent component, Option<MeshFacts> facts) =>
        facts.Map(static mesh => Fin.Succ(mesh.Bounds))
            .IfNone(() => Fin.Succ(component.Profiles.ToSeq().Fold(BoundingBox.Empty, static (box, loop) => {
                box.Union(loop.Bound());
                return box;
            })));

    private static Fin<bool> ConeClear(SpatialIndex index, Edge3 at, Arr<Vector3d> approaches, DfmPolicy policy) =>
        at.A.DistanceTo(at.B) > 0.0
            ? approaches.TraverseM(direction => RayHitT(
                    index,
                    new Ray3d(at.A + ((at.B - at.A) * 0.5) + (direction * policy.ArcTolerance.Millimeters), direction),
                    policy.ProbeReach.Millimeters))
                .Map(static answers => answers.Exists(static hit => hit.IsNone))
                .As()
            : Fin.Fail<bool>(DegenerateJoint);

    private static Fin<Polyline> ToPolyline(Loop loop, Length tolerance) {
        double path = loop.Length();
        int count = int.Max(loop.Spans, (int)Math.Ceiling(path / tolerance.Millimeters));
        return toSeq(Enumerable.Range(0, count)).TraverseM(index =>
                loop.Apply(new ProfileOp.Sample(Length.FromMillimeters(path * index / count)))
                    .Bind(result => result is ProfileResult.Sampled sample
                        ? Fin.Succ(sample.Point)
                        : Fin.Fail<Point3d>(ProfileSample)))
            .As()
            .Map(points => {
                Polyline ring = new(points);
                if (ring.Count > 0)
                    ring.Add(ring[0]);
                return ring;
            });
    }

    private static Fin<Option<double>> RayHitT(SpatialIndex index, Ray3d ray, double maxT) =>
        Spatial.Apply(new SpatialOp.Query(index, new SpatialQuery.Ray(ray, maxT)), DfmOp)
            .Bind(static answer => answer switch {
                SpatialAnswer.Result { Value: QueryResult.RayHit { Id.IsSome: true } hit } => Fin.Succ(Some(hit.T)),
                SpatialAnswer.Result { Value: QueryResult.RayHit } => Fin.Succ(Option<double>.None),
                _ => Fin.Fail<Option<double>>(ProbeRay),
            });

    private static Seq<(int Index, Edge3 Span, double Radius)> BulgeRadii(Loop loop) =>
        toSeq(Enumerable.Range(0, loop.Count)).Choose(index => {
            double bulge = Math.Abs(loop.BulgeAt(index));
            if (bulge == 0.0)
                return None;
            double chord = loop.At(index).DistanceTo(loop.At(index + 1));
            return Some((index, new Edge3(loop.At(index), loop.At(index + 1)), chord * (1.0 + (bulge * bulge)) / (4.0 * bulge)));
        });

    private static K<Validation<Error>, Unit> Check(bool condition, Error fault) =>
        guard(condition, fault).ToValidation();

    private static K<Validation<Error>, T> Accumulate<T>(Fin<T> effect) =>
        effect.ToValidation();
}

// The ONE mesh scratch per assessment. `MeshSpace.Native` is internal and face normals have no kernel query — the
// `Faces` family decomposes BREP faces, whose index space is not a mesh's — so one detached copy carries the face
// normals, the resting-face split, the spatial index, and the defect subject, and every mesh lane reads it.
internal sealed record MeshFacts(
    Mesh Native,
    Seq<(int Face, Vector3d Normal)> Faces,
    SpatialIndex Index,
    BoundingBox Bounds) {
    // ONE partition over ONE roster. A face the part RESTS on faces the build plate — its normal points down —
    // and carries no draft demand, so the drafted census is its complement. A face at exactly zero is a vertical
    // wall, which is the face draft matters most for, and it lands drafted rather than excluded.
    public Seq<(int Face, Vector3d Normal)> Drafted =>
        Faces.Filter(static row => Vector3d.Multiply(row.Normal, Vector3d.ZAxis) >= 0.0);

    public static Fin<Option<MeshFacts>> Of(AdmittedComponent component, DfmPolicy policy) =>
        component.Mesh.Match(
            None: static () => Fin.Succ(Option<MeshFacts>.None),
            Some: space => Built(space).Map(Some));

    private static Fin<MeshFacts> Built(MeshSpace space) {
        Mesh native = space.DuplicateNative();
        native.FaceNormals.ComputeFaceNormals();
        Seq<(int Face, Vector3d Normal)> faces = toSeq(Enumerable.Range(0, native.Faces.Count))
            .Map(index => (index, (Vector3d)native.FaceNormals[index]));
        // The factory's own default IS the axis-aligned row; naming it here would bind this record's `Bounds`
        // member rather than the kernel type, which is the name capture the defaulted call sidesteps.
        return from bounds in Analyze.Run<MeshSpace, BoundingBox>(AnalysisQuery.Bounds(), space)
                   .ToFin()
                   .Bind(boxes => boxes.Head.ToFin(Manufacturability.MeshBounds))
               from index in Spatial
                   .Apply(new SpatialOp.Build(SpatialKind.Bvh, FaceBoxes(native), BuildPolicy.Canonical),
                       Manufacturability.DfmOp)
                   .Bind(static answer => answer is SpatialAnswer.Index built
                       ? Fin.Succ(built.Value)
                       : Fin.Fail<SpatialIndex>(Manufacturability.MeshIndex))
               select new MeshFacts(native, faces, index, bounds);
    }

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
}
```

```mermaid
---
config:
  layout: elk
  flowchart:
    curve: linear
    padding: 25
---
flowchart LR
    accTitle: Manufacturability assessment fold
    accDescr: One admitted request builds a single mesh scratch, derives geometric, package, tolerance, and procedure observations each carrying its own resolution, evaluates parameterized rules into verdicts, ranks routes on weighted dimensionless burdens, and composes the tolerance chain receipt into one stackup precheck.
    Request["DfmRequest — component, policy, supplied evidence"] --> Admit["Manufacturability.Admit — accumulated gates"]
    Admit --> Facts["MeshFacts.Of — one native copy, normals, index, bounds"]
    Facts --> Derived["Derived — policy, profile, wall, removal, forming, joining, additive"]
    Package["DfmPackageEvidence — sidecar receipts with their own step"] --> Evidence
    Derived --> Evidence["DfmObservation set — route + resolution per reading"]
    Tolerance["Tolerance.Apply(Effective) + Capability.Achievable"] --> Evidence
    Procedure["ProcedureReceipt.Qualified"] --> Evidence
    Evidence --> Verdicts["Evaluate — criterion, confidence, outcome"]
    Verdicts --> Rows["Route — blockers, requirements, RouteScore"]
    Chain["ToleranceChain.Evaluate"] --> Stackup["StackupPrecheck — allowance census + chain verdict"]
    Stackup --> Rows
    Rows --> Report["DfmReport — routing, verdicts, observations"]
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
