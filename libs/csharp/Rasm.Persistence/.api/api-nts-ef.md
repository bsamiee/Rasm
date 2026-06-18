# [RASM_PERSISTENCE_API_NTS_EF]

`Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite` plugs NetTopologySuite
geometry into the EF Core PostgreSQL provider, `Npgsql.NetTopologySuite` admits
the geometry wire codecs on the ADO data source, and transitive
`NetTopologySuite` supplies the geometry model itself.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite`
- package: `Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite`
- assembly: `Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite`
- namespace: `Microsoft.EntityFrameworkCore`
- provider package: `Npgsql.EntityFrameworkCore.PostgreSQL`
- asset: runtime library
- rail: store-provider

[PACKAGE_SURFACE]: `Npgsql.NetTopologySuite`
- package: `Npgsql.NetTopologySuite`
- assembly: `Npgsql.NetTopologySuite`
- namespace: `Npgsql`
- ADO package: `Npgsql`
- asset: runtime library
- rail: store-provider

[PACKAGE_SURFACE]: `NetTopologySuite`
- package: `NetTopologySuite`
- assembly: `NetTopologySuite`
- namespace: `NetTopologySuite.Geometries`
- asset: transitive runtime library
- rail: spatial-values

## [2]-[PUBLIC_TYPES]

[PLUGIN_TYPES]: EF plugin admission and translation services
- rail: store-provider

| [INDEX] | [SYMBOL]                                                    | [PACKAGE_ROLE]       | [CAPABILITY]                          |
| :-----: | :---------------------------------------------------------- | :------------------- | :------------------------------------ |
|   [1]   | `NpgsqlNetTopologySuiteDbContextOptionsBuilderExtensions`   | builder extension    | admits plugin                         |
|   [2]   | `NpgsqlNetTopologySuiteServiceCollectionExtensions`         | service extension    | admits plugin services                |
|   [3]   | `NpgsqlNetTopologySuiteDbFunctionsExtensions`               | function surface     | projects PostGIS functions            |
|   [4]   | `NpgsqlNetTopologySuiteOptionsExtension`                    | options extension    | carries plugin policy                 |
|   [5]   | `NetTopologySuiteDataSourceConfigurationPlugin`             | data source plugin   | enables geometry wire on data source  |
|   [6]   | `NpgsqlNetTopologySuiteTypeMappingSourcePlugin`             | mapping plugin       | resolves geometry column mappings     |
|   [7]   | `NpgsqlGeometryTypeMapping<TGeometry>`                      | geometry mapping     | maps geometry and geography columns   |
|   [8]   | `NpgsqlJsonGeometryWktReaderWriter`                         | JSON value writer    | reads and writes geometry as WKT JSON |
|   [9]   | `NpgsqlNetTopologySuiteMethodCallTranslatorPlugin`          | method plugin        | translates geometry methods           |
|  [10]   | `NpgsqlNetTopologySuiteMemberTranslatorPlugin`              | member plugin        | translates geometry members           |
|  [11]   | `NpgsqlNetTopologySuiteAggregateMethodCallTranslatorPlugin` | aggregate plugin     | translates spatial aggregates         |
|  [12]   | `NpgsqlNetTopologySuiteAggregateMethodTranslator`           | aggregate translator | implements spatial aggregate SQL      |
|  [13]   | `NpgsqlGeometryMemberTranslator`                            | member translator    | implements geometry member SQL        |
|  [14]   | `NpgsqlGeometryMethodTranslator`                            | method translator    | implements geometry method SQL        |

[PLUGIN_TYPES]: EF plugin conventions and scaffolding
- rail: store-provider

| [INDEX] | [SYMBOL]                                          | [PACKAGE_ROLE]     | [CAPABILITY]                          |
| :-----: | :------------------------------------------------ | :----------------- | :------------------------------------ |
|   [1]   | `NpgsqlNetTopologySuiteConventionSetPlugin`       | convention plugin  | adds PostGIS extension convention     |
|   [2]   | `NpgsqlNetTopologySuiteExtensionAddingConvention` | convention         | finalizes PostGIS extension on model  |
|   [3]   | `NpgsqlNetTopologySuiteSingletonOptions`          | singleton options  | carries resolved geometry options     |
|   [4]   | `INpgsqlNetTopologySuiteSingletonOptions`         | singleton contract | singleton options contract            |
|   [5]   | `NpgsqlNetTopologySuiteCodeGeneratorPlugin`       | scaffolding plugin | emits plugin admission in scaffolding |
|   [6]   | `NpgsqlNetTopologySuiteDesignTimeServices`        | design services    | admits design tooling                 |

[WIRE_TYPES]: ADO wire admission
- rail: store-provider

| [INDEX] | [SYMBOL]                           | [PACKAGE_ROLE]        | [CAPABILITY]                          |
| :-----: | :--------------------------------- | :-------------------- | :------------------------------------ |
|   [1]   | `NpgsqlNetTopologySuiteExtensions` | type-mapper extension | admits geometry wire codecs on mapper |

[GEOMETRY_TYPES]: NetTopologySuite geometry model
- rail: spatial-values

| [INDEX] | [SYMBOL]             | [PACKAGE_ROLE]   | [CAPABILITY]                        |
| :-----: | :------------------- | :--------------- | :---------------------------------- |
|   [1]   | `Geometry`           | geometry root    | owns spatial algebra and predicates |
|   [2]   | `Point`              | point geometry   | carries X, Y, Z coordinates         |
|   [3]   | `LineString`         | curve geometry   | carries ordered coordinates         |
|   [4]   | `LinearRing`         | ring geometry    | closes a coordinate sequence        |
|   [5]   | `Polygon`            | surface geometry | carries shell and holes             |
|   [6]   | `MultiPoint`         | collection       | aggregates points                   |
|   [7]   | `MultiLineString`    | collection       | aggregates line strings             |
|   [8]   | `MultiPolygon`       | collection       | aggregates polygons                 |
|   [9]   | `GeometryCollection` | collection       | aggregates mixed geometries         |
|  [10]   | `GeometryFactory`    | factory          | creates geometries with SRID policy |
|  [11]   | `Coordinate`         | coordinate value | carries ordinate values             |
|  [12]   | `Envelope`           | bounding box     | bounds geometry extents             |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: plugin admission
- rail: store-provider

`UseNetTopologySuite` on `NpgsqlDbContextOptionsBuilder` accepts optional `CoordinateSequenceFactory`, `PrecisionModel`, `Ordinates handleOrdinates`, and `bool geographyAsDefault`.
`UseNetTopologySuite` on `INpgsqlTypeMapper` (generic `TMapper` overload) accepts the same parameters and is the ADO data-source admission seam.

| [INDEX] | [SURFACE]                                  | [CALL_SHAPE]          | [CAPABILITY]                                     |
| :-----: | :----------------------------------------- | :-------------------- | :----------------------------------------------- |
|   [1]   | `UseNetTopologySuite`                      | provider option       | maps geometry values in EF with precision policy |
|   [2]   | `AddEntityFrameworkNpgsqlNetTopologySuite` | service extension     | registers plugin services                        |
|   [3]   | `UseNetTopologySuite`                      | type-mapper extension | admits wire codecs on `INpgsqlTypeMapper`        |

[ENTRYPOINT_SCOPE]: SQL function projections
- rail: store-provider

| [INDEX] | [SURFACE]          | [CALL_SHAPE]            | [CAPABILITY]                |
| :-----: | :----------------- | :---------------------- | :-------------------------- |
|   [1]   | `Distance`         | `DbFunctions` extension | measures spheroid distance  |
|   [2]   | `IsWithinDistance` | `DbFunctions` extension | tests spheroid proximity    |
|   [3]   | `DistanceKnn`      | `DbFunctions` extension | orders by KNN distance      |
|   [4]   | `Transform`        | `DbFunctions` extension | reprojects to a target SRID |
|   [5]   | `Force2D`          | `DbFunctions` extension | drops Z and M ordinates     |

[ENTRYPOINT_SCOPE]: geometry construction and algebra
- rail: spatial-values

| [INDEX] | [SURFACE]                                   | [CALL_SHAPE]      | [CAPABILITY]            |
| :-----: | :------------------------------------------ | :---------------- | :---------------------- |
|   [1]   | `GeometryFactory.CreatePoint`               | factory call      | creates points          |
|   [2]   | `GeometryFactory.CreateLineString`          | factory call      | creates line strings    |
|   [3]   | `GeometryFactory.CreatePolygon`             | factory call      | creates polygons        |
|   [4]   | `Geometry.Intersects` / `Geometry.Contains` | spatial predicate | tests spatial relations |
|   [5]   | `Geometry.Distance` / `Geometry.Buffer`     | spatial operation | measures and dilates    |
|   [6]   | `Geometry.AsText` / `Geometry.AsBinary`     | projection call   | emits WKT and WKB       |

## [4]-[IMPLEMENTATION_LAW]

[STORE_PROFILE]:
- profile: the NetTopologySuite stack is spatial mapping policy for the PostgreSQL store profile
- admission root: `UseNetTopologySuite` on the PostgreSQL provider options and the data source builder
- mapping root: geometry and geography column mappings with per-property SRID
- query root: geometry method, member, and aggregate translation to PostGIS SQL

[LOCAL_ADMISSION]:
- Spatial mapping enters only through the PostgreSQL store-profile declaration.
- Persisted spatial semantics use NetTopologySuite types per the spatial-values rail.
- Wire admission on the data source builder pairs with the EF plugin; neither stands alone.
- SQL function projections are query facts and stay inside profile queries.

[RAIL_LAW]:
- Packages: `Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite`, `Npgsql.NetTopologySuite`, `NetTopologySuite`
- Own: geometry mapping for the PostgreSQL EF provider and ADO wire
- Accept: profile-declared NetTopologySuite mapping
- Reject: WKT strings or raw byte columns standing in for geometry contracts
