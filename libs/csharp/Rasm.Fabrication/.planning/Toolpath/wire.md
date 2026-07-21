# [RASM_FABRICATION_WIRE_EDM]

`WireEdm.Generate` admits one traveling-wire demand, resolves context-keyed pass law, derives access and recovery, registers lower-to-upper correspondence, and emits simultaneous-guide `WireBlock` rows through one typed rail. `WireProgram` preserves every electrical, hydraulic, geometric, station, and quality decision without flattening simultaneous guides into sequential motion.

`WireEdm.Generate`, `WireProgram`, and `WireBlock` remain the wire seam names. `WireProgram.SpecializedDirective` projects simultaneous lower/upper guides, process action, lag, rotary state, and derived duration into the shared specialized-toolpath envelope. `ProcessBudget.Erosion` supplies admitted process evidence, `ArcAlgebra.Apply` owns offset topology, `CavalierContours` owns arc-native station measurement, and `FabricationFault.WireTaperExceeded` carries guide-envelope refusal.

## [01]-[INDEX]

- [02]-[ADMISSION]: `WireDemand` materializes dimensional text once; generated owners admit guides, schedules, correspondence, access, retention, and recovery.
- [03]-[GENERATION]: `WireCycle` closes contour, taper, four-axis, clearing, collar, rotary, variable-taper, and cutoff payload timing under one dispatch.
- [04]-[EGRESS]: `WireProgram` projects through a caller-supplied arrow while retaining simultaneous guides and process evidence.

## [02]-[ADMISSION]

`WireJob` owns the whole trusted cutting context. Schedule compatibility is a declared relation over machine book, generator, material, wire, dielectric, and thickness; no named global roster can impersonate that evidence.

- Owner: `WireSchedule` re-admits contiguous generated `WirePass` rows and binds their provenance to `WireContext`; no field-parallel pass DTO survives beside the owner.
- Owner: `WirePass` carries the electrical, hydraulic, and mechanical columns the wire bow and corner law read — `Tension`, `FlushPressure`, `WireSpeedMmPerMin`, and `CornerAngleDeg`; `LagMm` derives the taut-string deflection instead of admitting it as a caller knob.
- Owner: `WireCorrespondence` binds seam, direction, and piecewise station anchors before any four-axis sample occurs.
- Owner: `WireAccess` carries open-edge, drilled, automatic-thread, and inherited-channel access as occurrence payload.
- Owner: `SlugRetention` carries explicit bridge intervals and a payload-bearing `WireRelease`; release evidence never rides a parallel integer knob.
- Owner: `WireRecovery` carries break detection, retract, rethread, restart, and attempt evidence for unattended cuts.
- Packages: `PhysicsQuantity.Length.Admit` converts dimensional text to machining-canonical `double`; `Interpolate.Linear` owns variable-taper evaluation; `TensorPrimitives.IsFiniteAll` admits numeric batches; `Thinktecture` generated owners close construction.
- Boundary: `WireDemand` is raw exactly once, and every interior function consumes `WireJob`.

## [03]-[GENERATION]

`WireCycle` separates payload timing while `WireEdm.Generate` remains the one operation. `Contour`, `TaperContour`, `Collar`, and `VariableTaper` share the boundary-pass algebra; `FourAxis` supplies an independent upper profile with registered correspondence; `NoCorePocket` supplies recession; `Rotary` supplies a cylindrical station map; `Cutoff` supplies a terminal slug handoff.

- Entry: `Generate<TOut>` fixes ingress as `WireDemand?` and parameterizes the egress projection result.
- Auto: every pass derives compound offset, vertex, bridge, taper, correspondence, access, and handoff stations, corner-control transitions, and simultaneous lower/upper samples.
- Auto: four-axis pairing maps registered physical stations rather than incidental indices or normalized-length coincidence.
- Auto: corner control scales by the exact arc turn angle at the nearest vertex, so a tangent-continuous vertex never draws a reversal's slowdown.
- Auto: `TaperCornerMode` applies every taper shift at full magnitude and owns only the upper corner radius, so no mode can silently flatten a taper.
- Auto: access precedes cutting, recovery checkpoints preserve restart custody, and pass quality derives from schedule data.
- Packages: `ArcAlgebra.Apply` owns offset topology; `Polyline<double>.PathLength`, `FindPointAtPathLength`, `PlineSeg.SegTangentVector`, `PrevWrappingIndex`, and `NextWrappingIndex` own arc-native stationing and turn measurement; `LanguageExt` `TraverseM`, `Fold`, and query syntax keep the rail flat.
- Exemption: vertex-station folding, span-frame normalization, and receipt reduction are measured kernel statement boundaries.
- Boundary: `FabricationFault.WireTaperExceeded` refuses guide demand without clamping, and offset failures remain typed.

## [04]-[EGRESS]

