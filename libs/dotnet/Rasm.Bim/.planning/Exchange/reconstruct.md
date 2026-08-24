# [BIM_RECONSTRUCTION]

`ReconstructionProjector : IElementProjection` lowers a kernel-segmented point cloud into a seam `Rasm.Element/Graph/delta#GRAPH_DELTA` `GraphDelta` of `Rasm.Element/Graph/element#ELEMENT_GRAPH` `Node.Object` occurrence nodes, each carrying a typed `Pset_Reconstruction` bag bound by a neutral `Rasm.Element/Relations/relation#EDGE_ALGEBRA` `Relationship.Assign` edge, with the `LasIngest` LAS/LAZ decode front.

Reconstruction is a PRIMARY projector, scan-source twin of the `Projection/semantic#SEMANTIC_PROJECTOR` IFC projector: it MINTS neutral rooted element identity through `NodeId.Of(new NodeSeed.Placement())` and records a deterministic IFC `GlobalId` as its 1:1 `ExternalId`, hashed from the `ReconstructionKey` run identity so a re-run at identical fit parameters dedups against its prior pass through the `Review/diff#MODEL_DIFF` federation diff.

Reconstruction is BIM-semantics-only and CONSUME-BY-REFERENCE: `Themis.Las`/`Unofficial.laszip.netstandard` own the LAS/LAZ decode, the kernel owns registration and fit (`Rasm/Processing/register#REGISTRATION` cloud-ICP places the capture in the kernel frame, `Rasm/Processing/segment#SEGMENTATION` partitions it into `SegmentedCloud` rows bounded by the `dotnet:ROBUST_ARRANGEMENT_SUBSTRATE` exact-arithmetic arrangement).

Geometry content keys are the kernel `Rasm.Domain.ContentHash` seed-zero `XxHash128` the seam `Rasm.Element/Projection/address#CONTENT_ADDRESS` `ContentAddress` wraps over the kernel `Rasm/Domain/identity#CONTENT_KEY` `CanonicalWriter` projection, never the upper-stratum `Rasm.Compute` interchange owner.

Fitted primitives are HOST-NEUTRAL — `Node.Object` references ALL geometry by `RepresentationContentHash` content key only (`Body`/`FootPrint`/`Axis`, EACH a kernel `XxHash128` over the `CanonicalWriter` projection of its `Vector3` coordinates), so a `Rasm.Compute` runner resolves the analytical axis/footprint one-hop, never an inline coordinate field on the seam node (no `Node.Object.BoundaryPolygon`/`Axis` member exists) and never a RhinoCommon `Brep`/`Mesh`.

## [01]-[INDEX]

- [02]-[RECONSTRUCTION]: `ReconstructionProjector` folds segmented clouds into a `GraphDelta` of classified occurrence nodes with typed fit evidence.
- [03]-[LAS_INGEST]: `LasIngest.Decode` sniffs compression and folds `.las`/`.laz` bytes into one `LasCloud` the kernel registration consumes, and `LasIngest.Pyramid` draws that carrier's colour-weighted `CloudLevel` detail bands over the shared interchange ratio schedule.

## [02]-[RECONSTRUCTION]

