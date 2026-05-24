# [TESTING_MANIFEST]

[REQUIRED]: Follow `/Users/bardiasamiee/Documents/99.Github/Rasm/CLAUDE.md`, `cs-testing`, and `coding-csharp` for every `.spec.cs` change.

## [1][CANONICAL_RAIL]

- Use xUnit v3 + CsCheck through `/Users/bardiasamiee/Documents/99.Github/Rasm/tests/csharp/_testkit`.
- Read tool/API truth before using advanced features:
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/testing-libs/xunit/api.md`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/testing-libs/cscheck/api.md`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/testing-libs/coverlet/api.md`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/testing-libs/stryker/api.md`
- Keep specs law-matrix shaped: each test should cover a behavior family, oracle, failure rail, or metamorphic relation.
- Static xUnit owns pure-managed behavior; native Rhino/GH behavior belongs in `*.verify.csx` bridge scenarios.
- If bridge execution reports `LanguageExt.*` value-type mismatch or already-loaded assembly identity failures, investigate loaded Rhino packages/assemblies before changing the scenario or static spec.

## [2][ORACLES]

- Never assert implementation against itself. Expected values come from independent math, BCL/Rhino facts, fixture geometry, or metamorphic laws.
- Before changing a failing test, investigate whether product code is wrong. Tests are allowed to expose real bugs.
- Prefer variable-driven samples through `Spec.ForAll`, `Spec.Metamorphic`, and `Spec.Regression`.
- Use explicit seeds only to preserve a discovered regression; otherwise let `CsCheck_*` environment policy flow through `Spec`.

## [3][TESTKIT]

- Use `Spec` for property execution, success/failure rails, validation rails, and numeric equality.
- Use `Gens` for shared edge-biased numeric, tolerance, dimension, unit interval, positive magnitude, and context inputs.
- Use `Numeric` for independent matrix/vector oracles; do not use production `Matrix` operators to compute expected values.
- Extend `_testkit` only when two or more specs will consume the abstraction. Testkit files may reach 350 LOC, but must stay polymorphic and dense.

## [4][SUPPRESSIONS_AND_GATES]

- Spec-local generator classes stay non-public unless discovery requires public visibility.
- Do not add local xUnit/analyzer suppressions for style friction. Adjust folder-wide policy only when the rule conflicts with discovery or generated-code reality.
- Validation ladder for test changes:
  - `dotnet build /Users/bardiasamiee/Documents/99.Github/Rasm/tests/csharp/libs/Rasm/Rasm.Tests.csproj --configuration Debug --no-restore`
  - `bash /Users/bardiasamiee/Documents/99.Github/Rasm/scripts/test.sh`
  - `bash /Users/bardiasamiee/Documents/99.Github/Rasm/scripts/rhino.sh verify /Users/bardiasamiee/Documents/99.Github/Rasm/apps/grasshopper/Radyab/Scenarios` when bridge scenarios changed.
  - `git diff --check`
