# [H1][PACKAGE_ADOPTION]
>**Dictum:** *Packages enter Rasm only when they own a real concern better than local code.*

<br>

[IMPORTANT] Baseline: NuGet V3 flat-container metadata. Candidate targets are verification anchors, not package declarations. Add central versions only with a concrete consumer.

---
## [1][ADOPT_WITH_CONSUMER]
>**Dictum:** *These packages are approved when the owning boundary is being implemented.*

<br>

| [INDEX] | [PACKAGE] | [TARGET] | [CAPABILITY] | [BOUNDARY] |
| :-----: | --------- | :------: | ------------ | ---------- |
| [1] | `Scrutor` | Verify at adoption | DI scanning and decoration. | Composition roots. |
| [2] | `FluentValidation` | Verify at adoption | Async boundary rule sets. | API boundaries before `Validation<Error,T>`. |
| [3] | `NodaTime` | Verify at adoption | Domain time and clocks. | Runtime metadata and deterministic tests. |
| [4] | `OpenTelemetry.*` | Verify at adoption | Traces, metrics, OTLP export. | Bridge, tools, app hosts. |
| [5] | `Serilog*` | Verify at adoption | Structured host logging. | Composition roots. |
| [6] | `Polly*` | Verify at adoption | Retry, timeout, breakers, hedging. | IO, process, bridge, network. |
| [7] | `Microsoft.Extensions.Options*` | Verify at adoption | Typed config binding and validation. | CLI and bridge config. |
| [8] | `BenchmarkDotNet` | Verify at adoption | Benchmark and allocation proof. | Hot-path validation projects. |

---
## [2][DOCUMENT_ONLY]
>**Dictum:** *Candidate packages stay out of the graph until their first real use.*

<br>

| [INDEX] | [PACKAGE] | [TARGET] | [WHY_WAIT] |
| :-----: | --------- | :------: | ---------- |
| [1] | `System.Numerics.Tensors` | Verify at adoption | Needs measured numeric consumer; MathNet owns matrix algorithms. |
| [2] | `System.IO.Hashing` | Verify at adoption | No non-crypto cache-key consumer. |
| [3] | `CommunityToolkit.HighPerformance` | Verify at adoption | Try BCL spans and pools first. |
| [4] | `Testcontainers.PostgreSql` | Verify at adoption | Add with Postgres/persistence lane. |
| [5] | `Respawn` | Verify at adoption | Pair with integration DB tests. |
| [6] | `dotnet-stryker` | Verify at adoption | Expensive opt-in or nightly gate. |
| [7] | `SonarAnalyzer.CSharp` | Verify at adoption | Trial only with curated severity overlap. |
| [8] | `Npgsql` / `Npgsql.OpenTelemetry` | Verify at adoption | Persistence boundary only. |

---
## [3][REJECT_BY_DEFAULT]
>**Dictum:** *Reject packages that create a second paradigm for an existing Rasm owner.*

<br>

| [INDEX] | [PACKAGE] | [REJECTION_REASON] |
| :-----: | --------- | ------------------ |
| [1] | `NetEscapades.EnumGenerators` | Duplicates Thinktecture smart enums. |
| [2] | `Riok.Mapperly` | Encourages parallel DTO/model surfaces. |
| [3] | `Roslynator.Analyzers` | Overlaps SDK analyzers, Meziantou, and local CSP rules. |
| [4] | `IDisposableAnalyzers` | Prefer current CA and local analyzer coverage. |
| [5] | `CommunityToolkit.Diagnostics` | Guard/throw helpers fight `Fin<T>` and `Validation<Error,T>`. |
| [6] | `Verify.Xunit` | Snapshot baseline does not fit current law-test stack. |
| [7] | `FsCheck` | Duplicates current `CsCheck` PBT stack. |
| [8] | `AutoMapper`, `Moq`, `NSubstitute`, `AutoFixture` | Adds magic mapping, mock pressure, and object bags. |

---
## [4][CENTRALIZATION_RULE]
>**Dictum:** *Central package management is a graph contract, not a wish list.*

<br>

- Add versions only in `Directory.Packages.props`.
- Add no unused `PackageVersion` entries.
- Add no `Version` attribute to project-local `PackageReference`.
- Prefer conditioned central `PackageReference` in `Directory.Build.props` for universal category dependencies.
- Keep RhinoWIP/GH2/Eto assemblies as app-bundle `Reference` items unless local host parity proof says otherwise.
