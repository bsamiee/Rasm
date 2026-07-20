# [RASM_FABRICATION_WORKHOLDING]

`Workholding` owns fixture admission, datum establishment, support, clamping, state transitions, tool access, keep-out conditioning, and restraint evidence. Aggregate admission proves the locating scheme, contact laws, actuation order, stock compatibility, and operation windows once; every interior operation consumes the admitted `Fixture`.

`ExclusionZone`, `Fixture`, `FixtureSet`, and `HoldingReceipt` remain the in-process seam vocabulary. `Workholding.Apply` admits every operation through one `WorkholdingOp` family and emits one `WorkholdingResult` family, while `ContentKey.Of` mints every projected fixture artifact.

## [01]-[INDEX]

- [01]-[ELEMENTS]: locating, support, clamp, contact, and actuation families.
- [02]-[FIXTURE]: aggregate admission, state, keep-outs, and datum evidence.
- [03]-[EVALUATION]: conditioning, clearance, restraint, projection, and receipt folds.

## [02]-[ELEMENTS]

- Owner: `FixtureElement` closes locating, support, and clamping under one element identity and aggregate admission; `ContactLaw` owns friction, pressure, stiffness, deflection, pull-off, and contact-field invariants.
- Cases: locating covers plane, round-pin, diamond-pin, nest, center, mandrel, and optical alignment; support covers fixed, adjustable, hydraulic, compliant, steady-rest, and sacrificial contact; clamping covers toe, vise, chuck, collet, expanding arbor, vacuum, magnetic, adhesive, freeze, center, tailstock, and bed mechanisms.
- Law: element identity, `FixtureRole`, `WorkholdingKind`, and optional `Actuation` ride the `FixtureElement` base constructor, so each case declares classification and loss-of-energy custody once; a case omitting any column fails to compile.
- Policy: `Actuation` carries typed energy source, transmission geometry, fail state, lock, response, and release behavior as data, and `FixtureState` carries which elements and zones are active.
- Growth: a new element mechanism is one generated case supplying its base classification and its contact and body projections; consumers remain exhaustive through generated `Switch`.
- Boundary: locating, support, and clamping remain cases because each contributes a distinct constraint role; template cases survive beside realized elements only because their payload arrives before geometry realization and aggregate admission.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------------------------------------------------------
using LanguageExt;
using LanguageExt.Common;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Numerics.Tensors;
using System.Text;
using CommunityToolkit.HighPerformance.Buffers;
using Rasm.Domain;
using Rasm.Fabrication.Geometry2D;
using Rasm.Fabrication.Process;
using Rasm.Numerics;
using Rhino.Geometry;
using Thinktecture;
using UnitsNet;
using UnitsNet.Units;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Fixturing;

// --- [ELEMENTS] -----------------------------------------------------------------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class WorkholdingKind {
    public static readonly WorkholdingKind Toe = new("toe", HoldingClass.Mechanical);
    public static readonly WorkholdingKind Vise = new("vise", HoldingClass.Mechanical);
    public static readonly WorkholdingKind Chuck = new("chuck", HoldingClass.Revolved);
    public static readonly WorkholdingKind Collet = new("collet", HoldingClass.Revolved);
    public static readonly WorkholdingKind Arbor = new("arbor", HoldingClass.Revolved);
    public static readonly WorkholdingKind Vacuum = new("vacuum", HoldingClass.Vacuum);
    public static readonly WorkholdingKind Magnetic = new("magnetic", HoldingClass.Magnetic);
    public static readonly WorkholdingKind Adhesive = new("adhesive", HoldingClass.Bed);
    public static readonly WorkholdingKind Freeze = new("freeze", HoldingClass.Bed);
    public static readonly WorkholdingKind Center = new("center", HoldingClass.Revolved);
    public static readonly WorkholdingKind Tailstock = new("tailstock", HoldingClass.Revolved);
    public static readonly WorkholdingKind Bed = new("bed", HoldingClass.Bed);

    public HoldingClass Holding { get; }
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

[SmartEnum<string>]
public sealed partial class FixtureRole {
    public static readonly FixtureRole Locate = new("locate");
    public static readonly FixtureRole Support = new("support");
    public static readonly FixtureRole Clamp = new("clamp");
}

[Union]
public abstract partial record Actuation {
    private Actuation() { }

    public sealed record Manual(Torque Torque, Length MeanRadius, Ratio Efficiency, bool SelfLocking) : Actuation;
    public sealed record Spring(Force Force, Length Stroke) : Actuation;
    public sealed record Pneumatic(Pressure Pressure, Area Piston, bool ClampsOnLoss) : Actuation;
    public sealed record Hydraulic(Pressure Pressure, Area Piston, bool AccumulatorHeld) : Actuation;
    public sealed record Electric(Force Force, Length Stroke, bool BrakeHeld) : Actuation;
    public sealed record Field(Force PullOff, Duration Release) : Actuation;

    public Force Preload => new(Switch(
        manual: static row => row.Torque.As(TorqueUnit.NewtonMeter) / row.MeanRadius.As(LengthUnit.Meter)
            * row.Efficiency.As(RatioUnit.DecimalFraction),
        spring: static row => row.Force.As(ForceUnit.Newton),
        pneumatic: static row => row.Pressure.As(PressureUnit.Pascal) * row.Piston.As(AreaUnit.SquareMeter),
        hydraulic: static row => row.Pressure.As(PressureUnit.Pascal) * row.Piston.As(AreaUnit.SquareMeter),
        electric: static row => row.Force.As(ForceUnit.Newton),
        field: static row => row.PullOff.As(ForceUnit.Newton)), ForceUnit.Newton);

    public bool HeldOnLoss => Switch(
        manual: static row => row.SelfLocking,
        spring: static _ => true,
        pneumatic: static row => row.ClampsOnLoss,
        hydraulic: static row => row.AccumulatorHeld,
        electric: static row => row.BrakeHeld,
        field: static _ => false);
}

[ComplexValueObject]
public readonly partial struct ContactLaw {
    public Ratio Friction { get; }
    public Pressure PressureLimit { get; }
    public double NormalStiffnessNPerMm { get; }
    public double TangentialStiffnessNPerMm { get; }
    public Length DeflectionLimit { get; }
    public Force PullOff { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Ratio friction,
        ref Pressure pressureLimit,
        ref double normalStiffnessNPerMm,
        ref double tangentialStiffnessNPerMm,
        ref Length deflectionLimit,
        ref Force pullOff) =>
        validationError = double.IsFinite(friction.As(RatioUnit.DecimalFraction)) && friction.As(RatioUnit.DecimalFraction) >= 0.0
            && double.IsFinite(pressureLimit.As(PressureUnit.Kilopascal)) && pressureLimit.As(PressureUnit.Kilopascal) > 0.0
            && double.IsFinite(normalStiffnessNPerMm) && normalStiffnessNPerMm > 0.0
            && double.IsFinite(tangentialStiffnessNPerMm) && tangentialStiffnessNPerMm > 0.0
            && double.IsFinite(deflectionLimit.As(LengthUnit.Millimeter)) && deflectionLimit.As(LengthUnit.Millimeter) > 0.0
            && double.IsFinite(pullOff.As(ForceUnit.Newton)) && pullOff.As(ForceUnit.Newton) >= 0.0
                ? null
                : new ValidationError(message: "<invalid-contact-law>");
}

public readonly record struct ContactReaction(
    int Element,
    Point3d At,
    Vector3d Normal,
    Force NormalCapacity,
    Force TangentialCapacity,
    Force PullOffCapacity,
    double AreaWeight);

public sealed record ContactPatch(
    int Element,
    Loop Footprint,
    Point3d Center,
    Vector3d Normal,
    ContactLaw Law,
    Force Preload) {
    // Footprint is admitted already lowered, so vertices are the true reaction stations and a
    // bulged pad cannot degenerate to three. Tributary edge length is the weight: uniform
    // weighting gives a corner between two short edges the same reaction as a far corner.
    public Seq<ContactReaction> Field {
        get {
            Seq<Point3d> ring = Footprint.Vertices.ToSeq();
            Seq<double> tributary = ring.Map((point, index) => 0.5 * (
                point.DistanceTo(ring[(index + ring.Count - 1) % ring.Count]) + point.DistanceTo(ring[(index + 1) % ring.Count])));
            double total = tributary.Sum();
            double friction = Law.Friction.As(RatioUnit.DecimalFraction);
            return total <= 1e-12
                ? Seq(new ContactReaction(Element, Center, Normal, Preload, Preload * friction, Law.PullOff, 1.0))
                : ring.Map((point, index) => new ContactReaction(
                    Element, point, Normal, Preload, Preload * friction, Law.PullOff, tributary[index] / total));
        }
    }
}

[Union]
public abstract partial record FixtureElement(
    int Element,
    FixtureRole Role,
    Option<WorkholdingKind> Kind,
    Option<Actuation> Actuator) {
    public bool HeldOnLoss => Actuator.Match(Some: static drive => drive.HeldOnLoss, None: static () => true);

    public sealed record LocatingPlane(int Element, ContactPatch Contact)
        : FixtureElement(Element, FixtureRole.Locate, None, None);
    public sealed record RoundPin(int Element, Point3d Center, Length Radius, Length Height, Seq<ContactPatch> Contacts)
        : FixtureElement(Element, FixtureRole.Locate, None, None);
    public sealed record DiamondPin(int Element, Point3d Center, Vector3d FreeAxis, Length Radius, Length Height, Seq<ContactPatch> Contacts)
        : FixtureElement(Element, FixtureRole.Locate, None, None);
    public sealed record Nest(int Element, Seq<ContactPatch> Contacts)
        : FixtureElement(Element, FixtureRole.Locate, None, None);
    public sealed record LocatingCenter(int Element, Point3d Point, Vector3d Axis, Angle IncludedAngle, ContactPatch Contact)
        : FixtureElement(Element, FixtureRole.Locate, None, None);
    public sealed record Mandrel(int Element, Point3d Center, Vector3d Axis, Length Radius, Length Length, Seq<ContactPatch> Contacts)
        : FixtureElement(Element, FixtureRole.Locate, None, None);
    public sealed record Optical(int Element, Plane Datum, Length Repeatability)
        : FixtureElement(Element, FixtureRole.Locate, None, None);
    public sealed record FixedSupport(int Element, ContactPatch Contact)
        : FixtureElement(Element, FixtureRole.Support, None, None);
    public sealed record AdjustableSupport(int Element, ContactPatch Contact, Length Travel)
        : FixtureElement(Element, FixtureRole.Support, None, None);
    public sealed record HydraulicSupport(int Element, ContactPatch Contact, Pressure EqualizedPressure)
        : FixtureElement(Element, FixtureRole.Support, None, None);
    public sealed record CompliantSupport(int Element, ContactPatch Contact, Length DeflectionLimit)
        : FixtureElement(Element, FixtureRole.Support, None, None);
    public sealed record SteadyRest(int Element, Seq<ContactPatch> Contacts, Length Station)
        : FixtureElement(Element, FixtureRole.Support, None, None);
    public sealed record SacrificialSupport(int Element, ContactPatch Contact, Length RemainingThickness)
        : FixtureElement(Element, FixtureRole.Support, None, None);
    public sealed record ToeClamp(int Element, Loop Body, ContactPatch Contact, Actuation Drive, Length Margin, Length Height)
        : FixtureElement(Element, FixtureRole.Clamp, WorkholdingKind.Toe, Some(Drive));
    public sealed record Vise(int Element, Seq<Loop> Bodies, Seq<ContactPatch> Contacts, Actuation Drive, Length Opening, Length Margin, Length Height)
        : FixtureElement(Element, FixtureRole.Clamp, WorkholdingKind.Vise, Some(Drive));
    public sealed record Chuck(int Element, Seq<Loop> Jaws, Seq<ContactPatch> Contacts, Actuation Drive, Force AxialCapacity, Length Margin, Length Height)
        : FixtureElement(Element, FixtureRole.Clamp, WorkholdingKind.Chuck, Some(Drive));
    public sealed record Collet(int Element, Loop Body, Seq<ContactPatch> Contacts, Actuation Drive, Length Collapse, Length Margin, Length Height)
        : FixtureElement(Element, FixtureRole.Clamp, WorkholdingKind.Collet, Some(Drive));
    public sealed record Arbor(int Element, Loop Body, Seq<ContactPatch> Contacts, Actuation Drive, Length Expansion, Length Margin, Length Height)
        : FixtureElement(Element, FixtureRole.Clamp, WorkholdingKind.Arbor, Some(Drive));
    public sealed record Vacuum(int Element, Loop Bed, Seq<Loop> Leaks, ContactLaw Law, Pressure Pressure, Length Margin, Length Height)
        : FixtureElement(Element, FixtureRole.Clamp, WorkholdingKind.Vacuum, None);
    public sealed record Magnetic(int Element, Loop Pad, ContactLaw Law, Ratio Coupling, Length Margin, Length Height)
        : FixtureElement(Element, FixtureRole.Clamp, WorkholdingKind.Magnetic, None);
    public sealed record Adhesive(int Element, Loop Bond, ContactLaw Law, Ratio Cure, Length Margin, Length Height)
        : FixtureElement(Element, FixtureRole.Clamp, WorkholdingKind.Adhesive, None);
    public sealed record Freeze(int Element, Loop Pad, ContactLaw Law, Ratio Frozen, Length Margin, Length Height)
        : FixtureElement(Element, FixtureRole.Clamp, WorkholdingKind.Freeze, None);
    public sealed record ClampingCenter(int Element, Loop Body, ContactPatch Contact, Actuation Drive, Length Margin, Length Height)
        : FixtureElement(Element, FixtureRole.Clamp, WorkholdingKind.Center, Some(Drive));
    public sealed record Tailstock(int Element, Loop Body, ContactPatch Contact, Actuation Drive, Length Margin, Length Height)
        : FixtureElement(Element, FixtureRole.Clamp, WorkholdingKind.Tailstock, Some(Drive));
    public sealed record Bed(int Element, Loop Contact, ContactLaw Law, Pressure Pressure, Length Height)
        : FixtureElement(Element, FixtureRole.Clamp, WorkholdingKind.Bed, None);
}

