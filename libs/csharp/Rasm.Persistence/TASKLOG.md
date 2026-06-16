# [PERSISTENCE_TASKLOG]

Open work owned by this folder; closed items do not appear. Each row carries its owner, the `page#cluster` it gates, and the named environment that closes it. The tier-2 probe battery (Redis 7.4.9, MinIO/Azurite/fake-gcs, PG18.4 + 12 extensions on paradedb + timescaledb-ha) closed every owner whose proof is a live emulator/server roundtrip; the residue below is the native-SQLite host-load class, the live logical-replication/pgaudit-session class, and the unified-image packaging build — each blocked only by a host/asset/build this single-RID arm64-mac console cannot supply, never a capability or member-shape gap.

## [1]-[NATIVE_SQLITE_HOST_LOAD]

Native-SQLite extension load + at-rest encryption + multi-process file coordination — the e_sqlite3 host-load class the console probe cannot exercise; the owner member shape is fence-complete (`native-sqlite#EXTENSION_GATES`, `store-profiles#CROSS_PROCESS_LAW` SPIKE on the DENSITY_BAR).

| [INDEX] | [OWNER] | [PAGE#CLUSTER] | [EXIT] |
| :-----: | ------ | ------ | ------ |
| [1] | `ExtensionGate` | native-sqlite#EXTENSION_GATES | vec0 sourcing route chosen (package payload vs vendored tarball) and live load verified on osx-arm64 |
| [2] | `EncryptionGate` | native-sqlite#EXTENSION_GATES | SQLCipher provider route with the external dylib confirmed on the target RID |
| [3] | `ExtensionGate` | native-sqlite#EXTENSION_GATES | sqlean per-RID artifacts land as build content per RID |
| [4] | `StoreLeaseRow` | store-profiles#CROSS_PROCESS_LAW | two-process first-open race (racing `MigrateAsync` + busy_timeout, one WAL file) outcome documented; WAL-lock handling and `Claim` arbitration confirmed |
| [5] | `TabularExportSpec` | data-lanes#ANALYTICAL_LANE | DuckDB `sqlite_scanner` ATTACH snapshot visibility under a concurrent WAL writer confirmed |
| [6] | `StoreLifecycle` | store-profiles#STORE_LIFECYCLE | APFS rename durability without directory fsync confirmed; migration-lock holder evidence captured |

## [2]-[LIVE_REPLICATION_AND_AUDIT]

The live PG18 logical-replication decode-fold and pgaudit session-audit class — extensions were `CREATE EXTENSION`'d in the battery but the running-stream and session-category semantics were not exercised; the owners are fence-complete (`sync-collaboration#TRANSPORT_AXIS`, `redaction-retention#AUDIT_BINDING` SPIKE on the DENSITY_BAR) and gate on a live PG18 root with a configured publication/subscription.

| [INDEX] | [OWNER] | [PAGE#CLUSTER] | [EXIT] |
| :-----: | ------ | ------ | ------ |
| [1] | `SyncTransport` | sync-collaboration#TRANSPORT_AXIS | live pgoutput stream emits the catalogued leaf set under `PgOutputProtocolVersion.V4` + `PgOutputStreamingMode.Parallel`; `publish_generated_columns`, idle-slot timeout, and subscription conflict-stat columns recorded; `SetReplicationStatus`/`SendStatusUpdate` LSN ack against the live slot ([LIVE_REPLICATION]) |
| [2] | `AuditBinding` | redaction-retention#AUDIT_BINDING | pgaudit session-audit category semantics under `shared_preload_libraries=pgaudit` against the `Categories` rows, and the per-tenant `BindTenant` category emission verified against a per-tenant `CREATE POLICY` on a live PG18 server ([PGAUDIT_CATEGORIES]) |

## [3]-[UNIFIED_IMAGE_PACKAGING]

No single off-the-shelf image carries the full admitted extension set (pg_search is AGPL pgrx shipping in paradedb; TSL timescaledb ships in timescaledb-ha; both halves PROVED on their respective images). The capability is FINALIZED; only the packaging build is open — a Gate-2C/implementation deliverable, never a capability or probe gap on `server-tier#CLUSTER_CONFIG`.

| [INDEX] | [OWNER] | [PAGE#CLUSTER] | [EXIT] |
| :-----: | ------ | ------ | ------ |
| [1] | `ClusterConfig` | server-tier#CLUSTER_CONFIG | custom Dockerfile `FROM timescale/timescaledb-ha:pg18` + apt partman/pgaudit/squeeze + build/copy pg_search + pgvectorscale, with the full `shared_preload_libraries` list, builds and boots; the `io_method=io_uring` vs `worker` triple's deploy-image kernel value observed in `pg_settings` |

## [4]-[HOST_BRIDGE]

| [INDEX] | [OWNER] | [PAGE#CLUSTER] | [EXIT] |
| :-----: | ------ | ------ | ------ |
| [1] | `ExtensionGate` | native-sqlite#EXTENSION_GATES | hardened-runtime dlopen of extension dylibs inside the signed Rhino host (`scenarios/extension-load.verify.csx`) succeeds; verify script passes |
