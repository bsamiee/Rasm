# [RASM_PERSISTENCE_API_DUCKDB]

`DuckDB.NET.Data.Full` owns the in-process DuckDB analytical store behind the `Query/columnar` `ColumnarProfile` algebra: the ADO.NET provider, the bulk appender and data-chunk vector rails, scalar and table function registration, and schema metadata. Appender and data-chunk throughput land as profile receipts, and the snapshot codecs project `[ValueObject]`/`[SmartEnum]` owners into columns through the typed `AppendValue`/`WriteValue` rails. One provider also carries the engine's full extension surface as `INSTALL`/`LOAD` SQL over `DuckDBCommand` — no second engine, no per-extension package.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `DuckDB.NET.Data.Full`
- package: `DuckDB.NET.Data.Full` (MIT)
- assembly: `DuckDB.NET.Data` (managed ADO surface; namespaces `DuckDB.NET.Data`, `DuckDB.NET.Data.Mapping`, `DuckDB.NET.Data.DataChunk.Reader`, `DuckDB.NET.Data.DataChunk.Writer`)
- companion: `DuckDB.NET.Bindings.Full` (transitive — assembly `DuckDB.NET.Bindings`, namespace `DuckDB.NET.Native`; carries `DuckDBQueryProgress`/`DuckDBErrorType`/`IDuckDBValueReader`/`DuckDBNativeConnection`/`DuckDBType`/`DuckDBDateOnly`/`DuckDBTimeOnly` and the native `duckdb` library)
- native: `DuckDB.NET.Bindings.Full/runtimes/<rid>/native` ships `osx`, `linux-x64`, `linux-arm64`, `win-x64`, `win-arm64`; the native `duckdb` shared library is RID-resolved at load, no per-RID managed split
- asset: runtime library
- rail: store-provider

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: ADO.NET provider surfaces

`DuckDBQueryProgress` (`DuckDB.NET.Native` struct) carries `double Percentage`, `ulong RowsProcessed`, `ulong TotalRowsToProcess` — the `GetQueryProgress()` receipt. `DuckDBErrorType` (`DuckDB.NET.Native` enum) discriminates `DuckDBException.ErrorType` over the native fault vocabulary, lifting `DuckDBException` to the store-profile fault rail.

`DuckDBConnectionStringBuilder` exposes `DataSource` and the `const string` anchors `InMemoryDataSource` (`:memory:`), `InMemoryConnectionString`, `InMemorySharedDataSource` (`:memory:?cache=shared`), `InMemorySharedConnectionString`. `DuckDBClientFactory : DbProviderFactory` carries `static readonly Instance` and `const ProviderInvariantName = "DuckDB.NET.Data"` for `DbProviderFactories` registration.

| [INDEX] | [SYMBOL]                                | [TYPE_FAMILY]     | [CAPABILITY]              |
| :-----: | :-------------------------------------- | :---------------- | :------------------------ |
|  [01]   | `DuckDBConnection`                      | connection root   | owns native connection    |
|  [02]   | `DuckDBCommand`                         | command surface   | executes SQL              |
|  [03]   | `DuckDBDataReader`                      | reader surface    | reads result rows         |
|  [04]   | `DuckDBParameter`                       | parameter surface | binds typed values        |
|  [05]   | `DuckDBParameterCollection`             | parameter set     | collects bound values     |
|  [06]   | `DuckDBTransaction`                     | transaction scope | commits or rolls back     |
|  [07]   | `DuckDBClientFactory`                   | provider factory  | creates ADO objects       |
|  [08]   | `DuckDBConnectionStringBuilder`         | connection string | names data sources        |
|  [09]   | `DuckDBException`                       | provider error    | carries native failure    |
|  [10]   | `DuckDB.NET.Native.DuckDBQueryProgress` | progress value    | reports query progress    |
|  [11]   | `DuckDB.NET.Native.DuckDBErrorType`     | error classifier  | classifies native failure |

[PUBLIC_TYPE_SCOPE]: bulk append surfaces

| [INDEX] | [SYMBOL]                                        | [TYPE_FAMILY]   | [CAPABILITY]           |
| :-----: | :---------------------------------------------- | :-------------- | :--------------------- |
|  [01]   | `DuckDB.NET.Data.DuckDBAppender`                | appender root   | streams bulk rows      |
|  [02]   | `DuckDB.NET.Data.IDuckDBAppenderRow`            | row contract    | appends typed values   |
|  [03]   | `DuckDB.NET.Data.DuckDBAppenderRow`             | row surface     | implements row appends |
|  [04]   | `DuckDB.NET.Data.DuckDBMappedAppender<T, TMap>` | mapped appender | appends mapped objects |
|  [05]   | `DuckDB.NET.Data.Mapping.DuckDBAppenderMap<T>`  | mapping owner   | maps object properties |

