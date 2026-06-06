# [XUNIT_API]

[IMPORTANT] Use xUnit v3 through Microsoft Testing Platform when the project configures that runner. Runner selection and generated runner config are project package-map facts, not generic xUnit API facts.

## [1][PACKAGE_MODE]

| [INDEX] | [PACKAGE]                     | [STATE]          | [USE]                                      |
| :-----: | ----------------------------- | ---------------- | ------------------------------------------ |
|   [1]   | `xunit.v3.mtp-v2`             | Shared           | xUnit v3 MTP discovery and execution       |
|   [2]   | `xunit.v3.assert`             | Shared (testkit) | Assertions and serializer APIs             |
|   [3]   | `xunit.v3.common`             | Shared (testkit) | Common v3 abstractions                     |
|   [4]   | `xunit.v3.extensibility.core` | Shared (testkit) | Pipeline startup and extensibility APIs    |
|   [5]   | `xunit.analyzers`             | Transitive       | xUnit analyzer diagnostics through package |

[SOURCE] xUnit MTP docs: https://xunit.net/docs/getting-started/v3/microsoft-testing-platform

## [2][DISCOVERY]

| [INDEX] | [SURFACE]                                      | [PROJECT_USE]                                               |
| :-----: | ---------------------------------------------- | ----------------------------------------------------------- |
|   [1]   | `[Fact]`, `[Theory]`                           | Own-line attributes; CsCheck facts over large theory tables |
|   [2]   | `TheoryData<T1..T15>`/`TheoryDataRow<T1..T15>` | Small explicit edge matrices only                           |
|   [3]   | `ITheoryDataRow`                               | Row metadata when display/skip/timeout/traits are contract  |
|   [4]   | `MatrixTheoryData<T1..T15>`                    | Rare cartesian rows; prefer `Gen.Select` for broad axes     |
|   [5]   | `preEnumerateTheories`                         | Runner JSON; stable inputs; no runtime-random rows          |

MTP commands use `dotnet test --project <test-project>` or the local test runner; positional project paths are invalid in MTP mode.

## [3][FIXTURES_CONTEXT]

| [INDEX] | [SURFACE]                                  | [NOTES]                                               |
| :-----: | ------------------------------------------ | ----------------------------------------------------- |
|   [1]   | `[AssemblyFixture]`                        | Attribute + ctor injection; not `IAssemblyFixture<T>` |
|   [2]   | `IClassFixture<T>`/`ICollectionFixture<T>` | Shared state or expensive fixture only                |
|   [3]   | `IAsyncLifetime`                           | Async setup/cleanup when boundary is async            |
|   [4]   | `ITestOutputHelper`                        | Diagnostics only; do not replace assertions           |
|   [5]   | `TestContext.Current.CancellationToken`    | Long loops, async checks, shrink-heavy work           |

Assembly fixtures use public parameterless constructors, initialize before assembly test execution, inject by exact fixture type, and clean up through `DisposeAsync` or `Dispose`.

## [4][ASSERTIONS]

- Use the project assertion wrappers for success, failure, validation, category, and multi-failure rails.
- Use `Assert.Equivalent` for stable object shape only when member equality is the oracle.
- Use `Assert.Throws*` only at boundary adapters where exceptions are the public contract.
- Avoid `Assert.Skip`; host-native behavior belongs in runtime scenarios.

## [5][MTP_FILTERS]

| [INDEX] | [INPUT]            | [MTP_FLAG]          | [PROJECT_ROUTE]                    |
| :-----: | ------------------ | ------------------- | ---------------------------------- |
|   [1]   | `/assembly/...`    | `--filter-query`    | xUnit query language               |
|   [2]   | `Category=Algebra` | `--filter-trait`    | Trait filter when traits exist     |
|   [3]   | `SomeSpec`         | `--filter-class`    | Class-shaped spec/law names        |
|   [4]   | `SomeLaw`          | `--filter-method`   | Method-shaped focused law names    |

The local test runner maps single filter text to MTP-native query, trait, class, or method flags when configured.

## [6][THEORY_DATA_FROM_SMARTENUM]

When a `[Theory]` should cover every case of a `[SmartEnum<int>]` or `[Union]`, build `TheoryData` from the closed-set `Items` property rather than enumerating `[InlineData]` rows. Adding a new case auto-extends coverage; no spec maintenance is required.

Cross-reference: this pattern is also a Stryker enabler — see `docs/testing-libs/stryker/api.md [6][THEORY_AS_STRYKER_ENABLER]`.

## [7][TEST_CONTEXT_CANCELLATION]

`Spec.ForAll` reads `TestContext.Current.CancellationToken` and propagates into the property body. Raw `Check.Sample` calls do not. Read `TestContext.Current` at the use site and do not cache it across samples.

## [8][CUSTOM_TEST_PIPELINE_HOOKS]

| [INDEX] | [HOOK]                                          | [USE_CASE]                                                     |
| :-----: | ----------------------------------------------- | -------------------------------------------------------------- |
|   [1]   | `ITestPipelineStartup`                          | Invariant culture and managed assembly setup only              |
|   [2]   | `BeforeAfterTestAttribute`                      | Per-test interception when an assertion-visible hook is needed |
|   [3]   | `[assembly: AssemblyFixture(typeof(T))]` + ctor | Shared immutables; thread-shared; no per-test mutation         |

Do not use pipeline startup to warm host-native APIs in static tests. Native probes belong in runtime scenarios. Avoid custom collection runner overrides; they break discovery in subtle ways.
