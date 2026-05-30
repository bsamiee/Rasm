# [H1][TESTING_LIBS]
>**Dictum:** *Test-tool APIs belong to the test rail, not product dependency policy.*

<br>

[IMPORTANT] Package injection and versions: `Directory.Build.props` + `Directory.Packages.props` §6. Cross-stack proof: `../usage.md` §5.

---
## [1][INDEX]
>**Dictum:** *Each leaf owns one test-tool surface.*

<br>

| [INDEX] | [TOOL]          | [FILE]                   |
| :-----: | --------------- | ------------------------ |
|   [1]   | xUnit v3        | `xunit/api.md`           |
|   [2]   | CsCheck         | `cscheck/api.md`         |
|   [3]   | coverlet        | `coverlet/api.md`        |
|   [4]   | Stryker         | `stryker/api.md`         |
|   [5]   | Verify          | `verify/api.md`          |
|   [6]   | ArchUnitNET     | `archunit/api.md`        |
|   [7]   | BenchmarkDotNet | `benchmarkdotnet/api.md` |
|   [8]   | SharpFuzz       | `sharpfuzz/api.md`       |

---
## [2][RAILS]
>**Dictum:** *Executable rails stay in `tests/csharp/_*` projects.*

<br>

- `_architecture` — ArchUnitNET dependency laws
- `_tooling` — Verify snapshots
- `_benchmarks` — BenchmarkDotNet measurement
- `_fuzz` — SharpFuzz harnesses
- `_testkit` — shared Spec/Gens/Numeric (see `tests/csharp/AGENTS.md`)
