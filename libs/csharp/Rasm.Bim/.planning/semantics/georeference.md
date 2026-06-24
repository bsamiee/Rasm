# [BIM_COORDINATE_REFERENCE]

The IFC4.3 coordinate-reference owner: one host-neutral `GeoReference` record (eastings, northings, orthogonal height, X-axis abscissa/ordinate rotation, scale, EPSG/CRS name) projected from `IfcMapConversion`/`IfcProjectedCRS`/`IfcCoordinateOperation`, reconciled into the canonical kernel frame by the `Exchange/format#FORMAT_AXIS` `FrameNormalization` so every federated model shares one georeferenced origin, and a datum-to-datum reprojection leg over the admitted `ProjNET` engine so a multi-CRS federation reconciles every model onto the shared projected frame by its full geodetic transform (ellipsoid, datum shift, projection) before the rigid map-conversion offset applies. The rigid map-conversion transform composes the kernel `Rasm` transform algebra, never a hand-rolled matrix; the `GeoReference` leg extends `FrameNormalization` with a translation/rotation/scale overload distinct from the existing up-axis/handedness `Canonicalize` signature rather than minting a new transform owner; the geodetic reprojection is the new datum-bridging leg the `IfcCoordinateOperation`/`IfcProjectedCRS` chain demands, the `ProjNET` `CoordinateSystemServices` SRID-keyed transformation owning the datum shift the kernel `Rasm` transform does not. The page composes the kernel `Rasm` transform algebra, the `ProjNET` `CoordinateSystemServices`/`MathTransform` surface, and the `Model/structure#ASSEMBLY_TREE` tree root as settled vocabulary so federated assemblies reconcile onto one frame. The page is HOST-NEUTRAL.

## [01]-[INDEX]

- [01]-[GEO_REFERENCE]: `GeoReference` record, the `IfcMapConversion`/`IfcProjectedCRS` `Project` body, and the `FrameNormalization.Georeference` CRS overload.
- [02]-[GEODETIC_TRANSFORM]: the `GeoReference.Reproject` datum-to-datum leg over `ProjNET` `CoordinateSystemServices`/`MathTransform`, composed before the rigid map-conversion offset.

## [02]-[GEO_REFERENCE]

- Owner: `GeoReference` the host-neutral coordinate-reference record carrying the map-conversion translation (eastings/northings/orthogonal height), the X-axis abscissa/ordinate rotation, the scale, and the EPSG/CRS identity; `ProjectedCrs` the value-object EPSG-keyed CRS identity carrying the geodetic datum and map-projection name.
- Entry: `GeoReference.Project(IfcSemanticModel semantic)` projects the model's `IfcMapConversion`/`IfcProjectedCRS` (the `Exchange/import#IMPORT_RAIL` `MapConversionRow` the `HasCoordinateOperation` extract carries) into the host-neutral `GeoReference` — a model carrying no map-conversion returns `GeoReference.Identity` so ingest never blocks; `GeoReference.ToTransform()` builds the rigid map-conversion transform over the kernel `Rasm` transform algebra so the CRS reconciles into the canonical kernel frame at ingest.
- Auto: `Project` reads the model's `IfcMapConversion` eastings/northings/orthogonal-height translation, its X-axis abscissa/ordinate rotation cosine/sine pair, and its scale, and the referenced `IfcProjectedCRS` EPSG name and geodetic datum, into the `GeoReference` columns; `ToTransform` composes the kernel `Rasm` rotation (from the X-axis abscissa/ordinate pair, the IFC convention carrying the rotation as a direction cosine rather than an angle), translation, and uniform scale into one rigid transform the `FrameNormalization.Georeference` overload applies after the up-axis/handedness `Canonicalize` so a federated model's local origin lands at the shared real-world origin; the `Model/structure#ASSEMBLY_TREE` tree root reconciles onto this frame so federated assemblies share an origin.
- Receipt: the `GeoReference` record is the coordinate-reference evidence a federated clash or an infrastructure placement reads; the transform it builds is the kernel `Rasm` transform the import frame normalization applies, never a second matrix.
- Packages: GeometryGymIFC_Core, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm
- Growth: a new CRS is one `ProjectedCrs` value-object EPSG row the `ProjNET` cache resolves; a new map-conversion parameter is one column on `GeoReference` folded into the kernel transform; the rigid map-conversion offset is one `FrameNormalization.Georeference` overload distinct from `Canonicalize` and the datum shift is the one `[3]-[GEODETIC_TRANSFORM]` `Reproject` leg; never a new transform owner and never a per-CRS class.
- Boundary: the `GeoReference` is HOST-NEUTRAL — it projects the IFC CRS surface onto a portable record and a host-bound coordinate type is the named seam violation; the projection rides the GeometryGym `IfcMapConversion`/`IfcProjectedCRS`/`IfcCoordinateOperation` surface consumed as settled vocabulary — a hand-rolled CRS parser is the deleted form; the map-conversion transform composes the kernel `Rasm` transform algebra and a hand-rolled rotation/translation/scale matrix is the named defect; the CRS leg EXTENDS `FrameNormalization` with a `Georeference` overload carrying the translation/rotation/scale distinct from the existing up-axis/handedness `Canonicalize` rather than minting a new transform owner, so the CRS reconciles into the canonical kernel frame at the same ingest seam the up-axis coercion runs; the `Model/structure#ASSEMBLY_TREE` tree root names this seam and reconciles onto the `GeoReference` frame so federated assemblies share one origin, never a per-model origin; a non-georeferenced model returns `GeoReference.Identity` so ingest never blocks on a missing CRS.

