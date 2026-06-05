# [ROOT_AGENTS]

Scope: repository root. `CLAUDE.md` owns universal project policy, skill routing, and quality rails; this file is the repo instruction router and first-hop overlay map for root-started work.

## [0][LOAD_ORDER]

[REQUIRED]: Read and follow `CLAUDE.md` before this file.

Codex loads project instructions from repository root toward the current directory; closer files override earlier conflicting guidance, and only one instruction file is loaded per directory. `AGENTS.override.md` takes precedence over `AGENTS.md` where present. Nested instruction chains share the project-doc budget, 32 KiB by default, so every overlay stays delta-only.

Root-started work must still discover the nearest nested `AGENTS.md` before editing a subtree that owns one. Fallback names and provider-loading behavior are configuration facts, not repo policy, unless current local config proves they apply here.

## [1][READ_ORDER]

- When editing C# libraries, read `libs/csharp/AGENTS.md`, then the nearest project overlay.
- When editing C# tests, `.spec.cs`, `.verify.csx`, bridge scenarios, or testkit code, read `tests/csharp/AGENTS.md`; library specs also read `tests/csharp/libs/AGENTS.md`.
- When editing docs, read `docs/standards/README.md`; instruction-file work also reads `docs/standards/agents-md.md`.
- When editing `tools/assay`, read `tools/assay/AGENTS.md`.
- When editing bridge runtime, bridge scenarios, package, deploy, publish, or host-runtime proof, read `tools/rhino-bridge/AGENTS.md`.
- When changing cross-stack owner precedence, proof order, or host-library routing, read `docs/usage.md`.
- When changing `System.*`, global usings, package/reference policy, host-provided BCL assumptions, or `global.json`, read `docs/system-api-map`.
- When adding product-library, host SDK, or host-composition assumptions, read `docs/external-libs` and `docs/host-libraries.md`.
- When changing test-tool APIs or advanced harness behavior, read `docs/testing-libs`.

## [2][NAVIGATION]

Use repository-native discovery before broad scans:
- File discovery: `fd`.
- Exact text search: `rg`.
- Structural search: `ast-grep` when symbol shape matters.
- Monorepo topology: Nx metadata and affected logic before workspace-wide edits.

Read full target files before editing. Read minimal surrounding files needed to prove ownership, existing patterns, and route conflicts.

## [3][ENGINEERING_CONTRACT]

Extend canonical owners before adding new rails. Prefer root-cause refactoring, caller updates, and obsolete-path removal over additive wrappers or compatibility shims.

Keep implementations dense, strongly typed, and value-driven. Collapse repeated case families into operation algebras, smart enums, unions, folds, projection carriers, typed receipts, or source-owned tables. Avoid low-quality branching when a value, policy, or algebra can drive behavior.

Use FP/ROP boundaries by default. Convert nullable, bool, exception, native ownership, disposable, and runtime-failure channels into typed rails at the boundary the owning library controls.

Do not add single-use helpers, utility files, generic receipt interfaces, generic ledgers, reported-value wrappers, or wrapper-only APIs unless an existing owner proves net simplification and stronger invariants.

Treat analyzer diagnostics as hypotheses. Fix true-positive code, and refine the analyzer when a diagnostic forces less native, larger, or less correct code.

## [4][ROUTING]

| [INDEX] | [CONCERN]                      | [OWNER]                        |
| :-----: | :----------------------------- | :----------------------------- |
|   [1]   | Documentation standards        | `docs/standards/README.md`     |
|   [2]   | `AGENTS.md` file shape         | `docs/standards/agents-md.md`  |
|   [3]   | Cross-stack owner ladder       | `docs/usage.md`                |
|   [4]   | BCL, packages, host references | `docs/system-api-map`          |
|   [5]   | Product and host libraries     | `docs/external-libs`           |
|   [6]   | Host composition adoption      | `docs/host-libraries.md`       |
|   [7]   | Test-tool APIs                 | `docs/testing-libs`            |
|   [8]   | Quality command behavior       | `tools/quality/README.md`      |
|   [9]   | Rhino bridge operator behavior | `tools/rhino-bridge/README.md` |
|  [10]   | Live bridge instruction deltas | `tools/rhino-bridge/AGENTS.md` |
|  [11]   | C# library-family deltas       | `libs/csharp/AGENTS.md`        |
|  [12]   | C# test and scenario deltas    | `tests/csharp/AGENTS.md`       |
|  [13]   | Assay tool deltas              | `tools/assay/AGENTS.md`        |

Host SDK boundaries use local RhinoWIP/GH2 XML, decompile evidence when XML is absent, the API rail, `docs/usage.md`, and the nearest host project overlay. AppUi package-consumer and package-pin truth live in central manifests plus `docs/system-api-map`; do not preserve package facts in root prose.

## [5][DOCUMENTATION]

Route README, ADR, architecture, roadmap, test strategy, API, reference, code documentation, support matrix, how-to, runbook, contributing, tutorial, onboarding, and instruction-file work through `docs/standards/README.md`.

Keep documentation rooted in current paths, commands, manifests, source, and configured tooling. Remove stale paths, stale commands, compatibility prose, and invented routes when current repository truth no longer supports them.

## [6][REJECTIONS]

- No command catalogs in root; `CLAUDE.md`, tool READMEs, and nested overlays own current command selection.
- No subtree-local implementation facts when a nested `AGENTS.md`, README, architecture, roadmap, or source file owns the behavior.
- No copied provider manuals, fallback-name tutorials, package version prose, roadmap state, generated contract bodies, or bridge transcripts.
- No C# static, test, or bridge proof claims for docs-only instruction edits.
