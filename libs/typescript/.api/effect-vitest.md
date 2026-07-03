# [TS_BRANCH_API_EFFECT_VITEST]

`@effect/vitest` is the dev-plane binding between Vitest and the `Effect` runtime: it makes an `Effect`-returning test body a first-class Vitest test, runs it under deterministic `TestServices` (`TestClock` for virtual time, `TestRandom` for seeded randomness), shares an `Effect` `Layer` across a test block without a hand-rolled harness (the standalone `layer(SharedLayer)` opener, nested via `it.layer`), derives property tests from `Schema`/`FastCheck` arbitraries (`it.prop`), retries a flaky effect to a deadline (`it.flakyTest`), and installs `Equal`-aware equality testers so `expect(...).toEqual(...)` compares Effect data structurally. It re-exports the full `vitest` surface (`expect`, `describe`, `vi`, the lifecycle hooks) and ships a `./utils` subpath of Effect-aware assertions (`assertSome`, `assertLeft`, `assertSuccess`, `assertFailure`). It is the whole reason `proof` needs no bespoke test wrapper: layer-sharing, deterministic clocks, and Schema-driven generators are package capability, and specs live beside the folder they prove.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@effect/vitest`
- package: `@effect/vitest` (0.29.0, MIT, © Effectful Technologies)
- module format: ESM + CJS dual (`dist/esm` + `dist/cjs`, types `dist/dts`), `sideEffects: []`; exports `.` (the binding + `export * from "vitest"`) and `./utils` (Effect-aware assertions)
- runtime target: dev/test only — a `devDependency`; `proof/gauge/purity.ts` asserts it never leaks into a runtime subpath or shipped bundle
- peer: `vitest@^3.2.0` (both hard; `peerDependenciesMeta` is null), `effect@^3.21.0`; no runtime dependencies of its own. The admitted runner is `vitest@4.1.9` — one major ahead of the declared range; pnpm resolves the binding against it and the collector API (`test`/`expect`/`TestContext`) is stable across v3→v4, so the binding runs unmodified until a newer `@effect/vitest` widens the peer to `vitest@^4`
- asset: pure-TypeScript test binding; the collectors run inside the Vitest worker
- rail: proof (the dev plane; specs co-located with the folder they prove). This branch tier is the canonical owner of the generic runner surface; the folder overlays (`_tmp/services/.api/effect-vitest.md`, `_tmp/projection/.api/effect-vitest.md`) compose folder-scoped stacking onto it without re-documenting the generic contract

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the test-method families
- rail: proof

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY]  | [CONSUMER]                                                              |
| :-----: | :-------------------------------- | :------------- | :--------------------------------------------------------------------- |
|  [01]   | `Vitest.Methods<R>`               | method set      | the `it` object — `effect`/`scoped`/`live`/`scopedLive`/`layer`/`prop`/`flakyTest` |
|  [02]   | `Vitest.MethodsNonLive<R, X>`     | scoped method set | the `it` handed to an `it.layer` block — no `live`/`scopedLive`, carries the block's `R` |
|  [03]   | `Vitest.Tester<R>`                | test collector  | a callable test (`it.effect`) with `.skip`/`.skipIf(c)`/`.runIf(c)`/`.only`/`.each`/`.fails`/`.prop` modifiers |
|  [04]   | `Vitest.TestFunction<A, E, R, Args>` | test body    | `(...args) => Effect.Effect<A, E, R>` — the effectful body a Tester runs |
|  [05]   | `Vitest.Arbitraries`              | generator spec  | `Array`/record of `Schema.Schema.Any \| FC.Arbitrary<any>` for `it.prop` |

[PUBLIC_TYPE_SCOPE]: the `Vitest` runner contract — full signatures (canonical owner; the folder overlays reference this block)
- rail: proof
- The exhaustive `Vitest` namespace and root API the tables above index. `Vitest.Methods` is the type of `it`; `Vitest.MethodsNonLive` is what `layer(...)` hands its callback (no `live`/`scopedLive` — a shared memoized `Layer` is incompatible with per-test real-clock fibers). `ExcludeTestServices extends true` drops the `TestServices` requirement when the shared layer already supplies real services.

```ts contract
import type * as Duration from "effect/Duration"
import type * as Effect from "effect/Effect"
import type * as FC from "effect/FastCheck"
import type * as Layer from "effect/Layer"
import type * as Schema from "effect/Schema"
import type * as Scope from "effect/Scope"
import type * as TestServices from "effect/TestServices"
import type * as V from "vitest"

