# [CSHARP_TESTS_AGENTS]

Scope: `tests/csharp/` only. Root `AGENTS.md`, `CLAUDE.md`, and the `testing-cs` skill own universal test policy; this file adds C# test-tree deltas. Library-spec deltas live in `tests/csharp/libs/AGENTS.md`.

## [1][READ_ORDER]

[REQUIRED]: Follow `CLAUDE.md`, `testing-cs`, and `coding-csharp` for every `.spec.cs`, `.verify.csx`, and testkit change.

- When changing any C# test, read this file first.
- Before changing an assertion, scenario fact, expected-value path, or mutation response, classify the behavior as static-managed law, bridge-owned runtime behavior, architecture/tooling/fuzz/benchmark rail, mutation-survivor triage, or proof gap.
- When editing `tests/csharp/libs/**/*.spec.cs`, read `tests/csharp/libs/AGENTS.md`.
- When editing or adding a spec, read the matching production owner and nearest production `AGENTS.md`.
- When using advanced xUnit, CsCheck, coverlet, Stryker, Verify, ArchUnitNET, BenchmarkDotNet, or SharpFuzz behavior, read `docs/testing-libs`.
- When changing serializer, fuzz-parser, bridge-probe, host-loader, filesystem, capture, or `System.*` test boundaries, read `docs/system-api-map`.
- When changing `.verify.csx` or bridge-runtime work, read `tools/rhino-bridge/AGENTS.md`.

## [2][TEST_CONTRACT]

[CRITICAL]:
- Build adversarial laws, not confirmation checks.
- Investigate every failing test before editing it.
- Keep Rhino/GH native behavior out of static xUnit on macOS; bridge scenarios own runtime behavior that requires the host.

Expected values come from independent math, smaller models, metamorphic relations, fixture geometry, maintained API/source contracts for static-managed behavior, or source-owned bridge scenario facts for host runtime behavior. Never assert implementation output against itself.

Proof classification comes before assertion. Static specs prove pure managed contracts, guards, deterministic algorithms, generated metadata, typed receipts, rail categories, and pre-native rejection; source-owned bridge scenarios prove successful Rhino/GH host runtime behavior, document/canvas/UI state, native geometry or topology, capture, resolver freshness, and assembly/package behavior that static xUnit cannot execute.

Mutation survivors are triage input. Classify each survivor as missing oracle, equivalent mutant, bridge-owned path, or product bug before editing tests; strengthen a spec only after that classification says the surviving behavior is static-managed and missing a real oracle.

## [3][EXTENSION_GRAMMAR]

- Shared law, oracle, generator, serializer, or scenario capability: extend `_testkit` only when multiple specs consume it.
- Architecture law: route dependency and layering checks through `tests/csharp/_architecture`.
- Stable snapshot: route through `tests/csharp/_tooling`; do not hand-maintain generated report bodies.
- Benchmark: route through `tests/csharp/_benchmarks` and keep hot-path measurement separate from correctness specs.
- Fuzz harness: route parser/token harnesses through `tests/csharp/_fuzz`.
- Library spec: mirror the production owner and follow `tests/csharp/libs/AGENTS.md`.
- Bridge scenario: route to `tools/rhino-bridge/AGENTS.md` and `testing-cs` bridge guidance.

## [4][ORACLE_RULES]

- Prefer variable-driven samples through `Spec.ForAll`, `Spec.Metamorphic`, and `Spec.Regression`.
- Use explicit seeds only to preserve a discovered regression.
- Grade expected-value paths before accepting them; shape-only assertions are supplemental unless paired with a real oracle or durable failure rail.
- Use deliberately distinct payload values so tests catch swapped fields, stale branches, and ignored inputs.
- Preserve a useful failing test by fixing the production owner first.
- Use `Spec` rail helpers for `Fin`, `Validation`, `Option`, success, failure category, and diagnostics, including `Spec.Some` and `Spec.None` for `Option` proof. Raw `.IsSucc`, `.IsFail`, `.IsSome`, and `.IsNone` checks are allowed only as secondary invariants after value, category, oracle, diagnostic, receipt, or bridge-fact proof, or when rail-state policy is the subject under test.
- Treat bridge facts as oracle material only when they come from source-owned scenario evidence, not from human-readable stdout, run-local artifact paths, or a static imitation of the host.

## [5][REJECTIONS]

- No implementation mirrors as expected values.
- No raw rail-state checks as primary proof when `Spec` helpers, expected values, failure categories, diagnostics, receipt invariants, or bridge facts can prove the behavior.
- No shape-only constructor proof without payload, admission, dispatch, or bridge evidence.
- No skipped xUnit, mock host, headless substitute, shape-only host assertion, or documentation caveat as bridge-owned proof.
- No mutation-response assertion until the survivor is classified as missing oracle, equivalent mutant, bridge-owned path, or product bug.
- No local suppressions for style friction.
- No generated reports, corpora, mutation output, benchmark output, or transient test results outside `.artifacts`.
- No bridge scenario `#r`, `#load`, or absolute build-output paths.

## [6][STOP_RULES]

If a bridge run reports host assembly identity, staged-reference, marker parsing, fact envelope, or LanguageExt bootstrap failures, verify bridge setup before weakening the scenario or static spec. If a static spec would only pretend to execute native runtime behavior, route the claim to a source-owned bridge scenario or record a proof gap instead.
