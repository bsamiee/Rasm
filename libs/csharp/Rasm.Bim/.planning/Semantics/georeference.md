# [BIM_COORDINATE_PROJECTION]

`GeoReferenceProjector` folds whatever georeferencing level an IFC model carries onto the `Rasm.Element` seam `GeoReference` record on the `ElementGraph` `Header` and `Coverage` nodes: `Project` switches the single `IfcGeometricRepresentationContext.HasCoordinateOperation` over an `IfcMapConversion`/`IfcMapConversionScaled` (rotation-and-scale, LoGeoRef 50) or a translation-only `IfcRigidOperation` (length-measured planar form, LoGeoRef 50, IFC4.3), falls back to an `IfcSite` geographic position lowered onto WGS84 (`EPSG:4326`, LoGeoRef 30), and returns `GeoReference.Identity` when a model carries neither so ingest never blocks. `GeoTransform` reprojects raw double-precision ordinate spans between two seam frames over `ProjNET`, escalating an exotic datum-grid or dynamic-datum transform to the `MaxRev.Gdal.Core` OSR PROJ engine, and returns the typed `Reprojection` receipt — engine route, shifted-vertex count, at-anchor differential scale and grid rotation, forward→inverse round-trip residual, dynamic-datum epoch posture — a survey-grade federation validates its rigid placement against.

