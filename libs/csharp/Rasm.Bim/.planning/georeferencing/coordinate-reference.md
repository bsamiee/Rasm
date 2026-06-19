# [BIM_COORDINATE_REFERENCE]

The IFC4.3 coordinate-reference owner: one host-neutral `GeoReference` record (eastings, northings, orthogonal height, X-axis abscissa/ordinate rotation, scale, EPSG/CRS name) projected from `IfcMapConversion`/`IfcProjectedCRS`/`IfcCoordinateOperation`, reconciled into the canonical kernel frame by the `exchange/format-axis#FORMAT_AXIS` `FrameNormalization` so every federated model shares one georeferenced origin. The rigid map-conversion transform composes the kernel `Rasm` transform algebra, never a hand-rolled matrix; the `GeoReference` leg extends `FrameNormalization` with a translation/rotation/scale overload distinct from the existing up-axis/handedness `Canonicalize` signature rather than minting a new transform owner. The page composes the kernel `Rasm` transform algebra and the `assembly/spatial-structure#ASSEMBLY_TREE` tree root as settled vocabulary so federated assemblies reconcile onto one frame. The page is HOST-NEUTRAL.

## [1]-[INDEX]

- [2]-[GEO_REFERENCE]: `GeoReference` record, the `IfcMapConversion`/`IfcProjectedCRS` projection, and the `FrameNormalization.Georeference` CRS overload.

## [2]-[GEO_REFERENCE]

- Owner: `GeoReference` the host-neutral coordinate-reference record carrying the map-conversion translation (eastings/northings/orthogonal height), the X-axis abscissa/ordinate rotation, the scale, and the EPSG/CRS identity; `ProjectedCrs` the value-object EPSG-keyed CRS identity carrying the geodetic datum and map-projection name.
- Entry: `GeoReference.Project(IfcSemanticModel semantic)` projects the model's `IfcMapConversion`/`IfcProjectedCRS` into the host-neutral `GeoReference` — `Fin<T>` aborts on a model carrying no map-conversion (a non-georeferenced model returns `GeoReference.Identity`, never a fault) or an unreconcilable CRS the kernel transform algebra cannot express (`faults#FAULT_BAND` `BimFault.CapabilityMiss`) lowered with `.ToError()`; `GeoReference.ToTransform()` builds the rigid map-conversion transform over the kernel `Rasm` transform algebra so the CRS reconciles into the canonical kernel frame at ingest.
- Auto: `Project` reads the model's `IfcMapConversion` eastings/northings/orthogonal-height translation, its X-axis abscissa/ordinate rotation cosine/sine pair, and its scale, and the referenced `IfcProjectedCRS` EPSG name and geodetic datum, into the `GeoReference` columns; `ToTransform` composes the kernel `Rasm` rotation (from the X-axis abscissa/ordinate pair, the IFC convention carrying the rotation as a direction cosine rather than an angle), translation, and uniform scale into one rigid transform the `FrameNormalization.Georeference` overload applies after the up-axis/handedness `Canonicalize` so a federated model's local origin lands at the shared real-world origin; the `assembly/spatial-structure#ASSEMBLY_TREE` tree root reconciles onto this frame so federated assemblies share an origin.
- Receipt: the `GeoReference` record is the coordinate-reference evidence a federated clash or an infrastructure placement reads; the transform it builds is the kernel `Rasm` transform the import frame normalization applies, never a second matrix.
- Packages: GeometryGymIFC_Core, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm
- Growth: a new CRS is one `ProjectedCrs` value-object EPSG row; a new map-conversion parameter is one column on `GeoReference` folded into the kernel transform; the CRS leg is one `FrameNormalization.Georeference` overload distinct from `Canonicalize`; never a new transform owner and never a per-CRS class.
- Boundary: the `GeoReference` is HOST-NEUTRAL — it projects the IFC CRS surface onto a portable record and a host-bound coordinate type is the named seam violation; the projection rides the GeometryGym `IfcMapConversion`/`IfcProjectedCRS`/`IfcCoordinateOperation` surface consumed as settled vocabulary — a hand-rolled CRS parser is the deleted form; the map-conversion transform composes the kernel `Rasm` transform algebra and a hand-rolled rotation/translation/scale matrix is the named defect; the CRS leg EXTENDS `FrameNormalization` with a `Georeference` overload carrying the translation/rotation/scale distinct from the existing up-axis/handedness `Canonicalize` rather than minting a new transform owner, so the CRS reconciles into the canonical kernel frame at the same ingest seam the up-axis coercion runs; the `assembly/spatial-structure#ASSEMBLY_TREE` tree root names this seam and reconciles onto the `GeoReference` frame so federated assemblies share one origin, never a per-model origin; a non-georeferenced model returns `GeoReference.Identity` so ingest never blocks on a missing CRS.

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