`WireProgram` is the inverse-sufficient receipt: every block carries simultaneous guides, physical station, traversed arc length, traversal progress, payload-timed action, wire bow, upper corner radius, and optional rotary position; program custody preserves access, retention, correspondence, context, and recovery beside pass quality evidence. `WireCycle` owns `Correspondence` and `Spindle` beside `Stations` and `TaperDemand`, so program custody reads occurrence evidence from the case rather than re-deriving it through a parallel dispatch.

- Receipt: `WirePassReceipt` preserves schedule identity, quality, removed offset, arc-true cut length, consumed wire, peak wire bow, bridge count, and recovery budget; cut length folds `TraversedMm` deltas, never chord distance between sampled guides.
- Projection: `WireProgram.PostingSource` carries the typed envelope into canonical posting; the caller arrow retains other result projections.
- Consumer: posting retains guide-pair simultaneity, simulation retains specialized rows and duration, and estimation consumes that simulation receipt.
- Growth: a machine-book capability is one `WirePass` row; a new occurrence payload is one `WireCycle` case; a new projection changes only the supplied arrow.
- Boundary: sequential lower/upper `Move` rows cannot represent `WireBlock` and never cross this seam.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------------------------------------------------------
using System.Numerics.Tensors;
using CavalierContours.Core;
using CavalierContours.Polyline;
using LanguageExt;
using LanguageExt.Common;
using MathNet.Numerics.Interpolation;
using Rasm.Domain;
using Rasm.Fabrication.Geometry2D;
using Rasm.Fabrication.Process;
using Rasm.Numerics;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Toolpath;

// --- [VOCABULARY] ---------------------------------------------------------------------------------------------------------------------------------
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

// --- [ADMISSION] ----------------------------------------------------------------------------------------------------------------------------------
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

    // Taut-string midspan bow under the distributed discharge and flush load across the guide span.
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
        if (pass < 1 || finish is null || !TensorPrimitives.IsFiniteAll<double>(values)
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
    public double ThicknessMm { get; }
    public Arr<WirePass> Passes { get; }

    public static Fin<WireSchedule> Admit(WireContext context, double thicknessMm, Arr<WirePass> passes) =>
        Validate(context, thicknessMm, passes, out WireSchedule admitted) is { } error
            ? Fin.Fail<WireSchedule>(new GeometryFault.DegenerateInput(Kind.Curve, -1, error.Message).ToError())
            : Fin.Succ(admitted);

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref WireContext context,
        ref double thicknessMm,
        ref Arr<WirePass> passes) {
        if (context is null || string.IsNullOrWhiteSpace(context.MachineBook) || string.IsNullOrWhiteSpace(context.Generator)
            || string.IsNullOrWhiteSpace(context.Material) || string.IsNullOrWhiteSpace(context.Wire)
            || string.IsNullOrWhiteSpace(context.Dielectric) || !double.IsFinite(thicknessMm) || thicknessMm <= 0.0
            || passes.IsEmpty || passes.Exists(static pass => pass is null)
            || passes.Map(static (pass, index) => pass.Pass == index + 1).Exists(static valid => !valid))
            validationError = new ValidationError("wire:schedule");
    }
}

[ComplexValueObject]
public sealed partial class GuidePlanes {
    public double LowerZ { get; }
    public double UpperZ { get; }
    public double ProgramZ { get; }
    public double MaxTaperDeg { get; }
    public double Span => UpperZ - LowerZ;

    public double ShiftAt(double targetZ, double baseZ, double taperDeg) =>
        Math.Tan(taperDeg * Math.PI / 180.0)
        * (Math.Max(targetZ - baseZ, 0.0) - Math.Max(ProgramZ - baseZ, 0.0));

    public Fin<Unit> Envelope(Point3d lower, Point3d upper) {
        double demand = Math.Atan2(Math.Sqrt(Math.Pow(upper.X - lower.X, 2.0) + Math.Pow(upper.Y - lower.Y, 2.0)), Span)
            * 180.0 / Math.PI;
        return demand <= MaxTaperDeg
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(FabricationFault.WireTaperExceeded(demand, MaxTaperDeg).ToError());
    }

