# [BIM_COORDINATE_PROJECTION]

The IFC coordinate-reference PROJECTOR over the `Rasm.Element` seam `GeoReference`: `GeoReferenceProjector` lowers an IFC model's `IfcMapConversion`/`IfcMapConversionScaled`/`IfcProjectedCRS` chain into the seam-owned `GeoReference` twelve-tuple carried on the `ElementGraph` `Header` (and on `Coverage` nodes), and `GeoTransform` is the datum-to-datum reprojection leg over the admitted `ProjNET` engine operating on that seam value. The `GeoReference`/`ProjectedCrs` value-objects are SEAM-owned (`Rasm.Element/Geospatial/reference`): the seam owns the full M1 field set — `Eastings`/`Northings`/`OrthogonalHeight`, `XAxisAbscissa`/`XAxisOrdinate`, per-axis `ScaleX`/`ScaleY`/`ScaleZ`, `GeodeticDatum`/`VerticalDatum`, the `ProjectedCrsName` (an `Option<ProjectedCrs>`), and the parsed `Epsg` (an `Option<int>`) — and Bim owns the IFC projection that fills it and the `ProjNET` transform that consumes it. The projector composes the seam `ProjectedCrs` EPSG parse (the URN/authority/`EPSG:` schemes the seam value-object already resolves) and FAULTS (`BimFault.CapabilityMiss`) when a CRS name is present but no EPSG resolves, never silently dropping a federation onto an unreferenced frame [M1] and never re-minting a Bim CRS parser beside the seam's. The seam `GeoReference` is carried on `Header`/`Coverage` ONLY — it is dropped from the `Object` node — so the model frame is one header fact, not a per-element axis. The rigid map-conversion offset composes the kernel `Rasm` transform algebra, never a hand-rolled matrix; the geodetic datum shift is the `ProjNET` leg the kernel transform does not own, reprojecting between two seam `GeoReference` frames for a federation. The page is HOST-NEUTRAL.

## [01]-[INDEX]

- [01]-[GEO_PROJECTION]: `GeoReferenceProjector.Project` lowering `IfcMapConversion`/`IfcMapConversionScaled`/`IfcProjectedCRS` into the seam `GeoReference` twelve-tuple (per-axis scale, the seam `ProjectedCrs` EPSG parse, fault-on-unresolvable) [M1].
- [02]-[GEODETIC_TRANSFORM]: `GeoTransform.Reproject` the datum-to-datum leg over `ProjNET` `CoordinateSystemServices`/`MathTransform` reprojecting between a source and target seam `GeoReference`, composed before the rigid map-conversion offset.

## [02]-[GEO_PROJECTION]

