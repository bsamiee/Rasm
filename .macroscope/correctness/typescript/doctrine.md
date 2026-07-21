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
- Fault architecture is one `TaggedError` family with its policy table; hand-rolled retry and poll loops where `Schedule` composes are findings.
- `let`/`push` accumulation across steps is a finding; folds and combinators own aggregation. Element-to-element state lives inside the pipeline as a `Stream.mapAccum`/`scan` fold accumulator, never a `Ref` mutated inside `map`/`tap`, and a whole key space rides one immutable `HashMap` accumulator so per-key memory is a fold fact, not a cell registry.
- Nominal identity runs two disjoint plane regimes — `Schema.brand` marks admitted values with decode evidence inside Schema owners, an annotated `unique symbol` TypeId marks own carrier types; a brand minted on the wrong plane, or a string-literal brand field forgeable across modules, is the defect.

## [02]-[DISPATCH_AND_BOUNDARIES]

- One concern exposes one overloaded `function` entrypoint over one input union, signatures ordered narrow to wide; `resolve`/`resolveMany`/`resolveByKey` siblings, arity twins, `batch: boolean` beside a widened array, and data-first and curried twins are findings — pipe-versus-direct is one `Function.dual`.
- Closed families dispatch via `$match`/`Match.exhaustive`/`Match.tagsExhaustive`; an `orElse` fallback absorbing future tags of a closed family, a `switch` restating a vocabulary table, and an `as`/`typeof` ladder for structural dispatch are findings.
- `Schema.decodeUnknown(Owner)` sits on the first line seeing a foreign value, fixing owner, accumulation posture (`errors: "all"`), and drift posture (`onExcessProperty: "error"`) as one module-scope policy per seam; interior re-validation, `as` casts where a decode produces evidence, scattered `process.env` reads, top-level `Effect.runPromise`, and hand-built SQL or fetch in domain flow are findings.
- Every dependency is a Tag in the `R` channel satisfied through the Layer graph; the owner is one `Effect.Service` class. An `interface` + `Context.GenericTag` + `Layer.effect` triple restating one concept, module-level live singletons, parameter-drilled dependencies, and mock frameworks are findings.
- Layer memoization is by reference identity within one build: a layer minted by calling a factory twice is two nodes, so a shared resource declares once as a const every consumer composes, `Layer.fresh` is the only sharing opt-out, and the composition root carries an explicit `Layer.Layer<Out>` annotation so a missing edge fails at the declaration, not the run seam.
- A replaceable capability is a `Context.Tag` port no declaring or consuming file names an implementation for; the engine roster is one `as const satisfies Record<string, Layer.Layer<Port>>` table whose union derives `keyof typeof` and whose selection composes via `Layer.unwrapEffect` over the config read — a work definition importing its own runner has hardcoded the deployment into the domain.
- N identical lookups in one flow are one `Request.TaggedClass` family under one `RequestResolver.makeBatched` resolver settling every request; a `getMany` twin, a hand `Map` of in-flight promises as a dedup cache, and a resolver rebuilt per call site (re-minting identity, defeating the batch window) are findings.
- One contract-checked `as const satisfies` table is the single source of truth for a vocabulary; a hand-written union, `enum`, or parallel constant a table derives, and a `switch` over a keyed domain a vocabulary row maps, are findings. A value beside a generated union rides a stated `Record<GeneratedUnion, V>` annotation that breaks loudly on anchor growth, and consumption completeness closes with the `(x satisfies never)` residue proof in the terminal narrowing arm. `TS2589`/`TS2590` are architecture pressure — the repair is value-level derivation, never a directive.
- Owners pre-solve inference (`const` type parameter, `NoInfer`, instantiation expression, reverse-mapped parameter); a call site writing a type argument, an `as const`, or a literal re-assertion the owner's signature owes marks the owner's signature as the defect.
- Runtime invariants ride Effect primitives — `HashMap`/`Chunk`/`SortedMap` for state, `Data` + `Equal.equals` for identity, `DateTime`/`Duration` for time, `BigDecimal` for money, `BigInt` for foreign 64-bit coordinates, `Redacted` for secrets; JS `Map`/`Set`/`Date`, epoch math, `===` carrying domain meaning, sort-on-read, and get-then-set pairs are findings, and a broker offset, log sequence, or other 64-bit coordinate crossing through `Number` loses precision silently — arithmetic rides `BigInt` over the provider's native string spelling.
- Fibers parent to their forker or a `Scope`; `Effect.runFork` in domain flow, a `Map<string, Fiber>` registry (use `FiberMap`), a polled `Ref` flag (use `Deferred`), a poison-pill sentinel on a `Queue` (use `Mailbox`), and a hand-counted in-flight bound (use `makeSemaphore`) are findings. A `Stream` is earned only by unbounded/over-time data, observe-before-end, or windowing; a `Stream` over bounded in-memory data is a finding.

## [03]-[MODULE_SHAPE]

Declarations author unexported with interiors `_`-prefixed; `export default`, re-export barrels (`export ... from`, `export *`), an `export` keyword on a body declaration, `enum`, runtime `namespace`, and constructor parameter properties are findings. `@ts-nocheck`/`@ts-ignore` anywhere, a `@ts-expect-error` spanning a live finding, and a construct only one compiler of the dual floor accepts are findings.
