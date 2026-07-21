# [RASM_FABRICATION_ADDITIVE_SCANPATH]

`Scan.Plan` owns LPBF vector planning from admitted layers to source-assigned machine events. One `ScanPolicy` controls region classification, parameterized cell generation, exposure, thermal order, multi-source partitioning, plume separation, remelt, delays, recoating, and canonical egress.

Wire posture: HOST-LOCAL. `SliceStack`, `ProcessBudget.Powder`, and optional `SupportPlan` enter once; `Audit.Preflight` gates commit; `ScanPlan` leaves through `ContentKey.Of(EgressKind.ScanVectors)` with complete timing and source evidence.

## [01]-[INDEX]

- [01]-[DOMAIN]: Unit-bearing exposure, source, field, timing, pattern, sort, and policy owners.
- [02]-[REGIONS]: `SliceRegion` classification and support projections become typed `ExposureRegion` rows.
- [03]-[PARTITION]: Relaxed point-site fields assign vectors to calibrated sources and retain overlap adjacency.
- [04]-[SCHEDULING]: Thermal coloring, gas, parity, spatial locality, plume, synchronization, remelt, delay, and recoater laws emit explicit events.
- [05]-[IDENTITY]: One canonical codec covers geometry, `Z`, exposure, source, focus, spot, timing, ordering, and policy identity.

## [02]-[DOMAIN]

- Owner: `ExposureClass` carries power, speed, spacing, focus, spot, contour, and remelt behavior on generated rows.
- Owner: `HatchProgram` closes line, cell, contour, and generated candidate modalities while `CellProgram` generates the point-site space from bounded parameters.
- Owner: `LaserSource` carries field envelope, calibrated power, focus, spot, stitch width, drift, and calibration identity.
- Owner: `ScanEvent` is the executable vocabulary; exposure with dwell, jump, delay, synchronization, and recoating all participate in identity and timing.
- Owner: `ScanOrder` rows carry their own `Project` key law and `Orient` direction rewrite, so `ScanSort.Order` is one sort over one comparable `ScanSortKey` and no caller re-tests order identity.
- Owner: `ScanPlane` derives every spatial quantity once — separation cell, Morton locality, thermal wave, gas bearing — and both ordering and wave election read it.
- Growth: an exposure class is a row, an ordering law is one `ScanOrder` row with its two columns, a source is a value, and a machine event is one union case consumed by the existing folds.

## [03]-[REGIONS]

- Law: one `SliceRegion` per layer is derived once and every zone reads that stack; down-skin and up-skin subtract the neighbour at their own policy depth, and core is the region minus their union.
- Law: support projects its own rows — `SupportLayer.Sparse` at `Density` and `SupportLayer.Interface` at `ContactDuty` — so support exposure carries the planner's realized duty, never a model-derived default.
- Law: an empty zone leaves the row set entirely; a class with no area never reaches candidate generation as a degenerate region.
- Receipt: `ExposureRegion` carries layer, elevation, class, region, and density, so spacing resolves from the row alone.

## [04]-[PARTITION]

- Boundary: `SourcePartition.Build` uses `VoronoiPlane.SetSites`, `Tessellate`, `Relax`, `ClockwisePoints`, `Neighbours`, and `GetNearestSiteTo(..., KDTree)` once per plan because calibrated source fields are invariant across layers.
- Auto: `MemoryOwner<double>` stages the vector-to-source score plane and `SourcePartition.Elect` walks it as one pooled span kernel; `TensorPrimitives.IndexOfMin`, `Average`, `StdDev`, and `SumOfSquares` derive assignment and balance evidence.
- Law: a vector no calibrated field admits leaves `Elect` as source `-1` and converts on the rail; no election throws.
- Law: exclusive vectors stay inside one source field; overlap vectors stitch under one policy and retain both adjacent source identities.
- Law: source scheduling assigns conflict-free thermal waves from whole-segment separation and emits one `ScanEvent.Synchronize` barrier per wave before recoating.

## [05]-[SCHEDULING]

- Entry: `Scan.Plan` runs audit, region projection, candidate generation, clipping, field assignment, ordering, machine-event projection, canonicalization, and receipt construction in one flat query inside the `EngineSpan.ScanpathDerive` span, so a long derivation traces and its histogram measurements carry exemplars.
- Law: exposure dwell, jump, layer delay, source synchronization, remelt, and recoater delay are executable values, never receipt-only estimates.
- Law: gas bearing, layer rotation, thermal separation, source overlap, skywriting, pulse, focus, spot, and distortion compensation are policy values consumed before identity.
- Law: `Scan.Waves` buckets scheduled vectors by separation cell, so wave election probes only the neighbourhood a vector can contend with and searches at most one wave past the blocked set.
- Receipt: `ScanReceipt` retains audit, source loads, thermal moments, plume conflicts, overlap stitches, field cells, event census, energy, path, and build time; the settled receipt fires the `FabricationFact.Engine.Of` exposure, jump, remelt, and stitch rows through the caller-supplied `FabricationTap`, defaulting silent for headless callers.

## [06]-[IDENTITY]

- Owner: `ScanCodec.Write` is the sole canonical octet projection.
- Law: `ScanIdentity` carries the complete output-driving `ScanPolicy`; canonical projection orders profile keys and writes generated behavior by content key.
- Law: every point writes `X`, `Y`, and `Z`; every exposure writes source, class, power, speed, dwell, focus, spot, pulse, and skywriting values.
- Egress: `ContentKey.Of(EgressKind.ScanVectors, bytes)` mints exactly once over the canonical stored bytes.

