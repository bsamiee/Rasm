# [API_CATALOGUE] @effect-atom/atom

Dependency catalogue for `@effect-atom/atom` (0.5.3) — the framework-agnostic reactive-atom state engine
that bridges `Effect` computations, `Stream` folds, `Layer`/`Runtime` roots, and `SubscriptionRef`/`Subscribable`
sources into subscribable cells. Grounded from installed `node_modules` `.d.ts` declarations;
exact spellings. The React binding `@effect-atom/atom-react` (0.5.0) is a separate package carried in `effect-atom-atom-react.md`
and is not reflected here. The package re-exports seven modules as namespaces through `index.d.ts`; each section below is
one module. The core cross-package owners (`Effect`, `Layer`, `Runtime`, `Stream`, `Scope`, `Schema`, `Option`, `Exit`,
`Cause`, `Context`, `Duration`, `SubscriptionRef`, `Subscribable`, `KeyValueStore`, `Reactivity`) resolve against
`effect.md`; the `@effect/rpc` `Rpc`/`RpcClient`/`RpcGroup` and `@effect/platform` `HttpApi*` families the `AtomRpc`/
`AtomHttpApi` namespaces re-expose are atom's transitive surface, off the `platform` consumption path.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@effect-atom/atom`
- package: `@effect-atom/atom`
- entry: `@effect-atom/atom` (namespace barrel) plus per-module deep paths `@effect-atom/atom/Atom`, `@effect-atom/atom/AtomHttpApi`, `@effect-atom/atom/AtomRef`, `@effect-atom/atom/AtomRpc`, `@effect-atom/atom/Hydration`, `@effect-atom/atom/Registry`, `@effect-atom/atom/Result`
- asset: reactive cell model + constructors/combinators (`Atom`), three-state computation result (`Result`), node store + service tag/layers (`Registry`), plain mutable ref / collection (`AtomRef`), RPC-backed client tag (`AtomRpc`), HTTP-API-backed client tag (`AtomHttpApi`), SSR dehydrate/hydrate (`Hydration`)
- tier: `neutral` — framework-agnostic; depends only on `effect`, `@effect/experimental` `Reactivity`, `@effect/platform` `KeyValueStore`/`HttpApi*`, `@effect/rpc`
- rail: state-cell
- peer: `effect`, `@effect/experimental`, `@effect/platform`, `@effect/rpc`

The `index.d.ts` barrel is the namespace-aggregating entry:

```ts contract
export * as Atom from "./Atom.js"
export * as AtomHttpApi from "./AtomHttpApi.js"
export * as AtomRef from "./AtomRef.js"
export * as AtomRpc from "./AtomRpc.js"
export * as Hydration from "./Hydration.js"
export * as Registry from "./Registry.js"
export * as Result from "./Result.js"
```

## [02]-[ATOM]

`@effect-atom/atom/Atom` — the reactive cell. `Atom<A>` is a read node carrying `keepAlive`/`lazy`/`idleTTL`
metadata and a `read: (get: Context) => A`; `Writable<R, W>` extends it with a `write`. The module owns every
cell constructor (`readable`/`writable`/`make`/`fn`/`fnSync`/`pull`/`subscriptionRef`/`subscribable`/`kvs`/`searchParam`/`family`),
the combinator family that decorates an atom in place, the `Context`/`WriteContext`/`FnContext` read environments,
the `AtomRuntime`/`RuntimeFactory` layer-scoped sub-atom builders, the `Reset`/`Interrupt` write symbols, the
`AtomResultFn`/`PullResult` result-function shapes, the optimistic-update pair, and the registry-bound conversions.

[PUBLIC_TYPE_SCOPE]: cell and runtime model
- rail: state-cell

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [CAPABILITY]         |
| :-----: | :------------------- | :------------ | :------------------- |
|  [01]   | `Atom<A>`            | interface     | read cell            |
|  [02]   | `Writable<R, W=R>`   | interface     | writable cell        |
|  [03]   | `AtomRuntime<R, ER>` | interface     | layer-backed cells   |
|  [04]   | `RuntimeFactory`     | interface     | runtime construction |

[PUBLIC_TYPE_SCOPE]: read and write environments
- rail: state-cell

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY] | [CAPABILITY]      |
| :-----: | :---------------- | :------------ | :---------------- |
|  [01]   | `Context`         | interface     | read environment  |
|  [02]   | `WriteContext<A>` | interface     | write environment |
|  [03]   | `FnContext`       | interface     | result-fn context |

[PUBLIC_TYPE_SCOPE]: projections, brands, guards
- rail: state-cell

| [INDEX] | [SYMBOL]                                                   | [TYPE_FAMILY]      | [CAPABILITY]             |
| :-----: | :--------------------------------------------------------- | :----------------- | :----------------------- |
|  [01]   | `AtomResultFn<Arg,A,E>`                                    | interface          | result function atom     |
|  [02]   | `PullResult<A,E>`                                          | type alias         | stream pull result       |
|  [03]   | `Type<T>` / `Success<T>` / `PullSuccess<T>` / `Failure<T>` | type projection    | atom type extraction     |
|  [04]   | `WithoutSerializable<T>`                                   | type projection    | serializable drop        |
|  [05]   | `Serializable<S>`                                          | interface          | serialization annotation |
|  [06]   | `Reset` / `Interrupt`                                      | unique symbol      | result-fn write signals  |
|  [07]   | `SerializableTypeId` / `ServerValueTypeId`                 | exported brand     | public brand ids         |
|  [08]   | `isAtom` / `isWritable` / `isSerializable`                 | guard / refinement | erased-cell predicates   |

