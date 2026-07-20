# [RASM_FABRICATION_TUBE_PROGRAM]

`TubeProgram` owns one tube-forming algebra across discrete bending, axis-specific section roll curving, and cope projection. `TubeSection`, `RollSection`, `TubeTool`, and `TubePolicy` admit section mechanics, material, weld seam, tooling, deformation limits, numeric tolerances, and egress policy once.

`TubeProgram.Apply` composes the frozen `ProcessEnvelope.Bender`, `ProcessEnvelope.Roll`, `ProcessBudget.Formed`, `Intersection.Apply`, `Development.Apply`, `UvIsland`, and `ContentKey.Of` wires. Intersection provenance and atlas provenance remain intact through sectioned cope projection.

## [01]-[INDEX]

- [01]-[TUBE_FORMING]: Generated process and format families, section mechanics, tooling admission, neutral-axis programs, multi-pass roll schedules, internalized cope provenance, and content-keyed results.

## [02]-[TUBE_FORMING]

- Owner: `TubeFormKind` owns discrete process physics; `BendFormat` owns command projection; `CopeEnd` owns analytic branch-end selection; `TubeSection` owns closed thin-wall mechanics; `RollSection` and `RollAxis` own closed, open, solid, and plate roll mechanics; `TubeTool` owns tooling evidence; `TubeProgram` owns all operation dispatch and projection.
- Cases: `TubeOp` carries `Form`, `Roll`, and `Cope`; `TubeResult` mirrors those modalities; `TubeCommand` binds one canonical `TubeCoordinate` to a `BendFormat` projection row; `TubeFormKind` carries rotary-draw, compression, ram, push, and freeform behavior; `CopeEnd` selects the negative or positive analytic root; `MandrelKind` carries the tooling axis.
- Entry: `TubeProgram.Apply(TubeOp)` is the one polymorphic entry for every modality.
- Auto: Centerlines normalize once, tooling resolves per bend, neutral-axis length consumes the forming budget, `Brent.TryFindRoot` inverts the cubic elastic-recovery law over the loaded radius for both bend springback and pass torque, mandrel rows supply their own interior wall support, weld-seam rotation propagates, roll passes generate command curvature with axis modulus and distortion gates, and sectioned cope lowers exact crossing keys through source vertices or source faces into developed islands.
- Receipt: Form results carry the canonical coordinate beside every projected command, force, tooling position, deformation witness, terminal feed, nominal centerline, developed body, cut length, and key; roll results carry input, commanded, and recovered radius, axis, distortion, and machine margin; cope results carry chart-coherent developed runs and defining-face crossing wedges.
- Packages: `LanguageExt.Core`, `Thinktecture.Runtime.Extensions`, `CommunityToolkit.HighPerformance`, `MathNet.Numerics`, `UnitsNet`, `RhinoCommon`, `Rasm.Meshing`, `Rasm.Parametric`, `Rasm.Processing`, and `ContentKey` compose the surface.
- Growth: A discrete process is one `TubeFormKind` row, a command convention is one `BendFormat` row, a physical tool is one catalog row, an analytic branch end is one `CopeEnd` row, a roll target is data, and a new modality is one `TubeOp`/`TubeResult` case pair.
- Boundary: Forming owns tube mechanics and projection; machine capacity, process material physics, exact intersection, development, planar loop admission, posting text, and content identity remain at their canonical owners.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------------------------------------------------------
using System.Buffers.Binary;
using CommunityToolkit.HighPerformance.Buffers;
using LanguageExt;
using LanguageExt.Common;
using MathNet.Numerics.RootFinding;
using Rasm.Domain;
using Rasm.Fabrication.Geometry2D;
using Rasm.Fabrication.Kinematics;
using Rasm.Fabrication.Process;
using Rasm.Meshing;
using Rasm.Parametric;
using Rasm.Processing;
using Rhino.Geometry;
using Thinktecture;
using UnitsNet;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Forming;

