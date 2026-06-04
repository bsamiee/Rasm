---
name: testing-cs
description: >-
  Builds and reviews dense adversarial law-matrix C# specs for Rasm using xUnit v3, CsCheck,
  Rasm.TestKit, coverlet, Stryker, Verify, ArchUnitNET, BenchmarkDotNet,
  SharpFuzz, and Rhino/GH bridge scenarios. Use when creating, refactoring, or
  validating C# tests, testkit code, test docs, benchmark/fuzz harnesses, or
  bridge-owned runtime verification for Rasm.
---

# [H1][TESTING_CS]
>**Dictum:** *A test is an adversarial law with an independent oracle, not confirmation of current output.*

<br>

Use this skill with `coding-csharp` for `.cs` specs/testkit code, `coding-bash` for scripts, and `docs/standards` for Markdown. The canonical unit rail is xUnit v3/MTP + CsCheck + `Rasm.TestKit`; native Rhino/GH runtime behavior belongs in `*.verify.csx` bridge scenarios. Managed tests attempt falsification through independent oracles, mutation-visible rows, failure categories, model/metamorphic relations, receipt tampering, and raw-payload laws.

---
## [1][WORKFLOW]
>**Dictum:** *Classify first; falsify with fewer, stronger laws second.*

<br>

1. Read the owning production file and its sibling tests.
2. Classify each behavior as static-managed or bridge-owned native/runtime.
3. Inventory public APIs, union/SmartEnum cases, failure categories, output projections, native calls, and receipt metadata before writing assertions.
4. Select laws from [oracles-laws.md](references/oracles-laws.md) and density axes from [density-axes.md](references/density-axes.md).
5. Reuse `Rasm.TestKit` contracts from [testkit.md](references/testkit.md).
6. Check raw tool API in `docs/testing-libs` before using unfamiliar APIs.
7. Check `docs/system-api-map` before changing serializers, fuzz parsers, bridge probes, host loaders, filesystem evidence, capture code, or System API usage in tests.
8. Write the spec from [unit-pbt.spec.template.md](templates/unit-pbt.spec.template.md).
9. Validate with scoped cleanup first, then targeted build/test proof for touched rails.

---
## [2][RAILS]
>**Dictum:** *Bridge ownership is executable proof, not a documentation escape hatch.*

<br>

| [INDEX] | [RAIL] | [LOCATION] | [OWNS] |
| :-----: | ------ | ---------- | ------ |
| [1] | Static spec | `tests/csharp/libs/<Project>/<MirrorPath>/<Source>.spec.cs` | Pure managed constructors, smart enums, unions, matrix/math rails, `Fin`/`Validation`, deterministic algorithms. |
| [2] | Testkit | `tests/csharp/_testkit` | Universal law adapters, reusable generators, independent numeric oracles, serializers. |
| [3] | Bridge scenario | `tests/csharp/libs/<Project>/<MirrorPath>/scenarios/*.verify.csx` | RhinoCommon/GH runtime APIs, native geometry validity, UI marshaling, document/canvas behavior. |
| [4] | Mutation | `tools.quality test run --mutation changed|full` (default managed target) | Explicit Stryker MTP survivor discovery for `libs/csharp/Rasm` and its tests. |
| [5] | Architecture | `tests/csharp/_architecture` | Assembly dependency direction and cycle laws. |
| [6] | Tooling snapshot | `tests/csharp/_tooling` | Stable generated/config artifacts through Verify. |
| [7] | Benchmark | `tests/csharp/_benchmarks` | Managed hot-path measurement outside xUnit. |
| [8] | Fuzz | `tests/csharp/_fuzz` | Pure managed parser/token harnesses outside xUnit. |

[CRITICAL]:
- Do not move native behavior into static xUnit just to improve coverage.
- Do not document a bridge gap when an executable `*.verify.csx` can own it.
- Do not add test-specific shared helpers. Promote only universal higher-order capability with at least two real consumers.
- Treat testkit additions as law/oracle/generator functionality, not extraction. Shared code must replace repeated spec logic with stronger evidence, not shorter spelling.
- Treat shape-only assertions as Grade D unless paired with a Grade A/B oracle or a durable Grade C failure/category rail.
- Delete or replace shape-only assertions that only inspect values constructed inside the same test.
- Pair every declared `Output`/accepted `TOut` surface with an actual projection law. Metadata-only output checks do not prove dispatch.
- Public union cases, record structs, and policy records that bypass factory methods need default-invalid/raw-payload laws or must be made unconstructable by design.
- Mixed guards must preserve categories: invalid input stays `Input`/`Tolerance`; unsupported capability stays `Unsupported`.
- Paired raw/domain outputs both need laws, for example `Vector3d` and `Direction`, receipt and geometry, scalar and value-object projections.
- Do not use `.IsSucc`, `.IsFail`, `.IsSome`, or `.IsNone` as primary proof; use `Spec.Succ`, `Spec.FailCategory`, `Spec.Valid`, `Spec.Invalid`, `Spec.Some`, or `Spec.None`.
- Classify each Stryker survivor before test edits: missing oracle, equivalent mutant, bridge-owned path, or product bug.
- Keep `ConcurrentDictionary`, `Interlocked`, `Directory`, `Path`, `Console.WriteLine`, and `System.Drawing` inside explicit test/tool/bridge boundary adapters.
- For `Rasm.Vectors`, the canonical bridge-vs-static rail list lives in [bridge-runtime.md `[2][RULES]`](references/bridge-runtime.md); static specs own pure MathNet/Spectral/factory/failure laws and managed input guards, every other native success belongs in bridge scenarios per that authoritative list.

