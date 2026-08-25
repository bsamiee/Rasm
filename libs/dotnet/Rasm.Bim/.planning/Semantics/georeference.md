# [BIM_COORDINATE_PROJECTION]

`GeoReferenceProjector` folds whatever georeferencing level an IFC model carries onto the `Rasm.Element` seam `GeoReference` record on the `ElementGraph` `Header` and `Coverage` nodes: `Project` switches the single `IfcGeometricRepresentationContext.HasCoordinateOperation` over an `IfcMapConversion`/`IfcMapConversionScaled` (rotation-and-scale, LoGeoRef 50) or a translation-only `IfcRigidOperation` (length-measured planar form, LoGeoRef 50, IFC4.3), falls back to an `IfcSite` geographic position lowered onto WGS84 (`EPSG:4326`, LoGeoRef 30), and returns `GeoReference.Identity` when a model carries neither so ingest never blocks. `GeoTransform` reprojects raw double-precision ordinate spans between two seam frames over `ProjNET`, escalating an exotic datum-grid or dynamic-datum transform to the `MaxRev.Gdal.Core` OSR PROJ engine, and returns the typed `Reprojection` receipt — engine route, shifted-vertex count, the ordinate rank the transform spans, at-anchor differential scale and grid rotation, forward→inverse round-trip residual, typed dynamic-datum `EpochPosture` — a survey-grade federation validates its rigid placement against.

