# [H1][RASM_COMPUTE]
>**Dictum:** *Compute extends measured execution; Rasm owns computational meaning.*

<br>

`Rasm.Compute` is the planned measured-compute platform for future long-running, tensor, model, and remote execution lanes. It is docs-only today: no `.csproj`, no production C# files, no active package references, and no runtime surface.

---
## [1][PURPOSE]
>**Dictum:** *Compute starts from existing Rasm numerics.*

<br>

`Rasm.Compute` will coordinate substrate selection, cancellation/progress, benchmark receipts, model lifecycle, remote dispatch, and failure taxonomy for work that outgrows direct `Rasm` operations. Existing `Rasm` + MathNet + CSparse algorithms remain the default substrate.

It is not a tensor wrapper, ML.NET wrapper, gRPC wrapper, job framework, acceleration claim, or replacement for existing `Rasm` numerics.

---
## [2][STATUS]
>**Dictum:** *No source means no acceleration claim.*

<br>

| [INDEX] | [SURFACE] | [STATE] |
| :-----: | --------- | ------- |
|   [1]   | Project file | Not created |
|   [2]   | Production API | Not created |
|   [3]   | Package references | None |
|   [4]   | Compute substrate | Rasm/MathNet/CSparse first |
|   [5]   | Performance proof | Pending future implementation |

Candidate package versions must be refreshed from latest stable sources immediately before a real consumer lands.

---
## [3][MANUAL]
>**Dictum:** *Read Compute before adding tensor, model, or remote execution packages.*

<br>

| [INDEX] | [FILE] | [READ_FOR] |
| :-----: | ------ | ---------- |
|   [1]   | `_ARCHITECTURE.md` | Substrate selection, candidates, failure model, benchmark proof |
|   [2]   | `AGENTS.md` | Future-agent rules and package rejections |
|   [3]   | `ROADMAP.md` | First measured slice and deferred candidates |

---
## [4][NON_CLAIMS]
>**Dictum:** *Compute docs do not imply speed.*

<br>

- No package is active.
- No tensor/model/remote rail exists.
- No performance improvement is claimed.
- No native asset behavior is proven.
