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
    ├── e2e/                # Playwright suite over served web apps
    └── support/            # Reusable TypeScript test support and the Vitest setup file
```

[PLACEMENT]:
- Each language area holds its reusable fixtures, generators, assertions, and doubles in one support project or directory, `libs/` holds none
- .NET and Python suites mirror `libs/<language>/` under `tests/<language>/libs/<group>/<package>/`
- TypeScript tests sit beside their source, a suite outside `libs/typescript` is a package under `tests/typescript/`
- Rust tests sit in a `#[cfg(test)]` module of the file they test, integration tests in one `[[test]]` target of their member
- Test files are `test_<module>.py`, `<module>.spec.ts`, and `<Subject>.Tests.cs`, PascalCase begins at a C# project directory

## [02]-[CLASSIFICATION]

Classify each test by scope, technique, and execution mode, and apply every classification that fits.

| [INDEX] | [AXIS]    | [VALUE]        | [DEFINITION]                           | [ROUTE]                                                                            |
| :-----: | :-------- | :------------- | :------------------------------------- | :--------------------------------------------------------------------------------- |
|  [01]   | Scope     | Unit           | Behavior with controlled collaborators | Default `test` run per language                                                    |
|  [02]   | Scope     | Integration    | Real components or external boundary   | Python `network` marker                                                            |
|  [03]   | Scope     | End-to-end     | Served app driven through a browser    | Playwright `e2e` target                                                            |
|  [04]   | Technique | Property-based | Generated examples exercise invariants | `TestAssertions.ForAll`, Hypothesis `@given`, `it.effect.prop`, `#[property_test]` |
|  [05]   | Mode      | Benchmark      | Timing outside functional test session | `benchmark` fixture, Vitest `*.bench.ts` file, Criterion `[[bench]]` target        |

Python `@property_test(subject)` records public-API coverage.

## [03]-[EXPECTED_BEHAVIOR]

Every test asserts observable behavior against evidence independent of the implementation under test.

[REQUIREMENTS]:
- Compilers, import checks, and type checkers verify symbols exist, runtime tests assert behavior
- Expected values come from a closed-form calculation, invariant, metamorphic relation, reference model, fixture, observation, or documented contract
- Expected output text is an inline snapshot in the module that produces it, never a loose `.stderr`, `.snap`, or golden file
- Structural assertions on values the test constructs assert nothing the code computes, pair them with a behavioral assertion or delete them
- Boundary tests supply invalid raw input through supported entry points, tests inside the boundary build every state through construction
- Parameterized and property-based tests cover input classes and invariants
- Properties defined from a predicate name a counterexample they must reject, laws equating evaluations need none

Treat a failing test as evidence until triage identifies a production defect, an obsolete requirement, or invalid reference evidence. Fix a production defect in its code, retire the test of a retired requirement or invalid reference evidence.

## [04]-[OWNERS]

Use root `README.md` for test dependency, target, tool, and output owners.
