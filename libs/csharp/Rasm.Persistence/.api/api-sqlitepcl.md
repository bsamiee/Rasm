# [RASM_PERSISTENCE_API_SQLITEPCL]

`SQLitePCLRaw.bundle_e_sqlite3` binds the `e_sqlite3` native SQLite engine to the process and opens `SQLitePCL.raw`, the 1:1 P/Invoke surface carrying every engine call the ADO transport omits. Every call takes the `sqlite3` handle `SqliteConnection.Handle` exposes, so the managed transport and the raw rail share one native connection and one transaction state. Provider binding is a process-wide act the embedded store profile owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `SQLitePCLRaw.bundle_e_sqlite3`
- package: `SQLitePCLRaw.bundle_e_sqlite3` (`Apache-2.0`, SQLitePCLRaw; the `e_sqlite3` engine it ships is public-domain SQLite)
- assembly: none — a native-asset and initializer graph resolving with no managed `lib` DLL; `SQLitePCLRaw.core` carries `raw` at runtime
- namespace: `SQLitePCL`
- asset: derive native coverage from the restored `SourceGear.sqlite3` `runtimes/<RID>/native/` asset graph
- rail: store-provider
- depends:
  - `SQLitePCLRaw.config.e_sqlite3`: carries `Batteries`/`Batteries_V2` in assembly `SQLitePCLRaw.batteries_v2`
  - `SQLitePCLRaw.provider.e_sqlite3`: implements `SQLite3Provider_e_sqlite3 : ISQLite3Provider`, the bundled P/Invoke arm
  - `SQLitePCLRaw.core`: carries `SQLitePCL.raw` and the `sqlite3*` handle types
  - `SourceGear.sqlite3`: ships the `e_sqlite3` native engine binaries


## [02]-[PUBLIC_TYPES]

[PROVIDER_TYPES]: binding graph — one provider implementation binds per process.

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY] | [CAPABILITY]                                                |
| :-----: | :-------------------------- | :------------ | :---------------------------------------------------------- |
|  [01]   | `raw`                       | static class  | every P/Invoke entrypoint and the `SQLITE_*` constant set   |
|  [02]   | `Batteries_V2`              | static class  | binds `SQLite3Provider_e_sqlite3` through `raw.SetProvider` |
|  [03]   | `Batteries`                 | static class  | facade forwarding to `Batteries_V2.Init()`                  |
|  [04]   | `ISQLite3Provider`          | interface     | the contract every `raw` member dispatches through          |
|  [05]   | `SQLite3Provider_e_sqlite3` | class         | the bundled `e_sqlite3` P/Invoke implementation             |

[HANDLE_TYPES]: `SafeHandle` engine handles, each reading `IsInvalid` as `handle == IntPtr.Zero`.

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [CAPABILITY]                                        |
| :-----: | :----------------- | :------------ | :-------------------------------------------------- |
|  [01]   | `sqlite3`          | safe handle   | the connection `SqliteConnection.Handle` hands over |
|  [02]   | `sqlite3_stmt`     | safe handle   | one prepared statement                              |
|  [03]   | `sqlite3_backup`   | safe handle   | an online paged-copy session                        |
|  [04]   | `sqlite3_snapshot` | safe handle   | a pinned WAL read view                              |
|  [05]   | `sqlite3_blob`     | safe handle   | an incremental blob cursor over one cell            |

[INTEROP_TYPES]: value carriers the P/Invoke signatures take and return.

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY]       | [CAPABILITY]                                                |
| :-----: | :---------------- | :------------------ | :---------------------------------------------------------- |
|  [01]   | `utf8z`           | readonly ref struct | zero-terminated UTF-8 span; `utf8_to_string()` materializes |
|  [02]   | `sqlite3_value`   | class               | one argument handed to a registered function                |
|  [03]   | `sqlite3_context` | class               | the result sink a registered function writes                |

[HOOK_DELEGATES]: `delegate_update` `delegate_commit` `delegate_rollback` `delegate_authorizer` `delegate_progress`
- `delegate_update`, `delegate_authorizer`: each carries a `strdelegate_*` twin marshalling `string` where the `utf8z` form hands raw engine spans.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: provider binding, before any `sqlite3_*` call on the process.

