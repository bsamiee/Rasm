# [BIM_RECONSTRUCTION]

The scan-to-BIM primitive-fitting owner: one `ReconstructionPrimitive` `[Union]` (plane/cylinder/torus/freeform) folding a kernel-registered segmented point cloud into `Model/elements#ELEMENT_MODEL` `BimElement` rows, each row classified through an `ElementClassifier` frozen primitive-shape-to-`IfcClass` projection (a data table, not enumerated `switch` arms), carrying its per-element fit confidence as a `Semantics/properties#PROPERTY_SETS` `Pset_Reconstruction` row and a `ReconstructionLineage` `[ValueObject]` source-cloud content-key joining the reconstructed element back to the capture that produced it. Reconstruction is BIM-semantics-only: the registration is the kernel's — the `csharp:Rasm/Vectors#ALIGN` cloud-ICP alignment and the `csharp:Rasm/Geometry/spatial#SEGMENTATION` plane/cylinder segmentation feed an already-fitted `csharp:ROBUST_ARRANGEMENT_SUBSTRATE` exact-arithmetic arrangement consumed by reference for the planar/cylindrical primitive boundaries, never a parallel scan engine re-minted here — and the splat/point payload plus the source-cloud content key are the `csharp:Compute/Runtime/codecs#CONTENT_ADDRESSING` `InterchangeIdentity` owner's, consumed as settled vocabulary. The fitted primitive is HOST-NEUTRAL: it carries the kernel `Rasm` geometry by `GeometryHandle` reference (or the `Model/elements#BIM_TYPE` `IfcRepresentationMap` instanced-geometry key when the fit reuses a type-library shape) and never a RhinoCommon `Brep`/`Mesh`. The fold composes the one `BimModel` the `Model/elements#ELEMENT_MODEL` `Project` produces and the `Model/query#ELEMENT_SET` query algebra reads — a reconstructed model is a `BimModel` like any imported model, the `Model/elements#ELEMENT_MODEL` `BimElement` vocabulary widened by no new column and the `Review/diff#MODEL_DIFF` federation diff joining a reconstructed element to its design counterpart by GlobalId plus content-key. The page is HOST-NEUTRAL.

## [1]-[INDEX]

- [1]-[RECONSTRUCTION]: `ReconstructionPrimitive` `[Union]` (Plane/Cylinder/Torus/Freeform), the `PrimitiveShape` `[SmartEnum]` discriminant, the `ElementClassifier` frozen shape-to-`IfcClass` table, the `ReconstructionLineage` `[ValueObject]` source-cloud key, and the `Reconstruct` fold from a kernel-registered `SegmentedCloud` into `BimElement` rows.

## [2]-[RECONSTRUCTION]

