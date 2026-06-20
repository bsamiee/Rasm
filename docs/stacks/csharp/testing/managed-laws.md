# [MANAGED_LAWS]

xUnit v3 and CsCheck own static managed law proof. Use project testkit wrappers first; promote raw package use into the testkit only after repeated specs need the same rail.

## [01]-[XUNIT_V3]

[DISCOVERY]:
- Use: `[Fact]` for named laws, regression seeds, and single-oracle managed contracts.
- Use: `[Theory]` only for small explicit edge matrices or closed-set rows where every row is meaningful.
- Use: `TheoryData<T1, ...>` from generated closed sets such as smart-enum `Items` when the source set is the contract.
- Reject: broad input axes in `[InlineData]` tables.
- Gate: `preEnumerateTheories` only for stable inputs; runtime-random rows break discovery.

[FIXTURES_AND_CONTEXT]:
- Use: `[AssemblyFixture]` with constructor injection for assembly-wide immutable state.
- Use: `IClassFixture<T>` and `ICollectionFixture<T>` only for expensive or shared managed fixtures.
- Use: `IAsyncLifetime` when setup or cleanup is genuinely async.
- Use: `ITestOutputHelper` for diagnostics, never as a substitute for assertions.
- Cancellation: read `TestContext.Current.CancellationToken` at the use site for long loops, async checks, and shrink-heavy work.

[ASSERTIONS]:
- Prefer: project assertion wrappers for success, failure, validation, category, and multi-failure rails.
- Use: `Assert.Equivalent` when stable member shape is the oracle.
- Use: `Assert.Throws*` only at boundary adapters where exceptions are the public contract.
- Reject: proving host-native behavior with static managed assertions.

[PIPELINE_HOOKS]:
- Use: `ITestPipelineStartup` for invariant culture or managed assembly setup only.
- Use: `BeforeAfterTestAttribute` only when an assertion-visible hook is required.
- Reject: warming host-native APIs in static tests.

## [02]-[MTP_FILTERS]

This table is a lookup by focused test input.

| [INDEX] | [INPUT]            | [FLAG]            | [USE]          |
| :-----: | :----------------- | :---------------- | :------------- |
|  [01]   | `/assembly/...`    | `--filter-query`  | xUnit query    |
|  [02]   | `Category=Algebra` | `--filter-trait`  | trait contract |
|  [03]   | `SomeSpec`         | `--filter-class`  | spec focus     |
|  [04]   | `SomeLaw`          | `--filter-method` | law focus      |

MTP commands use the local runner or `dotnet test --project <test-project>`. Positional project paths are invalid in MTP mode.

## [03]-[CSCHECK]

[PROPERTY_RAILS]:
- `Check.Sample` and `Check.SampleAsync`: generated input laws; project wrapper is `Spec.ForAll`.
- `Check.SampleModelBased`: actual versus smaller model; promote only when the model is meaningfully simpler than production.
- `Check.SampleMetamorphic`: paired-operation package API; do not confuse it with project `Spec.Metamorphic`.
- `Gen<T>.SampleParallel`: contention and linearizability; wrap through the project concurrent profile rail.
- `Check.Hash`: stable tool artifacts only.
- `Check.ChiSquared`: testkit generator distribution audits.
- `Check.Faster`: local algorithm selection only; durable performance claims route to BenchmarkDotNet.

[GENERATOR_DESIGN]:
- Use: `Select` and `SelectMany` for products and dependent values.
- Use: `OneOfConst`, `OneOf`, and `Frequency` for closed cases and edge bias.
- Use: `Shuffle` and `ShuffleSelect` for permutation and selection laws.
- Use: `Recursive` only with explicit depth discipline.
- Use: `Clone` for identical streams in two-path comparisons.
- Boundary: generate arrays, dictionaries, sets, and lists before projecting into LanguageExt collections.
- Gate: keep predicates broad enough to preserve shrinkable satisfying values.

[SHRINKING]:
- Rule: do not throw inside generator projections.
- Repair: use `Where` before projection when a factory can reject input.
- Gate: `WhereLimit` belongs to package filtering behavior; project generators keep acceptance broad enough for useful shrinking.

[MODEL_BASED_CHECKS]:
- Use: `Atom<T>` or `ConcurrentDictionary<TKey,TValue>` against `Dictionary<TKey,TValue>`.
- Use: generated union dispatch against a finite map.
- Use: validation accumulation against a list of errors.
- Use: stateful resource policy against a finite state machine.
- Reject: a model that duplicates production.

[DISTRIBUTION_AND_PARALLELISM]:
- Use: `Check.ChiSquared` in testkit generator audits when edge bias, filters, or smart-enum frequencies are part of coverage.
- Gate: expected bucket counts stay above 5, and actual and expected buckets use the same shape.
- Use: `SampleParallel` for contention and process-static cache races.
- Replay: use the emitted parallel seed, including thread suffixes.

## [04]-[PROJECT_TESTKIT]

[PROJECT_WRAPPERS]:
- `Spec.ForAll`: primary property rail; precedence is explicit arguments, environment variables, then package defaults.
- `Spec.Metamorphic`: one generated input checked by path or oracle equality; not CsCheck `SampleMetamorphic`.
- `Spec.Regression`: durable shrunk seed after product behavior is classified.
- `Spec.ModelBased`: project rail for smaller-model checks.
- `Spec.MetamorphicOps`: paired operation rail when the operation set is the input.

[ENVIRONMENT_VARIABLES]:
- `CsCheck_Iter`: per-property iteration count.
- `CsCheck_Time`: wall-clock budget in seconds.
- `CsCheck_Threads`: parallel sample workers.
- `CsCheck_Seed`: fixed seed for repro.
- `CsCheck_Replay`: failing-seed replay count for parallel reproduction.
- `CsCheck_Sigma`: statistical significance for `Check.Faster`.
- `CsCheck_Timeout`: per-sample timeout in seconds.
- `CsCheck_Ulps`: package floating equality slack; project tolerances live in assertion wrappers.
- `CsCheck_WhereLimit`: filter rejection cap.

## [05]-[MUTATION_INTERACTION]

A CsCheck fact is one mutation entry point. A theory with distinct rows creates separately tracked entry points. Convert a closed-set property into theory rows only when mutation analysis shows one smart-enum or union case is under-killed and the row split preserves the oracle.
