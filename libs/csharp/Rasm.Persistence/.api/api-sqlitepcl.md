# [RASM_PERSISTENCE_API_SQLITEPCL]

`SQLitePCLRaw.bundle_e_sqlite3` admits the bundled `e_sqlite3` native SQLite provider and the
`SQLitePCL.raw` 1:1 P/Invoke surface for the embedded SQLite store profile. The bundle ships no
managed `lib` assembly — it is a pure native-asset + initializer graph: it pins
`SQLitePCLRaw.config.e_sqlite3` (the `Batteries`/`Batteries_V2` initializer that calls
`raw.SetProvider`) and `SourceGear.sqlite3` (the SQLite **3.50.4** engine, shipped as `e_sqlite3`
native binaries for 31 RIDs). The store rail composes the low-level `raw.sqlite3_*` calls that
`Microsoft.Data.Sqlite` (`api-sqlite`) does not expose — backup, snapshot, WAL checkpoint,
per-connection db_config, and serialize/deserialize — reaching them through the
`SqliteConnection.Handle` (`SQLitePCL.sqlite3`) bridge.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `SQLitePCLRaw.bundle_e_sqlite3`
- package: `SQLitePCLRaw.bundle_e_sqlite3`
- version: `3.0.3` (direct `Directory.Packages.props` pin; overrides the `2.1.11` transitive constraint the `Microsoft.Data.Sqlite` graph requests)
- license: `Apache-2.0` (bundle/provider/core); native `SourceGear.sqlite3` is public-domain SQLite
- assembly: none — native-asset + initializer bundle, no managed `lib` DLL (resolves with `primary_assembly == None`)
- runtime assembly carrying `raw`: `SQLitePCLRaw.core` `3.0.3` (`lib/net8.0`, the consumer-bound TFM)
- namespace: `SQLitePCL`
- native engine: SQLite `3.50.4` via `SourceGear.sqlite3` `3.50.4.5` (`e_sqlite3.dll` / `libe_sqlite3.so` / `libe_sqlite3.dylib`)
- rail: store-provider

## [02]-[PACKAGE_ASSETS]

[PACKAGE_ASSET_SCOPE]: bundle dependency graph — which package carries which surface
- rail: store-provider

| [INDEX] | [SYMBOL]                          | [PACKAGE_ROLE]      | [CAPABILITY]                                                              |
| :-----: | :-------------------------------- | :------------------ | :----------------------------------------------------------------------- |
|  [01]   | `SQLitePCLRaw.bundle_e_sqlite3`   | bundle (no DLL)     | pins config + native, fixes the provider to `e_sqlite3`                  |
|  [02]   | `SQLitePCLRaw.config.e_sqlite3`   | config dependency   | carries `Batteries`/`Batteries_V2` (assembly `SQLitePCLRaw.batteries_v2`) |
|  [03]   | `SQLitePCLRaw.provider.e_sqlite3` | provider dependency | `SQLite3Provider_e_sqlite3 : ISQLite3Provider`, the bundled P/Invoke impl |
|  [04]   | `SQLitePCLRaw.core`               | core dependency     | the `SQLitePCL.raw` static API and `sqlite3`/`sqlite3_backup`/`sqlite3_snapshot` handle types |
|  [05]   | `SourceGear.sqlite3`              | native dependency   | SQLite 3.50.4 `e_sqlite3` binaries across 31 RIDs                        |

[RID_ABI]: native asset placement
- the bundle ships `e_sqlite3` for 31 RIDs (every desktop/mobile/wasm RID): `osx-arm64`, `osx-x64`, `win-x64`/`x86`/`arm64`, `linux-x64`/`arm64`/`musl-*`/`riscv64`/`s390x`/`ppc64le`, `android-*`, `ios*`, `maccatalyst-*`, `browser-wasm`
- the bridge target trims to the single consumer RID (`osx-arm64`) so only `libe_sqlite3.dylib` copies to output; a companion/outside-Rhino target keeps the host RID's asset
- `raw.GetNativeLibraryName()` returns the resolved native library basename for diagnostics

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: provider initialization
- rail: store-provider

| [INDEX] | [SURFACE]                          | [CALL_SHAPE]     | [CAPABILITY]                                                       |
| :-----: | :--------------------------------- | :--------------- | :----------------------------------------------------------------- |
|  [01]   | `Batteries_V2.Init()`              | static void      | `raw.SetProvider(new SQLite3Provider_e_sqlite3())` — the low-level arm |
|  [02]   | `Batteries.Init()`                 | static void      | thin facade forwarding to `Batteries_V2.Init()`                   |
|  [03]   | `raw.SetProvider(ISQLite3Provider)`| static void      | binds the provider explicitly; init must precede any `sqlite3_*` call |
|  [04]   | `raw.GetNativeLibraryName()`       | static string    | resolved native library basename                                   |

