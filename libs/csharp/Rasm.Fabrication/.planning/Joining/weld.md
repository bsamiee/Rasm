# [RASM_FABRICATION_WELD]

`Weld.Plan` consumes one admitted `WeldRequest` and derives fill-complete bead deposits, side-correct transported torch frames, preparation actions, qualification demands, and one content-keyed `WeldPlan`. Boundary-resolved preparation profiles carry the full section and cavity demand; planning never recreates `Rasm.Materials` groove geometry from a key or a nominal leg.

`WeldProcessLaw` converts `ProcessBudget.Joining` into deposited volume through one `ArcMode` per transfer mode. Joint process keys gate admission and select efficiency, travel, heat-input, and t8/5 envelopes. `BeadProgram` derives role and oscillation from fraction bands; fill roles close the groove, while butter and temper overlays deposit outside it. `Waveform` evaluates oscillation at every waypoint; named weaves never replace paths.

Bead placement is a two-dimensional lattice, not a vertical stack: `FillProfile` resolves the trapezoidal section at the current fill height and `BeadProgram.Lattice` seats as many overlapped beads across that width as it admits, so `WeldPass.LateralOffsetMm` and `WeldPass.HeightOffsetMm` carry a real position inside the groove. `ArcProgram` places run-in, backstep, run-out, and crater dwell on the emitted path and arc clock, so `WeldPass.HeatInputKjMm`, `BeadEvidence.EnergyJ`, and the EN 1011-2 `BeadEvidence.CoolingTimeS` read every burning move.

`WeldPlan.Passes`, `WeldPlan.Actions`, `WeldPlan.Demands`, `WeldPass.Path`, `WeldPass.Frames`, and `TorchFrame` remain the host-local sequence, procedure, Cam, and robot wires. `WeldProjection` parameterizes execution, qualification, and receipt egress without moving scheduling, kinematics, posting, or procedure ownership into joining.

## [01]-[INDEX]

- [01]-[WELD_PLAN]: owns admitted joint and policy input, preparation demand, process and bead policy, pass generation, transported torch frames, preparation actions, qualification evidence, projections, and the `WeldPlan` receipt.

## [02]-[WELD_PLAN]

- Owner: `FillProfile` owns boundary-resolved fill geometry and its trapezoidal section algebra; `RootProgram` owns preparation behavior and the side schedule; `CavityKind` and `FlareKind` own bounded preparation sub-kinds; `JointPrep` owns preparation modality and the EN 1011-2 shape factors; `WeldJoint` owns one admitted joint; `WeldRequest` owns census correspondence and the fill ledger; `DepositionSource`, `ArcMode`, `WeldProcessLaw`, `Waveform`, `WeavePattern`, `PassRole`, `RoleBand`, and `BeadProgram` own deposition policy; `WeldStandard` owns canonical policy rows; `IWeldAccess.Admit` mints internal `WeldAccess` strategies; `WeldDemandBinding` generates profile-defined procedure values; `WeldPolicySource` owns policy admission modality; `WeldPolicy` owns aggregate planning policy; `JointAction`, `TorchFrame`, `BeadEvidence`, `WeldPass`, `WeldDemand`, and `WeldPlan` own execution and evidence; `Weld` owns `Plan` and `HeatInput`.
- Cases: `JointPrep.Groove`, `JointPrep.Fillet`, `JointPrep.Cavity`, and `JointPrep.Flare` carry fill demand without local geometry formulae; the groove case preserves geometry and penetration keys independently. `DepositionSource.SolidWire` and `DepositionSource.CoredWire` parameterize multi-electrode count and spacing, while `DepositionSource.Rod`, `DepositionSource.Strip`, `DepositionSource.Powder`, `DepositionSource.Volumetric`, and `DepositionSource.Autogenous` cover the remaining deposition carriers. `Waveform.Harmonic` carries a phase-shifted sine series and `Waveform.Piecewise` a knot interpolation, so between them any mean-zero periodic oscillation generates. `RootProgram` covers no treatment, backing, backgouging, combined backing and backgouging, and seal deposition. `WeldPolicySource.Defined` and `WeldPolicySource.Canonical` collapse explicit and standard admission onto `WeldPolicy.Admit`, the canonical arm reading one `WeldStandard` row rather than inline constants. `JointAction.Preheat` and `JointAction.PostWeldHeatTreat` carry the thermal shop actions the joint's `PreheatC`, `PwhtC`, and `PwhtMinutes` demand. `WeldDemandBinding.Quantity`, `WeldDemandBinding.Categorical`, `WeldDemandBinding.Boolean`, and `WeldDemandBinding.Temporal` cover the procedure value modalities. `PassLineage.Planned`, `PassLineage.Repair`, and `PassLineage.Temper` preserve derivation evidence.
- Entry: `public static Fin<WeldPlan> Plan(WeldRequest request)` normalizes the census by joint identity, accumulates every joint's planning failure before reporting, resolves each process law and its admitted `ArcMode`, accumulates all access constraints, derives pass count from required volume and realized deposition, generates every pass from the role bands and the bead lattice, verifies heat, cooling, and fill conservation, emits procedure demand maps, and mints `ContentKey.Of(EgressKind.WeldPlan, ...)`.
- Admission: `WeldJoint.Admit` converts `UnitsNet` length, angle, temperature, and duration quantities once into canonical millimetre, degree, Celsius, and minute fields before `WeldJoint.Validate`. `WeldPolicy.Admit` validates process keys, role-band coverage, deposition laws, pass bounds, access constraints, and the procedure profile's `WeldDemandBinding` rows once. `WeldRequest.Validate` closes census identity and `ProcessBudget.Joining` invariants before planning. Interior operations consume only admitted owners.
- Derivation: `FillProfile.VolumeMm3` is the complete boundary-resolved deposit demand, including unequal fillet legs, contour, reinforcement, root opening and face, backing displacement, groove radii, variable section, plug or slot cavity, flare throat, side split, and repair excavation. `SectionStation.AreaMm2` derives from root width, face width, and height. `BeadProgram.Resolve` generates role and oscillation from deposited fraction, and `BeadProgram.Lattice` seats each bead across the layer width `FillProfile.WidthAtHeight` resolves at `FillProfile.HeightAtFill`. `Transport` carries the seam frame — tangent, lateral, normal — offsets its origin by admitted standoff, and resamples every `DepositSpan` boundary. `Weave` places the bead before work and travel rotation, so oscillation never bleeds into travel.
- Receipt: `WeldPass` retains the frozen scheduling fields and adds lattice placement, `CommandedFeedMmMin` scaled to hold seam progression through oscillation, `BeadEvidence`, `ArcProgram`, and `PassLineage`. `TorchFrame.StandoffMm` and its offset pose preserve setup geometry. `BeadEvidence` carries arc time, EN 1011-2 t8/5 cooling time, and deposit length beside bead geometry. `WeldPlan` retains passes, actions, demands, maximum heat input, bead count, and key. `WeldPlan.Project` returns execution, qualification, or receipt evidence through one closed egress family.
- Packages: Thinktecture.Runtime.Extensions supplies `[Union]`, `[SmartEnum<string>]`, and `[ComplexValueObject]`; LanguageExt.Core supplies `Fin`, `Validation`, `Option`, `Map`, `Set`, `Seq`, `Traverse`, `Apply`, `Fold`, `Choose`, and `Zip`; UnitsNet supplies typed boundary quantities and conversion; NodaTime supplies `Instant`; RhinoCommon supplies `Point3d`, `Vector3d`, `Plane`, `VectorAngle`, `CrossProduct`, `Unitize`, and `Rotate`; `Rasm.Fabrication.Process` supplies `ProcessBudget.Joining`, `Move`, `ContentKey`, and `EgressKind.WeldPlan`.
- Boundary: `Rasm.Materials` supplies material, penetration, and qualification keys; callers resolve preparation geometry into the local `FillProfile`. `Joining/sequence` alone orders deposits and cooling. `Joining/procedure` alone assesses `WeldPlan.Demands`. Kinematics alone turns `WeldPass.Path` and `WeldPass.Frames` into robot solutions. Cam alone conditions execution motion. `FillProfile.VolumeMm3`, `FillProfile.Fits`, and `Pass` are numerical fold kernels; `Transport`, `Pose`, and `Weave` are Rhino mutation kernels; `CanonicalBytes` is the BCL binary-encoding kernel. `Weld` never posts machine code.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------------------------------------------------------
using System.Runtime.InteropServices;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Domain;
using NodaTime;
using Rasm.Fabrication.Process;
using Rasm.Numerics;
using Rhino.Geometry;
using Thinktecture;
using UnitsNet;
using UnitsNet.Units;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Joining;

// --- [TYPES] --------------------------------------------------------------------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class PassRole {
    public static readonly PassRole Tack = new("tack", 0.35, 0.45, false, 1.00, 0.80, false);
    public static readonly PassRole Root = new("root", 0.55, 0.70, false, 1.00, 0.75, true);
    public static readonly PassRole HotPass = new("hot-pass", 0.70, 0.85, false, 1.00, 0.90, false);
    public static readonly PassRole Fill = new("fill", 1.00, 1.00, true, 1.00, 1.00, false);
    public static readonly PassRole Cap = new("cap", 0.80, 0.90, true, 1.00, 0.95, true);
    public static readonly PassRole Seal = new("seal", 0.50, 0.65, false, 1.00, 0.80, true);
    public static readonly PassRole Butter = new("butter", 0.75, 0.80, true, 0.00, 0.90, false);
    public static readonly PassRole Temper = new("temper", 0.55, 0.65, false, 0.00, 0.70, false);
    public static readonly PassRole Buildup = new("buildup", 0.90, 0.90, true, 1.00, 1.00, false);
    public static readonly PassRole Repair = new("repair", 0.65, 0.70, true, 1.00, 0.85, true);

    public double AreaFactor { get; }
    public double TravelFactor { get; }
    public bool OscillationAdmitted { get; }
    public double FillContribution { get; }
    public double CurrentFactor { get; }
    public bool HoldForInspection { get; }
}

[SmartEnum<string>]
public sealed partial class WeldPosition {
    public static readonly WeldPosition G1 = new("1g", 1.00, 1.00, 1.00, true);
    public static readonly WeldPosition G2 = new("2g", 0.90, 1.00, 0.95, true);
    public static readonly WeldPosition G3Up = new("3g-up", 0.60, 1.30, 0.65, true);
    public static readonly WeldPosition G3Down = new("3g-down", 1.05, 0.90, 0.85, false);
    public static readonly WeldPosition G4 = new("4g", 0.70, 1.15, 0.55, false);
    public static readonly WeldPosition G5Up = new("5g-up", 0.55, 1.35, 0.60, true);
    public static readonly WeldPosition G5Down = new("5g-down", 0.95, 1.00, 0.85, false);
    public static readonly WeldPosition G6 = new("6g", 0.50, 1.40, 0.50, true);
    public static readonly WeldPosition F1 = new("1f", 1.00, 1.00, 1.00, true);
    public static readonly WeldPosition F2 = new("2f", 0.90, 1.00, 0.95, true);
    public static readonly WeldPosition F3 = new("3f", 0.65, 1.25, 0.65, true);
    public static readonly WeldPosition F4 = new("4f", 0.70, 1.15, 0.55, false);

    public double TravelDerate { get; }
    public double CoolingScale { get; }
    public double DepositionDerate { get; }
    public bool OscillationAdmitted { get; }
}

[SmartEnum<string>]
public sealed partial class WeldStandard {
    public static readonly WeldStandard AwsD11 = new("aws-d1.1", 1.00, 2.50, 45.0, 10.0, 512, 1e-6, 1e-9);
    public static readonly WeldStandard Iso15614 = new("iso-15614", 1.20, 3.00, 45.0, 10.0, 512, 1e-6, 1e-9);
    public static readonly WeldStandard AsmeIx = new("asme-ix", 0.80, 2.20, 45.0, 5.0, 512, 1e-6, 1e-9);
    public static readonly WeldStandard Iso3834 = new("iso-3834", 1.00, 2.00, 45.0, 10.0, 512, 1e-6, 1e-9);

    public double TargetHeatInputKjMm { get; }
    public double HeatInputCapKjMm { get; }
    public double WorkAngleDeg { get; }
    public double TravelAngleDeg { get; }
    public int PassCap { get; }
    public double AbsoluteVolumeToleranceMm3 { get; }
    public double RelativeVolumeTolerance { get; }
}

[SmartEnum<string>]
public sealed partial class CavityKind {
    public static readonly CavityKind Plug = new("plug");
    public static readonly CavityKind Slot = new("slot");
}

