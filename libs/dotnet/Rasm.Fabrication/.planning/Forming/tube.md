# [RASM_FABRICATION_TUBE_PROGRAM]

`TubeProgram` owns one tube-forming algebra across discrete bending, axis-specific section roll curving, and cope projection. `TubeSection`, `RollSection`, `TubeTool`, and `TubePolicy` admit section mechanics, material, weld seam, tooling, deformation limits, and egress policy once; numeric bands are `ToleranceLane` reads off the admitting `Context`, never policy columns beside them.

Cross-section algebra is the `Rasm.Element` `Composition/material#SectionProperties` contract owner — `OfMillimetres` admits every column on this page's millimetre basis and `SectionForm` carries the shape witness the family, the tool catalogue, and every deformation law read.

`TubeProgram.Apply` composes the frozen `ProcessEnvelope.Bender`, `ProcessEnvelope.Roll`, `ProcessBudget.Formed`, `Intersection.Apply`, `Development.Apply`, `UvIsland`, and `ContentKey.Of` wires. Intersection provenance and atlas provenance remain intact through sectioned cope projection.

## [01]-[INDEX]

- [02]-[TUBE_FORMING]: generated process and format families, section and roll mechanics, tooling admission, and the operation and result vocabularies.
- [03]-[TUBE_PROGRAM]: operation dispatch, neutral-axis bend programs, multi-pass roll schedules, cope generation with internalized provenance, developed-chain projection, and the content-keyed preimage.

## [02]-[TUBE_FORMING]

- Owner: `TubeFormKind` owns discrete process physics; `BendFormat` owns command projection; `CopeEnd` owns analytic branch-end selection; `TubeSection` owns closed thin-wall mechanics ONTO the `Rasm.Element` section contract; `RollSection` and `RollAxis` own closed, open, solid, and plate roll mechanics; `TubeTool` owns tooling evidence; `TubeProgram` owns all operation dispatch and projection.
- Cases: `TubeOp` carries `Form`, `Roll`, and `Cope`; `TubeResult` mirrors those modalities through `BendProgram`, `RollSchedule`, and `CopePattern`; `TubeCommand` binds one canonical `TubeCoordinate` to a `BendFormat` projection row; `TubeFormKind` carries rotary-draw, compression, ram, push, stretch, and freeform behavior; `CopeEnd` selects the negative or positive analytic root; `MandrelKind` carries the tooling axis.
- Entry: `TubeProgram.Apply(TubeOp)` is the one polymorphic entry for every modality.
- Law: cross-section columns seat on the `Rasm.Element` contract owner and this page holds NO section record of its own — one discretized wall run derives every column, `OfMillimetres` admits them, and the interior reads evidence rather than re-gating it. `SectionForm` carries the vertex census, curved-edge count, radial compactness, outline perimeter, and the two BOUNDING extents `Major`/`Minor` — bounding, never the radii of gyration the same owner spells beside them.
- Auto: centerlines normalize once, tooling resolves per bend, neutral-axis length consumes the forming budget, the folder's `ElasticLaw` inverts the CUBIC elastic-recovery law over the loaded radius for bend springback and a bracketed root recovers pass curvature — the only transcendental inversions on the page, and the cope station's quadratic never reaches them — mandrel rows supply their own interior wall support, weld-seam rotation propagates, roll passes generate command curvature with axis modulus and distortion gates, and sectioned cope lowers exact crossing keys through source vertices or source faces into developed islands.
- Result: `BendProgram` carries bend evidence and key; `RollSchedule` carries roll evidence and key; `CopePattern` carries developed curves, cope evidence, and key. Each lane's frame mints its key from the complete operation result.
- Packages: `LanguageExt.Core`, `Thinktecture.Runtime.Extensions`, the `Rasm.Element` `CanonicalWriter` codec behind `FabricationCanon` and the `Composition/material` section contract, `MathNet.Numerics`, `UnitsNet` (`Length`, `Angle`, `Area`, `Ratio`, `ReciprocalLength`, `Force`, `Torque`), `RhinoCommon`, `Rasm.Meshing`, `Rasm.Parametric`, `Rasm.Processing`, and `ContentKey` compose the surface.
- Growth: A discrete process is one `TubeFormKind` row, a command convention is one `BendFormat` row, a physical tool is one catalog row, an analytic branch end is one `CopeEnd` row, a roll target is data, and a new modality is one `TubeOp`/`TubeResult` case pair.
- Boundary: Forming owns tube mechanics and projection; machine capacity, process material physics, exact intersection, development, planar loop admission, posting text, and content identity remain at their canonical owners.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using LanguageExt;
using LanguageExt.Common;
using MathNet.Numerics.RootFinding;
using Rasm.Domain;
using Rasm.Element.Composition;
using Rasm.Element.Projection;
using Rasm.Element.Properties;
using Rasm.Fabrication.Kinematics;
using Rasm.Fabrication.Process;
using Rasm.Meshing;
using Rasm.Numerics;
using Rasm.Parametric;
using Rasm.Processing;
using Rhino.Geometry;
using Thinktecture;
using UnitsNet;
using UnitsNet.Units;
using Vector3 = Rasm.Element.Graph.Vector3;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Forming;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class TubeFormKind {
    public static readonly TubeFormKind RotaryDraw = new("rotary-draw", forceFactor: 1.0, recoveryFactor: 1.0, static (tool, _) => tool.ClampLengthMm);
    public static readonly TubeFormKind Compression = new("compression", forceFactor: 1.35, recoveryFactor: 1.15, static (tool, _) => tool.ClampLengthMm);
    public static readonly TubeFormKind Ram = new("ram", forceFactor: 1.8, recoveryFactor: 1.35, static (tool, _) => tool.MinStraightMm);
    public static readonly TubeFormKind Push = new("push", forceFactor: 0.8, recoveryFactor: 0.9, static (_, policy) => policy.MinimumSegment.Millimeters);
    public static readonly TubeFormKind Stretch = new("stretch", forceFactor: 2.2, recoveryFactor: 0.35, static (tool, _) => 2.0 * tool.ClampLengthMm);
    public static readonly TubeFormKind Freeform = new("freeform", forceFactor: 0.7, recoveryFactor: 0.8, static (_, policy) => policy.MinimumSegment.Millimeters);

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
        : Fin.Fail<double>(new GeometryFault.DegenerateInput(Kind.Polyline, index, $"tube:cope-root:{index}:negative")));
    public static readonly CopeEnd Positive = new("positive", static (index, _, _, upper, upperZ) => upper
        ? Fin.Succ(upperZ)
        : Fin.Fail<double>(new GeometryFault.DegenerateInput(Kind.Polyline, index, $"tube:cope-root:{index}:positive")));

    [UseDelegateFromConstructor]
    public partial Fin<double> Select(int index, bool lower, double lowerZ, bool upper, double upperZ);
}

