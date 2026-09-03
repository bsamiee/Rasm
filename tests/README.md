# [TESTS]

`tests/` owns cross-language test policy, reusable test support, and suites that do not colocate with production source. Tests specify supported behavior and fail when it regresses. Delete a test only when the behavior is retired or its oracle is invalid, and repair tests that cannot run in a supported environment.

## [01]-[LAYOUT]

```text
tests/
├── dotnet/
│   ├── support/        # Reusable .NET test support
│   └── libs/           # Per-package suites mirroring libs/dotnet
├── python/
│   ├── support/        # Reusable Python test support
│   └── libs/           # Per-package suites mirroring libs/python
└── typescript/
    ├── support/        # Reusable TypeScript test support
    └── libs/           # Per-package suites mirroring libs/typescript
```

[CASING]:
- Language, grouping, and support directories are lowercase
- PascalCase begins at a C# project directory and continues inside it, grouping directories above stay lowercase
- Python test modules are `test_<module>.py`, TypeScript `<module>.spec.ts`, and C# `<Subject>.Tests.cs`

[SHARED_TEST_CODE]:
- Each language area centralizes reusable fixtures, generators, assertions, and test harness code in its `support/` directory
- Each test support module with executable behavior has its test beside it
- Production packages under `libs/` contain no shared test support

## [02]-[TEST_CLASSIFICATION]

Classify each test independently by scope, technique, and execution mode, and apply every classification that fits.

| [INDEX] | [AXIS]         | [VALUE]        | [DEFINITION]                                                | [ROUTE]                                                             |
| :-----: | :------------- | :------------- | :---------------------------------------------------------- | :------------------------------------------------------------------ |
|  [01]   | Scope          | Unit           | Isolated component behavior with controlled collaborators   | default test run per language                                       |
|  [02]   | Scope          | Integration    | Interaction between real components or an external boundary | Python `network`/`subprocess` markers, per-language integration run |
|  [03]   | Technique      | Property-based | Generated examples exercise an invariant                    | `TestAssertions.Verify`, `@property_test`, `it.effect.prop`         |
|  [04]   | Execution mode | Benchmark      | Performance measurement outside the functional test session | Python `test` `benchmark` configuration, Vitest benchmark glob      |

Integration follows the subject under test, not process count: an in-process test over real components is integration, a test with controlled collaborators is unit.

## [03]-[TEST_ORACLES]

Every test asserts observable behavior against an oracle independent of the implementation under test. Valid oracles include closed-form calculations, invariants, metamorphic relations, reference models, fixed fixtures, runtime observations, and documented external contracts.

Structural assertions on values the test constructs prove nothing, pair them with an independent behavioral assertion or delete them.

Each reusable property must include a known counterexample, a property that accepts it is vacuous and fails at registration.

[TEST_REQUIREMENTS]:
- Compilers, import checks, and type checkers verify symbols exist, runtime tests assert behavior
- Expected values come from an independent oracle, duplicating the production algorithm or snapshotting the test's own value is self-fulfilling
- Boundary tests supply invalid raw input through supported entry points, interior tests never bypass construction to build impossible states
- Parameterized and property-based tests cover input classes and invariants, a shallow test per function is not coverage

Treat a failing test as evidence until triage identifies a production defect, an obsolete requirement, or an invalid oracle. Fix production defects in production code, change the test only when the supported behavior or oracle is wrong.

## [04]-[GENERATED_OUTPUTS]

Tool configurations write reports under `.artifacts/` and relocatable temporary state under `.cache/<tool>/`. Stryker.NET creates the fixed `.stryker-tmp/` work directory, `.gitignore` excludes it and its reports still go to the configured artifact directory.

