# [TS_TESTS_API_FAST_CHECK]

[PACKAGE_SURFACE]:
- package: `fast-check` · version `3.23.2` · license `MIT`
- arrival: transitive under `effect` — never a workspace admission; the estate reaches the engine only through the `effect/FastCheck` re-export, so the version rides `effect`'s own manifest and moves with the substrate wave.
- module: dual CJS/ESM barrel `fast-check` — one `.` export, no deep-import paths; default export `fc` mirrors the named surface.
- asset: `lib/types/fast-check-default.d.ts` (barrel declarations); runtime `lib/fast-check.js` + `lib/esm/`.
- runtime: platform-neutral (node / bun / browser / worker); zero native or wasm; randomness via bundled `pure-rand`.
- plane: `plane:dev` — the `tests/typescript/_architecture` suite refuses a direct `fast-check` manifest admission outright, so no runtime graph can ever import it.
- rail: property / generative law.

`fast-check` is the sole generator engine behind the `_testkit` law/arbitrary source (`tests/typescript/_testkit`), which derives every kernel-brand and decoded-wire `Arbitrary<T>` from an `effect/Schema` via `Arbitrary.make` (never hand-rolls one) and folds those arbitraries into the reusable law combinators — each a `property`/`asyncProperty` closed by `assert` and bound to specs through `@effect/vitest` `it.effect.prop` / `it.effect`. String variation rides the one `string(constraints)` with its `unit` axis (shipped since `3.22.0`) and weighting rides `oneof` rows; the deprecated char-family (`char`, `ascii`, `hexaString`, `fullUnicodeString`, `stringOf`) still ships on this line and is rejected wholesale.

## [01]-[CORE_TYPES]

[PUBLIC_TYPE_SCOPE]: the generator algebra — one abstract owner, everything else is a row on it.

| [INDEX] | [SYMBOL]                                                   | [TYPE_FAMILY]       | [CAPABILITY]                                         |
| :-----: | :--------------------------------------------------------- | :------------------ | :--------------------------------------------------- |
|  [01]   | `Arbitrary<T>`                                             | abstract class      | typed generator; owns `map`/`filter`/`chain`         |
|  [02]   | `Value<T>`                                                 | class               | one generated value + `context` metadata             |
|  [03]   | `Stream<T>`                                                | class               | lazy `IterableIterator<T>` shrink stream             |
|  [04]   | `Random`                                                   | class               | the only RNG a predicate touches (`pure-rand`)       |
|  [05]   | `IRawProperty<Ts, IsAsync>`                                | interface           | property contract: `generate`/`shrink`/`run`         |
|  [06]   | `IProperty<Ts>` / `IPropertyWithHooks<Ts>`                 | interface           | sync property (+ `beforeEach`/`afterEach`)           |
|  [07]   | `IAsyncProperty<Ts>` / `…WithHooks`                        | interface           | async property; async hooks on the async variant     |
|  [08]   | `GeneratorValue`                                           | type alias          | callable in-predicate RNG from `gen()`               |
|  [09]   | `Parameters<T>` / `GlobalParameters`                       | interface + alias   | per-run config; global subset via `configureGlobal`  |
|  [10]   | `RunDetails<Ts>`                                           | discriminated union | `RunDetailsSuccess` \| three failure variants        |
|  [11]   | `PreconditionFailure`                                      | class (Error)       | thrown by `pre(false)` when the skip budget is spent |
|  [12]   | `ExecutionStatus` / `ExecutionTree<Ts>` / `VerbosityLevel` | enum + interface    | per-run replay tree feeding verbose reporters        |
|  [13]   | `Size` / `SizeForArbitrary` / `DepthSize`                  | union aliases       | size policy; `Size` = `"xsmall".."xlarge"`           |
|  [14]   | `WithCloneMethod` / `WithToStringMethod` / `cloneMethod`   | branded shape       | stateful-value cloning + counterexample rendering    |