- Owner: `ReconstructionProjector` the `IElementProjection` folding kernel-segmented clouds to a seam `GraphDelta`; `ReconstructionPrimitive` the ONE fit row carrying the columns every fit has — segment, kernel `GeometryHash`, inlier `FitConfidence`, `ReconstructionKey` — beside the closed `PrimitiveForm` union holding shape-specific payload alone, with `PrimitiveAnalytic` the single per-shape projection every consumer reads; `PrimitiveShape` the `[SmartEnum<string>]` discriminant the `ElementClassifier` table keys on; `CaptureLineage` the `[ValueObject<UInt128>]` source-bytes address and `ReconstructionKey` the `[ValueObject<UInt128>]` run identity, two disjoint key spaces with two names; `FitConfidence` the `[ValueObject<double>]` normalized inlier-ratio band; `SizeBand` the classifier's scale floor; `SegmentedCloud` the kernel-registered segment carrier; `ElementClassifier` the frozen shape-to-`IfcClass` projection.
- Cases: `PrimitiveForm` arms `Plane`/`Sphere`/`Cylinder`/`Cone`/`Torus`/`Freeform` ARE the complete efficient-RANSAC shape-detection family with the residual freeform — a primitive family is one arm, one `PrimitiveShape` row, and one `ElementClassifier` entry, never a per-shape fold or a `FitPlane`/`FitCylinder` operation family, and the shared columns sit on the row above the union so no arm can answer one of them differently; `ElementClassifier` rows are the `(shape, IfcDomain, orientation)`→`(IfcClass, predefined, SizeBand)` table, a wall-vs-slab disambiguation one row refines by orientation and scale a row COLUMN rather than a fourth key axis, never an enumerated `switch` arm.
- Entry: `ReconstructionProjector.Project(ProjectionContext ctx)` folds the constructible segments into one `GraphDelta`, seeding `GraphDelta.Empty.Reheader(ctx.Header)` from the app-supplied Header (the scan CRS WKT flows `LasCloud.CrsWkt`→app→`ctx.Header.Reference`, wiring is app-owned); a PRIMARY projector IGNORES `ctx.ElementIds` and PUBLISHES the rooted ids it mints for an aspect projector (`Rasm.Materials/Projection/component`) to attach `Associate` edges against; `Fin<T>` aborts on an unregistered segment (`Model/faults#FAULT_BAND` `BimFault.Refused` with `BimReason.Capability`) or a shape the classifier places at neither a table row nor its scale band (`BimFault.Refused` with `BimReason.Unmapped`), each `Op`-keyed case lifted BARE onto the rail (the `Fault`-derived case IS the `Error`, no `.ToError()` hop), while the seam assembly capture preserves any unknown thrown error exactly.
- Auto: `Project` reads each `SegmentedCloud` already fitted and registered by the kernel, so the fold NEVER re-fits geometry in-process; a `segment.Geometry.IsPending` handle is an unregistered capture faulted `BimFault.Refused` with `BimReason.Capability`. `ReconstructionContext.BiasOf` governs first — an `AsprsBias.Excluded` class is refused by the explicit `Project` filter before authoring, a `Pin` class short-circuits the table because its IFC landing is shape-independent — else `ElementClassifier.Classify` keys the frozen table on the EFFECTIVE `IfcDomain` (the bias domain when present, else the context discipline) and the `FitOrientation` the fit's own published datum selects, where a planar patch reads `OrientationOfNormal` (a vertical normal is a horizontal slab) and a swept solid reads `OrientationOfAxis` (a vertical axis is a vertical column), the two mappings inverse, and the resolved row's `SizeBand` then gates the fit's gauge so a 6 mm cylinder never lands `IfcClass.Column`; EVERY landing admits through the one `Model/elements#IFC_CLASS` `IfcClass.AdmitPredefined` per-token egress gate against `ctx.Header.Schema`. `Node.Object` mints a NEUTRAL rooted `NodeId` via `NodeId.Of(new NodeSeed.Placement())` and records the deterministic `ParserIfc.HashGlobalID` IFC `GlobalId` as its 1:1 `ExternalId`; ALL geometry rides the `RepresentationContentHash` keyed map (`Body`/`FootPrint`/`Axis`) so `Rasm.Compute` resolves the analytical axis/footprint one-hop, never a node coordinate field; the typed `Pset_Reconstruction` bag carries fit evidence as `PropertyValue` and binds to the occurrence through a `Relationship.Assign(AssignKind.PropertyDefinition)` edge the seam `Bake` folds.
- Receipt: `GraphDelta` is the projector's whole contribution, the merge the seam `Assemble` folds with sibling deltas onto a `Genesis` seed; the `ReconstructionPrimitive` row and its `PrimitiveForm` payload are the typed fit evidence, the `Pset_Reconstruction` bag the per-element review record a `Persistence`/`Compute` `ByProperty` read selects below-floor elements on, and the deterministic `ExternalId` joins a re-reconstructed element to its prior pass and its as-designed counterpart across the federation diff — no generic `IFitResult` abstraction, the union arms stay typed per primitive family.
- Packages: `Rasm.Element` (the seam `Node`/`NodeId`/`GraphDelta`/`Relationship`/`Classification`/`PredefinedType`/`PropertyBag`/`PropertyValue`/`MeasureValue`/`Dimension`/`RepresentationContentHash`/`AxisCurve`/`SchemaSpan`, the `IElementProjection`/`ProjectionContext` contract, and the seam-owned host-neutral `Graph/element#NODE_MODEL` `Vector3` coordinate with its `Dot`/`Unit`/`UnitX`/`UnitZ` algebra the orientation classifier folds — the seam owns the analytical `Vector3` the way it owns `Dimension`, and no kernel `Vector3` exists), `Rasm` (the `GeometryHandle` registration handle, the `Domain.ContentHash` seed-zero `XxHash128`, and the `Rasm/Domain/identity#CONTENT_KEY` `CanonicalWriter`, consumed by reference; the kernel `Rasm.Numerics` coordinate is the RhinoCommon `Vector3d` this host-neutral projection never touches), GeometryGymIFC_Core (`ParserIfc.HashGlobalID` the deterministic GlobalId codec), Thinktecture.Runtime.Extensions (`[Union]`/`[SmartEnum]`/`[ValueObject]`), LanguageExt.Core (`Fin`/`Seq`/`Map`/`Option`).
- Growth: a new fitted primitive is one `PrimitiveForm` arm carrying its analytic parameters, one `PrimitiveAnalytic` arm on the single per-shape dispatch, one `PrimitiveShape` row, and one `ElementClassifier` entry — the fold and classifier resolve it with no new operation; a new classification rule is one `ElementClassifier` row keyed on `(PrimitiveShape, IfcDomain, FitOrientation)` and a new scale floor one `SizeBand` value on the rows that carry it; a repeated identical fit shares ONE `GeometryHash` so the content-keyed blob store dedups the geometry with no parallel type-instance; a new confidence dimension is one `Pset_Reconstruction` row; a new discipline bias is one `BiasOf` arm with the `ElementClassifier` rows it resolves to (a bias arm with no matching rows steers a segment into an empty domain and faults `recon-shape-miss`), a shape-independent site class one `Pin` row, a non-constructible class one `AsprsBias.Excluded` row — `AsprsBias` is the one growth surface for all three; never a per-shape `Node.Object` subtype or a second receipt model.
- Boundary: reconstruction is the LAST fold to a seam `Node.Object`, never a geometry kernel — kernel cloud-ICP registration, plane/cylinder segmentation, and exact-arithmetic arrangement are consumed by reference, never re-minted here; both lineage keys compose the kernel `Rasm.Domain.ContentHash` seed-zero `XxHash128` through the seam `CanonicalWriter`, never the upper-stratum `Rasm.Compute` interchange owner (a `Rasm.Bim`→`Rasm.Compute` reference inverts the strata DAG) or a second hasher; ALL fitted geometry rides the `RepresentationContentHash` keyed map (`Body`/`FootPrint`/`Axis`), so the seam `Node.Object` carries no inline coordinate field, no RhinoCommon `Brep`/`Mesh`, and no stored `GeometryHandle` — host-neutral by construction; lineage is TWO axes with two names and two consumers — `CaptureLineage` addresses the source bytes and is what the `SourceCloud` row publishes and a re-fetch resolves, `ReconstructionKey` identifies one fit run under its own parameters and is what the `ReconstructionRun` row publishes and the deterministic `ExternalId` hashes from; one value type over both key spaces let the advertised re-fetch join cite a key carrying fit parameters in its preimage, which no store can answer; the rooted `NodeId` is the NEUTRAL kernel-minted id and the IFC `GlobalId` is the node's `ExternalId`, a deterministic mint giving re-run dedup without making the GUID the node identity; a reconstructed element is a `Node.Object` on the same generic `Classification`/`PredefinedType` axes an IFC-ingested element carries, so `Model/query` and `Review/validation` read it with no second selection surface; fit evidence rides the typed `Pset_Reconstruction` `PropertyValue` bag the seam property store owns; an unmapped shape faults `BimFault.Refused` with `BimReason.Unmapped` and an unregistered segment `BimFault.Refused` with `BimReason.Capability`, so an unclassifiable scan never silently produces a half-built model, distinct from the KNOWN-non-constructible ASPRS classes the `BiasOf` policy excludes by explicit filter before authoring — a deliberate policy row, never a dropped fault; the classifier's honest reach ends at per-segment single-primitive evidence, and four adjudicated NEGATIVES hold instead of fabricated rows — a Controls instrument publishes no primitive signature (a sensor, actuator, or controller is a fitting-scale blob any small fixture matches, and no ASPRS class biases a segment into Controls, so a Controls-disciplined context resolves through the fallback lanes), a stair is a repeated-tread COMPOSITION no single fit expresses and the kernel publishes no repetition signature, a railing's discriminant is guard height above a walking surface no per-segment fit datum carries (absolute Z is published, the floor it stands off is not), and a door or window is a point-ABSENCE void whose hole topology the single-ring `BoundaryPolygon` does not carry — each lane re-opens only when the kernel mints the evidence it needs (a composed repetition signature, a floor-relative datum, a hole-bearing ring), never through a row whose key cannot honestly discriminate it.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Globalization;
using GeometryGym.Ifc;
using LanguageExt;
using Rasm;
using Rasm.Domain;
using Rasm.Bim.Model;                       // BimFault and its compact scope/reason/boundary axes
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

// AsprsBias is the per-ASPRS-class policy CASE, not a flag beside two options: Excluded names the classes element
// minting refuses outright (noise/overlap/water — an explicit policy exclusion the Project fold filters BEFORE
// construction, never a silent drop of an UNMAPPED class, which still faults recon-shape-miss), and Constructed
// carries the classifier-lane Domain bias beside the shape-independent Pin. A stored Constructible bool left
// "excluded yet pinned" representable — a row the classifier would have short-circuited into a landing the
// Project filter had already thrown away — and the union makes that corner unspellable. Pin short-circuits the
// (shape, domain, orientation) table for classes whose IFC landing is shape-independent (a vegetation
// trunk/canopy/hedge is IfcGeographicElement/VEGETATION whether it fitted a cylinder, a freeform, or a plane).
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

// TWO axes, two keys. CaptureLineage keys the SOURCE bytes a capture arrived as — minted once at decode over the
// raw LAS/LAZ octets and carried on the cloud and every segment cut from it. It is the ONE address a re-fetch
// joins on, so the Pset row advertising the source cloud points HERE.
[ValueObject<UInt128>]
public sealed partial class CaptureLineage {
    public static CaptureLineage Of(ReadOnlyMemory<byte> bytes) =>
        Create(ContentHash.Of(bytes, static (payload, writer) => writer.String("las-capture").Raw(payload.Span)));
}

