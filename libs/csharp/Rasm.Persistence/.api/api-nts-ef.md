# [RASM_PERSISTENCE_API_NTS_EF]

Three coupled packages form one spatial rail. `NetTopologySuite` (transitive `2.6.0`) is
the geometry model and topology engine — the `Geometry` algebra, `GeometryFactory`, and
`Coordinate`. `Npgsql.NetTopologySuite` admits the PostGIS geometry/geography wire codecs on
the ADO `NpgsqlDataSource`/`INpgsqlTypeMapper` so an `NpgsqlCommand` reads/writes NTS types
directly. `Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite` plugs that into EF Core:
geometry/geography column type-mappings with per-property SRID, member/method/aggregate
translators that emit PostGIS SQL, the `EF.Functions` PostGIS extension surface, the PostGIS
extension-adding convention, a JSON-column geometry reader/writer, and scaffolding/design
services. Wire admission and the EF plugin pair — neither stands alone.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite`
- package: `Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite`
- version: `10.0.2`
- license: `PostgreSQL`
- assembly: `Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite`
- namespace: `Microsoft.EntityFrameworkCore` (builder), `Npgsql.EntityFrameworkCore.PostgreSQL.*` (plugins)
- provider package: `Npgsql.EntityFrameworkCore.PostgreSQL`
- asset: runtime library
- target: net10.0 (single-target; binds cleanly)
- rail: store-provider

[PACKAGE_SURFACE]: `Npgsql.NetTopologySuite`
- package: `Npgsql.NetTopologySuite`
- version: `10.0.3`
- license: `PostgreSQL`
- assembly: `Npgsql.NetTopologySuite`
- namespace: `Npgsql`
- ADO package: `Npgsql`
- asset: runtime library
- target: net8.0 (consumer-bound fallback; the package's highest lib TFM is net8.0, which the
  net10.0 consumer binds — no net10.0 asset exists)
- rail: store-provider

[PACKAGE_SURFACE]: `NetTopologySuite`
- package: `NetTopologySuite`
- version: `2.6.0` (transitive via `Npgsql.NetTopologySuite`)
- license: `BSD-3-Clause`
- assembly: `NetTopologySuite`
- namespace: `NetTopologySuite.Geometries`
- asset: transitive runtime library
- target: netstandard2.1 (consumer-bound; multi-targets netstandard2.0/2.1 — the net10.0
  consumer binds netstandard2.1)
- rail: spatial-values

## [02]-[PUBLIC_TYPES]

[PLUGIN_TYPES]: EF plugin admission and translation services
- rail: store-provider

| [INDEX] | [SYMBOL]                                                    | [PACKAGE_ROLE]       | [CAPABILITY]                          |
| :-----: | :---------------------------------------------------------- | :------------------- | :------------------------------------ |
|  [01]   | `NpgsqlNetTopologySuiteDbContextOptionsBuilderExtensions`   | builder extension    | admits plugin (provider-options seam)  |
|  [02]   | `NpgsqlNetTopologySuiteServiceCollectionExtensions`         | service extension    | admits plugin services                |
|  [03]   | `NpgsqlNetTopologySuiteDbFunctionsExtensions`               | function surface     | projects PostGIS functions on `EF.Functions` |
|  [04]   | `NpgsqlNetTopologySuiteOptionsExtension`                    | options extension    | `IDbContextOptionsExtension`; carries plugin policy |
|  [05]   | `NetTopologySuiteDataSourceConfigurationPlugin`             | data source plugin   | `INpgsqlDataSourceConfigurationPlugin`; enables geometry wire |
|  [06]   | `NpgsqlNetTopologySuiteTypeMappingSourcePlugin`             | mapping plugin       | `IRelationalTypeMappingSourcePlugin`; resolves column mappings |
|  [07]   | `NpgsqlGeometryTypeMapping<TGeometry>`                      | geometry mapping     | `RelationalGeometryTypeMapping`; geometry + geography columns |
|  [08]   | `NpgsqlJsonGeometryWktReaderWriter`                         | JSON value writer    | `JsonValueReaderWriter<Geometry>`; geometry in a JSON column as WKT |
|  [09]   | `NpgsqlNetTopologySuiteMethodCallTranslatorPlugin`          | method plugin        | `IMethodCallTranslatorPlugin`         |
|  [10]   | `NpgsqlNetTopologySuiteMemberTranslatorPlugin`              | member plugin        | `IMemberTranslatorPlugin`             |
|  [11]   | `NpgsqlNetTopologySuiteAggregateMethodCallTranslatorPlugin` | aggregate plugin     | `IAggregateMethodCallTranslatorPlugin` |
|  [12]   | `NpgsqlNetTopologySuiteAggregateMethodTranslator`          | aggregate translator | spatial aggregate SQL (`ST_Union`, `ST_Extent`, …) |
|  [13]   | `NpgsqlGeometryMemberTranslator`                           | member translator    | geometry member SQL (`.Area`, `.Length`, `.Centroid`, …) |
|  [14]   | `NpgsqlGeometryMethodTranslator`                          | method translator    | geometry method SQL (`.Distance`, `.Intersects`, `.Buffer`, …) |

[PLUGIN_TYPES]: EF plugin conventions and scaffolding
- rail: store-provider

| [INDEX] | [SYMBOL]                                          | [PACKAGE_ROLE]     | [CAPABILITY]                          |
| :-----: | :------------------------------------------------ | :----------------- | :------------------------------------ |
|  [01]   | `NpgsqlNetTopologySuiteConventionSetPlugin`       | convention plugin  | `IConventionSetPlugin`; adds PostGIS convention |
|  [02]   | `NpgsqlNetTopologySuiteExtensionAddingConvention` | convention         | `IModelFinalizingConvention`; finalizes `CREATE EXTENSION postgis` |
|  [03]   | `NpgsqlNetTopologySuiteSingletonOptions`          | singleton options  | carries resolved geometry options     |
|  [04]   | `INpgsqlNetTopologySuiteSingletonOptions`         | singleton contract | `ISingletonOptions` contract          |
|  [05]   | `NpgsqlNetTopologySuiteCodeGeneratorPlugin`       | scaffolding plugin | `ProviderCodeGeneratorPlugin`; emits admission in scaffolding |
|  [06]   | `NpgsqlNetTopologySuiteDesignTimeServices`        | design services    | `IDesignTimeServices`; admits design tooling |

[WIRE_TYPES]: ADO wire admission (`Npgsql.NetTopologySuite` — single public type)
- rail: store-provider

| [INDEX] | [SYMBOL]                           | [PACKAGE_ROLE]        | [CAPABILITY]                          |
| :-----: | :--------------------------------- | :-------------------- | :------------------------------------ |
|  [01]   | `NpgsqlNetTopologySuiteExtensions` | type-mapper extension | admits geometry wire codecs on the data source / type mapper |

[GEOMETRY_TYPES]: NetTopologySuite geometry model + value supports
- rail: spatial-values

`Geometry` is the abstract root carrying the predicate/operation/projection algebra
([03] geometry algebra); the concrete subtypes are constructed through `GeometryFactory`,
never `new`. `Coordinate` carries `X`/`Y` plus optional `Z`/`M`; `Ordinates`
(`None`/`XY=3`/`XYZ=7`/`XYM`/`XYZM`/`AllOrdinates`) and `PrecisionModel`
(`Floating`/`FloatingSingle`/`Fixed`) parameterize the factory and the wire admission.

| [INDEX] | [SYMBOL]                 | [PACKAGE_ROLE]   | [CAPABILITY]                                |
| :-----: | :----------------------- | :--------------- | :------------------------------------------ |
|  [01]   | `Geometry`               | geometry root    | predicate/operation/projection algebra (abstract) |
|  [02]   | `Point`                  | point geometry   | `X`/`Y`/`Z`/`M` ordinates                   |
|  [03]   | `LineString`             | curve geometry   | ordered coordinate sequence                 |
|  [04]   | `LinearRing`             | ring geometry    | closed coordinate sequence                  |
|  [05]   | `Polygon`                | surface geometry | exterior shell + interior holes             |
|  [06]   | `MultiPoint` / `MultiLineString` / `MultiPolygon` | collections | homogeneous collections          |
|  [07]   | `GeometryCollection`     | collection       | heterogeneous geometry collection           |
|  [08]   | `GeometryFactory`        | factory          | constructs geometries with SRID/precision/ordinates |
|  [09]   | `Coordinate`             | coordinate value | `X`/`Y` + optional `Z`/`M`                  |
|  [10]   | `Envelope`               | bounding box     | `MinX`/`MaxX`/`MinY`/`MaxY`/`Width`/`Height`/`Area`/`Centre` |
|  [11]   | `Ordinates`              | ordinate flags   | `None`/`XY`/`XYZ`/`XYM`/`XYZM`/`AllOrdinates` |
|  [12]   | `PrecisionModel`         | precision policy | `Floating`/`FloatingSingle`/`Fixed`         |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: plugin + wire admission
- rail: store-provider

`UseNetTopologySuite` appears on three seams with the same four optional parameters
(`CoordinateSequenceFactory?`, `PrecisionModel?`, `Ordinates handleOrdinates = None`,
`bool geographyAsDefault = false`): the EF `NpgsqlDbContextOptionsBuilder`, the ADO
`INpgsqlTypeMapper`, and the generic `<TMapper> where TMapper : INpgsqlTypeMapper` data-source
seam. The EF builder and the ADO mapper admission pair — the EF plugin maps columns, the ADO
wire codec moves the bytes.

| [INDEX] | [SURFACE]                                                                 | [CALL_SHAPE]          | [CAPABILITY]                                     |
| :-----: | :------------------------------------------------------------------------ | :-------------------- | :----------------------------------------------- |
|  [01]   | `UseNetTopologySuite(coordSeqFactory?, precisionModel?, handleOrdinates=None, geographyAsDefault=false)` on `NpgsqlDbContextOptionsBuilder` | provider option | EF geometry mapping with precision/ordinate policy |
|  [02]   | `AddEntityFrameworkNpgsqlNetTopologySuite(this IServiceCollection)`        | service extension     | registers plugin services                        |
|  [03]   | `UseNetTopologySuite(this INpgsqlTypeMapper, …same 4 params…)`             | type-mapper extension | admits wire codecs on `INpgsqlTypeMapper`        |
|  [04]   | `UseNetTopologySuite<TMapper>(this TMapper, …same 4 params…) where TMapper : INpgsqlTypeMapper` | data-source seam | admits wire codecs on the data-source mapper |

[ENTRYPOINT_SCOPE]: PostGIS SQL function projections (`EF.Functions` extensions)
- rail: store-provider

`DbFunctions` extensions translated to PostGIS in a LINQ predicate/order — never client-eval.
`Distance`/`IsWithinDistance` carry a `bool useSpheroid` selecting `ST_DistanceSpheroid` vs.
`ST_Distance`; `DistanceKnn` emits the `<->` KNN operator for index-ordered nearest-neighbour;
`Transform`/`Force2D` are generic over `TGeometry : Geometry`.

| [INDEX] | [SURFACE]                                                              | [RETURNS]    | [POSTGIS]                       |
| :-----: | :--------------------------------------------------------------------- | :----------- | :------------------------------ |
|  [01]   | `EF.Functions.Distance(Geometry, Geometry, bool useSpheroid)`          | `double`     | `ST_Distance` / `ST_DistanceSpheroid` |
|  [02]   | `EF.Functions.IsWithinDistance(Geometry, Geometry, double, bool useSpheroid)` | `bool` | `ST_DWithin` (optionally spheroid) |
|  [03]   | `EF.Functions.DistanceKnn(Geometry, Geometry)`                        | `double`     | `<->` KNN distance operator     |
|  [04]   | `EF.Functions.Transform<TGeometry>(TGeometry, int srid)`              | `TGeometry`  | `ST_Transform`                  |
|  [05]   | `EF.Functions.Force2D<TGeometry>(TGeometry)`                          | `TGeometry`  | `ST_Force2D`                    |

[ENTRYPOINT_SCOPE]: geometry construction (`GeometryFactory`)
- rail: spatial-values

A `GeometryFactory(precisionModel, srid, coordinateSequenceFactory, services)` (or the
`PrecisionModel`-only ctor / `GeometryFactory.Default`/`Fixed`/`FloatingSingle` statics)
stamps SRID + precision onto every geometry it creates. Construct through the factory; the
empty-arg `Create*()` forms build empties.

| [INDEX] | [SURFACE]                                                       | [RETURNS]            | [CAPABILITY]                  |
| :-----: | :------------------------------------------------------------- | :------------------- | :---------------------------- |
|  [01]   | `CreatePoint(Coordinate)` / `CreatePoint()`                    | `Point`              | point (or empty)              |
|  [02]   | `CreateLineString(Coordinate[])` / `CreateLinearRing(Coordinate[])` | `LineString`/`LinearRing` | curve / ring             |
|  [03]   | `CreatePolygon(LinearRing shell, LinearRing[] holes)`          | `Polygon`            | shell + holes                 |
|  [04]   | `CreateMultiPointFromCoords` / `CreateMultiPoint(CoordinateSequence)` | `MultiPoint`   | point collection              |
|  [05]   | `CreateGeometryCollection(Geometry[])` / `CreateEmpty(Dimension)` | `GeometryCollection`/`Geometry` | mixed / typed empty |

[ENTRYPOINT_SCOPE]: geometry algebra (`Geometry`) — predicates, operations, projections
- rail: spatial-values

The model-side algebra (DE-9IM topology engine); the EF translators map a subset of these
to PostGIS SQL, the rest run client-side on a materialized geometry. The catalog documents
the load-bearing surface, not a 3-method sketch.

| [INDEX] | [SURFACE]                                                                  | [RETURNS]            | [CAPABILITY]                       |
| :-----: | :------------------------------------------------------------------------- | :------------------- | :--------------------------------- |
|  [01]   | `Intersects` / `Contains` / `Within` / `Covers` / `CoveredBy`             | `bool`               | DE-9IM topological predicates      |
|  [02]   | `Overlaps` / `Touches` / `Crosses` / `Disjoint` / `EqualsTopologically`   | `bool`               | DE-9IM topological predicates      |
|  [03]   | `Relate(Geometry)` / `Relate(Geometry, string pattern)`                   | `IntersectionMatrix`/`bool` | full DE-9IM matrix / pattern test |
|  [04]   | `Distance(Geometry)` / `IsWithinDistance(Geometry, double)`               | `double`/`bool`      | cartesian distance / proximity     |
|  [05]   | `Buffer(double[, quadrantSegments / EndCapStyle / BufferParameters])`     | `Geometry`           | dilation with cap/segment policy   |
|  [06]   | `ConvexHull()` / `Centroid` / `InteriorPoint` / `PointOnSurface`          | `Geometry`/`Point`   | derived geometries                 |
|  [07]   | `Area` / `Length` / `IsValid` / `IsSimple` / `IsRectangle` / `IsEmpty`    | `double`/`bool`      | metrics + validity                 |
|  [08]   | `Envelope` / `EnvelopeInternal`                                           | `Geometry`/`Envelope`| bounding geometry / box value      |
|  [09]   | `SRID` (get/set) / `NumGeometries` / `GetGeometryN(int)` / `Factory`      | `int`/`Geometry`     | identity + collection access       |
|  [10]   | `AsText()` / `ToText()` / `AsBinary()` / `ToBinary()`                     | `string`/`byte[]`    | WKT / WKB projection               |

## [04]-[IMPLEMENTATION_LAW]

[STORE_PROFILE]:
- profile: the NetTopologySuite stack is the spatial mapping policy for the PostgreSQL store
  profile — one model (`NetTopologySuite`), one wire codec (`Npgsql.NetTopologySuite`), one
  EF plugin (`Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite`)
- admission root: `UseNetTopologySuite` on the EF provider options AND the data-source/type
  mapper — both, paired
- mapping root: `NpgsqlGeometryTypeMapping<TGeometry>` with per-property SRID;
  `geographyAsDefault` selects `geography` over `geometry` columns
- query root: member/method/aggregate translators -> PostGIS SQL; `EF.Functions.*` for
  spheroid/KNN/transform functions
- convention root: `NpgsqlNetTopologySuiteExtensionAddingConvention` finalizes
  `CREATE EXTENSION postgis` on the model

[LOCAL_ADMISSION]:
- Spatial mapping enters only through the PostgreSQL store-profile declaration; the EF plugin
  admission and the ADO data-source admission are declared together — neither stands alone.
- Persisted spatial semantics use `NetTopologySuite` types per the spatial-values rail;
  per-property SRID is mapping policy, never an inline literal.
- `NpgsqlJsonGeometryWktReaderWriter` is the geometry-in-a-`jsonb`-column path: a geometry
  nested inside an owned-entity JSON document serializes as WKT through this
  `JsonValueReaderWriter<Geometry>`, distinct from a top-level PostGIS geometry column.
- SQL function projections (`EF.Functions.*`) are query facts and stay inside profile queries
  — they are PostGIS-translated, never client-evaluated.

[STACKING_LAW]:
- ADO wire + EF plugin: `Npgsql.NetTopologySuite.UseNetTopologySuite` on the
  `NpgsqlDataSourceBuilder` registers the binary geometry codec; the EF
  `UseNetTopologySuite` registers the column mapping + translators. The data-source plugin
  (`NetTopologySuiteDataSourceConfigurationPlugin`) is auto-applied so the EF provider's
  pooled data source carries the codec.
- STJ geometry (GeoJSON): the spatial-values rail meets the wire rail at
  `NetTopologySuite.IO.GeoJSON4STJ` (`api-nts-io`) — a geometry column persists as PostGIS
  binary, but the same `Geometry` serializes to GeoJSON for the web/egress boundary through
  the GeoJSON4STJ STJ converters, reusing the configured `JsonSerializerOptions` that also
  carries NodaTime and Thinktecture converters.
- spatial index at the identity tier: the geometry/geography column the `Element/identity`
  spatial tier carries beside its `Cell`/`Embedding` locators is GiST-indexed for `DistanceKnn`
  (`<->`) and `IsWithinDistance` (`ST_DWithin`) predicate pushdown; the KNN order-by is the
  `Query/retrieval#FUSION_AND_REUSE` `RetrievalBranch` spatial sibling of the pgvector ANN order-by,
  both index-ordered nearest-neighbour seams the fusion fold ranks.
- linq2db bulk: geometry columns survive `BulkCopy` with `BulkCopyType.ProviderSpecific`
  (`api-linq2db-ef`) because the bridge reuses the EF model's geometry mapping; the Npgsql
  binary COPY writer emits the geometry codec's wire form.

[RAIL_LAW]:
- Packages: `Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite`, `Npgsql.NetTopologySuite`,
  `NetTopologySuite`
- Own: geometry mapping/translation for the PostgreSQL EF provider, geometry wire codecs on
  the ADO data source, and the `NetTopologySuite` geometry model + algebra
- Accept: profile-declared `UseNetTopologySuite` admission (EF options + data source paired);
  per-property SRID mapping; PostGIS-translated `EF.Functions.*` predicates
- Reject: WKT strings or raw `byte[]` columns standing in for geometry contracts;
  `new Point(…)` over `GeometryFactory` construction; client-evaluated spatial predicates;
  EF plugin admission without the paired ADO wire admission