    public static Fin<GuidePlanes> Admit(double lowerZ, double upperZ, double programZ, double maxTaperDeg) =>
        Validate(lowerZ, upperZ, programZ, maxTaperDeg, out GuidePlanes admitted) is { } error
            ? Fin.Fail<GuidePlanes>(new GeometryFault.DegenerateInput(Kind.Curve, -1, error.Message).ToError())
            : Fin.Succ(admitted);

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref double lowerZ,
        ref double upperZ,
        ref double programZ,
        ref double maxTaperDeg) {
        if (!TensorPrimitives.IsFiniteAll<double>([lowerZ, upperZ, programZ, maxTaperDeg])
            || upperZ <= lowerZ || maxTaperDeg < 0.0)
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
        Validate(direction, anchors, out WireCorrespondence admitted) is { } error
            ? Fin.Fail<WireCorrespondence>(new GeometryFault.DegenerateInput(Kind.Curve, -1, error.Message).ToError())
            : Fin.Succ(admitted);

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref WireDirection upperDirection,
        ref Arr<StationPair> anchors) {
        bool bounded = !anchors.IsEmpty && anchors.ForAll(static row => double.IsFinite(row.Lower) && double.IsFinite(row.Upper)
            && row.Lower >= 0.0 && row.Lower <= 1.0 && row.Upper >= 0.0 && row.Upper <= 1.0);
        bool terminal = bounded && anchors[0] == new StationPair(0.0, 0.0) && anchors[^1] == new StationPair(1.0, 1.0);
        bool monotone = terminal && toSeq(anchors).Zip(toSeq(anchors).Skip(1)).ForAll(static pair =>
            pair.First.Lower < pair.Second.Lower && pair.First.Upper < pair.Second.Upper);
        if (upperDirection is null || !monotone)
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

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref int attempts,
        ref double retractMm,
        ref double restartLeadMm,
        ref bool automaticRethread) {
        if (attempts < 0 || !TensorPrimitives.IsFiniteAll<double>([retractMm, restartLeadMm])
            || retractMm < 0.0 || restartLeadMm < 0.0 || !automaticRethread && attempts > 0)
            validationError = new ValidationError("wire:recovery");
    }
}

[Union]
public abstract partial record WireCycle {
    public sealed record Contour(TaperCornerMode Corners) : WireCycle;
    public sealed record TaperContour(double TaperDeg, TaperCornerMode Corners) : WireCycle;
    public sealed record FourAxis(Loop UpperProfile, WireCorrespondence Correspondence) : WireCycle;
    public sealed record NoCorePocket(double StepOverMm, int MaxPasses) : WireCycle;
    public sealed record Collar(double LandZ, double TaperDeg, TaperCornerMode Corners) : WireCycle;
    public sealed record Rotary(Point3d AxisOrigin, Vector3d Axis, double PitchMm) : WireCycle {
        public RotaryFrame Frame => new(AxisOrigin, Axis, PitchMm);
    }
    public sealed record VariableTaper(Arr<TaperKnot> AngleLaw, TaperCornerMode Corners) : WireCycle {
        public double At(double station) => Interpolate.Linear(
            AngleLaw.Map(static knot => knot.Station).ToArray(),
            AngleLaw.Map(static knot => knot.AngleDeg).ToArray()).Interpolate(station);
    }
    public sealed record Cutoff(double HandoffStation) : WireCycle;

    public Seq<double> Stations => Switch(
        contour: static _ => Seq<double>(),
        taperContour: static _ => Seq<double>(),
        fourAxis: static row => row.Correspondence.Anchors.Map(static anchor => anchor.Lower).ToSeq(),
        noCorePocket: static _ => Seq<double>(),
        collar: static _ => Seq<double>(),
        rotary: static _ => Seq<double>(),
        variableTaper: static row => row.AngleLaw.Map(static knot => knot.Station).ToSeq(),
        cutoff: static row => Seq(row.HandoffStation));

    public double TaperDemand => Switch(
        contour: static _ => 0.0,
        taperContour: static row => Math.Abs(row.TaperDeg),
        fourAxis: static _ => 0.0,
        noCorePocket: static _ => 0.0,
        collar: static row => Math.Abs(row.TaperDeg),
        rotary: static _ => 0.0,
        variableTaper: static row => row.AngleLaw.IsEmpty
            ? double.PositiveInfinity
            : row.AngleLaw.Map(static knot => Math.Abs(knot.AngleDeg)).Max(),
        cutoff: static _ => 0.0);

    public Option<WireCorrespondence> Correspondence => Switch(
        contour: static _ => Option<WireCorrespondence>.None,
        taperContour: static _ => Option<WireCorrespondence>.None,
        fourAxis: static row => Some(row.Correspondence),
        noCorePocket: static _ => Option<WireCorrespondence>.None,
        collar: static _ => Option<WireCorrespondence>.None,
        rotary: static _ => Option<WireCorrespondence>.None,
        variableTaper: static _ => Option<WireCorrespondence>.None,
        cutoff: static _ => Option<WireCorrespondence>.None);

    public Option<RotaryFrame> Spindle => Switch(
        contour: static _ => Option<RotaryFrame>.None,
        taperContour: static _ => Option<RotaryFrame>.None,
        fourAxis: static _ => Option<RotaryFrame>.None,
        noCorePocket: static _ => Option<RotaryFrame>.None,
        collar: static _ => Option<RotaryFrame>.None,
        rotary: static row => Some(row.Frame),
        variableTaper: static _ => Option<RotaryFrame>.None,
        cutoff: static _ => Option<RotaryFrame>.None);
}

