# [RASM_PERSISTENCE_ROADMAP]

`Rasm.Persistence` implementation starts from a manifest-backed store package and proceeds through one store, source, snapshot, redaction, and receipt rail.

## [1]-[CURRENT_POSITION]

| [INDEX] | [SURFACE]         | [STATE]                              |
| :-----: | :---------------- | :----------------------------------- |
|   [1]   | Project graph     | solution node present                |
|   [2]   | Package graph     | store and snapshot packages admitted |
|   [3]   | Production source | absent                               |
|   [4]   | API catalogues    | package lookup pages maintained      |
|   [5]   | Host references   | none                                 |

## [2]-[IMPLEMENTATION_TASKS]

[PERSISTENCE_FOLDER_ARCHITECTURE]:
- Status: QUEUED
- Exit: owner folders, rail entrypoints, generated shapes, store profiles, query shapes, snapshot codecs,
  redaction classes, receipts, retention policies, and boundaries are planned before production source.
- Proof: architecture plan consumes every Persistence package API catalogue and names the store rail owners.

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

## [3]-[CATALOGUE_USE]

[STORE_CATALOGUES]:
- Status: REQUIRED
- Action: store design consumes EF SQLite, raw SQLite, SQLitePCL, Npgsql, and EF PostgreSQL catalogues.
- Exit: store owners name profile axes, provider admission, schema gates, migration ownership, and open receipts.

[SCHEMA_CATALOGUES]:
- Status: REQUIRED
- Action: schema design consumes EF design and naming-convention catalogues before source shape selection.
- Exit: schema owners name migration source, naming policy, downgrade guard, and compiled-model boundaries.

[SNAPSHOT_CATALOGUES]:
- Status: REQUIRED
- Action: snapshot design consumes JSON, MessagePack, MessagePack analyzer, hashing, and LZ4 catalogues.
- Exit: snapshot owners name codec profile, generated formatter seams, hash policy, compression policy, and receipts.

[SUPPORT_CATALOGUES]:
- Status: REQUIRED
- Action: support design consumes redaction and NodaTime catalogues before support export design.
- Exit: support owners name classification, redactor policy, retention policy, semantic time, and export receipts.