[SmartEnum<string>]
public sealed partial class TubeSectionFamily {
    public static readonly TubeSectionFamily Circular = new("circular", analyticCope: true, static form =>
        form.CurvedEdges > 0 && form.Major.Si / form.Minor.Si <= 1.01 && form.RadialRatio <= 1.01);
    public static readonly TubeSectionFamily Elliptic = new("elliptic", analyticCope: false, static form =>
        form.CurvedEdges > 0 && form.Major.Si / form.Minor.Si > 1.01);
    public static readonly TubeSectionFamily Rectilinear = new("rectilinear", analyticCope: false, static form =>
        form.VertexCount == 4 && form.CurvedEdges is 0 or 4);
    public static readonly TubeSectionFamily Polygonal = new("polygonal", analyticCope: false, static form =>
        form.CurvedEdges == 0 && form.VertexCount >= 3);
    public static readonly TubeSectionFamily Custom = new("custom", analyticCope: false, static _ => true);

    public bool AnalyticCope { get; }

    [UseDelegateFromConstructor]
    public partial bool Admits(SectionForm form);
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

    [UseDelegateFromConstructor]
    public partial double InteriorSupport(TubeTool tool, double majorMm);
}

internal static class SectionMillimetres {
    internal static double Millimetres(this MeasureValue value) => value.Si.Millimetres();
    internal static double SquareMillimetres(this MeasureValue value) => value.Si * 1e6;
    internal static double CubicMillimetres(this MeasureValue value) => value.Si * 1e9;
    internal static double QuarticMillimetres(this MeasureValue value) => value.Si * 1e12;

    internal static double Millimetres(this double siMetres) => siMetres * 1e3;
}

[ComplexValueObject]
public sealed partial class TubeSection {
    public TubeSectionFamily Family { get; }
    public Loop Profile { get; }
    public double WallMm { get; }
    public Option<double> WeldSeamDeg { get; }
    public SectionProperties Properties { get; }

    public SectionForm Form { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref TubeSectionFamily family,
        ref Loop profile,
        ref double wallMm,
        ref Option<double> weldSeamDeg,
        ref SectionProperties properties,
        ref SectionForm form) =>
        validationError = family is not null && profile is { Closed: true }
            && double.IsFinite(wallMm) && wallMm > 0.0 && wallMm < form.Minor.Millimetres() / 2.0
            && weldSeamDeg.ForAll(static angle => double.IsFinite(angle) && angle is >= 0.0 and < 360.0)
            && form.VertexCount >= 3 && form.CurvedEdges >= 0 && form.CurvedEdges <= form.VertexCount
            && properties.Form == Some(form)
            && family.Admits(form)
                ? null
                : new KernelFault.InvalidValue("tube", "tube:tube-section");

    public static Fin<TubeSection> Admit(
        TubeSectionFamily family,
        Loop profile,
        Length wall,
        Option<Angle> weldSeam,
        Dimension maximumStations) =>
        family is null || profile is null
            ? Fin.Fail<TubeSection>(new KernelFault.InvalidValue("tube", "tube:section"))
            : from measured in Mechanics(profile, wall.Millimeters, maximumStations)
              from section in TubeSection.Validate(
                  family,
                  profile,
                  wall.Millimeters,
                  weldSeam.Map(static angle => angle.Radians / Angle.FromDegrees(1.0).Radians),
                  measured.Properties,
                  measured.Form,
                  out TubeSection admitted).Admitted(admitted)
              select section;