public readonly record struct TaperKnot(double Station, double AngleDeg);

public readonly record struct RotaryFrame(Point3d Origin, Vector3d Axis, double PitchMm) {
    public double AngleAt(double cutLengthMm) => 360.0 * cutLengthMm / PitchMm;
}

public sealed record WireDemand(
    WireCycle Cycle,
    Loop Profile,
    string WireRadius,
    string Thickness,
    double LowerGuideZ,
    double UpperGuideZ,
    double ProgramZ,
    double MaxTaperDeg,
    ProcessBudget.Erosion Budget,
    WireContext Context,
    Arr<WirePass> Schedule,
    WireAccess Access,
    SlugRetention Retention,
    WireRecovery Recovery);

[ComplexValueObject]
public sealed partial class WireJob {
    public WireCycle Cycle { get; }
    public Loop Profile { get; }
    public double WireRadiusMm { get; }
    public GuidePlanes Guides { get; }
    public ProcessBudget.Erosion Budget { get; }
    public WireSchedule Schedule { get; }
    public WireAccess Access { get; }
    public SlugRetention Retention { get; }
    public WireRecovery Recovery { get; }

    public static Fin<WireJob> Admit(WireDemand? candidate) =>
        from raw in Optional(candidate).ToFin(new GeometryFault.DegenerateInput(Kind.Curve, -1, "wire:demand").ToError())
        from cycle in Optional(raw.Cycle).ToFin(new GeometryFault.DegenerateInput(Kind.Curve, -1, "wire:cycle").ToError())
        from profile in Optional(raw.Profile).ToFin(new GeometryFault.DegenerateInput(Kind.Curve, -1, "wire:profile").ToError())
        from budget in Optional(raw.Budget).ToFin(new GeometryFault.DegenerateInput(Kind.Curve, -1, "wire:budget").ToError())
        from access in Optional(raw.Access).ToFin(new GeometryFault.DegenerateInput(Kind.Curve, -1, "wire:access").ToError())
        from retention in Optional(raw.Retention).ToFin(new GeometryFault.DegenerateInput(Kind.Curve, -1, "wire:retention").ToError())
        from recovery in Optional(raw.Recovery).ToFin(new GeometryFault.DegenerateInput(Kind.Curve, -1, "wire:recovery").ToError())
        from radius in PhysicsQuantity.Length.Admit(raw.WireRadius)
        from thickness in PhysicsQuantity.Length.Admit(raw.Thickness)
        from guides in GuidePlanes.Admit(raw.LowerGuideZ, raw.UpperGuideZ, raw.ProgramZ, raw.MaxTaperDeg)
        from _ in cycle.TaperDemand <= guides.MaxTaperDeg
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(FabricationFault.WireTaperExceeded(cycle.TaperDemand, guides.MaxTaperDeg).ToError())
        from context in Optional(raw.Context).ToFin(new GeometryFault.DegenerateInput(Kind.Curve, -1, "wire:context").ToError())
        from schedule in WireSchedule.Admit(context, thickness, raw.Schedule)
        from admitted in Validate(cycle, profile, radius, guides, budget, schedule,
            access, retention, recovery, out WireJob job) is { } error
            ? Fin.Fail<WireJob>(new GeometryFault.DegenerateInput(Kind.Curve, -1, error.Message).ToError())
            : Fin.Succ(job)
        select admitted;

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref WireCycle cycle,
        ref Loop profile,
        ref double wireRadiusMm,
        ref GuidePlanes guides,
        ref ProcessBudget.Erosion budget,
        ref WireSchedule schedule,
        ref WireAccess access,
        ref SlugRetention retention,
        ref WireRecovery recovery) {
        if (cycle is null || profile is null || guides is null || budget is null || schedule is null || access is null
            || retention is null || recovery is null || profile.Count < 2 || !double.IsFinite(wireRadiusMm) || wireRadiusMm <= 0.0
            || !Valid(cycle) || !Valid(access, profile) || !Valid(retention))
            validationError = new ValidationError("wire:job");
    }

    private static bool Valid(WireAccess access, Loop profile) => access.Switch(
        state: profile,
        openEdge: static (loop, row) => double.IsFinite(row.Station) && row.Station >= 0.0 && row.Station <= 1.0
            && (loop.Closed || row.Station is 0.0 or 1.0),
        startHole: static (_, row) => row.Point.IsValid && double.IsFinite(row.DiameterMm) && row.DiameterMm > 0.0,
        automatic: static (_, row) => row.Point.IsValid && row.Attempts > 0,
        channel: static (_, row) => row.Path is not null && double.IsFinite(row.Station) && row.Station >= 0.0 && row.Station <= 1.0);

    private static bool Valid(SlugRetention retention) => retention.Switch(
        fullCut: static _ => true,
        bridged: static row => row.Release is not null && !row.Windows.IsEmpty
            && row.Windows.ForAll(static window => double.IsFinite(window.From) && double.IsFinite(window.To)
                && window.From >= 0.0 && window.From < window.To && window.To <= 1.0)
            && toSeq(row.Windows).Zip(toSeq(row.Windows).Skip(1))
                .ForAll(static pair => pair.First.To <= pair.Second.From));

    private static bool Valid(WireCycle cycle) => cycle.Switch(
        contour: static row => row.Corners is not null,
        taperContour: static row => row.Corners is not null && double.IsFinite(row.TaperDeg),
        fourAxis: static row => row.UpperProfile is not null && row.Correspondence is not null,
        noCorePocket: static row => double.IsFinite(row.StepOverMm) && row.StepOverMm > 0.0 && row.MaxPasses > 0,
        collar: static row => row.Corners is not null && double.IsFinite(row.LandZ) && double.IsFinite(row.TaperDeg),
        rotary: static row => row.AxisOrigin.IsValid && row.Axis.IsValid && row.Axis.Length > 0.0
            && double.IsFinite(row.PitchMm) && row.PitchMm > 0.0,
        variableTaper: static row => row.Corners is not null && row.AngleLaw.Count >= 2
            && row.AngleLaw.ForAll(static knot => double.IsFinite(knot.Station) && double.IsFinite(knot.AngleDeg))
            && row.AngleLaw[0].Station == 0.0 && row.AngleLaw[^1].Station == 1.0
            && toSeq(row.AngleLaw).Zip(toSeq(row.AngleLaw).Skip(1))
                .ForAll(static pair => pair.First.Station < pair.Second.Station),
        cutoff: static row => double.IsFinite(row.HandoffStation) && row.HandoffStation >= 0.0 && row.HandoffStation <= 1.0);

}

