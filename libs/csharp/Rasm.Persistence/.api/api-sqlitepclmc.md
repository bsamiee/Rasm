# [RASM_PERSISTENCE_API_SQLITEPCLMC]

`SQLitePCLRaw.bundle_e_sqlite3mc` admits the `e_sqlite3mc` native provider — SQLite compiled with the
SQLite3 Multiple Ciphers extension — for the encrypted embedded store profile. It swaps only the
native provider under the same `SQLitePCL.raw` surface `api-sqlitepcl` documents: `Store/provisioning`
opens the encrypted floor over `SQLite3Provider_e_sqlite3mc` and keys it through `raw.sqlite3_key`
from the data key `Element/identity#KMS_CUSTODY` unwraps, so the backup, snapshot, WAL, db_config,
and serialize raw calls carry over unchanged and only the at-rest cipher and keying calls are new.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `SQLitePCLRaw.bundle_e_sqlite3mc`
- package: `SQLitePCLRaw.bundle_e_sqlite3mc`
- license: `MIT` (bundle/provider); native `e_sqlite3mc` layers the public-domain SQLite engine with the SQLite3 Multiple Ciphers extension
- assembly: `SQLitePCLRaw.batteries_v2` — the bundle carries its own `Batteries`/`Batteries_V2` initializer that binds the `mc` provider, distinct from the plain bundle's separate `config.e_sqlite3` package
- runtime assembly carrying `raw`: `SQLitePCLRaw.core`, unified through central transitive pinning
- namespace: `SQLitePCL`
- native engine: `e_sqlite3mc` (`libe_sqlite3mc.dylib` / `libe_sqlite3mc.so` / `e_sqlite3mc.dll`), SQLite with the SQLite3 Multiple Ciphers encryption layer
- rail: store-provider (encrypted floor)

## [02]-[PACKAGE_ASSETS]

[PACKAGE_ASSET_SCOPE]: `mc` bundle dependency graph — the cipher-provider delta over the plain bundle
- rail: store-provider (encrypted floor)

| [INDEX] | [SYMBOL]                              | [PACKAGE_ROLE]      | [CAPABILITY]                                                             |
| :-----: | :------------------------------------ | :------------------ | :----------------------------------------------------------------------- |
|  [01]   | `SQLitePCLRaw.bundle_e_sqlite3mc`     | bundle              | ships `batteries_v2` binding the `mc` provider; pins provider + native   |
|  [02]   | `SQLitePCLRaw.provider.e_sqlite3mc`   | provider dependency | `SQLite3Provider_e_sqlite3mc : ISQLite3Provider` cipher-enabled P/Invoke |
|  [03]   | `SQLitePCLRaw.provider.dynamic_cdecl` | provider glue       | cdecl dynamic-binding path resolving the `mc` native entry points        |
|  [04]   | `SQLitePCLRaw.lib.e_sqlite3mc`        | native dependency   | `e_sqlite3mc` binaries across desktop, mobile, and wasm RIDs             |
|  [05]   | `SQLitePCLRaw.core`                   | core dependency     | shared `SQLitePCL.raw` static API + `sqlite3*` handle types              |

[RID_ABI]: native asset placement
- derive native coverage from the restored `SQLitePCLRaw.lib.e_sqlite3mc` `runtimes/<RID>/native/` asset graph
- deployment copies only the native asset matching the selected runtime identifier
- `raw.GetNativeLibraryName()` returns the resolved `e_sqlite3mc` basename for diagnostics, distinguishing the encrypted floor from the plain `e_sqlite3` provider at runtime

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: cipher-provider initialization — the provider-binding delta over `api-sqlitepcl`
- rail: store-provider (encrypted floor)

| [INDEX] | [SURFACE]                           | [CALL_SHAPE]  | [CAPABILITY]                                                                     |
| :-----: | :---------------------------------- | :------------ | :------------------------------------------------------------------------------- |
|  [01]   | `Batteries_V2.Init()`               | static void   | `raw.SetProvider(new SQLite3Provider_e_sqlite3mc())` — binds the cipher provider |
|  [02]   | `Batteries.Init()`                  | static void   | thin facade forwarding to `Batteries_V2.Init()`                                  |
|  [03]   | `raw.SetProvider(ISQLite3Provider)` | static void   | binds `SQLite3Provider_e_sqlite3mc` explicitly; one bundle per process           |
|  [04]   | `raw.GetNativeLibraryName()`        | static string | resolved `e_sqlite3mc` native basename                                           |

One SQLitePCLRaw provider binds per process — `SQLite3Provider_e_sqlite3mc` and the plain
`SQLite3Provider_e_sqlite3` never coexist on one connection, so an app composes exactly one bundle
and the encrypted floor is a provisioning-row selection, not a per-connection knob.

[ENTRYPOINT_SCOPE]: keying surface — `SQLitePCL.raw` cipher members over `SqliteConnection.Handle`
- rail: store-provider (encrypted floor)