[PUBLIC_TYPE_SCOPE]: user-defined function surfaces

`TableFunction` is a record with a public `(IReadOnlyList<ColumnInfo> columns, IEnumerable data, CardinalityHint? cardinality = null)` ctor exposing `Columns`/`Cardinality`; its projection-push-down `DataFactory` ctor is `internal`, so column projection is reached only through `DuckDBConnectionTableFunctionExtensions.RegisterTableFunction<TData, TProjection>(…, Expression<Func<TData, TProjection>> projection)`.

`ScalarFunctionOptions` carries `IsPureFunction` (`bool?`, `init`) and `HandlesNulls` (`bool`, `init`); `ColumnInfo` is `(string Name, Type Type)`; `CardinalityHint` is `(ulong Value, bool IsExact = false)`; `ProjectedColumn` is `(int Index, string Name, Type Type)` materialized per-call when projection is active; `IDuckDBValueReader` exposes `T GetValue<T>()`.

| [INDEX] | [SYMBOL]                                   | [TYPE_FAMILY]      | [CAPABILITY]              |
| :-----: | :----------------------------------------- | :----------------- | :------------------------ |
|  [01]   | `TableFunction`                            | function result    | declares table function   |
|  [02]   | `ColumnInfo`                               | column declaration | names column type         |
|  [03]   | `CardinalityHint`                          | planner hint       | states row cardinality    |
|  [04]   | `ScalarFunctionOptions`                    | function options   | states scalar UDF policy  |
|  [05]   | `NamedAttribute`                           | parameter name     | names function parameters |
|  [06]   | `ProjectedColumn`                          | projection cell    | carries projected column  |
|  [07]   | `DuckDB.NET.Native.IDuckDBValueReader`     | value reader       | reads typed param values  |
|  [08]   | `DuckDBConnectionScalarFunctionExtensions` | scalar extension   | extends connection UDFs   |
|  [09]   | `DuckDBConnectionTableFunctionExtensions`  | table extension    | extends connection UDFs   |

[PUBLIC_TYPE_SCOPE]: data-chunk vector surfaces

| [INDEX] | [SYMBOL]                                             | [TYPE_FAMILY]   | [CAPABILITY]                     |
| :-----: | :--------------------------------------------------- | :-------------- | :------------------------------- |
|  [01]   | `DuckDB.NET.Data.DataChunk.Reader.IDuckDBDataReader` | reader contract | `IsValid(offset)`, `GetValue<T>` |
|  [02]   | `DuckDB.NET.Data.DataChunk.Writer.IDuckDBDataWriter` | writer contract | `WriteValue<T>`, `WriteNull`     |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: connection and command

`NativeConnection` returns `DuckDBNativeConnection` and requires an open connection; `BeginTransaction()` returns the typed `DuckDBTransaction` directly.

| [INDEX] | [SURFACE]          | [SHAPE]          | [CAPABILITY]              |
| :-----: | :----------------- | :--------------- | :------------------------ |
|  [01]   | `Open`             | connection call  | opens native database     |
|  [02]   | `BeginTransaction` | connection call  | starts typed transaction  |
|  [03]   | `CreateCommand`    | connection call  | creates typed command     |
|  [04]   | `Duplicate`        | connection call  | clones open connection    |
|  [05]   | `GetQueryProgress` | connection call  | reads execution progress  |
|  [06]   | `GetSchema`        | connection call  | reads metadata collection |
|  [07]   | `NativeConnection` | connection prop  | exposes native handle     |
|  [08]   | `ExecuteReader`    | command call     | streams result rows       |
|  [09]   | `ExecuteNonQuery`  | command call     | runs statement            |
|  [10]   | `ExecuteScalar`    | command call     | reads single value        |
|  [11]   | `UseStreamingMode` | command property | selects streaming results |
|  [12]   | `Prepare`          | command call     | prepares statement        |

[ENTRYPOINT_SCOPE]: appender operations