```ts contract
// @effect-atom/atom/Atom — model + brands
declare const TypeId: TypeId            // internal
type TypeId = "~effect-atom/atom/Atom"  // internal
type WritableTypeId = "~effect-atom/atom/Atom/Writable" // internal
type SerializableTypeId = "~effect-atom/atom/Atom/Serializable"

interface Atom<A> extends Pipeable, Inspectable.Inspectable {
  readonly [TypeId]: TypeId
  readonly keepAlive: boolean
  readonly lazy: boolean
  readonly read: (get: Context) => A
  readonly refresh?: (f: <A>(atom: Atom<A>) => void) => void
  readonly label?: readonly [name: string, stack: string]
  readonly idleTTL?: number
}
interface Writable<R, W = R> extends Atom<R> {
  readonly [WritableTypeId]: WritableTypeId
  readonly write: (ctx: WriteContext<R>, value: W) => void
}
type Type<T extends Atom<any>> = T extends Atom<infer A> ? A : never
type Success<T extends Atom<any>> = T extends Atom<Result.Result<infer A, infer _>> ? A : never
type PullSuccess<T extends Atom<any>> = T extends Atom<PullResult<infer A, infer _>> ? A : never
type Failure<T extends Atom<any>> = T extends Atom<Result.Result<infer _, infer E>> ? E : never
type WithoutSerializable<T extends Atom<any>> = T extends Writable<infer R, infer W> ? Writable<R, W> : Atom<Type<T>>

const isAtom: (u: unknown) => u is Atom<any>
const isWritable: <R, W>(atom: Atom<R>) => atom is Writable<R, W>

// read environments
interface Context {
  <A>(atom: Atom<A>): A
  get<A>(this: Context, atom: Atom<A>): A
  result<A, E>(this: Context, atom: Atom<Result.Result<A, E>>, options?: { readonly suspendOnWaiting?: boolean | undefined }): Effect.Effect<A, E>
  resultOnce<A, E>(this: Context, atom: Atom<Result.Result<A, E>>, options?: { readonly suspendOnWaiting?: boolean | undefined }): Effect.Effect<A, E>
  once<A>(this: Context, atom: Atom<A>): A
  addFinalizer(this: Context, f: () => void): void
  mount<A>(this: Context, atom: Atom<A>): void
  refresh<A>(this: Context, atom: Atom<A>): void
  refreshSelf(this: Context): void
  self<A>(this: Context): Option.Option<A>
  setSelf<A>(this: Context, a: A): void
  set<R, W>(this: Context, atom: Writable<R, W>, value: W): void
  setResult<A, E, W>(this: Context, atom: Writable<Result.Result<A, E>, W>, value: W): Effect.Effect<A, E>
  some<A>(this: Context, atom: Atom<Option.Option<A>>): Effect.Effect<A>
  someOnce<A>(this: Context, atom: Atom<Option.Option<A>>): Effect.Effect<A>
  stream<A>(this: Context, atom: Atom<A>, options?: { readonly withoutInitialValue?: boolean; readonly bufferSize?: number }): Stream.Stream<A>
  streamResult<A, E>(this: Context, atom: Atom<Result.Result<A, E>>, options?: { readonly withoutInitialValue?: boolean; readonly bufferSize?: number }): Stream.Stream<A, E>
  subscribe<A>(this: Context, atom: Atom<A>, f: (_: A) => void, options?: { readonly immediate?: boolean }): void
  readonly registry: Registry.Registry
}
interface WriteContext<A> {
  get<T>(this: WriteContext<A>, atom: Atom<T>): T
  refreshSelf(this: WriteContext<A>): void
  setSelf(this: WriteContext<A>, a: A): void
  set<R, W>(this: WriteContext<A>, atom: Writable<R, W>, value: W): void
}
// FnContext mirrors Context minus `get` (named)/`once`/`someOnce`/`resultOnce`/`refreshSelf`; keeps `self`/`setSelf`; same registry/stream/subscribe surface.
interface FnContext { /* result/addFinalizer/mount/refresh/self/setSelf/set/setResult/some/stream/streamResult/subscribe + registry */ }
```

[PUBLIC_TYPE_SCOPE]: constructors
- rail: state-cell

| [INDEX] | [SYMBOL]          | [RESULT]                                                                          |
| :-----: | :---------------- | :-------------------------------------------------------------------------------- |
|  [01]   | `readable`        | `Atom<A>` from `read` fn                                                          |
|  [02]   | `writable`        | `Writable<R, W>` from `read`+`write` fns                                          |
|  [03]   | `make`            | overloaded: Effect/Stream → `Atom<Result>`, fn → `Atom<A>`, value → `Writable<A>` |
|  [04]   | `fn`              | Effect/Stream result-function builder → `AtomResultFn<Arg, A, E>`                 |
|  [05]   | `fnSync`          | sync result-function builder → `Writable<Option<A> \| A, Arg>`                    |
|  [06]   | `pull`            | accumulating stream → `Writable<PullResult<A, E>, void>`                          |
|  [07]   | `subscriptionRef` | `SubscriptionRef`/effect → `Writable`                                             |
|  [08]   | `subscribable`    | `Subscribable`/effect → `Atom`                                                    |
|  [09]   | `kvs`             | `KeyValueStore`-backed `Writable<A>` (schema + default)                           |
|  [10]   | `searchParam`     | URL search param → `Writable<string \| Option<A>>`                                |
|  [11]   | `family`          | memoize `(arg) => T` cell builder                                                 |
|  [12]   | `context`         | `({ memoMap }) => RuntimeFactory`                                                 |
|  [13]   | `runtime`         | default `RuntimeFactory`                                                          |
|  [14]   | `defaultMemoMap`  | `Layer.MemoMap`                                                                   |