```ts signature
// Arbitrary algebra — every primitive/combinator below returns one of these; refine with three methods.
abstract class Arbitrary<T> {
  abstract generate(mrng: Random, biasFactor: number | undefined): Value<T>
  abstract canShrinkWithoutContext(value: unknown): value is T
  abstract shrink(value: T, context: unknown | undefined): Stream<Value<T>>
  map<U>(mapper: (t: T) => U, unmapper?: (possiblyU: unknown) => T): Arbitrary<U>   // unmapper keeps shrinking sound
  filter<U extends T>(refinement: (t: T) => t is U): Arbitrary<U>
  filter(predicate: (t: T) => boolean): Arbitrary<T>                                 // INGRESS_BUDGET refinement rail
  chain<U>(chainer: (t: T) => Arbitrary<U>): Arbitrary<U>                            // value-dependent generation
}
// Shrink/bias modifiers are FREE functions, not methods:
declare function noShrink<T>(arb: Arbitrary<T>): Arbitrary<T>
declare function noBias<T>(arb: Arbitrary<T>): Arbitrary<T>
declare function limitShrink<T>(arb: Arbitrary<T>, maxShrinks: number): Arbitrary<T>
declare function clone<T>(arb: Arbitrary<T>, numValues: number): Arbitrary<T[]>      // N clones of one draw
```

## [02]-[RUNNERS]

[ENTRYPOINT_SCOPE]: property construction + execution — the surface every law combinator terminates in.

| [INDEX] | [SURFACE]                                                          | [ENTRY_FAMILY]      | [CAPABILITY]                                  |
| :-----: | :----------------------------------------------------------------- | :------------------ | :-------------------------------------------- |
|  [01]   | `property(...arbs, predicate)`                                     | sync property       | predicate returns `boolean \| void`           |
|  [02]   | `asyncProperty(...arbs, predicate)`                                | async property      | async predicate returns a `Promise`           |
|  [03]   | `assert(property, params?)`                                        | throwing runner     | throws with seed + minimal counterexample     |
|  [04]   | `check(property, params?)`                                         | non-throwing runner | returns `RunDetails<Ts>` for custom reporting |
|  [05]   | `sample(gen, params? \| number)`                                   | extractor           | `Ts[]` for corpus generation / debugging      |
|  [06]   | `statistics(gen, classify, params? \| number)`                     | classifier          | frequency distribution of a `classify` label  |
|  [07]   | `pre(expectTruthy)`                                                | precondition        | `asserts expectTruthy` — a false guard skips  |
|  [08]   | `gen()`                                                            | inline RNG          | in-predicate `Arbitrary<GeneratorValue>`      |
|  [09]   | `configureGlobal` / `readConfigureGlobal` / `resetConfigureGlobal` | global config       | one `GlobalParameters` override per suite     |
|  [10]   | `hash` / `stringify` / `asyncStringify` / `defaultReportMessage`   | reporting utils     | stable hashing + counterexample rendering     |

```ts signature
// Signatures verified against lib/types/fast-check-default.d.ts @3.23.2 (variadic-tuple arbitraries → typed predicate).
declare function property<Ts extends [unknown, ...unknown[]]>(
  ...args: [...arbitraries: { [K in keyof Ts]: Arbitrary<Ts[K]> }, predicate: (...args: Ts) => boolean | void]
): IPropertyWithHooks<Ts>
declare function asyncProperty<Ts extends [unknown, ...unknown[]]>(
  ...args: [...arbitraries: { [K in keyof Ts]: Arbitrary<Ts[K]> }, predicate: (...args: Ts) => Promise<boolean | void>]
): IAsyncPropertyWithHooks<Ts>
declare function assert<Ts>(property: IAsyncProperty<Ts>, params?: Parameters<Ts>): Promise<void>
declare function assert<Ts>(property: IProperty<Ts>, params?: Parameters<Ts>): void
declare function check<Ts>(property: IProperty<Ts>, params?: Parameters<Ts>): RunDetails<Ts>
declare function sample<Ts>(generator: IRawProperty<Ts> | Arbitrary<Ts>, params?: Parameters<Ts> | number): Ts[]
```

## [03]-[ARBITRARIES]