`CreateAppender` takes table, schema/table, and catalog/schema/table overloads; mapped appenders close after batch append. `IDuckDBAppenderRow.AppendValue` carries typed overloads for `bool?`, `byte[]?`, `Span<byte>`, `string?`, `decimal?`, `Guid?`, `BigInteger?`, every signed and unsigned integer width, `float?`, `double?`, `DateTime?`, `DateTimeOffset?`, `TimeSpan?`, `DateOnly?`, `TimeOnly?`, `DuckDBDateOnly?`, `DuckDBTimeOnly?`, `IEnumerable<T>?`, and the generic `TEnum? where TEnum : Enum`.

| [INDEX] | [SURFACE]                                    | [SHAPE]         | [CAPABILITY]                |
| :-----: | :------------------------------------------- | :-------------- | :-------------------------- |
|  [01]   | `CreateAppender`                             | raw appender    | opens raw table appender    |
|  [02]   | `CreateAppender<T,TMap>`                     | mapped appender | opens mapped appender       |
|  [03]   | `DuckDBAppender.CreateRow`                   | appender call   | starts row append           |
|  [04]   | `AppendValue<T>` / `AppendNullValue`         | row call        | appends typed or null cell  |
|  [05]   | `AppendDefault`                              | row call        | appends default cell        |
|  [06]   | `IDuckDBAppenderRow.EndRow`                  | row call        | seals appended row          |
|  [07]   | `DuckDBMappedAppender<T,TMap>.AppendRecords` | mapped call     | bulk-appends mapped batch   |
|  [08]   | `DuckDBMappedAppender<T,TMap>.Close`         | mapped call     | flushes and closes appender |
|  [09]   | `DuckDBAppender.Clear`                       | appender call   | discards pending rows       |
|  [10]   | `DuckDBAppender.Close`                       | appender call   | flushes appended rows       |

[ENTRYPOINT_SCOPE]: mapped appender protocol — `DuckDBAppenderMap<T>`

| [INDEX] | [SURFACE]                                   | [SHAPE]                      | [CAPABILITY]                         |
| :-----: | :------------------------------------------ | :--------------------------- | :----------------------------------- |
|  [01]   | `Map<TProperty>(Func<T, TProperty> getter)` | protected; declaration order | maps one column in declaration order |
|  [02]   | `DefaultValue()`                            | protected                    | writes the column's engine default   |
|  [03]   | `NullValue()`                               | protected                    | writes an explicit null cell         |

[ENTRYPOINT_SCOPE]: functions and data chunks

| [INDEX] | [SURFACE]                      | [SHAPE]              | [CAPABILITY]                           |
| :-----: | :----------------------------- | :------------------- | :------------------------------------- |
|  [01]   | `RegisterScalarFunction`       | connection low-level | registers `Action`-callback scalar UDF |
|  [02]   | `RegisterScalarFunction`       | scalar extension     | registers `Func`-based scalar UDF      |
|  [03]   | `RegisterTableFunction`        | connection low-level | registers callback table UDF           |
|  [04]   | `RegisterTableFunction`        | table extension      | registers typed projecting table UDF   |
|  [05]   | `GetValue`                     | reader call          | reads vector value                     |
|  [06]   | `IsValid`                      | reader call          | checks vector null mask                |
|  [07]   | `WriteValue`                   | writer call          | writes vector value                    |
|  [08]   | `WriteNull`                    | writer call          | writes vector null                     |
|  [09]   | `DuckDBClientFactory.Instance` | static singleton     | `DbProviderFactories` provider object  |

## [04]-[EXTENSION_SURFACE]

Every DuckDB extension enters as `INSTALL`/`LOAD` SQL over `DuckDBCommand.ExecuteNonQuery` — the SQL text is the whole API, no NuGet asset per extension. Bundled `duckdb` statically links the always-on core (`parquet`, `json`, `icu`); every other extension downloads on first `INSTALL` from a signed repository and binds into the running process at `LOAD`.

[EXTENSION_GRAMMAR]: SQL run through `DuckDBCommand.ExecuteNonQuery`