// --- [TYPES] --------------------------------------------------------------------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class TubeFormKind {
    public static readonly TubeFormKind RotaryDraw = new("rotary-draw", forceFactor: 1.0, recoveryFactor: 1.0, static (tool, _) => tool.ClampLengthMm);
    public static readonly TubeFormKind Compression = new("compression", forceFactor: 1.35, recoveryFactor: 1.15, static (tool, _) => tool.ClampLengthMm);
    public static readonly TubeFormKind Ram = new("ram", forceFactor: 1.8, recoveryFactor: 1.35, static (tool, _) => tool.MinStraightMm);
    public static readonly TubeFormKind Push = new("push", forceFactor: 0.8, recoveryFactor: 0.9, static (_, policy) => policy.MinimumSegmentMm);
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
        : Fin.Fail<double>(new GeometryFault.DegenerateInput(Kind.Brep, -1, $"tube:cope-root:{index}:negative").ToError()));
    public static readonly CopeEnd Positive = new("positive", static (index, _, _, upper, upperZ) => upper
        ? Fin.Succ(upperZ)
        : Fin.Fail<double>(new GeometryFault.DegenerateInput(Kind.Brep, -1, $"tube:cope-root:{index}:positive").ToError()));

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
public sealed partial class TubeSection {
    public TubeSectionFamily Family { get; }
    public Loop Profile { get; }
    public double WallMm { get; }
    public Option<double> WeldSeamDeg { get; }
    public SectionProperties Properties { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
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
                : new ValidationError(message: "Tube section must carry a closed profile, wall, weld seam, and positive derived mechanics.");

    public static Fin<TubeSection> Admit(
        TubeSectionFamily family,
        Loop profile,
        Length wall,
        Option<Angle> weldSeam,
        Length chordTolerance,
        int maximumStations) =>
        family is null || profile is null
            ? Fin.Fail<TubeSection>(new GeometryFault.DegenerateInput(Kind.Brep, -1, "tube:section").ToError())
            : from properties in Mechanics(profile, wall.Millimeters, chordTolerance.Millimeters, maximumStations)
              from section in TubeSection.Validate(
                  family,
                  profile,
                  wall.Millimeters,
                  weldSeam.Map(static angle => angle.Radians / Angle.FromDegrees(1.0).Radians),
                  properties,
                  out TubeSection admitted) is { } error
                    ? Fin.Fail<TubeSection>(new GeometryFault.DegenerateInput(Kind.Brep, -1, error.Message).ToError())
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
            : Fin.Fail<ProfileResult.Measure>(new GeometryFault.DegenerateInput(Kind.Brep, -1, "tube:section-measure").ToError())
        from _ in double.IsFinite(chordToleranceMm) && chordToleranceMm > 0.0 && maximumStations >= 3
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new GeometryFault.DegenerateInput(Kind.Brep, -1, "tube:section-policy").ToError())
        let stationCount = Math.Max(3, (int)Math.Ceiling(metric.Path.Millimeters / chordToleranceMm))
        from __ in stationCount <= maximumStations
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new GeometryFault.DegenerateInput(Kind.Brep, -1, "tube:section-stations").ToError())
        from stations in toSeq(Enumerable.Range(0, stationCount)).Traverse(index =>
            profile.Apply(new ProfileOp.Sample(Length.FromMillimeters(metric.Path.Millimeters * index / stationCount)))
                .Bind(static result => result is ProfileResult.Sampled sample
                    ? Fin.Succ(sample.Point)
                    : Fin.Fail<Point3d>(new GeometryFault.DegenerateInput(Kind.Brep, -1, "tube:section-sample").ToError()))
                .ToValidation()).As().ToFin()
        from bound in profile.Apply(new ProfileOp.Bound())
        from box in bound is ProfileResult.Bound bounded
            ? Fin.Succ(bounded.Box)
            : Fin.Fail<BoundingBox>(new GeometryFault.DegenerateInput(Kind.Brep, -1, "tube:section-bound").ToError())
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
        let centroid = weighted.Fold(Vector3d.Zero, (sum, row) => sum + ((Vector3d)row.Midpoint * row.Area)) / area
        let ix = weighted.Fold(0.0, (sum, row) => sum + (row.Area * Math.Pow(row.Midpoint.Y - centroid.Y, 2.0)))
        let iy = weighted.Fold(0.0, (sum, row) => sum + (row.Area * Math.Pow(row.Midpoint.X - centroid.X, 2.0)))
        let enclosed = Math.Abs(edges.Fold(0.0, static (sum, edge) =>
            sum + ((edge.First.X * edge.Second.Y) - (edge.Second.X * edge.First.Y))) / 2.0)
        let radii = stations.Map(point => point.DistanceTo(new Point3d(centroid.X, centroid.Y, centroid.Z)))
        from radialRatio in radii.Head
            .Map(seed => radii.Fold(seed, Math.Max) / radii.Fold(seed, Math.Min))
            .ToFin(new GeometryFault.DegenerateInput(Kind.Brep, -1, "tube:section-radii").ToError())
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
        ref ValidationError? validationError,
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
                : new ValidationError(message: "Tube tooling must carry a process and section range, physical positions, capacity, and qualified deformation limits.");
}

[ComplexValueObject]
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
        ref ValidationError? validationError,
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
            && tools.GroupBy(static tool => tool.Key).ForAll(static group => group.Count() == 1)
            && format is not null && double.IsFinite(collinearAngleDeg) && collinearAngleDeg is >= 0.0 and < 180.0
            && Seq(minimumSegmentMm, maximumOverbendDeg, chordToleranceMm, copeAxialSpanMm)
                .ForAll(static value => double.IsFinite(value) && value > 0.0)
            && double.IsFinite(rootAccuracyDeg) && rootAccuracyDeg is > 0.0 and <= 1.0 && rootIterations > 0
            && double.IsFinite(maximumOvality) && maximumOvality is >= 0.0 and < 1.0
            && double.IsFinite(maximumThinning) && maximumThinning is >= 0.0 and < 1.0
            && maximumCopeStations >= 3
            && double.IsFinite(weldSeamExclusionDeg) && weldSeamExclusionDeg is >= 0.0 and <= 90.0
                ? null
                : new ValidationError(message: "Tube policy must carry unique tooling, numeric tolerances, deformation limits, and cope resolution.");
}