Initialization is explicit and idempotent through the store-profile open path; it never hides in
unrelated startup code. `Microsoft.Data.Sqlite` invokes the same provider init internally, so the
embedded profile shares one bound `e_sqlite3` provider with the raw call surface.

[ENTRYPOINT_SCOPE]: raw interop surface — `SQLitePCL.raw` static class, `SQLitePCLRaw.core`
- rail: store-provider

The handle types are `sqlite3` (connection), `sqlite3_backup`, and `sqlite3_snapshot`; every member
below is a `public static` on `SQLitePCL.raw` and is reached via `SqliteConnection.Handle`
(`sqlite3?`). Status is an `int` return checked against the `[RAW_CONSTANTS]` codes.

[RAW_SIGNATURES]: decompile-verified `SQLitePCL.raw` member signatures (`SQLitePCLRaw.core` 3.0.3)
- rail: store-provider

```csharp generated
// --- connection config (THREE overloads — int-flag, utf8z, pointer) ---
int sqlite3_db_config(sqlite3 db, int op, int val, out int result)
int sqlite3_db_config(sqlite3 db, int op, utf8z val)
int sqlite3_db_config(sqlite3 db, int op, nint ptr, int int0, int int1)
int sqlite3_extended_result_codes(sqlite3 db, int onoff)

// --- WAL checkpoint ---
int sqlite3_wal_checkpoint(sqlite3 db, string dbName)
int sqlite3_wal_checkpoint_v2(sqlite3 db, string dbName, int eMode, out int logSize, out int framesCheckPointed)

// --- snapshot (pin a consistent WAL read view) ---
int sqlite3_snapshot_get(sqlite3 db, string schema, out sqlite3_snapshot snap)
int sqlite3_snapshot_open(sqlite3 db, string schema, sqlite3_snapshot snap)
int sqlite3_snapshot_cmp(sqlite3_snapshot p1, sqlite3_snapshot p2)
int sqlite3_snapshot_recover(sqlite3 db, string name)
void sqlite3_snapshot_free(sqlite3_snapshot snap)

// --- online backup (paged copy) ---
sqlite3_backup sqlite3_backup_init(sqlite3 destDb, string destName, sqlite3 sourceDb, string sourceName)
int sqlite3_backup_step(sqlite3_backup backup, int nPage)
int sqlite3_backup_remaining(sqlite3_backup backup)
int sqlite3_backup_pagecount(sqlite3_backup backup)
int sqlite3_backup_finish(sqlite3_backup backup)

// --- in-memory serialize / deserialize (whole-schema byte image) ---
nint sqlite3_serialize(sqlite3 db, string schema, out long size, int flags)
int sqlite3_deserialize(sqlite3 db, string schema, nint data, long deserializedDataSize, long maxDataSize, int flags)

// --- extension loading (raw API; SQL-level load_extension() stays disabled by db_config) ---
int sqlite3_enable_load_extension(sqlite3 db, int onoff)
int sqlite3_load_extension(sqlite3 db, utf8z file, utf8z proc, out utf8z errmsg)
```

[RAW_CONSTANTS]: decompile-verified `SQLitePCL.raw` integer constants (`SQLitePCLRaw.core` carries 242 `SQLITE_*` consts)
- rail: store-provider

| [INDEX] | [CONSTANT]                              | [VALUE] | [CAPABILITY]                |
| :-----: | :-------------------------------------- | ------: | :-------------------------- |
|  [01]   | `SQLITE_DBCONFIG_DEFENSIVE`             |    1010 | defensive-mode hardening    |
|  [02]   | `SQLITE_DBCONFIG_DQS_DML`               |    1013 | double-quoted DML rejection |
|  [03]   | `SQLITE_DBCONFIG_DQS_DDL`               |    1014 | double-quoted DDL rejection |
|  [04]   | `SQLITE_DBCONFIG_ENABLE_TRIGGER`        |    1003 | trigger enablement          |
|  [05]   | `SQLITE_DBCONFIG_ENABLE_VIEW`           |    1015 | view enablement             |
|  [06]   | `SQLITE_DBCONFIG_ENABLE_LOAD_EXTENSION` |    1005 | db_config extension arming  |
|  [07]   | `SQLITE_OK`                             |       0 | status code                 |
|  [08]   | `SQLITE_DONE`                           |     101 | status code                 |
|  [09]   | `SQLITE_BUSY`                           |       5 | retry status code           |
|  [10]   | `SQLITE_CORRUPT`                        |      11 | corruption status code      |
|  [11]   | `SQLITE_NOTADB`                         |      26 | not-a-database status code  |
|  [12]   | `SQLITE_CHECKPOINT_PASSIVE`             |       0 | checkpoint mode             |
|  [13]   | `SQLITE_CHECKPOINT_FULL`                |       1 | checkpoint mode             |
|  [14]   | `SQLITE_CHECKPOINT_RESTART`             |       2 | checkpoint mode             |
|  [15]   | `SQLITE_CHECKPOINT_TRUNCATE`            |       3 | checkpoint mode             |

