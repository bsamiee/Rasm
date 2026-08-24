# [RASM_FABRICATION_WORKHOLDING]

`Workholding` owns fixture admission, datum establishment, support, clamping, stage lifecycle, tool access, keep-out conditioning, and restraint evidence. Aggregate admission proves the locating scheme, contact laws, actuation order, stock compatibility, and operation windows once; every interior operation consumes the admitted `Fixture`.

`WorkholdingKind` is the ONE holding-mechanism row table: role, holding class, cardinalities, the `CapabilitySet<FixtureDemand>` a form must match exactly, keep-out source, preload rule, and required metric roster are COLUMNS, so a locating pin, a soft-jaw vise, and a vacuum bed are one `FixtureElement` shape apart and every fold over elements has one body. `ExclusionZone`, `Fixture`, `FixtureSet`, and `RestraintProof` remain the in-process seam vocabulary; `Workholding.Apply` admits every operation through one `WorkholdingOp` family and emits one `WorkholdingResult` family. Planar containment, open-path clipping, and region union ride `Geometry2D/algebra` `PolygonOp`; the restraint distribution rides the kernel `Rasm.Numerics` rectangular sparse route; every preimage frames and closes at `Process/owner#RUN_DISPATCH` `FabricationCanon` over the one `Rasm.Element` `CanonicalWriter`, so every projected fixture artifact addresses through the keyed close and its refusal rides the caller's rail. `FixturingWitness` is this folder's evidence vocabulary for the `Process/faults` offset-54 `FixtureInadmissible` case, which is declared there and never re-declared here.

## [01]-[INDEX]

- [02]-[ELEMENTS]: `WorkholdingKind` and its column families, `Actuation`, `ContactLaw`, `ContactPatch`, `FixtureElement`, and the clamp-template synthesis vocabulary.
- [03]-[FIXTURE]: aggregate admission, the stage lifecycle graph, keep-out zones, datum evidence, and constraint closure.
- [04]-[EVALUATION]: conditioning, corridor clearance, the capacity-normalized restraint solve, synthesis ranking, projection, and receipt folds.

## [02]-[ELEMENTS]

- Owner: `WorkholdingKind` owns every holding mechanism as one row carrying its role, holding class, cardinalities, keep-out source, preload rule, `CapabilitySet<FixtureDemand>` demands, and required metrics; `ContactLaw` owns friction, pressure, stiffness, deflection, and pull-off invariants; `Actuation` carries energy source and transmission geometry per case with its `EnergyCustody` row on the ROOT.
- Provenance: `WorkholdingKind` and `FixtureMetric` are this folder's OWN design vocabularies, not transcriptions — no standard publishes a closed holding-mechanism roster or a closed fixture-scalar roster, and neither table cites one. Each row therefore earns its place by being reachable through admission and read by a fold on this page; a row nothing admits and nothing reads is a fabrication, and the growth law below is what a shop adds against.
- Cases: locating rows cover plane, round pin, diamond pin, nest, center, mandrel, and optical alignment; support rows cover fixed, adjustable, hydraulic, compliant, steady-rest, and sacrificial contact; clamping rows cover toe, vise, chuck, collet, expanding arbor, vacuum, magnetic, adhesive, freeze, center, tailstock, and bed mechanisms.
- Law: a mechanism differs from its siblings in COLUMN VALUES alone, so `FixtureElement` is one admitted owner rather than a case family — the four parallel folds a case family forced (geometry, contacts, validity, preimage) collapse onto one body each, and a mechanism-shaped `switch` anywhere below is the deleted form.
- Law: `ElementForm.Metrics` is the one scalar carrier — a metric is a ROW, so a new dimension needs no constructor slot and no validation clause, and `FixtureMetric.Bound` decides admissibility for every metric under one fold. `KeepoutSource` and `PreloadRule` carry their behaviour as delegate columns, so the zone body set and the contact preload are expressions, never dispatch.
- Entry: `FixtureElement.Admit` is the sole construction; it resolves the mechanism's keep-out bodies, distributes preload under the row's rule, and proves cardinality, the demand correspondence, metric roster, and contact validity in one accumulating gate.
- Law: what a mechanism DEMANDS of its form is one `CapabilitySet<FixtureDemand>` column and the gate is set EQUALITY — a drive handed to a mechanism that never actuates is as inadmissible as a missing one — so it reads as the kernel's `Require` door taken in BOTH directions, each arm's refusal RECEIVING its missing set from the door rather than deriving a complement here, and the two accumulate. A third demand is one roster row and the entries that hold it.
- Law: `EnergyCustody` is the ONE loss-of-energy answer and it seats on the `Actuation` ROOT, so a new drive case cannot arrive without stating what holds the workpiece when its source fails. Four per-case booleans spelling one question forced a six-arm switch to re-ask it and hard-coded the two unconditional answers in code rather than in data.
- Growth: a new element mechanism is one `WorkholdingKind` row and, where its scalar is new, one `FixtureMetric` row; consumers change nowhere.
- Boundary: template cases survive beside realized elements because their payload arrives before geometry realization and aggregate admission; provider geometry never reaches this cluster.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------------------------------------------------------
using LanguageExt;
using LanguageExt.Common;
using LanguageExt.Traits;
using QuikGraph;
using QuikGraph.Algorithms;
using Rasm.Domain;
using Rasm.Element.Projection;
using Rasm.Fabrication.Geometry2D;
using Rasm.Fabrication.Joining;
using Rasm.Fabrication.Process;
using Rasm.Numerics;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics.Tensors;
using Thinktecture;
using UnitsNet;
using UnitsNet.Units;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Fixturing;

// --- [TYPES] --------------------------------------------------------------------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class FixtureRole {
    public static readonly FixtureRole Locate = new("locate");
    public static readonly FixtureRole Support = new("support");
    public static readonly FixtureRole Clamp = new("clamp");
}

[SmartEnum<string>]
public sealed partial class FixtureState {
    public static readonly FixtureState Load = new("load", cutting: false);
    public static readonly FixtureState Locate = new("locate", cutting: false);
    public static readonly FixtureState Clamp = new("clamp", cutting: false);
    public static readonly FixtureState Cut = new("cut", cutting: true);
    public static readonly FixtureState Index = new("index", cutting: false);
    public static readonly FixtureState Probe = new("probe", cutting: false);
    public static readonly FixtureState Unload = new("unload", cutting: false);

    public bool Cutting { get; }
}

// A bound is the admissibility of ONE scalar axis, so every metric on every mechanism proves itself through
// the same fold and a per-column predicate ladder has nothing left to say.
[SmartEnum<string>]
public sealed partial class MetricBound {
    public static readonly MetricBound Positive = new("positive", static value => double.IsFinite(value) && value > 0.0);
    public static readonly MetricBound Nonnegative = new("nonnegative", static value => double.IsFinite(value) && value >= 0.0);
    public static readonly MetricBound Fraction = new("fraction", static value => double.IsFinite(value) && value is > 0.0 and <= 1.0);
    public static readonly MetricBound Finite = new("finite", double.IsFinite);
    public static readonly MetricBound Flag = new("flag", static value => value is 0.0 or 1.0);
    public static readonly MetricBound IncludedAngle = new("included-angle", static value => double.IsFinite(value) && value is > 0.0 and < 180.0);

    public Func<double, bool> Admits { get; }
}

// Millimetre, newton, pascal, decimal-fraction, and degree readings under one keyed carrier; the unit basis is
// the row's own and a consumer reads it through the named projection rather than a constructor position.
[SmartEnum<string>]
public sealed partial class FixtureMetric {
    public static readonly FixtureMetric Radius = new("radius", MetricBound.Positive);
    public static readonly FixtureMetric Height = new("height", MetricBound.Positive);
    public static readonly FixtureMetric Margin = new("margin", MetricBound.Nonnegative);
    public static readonly FixtureMetric Travel = new("travel", MetricBound.Positive);
    public static readonly FixtureMetric Station = new("station", MetricBound.Nonnegative);
    public static readonly FixtureMetric Opening = new("opening", MetricBound.Positive);
    public static readonly FixtureMetric Span = new("span", MetricBound.Positive);
    public static readonly FixtureMetric AxialCapacity = new("axial-capacity", MetricBound.Positive);
    public static readonly FixtureMetric Collapse = new("collapse", MetricBound.Positive);
    public static readonly FixtureMetric Expansion = new("expansion", MetricBound.Positive);
    public static readonly FixtureMetric Pressure = new("pressure", MetricBound.Positive);
    public static readonly FixtureMetric Coupling = new("coupling", MetricBound.Fraction);
    public static readonly FixtureMetric Cure = new("cure", MetricBound.Fraction);
    public static readonly FixtureMetric Frozen = new("frozen", MetricBound.Fraction);
    public static readonly FixtureMetric IncludedAngle = new("included-angle", MetricBound.IncludedAngle);
    public static readonly FixtureMetric Repeatability = new("repeatability", MetricBound.Nonnegative);
    public static readonly FixtureMetric RemainingThickness = new("remaining-thickness", MetricBound.Positive);
    public static readonly FixtureMetric DeflectionLimit = new("deflection-limit", MetricBound.Positive);
    public static readonly FixtureMetric EqualizedPressure = new("equalized-pressure", MetricBound.Positive);

    public MetricBound Bound { get; }
}

// What a mechanism demands of the form it is admitted with. `Rank` stays the kernel's derived declaration order,
// which fixes the wire rendering an admission refusal prints; `Key` is the wire token. A demand is a ROW because the
// next one — a vacuum circuit, a thermal loop, a probe reference — lands as one entry and changes no signature.
[SmartEnum<string>]
public sealed partial class FixtureDemand : ICapability<FixtureDemand> {
    public static readonly FixtureDemand Actuator = new("actuator");
    public static readonly FixtureDemand Anchor = new("anchor");
}

// The zone body set is the mechanism's own loops, its contact footprints, or nothing at all — optical alignment
// occupies no space. The row carries the projection, so zone construction reads one expression.
[SmartEnum<string>]
public sealed partial class KeepoutSource {
    public static readonly KeepoutSource Bodies = new("bodies", static form => form.Bodies);
    public static readonly KeepoutSource Footprints = new("footprints",
        static form => form.Contacts.Map(static contact => contact.Footprint));
    public static readonly KeepoutSource Absent = new("absent", static _ => Seq<Loop>());

    public Func<ElementForm, Seq<Loop>> Loops { get; }
}

// Preload custody as data: an admitted contact keeps the preload it arrived with, a driven mechanism takes its
// actuator's whole or divided force, and a field mechanism derives it from pressure over area or pull-off scaled
// by its own coupling fraction. `Axis` names which metric drives the field rules and is absent otherwise.
[SmartEnum<string>]
public sealed partial class PreloadRule {
    public static readonly PreloadRule Admitted = new("admitted", static seat => seat.Patch.Preload);
    public static readonly PreloadRule Whole = new("whole", static seat => seat.Drive.Preload);
    public static readonly PreloadRule Split = new("split", static seat => seat.Drive.Preload / seat.Contacts);
    public static readonly PreloadRule Pressure = new("pressure",
        static seat => UnitsNet.Pressure.FromPascals(seat.Axis) * Area.FromSquareMillimeters(Math.Abs(seat.Patch.Footprint.Area())));
    public static readonly PreloadRule Coupled = new("coupled", static seat => seat.Patch.Law.PullOff * seat.Axis);

    public Func<PreloadSeat, Force> Preload { get; }
}

