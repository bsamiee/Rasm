# [RASM_BIM_API_PROJNET]

`ProjNET` supplies a pure-managed.NET spatial-reference and projection engine:
EPSG/SRID-keyed coordinate-system lookup, WKT and ESRI coordinate-system parsing,
geographic-to-projected and datum-to-datum coordinate transformation, and the
`MathTransform` numeric pipeline the `Semantics/georeference#GEO_REFERENCE`
`GEODETIC_TRANSFORM` datum-bridging cluster composes over the rigid map-conversion frame.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ProjNET`
- package: `ProjNET`
- license: MIT (NetTopologySuite ecosystem)
- assembly: `ProjNet`
- namespace: `ProjNet` (top-level — owns `CoordinateSystemServices`)
- namespace: `ProjNet.CoordinateSystems`
- namespace: `ProjNet.CoordinateSystems.Transformations`
- namespace: `ProjNet.CoordinateSystems.Projections`
- namespace: `ProjNet.IO.CoordinateSystems`
- asset: netstandard2.0, netstandard2.1 ONLY; the net10.0 consumer binds the `lib/netstandard2.1` asset. No net6.0/net8.0 asset ships — the bound surface is the netstandard2.1 build, whose `MathTransform` carries the `Span<double>`/`Span<XY>`/`Span<XYZ>` batch overloads (the netstandard2.0 build has the same managed surface)
- asset: IL-only AnyCPU managed assembly; thread-safe `CoordinateSystemServices` cache; no P/Invoke, no native PROJ
- rail: geometry

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: coordinate-system services and factories
- package: `ProjNET`
- namespace: `ProjNet` (services), `ProjNet.CoordinateSystems`, `ProjNet.CoordinateSystems.Transformations`
- rail: geometry

| [INDEX] | [SYMBOL] | [RAIL] | [CAPABILITY] |
|:-----: |:-------------------------------- |:------- |:-------------------------------------------------------------------------------------------------------- |
| [01] | `ProjNet.CoordinateSystemServices` | geometry | SRID-keyed CS cache + transformation facade; `GetCoordinateSystem`, `CreateTransformation`, `GetSRID`. Top-level `ProjNet` namespace, NOT `ProjNet.CoordinateSystems`. Ctors: `()` (bundled `SRID.csv`), `(IEnumerable<KeyValuePair<int,string>> definitions)` (custom SRID->WKT source), `(CoordinateSystemFactory, CoordinateTransformationFactory[, IEnumerable<KeyValuePair<int,string>>])` |
| [02] | `CoordinateSystemFactory` | geometry | builds CS objects; `CreateFromWkt`, `CreateGeographicCoordinateSystem`, `CreateProjectedCoordinateSystem` |
| [03] | `CoordinateTransformationFactory` | geometry | builds transformations; `CreateFromCoordinateSystems(source, target)` |
| [04] | `ICoordinateTransformation` | geometry | a built transformation; `MathTransform`, `SourceCS`, `TargetCS`, `TransformType`, `Authority`/`AuthorityCode`; `AreaOfUse` is `string.Empty` on every factory-built instance (`[PHANTOM_SURFACE]`) |
| [05] | `MathTransform` | geometry | numeric transform pipeline; abstract `Transform(ref x,ref y,ref z)` + per-coord, array, list, and `Span<double>`/`Span<XY>`/`Span<XYZ>` batch overloads; `Inverse`/`Invert`, `DimSource`/`DimTarget`; `Derivative`/`GetDomainFlags`/`GetCodomainConvexHull` are member surface-refuted phantoms (`[PHANTOM_SURFACE]`) |

[PUBLIC_TYPE_SCOPE]: coordinate-system models and units
- package: `ProjNET`
- namespace: `ProjNet.CoordinateSystems`
- rail: geometry

| [INDEX] | [SYMBOL] | [RAIL] | [CAPABILITY] |
|:-----: |:--------------------------- |:------- |:----------------------------------------------------------------------------------------------------------- |
| [01] | `CoordinateSystem` | geometry | abstract CS base; `WKT`, `Authority`, `AuthorityCode`, `Dimension` |
| [02] | `GeographicCoordinateSystem` | geometry | lat/lon CS; `AngularUnit`, `HorizontalDatum`, `PrimeMeridian`, static `WGS84` |
| [03] | `ProjectedCoordinateSystem` | geometry | planar CS; `GeographicCoordinateSystem`, `LinearUnit`, `Projection`, `WGS84_UTM(zone, north)`, `WebMercator` |
| [04] | `HorizontalDatum` | geometry | geodetic datum; `Ellipsoid`, `Wgs84Parameters` (Bursa-Wolf 7-param shift) |
| [05] | `Ellipsoid` | geometry | reference ellipsoid; `SemiMajorAxis`, `InverseFlattening`, static `WGS84`/`GRS80` |
| [06] | `Projection` | geometry | map-projection parameter set; named projection + `ProjectionParameter` list |
| [07] | `LinearUnit` / `AngularUnit` | geometry | unit definitions; static `Metre`, `Degrees`, conversion factor |

[PUBLIC_TYPE_SCOPE]: WKT/SRID I/O
- package: `ProjNET`
- namespace: `ProjNet.IO.CoordinateSystems`
- rail: geometry

| [INDEX] | [SYMBOL] | [RAIL] | [CAPABILITY] |
|:-----: |:-------------------------- |:------- |:------------------------------------------------------------------- |
| [01] | `CoordinateSystemWktReader` | geometry | parses an OGC WKT string into a `CoordinateSystem` (`Parse(string)`) |
| [02] | `SridReader` | geometry | SRID-to-WKT lookup from the bundled `SRID.csv` EPSG table |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: build a transformation by EPSG/SRID
- package: `ProjNET`
- namespace: `ProjNet.CoordinateSystems`, `ProjNet.CoordinateSystems.Transformations`
- rail: geometry

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
|:-----: |:------------------------------------------------------------ |:--------------------------------------------------------------------------------- |:--------------------------------------------- |
| [01] | `CoordinateSystemServices.GetCoordinateSystem` | `(int srid)` → `CoordinateSystem` | resolves a CS from an EPSG/SRID code |
| [02] | `CoordinateSystemServices.GetCoordinateSystem` | `(string authority, long code)` → `CoordinateSystem` | resolves a CS by authority + code |
| [03] | `CoordinateSystemServices.CreateTransformation` | `(int sourceSrid, int targetSrid)` → `ICoordinateTransformation` | builds a transformation between two SRIDs |
| [04] | `CoordinateSystemFactory.CreateFromWkt` | `(string wkt)` → `CoordinateSystem` | parses a WKT CS definition |
| [05] | `CoordinateTransformationFactory.CreateFromCoordinateSystems` | `(CoordinateSystem source, CoordinateSystem target)` → `ICoordinateTransformation` | builds a transformation between two CS objects |

[ENTRYPOINT_SCOPE]: transform coordinates
- package: `ProjNET`
- namespace: `ProjNet.CoordinateSystems.Transformations`
- rail: geometry

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
|:-----: |:---------------------------------------- |:---------------------------------------------- |:---------------------------------------------- |
| [01] | `MathTransform.Transform` | `(double x, double y)` → `(double x, double y)` | transforms one 2D coordinate |
| [02] | `MathTransform.Transform` | `(double x, double y, double z)` → `(double, double, double)` | transforms one 3D coordinate, returns tuple |
| [03] | `MathTransform.Transform` | `(ref double x, ref double y, ref double z)` | abstract core; transforms one 3D coordinate in place |
| [04] | `MathTransform.Transform` | `(double[] point)` → `double[]` | transforms a single ordinate array |
| [05] | `MathTransform.Transform` | `(Span<XYZ> xyzs)` | strongly-typed `double`-XYZ batch in place; `XYZ` is `double`-backed (24 B), so this aliases a `double` triple buffer, NOT a `float` glTF buffer |
| [06] | `MathTransform.Transform` | `(Span<XY> xys, Span<double> zs = default, int strideZ = 0)` | `double`-XY batch + optional Z column; `XY` is `double`-backed (16 B) |
| [07] | `MathTransform.Transform` | `(Span<double> xs, Span<double> ys, int strideX=1, int strideY=1)` | strided `double` XY pair batch (Z assumed 0); the 2-column twin of `[08]` |
| [08] | `MathTransform.Transform` | `(Span<double> xs, Span<double> ys, Span<double> zs, int strideX=1, int strideY=1, int strideZ=1)` | strided `double` struct-of-arrays batch over three reinterpreted ordinate columns of one `double` vertex span |
| [09] | `MathTransform.TransformList` | `(IList<double[]>)` → `IList<double[]>` | array-of-arrays batch (allocating) |
| [10] | `MathTransform.Inverse` / `Invert` | → `MathTransform` / in-place | the reverse transform (target → source); every concrete transform overrides `Inverse` |
| [11] | `MathTransform.DimSource`/`DimTarget` | property | source/target ordinate dimensionality |
| [12] | `ICoordinateTransformation.MathTransform` | property | the numeric transform of a built transformation |

## [04]-[IMPLEMENTATION_LAW]

[PLATFORM_BOUNDARY]:
- ships a single managed `ProjNet.dll` (netstandard2.0/ only); no P/Invoke, no native PROJ dependency, no `runtimes/` assets; the net10.0 consumer binds the netstandard2.1 asset
- transformation is pure double-precision numeric; no geometry kernel, no topology
- the bundled `SRID.csv` EPSG table is the default SRID source; a custom SRID->WKT source overrides it through the `CoordinateSystemServices(IEnumerable<KeyValuePair<int,string>> definitions)` constructor

[CRS_TRANSFORM]:
- transformation root: `CoordinateSystemServices.CreateTransformation(sourceSrid, targetSrid)` yields an `ICoordinateTransformation` whose `.MathTransform` transforms ordinates
- datum bridge: a `HorizontalDatum.Wgs84Parameters` Bursa-Wolf 7-parameter set drives the datum-to-datum shift the `CoordinateTransformationFactory` concatenates between two `ProjectedCoordinateSystem` instances with distinct datums
- projection axis: each `ProjectedCoordinateSystem` carries its `Projection` (Transverse Mercator, Lambert, etc.) and `ProjectionParameter` list resolved from the EPSG/WKT definition
- inverse: `MathTransform.Inverse()` returns the reverse pipeline so a round-trip (forward then inverse) recovers the source ordinate within numeric tolerance
- escalation seam: `ProjNET` is the default managed CRS/datum owner (no native dependency, the `georeference#GEODETIC_TRANSFORM` leg). A transform `ProjNET` cannot express — an exotic datum requiring a PROJ grid shift, or a dynamic/plate-motion datum carrying a coordinate epoch — escalates to the reciprocal `MaxRev.Gdal.Core` OSR engine (the `api-maxrev-gdal` catalog): a one-shot `new CoordinateTransformation(src, dst[, CoordinateTransformationOptions])` over `SpatialReference` (PROJ-backed, datum-grid-aware, `IsDynamic`/`GetCoordinateEpoch`). The seam reads identically from both sides — OSR escalates only what `ProjNET` cannot reach, and `ProjNET` owns every transform that stays in the managed planar algebra
- batch rail (integration): EVERY `MathTransform` batch overload is double-precision — the `Span<double>` strided overloads and the `XY`/`XYZ`-typed overloads all operate on `double` ordinates (`XY` is 16 B, `XYZ` is 24 B, both `double`-backed; there is NO `Span<float>`/`float[]` overload). The `Semantics/georeference#GEODETIC_TRANSFORM` `Reproject` leg holds `double` end to end and runs ONE strided struct-of-arrays batch IN PLACE over the three ordinate columns of its interleaved `Span<double>` buffer — `Transform(ordinates, ordinates[1..], ordinates[2..], stride, stride, stride)`, the `TransformCore` `while (num < xs.Length)` walk driving the count off the full-length first column so the last vertex is covered, a `stride` above three leaving non-position interleave columns untouched. A consumer whose buffer is natively `float` (a glTF vertex span) widens the three ordinate columns into a pooled `double` staging buffer, batches once, and narrows back — a raw `MemoryMarshal.Cast<float,double>` reinterpret misreads the bytes, and a per-vertex `Transform(ref x, ref y, ref z)` loop is the un-batched rejected form. A natively-`double` `(x,y,z)` triple buffer maps directly to `Transform(Span<XYZ>)` with no staging.