EVERY CRS-identity value object — `GeoReference`, `ProjectedCrs`, `VerticalCrs` — is SEAM-owned (`Rasm.Element/Geospatial/reference`): the seam owns the compound record, its `GeoReference.Admit` admission, the three-state EPSG/WKT/projection resolution, and the derived `Epsg`/`CrsResolution` columns, while Bim owns only the IFC projection that fills the record and the `ProjNET`/OSR transform that consumes it. Every seam double arrives metre-normalized through the ONE `Projection/value#UNIT_INGRESS` `UnitScheme` regime over TWO distinct regimes the schema keeps apart — the MAP frame (`IfcProjectedCRS.MapUnit`, the declared length unit of the map coordinate axes, which every map ordinate and the seam's own metre-normalized scale ride) and the MODEL frame (the project `IfcUnitAssignment`, which the `IfcSite` elevation alone rides) — because a map ordinate is never a model-unit magnitude and the model↔map conversion is `IfcMapConversion.Scale`'s own declared job; `Project` re-bands the seam fault to `BimFault.Refused` with `BimReason.Capability` bare (band 2600) when a CRS name resolves to neither an EPSG code nor a WKT nor a projection+zone, never silently landing a federation on an unreferenced frame [M1].

Seam `GeoReference` rides `Header`/`Coverage` only, never the `Object` node, carrying the rigid map-conversion offset parameters — translation, projected `RotationRadians` direction-cosine, per-axis scale — a downstream host-bound consumer folds into the kernel `Transform` algebra in the Rhino runtime. This host-neutral page binds GeometryGym, `ProjNET`/`MaxRev.Gdal.Core`, the seam, and the kernel `Op` key, never the RhinoCommon `Transform`/`Point3d`/`Vector3d` it therefore cannot materialize.

## [01]-[INDEX]

- [02]-[GEO_PROJECTION]: `GeoReferenceProjector.Project` folds the model georeferencing level onto the seam `GeoReference` — the `HasCoordinateOperation` switch over `IfcMapConversion`/`IfcMapConversionScaled` or the translation-only `IfcRigidOperation`, else the `IfcSite` WGS84 fallback, else `Identity` — composing the seam `GeoReference.Admit`; `GeoAuthored` the closed level vocabulary the egress elects and returns; the seam `VerticalCrs` (admitted once inside `GeoReference.Admit`) that the `[03]-[GEODETIC_TRANSFORM]` refusal arm reads off `reference.Vertical`; `Author` the railed egress inverse `Projection/egress#IFC_EGRESS` `Emit` composes so an ingested LoGeoRef level round-trips at its own level instead of exporting geo-stripped or level-flattened [M1].
- [03]-[GEODETIC_TRANSFORM]: `GeoTransform.Reproject` the `ProjNET`-first datum leg over two seam frames returning the typed `Reprojection` receipt over the `EpochPosture` plate-motion arm, escalating a transform `ProjNET` cannot express to the `MaxRev.Gdal.Core` OSR engine; `GeoTransform.Preflight` the frame-identity-memoized federation alignment matrix of typed `FrameVerdict` rows a `Review/coordination#COORDINATION` rule engine gates its GlobalId joins on.

## [02]-[GEO_PROJECTION]

- Owner: `GeoReferenceProjector` the static IFC→seam projector folding the georeferencing level the model carries onto the seam `GeoReference` — the single `IfcGeometricRepresentationContext.HasCoordinateOperation` switched over `IfcMapConversion` (or `IfcMapConversionScaled` for per-axis scale) and the translation-only `IfcRigidOperation` (LoGeoRef 50 — the IFC4.3 rigid sibling carries no rotation and no scale, only the two coordinates + `Height` in the target CRS), else the first `IfcSite`'s `RefLatitude`/`RefLongitude`/`RefElevation` geographic position onto a WGS84 (`EPSG:4326`) reference (LoGeoRef 30), else `GeoReference.Identity`; and `GeoAuthored` the closed `[SmartEnum<string>]` level vocabulary the egress elects from the frame's own columns and returns as its receipt. The `GeoReference`/`ProjectedCrs`/`VerticalCrs` value-objects are seam-owned (`Rasm.Element/Geospatial/reference`); this page projects the IFC surface onto them through the ONE `Carriers` read — the base-CRS `Name`/`GeodeticDatum`/`WellKnownText` and the projected-only `VerticalDatum`/`MapProjection`/`MapZone`, every one admitted through the `Projection/value#PROPERTY_LOWERING` `PropertyLowering.Stated` blank-or-absent entry the folder holds as its ONE GeometryGym-string admission — composing the metre-normalized scale and the seam `GeoReference.Admit`, never re-declaring the family, never re-deriving the admission, never minting a CRS parser, and never materializing a kernel transform.
- Entry: `GeoReferenceProjector.Project(IfcProject project, UnitScheme model, Op key)` returns `Fin<GeoReference>` — a model with no map conversion AND no geographic site position returns `GeoReference.Identity` so ingest never blocks; an `IfcProjectedCRS` name present but resolving no identity at all raises `new BimFault.Refused(key, BimScope.Semantics, BimReason.Capability, string.Join(':', new object?[] { "crs-name-unresolvable", name }))` over `Model/faults#FAULT_BAND` `BimFault.Refused` with `BimReason.Capability` BARE (band 2600 owns the generated `Code`) rather than landing the federation on an unreferenced frame [M1]. The `Op key` and the MODEL `UnitScheme` both thread from the `Projection/semantic#SEMANTIC_PROJECTOR` fold head, which composes the success onto the `ElementGraph` `Header.Reference`; the MAP frame's own regime is derived per operation from the CRS it targets. `GeoReferenceProjector.Author(DatabaseIfc db, GeoReference reference, UnitScheme model, Op key)` is the egress inverse `Projection/egress#IFC_EGRESS` `Emit` composes beside `ReauthorHeader`, returning `Fin<GeoAuthored>` so a caller reads WHICH LoGeoRef level survived: `Unreferenced` for the `Identity` frame, `Geographic` for the EPSG:4326 site shape, `Rigid` for the translation-only metre-identity shape, `Conversion` for an isotropic scale, `Scaled` for an anisotropic one on a schema carrying the subtype — with `BimFault.Refused` with `BimReason.Rejected` on a database carrying no `IfcProject` and `BimFault.Refused` with `BimReason.DanglingReference` when the elected level's anchor entity is absent, each formerly a silent early return that reported success while writing nothing.
- Auto: `Project` is a three-arm fold. The map-conversion arm reads the rigid offset (`Eastings`/`Northings`/`OrthogonalHeight`), the `XAxisAbscissa`/`XAxisOrdinate` direction-cosine pair (each `double.NaN` when unset, coerced to the identity direction `(1,0)` so `RotationRadians` resolves to `0` rather than `Atan2(NaN,NaN)`), and the per-axis scale as the schema COMPOSES it — IFC4.3 gives `IfcMapConversion` ONE `Scale` applied equally to x, y and z (`1.0` when omitted) and puts the per-axis factors on the `IfcMapConversionScaled` subtype where the transform is `Scale × Factor(axis)` — so the seam axis is that PRODUCT. The rigid-operation arm reads `FirstCoordinate`/`SecondCoordinate` in its length-measured planar form ONLY, the `Project` guard pattern-binding BOTH as `IfcLengthMeasure` so each magnitude reads off the public `IfcMeasureValue.Measure` double with no boxed `Convert` hop, `Height` (NaN→`0.0`) likewise, identity direction and the map frame's own metre factor on all three scale axes; the angle-measured geographic-target form is left to the site arm's `Identity`. BOTH operation arms carry every map ordinate AND the composed axis scale through the ONE `UnitScheme.Coerce` entry over the MAP frame built from `IfcProjectedCRS.MapUnit`. The site arm folds `RefLatitude`/`RefLongitude` `IfcCompoundPlaneAngleMeasure` through `.Angle()` to decimal degrees and coerces `RefElevation` (NaN→`0.0`) through the MODEL regime — the one magnitude on this page that IS a project-unit length — then hands the literal `EPSG:4326` authority name with blank WKT/projection/zone carriers to the same `Admit`. Inside `Admit` the seam builds the three-state `ProjectedCrs`, resolving the EPSG code across the OGC URN and the authority form (a BARE numeric name carries no authority evidence and resolves none) while a WKT-defined or projection+zone CRS resolves WITHOUT an EPSG. `Author` elects its level from the frame's own columns through one TOLERANCE compare, authors ONE `IfcProjectedCRS` declaring NO `MapUnit`, and splits the scale back the way the schema composes it: three equal axes author the isotropic `Scale`, differing axes author `Scale = 1.0` with the three `Factor` columns, and differing axes on a pre-`IFC4X3_ADD2` target author the shared component isotropically and report `Conversion`, so the bounded anisotropy drop is a level a caller READS.
- Receipt: the seam `GeoReference` is the coordinate-reference evidence the `Header` carries (and the `Semantics/raster#RASTER_INGEST` `Coverage` node carries for a georeferenced raster); its parameters feed a DOWNSTREAM host-bound kernel `Transform` consumer in the Rhino runtime, never a transform this host-neutral projector builds; the seam `CrsResolution` mode drives the `[03]-[GEODETIC_TRANSFORM]` EPSG-keyed-vs-WKT-keyed build path; the egress `GeoAuthored` level is the round-trip evidence an export audit reads — an ingest that recorded a rigid operation and an export reporting `Rigid` is a closed loop, where `Conversion` on the same frame names a level promotion.
- Packages: GeometryGymIFC_Core, Rasm.Element, Rasm (the host-neutral `Rasm.Domain.Op` key and the `UnitScheme` coercion entry), Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new map-conversion parameter is one column on the seam `GeoReference`; a new georeferencing level is one arm on the `Project` fold plus one `GeoAuthored` row and its election clause (a future LoGeoRef-40 `WorldCoordinateSystem`/`TrueNorth` rotation folds onto the existing rotation field); a new CRS-name scheme is one arm on the seam `ProjectedCrs.Epsg`; the rigid offset is the downstream host-bound kernel transform's and the datum shift is the `[03]-[GEODETIC_TRANSFORM]` leg; never a new transform owner, never a Bim CRS parser, never a per-CRS class, and never a page-local unit factor beside the `UnitScheme` pair.
- Boundary: EVERY CRS-identity value object — `GeoReference`, `ProjectedCrs`, `VerticalCrs` — is SEAM-owned [M1]; re-declaring any of the family in Bim is the named drift defect, and constructing a `GeoReference` with a stored top-level `Epsg` slot, a bare `VerticalDatum` string column, or any column the seam does not declare is the deleted form (`Epsg` is a DERIVED `Crs.Bind(c => c.Epsg)`). The seam frame rides `Header`/`Coverage` ONLY; a `GeoReference` on the `Object` node is the deleted form [M1]. The projector composes `GeoReference.Admit` and an inline CRS parse beside it — or a hand-rolled Bim CRS parser — is the deleted form. The per-axis seam scale is the product the schema itself composes, so a `Factor`-only read (dropping the unit reconciliation whole) and a read of GeometryGym's vendor `ScaleY`/`ScaleZ` pair (which default to `1.0` rather than to `Scale`, fabricating an anisotropic frame out of every isotropic non-metre model) are both named defects. That composed scale AND every map ordinate cross the `UnitScheme.Coerce` entry over the MAP regime — one entry carries a length and a ratio alike because a map unit is never affine [M1] — so the doubles handed to `Admit` are METRE-NORMALIZED and the seam carries NO `MapUnit` field; the MAP and MODEL regimes are two distinct `UnitScheme` values and crossing them is the named defect, the `IfcSite` elevation alone riding the project regime; a page-local `MetrePerMapUnit`-shaped member returning a raw multiplier is the deleted form the `UnitScheme` pair exists to close. All three CRS carriers (authority `Name`, inline `WellKnownText`, `MapProjection`/`MapZone`) reach `Admit`; dropping the WKT/projection carry is the two-state slice that false-faults a GIS-origin WKT CRS. Every GeometryGym string crosses `PropertyLowering.Stated` — a `?? ""` coalesce beside it is the deleted duplicate, and a second blank-or-absent admission owner in this folder is the named twin. A DECLARED non-positive or non-finite scale component — `IfcMapConversion.Scale`, an `IfcMapConversionScaled` `Factor`, or the map unit's own `SIFactor()` — FAULTS by column name, because coercing a zero to unity deletes the seam's strictly-positive gate before `Admit` sees the value; absence needs no coercion beside it (the `Scale` getter answers unity for its NaN unset field and the three `Factor` fields initialize to unity), so a page-local absent-to-unity fold is the deleted duplicate. The rigid coordinates read off the public `IfcMeasureValue.Measure` under a pattern binding both as `IfcLengthMeasure`, and a boxed `Convert.ToDouble(measure.Value)` escaping the rail on a null or angle-measured coordinate is the deleted form. The projection rides the GeometryGym georeferencing entities (`.api/api-geometrygym-ifc`) as settled vocabulary, a hand-rolled IFC reader the deleted form; the kernel `Transform`/`Point3d`/`Vector3d` are RhinoCommon types and composing them here is the named host-neutrality defect. The egress is RAILED and TOTAL over the level vocabulary — a `void Author` whose absent-anchor paths returned silently is the deleted form that reported a frame it never wrote, and an egress flattening every non-site frame onto `IfcMapConversion` is the level-promotion defect the election closes; the authored `IfcProjectedCRS` declares no `MapUnit`, so an inverse map-unit fold at egress divides by a unit no authored CRS declares. The level election compares against a declared band and an exact `RotationRadians == 0.0` or `ScaleX == 1.0` on those derived doubles is the deleted form — the band SEATS on the kernel `ToleranceLane.Identity` row (Band.Residual, `EpsilonPolicy.ZeroTolerance`), read through `Context.Canonical` because the egress holds no composition `Context`; mode is the LANE'S OWN `Band` fact, never a carrier axis (E-B11 landed-and-refuted), a page literal beside the seated lane is the deleted form, and a composition retunes through `Context.Override`.

```csharp
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using System.Globalization;
using GeometryGym.Ifc;
using LanguageExt;
using Rasm.Bim.Projection;
using Rasm.Element.Geospatial;
using Thinktecture;
using Op = Rasm.Domain.Op;
using static LanguageExt.Prelude;

namespace Rasm.Bim;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GeoAuthored {
    public static readonly GeoAuthored Unreferenced = new("unreferenced");
    public static readonly GeoAuthored Geographic   = new("geographic");
    public static readonly GeoAuthored Rigid        = new("rigid");
    public static readonly GeoAuthored Conversion   = new("conversion");
    public static readonly GeoAuthored Scaled       = new("scaled");
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class GeoReferenceProjector {
    public static Fin<GeoReference> Project(IfcProject project, UnitScheme model, Op key) =>
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
        Fin<(double X, double Y, double Z)> axes = conversion is IfcMapConversionScaled scaled
            ? from s in Positive(conversion.Scale, nameof(IfcMapConversion.Scale), key)
              from fx in Positive(scaled.FactorX, nameof(IfcMapConversionScaled.FactorX), key)
              from fy in Positive(scaled.FactorY, nameof(IfcMapConversionScaled.FactorY), key)
              from fz in Positive(scaled.FactorZ, nameof(IfcMapConversionScaled.FactorZ), key)
              select (s * fx, s * fy, s * fz)
            : Positive(conversion.Scale, nameof(IfcMapConversion.Scale), key).Map(static s => (s, s, s));
        double abscissa = double.IsNaN(conversion.XAxisAbscissa) ? 1.0 : conversion.XAxisAbscissa;
        double ordinate = double.IsNaN(conversion.XAxisOrdinate) ? 0.0 : conversion.XAxisOrdinate;
        return from map in MapFrame(conversion.TargetCRS, key)
               from axis in axes
               from reference in Admit(
                   Metres(conversion.Eastings, map), Metres(conversion.Northings, map), Metres(conversion.OrthogonalHeight, map),
                   abscissa, ordinate, Metres(axis.X, map), Metres(axis.Y, map), Metres(axis.Z, map), conversion.TargetCRS, key)
               select reference;
    }

    static Fin<GeoReference> FromRigidOperation(IfcRigidOperation rigid, IfcLengthMeasure first, IfcLengthMeasure second, Op key) =>
        MapFrame(rigid.TargetCRS, key).Bind(map => {
            double metre = Metres(1.0, map);
            return Admit(
                Metres(first.Measure, map), Metres(second.Measure, map),
                Metres(double.IsNaN(rigid.Height) ? 0.0 : rigid.Height, map),
                1.0, 0.0, metre, metre, metre, rigid.TargetCRS, key);
        });

    static Fin<GeoReference> FromSite(IfcSite site, UnitScheme model, Op key) =>
        site.RefLatitude is null || site.RefLongitude is null
            ? Fin.Succ(GeoReference.Identity)
            : GeoReference.Admit(
                site.RefLongitude.Angle(), site.RefLatitude.Angle(),
                Metres(double.IsNaN(site.RefElevation) ? 0.0 : site.RefElevation, model),
                1.0, 0.0, 1.0, 1.0, 1.0, "WGS84", "", "EPSG:4326", "", "", "", key);

    static Fin<GeoReference> Admit(double e, double n, double h, double abscissa, double ordinate, double sx, double sy, double sz, IfcCoordinateReferenceSystem? crs, Op key) {
        var (name, datum, vertical, wkt, mapProjection, mapZone) = Carriers(crs);
        return GeoReference.Admit(e, n, h, abscissa, ordinate, sx, sy, sz, datum, vertical, name, wkt, mapProjection, mapZone, key)
            .MapFail(_ => new BimFault.Refused(key, BimScope.Semantics, BimReason.Capability, string.Join(':', new object?[] { "crs-name-unresolvable", name })));
    }

    static (string Name, string GeodeticDatum, string VerticalDatum, string Wkt, string MapProjection, string MapZone) Carriers(IfcCoordinateReferenceSystem? crs) {
        Option<IfcProjectedCRS> projected = Optional(crs as IfcProjectedCRS);
        return (PropertyLowering.Stated(crs?.Name).Filter(static n => !string.Equals(n, "Unknown", StringComparison.OrdinalIgnoreCase)).IfNone(""),
                PropertyLowering.Stated(crs?.GeodeticDatum).IfNone(""),
                projected.Bind(static p => PropertyLowering.Stated(p.VerticalDatum)).IfNone(""),
                PropertyLowering.Stated(crs?.WellKnownText?.WellKnownText).IfNone(""),
                projected.Bind(static p => PropertyLowering.Stated(p.MapProjection)).IfNone(""),
                projected.Bind(static p => PropertyLowering.Stated(p.MapZone)).IfNone(""));
    }

    static Fin<UnitScheme> MapFrame(IfcCoordinateReferenceSystem? crs, Op key) =>
        (crs as IfcProjectedCRS)?.MapUnit is { } unit
            ? Positive(unit.SIFactor(), nameof(IfcNamedUnit.SIFactor), key).Map(static factor => UnitScheme.Si with { L = factor })
            : Fin.Succ(UnitScheme.Si);

    static double Metres(double native, UnitScheme frame) => frame.Coerce(native, QuantityType.Length, Dimension.LengthDim);

    static Fin<double> Positive(double value, string column, Op key) =>
        double.IsFinite(value) && value > 0.0
            ? Fin.Succ(value)
            : Fin.Fail<double>(new BimFault.Refused(key, BimScope.Semantics, BimReason.Rejected, string.Join(':', new object?[] { "map-scale-degenerate", column, value.ToString("R", CultureInfo.InvariantCulture) })));

    static readonly double FrameEpsilon = Context.Canonical.For(ToleranceLane.Identity).Value;

    static bool Rigidly(GeoReference reference) =>
        Math.Abs(reference.RotationRadians) <= FrameEpsilon
        && Math.Abs(reference.ScaleX - 1.0) <= FrameEpsilon
        && Math.Abs(reference.ScaleY - 1.0) <= FrameEpsilon
        && Math.Abs(reference.ScaleZ - 1.0) <= FrameEpsilon;

    static bool Isotropic(GeoReference reference) =>
        Math.Abs(reference.ScaleX - reference.ScaleY) <= FrameEpsilon && Math.Abs(reference.ScaleX - reference.ScaleZ) <= FrameEpsilon;

    static GeoAuthored Level(DatabaseIfc db, GeoReference reference) =>
        reference == GeoReference.Identity ? GeoAuthored.Unreferenced
        : !Isotropic(reference) ? (db.Release >= ReleaseVersion.IFC4X3_ADD2 ? GeoAuthored.Scaled : GeoAuthored.Conversion)
        : !Rigidly(reference) ? GeoAuthored.Conversion
        : reference.Epsg == Some(4326) ? GeoAuthored.Geographic
        : GeoAuthored.Rigid;

    public static Fin<GeoAuthored> Author(DatabaseIfc db, GeoReference reference, UnitScheme model, Op key) {
        GeoAuthored level = Level(db, reference);
        if (level == GeoAuthored.Unreferenced) { return Fin.Succ(level); }
        if (db.Project is not IfcProject project) {
            return Fin.Fail<GeoAuthored>(new BimFault.Refused(key, BimScope.Semantics, BimReason.Rejected, string.Join(':', new object?[] { "geo-author-projectless" })));
        }
        return level == GeoAuthored.Geographic
            ? Optional(project.Extract<IfcSite>().FirstOrDefault())
                .ToFin(new BimFault.Refused(key, BimScope.Semantics, BimReason.DanglingReference, string.Join(':', new object?[] { "geo-author-anchor-miss", "site" })))
                .Bind(site => AuthorSite(site, reference, model, key))
            : Optional(project.RepresentationContexts.OfType<IfcGeometricRepresentationContext>().FirstOrDefault())
                .ToFin(new BimFault.Refused(key, BimScope.Semantics, BimReason.DanglingReference, string.Join(':', new object?[] { "geo-author-anchor-miss", "context" })))
                .Map(context => AuthorOperation(db, context, reference, level));
    }

    static Fin<GeoAuthored> AuthorSite(IfcSite site, GeoReference reference, UnitScheme model, Op key) =>
        MeasureValue.OfSi(Dimension.LengthDim, reference.OrthogonalHeight, key)
            .Map(model.Render)
            .Map(declared => {
                site.RefLongitude = new IfcCompoundPlaneAngleMeasure(reference.Eastings);
                site.RefLatitude = new IfcCompoundPlaneAngleMeasure(reference.Northings);
                site.RefElevation = declared.Value;
                return GeoAuthored.Geographic;
            });

    static GeoAuthored AuthorOperation(DatabaseIfc db, IfcGeometricRepresentationContext context, GeoReference reference, GeoAuthored level) {
        var crs = new IfcProjectedCRS(db, reference.Epsg.Match(
            Some: static code => $"EPSG:{code}",
            None: () => reference.Crs.Map(static c => c.Name).IfNone(""))) {
            GeodeticDatum = reference.GeodeticDatum,
            VerticalDatum = reference.Vertical.Map(static v => v.Name).IfNone(""),
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

- Owner: `GeoTransform` the datum-bridging leg reprojecting raw ordinate spans between two seam `GeoReference` frames — EACH frame resolves its `ProjNET` `CoordinateSystem` off its OWN seam `CrsResolution` (`ManagedCs`: the `Wkt` arm the shared `CoordinateSystemFactory.CreateFromWkt` over the frame's own payload, the `Epsg` arm the SAME parser over the `EpsgWkt` definition one OSR `ImportFromEPSGA`/`ExportToWkt` hop resolves, because `ProjNET` ships no code registry at all), the ONE `CoordinateTransformationFactory.CreateFromCoordinateSystems(src, dst)` building the managed transform for EVERY resolvable pair (both-EPSG, both-WKT, and the MIXED EPSG↔WKT federation alike) — escalating an exotic datum-grid or dynamic-datum transform `ProjNET` cannot express to the `MaxRev.Gdal.Core` OSR PROJ engine (keyed by `ImportFromEPSGA` or `ImportFromWkt` to match the frame's resolution) per the `.api/api-projnet` escalation-seam; `CsFactory` the one WKT parser, `TransformFactory` the one CS-pair build, and `ManagedFrames` the one `FrameKey`-keyed `CoordinateSystem` cache the `.api/api-projnet` `CRS_TRANSFORM` law names as the single owners. `EpochPosture` is the typed plate-motion arm the receipt carries in place of a bool, and the managed batch dispatches on the transform's own declared `DimTarget` rank. The leg operates on the seam `GeoReference` frame and a `ProjNET`/OSR datum shift folded onto the kernel transform is the named seam violation.
- Entry: `GeoTransform.Preflight(Seq<(string Model, GeoReference Frame)> frames, (double X, double Y, double Z) anchor, CancellationToken token, Op key)` returns `Fin<Seq<FrameAlignment>>` and folds a federation's frames into the complete pairwise `FrameAlignment` matrix on success — one row per unordered INDEX pair (so two models sharing a name both appear, where an ordinal name compare dropped every such pair), one probe reprojection per distinct FRAME-IDENTITY pair memoized across the run (an N-model federation over M distinct frames builds M(M−1)/2 transforms, not N(N−1)/2), every transform outcome a typed `FrameVerdict` row retaining its exact `Error` cause, the at-anchor displacement riding each row — the preflight artifact a coordination manager rules the federated join on before any element-level work runs. `GeoTransform.Reproject(GeoReference source, GeoReference target, Span<double> ordinates, int stride, Op key)` applies the datum-to-datum transform IN PLACE on the interleaved double ordinate buffer when both frames carry a resolvable CRS (EPSG or WKT) that differs, returning `Fin<Reprojection>` — the typed receipt carrying the engine route, the shifted-vertex count, the ordinate RANK the transform spans, the forward→inverse round-trip residual, the central-difference anchor `AnchorScale`/`AnchorConvergence` distortion evidence, and the `EpochPosture` — each probe column an `Option<double>` so a REFUSED probe is a recorded absence and never a fabricated unit scale or zero residual: the additive cases (a source or target `CrsResolution.Unreferenced`, an identical CRS, or fewer than one full vertex) return `Reprojection.Identity` — engine `Identity`, zero shifted vertices, `EpochPosture.Unprobed`, and every evidence column `None`, because an identity leg PROBED nothing — so the datum leg never blocks a single-datum federation; a `CrsResolution.Projection` frame (the seam's typed projection+zone-only mode) faults `crs-projection-only-unbuildable` by CASE, and a pair whose two `VerticalCrs` height datums DIFFER faults `crs-vertical-untransformable` by name — no geoid model reaches either engine, so a horizontal-only shift that carries Z across a datum boundary lands a federation correct in plan and metres wrong in elevation — neither engine builds from a bare projection identity, and the empty-`Wkt` payload sniff is the deleted form; a differing, resolvable pair resolves EACH frame's `CoordinateSystem` through its `Resolution` generated total `Switch` (`ManagedCs`) into the ONE facade `CreateTransformation(src, dst)` managed build (a mixed EPSG↔WKT pair included), runs the strided batch once at the transform's own rank, escalates to the matching OSR build (`ImportFromEPSGA`/`ImportFromWkt`) when `ProjNET` cannot express the transform, and faults `BimFault.Refused` with `BimReason.Capability` BARE only when BOTH engines fail. The buffer is `double` end to end — a survey easting never narrows to `float` (the `Semantics/feature#GEO_FEATURE` precision contract) — and the NTS `CoordinateSequence` flatten plus the `Geometry.Apply` write-back is the geospatial CONSUMER's marshalling, so the leg stays geometry-library-neutral over raw ordinates. Composed BEFORE the downstream host-bound rigid map-conversion offset so a federated model lands in the shared datum before its local-engineering placement applies.
- Auto: `Reproject` short-circuits when either frame is `CrsResolution.Unreferenced`, when the two CRS identities are equal (same EPSG, or same `Crs` value), or when the buffer holds fewer than one full vertex; otherwise EACH frame resolves its `CoordinateSystem` through its own `Resolution` generated total `Switch` (`ManagedCs` — the `Epsg` arm `CsFactory.CreateFromWkt(EpsgWkt(code))` over the PROJ-resolved definition, the `Wkt` arm `CsFactory.CreateFromWkt(wkt)` over the frame's own payload, both behind the `ManagedFrames` `FrameKey` cache, the `Unreferenced` arm unreachable here since the short-circuit already returned) and the ONE `TransformFactory.CreateFromCoordinateSystems(srcCS, dstCS).MathTransform` builds the managed transform. That build is captured through `Op.Catch` onto an `Option<MathTransform>` so an EPSG code PROJ's own database does not carry, a WKT `ProjNET` cannot parse, or a datum `ProjNET` cannot express routes the OSR escalation rather than throwing across the boundary or re-entering a null. The `ProjNET` apply reads the transform's own `DimTarget` and takes the matching `api-projnet#ENTRYPOINTS` strided overload IN PLACE on the interleaved buffer — the three-column `Transform(xs, ys, zs, stride×3)` for a rank-3 transform, the two-column `Transform(xs, ys, stride×2)` for a planar one so the Z column is untouched BY CONSTRUCTION rather than by trusting each concrete transform's core to leave it alone — with no staging copy, and the `TransformCore` `while (num < xs.Length)` walk driving the count off the full-length first column so the last vertex is covered, a `stride` above three leaving the non-position interleave columns untouched; the receipt records that rank so a survey audit reads whether the height column was reprojected at all. The OSR escalation deinterleaves the position columns into pooled `double[]` x/y/z, runs the one `Semantics/raster#RASTER_INGEST` `GeoGdal.Bootstrap` idempotent guard (`GdalBase.ConfigureAll` + `Osr.UseExceptions`), builds two `SpatialReference` keyed to match each frame's resolution through the TOTAL four-arm `CrsResolution` `Switch` (`ImportFromEPSGA` for an EPSG frame, `ImportFromWkt` for a WKT frame, the projection-only and unreferenced arms unreachable and empty by construction, `OAMS_TRADITIONAL_GIS_ORDER` pinning lon/lat against the GDAL-3 axis swap) and one `CoordinateTransformation` under the two options gates (`SetBallparkAllowed(false)` — a gridless pair faults, never a coarse ballpark shift; `SetOnlyBest(true)` — a missing best-accuracy operation faults, never a silent lower-accuracy fallback), elects the `EpochPosture` off each frame's `IsDynamic()` against its own seam `Epoch`, runs one `TransformPoints(count, xs, ys, zs)`, and reinterleaves; on BOTH engines the receipt evidence rides the same shifted anchor — the `GetInverse`/`Inverse()` round-trip residual and the `Distortion` central-difference Jacobian probe are inner-`Op.Catch` recorded absences (`None`), never leg faults; `Preflight` keys its probe memo on each frame's RESOLUTION IDENTITY (its EPSG code, its WKT text, or its projection identity) so a federation of many models on few frames pays one build per frame pair and a memo HIT re-uses the stored probe without re-writing it, and it checks the caller's `CancellationToken` at each PAIR boundary — the managed grain the `RULINGS` native-lane row demands stated honestly, because an in-flight `TransformPoints` batch and an OSR pipeline build publish no interrupt of their own — lowering abandonment to `Errors.Cancelled` on the returned rail, so no partial matrix can read as complete; the datum shift composes BEFORE the rigid offset so a model lands in the shared datum before its local-engineering-frame placement applies.
- Packages: ProjNET, MaxRev.Gdal.Core, Rasm.Element, Rasm, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new EPSG, WKT, or mixed CRS pair is the per-frame `ManagedCs` resolution joined by the one `TransformFactory.CreateFromCoordinateSystems` build, never a per-call factory; a new CRS-resolution mode is one arm on the seam `CrsResolution` that breaks BOTH `Switch` sites at compile time (the seam owns the discriminant, this leg owns the per-mode build); an exotic datum-grid or dynamic datum is the OSR PROJ pipeline's, resolved from the EPSG code or the WKT, never a hand-rolled Bursa-Wolf matrix; a float-buffered consumer widens to `double` at its OWN boundary and calls the one `Span<double>` leg, never a parallel `Span<float>` overload re-admitting the survey-precision-loss footgun; a denser batch is one `MathTransform`/`CoordinateTransformation` overload swap, never a second transform owner and never a per-vertex `ref` loop; a new PROJ pipeline gate is one `CoordinateTransformationOptions` setter row on the one OSR options build; a new plate-motion state is one `EpochPosture` row every consumer's compare breaks on; the coordinate epoch itself is the seam's `GeoReference.Epoch`, threaded through `SpatialReference.SetCoordinateEpoch` per frame, never a Bim-local epoch knob; a new receipt evidence column is one `Option<double>` `Reprojection` field fed by the shared anchor probes, never a per-engine receipt sibling; a new alignment verdict is one `FrameVerdict` case every matrix consumer's `Switch` breaks on at compile time, never a parallel per-consumer compatibility test.
- Boundary: the datum reprojection is `ProjNET`'s by default — the per-frame `ManagedCs` resolution joined by the ONE `CreateFromCoordinateSystems(src, dst)` build — escalating to the `MaxRev.Gdal.Core` OSR pipeline for what the managed algebra cannot express, and a hand-rolled datum shift, a per-CALL factory rebuild outside the shared owners (`CsFactory`, `TransformFactory`, `ManagedFrames`), or OSR for a transform `ProjNET` already covers is the deleted form per the `.api/api-projnet` single-cache-owner + escalation-seam law; `CoordinateSystemServices` is that deleted form's own vehicle here, since its EPSG registry is the two-code `DefaultInitialization` pair and every other SRID answers `null`, so an `Epsg` frame routed through it escalates a managed-expressible transform to OSR behind a swallowed null. Branching the build off a re-spelled `Epsg.IsSome` check rather than the seam `CrsResolution` `Switch` is the deleted form, reading only `source.Epsg`/`target.Epsg` so a WKT-only federation silently no-ops is the named defect this leg closes, and a source-only branch escalating a MIXED pair the per-frame build already expresses is the same deleted form; every `CrsResolution` `Switch` here supplies ALL FOUR arms in one return shape, the unreachable arms staying present and empty with their unreachability named, because a partial arm set compiles against a generated total dispatch only by accident of overload resolution. The managed build result is an `Option<MathTransform>` and a `MathTransform?`/`ValueUnsafe` re-entry inside the rail is the deleted form. The apply is the strided `double` batch run in place at the transform's DECLARED `DimTarget` rank, and handing a planar transform a Z span (or a per-vertex `Transform(ref x, ref y, ref z)` loop, or narrowing survey ordinates to `float`) is the rejected form; the receipt states that rank rather than implying every column shifted. The GDAL bootstrap is the one `GeoGdal.Bootstrap` idempotent guard and a second `GdalBase.ConfigureAll` owner is the deleted form. The leg is additive — a frame's `Unreferenced` or an identical CRS returns `Reprojection.Identity` — and faults `BimFault.Refused` with `BimReason.Capability` BARE only on a malformed buffer, a projection+zone-only frame (named BEFORE two doomed engine builds), an out-of-domain vertex, or a differing resolvable pair that defeats both engines, the `Op key` carrying the operation context with no `.ToError()` hop. Reading `MathTransform.Derivative`/`GetDomainFlags` or `ICoordinateTransformation.AreaOfUse` for the distortion evidence or the domain guard is the phantom form (base-only `NotImplementedException`, factory-empty `AreaOfUse` — decompile-verified) — the `Distortion` central-difference anchor probe and the engine-agnostic non-finite scan are the honest owners, and a receipt asserting evidence it never probed is the illusory form this receipt closes, so `Reprojection.Identity` publishes `None` on every evidence column and `EpochPosture.Unprobed` rather than the fabricated unit scale, zero residual, and `false` epoch flag an identity leg never measured; the plate-motion posture is a typed FOUR-state row and a `bool EpochDefaulted` that reads `false` for "static", "epoch modelled", and "never asked" alike is the deleted form. The distortion probe's step multiplier SEATS on the kernel `ToleranceLane.Probe` row (Band.Ratio, `EpsilonPolicy.CbrtEpsilon` — the principled central-difference balance): the RATIO band fact IS the relative mode, the consumer scales by its own anchor magnitude, and a mode axis on the `Tolerance` carrier was REFUTED at the kernel (E-B11) — a page literal beside the seated lane is the deleted form. The pairwise matrix keys on the frame INDEX pair and an ordinal MODEL-NAME compare is the deleted form that silently dropped every same-named pair; the abort grain is DECLARED — the token gates the pair boundary and a single in-flight batch or pipeline build runs to completion — and abandonment returns `Errors.Cancelled` rather than any partial matrix. The reprojection composes BEFORE the downstream host-bound rigid offset so the kernel transform stays datum-free; the page reprojects raw `Span<double>` buffers, so a `GeoTransform` overload binding an NTS `Geometry`/`CoordinateSequence` is the misplaced-concern form and a RhinoCommon geometry type crossing this leg is the host-bound defect.

```csharp
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using System.Buffers;
using System.Collections.Concurrent;
using System.Globalization;
using OSGeo.OSR;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GeoEngine {
    public static readonly GeoEngine Identity  = new("identity");
    public static readonly GeoEngine Managed   = new("managed");
    public static readonly GeoEngine Escalated = new("escalated");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class EpochPosture {
    public static readonly EpochPosture Unprobed  = new("unprobed",  severity: 0);
    public static readonly EpochPosture Static    = new("static",    severity: 1);
    public static readonly EpochPosture Modelled  = new("modelled",  severity: 2);
    public static readonly EpochPosture Defaulted = new("defaulted", severity: 3);

    public int Severity { get; }

    public static EpochPosture Pair(EpochPosture left, EpochPosture right) => left.Severity >= right.Severity ? left : right;
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct Reprojection(
    GeoEngine Engine, int ShiftedVertices, Option<int> ShiftedOrdinates, Option<double> RoundTripResidual,
    Option<double> AnchorScale, Option<double> AnchorConvergence, EpochPosture Epoch) {
    public static readonly Reprojection Identity =
        new(GeoEngine.Identity, 0, None, None, None, None, EpochPosture.Unprobed);
}

[Union]
public abstract partial record FrameVerdict {
    private FrameVerdict() { }
    public sealed record Identical : FrameVerdict;
    public sealed record Transformable(Reprojection Evidence) : FrameVerdict;
    public sealed record EpochMismatched(Reprojection Evidence) : FrameVerdict;
    public sealed record Unresolvable(Error Cause) : FrameVerdict;
}

public readonly record struct FrameAlignment(string SourceModel, string TargetModel, FrameVerdict Verdict, Option<double> AnchorShift);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class GeoTransform {
    static readonly CoordinateSystemFactory CsFactory = new();
    static readonly CoordinateTransformationFactory TransformFactory = new();
    static readonly ConcurrentDictionary<string, CoordinateSystem> ManagedFrames = new(StringComparer.Ordinal);

    public static Fin<Reprojection> Reproject(GeoReference source, GeoReference target, Span<double> ordinates, int stride, Op key) {
        if (stride < 3 || ordinates.Length % stride != 0) {
            return Fin.Fail<Reprojection>(new BimFault.Refused(key, BimScope.Semantics, BimReason.Capability, string.Join(':', new object?[] { "crs-buffer-malformed", "stride", stride.ToString(CultureInfo.InvariantCulture), "length", ordinates.Length.ToString(CultureInfo.InvariantCulture) })));
        }
        bool sameFrame =
            (from s in source.Epsg from t in target.Epsg select s == t).IfNone(false) || source.Crs == target.Crs;
        if (source.Resolution == CrsResolution.Unreferenced || target.Resolution == CrsResolution.Unreferenced
            || sameFrame || ordinates.Length < stride) {
            return Fin.Succ(Reprojection.Identity);
        }
        if ((ProjectionOnly(source) | ProjectionOnly(target)).Case is string gap) {
            return Fin.Fail<Reprojection>(new BimFault.Refused(key, BimScope.Semantics, BimReason.Capability, string.Join(':', new object?[] { "crs-projection-only-unbuildable", gap })));
        }
        if (VerticalGap(source, target).Case is string vertical) {
            return Fin.Fail<Reprojection>(new BimFault.Refused(key, BimScope.Semantics, BimReason.Capability, string.Join(':', new object?[] { "crs-vertical-untransformable", vertical })));
        }
        Option<MathTransform> managed = key.Catch(() =>
                from src in ManagedCs(source)
                from dst in ManagedCs(target)
                select TransformFactory.CreateFromCoordinateSystems(src, dst).MathTransform)
            .Match(Succ: static t => t, Fail: static _ => Option<MathTransform>.None);
        if (managed.Case is not MathTransform transform) {
            return Osr(source, target, ordinates, stride, key);
        }
        int count = ordinates.Length / stride;
        var (ox, oy, oz) = (ordinates[0], ordinates[1], ordinates[2]);
        int rank = transform.DimTarget;
        if (rank >= 3) {
            transform.Transform(ordinates, ordinates[1..], ordinates[2..], stride, stride, stride);
        } else {
            transform.Transform(ordinates, ordinates[1..], stride, stride);
        }
        var (scale, convergence) = Distortion((x, y) => { var (px, py, _) = transform.Transform(x, y, oz); return (px, py); }, ox, oy, key);
        return AllFinite(ordinates, stride, count)
            ? Fin.Succ(new Reprojection(GeoEngine.Managed, count, Some(rank), RoundTrip(transform, ordinates, ox, oy, oz, key), scale, convergence, EpochPosture.Unprobed))
            : Fin.Fail<Reprojection>(new BimFault.Refused(key, BimScope.Semantics, BimReason.Capability, string.Join(':', new object?[] { "crs-out-of-domain", source.Resolution.Key, target.Resolution.Key })));
    }

    static Option<string> VerticalGap(GeoReference source, GeoReference target) =>
        from s in source.Vertical
        from t in target.Vertical
        from gap in s == t ? Option<string>.None : Some($"{s.Name}->{t.Name}")
        select gap;

    static Option<string> ProjectionOnly(GeoReference frame) =>
        frame.Resolution == CrsResolution.Projection
            ? frame.Crs.Map(static c => $"{c.MapProjection}:{c.MapZone}")
            : Option<string>.None;

    static Option<CoordinateSystem> ManagedCs(GeoReference frame) =>
        frame.Resolution.Switch(
            epsg: () => frame.Epsg.Map(code => ManagedFrames.GetOrAdd($"epsg:{code}", _ => CsFactory.CreateFromWkt(EpsgWkt(code)))),
            wkt: () => frame.Crs.Map(static c => ManagedFrames.GetOrAdd($"wkt:{c.Wkt}", static _ => CsFactory.CreateFromWkt(c.Wkt))),
            projection: static () => Option<CoordinateSystem>.None,
            unreferenced: static () => Option<CoordinateSystem>.None);

    static string EpsgWkt(int code) {
        GeoGdal.Bootstrap();
        using var crs = new SpatialReference("");
        crs.ImportFromEPSGA(code);
        crs.ExportToWkt(out string wkt, []);
        return wkt;
    }

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
            Fin<(Option<double> RoundTrip, Option<double> Scale, Option<double> Convergence, EpochPosture Epoch)> outcome = key.Catch(() => {
                GeoGdal.Bootstrap();
                using SpatialReference src = Crs(source);
                using SpatialReference dst = Crs(target);
                using var options = new CoordinateTransformationOptions();
                options.SetBallparkAllowed(false);
                options.SetOnlyBest(true);
                EpochPosture epoch = EpochPosture.Pair(Posture(src, source), Posture(dst, target));
                using var pipeline = new CoordinateTransformation(src, dst, options);
                pipeline.TransformPoints(count, xs, ys, zs);
                Option<double> roundTrip = key.Catch(() => {
                    double[] rx = [xs[0]], ry = [ys[0]], rz = [zs[0]];
                    using CoordinateTransformation inverse = pipeline.GetInverse();
                    inverse.TransformPoints(1, rx, ry, rz);
                    return Hypot(rx[0] - ox, ry[0] - oy, rz[0] - oz);
                }).Match(Succ: Some, Fail: static _ => Option<double>.None);
                var (scale, convergence) = Distortion((x, y) => { double[] p = [x, y, oz]; pipeline.TransformPoint(p); return (p[0], p[1]); }, ox, oy, key);
                return (roundTrip, scale, convergence, epoch);
            });
            bool outOfDomain = outcome.IsSucc && !AllFinite(xs, ys, zs, count);
            if (outcome.IsFail || outOfDomain) {
                return Fin.Fail<Reprojection>(outOfDomain
                    ? new BimFault.Refused(key, BimScope.Semantics, BimReason.Capability, string.Join(':', new object?[] { "crs-out-of-domain", source.Resolution.Key, target.Resolution.Key }))
                    : new BimFault.Refused(key, BimScope.Semantics, BimReason.Capability, string.Join(':', new object?[] { "crs-pair-unreconcilable", source.Resolution.Key, target.Resolution.Key })));
            }
            for (int i = 0, o = 0; i < count; i++, o += stride) {
                (ordinates[o], ordinates[o + 1], ordinates[o + 2]) = (xs[i], ys[i], zs[i]);
            }
            return outcome.Map(o => new Reprojection(GeoEngine.Escalated, count, None, o.RoundTrip, o.Scale, o.Convergence, o.Epoch));
        } finally {
            ArrayPool<double>.Shared.Return(xs);
            ArrayPool<double>.Shared.Return(ys);
            ArrayPool<double>.Shared.Return(zs);
        }
    }

    static EpochPosture Posture(SpatialReference crs, GeoReference frame) =>
        !crs.IsDynamic() ? EpochPosture.Static
        : frame.Epoch.IsSome ? EpochPosture.Modelled
        : EpochPosture.Defaulted;

    static Option<double> RoundTrip(MathTransform forward, ReadOnlySpan<double> shifted, double ox, double oy, double oz, Op key) {
        (double sx, double sy, double sz) = (shifted[0], shifted[1], shifted[2]);
        return key.Catch(() => {
            (double x, double y, double z) = (sx, sy, sz);
            forward.Inverse().Transform(ref x, ref y, ref z);
            return Hypot(x - ox, y - oy, z - oz);
        }).Match(Succ: Some, Fail: static _ => Option<double>.None);
    }

    static (Option<double> Scale, Option<double> Convergence) Distortion(Func<double, double, (double X, double Y)> map, double ox, double oy, Op key) =>
        key.Catch(() => {
            double h = Math.Max(Math.Max(Math.Abs(ox), Math.Abs(oy)), 1.0) * Context.Canonical.For(ToleranceLane.Probe).Value;
            var ((xe, ye), (xw, yw), (xn, yn), (xs, ys)) = (map(ox + h, oy), map(ox - h, oy), map(ox, oy + h), map(ox, oy - h));
            var (dXdx, dYdx, dXdy, dYdy) = ((xe - xw) / (2.0 * h), (ye - yw) / (2.0 * h), (xn - xs) / (2.0 * h), (yn - ys) / (2.0 * h));
            double det = dXdx * dYdy - dXdy * dYdx;
            return double.IsFinite(det) && det != 0.0
                ? (Scale: Some(Math.Sqrt(Math.Abs(det))), Convergence: Some(Math.Atan2(dYdx, dXdx)))
                : (Scale: Option<double>.None, Convergence: Option<double>.None);
        }).IfFail((Option<double>.None, Option<double>.None));

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

    static double Hypot(double dx, double dy, double dz) => double.Hypot(double.Hypot(dx, dy), dz);

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

    public static Fin<Seq<FrameAlignment>> Preflight(Seq<(string Model, GeoReference Frame)> frames, (double X, double Y, double Z) anchor, CancellationToken token, Op key) =>
        toSeq(from i in Enumerable.Range(0, frames.Count)
              from j in Enumerable.Range(i + 1, frames.Count - i - 1)
              select (Source: frames[i], Target: frames[j]))
            .Fold(
                Fin.Succ((Memo: Map<(string Source, string Target), (Fin<Reprojection> Run, Option<double> Shift)>(), Rows: Seq<FrameAlignment>())),
                (held, pair) => held.Bind(state => token.IsCancellationRequested
                    ? Fin.Fail<(Map<(string Source, string Target), (Fin<Reprojection> Run, Option<double> Shift)> Memo, Seq<FrameAlignment> Rows)>(Errors.Cancelled)
                    : Align(pair.Source, pair.Target, anchor, state.Memo, key) switch {
                        var (memo, row) => Fin.Succ((memo, state.Rows.Add(row))),
                    }))
            .Map(static state => state.Rows);

    static string FrameKey(GeoReference frame) =>
        frame.Resolution.Switch(
            epsg: () => $"epsg:{frame.Epsg.IfNone(0)}",
            wkt: () => $"wkt:{frame.Crs.Map(static c => c.Wkt).IfNone("")}",
            projection: () => $"proj:{frame.Crs.Map(static c => $"{c.MapProjection}:{c.MapZone}").IfNone("")}",
            unreferenced: static () => "unreferenced");

    static (Map<(string Source, string Target), (Fin<Reprojection> Run, Option<double> Shift)> Memo, FrameAlignment Row) Align(
        (string Model, GeoReference Frame) source, (string Model, GeoReference Frame) target,
        (double X, double Y, double Z) anchor,
        Map<(string Source, string Target), (Fin<Reprojection> Run, Option<double> Shift)> probes, Op key) {
        if (source.Frame.Resolution == CrsResolution.Unreferenced || target.Frame.Resolution == CrsResolution.Unreferenced) {
            return (probes, new FrameAlignment(source.Model, target.Model,
                new FrameVerdict.Unresolvable(new BimFault.Refused(key, BimScope.Semantics, BimReason.Capability, "crs-unreferenced")), None));
        }
        (string, string) memo = (FrameKey(source.Frame), FrameKey(target.Frame));
        var (cache, probe) = probes.Find(memo).Match(
            Some: hit => (probes, hit),
            None: () => {
                double[] ordinates = [anchor.X, anchor.Y, anchor.Z];
                Fin<Reprojection> run = Reproject(source.Frame, target.Frame, ordinates, 3, key);
                (Fin<Reprojection> Run, Option<double> Shift) fresh =
                    (run, Some(Hypot(ordinates[0] - anchor.X, ordinates[1] - anchor.Y, ordinates[2] - anchor.Z)));
                return (probes.Add(memo, fresh), fresh);
            });
        return (cache, probe.Run.Match(
            Succ: receipt => new FrameAlignment(source.Model, target.Model,
                receipt.Engine == GeoEngine.Identity ? new FrameVerdict.Identical()
                : receipt.Epoch == EpochPosture.Defaulted ? new FrameVerdict.EpochMismatched(receipt)
                : new FrameVerdict.Transformable(receipt),
                probe.Shift),
            Fail: error => new FrameAlignment(source.Model, target.Model, new FrameVerdict.Unresolvable(error), None)));
    }
}
```

## [04]-[RESEARCH]

(none)
