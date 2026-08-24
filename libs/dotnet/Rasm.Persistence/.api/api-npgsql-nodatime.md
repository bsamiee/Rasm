# [RASM_PERSISTENCE_API_NPGSQL_NODATIME]

`Npgsql.NodaTime` admits the NodaTime temporal wire codecs onto an `INpgsqlTypeMapper`, so an Npgsql command reads and writes NodaTime values as native ADO parameters and result fields instead of degrading to BCL date types. Admission registers a `NodaTimeTypeInfoResolverFactory` covering the scalar, range, multirange, and array shapes of every mapped temporal store type; the codec owns the binary round-trip, the temporal model and the EF column mapping stay with their own owners.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Npgsql.NodaTime`
- package: `Npgsql.NodaTime` (`PostgreSQL`)
- assembly: `Npgsql.NodaTime`
- namespace: `Npgsql` public extensions; converters and resolvers under `Npgsql.NodaTime.Internal`
- depends: `Npgsql` ADO provider, `NodaTime` temporal model
- rail: temporal store codec

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: temporal wire-codec admission

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY] | [CAPABILITY]                            |
| :-----: | :------------------------- | :------------ | :-------------------------------------- |
|  [01]   | `NpgsqlNodaTimeExtensions` | class         | admits temporal codecs on a type mapper |

- Every converter and resolver ships `internal`, so the whole consumer surface is the one extension class.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: type-mapper codec admission

Both overloads take the receiver alone and carry no policy: each registers a `NodaTimeTypeInfoResolverFactory` and returns its receiver.

| [INDEX] | [SURFACE]                                             | [SHAPE] | [CAPABILITY]                           |
| :-----: | :---------------------------------------------------- | :------ | :------------------------------------- |
|  [01]   | `UseNodaTime(INpgsqlTypeMapper) -> INpgsqlTypeMapper` | static  | admits codecs on a mapper              |
|  [02]   | `UseNodaTime<TMapper>(TMapper) -> TMapper`            | static  | admits codecs, preserves receiver type |

- `UseNodaTime<TMapper>`: `TMapper : INpgsqlTypeMapper`, so `NpgsqlDataSourceBuilder` binds this generic and chains at provisioning.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Admission is provisioning-time: registering the resolver factory on the mapper is the only act, and thereafter every mapped temporal value round-trips as its NodaTime type with no query-path call.
- Coverage is the scalar set `timestamp`/`timestamptz`/`date`/`time`/`timetz`/`interval` over `Instant`, `LocalDateTime`, `ZonedDateTime`, `OffsetDateTime`, `LocalDate`, `LocalTime`, `OffsetTime`, `Period`, and `Duration`, widened by the range and multirange resolvers over `tsrange`/`tstzrange`/`daterange` carrying `Interval` and `DateInterval` and by the array resolver over each — so one admission covers every arity a temporal column reaches.
- Admission carries NO policy: unlike the spatial sibling there is no ordinate, precision, or default-type choice, so the call takes its receiver alone and a policy record beside it carries nothing.

[STACKING]:
- `api-npgsql-ef-nodatime`(`.api/api-npgsql-ef-nodatime.md`): the EF plugin's `UseNodaTime(NpgsqlDbContextOptionsBuilder)` maps the columns whose binary bytes this codec round-trips and depends on this package transitively; the two admissions pair, and the EF one never places a codec on a raw connection.
- `api-npgsql-nts`(`.api/api-npgsql-nts.md`): the spatial peer on the same builder, both admitted once at the data source so every raw lane reads both dialects.
- `api-nodatime`(`libs/dotnet/.api/api-nodatime.md`): this codec reads and writes that temporal object model, and the branch instant type it carries is the one every persisted timestamp resolves to.
- Store profile: `NpgsqlDataSourceBuilder` admits the codec at provisioning, so `QueueSqlCommand` writes, binary imports, and verification probes each read the branch instant rather than a platform date.

[LOCAL_ADMISSION]:
- PostgreSQL store profile admits `UseNodaTime` on the data source beside `UseNetTopologySuite` before any command opens a connection; a raw lane without it is the temporal drift the mapped lane never produces.

[RAIL_LAW]:
- Package: `Npgsql.NodaTime`
- Owns: NodaTime temporal ADO wire-codec admission for `Npgsql` across scalar, range, multirange, and array shapes
- Accept: `UseNodaTime` on the store data source or type mapper
- Reject: BCL `DateTime`/`DateTimeOffset` parameters on a mapped column, string-formatted timestamps, or EF-only plugin admission without the ADO codec