- Owner: `GeoReferenceProjector` the static IFC→seam projector reading the model's single `IfcGeometricRepresentationContext.HasCoordinateOperation` `IfcMapConversion` (or `IfcMapConversionScaled` for per-axis scale) and its referenced `IfcProjectedCRS` into the seam `GeoReference` value-object. The `GeoReference`/`ProjectedCrs` value-objects are seam-owned (`Rasm.Element/Geospatial/reference`); this page projects the IFC surface onto them, composing the seam `ProjectedCrs` EPSG parse, and never re-declares them or re-mints a CRS parser.
- Entry: `GeoReferenceProjector.Project(IfcProject project)` projects the model's map-conversion chain into the seam `GeoReference` — a model with no map conversion returns `GeoReference.Identity` so ingest never blocks; a model with an `IfcProjectedCRS` name that resolves no EPSG FAULTS `Model/faults#FAULT_BAND` `BimFault.CapabilityMiss` rather than landing the federation on an unreferenced frame [M1], lowered with `.ToError()`; `Fin<GeoReference>` carries the result. The `Projection/semantic#SEMANTIC_PROJECTOR` projector composes the success onto the `ElementGraph` `Header.Reference`.
- Auto: `Project` reads the `IfcMapConversion` rigid offset (`Eastings`/`Northings`/`OrthogonalHeight`), the `XAxisAbscissa`/`XAxisOrdinate` rotation direction-cosine pair, and the scale — promoting an `IfcMapConversionScaled`'s `FactorX`/`FactorY`/`FactorZ` onto the per-axis `ScaleX`/`ScaleY`/`ScaleZ` (a plain `IfcMapConversion` folding its single `Scale` onto all three axes, the degenerate `0.0` scale coerced to `1.0`) — and the referenced `IfcProjectedCRS` `Name`/`GeodeticDatum`/`VerticalDatum` into the seam `GeoReference` columns (the `Name` into the `Option<ProjectedCrs> ProjectedCrsName`, the parsed code into the `Option<int> Epsg`); the seam `ProjectedCrs.Epsg` resolves the EPSG code from the CRS `Name` across the OGC URN (`urn:ogc:def:crs:EPSG::25832`), the authority form (`EPSG:25832`), and a bare numeric code, so a federated model's CRS lands its EPSG for the `[3]-[GEODETIC_TRANSFORM]` datum leg, and a present-but-unresolvable name faults rather than degrading to a no-op transform.
- Receipt: the seam `GeoReference` is the coordinate-reference evidence the `Header` carries (and the `Semantics/geospatial#RASTER_INGEST` `Coverage` node carries for a georeferenced raster); the rigid map-conversion offset it builds composes the kernel `Rasm` transform via the seam `GeoReference.RotationRadians` direction-cosine projection; the `Epsg` codes drive the `ProjNET` datum leg.
- Packages: GeometryGymIFC_Core, Rasm.Element, ProjNET, Rasm, LanguageExt.Core
- Growth: a new map-conversion parameter is one column on the seam `GeoReference` (the seam's, not this page's); a new CRS-name scheme is one arm on the seam `ProjectedCrs.Epsg`; the rigid offset is the kernel transform's and the datum shift is the `[3]-[GEODETIC_TRANSFORM]` leg; never a new transform owner, never a Bim CRS parser, and never a per-CRS class.
- Boundary: the `GeoReference`/`ProjectedCrs` value-objects are SEAM-owned [M1] and re-declaring them in Bim is the named drift defect — this page is the IFC projector that fills the seam value, never its owner; the seam `GeoReference` is the twelve-tuple (`Eastings`/`Northings`/`OrthogonalHeight`, `XAxisAbscissa`/`XAxisOrdinate`, per-axis `ScaleX`/`ScaleY`/`ScaleZ`, `GeodeticDatum`/`VerticalDatum`, `Option<ProjectedCrs> ProjectedCrsName`, `Option<int> Epsg`) and constructing a `GeoReference` with a `SourceCrs`/`Crs` field or any column the seam does not declare is the deleted form; the seam `GeoReference` is carried on `Header`/`Coverage` ONLY and a `GeoReference` on the `Object` node is the deleted form [M1]; the EPSG parse composes the seam `ProjectedCrs.Epsg` (the URN/authority/`EPSG:` schemes) and a hand-rolled Bim CRS parser beside the seam's is the deleted form; the projection rides the GeometryGym `IfcMapConversion`/`IfcMapConversionScaled`/`IfcProjectedCRS`/`IfcCoordinateOperation` surface consumed as settled vocabulary (`.api/api-geometrygym-ifc` georeferencing entities), a hand-rolled IFC reader the deleted form; the map-conversion transform composes the kernel `Rasm` transform algebra and a hand-rolled rotation/translation/scale matrix is the named defect; a present-but-unresolvable CRS name FAULTS `BimFault.CapabilityMiss` [M1] and silently landing the model on an unreferenced frame is the named defect; a non-georeferenced model returns `GeoReference.Identity` so ingest never blocks on a missing CRS.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using GeometryGym.Ifc;
using LanguageExt;
using Rasm.Element;
using static LanguageExt.Prelude;

