# [API_CATALOGUE] @effect/vitest

`@effect/vitest` is the Effect-aware Vitest binding: it `export *`-forwards the entire `vitest` runtime surface and overlays an Effect test runner (`it.effect`/`scoped`/`live`/`scopedLive`), a `Layer`-sharing harness (`layer`/`describeWrapped`), a fast-check property runner (`prop`/`it.prop`), an `Equal`-trait-aware Vitest equality tester (`addEqualityTesters`), a flaky-retry combinator (`flakyTest`), and a closed family of `Equal`/`Option`/`Either`/`Exit`-aware assertion functions under the `utils` entry. Peer/import surface: `effect` (`Duration`, `Effect`, `FastCheck`, `Layer`, `Schema`, `Scope`, `TestServices`, `Cause`, `Either`, `Exit`, `Option`) and `vitest` (`TestAPI`, `TestFunction`, `TestContext`, `TestOptions`, `SuiteCollector`).

The package root `.` re-exports all of `vitest` plus the named runner/method symbols below; the
`@effect/vitest/utils` entry carries the assertion family. Every Effect-test combinator hangs off
the `Vitest.Methods` / `Vitest.Tester` interface tree, not parallel free functions.

```ts
// @effect/vitest
export * from "vitest"
export {
  addEqualityTesters, effect, scoped, live, scopedLive,
  layer, flakyTest, prop, it, makeMethods, describeWrapped
} from "@effect/vitest"
export declare namespace Vitest { /* TestFunction, Test, Arbitraries, Tester, MethodsNonLive, Methods */ }
// @effect/vitest/utils
export {
  fail, deepStrictEqual, notDeepStrictEqual, strictEqual, assertEquals,
  doesNotThrow, assertInstanceOf, assertTrue, assertFalse, assertInclude,
  assertMatch, throws, throwsAsync, assertNone, assertSome,
  assertLeft, assertRight, assertFailure, assertSuccess
} from "@effect/vitest/utils"
```

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@effect/vitest`
- package: `@effect/vitest`
- entry: `@effect/vitest` (root: `vitest` re-export + Effect runner/method surface), `@effect/vitest/utils` (Effect-aware assertions)
- asset: Effect test runner family (`it`/`effect`/`scoped`/`live`/`scopedLive`), `Layer`-sharing harness (`layer`/`describeWrapped`/`makeMethods`), fast-check property runner (`prop`/`Tester.prop`/`Tester.each`), `Equal`-trait Vitest equality tester (`addEqualityTesters`), flaky-retry combinator (`flakyTest`), closed assertion family over `Equal`/`Option`/`Either`/`Exit`/`Cause`
- rail: testing / effect-vitest
- tier: `neutral` (runtime-agnostic test harness; consumed under the Vitest node test runner but imports no `node:*` modules directly — the `effect` + `vitest` peers are the only dependencies)

## [02]-[PUBLIC_TYPES]

### @effect/vitest — Vitest namespace (runner type tree)

[PUBLIC_TYPE_SCOPE]: namespace interface tree
- rail: testing
- entry: `@effect/vitest` (`Vitest.*`)

| [INDEX] | [SYMBOL]                                        | [TYPE_FAMILY] | [CAPABILITY]             |
| :-----: | :---------------------------------------------- | :------------ | :----------------------- |
|  [01]   | `Vitest.TestFunction<A, E, R, TestArgs>`        | interface     | Effect test callback     |
|  [02]   | `Vitest.Test<R>`                                | interface     | Effect test registration |
|  [03]   | `Vitest.Arbitraries`                            | type alias    | property source shape    |
|  [04]   | `Vitest.Tester<R>`                              | interface     | tester plus modifiers    |
|  [05]   | `Vitest.MethodsNonLive<R, ExcludeTestServices>` | interface     | shared-layer method tree |
|  [06]   | `Vitest.Methods<R>`                             | interface     | full method tree         |

[PUBLIC_TYPE_SCOPE]: package-private root types (reached only through `it`/`API`)
- rail: testing
- entry: `@effect/vitest`

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY] | [CAPABILITY]         |
| :-----: | :------------------------- | :------------ | :------------------- |
|  [01]   | `API`                      | type alias    | Vitest API overlay   |
|  [02]   | `TestCollectorCallable<C>` | interface     | collector overloads  |
|  [03]   | `TestCollectorOptions`     | type alias    | collector option bag |

`Vitest.Methods` is the type of the exported `it`; `Vitest.MethodsNonLive` is what `layer(...)`
hands to its callback (no `live`/`scopedLive`, because a shared `Layer` is already memoized and a
real-clock run would defeat the shared-fiber model). `ExcludeTestServices extends true` drops the
`TestServices.TestServices` requirement from `effect`/`scoped` testers (use it when the layer
already supplies real services rather than the default test clock/console).

```ts contract
import type * as Duration from "effect/Duration"
import type * as Effect from "effect/Effect"
import type * as FC from "effect/FastCheck"
import type * as Layer from "effect/Layer"
import type * as Schema from "effect/Schema"
import type * as Scope from "effect/Scope"
import type * as TestServices from "effect/TestServices"
import type * as V from "vitest"

