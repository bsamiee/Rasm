# [BIM_COORDINATE_PROJECTION]

The IFC coordinate-reference PROJECTOR over the `Rasm.Element` seam `GeoReference`: `GeoReferenceProjector` FOLDS whatever georeferencing level a model carries onto the seam-owned `GeoReference` record carried on the `ElementGraph` `Header` (and on `Coverage` nodes), and `GeoTransform` is the datum-to-datum reprojection leg over `ProjNET` (escalating an exotic datum-grid or dynamic-datum transform `ProjNET` cannot express to the `MaxRev.Gdal.Core` OSR PROJ engine) operating on raw double-precision ordinate spans of that seam frame (survey eastings never narrow to `float`), returning the typed `Reprojection` receipt — the taken engine route, the shifted vertex count, the anchor evidence (local differential scale, local grid rotation, forward→inverse round-trip residual), and the dynamic-datum epoch posture a survey-grade federation validates its rigid placement against. `Project` switches the single `IfcGeometricRepresentationContext.HasCoordinateOperation` — the `IfcMapConversion`/`IfcMapConversionScaled` over its target CRS (the EPSG-bearing rotation-and-scale LoGeoRef-50 level) or the translation-only `IfcRigidOperation` in its length-measured planar form (the IFC4.3 sibling under `IfcCoordinateOperation`; the angle-measured geographic form is left to the site arm's `Identity` so no ambiguous radian↔degree fold ever mislocates a federation) — falls back to an `IfcSite`'s `RefLatitude`/`RefLongitude`/`RefElevation` geographic position lowered onto a WGS84 (`EPSG:4326`) reference (the LoGeoRef-30 level), and returns `GeoReference.Identity` when a model carries neither so ingest never blocks. The `GeoReference`/`ProjectedCrs` value-objects are SEAM-owned (`Rasm.Element/Geospatial/reference`): the seam owns the full M1 field set — the 11-field record carrying `Eastings`/`Northings`/`OrthogonalHeight`, `XAxisAbscissa`/`XAxisOrdinate`, per-axis `ScaleX`/`ScaleY`/`ScaleZ`, `GeodeticDatum`/`VerticalDatum`, and one `Option<ProjectedCrs> Crs` (the three-state `[ComplexValueObject]` CRS identity carrying the authority `Name`, the inline `Wkt`, and the `MapProjection`/`MapZone` projection identity), with the `Epsg` a DERIVED `Crs.Bind(c => c.Epsg)` projection and the `CrsResolution` mode a derived column — and Bim owns the IFC projection that fills it and the `ProjNET`/OSR transform that consumes it. The seam frame is METRE-NORMALIZED: this projector composes the `IfcProjectedCRS.MapUnit` `IfcNamedUnit.SIFactor()` model-unit↔CRS-unit factor onto the per-axis scale at ingest (the MapUnit fold below IS the seam's stated contract, not a divergence), so the seam doubles arrive in metres and the seam carries NO `MapUnit` field — a federation reconciles every model onto one metre frame from one record. The projector composes the seam `GeoReference.Admit` admission — which owns the URN/authority/`EPSG:` EPSG parse, the WKT/projection resolution, the fault-on-fully-unresolvable, and the 11-field record construction in one hop — and RE-BANDS its seam fault to `BimFault.CapabilityMiss` BARE (band 2600 IS the `Expected` `Code` — no `.ToError()` hop) when a CRS name is present but resolves to neither an EPSG code NOR a WKT NOR a projection+zone, never silently dropping a federation onto an unreferenced frame [M1] and never re-deriving the seam admission (an inline `new GeoReference(...)` construction or a Bim CRS parser beside the seam's) here. The seam `GeoReference` is carried on `Header`/`Coverage` ONLY — it is dropped from the `Object` node — so the model frame is one header fact, not a per-element axis. The seam `GeoReference` carries the rigid map-conversion offset's PARAMETERS (the translation, the `RotationRadians` direction-cosine the seam projects, the per-axis scale) that a DOWNSTREAM host-bound consumer folds into the kernel `Transform` algebra in the Rhino runtime; this projector NEVER materializes a transform, because the kernel `Transform`/`Point3d`/`Vector3d` are RhinoCommon types a host-neutral owner cannot bind. The geodetic datum shift is the `ProjNET`/OSR leg the kernel transform does not own, reprojecting raw ordinate spans between two seam `GeoReference` frames for a federation, resolving EACH frame's `CoordinateSystem` off its own `CrsResolution` (EPSG-keyed cache vs WKT parse) into the one facade `CreateTransformation` build, a mixed EPSG↔WKT federation included. The page is HOST-NEUTRAL: it binds GeometryGym (IFC), `ProjNET`/`MaxRev.Gdal.Core` (managed CRS), the seam, and the host-neutral kernel `Op` key — never RhinoCommon.

## [01]-[INDEX]

- [01]-[GEO_PROJECTION]: `GeoReferenceProjector.Project` folding the model georeferencing level onto the seam `GeoReference` 11-field record — the `HasCoordinateOperation` switch over `IfcMapConversion`/`IfcMapConversionScaled` (LoGeoRef 50, per-axis scale composed with the projected `MapUnit` `SIFactor()` model-unit↔CRS-unit factor) or the translation-only `IfcRigidOperation` length-measured planar form (IFC4.3, `First`→`Eastings`/`Second`→`Northings` metre-normalized, identity direction + unit scale; the angle-measured geographic form deferred to the site arm), the shared `Carriers` read handing the base-CRS `Name`/`GeodeticDatum`/`WellKnownText` + projected-only `VerticalDatum`/`MapProjection`/`MapZone` to the seam three-state `ProjectedCrs` EPSG/WKT/projection resolution (fault-on-fully-unresolvable), the `IfcSite` `RefLatitude`/`RefLongitude`/`RefElevation` WGS84 fallback (LoGeoRef 30), else `Identity` [M1].
- [03]-[GEODETIC_TRANSFORM]: `GeoTransform.Reproject` the datum-to-datum leg over `ProjNET` returning the typed `Reprojection` receipt, resolving EACH frame's `CoordinateSystem` through its `CrsResolution` `Switch` (`ManagedCs`: the EPSG-keyed `CoordinateSystemServices.GetCoordinateSystem` cache / the WKT-keyed `CoordinateSystemFactory.CreateFromWkt` parse) into the ONE facade `CreateTransformation(src, dst)` build — a mixed EPSG↔WKT pair builds managed — then the `.api/api-projnet#CRS_TRANSFORM` strided `double` batch in place on the `Span<double>` ordinate buffer, escalating an exotic datum-grid/dynamic-datum transform `ProjNET` cannot express to the resolution-keyed `MaxRev.Gdal.Core` OSR engine (`ImportFromEPSG`/`ImportFromWkt`, `SetBallparkAllowed(false)` + `SetOnlyBest(true)`); additive (`Reprojection.Identity` on an `Unreferenced` endpoint or an identical CRS), guarding the buffer shape (`stride >= 3`, whole vertices) and the post-transform domain validity (a non-finite shifted ordinate is `crs-out-of-domain`), carrying the central-difference anchor Jacobian `AnchorScale`/`AnchorConvergence`, the inverse round-trip residual, and the `IsDynamic()` `EpochDefaulted` posture on the receipt, and faulting `BimFault.CapabilityMiss` only on a malformed buffer, a projection+zone-only frame neither engine can build (`crs-projection-only-unbuildable`), an out-of-domain vertex, or a differing resolvable pair both engines fail.

## [02]-[GEO_PROJECTION]

- Owner: `GeoReferenceProjector` the static IFC→seam projector folding the georeferencing level the model carries onto the seam `GeoReference` — the single `IfcGeometricRepresentationContext.HasCoordinateOperation` switched over `IfcMapConversion` (or `IfcMapConversionScaled` for per-axis scale) and the translation-only `IfcRigidOperation` (LoGeoRef 50 — the IFC4.3 rigid sibling carries no rotation and no scale, only the two coordinates + `Height` in the target CRS), else the first `IfcSite`'s `RefLatitude`/`RefLongitude`/`RefElevation` geographic position onto a WGS84 (`EPSG:4326`) reference (LoGeoRef 30), else `GeoReference.Identity`. The `GeoReference`/`ProjectedCrs` value-objects are seam-owned (`Rasm.Element/Geospatial/reference`); this page projects the IFC surface onto them — the ONE `Carriers` read handing the base-CRS `Name`/`GeodeticDatum`/`WellKnownText` and the projected-only `VerticalDatum`/`MapProjection`/`MapZone` into the seam's three-state `ProjectedCrs` (an `IfcGeographicCRS` target keeps its base carriers, never a null-CRS drop), and composing the metre-normalized scale and the seam `GeoReference.Admit` (which owns the three-state EPSG/WKT/projection resolution and the 11-field record construction) — never re-declaring them, never re-deriving the admission, never re-minting a CRS parser, and never materializing a kernel transform (host-neutral).
- Entry: `GeoReferenceProjector.Project(IfcProject project, Op key)` projects the model's georeferencing into the seam `GeoReference` — a model with no map conversion AND no geographic site position returns `GeoReference.Identity` so ingest never blocks; an `IfcProjectedCRS` name present but resolving no EPSG FAULTS `Model/faults#FAULT_BAND` `BimFault.CapabilityMiss` BARE (band 2600 IS the `Expected` `Code` — NO `.ToError()` lowering hop) rather than landing the federation on an unreferenced frame [M1]; `Fin<GeoReference>` carries the result; the `Op key` threads from the `Projection/semantic#SEMANTIC_PROJECTOR` `ProjectionContext.Key`, and that projector composes the success onto the `ElementGraph` `Header.Reference`.
- Auto: `Project` is a three-arm fold — the LoGeoRef-50 map-conversion arm reads the `IfcMapConversion` rigid offset (`Eastings`/`Northings`/`OrthogonalHeight`), the `XAxisAbscissa`/`XAxisOrdinate` rotation direction-cosine pair (each defaulting to `double.NaN` when the rotation is unset, coerced to the identity direction `(1,0)` so `RotationRadians` resolves to `0` rather than `Atan2(NaN,NaN)`), and the per-axis scale (an `IfcMapConversionScaled`'s `FactorX`/`FactorY`/`FactorZ`, else the plain conversion's `Scale`/`ScaleY`/`ScaleZ` — each getter already coercing an unset NaN to `1.0`, the degenerate explicit `0.0` coerced to `1.0` so a zero map-conversion scale never collapses geometry) MULTIPLIED by the model-unit↔CRS-unit factor (the projected target's `MapUnit` `IfcNamedUnit.SIFactor()` metre-per-map-unit — `1.0` for a metre CRS, `~0.3048006096` for a US-survey-foot State Plane zone — so a non-metre projected CRS is not silently mislocated [M1]); the LoGeoRef-50 rigid-operation arm reads `IfcRigidOperation.FirstCoordinate`/`SecondCoordinate` (the boxed-`double` `IfcMeasureValue.Value`) in its length-measured planar form ONLY — `First`→`Eastings`/`Second`→`Northings` metre-normalized through the projected `MapUnit` [M1], `Height` (NaN→`0.0`) likewise, identity direction `(1,0)` and unit scale (the rigid operation defines neither) — the angle-measured geographic-target form left to the site arm's `Identity` (its raw radian↔degree convention is unit-ambiguous, and a mislocated federation is worse than an ungeoreferenced ingest); BOTH operation arms hand the offset, rotation pair, scale, datum names, and CRS strings through the ONE `Carriers` read — the base-CRS `Name` (GeometryGym `"Unknown"` sentinel normalized to blank)/`GeodeticDatum`/`WellKnownText.WellKnownText` plus the projected-only `VerticalDatum`/`MapProjection`/`MapZone` — to the seam `GeoReference.Admit`; the LoGeoRef-30 arm folds an `IfcSite`'s `RefLatitude`/`RefLongitude` `IfcCompoundPlaneAngleMeasure` through `.Angle()` to decimal degrees and reads `RefElevation` (NaN→`0.0`), then hands the literal `EPSG:4326` authority name with blank `wkt`/`mapProjection`/`mapZone` to the same `Admit` for a WGS84 reference (longitude east, latitude north, identity rotation, unit scale). Inside `Admit` the seam builds the three-state `ProjectedCrs` and `ProjectedCrs.Epsg` resolves the EPSG code from the authority `Name` across the OGC URN (`urn:ogc:def:crs:EPSG::25832`), the authority form (`EPSG:25832`), and a bare numeric code, while a WKT-defined CRS (blank/unresolvable `Name`, non-blank `Wkt`) or a projection+zone CRS resolves WITHOUT an EPSG; the projector first normalizes the GeometryGym empty-name sentinel `"Unknown"` (its `IfcCoordinateReferenceSystem.Name` setter coerces an empty value to `"Unknown"`) back to blank so `Admit` reads it as the no-CRS state, a name present yet resolving to NO identity at all (no EPSG, no WKT, no projection+zone) faulting (the seam `ElementFault` re-banded to `BimFault.CapabilityMiss`) rather than degrading to a no-op transform, a WKT-resolvable CRS never faulted.
- Receipt: the seam `GeoReference` is the coordinate-reference evidence the `Header` carries (and the `Semantics/geospatial#RASTER_INGEST` `Coverage` node carries for a georeferenced raster); its parameters (the translation, the seam-projected `RotationRadians` direction-cosine, the per-axis metre-frame scale) feed a DOWNSTREAM host-bound kernel `Transform` consumer in the Rhino runtime, never a transform this host-neutral projector builds; the seam `CrsResolution` mode (the derived `Epsg`-vs-`Wkt` column) drives the `[03]-[GEODETIC_TRANSFORM]` `ProjNET`/OSR datum leg's EPSG-keyed-vs-WKT-keyed build-path selection.
- Packages: GeometryGymIFC_Core, Rasm.Element, Rasm (the host-neutral `Rasm.Domain.Op` key only), LanguageExt.Core
- Growth: a new map-conversion parameter is one column on the seam `GeoReference` (the seam's, not this page's); a new georeferencing level is one arm on the `Project` fold (a future LoGeoRef-40 `WorldCoordinateSystem`/`TrueNorth` rotation folds onto the existing rotation field); a new CRS-name scheme is one arm on the seam `ProjectedCrs.Epsg`; the rigid offset is the downstream host-bound kernel transform's and the datum shift is the `[03]-[GEODETIC_TRANSFORM]` leg; never a new transform owner, never a Bim CRS parser, never a per-CRS class.
- Boundary: the `GeoReference`/`ProjectedCrs` value-objects are SEAM-owned [M1] and re-declaring them in Bim is the named drift defect — this page is the IFC projector that fills the seam value, never its owner; the seam `GeoReference` is the 11-field record (`Eastings`/`Northings`/`OrthogonalHeight`, `XAxisAbscissa`/`XAxisOrdinate`, per-axis `ScaleX`/`ScaleY`/`ScaleZ`, `GeodeticDatum`/`VerticalDatum`, and one `Option<ProjectedCrs> Crs`) with `Epsg` a DERIVED `Crs.Bind(c => c.Epsg)` property (NOT a stored slot), so constructing a `GeoReference` with a stored `Epsg` slot, a `ProjectedCrsName` field, or any column the seam does not declare is the deleted form; the seam `GeoReference` is carried on `Header`/`Coverage` ONLY and a `GeoReference` on the `Object` node is the deleted form [M1]; the projector composes the seam `GeoReference.Admit` (the one admission owning the three-state EPSG/WKT/projection resolution via the `ProjectedCrs` `[ComplexValueObject]`, the fault-on-fully-unresolvable, and the 11-field record construction) and re-deriving that admission inline (an `new GeoReference(...)` construction with an inline CRS parse beside `Admit`) OR a hand-rolled Bim CRS parser is the deleted form; the per-axis scale reads `IfcMapConversion.Scale`/`ScaleY`/`ScaleZ` (or `IfcMapConversionScaled.FactorX`/`FactorY`/`FactorZ`) MULTIPLIED by the `IfcProjectedCRS.MapUnit` `SIFactor()` model-unit↔CRS-unit factor [M1] so the doubles handed to `Admit` are METRE-NORMALIZED (the seam frame's stated contract), and folding a single `Scale` onto all three axes drops the per-axis map distortion while ignoring the `MapUnit` factor silently mislocates a non-metre projected CRS (a US-survey-foot State Plane zone) — both named defects, the unit reconciliation composed HERE at ingest so the seam record carries metre doubles and NO `MapUnit` field (a CRS-native-unit double on the seam is the rejected form); the three CRS carriers (the authority `Name`, the inline `WellKnownText.WellKnownText`, and `MapProjection`/`MapZone`) are ALL read off the GeometryGym surface and handed to `Admit` so a WKT-defined or projection-defined CRS resolves to its seam state — dropping the `Wkt`/`MapProjection`/`MapZone` carry and handing only the `Name` (the deleted two-state slice that false-faults a GIS-origin WKT CRS as unresolvable) is the named defect; the unset `XAxisAbscissa`/`XAxisOrdinate` (`double.NaN`) coerces to the identity direction `(1,0)` and reading the raw NaN into the rotation is the named defect; the projection rides the GeometryGym `IfcMapConversion`/`IfcMapConversionScaled`/`IfcProjectedCRS`/`IfcWellKnownText`/`IfcSite` surface consumed as settled vocabulary (`.api/api-geometrygym-ifc` georeferencing entities), a hand-rolled IFC reader the deleted form; the rigid map-conversion offset is NOT built here — the kernel `Transform`/`Point3d`/`Vector3d` are RhinoCommon types and composing them on this host-neutral projector is the named host-neutrality defect, the seam `GeoReference` carrying the offset parameters a downstream host-bound consumer folds; a CRS name present but resolving to NO identity at all (no EPSG, no WKT, no projection+zone) FAULTS `BimFault.CapabilityMiss` BARE [M1] (a `.ToError()` lowering hop or a keyless `new BimFault.CapabilityMiss(detail)` construction the named defect this aligns to `Model/faults#FAULT_BAND`) while a WKT-resolvable CRS is VALID and silently landing the model on an unreferenced frame is the named defect; a non-georeferenced model returns `GeoReference.Identity` so ingest never blocks on a missing CRS.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Buffers;
using GeometryGym.Ifc;
using LanguageExt;
using OSGeo.OSR;
using ProjNet;
using ProjNet.CoordinateSystems.Transformations;
using Rasm.Element;
using Op = Rasm.Domain.Op;                          // the host-neutral kernel operation key; NEVER the Rhino-bound kernel geometry
using static LanguageExt.Prelude;

namespace Rasm.Bim;

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class GeoReferenceProjector {
    // The model georeferencing fold onto the seam GeoReference: switch the single HasCoordinateOperation — an
    // IfcMapConversion(Scaled) (LoGeoRef 50, rotation+scale) OR the translation-only IfcRigidOperation (the IFC4.3
    // rigid sibling under IfcCoordinateOperation, First/Second/Height in a PROJECTED target — the length-measured planar
    // offset; an angle-measured geographic-target rigid operation is left to the site arm's Identity so no ambiguous
    // radian<->degree fold ever mislocates a federation) — else the IfcSite RefLatitude/RefLongitude/RefElevation
    // geographic position onto a WGS84 (EPSG:4326) reference (LoGeoRef 30), else Identity so ingest never blocks. Every
    // arm COMPOSES the seam GeoReference.Admit (the seam owns the three-state EPSG/WKT/projection resolution +
    // fault-on-fully-unresolvable + 11-field record construction); Bim only reads the IFC surface through the ONE
    // Carriers read, normalizes the GeometryGym "Unknown" empty-name sentinel, and re-bands Admit's seam fault.
    public static Fin<GeoReference> Project(IfcProject project, Op key) =>
        Optional(project.RepresentationContexts
                .OfType<IfcGeometricRepresentationContext>()
                .Select(static ctx => ctx.HasCoordinateOperation)
                .FirstOrDefault(static op => op is IfcMapConversion || op is IfcRigidOperation { FirstCoordinate: IfcLengthMeasure }))
            .Match(
                Some: op => op switch {
                    IfcMapConversion conversion => FromMapConversion(conversion, key),
                    IfcRigidOperation rigid     => FromRigidOperation(rigid, key),
                    _                           => Fin.Succ(GeoReference.Identity),
                },
                None: () => Optional(project.Extract<IfcSite>().FirstOrDefault())
                    .Match(Some: site => FromSite(site, key), None: static () => Fin.Succ(GeoReference.Identity)));

    static Fin<GeoReference> FromMapConversion(IfcMapConversion conversion, Op key) {
        // IfcMapConversionScaled carries FactorX/Y/Z; a plain IfcMapConversion carries per-axis Scale/ScaleY/ScaleZ
        // (each getter already coercing an unset NaN to 1.0). NonZero coerces the degenerate explicit 0.0 to 1.0.
        // The IfcMapConversion scale converts MODEL length units to MAP (CRS-native) units, and IfcProjectedCRS.MapUnit
        // declares that native unit — metre for the overwhelming majority of EPSG projected CRS, US-survey-foot for some
        // State Plane zones [§4-RT M1]. The seam GeoReference doubles are metre-normalized, so the MODEL-UNIT<->CRS-UNIT
        // factor (the IfcNamedUnit.SIFactor() polymorphic metre-per-mapunit multiplier — 1.0 metre, ~0.3048006096
        // US-survey-foot) folds onto the per-axis scale so a non-metre projected CRS is NOT silently mislocated.
        double mapUnitToMetre = MetrePerMapUnit((conversion.TargetCRS as IfcProjectedCRS)?.MapUnit);
        var (sx, sy, sz) = conversion is IfcMapConversionScaled scaled
            ? (NonZero(scaled.FactorX) * mapUnitToMetre, NonZero(scaled.FactorY) * mapUnitToMetre, NonZero(scaled.FactorZ) * mapUnitToMetre)
            : (NonZero(conversion.Scale) * mapUnitToMetre, NonZero(conversion.ScaleY) * mapUnitToMetre, NonZero(conversion.ScaleZ) * mapUnitToMetre);
        // XAxisAbscissa/XAxisOrdinate default to NaN when the rotation is unset — coerce the pair to the identity
        // direction (1,0) so the seam RotationRadians resolves to 0 rather than Atan2(NaN,NaN).
        double abscissa = double.IsNaN(conversion.XAxisAbscissa) ? 1.0 : conversion.XAxisAbscissa;
        double ordinate = double.IsNaN(conversion.XAxisOrdinate) ? 0.0 : conversion.XAxisOrdinate;
        return Admit(conversion.Eastings * mapUnitToMetre, conversion.Northings * mapUnitToMetre, conversion.OrthogonalHeight * mapUnitToMetre,
            abscissa, ordinate, sx, sy, sz, conversion.TargetCRS, key);
    }

    // LoGeoRef 50, the IFC4.3 rigid sibling: IfcRigidOperation carries only First/Second/Height in the target CRS (no
    // rotation, no scale — identity direction (1,0), unit scale). Project admits ONLY the length-measured planar form
    // (First->Eastings, Second->Northings, both metre-normalized through the projected MapUnit), so IfcMeasureValue.Value
    // is a planar length here; the angle-measured geographic form is intentionally not folded (its radian-vs-degree
    // convention is unit-ambiguous, and a mislocated federation is worse than the Identity the site arm yields).
    static Fin<GeoReference> FromRigidOperation(IfcRigidOperation rigid, Op key) {
        double metre = MetrePerMapUnit((rigid.TargetCRS as IfcProjectedCRS)?.MapUnit);
        return Admit(
            Convert.ToDouble(rigid.FirstCoordinate.Value) * metre, Convert.ToDouble(rigid.SecondCoordinate.Value) * metre,
            (double.IsNaN(rigid.Height) ? 0.0 : rigid.Height) * metre, 1.0, 0.0, 1.0, 1.0, 1.0, rigid.TargetCRS, key);
    }

    // LoGeoRef 30: the IfcSite geographic position onto a WGS84 (EPSG:4326) reference — RefLatitude/RefLongitude are
    // IfcCompoundPlaneAngleMeasure (deg/min/sec/micro), folded to decimal degrees by .Angle(); a site missing either
    // angle is ungeoreferenced (Identity), longitude landing Eastings and latitude Northings in the geographic frame.
    // The seam Admit resolves the literal EPSG:4326 by authority code with no fault, so this arm passes the three CRS
    // strings blank — the WGS84 reference is EPSG-resolved, not WKT/projection-defined.
    static Fin<GeoReference> FromSite(IfcSite site, Op key) =>
        site.RefLatitude is null || site.RefLongitude is null
            ? Fin.Succ(GeoReference.Identity)
            : GeoReference.Admit(
                site.RefLongitude.Angle(), site.RefLatitude.Angle(), double.IsNaN(site.RefElevation) ? 0.0 : site.RefElevation,
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

    // The metre-per-map-unit factor: the IfcNamedUnit.SIFactor() polymorphic multiplier (1.0 metre, ~0.3048006096
    // US-survey-foot); an absent unit defaults 1.0. NonZero guards a degenerate 0 factor so it never collapses geometry.
    static double MetrePerMapUnit(IfcNamedUnit? unit) => unit is null ? 1.0 : NonZero(unit.SIFactor());

    // The degenerate explicit 0.0 scale coerces to 1.0 so a zero map-conversion scale never collapses geometry.
    static double NonZero(double value) => value == 0.0 ? 1.0 : value;
}
```

## [03]-[GEODETIC_TRANSFORM]

- Owner: `GeoTransform` the datum-bridging leg reprojecting raw ordinate spans between two seam `GeoReference` frames — EACH frame resolves its `ProjNET` `CoordinateSystem` off its OWN seam `CrsResolution` (`ManagedCs`: the `Epsg` arm the facade's cached `GetCoordinateSystem(srid)`, the `Wkt` arm the shared `CoordinateSystemFactory.CreateFromWkt` — a GIS-origin CRS with no authority code, where `Epsg` is `None` so an SRID-only build would silently no-op the federation), the ONE facade `CoordinateSystemServices.CreateTransformation(src, dst)` building the managed transform for EVERY resolvable pair (both-EPSG, both-WKT, and the MIXED EPSG↔WKT federation alike) — escalating an exotic datum-grid or dynamic-datum transform `ProjNET` cannot express to the `MaxRev.Gdal.Core` OSR PROJ engine (keyed by `ImportFromEPSG` or `ImportFromWkt` to match the frame's resolution) per the `.api/api-projnet` escalation-seam; `CoordinateServices` the one process-wide `CoordinateSystemServices` (the SRID-keyed CS cache AND the CS-pair transformation build) and `CsFactory` the one WKT parser the `.api/api-projnet` `CRS_TRANSFORM` law names as the single owners. The leg operates on the seam `GeoReference` frame and a `ProjNET`/OSR datum shift folded onto the kernel transform is the named seam violation.
- Entry: `GeoTransform.Reproject(GeoReference source, GeoReference target, Span<double> ordinates, int stride, Op key)` applies the datum-to-datum transform IN PLACE on the interleaved double ordinate buffer when both frames carry a resolvable CRS (EPSG or WKT) that differs, returning `Fin<Reprojection>` — the typed receipt carrying the engine route, the shifted-vertex count, the forward→inverse round-trip residual, the central-difference anchor `AnchorScale`/`AnchorConvergence` distortion evidence, and the dynamic-datum `EpochDefaulted` posture: the additive cases (a source or target `CrsResolution.Unreferenced`, an identical CRS, or fewer than one full vertex) return `Reprojection.Identity` so the datum leg never blocks a single-datum federation; a `CrsResolution.Wkt` frame whose `Wkt` text is empty (the seam's projection+zone-only state) faults `crs-projection-only-unbuildable` by name — neither engine builds from a bare projection identity; a differing, resolvable pair resolves EACH frame's `CoordinateSystem` through its `Resolution` generated total `Switch` (`ManagedCs`) into the ONE facade `CreateTransformation(src, dst)` managed build (a mixed EPSG↔WKT pair included), runs the strided batch once, escalates to the matching OSR build (`ImportFromEPSG`/`ImportFromWkt`) when `ProjNET` cannot express the transform, and faults `BimFault.CapabilityMiss` BARE only when BOTH engines fail. The buffer is `double` end to end — a survey easting never narrows to `float` (a float32 round-trip drops sub-metre precision on a six-figure easting; the `Semantics/geospatial#GEOSPATIAL_SEAM` precision contract) — and the NTS `CoordinateSequence` flatten plus the `Geometry.Apply` write-back is the geospatial CONSUMER's marshalling, never this owner's, so the leg stays geometry-library-neutral over raw ordinates. Composed BEFORE the downstream host-bound rigid map-conversion offset so a federated model lands in the shared datum before its local-engineering placement applies.
- Auto: `Reproject` short-circuits when either frame is `CrsResolution.Unreferenced`, when the two CRS identities are equal (same EPSG, or same `Crs` value), or when the buffer holds fewer than one full vertex; otherwise EACH frame resolves its `CoordinateSystem` through its own `Resolution` generated total `Switch` (`ManagedCs` — the `Epsg` arm the facade's cached `GetCoordinateSystem(srid)`, the `Wkt` arm `CsFactory.CreateFromWkt(wkt)`, the `Unreferenced` arm unreachable here since the short-circuit already returned) and the ONE facade `CoordinateServices.CreateTransformation(srcCS, dstCS).MathTransform` builds the managed transform — both-EPSG, both-WKT, and the mixed EPSG↔WKT pair through one build. The `ProjNET` build is lifted through `Try` so a SRID absent from the bundled `SRID.csv`, a WKT `ProjNET` cannot parse, or a datum `ProjNET` cannot express routes the OSR escalation rather than throwing across the boundary; the `ProjNET` apply is the `.api/api-projnet#CRS_TRANSFORM` strided `double` batch run DIRECTLY on the interleaved buffer IN PLACE — a single `MathTransform.Transform(ordinates, ordinates[1..], ordinates[2..], stride, stride, stride)` call over the three ordinate columns of that one `Span<double>`, no staging copy (the buffer is already `double`, so there is no widen/narrow and no `MemoryMarshal.Cast<float,double>` to misread the bytes) and the `TransformCore` `while (num < xs.Length)` walk drives the count off the full-length first column so the last vertex is covered, a `stride` above three leaving the non-position interleave columns untouched; the OSR escalation deinterleaves the position columns into pooled `double[]` x/y/z, runs the one `Semantics/geospatial#RASTER_INGEST` `GeoGdal.Bootstrap` idempotent guard (`GdalBase.ConfigureAll` + `Osr.UseExceptions`), builds two `SpatialReference` keyed to match each frame's resolution (`ImportFromEPSG` for an EPSG frame, `ImportFromWkt` for a WKT frame, `OAMS_TRADITIONAL_GIS_ORDER` pinning lon/lat against the GDAL-3 axis swap) and one `CoordinateTransformation` under the two options gates (`SetBallparkAllowed(false)` — a gridless pair faults, never a coarse ballpark shift; `SetOnlyBest(true)` — a missing best-accuracy operation faults, never a silent lower-accuracy fallback), records either frame's `IsDynamic()` onto the receipt's `EpochDefaulted`, runs one `TransformPoints(count, xs, ys, zs)`, and reinterleaves; on BOTH engines the receipt evidence rides the same shifted anchor — the `GetInverse`/`Inverse()` round-trip residual and the `Distortion` central-difference Jacobian probe are inner-`Try` recorded absences (`NaN`), never leg faults; the datum shift composes BEFORE the rigid offset so a model lands in the shared datum before its local-engineering-frame placement applies.
- Packages: ProjNET, MaxRev.Gdal.Core, Rasm.Element, Rasm, LanguageExt.Core
- Growth: a new EPSG, WKT, or mixed CRS pair is the per-frame `ManagedCs` resolution joined by the one facade `CreateTransformation` (the `CoordinateSystemServices` cache resolving EPSG frames from the bundled `SRID.csv`, `CsFactory` parsing WKT frames), never a per-call factory; a new CRS-resolution mode is one arm on the seam `CrsResolution` that breaks this `Switch` at compile time (the seam owns the discriminant, this leg owns the per-mode build); an exotic datum-grid or dynamic datum is the OSR PROJ pipeline's, resolved from the EPSG code or the WKT, never a hand-rolled Bursa-Wolf matrix; a float-buffered consumer widens to `double` at its OWN boundary and calls the one `Span<double>` leg, never a parallel `Span<float>` overload re-admitting the survey-precision-loss footgun; a denser batch is one `MathTransform`/`CoordinateTransformation` overload swap, never a second transform owner and never a per-vertex `ref` loop; a new PROJ pipeline gate is one `CoordinateTransformationOptions` setter row on the one OSR options build, never a second pipeline owner; the coordinate epoch is the seam's — a `GeoReference` epoch column the OSR leg threads through `SpatialReference.SetCoordinateEpoch` once it lands, never a Bim-local epoch knob (until then a dynamic frame records `EpochDefaulted`); a new receipt evidence column is one `Reprojection` field fed by the shared anchor probes, never a per-engine receipt sibling.
- Boundary: the datum reprojection is `ProjNET`'s by default — the per-frame `ManagedCs` `CoordinateSystem` resolution (each frame's seam `CrsResolution` selecting `GetCoordinateSystem(srid)` or `CsFactory.CreateFromWkt`) joined by the ONE `CoordinateSystemServices.CreateTransformation(src, dst)` facade build, and the `MathTransform` Bursa-Wolf 7-parameter datum shift plus projection own the managed transform — escalating to the `MaxRev.Gdal.Core` OSR `SpatialReference`/`CoordinateTransformation` PROJ pipeline (`ImportFromEPSG` or `ImportFromWkt` matching the frame's resolution, the full datum-grid set under `SetBallparkAllowed(false)` + `SetOnlyBest(true)`, either frame's `IsDynamic()` recorded onto `EpochDefaulted` — epoch-aware plate-motion lands when the seam frame carries a coordinate epoch for `SetCoordinateEpoch`, never a Bim-local epoch) for what the managed algebra cannot express, and a hand-rolled datum shift, a per-CALL `new CoordinateTransformationFactory()`/`CoordinateTransformation` rebuild outside the two shared owners (`CoordinateServices`, `CsFactory`), or OSR for a transform `ProjNET` already covers is the deleted form per the `.api/api-projnet` single-cache-owner + escalation-seam law; branching the build off a re-spelled `Epsg.IsSome` check (the COLLAPSE_SCAN re-branch the seam forbids) rather than the seam `CrsResolution` `Switch` is the deleted form, reading only `source.Epsg`/`target.Epsg` so a WKT-only federation silently no-ops (both `Epsg` `None`) is the named defect this leg closes, and a source-only build branch that escalates a MIXED EPSG↔WKT pair to OSR when the per-frame `ManagedCs` + facade build already expresses it is the same deleted form; the `ProjNET` apply is the strided `double` batch run in place on the `Span<double>` and a per-vertex `Transform(ref x, ref y, ref z)` loop OR narrowing the survey ordinates to `float` (the precision-loss defect the geospatial seam forbids) is the rejected form; the GDAL bootstrap is the one `Semantics/geospatial#RASTER_INGEST` `GeoGdal.Bootstrap` idempotent guard and a second `GdalBase.ConfigureAll` owner is the deleted form; the leg is additive — a frame's `CrsResolution.Unreferenced` or an identical CRS returns `Reprojection.Identity` so `Reproject` never blocks ingest — and faults `BimFault.CapabilityMiss` BARE only on a malformed buffer, a projection+zone-only frame (`crs-projection-only-unbuildable` — named BEFORE two doomed engine builds, since neither `CreateFromWkt` nor `ImportFromWkt` builds from a bare projection identity), an out-of-domain vertex, or a differing resolvable pair that defeats both engines (the `Op key` carrying the operation context, never a `.ToError()` hop); reading `MathTransform.Derivative`/`GetDomainFlags` or `ICoordinateTransformation.AreaOfUse` for the distortion evidence or the domain guard is the phantom form (base-only `NotImplementedException`, factory-empty `AreaOfUse` — decompile-verified) — the `Distortion` central-difference anchor probe and the engine-agnostic non-finite scan are the honest owners, and a receipt asserting scale/convergence evidence it never probed is the illusory form this receipt closes; the reprojection composes BEFORE the downstream host-bound rigid map-conversion offset so the kernel transform stays datum-free, and folding the rigid offset into this datum leg is the named defect; the page reprojects raw `Span<double>` ordinate buffers — the NTS `CoordinateSequence` flatten and the `Geometry.Apply` write-back are the geospatial CONSUMER's marshalling, so a `GeoTransform` overload binding an NTS `Geometry`/`CoordinateSequence` is the misplaced-concern form — and a RhinoCommon geometry type crossing this leg is the host-bound defect.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using ProjNet.CoordinateSystems;                    // CoordinateSystemFactory + the CoordinateSystem currency ManagedCs resolves per frame

// --- [TYPES] -------------------------------------------------------------------------------
// The engine a reprojection took — the receipt route the .api/algorithms receipt law records so a federation reads
// WHICH datum engine reconciled a frame pair (a survey audit distinguishes a managed ProjNET planar shift from a
// PROJ grid-shifted OSR escalation): Identity (no shift — an unreferenced/equal frame), Managed (the ProjNET
// MathTransform), Escalated (the GDAL OSR PROJ pipeline). Keyed for telemetry, never a bool.
[SmartEnum<string>]
public sealed partial class GeoEngine {
    public static readonly GeoEngine Identity  = new("identity");
    public static readonly GeoEngine Managed   = new("managed");
    public static readonly GeoEngine Escalated = new("escalated");
}

// --- [MODELS] ------------------------------------------------------------------------------
// The typed datum-leg receipt (never a bare Unit): the engine route, the shifted-vertex count, the forward->inverse
// round-trip residual in the frame's native ordinate unit, the central-difference anchor Jacobian distortion pair, and
// the dynamic-datum epoch posture — the evidence a survey-grade federation validates its rigid placement against.
// RoundTripResidual rides ProjNET MathTransform.Inverse() (21 concrete overrides, decompile-verified) or the OSR
// CoordinateTransformation.GetInverse() reverse pipeline on a probe vertex; NaN when the pipeline is non-invertible —
// a recorded absence, never a fabricated 0. AnchorScale = sqrt(|det J|) of the Distortion probe's 2x2 anchor Jacobian
// (the local areal-scale root — the survey point-scale-distortion evidence for a like-unit projected pair, a unit-ratio
// across mixed-unit frames); AnchorConvergence = atan2(dYdx, dXdx) (the local grid rotation of the transformed source
// x-axis — the meridian-convergence evidence at a federation origin); both NaN when the probe is refused or non-finite.
// The Jacobian is PROBED because the package surfaces are phantoms (decompile-verified): MathTransform.Derivative/
// GetDomainFlags/GetCodomainConvexHull throw NotImplementedException with ZERO concrete overrides, and every
// factory-built ICoordinateTransformation.AreaOfUse is string.Empty — so the domain guard is the engine-agnostic
// post-transform non-finite scan and the distortion evidence is a central-difference probe, never a phantom call.
// EpochDefaulted: the OSR escalation's PROJ report flags either frame SpatialReference.IsDynamic() and the seam frame
// carries no coordinate epoch to thread — the shift is reference-epoch-defaulted, the plate-motion term unmodelled
// until the seam GeoReference epoch column lands for SetCoordinateEpoch; false on a static OSR pair AND on the managed
// route, whose planar algebra carries no epoch model (the epoch question rides the escalated route's receipt only).
public readonly record struct Reprojection(
    GeoEngine Engine, int ShiftedVertices, double RoundTripResidual,
    double AnchorScale, double AnchorConvergence, bool EpochDefaulted) {
    public static readonly Reprojection Identity = new(GeoEngine.Identity, 0, 0.0, 1.0, 0.0, false);
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class GeoTransform {
    static readonly CoordinateSystemServices CoordinateServices = new();         // the ONE ProjNET owner: the EPSG/SRID CS cache + the CS-pair CreateTransformation build
    static readonly CoordinateSystemFactory CsFactory = new();                   // the one shared WKT->CoordinateSystem parser

    // The datum leg over two seam GeoReferences: a source-to-target reprojection of an interleaved DOUBLE-precision
    // ordinate Span<double> IN PLACE — survey eastings/northings never narrow to float (a float32 round-trip loses
    // sub-metre precision on a 500_000 m easting), the Semantics/geospatial#GEOSPATIAL_SEAM GeoFeature.Reproject
    // consumer flattening its NTS CoordinateSequence into the double buffer and writing the shifted ordinates back
    // through Geometry.Apply on its side. The build path is the source CrsResolution case — the EPSG-keyed SRID facade
    // OR the WKT-keyed CreateFromWkt build (a GIS-origin WKT CRS carries no EPSG, so reading only Epsg silently no-ops
    // the federation; branching the seam CrsResolution Switch closes that). Additive — a frame's Unreferenced, an
    // identical CRS, or zero whole vertices returns Reprojection.Identity. ProjNET is the default managed engine; an
    // exotic datum-grid/dynamic-datum transform ProjNET cannot express escalates to the resolution-keyed GDAL OSR
    // build. Faults BimFault.CapabilityMiss bare on a malformed buffer (stride < 3), a projection+zone-only frame
    // (empty Wkt — neither engine builds from a bare projection identity), an out-of-domain vertex (a non-finite
    // shifted ordinate — the engine-agnostic domain guard), or a differing resolvable pair both engines fail.
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
        // The seam's third resolvable state — a projection+zone-only CRS (Wkt mode, EMPTY Wkt string) — is admissible on
        // the seam yet BUILDABLE by neither engine (CreateFromWkt and ImportFromWkt both need the WKT text): fault it by
        // NAME before two doomed engine builds, so the federation audit reads the real gap (a WKT synthesis from the
        // projection identity is unbuilt), never a conflated crs-pair-unreconcilable.
        if ((ProjectionOnly(source) | ProjectionOnly(target)).Case is string gap) {
            return Fin.Fail<Reprojection>(new BimFault.CapabilityMiss(key, $"crs-projection-only-unbuildable:{gap}"));
        }
        // The managed build: EACH frame resolves its CoordinateSystem off its OWN seam CrsResolution Switch
        // (ManagedCs), then the ONE facade CreateTransformation(src, dst) builds the transform — so a MIXED
        // EPSG<->WKT federation builds MANAGED (OSR for a transform ProjNET already covers is this page's own
        // deleted form; the retired source-only two-arm branch escalated every mixed pair). Lifted through Try so
        // a SRID absent from SRID.csv, a WKT ProjNET cannot parse, or a datum it cannot express lifts onto the
        // rail (no throw crossing the boundary) and the matching OSR escalation runs.
        MathTransform? managed = Try.lift(() =>
                from src in ManagedCs(source)
                from dst in ManagedCs(target)
                select CoordinateServices.CreateTransformation(src, dst).MathTransform)
            .Run().Match(Succ: static t => t.IfNoneUnsafe(() => null!), Fail: static _ => (MathTransform?)null);
        if (managed is null) {
            return Osr(source, target, ordinates, stride, key);
        }
        // The .api/api-projnet#CRS_TRANSFORM dense rail: ONE strided batch over the three ordinate columns of the
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

    // The projection+zone-only detector: a Wkt-resolution frame with no WKT text — the seam ProjectedCrs state whose
    // transform build this leg cannot yet express; the gap string names the projection identity for the fault detail.
    static Option<string> ProjectionOnly(GeoReference frame) =>
        frame.Resolution == CrsResolution.Wkt
            ? frame.Crs.Bind(static c => c.Wkt.Length == 0 ? Some($"{c.MapProjection}:{c.MapZone}") : Option<string>.None)
            : Option<string>.None;

    // Per-frame managed CS resolution keyed by the seam CrsResolution (the seam owns the discriminant, this leg owns
    // the per-mode build — never a re-spelled Epsg.IsSome re-branch): Epsg reads the facade's cached
    // GetCoordinateSystem(srid), Wkt the one shared CreateFromWkt parser (the projection+zone-only state was already
    // faulted by name, so this arm never parses an empty string), Unreferenced unreachable (the short-circuit returned).
    static Option<CoordinateSystem> ManagedCs(GeoReference frame) =>
        frame.Resolution.Switch(
            epsg: () => frame.Epsg.Map(CoordinateServices.GetCoordinateSystem),
            wkt: () => frame.Crs.Map(static c => CsFactory.CreateFromWkt(c.Wkt)),
            unreferenced: static () => Option<CoordinateSystem>.None);

    // The exotic datum escalation: GDAL OSR carries PROJ's full datum-grid + dynamic-datum pipeline ProjNET's managed
    // algebra cannot. TWO CoordinateTransformationOptions gates (both decompile-verified): SetBallparkAllowed(false) —
    // a low-accuracy ballpark shift (no PROJ grid for the pair) FAULTS rather than silently returning a coarse survey
    // result — and SetOnlyBest(true) — a best-accuracy operation whose grid is uninstantiable FAULTS rather than
    // silently degrading to the next-best pipeline. IsDynamic() on either frame records EpochDefaulted: the seam frame
    // carries no coordinate epoch yet, so a dynamic-datum shift is reference-epoch-defaulted — receipt evidence, never
    // a block. OSR's TransformPoints takes struct-of-arrays double columns, so the interleaved buffer deinterleaves
    // into pooled double x/y/z, transforms, and reinterleaves (no float anywhere). Each SpatialReference is keyed to
    // MATCH its frame's resolution — ImportFromEPSG for an EPSG frame, ImportFromWkt for a WKT frame. The build lifts
    // through Try so a missing RID runtime, an EPSG no PROJ grid covers, or an unparseable WKT surfaces as
    // BimFault.CapabilityMiss; a non-finite shifted ordinate is the out-of-domain fault; the GetInverse round-trip and
    // the Distortion probe are INNER Try recorded absences (NaN) — a non-invertible or probe-refusing pipeline never
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
            Fin<(double RoundTrip, double Scale, double Convergence, bool EpochDefaulted)> outcome = Try.lift(() => {
                GeoGdal.Bootstrap();
                using SpatialReference src = Crs(source);
                using SpatialReference dst = Crs(target);
                using var options = new CoordinateTransformationOptions();
                options.SetBallparkAllowed(false);                              // a gridless survey pair FAULTS, never a coarse ballpark shift
                options.SetOnlyBest(true);                                      // a missing best-accuracy grid FAULTS, never a silent degradation
                bool epochDefaulted = src.IsDynamic() || dst.IsDynamic();       // dynamic frame, no seam epoch: reference-epoch-defaulted shift
                using var pipeline = new CoordinateTransformation(src, dst, options);
                pipeline.TransformPoints(count, xs, ys, zs);
                // SEPARATE probe arrays through the reverse pipeline (GetInverse, decompile-verified) so the forward
                // result the reinterleave reads stays intact; a throwing inverse records NaN through the inner Try.
                double roundTrip = Try.lift(() => {
                    double[] rx = [xs[0]], ry = [ys[0]], rz = [zs[0]];
                    using CoordinateTransformation inverse = pipeline.GetInverse();
                    inverse.TransformPoints(1, rx, ry, rz);
                    return Hypot(rx[0] - ox, ry[0] - oy, rz[0] - oz);
                }).Run().IfFail(double.NaN);
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
    // pipeline whose Inverse throws records NaN (a recorded absence, never a fabricated 0), lifted through Try.
    static double RoundTrip(MathTransform forward, ReadOnlySpan<double> shifted, double ox, double oy, double oz) =>
        Try.lift(() => {
            (double x, double y, double z) = (shifted[0], shifted[1], shifted[2]);
            forward.Inverse().Transform(ref x, ref y, ref z);
            return Hypot(x - ox, y - oy, z - oz);
        }).Run().IfFail(double.NaN);

    // The anchor distortion probe — the honest replacement for the phantom MathTransform.Derivative/GetDomainFlags
    // (base NotImplementedException, ZERO concrete overrides; every factory-built ICoordinateTransformation.AreaOfUse
    // string.Empty — decompile-verified): a central-difference 2x2 Jacobian at the SOURCE anchor over the SAME engine
    // that shifted the buffer, engine-supplied as the four-probe map closure. AnchorScale = sqrt(|det J|), the local
    // areal-scale root; AnchorConvergence = atan2(dYdx, dXdx), the transformed source x-axis rotation. The step h
    // scales off the anchor magnitude (a degree-domain geographic source probes ~1e-4 deg, a six-figure easting
    // ~0.5 m — both well inside the slowly-varying distortion field); a refused or non-finite probe records
    // (NaN, NaN) through the Try — evidence absence, never a leg fault and never a fabricated unit scale.
    static (double Scale, double Convergence) Distortion(Func<double, double, (double X, double Y)> map, double ox, double oy) =>
        Try.lift(() => {
            double h = Math.Max(Math.Max(Math.Abs(ox), Math.Abs(oy)), 1.0) * 1e-6;
            var ((xe, ye), (xw, yw), (xn, yn), (xs, ys)) = (map(ox + h, oy), map(ox - h, oy), map(ox, oy + h), map(ox, oy - h));
            var (dXdx, dYdx, dXdy, dYdy) = ((xe - xw) / (2.0 * h), (ye - yw) / (2.0 * h), (xn - xs) / (2.0 * h), (yn - ys) / (2.0 * h));
            double det = dXdx * dYdy - dXdy * dYdx;
            return double.IsFinite(det) && det != 0.0
                ? (Scale: Math.Sqrt(Math.Abs(det)), Convergence: Math.Atan2(dYdx, dXdx))
                : (Scale: double.NaN, Convergence: double.NaN);
        }).Run().IfFail((double.NaN, double.NaN));

    // The engine-agnostic domain guard: an out-of-domain reprojection emits a non-finite ordinate (ProjNET NaN, PROJ
    // inf) rather than silent garbage, so every position column is finiteness-scanned before the shift is trusted —
    // the honest replacement for ProjNET's GetDomainFlags (a phantom: NotImplementedException on every 2.1.0 transform).
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

    static double Hypot(double dx, double dy, double dz) => Math.Sqrt(dx * dx + dy * dy + dz * dz);

    // A SpatialReference keyed to MATCH the frame's seam CrsResolution: ImportFromEPSG off the parsed Epsg, else
    // ImportFromWkt off the inline Wkt (OSR's ImportFromWkt takes the WKT by ref). OAMS_TRADITIONAL_GIS_ORDER pins
    // lon/lat order against the GDAL-3 axis swap; Osr.UseExceptions (set by the one GeoGdal.Bootstrap guard) makes a
    // failed import throw into the enclosing Try rather than return a code.
    static SpatialReference Crs(GeoReference frame) {
        var crs = new SpatialReference("");
        frame.Resolution.Switch(
            epsg: () => crs.ImportFromEPSG(frame.Epsg.IfNone(0)),
            wkt: () => { string wkt = frame.Crs.Map(static c => c.Wkt).IfNone(""); crs.ImportFromWkt(ref wkt); },
            unreferenced: static () => { });
        crs.SetAxisMappingStrategy(AxisMappingStrategy.OAMS_TRADITIONAL_GIS_ORDER);
        return crs;
    }
}
```

## [04]-[RESEARCH]

- [GEOREFERENCE_LEVELS]: the `GeoReferenceProjector.Project` two-arm fold grounds against the IFC level-of-georeferencing hierarchy decompile-verified via `uv run python -m tools.assay api` against the restored GeometryGym IFC assembly (`.api/api-geometrygym-ifc`) — the LoGeoRef-50 arm over `IfcMapConversion` (`Eastings`/`Northings`/`OrthogonalHeight`/`XAxisAbscissa`/`XAxisOrdinate`, the `Scale`/`ScaleY`/`ScaleZ` getters each coercing an unset `double.NaN` to `1.0`, `SourceCRS`/`TargetCRS`), `IfcMapConversionScaled` (`FactorX`/`FactorY`/`FactorZ` defaulting `1.0`, the IFC4.3 ADD2 per-axis scale), `IfcProjectedCRS` (`Name`/`GeodeticDatum`/`VerticalDatum`/`MapProjection`/`MapZone`/`MapUnit`), and the single `IfcGeometricRepresentationContext.HasCoordinateOperation`; the LoGeoRef-30 arm over `IfcSite.RefLatitude`/`RefLongitude` (`IfcCompoundPlaneAngleMeasure`, its `.Angle()` folding deg/min/sec/micro to decimal degrees) and `RefElevation` (`double`, NaN default) onto a WGS84 (`EPSG:4326`) reference. The `XAxisAbscissa`/`XAxisOrdinate` `double.NaN` default (a missing rotation) coerces to the identity direction `(1,0)`, the per-axis `Scale`/`ScaleY`/`ScaleZ` reading (not a single `Scale` folded thrice) preserves the per-axis map distortion, the `IfcProjectedCRS.MapUnit` (`IfcNamedUnit`) model-unit↔CRS-unit factor is composed onto the per-axis scale through the polymorphic `IfcNamedUnit.SIFactor()` (the abstract `public abstract double SIFactor()` on `IfcNamedUnit`, overridden by `IfcSIUnit.SIFactor()` and resolved through `IfcConversionBasedUnit.ConversionFactor.SIFactor()` for a US-survey-foot CRS — decompile-verified) so a non-metre projected CRS is reconciled to the metre model frame at ingest [M1], the three CRS carriers (the authority `Name`, the inline `IfcWellKnownText.WellKnownText` — decompile-verified `public string WellKnownText` on `IfcWellKnownText` reached via `IfcCoordinateReferenceSystem.WellKnownText` — and the `IfcProjectedCRS.MapProjection`/`MapZone` strings) all handed to `Admit` so a WKT-defined or projection-defined CRS resolves rather than false-faulting, and the GeometryGym `IfcCoordinateReferenceSystem.Name` empty→`"Unknown"` setter coercion is treated as the no-CRS sentinel — each correction grounded in the decompiled getter/setter bodies. The full 11-field record (the three-state `ProjectedCrs` `[ComplexValueObject]` carrying `Name`/`Epsg`/`Wkt`/`MapProjection`/`MapZone` and the derived `Epsg`) + fault-on-fully-unresolvable grounds against the seam `Geospatial/reference#GEO_REFERENCE` contract, the projector COMPOSING the seam `GeoReference.Admit` (which owns the three-state EPSG/WKT/projection resolution via `ProjectedCrs`, the fault, and the 11-field record construction) rather than re-deriving that admission in Bim.
- [SEAM_OWNERSHIP]: the `GeoReference`/`ProjectedCrs` ownership grounds against the seam `Rasm.Element/Geospatial/reference` contract — Bim owns the IFC projection and the `ProjNET`/OSR transform, the seam owns the value-object, its `GeoReference.Admit` admission, and the EPSG/WKT/projection resolution; the seam `GeoReference.Admit` 15-arg factory (`eastings, northings, orthogonalHeight, abscissa, ordinate, scaleX, scaleY, scaleZ, geodeticDatum, verticalDatum, projectedCrsName, wkt, mapProjection, mapZone, key`), its 11-field record, the derived `Epsg` (`Crs.Bind(c => c.Epsg)`), the `CrsResolution` `[SmartEnum<string>]` (`Epsg`/`Wkt`/`Unreferenced`) the datum leg `Switch`es on, and the `ProjectedCrs.Of`/`Epsg`/`Wkt`/`Resolution` member spellings `Admit` composes confirm against `Rasm.Element/Geospatial/reference`, and the `ProjNET` `CoordinateSystemServices.GetCoordinateSystem(int)`/`CreateTransformation(CoordinateSystem,CoordinateSystem)`/`CoordinateSystemFactory.CreateFromWkt`/`ICoordinateTransformation.MathTransform`/`MathTransform.Transform(Span<double>,Span<double>,Span<double>,int,int,int)` member spellings (decompile-verified against the restored `ProjNET` assembly) + the OSR `SpatialReference.ImportFromWkt(ref string)`/`ImportFromEPSG(int)` + the single-cache-owner + escalation-seam law confirm against `.api/api-projnet#CRS_TRANSFORM` before the datum leg is final.
- [DATUM_ESCALATION]: the `GeoTransform.Reproject` resolution-keyed ProjNET-first / OSR-escalation rail grounds against `.api/api-projnet#CRS_TRANSFORM` (EACH frame's `CrsResolution` `Switch` resolving its `CoordinateSystem` — the EPSG-keyed `CoordinateSystemServices.GetCoordinateSystem(int)` cache or the WKT-keyed `CoordinateSystemFactory.CreateFromWkt(string)` parse — joined by the ONE facade `CoordinateSystemServices.CreateTransformation(CoordinateSystem,CoordinateSystem)`→`ICoordinateTransformation` build, so a mixed EPSG↔WKT pair builds managed and reading only `Epsg` would silently no-op a WKT-only federation since both `Epsg` are `None`; the strided `double` batch in place on the `Span<double>` is the dense form, the per-vertex `ref` loop and narrowing the survey ordinates to `float` the rejected forms; the `MathTransform.Transform(Span<double>,Span<double>,Span<double>,int,int,int)` strided overload + the `TransformCore` `while (num < xs.Length)` count + `GetCoordinateSystem(int)`/`CreateTransformation(CoordinateSystem,CoordinateSystem)`/`CreateFromWkt(string)` are decompile-verified against the restored `ProjNET` assembly (`.api/api-projnet`); `ProjNET` escalates an exotic datum-grid/dynamic-datum transform to the reciprocal `MaxRev.Gdal.Core` OSR engine) and `.api/api-maxrev-gdal` (the OSR `SpatialReference.ImportFromEPSG(int)`/`ImportFromWkt(ref string)` keyed to match the frame's resolution, `SetAxisMappingStrategy(OAMS_TRADITIONAL_GIS_ORDER)`, `CoordinateTransformation.TransformPoints`, and the one `Semantics/geospatial#RASTER_INGEST` `GeoGdal.Bootstrap` idempotent `GdalBase.ConfigureAll`+`Osr.UseExceptions` guard); the `Fin<Reprojection>` additive-`Identity` + bare `BimFault.CapabilityMiss` (a CRS pair the transform algebra cannot reconcile) rail grounds against `Model/faults#FAULT_BAND`, the consumer `Semantics/geospatial#GEOSPATIAL_SEAM` `GeoFeature.Reproject` flattening its NTS `CoordinateSequence` into the double buffer and delegating the datum leg + its OSR escalation to this single owner. The receipt evidence grounds against the decompiled surfaces: `MathTransform.Derivative`/`GetDomainFlags`/`GetCodomainConvexHull` are PHANTOMS (base `NotImplementedException`, ZERO concrete overrides across the 2.1.0 assembly) and every `CoordinateTransformationFactory`-built `ICoordinateTransformation` passes `string.Empty` for `AreaOfUse` — the `.api/api-projnet#PHANTOM_SURFACE` record — so the distortion evidence is the `Distortion` central-difference anchor probe over whichever engine shifted the buffer and the domain guard the post-transform non-finite scan; `CoordinateTransformationOptions.SetBallparkAllowed(bool)`/`SetOnlyBest(bool)` and `SpatialReference.IsDynamic()`/`GetCoordinateEpoch()`/`SetCoordinateEpoch(double)` are decompile-verified against the restored `MaxRev.Gdal.Core` 3.13.1 bindings and catalogued on the `.api/api-maxrev-gdal` OSR rows; the seam `GeoReference` carries NO coordinate-epoch column, so a dynamic-datum frame records `EpochDefaulted` on the receipt until the seam epoch ripple lands.
- [HOST_NEUTRAL_OFFSET]: the rigid map-conversion offset is NOT materialized here — the kernel `Rasm` `Transform`/`Point3d`/`Vector3d` are RhinoCommon types (`Rasm.csproj` declares `<Using Include="Rhino.Geometry" />`), so a host-neutral Bim projector cannot bind them; the seam `GeoReference` carries the rigid-offset parameters (the translation, the per-axis scale, and the host-neutral `RotationRadians` direction-cosine the seam computes via `Math.Atan2(ordinate, abscissa)`, the IFC convention carrying the rotation as a direction not an angle), a DOWNSTREAM host-bound kernel `Transform` consumer in the Rhino runtime folding them into the rigid placement, the datum `Reproject` running over `ProjNET`/OSR between two seam `GeoReference` frames BEFORE that rigid offset so the kernel transform stays datum-free; this page composes the seam value-object, the managed `ProjNET`/`MaxRev.Gdal.Core` datum engine, and the host-neutral kernel `Rasm.Domain.Op` key only, never RhinoCommon.
