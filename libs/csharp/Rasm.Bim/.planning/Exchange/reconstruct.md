# [BIM_RECONSTRUCTION]

The scan-to-BIM PROJECTOR: one `ReconstructionProjector : IElementProjection` lowering a kernel-segmented point cloud into a `Rasm.Element/Graph/delta#GRAPH_DELTA` `GraphDelta` of seam `Rasm.Element/Graph/element#ELEMENT_GRAPH` `Node.Object` occurrence nodes, each carrying a typed `Pset_Reconstruction` `Node.PropertySet` bag bound by a neutral `Rasm.Element/Relations/relation#EDGE_ALGEBRA` `Relationship.Assign` edge, plus the `LasIngest` LAS/LAZ decode front. Reconstruction is a PRIMARY projector — the scan-source twin of the `Projection/semantic#SEMANTIC_PROJECTOR` IFC projector: it MINTS rooted element identity from scratch through the kernel static `NodeId.Rooted()` (the NEUTRAL kernel id, never an IFC GlobalId — §4-RT H6) and records a deterministic IFC `GlobalId` as the node's 1:1 `ExternalId` (hashed from the source-cloud `ReconstructionLineage`) so a re-run of the same capture dedups against its prior pass through the `Review/diff#MODEL_DIFF` federation diff, never a parallel `ScannedElement` record beside the seam graph. "Has it all" is the `Bake` fold over that graph — the retired `BimElement`/`BimModel` is GONE.

Reconstruction is BIM-semantics-only and CONSUME-BY-REFERENCE: the LAS/LAZ DECODE is `Themis.Las`/`Unofficial.laszip.netstandard`'s, the registration and fit are the kernel's (`csharp:Rasm/Processing/register#ALIGN` cloud-ICP places the capture in the canonical kernel frame, `csharp:Rasm/Spatial/cloud#SEGMENTATION` partitions it into shape-labeled `SegmentedCloud` rows whose planar/cylindrical boundaries read the `csharp:ROBUST_ARRANGEMENT_SUBSTRATE` exact-arithmetic arrangement), and the geometry CONTENT KEY is the kernel `Rasm.Domain.ContentHash` seed-zero `XxHash128` — the ONE hasher the seam `Rasm.Element/Projection/address#CONTENT_ADDRESS` `ContentAddress` wraps over the `Rasm.Element/Projection/address#CANONICAL_WRITER` `CanonicalWriter` projection, never the upper-stratum `Rasm.Compute` interchange owner a `Rasm.Bim`→`Rasm.Compute` reference would invert. The fitted primitive is HOST-NEUTRAL: the `Node.Object` references ALL its geometry by `RepresentationContentHash` content key only — the display solid under `Body`, the analytical surface (a planar wall/slab `BoundaryPolygon`) under `FootPrint`, and the analytical line (a swept column/beam `AxisCurve`) under `Axis`, EACH a kernel seed-zero `XxHash128` over the `CanonicalWriter` projection of its `Vector3` coordinates [M2] — so a `Rasm.Compute` structural/energy runner RESOLVES the analytical axis/footprint one-hop by content key from the blob store, NEVER an inline coordinate field on the seam node (no `Node.Object.BoundaryPolygon`/`Axis` member exists — the deleted §4-RT-M2 violation) and never a RhinoCommon `Brep`/`Mesh`. The page is HOST-NEUTRAL.

## [01]-[INDEX]

- [01]-[RECONSTRUCTION]: `ReconstructionProjector` the `IElementProjection` scan-source projector folding `Seq<SegmentedCloud>` into a `GraphDelta`; `ReconstructionPrimitive` the closed `[Union]` over the complete analytic-primitive set (`Plane`/`Sphere`/`Cylinder`/`Cone`/`Torus`/`Freeform`); `PrimitiveShape` the `[SmartEnum<string>]` shape discriminant; `ElementClassifier` the frozen `(PrimitiveShape, IfcDomain, FitOrientation)`→`IfcClass` table; `ReconstructionLineage` the `[ValueObject<UInt128>]` source-cloud content key; `FitConfidence` the `[ValueObject<double>]` inlier band; `SegmentedCloud` the kernel-registered carrier; `ReconstructionContext` the fit/discipline policy; `AsprsBias` the total ASPRS dominant-class policy row (constructibility, domain bias, class pin).
- [02]-[LAS_INGEST]: `LasCloud` the decoded point carrier plus its header/histogram receipt facts; `LasCompression` the `[SmartEnum<string>]` compression discriminant; `LasIngest.Decode` the dual-engine fold dispatching the uncompressed leg onto the `Themis.Las` `LasReader` (MathNet-native, the `GetNextPoint(ref LasPoint)` no-alloc loop) and the compressed leg onto the `Unofficial.laszip.netstandard` `laszip` arithmetic decoder (selective-channel), both producing the `MathNet.Numerics` point set the kernel registration/segmentation consume to yield the geometric segmentation the `[02]-[RECONSTRUCTION]` `SegmentedCloud` carrier rows are assembled from at the Bim boundary.

## [02]-[RECONSTRUCTION]

