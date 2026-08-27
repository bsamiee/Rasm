# [BIM_RECONSTRUCTION]

`ReconstructionProjector : IElementProjection` lowers a kernel-segmented point cloud into a shared `Rasm.Element/Graph/delta#GRAPH_DELTA` `GraphDelta` of `Rasm.Element/Graph/element#ELEMENT_GRAPH` `Node.Object` occurrence nodes, each carrying a typed `Pset_Reconstruction` bag bound by a neutral `Rasm.Element/Relations/relation#EDGE_ALGEBRA` `Relationship.Assign` edge, with the `LasIngest` LAS/LAZ decode front.

Reconstruction is a PRIMARY projector, scan-source twin of the `Projection/semantic#SEMANTIC_PROJECTOR` IFC projector: it MINTS neutral rooted element identity through `NodeId.Of(new NodeSeed.Placement())` and records a deterministic IFC `GlobalId` as its 1:1 `ExternalId`, hashed from the `ReconstructionKey` run identity so a re-run at identical fit parameters dedups against its prior pass through the `Review/diff#MODEL_DIFF` federation diff.

Reconstruction is BIM-semantics-only and CONSUME-BY-REFERENCE: `Themis.Las`/`Unofficial.laszip.netstandard` own the LAS/LAZ decode, the kernel owns registration and fit (`Rasm/Processing/register#REGISTRATION` cloud-ICP places the capture in the kernel frame, `Rasm/Processing/segment#SEGMENTATION` partitions it into `SegmentedCloud` rows bounded by the `dotnet:ROBUST_ARRANGEMENT_SUBSTRATE` exact-arithmetic arrangement).

Geometry content keys are the kernel `Rasm.Domain.ContentHash` seed-zero `XxHash128` the shared `Rasm.Element/Projection/address#CONTENT_ADDRESS` `ContentAddress` wraps over the kernel `Rasm/Domain/identity#CONTENT_KEY` `CanonicalWriter` projection, never the upper-stratum `Rasm.Compute` interchange owner.

Fitted primitives are HOST-NEUTRAL — `Node.Object` references ALL geometry by `RepresentationContentHash` content key only (`Body`/`FootPrint`/`Axis`, EACH a kernel `XxHash128` over the `CanonicalWriter` projection of its `Vector3` coordinates), so a `Rasm.Compute` runner resolves the analytical axis/footprint one-hop, never an inline coordinate field on the graph node (no `Node.Object.BoundaryPolygon`/`Axis` member exists) and never a RhinoCommon `Brep`/`Mesh`.

## [01]-[INDEX]

- [02]-[RECONSTRUCTION]: `ReconstructionProjector` folds segmented clouds into a `GraphDelta` of classified occurrence nodes with typed fit evidence.
- [03]-[LAS_INGEST]: `LasIngest.Decode` sniffs compression and folds `.las`/`.laz` bytes into one `LasCloud` the kernel registration consumes, and `LasIngest.Pyramid` draws that carrier's colour-weighted `CloudLevel` detail bands over the shared interchange ratio schedule.

## [02]-[RECONSTRUCTION]

