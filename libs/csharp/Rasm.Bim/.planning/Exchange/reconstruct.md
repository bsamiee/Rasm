# [BIM_RECONSTRUCTION]

`ReconstructionProjector : IElementProjection` lowers a kernel-segmented point cloud into a seam `Rasm.Element/Graph/delta#GRAPH_DELTA` `GraphDelta` of `Rasm.Element/Graph/element#ELEMENT_GRAPH` `Node.Object` occurrence nodes, each carrying a typed `Pset_Reconstruction` bag bound by a neutral `Rasm.Element/Relations/relation#EDGE_ALGEBRA` `Relationship.Assign` edge, with the `LasIngest` LAS/LAZ decode front. Reconstruction is a PRIMARY projector, scan-source twin of the `Projection/semantic#SEMANTIC_PROJECTOR` IFC projector: it MINTS neutral rooted element identity through the kernel static `NodeId.Rooted()` (never an IFC GlobalId — §4-RT H6) and records a deterministic IFC `GlobalId` as its 1:1 `ExternalId` hashed from `ReconstructionLineage`, so a re-run dedups against its prior pass through the `Review/diff#MODEL_DIFF` federation diff rather than authoring a parallel record.

Reconstruction is BIM-semantics-only and CONSUME-BY-REFERENCE: `Themis.Las`/`Unofficial.laszip.netstandard` own the LAS/LAZ decode, the kernel owns registration and fit (`csharp:Rasm/Processing/register#ALIGN` cloud-ICP places the capture in the kernel frame, `csharp:Rasm/Spatial/cloud#SEGMENTATION` partitions it into `SegmentedCloud` rows bounded by the `csharp:ROBUST_ARRANGEMENT_SUBSTRATE` exact-arithmetic arrangement), and the geometry content key is the kernel `Rasm.Domain.ContentHash` seed-zero `XxHash128` the seam `Rasm.Element/Projection/address#CONTENT_ADDRESS` `ContentAddress` wraps over the `Rasm.Element/Projection/address#CANONICAL_WRITER` `CanonicalWriter` projection, never the upper-stratum `Rasm.Compute` interchange owner. Fitted primitives are HOST-NEUTRAL: `Node.Object` references ALL geometry by `RepresentationContentHash` content key only (`Body`/`FootPrint`/`Axis`, EACH a kernel `XxHash128` over the `CanonicalWriter` projection of its `Vector3` coordinates [M2]), so a `Rasm.Compute` runner resolves the analytical axis/footprint one-hop, never an inline coordinate field on the seam node (no `Node.Object.BoundaryPolygon`/`Axis` member exists) and never a RhinoCommon `Brep`/`Mesh`.

## [01]-[INDEX]

- [01]-[RECONSTRUCTION]: `ReconstructionProjector` folds segmented clouds into a `GraphDelta` of classified occurrence nodes with typed fit evidence.
- [02]-[LAS_INGEST]: `LasIngest.Decode` sniffs compression and folds `.las`/`.laz` bytes into one `LasCloud` the kernel registration consumes.

## [02]-[RECONSTRUCTION]