// One row per holding mechanism. Every fold below reads these columns, so a new mechanism is one row and no
// consumer, admission clause, projection, or preimage changes.
[SmartEnum<string>]
public sealed partial class WorkholdingKind {
    public static readonly WorkholdingKind LocatingPlane = Of("locating-plane", FixtureRole.Locate, HoldingClass.Mechanical,
        KeepoutSource.Footprints, PreloadRule.Admitted, contacts: 1);
    public static readonly WorkholdingKind RoundPin = Of("round-pin", FixtureRole.Locate, HoldingClass.Mechanical,
        KeepoutSource.Footprints, PreloadRule.Admitted, contacts: 1, demands: [FixtureDemand.Anchor],
        metrics: FixtureMetric.Radius, FixtureMetric.Height);
    public static readonly WorkholdingKind DiamondPin = Of("diamond-pin", FixtureRole.Locate, HoldingClass.Mechanical,
        KeepoutSource.Footprints, PreloadRule.Admitted, contacts: 1, demands: [FixtureDemand.Anchor],
        metrics: FixtureMetric.Radius, FixtureMetric.Height);
    public static readonly WorkholdingKind Nest = Of("nest", FixtureRole.Locate, HoldingClass.Mechanical,
        KeepoutSource.Footprints, PreloadRule.Admitted, contacts: 2);
    public static readonly WorkholdingKind LocatingCenter = Of("locating-center", FixtureRole.Locate, HoldingClass.Revolved,
        KeepoutSource.Footprints, PreloadRule.Admitted, contacts: 1, demands: [FixtureDemand.Anchor],
        metrics: FixtureMetric.IncludedAngle);
    public static readonly WorkholdingKind Mandrel = Of("mandrel", FixtureRole.Locate, HoldingClass.Revolved,
        KeepoutSource.Footprints, PreloadRule.Admitted, contacts: 1, demands: [FixtureDemand.Anchor],
        metrics: FixtureMetric.Radius, FixtureMetric.Span);
    public static readonly WorkholdingKind Optical = Of("optical", FixtureRole.Locate, HoldingClass.Mechanical,
        KeepoutSource.Absent, PreloadRule.Admitted, contacts: 0, demands: [FixtureDemand.Anchor],
        metrics: FixtureMetric.Repeatability);
    public static readonly WorkholdingKind FixedSupport = Of("fixed-support", FixtureRole.Support, HoldingClass.Mechanical,
        KeepoutSource.Footprints, PreloadRule.Admitted, contacts: 1);
    public static readonly WorkholdingKind AdjustableSupport = Of("adjustable-support", FixtureRole.Support, HoldingClass.Mechanical,
        KeepoutSource.Footprints, PreloadRule.Admitted, contacts: 1, metrics: FixtureMetric.Travel);
    public static readonly WorkholdingKind HydraulicSupport = Of("hydraulic-support", FixtureRole.Support, HoldingClass.Mechanical,
        KeepoutSource.Footprints, PreloadRule.Admitted, contacts: 1, metrics: FixtureMetric.EqualizedPressure);
    public static readonly WorkholdingKind CompliantSupport = Of("compliant-support", FixtureRole.Support, HoldingClass.Mechanical,
        KeepoutSource.Footprints, PreloadRule.Admitted, contacts: 1, metrics: FixtureMetric.DeflectionLimit);
    public static readonly WorkholdingKind SteadyRest = Of("steady-rest", FixtureRole.Support, HoldingClass.Revolved,
        KeepoutSource.Footprints, PreloadRule.Admitted, contacts: 2, metrics: FixtureMetric.Station);
    public static readonly WorkholdingKind SacrificialSupport = Of("sacrificial-support", FixtureRole.Support, HoldingClass.Bed,
        KeepoutSource.Footprints, PreloadRule.Admitted, contacts: 1, metrics: FixtureMetric.RemainingThickness);
    public static readonly WorkholdingKind Toe = Of("toe", FixtureRole.Clamp, HoldingClass.Mechanical,
        KeepoutSource.Bodies, PreloadRule.Whole, contacts: 1, bodies: 1, bodyCeiling: 1, demands: [FixtureDemand.Actuator],
        metrics: FixtureMetric.Margin, FixtureMetric.Height);
    public static readonly WorkholdingKind Vise = Of("vise", FixtureRole.Clamp, HoldingClass.Mechanical,
        KeepoutSource.Bodies, PreloadRule.Split, contacts: 2, bodies: 2, bodyCeiling: 2, demands: [FixtureDemand.Actuator],
        metrics: FixtureMetric.Opening, FixtureMetric.Margin, FixtureMetric.Height);
    public static readonly WorkholdingKind Chuck = Of("chuck", FixtureRole.Clamp, HoldingClass.Revolved,
        KeepoutSource.Bodies, PreloadRule.Split, contacts: 3, bodies: 3, demands: [FixtureDemand.Actuator],
        metrics: FixtureMetric.AxialCapacity, FixtureMetric.Margin, FixtureMetric.Height);
    public static readonly WorkholdingKind Collet = Of("collet", FixtureRole.Clamp, HoldingClass.Revolved,
        KeepoutSource.Bodies, PreloadRule.Split, contacts: 1, bodies: 1, bodyCeiling: 1, demands: [FixtureDemand.Actuator],
        metrics: FixtureMetric.Collapse, FixtureMetric.Margin, FixtureMetric.Height);
    public static readonly WorkholdingKind Arbor = Of("arbor", FixtureRole.Clamp, HoldingClass.Revolved,
        KeepoutSource.Bodies, PreloadRule.Split, contacts: 1, bodies: 1, bodyCeiling: 1, demands: [FixtureDemand.Actuator],
        metrics: FixtureMetric.Expansion, FixtureMetric.Margin, FixtureMetric.Height);
    public static readonly WorkholdingKind Vacuum = Of("vacuum", FixtureRole.Clamp, HoldingClass.Vacuum,
        KeepoutSource.Bodies, PreloadRule.Pressure, contacts: 1, bodies: 1, axis: FixtureMetric.Pressure,
        metrics: FixtureMetric.Pressure, FixtureMetric.Margin, FixtureMetric.Height);
    public static readonly WorkholdingKind Magnetic = Of("magnetic", FixtureRole.Clamp, HoldingClass.Magnetic,
        KeepoutSource.Bodies, PreloadRule.Coupled, contacts: 1, bodies: 1, bodyCeiling: 1, axis: FixtureMetric.Coupling,
        metrics: FixtureMetric.Coupling, FixtureMetric.Margin, FixtureMetric.Height);
    public static readonly WorkholdingKind Adhesive = Of("adhesive", FixtureRole.Clamp, HoldingClass.Bed,
        KeepoutSource.Bodies, PreloadRule.Coupled, contacts: 1, bodies: 1, bodyCeiling: 1, axis: FixtureMetric.Cure,
        metrics: FixtureMetric.Cure, FixtureMetric.Margin, FixtureMetric.Height);
    public static readonly WorkholdingKind Freeze = Of("freeze", FixtureRole.Clamp, HoldingClass.Bed,
        KeepoutSource.Bodies, PreloadRule.Coupled, contacts: 1, bodies: 1, bodyCeiling: 1, axis: FixtureMetric.Frozen,
        metrics: FixtureMetric.Frozen, FixtureMetric.Margin, FixtureMetric.Height);
    public static readonly WorkholdingKind ClampingCenter = Of("clamping-center", FixtureRole.Clamp, HoldingClass.Revolved,
        KeepoutSource.Bodies, PreloadRule.Whole, contacts: 1, bodies: 1, bodyCeiling: 1, demands: [FixtureDemand.Actuator],
        metrics: FixtureMetric.Margin, FixtureMetric.Height);
    public static readonly WorkholdingKind Tailstock = Of("tailstock", FixtureRole.Clamp, HoldingClass.Revolved,
        KeepoutSource.Bodies, PreloadRule.Whole, contacts: 1, bodies: 1, bodyCeiling: 1, demands: [FixtureDemand.Actuator],
        metrics: FixtureMetric.Margin, FixtureMetric.Height);
    public static readonly WorkholdingKind Bed = Of("bed", FixtureRole.Clamp, HoldingClass.Bed,
        KeepoutSource.Bodies, PreloadRule.Pressure, contacts: 1, bodies: 1, bodyCeiling: 1, axis: FixtureMetric.Pressure,
        metrics: FixtureMetric.Pressure, FixtureMetric.Height);

    public FixtureRole Role { get; }
    public HoldingClass Holding { get; }
    public KeepoutSource Keepout { get; }
    public PreloadRule Rule { get; }

    // Cardinality floors bind the mechanism's physics: two opposed jaws, three chuck jaws, one nest with at
    // least two seating contacts. A ceiling is present only where the mechanism forbids a wider set.
    public int ContactFloor { get; }
    public int BodyFloor { get; }
    public Option<int> BodyCeiling { get; }

    // What the mechanism DEMANDS of the form handed to it, as the kernel capability column rather than a pair of
    // positional booleans. The demand is EXACT: a drive supplied to a mechanism that never actuates is as
    // inadmissible as a missing one, so admission compares sets and names both shortfalls off the same column.
    public CapabilitySet<FixtureDemand> Demands { get; }
    public Option<FixtureMetric> Axis { get; }
    public Set<FixtureMetric> Metrics { get; }

    private static WorkholdingKind Of(
        string key,
        FixtureRole role,
        HoldingClass holding,
        KeepoutSource keepout,
        PreloadRule rule,
        int contacts,
        int bodies = 0,
        int? bodyCeiling = null,
        FixtureDemand[]? demands = null,
        FixtureMetric? axis = null,
        params FixtureMetric[] metrics) =>
        new(key, role, holding, keepout, rule, contacts, bodies, Optional(bodyCeiling),
            CapabilitySet<FixtureDemand>.Of(demands ?? []), Optional(axis), toSet(metrics));
}

// What holds the workpiece when the drive's energy source fails, as a ROW naming the MECHANISM a shop reads off a
// setup sheet. `Retains` is the one column the cutting-custody gate reads, and four per-case booleans spelling
// that one question — self-locking, clamps-on-loss, accumulator-held, brake-held — forced a six-arm switch to
// re-ask it and left the two cases that answer unconditionally saying so in code rather than in data.
[SmartEnum<string>]
public sealed partial class EnergyCustody {
    public static readonly EnergyCustody SelfLocking = new("self-locking", retains: true);
    public static readonly EnergyCustody StoredEnergy = new("stored-energy", retains: true);
    public static readonly EnergyCustody CheckValve = new("check-valve", retains: true);
    public static readonly EnergyCustody Accumulator = new("accumulator", retains: true);
    public static readonly EnergyCustody Brake = new("brake", retains: true);
    public static readonly EnergyCustody Backdriving = new("backdriving", retains: false);
    public static readonly EnergyCustody FieldDecay = new("field-decay", retains: false);

    public bool Retains { get; }
}

// The custody column seats on the ROOT because every drive answers it, so a new drive case cannot arrive without
// stating what it holds on loss of energy, and no arm re-derives the answer.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Actuation(EnergyCustody Custody) {
    public sealed record Manual(Torque Torque, Length MeanRadius, Ratio Efficiency, EnergyCustody custody)
        : Actuation(custody);
    public sealed record Spring(Force Force, Length Stroke, EnergyCustody custody) : Actuation(custody);
    public sealed record Pneumatic(Pressure Pressure, Area Piston, EnergyCustody custody) : Actuation(custody);
    public sealed record Hydraulic(Pressure Pressure, Area Piston, EnergyCustody custody) : Actuation(custody);
    public sealed record Electric(Force Force, Length Stroke, EnergyCustody custody) : Actuation(custody);
    public sealed record Field(Force PullOff, Duration Release, EnergyCustody custody) : Actuation(custody);

    public bool HeldOnLoss => Custody.Retains;

    public Force Preload => new(Switch(
        manual: static row => row.Torque.As(TorqueUnit.NewtonMeter) / row.MeanRadius.As(LengthUnit.Meter)
            * row.Efficiency.As(RatioUnit.DecimalFraction),
        spring: static row => row.Force.As(ForceUnit.Newton),
        pneumatic: static row => row.Pressure.As(PressureUnit.Pascal) * row.Piston.As(AreaUnit.SquareMeter),
        hydraulic: static row => row.Pressure.As(PressureUnit.Pascal) * row.Piston.As(AreaUnit.SquareMeter),
        electric: static row => row.Force.As(ForceUnit.Newton),
        field: static row => row.PullOff.As(ForceUnit.Newton)), ForceUnit.Newton);

    // The corpus validity floor: every claim states its own requirement and the fold reports the conjunction, so
    // a new drive column is one claim row rather than a hand-chained comparison ladder.
    public bool IsValid => Switch(
        manual: static row => ValidityClaim.All(
            Fixtures.Positive(row.Torque), Fixtures.Positive(row.MeanRadius), Fixtures.Fraction(row.Efficiency)),
        spring: static row => ValidityClaim.All(Fixtures.Positive(row.Force), Fixtures.Positive(row.Stroke)),
        pneumatic: static row => ValidityClaim.All(Fixtures.Positive(row.Pressure), Fixtures.Positive(row.Piston)),
        hydraulic: static row => ValidityClaim.All(Fixtures.Positive(row.Pressure), Fixtures.Positive(row.Piston)),
        electric: static row => ValidityClaim.All(Fixtures.Positive(row.Force), Fixtures.Positive(row.Stroke)),
        field: static row => ValidityClaim.All(Fixtures.Positive(row.PullOff), Fixtures.Nonnegative(row.Release)));

    // ONE-TIME RE-KEY: the custody row frames ONCE at the root where four per-case presence bits stood, so the
    // layout gets shorter and every drive addresses under a named mechanism rather than a positional flag.
    public CanonicalWriter CanonicalBytes(CanonicalWriter writer) => Switch(
        state: writer.Discriminant(Custody),
        manual: static (held, row) => held.String(nameof(Manual)).Double(row.Torque.As(TorqueUnit.NewtonMeter))
            .Double(row.MeanRadius.As(LengthUnit.Millimeter)).Double(row.Efficiency.As(RatioUnit.DecimalFraction)),
        spring: static (held, row) => held.String(nameof(Spring)).Double(row.Force.As(ForceUnit.Newton))
            .Double(row.Stroke.As(LengthUnit.Millimeter)),
        pneumatic: static (held, row) => held.String(nameof(Pneumatic)).Double(row.Pressure.As(PressureUnit.Pascal))
            .Double(row.Piston.As(AreaUnit.SquareMeter)),
        hydraulic: static (held, row) => held.String(nameof(Hydraulic)).Double(row.Pressure.As(PressureUnit.Pascal))
            .Double(row.Piston.As(AreaUnit.SquareMeter)),
        electric: static (held, row) => held.String(nameof(Electric)).Double(row.Force.As(ForceUnit.Newton))
            .Double(row.Stroke.As(LengthUnit.Millimeter)),
        field: static (held, row) => held.String(nameof(Field)).Double(row.PullOff.As(ForceUnit.Newton))
            .Double(row.Release.As(DurationUnit.Second)));
}

// --- [MODELS] -------------------------------------------------------------------------------------------------------------------------------------
[ComplexValueObject]
public readonly partial struct ContactLaw {
    public Ratio Friction { get; }
    public Pressure PressureLimit { get; }
    public double NormalStiffnessNPerMm { get; }
    public double TangentialStiffnessNPerMm { get; }
    public Length DeflectionLimit { get; }
    public Force PullOff { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Ratio friction,
        ref Pressure pressureLimit,
        ref double normalStiffnessNPerMm,
        ref double tangentialStiffnessNPerMm,
        ref Length deflectionLimit,
        ref Force pullOff) {
        if (!(ValidityClaim.All(
            Fixtures.Nonnegative(friction), Fixtures.Positive(pressureLimit), ValidityClaim.Positive(normalStiffnessNPerMm),
            ValidityClaim.Positive(tangentialStiffnessNPerMm), Fixtures.Positive(deflectionLimit), Fixtures.Nonnegative(pullOff))))
            validationError = new ValidationError("contact-law");
    }

    public static Fin<ContactLaw> Admit(
        Ratio friction,
        Pressure pressureLimit,
        double normalStiffnessNPerMm,
        double tangentialStiffnessNPerMm,
        Length deflectionLimit,
        Force pullOff) =>
        Validate(friction, pressureLimit, normalStiffnessNPerMm, tangentialStiffnessNPerMm, deflectionLimit, pullOff,
            out ContactLaw law).Admitted(law);

    public CanonicalWriter CanonicalBytes(CanonicalWriter writer) =>
        writer.Double(Friction.As(RatioUnit.DecimalFraction))
            .Double(PressureLimit.As(PressureUnit.Pascal))
            .Double(NormalStiffnessNPerMm)
            .Double(TangentialStiffnessNPerMm)
            .Double(DeflectionLimit.As(LengthUnit.Millimeter))
            .Double(PullOff.As(ForceUnit.Newton));
}

public readonly record struct ContactReaction(
    int Element,
    Point3d At,
    Vector3d Normal,
    Force NormalCapacity,
    Force TangentialCapacity,
    Force PullOffCapacity,
    double AreaWeight) {
    public CanonicalWriter CanonicalBytes(CanonicalWriter writer) => writer
        .Ordinal(Element).Coords(At).Coords(Normal)
        .Double(NormalCapacity.As(ForceUnit.Newton))
        .Double(TangentialCapacity.As(ForceUnit.Newton))
        .Double(PullOffCapacity.As(ForceUnit.Newton))
        .Double(AreaWeight);
}

// The seat a preload rule reads: the patch it seats on, how many patches share the mechanism's actuator, the
// drive itself where the mechanism has one, and the mechanism's own driving metric where a field rule needs it.
public readonly record struct PreloadSeat(ContactPatch Patch, int Contacts, Actuation Drive, double Axis);

