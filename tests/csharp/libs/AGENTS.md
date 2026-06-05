# [LIB_SPEC_AGENTS]

Scope: `tests/csharp/libs/` only. Root test policy and `tests/csharp/AGENTS.md` own shared rails; this file adds source-mirrored library-spec deltas.

## [1][READ_ORDER]

[REQUIRED]: Follow `tests/csharp/AGENTS.md` before editing library specs.

- When editing or adding a spec, read the matching production file and nearest production `AGENTS.md`.
- When changing a target spec, read the full spec first.
- When adding or changing `*.verify.csx` scenarios, read bridge guidance first.
- When promoting shared law, generator, oracle, or scenario capability, read testkit owners first.

## [2][OWNERSHIP]

- Mirror source ownership: one spec targets one owning source file or one tight source concern.
- Add a spec only when no truthful owner exists.
- Move laws to the precise owner when a better source owner appears.
- Keep module-local generators in the spec until multiple specs need the same shape.

## [3][LAW_DENSITY]

[CRITICAL]:
- Replace shape-only tests with law matrices that vary construction, projection, unsupported output, failure category, and an independent oracle when the source exposes those axes.
- Delete tests that assert values built inside the same test unless they guard a durable failure rail or dispatch contract.
- Fix product owners when adversarial laws expose real bugs.

Prefer one law-packed fact over several narrow facts when the same generated sample can attack closure, dispatch, unsupported outputs, failure categories, receipts, and invariants together.

## [4][EXTENSION_GRAMMAR]

- New shared rail proof: use `Spec.Valid`, `Spec.Invalid`, `Spec.Succ`, `Spec.Fail`, `Spec.FailCategory`, and `Spec.FailMany` before local peeking.
- New input family: use `Gens` edge-biased numeric, tolerance, dimension, unit interval, positive magnitude, and context inputs before local generators.
- New expected-value engine: use `Numeric`, closed form, smaller model, or bridge observation instead of production operators.
- New dense case family: use generated case sweeps, union case sweeps, reflected `TheoryData`, product generators, or dispatch tables before adding repeated facts.
- New bridge-owned behavior: place source-only `*.verify.csx` scenarios under the relevant mirrored source path.

## [5][BRIDGE_BOUNDARIES]

- RhinoCommon or GH2 APIs that require RhinoWIP host state belong in bridge scenarios under the relevant mirrored source path.
- Static specs may classify bridge-owned behavior but must not pretend to execute native runtime paths.
- Pair each bridge scenario with an owning source file for bridge checks.
- Record bridge-owned gaps as executable scenario work, not skipped xUnit tests or shape-only assertions.

## [6][REJECTIONS]

- No helper files for one spec.
- No circular tests or snapshots of implementation structure.
- No mutable incidental ordering assertions unless ordering is the contract.
- No broad specs when a precise owner exists.
- No low-value coverage preserved for count alone.
- No public default-struct gaps without an invalid-default law where public construction bypasses the factory.

## [7][STOP_RULES]

Do not polymorphize when the oracle statement changes per case, the case count is 2 and likely stable, or CI triage needs a separately tracked test ID. Do not weaken a production contract to satisfy a test until the matching owner has been inspected.