- Owner: `ReconstructionPrimitive` the closed `[Union]` of fitted scan primitives — `Plane` (a bounded planar patch carrying its frame, in-plane extent, and inlier count), `Cylinder` (an axis/radius/height swept solid), `Torus` (a major/minor-radius revolved solid), and `Freeform` (a residual cloud no analytic primitive fit, carrying its content-keyed mesh handle) — each arm carrying its kernel `Rasm` geometry handle, its inlier/residual fit evidence, and the `SegmentId` of the segment it was fitted to; `PrimitiveShape` the `[SmartEnum<string>]` shape discriminant the `ElementClassifier` table keys on; `ReconstructionLineage` the `[ValueObject<UInt128>]` source-cloud content-key joining the reconstructed element to its capture; `FitConfidence` the `[ValueObject<double>]` normalized inlier-ratio band admitted into `[0,1]`; `SegmentedCloud` the kernel-registered segment carrier (segment id, fitted shape, inlier/total counts, the `csharp:ROBUST_ARRANGEMENT_SUBSTRATE` arrangement face index the planar/cylindrical boundary reads) the `csharp:Rasm/Geometry/spatial#SEGMENTATION` produces; `Reconstruction` the model-level fold owner producing a `BimModel` from a `Seq<SegmentedCloud>`.
- Cases: `ReconstructionPrimitive` arms `Plane` (the inlier-bounded planar patch a wall/slab/covering fits), `Cylinder` (the axis-and-radius swept solid a column/pile/pipe fits), `Torus` (the revolved solid a pipe-elbow/fitting fits), and `Freeform` (the residual mesh handle a proxy carries when no analytic primitive admits) (4); a new primitive family is one `ReconstructionPrimitive` arm plus one `PrimitiveShape` row plus one `ElementClassifier` table entry, never a per-shape fold.
- Entry: `Reconstruction.Reconstruct(Seq<SegmentedCloud> segments, ReconstructionContext context, ClockPolicy clocks)` folds the kernel-registered segments into a `BimModel` — each segment lifts to its `ReconstructionPrimitive` arm by its fitted `PrimitiveShape`, the arm projects to a `BimElement` through `ElementClassifier.Classify` (the frozen shape→`IfcClass` table resolving the entity class and the `PredefinedType` the `Model/elements#ELEMENT_MODEL` `IfcClass.AdmitPredefined` admits), the `FitConfidence` band lands as a `Pset_Reconstruction` `PropertyBinding`, and the `ReconstructionLineage` source-cloud key threads onto the element through the `csharp:Compute/Runtime/codecs#CONTENT_ADDRESSING` content key; `Fin<T>` aborts on a segment whose fitted shape the `ElementClassifier` table does not carry (`Model/faults#FAULT_BAND` `BimFault.UnmappedClass`) or a registration the kernel never produced (`BimFault.CapabilityMiss`), each lowered with `.ToError()` at the `Boundary` funnel.
- Auto: `Reconstruct` reads each `SegmentedCloud` already fitted and registered by the kernel — the `csharp:Rasm/Vectors#ALIGN` cloud-ICP alignment has placed the capture in the canonical kernel frame and the `csharp:Rasm/Geometry/spatial#SEGMENTATION` has partitioned it into shape-labeled segments whose planar/cylindrical boundaries read the `csharp:ROBUST_ARRANGEMENT_SUBSTRATE` exact-arithmetic arrangement face index — so the fold never re-fits geometry in-process; the `ReconstructionPrimitive.Of(segment)` lift selects the arm by `segment.Shape`, binding the kernel geometry handle the segmentation produced by reference; `ElementClassifier.Classify(primitive, context)` resolves the `IfcClass` from the frozen `PrimitiveShape`→`IfcClass` table refined by the `ReconstructionContext` discipline hint (a horizontal plane in a building context classifies `IfcSlab`, a vertical plane `IfcWall`, a cylinder `IfcColumn`, each carrying the table's default `PredefinedType` token the class row admits), and a shape with no table row faults `BimFault.UnmappedClass` rather than silently dropping the segment; the `FitConfidence` band is the segment inlier-ratio (`inliers / total`) admitted into `[0,1]`, written as the `Pset_Reconstruction.FitConfidence` `PropertyBinding` plus the `SourceSegment` and `Residual` rows so the `Model/query#ELEMENT_SET` `ByProperty` arm selects every below-threshold element for review; the `ReconstructionLineage` content key is the `InterchangeIdentity.Key("recon-cloud", segment.CloudBytes, context.Deflection, context.Tolerance, context.AngleTolerance)` over the segment's source-cloud payload so the `Review/diff#MODEL_DIFF` federation diff joins the reconstructed element to its as-designed counterpart and the `AppUi` reality-capture playback re-fetches the exact capture by lineage key.
- Receipt: the `ReconstructionPrimitive` arm is the typed fit evidence per element — the `Plane`/`Cylinder`/`Torus` arms carry the analytic fit parameters (frame, axis, radius) and the inlier count, the `Freeform` arm carries the residual mesh content key, and the `FitConfidence` band plus the `ReconstructionLineage` source key ride onto the `BimElement` as the `Pset_Reconstruction` rows a downstream review and the federation diff read; no generic `IFitResult`/reported-value abstraction, the union arms stay typed per primitive family.
- Packages: `Rasm` (the kernel geometry handle, the `csharp:Rasm/Vectors#ALIGN` cloud-ICP registration, the `csharp:Rasm/Geometry/spatial#SEGMENTATION` segmentation, and the `csharp:ROBUST_ARRANGEMENT_SUBSTRATE` arrangement — all consumed by reference), GeometryGymIFC_Core (the `Model/elements#ELEMENT_MODEL` `BimElement`/`IfcClass`/`PredefinedType` projection vocabulary and the `Model/elements#BIM_TYPE` `IfcRepresentationMap` instanced-geometry key the repeated fit reuses), Thinktecture.Runtime.Extensions (`[Union]`/`[SmartEnum]`/`[ValueObject]`), LanguageExt.Core (`Fin`/`Seq`/`Map`/`Option`), System.IO.Hashing (the `XxHash128` the content key composes), NodaTime (the `ClockPolicy.Now` the `BimModel` capture instant carries).
- Growth: a new fitted primitive is one `ReconstructionPrimitive` arm carrying its analytic parameters plus one `PrimitiveShape` `[SmartEnum]` row plus one `ElementClassifier` table entry mapping the shape and discipline to its `IfcClass`/`PredefinedType` — the fold and the classifier resolve the new shape with no new operation; a new classification rule is one `ElementClassifier` table row keyed on `(PrimitiveShape, IfcDomain, orientation)`, never an enumerated `switch` arm; a repeated identical fit (a window grid, a column array) mints one `Model/elements#BIM_TYPE` `BimType` carrying the `IfcRepresentationMap` key plus N occurrence references reading it, never N inlined geometries; a new confidence dimension is one `Pset_Reconstruction` `PropertyBinding` row, never a second receipt model; never a `FitPlane`/`FitCylinder`/`FitTorus` operation family and never a per-shape `BimElement` subtype.
- Boundary: reconstruction is the LAST fold to a `BimElement`, never a geometry kernel — the cloud-ICP registration is `csharp:Rasm/Vectors#ALIGN`'s, the plane/cylinder segmentation is `csharp:Rasm/Geometry/spatial#SEGMENTATION`'s, the exact-arithmetic arrangement the planar/cylindrical boundaries read is `csharp:ROBUST_ARRANGEMENT_SUBSTRATE`'s, and a re-minted RANSAC/region-grow fitter or a second arrangement here is the deleted form per the consume-the-kernel-by-reference law; the splat/point payload and the source-cloud content key are `csharp:Compute/Runtime/codecs#CONTENT_ADDRESSING`'s `InterchangeIdentity.Key` and a second hashing/identity scheme is the named seam violation; the fitted primitive binds the kernel `Rasm` geometry by `GeometryHandle` reference (or the `Model/elements#BIM_TYPE` `IfcRepresentationMap` content key for an instanced fit) and a RhinoCommon `Brep`/`Mesh` field on `ReconstructionPrimitive` is the named host-bound defect; the fold produces the one `Model/elements#ELEMENT_MODEL` `BimModel` and the reconstructed element is a `BimElement` discriminated by the same `IfcClass`/`PredefinedType` vocabulary an imported element carries — a parallel `ScannedElement`/`ReconstructedElement` record beside `BimElement` is the deleted form, so the `Model/query#ELEMENT_SET` query algebra and the `Review/validation#IDS_FACETS` audit read a reconstructed model with no second selection surface; the shape→class mapping is a frozen `ElementClassifier` data table keyed on `(PrimitiveShape, IfcDomain, orientation)`, not enumerated `switch` arms, so a wall-vs-slab disambiguation is a table row refined by the `ReconstructionContext` discipline hint rather than imperative branching; the `FitConfidence` band rides the `Semantics/properties#PROPERTY_SETS` `Pset_Reconstruction` rows the typed property store already owns, not a stringly-keyed property tacked on outside the property model; a segment whose fitted shape the `ElementClassifier` table does not carry faults `Model/faults#FAULT_BAND` `BimFault.UnmappedClass` and a registration the kernel never produced faults `BimFault.CapabilityMiss`, both lowered with `.ToError()`, so an unclassifiable scan never silently produces a half-built model; the `ReconstructionLineage` source-cloud key is the join the `Review/diff#MODEL_DIFF` federation diff and the `AppUi` reality-capture playback both read, never a second lineage scheme.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.IO.Hashing;
using System.Text;
using GeometryGym.Ifc;
using LanguageExt;
using NodaTime;
using Rasm;
using Rasm.Compute.Interchange;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Bim;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<InterchangeKeyPolicy, string>]
public sealed partial class PrimitiveShape {
    public static readonly PrimitiveShape Plane    = new("plane");
    public static readonly PrimitiveShape Cylinder = new("cylinder");
    public static readonly PrimitiveShape Torus    = new("torus");
    public static readonly PrimitiveShape Freeform = new("freeform");
}

