# [API_CATALOGUE] fast-check

`fast-check` is the algebraic property-based generator spine: one abstract `Arbitrary<T>` whose functorial/monadic methods (`.map`/`.filter`/`.chain`) compose every derived generator, a runner pair (`assert` throwing, `check` reflecting) that seeds deterministically and shrinks any counterexample to a minimal witness, and a structured `RunDetails<T>` carrying the seed + `counterexamplePath` for byte-exact replay. The whole primitive/collection/string/web/typed-array roster is one `constraints`-parameterized family over that class, never a fixed recipe list. `projection` consumes it as the external oracle of `convergence/law` — `opLogEntryArb`/`permutedPairArb` mint only decode-admitted wire vocabulary, the `.chain`/`.map`/`shuffledSubarray` combinators build the delivery-order permutation as a seeded shrinkable input, and `@effect/vitest` `it.prop` binds the spine to the pure `conflictStep` reduction that `@stryker-mutator/core` then mutation-scores.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `fast-check`
- package: `fast-check` (4.8.0, MIT, © Nicolas Dubien)
- module format: dual ESM/CJS, `sideEffects: false`; one `fast-check` barrel (`lib/fast-check.d.ts`, 215 exports), no deep-import subpaths; the `default` export is the `fc` namespace
- runtime target: isomorphic (node, bun, browser, worker); zero runtime dependencies (the `pure-rand` RNG is vendored); no native addon
- plane: dev/test only — a `devDependency`; the arbitraries and runners never leak into a shipped bundle
- v4 law: the string family unified — `string({ unit })` subsumes the removed v3 `char`/`ascii`/`hexa`/`unicode`/`fullUnicode`/`string16bits` and `asciiString`/`unicodeString`/`hexaString`/`fullUnicodeString`; `Arbitrary` shed its `.noShrink`/`.noBias` methods to the top-level `noShrink`/`noBias`/`limitShrink` wrappers
- rail: proof / property-based

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the generator carrier and its value/RNG siblings
- rail: proof / property-based

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY]  | [CAPABILITY]                                                    |
| :-----: | :---------------------------- | :------------- | :------------------------------------------------------------- |
|  [01]   | `Arbitrary<T>`                | abstract class | typed generator; `.map`/`.filter`/`.chain` compose all derivations |
|  [02]   | `Value<T>`                    | class          | a generated value plus its shrink context                      |
|  [03]   | `Stream<T>`                   | class          | lazy `IterableIterator<T>` the shrink tree walks               |
|  [04]   | `Random`                      | class          | the `pure-rand` RNG wrapper threaded into `generate`           |
|  [05]   | `GeneratorValue`              | type alias     | callable in-predicate RNG produced by `gen()`                  |
|  [06]   | `IRawProperty<Ts, IsAsync>`   | interface      | property contract (`generate`/`shrink`/`run`)                  |
|  [07]   | `IProperty<Ts>` / `IPropertyWithHooks<Ts>` | interface | sync property; the `WithHooks` form adds `beforeEach`/`afterEach` |
|  [08]   | `IAsyncProperty<Ts>` / `IAsyncPropertyWithHooks<Ts>` | interface | async property + async hooks             |
|  [09]   | `WithCloneMethod` / `WithToStringMethod` | mixin type | opt-in `cloneMethod`/`toStringMethod` protocols on a value     |

[PUBLIC_TYPE_SCOPE]: run configuration and the outcome union
- rail: proof / property-based

| [INDEX] | [SYMBOL]                            | [TYPE_FAMILY]       | [CAPABILITY]                                     |
| :-----: | :---------------------------------- | :------------------ | :----------------------------------------------- |
|  [01]   | `Parameters<T>`                     | interface           | per-run config: `seed`, `numRuns`, `path`, `examples` (regression corpus tried before the random draws), `timeout`, `interruptAfterTimeLimit`, `markInterruptAsFailure`, `reporter`, `verbose`, `endOnFailure` |
|  [02]   | `GlobalParameters`                  | type alias          | the ambient subset `configureGlobal` accepts — every `Parameters` field except `path`/`examples` |
|  [03]   | `RunDetails<Ts>`                    | discriminated union | success or one of three failure variants         |
|  [04]   | `RunDetailsCommon<Ts>`              | interface           | shared fields: `seed`, `numRuns`, `counterexample`, `counterexamplePath` |
|  [05]   | `RunDetailsFailureProperty<Ts>`     | interface           | property broke — carries the shrunk counterexample |
|  [06]   | `RunDetailsFailureTooManySkips<Ts>` | interface           | `pre(false)` skip budget exhausted               |
|  [07]   | `RunDetailsFailureInterrupted<Ts>`  | interface           | time-limit interruption                          |
|  [08]   | `PropertyFailure`                   | interface           | `{ error, errorMessage? }` failure carrier       |
|  [09]   | `PreconditionFailure`               | class (Error)       | thrown by `pre(false)` inside a predicate         |

