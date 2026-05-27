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
- Build dense laws before adding facts: one generated sample should attack construction, projection, unsupported output, category, receipt, and an independent oracle when the owner exposes those axes.
- Static xUnit owns pure-managed behavior; native Rhino/GH behavior belongs in `*.verify.csx` bridge scenarios.
- Architecture tests own assembly dependency direction only; code-shape rules stay in `tools/cs-analyzer`.
- Benchmarks and fuzz harnesses are separate executable rails, not xUnit specs.
- Verify snapshots compare stable artifacts only; never snapshot current implementation output as a domain oracle.
- Prove non-zero VSTest discovery before using Stryker survivor data; if Stryker reports zero tests while `bash scripts/test.sh` finds tests, treat mutation output as tooling evidence, not code quality evidence.
- Generated reports, corpora, mutation output, benchmark output, and transient test results belong under `/Users/bardiasamiee/Documents/99.Github/Rasm/.artifacts`; do not create local scratch roots such as `.remember`.
- If bridge execution reports `LanguageExt.*` value-type mismatch, `HashableResolve`/`OrdDefault` type-initializer failures, or already-loaded assembly identity failures, first verify `bridge check` is using staged `refs/<content-hash>/` paths, dependency-first `#r` order (`FSharp.Core` → `LanguageExt.Core` → transitive packages → `Rasm.dll` → target last), and the bridge-owned LanguageExt bootstrap before changing the scenario or static spec.

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
- A real oracle predicts behavior from another source of truth: closed-form math, conservation law, fixture geometry, category contract, external documented runtime behavior, or a bridge observation. A law states an invariant over a behavior family and varies enough axes to catch swapped inputs, missing validation, unsupported outputs, and receipt drift.
- A useful failing test is preserved by fixing the production owner first. Only weaken or delete the law after proving the expected value was circular or the behavior is bridge-owned.

## [3][TESTKIT]

- Use `Spec` for property execution, success/failure rails, validation rails, and numeric equality.
- Use `Gens` for shared edge-biased numeric, tolerance, dimension, unit interval, positive magnitude, and context inputs.
- Use `Numeric` for independent matrix/vector oracles; do not use production `Matrix` operators to compute expected values.
- Extend `_testkit` only when two or more specs will consume the abstraction. Testkit files may reach 350 LOC, but must stay polymorphic and dense.
- Do not add package versions as future intent. Every test tool package must have a concrete spec, benchmark, harness, or artifact owner.
- Promote only anti-circular value: reusable finite point sets, centroid/covariance/distance oracles, row-major residuals, Hermitian matvec/residuals, and stable serializers are valid candidates once multiple specs need them.
- Treat `_testkit` additions as higher-order testing capability, not extraction. New surface should replace repeated spec logic with a stronger oracle, generator, serializer, or law adapter that immediately improves multiple owners.
- Do not promote benchmark, fuzz, bridge, or one-off fixture adapters into `_testkit`; those rails have their own executable owners.
- Filtered generators must use `Gen.Where(...).Select(...)` or `Gen.Select(Try.lift).Where(IsSucc).Select(Get)` — never `Select+throw`. The `throw` form breaks CsCheck shrinking (`CsCheck_WhereLimit` exhausts at 100 with no useful message); the `Where` form preserves shrink minimality. Every factory-routed value-object generator follows this rule.
- The canonical `AcceptValue` extension hook for Rasm-defined types is the `IDomainValid` interface registered through `OpAcceptance.ValueValidity` reflection cache (see [[feedback_acceptvalue_validity_gap]]). New record/struct types that need `op.AcceptValue` support implement `IDomainValid { bool IsValid { get; } }` — do not extend the manual `ValidityOf` switch arm-by-arm.
- Shared assembly context belongs in `[assembly: AssemblyFixture(typeof(T))]` plus constructor injection (xUnit v3 has no `IAssemblyFixture<T>` API). When ≥3 specs share the same `static readonly Context Model = Spec.SuccValue(Context.Of(...).ToFin())` block, promote to an `AssemblyFixture`-routed value.

## [5][ANTI_PATTERNS]

