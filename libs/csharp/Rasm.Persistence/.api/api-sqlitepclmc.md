# [RASM_PERSISTENCE_API_SQLITEPCLMC]

`SQLitePCLRaw.bundle_e_sqlite3mc` binds the `e_sqlite3mc` native provider — SQLite carrying the SQLite3 Multiple Ciphers codec — as the encrypted embedded store floor. Swapping the native beneath the shared `SQLitePCL.raw` surface leaves every engine-operations call untouched, so the delta is at-rest keying: a KMS-unwrapped data key crosses `raw.sqlite3_key` at the physical open, and the cipher scheme and its cost parameters ride the engine's own pragma surface.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `SQLitePCLRaw.bundle_e_sqlite3mc`
- package: `SQLitePCLRaw.bundle_e_sqlite3mc` (`Apache-2.0`, SourceGear, LLC)
- assembly: `SQLitePCLRaw.batteries_v2` — the bundle mints its own initializer; `SQLitePCLRaw.core` carries `raw`
- namespace: `SQLitePCL`
- depends: `SQLitePCLRaw.provider.e_sqlite3mc` (P/Invoke), `SQLitePCLRaw.provider.dynamic_cdecl` (cdecl binding), `SQLitePCLRaw.lib.e_sqlite3mc` (native assets), `SQLitePCLRaw.core` (`raw` API, `sqlite3*` handles)
- abi: `libe_sqlite3mc.dylib` / `libe_sqlite3mc.so` / `e_sqlite3mc.dll`, one asset per RID under the restored `runtimes/<RID>/native/` graph
- rail: store-provider (encrypted floor)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: cipher-provider types the bundle adds over the shared `SQLitePCL.raw` surface

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY] | [CAPABILITY]                                              |
| :-----: | :---------------------------- | :------------ | :-------------------------------------------------------- |
|  [01]   | `Batteries_V2`                | static class  | binds `SQLite3Provider_e_sqlite3mc` through `raw`         |
|  [02]   | `Batteries`                   | static class  | facade over `Batteries_V2`                                |
|  [03]   | `SQLite3Provider_e_sqlite3mc` | class         | `ISQLite3Provider` cipher-enabled P/Invoke implementation |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: provider binding — one bundle binds per process, ahead of every `sqlite3_*` call

| [INDEX] | [SURFACE]                              | [SHAPE] | [CAPABILITY]                                        |
| :-----: | :------------------------------------- | :------ | :-------------------------------------------------- |
|  [01]   | `Batteries_V2.Init()`                  | static  | binds the cipher provider through `raw.SetProvider` |
|  [02]   | `Batteries.Init()`                     | static  | forwards to `Batteries_V2.Init()`                   |
|  [03]   | `raw.SetProvider(ISQLite3Provider)`    | static  | binds a provider instance explicitly                |
|  [04]   | `raw.GetNativeLibraryName() -> string` | static  | resolved `e_sqlite3mc` basename                     |

[ENTRYPOINT_SCOPE]: cipher keying — `SQLitePCL.raw` statics over the `SqliteConnection.Handle` (`SQLitePCL.sqlite3`) bridge

| [INDEX] | [SURFACE]                                                         | [SHAPE] | [CAPABILITY]                     |
| :-----: | :---------------------------------------------------------------- | :------ | :------------------------------- |
|  [01]   | `raw.sqlite3_key(sqlite3, ReadOnlySpan<byte>) -> int`             | static  | key the `main` schema            |
|  [02]   | `raw.sqlite3_key_v2(sqlite3, utf8z, ReadOnlySpan<byte>) -> int`   | static  | key a named or attached schema   |
|  [03]   | `raw.sqlite3_rekey(sqlite3, ReadOnlySpan<byte>) -> int`           | static  | rekey the `main` schema          |
|  [04]   | `raw.sqlite3_rekey_v2(sqlite3, utf8z, ReadOnlySpan<byte>) -> int` | static  | rekey a named or attached schema |

- `raw.sqlite3_rekey`: an empty key strips the codec, rewriting the store as plaintext.

[ENTRYPOINT_SCOPE]: cipher parameters — SQL text the bound engine answers, unreachable from `SQLitePCL.raw`

| [INDEX] | [SURFACE]                       | [SHAPE]  | [CAPABILITY]                                        |
| :-----: | :------------------------------ | :------- | :-------------------------------------------------- |
|  [01]   | `PRAGMA cipher`                 | pragma   | reads or selects the scheme; `chacha20` binds unset |
|  [02]   | `PRAGMA kdf_iter`               | pragma   | KDF round count for the bound scheme                |
|  [03]   | `PRAGMA hmac_check`             | pragma   | per-page HMAC verification on read                  |
|  [04]   | `PRAGMA plaintext_header_size`  | pragma   | leading header bytes left unencrypted               |
|  [05]   | `PRAGMA legacy`                 | pragma   | on-disk variant selector for a foreign store        |
|  [06]   | `sqlite3mc_version()`           | function | identifies the bound Multiple Ciphers engine        |
|  [07]   | `sqlite3mc_config(name, value)` | function | reads or sets one engine-side cipher parameter      |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- One provider binds per process, so the encrypted floor is a provisioning-row selection, never a per-connection knob.
- Key material crosses at the first physical open ahead of any user statement, and every later raw call rides the same keyed handle.

[STACKING]:
- `Microsoft.Data.Sqlite`(`.api/api-sqlite.md`): `SqliteConnection.Handle` (`SQLitePCL.sqlite3`) is the one bridge every keying call crosses, and its `Password=` builder key opens an inspected foreign store for a single ephemeral read.
- `SQLitePCLRaw.bundle_e_sqlite3`(`.api/api-sqlitepcl.md`): the backup, snapshot, WAL-checkpoint, db_config, and serialize raw surface carries over this provider unchanged, and its `[RAW_CONSTANTS]` codes match every keying status `int`.
- `Store/provisioning#EMBEDDED_FLOOR`: keys the floor with `raw.sqlite3_key` over the DEK `Element/identity#KMS_CUSTODY` unwraps, rotates through `raw.sqlite3_rekey` after a fresh mint, and dials the scheme and KDF cost with the pragma rows inside the one open ritual.

[LOCAL_ADMISSION]:
- Keying material is a `ReadOnlySpan<byte>` from the KMS tier; a compatibility passphrase lives inside one ephemeral open request and reaches no durable configuration, pooled connection string, receipt, log, or store metadata.
- Encrypted-floor mounts bind this bundle over the plain one, chosen by the `Store/provisioning` row that owns the offline store.

[RAIL_LAW]:
- Package: `SQLitePCLRaw.bundle_e_sqlite3mc`
- Owns: encrypted-at-rest provider admission; `raw.sqlite3_key`/`sqlite3_rekey` reach a live codec only under this native engine
- Accept: KMS-custodied keying and rotation over `Handle`; engine-side cipher parameters through the pragma surface; one ephemeral compatibility read of an inspected foreign store
- Reject: a durable passphrase literal standing in for the KMS-unwrapped DEK; a second provider bound in the same process