// --- root API (vitest TestAPI overlay) ---
type API = {
  scopedFixtures: V.TestAPI<{}>["scoped"]
} & {
  [K in keyof V.TestAPI<{}>]: K extends "scoped" ? unknown : V.TestAPI<{}>[K]
} & TestCollectorCallable

interface TestCollectorCallable<C = object> {
  /** @deprecated Use options as the second argument instead */
  <ExtraContext extends C>(name: string | Function, fn: V.TestFunction<ExtraContext>, options: TestCollectorOptions): void
  <ExtraContext extends C>(name: string | Function, fn?: V.TestFunction<ExtraContext>, options?: number | TestCollectorOptions): void
  <ExtraContext extends C>(name: string | Function, options?: TestCollectorOptions, fn?: V.TestFunction<ExtraContext>): void
}

type TestCollectorOptions = {
  concurrent?: boolean
  sequential?: boolean
  only?: boolean
  skip?: boolean
  todo?: boolean
  fails?: boolean
  timeout?: number
  retry?: number
  repeats?: number
}

// --- Vitest namespace: the Effect runner type tree ---
declare namespace Vitest {
  interface TestFunction<A, E, R, TestArgs extends Array<any>> {
    (...args: TestArgs): Effect.Effect<A, E, R>
  }

  interface Test<R> {
    <A, E>(name: string, self: TestFunction<A, E, R, [V.TestContext]>, timeout?: number | V.TestOptions): void
  }

  type Arbitraries =
    | Array<Schema.Schema.Any | FC.Arbitrary<any>>
    | { [K in string]: Schema.Schema.Any | FC.Arbitrary<any> }

  interface Tester<R> extends Vitest.Test<R> {
    skip: Vitest.Test<R>
    skipIf: (condition: unknown) => Vitest.Test<R>
    runIf: (condition: unknown) => Vitest.Test<R>
    only: Vitest.Test<R>
    each: <T>(cases: ReadonlyArray<T>) =>
      <A, E>(name: string, self: TestFunction<A, E, R, Array<T>>, timeout?: number | V.TestOptions) => void
    fails: Vitest.Test<R>
    prop: <const Arbs extends Arbitraries, A, E>(
      name: string,
      arbitraries: Arbs,
      self: TestFunction<A, E, R, [
        { [K in keyof Arbs]: Arbs[K] extends FC.Arbitrary<infer T> ? T : Schema.Schema.Type<Arbs[K]> },
        V.TestContext
      ]>,
      timeout?: number | V.TestOptions & {
        fastCheck?: FC.Parameters<{ [K in keyof Arbs]: Arbs[K] extends FC.Arbitrary<infer T> ? T : Schema.Schema.Type<Arbs[K]> }>
      }
    ) => void
  }