```ts contract
// @effect-atom/atom/Atom — constructors
const readable: <A>(read: (get: Context) => A, refresh?: (f: <A_1>(atom: Atom<A_1>) => void) => void) => Atom<A>
const writable: <R, W>(read: (get: Context) => R, write: (ctx: WriteContext<R>, value: W) => void, refresh?: (f: <A>(atom: Atom<A>) => void) => void) => Writable<R, W>

const make: {
  <A, E>(create: (get: Context) => Effect.Effect<A, E, Scope.Scope | AtomRegistry>, options?: { readonly initialValue?: A }): Atom<Result.Result<A, E>>
  <A, E>(effect: Effect.Effect<A, E, Scope.Scope | AtomRegistry>, options?: { readonly initialValue?: A }): Atom<Result.Result<A, E>>
  <A, E>(create: (get: Context) => Stream.Stream<A, E, AtomRegistry>, options?: { readonly initialValue?: A }): Atom<Result.Result<A, E>>
  <A, E>(stream: Stream.Stream<A, E, AtomRegistry>, options?: { readonly initialValue?: A }): Atom<Result.Result<A, E>>
  <A>(create: (get: Context) => A): Atom<A>
  <A>(initialValue: A): Writable<A>
}

const fn: {
  <Arg>(): <E, A>(fn: (arg: Arg, get: FnContext) => Effect.Effect<A, E, Scope.Scope | AtomRegistry>, options?: { readonly initialValue?: A | undefined; readonly concurrent?: boolean | undefined }) => AtomResultFn<Arg, A, E>
  <E, A, Arg = void>(fn: (arg: Arg, get: FnContext) => Effect.Effect<A, E, Scope.Scope | AtomRegistry>, options?: { readonly initialValue?: A | undefined; readonly concurrent?: boolean | undefined }): AtomResultFn<Arg, A, E>
  <Arg>(): <E, A>(fn: (arg: Arg, get: FnContext) => Stream.Stream<A, E, AtomRegistry>, options?: { readonly initialValue?: A | undefined; readonly concurrent?: boolean | undefined }) => AtomResultFn<Arg, A, E | NoSuchElementException>
  <E, A, Arg = void>(fn: (arg: Arg, get: FnContext) => Stream.Stream<A, E, AtomRegistry>, options?: { readonly initialValue?: A | undefined; readonly concurrent?: boolean | undefined }): AtomResultFn<Arg, A, E | NoSuchElementException>
}
const fnSync: {
  <Arg>(): {
    <A>(f: (arg: Arg, get: FnContext) => A): Writable<Option.Option<A>, Arg>
    <A>(f: (arg: Arg, get: FnContext) => A, options: { readonly initialValue: A }): Writable<A, Arg>
  }
  <A, Arg = void>(f: (arg: Arg, get: FnContext) => A): Writable<Option.Option<A>, Arg>
  <A, Arg = void>(f: (arg: Arg, get: FnContext) => A, options: { readonly initialValue: A }): Writable<A, Arg>
}
const pull: <A, E>(create: ((get: Context) => Stream.Stream<A, E, AtomRegistry>) | Stream.Stream<A, E, AtomRegistry>, options?: { readonly disableAccumulation?: boolean | undefined }) => Writable<PullResult<A, E>, void>
const subscriptionRef: {
  <A>(ref: SubscriptionRef.SubscriptionRef<A> | ((get: Context) => SubscriptionRef.SubscriptionRef<A>)): Writable<A>
  <A, E>(effect: Effect.Effect<SubscriptionRef.SubscriptionRef<A>, E, Scope.Scope | AtomRegistry> | ((get: Context) => Effect.Effect<SubscriptionRef.SubscriptionRef<A>, E, Scope.Scope | AtomRegistry>)): Writable<Result.Result<A, E>, A>
}
const subscribable: {
  <A, E>(ref: Subscribable.Subscribable<A, E> | ((get: Context) => Subscribable.Subscribable<A, E>)): Atom<A>
  <A, E, E1>(effect: Effect.Effect<Subscribable.Subscribable<A, E1>, E, Scope.Scope | AtomRegistry> | ((get: Context) => Effect.Effect<Subscribable.Subscribable<A, E1>, E, Scope.Scope | AtomRegistry>)): Atom<Result.Result<A, E | E1>>
}
const kvs: <A>(options: { readonly runtime: AtomRuntime<KeyValueStore.KeyValueStore, any>; readonly key: string; readonly schema: Schema.Schema<A, any>; readonly defaultValue: LazyArg<A> }) => Writable<A>
const searchParam: <A = never, I extends string = never>(name: string, options?: { readonly schema?: Schema.Schema<A, I> }) => Writable<[A] extends [never] ? string : Option.Option<A>>
const family: <Arg, T extends object>(f: (arg: Arg) => T) => (arg: Arg) => T
const context: (options: { readonly memoMap: Layer.MemoMap }) => RuntimeFactory
const defaultMemoMap: Layer.MemoMap
const runtime: RuntimeFactory
```

[PUBLIC_TYPE_SCOPE]: combinators (decorate an atom in place; return the same atom kind or `WithoutSerializable`)
- rail: state-cell

| [INDEX] | [SYMBOL]                                                        | [SHAPE]                                                         |
| :-----: | :-------------------------------------------------------------- | :-------------------------------------------------------------- |
|  [01]   | `keepAlive` / `autoDispose`                                     | `<A extends Atom<any>>(self: A) => A` (autoDispose reverts)     |
|  [02]   | `setLazy`                                                       | dual `(lazy: boolean)` → same atom                              |
|  [03]   | `setIdleTTL`                                                    | dual `(duration: DurationInput)` → same atom                    |
|  [04]   | `withLabel`                                                     | dual `(name: string)` → same atom                               |
|  [05]   | `initialValue`                                                  | dual → `readonly [Atom<A>, A]`                                  |
|  [06]   | `map` / `transform`                                             | dual map over value / over `Context` → `Atom`/`Writable<B>`     |
|  [07]   | `mapResult`                                                     | dual map over `Result.Success` → `Atom`/`Writable<Result<B,…>>` |
|  [08]   | `withFallback`                                                  | dual; replace `Result` initial/failure with a fallback atom     |
|  [09]   | `debounce`                                                      | dual `(duration)` → `WithoutSerializable<A>`                    |
|  [10]   | `serializable`                                                  | dual; attach `Serializable<S>` (key + schema)                   |
|  [11]   | `withReactivity`                                                | `(keys) => <A>(atom) => A` (alias of `runtime.withReactivity`)  |
|  [12]   | `makeRefreshOnSignal` / `refreshOnWindowFocus`                  | refresh-on-signal → `WithoutSerializable<A>`                    |
|  [13]   | `withServerValue` / `withServerValueInitial` / `getServerValue` | SSR server-read override                                        |