public sealed record ContactPatch(
    int Element,
    Loop Footprint,
    Point3d Center,
    Vector3d Normal,
    ContactLaw Law,
    Force Preload) : IValidityEvidence {
    // Geometry alone: the preload arrives from the mechanism's own `PreloadRule` at element admission, so a
    // patch handed to `FixtureElement.Admit` carries a placeholder and the seated value is gated there.
    public bool IsValid => ValidityClaim.All(
        Fixtures.Profile(Footprint), Footprint.Bulges.ForAll(static bulge => bulge == 0.0),
        Fixtures.Finite(Center), Fixtures.Unit(Normal));

    // Exemption: the tributary fold is a measured reaction kernel. Footprint is admitted already lowered, so
    // vertices are the true reaction stations and a bulged pad cannot degenerate to three; tributary edge length
    // is the weight, because uniform weighting gives a corner between two short edges the same reaction as a far one.
    public Seq<ContactReaction> Field {
        get {
            Seq<Point3d> ring = Footprint.Vertices.ToSeq();
            Seq<double> tributary = ring.Map((point, index) => 0.5 * (
                point.DistanceTo(ring[(index + ring.Count - 1) % ring.Count]) + point.DistanceTo(ring[(index + 1) % ring.Count])));
            double total = tributary.Fold(0.0, static (sum, value) => sum + value);
            double friction = Law.Friction.As(RatioUnit.DecimalFraction);
            return total <= EpsilonPolicy.ZeroTolerance
                ? Seq(new ContactReaction(Element, Center, Normal, Preload, Preload * friction, Law.PullOff, 1.0))
                : ring.Map((point, index) => new ContactReaction(
                    Element, point, Normal, Preload, Preload * friction, Law.PullOff, tributary[index] / total));
        }
    }

    public CanonicalWriter CanonicalBytes(CanonicalWriter writer) =>
        Law.CanonicalBytes(Footprint.CanonicalBytes(writer.Ordinal(Element)).Coords(Center).Coords(Normal))
            .Double(Preload.As(ForceUnit.Newton));
}

// Bodies, contacts, the optional locating anchor, and the keyed metric stream — the whole shape of every
// mechanism. `Anchor` carries a pin, mandrel, centre, or optical datum as ONE plane rather than a point and an
// axis spelled per mechanism: the plane's origin is the locating point, its normal the locating axis, and its
// X axis the free direction a diamond pin leaves unconstrained. `Bodies` arrives RESOLVED — a vacuum bed enters
// with its leak windows already subtracted, so no mechanism carries a second geometry pass past admission.
public sealed record ElementForm(
    Seq<Loop> Bodies,
    Seq<ContactPatch> Contacts,
    Option<Plane> Anchor,
    Map<FixtureMetric, double> Metrics) {
    public double Of(FixtureMetric axis) => Metrics.Find(axis).IfNone(0.0);

    public CanonicalWriter CanonicalBytes(CanonicalWriter writer) => writer
        .Rows(Bodies, static (held, loop) => loop.CanonicalBytes(held))
        .Rows(Contacts, static (held, contact) => contact.CanonicalBytes(held))
        .Maybe(Anchor, static (held, plane) => held.Coords(plane.Origin).Coords(plane.XAxis).Coords(plane.YAxis).Coords(plane.ZAxis))
        .Rows(toSeq(Metrics).OrderBy(static row => row.Key.Key, StringComparer.Ordinal).ToSeq(),
            static (held, row) => held.Discriminant(row.Key).Double(row.Value));
}

[ComplexValueObject]
public sealed partial class FixtureElement {
    public int Element { get; }
    public WorkholdingKind Kind { get; }
    public ElementForm Form { get; }
    public Option<Actuation> Actuator { get; }

    public FixtureRole Role => Kind.Role;
    public Seq<ContactPatch> Contacts => Form.Contacts;
    public Seq<Loop> Keepouts => Kind.Keepout.Loops(Form);
    public double MarginMm => Form.Of(FixtureMetric.Margin);
    public double HeightMm => Form.Of(FixtureMetric.Height);
    public bool HeldOnLoss => Actuator.Match(Some: static drive => drive.HeldOnLoss, None: static () => true);

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref int element,
        ref WorkholdingKind kind,
        ref ElementForm form,
        ref Option<Actuation> actuator) {
        if (element < 0)
            validationError = new ValidationError("fixture-element");
        else if (!form.Contacts.ForAll(static contact => Fixtures.Positive(contact.Preload)))
            validationError = new ValidationError("fixture-contacts");
    }

    // ONE admission for every mechanism: cardinality, the demand correspondence, metric roster, and contact
    // validity accumulate, then the row's own preload rule seats each contact so no caller distributes force.
    // The demand gate is ONE comparison over the capability column and its refusal names BOTH shortfalls off the
    // kernel's own `Missing` — required minus held in each direction — so no site hand-spells a complement.
    public static Fin<FixtureElement> Admit(int element, WorkholdingKind kind, ElementForm form, Option<Actuation> actuator) =>
        (Gate(element, kind, kind.Metrics.ForAll(axis => form.Metrics.Find(axis).Exists(axis.Bound.Admits)), nameof(FixtureMetric)),
         Gate(element, kind, form.Metrics.ForAll(static row => row.Key.Bound.Admits(row.Value)), nameof(ElementForm.Metrics)),
         Gate(element, kind, form.Contacts.Count >= kind.ContactFloor && form.Contacts.ForAll(contact =>
                contact is not null && contact.Element == element && contact.IsValid), nameof(ContactPatch)),
         Gate(element, kind, form.Bodies.Count >= kind.BodyFloor && kind.BodyCeiling.Map(form.Bodies.Count.Equals).IfNone(true)
                && form.Bodies.ForAll(Fixtures.Profile), nameof(ElementForm.Bodies)),
         Demanded(element, kind, form, actuator),
         Gate(element, kind, actuator.ForAll(static drive => drive.IsValid)
                && form.Anchor.ForAll(static plane => plane.IsValid), nameof(Actuation)))
            .Apply(static (_, _, _, _, _, _) => unit)
            .As()
            .ToFin()
            .Bind(_ => Validate(element, kind, Seated(kind, form, actuator), actuator, out FixtureElement seated).Admitted(seated));

    // The form supplies exactly what it carries; the mechanism demands exactly what it needs. Set EQUALITY is the
    // whole law, so this is the kernel `CapabilitySet` value comparison and NOT the `Require` door: `Require` is the
    // SUPERSET gate, and taking it in both directions split one verdict into two refusals a reader had to rejoin —
    // a form short one demand while carrying one nobody asked for is ONE inadmissible correspondence, not two. The
    // two `Missing` reads are the kernel's own evidence wires, so the single refusal still names both directions and
    // no site derives a complement; they run on the failing arm alone because a passing admission owes no rendering.
    private static K<Validation<Error>, Unit> Demanded(
        int element, WorkholdingKind kind, ElementForm form, Option<Actuation> actuator) {
        CapabilitySet<FixtureDemand> supplied = CapabilitySet<FixtureDemand>.Of(
            Seq(actuator.Map(static _ => FixtureDemand.Actuator), form.Anchor.Map(static _ => FixtureDemand.Anchor))
                .Somes().ToArray());
        // Both arms reach the CONCRETE carrier by user-defined implicit conversion, which C# cannot target at the
        // `K` interface this method publishes, so the local IS the lift rather than ceremony around one.
        Validation<Error, Unit> correspondence = supplied == kind.Demands ? unit : Refuse(element, kind, supplied);
        return correspondence;
    }

    private static Error Refuse(int element, WorkholdingKind kind, CapabilitySet<FixtureDemand> supplied) =>
        FabricationFault.Fixture(new FixturingWitness.Element(element, kind.Role,
            $"{nameof(FixtureDemand)}:unmet={supplied.Missing(kind.Demands).Wire}:unasked={kind.Demands.Missing(supplied).Wire}"));

    private static ElementForm Seated(WorkholdingKind kind, ElementForm form, Option<Actuation> actuator) {
        Actuation drive = actuator.IfNone(static () =>
            new Actuation.Spring(Force.Zero, Length.Zero, EnergyCustody.StoredEnergy));
        double axis = kind.Axis.Map(form.Of).IfNone(0.0);
        return form with {
            Contacts = form.Contacts.Map(contact =>
                contact with { Preload = kind.Rule.Preload(new PreloadSeat(contact, form.Contacts.Count, drive, axis)) }),
        };
    }

    private static K<Validation<Error>, Unit> Gate(int element, WorkholdingKind kind, bool holds, string axis) =>
        AdmissionSlots.Gate(holds, FabricationFault.Fixture(new FixturingWitness.Element(element, kind.Role, axis)));

    public CanonicalWriter CanonicalBytes(CanonicalWriter writer) =>
        Form.CanonicalBytes(writer.Ordinal(Element).Discriminant(Kind))
            .Maybe(Actuator, static (held, drive) => drive.CanonicalBytes(held));
}

// A template is pre-geometry: it carries the mechanism law a synthesis pass realizes against a part silhouette,
// so its payload arrives before contact geometry exists and cannot be a `FixtureElement`.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ClampTemplate {
    private ClampTemplate() { }

    public sealed record BoundaryToe(ContactLaw Law, Actuation Drive, Length Width, Length Depth, Length Height, Length Margin) : ClampTemplate;
    public sealed record OpposedVise(ContactLaw Law, Actuation Drive, Length JawDepth, Length JawWidth, Length Height, Length Margin) : ClampTemplate;
    public sealed record VacuumField(ContactLaw Law, Pressure Pressure, Length Height, Length Margin) : ClampTemplate;
    public sealed record SoftJaw(ContactLaw Law, Actuation Drive, Length BlankDepth, Length Clearance, Length Height, Length Margin) : ClampTemplate;

    internal Fin<(Seq<FixtureElement> Elements, Option<SoftJawInsert> Insert)> Generate(Loop part, int samples, int firstElement) =>
        Switch(
            state: (Part: part, Samples: samples, First: firstElement),
            boundaryToe: static (state, row) => Fixtures.Stations(state.Part, state.Samples)
                .Map((point, index) => Fixtures
                    .Box(point, row.Width.As(LengthUnit.Millimeter), row.Depth.As(LengthUnit.Millimeter), state.Part.Tolerance)
                    .Bind(body => FixtureElement.Admit(
                        state.First + index,
                        WorkholdingKind.Toe,
                        new ElementForm(
                            Seq(body),
                            Seq(new ContactPatch(state.First + index, body, body.Bound().Center, -Vector3d.ZAxis, row.Law, row.Drive.Preload)),
                            None,
                            Fixtures.Millimetres((FixtureMetric.Margin, row.Margin), (FixtureMetric.Height, row.Height))),
                        Some(row.Drive)))
                    .ToValidation())
                .Traverse(static row => row).As().ToFin().Map(static clamps => (clamps, Option<SoftJawInsert>.None)),
            opposedVise: static (state, row) => Jaws(state.Part, state.First, row)
                .Map(static clamp => (Seq(clamp), Option<SoftJawInsert>.None)),
            vacuumField: static (state, row) => FixtureElement.Admit(
                state.First,
                WorkholdingKind.Vacuum,
                new ElementForm(
                    Seq(state.Part),
                    Seq(new ContactPatch(state.First, state.Part, state.Part.Bound().Center, -Vector3d.ZAxis, row.Law, Force.Zero)),
                    None,
                    Fixtures.Millimetres((FixtureMetric.Margin, row.Margin), (FixtureMetric.Height, row.Height))
                        .Add(FixtureMetric.Pressure, row.Pressure.As(PressureUnit.Pascal))),
                None).Map(static element => (Seq(element), Option<SoftJawInsert>.None)),
            softJaw: static (state, row) => Fixtures.Offset(Seq(state.Part), row.Clearance.As(LengthUnit.Millimeter)).Bind(negative =>
                Jaws(state.Part, state.First, new OpposedVise(row.Law, row.Drive, row.BlankDepth, row.BlankDepth, row.Height, row.Margin))
                    .Map(clamp => (Seq(clamp), Some(new SoftJawInsert(clamp.Form.Bodies, negative, row.Clearance))))));

    private static Fin<FixtureElement> Jaws(Loop part, int element, OpposedVise row) {
        BoundingBox bound = part.Bound();
        double depth = row.JawDepth.As(LengthUnit.Millimeter);
        double width = row.JawWidth.As(LengthUnit.Millimeter);
        Point3d left = new(bound.Min.X - (0.5 * depth), bound.Center.Y, 0.0);
        Point3d right = new(bound.Max.X + (0.5 * depth), bound.Center.Y, 0.0);
        return (Fixtures.Box(left, depth, width, part.Tolerance).ToValidation(),
                Fixtures.Box(right, depth, width, part.Tolerance).ToValidation())
            .Apply((first, second) => new ElementForm(
                Seq(first, second),
                Seq(new ContactPatch(element, first, first.Bound().Center, Vector3d.XAxis, row.Law, row.Drive.Preload),
                    new ContactPatch(element, second, second.Bound().Center, -Vector3d.XAxis, row.Law, row.Drive.Preload)),
                None,
                Fixtures.Millimetres((FixtureMetric.Margin, row.Margin), (FixtureMetric.Height, row.Height))
                    .Add(FixtureMetric.Opening, bound.Diagonal.X)))
            .As()
            .ToFin()
            .Bind(form => FixtureElement.Admit(element, WorkholdingKind.Vise, form, Some(row.Drive)));
    }
}

public sealed record SoftJawInsert(Seq<Loop> Blanks, Seq<Loop> Negative, Length Clearance);

// The search shape: what to place, what it must survive, and the one budget triple that bounds enumeration.
public readonly record struct SynthesisBudget(int Samples, int MinimumTemplates, int MaximumTemplates, int CandidateBudget);

public sealed record FixtureSynthesis(
    FixtureSpec Basis,
    Loop Part,
    Seq<ClampTemplate> Templates,
    Seq<LoadCase> Loads,
    Seq<ToolCorridor> Corridors,
    SynthesisBudget Budget,
    Ratio SafetyFactor,
    FixtureObjective Objective);

[ComplexValueObject]
public readonly partial struct FixtureObjective {
    public double Holding { get; }
    public double Access { get; }
    public double Simplicity { get; }
    public double Total => Holding + Access + Simplicity;

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref double holding,
        ref double access,
        ref double simplicity) {
        if (!(ValidityClaim.All(
            double.IsFinite(holding), holding >= 0.0, double.IsFinite(access), access >= 0.0, double.IsFinite(simplicity), simplicity >= 0.0,
            ValidityClaim.Positive(holding + access + simplicity))))
            validationError = new ValidationError("fixture-objective");
    }
}

public readonly record struct FixtureScore(double Holding, double Access, double Simplicity, double Total);

public sealed record FixtureCandidate(
    Fixture Fixture,
    RestraintProof Holding,
    Seq<WorkholdingResult.Clearance> Clearance,
    Seq<SoftJawInsert> SoftJaws,
    FixtureScore Score);