public enum FitOrientation : byte {
    Any = 0, Horizontal = 1, Vertical = 2, Inclined = 3,
}

// --- [MODELS] -----------------------------------------------------------------------------
[ValueObject<double>]
public sealed partial class FitConfidence {
    static partial void NormalizeValidate(ref double value) =>
        value = double.IsFinite(value) ? Math.Clamp(value, 0.0, 1.0) : 0.0;

    public bool IsBelow(double threshold) => Value < threshold;
}

[ValueObject<UInt128>]
[KeyMemberEqualityComparer<InterchangeKeyPolicy, UInt128>]
public sealed partial class ReconstructionLineage {
    public static ReconstructionLineage Of(SegmentedCloud segment, ReconstructionContext context) =>
        ReconstructionLineage.Create(
            InterchangeIdentity.Key("recon-cloud", segment.CloudBytes.Span,
                context.Deflection, context.Tolerance, context.AngleTolerance));
}

public readonly record struct ReconstructionContext(
    IfcDomain Discipline,
    double Deflection,
    double Tolerance,
    double AngleTolerance,
    double ConfidenceFloor,
    double VerticalCosineLimit) {
    public static readonly ReconstructionContext Building =
        new(IfcDomain.Architecture, 1e-3, 1e-6, 1e-4, 0.6, 0.342);

    public FitOrientation OrientationOf(Vector3 normal) {
        double vertical = Math.Abs(Vector3.Dot(normal.Unit, Vector3.UnitZ));
        return vertical >= 1.0 - VerticalCosineLimit ? FitOrientation.Horizontal
            : vertical <= VerticalCosineLimit          ? FitOrientation.Vertical
            : FitOrientation.Inclined;
    }
}