```ts contract
// @effect-atom/atom/Atom — combinators (dual signatures shown data-first; data-last overload omitted)
const keepAlive: <A extends Atom<any>>(self: A) => A
const autoDispose: <A extends Atom<any>>(self: A) => A
const setLazy: <A extends Atom<any>>(self: A, lazy: boolean) => A
const setIdleTTL: <A extends Atom<any>>(self: A, duration: Duration.DurationInput) => A
const withLabel: <A extends Atom<any>>(self: A, name: string) => A
const initialValue: <A>(self: Atom<A>, initialValue: A) => readonly [Atom<A>, A]
const transform: <R extends Atom<any>, B>(self: R, f: (get: Context) => B) => [R] extends [Writable<infer _, infer RW>] ? Writable<B, RW> : Atom<B>
const map: <R extends Atom<any>, B>(self: R, f: (_: Type<R>) => B) => [R] extends [Writable<infer _, infer RW>] ? Writable<B, RW> : Atom<B>
const mapResult: <R extends Atom<Result.Result<any, any>>, B>(self: R, f: (_: Result.Result.Success<Type<R>>) => B) => [R] extends [Writable<infer _, infer RW>] ? Writable<Result.Result<B, Result.Result.Failure<Type<R>>>, RW> : Atom<Result.Result<B, Result.Result.Failure<Type<R>>>>
const withFallback: <R extends Atom<Result.Result<any, any>>, A2, E2>(self: R, fallback: Atom<Result.Result<A2, E2>>) => [R] extends [Writable<infer _, infer RW>] ? Writable<Result.Result<Result.Result.Success<Type<R>> | A2, Result.Result.Failure<Type<R>> | E2>, RW> : Atom<Result.Result<Result.Result.Success<Type<R>> | A2, Result.Result.Failure<Type<R>> | E2>>
const debounce: <A extends Atom<any>>(self: A, duration: Duration.DurationInput) => WithoutSerializable<A>
const serializable: <R extends Atom<any>, S extends Schema.Schema<Type<R>, any>>(self: R, options: { readonly key: string; readonly schema: S }) => R & Serializable<S>
const withReactivity: (keys: ReadonlyArray<unknown> | ReadonlyRecord<string, ReadonlyArray<unknown>>) => <A extends Atom<any>>(atom: A) => A
const makeRefreshOnSignal: <_>(signal: Atom<_>) => <A extends Atom<any>>(self: A) => WithoutSerializable<A>
const refreshOnWindowFocus: <A extends Atom<any>>(self: A) => WithoutSerializable<A>
const windowFocusSignal: Atom<number>
const withServerValue: <A extends Atom<any>>(self: A, read: (get: <A>(atom: Atom<A>) => A) => Type<A>) => A
const withServerValueInitial: <A extends Atom<Result.Result<any, any>>>(self: A) => A
const getServerValue: <A>(self: Atom<A>, registry: Registry.Registry) => A

interface Serializable<S extends Schema.Schema.Any> {
  readonly [SerializableTypeId]: { readonly key: string; readonly encode: (value: S["Type"]) => S["Encoded"]; readonly decode: (value: S["Encoded"]) => S["Type"] }
}
const isSerializable: (self: Atom<any>) => self is Atom<any> & Serializable<any>
```

[PUBLIC_TYPE_SCOPE]: result-function shapes, optimistic, batching, registry-bound conversions
- rail: state-cell

```ts contract
// @effect-atom/atom/Atom — result-fn + optimistic + symbols
interface AtomResultFn<Arg, A, E = never> extends Writable<Result.Result<A, E>, Arg | Reset | Interrupt> {}
type PullResult<A, E = never> = Result.Result<{ readonly done: boolean; readonly items: Arr.NonEmptyArray<A> }, E | Cause.NoSuchElementException>
const Reset: unique symbol;     type Reset = typeof Reset
const Interrupt: unique symbol; type Interrupt = typeof Interrupt

const optimistic: <A>(self: Atom<A>) => Writable<A, Atom<Result.Result<A, unknown>>>
const optimisticFn: <A, W, XA, XE, OW = void>(self: Writable<A, Atom<Result.Result<W, unknown>>>, options: {
  readonly reducer: (current: NoInfer<A>, update: OW) => NoInfer<W>
  readonly fn: AtomResultFn<OW, XA, XE> | ((set: (result: NoInfer<W>) => void) => AtomResultFn<OW, XA, XE>)
}) => AtomResultFn<OW, XA, XE>

const batch: (f: () => void) => void

// AtomRuntime — layer-scoped sub-atom factory
interface AtomRuntime<R, ER = never> extends Atom<Result.Result<Runtime.Runtime<R>, ER>> {
  readonly factory: RuntimeFactory
  readonly layer: Atom<Layer.Layer<R, ER>>
  readonly atom: { /* Effect/Stream overloads, requires Scope | R | AtomRegistry | Reactivity → Atom<Result<A, E | ER>> */ }
  readonly fn: { /* Arg-curried + direct Effect/Stream overloads → AtomResultFn<Arg, A, E | ER[ | NoSuchElementException]> */ }
  readonly pull: <A, E>(create: ((get: Context) => Stream.Stream<A, E, R | AtomRegistry | Reactivity.Reactivity>) | Stream.Stream<A, E, R | AtomRegistry | Reactivity.Reactivity>, options?: { readonly disableAccumulation?: boolean; readonly initialValue?: ReadonlyArray<A> }) => Writable<PullResult<A, E | ER>, void>
  readonly subscriptionRef: <A, E>(create: Effect.Effect<SubscriptionRef.SubscriptionRef<A>, E, Scope.Scope | R | AtomRegistry | Reactivity.Reactivity> | ((get: Context) => Effect.Effect<SubscriptionRef.SubscriptionRef<A>, E, Scope.Scope | R | AtomRegistry | Reactivity.Reactivity>)) => Writable<Result.Result<A, E>, A>
  readonly subscribable: <A, E, E1 = never>(create: Effect.Effect<Subscribable.Subscribable<A, E, R>, E1, Scope.Scope | R | AtomRegistry | Reactivity.Reactivity> | ((get: Context) => Effect.Effect<Subscribable.Subscribable<A, E, R>, E1, Scope.Scope | R | AtomRegistry | Reactivity.Reactivity>)) => Atom<Result.Result<A, E | E1>>
}
interface RuntimeFactory {
  <R, E>(create: Layer.Layer<R, E, AtomRegistry | Reactivity.Reactivity> | ((get: Context) => Layer.Layer<R, E, AtomRegistry | Reactivity.Reactivity>)): AtomRuntime<R, E>
  readonly memoMap: Layer.MemoMap
  readonly addGlobalLayer: <A, E>(layer: Layer.Layer<A, E, AtomRegistry | Reactivity.Reactivity>) => void
  readonly withReactivity: (keys: ReadonlyArray<unknown> | ReadonlyRecord<string, ReadonlyArray<unknown>>) => <A extends Atom<any>>(atom: A) => A
}

// registry-bound conversions — all require AtomRegistry from context
const toStream: <A>(self: Atom<A>) => Stream.Stream<A, never, AtomRegistry>
const toStreamResult: <A, E>(self: Atom<Result.Result<A, E>>) => Stream.Stream<A, E, AtomRegistry>
const get: <A>(self: Atom<A>) => Effect.Effect<A, never, AtomRegistry>
const getResult: <A, E>(self: Atom<Result.Result<A, E>>, options?: { readonly suspendOnWaiting?: boolean | undefined }) => Effect.Effect<A, E, AtomRegistry>
const refresh: <A>(self: Atom<A>) => Effect.Effect<void, never, AtomRegistry>
const modify: <R, W, A>(self: Writable<R, W>, f: (_: R) => [returnValue: A, nextValue: W]) => Effect.Effect<A, never, AtomRegistry>
const set: <R, W>(self: Writable<R, W>, value: W) => Effect.Effect<void, never, AtomRegistry>
const update: <R, W>(self: Writable<R, W>, f: (_: R) => W) => Effect.Effect<void, never, AtomRegistry>
```

