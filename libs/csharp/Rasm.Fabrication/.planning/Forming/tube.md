# [RASM_FABRICATION_TUBE_PROGRAM]

`TubeProgram` owns one tube-forming algebra across discrete bending, axis-specific section roll curving, and cope projection. `TubeSection`, `RollSection`, `TubeTool`, and `TubePolicy` admit section mechanics, material, weld seam, tooling, deformation limits, numeric tolerances, and egress policy once.

`TubeProgram.Apply` composes the frozen `ProcessEnvelope.Bender`, `ProcessEnvelope.Roll`, `ProcessBudget.Formed`, `Intersection.Apply`, `Development.Apply`, `UvIsland`, and `ContentKey.Of` wires. Intersection provenance and atlas provenance remain intact through sectioned cope projection.

## [01]-[INDEX]

- [02]-[TUBE_FORMING]: generated process and format families, section and roll mechanics, tooling admission, and the operation and result vocabularies.
- [03]-[TUBE_PROGRAM]: operation dispatch, neutral-axis bend programs, multi-pass roll schedules, cope generation with internalized provenance, developed-chain projection, and the content-keyed preimage.

## [02]-[TUBE_FORMING]

- Owner: `TubeFormKind` owns discrete process physics; `BendFormat` owns command projection; `CopeEnd` owns analytic branch-end selection; `TubeSection` owns closed thin-wall mechanics; `RollSection` and `RollAxis` own closed, open, solid, and plate roll mechanics; `TubeTool` owns tooling evidence; `TubeProgram` owns all operation dispatch and projection.
- Cases: `TubeOp` carries `Form`, `Roll`, and `Cope`; `TubeResult` mirrors those modalities; `TubeCommand` binds one canonical `TubeCoordinate` to a `BendFormat` projection row; `TubeFormKind` carries rotary-draw, compression, ram, push, stretch, and freeform behavior; `CopeEnd` selects the negative or positive analytic root; `MandrelKind` carries the tooling axis.
- Entry: `TubeProgram.Apply(TubeOp)` is the one polymorphic entry for every modality.
- Auto: centerlines normalize once, tooling resolves per bend, neutral-axis length consumes the forming budget, the folder's `ElasticLaw` inverts the CUBIC elastic-recovery law over the loaded radius for bend springback and a bracketed root recovers pass curvature — the only transcendental inversions on the page, and the cope station's quadratic never reaches them — mandrel rows supply their own interior wall support, weld-seam rotation propagates, roll passes generate command curvature with axis modulus and distortion gates, and sectioned cope lowers exact crossing keys through source vertices or source faces into developed islands.
- Receipt: Form results carry the canonical coordinate beside every projected command, force, tooling position, deformation witness, terminal feed, nominal centerline, developed body, cut length, and key; roll results carry input, commanded, and recovered radius, axis, distortion, and machine margin; cope results carry chart-coherent developed runs and defining-face crossing wedges.
- Packages: `LanguageExt.Core`, `Thinktecture.Runtime.Extensions`, the `Rasm.Element` `CanonicalWriter` codec behind `FabricationCanon`, `MathNet.Numerics`, `UnitsNet`, `RhinoCommon`, `Rasm.Meshing`, `Rasm.Parametric`, `Rasm.Processing`, and `ContentKey` compose the surface.
- Growth: A discrete process is one `TubeFormKind` row, a command convention is one `BendFormat` row, a physical tool is one catalog row, an analytic branch end is one `CopeEnd` row, a roll target is data, and a new modality is one `TubeOp`/`TubeResult` case pair.
- Boundary: Forming owns tube mechanics and projection; machine capacity, process material physics, exact intersection, development, planar loop admission, posting text, and content identity remain at their canonical owners.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------------------------------------------------------
using LanguageExt;
using LanguageExt.Common;
using MathNet.Numerics.RootFinding;
using Rasm.Domain;
using Rasm.Element.Projection;
using Rasm.Fabrication.Kinematics;
using Rasm.Fabrication.Process;
using Rasm.Meshing;
using Rasm.Numerics;
using Rasm.Parametric;
using Rasm.Processing;
using Rhino.Geometry;
using Thinktecture;
using UnitsNet;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Forming;

// --- [TYPES] --------------------------------------------------------------------------------------------------------------------------------------
// Each row carries the three facts a discrete process changes about one bend: how hard it loads the section, how
// much of the turn comes back elastically, and how much straight tube it demands before the arc. Stretch bending
// tensions the whole section past yield against a form die, so it loads hardest, recovers least — the reason the
// process exists — and demands a grip at BOTH ends rather than one clamp.
[SmartEnum<string>]
public sealed partial class TubeFormKind {
    public static readonly TubeFormKind RotaryDraw = new("rotary-draw", forceFactor: 1.0, recoveryFactor: 1.0, static (tool, _) => tool.ClampLengthMm);
    public static readonly TubeFormKind Compression = new("compression", forceFactor: 1.35, recoveryFactor: 1.15, static (tool, _) => tool.ClampLengthMm);
    public static readonly TubeFormKind Ram = new("ram", forceFactor: 1.8, recoveryFactor: 1.35, static (tool, _) => tool.MinStraightMm);
    public static readonly TubeFormKind Push = new("push", forceFactor: 0.8, recoveryFactor: 0.9, static (_, policy) => policy.MinimumSegmentMm);
    public static readonly TubeFormKind Stretch = new("stretch", forceFactor: 2.2, recoveryFactor: 0.35, static (tool, _) => 2.0 * tool.ClampLengthMm);
    public static readonly TubeFormKind Freeform = new("freeform", forceFactor: 0.7, recoveryFactor: 0.8, static (_, policy) => policy.MinimumSegmentMm);

    public double ForceFactor { get; }
    public double RecoveryFactor { get; }

    [UseDelegateFromConstructor]
    public partial double MinimumStraight(TubeTool tool, TubePolicy policy);
}

[SmartEnum<string>]
public sealed partial class BendFormat {
    public static readonly BendFormat Ybc = new("ybc", static coordinate => coordinate with { RotationDeg = Normalize(coordinate.RotationDeg) });
    public static readonly BendFormat Lra = new("lra", static coordinate => coordinate with { RotationDeg = Normalize(-coordinate.RotationDeg) });
    public static readonly BendFormat Cartesian = new("cartesian", static coordinate => coordinate);

    [UseDelegateFromConstructor]
    public partial TubeCoordinate Project(TubeCoordinate coordinate);

    private static double Normalize(double degrees) => ((degrees % 360.0) + 360.0) % 360.0;
}

[SmartEnum<string>]
public sealed partial class CopeEnd {
    public static readonly CopeEnd Negative = new("negative", static (index, lower, lowerZ, _, _) => lower
        ? Fin.Succ(lowerZ)
        : Fin.Fail<double>(new GeometryFault.DegenerateInput(Kind.Polyline, index, $"tube:cope-root:{index}:negative").ToError()));
    public static readonly CopeEnd Positive = new("positive", static (index, _, _, upper, upperZ) => upper
        ? Fin.Succ(upperZ)
        : Fin.Fail<double>(new GeometryFault.DegenerateInput(Kind.Polyline, index, $"tube:cope-root:{index}:positive").ToError()));

    [UseDelegateFromConstructor]
    public partial Fin<double> Select(int index, bool lower, double lowerZ, bool upper, double upperZ);
}

[SmartEnum<string>]
public sealed partial class TubeSectionFamily {
    public static readonly TubeSectionFamily Circular = new("circular", analyticCope: true, static properties =>
        properties.CurvedEdges > 0 && properties.MajorMm / properties.MinorMm <= 1.01 && properties.RadialRatio <= 1.01);
    public static readonly TubeSectionFamily Elliptic = new("elliptic", analyticCope: false, static properties =>
        properties.CurvedEdges > 0 && properties.MajorMm / properties.MinorMm > 1.01);
    // Structural RHS and SHS carry a formed corner radius on every vertex, so a rectilinear tube admits either a
    // sharp four-vertex profile or one whose curved-edge count equals its vertex count.
    public static readonly TubeSectionFamily Rectilinear = new("rectilinear", analyticCope: false, static properties =>
        properties.VertexCount == 4 && properties.CurvedEdges is 0 or 4);
    public static readonly TubeSectionFamily Polygonal = new("polygonal", analyticCope: false, static properties =>
        properties.CurvedEdges == 0 && properties.VertexCount >= 3);
    public static readonly TubeSectionFamily Custom = new("custom", analyticCope: false, static _ => true);

    public bool AnalyticCope { get; }

    [UseDelegateFromConstructor]
    public partial bool Admits(SectionProperties properties);
}

[SmartEnum<string>]
public sealed partial class MandrelKind {
    public static readonly MandrelKind None = new("none", 0, static (_, _) => 0.0);
    public static readonly MandrelKind Plug = new("plug", 0, static (tool, major) => 0.5 * tool.MandrelNoseMm / major);
    public static readonly MandrelKind FormedTip = new("formed-tip", 0, static (tool, major) => tool.MandrelNoseMm / major);
    public static readonly MandrelKind Ball = new("ball", 1, static (tool, major) => tool.BallCount + (tool.MandrelNoseMm / major));
    public static readonly MandrelKind LinkedBall = new("linked-ball", 2, static (tool, major) => (1.5 * tool.BallCount) + (tool.MandrelNoseMm / major));
    public static readonly MandrelKind Flexible = new("flexible", 1, static (tool, major) => (1.25 * tool.BallCount) + (tool.MandrelNoseMm / major));

    public int MinimumBalls { get; }

    // Interior wall support is the mandrel's own law: a plug reaches only its nose, a ball train carries every
    // ball through the arc, and a bare bend leaves the section wall supporting itself.
    [UseDelegateFromConstructor]
    public partial double InteriorSupport(TubeTool tool, double majorMm);
}