[PHANTOM_SURFACE]: member surface-refuted members — never cite, never compose
- `MathTransform.Derivative(double[])`, `GetDomainFlags(List<double>)`, and `GetCodomainConvexHull(List<double>)` are base-only `virtual` members throwing `NotImplementedException` with ZERO concrete overrides across the assembly — no transform computes a Jacobian, a domain verdict, or a codomain hull.
- every `CoordinateTransformationFactory`-built `ICoordinateTransformation` passes `string.Empty` for `AreaOfUse` — the EPSG area-of-validity is never populated.
- honest replacements (the `Semantics/georeference#GEODETIC_TRANSFORM` `Reprojection` receipt realizes both): local distortion evidence is a central-difference Jacobian probe over the SAME transform that shifted the buffer (`AnchorScale`/`AnchorConvergence`); the domain guard is the engine-agnostic post-transform non-finite scan — an out-of-domain reprojection emits NaN, never silent garbage.

[LOCAL_ADMISSION]:
- CRS resolution enters through `CoordinateSystemServices.GetCoordinateSystem(srid)` or `CoordinateSystemFactory.CreateFromWkt`.
- transformation construction enters through `CoordinateSystemServices.CreateTransformation` or `CoordinateTransformationFactory.CreateFromCoordinateSystems`.
- coordinate transformation enters through `MathTransform.Transform`; batch through the strided `Span<double>`/`Span<XY>`/`Span<XYZ>` overloads (`TransformList` is the allocating fallback).
- the `CoordinateSystemServices` cache is the single CS/transformation owner — a per-call factory rebuild is the rejected form.

[RAIL_LAW]:
- Package: `ProjNET`
- Owns: geodetic datum/projection coordinate transformation between EPSG-keyed coordinate systems
- Accept: CRS resolution, datum-to-datum reprojection, projected/geographic transformation
- Reject: geometry kernel, topology (`NetTopologySuite` owns the planar algebra), the rigid IFC map-conversion offset (the kernel `Rasm` transform owns that), native PROJ binding and exotic datum-grid/dynamic-datum transforms (`MaxRev.Gdal.Core` OSR is the escalation counterpart for what this managed engine cannot express)