Keying reaches through the same `SqliteConnection.Handle` (`SQLitePCL.sqlite3`) bridge the backup and
snapshot calls use, applied once per physical open before any user statement. Passphrase bytes derive
from the DEK `Element/identity#KMS_CUSTODY`'s `EnvelopeKeyring.Unwrap` recovers; a status `int` matches the
`api-sqlitepcl` `[RAW_CONSTANTS]` codes.

[KEYING_SIGNATURES]: decompile-verified `SQLitePCL.raw` cipher members (`SQLitePCLRaw.core`)
- rail: store-provider (encrypted floor)

```csharp generated
// --- key application (open a keyed database; k is the raw passphrase/key material) ---
int sqlite3_key(sqlite3 db, ReadOnlySpan<byte> k)                        // key the "main" schema
int sqlite3_key_v2(sqlite3 db, utf8z name, ReadOnlySpan<byte> k)         // key a named/attached schema

// --- rekey (change or strip the key on an open database; k empty decrypts to plaintext) ---
int sqlite3_rekey(sqlite3 db, ReadOnlySpan<byte> k)                      // rekey the "main" schema
int sqlite3_rekey_v2(sqlite3 db, utf8z name, ReadOnlySpan<byte> k)       // rekey a named/attached schema
```

## [04]-[CIPHER_SURFACE]

[CIPHER_DISCOVERY]: bound-engine inspection
- derive cipher selection from the bound engine's `PRAGMA cipher` surface and the foreign store's inspected metadata
- `select sqlite3mc_version()` identifies the bound Multiple Ciphers engine without freezing its scheme vocabulary in prose

[COMPATIBILITY_INPUT]: foreign encrypted stores
- `Password=` is a compatibility-only input for opening an inspected foreign store; its value exists only in the ephemeral open request
- compatibility passphrases never enter durable configuration, pooled connection strings, receipts, logs, or persisted store metadata
- encrypted-floor creation and rotation use KMS-unwrapped bytes through `raw.sqlite3_key` and `raw.sqlite3_rekey`

## [05]-[IMPLEMENTATION_LAW]

[NATIVE_ADMISSION]:
- package role: encrypted SQLite native provider admission over the shared `SQLitePCL.raw` API
- runtime root: `e_sqlite3mc` cipher provider (`SQLite3Provider_e_sqlite3mc`)
- initializer root: `Batteries_V2.Init()` (or the `Batteries.Init()` facade) binding the `mc` provider
- store root: encrypted embedded SQLite profile only

[INTEGRATION_STACK]:
- `Store/provisioning#EMBEDDED_FLOOR` keys the encrypted floor once per physical open: `raw.sqlite3_key(connection.Handle, dek)` over the `SqliteConnection.Handle` bridge, where `dek` is the `ReadOnlySpan<byte>` the `Element/identity#KMS_CUSTODY` `EnvelopeKeyring.Unwrap` recovers from the wrapped data key — no passphrase persists past the keyed open
- `Store/provisioning#EMBEDDED_FLOOR` rotates a store key without re-encrypting through the app layer: `raw.sqlite3_rekey(Handle, newDek)` after a fresh `Element/identity#KMS_CUSTODY` mint, so a key-rotation policy is a single raw call on the open connection, and an empty `newDek` strips the cipher for a plaintext export
- Foreign encrypted-store reads derive their ephemeral connection parameters from inspected input metadata; compatibility passphrases disappear after the physical open
- `api-sqlitepcl` `[RAW_SIGNATURES]` backup, snapshot, WAL-checkpoint, db_config, and serialize/deserialize calls all carry over the `mc` provider unchanged — the encrypted floor inherits the full engine-operations surface and adds only the keying delta above

[LOCAL_ADMISSION]:
- `mc` bundle supersedes `SQLitePCLRaw.bundle_e_sqlite3` where the encrypted floor mounts; both admit in the manifest, but one process binds one bundle, and the `Store/provisioning` row selects the encrypted provider for the offline store
- keying material stays a `ReadOnlySpan<byte>` sourced from the KMS tier; compatibility passphrase strings exist only for the physical open of inspected foreign input and persist nowhere
- provider-bundle facts stay in the encrypted-floor store-profile rail and never define public Persistence vocabulary

[RAIL_LAW]:
- Package: `SQLitePCLRaw.bundle_e_sqlite3mc`
- Owns: encrypted SQLite native provider admission + the `sqlite3_key`/`sqlite3_rekey` cipher-keying calls the plain bundle omits
- Accept: encrypted embedded runtime; KMS-custodied `sqlite3_key`/`rekey` over `Handle`; ephemeral compatibility reads over inspected foreign input
- Reject: coexistence with the plain `e_sqlite3` provider on one connection; a durable passphrase literal in place of the KMS-unwrapped DEK