public readonly record struct SectionProperties(
    double MetalAreaMm2,
    Point2d Centroid,
    double IxMm4,
    double IyMm4,
    double JMm4,
    double SxMm3,
    double SyMm3,
    double PerimeterMm,
    double WidthMm,
    double HeightMm,
    double MajorMm,
    double MinorMm,
    int VertexCount,
    int CurvedEdges,
    double RadialRatio);

[ComplexValueObject]
[ValidationError<FabricationFault>]
public sealed partial class TubeSection {
    public TubeSectionFamily Family { get; }
    public Loop Profile { get; }
    public double WallMm { get; }
    public Option<double> WeldSeamDeg { get; }
    public SectionProperties Properties { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref FabricationFault? validationError,
        ref TubeSectionFamily family,
        ref Loop profile,
        ref double wallMm,
        ref Option<double> weldSeamDeg,
        ref SectionProperties properties) =>
        validationError = family is not null && profile is { Closed: true }
            && double.IsFinite(wallMm) && wallMm > 0.0 && wallMm < properties.MinorMm / 2.0
            && weldSeamDeg.ForAll(static angle => double.IsFinite(angle) && angle is >= 0.0 and < 360.0)
            && Seq(properties.MetalAreaMm2, properties.IxMm4, properties.IyMm4, properties.JMm4,
                    properties.SxMm3, properties.SyMm3, properties.PerimeterMm, properties.WidthMm, properties.HeightMm,
                    properties.MajorMm, properties.MinorMm, properties.RadialRatio)
                .ForAll(static value => double.IsFinite(value) && value > 0.0)
            && properties.VertexCount >= 3 && properties.CurvedEdges >= 0 && properties.CurvedEdges <= properties.VertexCount
            && family.Admits(properties)
                ? null
                : new FabricationFault.PolicyInadmissible(FabConcern.Forming, "tube:tube-section");

    public static Fin<TubeSection> Admit(
        TubeSectionFamily family,
        Loop profile,
        Length wall,
        Option<Angle> weldSeam,
        Length chordTolerance,
        int maximumStations) =>
        family is null || profile is null
            ? Fin.Fail<TubeSection>(new FabricationFault.PolicyInadmissible(FabConcern.Forming, "tube:section"))
            : from properties in Mechanics(profile, wall.Millimeters, chordTolerance.Millimeters, maximumStations)
              from section in TubeSection.Validate(
                  family,
                  profile,
                  wall.Millimeters,
                  weldSeam.Map(static angle => angle.Radians / Angle.FromDegrees(1.0).Radians),
                  properties,
                  out TubeSection admitted) is { } error
                    ? Fin.Fail<TubeSection>(new FabricationFault.PolicyInadmissible(FabConcern.Forming, error.Message))
                    : Fin.Succ(admitted)
              select section;

    private static Fin<SectionProperties> Mechanics(
        Loop profile,
        double wallMm,
        double chordToleranceMm,
        int maximumStations) =>
        from measured in profile.Apply(new ProfileOp.Measure())
        from metric in measured is ProfileResult.Measure value
            ? Fin.Succ(value)
            : Fin.Fail<ProfileResult.Measure>(new FabricationFault.PolicyInadmissible(FabConcern.Forming, "tube:section-measure"))
        from _chord in double.IsFinite(chordToleranceMm) && chordToleranceMm > 0.0 && maximumStations >= 3
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new FabricationFault.PolicyInadmissible(FabConcern.Forming, "tube:section-policy"))
        let stationCount = Math.Max(3, (int)Math.Ceiling(metric.Path.Millimeters / chordToleranceMm))
        from _stations in stationCount <= maximumStations
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new FabricationFault.PolicyInadmissible(FabConcern.Forming, "tube:section-stations"))
        from stations in toSeq(Enumerable.Range(0, stationCount)).Traverse(index =>
            profile.Apply(new ProfileOp.Sample(Length.FromMillimeters(metric.Path.Millimeters * index / stationCount)))
                .Bind(static result => result is ProfileResult.Sampled sample
                    ? Fin.Succ(sample.Point)
                    : Fin.Fail<Point3d>(new FabricationFault.PolicyInadmissible(FabConcern.Forming, "tube:section-sample")))
                .ToValidation()).As().ToFin()
        from bound in profile.Apply(new ProfileOp.Bound())
        from box in bound is ProfileResult.Bound bounded
            ? Fin.Succ(bounded.Box)
            : Fin.Fail<BoundingBox>(new FabricationFault.PolicyInadmissible(FabConcern.Forming, "tube:section-bound"))
        let edges = toSeq(Enumerable.Range(0, stations.Count))
            .Map(index => (First: stations[index], Second: stations[(index + 1) % stations.Count]))
        let weighted = edges.Map(edge => {
            double length = edge.First.DistanceTo(edge.Second);
            Point3d midpoint = new(
                (edge.First.X + edge.Second.X) / 2.0,
                (edge.First.Y + edge.Second.Y) / 2.0,
                (edge.First.Z + edge.Second.Z) / 2.0);
            return (Area: length * wallMm, Midpoint: midpoint, Length: length);
        })
        let area = weighted.Fold(0.0, static (sum, row) => sum + row.Area)
        // A zero metal area divides the centroid, every second moment, and the machine-capacity gate that reads
        // them: the section refuses here, where the degeneracy is, rather than publishing NaN properties a torque
        // comparison then reads as "within capacity".
        from _area in Witness.Positive(area)
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new GeometryFault.DegenerateInput(Kind.Polyline, None, "tube:section-area").ToError())
        let centroid = weighted.Fold(Vector3d.Zero, (sum, row) => sum + ((Vector3d)row.Midpoint * row.Area)) / area
        let ix = weighted.Fold(0.0, (sum, row) => sum + (row.Area * Math.Pow(row.Midpoint.Y - centroid.Y, 2.0)))
        let iy = weighted.Fold(0.0, (sum, row) => sum + (row.Area * Math.Pow(row.Midpoint.X - centroid.X, 2.0)))
        let enclosed = Math.Abs(edges.Fold(0.0, static (sum, edge) =>
            sum + ((edge.First.X * edge.Second.Y) - (edge.Second.X * edge.First.Y))) / 2.0)
        let radii = stations.Map(point => point.DistanceTo(new Point3d(centroid.X, centroid.Y, centroid.Z)))
        from radialRatio in radii.Head
            .Map(seed => radii.Fold(seed, Math.Max) / radii.Fold(seed, Math.Min))
            .ToFin(new GeometryFault.DegenerateInput(Kind.Polyline, None, "tube:section-radii").ToError())
        let width = box.Diagonal.X
        let height = box.Diagonal.Y
        let major = Math.Max(width, height)
        let minor = Math.Min(width, height)
        select new SectionProperties(
            area,
            new Point2d(centroid.X, centroid.Y),
            ix,
            iy,
            4.0 * enclosed * enclosed * wallMm / metric.Path.Millimeters,
            ix / Math.Max(Math.Abs(box.Min.Y - centroid.Y), Math.Abs(box.Max.Y - centroid.Y)),
            iy / Math.Max(Math.Abs(box.Min.X - centroid.X), Math.Abs(box.Max.X - centroid.X)),
            metric.Path.Millimeters,
            width,
            height,
            major,
            minor,
            profile.Count,
            profile.Bulges.Count(static bulge => bulge != 0.0),
            radialRatio);
}

[ComplexValueObject]
[ValidationError<FabricationFault>]
public sealed partial class TubeTool {
    public string Key { get; }
    public Set<TubeFormKind> Processes { get; }
    public Set<TubeSectionFamily> Sections { get; }
    public Set<Material> Materials { get; }
    public double MinClrMm { get; }
    public double MaxClrMm { get; }
    public double MinDiameterWallRatio { get; }
    public double MaxDiameterWallRatio { get; }
    public double MinStraightMm { get; }
    public double ClampLengthMm { get; }
    public MandrelKind Mandrel { get; }
    public int BallCount { get; }
    public double MandrelNoseMm { get; }
    public double WiperRakeDeg { get; }
    public double PressureAssistKn { get; }
    public double BoostMm { get; }
    public double CapacityKn { get; }
    public double QualifiedOvality { get; }
    public double QualifiedThinning { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref FabricationFault? validationError,
        ref string key,
        ref Set<TubeFormKind> processes,
        ref Set<TubeSectionFamily> sections,
        ref Set<Material> materials,
        ref double minClrMm,
        ref double maxClrMm,
        ref double minDiameterWallRatio,
        ref double maxDiameterWallRatio,
        ref double minStraightMm,
        ref double clampLengthMm,
        ref MandrelKind mandrel,
        ref int ballCount,
        ref double mandrelNoseMm,
        ref double wiperRakeDeg,
        ref double pressureAssistKn,
        ref double boostMm,
        ref double capacityKn,
        ref double qualifiedOvality,
        ref double qualifiedThinning) =>
        validationError = !string.IsNullOrWhiteSpace(key) && !processes.IsEmpty && !sections.IsEmpty && !materials.IsEmpty && mandrel is not null
            && processes.ForAll(static process => process is not null)
            && sections.ForAll(static section => section is not null)
            && materials.ForAll(static material => material is not null)
            && double.IsFinite(minClrMm) && minClrMm > 0.0 && double.IsFinite(maxClrMm) && maxClrMm >= minClrMm
            && double.IsFinite(minDiameterWallRatio) && minDiameterWallRatio > 0.0
            && double.IsFinite(maxDiameterWallRatio) && maxDiameterWallRatio >= minDiameterWallRatio
            && Seq(minStraightMm, clampLengthMm, capacityKn).ForAll(static value => double.IsFinite(value) && value > 0.0)
            && Seq(mandrelNoseMm, pressureAssistKn, boostMm).ForAll(static value => double.IsFinite(value) && value >= 0.0)
            && double.IsFinite(wiperRakeDeg) && ballCount >= mandrel.MinimumBalls
            && double.IsFinite(qualifiedOvality) && qualifiedOvality is >= 0.0 and < 1.0
            && double.IsFinite(qualifiedThinning) && qualifiedThinning is >= 0.0 and < 1.0
                ? null
                : new FabricationFault.PolicyInadmissible(FabConcern.Forming, "tube:tube-tool");
}

