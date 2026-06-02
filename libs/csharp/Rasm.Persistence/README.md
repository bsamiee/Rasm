# [H1][RASM_PERSISTENCE]
>**Dictum:** *Persistence stores durable app state outside solve hot paths.*

<br>

`Rasm.Persistence` is the planned local durable-state platform for future Rasm plugins and apps. It is docs-only today: no `.csproj`, no production C# files, no active package references, and no runtime surface.

---
## [1][PURPOSE]
>**Dictum:** *Durability is an effectful shell, not domain logic.*

<br>

`Rasm.Persistence` will own local SQLite-backed app state, migrations, queries, presets, snapshots, caches, support artifacts, redaction, export receipts, and cleanup. AppHost may schedule durable work; Persistence owns open, migrate, query, transaction, dispose, and storage receipts.

It is not a repository wrapper, EF wrapper, serializer wrapper, solve-path cache, GH tree store, or domain model replacement.

---
## [2][STATUS]
>**Dictum:** *No project means no storage package is active.*

<br>

| [INDEX] | [SURFACE] | [STATE] |
| :-----: | --------- | ------- |
|   [1]   | Project file | Not created |
|   [2]   | Production API | Not created |
|   [3]   | Package references | None |
|   [4]   | Local store | Planned |
|   [5]   | Solve-path behavior | Forbidden |

Candidate package versions must be refreshed from latest stable sources immediately before a real consumer lands.

---
## [3][MANUAL]
>**Dictum:** *Read Persistence before adding storage concerns.*

<br>

| [INDEX] | [FILE] | [READ_FOR] |
| :-----: | ------ | ---------- |
|   [1]   | `_ARCHITECTURE.md` | Store rail, provider split, query algebra, migration policy |
|   [2]   | `AGENTS.md` | Future-agent rules and hot-path rejections |
|   [3]   | `ROADMAP.md` | First local-store slice and deferred candidates |

---
## [4][NON_CLAIMS]
>**Dictum:** *Storage intent does not reserve API.*

<br>

- No public API shape is reserved.
- No database schema exists.
- No EF/SQLite package is active.
- No support-bundle format exists.
- No persistence code may run inside GH solve hot paths.