// --- [EVIDENCE] -----------------------------------------------------------------------------------------------------------------------------------
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

    public string Key => Switch(
        access: static _ => "access",
        cut: static _ => "cut",
        bridge: static _ => "bridge",
        handoff: static _ => "handoff");

    public double Duration(double distanceMm) => Switch(
        state: Math.Abs(distanceMm),
        access: static (_, _) => 0.0,
        cut: static (distance, row) => row.Process.FeedMmPerMin > 0.0 ? distance / row.Process.FeedMmPerMin * 60.0 : 0.0,
        bridge: static (distance, row) => row.FeedMmPerMin > 0.0 ? distance / row.FeedMmPerMin * 60.0 : 0.0,
        handoff: static (_, _) => 0.0);
}

public readonly record struct WireBlock(
    int Pass,
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
        Pass == next.Pass ? Math.Max(0.0, next.TraversedMm - TraversedMm) : 0.0;

    public double DurationTo(WireBlock next) => Action.Duration(SpanTo(next));
}

public sealed record WirePassReceipt(
    int Pass,
    WireFinish Finish,
    double OffsetMm,
    double CutLengthMm,
    double WireConsumedMm,
    double MaxLagMm,
    int Bridges,
    int RecoveryBudget,
    BudgetEvidence Evidence) {
    public static WirePassReceipt From(WireJob job, Seq<WireBlock> blocks, WirePass pass) {
        Seq<WireBlock> rows = blocks.Filter(block => block.Pass == pass.Pass);
        double cut = rows.Zip(rows.Skip(1))
            .Filter(static pair => pair.First.Action is WireAction.Cut)
            .Fold(0.0, static (length, pair) => length + pair.First.SpanTo(pair.Second));
        return new WirePassReceipt(
            pass.Pass,
            pass.Finish,
            pass.Offset(job.WireRadiusMm),
            cut,
            pass.FeedMmPerMin * pass.SpeedScale == 0.0 ? 0.0 : pass.WireSpeedMmPerMin * cut / (pass.FeedMmPerMin * pass.SpeedScale),
            rows.IsEmpty ? 0.0 : rows.Map(static block => block.LagMm).Max(),
            rows.Filter(static block => block.Action is WireAction.Bridge).Count,
            job.Recovery.Attempts,
            job.Budget.Evidence);
    }
}

public sealed record WireProgram(
    Seq<WireBlock> Blocks,
    Seq<WirePassReceipt> Passes,
    WireContext Context,
    WireAccess Access,
    SlugRetention Retention,
    WireRecovery Recovery,
    Option<WireCorrespondence> Correspondence,
    Option<RotaryFrame> Rotary) {
    public WireProgram WithBlocks(WireJob job, Seq<WireBlock> blocks) => this with {
        Blocks = blocks,
        Passes = job.Schedule.Passes.Map(pass => WirePassReceipt.From(job, blocks, pass)),
    };

    public SpecializedToolpathEnvelope Specialized => new(
        SpecializedToolpathKind.Wire,
        Blocks.Map(static row => (SpecializedToolpathRow)new SpecializedToolpathRow.Wire(
            row.Pass, row.Station, row.Progress, row.TraversedMm,
            row.Lower.Point, row.Upper.Point, row.Action.Key, row.LagMm,
            row.UpperCornerRadiusMm, row.RotaryDeg)),
        Blocks.Zip(Blocks.Skip(1))
            .Filter(static pair => pair.First.Pass == pair.Second.Pass)
            .Sum(static pair => pair.First.DurationTo(pair.Second)));

    public MotionDirective SpecializedDirective => new MotionDirective.Specialized(
        Blocks.IsEmpty ? -1 : Blocks.Count - 1, Specialized);
    public PostSource PostingSource => new PostSource.Specialized(Specialized);

}