[ComplexValueObject]
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
        ref ValidationError? validationError,
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
                : new ValidationError(message: "Tube run must carry a centerline, admitted section and physics, CLR, context, and end allowances.");
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
public sealed partial class TubeCommand {
    public BendFormat Format { get; }
    public TubeCoordinate Coordinate { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref BendFormat format,
        ref TubeCoordinate coordinate) =>
        validationError = format is not null
            && Seq(coordinate.FeedMm, coordinate.RotationDeg, coordinate.CommandDeg, coordinate.RadiusMm)
                .ForAll(static value => double.IsFinite(value))
            && coordinate.FeedMm >= 0.0 && coordinate.RadiusMm > 0.0
            && coordinate.Vertex.IsValid && coordinate.Incoming.IsValid && coordinate.Outgoing.IsValid
                ? null
                : new ValidationError(message: "Tube commands require one format and a finite canonical coordinate.");
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
public sealed partial class RollSection {
    public string Key { get; }
    public RollSectionKind Kind { get; }
    public Loop Profile { get; }
    public SectionProperties Properties { get; }
    public double GoverningThicknessMm { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
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
                : new ValidationError(message: "Roll section must carry kind-correct profile topology, governing thickness, and positive axis mechanics.");

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
                ? Fin.Fail<RollSection>(new GeometryFault.DegenerateInput(Kind.Brep, -1, error.Message).ToError())
                : Fin.Succ(section);
}

[ComplexValueObject]
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
        ref ValidationError? validationError,
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
                : new ValidationError(message: "Roll policy must carry positive curvature, torque, gap, pass, springback, and distortion limits.");
}

[ComplexValueObject]
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
        ref ValidationError? validationError,
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
                : new ValidationError(message: "Roll runs require admitted section mechanics, axis, material physics, positive radius, sweep, and width.");
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

// --- [OPERATIONS] ---------------------------------------------------------------------------------------------------------------------------------
public static class TubeProgram {
    public static Fin<TubeResult> Apply(TubeOp operation) => operation is null
        ? Fin.Fail<TubeResult>(new GeometryFault.DegenerateInput(Kind.Brep, -1, "tube:operation").ToError())
        : operation.Switch(
            form: static value => Form(value.Run, value.Kind, value.Machine).Map<TubeResult>(static receipt => new TubeResult.Formed(receipt)),
            roll: static value => Roll(value.Run, value.Machine).Map<TubeResult>(static receipt => new TubeResult.Rolled(receipt)),
            cope: static value => Cope(value.Source).Map(result => (TubeResult)new TubeResult.Coped(result.Curves, result.Receipt)));

    private static Fin<TubeProgramReceipt> Form(TubeRun run, TubeFormKind kind, ProcessEnvelope.Bender machine) =>
        run is null || kind is null || machine is null
            ? Fin.Fail<TubeProgramReceipt>(new GeometryFault.DegenerateInput(Kind.Brep, -1, "tube:form").ToError())
            : from points in Normalize(run.Centerline, run.Policy, run.Tolerance)
              from bends in toSeq(Enumerable.Range(1, Math.Max(0, points.Count - 2)))
                  .Traverse(index => BendOf(index, points, run, kind).ToValidation()).As().ToFin()
              let requiredDies = bends.Fold(Set<string>(), static (keys, bend) => keys.Add(bend.ToolKey)).Count
              from _ in ValidMachine(machine, run.ClrMm, requiredDies)
              from receipt in Project(points, bends, run, kind)
              select receipt;

