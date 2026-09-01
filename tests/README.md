# [TESTS]

`tests/` owns cross-language test policy, reusable test support, and suites that do not colocate with production source. Tests specify supported behavior and fail when that behavior regresses; delete a test only when the behavior is retired or its oracle is invalid, and repair tests that cannot run in a supported environment.

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
- Tier, grouping, and shared infrastructure directories are lowercase
- PascalCase begins at a C# project boundary and continues through source folders inside it; grouping directories above a project stay lowercase
- Python test modules use `test_<module>.py`; TypeScript test files use `<module>.spec.ts`; C# test source files use `<Subject>.Tests.cs`

[SHARED_TEST_CODE]:
- Each language area centralizes reusable fixtures, generators, assertions, and test harness code in its `support/` directory
- Each test-support source with repository-owned executable behavior carries its test beside it
- Production packages under `libs/` contain no shared test support

## [02]-[TEST_CLASSIFICATION]

Classify each test independently by scope, technique, and execution mode; apply every classification that fits.

| [INDEX] | [AXIS]         | [VALUE]        | [DEFINITION]                                                | [ROUTE]                                                          |
| :-----: | :------------- | :------------- | :---------------------------------------------------------- | :--------------------------------------------------------------- |
|  [01]   | Scope          | Unit           | Isolated component behavior with controlled collaborators   | default test run per language                                    |
|  [02]   | Scope          | Integration    | Interaction between real components or an external boundary | Python `network`/`subprocess`; language-specific integration run |
|  [03]   | Technique      | Property-based | Generated examples exercise an invariant                    | `TestAssertions.Verify`, `@property_test`, `it.effect.prop`      |
|  [04]   | Execution mode | Benchmark      | Performance measurement outside the functional test session | `-m benchmark`, benchmark include glob                           |

`integration` follows the subject under test, not process count: an in-process test that composes real components is an integration test, while a test with controlled collaborators remains a unit test.

## [03]-[TEST_ORACLES]

Every test asserts observable behavior against an oracle independent of the implementation under test. Valid oracles include closed-form calculations, invariants, metamorphic relations, reference models, fixed fixtures, runtime observations, and documented external contracts.

Structural assertions on values constructed by the test do not establish behavior; pair them with an independent behavioral assertion or delete them.

Each reusable property definition must include a known counterexample. Properties accepting their counterexample are vacuous and fail at registration.

[TEST_REQUIREMENTS]:
- Compilers, import checks, and type checkers verify static symbol existence; runtime tests assert behavior
- Expected values come from an independent oracle; duplicating the production algorithm or snapshotting a value created by the same test is self-fulfilling
- Boundary tests supply invalid raw input through supported entry points; interior tests do not bypass construction to fabricate impossible states
- Parameterized and property-based tests cover input classes and invariants; one shallow test per function is not a coverage strategy

Treat a failing test as evidence until triage identifies a production defect, an obsolete requirement, or an invalid oracle. Fix production defects in production code; change the test only when the supported behavior or oracle is wrong.

## [04]-[GENERATED_OUTPUTS]

Tool configurations write reports under `.artifacts/` and relocatable temporary state under `.cache/<tool>/`. Stryker.NET creates the upstream-fixed `.stryker-tmp/` root work directory; `.gitignore` excludes it, and its reports still use the configured artifact directory.

| [INDEX] | [TOOL]            | [OUTPUT]                           | [CONFIGURATION]                                                                                  |
| :-----: | :---------------- | :--------------------------------- | :----------------------------------------------------------------------------------------------- |
|  [01]   | coverlet.MTP      | .NET coverage                      | `--coverlet` on run; writes to the run's results directory                                       |
|  [02]   | MTP results       | .NET results, dumps, xUnit reports | MTP default beside the test app under `.artifacts/dotnet/bin`; `nx test <project>` is the runner |
|  [03]   | pytest + coverage | Python coverage + caches           | `pyproject.toml` tool tables                                                                     |
|  [04]   | Hypothesis        | Example database + observability   | `tests/python/support/runtime.py`                                                                |
|  [05]   | Pytest-benchmark  | Python benchmark storage           | `pyproject.toml` addopts                                                                         |
|  [06]   | Vitest            | TypeScript test outputs            | Root `vitest.config.ts`                                                                          |
|  [07]   | StrykerJS         | TypeScript mutation                | `stryker.config.json`                                                                            |
|  [08]   | Stryker.NET       | .NET mutation reports              | `stryker-config.json`                                                                            |
|  [09]   | Nx                | Target outputs + cache             | `nx.json` plugins: `@nx/dotnet` infers `test` per MTP project                                    |

Configure output paths through the tool's documented configuration, preferring a config-file setting and then a CLI option; wrapper scripts and `conftest.py` must not control output paths. After the tool runs, `git status --short` and the repository-root listing must show no new generated entries.

## [05]-[SUITE_PLACEMENT]

Add each suite, reusable test capability, fixture, or test asset to the existing directory or module responsible for it. Extend that location when the responsibility is unchanged; refactor it when the current structure no longer fits.

| [INDEX] | [ADDITION]                       | [HOME]                                 |
| :-----: | :------------------------------- | :------------------------------------- |
|  [01]   | .NET reusable test support       | `tests/dotnet/support`                 |
|  [02]   | .NET package suite               | `tests/dotnet/libs/<package>/`         |
|  [03]   | Python reusable test support     | `tests/python/support`                 |
|  [04]   | Python package suite             | `tests/python/libs/<package>/`         |
|  [05]   | TypeScript unit test             | Beside its source in `libs/typescript` |
|  [06]   | TypeScript reusable support      | `tests/typescript/support`             |

.NET and Python package suites mirror their production package beneath `tests/<language>/libs`. TypeScript unit tests colocate with source; `tests/typescript/` holds reusable support and suites whose scope is outside one production package.

## [06]-[GATE_OWNERSHIP]

- TypeScript Stryker configuration lives in root `stryker.config.json`
- .NET Stryker configuration lives in root `stryker-config.json`
- Python mutmut configuration must live in root `pyproject.toml` under `[tool.mutmut]`; `conftest.py` remains pytest composition only
- .NET or TypeScript Stryker runs that discover zero mutants fail
- Coverage aggregates as Cobertura (.NET) and LCOV (Python, TypeScript) under `.artifacts/`; each reporter defines its output format

## [07]-[CONFIGURATION_OWNERS]

Read the owning configuration before changing a test dependency, runner, output, or required check.

| [INDEX] | [CONFIGURATION]                                  | [RESPONSIBILITY]                                                                   |
| :-----: | :----------------------------------------------- | :--------------------------------------------------------------------------------- |
|  [01]   | `Directory.Packages.props`                       | .NET test dependency versions                                                      |
|  [02]   | Each test `.csproj` + `Directory.Build.targets`  | MTP runner and package references; package-derived xUnit and CsCheck usings        |
|  [03]   | `pyproject.toml`                                 | Python test dependencies and pytest and coverage policy                            |
|  [04]   | `pnpm-workspace.yaml`                            | TypeScript test dependency versions, peer resolutions, and workspace package globs |
|  [05]   | Parametric_Forge `dev-tools.nix`                 | .NET CLI tools available on `PATH`                                                 |
|  [06]   | `vitest.config.ts` + `stryker*.json` + `nx.json` | TypeScript runner defaults, generated outputs, mutation policy, and Nx targets     |