| [INDEX] | [SURFACE]                              | [SHAPE] | [CAPABILITY]                                                |
| :-----: | :------------------------------------- | :------ | :---------------------------------------------------------- |
|  [01]   | `Batteries_V2.Init()`                  | static  | binds `SQLite3Provider_e_sqlite3` through `raw.SetProvider` |
|  [02]   | `Batteries.Init()`                     | static  | forwards to `Batteries_V2.Init()`                           |
|  [03]   | `raw.SetProvider(ISQLite3Provider)`    | static  | binds a provider implementation explicitly                  |
|  [04]   | `raw.FreezeProvider(bool)`             | static  | locks the bound provider against a later rebind             |
|  [05]   | `raw.GetNativeLibraryName() -> string` | static  | the resolved native library basename                        |

- `raw.SetProvider`: an unbound `raw` call throws; `Microsoft.Data.Sqlite` runs the same init internally, so the embedded profile shares one bound provider with the raw rail.

[ENTRYPOINT_SCOPE]: engine operations over the shared `sqlite3` handle — backup, snapshot, checkpoint, image moves, and blob cursors.

| [INDEX] | [SURFACE]                                                               | [SHAPE] | [CAPABILITY]                                        |
| :-----: | :---------------------------------------------------------------------- | :------ | :-------------------------------------------------- |
|  [01]   | `raw.sqlite3_backup_init(sqlite3, string, sqlite3, string)`             | static  | opens a paged copy between two connections          |
|  [02]   | `raw.sqlite3_backup_step(sqlite3_backup, int)`                          | static  | copies N pages; `SQLITE_DONE` closes the run        |
|  [03]   | `raw.sqlite3_backup_remaining(sqlite3_backup) -> int`                   | static  | pages left in the session                           |
|  [04]   | `raw.sqlite3_backup_pagecount(sqlite3_backup) -> int`                   | static  | total pages in the session                          |
|  [05]   | `raw.sqlite3_backup_finish(sqlite3_backup)`                             | static  | closes the session and reports its status           |
|  [06]   | `raw.sqlite3_snapshot_get(sqlite3, string, out sqlite3_snapshot)`       | static  | pins a schema's current WAL read view               |
|  [07]   | `raw.sqlite3_snapshot_open(sqlite3, string, sqlite3_snapshot)`          | static  | re-enters a pinned view on a fresh read transaction |
|  [08]   | `raw.sqlite3_snapshot_cmp(sqlite3_snapshot, sqlite3_snapshot) -> int`   | static  | orders two views; the monotonic-floor guard         |
|  [09]   | `raw.sqlite3_snapshot_recover(sqlite3, string)`                         | static  | restores snapshot availability after checkpointing  |
|  [10]   | `raw.sqlite3_snapshot_free(sqlite3_snapshot)`                           | static  | releases a held view                                |
|  [11]   | `raw.sqlite3_wal_checkpoint(sqlite3, string)`                           | static  | passive checkpoint of one schema                    |
|  [12]   | `raw.sqlite3_wal_checkpoint_v2(sqlite3, string, int, out int, out int)` | static  | mode-selected checkpoint over log and copied frames |
|  [13]   | `raw.sqlite3_wal_autocheckpoint(sqlite3, int)`                          | static  | the automatic checkpoint threshold in WAL frames    |
|  [14]   | `raw.sqlite3_serialize(sqlite3, string, out long, int) -> nint`         | static  | a whole-schema byte image without file IO           |
|  [15]   | `raw.sqlite3_deserialize(sqlite3, string, nint, long, long, int)`       | static  | mounts a byte image as the schema's database        |
|  [16]   | `raw.sqlite3_free(nint)`                                                | static  | releases a `sqlite3_serialize` buffer               |
|  [17]   | `raw.sqlite3_blob_open(sqlite3, string, string, string, long, int)`     | static  | opens a cursor on one cell as an out handle         |
|  [18]   | `raw.sqlite3_blob_reopen(sqlite3_blob, long)`                           | static  | rebinds the open cursor to another rowid            |
|  [19]   | `raw.sqlite3_blob_read(sqlite3_blob, Span<byte>, int)`                  | static  | reads at an offset into a caller span               |
|  [20]   | `raw.sqlite3_blob_write(sqlite3_blob, ReadOnlySpan<byte>, int)`         | static  | writes a span at an offset                          |
|  [21]   | `raw.sqlite3_blob_bytes(sqlite3_blob) -> int`                           | static  | the cell's byte length                              |
|  [22]   | `raw.sqlite3_blob_close(sqlite3_blob)`                                  | static  | closes the cursor                                   |