- Owner: `ReconstructionProjector` the `IElementProjection` capturing the kernel-produced `Seq<SegmentedCloud>` and the `ReconstructionContext` INTERNALLY and lowering them to a seam `GraphDelta` in `Project`; `ReconstructionPrimitive` the closed `[Union]` of fitted scan primitives — `Plane` (a bounded planar patch carrying its frame normal and its arrangement-bounded `BoundaryPolygon`), `Sphere` (a center/radius solid), `Cylinder` (an axis/radius swept solid carrying its fitted axis extent), `Cone` (an apex/axis/half-angle swept solid), `Torus` (a major/minor-radius revolved solid), and `Freeform` (a residual cloud no analytic primitive fit) — each arm carrying the kernel-computed `GeometryHash` content key, its inlier `FitConfidence`, and the `ReconstructionLineage` of the segment it was fitted to; `PrimitiveShape` the `[SmartEnum<string>]` shape discriminant the `ElementClassifier` table keys on; `ReconstructionLineage` the `[ValueObject<UInt128>]` source-cloud content key joining the reconstructed element to its capture; `FitConfidence` the `[ValueObject<double>]` normalized inlier-ratio band in `[0,1]`; `SegmentedCloud` the kernel-registered segment carrier; `ElementClassifier` the frozen shape-to-`IfcClass` projection.
- Cases: `ReconstructionPrimitive` arms `Plane`/`Sphere`/`Cylinder`/`Cone`/`Torus`/`Freeform` (6) ARE the complete efficient-RANSAC point-cloud shape-detection family (plane, sphere, cylinder, cone, torus) plus the residual freeform — a primitive family is one arm plus one `PrimitiveShape` row plus one `ElementClassifier` table entry, never a per-shape fold and never a `FitPlane`/`FitCylinder` operation family; `ElementClassifier` rows are the `(shape, IfcDomain, orientation)`→`(IfcClass, predefined)` table, a wall-vs-slab disambiguation one row refined by orientation, never an enumerated `switch` arm.
- Entry: `ReconstructionProjector.Project(ProjectionContext ctx)` folds the captured segments into one `GraphDelta` — it seeds `GraphDelta.Empty.Reheader(ctx.Header)` (the model-creating event establishes the release/view/georeference/tolerance Header the app supplies, the scan CRS WKT having flowed `LasCloud.CrsWkt`→app→`ctx.Header.Reference` per the wiring-is-app-owned law) and merges each segment's sub-delta; reconstruction is a PRIMARY projector so it IGNORES `ctx.ElementIds` and PUBLISHES the rooted ids it mints for an aspect projector (`Rasm.Materials/Projection/component`) to attach `Associate` edges against; `Fin<T>` aborts on an unregistered segment (`Model/faults#FAULT_BAND` `BimFault.CapabilityMiss`) or a shape the `ElementClassifier` table does not carry (`BimFault.UnmappedClass`), each `Op`-keyed case lifted BARE onto the `Fin<T>` rail (the `Expected`-derived case IS the `Error` — no `.ToError()` hop), the seam `Rasm.Element/Projection/projection#PROJECTION_CONTRACT` `Assemble` funnel capturing any thrown fault into `ElementFault.ProjectionFailed`.
- Auto: `Project` reads each `SegmentedCloud` already fitted and registered by the kernel — the cloud-ICP alignment placed the capture in the canonical kernel frame and the segmentation partitioned it into shape-labeled segments whose planar boundaries read the exact-arithmetic arrangement — so the fold NEVER re-fits geometry in-process; a `segment.Geometry.IsPending` handle is an unregistered capture faulted `BimFault.CapabilityMiss` rather than fitted here. `ReconstructionPrimitive.Of(segment, context)` lifts the segment to its arm by `segment.Shape`, binding the kernel `GeometryHash` content key the segmentation produced. The `ReconstructionContext.BiasOf` TOTAL ASPRS policy governs first: a non-`Constructible` class (noise 7/18, overlap 12, water 9) is EXCLUDED by the explicit `Project` filter before authoring; a `Pin` class (vegetation 3/4/5 → `GeographicElement`/`VEGETATION`, rail 10 → `Rail`/`RAIL`, transmission tower 15 → `ElementAssembly`/`MAST`, wire-structure connector 16 → `CableFitting`/`CONNECTOR`) short-circuits the table because its IFC landing is shape-independent. `ElementClassifier.Classify` otherwise resolves the `IfcClass` from the frozen table keyed on the EFFECTIVE `IfcDomain` (the bias domain when present, else the context discipline) and the `FitOrientation` — a planar patch's surface orientation reads `OrientationOfNormal` (a vertical normal is a horizontal slab, a horizontal normal a vertical wall) while a swept solid's reads `OrientationOfAxis` (a vertical axis is a vertical column, a horizontal axis a horizontal beam) — then EVERY landing (pin or table) admits through the one frozen 4-arg `Model/elements#IFC_CLASS` `IfcClass.AdmitPredefined(token, objectType, schema, key)` per-token egress gate returning `Fin<string>` against `ctx.Header.Schema` (§4-RT C6); the `Node.Object` mints a NEUTRAL rooted `NodeId` through the kernel static `NodeId.Rooted()` (§4-RT H6) and records the deterministic IFC `GlobalId` `ParserIfc.HashGlobalID("recon:{lineage}")` as its 1:1 `ExternalId`; ALL geometry rides the `RepresentationContentHash` keyed map through `ReconstructionPrimitive.Keys` — the display solid under `Body`, the analytical surface under `FootPrint`, the analytical line under `Axis`, each a kernel `ContentHash` over the `CanonicalWriter` projection of its coordinates [M2] — so `Rasm.Compute` resolves the analytical axis/footprint one-hop by content key, never a node coordinate field; the typed `Pset_Reconstruction` `Node.PropertySet` carries the fit evidence as `PropertyValue.Measure`/`Boolean`/`Enumerated`/`Text` (NOT a stringly binding) and binds to the occurrence through a `Relationship.Assign(objectId, bagId, AssignKind.PropertyDefinition)` edge the seam `Bake` folds.
- Receipt: the `GraphDelta` is the projector's whole contribution — the merge the seam `Assemble` folds with the other projectors' deltas onto a `Genesis` seed; the `ReconstructionPrimitive` arm is the typed fit evidence (analytic parameters + inlier `FitConfidence`), the `Pset_Reconstruction` bag the per-element review record (`FitConfidence`/`Residual`/`Inliers`/`Total` as dimensionless `MeasureValue`, `NeedsReview` a `Boolean` over `ConfidenceFloor`, `PrimitiveShape` an `Enumerated`, `SourceCloud` the lineage hex) a `Persistence`/`Compute` `ByProperty` read selects below-floor elements on, and the deterministic `ExternalId` the federation diff joins a re-reconstructed element to its prior pass and its as-designed counterpart on — no generic `IFitResult` abstraction, the union arms stay typed per primitive family.
- Packages: `Rasm.Element` (the seam `Node`/`NodeId`/`GraphDelta`/`Relationship`/`Classification`/`PredefinedType`/`PropertyBag`/`PropertyValue`/`MeasureValue`/`Dimension`/`RepresentationContentHash`/`AxisCurve`/`SchemaSpan`/the `Projection/address#CANONICAL_WRITER` `CanonicalWriter` + the `IElementProjection`/`ProjectionContext` contract AND the seam-owned host-neutral `Graph/element#NODE_MODEL` `Vector3` coordinate + its `Dot`/`Unit`/`UnitX`/`UnitZ` algebra the orientation classifier folds — the seam owns the analytical coordinate the way it owns `Dimension`, so `using Rasm.Element` provides it and no kernel `Vector3` exists), `Rasm` (the `GeometryHandle` registration handle, the `Domain.ContentHash` seed-zero `XxHash128` — consumed by reference; the kernel `Rasm.Numerics` coordinate is the RhinoCommon `Vector3d` this host-neutral projection never touches), GeometryGymIFC_Core (`ParserIfc.HashGlobalID` the deterministic GlobalId codec), Thinktecture.Runtime.Extensions (`[Union]`/`[SmartEnum]`/`[ValueObject]`), LanguageExt.Core (`Fin`/`Seq`/`Map`/`Option`).
- Growth: a new fitted primitive is one `ReconstructionPrimitive` arm carrying its analytic parameters plus one `PrimitiveShape` row plus one `ElementClassifier` table entry — the fold and the classifier resolve it with no new operation; a new classification rule is one `ElementClassifier` row keyed on `(PrimitiveShape, IfcDomain, FitOrientation)`; a repeated identical fit (a column array, a window grid) shares ONE `GeometryHash` so the content-keyed blob store dedups the geometry with no parallel type-instance; a new confidence dimension is one `Pset_Reconstruction` row; a new discipline bias is one `BiasOf` arm PLUS the `ElementClassifier` rows that bias resolves to (a bias arm with no matching rows steers a segment into an empty domain and faults `recon-shape-miss`), a shape-independent site class one `Pin` row, and a non-constructible class one `Excluded` row — the `AsprsBias` policy is the one growth surface for all three; never a per-shape `Node.Object` subtype and never a second receipt model.
- Boundary: reconstruction is the LAST fold to a seam `Node.Object`, never a geometry kernel — the cloud-ICP registration, the plane/cylinder segmentation, and the exact-arithmetic arrangement are the kernel's consumed by reference, and a re-minted RANSAC/region-grow fitter or a second arrangement here is the deleted form; the source-cloud content key composes the kernel `Rasm.Domain.ContentHash` seed-zero `XxHash128` through the seam `CanonicalWriter`, and the `Rasm.Compute` interchange owner (an upper-stratum `Rasm.Bim`→`Rasm.Compute` reference) or a second hashing scheme is the named seam violation; ALL fitted geometry rides the `RepresentationContentHash` keyed map (`Body`/`FootPrint`/`Axis`, each a kernel content key over the `CanonicalWriter` projection of its `Vector3` coordinates [M2]), an inline `Vector3`/`BoundaryPolygon`/`Axis` coordinate field on the seam `Node.Object` (the deleted §4-RT-M2 violation), a RhinoCommon `Brep`/`Mesh` field on `ReconstructionPrimitive`, or a stored `GeometryHandle` on the seam node the named host-bound defect; the rooted `NodeId` is the NEUTRAL kernel-minted id and the IFC GlobalId is the node's `ExternalId` projection (§4-RT H6), the deterministic mint from the lineage giving re-run dedup without making the GUID the node identity; the reconstructed element is a `Node.Object` discriminated by the same generic `Classification`/`PredefinedType` axes an IFC-ingested element carries, so the `Model/query` query algebra and the `Review/validation` audit read a reconstructed model with no second selection surface, and a parallel `ScannedElement`/`ReconstructedElement` record is the deleted form; the fit evidence rides the typed `Pset_Reconstruction` `PropertyValue`/`MeasureValue` bag the seam property store owns, the retired stringly `PropertyBinding(string,string,string)` triple the deleted form; a segment whose shape the table does not carry faults `BimFault.UnmappedClass` and an unregistered segment faults `BimFault.CapabilityMiss`, so an unclassifiable scan never silently produces a half-built model — distinct from the KNOWN-non-constructible ASPRS classes (noise/overlap/water) the `BiasOf` policy excludes by explicit filter before authoring, a deliberate policy row, never a dropped fault.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Globalization;
using GeometryGym.Ifc;
using LanguageExt;
using Rasm;
using Rasm.Domain;
using Rasm.Element.Classification;
using Rasm.Element.Graph;
using Rasm.Element.Projection;
using Rasm.Element.Properties;
using Rasm.Element.Relations;
using Thinktecture;
using static LanguageExt.Prelude;
using ReleaseVersion = Rasm.Element.Graph.ReleaseVersion;   // the seam schema currency the Header carries — disambiguated
                                                      // from GeometryGym.Ifc.ReleaseVersion (the IFC-text codec leg),
                                                      // which this projection never touches.

using Rasm.Spatial;

namespace Rasm.Bim;

// --- [TYPES] ------------------------------------------------------------------------------
// The complete efficient-RANSAC analytic-primitive set (plane/sphere/cylinder/cone/torus) plus the residual freeform.
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

// The ASPRS policy row: Constructible=false EXCLUDES the class from element minting (noise/overlap/water — an explicit
// policy exclusion the Project fold filters BEFORE construction, never a silent drop of an UNMAPPED class, which
// still faults recon-shape-miss); Domain biases the classifier lane; Pin short-circuits the (shape, domain,
// orientation) table for classes whose IFC landing is shape-independent (a vegetation trunk/canopy/hedge is
// IfcGeographicElement/VEGETATION whether it fitted a cylinder, a freeform, or a plane).
public readonly record struct AsprsBias(bool Constructible, Option<IfcDomain> Domain, Option<(IfcClass Class, string Predefined)> Pin);

[ValueObject<double>]
public sealed partial class FitConfidence {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) =>
        value = double.IsFinite(value) ? Math.Clamp(value, 0.0, 1.0) : 0.0;

    public bool IsBelow(double threshold) => Value < threshold;
}