namespace Rasm.Bim;

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class GeoReferenceProjector {
    // IfcMapConversion(Scaled) + IfcProjectedCRS -> seam GeoReference (the full 12-tuple). Per-axis scale rides
    // IfcMapConversionScaled.FactorX/Y/Z (a plain conversion folds Scale onto all three); the EPSG parse COMPOSES
    // the seam ProjectedCrs.Epsg (no Bim parser); a present-but-unresolvable CRS name FAULTS BimFault.CapabilityMiss
    // rather than landing on an unreferenced frame [M1]; no conversion returns the seam GeoReference.Identity.
    public static Fin<GeoReference> Project(IfcProject project) =>
        Optional(project.RepresentationContexts
                .OfType<IfcGeometricRepresentationContext>()
                .Select(static ctx => ctx.HasCoordinateOperation as IfcMapConversion)
                .FirstOrDefault(static conversion => conversion is not null))
            .Match(
                None: () => Fin.Succ(GeoReference.Identity),
                Some: conversion => Build(conversion, conversion.TargetCRS as IfcProjectedCRS));

    static Fin<GeoReference> Build(IfcMapConversion conversion, IfcProjectedCRS? crs) {
        var (sx, sy, sz) = conversion is IfcMapConversionScaled scaled
            ? (Scale(scaled.FactorX), Scale(scaled.FactorY), Scale(scaled.FactorZ))
            : (Scale(conversion.Scale), Scale(conversion.Scale), Scale(conversion.Scale));
        string name = crs?.Name ?? "";
        if (name.Length == 0) {
            return Fin.Succ(new GeoReference(
                conversion.Eastings, conversion.Northings, conversion.OrthogonalHeight,
                conversion.XAxisAbscissa, conversion.XAxisOrdinate, sx, sy, sz,
                crs?.GeodeticDatum ?? "", crs?.VerticalDatum ?? "", None, None));
        }
        // Compose the seam ProjectedCrs EPSG parse (URN / EPSG: / authority); a present name with no EPSG faults.
        Option<ProjectedCrs> projected = ProjectedCrs.TryCreate(name).ToOption();
        Option<int> epsg = projected.Bind(static crs => crs.Epsg);
        return epsg.IsNone
            ? Fin.Fail<GeoReference>(new BimFault.CapabilityMiss($"crs-name-unresolvable:{name}").ToError())
            : Fin.Succ(new GeoReference(
                conversion.Eastings, conversion.Northings, conversion.OrthogonalHeight,
                conversion.XAxisAbscissa, conversion.XAxisOrdinate, sx, sy, sz,
                crs!.GeodeticDatum ?? "", crs.VerticalDatum ?? "", projected, epsg));
    }

    // The degenerate 0.0 scale coerces to 1.0 so a missing/zero map-conversion scale never collapses geometry.
    static double Scale(double value) => value == 0.0 ? 1.0 : value;
}
```

## [03]-[GEODETIC_TRANSFORM]

- Owner: `GeoTransform` the datum-bridging leg reprojecting model ordinates from a source seam `GeoReference.Epsg` to a target seam `GeoReference.Epsg` over the admitted `ProjNET` `CoordinateSystemServices` BEFORE the rigid map-conversion offset applies; `CoordinateServices` the one process-wide `CoordinateSystemServices` SRID-keyed cache the `.api/api-projnet` `CRS_TRANSFORM` law names as the single CS/transformation owner. The leg operates on the seam `GeoReference` value-object and a `ProjNET`-side datum shift on the kernel transform is the named seam violation.
- Entry: `GeoTransform.Reproject(GeoReference source, GeoReference target, Span<float> vertices, int stride)` applies the EPSG-keyed datum-to-datum transform in place when both `source.Epsg` and `target.Epsg` resolve and differ — they drive `CoordinateServices.CreateTransformation(source, target)`, the resulting `MathTransform` reprojecting each ordinate triple; a federation with no source CRS, a single CRS, or matching EPSG returns unchanged so the datum leg is additive and never blocks a single-datum federation.
- Auto: `Reproject` reads the two seam `GeoReference.Epsg` codes, short-circuits when either is absent or they are equal, builds the `MathTransform` once from the SRID-keyed cache, and reprojects the vertex span — the readable form widening each `float` ordinate to a `double` inline; the dense form the `.api/api-projnet#CRS_TRANSFORM` batch rail names is one `MathTransform.Transform(Span<double> xs, Span<double> ys, Span<double> zs)` call over a pooled `double` staging buffer widened from the `Span<float>` columns (every `ProjNET` batch overload is double-precision, so a raw `MemoryMarshal.Cast<float,double>` is the rejected form), narrowed back after the batch — a vertex-count-gated densification, not a new owner; the datum shift composes BEFORE the rigid map-conversion offset so a model lands in the shared datum before its local-engineering-frame placement applies.
- Packages: ProjNET, Rasm.Element, Rasm, LanguageExt.Core
- Growth: a new CRS pair is one EPSG-keyed `CreateTransformation` the `CoordinateSystemServices` cache resolves from the bundled `SRID.csv`; a new projection or datum is the `ProjNET` library's, resolved from the EPSG code, never a hand-rolled Bursa-Wolf matrix; the datum leg is one `Reproject` op composed once before the rigid offset, never a second transform owner.
- Boundary: the datum reprojection is `ProjNET`'s — the `CoordinateSystemServices.CreateTransformation` SRID facade and the `MathTransform` Bursa-Wolf 7-parameter datum shift plus projection own the full geodetic transform (ellipsoid, datum shift, projection), and a hand-rolled datum shift or a per-call `CoordinateTransformationFactory` rebuild is the deleted form per the `.api/api-projnet` single-cache-owner law; the reprojection composes BEFORE the rigid map-conversion offset so the kernel transform stays datum-free; the leg is additive — a non-georeferenced or single-datum federation returns unchanged so `Reproject` never blocks ingest; each seam `GeoReference.Epsg` drives the SRID lookup and an unresolvable EPSG returns unchanged (the present-but-unresolvable CRS already faulted at `[2]-[GEO_PROJECTION]` `Project`).