---
## [3][SCENARIOS]
>**Dictum:** *Scenario files are source-only runtime laws owned by tests.*

<br>

Place `*.verify.csx` files beside the relevant test slice under `tests/csharp/libs/<Project>/<MirrorPath>/scenarios/`. The bridge maps that convention to `libs/csharp/<Project>/<Project>.csproj`; do not add manifests, catalogs, app-local scenario folders, or per-scenario path maps.

The bridge injects `SCENARIO_NAME` and `CAPTURE_PATH` before execution. Do not declare or shadow them. Author scenarios through the polymorphic `Rasm.TestKit.Scenarios.Scenario.Run(theme, capturePath, (key, facts) => { … })` harness — it builds the `Op key`, emits the `scenario=`/`capture=` plain header, runs the body, and serializes the populated `FactBag` to a single `facts={json}` plain line plus a `rasm.rhino-bridge.evidence=facts={json}` marker. Inside the body, populate evidence with `facts.Add(string key, object value);` as a void statement. Throw `InvalidOperationException` with observed values for failed predicates.

[CRITICAL]:
- Keep scenarios deterministic: clear or create only the document state they own, use constants instead of randomness/time/I/O, redraw before capture, and add a `facts.Add(...)` for every runtime fact that would be hard to infer from the exception alone.
- Do not call `BridgeMarker.EmitFact`, `BridgeMarker.EmitScenarioHeader`, `BridgeMarker.EmitReturn`, `BridgeMarker.EmitEvidence`, or `BridgeMarker.EmitCapture` — those public emitters were dropped during the protocol-surface tightening. The only public emitters are `BridgeMarker.Emit(BridgeMarker marker)` and `BridgeMarker.EmitFacts(IReadOnlyDictionary<string, object>)`; routine fact emission belongs in `Scenario.Run`.
- Do not emit `Console.WriteLine("key=value")` plain lines for per-fact evidence; the harness's batched `facts={json}` line is the canonical fact wire format. The agent-side parser is `BridgeMarker.Scan(stdout)` filtered on `BridgeMarker.Evidence` cases.
- Do not add `#r`, `#load`, absolute build-output paths, legacy bridge job files, or retired script-server flow.
- Do not treat a missing `~/.rasm/rhino-bridge.json` as missing scenario data. It is live endpoint metadata; run `bridge launch` (idempotent — reuses an existing endpoint or relaunches) or `bridge doctor`.
- Do not propose headless Rhino, Docker/VM Rhino, or Rhino.Inside as substitutes for the bridge scenario rail on macOS. Use the installed RhinoWIP bridge and reverify stale runtime claims locally.
- Capture paths live beside bridge reports under `.artifacts/rhino/verify` or `.artifacts/rhino/bridge/check`.
- On macOS, `ViewCapture.CaptureToBitmap(view)` can capture the active viewport. Set the active view and redraw before capture when a scenario uses viewport evidence.

---
## [4][SPEC_SHAPE]
>**Dictum:** *One generated input should exercise many assertions.*

<br>

| [INDEX] | [RULE] | [DETAIL] |
| :-----: | ------ | -------- |
| [1] | LOC | Target 175 LOC per normal spec; 176-225 when one owning source file has multiple real concepts and the result is denser than a split. Testkit files may reach 350 LOC. |
| [2] | Layout | Imports, namespace, `[CONSTANTS]`, `[ALGEBRAIC]`, optional `[EDGE_CASES]`. |
| [3] | Classes | Public xUnit test classes; spec-local generator classes stay non-public unless discovery requires public visibility. |
| [4] | Attributes | `[Fact]`/`[Theory]` on their own line. No local analyzer suppressions for normal test shape. |
| [5] | Generators | Route reusable value-object generators through production factories. Avoid parallel constructors. Use `Gen.Where(...).Select(...)`, never `Select+throw` — `throw` breaks CsCheck shrinking. |
| [6] | Names | PascalCase law names; no underscores. |
| [7] | Polymorphic-first | Before writing a second `[Fact]` that shares setup with an existing one, reach for a polymorphic pattern from [density-axes.md `[4][POLYMORPHIC_PATTERNS]`](references/density-axes.md). |

