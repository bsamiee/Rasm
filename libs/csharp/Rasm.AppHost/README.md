# [H1][RASM_APPHOST]
>**Dictum:** *AppHost orchestrates runtime; domain owners execute domain work.*

<br>

`Rasm.AppHost` is the planned runtime/composition platform for future Rasm plugins, apps, companion processes, and bridge-like services. It is docs-only today: no `.csproj`, no production C# files, no active package references, and no runtime surface.

---
## [1][PURPOSE]
>**Dictum:** *Runtime orchestration is not business logic.*

<br>

`Rasm.AppHost` will own runtime profiles, startup/drain/unload, scheduling, bounded in-process flow, telemetry correlation, external-hop policy, and lifecycle receipts. It will coordinate AppUi, Persistence, Compute, Rhino, and GH2 without importing their implementation concerns.

It is not a domain service layer, job framework, DI wrapper, telemetry wrapper, or catch-all runtime package.

---
## [2][STATUS]
>**Dictum:** *No project means no active host package.*

<br>

| [INDEX] | [SURFACE] | [STATE] |
| :-----: | --------- | ------- |
|   [1]   | Project file | Not created |
|   [2]   | Production API | Not created |
|   [3]   | Package references | None |
|   [4]   | Generic Host mode | Candidate only |
|   [5]   | In-process runtime record mode | Planned first mode |

Candidate package versions must be refreshed from latest stable sources immediately before a real consumer lands.

---
## [3][MANUAL]
>**Dictum:** *Read AppHost before adding runtime packages.*

<br>

| [INDEX] | [FILE] | [READ_FOR] |
| :-----: | ------ | ---------- |
|   [1]   | `_ARCHITECTURE.md` | Composition modes, ownership, candidates, flow policy |
|   [2]   | `AGENTS.md` | Future-agent rules and package boundaries |
|   [3]   | `ROADMAP.md` | First runtime slice and deferred host packages |

---
## [4][NON_CLAIMS]
>**Dictum:** *Runtime intent does not activate infrastructure.*

<br>

- No package is active.
- No Generic Host bootstrap exists.
- No DI scan/decorate topology exists.
- No telemetry exporter or HTTP resilience pipeline exists.
- No Dataflow graph exists.
- No AppUi, Persistence, or Compute implementation may be absorbed into AppHost.