| [INDEX] | [SQL]                                        | [CAPABILITY]                                                  |
| :-----: | :------------------------------------------- | :------------------------------------------------------------ |
|  [01]   | `INSTALL <name>;`                            | download + cache the extension binary from the default repo   |
|  [02]   | `LOAD <name>;`                               | bind the installed extension into the current connection      |
|  [03]   | `INSTALL <name> FROM core;`                  | pin the `core` source repository (the default)                |
|  [04]   | `INSTALL <name> FROM community;`             | pin the `community` source repository                         |
|  [05]   | `INSTALL <name> FROM core_nightly;`          | pin the `core_nightly` source repository                      |
|  [06]   | `INSTALL <name> FROM '⟨repo_url⟩';`          | install from a custom repository URL (gzip + plain probed)    |
|  [07]   | `FORCE INSTALL <name>;`                      | re-download + overwrite the cached binary                     |
|  [08]   | `FORCE INSTALL <name> FROM core_nightly;`    | re-download the binary and switch repository                  |
|  [09]   | `UPDATE EXTENSIONS;`                         | refresh all installed extensions to newest compatible         |
|  [10]   | `UPDATE EXTENSIONS (<name>, …);`             | refresh named extensions to newest compatible                 |
|  [11]   | `SET custom_extension_repository = '⟨url⟩';` | redirect the default `INSTALL` repo for the session           |
|  [12]   | `SET extension_directory = '⟨path⟩';`        | relocate the install cache (per-profile, off home dir)        |
|  [13]   | `SET autoinstall_known_extensions = true;`   | auto-`INSTALL` a known extension on first reference           |
|  [14]   | `SET autoload_known_extensions = true;`      | auto-`LOAD` a known extension on first reference (default-on) |
|  [15]   | `SELECT * FROM duckdb_extensions();`         | metadata table of installed/loaded extensions                 |

- [15]-[DUCKDB_EXTENSIONS]: result columns `extension_name`, `loaded`, `installed`, `install_path`, `description`, `aliases`, `extension_version`, `installed_from`.

[EXTENSION_ROSTER]: core and community extensions admitted by the Rasm analytical lanes, each row's key SQL surface keyed below

| [INDEX] | [EXTENSION] | [ALIAS]             | [RASM_LANE]                                      |
| :-----: | :---------- | :------------------ | :----------------------------------------------- |
|  [01]   | `spatial`   | —                   | geometry columnar; meets NTS/GDAL at WKB         |
|  [02]   | `httpfs`    | `http`/`https`/`s3` | remote/object-store extract                      |
|  [03]   | `parquet`   | —                   | Parquet projection lane (BIM frames)             |
|  [04]   | `json`      | —                   | semi-structured payload lane                     |
|  [05]   | `iceberg`   | —                   | lakehouse read projection                        |
|  [06]   | `delta`     | —                   | lakehouse read projection                        |
|  [07]   | `postgres`  | `postgres_scanner`  | live PG join lane                                |
|  [08]   | `sqlite`    | `sqlite_scanner`    | embedded-store join lane                         |
|  [09]   | `mysql`     | `mysql_scanner`     | external-store join lane                         |
|  [10]   | `vss`       | —                   | columnar ANN; PG `pgvector` owns the txn index   |
|  [11]   | `fts`       | —                   | columnar full-text lane                          |
|  [12]   | `excel`     | —                   | spreadsheet ingest/extract                       |
|  [13]   | `avro`      | —                   | event-payload ingest lane                        |
|  [14]   | `aws`       | —                   | S3 credential rail (with `httpfs`)               |
|  [15]   | `azure`     | —                   | Azure blob credential rail                       |
|  [16]   | `inet`      | —                   | network-typed columns                            |
|  [17]   | `icu`       | —                   | temporal/locale correctness                      |
|  [18]   | `ducklake`  | —                   | catalog-managed lakehouse lane                   |
|  [19]   | `substrait` | `community`         | cross-engine plan interchange (`Query/columnar`) |