public sealed record GeoReference(
    double Eastings,
    double Northings,
    double OrthogonalHeight,
    double XAxisAbscissa,
    double XAxisOrdinate,
    double Scale,
    Option<ProjectedCrs> Crs) {
    public static readonly GeoReference Identity = new(0.0, 0.0, 0.0, 1.0, 0.0, 1.0, Option<ProjectedCrs>.None);

    public bool IsGeoreferenced => Crs.IsSome || Eastings != 0.0 || Northings != 0.0;

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
        var transform = reference.ToTransform();
        for (int offset = 0; offset + 2 < vertices.Length; offset += stride) {
            var point = transform.Apply(new Vector3(vertices[offset], vertices[offset + 1], vertices[offset + 2]));
            (vertices[offset], vertices[offset + 1], vertices[offset + 2]) = ((float)point.X, (float)point.Y, (float)point.Z);
        }
    }
}
```

## [3]-[RESEARCH]

- [MAP_CONVERSION_PROJECTION]: the GeometryGym `IfcMapConversion` (`Eastings`/`Northings`/`OrthogonalHeight`/`XAxisAbscissa`/`XAxisOrdinate`/`Scale`/`SourceCRS`/`TargetCRS`), `IfcProjectedCRS` (`Name`/`GeodeticDatum`/`VerticalDatum`/`MapProjection`/`MapZone`), and `IfcCoordinateOperation` member spellings the `GeoReference.Project` fold reads ground against the GeometryGym IFC4.3 georeferencing entity surface so the projection from the model's coordinate-operation graph matches the real entity members before the `Project` body is final; the `IfcMapConversion` is reached through the `IfcProject` `RepresentationContexts` `IfcGeometricRepresentationContext.HasCoordinateOperation` chain, whose traversal spelling confirms against the GeometryGym surface before the `IfcSemanticModel` carries a `MapConversionRow` projection family.
- [KERNEL_TRANSFORM_COMPOSE]: the kernel `Rasm` `Transform.Translation`/`Transform.Rotation`/`Transform.Scale`/`Transform.Apply` and `Vector3`/`Vector3.UnitZ` member spellings the `GeoReference.ToTransform` and `FrameNormalization.Georeference` compose ground against the kernel `Rasm` transform-algebra owner at cross-folder alignment so the rigid map-conversion transform reuses the kernel transform rather than a hand-rolled matrix; the X-axis abscissa/ordinate direction-cosine-to-rotation conversion (`Math.Atan2(ordinate, abscissa)`, the IFC convention carrying the rotation as a direction rather than an angle) is settled and the kernel transform composition order (translation after rotation after scale) confirms against the kernel transform algebra.
- [GEO_FRAME_SEAM]: the `IfcSemanticModel` carries no `MapConversionRow` family today, so the `exchange/import-rail#IMPORT_RAIL` `Semantic` extract widens with a one-row map-conversion projection feeding `GeoReference.Project`, and the `FrameNormalization.Georeference` overload runs at the import `Framed` site after the up-axis `Canonicalize` so the CRS reconciliation and the up-axis coercion compose at one ingest seam; the import-rail widening grounds at cross-page alignment so the georeferencing leg consumes the same `IfcSemanticModel` the other sub-domains read, never a second graph.