// ReconstructionKey keys one FIT RUN — the segment bytes UNDER this run's fit parameters — so a re-run at
// identical parameters dedups its elements and a re-run at a different deflection or angle tolerance is honestly a
// different element. It is NOT a capture address: the parameters sit in its preimage, so nothing re-fetches
// through it. The two key spaces wore one value type and the published SourceCloud row cited the run key, which
// advertised a re-fetch join that could never close — each axis now carries its own name and its own Pset row.
// Both compose the kernel seed-zero XxHash128 through the seam CanonicalWriter (the ONE hasher), never the
// upper-stratum Rasm.Compute interchange owner.
[ValueObject<UInt128>]
public sealed partial class ReconstructionKey {
    public static ReconstructionKey Of(SegmentedCloud segment, ReconstructionContext context) =>
        Create(ContentAddress.Of((segment, context), context.Distance.Value, static (s, writer) => writer
            .String("recon-run").Raw(s.segment.CloudBytes.Span)
            .Double(s.context.Chord.Value).Double(s.context.Angle.Value)).Value);
}

// SizeBand carries the classifier's SCALE axis as a COLUMN on each table row rather than a fourth key axis
// multiplying every row by the bands it ignores. That (shape, domain, orientation) table answers WHAT a fit is and
// never HOW BIG, so a 6 mm-radius cylinder keyed exactly as a 600 mm one and classified Column. Each bound is the
// smallest GAUGE a building element of that class plausibly carries, so a fit under it is debris the
// reconstruction declines rather than a member it invents. Bounds are FLOORS, never ceilings — an oversized fit is
// a real large element — and a form publishing no gauge admits, because refusing an unmeasurable cross-section
// deletes the whole residual freeform family the table exists to place.
public readonly record struct SizeBand(double MinimumGauge) {
    public static readonly SizeBand Surface = new(0.25);
    public static readonly SizeBand Member = new(0.05);
    public static readonly SizeBand Fitting = new(0.01);

    // Absence and presence are DIFFERENT answers, so the read discriminates on the Option rather than defaulting
    // it to the bound: a form publishing no measurable cross-section admits (refusing it would delete the whole
    // residual freeform family the table exists to place), and a form that DID publish one is judged against the
    // floor. Folding both through IfNone(MinimumGauge) made the two indistinguishable at the comparison and read
    // as an unmeasurable form being exactly at its own floor.
    public bool Admits(Option<double> gauge) =>
        gauge.Match(Some: measured => measured >= MinimumGauge, None: static () => true);
}

// --- [MODELS] -----------------------------------------------------------------------------
// Every geometric budget rides a kernel Tolerance on its ELECTED lane, so each proves against that lane's own band
// and names the regime it belongs to: Chord is the tessellation deflection, Distance the absolute linear compare,
// Angle the angular one. VerticalCosineLimit is NOT a tolerance and takes no lane — it declares a 20-degree tilt
// band this classifier reads as a direction cosine, and seating it on an angle lane would put a
// numerical-agreement regime on a domain decision.
public readonly record struct ReconstructionContext(
    IfcDomain Discipline, Tolerance Chord, Tolerance Distance, Tolerance Angle,
    double ConfidenceFloor, double VerticalCosineLimit) {
    // Accessor-backed under the type-init proof idiom: the guarded Tolerance mint returns Fin, so a static field
    // initializer cannot carry it and a lazy first read proves every budget against its lane's band once.
    public static ReconstructionContext Building => BuildingRows.Value;

    static Tolerance Band(ToleranceLane lane, double value) =>
        Tolerance.Of(lane, value, Op.Of(nameof(ReconstructionContext))).ThrowIfFail();

    static readonly Lazy<ReconstructionContext> BuildingRows = new(static () => new(
        IfcDomain.Architecture,
        Band(ToleranceLane.Chord, 1e-3), Band(ToleranceLane.Distance, 1e-6), Band(ToleranceLane.Angle, 1e-4),
        0.6, 0.342),
        LazyThreadSafetyMode.ExecutionAndPublication);

    // VerticalCosineLimit is sin(band) — 0.342 pins a 20-degree tilt band; the parallel-side bound is the TRIG
    // complement cos(band) = sqrt(1 - limit^2) (0.940), never the arithmetic 1-limit (0.658), which slabs a
    // 45-degree-pitched roof.
    double UprightCosine => Math.Sqrt(1.0 - (VerticalCosineLimit * VerticalCosineLimit));

    // Surface orientation of a planar patch: a vertical NORMAL is a horizontal surface (slab), a horizontal normal a
    // vertical surface (wall) — distinct from a swept solid's axis orientation, whose vertical/horizontal mapping inverts.
    public FitOrientation OrientationOfNormal(Vector3 normal) {
        double vertical = Math.Abs(Vector3.Dot(normal.Unit, Vector3.UnitZ));
        return vertical >= UprightCosine      ? FitOrientation.Horizontal
            : vertical <= VerticalCosineLimit ? FitOrientation.Vertical
            : FitOrientation.Inclined;
    }

    // Axis orientation of a swept solid: a vertical AXIS is a vertical member (column), a horizontal axis a horizontal
    // member (beam) — the inverse of the surface-normal mapping, so a vertical-axis cylinder classifies Column, not Beam.
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
    byte DominantClass, int Inliers, int Total, ReadOnlyMemory<byte> CloudBytes, CaptureLineage Capture) {
    public FitConfidence Confidence => FitConfidence.Create(Total > 0 ? (double)Inliers / Total : 0.0);
    public double Residual => Total > 0 ? 1.0 - (double)Inliers / Total : 1.0;
}

// PrimitiveAnalytic answers every per-shape fact from ONE dispatch: the shape row, the analytical surface ring
// (planar arms alone), the analytical axis line (swept arms alone), the orientation DATUM paired with the mapping
// that reads it — a patch's normal and a swept solid's direction map INVERSELY, so datum and reading must travel
// together — and the GAUGE the size band judges. Gauge is a fit's characteristic CROSS-SECTION (a patch's largest
// bounding side, a swept solid's or sphere's diameter, a torus's tube diameter) because length never separates a
// 6 mm cable from a 600 mm column while cross-section always does; a form with no measurable cross-section
// publishes None.
public readonly record struct PrimitiveAnalytic(
    PrimitiveShape Shape, Seq<Vector3> Boundary, Option<AxisCurve> Axis,
    Option<Vector3> Normal, Option<Vector3> Direction, Option<double> Gauge);

// Shape-specific payload ALONE. Six arms each repeating SegmentId/GeometryHash/Confidence/lineage forced six total
// Switches to project four columns that were never per-shape, and every one of them was a chance for an arm to
// answer a shared column differently from its siblings.
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