// --- [OPERATIONS] ---------------------------------------------------------------------------------------------------------------------------------
public static class WireEdm {
    public static Fin<TOut> Generate<TOut>(WireDemand? raw, Func<WireProgram, TOut> project) =>
        from _ in Optional(project).ToFin(new GeometryFault.DegenerateInput(Kind.Curve, -1, "wire:projection").ToError())
        from job in WireJob.Admit(raw)
        from program in Dispatch(job)
        from projected in Try.lift(() => project(program)).Run()
            .MapFail(error => new GeometryFault.DegenerateInput(Kind.Curve, -1, $"wire:projection:{error.Message}").ToError())
        select projected;

    private static Fin<WireProgram> Dispatch(WireJob job) => job.Cycle.Switch(
        state: job,
        contour: static (admitted, row) => Boundary(admitted, taperAt: static _ => 0.0, admitted.Guides.LowerZ, row.Corners),
        taperContour: static (admitted, row) => Boundary(admitted, _ => row.TaperDeg, admitted.Guides.LowerZ, row.Corners),
        fourAxis: static (admitted, row) => Paired(admitted, row),
        noCorePocket: static (admitted, row) => Clearing(admitted, row),
        collar: static (admitted, row) => Boundary(admitted, _ => row.TaperDeg, row.LandZ, row.Corners),
        rotary: static (admitted, row) => Rotary(admitted, row),
        variableTaper: static (admitted, row) => Boundary(admitted, row.At, admitted.Guides.LowerZ, row.Corners),
        cutoff: static (admitted, row) => Cutoff(admitted, row.HandoffStation));

    private static Fin<WireProgram> Boundary(
        WireJob job,
        Func<double, double> taperAt,
        double baseZ,
        TaperCornerMode corners) =>
        job.Schedule.Passes.AsIterable().ToSeq().TraverseM(pass =>
            Offset(job.Profile, pass.Offset(job.WireRadiusMm)).Bind(ring => Emit(job, ring, ring, pass,
                station => job.Guides.ShiftAt(job.Guides.UpperZ, baseZ, taperAt(station)),
                WireCorrespondence.Identity, corners))).As()
            .Bind(rows => Program(job, rows));

    private static Fin<WireProgram> Paired(WireJob job, WireCycle.FourAxis cycle) =>
        job.Schedule.Passes.AsIterable().ToSeq().TraverseM(pass =>
            from lower in Offset(job.Profile, pass.Offset(job.WireRadiusMm))
            from upper in Offset(cycle.UpperProfile, pass.Offset(job.WireRadiusMm))
            from blocks in Emit(job, lower, upper, pass, static _ => 0.0, cycle.Correspondence, TaperCornerMode.Conical)
            select blocks).As().Bind(rows => Program(job, rows));

    private static Fin<WireProgram> Clearing(WireJob job, WireCycle.NoCorePocket cycle) =>
        from rough in toSeq(Enumerable.Range(0, cycle.MaxPasses)).TraverseM(level =>
                OffsetMany(job.Profile, -(job.Schedule.Passes[0].Offset(job.WireRadiusMm) + level * cycle.StepOverMm)).Bind(rings =>
                    rings.TraverseM(ring => Emit(job, ring, ring, job.Schedule.Passes[0], static _ => 0.0,
                        WireCorrespondence.Identity, TaperCornerMode.Conical)).As()
                        .Map(static rows => rows.Bind(static row => row)))).As()
            .Map(static levels => levels.Bind(static level => level))
        from finish in job.Schedule.Passes.AsIterable().ToSeq().Tail.TraverseM(pass =>
                Offset(job.Profile, pass.Offset(job.WireRadiusMm)).Bind(ring =>
                    Emit(job, ring, ring, pass, static _ => 0.0, WireCorrespondence.Identity, TaperCornerMode.Conical))).As()
        from program in Program(job, Seq(rough).Concat(finish))
        select program;

    private static Fin<WireProgram> Rotary(WireJob job, WireCycle.Rotary cycle) =>
        Boundary(job, static _ => 0.0, job.Guides.LowerZ, TaperCornerMode.ConstantLand)
            .Map(program => program.WithBlocks(job, RotaryBlocks(program.Blocks, cycle.Frame)));

