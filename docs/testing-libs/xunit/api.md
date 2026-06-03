# [H1][XUNIT_API]
>**Dictum:** *xUnit owns MTP discovery; Rasm owns laws and oracles.*

<br>

[IMPORTANT] Rasm uses xUnit v3 through .NET 10 Microsoft Testing Platform. Root `global.json` selects the MTP runner; `Directory.Build.props` injects `xunit.v3.mtp-v2` and generates per-project `obj/xunit.runner.json`. There is no root runner file and no retired adapter.

---
## [1][PACKAGE_MODE]
>**Dictum:** *Runner packages define discovery behavior.*

<br>

| [INDEX] | [PACKAGE]                     | [PIN]                      | [STATE]          | [USE]                                      |
| :-----: | ----------------------------- | -------------------------- | ---------------- | ------------------------------------------ |
|   [1]   | `xunit.v3.mtp-v2`             | `Directory.Packages.props` | Shared           | xUnit v3 MTP discovery and execution       |
|   [2]   | `xunit.v3.assert`             | `Directory.Packages.props` | Shared (testkit) | Assertions and serializer APIs             |
|   [3]   | `xunit.v3.common`             | `Directory.Packages.props` | Shared (testkit) | Common v3 abstractions                     |
|   [4]   | `xunit.v3.extensibility.core` | `Directory.Packages.props` | Shared (testkit) | Pipeline startup and extensibility APIs    |
|   [5]   | `xunit.analyzers`             | lock                       | Transitive       | xUnit analyzer diagnostics through package |

[SOURCE] xUnit MTP docs: https://xunit.net/docs/getting-started/v3/microsoft-testing-platform

---
## [2][DISCOVERY]
>**Dictum:** *Generated tests must remain stable under MTP.*

<br>

| [INDEX] | [SURFACE]                                      | [RASM_USE]                                                  |
| :-----: | ---------------------------------------------- | ----------------------------------------------------------- |
|   [1]   | `[Fact]`, `[Theory]`                           | Own-line attributes; CsCheck facts over large theory tables |
|   [2]   | `TheoryData<T1..T15>`/`TheoryDataRow<T1..T15>` | Small explicit edge matrices only                           |
|   [3]   | `ITheoryDataRow`                               | Row metadata when display/skip/timeout/traits are contract  |
|   [4]   | `MatrixTheoryData<T1..T15>`                    | Rare cartesian rows; prefer `Gen.Select` for broad axes     |
|   [5]   | `preEnumerateTheories`                         | Rasm runner JSON; stable inputs; no runtime-random rows     |

MTP commands use `dotnet test --project <csproj>` or `uv run python -m tools.quality test ...`; positional project paths are invalid in MTP mode.

---
## [3][FIXTURES_CONTEXT]
>**Dictum:** *Use v3 APIs that exist, not v2 folklore.*

<br>

| [INDEX] | [SURFACE]                                  | [NOTES]                                               |
| :-----: | ------------------------------------------ | ----------------------------------------------------- |
|   [1]   | `[AssemblyFixture]`                        | Attribute + ctor injection; not `IAssemblyFixture<T>` |
|   [2]   | `IClassFixture<T>`/`ICollectionFixture<T>` | Shared state or expensive fixture only                |
|   [3]   | `IAsyncLifetime`                           | Async setup/cleanup when boundary is async            |
|   [4]   | `ITestOutputHelper`                        | Diagnostics only; do not replace assertions           |
|   [5]   | `TestContext.Current.CancellationToken`    | Long loops, async checks, shrink-heavy work           |

Assembly fixtures use public parameterless constructors, initialize before assembly test execution, inject by exact fixture type, and clean up through `DisposeAsync` or `Dispose`.

---
## [4][ASSERTIONS]
>**Dictum:** *Assert structural contracts, not incidental strings.*

<br>

- Use `Spec.Succ`, `Spec.Fail`, `Spec.Valid`, `Spec.Invalid`, `Spec.FailCategory`, and `Spec.FailMany` for Rasm rails.
- Use `Assert.Equivalent` for stable object shape only when member equality is the oracle.
- Use `Assert.Throws*` only at boundary adapters where exceptions are the public contract.
- Avoid `Assert.Skip`; bridge-owned native behavior belongs in `*.verify.csx`.

---
## [5][MTP_FILTERS]
>**Dictum:** *Filters belong to the runner that owns discovery.*

<br>

| [INDEX] | [INPUT]            | [MTP_FLAG]          | [RASM_ROUTE]                       |
| :-----: | ------------------ | ------------------- | ---------------------------------- |
|   [1]   | `/assembly/...`    | `--filter-query`    | xUnit query language               |
|   [2]   | `Category=Algebra` | `--filter-trait`    | Trait filter when traits exist     |
|   [3]   | `SomeSpec`         | `--filter-class`    | Class-shaped spec/law names        |
|   [4]   | `SomeLaw`          | `--filter-method`   | Method-shaped focused law names    |

`tools.quality test` maps single filter text to MTP-native query, trait, class, or method flags.

---
## [6][THEORY_DATA_FROM_SMARTENUM]
>**Dictum:** *Reflection over closed-set catalogs is the maintenance-free Theory.*

<br>

When a `[Theory]` should cover every case of a `[SmartEnum<int>]` or `[Union]`, build `TheoryData` from the closed-set `Items` property rather than enumerating `[InlineData]` rows. Adding a new case auto-extends coverage; no spec maintenance is required.

Cross-reference: this pattern is also a Stryker enabler — see `docs/testing-libs/stryker/api.md [6][THEORY_AS_STRYKER_ENABLER]`.

---
## [7][TEST_CONTEXT_CANCELLATION]
>**Dictum:** *Long-running properties must respect `TestContext.Current.CancellationToken`.*

<br>

`Spec.ForAll` reads `TestContext.Current.CancellationToken` and propagates into the property body. Raw `Check.Sample` calls do not. Read `TestContext.Current` at the use site and do not cache it across samples.

---
## [8][CUSTOM_TEST_PIPELINE_HOOKS]
>**Dictum:** *Pre-test assembly setup belongs to `ITestPipelineStartup`.*

<br>

| [INDEX] | [HOOK]                                          | [USE_CASE]                                                     |
| :-----: | ----------------------------------------------- | -------------------------------------------------------------- |
|   [1]   | `ITestPipelineStartup`                          | Invariant culture and managed assembly setup only              |
|   [2]   | `BeforeAfterTestAttribute`                      | Per-test interception when an assertion-visible hook is needed |
|   [3]   | `[assembly: AssemblyFixture(typeof(T))]` + ctor | Shared immutables; thread-shared; no per-test mutation         |

Do not use pipeline startup to warm Rhino/GH native APIs in static tests. Native probes belong in bridge scenarios. Avoid custom collection runner overrides; they break discovery in subtle ways.