- Owner: `ReconstructionProjector` the `IElementProjection` folding kernel-segmented clouds to a shared `GraphDelta`; `ReconstructionPrimitive` the ONE fit row carrying the columns every fit has — segment, kernel `GeometryHash`, inlier `FitConfidence`, `ReconstructionKey` — beside the closed `PrimitiveForm` union holding shape-specific payload alone, with `PrimitiveAnalytic` the single per-shape projection every consumer reads; `PrimitiveShape` the `[SmartEnum<string>]` discriminant the `ElementClassifier` table keys on; `CaptureLineage` the `[ValueObject<UInt128>]` source-bytes address and `ReconstructionKey` the `[ValueObject<UInt128>]` run identity, two disjoint key spaces with two names; `FitConfidence` the `[ValueObject<double>]` normalized inlier-ratio band; `SizeBand` the classifier's scale floor; `SegmentedCloud` the kernel-registered segment carrier; `ElementClassifier` the frozen shape-to-`IfcClass` projection.
- Cases: `PrimitiveForm` arms `Plane`/`Sphere`/`Cylinder`/`Cone`/`Torus`/`Freeform` ARE the complete efficient-RANSAC shape-detection family with the residual freeform — a primitive family is one arm, one `PrimitiveShape` row, and one `ElementClassifier` entry, never a per-shape fold or a `FitPlane`/`FitCylinder` operation family, and the shared columns sit on the row above the union so no arm can answer one of them differently; `ElementClassifier` rows are the `(shape, IfcDomain, orientation)`→`(IfcClass, predefined, SizeBand)` table, a wall-vs-slab disambiguation one row refines by orientation and scale a row COLUMN rather than a fourth key axis, never an enumerated `switch` arm.
- Entry: `ReconstructionProjector.Project(ProjectionContext ctx)` folds the constructible segments into one `GraphDelta`, seeding `GraphDelta.Empty.Reheader(ctx.Header)` from the app-supplied Header (the scan CRS WKT flows `LasCloud.CrsWkt`→app→`ctx.Header.Reference`, wiring is app-owned); a PRIMARY projector IGNORES `ctx.ElementIds` and PUBLISHES the rooted ids it mints for an aspect projector (`Rasm.Materials/Projection/component`) to attach `Associate` edges against; `Fin<T>` aborts on an unregistered segment (`Model/faults#FAULT_BAND` `BimFault.Refused` with `BimReason.Capability`) or a shape the classifier places at neither a table row nor its scale band (`BimFault.Refused` with `BimReason.Unmapped`), each typed case lifted BARE onto the result (the `Fault`-derived case IS the `Error`, no `.ToError()` hop), while the shared assembly capture preserves any unknown thrown error exactly.
- Auto: `Project` reads each `SegmentedCloud` already fitted and registered by the kernel, so the fold NEVER re-fits geometry in-process; a `segment.Geometry.IsPending` handle is an unregistered capture faulted `BimFault.Refused` with `BimReason.Capability`. `ReconstructionContext.BiasOf` governs first — an `AsprsBias.Excluded` class is refused by the explicit `Project` filter before authoring, a `Pin` class short-circuits the table because its IFC landing is shape-independent — else `ElementClassifier.Classify` keys the frozen table on the EFFECTIVE `IfcDomain` (the bias domain when present, else the context discipline) and the `FitOrientation` the fit's own published datum selects, where a planar patch reads `OrientationOfNormal` (a vertical normal is a horizontal slab) and a swept solid reads `OrientationOfAxis` (a vertical axis is a vertical column), the two mappings inverse, and the resolved row's `SizeBand` then gates the fit's gauge so a 6 mm cylinder never lands `IfcClass.Column`; EVERY landing admits through the one `Model/elements#IFC_CLASS` `IfcClass.AdmitPredefined` per-token egress gate against `ctx.Header.Schema`. `Node.Object` mints a NEUTRAL rooted `NodeId` via `NodeId.Of(new NodeSeed.Placement())` and records the deterministic `ParserIfc.HashGlobalID` IFC `GlobalId` as its 1:1 `ExternalId`; ALL geometry rides the `RepresentationContentHash` keyed map (`Body`/`FootPrint`/`Axis`) so `Rasm.Compute` resolves the analytical axis/footprint one-hop, never a node coordinate field; the typed `Pset_Reconstruction` bag carries fit evidence as `PropertyValue` and binds to the occurrence through a `Relationship.Assign(AssignKind.PropertyDefinition)` edge the shared `Bake` folds.
- Output: `GraphDelta` is the projector's whole contribution, the merge the shared `Assemble` folds with sibling deltas onto a `Genesis` seed; the `ReconstructionPrimitive` row and its `PrimitiveForm` payload are the typed fit evidence, the `Pset_Reconstruction` bag the per-element review record a `Persistence`/`Compute` `ByProperty` read selects below-floor elements on, and the deterministic `ExternalId` joins a re-reconstructed element to its prior pass and its as-designed counterpart across the federation diff — no generic `IFitResult` abstraction, the union arms stay typed per primitive family.
- Packages: `Rasm.Element` (the shared `Node`/`NodeId`/`GraphDelta`/`Relationship`/`Classification`/`PredefinedType`/`PropertyBag`/`PropertyValue`/`MeasureValue`/`Dimension`/`RepresentationContentHash`/`AxisCurve`/`SchemaSpan`, the `IElementProjection`/`ProjectionContext` contract, and the contract-owned host-neutral `Graph/element#NODE_MODEL` `Vector3` coordinate with its `Dot`/`Unit`/`UnitX`/`UnitZ` algebra the orientation classifier folds — the contract owns the analytical `Vector3` the way it owns `Dimension`, and no kernel `Vector3` exists), `Rasm` (the `GeometryHandle` registration handle, the `Domain.ContentHash` seed-zero `XxHash128`, and the `Rasm/Domain/identity#CONTENT_KEY` `CanonicalWriter`, consumed by reference; the kernel `Rasm.Numerics` coordinate is the RhinoCommon `Vector3d` this host-neutral projection never touches), GeometryGymIFC_Core (`ParserIfc.HashGlobalID` the deterministic GlobalId codec), Thinktecture.Runtime.Extensions (`[Union]`/`[SmartEnum]`/`[ValueObject]`), LanguageExt.Core (`Fin`/`Seq`/`Map`/`Option`).
- Growth: a new fitted primitive is one `PrimitiveForm` arm carrying its analytic parameters, one `PrimitiveAnalytic` arm on the single per-shape dispatch, one `PrimitiveShape` row, and one `ElementClassifier` entry — the fold and classifier resolve it with no new operation; a new classification rule is one `ElementClassifier` row keyed on `(PrimitiveShape, IfcDomain, FitOrientation)` and a new scale floor one `SizeBand` value on the rows that carry it; a repeated identical fit shares ONE `GeometryHash` so the content-keyed blob store dedups the geometry with no parallel type-instance; a new confidence dimension is one `Pset_Reconstruction` row; a new discipline bias is one `BiasOf` arm with the `ElementClassifier` rows it resolves to (a bias arm with no matching rows steers a segment into an empty domain and faults `recon-shape-miss`), a shape-independent site class one `Pin` row, a non-constructible class one `AsprsBias.Excluded` row — `AsprsBias` is the one growth surface for all three; never a per-shape `Node.Object` subtype or a second fit-evidence model.
- Boundary: reconstruction is the LAST fold to a shared `Node.Object`, never a geometry kernel — kernel cloud-ICP registration, plane/cylinder segmentation, and exact-arithmetic arrangement are consumed by reference, never re-minted here; both lineage keys compose the kernel `Rasm.Domain.ContentHash` seed-zero `XxHash128` through the shared `CanonicalWriter`, never the upper-stratum `Rasm.Compute` interchange owner (a `Rasm.Bim`→`Rasm.Compute` reference inverts the strata DAG) or a second hasher; ALL fitted geometry rides the `RepresentationContentHash` keyed map (`Body`/`FootPrint`/`Axis`), so the shared `Node.Object` carries no inline coordinate field, no RhinoCommon `Brep`/`Mesh`, and no stored `GeometryHandle` — host-neutral by construction; lineage is TWO axes with two names and two consumers — `CaptureLineage` addresses the source bytes and is what the `SourceCloud` row publishes and a re-fetch resolves, `ReconstructionKey` identifies one fit run under its own parameters and is what the `ReconstructionRun` row publishes and the deterministic `ExternalId` hashes from; one value type over both key spaces let the advertised re-fetch join cite a key carrying fit parameters in its preimage, which no store can answer; the rooted `NodeId` is the NEUTRAL kernel-minted id and the IFC `GlobalId` is the node's `ExternalId`, a deterministic mint giving re-run dedup without making the GUID the node identity; a reconstructed element is a `Node.Object` on the same generic `Classification`/`PredefinedType` axes an IFC-ingested element carries, so `Model/query` and `Review/validation` read it with no second selection surface; fit evidence rides the typed `Pset_Reconstruction` `PropertyValue` bag the shared property store owns; an unmapped shape faults `BimFault.Refused` with `BimReason.Unmapped` and an unregistered segment `BimFault.Refused` with `BimReason.Capability`, so an unclassifiable scan never silently produces a half-built model, distinct from the KNOWN-non-constructible ASPRS classes the `BiasOf` policy excludes by explicit filter before authoring — a deliberate policy row, never a dropped fault; the classifier's honest reach ends at per-segment single-primitive evidence, and four adjudicated NEGATIVES hold instead of fabricated rows — a Controls instrument publishes no primitive signature (a sensor, actuator, or controller is a fitting-scale blob any small fixture matches, and no ASPRS class biases a segment into Controls, so a Controls-disciplined context resolves through the fallback lanes), a stair is a repeated-tread COMPOSITION no single fit expresses and the kernel publishes no repetition signature, a railing's discriminant is guard height above a walking surface no per-segment fit datum carries (absolute Z is published, the floor it stands off is not), and a door or window is a point-ABSENCE void whose hole topology the single-ring `BoundaryPolygon` does not carry — each lane re-opens only when the kernel mints the evidence it needs (a composed repetition signature, a floor-relative datum, a hole-bearing ring), never through a row whose key cannot honestly discriminate it.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Globalization;
using GeometryGym.Ifc;
using LanguageExt;
using Rasm;
using Rasm.Domain;
using Rasm.Bim.Model;
using Rasm.Element.Classification;
using Rasm.Element.Graph;
using Rasm.Element.Projection;
using Rasm.Element.Properties;
using Rasm.Element.Relations;
using Thinktecture;
using static LanguageExt.Prelude;
using ReleaseVersion = Rasm.Element.Graph.ReleaseVersion;

using Rasm.Spatial;

namespace Rasm.Bim;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class PrimitiveShape {
    public static readonly PrimitiveShape Plane    = new("plane");
    public static readonly PrimitiveShape Sphere   = new("sphere");
    public static readonly PrimitiveShape Cylinder = new("cylinder");
    public static readonly PrimitiveShape Cone     = new("cone");
    public static readonly PrimitiveShape Torus    = new("torus");
    public static readonly PrimitiveShape Freeform = new("freeform");
}

public enum FitOrientation : byte { Any = 0, Horizontal = 1, Vertical = 2, Inclined = 3 }

[Union]
public abstract partial record AsprsBias {
    public sealed record Excluded : AsprsBias;

    public sealed record Constructed(Option<IfcDomain> Domain, Option<(IfcClass Class, string Predefined)> Pin) : AsprsBias;
}