    private static Fin<Arr<Point3d>> Normalize(Arr<Point3d> source, TubePolicy policy, Context tolerance) =>
        toSeq(source.Skip(1)).FoldM<Fin, Seq<Point3d>>(
                Seq(source[0]),
                (held, point) => held.Last
                    .ToFin(new GeometryFault.DegenerateInput(Kind.Brep, -1, "tube:centerline-empty").ToError())
                    .Bind(prior => point.DistanceTo(prior) <= Math.Max(policy.MinimumSegmentMm, tolerance.Absolute.Value)
                        ? Fin.Fail<Seq<Point3d>>(new GeometryFault.DegenerateInput(Kind.Brep, -1, "tube:centerline-zero").ToError())
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
               from _ in directions && bendDeg is > 0.0 and < 180.0
                    && run.ClrMm >= run.Forming.MinBendRadiusFactor * run.Section.Properties.MajorMm
                ? Fin.Succ(unit)
                : Fin.Fail<Unit>(FabricationFault.MinBendRadiusViolated(index, run.ClrMm,
                    run.Forming.MinBendRadiusFactor * run.Section.Properties.MajorMm))
               from tool in ToolOf(run, kind, feed)
               from command in Springback(bendDeg, run.ClrMm, run, kind)
               from quality in Quality(run, tool, bendDeg, points, index)
               from __ in feed >= Math.Max(run.Policy.MinimumSegmentMm,
                       kind.MinimumStraight(tool, run.Policy))
                    ? Fin.Succ(unit)
                    : Fin.Fail<Unit>(new GeometryFault.DegenerateInput(Kind.Brep, -1, $"tube:straight:{index}:{feed:0.###}").ToError())
               let neutralRadius = run.ClrMm + ((run.Forming.KFactor - 0.5) * run.Section.WallMm)
               let neutralArc = Angle.FromDegrees(bendDeg).Radians * neutralRadius
               let force = Force.FromNewtons(
                   kind.ForceFactor * run.Forming.FlowStressMpa * run.Section.Properties.SxMm3 / run.ClrMm).Kilonewtons
               from ___ in force <= tool.CapacityKn
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
        return run.Policy.Tools
            .Filter(tool => tool.Processes.Contains(kind) && tool.Sections.Contains(run.Section.Family)
                && tool.Materials.Contains(run.Material.Family)
                && run.ClrMm >= tool.MinClrMm && run.ClrMm <= tool.MaxClrMm
                && ratio >= tool.MinDiameterWallRatio && ratio <= tool.MaxDiameterWallRatio
                && straightMm >= tool.MinStraightMm)
            .OrderBy(static tool => tool.MaxClrMm - tool.MinClrMm)
            .ThenBy(static tool => tool.QualifiedOvality + tool.QualifiedThinning)
            .ThenBy(static tool => tool.Key, StringComparer.Ordinal)
            .HeadOrNone()
            .ToFin(new GeometryFault.DegenerateInput(Kind.Brep, -1, $"tube:tool:{kind.Key}:{run.ClrMm:0.###}").ToError());
    }

    // Elastic recovery tracks the loaded radius under the conserved neutral-fibre arc, and the extreme fibre of a
    // section sits at half its major dimension, so the normalized elastic index is `r = 2 * R * yield / (E * major)`
    // and the recovered angle is `command * (4r^3 - 3r + 1)` — cubic in a reciprocal of the command, hence bracketed.
    private static Fin<double> Springback(double bendDeg, double clrMm, TubeRun run, TubeFormKind kind) {
        double fibre = (run.Forming.KFactor - 0.5) * run.Section.WallMm;
        double arc = Angle.FromDegrees(bendDeg).Radians * (clrMm + fibre);
        double elastic = kind.RecoveryFactor * run.Forming.SpringbackRatio
            * 2.0 * run.Material.Mechanical.YieldStrengthMpa
            / (run.Material.Mechanical.ElasticModulusMpa * run.Section.Properties.MajorMm);
        return Brent.TryFindRoot(
            command => Recovered(command) - bendDeg,
            bendDeg,
            bendDeg + run.Policy.MaximumOverbendDeg,
            run.Policy.RootAccuracyDeg,
            run.Policy.RootIterations,
            out double command)
                ? Fin.Succ(command)
                : Fin.Fail<double>(new GeometryFault.DegenerateInput(Kind.Brep, -1, "tube:springback").ToError());

        double Recovered(double commandDeg) {
            double index = elastic * ((arc / Angle.FromDegrees(commandDeg).Radians) - fibre);
            return commandDeg * ((4.0 * index * index * index) - (3.0 * index) + 1.0);
        }
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
               from _ in ovality <= Math.Min(run.Policy.MaximumOvality, tool.QualifiedOvality)
                && thinning <= Math.Min(run.Policy.MaximumThinning, tool.QualifiedThinning)
                && fiberStrain <= run.Forming.LimitStrain
                && weld.ForAll(angle => {
                    double seamAxis = Math.Min(angle % 180.0, 180.0 - (angle % 180.0));
                    return seamAxis >= run.Policy.WeldSeamExclusionDeg;
                })
                    ? Fin.Succ(unit)
                    : Fin.Fail<Unit>(new GeometryFault.DegenerateInput(Kind.Brep, -1, $"tube:quality:{index}:{ovality:0.######}:{thinning:0.######}").ToError())
               select new TubeQuality(ovality, thinning, fiberStrain, run.Forming.LimitStrain - fiberStrain, weld);
    }

    private static Fin<TubeCommand> CommandOf(BendFormat format, TubeCoordinate coordinate) =>
        TubeCommand.Validate(format, format.Project(coordinate), out TubeCommand command) is { } error
            ? Fin.Fail<TubeCommand>(new GeometryFault.DegenerateInput(Kind.Brep, -1, error.Message).ToError())
            : Fin.Succ(command);

    private static Fin<TubeProgramReceipt> Project(
        Arr<Point3d> points,
        Seq<TubeBend> bends,
        TubeRun run,
        TubeFormKind kind) {
        double nominal = points.Zip(points.Tail).Fold(0.0, static (sum, edge) => sum + edge.First.DistanceTo(edge.Second));
        double tangent = bends.Fold(0.0, static (sum, bend) => sum + bend.Coordinate.FeedMm);
        double terminal = points.Count < 2
            ? 0.0
            : points[^2].DistanceTo(points[^1])
                - bends.Last.Map(static bend => bend.Coordinate.RadiusMm * Math.Tan(Angle.FromDegrees(bend.GeometricBendDeg).Radians / 2.0)).IfNone(0.0)
                + run.TailAllowanceMm
                + (bends.IsEmpty ? run.LeadAllowanceMm : 0.0);
        if (!double.IsFinite(terminal) || terminal < run.Policy.MinimumSegmentMm)
            return Fin.Fail<TubeProgramReceipt>(new GeometryFault.DegenerateInput(Kind.Brep, -1, "tube:terminal-feed").ToError());
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
            ? Fin.Fail<RollReceipt>(new GeometryFault.DegenerateInput(Kind.Brep, -1, "tube:roll").ToError())
            : from _ in Seq(machine.MaxWidthMm, machine.MinThicknessMm, machine.MaxThicknessMm, machine.TorqueNm)
                        .ForAll(static value => double.IsFinite(value) && value > 0.0)
                    && machine.MaxThicknessMm >= machine.MinThicknessMm && machine.Stations >= 3
                        ? Fin.Succ(unit)
                        : Fin.Fail<Unit>(new GeometryFault.DegenerateInput(Kind.Brep, -1, "tube:roll-machine").ToError())
              let radius = run.TargetRadius.Millimeters
              let depth = run.Axis.Depth(run.Section.Properties)
              from __ in double.IsFinite(depth) && depth > 0.0
                    ? Fin.Succ(unit)
                    : Fin.Fail<Unit>(new GeometryFault.DegenerateInput(Kind.Brep, -1, "tube:roll-demand").ToError())
              let targetCurvature = 1.0 / radius
              let requiredPasses = Math.Ceiling(targetCurvature / run.Policy.MaximumCurvatureIncrement)
              from passes in double.IsFinite(requiredPasses)
                    && requiredPasses is >= 1.0 and <= int.MaxValue
                    && requiredPasses <= run.Policy.MaximumPasses
                        ? Fin.Succ((int)requiredPasses)
                        : Fin.Fail<int>(new GeometryFault.DegenerateInput(
                            Kind.Brep,
                            -1,
                            $"tube:roll-envelope:passes:{requiredPasses:R}").ToError())
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
              from ___ in run.WorkpieceWidth.Millimeters <= machine.MaxWidthMm
                    && run.Section.GoverningThicknessMm >= machine.MinThicknessMm
                    && run.Section.GoverningThicknessMm <= machine.MaxThicknessMm
                    && radius >= run.Section.Kind.MinimumRadiusFactor * run.Forming.MinBendRadiusFactor * depth
                    && peakTorque <= machine.TorqueNm
                    ? Fin.Succ(unit)
                    : Fin.Fail<Unit>(new GeometryFault.DegenerateInput(Kind.Brep, -1,
                        $"tube:roll-envelope:{peakTorque:0.###}:{machine.TorqueNm:0.###}").ToError())
              from ____ in double.IsFinite(maximumDistortion) && maximumDistortion <= run.Policy.MaximumDistortion
                    ? Fin.Succ(unit)
                    : Fin.Fail<Unit>(new GeometryFault.DegenerateInput(Kind.Brep, -1, $"tube:roll-distortion:{maximumDistortion:0.######}").ToError())
              let developed = radius * run.Sweep.Radians
              select new RollReceipt(
                  run.Section,
                  run.Axis,
                  run.Material,
                  run.Forming,
                  rows,
                  developed,
                  maximumDistortion,
                  machine.TorqueNm - peakTorque,
                  ContentKey.Of(EgressKind.BendProgram, Canonical(
                      run.Section,
                      run.Axis,
                      run.Material,
                      run.Forming,
                      rows,
                      developed,
                      maximumDistortion,
                      machine.TorqueNm - peakTorque)));

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
                : Fin.Fail<double>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "tube:roll-recovery-root").ToError());
    }

