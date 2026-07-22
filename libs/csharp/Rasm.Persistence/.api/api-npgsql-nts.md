# [RASM_PERSISTENCE_API_NPGSQL_NTS]

`Npgsql.NetTopologySuite` admits the PostGIS `geometry`/`geography` wire codecs onto an `INpgsqlTypeMapper`, so an Npgsql command reads and writes `NetTopologySuite.Geometries.Geometry` values as native ADO parameters and result fields. Admission registers a `NetTopologySuiteTypeInfoResolverFactory` carrying the coordinate-sequence, precision, ordinate, and geography-default policy; the codec owns the binary round-trip, the geometry model and the EF column mapping stay with their own owners.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Npgsql.NetTopologySuite`
- package: `Npgsql.NetTopologySuite` (`PostgreSQL`)
- assembly: `Npgsql.NetTopologySuite`
- namespace: `Npgsql`
- depends: `Npgsql` ADO provider, `NetTopologySuite` geometry model
- rail: spatial store codec

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: PostGIS wire-codec admission

| [INDEX] | [SYMBOL]                           | [TYPE_FAMILY] | [CAPABILITY]                                      |
| :-----: | :--------------------------------- | :------------ | :------------------------------------------------ |
|  [01]   | `NpgsqlNetTopologySuiteExtensions` | class         | admits geometry/geography codecs on a type mapper |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: type-mapper codec admission

Both overloads carry the shared optional tuple `CoordinateSequenceFactory?`, `PrecisionModel?`, `Ordinates handleOrdinates = None`, `bool geographyAsDefault = false`; each registers a `NetTopologySuiteTypeInfoResolverFactory` and returns its receiver.

| [INDEX] | [SURFACE]                                                        | [SHAPE] | [CAPABILITY]                           |
| :-----: | :--------------------------------------------------------------- | :------ | :------------------------------------- |
|  [01]   | `UseNetTopologySuite(INpgsqlTypeMapper, …) -> INpgsqlTypeMapper` | static  | admits codecs on a mapper              |
|  [02]   | `UseNetTopologySuite<TMapper>(TMapper, …) -> TMapper`            | static  | admits codecs, preserves receiver type |

- `UseNetTopologySuite<TMapper>`: `TMapper : INpgsqlTypeMapper`, so `NpgsqlDataSourceBuilder` binds this generic and chains at provisioning.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Admission is provisioning-time: registering the resolver factory on the mapper is the only act, and thereafter every PostGIS `geometry`/`geography` value round-trips as `Geometry` with no query-path call.

[STACKING]:
- `api-nts-ef`: `NpgsqlGeometryTypeMapping<TGeometry>` maps the columns whose binary bytes this codec round-trips; EF `UseNetTopologySuite` and this ADO admission pair, neither standing alone.
- `api-nettopologysuite`: this codec reads and writes the `Geometry` object model, and `GeometryFactory`, `PrecisionModel`, and `Ordinates` parameterize the registered resolver factory.
- Store profile: `NpgsqlDataSourceBuilder` admits the codec at provisioning; the geometry values it wires are the `Element/identity` footprint/boundary/envelope columns — the codec owns the wire round-trip, `Element/identity` owns the column.

[LOCAL_ADMISSION]:
- PostgreSQL store profile admits `UseNetTopologySuite` on the data source before any spatial command opens a connection; `geographyAsDefault`, precision, coordinate-sequence, and ordinate handling are profile policy, never call-site literals.

[RAIL_LAW]:
- Package: `Npgsql.NetTopologySuite`
- Owns: PostGIS `geometry`/`geography` ADO wire-codec admission for `Npgsql`
- Accept: `UseNetTopologySuite` on the store data source or type mapper
- Reject: WKT strings, raw WKB blobs, or EF-only plugin admission without the ADO codec