// The source-cloud content key — the kernel seed-zero XxHash128 over the segment bytes + fit params through the seam
// CanonicalWriter (the ONE hasher), NOT the upper-stratum Rasm.Compute interchange owner. UInt128 keys on its own value.
[ValueObject<UInt128>]
public sealed partial class ReconstructionLineage {
    public static ReconstructionLineage Of(SegmentedCloud segment, ReconstructionContext context) =>
        Create(ContentHash.Of(new CanonicalWriter(context.Tolerance)
            .String("recon-cloud").Raw(segment.CloudBytes.Span)
            .Double(context.Deflection).Double(context.AngleTolerance).ToBytes().Span));
}

// --- [MODELS] -----------------------------------------------------------------------------
public readonly record struct ReconstructionContext(
    IfcDomain Discipline, double Deflection, double Tolerance, double AngleTolerance,
    double ConfidenceFloor, double VerticalCosineLimit) {
    public static readonly ReconstructionContext Building =
        new(IfcDomain.Architecture, 1e-3, 1e-6, 1e-4, 0.6, 0.342);

    // VerticalCosineLimit is sin(band) — 0.342 pins a 20-degree tilt band; the parallel-side bound is the TRIG
    // complement cos(band) = sqrt(1 - limit^2) (0.940), never the arithmetic 1-limit (0.658), which slabs a
    // 45-degree-pitched roof.
    double UprightCosine => Math.Sqrt(1.0 - (VerticalCosineLimit * VerticalCosineLimit));

    // A planar patch's surface orientation: a vertical NORMAL is a horizontal surface (slab), a horizontal normal a
    // vertical surface (wall). Distinct from a swept solid's axis orientation, whose vertical/horizontal mapping inverts.
    public FitOrientation OrientationOfNormal(Vector3 normal) {
        double vertical = Math.Abs(Vector3.Dot(normal.Unit, Vector3.UnitZ));
        return vertical >= UprightCosine      ? FitOrientation.Horizontal
            : vertical <= VerticalCosineLimit ? FitOrientation.Vertical
            : FitOrientation.Inclined;
    }

    // A swept solid's axis orientation: a vertical AXIS is a vertical member (column), a horizontal axis a horizontal
    // member (beam) — the inverse of the surface-normal mapping, so a vertical-axis cylinder classifies Column, not Beam.
    public FitOrientation OrientationOfAxis(Vector3 axis) {
        double vertical = Math.Abs(Vector3.Dot(axis.Unit, Vector3.UnitZ));
        return vertical >= UprightCosine      ? FitOrientation.Vertical
            : vertical <= VerticalCosineLimit ? FitOrientation.Horizontal
            : FitOrientation.Inclined;
    }

    static readonly AsprsBias Vegetation    = new(true, Option<IfcDomain>.None, Some((IfcClass.GeographicElement, "VEGETATION")));
    static readonly AsprsBias Tower         = new(true, Option<IfcDomain>.None, Some((IfcClass.ElementAssembly, "MAST")));
    static readonly AsprsBias WireConnector = new(true, Option<IfcDomain>.None, Some((IfcClass.CableFitting, "CONNECTOR")));
    static readonly AsprsBias Excluded      = new(false, Option<IfcDomain>.None, None);
    static readonly AsprsBias Unbiased      = new(true, Option<IfcDomain>.None, None);

    // The TOTAL ASPRS dominant-class policy over the LAS 1.4 standard classes: ground 2 -> Geotechnical, low/medium/
    // high vegetation 3/4/5 -> the GeographicElement/VEGETATION pin, building 6 -> Architecture, low noise 7 and high
    // noise 18 and overlap 12 and water 9 -> NON-CONSTRUCTIBLE (a noise segment never mints a phantom element, a flat
    // water return never mints a pavement), road 11 / bridge deck 17 -> Infrastructure, rail 10 -> the Rail/"RAIL" pin
    // under Infrastructure, wire-guard 13 and wire-conductor 14 -> Electrical, transmission tower 15 -> the
    // ElementAssembly/MAST pin and wire-structure connector 16 -> the CableFitting/CONNECTOR pin (shape-independent —
    // a lattice tower fits freeform, an insulator a cylinder or freeform); 0/1 unassigned and every remaining reserved
    // class fall back to the context Discipline. Every biased domain resolves plane AND freeform lanes (the shapes an
    // outdoor segment fits), so no bias arm steers a segment into an empty domain; the cylinder lanes ride the
    // domains owning a swept member.
    public static AsprsBias BiasOf(byte asprsClass) => asprsClass switch {
        2                  => new AsprsBias(true, Some(IfcDomain.Geotechnical), None),
        3 or 4 or 5        => Vegetation,
        6                  => new AsprsBias(true, Some(IfcDomain.Architecture), None),
        7 or 9 or 12 or 18 => Excluded,
        11 or 17           => new AsprsBias(true, Some(IfcDomain.Infrastructure), None),
        10                 => new AsprsBias(true, Some(IfcDomain.Infrastructure), Some((IfcClass.Rail, "RAIL"))),
        13 or 14           => new AsprsBias(true, Some(IfcDomain.Electrical), None),
        15                 => Tower,
        16                 => WireConnector,
        _                  => Unbiased,
    };
}

// The Bim ingress carrier Reconstruct reads, assembled at the boundary from the kernel's geometric segmentation output —
// the kernel fits the primitive and detects its shape; this carrier and its PrimitiveShape discriminant are Bim-side, so
// the kernel never references a Bim type (no downward dep). A flat SoA whose fields the ReconstructionPrimitive arms
// project into typed shape evidence; the GeometryHash is the kernel-computed content key of the fitted solid (consumed by
// reference, never re-fit), the BoundaryPolygon the arrangement-bounded planar patch, AxisStart/AxisEnd the swept axis
// extent, and DominantClass the segment's modal ASPRS class.
public readonly record struct SegmentedCloud(
    int SegmentId, PrimitiveShape Shape, GeometryHandle Geometry, UInt128 GeometryHash,
    Vector3 Normal, Vector3 Center, Vector3 Axis, Vector3 AxisStart, Vector3 AxisEnd,
    double Radius, double MinorRadius, double HalfAngle, Seq<Vector3> BoundaryPolygon,
    byte DominantClass, int Inliers, int Total, ReadOnlyMemory<byte> CloudBytes) {
    public FitConfidence Confidence => FitConfidence.Create(Total > 0 ? (double)Inliers / Total : 0.0);
    public double Residual => Total > 0 ? 1.0 - (double)Inliers / Total : 1.0;
}

[Union]
public abstract partial record ReconstructionPrimitive {
    private ReconstructionPrimitive() { }

    public sealed record Plane(int SegmentId, UInt128 GeometryHash, Vector3 Normal, Seq<Vector3> Boundary, FitConfidence Confidence, ReconstructionLineage Lineage) : ReconstructionPrimitive;
    public sealed record Sphere(int SegmentId, UInt128 GeometryHash, Vector3 Center, double Radius, FitConfidence Confidence, ReconstructionLineage Lineage) : ReconstructionPrimitive;
    public sealed record Cylinder(int SegmentId, UInt128 GeometryHash, Vector3 AxisStart, Vector3 AxisEnd, Vector3 Direction, double Radius, FitConfidence Confidence, ReconstructionLineage Lineage) : ReconstructionPrimitive;
    public sealed record Cone(int SegmentId, UInt128 GeometryHash, Vector3 AxisStart, Vector3 AxisEnd, Vector3 Direction, double Radius, double HalfAngle, FitConfidence Confidence, ReconstructionLineage Lineage) : ReconstructionPrimitive;
    public sealed record Torus(int SegmentId, UInt128 GeometryHash, Vector3 AxisStart, Vector3 AxisEnd, Vector3 Direction, double Radius, double MinorRadius, FitConfidence Confidence, ReconstructionLineage Lineage) : ReconstructionPrimitive;
    public sealed record Freeform(int SegmentId, UInt128 GeometryHash, FitConfidence Confidence, ReconstructionLineage Lineage) : ReconstructionPrimitive;

    public static ReconstructionPrimitive Of(SegmentedCloud s, ReconstructionContext context) {
        ReconstructionLineage lineage = ReconstructionLineage.Of(s, context);
        return s.Shape.Switch<ReconstructionPrimitive>(
            plane:    () => new Plane(s.SegmentId, s.GeometryHash, s.Normal, s.BoundaryPolygon, s.Confidence, lineage),
            sphere:   () => new Sphere(s.SegmentId, s.GeometryHash, s.Center, s.Radius, s.Confidence, lineage),
            cylinder: () => new Cylinder(s.SegmentId, s.GeometryHash, s.AxisStart, s.AxisEnd, s.Axis, s.Radius, s.Confidence, lineage),
            cone:     () => new Cone(s.SegmentId, s.GeometryHash, s.AxisStart, s.AxisEnd, s.Axis, s.Radius, s.HalfAngle, s.Confidence, lineage),
            torus:    () => new Torus(s.SegmentId, s.GeometryHash, s.AxisStart, s.AxisEnd, s.Axis, s.Radius, s.MinorRadius, s.Confidence, lineage),
            freeform: () => new Freeform(s.SegmentId, s.GeometryHash, s.Confidence, lineage));
    }