// --- root API (vitest TestAPI overlay; the Effect `scoped` runner shadows Vitest's fixture `scoped`, reached via `scopedFixtures`) ---
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
  concurrent?: boolean; sequential?: boolean; only?: boolean; skip?: boolean; todo?: boolean
  fails?: boolean; timeout?: number; retry?: number; repeats?: number
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

// --- runner/harness values ---
const addEqualityTesters: () => void
const effect: Vitest.Tester<TestServices.TestServices>
const scoped: Vitest.Tester<TestServices.TestServices | Scope.Scope>
const live: Vitest.Tester<never>
const scopedLive: Vitest.Tester<Scope.Scope>
const layer: <R, E, const ExcludeTestServices extends boolean = false>(
  layer_: Layer.Layer<R, E>,
  options?: { readonly memoMap?: Layer.MemoMap; readonly timeout?: Duration.DurationInput; readonly excludeTestServices?: ExcludeTestServices }
) => {
  (f: (it: Vitest.MethodsNonLive<R, ExcludeTestServices>) => void): void
  (name: string, f: (it: Vitest.MethodsNonLive<R, ExcludeTestServices>) => void): void
}
const flakyTest: <A, E, R>(self: Effect.Effect<A, E, R>, timeout?: Duration.DurationInput) => Effect.Effect<A, never, R>
const prop: Vitest.Methods["prop"]
const it: Vitest.Methods
const makeMethods: (it: API) => Vitest.Methods
const describeWrapped: (name: string, f: (it: Vitest.Methods) => void) => V.SuiteCollector
```

[PUBLIC_TYPE_SCOPE]: `@effect/vitest/utils` — Effect-data assertion signatures (canonical owner)
- rail: proof / assertion
- Every two-value assertion takes a trailing `..._: Array<never>` rest — a compile-time guard forbidding extra positional arguments. `assertNone`/`assertSome`/`assertLeft`/`assertRight`/`assertFailure`/`assertSuccess` are TypeScript assertion functions (`asserts x is …`), so post-assertion code reaches `Some.value`/`Right.right`/`Success` payloads without a second guard; `deepStrictEqual`/`assertEquals` compare via the `Equal.equals` trait.

```ts contract
import type * as Cause from "effect/Cause"
import type * as Either from "effect/Either"
import type * as Exit from "effect/Exit"
import type * as Option from "effect/Option"

function fail(message: string): void
function deepStrictEqual<A>(actual: A, expected: A, message?: string, ..._: Array<never>): void   // via Equal.equals trait
function notDeepStrictEqual<A>(actual: A, expected: A, message?: string, ..._: Array<never>): void
function strictEqual<A>(actual: A, expected: A, message?: string, ..._: Array<never>): void
function assertEquals<A>(actual: A, expected: A, message?: string, ..._: Array<never>): void
function doesNotThrow(thunk: () => void, message?: string, ..._: Array<never>): void
function throws(thunk: () => void, error?: Error | ((u: unknown) => undefined), ..._: Array<never>): void
function throwsAsync(thunk: () => Promise<void>, error?: Error | ((u: unknown) => undefined), ..._: Array<never>): Promise<void>
function assertInstanceOf<C extends abstract new (...args: any) => any>(value: unknown, constructor: C, message?: string, ..._: Array<never>): asserts value is InstanceType<C>
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

[ENTRYPOINT_SCOPE]: effect test collectors and their service context
- rail: proof