## [04]-[IMPLEMENTATION_LAW]

[NATIVE_ADMISSION]:
- package role: SQLite native provider admission + the raw P/Invoke API
- runtime root: bundled `e_sqlite3` provider (`SQLite3Provider_e_sqlite3`)
- initializer root: `Batteries_V2.Init()` (or the `Batteries.Init()` facade) calling `raw.SetProvider`
- store root: embedded SQLite profile only

[INTEGRATION_STACK]:
- `Store/engine#WITH_SNAPSHOT` brackets a consistent multi-transaction WAL read view: `raw.sqlite3_snapshot_get(connection.Handle, schema, out snapshot)`, one `sqlite3_snapshot_recover` retry on a refused pin, a `sqlite3_snapshot_cmp` monotonic-floor guard so a reader never regresses across brackets, `sqlite3_snapshot_open`, and `sqlite3_snapshot_free` of only a held handle — all over the `SqliteConnection.Handle` bridge from `api-sqlite`
- `Store/engine#MAINTAIN` runs the `raw.sqlite3_wal_checkpoint_v2(Handle, "main", raw.SQLITE_CHECKPOINT_TRUNCATE, out logFrames, out checkpointed)` out-param form so the typed `SqliteFact` carries log-frame and checkpointed-frame counts, and a `SQLITE_BUSY` return receipts a retry; the paged `sqlite3_backup_*` session over `Handle` subsumes the provider's whole-file `BackupDatabase` by adding `_remaining`/`_pagecount` progress facts
- `Store/engine#EXTENSION_LOAD` folds the `DbConfig.Hardened` set through `raw.sqlite3_db_config(Handle, Op, Value, out _)` (the int-flag overload): `SQLITE_DBCONFIG_DEFENSIVE = 1`, `SQLITE_DBCONFIG_DQS_DDL`/`DQS_DML = 0`, `SQLITE_DBCONFIG_ENABLE_TRIGGER`/`ENABLE_VIEW`/`ENABLE_LOAD_EXTENSION = 1`, applied once per physical open before any user statement — so defensive mode and double-quoted-literal rejection are connection policy, not connection-string knobs, and only the db_config extension-arming path is enabled while the SQL-level `load_extension()` function stays off
- `Store/encryption#SQLITE_KEYING` swaps the provider: SQLCipher keying (`PRAGMA key`/`cipher_migrate`/`rekey`) runs over `SQLite3Provider_sqlcipher`, a different bundle, not this `e_sqlite3` provider — this catalog owns the `e_sqlite3` loadable-extension and db-config posture only
- `sqlite3_serialize`/`sqlite3_deserialize` move a whole-schema byte image in/out of an in-memory database without file IO — the snapshot rail's path for a memory-backed store image distinct from the `byte[]` content-chunk frame

[LOCAL_ADMISSION]:
- native SQLite provider setup belongs to the SQLite store profile; init is explicit and cannot hide in unrelated startup code
- the raw `sqlite3_*` calls reach through `SqliteConnection.Handle`; provider-bundle facts stay in the store-profile rail and never define public Persistence vocabulary
- a status `int` is matched against the `[RAW_CONSTANTS]` codes; `SQLITE_BUSY` is a retry receipt, never a fault

[RAIL_LAW]:
- Package: `SQLitePCLRaw.bundle_e_sqlite3`
- Owns: SQLite native provider admission + the `SQLitePCL.raw` low-level interop the ADO surface omits
- Accept: embedded SQLite runtime; backup/snapshot/WAL/db_config/serialize raw calls over `Handle`
- Reject: the SQLCipher bundle (it is the `Store/encryption` provider); SQL-level `load_extension()` (db_config-armed loader only)
