# [RASM_COMPUTE_API_SQLITE]

Compute reads one slice of `Microsoft.Data.Sqlite`: a read-only ADO.NET fold over the EnergyPlus `eplusout.sql` results database, extracting the `TabularDataWithStrings` setpoint-not-met validity rows the SWIG `SqlFile` spells no accessor for. One bracketed `Mode=ReadOnly` connection per fold carries a parameterized SELECT family keyed on `(report, table, row, column)` onto `Option<double>`, so an absent or malformed row stays an absent fact. Persistence owns the embedded-store rail — raw interop, blob streams, UDF, checkpoint — at `libs/csharp/Rasm.Persistence/.api/api-sqlite.md`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Data.Sqlite` (Compute energy-results reader slice)
- package: `Microsoft.Data.Sqlite` (MIT) — meta-package; `Microsoft.Data.Sqlite.Core` carries the assembly, `SQLitePCLRaw.bundle_e_sqlite3` the `e_sqlite3` native provider
- assembly: `Microsoft.Data.Sqlite` (namespace `Microsoft.Data.Sqlite`); the `net10.0` consumer binds the `lib/net8.0` asset
- rail: read-only `eplusout.sql` tabular reader

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: consumed read-rail types — the ADO.NET `Db*` reader contract, every handle-owning type `IDisposable`

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY] | [CAPABILITY]                                                             |
| :-----: | :------------------------------ | :------------ | :----------------------------------------------------------------------- |
|  [01]   | `SqliteConnection`              | class         | opens the results file; ctor binds the string, `Open()` does the IO      |
|  [02]   | `SqliteConnectionStringBuilder` | class         | keywords: `DataSource`/`Mode`/`Cache`/`Pooling`/`DefaultTimeout`/`Vfs`   |
|  [03]   | `SqliteOpenMode`                | enum          | `ReadWriteCreate`, `ReadWrite`, `ReadOnly`, `Memory`                     |
|  [04]   | `SqliteCommand`                 | class         | parameterized SELECT carrier; `Prepare`/`ExecuteScalar`/`ExecuteReader`  |
|  [05]   | `SqliteParameterCollection`     | class         | `AddWithValue` + typed `Add(name, SqliteType)`; `@`/`$`/`:` placeholders |
|  [06]   | `SqliteParameter`               | class         | one bind; `SqliteType` pins the storage class over inference             |
|  [07]   | `SqliteType`                    | enum          | `Integer`/`Real`/`Text`/`Blob`; a string binds `Text` deterministically  |
|  [08]   | `SqliteDataReader`              | class         | row folds; `Read`/`GetOrdinal`/typed getters/`GetValue`/`IsDBNull`       |
|  [09]   | `SqliteException`               | class         | provider failure with `SqliteErrorCode` + `SqliteExtendedErrorCode`      |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: read-only connection lifecycle — one bracketed connection per fold

| [INDEX] | [SURFACE]                                                    | [SHAPE]  | [CAPABILITY]                                  |
| :-----: | :----------------------------------------------------------- | :------- | :-------------------------------------------- |
|  [01]   | `new SqliteConnection(string?)`                              | ctor     | binds the string; zero IO until `Open()`      |
|  [02]   | `Open()` / `Close()` / `Dispose()`                           | instance | `sqlite3_open_v2` under the resolved flags    |
|  [03]   | `Mode=ReadOnly`                                              | property | `SQLITE_OPEN_READONLY` — the loud-fail floor  |
|  [04]   | `Data Source=file:{path}?immutable=1`                        | property | `SQLITE_OPEN_URI` + `immutable=1` sealed read |
|  [05]   | `Pooling=False`                                              | property | disables default-on pooling                   |
|  [06]   | `SqliteConnection.ClearPool(connection)` / `ClearAllPools()` | static   | evicts pooled physical handles                |
|  [07]   | `Default Timeout` / `DefaultTimeout`                         | property | busy-timeout seconds (default 30)             |

- [03]-[READONLY]: `Mode=ReadOnly` throws `SqliteException` (error 14) on a missing file, where the default `ReadWriteCreate` silently creates an empty database — the fold needs the loud failure.
- [04]-[IMMUTABLE]: a `file:`-prefixed source arms `SQLITE_OPEN_URI`; `immutable=1` skips locking and change detection over a sealed post-run artifact.
- [05]-[POOLING]: pooling defaults on keyed by exact string; `Pooling=False` or `ClearPool`/`ClearAllPools` drops the physical handle with the bracket, before the scratch source is deleted.
- [07]-[TIMEOUT]: busy-timeout is moot under `immutable=1`, load-bearing when EnergyPlus still holds the artifact.

[ENTRYPOINT_SCOPE]: the parameterized tabular query — `SqliteCommand` + binds

| [INDEX] | [SURFACE]                                                     | [SHAPE]  | [CAPABILITY]                                    |
| :-----: | :------------------------------------------------------------ | :------- | :---------------------------------------------- |
|  [01]   | `connection.CreateCommand()`                                  | factory  | returns `SqliteCommand` bound to the connection |
|  [02]   | `command.CommandText`                                         | property | the SQL; `@`/`$`/`:`-prefixed placeholders      |
|  [03]   | `command.Parameters.AddWithValue(string?, object?)`           | instance | infers the storage class from the CLR value     |
|  [04]   | `command.Parameters.Add(string?, SqliteType)`                 | instance | pins the storage class explicitly               |
|  [05]   | `command.Prepare()`                                           | instance | precompiles into the statement cache            |
|  [06]   | `command.ExecuteScalar()`                                     | instance | first cell, `null` on empty → `Option` rail     |
|  [07]   | `command.ExecuteReader()` / `ExecuteReader(CommandBehavior)`  | instance | multi-row folds                                 |
|  [08]   | `SqliteException.SqliteErrorCode` / `SqliteExtendedErrorCode` | property | numeric provider codes for the fault row        |

- [03]-[BIND]: `AddWithValue` infers `Text` from a `string` — what the report/table/row key predicates need; `Add(name, SqliteType)` pins the class when inference must not decide.
- [05]-[PREPARE]: pays off only when one command re-executes across rebound parameters.
- [06]-[SCALAR]: `Optional(...)` absorbs both absence shapes — empty result, SQL `null` — onto the `Option` rail.
- [07]-[READER]: `ExecuteReader` folds a whole table (per-zone rows) where `ExecuteScalar` reads one cell.
- [08]-[ERRORCODE]: `SqliteErrorCode`/`SqliteExtendedErrorCode` are the numeric codes a `(Extraction, Foreign)` `ComputeFault.AnalysisFailed` row carries on a corrupt artifact.

[ENTRYPOINT_SCOPE]: reader folds and schema probes — `SqliteDataReader`

| [INDEX] | [SURFACE]                                                                | [SHAPE]  | [CAPABILITY]                                |
| :-----: | :----------------------------------------------------------------------- | :------- | :------------------------------------------ |
|  [01]   | `reader.Read()` / `NextResult()` / `HasRows`                             | fold     | row + statement advance                     |
|  [02]   | `reader.GetOrdinal(string)` / `GetName(int)`                             | instance | name→ordinal once, ordinal reads after      |
|  [03]   | `reader.GetValue(int)` / `GetString(int)`                                | instance | reads the TEXT `Value` for invariant parse  |
|  [04]   | `reader.GetDouble(int)` / `GetInt64(int)`                                | instance | coercing getters — the fabricated-zero trap |
|  [05]   | `reader.GetFieldValue<T>(int)` / `IsDBNull(int)`                         | instance | generic typed read + null guard             |
|  [06]   | `reader.GetSchemaTable()` / `GetDataTypeName(int)` / `GetFieldType(int)` | instance | declared-type result metadata               |
|  [07]   | `SELECT name FROM sqlite_master WHERE type='table'`                      | fold     | table listing                               |
|  [08]   | `SELECT * FROM pragma_table_info($table)`                                | fold     | column listing                              |

- [01]-[ADVANCE]: `Read()` advances rows over one result; `NextResult()` advances across batched statements.
- [03]-[TEXTVALUE]: `TabularDataWithStrings.Value` is TEXT, so the fold reads the string and parses invariant `double.TryParse`.
- [04]-[COERCE]: `sqlite3_column_double`/`_int64` coerce non-numeric TEXT to `0.0` silently — the fabricated-zero shape the invariant parse rejects.
- [07]-[INTROSPECT]: `GetSchema` surfaces only `MetaDataCollections` and `ReservedWords`, so table and column probes run as SQL.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- one connection per fold, bracketed `using` over the scratch directory's lifetime; the connection string builds from the resolved `sqlPath` parameter, never a literal path
- `Mode=ReadOnly` is the rail's floor: the open fails loudly on a missing artifact, the solver's file is never created or write-locked, and a write statement faults at the engine (`SQLITE_READONLY`, error 8) by open-flag construction
- one `(report, table, row, column)`-keyed query family covers every metric; a new metric is a new key tuple in the caller's data row, never a new method
- absence is the rail's third value: an empty `ExecuteScalar`, a non-numeric `Value`, and a missing table all land `None` through `Optional` + invariant `double.TryParse`; only a corrupt artifact (a thrown `SqliteException`) escalates to the typed `(Extraction, Foreign)` fault row

[STACKING]:
- `NREL.OpenStudio.macOS-arm64` (`.api/api-openstudio.md`): the SWIG `SqlFile` owns the structured summary reads (`totalSiteEnergy`/`totalSourceEnergy`/`endUses`) over the same `eplusout.sql`; this rail reads only the `TabularDataWithStrings` table `SqlFile` cannot spell, partitioning the file by accessor coverage
- `PollinationSDK` (`.api/api-pollination-sdk.md`): `EnergyRoute.Cloud` pulls the run's `eplusout.sql` asset and folds it through this identical extraction — one tabular reader serves both routes
- `Rasm.Persistence` (`libs/csharp/Rasm.Persistence/.api/api-sqlite.md`): the durable-store rail — `Handle` raw interop, `SqliteBlob` streams, UDF/collation/extension registration, backup and checkpoint — is Persistence-owned; Compute composes none of it and grows no second SQLite rail beside this reader
- `Analysis/energy` (`#SUBPROCESS_RESULTS`): `TabularFacts`/`Tabular` fold the `(report, table, row)`-keyed rows into `AssessmentFact.Measure` duration facts beside the `SqlFile` annual reads

[LOCAL_ADMISSION]:
- admitted in `Rasm.Compute.csproj` for this rail alone
- SQL text exists only inside the one query family; every predicate is a bound parameter, so the family's signature carries the keys and `CommandText` interpolation has no site
- coercing typed getters (`GetDouble`/`GetInt64` over TEXT) stay off the fold; the invariant-parse route owns numeric extraction

[RAIL_LAW]:
- Package: `Microsoft.Data.Sqlite` (Compute energy-results reader slice; full surface at the Persistence catalog)
- Owns: the read-only tabular extraction over `eplusout.sql` — the `(report, table, row, column)`-keyed `TabularDataWithStrings` query family and its `Option<double>` scalar fold
- Accept: a post-run EnergyPlus results artifact in the bracketed scratch (local subprocess or cloud-pulled), read under `Mode=ReadOnly` with pooling disabled for the one-shot path
- Reject: any write, transaction, or `PRAGMA` mutation; a second SQLite rail in Compute (EF through Persistence `api-ef-sqlite`, raw interop through the Persistence `Handle` bridge); re-deriving a read the SWIG `SqlFile` already spells; coercing typed getters where the invariant parse owns the numeric fold