[SmartEnum<string>]
public sealed partial class FlareKind {
    public static readonly FlareKind Bevel = new("flare-bevel");
    public static readonly FlareKind V = new("flare-v");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Waveform {
    private Waveform() { }

    public sealed record Harmonic(Arr<HarmonicTerm> Terms) : Waveform;
    public sealed record Piecewise(Arr<WaveKnot> Knots) : Waveform;

    public double Offset(double phase) => Switch(
        state: phase - Math.Floor(phase),
        harmonic: static (p, value) => value.Terms.Fold(0.0,
            (sum, term) => sum + term.Amplitude * Math.Sin((2.0 * Math.PI * term.Order * p) + term.PhaseRad)),
        piecewise: static (p, value) => value.Knots.Zip(value.Knots.Tail)
            .Choose(pair => p >= pair.Item1.Phase && p <= pair.Item2.Phase
                ? Some(pair.Item1.Offset + ((pair.Item2.Offset - pair.Item1.Offset)
                    * ((p - pair.Item1.Phase) / (pair.Item2.Phase - pair.Item1.Phase))))
                : None)
            .HeadOrNone()
            .IfNone(0.0));

    public bool Admitted => Switch(
        harmonic: static value => !value.Terms.IsEmpty && value.Terms.ForAll(static term => term != default)
            && value.Terms.Map(static term => term.Order).Distinct().Count == value.Terms.Count,
        piecewise: static value => value.Knots.Count >= 2 && value.Knots.ForAll(static knot => knot != default)
            && value.Knots.Head.Phase == 0.0 && value.Knots.Last.Phase == 1.0
            && value.Knots.Head.Offset == value.Knots.Last.Offset
            && value.Knots.Zip(value.Knots.Tail).ForAll(static pair => pair.Item1.Phase < pair.Item2.Phase));
}

[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct HarmonicTerm {
    public int Order { get; }
    public double Amplitude { get; }
    public double PhaseRad { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref int order,
        ref double amplitude,
        ref double phaseRad) =>
        validationError = order <= 0 || !double.IsFinite(amplitude) || !double.IsFinite(phaseRad)
            ? new ValidationError(message: "weld-harmonic")
            : null;
}

[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct WaveKnot {
    public double Phase { get; }
    public double Offset { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref double phase,
        ref double offset) =>
        validationError = !double.IsFinite(phase) || phase is < 0.0 or > 1.0 || !double.IsFinite(offset)
            ? new ValidationError(message: "weld-wave-knot")
            : null;
}

[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct WeavePattern {
    public Waveform Shape { get; }
    public double AmplitudeMm { get; }
    public double PitchMm { get; }
    public double EdgeDwellS { get; }
    public int TogglesPerCycle { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Waveform shape,
        ref double amplitudeMm,
        ref double pitchMm,
        ref double edgeDwellS,
        ref int togglesPerCycle) =>
        validationError = shape is null || !shape.Admitted || amplitudeMm < 0.0 || pitchMm <= 0.0 || edgeDwellS < 0.0
            || togglesPerCycle < 0 || (edgeDwellS > 0.0) != (togglesPerCycle > 0)
            || (amplitudeMm == 0.0 && edgeDwellS > 0.0)
            || Seq(amplitudeMm, pitchMm, edgeDwellS).Exists(static value => !double.IsFinite(value))
            ? new ValidationError(message: "weld-weave")
            : null;

    public double Offset(double stationMm) => AmplitudeMm * Shape.Offset(stationMm / PitchMm);

    public double DwellSeconds(double lengthMm) => EdgeDwellS * TogglesPerCycle * (lengthMm / PitchMm);
}

// --- [MODELS] -------------------------------------------------------------------------------------------------------------------------------------
[ComplexValueObject]
public sealed partial class FillProfile {
    public Arr<SectionStation> Stations { get; }
    public Arr<DepositSpan> Spans { get; }
    public double EffectiveThroatMm { get; }
    public double ReinforcementMm { get; }
    public double ToeRadiusMm { get; }
    public double VolumeMm3 => Spans.Fold(0.0, (sum, span) => {
        Seq<double> stations = Seq(span.StartMm)
            + Stations.Map(static row => row.StationMm).Filter(value => value > span.StartMm && value < span.EndMm)
            + Seq(span.EndMm);
        return sum + stations.Zip(stations.Tail).Fold(0.0, (volume, pair) => volume
            + (0.5 * (Section(pair.Item1).AreaMm2 + Section(pair.Item2).AreaMm2) * (pair.Item2 - pair.Item1)));
    });

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Arr<SectionStation> stations,
        ref Arr<DepositSpan> spans,
        ref double effectiveThroatMm,
        ref double reinforcementMm,
        ref double toeRadiusMm) =>
        validationError = effectiveThroatMm <= 0.0 || reinforcementMm < 0.0 || toeRadiusMm < 0.0
            || Seq(effectiveThroatMm, reinforcementMm, toeRadiusMm).Exists(static value => !double.IsFinite(value))
            || stations.IsEmpty || stations.Exists(static row => row == default)
            || spans.IsEmpty || spans.Exists(static row => row == default)
            || stations.Head.StationMm != 0.0
            || stations.Map(static row => row.StationMm).Distinct().Count != stations.Count
            || !stations.AsEnumerable().OrderBy(static row => row.StationMm).Select(static row => row.StationMm)
                .SequenceEqual(stations.Map(static row => row.StationMm))
            || spans.Zip(spans.Tail).Exists(static pair => pair.Item1.EndMm >= pair.Item2.StartMm)
            || spans.Last.EndMm > stations.Last.StationMm
            ? new ValidationError(message: "weld-fill-profile")
            : null;

    public double EnvelopeWidthMm => Stations.Map(static row => row.WidthMm).Fold(0.0, Math.Max);
    public double EnvelopeRootWidthMm => Stations.Map(static row => row.RootWidthMm).Fold(double.PositiveInfinity, Math.Min);
    public double EnvelopeHeightMm => Stations.Map(static row => row.HeightMm).Fold(0.0, Math.Max);
    public double DepositLengthMm => Spans.Fold(0.0, static (sum, span) => sum + span.EndMm - span.StartMm);

    public (double AreaMm2, double WidthMm, double RootWidthMm, double HeightMm) Section(double stationMm) => Stations
        .Zip(Stations.Tail)
        .Choose(pair => stationMm >= pair.Item1.StationMm && stationMm <= pair.Item2.StationMm
            ? Some((pair.Item1, pair.Item2))
            : None)
        .HeadOrNone()
        .Map(pair => {
            double t = (stationMm - pair.Item1.StationMm) / (pair.Item2.StationMm - pair.Item1.StationMm);
            return (
                pair.Item1.AreaMm2 + ((pair.Item2.AreaMm2 - pair.Item1.AreaMm2) * t),
                pair.Item1.WidthMm + ((pair.Item2.WidthMm - pair.Item1.WidthMm) * t),
                pair.Item1.RootWidthMm + ((pair.Item2.RootWidthMm - pair.Item1.RootWidthMm) * t),
                pair.Item1.HeightMm + ((pair.Item2.HeightMm - pair.Item1.HeightMm) * t));
        })
        .IfNone(stationMm <= Stations.Head.StationMm
            ? (Stations.Head.AreaMm2, Stations.Head.WidthMm, Stations.Head.RootWidthMm, Stations.Head.HeightMm)
            : (Stations.Last.AreaMm2, Stations.Last.WidthMm, Stations.Last.RootWidthMm, Stations.Last.HeightMm));

    // Envelope section is the trapezoid root -> face; a square groove degenerates to the constant-width arm.
    public double WidthAtHeight(double heightMm) => EnvelopeRootWidthMm
        + ((EnvelopeWidthMm - EnvelopeRootWidthMm) * Math.Clamp(heightMm / EnvelopeHeightMm, 0.0, 1.0));

    public double HeightAtFill(double fraction) {
        double taper = (EnvelopeWidthMm - EnvelopeRootWidthMm) / EnvelopeHeightMm;
        double area = 0.5 * (EnvelopeRootWidthMm + EnvelopeWidthMm) * EnvelopeHeightMm * Math.Clamp(fraction, 0.0, 1.0);
        return taper <= 0.0
            ? area / EnvelopeRootWidthMm
            : (Math.Sqrt((EnvelopeRootWidthMm * EnvelopeRootWidthMm) + (2.0 * taper * area)) - EnvelopeRootWidthMm) / taper;
    }

    public bool Fits(Arr<Point3d> seam) => Spans.Last.EndMm <= seam.Zip(seam.Tail)
        .Fold(0.0, static (sum, pair) => sum + pair.Item1.DistanceTo(pair.Item2));
}

[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct DepositSpan {
    public double StartMm { get; }
    public double EndMm { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref double startMm,
        ref double endMm) =>
        validationError = startMm < 0.0 || startMm >= endMm || !double.IsFinite(startMm) || !double.IsFinite(endMm)
            ? new ValidationError(message: "weld-deposit-span")
            : null;

    public bool Contains(double stationMm) => stationMm >= StartMm && stationMm <= EndMm;
}

[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct SectionStation {
    public double StationMm { get; }
    public double WidthMm { get; }
    public double RootWidthMm { get; }
    public double HeightMm { get; }
    public double AreaMm2 => 0.5 * (WidthMm + RootWidthMm) * HeightMm;

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref double stationMm,
        ref double widthMm,
        ref double rootWidthMm,
        ref double heightMm) =>
        validationError = stationMm < 0.0 || widthMm <= 0.0 || heightMm <= 0.0
            || rootWidthMm < 0.0 || rootWidthMm > widthMm
            || Seq(stationMm, widthMm, rootWidthMm, heightMm).Exists(static value => !double.IsFinite(value))
            ? new ValidationError(message: "weld-section-station")
            : null;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RootProgram {
    private RootProgram() { }

    public sealed record None : RootProgram;
    public sealed record Backing(string Key, bool RemoveAfterWeld) : RootProgram;
    public sealed record Backgouge(double DepthMm, int BeforeSide) : RootProgram;
    public sealed record BackingAndBackgouge(string Key, bool RemoveAfterWeld, double DepthMm, int BeforeSide) : RootProgram;
    public sealed record Seal(int Side) : RootProgram;

    public bool Admitted => Switch(
        none: static _ => true,
        backing: static value => !string.IsNullOrWhiteSpace(value.Key),
        backgouge: static value => value.DepthMm > 0.0 && double.IsFinite(value.DepthMm) && value.BeforeSide is 0 or 1,
        backingAndBackgouge: static value => !string.IsNullOrWhiteSpace(value.Key) && value.DepthMm > 0.0
            && double.IsFinite(value.DepthMm) && value.BeforeSide is 0 or 1,
        seal: static value => value.Side is 0 or 1);

    // Side the deposit opens on; a double-sided groove flips to its complement once the first side reaches half fill.
    public int FirstSide => Switch(
        none: static _ => 0,
        backing: static _ => 0,
        backgouge: static value => 1 - value.BeforeSide,
        backingAndBackgouge: static value => 1 - value.BeforeSide,
        seal: static value => 1 - value.Side);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record JointPrep {
    private JointPrep() { }

    public sealed record Groove(
        string GeometryKey,
        string PenetrationKey,
        FillProfile Fill,
        RootProgram Root,
        bool DoubleSided) : JointPrep;
    public sealed record Fillet(string ContourKey, FillProfile Fill, double LegAMm, double LegBMm) : JointPrep;
    public sealed record Cavity(CavityKind Kind, FillProfile Fill) : JointPrep;
    public sealed record Flare(FlareKind Kind, FillProfile Fill, double RadiusMm) : JointPrep;

    public FillProfile Demand => Switch(
        groove: static value => value.Fill,
        fillet: static value => value.Fill,
        cavity: static value => value.Fill,
        flare: static value => value.Fill);

    public string QualificationType => Switch(
        groove: static _ => "groove",
        fillet: static _ => "fillet",
        cavity: static value => value.Kind.Key,
        flare: static value => value.Kind.Key);

    // EN 1011-2 joint shape factors: sheet-plane (F2) and volumetric (F3) heat-flow correction per preparation.
    public (double Planar, double Spatial) ShapeFactors => Switch(
        groove: static value => value.DoubleSided ? (0.90, 0.90) : (1.00, 1.00),
        fillet: static _ => (0.90, 0.67),
        cavity: static _ => (0.67, 0.67),
        flare: static _ => (0.90, 0.67));

    public int FirstSide => Switch(
        groove: static value => value.Root.FirstSide,
        fillet: static _ => 0,
        cavity: static _ => 0,
        flare: static _ => 0);

    public bool DoubleSided => Switch(
        groove: static value => value.DoubleSided,
        fillet: static _ => false,
        cavity: static _ => false,
        flare: static _ => false);

    public bool Admitted(double thicknessMm) => Demand is not null && Demand.VolumeMm3 > 0.0 && Switch(
        state: thicknessMm,
        groove: static (thickness, value) => !string.IsNullOrWhiteSpace(value.GeometryKey)
            && !string.IsNullOrWhiteSpace(value.PenetrationKey) && value.Root is not null && value.Root.Admitted
            && value.Fill.EffectiveThroatMm <= thickness && (!value.DoubleSided || value.Root is not RootProgram.None),
        fillet: static (_, value) => !string.IsNullOrWhiteSpace(value.ContourKey) && value.LegAMm > 0.0 && value.LegBMm > 0.0
            && double.IsFinite(value.LegAMm) && double.IsFinite(value.LegBMm),
        cavity: static (_, value) => value.Kind is not null,
        flare: static (_, value) => value.Kind is not null && value.RadiusMm > 0.0
            && double.IsFinite(value.RadiusMm));
}

[ComplexValueObject]
public sealed partial class WeldJoint {
    public int Joint { get; }
    public Arr<Point3d> Seam { get; }
    public Arr<Vector3d> Normals { get; }
    public double NormalToleranceRad { get; }
    public JointPrep Prep { get; }
    public string ProcessKey { get; }
    public Option<string> FillerKey { get; }
    public Option<string> ShieldingKey { get; }
    public WeldPosition Position { get; }
    public Option<string> FillerClassificationKey { get; }
    public Option<string> FluxKey { get; }
    public string ProgressionKey { get; }
    public string CurrentTypeKey { get; }
    public string PolarityKey { get; }
    public Option<string> TransferModeKey { get; }
    public string PassTechniqueKey { get; }
    public double ElectrodeDiameterMm { get; }
    public double ThicknessMm { get; }
    public Option<double> DiameterMm { get; }
    public string MaterialGroupKey { get; }
    public double PreheatC { get; }
    public Option<double> PwhtC { get; }
    public Option<double> PwhtMinutes { get; }
    public bool ImpactDemanded { get; }
    public Set<string> QualificationContext { get; }
    public InspectionBasis Inspection { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref int joint,
        ref Arr<Point3d> seam,
        ref Arr<Vector3d> normals,
        ref double normalToleranceRad,
        ref JointPrep prep,
        ref string processKey,
        ref Option<string> fillerKey,
        ref Option<string> shieldingKey,
        ref WeldPosition position,
        ref Option<string> fillerClassificationKey,
        ref Option<string> fluxKey,
        ref string progressionKey,
        ref string currentTypeKey,
        ref string polarityKey,
        ref Option<string> transferModeKey,
        ref string passTechniqueKey,
        ref double electrodeDiameterMm,
        ref double thicknessMm,
        ref Option<double> diameterMm,
        ref string materialGroupKey,
        ref double preheatC,
        ref Option<double> pwhtC,
        ref Option<double> pwhtMinutes,
        ref bool impactDemanded,
        ref Set<string> qualificationContext,
        ref InspectionBasis inspection) =>
        validationError = joint < 0 || prep is null || position is null || seam.Count < 2 || seam.Count != normals.Count
            || seam.Exists(static point => !point.IsValid) || normals.Exists(static normal => !normal.IsValid || normal.IsZero)
            || seam.Zip(seam.Tail).Exists(static pair => pair.Item1.DistanceTo(pair.Item2) <= 0.0)
            || !double.IsFinite(normalToleranceRad) || normalToleranceRad <= 0.0 || normalToleranceRad > 0.5 * Math.PI
            || seam.Map((_, index) => (Tangent: Tangent(seam, index), Normal: normals[index]))
                .Exists(pair => pair.Tangent.IsZero
                    || !double.IsFinite(Vector3d.VectorAngle(pair.Tangent, pair.Normal))
                    || Math.Abs(Vector3d.VectorAngle(pair.Tangent, pair.Normal) - (0.5 * Math.PI)) > normalToleranceRad)
            || Seq(processKey, progressionKey, currentTypeKey, polarityKey, passTechniqueKey, materialGroupKey)
                .Exists(string.IsNullOrWhiteSpace)
            || Seq(fillerKey, shieldingKey, fillerClassificationKey, fluxKey, transferModeKey)
                .Exists(static value => value.Exists(string.IsNullOrWhiteSpace))
            || electrodeDiameterMm <= 0.0 || thicknessMm <= 0.0
            || diameterMm.Exists(static value => value <= 0.0 || !double.IsFinite(value))
            || preheatC < 0.0 || preheatC >= 500.0
            || Seq(electrodeDiameterMm, thicknessMm, preheatC).Exists(static value => !double.IsFinite(value))
            || !prep.Admitted(thicknessMm) || !prep.Demand.Fits(seam)
            || inspection == default || pwhtC.IsSome != pwhtMinutes.IsSome
            || pwhtC.Exists(static value => value <= 0.0 || !double.IsFinite(value))
            || pwhtMinutes.Exists(static value => value <= 0.0 || !double.IsFinite(value))
            ? new ValidationError(message: "weld-joint")
            : null;

    public static Fin<WeldJoint> Admit(
        int joint,
        Arr<Point3d> seam,
        Arr<Vector3d> normals,
        Angle normalTolerance,
        JointPrep prep,
        string processKey,
        Option<string> fillerKey,
        Option<string> shieldingKey,
        WeldPosition position,
        Option<string> fillerClassificationKey,
        Option<string> fluxKey,
        string progressionKey,
        string currentTypeKey,
        string polarityKey,
        Option<string> transferModeKey,
        string passTechniqueKey,
        Length electrodeDiameter,
        Length thickness,
        Option<Length> diameter,
        string materialGroupKey,
        Temperature preheat,
        Option<Temperature> pwht,
        Option<Duration> pwhtDuration,
        bool impactDemanded,
        Set<string> qualificationContext,
        InspectionBasis inspection) =>
        Validate(
            joint, seam, normals, normalTolerance.As(AngleUnit.Radian), prep, processKey, fillerKey, shieldingKey,
            position, fillerClassificationKey, fluxKey,
            progressionKey, currentTypeKey, polarityKey, transferModeKey, passTechniqueKey,
            electrodeDiameter.As(LengthUnit.Millimeter), thickness.As(LengthUnit.Millimeter),
            diameter.Map(static value => value.As(LengthUnit.Millimeter)), materialGroupKey,
            preheat.DegreesCelsius, pwht.Map(static value => value.DegreesCelsius),
            pwhtDuration.Map(static value => value.Minutes), impactDemanded, qualificationContext, inspection,
            out WeldJoint admitted) is { } error
                ? Fin.Fail<WeldJoint>(new GeometryFault.DegenerateInput(Kind.Curve, -1, error.Message).ToError())
                : Fin.Succ(admitted);

    internal static Vector3d Tangent(Arr<Point3d> seam, int index) => index switch {
        0 => seam[1] - seam[0],
        _ when index == seam.Count - 1 => seam[index] - seam[index - 1],
        _ => seam[index + 1] - seam[index - 1],
    };
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DepositionSource {
    private DepositionSource() { }

    public sealed record SolidWire(double DiameterMm, int Count, double SpacingMm, double Yield) : DepositionSource;
    public sealed record CoredWire(
        double OuterDiameterMm,
        double FillFraction,
        int Count,
        double SpacingMm,
        double Yield) : DepositionSource;
    public sealed record Rod(double AreaMm2, double FeedMmMin, double Yield) : DepositionSource;
    public sealed record Strip(double WidthMm, double ThicknessMm, double FeedMmMin, double Yield) : DepositionSource;
    public sealed record Powder(double Mm3Min, double Capture, double CharacteristicWidthMm) : DepositionSource;
    public sealed record Volumetric(double Mm3Min, double CharacteristicWidthMm) : DepositionSource;
    public sealed record Autogenous(double FusedAreaMm2) : DepositionSource;

    public double Rate(ProcessBudget.Joining budget) => Switch(
        state: budget,
        solidWire: static (joined, value) => 0.25 * Math.PI * value.DiameterMm * value.DiameterMm
            * value.Count * joined.WireFeedRate * value.Yield,
        coredWire: static (joined, value) => 0.25 * Math.PI * value.OuterDiameterMm * value.OuterDiameterMm
            * value.FillFraction * value.Count * joined.WireFeedRate * value.Yield,
        rod: static (_, value) => value.AreaMm2 * value.FeedMmMin * value.Yield,
        strip: static (_, value) => value.WidthMm * value.ThicknessMm * value.FeedMmMin * value.Yield,
        powder: static (_, value) => value.Mm3Min * value.Capture,
        volumetric: static (_, value) => value.Mm3Min,
        autogenous: static (joined, value) => value.FusedAreaMm2 * joined.TravelSpeed);

    public double Width => Switch(
        solidWire: static value => value.DiameterMm + ((value.Count - 1) * value.SpacingMm),
        coredWire: static value => value.OuterDiameterMm + ((value.Count - 1) * value.SpacingMm),
        rod: static value => Math.Sqrt(value.AreaMm2),
        strip: static value => value.WidthMm,
        powder: static value => value.CharacteristicWidthMm,
        volumetric: static value => value.CharacteristicWidthMm,
        autogenous: static value => Math.Sqrt(value.FusedAreaMm2));

    public double FillerLength(double depositedMm3) => Switch(
        state: depositedMm3,
        solidWire: static (deposited, value) => deposited
            / (0.25 * Math.PI * value.DiameterMm * value.DiameterMm * value.Count * value.Yield),
        coredWire: static (deposited, value) => deposited
            / (0.25 * Math.PI * value.OuterDiameterMm * value.OuterDiameterMm
                * value.FillFraction * value.Count * value.Yield),
        rod: static (deposited, value) => deposited / (value.AreaMm2 * value.Yield),
        strip: static (deposited, value) => deposited / (value.WidthMm * value.ThicknessMm * value.Yield),
        powder: static (_, _) => 0.0,
        volumetric: static (_, _) => 0.0,
        autogenous: static (_, _) => 0.0);

    public bool Admitted => Switch(
        solidWire: static value => value.DiameterMm > 0.0 && value.Count > 0 && value.SpacingMm >= 0.0
            && (value.Count > 1 || value.SpacingMm == 0.0) && value.Yield is > 0.0 and <= 1.0
            && Seq(value.DiameterMm, value.SpacingMm, value.Yield).ForAll(double.IsFinite),
        coredWire: static value => value.OuterDiameterMm > 0.0 && value.FillFraction is > 0.0 and <= 1.0
            && value.Count > 0 && value.SpacingMm >= 0.0 && (value.Count > 1 || value.SpacingMm == 0.0)
            && value.Yield is > 0.0 and <= 1.0
            && Seq(value.OuterDiameterMm, value.FillFraction, value.SpacingMm, value.Yield).ForAll(double.IsFinite),
        rod: static value => value.AreaMm2 > 0.0 && value.FeedMmMin > 0.0 && value.Yield is > 0.0 and <= 1.0
            && Seq(value.AreaMm2, value.FeedMmMin, value.Yield).ForAll(double.IsFinite),
        strip: static value => value.WidthMm > 0.0 && value.ThicknessMm > 0.0 && value.FeedMmMin > 0.0
            && value.Yield is > 0.0 and <= 1.0
            && Seq(value.WidthMm, value.ThicknessMm, value.FeedMmMin, value.Yield).ForAll(double.IsFinite),
        powder: static value => value.Mm3Min > 0.0 && value.Capture is > 0.0 and <= 1.0
            && value.CharacteristicWidthMm > 0.0
            && Seq(value.Mm3Min, value.Capture, value.CharacteristicWidthMm).ForAll(double.IsFinite),
        volumetric: static value => value.Mm3Min > 0.0 && value.CharacteristicWidthMm > 0.0
            && double.IsFinite(value.Mm3Min) && double.IsFinite(value.CharacteristicWidthMm),
        autogenous: static value => value.FusedAreaMm2 > 0.0 && double.IsFinite(value.FusedAreaMm2));
}

[ComplexValueObject]
public sealed partial class ArcMode {
    public double Efficiency { get; }
    public double TravelLowMmMin { get; }
    public double TravelHighMmMin { get; }
    public double HeatInputLowKjMm { get; }
    public double HeatInputHighKjMm { get; }
    public double CoolingLowS { get; }
    public double CoolingHighS { get; }
    public double CurrentCapA { get; }
    public Set<string> Polarities { get; }
    public Set<string> CurrentTypes { get; }
    public Set<string> Progressions { get; }
    public Set<string> Techniques { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref double efficiency,
        ref double travelLowMmMin,
        ref double travelHighMmMin,
        ref double heatInputLowKjMm,
        ref double heatInputHighKjMm,
        ref double coolingLowS,
        ref double coolingHighS,
        ref double currentCapA,
        ref Set<string> polarities,
        ref Set<string> currentTypes,
        ref Set<string> progressions,
        ref Set<string> techniques) =>
        validationError = efficiency is <= 0.0 or > 1.0 || travelLowMmMin <= 0.0 || travelLowMmMin > travelHighMmMin
            || heatInputLowKjMm <= 0.0 || heatInputLowKjMm > heatInputHighKjMm
            || coolingLowS <= 0.0 || coolingLowS > coolingHighS || currentCapA <= 0.0
            || Seq(efficiency, travelLowMmMin, travelHighMmMin, heatInputLowKjMm, heatInputHighKjMm,
                coolingLowS, coolingHighS, currentCapA).Exists(static value => !double.IsFinite(value))
            || Seq(polarities, currentTypes, progressions, techniques)
                .Exists(static row => row.IsEmpty || row.Exists(string.IsNullOrWhiteSpace))
            ? new ValidationError(message: "weld-arc-mode")
            : null;

    public Validation<Error, Unit> Admits(WeldJoint joint) => (
            Gate(Polarities, joint.PolarityKey, "polarity", joint.Joint),
            Gate(CurrentTypes, joint.CurrentTypeKey, "current-type", joint.Joint),
            Gate(Progressions, joint.ProgressionKey, "progression", joint.Joint),
            Gate(Techniques, joint.PassTechniqueKey, "technique", joint.Joint))
        .Apply(static (_, _, _, _) => unit)
        .As();

    public Fin<double> Travel(double requestedMmMin, int joint) =>
        !double.IsFinite(requestedMmMin) || requestedMmMin < TravelLowMmMin
            ? Fin.Fail<double>(new GeometryFault.DegenerateInput(
                Kind.Curve,
                -1,
                $"weld-plan:travel-floor:{joint}:{requestedMmMin:R}").ToError())
            : Fin.Succ(Math.Min(requestedMmMin, TravelHighMmMin));

    private static K<Validation<Error>, Unit> Gate(Set<string> admitted, string key, string axis, int joint) =>
        admitted.Contains(key)
            ? Validation<Error, Unit>.Success(unit)
            : Validation<Error, Unit>.Fail(
                new GeometryFault.DegenerateInput(Kind.Curve, -1, $"weld-arc-mode:{axis}:{joint}:{key}").ToError());
}

[ComplexValueObject]
public sealed partial class WeldProcessLaw {
    public DepositionSource Deposition { get; }
    public string DefaultModeKey { get; }
    public Map<string, ArcMode> Modes { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref DepositionSource deposition,
        ref string defaultModeKey,
        ref Map<string, ArcMode> modes) =>
        validationError = deposition is null || !deposition.Admitted || string.IsNullOrWhiteSpace(defaultModeKey)
            || modes.IsEmpty || modes.Keys.Exists(string.IsNullOrWhiteSpace)
            || modes.Values.Exists(static value => value is null) || !modes.ContainsKey(defaultModeKey)
            ? new ValidationError(message: "weld-process-law")
            : null;

    public Fin<ArcMode> Mode(WeldJoint joint) => joint.TransferModeKey
        .Match(
            Some: key => Modes.Find(key)
                .ToFin(new GeometryFault.DegenerateInput(Kind.Curve, -1, $"weld-plan:transfer-mode:{joint.Joint}:{key}").ToError()),
            None: () => Modes.Find(DefaultModeKey)
                .ToFin(new GeometryFault.DegenerateInput(Kind.Curve, -1, $"weld-plan:default-mode:{DefaultModeKey}").ToError()))
        .Bind(mode => mode.Admits(joint).ToFin().Map(_ => mode));
}

[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct RoleBand {
    public double StartFraction { get; }
    public double EndFraction { get; }
    public PassRole Role { get; }
    public WeavePattern Weave { get; }
    public ArcProgram Arc { get; }
    public PassLineage Lineage { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref double startFraction,
        ref double endFraction,
        ref PassRole role,
        ref WeavePattern weave,
        ref ArcProgram arc,
        ref PassLineage lineage) =>
        validationError = role is null || weave == default || arc == default || lineage is null || !lineage.Admitted
            || !double.IsFinite(startFraction) || !double.IsFinite(endFraction)
            || startFraction < 0.0 || endFraction > 1.0 || startFraction >= endFraction
            || (!role.OscillationAdmitted && weave.AmplitudeMm > 0.0)
            || (role == PassRole.Repair) != (lineage is PassLineage.Repair)
            || (role == PassRole.Temper) != (lineage is PassLineage.Temper)
            ? new ValidationError(message: "weld-role-band")
            : null;
}

[ComplexValueObject]
public sealed partial class BeadProgram {
    public Seq<RoleBand> Bands { get; }
    public Seq<RoleBand> Overlay { get; }
    public double OverlapFraction { get; }
    public double WidthFactor { get; }
    public double HeightFactor { get; }

    // Fill bands must advance the groove ledger, so a zero-contribution role (butter, temper) rides the overlay
    // and deposits once after closure; interleaving it into Bands would stall the fill fold against the pass cap.
    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Seq<RoleBand> bands,
        ref Seq<RoleBand> overlay,
        ref double overlapFraction,
        ref double widthFactor,
        ref double heightFactor) =>
        validationError = bands.IsEmpty || bands.Exists(static row => row == default)
            || bands.Head.StartFraction != 0.0 || bands.Last.EndFraction != 1.0
            || bands.Zip(bands.Tail).Exists(static pair => pair.Item1.EndFraction != pair.Item2.StartFraction)
            || bands.Exists(static row => row.Role.FillContribution <= 0.0)
            || overlay.Exists(static row => row == default || row.Role.FillContribution > 0.0)
            || overlapFraction is < 0.0 or >= 1.0 || widthFactor <= 0.0 || heightFactor <= 0.0
            || Seq(overlapFraction, widthFactor, heightFactor).Exists(static value => !double.IsFinite(value))
            ? new ValidationError(message: "weld-bead-program")
            : null;

    public RoleBand Resolve(double fraction) => Bands.Find(row => fraction >= row.StartFraction && fraction < row.EndFraction)
        .IfNone(Bands.Last);

    // One layer carries as many overlapped beads as its section width admits; bead 0 sits at the left toe.
    public (int BeadsInLayer, double LateralOffsetMm) Lattice(double layerWidthMm, double beadWidthMm, int bead) {
        int count = Math.Max(1, (int)Math.Ceiling(layerWidthMm / (beadWidthMm * (1.0 - OverlapFraction))));
        return (count, (-0.5 * layerWidthMm) + ((Math.Min(bead, count - 1) + 0.5) * (layerWidthMm / count)));
    }
}

public interface IWeldAccess {
    string Key { get; }
    Validation<Error, Unit> Check(WeldJoint joint, Seq<WeldPass> passes);

    public static Fin<IWeldAccess> Admit(
        string key,
        Func<WeldJoint, Seq<WeldPass>, Validation<Error, Unit>> check) =>
        WeldAccess.Validate(key, check, out WeldAccess access) is { } error
            ? Fin.Fail<IWeldAccess>(new GeometryFault.DegenerateInput(Kind.Curve, -1, error.Message).ToError())
            : Fin.Succ<IWeldAccess>(access);
}

[ComplexValueObject]
internal sealed partial class WeldAccess : IWeldAccess {
    public string Key { get; }
    public Func<WeldJoint, Seq<WeldPass>, Validation<Error, Unit>> Constraint { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref string key,
        ref Func<WeldJoint, Seq<WeldPass>, Validation<Error, Unit>> constraint) =>
        validationError = string.IsNullOrWhiteSpace(key) || constraint is null
            ? new ValidationError(message: "weld-access")
            : null;

    public Validation<Error, Unit> Check(WeldJoint joint, Seq<WeldPass> passes) =>
        Try.lift<Validation<Error, Unit>>(() => Constraint(joint, passes))
            .Run()
            .Match(
                Succ: static result => result,
                Fail: error => Validation<Error, Unit>.Fail(new GeometryFault.DegenerateInput(
                    Kind.Curve,
                    -1,
                    $"weld-access:{Key}:{error.Message}").ToError()));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record WeldDemandBinding {
    private WeldDemandBinding() { }

    public sealed record Facts(
        WeldJoint Joint,
        ProcessBudget.Joining Budget,
        Seq<WeldPass> Passes,
        double MaxHeatInputKjMm);

    public sealed record Quantity(
        EssentialVariable Variable,
        Func<Facts, Option<IQuantity>> Project) : WeldDemandBinding;

    public sealed record Categorical(
        EssentialVariable Variable,
        Func<Facts, Option<string>> Project) : WeldDemandBinding;

    public sealed record Boolean(
        EssentialVariable Variable,
        Func<Facts, Option<bool>> Project) : WeldDemandBinding;

    public sealed record Temporal(
        EssentialVariable Variable,
        Func<Facts, Option<Instant>> Project) : WeldDemandBinding;

    public EssentialVariable Field => Switch(
        quantity: static value => value.Variable,
        categorical: static value => value.Variable,
        boolean: static value => value.Variable,
        temporal: static value => value.Variable);

    public bool Admitted => Field is not null && Switch(
        quantity: static value => value.Variable.Modality == VariableModality.Quantity && value.Project is not null,
        categorical: static value => value.Variable.Modality == VariableModality.Categorical && value.Project is not null,
        boolean: static value => value.Variable.Modality == VariableModality.Boolean && value.Project is not null,
        temporal: static value => value.Variable.Modality == VariableModality.Temporal && value.Project is not null);

    public Fin<QualificationValue> Resolve(Facts facts) => Switch(
        state: facts,
        quantity: static (state, binding) => Resolved(
            state, binding.Variable, binding.Project(state), static value => new QualificationValue.Quantity(value)),
        categorical: static (state, binding) => Resolved(
            state, binding.Variable, binding.Project(state), static value => new QualificationValue.Categorical(value)),
        boolean: static (state, binding) => Resolved(
            state, binding.Variable, binding.Project(state), static value => new QualificationValue.Boolean(value)),
        temporal: static (state, binding) => Resolved(
            state, binding.Variable, binding.Project(state), static value => new QualificationValue.Temporal(value)));

    private static Fin<QualificationValue> Resolved<T>(
        Facts facts,
        EssentialVariable variable,
        Option<T> projected,
        Func<T, QualificationValue> wrap) =>
        variable.Applicability.Exists(law => !law.Matches(facts.Joint.QualificationContext))
            ? Fin.Succ<QualificationValue>(new QualificationValue.ContextExcluded())
            : projected.Match(
                Some: value => Fin.Succ(wrap(value)),
                None: () => variable.Requirement.EvidenceRequired
                    ? Fin.Fail<QualificationValue>(new GeometryFault.DegenerateInput(
                        Kind.Curve,
                        -1,
                        $"weld-demand:required:{variable.Key}").ToError())
                    : Fin.Succ<QualificationValue>(new QualificationValue.EvidenceOmitted()));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record WeldPolicySource {
    private WeldPolicySource() { }

    public sealed record Defined(
        double TargetHeatInputKjMm,
        double HeatInputCapKjMm,
        Angle WorkAngle,
        Angle TravelAngle,
        int PassCap,
        Volume AbsoluteVolumeTolerance,
        double RelativeVolumeTolerance) : WeldPolicySource;

    public sealed record Canonical(WeldStandard Standard) : WeldPolicySource;
}

[ComplexValueObject]
public sealed partial class WeldPolicy {
    public double TargetHeatInputKjMm { get; }
    public double HeatInputCapKjMm { get; }
    public double WorkAngleDeg { get; }
    public double TravelAngleDeg { get; }
    public int PassCap { get; }
    public double AbsoluteVolumeToleranceMm3 { get; }
    public double RelativeVolumeTolerance { get; }
    public BeadProgram Beads { get; }
    public Map<string, WeldProcessLaw> Processes { get; }
    public Seq<IWeldAccess> Access { get; }
    public Seq<WeldDemandBinding> DemandBindings { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref double targetHeatInputKjMm,
        ref double heatInputCapKjMm,
        ref double workAngleDeg,
        ref double travelAngleDeg,
        ref int passCap,
        ref double absoluteVolumeToleranceMm3,
        ref double relativeVolumeTolerance,
        ref BeadProgram beads,
        ref Map<string, WeldProcessLaw> processes,
        ref Seq<IWeldAccess> access,
        ref Seq<WeldDemandBinding> demandBindings) =>
        validationError = targetHeatInputKjMm <= 0.0 || heatInputCapKjMm < targetHeatInputKjMm
            || workAngleDeg is < 0.0 or > 180.0 || Math.Abs(travelAngleDeg) > 90.0 || passCap <= 0
            || absoluteVolumeToleranceMm3 <= 0.0 || relativeVolumeTolerance <= 0.0
            || Seq(targetHeatInputKjMm, heatInputCapKjMm, workAngleDeg, travelAngleDeg,
                absoluteVolumeToleranceMm3, relativeVolumeTolerance)
                .Exists(static value => !double.IsFinite(value))
            || beads is null || processes.IsEmpty || processes.Keys.Exists(string.IsNullOrWhiteSpace)
            || processes.Values.Exists(static value => value is null) || access.IsEmpty
            || access.Exists(static value => value is null) || access.Map(static value => value.Key).Distinct().Count != access.Count
            || demandBindings.IsEmpty || demandBindings.Exists(static value => value is null || !value.Admitted)
            || demandBindings.Map(static value => value.Field).Distinct().Count != demandBindings.Count
            ? new ValidationError(message: "weld-policy")
            : null;

    public static Fin<WeldPolicy> Admit(
        WeldPolicySource source,
        BeadProgram beads,
        Map<string, WeldProcessLaw> processes,
        Seq<IWeldAccess> access,
        Seq<WeldDemandBinding> demandBindings) => Optional(source)
        .ToFin(new GeometryFault.DegenerateInput(Kind.Curve, -1, "weld-policy-source").ToError())
        .Map(input => input.Switch(
            defined: static value => (
                Target: value.TargetHeatInputKjMm,
                Cap: value.HeatInputCapKjMm,
                Work: value.WorkAngle.Degrees,
                Travel: value.TravelAngle.Degrees,
                Passes: value.PassCap,
                Absolute: value.AbsoluteVolumeTolerance.As(VolumeUnit.CubicMillimeter),
                Relative: value.RelativeVolumeTolerance),
            canonical: static value => (
                Target: value.Standard.TargetHeatInputKjMm,
                Cap: value.Standard.HeatInputCapKjMm,
                Work: value.Standard.WorkAngleDeg,
                Travel: value.Standard.TravelAngleDeg,
                Passes: value.Standard.PassCap,
                Absolute: value.Standard.AbsoluteVolumeToleranceMm3,
                Relative: value.Standard.RelativeVolumeTolerance)))
        .Bind(values => Validate(
            values.Target,
            values.Cap,
            values.Work,
            values.Travel,
            values.Passes,
            values.Absolute,
            values.Relative,
            beads,
            processes,
            access,
            demandBindings,
            out WeldPolicy admitted) is { } error
                ? Fin.Fail<WeldPolicy>(new GeometryFault.DegenerateInput(Kind.Curve, -1, error.Message).ToError())
                : Fin.Succ(admitted));
}

[ComplexValueObject]
public sealed partial class WeldRequest {
    public Seq<WeldJoint> Joints { get; }
    public WeldPolicy Policy { get; }
    public ProcessBudget.Joining Budget { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Seq<WeldJoint> joints,
        ref WeldPolicy policy,
        ref ProcessBudget.Joining budget) =>
        validationError = joints.IsEmpty || joints.Exists(static joint => joint is null) || policy is null || budget is null
            || joints.Map(static joint => joint.Joint).Distinct().Count != joints.Count
            || budget.CurrentA <= 0.0 || budget.VoltageV <= 0.0 || budget.WireFeedRate <= 0.0
            || budget.TravelSpeed <= 0.0 || budget.Standoff <= 0.0 || budget.InterpassTemp <= 0.0
            || Seq(budget.CurrentA, budget.VoltageV, budget.WireFeedRate, budget.TravelSpeed,
                budget.Standoff, budget.InterpassTemp).Exists(static value => !double.IsFinite(value))
            ? new ValidationError(message: "weld-request")
            : null;

    // Only fill-contributing roles close the groove; buttering and temper metal lands outside it, and every
    // repair excavation re-opens demand, so the ledger balances contribution against required plus excavated.
    public Fin<Unit> Coverage(Seq<WeldPass> passes) => Joints.Map(joint => (
            Joint: joint.Joint,
            Required: joint.Prep.Demand.VolumeMm3 + passes.Filter(pass => pass.Joint == joint.Joint)
                .Map(static pass => pass.Lineage.ExcavatedMm3).Sum(),
            Deposited: passes.Filter(pass => pass.Joint == joint.Joint)
                .Map(static pass => pass.Deposit.DepositedVolumeMm3 * pass.Role.FillContribution).Sum()))
        .Map(row => Math.Abs(row.Required - row.Deposited) <= Math.Max(
                Policy.AbsoluteVolumeToleranceMm3,
                row.Required * Policy.RelativeVolumeTolerance)
            ? Fin.Succ(unit).ToValidation()
            : Fin.Fail<Unit>(new GeometryFault.DegenerateInput(Kind.Curve, -1, $"weld-plan:coverage:{row.Joint}").ToError()).ToValidation())
        .Traverse(identity)
        .As()
        .ToFin()
        .Map(static _ => unit);
}

[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct ArcProgram {
    public double RunInMm { get; }
    public double BackstepMm { get; }
    public double CraterFillS { get; }
    public double RunOutMm { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref double runInMm,
        ref double backstepMm,
        ref double craterFillS,
        ref double runOutMm) =>
        validationError = Seq(runInMm, backstepMm, craterFillS, runOutMm)
            .Exists(static value => !double.IsFinite(value) || value < 0.0)
            ? new ValidationError(message: "weld-arc-program")
            : null;

    public Seq<Move> Apply(Seq<TorchFrame> run, double feedMmMin) {
        TorchFrame first = run.Head;
        TorchFrame last = run.Last;
        Point3d start = first.Pose.Origin - (RunInMm * first.Pose.XAxis);
        return Seq<Move>(new Move.Rapid(start))
            + (RunInMm > 0.0
                ? Seq<Move>(new Move.Linear(first.Pose.Origin, feedMmMin))
                : Seq<Move>())
            + (BackstepMm > 0.0
                ? Seq<Move>(
                    new Move.Linear(first.Pose.Origin + (BackstepMm * first.Pose.XAxis), feedMmMin),
                    new Move.Linear(first.Pose.Origin, feedMmMin))
                : Seq<Move>())
            + run.Tail.Map(frame => (Move)new Move.Linear(frame.Pose.Origin, feedMmMin))
            + (RunOutMm > 0.0
                ? Seq<Move>(new Move.Linear(last.Pose.Origin + (RunOutMm * last.Pose.XAxis), feedMmMin))
                : Seq<Move>());
    }

    public double ArcTime(Seq<Move> path) => path.Zip(path.Tail).Fold(
        CraterFillS,
        static (seconds, pair) => pair.Item2 is Move.Linear linear
            ? seconds + (60.0 * pair.Item1.Target.DistanceTo(linear.Target) / linear.Feed)
            : seconds);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PassLineage {
    private PassLineage() { }

    public sealed record Planned : PassLineage;
    public sealed record Repair(int ReplacesOrdinal, string DefectKey, double ExcavatedMm3) : PassLineage;
    public sealed record Temper(int ConditionsOrdinal) : PassLineage;

    public bool Admitted => Switch(
        planned: static _ => true,
        repair: static value => value.ReplacesOrdinal >= 0 && !string.IsNullOrWhiteSpace(value.DefectKey)
            && value.ExcavatedMm3 > 0.0 && double.IsFinite(value.ExcavatedMm3),
        temper: static value => value.ConditionsOrdinal >= 0);

    // Excavated metal re-opens fill demand, so the coverage ledger balances against required plus every excavation.
    public double ExcavatedMm3 => Switch(
        planned: static _ => 0.0,
        repair: static value => value.ExcavatedMm3,
        temper: static _ => 0.0);

    public bool Resolves(int joint, Seq<WeldPass> emitted) => Switch(
        state: (Joint: joint, Emitted: emitted),
        planned: static (_, _) => true,
        repair: static (state, value) => state.Emitted.Exists(
            pass => pass.Joint == state.Joint && pass.Ordinal == value.ReplacesOrdinal),
        temper: static (state, value) => state.Emitted.Exists(
            pass => pass.Joint == state.Joint && pass.Ordinal == value.ConditionsOrdinal));
}

public readonly record struct TorchFrame(
    int Joint,
    int Side,
    int Waypoint,
    double StationMm,
    Plane Pose,
    double WorkAngleDeg,
    double TravelAngleDeg,
    double Phase,
    double LateralOffsetMm,
    double StandoffMm) {
    public TorchFrame Opposed() => this with {
        Side = 1,
        Pose = new Plane(Pose.Origin, Pose.XAxis, -Pose.YAxis),
    };
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record JointAction {
    private JointAction() { }

    public sealed record PrepareGroove(
        int Joint,
        string GeometryKey,
        string PenetrationKey,
        FillProfile Profile,
        bool DoubleSided) : JointAction;
    public sealed record InstallBacking(int Joint, string BackingKey) : JointAction;
    public sealed record Backgouge(int Joint, int BeforeSide, double DepthMm) : JointAction;
    public sealed record RemoveBacking(int Joint, string BackingKey) : JointAction;
    public sealed record Preheat(int Joint, double TargetC, double InterpassCapC) : JointAction;
    public sealed record PostWeldHeatTreat(int Joint, double SoakC, double SoakMinutes) : JointAction;

    public int Joint => Switch(
        prepareGroove: static value => value.Joint,
        installBacking: static value => value.Joint,
        backgouge: static value => value.Joint,
        removeBacking: static value => value.Joint,
        preheat: static value => value.Joint,
        postWeldHeatTreat: static value => value.Joint);

    public bool Admitted => Joint >= 0 && Switch(
        prepareGroove: static value => !string.IsNullOrWhiteSpace(value.GeometryKey)
            && !string.IsNullOrWhiteSpace(value.PenetrationKey) && value.Profile is not null,
        installBacking: static value => !string.IsNullOrWhiteSpace(value.BackingKey),
        backgouge: static value => value.BeforeSide is 0 or 1 && value.DepthMm > 0.0 && double.IsFinite(value.DepthMm),
        removeBacking: static value => !string.IsNullOrWhiteSpace(value.BackingKey),
        preheat: static value => value.TargetC >= 0.0 && value.InterpassCapC >= value.TargetC
            && double.IsFinite(value.TargetC) && double.IsFinite(value.InterpassCapC),
        postWeldHeatTreat: static value => value.SoakC > 0.0 && value.SoakMinutes > 0.0
            && double.IsFinite(value.SoakC) && double.IsFinite(value.SoakMinutes));
}

[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct BeadEvidence {
    public double DepositedVolumeMm3 { get; }
    public double BeadAreaMm2 { get; }
    public double WidthMm { get; }
    public double HeightMm { get; }
    public double EnergyJ { get; }
    public double FillerLengthMm { get; }
    public double CoverageFraction { get; }
    public double ArcTimeS { get; }
    public double CoolingTimeS { get; }
    public double DepositLengthMm { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref double depositedVolumeMm3,
        ref double beadAreaMm2,
        ref double widthMm,
        ref double heightMm,
        ref double energyJ,
        ref double fillerLengthMm,
        ref double coverageFraction,
        ref double arcTimeS,
        ref double coolingTimeS,
        ref double depositLengthMm) =>
        validationError = depositedVolumeMm3 <= 0.0 || beadAreaMm2 <= 0.0 || widthMm <= 0.0 || heightMm <= 0.0
            || energyJ <= 0.0 || fillerLengthMm < 0.0 || coverageFraction is <= 0.0 or > 1.0
            || arcTimeS <= 0.0 || coolingTimeS <= 0.0 || depositLengthMm <= 0.0
            || Seq(depositedVolumeMm3, beadAreaMm2, widthMm, heightMm, energyJ, fillerLengthMm, coverageFraction,
                arcTimeS, coolingTimeS, depositLengthMm).Exists(static value => !double.IsFinite(value))
            ? new ValidationError(message: "weld-bead-evidence")
            : null;

    // EN 1011-2 t8/5: the thicker arm governs above the transition thickness, the sheet arm below it.
    public static double CoolingTime(
        double heatInputKjMm,
        double preheatC,
        double thicknessMm,
        (double Planar, double Spatial) shape,
        double positionScale) {
        double planar = (4300.0 - (4.3 * preheatC)) * 1e5 * Math.Pow(heatInputKjMm / thicknessMm, 2.0)
            * ((1.0 / Math.Pow(500.0 - preheatC, 2.0)) - (1.0 / Math.Pow(800.0 - preheatC, 2.0))) * shape.Planar;
        double spatial = (6700.0 - (5.0 * preheatC)) * heatInputKjMm
            * ((1.0 / (500.0 - preheatC)) - (1.0 / (800.0 - preheatC))) * shape.Spatial;
        return Math.Max(planar, spatial) * positionScale;
    }
}

[ComplexValueObject]
public sealed partial class WeldPass {
    public int Joint { get; }
    public PassRole Role { get; }
    public int Layer { get; }
    public int Bead { get; }
    public int BeadsInLayer { get; }
    public int Side { get; }
    public int Ordinal { get; }
    public WeavePattern Weave { get; }
    public int EdgeDwellMs { get; }
    public WeldPosition Position { get; }
    public double LateralOffsetMm { get; }
    public double HeightOffsetMm { get; }
    public double TravelMmMin { get; }
    public double CommandedFeedMmMin { get; }
    public double HeatInputKjMm { get; }
    public double ThicknessMm { get; }
    public Seq<Move> Path { get; }
    public Seq<TorchFrame> Frames { get; }
    public BeadEvidence Deposit { get; }
    public ArcProgram Arc { get; }
    public PassLineage Lineage { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref int joint,
        ref PassRole role,
        ref int layer,
        ref int bead,
        ref int beadsInLayer,
        ref int side,
        ref int ordinal,
        ref WeavePattern weave,
        ref int edgeDwellMs,
        ref WeldPosition position,
        ref double lateralOffsetMm,
        ref double heightOffsetMm,
        ref double travelMmMin,
        ref double commandedFeedMmMin,
        ref double heatInputKjMm,
        ref double thicknessMm,
        ref Seq<Move> path,
        ref Seq<TorchFrame> frames,
        ref BeadEvidence deposit,
        ref ArcProgram arc,
        ref PassLineage lineage) =>
        validationError = joint < 0 || role is null || layer < 0 || side is < 0 or > 1 || ordinal < 0
            || bead < 0 || beadsInLayer <= 0 || bead >= beadsInLayer
            || weave == default || edgeDwellMs < 0 || position is null
            || (!position.OscillationAdmitted && weave.AmplitudeMm > 0.0)
            || Seq(lateralOffsetMm, heightOffsetMm, travelMmMin, commandedFeedMmMin, heatInputKjMm, thicknessMm)
                .Exists(static value => !double.IsFinite(value))
            || travelMmMin <= 0.0 || commandedFeedMmMin < travelMmMin || heatInputKjMm <= 0.0 || thicknessMm <= 0.0
            || path.IsEmpty || path.Exists(static move => move is null)
            || !path.ForAll(static move => move.Target.IsValid && move.Switch(
                rapid: static _ => true,
                linear: static value => value.Feed > 0.0 && double.IsFinite(value.Feed),
                circular: static _ => false))
            || frames.Count < 2 || frames.Exists(frame => frame.Joint != joint || frame.Side != side
                || frame.Waypoint < 0 || frame.StationMm < 0.0 || frame.StandoffMm <= 0.0 || !frame.Pose.IsValid
                || Seq(frame.StationMm, frame.WorkAngleDeg, frame.TravelAngleDeg, frame.Phase,
                    frame.LateralOffsetMm, frame.StandoffMm)
                    .Exists(static value => !double.IsFinite(value)))
            || frames.Zip(frames.Tail).Exists(static pair => pair.Item1.StationMm >= pair.Item2.StationMm)
            || frames.Exists(frame => !path.Exists(move => move is not Move.Circular
                && frame.Pose.Origin.DistanceTo(move.Target) == 0.0))
            || deposit == default || arc == default || lineage is null || !lineage.Admitted
            ? new ValidationError(message: "weld-pass")
            : null;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record WeldProjection {
    private WeldProjection() { }

    public sealed record Execution(Option<Set<int>> Joints, Option<Set<string>> RoleKeys) : WeldProjection;
    public sealed record Qualification(Option<Set<int>> Joints) : WeldProjection;
    public sealed record Identity : WeldProjection;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record WeldProjectionReceipt {
    private WeldProjectionReceipt() { }

    public sealed record Execution(Seq<WeldPass> Passes, Seq<JointAction> Actions) : WeldProjectionReceipt;
    public sealed record Qualification(Seq<WeldDemand> Demands) : WeldProjectionReceipt;
    public sealed record Identity(double MaxHeatInputKjMm, int Beads, ContentKey Key) : WeldProjectionReceipt;
}

[ComplexValueObject]
public sealed partial class WeldPlan {
    public Seq<WeldPass> Passes { get; }
    public Seq<JointAction> Actions { get; }
    public Seq<WeldDemand> Demands { get; }
    public double MaxHeatInputKjMm { get; }
    public int Beads { get; }
    public ContentKey Key { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Seq<WeldPass> passes,
        ref Seq<JointAction> actions,
        ref Seq<WeldDemand> demands,
        ref double maxHeatInputKjMm,
        ref int beads,
        ref ContentKey key) {
        Seq<int> passJoints = passes.Choose(pass => Optional(pass).Map(static row => row.Joint)).Distinct().ToSeq();
        Seq<int> demandJoints = demands.Choose(demand => Optional(demand).Map(static row => row.Joint)).Distinct().ToSeq();
        validationError = passes.IsEmpty || passes.Exists(static pass => pass is null)
            || demands.IsEmpty || demands.Exists(static demand => demand is null)
            || !LineageClosed(passes)
            || demands.Count != demandJoints.Count
            || passJoints.Count != demandJoints.Count || passJoints.Exists(joint => !demandJoints.Contains(joint))
            || actions.Exists(static action => action is null || !action.Admitted)
            || actions.Exists(action => !passJoints.Contains(action.Joint))
            || maxHeatInputKjMm <= 0.0 || !double.IsFinite(maxHeatInputKjMm)
            || beads != passes.Count || key is null || key.Kind != EgressKind.WeldPlan
            ? new ValidationError(message: "weld-plan")
            : null;
    }

    private static bool LineageClosed(Seq<WeldPass> passes) => passes
        .Map((pass, index) => (Pass: pass, Prior: toSeq(passes.AsEnumerable().Take(index))))
        .ForAll(static row => !row.Prior.Exists(prior => prior.Joint == row.Pass.Joint && prior.Ordinal == row.Pass.Ordinal)
            && row.Pass.Lineage.Resolves(row.Pass.Joint, row.Prior));

    public Fin<WeldProjectionReceipt> Project(WeldProjection projection) => Optional(projection)
        .ToFin(new GeometryFault.DegenerateInput(Kind.Curve, -1, "weld-projection").ToError())
        .Map(request => request.Switch(
            state: this,
            execution: static (plan, value) => new WeldProjectionReceipt.Execution(
                plan.Passes.Filter(pass => value.Joints.ForAll(rows => rows.Contains(pass.Joint))
                    && value.RoleKeys.ForAll(rows => rows.Contains(pass.Role.Key))),
                plan.Actions.Filter(action => value.Joints.ForAll(rows => rows.Contains(action.Joint)))),
            qualification: static (plan, value) => new WeldProjectionReceipt.Qualification(
                plan.Demands.Filter(demand => value.Joints.ForAll(rows => rows.Contains(demand.Joint)))),
            identity: static (plan, _) => new WeldProjectionReceipt.Identity(plan.MaxHeatInputKjMm, plan.Beads, plan.Key)));
}

// --- [OPERATIONS] ---------------------------------------------------------------------------------------------------------------------------------
public static class Weld {
    public static Fin<WeldPlan> Plan(WeldRequest request) =>
        request.Joints.OrderBy(static joint => joint.Joint).ToSeq()
            .Map(joint => PlanJoint(joint, request.Policy, request.Budget).ToValidation())
            .Traverse(identity)
            .As()
            .ToFin()
            .Map(rows => (
                Passes: rows.Bind(static row => row.Passes),
                Actions: rows.Bind(static row => row.Actions),
                Demands: rows.Map(static row => row.Demand),
                Maximum: rows.Map(static row => row.MaxHeatInputKjMm).Fold(0.0, Math.Max)))
            .Bind(receipt => request.Coverage(receipt.Passes).Bind(_ => WeldPlan.Validate(
                receipt.Passes,
                receipt.Actions,
                receipt.Demands,
                receipt.Maximum,
                receipt.Passes.Count,
                ContentKey.Of(EgressKind.WeldPlan, CanonicalBytes(receipt.Passes, receipt.Actions, receipt.Demands)),
                out WeldPlan plan) is { } error
                    ? Fin.Fail<WeldPlan>(new GeometryFault.DegenerateInput(Kind.Curve, -1, error.Message).ToError())
                    : Fin.Succ(plan)));

    public static double HeatInput(double efficiency, double powerW, double arcTimeS, double weldLengthMm) =>
        efficiency * powerW * arcTimeS / (1000.0 * weldLengthMm);

    private static Fin<(Seq<WeldPass> Passes, Seq<JointAction> Actions, WeldDemand Demand, double MaxHeatInputKjMm)> PlanJoint(
        WeldJoint joint,
        WeldPolicy policy,
        ProcessBudget.Joining budget) =>
        from law in policy.Processes.Find(joint.ProcessKey)
            .ToFin(new GeometryFault.DegenerateInput(Kind.Curve, -1, $"weld-plan:process-law:{joint.ProcessKey}").ToError())
        from mode in law.Mode(joint)
        from consumable in law.Deposition is DepositionSource.Autogenous
            ? joint.FillerKey.IsNone && joint.FillerClassificationKey.IsNone
                ? Fin.Succ(unit)
                : Fin.Fail<Unit>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "weld-plan:autogenous-filler").ToError())
            : joint.FillerKey.IsSome && joint.FillerClassificationKey.IsSome
                ? Fin.Succ(unit)
                : Fin.Fail<Unit>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "weld-plan:filler").ToError())
        from arc in budget.CurrentA <= mode.CurrentCapA && budget.InterpassTemp >= joint.PreheatC
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new GeometryFault.DegenerateInput(Kind.Curve, -1, $"weld-plan:arc-envelope:{joint.Joint}").ToError())
        from frames in Transport(joint, policy, budget.Standoff)
        from seal in joint.Prep is JointPrep.Groove { Root: RootProgram.Seal }
            && !policy.Beads.Bands.Exists(static band => band.Role == PassRole.Seal && band.EndFraction == 1.0)
                ? Fin.Fail<Unit>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "weld-plan:seal-band").ToError())
                : Fin.Succ(unit)
        from passes in Generate(joint, policy, budget, law, mode, frames)
        from access in policy.Access.Traverse(constraint => constraint.Check(joint, passes)).As().ToFin()
        let maximum = passes.Map(static pass => pass.HeatInputKjMm).Fold(0.0, Math.Max)
        let minimum = passes.Map(static pass => pass.HeatInputKjMm).Fold(double.PositiveInfinity, Math.Min)
        from ceiling in maximum <= policy.HeatInputCapKjMm && maximum <= mode.HeatInputHighKjMm
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new FabricationFault.HeatInputExceeded(
                joint.Joint,
                maximum,
                Math.Min(policy.HeatInputCapKjMm, mode.HeatInputHighKjMm)).ToError())
        from floor in minimum >= mode.HeatInputLowKjMm
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new GeometryFault.DegenerateInput(Kind.Curve, -1, $"weld-plan:heat-input-floor:{joint.Joint}:{minimum:R}").ToError())
        from cooling in passes.Map(static pass => pass.Deposit.CoolingTimeS)
            .Filter(value => value < mode.CoolingLowS || value > mode.CoolingHighS)
            .HeadOrNone()
            .Match(
                Some: value => Fin.Fail<Unit>(
                    new GeometryFault.DegenerateInput(Kind.Curve, -1, $"weld-plan:cooling-band:{joint.Joint}:{value:R}").ToError()),
                None: () => Fin.Succ(unit))
        from demand in Demand(joint, policy, budget, passes, maximum)
        select (passes, Actions(joint, budget), demand, maximum);

    // Fold cursor: fill metal closes the groove, deposit metal is every carrier, and the lattice indices place
    // each bead within its layer. Threading it keeps the pass generator a pure state advance.
    private readonly record struct BeadCursor(
        double FillMm3,
        double DepositMm3,
        int Layer,
        int Bead,
        int BeadsInLayer,
        Seq<WeldPass> Passes);

    private static Fin<Seq<WeldPass>> Generate(
        WeldJoint joint,
        WeldPolicy policy,
        ProcessBudget.Joining budget,
        WeldProcessLaw law,
        ArcMode mode,
        Seq<TorchFrame> baseFrames) {
        double requestedTravel = Math.Min(
            (mode.Efficiency * 60.0 * budget.CurrentA * budget.VoltageV) / (1000.0 * policy.TargetHeatInputKjMm),
            budget.TravelSpeed) * joint.Position.TravelDerate;
        double pathLength = joint.Prep.Demand.DepositLengthMm;
        double rate = law.Deposition.Rate(budget) * joint.Position.DepositionDerate;
        double required = joint.Prep.Demand.VolumeMm3;
        return mode.Travel(requestedTravel, joint.Joint).Bind(travel => rate <= 0.0 || pathLength <= 0.0
                ? Fin.Fail<Seq<WeldPass>>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "weld-plan:capacity").ToError())
                : Range(0, policy.PassCap).Fold(
                    Fin.Succ(new BeadCursor(0.0, 0.0, 0, 0, 1, Seq<WeldPass>())),
                    (held, ordinal) => held.Bind(cursor => cursor.FillMm3 >= required
                        ? Fin.Succ(cursor)
                        : Pass(joint, policy, law, mode, budget, baseFrames,
                            policy.Beads.Resolve(Math.Min(double.BitDecrement(1.0), cursor.FillMm3 / required)),
                            cursor, ordinal, travel, rate, pathLength)))
                    .Bind(cursor => cursor.FillMm3 >= required
                        ? Fin.Succ(cursor)
                        : Fin.Fail<BeadCursor>(new GeometryFault.DegenerateInput(
                            Kind.Curve, -1, "weld-plan:pass-cap").ToError()))
                    .Bind(cursor => policy.Beads.Overlay.Fold(
                        Fin.Succ(cursor),
                        (held, band) => held.Bind(row => Pass(
                            joint, policy, law, mode, budget, baseFrames, band, row,
                            row.Passes.Count, travel, rate, pathLength))))
                    .Map(static cursor => cursor.Passes));
    }

    private static Fin<BeadCursor> Pass(
        WeldJoint joint,
        WeldPolicy policy,
        WeldProcessLaw law,
        ArcMode mode,
        ProcessBudget.Joining budget,
        Seq<TorchFrame> baseFrames,
        RoleBand band,
        BeadCursor cursor,
        int ordinal,
        double travel,
        double rate,
        double pathLength) {
        FillProfile profile = joint.Prep.Demand;
        double required = profile.VolumeMm3;
        double fraction = Math.Min(double.BitDecrement(1.0), cursor.FillMm3 / required);
        return mode.Travel(travel * band.Role.TravelFactor, joint.Joint).Bind(roleTravel => {
        double powerW = budget.CurrentA * band.Role.CurrentFactor * budget.VoltageV;
        double capacity = Math.Min(rate * pathLength / roleTravel, required * band.Role.AreaFactor);
        double deposited = band.Role.FillContribution > 0.0
            ? Math.Min(capacity, (required - cursor.FillMm3) / band.Role.FillContribution)
            : capacity;
        double area = deposited / pathLength;
        // Bead geometry resolves against the layer's section width, so a wide groove takes several beads across
        // one layer instead of a single full-width deposit stacked vertically.
        double fillHeight = profile.HeightAtFill(fraction);
        double layerWidth = profile.WidthAtHeight(fillHeight);
        // A bead never runs narrower than its deposition source, so a fillet root of zero layer width still
        // seats one bead instead of dividing height by a vanishing width.
        double width = Math.Max(law.Deposition.Width, Math.Min(layerWidth, Math.Max(law.Deposition.Width,
            Math.Sqrt((area * band.Role.AreaFactor) / policy.Beads.HeightFactor) * policy.Beads.WidthFactor)));
        double height = area / width;
        (int beadsInLayer, double lateral) = policy.Beads.Lattice(layerWidth, width, cursor.Bead);
        int bead = Math.Min(cursor.Bead, beadsInLayer - 1);
        int side = joint.Prep.DoubleSided
            ? fraction < 0.5 ? joint.Prep.FirstSide : 1 - joint.Prep.FirstSide
            : 0;
        Seq<TorchFrame> oriented = side == 0 ? baseFrames : baseFrames.Map(static frame => frame.Opposed());
        Seq<TorchFrame> frames = Weave(oriented, band.Weave, profile, policy, lateral, fillHeight)
            .Filter(frame => profile.Spans.Exists(span => span.Contains(frame.StationMm)));
        double wovenLength = frames.Zip(frames.Tail)
            .Filter(pair => profile.Spans.Exists(span => span.Contains(pair.Item1.StationMm)
                && span.Contains(pair.Item2.StationMm)))
            .Fold(0.0, static (sum, pair) => sum + pair.Item1.Pose.Origin.DistanceTo(pair.Item2.Pose.Origin));
        // Oscillated frames lengthen the commanded path, so the feed scales to hold seam progression at roleTravel.
        double commandedFeed = roleTravel * Math.Max(1.0, wovenLength / pathLength);
        Seq<Move> path = toSeq(profile.Spans).Bind(span => {
            Seq<TorchFrame> run = frames.Filter(frame => span.Contains(frame.StationMm));
            return run.Count < 2
                ? Seq<Move>()
                : band.Arc.Apply(run, commandedFeed);
        });
        double fillerLength = law.Deposition.FillerLength(deposited);
        // Arc time carries oscillation dwell and crater fill, so heat input and energy stop reading a bare
        // travel-speed quotient that under-reports every dwelling weave.
        double arcTime = band.Arc.ArcTime(path) + band.Weave.DwellSeconds(pathLength);
        double heatInput = HeatInput(mode.Efficiency, powerW, arcTime, pathLength);
        double cooling = BeadEvidence.CoolingTime(
            heatInput, joint.PreheatC, joint.ThicknessMm, joint.Prep.ShapeFactors, joint.Position.CoolingScale);
        double fill = deposited * band.Role.FillContribution;
        ValidationError? evidenceError = BeadEvidence.Validate(
            deposited, area, width, height, powerW * arcTime, fillerLength,
            Math.Min(1.0, (cursor.FillMm3 + fill) / required), arcTime, cooling, pathLength,
            out BeadEvidence evidence);
        ValidationError? passError = WeldPass.Validate(
            joint.Joint, band.Role, cursor.Layer, bead, beadsInLayer, side, ordinal, band.Weave,
            (int)Math.Round(band.Weave.EdgeDwellS * 1000.0), joint.Position, lateral, fillHeight,
            roleTravel, commandedFeed, heatInput, joint.ThicknessMm, path, frames, evidence,
            band.Arc, band.Lineage, out WeldPass pass);
        return frames.Count >= 2 && !path.IsEmpty && band.Lineage.Resolves(joint.Joint, cursor.Passes)
            && evidenceError is null && passError is null
            ? Fin.Succ(cursor with {
                FillMm3 = cursor.FillMm3 + fill,
                DepositMm3 = cursor.DepositMm3 + deposited,
                Layer = bead + 1 >= beadsInLayer ? cursor.Layer + 1 : cursor.Layer,
                Bead = bead + 1 >= beadsInLayer ? 0 : bead + 1,
                BeadsInLayer = beadsInLayer,
                Passes = cursor.Passes.Add(pass),
            })
            : Fin.Fail<BeadCursor>(
                new GeometryFault.DegenerateInput(
                    Kind.Curve,
                    -1,
                    evidenceError?.Message ?? passError?.Message ?? "weld-plan:deposit-span").ToError());
        });
    }

    // Transport carries the SEAM frame — X tangent, Y lateral, Z surface normal — and never the torch attitude;
    // work and travel rotation applies after Weave places the bead, so lateral and height offsets stay orthogonal
    // to travel instead of bleeding into it through a pre-rotated basis.
    private static Fin<Seq<TorchFrame>> Transport(WeldJoint joint, WeldPolicy policy, double standoffMm) {
        (Option<Vector3d> Prior, Seq<TorchFrame> Rows) seed = (None, Seq<TorchFrame>());
        Seq<double> stations = joint.Seam.Zip(joint.Seam.Tail).Fold(
            Seq(0.0),
            static (held, pair) => held.Add(held.Last + pair.Item1.DistanceTo(pair.Item2)));
        return joint.Seam.Zip(joint.Normals).Map((pair, index) => (pair.Item1, pair.Item2, index)).Fold(
            Fin.Succ(seed),
            (held, row) => held.Bind(state => Frame(
                    joint, policy, standoffMm, state.Prior, row.Item1, row.Item2, row.index)
                .Map(frame => (Some(frame.Normal), state.Rows.Add(frame.Frame)))))
            .Map(state => state.Rows.Map((frame, index) => frame with { StationMm = stations[index] }))
            .Map(rows => Resample(rows, toSeq(joint.Prep.Demand.Spans).Bind(static span => Seq(span.StartMm, span.EndMm))))
            .Map(static rows => rows.Map(static (frame, index) => frame with { Waypoint = index }));
    }

    // Span endpoints rarely coincide with seam vertices, so each boundary station gains an interpolated frame;
    // without it a span bracketed by two distant vertices yields fewer than two run frames and no deposit path.
    private static Seq<TorchFrame> Resample(Seq<TorchFrame> rows, Seq<double> required) => required
        .Distinct()
        .OrderBy(static station => station)
        .ToSeq()
        .Fold(rows, static (held, station) => held.Exists(row => row.StationMm == station)
            ? held
            : held.Zip(held.Tail)
                .Choose(pair => station > pair.Item1.StationMm && station < pair.Item2.StationMm
                    ? Some(Interpolate(pair.Item1, pair.Item2, station))
                    : None)
                .HeadOrNone()
                .Match(
                    Some: frame => held.Add(frame).OrderBy(static row => row.StationMm).ToSeq(),
                    None: () => held));

    private static TorchFrame Interpolate(TorchFrame low, TorchFrame high, double station) {
        double t = (station - low.StationMm) / (high.StationMm - low.StationMm);
        return low with {
            StationMm = station,
            Pose = new Plane(
                low.Pose.Origin + ((high.Pose.Origin - low.Pose.Origin) * t),
                low.Pose.XAxis + ((high.Pose.XAxis - low.Pose.XAxis) * t),
                low.Pose.YAxis + ((high.Pose.YAxis - low.Pose.YAxis) * t)),
        };
    }

    private static Fin<(TorchFrame Frame, Vector3d Normal)> Frame(
        WeldJoint joint,
        WeldPolicy policy,
        double standoffMm,
        Option<Vector3d> prior,
        Point3d point,
        Vector3d suppliedNormal,
        int index) {
        Vector3d tangent = WeldJoint.Tangent(joint.Seam, index);
        Vector3d normal = prior.Exists(value => Vector3d.Multiply(value, suppliedNormal) < 0.0)
            ? -suppliedNormal
            : suppliedNormal;
        return tangent.Unitize() && normal.Unitize()
            ? Pose(point, tangent, normal, standoffMm, policy, joint.Joint, index).Map(frame => (frame, normal))
            : Fin.Fail<(TorchFrame, Vector3d)>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "weld-plan:frame").ToError());
    }

    private static Fin<TorchFrame> Pose(
        Point3d point,
        Vector3d tangent,
        Vector3d normal,
        double standoffMm,
        WeldPolicy policy,
        int joint,
        int index) {
        Plane pose = new(point + (standoffMm * normal), tangent, Vector3d.CrossProduct(normal, tangent));
        return pose.IsValid
            ? Fin.Succ(new TorchFrame(
                joint, 0, index, 0.0, pose, policy.WorkAngleDeg, policy.TravelAngleDeg, 0.0, 0.0, standoffMm))
            : Fin.Fail<TorchFrame>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "weld-plan:pose").ToError());
    }

