---
include:
  - "libs/typescript/**"
  - "**/*.ts"
  - "**/*.tsx"
---

# [TYPESCRIPT_DOCTRINE]

`docs/stacks/typescript/` is the floor for every TypeScript surface — fence code judged as production on the dual TS7/TS6 compiler floor with Effect as the substrate. Absolutes: zero `any`, zero `throw`, zero `enum`; absence is `Option`; files end at one terminal `// --- [EXPORTS]` block.

## [01]-[SCHEMA_AND_RAILS]

- A domain concept is one Schema owner (`Schema.Class`, `Schema.Union` of tagged cases, `Schema.TaggedError`); the static type derives from the owner and variants derive via `pick`/`omit`/`extend`/`transform` — a hand-written `interface` or DTO beside a Schema owner, a standalone exported branded scalar, and a boolean-discriminated pair where a tagged union owns the family are findings.
- `Effect<A, E, R>` is the one rail: bare `Promise`/`await`, `throw`/`try`/`catch` in domain flow, a `{ loading, error, data }` record re-deriving `Exit`, and reflex `catchAll` blanketing a typed channel are findings; `Micro` is banned.
- `Effect.gen` over independent operands serializes and drops faults — independent operands compose applicatively (`Effect.all`, `zip` variants); `Effect.Do`/`bind`/`bindTo` chains are findings (only `Effect.gen` is do-notation), as is a `flatMap(Effect.succeed(...))` restating `map`.
- Fault architecture is one `TaggedError` family plus a policy table; hand-rolled retry and poll loops where `Schedule` composes are findings.
- `let`/`push` accumulation across steps is a finding; folds and combinators own aggregation.

## [02]-[DISPATCH_AND_BOUNDARIES]

- One concern exposes one overloaded `function` entrypoint over one input union, signatures ordered narrow to wide; `resolve`/`resolveMany`/`resolveByKey` siblings, arity twins, `batch: boolean` beside a widened array, and data-first plus curried twins are findings — pipe-versus-direct is one `Function.dual`.
- Closed families dispatch via `$match`/`Match.exhaustive`/`Match.tagsExhaustive`; an `orElse` fallback absorbing future tags of a closed family, a `switch` restating a vocabulary table, and an `as`/`typeof` ladder for structural dispatch are findings.
- `Schema.decodeUnknown(Owner)` sits on the first line seeing a foreign value, fixing owner, accumulation posture (`errors: "all"`), and drift posture (`onExcessProperty: "error"`) as one module-scope policy per seam; interior re-validation, `as` casts where a decode produces evidence, scattered `process.env` reads, top-level `Effect.runPromise`, and hand-built SQL or fetch in domain flow are findings.
- Every dependency is a Tag in the `R` channel satisfied through the Layer graph; the owner is one `Effect.Service` class. An `interface` + `Context.GenericTag` + `Layer.effect` triple restating one concept, module-level live singletons, parameter-drilled dependencies, and mock frameworks are findings.
- One contract-checked `as const satisfies` table is the single source of truth for a vocabulary; a hand-written union, `enum`, or parallel constant a table derives, and a `switch` over a keyed domain a vocabulary row maps, are findings. `TS2589`/`TS2590` are architecture pressure — the repair is value-level derivation, never a directive.
- Runtime invariants ride Effect primitives — `HashMap`/`Chunk`/`SortedMap` for state, `Data` + `Equal.equals` for identity, `DateTime`/`Duration` for time, `BigDecimal` for money, `Redacted` for secrets; JS `Map`/`Set`/`Date`, epoch math, `===` carrying domain meaning, sort-on-read, and get-then-set pairs are findings.
- Fibers parent to their forker or a `Scope`; `Effect.runFork` in domain flow, a `Map<string, Fiber>` registry (use `FiberMap`), a polled `Ref` flag (use `Deferred`), a poison-pill sentinel on a `Queue` (use `Mailbox`), and a hand-counted in-flight bound (use `makeSemaphore`) are findings. A `Stream` is earned only by unbounded/over-time data, observe-before-end, or windowing; a `Stream` over bounded in-memory data is a finding.

## [03]-[MODULE_SHAPE]

Declarations author unexported with interiors `_`-prefixed; `export default`, re-export barrels (`export ... from`, `export *`), an `export` keyword on a body declaration, `enum`, runtime `namespace`, and constructor parameter properties are findings. `@ts-nocheck`/`@ts-ignore` anywhere, a `@ts-expect-error` spanning a live finding, and a construct only one compiler of the dual floor accepts are findings.