    public PrimitiveShape Shape => Switch(
        plane:    static _ => PrimitiveShape.Plane,  sphere:   static _ => PrimitiveShape.Sphere,
        cylinder: static _ => PrimitiveShape.Cylinder, cone:   static _ => PrimitiveShape.Cone,
        torus:    static _ => PrimitiveShape.Torus,  freeform: static _ => PrimitiveShape.Freeform);

    public UInt128 GeometryHash => Switch(
        plane:    static p => p.GeometryHash, sphere:   static s => s.GeometryHash,
        cylinder: static c => c.GeometryHash, cone:     static c => c.GeometryHash,
        torus:    static t => t.GeometryHash, freeform: static f => f.GeometryHash);

    public FitConfidence Confidence => Switch(
        plane:    static p => p.Confidence, sphere:   static s => s.Confidence,
        cylinder: static c => c.Confidence, cone:     static c => c.Confidence,
        torus:    static t => t.Confidence, freeform: static f => f.Confidence);

    public ReconstructionLineage Lineage => Switch(
        plane:    static p => p.Lineage, sphere:   static s => s.Lineage,
        cylinder: static c => c.Lineage, cone:     static c => c.Lineage,
        torus:    static t => t.Lineage, freeform: static f => f.Lineage);

    // The analytical surface polygon of a planar wall/slab; the non-planar arms carry none. It is CONTENT-KEYED into the
    // Node.Object Representations map under the "FootPrint" key (Keys, below), NEVER inlined as a coordinate field on the
    // seam node (the deleted §4-RT-M2 violation) — a Rasm.Compute energy runner resolves it one-hop from the blob store.
    public Seq<Vector3> BoundaryPolygon => Switch(
        plane:    static p => p.Boundary, sphere:   static _ => Seq<Vector3>(),
        cylinder: static _ => Seq<Vector3>(), cone:  static _ => Seq<Vector3>(),
        torus:    static _ => Seq<Vector3>(), freeform: static _ => Seq<Vector3>());

    // The idealized structural line of a swept column/beam; the non-swept arms carry none. It is CONTENT-KEYED into the
    // Node.Object Representations map under the "Axis" key (Keys, below), NEVER inlined on the seam node — a Rasm.Compute
    // structural runner resolves it one-hop from the blob store. The Curve fold derives a non-degenerate local up once.
    public Option<AxisCurve> Axis => Switch(
        plane:    static _ => Option<AxisCurve>.None, sphere: static _ => Option<AxisCurve>.None,
        cylinder: static c => Some(Curve(c.AxisStart, c.AxisEnd, c.Direction)),
        cone:     static c => Some(Curve(c.AxisStart, c.AxisEnd, c.Direction)),
        torus:    static t => Some(Curve(t.AxisStart, t.AxisEnd, t.Direction)),
        freeform: static _ => Option<AxisCurve>.None);

    static AxisCurve Curve(Vector3 start, Vector3 end, Vector3 axis) =>
        new(start, end, Math.Abs(Vector3.Dot(axis.Unit, Vector3.UnitZ)) > 0.9 ? Vector3.UnitX : Vector3.UnitZ);

    // The keyed geometry map [M2]: the fitted-solid display geometry rides "Body" (the kernel GeometryHash), the analytical
    // surface "FootPrint" and the analytical line "Axis" — EACH a kernel seed-zero ContentHash over the CanonicalWriter
    // projection of its Vector3 coordinates (the ONE hasher the seam CanonicalWriter composes), so a Rasm.Compute runner
    // resolves the analytical axis/footprint one-hop by content key from the blob store rather than reading a node field.
    // An empty BoundaryPolygon / a None Axis contributes no key, so a non-planar/non-swept arm carries only "Body".
    public RepresentationContentHash Keys(double tolerance) {
        RepresentationContentHash map = RepresentationContentHash.Empty.With("Body", GeometryHash);
        Seq<Vector3> footprint = BoundaryPolygon;
        if (footprint.Count > 0) {
            CanonicalWriter w = new(tolerance);
            w.String("recon-footprint").Ordinal(footprint.Count);
            footprint.Iter(p => w.Double(p.X).Double(p.Y).Double(p.Z));
            map = map.With("FootPrint", ContentHash.Of(w.ToBytes().Span));
        }
        return Axis.Match(
            Some: axis => {
                CanonicalWriter w = new(tolerance);
                w.String("recon-axis")
                 .Double(axis.Start.X).Double(axis.Start.Y).Double(axis.Start.Z)
                 .Double(axis.End.X).Double(axis.End.Y).Double(axis.End.Z)
                 .Double(axis.Up.X).Double(axis.Up.Y).Double(axis.Up.Z);
                return map.With("Axis", ContentHash.Of(w.ToBytes().Span));
            },
            None: () => map);
    }
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class ElementClassifier {
    // The frozen (shape, domain, orientation) -> (IfcClass, predefined) projection — a data table, not enumerated switch
    // arms. The predefined tokens are members of the Model/elements#IFC_CLASS valid sets, admitted at the egress gate; the
    // domain axis carries every IfcDomain the BiasOf ASPRS bias yields (Architecture/Structural/HvacFire AND the outdoor
    // Geotechnical/Infrastructure/Electrical rows — plane AND freeform lanes for the outdoor domains), so a ground/road/
    // conductor segment resolves a real class rather than an empty-domain miss; the shape-independent site classes ride
    // the BiasOf Pin tier, never table rows. A row whose Ifc4x3 infrastructure/geotechnical class the older target schema
    // cannot carry faults class-out-of-schema at AdmitPredefined (not recon-shape-miss), the egress gate's job, never a
    // silent half-model.
    static readonly Map<(PrimitiveShape Shape, IfcDomain Domain, FitOrientation Orientation), (IfcClass Class, string Predefined)> Table =
        Map(
            ((PrimitiveShape.Plane,    IfcDomain.Architecture,   FitOrientation.Vertical),   (IfcClass.Wall,                "STANDARD")),
            ((PrimitiveShape.Plane,    IfcDomain.Architecture,   FitOrientation.Horizontal), (IfcClass.Slab,                "FLOOR")),
            ((PrimitiveShape.Plane,    IfcDomain.Architecture,   FitOrientation.Inclined),   (IfcClass.Roof,                "FREEFORM")),
            ((PrimitiveShape.Plane,    IfcDomain.Architecture,   FitOrientation.Any),        (IfcClass.Covering,            "CLADDING")),
            ((PrimitiveShape.Plane,    IfcDomain.Infrastructure, FitOrientation.Any),        (IfcClass.Pavement,            "FLEXIBLE")),
            ((PrimitiveShape.Plane,    IfcDomain.Geotechnical,   FitOrientation.Any),        (IfcClass.GeotechnicalStratum, "SOLID")),
            ((PrimitiveShape.Plane,    IfcDomain.Electrical,     FitOrientation.Any),        (IfcClass.CableCarrierSegment, "CABLETRAYSEGMENT")),
            ((PrimitiveShape.Sphere,   IfcDomain.Architecture,   FitOrientation.Any),        (IfcClass.BuildingElementProxy, "ELEMENT")),
            ((PrimitiveShape.Sphere,   IfcDomain.HvacFire,       FitOrientation.Any),        (IfcClass.FlowTerminal,        "NOTDEFINED")),
            ((PrimitiveShape.Cylinder, IfcDomain.Architecture,   FitOrientation.Vertical),   (IfcClass.Column,              "COLUMN")),
            ((PrimitiveShape.Cylinder, IfcDomain.Architecture,   FitOrientation.Horizontal), (IfcClass.Beam,                "BEAM")),
            ((PrimitiveShape.Cylinder, IfcDomain.Structural,     FitOrientation.Vertical),   (IfcClass.Pile,                "BORED")),
            ((PrimitiveShape.Cylinder, IfcDomain.Geotechnical,   FitOrientation.Vertical),   (IfcClass.Borehole,            "NOTDEFINED")),
            ((PrimitiveShape.Cylinder, IfcDomain.Electrical,     FitOrientation.Any),        (IfcClass.CableSegment,        "CONDUCTORSEGMENT")),
            ((PrimitiveShape.Cylinder, IfcDomain.HvacFire,       FitOrientation.Any),        (IfcClass.FlowSegment,         "NOTDEFINED")),
            ((PrimitiveShape.Cone,     IfcDomain.Architecture,   FitOrientation.Any),        (IfcClass.Roof,                "FREEFORM")),
            ((PrimitiveShape.Cone,     IfcDomain.HvacFire,       FitOrientation.Any),        (IfcClass.FlowFitting,         "NOTDEFINED")),
            ((PrimitiveShape.Torus,    IfcDomain.HvacFire,       FitOrientation.Any),        (IfcClass.FlowFitting,         "NOTDEFINED")),
            ((PrimitiveShape.Freeform, IfcDomain.Geotechnical,   FitOrientation.Any),        (IfcClass.GeotechnicalStratum, "SOLID")),
            ((PrimitiveShape.Freeform, IfcDomain.Infrastructure, FitOrientation.Any),        (IfcClass.Course,              "PAVEMENT")),
            ((PrimitiveShape.Freeform, IfcDomain.Electrical,     FitOrientation.Any),        (IfcClass.CableSegment,        "CONDUCTORSEGMENT")),
            ((PrimitiveShape.Freeform, IfcDomain.Architecture,   FitOrientation.Any),        (IfcClass.BuildingElementProxy, "ELEMENT")));

    // Two tiers, one gate: the BiasOf Pin short-circuits the table for shape-independent classes (vegetation, rail),
    // else the effective domain (the bias, else the context discipline) keys the frozen table with the orientation
    // fallback to Any — and EVERY landing admits through the one 4-arg egress hop.
    public static Fin<(IfcClass Class, PredefinedType Predefined)> Classify(
        ReconstructionPrimitive primitive, SegmentedCloud segment, ReconstructionContext context, ReleaseVersion schema, Op key) {
        AsprsBias bias = ReconstructionContext.BiasOf(segment.DominantClass);
        return bias.Pin.Match(
            Some: pin => Admit(pin.Class, pin.Predefined, schema, key),
            None: () => {
                IfcDomain domain = bias.Domain.IfNone(context.Discipline);
                FitOrientation orientation = primitive.Switch(
                    plane:    p => context.OrientationOfNormal(p.Normal),
                    sphere:   static _ => FitOrientation.Any,
                    cylinder: c => context.OrientationOfAxis(c.Direction),
                    cone:     c => context.OrientationOfAxis(c.Direction),
                    torus:    static _ => FitOrientation.Any,
                    freeform: static _ => FitOrientation.Any);
                return (Table.Find((primitive.Shape, domain, orientation))
                        | Table.Find((primitive.Shape, domain, FitOrientation.Any)))
                    .ToFin(new BimFault.UnmappedClass(key, $"recon-shape-miss:{primitive.Shape.Key}:{domain}:{orientation}"))
                    .Bind(row => Admit(row.Class, row.Predefined, schema, key));
            });
    }

    // The ONE egress-gate hop — the frozen Model/elements#IFC_CLASS per-token span gate
    // AdmitPredefined(token, objectType, schema, key) -> Fin<string>; pin tier and table tier both admit through it.
    static Fin<(IfcClass Class, PredefinedType Predefined)> Admit(IfcClass @class, string predefined, ReleaseVersion schema, Op key) =>
        @class.AdmitPredefined(predefined, "", schema, key).Map(token => (@class, PredefinedType.Create(token)));
}

// --- [SERVICES] ---------------------------------------------------------------------------
// The scan-source PRIMARY projector: the kernel-segmented clouds are captured internally (the IElementProjection contract
// holds only Node/Relationship/GraphDelta), and Project mints the neutral rooted identity while recording a deterministic
// IFC GlobalId as the node ExternalId so a re-run dedups (§4-RT H6). The seam Assemble fold composes the GraphDelta.
public sealed class ReconstructionProjector(Seq<SegmentedCloud> segments, ReconstructionContext context) : IElementProjection {
    // Non-constructible ASPRS classes (noise 7/18, overlap 12, water 9) are EXCLUDED by the explicit BiasOf policy predicate
    // BEFORE authoring — a known-noise segment mints no phantom element; an UNMAPPED shape still faults loud.
    public Fin<GraphDelta> Project(ProjectionContext ctx) =>
        segments.Filter(static s => ReconstructionContext.BiasOf(s.DominantClass).Constructible)
            .Fold(
                Fin.Succ(GraphDelta.Empty.Reheader(ctx.Header)),
                (acc, segment) => acc.Bind(delta => Author(segment, ctx).Map(delta.Merge)));