| [INDEX] | [SURFACE]                                                              | [ENTRY_FAMILY]  | [CONSUMER]                                                   |
| :-----: | :--------------------------------------------------------------------- | :-------------- | :---------------------------------------------------------- |
|  [01]   | `it.effect(name, (ctx) => Effect<A, E, TestServices>, timeout?)`       | deterministic   | every folder spec — runs under `TestClock`/`TestRandom`; virtual, seeded, fast |
|  [02]   | `it.scoped(name, (ctx) => Effect<A, E, TestServices \| Scope>, …)`     | scoped          | `store`/`work`/`host` resource specs — a `Scope` is opened and closed per test |
|  [03]   | `it.live(name, …)` / `it.scopedLive(name, …)`                          | real services   | specs needing wall-clock time, real randomness, or real I/O timing |
|  [04]   | `it.effect.each(cases)(name, (…caseArgs) => Effect, …)`                | table           | data-driven law rows — the case row spreads into the effect body (`TestFunction<…, Array<T>>`), no appended `ctx` |
|  [05]   | `it.effect.skip` / `.only` / `.fails` / `.skipIf(cond)` / `.runIf(cond)` | modifier      | conditional/expected-failure specs; `.fails` asserts the effect fails |
|  [06]   | Effect-data assertions via `./utils` (full family in the `./utils` sub-block below): `assertSome`/`assertNone`/`assertRight`/`assertLeft`/`assertSuccess`/`assertFailure` + `Equal`-scalar/guard/throw asserts | assert | `proof` — structural asserts over `Option`/`Either`/`Exit`, never raw `expect` or `._tag` |

[ENTRYPOINT_SCOPE]: layer-sharing, property testing, and flake control
- rail: proof