## [03]-[RESULT]

`@effect-atom/atom/Result` — the three-state computation result an effect/stream atom holds. `Result<A, E>` is the
union `Initial | Success | Failure`, each a `Result.Proto` carrying a `waiting` flag; `Failure` carries a
`Cause<E>` and an `Option<Success>` previous value. The module ships constructors, the `waiting`/`previousSuccess`
lifecycle helpers, the guard/refinement family, the `match`/`matchWithError`/`matchWithWaiting` folds, the chainable
`builder` render DSL, the `all` combinator, and the `Schema`/`Encoded`/`PartialEncoded` serialization family.

[PUBLIC_TYPE_SCOPE]: result union and proto
- rail: state-cell

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]  | [CAPABILITY]             |
| :-----: | :------------------ | :------------- | :----------------------- |
|  [01]   | `Result<A, E>`      | union          | three-state result       |
|  [02]   | `Result.Proto<A,E>` | interface      | shared result protocol   |
|  [03]   | `Initial`           | interface      | initial variant          |
|  [04]   | `Success`           | interface      | success variant          |
|  [05]   | `Failure`           | interface      | failure variant          |
|  [06]   | `Result.Success<R>` | type alias     | success extraction       |
|  [07]   | `Result.Failure<R>` | type alias     | failure extraction       |
|  [08]   | `With<R, A, E>`     | type alias     | variant-preserving retag |
|  [09]   | `TypeId`            | internal brand | proto brand key          |

[PUBLIC_TYPE_SCOPE]: result constructors and guards
- rail: state-cell

| [INDEX] | [SYMBOL]                                                  | [TYPE_FAMILY] | [CAPABILITY]              |
| :-----: | :-------------------------------------------------------- | :------------ | :------------------------ |
|  [01]   | `isResult` / `isInitial` / `isNotInitial`                 | guard         | result-state refinement   |
|  [02]   | `isSuccess` / `isFailure` / `isInterrupted` / `isWaiting` | guard         | result-state refinement   |
|  [03]   | `initial` / `success`                                     | constructor   | initial or success value  |
|  [04]   | `failure` / `fail`                                        | constructor   | failure value             |
|  [05]   | `fromExit` / `fromExitWithPrevious`                       | constructor   | `Exit` conversion         |
|  [06]   | `failureWithPrevious` / `failWithPrevious`                | constructor   | failure with history      |
|  [07]   | `waitingFrom` / `waiting` / `touch` / `replacePrevious`   | lifecycle     | waiting/previous handling |

```ts contract
// @effect-atom/atom/Result — union + proto
declare const TypeId: TypeId             // internal (not exported)
type TypeId = "~effect-atom/atom/Result" // internal (not exported)
type Result<A, E = never> = Initial<A, E> | Success<A, E> | Failure<A, E>

declare namespace Result {
  interface Proto<A, E> extends Pipeable {
    readonly [TypeId]: { readonly E: (_: never) => E; readonly A: (_: never) => A }
    readonly waiting: boolean
  }
  type Success<R> = R extends Result<infer A, infer _> ? A : never
  type Failure<R> = R extends Result<infer _, infer E> ? E : never
}
interface Initial<A, E = never> extends Result.Proto<A, E> { readonly _tag: "Initial" }
interface Success<A, E = never> extends Result.Proto<A, E> { readonly _tag: "Success"; readonly value: A; readonly timestamp: number }
interface Failure<A, E = never> extends Result.Proto<A, E> { readonly _tag: "Failure"; readonly cause: Cause.Cause<E>; readonly previousSuccess: Option.Option<Success<A, E>> }
type With<R extends Result<any, any>, A, E> = R extends Initial<infer _A, infer _E> ? Initial<A, E> : R extends Success<infer _A, infer _E> ? Success<A, E> : R extends Failure<infer _A, infer _E> ? Failure<A, E> : never

const isResult: (u: unknown) => u is Result<unknown, unknown>
const isWaiting: <A, E>(result: Result<A, E>) => boolean
const isInitial: <A, E>(result: Result<A, E>) => result is Initial<A, E>
const isNotInitial: <A, E>(result: Result<A, E>) => result is Success<A, E> | Failure<A, E>
const isSuccess: <A, E>(result: Result<A, E>) => result is Success<A, E>
const isFailure: <A, E>(result: Result<A, E>) => result is Failure<A, E>
const isInterrupted: <A, E>(result: Result<A, E>) => result is Failure<A, E>

const initial: <A = never, E = never>(waiting?: boolean) => Initial<A, E>
const success: <A, E = never>(value: A, options?: { readonly waiting?: boolean | undefined; readonly timestamp?: number | undefined }) => Success<A, E>
const failure: <A, E = never>(cause: Cause.Cause<E>, options?: { readonly previousSuccess?: Option.Option<Success<A, E>> | undefined; readonly waiting?: boolean | undefined }) => Failure<A, E>
const fail: <E, A = never>(error: E, options?: { readonly previousSuccess?: Option.Option<Success<A, E>> | undefined; readonly waiting?: boolean | undefined }) => Failure<A, E>
const fromExit: <A, E>(exit: Exit.Exit<A, E>) => Success<A, E> | Failure<A, E>
const fromExitWithPrevious: <A, E>(exit: Exit.Exit<A, E>, previous: Option.Option<Result<A, E>>) => Success<A, E> | Failure<A, E>
const failureWithPrevious: <A, E>(cause: Cause.Cause<E>, options: { readonly previous: Option.Option<Result<A, E>>; readonly waiting?: boolean | undefined }) => Failure<A, E>
const failWithPrevious: <A, E>(error: E, options: { readonly previous: Option.Option<Result<A, E>>; readonly waiting?: boolean | undefined }) => Failure<A, E>
const waitingFrom: <A, E>(previous: Option.Option<Result<A, E>>) => Result<A, E>
const waiting: <R extends Result<any, any>>(self: R, options?: { readonly touch?: boolean | undefined }) => R
const touch: <A extends Result<any, any>>(result: A) => A
const replacePrevious: <R extends Result<any, any>, XE, A>(self: R, previous: Option.Option<Result<A, XE>>) => With<R, A, Result.Failure<R>>
```