  interface MethodsNonLive<R = never, ExcludeTestServices extends boolean = false> extends API {
    readonly effect: Vitest.Tester<(ExcludeTestServices extends true ? never : TestServices.TestServices) | R>
    readonly flakyTest: <A, E, R2>(self: Effect.Effect<A, E, R2>, timeout?: Duration.DurationInput) => Effect.Effect<A, never, R2>
    readonly scoped: Vitest.Tester<(ExcludeTestServices extends true ? never : TestServices.TestServices) | Scope.Scope | R>
    readonly layer: <R2, E>(layer: Layer.Layer<R2, E, R>, options?: { readonly timeout?: Duration.DurationInput }) => {
      (f: (it: Vitest.MethodsNonLive<R | R2, ExcludeTestServices>) => void): void
      (name: string, f: (it: Vitest.MethodsNonLive<R | R2, ExcludeTestServices>) => void): void
    }
    readonly prop: <const Arbs extends Arbitraries>(
      name: string,
      arbitraries: Arbs,
      self: (
        properties: { [K in keyof Arbs]: Arbs[K] extends FC.Arbitrary<infer T> ? T : Schema.Schema.Type<Arbs[K]> },
        ctx: V.TestContext
      ) => void,
      timeout?: number | V.TestOptions & {
        fastCheck?: FC.Parameters<{ [K in keyof Arbs]: Arbs[K] extends FC.Arbitrary<infer T> ? T : Schema.Schema.Type<Arbs[K]> }>
      }
    ) => void
  }

  interface Methods<R = never> extends MethodsNonLive<R> {
    readonly live: Vitest.Tester<R>
    readonly scopedLive: Vitest.Tester<Scope.Scope | R>
  }
}
```

### @effect/vitest — runner + harness values

[PUBLIC_TYPE_SCOPE]: runner testers
- rail: testing
- entry: `@effect/vitest`

| [INDEX] | [SYMBOL]     | [TYPE_FAMILY] | [CAPABILITY]               |
| :-----: | :----------- | :------------ | :------------------------- |
|  [01]   | `it`         | const         | default method tree        |
|  [02]   | `effect`     | const tester  | test-service runner        |
|  [03]   | `scoped`     | const tester  | scoped test-service runner |
|  [04]   | `live`       | const tester  | real-clock runner          |
|  [05]   | `scopedLive` | const tester  | scoped real-clock runner   |
|  [06]   | `prop`       | const runner  | property runner            |

[PUBLIC_TYPE_SCOPE]: harness and setup values
- rail: testing
- entry: `@effect/vitest`

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY]    | [CAPABILITY]          |
| :-----: | :------------------- | :--------------- | :-------------------- |
|  [01]   | `flakyTest`          | const combinator | retry combinator      |
|  [02]   | `layer`              | const harness    | shared-layer harness  |
|  [03]   | `describeWrapped`    | const wrapper    | described method tree |
|  [04]   | `makeMethods`        | const factory    | custom method tree    |
|  [05]   | `addEqualityTesters` | const setup hook | equality tester setup |

```ts contract
import type * as Duration from "effect/Duration"
import type * as Effect from "effect/Effect"
import type * as Layer from "effect/Layer"
import type * as Scope from "effect/Scope"
import type * as TestServices from "effect/TestServices"
import type * as V from "vitest"

const addEqualityTesters: () => void

const effect: Vitest.Tester<TestServices.TestServices>
const scoped: Vitest.Tester<TestServices.TestServices | Scope.Scope>
const live: Vitest.Tester<never>
const scopedLive: Vitest.Tester<Scope.Scope>