```

## [03]-[FIXTURE]

- Owner: `FixtureSpec` is raw aggregate ingress; `Fixture` is the admitted owner carrying datum lineage, element identity, the stage lifecycle receipt, exact keep-outs, motion partition, and stock witness; `ExclusionZone` carries lower and upper height, active states, exact loops, source element, role, and mechanism.
- Law: the stage sequence is a `BidirectionalGraph<FixtureStage, SEdge<FixtureStage>>` gated `IsDirectedAcyclicGraph` before `SourceFirstBidirectionalTopologicalSort` orders it, and `TreeBreadthFirstSearch` from the opening stage proves custody coverage — a stage no earlier stage reaches is a dead step the index arithmetic it replaces admitted silently. `StageLifecycle` publishes the order and the reachable census as receipt COLUMNS, so `Transition` answers reachability off the built graph rather than re-scanning the sequence twice per query.
- Law: each `FixtureStep` activates and releases elements against one `FixtureState`; cutting states require settled location, support, and clamp custody, and a clamp whose actuator releases on loss of energy fails that custody.
- Law: `ConstraintCensus` preserves both closure ranks and redundancy — `Rank` over the friction-cone wrench set, `Frictionless` over normals alone — so underconstraint, overconstraint, and the form-versus-force closure distinction stay separable from holding-force sufficiency.
- Exemption: `ConstraintRank` is the bounded six-column contact-wrench kernel; its Gram-Schmidt span loop is the measured numeric exemption.
- Entry: `Fixture.Admit` is the sole construction; element, topology, lifecycle, datum, geometry, contact, and six-degree constraint failures accumulate before the `Fin<Fixture>` rail resumes.
- Receipt: `DatumFrame` records primary, secondary, and tertiary contact evidence with the work coordinate system transform and repeatability budget; `DatumTransfer` folds the `Joining/sequence` `DistortionField` into a per-setup datum budget, so a distortion the weld plane measured narrows the repeatability a later setup may claim instead of being re-estimated here.
- Boundary: `FixturingWitness` closes the admission rejection reasons and lowers through the `Process/faults` offset-54 `FabricationFault.FixtureInadmissible` case; degenerate geometry stays on `GeometryFault.DegenerateInput`.

```csharp signature
// --- [FIXTURE] ------------------------------------------------------------------------------------------------------------------------------------
[ValueObject<int>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public readonly partial struct FixtureStage {
    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) {
        if (value < 0)
            validationError = new ValidationError("fixture-stage");
    }
}

public readonly record struct FixtureStep(FixtureStage Stage, FixtureState State, Arr<int> Activate, Arr<int> Release, Duration Settle) {
    public CanonicalWriter CanonicalBytes(CanonicalWriter writer) => writer
        .Ordinal(Stage.Value).Discriminant(State)
        .Rows(Activate.ToSeq(), static (held, index) => held.Ordinal(index))
        .Rows(Release.ToSeq(), static (held, index) => held.Ordinal(index))
        .Double(Settle.As(DurationUnit.Second));
}

// The walk's own outputs as named columns: the topological order every fold consumes, the stages the opening
// stage reaches, and the element set live at each stage. The container never leaves the admission fold.
public sealed record StageLifecycle(Seq<FixtureStage> Order, Set<FixtureStage> Reachable, Map<FixtureStage, Set<int>> Active) {
    public Map<FixtureStage, int> Ordinal => Order.Fold(
        Map<FixtureStage, int>(), static (index, stage) => index.Add(stage, index.Count));

    public Option<Set<int>> At(FixtureStage stage) => Active.Find(stage);

    public bool Covers(FixtureStage from, FixtureStage to) =>
        Reachable.Contains(from) && Reachable.Contains(to)
        && (Ordinal.Find(from), Ordinal.Find(to)).Apply(static (start, end) => end >= start).As().IfNone(false);
}

public readonly record struct DatumFrame(
    Plane Work,
    Arr<int> Primary,
    Arr<int> Secondary,
    Arr<int> Tertiary,
    Length Repeatability) {
    public CanonicalWriter CanonicalBytes(CanonicalWriter writer) => writer
        .Coords(Work.Origin).Coords(Work.XAxis).Coords(Work.YAxis).Coords(Work.ZAxis)
        .Rows(Primary.ToSeq(), static (held, index) => held.Ordinal(index))
        .Rows(Secondary.ToSeq(), static (held, index) => held.Ordinal(index))
        .Rows(Tertiary.ToSeq(), static (held, index) => held.Ordinal(index))
        .Double(Repeatability.As(LengthUnit.Millimeter));
}

// The setup half of the ONE `Joining/sequence` displacement receipt: a member's measured displacement consumes
// the repeatability budget the datum frame promises, so a setup whose datum moved more than it can hold refuses
// on evidence the weld plane produced rather than on a second estimate minted here.
public sealed record DatumTransfer(Length Budget, Length Consumed, Seq<DistortionSource> Sources) {
    public Length Remaining => Budget - Consumed;
    public bool Holds => Consumed <= Budget;

    // A setup holds the WHOLE assembly, so its budget spends against every measured member; a joint holds its own
    // members, so the subset arity narrows to those. Both read the ONE receipt the weld plane produced.
    public static DatumTransfer Of(Length budget, DistortionField displacement) =>
        Of(budget, displacement, static _ => true);

    public static DatumTransfer Of(Length budget, DistortionField displacement, Set<AssemblyMemberKey> members) =>
        Of(budget, displacement, members.Contains);

    private static DatumTransfer Of(Length budget, DistortionField displacement, Func<AssemblyMemberKey, bool> member) {
        Seq<DisplacementRow> rows = displacement.Rows.Filter(row => member(row.Member));
        return new DatumTransfer(
            budget,
            Length.FromMillimeters(rows.Fold(0.0, static (worst, row) => Math.Max(worst, row.Displacement.Length))),
            rows.Choose(static row => row.Source));
    }
}

public sealed record ExclusionZone(
    int Operation,
    int Element,
    FixtureRole Role,
    WorkholdingKind Kind,
    Seq<Loop> Keepouts,
    Length Lower,
    Length Upper,
    Set<FixtureState> Active,
    Length ArcChordError) {
    public BoundingBox Bounds {
        get {
            BoundingBox plan = Keepouts.Fold(BoundingBox.Empty, static (box, loop) => { box.Union(loop.Bound()); return box; });
            return new BoundingBox(
                new Point3d(plan.Min.X, plan.Min.Y, Lower.As(LengthUnit.Millimeter)),
                new Point3d(plan.Max.X, plan.Max.Y, Upper.As(LengthUnit.Millimeter)));
        }
    }

    public Fin<CollisionZone> Collision =>
        Fixtures.ZoneIdentity(this).Bind(key => CollisionZone.Admit(key, Bounds));

    // The height band is the only test this record answers alone; planar membership and crossing belong to the
    // `Geometry2D` owner and run BATCHED over every segment of every zone at the call site.
    public Option<Edge3> Banded(Edge3 segment, FixtureState state) => !Active.Contains(state)
        ? None
        : Fixtures.Slab(segment, Lower.As(LengthUnit.Millimeter), Upper.As(LengthUnit.Millimeter));

    public CanonicalWriter CanonicalBytes(CanonicalWriter writer) => writer
        .Ordinal(Operation).Ordinal(Element).Discriminant(Role).Discriminant(Kind)
        .Rows(Keepouts, static (held, loop) => loop.CanonicalBytes(held))
        .Double(Lower.As(LengthUnit.Millimeter)).Double(Upper.As(LengthUnit.Millimeter))
        .Rows(toSeq(Active).OrderBy(static state => state.Key, StringComparer.Ordinal).ToSeq(),
            static (held, state) => held.Discriminant(state))
        .Double(ArcChordError.As(LengthUnit.Millimeter));
}

public readonly record struct MoveRun(int Loop, int Start, int Count);

public readonly record struct CorridorStation(Point3d Point, Length Cutter, Length Holder, Length Chip, Length Coolant);

[SmartEnum<string>]
public sealed partial class CorridorKind {
    public static readonly CorridorKind Tool = new("tool",
        static station => Math.Max(station.Cutter.As(LengthUnit.Millimeter), station.Holder.As(LengthUnit.Millimeter)));
    public static readonly CorridorKind Chip = new("chip", static station => station.Chip.As(LengthUnit.Millimeter));
    public static readonly CorridorKind Coolant = new("coolant", static station => station.Coolant.As(LengthUnit.Millimeter));

    public Func<CorridorStation, double> RadiusMm { get; }
}

public sealed record ToolCorridor(CorridorKind Kind, Seq<CorridorStation> Stations);

// --- [ERRORS] -------------------------------------------------------------------------------------------------------------------------------------
// The folder's own evidence vocabulary for the offset-54 band case `Process/faults` declares. The witness family
// homes here because its axes are fixturing's; the CASE homes there because the offset ledger is whole on one page.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FixturingWitness {
    private FixturingWitness() { }

    public sealed record Absent : FixturingWitness;
    public sealed record Aggregate(int Operation, int Elements, int Duplicates) : FixturingWitness;
    public sealed record Element(int Element, FixtureRole Role, string Axis) : FixturingWitness;
    public sealed record Lifecycle(Option<FixtureStep> Step, Option<FixtureRole> Uncovered, int Steps, int Unreachable) : FixturingWitness;
    public sealed record Datum(int Primary, int Secondary, int Tertiary, int Unlocated) : FixturingWitness;
    public sealed record Closure(int Rank, int Required, int Redundancy, int Frictionless) : FixturingWitness;
    public sealed record Restraint(int Loads, int Invalid, double Safety) : FixturingWitness;
    public sealed record Corridor(CorridorKind Kind, int Stations) : FixturingWitness;
    public sealed record Partition(int Runs, int Moves) : FixturingWitness;
    public sealed record Synthesis(int Templates, int Minimum, int Maximum, int Budget) : FixturingWitness;
    public sealed record Plan(int Operations, int Machines, int Fixtures, int MaxSetups) : FixturingWitness;
    public sealed record Operation(Option<int> Key, string Axis) : FixturingWitness;
    public sealed record Offsets(int Requested, int Available, int MaxSetups) : FixturingWitness;
    public sealed record Roster(Option<CarrierKey> Carrier, Option<int> Station, int Instances) : FixturingWitness;
    public sealed record Rebase(int Setup, Length Correction, Angle Rotation, Length Tolerance) : FixturingWitness;
    public sealed record Membership(Option<int> Joint, int Components, int Resolved) : FixturingWitness;
    public sealed record Join(int Joint, JoinRejection Reason) : FixturingWitness;
    public sealed record Residual(int Completed, int Blocked, int Joints) : FixturingWitness;
}

// `Visibility` names a MALFORMED sight census — a row addressing a corridor outside the joint's own roster — and
// `Sight` names a corridor demanding line of sight that an occlusion row blocks; two different faults, two rows.
[SmartEnum<string>]
public sealed partial class JoinRejection {
    public static readonly JoinRejection Fit = new("fit");
    public static readonly JoinRejection Stability = new("stability");
    public static readonly JoinRejection Components = new("components");
    public static readonly JoinRejection Custody = new("custody");
    public static readonly JoinRejection Robot = new("robot");
    public static readonly JoinRejection Visibility = new("visibility");
    public static readonly JoinRejection Sight = new("sight");
    public static readonly JoinRejection Access = new("access");
}

// --- [MODELS] -------------------------------------------------------------------------------------------------------------------------------------
public sealed record FixtureSpec(
    int Operation,
    Seq<FixtureElement> Elements,
    Seq<FixtureStep> Sequence,
    DatumFrame Datum,
    Arr<Loop> Profiles,
    Seq<MoveRun> Runs,
    Point3d InitialCursor,
    Option<StockSnapshot> Current,
    Length ArcChordError) {
    public CanonicalWriter CanonicalBytes(CanonicalWriter writer) => Datum
        .CanonicalBytes(writer.Ordinal(Operation)
            .Rows(Elements, static (held, element) => element.CanonicalBytes(held))
            .Rows(Sequence, static (held, step) => step.CanonicalBytes(held)))
        .Rows(Profiles.ToSeq(), static (held, loop) => loop.CanonicalBytes(held))
        .Rows(Runs, static (held, run) => held.Ordinal(run.Loop).Ordinal(run.Start).Ordinal(run.Count))
        .Coords(InitialCursor)
        .Maybe(Current, static (held, stock) => stock.Key.CanonicalBytes(held.Ordinal(stock.Setup))
            .Rows(stock.Machined.ToSeq(), static (row, loop) => loop.CanonicalBytes(row)))
        .Double(ArcChordError.As(LengthUnit.Millimeter));
}

public readonly record struct Wrench(Vector3d Force, Vector3d Moment) {
    public static Wrench Of(Point3d at, Vector3d direction) =>
        new(direction, Vector3d.CrossProduct(Fixtures.Meters(at - Point3d.Origin), direction));
}

public sealed record ConstraintCensus(int Rank, int Frictionless, int Redundancy, Seq<ContactReaction> Reactions) {
    public bool Constrained => Rank == 6;
    public bool FormClosed => Frictionless == 6;
    public bool Determinate => Constrained && Redundancy == 0;

    public CanonicalWriter CanonicalBytes(CanonicalWriter writer) =>
        writer.Ordinal(Rank).Ordinal(Frictionless).Ordinal(Redundancy)
            .Rows(Reactions, static (held, reaction) => reaction.CanonicalBytes(held));
}

[ComplexValueObject]
public sealed partial class Fixture {
    public FixtureSpec Spec { get; }
    public Seq<ExclusionZone> Zones { get; }
    public Seq<ContactPatch> Contacts { get; }
    public ConstraintCensus Constraint { get; }
    public StageLifecycle Lifecycle { get; }

    public int Operation => Spec.Operation;
    public Point3d InitialCursor => Spec.InitialCursor;

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref FixtureSpec spec,
        ref Seq<ExclusionZone> zones,
        ref Seq<ContactPatch> contacts,
        ref ConstraintCensus constraint,
        ref StageLifecycle lifecycle) {
        if (contacts.IsEmpty)
            validationError = new ValidationError("fixture");
        else if (!constraint.Constrained)
            validationError = new ValidationError("fixture-closure");
    }

    // Independent aggregate gates accumulate, the derived members build once, and the stock witness closes last
    // because it is the only gate needing the fully seated zone set.
    public static Fin<Fixture> Admit(FixtureSpec? candidate) =>
        Optional(candidate)
            .ToFin(FabricationFault.Fixture(new FixturingWitness.Absent()))
            .Bind(spec =>
                (Fixtures.GateSpec(spec), Fixtures.GateLifecycle(spec), Fixtures.GateDatum(spec))
                    .Apply(static (accepted, lifecycle, _) => (accepted, lifecycle))
                    .As()
                    .ToFin())
            .Bind(static row => Fixtures.Zones(row.accepted).Map(zones => (row.accepted, row.lifecycle, zones)))
            .Bind(static row => Fixtures
                .Constraint(row.accepted.Elements.Bind(static element => element.Contacts))
                .Bind(constraint => Validate(row.accepted, row.zones,
                    row.accepted.Elements.Bind(static element => element.Contacts), constraint, row.lifecycle,
                    out Fixture fixture).Admitted(fixture)))
            .Bind(static fixture => fixture.Spec.Current.Match(
                Some: stock => Fixtures.Machined(fixture, stock).Bind(hit => hit.Match(
                    Some: point => Fin.Fail<Fixture>(new FabricationFault.ClampOnMachinedFace(fixture.Operation, point)),
                    None: () => Fin.Succ(fixture))),
                None: () => Fin.Succ(fixture)));

    public CanonicalWriter CanonicalBytes(CanonicalWriter writer) =>
        Constraint.CanonicalBytes(Spec.CanonicalBytes(writer)
            .Rows(Zones, static (held, zone) => zone.CanonicalBytes(held))
            .Rows(Contacts, static (held, contact) => contact.CanonicalBytes(held)));
}

