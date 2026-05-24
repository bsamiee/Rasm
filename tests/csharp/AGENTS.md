# [TESTING_MANIFEST]

[REQUIRED]: Follow `/Users/bardiasamiee/Documents/99.Github/Rasm/CLAUDE.md`, `cs-testing`, and `coding-csharp` for every `.spec.cs` change.

## [1][CANONICAL_RAIL]

- Use xUnit v3 + CsCheck through `/Users/bardiasamiee/Documents/99.Github/Rasm/tests/csharp/_testkit`.
- Read tool/API truth before using advanced features:
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/testing-libs/xunit/api.md`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/testing-libs/cscheck/api.md`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/testing-libs/coverlet/api.md`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/testing-libs/stryker/api.md`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/testing-libs/verify/api.md`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/testing-libs/archunit/api.md`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/testing-libs/benchmarkdotnet/api.md`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/testing-libs/sharpfuzz/api.md`
- Keep specs law-matrix shaped: each test should cover a behavior family, oracle, failure rail, or metamorphic relation.
- Static xUnit owns pure-managed behavior; native Rhino/GH behavior belongs in `*.verify.csx` bridge scenarios.
- Architecture tests own assembly dependency direction only; code-shape rules stay in `tools/cs-analyzer`.
- Benchmarks and fuzz harnesses are separate executable rails, not xUnit specs.
- Verify snapshots compare stable artifacts only; never snapshot current implementation output as a domain oracle.
- Prove non-zero VSTest discovery before using Stryker survivor data; if Stryker reports zero tests while `bash scripts/test.sh` finds tests, treat mutation output as tooling evidence, not code quality evidence.
- Generated reports, corpora, mutation output, benchmark output, and transient test results belong under `/Users/bardiasamiee/Documents/99.Github/Rasm/.artifacts`; do not create local scratch roots such as `.remember`.
- If bridge execution reports `LanguageExt.*` value-type mismatch or already-loaded assembly identity failures, investigate loaded Rhino packages/assemblies before changing the scenario or static spec.

## [2][ORACLES]

- Never assert implementation against itself. Expected values come from independent math, BCL/Rhino facts, fixture geometry, or metamorphic laws.
- Before changing a failing test, investigate whether product code is wrong. Tests are allowed to expose real bugs.
- Prefer variable-driven samples through `Spec.ForAll`, `Spec.Metamorphic`, and `Spec.Regression`.
- Use explicit seeds only to preserve a discovered regression; otherwise let `CsCheck_*` environment policy flow through `Spec`.
- Grade every expected-value path: prefer independent closed form, smaller model, or metamorphic relation; reject implementation mirrors.
- Treat Stryker survivors as missing oracle, equivalent mutant, bridge-owned path, or product bug before editing tests.
- Shape-only assertions are Grade D. They are acceptable only as supplemental dispatch proof when paired with at least one Grade A/B oracle or a durable Grade C failure/category rail.
- Good laws attack more than one dimension: construction, projection, unsupported output, failure category, receipt metadata, and a metamorphic or closed-form invariant should share the same generated sample whenever the source behavior allows it.
- Use deliberately distinct payload values in product generators so tests catch swapped fields, stale branches, and ignored input rather than only proving that "some value" survived.
- For numeric/domain code, prefer an independent scalar loop, small fixture geometry, or algebraic identity over reusing the production method with different spelling.
- A useful failing test is preserved by fixing the production owner first. Only weaken or delete the law after proving the expected value was circular or the behavior is bridge-owned.

## [3][TESTKIT]

- Use `Spec` for property execution, success/failure rails, validation rails, and numeric equality.
- Use `Gens` for shared edge-biased numeric, tolerance, dimension, unit interval, positive magnitude, and context inputs.
- Use `Numeric` for independent matrix/vector oracles; do not use production `Matrix` operators to compute expected values.
- Extend `_testkit` only when two or more specs will consume the abstraction. Testkit files may reach 350 LOC, but must stay polymorphic and dense.
- Do not add package versions as future intent. Every test tool package must have a concrete spec, benchmark, harness, or artifact owner.
- Promote only anti-circular value: reusable finite point sets, centroid/covariance/distance oracles, row-major residuals, Hermitian matvec/residuals, and stable serializers are valid candidates once multiple specs need them.
- Do not promote benchmark, fuzz, bridge, or one-off fixture adapters into `_testkit`; those rails have their own executable owners.

## [4][SUPPRESSIONS_AND_GATES]

- Spec-local generator classes stay non-public unless discovery requires public visibility.
- Do not add local xUnit/analyzer suppressions for style friction. Adjust folder-wide policy only when the rule conflicts with discovery or generated-code reality.
- Validation ladder for test changes:
  - `dotnet build /Users/bardiasamiee/Documents/99.Github/Rasm/tests/csharp/libs/Rasm/Rasm.Tests.csproj --configuration Debug --no-restore`
  - `bash /Users/bardiasamiee/Documents/99.Github/Rasm/scripts/test.sh`
  - `TEST_TARGET=/Users/bardiasamiee/Documents/99.Github/Rasm/tests/csharp/libs/Rasm.Grasshopper/Rasm.Grasshopper.Tests.csproj bash /Users/bardiasamiee/Documents/99.Github/Rasm/scripts/test.sh` when GH2 managed specs changed.
  - `TEST_TARGET=/Users/bardiasamiee/Documents/99.Github/Rasm/tests/csharp/libs/Rasm.Rhino/Rasm.Rhino.Tests.csproj bash /Users/bardiasamiee/Documents/99.Github/Rasm/scripts/test.sh` when Rhino managed specs changed.
  - `dotnet test /Users/bardiasamiee/Documents/99.Github/Rasm/tests/csharp/_architecture/Rasm.Architecture.Tests.csproj --configuration Debug` when architecture laws changed.
  - `dotnet test /Users/bardiasamiee/Documents/99.Github/Rasm/tests/csharp/_tooling/Rasm.TestingTools.Tests.csproj --configuration Release` when stable testing-rail snapshots changed.
  - `dotnet run --project /Users/bardiasamiee/Documents/99.Github/Rasm/tests/csharp/_benchmarks/Rasm.Benchmarks.csproj --configuration Release -- --list flat` when benchmark projects or config changed.
  - `printf 'running,1000,240,0,modular,9.525,9.525,concave' | dotnet run --project /Users/bardiasamiee/Documents/99.Github/Rasm/tests/csharp/_fuzz/Rasm.Fuzz.csproj --configuration Release` when fuzz harnesses changed.
  - `bash /Users/bardiasamiee/Documents/99.Github/Rasm/scripts/rhino.sh verify /Users/bardiasamiee/Documents/99.Github/Rasm/apps/grasshopper/Radyab/Scenarios` when bridge scenarios changed.
  - `git diff --check`