Full generator roster is SEED DATA for the one `Arbitrary<T>` algebra — a new shape is a row, never a new mechanism; the `_testkit` law/arbitrary source reaches almost none of these by hand: it derives from `Schema` (see [05]) and drops to raw arbitraries only for a shape `Schema` cannot express.

[ENTRYPOINT_SCOPE]: scalars — `nat`, `integer`, `maxSafeInteger`, `maxSafeNat`, `double`, `float`, `boolean`, `bigInt`, `constant`, `constantFrom`, `falsy`, `date`, `ulid`, `uuid` (all take a `*Constraints` bag; `constant<const T>` preserves literal type).

[ENTRYPOINT_SCOPE]: strings — `string(constraints)` (the one string owner; `constraints.unit` selects `'grapheme' | 'grapheme-composite' | 'grapheme-ascii' | 'binary' | 'binary-ascii' | Arbitrary<string>`, default `'grapheme-ascii'`, superseding the deprecated char-family), `stringMatching(regex)`, `base64String`, `lorem`, `mixedCase`, and the web/net family `emailAddress`, `domain`, `webUrl`, `webAuthority`, `webPath`, `webSegment`, `webFragments`, `webQueryParameters`, `ipV4`, `ipV4Extended`, `ipV6`.

[ENTRYPOINT_SCOPE]: collections + structured — `array`, `uniqueArray` (comparator/selector overload family), `subarray`, `shuffledSubarray`, `sparseArray`, `tuple`, `record`, `dictionary`, `mapToConstant`, `object`, `json`, `jsonValue`, `anything`, and the typed-array family `int8Array`…`float64Array`, `uint8Array`, `uint8ClampedArray`, `uint16Array`, `uint32Array`, `bigInt64Array`, `bigUint64Array`.

[ENTRYPOINT_SCOPE]: combinators + recursion — `oneof` (weighted via `MaybeWeightedArbitrary` / `WeightedArbitrary` rows or an `OneOfConstraints` head), `option`, `letrec` (typed + loosely-typed builders), `memo` (depth-cached), `func` (deterministic pure-function arbitrary), `context`, `infiniteStream`.

[ENTRYPOINT_SCOPE]: model-based + async — `commands` + `modelRun` / `asyncModelRun` / `scheduledModelRun` (stateful `Command`/`AsyncCommand` sequences), `scheduler` / `schedulerFor` (`Arbitrary<Scheduler>` for deterministic async-race ordering).

```ts signature
declare function string(constraints?: StringConstraints): Arbitrary<string>
declare function oneof<Ts extends MaybeWeightedArbitrary<unknown>[]>(...arbs: Ts): Arbitrary<OneOfValue<Ts>>
declare function oneof<Ts extends MaybeWeightedArbitrary<unknown>[]>(constraints: OneOfConstraints, ...arbs: Ts): Arbitrary<OneOfValue<Ts>>
declare function record<T>(model: { [K in keyof T]: Arbitrary<T[K]> }, constraints?: RecordConstraints<keyof T>): Arbitrary<T>
declare function letrec<T>(builder: T extends Record<string, unknown> ? LetrecTypedBuilder<T> : never): LetrecValue<T>
declare function constant<T>(value: T): Arbitrary<T>
declare function constantFrom<TArgs extends unknown[]>(...values: TArgs): Arbitrary<TArgs[number]> // the literal-preserving row picker
```

## [04]-[RUN_CONFIG_AND_RECEIPT]

`Parameters<T>` is the run policy; `RunDetails<T>` is the verified receipt `check` returns and `assert` embeds on throw. Replay a failure with `{ seed, path }` — both ride the receipt.