[PUBLIC_TYPE_SCOPE]: accessors, folds, builder, combinators
- rail: state-cell

```ts contract
// @effect-atom/atom/Result — accessors
const value: <A, E>(self: Result<A, E>) => Option.Option<A>
const getOrElse: { <B>(orElse: LazyArg<B>): <A, E>(self: Result<A, E>) => A | B; <A, E, B>(self: Result<A, E>, orElse: LazyArg<B>): A | B }
const getOrThrow: <A, E>(self: Result<A, E>) => A
const cause: <A, E>(self: Result<A, E>) => Option.Option<Cause.Cause<E>>
const error: <A, E>(self: Result<A, E>) => Option.Option<E>
const toExit: <A, E>(self: Result<A, E>) => Exit.Exit<A, E | Cause.NoSuchElementException>

// folds (data-first overload shown; each is dual)
const map: <E, A, B>(self: Result<A, E>, f: (a: A) => B) => Result<B, E>
const flatMap: <E, A, B, E2>(self: Result<A, E>, f: (a: A, prev: Success<A, E>) => Result<B, E2>) => Result<B, E | E2>
const match: <A, E, X, Y, Z>(self: Result<A, E>, options: { readonly onInitial: (_: Initial<A, E>) => X; readonly onFailure: (_: Failure<A, E>) => Y; readonly onSuccess: (_: Success<A, E>) => Z }) => X | Y | Z
const matchWithError: <A, E, W, X, Y, Z>(self: Result<A, E>, options: { readonly onInitial: (_: Initial<A, E>) => W; readonly onError: (error: E, _: Failure<A, E>) => X; readonly onDefect: (defect: unknown, _: Failure<A, E>) => Y; readonly onSuccess: (_: Success<A, E>) => Z }) => W | X | Y | Z
const matchWithWaiting: <A, E, W, X, Y, Z>(self: Result<A, E>, options: { readonly onWaiting: (_: Result<A, E>) => W; readonly onError: (error: E, _: Failure<A, E>) => X; readonly onDefect: (defect: unknown, _: Failure<A, E>) => Y; readonly onSuccess: (_: Success<A, E>) => Z }) => W | X | Y | Z
const all: <const Arg extends Iterable<any> | Record<string, any>>(results: Arg) => Result<any, any>

// builder render DSL — chain on*(…) handlers then orElse/orNull/render
const builder: <A extends Result<any, any>>(self: A) => Builder<never, A extends Success<infer _A, infer _E> ? _A : never, A extends Failure<infer _A, infer _E_1> ? _E_1 : never, A extends Initial<infer _A, infer _E_2> ? true : never>
type Builder<Out, A, E, I> = Pipeable & {
  onWaiting<B>(f: (result: Result<A, E>) => B): Builder<Out | B, A, E, I>
  onDefect<B>(f: (defect: unknown, result: Failure<A, E>) => B): Builder<Out | B, A, E, I>
  orElse<B>(orElse: LazyArg<B>): Out | B
  orNull(): Out | null
  render(): [A | I] extends [never] ? Out : Out | null
} & (/* onInitial/onInitialOrWaiting when I */ {}) & (/* onSuccess when A */ {}) & (/* onFailure/onError/onErrorIf/onErrorTag when E */ {})
```

[PUBLIC_TYPE_SCOPE]: serialization (`Schema`/`Encoded`/`PartialEncoded`)
- rail: state-cell

```ts contract
// @effect-atom/atom/Result — Schemas
type PartialEncoded<A, E> =
  | { readonly _tag: "Initial"; readonly waiting: boolean }
  | { readonly _tag: "Success"; readonly waiting: boolean; readonly timestamp: number; readonly value: A }
  | { readonly _tag: "Failure"; readonly waiting: boolean; readonly previousValue: Option.Option<A>; readonly cause: Cause.Cause<E> }
type Encoded<A, E> =
  | { readonly _tag: "Initial"; readonly waiting: boolean }
  | { readonly _tag: "Success"; readonly waiting: boolean; readonly timestamp: number; readonly value: A }
  | { readonly _tag: "Failure"; readonly waiting: boolean; readonly previousValue: Schema_.OptionEncoded<A>; readonly cause: Schema_.CauseEncoded<E, unknown> }
const schemaFromSelf: Schema_.Schema<Result<any, any>>
interface Schema<Success extends Schema_.Schema.All, Error extends Schema_.Schema.All> extends Schema_.Schema<Result<Success["Type"], Error["Type"]>, Encoded<Success["Encoded"], Error["Encoded"]>, Success["Context"] | Error["Context"]> {}
const Schema: <Success extends Schema_.Schema.All = typeof Schema_.Never, Error extends Schema_.Schema.All = typeof Schema_.Never>(options: { readonly success?: Success | undefined; readonly error?: Error | undefined }) => Schema<Success, Error>
```

## [04]-[REGISTRY]