- Owner: `ReconstructionProjector` the `IElementProjection` folding kernel-segmented clouds to a seam `GraphDelta`; `ReconstructionPrimitive` the closed `[Union]` of fitted scan primitives, each arm carrying its analytic parameters, the kernel `GeometryHash`, its inlier `FitConfidence`, and the segment `ReconstructionLineage`; `PrimitiveShape` the `[SmartEnum<string>]` discriminant the `ElementClassifier` table keys on; `ReconstructionLineage` the `[ValueObject<UInt128>]` source-cloud content key; `FitConfidence` the `[ValueObject<double>]` normalized inlier-ratio band; `SegmentedCloud` the kernel-registered segment carrier; `ElementClassifier` the frozen shape-to-`IfcClass` projection.
- Cases: `ReconstructionPrimitive` arms `Plane`/`Sphere`/`Cylinder`/`Cone`/`Torus`/`Freeform` ARE the complete efficient-RANSAC shape-detection family with the residual freeform — a primitive family is one arm, one `PrimitiveShape` row, and one `ElementClassifier` entry, never a per-shape fold or a `FitPlane`/`FitCylinder` operation family; `ElementClassifier` rows are the `(shape, IfcDomain, orientation)`→`(IfcClass, predefined)` table, a wall-vs-slab disambiguation one row refines by orientation, never an enumerated `switch` arm.
- Entry: `ReconstructionProjector.Project(ProjectionContext ctx)` folds the constructible segments into one `GraphDelta`, seeding `GraphDelta.Empty.Reheader(ctx.Header)` from the app-supplied Header (the scan CRS WKT flows `LasCloud.CrsWkt`→app→`ctx.Header.Reference`, wiring is app-owned); a PRIMARY projector IGNORES `ctx.ElementIds` and PUBLISHES the rooted ids it mints for an aspect projector (`Rasm.Materials/Projection/component`) to attach `Associate` edges against; `Fin<T>` aborts on an unregistered segment (`Model/faults#FAULT_BAND` `BimFault.CapabilityMiss`) or an unmapped shape (`BimFault.UnmappedClass`), each `Op`-keyed case lifted BARE onto the rail (the `Expected`-derived case IS the `Error`, no `.ToError()` hop), the seam `Rasm.Element/Projection/projection#PROJECTION_CONTRACT` `Assemble` funnel capturing a thrown fault into `ElementFault.ProjectionFailed`.
- Auto: `Project` reads each `SegmentedCloud` already fitted and registered by the kernel, so the fold NEVER re-fits geometry in-process; a `segment.Geometry.IsPending` handle is an unregistered capture faulted `BimFault.CapabilityMiss`. `ReconstructionContext.BiasOf` governs first — a non-`Constructible` class is EXCLUDED by the explicit `Project` filter before authoring, a `Pin` class short-circuits the table because its IFC landing is shape-independent — else `ElementClassifier.Classify` keys the frozen table on the EFFECTIVE `IfcDomain` (the bias domain when present, else the context discipline) and the `FitOrientation`, where a planar patch reads `OrientationOfNormal` (a vertical normal is a horizontal slab) and a swept solid reads `OrientationOfAxis` (a vertical axis is a vertical column), the two mappings inverse; EVERY landing admits through the one `Model/elements#IFC_CLASS` `IfcClass.AdmitPredefined` per-token egress gate against `ctx.Header.Schema` (§4-RT C6). `Node.Object` mints a NEUTRAL rooted `NodeId` via `NodeId.Rooted()` (§4-RT H6) and records the deterministic `ParserIfc.HashGlobalID` IFC `GlobalId` as its 1:1 `ExternalId`; ALL geometry rides the `RepresentationContentHash` keyed map (`Body`/`FootPrint`/`Axis`) so `Rasm.Compute` resolves the analytical axis/footprint one-hop, never a node coordinate field; the typed `Pset_Reconstruction` bag carries fit evidence as `PropertyValue` and binds to the occurrence through a `Relationship.Assign(AssignKind.PropertyDefinition)` edge the seam `Bake` folds.
- Receipt: `GraphDelta` is the projector's whole contribution, the merge the seam `Assemble` folds with sibling deltas onto a `Genesis` seed; the `ReconstructionPrimitive` arm is the typed fit evidence, the `Pset_Reconstruction` bag the per-element review record a `Persistence`/`Compute` `ByProperty` read selects below-floor elements on, and the deterministic `ExternalId` joins a re-reconstructed element to its prior pass and its as-designed counterpart across the federation diff — no generic `IFitResult` abstraction, the union arms stay typed per primitive family.
- Packages: `Rasm.Element` (the seam `Node`/`NodeId`/`GraphDelta`/`Relationship`/`Classification`/`PredefinedType`/`PropertyBag`/`PropertyValue`/`MeasureValue`/`Dimension`/`RepresentationContentHash`/`AxisCurve`/`SchemaSpan`, the `Projection/address#CANONICAL_WRITER` `CanonicalWriter`, the `IElementProjection`/`ProjectionContext` contract, and the seam-owned host-neutral `Graph/element#NODE_MODEL` `Vector3` coordinate with its `Dot`/`Unit`/`UnitX`/`UnitZ` algebra the orientation classifier folds — the seam owns the analytical `Vector3` the way it owns `Dimension`, and no kernel `Vector3` exists), `Rasm` (the `GeometryHandle` registration handle and the `Domain.ContentHash` seed-zero `XxHash128`, consumed by reference; the kernel `Rasm.Numerics` coordinate is the RhinoCommon `Vector3d` this host-neutral projection never touches), GeometryGymIFC_Core (`ParserIfc.HashGlobalID` the deterministic GlobalId codec), Thinktecture.Runtime.Extensions (`[Union]`/`[SmartEnum]`/`[ValueObject]`), LanguageExt.Core (`Fin`/`Seq`/`Map`/`Option`).
- Growth: a new fitted primitive is one `ReconstructionPrimitive` arm carrying its analytic parameters, one `PrimitiveShape` row, and one `ElementClassifier` entry — the fold and classifier resolve it with no new operation; a new classification rule is one `ElementClassifier` row keyed on `(PrimitiveShape, IfcDomain, FitOrientation)`; a repeated identical fit shares ONE `GeometryHash` so the content-keyed blob store dedups the geometry with no parallel type-instance; a new confidence dimension is one `Pset_Reconstruction` row; a new discipline bias is one `BiasOf` arm with the `ElementClassifier` rows it resolves to (a bias arm with no matching rows steers a segment into an empty domain and faults `recon-shape-miss`), a shape-independent site class one `Pin` row, a non-constructible class one `Excluded` row — `AsprsBias` is the one growth surface for all three; never a per-shape `Node.Object` subtype or a second receipt model.
- Boundary: reconstruction is the LAST fold to a seam `Node.Object`, never a geometry kernel — kernel cloud-ICP registration, plane/cylinder segmentation, and exact-arithmetic arrangement are consumed by reference, never re-minted here; the source-cloud content key composes the kernel `Rasm.Domain.ContentHash` seed-zero `XxHash128` through the seam `CanonicalWriter`, never the upper-stratum `Rasm.Compute` interchange owner (a `Rasm.Bim`→`Rasm.Compute` reference inverts the strata DAG) or a second hasher; ALL fitted geometry rides the `RepresentationContentHash` keyed map (`Body`/`FootPrint`/`Axis`), so the seam `Node.Object` carries no inline coordinate field, no RhinoCommon `Brep`/`Mesh`, and no stored `GeometryHandle` — host-neutral by construction; the rooted `NodeId` is the NEUTRAL kernel-minted id and the IFC `GlobalId` is the node's `ExternalId` (§4-RT H6), a deterministic mint giving re-run dedup without making the GUID the node identity; a reconstructed element is a `Node.Object` on the same generic `Classification`/`PredefinedType` axes an IFC-ingested element carries, so `Model/query` and `Review/validation` read it with no second selection surface; fit evidence rides the typed `Pset_Reconstruction` `PropertyValue` bag the seam property store owns; an unmapped shape faults `BimFault.UnmappedClass` and an unregistered segment `BimFault.CapabilityMiss`, so an unclassifiable scan never silently produces a half-built model, distinct from the KNOWN-non-constructible ASPRS classes the `BiasOf` policy excludes by explicit filter before authoring — a deliberate policy row, never a dropped fault.

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
// Complete efficient-RANSAC analytic-primitive set (plane/sphere/cylinder/cone/torus) with the residual freeform.
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

