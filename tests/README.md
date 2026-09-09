# [TESTS]

Tests under `tests/` hold the cross-language test policy, reusable test support, and suites that do not colocate with production source. Tests specify supported behavior and fail when it regresses. Delete a test only when the behavior is retired or its oracle is invalid.

## [01]-[LAYOUT]

```text
tests/
├── dotnet/
│   ├── Rasm.TestSupport/   # Reusable .NET test support
│   └── libs/               # Suites mirroring libs/dotnet, one per package
├── python/
│   ├── support/            # Reusable Python test support
│   └── libs/               # Per-package suites mirroring libs/python
└── typescript/
    ├── support/            # Reusable TypeScript test support
    ├── browser/            # Playwright suites over a served page
    └── libs/               # Suites that span more than one package
```

[CASING]:
- Language and grouping directories are lowercase, and the Python and TypeScript support directories are `support/`
- PascalCase begins at a C# project directory and continues inside it
- Python test modules are `test_<module>.py`, TypeScript `<module>.spec.ts`, and C# `<Subject>.Tests.cs`

[SHARED_TEST_CODE]:
- Each language area centralizes reusable fixtures, generators, assertions, and harness code in one support project or directory
- Production packages under `libs/` contain no shared test support

## [02]-[TEST_CLASSIFICATION]

Classify each test by scope, technique, and execution mode, and apply every classification that fits.

| [INDEX] | [AXIS]    | [VALUE]        | [DEFINITION]                                | [ROUTE]                                                     |
| :-----: | :-------- | :------------- | :------------------------------------------ | :---------------------------------------------------------- |
|  [01]   | Scope     | Unit           | Isolated behavior, controlled collaborators | Default `test` run per language                             |
|  [02]   | Scope     | Integration    | Real components or an external boundary     | Python `network` and `subprocess` markers                   |
|  [03]   | Technique | Property-based | Generated examples exercise an invariant    | `TestAssertions.ForAll`, `@property_test`, `it.effect.prop` |
|  [04]   | Mode      | Benchmark      | Timing outside the functional test session  | `benchmark` marker, Vitest bench glob                       |

## [03]-[TEST_ORACLES]

Every test asserts observable behavior against an oracle independent of the implementation under test. Valid oracles include closed-form calculations, invariants, metamorphic relations, reference models, fixed fixtures, runtime observations, and documented external contracts.

Structural assertions on values the test constructs prove nothing, pair them with an independent behavioral assertion or delete them.

[TEST_REQUIREMENTS]:
- Compilers, import checks, and type checkers verify symbols exist, runtime tests assert behavior
- Expected values come from an independent oracle
- Boundary tests supply invalid raw input through supported entry points, and tests inside the boundary build every state through construction
- Parameterized and property-based tests cover input classes and invariants

Treat a failing test as evidence until triage identifies a production defect, an obsolete requirement, or an invalid oracle.

## [04]-[GENERATED_OUTPUTS]

Every test tool writes its reports under `.artifacts/<language>/` and its relocatable state under `.cache/<tool>/`, configured in the tool's own config file or on its command. After a tool runs, `git status --short` shows no new entry.

## [05]-[SUITE_PLACEMENT]

| [INDEX] | [ADDITION]                   | [HOME]                                  |
| :-----: | :--------------------------- | :-------------------------------------- |
|  [01]   | .NET reusable test support   | `tests/dotnet/Rasm.TestSupport`         |
|  [02]   | .NET package suite           | `tests/dotnet/libs/<package>/`          |
|  [03]   | Python reusable test support | `tests/python/support`                  |
|  [04]   | Python package suite         | `tests/python/libs/<package>/`          |
|  [05]   | TypeScript unit test         | Beside its source in `libs/typescript`  |
|  [06]   | TypeScript reusable support  | `tests/typescript/support`              |
|  [07]   | Browser end-to-end test      | `tests/typescript/browser`              |

## [06]-[CONFIGURATION_OWNERS]

| [INDEX] | [CONFIGURATION]                                    | [RESPONSIBILITY]                                                   |
| :-----: | :------------------------------------------------- | :----------------------------------------------------------------- |
|  [01]   | `Directory.Packages.props`                         | .NET test dependency versions                                      |
|  [02]   | Each test `.csproj` with `Directory.Build.targets` | MTP runner and package references, global xUnit and CsCheck usings |
|  [03]   | `pyproject.toml`                                   | Python test dependencies, pytest and coverage policy               |
|  [04]   | `pnpm-workspace.yaml`                              | TypeScript test versions, peer resolutions, package globs          |
|  [05]   | `vitest.config.ts`                                 | TypeScript runner defaults and outputs                             |