[PUBLIC_TYPE_SCOPE]: size, depth, verbosity, and execution-tree policy
- rail: proof / property-based

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY] | [CAPABILITY]                                                    |
| :-----: | :------------------------------ | :------------ | :------------------------------------------------------------- |
|  [01]   | `Size`                          | string union  | `"xsmall" \| "small" \| "medium" \| "large" \| "xlarge"`        |
|  [02]   | `RelativeSize` / `SizeForArbitrary` | union     | `"-4"…"+4" \| "="`; `SizeForArbitrary = Size \| RelativeSize \| "max" \| undefined` |
|  [03]   | `DepthSize` / `DepthContext` / `DepthIdentifier` | policy | recursion budget for `letrec`/`memo` depth control      |
|  [04]   | `RandomType`                    | string union  | `"mersenne" \| "congruential32" \| "xorshift128plus" \| "xoroshiro128plus"` |
|  [05]   | `VerbosityLevel`                | enum          | `None = 0`, `Verbose = 1`, `VeryVerbose = 2`                   |
|  [06]   | `ExecutionStatus` / `ExecutionTree<Ts>` | enum + node | `Success = 0`, `Skipped = -1`, `Failure = 1`; the run tree the reporter walks |
|  [07]   | `*Constraints`                  | interface set | one options record per arbitrary (`ArrayConstraints`, `StringConstraints`, `IntegerConstraints`, `DoubleConstraints`, `RecordConstraints`, `WebUrlConstraints`, `UniqueArrayConstraints`, …) — the parameterization surface |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the `Arbitrary<T>` combinator methods — the composition mechanism
- rail: proof / property-based

| [INDEX] | [SURFACE]                                    | [ENTRY_FAMILY] | [CAPABILITY]                                                    |
| :-----: | :------------------------------------------- | :------------- | :------------------------------------------------------------- |
|  [01]   | `arb.map<U>(mapper, unmapper?)`              | functor        | project each value; the optional `unmapper` restores shrinking through the map |
|  [02]   | `arb.filter<U extends T>(refinement)`        | refinement     | keep values passing a predicate/type-guard (over-filtering slows generation) |
|  [03]   | `arb.chain<U>(chainer)`                      | monad          | draw a second arbitrary from a first value — the dependent-draw the permutation law rides |
|  [04]   | `noShrink(arb)` / `noBias(arb)` / `limitShrink(arb, n)` | modifier | disable shrinking, disable size bias, or cap shrink steps (v4 top-level, off the class) |
|  [05]   | `clone(arb, n)`                              | duplication    | `Arbitrary<CloneValue<T, n>>` — n referentially-cloned shrinkable draws (via the `cloneMethod` protocol) for a law over a repeated-identical input |

[ENTRYPOINT_SCOPE]: property construction and the run surface
- rail: proof / property-based

| [INDEX] | [SURFACE]                                      | [ENTRY_FAMILY]      | [CAPABILITY]                                     |
| :-----: | :--------------------------------------------- | :------------------ | :----------------------------------------------- |
|  [01]   | `property<Ts>(...arbs, predicate)`             | sync property       | ordered arbitraries + typed predicate → `IPropertyWithHooks` |
|  [02]   | `asyncProperty<Ts>(...arbs, predicate)`        | async property      | the `Promise`-returning predicate form           |
|  [03]   | `assert(property, params?)`                    | throwing runner     | throws with the seed + shrunk counterexample in the message |
|  [04]   | `check(property, params?)`                     | reflecting runner   | returns `RunDetails<Ts>` for custom reporting/gating |
|  [05]   | `pre(expectTruthy)`                            | precondition        | cancels the run (increments `numSkips`) when falsy |
|  [06]   | `sample(arb, params? \| n)` / `statistics(arb, classify, params?)` | inspector | extract a value array / log a frequency distribution (test-authoring only, not in a predicate) |
|  [07]   | `gen()`                                        | inline RNG          | `Arbitrary<GeneratorValue>` — a callable RNG usable inside a predicate (partial shrink, no path replay) |
|  [08]   | `configureGlobal` / `readConfigureGlobal` / `resetConfigureGlobal` | global config | set/read/reset the ambient `GlobalParameters` (e.g. `numRuns`, `baseSize`, `seed`) |

