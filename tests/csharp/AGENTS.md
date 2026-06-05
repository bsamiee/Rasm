# [CSHARP_TESTS_AGENTS]

Scope: `tests/csharp/` only. Root `AGENTS.md`, `CLAUDE.md`, and the `testing-cs` skill own universal test policy; this file adds C# test-tree deltas. Library-spec deltas live in `tests/csharp/libs/AGENTS.md`.

## [1][READ_ORDER]

[REQUIRED]: Follow `CLAUDE.md`, `testing-cs`, and `coding-csharp` for every `.spec.cs` change.

- When changing any C# test, read this file first.
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

Expected values come from independent math, smaller models, metamorphic relations, fixture geometry, documented runtime behavior, or bridge evidence. Never assert implementation output against itself.

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
- Use `Spec` rail helpers for `Fin`, `Validation`, `Option`, success, failure category, and diagnostics; direct rail peeking is supplemental only.

## [5][REJECTIONS]

- No implementation mirrors as expected values.
- No `.IsSucc`, `.IsFail`, `.IsSome`, or `.IsNone` as primary proof.
- No shape-only constructor proof without payload, admission, dispatch, or bridge evidence.
- No local suppressions for style friction.
- No generated reports, corpora, mutation output, benchmark output, or transient test results outside `.artifacts`.
- No bridge scenario `#r`, `#load`, or absolute build-output paths.

## [6][STOP_RULES]

If a bridge run reports host assembly identity, staged-reference, or LanguageExt bootstrap failures, verify bridge setup before weakening the scenario or static spec. If a static spec would only pretend to execute native runtime behavior, route the claim to a bridge scenario instead.
