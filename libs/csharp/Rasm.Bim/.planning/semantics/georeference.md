# [BIM_COORDINATE_PROJECTION]

The IFC coordinate-reference PROJECTOR over the `Rasm.Element` seam `GeoReference`: `GeoReferenceProjector` FOLDS whatever georeferencing level a model carries onto the seam-owned `GeoReference` twelve-tuple carried on the `ElementGraph` `Header` (and on `Coverage` nodes), and `GeoTransform` is the datum-to-datum reprojection leg over `ProjNET` (escalating an exotic datum-grid or dynamic-datum transform `ProjNET` cannot express to the `MaxRev.Gdal.Core` OSR PROJ engine) operating on raw double-precision ordinate spans of that seam frame (survey eastings never narrow to `float`). `Project` prefers the `IfcMapConversion`/`IfcMapConversionScaled` over an `IfcProjectedCRS` (the EPSG-bearing LoGeoRef-50 level), falls back to an `IfcSite`'s `RefLatitude`/`RefLongitude`/`RefElevation` geographic position lowered onto a WGS84 (`EPSG:4326`) reference (the LoGeoRef-30 level), and returns `GeoReference.Identity` when a model carries neither so ingest never blocks. The `GeoReference`/`ProjectedCrs` value-objects are SEAM-owned (`Rasm.Element/Geospatial/reference`): the seam owns the full M1 field set — `Eastings`/`Northings`/`OrthogonalHeight`, `XAxisAbscissa`/`XAxisOrdinate`, per-axis `ScaleX`/`ScaleY`/`ScaleZ`, `GeodeticDatum`/`VerticalDatum`, the `ProjectedCrsName` (an `Option<ProjectedCrs>`), and the parsed `Epsg` (an `Option<int>`) — and Bim owns the IFC projection that fills it and the `ProjNET`/OSR transform that consumes it. The projector composes the seam `GeoReference.Admit` admission — which owns the URN/authority/`EPSG:` EPSG parse, the fault-on-unresolvable, and the twelve-tuple construction in one hop — and RE-BANDS its seam fault to `BimFault.CapabilityMiss` BARE (band 2600 IS the `Expected` `Code` — no `.ToError()` hop) when a CRS name is present but no EPSG resolves, never silently dropping a federation onto an unreferenced frame [M1] and never re-deriving the seam admission (an inline `new GeoReference(...)` construction or a Bim CRS parser beside the seam's) here. The seam `GeoReference` is carried on `Header`/`Coverage` ONLY — it is dropped from the `Object` node — so the model frame is one header fact, not a per-element axis. The seam `GeoReference` carries the rigid map-conversion offset's PARAMETERS (the translation, the `RotationRadians` direction-cosine the seam projects, the per-axis scale) that a DOWNSTREAM host-bound consumer folds into the kernel `Transform` algebra in the Rhino runtime; this projector NEVER materializes a transform, because the kernel `Transform`/`Point3d`/`Vector3d` are RhinoCommon types a host-neutral owner cannot bind. The geodetic datum shift is the `ProjNET`/OSR leg the kernel transform does not own, reprojecting raw ordinate spans between two seam `GeoReference` frames for a federation. The page is HOST-NEUTRAL: it binds GeometryGym (IFC), `ProjNET`/`MaxRev.Gdal.Core` (managed CRS), the seam, and the host-neutral kernel `Op` key — never RhinoCommon.

## [01]-[INDEX]

- [01]-[GEO_PROJECTION]: `GeoReferenceProjector.Project` folding the model georeferencing level onto the seam `GeoReference` twelve-tuple — `IfcMapConversion`/`IfcMapConversionScaled` over `IfcProjectedCRS` (LoGeoRef 50, per-axis scale + the seam `ProjectedCrs` EPSG parse + fault-on-unresolvable), the `IfcSite` `RefLatitude`/`RefLongitude`/`RefElevation` WGS84 fallback (LoGeoRef 30), else `Identity` [M1].
- [02]-[GEODETIC_TRANSFORM]: `GeoTransform.Reproject` the datum-to-datum leg over `ProjNET` `CoordinateSystemServices`/`MathTransform` (the `.api/api-projnet#CRS_TRANSFORM` strided `double` batch in place on the `Span<double>` ordinate buffer), escalating an exotic datum-grid/dynamic-datum transform `ProjNET` cannot express to the `MaxRev.Gdal.Core` OSR engine, additive (`Fin.Succ` no-op on absent/equal EPSG) and faulting `BimFault.CapabilityMiss` only when both engines fail a present, differing pair.

