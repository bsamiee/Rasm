# [RASM_FABRICATION_WIRE_EDM]

`WireEdm.Generate` admits one traveling-wire demand, resolves context-keyed pass law, derives access and recovery, registers lower-to-upper correspondence, and emits simultaneous-guide `WireBlock` rows through one typed rail. `WireProgram` preserves every electrical, hydraulic, geometric, station, and quality decision without flattening simultaneous guides into sequential motion.

`WireEdm.Generate`, `WireProgram`, and `WireBlock` remain the wire seam names. `WirePolicy` is the admitted, profile-independent half of a wire job, so a CAM run routing an erosion boundary pass carries the policy it already holds and `WireDemand` is the profile and the budget joined to it — no eleven-column transcription stands between the two. `WireProgram.SpecializedDirective` projects simultaneous lower/upper guides, process action, lag, rotary state, and derived duration into the admitted `SpecializedToolpathEnvelope`, and `WireEdm.Lower` projects the lower guide as the Cartesian path a routed erosion pass executes while the upper guide stays whole on that same `SpecializedToolpathEnvelope`. `ProcessBudget.Erosion` supplies admitted process evidence, `ArcOffset` owns offset topology, `CavalierContours` owns arc-native station measurement, and `FabricationFault.WireTaperExceeded` carries refusal against the guide pair's operating envelope.

## [01]-[INDEX]

- [02]-[ADMISSION]: `WirePolicy` closes guides, schedules, correspondence, access, retention, and recovery once; `WireDemand` joins it to a profile and a budget.
- [03]-[GENERATION]: `WireCycle` closes contour, taper, four-axis, clearing, collar, rotary, variable-taper, and cutoff payload timing under one dispatch over shared occurrence columns.
- [04]-[EGRESS]: `WireProgram` projects through a caller-supplied arrow while retaining simultaneous guides and process evidence.

## [02]-[ADMISSION]

`WireJob` owns the whole trusted cutting context as THREE columns — the admitted policy, the profile it cuts, and the budget it spends. Schedule compatibility is a declared relation over machine book, generator, material, wire, dielectric, and thickness; no named global roster can impersonate that evidence.

- Owner: `WirePolicy` is the admitted cutting law independent of any one profile — cycle, wire radius, guide planes, schedule, access, retention, and recovery — so the same policy admits once and cuts many profiles, and a routed erosion pass carries it rather than re-parsing dimensions per contour.
- Owner: `WireSchedule` re-admits contiguous generated `WirePass` rows and binds their provenance to `WireContext`; no field-parallel pass DTO survives beside the owner.
- Owner: `WirePass` carries the electrical, hydraulic, and mechanical columns the wire bow and corner law read — `Tension`, `FlushPressure`, `WireSpeedMmPerMin`, and `CornerAngleDeg`; `LagMm` derives the taut-string deflection instead of admitting it as a caller knob.
- Owner: `WireCorrespondence` binds seam, direction, and piecewise station anchors before any four-axis sample occurs.
- Owner: `WireAccess` carries open-edge, drilled, automatic-thread, and inherited-channel access as occurrence payload.
- Owner: `SlugRetention` carries explicit bridge intervals and a payload-bearing `WireRelease`; release evidence never rides a parallel integer knob.
- Owner: `WireRecovery` carries break detection, retract, rethread, restart, and attempt evidence for unattended cuts.
- Law: admission runs ONCE per fact. `WirePolicy.Admit` proves guides, schedule, cycle, retention, and recovery over quantities that are already DIMENSIONED; `WireJob.Admit` then proves only what a profile adds — the ring itself and the access relation that reads it. A per-column presence bridge at the job boundary re-asks a question the policy already answered.
- Boundary: no dimension text reaches this page. A shop string crosses `Process/owner#RUN_DISPATCH` `QuantityArrow` in the caller's hand, so wire radius, thickness, the three guide planes, and the taper ceiling arrive as `Length` and `Angle` under ONE admission regime; the deleted form parsed two of those six here and took the other four as bare scalars.
- Packages: `UnitsNet` types every admitted magnitude; `Interpolate.Linear` owns variable-taper evaluation, built once per dispatch rather than per station; `TensorPrimitives.IsFiniteAll` admits numeric batches; `Thinktecture` generated owners close construction.
- Boundary: `WireDemand` is raw exactly once, and every interior function consumes `WireJob`.

## [03]-[GENERATION]

`WireCycle` separates payload timing while `WireEdm.Generate` remains the one operation. `Contour`, `TaperContour`, `Collar`, and `VariableTaper` share the boundary-pass algebra; `FourAxis` supplies an independent upper profile with registered correspondence; `NoCorePocket` supplies recession; `Rotary` supplies a cylindrical station map; `Cutoff` supplies a terminal slug handoff.

- Owner: the occurrence evidence every cycle publishes — taper demand, extra stations, correspondence, and spindle frame — rides BASE COLUMNS each case fills at construction, so dispatch is the two folds that genuinely differ per case and not six parallel eight-arm ladders restating one roster.
- Exemption: vertex-station folding, span-frame normalization, and pass reduction are measured kernel statement boundaries.
- Entry: `Generate<TOut>` fixes ingress as `WireDemand?` and parameterizes the egress projection result.
- Auto: every pass derives compound offset, vertex, bridge, taper, correspondence, access, and handoff stations, corner-control transitions, and simultaneous lower/upper samples; a clearing cycle carries its offset LEVEL on every block, so a span is measured inside one ring and a level boundary reports no length rather than being clamped to zero.
- Auto: four-axis pairing maps registered physical stations rather than incidental indices or normalized-length coincidence.
- Auto: corner control scales by the exact arc turn angle at the SHARPEST vertex inside the corner window — the sharpest corner is what the wire must survive, so the window folds a maximum and not a nearest — and the vertices index into window-width buckets, so a mark probes three buckets rather than the whole vertex roster.
- Auto: `TaperCornerMode` applies every taper shift at full magnitude and owns only the upper corner radius, so no mode can silently flatten a taper.
- Auto: access precedes cutting, recovery checkpoints preserve restart custody, and pass quality derives from schedule data.
- Packages: `ArcOffset` owns offset topology; `Polyline<double>.PathLength`, `FindPointAtPathLength`, `PlineSeg.SegTangentVector`, `PrevWrappingIndex`, and `NextWrappingIndex` own arc-native stationing and turn measurement; `LanguageExt` `TraverseM`, `Fold`, and query syntax keep the rail flat.
- Boundary: `FabricationFault.WireTaperExceeded` refuses guide demand without clamping, and offset failures remain typed. No absence rides an infinity: an empty taper law is refused at admission, so the taper demand column measures a real law rather than carrying a sentinel past a structural gate.