public readonly record struct SegmentedCloud(
    int SegmentId,
    PrimitiveShape Shape,
    GeometryHandle Geometry,
    Vector3 Normal,
    Vector3 Axis,
    double Radius,
    double MinorRadius,
    int Inliers,
    int Total,
    Option<int> ArrangementFace,
    ReadOnlyMemory<byte> CloudBytes) {
    public FitConfidence Confidence => FitConfidence.Create(Total > 0 ? (double)Inliers / Total : 0.0);
    public double Residual => Total > 0 ? 1.0 - (double)Inliers / Total : 1.0;
}

[Union]
public partial record ReconstructionPrimitive {
    partial record Plane(int SegmentId, GeometryHandle Geometry, Vector3 Normal, FitConfidence Confidence, ReconstructionLineage Lineage);
    partial record Cylinder(int SegmentId, GeometryHandle Geometry, Vector3 Axis, double Radius, FitConfidence Confidence, ReconstructionLineage Lineage);
    partial record Torus(int SegmentId, GeometryHandle Geometry, Vector3 Axis, double Radius, double MinorRadius, FitConfidence Confidence, ReconstructionLineage Lineage);
    partial record Freeform(int SegmentId, GeometryHandle Geometry, FitConfidence Confidence, ReconstructionLineage Lineage);

    public static ReconstructionPrimitive Of(SegmentedCloud segment, ReconstructionContext context) {
        var lineage = ReconstructionLineage.Of(segment, context);
        return segment.Shape.Switch<ReconstructionPrimitive>(
            plane:    () => new Plane(segment.SegmentId, segment.Geometry, segment.Normal, segment.Confidence, lineage),
            cylinder: () => new Cylinder(segment.SegmentId, segment.Geometry, segment.Axis, segment.Radius, segment.Confidence, lineage),
            torus:    () => new Torus(segment.SegmentId, segment.Geometry, segment.Axis, segment.Radius, segment.MinorRadius, segment.Confidence, lineage),
            freeform: () => new Freeform(segment.SegmentId, segment.Geometry, segment.Confidence, lineage));
    }

    public PrimitiveShape Shape => Switch(
        plane:    static _ => PrimitiveShape.Plane,
        cylinder: static _ => PrimitiveShape.Cylinder,
        torus:    static _ => PrimitiveShape.Torus,
        freeform: static _ => PrimitiveShape.Freeform);

    public GeometryHandle Geometry => Switch(
        plane:    static p => p.Geometry,
        cylinder: static c => c.Geometry,
        torus:    static t => t.Geometry,
        freeform: static f => f.Geometry);

    public FitConfidence Confidence => Switch(
        plane:    static p => p.Confidence,
        cylinder: static c => c.Confidence,
        torus:    static t => t.Confidence,
        freeform: static f => f.Confidence);