[Union]
public abstract partial record ClampTemplate {
    private ClampTemplate() { }

    public sealed record BoundaryToe(ContactLaw Law, Actuation Drive, Length Width, Length Depth, Length Height, Length Margin) : ClampTemplate;
    public sealed record OpposedVise(ContactLaw Law, Actuation Drive, Length JawDepth, Length JawWidth, Length Height, Length Margin) : ClampTemplate;
    public sealed record VacuumField(ContactLaw Law, Pressure Pressure, Length Height, Length Margin) : ClampTemplate;
    public sealed record SoftJaw(ContactLaw Law, Actuation Drive, Length BlankDepth, Length Clearance, Length Height, Length Margin) : ClampTemplate;

    internal Fin<(Seq<FixtureElement> Elements, Option<SoftJawInsert> Insert)> Generate(Loop part, int samples, int firstElement) =>
        Switch(
            state: (Part: part, Samples: samples, First: firstElement),
            boundaryToe: static (state, row) => Sample(state.Part, state.Samples).Traverse((point, index) =>
                Box(point, row.Width.As(LengthUnit.Millimeter), row.Depth.As(LengthUnit.Millimeter), state.Part.Tolerance).Map(body => {
                    ContactPatch contact = new(state.First + index, body, body.Bound().Center, -Vector3d.ZAxis, row.Law, row.Drive.Preload);
                    return (FixtureElement)new FixtureElement.ToeClamp(state.First + index, body, contact, row.Drive, row.Margin, row.Height);
                }).ToValidation()).As().ToFin().Map(static clamps => (clamps, Option<SoftJawInsert>.None)),
            opposedVise: static (state, row) => Vise(state.Part, state.First, row).Map(clamp => (Seq(clamp), Option<SoftJawInsert>.None)),
            vacuumField: static (state, row) => Fin.Succ((Seq<FixtureElement>(new FixtureElement.Vacuum(
                state.First, state.Part, Seq<Loop>(), row.Law, row.Pressure, row.Margin, row.Height)), Option<SoftJawInsert>.None)),
            softJaw: static (state, row) => Offset(Seq(state.Part), row.Clearance.As(LengthUnit.Millimeter)).Bind(negative =>
                Vise(state.Part, state.First, new OpposedVise(row.Law, row.Drive, row.BlankDepth, row.BlankDepth, row.Height, row.Margin))
                    .Map(clamp => (Seq<FixtureElement>(clamp), Some(new SoftJawInsert(clamp.Bodies, negative, row.Clearance))))));

    static Fin<FixtureElement.Vise> Vise(Loop part, int element, OpposedVise row) {
        BoundingBox bound = part.Bound();
        double depth = row.JawDepth.As(LengthUnit.Millimeter);
        double width = row.JawWidth.As(LengthUnit.Millimeter);
        Point3d left = new(bound.Min.X - (0.5 * depth), bound.Center.Y, 0.0);
        Point3d right = new(bound.Max.X + (0.5 * depth), bound.Center.Y, 0.0);
        return (Box(left, depth, width, part.Tolerance).ToValidation(),
                Box(right, depth, width, part.Tolerance).ToValidation())
            .Apply((first, second) => {
                Seq<Loop> bodies = Seq(first, second);
                Seq<ContactPatch> contacts = Seq(
                    new ContactPatch(element, first, first.Bound().Center, Vector3d.XAxis, row.Law, row.Drive.Preload / 2.0),
                    new ContactPatch(element, second, second.Bound().Center, -Vector3d.XAxis, row.Law, row.Drive.Preload / 2.0));
                return new FixtureElement.Vise(element, bodies, contacts, row.Drive,
                    Length.FromMillimeters(bound.Diagonal.X), row.Margin, row.Height);
            }).As().ToFin();
    }

    static Seq<Point3d> Sample(Loop part, int count) =>
        toSeq(Enumerable.Range(0, count)).Map(index => part.At(index * Math.Max(1, part.Count / count)));

    static Fin<Loop> Box(Point3d center, double width, double depth, Context tolerance) {
        double halfWidth = 0.5 * width;
        double halfDepth = 0.5 * depth;
        return Loop.Admit(Arr(
            new Point3d(center.X - halfWidth, center.Y - halfDepth, center.Z),
            new Point3d(center.X + halfWidth, center.Y - halfDepth, center.Z),
            new Point3d(center.X + halfWidth, center.Y + halfDepth, center.Z),
            new Point3d(center.X - halfWidth, center.Y + halfDepth, center.Z)), closed: true, Arr<double>(), tolerance);
    }
}

public sealed record SoftJawInsert(Seq<Loop> Blanks, Seq<Loop> Negative, Length Clearance);

public sealed record FixtureSynthesis(
    FixtureSpec Basis,
    Loop Part,
    Seq<ClampTemplate> Templates,
    Seq<LoadCase> Loads,
    Seq<ToolCorridor> Corridors,
    int Samples,
    int MinimumTemplates,
    int MaximumTemplates,
    int CandidateBudget,
    Ratio SafetyFactor,
    FixtureObjective Objective);

[ComplexValueObject]
public readonly partial struct FixtureObjective {
    public double Holding { get; }
    public double Access { get; }
    public double Simplicity { get; }
    public double Total => Holding + Access + Simplicity;

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref double holding,
        ref double access,
        ref double simplicity) =>
        validationError = double.IsFinite(holding) && holding >= 0.0
            && double.IsFinite(access) && access >= 0.0
            && double.IsFinite(simplicity) && simplicity >= 0.0
            && double.IsFinite(holding + access + simplicity)
            && holding + access + simplicity > 0.0
                ? null
                : new ValidationError(message: "<invalid-fixture-objective>");
}

public readonly record struct FixtureScore(double Holding, double Access, double Simplicity, double Total);

public sealed record FixtureCandidate(
    Fixture Fixture,
    HoldingReceipt Holding,
    Seq<WorkholdingResult.Clearance> Clearance,
    Seq<SoftJawInsert> SoftJaws,
    FixtureScore Score);
```

## [03]-[FIXTURE]

- Owner: `FixtureSpec` is raw aggregate ingress; `Fixture` is the admitted owner carrying datum lineage, element identity, activation sequence, exact keep-outs, motion partition, and stock witness.
- Admission: independent element, topology, state, geometry, contact, and six-degree constraint failures accumulate before the `Fin<Fixture>` rail resumes.
- State: each `FixtureStep` activates and releases elements against one `FixtureState`; cutting states require settled location, support, and clamp custody before any move conditions.
- Keep-out: `ExclusionZone` carries lower and upper height, active states, exact loops, once-densified walls, source element, role, and mechanism; every physical element contributes a zone, while optical alignment remains geometry-free.
- Datum: `DatumFrame` records primary, secondary, and tertiary contact evidence with the work coordinate system transform and repeatability budget.
- Constraint: `ConstraintReceipt` preserves both closure ranks and redundancy — `Rank` over the friction-cone wrench set, `Frictionless` over normals alone — so underconstraint, overconstraint, and the form-versus-force closure distinction remain separable from holding-force sufficiency.
- Fault: `FixturingWitness` closes the admission rejection reasons and lowers through one `FabricationFault.FixtureInadmissible` band arm; degenerate geometry stays on `GeometryFault.DegenerateInput`.
- Growth: a fixture-wide invariant becomes one aggregate gate; no consumer revalidates element fields or reconstructs datum lineage.
- Exemption: `ConstraintRank` is the bounded six-column contact-wrench kernel; its span loops are the measured numeric exemption.

```csharp signature
// --- [FIXTURE] ------------------------------------------------------------------------------------------------------------------------------------
[ValueObject<int>]
public readonly partial struct FixtureStage {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) =>
        validationError = value >= 0 ? null : new ValidationError(message: "<invalid-fixture-stage>");
}

public readonly record struct FixtureStep(FixtureStage Stage, FixtureState State, Arr<int> Activate, Arr<int> Release, Duration Settle);

public readonly record struct DatumFrame(
    Plane Work,
    Arr<int> Primary,
    Arr<int> Secondary,
    Arr<int> Tertiary,
    Length Repeatability);

public sealed record ExclusionZone(
    int Operation,
    int Element,
    FixtureRole Role,
    Option<WorkholdingKind> Kind,
    Seq<Loop> Keepouts,
    Seq<Loop> Walls,
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

    public CollisionZone Collision => CollisionZone.Create(
        ContentKey.Of(EgressKind.Plan, Workholding.ZoneIdentity(this)), Bounds);

    public bool Covers(Point3d point, FixtureState state) =>
        Active.Contains(state) && point.Z >= Lower.As(LengthUnit.Millimeter) && point.Z <= Upper.As(LengthUnit.Millimeter)
        && Keepouts.Sum(loop => !loop.Covers(point) ? 0 : loop.Winding() switch {
            Sign.Positive => 1,
            Sign.Negative => -1,
            _ => 0,
        }) != 0;

    public bool Crosses(Edge3 segment, FixtureState state) =>
        Active.Contains(state) && Workholding.Below(segment, Lower.As(LengthUnit.Millimeter), Upper.As(LengthUnit.Millimeter)).Exists(part =>
            Covers(part.A, state) || Covers(part.B, state) || Covers(part.A + (0.5 * (part.B - part.A)), state)
            || Walls.Exists(wall => Workholding.Crosses(part, wall)));
}

public readonly record struct MoveRun(int Loop, int Start, int Count);

public readonly record struct CorridorStation(Point3d Point, Length Cutter, Length Holder, Length Chip, Length Coolant);

[SmartEnum<string>]
public sealed partial class CorridorKind {
    public static readonly CorridorKind Tool = new("tool", static station => Math.Max(station.Cutter.As(LengthUnit.Millimeter), station.Holder.As(LengthUnit.Millimeter)));
    public static readonly CorridorKind Chip = new("chip", static station => station.Chip.As(LengthUnit.Millimeter));
    public static readonly CorridorKind Coolant = new("coolant", static station => station.Coolant.As(LengthUnit.Millimeter));

    public Func<CorridorStation, double> RadiusMm { get; }
}

public sealed record ToolCorridor(CorridorKind Kind, Seq<CorridorStation> Stations);

[Union]
public abstract partial record FixturingWitness {
    private FixturingWitness() { }

    public sealed record Absent : FixturingWitness;
    public sealed record Aggregate(int Operation, int Elements, int Duplicates) : FixturingWitness;
    public sealed record Element(int Element, FixtureRole Role, string Axis) : FixturingWitness;
    public sealed record Lifecycle(Option<FixtureStep> Step, Option<FixtureRole> Uncovered, int Steps, int Active) : FixturingWitness;
    public sealed record Datum(int Primary, int Secondary, int Tertiary, int Unlocated) : FixturingWitness;
    public sealed record Closure(int Rank, int Required, int Redundancy, int Frictionless) : FixturingWitness;
    public sealed record Restraint(int Loads, int Invalid, double Safety) : FixturingWitness;
    public sealed record Corridor(CorridorKind Kind, int Stations) : FixturingWitness;
    public sealed record Partition(int Runs, int Moves) : FixturingWitness;
    public sealed record Synthesis(int Templates, int Minimum, int Maximum, int Budget) : FixturingWitness;
    public sealed record Plan(int Operations, int Machines, int Fixtures, int MaxSetups) : FixturingWitness;
    public sealed record Operation(int Key, string Axis) : FixturingWitness;
    public sealed record Offsets(int Requested, int Available, int MaxSetups) : FixturingWitness;
    public sealed record Roster(CarrierKey Carrier, int Station, int Instances) : FixturingWitness;
    public sealed record Rebase(int Setup, Length Correction, Angle Rotation, Length Tolerance) : FixturingWitness;
    public sealed record Membership(int Joint, int Components, int Resolved) : FixturingWitness;
    public sealed record Join(int Joint, JoinRejection Reason) : FixturingWitness;
    public sealed record Residual(int Completed, int Blocked, int Joints) : FixturingWitness;
}

public abstract partial record FabricationFault {
    public sealed record FixtureInadmissible(FixturingWitness Witness)
        : FabricationFault(53, "fabrication:fixture-inadmissible", FabConcern.Fixturing);
}

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

public sealed record FixtureSpec(
    int Operation,
    Seq<FixtureElement> Elements,
    Seq<FixtureStep> Sequence,
    DatumFrame Datum,
    Arr<Loop> Profiles,
    Seq<MoveRun> Runs,
    Point3d InitialCursor,
    Option<StockSnapshot> Current,
    Length ArcChordError);

public sealed record Fixture {
    private Fixture(FixtureSpec spec, Seq<ExclusionZone> zones, Seq<ContactPatch> contacts, ConstraintReceipt constraint) =>
        (Spec, Zones, Contacts, Constraint) = (spec, zones, contacts, constraint);

    public FixtureSpec Spec { get; }
    public Seq<ExclusionZone> Zones { get; }
    public Seq<ContactPatch> Contacts { get; }
    public ConstraintReceipt Constraint { get; }
    public int Operation => Spec.Operation;
    public Point3d InitialCursor => Spec.InitialCursor;

    public static Fin<Fixture> Admit(FixtureSpec? candidate) =>
        Optional(candidate).ToFin(new FabricationFault.FixtureInadmissible(new FixturingWitness.Absent()).ToError())
            .Bind(Workholding.GateMembers)
            .Bind(spec =>
        (Workholding.GateSpec(spec), Workholding.GateSequence(spec), Workholding.GateDatum(spec), Workholding.Zones(spec), Workholding.Contacts(spec))
            .Apply(static (accepted, _, _, zones, contacts) => (accepted, zones, contacts))
            .As()
            .ToFin()
            .Bind(static row => row.contacts.Count > 0
                ? Fin.Succ(row)
                : Fin.Fail<(FixtureSpec accepted, Seq<ExclusionZone> zones, Seq<ContactPatch> contacts)>(
                    new FabricationFault.FixtureInadmissible(new FixturingWitness.Aggregate(row.accepted.Operation, row.accepted.Elements.Count, 0)).ToError()))
            .Bind(static row => Workholding.Constraint(row.contacts).Bind(constraint => constraint.Constrained
                ? Fin.Succ(new Fixture(row.accepted, row.zones, row.contacts, constraint))
                : Fin.Fail<Fixture>(new FabricationFault.FixtureInadmissible(
                    new FixturingWitness.Closure(constraint.Rank, 6, constraint.Redundancy, constraint.Frictionless)).ToError())))
            .Bind(static fixture => fixture.Spec.Current.Match(
                Some: stock => Workholding.Machined(fixture, stock).Bind(hit => hit.Match(
                    Some: point => Fin.Fail<Fixture>(FabricationFault.ClampOnMachinedFace(fixture.Operation, point).ToError()),
                    None: () => Fin.Succ(fixture))),
                None: () => Fin.Succ(fixture))));
}