    private static Seq<WireBlock> RotaryBlocks(Seq<WireBlock> blocks, RotaryFrame frame) =>
        blocks.Fold(
            (Rows: Seq<WireBlock>(), Previous: Option<WireBlock>.None, Distance: 0.0),
            (state, block) => {
                double distance = state.Previous.Match(
                    Some: previous => previous.Pass == block.Pass
                        ? state.Distance + previous.SpanTo(block)
                        : state.Distance,
                    None: static () => 0.0);
                return (
                    state.Rows.Add(block with { RotaryDeg = Some(frame.AngleAt(distance)) }),
                    Some(block),
                    distance);
            })
            .Rows;

    private static Fin<WireProgram> Cutoff(WireJob job, double handoff) =>
        from program in Boundary(job, static _ => 0.0, job.Guides.LowerZ, TaperCornerMode.Reduced)
        let retained = program.Blocks.Filter(block => block.Action is WireAction.Access || block.Station <= handoff)
        select program.WithBlocks(job, retained.Map((block, index) =>
            index == retained.Count - 1 && block.Action is not WireAction.Access
                ? block with { Action = new WireAction.Handoff(), Recovery = Option<WireRecovery>.None }
                : block));

    private static Fin<Seq<WireBlock>> Emit(
        WireJob job,
        Loop lower,
        Loop upper,
        WirePass pass,
        Func<double, double> upperShift,
        WireCorrespondence correspondence,
        TaperCornerMode corners) =>
        from staged in Stations(job, lower, pass)
        let lowerPath = staged.Path
        let marks = staged.Marks
        let upperPath = Native(upper)
        let lowerLength = lowerPath.PathLength()
        let upperLength = upperPath.PathLength()
        let lag = pass.LagMm(job.Schedule.ThicknessMm, job.WireRadiusMm)
        from blocks in marks.TraverseM(mark =>
            from low in Sample(lowerPath, lower, mark.Station * lowerLength)
            from high in Sample(upperPath, upper, correspondence.UpperAt(mark.Station) * upperLength)
            let shift = upperShift(mark.Station)
            let upperPoint = high.Point + corners.Direction(high, upper) * shift
            let lowerGuide = low with { Point = new Point3d(low.Point.X, low.Point.Y, job.Guides.LowerZ) }
            let upperGuide = high with { Point = new Point3d(upperPoint.X, upperPoint.Y, job.Guides.UpperZ) }
            from _ in job.Guides.Envelope(lowerGuide.Point, upperGuide.Point)
            let process = new WireProcess(
                pass.FeedMmPerMin * pass.SpeedScale * mark.SpeedScale,
                pass.Power,
                pass.FlushPressure,
                pass.Tension)
            select new WireBlock(
                pass.Pass,
                mark.Station,
                mark.Progress,
                mark.Progress * lowerLength,
                lowerGuide,
                upperGuide,
                job.Retention.Cutting(mark.Station, pass.Pass, job.Schedule.Passes.Count)
                    ? new WireAction.Cut(process)
                    : new WireAction.Bridge(process.FeedMmPerMin),
                lag * mark.SpeedScale,
                mark.TurnDeg > 0.0 ? corners.CornerRadiusMm(shift) : 0.0,
                Option<double>.None,
                mark.Progress == 0.0 && job.Recovery.Attempts > 0 ? Some(job.Recovery) : Option<WireRecovery>.None)).As()
        select blocks;