[ComplexValueObject]
[ValidationError<FabricationFault>]
public sealed partial class TubePolicy {
    public Arr<TubeTool> Tools { get; }
    public BendFormat Format { get; }
    public double CollinearAngleDeg { get; }
    public double MinimumSegmentMm { get; }
    public double RootAccuracyDeg { get; }
    public int RootIterations { get; }
    public double MaximumOverbendDeg { get; }
    public double MaximumOvality { get; }
    public double MaximumThinning { get; }
    public double ChordToleranceMm { get; }
    public double CopeAxialSpanMm { get; }
    public int MaximumCopeStations { get; }
    public double WeldSeamExclusionDeg { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref FabricationFault? validationError,
        ref Arr<TubeTool> tools,
        ref BendFormat format,
        ref double collinearAngleDeg,
        ref double minimumSegmentMm,
        ref double rootAccuracyDeg,
        ref int rootIterations,
        ref double maximumOverbendDeg,
        ref double maximumOvality,
        ref double maximumThinning,
        ref double chordToleranceMm,
        ref double copeAxialSpanMm,
        ref int maximumCopeStations,
        ref double weldSeamExclusionDeg) =>
        validationError = !tools.IsEmpty && tools.ForAll(static tool => tool is not null)
            && toSeq(tools.GroupBy(static tool => tool.Key)).ForAll(static group => group.Count() == 1)
            && format is not null && double.IsFinite(collinearAngleDeg) && collinearAngleDeg is >= 0.0 and < 180.0
            && Seq(minimumSegmentMm, maximumOverbendDeg, chordToleranceMm, copeAxialSpanMm)
                .ForAll(static value => double.IsFinite(value) && value > 0.0)
            && double.IsFinite(rootAccuracyDeg) && rootAccuracyDeg is > 0.0 and <= 1.0 && rootIterations > 0
            && double.IsFinite(maximumOvality) && maximumOvality is >= 0.0 and < 1.0
            && double.IsFinite(maximumThinning) && maximumThinning is >= 0.0 and < 1.0
            && maximumCopeStations >= 3
            && double.IsFinite(weldSeamExclusionDeg) && weldSeamExclusionDeg is >= 0.0 and <= 90.0
                ? null
                : new FabricationFault.PolicyInadmissible(FabConcern.Forming, "tube:tube-policy");
}

[ComplexValueObject]
[ValidationError<FabricationFault>]
public sealed partial class TubeRun {
    public Arr<Point3d> Centerline { get; }
    public TubeSection Section { get; }
    public MaterialSpec Material { get; }
    public ProcessBudget.Formed Forming { get; }
    public TubePolicy Policy { get; }
    public Context Tolerance { get; }
    public double ClrMm { get; }
    public double LeadAllowanceMm { get; }
    public double TailAllowanceMm { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref FabricationFault? validationError,
        ref Arr<Point3d> centerline,
        ref TubeSection section,
        ref MaterialSpec material,
        ref ProcessBudget.Formed forming,
        ref TubePolicy policy,
        ref Context tolerance,
        ref double clrMm,
        ref double leadAllowanceMm,
        ref double tailAllowanceMm) =>
        validationError = centerline.Count >= 2 && centerline.ForAll(static point => point.IsValid)
            && section is not null && material is not null && forming is not null && policy is not null && tolerance is not null
            && double.IsFinite(clrMm) && clrMm > 0.0
            && Seq(leadAllowanceMm, tailAllowanceMm).ForAll(static value => double.IsFinite(value) && value >= 0.0)
                ? null
                : new FabricationFault.PolicyInadmissible(FabConcern.Forming, "tube:tube-run");
}

public readonly record struct TubeCoordinate(
    double FeedMm,
    double RotationDeg,
    double CommandDeg,
    double RadiusMm,
    Point3d Vertex,
    Vector3d Incoming,
    Vector3d Outgoing);

[ComplexValueObject]
[ValidationError<FabricationFault>]
public sealed partial class TubeCommand {
    public BendFormat Format { get; }
    public TubeCoordinate Coordinate { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref FabricationFault? validationError,
        ref BendFormat format,
        ref TubeCoordinate coordinate) =>
        validationError = format is not null
            && Seq(coordinate.FeedMm, coordinate.RotationDeg, coordinate.CommandDeg, coordinate.RadiusMm)
                .ForAll(static value => double.IsFinite(value))
            && coordinate.FeedMm >= 0.0 && coordinate.RadiusMm > 0.0
            && coordinate.Vertex.IsValid && coordinate.Incoming.IsValid && coordinate.Outgoing.IsValid
                ? null
                : new FabricationFault.PolicyInadmissible(FabConcern.Forming, "tube:tube-command");
}

public readonly record struct TubeQuality(
    double Ovality,
    double WallThinning,
    double FiberStrain,
    double StrainMargin,
    Option<double> WeldSeamDeg);

public sealed record TubeBend(
    int Index,
    TubeCoordinate Coordinate,
    TubeCommand Command,
    double GeometricBendDeg,
    double NeutralArcMm,
    double ForceKn,
    string ToolKey,
    MandrelKind Mandrel,
    int BallCount,
    double MandrelNoseMm,
    double WiperRakeDeg,
    double PressureAssistKn,
    double BoostMm,
    TubeQuality Quality);

public sealed record TubeProgramReceipt(
    TubeFormKind Process,
    TubeSection Section,
    MaterialSpec Material,
    ProcessBudget.Formed Forming,
    Seq<TubeBend> Bends,
    double TerminalFeedMm,
    double NominalCenterlineMm,
    double DevelopedLengthMm,
    double CutLengthMm,
    ContentKey Key);

[SmartEnum<string>]
public sealed partial class RollAxis {
    public static readonly RollAxis X = new("x", static properties => properties.SxMm3, static properties => properties.HeightMm);
    public static readonly RollAxis Y = new("y", static properties => properties.SyMm3, static properties => properties.WidthMm);

    [UseDelegateFromConstructor]
    public partial double Modulus(SectionProperties properties);

    [UseDelegateFromConstructor]
    public partial double Depth(SectionProperties properties);
}

[SmartEnum<string>]
public sealed partial class RollSectionKind {
    public static readonly RollSectionKind Closed = new("closed", 1.0, static (section, radius, depth) => depth / (2.0 * radius));
    public static readonly RollSectionKind Open = new("open", 1.5, static (section, radius, depth) =>
        depth * depth / (radius * section.GoverningThicknessMm));
    public static readonly RollSectionKind Solid = new("solid", 0.5, static (_, radius, depth) => depth / (4.0 * radius));
    public static readonly RollSectionKind Plate = new("plate", 1.25, static (section, radius, depth) =>
        section.Properties.WidthMm * depth / (radius * section.GoverningThicknessMm));

    public double MinimumRadiusFactor { get; }

    [UseDelegateFromConstructor]
    public partial double Distortion(RollSection section, double radiusMm, double depthMm);
}

[ComplexValueObject]
[ValidationError<FabricationFault>]
public sealed partial class RollSection {
    public string Key { get; }
    public RollSectionKind Kind { get; }
    public Loop Profile { get; }
    public SectionProperties Properties { get; }
    public double GoverningThicknessMm { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref FabricationFault? validationError,
        ref string key,
        ref RollSectionKind kind,
        ref Loop profile,
        ref SectionProperties properties,
        ref double governingThicknessMm) =>
        validationError = !string.IsNullOrWhiteSpace(key) && kind is not null && profile is not null
            && (kind == RollSectionKind.Open ? !profile.Closed : profile.Closed)
            && double.IsFinite(governingThicknessMm) && governingThicknessMm > 0.0
            && Seq(properties.MetalAreaMm2, properties.IxMm4, properties.IyMm4, properties.SxMm3, properties.SyMm3,
                    properties.WidthMm, properties.HeightMm, properties.MajorMm, properties.MinorMm)
                .ForAll(static value => double.IsFinite(value) && value > 0.0)
                ? null
                : new FabricationFault.PolicyInadmissible(FabConcern.Forming, "tube:roll-section");

    public static Fin<RollSection> Admit(
        string key,
        RollSectionKind kind,
        Loop profile,
        SectionProperties properties,
        Length governingThickness) =>
        RollSection.Validate(
            key,
            kind,
            profile,
            properties,
            governingThickness.Millimeters,
            out RollSection section) is { } error
                ? Fin.Fail<RollSection>(new FabricationFault.PolicyInadmissible(FabConcern.Forming, error.Message))
                : Fin.Succ(section);
}

[ComplexValueObject]
[ValidationError<FabricationFault>]
public sealed partial class RollPolicy {
    public double MaximumCurvatureIncrement { get; }
    public int MaximumPasses { get; }
    public double SpringbackFactor { get; }
    public double TorqueSafetyFactor { get; }
    public double GapPerCurvatureMm2 { get; }
    public double MaximumDistortion { get; }
    public double RootRelativeAccuracy { get; }
    public int RootIterations { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref FabricationFault? validationError,
        ref double maximumCurvatureIncrement,
        ref int maximumPasses,
        ref double springbackFactor,
        ref double torqueSafetyFactor,
        ref double gapPerCurvatureMm2,
        ref double maximumDistortion,
        ref double rootRelativeAccuracy,
        ref int rootIterations) =>
        validationError = maximumPasses > 0 && Seq(maximumCurvatureIncrement, torqueSafetyFactor, gapPerCurvatureMm2)
                .ForAll(static value => double.IsFinite(value) && value > 0.0)
            && double.IsFinite(springbackFactor) && springbackFactor >= 0.0
            && double.IsFinite(maximumDistortion) && maximumDistortion > 0.0
            && double.IsFinite(rootRelativeAccuracy) && rootRelativeAccuracy is > 0.0 and <= 1.0
            && rootIterations > 0
                ? null
                : new FabricationFault.PolicyInadmissible(FabConcern.Forming, "tube:roll-policy");
}