```ts signature
interface Parameters<T = void> {
  seed?: number                                  // 32-bit; replay anchor
  randomType?: RandomType | ((seed: number) => RandomGenerator)
  numRuns?: number                               // sample budget (default 100)
  maxSkipsPerRun?: number                        // pre() budget before RunDetailsFailureTooManySkips
  timeout?: number                               // per-predicate ms (async)
  interruptAfterTimeLimit?: number; markInterruptAsFailure?: boolean
  skipAllAfterTimeLimit?: number; skipEqualValues?: boolean; ignoreEqualValues?: boolean
  path?: string                                  // replay coordinate from a prior counterexample
  examples?: T[]                                 // always-run seeded cases (regression corpus)
  endOnFailure?: boolean; unbiased?: boolean; verbose?: boolean | VerbosityLevel
  reporter?: (r: RunDetails<T>) => void; asyncReporter?: (r: RunDetails<T>) => Promise<void>
  includeErrorInReport?: boolean; logger?(v: string): void
}
interface RunDetailsCommon<Ts> {
  failed: boolean; interrupted: boolean
  numRuns: number; numSkips: number; numShrinks: number; seed: number
  counterexample: Ts | null; counterexamplePath: string | null; errorInstance: unknown | null
  failures: Ts[]; executionSummary: ExecutionTree<Ts>[]
  verbose: VerbosityLevel; runConfiguration: Parameters<Ts>
}
```

## [05]-[INTEGRATION]

[STACK: `effect/Schema` → `Arbitrary.make` → `fast-check`] — the ONE arbitrary source in `tests/typescript/_testkit`. Every kernel brand (`ContentKey`, `Hlc`, `AppKey`, `Guid-v7`, `OrdinalKey`, SI `Quantity`) and decoded wire shape is a `Schema`; `Arbitrary.make(schema)` yields a `FastCheck.Arbitrary<A>` that already honors the schema's `filter`/refinement, so `INGRESS_BUDGET` decode budgets hold at generation time — never re-encoded as a hand-written `.filter`. `effect` re-exports the SAME engine as `effect/FastCheck`, so the arbitrary a spec composes and the engine a property runs are one instance.

```ts signature
import * as Arbitrary from "effect/Arbitrary"
import type * as FastCheck from "effect/FastCheck"       // re-export of this package — one engine, no version skew
import type { Schema } from "effect/Schema"
declare const make: <A, I, R>(schema: Schema.Schema<A, I, R>) => FastCheck.Arbitrary<A>
declare const makeLazy: <A, I, R>(schema: Schema.Schema<A, I, R>) => LazyArbitrary<A>   // LazyArbitrary<A> = (fc: typeof FastCheck) => FastCheck.Arbitrary<A>
```

[STACK: `fast-check` + `@effect/vitest`] — the `_testkit` law source closes each law with `assert`, but specs bind arbitraries through `@effect/vitest` `it.prop(name, arbitraries, self)` (array OR record of arbitraries) and effect-returning predicates through `it.effect` / `it.scoped`; `layer(SharedLayer)(…)` shares one acquired Layer across a property block so the harness resources (see `electric-sql-pglite.md`) build once. `it.flakyTest` wraps a known-nondeterministic effect.

[LAW COMBINATOR SHAPE] — the three reusable combinators are ONE parameterized rail, not three ad-hoc specs: a combinator takes `(arb: Arbitrary<A>, op)` and emits a `property`. Fold identity = `property(arb, a => equals(fold([a]), a))`; merge commutativity = `property(arb, arb, (a, b) => equals(merge(a, b), merge(b, a)))`; upcast totality = `property(arbLower, a => isRight(upcast(a)))`. New law = new row (arbs + predicate), never a new mechanism.

## [06]-[RAIL_LAW]

- Owns: typed generator construction, property assertion, deterministic replay (`{ seed, path }`), and automatic shrinking to a minimal counterexample.
- Accept: `Arbitrary<T>` composed via `map`/`filter`/`chain`; `Schema`-derived arbitraries via `Arbitrary.make`; `Parameters` for seed/run-count/timeout; `examples` for a seeded regression corpus.
- Reject: hand-rolled `Math.random()` inside a predicate (use `gen()` / `Random`); manual shrink loops; re-deriving a brand arbitrary by hand where a `Schema` exists; importing the barrel from any `plane:runtime` folder — dev subpath only.
- Boundary: async predicates require `asyncProperty` + `await assert(...)`; a sync `property` given an async predicate silently passes (the returned Promise is truthy). Discriminate `RunDetails` on its `failed` / `interrupted` flags, never on message text; replay a failure from `{ seed, counterexamplePath }`, not by re-running unseeded.