    Fin<GraphDelta> Author(SegmentedCloud segment, ProjectionContext ctx) =>
        segment.Geometry.IsPending
            ? Fin.Fail<GraphDelta>(new BimFault.CapabilityMiss(ctx.Key, $"recon-unregistered:{segment.SegmentId}"))
            : Fin.Succ(ReconstructionPrimitive.Of(segment, context)).Bind(primitive =>
                ElementClassifier.Classify(primitive, segment, context, ctx.Header.Schema, ctx.Key)
                    .Bind(row => Build(primitive, segment, row, ctx)));

    Fin<GraphDelta> Build(ReconstructionPrimitive primitive, SegmentedCloud segment, (IfcClass Class, PredefinedType Predefined) row, ProjectionContext ctx) =>
        ReconstructionPset(primitive, segment, ctx.Header.Tolerance).Map(bag => {
            NodeId objectId = NodeId.Rooted();
            Node.Object element = new(
                Id:              objectId,
                Kind:            ObjectKind.Occurrence,
                ExternalId:      Some(ParserIfc.HashGlobalID($"recon:{primitive.Lineage.Value:X32}")),
                Classification:  Classification.Create("ifc", row.Class.Key, "", None, None, None),
                PredefinedType:  row.Predefined,
                Name:            $"{row.Class.Key}-recon-{segment.SegmentId.ToString(CultureInfo.InvariantCulture)}",
                Tag:             segment.SegmentId.ToString(CultureInfo.InvariantCulture),
                Representations: primitive.Keys(ctx.Header.Tolerance),
                History:         None,
                Span:            SchemaSpan.From(ctx.Header.Schema));
            return GraphDelta.Empty.Put(element).Put(bag)
                .Link(new Relationship.Assign(objectId, bag.Id, AssignKind.PropertyDefinition));
        });