## [02]-[GEO_PROJECTION]

- Owner: `GeoReferenceProjector` the static IFC→seam projector folding the georeferencing level the model carries onto the seam `GeoReference` — the single `IfcGeometricRepresentationContext.HasCoordinateOperation` `IfcMapConversion` (or `IfcMapConversionScaled` for per-axis scale) over its referenced `IfcProjectedCRS` (LoGeoRef 50), else the first `IfcSite`'s `RefLatitude`/`RefLongitude`/`RefElevation` geographic position onto a WGS84 (`EPSG:4326`) reference (LoGeoRef 30), else `GeoReference.Identity`. The `GeoReference`/`ProjectedCrs` value-objects are seam-owned (`Rasm.Element/Geospatial/reference`); this page projects the IFC surface onto them, composing the seam `GeoReference.Admit` (which owns the `ProjectedCrs` EPSG parse), never re-declaring them, never re-deriving the admission, never re-minting a CRS parser, and never materializing a kernel transform (host-neutral).
- Entry: `GeoReferenceProjector.Project(IfcProject project, Op key)` projects the model's georeferencing into the seam `GeoReference` — a model with no map conversion AND no geographic site position returns `GeoReference.Identity` so ingest never blocks; an `IfcProjectedCRS` name present but resolving no EPSG FAULTS `Model/faults#FAULT_BAND` `BimFault.CapabilityMiss` BARE (band 2600 IS the `Expected` `Code` — NO `.ToError()` lowering hop) rather than landing the federation on an unreferenced frame [M1]; `Fin<GeoReference>` carries the result; the `Op key` threads from the `Projection/semantic#SEMANTIC_PROJECTOR` `ProjectionContext.Key`, and that projector composes the success onto the `ElementGraph` `Header.Reference`.
- Auto: `Project` is a two-arm fold — the LoGeoRef-50 arm reads the `IfcMapConversion` rigid offset (`Eastings`/`Northings`/`OrthogonalHeight`), the `XAxisAbscissa`/`XAxisOrdinate` rotation direction-cosine pair (each defaulting to `double.NaN` when the rotation is unset, coerced to the identity direction `(1,0)` so `RotationRadians` resolves to `0` rather than `Atan2(NaN,NaN)`), and the per-axis scale (an `IfcMapConversionScaled`'s `FactorX`/`FactorY`/`FactorZ`, else the plain conversion's `Scale`/`ScaleY`/`ScaleZ` — each getter already coercing an unset NaN to `1.0`, the degenerate explicit `0.0` coerced to `1.0` so a zero map-conversion scale never collapses geometry), plus the referenced `IfcProjectedCRS` `Name`/`GeodeticDatum`/`VerticalDatum`, then hands the rigid offset, the coerced rotation pair, the per-axis scale, and the datum/CRS names to the seam `GeoReference.Admit`; the LoGeoRef-30 arm folds an `IfcSite`'s `RefLatitude`/`RefLongitude` `IfcCompoundPlaneAngleMeasure` through `.Angle()` to decimal degrees and reads `RefElevation` (NaN→`0.0`), then hands the literal `EPSG:4326` to the same `Admit` for a WGS84 reference (longitude east, latitude north, identity rotation, unit scale). Inside `Admit` the seam `ProjectedCrs.Epsg` resolves the EPSG code from the CRS `Name` across the OGC URN (`urn:ogc:def:crs:EPSG::25832`), the authority form (`EPSG:25832`), and a bare numeric code; the projector first normalizes the GeometryGym empty-name sentinel `"Unknown"` (its `IfcCoordinateReferenceSystem.Name` setter coerces an empty value to `"Unknown"`) back to blank so `Admit` reads it as the no-CRS state, a present-but-unresolvable name faulting (the seam `ElementFault` re-banded to `BimFault.CapabilityMiss`) rather than degrading to a no-op transform.
- Receipt: the seam `GeoReference` is the coordinate-reference evidence the `Header` carries (and the `Semantics/geospatial#RASTER_INGEST` `Coverage` node carries for a georeferenced raster); its parameters (the translation, the seam-projected `RotationRadians` direction-cosine, the per-axis scale) feed a DOWNSTREAM host-bound kernel `Transform` consumer in the Rhino runtime, never a transform this host-neutral projector builds; the `Epsg` codes drive the `[03]-[GEODETIC_TRANSFORM]` `ProjNET`/OSR datum leg.
- Packages: GeometryGymIFC_Core, Rasm.Element, Rasm (the host-neutral `Rasm.Domain.Op` key only), LanguageExt.Core
- Growth: a new map-conversion parameter is one column on the seam `GeoReference` (the seam's, not this page's); a new georeferencing level is one arm on the `Project` fold (a future LoGeoRef-40 `WorldCoordinateSystem`/`TrueNorth` rotation folds onto the existing rotation field); a new CRS-name scheme is one arm on the seam `ProjectedCrs.Epsg`; the rigid offset is the downstream host-bound kernel transform's and the datum shift is the `[03]-[GEODETIC_TRANSFORM]` leg; never a new transform owner, never a Bim CRS parser, never a per-CRS class.
- Boundary: the `GeoReference`/`ProjectedCrs` value-objects are SEAM-owned [M1] and re-declaring them in Bim is the named drift defect — this page is the IFC projector that fills the seam value, never its owner; the seam `GeoReference` is the twelve-tuple (`Eastings`/`Northings`/`OrthogonalHeight`, `XAxisAbscissa`/`XAxisOrdinate`, per-axis `ScaleX`/`ScaleY`/`ScaleZ`, `GeodeticDatum`/`VerticalDatum`, `Option<ProjectedCrs> ProjectedCrsName`, `Option<int> Epsg`) and constructing a `GeoReference` with a `SourceCrs`/`Crs` field or any column the seam does not declare is the deleted form; the seam `GeoReference` is carried on `Header`/`Coverage` ONLY and a `GeoReference` on the `Object` node is the deleted form [M1]; the projector composes the seam `GeoReference.Admit` (the one admission owning the URN/authority/`EPSG:` EPSG parse via `ProjectedCrs.Epsg`, the fault-on-unresolvable, and the twelve-tuple construction) and re-deriving that admission inline (an `new GeoReference(...)` construction with an inline EPSG parse beside `Admit`) OR a hand-rolled Bim CRS parser is the deleted form; the per-axis scale reads `IfcMapConversion.Scale`/`ScaleY`/`ScaleZ` (or `IfcMapConversionScaled.FactorX`/`FactorY`/`FactorZ`) and folding a single `Scale` onto all three axes drops the per-axis map distortion; the unset `XAxisAbscissa`/`XAxisOrdinate` (`double.NaN`) coerces to the identity direction `(1,0)` and reading the raw NaN into the rotation is the named defect; the projection rides the GeometryGym `IfcMapConversion`/`IfcMapConversionScaled`/`IfcProjectedCRS`/`IfcSite` surface consumed as settled vocabulary (`.api/api-geometrygym-ifc` georeferencing entities), a hand-rolled IFC reader the deleted form; the rigid map-conversion offset is NOT built here — the kernel `Transform`/`Point3d`/`Vector3d` are RhinoCommon types and composing them on this host-neutral projector is the named host-neutrality defect, the seam `GeoReference` carrying the offset parameters a downstream host-bound consumer folds; a present-but-unresolvable CRS name FAULTS `BimFault.CapabilityMiss` BARE [M1] (a `.ToError()` lowering hop or a keyless `new BimFault.CapabilityMiss(detail)` construction the named defect this aligns to `Model/faults#FAULT_BAND`) and silently landing the model on an unreferenced frame is the named defect; a non-georeferenced model returns `GeoReference.Identity` so ingest never blocks on a missing CRS.

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
    // GeoReference.Admit (the seam owns the EPSG parse + fault-on-unresolvable + twelve-tuple construction); Bim only
    // reads the IFC surface, normalizes the GeometryGym "Unknown" empty-name sentinel, and re-bands Admit's seam fault.
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
        var (sx, sy, sz) = conversion is IfcMapConversionScaled scaled
            ? (NonZero(scaled.FactorX), NonZero(scaled.FactorY), NonZero(scaled.FactorZ))
            : (NonZero(conversion.Scale), NonZero(conversion.ScaleY), NonZero(conversion.ScaleZ));
        // XAxisAbscissa/XAxisOrdinate default to NaN when the rotation is unset — coerce the pair to the identity
        // direction (1,0) so the seam RotationRadians resolves to 0 rather than Atan2(NaN,NaN).
        double abscissa = double.IsNaN(conversion.XAxisAbscissa) ? 1.0 : conversion.XAxisAbscissa;
        double ordinate = double.IsNaN(conversion.XAxisOrdinate) ? 0.0 : conversion.XAxisOrdinate;
        // GeometryGym coerces an empty IfcCoordinateReferenceSystem.Name to the "Unknown" sentinel; normalize it back
        // to blank so the seam Admit reads the no-CRS state (valid ungeoreferenced offset) rather than faulting it.
        string name = crs?.Name is { } raw && !string.Equals(raw, "Unknown", StringComparison.OrdinalIgnoreCase) ? raw : "";
        // Compose the seam admission (one hop: it parses EPSG across EPSG:/URN/authority, faults the unresolvable name,
        // and constructs the twelve-tuple); re-band the seam ElementFault to Bim's CapabilityMiss at the boundary [M1].
        return GeoReference.Admit(
                conversion.Eastings, conversion.Northings, conversion.OrthogonalHeight,
                abscissa, ordinate, sx, sy, sz,
                crs?.GeodeticDatum ?? "", crs?.VerticalDatum ?? "", name, key)
            .MapFail(_ => new BimFault.CapabilityMiss(key, $"crs-name-unresolvable:{name}"));
    }

    // LoGeoRef 30: the IfcSite geographic position onto a WGS84 (EPSG:4326) reference — RefLatitude/RefLongitude are
    // IfcCompoundPlaneAngleMeasure (deg/min/sec/micro), folded to decimal degrees by .Angle(); a site missing either
    // angle is ungeoreferenced (Identity), longitude landing Eastings and latitude Northings in the geographic frame.
    // The seam Admit resolves the literal EPSG:4326 with no fault, so this arm carries no MapFail re-band.
    static Fin<GeoReference> FromSite(IfcSite site, Op key) =>
        site.RefLatitude is null || site.RefLongitude is null
            ? Fin.Succ(GeoReference.Identity)
            : GeoReference.Admit(
                site.RefLongitude.Angle(), site.RefLatitude.Angle(), double.IsNaN(site.RefElevation) ? 0.0 : site.RefElevation,
                1.0, 0.0, 1.0, 1.0, 1.0, "WGS84", "", "EPSG:4326", key);

    // The degenerate explicit 0.0 scale coerces to 1.0 so a zero map-conversion scale never collapses geometry.
    static double NonZero(double value) => value == 0.0 ? 1.0 : value;
}
```

## [03]-[GEODETIC_TRANSFORM]

- Owner: `GeoTransform` the datum-bridging leg reprojecting raw ordinate spans from a source seam `GeoReference.Epsg` to a target seam `GeoReference.Epsg` over the admitted `ProjNET` `CoordinateSystemServices`, escalating an exotic datum-grid or dynamic-datum transform `ProjNET` cannot express to the `MaxRev.Gdal.Core` OSR PROJ engine per the `.api/api-projnet` escalation-seam; `CoordinateServices` the one process-wide `CoordinateSystemServices` SRID-keyed cache the `.api/api-projnet` `CRS_TRANSFORM` law names as the single CS/transformation owner. The leg operates on the seam `GeoReference` frame and a `ProjNET`/OSR datum shift folded onto the kernel transform is the named seam violation.
- Entry: `GeoTransform.Reproject(GeoReference source, GeoReference target, Span<double> ordinates, int stride, Op key)` applies the EPSG-keyed datum-to-datum transform IN PLACE on the interleaved double ordinate buffer when both `source.Epsg` and `target.Epsg` resolve and differ, returning `Fin<Unit>`: the additive cases (no source CRS, a single CRS, matching EPSG, or fewer than one full vertex) return `Fin.Succ(unit)` so the datum leg never blocks a single-datum federation; a present, differing pair builds the `ProjNET` `MathTransform` once and runs the strided batch, escalating to OSR when `ProjNET` cannot express the transform, and faulting `BimFault.CapabilityMiss` BARE only when BOTH engines fail. The buffer is `double` end to end — a survey easting never narrows to `float` (a float32 round-trip drops sub-metre precision on a six-figure easting; the `Semantics/geospatial#GEOSPATIAL_SEAM` precision contract) — and the NTS `CoordinateSequence` flatten plus the `Geometry.Apply` write-back is the geospatial CONSUMER's marshalling, never this owner's, so the leg stays geometry-library-neutral over raw ordinates. Composed BEFORE the downstream host-bound rigid map-conversion offset so a federated model lands in the shared datum before its local-engineering placement applies.
- Auto: `Reproject` reads the two seam `GeoReference.Epsg` codes and short-circuits when either is absent or they are equal; the `ProjNET` build is lifted through `Try` so a SRID absent from the bundled `SRID.csv` or a datum `ProjNET` cannot express routes the OSR escalation rather than throwing across the boundary; the `ProjNET` apply is the `.api/api-projnet#CRS_TRANSFORM` strided `double` batch run DIRECTLY on the interleaved buffer IN PLACE — a single `MathTransform.Transform(ordinates, ordinates[1..], ordinates[2..], stride, stride, stride)` call over the three ordinate columns of that one `Span<double>`, no staging copy (the buffer is already `double`, so there is no widen/narrow and no `MemoryMarshal.Cast<float,double>` to misread the bytes) and the `TransformCore` `while (num < xs.Length)` walk drives the count off the full-length first column so the last vertex is covered, a `stride` above three leaving the non-position interleave columns untouched; the OSR escalation deinterleaves the position columns into pooled `double[]` x/y/z, runs the one `Semantics/geospatial#RASTER_INGEST` `GeoGdal.Bootstrap` idempotent guard (`GdalBase.ConfigureAll` + `Osr.UseExceptions`), builds two `SpatialReference` (`ImportFromEPSG`, `OAMS_TRADITIONAL_GIS_ORDER` pinning lon/lat against the GDAL-3 axis swap) and one `CoordinateTransformation`, runs one `TransformPoints(count, xs, ys, zs)`, and reinterleaves; the datum shift composes BEFORE the rigid offset so a model lands in the shared datum before its local-engineering-frame placement applies.
- Packages: ProjNET, MaxRev.Gdal.Core, Rasm.Element, Rasm, LanguageExt.Core
- Growth: a new CRS pair is one EPSG-keyed `CreateTransformation` the `CoordinateSystemServices` cache resolves from the bundled `SRID.csv`; an exotic datum-grid or dynamic datum is the OSR PROJ pipeline's, resolved from the EPSG code, never a hand-rolled Bursa-Wolf matrix; a float-buffered consumer widens to `double` at its OWN boundary and calls the one `Span<double>` leg, never a parallel `Span<float>` overload re-admitting the survey-precision-loss footgun; a denser batch is one `MathTransform`/`CoordinateTransformation` overload swap, never a second transform owner and never a per-vertex `ref` loop.
- Boundary: the datum reprojection is `ProjNET`'s by default — the `CoordinateSystemServices.CreateTransformation` SRID facade and the `MathTransform` Bursa-Wolf 7-parameter datum shift plus projection own the managed transform — escalating to the `MaxRev.Gdal.Core` OSR `SpatialReference`/`CoordinateTransformation` PROJ pipeline (the full datum-grid set + `IsDynamic`/`GetCoordinateEpoch` plate-motion) for what the managed algebra cannot express, and a hand-rolled datum shift, a per-call `CoordinateTransformationFactory`/`CoordinateTransformation` rebuild outside the single cache, or OSR for a transform `ProjNET` already covers is the deleted form per the `.api/api-projnet` single-cache-owner + escalation-seam law; the `ProjNET` apply is the strided `double` batch run in place on the `Span<double>` and a per-vertex `Transform(ref x, ref y, ref z)` loop OR narrowing the survey ordinates to `float` (the precision-loss defect the geospatial seam forbids) is the rejected form; the GDAL bootstrap is the one `Semantics/geospatial#RASTER_INGEST` `GeoGdal.Bootstrap` idempotent guard and a second `GdalBase.ConfigureAll` owner is the deleted form; the leg is additive — an absent or equal EPSG returns `Fin.Succ(unit)` so `Reproject` never blocks ingest — and faults `BimFault.CapabilityMiss` BARE only when a present, differing pair defeats both engines (the `Op key` carrying the operation context, never a `.ToError()` hop); the reprojection composes BEFORE the downstream host-bound rigid map-conversion offset so the kernel transform stays datum-free, and folding the rigid offset into this datum leg is the named defect; the page reprojects raw `Span<double>` ordinate buffers — the NTS `CoordinateSequence` flatten and the `Geometry.Apply` write-back are the geospatial CONSUMER's marshalling, so a `GeoTransform` overload binding an NTS `Geometry`/`CoordinateSequence` is the misplaced-concern form — and a RhinoCommon geometry type crossing this leg is the host-bound defect.