- [01]-[SPATIAL]: `ST_*` geometry algebra, `ST_Read` (GDAL-backed shapefile/GeoJSON/FlatGeobuf), `ST_GeomFromWKB`/`ST_AsWKB`, GeoParquet read/write.
- [02]-[HTTPFS]: read/write over HTTP(S) + S3 (`read_parquet('s3://…')`, `COPY … TO 's3://…'`).
- [03]-[PARQUET]: built-in — `read_parquet`, `COPY … TO '…' (FORMAT parquet)`, Hive partition pruning.
- [04]-[JSON]: built-in — `read_json_auto`, `->`/`->>` path ops, `json_*` functions.
- [05]-[ICEBERG]: `iceberg_scan('⟨table⟩')`, `ATTACH '⟨catalog⟩' (TYPE iceberg)` — Apache Iceberg tables.
- [06]-[DELTA]: `delta_scan('⟨path⟩')` — Delta Lake tables (complements `api-deltalake`).
- [07]-[POSTGRES]: `ATTACH '⟨conn⟩' AS pg (TYPE postgres)`, `postgres_scan('⟨conn⟩','schema','table')`.
- [08]-[SQLITE]: `ATTACH '⟨file.db⟩' (TYPE sqlite)` — SQLite tables.
- [09]-[MYSQL]: `ATTACH '⟨conn⟩' (TYPE mysql)` — MySQL tables.
- [10]-[VSS]: `CREATE INDEX … USING HNSW`, `array_distance`/`array_cosine_similarity` ANN.
- [11]-[FTS]: `PRAGMA create_fts_index(…)`, `match_bm25` — BM25 full-text search.
- [12]-[EXCEL]: `read_xlsx`/`COPY … TO '…' (FORMAT xlsx)` (complements `api-miniexcel`/`api-mpxj`).
- [13]-[AVRO]: `read_avro('⟨file⟩')` — Apache Avro decode.
- [14]-[AWS]: `CREATE SECRET (TYPE s3, PROVIDER credential_chain, …)` credential resolution.
- [15]-[AZURE]: `CREATE SECRET (TYPE azure, …)`, `az://`/`abfss://` paths.
- [16]-[INET]: `INET` type + CIDR/host operators.
- [17]-[ICU]: time-zone + collation support (built-in).
- [18]-[DUCKLAKE]: `ATTACH '⟨catalog⟩' (TYPE ducklake)` — DuckLake SQL lakehouse catalog.
- [19]-[SUBSTRAIT]: `get_substrait('⟨sql⟩')`/`get_substrait_json` → binary/JSON Substrait plan; `from_substrait(⟨blob⟩)`/`from_substrait_json` execute a foreign plan (community repo: `INSTALL substrait FROM community`).

[SECRET_AND_ATTACH]: credential and cross-engine SQL

| [INDEX] | [SQL]                                                                                  | [CAPABILITY]                                |
| :-----: | :------------------------------------------------------------------------------------- | :------------------------------------------ |
|  [01]   | `CREATE OR REPLACE SECRET ⟨name⟩ (TYPE s3, PROVIDER config, KEY_ID '…', SECRET '…');`  | explicit S3 credential secret               |
|  [02]   | `CREATE OR REPLACE SECRET ⟨name⟩ (TYPE s3, PROVIDER credential_chain, …);`             | chained/role S3 credentials                 |
|  [03]   | `CREATE OR REPLACE SECRET ⟨name⟩ IN postgres_⟨db⟩ (TYPE s3, …);`                       | persist into an attached store              |
|  [04]   | `ATTACH '⟨conn⟩' AS ⟨alias⟩ (TYPE postgres, READ_ONLY);`                               | cross-engine join against live PG           |
|  [05]   | `COPY (SELECT … FROM postgres_scan('⟨conn⟩',…)) TO '⟨file⟩.parquet' (FORMAT parquet);` | PG → Parquet extract                        |
|  [06]   | `SELECT extension_name, loaded, installed FROM duckdb_extensions() WHERE installed;`   | profile receipt of the loaded extension set |

[LOAD_PROTOCOL]:
- A profile declares its extension set once and runs the ordered `INSTALL <ext>; LOAD <ext>;` pairs through `DuckDBConnection.CreateCommand().ExecuteNonQuery()` right after `Open`, before any analytical query; the loaded set lands as a `Query/columnar` receipt queried back via `duckdb_extensions()`.
- Statically-linked `parquet`/`json`/`icu` resolve without network access; on-demand extensions require a one-time repository download, so a sealed profile pins a deterministic `extension_directory` to pre-warm the cache off the query path.
- `autoinstall_known_extensions`/`autoload_known_extensions` self-install a bare `read_parquet`/`ST_Read`/`iceberg_scan` reference; an explicit bootstrap `INSTALL`/`LOAD` stays the declared contract so the required set is a receipt, not an implicit query-time side effect.
- Credentials are `CREATE SECRET` objects, never inline keys in a path or `SET`; a secret persisted `IN postgres_⟨db⟩` survives reconnect, an in-memory secret is profile-scoped. `httpfs` is the prerequisite for every `s3://`/`http(s)://` path, and `aws`/`azure` resolve the credential, not the transport.

## [05]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every DuckDB op enters behind the one `Query/columnar` store-profile vocabulary: `DuckDBConnection` is the provider root, appenders and data-chunk writers the bulk root, scalar and table UDF registration the function root, and `GetSchema` the metadata root.

