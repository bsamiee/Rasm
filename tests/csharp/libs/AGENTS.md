# [LIB_SPEC_AGENTS]

Scope: `tests/csharp/libs/` only. Root test policy and `tests/csharp/AGENTS.md` own shared rails; this file adds source-mirrored library-spec deltas.

## [1][READ_ORDER]

[REQUIRED]: Follow `tests/csharp/AGENTS.md` before editing library specs.

- When editing or adding a spec, read the matching production file and nearest production `AGENTS.md`.
- When changing a target spec, read the full spec first.
- When a spec marks behavior `BRIDGE-DEFERRED`, touches RhinoCommon/GH2 host behavior, or reacts to a bridge-owned mutation survivor, read sibling `scenarios/` where present before deciding whether the runtime gap is already owned.
- When adding or changing `*.verify.csx` scenarios, read `tools/rhino-bridge/AGENTS.md` first.
- When promoting shared law, generator, oracle, or scenario capability, read testkit owners first.

## [2][OWNERSHIP]

- Mirror source ownership: one spec targets one owning source file or one tight source concern.
- Add a spec only when no truthful owner exists.
- Move laws to the precise owner when a better source owner appears.
- Keep module-local generators in the spec until multiple specs need the same shape.
- Keep one-spec generators, fixtures, and assertion data local; promote only law, oracle, generator, serializer, or scenario capability that has multiple real consumers or removes repeated owner-local proof logic.

## [3][LAW_DENSITY]

[CRITICAL]:
- Replace shape-only tests with law matrices that vary construction, projection, unsupported output, failure category, and an independent oracle when the source exposes those axes.
- Delete tests that assert values built inside the same test unless they guard a durable failure rail or dispatch contract.
- Fix product owners when adversarial laws expose real bugs.
- Classify bridge-deferred behavior before writing static assertions; static specs may prove guards, admission, unsupported outputs, receipts, and failure categories around a native path, but successful host execution belongs in a scenario.

Prefer one owner-local law matrix or generated sweep over several narrow facts when the same sample can attack construction, closure, dispatch, unsupported outputs, failure categories, receipts, invariants, and an independent oracle together. Split only when the oracle statement, source owner, or triage identity changes.

## [4][EXTENSION_GRAMMAR]

- New shared rail proof: use `Spec.Valid`, `Spec.Invalid`, `Spec.Succ`, `Spec.Fail`, `Spec.FailCategory`, and `Spec.FailMany` before local peeking.
- New `Option` proof: use `Spec.Some` and `Spec.None` before raw `.IsSome`, `.IsNone`, or `IfNone` default peeking unless rail state is the contract under test.
- New input family: use `Gens` edge-biased numeric, tolerance, dimension, unit interval, positive magnitude, and context inputs before local generators.
- New expected-value engine: use `Numeric`, closed form, smaller model, or source-owned bridge scenario facts emitted through `Scenario.Run` and `FactBag` instead of production operators.
- New dense case family: use generated case sweeps, union case sweeps, reflected `TheoryData`, product generators, or dispatch tables before adding repeated facts.
- New bridge-owned behavior: add or update source-only `*.verify.csx` scenarios under the relevant mirrored source path, paired with the owning source file or tight source concern through bridge routing; do not add manifests, scenario catalogs, `#r`, `#load`, absolute build-output paths, skipped xUnit, or mock-host substitutes.

## [5][BRIDGE_BOUNDARIES]

- RhinoCommon or GH2 APIs that require RhinoWIP host state belong in bridge scenarios under the relevant mirrored source path.
- Static specs may classify bridge-owned behavior and prove managed guards, categories, receipts, or unsupported-output rails, but must not pretend to execute successful native runtime paths.
- Pair each bridge scenario with an owning source file or tight source concern so bridge routing can rebuild the real project and prove the runtime fact.
- Record bridge-owned gaps as executable scenario work or explicit proof gaps, not skipped xUnit tests, mock-host assertions, shape-only host checks, or documentation caveats.
- Bridge scenario facts must be emitted through the scenario harness and fact bag; do not parse raw stdout, duplicate human-readable lines, or run-local artifact paths as proof.

## [6][REJECTIONS]

- No helper files for one spec.
- No circular tests or snapshots of implementation structure.
- No mutable incidental ordering assertions unless ordering is the contract.
- No broad specs when a precise owner exists.
- No low-value coverage preserved for count alone.
- No public default-struct gaps without an invalid-default law where public construction bypasses the factory.

## [7][STOP_RULES]

Do not polymorphize when the oracle statement changes per case, the case count is 2 and likely stable, or CI triage needs a separately tracked test ID. Do not weaken a production contract or add static fake proof until the matching source owner and any sibling scenario evidence have been inspected.