[CHECKPOINT_MODES]: `SQLITE_CHECKPOINT_PASSIVE` `SQLITE_CHECKPOINT_FULL` `SQLITE_CHECKPOINT_RESTART` `SQLITE_CHECKPOINT_TRUNCATE`
[IMAGE_FLAGS]: `SQLITE_SERIALIZE_NOCOPY` `SQLITE_DESERIALIZE_FREEONCLOSE` `SQLITE_DESERIALIZE_RESIZEABLE` `SQLITE_DESERIALIZE_READONLY`
- `raw.sqlite3_serialize`: `SQLITE_SERIALIZE_NOCOPY` hands back the engine's own image pointer instead of a copy, and yields null where the schema is not memory-backed.
- `raw.sqlite3_free`: `SQLITE_DESERIALIZE_FREEONCLOSE` transfers buffer ownership to the engine, which frees it on close and on a rejected load.
- `raw.sqlite3_blob_reopen`: one cursor scans a rowid sequence without a per-cell open, where the ADO blob stream allocates per cell.

[ENTRYPOINT_SCOPE]: per-connection configuration and caps, applied once per physical open.

| [INDEX] | [SURFACE]                                                      | [SHAPE] | [CAPABILITY]                                       |
| :-----: | :------------------------------------------------------------- | :------ | :------------------------------------------------- |
|  [01]   | `raw.sqlite3_db_config(sqlite3, int, int, out int)`            | static  | sets a boolean op and reads back the applied value |
|  [02]   | `raw.sqlite3_db_config(sqlite3, int, utf8z)`                   | static  | sets a text-valued op                              |
|  [03]   | `raw.sqlite3_db_config(sqlite3, int, nint, int, int)`          | static  | sets a pointer-valued op                           |
|  [04]   | `raw.sqlite3_limit(sqlite3, int, int) -> int`                  | static  | reads or sets one per-connection cap               |
|  [05]   | `raw.sqlite3_busy_timeout(sqlite3, int)`                       | static  | the busy-handler wait in milliseconds              |
|  [06]   | `raw.sqlite3_extended_result_codes(sqlite3, int)`              | static  | widens returns to extended result codes            |
|  [07]   | `raw.sqlite3_enable_load_extension(sqlite3, int)`              | static  | arms the C-API extension loader                    |
|  [08]   | `raw.sqlite3_load_extension(sqlite3, utf8z, utf8z, out utf8z)` | static  | loads a native extension by path and entry         |

[DB_CONFIG_OPS]: `SQLITE_DBCONFIG_DEFENSIVE` `SQLITE_DBCONFIG_DQS_DML` `SQLITE_DBCONFIG_DQS_DDL` `SQLITE_DBCONFIG_TRUSTED_SCHEMA` `SQLITE_DBCONFIG_ENABLE_FKEY` `SQLITE_DBCONFIG_ENABLE_TRIGGER` `SQLITE_DBCONFIG_ENABLE_VIEW` `SQLITE_DBCONFIG_ENABLE_QPSG` `SQLITE_DBCONFIG_ENABLE_LOAD_EXTENSION` `SQLITE_DBCONFIG_NO_CKPT_ON_CLOSE` `SQLITE_DBCONFIG_WRITABLE_SCHEMA` `SQLITE_DBCONFIG_RESET_DATABASE`
[LIMIT_IDS]: `SQLITE_LIMIT_LENGTH` `SQLITE_LIMIT_SQL_LENGTH` `SQLITE_LIMIT_COLUMN` `SQLITE_LIMIT_EXPR_DEPTH` `SQLITE_LIMIT_COMPOUND_SELECT` `SQLITE_LIMIT_VDBE_OP` `SQLITE_LIMIT_FUNCTION_ARG` `SQLITE_LIMIT_ATTACHED` `SQLITE_LIMIT_LIKE_PATTERN_LENGTH` `SQLITE_LIMIT_VARIABLE_NUMBER` `SQLITE_LIMIT_TRIGGER_DEPTH` `SQLITE_LIMIT_WORKER_THREADS`
- `raw.sqlite3_limit`: a negative `newVal` reads the cap without changing it, and every call returns the prior value.

