---
name: cs-testing
description: >-
  Generates dense law-driven C# 14 test suites for the Rasm Rhino/Grasshopper
  monorepo via xUnit v3 + CsCheck property-based testing. Enforces a 175 LOC
  per-spec cap, 90% per-file coverage, generators routed through production
  factories, and oracle independence (no re-derivation of implementation).
  Use when writing/reviewing spec files, adding coverage, generating property
  tests, or scaffolding unit/algebra/snapshot tests for `.cs` modules.
  Companion to `coding-csharp` (production patterns) and `rhino-verify`
  (bridge-driven runtime evidence).
---

# [H1][CS-TESTING]
>**Dictum:** *Property-based laws are external oracles; mocks of the production type are not.*

<br>

Generate dense, law-driven test suites. Pure-managed logic exercises algebraic properties via CsCheck; anything that calls a native RhinoCommon function lives in the bridge rail (`*.verify.csx`). Density over quantity — few powerful properties multiplied by 100 generated cases per run, not many trivial assertions.

**Tasks:**
1. Classify rail via [section 4](#4category_routing) — static (CsCheck/xUnit) vs bridge (`rhino-verify`).
2. For static specs: read [->laws.md](references/laws.md), [->density.md](references/density.md).
3. For RhinoCommon-touching behavior: delegate to the `rhino-verify` skill.
4. Author spec from [->unit-pbt.spec.template.md](templates/unit-pbt.spec.template.md).
5. Verify oracle independence via [->guardrails.md](references/guardrails.md).
6. Validate against [section 8](#8validation).

**Stack:** xUnit v3 (`xunit.v3.mtp-off` for runnable test projects, `xunit.v3.assert` for `Rasm.TestKit` extensibility), CsCheck 4.7 (PBT + shrinking + `Sample`/`SampleMetamorphic`/`SampleParallel`/`SampleModelBased`), LanguageExt 5 (Fin/Validation/Eff under test), Thinktecture (smart constructors), coverlet.msbuild 6 (coverage configured in `Directory.Build.props`, opt-in via `/p:CollectCoverage=true`).

**References:**

| [INDEX] | [DOMAIN]   | [FILE]                                                              |
| :-----: | ---------- | ------------------------------------------------------------------- |
|   [1]   | Laws       | [laws.md](references/laws.md)                                       |
|   [2]   | Density    | [density.md](references/density.md)                                 |
|   [3]   | Categories | [categories.md](references/categories.md)                           |
|   [4]   | Guardrails | [guardrails.md](references/guardrails.md)                           |
|   [5]   | Template   | [unit-pbt.spec.template.md](templates/unit-pbt.spec.template.md)    |

---
## [1][WORKFLOW]
>**Dictum:** *Systematic workflow ensures complete, dense, non-circular tests.*

<br>

1. **Read source module.** Inventory smart constructors, `[Union]`/`[SmartEnum]` cases, `Fin`/`Validation`/`Eff` rails, and which calls reach into RhinoCommon native code.
2. **Classify rail.** If every code path stays in pure-managed CLR (no `Vector3d.IsTiny`, `Vector3d.VectorAngle`, `RhinoMath.UnitScale`, `Brep.*`, etc.), the spec is static. If any path crosses into native, route to `rhino-verify`.
3. **Select laws.** Walk the law taxonomy per export from [->laws.md](references/laws.md). Choose every law that applies; pack laws sharing arbitraries into one `[Fact]`.
4. **Design generators.** Place reusable arbitraries in `tests/csharp/_testkit/Gens.cs`; module-specific in a `static class <Module>Gens` at the top of the spec. **Generators MUST call production smart constructors** — never bypass `TryCreate`/`Validate`.
5. **Author spec from the template.** Sections in canonical order: `[CONSTANTS]`, `[ALGEBRAIC]`, optional `[EDGE_CASES]`.
6. **Verify oracle independence.** Walk [->guardrails.md](references/guardrails.md). Expected values come from algebraic laws, external math identities, or the spec's input — never from re-running the implementation under test.
7. **Run.** `bash scripts/test.sh` (whole solution) or `bash scripts/test.sh "FullyQualifiedName~Atoms"` (filtered).

---
## [2][HARD_CONSTRAINTS]
>**Dictum:** *Numeric thresholds are non-negotiable.*

<br>

[CRITICAL]:
- [ALWAYS] **175 LOC flat cap** per `.spec.cs` file.
- [ALWAYS] **90% per-file branch coverage** for pure-managed surfaces. Lower coverage means a missing law or a bridge-side behavior the static rail cannot reach.
- [ALWAYS] **One spec per source `.cs` file** under `tests/csharp/<project>/<MirrorPath>/<Source>.spec.cs`.
- [ALWAYS] **Generators route through production factories.** No parallel construction paths.
- [ALWAYS] **Same analyzer + editorconfig discipline as production** — `TreatWarningsAsErrors=true`, CSP rules, IDE0055 formatting, no relaxed gates.

| [INDEX] | [METRIC]          | [VALUE] | [NOTE]                                                  |
| :-----: | ----------------- | :-----: | ------------------------------------------------------- |
|   [1]   | Max file LOC      |   175   | Flat cap; split a spec into focused sub-areas if over.  |
|   [2]   | Branch coverage   |   90%   | Pure-managed surfaces; bridge code covered separately.  |
|   [3]   | Test method names |  Pascal | CA1707 enforces no underscores; `[Fact]` on own line.   |
|   [4]   | LOC density       |  ≥5 properties / 100 LOC | Pack laws via packed Sample callbacks. |

---
## [3][FILE_STRUCTURE]
>**Dictum:** *Canonical section order enables rapid navigation.*

<br>

**Imports:**
1. `Rasm.<Module>` — module(s) under test (qualify when test namespace collides; see [->guardrails.md](references/guardrails.md))
2. `Rasm.TestKit` — `Spec` + `Gens`
3. `Rhino` / `Rhino.Geometry` — only when the spec touches pure-managed RhinoCommon types (`Point3d`, `Vector3d` constants, `RhinoMath.IsValidDouble`)
4. `Xunit.Sdk` — `XunitException` for inline throws

**Section order** (omit unused):

```csharp
// --- [CONSTANTS] ----------------------------------------------------------------------------
// --- [ALGEBRAIC] ----------------------------------------------------------------------------
// --- [EDGE_CASES] ---------------------------------------------------------------------------
```

| [INDEX] | [SECTION]      | [PURPOSE]                                                       |
| :-----: | -------------- | --------------------------------------------------------------- |
|   [1]   | `[CONSTANTS]`  | Static-class generators routed through production `TryCreate`.  |
|   [2]   | `[ALGEBRAIC]`  | `sealed class <Module>Laws` per concept, `[Fact]` per property. |
|   [3]   | `[EDGE_CASES]` | Boundary samples (NaN/∞/empty) when not packed into algebra.    |

**Naming:**
- Spec file: `<Source>.spec.cs` mirroring source path (`Vectors/Atoms.cs` → `Vectors/Atoms.spec.cs`).
- Test class: `<TypeOrConcept>Laws` or `<TypeOrConcept>Props` (PascalCase).
- Test method: PascalCase, statement-form law (`ClosureRejectsZeroVector`, `RoundtripPreservesValue`).
- Generator class: `<Module>Gens` (static).

**Attributes:** `[Fact]` and `[Theory]` on their own line; method declaration follows. CA1707 + IDE0055 enforce this; do not co-locate.

---
## [4][CATEGORY_ROUTING]
>**Dictum:** *Native dependencies separate the rails.*

<br>

| [INDEX] | [RAIL]        | [LOCATION]                              | [WHAT_RUNS_HERE]                                                                                            | [TOOL]                                |
| :-----: | ------------- | --------------------------------------- | ----------------------------------------------------------------------------------------------------------- | ------------------------------------- |
|   [1]   | Static        | `tests/csharp/<project>/<MirrorPath>/`  | Smart constructors, fold algebras, SmartEnum cases, Union dispatch, Validation/Fin pipelines, pure math.    | xUnit v3 + CsCheck via `scripts/test.sh` |
|   [2]   | Bridge        | `apps/.../Scenarios/<name>.verify.csx`  | Anything touching RhinoCommon native: `Vector3d.IsTiny`/`IsValid`/`Unitize`, `Brep`/`NurbsCurve`, viewport. | RhinoCode via `scripts/rhino.sh verify` |

**Trigger to switch rails:** when a static test fails at `UnsafeNativeMethods.*` or `System.IO.FileNotFoundException : Could not load file or assembly 'RhinoCommon'`, the test must move to the bridge. Do not attempt to copy native dylibs into the test process.

[REFERENCE] Bridge authoring: invoke the `rhino-verify` skill. Static layout per project: [->categories.md](references/categories.md).

---
## [5][LAW_SELECTION]
>**Dictum:** *Laws define WHAT to test; density techniques define HOW.*

<br>

Walk the law taxonomy per export. Pack laws sharing the same arbitrary into one `[Fact]`.

**Core laws:** Closure (factory rejects invalid input), Roundtrip (`back ∘ forward = id`), Identity (`f(x) = x`), Idempotent (`f(f(x)) = f(x)`), Inverse (`g(f(x)) = x`), Commutative (`a∘b = b∘a`), Associative (`(a∘b)∘c = a∘(b∘c)`), Monotone (input order ⇒ output order).
**Equivalence:** Reflexive, Symmetric, Transitive (for `IEquatable<T>`).
**Domain invariants:** Smart constructor closure, DU exhaustive case coverage, SmartEnum key uniqueness, Fin rail propagation, Validation parallel accumulation.
**Statistical laws:** Mean ∈ [Min, Max], Variance ≥ 0, Count = |input|, constant sequence ⇒ zero variance.

[REFERENCE] Full law taxonomy with C# 14 code patterns: [->laws.md](references/laws.md).

---
## [6][DENSITY_SELECTION]
>**Dictum:** *Parametric generation multiplies coverage per LOC.*

<br>

**Top techniques (in priority order):**
1. **Single Sample, packed assertions** — multiple expectations inside one Action property body. Coverage multiplier: assertions × generated cases.
2. **`Gen.OneOfConst` over DU/SmartEnum cases** — exhaustive case enumeration, no enum drift.
3. **`gen.Select(gen)` tuple generators** — algebraic laws over pairs and triples.
4. **`Gen.Where` filtering** — refine domain (`NonZeroVec`, `Positive`).
5. **`Gen.OneOf` / `Gen.Frequency`** — weighted alternatives for parametric coverage.

[REFERENCE] CsCheck combinator catalog and packing recipes: [->density.md](references/density.md).

---
## [6.1][CSCHECK_API]
>**Dictum:** *Use CsCheck's full surface — combinators, shrinking, and OneOfConst.*

<br>

| [INDEX] | [API]                                | [WHEN_TO_USE]                                                                          | [BENEFIT]                                                |
| :-----: | ------------------------------------ | -------------------------------------------------------------------------------------- | -------------------------------------------------------- |
|   [1]   | `Gen.Double[start, finish]`          | Bounded double generator                                                               | Avoids NaN/∞ unless explicitly added                     |
|   [2]   | `Gen.Int[start, finish]`             | Bounded integer generator                                                              | Same shape                                               |
|   [3]   | `Gen.OneOfConst(a, b, c)`            | Enum/SmartEnum/DU case enumeration                                                     | Exhaustive coverage of bounded vocabularies              |
|   [4]   | `gen.Select(...)`                    | Mapping / projecting one generator                                                     | Composable, type-safe                                    |
|   [5]   | `g1.Select(g2)`                      | Tuple generator                                                                        | Pairs/triples for algebraic laws                         |
|   [6]   | `gen.Where(predicate)`               | Refine domain                                                                          | Filter to satisfy preconditions                          |
|   [7]   | `gen.Array[min, max]`                | Variable-length array generator                                                        | Collection-driven tests                                  |
|   [8]   | `gen.Sample(action)`                 | Run 100 cases (default), shrink on throw                                               | Property loop                                            |
|   [9]   | `gen.Sample(predicate)`              | bool-form property                                                                     | Idiomatic boolean laws                                   |
|  [10]   | `gen.Sample(..., seed: "…")`         | Replay a previously-shrunk failing case (the seed string in the failure message IS the persistence) | Regression pin without copy-paste literals |
|  [11]   | `gen.Sample(..., time: seconds)`     | Bound CsCheck sampling by wall-clock instead of iter count                              | Suits CI budgets where shrink cost varies                |
|  [12]   | `gen.Sample(..., threads: n)`        | Override CsCheck's per-sample thread pool (default `ProcessorCount`)                    | Determinism for cross-machine repro; CI = 1              |
|  [13]   | `Gen.Frequency(...)`                 | Weighted alternatives                                                                  | Bias case distribution                                   |
|  [14]   | `Gen.Recursive(...)`                 | Bounded-depth tree generation                                                          | Recursive structures (curves, breps)                     |
|  [15]   | `Gen.Shuffle(arr)`                   | Generator of permutations of a fixed array                                              | Powers `Spec.Permutation`; order-invariance laws         |
|  [16]   | `Spec.Metamorphic(gen, path, oracle)` | Assert SUT result equals an INDEPENDENT oracle path over the same input               | External oracle without snapshot files                   |
|  [17]   | `Spec.Permutation(gen, f, eq?)`      | `f(xs) = f(shuffle(xs))` — order invariance via `Gen.Shuffle`                          | Aggregations (Mean/Sum/Variance/Set ops)                 |
|  [18]   | `Spec.Monotone(orderedPair, proj, cmp?)` | `f(lo) ≤ f(hi)` over ordered pair generators                                       | Quantile/sort/projection laws                            |
|  [19]   | `gen.SampleMetamorphic(Gen<Metamorphic<T>>)` | Two-transformation MR (e.g. `f(x)` vs `f(T(x))` related by R)                  | Catches non-pointwise invariants                         |
|  [20]   | `gen.SampleModelBased(...ops)`       | State-machine PBT: actual vs reference-model after each `Operation<TActual, TModel>`   | Stateful APIs, mutable wrappers, caches                  |
|  [21]   | `gen.SampleParallel(...ops)`         | Concurrent execution; CsCheck shrinks failing interleavings                            | Linearizability of thread-safe APIs                      |
|  [22]   | `gen.Single(predicate, seed)`        | Find AND pin one sample matching predicate                                              | Single-case regression without re-running shrink         |
|  [23]   | `Check.Hash(action, expected, dp)`   | Hash-based regression with `.cs.cache` file alongside test source                       | Replaces golden-file fixture suites                      |
|  [24]   | `Spec.Regression(gen, action, seed)` | Pin a shrunk failing case under a stable name                                          | Reproducible regression evidence                         |
|  [25]   | `action.Faster(other, sigma: 6)`     | Paired-sample sigma comparison (`σ ≥ 6` ≈ 1-in-5×10⁸ false positive); use inside `[Fact(Explicit = true)]` | Statistical perf guard without BenchmarkDotNet           |

**Environment variable overrides (assembly-wide, no code edit):**

| [VAR]              | [OVERRIDES]                   | [USE]                                                              |
| ------------------ | ----------------------------- | ------------------------------------------------------------------ |
| `CsCheck_Iter`     | `iter:` (default 100)         | `CsCheck_Iter=10000 bash scripts/test.sh` — deep coverage on demand |
| `CsCheck_Time`     | `time:` (seconds)             | `CsCheck_Time=60 bash scripts/test.sh` — wall-clock budget         |
| `CsCheck_Seed`     | `seed:` (replay shrunk case)  | `CsCheck_Seed=<seed> bash scripts/test.sh "...~MyLaw"` — repro     |
| `CsCheck_Sigma`    | `sigma:` for `Faster` (def 6) | `CsCheck_Sigma=50 bash scripts/test.sh "...~PerfLaw"` — stricter   |
| `CsCheck_Threads`  | per-sample thread count       | `CsCheck_Threads=1 bash scripts/test.sh` — single-threaded CI run  |

[CRITICAL]:
- [NEVER] Use `gen.Sample(_ => { ... })` without returning `true` / asserting — the property runs but silently passes.
- [ALWAYS] Use `Spec.ForAll(gen, Action<T>)` from `Rasm.TestKit` to wrap properties — it adapts `Action` to CsCheck's `Func<T, bool>` contract.
- [ALWAYS] Use `Spec.Succ` / `Spec.Fail` for `Fin<T>` assertions — null-coalescing-guarded; no CA1062 suppression.
- [ALWAYS] Use `Spec.Metamorphic` when an external algorithm (LINQ, NIST vector, hand-derived formula) can compute the same result via an unrelated path. Metamorphic tests are the strongest defense against oracle-impl coupling AND against subtle language-precedence bugs (`Stats.spec.cs::MedianMatchesSortedMiddleOracle` localized a C# `switch` vs `*` precedence error that no hand-written assertion caught).
- [ALWAYS] Pair `Sample(threads: 1)` with `[CollectionDefinition(DisableParallelization = true)]` when the predicate body touches RhinoCommon static state.

---
## [6.2][XUNIT_V3_ADVANCED]
>**Dictum:** *Use v3's runtime surface — Skip, AssemblyFixture, TheoryData — over v2 workarounds.*

<br>

| [INDEX] | [API]                                                  | [WHEN_TO_USE]                                                                          |
| :-----: | ------------------------------------------------------ | -------------------------------------------------------------------------------------- |
|   [1]   | `Assert.Skip` / `SkipUnless` / `SkipWhen`              | Runtime skip when a precondition is unmet (e.g. native dep absent). Static-rail tests do NOT use this — bridge-rail-only behaviors do not belong in a static spec at all. |
|   [2]   | `[Fact(Explicit = true)]`                              | Opt-in tests (perf rails, exploratory). Excluded by default; enabled via `dotnet test --filter "Explicit=true"`. |
|   [3]   | `[Fact(Skip = "reason")]`                              | Static skip with rationale. Prefer `SkipUnless` over an indefinite hard skip.          |
|   [4]   | `[Fact(Timeout = ms)]`                                 | Bound runtime; pair with `TestContext.Current.CancellationToken` propagation.          |
|   [5]   | `TestContext.Current.CancellationToken`                | Thread cancellation into CsCheck Sample bodies so timeouts cancel mid-shrink.          |
|   [6]   | `TheoryData<T1..T10>` + collection-expression init     | Type-safe theory data: `public static TheoryData<int, int> Cases => [(1,2), (3,4)];`   |
|   [7]   | `IAsyncLifetime` / `IAsyncDisposable`                  | Per-class async setup/teardown. Prefer over constructor + IDisposable when fixtures are async. |
|   [8]   | `[assembly: AssemblyFixture(typeof(T))]` + `IAssemblyFixture<T>` | One-time process-wide fixture (e.g. bridge handle). v3-only; no v2 equivalent.         |
|   [9]   | `[assembly: CaptureConsole]` / `[assembly: CaptureTrace]` | Route stdout/Trace into per-test buffer. Replaces `Console.SetOut` shimming.           |
|  [10]   | `xunit.runner.json` generated into `obj/` per test project (source: `_XunitRunnerJsonContent` in `Directory.Build.props`) | Shared runtime config: `parallelAlgorithm: "conservative"`, `preEnumerateTheories: true`, `longRunningTestSeconds: 30`, `shadowCopy: false`. |

[CRITICAL]:
- [NEVER] Use MTP-only switches (`-filter` query language, `--list-tests` MTP variant) — this monorepo pins `xunit.v3.mtp-off` for VSTest stability + coverlet collector-compat.
- [NEVER] Reference `xunit.v3.mtp-off` from a non-executable assembly (`OutputType` != Exe). xUnit v3 rejects it with `xunit.v3.core.mtp-off.targets(15,5)`. For test-support libraries like `Rasm.TestKit`, set `IsTestKitProject=true` (via path under `tests/csharp/_testkit/`) — `Directory.Build.props` routes it to `xunit.v3.assert` instead.
- [ALWAYS] Add `[Fact(Explicit = true)]` to any perf rail using `Action.Faster(...)`. Standard test runs must not pay the comparison cost.

---
## [7][THESIS]
>**Dictum:** *Three pillars eliminate circular AI-generated tests.*

<br>

- **Algebraic laws as external oracles.** Roundtrip, idempotence, commutativity, associativity, monotonicity — these are mathematical truths independent of how the implementation was written. AI cannot fabricate a law that "happens to pass"; law correctness is provable independent of implementation.
- **Parametric generation multiplies coverage per LOC.** A single `Spec.ForAll(gen, property)` invocation runs 100 cases per default; one line replaces hundreds of hand-assertions with a universally-quantified property.
- **Generators routed through production factories.** Arbitraries call `TryCreate`/`Validate` on the same smart-constructor surface as production code. There is no parallel construction path that drifts under refactoring; shrinking finds the minimum counterexample at the exact boundary the implementation must honor.
- **The xUnit assertion is the EXCEPTION, not the call.** CsCheck's `Sample(predicate)` runs `predicate(value)`; predicate returns `true` for pass, throws for fail. `Spec.*` helpers wrap this with named law primitives that throw `XunitException` on mismatch. A spec body without visible `Assert.XXX(...)` calls is NOT missing assertions — the law helper IS the assertion. Inline `Assert` inside a `Sample` predicate is redundant.

| [INDEX] | [LAYER]                     | [MECHANISM]                                         |
| :-----: | --------------------------- | --------------------------------------------------- |
|   [1]   | Algebraic PBT               | Laws are external oracles by nature                 |
|   [2]   | Generator-via-factory       | Production smart constructor is the sole entry      |
|   [3]   | DU/SmartEnum case scan      | `Gen.OneOfConst` enumerates every case              |
|   [4]   | Analyzer + editorconfig     | Tests obey the same code-quality contract as src    |
|   [5]   | Two-rail separation         | Native deps land in `rhino-verify`, not static spec |

---
## [8][VALIDATION]
>**Dictum:** *Gates prevent non-compliant output.*

<br>

[VERIFY]:
- [ ] Spec ≤ 175 LOC; one spec per source `.cs`.
- [ ] PascalCase test method names; `[Fact]` on own line.
- [ ] Imports respect canonical order; no `using` for namespaces unused.
- [ ] Generators call production factories; no parallel construction.
- [ ] No re-derivation of expected values from implementation logic.
- [ ] No CA-rule suppressions on the spec; address the underlying analyzer signal.
- [ ] All tests in the spec pass: `bash scripts/test.sh "FullyQualifiedName~<TestClass>"`.
- [ ] No native RhinoCommon call paths exercised in static spec — those go to `rhino-verify`.
- [ ] Solution gate passes: `bash scripts/check-cs.sh check`.

**Commands:**

```bash
bash scripts/check-cs.sh check                                  # static analysis (no tests)
bash scripts/test.sh                                            # run all xUnit tests
bash scripts/test.sh "FullyQualifiedName~VectorAngleProps"      # filter to one class
dotnet test Workspace.slnx /p:CollectCoverage=true              # opt-in coverage via coverlet.msbuild
CsCheck_Seed=<seed> bash scripts/test.sh "FullyQualifiedName~X" # replay a shrunk seed without code edits
```

**Coverage measurement.** Configured in `Directory.Build.props` (Coverage PropertyGroup) for every `IsTestProject=true` assembly: `<CoverletOutputFormat>cobertura</CoverletOutputFormat>`, `<CoverletOutput>.artifacts/coverage/<project>/</CoverletOutput>`, `<Include>[Rasm*]*,[Foundation.CSharp.Analyzers]*,[Radyab]*</Include>`, `<Exclude>[*Tests]*,[*TestKit]*</Exclude>`, plus `SkipAutoProps`, `DeterministicReport`, generated-code attribute filters. Opt in via `dotnet test /p:CollectCoverage=true` — the test script itself stays unflagged. Output: per-project cobertura.xml under `.artifacts/coverage/`. A 0% reading on a production file in the static rail is a routing signal, not a defect: the file likely touches RhinoCommon native code and belongs in the bridge rail.

[REFERENCE] Detailed guardrails and anti-patterns: [->guardrails.md](references/guardrails.md).