[ENTRYPOINT_SCOPE]: the constraint-parameterized arbitrary family (representative — one `constraints` record per row)
- rail: proof / property-based

| [INDEX] | [SURFACE]                                       | [PRODUCES]                | [CAPABILITY]                                     |
| :-----: | :---------------------------------------------- | :------------------------ | :----------------------------------------------- |
|  [01]   | `constant(v)` / `constantFrom(...vs)` / `mapToConstant(...entries)` | `Arbitrary<T>` | fixed value, bounded set draw, exhaustive count→constant map |
|  [02]   | `boolean()` / `falsy(c?)`                       | scalar                    | `true`/`false`; the falsy set (`false`/`null`/`0`/`""`/`NaN`/`undefined`) |
|  [03]   | `integer(c?)` / `nat(max? \| c?)` / `maxSafeInteger()` / `maxSafeNat()` / `bigInt(c?)` | numeric | signed/unsigned/safe-range integers, arbitrary-precision `bigint` |
|  [04]   | `double(c?)` / `float(c?)`                      | numeric                   | IEEE doubles / float32-range doubles with `min`/`max`/`noNaN`/`noDefaultInfinity` |
|  [05]   | `string(c?)` / `stringMatching(regex, c?)` / `base64String(c?)` / `lorem(c?)` / `mixedCase(strArb, c?)` | `Arbitrary<string>` | v4-unified string via `{ unit }`; regex-conforming; base64; lorem; case-randomized |
|  [06]   | `emailAddress(c?)` / `domain(c?)` / `webUrl(c?)` / `webPath(c?)` / `webQueryParameters(c?)` / `webFragments(c?)` / `webSegment(c?)` / `webAuthority(c?)` / `ipV4()` / `ipV4Extended()` / `ipV6()` | `Arbitrary<string>` | RFC-conforming network/web string family |
|  [07]   | `uuid(c?)` / `ulid()` / `date(c?)`              | `Arbitrary<string \| Date>` | identifier and temporal generators              |
|  [08]   | `array(arb, c?)` / `uniqueArray(arb, c?)` / `sparseArray(arb, c?)` / `tuple(...arbs)` / `set(...)` | collection | sized/deduped/sparse arrays, fixed typed tuples |
|  [09]   | `record(model, c?)` / `dictionary(keyArb, valArb, c?)` / `object(c?)` | structured | per-key record, keyed object, arbitrary object |
|  [10]   | `json(c?)` / `jsonValue(c?)` / `anything(c?)`   | structured                | JSON-shaped / any-value trees                    |
|  [11]   | `int8Array`/`uint8Array`/`uint8ClampedArray`/`int16Array`/`uint16Array`/`int32Array`/`uint32Array`/`float32Array`/`float64Array`/`bigInt64Array`/`bigUint64Array`(c?) | typed array | one row per `TypedArray` kind over a numeric element arbitrary |
|  [12]   | `oneof(...arbs \| {weight,arbitrary})` / `option(arb, c?)` | union    | weighted union; `T \| null` (nil configurable)   |
|  [13]   | `letrec(builder)` / `memo(builder)`             | recursive                 | mutual-recursion fixpoint; depth-cached recursive arbitrary |
|  [14]   | `func(arb)` / `compareFunc()` / `compareBooleanFunc()` | function     | pure deterministic function arbitraries (shrinkable, printable) |
|  [15]   | `shuffledSubarray(items, c?)` / `subarray(items, c?)` | permutation | seeded sub-permutation of a source array — the delivery-order draw of the convergence law |
|  [16]   | `context()` / `infiniteStream(arb)` / `stream(it)` | inspection/stream | in-predicate logging sink; lazy infinite/finite value streams |
|  [17]   | `commands(cmdArbs, c?)` / `modelRun` / `asyncModelRun` / `scheduledModelRun` | model-based | `Iterable<Command<M,R>>` stateful-model testing + its runners |
|  [18]   | `scheduler(c?)` / `schedulerFor(order)`         | async ordering            | `Arbitrary<Scheduler>` exploring async interleavings/races |
|  [19]   | `entityGraph(relations, c?)`                    | relational                | a consistent entity graph over declared relations |
|  [20]   | `hash(v)` / `stringify(v)` / `asyncStringify(v)` | utility                  | stable hashing and value pretty-printing for reporters |

## [04]-[IMPLEMENTATION_LAW]