    public ReconstructionLineage Lineage => Switch(
        plane:    static p => p.Lineage,
        cylinder: static c => c.Lineage,
        torus:    static t => t.Lineage,
        freeform: static f => f.Lineage);
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class ElementClassifier {
    static readonly Map<(string Shape, IfcDomain Domain, FitOrientation Orientation), (IfcClass Class, string Predefined)> Table =
        Map(
            (("plane",    IfcDomain.Architecture,  FitOrientation.Vertical),   (IfcClass.Wall,         "STANDARD")),
            (("plane",    IfcDomain.Architecture,  FitOrientation.Horizontal), (IfcClass.Slab,         "FLOOR")),
            (("plane",    IfcDomain.Architecture,  FitOrientation.Inclined),   (IfcClass.Roof,         "FREEFORM")),
            (("plane",    IfcDomain.Architecture,  FitOrientation.Any),        (IfcClass.Covering,     "CLADDING")),
            (("cylinder", IfcDomain.Architecture,  FitOrientation.Vertical),   (IfcClass.Column,       "COLUMN")),
            (("cylinder", IfcDomain.Architecture,  FitOrientation.Horizontal), (IfcClass.Beam,         "BEAM")),
            (("cylinder", IfcDomain.Structural,    FitOrientation.Vertical),   (IfcClass.Pile,         "BORED")),
            (("cylinder", IfcDomain.HvacFire,      FitOrientation.Any),        (IfcClass.FlowSegment,  "NOTDEFINED")),
            (("torus",    IfcDomain.HvacFire,      FitOrientation.Any),        (IfcClass.FlowFitting,  "NOTDEFINED")),
            (("freeform", IfcDomain.Architecture,  FitOrientation.Any),        (IfcClass.Proxy,        "ELEMENT")));

    public static Fin<(IfcClass Class, PredefinedType Predefined)> Classify(
        ReconstructionPrimitive primitive, SegmentedCloud segment, ReconstructionContext context) {
        var orientation = primitive switch {
            ReconstructionPrimitive.Plane p    => context.OrientationOf(p.Normal),
            ReconstructionPrimitive.Cylinder c => context.OrientationOf(c.Axis),
            _                                  => FitOrientation.Any,
        };
        var key = (primitive.Shape.Key, context.Discipline, orientation);
        return Table.Find(key)
            .OrElse(() => Table.Find((primitive.Shape.Key, context.Discipline, FitOrientation.Any)))
            .ToFin(new BimFault.UnmappedClass($"recon-shape-miss:{primitive.Shape.Key}:{context.Discipline}:{orientation}").ToError())
            .Bind(row => row.Class.AdmitPredefined(row.Predefined, row.Predefined)
                .Map(predefined => (row.Class, predefined)));
    }
}

public static class Reconstruction {
    public static Fin<BimModel> Reconstruct(Seq<SegmentedCloud> segments, ReconstructionContext context, ClockPolicy clocks) =>
        segments
            .TraverseM(segment => segment.Geometry.IsPending
                ? Fin.Fail<BimElement>(new BimFault.CapabilityMiss($"recon-unregistered:{segment.SegmentId}").ToError())
                : Project(segment, context))
            .As()
            .Map(elements => BimModel.Empty with { Elements = elements, At = clocks.Now });

    static Fin<BimElement> Project(SegmentedCloud segment, ReconstructionContext context) {
        var primitive = ReconstructionPrimitive.Of(segment, context);
        return ElementClassifier.Classify(primitive, segment, context)
            .Map(row => new BimElement(
                GlobalId:          ParserIfc.HashGlobalID($"recon:{primitive.Lineage.Value:X32}"),
                Class:             row.Class,
                Predefined:        row.Predefined,
                Name:              $"{row.Class.Key}-recon-{segment.SegmentId}",
                Tag:               segment.SegmentId.ToString(),
                Geometry:          primitive.Geometry,
                Properties:        Pset(primitive, segment),
                Quantities:        Seq<BimElement.QuantityBinding>(),
                Materials:         Seq<BimMaterial>(),
                Classifications:   Seq<ClassificationRef>(),
                TypeGlobalId:      Option<string>.None,
                SpatialContainerId: Option<string>.None));
    }

