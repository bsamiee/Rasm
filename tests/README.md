# [TESTS]

Tests under `tests/` hold the cross-language test policy, reusable test support, and suites that do not colocate with production source. Tests specify supported behavior and fail when it regresses. Delete a test only when the behavior is retired or its oracle is invalid, and repair tests that cannot run in a supported environment.

## [01]-[LAYOUT]

```text
tests/
├── dotnet/
│   ├── Rasm.TestSupport/   # Reusable .NET test support, one project with the test of each module beside it
│   └── libs/               # Suites mirroring libs/dotnet, one per package or release group
├── python/
│   ├── support/        # Reusable Python test support
│   └── libs/           # Per-package suites mirroring libs/python
└── typescript/
    ├── support/        # Reusable TypeScript test support
    └── libs/           # Suites that span more than one package
```

[CASING]:
- Language and grouping directories are lowercase, and the Python and TypeScript support directories are `support/`
- PascalCase begins at a C# project directory and continues inside it, and the grouping directories that contain it stay lowercase
- Python test modules are `test_<module>.py`, TypeScript `<module>.spec.ts`, and C# `<Subject>.Tests.cs`

[SHARED_TEST_CODE]:
- Each language area centralizes reusable fixtures, generators, assertions, and test harness code in one support project or directory
- Each test support module with executable behavior has its test beside it
- Production packages under `libs/` contain no shared test support

## [02]-[TEST_CLASSIFICATION]

Classify each test independently by scope, technique, and execution mode, and apply every classification that fits.

| [INDEX] | [AXIS]    | [VALUE]        | [DEFINITION]                                | [ROUTE]                                                     |
| :-----: | :-------- | :------------- | :------------------------------------------ | :---------------------------------------------------------- |
|  [01]   | Scope     | Unit           | Isolated behavior, controlled collaborators | Default `test` run per language                             |
|  [02]   | Scope     | Integration    | Real components or an external boundary     | Python `network` and `subprocess` markers                   |
|  [03]   | Technique | Property-based | Generated examples exercise an invariant    | `TestAssertions.ForAll`, `@property_test`, `it.effect.prop` |
|  [04]   | Mode      | Benchmark      | Timing outside the functional test session  | `benchmark` configuration of `test`, Vitest bench glob      |

Scope follows the subject under test, an in-process test over real components is integration and a test with controlled collaborators is unit.

## [03]-[TEST_ORACLES]

Every test asserts observable behavior against an oracle independent of the implementation under test. Valid oracles include closed-form calculations, invariants, metamorphic relations, reference models, fixed fixtures, runtime observations, and documented external contracts.

Structural assertions on values the test constructs prove nothing, pair them with an independent behavioral assertion or delete them.

Each property defined from a predicate includes a known counterexample, a predicate that accepts it is vacuous and fails at registration, and a law that compares two evaluations needs none.

[TEST_REQUIREMENTS]:
- Compilers, import checks, and type checkers verify symbols exist, runtime tests assert behavior
- Expected values come from an independent oracle, duplicating the production algorithm or snapshotting the test's own value is self-fulfilling
- Boundary tests supply invalid raw input through supported entry points, and tests inside the boundary build every state through construction
- Parameterized and property-based tests cover input classes and invariants, a shallow test per function is not coverage

Treat a failing test as evidence until triage identifies a production defect, an obsolete requirement, or an invalid oracle. Fix production defects in production code, change the test only when the supported behavior or oracle is wrong.

## [04]-[GENERATED_OUTPUTS]

Every test tool writes its reports under `.artifacts/<language>/` and its relocatable state under `.cache/<tool>/`, configured in the tool's own config file or in its target when the file has no setting, and wrapper scripts and `conftest.py` set none. Stryker.NET takes its report directory from `--output` on the command the `mutation` script runs, because `stryker-config.json` rejects every key outside its schema. StrykerJS keeps the root `tsconfig.json` out of its sandbox through `ignorePatterns`, because its core parses that file with the JavaScript compiler API the native `typescript` package lacks. After a tool runs, `git status --short` shows no new entry.

Use the `monorepo-build-infrastructure` skill for the target and output layout.

## [05]-[SUITE_PLACEMENT]

Add each suite, reusable test capability, fixture, or test asset to the existing directory or module that holds its kind. Extend the location when the responsibility is unchanged, and refactor it when the structure no longer fits.

| [INDEX] | [ADDITION]                   | [HOME]                                  |
| :-----: | :--------------------------- | :-------------------------------------- |
|  [01]   | .NET reusable test support   | `tests/dotnet/Rasm.TestSupport`         |
|  [02]   | .NET package suite           | `tests/dotnet/libs/<release group>/`    |
|  [03]   | Python reusable test support | `tests/python/support`                  |
|  [04]   | Python package suite         | `tests/python/libs/<package>/`          |
|  [05]   | TypeScript unit test         | Beside its source in `libs/typescript`  |
|  [06]   | TypeScript reusable support  | `tests/typescript/support`              |

## [06]-[MUTATION_AND_COVERAGE]

Mutation and coverage runs report what the tests reach:
- The root `mutation` target runs one language's Stryker and fails when its report holds zero mutants
- Every `test` run collects coverage, and the root `coverage` target merges one language's data afterwards
- Python coverage measures the trees the `source` list of `pyproject.toml` names

## [07]-[CONFIGURATION_OWNERS]

Read the owning configuration before changing a test dependency, runner, output, or required check.

| [INDEX] | [CONFIGURATION]                                    | [RESPONSIBILITY]                                                   |
| :-----: | :------------------------------------------------- | :----------------------------------------------------------------- |
|  [01]   | `Directory.Packages.props`                         | .NET test dependency versions                                      |
|  [02]   | Each test `.csproj` with `Directory.Build.targets` | MTP runner and package references, global xUnit and CsCheck usings |
|  [03]   | `pyproject.toml`                                   | Python test dependencies, pytest and coverage policy               |
|  [04]   | `pnpm-workspace.yaml`                              | TypeScript test versions, peer resolutions, package globs          |
|  [05]   | `vitest.config.ts` with `stryker*.json`            | TypeScript runner defaults, outputs, mutation configuration        |

Use the `monorepo-build-infrastructure` skill for the toolchain and the targets.