[GENERATOR_TOPOLOGY]:
- one abstract `Arbitrary<T>` owns every generator; `map`/`filter`/`chain` are the only composition verbs, so a derived generator is always the base spine plus lawful transforms — never a bespoke `Math.random` sampler. Every leaf arbitrary is `constraints`-parameterized; a new shape is a `constraints` field or a `map`/`chain` off an existing arbitrary, never a parallel recipe function.
- `.chain` is the dependent draw: `opSetArb.chain((ops) => shuffledSubarray(indices).map((order) => [ops, order.map((i) => ops[i])]))` mints the op-set and one full-length permutation of it as a single paired value, so the permutation is a seeded, shrinkable, replayable input — never an un-seeded in-predicate `sample` whose draw makes a counterexample irreproducible.
- `.map(mapper, unmapper?)` keeps shrinking alive across a projection only when the `unmapper` is supplied; a lossy `map` shrinks the pre-image, so the wire-shaped `record(...).map((stamp) => ({ ...FIXED_OP, ...stamp }))` stays shrinkable on the stamp fields it varies.
- `assert(property, params?)` is the test entry (throws the shrunk witness + seed); `check` is the reflecting form for a gate that folds `RunDetails` itself. Replay is deterministic: `{ seed, path }` from a failure's `counterexamplePath` re-runs the exact minimal witness, and `Parameters.examples` seeds a fixed regression corpus tried before the random draws (excluded from `GlobalParameters`, so it stays per-property).

[V4_ADMISSION]:
- import from the `fast-check` barrel only; there is no deep-import subpath in the `dist` layout, and the `default` export is the `fc` namespace (`import fc from "fast-check"` or `import * as fc from "fast-check"`).
- the string arbitraries are one `string({ unit })` in v4 (`unit` = `"grapheme"`/`"grapheme-ascii"`/`"binary"`/`"binary-ascii"` or a char arbitrary); the v3 single-char and per-alphabet string arbitraries are gone — target them through `string`/`constantFrom`.
- `noShrink`/`noBias`/`limitShrink` are top-level wrappers in v4 (the `Arbitrary` methods were removed); size defaults to `"small"` and is overridable per-arbitrary via `constraints.size` or globally via `configureGlobal({ baseSize })`.
- seeds are 32-bit; `pre(false)` past `maxSkipsPerRun × numRuns` fails as `RunDetailsFailureTooManySkips`; `interruptAfterTimeLimit` yields `RunDetailsFailureInterrupted`.

[STACKING]:
- `effect` `Schema` → generator: `Arbitrary.make(schema)` returns a `FastCheck.Arbitrary<A>` (`effect` re-exports fast-check as its `FastCheck` module), so a decoded wire `Schema` derives its arbitrary for free; a `Schema` may carry an `arbitrary` annotation overriding the derived generator. `convergence/law` opts OUT of derivation on purpose — it hand-writes `opLogEntryArb`/`conflictReceiptArb` so the bounded `CollisionBounds.origins`/`keyBytes` sets force key/stamp collisions the derived generator would scatter — but the derivation rail is the default for any spec that only needs "some valid value".
- `@effect/vitest` `it.prop(name, [...arbs], ([values], ctx) => Effect | boolean, { fastCheck? })` binds the arbitraries to the Effect-aware runner under `TestClock`/`TestRandom` (virtual, seeded) — the `fastCheck` opts forward `Parameters` (`numRuns`/`seed`/`endOnFailure`) for run and replay control — and `it.scoped.prop`/`it.effect.prop` do the same for scoped/effectful bodies (`query/window` binds the live `windowFold` under a `Scope` through `it.scoped.prop`, deliberately not `it.prop`); `Vitest.Arbitraries` accepts `Schema.Schema.Any | FC.Arbitrary`, so a raw fast-check arbitrary and a `Schema` are interchangeable at the `it.prop` call site. The `convergence/law` suite folds `conflictStep` inside the `it.prop` body and asserts `Equal.equals` over the `HashMap`/`HashSet` state — structural byte-identity, not a hand-rolled deep compare.
- `@stryker-mutator/core` mutation-scores the property suite: a mutated `conflictStep` guard must break the delivery-order/idempotence/tombstone laws, so the kill ratio is the strength signal on the generators — a thin arbitrary that never collides survives mutants and fails the gate.

[RAIL_LAW]:
- package: `fast-check`
- owns: typed generator construction, property assertion, deterministic seeding, and automatic counterexample shrinking
- accept: `Arbitrary<T>` composed through `.map`/`.filter`/`.chain`; `constraints`-parameterized leaves; `Parameters.seed`/`path` for replay; `Arbitrary.make(schema)` when a `Schema` already models the shape
- reject: `Math.random()` or hand-rolled sampling in a predicate; a manual shrink loop; an un-seeded in-predicate `sample`/`gen` where a `.chain` draw would shrink and replay; a v3 per-alphabet string arbitrary where `string({ unit })` or `constantFrom` covers it