    private static Fin<(Seq<Loop> Curves, CopeReceipt Receipt)> Cope(CopeSource source) => source is null
        ? Fin.Fail<(Seq<Loop>, CopeReceipt)>(new GeometryFault.DegenerateInput(Kind.Brep, -1, "tube:cope").ToError())
        : source.Switch(
            analytic: static value => AnalyticCope(value),
            sectioned: static value => SectionedCope(value));

    private static Fin<(Seq<Loop> Curves, CopeReceipt Receipt)> AnalyticCope(CopeSource.Analytic source) {
        if (source.Branch is null || source.Main is null || source.End is null || source.Policy is null || source.Tolerance is null
            || !source.Branch.Family.AnalyticCope || !source.Main.Family.AnalyticCope
            || source.Intersection.Radians is <= 0.0 || source.Intersection.Radians >= Math.PI)
            return Fin.Fail<(Seq<Loop>, CopeReceipt)>(new GeometryFault.DegenerateInput(Kind.Brep, -1, "tube:cope-analytic").ToError());
        double branchRadius = (source.Branch.Properties.MajorMm + source.Branch.WallMm) / 2.0;
        double mainRadius = (source.Main.Properties.MajorMm + source.Main.WallMm) / 2.0;
        int samples = Math.Max(3, (int)Math.Ceiling(2.0 * Math.PI * branchRadius / source.Policy.ChordToleranceMm));
        if (samples > source.Policy.MaximumCopeStations)
            return Fin.Fail<(Seq<Loop>, CopeReceipt)>(new GeometryFault.DegenerateInput(Kind.Brep, -1, "tube:cope-stations").ToError());
        Fin<Seq<Point3d>> solved = toSeq(Enumerable.Range(0, samples)).Traverse(index => {
            double theta = 2.0 * Math.PI * index / samples;
            double x = branchRadius * Math.Cos(theta);
            double y = branchRadius * Math.Sin(theta);
            double alpha = source.Intersection.Radians;
            Func<double, double> residual = z => {
                    Vector3d point = new(x, y, z);
                    Vector3d axis = new(Math.Sin(alpha), 0.0, Math.Cos(alpha));
                    return point.SquareLength - Math.Pow(point * axis, 2.0) - (mainRadius * mainRadius);
                };
            bool lower = Brent.TryFindRoot(
                residual,
                -source.Policy.CopeAxialSpanMm,
                0.0,
                source.Policy.ChordToleranceMm,
                source.Policy.RootIterations,
                out double lowerZ);
            bool upper = Brent.TryFindRoot(
                residual,
                0.0,
                source.Policy.CopeAxialSpanMm,
                source.Policy.ChordToleranceMm,
                source.Policy.RootIterations,
                out double upperZ);
            return source.End.Select(index, lower, lowerZ, upper, upperZ)
                .Map(z => new Point3d(branchRadius * theta, z, 0.0))
                .ToValidation();
        }).As().ToFin();
        return from points in solved
               from loop in Loop.Admit(points.ToArr(), closed: true, Arr<double>(), source.Tolerance)
               let key = ContentKey.Of(EgressKind.FlatPattern, Canonical(Seq(loop), Seq<CopeProjection>(), None))
               select (Seq(loop), new CopeReceipt(samples, samples, Seq<CopeProjection>(), None, key));
    }