[ENTRYPOINT_SCOPE]: status, introspection, and diagnostics feeding typed receipts.

| [INDEX] | [SURFACE]                                                            | [SHAPE]  | [CAPABILITY]                                           |
| :-----: | :------------------------------------------------------------------- | :------- | :----------------------------------------------------- |
|  [01]   | `raw.sqlite3_db_status(sqlite3, int, out int, out int, int)`         | static   | current and highwater for one connection metric        |
|  [02]   | `raw.sqlite3_stmt_status(sqlite3_stmt, int, int) -> int`             | static   | one statement counter, cleared by a nonzero reset flag |
|  [03]   | `sqlite3.enable_sqlite3_next_stmt(bool)`                             | instance | arms the connection's prepared-statement registry      |
|  [04]   | `raw.sqlite3_next_stmt(sqlite3, sqlite3_stmt) -> sqlite3_stmt`       | static   | walks the connection's prepared statements             |
|  [05]   | `raw.sqlite3_sql(sqlite3_stmt) -> utf8z`                             | static   | the statement's original SQL text                      |
|  [06]   | `raw.sqlite3_stmt_readonly(sqlite3_stmt) -> int`                     | static   | whether the statement writes                           |
|  [07]   | `raw.sqlite3_stmt_isexplain(sqlite3_stmt) -> int`                    | static   | whether the statement is an EXPLAIN form               |
|  [08]   | `raw.sqlite3_stmt_busy(sqlite3_stmt) -> int`                         | static   | whether the statement is mid-step                      |
|  [09]   | `raw.sqlite3_extended_errcode(sqlite3) -> int`                       | static   | the extended code of the last failed call              |
|  [10]   | `raw.sqlite3_errmsg(sqlite3) -> utf8z`                               | static   | the message for the last failed call                   |
|  [11]   | `raw.sqlite3_errstr(int) -> utf8z`                                   | static   | the text for any result code                           |
|  [12]   | `raw.sqlite3_db_readonly(sqlite3, string) -> int`                    | static   | whether a schema is read-only                          |
|  [13]   | `raw.sqlite3_db_filename(sqlite3, string) -> utf8z`                  | static   | a schema's backing file path                           |
|  [14]   | `raw.sqlite3_get_autocommit(sqlite3) -> int`                         | static   | whether a transaction is open                          |
|  [15]   | `raw.sqlite3_changes(sqlite3) -> int`                                | static   | rows changed by the last statement                     |
|  [16]   | `raw.sqlite3_total_changes(sqlite3) -> int`                          | static   | rows changed over the connection's life                |
|  [17]   | `raw.sqlite3_last_insert_rowid(sqlite3) -> long`                     | static   | the rowid of the last insert                           |
|  [18]   | `raw.sqlite3_table_column_metadata(sqlite3, string, string, string)` | static   | declared type, collation, and key flags as out values  |
|  [19]   | `raw.sqlite3_compileoption_used(string) -> int`                      | static   | whether the bound engine carries a compile option      |
|  [20]   | `raw.sqlite3_compileoption_get(int) -> utf8z`                        | static   | enumerates the bound engine's compile options          |
|  [21]   | `raw.sqlite3_libversion() -> utf8z`                                  | static   | the engine version text                                |
|  [22]   | `raw.sqlite3_libversion_number() -> int`                             | static   | the engine version as an integer                       |
|  [23]   | `raw.sqlite3_sourceid() -> utf8z`                                    | static   | the engine's source check-in identity                  |
|  [24]   | `raw.sqlite3_status(int, out int, out int, int)`                     | static   | a process-wide engine metric with its highwater        |
|  [25]   | `raw.sqlite3_memory_used() -> long`                                  | static   | bytes the engine holds                                 |
|  [26]   | `raw.sqlite3_memory_highwater(int) -> long`                          | static   | peak bytes, cleared by a nonzero reset flag            |
|  [27]   | `raw.sqlite3_soft_heap_limit64(long) -> long`                        | static   | the advisory heap ceiling                              |
|  [28]   | `raw.sqlite3_hard_heap_limit64(long) -> long`                        | static   | the enforced heap ceiling                              |

