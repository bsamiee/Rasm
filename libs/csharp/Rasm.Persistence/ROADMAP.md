# [RASM_PERSISTENCE_ROADMAP]

`Rasm.Persistence` implementation starts from a manifest-backed store package and proceeds through one store, source, snapshot, redaction, and receipt rail.

## [1]-[CURRENT_POSITION]

This table is a lookup by implementation surface.

| [INDEX] | [SURFACE]         | [STATE]                         |
| :-----: | :---------------- | :------------------------------ |
|   [1]   | Project graph     | solution node present           |
|   [2]   | Package graph     | store and snapshot packages admitted |
|   [3]   | Production source | absent                          |
|   [4]   | API catalogues    | package lookup pages maintained |
|   [5]   | Host references   | none                            |

## [2]-[IMPLEMENTATION_TASKS]

[PERSISTENCE_STORE_PROFILE]:
- Status: QUEUED
- Exit: store profile carries provider, path, scope, schema identity, retention, and host profile inputs.
- Proof: profile admission specs and host-free dependency tests.

[PERSISTENCE_SCHEMA_QUERY]:
- Status: QUEUED
- Exit: lifecycle/query dispatch owns migration, lock, downgrade, integrity, projection, compaction, and drain states.
- Proof: schema lifecycle specs and operation-scoped context tests.

[PERSISTENCE_SNAPSHOT_CODEC]:
- Status: QUEUED
- Exit: JSON, MessagePack, file snapshot, checksum, compression, and compatibility receipts enter one snapshot rail.
- Proof: round-trip, hash, size, and compatibility specs.

[PERSISTENCE_SUPPORT_CACHE]:
- Status: QUEUED
- Exit: redaction, support artifact classification, model-result cache, benchmark index, and retention states enter typed receipts.
- Proof: redaction specs, cache key specs, and support export receipts.

## [3]-[PACKAGE_PROOF]

This table is a lookup by package rail.

| [INDEX] | [RAIL]     | [REQUIRED_STATE]                       |
| :-----: | :--------- | :------------------------------------- |
|   [1]   | Store      | EF SQLite and raw SQLite admitted      |
|   [2]   | Provider   | PostgreSQL provider packages admitted  |
|   [3]   | Snapshots  | JSON, MessagePack, hashing, LZ4        |
|   [4]   | Redaction  | compliance redaction admitted          |
|   [5]   | Functional | rails and generated shapes inherited   |