[ValueObject<double>]
public sealed partial class FitConfidence {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) =>
        value = double.IsFinite(value) ? Math.Clamp(value, 0.0, 1.0) : 0.0;

    public bool IsBelow(double threshold) => Value < threshold;
}

[ValueObject<UInt128>]
public sealed partial class CaptureLineage {
    public static CaptureLineage Of(ReadOnlyMemory<byte> bytes) =>
        Create(ContentHash.Of(bytes, static (payload, writer) => writer.String("las-capture").Raw(payload.Span)));
}

[ValueObject<UInt128>]
public sealed partial class ReconstructionKey {
    public static ReconstructionKey Of(SegmentedCloud segment, ReconstructionContext context) =>
        Create(ContentAddress.Of((segment, context), context.Distance.Value, static (s, writer) => writer
            .String("recon-run").Raw(s.segment.CloudBytes.Span)
            .Double(s.context.Chord.Value).Double(s.context.Angle.Value)).Value);
}

public readonly record struct SizeBand(double MinimumGauge) {
    public static readonly SizeBand Surface = new(0.25);
    public static readonly SizeBand Member = new(0.05);
    public static readonly SizeBand Fitting = new(0.01);

    public bool Admits(Option<double> gauge) =>
        gauge.Match(Some: measured => measured >= MinimumGauge, None: static () => true);
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct ReconstructionContext(
    IfcDomain Discipline, Tolerance Chord, Tolerance Distance, Tolerance Angle,
    double ConfidenceFloor, double VerticalCosineLimit) {
    public static ReconstructionContext Building => BuildingRows.Value;

    static Tolerance Band(ToleranceLane lane, double value) =>
        Tolerance.Of(lane, value).ThrowIfFail();

    static readonly Lazy<ReconstructionContext> BuildingRows = new(static () => new(
        IfcDomain.Architecture,
        Band(ToleranceLane.Chord, 1e-3), Band(ToleranceLane.Distance, 1e-6), Band(ToleranceLane.Angle, 1e-4),
        0.6, 0.342),
        LazyThreadSafetyMode.ExecutionAndPublication);

    double UprightCosine => Math.Sqrt(1.0 - (VerticalCosineLimit * VerticalCosineLimit));

    public FitOrientation OrientationOfNormal(Vector3 normal) {
        double vertical = Math.Abs(Vector3.Dot(normal.Unit, Vector3.UnitZ));
        return vertical >= UprightCosine      ? FitOrientation.Horizontal
            : vertical <= VerticalCosineLimit ? FitOrientation.Vertical
            : FitOrientation.Inclined;
    }

    public FitOrientation OrientationOfAxis(Vector3 axis) {
        double vertical = Math.Abs(Vector3.Dot(axis.Unit, Vector3.UnitZ));
        return vertical >= UprightCosine      ? FitOrientation.Vertical
            : vertical <= VerticalCosineLimit ? FitOrientation.Horizontal
            : FitOrientation.Inclined;
    }

    static readonly AsprsBias Vegetation    = new AsprsBias.Constructed(Option<IfcDomain>.None, Some((IfcClass.GeographicElement, "VEGETATION")));
    static readonly AsprsBias Tower         = new AsprsBias.Constructed(Option<IfcDomain>.None, Some((IfcClass.ElementAssembly, "MAST")));
    static readonly AsprsBias WireConnector = new AsprsBias.Constructed(Option<IfcDomain>.None, Some((IfcClass.CableFitting, "CONNECTOR")));
    static readonly AsprsBias Refused       = new AsprsBias.Excluded();
    static readonly AsprsBias Unbiased      = new AsprsBias.Constructed(Option<IfcDomain>.None, None);

    public static AsprsBias BiasOf(byte asprsClass) => asprsClass switch {
        2                  => new AsprsBias.Constructed(Some(IfcDomain.Geotechnical), None),
        3 or 4 or 5        => Vegetation,
        6                  => new AsprsBias.Constructed(Some(IfcDomain.Architecture), None),
        7 or 9 or 12 or 18 => Refused,
        11 or 17           => new AsprsBias.Constructed(Some(IfcDomain.Infrastructure), None),
        10                 => new AsprsBias.Constructed(Some(IfcDomain.Infrastructure), Some((IfcClass.Rail, "RAIL"))),
        13 or 14           => new AsprsBias.Constructed(Some(IfcDomain.Electrical), None),
        15                 => Tower,
        16                 => WireConnector,
        _                  => Unbiased,
    };
}

public readonly record struct SegmentedCloud(
    int SegmentId, PrimitiveShape Shape, GeometryHandle Geometry, UInt128 GeometryHash,
    Vector3 Normal, Vector3 Center, Vector3 Axis, Vector3 AxisStart, Vector3 AxisEnd,
    double Radius, double MinorRadius, double HalfAngle, Seq<Vector3> BoundaryPolygon,
    byte DominantClass, int Inliers, int Total, ReadOnlyMemory<byte> CloudBytes, CaptureLineage Capture) {
    public FitConfidence Confidence => FitConfidence.Create(Total > 0 ? (double)Inliers / Total : 0.0);
    public double Residual => Total > 0 ? 1.0 - (double)Inliers / Total : 1.0;
}

public readonly record struct PrimitiveAnalytic(
    PrimitiveShape Shape, Seq<Vector3> Boundary, Option<AxisCurve> Axis,
    Option<Vector3> Normal, Option<Vector3> Direction, Option<double> Gauge);

[Union]
public abstract partial record PrimitiveForm {
    private PrimitiveForm() { }

    public sealed record Plane(Vector3 Normal, Seq<Vector3> Boundary) : PrimitiveForm;
    public sealed record Sphere(Vector3 Center, double Radius) : PrimitiveForm;
    public sealed record Cylinder(Vector3 AxisStart, Vector3 AxisEnd, Vector3 Direction, double Radius) : PrimitiveForm;
    public sealed record Cone(Vector3 AxisStart, Vector3 AxisEnd, Vector3 Direction, double Radius, double HalfAngle) : PrimitiveForm;
    public sealed record Torus(Vector3 AxisStart, Vector3 AxisEnd, Vector3 Direction, double Radius, double MinorRadius) : PrimitiveForm;
    public sealed record Freeform : PrimitiveForm;

    public static PrimitiveForm Of(SegmentedCloud s) => s.Shape.Switch<PrimitiveForm>(
        plane:    () => new Plane(s.Normal, s.BoundaryPolygon),
        sphere:   () => new Sphere(s.Center, s.Radius),
        cylinder: () => new Cylinder(s.AxisStart, s.AxisEnd, s.Axis, s.Radius),
        cone:     () => new Cone(s.AxisStart, s.AxisEnd, s.Axis, s.Radius, s.HalfAngle),
        torus:    () => new Torus(s.AxisStart, s.AxisEnd, s.Axis, s.Radius, s.MinorRadius),
        freeform: () => new Freeform());
}

public readonly record struct ReconstructionPrimitive(
    int SegmentId, UInt128 GeometryHash, FitConfidence Confidence, ReconstructionKey Key, PrimitiveForm Form) {

    public static ReconstructionPrimitive Of(SegmentedCloud s, ReconstructionContext context) =>
        new(s.SegmentId, s.GeometryHash, s.Confidence, ReconstructionKey.Of(s, context), PrimitiveForm.Of(s));

    public PrimitiveAnalytic Analytic => Form.Switch(
        plane:    static p => new PrimitiveAnalytic(PrimitiveShape.Plane, p.Boundary, None, Some(p.Normal), None, Some(Extent(p.Boundary))),
        sphere:   static s => new PrimitiveAnalytic(PrimitiveShape.Sphere, Seq<Vector3>(), None, None, None, Some(s.Radius * 2.0)),
        cylinder: static c => new PrimitiveAnalytic(PrimitiveShape.Cylinder, Seq<Vector3>(), Some(Curve(c.AxisStart, c.AxisEnd, c.Direction)), None, Some(c.Direction), Some(c.Radius * 2.0)),
        cone:     static c => new PrimitiveAnalytic(PrimitiveShape.Cone, Seq<Vector3>(), Some(Curve(c.AxisStart, c.AxisEnd, c.Direction)), None, Some(c.Direction), Some(c.Radius * 2.0)),
        torus:    static t => new PrimitiveAnalytic(PrimitiveShape.Torus, Seq<Vector3>(), Some(Curve(t.AxisStart, t.AxisEnd, t.Direction)), None, Some(t.Direction), Some(t.MinorRadius * 2.0)),
        freeform: static _ => new PrimitiveAnalytic(PrimitiveShape.Freeform, Seq<Vector3>(), None, None, None, None));

    static AxisCurve Curve(Vector3 start, Vector3 end, Vector3 axis) =>
        new(start, end, Math.Abs(Vector3.Dot(axis.Unit, Vector3.UnitZ)) > 0.9 ? Vector3.UnitX : Vector3.UnitZ);

    static double Extent(Seq<Vector3> ring) =>
        ring.IsEmpty
            ? 0.0
            : ring.Fold(
                  (Min: ring[0], Max: ring[0]),
                  static (box, p) => (
                      new Vector3(Math.Min(box.Min.X, p.X), Math.Min(box.Min.Y, p.Y), Math.Min(box.Min.Z, p.Z)),
                      new Vector3(Math.Max(box.Max.X, p.X), Math.Max(box.Max.Y, p.Y), Math.Max(box.Max.Z, p.Z))))
              switch {
                  var box => Math.Max(box.Max.X - box.Min.X, Math.Max(box.Max.Y - box.Min.Y, box.Max.Z - box.Min.Z)),
              };

    public RepresentationContentHash Keys(double tolerance) {
        PrimitiveAnalytic analytic = Analytic;
        RepresentationContentHash body = RepresentationContentHash.Empty.With("Body", GeometryHash);
        RepresentationContentHash surface = analytic.Boundary.IsEmpty
            ? body
            : body.With("FootPrint", ContentAddress.Of(analytic.Boundary, tolerance, static (boundary, writer) => boundary
                .Fold(writer.String("recon-footprint").Ordinal(boundary.Count),
                      static (w, p) => w.Double(p.X).Double(p.Y).Double(p.Z))).Value);
        return analytic.Axis.Match(
            Some: axis => surface.With("Axis", ContentAddress.Of(axis, tolerance, static (a, writer) => writer
                .String("recon-axis")
                .Double(a.Start.X).Double(a.Start.Y).Double(a.Start.Z)
                .Double(a.End.X).Double(a.End.Y).Double(a.End.Z)
                .Double(a.Up.X).Double(a.Up.Y).Double(a.Up.Z)).Value),
            None: () => surface);
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ReconstructionRows {
    public const string Set = "Pset_Reconstruction";
    public static readonly PropertyName FitConfidence = PropertyCategory.Neutral.Row("FitConfidence");
    public static readonly PropertyName Residual = PropertyCategory.Neutral.Row("Residual");
    public static readonly PropertyName Inliers = PropertyCategory.Neutral.Row("Inliers");
    public static readonly PropertyName Total = PropertyCategory.Neutral.Row("Total");
    public static readonly PropertyName AsprsClass = PropertyCategory.Neutral.Row("AsprsClass");
    public static readonly PropertyName NeedsReview = PropertyCategory.Neutral.Row("NeedsReview");
    public static readonly PropertyName PrimitiveShape = PropertyCategory.Neutral.Row("PrimitiveShape");
    public static readonly PropertyName SourceSegment = PropertyCategory.Neutral.Row("SourceSegment");
    public static readonly PropertyName SourceCloud = PropertyCategory.Neutral.Row("SourceCloud");
    public static readonly PropertyName ReconstructionRun = PropertyCategory.Neutral.Row("ReconstructionRun");
}

public static class ElementClassifier {
    static readonly Map<(PrimitiveShape Shape, Option<IfcDomain> Domain, FitOrientation Orientation), (IfcClass Class, string Predefined, SizeBand Band)> Table =
        Map(
            ((PrimitiveShape.Plane,    None,                          FitOrientation.Vertical),   (IfcClass.Wall,                "STANDARD",         SizeBand.Surface)),
            ((PrimitiveShape.Plane,    None,                          FitOrientation.Horizontal), (IfcClass.Slab,                "FLOOR",            SizeBand.Surface)),
            ((PrimitiveShape.Plane,    None,                          FitOrientation.Inclined),   (IfcClass.Roof,                "FREEFORM",         SizeBand.Surface)),
            ((PrimitiveShape.Plane,    None,                          FitOrientation.Any),        (IfcClass.Covering,            "CLADDING",         SizeBand.Surface)),
            ((PrimitiveShape.Plane,    Some(IfcDomain.Infrastructure), FitOrientation.Any),       (IfcClass.Pavement,            "FLEXIBLE",         SizeBand.Surface)),
            ((PrimitiveShape.Plane,    Some(IfcDomain.Geotechnical),   FitOrientation.Any),       (IfcClass.GeotechnicalStratum, "SOLID",            SizeBand.Surface)),
            ((PrimitiveShape.Plane,    Some(IfcDomain.Electrical),     FitOrientation.Any),       (IfcClass.CableCarrierSegment, "CABLETRAYSEGMENT", SizeBand.Fitting)),
            ((PrimitiveShape.Sphere,   None,                          FitOrientation.Any),        (IfcClass.BuildingElementProxy, "ELEMENT",         SizeBand.Fitting)),
            ((PrimitiveShape.Sphere,   Some(IfcDomain.HvacFire),       FitOrientation.Any),       (IfcClass.FlowTerminal,        "NOTDEFINED",       SizeBand.Fitting)),
            ((PrimitiveShape.Sphere,   Some(IfcDomain.Plumbing),       FitOrientation.Any),       (IfcClass.PipeFitting,         "NOTDEFINED",       SizeBand.Fitting)),
            ((PrimitiveShape.Cylinder, None,                          FitOrientation.Vertical),   (IfcClass.Column,              "COLUMN",           SizeBand.Member)),
            ((PrimitiveShape.Cylinder, None,                          FitOrientation.Horizontal), (IfcClass.Beam,                "BEAM",             SizeBand.Member)),
            ((PrimitiveShape.Cylinder, Some(IfcDomain.Structural),     FitOrientation.Vertical),  (IfcClass.Pile,                "BORED",            SizeBand.Member)),
            ((PrimitiveShape.Cylinder, Some(IfcDomain.Geotechnical),   FitOrientation.Vertical),  (IfcClass.Borehole,            "NOTDEFINED",       SizeBand.Member)),
            ((PrimitiveShape.Cylinder, Some(IfcDomain.Electrical),     FitOrientation.Any),       (IfcClass.CableSegment,        "CONDUCTORSEGMENT", SizeBand.Fitting)),
            ((PrimitiveShape.Cylinder, Some(IfcDomain.HvacFire),       FitOrientation.Any),       (IfcClass.FlowSegment,         "NOTDEFINED",       SizeBand.Fitting)),
            ((PrimitiveShape.Cylinder, Some(IfcDomain.Plumbing),       FitOrientation.Any),       (IfcClass.PipeSegment,         "RIGIDSEGMENT",     SizeBand.Fitting)),
            ((PrimitiveShape.Cone,     None,                          FitOrientation.Any),        (IfcClass.Roof,                "FREEFORM",         SizeBand.Surface)),
            ((PrimitiveShape.Cone,     Some(IfcDomain.HvacFire),       FitOrientation.Any),       (IfcClass.FlowFitting,         "NOTDEFINED",       SizeBand.Fitting)),
            ((PrimitiveShape.Cone,     Some(IfcDomain.Plumbing),       FitOrientation.Any),       (IfcClass.PipeFitting,         "TRANSITION",       SizeBand.Fitting)),
            ((PrimitiveShape.Torus,    Some(IfcDomain.HvacFire),       FitOrientation.Any),       (IfcClass.FlowFitting,         "NOTDEFINED",       SizeBand.Fitting)),
            ((PrimitiveShape.Torus,    Some(IfcDomain.Plumbing),       FitOrientation.Any),       (IfcClass.PipeFitting,         "BEND",             SizeBand.Fitting)),
            ((PrimitiveShape.Freeform, None,                          FitOrientation.Any),        (IfcClass.BuildingElementProxy, "ELEMENT",         SizeBand.Fitting)),
            ((PrimitiveShape.Freeform, Some(IfcDomain.Geotechnical),   FitOrientation.Any),       (IfcClass.GeotechnicalStratum, "SOLID",            SizeBand.Surface)),
            ((PrimitiveShape.Freeform, Some(IfcDomain.Infrastructure), FitOrientation.Any),       (IfcClass.Course,              "PAVEMENT",         SizeBand.Surface)),
            ((PrimitiveShape.Freeform, Some(IfcDomain.Electrical),     FitOrientation.Any),       (IfcClass.CableSegment,        "CONDUCTORSEGMENT", SizeBand.Fitting)));

    public static Fin<(IfcClass Class, PredefinedType Predefined)> Classify(
        ReconstructionPrimitive primitive, SegmentedCloud segment, ReconstructionContext context, ReleaseVersion schema) =>
        ReconstructionContext.BiasOf(segment.DominantClass).Switch(
            state: (primitive.Analytic, context, schema),
            excluded: static (s, _) => Fin.Fail<(IfcClass Class, PredefinedType Predefined)>(
                new BimFault.Refused(s.key, BimScope.Reconstruct, BimReason.Capability, string.Join(':', new object?[] { "recon-unregistered", s.Analytic.Shape.Key, "asprs-excluded" }))),
            constructed: static (s, row) => row.Pin.Match(
                Some: pin => Admit(pin.Class, pin.Predefined, s.schema, s.key),
                None: () => Tabled(s.Analytic, row.Domain.IfNone(s.context.Discipline), s.context, s.schema, s.key)));

    static Fin<(IfcClass Class, PredefinedType Predefined)> Tabled(
        PrimitiveAnalytic analytic, IfcDomain domain, ReconstructionContext context, ReleaseVersion schema) {
        FitOrientation orientation = (analytic.Normal.Map(normal => context.OrientationOfNormal(normal))
                | analytic.Direction.Map(direction => context.OrientationOfAxis(direction)))
            .IfNone(FitOrientation.Any);
        return (Table.Find((analytic.Shape, Some(domain), orientation))
                | Table.Find((analytic.Shape, Some(domain), FitOrientation.Any))
                | Table.Find((analytic.Shape, Option<IfcDomain>.None, orientation))
                | Table.Find((analytic.Shape, Option<IfcDomain>.None, FitOrientation.Any)))
            .ToFin(new BimFault.Refused(BimScope.Reconstruct, BimReason.Unmapped, string.Join(':', new object?[] { "recon-shape-miss", analytic.Shape.Key, domain.ToString(), orientation.ToString() })))
            .Bind(row => row.Band.Admits(analytic.Gauge)
                ? Admit(row.Class, row.Predefined, schema)
                : Fin.Fail<(IfcClass, PredefinedType)>(new BimFault.Refused(BimScope.Reconstruct, BimReason.Unmapped, string.Join(':', new object?[] { "recon-below-band", analytic.Shape.Key, row.Class.Key, analytic.Gauge.IfNone(0.0).ToString(CultureInfo.InvariantCulture) }))));
    }

    static Fin<(IfcClass Class, PredefinedType Predefined)> Admit(IfcClass @class, string predefined, ReleaseVersion schema) =>
        @class.AdmitPredefined(predefined, "", schema).Map(token => (@class, PredefinedType.Create(token)));
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class ReconstructionProjector(Seq<SegmentedCloud> segments, ReconstructionContext context) : IElementProjection {
    public Fin<GraphDelta> Project(ProjectionContext ctx) =>
        segments.Filter(static s => ReconstructionContext.BiasOf(s.DominantClass) is AsprsBias.Constructed)
            .FoldM(
                GraphDelta.Empty.Reheader(ctx.Header),
                (delta, segment) => Author(segment, ctx).Map(delta.Merge))
            .As();

    Fin<GraphDelta> Author(SegmentedCloud segment, ProjectionContext ctx) =>
        segment.Geometry.IsPending
            ? Fin.Fail<GraphDelta>(new BimFault.Refused(ctx.Key, BimScope.Reconstruct, BimReason.Capability, string.Join(':', new object?[] { "recon-unregistered", segment.SegmentId.ToString(CultureInfo.InvariantCulture) })))
            : Fin.Succ(ReconstructionPrimitive.Of(segment, context)).Bind(primitive =>
                ElementClassifier.Classify(primitive, segment, context, ctx.Header.Schema, ctx.Key)
                    .Bind(row => Build(primitive, segment, row, ctx)));

    Fin<GraphDelta> Build(ReconstructionPrimitive primitive, SegmentedCloud segment, (IfcClass Class, PredefinedType Predefined) row, ProjectionContext ctx) =>
        ReconstructionPset(primitive, segment, ctx.Header.Tolerance, ctx.Key).Map(bag => {
            NodeId objectId = NodeId.Of(new NodeSeed.Placement());
            Node.Object element = new(
                Id:              objectId,
                Kind:            ObjectKind.Occurrence,
                ExternalId:      Some(ParserIfc.HashGlobalID($"recon:{primitive.Key.Value:X32}")),
                Classification:  row.Class.EntityClass,
                PredefinedType:  row.Predefined,
                Name:            $"{row.Class.Key}-recon-{segment.SegmentId.ToString(CultureInfo.InvariantCulture)}",
                Tag:             segment.SegmentId.ToString(CultureInfo.InvariantCulture),
                Representations: primitive.Keys(ctx.Header.Tolerance),
                History:         None,
                Span:            SchemaSpan.From(ctx.Header.Schema));
            return GraphDelta.Empty.Put(element).Put(bag)
                .Link(new Relationship.Assign(objectId, bag.Id, AssignKind.PropertyDefinition));
        });

    Fin<Node.PropertySet> ReconstructionPset(ReconstructionPrimitive primitive, SegmentedCloud segment, double tolerance) =>
        from confidence in MeasureValue.OfSi(Dimension.Dimensionless, primitive.Confidence.Value)
        from residual in MeasureValue.OfSi(Dimension.Dimensionless, segment.Residual)
        from inliers in MeasureValue.OfSi(Dimension.Dimensionless, segment.Inliers)
        from total in MeasureValue.OfSi(Dimension.Dimensionless, segment.Total)
        from asprs in MeasureValue.OfSi(Dimension.Dimensionless, segment.DominantClass)
        let bag = new PropertyBag(ReconstructionRows.Set, Map<PropertyName, PropertyValue>(
            (ReconstructionRows.FitConfidence,  new PropertyValue.Measure(confidence)),
            (ReconstructionRows.Residual,       new PropertyValue.Measure(residual)),
            (ReconstructionRows.Inliers,        new PropertyValue.Measure(inliers)),
            (ReconstructionRows.Total,          new PropertyValue.Measure(total)),
            (ReconstructionRows.AsprsClass,     new PropertyValue.Measure(asprs)),
            (ReconstructionRows.NeedsReview,    new PropertyValue.Boolean(primitive.Confidence.IsBelow(context.ConfidenceFloor))),
            (ReconstructionRows.PrimitiveShape, new PropertyValue.Enumerated(Seq(primitive.Analytic.Shape.Key), toSeq(PrimitiveShape.Items).Map(static s => s.Key))),
            (ReconstructionRows.SourceSegment,  new PropertyValue.Text(segment.SegmentId.ToString(CultureInfo.InvariantCulture))),
            (ReconstructionRows.SourceCloud,    new PropertyValue.Text(segment.Capture.Value.ToString("X32", CultureInfo.InvariantCulture))),
            (ReconstructionRows.ReconstructionRun, new PropertyValue.Text(primitive.Key.Value.ToString("X32", CultureInfo.InvariantCulture)))),
            InheritanceMode.OccurrenceWins, EvidenceGrade.Derived)
        let probe = new Node.PropertySet(NodeId.Of(new NodeSeed.Placement()), bag)
        select probe with { Id = NodeId.Of(new NodeSeed.Content(probe, tolerance)) };
}
```

## [03]-[LAS_INGEST]

- Owner: `LasCloud` the decoded point carrier — position set (each `Position` a `MathNet.Numerics.LinearAlgebra.Vector<double>` the kernel registration and Compute dense-LA substrate consume without a re-wrap), the per-point ASPRS `Classifications` the segmentation reduces to the `[02]-[RECONSTRUCTION]` `SegmentedCloud.DominantClass` hint, the unit-normalized `Colors` lane a colour-bearing point format carries, and the header facts (`ClassHistogram`, `CountsByReturn`, extrema, integer-grid `Scale`/`Offset`, `PointFormat`, CRS WKT, `CaptureLineage`, count, `Instant`); `CloudLevel` one decimated detail band over that carrier — retained indices, measured point count, meshopt cull sphere, per-level content key; `LasCompression` the `[SmartEnum<string>]` discriminant; `LasIngest` the dual-engine decode fold decoding raw `.las`/`.laz` bytes into the `LasCloud` the kernel registration/segmentation consume AND drawing that carrier's progressive-detail pyramid — `Themis.Las` owns the uncompressed codec, `Unofficial.laszip.netstandard` the compressed codec, `Alimer.Bindings.MeshOptimizer` the point decimation and the sphere bound, the kernel owns the fit; this owner re-mints none.
- Entry: `LasIngest.Decode(ReadOnlyMemory<byte> bytes, Instant at)` dispatches on `LasCompression.Sniff` (the offset-104 public-header byte whose high bit marks LASzip compression), routing the uncompressed leg through `ReadLas` and the compressed leg through `ReadLaz`; `LasIngest.Pyramid(LasCloud cloud, InterchangePolicy policy)` draws the detail bands over the `format#FORMAT_AXIS`-neighbouring `export#EXPORT_PIPELINE` `InterchangePolicy.LodRatios` schedule the mesh pyramid reads, weighting the draw by the `AttributeWeights` `base_color` row so a facade capture keeps its material boundaries; `Fin<T>` traps a malformed header, an unreadable archive, or a degenerate decimation into `Model/faults#FAULT_BAND` their original captured `Error` through `Try.lift`, and a capture whose point count exceeds the int-domain decimator into `BimFault.Refused` with `BimReason.Capability` before any narrowing, the typed case IS the `Error`, never a `.ToError()` hop.
- Auto: `Sniff` selects the engine from the compression marker WITHOUT a full open; `ReadLas` streams the `Themis.Las` `LasReader` over one temp path (byte admission is path-bound — the one shipped `AsyncStreamHandler` is path-constructed), and `ReadLaz` folds the `laszip` decoder over the in-memory stream gating each non-zero C-API status through `Check`; both mask the classification format-correctly (formats 0-5 strip the flag bits `& 0x1F`, formats 6-10 keep the full class byte), fill the `Colors` lane on the colour-bearing formats alone (2/3/5/7/8/10, the 16-bit channel unit-normalized; every other format keeps the typed EMPTY lane rather than a black cloud a colour-weighted draw reads as uniform), read the header facts and the record-`2112` OGC WKT CRS, and assemble one `LasCloud` whose lineage is the kernel `XxHash128` over the raw bytes through the shared `CanonicalWriter` and whose `ClassHistogram` folds in one dense-array pass; `Pyramid` stages the float32 position lane ONCE (meshopt is a float kernel, the carrier a MathNet double the registration consumes) and folds each ratio through `Meshopt.SimplifyPoints`, keeping the RETAINED source indices rather than a copied point set and computing each band's `Meshopt.ComputeSphereBounds` cull sphere over the retained points alone, each level content-keyed off the cloud lineage so the tile pyramid addresses a capture's bands exactly as it addresses a mesh's; the per-point ASPRS classes feed the kernel segmentation reducing them to the per-segment modal `DominantClass`, and the CRS WKT feeds the app's `Header.Reference` `GeoReference` (`Semantics/georeference#GEO_PROJECTION` `ProjNET` leg) so a georeferenced capture lands in the canonical kernel frame.
- Output: `LasCloud` is the decoded scan evidence — point/per-return counts, the `ClassHistogram` computed from the decoded class bytes (evidence the header cannot forge), header extrema and quantization, point-data-record format, colour-lane occupancy, and CRS WKT presence; the `CaptureLineage` over the source bytes joins the reconstructed model back to its capture — the `Pset_Reconstruction` `SourceCloud` row publishes THAT key, so the `Review/diff#MODEL_DIFF` federation diff and reality-capture playback re-fetch the exact LAS/LAZ through a join that closes, where the parameter-derived run key opens one nothing answers. `CloudLevel` is the per-band draw evidence — the MEASURED retained count (never the requested target, which the decimator may undershoot on a sparse capture), the cull sphere a client selects on, and the content key it streams by.
- Packages: `Themis.Las` (the MIT pure-managed uncompressed ASPRS LAS reader over `MathNet.Numerics`), `Unofficial.laszip.netstandard` (the LGPL-2.1 separate-assembly pure-managed LASzip codec — `.laz` arithmetic decode, selective-channel decompression, the `.lax` spatial-index bbox query), `Alimer.Bindings.MeshOptimizer` (the colour-weighted point decimation and the sphere bound), `Rasm` (the kernel `Domain.ContentHash` and the `Rasm/Domain/identity#CONTENT_KEY` `CanonicalWriter`), Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime.
- Growth: a new ASPRS point data record format is one `Themis.Las` `PointTypeMap` row (formats 0-10 share one reader, never a per-format reader family); a compression state is one `LasCompression` row dispatched by `Sniff`; a per-point facet (intensity, return index, GPS time, NIR) is one column the `LasPoint` facet set already carries, the `LasCloud.PointFormat` column announcing which facets a capture holds without a re-decode; a new detail band is one ratio on the shared `InterchangePolicy.LodRatios` column and a new decimation weight one `AttributeWeights` row, both the same policy the mesh pyramid reads — never a cloud-local schedule; a tiled ingest enters through the `laszip` `.lax` `inside_rectangle` windowed path when an index exists; never a re-minted point-cloud decoder, never a hand-rolled point decimator, and never a second hashing scheme over the LAS/LAZ bytes.
- Boundary: `Themis.Las` (`LasReader`/`LasPoint`/`ILasHeader`/`LasVariableLengthRecord`) owns the uncompressed stream and the `laszip` C-API codec the compressed stream, `LasPoint.Position`/`get_coordinates` lifting into the one `MathNet.Numerics.LinearAlgebra.Vector<double>` the kernel registration consumes with no re-wrap, never a hand-rolled LAS/LAZ reader; the LGPL-2.1 `Unofficial.laszip.netstandard` is referenced as a SEPARATE assembly, never ILMerged, so the in-Rhino plugin ALC firebreak holds; `LasIngest` decodes and DRAWS and never fits, registration and segmentation staying the kernel's by reference — `Meshopt.SimplifyPoints` owns the colour-weighted decimation and `Meshopt.ComputeSphereBounds` the cull sphere, so a hand-rolled voxel thin, an octree decimator, or a hand-computed bounding sphere beside them is the deleted form, and a level carrying no bound is a residency band with no selection criterion; the CRS WKT VLR feeds the app's `GeoReference` (`Semantics/georeference#GEO_PROJECTION` `ProjNET` leg), never a codec-local reprojection; the source-cloud content key composes the kernel `Rasm.Domain.ContentHash` seed-zero `XxHash128`, never a second hasher or the upper-stratum `Rasm.Compute` interchange owner; the decoded `LasPoint`/`laszip_point` types never leak past this fold — internal code holds the canonical `LasCloud`/`SegmentedCloud` per the boundary-mapping law.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using LanguageExt;
using LASzip.Net;
using MathNet.Numerics.LinearAlgebra;
using MeshOptimizer;
using NodaTime;
using Rasm.Domain;
using Rasm.Element.Classification;
using Rasm.Element.Graph;
using Rasm.Element.Projection;
using Rasm.Element.Properties;
using Rasm.Element.Relations;
using Themis.Las;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Bim;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LasCompression {
    public static readonly LasCompression Uncompressed = new("las");
    public static readonly LasCompression Compressed   = new("laz");

    public static LasCompression Sniff(ReadOnlySpan<byte> bytes) =>
        bytes.Length > 104 && (bytes[104] & 0x80) != 0 ? Compressed : Uncompressed;
}

public readonly record struct CloudWindow(double MinX, double MinY, double MaxX, double MaxY) {
    public bool Holds(double x, double y) => x >= MinX && x <= MaxX && y >= MinY && y <= MaxY;
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record LasCloud(
    ReadOnlyMemory<Vector<double>> Positions, ReadOnlyMemory<byte> Classifications, ReadOnlyMemory<float> Colors,
    Map<byte, ulong> ClassHistogram, ReadOnlyMemory<ulong> CountsByReturn,
    Vector3 Min, Vector3 Max, Vector3 Scale, Vector3 Offset,
    Option<string> CrsWkt, CaptureLineage Lineage, byte PointFormat, ulong PointCount, Instant At);

public sealed record CloudLevel(
    int Level, double TargetRatio, int PointCount, ReadOnlyMemory<uint> Indices, Bounds Bounds, UInt128 ContentKey);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class LasIngest {
    const string ColorChannel = "base_color";

    static bool Colored(byte pointFormat) => pointFormat is 2 or 3 or 5 or 7 or 8 or 10;

    public static Fin<LasCloud> Decode(ReadOnlyMemory<byte> bytes, Instant at, Option<CloudWindow> window = default) =>
        Decoded(LasCompression.Sniff(bytes.Span), bytes, window, at);

    static Fin<LasCloud> Decoded(LasCompression codec, ReadOnlyMemory<byte> bytes, Option<CloudWindow> window, Instant at) =>
        codec == LasCompression.Compressed
            ? Trap(codec, () => ReadLaz(bytes, window, at))
            : Trap(codec, () => ReadLas(bytes, window, at)).Bind(static read => read);

    static Fin<T> Trap<T>(LasCompression codec, Func<T> read) =>
        Try.lift(read).Run().Bind(static inner => inner);

    public static Fin<Seq<CloudLevel>> Pyramid(LasCloud cloud, InterchangePolicy policy) =>
        cloud.PointCount > int.MaxValue
            ? Fin.Fail<Seq<CloudLevel>>(new BimFault.Refused(BimScope.Reconstruct, BimReason.Capability, string.Join(':', new object?[] { "cloud-extent", cloud.PointCount.ToString(CultureInfo.InvariantCulture) })))
            : Try.lift(() => Levels(cloud, policy, (int)cloud.PointCount)).Run().Bind(static inner => inner);

    readonly record struct CloudStage(float[] Positions, ReadOnlyMemory<float> Colors, float Weight, int Count);

    static Seq<CloudLevel> Levels(LasCloud cloud, InterchangePolicy policy, int count) {
        var positions = new float[count * 3];
        var source = cloud.Positions.Span;
        for (int p = 0; p < count; p++) {
            Vector<double> xyz = source[p];
            (positions[p * 3], positions[(p * 3) + 1], positions[(p * 3) + 2]) = ((float)xyz[0], (float)xyz[1], (float)xyz[2]);
        }
        float weight = cloud.Colors.IsEmpty
            ? 0f
            : policy.AttributeWeights.Find(static row => row.Channel == ColorChannel).Map(static row => row.Weight).IfNone(0f);
        var staged = new CloudStage(positions, weight > 0f ? cloud.Colors : ReadOnlyMemory<float>.Empty, weight, count);
        return policy.LodRatios.Map((ratio, level) => Decimate(staged, ratio, level, cloud, policy));
    }

    static CloudLevel Decimate(CloudStage staged, double ratio, int level, LasCloud cloud, InterchangePolicy policy) {
        nuint stride = (nuint)(3 * sizeof(float));
        var destination = new uint[staged.Count];
        nuint retained = Meshopt.SimplifyPoints(
            destination, staged.Positions, stride, staged.Colors.Span, staged.Colors.IsEmpty ? 0u : stride,
            staged.Weight, (nuint)(long)(staged.Count * ratio));
        var indices = destination.AsSpan(0, (int)retained).ToArray();
        var drawn = new float[indices.Length * 3];
        for (int i = 0; i < indices.Length; i++) { staged.Positions.AsSpan((int)indices[i] * 3, 3).CopyTo(drawn.AsSpan(i * 3, 3)); }
        return new CloudLevel(level, ratio, indices.Length, indices, Meshopt.ComputeSphereBounds(drawn, stride),
            ContentAddress.Of((level, cloud, ratio, indices), policy.Distance.Value, static (s, writer) => writer
                .String($"cloud-lod{s.level}").U128(s.cloud.Lineage.Value).Double(s.ratio)
                .Raw(MemoryMarshal.AsBytes(s.indices.AsSpan()))).Value);
    }

    static void Check(laszip codec, int status) {
        if (status != 0) { throw new IOException(codec.get_error()); }
    }

    static Fin<LasCloud> ReadLas(ReadOnlyMemory<byte> bytes, Option<CloudWindow> window, Instant at) {
        string path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.las");
        using (FileStream sink = File.Create(path)) { sink.Write(bytes.Span); }
        try {
            using LasReader reader = new(path);
            Vector<double>[] positions = new Vector<double>[reader.PointCount];
            byte[] classes = new byte[reader.PointCount];
            byte classMask = reader.Header.PointDataFormat < 6 ? (byte)0x1F : (byte)0xFF;
            bool colored = Colored(reader.Header.PointDataFormat);
            float[] colors = colored ? new float[reader.PointCount * 3] : [];
            LasPoint point = new();
            ulong scanned = 0, read = 0;
            for (; !reader.EOF && scanned < reader.PointCount; scanned++) {
                reader.GetNextPoint(ref point);
                if (window.Exists(w => !w.Holds(point.Position[0], point.Position[1]))) { continue; }
                positions[read] = point.Position.Clone();
                classes[read] = (byte)(point.Classification & classMask);
                if (colored) {
                    (colors[read * 3], colors[(read * 3) + 1], colors[(read * 3) + 2]) =
                        (point.R / 65535f, point.G / 65535f, point.B / 65535f);
                }
                read++;
            }
            Option<string> crs = reader.VLRs.AsIterable()
                .Filter(static vlr => vlr.RecordID == LasVariableLengthRecord.ProjectionRecordID).Head
                .Map(static vlr => Encoding.UTF8.GetString(vlr.Data).TrimEnd('\0'));
            ILasHeader h = reader.Header;
            return scanned < reader.PointCount
                ? Fin.Fail<LasCloud>(new BimFault.Refused(BimScope.Reconstruct, BimReason.Rejected, string.Join(':', new object?[] { "cloud-truncated", scanned.ToString(CultureInfo.InvariantCulture), reader.PointCount.ToString(CultureInfo.InvariantCulture) })))
                : Fin.Succ(Assemble(bytes, positions, classes, colors, crs, read, h.NumPointRecordsByReturn,
                    new Vector3(h.MinX, h.MinY, h.MinZ), new Vector3(h.MaxX, h.MaxY, h.MaxZ),
                    new Vector3(h.ScaleX, h.ScaleY, h.ScaleZ), new Vector3(h.OriginX, h.OriginY, h.OriginZ),
                    h.PointDataFormat, at));
        } finally { File.Delete(path); }
    }

    static LasCloud ReadLaz(ReadOnlyMemory<byte> bytes, Option<CloudWindow> window, Instant at) {
        using MemoryStream stream = MemoryMarshal.TryGetArray(bytes, out ArraySegment<byte> segment)
            ? new(segment.Array!, segment.Offset, segment.Count, writable: false)
            : new(bytes.ToArray(), writable: false);
        laszip codec = laszip.create();
        Check(codec, codec.decompress_selective(LASZIP_DECOMPRESS_SELECTIVE.CHANNEL_RETURNS_XY | LASZIP_DECOMPRESS_SELECTIVE.Z | LASZIP_DECOMPRESS_SELECTIVE.CLASSIFICATION | LASZIP_DECOMPRESS_SELECTIVE.RGB));
        Check(codec, codec.open_reader_stream(stream, out _, leaveOpen: true));
        try {
            Check(codec, codec.get_number_of_point(out long count));
            bool clipped = window.Map(w => Windowed(codec, w)).IfNone(false);
            bool colored = Colored((byte)(codec.header.point_data_format & 0x7F));
            Vector<double>[] positions = new Vector<double>[count];
            byte[] classes = new byte[count];
            float[] colors = colored ? new float[count * 3] : [];
            double[] xyz = new double[3];
            long read = 0;
            for (long i = 0; i < count; i++) {
                if (clipped) {
                    Check(codec, codec.read_inside_point(out bool done));
                    if (done) { break; }
                } else {
                    Check(codec, codec.read_point());
                }
                Check(codec, codec.get_coordinates(xyz));
                if (!clipped && window.Exists(w => !w.Holds(xyz[0], xyz[1]))) { continue; }
                positions[read] = Vector<double>.Build.DenseOfArray(xyz);
                classes[read] = codec.point.extended_point_type != 0 ? codec.point.extended_classification : codec.point.classification;
                if (colored) {
                    ushort[] rgb = codec.point.rgb;
                    (colors[read * 3], colors[(read * 3) + 1], colors[(read * 3) + 2]) = (rgb[0] / 65535f, rgb[1] / 65535f, rgb[2] / 65535f);
                }
                read++;
            }
            Option<string> crs = codec.header.vlrs.AsIterable()
                .Filter(static vlr => vlr.record_id == LasVariableLengthRecord.ProjectionRecordID).Head
                .Map(static vlr => Encoding.UTF8.GetString(vlr.data).TrimEnd('\0'));
            var h = codec.header;
            ulong[] byReturn = h.extended_number_of_point_records > 0
                ? h.extended_number_of_points_by_return
                : System.Array.ConvertAll(h.number_of_points_by_return, static c => (ulong)c);
            return Assemble(bytes, positions, classes, colors, crs, (ulong)read, byReturn,
                new Vector3(h.min_x, h.min_y, h.min_z), new Vector3(h.max_x, h.max_y, h.max_z),
                new Vector3(h.x_scale_factor, h.y_scale_factor, h.z_scale_factor),
                new Vector3(h.x_offset, h.y_offset, h.z_offset),
                (byte)(h.point_data_format & 0x7F), at);
        } finally { codec.close_reader(); }
    }

    static bool Windowed(laszip codec, CloudWindow window) {
        Check(codec, codec.has_spatial_index(out bool indexed, out _));
        if (!indexed) { return false; }
        Check(codec, codec.inside_rectangle(window.MinX, window.MinY, window.MaxX, window.MaxY, out bool empty));
        if (empty) { return false; }
        Check(codec, codec.exploit_spatial_index(true));
        return true;
    }

    static LasCloud Assemble(
        ReadOnlyMemory<byte> bytes, Vector<double>[] positions, byte[] classes, float[] colors, Option<string> crs, ulong count,
        ulong[] byReturn, Vector3 min, Vector3 max, Vector3 scale, Vector3 offset, byte pointFormat, Instant at) {
        var counts = new ulong[256];
        foreach (byte cls in classes) { counts[cls]++; }
        Map<byte, ulong> histogram = toMap(Enumerable.Range(0, 256).Where(c => counts[c] > 0).Select(c => ((byte)c, counts[c])));
        return new(positions, classes, colors, histogram, byReturn, min, max, scale, offset, crs,
            CaptureLineage.Of(bytes), pointFormat, count, at);
    }
}
```

## [04]-[RESEARCH]

(none)
