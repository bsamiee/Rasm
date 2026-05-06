# [H1][TESTING_STANDARDS]
>**Dictum:** *Universal tests verify contracts from stable roots, not copied topology.*

<br>

Canonical standards for test authoring. Root `tests/` owns universal JS, TS, Python, browser, C#, Rhino, analyzer, and cross-workspace tests. Language-native project files may live under `tests/` when the runner requires a project boundary.

[REFERENCE] Patterns: [->patterns.md](patterns.md) | Laws: [->laws.md](laws.md) | Guardrails: [->guardrails.md](guardrails.md)

---
## [1][ROOT_POLICY]
>**Dictum:** *Stable roots make discovery, ownership, and quality gates deterministic.*

<br>

| [INDEX] | [PATH]                        | [PURPOSE]                                                            |
| :-----: | ----------------------------- | -------------------------------------------------------------------- |
|   [1]   | **`tests/unit/`**             | Fast contract tests for pure modules and public APIs.                |
|   [2]   | **`tests/integration/`**      | Boundary tests requiring databases, queues, services, or containers. |
|   [3]   | **`tests/system/`**           | Cross-component behavior and release-critical workflows.             |
|   [4]   | **`tests/e2e/`**              | Browser, CLI, or external user journeys when configured.             |
|   [5]   | **`tests/fixtures/`**         | Shared data, oracle vectors, snapshots, and factories.               |
|   [6]   | **`tests/csharp/<project>/`** | .NET xUnit projects for pure C# package contracts.                   |
|   [7]   | **`tests/rhino/`**            | macOS RhinoWIP runtime integration tests, opt-in by environment.      |
|   [8]   | **`tests/ast-grep/`**         | Static-rule pass/fail fixture corpus.                                |
|   [9]   | **`tests/py_analyzer/`**      | Python analyzer contract tests.                                      |

[CRITICAL]:
- [NEVER] Place JS, TS, Python, C#, Rhino, analyzer, browser, or cross-workspace tests outside root `tests/`.
- [NEVER] create app-local test roots unless the language toolchain requires a project-owned test project outside `tests/`.
- [NEVER] reference copied sample paths from prior packages or services.

**Rhino runtime gate:** `RASM_RHINO_TESTS=1 pnpm check:cs` verifies RhinoWIP paths and runs only when `Rhino.Testing`
resolves a `net10.0` runtime asset. Current `Rhino.Testing 9.0.8-beta` resolves `lib/net9.0/Rhino.Testing.dll`; the
gate skips instead of launching a known-crashing macOS testhost. Use `RASM_RHINO_TESTS=require` for strict CI failure,
`RASM_RHINO_TESTS_FORCE=1` for diagnostics, and `RASM_RHINO_DOTNET` to point at an official Microsoft .NET SDK binary.

---
## [2][FILE_STRUCTURE]
>**Dictum:** *Spec files organize evidence by contract, not framework ceremony.*

<br>

Spec files use one public contract per file when practical. File names follow `<contract>.spec.<ext>` and sit under the closest `tests/` scope. Generated, build, coverage, and report artifacts stay outside author-owned spec directories.

**Canonical sections** (omit unused): Constants -> Oracles -> Properties -> Examples -> Regressions.

[IMPORTANT]:
1. [ALWAYS] **Constants:** Keep bounded values, oracle vectors, and reusable generators near top of spec file.
2. [ALWAYS] **Oracles:** Prefer algebraic laws, standards documents, fixture vectors, or reference implementations.
3. [ALWAYS] **Regressions:** Link each regression to user-visible failure or issue identifier.

[CRITICAL]:
- [NEVER] Add `HELPERS`, `UTILS`, `COMMON`, or framework-specific support sections.
- [NEVER] Re-derive expected values from implementation under test.

---
## [3][DENSITY]
>**Dictum:** *Generated input space multiplies confidence without verbose assertions.*

<br>

| [INDEX] | [TECHNIQUE]                 | [USE_CASE]                                                            |
| :-----: | --------------------------- | --------------------------------------------------------------------- |
|   [1]   | **Property-based testing**  | Laws, round-trips, invariants, serialization, ordering.               |
|   [2]   | **Table-driven examples**   | Small finite matrices of boundary inputs and expected outcomes.       |
|   [3]   | **External oracle vectors** | Protocol, crypto, parser, and data-format conformance.                |
|   [4]   | **Model-based testing**     | Stateful systems with command sequences and invariant checks.         |
|   [5]   | **Fault injection**         | Retry, timeout, cancellation, partial failure, and recovery behavior. |

[IMPORTANT]:
1. [ALWAYS] **Law-first:** Prefer algebraic or protocol law before example assertions.
2. [ALWAYS] **Boundary-rich:** Include empty, maximum, malformed, duplicate, concurrent, and unauthorized cases.
3. [ALWAYS] **Minimal examples:** Use examples only where laws or oracles cannot express user-facing behavior.

---
## [4][ASSERTIONS]
>**Dictum:** *Assertions state observable contracts and preserve refactor freedom.*

<br>

Assert public output, state transition, emitted event, persisted record, or failure rail. Keep assertions independent from private structure, intermediate variables, and source algorithm shape.

[CRITICAL]:
- [NEVER] Assert private methods, private fields, internal ordering, or transient implementation details.
- [NEVER] Branch inside tests to select expected values; encode cases as data or separate properties.
- [NEVER] Hide failed expectations behind catch-all exception handling.

---
## [5][VALIDATION]
>**Dictum:** *Completion requires structure, independence, and executable evidence.*

<br>

[VERIFY]:
- [ ] Every new or changed JS, TS, Python, C#, Rhino, analyzer, browser, or cross-workspace spec lives under `tests/`.
- [ ] Every native test project is invoked by default quality gates or documented as an explicit opt-in runtime gate.
- [ ] Each spec names one public contract or workflow.
- [ ] Expected results come from laws, external oracles, fixtures, or explicit examples.
- [ ] Tests avoid package/app/source-adjacent roots unless required by language-native project tooling.
- [ ] Quality gates run through configured project tooling.
