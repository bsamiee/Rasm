# [RASM_PERSISTENCE_API_NTS_EF]

`Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite` maps `NetTopologySuite` geometry onto PostGIS `geometry`/`geography` columns for the EF Core PostgreSQL provider: per-property-SRID type-mapping, member/method/aggregate translators emitting PostGIS SQL, the `EF.Functions` spatial surface, and the `CREATE EXTENSION postgis` finalizing convention. It composes the substrate geometry model and the `Npgsql.NetTopologySuite` ADO wire codec, feeding the PostgreSQL store profile's spatial mapping.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite`
- package: `Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite` (`PostgreSQL`, Npgsql)
- assembly: `Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite`
- namespace: `Microsoft.EntityFrameworkCore` (builder), `Npgsql.EntityFrameworkCore.PostgreSQL.*` (plugins)
- depends: `Npgsql.EntityFrameworkCore.PostgreSQL` provider, `NetTopologySuite` model, `Npgsql.NetTopologySuite` wire codec
- asset: runtime library, net10.0
- rail: store-provider spatial mapping

## [02]-[PUBLIC_TYPES]

[PLUGIN_TYPES]: EF plugin admission and translation services

| [INDEX] | [SYMBOL]                                                    | [TYPE_FAMILY]        | [CAPABILITY]                                        |
| :-----: | :---------------------------------------------------------- | :------------------- | :-------------------------------------------------- |
|  [01]   | `NpgsqlNetTopologySuiteDbContextOptionsBuilderExtensions`   | builder extension    | admits plugin via provider-options seam             |
|  [02]   | `NpgsqlNetTopologySuiteServiceCollectionExtensions`         | service extension    | admits plugin services                              |
|  [03]   | `NpgsqlNetTopologySuiteDbFunctionsExtensions`               | function surface     | projects PostGIS functions on `EF.Functions`        |
|  [04]   | `NpgsqlNetTopologySuiteOptionsExtension`                    | options extension    | `IDbContextOptionsExtension`; carries plugin policy |
|  [05]   | `NetTopologySuiteDataSourceConfigurationPlugin`             | data source plugin   | `INpgsqlDataSourceConfigurationPlugin`              |
|  [06]   | `NpgsqlNetTopologySuiteTypeMappingSourcePlugin`             | mapping plugin       | `IRelationalTypeMappingSourcePlugin`                |
|  [07]   | `NpgsqlGeometryTypeMapping<TGeometry>`                      | geometry mapping     | `RelationalGeometryTypeMapping`; geometry+geography |
|  [08]   | `NpgsqlJsonGeometryWktReaderWriter`                         | JSON value writer    | `JsonValueReaderWriter<Geometry>`; WKT in `jsonb`   |
|  [09]   | `NpgsqlNetTopologySuiteMethodCallTranslatorPlugin`          | method plugin        | `IMethodCallTranslatorPlugin`                       |
|  [10]   | `NpgsqlNetTopologySuiteMemberTranslatorPlugin`              | member plugin        | `IMemberTranslatorPlugin`                           |
|  [11]   | `NpgsqlNetTopologySuiteAggregateMethodCallTranslatorPlugin` | aggregate plugin     | `IAggregateMethodCallTranslatorPlugin`              |
|  [12]   | `NpgsqlNetTopologySuiteAggregateMethodTranslator`           | aggregate translator | emits `ST_Union`/`ST_Extent` SQL                    |
|  [13]   | `NpgsqlGeometryMemberTranslator`                            | member translator    | emits `.Area`/`.Length`/`.Centroid` SQL             |
|  [14]   | `NpgsqlGeometryMethodTranslator`                            | method translator    | emits `.Distance`/`.Intersects`/`.Buffer` SQL       |

[PLUGIN_TYPES]: EF plugin conventions and scaffolding

| [INDEX] | [SYMBOL]                                          | [TYPE_FAMILY]      | [CAPABILITY]                                                  |
| :-----: | :------------------------------------------------ | :----------------- | :------------------------------------------------------------ |
|  [01]   | `NpgsqlNetTopologySuiteConventionSetPlugin`       | convention plugin  | `IConventionSetPlugin`; adds PostGIS convention               |
|  [02]   | `NpgsqlNetTopologySuiteExtensionAddingConvention` | convention         | `IModelFinalizingConvention`; adds `CREATE EXTENSION postgis` |
|  [03]   | `NpgsqlNetTopologySuiteSingletonOptions`          | singleton options  | carries resolved geometry options                             |
|  [04]   | `INpgsqlNetTopologySuiteSingletonOptions`         | singleton contract | `ISingletonOptions` contract                                  |
|  [05]   | `NpgsqlNetTopologySuiteCodeGeneratorPlugin`       | scaffolding plugin | `ProviderCodeGeneratorPlugin`; emits admission                |
|  [06]   | `NpgsqlNetTopologySuiteDesignTimeServices`        | design services    | `IDesignTimeServices`; admits design tooling                  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: EF plugin admission

`UseNetTopologySuite` on the EF `NpgsqlDbContextOptionsBuilder` carries the four optional policy parameters (`CoordinateSequenceFactory?`, `PrecisionModel?`, `Ordinates handleOrdinates = None`, `bool geographyAsDefault = false`); the ADO data-source seam of the same name is `api-npgsql-nts.md`, and admission binds both.

