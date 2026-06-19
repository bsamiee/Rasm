# [API_CATALOGUE] fast-check

`fast-check` supplies property-based testing for TypeScript: typed `Arbitrary<T>` generators, synchronous and asynchronous property wrappers (`property`/`asyncProperty`), the `assert`/`check` runners with deterministic seeding and automatic shrinking, a `gen()` inline value generator for in-predicate randomness, and a structured `RunDetails<T>` result carrying the counterexample path, seed, shrink count, and execution summary. The convergence and window-fold law suites consume `Arbitrary`, `property`/`asyncProperty`, `assert`/`check`, `Parameters`, and the primitive arbitraries (`nat`, `integer`, `string`, `array`, `record`, `tuple`, `oneof`, `constant`, `boolean`, `date`, `uuid`, `letrec`) to produce mutation-killable algebraic specs.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `fast-check`
- package: `fast-check`
- module: `fast-check` (single barrel; deep imports not present)
- asset: `lib/fast-check.d.ts`
- rail: testing / property-based

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: core abstractions
- rail: testing / property-based

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY]  | [CAPABILITY]                                    |
| :-----: | :---------------------------- | :------------- | :---------------------------------------------- |
|  [01]   | `Arbitrary<T>`                | abstract class | typed value generator with shrink tree          |
|  [02]   | `Value<T>`                    | class          | generated value with clone and context metadata |
|  [03]   | `Stream<T>`                   | class          | lazy `IterableIterator<T>` for shrink streams   |
|  [04]   | `Random`                      | class          | mutable pure-rand RNG wrapper                   |
|  [05]   | `IRawProperty<Ts, IsAsync>`   | interface      | property contract (generate / shrink / run)     |
|  [06]   | `IProperty<Ts>`               | interface      | synchronous property                            |
|  [07]   | `IAsyncProperty<Ts>`          | interface      | asynchronous property                           |
|  [08]   | `IPropertyWithHooks<Ts>`      | interface      | sync property with `beforeEach`/`afterEach`     |
|  [09]   | `IAsyncPropertyWithHooks<Ts>` | interface      | async property with hooks                       |
|  [10]   | `GeneratorValue`              | type alias     | callable in-predicate RNG with shrink support   |

[PUBLIC_TYPE_SCOPE]: configuration and result shapes
- rail: testing / property-based

| [INDEX] | [SYMBOL]                            | [TYPE_FAMILY]       | [CAPABILITY]                                  |
| :-----: | :---------------------------------- | :------------------ | :-------------------------------------------- |
|  [01]   | `Parameters<T>`                     | interface           | per-run config: seed, runs, timeout, reporter |
|  [02]   | `GlobalParameters`                  | type alias          | subset of `Parameters` for global override    |
|  [03]   | `RunDetails<Ts>`                    | discriminated union | run outcome: success or failure variant       |
|  [04]   | `RunDetailsFailureProperty<Ts>`     | interface           | property failure with counterexample          |
|  [05]   | `RunDetailsFailureTooManySkips<Ts>` | interface           | skip-budget exhausted failure                 |
|  [06]   | `RunDetailsFailureInterrupted<Ts>`  | interface           | timeout-interrupted failure                   |
|  [07]   | `RunDetailsSuccess<Ts>`             | interface           | successful run                                |
|  [08]   | `RunDetailsCommon<Ts>`              | interface           | shared fields: seed, numRuns, counterexample  |
|  [09]   | `PropertyFailure`                   | type                | `{ error: unknown }` run failure carrier      |
|  [10]   | `PreconditionFailure`               | class (Error)       | failure from `pre(false)` guard               |

[PUBLIC_TYPE_SCOPE]: size and depth policy
- rail: testing / property-based

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [CAPABILITY]                                             |
| :-----: | :------------------ | :------------ | :------------------------------------------------------- |
|  [01]   | `Size`              | string union  | `"xsmall" \| "small" \| "medium" \| "large" \| "xlarge"` |
|  [02]   | `RelativeSize`      | string union  | `"-4"` through `"+4"` and `"="`                          |
|  [03]   | `SizeForArbitrary`  | type alias    | `Size \| RelativeSize \| "max" \| undefined`             |
|  [04]   | `DepthSize`         | type alias    | `SizeForArbitrary \| number`                             |
|  [05]   | `RandomType`        | string union  | `"mersenne" \| "xorshift128plus"` etc.                   |
|  [06]   | `VerbosityLevel`    | enum          | `None = 0`, `Verbose = 1`, `VeryVerbose = 2`             |
|  [07]   | `ExecutionStatus`   | enum          | `Success = 0`, `Skipped = -1`, `Failure = 1`             |
|  [08]   | `ExecutionTree<Ts>` | interface     | run tree node with status and children                   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: property and runner
- rail: testing / property-based