[ComplexValueObject]
public sealed partial class FixtureSet {
    public Seq<Fixture> Fixtures { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Seq<Fixture> fixtures) {
        if (fixtures.IsEmpty || fixtures.Map(static fixture => fixture.Operation).Distinct().Count != fixtures.Count)
            validationError = new ValidationError("fixture-set");
    }

    public static Fin<FixtureSet> Admit(Seq<Fixture> fixtures) =>
        Validate(fixtures, out FixtureSet set).Admitted(set);

    public HashMap<int, Fixture> ByOperation => Fixtures.Fold(
        HashMap<int, Fixture>(),
        static (index, fixture) => index.Add(fixture.Operation, fixture));
}
```

## [04]-[EVALUATION]

- Owner: `WorkholdingOp` closes admission, conditioning, clearance, machined-stock, restraint, transition, synthesis, and projection modalities; `WorkholdingResult` closes their typed outcomes; `Fixtures` owns every interior fold.
- Cases: `LoadCase` covers cutting, gravity, acceleration, probing, handling, thermal, and process-pressure demand; regional cases transfer their resultant through the region center, and each cutting operation matches the admitted fixture operation.
- Law: the restraint distribution is ONE rectangular minimum-norm solve. Six equilibrium rows meet one capacity-normalized column per contact axis, so the solution the kernel `SparseMatrix.SolveLeastSquaresDetailed` returns IS the least-work distribution proportional to capacity, its coefficients ARE dimensionless utilizations, and the admissible load factor is the reciprocal of the largest one in closed form. A projected Gauss-Seidel sweep inside a bisection on the load factor is the deleted form — it searched numerically for a scale this reads directly, and its per-contact projection needed three hand epsilons the normalization removes.
- Law: tipping resolves against the SUPPORT REGION — the union of contact footprints through `PolygonOp.Boolean` — not the convex hull of reaction stations and not the `PolygonOp.Calipers` minimum-area rectangle: that rectangle contains the region, so its edges sit further from the load and over-report the restoring lever, while the union boundary under-reports it wherever the region is concave and fails safe. `Calipers` stays the yield and remnant owner.
- Law: planar containment and path crossing are ONE question here — `PolygonOp.ClipOpen` batched once per zone over every segment of the whole path answers both, because a segment with an inside run is contained and a segment with two runs crossed — so a hand winding sum, a hand segment intersection, and the densified wall ring each zone carried are all deleted; the zone keeps its height band alone.
- Law: tool-corridor clearance uses every cutter and holder station; chip and coolant corridors use the same zone algebra with their own radius and active state. `Clearance` carries the blocking zone alone and derives clearness from its absence.
- Law: `ClampTemplate` generates boundary layouts, evaluates every candidate through the same restraint and corridor algebra, ranks the survivors, and derives soft-jaw negatives from the part silhouette. `Programs` enumerates only the admitted cardinalities under `SynthesisBudget.CandidateBudget`, so template-roster growth never costs a powerset.
- Exemption: statements stay inside the bounded kernels alone — `ConstraintRank` spans the six-column wrench basis, `Utilization` assembles the normalized triplet stream, `ArcSegments` chords one arc, and `Slab` clips one segment against one height band.
- Entry: `Workholding.Apply` is the sole public operation surface; each case carries every discriminant and parameter its arm consumes.
- Output: `FixtureProjection` selects machine, setup-sheet, inspection, and evidence payloads; `Keyed` dispatches on that family and frames through `FabricationCanon` over the one `CanonicalWriter`, closing on the retaining mint's own rail so a projection never addresses under bytes no writer held.
- Law: keep-out admission ACCUMULATES across elements — `Zones` traverses into `Validation`, so a spec with three degenerate offsets reports three, not the first. Inside one zone the band ordering needs no gate because `FixtureMetric.Height` carries `MetricBound.Positive` and the band reads a min and a max of the same vertex set, and activation coverage needs none because `GateLifecycle` already proves every element is activated by some step; a guard restating either is the vacuous form.
- Packages: `Rasm.Numerics` (`SparseMatrix.FromTriplets`, `SolveLeastSquaresDetailed`, `SolveReceipt`, `Dimension`, `EpsilonPolicy`), `Geometry2D/algebra` (`PolygonOp.ClipOpen`, `.Boolean`, `PolygonTrace.Regioned`/`.Runs`, `RegionTopology`), `QuikGraph` (`BidirectionalGraph`, `SEdge`, `IsDirectedAcyclicGraph`, `SourceFirstBidirectionalTopologicalSort`, `TreeBreadthFirstSearch`), `UnitsNet`, LanguageExt.Core.
- Boundary: geometry, aggregate, and stability failures remain typed; no failure becomes an empty fixture, a clear path, or a passing margin.

```csharp signature
// --- [EVALUATION] ---------------------------------------------------------------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class FixtureProjection {
    public static readonly FixtureProjection Machine = new("machine");
    public static readonly FixtureProjection SetupSheet = new("setup-sheet");
    public static readonly FixtureProjection Inspection = new("inspection");
    public static readonly FixtureProjection Evidence = new("evidence");
}

[ComplexValueObject]
public readonly partial struct ForceVector {
    public Vector3d Direction { get; }
    public Force Magnitude { get; }
    public Vector3d Vector => Direction / Direction.Length * Magnitude.As(ForceUnit.Newton);

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Vector3d direction, ref Force magnitude) {
        if (!(Fixtures.Finite(direction) && direction.Length > EpsilonPolicy.ZeroTolerance && Fixtures.Nonnegative(magnitude)))
            validationError = new ValidationError("force-vector");
    }
}