    private static Fin<Loop> Offset(Loop loop, double distance) =>
        OffsetMany(loop, distance).Bind(rows => rows.Count == 1
            ? Fin.Succ(rows.Head)
            : Fin.Fail<Loop>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "wire:offset-topology").ToError()));

    private static Fin<Seq<Loop>> OffsetMany(Loop loop, double distance) =>
        ArcAlgebra.Apply(new ArcOp.Offset(new ArcOffsetSource.Path(loop), distance)).Bind(trace => trace.Switch(
            forest: static row => Fin.Succ(row.Result.Loops),
            paths: static row => Fin.Succ(row.Result),
            motion: static _ => Fin.Fail<Seq<Loop>>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "wire:offset-result").ToError()),
            inspection: static _ => Fin.Fail<Seq<Loop>>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "wire:offset-result").ToError()),
            densified: static _ => Fin.Fail<Seq<Loop>>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "wire:offset-result").ToError()),
            recovered: static _ => Fin.Fail<Seq<Loop>>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "wire:offset-result").ToError())));

    private static Polyline<double> Native(Loop loop) =>
        new(loop.Vertices.Map((point, index) => PlineVertex<double>.FromSlice([point.X, point.Y, loop.BulgeAt(index)])), loop.Closed);

    private static Fin<WireGuidePoint> Sample(Polyline<double> path, Loop source, double length) =>
        path.FindPointAtPathLength(length) switch {
            (true, int span, Vector2<double> point, _) => Normal(path, source, span, point).Map(normal => new WireGuidePoint(
                new Point3d(point.X, point.Y, source.Plane), span, source.BulgeAt(span), normal)),
            _ => Fin.Fail<WireGuidePoint>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "wire:station").ToError()),
        };

    private static Fin<Vector3d> Normal(Polyline<double> path, Loop source, int span, Vector2<double> point) {
        int next = source.Closed ? path.NextWrappingIndex(span) : Math.Min(span + 1, source.Count - 1);
        Vector2<double> tangent = PlineSeg.SegTangentVector(path[span], path[next], point);
        Vector3d normal = new(-tangent.Y, tangent.X, 0.0);
        return normal.Unitize()
            ? Fin.Succ(normal)
            : Fin.Fail<Vector3d>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "wire:tangent").ToError());
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
            .Fold(Seq(0.0), static (rows, length) => rows.Add(rows.Last + length))
            .Take(loop.Closed ? loop.Spans : loop.Spans + 1)
            .ToSeq()
            .Map((cumulative, index) => (Station: cumulative / perimeter, TurnDeg: TurnDeg(path, loop, index)));

    // Statement preamble binds the one native projection and perimeter every later clause measures against.
    private static Fin<(Polyline<double> Path, Seq<(double Station, double Progress, double SpeedScale, double TurnDeg)> Marks)> Stations(
        WireJob job,
        Loop loop,
        WirePass pass) {
        Polyline<double> path = Native(loop);
        double perimeter = path.PathLength();
        if (!(perimeter > 0.0))
            return Fin.Fail<(Polyline<double>, Seq<(double, double, double, double)>)>(
                new GeometryFault.DegenerateInput(Kind.Curve, -1, "wire:perimeter").ToError());
        Seq<(double Station, double TurnDeg)> vertices = VertexStations(loop, path, perimeter);
        double start = job.Access.Start.IfNone(0.0);
        double window = pass.CornerSlowMm / perimeter;
        Seq<(double Station, double Progress, double SpeedScale, double TurnDeg)> ordered =
            pass.CornerStations(vertices.Map(static row => row.Station), perimeter, loop.Closed)
                .Concat(job.Retention.Stations)
                .Concat(job.Cycle.Stations)
                .Concat(Seq(0.0, 1.0, start))
                .Filter(static station => double.IsFinite(station) && station >= 0.0 && station <= 1.0)
                .Map(station => loop.Closed ? station - Math.Floor(station) : station)
                .Distinct()
                .Map(station => (
                    Station: station,
                    Progress: station >= start ? station - start : 1.0 - start + station,
                    Turn: vertices
                        .Filter(vertex => Math.Min(
                            Math.Abs(vertex.Station - station),
                            loop.Closed ? 1.0 - Math.Abs(vertex.Station - station) : double.PositiveInfinity) <= window)
                        .Fold(0.0, static (peak, vertex) => Math.Max(peak, vertex.TurnDeg))))
                .Map(row => (row.Station, row.Progress, SpeedScale: pass.CornerScale(row.Turn), TurnDeg: row.Turn))
                .Filter(row => loop.Closed || row.Station >= start)
                .OrderBy(static row => row.Progress)
                .ToSeq();
        return Fin.Succ((path, loop.Closed
            ? ordered.Concat(Seq((Station: start, Progress: 1.0, SpeedScale: ordered.Head.SpeedScale, TurnDeg: ordered.Head.TurnDeg)))
            : ordered));
    }

    private static Fin<WireProgram> Program(WireJob job, Seq<Seq<WireBlock>> rows) =>
        from access in rows.Bind(static pass => pass).HeadOrNone()
            .Traverse(first => Access(job, first))
            .As()
        let cut = rows.Bind(static pass => pass)
        let blocks = access.Map(entry => Seq(entry).Concat(cut)).IfNone(cut)
        select new WireProgram(
            blocks,
            job.Schedule.Passes.Map(pass => WirePassReceipt.From(job, blocks, pass)),
            job.Schedule.Context,
            job.Access,
            job.Retention,
            job.Recovery,
            job.Cycle.Correspondence,
            job.Cycle.Spindle);

    private static Fin<WireBlock> Access(WireJob job, WireBlock first) => job.Access.Switch(
        state: (job, first),
        openEdge: static (state, _) => Fin.Succ(state.job.Access.Entry(state.first, state.first.Lower)),
        startHole: static (state, row) => Fin.Succ(state.job.Access.Entry(
            state.first, state.first.Lower with { Point = row.Point })),
        automatic: static (state, row) => Fin.Succ(state.job.Access.Entry(
            state.first, state.first.Lower with { Point = row.Point })),
        channel: static (state, row) => Sample(Native(row.Path), row.Path, row.Station * row.Path.Length())
            .Map(guide => state.job.Access.Entry(state.first, guide)));
}
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
