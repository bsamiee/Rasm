# [H1][RASM_APPHOST_ROADMAP]
>**Dictum:** *Start with lifecycle receipts, not infrastructure breadth.*

<br>

`Rasm.AppHost` remains documentation-only. This roadmap sequences future implementation; it does not authorize package pins or source creation by itself.

---
## [1][READINESS]
>**Dictum:** *A runtime package starts when one host profile needs orchestration.*

<br>

Start implementation only when a concrete plugin/app needs startup, drain, cancellation, status correlation, or scheduled background work that cannot stay local to an existing owner.

Required decisions before source:

- In-process plugin runtime record or companion Generic Host.
- First runtime operation and receipt.
- Cancellation and drain policy.
- Whether Channels are enough for first flow.
- Candidate package versions refreshed from latest stable source.

---
## [2][FIRST_SLICE]
>**Dictum:** *Lifecycle proof precedes jobs, telemetry exporters, and dataflow graphs.*

<br>

First slice: one runtime profile with boot, drain, cancellation, and lifecycle receipts.

| [INDEX] | [LANDS] | [DEFERRED] |
| :-----: | ------- | ---------- |
|   [1]   | Runtime profile data | Generic Host bootstrap |
|   [2]   | Boot/drain operation | Scan/decorate topology |
|   [3]   | Cancellation/fault receipt | Telemetry exporters |
|   [4]   | Optional bounded Channel | Dataflow graph |

---
## [3][DONE_WHEN]
>**Dictum:** *A host slice is done when it proves lifecycle behavior.*

<br>

The first slice is complete when owner-local receipts identify startup, drain, cancellation, fault propagation, scope disposal, and host profile. Runtime claims remain scoped to the proven profile.

---
## [4][DEFERRED_UNTIL]
>**Dictum:** *Host packages wait for real host pressure.*

<br>

| [INDEX] | [DEFERRED] | [UNBLOCKS_WHEN] |
| :-----: | ---------- | --------------- |
|   [1]   | Generic Host / DI abstractions | Companion/test/bridge process exists |
|   [2]   | Scrutor | Many registrations or decorators exist |
|   [3]   | Serilog / OpenTelemetry | Support or telemetry sink exists |
|   [4]   | HTTP resilience | Typed outbound HTTP service exists |
|   [5]   | FluentValidation | External DTO/config boundary exists |
|   [6]   | NodaTime | Persisted/audited semantic time exists |
|   [7]   | Dataflow | Channels or single queue prove insufficient |