    // --- [SECTION_MECHANICS]
    private static Fin<(SectionProperties Properties, SectionForm Form)> Mechanics(
        Loop profile,
        double wallMm,
        Dimension maximumStations) =>
        from measured in profile.Apply(new ProfileOp.Measure())
        from metric in measured is ProfileResult.Measure value
            ? Fin.Succ(value)
            : Fin.Fail<ProfileResult.Measure>(new KernelFault.InvalidValue("tube", "tube:section-measure"))
        let chordToleranceMm = profile.Tolerance.For(ToleranceLane.Chord).Value
        from _budget in maximumStations.Value >= 3
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new KernelFault.InvalidValue("tube", "tube:section-policy"))
        let stationCount = Math.Max(3, (int)Math.Ceiling(metric.Path.Millimeters / chordToleranceMm))
        from _stations in stationCount <= maximumStations.Value
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new KernelFault.InvalidValue("tube", "tube:section-stations"))
        from stations in toSeq(Enumerable.Range(0, stationCount)).Traverse(index =>
            profile.Apply(new ProfileOp.Sample(Length.FromMillimeters(metric.Path.Millimeters * index / stationCount)))
                .Bind(static result => result is ProfileResult.Sampled sample
                    ? Fin.Succ(sample.Point)
                    : Fin.Fail<Point3d>(new KernelFault.InvalidValue("tube", "tube:section-sample")))
                .ToValidation()).As().ToFin()
        from bound in profile.Apply(new ProfileOp.Bound())
        from box in bound is ProfileResult.Bound bounded
            ? Fin.Succ(bounded.Box)
            : Fin.Fail<BoundingBox>(new KernelFault.InvalidValue("tube", "tube:section-bound"))
        let edges = toSeq(Enumerable.Range(0, stations.Count))
            .Map(index => (First: stations[index], Second: stations[(index + 1) % stations.Count]))
        let weighted = edges.Map(edge => {
            double length = edge.First.DistanceTo(edge.Second);
            Point3d midpoint = new(
                (edge.First.X + edge.Second.X) / 2.0,
                (edge.First.Y + edge.Second.Y) / 2.0,
                (edge.First.Z + edge.Second.Z) / 2.0);
            Vector3d tangent = length > 0.0 ? (edge.Second - edge.First) / length : Vector3d.Zero;
            return (Area: length * wallMm, Midpoint: midpoint, Tangent: tangent);
        })
        let area = weighted.Fold(0.0, static (sum, row) => sum + row.Area)
        from _area in ValidityClaim.Positive(area) ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new GeometryFault.DegenerateInput(Kind.Polyline, None, "tube:section-area"))
        let centroid = weighted.Fold(Vector3d.Zero, (sum, row) => sum + ((Vector3d)row.Midpoint * row.Area)) / area
        let ix = weighted.Fold(0.0, (sum, row) => sum + (row.Area * Math.Pow(row.Midpoint.Y - centroid.Y, 2.0)))
        let iy = weighted.Fold(0.0, (sum, row) => sum + (row.Area * Math.Pow(row.Midpoint.X - centroid.X, 2.0)))
        let enclosed = Math.Abs(edges.Fold(0.0, static (sum, edge) =>
            sum + ((edge.First.X * edge.Second.Y) - (edge.Second.X * edge.First.Y))) / 2.0)
        let radii = stations.Map(point => point.DistanceTo(new Point3d(centroid.X, centroid.Y, centroid.Z)))
        from radialRatio in radii.Head
            .Map(seed => radii.Fold(seed, Math.Max) / radii.Fold(seed, Math.Min))
            .ToFin(new GeometryFault.DegenerateInput(Kind.Polyline, None, "tube:section-radii"))
        let width = box.Diagonal.X
        let height = box.Diagonal.Y
        let major = Math.Max(width, height)
        let minor = Math.Min(width, height)
        let plasticY = EqualAreaAxis(weighted.Map(static row => (Area: row.Area, Ordinate: row.Midpoint.Y)), area)
        let plasticX = EqualAreaAxis(weighted.Map(static row => (Area: row.Area, Ordinate: row.Midpoint.X)), area)
        from extents in (
                Measured(metric.Path.Millimeters), Measured(major), Measured(minor))
            .Apply(static (perimeter, largest, smallest) => (Perimeter: perimeter, Major: largest, Minor: smallest))
            .As()
            .ToFin()
        let form = new SectionForm(
            profile.Count,
            profile.Bulges.Count(static bulge => bulge != 0.0),
            radialRatio,
            extents.Perimeter,
            extents.Major,
            extents.Minor)
        from properties in SectionProperties.OfMillimetres(
                areaMm2: area,
                iyyMm4: ix,
                izzMm4: iy,
                jMm4: 4.0 * enclosed * enclosed * wallMm / metric.Path.Millimeters,
                iwMm6: 0.0,
                welyMm3: ix / Math.Max(Math.Abs(box.Min.Y - centroid.Y), Math.Abs(box.Max.Y - centroid.Y)),
                welzMm3: iy / Math.Max(Math.Abs(box.Min.X - centroid.X), Math.Abs(box.Max.X - centroid.X)),
                wplyMm3: weighted.Fold(0.0, (sum, row) => sum + (row.Area * Math.Abs(row.Midpoint.Y - plasticY))),
                wplzMm3: weighted.Fold(0.0, (sum, row) => sum + (row.Area * Math.Abs(row.Midpoint.X - plasticX))),
                avyMm2: weighted.Fold(0.0, static (sum, row) => sum + (row.Area * row.Tangent.Y * row.Tangent.Y)),
                avzMm2: weighted.Fold(0.0, static (sum, row) => sum + (row.Area * row.Tangent.X * row.Tangent.X)),
                radiusMajorMm: Math.Sqrt(Math.Max(ix, iy) / area),
                radiusMinorMm: Math.Sqrt(Math.Min(ix, iy) / area),
                depthMm: height,
                widthMm: width,
                heatedPerimeterMm: metric.Path.Millimeters,
                axisDistanceMm: 0.0,
                shearCentreYMm: 0.0,
                shearCentreZMm: 0.0,
                monosymmetryFactor: 0.0,
                centroidMm: new Vector3(centroid.X, centroid.Y, 0.0),
                form: Some(form),
                key: Key)
        select (properties, form);

    private static double EqualAreaAxis(Seq<(double Area, double Ordinate)> rows, double area) {
        double half = area / 2.0;
        return toSeq(rows.OrderBy(static row => row.Ordinate))
            .Fold((Accrued: 0.0, Axis: 0.0), (held, row) => held.Accrued >= half
                ? held
                : (held.Accrued + row.Area, row.Ordinate))
            .Axis;
    }

    private static Validation<Error, MeasureValue> Measured(double millimetres) =>
        MeasureValue.Of(millimetres, LengthUnit.Millimeter, Key, Some(QuantityType.Length)).ToValidation();

    private static readonly Op Key = Op.Of(name: nameof(TubeSection));
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
                : new ValidationError("tube:tube-tool");
}

[ComplexValueObject]
public sealed partial class TubePolicy {
    public Arr<TubeTool> Tools { get; }
    public BendFormat Format { get; }
    public Angle CollinearAngle { get; }
    public Length MinimumSegment { get; }
    public Dimension RootIterations { get; }
    public Angle MaximumOverbend { get; }
    public UnitInterval MaximumOvality { get; }
    public UnitInterval MaximumThinning { get; }
    public Length CopeAxialSpan { get; }
    public Dimension MaximumCopeStations { get; }
    public Angle WeldSeamExclusion { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Arr<TubeTool> tools,
        ref BendFormat format,
        ref Angle collinearAngle,
        ref Length minimumSegment,
        ref Dimension rootIterations,
        ref Angle maximumOverbend,
        ref UnitInterval maximumOvality,
        ref UnitInterval maximumThinning,
        ref Length copeAxialSpan,
        ref Dimension maximumCopeStations,
        ref Angle weldSeamExclusion) =>
        validationError = ValidityClaim.All(
            !tools.IsEmpty && tools.ForAll(static tool => tool is not null),
            toSeq(tools.GroupBy(static tool => tool.Key)).ForAll(static group => group.Count() == 1),
            format is not null,
            ValidityClaim.Finite(collinearAngle), collinearAngle >= Angle.Zero, collinearAngle < Angle.FromDegrees(180.0),
            ValidityClaim.Finite(minimumSegment), minimumSegment > Length.Zero,
            ValidityClaim.Finite(maximumOverbend), maximumOverbend > Angle.Zero,
            ValidityClaim.Finite(copeAxialSpan), copeAxialSpan > Length.Zero,
            maximumOvality.Value < 1.0, maximumThinning.Value < 1.0,
            maximumCopeStations.Value >= 3,
            ValidityClaim.Finite(weldSeamExclusion),
            weldSeamExclusion >= Angle.Zero, weldSeamExclusion <= Angle.FromDegrees(90.0))
                ? null
                : new ValidationError("tube:tube-policy");
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
                : new ValidationError("tube:tube-run");
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
                : new ValidationError("tube:tube-command");
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

public sealed record TubeEvidence(
    TubeFormKind Process,
    TubeSection Section,
    MaterialSpec Material,
    ProcessBudget.Formed Forming,
    Seq<TubeBend> Bends,
    double TerminalFeedMm,
    double NominalCenterlineMm,
    double DevelopedLengthMm,
    double CutLengthMm);
public sealed record BendProgram(TubeEvidence Evidence, ContentKey Key);

[SmartEnum<string>]
public sealed partial class RollAxis {
    public static readonly RollAxis X = new("x",
        static properties => properties.Wely.CubicMillimetres(), static properties => properties.Depth.Millimetres());
    public static readonly RollAxis Y = new("y",
        static properties => properties.Welz.CubicMillimetres(), static properties => properties.Width.Millimetres());

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
        section.Properties.Width.Millimetres() * depth / (radius * section.GoverningThicknessMm));

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
                ? null
                : new ValidationError("tube:roll-section");

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
            out RollSection section).Admitted(section);
}