## [04]-[EGRESS]

`WireProgram` is the inverse-sufficient program: every block carries simultaneous guides, physical station, offset level, traversed arc length, traversal progress, payload-timed action, wire bow, upper corner radius, and optional rotary position; program custody preserves access, retention, correspondence, context, and recovery beside pass quality evidence.

- Law: posting retains guide-pair simultaneity, simulation retains specialized rows and duration, and estimation consumes that simulation ledger. The `SpecializedToolpathEnvelope` is admitted ONCE at program construction through the S0 factory, so no consumer re-walks its rows, and `ToolpathRowMap` owns the block-to-row transcription — including the projection of the payload-bearing `WireAction` case onto the S0 `WireActionKind` row a preimage can frame.
- Law: a routed erosion pass reaches this owner through `WireEdm.Lower`: the LOWER guide is the Cartesian path the machine's axes execute and every simultaneous, electrical, and rotary fact stays on the `SpecializedToolpathEnvelope`, so routing preserves exactly what a flattened lower/upper move pair would destroy.
- Output: `WireProgram.PostingSource` carries the typed envelope into canonical posting; the caller arrow retains other result projections.
- Output: `WirePassEvidence` preserves schedule identity, quality, removed offset, arc-true cut length, consumed wire, peak wire bow, bridge count, and recovery budget; cut length folds `TraversedMm` deltas within one ring, never chord distance between sampled guides; the per-pass fold mints no key and reads no clock.
- Growth: a machine-book capability is one `WirePass` row; a new occurrence payload is one `WireCycle` case filling the shared columns; a new projection changes only the supplied arrow.
- Boundary: sequential lower/upper `Move` rows cannot represent `WireBlock` and never cross this seam.