// ONE reconstruction row: the columns EVERY fit carries — its segment, its kernel content key, its inlier
// confidence, its run key — sit here as DIRECT reads, and only the shape-specific payload rides the closed
// PrimitiveForm. The analytical surface and line are content-keyed into the Node.Object Representations map under
// "FootPrint" and "Axis" (Keys, below), NEVER inlined as a coordinate field on the seam node — a Rasm.Compute
// runner resolves either one-hop from the blob store.
public readonly record struct ReconstructionPrimitive(
    int SegmentId, UInt128 GeometryHash, FitConfidence Confidence, ReconstructionKey Key, PrimitiveForm Form) {

    public static ReconstructionPrimitive Of(SegmentedCloud s, ReconstructionContext context) =>
        new(s.SegmentId, s.GeometryHash, s.Confidence, ReconstructionKey.Of(s, context), PrimitiveForm.Of(s));

    // Every consumer reads this ONE per-shape dispatch: a new fitted form is one arm here and nowhere else.
    public PrimitiveAnalytic Analytic => Form.Switch(
        plane:    static p => new PrimitiveAnalytic(PrimitiveShape.Plane, p.Boundary, None, Some(p.Normal), None, Some(Extent(p.Boundary))),
        sphere:   static s => new PrimitiveAnalytic(PrimitiveShape.Sphere, Seq<Vector3>(), None, None, None, Some(s.Radius * 2.0)),
        cylinder: static c => new PrimitiveAnalytic(PrimitiveShape.Cylinder, Seq<Vector3>(), Some(Curve(c.AxisStart, c.AxisEnd, c.Direction)), None, Some(c.Direction), Some(c.Radius * 2.0)),
        cone:     static c => new PrimitiveAnalytic(PrimitiveShape.Cone, Seq<Vector3>(), Some(Curve(c.AxisStart, c.AxisEnd, c.Direction)), None, Some(c.Direction), Some(c.Radius * 2.0)),
        torus:    static t => new PrimitiveAnalytic(PrimitiveShape.Torus, Seq<Vector3>(), Some(Curve(t.AxisStart, t.AxisEnd, t.Direction)), None, Some(t.Direction), Some(t.MinorRadius * 2.0)),
        freeform: static _ => new PrimitiveAnalytic(PrimitiveShape.Freeform, Seq<Vector3>(), None, None, None, None));

    static AxisCurve Curve(Vector3 start, Vector3 end, Vector3 axis) =>
        new(start, end, Math.Abs(Vector3.Dot(axis.Unit, Vector3.UnitZ)) > 0.9 ? Vector3.UnitX : Vector3.UnitZ);

    // Extent answers a patch's dominant dimension: the largest side of its axis-aligned bound, one fold.
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

    // Keyed geometry map: the fitted-solid display geometry rides "Body" (the kernel GeometryHash), the analytical
    // surface "FootPrint" and the analytical line "Axis" — EACH a kernel seed-zero digest over the CanonicalWriter
    // projection of its Vector3 coordinates, minted through the seam's ONE tolerance-bound entry so the model grid
    // rides the key the kernel's own ZeroTolerance-pinned leg cannot carry. An empty boundary / a None axis
    // contributes no key, so a non-planar/non-swept form carries only "Body".
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

// --- [OPERATIONS] -------------------------------------------------------------------------
// Reconstruction owns its bag row names here. Three consumers spell them — this fold authors the bag, the
// Planning/progress review read selects on them, and a federation diff joins on them — so every row is ONE
// PropertyName static minted through the owner-blessed empty-prefix PropertyCategory.Seam.Row (the
// Properties/property#DETAIL_SCHEMA custody law) rather than a literal re-typed at each read, where a rename at
// one end reads downstream as an absent row rather than as a break.
public static class ReconstructionRows {
    public const string Set = "Pset_Reconstruction";
    public static readonly PropertyName FitConfidence = PropertyCategory.Seam.Row("FitConfidence");
    public static readonly PropertyName Residual = PropertyCategory.Seam.Row("Residual");
    public static readonly PropertyName Inliers = PropertyCategory.Seam.Row("Inliers");
    public static readonly PropertyName Total = PropertyCategory.Seam.Row("Total");
    public static readonly PropertyName AsprsClass = PropertyCategory.Seam.Row("AsprsClass");
    public static readonly PropertyName NeedsReview = PropertyCategory.Seam.Row("NeedsReview");
    public static readonly PropertyName PrimitiveShape = PropertyCategory.Seam.Row("PrimitiveShape");
    public static readonly PropertyName SourceSegment = PropertyCategory.Seam.Row("SourceSegment");
    public static readonly PropertyName SourceCloud = PropertyCategory.Seam.Row("SourceCloud");
    public static readonly PropertyName ReconstructionRun = PropertyCategory.Seam.Row("ReconstructionRun");
}

public static class ElementClassifier {
    // Frozen (shape, domain?, orientation) -> (IfcClass, predefined, band) projection — a data table, never
    // enumerated switch arms. DOMAIN is an OPTIONAL refinement exactly as the geospatial classifier's geometry-kind
    // axis is: a `None` row is the FALLBACK lane every segment reaches, and a `Some(domain)` row wins the ladder
    // where the discipline genuinely discriminates — a pavement plane, a geotechnical stratum, a cable tray, a
    // structural pile. Making the axis mandatory forced the ordinary building lanes to be re-declared under an
    // Architecture key they never discriminated on, so a segment biased into any other domain missed a wall, a
    // slab, a column, and a beam that were never domain-specific at all. Predefined tokens are members of the
    // Model/elements#IFC_CLASS valid sets, admitted at the egress gate, and the shape-independent site classes ride the
    // BiasOf Pin tier rather than table rows. The Plumbing lane is discipline-reached (no ASPRS class biases into
    // it) and swept-family ONLY, because there the shape itself is the pipe evidence — a cylinder is a rigid
    // straight run, a torus arc an elbow, a cone a diameter transition, a fitting-scale sphere a fitting body
    // whose leaf token no shape picks — while a plumbing-disciplined plane or freeform rides the fallback lanes: a
    // wall near pipework is still a wall, and a blob is still the proxy, never a sanitary fixture the fit cannot
    // discriminate. A row whose newer-schema class (the Ifc4 plumbing occurrences, the Ifc4x3 infrastructure/
    // geotechnical rows) the older target schema cannot carry faults class-out-of-schema at AdmitPredefined
    // (not recon-shape-miss).
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

    // Two tiers, two gates: the BiasOf Pin short-circuits the table for shape-independent classes (vegetation,
    // rail), else the effective domain (the bias, else the context discipline) keys the frozen table with the
    // orientation fallback to Any, the row's SizeBand then judging the fit's gauge. Orientation reads whichever
    // datum the form publishes — a patch's normal or a swept solid's direction, the two mappings inverse — so no
    // consumer picks the reading. A sub-band fit is a MISS at that class and faults exactly as an unmapped shape
    // does: dropping it silently would leave a half-model, and classifying it would mint a member out of debris.
    // EVERY surviving landing admits through the one 4-arg egress hop.
    public static Fin<(IfcClass Class, PredefinedType Predefined)> Classify(
        ReconstructionPrimitive primitive, SegmentedCloud segment, ReconstructionContext context, ReleaseVersion schema, Op key) =>
        ReconstructionContext.BiasOf(segment.DominantClass).Switch(
            state: (primitive.Analytic, context, schema, key),
            // Project filters an excluded class BEFORE authoring, so this arm states that law under the compiler
            // rather than leaving the case to a default that would classify a noise segment as a real element.
            excluded: static (s, _) => Fin.Fail<(IfcClass Class, PredefinedType Predefined)>(
                new BimFault.Refused(s.key, BimScope.Reconstruct, BimReason.Capability, string.Join(':', new object?[] { "recon-unregistered", s.Analytic.Shape.Key, "asprs-excluded" }))),
            constructed: static (s, row) => row.Pin.Match(
                Some: pin => Admit(pin.Class, pin.Predefined, s.schema, s.key),
                None: () => Tabled(s.Analytic, row.Domain.IfNone(s.context.Discipline), s.context, s.schema, s.key)));

    // Four rungs, most specific first: the discipline row at this orientation, the discipline row at any
    // orientation, the fallback row at this orientation, the fallback row at any orientation.
    static Fin<(IfcClass Class, PredefinedType Predefined)> Tabled(
        PrimitiveAnalytic analytic, IfcDomain domain, ReconstructionContext context, ReleaseVersion schema, Op key) {
        FitOrientation orientation = (analytic.Normal.Map(normal => context.OrientationOfNormal(normal))
                | analytic.Direction.Map(direction => context.OrientationOfAxis(direction)))
            .IfNone(FitOrientation.Any);
        return (Table.Find((analytic.Shape, Some(domain), orientation))
                | Table.Find((analytic.Shape, Some(domain), FitOrientation.Any))
                | Table.Find((analytic.Shape, Option<IfcDomain>.None, orientation))
                | Table.Find((analytic.Shape, Option<IfcDomain>.None, FitOrientation.Any)))
            .ToFin(new BimFault.Refused(key, BimScope.Reconstruct, BimReason.Unmapped, string.Join(':', new object?[] { "recon-shape-miss", analytic.Shape.Key, domain.ToString(), orientation.ToString() })))
            .Bind(row => row.Band.Admits(analytic.Gauge)
                ? Admit(row.Class, row.Predefined, schema, key)
                : Fin.Fail<(IfcClass, PredefinedType)>(new BimFault.Refused(key, BimScope.Reconstruct, BimReason.Unmapped, string.Join(':', new object?[] { "recon-below-band", analytic.Shape.Key, row.Class.Key, analytic.Gauge.IfNone(0.0).ToString(CultureInfo.InvariantCulture) }))));
    }

    // One egress-gate hop — the frozen Model/elements#IFC_CLASS per-token span gate
    // AdmitPredefined(token, objectType, schema, key) -> Fin<string>; pin tier and table tier both admit through it.
    static Fin<(IfcClass Class, PredefinedType Predefined)> Admit(IfcClass @class, string predefined, ReleaseVersion schema, Op key) =>
        @class.AdmitPredefined(predefined, "", schema, key).Map(token => (@class, PredefinedType.Create(token)));
}

// --- [SERVICES] ---------------------------------------------------------------------------
// Scan-source PRIMARY projector: the kernel-segmented clouds are captured internally (the IElementProjection contract
// holds only Node/Relationship/GraphDelta), and Project mints the neutral rooted identity while recording a deterministic
// IFC GlobalId as the node ExternalId so a re-run dedups. The seam Assemble fold composes the GraphDelta.
public sealed class ReconstructionProjector(Seq<SegmentedCloud> segments, ReconstructionContext context) : IElementProjection {
    // AsprsBias.Excluded classes (noise 7/18, overlap 12, water 9) are refused by the explicit BiasOf policy case
    // BEFORE authoring — a known-noise segment mints no phantom element; an UNMAPPED shape still faults loud.
    public Fin<GraphDelta> Project(ProjectionContext ctx) =>
        segments.Filter(static s => ReconstructionContext.BiasOf(s.DominantClass) is AsprsBias.Constructed)
            .Fold(
                Fin.Succ(GraphDelta.Empty.Reheader(ctx.Header)),
                (acc, segment) => acc.Bind(delta => Author(segment, ctx).Map(delta.Merge)));

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

    // Typed Pset_Reconstruction bag NODE: the fit evidence as PropertyValue/MeasureValue, never the retired stringly
    // PropertyBinding; EvidenceGrade.Derived because the rows are computed fit evidence (the seam ValueBag 4-column
    // shape — SetName/Values/Inheritance/Source). AsprsClass records the modal class the BiasOf policy keyed on, the
    // classification provenance a review reads. Non-rooted id is the kernel content hash over the bag's canonical
    // bytes (the id is EXCLUDED from ToCanonicalBytes, so the empty-probe id is overwritten) so an identical bag dedups.
    // Two lineage axes ride their OWN rows: SourceCloud is the CAPTURE key a re-fetch resolves the exact LAS/LAZ
    // through, ReconstructionRun the run key identifying this fit under this run's parameters — publishing the run
    // key as the source address advertised a join no store answers.
    // Five fit-evidence mints ride the seam OfSi finite gate first-fault — a NaN residual rails, never hashes.
    Fin<Node.PropertySet> ReconstructionPset(ReconstructionPrimitive primitive, SegmentedCloud segment, double tolerance, Op key) =>
        from confidence in MeasureValue.OfSi(Dimension.Dimensionless, primitive.Confidence.Value, key)
        from residual in MeasureValue.OfSi(Dimension.Dimensionless, segment.Residual, key)
        from inliers in MeasureValue.OfSi(Dimension.Dimensionless, segment.Inliers, key)
        from total in MeasureValue.OfSi(Dimension.Dimensionless, segment.Total, key)
        from asprs in MeasureValue.OfSi(Dimension.Dimensionless, segment.DominantClass, key)
        let bag = new PropertyBag(ReconstructionRows.Set, Map<PropertyName, PropertyValue>(
            (ReconstructionRows.FitConfidence,  new PropertyValue.Measure(confidence)),
            (ReconstructionRows.Residual,       new PropertyValue.Measure(residual)),
            (ReconstructionRows.Inliers,        new PropertyValue.Measure(inliers)),
            (ReconstructionRows.Total,          new PropertyValue.Measure(total)),
            (ReconstructionRows.AsprsClass,     new PropertyValue.Measure(asprs)),
            (ReconstructionRows.NeedsReview,    new PropertyValue.Boolean(primitive.Confidence.IsBelow(context.ConfidenceFloor))),
            (ReconstructionRows.PrimitiveShape, new PropertyValue.Enumerated(Seq(primitive.Analytic.Shape.Key), PrimitiveShape.Items.AsIterable().Map(static s => s.Key).ToSeq())),
            (ReconstructionRows.SourceSegment,  new PropertyValue.Text(segment.SegmentId.ToString(CultureInfo.InvariantCulture))),
            (ReconstructionRows.SourceCloud,    new PropertyValue.Text(segment.Capture.Value.ToString("X32", CultureInfo.InvariantCulture))),
            (ReconstructionRows.ReconstructionRun, new PropertyValue.Text(primitive.Key.Value.ToString("X32", CultureInfo.InvariantCulture)))),
            InheritanceMode.OccurrenceWins, EvidenceGrade.Derived)
        let probe = new Node.PropertySet(NodeId.Of(new NodeSeed.Placement()), bag)
        select probe with { Id = NodeId.Of(new NodeSeed.Content(probe, tolerance)) };
}
```

## [03]-[LAS_INGEST]

- Owner: `LasCloud` the decoded point carrier — position set (each `Position` a `MathNet.Numerics.LinearAlgebra.Vector<double>` the kernel registration and Compute dense-LA substrate consume without a re-wrap), the per-point ASPRS `Classifications` the segmentation reduces to the `[02]-[RECONSTRUCTION]` `SegmentedCloud.DominantClass` hint, the unit-normalized `Colors` lane a colour-bearing point format carries, and the header receipt facts (`ClassHistogram`, `CountsByReturn`, extrema, integer-grid `Scale`/`Offset`, `PointFormat`, CRS WKT, `CaptureLineage`, count, `Instant`); `CloudLevel` one decimated detail band over that carrier — retained indices, measured point count, meshopt cull sphere, per-level content key; `LasCompression` the `[SmartEnum<string>]` discriminant; `LasIngest` the dual-engine decode fold decoding raw `.las`/`.laz` bytes into the `LasCloud` the kernel registration/segmentation consume AND drawing that carrier's progressive-detail pyramid — `Themis.Las` owns the uncompressed codec, `Unofficial.laszip.netstandard` the compressed codec, `Alimer.Bindings.MeshOptimizer` the point decimation and the sphere bound, the kernel owns the fit; this owner re-mints none.
- Entry: `LasIngest.Decode(ReadOnlyMemory<byte> bytes, Instant at, Op key)` dispatches on `LasCompression.Sniff` (the offset-104 public-header byte whose high bit marks LASzip compression), routing the uncompressed leg through `ReadLas` and the compressed leg through `ReadLaz`; `LasIngest.Pyramid(LasCloud cloud, InterchangePolicy policy, Op key)` draws the detail bands over the `format#FORMAT_AXIS`-neighbouring `export#EXPORT_RAIL` `InterchangePolicy.LodRatios` schedule the mesh pyramid reads, weighting the draw by the `AttributeWeights` `base_color` row so a facade capture keeps its material boundaries; `Fin<T>` traps a malformed header, an unreadable archive, or a degenerate decimation into `Model/faults#FAULT_BAND` their original captured `Error` through `Op.Catch`, and a capture whose point count exceeds the int-domain decimator into `BimFault.Refused` with `BimReason.Capability` before any narrowing, the `Op`-keyed case IS the `Error`, never a `.ToError()` hop.
- Auto: `Sniff` selects the engine from the compression marker WITHOUT a full open; `ReadLas` streams the `Themis.Las` `LasReader` over one temp path (byte admission is path-bound — the one shipped `AsyncStreamHandler` is path-constructed), and `ReadLaz` folds the `laszip` decoder over the in-memory stream gating each non-zero C-API status through `Check`; both mask the classification format-correctly (formats 0-5 strip the flag bits `& 0x1F`, formats 6-10 keep the full class byte), fill the `Colors` lane on the colour-bearing formats alone (2/3/5/7/8/10, the 16-bit channel unit-normalized; every other format keeps the typed EMPTY lane rather than a black cloud a colour-weighted draw reads as uniform), read the header receipt facts and the record-`2112` OGC WKT CRS, and assemble one `LasCloud` whose lineage is the kernel `XxHash128` over the raw bytes through the seam `CanonicalWriter` and whose `ClassHistogram` folds in one dense-array pass; `Pyramid` stages the float32 position lane ONCE (meshopt is a float kernel, the carrier a MathNet double the registration consumes) and folds each ratio through `Meshopt.SimplifyPoints`, keeping the RETAINED source indices rather than a copied point set and computing each band's `Meshopt.ComputeSphereBounds` cull sphere over the retained points alone, each level content-keyed off the cloud lineage so the tile pyramid addresses a capture's bands exactly as it addresses a mesh's; the per-point ASPRS classes feed the kernel segmentation reducing them to the per-segment modal `DominantClass`, and the CRS WKT feeds the app's `Header.Reference` `GeoReference` (`Semantics/georeference#GEO_PROJECTION` `ProjNET` leg) so a georeferenced capture lands in the canonical kernel frame.
- Receipt: `LasCloud` is the decoded scan evidence — point/per-return counts, the `ClassHistogram` computed from the decoded class bytes (evidence the header cannot forge), header extrema and quantization, point-data-record format, colour-lane occupancy, and CRS WKT presence; the `CaptureLineage` over the source bytes joins the reconstructed model back to its capture — the `Pset_Reconstruction` `SourceCloud` row publishes THAT key, so the `Review/diff#MODEL_DIFF` federation diff and reality-capture playback re-fetch the exact LAS/LAZ through a join that closes, where the parameter-derived run key opens one nothing answers. `CloudLevel` is the per-band draw evidence — the MEASURED retained count (never the requested target, which the decimator may undershoot on a sparse capture), the cull sphere a client selects on, and the content key it streams by.
- Packages: `Themis.Las` (the MIT pure-managed uncompressed ASPRS LAS reader over `MathNet.Numerics`), `Unofficial.laszip.netstandard` (the LGPL-2.1 separate-assembly pure-managed LASzip codec — `.laz` arithmetic decode, selective-channel decompression, the `.lax` spatial-index bbox query), `Alimer.Bindings.MeshOptimizer` (the colour-weighted point decimation and the sphere bound), `Rasm` (the kernel `Domain.ContentHash` and the `Rasm/Domain/identity#CONTENT_KEY` `CanonicalWriter`), Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime.
- Growth: a new ASPRS point data record format is one `Themis.Las` `PointTypeMap` row (formats 0-10 share one reader, never a per-format reader family); a compression state is one `LasCompression` row dispatched by `Sniff`; a per-point facet (intensity, return index, GPS time, NIR) is one column the `LasPoint` facet set already carries, the `LasCloud.PointFormat` receipt column announcing which facets a capture holds without a re-decode; a new detail band is one ratio on the shared `InterchangePolicy.LodRatios` column and a new decimation weight one `AttributeWeights` row, both the same policy the mesh pyramid reads — never a cloud-local schedule; a tiled ingest enters through the `laszip` `.lax` `inside_rectangle` windowed path when an index exists; never a re-minted point-cloud decoder, never a hand-rolled point decimator, and never a second hashing scheme over the LAS/LAZ bytes.
- Boundary: `Themis.Las` (`LasReader`/`LasPoint`/`ILasHeader`/`LasVariableLengthRecord`) owns the uncompressed stream and the `laszip` C-API codec the compressed stream, `LasPoint.Position`/`get_coordinates` lifting into the one `MathNet.Numerics.LinearAlgebra.Vector<double>` the kernel registration consumes with no re-wrap, never a hand-rolled LAS/LAZ reader; the LGPL-2.1 `Unofficial.laszip.netstandard` is referenced as a SEPARATE assembly, never ILMerged, so the in-Rhino plugin ALC firebreak holds; `LasIngest` decodes and DRAWS and never fits, registration and segmentation staying the kernel's by reference — `Meshopt.SimplifyPoints` owns the colour-weighted decimation and `Meshopt.ComputeSphereBounds` the cull sphere, so a hand-rolled voxel thin, an octree decimator, or a hand-computed bounding sphere beside them is the deleted form, and a level carrying no bound is a residency band with no selection criterion; the CRS WKT VLR feeds the app's `GeoReference` (`Semantics/georeference#GEO_PROJECTION` `ProjNET` leg), never a codec-local reprojection; the source-cloud content key composes the kernel `Rasm.Domain.ContentHash` seed-zero `XxHash128`, never a second hasher or the upper-stratum `Rasm.Compute` interchange owner; the decoded `LasPoint`/`laszip_point` types never leak past this fold — internal code holds the canonical `LasCloud`/`SegmentedCloud` per the boundary-mapping law.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
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

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LasCompression {
    public static readonly LasCompression Uncompressed = new("las");
    public static readonly LasCompression Compressed   = new("laz");

    // ASPRS public-header point-data-record-format byte sits at offset 104; LASzip marks compression by setting its
    // high bit. The sniff selects the decode engine without a full open — Themis (MIT, MathNet-native) for the uncompressed
    // leg, the LGPL laszip arithmetic decoder for the compressed leg.
    public static LasCompression Sniff(ReadOnlySpan<byte> bytes) =>
        bytes.Length > 104 && (bytes[104] & 0x80) != 0 ? Compressed : Uncompressed;
}

// Bbox query rectangle a windowed ingest reads inside — plan coordinates alone: the `.lax` index the compressed
// leg exploits is a 2D spatial index, so a vertical bound is a filter the index cannot serve and the two engines
// diverge on what the same window means.
public readonly record struct CloudWindow(double MinX, double MinY, double MaxX, double MaxY) {
    public bool Holds(double x, double y) => x >= MinX && x <= MaxX && y >= MinY && y <= MaxY;
}

// --- [MODELS] -----------------------------------------------------------------------------
// Decoded scan carrier with its receipt facts: the ASPRS classification histogram (folded from the decoded class
// bytes — evidence the header cannot forge), the header point-count-by-return set, header extrema and the integer-grid
// scale/offset (the quantization the capture was recorded at, the registration tolerance floor), and the point-data-
// record format (which per-point facets — intensity/GPS-time/RGB/NIR — the capture carries, readable without re-
// decoding; formats 6-10 mark the LAS 1.4 extended-class captures).
// Colors is the unit-normalized interleaved RGB lane, EMPTY exactly where the point-data-record format declares no
// colour channel (formats 0/1/4/6/9) — the typed absence the decimator reads, never a zero-filled black cloud a
// colour-weighted draw would treat as uniform and thin through every material boundary.
public sealed record LasCloud(
    ReadOnlyMemory<Vector<double>> Positions, ReadOnlyMemory<byte> Classifications, ReadOnlyMemory<float> Colors,
    Map<byte, ulong> ClassHistogram, ReadOnlyMemory<ulong> CountsByReturn,
    Vector3 Min, Vector3 Max, Vector3 Scale, Vector3 Offset,
    Option<string> CrsWkt, CaptureLineage Lineage, byte PointFormat, ulong PointCount, Instant At);

// One decimated level of a decoded cloud: the RETAINED point indices into the source carrier (never a copied point
// set — the decoded positions stay the one carrier the kernel registration already consumes), the ratio it was drawn
// at, the measured retained count, the meshopt sphere bound a streaming client culls the level on, and the level's
// own content key so the tile pyramid content-addresses each detail band exactly as the mesh pyramid's levels do.
public sealed record CloudLevel(
    int Level, double TargetRatio, int PointCount, ReadOnlyMemory<uint> Indices, Bounds Bounds, UInt128 ContentKey);

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class LasIngest {
    // Decimation weight reads this canonical colour channel off the shared attribute roster — the SAME roster
    // export#BIM_LOD reads its normal and uv weights from, so one policy governs mesh and cloud draws alike.
    const string ColorChannel = "base_color";

    // ASPRS point-data-record formats 2/3/5/7/8/10 carry the RGB channel; every other format has none.
    static bool Colored(byte pointFormat) => pointFormat is 2 or 3 or 5 or 7 or 8 or 10;

    // Optional WINDOW is the bbox push-down: with a built `.lax` index the compressed leg decodes only the
    // points inside the rectangle, and the uncompressed leg — which has no index to exploit — applies the same
    // rectangle as it reads, so ONE window argument means one thing on both engines and a caller never has to know
    // which one served it. This is the FGB Packed-R-tree filter posture the vector ingest already holds, projected
    // onto the point cloud, and it closes the declared tiled-ingest growth row rather than opening a second entry.
    public static Fin<LasCloud> Decode(ReadOnlyMemory<byte> bytes, Instant at, Op key, Option<CloudWindow> window = default) =>
        Decoded(LasCompression.Sniff(bytes.Span), bytes, window, at, key);

    static Fin<LasCloud> Decoded(LasCompression codec, ReadOnlyMemory<byte> bytes, Option<CloudWindow> window, Instant at, Op key) =>
        codec == LasCompression.Compressed
            ? Trap(codec, key, () => ReadLaz(bytes, window, at))
            : Trap(codec, key, () => ReadLas(bytes, window, at, key)).Bind(static read => read);

    // Codec legs are SUBJECTS on the one roster row, so both engines grep under one fixed prefix.
    static Fin<T> Trap<T>(LasCompression codec, Op key, Func<T> read) =>
        key.Catch(read);

    // Pyramid draws the cloud's detail bands — point-set twin of the export#BIM_LOD mesh pyramid over the SAME
    // InterchangePolicy.LodRatios schedule, so a streaming budget tuned for meshes tunes captures with no second
    // ratio column and a served scan and a served model band identically.
    // meshopt's count and index surface is int-domain, so a capture beyond int.MaxValue points cannot be drawn in
    // one pass and FAULTS at the boundary — a bare (int) narrowing turned a 3-billion-point capture into a silent
    // 2^31 prefix that rendered a fraction of the scan as if it were the whole, and every level below it inherited
    // that lie. Gating here proves the narrowing inside Levels rather than assuming it.
    public static Fin<Seq<CloudLevel>> Pyramid(LasCloud cloud, InterchangePolicy policy, Op key) =>
        cloud.PointCount > int.MaxValue
            ? Fin.Fail<Seq<CloudLevel>>(new BimFault.Refused(key, BimScope.Reconstruct, BimReason.Capability, string.Join(':', new object?[] { "cloud-extent", cloud.PointCount.ToString(CultureInfo.InvariantCulture) })))
            : key.Catch(() => Levels(cloud, policy, (int)cloud.PointCount));

    // meshopt is a float32 kernel and the decoded carrier holds the MathNet double vector the kernel registration
    // consumes, so the flat lane stages ONCE here and every level reads it — a second float copy parked on LasCloud
    // would double the largest allocation in the ingest for a lane only the draw reads.
    // Colour weighting is the whole discriminant between SimplifyPoints and a bare spatial thin: a facade capture
    // keeps its material boundaries because a colour discontinuity costs what it costs to cross. Weighting reads that
    // shared attribute roster by canonical channel, so a capture with no colour lane AND a roster with no row
    // both fall to an unweighted spatial draw — the same "a channel with no row is unweighted" law the mesh
    // pyramid's attribute lanes hold.
    // Staged lane, colour partner, and point count travel as ONE carrier, so a colour span whose length disagrees
    // with the position count is unrepresentable rather than read as a shorter cloud.
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

    // SimplifyPoints returns the RETAINED source indices, so a level costs one index buffer rather than a second
    // point set. The cull sphere is computed over the retained points ALONE — a bound taken over the source set
    // describes geometry the level does not draw, so a client culling on it keeps a band that renders nothing.
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

    // laszip C-API signals failure by a NON-ZERO int status (get_error carries the message), never an exception, so
    // every status is gated here and a non-zero lifts through the Trap's Op.Catch while retaining the IOException;
    // a raw status code never branches domain logic and a malformed LAZ never reads garbage past a failed open/read.
    // The Themis uncompressed leg needs no analog: its managed reader throws into the same capture boundary.
    // NAMED EXEMPTION: this is the ONE P/Invoke status-to-rail adapter on the page and it stays an adapter. Railing it
    // would thread Fin through roughly a dozen sequential C-API calls inside a bracket-and-loop body whose release
    // order the rail cannot express, and every one of those statuses is the LIBRARY's answer rather than a domain
    // comparison — which is exactly what separates it from the truncation guard above, now railed.
    static void Check(laszip codec, int status) {
        if (status != 0) { throw new IOException(codec.get_error()); }
    }

    // Themis uncompressed leg: ONE caller-owned LasPoint filled by the no-alloc GetNextPoint(ref) overload — the
    // per-point-allocating GetNextPoint() is the deleted form. LasPoint.Update MUTATES the one Position vector IN
    // PLACE (Position[0..2] scale+offset writes on the same instance), so each collected position detaches via
    // Clone() — one fresh MathNet vector per point; storing point.Position bare aliases every slot onto the last
    // decoded point. NAMED EXEMPTION for that per-point mint: the SEAM carrier is Vector<double> (the kernel
    // registration and the Compute dense-LA substrate consume it unwrapped), so a TensorPrimitives span fold would
    // have to re-wrap every triple back into a Vector<double> at the seam — the same N allocations plus a copy —
    // and there is no arithmetic here to vectorize, only a detaching copy the reader's mutate-in-place contract
    // forces. The exemption retires the day the seam carrier becomes a lane arena. The byte admission span-writes one temp path (no whole-buffer ToArray copy) because the
    // package's one shipped IStreamHandler (AsyncStreamHandler) is path-ctor'd; LasReader(IStreamHandler) is the
    // stream growth seam. The ILasHeader carries the receipt facts: extrema, scale/offset, counts by return, point
    // format. Truncation is a DOMAIN guard, not a package raise: the archive body being shorter than the header
    // count is this rail's own comparison, so it rails a named row carrying both counts instead of the
    // string-encoded IOException the Trap funnel could only re-emit as an opaque message.
    static Fin<LasCloud> ReadLas(ReadOnlyMemory<byte> bytes, Option<CloudWindow> window, Instant at, Op key) {
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
            // Colour materializes only on a colour-bearing format; ASPRS RGB is 16-bit, so unit normalization
            // divides by 65535 and a capture with no channel keeps the typed EMPTY lane.
            bool colored = Colored(reader.Header.PointDataFormat);
            float[] colors = colored ? new float[reader.PointCount * 3] : [];
            LasPoint point = new();
            // Themis ships no spatial index, so the window applies as a plan filter over the points already being
            // read — the same rectangle the compressed leg pushes into its `.lax` index, so a caller's window means
            // one thing on both engines and only the COST differs. `scanned` counts the archive, `read` the landed
            // set, so the truncation guard still compares against the header while the receipt reports what landed.
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
                ? Fin.Fail<LasCloud>(new BimFault.Refused(key, BimScope.Reconstruct, BimReason.Rejected, string.Join(':', new object?[] { "cloud-truncated", scanned.ToString(CultureInfo.InvariantCulture), reader.PointCount.ToString(CultureInfo.InvariantCulture) })))
                : Fin.Succ(Assemble(bytes, positions, classes, colors, crs, read, h.NumPointRecordsByReturn,
                    new Vector3(h.MinX, h.MinY, h.MinZ), new Vector3(h.MaxX, h.MaxY, h.MaxZ),
                    new Vector3(h.ScaleX, h.ScaleY, h.ScaleZ), new Vector3(h.OriginX, h.OriginY, h.OriginZ),
                    h.PointDataFormat, at));
        } finally { File.Delete(path); }
    }

    // laszip compressed leg: decompress_selective masks the decode to position+classification+RGB so the arithmetic
    // decoder skips the waveform and extra-byte channels no fit or draw reads while keeping the colour lane the
    // decimation weighs; get_coordinates lifts the raw XYZ into the same MathNet vector; the
    // LAS 1.4 extended counts supersede the legacy uint counts when present. The class channel is FORMAT-CORRECT: a
    // format-6-10 record (extended_point_type set) carries its full class byte in extended_classification — the legacy
    // classification field is a 5-bit mask (& 0x1F) that truncates extended records to garbage. An array-backed memory
    // opens zero-copy; only a non-array-backed source pays the ToArray.
    static LasCloud ReadLaz(ReadOnlyMemory<byte> bytes, Option<CloudWindow> window, Instant at) {
        using MemoryStream stream = MemoryMarshal.TryGetArray(bytes, out ArraySegment<byte> segment)
            ? new(segment.Array!, segment.Offset, segment.Count, writable: false)
            : new(bytes.ToArray(), writable: false);
        // Release brackets ACQUISITION: the create and the open are the acquisition, so both Checks run OUTSIDE the
        // try and close_reader releases only a reader that actually opened — an open inside the try runs
        // close_reader against a never-acquired reader, a native call on an uninitialised codec.
        laszip codec = laszip.create();
        Check(codec, codec.decompress_selective(LASZIP_DECOMPRESS_SELECTIVE.CHANNEL_RETURNS_XY | LASZIP_DECOMPRESS_SELECTIVE.Z | LASZIP_DECOMPRESS_SELECTIVE.CLASSIFICATION | LASZIP_DECOMPRESS_SELECTIVE.RGB));
        Check(codec, codec.open_reader_stream(stream, out _, leaveOpen: true));
        try {
            Check(codec, codec.get_number_of_point(out long count));
            // Windowed read: only where the archive carries a built `.lax` index AND the rectangle is non-empty
            // does the decoder switch to read_inside_point, which skips whole chunks the index rules out. An
            // unindexed archive falls to the full read and the window applies as the plan filter below, so the
            // caller's window means one thing either way.
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
                    ushort[] rgb = codec.point.rgb;   // the four-slot R/G/B/NIR channel array; the NIR slot rides format 8/10 alone
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
                : Array.ConvertAll(h.number_of_points_by_return, static c => (ulong)c);
            // Receipts count the points this decode LANDED, never the header's total: a windowed read reporting the
            // header count describes a cloud the carrier does not hold, and every ratio the pyramid then draws is
            // a fraction of a number that is not there.
            return Assemble(bytes, positions, classes, colors, crs, (ulong)read, byReturn,
                new Vector3(h.min_x, h.min_y, h.min_z), new Vector3(h.max_x, h.max_y, h.max_z),
                new Vector3(h.x_scale_factor, h.y_scale_factor, h.z_scale_factor),
                new Vector3(h.x_offset, h.y_offset, h.z_offset),
                (byte)(h.point_data_format & 0x7F), at);   // the LASzip high bit masked off the stored format
        } finally { codec.close_reader(); }
    }

    // `.lax` exploitation runs in the package's own order: prove an index exists, set the rectangle, then arm the
    // exploitation. An absent index or an empty rectangle answers false and the caller reads the whole archive —
    // a windowed read armed over no index decodes nothing and reports success.
    static bool Windowed(laszip codec, CloudWindow window) {
        Check(codec, codec.has_spatial_index(out bool indexed, out _));
        if (!indexed) { return false; }
        Check(codec, codec.inside_rectangle(window.MinX, window.MinY, window.MaxX, window.MaxY, out bool empty));
        if (empty) { return false; }
        Check(codec, codec.exploit_spatial_index(true));
        return true;
    }

    // One LasCloud assembler shared by both legs: the capture key minted over the raw bytes through its own owner,
    // with the ASPRS classification histogram folded in ONE dense-array pass over the decoded class bytes.
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
