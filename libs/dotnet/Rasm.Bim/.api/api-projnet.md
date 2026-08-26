# [RASM_BIM_API_PROJNET]

`ProjNET` owns pure-managed .NET coordinate-system resolution and transformation: EPSG/SRID-keyed CRS lookup, WKT/ESRI parsing, geographic-to-projected and datum-to-datum coordinate transformation, and the `MathTransform` numeric pipeline. It carries no native PROJ and no geometry kernel — the managed planar-algebra transform engine feeding the `Semantics/georeference#GEODETIC_TRANSFORM` datum-bridge leg.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: coordinate-system services and factories

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY]  | [CAPABILITY]                                                                      |
| :-----: | :-------------------------------- | :------------- | :-------------------------------------------------------------------------------- |
|  [01]   | `CoordinateSystemServices`        | class          | SRID-keyed CS cache + transformation facade; top-level `ProjNet` ns               |
|  [02]   | `CoordinateSystemFactory`         | class          | CS factory; `CreateFromWkt`/`CreateGeographicCoordinateSystem`/`CreateProjected…` |
|  [03]   | `CoordinateTransformationFactory` | class          | builds transformations; `CreateFromCoordinateSystems(source, target)`             |
|  [04]   | `ICoordinateTransformation`       | interface      | built transformation; members below                                               |
|  [05]   | `MathTransform`                   | abstract class | numeric transform pipeline; members in `[03]`                                     |

- `CoordinateSystemServices` ctors: `()` (seeds EPSG:4326 `WGS84` and 3857 `WebMercator` only), `(IEnumerable<KeyValuePair<int,string>>)` (SRID→WKT definition source), `(CoordinateSystemFactory, CoordinateTransformationFactory[, IEnumerable<KeyValuePair<int,string>>])`.
- `ICoordinateTransformation` members: `MathTransform`, `SourceCS`, `TargetCS`, `TransformType`, `Authority`, `AuthorityCode`.

[PUBLIC_TYPE_SCOPE]: coordinate-system models and units

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY]  | [CAPABILITY]                                                                      |
| :-----: | :--------------------------- | :------------- | :-------------------------------------------------------------------------------- |
|  [01]   | `CoordinateSystem`           | abstract class | CS base; `WKT`, `Authority`, `AuthorityCode`, `Dimension`                         |
|  [02]   | `GeographicCoordinateSystem` | class          | lat/lon CS; `AngularUnit`, `HorizontalDatum`, `PrimeMeridian`, static `WGS84`     |
|  [03]   | `ProjectedCoordinateSystem`  | class          | planar CS; `LinearUnit`/`Projection`/`WGS84_UTM(zone, north)`/`WebMercator`       |
|  [04]   | `HorizontalDatum`            | class          | geodetic datum; `Ellipsoid`, `Wgs84Parameters` (Bursa-Wolf 7-param shift)         |
|  [05]   | `Ellipsoid`                  | class          | reference ellipsoid; `SemiMajorAxis`, `InverseFlattening`, static `WGS84`/`GRS80` |
|  [06]   | `Projection`                 | class          | map-projection parameter set; named projection + `ProjectionParameter` list       |
|  [07]   | `LinearUnit`                 | class          | linear unit; static `Metre`, conversion factor                                    |
|  [08]   | `AngularUnit`                | class          | angular unit; static `Degrees`, conversion factor                                 |

[PUBLIC_TYPE_SCOPE]: WKT parsing

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY] | [CAPABILITY]                                                       |
| :-----: | :-------------------------- | :------------ | :----------------------------------------------------------------- |
|  [01]   | `CoordinateSystemWktReader` | static class  | `Parse(string) -> IInfo` (cast to `CoordinateSystem`) from OGC WKT |
|  [02]   | `MathTransformWktReader`    | static class  | `Parse(string) -> MathTransform` from a transform WKT              |

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: build a transformation by EPSG/SRID or WKT

| [INDEX] | [SURFACE]                                                             | [SHAPE]  | [CAPABILITY]                     |
| :-----: | :-------------------------------------------------------------------- | :------- | :------------------------------- |
|  [01]   | `CoordinateSystemServices.GetCoordinateSystem(int)`                   | instance | CS from an EPSG/SRID code        |
|  [02]   | `CoordinateSystemServices.GetCoordinateSystem(string, long)`          | instance | CS by authority + code           |
|  [03]   | `CoordinateSystemServices.CreateTransformation(int, int)`             | instance | transformation between two SRIDs |
|  [04]   | `CoordinateSystemServices.CreateTransformation(CS, CS)`               | instance | facade forward to the factory    |
|  [05]   | `CoordinateSystemFactory.CreateFromWkt(string) -> CoordinateSystem`   | instance | parse a WKT CS definition        |
|  [06]   | `CoordinateTransformationFactory.CreateFromCoordinateSystems(CS, CS)` | instance | transformation between two CS    |

[ENTRYPOINT_SCOPE]: transform coordinates — `MathTransform.Transform` overloads

| [INDEX] | [SURFACE]                                                    | [SHAPE]  | [CAPABILITY]                                         |
| :-----: | :----------------------------------------------------------- | :------- | :--------------------------------------------------- |
|  [01]   | `Transform(double, double)`                                  | instance | one 2D coordinate, tuple return                      |
|  [02]   | `Transform(double, double, double)`                          | instance | one 3D coordinate, tuple return                      |
|  [03]   | `Transform(ref double, ref double, ref double)`              | abstract | 3D in place; the core every concrete overrides       |
|  [04]   | `Transform(double[]) -> double[]`                            | instance | single ordinate array                                |
|  [05]   | `Transform(Span<XYZ>)`                                       | instance | `double`-XYZ batch (24 B struct)                     |
|  [06]   | `Transform(Span<XY>, Span<double>, int)`                     | instance | `double`-XY + optional Z (16 B; `strideZ` default 0) |
|  [07]   | `Transform(Span<double>, Span<double>, int, int)`            | instance | strided `double` XY, Z=0 (`stride` default 1)        |
|  [08]   | `Transform(Span<double>, Span<double>, Span<double>, int×3)` | instance | strided `double` struct-of-arrays batch              |