// AsprsBias policy row: Constructible=false EXCLUDES the class from element minting (noise/overlap/water — an explicit
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

// Source-cloud content key — the kernel seed-zero XxHash128 over the segment bytes + fit params through the seam
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

    // TOTAL ASPRS dominant-class policy over the LAS 1.4 standard classes: ground 2 -> Geotechnical, low/medium/
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

// Bim ingress carrier Reconstruct reads, assembled at the boundary from the kernel's geometric segmentation output:
// kernel fits the primitive and detects its shape, this carrier and its PrimitiveShape discriminant staying Bim-side so
// no kernel type references a Bim type (no downward dep). SoA fields the ReconstructionPrimitive arms project into typed
// shape evidence — GeometryHash the kernel-computed content key of the fitted solid (consumed by reference, never
// re-fit), BoundaryPolygon the arrangement-bounded planar patch, AxisStart/AxisEnd the swept axis extent, and
// DominantClass the segment's modal ASPRS class.
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

    // Analytical surface polygon of a planar wall/slab, the non-planar arms carrying none — content-keyed into the
    // Node.Object Representations map under the "FootPrint" key (Keys, below), NEVER inlined as a coordinate field on the
    // seam node (the deleted §4-RT-M2 violation) — a Rasm.Compute energy runner resolves it one-hop from the blob store.
    public Seq<Vector3> BoundaryPolygon => Switch(
        plane:    static p => p.Boundary, sphere:   static _ => Seq<Vector3>(),
        cylinder: static _ => Seq<Vector3>(), cone:  static _ => Seq<Vector3>(),
        torus:    static _ => Seq<Vector3>(), freeform: static _ => Seq<Vector3>());

    // Idealized structural line of a swept column/beam, the non-swept arms carrying none — content-keyed into the
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

    // Keyed geometry map [M2]: the fitted-solid display geometry rides "Body" (the kernel GeometryHash), the analytical
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
    // Frozen (shape, domain, orientation) -> (IfcClass, predefined) projection — a data table, not enumerated switch arms.
    // Predefined tokens are members of the Model/elements#IFC_CLASS valid sets, admitted at the egress gate; the domain
    // axis carries every IfcDomain the BiasOf ASPRS bias yields (Architecture/Structural/HvacFire AND the outdoor
    // Geotechnical/Infrastructure/Electrical rows — plane AND freeform lanes for the outdoor domains), so a ground/road/
    // conductor segment resolves a real class rather than an empty-domain miss, and the shape-independent site classes
    // ride the BiasOf Pin tier, never table rows. A row whose Ifc4x3 infrastructure/geotechnical class the older target
    // schema cannot carry faults class-out-of-schema at AdmitPredefined (not recon-shape-miss), the egress gate's job,
    // never a silent half-model.
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

    // One egress-gate hop — the frozen Model/elements#IFC_CLASS per-token span gate
    // AdmitPredefined(token, objectType, schema, key) -> Fin<string>; pin tier and table tier both admit through it.
    static Fin<(IfcClass Class, PredefinedType Predefined)> Admit(IfcClass @class, string predefined, ReleaseVersion schema, Op key) =>
        @class.AdmitPredefined(predefined, "", schema, key).Map(token => (@class, PredefinedType.Create(token)));
}