[ComplexValueObject]
public sealed partial class RollPolicy {
    public ReciprocalLength MaximumCurvatureIncrement { get; }
    public Dimension MaximumPasses { get; }
    public Ratio SpringbackFactor { get; }
    public PositiveMagnitude TorqueSafetyFactor { get; }
    public Area GapPerCurvature { get; }
    public PositiveMagnitude MaximumDistortion { get; }
    public Dimension RootIterations { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref ReciprocalLength maximumCurvatureIncrement,
        ref Dimension maximumPasses,
        ref Ratio springbackFactor,
        ref PositiveMagnitude torqueSafetyFactor,
        ref Area gapPerCurvature,
        ref PositiveMagnitude maximumDistortion,
        ref Dimension rootIterations) =>
        validationError = ValidityClaim.All(
            ValidityClaim.Finite(maximumCurvatureIncrement), maximumCurvatureIncrement > ReciprocalLength.Zero,
            ValidityClaim.Finite(gapPerCurvature), gapPerCurvature > Area.Zero,
            ValidityClaim.Finite(springbackFactor), springbackFactor >= Ratio.Zero)
                ? null
                : new ValidationError("tube:roll-policy");
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
                : new ValidationError("tube:roll-run");
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
public sealed record RollEvidence(
    RollSection Section,
    RollAxis Axis,
    MaterialSpec Material,
    ProcessBudget.Formed Forming,
    Seq<RollPass> Passes,
    double DevelopedLengthMm,
    double MaximumDistortion,
    double TorqueMarginNm);
public sealed record RollSchedule(RollEvidence Evidence, ContentKey Key);

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
public sealed record CopeEvidence(int Crossings, int Segments, Seq<CopeProjection> Projection, Option<Distortion> Distortion);
public sealed record CopePattern(Seq<Loop> Curves, CopeEvidence Evidence, ContentKey Key);

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

    public sealed record Formed(BendProgram Program) : TubeResult;
    public sealed record Rolled(RollSchedule Schedule) : TubeResult;
    public sealed record Coped(CopePattern Pattern) : TubeResult;

    public ContentKey Key => Map(
        formed: static value => value.Program.Key,
        rolled: static value => value.Schedule.Key,
        coped: static value => value.Pattern.Key);
}

