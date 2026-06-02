# [H1][RASM_PERSISTENCE_ROADMAP]
>**Dictum:** *First prove one local store lifecycle.*

<br>

`Rasm.Persistence` remains documentation-only. This roadmap sequences future implementation; it does not authorize package pins or source creation by itself.

---
## [1][READINESS]
>**Dictum:** *Persistence starts with a concrete durable record.*

<br>

Start implementation only when a concrete plugin/app needs local durable state outside solve paths.

Required decisions before source:

- First record class: preset, session, cache metadata, or support artifact.
- Store path owner and host/profile scope.
- Schema version source and migration policy.
- Query algebra shape and receipt taxonomy.
- Candidate package versions refreshed from latest stable source.

---
## [2][FIRST_SLICE]
>**Dictum:** *Open, migrate, query, close comes before snapshots and caches.*

<br>

First slice: one SQLite-local schema with open, migrate, query, write, close, and downgrade rejection receipts.

| [INDEX] | [LANDS] | [DEFERRED] |
| :-----: | ------- | ---------- |
|   [1]   | Store profile | Multiple stores/profiles |
|   [2]   | One schema version | Migration families |
|   [3]   | One query algebra case | Repository breadth |
|   [4]   | Open/migrate/query/close receipts | Support bundle export |
|   [5]   | Downgrade rejection | MessagePack snapshots |

---
## [3][DONE_WHEN]
>**Dictum:** *A store slice is done when lifecycle and failure are explicit.*

<br>

The first slice is complete when owner-local receipts identify database path, schema version, migration result, query result, close/dispose path, downgrade rejection, and lock/corrupt failure behavior. Runtime claims remain scoped to the proven local store.

---
## [4][DEFERRED_UNTIL]
>**Dictum:** *Storage breadth waits for real payload pressure.*

<br>

| [INDEX] | [DEFERRED] | [UNBLOCKS_WHEN] |
| :-----: | ---------- | --------------- |
|   [1]   | MessagePack snapshots | JSON baseline proves size/perf problem |
|   [2]   | Support bundle export | AppHost emits real support collection |
|   [3]   | Cache invalidation | Real cache key/version policy exists |
|   [4]   | Raw Microsoft.Data.Sqlite bypass | EF path is too heavy for a proven slice |
|   [5]   | Companion-service database | A separate process/service exists |
