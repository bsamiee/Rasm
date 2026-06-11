# [RASM_PERSISTENCE]

`Rasm.Persistence` is the durable-state and source-profile package for app packages.
It owns store profiles, query shapes, provider stores, schema state, migrations,
snapshots, codecs, compression, cache metadata, benchmark indexes, support
artifacts, redaction, retention, and AppHost drain integration as one store rail.

## [1]-[PURPOSE]

Persistence exposes durable-state operations through typed store, query, snapshot,
projection, and receipt algebras. SQLite/EF, PostgreSQL/provider stores, JSON
snapshots, MessagePack snapshots, file snapshots, cache metadata, benchmark
indexes, companion stores, and support bundles enter the same source rail.

It is not an EF wrapper, repository family, serializer wrapper, provider service set, GH solve-path cache, Rhino settings wrapper, or domain model replacement.

## [2]-[STACK_DOCTRINE]

| [INDEX] | [READ_FOR]        | [OPEN]                                            |
| :-----: | :---------------- | :------------------------------------------------ |
|   [1]   | C# package law    | [C# stack](../../../docs/stacks/csharp/README.md) |
|   [2]   | package API facts | [.reports/api](.reports/api/README.md)            |

Implementation planning follows the C# stack atlas and finalized concept pages. Package-local documents state Persistence law and package API facts.

## [3]-[STATUS]

| [INDEX] | [SURFACE]         | [STATE]                                       |
| :-----: | :---------------- | :-------------------------------------------- |
|   [1]   | Project file      | present in `Workspace.slnx`                   |
|   [2]   | Package manifest  | store, provider, snapshot, redaction admitted |
|   [3]   | Project contracts | AppHost                                       |
|   [4]   | Lockfile          | restored package closure tracked              |
|   [5]   | Production source | absent                                        |
|   [6]   | Package law       | documented in this folder                     |

## [4]-[DOCUMENTS]

| [INDEX] | [READ_FOR]              | [OPEN]                                 |
| :-----: | :---------------------- | :------------------------------------- |
|   [1]   | current structure       | [architecture](ARCHITECTURE.md)        |
|   [2]   | implementation sequence | [roadmap](ROADMAP.md)                  |
|   [3]   | package API catalogues  | [.reports/api](.reports/api/README.md) |

## [5]-[CONSTRAINTS]

- Persistence is RhinoCommon-free. App roots resolve host profile and path values before calling Persistence.
- Store logic enters through one lifecycle/query dispatch rail. Repository families, ad hoc stores, and per-entity service sets are rejected.
- SQLite/EF, PostgreSQL/provider stores, JSON snapshots, MessagePack snapshots, file snapshots, cache metadata,
  benchmark indexes, support bundles, and companion stores are store-profile or snapshot-profile cases.
- Public concepts are store profile, entity kind, query shape, operation state, projection state,
  snapshot codec, compression policy, redaction class, retention policy, and receipt evidence.
- `DbContext` is operation-scoped and disposed through the store rail.
- Native SQLite, PRAGMA policy, schema gates, and migration locks are proven at open time before normal operations run.
- Snapshot codecs and compression carry round-trip, size, hash, and compatibility receipts.
- Redaction is part of support export. Classification, redactor registration, redacted output proof, and failure receipts ship with support bundles.
- No store operation runs in GH solve hot paths.