    private static Fin<(Seq<Loop> Curves, CopeReceipt Receipt)> SectionedCope(CopeSource.Sectioned source) =>
        from _ in source.Part is not null && source.Tool is not null && source.Development is not null && source.Intersection is not null
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new GeometryFault.DegenerateInput(Kind.Brep, -1, "tube:cope-sectioned").ToError())
        from intersection in Intersection.Apply(new IntersectOp.MeshMesh(source.Part.Mesh, source.Tool, source.Intersection))
        from chains in intersection is IntersectResult.Chains crossed
            ? Fin.Succ(crossed)
            : Fin.Fail<IntersectResult.Chains>(new GeometryFault.DegenerateInput(Kind.Brep, -1, "tube:cope-intersection").ToError())
        from development in Development.Apply(new DevelopOp.Unroll(source.Part, source.Development))
        from unrolled in development is DevelopmentResult.Unrolled value
            ? Fin.Succ(value)
            : Fin.Fail<DevelopmentResult.Unrolled>(new GeometryFault.DegenerateInput(Kind.Brep, -1, "tube:cope-development").ToError())
        from edges in ProjectEdges(chains.Lattice, unrolled.Atlas)
        from developed in chains.Walked
            .Traverse(chain => DevelopedChain(chain, chains.Lattice, edges, source.Part.Mesh.Tolerance).ToValidation()).As().ToFin()
        let loops = developed.Bind(static run => run)
        let projected = edges.Bind(static edge => Seq(edge.A, edge.B))
        let distortion = Some(unrolled.Atlas.Receipt)
        let key = ContentKey.Of(EgressKind.FlatPattern, Canonical(loops, projected, distortion))
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
            from _ in a.Chart == b.Chart
                ? Fin.Succ(unit)
                : Fin.Fail<Unit>(new GeometryFault.DegenerateInput(Kind.Brep, -1, $"tube:cope-edge-chart:{row.A}:{row.B}").ToError())
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
        return Fin.Fail<CopeProjection>(new GeometryFault.DegenerateInput(Kind.Brep, -1, $"tube:cope-key:{index}").ToError());
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
                   .HeadOrNone()
                   .ToFin(new GeometryFault.DegenerateInput(Kind.Brep, -1, $"tube:cope-face:{index}").ToError())
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
        CrossLattice lattice,
        Seq<DevelopedEdge> projected,
        Context tolerance) =>
        from segments in chain.Points.ToSeq().Zip(chain.Points.ToSeq().Skip(1))
            .Traverse(pair => (
                from crossingA in CrossingAt(pair.First, lattice)
                from crossingB in CrossingAt(pair.Second, lattice)
                from edge in projected
                    .Find(row => row.CrossingA == crossingA && row.CrossingB == crossingB
                        || row.CrossingA == crossingB && row.CrossingB == crossingA)
                    .ToFin(new GeometryFault.DegenerateInput(Kind.Brep, -1, $"tube:cope-chain-edge:{crossingA}:{crossingB}").ToError())
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

    private static Fin<int> CrossingAt(Point3d point, CrossLattice lattice) {
        Seq<(Point3d Point, int Index)> matches = toSeq(lattice.Rows)
            .Map((crossing, index) => (Point: crossing.Point.Round(), Index: index))
            .Filter(row => row.Point == point);
        return matches.Count == 1
            ? matches.Head
                .Map(static row => row.Index)
                .ToFin(new GeometryFault.DegenerateInput(Kind.Brep, -1, "tube:cope-chain-provenance:0").ToError())
            : Fin.Fail<int>(new GeometryFault.DegenerateInput(Kind.Brep, -1, $"tube:cope-chain-provenance:{matches.Count}").ToError());
    }

    private static bool Near(Point2d a, Point2d b, double toleranceMm) =>
        Math.Sqrt(Math.Pow(a.X - b.X, 2.0) + Math.Pow(a.Y - b.Y, 2.0)) <= toleranceMm;

    private static Fin<UvIsland> Island(Seq<UvIsland> islands, MeshFace sourceFace, Seq<int> vertices) {
        Seq<int> source = sourceFace.IsQuad
            ? Seq(sourceFace.A, sourceFace.B, sourceFace.C, sourceFace.D)
            : Seq(sourceFace.A, sourceFace.B, sourceFace.C);
        return islands.Find(island => vertices.ForAll(island.Vertices.Contains)
                && island.Faces.Exists(face => Seq(face.A, face.B, face.C).ForAll(source.Contains)))
            .ToFin(new GeometryFault.DegenerateInput(Kind.Brep, -1, "tube:cope-chart").ToError());
    }

    private static Fin<Point2d> Uv(UvIsland island, int vertex) =>
        island.Vertices.Map((value, index) => (Vertex: value, Index: index))
            .Find(row => row.Vertex == vertex)
            .Map(row => island.Uv[row.Index])
            .ToFin(new GeometryFault.DegenerateInput(Kind.Brep, -1, $"tube:cope-vertex:{vertex}").ToError());

    private static Point2d Lerp(Point2d a, Point2d b, double t) => ((1.0 - t) * a) + (t * b);

    private static Fin<(double A, double B, double C)> Barycentric(Point3d point, Point3d a, Point3d b, Point3d c) {
        Vector3d v0 = b - a;
        Vector3d v1 = c - a;
        Vector3d v2 = point - a;
        double d00 = v0 * v0;
        double d01 = v0 * v1;
        double d11 = v1 * v1;
        double d20 = v2 * v0;
        double d21 = v2 * v1;
        double denominator = (d00 * d11) - (d01 * d01);
        if (!double.IsFinite(denominator) || denominator == 0.0)
            return Fin.Fail<(double, double, double)>(new GeometryFault.DegenerateInput(Kind.Brep, -1, "tube:cope-barycentric").ToError());
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
            : Fin.Fail<double>(new GeometryFault.DegenerateInput(Kind.Brep, -1, $"tube:rotation:{index}").ToError());
    }

    private static Fin<double> AngleAt(Point3d before, Point3d at, Point3d after) {
        Vector3d incoming = at - before;
        Vector3d outgoing = after - at;
        return incoming.Unitize() && outgoing.Unitize()
            ? Fin.Succ(Degrees(Math.Acos(Math.Clamp(incoming * outgoing, -1.0, 1.0))))
            : Fin.Fail<double>(new GeometryFault.DegenerateInput(Kind.Brep, -1, "tube:centerline-angle").ToError());
    }

    private static double Degrees(double radians) => radians / Angle.FromDegrees(1.0).Radians;

    private static Fin<Unit> ValidMachine(ProcessEnvelope.Bender machine, double clrMm, int requiredDies) =>
        double.IsFinite(machine.MinClrMm) && machine.MinClrMm > 0.0
            && double.IsFinite(machine.MaxClrMm) && machine.MaxClrMm >= machine.MinClrMm
            && machine.DieCount >= 0 && requiredDies >= 0
            && (requiredDies == 0 || machine.DieCount >= requiredDies && clrMm >= machine.MinClrMm && clrMm <= machine.MaxClrMm)
                ? Fin.Succ(unit)
                : Fin.Fail<Unit>(new GeometryFault.DegenerateInput(Kind.Brep, -1, "tube:bender-envelope").ToError());

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
        using ArrayPoolBufferWriter<byte> writer = new();
        Write(writer, process.Key);
        Write(writer, section.Family.Key);
        Write(writer, section.WallMm);
        Write(writer, section.WeldSeamDeg.IsSome ? 1 : 0);
        section.WeldSeamDeg.Iter(value => Write(writer, value));
        Write(writer, section.Profile);
        Write(writer, section.Properties);
        Write(writer, material.Family.Key);
        Write(writer, material.Identity.Grade);
        Write(writer, forming.KFactor);
        Write(writer, forming.TensileRm);
        Write(writer, forming.SpringbackRatio);
        Write(writer, forming.MinBendRadiusFactor);
        Write(writer, forming.FlowStressMpa);
        Write(writer, forming.LimitStrain);
        Write(writer, forming.Evidence);
        Write(writer, bends.Count);
        bends.Iter(bend => {
            Write(writer, bend.Index);
            Write(writer, bend.Command.Format.Key);
            Write(writer, bend.Command.Coordinate);
            Write(writer, bend.Coordinate);
            Write(writer, bend.GeometricBendDeg);
            Write(writer, bend.NeutralArcMm);
            Write(writer, bend.ToolKey);
            Write(writer, bend.Mandrel.Key);
            Write(writer, bend.BallCount);
            Write(writer, bend.MandrelNoseMm);
            Write(writer, bend.WiperRakeDeg);
            Write(writer, bend.PressureAssistKn);
            Write(writer, bend.BoostMm);
            Write(writer, bend.ForceKn);
            Write(writer, bend.Quality.Ovality);
            Write(writer, bend.Quality.WallThinning);
            Write(writer, bend.Quality.FiberStrain);
            Write(writer, bend.Quality.StrainMargin);
            Write(writer, bend.Quality.WeldSeamDeg.IsSome ? 1 : 0);
            bend.Quality.WeldSeamDeg.Iter(value => Write(writer, value));
        });
        Write(writer, terminalFeedMm);
        Write(writer, nominalCenterlineMm);
        Write(writer, developedLengthMm);
        Write(writer, cutLengthMm);
        return writer.WrittenSpan.ToArray();
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
        using ArrayPoolBufferWriter<byte> writer = new();
        Write(writer, section.Key);
        Write(writer, section.Kind.Key);
        Write(writer, axis.Key);
        Write(writer, section.GoverningThicknessMm);
        Write(writer, section.Profile);
        Write(writer, section.Properties);
        Write(writer, material.Family.Key);
        Write(writer, material.Identity.Grade);
        Write(writer, forming.FlowStressMpa);
        Write(writer, forming.TensileRm);
        Write(writer, forming.KFactor);
        Write(writer, forming.SpringbackRatio);
        Write(writer, forming.MinBendRadiusFactor);
        Write(writer, forming.LimitStrain);
        Write(writer, forming.Evidence);
        Write(writer, passes.Count);
        passes.Iter(pass => {
            Write(writer, pass.Index);
            Write(writer, pass.InputRadiusMm.IsSome ? 1 : 0);
            pass.InputRadiusMm.Iter(value => Write(writer, value));
            Write(writer, pass.CommandRadiusMm);
            Write(writer, pass.OutputRadiusMm);
            Write(writer, pass.GapMm);
            Write(writer, pass.TorqueNm);
            Write(writer, pass.SpringbackDeg);
            Write(writer, pass.Distortion);
        });
        Write(writer, developedLengthMm);
        Write(writer, maximumDistortion);
        Write(writer, torqueMarginNm);
        return writer.WrittenSpan.ToArray();
    }

    private static byte[] Canonical(
        Seq<Loop> loops,
        Seq<CopeProjection> projection,
        Option<DistortionReceipt> distortion) {
        using ArrayPoolBufferWriter<byte> writer = new();
        Write(writer, loops.Count);
        loops.Iter(loop => {
            Write(writer, loop.Count);
            Write(writer, loop.Closed ? 1 : 0);
            Write(writer, loop.Tolerance.Absolute.Value);
            loop.Vertices.Iter(point => { Write(writer, point.X); Write(writer, point.Y); Write(writer, point.Z); });
            loop.Bulges.Iter(bulge => Write(writer, bulge));
        });
        Write(writer, projection.Count);
        projection.Iter(row => {
            Write(writer, row.Crossing); Write(writer, row.Chart.Value); Write(writer, row.Uv.X); Write(writer, row.Uv.Y);
        });
        Write(writer, distortion.IsSome ? 1 : 0);
        distortion.Iter(row => {
            Write(writer, row.MaxConformal); Write(writer, row.MeanConformal);
            Write(writer, row.MaxArea); Write(writer, row.MinArea); Write(writer, row.MeanArea);
            Write(writer, row.MaxQuasiConformal); Write(writer, row.Iterations); Write(writer, row.Residual);
            Write(writer, row.FactorNonZeros); Write(writer, row.FlipFreeBijective ? 1 : 0);
        });
        return writer.WrittenSpan.ToArray();
    }

    private static void Write(ArrayPoolBufferWriter<byte> writer, int value) {
        BinaryPrimitives.WriteInt32LittleEndian(writer.GetSpan(sizeof(int)), value);
        writer.Advance(sizeof(int));
    }

    private static void Write(ArrayPoolBufferWriter<byte> writer, double value) {
        BinaryPrimitives.WriteDoubleLittleEndian(writer.GetSpan(sizeof(double)), value);
        writer.Advance(sizeof(double));
    }

    private static void Write(ArrayPoolBufferWriter<byte> writer, string value) {
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(value);
        Write(writer, bytes.Length);
        bytes.CopyTo(writer.GetSpan(bytes.Length));
        writer.Advance(bytes.Length);
    }

    private static void Write(ArrayPoolBufferWriter<byte> writer, TubeCoordinate coordinate) {
        Write(writer, coordinate.FeedMm); Write(writer, coordinate.RotationDeg);
        Write(writer, coordinate.CommandDeg); Write(writer, coordinate.RadiusMm);
        Write(writer, coordinate.Vertex.X); Write(writer, coordinate.Vertex.Y); Write(writer, coordinate.Vertex.Z);
        Write(writer, coordinate.Incoming.X); Write(writer, coordinate.Incoming.Y); Write(writer, coordinate.Incoming.Z);
        Write(writer, coordinate.Outgoing.X); Write(writer, coordinate.Outgoing.Y); Write(writer, coordinate.Outgoing.Z);
    }

    private static void Write(ArrayPoolBufferWriter<byte> writer, Loop loop) {
        Write(writer, loop.Count); Write(writer, loop.Closed ? 1 : 0);
        Write(writer, loop.Tolerance.Absolute.Value);
        loop.Vertices.Iter(point => { Write(writer, point.X); Write(writer, point.Y); Write(writer, point.Z); });
        loop.Bulges.Iter(value => Write(writer, value));
    }

    private static void Write(ArrayPoolBufferWriter<byte> writer, SectionProperties properties) {
        Write(writer, properties.MetalAreaMm2); Write(writer, properties.Centroid.X); Write(writer, properties.Centroid.Y);
        Write(writer, properties.IxMm4); Write(writer, properties.IyMm4); Write(writer, properties.JMm4);
        Write(writer, properties.SxMm3); Write(writer, properties.SyMm3); Write(writer, properties.PerimeterMm);
        Write(writer, properties.WidthMm); Write(writer, properties.HeightMm);
        Write(writer, properties.MajorMm); Write(writer, properties.MinorMm);
        Write(writer, properties.VertexCount); Write(writer, properties.CurvedEdges); Write(writer, properties.RadialRatio);
    }

    private static void Write(ArrayPoolBufferWriter<byte> writer, BudgetEvidence evidence) {
        Write(writer, evidence.State.TemperatureC); Write(writer, evidence.State.Hardness);
        Write(writer, evidence.State.StrainRate); Write(writer, evidence.State.Strain);
        Write(writer, evidence.State.MoistureFraction); Write(writer, evidence.State.GrainSizeUm);
        Write(writer, evidence.PowerW);
        Write(writer, evidence.Energy.Joules.IsSome ? 1 : 0);
        evidence.Energy.Joules.Iter(value => Write(writer, value));
        Write(writer, evidence.Energy.Seconds.IsSome ? 1 : 0);
        evidence.Energy.Seconds.Iter(value => Write(writer, value));
        Write(writer, evidence.Material.Family.Key); Write(writer, evidence.Material.Identity.Grade);
    }
}
```