    private static Seq<TorchFrame> Weave(
        Seq<TorchFrame> frames,
        WeavePattern weave,
        FillProfile profile,
        WeldPolicy policy,
        double lateralOffset,
        double heightOffset) =>
        frames.Map(frame => {
            double station = frame.StationMm;
            (double _, double width, double _, double _) = profile.Section(station);
            double lateral = Math.Clamp(lateralOffset + weave.Offset(station), -0.5 * width, 0.5 * width);
            Plane pose = frame.Pose;
            pose.Origin += (heightOffset * pose.ZAxis) + (lateral * pose.YAxis);
            _ = pose.Rotate(policy.WorkAngleDeg * Math.PI / 180.0, pose.XAxis);
            _ = pose.Rotate(policy.TravelAngleDeg * Math.PI / 180.0, pose.YAxis);
            return frame with {
                Pose = pose,
                Phase = station / weave.PitchMm,
                LateralOffsetMm = lateral,
            };
        });

    private static Seq<JointAction> Actions(WeldJoint joint, ProcessBudget.Joining budget) =>
        Seq<JointAction>(new JointAction.Preheat(joint.Joint, joint.PreheatC, budget.InterpassTemp))
        + Prep(joint)
        + joint.PwhtC
            .Bind(soak => joint.PwhtMinutes.Map(minutes =>
                (JointAction)new JointAction.PostWeldHeatTreat(joint.Joint, soak, minutes)))
            .ToSeq();