// Share a Layer across a block, optionally wrapping the tests in a `describe`.
// `excludeTestServices: true` drops TestServices from the inner testers' requirement channel.
const layer: <R, E, const ExcludeTestServices extends boolean = false>(
  layer_: Layer.Layer<R, E>,
  options?: {
    readonly memoMap?: Layer.MemoMap
    readonly timeout?: Duration.DurationInput
    readonly excludeTestServices?: ExcludeTestServices
  }
) => {
  (f: (it: Vitest.MethodsNonLive<R, ExcludeTestServices>) => void): void
  (name: string, f: (it: Vitest.MethodsNonLive<R, ExcludeTestServices>) => void): void
}

// Retry until success or timeout; the result error channel is erased to `never`.
const flakyTest: <A, E, R>(self: Effect.Effect<A, E, R>, timeout?: Duration.DurationInput) => Effect.Effect<A, never, R>

const prop: Vitest.Methods["prop"]
const it: Vitest.Methods
const makeMethods: (it: API) => Vitest.Methods
const describeWrapped: (name: string, f: (it: Vitest.Methods) => void) => V.SuiteCollector
```

### @effect/vitest/utils — Effect-aware assertion family

[PUBLIC_TYPE_SCOPE]: assertion functions
- rail: testing / assertion
- entry: `@effect/vitest/utils`

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY]      | [CAPABILITY]                                                                        |
| :-----: | :------------------- | :----------------- | :---------------------------------------------------------------------------------- |
|  [01]   | `fail`               | assert             | throw `AssertionError` with `message`                                               |
|  [02]   | `strictEqual`        | assert             | `actual === expected` (also covered by `Equal.equals` trait wiring)                 |
|  [03]   | `deepStrictEqual`    | assert             | equal via `Equal.equals` trait                                                      |
|  [04]   | `notDeepStrictEqual` | assert             | not equal via `Equal.equals` trait                                                  |
|  [05]   | `assertEquals`       | assert             | equal via `Equal.equals` trait (alias-shaped peer of `deepStrictEqual`)             |
|  [06]   | `assertTrue`         | assert (narrows)   | `asserts self` — value is `true`/truthy                                             |
|  [07]   | `assertFalse`        | assert             | `self: boolean` is `false`                                                          |
|  [08]   | `assertInclude`      | assert             | `actual: string \| undefined` includes `expected`                                   |
|  [09]   | `assertMatch`        | assert             | `actual: string` matches `regexp: RegExp`                                           |
|  [10]   | `doesNotThrow`       | assert             | `thunk` does not throw                                                              |
|  [11]   | `throws`             | assert             | sync `thunk` throws (optional `Error \| (u) => undefined` predicate)                |
|  [12]   | `throwsAsync`        | assert (`Promise`) | async `thunk` rejects (optional predicate); returns `Promise<void>`                 |
|  [13]   | `assertInstanceOf`   | assert (narrows)   | `asserts value is InstanceType<C>`                                                  |
|  [14]   | `assertNone`         | assert (narrows)   | `asserts option is Option.None<never>`                                              |
|  [15]   | `assertSome`         | assert (narrows)   | `asserts option is Option.Some<A>`; checks payload equals `expected`                |
|  [16]   | `assertLeft`         | assert (narrows)   | `asserts either is Either.Left<L, never>`; checks `expected`                        |
|  [17]   | `assertRight`        | assert (narrows)   | `asserts either is Either.Right<never, R>`; checks `expected`                       |
|  [18]   | `assertFailure`      | assert (narrows)   | `asserts exit is Exit.Failure<never, E>`; checks `Cause.Cause<E>` equals `expected` |

[PUBLIC_TYPE_SCOPE]: assertion functions — exit and success
- rail: testing / assertion
- entry: `@effect/vitest/utils`

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY]    | [CAPABILITY]                                                               |
| :-----: | :-------------- | :--------------- | :------------------------------------------------------------------------- |
|  [01]   | `assertSuccess` | assert (narrows) | `asserts exit is Exit.Success<A, never>`; checks payload equals `expected` |

Every two-value assertion takes a trailing `..._: Array<never>` rest parameter — a compile-time
guard forbidding extra positional arguments (so a stray `expected` cannot be silently dropped).
`message?` is the optional final scalar where present.

```ts contract
import type * as Cause from "effect/Cause"
import type * as Either from "effect/Either"
import type * as Exit from "effect/Exit"
import type * as Option from "effect/Option"