[STACKING]:
- `api-thinktecture-serialization`(`.api/api-thinktecture-serialization.md`): a `[ValueObject]`/`[SmartEnum]` owner crosses into a DuckDB column through `ThinktectureJsonConverterFactory`/`ThinktectureMessageFormatterResolver` projecting the owner to its key, which the typed `AppendValue` (or data-chunk `WriteValue<T>`) writes; the inverse decodes the column value back through the same factory, never a hand-rolled column mapping.
- `api-arrow`(`.api/api-arrow.md`): the `Query/columnar` extract path (DuckDB result → Arrow record batch) bridges through `Apache.Arrow` `RecordBatch`/`Schema` and the `Apache.Arrow.Adbc` driver manager over a native `[LibraryImport]` against `duckdb`; the managed `DuckDB.NET.Data` surface exposes no Arrow CLR member, so a zero-copy DuckDB→Arrow path is an explicit native-bridge rail, never a `DuckDBConnection` member.
- `api-ara3d-bimopenschema`(`.api/api-ara3d-bimopenschema.md`): the BIM analytics-frame producer this provider reads — `WriteDuckDB`/`DuckDbUtils.WriteToDuckDB` bulk-appends the columnar BIM tables (each suffixed `<Name>_<n>`) through a `DuckDBAppender`, and a Persistence analytics query opens that `.duckdb` over this same `DuckDBConnection` and SQL-joins the suffixed entity/parameter/relation tables.
- store-profile receipts: the appender batch and data-chunk vector throughput land as the `Query/columnar` typed receipt, and `GetQueryProgress()` `DuckDBQueryProgress` feeds a progress span through the AppHost `telemetry`/`drain` ports, never a bespoke logger.
- fault rail: `DuckDBException` lifts at the provider edge discriminated on `DuckDBErrorType`, joining the store-profile failure rail rather than surfacing as a raw ADO exception.
- spatial: `ST_AsWKB`/`ST_GeomFromWKB` exchange geometry with `api-nts-io` (`WKBReader`/`WKBWriter`) and `ST_Read` reads the GDAL formats admitted via `api-npgsql-nts`; DuckDB owns columnar spatial aggregation, PG GiST owns transactional spatial indexing.
- lakehouse: `delta_scan`/`iceberg_scan` read the same tables the managed `api-deltalake` writer produces — DuckDB is the read/aggregate projection, the managed writer the system of record, meeting at the table path.
- live PG join: `postgres` `ATTACH` reads the same database `api-npgsql`/`api-npgsql-ef` write; the analytical lane joins columnar DuckDB against live PG without an ETL hop under a staleness watermark, while the synchronous Marten/Npgsql path owns authoritative read-your-writes consistency.
- object store: `httpfs` with `aws`/`azure` secrets streams `s3://` Parquet against the same object store `api-objectstore`/`api-minio` front, while the content-keyed geometry-blob store owns durable artifacts.
- substrait: the `substrait` community extension serializes a DuckDB query to a portable Substrait plan (`get_substrait`) and executes a foreign plan (`from_substrait`), bridging to `Apache.Arrow.Adbc`'s Substrait surface as the `Query/columnar` `ColumnarExtension.Substrait` member; community-signed, so the profile is fail-closed — bootstrap probes `duckdb_extensions()` and rejects the profile when `substrait` is absent or unloaded rather than silently degrading.

[LOCAL_ADMISSION]:
- DuckDB surfaces enter behind the same store-profile vocabulary as every provider; appender and data-chunk throughput are profile receipts, not public service families.
- UDF registration requires explicit profile declarations with typed readers and writers; scalar UDFs bind either low-level `Action`-callbacks or high-level typed `Func` overloads, and table UDFs reach column-projection push-down only through the `Expression`-selector `RegisterTableFunction<TData, TProjection>`.
- Extension loading is profile policy expressed as `INSTALL`/`LOAD` SQL through `DuckDBCommand` at bootstrap; the loaded set is a profile receipt, never a per-extension NuGet package.

[RAIL_LAW]:
- Package: `DuckDB.NET.Data.Full`
- Owns: in-process DuckDB ADO provider admission, the bulk appender and data-chunk rails, scalar and table UDF registration, and the `INSTALL`/`LOAD` SQL extension-bootstrap that makes the engine fully-featured (spatial, remote, lakehouse, cross-engine, ANN, text) without a second engine or per-extension package
- Accept: the store profile with appender and UDF declarations, ordered `INSTALL`/`LOAD` bootstrap via `DuckDBCommand`, `CREATE SECRET` credentials, `ATTACH` cross-engine joins, `duckdb_extensions()` receipts
- Reject: provider-branded service families, a per-extension NuGet or native package, inline credentials in paths or `SET`, and treating an `ATTACH`-ed analytical lane as the consistency owner