```csharp signature
extern alias Voronoi;

using System.Buffers.Binary;
using System.Collections.Frozen;
using System.Globalization;
using System.Numerics.Tensors;
using System.Text;
using CommunityToolkit.HighPerformance.Buffers;
using CommunityToolkit.HighPerformance.Helpers;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Domain;
using Rasm.Fabrication.Process;
using Rasm.Geometry;
using Rhino.Geometry;
using Thinktecture;
using UnitsNet;
using UnitsNet.Units;
using Voronoi::SharpVoronoiLib;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Additive;

// --- [GENERATED_OWNERS] ---------------------------------------------------------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class ExposureClass {
    public static readonly ExposureClass Core = new("core", Ratio.FromDecimalFractions(1.00), Ratio.FromDecimalFractions(1.00), Ratio.FromDecimalFractions(1.00), Length.Zero, Ratio.FromDecimalFractions(1.00), contourPasses: 0, remeltPasses: 0);
    public static readonly ExposureClass DownSkin = new("down-skin", Ratio.FromDecimalFractions(0.82), Ratio.FromDecimalFractions(0.76), Ratio.FromDecimalFractions(0.82), Length.FromMillimeters(-0.15), Ratio.FromDecimalFractions(1.08), contourPasses: 1, remeltPasses: 0);
    public static readonly ExposureClass UpSkin = new("up-skin", Ratio.FromDecimalFractions(0.88), Ratio.FromDecimalFractions(0.82), Ratio.FromDecimalFractions(0.86), Length.FromMillimeters(-0.08), Ratio.FromDecimalFractions(1.04), contourPasses: 1, remeltPasses: 1);
    public static readonly ExposureClass Contour = new("contour", Ratio.FromDecimalFractions(0.72), Ratio.FromDecimalFractions(0.62), Ratio.FromDecimalFractions(0.70), Length.FromMillimeters(-0.10), Ratio.FromDecimalFractions(0.78), contourPasses: 2, remeltPasses: 0);
    public static readonly ExposureClass SupportSparse = new("support-sparse", Ratio.FromDecimalFractions(0.65), Ratio.FromDecimalFractions(1.20), Ratio.FromDecimalFractions(1.55), Length.FromMillimeters(0.10), Ratio.FromDecimalFractions(1.12), contourPasses: 0, remeltPasses: 0);
    public static readonly ExposureClass SupportInterface = new("support-interface", Ratio.FromDecimalFractions(0.80), Ratio.FromDecimalFractions(0.92), Ratio.FromDecimalFractions(0.92), Length.FromMillimeters(0.02), Ratio.FromDecimalFractions(1.04), contourPasses: 1, remeltPasses: 0);
    public static readonly ExposureClass Remelt = new("remelt", Ratio.FromDecimalFractions(0.58), Ratio.FromDecimalFractions(0.54), Ratio.FromDecimalFractions(1.00), Length.FromMillimeters(-0.18), Ratio.FromDecimalFractions(0.90), contourPasses: 0, remeltPasses: 1);

    public Ratio PowerScale { get; }
    public Ratio SpeedScale { get; }
    public Ratio SpacingScale { get; }
    public Length FocusOffset { get; }
    public Ratio SpotScale { get; }
    public int ContourPasses { get; }
    public int RemeltPasses { get; }
}

[SmartEnum<string>]
public sealed partial class ScanOrder {
    public static readonly ScanOrder Spatial = new("spatial",
        static (row, plane) => new ScanSortKey(0, 0, 0.0, plane.Locality(row), row.Score),
        static rows => rows);
    public static readonly ScanOrder ThermalColored = new("thermal-colored",
        static (row, plane) => new ScanSortKey(plane.Wave(row), 0, 0.0,
            plane.Wave(row) % 2 == 0 ? plane.Locality(row) : ulong.MaxValue - plane.Locality(row), row.Score),
        static rows => rows);
    public static readonly ScanOrder AgainstGas = new("against-gas",
        static (row, plane) => new ScanSortKey(0, 0, plane.Bearing(row), plane.Locality(row), row.Score),
        static rows => rows);
    public static readonly ScanOrder Alternating = new("alternating",
        static (row, plane) => new ScanSortKey(0, 0, 0.0, plane.Locality(row), row.Score),
        static rows => rows.Map(static (row, index) => index % 2 == 0
            ? row
            : row with { Vector = row.Vector with { Geometry = new Edge3(row.Vector.Geometry.B, row.Vector.Geometry.A) } }));
    public static readonly ScanOrder SourceBalanced = new("source-balanced",
        static (row, plane) => new ScanSortKey(0, row.Source.Id.ToValue(), 0.0, plane.Locality(row), row.Score),
        static rows => rows);

    public Func<SourceAssignment, ScanPlane, ScanSortKey> Project { get; }
    public Func<Seq<SourceAssignment>, Seq<SourceAssignment>> Orient { get; }
}

[ValueObject<int>]
public readonly partial struct LaserId;

[ComplexValueObject]
public sealed partial class LaserSource {
    public LaserId Id { get; }
    public BoundingBox Field { get; }
    public Power MaximumPower { get; }
    public Length SpotDiameter { get; }
    public Length StitchWidth { get; }
    public Length FocusMinimum { get; }
    public Length FocusMaximum { get; }
    public Ratio Drift { get; }
    public ContentKey Calibration { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref LaserId id,
        ref BoundingBox field,
        ref Power maximumPower,
        ref Length spotDiameter,
        ref Length stitchWidth,
        ref Length focusMinimum,
        ref Length focusMaximum,
        ref Ratio drift,
        ref ContentKey calibration) {
        Seq<double> values = Seq(maximumPower.Watts, spotDiameter.Millimeters, stitchWidth.Millimeters,
            focusMinimum.Millimeters, focusMaximum.Millimeters, drift.DecimalFractions);
        if (!field.IsValid || values.Exists(static value => !double.IsFinite(value))
            || maximumPower <= Power.Zero || spotDiameter <= Length.Zero || stitchWidth < Length.Zero
            || focusMinimum > focusMaximum || drift < Ratio.Zero || drift >= Ratio.FromPercent(100))
            validationError = new ValidationError("laser source contains an invalid field or calibration envelope");
    }
}

[ComplexValueObject]
public sealed partial class ExposureProfile {
    public Power Power { get; }
    public Speed Speed { get; }
    public Length Spacing { get; }
    public Duration Dwell { get; }
    public Length Spot { get; }
    public Length Focus { get; }
    public Duration PulseOn { get; }
    public Duration PulseOff { get; }
    public Length SkywritingLead { get; }
    public Length SkywritingLag { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Power power,
        ref Speed speed,
        ref Length spacing,
        ref Duration dwell,
        ref Length spot,
        ref Length focus,
        ref Duration pulseOn,
        ref Duration pulseOff,
        ref Length skywritingLead,
        ref Length skywritingLag) {
        Seq<double> values = Seq(power.Watts, speed.MillimetersPerSecond, spacing.Millimeters, dwell.Seconds,
            spot.Millimeters, focus.Millimeters, pulseOn.Seconds, pulseOff.Seconds,
            skywritingLead.Millimeters, skywritingLag.Millimeters);
        if (values.Exists(static value => !double.IsFinite(value))
            || power <= Power.Zero || speed <= Speed.Zero || spacing <= Length.Zero || dwell < Duration.Zero
            || spot <= Length.Zero || pulseOn < Duration.Zero || pulseOff < Duration.Zero
            || skywritingLead < Length.Zero || skywritingLag < Length.Zero)
            validationError = new ValidationError("exposure profile contains an invalid physical value");
    }
}

public sealed record CellProgram(Length Pitch, int Relaxations, Ratio RelaxationStrength, int MaximumSites, int Seed, Area MergeBelowArea);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record HatchProgram {
    private HatchProgram() { }
    public sealed record Lines(Angle Bearing) : HatchProgram;
    public sealed record Cells(CellProgram Cells, Angle Bearing) : HatchProgram;
    public sealed record Contours(int Passes, Length Offset) : HatchProgram;
    public sealed record Generated(ContentKey Identity, Func<FillContext, Fin<Seq<Edge3>>> Candidates) : HatchProgram;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DistortionCompensation {
    private DistortionCompensation() { }
    public sealed record None : DistortionCompensation;
    public sealed record Affine(Transform BuildToCommand, ContentKey Calibration) : DistortionCompensation;
    public sealed record Generated(Func<Point3d, Fin<Point3d>> Correct, ContentKey Calibration) : DistortionCompensation;

    public Fin<Point3d> Apply(Point3d point) => Switch(
        state: point,
        none: static (value, _) => Fin.Succ(value),
        affine: static (value, compensation) => Fin.Succ(compensation.BuildToCommand * value),
        generated: static (value, compensation) => Scan.Capture(
            () => compensation.Correct(value),
            "scan:compensation-callback"));

    public string Identity => Switch(
        none: static _ => "none",
        affine: static value => value.Calibration.Digest.ToString("x32", CultureInfo.InvariantCulture),
        generated: static value => value.Calibration.Digest.ToString("x32", CultureInfo.InvariantCulture));
}

public sealed record SourcePolicy(
    Arr<LaserSource> Sources,
    Ratio BalanceWeight,
    Length PlumeClearance,
    Length Overlap,
    int FieldRelaxations,
    Ratio FieldRelaxationStrength,
    Vector3d GasBearing);

public sealed record TimingPolicy(
    Speed JumpSpeed,
    Duration LayerDelay,
    Duration RecoatDelay,
    Duration SourceDelay,
    Speed RecoatSpeed,
    Length RecoatTravel);

public sealed record RotationPolicy(Angle LayerIncrement, int Cycle, Angle ContourOffset);

public sealed record ScanPolicy(
    AuditPolicy Audit,
    ExposureProfile Base,
    FrozenDictionary<ExposureClass, ExposureProfile> Profiles,
    HatchProgram Hatch,
    ScanOrder Order,
    SourcePolicy Sources,
    TimingPolicy Timing,
    RotationPolicy Rotation,
    DistortionCompensation Compensation,
    OffsetPolicy Offset,
    Length ThermalSeparation,
    Length IntersectionTolerance,
    int ThermalWindow,
    int MaximumVectors,
    int DownSkinLayers,
    int UpSkinLayers);

// --- [DOMAIN_MODEL] --------------------------------------------------------------------------------------------------------------------------------
public sealed record ExposureRegion(int Layer, Length Elevation, ExposureClass Class, SliceRegion Region, Ratio Density);
public sealed record CandidateVector(int Layer, Length Elevation, ExposureClass Class, Edge3 Geometry);
public sealed record FieldCell(LaserId Source, Seq<Point2d> Boundary, Seq<LaserId> Neighbours, Point2d Centroid, Length Perimeter, bool Closed);
public sealed record SourceAssignment(CandidateVector Vector, LaserSource Source, Seq<LaserSource> StitchPeers, double Score);

public readonly record struct ScanSortKey(int ThermalClass, int Source, double Bearing, ulong Locality, double Score)
    : IComparable<ScanSortKey> {
    public int CompareTo(ScanSortKey other) =>
        ThermalClass != other.ThermalClass ? ThermalClass.CompareTo(other.ThermalClass)
        : Source != other.Source ? Source.CompareTo(other.Source)
        : Bearing != other.Bearing ? Bearing.CompareTo(other.Bearing)
        : Locality != other.Locality ? Locality.CompareTo(other.Locality)
        : Score.CompareTo(other.Score);
}

public sealed record ScanPlane(Vector3d Gas, Length Separation, Length IntersectionTolerance, int ThermalWindow) {
    public ulong Locality(SourceAssignment row) {
        (long x, long y) = Cell(row);
        return Morton(ZigZag(x), ZigZag(y));
    }

    public int Wave(SourceAssignment row) {
        (long x, long y) = Cell(row);
        return (int)(ZigZag(unchecked(x + (2 * y))) % (uint)ThermalWindow);
    }

    public double Bearing(SourceAssignment row) => Vector3d.Multiply(row.Vector.Geometry.B - row.Vector.Geometry.A, Gas);

    private (long X, long Y) Cell(SourceAssignment row) {
        Point3d midpoint = 0.5 * (row.Vector.Geometry.A + row.Vector.Geometry.B);
        double pitch = Math.Max(
            Math.Max(Separation.Millimeters, row.Vector.Geometry.A.DistanceTo(row.Vector.Geometry.B)),
            double.Epsilon);
        return ((long)Math.Floor(midpoint.X / pitch), (long)Math.Floor(midpoint.Y / pitch));
    }

    private static uint ZigZag(long value) => unchecked((uint)((value << 1) ^ (value >> 63)));

    private static ulong Morton(uint x, uint y) => Spread(x) | (Spread(y) << 1);

    private static ulong Spread(uint value) {
        ulong bits = value;
        bits = (bits | (bits << 16)) & 0x0000FFFF0000FFFF;
        bits = (bits | (bits << 8)) & 0x00FF00FF00FF00FF;
        bits = (bits | (bits << 4)) & 0x0F0F0F0F0F0F0F0F;
        bits = (bits | (bits << 2)) & 0x3333333333333333;
        return (bits | (bits << 1)) & 0x5555555555555555;
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ScanEvent {
    private ScanEvent() { }
    public sealed record Expose(
        LaserId Source,
        ExposureClass Class,
        Point3d From,
        Point3d To,
        Power Power,
        Speed Speed,
        Duration Dwell,
        Length Focus,
        Length Spot,
        Duration PulseOn,
        Duration PulseOff,
        Length SkywritingLead,
        Length SkywritingLag,
        Seq<LaserId> StitchPeers,
        int Wave,
        int Pass) : ScanEvent;
    public sealed record Jump(LaserId Source, Point3d From, Point3d To, Speed Speed, int Wave) : ScanEvent;
    public sealed record Synchronize(Seq<LaserId> Sources, int Wave, Duration Duration, string Reason) : ScanEvent;
    public sealed record Recoat(int Layer, Length Travel, Speed Speed, Duration Delay) : ScanEvent;
    public sealed record LayerDelay(int Layer, Duration Duration) : ScanEvent;
}

public sealed record SourceLane(LaserId Source, Seq<ScanEvent> Events);
public sealed record ScanLayer(int Layer, Length Elevation, Seq<SourceLane> Sources, Seq<ScanEvent> Events);

public sealed record ScanIdentity(ScanPolicy Policy);

public sealed record SourceLoad(LaserId Source, int Vectors, Length Path, Duration Exposure, Energy Energy);
public sealed record ThermalEvidence(double AverageSeparation, double StandardDeviation, double SumOfSquares, int PlumeConflicts);
public sealed record ScanReceipt(
    AuditReceipt Audit,
    Seq<SourceLoad> Sources,
    Seq<FieldCell> Fields,
    ThermalEvidence Thermal,
    int Exposures,
    int Jumps,
    int Remelts,
    int Stitches,
    Length Path,
    Energy Energy,
    Duration BuildTime,
    int CanonicalBytes);

public sealed record ScanPlan(Seq<ScanLayer> Layers, ReadOnlyMemory<byte> Bytes, ContentKey Key, ScanReceipt Receipt);

// --- [OPERATIONS] ----------------------------------------------------------------------------------------------------------------------------------
public static class Scan {
    public static Fin<ScanPlan> Plan(
        SliceStack stack,
        ScanPolicy policy,
        ProcessBudget.Powder budget,
        Option<SupportPlan> support,
        FabricationTap? tap = null) =>
        EngineSpan.ScanpathDerive.Traced(_ =>
        from _policy in (
            Gate(policy.Rotation.Cycle > 0
                && double.IsFinite(policy.Rotation.LayerIncrement.Radians)
                && double.IsFinite(policy.Rotation.ContourOffset.Radians)
                && policy.DownSkinLayers > 0
                && policy.UpSkinLayers > 0,
                "scan:layer-policy"),
            Gate(policy.ThermalWindow > 0
                && policy.MaximumVectors > 0
                && policy.MaximumVectors < int.MaxValue
                && policy.ThermalSeparation >= Length.Zero
                && policy.IntersectionTolerance > Length.Zero
                && double.IsFinite(policy.IntersectionTolerance.Millimeters),
                "scan:thermal-policy"),
            Gate(policy.Sources.FieldRelaxations >= 0
                && policy.Sources.FieldRelaxationStrength >= Ratio.Zero
                && policy.Sources.Sources.Length > 0
                && policy.Sources.Sources.Map(static source => source.Id).Distinct().Count == policy.Sources.Sources.Length
                && policy.Sources.GasBearing.IsValid
                && !policy.Sources.GasBearing.IsZero
                && policy.Sources.BalanceWeight >= Ratio.Zero
                && policy.Sources.PlumeClearance >= Length.Zero
                && policy.Sources.Overlap >= Length.Zero,
                "scan:source-policy"),
            Gate(policy.Timing.JumpSpeed > Speed.Zero
                && policy.Timing.RecoatSpeed > Speed.Zero
                && policy.Timing.RecoatTravel > Length.Zero
                && policy.Timing.LayerDelay >= Duration.Zero
                && policy.Timing.RecoatDelay >= Duration.Zero
                && policy.Timing.SourceDelay >= Duration.Zero,
                "scan:timing-policy"),
            Gate(HatchValid(policy.Hatch), "scan:hatch-policy"),
            Gate(CompensationValid(policy.Compensation), "scan:compensation-policy"))
            .Apply(static (_, _, _, _, _, _) => unit)
            .As()
            .ToFin()
        from audit in Audit.Preflight(stack, policy.Audit)
        from _clean in audit.Clean
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new GeometryFault.DegenerateInput(Kind.Mesh, -1, $"scan:audit:{audit.Defects.Count}").ToError())
        from physics in Physics(budget)
        from _physics in physics.Power == policy.Base.Power
                && physics.Speed == policy.Base.Speed
                && physics.Spacing == policy.Base.Spacing
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new GeometryFault.DegenerateInput(Kind.Mesh, -1, "scan:physics-policy").ToError())
        from regions in Regions(stack, support, policy)
        from _regions in regions.ForAll(static region => region.Density > Ratio.Zero
                && double.IsFinite(region.Density.DecimalFractions))
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new GeometryFault.DegenerateInput(Kind.Mesh, -1, "scan:region-density").ToError())
        from fields in SourcePartition.Build(stack, policy.Sources)
        from vectors in Candidates(regions, policy)
        from assigned in SourcePartition.Assign(vectors, fields, policy.Sources, policy.MaximumVectors)
        from ordered in Schedule(assigned, policy)
        from layers in Events(ordered, policy)
        let identity = Identity(policy)
        let bytes = ScanCodec.Write(identity, layers)
        let key = ContentKey.Of(EgressKind.ScanVectors, bytes)
        let receipt = Receipt(audit, fields, ordered, layers, policy, bytes.Length)
        from _plume in receipt.Thermal.PlumeConflicts == 0
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new GeometryFault.DegenerateInput(Kind.Mesh, -1, $"scan:plume-conflicts:{receipt.Thermal.PlumeConflicts}").ToError())
        let _fact = FabricationFact.Engine.Of(receipt).Map((tap ?? FabricationTap.Silent).Fire).Strict()
        select new ScanPlan(layers, bytes, key, receipt));

    internal static Fin<T> Capture<T>(Func<Fin<T>> callback, string locus) =>
        Try.lift(callback).Run()
            .MapFail(error => new GeometryFault.DegenerateInput(Kind.Mesh, -1, $"{locus}:{error.Message}").ToError())
            .Bind(identity);

    private static bool HatchValid(HatchProgram program) => program.Switch(
        lines: static value => double.IsFinite(value.Bearing.Radians),
        cells: static value => double.IsFinite(value.Bearing.Radians)
            && value.Cells.Pitch > Length.Zero
            && value.Cells.Relaxations >= 0
            && value.Cells.RelaxationStrength >= Ratio.Zero
            && value.Cells.MaximumSites > 0
            && value.Cells.MergeBelowArea >= Area.Zero,
        contours: static value => value.Passes > 0 && value.Offset > Length.Zero,
        generated: static value => value.Identity != default && value.Candidates is not null);

    private static bool CompensationValid(DistortionCompensation compensation) => compensation.Switch(
        none: static _ => true,
        affine: static value => TransformValid(value.BuildToCommand) && value.Calibration != default,
        generated: static value => value.Calibration != default && value.Correct is not null);

    private static bool TransformValid(Transform value) => Seq(
        value.M00, value.M01, value.M02, value.M03,
        value.M10, value.M11, value.M12, value.M13,
        value.M20, value.M21, value.M22, value.M23,
        value.M30, value.M31, value.M32, value.M33).ForAll(double.IsFinite);

    private static Fin<ExposureProfile> Physics(ProcessBudget.Powder budget) =>
        ExposureProfile.Validate(
            new Power(budget.LaserPower, PowerUnit.Watt),
            new Speed(budget.ScanSpeed, SpeedUnit.MillimeterPerSecond),
            Length.FromMillimeters(budget.HatchSpacing),
            Duration.Zero,
            Length.FromMillimeters(budget.HatchSpacing / 2.0),
            Length.Zero,
            Duration.Zero,
            Duration.Zero,
            Length.Zero,
            Length.Zero,
            out ExposureProfile? admitted) is { } error || admitted is null
                ? Fin.Fail<ExposureProfile>(new GeometryFault.DegenerateInput(Kind.Mesh, -1, $"scan:physics:{error?.ToString() ?? "unadmitted"}").ToError())
                : Fin.Succ(admitted);

    private static Fin<Seq<ExposureRegion>> Regions(SliceStack stack, Option<SupportPlan> support, ScanPolicy policy) =>
        toSeq(Enumerable.Range(0, stack.LayerCount))
            .Traverse(layer => SliceRegion.Of(stack, layer))
            .As()
            .Bind(slices => Zoned(stack, slices, support, policy));

    private static Fin<Seq<ExposureRegion>> Zoned(
        SliceStack stack,
        Seq<SliceRegion> slices,
        Option<SupportPlan> support,
        ScanPolicy policy) =>
        toSeq(Enumerable.Range(0, stack.LayerCount)).Traverse(layer => {
            SliceRegion region = slices[layer];
            Length elevation = Length.FromMillimeters(stack.Elevations[layer]);
            Seq<SupportLayer> supports = support.Map(plan => plan.PlanarRows.Filter(row => row.Layer == layer)).IfNone(Seq<SupportLayer>());
            return from down in Skin(region, At(slices, layer - policy.DownSkinLayers))
                   from up in Skin(region, At(slices, layer + policy.UpSkinLayers))
                   from skin in down.Union(up)
                   from core in region.Difference(skin)
                   select Seq(
                       new ExposureRegion(layer, elevation, ExposureClass.Core, core, Ratio.FromPercent(100)),
                       new ExposureRegion(layer, elevation, ExposureClass.Contour, region, Ratio.FromPercent(100)),
                       new ExposureRegion(layer, elevation, ExposureClass.DownSkin, down, Ratio.FromPercent(100)),
                       new ExposureRegion(layer, elevation, ExposureClass.UpSkin, up, Ratio.FromPercent(100)))
                       .Concat(supports.Bind(row => Seq(
                           new ExposureRegion(layer, elevation, ExposureClass.SupportSparse, row.Sparse, row.Density),
                           new ExposureRegion(layer, elevation, ExposureClass.SupportInterface, row.Interface, row.ContactDuty))));
        }).As()
            .Map(static rows => rows.Bind(static row => row).Filter(static row => !row.Region.IsEmpty));

    private static Fin<SliceRegion> Skin(SliceRegion current, Option<SliceRegion> neighbour) =>
        neighbour.Map(current.Difference).IfNone(Fin.Succ(current));

    private static Option<SliceRegion> At(Seq<SliceRegion> slices, int layer) =>
        layer >= 0 && layer < slices.Count ? Some(slices[layer]) : None;

    private static Fin<Seq<CandidateVector>> Candidates(Seq<ExposureRegion> regions, ScanPolicy policy) =>
        regions.Fold(
            Fin.Succ(Seq<CandidateVector>()),
            (state, region) => state.Bind(held => Candidates(region, policy, policy.MaximumVectors - held.Count)
                .Map(held.Concat)));

    private static Fin<Seq<CandidateVector>> Candidates(ExposureRegion region, ScanPolicy policy, int remaining) {
        ExposureProfile profile = policy.Profiles.TryGetValue(region.Class, out ExposureProfile? specialized) ? specialized : policy.Base;
        Angle rotation = Angle.FromDegrees((region.Layer % policy.Rotation.Cycle) * policy.Rotation.LayerIncrement.Degrees);
        HatchProgram program = region.Class == ExposureClass.Contour
            ? new HatchProgram.Contours(
                region.Class.ContourPasses,
                profile.Spacing * region.Class.SpacingScale.DecimalFractions)
            : policy.Hatch;
        Length spacing = profile.Spacing * region.Class.SpacingScale.DecimalFractions / region.Density.DecimalFractions;
        return spacing <= Length.Zero || !double.IsFinite(spacing.Millimeters)
            ? Fin.Fail<Seq<CandidateVector>>(new GeometryFault.DegenerateInput(Kind.Mesh, -1, "scan:hatch-spacing").ToError())
            : program.Switch(
            state: (region, profile, rotation, spacing),
            lines: (state, lines) => state.region.Region.Rays(ScanGeometry.Parallel(state.region.Region.Bound(), lines.Bearing + state.rotation, spacing)),
            cells: (state, cells) => CellHatch(state.region, cells.Cells, cells.Bearing + state.rotation, spacing),
            contours: (state, contours) => ContourHatch(state.region, contours.Passes, contours.Offset, policy.Offset, policy.Rotation.ContourOffset + state.rotation),
            generated: static (state, generated) => Capture(
                () => generated.Candidates(new FillContext(
                        state.region.Region,
                        state.region.Elevation,
                        state.region.Region.Bound(),
                        state.region.Layer,
                        _ => state.spacing,
                        state.rotation)),
                    "scan:hatch-callback")
                .Bind(state.region.Region.Rays))
            .Bind(edges => Bounded(edges, remaining))
            .Map(edges => edges.Map(edge => new CandidateVector(region.Layer, region.Elevation, region.Class, edge)));
    }

    private static Fin<Seq<Edge3>> Bounded(Seq<Edge3> edges, int maximum) {
        Seq<Edge3> bounded = edges.Take(maximum + 1).Strict();
        return bounded.Count <= maximum
            ? Fin.Succ(bounded)
            : Fin.Fail<Seq<Edge3>>(new GeometryFault.DegenerateInput(Kind.Mesh, -1, $"scan:vector-cap:{bounded.Count}").ToError());
    }

    private static Fin<Seq<Edge3>> CellHatch(ExposureRegion region, CellProgram cells, Angle bearing, Length spacing) =>
        CellDiagram.Build(region.Region.Bound(), cells).Bind(diagram => region.Region.Rays(
            diagram.Bind(cell => ScanGeometry.Parallel(cell, bearing, spacing))));

    private static Fin<Seq<Edge3>> ContourHatch(ExposureRegion region, int passes, Length offset, OffsetPolicy policy, Angle phase) =>
        passes <= 0 || offset <= Length.Zero
            ? Fin.Fail<Seq<Edge3>>(new GeometryFault.DegenerateInput(Kind.Mesh, -1, "scan:contour-program").ToError())
            : toSeq(Enumerable.Range(0, passes))
            .Traverse(pass => region.Region.Grow(offset * -(pass + 1), policy))
            .As()
            .Map(rows => {
                Point3d center = region.Region.Bound().Center;
                return rows.Bind(static row => row.Outers.Bind(loop =>
                    toSeq(Enumerable.Range(0, loop.Count)).Map(index => new Edge3(loop.At(index), loop.At(index + 1)))))
                    .OrderBy(edge => Wrapped(Math.Atan2(edge.A.Y - center.Y, edge.A.X - center.X) - phase.Radians))
                    .ToSeq();
            });

    private static double Wrapped(double radians) => radians - (2.0 * Math.PI * Math.Floor(radians / (2.0 * Math.PI)));

    private static ScanPlane Plane(ScanPolicy policy) => new(
        policy.Sources.GasBearing,
        UnitMath.Max(policy.ThermalSeparation, policy.Sources.PlumeClearance),
        policy.IntersectionTolerance,
        policy.ThermalWindow);

    private static Fin<Seq<SourceAssignment>> Schedule(Seq<SourceAssignment> assigned, ScanPolicy policy) =>
        assigned.GroupBy(static row => row.Vector.Layer)
            .Map(group => ScanSort.Order(toSeq(group), policy.Order, Plane(policy)))
            .Sequence()
            .Map(static rows => rows.Bind(static row => row));

    private static Fin<Seq<ScanLayer>> Events(Seq<SourceAssignment> ordered, ScanPolicy policy) =>
        ordered.GroupBy(static row => row.Vector.Layer).Map(group => {
            Seq<SourceAssignment> rows = toSeq(group);
            Seq<(SourceAssignment Row, int Wave)> scheduled = Waves(rows, Plane(policy));
            Fin<Seq<SourceLane>> lanes = scheduled.GroupBy(static row => row.Row.Source.Id).Map(source => {
                Seq<(SourceAssignment Row, int Wave)> sourceRows = toSeq(source)
                    .OrderBy(static row => row.Item2)
                    .ToSeq();
                Seq<ScanEvent> exposure = sourceRows.Map((scheduled, index) => Exposure(
                        scheduled.Row,
                        policy,
                        index > 0 ? Some(sourceRows[index - 1].Row) : None,
                        scheduled.Wave))
                    .Bind(static row => row);
                return exposure.Traverse(EventCompensated(policy.Compensation)).As()
                    .Map(events => new SourceLane(source.Key, events));
            }).Sequence();
            Seq<ScanEvent> global = scheduled.Map(static row => row.Wave).Distinct().OrderBy(static wave => wave).ToSeq().Map(wave =>
                    (ScanEvent)new ScanEvent.Synchronize(
                        policy.Sources.Sources.Map(static source => source.Id).ToSeq(),
                        wave,
                        policy.Timing.SourceDelay,
                        "wave-barrier"))
                .Concat(Seq<ScanEvent>(new ScanEvent.Recoat(
                    group.Key,
                    policy.Timing.RecoatTravel,
                    policy.Timing.RecoatSpeed,
                    policy.Timing.RecoatDelay)))
                .Concat(Seq<ScanEvent>(new ScanEvent.LayerDelay(group.Key, policy.Timing.LayerDelay)));
            return from sources in lanes
                   from events in global.Traverse(EventCompensated(policy.Compensation)).As()
                   select new ScanLayer(
                       group.Key,
                       rows.Head.Map(static row => row.Vector.Elevation).IfNone(Length.Zero),
                       sources,
                       events);
        }).Sequence();

    // Separation buckets bound the conflict probe: only peers sharing or neighbouring a covered cell can fall
    // inside the separation, so wave election stays linear in the vectors it actually contends with.
    private static Seq<(SourceAssignment Row, int Wave)> Waves(Seq<SourceAssignment> rows, ScanPlane plane) =>
        rows.Fold(
            (Scheduled: Seq<(SourceAssignment Row, int Wave)>(), Index: HashMap<(long X, long Y), Seq<(SourceAssignment Row, int Wave)>>()),
            (state, row) => {
                Seq<(long X, long Y)> cells = Cells(row.Vector.Geometry, plane.Separation);
                Set<int> blocked = cells
                    .Bind(cell => Neighbourhood(cell).Bind(probe => state.Index.Find(probe).IfNone(Seq<(SourceAssignment Row, int Wave)>())))
                    .Filter(peer => peer.Row.Source.Id != row.Source.Id
                        && SegmentGap(peer.Row.Vector.Geometry, row.Vector.Geometry, plane.IntersectionTolerance) < plane.Separation.Millimeters)
                    .Map(static peer => peer.Wave)
                    .ToSet();
                int seed = plane.Wave(row);
                (SourceAssignment Row, int Wave) placed = (row, toSeq(Enumerable.Range(0, blocked.Count + 1))
                    .Map(offset => seed + (offset * plane.ThermalWindow))
                    .Find(candidate => !blocked.Contains(candidate))
                    .IfNone(seed + ((blocked.Count + 1) * plane.ThermalWindow)));
                return (
                    state.Scheduled.Add(placed),
                    cells.Fold(state.Index, (index, cell) => index.AddOrUpdate(
                        cell,
                        index.Find(cell).IfNone(Seq<(SourceAssignment Row, int Wave)>()).Add(placed))));
            })
        .Scheduled;

    private static Seq<(long X, long Y)> Cells(Edge3 segment, Length separation) {
        double pitch = Math.Max(separation.Millimeters, double.Epsilon);
        long minimumX = (long)Math.Floor(Math.Min(segment.A.X, segment.B.X) / pitch);
        long maximumX = (long)Math.Floor(Math.Max(segment.A.X, segment.B.X) / pitch);
        long minimumY = (long)Math.Floor(Math.Min(segment.A.Y, segment.B.Y) / pitch);
        long maximumY = (long)Math.Floor(Math.Max(segment.A.Y, segment.B.Y) / pitch);
        return toSeq(
            from x in LongRange(minimumX, maximumX)
            from y in LongRange(minimumY, maximumY)
            select (x, y));
    }

    private static Seq<(long X, long Y)> Neighbourhood((long X, long Y) cell) => toSeq(
        from x in Enumerable.Range(-1, 3)
        from y in Enumerable.Range(-1, 3)
        select (cell.X + x, cell.Y + y));

    private static IEnumerable<long> LongRange(long from, long to) =>
        Enumerable.Range(0, checked((int)(to - from + 1))).Select(offset => from + offset);

    private static double SegmentGap(Edge3 left, Edge3 right, Length tolerance) =>
        Intersects(left, right, tolerance)
            ? 0.0
            : Seq(
                PointGap(left.A, right),
                PointGap(left.B, right),
                PointGap(right.A, left),
                PointGap(right.B, left)).Min();

    private static bool Intersects(Edge3 left, Edge3 right, Length tolerance) {
        double leftA = Cross(left.A, left.B, right.A);
        double leftB = Cross(left.A, left.B, right.B);
        double rightA = Cross(right.A, right.B, left.A);
        double rightB = Cross(right.A, right.B, left.B);
        double linear = tolerance.Millimeters;
        double scale = Math.Max(1.0, Math.Max(left.A.DistanceTo(left.B), right.A.DistanceTo(right.B)));
        double area = linear * scale;
        int leftASign = Sign(leftA, area);
        int leftBSign = Sign(leftB, area);
        int rightASign = Sign(rightA, area);
        int rightBSign = Sign(rightB, area);
        bool rightAOnLeft = leftASign == 0 && OnSegment(left, right.A, linear);
        bool rightBOnLeft = leftBSign == 0 && OnSegment(left, right.B, linear);
        bool leftAOnRight = rightASign == 0 && OnSegment(right, left.A, linear);
        bool leftBOnRight = rightBSign == 0 && OnSegment(right, left.B, linear);
        return rightAOnLeft || rightBOnLeft || leftAOnRight || leftBOnRight
            || (leftASign != leftBSign && rightASign != rightBSign);
    }

    private static int Sign(double value, double tolerance) => value > tolerance ? 1 : value < -tolerance ? -1 : 0;

    private static bool OnSegment(Edge3 segment, Point3d point, double tolerance) =>
        point.X >= Math.Min(segment.A.X, segment.B.X) - tolerance
        && point.X <= Math.Max(segment.A.X, segment.B.X) + tolerance
        && point.Y >= Math.Min(segment.A.Y, segment.B.Y) - tolerance
        && point.Y <= Math.Max(segment.A.Y, segment.B.Y) + tolerance;

    private static double Cross(Point3d origin, Point3d along, Point3d point) =>
        (along.X - origin.X) * (point.Y - origin.Y) - (along.Y - origin.Y) * (point.X - origin.X);

    private static double PointGap(Point3d point, Edge3 segment) =>
        point.DistanceTo(new Line(segment.A, segment.B).ClosestPoint(point, limitToFiniteSegment: true));

    private static Seq<ScanEvent> Exposure(SourceAssignment assignment, ScanPolicy policy, Option<SourceAssignment> prior, int wave) {
        ExposureProfile profile = policy.Profiles.TryGetValue(assignment.Vector.Class, out ExposureProfile? specialized)
            ? specialized
            : policy.Base;
        Seq<ScanEvent> jump = Seq<ScanEvent>(new ScanEvent.Jump(
            assignment.Source.Id,
            prior.Map(static previous => previous.Vector.Geometry.B).IfNone(assignment.Source.Field.Center),
            assignment.Vector.Geometry.A,
            policy.Timing.JumpSpeed,
            wave));
        Seq<ScanEvent> passes = toSeq(Enumerable.Range(0, Math.Max(1, assignment.Vector.Class.RemeltPasses + 1)))
            .Map(pass => {
                ExposureClass active = pass == 0 ? assignment.Vector.Class : ExposureClass.Remelt;
                ExposureProfile activeProfile = policy.Profiles.TryGetValue(active, out ExposureProfile? specialized)
                    ? specialized
                    : profile;
                double power = Math.Min(
                    activeProfile.Power.Watts * active.PowerScale.DecimalFractions * (1.0 - assignment.Source.Drift.DecimalFractions),
                    assignment.Source.MaximumPower.Watts);
                double focus = Math.Clamp(
                    (activeProfile.Focus + active.FocusOffset).Millimeters,
                    assignment.Source.FocusMinimum.Millimeters,
                    assignment.Source.FocusMaximum.Millimeters);
                double spot = Math.Max(
                    activeProfile.Spot.Millimeters * active.SpotScale.DecimalFractions,
                    assignment.Source.SpotDiameter.Millimeters);
                return (ScanEvent)new ScanEvent.Expose(
                    assignment.Source.Id,
                    active,
                    assignment.Vector.Geometry.A,
                    assignment.Vector.Geometry.B,
                    Power.FromWatts(power),
                    activeProfile.Speed * active.SpeedScale.DecimalFractions,
                    activeProfile.Dwell,
                    Length.FromMillimeters(focus),
                    Length.FromMillimeters(spot),
                    activeProfile.PulseOn,
                    activeProfile.PulseOff,
                    activeProfile.SkywritingLead,
                    activeProfile.SkywritingLag,
                    assignment.StitchPeers.Map(static source => source.Id),
                    wave,
                    pass);
            });
        return jump.Concat(passes);
    }

    private static Func<ScanEvent, Fin<ScanEvent>> EventCompensated(DistortionCompensation compensation) => scanEvent =>
        scanEvent.Switch(
            expose: expose => from start in compensation.Apply(expose.From)
                               from end in compensation.Apply(expose.To)
                               select (ScanEvent)(expose with { From = start, To = end }),
            jump: jump => from start in compensation.Apply(jump.From)
                          from end in compensation.Apply(jump.To)
                          select (ScanEvent)(jump with { From = start, To = end }),
            synchronize: static value => Fin.Succ<ScanEvent>(value),
            recoat: static value => Fin.Succ<ScanEvent>(value),
            layerDelay: static value => Fin.Succ<ScanEvent>(value));

    private static ScanIdentity Identity(ScanPolicy policy) => new(policy);

    private static ScanReceipt Receipt(
        AuditReceipt audit,
        Seq<FieldCell> fields,
        Seq<SourceAssignment> ordered,
        Seq<ScanLayer> layers,
        ScanPolicy policy,
        int bytes) {
        Seq<ScanEvent> events = layers.Bind(static layer => layer.Sources.Bind(static source => source.Events).Concat(layer.Events));
        Seq<ScanEvent.Expose> exposure = events.Choose(static value => value is ScanEvent.Expose row ? Some(row) : None);
        Seq<double> distances = ordered.GroupBy(static row => row.Vector.Layer).Bind(group => {
            Seq<SourceAssignment> rows = toSeq(group);
            return rows.Skip(1).Map((row, index) => rows[index].Vector.Geometry.B.DistanceTo(row.Vector.Geometry.A));
        });
        double[] samples = distances.ToArray();
        Seq<(int Layer, int Wave, ScanEvent.Expose Event)> waved = layers.Bind(layer => layer.Sources.Bind(source => source.Events
            .Choose(value => value is ScanEvent.Expose expose ? Some((Layer: layer.Layer, Wave: expose.Wave, Event: expose)) : None)));
        int plumeConflicts = waved.GroupBy(static row => (row.Layer, row.Wave)).Map(group => {
            Seq<(int Layer, int Wave, ScanEvent.Expose Event)> wave = toSeq(group);
            return wave.Map((row, index) => wave.Skip(index + 1).Count(peer =>
                row.Event.Source != peer.Event.Source
                && SegmentGap(new Edge3(row.Event.From, row.Event.To), new Edge3(peer.Event.From, peer.Event.To), policy.IntersectionTolerance)
                    < policy.Sources.PlumeClearance.Millimeters)).Sum();
        }).Sum();
        ThermalEvidence thermal = samples.Length == 0
            ? new ThermalEvidence(0.0, 0.0, 0.0, plumeConflicts)
            : new ThermalEvidence(
                TensorPrimitives.Average(samples),
                TensorPrimitives.StdDev(samples),
                TensorPrimitives.SumOfSquares(samples),
                plumeConflicts);
        Seq<SourceLoad> loads = exposure.GroupBy(static row => row.Source).Map(group => {
            Seq<ScanEvent.Expose> rows = toSeq(group);
            Length path = Length.FromMillimeters(rows.Map(static row =>
                row.From.DistanceTo(row.To) + row.SkywritingLead.Millimeters + row.SkywritingLag.Millimeters).Sum());
            Duration duration = rows.Map(static row => DurationOf(row)).Fold(Duration.Zero, static (sum, value) => sum + value);
            Energy energy = rows.Map(static row => row.Power
                    * (Duration.FromSeconds(row.From.DistanceTo(row.To) / row.Speed.MillimetersPerSecond) + row.Dwell)
                    * Duty(row))
                .Fold(Energy.Zero, static (sum, value) => sum + value);
            return new SourceLoad(group.Key, rows.Count, path, duration, energy);
        });
        Length path = events.Fold(Length.Zero, static (sum, value) => sum + LengthOf(value));
        Duration buildTime = layers.Map(layer => {
            Duration sourceTime = layer.Sources.Bind(source => source.Events.Choose(value => WaveOf(value)
                    .Map(wave => (source.Source, Wave: wave, Event: value))))
                .GroupBy(static row => row.Wave)
                .Map(wave => toSeq(wave).GroupBy(static row => row.Source)
                    .Map(source => toSeq(source).Map(static row => DurationOf(row.Event)).Fold(Duration.Zero, static (sum, value) => sum + value))
                    .Fold(Duration.Zero, static (maximum, value) => value > maximum ? value : maximum))
                .Fold(Duration.Zero, static (sum, value) => sum + value);
            Duration globalTime = layer.Events.Fold(Duration.Zero, static (sum, value) => sum + DurationOf(value));
            return sourceTime + globalTime;
        }).Fold(Duration.Zero, static (sum, value) => sum + value);
        return new ScanReceipt(
            audit,
            loads,
            fields,
            thermal,
            exposure.Count,
            events.Count(static value => value is ScanEvent.Jump),
            exposure.Count(static value => value.Pass > 0),
            ordered.Map(static row => row.StitchPeers.Count).Sum(),
            path,
            loads.Map(static load => load.Energy).Fold(Energy.Zero, static (sum, value) => sum + value),
            buildTime,
            bytes);
    }

    private static Duration DurationOf(ScanEvent value) => value.Switch(
        expose: static row => Duration.FromSeconds(
            (row.From.DistanceTo(row.To) + row.SkywritingLead.Millimeters + row.SkywritingLag.Millimeters)
                / row.Speed.MillimetersPerSecond) + row.Dwell,
        jump: static row => Duration.FromSeconds(row.From.DistanceTo(row.To) / row.Speed.MillimetersPerSecond),
        synchronize: static row => row.Duration,
        recoat: static row => Duration.FromSeconds(row.Travel.Millimeters / row.Speed.MillimetersPerSecond) + row.Delay,
        layerDelay: static row => row.Duration);

    private static Length LengthOf(ScanEvent value) => value.Switch(
        expose: static row => Length.FromMillimeters(
            row.From.DistanceTo(row.To) + row.SkywritingLead.Millimeters + row.SkywritingLag.Millimeters),
        jump: static row => Length.FromMillimeters(row.From.DistanceTo(row.To)),
        synchronize: static _ => Length.Zero,
        recoat: static row => row.Travel,
        layerDelay: static _ => Length.Zero);

    private static double Duty(ScanEvent.Expose row) => row.PulseOn + row.PulseOff == Duration.Zero
        ? 1.0
        : row.PulseOn.Seconds / (row.PulseOn + row.PulseOff).Seconds;

    private static Option<int> WaveOf(ScanEvent value) => value.Switch(
        expose: static row => Some(row.Wave),
        jump: static row => Some(row.Wave),
        synchronize: static _ => Option<int>.None,
        recoat: static _ => Option<int>.None,
        layerDelay: static _ => Option<int>.None);

    private static K<Validation<Error>, Unit> Gate(bool valid, string locus) =>
        (valid ? Fin.Succ(unit) : Fin.Fail<Unit>(new GeometryFault.DegenerateInput(Kind.Mesh, -1, locus).ToError())).ToValidation();
}

// --- [FIELD_PARTITION] -----------------------------------------------------------------------------------------------------------------------------
public static class SourcePartition {
    public static Fin<Seq<FieldCell>> Build(SliceStack stack, SourcePolicy policy) =>
        stack.LayerCount == 0 || policy.Sources.IsEmpty || stack.X.Length == 0 || stack.X.Length != stack.Y.Length || stack.X.Length != stack.Z.Length
            ? Fin.Fail<Seq<FieldCell>>(new GeometryFault.DegenerateInput(Kind.Mesh, -1, "scan:source-partition").ToError())
            : policy.Sources.Map(static source => (source.Field.Center.X, source.Field.Center.Y)).Distinct().Count != policy.Sources.Length
            ? Fin.Fail<Seq<FieldCell>>(new GeometryFault.DegenerateInput(Kind.Mesh, -1, "scan:duplicate-source-sites").ToError())
            : Try.lift(() => {
                BoundingBox bound = new(
                    new Point3d(stack.X.Min(), stack.Y.Min(), 0.0),
                    new Point3d(stack.X.Max(), stack.Y.Max(), 0.0));
                VoronoiPlane plane = new(bound.Min.X, bound.Min.Y, bound.Max.X, bound.Max.Y);
                plane.SetSites(policy.Sources.Map(static source => new VoronoiSite(source.Field.Center.X, source.Field.Center.Y)).ToList());
                plane.Tessellate(BorderEdgeGeneration.MakeBorderEdges);
                plane.Relax(policy.FieldRelaxations, (float)policy.FieldRelaxationStrength.DecimalFractions, reTessellate: true);
                return plane.DuplicateCount != 0 || plane.Sites.Count != policy.Sources.Length
                    ? Fin.Fail<Seq<FieldCell>>(new GeometryFault.DegenerateInput(
                        Kind.Mesh,
                        -1,
                        $"scan:duplicate-source-sites:{plane.DuplicateCount}").ToError())
                    : Fin.Succ(toSeq(plane.Sites).Map(site => new FieldCell(
                        policy.Sources[plane.Sites.IndexOf(plane.GetNearestSiteTo(
                            site.Centroid.X,
                            site.Centroid.Y,
                            NearestSiteLookupMethod.KDTree))].Id,
                        toSeq(site.ClockwisePoints).Map(static point => new Point2d(point.X, point.Y)),
                        toSeq(site.Neighbours).Map(neighbour => policy.Sources[plane.Sites.IndexOf(neighbour)].Id),
                        new Point2d(site.Centroid.X, site.Centroid.Y),
                        Length.FromMillimeters(toSeq(site.ClockwiseEdges).Map(static edge => edge.Length).Sum()),
                        site.Closed)));
            }).Run()
                .MapFail(static error => new GeometryFault.DegenerateInput(Kind.Mesh, -1, $"scan:voronoi:{error.Message}").ToError())
                .Bind(identity);

    public static Fin<Seq<SourceAssignment>> Assign(
        Seq<CandidateVector> vectors,
        Seq<FieldCell> fields,
        SourcePolicy policy,
        int maximumVectors) {
        if (vectors.IsEmpty)
            return Fin.Succ(Seq<SourceAssignment>());
        if (vectors.Count > maximumVectors)
            return Fin.Fail<Seq<SourceAssignment>>(new GeometryFault.DegenerateInput(Kind.Mesh, -1, $"scan:vector-cap:{vectors.Count}").ToError());
        return Try.lift(() => {
            int capacity = checked(vectors.Count * policy.Sources.Count);
            using MemoryOwner<double> scores = MemoryOwner<double>.Allocate(capacity, AllocationMode.Clear);
            ScoreAction action = new(scores.Memory, policy.Sources.Count, vectors.ToArr(), policy.Sources);
            ParallelHelper.For2D(0, vectors.Count, 0, policy.Sources.Count, in action);
            Arr<(int Source, double Score)> elected = Elect(scores.Span, vectors.Count, policy.Sources.Count, policy.BalanceWeight.DecimalFractions);
            return vectors.Map((vector, row) => (Vector: vector, Election: elected[row]))
                .Find(static row => row.Election.Source < 0)
                .Match(
                    Some: row => Fin.Fail<Seq<SourceAssignment>>(new GeometryFault.DegenerateInput(Kind.Mesh, -1,
                        $"scan:source-field-miss:{row.Vector.Layer}:{row.Vector.Geometry.A}").ToError()),
                    None: () => Fin.Succ(vectors.Map((vector, row) => {
                        LaserSource source = policy.Sources[elected[row].Source];
                        return new SourceAssignment(vector, source, Peers(vector, source, fields, policy), elected[row].Score);
                    })));
        }).Run()
            .MapFail(static error => new GeometryFault.DegenerateInput(Kind.Mesh, -1, $"scan:score-plane:{error.Message}").ToError())
            .Bind(identity);
    }

    private static Seq<LaserSource> Peers(CandidateVector vector, LaserSource source, Seq<FieldCell> fields, SourcePolicy policy) =>
        fields.Find(field => field.Source == source.Id)
            .Map(field => field.Neighbours.Bind(id => policy.Sources
                .Find(candidate => candidate.Id == id)
                .Filter(candidate => candidate.Field.Center.DistanceTo(0.5 * (vector.Geometry.A + vector.Geometry.B))
                    <= candidate.StitchWidth.Millimeters + policy.Overlap.Millimeters)
                .ToSeq()))
            .IfNone(Seq<LaserSource>());

    // Exemption: the balance cell must carry between elections, so the running assignment stays a span loop
    // inside this one kernel; a rejected election lands as source -1 and the caller converts it on the rail.
    private static Arr<(int Source, double Score)> Elect(Span<double> plane, int rows, int width, double balanceWeight) {
        (int Source, double Score)[] elected = new (int, double)[rows];
        using SpanOwner<double> load = SpanOwner<double>.Allocate(width, AllocationMode.Clear);
        using SpanOwner<double> balanced = SpanOwner<double>.Allocate(width, AllocationMode.Clear);
        for (int row = 0; row < rows; row++) {
            ReadOnlySpan<double> score = plane.Slice(row * width, width);
            TensorPrimitives.MultiplyAdd(load.Span, balanceWeight, score, balanced.Span);
            int index = TensorPrimitives.IndexOfMin(balanced.Span);
            bool admitted = index >= 0 && double.IsFinite(score[index]);
            elected[row] = admitted ? (index, score[index]) : (-1, double.PositiveInfinity);
            if (admitted)
                load.Span[index]++;
        }
        return elected;
    }

    private readonly struct ScoreAction(Memory<double> scores, int width, Arr<CandidateVector> vectors, Arr<LaserSource> sources) : IAction2D {
        public void Invoke(int row, int column) {
            CandidateVector vector = vectors[row];
            LaserSource source = sources[column];
            Point3d midpoint = 0.5 * (vector.Geometry.A + vector.Geometry.B);
            scores.Span[row * width + column] = source.Field.Contains(midpoint)
                ? TensorPrimitives.Distance(
                    [midpoint.X, midpoint.Y, midpoint.Z],
                    [source.Field.Center.X, source.Field.Center.Y, source.Field.Center.Z])
                : double.PositiveInfinity;
        }
    }
}

public static class CellDiagram {
    public static Fin<Seq<BoundingBox>> Build(BoundingBox bound, CellProgram policy) =>
        policy.Pitch <= Length.Zero || policy.Relaxations < 0 || policy.RelaxationStrength < Ratio.Zero || policy.MaximumSites <= 0 || policy.MergeBelowArea < Area.Zero
            ? Fin.Fail<Seq<BoundingBox>>(new GeometryFault.DegenerateInput(Kind.Mesh, -1, "scan:cell-program").ToError())
            : Try.lift(() => {
                VoronoiPlane plane = new(bound.Min.X, bound.Min.Y, bound.Max.X, bound.Max.Y);
                int count = Math.Min(policy.MaximumSites, Math.Max(1, (int)Math.Ceiling(
                    (bound.Diagonal.X * bound.Diagonal.Y) / Math.Pow(policy.Pitch.Millimeters, 2.0))));
                plane.GenerateRandomSites(count, PointGenerationMethod.Uniform, new SeededRandomNumberGenerator(policy.Seed));
                plane.Tessellate(BorderEdgeGeneration.MakeBorderEdges);
                plane.Relax(policy.Relaxations, (float)policy.RelaxationStrength.DecimalFractions, reTessellate: true);
                plane.MergeSites((left, right) => CellArea(left) < policy.MergeBelowArea.SquareMillimeters
                    ? VoronoiSiteMergeDecision.MergeIntoSite2
                    : CellArea(right) < policy.MergeBelowArea.SquareMillimeters
                        ? VoronoiSiteMergeDecision.MergeIntoSite1
                        : VoronoiSiteMergeDecision.DontMerge);
                return toSeq(plane.Sites).Filter(static site => site.Closed).Map(site => new BoundingBox(
                    new Point3d(site.ClockwisePoints.Min(static point => point.X), site.ClockwisePoints.Min(static point => point.Y), bound.Min.Z),
                    new Point3d(site.ClockwisePoints.Max(static point => point.X), site.ClockwisePoints.Max(static point => point.Y), bound.Max.Z)));
            }).Run().MapFail(static error => new GeometryFault.DegenerateInput(Kind.Mesh, -1, $"scan:cells:{error.Message}").ToError());

    private static double CellArea(VoronoiSite site) =>
        Math.Abs(toSeq(site.ClockwisePoints).Zip(toSeq(site.ClockwisePoints).Tail.Add(site.ClockwisePoints[0]))
            .Map(static pair => pair.First.X * pair.Second.Y - pair.Second.X * pair.First.Y).Sum()) / 2.0;
}

public static class ScanSort {
    public static Fin<Seq<SourceAssignment>> Order(Seq<SourceAssignment> rows, ScanOrder order, ScanPlane plane) =>
        Fin.Succ(rows.IsEmpty ? rows : order.Orient(rows.OrderBy(row => order.Project(row, plane)).ToSeq()));
}

// --- [CANONICAL_EGRESS] ----------------------------------------------------------------------------------------------------------------------------
public static class ScanCodec {
    public static byte[] Write(ScanIdentity identity, Seq<ScanLayer> layers) {
        using ArrayPoolBufferWriter<byte> writer = new();
        Identity(identity, writer);
        Int32(layers.Count, writer);
        layers.Iter(layer => {
            Int32(layer.Layer, writer);
            Float64(layer.Elevation.Millimeters, writer);
            Int32(layer.Sources.Count, writer);
            layer.Sources.Iter(source => {
                Int32(source.Source, writer);
                Int32(source.Events.Count, writer);
                source.Events.Iter(scanEvent => Event(scanEvent, writer));
            });
            Int32(layer.Events.Count, writer);
            layer.Events.Iter(scanEvent => Event(scanEvent, writer));
        });
        return writer.WrittenSpan.ToArray();
    }

    private static void Event(ScanEvent scanEvent, ArrayPoolBufferWriter<byte> writer) => scanEvent.Switch(
        state: writer,
        expose: static (sink, value) => {
            Byte(1, sink); Int32(value.Source, sink); Utf8(value.Class.Key, sink); Point(value.From, sink); Point(value.To, sink);
            Float64(value.Power.Watts, sink); Float64(value.Speed.MillimetersPerSecond, sink); Float64(value.Dwell.Seconds, sink);
            Float64(value.Focus.Millimeters, sink); Float64(value.Spot.Millimeters, sink); Float64(value.PulseOn.Seconds, sink);
            Float64(value.PulseOff.Seconds, sink); Float64(value.SkywritingLead.Millimeters, sink); Float64(value.SkywritingLag.Millimeters, sink);
            Int32(value.StitchPeers.Count, sink); value.StitchPeers.Iter(peer => Int32(peer, sink));
            Int32(value.Wave, sink); Int32(value.Pass, sink); return unit;
        },
        jump: static (sink, value) => { Byte(2, sink); Int32(value.Source, sink); Point(value.From, sink); Point(value.To, sink); Float64(value.Speed.MillimetersPerSecond, sink); Int32(value.Wave, sink); return unit; },
        synchronize: static (sink, value) => { Byte(3, sink); Int32(value.Sources.Count, sink); value.Sources.Iter(id => Int32(id, sink)); Int32(value.Wave, sink); Float64(value.Duration.Seconds, sink); Utf8(value.Reason, sink); return unit; },
        recoat: static (sink, value) => { Byte(4, sink); Int32(value.Layer, sink); Float64(value.Travel.Millimeters, sink); Float64(value.Speed.MillimetersPerSecond, sink); Float64(value.Delay.Seconds, sink); return unit; },
        layerDelay: static (sink, value) => { Byte(5, sink); Int32(value.Layer, sink); Float64(value.Duration.Seconds, sink); return unit; });

    private static void Identity(ScanIdentity identity, ArrayPoolBufferWriter<byte> writer) {
        ScanPolicy policy = identity.Policy;
        Profile(policy.Base, writer);
        Int32(policy.Profiles.Count, writer);
        policy.Profiles.OrderBy(static pair => pair.Key.Key).Iter(pair => {
            Utf8(pair.Key.Key, writer);
            Profile(pair.Value, writer);
        });
        Hatch(policy.Hatch, writer);
        Utf8(policy.Order.Key, writer);
        Sources(policy.Sources, writer);
        Timing(policy.Timing, writer);
        Rotation(policy.Rotation, writer);
        Compensation(policy.Compensation, writer);
        Offset(policy.Offset, writer);
        Float64(policy.ThermalSeparation.Millimeters, writer);
        Float64(policy.IntersectionTolerance.Millimeters, writer);
        Int32(policy.ThermalWindow, writer);
        Int32(policy.MaximumVectors, writer);
        Int32(policy.DownSkinLayers, writer);
        Int32(policy.UpSkinLayers, writer);
    }

    private static void Profile(ExposureProfile value, ArrayPoolBufferWriter<byte> writer) {
        Float64(value.Power.Watts, writer); Float64(value.Speed.MillimetersPerSecond, writer);
        Float64(value.Spacing.Millimeters, writer); Float64(value.Dwell.Seconds, writer);
        Float64(value.Spot.Millimeters, writer); Float64(value.Focus.Millimeters, writer);
        Float64(value.PulseOn.Seconds, writer); Float64(value.PulseOff.Seconds, writer);
        Float64(value.SkywritingLead.Millimeters, writer); Float64(value.SkywritingLag.Millimeters, writer);
    }

    private static void Sources(SourcePolicy value, ArrayPoolBufferWriter<byte> writer) {
        Int32(value.Sources.Length, writer);
        value.Sources.Iter(source => {
            Int32(source.Id, writer); Point(source.Field.Min, writer); Point(source.Field.Max, writer);
            Float64(source.MaximumPower.Watts, writer); Float64(source.SpotDiameter.Millimeters, writer);
            Float64(source.StitchWidth.Millimeters, writer); Float64(source.FocusMinimum.Millimeters, writer);
            Float64(source.FocusMaximum.Millimeters, writer); Float64(source.Drift.DecimalFractions, writer);
            Utf8(source.Calibration.Digest.ToString("x32", CultureInfo.InvariantCulture), writer);
        });
        Float64(value.BalanceWeight.DecimalFractions, writer); Float64(value.PlumeClearance.Millimeters, writer);
        Float64(value.Overlap.Millimeters, writer); Int32(value.FieldRelaxations, writer);
        Float64(value.FieldRelaxationStrength.DecimalFractions, writer);
        Float64(value.GasBearing.X, writer); Float64(value.GasBearing.Y, writer); Float64(value.GasBearing.Z, writer);
    }

    private static void Timing(TimingPolicy value, ArrayPoolBufferWriter<byte> writer) {
        Float64(value.JumpSpeed.MillimetersPerSecond, writer); Float64(value.LayerDelay.Seconds, writer);
        Float64(value.RecoatDelay.Seconds, writer); Float64(value.SourceDelay.Seconds, writer);
        Float64(value.RecoatSpeed.MillimetersPerSecond, writer); Float64(value.RecoatTravel.Millimeters, writer);
    }

    private static void Rotation(RotationPolicy value, ArrayPoolBufferWriter<byte> writer) {
        Float64(value.LayerIncrement.Radians, writer); Int32(value.Cycle, writer); Float64(value.ContourOffset.Radians, writer);
    }

    private static void Offset(OffsetPolicy value, ArrayPoolBufferWriter<byte> writer) {
        Utf8(value.Join.Key, writer); Utf8(value.End.Key, writer); Float64(value.MiterLimit, writer);
        Float64(value.ArcTolerance, writer); Byte(value.PreserveCollinear ? (byte)1 : (byte)0, writer);
        Byte(value.ReverseSolution ? (byte)1 : (byte)0, writer); Byte(value.MergeGroups ? (byte)1 : (byte)0, writer);
    }

    private static void Hatch(HatchProgram program, ArrayPoolBufferWriter<byte> writer) => program.Switch(
        state: writer,
        lines: static (sink, value) => { Byte(1, sink); Float64(value.Bearing.Radians, sink); return unit; },
        cells: static (sink, value) => {
            Byte(2, sink); Float64(value.Bearing.Radians, sink); Float64(value.Cells.Pitch.Millimeters, sink);
            Int32(value.Cells.Relaxations, sink); Float64(value.Cells.RelaxationStrength.DecimalFractions, sink);
            Int32(value.Cells.MaximumSites, sink); Int32(value.Cells.Seed, sink); Float64(value.Cells.MergeBelowArea.SquareMillimeters, sink);
            return unit;
        },
        contours: static (sink, value) => { Byte(3, sink); Int32(value.Passes, sink); Float64(value.Offset.Millimeters, sink); return unit; },
        generated: static (sink, value) => { Byte(4, sink); Utf8(value.Identity.Digest.ToString("x32", CultureInfo.InvariantCulture), sink); return unit; });

    private static void Compensation(DistortionCompensation compensation, ArrayPoolBufferWriter<byte> writer) => compensation.Switch(
        state: writer,
        none: static (sink, _) => { Byte(1, sink); return unit; },
        affine: static (sink, value) => {
            Byte(2, sink); Transform(value.BuildToCommand, sink); Utf8(value.Calibration.Digest.ToString("x32", CultureInfo.InvariantCulture), sink); return unit;
        },
        generated: static (sink, value) => { Byte(3, sink); Utf8(value.Calibration.Digest.ToString("x32", CultureInfo.InvariantCulture), sink); return unit; });

    private static void Transform(Transform value, ArrayPoolBufferWriter<byte> writer) {
        Float64(value.M00, writer); Float64(value.M01, writer); Float64(value.M02, writer); Float64(value.M03, writer);
        Float64(value.M10, writer); Float64(value.M11, writer); Float64(value.M12, writer); Float64(value.M13, writer);
        Float64(value.M20, writer); Float64(value.M21, writer); Float64(value.M22, writer); Float64(value.M23, writer);
        Float64(value.M30, writer); Float64(value.M31, writer); Float64(value.M32, writer); Float64(value.M33, writer);
    }

    private static void Point(Point3d point, ArrayPoolBufferWriter<byte> writer) { Float64(point.X, writer); Float64(point.Y, writer); Float64(point.Z, writer); }
    private static void Byte(byte value, ArrayPoolBufferWriter<byte> writer) { Span<byte> span = writer.GetSpan(1); span[0] = value; writer.Advance(1); }
    private static void Int32(int value, ArrayPoolBufferWriter<byte> writer) { Span<byte> span = writer.GetSpan(sizeof(int)); BinaryPrimitives.WriteInt32LittleEndian(span, value); writer.Advance(sizeof(int)); }
    private static void Int32(LaserId value, ArrayPoolBufferWriter<byte> writer) => Int32(value.ToValue(), writer);
    private static void Float64(double value, ArrayPoolBufferWriter<byte> writer) { Span<byte> span = writer.GetSpan(sizeof(double)); BinaryPrimitives.WriteInt64LittleEndian(span, BitConverter.DoubleToInt64Bits(value)); writer.Advance(sizeof(double)); }
    private static void Utf8(string value, ArrayPoolBufferWriter<byte> writer) {
        int length = Encoding.UTF8.GetByteCount(value);
        Int32(length, writer);
        Span<byte> target = writer.GetSpan(length);
        int written = Encoding.UTF8.GetBytes(value, target);
        writer.Advance(written);
    }
}

public static class ScanGeometry {
    public static Seq<Edge3> Parallel(BoundingBox bound, Angle bearing, Length spacing) {
        Vector3d direction = new(Math.Cos(bearing.Radians), Math.Sin(bearing.Radians), 0.0);
        Vector3d normal = new(-direction.Y, direction.X, 0.0);
        Point3d center = bound.Center;
        double span = bound.Diagonal.Length;
        int count = Math.Max(1, (int)Math.Ceiling(span / spacing.Millimeters));
        return toSeq(Enumerable.Range(-count, 2 * count + 1)).Map(index =>
            new Edge3(center + index * spacing.Millimeters * normal - span * direction, center + index * spacing.Millimeters * normal + span * direction));
    }
}
```

```mermaid
---
config:
  layout: elk
  look: handDrawn
  theme: dark
---
flowchart LR
    accTitle: Additive scanpath planning flow
    accDescr: Admitted layers become bounded candidate vectors, source assignments, thermal schedules, compensated scan events, and canonical plan evidence.
    Stack["SliceStack"] --> Audit["Audit.Preflight"]
    Audit --> Regions["ExposureRegion rows"]
    Regions --> Cells["HatchProgram + relaxed cells"]
    Cells --> Fields["Voronoi source fields"]
    Fields --> Assign["pooled score plane + IndexOfMin"]
    Assign --> Order["thermal / gas / plume schedule"]
    Order --> Events["explicit ScanEvent program"]
    Events --> Codec["XYZ + physics + timing codec"]
    Codec --> Key["ContentKey.Of ScanVectors"]
    Key --> Plan["ScanPlan + ScanReceipt"]
```

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