| [INDEX] | [SURFACE]                                      | [ENTRY_FAMILY]      | [CAPABILITY]                                     |
| :-----: | :--------------------------------------------- | :------------------ | :----------------------------------------------- |
|  [01]   | `property<Ts>(...arbs, predicate)`             | sync property       | constructs `IPropertyWithHooks<Ts>`              |
|  [02]   | `asyncProperty<Ts>(...arbs, predicate)`        | async property      | constructs `IAsyncPropertyWithHooks<Ts>`         |
|  [03]   | `assert<Ts>(property, params?)`                | throwing runner     | throws on first counterexample                   |
|  [04]   | `check<Ts>(property, params?)`                 | non-throwing runner | returns `RunDetails<Ts>` for inspection          |
|  [05]   | `sample<Ts>(generator, params? \| number)`     | sample extractor    | extracts array of generated values               |
|  [06]   | `statistics<Ts>(generator, classify, params?)` | classifier          | logs frequency distribution                      |
|  [07]   | `pre(expectTruthy)`                            | precondition        | cancels run when `expectTruthy` is falsy         |
|  [08]   | `configureGlobal(parameters)`                  | global config       | sets global `Parameters` override                |
|  [09]   | `readConfigureGlobal()`                        | global config       | reads current global `GlobalParameters`          |
|  [10]   | `resetConfigureGlobal()`                       | global config       | resets global parameters to defaults             |
|  [11]   | `gen()`                                        | inline generator    | `Arbitrary<GeneratorValue>` for in-predicate RNG |

[ENTRYPOINT_SCOPE]: primitive arbitraries — scalars
- rail: testing / property-based

| [INDEX] | [SURFACE]                            | [PRODUCES]              | [CAPABILITY]                                   |
| :-----: | :----------------------------------- | :---------------------- | :--------------------------------------------- |
|  [01]   | `nat(max? \| constraints?)`          | `Arbitrary<number>`     | integer in `[0, max]`                          |
|  [02]   | `integer(constraints?)`              | `Arbitrary<number>`     | signed integer with min/max bounds             |
|  [03]   | `maxSafeInteger()`                   | `Arbitrary<number>`     | full safe integer range                        |
|  [04]   | `maxSafeNat()`                       | `Arbitrary<number>`     | non-negative safe integer range                |
|  [05]   | `double(constraints?)`               | `Arbitrary<number>`     | IEEE double with bounds, NaN, Infinity         |
|  [06]   | `float(constraints?)`                | `Arbitrary<number>`     | float32-range double                           |
|  [07]   | `boolean()`                          | `Arbitrary<boolean>`    | true / false                                   |
|  [08]   | `bigInt(min?, max? \| constraints?)` | `Arbitrary<bigint>`     | arbitrary-precision integer                    |
|  [09]   | `constant<T>(value)`                 | `Arbitrary<T>`          | always returns `value`                         |
|  [10]   | `constantFrom<T>(...values)`         | `Arbitrary<T>`          | one value from the provided set                |
|  [11]   | `falsy(constraints?)`                | `Arbitrary<FalsyValue>` | `false \| null \| 0 \| "" \| NaN \| undefined` |
|  [12]   | `date(constraints?)`                 | `Arbitrary<Date>`       | date with optional min/max bounds              |

[ENTRYPOINT_SCOPE]: primitive arbitraries — strings and structured
- rail: testing / property-based