    static Seq<BimElement.PropertyBinding> Pset(ReconstructionPrimitive primitive, SegmentedCloud segment) =>
        Seq(
            new BimElement.PropertyBinding("Pset_Reconstruction", "FitConfidence", primitive.Confidence.Value.ToString("R")),
            new BimElement.PropertyBinding("Pset_Reconstruction", "Residual", segment.Residual.ToString("R")),
            new BimElement.PropertyBinding("Pset_Reconstruction", "PrimitiveShape", primitive.Shape.Key),
            new BimElement.PropertyBinding("Pset_Reconstruction", "SourceSegment", segment.SegmentId.ToString()),
            new BimElement.PropertyBinding("Pset_Reconstruction", "SourceCloud", primitive.Lineage.Value.ToString("X32")));
}
```

## [3]-[RESEARCH]

- [KERNEL_REGISTRATION_SEAM]: the `SegmentedCloud` carrier the `Reconstruct` fold reads (segment id, fitted `PrimitiveShape`, the kernel `Rasm` `GeometryHandle`, the fit normal/axis/radius, the inlier/total counts, and the `csharp:ROBUST_ARRANGEMENT_SUBSTRATE` arrangement face index) grounds against the kernel `csharp:Rasm/Vectors#ALIGN` cloud-ICP registration owner and the `csharp:Rasm/Geometry/spatial#SEGMENTATION` plane/cylinder segmentation owner at cross-folder alignment — the segmentation produces the shape-labeled segments already aligned by ICP into the canonical kernel frame and the exact-arithmetic arrangement bounds the planar/cylindrical patches, so the `SegmentedCloud.Geometry`/`Normal`/`Axis`/`Radius`/`ArrangementFace` member spellings and the `Vector3`/`Vector3.Dot`/`Vector3.UnitZ`/`Vector3.Unit` kernel-geometry surface confirm against the kernel geometry owner before the carrier is final; the consume-by-reference law holds — Bim re-fits no geometry and re-mints no arrangement, a `GeometryHandle.IsPending` segment is an unregistered capture the fold faults `BimFault.CapabilityMiss` rather than fitting in-process.
- [CONTENT_KEY_LINEAGE]: the `ReconstructionLineage` source-cloud key over `InterchangeIdentity.Key(string formatKey, ReadOnlySpan<byte> bytes, double deflection, double tolerance, double angleTolerance)` grounds against the `csharp:Compute/Runtime/codecs#CONTENT_ADDRESSING` content-key owner at cross-folder alignment so the reconstructed element joins its source capture by the same content key the `Review/diff#MODEL_DIFF` `ElementFingerprint.ContentKey` and the export artifact address — Bim mints no second identity scheme; the splat/point payload the `segment.CloudBytes` carries is the Compute interchange owner's, consumed as settled vocabulary, and the `XxHash128.HashToUInt128` the content key composes is BCL `System.IO.Hashing` inbox and settled.
- [ELEMENT_PROJECTION]: the `BimElement` the `Project` body mints, the `Model/elements#ELEMENT_MODEL` `IfcClass.AdmitPredefined(token, objectType)` the `ElementClassifier` resolves the default predefined token through, the `Model/elements#BIM_TYPE` `IfcRepresentationMap` instanced-geometry key a repeated fit reuses, and the `Semantics/properties#PROPERTY_SETS` `Pset_Reconstruction` `PropertyBinding` the `FitConfidence` band rides confirm against the `Model/elements#ELEMENT_MODEL` element-vocabulary owner and the `Semantics/properties#PROPERTY_SETS` property store as settled vocabulary — a reconstructed element is a `BimElement` discriminated by the same `IfcClass`/`PredefinedType` axes an imported element carries, so the `Model/query#ELEMENT_SET` `ByClass`/`ByDomain`/`ByProperty` arms and the `Review/validation#IDS_FACETS` audit read a reconstructed model with no second selection surface; the deterministic GlobalId mint from the lineage content key rides `ParserIfc.HashGlobalID(string uniqueString)` (`.api/api-geometrygym-ifc` GlobalId codec scope: `DecodeGlobalID`/`EncodeGuid`/`HashGlobalID`) so a re-run of the same capture yields the same stable GlobalId and the `Review/diff#MODEL_DIFF` federation diff dedups the re-reconstructed element against its prior pass rather than minting a fresh identity.