[DB_STATUS_OPS]: `SQLITE_DBSTATUS_CACHE_USED` `SQLITE_DBSTATUS_SCHEMA_USED` `SQLITE_DBSTATUS_STMT_USED` `SQLITE_DBSTATUS_CACHE_HIT` `SQLITE_DBSTATUS_CACHE_MISS` `SQLITE_DBSTATUS_CACHE_WRITE` `SQLITE_DBSTATUS_DEFERRED_FKS` `SQLITE_DBSTATUS_LOOKASIDE_USED` `SQLITE_DBSTATUS_LOOKASIDE_HIT` `SQLITE_DBSTATUS_LOOKASIDE_MISS_SIZE` `SQLITE_DBSTATUS_LOOKASIDE_MISS_FULL`
[STMT_STATUS_OPS]: `SQLITE_STMTSTATUS_FULLSCAN_STEP` `SQLITE_STMTSTATUS_SORT` `SQLITE_STMTSTATUS_AUTOINDEX` `SQLITE_STMTSTATUS_VM_STEP`
[RESULT_CODES]: `SQLITE_OK` `SQLITE_DONE` `SQLITE_ERROR` `SQLITE_BUSY` `SQLITE_LOCKED` `SQLITE_READONLY` `SQLITE_IOERR` `SQLITE_FULL` `SQLITE_CORRUPT` `SQLITE_NOTADB` `SQLITE_BUSY_SNAPSHOT` `SQLITE_BUSY_RECOVERY`
- `sqlite3.enable_sqlite3_next_stmt`: an unarmed connection throws at the walk, so arming rides the open path.
- `raw.sqlite3_stmt_status`: `SQLITE_STMTSTATUS_FULLSCAN_STEP` is the plan-regression tell across a harvest window.
- `raw.sqlite3_compileoption_used`: `SQLITE_ENABLE_DBSTAT_VTAB` is absent from this `e_sqlite3` build, so the probe gates any `dbstat` metric.

[ENTRYPOINT_SCOPE]: change notification, statement authorization, and cancellation — none of it reachable from the ADO transport.

| [INDEX] | [SURFACE]                                                               | [SHAPE] | [CAPABILITY]                                         |
| :-----: | :---------------------------------------------------------------------- | :------ | :--------------------------------------------------- |
|  [01]   | `raw.sqlite3_update_hook(sqlite3, delegate_update, object)`             | static  | per-row change notification carrying table and rowid |
|  [02]   | `raw.sqlite3_commit_hook(sqlite3, delegate_commit, object)`             | static  | pre-commit veto; a nonzero return forces rollback    |
|  [03]   | `raw.sqlite3_rollback_hook(sqlite3, delegate_rollback, object)`         | static  | rollback notification                                |
|  [04]   | `raw.sqlite3_set_authorizer(sqlite3, delegate_authorizer, object)`      | static  | per-action verdict at prepare time                   |
|  [05]   | `raw.sqlite3_progress_handler(sqlite3, int, delegate_progress, object)` | static  | callback every N VM steps; a nonzero return aborts   |
|  [06]   | `raw.sqlite3_interrupt(sqlite3)`                                        | static  | aborts the running statement from another thread     |