    private static Seq<JointAction> Prep(WeldJoint joint) => joint.Prep.Switch(
        state: joint.Joint,
        groove: static (jointId, prep) => Seq<JointAction>(new JointAction.PrepareGroove(
                jointId, prep.GeometryKey, prep.PenetrationKey, prep.Fill, prep.DoubleSided))
            + prep.Root.Switch(
                none: static _ => Seq<JointAction>(),
                backing: value => Seq<JointAction>(new JointAction.InstallBacking(jointId, value.Key))
                    + (value.RemoveAfterWeld
                        ? Seq<JointAction>(new JointAction.RemoveBacking(jointId, value.Key))
                        : Seq<JointAction>()),
                backgouge: value => Seq<JointAction>(new JointAction.Backgouge(jointId, value.BeforeSide, value.DepthMm)),
                backingAndBackgouge: value => Seq<JointAction>(
                        new JointAction.InstallBacking(jointId, value.Key),
                        new JointAction.Backgouge(jointId, value.BeforeSide, value.DepthMm))
                    + (value.RemoveAfterWeld
                        ? Seq<JointAction>(new JointAction.RemoveBacking(jointId, value.Key))
                        : Seq<JointAction>()),
                seal: static _ => Seq<JointAction>()),
        fillet: static (_, _) => Seq<JointAction>(),
        cavity: static (_, _) => Seq<JointAction>(),
        flare: static (_, _) => Seq<JointAction>());