- **Stub-receipt construction**: hand-building a record type (e.g., `IsoSurfaceReceipt`, `TopologyReceipt`, `SignedHeatReceipt`) then asserting its own fields is Grade D mirror coverage — the assertion reads what the test just wrote. Delete and migrate to a bridge scenario, OR add an `IsValid` predicate law that covers the conjunctive invariant via `TheoryData<Receipt, bool>` rows (one valid, each invariant individually broken).
- **Filtered-generator `throw`**: `Gen.Int.Select(i => i > 0 ? new T(i) : throw new InvalidOperationException(...))` exhausts CsCheck's where-limit silently. Use `Gen.Int.Where(i => i > 0).Select(i => new T(i))`.
- **Single-fixture distinct-detection blind spot**: a test that uses `[1.0, 1.0, 1.0]` cannot detect a swap between any two slots. Distinct-value product generators (`[7.0, 13.0, 3.0]`) are the minimum bar for swap-detecting laws.
- **Hoisted `Op key` across `Switch` arms**: every `[Union].Switch` arm constructs its own `Op.Of(name: nameof(CaseName))` for diagnostic provenance (see [[feedback_per_arm_op_provenance]]). The Rasm analyzer enforces this via CSP0801; do not suppress.
- **`TestContext.Current` ignored in long ForAll bodies**: every `Spec.ForAll`/`Spec.Cases`/`Spec.Metamorphic` should propagate `TestContext.Current.CancellationToken` into the body. The `Spec.*` adapters do this automatically — raw `Check.Sample` calls do not.
- **Spec.ForAll(Gen.OneOfConst([A,B,C]))** is ONE Stryker mutation target. Converting to `[Theory][InlineData(A)][InlineData(B)][InlineData(C)]` (or `MemberData(...)` populated from `SmartEnum.Items`) gives N mutation targets, each individually killable. Convert when Stryker survivors include uncovered SmartEnum/Union cases.

## [6][BRIDGE_RAIL_OPERATIONS]

- Each `bash scripts/rhino.sh verify <scenario>` invocation pays a 3-8s Rhino handshake. Group thematically related scenarios per `.verify.csx` file (e.g. `vectors-mesh-topology-and-validity.verify.csx` bundles topology census + naked-edge + validity guards) to amortize the handshake ~4×.
- Populate runtime evidence inside the `Scenario.Run(theme, capturePath, (key, facts) => { … })` body via `facts.Add(string key, object value);` statements. The harness emits one `facts={json}` plain line plus one `rasm.rhino-bridge.evidence=facts={json}` marker on scope exit — that batched dictionary is the durable runtime fact channel; exception messages alone make failed scenarios hard to triage. Do not call `BridgeMarker.EmitFact`/`EmitScenarioHeader` — those public emitters were dropped during the protocol-surface tightening.
- Grasshopper-aware scenarios receive bridge-owned `ScenarioHostUsings` (`Eto.Drawing`, `LanguageExt`) after the scenario preamble — host assemblies stay off `#r`; add explicit `using Grasshopper2.*` in scenario source only when a rail needs GH2 types. Do not add production icon/motion shims.
- Prefer `Probe.ExpectCase`, `Probe.ExpectRejectedContains`, and `FactBag.AddIfSome` over duplicated `switch`/`Match` boilerplate in GH UI scenarios.
- If a scenario passes locally but fails in CI, first check loaded RhinoCommon/Grasshopper assembly identity (`bridge doctor` output) before changing the scenario. Host-package collisions are evidence, not noise.

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
  - `bash /Users/bardiasamiee/Documents/99.Github/Rasm/scripts/rhino.sh verify /Users/bardiasamiee/Documents/99.Github/Rasm/tests/csharp/libs/Rasm/Vectors/scenarios` when vector bridge scenarios changed; scenarios must not contain `#r` or `#load`.
  - `bash /Users/bardiasamiee/Documents/99.Github/Rasm/scripts/rhino.sh verify /Users/bardiasamiee/Documents/99.Github/Rasm/tests/csharp/libs/Rasm.Grasshopper/UI/scenarios` when GH UI bridge scenarios changed; scenarios must not contain `#r` or `#load`.
  - `git diff --check`