[AUTHORIZER_VERDICTS]: `SQLITE_OK` `SQLITE_DENY` `SQLITE_IGNORE`
[ACTION_CODES]: `SQLITE_SELECT` `SQLITE_READ` `SQLITE_INSERT` `SQLITE_UPDATE` `SQLITE_DELETE` `SQLITE_PRAGMA` `SQLITE_ATTACH`
- `raw.sqlite3_update_hook`: `sqlite3` roots every hook delegate and its `user_data` in the connection's extra-state slot for the connection's life.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `raw` dispatches every member through the one `ISQLite3Provider` bound at init, so provider selection is a process fact and never a per-connection knob.
- A raw member returns an `int` status matched against the result codes; `SQLITE_BUSY` and `SQLITE_LOCKED` are retry receipts, never faults.
- A status receipt widens as one row when the interop admits a further `SQLITE_DBSTATUS_*` or `SQLITE_STMTSTATUS_*` constant.
- A `utf8z` return is an unmaterialized span over engine memory; `utf8_to_string()` copies it at the boundary.

[STACKING]:
- `api-sqlite`(`.api/api-sqlite.md`): `SqliteConnection.Handle` hands the `sqlite3` every call here takes; the paged `sqlite3_backup_*` session subsumes `BackupDatabase` by adding `_remaining`/`_pagecount` progress facts, and one `sqlite3_blob_*` cursor scans a rowid sequence where the ADO blob stream allocates per cell.
- `api-sqlitepclmc`(`.api/api-sqlitepclmc.md`): its cipher provider swaps in under this same surface, so the encrypted floor inherits every call here and adds only the keying delta.
- `Store/provisioning#EMBEDDED_FLOOR` folds the open ritual through the int-flag `sqlite3_db_config` overload — `SQLITE_DBCONFIG_DEFENSIVE` armed, `SQLITE_DBCONFIG_DQS_DDL`/`DQS_DML` cleared, the loader op absent — once per physical open before any user statement, so defensive posture and double-quoted-literal rejection are connection policy rather than connection-string knobs.
- `Store/provisioning#ENGINE_OPERATIONS` brackets a consistent multi-transaction read: `sqlite3_snapshot_get`, one `sqlite3_snapshot_recover` retry on a refused pin, a `sqlite3_snapshot_cmp` monotonic-floor guard so a reader never regresses across brackets, `sqlite3_snapshot_open`, and `sqlite3_snapshot_free` of only a held handle; the `sqlite3_wal_checkpoint_v2` out-params carry log-frame and checkpointed-frame counts into the typed `EmbeddedFact`, and a `SQLITE_BUSY` return receipts a retry.
- `Store/observability#SQLITE_STATUS_HARVEST` arms the statement registry at open, walks `sqlite3_next_stmt` over the shared handle, and folds the read-and-reset `sqlite3_stmt_status` counters with the `sqlite3_db_status` gauges into the `store.stat.sqlite.statements`/`store.stat.sqlite.connection` receipts over that one native connection.
- `sqlite3_serialize`/`sqlite3_deserialize` move a whole-schema image between memory and a store without file IO, the snapshot rail's path for a memory-backed image distinct from the `byte[]` content-chunk frame.

[LOCAL_ADMISSION]:
- `Batteries_V2.Init()` runs on the store-profile open path, explicit and idempotent.
- Deployment copies only the native asset matching the selected runtime identifier, and the bridge target trims to the single consumer RID.
- Extension loading arms per deployment through an explicit `sqlite3_db_config` op absent from the open ritual's set.
- Provider-bundle facts stay on the store-profile rail and never define public Persistence vocabulary.

[RAIL_LAW]:
- Package: `SQLitePCLRaw.bundle_e_sqlite3`
- Owns: native provider admission and the `SQLitePCL.raw` engine calls the ADO transport omits
- Accept: backup, snapshot, checkpoint, `db_config`, limit, serialize, blob-cursor, status, hook, and authorizer calls over `SqliteConnection.Handle`
- Reject: a hand-rolled P/Invoke or second native binding beside the bundled provider; a whole-file copy where the paged backup session carries progress