public readonly record struct Wrench(Vector3d Force, Vector3d Moment) {
    public static Wrench Of(Point3d at, Vector3d direction) =>
        new(direction, Vector3d.CrossProduct(Workholding.Meters(at - Point3d.Origin), direction));
}

public sealed record ConstraintReceipt(int Rank, int Frictionless, int Redundancy, Seq<ContactReaction> Reactions) {
    public bool Constrained => Rank == 6;
    public bool FormClosed => Frictionless == 6;
    public bool Determinate => Constrained && Redundancy == 0;
}

[ComplexValueObject]
public sealed partial class FixtureSet {
    public Seq<Fixture> Fixtures { get; }

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Seq<Fixture> fixtures) =>
        validationError = fixtures.Count > 0 && fixtures.ForAll(static fixture => fixture is not null)
            && fixtures.Map(static fixture => fixture.Operation).Distinct().Count == fixtures.Count
                ? null
                : new ValidationError(message: "<invalid-fixture-set>");

    public HashMap<int, Fixture> ByOperation => Fixtures.Fold(
        HashMap<int, Fixture>(),
        static (index, fixture) => index.Add(fixture.Operation, fixture));
}
```

## [04]-[EVALUATION]

- Owner: `WorkholdingOp` closes admission, conditioning, clearance, machined-stock, restraint, state, and projection modalities; `WorkholdingResult` closes their typed outcomes.
- Entry: `Workholding.Apply` is the sole public operation surface; each case carries every discriminant and parameter its arm consumes.
- Loads: `LoadCase` covers cutting, gravity, acceleration, probing, handling, thermal, and process-pressure demand; regional cases transfer their resultant through the region center, and each cutting operation matches the admitted fixture operation.
- Stability: one scale-normalized six-DOF reaction solve resolves force, moment, friction, pull-off, and lift together; contact pressure and elastic limits derive beside its reaction distribution, and `TipMargin` resolves each support-polygon edge against its own restoring reactions.
- Access: tool-corridor clearance uses every cutter and holder station; chip and coolant corridors use the same zone algebra with their own radius and active state. `Clearance` carries the blocking zone alone and derives clearness from its absence.
- Synthesis: `ClampTemplate` generates boundary layouts, evaluates every candidate through the same restraint and corridor algebra, ranks the survivors, and derives soft-jaw negatives from the part silhouette. `Programs` enumerates only the admitted cardinalities under `CandidateBudget`, so template-roster growth never costs a powerset.
- Projection: `FixtureProjection` selects machine, setup-sheet, inspection, and evidence payloads; `Canonical` dispatches on that family and writes every field of the selected payload before `ContentKey.Of` mints its identity.
- Exemption: `Canonical`, `ZoneIdentity`, `Frame`, and the typed `Write` overloads are the boundary serialization kernel; every union case, optional value, string, and adjacent collection is framed, and `ZoneIdentity` reuses the complete zone writer.
- Exemption: `GateSequence` and `GateDatum` are bounded aggregate-admission folds; statements remain inside those validation kernels.
- Exemption: `Rank`, `Hull`, and `Chain` are the bounded synthesis-scoring and support-polygon kernels; statements remain inside those numeric bodies.
- Exemption: `ContactPatch.Field`, `Support`, `TangentialSupport`, `NormalSupport`, `MomentSupport`, and `TipMargin` are measured contact kernels; statements remain inside those bounded numeric bodies.
- Exemption: `Zone`, `Below`, `Crosses`, `Intersection`, and `ArcSegments` are measured geometry kernels; statements remain inside those bounded numeric bodies.
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

[Union]
public abstract partial record LoadCase {
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

    private static (Vector3d Force, Vector3d Moment, Point3d At) Regional(
        Vector3d force,
        Vector3d moment,
        Point3d reference,
        Loop region) => (
            force,
            moment + Vector3d.CrossProduct(Workholding.Meters(region.Bound().Center - reference), force),
            reference);
}

[ComplexValueObject]
public readonly partial struct ForceVector {
    public Vector3d Direction { get; }
    public Force Magnitude { get; }
    public Vector3d Vector => Direction / Direction.Length * Magnitude.As(ForceUnit.Newton);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Vector3d direction, ref Force magnitude) =>
        validationError = double.IsFinite(direction.X) && double.IsFinite(direction.Y) && double.IsFinite(direction.Z) && direction.Length > 1e-9
            && double.IsFinite(magnitude.As(ForceUnit.Newton)) && magnitude.As(ForceUnit.Newton) >= 0.0
                ? null
                : new ValidationError(message: "<invalid-force-vector>");
}

[ComplexValueObject]
public readonly partial struct MomentVector {
    public Vector3d Axis { get; }
    public Torque Magnitude { get; }
    public Vector3d Vector => Axis / Axis.Length * Magnitude.As(TorqueUnit.NewtonMeter);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Vector3d axis, ref Torque magnitude) =>
        validationError = double.IsFinite(axis.X) && double.IsFinite(axis.Y) && double.IsFinite(axis.Z) && axis.Length > 1e-9
            && double.IsFinite(magnitude.As(TorqueUnit.NewtonMeter)) && magnitude.As(TorqueUnit.NewtonMeter) >= 0.0
                ? null
                : new ValidationError(message: "<invalid-moment-vector>");
}

public readonly record struct AxisMargin(Vector3d Capacity, Vector3d Demand) {
    public double Minimum => Seq(
        Demand.X > 0.0 ? Capacity.X / Demand.X : double.PositiveInfinity,
        Demand.Y > 0.0 ? Capacity.Y / Demand.Y : double.PositiveInfinity,
        Demand.Z > 0.0 ? Capacity.Z / Demand.Z : double.PositiveInfinity).Min();
}

public readonly record struct LoadReceipt(
    LoadCase Load,
    AxisMargin Force,
    AxisMargin Moment,
    double FrictionMargin,
    double PullOffMargin,
    double PressureMargin,
    double DeflectionMargin,
    double TangentialDeflectionMargin,
    double TipMargin,
    double EquilibriumMargin,
    double LiftMargin,
    Seq<ContactReaction> Reactions,
    Seq<Vector3d> SolvedReactions) {
    public double Minimum => Seq(Force.Minimum, Moment.Minimum, FrictionMargin, PullOffMargin, PressureMargin, DeflectionMargin, TangentialDeflectionMargin,
        TipMargin, EquilibriumMargin, LiftMargin).Min();
}

public sealed record HoldingReceipt(Seq<LoadReceipt> Loads, Seq<ContactPatch> Contacts) {
    public double MinimumMargin => Loads.Map(static receipt => receipt.Minimum).Min();
    public bool Holds => MinimumMargin >= 1.0;
}

[Union]
public abstract partial record FixtureArtifact {
    private FixtureArtifact() { }

    public sealed record Machine(ContentKey Key, Seq<ExclusionZone> Zones, DatumFrame Datum, ConstraintReceipt Constraint) : FixtureArtifact;
    public sealed record SetupSheet(ContentKey Key, Seq<FixtureElement> Elements, Seq<FixtureStep> Sequence, DatumFrame Datum, ConstraintReceipt Constraint) : FixtureArtifact;
    public sealed record Inspection(ContentKey Key, Seq<ContactPatch> Contacts, DatumFrame Datum, ConstraintReceipt Constraint) : FixtureArtifact;
    public sealed record Evidence(ContentKey Key, Fixture Fixture) : FixtureArtifact;
}

[Union]
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

[Union]
public abstract partial record WorkholdingResult {
    private WorkholdingResult() { }

    public sealed record Admitted(Fixture Fixture) : WorkholdingResult;
    public sealed record Conditioned(Seq<Move> Moves) : WorkholdingResult;
    public sealed record Clearance(Option<ExclusionZone> Blocked) : WorkholdingResult {
        public bool Clear => Blocked.IsNone;
    }
    public sealed record MachinedHit(Option<Point3d> Point) : WorkholdingResult;
    public sealed record Restrained(HoldingReceipt Receipt) : WorkholdingResult;
    public sealed record Transitioned(FixtureState State, Arr<int> Active) : WorkholdingResult;
    public sealed record Synthesized(Seq<FixtureCandidate> Candidates) : WorkholdingResult;
    public sealed record Projected(FixtureArtifact Artifact) : WorkholdingResult;
}

public static class Workholding {
    public static Fin<WorkholdingResult> Apply(WorkholdingOp? candidate) =>
        Optional(candidate).ToFin(new FabricationFault.FixtureInadmissible(new FixturingWitness.Absent()).ToError())
            .Bind(static op => op.Switch(
                admit: static row => Fixture.Admit(row.Spec).Map<WorkholdingResult>(static fixture => new WorkholdingResult.Admitted(fixture)),
                condition: static row => Condition(row.Fixture, row.State, row.Moves)
                    .Map<WorkholdingResult>(static moves => new WorkholdingResult.Conditioned(moves)),
                clear: static row => Clear(row.Fixture, row.State, row.Corridor)
                    .Map<WorkholdingResult>(static blocked => new WorkholdingResult.Clearance(blocked)),
                machined: static row => Machined(row.Fixture, row.Stock)
                    .Map<WorkholdingResult>(static point => new WorkholdingResult.MachinedHit(point)),
                restrain: static row => Restrain(row.Fixture, row.Loads, row.SafetyFactor.As(RatioUnit.DecimalFraction))
                    .Map<WorkholdingResult>(static receipt => new WorkholdingResult.Restrained(receipt)),
                transition: static row => Transition(row.Fixture, row.From, row.To)
                    .Map<WorkholdingResult>(static receipt => new WorkholdingResult.Transitioned(receipt.State, receipt.Active)),
                synthesize: static row => Synthesize(row.Seed)
                    .Map<WorkholdingResult>(static candidates => new WorkholdingResult.Synthesized(candidates)),
                project: static row => Project(row.Fixture, row.Projection)
                    .Map<WorkholdingResult>(static artifact => new WorkholdingResult.Projected(artifact))));

    internal static K<Validation<Error>, FixtureSpec> GateSpec(FixtureSpec spec) =>
        spec is not null && spec.Operation >= 0
        && spec.ArcChordError.As(LengthUnit.Millimeter) > 0.0 && double.IsFinite(spec.ArcChordError.As(LengthUnit.Millimeter))
        && Finite(spec.InitialCursor) && spec.Profiles.ForAll(Profile) && ValidRuns(spec.Profiles, spec.Runs)
        && spec.Elements.Count > 0 && spec.Elements.ForAll(static element => element is not null)
        && spec.Elements.Map(static element => element.Element).Distinct().Count == spec.Elements.Count
        && spec.Elements.ForAll(ValidElement)
            ? Validation<Error, FixtureSpec>.Success(spec)
            : Validation<Error, FixtureSpec>.Fail(new FabricationFault.FixtureInadmissible(new FixturingWitness.Aggregate(
                spec.Operation,
                spec.Elements.Count,
                spec.Elements.Count - spec.Elements.Map(static element => element.Element).Distinct().Count)).ToError());

    // Member presence gates above the applicative fan, so the four accumulating gates
    // read proven members instead of each re-deriving the same null guard.
    internal static Fin<FixtureSpec> GateMembers(FixtureSpec spec) =>
        spec.Elements.ForAll(static element => element is not null)
        && spec.Sequence.ForAll(static step => step.State is not null)
            ? Fin.Succ(spec)
            : Fin.Fail<FixtureSpec>(new FabricationFault.FixtureInadmissible(
                new FixturingWitness.Aggregate(spec.Operation, spec.Elements.Count, 0)).ToError());

    internal static K<Validation<Error>, Unit> GateSequence(FixtureSpec spec) {
        HashMap<int, FixtureRole> roles = spec.Elements.Fold(HashMap<int, FixtureRole>(), static (map, element) => map.Add(element.Element, element.Role));
        HashMap<int, bool> held = spec.Elements.Fold(HashMap<int, bool>(), static (map, element) => map.Add(element.Element, element.HeldOnLoss));
        Seq<FixtureRole> custody = Seq(FixtureRole.Locate, FixtureRole.Support, FixtureRole.Clamp);
        (Set<int> Active, Option<(FixtureStep Step, Option<FixtureRole> Uncovered)> Broken) lifecycle = spec.Sequence.Fold(
            (Active: Set<int>(), Broken: Option<(FixtureStep, Option<FixtureRole>)>.None),
            (state, step) => {
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
                return (next, state.Broken | (ordered && uncovered.IsNone ? None : Some((step, uncovered))));
            });
        return lifecycle.Broken.IsNone
            && spec.Sequence.Count > 0
            && spec.Sequence.Map(static step => step.Stage).Distinct().Count == spec.Sequence.Count
            && spec.Sequence.ForAll(step => step.Settle.As(DurationUnit.Second) >= 0.0 && double.IsFinite(step.Settle.As(DurationUnit.Second)))
            && spec.Elements.ForAll(element => spec.Sequence.Exists(step => step.Activate.Contains(element.Element)))
                ? Validation<Error, Unit>.Success(unit)
                : Validation<Error, Unit>.Fail(new FabricationFault.FixtureInadmissible(new FixturingWitness.Lifecycle(
                    lifecycle.Broken.Map(static broken => broken.Step),
                    lifecycle.Broken.Bind(static broken => broken.Uncovered),
                    spec.Sequence.Count,
                    lifecycle.Active.Count)).ToError());
    }

    internal static K<Validation<Error>, Unit> GateDatum(FixtureSpec spec) {
        Set<int> locators = spec.Elements.Filter(static element => element.Role == FixtureRole.Locate).Map(static element => element.Element).ToSet();
        Set<int> datum = spec.Datum.Primary.Concat(spec.Datum.Secondary).Concat(spec.Datum.Tertiary).ToSet();
        bool disjoint = datum.Count == spec.Datum.Primary.Count + spec.Datum.Secondary.Count + spec.Datum.Tertiary.Count;
        return
        spec.Datum.Primary.Count >= 1 && spec.Datum.Secondary.Count >= 1 && spec.Datum.Tertiary.Count >= 1
        && disjoint && datum.ForAll(locators.Contains)
        && spec.Datum.Work.IsValid
        && spec.Datum.Repeatability.As(LengthUnit.Millimeter) >= 0.0 && double.IsFinite(spec.Datum.Repeatability.As(LengthUnit.Millimeter))
            ? Validation<Error, Unit>.Success(unit)
            : Validation<Error, Unit>.Fail(new FabricationFault.FixtureInadmissible(new FixturingWitness.Datum(
                spec.Datum.Primary.Count, spec.Datum.Secondary.Count, spec.Datum.Tertiary.Count,
                datum.Count(element => !locators.Contains(element)))).ToError());
    }

    internal static K<Validation<Error>, Seq<ExclusionZone>> Zones(FixtureSpec spec) =>
        spec.Elements.Traverse(element =>
            Zone(spec.Operation, element, spec.Sequence, spec.ArcChordError.As(LengthUnit.Millimeter)).ToValidation())
            .Map(static rows => rows.Choose(identity));

    internal static K<Validation<Error>, Seq<ContactPatch>> Contacts(FixtureSpec spec) =>
        spec.Elements.Traverse(element => ElementContacts(element).ToValidation()).Map(static rows => rows.Bind(identity));

    internal static Fin<ConstraintReceipt> Constraint(Seq<ContactPatch> contacts) {
        Seq<ContactReaction> reactions = contacts.Bind(static contact => contact.Field);
        Seq<Wrench> normal = reactions.Map(static reaction => Wrench.Of(reaction.At, Unitized(reaction.Normal)));
        Seq<Wrench> closure = normal + reactions.Bind(Friction);
        double characteristicLength = CharacteristicLength(reactions);
        double relativeTolerance = Math.Sqrt(double.BitIncrement(1.0) - 1.0);
        int rank = ConstraintRank(closure, characteristicLength, relativeTolerance);
        return Fin.Succ(new ConstraintReceipt(rank, ConstraintRank(normal, characteristicLength, relativeTolerance),
            Math.Max(0, closure.Count - rank), reactions));
    }

    // A frictional contact spans three wrench directions, not one: dropping the tangential pair
    // caps an opposed-jaw fixture at rank 3 and reports every valid vise as underconstrained.
    static Seq<Wrench> Friction(ContactReaction reaction) {
        if (reaction.TangentialCapacity.As(ForceUnit.Newton) <= 0.0) return Seq<Wrench>();
        Vector3d normal = Unitized(reaction.Normal);
        Vector3d first = Unitized(Vector3d.CrossProduct(normal, Math.Abs(normal.Z) < 0.9 ? Vector3d.ZAxis : Vector3d.XAxis));
        return Seq(Wrench.Of(reaction.At, first), Wrench.Of(reaction.At, Unitized(Vector3d.CrossProduct(normal, first))));
    }

    static double CharacteristicLength(Seq<ContactReaction> reactions) => Math.Max(
        Math.Sqrt(double.BitIncrement(1.0) - 1.0),
        reactions.Bind(first => reactions.Map(second => Meters(first.At - second.At).Length)).Max());

    static int ConstraintRank(Seq<Wrench> wrenches, double characteristicLength, double relativeTolerance) {
        double[,] basis = new double[6, 6];
        int rank = 0;
        double scale = wrenches.Map(wrench => Math.Sqrt(
            (wrench.Force * wrench.Force) + ((wrench.Moment * wrench.Moment) / (characteristicLength * characteristicLength)))).Max();
        Span<double> row = stackalloc double[6];
        foreach (Wrench wrench in wrenches) {
            row[0] = wrench.Force.X; row[1] = wrench.Force.Y; row[2] = wrench.Force.Z;
            row[3] = wrench.Moment.X / characteristicLength;
            row[4] = wrench.Moment.Y / characteristicLength;
            row[5] = wrench.Moment.Z / characteristicLength;
            for (int held = 0; held < rank; held++) {
                double dot = 0.0;
                for (int column = 0; column < 6; column++) dot += row[column] * basis[held, column];
                for (int column = 0; column < 6; column++) row[column] -= dot * basis[held, column];
            }
            double squared = 0.0;
            for (int column = 0; column < 6; column++) squared += row[column] * row[column];
            double norm = Math.Sqrt(squared);
            if (norm <= relativeTolerance * scale) continue;
            for (int column = 0; column < 6; column++) basis[rank, column] = row[column] / norm;
            if (++rank == 6) break;
        }
        return rank;
    }

    static Fin<Option<ExclusionZone>> Zone(int operation, FixtureElement element, Seq<FixtureStep> sequence, double chord) =>
        ElementGeometry(element, chord).Bind(shape => shape.Match(
            Some: row => Offset(row.Loops, row.Margin).Bind(keepouts => keepouts.Count > 0
                ? keepouts.Traverse(loop => Lower(loop, chord).ToValidation()).As().ToFin().Map(walls => {
                    Seq<Point3d> points = keepouts.Bind(static loop => loop.Vertices);
                    double lower = points.Map(static point => point.Z).Min();
                    double upper = points.Map(static point => point.Z).Max() + Math.Max(row.Height, chord);
                    Set<FixtureState> active = sequence.Fold((Active: false, States: Set<FixtureState>()), (state, step) => {
                        bool held = (state.Active || step.Activate.Contains(element.Element)) && !step.Release.Contains(element.Element);
                        return (held, held ? state.States.Add(step.State) : state.States);
                    }).States;
                    return Some(new ExclusionZone(operation, element.Element, element.Role, element.Kind, keepouts, walls,
                        Length.FromMillimeters(lower), Length.FromMillimeters(upper), active, Length.FromMillimeters(chord)));
                })
                : Fin.Fail<Option<ExclusionZone>>(new GeometryFault.DegenerateInput(Kind.Polyline, element.Element, nameof(ExclusionZone)).ToError())),
            None: static () => Fin.Succ(Option<ExclusionZone>.None)));

    static Fin<Option<(Seq<Loop> Loops, double Margin, double Height)>> ElementGeometry(FixtureElement element, double chord) =>
        element.Switch(
            state: chord,
            locatingPlane: static (state, row) => Shape(Seq(row.Contact), 0.0, state),
            roundPin: static (_, row) => Shape(row.Contacts, 0.0, row.Height.As(LengthUnit.Millimeter)),
            diamondPin: static (_, row) => Shape(row.Contacts, 0.0, row.Height.As(LengthUnit.Millimeter)),
            nest: static (state, row) => Shape(row.Contacts, 0.0, state),
            locatingCenter: static (state, row) => Shape(Seq(row.Contact), 0.0, state),
            mandrel: static (_, row) => Shape(row.Contacts, 0.0, row.Length.As(LengthUnit.Millimeter)),
            optical: static (_, _) => Fin.Succ(Option<(Seq<Loop>, double, double)>.None),
            fixedSupport: static (state, row) => Shape(Seq(row.Contact), 0.0, state),
            adjustableSupport: static (_, row) => Shape(Seq(row.Contact), 0.0, row.Travel.As(LengthUnit.Millimeter)),
            hydraulicSupport: static (state, row) => Shape(Seq(row.Contact), 0.0, state),
            compliantSupport: static (_, row) => Shape(Seq(row.Contact), 0.0, row.DeflectionLimit.As(LengthUnit.Millimeter)),
            steadyRest: static (state, row) => Shape(row.Contacts, 0.0, state),
            sacrificialSupport: static (_, row) => Shape(Seq(row.Contact), 0.0, row.RemainingThickness.As(LengthUnit.Millimeter)),
            toeClamp: static (_, row) => Fin.Succ(Some((Seq(row.Body), row.Margin.As(LengthUnit.Millimeter), row.Height.As(LengthUnit.Millimeter)))),
            vise: static (_, row) => Fin.Succ(Some((row.Bodies, row.Margin.As(LengthUnit.Millimeter), row.Height.As(LengthUnit.Millimeter)))),
            chuck: static (_, row) => Fin.Succ(Some((row.Jaws, row.Margin.As(LengthUnit.Millimeter), row.Height.As(LengthUnit.Millimeter)))),
            collet: static (_, row) => Fin.Succ(Some((Seq(row.Body), row.Margin.As(LengthUnit.Millimeter), row.Height.As(LengthUnit.Millimeter)))),
            arbor: static (_, row) => Fin.Succ(Some((Seq(row.Body), row.Margin.As(LengthUnit.Millimeter), row.Height.As(LengthUnit.Millimeter)))),
            vacuum: static (_, row) => Boolean(Seq(row.Bed), row.Leaks, BoolKind.Not).Map(loops => Some((loops, row.Margin.As(LengthUnit.Millimeter), row.Height.As(LengthUnit.Millimeter)))),
            magnetic: static (_, row) => Fin.Succ(Some((Seq(row.Pad), row.Margin.As(LengthUnit.Millimeter), row.Height.As(LengthUnit.Millimeter)))),
            adhesive: static (_, row) => Fin.Succ(Some((Seq(row.Bond), row.Margin.As(LengthUnit.Millimeter), row.Height.As(LengthUnit.Millimeter)))),
            freeze: static (_, row) => Fin.Succ(Some((Seq(row.Pad), row.Margin.As(LengthUnit.Millimeter), row.Height.As(LengthUnit.Millimeter)))),
            clampingCenter: static (_, row) => Fin.Succ(Some((Seq(row.Body), row.Margin.As(LengthUnit.Millimeter), row.Height.As(LengthUnit.Millimeter)))),
            tailstock: static (_, row) => Fin.Succ(Some((Seq(row.Body), row.Margin.As(LengthUnit.Millimeter), row.Height.As(LengthUnit.Millimeter)))),
            bed: static (_, row) => Fin.Succ(Some((Seq(row.Contact), 0.0, row.Height.As(LengthUnit.Millimeter)))));

    static Fin<Option<(Seq<Loop> Loops, double Margin, double Height)>> Shape(Seq<ContactPatch> contacts, double margin, double height) =>
        contacts.Count > 0
            ? Fin.Succ(Some((contacts.Map(static contact => contact.Footprint), margin, height)))
            : Fin.Fail<Option<(Seq<Loop>, double, double)>>(
                new GeometryFault.DegenerateInput(Kind.Polyline, 0, nameof(ContactPatch.Footprint)).ToError());

    static Fin<Seq<Move>> Condition(Fixture fixture, FixtureState state, Seq<Move> moves) =>
        ValidRuns(fixture.Spec.Profiles, fixture.Spec.Runs)
        && fixture.Spec.Runs[fixture.Spec.Runs.Count - 1].Start + fixture.Spec.Runs[fixture.Spec.Runs.Count - 1].Count == moves.Count
            ? moves.Traverse(move => Move.Admit(move).ToValidation()).As().ToFin().Bind(admitted => admitted
            .Fold(
                Fin.Succ((Cursor: fixture.InitialCursor, Accepted: Seq<Move>())),
                (rail, move) => rail.Bind(current => Segments(current.Cursor, move, fixture.Spec.ArcChordError.As(LengthUnit.Millimeter)).Bind(path =>
                    Blocks(path, fixture, state).Match(
                        Some: zone => Fin.Fail<(Point3d, Seq<Move>)>(FabricationFault.Collision(zone.Collision, CollisionContact.Cutter).ToError()),
                        None: () => Fin.Succ((Target(move), current.Accepted.Add(move))))))
            .Map(static current => current.Accepted))
            : Fin.Fail<Seq<Move>>(new FabricationFault.FixtureInadmissible(new FixturingWitness.Partition(
                fixture.Spec.Runs.Sum(static run => run.Count), moves.Count)).ToError());

    static Fin<Option<ExclusionZone>> Clear(Fixture fixture, FixtureState state, ToolCorridor corridor) =>
        corridor.Stations.Count >= 2 && corridor.Stations.ForAll(station =>
            Finite(station.Point) && double.IsFinite(corridor.Kind.RadiusMm(station)) && corridor.Kind.RadiusMm(station) >= 0.0)
            ? corridor.Stations.Zip(corridor.Stations.Tail).Traverse(pair =>
                InflatedBlocks(new Edge3(pair.First.Point, pair.Second.Point), Math.Max(corridor.Kind.RadiusMm(pair.First), corridor.Kind.RadiusMm(pair.Second)), fixture, state)
                    .ToValidation()).As().ToFin().Map(static hits => hits.Choose(identity).Head)
            : Fin.Fail<Option<ExclusionZone>>(new FabricationFault.FixtureInadmissible(
                new FixturingWitness.Corridor(corridor.Kind, corridor.Stations.Count)).ToError());

    static Fin<Option<Point3d>> Machined(Fixture fixture, StockSnapshot stock) =>
        fixture.Zones.Bind(zone => stock.Machined.Map(face => (zone, face)))
            .Fold(Fin.Succ(Option<Point3d>.None), static (rail, pair) => rail.Bind(found => found.IsSome
                ? Fin.Succ(found)
                : Boolean(pair.zone.Keepouts, Seq(pair.face), BoolKind.And)
                    .Map(overlap => overlap.Find(static loop => loop.Winding() == Sign.Positive && loop.Count > 0).Map(static loop => loop.At(0)))));

    static Fin<HoldingReceipt> Restrain(Fixture fixture, Seq<LoadCase> loads, double safety) =>
        loads.Count > 0 && loads.ForAll(load => ValidLoad(fixture, load)) && double.IsFinite(safety) && safety >= 1.0
            ? Fin.Succ(new HoldingReceipt(loads.Map(load => Evaluate(fixture.Contacts, load, safety)), fixture.Contacts))
            : Fin.Fail<HoldingReceipt>(new FabricationFault.FixtureInadmissible(
                new FixturingWitness.Restraint(loads.Count, loads.Count(load => !ValidLoad(fixture, load)), safety)).ToError());

    static LoadReceipt Evaluate(Seq<ContactPatch> contacts, LoadCase load, double safety) {
        (Vector3d force, Vector3d moment, Point3d at) = load.Demand;
        Seq<ContactReaction> reactions = contacts.Bind(static contact => contact.Field);
        Vector3d forceDemand = force * safety;
        Vector3d momentDemand = (moment + Vector3d.CrossProduct(Meters(at - Point3d.Origin), force)) * safety;
        ReactionSolution solution = Solve(reactions, new Wrench(forceDemand, momentDemand));
        double pressure = contacts.Map(contact => contact.Law.PressureLimit.As(PressureUnit.Pascal) * Math.Abs(contact.Footprint.Area()) * 1e-6
            / Math.Max(contact.Preload.As(ForceUnit.Newton), 1e-9)).Min();
        double deflection = contacts.Map(contact => contact.Law.NormalStiffnessNPerMm
            * contact.Law.DeflectionLimit.As(LengthUnit.Millimeter) / Math.Max(contact.Preload.As(ForceUnit.Newton), 1e-9)).Min();
        double tangentialDeflection = contacts.Map(contact => contact.Law.TangentialStiffnessNPerMm
            * contact.Law.DeflectionLimit.As(LengthUnit.Millimeter) / Math.Max(forceDemand.Length / contacts.Count, 1e-9)).Min();
        double tip = TipMargin(reactions, forceDemand, momentDemand, at);
        return new LoadReceipt(
            load,
            new AxisMargin(ScaledCapacity(forceDemand, solution.Scale), Abs(forceDemand)),
            new AxisMargin(ScaledCapacity(momentDemand, solution.Scale), Abs(momentDemand)),
            solution.FrictionMargin,
            solution.PullOffMargin,
            pressure,
            deflection,
            tangentialDeflection,
            tip,
            solution.Scale,
            solution.LiftMargin,
            reactions,
            solution.Forces);
    }

    private readonly record struct ReactionSolution(
        double Scale,
        Seq<Vector3d> Forces,
        double Residual,
        double FrictionMargin,
        double PullOffMargin,
        double LiftMargin);

    // One projected six-DOF solve carries force, moment, unilateral, pull-off, and friction limits together;
    // every reported capacity is a projection of its maximum admissible load factor and reaction distribution.
    static ReactionSolution Solve(Seq<ContactReaction> reactions, Wrench demand) {
        double characteristicLength = CharacteristicLength(reactions);
        double relativeTolerance = Math.Sqrt(double.BitIncrement(1.0) - 1.0);
        double demandNorm = Residual(demand.Force, demand.Moment, characteristicLength);
        if (demandNorm <= relativeTolerance)
            return new ReactionSolution(double.PositiveInfinity, reactions.Map(static _ => Vector3d.Zero), 0.0,
                double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity);
        double upper = Math.Max(1.0, reactions.Sum(reaction => {
            Vector3d arm = Meters(reaction.At - Point3d.Origin);
            double capacity = (reaction.NormalCapacity + reaction.TangentialCapacity + reaction.PullOffCapacity)
                .As(ForceUnit.Newton) * reaction.AreaWeight;
            return capacity * Math.Sqrt(1.0 + ((arm * arm) / (characteristicLength * characteristicLength)));
        }) / demandNorm);
        double lower = 0.0;
        ReactionSolution best = Equilibrate(reactions, demand, lower, characteristicLength, relativeTolerance);
        for (int bit = 0; bit < sizeof(double) * 8; bit++) {
            double trial = (lower + upper) / 2.0;
            ReactionSolution candidate = Equilibrate(reactions, demand, trial, characteristicLength, relativeTolerance);
            if (candidate.Residual <= relativeTolerance * Math.Max(1.0, trial * demandNorm)) {
                lower = trial;
                best = candidate;
            }
            else {
                upper = trial;
            }
        }
        return best with { Scale = lower };
    }

    static ReactionSolution Equilibrate(
        Seq<ContactReaction> reactions,
        Wrench demand,
        double scale,
        double characteristicLength,
        double relativeTolerance) {
        Vector3d[] forces = new Vector3d[reactions.Count];
        Vector3d forceResidual = demand.Force * scale;
        Vector3d momentResidual = demand.Moment * scale;
        double step = 1.0 / reactions.Sum(reaction => {
            Vector3d arm = Meters(reaction.At - Point3d.Origin);
            return 1.0 + ((arm * arm) / (characteristicLength * characteristicLength));
        });
        double demandNorm = Residual(demand.Force, demand.Moment, characteristicLength);
        for (int pass = 0; pass < sizeof(double) * 8 * reactions.Count; pass++) {
            for (int index = 0; index < reactions.Count; index++) {
                ContactReaction reaction = reactions[index];
                Vector3d arm = Meters(reaction.At - Point3d.Origin);
                Vector3d gradient = forceResidual + Vector3d.CrossProduct(
                    momentResidual / characteristicLength,
                    arm / characteristicLength);
                Vector3d projected = Project(reaction, forces[index] - (step * gradient));
                Vector3d delta = projected - forces[index];
                forces[index] = projected;
                forceResidual += delta;
                momentResidual += Vector3d.CrossProduct(arm, delta);
            }
            if (Residual(forceResidual, momentResidual, characteristicLength)
                <= relativeTolerance * Math.Max(1.0, scale * demandNorm))
                break;
        }
        Seq<Vector3d> solved = toSeq(forces);
        return new ReactionSolution(
            scale,
            solved,
            Residual(forceResidual, momentResidual, characteristicLength),
            reactions.Zip(solved).Map(row => Margin(
                row.First.TangentialCapacity.As(ForceUnit.Newton) * row.First.AreaWeight,
                Tangential(row.First, row.Second).Length,
                relativeTolerance)).Min(),
            reactions.Zip(solved).Map(row => Margin(
                row.First.PullOffCapacity.As(ForceUnit.Newton) * row.First.AreaWeight,
                Math.Max(0.0, -(row.Second * Unitized(row.First.Normal))),
                relativeTolerance)).Min(),
            reactions.Zip(solved).Map(row => Margin(
                row.First.NormalCapacity.As(ForceUnit.Newton) * row.First.AreaWeight,
                Math.Max(0.0, row.Second * Unitized(row.First.Normal)),
                relativeTolerance)).Min());
    }

    static Vector3d Project(ContactReaction reaction, Vector3d candidate) {
        Vector3d normal = Unitized(reaction.Normal);
        double normalCapacity = reaction.NormalCapacity.As(ForceUnit.Newton) * reaction.AreaWeight;
        double pullOffCapacity = reaction.PullOffCapacity.As(ForceUnit.Newton) * reaction.AreaWeight;
        double tangentialCapacity = reaction.TangentialCapacity.As(ForceUnit.Newton) * reaction.AreaWeight;
        double axial = Math.Clamp(candidate * normal, -pullOffCapacity, normalCapacity);
        Vector3d tangent = Tangential(reaction, candidate);
        return axial * normal + (tangent.Length > tangentialCapacity
            ? tangent * (tangentialCapacity / tangent.Length)
            : tangent);
    }

    static Vector3d Tangential(ContactReaction reaction, Vector3d force) {
        Vector3d normal = Unitized(reaction.Normal);
        return force - ((force * normal) * normal);
    }

    static double Residual(Vector3d force, Vector3d moment, double characteristicLength) => Math.Sqrt(
        (force * force) + ((moment * moment) / (characteristicLength * characteristicLength)));

    static double Margin(double capacity, double used, double relativeTolerance) => used <= relativeTolerance
        ? double.PositiveInfinity
        : capacity / used;

    static Vector3d ScaledCapacity(Vector3d demand, double scale) => new(
        demand.X == 0.0 ? 0.0 : Math.Abs(demand.X) * scale,
        demand.Y == 0.0 ? 0.0 : Math.Abs(demand.Y) * scale,
        demand.Z == 0.0 ? 0.0 : Math.Abs(demand.Z) * scale);

    // Tipping is overturning about a support-polygon edge, not general moment capacity: the
    // restoring term is the normal reaction's own lever about that edge, so a load inside the
    // hull is stable while the same magnitude outside it tips.
    static double TipMargin(Seq<ContactReaction> reactions, Vector3d force, Vector3d moment, Point3d at) {
        Seq<Point3d> hull = Hull(reactions.Map(static reaction => reaction.At));
        if (hull.Count < 2) return 0.0;
        Vector3d arm = Meters(at - Point3d.Origin);
        return hull.Zip(hull.Tail + hull.Take(1)).Map(edge => {
            Vector3d axis = Unitized(Meters(edge.Second - edge.First));
            Vector3d pivot = Meters(edge.First - Point3d.Origin);
            double overturning = Math.Abs(axis * (moment + Vector3d.CrossProduct(arm - pivot, force)));
            double restoring = reactions.Sum(reaction => Math.Max(0.0, axis * Vector3d.CrossProduct(
                Meters(reaction.At - Point3d.Origin) - pivot, -Unitized(reaction.Normal)))
                * reaction.NormalCapacity.As(ForceUnit.Newton) * reaction.AreaWeight);
            return overturning <= 1e-9 ? double.PositiveInfinity : restoring / overturning;
        }).Min();
    }

    static Seq<Point3d> Hull(Seq<Point3d> points) {
        Seq<Point3d> ordered = points.Distinct().OrderBy(static point => point.X).ThenBy(static point => point.Y).ToSeq();
        return ordered.Count < 3 ? ordered : Chain(ordered) + Chain(ordered.Reverse());
    }

    static Seq<Point3d> Chain(Seq<Point3d> ordered) =>
        ordered.Fold(Seq<Point3d>(), static (stack, point) => {
            Seq<Point3d> held = stack;
            while (held.Count >= 2 && Cross(held[held.Count - 2], held[held.Count - 1], point) <= 0.0)
                held = held.Take(held.Count - 1);
            return held.Add(point);
        }).Take(Math.Max(0, ordered.Count - 1));

    static double Cross(Point3d origin, Point3d first, Point3d second) =>
        ((first.X - origin.X) * (second.Y - origin.Y)) - ((first.Y - origin.Y) * (second.X - origin.X));

    internal static Vector3d Meters(Vector3d millimeters) => new(
        Length.FromMillimeters(millimeters.X).As(LengthUnit.Meter),
        Length.FromMillimeters(millimeters.Y).As(LengthUnit.Meter),
        Length.FromMillimeters(millimeters.Z).As(LengthUnit.Meter));

    static Vector3d Unitized(Vector3d value) {
        Vector3d unit = value;
        unit.Unitize();
        return unit;
    }

    static Fin<(FixtureState State, Arr<int> Active)> Transition(Fixture fixture, FixtureStage from, FixtureStage to) {
        int fromIndex = fixture.Spec.Sequence.TakeWhile(step => step.Stage != from).Count;
        int toIndex = fixture.Spec.Sequence.TakeWhile(step => step.Stage != to).Count;
        return fromIndex < fixture.Spec.Sequence.Count && toIndex >= fromIndex && toIndex < fixture.Spec.Sequence.Count
            ? Fin.Succ((fixture.Spec.Sequence[toIndex].State, fixture.Spec.Sequence.Take(toIndex + 1)
                .Fold(Set<int>(), static (active, step) => active.Union(step.Activate).Except(step.Release)).ToArr()))
            : Fin.Fail<(FixtureState, Arr<int>)>(new FabricationFault.FixtureInadmissible(new FixturingWitness.Lifecycle(
                fixture.Spec.Sequence.Find(step => step.Stage == from), None, fixture.Spec.Sequence.Count, toIndex)).ToError());
    }

    // Programs enumerate directly at the admitted cardinalities: the powerset over the template
    // roster is 2^n candidates for an n the seed never bounds, and each survivor costs a full
    // admission, restraint, and corridor pass.
    static Fin<Seq<FixtureCandidate>> Synthesize(FixtureSynthesis seed) {
        if (!Profile(seed.Part) || seed.Samples <= 0 || seed.Templates.Count == 0
            || !seed.Templates.ForAll(static template => template is not null) || seed.Loads.Count == 0
            || seed.MinimumTemplates <= 0 || seed.MaximumTemplates < seed.MinimumTemplates
            || seed.MaximumTemplates > seed.Templates.Count || seed.CandidateBudget <= 0
            || !double.IsFinite(seed.Objective.Total) || seed.Objective.Total <= 0.0
            || !double.IsFinite(seed.SafetyFactor.As(RatioUnit.DecimalFraction)) || seed.SafetyFactor.As(RatioUnit.DecimalFraction) < 1.0)
            return Fin.Fail<Seq<FixtureCandidate>>(new FabricationFault.FixtureInadmissible(new FixturingWitness.Synthesis(
                seed.Templates.Count, seed.MinimumTemplates, seed.MaximumTemplates, seed.CandidateBudget)).ToError());

        return Fixture.Admit(seed.Basis).Bind(basis => {
            int firstElement = basis.Spec.Elements.Map(static element => element.Element).Max() + 1;
            return Programs(seed.Templates, seed.MinimumTemplates, seed.MaximumTemplates).Take(seed.CandidateBudget).ToSeq()
                .Traverse(program => program.Map(static (template, index) => (template, index)).Traverse(row =>
                    row.template.Generate(seed.Part, seed.Samples, firstElement + (row.index * seed.Samples)).ToValidation())
                    .As().ToFin().Bind(generated => {
                        Seq<FixtureElement> elements = generated.Bind(static row => row.Elements);
                        return SynthesisSequence(basis, elements).Bind(sequence => Fixture.Admit(basis.Spec with {
                            Elements = basis.Spec.Elements.Filter(static element => element.Role != FixtureRole.Clamp) + elements,
                            Sequence = sequence,
                        }).Bind(fixture =>
                            Restrain(fixture, seed.Loads, seed.SafetyFactor.As(RatioUnit.DecimalFraction)).Bind(holding => seed.Corridors.Traverse(corridor =>
                                Clear(fixture, FixtureState.Cut, corridor).Map(static blocked => new WorkholdingResult.Clearance(blocked)).ToValidation())
                                .As().ToFin().Map(clearance => Rank(seed.Objective, fixture, holding, clearance, generated)))));
                    }).ToValidation())
                .As().ToFin().Map(static candidates => candidates.Filter(static candidate => candidate.Holding.Holds
                    && candidate.Clearance.ForAll(static receipt => receipt.Clear)).OrderByDescending(static candidate => candidate.Score.Total).ToSeq());
        });
    }

    static Fin<Seq<FixtureStep>> SynthesisSequence(Fixture basis, Seq<FixtureElement> generated) {
        Seq<(FixtureStep Step, int Index)> indexed = basis.Spec.Sequence.Map(static (step, index) => (step, index));
        Error fault = new FabricationFault.FixtureInadmissible(new FixturingWitness.Lifecycle(
            None, None, basis.Spec.Sequence.Count, 0)).ToError();
        return indexed.Find(static row => row.Step.State == FixtureState.Clamp).ToFin(fault).Bind(activation =>
            indexed.Find(row => row.Index > activation.Index && row.Step.State == FixtureState.Unload).ToFin(fault).Map(release => {
                Set<int> retired = basis.Spec.Elements.Filter(static element => element.Role == FixtureRole.Clamp)
                    .Map(static element => element.Element).ToSet();
                Seq<int> added = generated.Map(static element => element.Element);
                return basis.Spec.Sequence.Map(step => step with {
                    Activate = (step.Activate.ToSeq().Filter(element => !retired.Contains(element))
                        + (step.Stage == activation.Step.Stage ? added : Seq<int>())).Distinct().ToArr(),
                    Release = (step.Release.ToSeq().Filter(element => !retired.Contains(element))
                        + (step.Stage == release.Step.Stage ? added : Seq<int>())).Distinct().ToArr(),
                });
            }));
    }

    static IEnumerable<Seq<ClampTemplate>> Programs(Seq<ClampTemplate> templates, int minimum, int maximum) =>
        Enumerable.Range(minimum, Math.Max(0, maximum - minimum + 1))
            .SelectMany(size => Choose(templates, size, cursor: 0));

    static IEnumerable<Seq<ClampTemplate>> Choose(Seq<ClampTemplate> templates, int size, int cursor) =>
        size == 0
            ? [Seq<ClampTemplate>()]
            : Enumerable.Range(cursor, Math.Max(0, templates.Count - cursor - size + 1))
                .SelectMany(index => Choose(templates, size - 1, index + 1).Select(rest => rest.Insert(0, templates[index])));

    static FixtureCandidate Rank(
        FixtureObjective objective,
        Fixture fixture,
        HoldingReceipt holding,
        Seq<WorkholdingResult.Clearance> clearance,
        Seq<(Seq<FixtureElement> Elements, Option<SoftJawInsert> Insert)> generated) {
        Seq<SoftJawInsert> inserts = generated.Bind(row => row.Insert.Match(
            Some: static insert => Seq(insert),
            None: static () => Seq<SoftJawInsert>()));
        double hold = holding.MinimumMargin / (1.0 + holding.MinimumMargin);
        double access = clearance.Count == 0 ? 1.0 : (double)clearance.Count(static receipt => receipt.Clear) / clearance.Count;
        double simplicity = 1.0 / (1.0 + fixture.Spec.Elements.Count + inserts.Count);
        FixtureScore score = new(hold, access, simplicity,
            ((hold * objective.Holding) + (access * objective.Access) + (simplicity * objective.Simplicity)) / objective.Total);
        return new FixtureCandidate(fixture, holding, clearance, inserts, score);
    }

    static Fin<FixtureArtifact> Project(Fixture fixture, FixtureProjection projection) {
        ContentKey key = ContentKey.Of(EgressKind.Plan, Canonical(fixture, projection));
        return Fin.Succ(projection.Switch<FixtureArtifact>(
            machine: () => new FixtureArtifact.Machine(key, fixture.Zones, fixture.Spec.Datum, fixture.Constraint),
            setupSheet: () => new FixtureArtifact.SetupSheet(key, fixture.Spec.Elements, fixture.Spec.Sequence, fixture.Spec.Datum, fixture.Constraint),
            inspection: () => new FixtureArtifact.Inspection(key, fixture.Contacts, fixture.Spec.Datum, fixture.Constraint),
            evidence: () => new FixtureArtifact.Evidence(key, fixture)));
    }

    // Projection dispatch writes each exact payload minus its recursively defined key; every collection and optional
    // value is framed, so payload cardinality, case, absence, and field boundaries remain injective.
    static ReadOnlySpan<byte> Canonical(Fixture fixture, FixtureProjection projection) {
        using ArrayPoolBufferWriter<byte> buffer = new();
        Write(buffer, projection.Key);
        _ = projection.Switch(
            machine: () => {
                Frame(buffer, fixture.Zones, Write);
                Write(buffer, fixture.Spec.Datum);
                Write(buffer, fixture.Constraint);
                return unit;
            },
            setupSheet: () => {
                Frame(buffer, fixture.Spec.Elements, Write);
                Frame(buffer, fixture.Spec.Sequence, Write);
                Write(buffer, fixture.Spec.Datum);
                Write(buffer, fixture.Constraint);
                return unit;
            },
            inspection: () => {
                Frame(buffer, fixture.Contacts, Write);
                Write(buffer, fixture.Spec.Datum);
                Write(buffer, fixture.Constraint);
                return unit;
            },
            evidence: () => { Write(buffer, fixture); return unit; });
        return buffer.WrittenSpan.ToArray();
    }

    internal static ReadOnlySpan<byte> ZoneIdentity(ExclusionZone zone) {
        using ArrayPoolBufferWriter<byte> buffer = new();
        Write(buffer, zone);
        return buffer.WrittenSpan.ToArray();
    }

    static void Frame<T>(ArrayPoolBufferWriter<byte> buffer, Seq<T> rows, Action<ArrayPoolBufferWriter<byte>, T> write) {
        Write(buffer, rows.Count);
        _ = rows.Iter(row => write(buffer, row));
    }

    static void Write(ArrayPoolBufferWriter<byte> buffer, int value) {
        BinaryPrimitives.WriteInt32LittleEndian(buffer.GetSpan(sizeof(int)), value);
        buffer.Advance(sizeof(int));
    }

    static void Write(ArrayPoolBufferWriter<byte> buffer, long value) {
        BinaryPrimitives.WriteInt64LittleEndian(buffer.GetSpan(sizeof(long)), value);
        buffer.Advance(sizeof(long));
    }

    static void Write(ArrayPoolBufferWriter<byte> buffer, double value) {
        BinaryPrimitives.WriteInt64LittleEndian(buffer.GetSpan(sizeof(long)), BitConverter.DoubleToInt64Bits(value));
        buffer.Advance(sizeof(long));
    }

    static void Write(ArrayPoolBufferWriter<byte> buffer, Point3d value) {
        Write(buffer, value.X);
        Write(buffer, value.Y);
        Write(buffer, value.Z);
    }

    static void Write(ArrayPoolBufferWriter<byte> buffer, Vector3d value) {
        Write(buffer, value.X);
        Write(buffer, value.Y);
        Write(buffer, value.Z);
    }

    static void Write(ArrayPoolBufferWriter<byte> buffer, bool value) => Write(buffer, value ? 1 : 0);
    static void Write(ArrayPoolBufferWriter<byte> buffer, UInt128 value) {
        Write(buffer, unchecked((long)(ulong)value));
        Write(buffer, unchecked((long)(ulong)(value >> 64)));
    }

    static void Write(ArrayPoolBufferWriter<byte> buffer, Plane value) {
        Write(buffer, value.Origin);
        Write(buffer, value.XAxis);
        Write(buffer, value.YAxis);
        Write(buffer, value.ZAxis);
    }

    static void Write(ArrayPoolBufferWriter<byte> buffer, string value) {
        int length = Encoding.UTF8.GetByteCount(value);
        Write(buffer, length);
        Encoding.UTF8.GetBytes(value, buffer.GetSpan(length));
        buffer.Advance(length);
    }

    static void Write(ArrayPoolBufferWriter<byte> buffer, Context value) {
        Write(buffer, value.Absolute.Value);
        Write(buffer, value.Relative.Value);
        Write(buffer, value.Angle.Value);
        Write(buffer, (int)value.Unit.System);
        Write(buffer, value.Unit.MetersPerUnit);
        value.Unit.Name.Match(
            Some: name => { Write(buffer, true); Write(buffer, name); },
            None: () => Write(buffer, false));
    }

    static void Write(ArrayPoolBufferWriter<byte> buffer, Loop value) {
        Frame(buffer, value.Vertices.ToSeq(), Write);
        Write(buffer, value.Closed);
        Frame(buffer, value.Bulges.ToSeq(), Write);
        Write(buffer, value.Tolerance);
    }

    static void Write(ArrayPoolBufferWriter<byte> buffer, ContactLaw value) {
        Write(buffer, value.Friction.As(RatioUnit.DecimalFraction));
        Write(buffer, value.PressureLimit.As(PressureUnit.Pascal));
        Write(buffer, value.NormalStiffnessNPerMm);
        Write(buffer, value.TangentialStiffnessNPerMm);
        Write(buffer, value.DeflectionLimit.As(LengthUnit.Millimeter));
        Write(buffer, value.PullOff.As(ForceUnit.Newton));
    }

    static void Write(ArrayPoolBufferWriter<byte> buffer, Actuation value) => _ = value.Switch(
        state: buffer,
        manual: static (held, row) => {
            Write(held, nameof(Actuation.Manual)); Write(held, row.Torque.As(TorqueUnit.NewtonMeter));
            Write(held, row.MeanRadius.As(LengthUnit.Millimeter)); Write(held, row.Efficiency.As(RatioUnit.DecimalFraction));
            Write(held, row.SelfLocking); return unit;
        },
        spring: static (held, row) => {
            Write(held, nameof(Actuation.Spring)); Write(held, row.Force.As(ForceUnit.Newton));
            Write(held, row.Stroke.As(LengthUnit.Millimeter)); return unit;
        },
        pneumatic: static (held, row) => {
            Write(held, nameof(Actuation.Pneumatic)); Write(held, row.Pressure.As(PressureUnit.Pascal));
            Write(held, row.Piston.As(AreaUnit.SquareMeter)); Write(held, row.ClampsOnLoss); return unit;
        },
        hydraulic: static (held, row) => {
            Write(held, nameof(Actuation.Hydraulic)); Write(held, row.Pressure.As(PressureUnit.Pascal));
            Write(held, row.Piston.As(AreaUnit.SquareMeter)); Write(held, row.AccumulatorHeld); return unit;
        },
        electric: static (held, row) => {
            Write(held, nameof(Actuation.Electric)); Write(held, row.Force.As(ForceUnit.Newton));
            Write(held, row.Stroke.As(LengthUnit.Millimeter)); Write(held, row.BrakeHeld); return unit;
        },
        field: static (held, row) => {
            Write(held, nameof(Actuation.Field)); Write(held, row.PullOff.As(ForceUnit.Newton));
            Write(held, row.Release.As(DurationUnit.Second)); return unit;
        });

    static void Write(ArrayPoolBufferWriter<byte> buffer, ContactPatch value) {
        Write(buffer, value.Element);
        Write(buffer, value.Footprint);
        Write(buffer, value.Center);
        Write(buffer, value.Normal);
        Write(buffer, value.Law);
        Write(buffer, value.Preload.As(ForceUnit.Newton));
    }

    static void Write(ArrayPoolBufferWriter<byte> buffer, FixtureElement value) {
        Write(buffer, value.Element);
        Write(buffer, value.Role.Key);
        value.Kind.Match(
            Some: kind => { Write(buffer, true); Write(buffer, kind.Key); },
            None: () => Write(buffer, false));
        _ = value.Switch(
            state: buffer,
            locatingPlane: static (held, row) => { Write(held, nameof(FixtureElement.LocatingPlane)); Write(held, row.Contact); return unit; },
            roundPin: static (held, row) => { Write(held, nameof(FixtureElement.RoundPin)); Write(held, row.Center); Write(held, row.Radius.As(LengthUnit.Millimeter)); Write(held, row.Height.As(LengthUnit.Millimeter)); Frame(held, row.Contacts, Write); return unit; },
            diamondPin: static (held, row) => { Write(held, nameof(FixtureElement.DiamondPin)); Write(held, row.Center); Write(held, row.FreeAxis); Write(held, row.Radius.As(LengthUnit.Millimeter)); Write(held, row.Height.As(LengthUnit.Millimeter)); Frame(held, row.Contacts, Write); return unit; },
            nest: static (held, row) => { Write(held, nameof(FixtureElement.Nest)); Frame(held, row.Contacts, Write); return unit; },
            locatingCenter: static (held, row) => { Write(held, nameof(FixtureElement.LocatingCenter)); Write(held, row.Point); Write(held, row.Axis); Write(held, row.IncludedAngle.As(AngleUnit.Degree)); Write(held, row.Contact); return unit; },
            mandrel: static (held, row) => { Write(held, nameof(FixtureElement.Mandrel)); Write(held, row.Center); Write(held, row.Axis); Write(held, row.Radius.As(LengthUnit.Millimeter)); Write(held, row.Length.As(LengthUnit.Millimeter)); Frame(held, row.Contacts, Write); return unit; },
            optical: static (held, row) => { Write(held, nameof(FixtureElement.Optical)); Write(held, row.Datum); Write(held, row.Repeatability.As(LengthUnit.Millimeter)); return unit; },
            fixedSupport: static (held, row) => { Write(held, nameof(FixtureElement.FixedSupport)); Write(held, row.Contact); return unit; },
            adjustableSupport: static (held, row) => { Write(held, nameof(FixtureElement.AdjustableSupport)); Write(held, row.Contact); Write(held, row.Travel.As(LengthUnit.Millimeter)); return unit; },
            hydraulicSupport: static (held, row) => { Write(held, nameof(FixtureElement.HydraulicSupport)); Write(held, row.Contact); Write(held, row.EqualizedPressure.As(PressureUnit.Pascal)); return unit; },
            compliantSupport: static (held, row) => { Write(held, nameof(FixtureElement.CompliantSupport)); Write(held, row.Contact); Write(held, row.DeflectionLimit.As(LengthUnit.Millimeter)); return unit; },
            steadyRest: static (held, row) => { Write(held, nameof(FixtureElement.SteadyRest)); Frame(held, row.Contacts, Write); Write(held, row.Station.As(LengthUnit.Millimeter)); return unit; },
            sacrificialSupport: static (held, row) => { Write(held, nameof(FixtureElement.SacrificialSupport)); Write(held, row.Contact); Write(held, row.RemainingThickness.As(LengthUnit.Millimeter)); return unit; },
            toeClamp: static (held, row) => { Write(held, nameof(FixtureElement.ToeClamp)); Write(held, row.Body); Write(held, row.Contact); Write(held, row.Drive); Write(held, row.Margin.As(LengthUnit.Millimeter)); Write(held, row.Height.As(LengthUnit.Millimeter)); return unit; },
            vise: static (held, row) => { Write(held, nameof(FixtureElement.Vise)); Frame(held, row.Bodies, Write); Frame(held, row.Contacts, Write); Write(held, row.Drive); Write(held, row.Opening.As(LengthUnit.Millimeter)); Write(held, row.Margin.As(LengthUnit.Millimeter)); Write(held, row.Height.As(LengthUnit.Millimeter)); return unit; },
            chuck: static (held, row) => { Write(held, nameof(FixtureElement.Chuck)); Frame(held, row.Jaws, Write); Frame(held, row.Contacts, Write); Write(held, row.Drive); Write(held, row.AxialCapacity.As(ForceUnit.Newton)); Write(held, row.Margin.As(LengthUnit.Millimeter)); Write(held, row.Height.As(LengthUnit.Millimeter)); return unit; },
            collet: static (held, row) => { Write(held, nameof(FixtureElement.Collet)); Write(held, row.Body); Frame(held, row.Contacts, Write); Write(held, row.Drive); Write(held, row.Collapse.As(LengthUnit.Millimeter)); Write(held, row.Margin.As(LengthUnit.Millimeter)); Write(held, row.Height.As(LengthUnit.Millimeter)); return unit; },
            arbor: static (held, row) => { Write(held, nameof(FixtureElement.Arbor)); Write(held, row.Body); Frame(held, row.Contacts, Write); Write(held, row.Drive); Write(held, row.Expansion.As(LengthUnit.Millimeter)); Write(held, row.Margin.As(LengthUnit.Millimeter)); Write(held, row.Height.As(LengthUnit.Millimeter)); return unit; },
            vacuum: static (held, row) => { Write(held, nameof(FixtureElement.Vacuum)); Write(held, row.Bed); Frame(held, row.Leaks, Write); Write(held, row.Law); Write(held, row.Pressure.As(PressureUnit.Pascal)); Write(held, row.Margin.As(LengthUnit.Millimeter)); Write(held, row.Height.As(LengthUnit.Millimeter)); return unit; },
            magnetic: static (held, row) => { Write(held, nameof(FixtureElement.Magnetic)); Write(held, row.Pad); Write(held, row.Law); Write(held, row.Coupling.As(RatioUnit.DecimalFraction)); Write(held, row.Margin.As(LengthUnit.Millimeter)); Write(held, row.Height.As(LengthUnit.Millimeter)); return unit; },
            adhesive: static (held, row) => { Write(held, nameof(FixtureElement.Adhesive)); Write(held, row.Bond); Write(held, row.Law); Write(held, row.Cure.As(RatioUnit.DecimalFraction)); Write(held, row.Margin.As(LengthUnit.Millimeter)); Write(held, row.Height.As(LengthUnit.Millimeter)); return unit; },
            freeze: static (held, row) => { Write(held, nameof(FixtureElement.Freeze)); Write(held, row.Pad); Write(held, row.Law); Write(held, row.Frozen.As(RatioUnit.DecimalFraction)); Write(held, row.Margin.As(LengthUnit.Millimeter)); Write(held, row.Height.As(LengthUnit.Millimeter)); return unit; },
            clampingCenter: static (held, row) => { Write(held, nameof(FixtureElement.ClampingCenter)); Write(held, row.Body); Write(held, row.Contact); Write(held, row.Drive); Write(held, row.Margin.As(LengthUnit.Millimeter)); Write(held, row.Height.As(LengthUnit.Millimeter)); return unit; },
            tailstock: static (held, row) => { Write(held, nameof(FixtureElement.Tailstock)); Write(held, row.Body); Write(held, row.Contact); Write(held, row.Drive); Write(held, row.Margin.As(LengthUnit.Millimeter)); Write(held, row.Height.As(LengthUnit.Millimeter)); return unit; },
            bed: static (held, row) => { Write(held, nameof(FixtureElement.Bed)); Write(held, row.Contact); Write(held, row.Law); Write(held, row.Pressure.As(PressureUnit.Pascal)); Write(held, row.Height.As(LengthUnit.Millimeter)); return unit; });
    }

    static void Write(ArrayPoolBufferWriter<byte> buffer, FixtureStep value) {
        Write(buffer, value.Stage.Value);
        Write(buffer, value.State.Key);
        Frame(buffer, value.Activate.ToSeq(), Write);
        Frame(buffer, value.Release.ToSeq(), Write);
        Write(buffer, value.Settle.As(DurationUnit.Second));
    }

    static void Write(ArrayPoolBufferWriter<byte> buffer, DatumFrame value) {
        Write(buffer, value.Work);
        Frame(buffer, value.Primary.ToSeq(), Write);
        Frame(buffer, value.Secondary.ToSeq(), Write);
        Frame(buffer, value.Tertiary.ToSeq(), Write);
        Write(buffer, value.Repeatability.As(LengthUnit.Millimeter));
    }

    static void Write(ArrayPoolBufferWriter<byte> buffer, ExclusionZone value) {
        Write(buffer, value.Operation);
        Write(buffer, value.Element);
        Write(buffer, value.Role.Key);
        value.Kind.Match(
            Some: kind => { Write(buffer, true); Write(buffer, kind.Key); },
            None: () => Write(buffer, false));
        Frame(buffer, value.Keepouts, Write);
        Frame(buffer, value.Walls, Write);
        Write(buffer, value.Lower.As(LengthUnit.Millimeter));
        Write(buffer, value.Upper.As(LengthUnit.Millimeter));
        Frame(buffer, value.Active.OrderBy(static state => state.Key).ToSeq(), static (held, state) => Write(held, state.Key));
        Write(buffer, value.ArcChordError.As(LengthUnit.Millimeter));
    }

    static void Write(ArrayPoolBufferWriter<byte> buffer, ContactReaction value) {
        Write(buffer, value.Element);
        Write(buffer, value.At);
        Write(buffer, value.Normal);
        Write(buffer, value.NormalCapacity.As(ForceUnit.Newton));
        Write(buffer, value.TangentialCapacity.As(ForceUnit.Newton));
        Write(buffer, value.AreaWeight);
    }

    static void Write(ArrayPoolBufferWriter<byte> buffer, ConstraintReceipt value) {
        Write(buffer, value.Rank);
        Write(buffer, value.Frictionless);
        Write(buffer, value.Redundancy);
        Frame(buffer, value.Reactions, Write);
    }

    static void Write(ArrayPoolBufferWriter<byte> buffer, ContentKey value) {
        Write(buffer, value.Kind.Key);
        Write(buffer, value.Digest);
    }

    static void Write(ArrayPoolBufferWriter<byte> buffer, StockSnapshot value) {
        Write(buffer, value.Setup);
        Write(buffer, value.Key);
        Frame(buffer, value.Machined.ToSeq(), Write);
    }

    static void Write(ArrayPoolBufferWriter<byte> buffer, FixtureSpec value) {
        Write(buffer, value.Operation);
        Frame(buffer, value.Elements, Write);
        Frame(buffer, value.Sequence, Write);
        Write(buffer, value.Datum);
        Frame(buffer, value.Profiles.ToSeq(), Write);
        Frame(buffer, value.Runs, static (held, run) => { Write(held, run.Loop); Write(held, run.Start); Write(held, run.Count); });
        Write(buffer, value.InitialCursor);
        value.Current.Match(
            Some: current => { Write(buffer, true); Write(buffer, current); },
            None: () => Write(buffer, false));
        Write(buffer, value.ArcChordError.As(LengthUnit.Millimeter));
    }

    static void Write(ArrayPoolBufferWriter<byte> buffer, Fixture value) {
        Write(buffer, value.Spec);
        Frame(buffer, value.Zones, Write);
        Frame(buffer, value.Contacts, Write);
        Write(buffer, value.Constraint);
    }

    static Fin<Option<ExclusionZone>> InflatedBlocks(Edge3 axis, double radius, Fixture fixture, FixtureState state) =>
        fixture.Zones.Traverse(zone => Offset(zone.Keepouts, radius).Bind(grown => grown
            .Traverse(loop => Lower(loop, zone.ArcChordError.As(LengthUnit.Millimeter)).ToValidation())
            .As().ToFin().Map(walls => zone with { Keepouts = grown, Walls = walls })).ToValidation())
            .As().ToFin().Map(zones => zones.Find(zone => zone.Crosses(axis, state)));

    static Option<ExclusionZone> Blocks(Seq<Edge3> path, Fixture fixture, FixtureState state) =>
        fixture.Zones.Find(zone => path.Exists(segment => zone.Crosses(segment, state)));

    internal static Option<Edge3> Below(Edge3 segment, double lower, double upper) {
        double dz = segment.B.Z - segment.A.Z;
        if (Math.Abs(dz) < 1e-12) return segment.A.Z >= lower && segment.A.Z <= upper ? Some(segment) : None;
        double first = (lower - segment.A.Z) / dz;
        double second = (upper - segment.A.Z) / dz;
        double start = Math.Max(0.0, Math.Min(first, second));
        double end = Math.Min(1.0, Math.Max(first, second));
        return start <= end ? Some(new Edge3(segment.A + (start * (segment.B - segment.A)), segment.A + (end * (segment.B - segment.A)))) : None;
    }

    internal static bool Crosses(Edge3 segment, Loop wall) =>
        toSeq(Enumerable.Range(0, wall.Count)).Exists(index => Intersection(segment, new Edge3(wall.At(index), wall.At(index + 1))));

    static bool Intersection(Edge3 first, Edge3 second) {
        Vector3d r = first.B - first.A;
        Vector3d s = second.B - second.A;
        Vector3d q = second.A - first.A;
        double cross = (r.X * s.Y) - (r.Y * s.X);
        if (Math.Abs(cross) < 1e-12) {
            if (Math.Abs((q.X * r.Y) - (q.Y * r.X)) >= 1e-12) return false;
            double length = r * r;
            if (length <= 1e-18) return first.A.DistanceTo(second.A) <= 1e-9;
            double left = ((second.A - first.A) * r) / length;
            double right = ((second.B - first.A) * r) / length;
            return Math.Max(0.0, Math.Min(left, right)) <= Math.Min(1.0, Math.Max(left, right));
        }
        double t = ((q.X * s.Y) - (q.Y * s.X)) / cross;
        double u = ((q.X * r.Y) - (q.Y * r.X)) / cross;
        return t is >= 0.0 and <= 1.0 && u is >= 0.0 and <= 1.0;
    }

    static Fin<Seq<Edge3>> Segments(Point3d from, Move move, double error) =>
        move.Switch(
            state: (from, error),
            rapid: static (state, row) => Fin.Succ(Seq(new Edge3(state.from, row.Target))),
            linear: static (state, row) => Fin.Succ(Seq(new Edge3(state.from, row.Target))),
            circular: static (state, row) => ArcSegments(state.from, row.Target, row.Arc, state.error));

    static Fin<Seq<Edge3>> ArcSegments(Point3d from, Point3d to, ArcCenter arc, double error) {
        Vector3d a = from - arc.Center;
        Vector3d b = to - arc.Center;
        double radius = a.Length;
        if (!double.IsFinite(radius) || radius <= 0.0 || Math.Abs(radius - b.Length) > error)
            return Fin.Fail<Seq<Edge3>>(new GeometryFault.DegenerateInput(Kind.Arc, 0, nameof(ArcCenter)).ToError());
        double start = Math.Atan2(a.Y, a.X);
        double end = Math.Atan2(b.Y, b.X);
        double sweep = arc.Sense == RotationSense.Clockwise ? -Normalize(start - end) : Normalize(end - start);
        if (from.DistanceTo(to) <= error) sweep = arc.Sense == RotationSense.Clockwise ? -Math.Tau : Math.Tau;
        int count = Math.Max(1, (int)Math.Ceiling(Math.Abs(sweep) / Math.Max(1e-6, 2.0 * Math.Acos(Math.Clamp(1.0 - (error / radius), -1.0, 1.0)))));
        Seq<Point3d> points = toSeq(Enumerable.Range(0, count + 1)).Map(index => {
            double angle = start + (sweep * index / count);
            return new Point3d(arc.Center.X + (radius * Math.Cos(angle)), arc.Center.Y + (radius * Math.Sin(angle)), from.Z + ((to.Z - from.Z) * index / count));
        });
        return Fin.Succ(toSeq(Enumerable.Range(0, count)).Map(index => new Edge3(points[index], points[index + 1])));
    }

    static Fin<Seq<ContactPatch>> ElementContacts(FixtureElement element) =>
        element.Switch(
            locatingPlane: static row => Fin.Succ(Seq(row.Contact)),
            roundPin: static row => Fin.Succ(row.Contacts),
            diamondPin: static row => Fin.Succ(row.Contacts),
            nest: static row => Fin.Succ(row.Contacts),
            locatingCenter: static row => Fin.Succ(Seq(row.Contact)),
            mandrel: static row => Fin.Succ(row.Contacts),
            optical: static _ => Fin.Succ(Seq<ContactPatch>()),
            fixedSupport: static row => Fin.Succ(Seq(row.Contact)),
            adjustableSupport: static row => Fin.Succ(Seq(row.Contact)),
            hydraulicSupport: static row => Fin.Succ(Seq(row.Contact)),
            compliantSupport: static row => Fin.Succ(Seq(row.Contact)),
            steadyRest: static row => Fin.Succ(row.Contacts),
            sacrificialSupport: static row => Fin.Succ(Seq(row.Contact)),
            toeClamp: static row => Fin.Succ(Seq(row.Contact with { Preload = row.Drive.Preload })),
            vise: static row => Fin.Succ(row.Contacts.Map(contact => contact with { Preload = row.Drive.Preload / row.Contacts.Count })),
            chuck: static row => Fin.Succ(row.Contacts.Map(contact => contact with { Preload = row.Drive.Preload / row.Contacts.Count })),
            collet: static row => Fin.Succ(row.Contacts.Map(contact => contact with { Preload = row.Drive.Preload / row.Contacts.Count })),
            arbor: static row => Fin.Succ(row.Contacts.Map(contact => contact with { Preload = row.Drive.Preload / row.Contacts.Count })),
            vacuum: static row => Fin.Succ(Seq(new ContactPatch(row.Element, row.Bed, row.Bed.Bound().Center, -Vector3d.ZAxis, row.Law,
                new Force(row.Pressure.As(PressureUnit.Pascal) * Math.Abs(row.Bed.Area()) * 1e-6, ForceUnit.Newton)))),
            magnetic: static row => Fin.Succ(Seq(new ContactPatch(row.Element, row.Pad, row.Pad.Bound().Center, -Vector3d.ZAxis, row.Law,
                row.Law.PullOff * row.Coupling.As(RatioUnit.DecimalFraction)))),
            adhesive: static row => Fin.Succ(Seq(new ContactPatch(row.Element, row.Bond, row.Bond.Bound().Center, -Vector3d.ZAxis, row.Law,
                row.Law.PullOff * row.Cure.As(RatioUnit.DecimalFraction)))),
            freeze: static row => Fin.Succ(Seq(new ContactPatch(row.Element, row.Pad, row.Pad.Bound().Center, -Vector3d.ZAxis, row.Law,
                row.Law.PullOff * row.Frozen.As(RatioUnit.DecimalFraction)))),
            clampingCenter: static row => Fin.Succ(Seq(row.Contact with { Preload = row.Drive.Preload })),
            tailstock: static row => Fin.Succ(Seq(row.Contact with { Preload = row.Drive.Preload })),
            bed: static row => Fin.Succ(Seq(new ContactPatch(row.Element, row.Contact, row.Contact.Bound().Center, Vector3d.ZAxis, row.Law,
                new Force(row.Pressure.As(PressureUnit.Pascal) * Math.Abs(row.Contact.Area()) * 1e-6, ForceUnit.Newton)))));

    static bool ValidElement(FixtureElement element) => element.Element >= 0 && element.Switch(
        locatingPlane: static row => Valid(row.Element, Seq(row.Contact)),
        roundPin: static row => Positive(row.Radius) && Positive(row.Height) && Valid(row.Element, row.Contacts),
        diamondPin: static row => Positive(row.Radius) && Positive(row.Height) && Finite(row.FreeAxis) && row.FreeAxis.Length > 1e-9 && Valid(row.Element, row.Contacts),
        nest: static row => Valid(row.Element, row.Contacts),
        locatingCenter: static row => Finite(row.Point) && Finite(row.Axis) && row.Axis.Length > 1e-9
            && row.IncludedAngle.As(AngleUnit.Degree) is > 0.0 and < 180.0 && Valid(row.Element, Seq(row.Contact)),
        mandrel: static row => Finite(row.Center) && Finite(row.Axis) && row.Axis.Length > 1e-9
            && Positive(row.Radius) && Positive(row.Length) && Valid(row.Element, row.Contacts),
        optical: static row => row.Datum.IsValid && Nonnegative(row.Repeatability),
        fixedSupport: static row => Valid(row.Element, Seq(row.Contact)),
        adjustableSupport: static row => Positive(row.Travel) && Valid(row.Element, Seq(row.Contact)),
        hydraulicSupport: static row => Positive(row.EqualizedPressure) && Valid(row.Element, Seq(row.Contact)),
        compliantSupport: static row => Positive(row.DeflectionLimit) && Valid(row.Element, Seq(row.Contact)),
        steadyRest: static row => Nonnegative(row.Station) && Valid(row.Element, row.Contacts),
        sacrificialSupport: static row => Positive(row.RemainingThickness) && Valid(row.Element, Seq(row.Contact)),
        toeClamp: static row => Profile(row.Body) && Valid(row.Drive) && Nonnegative(row.Margin) && Positive(row.Height) && Valid(row.Element, Seq(row.Contact)),
        vise: static row => row.Bodies.ForAll(Profile) && row.Bodies.Count == 2 && Valid(row.Drive) && Positive(row.Opening)
            && Nonnegative(row.Margin) && Positive(row.Height) && Valid(row.Element, row.Contacts),
        chuck: static row => row.Jaws.ForAll(Profile) && row.Jaws.Count >= 3 && Valid(row.Drive) && Positive(row.AxialCapacity)
            && Nonnegative(row.Margin) && Positive(row.Height) && Valid(row.Element, row.Contacts),
        collet: static row => Profile(row.Body) && Valid(row.Drive) && Positive(row.Collapse) && Nonnegative(row.Margin)
            && Positive(row.Height) && Valid(row.Element, row.Contacts),
        arbor: static row => Profile(row.Body) && Valid(row.Drive) && Positive(row.Expansion) && Nonnegative(row.Margin)
            && Positive(row.Height) && Valid(row.Element, row.Contacts),
        vacuum: static row => Profile(row.Bed) && row.Leaks.ForAll(Profile) && Valid(row.Law)
            && Positive(row.Pressure) && Nonnegative(row.Margin) && Positive(row.Height),
        magnetic: static row => Profile(row.Pad) && Valid(row.Law) && Fraction(row.Coupling) && Nonnegative(row.Margin) && Positive(row.Height),
        adhesive: static row => Profile(row.Bond) && Valid(row.Law) && Fraction(row.Cure) && Nonnegative(row.Margin) && Positive(row.Height),
        freeze: static row => Profile(row.Pad) && Valid(row.Law) && Fraction(row.Frozen) && Nonnegative(row.Margin) && Positive(row.Height),
        clampingCenter: static row => Profile(row.Body) && Valid(row.Drive) && Nonnegative(row.Margin) && Positive(row.Height) && Valid(row.Element, Seq(row.Contact)),
        tailstock: static row => Profile(row.Body) && Valid(row.Drive) && Nonnegative(row.Margin) && Positive(row.Height) && Valid(row.Element, Seq(row.Contact)),
        bed: static row => Profile(row.Contact) && Valid(row.Law) && Positive(row.Pressure) && Positive(row.Height));

    static bool Valid(int element, Seq<ContactPatch> contacts) => contacts.Count > 0 && contacts.ForAll(contact => contact is not null && contact.Element == element
        && Profile(contact.Footprint) && contact.Footprint.Bulges.ForAll(static bulge => bulge == 0.0)
        && Finite(contact.Center) && Finite(contact.Normal) && contact.Normal.Length > 1e-9
        && Valid(contact.Law) && double.IsFinite(contact.Preload.As(ForceUnit.Newton)) && contact.Preload.As(ForceUnit.Newton) > 0.0);

    static bool Valid(Actuation drive) => drive is not null && drive.Switch(
        manual: static row => Positive(row.Torque) && Positive(row.MeanRadius) && Fraction(row.Efficiency),
        spring: static row => Positive(row.Force) && Positive(row.Stroke),
        pneumatic: static row => Positive(row.Pressure) && Positive(row.Piston),
        hydraulic: static row => Positive(row.Pressure) && Positive(row.Piston),
        electric: static row => Positive(row.Force) && Positive(row.Stroke),
        field: static row => Positive(row.PullOff) && Nonnegative(row.Release));
    static bool Valid(ContactLaw law) => double.IsFinite(law.Friction.As(RatioUnit.DecimalFraction)) && law.Friction.As(RatioUnit.DecimalFraction) >= 0.0
        && Positive(law.PressureLimit) && law.NormalStiffnessNPerMm > 0.0 && double.IsFinite(law.NormalStiffnessNPerMm)
        && law.TangentialStiffnessNPerMm > 0.0 && double.IsFinite(law.TangentialStiffnessNPerMm)
        && Positive(law.DeflectionLimit) && double.IsFinite(law.PullOff.As(ForceUnit.Newton)) && law.PullOff.As(ForceUnit.Newton) >= 0.0;
    static bool ValidLoad(LoadCase load) => load is not null && load.Switch(
        cutting: static row => row.Operation >= 0 && Finite(row.At) && Profile(row.Region) && Finite(row.Force.Vector) && Finite(row.Moment.Vector),
        gravity: static row => Finite(row.Center) && Finite(row.Force.Vector),
        acceleration: static row => Finite(row.Center) && Finite(row.Force.Vector) && Finite(row.Moment.Vector),
        probing: static row => Finite(row.At) && Finite(row.Force.Vector),
        handling: static row => Finite(row.At) && Finite(row.Force.Vector) && Finite(row.Moment.Vector),
        thermal: static row => Finite(row.At) && Finite(row.Force.Vector) && Finite(row.Moment.Vector),
        pressure: static row => Finite(row.Center) && Profile(row.Region) && Finite(row.Force.Vector));
    static bool ValidLoad(Fixture fixture, LoadCase load) => ValidLoad(load) && load.Switch(
        state: fixture.Operation,
        cutting: static (operation, row) => row.Operation == operation,
        gravity: static (_, _) => true,
        acceleration: static (_, _) => true,
        probing: static (_, _) => true,
        handling: static (_, _) => true,
        thermal: static (_, _) => true,
        pressure: static (_, _) => true);
    static bool Positive(Length value) => double.IsFinite(value.As(LengthUnit.Millimeter)) && value.As(LengthUnit.Millimeter) > 0.0;
    static bool Nonnegative(Length value) => double.IsFinite(value.As(LengthUnit.Millimeter)) && value.As(LengthUnit.Millimeter) >= 0.0;
    static bool Positive(Force value) => double.IsFinite(value.As(ForceUnit.Newton)) && value.As(ForceUnit.Newton) > 0.0;
    static bool Positive(Pressure value) => double.IsFinite(value.As(PressureUnit.Pascal)) && value.As(PressureUnit.Pascal) > 0.0;
    static bool Positive(Area value) => double.IsFinite(value.As(AreaUnit.SquareMeter)) && value.As(AreaUnit.SquareMeter) > 0.0;
    static bool Positive(Torque value) => double.IsFinite(value.As(TorqueUnit.NewtonMeter)) && value.As(TorqueUnit.NewtonMeter) > 0.0;
    static bool Nonnegative(Duration value) => double.IsFinite(value.As(DurationUnit.Second)) && value.As(DurationUnit.Second) >= 0.0;
    static bool Fraction(Ratio value) => double.IsFinite(value.As(RatioUnit.DecimalFraction)) && value.As(RatioUnit.DecimalFraction) is > 0.0 and <= 1.0;

    static Fin<Seq<Loop>> Offset(Seq<Loop> loops, double distance) =>
        loops.Head.ToFin(new GeometryFault.DegenerateInput(Kind.Polyline, 0, nameof(ArcOp.Offset)).ToError()).Bind(basis =>
            ArcForest.Admit(loops, basis.Tolerance, basis.Plane).As().ToFin().Bind(forest =>
                ArcAlgebra.Apply(new ArcOp.Offset(new ArcOffsetSource.Forest(forest), distance)).Bind(static trace => trace switch {
                    ArcTrace.Forest(var result, _) => Fin.Succ(result.Loops),
                    ArcTrace.Paths(var result, _) => Fin.Succ(result),
                    _ => Fin.Fail<Seq<Loop>>(new GeometryFault.DegenerateInput(Kind.Polyline, 0, nameof(ArcTrace)).ToError()),
                })));

    static Fin<Seq<Loop>> Boolean(Seq<Loop> subject, Seq<Loop> clip, BoolKind kind) =>
        subject.Head.ToFin(new GeometryFault.DegenerateInput(Kind.Polyline, 0, nameof(ArcOp.Boolean)).ToError()).Bind(basis =>
            (ArcForest.Admit(subject, basis.Tolerance, basis.Plane), ArcForest.Admit(clip, basis.Tolerance, basis.Plane))
                .Apply((first, second) => ArcAlgebra.Apply(new ArcOp.Boolean(first, second, kind)))
                .As()
                .ToFin()
                .Bind(identity)
                .Bind(static trace => trace switch {
                    ArcTrace.Forest(var result, _) => Fin.Succ(result.Loops),
                    _ => Fin.Fail<Seq<Loop>>(new GeometryFault.DegenerateInput(Kind.Polyline, 1, nameof(ArcTrace)).ToError()),
                }));

    static Fin<Loop> Lower(Loop loop, double error) =>
        ArcAlgebra.Densify(new ArcProjection.Lower(loop, error)).Bind(static trace => trace switch {
            ArcTrace.Densified(var receipt) => Fin.Succ(receipt.Result),
            _ => Fin.Fail<Loop>(new GeometryFault.DegenerateInput(Kind.Polyline, 2, nameof(ArcTrace)).ToError()),
        });

    static Point3d Target(Move move) => move.Switch(rapid: static row => row.Target, linear: static row => row.Target, circular: static row => row.Target);
    static bool ValidRuns(Arr<Loop> profiles, Seq<MoveRun> runs) => runs.Count > 0 && runs[0].Start == 0
        && runs.ForAll(run => run.Loop >= 0 && run.Loop < profiles.Count && run.Start >= 0 && run.Count > 0)
        && runs.Zip(runs.Tail).ForAll(static pair => pair.First.Start + pair.First.Count == pair.Second.Start);
    static double Normalize(double radians) => radians < 0.0 ? radians + Math.Tau : radians;
    static Vector3d Abs(Vector3d value) => new(Math.Abs(value.X), Math.Abs(value.Y), Math.Abs(value.Z));
    static bool Profile(Loop loop) => loop is not null && loop.Closed && loop.Count >= 3 && loop.Vertices.ForAll(Finite) && loop.Bulges.ForAll(double.IsFinite);
    static bool Finite(Point3d value) => TensorPrimitives.IsFiniteAll<double>([value.X, value.Y, value.Z]);
    static bool Finite(Vector3d value) => TensorPrimitives.IsFiniteAll<double>([value.X, value.Y, value.Z]);
}
```