`@effect-atom/atom/Registry` — the node store the cells live in. `Registry` is the mutable graph that holds each
atom's `Node`, drives `get`/`set`/`modify`/`update`/`refresh`/`subscribe`, and owns `reset`/`dispose`. `make`
constructs one with optional seed values and scheduling; `AtomRegistry` is the `Context.TagClass`
(`"@effect/atom/Registry/CurrentRegistry"`) every registry-bound `Atom` conversion requires, with `layer`/`layerOptions`
providing it.

```ts contract
// @effect-atom/atom/Registry
declare const TypeId: TypeId               // internal (not exported)
type TypeId = "~effect-atom/atom/Registry" // internal (not exported)
const isRegistry: (u: unknown) => u is Registry

interface Registry {
  readonly [TypeId]: TypeId
  readonly getNodes: () => ReadonlyMap<Atom.Atom<any> | string, Node<any>>
  readonly get: <A>(atom: Atom.Atom<A>) => A
  readonly mount: <A>(atom: Atom.Atom<A>) => () => void
  readonly refresh: <A>(atom: Atom.Atom<A>) => void
  readonly set: <R, W>(atom: Atom.Writable<R, W>, value: W) => void
  readonly setSerializable: (key: string, encoded: unknown) => void
  readonly modify: <R, W, A>(atom: Atom.Writable<R, W>, f: (_: R) => [returnValue: A, nextValue: W]) => A
  readonly update: <R, W>(atom: Atom.Writable<R, W>, f: (_: R) => W) => void
  readonly subscribe: <A>(atom: Atom.Atom<A>, f: (_: A) => void, options?: { readonly immediate?: boolean }) => () => void
  readonly reset: () => void
  readonly dispose: () => void
}
interface Node<A> { readonly atom: Atom.Atom<A>; readonly value: () => A } // not exported

const make: (options?: {
  readonly initialValues?: Iterable<readonly [Atom.Atom<any>, any]> | undefined
  readonly scheduleTask?: ((f: () => void) => void) | undefined
  readonly timeoutResolution?: number | undefined
  readonly defaultIdleTTL?: number | undefined
} | undefined) => Registry

// AtomRegistry tag — Context.TagClass<AtomRegistry, "@effect/atom/Registry/CurrentRegistry", Registry>
declare class AtomRegistry extends AtomRegistry_base {}
const layerOptions: (options?: { readonly initialValues?: …; readonly scheduleTask?: …; readonly timeoutResolution?: number | undefined; readonly defaultIdleTTL?: number | undefined }) => Layer.Layer<AtomRegistry>
const layer: Layer.Layer<AtomRegistry>

// registry-first conversions (dual)
const toStream: <A>(self: Registry, atom: Atom.Atom<A>) => Stream.Stream<A>
const toStreamResult: <A, E>(self: Registry, atom: Atom.Atom<Result.Result<A, E>>) => Stream.Stream<A, E>
const getResult: <A, E>(self: Registry, atom: Atom.Atom<Result.Result<A, E>>, options?: { readonly suspendOnWaiting?: boolean | undefined }) => Effect.Effect<A, E>
```

## [05]-[ATOM_REF]

`@effect-atom/atom/AtomRef` — a plain mutable reference outside the registry graph (synchronous, `Equal`-based
subscription). `ReadonlyRef<A>` carries `key`/`value`/`subscribe`/`map`; `AtomRef<A>` adds `prop`/`set`/`update`;
`Collection<A>` is a `ReadonlyRef<ReadonlyArray<AtomRef<A>>>` with `push`/`insertAt`/`remove`/`toArray`.

```ts contract
// @effect-atom/atom/AtomRef
declare const TypeId: TypeId              // internal (not exported)
type TypeId = "~effect-atom/atom/AtomRef" // internal (not exported)

interface ReadonlyRef<A> extends Equal.Equal {
  readonly [TypeId]: TypeId
  readonly key: string
  readonly value: A
  readonly subscribe: (f: (a: A) => void) => () => void
  readonly map: <B>(f: (a: A) => B) => ReadonlyRef<B>
}
interface AtomRef<A> extends ReadonlyRef<A> {
  readonly prop: <K extends keyof A>(prop: K) => AtomRef<A[K]>
  readonly set: (value: A) => AtomRef<A>
  readonly update: (f: (value: A) => A) => AtomRef<A>
}
interface Collection<A> extends ReadonlyRef<ReadonlyArray<AtomRef<A>>> {
  readonly push: (item: A) => Collection<A>
  readonly insertAt: (index: number, item: A) => Collection<A>
  readonly remove: (ref: AtomRef<A>) => Collection<A>
  readonly toArray: () => Array<A>
}
const make: <A>(value: A) => AtomRef<A>
const collection: <A>(items: Iterable<A>) => Collection<A>
```

## [06]-[ATOM_RPC]

`@effect-atom/atom/AtomRpc` — projects an `@effect/rpc` `RpcGroup` into a `Context.Tag`-backed client whose
procedures become atoms.

```ts contract
// @effect-atom/atom/AtomRpc
interface AtomRpcClient<Self, Id extends string, Rpcs extends Rpc.Any, E> extends Context.Tag<Self, RpcClient.RpcClient.Flat<Rpcs, RpcClientError>> {
  new (_: never): Context.TagClassShape<Id, RpcClient.RpcClient.Flat<Rpcs, RpcClientError>>
  readonly layer: Layer.Layer<Self, E>
  readonly runtime: Atom.AtomRuntime<Self, E>
  readonly mutation: <Tag extends Rpc.Tag<Rpcs>>(arg: Tag) =>
    Atom.AtomResultFn<{ readonly payload: Rpc.PayloadConstructor<…>; readonly reactivityKeys?: …; readonly headers?: Headers.Input | undefined }, _Success["Type"], _Error["Type"] | E | _Middleware["failure"]["Type"]>
  readonly query: <Tag extends Rpc.Tag<Rpcs>>(tag: Tag, payload: Rpc.PayloadConstructor<Rpc.ExtractTag<Rpcs, Tag>>, options?: { readonly headers?: Headers.Input | undefined; readonly reactivityKeys?: …; readonly timeToLive?: Duration.DurationInput | undefined }) =>
    Atom.Writable<Atom.PullResult<_A["Type"], …>, void> | Atom.Atom<Result.Result<_Success["Type"], _Error["Type"] | E | _Middleware["failure"]["Type"]>>
}

const Tag: <Self>() => <const Id extends string, Rpcs extends Rpc.Any, ER, RM = RpcClient.Protocol | Rpc.MiddlewareClient<NoInfer<Rpcs>> | Rpc.Context<NoInfer<Rpcs>>>(id: Id, options: {
  readonly group: RpcGroup.RpcGroup<Rpcs>
  readonly protocol: Layer.Layer<Exclude<NoInfer<RM>, Scope>, ER>
  readonly spanPrefix?: string | undefined
  readonly spanAttributes?: Record<string, unknown> | undefined
  readonly generateRequestId?: (() => RequestId) | undefined
  readonly disableTracing?: boolean | undefined
  readonly makeEffect?: Effect.Effect<RpcClient.RpcClient.Flat<Rpcs, RpcClientError>, never, RM> | undefined
  readonly runtime?: Atom.RuntimeFactory | undefined
}) => AtomRpcClient<Self, Id, Rpcs, ER>
```

