---
name: testing-cs
description: >-
  Builds and reviews dense adversarial law-matrix C# specs using xUnit v3, CsCheck,
  the active repo testkit, coverlet, Stryker, Verify, ArchUnitNET, BenchmarkDotNet,
  and host/runtime scenarios. Use when creating, refactoring, or validating C#
  tests, testkit code, test docs, benchmark harnesses, or runtime verification.
---

# [H1][TESTING_CS]
>**Dictum:** *A test is an adversarial law with an independent oracle, not confirmation of current output.*

<br>

Use this skill with `coding-csharp` for `.cs` specs/testkit code, `coding-bash` for scripts, and repository documentation standards for Markdown. The canonical unit rail is xUnit v3/MTP + CsCheck + the active repo testkit; host-native runtime behavior belongs in the repo-declared runtime scenario rail. Managed tests attempt falsification through independent oracles, mutation-visible rows, failure categories, model/metamorphic relations, receipt tampering, and raw-payload laws.

---
## [01]-[WORKFLOW]
>**Dictum:** *Classify first; falsify with fewer, stronger laws second.*

<br>

1. Read the owning production file and its sibling tests.
2. Classify each behavior as static-managed or runtime-owned native/host behavior.
3. Inventory public APIs, union/SmartEnum cases, failure categories, output projections, native calls, and receipt metadata before writing assertions.
4. Select laws from [oracles-laws.md](references/oracles-laws.md) and density axes from [density-axes.md](references/density-axes.md).
5. Reuse active repo testkit contracts from [testkit.md](references/testkit.md).
6. Verify unfamiliar tool APIs against the shipped package surface before use; never write from recall.
7. Check the repo-declared platform/API owner before changing serializers, fuzz parsers, runtime probes, host loaders, filesystem evidence, capture code, or standard-library API usage in tests.
8. Write the spec from [unit-pbt.spec.template.md](templates/unit-pbt.spec.template.md).
9. Validate with scoped cleanup first, then targeted build/test proof for touched rails.

---
## [02]-[RAILS]
>**Dictum:** *Runtime ownership is executable proof, not a documentation escape hatch.*

<br>

| [INDEX] | [RAIL]           | [LOCATION]                                                  | [OWNS]                                                                                                           |
| :-----: | ---------------- | ----------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------- |
|  [01]   | Static spec      | `tests/csharp/libs/<Project>/<MirrorPath>/<Source>.spec.cs` | Pure managed constructors, smart enums, unions, matrix/math rails, `Fin`/`Validation`, deterministic algorithms. |
|  [02]   | Testkit          | `tests/csharp/_testkit` (`Rasm.TestKit`, host-free)         | Law algebra, reusable generators, numeric oracles, seam probes, manifest census, law-coverage gate.              |
|  [03]   | Scenario SDK     | `tests/csharp/_scenariokit` (`Rasm.ScenarioKit`)            | Host-aware scenario attribute, evidence context, document scope, and viewport capture surfaces.                  |
|  [04]   | Runtime scenario | `tests/csharp/scenarios` (`Rasm.Scenarios`)                 | Host runtime APIs, native validity, UI marshaling, document/canvas behavior proven through bridge evidence.      |
|  [05]   | Mutation         | root `stryker-config.json` via the repo mutation route  | Explicit Stryker MTP survivor discovery over the solution; zero discovery fails the rail.                        |
|  [06]   | Architecture     | `tests/csharp/_architecture`                                | Assembly dependency/cycle laws, manifest census laws, and infra-primitive falsification proving both kits.      |
|  [07]   | Benchmark        | `tests/csharp/_benchmarks`                                  | Managed hot-path measurement and the regression gate verb outside xUnit.                                         |

[CRITICAL]:
- Do not move native behavior into static xUnit just to improve coverage.
- Do not document a runtime gap when an executable scenario can own it.
- Do not add test-specific shared helpers. Promote only universal higher-order capability with at least two real consumers.
- Treat testkit additions as law/oracle/generator functionality, not extraction. Shared code must replace repeated spec logic with stronger evidence, not shorter spelling.
- Treat shape-only assertions as Grade D unless paired with a Grade A/B oracle or a durable Grade C failure/category rail.
- Delete or replace shape-only assertions that only inspect values constructed inside the same test.
- Pair every declared `Output`/accepted `TOut` surface with an actual projection law. Metadata-only output checks do not prove dispatch.
- Public union cases, record structs, and policy records that bypass factory methods need default-invalid/raw-payload laws or must be made unconstructable by design.
- Mixed guards must preserve categories: invalid input stays `Input`/`Tolerance`; unsupported capability stays `Unsupported`.
- Paired raw/domain outputs both need laws, for example `Vector3d` and `Direction`, receipt and geometry, scalar and value-object projections.
- Do not use `.IsSucc`, `.IsFail`, `.IsSome`, or `.IsNone` as primary proof; use `Spec.Succ`, `Spec.FailCategory`, `Spec.Valid`, `Spec.Invalid`, `Spec.Some`, or `Spec.None`.
- Classify each Stryker survivor before test edits: missing oracle, equivalent mutant, runtime-owned path, or product bug.
- Keep `ConcurrentDictionary`, `Interlocked`, `Directory`, `Path`, `Console.WriteLine`, and platform drawing APIs inside explicit test/tool/runtime boundary adapters.
- If a repo declares a static-vs-runtime classification list, treat it as the owner for that surface; static specs own pure managed laws and managed input guards, while native success belongs in runtime scenarios.