```csharp signature
[ValueObject<string>]
[KeyMemberEqualityComparer<InterchangeKeyPolicy, string>]
public sealed partial class ProjectedCrs {
    static partial void NormalizeValidate(ref string value) => value = value.Trim().ToUpperInvariant();

    public Option<int> Epsg =>
        Value.StartsWith("EPSG:", StringComparison.OrdinalIgnoreCase) && int.TryParse(Value.AsSpan(5), out var code)
            ? Some(code)
            : Option<int>.None;
}

public sealed partial record GeoReference(
    double Eastings,
    double Northings,
    double OrthogonalHeight,
    double XAxisAbscissa,
    double XAxisOrdinate,
    double Scale,
    Option<ProjectedCrs> SourceCrs,
    Option<ProjectedCrs> Crs) {
    public static readonly GeoReference Identity = new(0.0, 0.0, 0.0, 1.0, 0.0, 1.0, Option<ProjectedCrs>.None, Option<ProjectedCrs>.None);

    public bool IsGeoreferenced => Crs.IsSome || Eastings != 0.0 || Northings != 0.0;

    public static GeoReference Project(IfcSemanticModel semantic) =>
        semantic.MapConversion.Match(
            None: () => Identity,
            Some: row => new GeoReference(
                row.Eastings, row.Northings, row.OrthogonalHeight,
                row.XAxisAbscissa, row.XAxisOrdinate, row.Scale,
                ProjectedCrs.TryCreate(row.SourceCrsName).ToOption(),
                ProjectedCrs.TryCreate(row.TargetCrsName).ToOption()));

    public Transform ToTransform() {
        double rotation = Math.Atan2(XAxisOrdinate, XAxisAbscissa);
        return Transform.Translation(new Vector3(Eastings, Northings, OrthogonalHeight))
            * Transform.Rotation(rotation, Vector3.UnitZ, Vector3.Zero)
            * Transform.Scale(Scale);
    }
}
```

```csharp signature
public static partial class FrameNormalization {
    public static void Georeference(GeoReference reference, Span<float> vertices, int stride) {
        if (!reference.IsGeoreferenced) {
            return;
        }
        reference.Reproject(vertices, stride);
        var transform = reference.ToTransform();
        for (int offset = 0; offset + 2 < vertices.Length; offset += stride) {
            var point = transform.Apply(new Vector3(vertices[offset], vertices[offset + 1], vertices[offset + 2]));
            (vertices[offset], vertices[offset + 1], vertices[offset + 2]) = ((float)point.X, (float)point.Y, (float)point.Z);
        }
    }
}
```

## [03]-[GEODETIC_TRANSFORM]

- Owner: the `GeoReference.Reproject` datum-bridging leg — a NET-NEW operation on the `GeoReference` owner (distinct from `ToTransform`, which stays the rigid map-conversion offset) reprojecting model ordinates from the source CRS datum to the target projected CRS over the admitted `ProjNET` `CoordinateSystemServices` before the rigid map-conversion offset applies; `CoordinateServices` the one process-wide `CoordinateSystemServices` SRID-keyed cache the `.api/api-projnet` `CRS_TRANSFORM` law names as the single CS/transformation owner.
- Entry: `GeoReference.Reproject(Span<float> vertices, int stride)` applies the EPSG-keyed datum-to-datum transform in place when both the source and target `ProjectedCrs.Epsg` resolve and differ — the `SourceCrs.Epsg` and `Crs.Epsg` drive `CoordinateServices.CreateTransformation(sourceSrid, targetSrid)`, the resulting `ICoordinateTransformation.MathTransform` reprojecting each ordinate triple through `MathTransform.Transform(ref x, ref y, ref z)`; a model with no source CRS, a single CRS, or unresolved EPSG codes returns unchanged so the datum leg is additive and never blocks a single-datum federation.
- Packages: ProjNET, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm
- Growth: a new CRS pair is one EPSG-keyed `CreateTransformation` the `CoordinateSystemServices` cache resolves from the bundled `SRID.csv`; a new projection or datum is the `ProjNET` library's, resolved from the EPSG code, never a hand-rolled Bursa-Wolf matrix; the datum leg is one `Reproject` op composed once before `ToTransform`, never a second transform owner. The per-ordinate `MathTransform.Transform(ref x, ref y, ref z)` loop is the readable form that widens each `float` ordinate to a `double` local inline; the dense form the `.api/api-projnet#CRS_TRANSFORM` batch rail names is one `MathTransform.Transform(Span<double> xs, Span<double> ys, Span<double> zs, …)` call over a pooled `double` staging buffer widened from the `Span<float>` columns (every `ProjNET` batch overload is double-precision — `XY`/`XYZ` are `double`-backed and no `float` overload exists, so a raw `MemoryMarshal.Cast<float,double>` is the rejected form), narrowed back after the batch — a vertex-count-gated densification, not a new owner.
- Boundary: the datum reprojection is `ProjNET`'s — the `CoordinateSystemServices.CreateTransformation` SRID facade and the `MathTransform.Transform` Bursa-Wolf 7-parameter datum shift plus projection own the full geodetic transform (ellipsoid, datum shift, projection), and a hand-rolled datum shift or a per-call `CoordinateTransformationFactory` rebuild is the deleted form per the `.api/api-projnet` single-cache-owner law; the reprojection composes BEFORE the rigid map-conversion offset so a model lands in the shared datum before its local-engineering-frame placement applies — the map-conversion stays the kernel `Rasm` transform's rigid offset, the datum shift is the new `ProjNET` leg, never the kernel transform re-implementing a datum shift; the leg is additive — a non-georeferenced or single-datum model returns unchanged so `Reproject` never blocks ingest; the `ProjectedCrs.Epsg` Option drives the SRID lookup and an unresolvable EPSG returns unchanged rather than faulting, the rigid offset still reconciling the local frame.