```

## [03]-[TUBE_PROGRAM]

- Owner: `TubeProgram` owns every operation dispatch, the bend and roll passes, cope generation, developed-chain projection, and the canonical preimage; the vocabulary cluster above owns the values every arm consumes.
- Law: a cope station's residual is a QUADRATIC in the axial coordinate, so its two branch ends are closed-form roots. A bracketed root-find run twice per station burned the iteration budget the page reserves for the genuinely transcendental elastic-recovery law, and a bracket that failed to straddle silently dropped a station the algebra always answers.
- Law: a chain point resolves to the crossing that PRODUCED it through one station index built per cope on the admitted quantum — exact equality against a rounded grid station discarded the intersection walk's own provenance and rescanned the grid once per point.
- Law: a section refuses on zero metal area where the degeneracy is, so no NaN second moment reaches the machine-capacity comparison that reads it as within capacity.
- Exemption: the barycentric solve and the quadratic root pair are bounded numeric kernels — the kernel publishes no barycentric triangle query, so the solve stays local with a scale-relative degeneracy gate.
- Boundary: intersection provenance and atlas provenance stay intact through sectioned cope projection; developed islands carry their chart identity and no arm re-derives a crossing.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using LanguageExt;
using LanguageExt.Common;
using MathNet.Numerics.RootFinding;
using Rasm.Domain;
using Rasm.Element.Composition;
using Rasm.Element.Projection;
using Rasm.Element.Properties;
using Rasm.Fabrication.Kinematics;
using Rasm.Fabrication.Process;
using Rasm.Meshing;
using Rasm.Numerics;
using Rasm.Parametric;
using Rasm.Processing;
using Rhino.Geometry;
using Thinktecture;
using UnitsNet;
using UnitsNet.Units;
using Vector3 = Rasm.Element.Graph.Vector3;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Forming;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class TubeProgram {
    public static Fin<TubeResult> Apply(TubeOp operation) => operation is null
        ? Fin.Fail<TubeResult>(new KernelFault.InvalidValue("tube", "tube:operation"))
        : operation.Switch(
            form: static value => Form(value.Run, value.Kind, value.Machine).Map<TubeResult>(static result => new TubeResult.Formed(result)),
            roll: static value => Roll(value.Run, value.Machine).Map<TubeResult>(static result => new TubeResult.Rolled(result)),
            cope: static value => Cope(value.Source).Map<TubeResult>(static result => new TubeResult.Coped(result)));

    private static Fin<BendProgram> Form(TubeRun run, TubeFormKind kind, ProcessEnvelope.Bender machine) =>
        run is null || kind is null || machine is null
            ? Fin.Fail<BendProgram>(new KernelFault.InvalidValue("tube", "tube:form"))
            : from points in Normalize(run.Centerline, run.Policy, run.Tolerance)
              from bends in toSeq(Enumerable.Range(1, Math.Max(0, points.Count - 2)))
                  .Traverse(index => BendOf(index, points, run, kind).ToValidation()).As().ToFin()
              let requiredDies = bends.Fold(Set<string>(), static (keys, bend) => keys.Add(bend.ToolKey)).Count
              from _machine in ValidMachine(machine, run.ClrMm, requiredDies)
              from result in Project(points, bends, run, kind)
              select result;

    private static Fin<Arr<Point3d>> Normalize(Arr<Point3d> source, TubePolicy policy, Context tolerance) =>
        toSeq(source.Skip(1)).FoldM<Fin, Seq<Point3d>>(
                Seq(source[0]),
                (held, point) => held.Last
                    .ToFin(new GeometryFault.DegenerateInput(Kind.Polyline, None, "tube:centerline-empty"))
                    .Bind(prior => point.DistanceTo(prior) <= Math.Max(policy.MinimumSegment.Millimeters, tolerance.Absolute.Value)
                        ? Fin.Fail<Seq<Point3d>>(new GeometryFault.DegenerateInput(Kind.Polyline, None, "tube:centerline-zero"))
                        : held.Count < 2
                            ? Fin.Succ(held.Add(point))
                            : from angle in AngleAt(held[^2], held[^1], point)
                              select angle <= policy.CollinearAngle.Degrees
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
               let majorMm = run.Section.Form.Major.Millimetres()
               from _direction in directions && bendDeg is > 0.0 and < 180.0
                    && run.ClrMm >= run.Forming.MinBendRadiusFactor * majorMm
                ? Fin.Succ(unit)
                : Fin.Fail<Unit>(FabricationFault.MinBendRadiusViolated(index, run.ClrMm,
                    run.Forming.MinBendRadiusFactor * majorMm))
               from tool in ToolOf(run, kind, feed)
               from command in Springback(bendDeg, run.ClrMm, run, kind)
               from quality in Quality(run, tool, bendDeg, points, index)
               from _feed in feed >= Math.Max(run.Policy.MinimumSegment.Millimeters,
                       kind.MinimumStraight(tool, run.Policy))
                    ? Fin.Succ(unit)
                    : Fin.Fail<Unit>(new KernelFault.InvalidValue("tube", $"tube:straight:{index}:{feed:0.###}"))
               let neutralRadius = run.ClrMm + ((run.Forming.KFactor - 0.5) * run.Section.WallMm)
               let neutralArc = Angle.FromDegrees(bendDeg).Radians * neutralRadius
               let force = Force.FromNewtons(
                   kind.ForceFactor * run.Forming.FlowStressMpa
                   * run.Section.Properties.Wely.CubicMillimetres() / run.ClrMm).Kilonewtons
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
        double ratio = run.Section.Form.Major.Millimetres() / run.Section.WallMm;
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
            .ToFin(new KernelFault.InvalidValue("tube", $"tube:tool:{kind.Key}:{run.ClrMm:0.###}"));
    }

    private static Fin<double> Springback(double bendDeg, double clrMm, TubeRun run, TubeFormKind kind) {
        double fibre = (run.Forming.KFactor - 0.5) * run.Section.WallMm;
        return new ElasticLaw(
            Angle.FromDegrees(bendDeg).Radians * (clrMm + fibre),
            fibre,
            kind.RecoveryFactor * run.Forming.SpringbackRatio
                * 2.0 * run.Material.Mechanical.YieldStrengthMpa
                / (run.Material.Mechanical.ElasticModulusMpa * run.Section.Form.Major.Millimetres()))
            .Commanded(
                bendDeg,
                run.Policy.MaximumOverbend.Degrees,
                run.Tolerance.For(ToleranceLane.Root).Value,
                run.Policy.RootIterations.Value)
            .ToFin(new KernelFault.InvalidValue("tube", "tube:springback"));
    }

    private static Fin<TubeQuality> Quality(TubeRun run, TubeTool tool, double bendDeg, Arr<Point3d> points, int index) {
        double majorMm = run.Section.Form.Major.Millimetres();
        double curvature = majorMm / (2.0 * run.ClrMm);
        double wallRatio = majorMm / run.Section.WallMm;
        double strengthRatio = run.Forming.FlowStressMpa / run.Forming.TensileRm;
        double neutralRadius = run.ClrMm + ((run.Forming.KFactor - 0.5) * run.Section.WallMm);
        double fiberStrain = Math.Log(1.0 + (majorMm / (2.0 * neutralRadius))) * strengthRatio;
        double support = 1.0 + tool.Mandrel.InteriorSupport(tool, majorMm)
            + Math.Max(0.0, Math.Cos(Angle.FromDegrees(tool.WiperRakeDeg).Radians))
            + (tool.PressureAssistKn / tool.CapacityKn) + (tool.BoostMm / majorMm);
        double ovality = curvature * Angle.FromDegrees(bendDeg).Radians * (1.0 + strengthRatio) / support;
        double thinning = fiberStrain * Math.Sqrt(wallRatio) / support;
        return from rotations in toSeq(Enumerable.Range(1, index)).Traverse(bend => Rotation(points, bend).ToValidation()).As().ToFin()
               let weld = run.Section.WeldSeamDeg
            .Map(angle => angle + rotations.Fold(0.0, static (sum, rotation) => sum + rotation))
            .Map(angle => ((angle % 360.0) + 360.0) % 360.0)
               from _ovality in ovality <= Math.Min(run.Policy.MaximumOvality.Value, tool.QualifiedOvality)
                && thinning <= Math.Min(run.Policy.MaximumThinning.Value, tool.QualifiedThinning)
                && fiberStrain <= run.Forming.LimitStrain
                && weld.ForAll(angle => {
                    double seamAxis = Math.Min(angle % 180.0, 180.0 - (angle % 180.0));
                    return seamAxis >= run.Policy.WeldSeamExclusion.Degrees;
                })
                    ? Fin.Succ(unit)
                    : Fin.Fail<Unit>(new KernelFault.InvalidValue("tube", $"tube:quality:{index}:{ovality:0.######}:{thinning:0.######}"))
               select new TubeQuality(ovality, thinning, fiberStrain, run.Forming.LimitStrain - fiberStrain, weld);
    }

    private static Fin<TubeCommand> CommandOf(BendFormat format, TubeCoordinate coordinate) =>
        TubeCommand.Validate(format, format.Project(coordinate), out TubeCommand command).Admitted(command);

    private static Fin<BendProgram> Project(
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
        if (!double.IsFinite(terminal) || terminal < run.Policy.MinimumSegment.Millimeters)
            return Fin.Fail<BendProgram>(new KernelFault.InvalidValue("tube", "tube:terminal-feed"));
        double cut = tangent + terminal + bends.Fold(0.0, static (sum, bend) => sum + bend.NeutralArcMm);
        TubeEvidence evidence = new(
            kind,
            run.Section,
            run.Material,
            run.Forming,
            bends,
            terminal,
            nominal,
            cut - run.LeadAllowanceMm - run.TailAllowanceMm,
            cut);
        return Canonical(evidence).Map(key => new BendProgram(evidence, key));
    }

    private static Fin<RollSchedule> Roll(RollRun run, ProcessEnvelope.Roll machine) =>
        run is null || machine is null
            ? Fin.Fail<RollSchedule>(new KernelFault.InvalidValue("tube", "tube:roll"))
            : from _capacity in Seq(
                machine.MaxWidth.Millimeters, machine.MinThickness.Millimeters, machine.MaxThickness.Millimeters, machine.Torque.NewtonMeters)
                        .ForAll(static value => double.IsFinite(value) && value > 0.0)
                    && machine.MaxThickness >= machine.MinThickness && machine.Stations >= 3
                        ? Fin.Succ(unit)
                        : Fin.Fail<Unit>(new KernelFault.InvalidValue("tube", "tube:roll-machine"))
              let radius = run.TargetRadius.Millimeters
              let depth = run.Axis.Depth(run.Section.Properties)
              from _depth in double.IsFinite(depth) && depth > 0.0
                    ? Fin.Succ(unit)
                    : Fin.Fail<Unit>(new KernelFault.InvalidValue("tube", "tube:roll-demand"))
              let targetCurvature = 1.0 / radius
              let requiredPasses = Math.Ceiling(targetCurvature / run.Policy.MaximumCurvatureIncrement.InverseMillimeters)
              from passes in double.IsFinite(requiredPasses)
                    && requiredPasses is >= 1.0 and <= int.MaxValue
                    && requiredPasses <= run.Policy.MaximumPasses.Value
                        ? Fin.Succ((int)requiredPasses)
                        : Fin.Fail<int>(new KernelFault.InvalidValue("tube", $"tube:roll-envelope:passes:{requiredPasses:R}"))
              let yieldCurvature = 2.0 * run.Material.Mechanical.YieldStrengthMpa
                  / (run.Material.Mechanical.ElasticModulusMpa * depth)
              let plasticTorque = Torque.FromNewtonMeters(
                  run.Forming.FlowStressMpa * run.Axis.Modulus(run.Section.Properties)
                  * run.Policy.TorqueSafetyFactor.Value / Length.FromMeters(1.0).Millimeters).NewtonMeters
              from rows in toSeq(Enumerable.Range(1, passes)).Traverse(index => {
                  double inputCurvature = (index - 1.0) / passes * targetCurvature;
                  double outputCurvature = index / (double)passes * targetCurvature;
                  double recovery = run.Forming.SpringbackRatio * run.Policy.SpringbackFactor.DecimalFractions;
                  return CommandCurvature(outputCurvature, yieldCurvature, recovery, run.Policy, run.Section.Profile.Tolerance)
                      .Map(commandCurvature => new RollPass(
                      index,
                      inputCurvature == 0.0 ? Option<double>.None : Some(1.0 / inputCurvature),
                      1.0 / commandCurvature,
                      1.0 / outputCurvature,
                      commandCurvature * run.Policy.GapPerCurvature.SquareMillimeters,
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
                    : Fin.Fail<Unit>(new KernelFault.InvalidValue("tube", $"tube:roll-envelope:{peakTorque:0.###}:{machine.Torque.NewtonMeters:0.###}"))
              from _distortion in double.IsFinite(maximumDistortion) && maximumDistortion <= run.Policy.MaximumDistortion.Value
                    ? Fin.Succ(unit)
                    : Fin.Fail<Unit>(new KernelFault.InvalidValue("tube", $"tube:roll-distortion:{maximumDistortion:0.######}"))
              let evidence = new RollEvidence(
                  run.Section,
                  run.Axis,
                  run.Material,
                  run.Forming,
                  rows,
                  radius * run.Sweep.Radians,
                  maximumDistortion,
                  machine.Torque.NewtonMeters - peakTorque)
              from key in Canonical(evidence)
              select new RollSchedule(evidence, key);

    private static Fin<double> CommandCurvature(
        double outputCurvature,
        double yieldCurvature,
        double recovery,
        RollPolicy policy,
        Context tolerance) {
        if (recovery == 0.0)
            return Fin.Succ(outputCurvature);
        double upper = outputCurvature * (1.0 + recovery) + yieldCurvature;
        return Brent.TryFindRoot(
            command => command / (1.0 + (recovery * Math.Min(1.0, command / yieldCurvature))) - outputCurvature,
            outputCurvature,
            upper,
            outputCurvature * tolerance.For(ToleranceLane.Relative).Value,
            policy.RootIterations.Value,
            out double command)
                ? Fin.Succ(command)
                : Fin.Fail<double>(new KernelFault.InvalidValue("tube", "tube:roll-recovery-root"));
    }

    private static Fin<CopePattern> Cope(CopeSource source) => source is null
        ? Fin.Fail<CopePattern>(new KernelFault.InvalidValue("tube", "tube:cope"))
        : source.Switch(
            analytic: static value => AnalyticCope(value),
            sectioned: static value => SectionedCope(value));

    private static Fin<CopePattern> AnalyticCope(CopeSource.Analytic source) {
        if (source.Branch is null || source.Main is null || source.End is null || source.Policy is null || source.Tolerance is null
            || !source.Branch.Family.AnalyticCope || !source.Main.Family.AnalyticCope
            || source.Intersection.Radians is <= 0.0 || source.Intersection.Radians >= Math.PI)
            return Fin.Fail<CopePattern>(new KernelFault.InvalidValue("tube", "tube:cope-analytic"));
        double branchRadius = (source.Branch.Form.Major.Millimetres() + source.Branch.WallMm) / 2.0;
        double mainRadius = (source.Main.Form.Major.Millimetres() + source.Main.WallMm) / 2.0;
        int samples = Math.Max(3, (int)Math.Ceiling(
            2.0 * Math.PI * branchRadius / source.Tolerance.For(ToleranceLane.Chord).Value));
        if (samples > source.Policy.MaximumCopeStations.Value)
            return Fin.Fail<CopePattern>(new KernelFault.InvalidValue("tube", "tube:cope-stations"));
        Fin<Seq<Point3d>> solved = toSeq(Enumerable.Range(0, samples)).Traverse(index => {
            double theta = 2.0 * Math.PI * index / samples;
            double x = branchRadius * Math.Cos(theta);
            double y = branchRadius * Math.Sin(theta);
            double alpha = source.Intersection.Radians;
            double sin = Math.Sin(alpha), cos = Math.Cos(alpha);
            double quadratic = 1.0 - (cos * cos);
            double linear = -2.0 * x * sin * cos;
            double constant = (x * x) + (y * y) - (x * sin * x * sin) - (mainRadius * mainRadius);
            (bool Lower, double LowerZ, bool Upper, double UpperZ) roots =
                Roots(quadratic, linear, constant, source.Policy.CopeAxialSpan.Millimeters);
            return source.End.Select(index, roots.Lower, roots.LowerZ, roots.Upper, roots.UpperZ)
                .Map(z => new Point3d(branchRadius * theta, z, 0.0))
                .ToValidation();
        }).As().ToFin();
        return from points in solved
               from loop in Loop.Admit(points.ToArr(), closed: true, Arr<double>(), source.Tolerance)
               let evidence = new CopeEvidence(samples, samples, Seq<CopeProjection>(), None)
               from key in Canonical(Seq(loop), evidence, source.Tolerance)
               select new CopePattern(Seq(loop), evidence, key);
    }

    private static Fin<CopePattern> SectionedCope(CopeSource.Sectioned source) =>
        from intersection in Intersection.Apply(new IntersectOp.MeshMesh(source.Part.Mesh, source.Tool, source.Intersection))
        from chains in intersection is IntersectResult.Chains crossed
            ? Fin.Succ(crossed)
            : Fin.Fail<IntersectResult.Chains>(new KernelFault.InvalidValue("tube", "tube:cope-intersection"))
        from development in Development.Apply(new DevelopOp.Unroll(source.Part, source.Development))
        from unrolled in development.SwitchPartially(
            @default: static _ => Fin.Fail<DevelopmentResult.Unrolled>(new KernelFault.InvalidValue("tube", "tube:cope-development")),
            unrolled: static value => Fin.Succ(value))
        from edges in ProjectEdges(chains.Table, unrolled.Atlas)
        let stations = Stations(chains.Table, source.Part.Mesh.Tolerance.Absolute.Value)
        from developed in chains.Walked
            .Traverse(chain => DevelopedChain(chain, stations, edges, source.Part.Mesh.Tolerance).ToValidation()).As().ToFin()
        let loops = developed.Bind(static run => run)
        let evidence = new CopeEvidence(
            chains.Table.Rows.Count,
            chains.Table.Segments.Count + chains.Table.Coplanar.Count,
            edges.Bind(static edge => Seq(edge.A, edge.B)),
            Some(unrolled.Atlas.Distortion))
        from key in Canonical(loops, evidence, source.Part.Mesh.Tolerance)
        select new CopePattern(loops, evidence, key);

    private sealed record DevelopedEdge(int CrossingA, int CrossingB, CopeProjection A, CopeProjection B);
    private sealed record DevelopedRun(ChartId Chart, Seq<Point2d> Points);

    private static Fin<Seq<DevelopedEdge>> ProjectEdges(CrossTable table, ChartAtlas atlas) {
        Mesh mesh = atlas.Source.DuplicateNative();
        return toSeq(table.Segments).Traverse(row => (
            from a in CrossUv(row.A, table.Rows[row.A], row.FaceA, mesh, atlas.Source.Tolerance, atlas.Islands)
            from b in CrossUv(row.B, table.Rows[row.B], row.FaceA, mesh, atlas.Source.Tolerance, atlas.Islands)
            from _chart in a.Chart == b.Chart
                ? Fin.Succ(unit)
                : Fin.Fail<Unit>(new GeometryFault.DegenerateInput(Kind.Mesh, None, $"tube:cope-edge-chart:{row.A}:{row.B}"))
            select new DevelopedEdge(row.A, row.B, a, b)).ToValidation()).As().ToFin();
    }

    private static Fin<CopeProjection> CrossUv(
        int index,
        CrossTable.Row crossing,
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
        return Fin.Fail<CopeProjection>(new GeometryFault.DegenerateInput(Kind.Mesh, index, $"tube:cope-key:{index}"));
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
                   .ToFin(new GeometryFault.DegenerateInput(Kind.Mesh, index, $"tube:cope-face:{index}"))
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
                    .ToFin(new GeometryFault.DegenerateInput(Kind.Mesh, None, $"tube:cope-chain-edge:{crossingA}:{crossingB}"))
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
            chain.Points.IsClosed && runs.Count == 1,
            Arr<double>(),
            tolerance).ToValidation()).As().ToFin()
        select loops;

    private static Map<(long X, long Y, long Z), int> Stations(CrossTable table, double quantum) =>
        toSeq(table.Rows)
            .Map((crossing, index) => (Station(crossing.Point.Round(), quantum), index))
            .Fold(Map<(long, long, long), int>(), static (index, row) => index.AddOrUpdate(row.Item1, row.index));

    private static Fin<int> CrossingAt(Point3d point, Map<(long X, long Y, long Z), int> stations, double quantum) =>
        stations
            .Find(Station(point, quantum))
            .ToFin(new GeometryFault.DegenerateInput(Kind.Mesh, None, "tube:cope-chain-provenance"));

    internal static (long X, long Y, long Z) Station(Point3d point, double quantum) => (
        (long)Math.Round(point.X / quantum, MidpointRounding.ToEven),
        (long)Math.Round(point.Y / quantum, MidpointRounding.ToEven),
        (long)Math.Round(point.Z / quantum, MidpointRounding.ToEven));

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
            .ToFin(new GeometryFault.DegenerateInput(Kind.Mesh, None, "tube:cope-chart"));
    }

    private static Fin<Point2d> Uv(UvIsland island, int vertex) =>
        toSeq(island.Vertices).Map((value, index) => (Vertex: value, Index: index))
            .Find(row => row.Vertex == vertex)
            .Map(row => island.Uv[row.Index])
            .ToFin(new GeometryFault.DegenerateInput(Kind.Mesh, vertex, $"tube:cope-vertex:{vertex}"));

    private static Point2d Lerp(Point2d a, Point2d b, double t) => ((1.0 - t) * a) + (t * b);

    private static Fin<(double A, double B, double C)> Barycentric(Point3d point, Point3d a, Point3d b, Point3d c) {
        Vector3d v0 = b - a, v1 = c - a, v2 = point - a;
        double d00 = v0 * v0, d01 = v0 * v1, d11 = v1 * v1, d20 = v2 * v0, d21 = v2 * v1;
        double denominator = (d00 * d11) - (d01 * d01);
        if (!double.IsFinite(denominator) || Math.Abs(denominator) <= EpsilonPolicy.SqrtEpsilon * Math.Max(d00 * d11, 1.0))
            return Fin.Fail<(double, double, double)>(
                new GeometryFault.DegenerateInput(Kind.Mesh, None, "tube:cope-barycentric"));
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
            : Fin.Fail<double>(new GeometryFault.DegenerateInput(Kind.Polyline, index, $"tube:rotation:{index}"));
    }

    private static Fin<double> AngleAt(Point3d before, Point3d at, Point3d after) {
        Vector3d incoming = at - before;
        Vector3d outgoing = after - at;
        return incoming.Unitize() && outgoing.Unitize()
            ? Fin.Succ(Degrees(Math.Acos(Math.Clamp(incoming * outgoing, -1.0, 1.0))))
            : Fin.Fail<double>(new GeometryFault.DegenerateInput(Kind.Polyline, None, "tube:centerline-angle"));
    }

    private static double Degrees(double radians) => radians / Angle.FromDegrees(1.0).Radians;

    private static Fin<Unit> ValidMachine(ProcessEnvelope.Bender machine, double clrMm, int requiredDies) =>
        double.IsFinite(machine.MinClr.Millimeters) && machine.MinClr > Length.Zero
            && double.IsFinite(machine.MaxClr.Millimeters) && machine.MaxClr >= machine.MinClr
            && machine.DieCount >= 0 && requiredDies >= 0
            && (requiredDies == 0 || machine.DieCount >= requiredDies && clrMm >= machine.MinClr.Millimeters && clrMm <= machine.MaxClr.Millimeters)
                ? Fin.Succ(unit)
                : Fin.Fail<Unit>(new KernelFault.InvalidValue("tube", "tube:bender-envelope"));

    internal static CanonicalWriter Frame(TubeEvidence evidence, CanonicalWriter writer) => Write(
            evidence.Section.Profile.CanonicalBytes(writer
                .Discriminant(evidence.Process).Discriminant(evidence.Section.Family).Double(evidence.Section.WallMm)
                .Maybe(evidence.Section.WeldSeamDeg, static (target, value) => target.Double(value))),
            evidence.Section.Properties)
        .Discriminant(evidence.Material.Family).String(evidence.Material.Identity.Grade)
        .Double(evidence.Forming.KFactor).Double(evidence.Forming.TensileRm).Double(evidence.Forming.SpringbackRatio)
        .Double(evidence.Forming.MinBendRadiusFactor).Double(evidence.Forming.FlowStressMpa).Double(evidence.Forming.LimitStrain)
        .Budget(evidence.Forming.Evidence)
        .Rows(evidence.Bends, static (target, bend) => Write(Write(
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
        .Double(evidence.TerminalFeedMm).Double(evidence.NominalCenterlineMm)
        .Double(evidence.DevelopedLengthMm).Double(evidence.CutLengthMm);

    internal static CanonicalWriter Frame(RollEvidence evidence, CanonicalWriter writer) => Write(
            evidence.Section.Profile.CanonicalBytes(writer
                .String(evidence.Section.Key).Discriminant(evidence.Section.Kind).Discriminant(evidence.Axis)
                .Double(evidence.Section.GoverningThicknessMm)),
            evidence.Section.Properties)
        .Discriminant(evidence.Material.Family).String(evidence.Material.Identity.Grade)
        .Double(evidence.Forming.FlowStressMpa).Double(evidence.Forming.TensileRm).Double(evidence.Forming.KFactor)
        .Double(evidence.Forming.SpringbackRatio).Double(evidence.Forming.MinBendRadiusFactor).Double(evidence.Forming.LimitStrain)
        .Budget(evidence.Forming.Evidence)
        .Rows(evidence.Passes, static (target, pass) => target
            .Ordinal(pass.Index)
            .Maybe(pass.InputRadiusMm, static (slot, value) => slot.Double(value))
            .Double(pass.CommandRadiusMm).Double(pass.OutputRadiusMm).Double(pass.GapMm)
            .Double(pass.TorqueNm).Double(pass.SpringbackDeg).Double(pass.Distortion))
        .Double(evidence.DevelopedLengthMm).Double(evidence.MaximumDistortion).Double(evidence.TorqueMarginNm);

    internal static CanonicalWriter Frame(Seq<Loop> loops, CopeEvidence evidence, CanonicalWriter writer) => writer
        .Ordinal(evidence.Crossings).Ordinal(evidence.Segments)
        .Rows(loops, static (target, loop) => loop.CanonicalBytes(target))
        .Rows(evidence.Projection, static (target, row) => target
            .Ordinal(row.Crossing).Ordinal(row.Chart.Value).Double(row.Uv.X).Double(row.Uv.Y))
        .Maybe(evidence.Distortion, static (target, row) => target
            .Double(row.MaxConformal).Double(row.MeanConformal)
            .Double(row.MaxArea).Double(row.MinArea).Double(row.MeanArea)
            .Double(row.MaxQuasiConformal).Ordinal(row.Iterations)
            .Double(row.SolveResidual).Maybe(row.ConvergenceDelta, static (slot, value) => slot.Double(value))
            .Maybe(row.FactorNonZeros, static (slot, value) => slot.Ordinal(value))
            .Maybe(row.LscmEigenvalue, static (slot, value) => slot.Double(value)));

    private static Fin<ContentKey> Canonical(TubeEvidence evidence) => FabricationCanon.Keyed(
        EgressKind.BendProgram, evidence.Section.Profile.Tolerance, writer => Frame(evidence, writer), Key);

    private static Fin<ContentKey> Canonical(RollEvidence evidence) => FabricationCanon.Keyed(
        EgressKind.BendProgram, evidence.Section.Profile.Tolerance, writer => Frame(evidence, writer), Key);

    private static Fin<ContentKey> Canonical(Seq<Loop> loops, CopeEvidence evidence, Context tolerance) =>
        FabricationCanon.Keyed(EgressKind.FlatPattern, tolerance, writer => Frame(loops, evidence, writer), Key);

    private static CanonicalWriter Write(CanonicalWriter writer, TubeCoordinate coordinate) => writer
        .Double(coordinate.FeedMm).Double(coordinate.RotationDeg)
        .Double(coordinate.CommandDeg).Double(coordinate.RadiusMm)
        .Coords(coordinate.Vertex).Coords(coordinate.Incoming).Coords(coordinate.Outgoing);

    private static CanonicalWriter Write(CanonicalWriter writer, SectionProperties properties) => writer
        .Double(properties.Area.SquareMillimetres())
        .Double(properties.Centroid.X.Millimetres()).Double(properties.Centroid.Y.Millimetres())
        .Double(properties.Iyy.QuarticMillimetres()).Double(properties.Izz.QuarticMillimetres())
        .Double(properties.J.QuarticMillimetres())
        .Double(properties.Wely.CubicMillimetres()).Double(properties.Welz.CubicMillimetres())
        .Double(properties.HeatedPerimeter.Millimetres())
        .Double(properties.Width.Millimetres()).Double(properties.Depth.Millimetres())
        .Maybe(properties.Form, static (target, form) => target
            .Double(form.Major.Millimetres()).Double(form.Minor.Millimetres())
            .Ordinal(form.VertexCount).Ordinal(form.CurvedEdges).Double(form.RadialRatio));

    private static readonly Op Key = Op.Of(name: nameof(TubeProgram));
}
```

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