```csharp signature
// --- [OPERATIONS] -------------------------------------------------------------------------
public static class GeoTransform {
    static readonly CoordinateSystemServices CoordinateServices = new();

    // The datum leg over two seam GeoReferences: source.Epsg -> target.Epsg reprojection of an interleaved
    // DOUBLE-precision ordinate Span<double> IN PLACE — survey eastings/northings never narrow to float (a float32
    // round-trip loses sub-metre precision on a 500_000 m easting), the Semantics/geospatial#GEOSPATIAL_SEAM
    // GeoFeature.Reproject consumer flattening its NTS CoordinateSequence into the double buffer and writing the
    // shifted ordinates back through Geometry.Apply on its side. Additive — no source/target EPSG, matching EPSG, or
    // fewer than one full vertex returns Fin.Succ(unit). ProjNET is the default managed engine; an exotic datum-grid/
    // dynamic-datum transform ProjNET cannot express escalates to GDAL OSR; both engines failing a present, differing
    // pair faults BimFault.CapabilityMiss bare. Composed before the downstream host-bound rigid map-conversion offset
    // so a federated model lands in the shared datum first.
    public static Fin<Unit> Reproject(GeoReference source, GeoReference target, Span<double> ordinates, int stride, Op key) {
        Option<(int From, int To)> pair =
            from s in source.Epsg from t in target.Epsg where s != t select (s, t);
        if (pair.IsNone || ordinates.Length < stride) {
            return Fin.Succ(unit);
        }
        var (from, to) = pair.IfNone((0, 0));
        // ProjNET first; a SRID absent from SRID.csv or a datum ProjNET cannot express lifts onto the rail (no throw
        // crossing the boundary) so the OSR escalation runs.
        MathTransform? managed = Try.lift(() => CoordinateServices.CreateTransformation(from, to).MathTransform)
            .Run().Match(Succ: static t => t, Fail: static _ => (MathTransform?)null);
        if (managed is not null) {
            // The .api/api-projnet#CRS_TRANSFORM dense rail: ONE strided batch over the three ordinate columns of the
            // one interleaved buffer IN PLACE (xs=ordinates@0, ys=ordinates[1..]@1, zs=ordinates[2..]@2, stride each),
            // no staging copy and no per-vertex Transform(ref x,ref y,ref z) loop. TransformCore walks num<xs.Length so
            // the full-length first column drives the count and the last vertex is covered; stride>3 skips any normal/uv
            // interleave columns (left untouched). Survey ordinates stay double end to end.
            managed.Transform(ordinates, ordinates[1..], ordinates[2..], stride, stride, stride);
            return Fin.Succ(unit);
        }
        return Osr(from, to, ordinates, stride, key);
    }

    // The exotic datum escalation: GDAL OSR carries PROJ's full datum-grid + dynamic-datum pipeline ProjNET's managed
    // algebra cannot. OSR's TransformPoints takes struct-of-arrays double columns, so the interleaved buffer deinterleaves
    // into pooled double x/y/z, transforms, and reinterleaves (no float anywhere). The build (bootstrap, ImportFromEPSG,
    // CoordinateTransformation) lifts through Try so a missing RID runtime or an EPSG no PROJ grid covers surfaces as
    // BimFault.CapabilityMiss rather than an exception; both engines failing leaves the ordinates untouched.
    static Fin<Unit> Osr(int from, int to, Span<double> ordinates, int stride, Op key) {
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
                using SpatialReference src = Crs(from);
                using SpatialReference dst = Crs(to);
                using var pipeline = new CoordinateTransformation(src, dst);
                pipeline.TransformPoints(count, xs, ys, zs);
                return unit;
            }).Run();
            if (shifted.IsFail) {
                return Fin.Fail<Unit>(new BimFault.CapabilityMiss(key, $"crs-pair-unreconcilable:{from}->{to}"));
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

    // OAMS_TRADITIONAL_GIS_ORDER pins lon/lat order against the GDAL-3 axis swap; Osr.UseExceptions (set by the one
    // GeoGdal.Bootstrap guard) makes a failed ImportFromEPSG throw into the enclosing Try rather than return a code.
    static SpatialReference Crs(int epsg) {
        var crs = new SpatialReference("");
        crs.ImportFromEPSG(epsg);
        crs.SetAxisMappingStrategy(AxisMappingStrategy.OAMS_TRADITIONAL_GIS_ORDER);
        return crs;
    }
}
```