```csharp signature
// --- [SERVICES] ---------------------------------------------------------------------------
public static class GeoTransform {
    static readonly CoordinateSystemServices CoordinateServices = new();

    // The datum leg over two seam GeoReferences: source.Epsg -> target.Epsg reprojection in place, additive
    // (no source/target EPSG, or matching EPSG, returns unchanged). Composed before the rigid map-conversion
    // offset so a federated model lands in the shared datum before its local-engineering placement applies.
    public static void Reproject(GeoReference source, GeoReference target, Span<float> vertices, int stride) {
        var pair = from s in source.Epsg
                   from t in target.Epsg
                   where s != t
                   select (s, t);
        if (pair.IsNone) {
            return;
        }
        var (fromEpsg, toEpsg) = pair.IfNone((0, 0));
        var transform = CoordinateServices.CreateTransformation(fromEpsg, toEpsg).MathTransform;
        for (int offset = 0; offset + 2 < vertices.Length; offset += stride) {
            double x = vertices[offset], y = vertices[offset + 1], z = vertices[offset + 2];
            transform.Transform(ref x, ref y, ref z);
            (vertices[offset], vertices[offset + 1], vertices[offset + 2]) = ((float)x, (float)y, (float)z);
        }
    }
}
```

## [04]-[RESEARCH]

- [MAP_CONVERSION_PROJECTION]: the `GeoReferenceProjector.Project` body grounds against the GeometryGym `IfcMapConversion` (`Eastings`/`Northings`/`OrthogonalHeight`/`XAxisAbscissa`/`XAxisOrdinate`/`Scale`/`SourceCRS`/`TargetCRS`), `IfcMapConversionScaled` (`FactorX`/`FactorY`/`FactorZ`, IFC4.3 ADD2 per-axis scale), `IfcProjectedCRS` (`Name`/`GeodeticDatum`/`VerticalDatum`/`MapProjection`/`MapZone`), and the single `IfcGeometricRepresentationContext.HasCoordinateOperation` (the `IfcMapConversion` narrowed by `as`) catalogued at `.api/api-geometrygym-ifc` (georeferencing entity scope + traversal entrypoint); the full twelve-tuple field set + per-axis scale + fault-on-unresolvable grounds against `ELEMENT-REBUILD-PLAN.md` §4-RT M1 (extend GeoReference to the full eastings/northings/height/rotation/per-axis-scale/datum/EPSG set; parse EPSG from URN/authority/`EPSG:`; FAULT when a CRS name is present but unresolvable; keep GeoReference on Header/Coverage ONLY, drop it from the Object node), the EPSG parse COMPOSING the seam `Geospatial/reference#GEO_REFERENCE` `ProjectedCrs.Epsg` rather than a Bim parser.
- [SEAM_OWNERSHIP]: the `GeoReference`/`ProjectedCrs` ownership grounds against `ELEMENT-REBUILD-PLAN.md` §4B (generic `GeoReference`/`ProjectedCrs` value-object on Header/Coverage, owned at `Rasm.Element/Geospatial/reference`) and §6 (the geospatial projector → Object/Coverage nodes; `georeference.md` maps `IfcMapConversion`→seam `GeoReference`) — Bim owns the IFC projection and the `ProjNET` transform, the seam owns the value-object and its EPSG parse; the seam `GeoReference` twelve-tuple (`Eastings`/`Northings`/`OrthogonalHeight`, `XAxisAbscissa`/`XAxisOrdinate`, `ScaleX`/`ScaleY`/`ScaleZ`, `GeodeticDatum`/`VerticalDatum`, `Option<ProjectedCrs> ProjectedCrsName`, `Option<int> Epsg`) and the `ProjectedCrs.TryCreate`/`Epsg` member spellings confirm against `Rasm.Element/Geospatial/reference`, and the `ProjNET` `CoordinateSystemServices`/`ICoordinateTransformation.MathTransform`/`MathTransform.Transform` member spellings and the single-cache-owner law confirm against `.api/api-projnet#CRS_TRANSFORM` before the datum leg is final.
- [KERNEL_TRANSFORM_COMPOSE]: the rigid map-conversion offset composes the kernel `Rasm` `Transform`/`Vector3` algebra via the seam `GeoReference.RotationRadians` direction-cosine projection (the X-axis abscissa/ordinate carried as a direction not an angle, `Math.Atan2(ordinate, abscissa)`, the IFC convention) at cross-folder alignment with the kernel transform owner, the datum `Reproject` running over `ProjNET` between two seam `GeoReference` frames BEFORE the rigid offset so the kernel transform stays datum-free; the rigid-offset construction stays the kernel transform's (the seam `GeoReference` carries the parameters, the kernel composes them), never a hand-rolled matrix.
