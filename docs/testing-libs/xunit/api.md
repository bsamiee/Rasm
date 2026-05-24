# [H1][XUNIT_API]
>**Dictum:** *xUnit owns orchestration; Rasm owns laws.*

<br>

[IMPORTANT] Rasm pins xUnit v3 packages in `Directory.Packages.props` and injects runnable test packages from `Directory.Build.props`. Keep raw xUnit API detail here; keep law strategy in `.claude/skills/cs-testing`.

---
## [1][PACKAGE_MODE]
>**Dictum:** *The runner rail is explicit.*

<br>

| [INDEX] | [PACKAGE] | [PIN] | [USE] |
| :-----: | --------- | ----- | ----- |
| [1] | `xunit.v3.mtp-off` | `3.2.2` | Runnable test projects; VSTest adapter path, not MTP. |
| [2] | `xunit.v3.assert` | `3.2.2` | `Rasm.TestKit` assertion and serializer support. |
| [3] | `xunit.runner.visualstudio` | `3.1.5` | VSTest discovery/execution from `dotnet test`. |
| [4] | `xunit.analyzers` | `1.27.0` | Test analyzer diagnostics. |

[SOURCE] xUnit v3.2.2 release notes: https://xunit.net/releases/v3/3.2.2

---
## [2][ASSERTIONS]
>**Dictum:** *Prefer structural assertions over message strings.*

<br>

| [INDEX] | [SURFACE] | [RASM_USE] |
| :-----: | --------- | ----------- |
| [1] | `Assert.Equal`, `NotEqual`, `Same`, `IsType`, `Contains`, `Empty` | Structural laws and typed receipts. |
| [2] | `Assert.True` / `False` | Only for simple boolean invariants; use `Spec.Holds` when the label needs generated values. |
| [3] | `Assert.Equivalent` | Deep shape comparison when member-by-member equality is the oracle. |
| [4] | `Assert.Throws*`, `Record.Exception*` | Boundary adapters only; Rasm product rails usually return `Fin`/`Validation`. |
| [5] | `Assert.Skip`, `SkipWhen`, `SkipUnless` | Avoid in static specs; bridge-owned native behavior belongs in `*.verify.csx`. |

---
## [3][ATTRIBUTES_AND_FIXTURES]
>**Dictum:** *Use the v3 API that exists, not v2 folklore.*

<br>

| [INDEX] | [SURFACE] | [NOTES] |
| :-----: | --------- | ------- |
| [1] | `[Fact]`, `[Theory]` | Put attributes on their own line. Prefer CsCheck packed facts over large theory tables. |
| [2] | `[Trait]`, `[Collection]`, `[CollectionDefinition]` | Use for runner partitioning only when needed by shared state. |
| [3] | `[ClassData]`, `[MemberData]`, `[InlineData]` | v3 data rows support `object?[]`, `ITheoryDataRow`, and `ITuple`. |
| [4] | `[AssemblyFixture]` | Assembly fixture is an attribute; do not invent `IAssemblyFixture<T>`. Constructor injection gives test classes access. |
| [5] | `IClassFixture<T>`, `ICollectionFixture<T>` | Fixture interfaces still own class/collection lifetimes. |
| [6] | `IAsyncLifetime`, `IAsyncDisposable`, `IDisposable` | Async fixture/test cleanup when the tested boundary is async. |

[SOURCE] API namespace list and fixture descriptions: https://api.xunit.net/v3/3.2.2/Xunit.html

---
## [4][THEORY_DATA]
>**Dictum:** *Typed data is bounded; generators own the cartesian explosion.*

<br>

| [INDEX] | [TYPE] | [ARITY] | [USE] |
| :-----: | ------ | :-----: | ----- |
| [1] | `TheoryData` | Untyped | Rare; prefer typed rows. |
| [2] | `TheoryData<T1..T15>` | 1 through 15 | Small hand-curated edge tables. |
| [3] | `TheoryDataRow<T1..T15>` | 1 through 15 | Row metadata and pre-enumerated data. |
| [4] | `MatrixTheoryData<T1..T15>` | 2 through 15 | Matrix expansion; avoid when CsCheck can generate the axis. |

[SOURCE] xUnit API exposes `TheoryData` and `TheoryDataRow` through 15 generic arguments: https://api.xunit.net/v3/3.2.2/Xunit.html

---
## [5][TEST_CONTEXT]
>**Dictum:** *Cancellation is part of the oracle contract.*

<br>

- `TestContext.Current` exposes current engine state during execution; check nullable members before use.
- Thread `TestContext.Current.CancellationToken` into long loops or async bridge clients when a timeout or runner cancellation must stop shrinking.
- Prefer `ITestOutputHelper` for structured diagnostic output from a single test; prefer `Spec.ConcurrentProfiled`/`Causal.Profile` for CsCheck causal profiling.

---
## [6][RUNNER_JSON]
>**Dictum:** *Rasm generates runner config; there is no root runner file.*

<br>

`Directory.Build.props` generates per-project `obj/xunit.runner.json` content for runnable tests. The official xUnit docs also support a test-project-root `xunit.runner.json` or an assembly-named variant copied to output, but Rasm keeps this generated to avoid loose root files.

Key runner settings to know:

| [INDEX] | [SETTING] | [USE] |
| :-----: | --------- | ----- |
| [1] | `assertEquivalentMaxDepth` | Bound deep equivalence recursion. |
| [2] | `culture` | Force invariant/default culture. |
| [3] | `diagnosticMessages`, `internalDiagnosticMessages` | Runner diagnostics. |
| [4] | `failSkips`, `failWarns` | Treat skips/warnings as failures. |
| [5] | `longRunningTestSeconds` | Emit hung-test diagnostics when diagnostics are enabled. |
| [6] | `maxParallelThreads`, `parallelAlgorithm` | Assembly parallelism. |
| [7] | `methodDisplay`, `methodDisplayOptions` | Display names only. |

[SOURCE] xUnit runner config docs: https://xunit.net/docs/config-xunit-runner-json

---
## [7][MTP_VS_VSTEST]
>**Dictum:** *Do not mix runner paradigms without a migration task.*

<br>

xUnit v3 templates can target MTP or VSTest, and v3 package defaults changed across releases. Rasm currently pins `xunit.v3.mtp-off` and `xunit.runner.visualstudio`, so validation and mutation should stay on VSTest unless a future migration changes central props and proves it through restore/build/test.

[SOURCE] Microsoft test platform comparison: https://learn.microsoft.com/en-us/dotnet/core/testing/test-platforms-overview