```csharp signature
public sealed partial record GeoReference {
    static readonly CoordinateSystemServices CoordinateServices = new();

    public void Reproject(Span<float> vertices, int stride) {
        var pair = from source in SourceCrs.Bind(static c => c.Epsg)
                   from target in Crs.Bind(static c => c.Epsg)
                   where source != target
                   select (source, target);
        if (pair.IsNone) {
            return;
        }
        var (sourceSrid, targetSrid) = pair.Head();
        var transform = CoordinateServices.CreateTransformation(sourceSrid, targetSrid).MathTransform;
        for (int offset = 0; offset + 2 < vertices.Length; offset += stride) {
            double x = vertices[offset], y = vertices[offset + 1], z = vertices[offset + 2];
            transform.Transform(ref x, ref y, ref z);
            (vertices[offset], vertices[offset + 1], vertices[offset + 2]) = ((float)x, (float)y, (float)z);
        }
    }
}
```

## [04]-[RESEARCH]

- [MAP_CONVERSION_PROJECTION]: the GeometryGym `IfcMapConversion` (`Eastings`/`Northings`/`OrthogonalHeight`/`XAxisAbscissa`/`XAxisOrdinate`/`Scale`/`SourceCRS`/`TargetCRS`), `IfcProjectedCRS` (`Name`/`GeodeticDatum`/`VerticalDatum`/`MapProjection`/`MapZone`), and the single `IfcGeometricRepresentationContext.HasCoordinateOperation` `IfcCoordinateOperation` the `GeoReference.Project` body reads are catalogued at `.api/api-geometrygym-ifc` (georeferencing entity scope + traversal entrypoint) so the `MapConversionRow` projection the `Exchange/import#IMPORT_RAIL` `MapConversion` extract carries matches the real entity members; `HasCoordinateOperation` is a single `IfcCoordinateOperation` reference (the `IfcMapConversion` is itself an `IfcCoordinateOperation`), narrowed by `as IfcMapConversion` at the extract, not a collection; `XAxisAbscissa`/`XAxisOrdinate`/`Scale` are non-nullable doubles GeometryGym defaults on parse, so the import extract guards only the degenerate `Scale == 0.0` onto the canonical `1.0`.
- [KERNEL_TRANSFORM_COMPOSE]: the kernel `Rasm` `Transform.Translation`/`Transform.Rotation`/`Transform.Scale`/`Transform.Apply` and `Vector3`/`Vector3.UnitZ` member spellings the `GeoReference.ToTransform` and `FrameNormalization.Georeference` compose ground against the kernel `Rasm` transform-algebra owner at cross-folder alignment so the rigid map-conversion transform reuses the kernel transform rather than a hand-rolled matrix; the X-axis abscissa/ordinate direction-cosine-to-rotation conversion (`Math.Atan2(ordinate, abscissa)`, the IFC convention carrying the rotation as a direction rather than an angle) is settled and the kernel transform composition order (translation after rotation after scale) confirms against the kernel transform algebra; the datum `Reproject` leg runs over `ProjNET` `MathTransform` BEFORE the rigid offset so the kernel transform stays datum-free.
- [GEO_FRAME_SEAM]: the `Exchange/import#IMPORT_RAIL` `Semantic` extract carries the `MapConversionRow` (reached through `IfcGeometricRepresentationContext.HasCoordinateOperation`) and the `FrameNormalization.Georeference` overload runs at the import `Framed` site after the up-axis `Canonicalize` — the datum `Reproject`, the rigid map-conversion offset, and the up-axis coercion compose at one ingest seam so the georeferencing leg consumes the same `IfcSemanticModel` the other sub-domains read, never a second graph.
