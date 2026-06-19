# [RASM_BIM_API_PROJNET]

`ProjNET` supplies a pure-managed .NET spatial-reference and projection engine:
EPSG/SRID-keyed coordinate-system lookup, WKT and ESRI coordinate-system parsing,
geographic-to-projected and datum-to-datum coordinate transformation, and the
`MathTransform` numeric pipeline the `Semantics/georeference#GEO_REFERENCE`
`GEODETIC_TRANSFORM` datum-bridging cluster composes over the rigid map-conversion frame.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ProjNET`
- package: `ProjNET`
- version: `2.1.0`
- assembly: `ProjNet`
- namespace: `ProjNet.CoordinateSystems`
- namespace: `ProjNet.CoordinateSystems.Transformations`
- namespace: `ProjNet.CoordinateSystems.Projections`
- namespace: `ProjNet.IO.CoordinateSystems`
- asset: net6.0, net8.0, netstandard2.0; IL-only AnyCPU managed assembly, no native binaries
- asset: NetTopologySuite-ecosystem peer (MIT); thread-safe `CoordinateSystemServices` cache
- rail: geometry

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: coordinate-system services and factories
- package: `ProjNET`
- namespace: `ProjNet.CoordinateSystems`, `ProjNet.CoordinateSystems.Transformations`
- rail: geometry

| [INDEX] | [SYMBOL]                          | [RAIL]   | [CAPABILITY]                                                                                              |
| :-----: | :-------------------------------- | :------- | :-------------------------------------------------------------------------------------------------------- |
|   [1]   | `CoordinateSystemServices`        | geometry | SRID-keyed CS cache + transformation facade; `GetCoordinateSystem`, `CreateTransformation`                |
|   [2]   | `CoordinateSystemFactory`         | geometry | builds CS objects; `CreateFromWkt`, `CreateGeographicCoordinateSystem`, `CreateProjectedCoordinateSystem` |
|   [3]   | `CoordinateTransformationFactory` | geometry | builds transformations; `CreateFromCoordinateSystems(source, target)`                                     |
|   [4]   | `ICoordinateTransformation`       | geometry | a built transformation; `MathTransform`, `SourceCS`, `TargetCS`, `TransformType`                          |
|   [5]   | `MathTransform`                   | geometry | numeric transform pipeline; `Transform`, `Inverse`, `DimSource`, `DimTarget`                              |

[PUBLIC_TYPE_SCOPE]: coordinate-system models and units
- package: `ProjNET`
- namespace: `ProjNet.CoordinateSystems`
- rail: geometry

| [INDEX] | [SYMBOL]                     | [RAIL]   | [CAPABILITY]                                                                                                 |
| :-----: | :--------------------------- | :------- | :----------------------------------------------------------------------------------------------------------- |
|   [1]   | `CoordinateSystem`           | geometry | abstract CS base; `WKT`, `Authority`, `AuthorityCode`, `Dimension`                                           |
|   [2]   | `GeographicCoordinateSystem` | geometry | lat/lon CS; `AngularUnit`, `HorizontalDatum`, `PrimeMeridian`, static `WGS84`                                |
|   [3]   | `ProjectedCoordinateSystem`  | geometry | planar CS; `GeographicCoordinateSystem`, `LinearUnit`, `Projection`, `WGS84_UTM(zone, north)`, `WebMercator` |
|   [4]   | `HorizontalDatum`            | geometry | geodetic datum; `Ellipsoid`, `Wgs84Parameters` (Bursa-Wolf 7-param shift)                                    |
|   [5]   | `Ellipsoid`                  | geometry | reference ellipsoid; `SemiMajorAxis`, `InverseFlattening`, static `WGS84`/`GRS80`                            |
|   [6]   | `Projection`                 | geometry | map-projection parameter set; named projection + `ProjectionParameter` list                                  |
|   [7]   | `LinearUnit` / `AngularUnit` | geometry | unit definitions; static `Metre`, `Degrees`, conversion factor                                               |

[PUBLIC_TYPE_SCOPE]: WKT/SRID I/O
- package: `ProjNET`
- namespace: `ProjNet.IO.CoordinateSystems`
- rail: geometry

| [INDEX] | [SYMBOL]                    | [RAIL]   | [CAPABILITY]                                                         |
| :-----: | :-------------------------- | :------- | :------------------------------------------------------------------- |
|   [1]   | `CoordinateSystemWktReader` | geometry | parses an OGC WKT string into a `CoordinateSystem` (`Parse(string)`) |
|   [2]   | `SridReader`                | geometry | SRID-to-WKT lookup from the bundled `SRID.csv` EPSG table            |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: build a transformation by EPSG/SRID
- package: `ProjNET`
- namespace: `ProjNet.CoordinateSystems`, `ProjNet.CoordinateSystems.Transformations`
- rail: geometry

| [INDEX] | [SURFACE]                                                     | [CALL_SHAPE]                                                                       | [CAPABILITY]                                   |
| :-----: | :------------------------------------------------------------ | :--------------------------------------------------------------------------------- | :--------------------------------------------- |
|   [1]   | `CoordinateSystemServices.GetCoordinateSystem`                | `(int srid)` → `CoordinateSystem`                                                  | resolves a CS from an EPSG/SRID code           |
|   [2]   | `CoordinateSystemServices.GetCoordinateSystem`                | `(string authority, long code)` → `CoordinateSystem`                               | resolves a CS by authority + code              |
|   [3]   | `CoordinateSystemServices.CreateTransformation`               | `(int sourceSrid, int targetSrid)` → `ICoordinateTransformation`                   | builds a transformation between two SRIDs      |
|   [4]   | `CoordinateSystemFactory.CreateFromWkt`                       | `(string wkt)` → `CoordinateSystem`                                                | parses a WKT CS definition                     |
|   [5]   | `CoordinateTransformationFactory.CreateFromCoordinateSystems` | `(CoordinateSystem source, CoordinateSystem target)` → `ICoordinateTransformation` | builds a transformation between two CS objects |

[ENTRYPOINT_SCOPE]: transform coordinates
- package: `ProjNET`
- namespace: `ProjNet.CoordinateSystems.Transformations`
- rail: geometry

| [INDEX] | [SURFACE]                                 | [CALL_SHAPE]                                    | [CAPABILITY]                                    |
| :-----: | :---------------------------------------- | :---------------------------------------------- | :---------------------------------------------- |
|   [1]   | `MathTransform.Transform`                 | `(double x, double y)` → `(double x, double y)` | transforms one 2D coordinate                    |
|   [2]   | `MathTransform.Transform`                 | `(ref double x, ref double y, ref double z)`    | transforms one 3D coordinate in place           |
|   [3]   | `MathTransform.Transform`                 | `(double[] point)` → `double[]`                 | transforms a single ordinate array              |
|   [4]   | `MathTransform.TransformList`             | `(IList<double[]>)` → `IList<double[]>`         | transforms a batch of coordinates               |
|   [5]   | `MathTransform.Inverse`                   | `()` → `MathTransform`                          | the reverse transform (target → source)         |
|   [6]   | `MathTransform.DimSource`/`DimTarget`     | property                                        | source/target ordinate dimensionality           |
|   [7]   | `ICoordinateTransformation.MathTransform` | property                                        | the numeric transform of a built transformation |

## [4]-[IMPLEMENTATION_LAW]

[PLATFORM_BOUNDARY]:
- ships a single managed `ProjNet.dll`; no P/Invoke, no native PROJ dependency, no `runtimes/` assets
- transformation is pure double-precision numeric; no geometry kernel, no topology
- the bundled `SRID.csv` EPSG table is the default SRID source; a custom CS source overrides it through the `CoordinateSystemServices` constructor

[CRS_TRANSFORM]:
- transformation root: `CoordinateSystemServices.CreateTransformation(sourceSrid, targetSrid)` yields an `ICoordinateTransformation` whose `.MathTransform` transforms ordinates
- datum bridge: a `HorizontalDatum.Wgs84Parameters` Bursa-Wolf 7-parameter set drives the datum-to-datum shift the `CoordinateTransformationFactory` concatenates between two `ProjectedCoordinateSystem` instances with distinct datums
- projection axis: each `ProjectedCoordinateSystem` carries its `Projection` (Transverse Mercator, Lambert, etc.) and `ProjectionParameter` list resolved from the EPSG/WKT definition
- inverse: `MathTransform.Inverse()` returns the reverse pipeline so a round-trip (forward then inverse) recovers the source ordinate within numeric tolerance

[LOCAL_ADMISSION]:
- CRS resolution enters through `CoordinateSystemServices.GetCoordinateSystem(srid)` or `CoordinateSystemFactory.CreateFromWkt`.
- transformation construction enters through `CoordinateSystemServices.CreateTransformation` or `CoordinateTransformationFactory.CreateFromCoordinateSystems`.
- coordinate transformation enters through `MathTransform.Transform`; batch through `TransformList`.
- the `CoordinateSystemServices` cache is the single CS/transformation owner — a per-call factory rebuild is the rejected form.

[RAIL_LAW]:
- Package: `ProjNET`
- Owns: geodetic datum/projection coordinate transformation between EPSG-keyed coordinate systems
- Accept: CRS resolution, datum-to-datum reprojection, projected/geographic transformation
- Reject: geometry kernel, topology, the rigid IFC map-conversion offset (the kernel `Rasm` transform owns that), native PROJ binding