| [INDEX] | [SURFACE]                                                           | [CAPABILITY]                                       |
| :-----: | :------------------------------------------------------------------ | :------------------------------------------------- |
|  [01]   | `UseNetTopologySuite(…)` on `NpgsqlDbContextOptionsBuilder`         | EF provider-option seam; precision/ordinate policy |
|  [02]   | `AddEntityFrameworkNpgsqlNetTopologySuite(this IServiceCollection)` | service-extension seam; registers plugin services  |

[ENTRYPOINT_SCOPE]: PostGIS SQL function projections (`EF.Functions` extensions)

`DbFunctions` extensions translate to PostGIS in a LINQ predicate or order-by and throw on client evaluation. `Distance`/`IsWithinDistance` carry `bool useSpheroid` selecting `ST_DistanceSpheroid` over `ST_Distance`; `DistanceKnn` emits the `<->` KNN operator for index-ordered nearest-neighbour; `Transform`/`Force2D` are generic over `TGeometry : Geometry`.

| [INDEX] | [SURFACE]                                                                     | [RETURNS]   | [POSTGIS]                             |
| :-----: | :---------------------------------------------------------------------------- | :---------- | :------------------------------------ |
|  [01]   | `EF.Functions.Distance(Geometry, Geometry, bool useSpheroid)`                 | `double`    | `ST_Distance` / `ST_DistanceSpheroid` |
|  [02]   | `EF.Functions.IsWithinDistance(Geometry, Geometry, double, bool useSpheroid)` | `bool`      | `ST_DWithin` (optionally spheroid)    |
|  [03]   | `EF.Functions.DistanceKnn(Geometry, Geometry)`                                | `double`    | `<->` KNN distance operator           |
|  [04]   | `EF.Functions.Transform<TGeometry>(TGeometry, int srid)`                      | `TGeometry` | `ST_Transform`                        |
|  [05]   | `EF.Functions.Force2D<TGeometry>(TGeometry)`                                  | `TGeometry` | `ST_Force2D`                          |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every spatial column maps through `NpgsqlGeometryTypeMapping<TGeometry>` carrying per-property SRID; `geographyAsDefault` selects `geography` over `geometry`.
- Member/method/aggregate translators emit PostGIS SQL for a LINQ predicate or order-by; an untranslated operation runs client-side on a materialized geometry, while `EF.Functions.*` are server-only and throw on client evaluation.
- `NpgsqlNetTopologySuiteExtensionAddingConvention` finalizes `CREATE EXTENSION postgis` on the model.

[STACKING]:
- `Npgsql.NetTopologySuite`(`.api/api-npgsql-nts.md`): the ADO wire seam — this plugin's `UseNetTopologySuite` registers the column mapping and translators while the data-source codec moves the bytes, and `NetTopologySuiteDataSourceConfigurationPlugin` auto-applies so the provider's pooled data source carries the binary codec.
- `NetTopologySuite`(`libs/csharp/.api/api-nettopologysuite.md`): the geometry model this plugin maps — `GeometryFactory`-constructed `Geometry` and the DE-9IM algebra the translators project to PostGIS SQL.
- `NetTopologySuite.IO.GeoJSON4STJ`(`.api/api-nts-io.md`): a `geometry` column persists as PostGIS binary, but the same `Geometry` serializes to GeoJSON at the web egress boundary through the GeoJSON4STJ converters on the shared `JsonSerializerOptions`.
- `linq2db`(`.api/api-linq2db-ef.md`): geometry columns survive `BulkCopy` with `BulkCopyType.ProviderSpecific` because the bridge reuses this EF model's geometry mapping and the Npgsql binary COPY writer emits the codec's wire form.
- identity spatial tier: the `Element/identity` geometry/geography column is GiST-indexed for `DistanceKnn` (`<->`) and `IsWithinDistance` (`ST_DWithin`) predicate pushdown — the index-ordered nearest-neighbour sibling of the pgvector ANN order-by.

[LOCAL_ADMISSION]:
- Spatial mapping enters through the PostgreSQL store-profile declaration; the EF plugin admission and the ADO data-source admission are declared together, neither standing alone.
- Persisted spatial semantics carry `NetTopologySuite` types; per-property SRID is mapping policy, never an inline literal.
- `NpgsqlJsonGeometryWktReaderWriter` serializes a geometry nested inside an owned-entity JSON document as WKT through `JsonValueReaderWriter<Geometry>`, distinct from a top-level PostGIS `geometry` column.

[RAIL_LAW]:
- Package: `Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite`
- Owns: geometry/geography column mapping, member/method/aggregate translation to PostGIS SQL, the `EF.Functions` spatial surface, and the `CREATE EXTENSION postgis` convention for the PostgreSQL EF provider
- Accept: profile-declared `UseNetTopologySuite` on the EF provider options paired with the ADO data-source codec; per-property SRID mapping; PostGIS-translated `EF.Functions.*` predicates
- Reject: WKT strings or raw `byte[]` columns standing in for geometry contracts; `new Point(…)` over `GeometryFactory` construction; client-evaluated spatial predicates; EF plugin admission without the paired ADO wire codec