[ComplexValueObject]
[ValidationError<FabricationFault>]
public sealed partial class RollRun {
    public RollSection Section { get; }
    public RollAxis Axis { get; }
    public MaterialSpec Material { get; }
    public ProcessBudget.Formed Forming { get; }
    public Length TargetRadius { get; }
    public Angle Sweep { get; }
    public Length WorkpieceWidth { get; }
    public RollPolicy Policy { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref FabricationFault? validationError,
        ref RollSection section,
        ref RollAxis axis,
        ref MaterialSpec material,
        ref ProcessBudget.Formed forming,
        ref Length targetRadius,
        ref Angle sweep,
        ref Length workpieceWidth,
        ref RollPolicy policy) =>
        validationError = section is not null && axis is not null && material is not null && forming is not null && policy is not null
            && double.IsFinite(targetRadius.Millimeters) && targetRadius > Length.Zero
            && double.IsFinite(sweep.Radians) && sweep > Angle.Zero
            && double.IsFinite(workpieceWidth.Millimeters) && workpieceWidth > Length.Zero
                ? null
                : new FabricationFault.PolicyInadmissible(FabConcern.Forming, "tube:roll-run");
}

public sealed record RollPass(
    int Index,
    Option<double> InputRadiusMm,
    double CommandRadiusMm,
    double OutputRadiusMm,
    double GapMm,
    double TorqueNm,
    double SpringbackDeg,
    double Distortion);
public sealed record RollReceipt(
    RollSection Section,
    RollAxis Axis,
    MaterialSpec Material,
    ProcessBudget.Formed Forming,
    Seq<RollPass> Passes,
    double DevelopedLengthMm,
    double MaximumDistortion,
    double TorqueMarginNm,
    ContentKey Key);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CopeSource {
    private CopeSource() { }

    public sealed record Analytic(
        TubeSection Branch,
        TubeSection Main,
        Angle Intersection,
        CopeEnd End,
        TubePolicy Policy,
        Context Tolerance) : CopeSource;
    public sealed record Sectioned(
        SurfaceResult.UvTessellation Part,
        MeshSpace Tool,
        DevelopPolicy Development,
        IntersectPolicy Intersection) : CopeSource;
}

public sealed record CopeProjection(int Crossing, ChartId Chart, Point2d Uv);
public sealed record CopeReceipt(int Crossings, int Segments, Seq<CopeProjection> Projection, Option<DistortionReceipt> Distortion, ContentKey Key);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TubeOp {
    private TubeOp() { }

    public sealed record Form(TubeRun Run, TubeFormKind Kind, ProcessEnvelope.Bender Machine) : TubeOp;
    public sealed record Roll(RollRun Run, ProcessEnvelope.Roll Machine) : TubeOp;
    public sealed record Cope(CopeSource Source) : TubeOp;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TubeResult {
    private TubeResult() { }