```csharp
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using System.Numerics.Tensors;
using CavalierContours.Core;
using CavalierContours.Polyline;
using LanguageExt;
using LanguageExt.Common;
using MathNet.Numerics.Interpolation;
using Rasm.Domain;
using Rasm.Element.Projection;
using Rasm.Fabrication.Geometry2D;
using Rasm.Fabrication.Process;
using Rasm.Numerics;
using Rhino.Geometry;
using Riok.Mapperly.Abstractions;
using Thinktecture;
using UnitsNet;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Toolpath;

// --- [VOCABULARY] ----------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class TaperCornerMode {
    public static readonly TaperCornerMode Conical = new("conical", translated: false, cornerRadiusShare: 0.0);
    public static readonly TaperCornerMode Cylindrical = new("cylindrical", translated: true, cornerRadiusShare: 0.0);
    public static readonly TaperCornerMode ConstantLand = new("constant-land", translated: true, cornerRadiusShare: 1.0);
    public static readonly TaperCornerMode Reduced = new("reduced", translated: false, cornerRadiusShare: 0.5);

    public bool Translated { get; }
    public double CornerRadiusShare { get; }

    public Vector3d Direction(WireGuidePoint guide, Loop loop) {
        Vector3d direction = Translated ? guide.SpanNormal(loop) : guide.Normal;
        return direction.Unitize() ? direction : Vector3d.Unset;
    }

    public double CornerRadiusMm(double shiftMm) => CornerRadiusShare * Math.Abs(shiftMm);
}

[SmartEnum<string>]
public sealed partial class WireFinish {
    public static readonly WireFinish Rough = new("rough");
    public static readonly WireFinish Precision = new("precision");
    public static readonly WireFinish Polish = new("polish");
    public static readonly WireFinish Release = new("release");
}

[SmartEnum<string>]
public sealed partial class WireDirection {
    public static readonly WireDirection Forward = new("forward", reversed: false);
    public static readonly WireDirection Reverse = new("reverse", reversed: true);

    public bool Reversed { get; }
}

[Union]
public abstract partial record WireRelease {
    public sealed record AfterRough : WireRelease;
    public sealed record AtPass(int Pass) : WireRelease;
    public sealed record AtFinal : WireRelease;

    public bool Released(int pass, int finalPass) => Switch(
        state: (pass, finalPass),
        afterRough: static (state, _) => state.pass > 1,
        atPass: static (state, release) => state.pass >= release.Pass,
        atFinal: static (state, _) => state.pass >= state.finalPass);
}

// --- [ADMISSION] -----------------------------------------------------------------------
[ComplexValueObject]
public sealed partial class WirePass {
    public int Pass { get; }
    public double SparkGapMm { get; }
    public double OverburnMm { get; }
    public double StockMm { get; }
    public double SpeedScale { get; }
    public double Power { get; }
    public double FlushPressure { get; }
    public double Tension { get; }
    public double FeedMmPerMin { get; }
    public double WireSpeedMmPerMin { get; }
    public double CornerSlowMm { get; }
    public double CornerSpeedScale { get; }
    public double CornerAngleDeg { get; }
    public WireFinish Finish { get; }

    public double Offset(double wireRadiusMm) => wireRadiusMm + SparkGapMm + OverburnMm + StockMm;

    public double LagMm(double thicknessMm, double wireRadiusMm) =>
        FlushPressure * FeedMmPerMin * SpeedScale * wireRadiusMm * thicknessMm * thicknessMm
        / (4000.0 * Tension);

    public double CornerScale(double turnDeg) =>
        turnDeg < CornerAngleDeg
            ? 1.0
            : 1.0 - (1.0 - CornerSpeedScale) * Math.Clamp((turnDeg - CornerAngleDeg) / (180.0 - CornerAngleDeg), 0.0, 1.0);

    public Seq<double> CornerStations(Seq<double> stations, double perimeter, bool closed) =>
        CornerSlowMm == 0.0
            ? stations
            : stations.Bind(station => Seq(station, station - CornerSlowMm / perimeter, station + CornerSlowMm / perimeter))
                .Map(station => closed ? station - Math.Floor(station) : Math.Clamp(station, 0.0, 1.0));

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref int pass,
        ref double sparkGapMm,
        ref double overburnMm,
        ref double stockMm,
        ref double speedScale,
        ref double power,
        ref double flushPressure,
        ref double tension,
        ref double feedMmPerMin,
        ref double wireSpeedMmPerMin,
        ref double cornerSlowMm,
        ref double cornerSpeedScale,
        ref double cornerAngleDeg,
        ref WireFinish finish) {
        ReadOnlySpan<double> values = [sparkGapMm, overburnMm, stockMm, speedScale, power, flushPressure, tension,
            feedMmPerMin, wireSpeedMmPerMin, cornerSlowMm, cornerSpeedScale, cornerAngleDeg];
        if (pass < 1 || !TensorPrimitives.IsFiniteAll<double>(values)
            || sparkGapMm < 0.0 || overburnMm < 0.0 || stockMm < 0.0 || speedScale <= 0.0
            || power <= 0.0 || flushPressure < 0.0 || tension <= 0.0 || feedMmPerMin <= 0.0
            || wireSpeedMmPerMin <= 0.0 || cornerSlowMm < 0.0 || cornerSpeedScale <= 0.0 || cornerSpeedScale > 1.0
            || cornerAngleDeg < 0.0 || cornerAngleDeg >= 180.0)
            validationError = new ValidationError("wire:pass");
    }
}

public sealed record WireContext(
    string MachineBook,
    string Generator,
    string Material,
    string Wire,
    string Dielectric);

[ComplexValueObject]
public sealed partial class WireSchedule {
    public WireContext Context { get; }
    public Length Thickness { get; }
    public Arr<WirePass> Passes { get; }

    public static Fin<WireSchedule> Admit(WireContext context, Length thickness, Arr<WirePass> passes) =>
        Validate(context, thickness, passes, out WireSchedule admitted).Admitted(admitted);

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref WireContext context,
        ref Length thickness,
        ref Arr<WirePass> passes) {
        if (!(ValidityClaim.All(
            Witness.Keyed(context.MachineBook), Witness.Keyed(context.Generator), Witness.Keyed(context.Material), Witness.Keyed(context.Wire),
            Witness.Keyed(context.Dielectric), ValidityClaim.Positive(thickness.Millimeters), !passes.IsEmpty,
            toSeq(passes).Map(static (pass, index) => pass.Pass == index + 1).ForAll(static valid => valid))))
            validationError = new ValidationError("wire:schedule");
    }
}

[ComplexValueObject]
public sealed partial class GuidePlanes {
    public Length LowerZ { get; }
    public Length UpperZ { get; }
    public Length ProgramZ { get; }
    public Angle MaxTaper { get; }

    public double SpanMm => UpperZ.Millimeters - LowerZ.Millimeters;

    public double ShiftAt(double targetZmm, double baseZmm, double taperDeg) =>
        Math.Tan(taperDeg * Math.PI / 180.0)
        * (Math.Max(targetZmm - baseZmm, 0.0) - Math.Max(ProgramZ.Millimeters - baseZmm, 0.0));

    public Fin<Unit> Envelope(Point3d lower, Point3d upper) {
        double demand = Math.Atan2(
            Math.Sqrt(Math.Pow(upper.X - lower.X, 2.0) + Math.Pow(upper.Y - lower.Y, 2.0)), SpanMm)
            * 180.0 / Math.PI;
        return demand <= MaxTaper.Degrees
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new FabricationFault.WireTaperExceeded(demand, MaxTaper.Degrees));
    }

    public static Fin<GuidePlanes> Admit(Length lowerZ, Length upperZ, Length programZ, Angle maxTaper) =>
        Validate(lowerZ, upperZ, programZ, maxTaper, out GuidePlanes admitted).Admitted(admitted);

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Length lowerZ,
        ref Length upperZ,
        ref Length programZ,
        ref Angle maxTaper) {
        if (!TensorPrimitives.IsFiniteAll<double>(
                [lowerZ.Millimeters, upperZ.Millimeters, programZ.Millimeters, maxTaper.Degrees])
            || upperZ <= lowerZ || maxTaper.Degrees < 0.0)
            validationError = new ValidationError("wire:guides");
    }
}

public readonly record struct StationPair(double Lower, double Upper);

[ComplexValueObject]
public sealed partial class WireCorrespondence {
    public static readonly WireCorrespondence Identity = Create(
        WireDirection.Forward,
        [new StationPair(0.0, 0.0), new StationPair(1.0, 1.0)]);

    public WireDirection UpperDirection { get; }
    public Arr<StationPair> Anchors { get; }

    public double UpperAt(double lower) =>
        toSeq(Anchors).Zip(toSeq(Anchors).Skip(1)).Find(pair => lower >= pair.First.Lower && lower <= pair.Second.Lower)
            .Map(pair => pair.First.Upper
                + ((lower - pair.First.Lower) / (pair.Second.Lower - pair.First.Lower)) * (pair.Second.Upper - pair.First.Upper))
            .Map(upper => UpperDirection.Reversed ? 1.0 - upper : upper)
            .IfNone(lower);

    public static Fin<WireCorrespondence> Admit(WireDirection direction, Arr<StationPair> anchors) =>
        Validate(direction, anchors, out WireCorrespondence admitted).Admitted(admitted);

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref WireDirection upperDirection,
        ref Arr<StationPair> anchors) {
        bool bounded = !anchors.IsEmpty && anchors.ForAll(static row => ValidityClaim.All(
            ValidityClaim.Finite([row.Lower, row.Upper]), row.Lower >= 0.0, row.Lower <= 1.0, row.Upper >= 0.0, row.Upper <= 1.0));
        bool terminal = bounded && anchors[0] == new StationPair(0.0, 0.0) && anchors[^1] == new StationPair(1.0, 1.0);
        if (!(terminal && toSeq(anchors).Zip(toSeq(anchors).Skip(1)).ForAll(static pair =>
                pair.First.Lower < pair.Second.Lower && pair.First.Upper < pair.Second.Upper)))
            validationError = new ValidationError("wire:correspondence");
    }
}

[Union]
public abstract partial record WireAccess {
    public sealed record OpenEdge(double Station) : WireAccess;
    public sealed record StartHole(Point3d Point, double DiameterMm) : WireAccess;
    public sealed record Automatic(Point3d Point, int Attempts) : WireAccess;
    public sealed record Channel(Loop Path, double Station) : WireAccess;

    public Option<double> Start => Switch(
        openEdge: static row => Some(row.Station),
        startHole: static _ => Option<double>.None,
        automatic: static _ => Option<double>.None,
        channel: static row => Some(row.Station));

    public WireBlock Entry(WireBlock first, WireGuidePoint guide) => first with {
        Pass = 0,
        Ring = 0,
        Progress = 0.0,
        TraversedMm = 0.0,
        Lower = guide,
        Upper = guide,
        Action = new WireAction.Access(this),
        LagMm = 0.0,
        UpperCornerRadiusMm = 0.0,
        RotaryDeg = Option<double>.None,
        Recovery = Option<WireRecovery>.None,
    };
}

public readonly record struct BridgeWindow(double From, double To);

[Union]
public abstract partial record SlugRetention {
    public sealed record FullCut : SlugRetention;
    public sealed record Bridged(Arr<BridgeWindow> Windows, WireRelease Release) : SlugRetention;

    public bool Cutting(double station, int pass, int finalPass) => Switch(
        state: (station, pass, finalPass),
        fullCut: static (_, _) => true,
        bridged: static (state, row) => row.Release.Released(state.pass, state.finalPass)
            || !row.Windows.Exists(window => state.station >= window.From && state.station < window.To));

    public Seq<double> Stations => Switch(
        fullCut: static _ => Seq<double>(),
        bridged: static row => row.Windows.Bind(static window => Seq(window.From, window.To)));
}

[ComplexValueObject]
public sealed partial class WireRecovery {
    public int Attempts { get; }
    public double RetractMm { get; }
    public double RestartLeadMm { get; }
    public bool AutomaticRethread { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref int attempts,
        ref double retractMm,
        ref double restartLeadMm,
        ref bool automaticRethread) {
        if (attempts < 0 || !TensorPrimitives.IsFiniteAll<double>([retractMm, restartLeadMm])
            || retractMm < 0.0 || restartLeadMm < 0.0 || (!automaticRethread && attempts > 0))
            validationError = new ValidationError("wire:recovery");
    }
}

[Union]
public abstract partial record WireCycle(
    double TaperDemand,
    Seq<double> Stations,
    Option<WireCorrespondence> Correspondence,
    Option<RotaryFrame> Spindle) {
    public sealed record Contour(TaperCornerMode Corners)
        : WireCycle(0.0, Seq<double>(), None, None);
    public sealed record TaperContour(double TaperDeg, TaperCornerMode Corners)
        : WireCycle(Math.Abs(TaperDeg), Seq<double>(), None, None);
    public sealed record FourAxis(Loop UpperProfile, WireCorrespondence Registration)
        : WireCycle(0.0, Registration.Anchors.Map(static anchor => anchor.Lower).ToSeq(), Some(Registration), None);
    public sealed record NoCorePocket(double StepOverMm, int MaxPasses)
        : WireCycle(0.0, Seq<double>(), None, None);
    public sealed record Collar(double LandZ, double TaperDeg, TaperCornerMode Corners)
        : WireCycle(Math.Abs(TaperDeg), Seq<double>(), None, None);
    public sealed record Rotary(Point3d AxisOrigin, Vector3d Axis, double PitchMm)
        : WireCycle(0.0, Seq<double>(), None, Some(new RotaryFrame(AxisOrigin, Axis, PitchMm)));
    public sealed record VariableTaper(Arr<TaperKnot> AngleLaw, TaperCornerMode Corners)
        : WireCycle(
            AngleLaw.Fold(0.0, static (peak, knot) => Math.Max(peak, Math.Abs(knot.AngleDeg))),
            AngleLaw.Map(static knot => knot.Station).ToSeq(),
            None,
            None);
    public sealed record Cutoff(double HandoffStation)
        : WireCycle(0.0, Seq(HandoffStation), None, None);
}

public readonly record struct TaperKnot(double Station, double AngleDeg);

public readonly record struct RotaryFrame(Point3d Origin, Vector3d Axis, double PitchMm) {
    public double AngleAt(double cutLengthMm) => 360.0 * cutLengthMm / PitchMm;
}

[ComplexValueObject]
public sealed partial class WirePolicy {
    public WireCycle Cycle { get; }
    public Length WireRadius { get; }
    public GuidePlanes Guides { get; }
    public WireSchedule Schedule { get; }
    public WireAccess Access { get; }
    public SlugRetention Retention { get; }
    public WireRecovery Recovery { get; }

    public static Fin<WirePolicy> Admit(
        WireCycle cycle,
        Length wireRadius,
        Length thickness,
        Length lowerGuideZ,
        Length upperGuideZ,
        Length programZ,
        Angle maxTaper,
        WireContext context,
        Arr<WirePass> passes,
        WireAccess access,
        SlugRetention retention,
        WireRecovery recovery) =>
        from guides in GuidePlanes.Admit(lowerGuideZ, upperGuideZ, programZ, maxTaper)
        from schedule in WireSchedule.Admit(context, thickness, passes)
        from admitted in Validate(cycle, wireRadius, guides, schedule, access, retention, recovery, out WirePolicy policy)
            .Admitted(policy)
        select admitted;

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref WireCycle cycle,
        ref Length wireRadius,
        ref GuidePlanes guides,
        ref WireSchedule schedule,
        ref WireAccess access,
        ref SlugRetention retention,
        ref WireRecovery recovery) {
        double radiusMm = wireRadius.Millimeters;
        validationError = (
            AdmissionSlots.Gate(ValidityClaim.Positive(radiusMm), FabConcern.Toolpath, "wire-policy:wire-radius", FabricationFault.Inadmissible),
            AdmissionSlots.Gate(Valid(cycle), FabConcern.Toolpath, "wire-policy:cycle", FabricationFault.Inadmissible),
            AdmissionSlots.Gate(Valid(retention), FabConcern.Toolpath, "wire-policy:retention", FabricationFault.Inadmissible),
            AdmissionSlots.Gate(cycle.TaperDemand <= guides.MaxTaper.Degrees,
                FabConcern.Toolpath, "wire-policy:taper-envelope", FabricationFault.Inadmissible))
            .Apply(static (_, _, _, _) => unit)
            .As()
            .Match<ValidationError?>(
                Fail: static _ => new ValidationError("wire-policy"),
                Succ: static _ => null);
    }


    private static bool Valid(SlugRetention retention) => retention.Switch(
        fullCut: static _ => true,
        bridged: static row => !row.Windows.IsEmpty
            && row.Windows.ForAll(static window => ValidityClaim.All(
                ValidityClaim.Finite([window.From, window.To]), window.From >= 0.0, window.From < window.To, window.To <= 1.0))
            && toSeq(row.Windows).Zip(toSeq(row.Windows).Skip(1))
                .ForAll(static pair => pair.First.To <= pair.Second.From));

    private static bool Valid(WireCycle cycle) => cycle.Switch(
        contour: static _ => true,
        taperContour: static row => double.IsFinite(row.TaperDeg),
        fourAxis: static row => row.UpperProfile.Count >= 2,
        noCorePocket: static row => ValidityClaim.All(ValidityClaim.Positive(row.StepOverMm), row.MaxPasses > 0),
        collar: static row => ValidityClaim.Finite([row.LandZ, row.TaperDeg]),
        rotary: static row => ValidityClaim.All(row.AxisOrigin.IsValid, row.Axis.IsValid, row.Axis.Length > 0.0, ValidityClaim.Positive(row.PitchMm)),
        variableTaper: static row => row.AngleLaw.Count >= 2
            && row.AngleLaw.ForAll(static knot => ValidityClaim.Finite([knot.Station, knot.AngleDeg]))
            && row.AngleLaw[0].Station == 0.0 && row.AngleLaw[^1].Station == 1.0
            && toSeq(row.AngleLaw).Zip(toSeq(row.AngleLaw).Skip(1))
                .ForAll(static pair => pair.First.Station < pair.Second.Station),
        cutoff: static row => double.IsFinite(row.HandoffStation)
            && row.HandoffStation >= 0.0 && row.HandoffStation <= 1.0);
}

public sealed record WireDemand(WirePolicy Policy, Loop Profile, ProcessBudget.Erosion Budget);

[ComplexValueObject]
public sealed partial class WireJob {
    public WirePolicy Policy { get; }
    public Loop Profile { get; }
    public ProcessBudget.Erosion Budget { get; }

    public static Fin<WireJob> Admit(WireDemand? candidate) =>
        from raw in Optional(candidate).ToFin(new KernelFault.InvalidValue("wire", "wire:demand"))
        from admitted in Validate(raw.Policy, raw.Profile, raw.Budget, out WireJob job).Admitted(job)
        select admitted;

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref WirePolicy policy,
        ref Loop profile,
        ref ProcessBudget.Erosion budget) {
        if (!(profile.Count >= 2 && Reachable(policy.Access, profile)))
            validationError = new ValidationError("wire:job");
    }

    private static bool Reachable(WireAccess access, Loop profile) => access.Switch(
        state: profile,
        openEdge: static (loop, row) => double.IsFinite(row.Station) && row.Station >= 0.0 && row.Station <= 1.0
            && (loop.Closed || row.Station is 0.0 or 1.0),
        startHole: static (_, row) => ValidityClaim.All(row.Point.IsValid, ValidityClaim.Positive(row.DiameterMm)),
        automatic: static (_, row) => row.Point.IsValid && row.Attempts > 0,
        channel: static (_, row) => row.Path.Count >= 2
            && double.IsFinite(row.Station) && row.Station >= 0.0 && row.Station <= 1.0);
}

// --- [EVIDENCE] ------------------------------------------------------------------------
public readonly record struct WireGuidePoint(Point3d Point, int Span, double Bulge, Vector3d Normal) {
    public Vector3d SpanNormal(Loop loop) {
        Vector3d tangent = loop.At(Span + 1) - loop.At(Span);
        return tangent.Unitize() ? new Vector3d(-tangent.Y, tangent.X, 0.0) : Vector3d.Unset;
    }
}

public readonly record struct WireProcess(
    double FeedMmPerMin,
    double Power,
    double FlushPressure,
    double Tension);

[Union]
public abstract partial record WireAction {
    public sealed record Access(WireAccess Source) : WireAction;
    public sealed record Cut(WireProcess Process) : WireAction;
    public sealed record Bridge(double FeedMmPerMin) : WireAction;
    public sealed record Handoff : WireAction;

    public WireActionKind Kind => Switch(
        access: static _ => WireActionKind.Access,
        cut: static _ => WireActionKind.Cut,
        bridge: static _ => WireActionKind.Bridge,
        handoff: static _ => WireActionKind.Handoff);

    public double Duration(double distanceMm) => Switch(
        state: Math.Abs(distanceMm),
        access: static (_, _) => 0.0,
        cut: static (distance, row) => distance / row.Process.FeedMmPerMin * 60.0,
        bridge: static (distance, row) => distance / row.FeedMmPerMin * 60.0,
        handoff: static (_, _) => 0.0);
}

public readonly record struct WireBlock(
    int Pass,
    int Ring,
    double Station,
    double Progress,
    double TraversedMm,
    WireGuidePoint Lower,
    WireGuidePoint Upper,
    WireAction Action,
    double LagMm,
    double UpperCornerRadiusMm,
    Option<double> RotaryDeg,
    Option<WireRecovery> Recovery) {
    public double SpanTo(WireBlock next) =>
        Pass == next.Pass && Ring == next.Ring ? next.TraversedMm - TraversedMm : 0.0;

    public double DurationTo(WireBlock next) => Action.Duration(SpanTo(next));
}

public sealed record WirePassEvidence(
    int Pass,
    WireFinish Finish,
    double OffsetMm,
    double CutLengthMm,
    double WireConsumedMm,
    double MaxLagMm,
    int Bridges,
    int RecoveryBudget,
    BudgetEvidence Evidence) {
    public static WirePassEvidence From(WireJob job, Seq<WireBlock> blocks, WirePass pass) {
        Seq<WireBlock> rows = blocks.Filter(block => block.Pass == pass.Pass);
        double cut = rows.Zip(rows.Skip(1))
            .Filter(static pair => pair.First.Action is WireAction.Cut)
            .Fold(0.0, static (length, pair) => length + pair.First.SpanTo(pair.Second));
        return new WirePassEvidence(
            pass.Pass,
            pass.Finish,
            pass.Offset(job.Policy.WireRadius.Millimeters),
            cut,
            pass.WireSpeedMmPerMin * cut / (pass.FeedMmPerMin * pass.SpeedScale),
            rows.Map(static block => block.LagMm).Max(0.0),
            rows.Filter(static block => block.Action is WireAction.Bridge).Count,
            job.Policy.Recovery.Attempts,
            job.Budget.Evidence);
    }
}

public sealed record WireProgram(
    Seq<WireBlock> Blocks,
    Seq<WirePassEvidence> Passes,
    WireContext Context,
    WireAccess Access,
    SlugRetention Retention,
    WireRecovery Recovery,
    Option<WireCorrespondence> Correspondence,
    Option<RotaryFrame> Rotary,
    SpecializedToolpathEnvelope Specialized) {
    public MotionDirective SpecializedDirective => new MotionDirective.Specialized(
        Blocks.IsEmpty ? -1 : Blocks.Count - 1, Specialized);
    public PostSource PostingSource => new PostSource.Specialized(Specialized);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static partial class ToolpathRowMap {
    [MapProperty([nameof(WireBlock.Lower), nameof(WireGuidePoint.Point)], [nameof(SpecializedToolpathRow.Wire.Lower)])]
    [MapProperty([nameof(WireBlock.Upper), nameof(WireGuidePoint.Point)], [nameof(SpecializedToolpathRow.Wire.Upper)])]
    [MapProperty([nameof(WireBlock.Action), nameof(WireAction.Kind)], [nameof(SpecializedToolpathRow.Wire.Action)])]
    [MapperIgnoreSource(nameof(WireBlock.Ring))]
    [MapperIgnoreSource(nameof(WireBlock.Recovery))]
    public static partial SpecializedToolpathRow.Wire ToRow(WireBlock block);
}

public static class WireEdm {
    public static Fin<TOut> Generate<TOut>(WireDemand? raw, Func<WireProgram, TOut> project) =>
        from _ in Optional(project).ToFin(new KernelFault.InvalidValue("wire", "wire:projection"))
        from job in WireJob.Admit(raw)
        from program in Dispatch(job)
        from projected in Op.Of().Catch(() => Fin.Succ(project(program)))
        select projected;

    public static Fin<(Seq<Move> Moves, MotionDirective Directive)> Lower(WireProgram program) =>
        program.Blocks
            .Map(static block => block.Action.Switch(
                state: block.Lower.Point,
                access: static (point, _) => Move.Rapid.Of(point),
                cut: static (point, row) => Move.Linear.Of(point, row.Process.FeedMmPerMin),
                bridge: static (point, row) => Move.Linear.Of(point, row.FeedMmPerMin),
                handoff: static (point, _) => Move.Rapid.Of(point)))
            .TraverseM(identity)
            .As()
            .Map(moves => (moves, program.SpecializedDirective));

    private static Fin<WireProgram> Dispatch(WireJob job) => job.Policy.Cycle.Switch(
        state: job,
        contour: static (admitted, row) => Boundary(admitted, static _ => 0.0, admitted.Policy.Guides.LowerZ.Millimeters, row.Corners)
            .Bind(rows => Program(admitted, rows)),
        taperContour: static (admitted, row) => Boundary(admitted, _ => row.TaperDeg, admitted.Policy.Guides.LowerZ.Millimeters, row.Corners)
            .Bind(rows => Program(admitted, rows)),
        fourAxis: static (admitted, row) => Paired(admitted, row).Bind(rows => Program(admitted, rows)),
        noCorePocket: static (admitted, row) => Clearing(admitted, row).Bind(rows => Program(admitted, rows)),
        collar: static (admitted, row) => Boundary(admitted, _ => row.TaperDeg, row.LandZ, row.Corners)
            .Bind(rows => Program(admitted, rows)),
        rotary: static (admitted, row) => Boundary(admitted, static _ => 0.0, admitted.Policy.Guides.LowerZ.Millimeters, TaperCornerMode.ConstantLand)
            .Map(rows => rows.Map(pass => RotaryBlocks(pass, new RotaryFrame(row.AxisOrigin, row.Axis, row.PitchMm))))
            .Bind(rows => Program(admitted, rows)),
        variableTaper: static (admitted, row) => Boundary(admitted, Taper(row), admitted.Policy.Guides.LowerZ.Millimeters, row.Corners)
            .Bind(rows => Program(admitted, rows)),
        cutoff: static (admitted, row) => Boundary(admitted, static _ => 0.0, admitted.Policy.Guides.LowerZ.Millimeters, TaperCornerMode.Reduced)
            .Map(rows => Cutoff(admitted, rows, row.HandoffStation))
            .Bind(rows => Program(admitted, rows)));

    private static Func<double, double> Taper(WireCycle.VariableTaper cycle) {
        IInterpolation law = Interpolate.Linear(
            cycle.AngleLaw.Map(static knot => knot.Station).ToArray(),
            cycle.AngleLaw.Map(static knot => knot.AngleDeg).ToArray());
        return law.Interpolate;
    }

    private static Fin<Seq<Seq<WireBlock>>> Boundary(
        WireJob job,
        Func<double, double> taperAt,
        double baseZ,
        TaperCornerMode corners) =>
        job.Policy.Schedule.Passes.AsIterable().ToSeq().TraverseM(pass =>
            ArcOffset.Single(job.Profile, pass.Offset(job.Policy.WireRadius.Millimeters), "wire:offset")
                .Bind(ring => Emit(job, ring, ring, pass, ring: 0,
                    station => job.Policy.Guides.ShiftAt(job.Policy.Guides.UpperZ.Millimeters, baseZ, taperAt(station)),
                    WireCorrespondence.Identity, corners))).As();

    private static Fin<Seq<Seq<WireBlock>>> Paired(WireJob job, WireCycle.FourAxis cycle) =>
        job.Policy.Schedule.Passes.AsIterable().ToSeq().TraverseM(pass =>
            from lower in ArcOffset.Single(job.Profile, pass.Offset(job.Policy.WireRadius.Millimeters), "wire:offset")
            from upper in ArcOffset.Single(cycle.UpperProfile, pass.Offset(job.Policy.WireRadius.Millimeters), "wire:offset")
            from blocks in Emit(job, lower, upper, pass, ring: 0, static _ => 0.0, cycle.Registration, TaperCornerMode.Conical)
            select blocks).As();

    private static Fin<Seq<Seq<WireBlock>>> Clearing(WireJob job, WireCycle.NoCorePocket cycle) =>
        from rough in toSeq(Enumerable.Range(0, cycle.MaxPasses)).TraverseM(level =>
                ArcOffset.Family(
                        job.Profile,
                        -(job.Policy.Schedule.Passes[0].Offset(job.Policy.WireRadius.Millimeters) + (level * cycle.StepOverMm)),
                        "wire:offset")
                    .Bind(rings => rings.Map((ring, ordinal) => (Ring: ring, Ordinal: ordinal)).TraverseM(row =>
                        Emit(job, row.Ring, row.Ring, job.Policy.Schedule.Passes[0],
                            (level * rings.Count) + row.Ordinal, static _ => 0.0,
                            WireCorrespondence.Identity, TaperCornerMode.Conical)).As())).As()
            .Map(static levels => levels.Bind(static level => level))
        from finish in job.Policy.Schedule.Passes.AsIterable().ToSeq().Tail.TraverseM(pass =>
                ArcOffset.Single(job.Profile, pass.Offset(job.Policy.WireRadius.Millimeters), "wire:offset").Bind(ring =>
                    Emit(job, ring, ring, pass, ring: 0, static _ => 0.0,
                        WireCorrespondence.Identity, TaperCornerMode.Conical))).As()
        select rough + finish;

    private static Seq<WireBlock> RotaryBlocks(Seq<WireBlock> blocks, RotaryFrame frame) =>
        blocks.Fold(
            (Rows: Seq<WireBlock>(), Previous: Option<WireBlock>.None, Distance: 0.0),
            (state, block) => state.Previous
                .Map(previous => state.Distance + previous.SpanTo(block))
                .IfNone(0.0)
                .Apply(distance => (
                    state.Rows.Add(block with { RotaryDeg = Some(frame.AngleAt(distance)) }),
                    Some(block),
                    distance)))
            .Rows;

    private static Seq<Seq<WireBlock>> Cutoff(WireJob job, Seq<Seq<WireBlock>> rows, double handoffStation) {
        double start = job.Policy.Access.Start.IfNone(0.0);
        double handoff = handoffStation >= start ? handoffStation - start : 1.0 - start + handoffStation;
        return rows.Map(pass => pass.Filter(block => block.Action is WireAction.Access || block.Progress <= handoff))
            .Map(static pass => pass.Map((block, index) =>
                index == pass.Count - 1 && block.Action is not WireAction.Access
                    ? block with { Action = new WireAction.Handoff(), Recovery = Option<WireRecovery>.None }
                    : block));
    }

    private static Fin<Seq<WireBlock>> Emit(
        WireJob job,
        Loop lower,
        Loop upper,
        WirePass pass,
        int ring,
        Func<double, double> upperShift,
        WireCorrespondence correspondence,
        TaperCornerMode corners) =>
        from staged in Stations(job, lower, pass)
        let lowerPath = staged.Path
        let marks = staged.Marks
        let upperPath = Native(upper)
        let lowerLength = lowerPath.PathLength()
        let upperLength = upperPath.PathLength()
        let lag = pass.LagMm(job.Policy.Schedule.Thickness.Millimeters, job.Policy.WireRadius.Millimeters)
        from blocks in marks.TraverseM(mark =>
            from low in Sample(lowerPath, lower, mark.Station * lowerLength)
            from high in Sample(upperPath, upper, correspondence.UpperAt(mark.Station) * upperLength)
            let shift = upperShift(mark.Station)
            let upperPoint = high.Point + corners.Direction(high, upper) * shift
            let lowerGuide = low with { Point = new Point3d(low.Point.X, low.Point.Y, job.Policy.Guides.LowerZ.Millimeters) }
            let upperGuide = high with { Point = new Point3d(upperPoint.X, upperPoint.Y, job.Policy.Guides.UpperZ.Millimeters) }
            from _ in job.Policy.Guides.Envelope(lowerGuide.Point, upperGuide.Point)
            let process = new WireProcess(
                pass.FeedMmPerMin * pass.SpeedScale * mark.SpeedScale,
                pass.Power,
                pass.FlushPressure,
                pass.Tension)
            select new WireBlock(
                pass.Pass,
                ring,
                mark.Station,
                mark.Progress,
                mark.Progress * lowerLength,
                lowerGuide,
                upperGuide,
                job.Policy.Retention.Cutting(mark.Station, pass.Pass, job.Policy.Schedule.Passes.Count)
                    ? new WireAction.Cut(process)
                    : new WireAction.Bridge(process.FeedMmPerMin),
                lag * mark.SpeedScale,
                mark.TurnDeg > 0.0 ? corners.CornerRadiusMm(shift) : 0.0,
                Option<double>.None,
                mark.Progress == 0.0 && job.Policy.Recovery.Attempts > 0
                    ? Some(job.Policy.Recovery)
                    : Option<WireRecovery>.None)).As()
        select blocks;

    private static Polyline<double> Native(Loop loop) =>
        new(toSeq(loop.Vertices).Map((point, index) => PlineVertex<double>.FromSlice([point.X, point.Y, loop.BulgeAt(index)])), loop.Closed);

    private static Fin<WireGuidePoint> Sample(Polyline<double> path, Loop source, double length) =>
        path.FindPointAtPathLength(length) switch {
            (true, int span, Vector2<double> point, _) => Normal(path, source, span, point).Map(normal => new WireGuidePoint(
                new Point3d(point.X, point.Y, source.Plane), span, source.BulgeAt(span), normal)),
            _ => Fin.Fail<WireGuidePoint>(new GeometryFault.DegenerateInput(Kind.Curve, None, "wire:station")),
        };

    private static Fin<Vector3d> Normal(Polyline<double> path, Loop source, int span, Vector2<double> point) {
        int next = source.Closed ? path.NextWrappingIndex(span) : Math.Min(span + 1, source.Count - 1);
        Vector2<double> tangent = PlineSeg.SegTangentVector(path[span], path[next], point);
        Vector3d normal = new(-tangent.Y, tangent.X, 0.0);
        return normal.Unitize()
            ? Fin.Succ(normal)
            : Fin.Fail<Vector3d>(new GeometryFault.DegenerateInput(Kind.Curve, span, "wire:tangent"));
    }

    private static double SpanLength(Loop loop, int index) {
        double chord = loop.At(index).DistanceTo(loop.At(index + 1));
        double bulge = Math.Abs(loop.BulgeAt(index));
        return bulge == 0.0
            ? chord
            : chord * (1.0 + bulge * bulge) * Math.Atan(bulge) / bulge;
    }

    private static double TurnDeg(Polyline<double> path, Loop loop, int index) {
        if (!loop.Closed && (index == 0 || index >= loop.Spans))
            return 0.0;
        Vector2<double> incoming = PlineSeg.SegTangentVector(
            path[path.PrevWrappingIndex(index)], path[index], path[index].Pos());
        Vector2<double> outgoing = PlineSeg.SegTangentVector(
            path[index], path[path.NextWrappingIndex(index)], path[index].Pos());
        return Vector3d.VectorAngle(
            new Vector3d(incoming.X, incoming.Y, 0.0),
            new Vector3d(outgoing.X, outgoing.Y, 0.0)) * 180.0 / Math.PI;
    }

    private static Seq<(double Station, double TurnDeg)> VertexStations(Loop loop, Polyline<double> path, double perimeter) =>
        toSeq(Enumerable.Range(0, loop.Spans))
            .Map(index => SpanLength(loop, index))
            .Fold(Seq(0.0), static (rows, length) => rows.Add(rows.Last.IfNone(0.0) + length))
            .Take(loop.Closed ? loop.Spans : loop.Spans + 1)
            .ToSeq()
            .Map((cumulative, index) => (Station: cumulative / perimeter, TurnDeg: TurnDeg(path, loop, index)));

    private static Fin<(Polyline<double> Path, Seq<(double Station, double Progress, double SpeedScale, double TurnDeg)> Marks)> Stations(
        WireJob job,
        Loop loop,
        WirePass pass) {
        Polyline<double> path = Native(loop);
        double perimeter = path.PathLength();
        if (!(perimeter > 0.0))
            return Fin.Fail<(Polyline<double>, Seq<(double, double, double, double)>)>(
                new GeometryFault.DegenerateInput(Kind.Curve, None, "wire:perimeter"));
        Seq<(double Station, double TurnDeg)> vertices = VertexStations(loop, path, perimeter);
        double start = job.Policy.Access.Start.IfNone(0.0);
        double window = pass.CornerSlowMm / perimeter;
        HashMap<int, Seq<(double Station, double TurnDeg)>> corners = window > 0.0
            ? vertices.Fold(
                HashMap<int, Seq<(double Station, double TurnDeg)>>.Empty,
                (index, vertex) => index.AddOrUpdate(
                    (int)Math.Floor(vertex.Station / window), held => held.Add(vertex), Seq(vertex)))
            : HashMap<int, Seq<(double Station, double TurnDeg)>>.Empty;
        Seq<(double Station, double Progress, double SpeedScale, double TurnDeg)> ordered =
            toSeq(pass.CornerStations(vertices.Map(static row => row.Station), perimeter, loop.Closed)
                .Concat(job.Policy.Retention.Stations)
                .Concat(job.Policy.Cycle.Stations)
                .Concat(Seq(0.0, 1.0, start))
                .Filter(static station => double.IsFinite(station) && station >= 0.0 && station <= 1.0)
                .Map(station => loop.Closed ? station - Math.Floor(station) : station)
                .DistinctBy(station => Math.Round(station * perimeter / loop.Tolerance.Absolute.Value))
                .Map(station => (
                    Station: station,
                    Progress: station >= start ? station - start : 1.0 - start + station,
                    Turn: window <= 0.0
                        ? 0.0
                        : Range(-1, 3).ToSeq()
                            .Bind(shift => corners
                                .Find((int)Math.Floor(station / window) + shift)
                                .IfNone(Seq<(double Station, double TurnDeg)>()))
                            .Filter(vertex => (loop.Closed
                                ? Math.Min(Math.Abs(vertex.Station - station), 1.0 - Math.Abs(vertex.Station - station))
                                : Math.Abs(vertex.Station - station)) <= window)
                            .Fold(0.0, static (peak, vertex) => Math.Max(peak, vertex.TurnDeg))))
                .Map(row => (row.Station, row.Progress, SpeedScale: pass.CornerScale(row.Turn), TurnDeg: row.Turn))
                .Filter(row => loop.Closed || row.Station >= start)
                .OrderBy(static row => row.Progress));
        return Fin.Succ((path, loop.Closed
            ? ordered.Head
                .Map(first => ordered.Add((Station: start, Progress: 1.0, first.SpeedScale, first.TurnDeg)))
                .IfNone(ordered)
            : ordered));
    }

    private static Fin<WireProgram> Program(WireJob job, Seq<Seq<WireBlock>> rows) =>
        from access in rows.Bind(static pass => pass).Head
            .Traverse(first => Access(job, first))
            .As()
        let cut = rows.Bind(static pass => pass)
        let blocks = access.Map(entry => Seq(entry).Concat(cut)).IfNone(cut)
        from envelope in SpecializedToolpathEnvelope.Admit(
            SpecializedToolpathKind.Wire,
            blocks.Map(static row => (SpecializedToolpathRow)ToolpathRowMap.ToRow(row)),
            blocks.Zip(blocks.Skip(1)).Sum(static pair => pair.First.DurationTo(pair.Second)))
        select new WireProgram(
            blocks,
            job.Policy.Schedule.Passes.Map(pass => WirePassEvidence.From(job, blocks, pass)),
            job.Policy.Schedule.Context,
            job.Policy.Access,
            job.Policy.Retention,
            job.Policy.Recovery,
            job.Policy.Cycle.Correspondence,
            job.Policy.Cycle.Spindle,
            envelope);

    private static Fin<WireBlock> Access(WireJob job, WireBlock first) => job.Policy.Access.Switch(
        state: (job, first),
        openEdge: static (state, _) => Fin.Succ(state.job.Policy.Access.Entry(state.first, state.first.Lower)),
        startHole: static (state, row) => Fin.Succ(state.job.Policy.Access.Entry(
            state.first, state.first.Lower with { Point = row.Point })),
        automatic: static (state, row) => Fin.Succ(state.job.Policy.Access.Entry(
            state.first, state.first.Lower with { Point = row.Point })),
        channel: static (state, row) => Sample(Native(row.Path), row.Path, row.Station * row.Path.Length())
            .Map(guide => state.job.Policy.Access.Entry(state.first, guide)));
}
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