    private static Fin<WeldDemand> Demand(
        WeldJoint joint,
        WeldPolicy policy,
        ProcessBudget.Joining budget,
        Seq<WeldPass> passes,
        double maximum) =>
        policy.DemandBindings.Map(binding => Try.lift(() => binding.Resolve(
                    new WeldDemandBinding.Facts(joint, budget, passes, maximum)))
                .Run()
                .MapFail(error => new GeometryFault.DegenerateInput(Kind.Curve, -1, $"weld-demand:{binding.Field.Key}:{error.Message}").ToError())
                .Bind(identity)
                .Map(value => (binding.Field.Key, value))
                .ToValidation())
            .Traverse(identity)
            .As()
            .ToFin()
            .Bind(rows => WeldDemand.Validate(
                    joint.Joint,
                    rows.ToMap(),
                    joint.QualificationContext,
                    joint.Inspection,
                    out WeldDemand demand) is { } error
                        ? Fin.Fail<WeldDemand>(new GeometryFault.DegenerateInput(Kind.Curve, -1, error.Message).ToError())
                        : Fin.Succ(demand));

    private static byte[] CanonicalBytes(Seq<WeldPass> passes, Seq<JointAction> actions, Seq<WeldDemand> demands) {
        using System.IO.MemoryStream stream = new();
        using System.IO.BinaryWriter writer = new(stream, System.Text.Encoding.UTF8, leaveOpen: true);
        writer.Write(passes.Count);
        passes.Iter(pass => {
            writer.Write(pass.Joint);
            writer.Write(pass.Role.Key);
            WriteWeave(writer, pass.Weave);
            WriteLineage(writer, pass.Lineage);
            writer.Write(pass.Layer);
            writer.Write(pass.Bead);
            writer.Write(pass.BeadsInLayer);
            writer.Write(pass.Side);
            writer.Write(pass.Ordinal);
            writer.Write(pass.Position.Key);
            writer.Write(pass.EdgeDwellMs);
            writer.Write(pass.LateralOffsetMm);
            writer.Write(pass.HeightOffsetMm);
            writer.Write(pass.TravelMmMin);
            writer.Write(pass.CommandedFeedMmMin);
            writer.Write(pass.HeatInputKjMm);
            writer.Write(pass.ThicknessMm);
            writer.Write(pass.Deposit.DepositedVolumeMm3);
            writer.Write(pass.Deposit.BeadAreaMm2);
            writer.Write(pass.Deposit.WidthMm);
            writer.Write(pass.Deposit.HeightMm);
            writer.Write(pass.Deposit.EnergyJ);
            writer.Write(pass.Deposit.FillerLengthMm);
            writer.Write(pass.Deposit.CoverageFraction);
            writer.Write(pass.Deposit.ArcTimeS);
            writer.Write(pass.Deposit.CoolingTimeS);
            writer.Write(pass.Deposit.DepositLengthMm);
            writer.Write(pass.Arc.RunInMm);
            writer.Write(pass.Arc.BackstepMm);
            writer.Write(pass.Arc.CraterFillS);
            writer.Write(pass.Arc.RunOutMm);
            writer.Write(pass.Frames.Count);
            pass.Frames.Iter(frame => {
                writer.Write(frame.Joint);
                writer.Write(frame.Side);
                writer.Write(frame.Waypoint);
                writer.Write(frame.StationMm);
                writer.Write(frame.WorkAngleDeg);
                writer.Write(frame.TravelAngleDeg);
                writer.Write(frame.Phase);
                writer.Write(frame.LateralOffsetMm);
                writer.Write(frame.StandoffMm);
                WritePoint(writer, frame.Pose.Origin);
                WriteVector(writer, frame.Pose.XAxis);
                WriteVector(writer, frame.Pose.YAxis);
                WriteVector(writer, frame.Pose.ZAxis);
            });
            writer.Write(pass.Path.Count);
            pass.Path.Iter(move => move.Switch(
                state: writer,
                rapid: static (sink, value) => { sink.Write((byte)0); WritePoint(sink, value.Target); },
                linear: static (sink, value) => { sink.Write((byte)1); WritePoint(sink, value.Target); sink.Write(value.Feed); },
                circular: static (sink, value) => {
                    sink.Write((byte)2);
                    WritePoint(sink, value.Target);
                    sink.Write(value.Feed);
                    WritePoint(sink, value.Arc.Center);
                    sink.Write(value.Arc.Sense.Key);
                }));
        });
        writer.Write(actions.Count);
        actions.Iter(action => action.Switch(
            state: writer,
            prepareGroove: static (sink, value) => {
                sink.Write((byte)0); sink.Write(value.Joint); sink.Write(value.GeometryKey); sink.Write(value.PenetrationKey);
                WriteProfile(sink, value.Profile); sink.Write(value.DoubleSided);
            },
            installBacking: static (sink, value) => { sink.Write((byte)1); sink.Write(value.Joint); sink.Write(value.BackingKey); },
            backgouge: static (sink, value) => {
                sink.Write((byte)2); sink.Write(value.Joint); sink.Write(value.BeforeSide); sink.Write(value.DepthMm);
            },
            removeBacking: static (sink, value) => { sink.Write((byte)3); sink.Write(value.Joint); sink.Write(value.BackingKey); },
            preheat: static (sink, value) => {
                sink.Write((byte)4); sink.Write(value.Joint); sink.Write(value.TargetC); sink.Write(value.InterpassCapC);
            },
            postWeldHeatTreat: static (sink, value) => {
                sink.Write((byte)5); sink.Write(value.Joint); sink.Write(value.SoakC); sink.Write(value.SoakMinutes);
            }));
        writer.Write(demands.Count);
        demands.Iter(demand => {
            writer.Write(demand.Joint);
            writer.Write(demand.Context.Count);
            demand.Context.OrderBy(static value => value).Iter(value => writer.Write(value));
            writer.Write(demand.Inspection.JointClass.Key);
            writer.Write(demand.Inspection.ExecutionClass);
            writer.Write(demand.Inspection.StressCategory);
            writer.Write(demand.Inspection.FatigueCritical);
            writer.Write(demand.Inspection.Thickness.As(LengthUnit.Millimeter));
            writer.Write(demand.Inspection.Populations.Count);
            demand.Inspection.Populations.OrderBy(static row => row.Key.Key).Iter(row => {
                writer.Write(row.Key.Key);
                row.Value.Switch(
                    state: writer,
                    joints: static (sink, value) => { sink.Write((byte)0); sink.Write(value.Count); },
                    linear: static (sink, value) => { sink.Write((byte)1); sink.Write(value.Value.Millimeters); },
                    areal: static (sink, value) => { sink.Write((byte)2); sink.Write(value.Value.SquareMillimeters); },
                    volumetric: static (sink, value) => { sink.Write((byte)3); sink.Write(value.Value.CubicMillimeters); });
            });
            writer.Write(demand.Values.Count);
            demand.Values.OrderBy(static row => row.Key.Value).Iter(row => {
                writer.Write(row.Key.Value);
                row.Value.Switch(
                    state: writer,
                    quantity: static (sink, value) => {
                        sink.Write((byte)0);
                        sink.Write(value.Value.GetType().FullName ?? value.Value.GetType().Name);
                        sink.Write(value.Value.Unit.ToString());
                        sink.Write(Convert.ToDouble(value.Value.Value, System.Globalization.CultureInfo.InvariantCulture));
                    },
                    categorical: static (sink, value) => { sink.Write((byte)1); sink.Write(value.Value); },
                    boolean: static (sink, value) => { sink.Write((byte)2); sink.Write(value.Value); },
                    temporal: static (sink, value) => { sink.Write((byte)3); sink.Write(value.Value.ToUnixTimeTicks()); },
                    notApplicable: static (sink, _) => sink.Write((byte)4));
            });
        });
        writer.Flush();
        return stream.ToArray();

        static void WritePoint(System.IO.BinaryWriter writer, Point3d point) {
            writer.Write(point.X);
            writer.Write(point.Y);
            writer.Write(point.Z);
        }

        static void WriteVector(System.IO.BinaryWriter writer, Vector3d vector) {
            writer.Write(vector.X);
            writer.Write(vector.Y);
            writer.Write(vector.Z);
        }

        static void WriteProfile(System.IO.BinaryWriter writer, FillProfile profile) {
            writer.Write(profile.VolumeMm3);
            writer.Write(profile.EffectiveThroatMm);
            writer.Write(profile.ReinforcementMm);
            writer.Write(profile.ToeRadiusMm);
            writer.Write(profile.Stations.Count);
            profile.Stations.Iter(station => {
                writer.Write(station.StationMm);
                writer.Write(station.AreaMm2);
                writer.Write(station.WidthMm);
                writer.Write(station.RootWidthMm);
                writer.Write(station.HeightMm);
            });
            writer.Write(profile.Spans.Count);
            profile.Spans.Iter(span => { writer.Write(span.StartMm); writer.Write(span.EndMm); });
        }

        static void WriteWeave(System.IO.BinaryWriter writer, WeavePattern weave) {
            writer.Write(weave.AmplitudeMm);
            writer.Write(weave.PitchMm);
            writer.Write(weave.EdgeDwellS);
            writer.Write(weave.TogglesPerCycle);
            weave.Shape.Switch(
                state: writer,
                harmonic: static (sink, value) => {
                    sink.Write((byte)0);
                    sink.Write(value.Terms.Count);
                    value.Terms.Iter(term => { sink.Write(term.Order); sink.Write(term.Amplitude); sink.Write(term.PhaseRad); });
                },
                piecewise: static (sink, value) => {
                    sink.Write((byte)1);
                    sink.Write(value.Knots.Count);
                    value.Knots.Iter(knot => { sink.Write(knot.Phase); sink.Write(knot.Offset); });
                });
        }

        static void WriteLineage(System.IO.BinaryWriter writer, PassLineage lineage) => lineage.Switch(
            state: writer,
            planned: static (sink, _) => sink.Write((byte)0),
            repair: static (sink, value) => {
                sink.Write((byte)1); sink.Write(value.ReplacesOrdinal); sink.Write(value.DefectKey); sink.Write(value.ExcavatedMm3);
            },
            temper: static (sink, value) => { sink.Write((byte)2); sink.Write(value.ConditionsOrdinal); });
    }
}
```