// --- [SERVICES] ---------------------------------------------------------------------------
// Scan-source PRIMARY projector: the kernel-segmented clouds are captured internally (the IElementProjection contract
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

    // Typed Pset_Reconstruction bag NODE: the fit evidence as PropertyValue/MeasureValue, never the retired stringly
    // PropertyBinding; PropertySource.Derived because the rows are computed fit evidence (the seam ValueBag 4-column
    // shape — SetName/Values/Inheritance/Source). AsprsClass records the modal class the BiasOf policy keyed on, the
    // classification provenance a review reads. Non-rooted id is the kernel content hash over the bag's canonical
    // bytes (the id is EXCLUDED from ToCanonicalBytes, so the empty-probe id is overwritten) so an identical bag dedups.
    // Five fit-evidence mints ride the seam OfSi finite gate first-fault — a NaN residual rails, never hashes.
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

- Owner: `LasCloud` the decoded point carrier — position set (each `Position` a `MathNet.Numerics.LinearAlgebra.Vector<double>` the kernel registration and Compute dense-LA substrate consume without a re-wrap), the per-point ASPRS `Classifications` the segmentation reduces to the `[02]-[RECONSTRUCTION]` `SegmentedCloud.DominantClass` hint, and the header receipt facts (`ClassHistogram`, `CountsByReturn`, extrema, integer-grid `Scale`/`Offset`, `PointFormat`, CRS WKT, `ReconstructionLineage`, count, `Instant`); `LasCompression` the `[SmartEnum<string>]` discriminant; `LasIngest` the dual-engine decode fold decoding raw `.las`/`.laz` bytes into the `LasCloud` the kernel registration/segmentation consume — `Themis.Las` owns the uncompressed codec, `Unofficial.laszip.netstandard` the compressed codec, the kernel owns the fit; this owner re-mints none.
- Entry: `LasIngest.Decode(ReadOnlyMemory<byte> bytes, Instant at, Op key)` dispatches on `LasCompression.Sniff` (the offset-104 public-header byte whose high bit marks LASzip compression), routing the uncompressed leg through `ReadLas` and the compressed leg through `ReadLaz`; `Fin<T>` traps a malformed header or unreadable archive into `Model/faults#FAULT_BAND` `BimFault.CodecReject` lifted BARE through the `Try.lift` funnel, the `Op`-keyed case IS the `Error`, never a `.ToError()` hop.
- Auto: `Sniff` selects the engine from the compression marker WITHOUT a full open; `ReadLas` streams the `Themis.Las` `LasReader` over one temp path (byte admission is path-bound — the one shipped `AsyncStreamHandler` is path-constructed), and `ReadLaz` folds the `laszip` decoder over the in-memory stream gating each non-zero C-API status through `Check`; both mask the classification format-correctly (formats 0-5 strip the flag bits `& 0x1F`, formats 6-10 keep the full class byte), read the header receipt facts and the record-`2112` OGC WKT CRS, and assemble one `LasCloud` whose lineage is the kernel `XxHash128` over the raw bytes through the seam `CanonicalWriter` and whose `ClassHistogram` folds in one dense-array pass; the per-point ASPRS classes feed the kernel segmentation reducing them to the per-segment modal `DominantClass`, and the CRS WKT feeds the app's `Header.Reference` `GeoReference` (`Semantics/georeference#GEO_PROJECTION` `ProjNET` leg) so a georeferenced capture lands in the canonical kernel frame.
- Receipt: `LasCloud` is the decoded scan evidence — point/per-return counts, the `ClassHistogram` computed from the decoded class bytes (evidence the header cannot forge), header extrema and quantization, point-data-record format, and CRS WKT presence; the `ReconstructionLineage` over the source bytes joins the reconstructed model back to its capture so the `Review/diff#MODEL_DIFF` federation diff and reality-capture playback re-fetch the exact LAS/LAZ by lineage key.
- Packages: `Themis.Las` (the MIT pure-managed uncompressed ASPRS LAS reader over `MathNet.Numerics`), `Unofficial.laszip.netstandard` (the LGPL-2.1 separate-assembly pure-managed LASzip codec — `.laz` arithmetic decode, selective-channel decompression, the `.lax` spatial-index bbox query), `Rasm.Element` (the seam `Projection/address#CANONICAL_WRITER` `CanonicalWriter`), `Rasm` (the kernel `Domain.ContentHash`), Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime.
- Growth: a new ASPRS point data record format is one `Themis.Las` `PointTypeMap` row (formats 0-10 share one reader, never a per-format reader family); a compression state is one `LasCompression` row dispatched by `Sniff`; a per-point facet (intensity, return index, GPS time, RGB/NIR) is one column the `LasPoint` facet set already carries, the `LasCloud.PointFormat` receipt column announcing which facets a capture holds without a re-decode; a tiled ingest enters through the `laszip` `.lax` `inside_rectangle` windowed path when an index exists; never a re-minted point-cloud decoder and never a second hashing scheme over the LAS/LAZ bytes.
- Boundary: `Themis.Las` (`LasReader`/`LasPoint`/`ILasHeader`/`LasVariableLengthRecord`) owns the uncompressed stream and the `laszip` C-API codec the compressed stream, `LasPoint.Position`/`get_coordinates` lifting into the one `MathNet.Numerics.LinearAlgebra.Vector<double>` the kernel registration consumes with no re-wrap, never a hand-rolled LAS/LAZ reader; the LGPL-2.1 `Unofficial.laszip.netstandard` is referenced as a SEPARATE assembly, never ILMerged, so the in-Rhino plugin ALC firebreak holds; `LasIngest` decodes only and never fits, registration staying the kernel's by reference; the CRS WKT VLR feeds the app's `GeoReference` (`Semantics/georeference#GEO_PROJECTION` `ProjNET` leg), never a codec-local reprojection; the source-cloud content key composes the kernel `Rasm.Domain.ContentHash` seed-zero `XxHash128`, never a second hasher or the upper-stratum `Rasm.Compute` interchange owner; the decoded `LasPoint`/`laszip_point` types never leak past this fold — internal code holds the canonical `LasCloud`/`SegmentedCloud` per the boundary-mapping law.

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

    // ASPRS public-header point-data-record-format byte sits at offset 104; LASzip marks compression by setting its
    // high bit. The sniff selects the decode engine without a full open — Themis (MIT, MathNet-native) for the uncompressed
    // leg, the LGPL laszip arithmetic decoder for the compressed leg.
    public static LasCompression Sniff(ReadOnlySpan<byte> bytes) =>
        bytes.Length > 104 && (bytes[104] & 0x80) != 0 ? Compressed : Uncompressed;
}