| [INDEX] | [TOOL]            | [OUTPUT]                           | [CONFIGURATION]                                                                            |
| :-----: | :---------------- | :--------------------------------- | :----------------------------------------------------------------------------------------- |
|  [01]   | coverlet.MTP      | .NET coverage per test project     | `test` target, `Directory.Build.targets` passes the coverlet arguments per test project    |
|  [02]   | MTP results       | .NET results, dumps, xUnit reports | MTP default beside the test app under `.artifacts/dotnet/bin`, `nx test <project>` runs it |
|  [03]   | pytest-cov        | Python coverage data per run       | `test` target, `pyproject.toml` `[tool.coverage.*]` tables                                 |
|  [04]   | Hypothesis        | Example database and observability | `tests/python/support/runtime.py`                                                          |
|  [05]   | pytest-benchmark  | Python benchmark storage           | `test` target `benchmark` configuration, `pyproject.toml` addopts                          |
|  [06]   | Vitest            | TypeScript results and coverage    | `test` target, each `vitest.config.ts` from `createVitestConfig` in the root one           |
|  [07]   | Coverage merge    | One coverage report per language   | Root `coverage` target, `.artifacts/<language>/coverage/`                                  |
|  [08]   | StrykerJS         | TypeScript mutation                | `stryker.config.json`                                                                      |
|  [09]   | Stryker.NET       | .NET mutation reports              | `stryker-config.json`                                                                      |
|  [10]   | Nx                | Target outputs and cache           | `@nx/dotnet` and `@nx/vitest` infer `test`, `tools/nx/workspace.ts` tags and empty targets |

Configure output paths through the tool's documented configuration, config file first and CLI option second, never through wrapper scripts or `conftest.py`. After the tool runs, `git status --short` and the repository-root listing must show no new generated entries.

## [05]-[SUITE_PLACEMENT]

Add each suite, reusable test capability, fixture, or test asset to the existing directory or module responsible for it. Extend that location when the responsibility is unchanged, refactor it when the structure no longer fits.

| [INDEX] | [ADDITION]                   | [HOME]                                 |
| :-----: | :--------------------------- | :------------------------------------- |
|  [01]   | .NET reusable test support   | `tests/dotnet/support`                 |
|  [02]   | .NET package suite           | `tests/dotnet/libs/<package>/`         |
|  [03]   | Python reusable test support | `tests/python/support`                 |
|  [04]   | Python package suite         | `tests/python/libs/<package>/`         |
|  [05]   | TypeScript unit test         | Beside its source in `libs/typescript` |
|  [06]   | TypeScript reusable support  | `tests/typescript/support`             |

.NET and Python package suites mirror their production package beneath `tests/<language>/libs`. TypeScript unit tests colocate with source, `tests/typescript/` holds reusable support and suites that span more than one package.

## [06]-[MUTATION_AND_COVERAGE]

Mutation and coverage runs report what the tests reach, and no score gates a merge:
- Root `stryker.config.json` holds the TypeScript Stryker configuration, and root `stryker-config.json` holds the .NET one
- .NET or TypeScript Stryker runs that discover zero mutants fail
- Every `test` run collects coverage, and the root `coverage` target runs after them and merges the data into one report per language
- The merged reports sit under `.artifacts/<language>/coverage/` as lcov and xml for Python and lcov and json for TypeScript, and the .NET Cobertura merge arrives with the first .NET test project

## [07]-[CONFIGURATION_OWNERS]

Read the owning configuration before changing a test dependency, runner, output, or required check.

| [INDEX] | [CONFIGURATION]                                    | [RESPONSIBILITY]                                                       |
| :-----: | :------------------------------------------------- | :--------------------------------------------------------------------- |
|  [01]   | `Directory.Packages.props`                         | .NET test dependency versions                                          |
|  [02]   | Each test `.csproj` with `Directory.Build.targets` | MTP runner and package references, global xUnit and CsCheck usings     |
|  [03]   | `pyproject.toml`                                   | Python test dependencies, pytest and coverage policy                   |
|  [04]   | `pnpm-workspace.yaml`                              | TypeScript test dependency versions, peer resolutions, and package globs |
|  [05]   | `mise.toml` with `dotnet dnx`                      | Runtimes on `PATH` and the .NET CLI tools the checks run               |
|  [06]   | `vitest.config.ts` with `stryker*.json`            | TypeScript runner defaults, generated outputs, and mutation configuration |
|  [07]   | `nx.json` and the root `package.json` `nx` field   | Per-language targets by tag, the root targets, and the `coverage` merge |
