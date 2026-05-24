---
name: cs-testing
description: >-
  Builds and reviews dense law-matrix C# specs for Rasm using xUnit v3, CsCheck,
  Rasm.TestKit, coverlet, Stryker, and Rhino/GH bridge scenarios. Use when
  creating, refactoring, or validating C# tests, testkit code, test docs, or
  bridge-owned runtime verification for Rasm.
---

# [H1][CS_TESTING]
>**Dictum:** *A test is a law with an independent oracle, not a mirror of the implementation.*

<br>

Use this skill with `coding-csharp` for `.cs` specs/testkit code, `coding-bash` for scripts, and `docgen` + `style-standards` for Markdown. The canonical rail is xUnit v3 stable/VSTest + CsCheck + `Rasm.TestKit`; native Rhino/GH runtime behavior belongs in `*.verify.csx` bridge scenarios.

---
## [1][WORKFLOW]
>**Dictum:** *Classify first; write fewer, stronger laws second.*

<br>

1. Read the owning production file and its sibling tests.
2. Classify each behavior as static-managed or bridge-owned native/runtime.
3. Select laws from [oracles-laws.md](references/oracles-laws.md) and density axes from [density-axes.md](references/density-axes.md).
4. Reuse `Rasm.TestKit` contracts from [testkit.md](references/testkit.md).
5. Check raw tool API in `docs/testing-libs/xunit`, `cscheck`, `coverlet`, and `stryker` before using unfamiliar APIs.
6. Write the spec from [unit-pbt.spec.template.md](templates/unit-pbt.spec.template.md).
7. Validate with the smallest targeted build/test first, then the full C# gates.

---
## [2][RAILS]
>**Dictum:** *Bridge ownership is executable proof, not a documentation escape hatch.*

<br>

| [INDEX] | [RAIL] | [LOCATION] | [OWNS] |
| :-----: | ------ | ---------- | ------ |
| [1] | Static spec | `tests/csharp/libs/<Project>/<MirrorPath>/<Source>.spec.cs` | Pure managed constructors, smart enums, unions, matrix/math rails, `Fin`/`Validation`, deterministic algorithms. |
| [2] | Testkit | `tests/csharp/_testkit` | Universal law adapters, reusable generators, independent numeric oracles, serializers. |
| [3] | Bridge scenario | `apps/grasshopper/Radyab/Scenarios/*.verify.csx` | RhinoCommon/GH runtime APIs, native geometry validity, UI marshaling, document/canvas behavior. |
| [4] | Mutation | `scripts/mutate-cs.sh` | Opt-in managed survivor discovery for `libs/csharp/Rasm` and its tests. |

[CRITICAL]:
- Do not move native behavior into static xUnit just to improve coverage.
- Do not document a bridge gap when an executable `*.verify.csx` can own it.
- Do not add test-specific shared helpers. Promote only universal abstractions with at least two real consumers.

---
## [3][SPEC_SHAPE]
>**Dictum:** *One generated input should exercise many assertions.*

<br>

| [INDEX] | [RULE] | [DETAIL] |
| :-----: | ------ | -------- |
| [1] | LOC | Target 175 LOC per normal spec; 176-200 only when one owning source file has multiple real concepts and the result is denser than a split. Testkit files may reach 350 LOC. |
| [2] | Layout | Imports, namespace, `[CONSTANTS]`, `[ALGEBRAIC]`, optional `[EDGE_CASES]`. |
| [3] | Classes | Public xUnit test classes; spec-local generator classes stay non-public unless discovery requires public visibility. |
| [4] | Attributes | `[Fact]`/`[Theory]` on their own line. No local analyzer suppressions for normal test shape. |
| [5] | Generators | Route reusable value-object generators through production factories. Avoid parallel constructors. |
| [6] | Names | PascalCase law names; no underscores. |

---
## [4][TOOL_RAIL]
>**Dictum:** *Tool knowledge lives in docs; specs use the small contract.*

<br>

| [INDEX] | [TOOL] | [DOC] | [LOCAL_USE] |
| :-----: | ------ | ----- | ----------- |
| [1] | xUnit v3 | `docs/testing-libs/xunit/api.md` | VSTest rail, assertions, fixtures, `TheoryData<T1..T15>`, generated runner JSON. |
| [2] | CsCheck | `docs/testing-libs/cscheck/api.md` | `Gen`, `Sample`, shrink/replay, model/metamorphic/parallel/perf APIs. |
| [3] | coverlet | `docs/testing-libs/coverlet/api.md` | Opt-in managed coverage map. |
| [4] | Stryker.NET | `docs/testing-libs/stryker/api.md` | Opt-in mutation through `scripts/mutate-cs.sh`. |

Current local truth:

- There is no root `xunit.runner.json`; `Directory.Build.props` generates per-project runner JSON under `obj`.
- There is no user-facing `IAssemblyFixture<T>` API; use `[assembly: AssemblyFixture(...)]` plus constructor injection.
- xUnit typed theory rows/data exist through arity 15.
- Do not claim a `Check.Hash` cache path without inspecting the current package/source.

---
## [5][VALIDATION]
>**Dictum:** *A failing law is evidence; investigate product behavior before weakening the test.*

<br>

Use the repo gate ladder appropriate to the touched surface:

```bash
dotnet restore Workspace.slnx
dotnet restore Workspace.slnx --locked-mode
dotnet tool restore
bash scripts/check-cs.sh full
bash scripts/test.sh
dotnet test tests/csharp/libs/Rasm/Rasm.Tests.csproj --configuration Release /p:CollectCoverage=true
bash scripts/rhino.sh verify apps/grasshopper/Radyab/Scenarios
git diff --check
```

Opt-in mutation:

```bash
bash scripts/mutate-cs.sh --self-test
bash scripts/mutate-cs.sh
```

Bridge ownership proof:

```bash
bash scripts/rhino.sh bridge check-source libs/csharp/Rasm/Vectors/Space.cs --script apps/grasshopper/Radyab/Scenarios/vectors-space-projection.verify.csx
```