    // The typed Pset_Reconstruction bag NODE: the fit evidence as PropertyValue/MeasureValue, never the retired stringly
    // PropertyBinding; PropertySource.Derived because the rows are computed fit evidence (the seam ValueBag 4-column
    // shape — SetName/Values/Inheritance/Source). AsprsClass records the modal class the BiasOf policy keyed on, the
    // classification provenance a review reads. The non-rooted id is the kernel content hash over the bag's canonical
    // bytes (the id is EXCLUDED from ToCanonicalBytes, so the empty-probe id is overwritten) so an identical bag dedups.
    // The five fit-evidence mints ride the seam OfSi finite gate first-fault — a NaN residual rails, never hashes.
    Fin<Node.PropertySet> ReconstructionPset(ReconstructionPrimitive primitive, SegmentedCloud segment, double tolerance) =>
        from confidence in MeasureValue.OfSi(Dimension.Dimensionless, primitive.Confidence.Value)
        from residual in MeasureValue.OfSi(Dimension.Dimensionless, segment.Residual)
        from inliers in MeasureValue.OfSi(Dimension.Dimensionless, segment.Inliers)
        from total in MeasureValue.OfSi(Dimension.Dimensionless, segment.Total)
        from asprs in MeasureValue.OfSi(Dimension.Dimensionless, segment.DominantClass)
        let bag = new PropertyBag("Pset_Reconstruction", Map<PropertyName, PropertyValue>(
            (PropertyName.Create("FitConfidence"),  new PropertyValue.Measure(confidence)),
            (PropertyName.Create("Residual"),       new PropertyValue.Measure(residual)),
            (PropertyName.Create("Inliers"),        new PropertyValue.Measure(inliers)),
            (PropertyName.Create("Total"),          new PropertyValue.Measure(total)),
            (PropertyName.Create("AsprsClass"),     new PropertyValue.Measure(asprs)),
            (PropertyName.Create("NeedsReview"),    new PropertyValue.Boolean(primitive.Confidence.IsBelow(context.ConfidenceFloor))),
            (PropertyName.Create("PrimitiveShape"), new PropertyValue.Enumerated(Seq(primitive.Shape.Key), PrimitiveShape.Items.AsIterable().Map(static s => s.Key).ToSeq())),
            (PropertyName.Create("SourceSegment"),  new PropertyValue.Text(segment.SegmentId.ToString(CultureInfo.InvariantCulture))),
            (PropertyName.Create("SourceCloud"),    new PropertyValue.Text(primitive.Lineage.Value.ToString("X32", CultureInfo.InvariantCulture)))),
            InheritanceMode.OccurrenceWins, PropertySource.Derived)
        let probe = new Node.PropertySet(NodeId.Content([]), bag)
        select probe with { Id = NodeId.Content(probe.ToCanonicalBytes(tolerance).Span) };
}
```

## [03]-[LAS_INGEST]

- Owner: `LasCloud` the decoded point carrier — the per-point position set (each `Position` a `MathNet.Numerics.LinearAlgebra.Vector<double>` the kernel registration and the Compute dense-LA substrate consume without a re-wrap), the per-point ASPRS `Classifications` the segmentation reduces to the `[02]-[RECONSTRUCTION]` `SegmentedCloud.DominantClass` discipline hint, the `ClassHistogram` folded from the decoded class bytes, the header `CountsByReturn`/`Min`/`Max` extrema and the integer-grid `Scale`/`Offset` quantization (the registration tolerance floor), the header `PointFormat` point-data-record format (which per-point facets the capture carries, formats 6-10 marking LAS 1.4 extended-class records), the source CRS WKT the app lowers onto the `Header.Reference` `GeoReference`, the source-cloud `ReconstructionLineage`, the point count, and the capture `Instant`; `LasCompression` the `[SmartEnum<string>]` discriminant (`Uncompressed`/`Compressed`); `LasIngest` the dual-engine decode fold — the scan-to-BIM FRONT decoding raw `.las`/`.laz` bytes into the `LasCloud` the kernel registration/segmentation consume. `Themis.Las` owns the uncompressed codec, `Unofficial.laszip.netstandard` the compressed codec, the kernel owns the fit; this owner re-mints none.
- Entry: `LasIngest.Decode(ReadOnlyMemory<byte> bytes, Instant at, Op key)` dispatches on `LasCompression.Sniff` — the ASPRS public-header point-data-record-format byte (offset 104) whose high bit marks LASzip compression — routing the uncompressed leg through `ReadLas` (the `Themis.Las` `LasReader` streaming every ASPRS point data record format 0-10 into a `LasPoint` whose `Position` is already a `MathNet` vector) and the compressed leg through `ReadLaz` (the `laszip` arithmetic decoder over a `MemoryStream`, `decompress_selective` masking the decode to `CHANNEL_RETURNS_XY | Z | CLASSIFICATION` so the arithmetic decoder SKIPS the RGB/waveform/extra-bytes a fit ignores, `get_coordinates` lifting the raw XYZ into the SAME `MathNet` vector through the copying `Vector<double>.Build.DenseOfArray`); `Fin<T>` traps a malformed header or unreadable archive into `Model/faults#FAULT_BAND` `BimFault.CodecReject` (`las-decode`) lifted BARE through the `Try.lift(...).Run().MapFail(...)` funnel — the `Op`-keyed case IS the `Error`, never a `.ToError()` hop.
- Auto: `Sniff` reads the compression marker WITHOUT a full open so the engine is selected once; `ReadLas` writes the bytes to one temp path the `LasReader` streams (the package's one shipped `IStreamHandler`, `AsyncStreamHandler`, is path-constructed — decompile-verified — so the byte admission is path-bound; `new LasReader(IStreamHandler)` is the stream growth seam), fills ONE caller-owned `LasPoint` through the no-alloc `GetNextPoint(ref LasPoint)` overload until `EOF` collecting each `Position.Clone()`/flag-masked `Classification` (the ref overload's `Update` mutates the ONE `Position` vector in place, so the clone detaches each collected position — a bare `Position` read aliases every slot onto the last decoded point; Themis passes the classification byte through RAW, so formats 0-5 mask `& 0x1F` off the synthetic/key-point/withheld flag bits while formats 6-10 keep the full dedicated class byte; a body shorter than the header count faults `las-decode` through the trap, never a hole-filled buffer), reads the `ILasHeader` extrema/scale/offset/`NumPointRecordsByReturn`/`PointDataFormat` receipt facts, and reads the `VLRs` record `2112` OGC WKT CRS; `ReadLaz` opens the in-memory stream (the object-store transport read), folds `read_point()`/`get_coordinates` over `get_number_of_point` (each call's non-zero `int` status gated through `Check` onto the decode-trap funnel, since the C-API reports failure by status not exception), reads the FORMAT-CORRECT class channel (`extended_classification` when `extended_point_type` marks a LAS 1.4 format-6-10 record, else the legacy `classification` — the legacy field is a 5-bit mask that truncates extended records), the `laszip_header` `min/max_*`/`*_scale_factor`/`*_offset`, `point_data_format` (compression bit masked), and by-return counts (the LAS 1.4 `extended_number_of_points_by_return` superseding the legacy counts when present), and reads the `header.vlrs` record `2112` CRS — both legs assemble one `LasCloud` whose lineage is the kernel seed-zero `XxHash128` over the raw bytes through the seam `CanonicalWriter` and whose `ClassHistogram` folds in one dense-array pass; the per-point ASPRS classes feed the kernel segmentation that reduces them to the per-segment modal `DominantClass`, and the CRS WKT feeds the app's `Header.Reference` `GeoReference` composition (the `Semantics/georeference#GEO_PROJECTION` `ProjNET` datum leg) so a georeferenced capture lands in the canonical kernel frame.
- Receipt: the `LasCloud` is the decoded scan evidence — the point count and per-return counts, the `ClassHistogram` computed from the decoded class bytes (evidence the header cannot forge), the header extrema and integer-grid scale/offset quantization, the point-data-record format, and the CRS WKT presence; the `ReconstructionLineage` over the source bytes joins the reconstructed model back to its capture so the `Review/diff#MODEL_DIFF` federation diff and the reality-capture playback re-fetch the exact LAS/LAZ by lineage key.
- Packages: `Themis.Las` (the MIT pure-managed uncompressed ASPRS LAS reader over `MathNet.Numerics`), `Unofficial.laszip.netstandard` (the LGPL-2.1 separate-assembly pure-managed LASzip codec — `.laz` arithmetic decode, selective-channel decompression, the `.lax` spatial-index bbox query), `Rasm.Element` (the seam `Projection/address#CANONICAL_WRITER` `CanonicalWriter`), `Rasm` (the kernel `Domain.ContentHash`), Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime.
- Growth: a new ASPRS point data record format is one `Themis.Las` `PointTypeMap` row (formats 0-10 share one reader, never a per-format reader family); a compression state is one `LasCompression` row dispatched by `Sniff`; a per-point facet (intensity, return index, GPS time, RGB/NIR) is one column the `LasPoint` facet set already carries, the `LasCloud.PointFormat` receipt column announcing which facets a capture holds without a re-decode; a tiled ingest enters through the `laszip` `.lax` `inside_rectangle` windowed path when an index exists; never a re-minted point-cloud decoder and never a second hashing scheme over the LAS/LAZ bytes.
- Boundary: the LAS/LAZ decode is the package surfaces' — `Themis.Las` `LasReader`/`LasPoint`/`ILasHeader`/`LasVariableLengthRecord` own the uncompressed stream and the `laszip` C-API codec the compressed stream, the `LasPoint.Position`/`get_coordinates` lifting into the one `MathNet.Numerics.LinearAlgebra.Vector<double>` the kernel registration consumes with no re-wrap, and a hand-rolled LAS byte-layout reader or a hand-rolled LAZ arithmetic decoder is the deleted form; the LGPL-2.1 `Unofficial.laszip.netstandard` is referenced as a SEPARATE assembly, never ILMerged, the in-Rhino plugin ALC firebreak preserved; the registration is the kernel's consumed by reference — `LasIngest` decodes only, never fits, and a re-minted RANSAC/region-grow fitter here is the deleted form; the CRS WKT VLR feeds the app's `GeoReference` composition (the `Semantics/georeference#GEO_PROJECTION` `ProjNET` leg) and a codec-local reprojection is the named seam violation; the source-cloud content key composes the kernel `Rasm.Domain.ContentHash` seed-zero `XxHash128` and a second hashing scheme or the upper-stratum `Rasm.Compute` interchange owner is the named drift defect; the decoded `LasPoint`/`laszip_point` types never leak past this fold — internal code holds the canonical `LasCloud`/`SegmentedCloud` per the boundary-mapping law.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using LanguageExt;
using LASzip.Net;
using MathNet.Numerics.LinearAlgebra;
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

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class LasCompression {
    public static readonly LasCompression Uncompressed = new("las");
    public static readonly LasCompression Compressed   = new("laz");

    // The ASPRS public-header point-data-record-format byte sits at offset 104; LASzip marks compression by setting its
    // high bit. The sniff selects the decode engine without a full open — Themis (MIT, MathNet-native) for the uncompressed
    // leg, the LGPL laszip arithmetic decoder for the compressed leg.
    public static LasCompression Sniff(ReadOnlySpan<byte> bytes) =>
        bytes.Length > 104 && (bytes[104] & 0x80) != 0 ? Compressed : Uncompressed;
}

// --- [MODELS] -----------------------------------------------------------------------------
// The decoded scan carrier PLUS its receipt facts: the ASPRS classification histogram (folded from the decoded
// class bytes — evidence the header cannot forge), the header point-count-by-return set, the header extrema and
// the integer-grid scale/offset (the quantization the capture was recorded at, the registration tolerance floor),
// and the point-data-record format (which per-point facets — intensity/GPS-time/RGB/NIR — the capture carries,
// readable without re-decoding; formats 6-10 mark the LAS 1.4 extended-class captures).
public sealed record LasCloud(
    ReadOnlyMemory<Vector<double>> Positions, ReadOnlyMemory<byte> Classifications,
    Map<byte, ulong> ClassHistogram, ReadOnlyMemory<ulong> CountsByReturn,
    Vector3 Min, Vector3 Max, Vector3 Scale, Vector3 Offset,
    Option<string> CrsWkt, ReconstructionLineage Lineage, byte PointFormat, ulong PointCount, Instant At);

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class LasIngest {
    public static Fin<LasCloud> Decode(ReadOnlyMemory<byte> bytes, Instant at, Op key) =>
        LasCompression.Sniff(bytes.Span).Switch(
            uncompressed: () => Trap("las", key, () => ReadLas(bytes, at)),
            compressed:   () => Trap("laz", key, () => ReadLaz(bytes, at)));

    static Fin<LasCloud> Trap(string codec, Op key, Func<LasCloud> read) =>
        Try.lift(read).Run().MapFail(error => new BimFault.CodecReject(key, $"{codec}-decode:{error.Message}"));

    // The laszip C-API signals failure by a NON-ZERO int status (get_error carries the message), never an exception, so
    // every status is gated here and a non-zero lifts the message into the Trap funnel that MapFails it to
    // BimFault.CodecReject — a raw status code never branches domain logic and a malformed LAZ never reads garbage past a
    // failed open/read. The Themis uncompressed leg needs no analog: its managed reader throws, which Try.lift catches.
    static void Check(laszip codec, int status) {
        if (status != 0) { throw new IOException(codec.get_error()); }
    }

    // The Themis uncompressed leg: ONE caller-owned LasPoint filled by the no-alloc GetNextPoint(ref) overload — the
    // per-point-allocating GetNextPoint() is the deleted form. LasPoint.Update MUTATES the one Position vector IN
    // PLACE (Position[0..2] scale+offset writes on the same instance), so each collected position detaches via
    // Clone() — one fresh MathNet vector per point; storing point.Position bare aliases every slot onto the last
    // decoded point. The byte admission span-writes one temp path (no whole-buffer ToArray copy) because the
    // package's one shipped IStreamHandler (AsyncStreamHandler) is path-ctor'd; LasReader(IStreamHandler) is the
    // stream growth seam. The ILasHeader carries the receipt facts: extrema, scale/offset, counts by return, point
    // format. A body shorter than the header count throws into the Trap funnel — a truncated capture never yields a
    // null-holed point buffer.
    static LasCloud ReadLas(ReadOnlyMemory<byte> bytes, Instant at) {
        string path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.las");
        using (FileStream sink = File.Create(path)) { sink.Write(bytes.Span); }
        try {
            using LasReader reader = new(path);
            Vector<double>[] positions = new Vector<double>[reader.PointCount];
            byte[] classes = new byte[reader.PointCount];
            // Themis surfaces the RAW classification byte: formats 0-5 pack synthetic/key-point/withheld flags into
            // bits 5-7 (a withheld ground point reads 130, not 2), formats 6-10 carry a full dedicated class byte —
            // the format-keyed mask strips the flag bits exactly as the laszip legacy getter does.
            byte classMask = reader.Header.PointDataFormat < 6 ? (byte)0x1F : (byte)0xFF;
            LasPoint point = new();
            ulong read = 0;
            for (; !reader.EOF && read < reader.PointCount; read++) {
                reader.GetNextPoint(ref point);
                positions[read] = point.Position.Clone();
                classes[read] = (byte)(point.Classification & classMask);
            }
            if (read < reader.PointCount) { throw new IOException($"las-truncated:{read}:{reader.PointCount}"); }
            Option<string> crs = reader.VLRs.AsIterable()
                .Filter(static vlr => vlr.RecordID == LasVariableLengthRecord.ProjectionRecordID).Head
                .Map(static vlr => Encoding.UTF8.GetString(vlr.Data).TrimEnd('\0'));
            ILasHeader h = reader.Header;
            return Assemble(bytes, positions, classes, crs, reader.PointCount, h.NumPointRecordsByReturn,
                new Vector3(h.MinX, h.MinY, h.MinZ), new Vector3(h.MaxX, h.MaxY, h.MaxZ),
                new Vector3(h.ScaleX, h.ScaleY, h.ScaleZ), new Vector3(h.OriginX, h.OriginY, h.OriginZ),
                h.PointDataFormat, at);
        } finally { File.Delete(path); }
    }

    // The laszip compressed leg: decompress_selective masks the decode to position+classification so the arithmetic decoder
    // skips RGB/waveform/extra-bytes a fit ignores; get_coordinates lifts the raw XYZ into the same MathNet vector; the
    // LAS 1.4 extended counts supersede the legacy uint counts when present. The class channel is FORMAT-CORRECT: a
    // format-6-10 record (extended_point_type set) carries its full class byte in extended_classification — the legacy
    // classification field is a 5-bit mask (& 0x1F) that truncates extended records to garbage. An array-backed memory
    // opens zero-copy; only a non-array-backed source pays the ToArray.
    static LasCloud ReadLaz(ReadOnlyMemory<byte> bytes, Instant at) {
        laszip codec = laszip.create();
        using MemoryStream stream = MemoryMarshal.TryGetArray(bytes, out ArraySegment<byte> segment)
            ? new(segment.Array!, segment.Offset, segment.Count, writable: false)
            : new(bytes.ToArray(), writable: false);
        try {
            Check(codec, codec.decompress_selective(LASZIP_DECOMPRESS_SELECTIVE.CHANNEL_RETURNS_XY | LASZIP_DECOMPRESS_SELECTIVE.Z | LASZIP_DECOMPRESS_SELECTIVE.CLASSIFICATION));
            Check(codec, codec.open_reader_stream(stream, out _, leaveOpen: true));
            Check(codec, codec.get_number_of_point(out long count));
            Vector<double>[] positions = new Vector<double>[count];
            byte[] classes = new byte[count];
            double[] xyz = new double[3];
            for (long i = 0; i < count; i++) {
                Check(codec, codec.read_point());
                Check(codec, codec.get_coordinates(xyz));
                positions[i] = Vector<double>.Build.DenseOfArray(xyz);
                classes[i] = codec.point.extended_point_type != 0 ? codec.point.extended_classification : codec.point.classification;
            }
            Option<string> crs = codec.header.vlrs.AsIterable()
                .Filter(static vlr => vlr.record_id == LasVariableLengthRecord.ProjectionRecordID).Head
                .Map(static vlr => Encoding.UTF8.GetString(vlr.data).TrimEnd('\0'));
            var h = codec.header;
            ulong[] byReturn = h.extended_number_of_point_records > 0
                ? h.extended_number_of_points_by_return
                : Array.ConvertAll(h.number_of_points_by_return, static c => (ulong)c);
            return Assemble(bytes, positions, classes, crs, (ulong)count, byReturn,
                new Vector3(h.min_x, h.min_y, h.min_z), new Vector3(h.max_x, h.max_y, h.max_z),
                new Vector3(h.x_scale_factor, h.y_scale_factor, h.z_scale_factor),
                new Vector3(h.x_offset, h.y_offset, h.z_offset),
                (byte)(h.point_data_format & 0x7F), at);   // the LASzip high bit masked off the stored format
        } finally { codec.close_reader(); }
    }

    // One LasCloud assembler shared by both legs: the cloud-level content key over the raw bytes (kernel seed-zero
    // XxHash128 through the seam CanonicalWriter — the ONE hasher, never the upper-stratum Compute interchange owner)
    // plus the ASPRS classification histogram folded in ONE dense-array pass over the decoded class bytes.
    static LasCloud Assemble(
        ReadOnlyMemory<byte> bytes, Vector<double>[] positions, byte[] classes, Option<string> crs, ulong count,
        ulong[] byReturn, Vector3 min, Vector3 max, Vector3 scale, Vector3 offset, byte pointFormat, Instant at) {
        var counts = new ulong[256];
        foreach (byte cls in classes) { counts[cls]++; }
        Map<byte, ulong> histogram = toMap(Enumerable.Range(0, 256).Where(c => counts[c] > 0).Select(c => ((byte)c, counts[c])));
        return new(positions, classes, histogram, byReturn, min, max, scale, offset, crs,
            ReconstructionLineage.Create(ContentHash.Of(new CanonicalWriter(0.0).String("las-cloud").Raw(bytes.Span).ToBytes().Span)),
            pointFormat, count, at);
    }
}
```

## [04]-[RESEARCH]

- [PROJECTOR_SEAM]: `ReconstructionProjector : IElementProjection` grounds against `Rasm.Element/Projection/projection#PROJECTION_CONTRACT` (the one polymorphic `Project(ProjectionContext) → Fin<GraphDelta>`, the seam capturing the foreign source internally and folding deltas) and §4-RT H6 — a PRIMARY projector mints the rooted element id through the kernel static `NodeId.Rooted()` (the neutral kernel `IObjectFactory` floor, a Guid-v7, NOT an IFC GlobalId; `ProjectionContext` exposes only `For`/`Owns`, never a mint pass-through) and records the compressed IFC `GlobalId` as the node's 1:1 `ExternalId` re-emitted at `Emit`; reconstruction is the scan-source twin of the `Projection/semantic#SEMANTIC_PROJECTOR` IFC projector, both establishing element identity and publishing it for aspect projectors, the `GraphDelta` the seam `Assemble` fold composes onto a `Genesis` seed. The deterministic `ParserIfc.HashGlobalID("recon:{lineage}")` `ExternalId` gives a re-run of the same capture the same stable join key so the `Review/diff#MODEL_DIFF` federation diff dedups it against its prior pass.
- [TYPED_PSET_COLLAPSE]: the `Pset_Reconstruction` `Node.PropertySet` carrying `PropertyValue.Measure`/`Boolean`/`Enumerated`/`Text` over the `Rasm.Element/Properties/quantity#MEASURE_VALUE` `MeasureValue` (`Dimension.Dimensionless` for the inlier ratios/counts) bound by a `Relationship.Assign(AssignKind.PropertyDefinition)` edge grounds against `Rasm.Element/Properties/property#PROPERTY_VALUE`/`#PROPERTY_BAG` and §2/§4B — the bag construction carries the seam `ValueBag<V>` 4-column shape (`SetName`/`Values`/`Inheritance`/`Source`) stamped `PropertySource.Derived` because the rows are computed fit evidence, and the `AsprsClass` row records the modal-class provenance the `BiasOf` policy keyed on; it collapses the retired stringly `BimElement.PropertyBinding(string,string,string)` triple the migration source tacked onto an off-element store, the seam `Bake` folding the bag into the element so a `ByProperty` read resolves `element.Properties.Find("FitConfidence")` as one typed `Option<PropertyValue>`.
- [CONTENT_IDENTITY_STRATA]: the `ReconstructionLineage` and the `RepresentationContentHash` `Body` key compose the kernel `Rasm.Domain.ContentHash` seed-zero `XxHash128` (the `Rasm.Element/Projection/address#CONTENT_ADDRESS` `ContentAddress` over the `Rasm.Element/Projection/address#CANONICAL_WRITER` `CanonicalWriter` projection + §4-RT H7) — the ONE hasher shared with the geometry `GeometryHash`, the snapshot spine, and the cross-runtime Python/TypeScript peers — and NOT the `Rasm.Compute` interchange owner, because `Rasm.Compute` is an APP-PLATFORM stratum ABOVE the AEC-DOMAIN `Rasm.Bim` and a `Rasm.Bim`→`Rasm.Compute` reference inverts the dependency DAG; the migration source's `Rasm.Compute.Interchange.InterchangeIdentity` use was the strata leak this rebuild closes.
- [PRIMITIVE_COMPLETENESS]: the six-arm `ReconstructionPrimitive` (`Plane`/`Sphere`/`Cylinder`/`Cone`/`Torus`/`Freeform`) IS the complete analytic-primitive set of efficient RANSAC point-cloud shape detection (Schnabel/Wahl/Klein — plane, sphere, cylinder, cone, torus) plus the residual freeform; the migration source's four-arm slice dropped `Sphere` (a dome/spherical-vessel fit) and `Cone` (a conical-roof/reducer fit), a naive slice of the domain. Each new arm widens the `ElementClassifier` by one `(shape, domain, orientation)` row resolving to a `Model/elements#IFC_CLASS` `IfcClass` whose `ValidPredefined` set admits the row's token at the `AdmitPredefined` egress gate (§4-RT C6) against `ctx.Header.Schema`.
- [DUAL_ENGINE_INGEST]: `LasIngest` dispatches by `LasCompression.Sniff` onto two engines per the `api-themis-las`/`api-laszip` integration law — `Themis.Las.LasReader` (MIT, `MathNet`-native `LasPoint.Position`, the `GetNextPoint(ref LasPoint)` no-alloc loop over ONE caller-owned point whose `Update` MUTATES the one `Position` vector in place — ctor, ref overload, and the `Position[0..2]` in-place writes decompile-verified against 2025.3.5 — so each collected position detaches via `Position.Clone()`, the bare read the aliasing defect that collapses the cloud to N copies of the last point; the classification byte passes through RAW — the format-0-5 structs pack synthetic/key-point/withheld flags into bits 5-7, decompile-verified unmasked, so the leg masks `& 0x1F` below format 6, the exact mask the laszip legacy getter applies) for the uncompressed leg and `Unofficial.laszip.netstandard` `laszip` (LGPL-2.1, separate-assembly, `decompress_selective(CHANNEL_RETURNS_XY | Z | CLASSIFICATION)` masking the arithmetic decode, `get_coordinates` → `Vector<double>.Build.DenseOfArray` — the copying lift over the one reused coordinate buffer, never the array-WRAPPING `Dense(double[])` that would alias every position onto one mutating buffer — and the format-correct class read `extended_point_type != 0 ? extended_classification : classification`, both members decompile-verified: the legacy `classification` getter masks `& 0x1F`, truncating every LAS 1.4 format-6-10 record) for the compressed leg, both lifting into the one `MathNet.Numerics.LinearAlgebra.Vector<double>` the kernel registration consumes; the Themis byte admission is PATH-BOUND — `new LasReader(IStreamHandler)` exists but the package's one shipped handler `AsyncStreamHandler` constructs from `(string, uint)` only (decompile-verified against 2025.3.5), so a stream-backed handler is a package-watch item and one temp path scoped by `try/finally` is the admission form, never a hand-implemented `IStreamHandler` re-minting the LAS decode the catalogue rejects; both legs read the public-header receipt facts (`ILasHeader.{Min*,Max*,Scale*,Origin*,NumPointRecordsByReturn}` / `laszip_header.{min/max_*, *_scale_factor, *_offset, number_of_points_by_return, extended_number_of_points_by_return}` — all decompile-verified) onto the widened `LasCloud`; the migration source's prose promised `.laz` support its code never wired, the hollow claim this rebuild closes by actually composing the admitted `laszip` codec AND gating its non-zero `int` status onto the `CodecReject` funnel (the C-API signals failure by status, not exception — an unchecked status reads garbage past a failed open, so `Check` lifts `get_error` through `Try.lift` per the `api-laszip` admission law).
- [ORIENTATION_AND_BIAS]: the axis-versus-normal orientation split corrects the migration source's inverted classification — `OrientationOfNormal` maps a vertical patch normal to a horizontal `Slab` while `OrientationOfAxis` maps a vertical swept axis to a vertical `Column`, so a vertical-axis cylinder no longer mis-classifies `Beam`; the two band thresholds are the trigonometric pair `sin(band)`/`cos(band)` (`VerticalCosineLimit` 0.342 = sin 20° with the derived `UprightCosine` √(1−limit²) = cos 20° — the arithmetic `1−limit` complement mis-slabbed a 45°-pitched roof); the `ReconstructionContext.BiasOf` TOTAL `AsprsBias` policy covers the LAS 1.4 standard classes on three axes grounded against the ASPRS classification standard and the `LasPoint.Classification`/`laszip_point.classification` channel the decode reads — DOMAIN bias (ground 2→Geotechnical, building 6→Architecture, road 11/bridge-deck 17→Infrastructure, wire 13/14→Electrical), CLASS PIN for the shape-independent site classes (vegetation 3/4/5→`GeographicElement`/`VEGETATION`; rail 10→`Rail`/`RAIL`, a member of the `Rail` row's valid set; transmission tower 15→`ElementAssembly`/`MAST`; wire-structure connector 16→`CableFitting`/`CONNECTOR` — `VEGETATION`/`MAST`/`CONNECTOR` decompile-verified on `IfcGeographicElementTypeEnum`/`IfcElementAssemblyTypeEnum`/`IfcCableFittingTypeEnum`, the generated LEG-BIM roster carrying the rows mechanically), and CONSTRUCTIBILITY (low/high noise 7/18, overlap 12, and water 9 excluded by the explicit `Project` filter — a noise segment mints no phantom element and a flat water return no pavement, per the explicit-`Filter`-before-construction rail law); the `ElementClassifier` table carries the rows every biased domain resolves for plane AND freeform lanes (Geotechnical→`GeotechnicalStratum`/`Borehole`, Infrastructure→`Pavement`+`Course`/`PAVEMENT`, Electrical→`CableCarrierSegment`/`CABLETRAYSEGMENT` plane + `CableSegment`/`CONDUCTORSEGMENT` cylinder-and-freeform — a sagging conductor fails the cylinder fit yet still lands `CONDUCTORSEGMENT`) so a ground/road/conductor segment yields a real `Model/elements#IFC_CLASS` `IfcClass` rather than steering into an empty domain — the six-class bias whose vegetation/rail/noise coverage gap this rebuild closes was the page's own named latent defect.
- [CONSUME_BY_REFERENCE]: the `SegmentedCloud` carrier grounds against the kernel `csharp:Rasm/Processing/register#ALIGN` cloud-ICP registration and `csharp:Rasm/Spatial/cloud#SEGMENTATION` plane/cylinder segmentation owners — the segmentation produces the shape-labeled segments already aligned into the canonical kernel frame, the `csharp:ROBUST_ARRANGEMENT_SUBSTRATE` exact-arithmetic arrangement bounds the planar `BoundaryPolygon`, the kernel computes the fitted-solid `GeometryHash`, and the `GeometryHandle.IsPending` registration handle confirms against that kernel geometry owner before the carrier is final; the orientation discriminant (`Vector3.Dot(normal.Unit, Vector3.UnitZ)`) is the SEAM `Rasm.Element/Graph/element#NODE_MODEL` `Vector3` coordinate algebra (NOT a kernel surface — no neutral kernel `Vector3` exists, the kernel coordinate being the RhinoCommon `Vector3d` this host-neutral fold never touches), composed Bim-side over the registered segment's fitted normal/axis to classify the patch; the consume-by-reference law holds — Bim re-fits no geometry, re-mints no arrangement, and content-hashes through the one kernel `XxHash128`, a `GeometryHandle.IsPending` segment faulted `BimFault.CapabilityMiss` rather than fitted in-process.
