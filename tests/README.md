# [TESTS]

## [01]-[CLASSIFICATION]

Classify each test by scope, technique, and execution mode, and apply every classification that fits.

| [INDEX] | [AXIS]    | [VALUE]        | [DEFINITION]                           | [ROUTE]                                                    |
| :-----: | :-------- | :------------- | :------------------------------------- | :--------------------------------------------------------- |
|  [01]   | Scope     | Unit           | Behavior with controlled collaborators | Default `test` run per language                            |
|  [02]   | Scope     | Integration    | Real components or external boundary   | `test:integration` per project, Python `network` marker    |
|  [03]   | Scope     | Component      | Component rendered in a browser        | Vitest browser project and Storybook stories in `test`     |
|  [04]   | Scope     | End-to-end     | Served app driven through a browser    | Playwright `e2e` target                                    |
|  [05]   | Technique | Property-based | Generated examples exercise invariants | `ForAll`, `@given`, `it.effect.prop`, `#[property_test]`   |
|  [06]   | Mode      | Benchmark      | Timing outside functional test session | `benchmark` fixture, `*.bench.ts` file, `[[bench]]` target |

Python `@property_test(subject)` records public-API coverage.

## [02]-[EXPECTED_BEHAVIOR]

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

## [03]-[OWNERS]

Use root `README.md` for test dependency, target, tool, and output owners.