| [INDEX] | [SURFACE]                                                                                   | [ENTRY_FAMILY]  | [CONSUMER]                                                   |
| :-----: | :------------------------------------------------------------------------------------------ | :-------------- | :---------------------------------------------------------- |
|  [01]   | `layer(rootLayer, { memoMap?, timeout?, excludeTestServices? })(name?, (it) => …)`          | share layer (opener) | `proof/harness` — the STANDALONE combinator: build a `Layer<R, E>` (container, pglite, `HttpServer`) once per block; consuming catalogs bind it as `layer(SharedLayer)(…)` |
|  [02]   | `it.layer(childLayer, { timeout? })(name?, (it) => …)`                                       | share layer (nest)   | inside an open block only: extend the context with `Layer<R2, E, R>` (may require the block's `R`); inherits `R` + `ExcludeTestServices`, takes `timeout?` alone — no `memoMap`/`excludeTestServices` |
|  [03]   | `it.prop(name, arbitraries, (values, ctx) => …, { fastCheck: { numRuns, seed } })`          | property        | `proof/law` — Schema/`FC.Arbitrary` generators; sync predicate body (`MethodsNonLive.prop`); `fastCheck` params tune runs and seed |
|  [04]   | `it.effect.prop(name, arbitraries, ({ ...values }, ctx) => Effect, …)`                       | effect property | any `Tester` carries `.prop`; here the case body is an `Effect` under `TestServices` (`Tester.prop`) |
|  [05]   | `it.flakyTest(effect, timeout?)` / top-level `flakyTest(effect, timeout?)`                   | flake retry     | wrap a nondeterministic effect to retry until success or the `Duration` deadline; the typed `E` collapses to `never` on the result |
|  [06]   | `addEqualityTesters()`                                                                       | equality setup  | one call so `expect(a).toEqual(b)` uses `Equal.equals` on Effect data (Chunk, Option, Data) |
|  [07]   | top-level mirrors: `effect` / `scoped` / `live` / `scopedLive` / `prop` / `describeWrapped(name, f)` / `makeMethods(api)` | top-level | the same collectors as `it.*` when imported standalone; `describeWrapped` names an outer `describe`; `makeMethods` rebuilds `it` over a custom base `API` |
|  [08]   | re-exports from `vitest`: `expect` / `describe` / `vi` / `beforeEach` / `afterAll`          | vitest surface   | lifecycle, mocking, and the assertion entry — one import for the whole spec |

[ENTRYPOINT_SCOPE]: Effect-data assertions — the `./utils` subpath
- rail: proof

| [INDEX] | [SURFACE]                                                                                   | [ASSERT_FAMILY]  | [CONSUMER]                                                   |
| :-----: | :------------------------------------------------------------------------------------------ | :--------------- | :---------------------------------------------------------- |
|  [01]   | `assertSome(o, a)` / `assertNone(o)`                                                         | `Option`         | narrow `Option<A>` to `Some`/`None` as a type guard, never an `._tag` read |
|  [02]   | `assertRight(e, r)` / `assertLeft(e, l)`                                                     | `Either`         | narrow `Either<R, L>` to `Right`/`Left` with the expected value |
|  [03]   | `assertSuccess(exit, a)` / `assertFailure(exit, cause)`                                      | `Exit`           | narrow a folded `Exit<A, E>` to its success value or failure `Cause` |
|  [04]   | `strictEqual` / `deepStrictEqual` / `notDeepStrictEqual` / `assertEquals` `(actual, expected, msg?)` | `Equal` scalar | structural equality via the `Equal` trait — Chunk/Data/Option compare by value, not reference |
|  [05]   | `assertTrue(x)` / `assertFalse(x)` / `assertInstanceOf(x, Ctor)`                             | guard            | boolean + instance guards that also narrow the static type |
|  [06]   | `assertInclude(s, sub)` / `assertMatch(s, re)` / `throws(thunk, err?)` / `throwsAsync(thunk, err?)` / `doesNotThrow(thunk)` / `fail(msg)` | string / throw | substring/regex membership, throw-shape assertions, and an explicit `fail` |

## [04]-[IMPLEMENTATION_LAW]

[VITEST_TOPOLOGY]:
- `it.effect` runs the body as an `Effect` inside the Vitest test, folds its `Exit`, and fails the test on a failure `Cause` — the typed error channel and defects both surface, so a spec asserts on the value, never on a thrown exception. The default service context is `TestServices`: `TestClock` makes time virtual (advance it explicitly with `TestClock.adjust`, so a `Duration`-bounded effect resolves instantly and deterministically) and `TestRandom` makes randomness seeded and reproducible.
- The collector family discriminates by service context, not by name: `it.effect` (TestServices), `it.scoped` (TestServices + `Scope`, closed per test), `it.live` (real services — real clock/random), `it.scopedLive` (real + `Scope`). `excludeTestServices: true` is an option on the STANDALONE `layer(rootLayer, { excludeTestServices: true })` opener — not the instance `it.layer` — dropping the TestServices requirement for a block that supplies its own real clock; every nested `it.layer` inherits the flag as a type parameter.
- The standalone `layer(rootLayer, options?)` is the block OPENER and the harness: it builds the `Layer` once (memoized across sibling blocks via an optional shared `memoMap`), provides it to every `it.effect`/`it.scoped` in the block, and tears it down after. The instance `it.layer(childLayer, { timeout? })` NESTS inside an already-open block — its child `Layer<R2, E, R>` may require the parent's provided `R`, extending the outer context. This is why `proof` ships no wrapper and why every consuming catalog binds the opener as `layer(SharedLayer)(…)`: a testcontainers Postgres, a pglite instance, or an `HttpServer.layerTestClient` is a `Layer`, and layer-sharing is package capability.
- `it.prop`/`it.effect.prop` derive generators from the `Arbitraries` spec — an array or record whose entries are `Schema.Schema.Any` or `FC.Arbitrary<any>` (`effect/FastCheck`). A `Schema` entry is turned into an arbitrary via `Arbitrary.make`, so `proof/law/arbitrary.ts` declares one `Schema`-driven generator per kernel brand and every property law consumes it; the `fastCheck` option passes `FC.Parameters` (run count, seed, `endOnFailure`).
- `it.flakyTest(effect, timeout)` retries a nondeterministic effect until it succeeds or the `Duration` elapses — the sanctioned tool for an inherently timing-dependent assertion, never a bare `setTimeout` or a re-run loop.
- `addEqualityTesters()` registers `Equal.equals` as Vitest's deep-equality comparator for Effect data, so `expect(chunkA).toEqual(chunkB)` and `expect(optionA).toEqual(optionB)` compare by structural `Equal`/`Hash` rather than reference — called once in the proof setup.

[STACKS_WITH]:
- `effect` (`.api/effect.md`): the test body is an `Effect`; `TestClock`/`TestRandom` come from `effect/TestServices`; `it.scoped` provides a `Scope`; `it.prop` arbitraries derive from `Schema` via `Arbitrary.make`; `addEqualityTesters` uses `Equal.equals`. `@effect/vitest` is `effect` projected into the Vitest worker.
- `@effect/platform` + `@effect/platform-node` (`.api/effect-platform.md`, `.api/effect-platform-node.md`): the standalone `layer(NodeContext.layer)(…)` opener runs an integration spec against the real filesystem/command bindings; `layer(HttpServer.layerTestClient)(…)` serves a declarative `HttpApi` in-process and exercises it with the derived `HttpApiClient` — the same Tags production uses, bound to test Layers; a per-suite child extends the block via `it.layer`.
- `fast-check` (catalogued at `libs/typescript/proof/.api/`): `it.prop` accepts raw `FC.Arbitrary`s beside `Schema`s, and `proof/law/property.ts` composes reusable law combinators (fold identity, merge commutativity, upcast totality) over them; the `fastCheck` option forwards `FC.Parameters` for shrink and seed control.
- `testcontainers` + `@electric-sql/pglite` (catalogued at `libs/typescript/proof/.api/`): each is wrapped as an Effect `Layer` (a scoped container / an in-memory Postgres) and shared through the standalone `layer(containerLayer, { timeout })("suite", (it) => …)` opener across a `describe` block — `proof/harness/container.ts` and `harness/unit.ts` own these Layers, binding the combinator exactly as `testcontainers.md` / `electric-sql-pglite.md` [04] document (`layer(PgContainer)` / `layer(PgLiteTest)`).
- `vitest` + `@vitest/coverage-v8` + `@stryker-mutator/*` (catalogued at `libs/typescript/proof/.api/`): `@effect/vitest` re-exports the `vitest` surface (`expect`/`describe`/`vi`/lifecycle); coverage and mutation gauges (`proof/gauge/mutation.ts`) run the same specs under coverage-v8 and Stryker thresholds expressed as data.

[LOCAL_ADMISSION]:
- Use `it.effect` for every spec whose body is an `Effect`; never `it(async () => { await Effect.runPromise(...) })` — that loses `TestServices`, the typed `Exit` fold, and virtual time.
- Use `it.scoped` when the effect acquires a resource; never leak a `Scope` by running an acquiring effect under plain `it.effect`.
- Use the standalone `layer(SharedLayer)(…)` opener to share a container/server/Layer across a block (nest a block-local extension with `it.layer(childLayer)`); never construct a service in a `beforeAll` and thread it by hand — that is the wrapper the package exists to delete.
- Use `it.prop` with `Schema`-derived arbitraries for law-style tests; never hand-write example arrays where a `Schema` already generates the domain (`proof/law/arbitrary.ts` owns the generators).
- Use `TestClock.adjust` to drive time in an `it.effect`; never `it.live` with a real `sleep` unless the assertion genuinely depends on wall-clock timing.
- Use the `./utils` `assertSome`/`assertRight`/`assertSuccess`/`assertFailure` for Effect-data assertions and call `addEqualityTesters()` once; never unwrap an `Option`/`Either`/`Exit` with `._tag` checks inside a spec.

[RAIL_LAW]:
- Package: `@effect/vitest`
- Owns: the `Effect`-aware Vitest binding — `it.effect`/`it.scoped`/`it.live`/`it.scopedLive` collectors under `TestServices`, the `.skip`/`.skipIf`/`.runIf`/`.only`/`.each`/`.fails` modifiers, the standalone `layer(rootLayer, { memoMap?, timeout?, excludeTestServices? })` block opener plus the nestable instance `it.layer(childLayer, { timeout? })`, `it.prop`/`it.effect.prop` Schema/`FastCheck` property testing, `it.flakyTest`, `addEqualityTesters`, `describeWrapped`/`makeMethods`, the `vitest` re-export, and the full `./utils` Effect-data assertion family (`assertSome`/`assertNone`/`assertLeft`/`assertRight`/`assertSuccess`/`assertFailure` + `strictEqual`/`deepStrictEqual`/`assertEquals` + `assertTrue`/`assertFalse`/`assertInstanceOf`)
- Accept: `it.effect` with a deterministic `TestServices` body, `it.scoped` for resources, the standalone `layer(SharedLayer)(…)` opener for a shared container/server/pglite Layer plus nested `it.layer` for block-local extension, `it.prop` over `Schema`-derived arbitraries with `fastCheck` params, `it.flakyTest` for timing-dependent assertions, `./utils` assertions + `addEqualityTesters`
- Reject: `Effect.runPromise` inside a plain `it`, hand-threaded services in `beforeAll` where `it.layer` fits, example arrays where a `Schema` generates the domain, real `sleep` under `it.live` where `TestClock.adjust` is deterministic, `._tag` unwrapping in place of the `./utils` assertions
