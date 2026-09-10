# [TESTS]

Shared test support per language and suites that do not sit beside their source.

## [01]-[LAYOUT]

```text
tests/
├── dotnet/
│   └── Rasm.TestSupport/   # Reusable .NET test support
├── python/
│   ├── conftest.py         # Package registration for public-API coverage
│   └── support/            # Reusable Python test support and the pytest runtime plugin
└── typescript/
    └── support/            # Reusable TypeScript test support and the Vitest setup file
```

[PLACEMENT]:
- Each language area holds its reusable fixtures, generators, assertions, and doubles in one support project or directory, `libs/` holds none
- .NET and Python suites mirror `libs/<language>/` under `tests/<language>/libs/<package>/`
- TypeScript tests sit beside their source, a suite outside `libs/typescript` is a package under `tests/typescript/`
- Test files are `test_<module>.py`, `<module>.spec.ts`, and `<Subject>.Tests.cs`, PascalCase begins at a C# project directory

## [02]-[CLASSIFICATION]

Classify each test by scope, technique, and execution mode, and apply every classification that fits.

| [INDEX] | [AXIS]    | [VALUE]        | [DEFINITION]                                | [ROUTE]                                                     |
| :-----: | :-------- | :------------- | :------------------------------------------ | :---------------------------------------------------------- |
|  [01]   | Scope     | Unit           | Isolated behavior, controlled collaborators | Default `test` run per language                             |
|  [02]   | Scope     | Integration    | Real components or an external boundary     | Python `network` marker                                     |
|  [03]   | Technique | Property-based | Generated examples exercise an invariant    | `TestAssertions.ForAll`, `@property_test`, `it.effect.prop` |
|  [04]   | Mode      | Benchmark      | Timing outside the functional test session  | `benchmark` fixture, Vitest `*.bench.ts` file               |

## [03]-[ORACLES]

Every test asserts observable behavior against an oracle independent of the implementation under test: closed-form calculations, invariants, metamorphic relations, reference models, fixtures, runtime observations, and documented external contracts.

[REQUIREMENTS]:
- Compilers, import checks, and type checkers verify symbols exist, runtime tests assert behavior
- Expected values come from an independent oracle
- Structural assertions on values the test constructs prove nothing, pair them with a behavioral assertion or delete them
- Boundary tests supply invalid raw input through supported entry points, tests inside the boundary build every state through construction
- Parameterized and property-based tests cover input classes and invariants
- Properties defined from a predicate name a counterexample they must reject, a law that compares two evaluations needs none

Treat a failing test as evidence until triage identifies a production defect, an obsolete requirement, or an invalid oracle. Fix a production defect in its code, retire the test of a retired requirement or invalid oracle.

## [04]-[OWNERS]

Use README.md for the owner of every test dependency version, target, tool configuration, and output location.