[CRITICAL]:
- A spec is not strong because it has many facts. It is strong when one generated domain attacks construction, projection, unsupported outputs, failure categories, receipt invariants, and an independent oracle without mirroring production code.
- A real oracle predicts behavior from another source of truth: closed-form math, conservation, fixture geometry, category contract, runtime bridge observation, or documented external behavior. A law varies a behavior family enough to catch swapped inputs, missing validation, unsupported outputs, and receipt drift.
- Use distinct generated payload values when a source function transports or dispatches multiple inputs. Equal or placeholder payloads hide swaps and ignored branches.
- Allow 225 LOC, and exceptionally 300 LOC, only when each added line buys a real oracle, boundary, bridge classification, or product-bug guard that cannot be compressed through `Spec`, `Gens`, `Numeric`, local arrays, or product generators. Specs that exceed 225 LOC must document the per-section LOC delta justifying the overage (e.g., Matrix spec at 300-350 LOC owns SVD/QR/LU/Cholesky/Eigen reconstruction + 5 SmartEnum dispatch paths + 8 norm laws).
- If a failing law reveals a real production bug, fix the owner. Do not weaken the law into shape-only proof.
- Hand-constructing a record type (e.g., `TopologyReceipt`) and asserting its own fields is Grade D mirror coverage. Migrate to a bridge scenario OR add an `IsValid` predicate law via `TheoryData<Record, bool>` (one valid row, each invariant individually broken).
- Filter `Spec.ForAll(Gen.OneOfConst([A,B,C]), ...)` shows as ONE Stryker mutation target. Convert to `[Theory][InlineData(A)][InlineData(B)][InlineData(C)]` (or `MemberData(...)` from `SmartEnum.Items`) when Stryker survivors include per-case logic — Theory rows give N separately-killable targets.

---
## [5][TOOL_RAIL]
>**Dictum:** *Tool knowledge lives in docs; specs use the small contract.*

<br>

| [INDEX] | [TOOL] | [DOC] | [LOCAL_USE] |
| :-----: | ------ | ----- | ----------- |
| [1] | xUnit v3 | `docs/testing-libs/xunit/api.md` | MTP rail, assertions, fixtures, `TheoryData<T1..T15>`, generated runner JSON. |
| [2] | CsCheck | `docs/testing-libs/cscheck/api.md` | `Gen`, `Sample`, shrink/replay, model/metamorphic/parallel/perf APIs. |
| [3] | coverlet | `docs/testing-libs/coverlet/api.md` | Opt-in managed coverage map. |
| [4] | Stryker.NET | `docs/testing-libs/stryker/api.md` | Explicit mutation through `tools.quality test run --mutation changed|full`. |
| [5] | Verify | `docs/testing-libs/verify/api.md` | Stable artifact snapshots only. |
| [6] | ArchUnitNET | `docs/testing-libs/archunit/api.md` | Assembly boundary laws. |
| [7] | BenchmarkDotNet | `docs/testing-libs/benchmarkdotnet/api.md` | Benchmark console projects. |
| [8] | SharpFuzz | `docs/testing-libs/sharpfuzz/api.md` | Fuzz harness projects. |

Current local truth:

- There is no root `xunit.runner.json`; `Directory.Build.props` generates per-project runner JSON under `obj`.
- There is no user-facing `IAssemblyFixture<T>` API; use `[assembly: AssemblyFixture(...)]` plus constructor injection.
- xUnit typed theory rows/data exist through arity 15.
- `Spec.Metamorphic` is a path/oracle wrapper over `Spec.ForAll`, not CsCheck `SampleMetamorphic`.
- Do not claim a `Check.Hash` cache path without inspecting the current package/source.
- Treat Stryker zero discovery as a failed mutation rail; the managed target is 95% after discovery proof.

---
## [6][VALIDATION]
>**Dictum:** *A failing law is evidence; investigate product behavior before weakening the test.*

<br>

Use the repo gate appropriate to the touched surface:

```bash
uv run python -m tools.quality static fix
uv run python -m tools.quality static build
uv run python -m tools.quality test run
uv run python -m tools.quality bridge verify tests/csharp/libs/Rasm/Vectors/scenarios
uv run python -m tools.quality bridge verify tests/csharp/libs/Rasm.Grasshopper/UI/scenarios
```

Default `tools.quality test run` executes MTP unit tests only. Use `--mutation changed` or `--mutation full` for Stryker on the managed `Rasm` pair.

Bridge ownership proof:

```bash
uv run python -m tools.quality bridge check libs/csharp/Rasm/Vectors/Space.cs tests/csharp/libs/Rasm/Vectors/scenarios/vectors-space-projection.verify.csx
```

Bridge scenarios are source-only. Do not add `#r`, `#load`, or absolute build-output paths; the bridge owns reference projection and fresh artifact refs.