// --- [MODELS] -----------------------------------------------------------------------------
// Decoded scan carrier with its receipt facts: the ASPRS classification histogram (folded from the decoded class
// bytes — evidence the header cannot forge), the header point-count-by-return set, header extrema and the integer-grid
// scale/offset (the quantization the capture was recorded at, the registration tolerance floor), and the point-data-
// record format (which per-point facets — intensity/GPS-time/RGB/NIR — the capture carries, readable without re-
// decoding; formats 6-10 mark the LAS 1.4 extended-class captures).
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

    // laszip C-API signals failure by a NON-ZERO int status (get_error carries the message), never an exception, so
    // every status is gated here and a non-zero lifts the message into the Trap funnel that MapFails it to
    // BimFault.CodecReject — a raw status code never branches domain logic and a malformed LAZ never reads garbage past a
    // failed open/read. The Themis uncompressed leg needs no analog: its managed reader throws, which Try.lift catches.
    static void Check(laszip codec, int status) {
        if (status != 0) { throw new IOException(codec.get_error()); }
    }

    // Themis uncompressed leg: ONE caller-owned LasPoint filled by the no-alloc GetNextPoint(ref) overload — the
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
            // format-keyed mask strips the flag bits exactly as the laszip legacy getter does.
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

    // laszip compressed leg: decompress_selective masks the decode to position+classification so the arithmetic decoder
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
    // with the ASPRS classification histogram folded in ONE dense-array pass over the decoded class bytes.
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

(none)