`GeoReference`/`ProjectedCrs` are SEAM-owned (`Rasm.Element/Geospatial/reference`): the seam owns the 12-field record, its `GeoReference.Admit` admission, the three-state EPSG/WKT/projection resolution, and the derived `Epsg`/`CrsResolution` columns, while Bim owns only the IFC projection that fills the record and the `ProjNET`/OSR transform that consumes it. Every seam double arrives metre-normalized through the ONE `Projection/semantic#SEMANTIC_PROJECTOR` `UnitScale` pair over TWO distinct regimes the schema keeps apart — the MAP frame (`IfcProjectedCRS.MapUnit`, the declared length unit of the map coordinate axes, which every map ordinate and the seam's own metre-normalized scale ride) and the MODEL frame (the project `IfcUnitAssignment`, which the `IfcSite` elevation alone rides) — because a map ordinate is never a model-unit magnitude and the model↔map conversion is `IfcMapConversion.Scale`'s own declared job; `Project` re-bands the seam fault to `BimFault.CapabilityMiss` bare (band 2600) when a CRS name resolves to neither an EPSG code nor a WKT nor a projection+zone, never silently landing a federation on an unreferenced frame [M1].

Seam `GeoReference` rides `Header`/`Coverage` only, never the `Object` node, carrying the rigid map-conversion offset parameters — translation, projected `RotationRadians` direction-cosine, per-axis scale — a downstream host-bound consumer folds into the kernel `Transform` algebra in the Rhino runtime. This host-neutral page binds GeometryGym, `ProjNET`/`MaxRev.Gdal.Core`, the seam, and the kernel `Op` key, never the RhinoCommon `Transform`/`Point3d`/`Vector3d` it therefore cannot materialize.

## [01]-[INDEX]

- [02]-[GEO_PROJECTION]: `GeoReferenceProjector.Project` folds the model georeferencing level onto the seam `GeoReference` — the `HasCoordinateOperation` switch over `IfcMapConversion`/`IfcMapConversionScaled` or the translation-only `IfcRigidOperation`, else the `IfcSite` WGS84 fallback, else `Identity` — composing the seam `GeoReference.Admit`; `GeoAuthored` the closed level vocabulary the egress elects and returns; `VerticalCrs` the three-state height-datum resolution over the seam `VerticalDatum` declaration that the `[03]-[GEODETIC_TRANSFORM]` refusal arm reads; `Author` the railed egress inverse `Projection/egress#IFC_EGRESS` `Emit` composes so an ingested LoGeoRef level round-trips at its own level instead of exporting geo-stripped or level-flattened [M1].
- [03]-[GEODETIC_TRANSFORM]: `GeoTransform.Reproject` the `ProjNET`-first datum leg over two seam frames returning the typed `Reprojection` receipt, escalating a transform `ProjNET` cannot express to the `MaxRev.Gdal.Core` OSR engine; `GeoTransform.Preflight` the frame-identity-memoized federation alignment matrix of typed `FrameVerdict` rows a `Review/coordination#COORDINATION` rule engine gates its GlobalId joins on.

## [02]-[GEO_PROJECTION]

- Owner: `GeoReferenceProjector` the static IFC→seam projector folding the georeferencing level the model carries onto the seam `GeoReference` — the single `IfcGeometricRepresentationContext.HasCoordinateOperation` switched over `IfcMapConversion` (or `IfcMapConversionScaled` for per-axis scale) and the translation-only `IfcRigidOperation` (LoGeoRef 50 — the IFC4.3 rigid sibling carries no rotation and no scale, only the two coordinates + `Height` in the target CRS), else the first `IfcSite`'s `RefLatitude`/`RefLongitude`/`RefElevation` geographic position onto a WGS84 (`EPSG:4326`) reference (LoGeoRef 30), else `GeoReference.Identity`; and `GeoAuthored` the closed `[SmartEnum<string>]` level vocabulary the egress elects from the frame's own columns and returns as its receipt. The `GeoReference`/`ProjectedCrs` value-objects are seam-owned (`Rasm.Element/Geospatial/reference`); this page projects the IFC surface onto them — the ONE `Carriers` read handing the base-CRS `Name`/`GeodeticDatum`/`WellKnownText` and the projected-only `VerticalDatum`/`MapProjection`/`MapZone` into the seam's three-state `ProjectedCrs` (an `IfcGeographicCRS` target keeps its base carriers, never a null-CRS drop), and composing the metre-normalized scale and the seam `GeoReference.Admit` (which owns the three-state EPSG/WKT/projection resolution and the 12-field record construction) — never re-declaring them, never re-deriving the admission, never re-minting a CRS parser, and never materializing a kernel transform (host-neutral).
- Entry: `GeoReferenceProjector.Project(IfcProject project, UnitScale model, Op key)` projects the model's georeferencing into the seam `GeoReference` — a model with no map conversion AND no geographic site position returns `GeoReference.Identity` so ingest never blocks; an `IfcProjectedCRS` name present but resolving no EPSG FAULTS `Model/faults#FAULT_BAND` `BimFault.CapabilityMiss` BARE (band 2600 IS the `Expected` `Code` — NO `.ToError()` lowering hop) rather than landing the federation on an unreferenced frame [M1]; `Fin<GeoReference>` carries the result; the `Op key` and the MODEL `UnitScale` both thread from the `Projection/semantic#SEMANTIC_PROJECTOR` context — the projector building the coercion record ONCE per projection and composing the success onto the `ElementGraph` `Header.Reference`, this page deriving the MAP frame's own regime per operation from the CRS it targets. `GeoReferenceProjector.Author(DatabaseIfc db, GeoReference reference, UnitScale model, Op key)` is the egress inverse `Projection/egress#IFC_EGRESS` `Emit` composes beside `ReauthorHeader`, returning `Fin<GeoAuthored>` — the LEVEL it authored, so a caller reads which LoGeoRef level survived rather than trusting a void call: `Unreferenced` when the frame is `Identity` (nothing authored, never a fabricated frame), `Geographic` when the frame is the EPSG:4326 site shape (`IfcSite` re-stamped), `Rigid` when the frame carries the translation-only metre-identity shape (`IfcRigidOperation` + `IfcProjectedCRS`), `Conversion` for an isotropic scale (`IfcMapConversion`), and `Scaled` for an anisotropic one on a schema that carries the subtype (`IfcMapConversionScaled`) — with `BimFault.ModelRejected` on a database carrying no `IfcProject` and `BimFault.DanglingReference` when the elected level's anchor entity (the site, the representation context) is absent, each formerly a silent early return that reported success while writing nothing.
- Auto: `Project` is a three-arm fold — the LoGeoRef-50 map-conversion arm reads the `IfcMapConversion` rigid offset (`Eastings`/`Northings`/`OrthogonalHeight`), the `XAxisAbscissa`/`XAxisOrdinate` rotation direction-cosine pair (each defaulting to `double.NaN` when the rotation is unset, coerced to the identity direction `(1,0)` so `RotationRadians` resolves to `0` rather than `Atan2(NaN,NaN)`), and the per-axis scale as the schema COMPOSES it — IFC4.3 gives `IfcMapConversion` ONE `Scale` applied equally to x, y and z (the model-unit↔CRS-unit reconciliation, `1.0` when omitted) and puts the per-axis factors on the `IfcMapConversionScaled` subtype where the transform is `Scale × Factor(axis)`, so the seam axis is that product and never a factor alone (a factor-only read drops the unit conversion whole on every scaled export) and never GeometryGym's vendor `ScaleY`/`ScaleZ` pair (they default to `1.0` rather than to `Scale`, so reading them turns a mm-model isotropic `Scale` into an anisotropic frame whose Y and Z lost their unit conversion); the LoGeoRef-50 rigid-operation arm reads `IfcRigidOperation.FirstCoordinate`/`SecondCoordinate` in its length-measured planar form ONLY — the `Project` guard pattern-binding BOTH as `IfcLengthMeasure` (the schema's own `SameCoordinateType` rule admits length or plane-angle, never a mix) so each magnitude reads off the public `IfcMeasureValue.Measure` double with no boxed `Convert` hop, `First`→`Eastings`/`Second`→`Northings`, `Height` (NaN→`0.0`) likewise, identity direction `(1,0)` and the map frame's own metre factor on all three scale axes (a rigid operation declares NO conversion and NO distortion, so the model coordinates ARE map coordinates and the only residual scale is the map's length unit) — the angle-measured geographic-target form left to the site arm's `Identity` (a mislocated federation is worse than an ungeoreferenced ingest); BOTH operation arms carry every map ordinate AND the composed axis scale through the ONE `UnitScale.Coerce` entry over the MAP frame's regime, built from the target `IfcProjectedCRS.MapUnit` — the schema's only declaration of the map axes' length unit, constrained `LENGTHUNIT` by its own `MapUnitIsLength` rule, and given NO default by the schema, so an undeclared map unit leaves the CRS itself the authority and the metre every EPSG projected axis publishes is the read (the model's `IfcUnitAssignment` regime never applies to a map ordinate) — and hand the offset, rotation pair, scale, datum names, and CRS strings through the ONE `Carriers` read — the base-CRS `Name` (GeometryGym `"Unknown"` sentinel normalized to blank)/`GeodeticDatum`/`WellKnownText.WellKnownText` plus the projected-only `VerticalDatum`/`MapProjection`/`MapZone` — to the seam `GeoReference.Admit`; the LoGeoRef-30 arm folds an `IfcSite`'s `RefLatitude`/`RefLongitude` `IfcCompoundPlaneAngleMeasure` through `.Angle()` to decimal degrees and coerces `RefElevation` (NaN→`0.0`) through the MODEL regime — the one magnitude on this page that IS a project-unit length, so a mm-declared export lands metres — then hands the literal `EPSG:4326` authority name with blank `wkt`/`mapProjection`/`mapZone` to the same `Admit` for a WGS84 reference (longitude east, latitude north, identity rotation, unit scale). Inside `Admit` the seam builds the three-state `ProjectedCrs` and `ProjectedCrs.Epsg` resolves the EPSG code from the authority `Name` across the OGC URN (`urn:ogc:def:crs:EPSG::25832`), the authority form (`EPSG:25832`), and a bare numeric code, while a WKT-defined CRS (blank/unresolvable `Name`, non-blank `Wkt`) or a projection+zone CRS resolves WITHOUT an EPSG; the projector first normalizes the GeometryGym empty-name sentinel `"Unknown"` (its `IfcCoordinateReferenceSystem.Name` setter coerces an empty value to `"Unknown"`) back to blank so `Admit` reads it as the no-CRS state, a name present yet resolving to NO identity at all (no EPSG, no WKT, no projection+zone) faulting (the seam `ElementFault` re-banded to `BimFault.CapabilityMiss`) rather than degrading to a no-op transform, a WKT-resolvable CRS never faulted. `Author` elects its level from the frame's own columns through one TOLERANCE compare — the `RotationRadians` is an `Atan2` derivative and each scale a product of unit factors, so an exact `== 0.0`/`== 1.0` reads a numerically-identity frame as a rotated one and authors the wrong level — authors ONE `IfcProjectedCRS` that declares NO `MapUnit` (so the authored map frame IS metre and the metre-normalized seam ordinates land verbatim, the exact inverse of the ingress read), and splits the scale back the way the schema composes it: three equal axes author the isotropic `Scale`, differing axes author `Scale = 1.0` with the three `Factor` columns, and differing axes on a pre-`IFC4X3_ADD2` target author the shared component isotropically because GeometryGym writes the subtype under the base class name there and the factors vanish silently — the receipt naming `Conversion` rather than `Scaled` so the bounded anisotropy drop is READ, never assumed.
- Receipt: the seam `GeoReference` is the coordinate-reference evidence the `Header` carries (and the `Semantics/geospatial#RASTER_INGEST` `Coverage` node carries for a georeferenced raster); its parameters (the translation, the seam-projected `RotationRadians` direction-cosine, the per-axis metre-frame scale) feed a DOWNSTREAM host-bound kernel `Transform` consumer in the Rhino runtime, never a transform this host-neutral projector builds; the seam `CrsResolution` mode (the derived `Epsg`-vs-`Wkt` column) drives the `[03]-[GEODETIC_TRANSFORM]` `ProjNET`/OSR datum leg's EPSG-keyed-vs-WKT-keyed build-path selection; the egress `GeoAuthored` level is the round-trip evidence an export audit reads — an ingest that recorded a rigid operation and an export that reports `Rigid` is a closed loop, where the reported `Conversion` on the same frame names a level promotion.
- Packages: GeometryGymIFC_Core, Rasm.Element, Rasm (the host-neutral `Rasm.Domain.Op` key and the `UnitScale`/`MeasureRow` coercion pair), Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new map-conversion parameter is one column on the seam `GeoReference` (the seam's, not this page's); a new georeferencing level is one arm on the `Project` fold plus one `GeoAuthored` row and its election clause (a future LoGeoRef-40 `WorldCoordinateSystem`/`TrueNorth` rotation folds onto the existing rotation field); a new CRS-name scheme is one arm on the seam `ProjectedCrs.Epsg`; the rigid offset is the downstream host-bound kernel transform's and the datum shift is the `[03]-[GEODETIC_TRANSFORM]` leg; never a new transform owner, never a Bim CRS parser, never a per-CRS class, and never a page-local unit factor beside the `UnitScale` pair.
- Boundary: the `GeoReference`/`ProjectedCrs` value-objects are SEAM-owned [M1] and re-declaring them in Bim is the named drift defect — this page is the IFC projector that fills the seam value, never its owner; the seam `GeoReference` is the 12-field record (`Eastings`/`Northings`/`OrthogonalHeight`, `XAxisAbscissa`/`XAxisOrdinate`, per-axis `ScaleX`/`ScaleY`/`ScaleZ`, `GeodeticDatum`/`VerticalDatum`, and one `Option<ProjectedCrs> Crs`) with `Epsg` a DERIVED `Crs.Bind(c => c.Epsg)` property (NOT a stored slot), so constructing a `GeoReference` with a stored `Epsg` slot, a `ProjectedCrsName` field, or any column the seam does not declare is the deleted form; the seam `GeoReference` is carried on `Header`/`Coverage` ONLY and a `GeoReference` on the `Object` node is the deleted form [M1]; the projector composes the seam `GeoReference.Admit` (the one admission owning the three-state EPSG/WKT/projection resolution via the `ProjectedCrs` `[ComplexValueObject]`, the fault-on-fully-unresolvable, and the 12-field record construction) and re-deriving that admission inline (an `new GeoReference(...)` construction with an inline CRS parse beside `Admit`) OR a hand-rolled Bim CRS parser is the deleted form; the per-axis seam scale is the product IFC4.3 itself composes — `IfcMapConversion.Scale` alone on a plain conversion (the schema declares ONE scale applied equally to x, y and z, `1.0` when omitted) and `Scale × FactorX`/`FactorY`/`FactorZ` on the `IfcMapConversionScaled` subtype — so a `Factor`-only read (which drops the unit reconciliation whole) and a read of GeometryGym's vendor `ScaleY`/`ScaleZ` pair (which default to `1.0` rather than to `Scale`, fabricating an anisotropic frame out of every isotropic non-metre model) are both named defects; that composed scale AND every map ordinate cross the `UnitScale.Coerce` entry over the MAP frame's regime — a ratio in map-units-per-model-unit converts to metres-per-model-unit by the SAME length factor a length in map units takes, and a map unit is never affine, so one entry carries both [M1] — so the doubles handed to `Admit` are METRE-NORMALIZED (the seam frame's stated contract), the unit reconciliation composed HERE at ingest so the seam record carries metre doubles and NO `MapUnit` field (a CRS-native-unit double on the seam is the rejected form); the MAP regime and the MODEL regime are two distinct `UnitScale` values and crossing them is the named defect — the map ordinates and the composed scale ride the frame built from `IfcProjectedCRS.MapUnit` (the schema's only map-axis unit declaration, `LENGTHUNIT`-constrained by `MapUnitIsLength` and given NO default, so an undeclared unit reads as the metre the CRS itself publishes and never as the project's own length unit), while the `IfcSite` elevation alone rides the project regime; a page-local `MetrePerMapUnit`-shaped member returning a raw multiplier is the deleted form the `UnitScale` pair exists to close; the three CRS carriers (the authority `Name`, the inline `WellKnownText.WellKnownText`, and `MapProjection`/`MapZone`) are ALL read off the GeometryGym surface and handed to `Admit` so a WKT-defined or projection-defined CRS resolves to its seam state — dropping the `Wkt`/`MapProjection`/`MapZone` carry and handing only the `Name` (the deleted two-state slice that false-faults a GIS-origin WKT CRS as unresolvable) is the named defect; the unset `XAxisAbscissa`/`XAxisOrdinate` (`double.NaN`) coerces to the identity direction `(1,0)` and reading the raw NaN into the rotation is the named defect; a DECLARED non-positive or non-finite scale component — `IfcMapConversion.Scale`, an `IfcMapConversionScaled` `Factor` column, or the map unit's own `SIFactor()` — FAULTS `BimFault.ModelRejected` by column name, because coercing a zero to unity deletes the seam's strictly-positive scale gate before `Admit` can see the value it was handed and a negative one mirrors the frame; absence needs no coercion beside it, the `Scale` getter answering unity for its NaN unset field and the three `Factor` fields initializing to unity, so a page-local absent-to-unity fold is the deleted duplicate; the rigid operation's two coordinates read off the public `IfcMeasureValue.Measure` double under a pattern that BINDS both as `IfcLengthMeasure`, and a boxed `Convert.ToDouble(measure.Value)` that escapes the rail on a null or angle-measured coordinate is the deleted form; the projection rides the GeometryGym `IfcMapConversion`/`IfcMapConversionScaled`/`IfcRigidOperation`/`IfcProjectedCRS`/`IfcWellKnownText`/`IfcSite` surface consumed as settled vocabulary (`.api/api-geometrygym-ifc` georeferencing entities), a hand-rolled IFC reader the deleted form; the rigid map-conversion offset is NOT built here — the kernel `Transform`/`Point3d`/`Vector3d` are RhinoCommon types and composing them on this host-neutral projector is the named host-neutrality defect, the seam `GeoReference` carrying the offset parameters a downstream host-bound consumer folds; a CRS name present but resolving to NO identity at all (no EPSG, no WKT, no projection+zone) FAULTS `BimFault.CapabilityMiss` BARE [M1] (a `.ToError()` lowering hop or a keyless `new BimFault.CapabilityMiss(detail)` construction the named defect this aligns to `Model/faults#FAULT_BAND`) while a WKT-resolvable CRS is VALID and silently landing the model on an unreferenced frame is the named defect; a non-georeferenced model returns `GeoReference.Identity` so ingest never blocks on a missing CRS; the egress is RAILED and TOTAL over the level vocabulary — a `void Author` whose two absent-anchor paths returned silently is the deleted form that reported a written frame it never wrote, and an egress that flattens every non-site frame onto `IfcMapConversion` is the level-promotion defect the `GeoAuthored` election closes; the authored `IfcProjectedCRS` declares no `MapUnit`, so the authored map frame is metre and the metre-normalized seam ordinates land VERBATIM — an inverse map-unit fold at egress divides by a unit the authored CRS never declares; the anisotropic scale authors as `Scale = 1.0` plus the three `Factor` columns on `IfcMapConversionScaled` and ONLY on an `IFC4X3_ADD2`-or-later target, because GeometryGym writes that subtype under the base `IfcMapConversion` class name on an earlier release and the factors vanish with no diagnostic — an older target authors the shared component isotropically and the receipt reports `Conversion`, so the bounded anisotropy drop is a READ level rather than a silent one; the level election compares against a declared epsilon and an exact `RotationRadians == 0.0` or `ScaleX == 1.0` on those derived doubles is the deleted form.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using GeometryGym.Ifc;
using LanguageExt;
using Rasm.Bim.Projection;                          // the ONE UnitScale/MeasureRow coercion pair every magnitude crosses
using Rasm.Element.Geospatial;
using Thinktecture;
using Op = Rasm.Domain.Op;                          // the host-neutral kernel operation key; NEVER the Rhino-bound kernel geometry
using static LanguageExt.Prelude;

namespace Rasm.Bim;

// --- [TYPES] --------------------------------------------------------------------------------
// The georeferencing entity an egress AUTHORED — the receipt Author returns so a caller reads what survived the
// re-export rather than trusting a void call, and the discriminant Author itself elects from the frame's own
// columns. Unreferenced: the Identity frame, nothing authored (never a fabricated frame). Geographic: LoGeoRef 30,
// the EPSG:4326 site shape re-stamped onto IfcSite. Rigid: LoGeoRef 50, the translation-only IfcRigidOperation.
// Conversion: LoGeoRef 50, the isotropic IfcMapConversion. Scaled: LoGeoRef 50, the anisotropic
// IfcMapConversionScaled — a SEPARATE row because a pre-IFC4X3_ADD2 target cannot carry it and reports Conversion
// with its anisotropy dropped, which a caller must be able to read. Keyed for telemetry, never a bool.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GeoAuthored {
    public static readonly GeoAuthored Unreferenced = new("unreferenced");
    public static readonly GeoAuthored Geographic   = new("geographic");
    public static readonly GeoAuthored Rigid        = new("rigid");
    public static readonly GeoAuthored Conversion   = new("conversion");
    public static readonly GeoAuthored Scaled       = new("scaled");
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class GeoReferenceProjector {
    // The model georeferencing fold onto the seam GeoReference: switch the single HasCoordinateOperation — an
    // IfcMapConversion(Scaled) (LoGeoRef 50, rotation+scale) OR the translation-only IfcRigidOperation (the IFC4.3
    // rigid sibling under IfcCoordinateOperation, First/Second/Height in a PROJECTED target — the length-measured planar
    // offset; an angle-measured geographic-target rigid operation is left to the site arm's Identity so no ambiguous
    // radian<->degree fold ever mislocates a federation) — else the IfcSite RefLatitude/RefLongitude/RefElevation
    // geographic position onto a WGS84 (EPSG:4326) reference (LoGeoRef 30), else Identity so ingest never blocks. Every
    // arm COMPOSES the seam GeoReference.Admit (the seam owns the three-state EPSG/WKT/projection resolution +
    // fault-on-fully-unresolvable + 12-field record construction); Bim only reads the IFC surface through the ONE
    // Carriers read, coerces through the ONE UnitScale entry, normalizes the GeometryGym "Unknown" empty-name
    // sentinel, and re-bands Admit's seam fault. The rigid guard binds BOTH coordinates as IfcLengthMeasure — the
    // schema's own SameCoordinateType rule admits length OR plane-angle for the pair and never a mix — so the arm
    // reads two public IfcMeasureValue.Measure doubles (decompile-confirmed) with no boxed Convert hop and no null
    // coordinate reaching it. `model` is the PROJECT unit regime and reaches the site elevation ALONE; every map
    // ordinate rides the map frame this page derives per operation.
    public static Fin<GeoReference> Project(IfcProject project, UnitScale model, Op key) =>
        Optional(project.RepresentationContexts
                .OfType<IfcGeometricRepresentationContext>()
                .Select(static ctx => ctx.HasCoordinateOperation)
                .FirstOrDefault(static op => op is IfcMapConversion
                    || op is IfcRigidOperation { FirstCoordinate: IfcLengthMeasure, SecondCoordinate: IfcLengthMeasure }))
            .Match(
                Some: op => op switch {
                    IfcMapConversion conversion => FromMapConversion(conversion, key),
                    IfcRigidOperation { FirstCoordinate: IfcLengthMeasure first, SecondCoordinate: IfcLengthMeasure second } rigid
                                                => FromRigidOperation(rigid, first, second, key),
                    _                           => Fin.Succ(GeoReference.Identity),
                },
                None: () => Optional(project.Extract<IfcSite>().FirstOrDefault())
                    .Match(Some: site => FromSite(site, model, key), None: static () => Fin.Succ(GeoReference.Identity)));

    static Fin<GeoReference> FromMapConversion(IfcMapConversion conversion, Op key) {
        // IFC4.3 gives IfcMapConversion ONE Scale — "to be used when the units of the CRS are not identical to the
        // units of the engineering coordinate system", applied EQUALLY to x, y and z, 1.0 when omitted — and puts the
        // per-axis factors on the IfcMapConversionScaled subtype, whose own transform composes Scale . Factor(axis)
        // ("Scale converts units, the Factors scale coordinates"). So the seam axis is that PRODUCT: reading a Factor
        // alone drops the unit reconciliation whole, and reading GeometryGym's vendor ScaleY/ScaleZ pair is worse
        // still — both getters answer 1.0 when unset rather than falling back to Scale, so an isotropic mm-model
        // conversion would land (0.001, 1.0, 1.0), an anisotropic frame whose Y and Z silently lost their unit
        // conversion. Absence needs no coercion here: the Scale getter already answers 1.0 for its NaN unset field and
        // the three Factor fields initialize to 1.0, so Positive gates the DECLARED value alone.
        Fin<(double X, double Y, double Z)> axes = conversion is IfcMapConversionScaled scaled
            ? from s in Positive(conversion.Scale, nameof(IfcMapConversion.Scale), key)
              from fx in Positive(scaled.FactorX, nameof(IfcMapConversionScaled.FactorX), key)
              from fy in Positive(scaled.FactorY, nameof(IfcMapConversionScaled.FactorY), key)
              from fz in Positive(scaled.FactorZ, nameof(IfcMapConversionScaled.FactorZ), key)
              select (s * fx, s * fy, s * fz)
            : Positive(conversion.Scale, nameof(IfcMapConversion.Scale), key).Map(static s => (s, s, s));
        // XAxisAbscissa/XAxisOrdinate default to NaN when the rotation is unset — coerce the pair to the identity
        // direction (1,0) so the seam RotationRadians resolves to 0 rather than Atan2(NaN,NaN). The pair is a
        // direction cosine, dimensionless, so it crosses no unit entry.
        double abscissa = double.IsNaN(conversion.XAxisAbscissa) ? 1.0 : conversion.XAxisAbscissa;
        double ordinate = double.IsNaN(conversion.XAxisOrdinate) ? 0.0 : conversion.XAxisOrdinate;
        // The seam is metre-normalized, so the offsets (map-CRS lengths) AND the composed axis scale (a ratio whose
        // OUTPUT unit is the map unit) both cross the map frame's own UnitScale entry: a length in map units and a
        // ratio in map-units-per-model-unit convert by the SAME factor, and a map unit is never affine, so one entry
        // carries both and no member here multiplies a raw factor.
        return from map in MapFrame(conversion.TargetCRS, key)
               from axis in axes
               from reference in Admit(
                   Metres(conversion.Eastings, map), Metres(conversion.Northings, map), Metres(conversion.OrthogonalHeight, map),
                   abscissa, ordinate, Metres(axis.X, map), Metres(axis.Y, map), Metres(axis.Z, map), conversion.TargetCRS, key)
               select reference;
    }

    // LoGeoRef 50, the IFC4.3 rigid sibling: "a rigid operation specifies an offset in the coordinate reference
    // system; it does not specify any conversion or distortion" — First/Second/Height in the target CRS, no rotation
    // and no scale channel at all. The model coordinates therefore ARE map coordinates, so the only residual seam
    // scale is the map frame's own metre factor on all three axes (unity for a metre CRS, ~0.3048006096 for a
    // US-survey-foot State Plane zone) — writing a hard 1.0 there would strand a foot-unit rigid model at foot
    // magnitudes inside a metre-contract seam. The Project guard already bound both coordinates as IfcLengthMeasure,
    // so this arm takes them typed and reads the public Measure double directly; the angle-measured geographic form
    // is intentionally not folded (a mislocated federation is worse than the Identity the site arm yields).
    static Fin<GeoReference> FromRigidOperation(IfcRigidOperation rigid, IfcLengthMeasure first, IfcLengthMeasure second, Op key) =>
        MapFrame(rigid.TargetCRS, key).Bind(map => {
            double metre = Metres(1.0, map);
            return Admit(
                Metres(first.Measure, map), Metres(second.Measure, map),
                Metres(double.IsNaN(rigid.Height) ? 0.0 : rigid.Height, map),
                1.0, 0.0, metre, metre, metre, rigid.TargetCRS, key);
        });

    // LoGeoRef 30: the IfcSite geographic position onto a WGS84 (EPSG:4326) reference — RefLatitude/RefLongitude are
    // IfcCompoundPlaneAngleMeasure (deg/min/sec/micro), folded to decimal degrees by .Angle(); a site missing either
    // angle is ungeoreferenced (Identity), longitude landing Eastings and latitude Northings in the geographic frame.
    // RefElevation is the ONE magnitude on this page that is a PROJECT-unit IfcLengthMeasure (IfcSite declares no map
    // frame), so it alone crosses the MODEL regime — a mm-declared export lands metres. The seam Admit resolves the
    // literal EPSG:4326 by authority code with no fault, so this arm passes the three CRS strings blank — the WGS84
    // reference is EPSG-resolved, not WKT/projection-defined.
    static Fin<GeoReference> FromSite(IfcSite site, UnitScale model, Op key) =>
        site.RefLatitude is null || site.RefLongitude is null
            ? Fin.Succ(GeoReference.Identity)
            : GeoReference.Admit(
                site.RefLongitude.Angle(), site.RefLatitude.Angle(),
                Metres(double.IsNaN(site.RefElevation) ? 0.0 : site.RefElevation, model),
                1.0, 0.0, 1.0, 1.0, 1.0, "WGS84", "", "EPSG:4326", "", "", "", key);

    // The ONE offset->seam admission both operation arms compose: the metre-normalized rigid offset + rotation pair +
    // per-axis scale, the datum names + three CRS carriers read through Carriers, the seam ElementFault re-banded to
    // Bim's CapabilityMiss at the boundary [M1] (one hop: Admit builds the three-state ProjectedCrs, parses EPSG across
    // EPSG:/URN/authority, resolves a WKT/projection CRS, faults ONLY a name with no EPSG/WKT/projection).
    static Fin<GeoReference> Admit(double e, double n, double h, double abscissa, double ordinate, double sx, double sy, double sz, IfcCoordinateReferenceSystem? crs, Op key) {
        var (name, datum, vertical, wkt, mapProjection, mapZone) = Carriers(crs);
        return GeoReference.Admit(e, n, h, abscissa, ordinate, sx, sy, sz, datum, vertical, name, wkt, mapProjection, mapZone, key)
            .MapFail(_ => new BimFault.CapabilityMiss(key, $"crs-name-unresolvable:{name}"));
    }

    // The ONE CRS-carrier read [§4-RT M1]: the base IfcCoordinateReferenceSystem carries Name/GeodeticDatum/
    // WellKnownText, the projected subtype adds VerticalDatum/MapProjection/MapZone. GeometryGym coerces an empty Name
    // to the "Unknown" sentinel; normalize it back to blank so the seam Admit reads the no-CRS state rather than faulting
    // a valid ungeoreferenced offset. Reading all three CRS states (Name, inline WKT, projection identity) is what lets
    // the seam Admit resolve a GIS-origin WKT/projection CRS instead of false-faulting the deleted two-state Name-only slice.
    static (string Name, string GeodeticDatum, string VerticalDatum, string Wkt, string MapProjection, string MapZone) Carriers(IfcCoordinateReferenceSystem? crs) =>
        crs is null
            ? ("", "", "", "", "", "")
            : (string.Equals(crs.Name, "Unknown", StringComparison.OrdinalIgnoreCase) ? "" : crs.Name ?? "",
               crs.GeodeticDatum ?? "", (crs as IfcProjectedCRS)?.VerticalDatum ?? "",
               crs.WellKnownText?.WellKnownText ?? "", (crs as IfcProjectedCRS)?.MapProjection ?? "", (crs as IfcProjectedCRS)?.MapZone ?? "");

    // The MAP frame's own metre regime — NOT the model's. IfcProjectedCRS.MapUnit is the schema's ONLY declaration of
    // the map coordinate axes' length unit (its MapUnitIsLength rule constrains it to LENGTHUNIT), and IFC4.3 states
    // NO default for its absence: an undeclared map unit leaves the projected CRS itself the authority, and every EPSG
    // projected axis publishes metre, so the absent case reads unity. The project IfcUnitAssignment regime is a
    // DIFFERENT axis and never applies to a map ordinate — the model-to-map step is IfcMapConversion.Scale's own
    // declared job, already folded into the composed axis scale before it reaches here. Building the frame as a
    // UnitScale value keeps every crossing on the folder's ONE Coerce/Declare pair with no bare multiplier at a site.
    static Fin<UnitScale> MapFrame(IfcCoordinateReferenceSystem? crs, Op key) =>
        (crs as IfcProjectedCRS)?.MapUnit is { } unit
            ? Positive(unit.SIFactor(), nameof(IfcNamedUnit.SIFactor), key).Map(static factor => UnitScale.Si with { L = factor })
            : Fin.Succ(UnitScale.Si);

    static double Metres(double native, UnitScale frame) => frame.Coerce(native, MeasureRow.Length, null);

    // The strictly-positive scale gate, held at the SAME grain the seam Admit holds it: a zero factor collapses the
    // frame and a negative one mirrors it, so a DECLARED non-positive or non-finite scale FAULTS by column name here
    // rather than coercing to unity where the seam's own gate can no longer see the value it was handed.
    static Fin<double> Positive(double value, string column, Op key) =>
        double.IsFinite(value) && value > 0.0
            ? Fin.Succ(value)
            : Fin.Fail<double>(new BimFault.ModelRejected(key, $"map-scale-degenerate:{column}:{value:R}"));

    // The identity band the level election compares against: RotationRadians is an Atan2 derivative and each scale a
    // product of unit factors, so an exact == 0.0 / == 1.0 reads a numerically-identity frame as rotated and promotes
    // its LoGeoRef level on every re-export.
    const double FrameEpsilon = 1e-12;

    static bool Rigidly(GeoReference reference) =>
        Math.Abs(reference.RotationRadians) <= FrameEpsilon
        && Math.Abs(reference.ScaleX - 1.0) <= FrameEpsilon
        && Math.Abs(reference.ScaleY - 1.0) <= FrameEpsilon
        && Math.Abs(reference.ScaleZ - 1.0) <= FrameEpsilon;

    static bool Isotropic(GeoReference reference) =>
        Math.Abs(reference.ScaleX - reference.ScaleY) <= FrameEpsilon && Math.Abs(reference.ScaleX - reference.ScaleZ) <= FrameEpsilon;

    // The level election off the frame's OWN columns — the discriminant the seam does not store, derived once here so
    // Author writes what the ingest recorded instead of flattening every non-site frame onto IfcMapConversion. The
    // Scaled row is gated on the TARGET release because GeometryGym writes IfcMapConversionScaled under the base
    // IfcMapConversion class name before IFC4X3_ADD2 (StepClassName, decompile-confirmed) — the three Factor columns
    // would vanish with no diagnostic, so an older target authors the shared component isotropically and REPORTS
    // Conversion, making the bounded anisotropy drop a level a caller reads rather than one it must infer.
    static GeoAuthored Level(DatabaseIfc db, GeoReference reference) =>
        reference == GeoReference.Identity ? GeoAuthored.Unreferenced
        : !Isotropic(reference) ? (db.Release >= ReleaseVersion.IFC4X3_ADD2 ? GeoAuthored.Scaled : GeoAuthored.Conversion)
        : !Rigidly(reference) ? GeoAuthored.Conversion
        : reference.Epsg == Some(4326) ? GeoAuthored.Geographic
        : GeoAuthored.Rigid;

    // The egress inverse [M1] the Projection/egress#IFC_EGRESS Emit composes beside ReauthorHeader — a LoGeoRef-50
    // model exporting geo-stripped was the named round-trip drop this closes, and a rigid model exporting as a full
    // map conversion is the level promotion the election closes beside it. Identity authors NOTHING. The two formerly
    // SILENT paths — a database with no IfcProject, an elected level whose anchor entity is absent — are typed
    // outcomes now, because a void return made "authored" and "wrote nothing" indistinguishable to the caller.
    // `model` is the PROJECT regime and reaches the site elevation ALONE: the authored IfcProjectedCRS declares no
    // MapUnit, so the authored map frame is metre by construction and the metre-normalized seam ordinates land
    // VERBATIM — an inverse map-unit fold would divide by a unit no authored CRS declares. The seam Epoch has no
    // IFC4X3 attribute and stays transform-side evidence only.
    public static Fin<GeoAuthored> Author(DatabaseIfc db, GeoReference reference, UnitScale model, Op key) {
        GeoAuthored level = Level(db, reference);
        if (level == GeoAuthored.Unreferenced) { return Fin.Succ(level); }
        if (db.Project is not IfcProject project) {
            return Fin.Fail<GeoAuthored>(new BimFault.ModelRejected(key, "geo-author-projectless"));
        }
        return level == GeoAuthored.Geographic
            ? Optional(project.Extract<IfcSite>().FirstOrDefault())
                .ToFin(new BimFault.DanglingReference(key, "geo-author-site-missing"))
                .Map(site => AuthorSite(site, reference, model))
            : Optional(project.RepresentationContexts.OfType<IfcGeometricRepresentationContext>().FirstOrDefault())
                .ToFin(new BimFault.DanglingReference(key, "geo-author-context-missing"))
                .Map(context => AuthorOperation(db, context, reference, level));
    }

    // The LoGeoRef-30 ingest arm's exact inverse: Eastings carried longitude, Northings latitude, both decimal degrees
    // through the IfcCompoundPlaneAngleMeasure(double) decimal-degree constructor (decompile-confirmed), and the
    // elevation — the one project-unit length on this page — declared back into the model's own regime.
    static GeoAuthored AuthorSite(IfcSite site, GeoReference reference, UnitScale model) {
        site.RefLongitude = new IfcCompoundPlaneAngleMeasure(reference.Eastings);
        site.RefLatitude = new IfcCompoundPlaneAngleMeasure(reference.Northings);
        site.RefElevation = model.Declare(reference.OrthogonalHeight, MeasureRow.Length, null);
        return GeoAuthored.Geographic;
    }

    // ONE IfcProjectedCRS (the densest carrier first: the EPSG authority name, else the carried Name; the WKT/
    // projection/zone/datum carriers re-stamped verbatim) and the elected operation entity on the model context. The
    // CRS deliberately declares NO MapUnit, so the authored map axes are metre and the seam metres are the map
    // ordinates verbatim — the exact inverse of the ingress MapFrame read.
    // Rigid authors IfcRigidOperation through its LENGTH-measured ctor (IfcCoordinateReferenceSystemSelect source,
    // IfcCoordinateReferenceSystem target, IfcLengthMeasure, IfcLengthMeasure, double — decompile-confirmed), the
    // exact shape the ingest guard reads back, and carries no scale channel because the frame is metre-identity.
    // Scaled splits the seam axes the way the schema composes them — Scale carries the shared unit component and the
    // three Factor columns the coordinate anisotropy — while Conversion writes the shared component alone, so a
    // pre-IFC4X3_ADD2 target (where GeometryGym emits the subtype under the base class name and the factors vanish)
    // lands the isotropic truth rather than a silently halved frame. GeometryGym's vendor ScaleY/ScaleZ setters are
    // NOT written: the schema carries no such attributes and a reader defaulting them to 1.0 would fork the frame.
    static GeoAuthored AuthorOperation(DatabaseIfc db, IfcGeometricRepresentationContext context, GeoReference reference, GeoAuthored level) {
        var crs = new IfcProjectedCRS(db, reference.Epsg.Match(
            Some: static code => $"EPSG:{code}",
            None: () => reference.Crs.Map(static c => c.Name).IfNone(""))) {
            GeodeticDatum = reference.GeodeticDatum,
            VerticalDatum = reference.VerticalDatum,
            MapProjection = reference.Crs.Map(static c => c.MapProjection).IfNone(""),
            MapZone = reference.Crs.Map(static c => c.MapZone).IfNone(""),
        };
        reference.Crs.Map(static c => c.Wkt).Filter(static w => w.Length > 0)
            .Iter(wkt => crs.WellKnownText = new IfcWellKnownText(wkt, crs));
        var (e, n, h) = (reference.Eastings, reference.Northings, reference.OrthogonalHeight);
        context.HasCoordinateOperation = level switch {
            _ when level == GeoAuthored.Rigid =>
                new IfcRigidOperation(context, crs, new IfcLengthMeasure(e), new IfcLengthMeasure(n), h),
            _ when level == GeoAuthored.Scaled =>
                new IfcMapConversionScaled(context, crs, e, n, h, reference.ScaleX, reference.ScaleY, reference.ScaleZ) {
                    XAxisAbscissa = reference.XAxisAbscissa, XAxisOrdinate = reference.XAxisOrdinate, Scale = 1.0,
                },
            _ => new IfcMapConversion(context, crs, e, n, h) {
                XAxisAbscissa = reference.XAxisAbscissa, XAxisOrdinate = reference.XAxisOrdinate, Scale = reference.ScaleX,
            },
        };
        return level;
    }
}
```

## [03]-[GEODETIC_TRANSFORM]

- Owner: `GeoTransform` the datum-bridging leg reprojecting raw ordinate spans between two seam `GeoReference` frames — EACH frame resolves its `ProjNET` `CoordinateSystem` off its OWN seam `CrsResolution` (`ManagedCs`: the `Wkt` arm the shared `CoordinateSystemFactory.CreateFromWkt` over the frame's own payload — a GIS-origin CRS with no authority code, where `Epsg` is `None` so an SRID-only build silently no-ops the federation — the `Epsg` arm the SAME parser over the `EpsgWkt` definition one OSR `ImportFromEPSGA`/`ExportToWkt` hop resolves, because `ProjNET` ships no code registry at all), the ONE `CoordinateTransformationFactory.CreateFromCoordinateSystems(src, dst)` building the managed transform for EVERY resolvable pair (both-EPSG, both-WKT, and the MIXED EPSG↔WKT federation alike) — escalating an exotic datum-grid or dynamic-datum transform `ProjNET` cannot express to the `MaxRev.Gdal.Core` OSR PROJ engine (keyed by `ImportFromEPSGA` or `ImportFromWkt` to match the frame's resolution) per the `.api/api-projnet` escalation-seam; `CsFactory` the one WKT parser, `TransformFactory` the one CS-pair build, and `ManagedFrames` the one `FrameKey`-keyed `CoordinateSystem` cache the `.api/api-projnet` `CRS_TRANSFORM` law names as the single owners. The leg operates on the seam `GeoReference` frame and a `ProjNET`/OSR datum shift folded onto the kernel transform is the named seam violation.
- Entry: `GeoTransform.Preflight(Seq<(string Model, GeoReference Frame)> frames, (double X, double Y, double Z) anchor, CancellationToken token, Op key)` folds a federation's frames into the TOTAL pairwise `FrameAlignment` matrix — one row per unordered INDEX pair (so two models sharing a name both appear, where an ordinal name compare dropped every such pair), one probe reprojection per distinct FRAME-IDENTITY pair memoized across the run (an N-model federation over M distinct frames builds M(M−1)/2 transforms, not N(N−1)/2), every outcome a typed `FrameVerdict` row (a leg fault lands `Unresolvable` with its cause, never a fold abort), the at-anchor displacement riding each row — the preflight artifact a coordination manager rules the federated join on before any element-level work runs. `GeoTransform.Reproject(GeoReference source, GeoReference target, Span<double> ordinates, int stride, Op key)` applies the datum-to-datum transform IN PLACE on the interleaved double ordinate buffer when both frames carry a resolvable CRS (EPSG or WKT) that differs, returning `Fin<Reprojection>` — the typed receipt carrying the engine route, the shifted-vertex count, the forward→inverse round-trip residual, the central-difference anchor `AnchorScale`/`AnchorConvergence` distortion evidence, and the dynamic-datum `EpochDefaulted` posture, each evidence column an `Option<double>` so a REFUSED probe is a recorded absence and never a fabricated unit scale or zero residual: the additive cases (a source or target `CrsResolution.Unreferenced`, an identical CRS, or fewer than one full vertex) return `Reprojection.Identity` — engine `Identity`, zero shifted vertices, and every evidence column `None`, because an identity leg PROBED nothing — so the datum leg never blocks a single-datum federation; a `CrsResolution.Projection` frame (the seam's typed projection+zone-only mode) faults `crs-projection-only-unbuildable` by CASE, and a pair whose two `VerticalCrs` height datums DIFFER faults `crs-vertical-untransformable` by name — no geoid model reaches either engine, so a horizontal-only shift that carried Z across a datum boundary would land a federation correct in plan and metres wrong in elevation — neither engine builds from a bare projection identity, and the empty-`Wkt` payload sniff is the deleted form; a differing, resolvable pair resolves EACH frame's `CoordinateSystem` through its `Resolution` generated total `Switch` (`ManagedCs`) into the ONE facade `CreateTransformation(src, dst)` managed build (a mixed EPSG↔WKT pair included), runs the strided batch once, escalates to the matching OSR build (`ImportFromEPSGA`/`ImportFromWkt`) when `ProjNET` cannot express the transform, and faults `BimFault.CapabilityMiss` BARE only when BOTH engines fail. The buffer is `double` end to end — a survey easting never narrows to `float` (a float32 round-trip drops sub-metre precision on a six-figure easting; the `Semantics/geospatial#GEOSPATIAL_SEAM` precision contract) — and the NTS `CoordinateSequence` flatten plus the `Geometry.Apply` write-back is the geospatial CONSUMER's marshalling, never this owner's, so the leg stays geometry-library-neutral over raw ordinates. Composed BEFORE the downstream host-bound rigid map-conversion offset so a federated model lands in the shared datum before its local-engineering placement applies.
- Auto: `Reproject` short-circuits when either frame is `CrsResolution.Unreferenced`, when the two CRS identities are equal (same EPSG, or same `Crs` value), or when the buffer holds fewer than one full vertex; otherwise EACH frame resolves its `CoordinateSystem` through its own `Resolution` generated total `Switch` (`ManagedCs` — the `Epsg` arm `CsFactory.CreateFromWkt(EpsgWkt(code))` over the PROJ-resolved definition, the `Wkt` arm `CsFactory.CreateFromWkt(wkt)` over the frame's own payload, both behind the `ManagedFrames` `FrameKey` cache, the `Unreferenced` arm unreachable here since the short-circuit already returned) and the ONE `TransformFactory.CreateFromCoordinateSystems(srcCS, dstCS).MathTransform` builds the managed transform — both-EPSG, both-WKT, and the mixed EPSG↔WKT pair through one build. The `ProjNET` build is lifted through `Try` so an EPSG code PROJ's own database does not carry, a WKT `ProjNET` cannot parse, or a datum `ProjNET` cannot express routes the OSR escalation rather than throwing across the boundary; the `ProjNET` apply is the `api-projnet#ENTRYPOINTS` strided `double` batch run DIRECTLY on the interleaved buffer IN PLACE — a single `MathTransform.Transform(ordinates, ordinates[1..], ordinates[2..], stride, stride, stride)` call over the three ordinate columns of that one `Span<double>`, no staging copy (the buffer is already `double`, so there is no widen/narrow and no `MemoryMarshal.Cast<float,double>` to misread the bytes) and the `TransformCore` `while (num < xs.Length)` walk drives the count off the full-length first column so the last vertex is covered, a `stride` above three leaving the non-position interleave columns untouched; the OSR escalation deinterleaves the position columns into pooled `double[]` x/y/z, runs the one `Semantics/geospatial#RASTER_INGEST` `GeoGdal.Bootstrap` idempotent guard (`GdalBase.ConfigureAll` + `Osr.UseExceptions`), builds two `SpatialReference` keyed to match each frame's resolution through the TOTAL four-arm `CrsResolution` `Switch` (`ImportFromEPSGA` for an EPSG frame, `ImportFromWkt` for a WKT frame, the projection-only and unreferenced arms unreachable and empty by construction, `OAMS_TRADITIONAL_GIS_ORDER` pinning lon/lat against the GDAL-3 axis swap) and one `CoordinateTransformation` under the two options gates (`SetBallparkAllowed(false)` — a gridless pair faults, never a coarse ballpark shift; `SetOnlyBest(true)` — a missing best-accuracy operation faults, never a silent lower-accuracy fallback), records either frame's `IsDynamic()` onto the receipt's `EpochDefaulted`, runs one `TransformPoints(count, xs, ys, zs)`, and reinterleaves; on BOTH engines the receipt evidence rides the same shifted anchor — the `GetInverse`/`Inverse()` round-trip residual and the `Distortion` central-difference Jacobian probe are inner-`Try` recorded absences (`None`), never leg faults; `Preflight` keys its probe memo on each frame's RESOLUTION IDENTITY (its EPSG code, its WKT text, or its projection identity) so a federation of many models on few frames pays one build per frame pair, and it checks the caller's `CancellationToken` at each PAIR boundary — the managed grain the `RULINGS` native-lane row demands stated honestly, because an in-flight `TransformPoints` batch and an OSR pipeline build publish no interrupt of their own — landing an abandoned pair as an `Unresolvable` ROW and forcing the whole fold with `Strict` before the `Seq` leaves, so an abandoned run is a matrix still TOTAL over the federation and never a short row set a gate reads as clean; the datum shift composes BEFORE the rigid offset so a model lands in the shared datum before its local-engineering-frame placement applies.
- Packages: ProjNET, MaxRev.Gdal.Core, Rasm.Element, Rasm, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new EPSG, WKT, or mixed CRS pair is the per-frame `ManagedCs` resolution joined by the one `TransformFactory.CreateFromCoordinateSystems` build (`EpsgWkt` resolving an EPSG frame's definition, `CsFactory` parsing every payload, `ManagedFrames` caching by `FrameKey`), never a per-call factory; a new CRS-resolution mode is one arm on the seam `CrsResolution` that breaks BOTH `Switch` sites at compile time (the seam owns the discriminant, this leg owns the per-mode build); an exotic datum-grid or dynamic datum is the OSR PROJ pipeline's, resolved from the EPSG code or the WKT, never a hand-rolled Bursa-Wolf matrix; a float-buffered consumer widens to `double` at its OWN boundary and calls the one `Span<double>` leg, never a parallel `Span<float>` overload re-admitting the survey-precision-loss footgun; a denser batch is one `MathTransform`/`CoordinateTransformation` overload swap, never a second transform owner and never a per-vertex `ref` loop; a new PROJ pipeline gate is one `CoordinateTransformationOptions` setter row on the one OSR options build, never a second pipeline owner; the coordinate epoch is the seam's — the `GeoReference.Epoch` decimal-year column the OSR leg threads through `SpatialReference.SetCoordinateEpoch` per frame, never a Bim-local epoch knob (an epoch-LESS dynamic frame records `EpochDefaulted`); a new receipt evidence column is one `Option<double>` `Reprojection` field fed by the shared anchor probes, never a per-engine receipt sibling; a new alignment verdict is one `FrameVerdict` case every matrix consumer's `Switch` breaks on at compile time, never a parallel per-consumer compatibility test.
- Boundary: the datum reprojection is `ProjNET`'s by default — the per-frame `ManagedCs` `CoordinateSystem` resolution (each frame's seam `CrsResolution` selecting `CsFactory.CreateFromWkt` over `EpsgWkt(code)` or over the frame's own payload, cached by `FrameKey`) joined by the ONE `TransformFactory.CreateFromCoordinateSystems(src, dst)` build, and the `MathTransform` Bursa-Wolf 7-parameter datum shift plus projection own the managed transform — escalating to the `MaxRev.Gdal.Core` OSR `SpatialReference`/`CoordinateTransformation` PROJ pipeline (`ImportFromEPSGA` or `ImportFromWkt` matching the frame's resolution, the full datum-grid set under `SetBallparkAllowed(false)` + `SetOnlyBest(true)`, a dynamic frame with no seam `Epoch` recorded onto `EpochDefaulted` — an epoch-bearing frame threads `SetCoordinateEpoch(frame.Epoch)`, never a Bim-local epoch) for what the managed algebra cannot express, and a hand-rolled datum shift, a per-CALL `new CoordinateTransformationFactory()`/`CoordinateTransformation` rebuild outside the shared owners (`CsFactory`, `TransformFactory`, `ManagedFrames`), or OSR for a transform `ProjNET` already covers is the deleted form per the `.api/api-projnet` single-cache-owner + escalation-seam law — and `CoordinateSystemServices` is that deleted form's own vehicle here, since its EPSG registry is the two-code `DefaultInitialization` pair and every other SRID answers `null`, so an `Epsg` frame routed through it escalates a managed-expressible transform to OSR behind a swallowed null; branching the build off a re-spelled `Epsg.IsSome` check (the COLLAPSE_SCAN re-branch the seam forbids) rather than the seam `CrsResolution` `Switch` is the deleted form, reading only `source.Epsg`/`target.Epsg` so a WKT-only federation silently no-ops (both `Epsg` `None`) is the named defect this leg closes, and a source-only build branch that escalates a MIXED EPSG↔WKT pair to OSR when the per-frame `ManagedCs` + facade build already expresses it is the same deleted form; every `CrsResolution` `Switch` on this page supplies ALL FOUR arms in one return shape — a partial arm set compiles against a generated total dispatch only by accident of overload resolution and a mixed `void`/value arm set is the named defect, the unreachable arms staying present and empty with their unreachability named; the `ProjNET` apply is the strided `double` batch run in place on the `Span<double>` and a per-vertex `Transform(ref x, ref y, ref z)` loop OR narrowing the survey ordinates to `float` (the precision-loss defect the geospatial seam forbids) is the rejected form; the GDAL bootstrap is the one `Semantics/geospatial#RASTER_INGEST` `GeoGdal.Bootstrap` idempotent guard and a second `GdalBase.ConfigureAll` owner is the deleted form; the leg is additive — a frame's `CrsResolution.Unreferenced` or an identical CRS returns `Reprojection.Identity` so `Reproject` never blocks ingest — and faults `BimFault.CapabilityMiss` BARE only on a malformed buffer, a projection+zone-only frame (`crs-projection-only-unbuildable` — named BEFORE two doomed engine builds, since neither `CreateFromWkt` nor `ImportFromWkt` builds from a bare projection identity), an out-of-domain vertex, or a differing resolvable pair that defeats both engines (the `Op key` carrying the operation context, never a `.ToError()` hop); reading `MathTransform.Derivative`/`GetDomainFlags` or `ICoordinateTransformation.AreaOfUse` for the distortion evidence or the domain guard is the phantom form (base-only `NotImplementedException`, factory-empty `AreaOfUse` — decompile-verified) — the `Distortion` central-difference anchor probe and the engine-agnostic non-finite scan are the honest owners, and a receipt asserting scale/convergence evidence it never probed is the illusory form this receipt closes, so `Reprojection.Identity` publishes `None` on every evidence column rather than the fabricated unit scale and zero residual an identity leg never measured; the pairwise matrix keys on the frame INDEX pair and an ordinal MODEL-NAME compare is the deleted form that silently dropped every same-named pair from a federation the matrix claims to be total over; the abort grain is DECLARED — the token gates the pair boundary and a single in-flight batch or pipeline build runs to completion, so an unqualified cancellable claim over the native call is the overclaim the `RULINGS` native-lane row names — and the abandoned pair lands a typed `Unresolvable` ROW over a `Strict`-forced fold, a lazily-yielded `where`-filtered matrix that shortens under cancellation being the deleted form that hands a gate a partial join set indistinguishable from a complete one; the reprojection composes BEFORE the downstream host-bound rigid map-conversion offset so the kernel transform stays datum-free, and folding the rigid offset into this datum leg is the named defect; the page reprojects raw `Span<double>` ordinate buffers — the NTS `CoordinateSequence` flatten and the `Geometry.Apply` write-back are the geospatial CONSUMER's marshalling, so a `GeoTransform` overload binding an NTS `Geometry`/`CoordinateSequence` is the misplaced-concern form — and a RhinoCommon geometry type crossing this leg is the host-bound defect.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Buffers;
using System.Collections.Concurrent;
using LanguageExt.UnsafeValueAccess;
using OSGeo.OSR;
using ProjNet.CoordinateSystems;                    // CoordinateSystemFactory + the CoordinateSystem currency ManagedCs resolves per frame
using ProjNet.CoordinateSystems.Transformations;    // CoordinateTransformationFactory + the MathTransform the batch runs on

// --- [TYPES] -------------------------------------------------------------------------------
// The engine a reprojection took — the receipt route the .api/algorithms receipt law records so a federation reads
// WHICH datum engine reconciled a frame pair (a survey audit distinguishes a managed ProjNET planar shift from a
// PROJ grid-shifted OSR escalation): Identity (no shift — an unreferenced/equal frame), Managed (the ProjNET
// MathTransform), Escalated (the GDAL OSR PROJ pipeline). Keyed for telemetry, never a bool.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GeoEngine {
    public static readonly GeoEngine Identity  = new("identity");
    public static readonly GeoEngine Managed   = new("managed");
    public static readonly GeoEngine Escalated = new("escalated");
}

// The VERTICAL reference a frame declares — the height datum its OrthogonalHeight is measured against, resolved from
// the seam GeoReference.VerticalDatum declaration into the SAME three states the horizontal ProjectedCrs resolves its
// authority name into: an EPSG-coded datum (the OGC URN, the authority form, or a bare numeric code), a NAMED datum no
// code resolves ("NAVD88", "Ordnance Datum Newlyn"), or nothing declared. Height is exactly as identity-bearing as
// plan position — two frames sharing one horizontal CRS and declaring different height datums are metres apart in Z —
// so the datum leg must be able to ASK whether two elevations are comparable rather than assume they are.
// The code resolution COMPOSES the seam's own authority parse (ProjectedCrs.EpsgOf, the static its instance Epsg
// property already runs) rather than re-spelling the three-form grammar: one grammar, one owner, both axes.
// Name compares ORDINAL by construction — a string member's generated equality is EqualityComparer<string>.Default,
// which IS the ordinal comparison — so two spellings of one datum stay two values, exactly as the horizontal side
// treats two spellings of one CRS name.
[ComplexValueObject]
public sealed partial class VerticalCrs {
    public Option<int> Epsg { get; }
    public string Name { get; }

    // TOTAL over the declaration: a blank one is None (a frame that stated no height datum) and a stated one always
    // lands — with its code when the name carries one, named-only otherwise — because an unrecognised height datum is
    // a fact the refusal arm reads, never a reason to reject a model at ingest.
    public static Option<VerticalCrs> Of(string verticalDatum) =>
        verticalDatum.Trim() is { Length: > 0 } declared
            ? Some(Create(ProjectedCrs.EpsgOf(declared), declared))
            : Option<VerticalCrs>.None;
}

// --- [MODELS] ------------------------------------------------------------------------------
// The typed datum-leg receipt (never a bare Unit): the engine route, the shifted-vertex count, the forward->inverse
// round-trip residual in the frame's native ordinate unit, the central-difference anchor Jacobian distortion pair, and
// the dynamic-datum epoch posture — the evidence a survey-grade federation validates its rigid placement against.
// EVERY probe column is Option<double>: a refused, non-invertible, or non-finite probe is a RECORDED ABSENCE, and the
// Identity row (an unreferenced or equal-frame leg) publishes None on all three because it measured nothing — the
// fabricated (0.0 residual, 1.0 scale, 0.0 convergence) triple read as a probed identity to every consumer.
// RoundTripResidual rides ProjNET MathTransform.Inverse() (21 concrete overrides, decompile-verified) or the OSR
// CoordinateTransformation.GetInverse() reverse pipeline on a probe vertex.
// AnchorScale = sqrt(|det J|) of the Distortion probe's 2x2 anchor Jacobian (the local areal-scale root — the survey
// point-scale-distortion evidence for a like-unit projected pair, a unit-ratio across mixed-unit frames);
// AnchorConvergence = atan2(dYdx, dXdx) (the local grid rotation of the transformed source x-axis — the
// meridian-convergence evidence at a federation origin).
// The Jacobian is PROBED because the package surfaces are phantoms (decompile-verified): MathTransform.Derivative/
// GetDomainFlags/GetCodomainConvexHull throw NotImplementedException with ZERO concrete overrides, and every
// factory-built ICoordinateTransformation.AreaOfUse is string.Empty — so the domain guard is the engine-agnostic
// post-transform non-finite scan and the distortion evidence is a central-difference probe, never a phantom call.
// EpochDefaulted: an OSR frame reports SpatialReference.IsDynamic() while ITS seam GeoReference.Epoch is None —
// that frame's shift is reference-epoch-defaulted, the plate-motion term unmodelled; an epoch-bearing dynamic frame
// threads SetCoordinateEpoch and reports false. False on a static OSR pair AND on the managed route, whose planar
// algebra carries no epoch model (the epoch question rides the escalated route's receipt only).
public readonly record struct Reprojection(
    GeoEngine Engine, int ShiftedVertices, Option<double> RoundTripResidual,
    Option<double> AnchorScale, Option<double> AnchorConvergence, bool EpochDefaulted) {
    public static readonly Reprojection Identity = new(GeoEngine.Identity, 0, None, None, None, false);
}

// The per-pair frame-compatibility verdict the federation preflight yields — the receipt evidence Reproject already
// mints, organized as the alignment matrix: Identical (one frame — no transform builds), Transformable (the datum
// leg reconciles the pair, its Reprojection the residual/distortion evidence), EpochMismatched (the pair reconciles
// but a dynamic-datum frame's plate-motion term is reference-epoch-defaulted — survey-grade review required), and
// Unresolvable (an unreferenced endpoint, a projection-only frame, or a pair both engines fail — the cause named).
[Union]
public abstract partial record FrameVerdict {
    private FrameVerdict() { }
    public sealed record Identical : FrameVerdict;
    public sealed record Transformable(Reprojection Evidence) : FrameVerdict;
    public sealed record EpochMismatched(Reprojection Evidence) : FrameVerdict;
    public sealed record Unresolvable(string Cause) : FrameVerdict;
}

// One alignment row per unordered model INDEX pair (never a name pair — two models may share a name and the matrix
// is total over the federation): the two model names, the typed verdict, and the anchor displacement — the "model C
// is on a different datum, 0.4 m apart at site anchor" magnitude a coordination manager reads first. The shift is
// Option-valued for the same reason the receipt columns are: an Unresolvable pair measured no displacement.
public readonly record struct FrameAlignment(string SourceModel, string TargetModel, FrameVerdict Verdict, Option<double> AnchorShift);

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class GeoTransform {
    static readonly CoordinateSystemFactory CsFactory = new();                   // the one shared WKT->CoordinateSystem parser
    static readonly CoordinateTransformationFactory TransformFactory = new();    // the one shared CS-pair transformation build
    static readonly ConcurrentDictionary<string, CoordinateSystem> ManagedFrames = new(StringComparer.Ordinal);  // the CS cache, keyed by FrameKey

    // The datum leg over two seam GeoReferences: a source-to-target reprojection of an interleaved DOUBLE-precision
    // ordinate Span<double> IN PLACE — survey eastings/northings never narrow to float (a float32 round-trip loses
    // sub-metre precision on a 500_000 m easting), the Semantics/geospatial#GEOSPATIAL_SEAM GeoFeature.Reproject
    // consumer flattening its NTS CoordinateSequence into the double buffer and writing the shifted ordinates back
    // through Geometry.Apply on its side. The build path is the per-frame CrsResolution case — the EPSG-keyed SRID
    // facade OR the WKT-keyed CreateFromWkt build (a GIS-origin WKT CRS carries no EPSG, so reading only Epsg silently
    // no-ops the federation; branching the seam CrsResolution Switch closes that). Additive — a frame's Unreferenced,
    // an identical CRS, or zero whole vertices returns Reprojection.Identity. ProjNET is the default managed engine; an
    // exotic datum-grid/dynamic-datum transform ProjNET cannot express escalates to the resolution-keyed GDAL OSR
    // build. Faults BimFault.CapabilityMiss bare on a malformed buffer (stride < 3), a projection+zone-only frame
    // (the typed CrsResolution.Projection case — neither engine builds from a bare projection identity), an
    // out-of-domain vertex (a non-finite shifted ordinate — the engine-agnostic domain guard), or a differing
    // resolvable pair both engines fail. One call is ONE native batch and publishes no interrupt of its own — the
    // cancellation grain is the PAIR boundary Preflight owns.
    public static Fin<Reprojection> Reproject(GeoReference source, GeoReference target, Span<double> ordinates, int stride, Op key) {
        // Malformed-buffer guard: a stride below the three position columns would misread the interleave, and a
        // RAGGED length (not a whole number of stride blocks) would drive the full-length xs walk one partial
        // vertex past the ys/zs slices — an IndexOutOfRange escaping the rail through the un-lifted batch call.
        if (stride < 3 || ordinates.Length % stride != 0) {
            return Fin.Fail<Reprojection>(new BimFault.CapabilityMiss(key, $"crs-buffer-malformed:stride:{stride}:length:{ordinates.Length}"));
        }
        // Additive short-circuit: an unreferenced endpoint, an identical CRS, or zero whole vertices is a no-op.
        // Sameness folds the EPSG (two names resolving the same code are one frame, so `EPSG:25832` and the URN form do
        // not build a redundant identity transform) THEN the Crs value-object (two WKT frames structurally equal under
        // the seam ProjectedCrs comparer policy).
        bool sameFrame =
            (from s in source.Epsg from t in target.Epsg select s == t).IfNone(false) || source.Crs == target.Crs;
        if (source.Resolution == CrsResolution.Unreferenced || target.Resolution == CrsResolution.Unreferenced
            || sameFrame || ordinates.Length < stride) {
            return Fin.Succ(Reprojection.Identity);
        }
        // The seam's TYPED projection-only state — CrsResolution.Projection, a MapProjection+MapZone identity with no
        // WKT payload — is admissible on the seam yet BUILDABLE by neither engine (CreateFromWkt and ImportFromWkt
        // both need the WKT text): fault it by CASE before two doomed engine builds, so the federation audit reads
        // the real gap (a WKT synthesis from the projection identity is unbuilt), never a conflated
        // crs-pair-unreconcilable — the seam mode replaced the empty-Wkt payload sniff this leg formerly ran.
        if ((ProjectionOnly(source) | ProjectionOnly(target)).Case is string gap) {
            return Fin.Fail<Reprojection>(new BimFault.CapabilityMiss(key, $"crs-projection-only-unbuildable:{gap}"));
        }
        // The VERTICAL leg is REFUSED, never faked. Neither engine is handed a geoid model here: the managed ProjNET
        // transform is planar-plus-datum with no height correction at all, and the OSR pipeline builds from the two
        // frames' HORIZONTAL identities, so its Z column carries through whatever height it was given. Two frames
        // declaring DIFFERENT height datums are therefore not reconcilable by this leg — and shifting X/Y while
        // silently carrying Z across a datum boundary lands a model correct in plan and metres wrong in elevation,
        // which is precisely the failure a survey federation runs this preflight to catch. It faults by NAME so the
        // audit reads the unbuilt capability (a geoid-model leg) rather than a generic unreconcilable pair. Two frames
        // on ONE declared datum, or either declaring none, ride through with Z untouched — the truth the file states.
        if (VerticalGap(source, target).Case is string vertical) {
            return Fin.Fail<Reprojection>(new BimFault.CapabilityMiss(key, $"crs-vertical-untransformable:{vertical}"));
        }
        // The managed build: EACH frame resolves its CoordinateSystem off its OWN seam CrsResolution Switch
        // (ManagedCs), then the ONE shared CreateFromCoordinateSystems(src, dst) builds the transform — so a MIXED
        // EPSG<->WKT federation builds MANAGED (OSR for a transform ProjNET already covers is this page's own
        // deleted form; the retired source-only two-arm branch escalated every mixed pair). Lifted through Try so
        // an EPSG PROJ's database does not carry, a WKT ProjNET cannot parse, or a datum it cannot express lifts onto
        // the rail (no throw crossing the boundary) and the matching OSR escalation runs.
        MathTransform? managed = Try.lift(() =>
                from src in ManagedCs(source)
                from dst in ManagedCs(target)
                select TransformFactory.CreateFromCoordinateSystems(src, dst).MathTransform)
            .Run().Match(Succ: static t => t.ValueUnsafe(), Fail: static _ => (MathTransform?)null);
        if (managed is null) {
            return Osr(source, target, ordinates, stride, key);
        }
        // The api-projnet#ENTRYPOINTS dense rail: ONE strided batch over the three ordinate columns of the
        // one interleaved buffer IN PLACE (xs=ordinates@0, ys=ordinates[1..]@1, zs=ordinates[2..]@2, stride each), no
        // staging copy and no per-vertex Transform(ref x,ref y,ref z) loop. Capture the first vertex FIRST for the
        // round-trip self-check, then TransformCore walks num<xs.Length so the last vertex is covered; stride>3 skips
        // any normal/uv interleave columns. Survey ordinates stay double end to end.
        int count = ordinates.Length / stride;
        var (ox, oy, oz) = (ordinates[0], ordinates[1], ordinates[2]);
        managed.Transform(ordinates, ordinates[1..], ordinates[2..], stride, stride, stride);
        // Anchor evidence off the SAME managed engine that shifted the buffer: the Distortion central-difference probe
        // + the Inverse round-trip. ProjNET's planar algebra carries no epoch model, so the managed route records
        // EpochDefaulted: false by construction — the epoch posture is the OSR escalation's PROJ report.
        var (scale, convergence) = Distortion((x, y) => { var (px, py, _) = managed.Transform(x, y, oz); return (px, py); }, ox, oy);
        return AllFinite(ordinates, stride, count)
            ? Fin.Succ(new Reprojection(GeoEngine.Managed, count, RoundTrip(managed, ordinates, ox, oy, oz), scale, convergence, EpochDefaulted: false))
            : Fin.Fail<Reprojection>(new BimFault.CapabilityMiss(key, $"crs-out-of-domain:{source.Resolution.Key}->{target.Resolution.Key}"));
    }

    // The projection+zone-only detector — the seam's TYPED CrsResolution.Projection case (never an empty-Wkt payload
    // sniff, the deleted form); the gap string names the projection identity for the fault detail.
    // The height-datum gap: BOTH frames must declare a vertical reference for a difference to exist at all, and two
    // equal declarations are one datum. The comparison is the value-object's own — a coded datum matches a coded one
    // by code, a named datum by its ordinal name — so a URN and its authority-form spelling of one datum do not read
    // as a gap while two genuinely different datums always do.
    static Option<string> VerticalGap(GeoReference source, GeoReference target) =>
        from s in VerticalCrs.Of(source.VerticalDatum)
        from t in VerticalCrs.Of(target.VerticalDatum)
        from gap in s == t ? Option<string>.None : Some($"{s.Name}->{t.Name}")
        select gap;

    static Option<string> ProjectionOnly(GeoReference frame) =>
        frame.Resolution == CrsResolution.Projection
            ? frame.Crs.Map(static c => $"{c.MapProjection}:{c.MapZone}")
            : Option<string>.None;

    // Per-frame managed CS resolution keyed by the seam CrsResolution (the seam owns the discriminant, this leg owns
    // the per-mode build — never a re-spelled Epsg.IsSome re-branch): BOTH arms end at the one CreateFromWkt parser
    // behind the FrameKey-keyed cache, the Epsg arm resolving its definition through EpsgWkt first, the Wkt arm
    // handing its own payload straight in (always non-empty — the seam mode guarantees it); Projection unreachable
    // (ProjectionOnly already faulted the case by name), Unreferenced unreachable (the short-circuit returned). All
    // four arms present, one return shape.
    static Option<CoordinateSystem> ManagedCs(GeoReference frame) =>
        frame.Resolution.Switch(
            epsg: () => frame.Epsg.Map(code => ManagedFrames.GetOrAdd($"epsg:{code}", _ => CsFactory.CreateFromWkt(EpsgWkt(code)))),
            wkt: () => frame.Crs.Map(static c => ManagedFrames.GetOrAdd($"wkt:{c.Wkt}", static _ => CsFactory.CreateFromWkt(c.Wkt))),
            projection: static () => Option<CoordinateSystem>.None,
            unreferenced: static () => Option<CoordinateSystem>.None);

    // The EPSG definition source. ProjNET ships NO code registry — CoordinateSystemServices seeds exactly EPSG:4326
    // and 3857 from its private DefaultInitialization and answers null for every other SRID, so routing EPSG frames
    // through that facade escalated every real projected federation to OSR under a swallowed null, which is this
    // page's own named deleted form (OSR for a transform ProjNET already covers). PROJ's EPSG database is already
    // bound here for the escalation leg, so ONE ImportFromEPSGA/ExportToWkt hop per code supplies the WKT the managed
    // parser builds from, cached by FrameKey — the transform itself still runs on the managed strided algebra, which
    // is the whole reason ProjNET is the default engine. Under Osr.UseExceptions a code PROJ does not carry throws
    // into the enclosing Try and takes the OSR escalation, exactly as an unparseable WKT does.
    static string EpsgWkt(int code) {
        GeoGdal.Bootstrap();
        using var crs = new SpatialReference("");
        // The A-form is the axis-order-AUTHORITATIVE import: it builds the CRS with the axis order the EPSG
        // registry declares (lat/lon on a geographic frame), which the explicit OAMS_TRADITIONAL_GIS_ORDER pin
        // then normalizes to lon/lat deterministically. The bare form pre-swaps to traditional order itself, so
        // the pin becomes a redundant second opinion and a frame whose registry order genuinely matters is
        // already flattened before this owner can state its own convention.
        crs.ImportFromEPSGA(code);
        crs.ExportToWkt(out string wkt, []);
        return wkt;
    }

    // The exotic datum escalation: GDAL OSR carries PROJ's full datum-grid + dynamic-datum pipeline ProjNET's managed
    // algebra cannot. TWO CoordinateTransformationOptions gates (both decompile-verified): SetBallparkAllowed(false) —
    // a low-accuracy ballpark shift (no PROJ grid for the pair) FAULTS rather than silently returning a coarse survey
    // result — and SetOnlyBest(true) — a best-accuracy operation whose grid is uninstantiable FAULTS rather than
    // silently degrading to the next-best pipeline. IsDynamic() on either frame records EpochDefaulted: the seam frame
    // carries no coordinate epoch yet, so a dynamic-datum shift is reference-epoch-defaulted — receipt evidence, never
    // a block. OSR's TransformPoints takes struct-of-arrays double columns, so the interleaved buffer deinterleaves
    // into pooled double x/y/z, transforms, and reinterleaves (no float anywhere). Each SpatialReference is keyed to
    // MATCH its frame's resolution — ImportFromEPSGA for an EPSG frame, ImportFromWkt for a WKT frame. The build lifts
    // through Try so a missing RID runtime, an EPSG no PROJ grid covers, or an unparseable WKT surfaces as
    // BimFault.CapabilityMiss; a non-finite shifted ordinate is the out-of-domain fault; the GetInverse round-trip and
    // the Distortion probe are INNER Try recorded absences (None) — a non-invertible or probe-refusing pipeline never
    // fails a leg whose forward shift succeeded.
    static Fin<Reprojection> Osr(GeoReference source, GeoReference target, Span<double> ordinates, int stride, Op key) {
        int count = ordinates.Length / stride;
        double[] xs = ArrayPool<double>.Shared.Rent(count);
        double[] ys = ArrayPool<double>.Shared.Rent(count);
        double[] zs = ArrayPool<double>.Shared.Rent(count);
        try {
            for (int i = 0, o = 0; i < count; i++, o += stride) {
                (xs[i], ys[i], zs[i]) = (ordinates[o], ordinates[o + 1], ordinates[o + 2]);
            }
            var (ox, oy, oz) = (xs[0], ys[0], zs[0]);
            Fin<(Option<double> RoundTrip, Option<double> Scale, Option<double> Convergence, bool EpochDefaulted)> outcome = Try.lift(() => {
                GeoGdal.Bootstrap();
                using SpatialReference src = Crs(source);
                using SpatialReference dst = Crs(target);
                using var options = new CoordinateTransformationOptions();
                options.SetBallparkAllowed(false);                              // a gridless survey pair FAULTS, never a coarse ballpark shift
                options.SetOnlyBest(true);                                      // a missing best-accuracy grid FAULTS, never a silent degradation
                // A dynamic frame WITHOUT a seam epoch is reference-epoch-defaulted; an epoch-bearing frame threaded
                // SetCoordinateEpoch inside Crs(frame), so its plate-motion term is modelled and reports false.
                bool epochDefaulted = (src.IsDynamic() && source.Epoch.IsNone) || (dst.IsDynamic() && target.Epoch.IsNone);
                using var pipeline = new CoordinateTransformation(src, dst, options);
                pipeline.TransformPoints(count, xs, ys, zs);
                // SEPARATE probe arrays through the reverse pipeline (GetInverse, decompile-verified) so the forward
                // result the reinterleave reads stays intact; a throwing inverse records None through the inner Try.
                Option<double> roundTrip = Try.lift(() => {
                    double[] rx = [xs[0]], ry = [ys[0]], rz = [zs[0]];
                    using CoordinateTransformation inverse = pipeline.GetInverse();
                    inverse.TransformPoints(1, rx, ry, rz);
                    return Hypot(rx[0] - ox, ry[0] - oy, rz[0] - oz);
                }).Run().Match(Succ: Some, Fail: static _ => Option<double>.None);
                var (scale, convergence) = Distortion((x, y) => { double[] p = [x, y, oz]; pipeline.TransformPoint(p); return (p[0], p[1]); }, ox, oy);
                return (roundTrip, scale, convergence, epochDefaulted);
            }).Run();
            bool outOfDomain = outcome.IsSucc && !AllFinite(xs, ys, zs, count);
            if (outcome.IsFail || outOfDomain) {
                return Fin.Fail<Reprojection>(new BimFault.CapabilityMiss(key,
                    outOfDomain ? $"crs-out-of-domain:{source.Resolution.Key}->{target.Resolution.Key}"
                                : $"crs-pair-unreconcilable:{source.Resolution.Key}->{target.Resolution.Key}"));
            }
            for (int i = 0, o = 0; i < count; i++, o += stride) {
                (ordinates[o], ordinates[o + 1], ordinates[o + 2]) = (xs[i], ys[i], zs[i]);
            }
            return outcome.Map(o => new Reprojection(GeoEngine.Escalated, count, o.RoundTrip, o.Scale, o.Convergence, o.EpochDefaulted));
        } finally {
            ArrayPool<double>.Shared.Return(xs);
            ArrayPool<double>.Shared.Return(ys);
            ArrayPool<double>.Shared.Return(zs);
        }
    }

    // The forward->inverse round-trip residual on the FIRST vertex: MathTransform.Inverse() (an abstract member all 21
    // concrete ProjNET transforms override, decompile-verified — never the base NotImplementedException) reverses the
    // shifted image, the residual to the captured source vertex the precision self-check. A non-invertible concatenated
    // pipeline whose Inverse throws records None (a recorded absence, never a fabricated 0), lifted through Try.
    static Option<double> RoundTrip(MathTransform forward, ReadOnlySpan<double> shifted, double ox, double oy, double oz) {
        (double sx, double sy, double sz) = (shifted[0], shifted[1], shifted[2]);
        return Try.lift(() => {
            (double x, double y, double z) = (sx, sy, sz);
            forward.Inverse().Transform(ref x, ref y, ref z);
            return Hypot(x - ox, y - oy, z - oz);
        }).Run().Match(Succ: Some, Fail: static _ => Option<double>.None);
    }

    // The anchor distortion probe — the honest replacement for the phantom MathTransform.Derivative/GetDomainFlags
    // (base NotImplementedException, ZERO concrete overrides; every factory-built ICoordinateTransformation.AreaOfUse
    // string.Empty — decompile-verified): a central-difference 2x2 Jacobian at the SOURCE anchor over the SAME engine
    // that shifted the buffer, engine-supplied as the four-probe map closure. AnchorScale = sqrt(|det J|), the local
    // areal-scale root; AnchorConvergence = atan2(dYdx, dXdx), the transformed source x-axis rotation. The step h
    // scales off the anchor magnitude (a degree-domain geographic source probes ~1e-4 deg, a six-figure easting
    // ~0.5 m — both well inside the slowly-varying distortion field); a refused or non-finite probe records
    // (None, None) through the Try — evidence absence, never a leg fault and never a fabricated unit scale.
    static (Option<double> Scale, Option<double> Convergence) Distortion(Func<double, double, (double X, double Y)> map, double ox, double oy) =>
        Try.lift(() => {
            double h = Math.Max(Math.Max(Math.Abs(ox), Math.Abs(oy)), 1.0) * 1e-6;
            var ((xe, ye), (xw, yw), (xn, yn), (xs, ys)) = (map(ox + h, oy), map(ox - h, oy), map(ox, oy + h), map(ox, oy - h));
            var (dXdx, dYdx, dXdy, dYdy) = ((xe - xw) / (2.0 * h), (ye - yw) / (2.0 * h), (xn - xs) / (2.0 * h), (yn - ys) / (2.0 * h));
            double det = dXdx * dYdy - dXdy * dYdx;
            return double.IsFinite(det) && det != 0.0
                ? (Scale: Some(Math.Sqrt(Math.Abs(det))), Convergence: Some(Math.Atan2(dYdx, dXdx)))
                : (Scale: Option<double>.None, Convergence: Option<double>.None);
        }).Run().IfFail((Option<double>.None, Option<double>.None));

    // The engine-agnostic domain guard: an out-of-domain reprojection emits a non-finite ordinate (ProjNET NaN, PROJ
    // inf) rather than silent garbage, so every position column is finiteness-scanned before the shift is trusted —
    // the honest replacement for ProjNET's GetDomainFlags (a phantom: NotImplementedException on every transform).
    // The interleaved overload scans the managed buffer's three position columns; the SoA overload scans the OSR
    // deinterleaved x/y/z pools.
    static bool AllFinite(ReadOnlySpan<double> ordinates, int stride, int count) {
        for (int i = 0, o = 0; i < count; i++, o += stride) {
            if (!double.IsFinite(ordinates[o]) || !double.IsFinite(ordinates[o + 1]) || !double.IsFinite(ordinates[o + 2])) { return false; }
        }
        return true;
    }

    static bool AllFinite(ReadOnlySpan<double> xs, ReadOnlySpan<double> ys, ReadOnlySpan<double> zs, int count) {
        for (int i = 0; i < count; i++) {
            if (!double.IsFinite(xs[i]) || !double.IsFinite(ys[i]) || !double.IsFinite(zs[i])) { return false; }
        }
        return true;
    }

    // The BCL double.Hypot is the overflow- and underflow-safe two-argument magnitude; the 3-D residual composes it
    // rather than summing squares, because a six-figure easting difference squared is where the naive form loses bits.
    static double Hypot(double dx, double dy, double dz) => double.Hypot(double.Hypot(dx, dy), dz);

    // A SpatialReference keyed to MATCH the frame's seam CrsResolution: ImportFromEPSGA off the parsed Epsg, else
    // ImportFromWkt off the inline Wkt (OSR's ImportFromWkt takes the WKT by ref). All FOUR arms are supplied in one
    // Action shape — the projection-only case faulted by name before any build and the unreferenced case returned at
    // the short-circuit, so both arms stay present and empty rather than absent. OAMS_TRADITIONAL_GIS_ORDER pins
    // lon/lat order against the GDAL-3 axis swap; Osr.UseExceptions (set by the one GeoGdal.Bootstrap guard) makes a
    // failed import throw into the enclosing Try rather than return a code. The seam Epoch threads
    // SetCoordinateEpoch (decompile-verified) so a dynamic-datum frame's plate-motion term is modelled per frame.
    static SpatialReference Crs(GeoReference frame) {
        var crs = new SpatialReference("");
        frame.Resolution.Switch(
            epsg: () => { crs.ImportFromEPSGA(frame.Epsg.IfNone(0)); },
            wkt: () => { string wkt = frame.Crs.Map(static c => c.Wkt).IfNone(""); crs.ImportFromWkt(ref wkt); },
            projection: static () => { },
            unreferenced: static () => { });
        crs.SetAxisMappingStrategy(AxisMappingStrategy.OAMS_TRADITIONAL_GIS_ORDER);
        frame.Epoch.IfSome(epoch => crs.SetCoordinateEpoch(epoch));
        return crs;
    }

    // The federation ALIGNMENT preflight: N model frames folded into the pairwise compatibility matrix BEFORE any
    // element-level join — one row per unordered INDEX pair (an ordinal MODEL-NAME compare dropped every same-named
    // pair, so the matrix it produced was not total over the federation it claimed to cover), each Reproject outcome
    // organized as the typed FrameVerdict (a leg fault becomes an Unresolvable ROW, never a fold abort). The probe
    // memo keys on the pair of frame RESOLUTION IDENTITIES, so a twenty-model federation on three frames builds three
    // transforms rather than one hundred and ninety. Cancellation is checked at the PAIR boundary — the one managed
    // grain this leg owns, since a single in-flight batch or OSR pipeline build publishes no interrupt — so an
    // abandoned preflight stops between pairs and never mid-transform.
    // Cancellation TYPES its own rows rather than shortening the matrix: an abandoned pair lands Unresolvable with the
    // named cause, so the matrix stays TOTAL over the federation it claims to cover and a gate reads the abandonment
    // as a verdict instead of reading a short row set as a clean bill.
    // The probe memo is FOLD STATE, threaded through the pair walk beside the rows it feeds — so the whole matrix is
    // one eager left fold whose every probe, memo write, and token read has already happened when the Seq leaves. The
    // retired shape hid a mutable Dictionary inside a lazily-yielded comprehension, which made the matrix a caller
    // holds depend on WHEN it was walked and on how many times: a second enumeration re-read the token against a
    // memo the first pass had already filled, so the same value could answer differently on its second read. Strict()
    // patched the symptom; a fold has no lazy tail to force.
    // The Review/diff#MODEL_DIFF federation change-set and the Review/coordination#COORDINATION rule engine gate their
    // GlobalId joins on this matrix; the AnchorShift is the at-anchor displacement magnitude the AppUi banner renders.
    public static Seq<FrameAlignment> Preflight(Seq<(string Model, GeoReference Frame)> frames, (double X, double Y, double Z) anchor, CancellationToken token, Op key) =>
        toSeq(from i in Enumerable.Range(0, frames.Count)
              from j in Enumerable.Range(i + 1, frames.Count - i - 1)
              select (Source: frames[i], Target: frames[j]))
            .Fold(
                (Memo: Map<(string Source, string Target), (Fin<Reprojection> Run, Option<double> Shift)>(), Rows: Seq<FrameAlignment>()),
                (state, pair) => token.IsCancellationRequested
                    ? (state.Memo, state.Rows.Add(new FrameAlignment(pair.Source.Model, pair.Target.Model, new FrameVerdict.Unresolvable("preflight-abandoned"), None)))
                    : Align(pair.Source, pair.Target, anchor, state.Memo, key) switch {
                        var (memo, row) => (memo, state.Rows.Add(row)),
                    })
            .Rows;

    // The frame's resolution IDENTITY — the memo key, and the only axis a probe outcome depends on: two models on one
    // EPSG code, one WKT text, or one projection identity share their verdict exactly.
    static string FrameKey(GeoReference frame) =>
        frame.Resolution.Switch(
            epsg: () => $"epsg:{frame.Epsg.IfNone(0)}",
            wkt: () => $"wkt:{frame.Crs.Map(static c => c.Wkt).IfNone("")}",
            projection: () => $"proj:{frame.Crs.Map(static c => $"{c.MapProjection}:{c.MapZone}").IfNone("")}",
            unreferenced: static () => "unreferenced");

    // One pair -> its verdict row AND the memo the next pair inherits. Returning the memo is what keeps the walk a
    // fold: the probe cache grows as a VALUE the caller threads, never a container this member reaches into.
    static (Map<(string Source, string Target), (Fin<Reprojection> Run, Option<double> Shift)> Memo, FrameAlignment Row) Align(
        (string Model, GeoReference Frame) source, (string Model, GeoReference Frame) target,
        (double X, double Y, double Z) anchor,
        Map<(string Source, string Target), (Fin<Reprojection> Run, Option<double> Shift)> probes, Op key) {
        if (source.Frame.Resolution == CrsResolution.Unreferenced || target.Frame.Resolution == CrsResolution.Unreferenced) {
            return (probes, new FrameAlignment(source.Model, target.Model, new FrameVerdict.Unresolvable("unreferenced"), None));
        }
        (string, string) memo = (FrameKey(source.Frame), FrameKey(target.Frame));
        (Fin<Reprojection> Run, Option<double> Shift) probe = probes.Find(memo).IfNone(() => {
            double[] ordinates = [anchor.X, anchor.Y, anchor.Z];
            Fin<Reprojection> run = Reproject(source.Frame, target.Frame, ordinates, 3, key);
            return (run, Some(Hypot(ordinates[0] - anchor.X, ordinates[1] - anchor.Y, ordinates[2] - anchor.Z)));
        });
        return (probes.AddOrUpdate(memo, probe), probe.Run.Match(
            Succ: receipt => new FrameAlignment(source.Model, target.Model,
                receipt.Engine == GeoEngine.Identity ? new FrameVerdict.Identical()
                : receipt.EpochDefaulted ? new FrameVerdict.EpochMismatched(receipt)
                : new FrameVerdict.Transformable(receipt),
                probe.Shift),
            Fail: error => new FrameAlignment(source.Model, target.Model, new FrameVerdict.Unresolvable(error.Message), None)));
    }
}
```

## [04]-[RESEARCH]

(none)