function fail(message: string): void

function deepStrictEqual<A>(actual: A, expected: A, message?: string, ..._: Array<never>): void
function notDeepStrictEqual<A>(actual: A, expected: A, message?: string, ..._: Array<never>): void
function strictEqual<A>(actual: A, expected: A, message?: string, ..._: Array<never>): void
function assertEquals<A>(actual: A, expected: A, message?: string, ..._: Array<never>): void

function doesNotThrow(thunk: () => void, message?: string, ..._: Array<never>): void
function throws(thunk: () => void, error?: Error | ((u: unknown) => undefined), ..._: Array<never>): void
function throwsAsync(thunk: () => Promise<void>, error?: Error | ((u: unknown) => undefined), ..._: Array<never>): Promise<void>

function assertInstanceOf<C extends abstract new (...args: any) => any>(
  value: unknown, constructor: C, message?: string, ..._: Array<never>
): asserts value is InstanceType<C>

function assertTrue(self: unknown, message?: string, ..._: Array<never>): asserts self
function assertFalse(self: boolean, message?: string, ..._: Array<never>): void
function assertInclude(actual: string | undefined, expected: string, ..._: Array<never>): void
function assertMatch(actual: string, regexp: RegExp, ..._: Array<never>): void

function assertNone<A>(option: Option.Option<A>, ..._: Array<never>): asserts option is Option.None<never>
function assertSome<A>(option: Option.Option<A>, expected: A, ..._: Array<never>): asserts option is Option.Some<A>
function assertLeft<R, L>(either: Either.Either<R, L>, expected: L, ..._: Array<never>): asserts either is Either.Left<L, never>
function assertRight<R, L>(either: Either.Either<R, L>, expected: R, ..._: Array<never>): asserts either is Either.Right<never, R>
function assertFailure<A, E>(exit: Exit.Exit<A, E>, expected: Cause.Cause<E>, ..._: Array<never>): asserts exit is Exit.Failure<never, E>
function assertSuccess<A, E>(exit: Exit.Exit<A, E>, expected: A, ..._: Array<never>): asserts exit is Exit.Success<A, never>
```

## [03]-[ENTRYPOINTS]

The root entry `export * from "vitest"` forwards the entire `vitest` public surface unchanged —
`describe`, `expect`, `test`, `vi`, `beforeAll`/`afterAll`/`beforeEach`/`afterEach`, `assert`,
`TestContext`, `TestOptions`, `TestAPI`, `TestFunction`, `SuiteCollector`, etc. These are not
re-typed here; they carry the spellings of the installed `vitest` distribution. Two consequences
are load-bearing for callers:
- `expect` and `describe` are imported from `@effect/vitest`, not `vitest`, so the `Equal`-trait
  equality tester registered by `addEqualityTesters` and the Effect runner share one import site.
- `API` is `vitest`'s `TestAPI<{}>` with `scoped` remapped to `unknown` and a `scopedFixtures`
  alias added; the Effect `scoped` runner shadows Vitest's fixture `scoped`, so reach Vitest
  scoped-fixtures through `scopedFixtures` when both are needed.

## [04]-[IMPLEMENTATION_LAW]

[RUNNER_TOPOLOGY]:
- `it` is the canonical entry (`Vitest.Methods`); the free `effect`/`scoped`/`live`/`scopedLive`/
  `prop` consts are the same testers hoisted to module scope. Prefer `it.effect`/`it.layer`/
  `it.prop` so the whole method tree (modifiers + nested `layer`) stays reachable from one binding.
- `effect`/`scoped` install `TestServices.TestServices` (test clock, deterministic console,
  `TestRandom`); `live`/`scopedLive` run on the real clock with no test services. Use `live`/
  `scopedLive` only for genuinely wall-clock-dependent behavior — default to `effect`/`scoped`.
- `scoped`/`scopedLive` add a per-test `Scope.Scope`, finalized after the test fiber completes;
  use them for any test acquiring a scoped resource (`Effect.acquireRelease`, `Scope`-requiring
  layers run inline).
- Tester modifiers (`skip`/`skipIf`/`runIf`/`only`/`fails`/`each`) mirror Vitest semantics over the
  Effect runner; `fails` asserts the Effect test fails, `each(cases)` is the typed table-driven
  form whose `self` receives `Array<T>` plus the `TestContext`.

[LAYER_HARNESS_LAW]:
- `it.layer(layer)(...)` / `layer(layer)(...)` builds the `Layer` once and shares it across every
  test in the block (memoized via `Layer.MemoMap`); the callback receives `MethodsNonLive` — no
  `live`/`scopedLive`, because a shared memoized layer is incompatible with per-test real-clock
  fibers. Nest `it.layer` inside the callback to compose additional context (`R | R2`).
- `excludeTestServices: true` removes `TestServices.TestServices` from the inner testers'
  requirement channel — use when the shared layer itself supplies the clock/console/services and the
  default test services would otherwise be layered redundantly.
- `options.memoMap` shares one `Layer.MemoMap` across sibling `layer` blocks (cross-block layer
  reuse); `options.timeout` bounds layer build time.
- `describeWrapped(name, f)` is `layer` without a layer — a `describe` block whose callback gets the
  full `Vitest.Methods` tree (including `live`). Use it to group Effect tests under a named suite
  when no shared layer is required.

[PROPERTY_LAW]:
- `it.prop` / `prop` drive fast-check (`effect/FastCheck`) from a `Vitest.Arbitraries` source — an
  array yields positional `self` arguments, a record yields a named-property object. Each entry is a
  raw `FC.Arbitrary<T>` or a `Schema.Schema` (decoded to its `Type` via `Schema.Schema.Type`).
- The standalone `prop` (`Vitest.Methods["prop"]`) takes a synchronous `self` (`(properties, ctx) =>
  void`); the `Tester.prop` form takes an Effect-returning `TestFunction` and runs under the tester's
  requirement `R`. Use `it.effect.prop`-style access (tester `prop`) for property tests over Effects.
- fast-check run parameters (runs, seed, etc.) ride the trailing options object as
  `{ fastCheck?: FC.Parameters<...> }` keyed to the decoded arbitrary value type.

[ASSERTION_LAW]:
- The `@effect/vitest/utils` family is `Equal.equals`-trait-aware: `deepStrictEqual`/`assertEquals`/
  `notDeepStrictEqual` compare via the Effect `Equal` trait, so structurally-equal Effect values
  (`Data`, `Chunk`, `Option`, branded records) compare correctly where Vitest's native
  `toEqual`/`toStrictEqual` would not. Call `addEqualityTesters()` once in test setup to extend
  Vitest's own `expect` matchers with the same trait.
- The `assertNone`/`assertSome`/`assertLeft`/`assertRight`/`assertFailure`/`assertSuccess` family are
  TypeScript assertion functions: they narrow the argument type (`asserts x is …`) on success, so
  post-assertion code accesses `Some.value` / `Right.right` / `Success` payloads without a second
  guard. Prefer these over `expect(Option.isSome(x)).toBe(true)` chains.
- The trailing `..._: Array<never>` parameter rejects extra positional arguments at compile time;
  do not pass beyond the documented arity.

[RAIL_LAW]:
- `@effect/vitest`: testing rail; `neutral` tier. It is a dev/test-time dependency only — never
  imported by shipped library or app source. The `effect` and `vitest` peers are its sole runtime
  surface; no `node:*` import means a test file may target either the node or browser Vitest
  environment, governed by the Vitest project config, not by this package.
- `expect`/`describe` and the Effect runner must come from the same `@effect/vitest` import so the
  trait-equality tester and the runner share one module instance.