## [07]-[ATOM_HTTP_API]

`@effect-atom/atom/AtomHttpApi` — the same client-as-atoms projection over an `@effect/platform` `HttpApi`.

```ts contract
// @effect-atom/atom/AtomHttpApi
interface AtomHttpApiClient<Self, Id extends string, Groups extends HttpApiGroup.HttpApiGroup.Any, ApiE, E> extends Context.Tag<Self, Simplify<HttpApiClient.Client<Groups, ApiE, never>>> {
  new (_: never): Context.TagClassShape<Id, Simplify<HttpApiClient.Client<Groups, ApiE, never>>>
  readonly layer: Layer.Layer<Self, E>
  readonly runtime: Atom.AtomRuntime<Self, E>
  readonly mutation: <GroupName, Name, …, const WithResponse extends boolean = false>(group: GroupName, endpoint: Name, options?: { readonly withResponse?: WithResponse | undefined }) =>
    Atom.AtomResultFn<Simplify<HttpApiEndpoint.HttpApiEndpoint.ClientRequest<_Path, _UrlParams, _Payload, _Headers, false> & { readonly reactivityKeys?: … }>, WithResponse extends true ? [_Success, HttpClientResponse] : _Success, _Error | HttpApiGroup.HttpApiGroup.Error<Group> | E | HttpClientError.HttpClientError | ParseResult.ParseError>
  readonly query: <GroupName, Name, …, const WithResponse extends boolean = false>(group: GroupName, endpoint: Name, request: Simplify<HttpApiEndpoint.HttpApiEndpoint.ClientRequest<_Path, _UrlParams, _Payload, _Headers, WithResponse> & { readonly reactivityKeys?: …; readonly timeToLive?: Duration.DurationInput | undefined }>) =>
    Atom.Atom<Result.Result<WithResponse extends true ? [_Success, HttpClientResponse] : _Success, _Error | HttpApiGroup.HttpApiGroup.Error<Group> | E | HttpClientError.HttpClientError | ParseResult.ParseError>>
}

const Tag: <Self>() => <const Id extends string, ApiId extends string, Groups extends HttpApiGroup.HttpApiGroup.Any, ApiE, E, R>(id: Id, options: {
  readonly api: HttpApi.HttpApi<ApiId, Groups, ApiE, R>
  readonly httpClient: Layer.Layer<HttpApiMiddleware.HttpApiMiddleware.Without<NoInfer<R> | HttpApiGroup.HttpApiGroup.ClientContext<NoInfer<Groups>>> | HttpClient.HttpClient, E>
  readonly transformClient?: ((client: HttpClient.HttpClient) => HttpClient.HttpClient) | undefined
  readonly transformResponse?: ((effect: Effect.Effect<unknown, unknown>) => Effect.Effect<unknown, unknown>) | undefined
  readonly baseUrl?: URL | string | undefined
  readonly runtime?: Atom.RuntimeFactory | undefined
}) => AtomHttpApiClient<Self, Id, Groups, ApiE, E>
```

## [08]-[HYDRATION]

`@effect-atom/atom/Hydration` — SSR transfer.

```ts contract
// @effect-atom/atom/Hydration
interface DehydratedAtom { readonly "~@effect-atom/atom/DehydratedAtom": true }
interface DehydratedAtomValue extends DehydratedAtom {
  readonly key: string
  readonly value: unknown
  readonly dehydratedAt: number
  readonly resultPromise?: Promise<unknown> | undefined
}
const dehydrate: (registry: Registry.Registry, options?: { readonly encodeInitialAs?: "ignore" | "promise" | "value-only" | undefined }) => Array<DehydratedAtom>
const toValues: (state: ReadonlyArray<DehydratedAtom>) => Array<DehydratedAtomValue>
const hydrate: (registry: Registry.Registry, dehydratedState: Iterable<DehydratedAtom>) => void
```

## [09]-[GAPS]

- `TypeId`/`WritableTypeId` (`Atom`, `Result`, `Registry`, `AtomRef`) are declared without `export` — module-internal brand keys, public only as the `[TypeId]` interface keys; only `SerializableTypeId`/`ServerValueTypeId` (`Atom`) are exported brand consts.
- `Atom.AtomRuntime.atom`/`.fn` overload bodies are summarized (full Effect/Stream overload set per `Atom.d.ts:174-211`); `AtomRuntime.fn` adds a `reactivityKeys` option absent from the top-level `Atom.fn`. The `R`-required variants thread `Scope.Scope | R | AtomRegistry | Reactivity.Reactivity`.
- Dual (data-first/data-last) combinators in `Atom` and `Result` are shown data-first only; each also has the curried data-last overload.
- `Result.all`, the `Result.Builder` conditional `on*` blocks, and the `AtomRpc`/`AtomHttpApi` `query`/`mutation` conditional-type result bodies are abbreviated to their resolved shapes.
- `Registry.Node<A>` is a module-internal interface (not re-exported); listed for completeness as the `getNodes` element.
- `@effect-atom/atom-react` (0.5.0, the React binding + atom inspector) is a separate package admitted on the view/UI surface and is not reflected on this page.
