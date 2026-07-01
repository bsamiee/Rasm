# [BIM_COORDINATE_PROJECTION]

The IFC coordinate-reference PROJECTOR over the `Rasm.Element` seam `GeoReference`: `GeoReferenceProjector` FOLDS whatever georeferencing level a model carries onto the seam-owned `GeoReference` record carried on the `ElementGraph` `Header` (and on `Coverage` nodes), and `GeoTransform` is the datum-to-datum reprojection leg over `ProjNET` (escalating an exotic datum-grid or dynamic-datum transform `ProjNET` cannot express to the `MaxRev.Gdal.Core` OSR PROJ engine) operating on raw double-precision ordinate spans of that seam frame (survey eastings never narrow to `float`). `Project` prefers the `IfcMapConversion`/`IfcMapConversionScaled` over an `IfcProjectedCRS` (the EPSG-bearing LoGeoRef-50 level), falls back to an `IfcSite`'s `RefLatitude`/`RefLongitude`/`RefElevation` geographic position lowered onto a WGS84 (`EPSG:4326`) reference (the LoGeoRef-30 level), and returns `GeoReference.Identity` when a model carries neither so ingest never blocks. The `GeoReference`/`ProjectedCrs` value-objects are SEAM-owned (`Rasm.Element/Geospatial/reference`): the seam owns the full M1 field set — the 11-field record carrying `Eastings`/`Northings`/`OrthogonalHeight`, `XAxisAbscissa`/`XAxisOrdinate`, per-axis `ScaleX`/`ScaleY`/`ScaleZ`, `GeodeticDatum`/`VerticalDatum`, and one `Option<ProjectedCrs> Crs` (the three-state `[ComplexValueObject]` CRS identity carrying the authority `Name`, the inline `Wkt`, and the `MapProjection`/`MapZone` projection identity), with the `Epsg` a DERIVED `Crs.Bind(c => c.Epsg)` projection and the `CrsResolution` mode a derived column — and Bim owns the IFC projection that fills it and the `ProjNET`/OSR transform that consumes it. The seam frame is METRE-NORMALIZED: this projector composes the `IfcProjectedCRS.MapUnit` `IfcNamedUnit.SIFactor()` model-unit↔CRS-unit factor onto the per-axis scale at ingest (the MapUnit fold below IS the seam's stated contract, not a divergence), so the seam doubles arrive in metres and the seam carries NO `MapUnit` field — a federation reconciles every model onto one metre frame from one record. The projector composes the seam `GeoReference.Admit` admission — which owns the URN/authority/`EPSG:` EPSG parse, the WKT/projection resolution, the fault-on-fully-unresolvable, and the 11-field record construction in one hop — and RE-BANDS its seam fault to `BimFault.CapabilityMiss` BARE (band 2600 IS the `Expected` `Code` — no `.ToError()` hop) when a CRS name is present but resolves to neither an EPSG code NOR a WKT NOR a projection+zone, never silently dropping a federation onto an unreferenced frame [M1] and never re-deriving the seam admission (an inline `new GeoReference(...)` construction or a Bim CRS parser beside the seam's) here. The seam `GeoReference` is carried on `Header`/`Coverage` ONLY — it is dropped from the `Object` node — so the model frame is one header fact, not a per-element axis. The seam `GeoReference` carries the rigid map-conversion offset's PARAMETERS (the translation, the `RotationRadians` direction-cosine the seam projects, the per-axis scale) that a DOWNSTREAM host-bound consumer folds into the kernel `Transform` algebra in the Rhino runtime; this projector NEVER materializes a transform, because the kernel `Transform`/`Point3d`/`Vector3d` are RhinoCommon types a host-neutral owner cannot bind. The geodetic datum shift is the `ProjNET`/OSR leg the kernel transform does not own, reprojecting raw ordinate spans between two seam `GeoReference` frames for a federation, BRANCHING on the source CRS `CrsResolution` to pick the EPSG-keyed vs WKT-keyed transform-build path. The page is HOST-NEUTRAL: it binds GeometryGym (IFC), `ProjNET`/`MaxRev.Gdal.Core` (managed CRS), the seam, and the host-neutral kernel `Op` key — never RhinoCommon.

## [01]-[INDEX]

- [01]-[GEO_PROJECTION]: `GeoReferenceProjector.Project` folding the model georeferencing level onto the seam `GeoReference` 11-field record — `IfcMapConversion`/`IfcMapConversionScaled` over `IfcProjectedCRS` (LoGeoRef 50, per-axis scale composed with the `IfcProjectedCRS.MapUnit` `SIFactor()` model-unit↔CRS-unit factor + the seam three-state `ProjectedCrs` EPSG/WKT/projection resolution + fault-on-fully-unresolvable), the `IfcSite` `RefLatitude`/`RefLongitude`/`RefElevation` WGS84 fallback (LoGeoRef 30), else `Identity` [M1].
- [03]-[GEODETIC_TRANSFORM]: `GeoTransform.Reproject` the datum-to-datum leg over `ProjNET`, BRANCHING on the source `CrsResolution` `Switch` to the EPSG-keyed `CoordinateSystemServices` build or the WKT-keyed `CoordinateSystemFactory.CreateFromWkt`+`CoordinateTransformationFactory` build, then the `.api/api-projnet#CRS_TRANSFORM` strided `double` batch in place on the `Span<double>` ordinate buffer, escalating an exotic datum-grid/dynamic-datum transform `ProjNET` cannot express to the resolution-keyed `MaxRev.Gdal.Core` OSR engine (`ImportFromEPSG`/`ImportFromWkt`), additive (`Fin.Succ` no-op on an `Unreferenced` endpoint or an identical CRS) and faulting `BimFault.CapabilityMiss` only when both engines fail a differing, resolvable pair.

## [02]-[GEO_PROJECTION]

- Owner: `GeoReferenceProjector` the static IFC→seam projector folding the georeferencing level the model carries onto the seam `GeoReference` — the single `IfcGeometricRepresentationContext.HasCoordinateOperation` `IfcMapConversion` (or `IfcMapConversionScaled` for per-axis scale) over its referenced `IfcProjectedCRS` (LoGeoRef 50), else the first `IfcSite`'s `RefLatitude`/`RefLongitude`/`RefElevation` geographic position onto a WGS84 (`EPSG:4326`) reference (LoGeoRef 30), else `GeoReference.Identity`. The `GeoReference`/`ProjectedCrs` value-objects are seam-owned (`Rasm.Element/Geospatial/reference`); this page projects the IFC surface onto them — reading the authority `Name`, the inline `IfcWellKnownText.WellKnownText`, and the `MapProjection`/`MapZone` into the seam's three-state `ProjectedCrs`, and composing the metre-normalized scale and the seam `GeoReference.Admit` (which owns the three-state EPSG/WKT/projection resolution and the 11-field record construction) — never re-declaring them, never re-deriving the admission, never re-minting a CRS parser, and never materializing a kernel transform (host-neutral).
- Entry: `GeoReferenceProjector.Project(IfcProject project, Op key)` projects the model's georeferencing into the seam `GeoReference` — a model with no map conversion AND no geographic site position returns `GeoReference.Identity` so ingest never blocks; an `IfcProjectedCRS` name present but resolving no EPSG FAULTS `Model/faults#FAULT_BAND` `BimFault.CapabilityMiss` BARE (band 2600 IS the `Expected` `Code` — NO `.ToError()` lowering hop) rather than landing the federation on an unreferenced frame [M1]; `Fin<GeoReference>` carries the result; the `Op key` threads from the `Projection/semantic#SEMANTIC_PROJECTOR` `ProjectionContext.Key`, and that projector composes the success onto the `ElementGraph` `Header.Reference`.
- Auto: `Project` is a two-arm fold — the LoGeoRef-50 arm reads the `IfcMapConversion` rigid offset (`Eastings`/`Northings`/`OrthogonalHeight`), the `XAxisAbscissa`/`XAxisOrdinate` rotation direction-cosine pair (each defaulting to `double.NaN` when the rotation is unset, coerced to the identity direction `(1,0)` so `RotationRadians` resolves to `0` rather than `Atan2(NaN,NaN)`), and the per-axis scale (an `IfcMapConversionScaled`'s `FactorX`/`FactorY`/`FactorZ`, else the plain conversion's `Scale`/`ScaleY`/`ScaleZ` — each getter already coercing an unset NaN to `1.0`, the degenerate explicit `0.0` coerced to `1.0` so a zero map-conversion scale never collapses geometry) MULTIPLIED by the model-unit↔CRS-unit factor (the referenced `IfcProjectedCRS.MapUnit` `IfcNamedUnit.SIFactor()` metre-per-map-unit — `1.0` for a metre CRS, `~0.3048006096` for a US-survey-foot State Plane zone — so a non-metre projected CRS is not silently mislocated [M1]), plus the referenced `IfcProjectedCRS`'s three CRS carriers — the authority `Name`, the inline `WellKnownText.WellKnownText` OGC WKT, and the `MapProjection`/`MapZone` projection identity — alongside `GeodeticDatum`/`VerticalDatum`, then hands the rigid offset, the coerced rotation pair, the unit-composed per-axis scale, the datum names, and the three CRS strings to the seam `GeoReference.Admit`; the LoGeoRef-30 arm folds an `IfcSite`'s `RefLatitude`/`RefLongitude` `IfcCompoundPlaneAngleMeasure` through `.Angle()` to decimal degrees and reads `RefElevation` (NaN→`0.0`), then hands the literal `EPSG:4326` authority name with blank `wkt`/`mapProjection`/`mapZone` to the same `Admit` for a WGS84 reference (longitude east, latitude north, identity rotation, unit scale). Inside `Admit` the seam builds the three-state `ProjectedCrs` and `ProjectedCrs.Epsg` resolves the EPSG code from the authority `Name` across the OGC URN (`urn:ogc:def:crs:EPSG::25832`), the authority form (`EPSG:25832`), and a bare numeric code, while a WKT-defined CRS (blank/unresolvable `Name`, non-blank `Wkt`) or a projection+zone CRS resolves WITHOUT an EPSG; the projector first normalizes the GeometryGym empty-name sentinel `"Unknown"` (its `IfcCoordinateReferenceSystem.Name` setter coerces an empty value to `"Unknown"`) back to blank so `Admit` reads it as the no-CRS state, a name present yet resolving to NO identity at all (no EPSG, no WKT, no projection+zone) faulting (the seam `ElementFault` re-banded to `BimFault.CapabilityMiss`) rather than degrading to a no-op transform, a WKT-resolvable CRS never faulted.
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
    // The model georeferencing fold onto the seam GeoReference: prefer the IfcMapConversion(Scaled)+IfcProjectedCRS
    // (LoGeoRef 50, EPSG-bearing), else the IfcSite RefLatitude/RefLongitude/RefElevation geographic position onto a
    // WGS84 (EPSG:4326) reference (LoGeoRef 30), else Identity so ingest never blocks. Both arms COMPOSE the seam
    // GeoReference.Admit (the seam owns the three-state EPSG/WKT/projection resolution + fault-on-fully-unresolvable +
    // 11-field record construction); Bim only reads the IFC surface (the authority Name, the inline WKT, the projection
    // strings), normalizes the GeometryGym "Unknown" empty-name sentinel, and re-bands Admit's seam fault.
    public static Fin<GeoReference> Project(IfcProject project, Op key) =>
        Optional(project.RepresentationContexts
                .OfType<IfcGeometricRepresentationContext>()
                .Select(static ctx => ctx.HasCoordinateOperation as IfcMapConversion)
                .FirstOrDefault(static conversion => conversion is not null))
            .Match(
                Some: conversion => FromMapConversion(conversion, conversion.TargetCRS as IfcProjectedCRS, key),
                None: () => Optional(project.Extract<IfcSite>().FirstOrDefault())
                    .Match(Some: site => FromSite(site, key), None: static () => Fin.Succ(GeoReference.Identity)));

    static Fin<GeoReference> FromMapConversion(IfcMapConversion conversion, IfcProjectedCRS? crs, Op key) {
        // IfcMapConversionScaled carries FactorX/Y/Z; a plain IfcMapConversion carries per-axis Scale/ScaleY/ScaleZ
        // (each getter already coercing an unset NaN to 1.0). NonZero coerces the degenerate explicit 0.0 to 1.0.
        // The IfcMapConversion scale converts MODEL length units to MAP (CRS-native) units, and IfcProjectedCRS.MapUnit
        // (an IfcNamedUnit) declares that native unit — metre for the overwhelming majority of EPSG projected CRS,
        // US-survey-foot for some State Plane zones [§4-RT M1]. The seam GeoReference scale/translation doubles are
        // expressed in the CRS-native unit, so the MODEL-UNIT<->CRS-UNIT factor (the IfcNamedUnit.SIFactor() polymorphic
        // metre-per-mapunit multiplier — 1.0 metre, ~0.3048006096 US-survey-foot, computed once here, not in the seam)
        // folds onto the per-axis scale so a non-metre projected CRS is NOT silently mislocated; an absent MapUnit
        // defaults the factor to 1.0 (a metre map unit).
        double mapUnitToMetre = crs?.MapUnit is { } mapUnit ? NonZero(mapUnit.SIFactor()) : 1.0;
        var (sx, sy, sz) = conversion is IfcMapConversionScaled scaled
            ? (NonZero(scaled.FactorX) * mapUnitToMetre, NonZero(scaled.FactorY) * mapUnitToMetre, NonZero(scaled.FactorZ) * mapUnitToMetre)
            : (NonZero(conversion.Scale) * mapUnitToMetre, NonZero(conversion.ScaleY) * mapUnitToMetre, NonZero(conversion.ScaleZ) * mapUnitToMetre);
        // XAxisAbscissa/XAxisOrdinate default to NaN when the rotation is unset — coerce the pair to the identity
        // direction (1,0) so the seam RotationRadians resolves to 0 rather than Atan2(NaN,NaN).
        double abscissa = double.IsNaN(conversion.XAxisAbscissa) ? 1.0 : conversion.XAxisAbscissa;
        double ordinate = double.IsNaN(conversion.XAxisOrdinate) ? 0.0 : conversion.XAxisOrdinate;
        // GeometryGym coerces an empty IfcCoordinateReferenceSystem.Name to the "Unknown" sentinel; normalize it back
        // to blank so the seam Admit reads the no-CRS state (valid ungeoreferenced offset) rather than faulting it.
        string name = crs?.Name is { } raw && !string.Equals(raw, "Unknown", StringComparison.OrdinalIgnoreCase) ? raw : "";
        // The three-state CRS carriers [§4-RT M1]: a GIS-origin IfcProjectedCRS may define its CRS by an inline OGC WKT
        // (IfcCoordinateReferenceSystem.WellKnownText.WellKnownText) with NO authority Name, or by the MapProjection/MapZone
        // projection identity. Read all three off the GeometryGym surface so the seam Admit reaches the WKT/projection
        // resolution state and a WKT-defined CRS no longer false-faults as "unresolvable" (the deleted two-state slice).
        string wkt = crs?.WellKnownText?.WellKnownText ?? "";
        string mapProjection = crs?.MapProjection ?? "";
        string mapZone = crs?.MapZone ?? "";
        // Compose the seam admission (one hop: it builds the three-state ProjectedCrs, parses EPSG across EPSG:/URN/authority,
        // resolves a WKT/projection CRS, faults ONLY a name with no EPSG/WKT/projection, and constructs the 11-field record);
        // re-band the seam ElementFault to Bim's CapabilityMiss at the boundary [M1].
        return GeoReference.Admit(
                conversion.Eastings, conversion.Northings, conversion.OrthogonalHeight,
                abscissa, ordinate, sx, sy, sz,
                crs?.GeodeticDatum ?? "", crs?.VerticalDatum ?? "",
                name, wkt, mapProjection, mapZone, key)
            .MapFail(_ => new BimFault.CapabilityMiss(key, $"crs-name-unresolvable:{name}"));
    }

    // LoGeoRef 30: the IfcSite geographic position onto a WGS84 (EPSG:4326) reference — RefLatitude/RefLongitude are
    // IfcCompoundPlaneAngleMeasure (deg/min/sec/micro), folded to decimal degrees by .Angle(); a site missing either
    // angle is ungeoreferenced (Identity), longitude landing Eastings and latitude Northings in the geographic frame.
    // The seam Admit resolves the literal EPSG:4326 by authority code with no fault, so this arm carries no MapFail
    // re-band and passes the three CRS strings blank — the WGS84 reference is EPSG-resolved, not WKT/projection-defined.
    static Fin<GeoReference> FromSite(IfcSite site, Op key) =>
        site.RefLatitude is null || site.RefLongitude is null
            ? Fin.Succ(GeoReference.Identity)
            : GeoReference.Admit(
                site.RefLongitude.Angle(), site.RefLatitude.Angle(), double.IsNaN(site.RefElevation) ? 0.0 : site.RefElevation,
                1.0, 0.0, 1.0, 1.0, 1.0, "WGS84", "", "EPSG:4326", "", "", "", key);

    // The degenerate explicit 0.0 scale coerces to 1.0 so a zero map-conversion scale never collapses geometry.
    static double NonZero(double value) => value == 0.0 ? 1.0 : value;
}
```

## [03]-[GEODETIC_TRANSFORM]

- Owner: `GeoTransform` the datum-bridging leg reprojecting raw ordinate spans between two seam `GeoReference` frames, BRANCHING on the source `CrsResolution` to pick the build path — the EPSG-keyed `ProjNET` `CoordinateSystemServices.CreateTransformation(srid, srid)` SRID facade when both frames resolve by EPSG, the WKT-keyed `CoordinateSystemFactory.CreateFromWkt` + `CoordinateTransformationFactory.CreateFromCoordinateSystems` build when either frame defines its CRS by inline WKT (a GIS-origin CRS with no authority code, where both `Epsg` are `None` so the SRID facade alone silently no-ops the federation) — escalating an exotic datum-grid or dynamic-datum transform `ProjNET` cannot express to the `MaxRev.Gdal.Core` OSR PROJ engine (keyed by `ImportFromEPSG` or `ImportFromWkt` to match the frame's resolution) per the `.api/api-projnet` escalation-seam; `CoordinateServices` the one process-wide `CoordinateSystemServices` SRID-keyed cache and `TransformFactory` the one `CoordinateTransformationFactory` the `.api/api-projnet` `CRS_TRANSFORM` law names as the single CS/transformation owners. The leg operates on the seam `GeoReference` frame and a `ProjNET`/OSR datum shift folded onto the kernel transform is the named seam violation.
- Entry: `GeoTransform.Reproject(GeoReference source, GeoReference target, Span<double> ordinates, int stride, Op key)` applies the datum-to-datum transform IN PLACE on the interleaved double ordinate buffer when both frames carry a resolvable CRS (EPSG or WKT) that differs, returning `Fin<Unit>`: the additive cases (a source or target `CrsResolution.Unreferenced`, an identical CRS, or fewer than one full vertex) return `Fin.Succ(unit)` so the datum leg never blocks a single-datum federation; a differing, resolvable pair dispatches the source `Resolution` generated total `Switch` to the EPSG-keyed-vs-WKT-keyed `ProjNET` `MathTransform` build, runs the strided batch once, escalates to the matching OSR build (`ImportFromEPSG`/`ImportFromWkt`) when `ProjNET` cannot express the transform, and faults `BimFault.CapabilityMiss` BARE only when BOTH engines fail. The buffer is `double` end to end — a survey easting never narrows to `float` (a float32 round-trip drops sub-metre precision on a six-figure easting; the `Semantics/geospatial#GEOSPATIAL_SEAM` precision contract) — and the NTS `CoordinateSequence` flatten plus the `Geometry.Apply` write-back is the geospatial CONSUMER's marshalling, never this owner's, so the leg stays geometry-library-neutral over raw ordinates. Composed BEFORE the downstream host-bound rigid map-conversion offset so a federated model lands in the shared datum before its local-engineering placement applies.
- Auto: `Reproject` short-circuits when either frame is `CrsResolution.Unreferenced`, when the two CRS identities are equal (same EPSG, or same `Crs` value), or when the buffer holds fewer than one full vertex; otherwise it dispatches the source `Resolution` generated total `Switch` — the `Epsg` arm builds the managed transform off the SRID facade (`CoordinateServices.CreateTransformation(from, to).MathTransform`), the `Wkt` arm builds it off the two WKT definitions (`CoordinateSystemFactory.CreateFromWkt(sourceWkt)`/`CreateFromWkt(targetWkt)` → `TransformFactory.CreateFromCoordinateSystems(srcCS, tgtCS).MathTransform`), the `Unreferenced` arm unreachable here (the short-circuit already returned). The `ProjNET` build is lifted through `Try` so a SRID absent from the bundled `SRID.csv`, a WKT `ProjNET` cannot parse, or a datum `ProjNET` cannot express routes the OSR escalation rather than throwing across the boundary; the `ProjNET` apply is the `.api/api-projnet#CRS_TRANSFORM` strided `double` batch run DIRECTLY on the interleaved buffer IN PLACE — a single `MathTransform.Transform(ordinates, ordinates[1..], ordinates[2..], stride, stride, stride)` call over the three ordinate columns of that one `Span<double>`, no staging copy (the buffer is already `double`, so there is no widen/narrow and no `MemoryMarshal.Cast<float,double>` to misread the bytes) and the `TransformCore` `while (num < xs.Length)` walk drives the count off the full-length first column so the last vertex is covered, a `stride` above three leaving the non-position interleave columns untouched; the OSR escalation deinterleaves the position columns into pooled `double[]` x/y/z, runs the one `Semantics/geospatial#RASTER_INGEST` `GeoGdal.Bootstrap` idempotent guard (`GdalBase.ConfigureAll` + `Osr.UseExceptions`), builds two `SpatialReference` keyed to match each frame's resolution (`ImportFromEPSG` for an EPSG frame, `ImportFromWkt` for a WKT frame, `OAMS_TRADITIONAL_GIS_ORDER` pinning lon/lat against the GDAL-3 axis swap) and one `CoordinateTransformation`, runs one `TransformPoints(count, xs, ys, zs)`, and reinterleaves; the datum shift composes BEFORE the rigid offset so a model lands in the shared datum before its local-engineering-frame placement applies.
- Packages: ProjNET, MaxRev.Gdal.Core, Rasm.Element, Rasm, LanguageExt.Core
- Growth: a new EPSG CRS pair is one EPSG-keyed `CreateTransformation` the `CoordinateSystemServices` cache resolves from the bundled `SRID.csv`; a new WKT CRS pair is the one shared `TransformFactory.CreateFromCoordinateSystems` over two `CoordinateSystemFactory.CreateFromWkt` builds, never a per-call factory; a new CRS-resolution mode is one arm on the seam `CrsResolution` that breaks this `Switch` at compile time (the seam owns the discriminant, this leg owns the per-mode build); an exotic datum-grid or dynamic datum is the OSR PROJ pipeline's, resolved from the EPSG code or the WKT, never a hand-rolled Bursa-Wolf matrix; a float-buffered consumer widens to `double` at its OWN boundary and calls the one `Span<double>` leg, never a parallel `Span<float>` overload re-admitting the survey-precision-loss footgun; a denser batch is one `MathTransform`/`CoordinateTransformation` overload swap, never a second transform owner and never a per-vertex `ref` loop.
- Boundary: the datum reprojection is `ProjNET`'s by default — the `CoordinateSystemServices.CreateTransformation` SRID facade (the EPSG path) OR the `TransformFactory.CreateFromCoordinateSystems` over two `CoordinateSystemFactory.CreateFromWkt` builds (the WKT path, selected off the source `CrsResolution`), and the `MathTransform` Bursa-Wolf 7-parameter datum shift plus projection own the managed transform — escalating to the `MaxRev.Gdal.Core` OSR `SpatialReference`/`CoordinateTransformation` PROJ pipeline (`ImportFromEPSG` or `ImportFromWkt` matching the frame's resolution, the full datum-grid set + `IsDynamic`/`GetCoordinateEpoch` plate-motion) for what the managed algebra cannot express, and a hand-rolled datum shift, a per-CALL `new CoordinateTransformationFactory()`/`CoordinateTransformation` rebuild outside the two shared owners, or OSR for a transform `ProjNET` already covers is the deleted form per the `.api/api-projnet` single-cache-owner + escalation-seam law; branching the build off a re-spelled `Epsg.IsSome` check (the COLLAPSE_SCAN re-branch the seam forbids) rather than the seam `CrsResolution` `Switch` is the deleted form, and reading only `source.Epsg`/`target.Epsg` so a WKT-only federation silently no-ops (both `Epsg` `None`) is the named defect this leg closes; the `ProjNET` apply is the strided `double` batch run in place on the `Span<double>` and a per-vertex `Transform(ref x, ref y, ref z)` loop OR narrowing the survey ordinates to `float` (the precision-loss defect the geospatial seam forbids) is the rejected form; the GDAL bootstrap is the one `Semantics/geospatial#RASTER_INGEST` `GeoGdal.Bootstrap` idempotent guard and a second `GdalBase.ConfigureAll` owner is the deleted form; the leg is additive — a frame's `CrsResolution.Unreferenced` or an identical CRS returns `Fin.Succ(unit)` so `Reproject` never blocks ingest — and faults `BimFault.CapabilityMiss` BARE only when a differing, resolvable pair defeats both engines (the `Op key` carrying the operation context, never a `.ToError()` hop); the reprojection composes BEFORE the downstream host-bound rigid map-conversion offset so the kernel transform stays datum-free, and folding the rigid offset into this datum leg is the named defect; the page reprojects raw `Span<double>` ordinate buffers — the NTS `CoordinateSequence` flatten and the `Geometry.Apply` write-back are the geospatial CONSUMER's marshalling, so a `GeoTransform` overload binding an NTS `Geometry`/`CoordinateSequence` is the misplaced-concern form — and a RhinoCommon geometry type crossing this leg is the host-bound defect.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using ProjNet.CoordinateSystems;                    // CoordinateSystemFactory/CoordinateSystem (the WKT-keyed build)

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class GeoTransform {
    static readonly CoordinateSystemServices CoordinateServices = new();         // the EPSG/SRID-keyed CS+transformation cache
    static readonly CoordinateSystemFactory CsFactory = new();                   // the one shared WKT->CoordinateSystem parser
    static readonly CoordinateTransformationFactory TransformFactory = new();    // the one shared CS-pair->transformation owner

    // The datum leg over two seam GeoReferences: a source-to-target reprojection of an interleaved DOUBLE-precision
    // ordinate Span<double> IN PLACE — survey eastings/northings never narrow to float (a float32 round-trip loses
    // sub-metre precision on a 500_000 m easting), the Semantics/geospatial#GEOSPATIAL_SEAM GeoFeature.Reproject
    // consumer flattening its NTS CoordinateSequence into the double buffer and writing the shifted ordinates back
    // through Geometry.Apply on its side. The build path is the source CrsResolution case — the EPSG-keyed SRID facade
    // OR the WKT-keyed CreateFromWkt build (a GIS-origin WKT CRS carries no EPSG, so reading only Epsg silently no-ops
    // the federation; branching the seam CrsResolution Switch closes that). Additive — a frame's Unreferenced, an
    // identical CRS, or fewer than one full vertex returns Fin.Succ(unit). ProjNET is the default managed engine; an
    // exotic datum-grid/dynamic-datum transform ProjNET cannot express escalates to the resolution-keyed GDAL OSR
    // build; both engines failing a differing, resolvable pair faults BimFault.CapabilityMiss bare. Composed before the
    // downstream host-bound rigid map-conversion offset so a federated model lands in the shared datum first.
    public static Fin<Unit> Reproject(GeoReference source, GeoReference target, Span<double> ordinates, int stride, Op key) {
        // Additive short-circuit: an unreferenced endpoint, an identical CRS, or fewer than one full vertex is a no-op.
        // Sameness folds the EPSG (two names resolving the same code are one frame, so `EPSG:25832` and the URN form do
        // not build a redundant identity transform) THEN the Crs value-object (two WKT frames structurally equal under
        // the seam ProjectedCrs comparer policy).
        bool sameFrame =
            (from s in source.Epsg from t in target.Epsg select s == t).IfNone(false) || source.Crs == target.Crs;
        if (source.Resolution == CrsResolution.Unreferenced || target.Resolution == CrsResolution.Unreferenced
            || sameFrame || ordinates.Length < stride) {
            return Fin.Succ(unit);
        }
        // The build path is the source resolution case (the seam owns the discriminant, this leg owns the per-mode
        // build) — never a re-spelled Epsg.IsSome re-branch. Lifted through Try so a SRID absent from SRID.csv, a WKT
        // ProjNET cannot parse, or a datum it cannot express lifts onto the rail (no throw crossing the boundary) and
        // the matching OSR escalation runs. The Unreferenced arm is unreachable (the short-circuit already returned).
        MathTransform? managed = source.Resolution.Switch(
            epsg: () => Try.lift(() =>
                from s in source.Epsg from t in target.Epsg
                select CoordinateServices.CreateTransformation(s, t).MathTransform),
            wkt: () => Try.lift(() =>
                from sc in source.Crs from tc in target.Crs
                select TransformFactory.CreateFromCoordinateSystems(CsFactory.CreateFromWkt(sc.Wkt), CsFactory.CreateFromWkt(tc.Wkt)).MathTransform),
            unreferenced: static () => Try.lift(static () => Option<MathTransform>.None))
            .Run().Match(Succ: static t => t.IfNoneUnsafe(() => null!), Fail: static _ => (MathTransform?)null);
        if (managed is not null) {
            // The .api/api-projnet#CRS_TRANSFORM dense rail: ONE strided batch over the three ordinate columns of the
            // one interleaved buffer IN PLACE (xs=ordinates@0, ys=ordinates[1..]@1, zs=ordinates[2..]@2, stride each),
            // no staging copy and no per-vertex Transform(ref x,ref y,ref z) loop. TransformCore walks num<xs.Length so
            // the full-length first column drives the count and the last vertex is covered; stride>3 skips any normal/uv
            // interleave columns (left untouched). Survey ordinates stay double end to end.
            managed.Transform(ordinates, ordinates[1..], ordinates[2..], stride, stride, stride);
            return Fin.Succ(unit);
        }
        return Osr(source, target, ordinates, stride, key);
    }

    // The exotic datum escalation: GDAL OSR carries PROJ's full datum-grid + dynamic-datum pipeline ProjNET's managed
    // algebra cannot. OSR's TransformPoints takes struct-of-arrays double columns, so the interleaved buffer deinterleaves
    // into pooled double x/y/z, transforms, and reinterleaves (no float anywhere). Each SpatialReference is keyed to MATCH
    // its frame's resolution — ImportFromEPSG for an EPSG frame, ImportFromWkt for a WKT frame — so a WKT-only federation
    // escalates correctly. The build (bootstrap, import, CoordinateTransformation) lifts through Try so a missing RID
    // runtime, an EPSG no PROJ grid covers, or an unparseable WKT surfaces as BimFault.CapabilityMiss rather than an
    // exception; both engines failing leaves the ordinates untouched.
    static Fin<Unit> Osr(GeoReference source, GeoReference target, Span<double> ordinates, int stride, Op key) {
        int count = ordinates.Length / stride;
        double[] xs = ArrayPool<double>.Shared.Rent(count);
        double[] ys = ArrayPool<double>.Shared.Rent(count);
        double[] zs = ArrayPool<double>.Shared.Rent(count);
        try {
            for (int i = 0, o = 0; i < count; i++, o += stride) {
                (xs[i], ys[i], zs[i]) = (ordinates[o], ordinates[o + 1], ordinates[o + 2]);
            }
            Fin<Unit> shifted = Try.lift(() => {
                GeoGdal.Bootstrap();
                using SpatialReference src = Crs(source);
                using SpatialReference dst = Crs(target);
                using var pipeline = new CoordinateTransformation(src, dst);
                pipeline.TransformPoints(count, xs, ys, zs);
                return unit;
            }).Run();
            if (shifted.IsFail) {
                return Fin.Fail<Unit>(new BimFault.CapabilityMiss(key, $"crs-pair-unreconcilable:{source.Resolution.Key}->{target.Resolution.Key}"));
            }
            for (int i = 0, o = 0; i < count; i++, o += stride) {
                (ordinates[o], ordinates[o + 1], ordinates[o + 2]) = (xs[i], ys[i], zs[i]);
            }
            return Fin.Succ(unit);
        } finally {
            ArrayPool<double>.Shared.Return(xs);
            ArrayPool<double>.Shared.Return(ys);
            ArrayPool<double>.Shared.Return(zs);
        }
    }

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
- [SEAM_OWNERSHIP]: the `GeoReference`/`ProjectedCrs` ownership grounds against the seam `Rasm.Element/Geospatial/reference` contract — Bim owns the IFC projection and the `ProjNET`/OSR transform, the seam owns the value-object, its `GeoReference.Admit` admission, and the EPSG/WKT/projection resolution; the seam `GeoReference.Admit` 15-arg factory (`eastings, northings, orthogonalHeight, abscissa, ordinate, scaleX, scaleY, scaleZ, geodeticDatum, verticalDatum, projectedCrsName, wkt, mapProjection, mapZone, key`), its 11-field record, the derived `Epsg` (`Crs.Bind(c => c.Epsg)`), the `CrsResolution` `[SmartEnum<string>]` (`Epsg`/`Wkt`/`Unreferenced`) the datum leg `Switch`es on, and the `ProjectedCrs.Of`/`Epsg`/`Wkt`/`Resolution` member spellings `Admit` composes confirm against `Rasm.Element/Geospatial/reference`, and the `ProjNET` `CoordinateSystemServices.CreateTransformation(int,int)`/`CoordinateSystemFactory.CreateFromWkt`/`CoordinateTransformationFactory.CreateFromCoordinateSystems`/`ICoordinateTransformation.MathTransform`/`MathTransform.Transform(Span<double>,Span<double>,Span<double>,int,int,int)` member spellings (decompile-verified against the restored `ProjNET` assembly) + the OSR `SpatialReference.ImportFromWkt(ref string)`/`ImportFromEPSG(int)` + the single-cache-owner + escalation-seam law confirm against `.api/api-projnet#CRS_TRANSFORM` before the datum leg is final.
- [DATUM_ESCALATION]: the `GeoTransform.Reproject` resolution-branched ProjNET-first / OSR-escalation rail grounds against `.api/api-projnet#CRS_TRANSFORM` (the source `CrsResolution` `Switch` selecting the EPSG-keyed `CoordinateSystemServices.CreateTransformation(int,int)` build or the WKT-keyed `CoordinateSystemFactory.CreateFromWkt`→`CoordinateTransformationFactory.CreateFromCoordinateSystems` build — reading only `Epsg` would silently no-op a WKT-only federation since both `Epsg` are `None`; the strided `double` batch in place on the `Span<double>` is the dense form, the per-vertex `ref` loop and narrowing the survey ordinates to `float` the rejected forms; the `MathTransform.Transform(Span<double>,Span<double>,Span<double>,int,int,int)` strided overload + the `TransformCore` `while (num < xs.Length)` count + `CoordinateSystemFactory.CreateFromWkt(string)`→`CoordinateSystem` + `CoordinateTransformationFactory.CreateFromCoordinateSystems(CoordinateSystem,CoordinateSystem)`→`ICoordinateTransformation` are decompile-verified against the restored `ProjNET` assembly (`.api/api-projnet`); `ProjNET` escalates an exotic datum-grid/dynamic-datum transform to the reciprocal `MaxRev.Gdal.Core` OSR engine) and `.api/api-maxrev-gdal` (the OSR `SpatialReference.ImportFromEPSG(int)`/`ImportFromWkt(ref string)` keyed to match the frame's resolution, `SetAxisMappingStrategy(OAMS_TRADITIONAL_GIS_ORDER)`, `CoordinateTransformation.TransformPoints`, and the one `Semantics/geospatial#RASTER_INGEST` `GeoGdal.Bootstrap` idempotent `GdalBase.ConfigureAll`+`Osr.UseExceptions` guard); the `Fin<Unit>` additive-no-op + bare `BimFault.CapabilityMiss` (a CRS pair the transform algebra cannot reconcile) rail grounds against `Model/faults#FAULT_BAND`, the consumer `Semantics/geospatial#GEOSPATIAL_SEAM` `GeoFeature.Reproject` flattening its NTS `CoordinateSequence` into the double buffer and delegating the datum leg + its OSR escalation to this single owner.
- [HOST_NEUTRAL_OFFSET]: the rigid map-conversion offset is NOT materialized here — the kernel `Rasm` `Transform`/`Point3d`/`Vector3d` are RhinoCommon types (`Rasm.csproj` declares `<Using Include="Rhino.Geometry" />`), so a host-neutral Bim projector cannot bind them; the seam `GeoReference` carries the rigid-offset parameters (the translation, the per-axis scale, and the host-neutral `RotationRadians` direction-cosine the seam computes via `Math.Atan2(ordinate, abscissa)`, the IFC convention carrying the rotation as a direction not an angle), a DOWNSTREAM host-bound kernel `Transform` consumer in the Rhino runtime folding them into the rigid placement, the datum `Reproject` running over `ProjNET`/OSR between two seam `GeoReference` frames BEFORE that rigid offset so the kernel transform stays datum-free; this page composes the seam value-object, the managed `ProjNET`/`MaxRev.Gdal.Core` datum engine, and the host-neutral kernel `Rasm.Domain.Op` key only, never RhinoCommon.
