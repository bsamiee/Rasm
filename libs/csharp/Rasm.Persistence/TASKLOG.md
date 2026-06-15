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

## [2]-[SPEC_PROOFS_AT_IMPLEMENTATION]

| [INDEX] | [ITEM] | [EXIT] |
| :-----: | ------ | ------ |
| [1] | Compile-options receipt + Batteries_V2 round-trip under the bundle-line override; snapshot bracket preconditions | round-trip passes; preconditions enforced by spec |

## [3]-[PLANNING_CLOSE_OUT_SPIKES]

| [INDEX] | [ITEM] | [EXIT] |
| :-----: | ------ | ------ |
| [1] | Hardened-runtime dlopen of extension dylibs inside the signed Rhino host (extension-load.verify.csx) | dlopen succeeds under hardened runtime; verify script passes |
