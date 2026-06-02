# [H1][RASM_APPHOST_AGENTS]
>**Dictum:** *Coordinate runtime; do not absorb owners.*

<br>

[CRITICAL] `Rasm.AppHost` is docs-only until a future implementation explicitly creates a project and production source. Do not add package references, `.csproj`, or C# files from this folder contract alone.

---
## [1][OWNER_CONTRACT]
>**Dictum:** *One runtime rail routes operations and receipts.*

<br>

- Use runtime-record `Eff.runtime<RT>()` as the default Rhino/GH2 plugin composition mode.
- Use Generic Host, `IServiceCollection`, and Scrutor only for companion/test/bridge bootstraps.
- Emit typed lifecycle/status/fault receipts; do not expose service provider resolution as public API.
- Keep package candidates `[NOT_IN_GRAPH]` until a concrete consumer exists.

---
## [2][BOUNDARY_RULES]
>**Dictum:** *One owner per operational concern.*

<br>

| [INDEX] | [CONCERN] | [RULE] |
| :-----: | --------- | ------ |
|   [1]   | UI | Emit status; AppUi renders |
|   [2]   | Persistence | Schedule; Persistence stores |
|   [3]   | Compute | Schedule/drain; Compute executes |
|   [4]   | Retry | LanguageExt `Schedule` or HTTP resilience, never both on one hop |
|   [5]   | Flow | Channels first; Dataflow only after topology proof |
|   [6]   | Time | `TimeProvider` for timers; NodaTime for semantic persisted time |

---
## [3][EVIDENCE]
>**Dictum:** *Docs name evidence categories; source slices produce evidence.*

<br>

Documentation edits require manual consistency only. Executable proof begins when source, package references, or host scenarios land.

Future AppHost evidence categories:

- Startup/drain/unload receipt.
- Cancellation and fault propagation.
- Scope disposal.
- Channel backpressure or Dataflow topology proof.
- Telemetry correlation and support-bundle receipt.
- Outbound HTTP resilience ownership when external services exist.

---
## [4][REJECTIONS]
>**Dictum:** *AppHost is not an infrastructure dumping ground.*

<br>

- No domain logic.
- No AppUi rendering.
- No Persistence `DbContext` ownership.
- No Compute substrate/model execution.
- No Scrutor in in-process plugin hot paths.
- No Dataflow default before Channels are insufficient.