    public sealed record Formed(TubeProgramReceipt Program) : TubeResult;
    public sealed record Rolled(RollReceipt Schedule) : TubeResult;
    public sealed record Coped(Seq<Loop> Curves, CopeReceipt Receipt) : TubeResult;
}

```

## [03]-[TUBE_PROGRAM]

- Owner: `TubeProgram` owns every operation dispatch, the bend and roll passes, cope generation, developed-chain projection, and the canonical preimage; the vocabulary cluster above owns the values every arm consumes.
- Law: a cope station's residual is a QUADRATIC in the axial coordinate, so its two branch ends are closed-form roots. A bracketed root-find run twice per station burned the iteration budget the page reserves for the genuinely transcendental elastic-recovery law, and a bracket that failed to straddle silently dropped a station the algebra always answers.
- Law: a chain point resolves to the crossing that PRODUCED it through one station index built per cope on the admitted quantum — exact equality against a rounded lattice station discarded the intersection walk's own provenance and rescanned the lattice once per point.
- Law: a section refuses on zero metal area where the degeneracy is, so no NaN second moment reaches the machine-capacity comparison that reads it as within capacity.
- Exemption: the barycentric solve and the quadratic root pair are bounded numeric kernels — the kernel publishes no barycentric triangle query, so the solve stays local with a scale-relative degeneracy gate.
- Boundary: intersection provenance and atlas provenance stay intact through sectioned cope projection; developed islands carry their chart identity and no arm re-derives a crossing.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------------------------------------------------------
using LanguageExt;
using LanguageExt.Common;
using MathNet.Numerics.RootFinding;
using Rasm.Domain;
using Rasm.Element.Projection;
using Rasm.Fabrication.Kinematics;
using Rasm.Fabrication.Process;
using Rasm.Meshing;
using Rasm.Numerics;
using Rasm.Parametric;
using Rasm.Processing;
using Rhino.Geometry;
using Thinktecture;
using UnitsNet;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Forming;

// --- [OPERATIONS] ---------------------------------------------------------------------------------------------------------------------------------
public static class TubeProgram {
    public static Fin<TubeResult> Apply(TubeOp operation) => operation is null
        ? Fin.Fail<TubeResult>(new FabricationFault.PolicyInadmissible(FabConcern.Forming, "tube:operation"))
        : operation.Switch(
            form: static value => Form(value.Run, value.Kind, value.Machine).Map<TubeResult>(static receipt => new TubeResult.Formed(receipt)),
            roll: static value => Roll(value.Run, value.Machine).Map<TubeResult>(static receipt => new TubeResult.Rolled(receipt)),
            cope: static value => Cope(value.Source).Map(result => (TubeResult)new TubeResult.Coped(result.Curves, result.Receipt)));

    private static Fin<TubeProgramReceipt> Form(TubeRun run, TubeFormKind kind, ProcessEnvelope.Bender machine) =>
        run is null || kind is null || machine is null
            ? Fin.Fail<TubeProgramReceipt>(new FabricationFault.PolicyInadmissible(FabConcern.Forming, "tube:form"))
            : from points in Normalize(run.Centerline, run.Policy, run.Tolerance)
              from bends in toSeq(Enumerable.Range(1, Math.Max(0, points.Count - 2)))
                  .Traverse(index => BendOf(index, points, run, kind).ToValidation()).As().ToFin()
              let requiredDies = bends.Fold(Set<string>(), static (keys, bend) => keys.Add(bend.ToolKey)).Count
              from _machine in ValidMachine(machine, run.ClrMm, requiredDies)
              from receipt in Project(points, bends, run, kind)
              select receipt;

    private static Fin<Arr<Point3d>> Normalize(Arr<Point3d> source, TubePolicy policy, Context tolerance) =>
        toSeq(source.Skip(1)).FoldM<Fin, Seq<Point3d>>(
                Seq(source[0]),
                (held, point) => held.Last
                    .ToFin(new GeometryFault.DegenerateInput(Kind.Polyline, None, "tube:centerline-empty").ToError())
                    .Bind(prior => point.DistanceTo(prior) <= Math.Max(policy.MinimumSegmentMm, tolerance.Absolute.Value)
                        ? Fin.Fail<Seq<Point3d>>(new GeometryFault.DegenerateInput(Kind.Polyline, None, "tube:centerline-zero").ToError())
                        : held.Count < 2
                            ? Fin.Succ(held.Add(point))
                            : from angle in AngleAt(held[^2], held[^1], point)
                              select angle <= policy.CollinearAngleDeg
                                  ? held.Take(held.Count - 1).Add(point)
                                  : held.Add(point)))
            .As()
            .Map(static points => points.ToArr());

    private static Fin<TubeBend> BendOf(
        int index,
        Arr<Point3d> points,
        TubeRun run,
        TubeFormKind kind) {
        Point3d before = points[index - 1];
        Point3d at = points[index];
        Point3d after = points[index + 1];
        Vector3d incoming = at - before;
        Vector3d outgoing = after - at;
        double incomingLength = incoming.Length;
        bool directions = incoming.Unitize() && outgoing.Unitize();
        return from bendDeg in AngleAt(before, at, after)
               from priorBend in index <= 1 ? Fin.Succ(0.0) : AngleAt(points[index - 2], before, at)
               let tangent = run.ClrMm * Math.Tan(Angle.FromDegrees(bendDeg).Radians / 2.0)
               let priorTangent = run.ClrMm * Math.Tan(Angle.FromDegrees(priorBend).Radians / 2.0)
               let feed = incomingLength - priorTangent - tangent + (index == 1 ? run.LeadAllowanceMm : 0.0)
               from rotation in Rotation(points, index)
               from _direction in directions && bendDeg is > 0.0 and < 180.0
                    && run.ClrMm >= run.Forming.MinBendRadiusFactor * run.Section.Properties.MajorMm
                ? Fin.Succ(unit)
                : Fin.Fail<Unit>(FabricationFault.MinBendRadiusViolated(index, run.ClrMm,
                    run.Forming.MinBendRadiusFactor * run.Section.Properties.MajorMm))
               from tool in ToolOf(run, kind, feed)
               from command in Springback(bendDeg, run.ClrMm, run, kind)
               from quality in Quality(run, tool, bendDeg, points, index)
               from _feed in feed >= Math.Max(run.Policy.MinimumSegmentMm,
                       kind.MinimumStraight(tool, run.Policy))
                    ? Fin.Succ(unit)
                    : Fin.Fail<Unit>(new FabricationFault.PolicyInadmissible(FabConcern.Forming, $"tube:straight:{index}:{feed:0.###}"))
               let neutralRadius = run.ClrMm + ((run.Forming.KFactor - 0.5) * run.Section.WallMm)
               let neutralArc = Angle.FromDegrees(bendDeg).Radians * neutralRadius
               let force = Force.FromNewtons(
                   kind.ForceFactor * run.Forming.FlowStressMpa * run.Section.Properties.SxMm3 / run.ClrMm).Kilonewtons
               from _force in force <= tool.CapacityKn
                    ? Fin.Succ(unit)
                    : Fin.Fail<Unit>(FabricationFault.TonnageExceeded(force, tool.CapacityKn))
               let coordinate = new TubeCoordinate(feed, rotation, command, run.ClrMm, at, incoming, outgoing)
               from projected in CommandOf(run.Policy.Format, coordinate)
               select new TubeBend(
                   index,
                   coordinate,
                   projected,
                   bendDeg,
                   neutralArc,
                   force,
                   tool.Key,
                   tool.Mandrel,
                   tool.BallCount,
                   tool.MandrelNoseMm,
                   tool.WiperRakeDeg,
                   tool.PressureAssistKn,
                   tool.BoostMm,
                   quality);
    }

    private static Fin<TubeTool> ToolOf(TubeRun run, TubeFormKind kind, double straightMm) {
        double ratio = run.Section.Properties.MajorMm / run.Section.WallMm;
        return toSeq(run.Policy.Tools
                .Filter(tool => tool.Processes.Contains(kind) && tool.Sections.Contains(run.Section.Family)
                    && tool.Materials.Contains(run.Material.Family)
                    && run.ClrMm >= tool.MinClrMm && run.ClrMm <= tool.MaxClrMm
                    && ratio >= tool.MinDiameterWallRatio && ratio <= tool.MaxDiameterWallRatio
                    && straightMm >= tool.MinStraightMm)
                .OrderBy(static tool => tool.MaxClrMm - tool.MinClrMm)
                .ThenBy(static tool => tool.QualifiedOvality + tool.QualifiedThinning)
                .ThenBy(static tool => tool.Key, StringComparer.Ordinal))
            .Head
            .ToFin(new FabricationFault.PolicyInadmissible(FabConcern.Forming, $"tube:tool:{kind.Key}:{run.ClrMm:0.###}"));
    }

    // The tube instance of the folder's ONE elastic-recovery law: the neutral fibre sits `(k - 0.5)·wall` off the
    // centreline radius and the extreme fibre at half the major dimension, so the elastic index normalizes on the
    // major rather than a thickness. Absence is the bracket refusing to straddle, which this lane raises as a
    // typed refusal because a bend it cannot command is not a candidate the caller retries elsewhere.
    private static Fin<double> Springback(double bendDeg, double clrMm, TubeRun run, TubeFormKind kind) {
        double fibre = (run.Forming.KFactor - 0.5) * run.Section.WallMm;
        return new ElasticLaw(
            Angle.FromDegrees(bendDeg).Radians * (clrMm + fibre),
            fibre,
            kind.RecoveryFactor * run.Forming.SpringbackRatio
                * 2.0 * run.Material.Mechanical.YieldStrengthMpa
                / (run.Material.Mechanical.ElasticModulusMpa * run.Section.Properties.MajorMm))
            .Commanded(bendDeg, run.Policy.MaximumOverbendDeg, run.Policy.RootAccuracyDeg, run.Policy.RootIterations)
            .ToFin(new FabricationFault.PolicyInadmissible(FabConcern.Forming, "tube:springback"));
    }

    private static Fin<TubeQuality> Quality(TubeRun run, TubeTool tool, double bendDeg, Arr<Point3d> points, int index) {
        double curvature = run.Section.Properties.MajorMm / (2.0 * run.ClrMm);
        double wallRatio = run.Section.Properties.MajorMm / run.Section.WallMm;
        double strengthRatio = run.Forming.FlowStressMpa / run.Forming.TensileRm;
        double neutralRadius = run.ClrMm + ((run.Forming.KFactor - 0.5) * run.Section.WallMm);
        double fiberStrain = Math.Log(1.0 + (run.Section.Properties.MajorMm / (2.0 * neutralRadius))) * strengthRatio;
        double support = 1.0 + tool.Mandrel.InteriorSupport(tool, run.Section.Properties.MajorMm)
            + Math.Max(0.0, Math.Cos(Angle.FromDegrees(tool.WiperRakeDeg).Radians))
            + (tool.PressureAssistKn / tool.CapacityKn) + (tool.BoostMm / run.Section.Properties.MajorMm);
        double ovality = curvature * Angle.FromDegrees(bendDeg).Radians * (1.0 + strengthRatio) / support;
        double thinning = fiberStrain * Math.Sqrt(wallRatio) / support;
        return from rotations in toSeq(Enumerable.Range(1, index)).Traverse(bend => Rotation(points, bend).ToValidation()).As().ToFin()
               let weld = run.Section.WeldSeamDeg
            .Map(angle => angle + rotations.Fold(0.0, static (sum, rotation) => sum + rotation))
            .Map(angle => ((angle % 360.0) + 360.0) % 360.0)
               from _ovality in ovality <= Math.Min(run.Policy.MaximumOvality, tool.QualifiedOvality)
                && thinning <= Math.Min(run.Policy.MaximumThinning, tool.QualifiedThinning)
                && fiberStrain <= run.Forming.LimitStrain
                && weld.ForAll(angle => {
                    double seamAxis = Math.Min(angle % 180.0, 180.0 - (angle % 180.0));
                    return seamAxis >= run.Policy.WeldSeamExclusionDeg;
                })
                    ? Fin.Succ(unit)
                    : Fin.Fail<Unit>(new FabricationFault.PolicyInadmissible(FabConcern.Forming, $"tube:quality:{index}:{ovality:0.######}:{thinning:0.######}"))
               select new TubeQuality(ovality, thinning, fiberStrain, run.Forming.LimitStrain - fiberStrain, weld);
    }

    private static Fin<TubeCommand> CommandOf(BendFormat format, TubeCoordinate coordinate) =>
        TubeCommand.Validate(format, format.Project(coordinate), out TubeCommand command) is { } error
            ? Fin.Fail<TubeCommand>(new FabricationFault.PolicyInadmissible(FabConcern.Forming, error.Message))
            : Fin.Succ(command);

    private static Fin<TubeProgramReceipt> Project(
        Arr<Point3d> points,
        Seq<TubeBend> bends,
        TubeRun run,
        TubeFormKind kind) {
        Seq<Point3d> path = toSeq(points);
        double nominal = path.Zip(path.Tail).Fold(0.0, static (sum, edge) => sum + edge.First.DistanceTo(edge.Second));
        double tangent = bends.Fold(0.0, static (sum, bend) => sum + bend.Coordinate.FeedMm);
        double terminal = points.Count < 2
            ? 0.0
            : points[^2].DistanceTo(points[^1])
                - bends.Last.Map(static bend => bend.Coordinate.RadiusMm * Math.Tan(Angle.FromDegrees(bend.GeometricBendDeg).Radians / 2.0)).IfNone(0.0)
                + run.TailAllowanceMm
                + (bends.IsEmpty ? run.LeadAllowanceMm : 0.0);
        if (!double.IsFinite(terminal) || terminal < run.Policy.MinimumSegmentMm)
            return Fin.Fail<TubeProgramReceipt>(new FabricationFault.PolicyInadmissible(FabConcern.Forming, "tube:terminal-feed"));
        double cut = tangent + terminal + bends.Fold(0.0, static (sum, bend) => sum + bend.NeutralArcMm);
        double developed = cut - run.LeadAllowanceMm - run.TailAllowanceMm;
        ContentKey key = ContentKey.Of(EgressKind.BendProgram, Canonical(
            kind,
            run.Section,
            run.Material,
            run.Forming,
            bends,
            terminal,
            nominal,
            developed,
            cut));
        return Fin.Succ(new TubeProgramReceipt(
            kind,
            run.Section,
            run.Material,
            run.Forming,
            bends,
            terminal,
            nominal,
            developed,
            cut,
            key));
    }

    private static Fin<RollReceipt> Roll(RollRun run, ProcessEnvelope.Roll machine) =>
        run is null || machine is null
            ? Fin.Fail<RollReceipt>(new FabricationFault.PolicyInadmissible(FabConcern.Forming, "tube:roll"))
            : from _capacity in Seq(machine.MaxWidth.Millimeters, machine.MinThickness.Millimeters, machine.MaxThickness.Millimeters, machine.Torque.NewtonMeters)
                        .ForAll(static value => double.IsFinite(value) && value > 0.0)
                    && machine.MaxThickness >= machine.MinThickness && machine.Stations >= 3
                        ? Fin.Succ(unit)
                        : Fin.Fail<Unit>(new FabricationFault.PolicyInadmissible(FabConcern.Forming, "tube:roll-machine"))
              let radius = run.TargetRadius.Millimeters
              let depth = run.Axis.Depth(run.Section.Properties)
              from _depth in double.IsFinite(depth) && depth > 0.0
                    ? Fin.Succ(unit)
                    : Fin.Fail<Unit>(new FabricationFault.PolicyInadmissible(FabConcern.Forming, "tube:roll-demand"))
              let targetCurvature = 1.0 / radius
              let requiredPasses = Math.Ceiling(targetCurvature / run.Policy.MaximumCurvatureIncrement)
              from passes in double.IsFinite(requiredPasses)
                    && requiredPasses is >= 1.0 and <= int.MaxValue
                    && requiredPasses <= run.Policy.MaximumPasses
                        ? Fin.Succ((int)requiredPasses)
                        : Fin.Fail<int>(new FabricationFault.PolicyInadmissible(
                            FabConcern.Forming,
                            $"tube:roll-envelope:passes:{requiredPasses:R}"))
              // Torque is a per-pass fact: a pass below the elastic-limit curvature of the governing axis never
              // develops the fully plastic moment, so a schedule-constant torque overstates every early pass.
              let yieldCurvature = 2.0 * run.Material.Mechanical.YieldStrengthMpa
                  / (run.Material.Mechanical.ElasticModulusMpa * depth)
              let plasticTorque = Torque.FromNewtonMeters(
                  run.Forming.FlowStressMpa * run.Axis.Modulus(run.Section.Properties)
                  * run.Policy.TorqueSafetyFactor / Length.FromMeters(1.0).Millimeters).NewtonMeters
              from rows in toSeq(Enumerable.Range(1, passes)).Traverse(index => {
                  double inputCurvature = (index - 1.0) / passes * targetCurvature;
                  double outputCurvature = index / (double)passes * targetCurvature;
                  double recovery = run.Forming.SpringbackRatio * run.Policy.SpringbackFactor;
                  return CommandCurvature(outputCurvature, yieldCurvature, recovery, run.Policy).Map(commandCurvature => new RollPass(
                      index,
                      inputCurvature == 0.0 ? Option<double>.None : Some(1.0 / inputCurvature),
                      1.0 / commandCurvature,
                      1.0 / outputCurvature,
                      commandCurvature * run.Policy.GapPerCurvatureMm2,
                      plasticTorque * Math.Min(1.0, outputCurvature / yieldCurvature),
                      recovery * Degrees(run.Sweep.Radians) / passes,
                      run.Section.Kind.Distortion(run.Section, 1.0 / outputCurvature, depth)));
              }).As()
              let peakTorque = rows.Map(static row => row.TorqueNm).Fold(0.0, Math.Max)
              let maximumDistortion = rows.Map(static row => row.Distortion).Fold(0.0, Math.Max)
              from _width in run.WorkpieceWidth <= machine.MaxWidth
                    && run.Section.GoverningThicknessMm >= machine.MinThickness.Millimeters
                    && run.Section.GoverningThicknessMm <= machine.MaxThickness.Millimeters
                    && radius >= run.Section.Kind.MinimumRadiusFactor * run.Forming.MinBendRadiusFactor * depth
                    && peakTorque <= machine.Torque.NewtonMeters
                    ? Fin.Succ(unit)
                    : Fin.Fail<Unit>(new FabricationFault.PolicyInadmissible(FabConcern.Forming,
                        $"tube:roll-envelope:{peakTorque:0.###}:{machine.Torque.NewtonMeters:0.###}"))
              from _distortion in double.IsFinite(maximumDistortion) && maximumDistortion <= run.Policy.MaximumDistortion
                    ? Fin.Succ(unit)
                    : Fin.Fail<Unit>(new FabricationFault.PolicyInadmissible(FabConcern.Forming, $"tube:roll-distortion:{maximumDistortion:0.######}"))
              let developed = radius * run.Sweep.Radians
              select new RollReceipt(
                  run.Section,
                  run.Axis,
                  run.Material,
                  run.Forming,
                  rows,
                  developed,
                  maximumDistortion,
                  machine.Torque.NewtonMeters - peakTorque,
                  ContentKey.Of(EgressKind.BendProgram, Canonical(
                      run.Section,
                      run.Axis,
                      run.Material,
                      run.Forming,
                      rows,
                      developed,
                      maximumDistortion,
                      machine.Torque.NewtonMeters - peakTorque)));

    private static Fin<double> CommandCurvature(
        double outputCurvature,
        double yieldCurvature,
        double recovery,
        RollPolicy policy) {
        if (recovery == 0.0)
            return Fin.Succ(outputCurvature);
        double upper = outputCurvature * (1.0 + recovery) + yieldCurvature;
        return Brent.TryFindRoot(
            command => command / (1.0 + (recovery * Math.Min(1.0, command / yieldCurvature))) - outputCurvature,
            outputCurvature,
            upper,
            outputCurvature * policy.RootRelativeAccuracy,
            policy.RootIterations,
            out double command)
                ? Fin.Succ(command)
                : Fin.Fail<double>(new FabricationFault.PolicyInadmissible(FabConcern.Forming, "tube:roll-recovery-root"));
    }

    private static Fin<(Seq<Loop> Curves, CopeReceipt Receipt)> Cope(CopeSource source) => source is null
        ? Fin.Fail<(Seq<Loop>, CopeReceipt)>(new FabricationFault.PolicyInadmissible(FabConcern.Forming, "tube:cope"))
        : source.Switch(
            analytic: static value => AnalyticCope(value),
            sectioned: static value => SectionedCope(value));

    private static Fin<(Seq<Loop> Curves, CopeReceipt Receipt)> AnalyticCope(CopeSource.Analytic source) {
        if (source.Branch is null || source.Main is null || source.End is null || source.Policy is null || source.Tolerance is null
            || !source.Branch.Family.AnalyticCope || !source.Main.Family.AnalyticCope
            || source.Intersection.Radians is <= 0.0 || source.Intersection.Radians >= Math.PI)
            return Fin.Fail<(Seq<Loop>, CopeReceipt)>(new FabricationFault.PolicyInadmissible(FabConcern.Forming, "tube:cope-analytic"));
        double branchRadius = (source.Branch.Properties.MajorMm + source.Branch.WallMm) / 2.0;
        double mainRadius = (source.Main.Properties.MajorMm + source.Main.WallMm) / 2.0;
        int samples = Math.Max(3, (int)Math.Ceiling(2.0 * Math.PI * branchRadius / source.Policy.ChordToleranceMm));
        if (samples > source.Policy.MaximumCopeStations)
            return Fin.Fail<(Seq<Loop>, CopeReceipt)>(new FabricationFault.PolicyInadmissible(FabConcern.Forming, "tube:cope-stations"));
        Fin<Seq<Point3d>> solved = toSeq(Enumerable.Range(0, samples)).Traverse(index => {
            double theta = 2.0 * Math.PI * index / samples;
            double x = branchRadius * Math.Cos(theta);
            double y = branchRadius * Math.Sin(theta);
            double alpha = source.Intersection.Radians;
            // The cope residual is a QUADRATIC in z: expanding the axial projection leaves
            // (1 - cos^2)z^2 - 2(x sin)(cos)z + (x^2 + y^2 - (x sin)^2 - R^2), so its two roots are the two branch
            // ends in closed form. Running a bracketed root-find twice per station over a quadratic burned the
            // iteration budget the page's own criterion reserves for the genuinely transcendental springback law,
            // and a bracket that failed to straddle silently dropped a station the algebra always answers.
            double sin = Math.Sin(alpha), cos = Math.Cos(alpha);
            double quadratic = 1.0 - (cos * cos);
            double linear = -2.0 * x * sin * cos;
            double constant = (x * x) + (y * y) - (x * sin * x * sin) - (mainRadius * mainRadius);
            (bool Lower, double LowerZ, bool Upper, double UpperZ) roots =
                Roots(quadratic, linear, constant, source.Policy.CopeAxialSpanMm);
            return source.End.Select(index, roots.Lower, roots.LowerZ, roots.Upper, roots.UpperZ)
                .Map(z => new Point3d(branchRadius * theta, z, 0.0))
                .ToValidation();
        }).As().ToFin();
        return from points in solved
               from loop in Loop.Admit(points.ToArr(), closed: true, Arr<double>(), source.Tolerance)
               let key = ContentKey.Of(EgressKind.FlatPattern, Canonical(Seq(loop), Seq<CopeProjection>(), None, source.Tolerance))
               select (Seq(loop), new CopeReceipt(samples, samples, Seq<CopeProjection>(), None, key));
    }

    private static Fin<(Seq<Loop> Curves, CopeReceipt Receipt)> SectionedCope(CopeSource.Sectioned source) =>
        from intersection in Intersection.Apply(new IntersectOp.MeshMesh(source.Part.Mesh, source.Tool, source.Intersection))
        from chains in intersection is IntersectResult.Chains crossed
            ? Fin.Succ(crossed)
            : Fin.Fail<IntersectResult.Chains>(new FabricationFault.PolicyInadmissible(FabConcern.Forming, "tube:cope-intersection"))
        from development in Development.Apply(new DevelopOp.Unroll(source.Part, source.Development))
        from unrolled in development is DevelopmentResult.Unrolled value
            ? Fin.Succ(value)
            : Fin.Fail<DevelopmentResult.Unrolled>(new FabricationFault.PolicyInadmissible(FabConcern.Forming, "tube:cope-development"))
        from edges in ProjectEdges(chains.Lattice, unrolled.Atlas)
        let stations = Stations(chains.Lattice, source.Part.Mesh.Tolerance.Absolute.Value)
        from developed in chains.Walked
            .Traverse(chain => DevelopedChain(chain, stations, edges, source.Part.Mesh.Tolerance).ToValidation()).As().ToFin()
        let loops = developed.Bind(static run => run)
        let projected = edges.Bind(static edge => Seq(edge.A, edge.B))
        let distortion = Some(unrolled.Atlas.Receipt)
        let key = ContentKey.Of(EgressKind.FlatPattern, Canonical(loops, projected, distortion, source.Part.Mesh.Tolerance))
        select (
            loops,
            new CopeReceipt(
                chains.Lattice.Rows.Length,
                chains.Lattice.Segments.Length + chains.Lattice.Coplanar.Length,
                projected,
                distortion,
                key));

    private sealed record DevelopedEdge(int CrossingA, int CrossingB, CopeProjection A, CopeProjection B);
    private sealed record DevelopedRun(ChartId Chart, Seq<Point2d> Points);

    private static Fin<Seq<DevelopedEdge>> ProjectEdges(CrossLattice lattice, ChartAtlas atlas) {
        Mesh mesh = atlas.Source.DuplicateNative();
        return toSeq(lattice.Segments).Traverse(row => (
            from a in CrossUv(row.A, lattice.Rows[row.A], row.FaceA, mesh, atlas.Source.Tolerance, atlas.Islands)
            from b in CrossUv(row.B, lattice.Rows[row.B], row.FaceA, mesh, atlas.Source.Tolerance, atlas.Islands)
            from _chart in a.Chart == b.Chart
                ? Fin.Succ(unit)
                : Fin.Fail<Unit>(new GeometryFault.DegenerateInput(Kind.Mesh, None, $"tube:cope-edge-chart:{row.A}:{row.B}").ToError())
            select new DevelopedEdge(row.A, row.B, a, b)).ToValidation()).As().ToFin();
    }

    private static Fin<CopeProjection> CrossUv(
        int index,
        Crossing crossing,
        int partFace,
        Mesh mesh,
        Context tolerance,
        Seq<UvIsland> islands) {
        Point3d point = crossing.Point.Round();
        CrossKey key = crossing.Key;
        if (key.Side == 0 && key.EdgeU >= 0 && key.EdgeU == key.EdgeV)
            return from island in Island(islands, mesh.Faces[partFace], Seq(key.EdgeU))
                   from uv in Uv(island, key.EdgeU)
                   select new CopeProjection(index, island.Chart, uv);
        if (key.Side == 0 && key.EdgeU >= 0 && key.EdgeV >= 0)
            return EdgeUv(index, point, key.EdgeU, key.EdgeV, partFace, mesh, islands);
        if (key.Side == 1)
            return FaceUv(index, point, partFace, mesh, tolerance.Absolute.Value, islands);
        return Fin.Fail<CopeProjection>(new GeometryFault.DegenerateInput(Kind.Mesh, index, $"tube:cope-key:{index}").ToError());
    }

    private static Fin<CopeProjection> EdgeUv(
        int index,
        Point3d point,
        int edgeU,
        int edgeV,
        int faceIndex,
        Mesh mesh,
        Seq<UvIsland> islands) {
        Point3d a = mesh.Vertices.Point3dAt(edgeU);
        Point3d b = mesh.Vertices.Point3dAt(edgeV);
        double t = new Line(a, b).ClosestParameter(point);
        return from island in Island(islands, mesh.Faces[faceIndex], Seq(edgeU, edgeV))
               from uvA in Uv(island, edgeU)
               from uvB in Uv(island, edgeV)
               select new CopeProjection(index, island.Chart, Lerp(uvA, uvB, t));
    }

    private static Fin<CopeProjection> FaceUv(
        int index,
        Point3d point,
        int faceIndex,
        Mesh mesh,
        double toleranceMm,
        Seq<UvIsland> islands) {
        MeshFace face = mesh.Faces[faceIndex];
        double scale = Seq(face.A, face.B, face.C, face.IsQuad ? face.D : face.C)
            .Map(vertex => mesh.Vertices.Point3dAt(vertex).DistanceTo(point))
            .Fold(0.0, static (largest, distance) => Math.Max(largest, distance));
        double weightTolerance = toleranceMm / Math.Max(scale, toleranceMm);
        Seq<int> source = face.IsQuad
            ? Seq(face.A, face.B, face.C, face.D)
            : Seq(face.A, face.B, face.C);
        Seq<(UvIsland Island, int A, int B, int C)> triangles = islands.Bind(island => island.Faces
            .Filter(triangle => Seq(triangle.A, triangle.B, triangle.C).ForAll(source.Contains))
            .Map(triangle => (island, triangle.A, triangle.B, triangle.C)));
        return from candidates in triangles.Traverse(triangle => Barycentric(
                    point,
                    mesh.Vertices.Point3dAt(triangle.A),
                    mesh.Vertices.Point3dAt(triangle.B),
                    mesh.Vertices.Point3dAt(triangle.C))
                .Map(weights => (Triangle: triangle, Weights: weights))
                .ToValidation()).As().ToFin()
               from chosen in candidates
                   .Filter(row => row.Weights.A >= -weightTolerance
                       && row.Weights.B >= -weightTolerance
                       && row.Weights.C >= -weightTolerance)
                   .Head
                   .ToFin(new GeometryFault.DegenerateInput(Kind.Mesh, index, $"tube:cope-face:{index}").ToError())
               from uvA in Uv(chosen.Triangle.Island, chosen.Triangle.A)
               from uvB in Uv(chosen.Triangle.Island, chosen.Triangle.B)
               from uvC in Uv(chosen.Triangle.Island, chosen.Triangle.C)
               select new CopeProjection(
                   index,
                   chosen.Triangle.Island.Chart,
                   (chosen.Weights.A * uvA) + (chosen.Weights.B * uvB) + (chosen.Weights.C * uvC));
    }

    private static Fin<Seq<Loop>> DevelopedChain(
        Chain chain,
        Map<(long X, long Y, long Z), int> stations,
        Seq<DevelopedEdge> projected,
        Context tolerance) =>
        from segments in chain.Points.ToSeq().Zip(chain.Points.ToSeq().Skip(1))
            .Traverse(pair => (
                from crossingA in CrossingAt(pair.First, stations, tolerance.Absolute.Value)
                from crossingB in CrossingAt(pair.Second, stations, tolerance.Absolute.Value)
                from edge in projected
                    .Find(row => row.CrossingA == crossingA && row.CrossingB == crossingB
                        || row.CrossingA == crossingB && row.CrossingB == crossingA)
                    .ToFin(new GeometryFault.DegenerateInput(Kind.Mesh, None, $"tube:cope-chain-edge:{crossingA}:{crossingB}").ToError())
                select edge.CrossingA == crossingA
                    ? new DevelopedRun(edge.A.Chart, Seq(edge.A.Uv, edge.B.Uv))
                    : new DevelopedRun(edge.B.Chart, Seq(edge.B.Uv, edge.A.Uv))).ToValidation()).As().ToFin()
        let runs = segments.Fold(Seq<DevelopedRun>(), (held, segment) => held.Last
            .Filter(prior => prior.Chart == segment.Chart)
            .Filter(prior => (prior.Points.Last, segment.Points.Head)
                .Apply((end, start) => Near(end, start, tolerance.Absolute.Value)).As().IfNone(false))
            .Bind(prior => segment.Points.Last.Map(last => prior with { Points = prior.Points.Add(last) }))
            .Match(
                Some: joined => held.Take(held.Count - 1).Add(joined),
                None: () => held.Add(segment)))
        from loops in runs.Traverse(run => Loop.Admit(
            run.Points.ToArr(),
            chain.Closed && runs.Count == 1,
            Arr<double>(),
            tolerance).ToValidation()).As().ToFin()
        select loops;

    // A chain point carries the crossing it CAME from: matching it back by exact equality against a ROUNDED
    // lattice station discarded that provenance and answered "no crossing" wherever the round moved a coordinate,
    // while a linear rescan ran once per chain point. The station index is built ONCE per cope on the admitted
    // quantum, so the lookup is a map read and every chain point resolves to the crossing that produced it.
    private static Map<(long X, long Y, long Z), int> Stations(CrossLattice lattice, double quantum) =>
        toSeq(lattice.Rows)
            .Map((crossing, index) => (Station(crossing.Point.Round(), quantum), index))
            .Fold(Map<(long, long, long), int>(), static (index, row) => index.AddOrUpdate(row.Item1, row.index));

    private static Fin<int> CrossingAt(Point3d point, Map<(long X, long Y, long Z), int> stations, double quantum) =>
        stations
            .Find(Station(point, quantum))
            .ToFin(new GeometryFault.DegenerateInput(Kind.Mesh, None, "tube:cope-chain-provenance").ToError());

    internal static (long X, long Y, long Z) Station(Point3d point, double quantum) => (
        (long)Math.Round(point.X / quantum, MidpointRounding.ToEven),
        (long)Math.Round(point.Y / quantum, MidpointRounding.ToEven),
        (long)Math.Round(point.Z / quantum, MidpointRounding.ToEven));

    // Exemption: the two branch ends of a cope station are the roots of one quadratic; the degenerate arm is the
    // linear case the grazing intersection leaves.
    private static (bool Lower, double LowerZ, bool Upper, double UpperZ) Roots(
        double quadratic,
        double linear,
        double constant,
        double spanMm) {
        if (Math.Abs(quadratic) <= EpsilonPolicy.ZeroTolerance) {
            if (Math.Abs(linear) <= EpsilonPolicy.ZeroTolerance) return (false, 0.0, false, 0.0);
            double only = -constant / linear;
            bool inside = Math.Abs(only) <= spanMm;
            return (inside && only <= 0.0, only, inside && only >= 0.0, only);
        }
        double discriminant = (linear * linear) - (4.0 * quadratic * constant);
        if (discriminant < 0.0) return (false, 0.0, false, 0.0);
        double root = Math.Sqrt(discriminant);
        double first = (-linear - root) / (2.0 * quadratic);
        double second = (-linear + root) / (2.0 * quadratic);
        double lower = Math.Min(first, second), upper = Math.Max(first, second);
        return (Math.Abs(lower) <= spanMm, lower, Math.Abs(upper) <= spanMm, upper);
    }

    private static bool Near(Point2d a, Point2d b, double toleranceMm) =>
        Math.Sqrt(Math.Pow(a.X - b.X, 2.0) + Math.Pow(a.Y - b.Y, 2.0)) <= toleranceMm;

    private static Fin<UvIsland> Island(Seq<UvIsland> islands, MeshFace sourceFace, Seq<int> vertices) {
        Seq<int> source = sourceFace.IsQuad
            ? Seq(sourceFace.A, sourceFace.B, sourceFace.C, sourceFace.D)
            : Seq(sourceFace.A, sourceFace.B, sourceFace.C);
        return islands.Find(island => vertices.ForAll(island.Vertices.Contains)
                && island.Faces.Exists(face => Seq(face.A, face.B, face.C).ForAll(source.Contains)))
            .ToFin(new GeometryFault.DegenerateInput(Kind.Mesh, None, "tube:cope-chart").ToError());
    }

    private static Fin<Point2d> Uv(UvIsland island, int vertex) =>
        toSeq(island.Vertices).Map((value, index) => (Vertex: value, Index: index))
            .Find(row => row.Vertex == vertex)
            .Map(row => island.Uv[row.Index])
            .ToFin(new GeometryFault.DegenerateInput(Kind.Mesh, vertex, $"tube:cope-vertex:{vertex}").ToError());

    private static Point2d Lerp(Point2d a, Point2d b, double t) => ((1.0 - t) * a) + (t * b);

    // Exemption: the barycentric solve is a bounded numeric kernel. `Rasm.Meshing` publishes no barycentric query
    // — the `TetInterpolation` receipt it carries is a tetrahedral reconstruction column, not a triangle solve — so
    // this stays local, and the degeneracy gate is RELATIVE to the triangle's own Gram determinant scale rather
    // than an exact-zero test that admits a slivered face as invertible.
    private static Fin<(double A, double B, double C)> Barycentric(Point3d point, Point3d a, Point3d b, Point3d c) {
        Vector3d v0 = b - a, v1 = c - a, v2 = point - a;
        double d00 = v0 * v0, d01 = v0 * v1, d11 = v1 * v1, d20 = v2 * v0, d21 = v2 * v1;
        double denominator = (d00 * d11) - (d01 * d01);
        if (!double.IsFinite(denominator) || Math.Abs(denominator) <= EpsilonPolicy.SqrtEpsilon * Math.Max(d00 * d11, 1.0))
            return Fin.Fail<(double, double, double)>(
                new GeometryFault.DegenerateInput(Kind.Mesh, None, "tube:cope-barycentric").ToError());
        double bWeight = ((d11 * d20) - (d01 * d21)) / denominator;
        double cWeight = ((d00 * d21) - (d01 * d20)) / denominator;
        return Fin.Succ((1.0 - bWeight - cWeight, bWeight, cWeight));
    }

    private static Fin<double> Rotation(Arr<Point3d> points, int index) {
        if (index <= 1)
            return Fin.Succ(0.0);
        Vector3d prior = Vector3d.CrossProduct(points[index - 1] - points[index - 2], points[index] - points[index - 1]);
        Vector3d next = Vector3d.CrossProduct(points[index] - points[index - 1], points[index + 1] - points[index]);
        Vector3d axis = points[index] - points[index - 1];
        return prior.Unitize() && next.Unitize() && axis.Unitize()
            ? Fin.Succ(Degrees(Math.Atan2(Vector3d.CrossProduct(prior, next) * axis, prior * next)))
            : Fin.Fail<double>(new GeometryFault.DegenerateInput(Kind.Polyline, index, $"tube:rotation:{index}").ToError());
    }

    private static Fin<double> AngleAt(Point3d before, Point3d at, Point3d after) {
        Vector3d incoming = at - before;
        Vector3d outgoing = after - at;
        return incoming.Unitize() && outgoing.Unitize()
            ? Fin.Succ(Degrees(Math.Acos(Math.Clamp(incoming * outgoing, -1.0, 1.0))))
            : Fin.Fail<double>(new GeometryFault.DegenerateInput(Kind.Polyline, None, "tube:centerline-angle").ToError());
    }

    private static double Degrees(double radians) => radians / Angle.FromDegrees(1.0).Radians;

    private static Fin<Unit> ValidMachine(ProcessEnvelope.Bender machine, double clrMm, int requiredDies) =>
        double.IsFinite(machine.MinClr.Millimeters) && machine.MinClr > Length.Zero
            && double.IsFinite(machine.MaxClr.Millimeters) && machine.MaxClr >= machine.MinClr
            && machine.DieCount >= 0 && requiredDies >= 0
            && (requiredDies == 0 || machine.DieCount >= requiredDies && clrMm >= machine.MinClr.Millimeters && clrMm <= machine.MaxClr.Millimeters)
                ? Fin.Succ(unit)
                : Fin.Fail<Unit>(new FabricationFault.PolicyInadmissible(FabConcern.Forming, "tube:bender-envelope"));

    private static byte[] Canonical(
        TubeFormKind process,
        TubeSection section,
        MaterialSpec material,
        ProcessBudget.Formed forming,
        Seq<TubeBend> bends,
        double terminalFeedMm,
        double nominalCenterlineMm,
        double developedLengthMm,
        double cutLengthMm) {
        // Every preimage on this page composes the ONE `Rasm.Element` `CanonicalWriter` through the S0
        // `FabricationCanon` family and `Loop.CanonicalBytes`, so a bend program and a flat pattern keyed here
        // address byte-identically with the same artifact keyed at any sibling page.
        CanonicalWriter writer = new(section.Profile.Tolerance.Absolute.Value);
        _ = Write(section.Profile
                .CanonicalBytes(writer
                    .Discriminant(process).Discriminant(section.Family).Double(section.WallMm)
                    .Maybe(section.WeldSeamDeg, static (target, value) => target.Double(value))),
                section.Properties)
            .Discriminant(material.Family).String(material.Identity.Grade)
            .Double(forming.KFactor).Double(forming.TensileRm).Double(forming.SpringbackRatio)
            .Double(forming.MinBendRadiusFactor).Double(forming.FlowStressMpa).Double(forming.LimitStrain)
            .Budget(forming.Evidence)
            .Rows(bends, static (target, bend) => Write(Write(
                    target.Ordinal(bend.Index).Discriminant(bend.Command.Format),
                    bend.Command.Coordinate),
                    bend.Coordinate)
                .Double(bend.GeometricBendDeg).Double(bend.NeutralArcMm)
                .String(bend.ToolKey).Discriminant(bend.Mandrel).Ordinal(bend.BallCount)
                .Double(bend.MandrelNoseMm).Double(bend.WiperRakeDeg).Double(bend.PressureAssistKn)
                .Double(bend.BoostMm).Double(bend.ForceKn)
                .Double(bend.Quality.Ovality).Double(bend.Quality.WallThinning)
                .Double(bend.Quality.FiberStrain).Double(bend.Quality.StrainMargin)
                .Maybe(bend.Quality.WeldSeamDeg, static (slot, value) => slot.Double(value)))
            .Double(terminalFeedMm).Double(nominalCenterlineMm)
            .Double(developedLengthMm).Double(cutLengthMm);
        return writer.ToBytes().ToArray();
    }

    private static byte[] Canonical(
        RollSection section,
        RollAxis axis,
        MaterialSpec material,
        ProcessBudget.Formed forming,
        Seq<RollPass> passes,
        double developedLengthMm,
        double maximumDistortion,
        double torqueMarginNm) {
        CanonicalWriter writer = new(section.Profile.Tolerance.Absolute.Value);
        _ = Write(section.Profile
                .CanonicalBytes(writer
                    .String(section.Key).Discriminant(section.Kind).Discriminant(axis)
                    .Double(section.GoverningThicknessMm)),
                section.Properties)
            .Discriminant(material.Family).String(material.Identity.Grade)
            .Double(forming.FlowStressMpa).Double(forming.TensileRm).Double(forming.KFactor)
            .Double(forming.SpringbackRatio).Double(forming.MinBendRadiusFactor).Double(forming.LimitStrain)
            .Budget(forming.Evidence)
            .Rows(passes, static (target, pass) => target
                .Ordinal(pass.Index)
                .Maybe(pass.InputRadiusMm, static (slot, value) => slot.Double(value))
                .Double(pass.CommandRadiusMm).Double(pass.OutputRadiusMm).Double(pass.GapMm)
                .Double(pass.TorqueNm).Double(pass.SpringbackDeg).Double(pass.Distortion))
            .Double(developedLengthMm).Double(maximumDistortion).Double(torqueMarginNm);
        return writer.ToBytes().ToArray();
    }

    // The cope preimage takes its grid as an argument because a cope carries no section of its own; both callers
    // already hold the admitting `Context`, so the writer is never opened on a fabricated tolerance.
    private static byte[] Canonical(
        Seq<Loop> loops,
        Seq<CopeProjection> projection,
        Option<DistortionReceipt> distortion,
        Context tolerance) {
        CanonicalWriter writer = new(tolerance.Absolute.Value);
        _ = writer
            .Rows(loops, static (target, loop) => loop.CanonicalBytes(target))
            .Rows(projection, static (target, row) => target
                .Ordinal(row.Crossing).Ordinal(row.Chart.Value).Double(row.Uv.X).Double(row.Uv.Y))
            .Maybe(distortion, static (target, row) => target
                .Double(row.MaxConformal).Double(row.MeanConformal)
                .Double(row.MaxArea).Double(row.MinArea).Double(row.MeanArea)
                .Double(row.MaxQuasiConformal).Ordinal(row.Iterations).Double(row.Residual)
                .Ordinal(row.FactorNonZeros).Bool(row.FlipFreeBijective));
        return writer.ToBytes().ToArray();
    }

    // The two vocabularies this page OWNS carry the only writers it declares; points, vectors, loops, optional
    // slots, row counts, and discriminants all frame at the S0 owner.
    private static CanonicalWriter Write(CanonicalWriter writer, TubeCoordinate coordinate) => writer
        .Double(coordinate.FeedMm).Double(coordinate.RotationDeg)
        .Double(coordinate.CommandDeg).Double(coordinate.RadiusMm)
        .Coords(coordinate.Vertex).Coords(coordinate.Incoming).Coords(coordinate.Outgoing);

    private static CanonicalWriter Write(CanonicalWriter writer, SectionProperties properties) => writer
        .Double(properties.MetalAreaMm2).Double(properties.Centroid.X).Double(properties.Centroid.Y)
        .Double(properties.IxMm4).Double(properties.IyMm4).Double(properties.JMm4)
        .Double(properties.SxMm3).Double(properties.SyMm3).Double(properties.PerimeterMm)
        .Double(properties.WidthMm).Double(properties.HeightMm)
        .Double(properties.MajorMm).Double(properties.MinorMm)
        .Ordinal(properties.VertexCount).Ordinal(properties.CurvedEdges).Double(properties.RadialRatio);
}
```

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
