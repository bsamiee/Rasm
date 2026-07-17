---
include:
  - "libs/typescript/**"
  - "**/*.ts"
  - "**/*.tsx"
  - "**/tsconfig*.json"
  - "**/biome.json"
---

# [TYPESCRIPT_SURFACE_FACTS]

Verified Effect and compiler truths a generic reviewer misfires on — each listed shape is deliberate or required; flagging it is a false positive.

## [01]-[EFFECT_SURFACE]

- `Option` and `Either` are Effect subtypes — `yield*`ing them directly with no lift adapter is correct; a yielded `Option.none` lifts as `Cause.NoSuchElementException`, and re-spelling it via `Effect.mapError` at the point of knowledge is required.
- `Effect.fn("name")(function* (...) {...})` is the definition seam — its `function*` and `return` are sanctioned, not statement residue; `Cause.pretty`/`squash` belong only at terminal and logging edges.
- `class X extends Effect.Service<X>()("<scope>/X", { scoped: ..., dependencies: [...] })` is the canonical owner with generated `Default` statics; service-key strings carry a scope path segment intentionally — global identity where a duplicate key is a silent collision.
- `Schema.suspend` with a mandatory return annotation, plus the one sanctioned interior `interface OwnerEncoded` beside a recursive owner, is required — inference cannot self-close; getters and `static` `Order`/algebra members on a `Schema.Class` are the canonical owner, not logic-in-models.
- `as const satisfies Record<string, Row>` (satisfies without a widening annotation) is the vocabulary anchor form; merged registry `interface` plus `declare module` row contribution is the intended cross-module vocabulary.
- Overloaded `function` declarations (multiple signatures over one wider implementation) are required for overload sets — a `const` arrow forces a cast; narrow-to-wide ordering is deliberate.
- `Effect.uninterruptibleMask` + `restore` over `Effect.uninterruptible`, and `Effect.onInterrupt` versus `Effect.ensuring`, are real distinctions, never redundancy; `Match.instanceOf` ladders with `orElse` are correct over foreign thrown values (the closed-family `orElse` ban does not apply there).
- A foreign value resurfacing past the seam — a nested `unknown` band, a second source read — decodes again: a second admission site, not a duplicate validation.
- `Schema.optionalWith(S, { as: "Option" })` is the sole absence spelling on a decoded field — total `Option<A>` interior, optional encoded; bare `Schema.optional` leaks `| undefined` into the decoded type, and `Schema.NullOr` on a domain field or a sentinel-for-absence is the rejected form.
- `Redacted` implements `Equal`, prints `<redacted>` on every string, JSON, and inspect channel, and `unsafeWipe` makes later `value` reads throw terminally — a sealed secret comparing through `Redacted.getEquivalence` and unwrapping once at the consuming boundary is safety by construction, not missing discipline.
- `Arbitrary.make`/`Pretty.make` throw `Missing annotation` at first derivation over a bare `Schema.declare`, while `Schema.equivalence` silently falls back to `Equal.equals` (reference identity on a foreign instance) — the silent fallback is the audit line, so a foreign owner admits with its annotation set complete or not at all.

## [02]-[COMPILER_SURFACE]

- `import { Array, type Duration } from "effect"` inline-type specifiers on one statement are required under `verbatimModuleSyntax`; `Array`/`Order`/`Record`/`Struct` deliberately shadow globals on the value plane while `ReadonlyArray<T>` stays the global type — alias rebinding to spare a shadowed global is the rejected form.
- `?: T` (exact-optional) versus `T | undefined` (always-present, no value) are distinct spellings; the `?: T | undefined` blur is the defect, not the distinction. Dot-on-declared-key versus bracket-on-signature-member is required key-provenance spelling, and a bracket read lifts to `Option` at the seam, never `!`.
- `@ts-expect-error` is a legal proof token where a surface must prove a form rejected; a plain record under `Record`/`Struct` folds is correct for a closed literal key set (not `HashMap`); JS stdlib survives at the FFI seam and inside marked kernels; `globalThis.<Name>` at the FFI seam is the sanctioned global reach.
- `import defer * as Name` and side-effect imports are legal only leading a boot-edge module; `runMain` lives in one boot module.
- A file-scoped `biome.json` linter-off override on one e2e platform spec dodges an upstream Biome resolver panic on a tsc-clean file — never a suppressed-lint smell.
- Package rulings are settled: BM25 rides `vchord_bm25`, NATS JetStream is admitted, `pg_uuidv7` is pruned for PG18-native `uuidv7()`, cluster is leaderless, and `Effect.withExecutionPlan` replaces the removed AiPlan — never re-litigate these admissions in review.
