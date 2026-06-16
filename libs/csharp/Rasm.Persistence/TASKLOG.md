# [PERSISTENCE_TASKLOG]

Open work owned by this folder; closed items do not appear.

## [1]-[NATIVE_AND_SERVER_PROBES]

| [INDEX] | [ITEM] | [EXIT] |
| :-----: | ------ | ------ |
| [1] | vec0 sourcing decision (package payload vs vendored tarball) + live-load proof | sourcing route chosen and live load verified |
| [2] | SQLCipher provider route with external dylib on osx-arm64 | provider route confirmed on target RID |
| [3] | sqlean per-RID vendoring as build content | per-RID artifacts land as build content |
| [4] | MergeWithOutput RETURNING old/new emission on the pg provider | old/new row emission verified against pg provider |
| [5] | Live-PG18 probes: publish_generated_columns, idle slot timeout, subscription conflict stats, pgaudit categories | all four probe results recorded |
| [6] | Two-process first-open race (racing MigrateAsync + busy_timeout, one WAL file) | race outcome documented; WAL-lock handling confirmed |
| [7] | DuckDB sqlite_scanner ATTACH snapshot visibility under a concurrent WAL writer | visibility semantics confirmed under concurrent WAL |
| [8] | uuidv7 double-generation precedence (client CreateVersion7 vs pg column default) | precedence rule grounded; one generation site confirmed |
| [9] | APFS rename durability without directory fsync; migration-lock holder evidence | durability posture confirmed; lock-holder evidence captured |
| [10] | Live-PG18 server-tier: create_hypertable/continuous-aggregate/columnstore apply, diskann-over-halfvec storage_layout, pg_search BM25 @@@, RLS current_setting per-tenant | all server-tier provisioning shapes verified against colima PG18 image |
| [11] | Cluster GUC portability: io_method=io_uring vs worker on the deploy-image kernel | observed pg_settings value confirms io_uring or worker fallback |
| [12] | Migration bundle: self-contained executable + idempotent ScriptMigration + NOT-VALID/NOT-ENFORCED lock-light apply-then-validate | bundle apply and lock-light constraint contract verified |
| [13] | Object-store multipart resume: skip-on-committed-ETag, 412 conditional-write conflict, range-read resume against MinIO/Azurite/fake-gcs-server | resume/conflict/range contract verified per provider |
| [14] | Object-store member spellings + boundary-exception types decompile-verified at admission | get/head/list/delete members + AmazonS3Exception/RequestFailedException/GoogleApiException confirmed |
| [15] | StackExchange.Redis L2 residence roundtrip (RedisCache : IBufferDistributedCache via AddStackExchangeRedisCache) | local redis-server roundtrip + buffer-contract zero-copy path verified |

## [2]-[PLANNING_CLOSE_OUT_SPIKES]

| [INDEX] | [ITEM] | [EXIT] |
| :-----: | ------ | ------ |
| [1] | Hardened-runtime dlopen of extension dylibs inside the signed Rhino host (extension-load.verify.csx) | dlopen succeeds under hardened runtime; verify script passes |