---
## [03]-[SCENARIOS]
>**Dictum:** *Scenario files are source-only runtime laws owned by tests.*

<br>

Scenario content lives in `tests/csharp/scenarios`; the SDK is `Rasm.ScenarioKit` in `tests/csharp/_scenariokit`. A scenario is a static `Fin<Unit>(ScenarioContext)` entrypoint carrying `[RhinoScenario(theme)]` with optional `Requires`/`BudgetMs`; discovery reads the attribute by full name over staged assemblies — there is no pre-host registry, so the attribute name and member signatures are frozen wire law.

`ScenarioContext` is the one evidence channel: `Require`/`Expect` for asserted facts, `Note` and the manifest writers (`ObjectManifest`/`GeometryManifest`/`ViewportManifest`/`Gh2CanvasManifest`) for observations, `Certify` for reference evidence (one generic verb over typed and raw `JsonElement` actuals; admission stays supervisor-decided), `Case` for named sub-case status, `Scratch`/`Stamp` for deterministic paths, and `Artifact` for capture registration. `DocumentScope` owns document lifecycle — leaked scopes are drained and reported — and `Capture.Snapshot` owns viewport captures.

[CRITICAL]:
- Keep scenarios deterministic: open only the document state they own through `DocumentScope`, use constants instead of randomness/time/I/O, redraw before capture, and add a `ctx.Fact`/`ctx.Note` for every runtime fact that would be hard to infer from the failure alone.
- Do not bypass `ScenarioContext` with ad hoc fact emitters or `Console.WriteLine("key=value")` evidence lines; the context-owned fact keys are the canonical evidence wire format the supervisor folds.
- Do not add `#r`, `#load`, absolute build-output paths, legacy bridge job files, or retired script-server flow.
- Do not treat missing endpoint metadata as missing scenario data. Use the repo-declared runtime health command.
- Do not propose headless, containerized, or simulated host substitutes for the live host scenario rail.
- Capture paths live under the repo-owned runtime artifact directory.
- When a scenario uses viewport evidence, set the active view and redraw before capture.

---
## [04]-[SPEC_SHAPE]
>**Dictum:** *One generated input should exercise many assertions.*

<br>

| [INDEX] | [RULE]            | [DETAIL]                                                                                                                                                                                     |
| :-----: | ----------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | LOC               | Target 175 LOC per normal spec; 176-225 when one owning source file has multiple real concepts and the result is denser than a split. Testkit files may reach 350 LOC.                       |
|  [02]   | Layout            | Imports, namespace, `[CONSTANTS]`, `[ALGEBRAIC]`, optional `[EDGE_CASES]`.                                                                                                                   |
|  [03]   | Classes           | Public xUnit test classes; spec-local generator classes stay non-public unless discovery requires public visibility.                                                                         |
|  [04]   | Attributes        | `[Fact]`/`[Theory]` on their own line. No local analyzer suppressions for normal test shape.                                                                                                 |
|  [05]   | Generators        | Route reusable value-object generators through production factories. Avoid parallel constructors. Use `Gen.Where(...).Select(...)`, never `Select+throw` — `throw` breaks CsCheck shrinking. |
|  [06]   | Names             | PascalCase law names; no underscores.                                                                                                                                                        |
|  [07]   | Polymorphic-first | Before writing a second `[Fact]` that shares setup with an existing one, reach for a polymorphic pattern from [density-axes.md `[4][POLYMORPHIC_PATTERNS]`](references/density-axes.md).     |

[CRITICAL]:
- A spec is not strong because it has many facts. It is strong when one generated domain attacks construction, projection, unsupported outputs, failure categories, receipt invariants, and an independent oracle without mirroring production code.
- A real oracle predicts behavior from an independent source: closed-form math, conservation, fixture geometry, category contract, runtime observation, or documented external behavior. A law varies a behavior family enough to catch swapped inputs, missing validation, unsupported outputs, and receipt drift.
- Use distinct generated payload values when a source function transports or dispatches multiple inputs. Equal or placeholder payloads hide swaps and ignored branches.
- Allow 225 LOC, and exceptionally 300 LOC, only when each added line buys a real oracle, boundary, runtime classification, or product-bug guard that cannot be compressed through the project testkit, local arrays, or product generators. Specs that exceed 225 LOC must document the per-section LOC delta justifying the overage.
- If a failing law reveals a real production bug, fix the owner. Do not weaken the law into shape-only proof.
- Hand-constructing a record type and asserting its own fields is Grade D mirror coverage. Migrate to a runtime scenario OR add an `IsValid` predicate law via `TheoryData<Record, bool>` (one valid row, each invariant individually broken).
- Filter `Spec.ForAll(Gen.OneOfConst([A,B,C]), ...)` shows as ONE Stryker mutation target. Convert to `[Theory][InlineData(A)][InlineData(B)][InlineData(C)]` (or `MemberData(...)` from `SmartEnum.Items`) when Stryker survivors include per-case logic — Theory rows give N separately-killable targets.