[ComplexValueObject]
public readonly partial struct MomentVector {
    public Vector3d Axis { get; }
    public Torque Magnitude { get; }
    public Vector3d Vector => Axis / Axis.Length * Magnitude.As(TorqueUnit.NewtonMeter);

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Vector3d axis, ref Torque magnitude) {
        if (!(Fixtures.Finite(axis) && axis.Length > EpsilonPolicy.ZeroTolerance && Fixtures.Nonnegative(magnitude)))
            validationError = new ValidationError("moment-vector");
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LoadCase : IValidityEvidence {
    private LoadCase() { }

    public sealed record Cutting(int Operation, ForceVector Force, MomentVector Moment, Point3d At, Loop Region) : LoadCase;
    public sealed record Gravity(ForceVector Force, Point3d Center) : LoadCase;
    public sealed record Acceleration(ForceVector Force, MomentVector Moment, Point3d Center) : LoadCase;
    public sealed record Probing(ForceVector Force, Point3d At) : LoadCase;
    public sealed record Handling(ForceVector Force, MomentVector Moment, Point3d At) : LoadCase;
    public sealed record Thermal(ForceVector Force, MomentVector Moment, Point3d At) : LoadCase;
    public sealed record Pressure(ForceVector Force, Point3d Center, Loop Region) : LoadCase;

    public (Vector3d Force, Vector3d Moment, Point3d At) Demand => Switch(
        cutting: static row => Regional(row.Force.Vector, row.Moment.Vector, row.At, row.Region),
        gravity: static row => (row.Force.Vector, Vector3d.Zero, row.Center),
        acceleration: static row => (row.Force.Vector, row.Moment.Vector, row.Center),
        probing: static row => (row.Force.Vector, Vector3d.Zero, row.At),
        handling: static row => (row.Force.Vector, row.Moment.Vector, row.At),
        thermal: static row => (row.Force.Vector, row.Moment.Vector, row.At),
        pressure: static row => Regional(row.Force.Vector, Vector3d.Zero, row.Center, row.Region));

    public bool IsValid => Switch(
        cutting: static row => ValidityClaim.All(
            ValidityClaim.Nonnegative(row.Operation), Fixtures.Finite(row.At), Fixtures.Profile(row.Region)),
        gravity: static row => Fixtures.Finite(row.Center),
        acceleration: static row => Fixtures.Finite(row.Center),
        probing: static row => Fixtures.Finite(row.At),
        handling: static row => Fixtures.Finite(row.At),
        thermal: static row => Fixtures.Finite(row.At),
        pressure: static row => ValidityClaim.All(Fixtures.Finite(row.Center), Fixtures.Profile(row.Region)));

    // Only the cutting case names an operation, so operation correspondence is one read rather than a second
    // seven-arm switch whose other six arms answer `true`.
    public Option<int> Operation => Switch(
        state: unit,
        cutting: static (_, row) => Some(row.Operation),
        gravity: static (_, _) => Option<int>.None,
        acceleration: static (_, _) => Option<int>.None,
        probing: static (_, _) => Option<int>.None,
        handling: static (_, _) => Option<int>.None,
        thermal: static (_, _) => Option<int>.None,
        pressure: static (_, _) => Option<int>.None);

    private static (Vector3d Force, Vector3d Moment, Point3d At) Regional(
        Vector3d force,
        Vector3d moment,
        Point3d reference,
        Loop region) => (
            force,
            moment + Vector3d.CrossProduct(Fixtures.Meters(region.Bound().Center - reference), force),
            reference);
}

public readonly record struct AxisMargin(Vector3d Capacity, Vector3d Demand) {
    public double Minimum => Seq(
        Demand.X > 0.0 ? Capacity.X / Demand.X : double.PositiveInfinity,
        Demand.Y > 0.0 ? Capacity.Y / Demand.Y : double.PositiveInfinity,
        Demand.Z > 0.0 ? Capacity.Z / Demand.Z : double.PositiveInfinity).Min(double.PositiveInfinity);
}

// The solve's own outputs as named columns. `Utilization` is the per-reaction demand fraction the normalized
// solve returned, `Scale` its reciprocal maximum, and `Residual` the normal-equation witness the kernel receipt
// carried — an equilibrium the contact set cannot represent shows here rather than as a silently small margin.
public sealed record RestraintSolution(
    double Scale,
    double Residual,
    Seq<double> Utilization,
    Seq<Vector3d> Forces,
    double FrictionMargin,
    double PullOffMargin,
    double LiftMargin);

public readonly record struct LoadMargin(
    LoadCase Load,
    AxisMargin Force,
    AxisMargin Moment,
    RestraintSolution Solution,
    double PressureMargin,
    double DeflectionMargin,
    double TangentialDeflectionMargin,
    double TipMargin,
    Seq<ContactReaction> Reactions) {
    public double Minimum => Seq(
        Force.Minimum, Moment.Minimum, Solution.FrictionMargin, Solution.PullOffMargin, Solution.LiftMargin,
        PressureMargin, DeflectionMargin, TangentialDeflectionMargin, TipMargin, Solution.Scale).Min(double.PositiveInfinity);
}

public sealed record RestraintProof(Seq<LoadMargin> Loads, Seq<ContactPatch> Contacts) {
    public double MinimumMargin => Loads.Min(static receipt => receipt.Minimum);
    public bool Holds => MinimumMargin >= 1.0;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FixtureArtifact {
    private FixtureArtifact() { }

    public sealed record Machine(ContentKey Key, Seq<ExclusionZone> Zones, DatumFrame Datum, ConstraintCensus Constraint) : FixtureArtifact;
    public sealed record SetupSheet(ContentKey Key, Seq<FixtureElement> Elements, Seq<FixtureStep> Sequence, DatumFrame Datum, ConstraintCensus Constraint) : FixtureArtifact;
    public sealed record Inspection(ContentKey Key, Seq<ContactPatch> Contacts, DatumFrame Datum, ConstraintCensus Constraint) : FixtureArtifact;
    public sealed record Evidence(ContentKey Key, Fixture Fixture) : FixtureArtifact;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record WorkholdingOp {
    private WorkholdingOp() { }

    public sealed record Admit(FixtureSpec Spec) : WorkholdingOp;
    public sealed record Condition(Fixture Fixture, FixtureState State, Seq<Move> Moves) : WorkholdingOp;
    public sealed record Clear(Fixture Fixture, FixtureState State, ToolCorridor Corridor) : WorkholdingOp;
    public sealed record Machined(Fixture Fixture, StockSnapshot Stock) : WorkholdingOp;
    public sealed record Restrain(Fixture Fixture, Seq<LoadCase> Loads, Ratio SafetyFactor) : WorkholdingOp;
    public sealed record Transition(Fixture Fixture, FixtureStage From, FixtureStage To) : WorkholdingOp;
    public sealed record Synthesize(FixtureSynthesis Seed) : WorkholdingOp;
    public sealed record Project(Fixture Fixture, FixtureProjection Projection) : WorkholdingOp;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record WorkholdingResult {
    private WorkholdingResult() { }

    public sealed record Admitted(Fixture Fixture) : WorkholdingResult;
    public sealed record Conditioned(Seq<Move> Moves) : WorkholdingResult;
    public sealed record Clearance(Option<ExclusionZone> Blocked) : WorkholdingResult {
        public bool Clear => Blocked.IsNone;
    }
    public sealed record MachinedHit(Option<Point3d> Point) : WorkholdingResult;
    public sealed record Restrained(RestraintProof Receipt) : WorkholdingResult;
    public sealed record Transitioned(FixtureState State, Arr<int> Active) : WorkholdingResult;
    public sealed record Synthesized(Seq<FixtureCandidate> Candidates) : WorkholdingResult;
    public sealed record Projected(FixtureArtifact Artifact) : WorkholdingResult;
}

// --- [OPERATIONS] ---------------------------------------------------------------------------------------------------------------------------------
public static class Workholding {
    public static Fin<WorkholdingResult> Apply(WorkholdingOp? candidate) =>
        Optional(candidate).ToFin(FabricationFault.Fixture(new FixturingWitness.Absent()))
            .Bind(static op => op.Switch(
                admit: static row => Fixture.Admit(row.Spec)
                    .Map<WorkholdingResult>(static fixture => new WorkholdingResult.Admitted(fixture)),
                condition: static row => Fixtures.Condition(row.Fixture, row.State, row.Moves)
                    .Map<WorkholdingResult>(static moves => new WorkholdingResult.Conditioned(moves)),
                clear: static row => Fixtures.Clear(row.Fixture, row.State, row.Corridor)
                    .Map<WorkholdingResult>(static blocked => new WorkholdingResult.Clearance(blocked)),
                machined: static row => Fixtures.Machined(row.Fixture, row.Stock)
                    .Map<WorkholdingResult>(static point => new WorkholdingResult.MachinedHit(point)),
                restrain: static row => Fixtures.Restrain(row.Fixture, row.Loads, row.SafetyFactor.As(RatioUnit.DecimalFraction))
                    .Map<WorkholdingResult>(static receipt => new WorkholdingResult.Restrained(receipt)),
                transition: static row => Fixtures.Transition(row.Fixture, row.From, row.To)
                    .Map<WorkholdingResult>(static receipt => new WorkholdingResult.Transitioned(receipt.State, receipt.Active)),
                synthesize: static row => Fixtures.Synthesize(row.Seed)
                    .Map<WorkholdingResult>(static candidates => new WorkholdingResult.Synthesized(candidates)),
                project: static row => Fixtures.Project(row.Fixture, row.Projection)
                    .Map<WorkholdingResult>(static artifact => new WorkholdingResult.Projected(artifact))));
}

internal static class Fixtures {
    // --- [ADMISSION]
    internal static K<Validation<Error>, FixtureSpec> GateSpec(FixtureSpec spec) =>
        AdmissionSlots.Gate(
            ValidityClaim.All(
                ValidityClaim.Nonnegative(spec.Operation), Positive(spec.ArcChordError), Finite(spec.InitialCursor), spec.Profiles.ForAll(Profile),
                ValidRuns(spec.Profiles, spec.Runs), !spec.Elements.IsEmpty,
                spec.Elements.Map(static element => element.Element).Distinct().Count == spec.Elements.Count),
            FabricationFault.Fixture(new FixturingWitness.Aggregate(
                spec.Operation,
                spec.Elements.Count,
                spec.Elements.Count - spec.Elements.Map(static element => element.Element).Distinct().Count)))
            .Map(_ => spec);

    // The stage sequence is a DAG, so acyclicity rails BEFORE the sort throws, the topological order is the
    // authority every later fold reads, and unreachable stages are named on the witness rather than skipped.
    internal static K<Validation<Error>, StageLifecycle> GateLifecycle(FixtureSpec spec) {
        BidirectionalGraph<FixtureStage, SEdge<FixtureStage>> stages = new(allowParallelEdges: false);
        stages.AddVertexRange(spec.Sequence.Map(static step => step.Stage));
        stages.AddEdgeRange(spec.Sequence.Zip(spec.Sequence.Tail)
            .Map(static pair => new SEdge<FixtureStage>(pair.First.Stage, pair.Second.Stage)));
        Error broken = FabricationFault.Fixture(
            new FixturingWitness.Lifecycle(spec.Sequence.Head, None, spec.Sequence.Count, spec.Sequence.Count));
        if (spec.Sequence.IsEmpty || !stages.IsDirectedAcyclicGraph()
            || spec.Sequence.Map(static step => step.Stage).Distinct().Count != spec.Sequence.Count)
            return Validation<Error, StageLifecycle>.Fail(broken);

        Seq<FixtureStage> order = toSeq(stages.SourceFirstBidirectionalTopologicalSort(TopologicalSortDirection.Forward));
        TryFunc<FixtureStage, IEnumerable<SEdge<FixtureStage>>> reach = stages.TreeBreadthFirstSearch(order[0]);
        Set<FixtureStage> reachable = order.Filter(stage => stage == order[0] || reach(stage, out _)).ToSet();
        Map<FixtureStage, FixtureStep> steps = toMap(spec.Sequence.Map(static step => (step.Stage, step)));
        HashMap<int, FixtureRole> roles = spec.Elements.Fold(HashMap<int, FixtureRole>(),
            static (map, element) => map.Add(element.Element, element.Role));
        HashMap<int, bool> held = spec.Elements.Fold(HashMap<int, bool>(),
            static (map, element) => map.Add(element.Element, element.HeldOnLoss));
        Seq<FixtureRole> custody = Seq(FixtureRole.Locate, FixtureRole.Support, FixtureRole.Clamp);

        (Set<int> Active, Map<FixtureStage, Set<int>> Live, Option<(FixtureStep Step, Option<FixtureRole> Uncovered)> Broken) walk =
            order.Fold(
                (Active: Set<int>(), Live: Map<FixtureStage, Set<int>>(), Broken: Option<(FixtureStep, Option<FixtureRole>)>.None),
                (state, stage) => {
                    FixtureStep step = steps[stage];
                    Set<int> next = state.Active.Union(step.Activate).Except(step.Release);
                    Option<FixtureRole> uncovered = step.State.Cutting
                        ? custody.Find(role => !next.Exists(element => roles.Find(element).Contains(role)))
                            | (next.Exists(element => roles.Find(element).Contains(FixtureRole.Clamp)
                                && held.Find(element).Exists(static retained => !retained))
                                    ? Some(FixtureRole.Clamp)
                                    : None)
                        : None;
                    bool ordered = step.Activate.ForAll(element => roles.ContainsKey(element) && !state.Active.Contains(element))
                        && step.Release.ForAll(state.Active.Contains);
                    return (next, state.Live.Add(stage, next),
                        state.Broken | (ordered && uncovered.IsNone ? None : Some((step, uncovered))));
                });

        return AdmissionSlots.Gate(
            walk.Broken.IsNone
            && reachable.Count == spec.Sequence.Count
            && spec.Sequence.ForAll(static step => Nonnegative(step.Settle))
            && spec.Elements.ForAll(element => spec.Sequence.Exists(step => step.Activate.Contains(element.Element))),
            FabricationFault.Fixture(new FixturingWitness.Lifecycle(
                walk.Broken.Map(static row => row.Step),
                walk.Broken.Bind(static row => row.Uncovered),
                spec.Sequence.Count,
                spec.Sequence.Count - reachable.Count)))
            .Map(_ => new StageLifecycle(order, reachable, walk.Live));
    }

    internal static K<Validation<Error>, Unit> GateDatum(FixtureSpec spec) {
        Set<int> locators = toSet(spec.Elements
            .Filter(static element => element.Role == FixtureRole.Locate)
            .Map(static element => element.Element));
        Set<int> datum = toSet(spec.Datum.Primary.Concat(spec.Datum.Secondary).Concat(spec.Datum.Tertiary));
        bool disjoint = datum.Count == spec.Datum.Primary.Count + spec.Datum.Secondary.Count + spec.Datum.Tertiary.Count;
        return AdmissionSlots.Gate(
            spec.Datum.Primary.Count >= 1 && spec.Datum.Secondary.Count >= 1 && spec.Datum.Tertiary.Count >= 1
            && disjoint && datum.ForAll(locators.Contains)
            && spec.Datum.Work.IsValid && Nonnegative(spec.Datum.Repeatability),
            FabricationFault.Fixture(new FixturingWitness.Datum(
                spec.Datum.Primary.Count, spec.Datum.Secondary.Count, spec.Datum.Tertiary.Count,
                datum.Count(element => !locators.Contains(element))));
    }

    internal static Fin<Seq<ExclusionZone>> Zones(FixtureSpec spec) =>
        spec.Elements.Traverse(element => Zone(spec, element).ToValidation()).As().ToFin()
            .Map(static rows => rows.Choose(identity));

    // A zone is the element's own keep-out loops offset by its margin, banded between the footprint floor and
    // the mechanism height, and active across every stage that holds it — one body for every mechanism.
    private static Fin<Option<ExclusionZone>> Zone(FixtureSpec spec, FixtureElement element) {
        Seq<Loop> shape = element.Keepouts;
        if (shape.IsEmpty) return Fin.Succ(Option<ExclusionZone>.None);
        double chord = spec.ArcChordError.As(LengthUnit.Millimeter);
        return Offset(shape, element.MarginMm).Bind(keepouts => keepouts.IsEmpty
            ? Fin.Fail<Option<ExclusionZone>>(
                new GeometryFault.DegenerateInput(Kind.Polyline, element.Element, nameof(ExclusionZone)))
            : Fin.Succ(Some(new ExclusionZone(
                spec.Operation,
                element.Element,
                element.Role,
                element.Kind,
                keepouts,
                Length.FromMillimeters(keepouts.Bind(static loop => loop.Vertices).Min(static point => point.Z)),
                Length.FromMillimeters(keepouts.Bind(static loop => loop.Vertices).Max(static point => point.Z)
                    + Math.Max(element.HeightMm, chord)),
                spec.Sequence.Fold((Held: false, States: Set<FixtureState>()), (state, step) => {
                    bool live = (state.Held || step.Activate.Contains(element.Element)) && !step.Release.Contains(element.Element);
                    return (live, live ? state.States.Add(step.State) : state.States);
                }).States,
                spec.ArcChordError))));
    }

    // --- [CONSTRAINT]
    // The empty reaction set refuses HERE. A locating-only roster — every `WorkholdingKind` declaring zero contacts —
    // clears spec, lifecycle, and datum admission and reaches this kernel before the fixture's own contact gate ever
    // runs, and the rank fold's scale is a selector `Max` that throws on an empty sequence rather than railing.
    internal static Fin<ConstraintCensus> Constraint(Seq<ContactPatch> contacts) {
        Seq<ContactReaction> reactions = contacts.Bind(static contact => contact.Field);
        if (reactions.IsEmpty)
            return Fin.Fail<ConstraintCensus>(FabricationFault.Fixture(
                new FixturingWitness.Closure(Rank: 0, Required: 6, Redundancy: 0, Frictionless: 0)));
        Seq<Wrench> normal = reactions.Map(static reaction => Wrench.Of(reaction.At, Unitized(reaction.Normal)));
        Seq<Wrench> closure = normal + reactions.Bind(Friction);
        double lever = Lever(reactions);
        int rank = ConstraintRank(closure, lever);
        return Fin.Succ(new ConstraintCensus(
            rank, ConstraintRank(normal, lever), Math.Max(0, closure.Count - rank), reactions));
    }

    // Frictional contact spans three wrench directions, not one: dropping the tangential pair caps an opposed-jaw
    // fixture at rank 3 and reports every valid vise as underconstrained.
    private static Seq<Wrench> Friction(ContactReaction reaction) {
        if (!Positive(reaction.TangentialCapacity)) return Seq<Wrench>();
        Vector3d normal = Unitized(reaction.Normal);
        Vector3d first = Unitized(Vector3d.CrossProduct(normal, Math.Abs(normal.Z) < 0.9 ? Vector3d.ZAxis : Vector3d.XAxis));
        return Seq(Wrench.Of(reaction.At, first), Wrench.Of(reaction.At, Unitized(Vector3d.CrossProduct(normal, first))));
    }

    // The moment normalizer is the station extent, read from ONE bounding pass — the pairwise maximum it replaces
    // computed the same span in quadratic time.
    private static double Lever(Seq<ContactReaction> reactions) {
        BoundingBox stations = reactions.Fold(BoundingBox.Empty, static (box, reaction) => { box.Union(reaction.At); return box; });
        return Math.Max(EpsilonPolicy.SqrtEpsilon, Meters(stations.Diagonal).Length);
    }

    private static int ConstraintRank(Seq<Wrench> wrenches, double lever) {
        double[,] basis = new double[6, 6];
        int rank = 0;
        double scale = wrenches.Max(wrench => Math.Sqrt(
            (wrench.Force * wrench.Force) + ((wrench.Moment * wrench.Moment) / (lever * lever))));
        Span<double> row = stackalloc double[6];
        foreach (Wrench wrench in wrenches) {
            row[0] = wrench.Force.X; row[1] = wrench.Force.Y; row[2] = wrench.Force.Z;
            row[3] = wrench.Moment.X / lever;
            row[4] = wrench.Moment.Y / lever;
            row[5] = wrench.Moment.Z / lever;
            for (int held = 0; held < rank; held++) {
                double dot = 0.0;
                for (int column = 0; column < 6; column++) dot += row[column] * basis[held, column];
                for (int column = 0; column < 6; column++) row[column] -= dot * basis[held, column];
            }
            double squared = 0.0;
            for (int column = 0; column < 6; column++) squared += row[column] * row[column];
            double norm = Math.Sqrt(squared);
            if (norm <= EpsilonPolicy.SqrtEpsilon * scale) continue;
            for (int column = 0; column < 6; column++) basis[rank, column] = row[column] / norm;
            if (++rank == 6) break;
        }
        return rank;
    }

    // --- [RESTRAINT]
    internal static Fin<RestraintProof> Restrain(Fixture fixture, Seq<LoadCase> loads, double safety) =>
        !loads.IsEmpty && loads.ForAll(load => ValidLoad(fixture, load)) && double.IsFinite(safety) && safety >= 1.0
            ? Support(fixture.Contacts).Bind(support => loads
                .Traverse(load => Evaluate(fixture.Contacts, support, load, safety).ToValidation())
                .As().ToFin().Map(receipts => new RestraintProof(receipts, fixture.Contacts)))
            : Fin.Fail<RestraintProof>(FabricationFault.Fixture(new FixturingWitness.Restraint(
                loads.Count, loads.Count(load => !ValidLoad(fixture, load)), safety)));

    private static Fin<LoadMargin> Evaluate(Seq<ContactPatch> contacts, Seq<Loop> support, LoadCase load, double safety) {
        (Vector3d force, Vector3d moment, Point3d at) = load.Demand;
        Seq<ContactReaction> reactions = contacts.Bind(static contact => contact.Field);
        Vector3d forceDemand = force * safety;
        Vector3d momentDemand = (moment + Vector3d.CrossProduct(Meters(at - Point3d.Origin), force)) * safety;
        return Utilization(reactions, new Wrench(forceDemand, momentDemand)).Map(solution => new LoadMargin(
            load,
            new AxisMargin(Scaled(forceDemand, solution.Scale), Abs(forceDemand)),
            new AxisMargin(Scaled(momentDemand, solution.Scale), Abs(momentDemand)),
            solution,
            // Pressure x Area closes to Force in the dimensioned algebra, so the mm2 footprint reaches a newton
            // through the package's own scale and no transcribed conversion sits between a pascal limit and a preload.
            contacts.Min(contact => Ratio(
                (contact.Law.PressureLimit * Area.FromSquareMillimeters(Math.Abs(contact.Footprint.Area()))).Newtons,
                contact.Preload.As(ForceUnit.Newton))),
            contacts.Min(contact => Ratio(
                contact.Law.NormalStiffnessNPerMm * contact.Law.DeflectionLimit.As(LengthUnit.Millimeter),
                contact.Preload.As(ForceUnit.Newton))),
            contacts.Min(contact => Ratio(
                contact.Law.TangentialStiffnessNPerMm * contact.Law.DeflectionLimit.As(LengthUnit.Millimeter),
                forceDemand.Length / contacts.Count)),
            TipMargin(support, reactions, forceDemand, momentDemand, at),
            reactions));
    }

    // Exemption: the triplet assembly is a measured numeric kernel. Each reaction contributes three columns —
    // normal, and two friction tangents — scaled by that reaction's own capacity, so the minimum-norm solution
    // the kernel returns is DIMENSIONLESS: coefficient magnitude IS the fraction of capacity the distribution
    // spends, and the admissible load factor is the reciprocal of the largest one.
    private static Fin<RestraintSolution> Utilization(Seq<ContactReaction> reactions, Wrench demand) {
        double lever = Lever(reactions);
        Seq<(Vector3d Direction, double Capacity, int Axis)> columns = reactions.Bind((reaction, index) => {
            Vector3d normal = Unitized(reaction.Normal);
            Vector3d first = Unitized(Vector3d.CrossProduct(normal, Math.Abs(normal.Z) < 0.9 ? Vector3d.ZAxis : Vector3d.XAxis));
            double tangential = reaction.TangentialCapacity.As(ForceUnit.Newton) * reaction.AreaWeight;
            return Seq(
                (normal, reaction.NormalCapacity.As(ForceUnit.Newton) * reaction.AreaWeight, index),
                (first, tangential, index),
                (Unitized(Vector3d.CrossProduct(normal, first)), tangential, index));
        });
        IEnumerable<(int Row, int Col, double Value)> triplets = columns.Map((column, index) => {
            Wrench wrench = Wrench.Of(reactions[column.Axis].At, column.Direction * column.Capacity);
            return Seq(
                (0, index, wrench.Force.X), (1, index, wrench.Force.Y), (2, index, wrench.Force.Z),
                (3, index, wrench.Moment.X / lever), (4, index, wrench.Moment.Y / lever), (5, index, wrench.Moment.Z / lever));
        }).Bind(identity);

        // `Dimension` is a kernel `[ValueObject<int>]`: its generated factory is `Create` and returns the bare
        // value, so both extents bind above the query rather than as monadic clauses that have no carrier.
        Dimension rows = Dimension.Create(6);
        Dimension cols = Dimension.Create(columns.Count);
        return from matrix in SparseMatrix.FromTriplets(rows, cols, triplets)
               from solved in matrix.SolveLeastSquaresDetailed(Arr(
                   demand.Force.X, demand.Force.Y, demand.Force.Z,
                   demand.Moment.X / lever, demand.Moment.Y / lever, demand.Moment.Z / lever))
               select Read(reactions, columns, solved);
    }

    private static RestraintSolution Read(
        Seq<ContactReaction> reactions,
        Seq<(Vector3d Direction, double Capacity, int Axis)> columns,
        SolveReceipt solved) {
        Seq<double> coefficients = toSeq(solved.Solution);
        Seq<Vector3d> forces = reactions.Map((_, index) => columns
            .Map((column, slot) => (column, slot))
            .Filter(row => row.column.Axis == index)
            .Fold(Vector3d.Zero, (sum, row) => sum + (row.column.Direction * row.column.Capacity * coefficients[row.slot])));
        // A tensile reaction is bounded by PULL-OFF, not by normal capacity, so the compressive coefficient
        // re-scales against the pull-off column before it enters the utilization census.
        Seq<double> normal = reactions.Map((reaction, index) => coefficients[index * 3] switch {
            >= 0.0 and var push => push,
            var pull => Math.Abs(pull) * Ratio(
                reaction.NormalCapacity.As(ForceUnit.Newton), reaction.PullOffCapacity.As(ForceUnit.Newton)),
        });
        Seq<double> tangential = reactions.Map((_, index) =>
            Math.Sqrt((coefficients[(index * 3) + 1] * coefficients[(index * 3) + 1])
                + (coefficients[(index * 3) + 2] * coefficients[(index * 3) + 2])));
        Seq<double> utilization = normal.Zip(tangential).Map(static row => Math.Max(Math.Abs(row.First), row.Second));
        return new RestraintSolution(
            Invert(utilization.Max(0.0)),
            solved.Residual,
            utilization,
            forces,
            Invert(tangential.Max(0.0)),
            Invert(reactions.Zip(normal).Filter(static row => row.Second < 0.0).Map(static row => -row.Second).Max(0.0)),
            Invert(normal.Filter(static value => value > 0.0).Max(0.0)));
    }

    // Tipping is overturning about a SUPPORT-REGION edge, not general moment capacity: the restoring term is the
    // normal reaction's own lever about that edge, so a load inside the region is stable while the same magnitude
    // outside it tips. The region is the union of contact footprints, so a concave seat under-reports its lever.
    private static double TipMargin(Seq<Loop> support, Seq<ContactReaction> reactions, Vector3d force, Vector3d moment, Point3d at) {
        Seq<Edge3> edges = support.Bind(static loop =>
            toSeq(Enumerable.Range(0, loop.Count)).Map(index => new Edge3(loop.At(index), loop.At(index + 1))));
        if (edges.IsEmpty) return 0.0;
        Vector3d arm = Meters(at - Point3d.Origin);
        return edges.Min(edge => {
            Vector3d axis = Unitized(Meters(edge.B - edge.A));
            Vector3d pivot = Meters(edge.A - Point3d.Origin);
            double overturning = Math.Abs(axis * (moment + Vector3d.CrossProduct(arm - pivot, force)));
            double restoring = reactions.Sum(reaction => Math.Max(0.0, axis * Vector3d.CrossProduct(
                Meters(reaction.At - Point3d.Origin) - pivot, -Unitized(reaction.Normal)))
                * reaction.NormalCapacity.As(ForceUnit.Newton) * reaction.AreaWeight);
            return Ratio(restoring, overturning);
        });
    }

    private static Fin<Seq<Loop>> Support(Seq<ContactPatch> contacts) {
        Seq<Loop> footprints = contacts.Map(static contact => contact.Footprint);
        return Regions(footprints, footprints, BooleanOp.Union, nameof(TipMargin))
            .Map(static topology => topology.Nodes.Filter(static node => !node.IsHole).Map(static node => node.Boundary));
    }

    // --- [CONDITIONING]
    internal static Fin<Seq<Move>> Condition(Fixture fixture, FixtureState state, Seq<Move> moves) =>
        ValidRuns(fixture.Spec.Profiles, fixture.Spec.Runs)
        && fixture.Spec.Runs[fixture.Spec.Runs.Count - 1].Start + fixture.Spec.Runs[fixture.Spec.Runs.Count - 1].Count == moves.Count
            ? moves
                .Fold(
                    Fin.Succ((Cursor: fixture.InitialCursor, Path: Seq<Edge3>())),
                    (rail, move) => rail.Bind(current => Segments(current.Cursor, move, fixture.Spec.ArcChordError.As(LengthUnit.Millimeter))
                        .Map(path => (move.Target, current.Path + path))))
                .Bind(current => Blocked(fixture.Zones, current.Path, state).Bind(hit => hit.Match(
                    Some: zone => zone.Collision.Bind(volume =>
                        Fin.Fail<Seq<Move>>(new FabricationFault.Collision(volume, CollisionContact.Cutter))),
                    None: () => Fin.Succ(moves))))
            : Fin.Fail<Seq<Move>>(FabricationFault.Fixture(new FixturingWitness.Partition(
                fixture.Spec.Runs.Sum(static run => run.Count), moves.Count)));

    internal static Fin<Option<ExclusionZone>> Clear(Fixture fixture, FixtureState state, ToolCorridor corridor) =>
        corridor.Stations.Count >= 2 && corridor.Stations.ForAll(station =>
            Finite(station.Point) && double.IsFinite(corridor.Kind.RadiusMm(station)) && corridor.Kind.RadiusMm(station) >= 0.0)
            ? corridor.Stations.Zip(corridor.Stations.Tail)
                .Map(pair => (Axis: new Edge3(pair.First.Point, pair.Second.Point),
                    Radius: Math.Max(corridor.Kind.RadiusMm(pair.First), corridor.Kind.RadiusMm(pair.Second))))
                .Fold(Fin.Succ(Option<ExclusionZone>.None), (rail, leg) => rail.Bind(found => found.IsSome
                    ? Fin.Succ(found)
                    : Inflated(fixture.Zones, leg.Radius).Bind(grown => Blocked(grown, Seq(leg.Axis), state))))
            : Fin.Fail<Option<ExclusionZone>>(FabricationFault.Fixture(
                new FixturingWitness.Corridor(corridor.Kind, corridor.Stations.Count)));

    // ONE open-path clip per zone over the WHOLE banded path: a per-segment membership walk re-entered the same
    // overlay once per edge and per wall, and the densified wall ring it needed disappears with it.
    private static Fin<Option<ExclusionZone>> Blocked(Seq<ExclusionZone> zones, Seq<Edge3> path, FixtureState state) =>
        zones.Fold(Fin.Succ(Option<ExclusionZone>.None), (rail, zone) => rail.Bind(found => found.IsSome
            ? Fin.Succ(found)
            : path.Choose(segment => zone.Banded(segment, state)) is { IsEmpty: false } banded
                ? Clipped(banded.Map(static segment => Seq(segment)), zone.Keepouts)
                    .Map(inside => inside.Exists(static run => !run.IsEmpty) ? Some(zone) : Option<ExclusionZone>.None)
                : Fin.Succ(Option<ExclusionZone>.None)));

    private static Fin<Seq<ExclusionZone>> Inflated(Seq<ExclusionZone> zones, double radius) =>
        zones.Traverse(zone => Offset(zone.Keepouts, radius)
            .Map(grown => zone with { Keepouts = grown })
            .ToValidation()).As().ToFin();

    internal static Fin<Option<Point3d>> Machined(Fixture fixture, StockSnapshot stock) =>
        Regions(fixture.Zones.Bind(static zone => zone.Keepouts), stock.Machined.ToSeq(), BooleanOp.Intersection, nameof(Machined))
            .Map(static topology => topology.Nodes
                .Find(static node => !node.IsHole && node.Boundary.Count > 0)
                .Map(static node => node.Boundary.At(0)));

    // --- [SYNTHESIS]
    // Programs enumerate directly at the admitted cardinalities: the powerset over the template roster is 2^n
    // candidates for an n the seed never bounds, and each survivor costs a full admission, restraint, and
    // corridor pass.
    internal static Fin<Seq<FixtureCandidate>> Synthesize(FixtureSynthesis seed) {
        SynthesisBudget budget = seed.Budget;
        if (!Profile(seed.Part) || budget.Samples <= 0 || seed.Templates.IsEmpty || seed.Loads.IsEmpty
            || budget.MinimumTemplates <= 0 || budget.MaximumTemplates < budget.MinimumTemplates
            || budget.MaximumTemplates > seed.Templates.Count || budget.CandidateBudget <= 0
            || !AtLeastUnit(seed.SafetyFactor))
            return Fin.Fail<Seq<FixtureCandidate>>(FabricationFault.Fixture(new FixturingWitness.Synthesis(
                seed.Templates.Count, budget.MinimumTemplates, budget.MaximumTemplates, budget.CandidateBudget)));

        double safety = seed.SafetyFactor.As(RatioUnit.DecimalFraction);
        return Fixture.Admit(seed.Basis).Bind(basis => {
            int first = basis.Spec.Elements.Max(static element => element.Element) + 1;
            return toSeq(Programs(seed.Templates, budget).Take(budget.CandidateBudget))
                .Traverse(program => Candidate(seed, basis, program, first, safety).ToValidation())
                .As().ToFin()
                .Map(static candidates => toSeq(candidates
                    .Filter(static candidate => candidate.Holding.Holds && candidate.Clearance.ForAll(static receipt => receipt.Clear))
                    .OrderByDescending(static candidate => candidate.Score.Total)));
        });
    }

    private static Fin<FixtureCandidate> Candidate(
        FixtureSynthesis seed,
        Fixture basis,
        Seq<ClampTemplate> program,
        int first,
        double safety) =>
        program
            .Map((template, index) => template.Generate(seed.Part, seed.Budget.Samples, first + (index * seed.Budget.Samples)).ToValidation())
            .Traverse(static row => row).As().ToFin()
            .Bind(generated => SynthesisSequence(basis, generated.Bind(static row => row.Elements)).Bind(sequence =>
                Fixture.Admit(basis.Spec with {
                    Elements = basis.Spec.Elements.Filter(static element => element.Role != FixtureRole.Clamp)
                        + generated.Bind(static row => row.Elements),
                    Sequence = sequence,
                }).Bind(fixture => Restrain(fixture, seed.Loads, safety).Bind(holding => seed.Corridors
                    .Traverse(corridor => Clear(fixture, FixtureState.Cut, corridor)
                        .Map(static blocked => new WorkholdingResult.Clearance(blocked)).ToValidation())
                    .As().ToFin()
                    .Map(clearance => Rank(seed.Objective, fixture, holding, clearance,
                        generated.Choose(static row => row.Insert)))))));

    private static Fin<Seq<FixtureStep>> SynthesisSequence(Fixture basis, Seq<FixtureElement> generated) {
        Seq<(FixtureStep Step, int Index)> indexed = basis.Spec.Sequence.Map(static (step, index) => (step, index));
        Error broken = FabricationFault.Fixture(
            new FixturingWitness.Lifecycle(None, None, basis.Spec.Sequence.Count, 0));
        return indexed.Find(static row => row.Step.State == FixtureState.Clamp).ToFin(broken).Bind(activation =>
            indexed.Find(row => row.Index > activation.Index && row.Step.State == FixtureState.Unload).ToFin(broken).Map(release => {
                Set<int> retired = toSet(basis.Spec.Elements
                    .Filter(static element => element.Role == FixtureRole.Clamp)
                    .Map(static element => element.Element));
                Seq<int> added = generated.Map(static element => element.Element);
                return basis.Spec.Sequence.Map(step => step with {
                    Activate = (step.Activate.ToSeq().Filter(element => !retired.Contains(element))
                        + (step.Stage == activation.Step.Stage ? added : Seq<int>())).Distinct().ToArr(),
                    Release = (step.Release.ToSeq().Filter(element => !retired.Contains(element))
                        + (step.Stage == release.Step.Stage ? added : Seq<int>())).Distinct().ToArr(),
                });
            }));
    }

    private static IEnumerable<Seq<ClampTemplate>> Programs(Seq<ClampTemplate> templates, SynthesisBudget budget) =>
        Enumerable.Range(budget.MinimumTemplates, Math.Max(0, budget.MaximumTemplates - budget.MinimumTemplates + 1))
            .SelectMany(size => Choose(templates, size, cursor: 0));

    private static IEnumerable<Seq<ClampTemplate>> Choose(Seq<ClampTemplate> templates, int size, int cursor) =>
        size == 0
            ? [Seq<ClampTemplate>()]
            : Enumerable.Range(cursor, Math.Max(0, templates.Count - cursor - size + 1))
                .SelectMany(index => Choose(templates, size - 1, index + 1).Select(rest => rest.Insert(0, templates[index])));

    private static FixtureCandidate Rank(
        FixtureObjective objective,
        Fixture fixture,
        RestraintProof holding,
        Seq<WorkholdingResult.Clearance> clearance,
        Seq<SoftJawInsert> inserts) {
        double hold = holding.MinimumMargin / (1.0 + holding.MinimumMargin);
        double access = clearance.IsEmpty ? 1.0 : (double)clearance.Count(static receipt => receipt.Clear) / clearance.Count;
        double simplicity = 1.0 / (1.0 + fixture.Spec.Elements.Count + inserts.Count);
        return new FixtureCandidate(fixture, holding, clearance, inserts, new FixtureScore(hold, access, simplicity,
            ((hold * objective.Holding) + (access * objective.Access) + (simplicity * objective.Simplicity)) / objective.Total));
    }

    // --- [LIFECYCLE]
    // Reachability answers off the built order, so a transition between stages the sequence never connects
    // refuses instead of returning the active set of an unrelated step.
    internal static Fin<(FixtureState State, Arr<int> Active)> Transition(Fixture fixture, FixtureStage from, FixtureStage to) =>
        fixture.Lifecycle.Covers(from, to)
            ? fixture.Spec.Sequence.Find(step => step.Stage == to)
                .Map(step => (step.State, fixture.Lifecycle.At(to).IfNone(Set<int>()).ToArr()))
                .ToFin(Broken(fixture, from))
            : Fin.Fail<(FixtureState, Arr<int>)>(Broken(fixture, from));

    private static Error Broken(Fixture fixture, FixtureStage from) =>
        FabricationFault.Fixture(new FixturingWitness.Lifecycle(
            fixture.Spec.Sequence.Find(step => step.Stage == from), None, fixture.Spec.Sequence.Count, 0));

    // --- [PROJECTION]
    internal static Fin<FixtureArtifact> Project(Fixture fixture, FixtureProjection projection) =>
        Keyed(fixture, projection).Map(key => projection.Switch<FixtureArtifact>(
            machine: () => new FixtureArtifact.Machine(key, fixture.Zones, fixture.Spec.Datum, fixture.Constraint),
            setupSheet: () => new FixtureArtifact.SetupSheet(key, fixture.Spec.Elements, fixture.Spec.Sequence, fixture.Spec.Datum, fixture.Constraint),
            inspection: () => new FixtureArtifact.Inspection(key, fixture.Contacts, fixture.Spec.Datum, fixture.Constraint),
            evidence: () => new FixtureArtifact.Evidence(key, fixture)));

    // Every preimage frames and closes at `FabricationCanon` over the ONE `Rasm.Element` `CanonicalWriter`:
    // `Double` normalizes `-0.0` and every NaN payload before framing the IEEE bits, `String` length-prefixes
    // UTF-8 so no delimiter can forge equality, `Rows` writes the count ahead of its rows, and `Keyed` opens the
    // retaining mint and closes it on the rail so no artifact addresses under bytes no writer held.
    private static Fin<ContentKey> Keyed(Fixture fixture, FixtureProjection projection) =>
        FabricationCanon.Keyed(
            EgressKind.Plan,
            fixture.Spec.ArcChordError.As(LengthUnit.Millimeter),
            writer => projection.Switch(
                state: (Writer: writer.Discriminant(projection), Fixture: fixture),
                machine: static state => state.Fixture.Constraint.CanonicalBytes(state.Fixture.Spec.Datum.CanonicalBytes(
                    state.Writer.Rows(state.Fixture.Zones, static (held, zone) => zone.CanonicalBytes(held)))),
                setupSheet: static state => state.Fixture.Constraint.CanonicalBytes(state.Fixture.Spec.Datum.CanonicalBytes(
                    state.Writer
                        .Rows(state.Fixture.Spec.Elements, static (held, element) => element.CanonicalBytes(held))
                        .Rows(state.Fixture.Spec.Sequence, static (held, step) => step.CanonicalBytes(held)))),
                inspection: static state => state.Fixture.Constraint.CanonicalBytes(state.Fixture.Spec.Datum.CanonicalBytes(
                    state.Writer.Rows(state.Fixture.Contacts, static (held, contact) => contact.CanonicalBytes(held)))),
                evidence: static state => state.Fixture.CanonicalBytes(state.Writer)),
            Key);

    internal static Fin<ContentKey> ZoneIdentity(ExclusionZone zone) => FabricationCanon.Keyed(
        EgressKind.Plan,
        zone.ArcChordError.As(LengthUnit.Millimeter),
        zone.CanonicalBytes,
        Key);

    private static readonly Op Key = Op.Of(name: nameof(Workholding));

    // --- [GEOMETRY]
    private static Fin<Seq<Edge3>> Segments(Point3d from, Move move, double error) =>
        move.Switch(
            state: (From: from, Error: error),
            rapid: static (state, row) => Fin.Succ(Seq(new Edge3(state.From, row.Target))),
            linear: static (state, row) => Fin.Succ(Seq(new Edge3(state.From, row.Target))),
            circular: static (state, row) => ArcSegments(state.From, row.Target, row.Arc, state.Error));

    // Exemption: chord subdivision is a measured geometric kernel bounded by the admitted chord error.
    private static Fin<Seq<Edge3>> ArcSegments(Point3d from, Point3d to, ArcCenter arc, double error) {
        Vector3d start = from - arc.Center;
        Vector3d end = to - arc.Center;
        double radius = start.Length;
        if (!ValidityClaim.Positive(radius).Holds || Math.Abs(radius - end.Length) > error)
            return Fin.Fail<Seq<Edge3>>(new GeometryFault.DegenerateInput(Kind.Arc, None, nameof(ArcCenter)));
        double opening = Math.Atan2(start.Y, start.X);
        double closing = Math.Atan2(end.Y, end.X);
        double sweep = arc.Sense == RotationSense.Clockwise ? -Normalize(opening - closing) : Normalize(closing - opening);
        if (from.DistanceTo(to) <= error) sweep = arc.Sense == RotationSense.Clockwise ? -Math.Tau : Math.Tau;
        int count = Math.Max(1, (int)Math.Ceiling(Math.Abs(sweep)
            / Math.Max(EpsilonPolicy.SqrtEpsilon, 2.0 * Math.Acos(Math.Clamp(1.0 - (error / radius), -1.0, 1.0)))));
        Seq<Point3d> points = toSeq(Enumerable.Range(0, count + 1)).Map(index => {
            double angle = opening + (sweep * index / count);
            return new Point3d(
                arc.Center.X + (radius * Math.Cos(angle)),
                arc.Center.Y + (radius * Math.Sin(angle)),
                from.Z + ((to.Z - from.Z) * index / count));
        });
        return Fin.Succ(toSeq(Enumerable.Range(0, count)).Map(index => new Edge3(points[index], points[index + 1])));
    }

    // Exemption: the height band is a one-dimensional interval clip with no planar owner.
    internal static Option<Edge3> Slab(Edge3 segment, double lower, double upper) {
        double rise = segment.B.Z - segment.A.Z;
        if (Math.Abs(rise) < EpsilonPolicy.ZeroTolerance)
            return segment.A.Z >= lower && segment.A.Z <= upper ? Some(segment) : None;
        double first = (lower - segment.A.Z) / rise;
        double second = (upper - segment.A.Z) / rise;
        double opening = Math.Max(0.0, Math.Min(first, second));
        double closing = Math.Min(1.0, Math.Max(first, second));
        return opening <= closing
            ? Some(new Edge3(
                segment.A + (opening * (segment.B - segment.A)),
                segment.A + (closing * (segment.B - segment.A))))
            : None;
    }

    internal static Fin<Seq<Loop>> Offset(Seq<Loop> loops, double distance) =>
        distance <= EpsilonPolicy.ZeroTolerance
            ? Fin.Succ(loops)
            : loops.Head.ToFin(new GeometryFault.DegenerateInput(Kind.Polyline, None, nameof(ArcOp.Offset))).Bind(basis =>
                ArcForest.Admit(loops, basis.Tolerance, basis.Plane).Bind(forest =>
                    ArcAlgebra.Apply(new ArcOp.Offset(new ArcOffsetSource.Forest(forest), distance)).Bind(static trace => trace switch {
                        ArcTrace.Forest(var result, _) => Fin.Succ(result.Loops),
                        ArcTrace.Paths(var result, _) => Fin.Succ(result),
                        _ => Fin.Fail<Seq<Loop>>(new KernelFault.InvalidValue("workholding", $"{nameof(ArcOp.Offset)}:{nameof(ArcTrace)}")),
                    })));

    private static Fin<RegionTopology> Regions(Seq<Loop> subject, Seq<Loop> clip, BooleanOp kind, string locus) =>
        PolygonAlgebra
            .Apply(new PolygonOp.Boolean(subject, clip, kind, PolygonFill.NonZero), Op.Of(name: locus))
            .Bind(trace => trace.Regioned(new KernelFault.InvalidValue("workholding", locus)));

    private static Fin<Seq<Seq<Edge3>>> Clipped(Seq<Seq<Edge3>> runs, Seq<Loop> clip) =>
        PolygonAlgebra
            .Apply(new PolygonOp.ClipOpen(runs, clip, PolygonFill.NonZero), Op.Of(name: nameof(ExclusionZone)))
            .Bind(static trace => trace
                .Runs(new KernelFault.InvalidValue("workholding", nameof(ExclusionZone)))
                .Map(static split => split.Inside));

    internal static Fin<Loop> Box(Point3d center, double width, double depth, Context tolerance) {
        double halfWidth = 0.5 * width;
        double halfDepth = 0.5 * depth;
        return Loop.Admit(Arr(
            new Point3d(center.X - halfWidth, center.Y - halfDepth, center.Z),
            new Point3d(center.X + halfWidth, center.Y - halfDepth, center.Z),
            new Point3d(center.X + halfWidth, center.Y + halfDepth, center.Z),
            new Point3d(center.X - halfWidth, center.Y + halfDepth, center.Z)), closed: true, Arr<double>(), tolerance);
    }

    internal static Seq<Point3d> Stations(Loop part, int count) =>
        toSeq(Enumerable.Range(0, count)).Map(index => part.At(index * Math.Max(1, part.Count / count)));

    internal static Map<FixtureMetric, double> Millimetres(params (FixtureMetric Axis, Length Value)[] rows) =>
        toMap(toSeq(rows).Map(static row => (row.Axis, row.Value.As(LengthUnit.Millimeter))));

    // --- [PREDICATES]
    internal static Vector3d Meters(Vector3d millimetres) => new(
        Length.FromMillimeters(millimetres.X).As(LengthUnit.Meter),
        Length.FromMillimeters(millimetres.Y).As(LengthUnit.Meter),
        Length.FromMillimeters(millimetres.Z).As(LengthUnit.Meter));

    private static Vector3d Unitized(Vector3d value) {
        Vector3d unit = value;
        unit.Unitize();
        return unit;
    }

    // A ratio whose denominator vanishes is UNBOUNDED, not a large number: one reciprocal owner keeps every
    // margin on one convention and no call site re-spells a guard against its own divisor.
    private static double Ratio(double capacity, double used) =>
        used <= EpsilonPolicy.SqrtEpsilon ? double.PositiveInfinity : capacity / used;

    private static double Invert(double utilization) => Ratio(1.0, utilization);
    private static Vector3d Abs(Vector3d value) => new(Math.Abs(value.X), Math.Abs(value.Y), Math.Abs(value.Z));
    private static Vector3d Scaled(Vector3d demand, double scale) => new(
        demand.X == 0.0 ? 0.0 : Math.Abs(demand.X) * scale,
        demand.Y == 0.0 ? 0.0 : Math.Abs(demand.Y) * scale,
        demand.Z == 0.0 ? 0.0 : Math.Abs(demand.Z) * scale);
    private static double Normalize(double radians) => radians < 0.0 ? radians + Math.Tau : radians;

    private static bool ValidLoad(Fixture fixture, LoadCase load) =>
        load.IsValid && load.Operation.ForAll(operation => operation == fixture.Operation);

    internal static bool ValidRuns(Arr<Loop> profiles, Seq<MoveRun> runs) =>
        !runs.IsEmpty && runs[0].Start == 0
        && runs.ForAll(run => run.Loop >= 0 && run.Loop < profiles.Count && run.Start >= 0 && run.Count > 0)
        && runs.Zip(runs.Tail).ForAll(static pair => pair.First.Start + pair.First.Count == pair.Second.Start);

    internal static bool Profile(Loop loop) =>
        loop is not null && loop.Closed && loop.Count >= 3 && loop.Vertices.ForAll(Finite) && loop.Bulges.ForAll(double.IsFinite);
    internal static bool Finite(Point3d value) => TensorPrimitives.IsFiniteAll<double>([value.X, value.Y, value.Z]);
    internal static bool Finite(Vector3d value) => TensorPrimitives.IsFiniteAll<double>([value.X, value.Y, value.Z]);
    internal static bool Unit(Vector3d value) => Finite(value) && value.Length > EpsilonPolicy.ZeroTolerance;
    internal static bool Positive(Length value) => ValidityClaim.Positive(value.As(LengthUnit.Millimeter));
    internal static bool Nonnegative(Length value) => double.IsFinite(value.As(LengthUnit.Millimeter)) && value.As(LengthUnit.Millimeter) >= 0.0;
    internal static bool Positive(Force value) => ValidityClaim.Positive(value.As(ForceUnit.Newton));
    internal static bool Nonnegative(Force value) => double.IsFinite(value.As(ForceUnit.Newton)) && value.As(ForceUnit.Newton) >= 0.0;
    internal static bool Positive(Pressure value) => ValidityClaim.Positive(value.As(PressureUnit.Pascal));
    internal static bool Positive(Area value) => ValidityClaim.Positive(value.As(AreaUnit.SquareMeter));
    internal static bool Positive(Torque value) => ValidityClaim.Positive(value.As(TorqueUnit.NewtonMeter));
    internal static bool Positive(Duration value) => ValidityClaim.Positive(value.As(DurationUnit.Second));
    internal static bool Nonnegative(Duration value) => double.IsFinite(value.As(DurationUnit.Second)) && value.As(DurationUnit.Second) >= 0.0;
    internal static bool Finite(Angle value) => double.IsFinite(value.As(AngleUnit.Radian));
    internal static bool Positive(Angle value) => ValidityClaim.Positive(value.As(AngleUnit.Radian));
    internal static bool Nonnegative(Angle value) => Finite(value) && value.As(AngleUnit.Radian) >= 0.0;
    internal static bool Fraction(Ratio value) =>
        double.IsFinite(value.As(RatioUnit.DecimalFraction)) && value.As(RatioUnit.DecimalFraction) is > 0.0 and <= 1.0;
    internal static bool AtLeastUnit(Ratio value) =>
        double.IsFinite(value.As(RatioUnit.DecimalFraction)) && value.As(RatioUnit.DecimalFraction) >= 1.0;
}
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
