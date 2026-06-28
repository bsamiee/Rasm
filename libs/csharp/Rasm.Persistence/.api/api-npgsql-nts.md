# [RASM_PERSISTENCE_API_NPGSQL_NTS]

`Npgsql.NetTopologySuite` is the ADO wire-codec package that lets `NpgsqlDataSourceBuilder` and `INpgsqlTypeMapper` read and write PostGIS `geometry` and `geography` values as `NetTopologySuite.Geometries.Geometry` values. The EF plugin catalogue remains `api-nts-ef.md`; this file records the direct ADO codec package now referenced by `Rasm.Persistence`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Npgsql.NetTopologySuite`
- package: `Npgsql.NetTopologySuite`
- assembly: `Npgsql.NetTopologySuite`
- namespace: `Npgsql`
- ADO provider: `Npgsql`
- geometry model: `NetTopologySuite`
- rail: spatial store codec

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: wire-codec admission
- rail: spatial store codec

| [INDEX] | [SYMBOL]                           | [TYPE_FAMILY]        | [CAPABILITY]                                  |
| :-----: | :--------------------------------- | :------------------- | :-------------------------------------------- |
|  [01]   | `NpgsqlNetTopologySuiteExtensions` | type-mapper extension | registers PostGIS geometry/geography codecs  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: Npgsql type mapper admission
- rail: spatial store codec

Both overload families accept `CoordinateSequenceFactory?`, `PrecisionModel?`, `Ordinates handleOrdinates = Ordinates.None`, and `bool geographyAsDefault = false`.

| [INDEX] | [SURFACE]                                                                 | [CAPABILITY]                                      |
| :-----: | :------------------------------------------------------------------------ | :----------------------------------------------- |
|  [01]   | `UseNetTopologySuite(this INpgsqlTypeMapper mapper, ...)`                 | admits geometry codecs on a mapper               |
|  [02]   | `UseNetTopologySuite<TMapper>(this TMapper mapper, ...) where TMapper : INpgsqlTypeMapper` | admits geometry codecs while preserving mapper type |
|  [03]   | `NpgsqlDataSourceBuilder.UseNetTopologySuite(...)`                        | registers codecs on the store profile data source |

## [04]-[IMPLEMENTATION_LAW]

[LOCAL_ADMISSION]:
- The PostgreSQL spatial store profile admits `UseNetTopologySuite` on the Npgsql data source before any spatial query or command path opens a connection.
- `geographyAsDefault`, precision, coordinate-sequence, and ordinate handling are profile policy values, not call-site literals.
- The EF provider plugin and ADO data-source codec are declared together for store profiles using spatial columns.

[STACKING_LAW]:
- `Npgsql.NetTopologySuite` owns the binary wire codec.
- `Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite` owns EF column mapping, conventions, and SQL translators.
- `NetTopologySuite` owns the geometry object model and topology algebra.

[RAIL_LAW]:
- Package: `Npgsql.NetTopologySuite`
- Own: PostGIS ADO codec admission for `Npgsql`
- Accept: `UseNetTopologySuite` on the project data source/type mapper
- Reject: WKT strings, raw WKB blobs, or EF-only plugin admission without the data-source codec