---
## [05]-[TOOL_RAIL]
>**Dictum:** *One admitted tool per concern; specs use the small contract.*

<br>

| [INDEX] | [TOOL]          | [LOCAL_USE]                                                                                                |
| :-----: | --------------- | ----------------------------------------------------------------------------------------------------------- |
|  [01]   | xUnit v3        | MTP rail, assertions, fixtures, `TheoryData<T1..T15>`, `TheoryDataRow`, `MatrixTheoryData`, runner JSON.    |
|  [02]   | CsCheck         | `Gen`/`Sample`, shrink/replay, classify, chi-squared, model-based, parallel, regression-hash, `Faster`.     |
|  [03]   | coverlet.MTP    | Opt-in managed coverage map.                                                                                 |
|  [04]   | Stryker.NET     | Explicit MTP mutation over the solution through the root `stryker-config.json`.                               |
|  [05]   | Verify          | Stable artifact snapshots only; `Verify.DiffPlex` renders string diffs beside `Verify.XunitV3` consumers.   |
|  [06]   | ArchUnitNET     | Assembly boundary laws.                                                                                      |
|  [07]   | BenchmarkDotNet | Benchmark console project with the regression gate verb.                                                     |

<br>

**xUnit v3 surfaces the specs lean on:**

- `TheoryDataRow<T1..>` carries per-row metadata — traits (`WithTrait`), timeout (`WithTimeout`), skip (`WithSkip`), explicit marking, and display name — through fluent construction or property initializers; `MemberData` providers may return rows, tuples, or `object[]`, sync or async.
- `Assert.Skip(reason)`, `Assert.SkipUnless(condition, reason)`, and `Assert.SkipWhen(condition, reason)` skip dynamically at runtime; prefer them over conditional early returns that report false green.
- `[Fact(Explicit = true)]` / `[Theory(Explicit = true)]` tests never run by default and report as not-run until a runner explicitly requests them — the lane for expensive or environment-gated laws.
- `[assembly: TestPipelineStartup(typeof(T))]` with `T : ITestPipelineStartup` owns whole-run initialization/teardown around the test pipeline.
- `MatrixTheoryData<T1, T2>(axis1, axis2)` builds the combinatorial `TheoryData` product from per-axis value sets.
- `TestContext.Current` is the ambient test state; pass `TestContext.Current.CancellationToken` into every cancellation-accepting call — `Spec.ForAll` already threads it into CsCheck sample loops.

**CsCheck surfaces the rebuilt `Spec` wraps:**

- Classify tables: `gen.Sample(classify, writeLine, ...)` prints a per-class count/percent distribution table — `Spec.Classified`.
- `Check.ChiSquared(expected, actual)` gates generator/bucket distributions statistically — `Spec.Distributed`.
- `SampleModelBased(operations, equal, ...)` threads an operation sequence over an actual/model pair — `Spec.ModelBased`.
- `SampleParallel(operations[, equal])` runs operations concurrently against shared state, accepts any valid linearization, and shrinks failing interleavings — `Spec.Parallel`.
- `SampleMetamorphic` proves two mutation paths land in equal states — `Spec.DualPath`.
- `Check.Hash(h => { h.Add(...); }, expected, decimalPlaces)` pins a regression hash over many computed values; `expected: 0` discovers the hash on first run, and a later mismatch reports exactly which cached value changed.
- `Gen.Faster`/`Check.Faster` statistically compare two implementations (binomial sigma test) — a spot performance law, never a BenchmarkDotNet replacement.

Current local facts:

- Runner configuration is repo-owned; do not invent root runner files.
- There is no user-facing `IAssemblyFixture<T>` API; use `[assembly: AssemblyFixture(...)]` plus constructor injection.
- xUnit typed theory rows/data exist through arity 15.
- `Spec.Metamorphic` is a relation-table law family over `Spec.ForAll`, not CsCheck `SampleMetamorphic`; the dual-path CsCheck form is `Spec.DualPath`.
- Treat Stryker zero discovery as a failed mutation rail; thresholds live in the root `stryker-config.json`, never in specs or prose.

---
## [06]-[VALIDATION]
>**Dictum:** *A failing law is evidence; investigate product behavior before weakening the test.*

<br>

Use the repo-declared gate appropriate to the touched surface:

```bash
<quality-router> static fix <touched-paths>
<quality-router> static build <touched-paths>
<test-runner> [<filter>] [--target <test-project>]
<runtime-runner> verify <scenario-or-glob>
```

Default test runs execute unit tests only unless the repo quality router declares otherwise. Use explicit mutation flags for Stryker on the configured managed pair.

Runtime ownership proof:

```bash
<runtime-runner> check <source.cs> <scenario>
```

Runtime scenarios are source-only unless the repo scenario owner declares otherwise. Do not add `#r`, `#load`, or absolute build-output paths; the runtime rail owns reference projection and fresh artifact refs.