| [INDEX] | [SURFACE]                                          | [PRODUCES]                | [CAPABILITY]                           |
| :-----: | :------------------------------------------------- | :------------------------ | :------------------------------------- |
|  [01]   | `string(constraints?)`                             | `Arbitrary<string>`       | unicode string with size control       |
|  [02]   | `stringMatching(regex, constraints?)`              | `Arbitrary<string>`       | string matching a `RegExp`             |
|  [03]   | `base64String(constraints?)`                       | `Arbitrary<string>`       | base64-valid string                    |
|  [04]   | `emailAddress(constraints?)`                       | `Arbitrary<string>`       | RFC-valid email address                |
|  [05]   | `domain(constraints?)`                             | `Arbitrary<string>`       | RFC-valid domain name                  |
|  [06]   | `ipV4()`                                           | `Arbitrary<string>`       | IPv4 dotted-decimal address            |
|  [07]   | `ipV6()`                                           | `Arbitrary<string>`       | IPv6 address                           |
|  [08]   | `webUrl(constraints?)`                             | `Arbitrary<string>`       | valid URL string                       |
|  [09]   | `lorem(constraints?)`                              | `Arbitrary<string>`       | lorem-ipsum words or sentences         |
|  [10]   | `array<T>(arb, constraints?)`                      | `Arbitrary<T[]>`          | array with size and uniqueness control |
|  [11]   | `uniqueArray<T, U>(arb, constraints?)`             | `Arbitrary<T[]>`          | array with deduplication policy        |
|  [12]   | `tuple<Ts>(...arbs)`                               | `Arbitrary<Ts>`           | fixed-length typed tuple               |
|  [13]   | `record<T>(model, constraints?)`                   | `Arbitrary<T>`            | record with per-key arbitraries        |
|  [14]   | `dictionary<K, V>(keyArb, valueArb, constraints?)` | `Arbitrary<Record<K, V>>` | keyed object                           |
|  [15]   | `oneof<Ts>(...arbs \| constraints, ...arbs)`       | `Arbitrary<T>`            | weighted union of arbitraries          |

[ENTRYPOINT_SCOPE]: combinators and model-based
- rail: testing / property-based

| [INDEX] | [SURFACE]                                   | [ENTRY_FAMILY] | [CAPABILITY]                              |
| :-----: | :------------------------------------------ | :------------- | :---------------------------------------- |
|  [01]   | `option<T, TNil>(arb, constraints?)`        | combinator     | `T \| TNil` (defaults nil to `null`)      |
|  [02]   | `mapToConstant(...entries)`                 | combinator     | maps counts to constants exhaustively     |
|  [03]   | `letrec<T>(builder)`                        | recursive      | mutual recursion with fixedpoint          |
|  [04]   | `memo<T>(builder)`                          | recursive      | cached recursive arbitrary by depth       |
|  [05]   | `mixedCase(stringArb, constraints?)`        | combinator     | randomizes casing of string arbitrary     |
|  [06]   | `func<TArgs, TOut>(arb)`                    | combinator     | pure deterministic function arbitrary     |
|  [07]   | `commands<M, R>(commandArbs, constraints?)` | model-based    | `Iterable<Command<M, R>>` for model tests |
|  [08]   | `scheduler<TMetaData>(constraints?)`        | async ordering | `Arbitrary<Scheduler>` for async races    |
|  [09]   | `gen()`                                     | inline RNG     | `GeneratorValue` for in-predicate values  |
|  [10]   | `noShrink<T>(arb)`                          | modifier       | disables shrinking on an arbitrary        |
|  [11]   | `noBias<T>(arb)`                            | modifier       | disables size bias on an arbitrary        |
|  [12]   | `limitShrink<T>(arb, maxShrinks)`           | modifier       | caps shrink steps to `maxShrinks`         |

## [04]-[IMPLEMENTATION_LAW]

[PROPERTY_TOPOLOGY]:
- `property` / `asyncProperty` compose ordered `Arbitrary` inputs with a typed `predicate`; the runner feeds each generated `Ts` to the predicate
- `assert` is the primary test-framework entry point — it throws on failure with the counterexample and seed embedded in the message; `check` is the non-throwing form for custom reporting
- `gen()` produces a `GeneratorValue` that acts as a callable in-predicate RNG; its values shrink partially but cannot replay with a path-suffix — use tailored `filter`/`map` arbitraries when full replay is required
- `IPropertyWithHooks.beforeEach`/`afterEach` are synchronous; `IAsyncPropertyWithHooks` carries the async variants; never pass an async hook to the synchronous overload

[LOCAL_ADMISSION]:
- Import from `fast-check` only; no deep-import path exists in the `dist` layout
- Size control for collection and string arbitraries defaults to `"small"`; override via `constraints.size` or `configureGlobal({ baseSize })`
- Seeds are 32-bit integers; float seeds are rescaled; replay requires both `seed` and `counterexamplePath` from `RunDetails`
- `pre(false)` increments `numSkips`; exceeding `maxSkipsPerRun * numRuns` fails the run with `RunDetailsFailureTooManySkips`

[RAIL_LAW]:
- Package: `fast-check`
- Owns: typed generator construction, property assertion, and automatic counterexample shrinking
- Accept: typed `Arbitrary<T>` composition; `Parameters` for seed, run count, and timeout configuration
- Reject: hand-rolled random sampling, manual shrink loops, or raw `Math.random()` inside property predicates