## [04]-[RESEARCH]

- [GEOREFERENCE_LEVELS]: the `GeoReferenceProjector.Project` two-arm fold grounds against the IFC level-of-georeferencing hierarchy decompile-verified via `uv run python -m tools.assay api` against GeometryGymIFC_Core 25.7.30 — the LoGeoRef-50 arm over `IfcMapConversion` (`Eastings`/`Northings`/`OrthogonalHeight`/`XAxisAbscissa`/`XAxisOrdinate`, the `Scale`/`ScaleY`/`ScaleZ` getters each coercing an unset `double.NaN` to `1.0`, `SourceCRS`/`TargetCRS`), `IfcMapConversionScaled` (`FactorX`/`FactorY`/`FactorZ` defaulting `1.0`, the IFC4.3 ADD2 per-axis scale), `IfcProjectedCRS` (`Name`/`GeodeticDatum`/`VerticalDatum`/`MapProjection`/`MapZone`/`MapUnit`), and the single `IfcGeometricRepresentationContext.HasCoordinateOperation`; the LoGeoRef-30 arm over `IfcSite.RefLatitude`/`RefLongitude` (`IfcCompoundPlaneAngleMeasure`, its `.Angle()` folding deg/min/sec/micro to decimal degrees) and `RefElevation` (`double`, NaN default) onto a WGS84 (`EPSG:4326`) reference. The `XAxisAbscissa`/`XAxisOrdinate` `double.NaN` default (a missing rotation) coerces to the identity direction `(1,0)`, the per-axis `Scale`/`ScaleY`/`ScaleZ` reading (not a single `Scale` folded thrice) preserves the per-axis map distortion, and the GeometryGym `IfcCoordinateReferenceSystem.Name` empty→`"Unknown"` setter coercion is treated as the no-CRS sentinel — each correction grounded in the decompiled getter/setter bodies. The full twelve-tuple + fault-on-unresolvable grounds against `ELEMENT-REBUILD-PLAN.md` §4-RT M1, the projector COMPOSING the seam `Geospatial/reference#GEO_REFERENCE` `GeoReference.Admit` (which owns the EPSG parse via `ProjectedCrs.Epsg`, the fault, and the twelve-tuple construction) rather than re-deriving that admission in Bim.
- [SEAM_OWNERSHIP]: the `GeoReference`/`ProjectedCrs` ownership grounds against `ELEMENT-REBUILD-PLAN.md` §4B and §6 — Bim owns the IFC projection and the `ProjNET`/OSR transform, the seam owns the value-object, its `GeoReference.Admit` admission, and the EPSG parse; the seam `GeoReference.Admit` factory, its twelve-tuple, and the `ProjectedCrs.TryCreate`/`Epsg` member spellings `Admit` composes confirm against `Rasm.Element/Geospatial/reference`, and the `ProjNET` `CoordinateSystemServices`/`ICoordinateTransformation.MathTransform`/`MathTransform.Transform(Span<double>,Span<double>,Span<double>,…)` member spellings + the single-cache-owner + escalation-seam law confirm against `.api/api-projnet#CRS_TRANSFORM` before the datum leg is final.
- [DATUM_ESCALATION]: the `GeoTransform.Reproject` ProjNET-first / OSR-escalation rail grounds against `.api/api-projnet#CRS_TRANSFORM` (the strided `double` batch in place on the `Span<double>` is the dense form, the per-vertex `ref` loop and narrowing the survey ordinates to `float` the rejected forms; the `MathTransform.Transform(Span<double>,Span<double>,Span<double>,int,int,int)` strided overload + the `TransformCore` `while (num < xs.Length)` count are decompile-verified against ProjNET 2.1.0; `ProjNET` escalates an exotic datum-grid/dynamic-datum transform to the reciprocal `MaxRev.Gdal.Core` OSR engine) and `.api/api-maxrev-gdal` (the OSR `SpatialReference.ImportFromEPSG`/`SetAxisMappingStrategy(OAMS_TRADITIONAL_GIS_ORDER)`, `CoordinateTransformation.TransformPoints`, and the one `Semantics/geospatial#RASTER_INGEST` `GeoGdal.Bootstrap` idempotent `GdalBase.ConfigureAll`+`Osr.UseExceptions` guard); the `Fin<Unit>` additive-no-op + bare `BimFault.CapabilityMiss` (a CRS the transform algebra cannot reconcile) rail grounds against `Model/faults#FAULT_BAND`, the consumer `Semantics/geospatial#GEOSPATIAL_SEAM` `GeoFeature.Reproject` flattening its NTS `CoordinateSequence` into the double buffer and delegating the datum leg + its OSR escalation to this single owner.
- [HOST_NEUTRAL_OFFSET]: the rigid map-conversion offset is NOT materialized here — the kernel `Rasm` `Transform`/`Point3d`/`Vector3d` are RhinoCommon types (`Rasm.csproj` declares `<Using Include="Rhino.Geometry" />`), so a host-neutral Bim projector cannot bind them; the seam `GeoReference` carries the rigid-offset parameters (the translation, the per-axis scale, and the host-neutral `RotationRadians` direction-cosine the seam computes via `Math.Atan2(ordinate, abscissa)`, the IFC convention carrying the rotation as a direction not an angle), a DOWNSTREAM host-bound kernel `Transform` consumer in the Rhino runtime folding them into the rigid placement, the datum `Reproject` running over `ProjNET`/OSR between two seam `GeoReference` frames BEFORE that rigid offset so the kernel transform stays datum-free; this page composes the seam value-object, the managed `ProjNET`/`MaxRev.Gdal.Core` datum engine, and the host-neutral kernel `Rasm.Domain.Op` key only, never RhinoCommon.