[ENTRYPOINT_SCOPE]: inverse, dimensionality, accessor

| [INDEX] | [SURFACE]                                               | [SHAPE]  | [CAPABILITY]                                                  |
| :-----: | :------------------------------------------------------ | :------- | :------------------------------------------------------------ |
|  [01]   | `MathTransform.TransformList(IList<double[]>)`          | instance | array-of-arrays batch (allocating fallback)                   |
|  [02]   | `MathTransform.Inverse() -> MathTransform` / `Invert()` | instance | reverse transform (target → source); every concrete overrides |
|  [03]   | `MathTransform.DimSource` / `DimTarget`                 | property | source/target ordinate dimensionality                         |
|  [04]   | `ICoordinateTransformation.MathTransform`               | property | the numeric transform of a built transformation               |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Coordinate transformation is pure double-precision managed numeric algebra — no geometry kernel, no topology, no native PROJ. Every batch overload operates on `double` ordinates (`XY` 16 B, `XYZ` 24 B, both `double`-backed); no `Span<float>`/`float[]` overload exists.
- `CoordinateSystemServices.CreateTransformation(sourceSrid, targetSrid)` yields an `ICoordinateTransformation` whose `.MathTransform` transforms ordinates; `MathTransform.Inverse()` returns the reverse pipeline, so forward-then-inverse recovers the source within numeric tolerance.
- A `HorizontalDatum.Wgs84Parameters` Bursa-Wolf 7-parameter set drives the datum-to-datum shift `CoordinateTransformationFactory` concatenates between two `ProjectedCoordinateSystem` instances with distinct datums; each `ProjectedCoordinateSystem` resolves its `Projection` and `ProjectionParameter` list from the EPSG/WKT definition.
- `CoordinateSystemServices()` parameterless seeds only EPSG:4326 (`GeographicCoordinateSystem.WGS84`) and 3857 (`ProjectedCoordinateSystem.WebMercator`) from its private `DefaultInitialization`; a broader EPSG set enters through the `IEnumerable<KeyValuePair<int,string>>` SRID→WKT definitions ctor, enumerated ONCE on a background task at construction, so a code absent from the seed can never be added afterwards (`AddCoordinateSystem` is `protected`).
- `ProjNET` ships NO EPSG registry file and no code-to-definition resolver: `GetCoordinateSystem(srid)` answers `null` for an unseeded code and `CreateTransformation` forwards that `null` into `CreateFromCoordinateSystems`. Every consumer covering arbitrary EPSG frames resolves each definition itself — the OSR `ImportFromEPSG`/`ExportToWkt` pair over the staged PROJ database is the solution's source — then drives `CoordinateSystemFactory.CreateFromWkt` and `CoordinateTransformationFactory` directly behind its own cache.
- Phantom members, never cited: `MathTransform.Derivative(double[])`, `GetDomainFlags(List<double>)`, and `GetCodomainConvexHull(List<double>)` are base-only `virtual`, throwing `NotImplementedException` with no override; every factory-built `ICoordinateTransformation` returns `string.Empty` for `AreaOfUse`. Distortion evidence is a central-difference Jacobian probe over the same transform; the domain guard a post-transform non-finite scan (out-of-domain emits NaN).

[STACKING]:
- `MaxRev.Gdal.Core`(`.api/api-maxrev-gdal.md`): OSR escalation counterpart. A transform this managed engine cannot express — a PROJ grid-shift datum, or a dynamic/plate-motion datum carrying a coordinate epoch — escalates to `OSGeo.OSR` `new CoordinateTransformation(src, dst[, CoordinateTransformationOptions])` over `SpatialReference` (PROJ-backed, `IsDynamic`/`GetCoordinateEpoch`). Both sides read the contract identically: OSR takes only what the planar algebra cannot reach, `ProjNET` owns every transform that stays managed.
- within-lib: the `Semantics/georeference` `Reproject` leg holds `double` end to end and batches once in place over an interleaved `Span<double>` through the strided struct-of-arrays overload `Transform(xs, ys, zs, stride, stride, stride)`. A natively-`double` `(x,y,z)` triple buffer maps directly to `Transform(Span<XYZ>)`; a `float` vertex span stages the three ordinate columns through a pooled `double` buffer (a `MemoryMarshal.Cast<float,double>` reinterpret misreads the bytes).

[LOCAL_ADMISSION]:
- CRS resolution enters through `CoordinateSystemServices.GetCoordinateSystem(srid)` or `CoordinateSystemFactory.CreateFromWkt`; transformation construction through `CoordinateSystemServices.CreateTransformation` or `CoordinateTransformationFactory.CreateFromCoordinateSystems`.
- Coordinate transformation enters through `MathTransform.Transform`; batch through the strided `Span<double>`/`Span<XY>`/`Span<XYZ>` overloads, with `TransformList` the allocating fallback.
- `CoordinateSystemFactory` and `CoordinateTransformationFactory` stay single shared owners behind a consumer-held cache; a per-call `new` rebuild of either is the rejected form. `CoordinateSystemServices` wraps that pair around a two-code seed, so it serves only a consumer whose SRID set is the seeded one.
