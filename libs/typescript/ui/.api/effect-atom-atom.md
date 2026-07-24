# [TS_UI_API_EFFECT_ATOM_ATOM]

`@effect-atom/atom` folds an `Effect`/`Stream`/`Layer` graph into memoized, dependency-tracked `Atom` cells — the one `ONE_FOLD_ONE_BINDING` store the `ui` folder binds for all client state. Async is a value: an effect-backed atom holds `Result<A, E>` whose `waiting`/`previous` arms keep the last-good value through a refresh, `Registry` owns every node's lifecycle, and `AtomHttpApi.Tag`/`AtomRpc.Tag` bind a `@effect/platform`/`@effect/rpc` contract into reactive atoms with no fetching glue.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@effect-atom/atom`
- package: `@effect-atom/atom` (MIT)
- module: ESM + CJS dual (`.d.ts` under `dist/dts`), `sideEffects: []` fully tree-shakeable; per-namespace deep-import subpaths `@effect-atom/atom/{Atom,Result,Registry,AtomRef,AtomHttpApi,AtomRpc,Hydration}`
- runtime: framework-neutral, any JS runtime; peers `effect`, `@effect/platform`, `@effect/rpc`, `@effect/experimental`; the reactive graph is pure with only the `Registry` stateful, so React is not a peer — `atom-react` adds the hooks
- rail: state binding — the one `ONE_FOLD_ONE_BINDING` store; `atom/binding` + `atom/derive` own it, every `view` row reads it

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: atom algebra core — the reactive cell and its read/write contexts
[CONTEXT]: `get` `result` `once` `mount` `subscribe` `stream` `self` `setSelf` `addFinalizer` `registry`
[WRITE_CONTEXT]: `get` `set` `setSelf` `refreshSelf`

| [INDEX] | [SYMBOL]                                    | [TYPE_FAMILY]    | [CAPABILITY]                                                         |
| :-----: | :------------------------------------------ | :--------------- | :------------------------------------------------------------------- |
|  [01]   | `Atom<A>`                                   | reactive cell    | extends `Pipeable`/`Inspectable`; `.pipe`-composable, memoized       |
|  [02]   | `Writable<R, W = R>`                        | writable cell    | read `R`, write `W`; the only atom `useAtom`/`set` accept            |
|  [03]   | `Context`                                   | read context     | the derived-atom `get` arg; reads track deps, `addFinalizer` cleanup |
|  [04]   | `WriteContext<A>`                           | write context    | the write callback arg of `writable`; commits derived writes         |
|  [05]   | `FnContext`                                 | fn read context  | the `get`-plus-args context of a `fn`/`fnSync` atom                  |
|  [06]   | `Type<T>` / `Success<T>` / `Failure<T>`     | type projections | extract an atom's value/success/error type, no runtime cost          |
|  [07]   | `PullSuccess<T>` / `WithoutSerializable<T>` | type projections | the `pull` success type and serializable-stripped type               |
|  [08]   | `isAtom` / `isWritable` / `isSerializable`  | guards           | boundary narrowing before `set`/serialization                        |

[PUBLIC_TYPE_SCOPE]: async result ADT — the value every effect-backed atom holds

| [INDEX] | [SYMBOL]                                        | [TYPE_FAMILY]  | [CAPABILITY]                                                 |
| :-----: | :---------------------------------------------- | :------------- | :----------------------------------------------------------- |
|  [01]   | `Result<A, E>`                                  | async union    | `Initial \| Success \| Failure`; every atom yields it        |
|  [02]   | `Initial<A,E>` / `Success<A,E>`                 | union arms     | each carries `waiting: boolean` + `previous: Option<Result>` |
|  [03]   | `Failure<A,E>`                                  | union arm      | the failed arm; carries `Cause<E>` plus `waiting`/`previous` |
|  [04]   | `AtomResultFn<Arg, A, E>`                       | callable atom  | `Writable<Result<A,E>, Arg \| Reset \| Interrupt>`           |
|  [05]   | `Reset` / `Interrupt`                           | write commands | write to clear to `Initial` or cancel the fiber              |
|  [06]   | `PullResult<A, E>`                              | paged result   | a `pull` atom's `Result` over a `Stream`; `{ done, items }`  |
|  [07]   | `Result.Schema<Success, Error>`                 | wire schema    | encode/decode a `Result` across the SSR/persistence boundary |
|  [08]   | `PartialEncoded` / `Encoded` / `schemaFromSelf` | wire schema    | the encoded forms and self-schema of a `Result.Schema`       |
|  [09]   | `Result.Builder<Out, A, E, I>`                  | fold builder   | pipeable exhaustive `Result` fold with fluent case arms      |

[PUBLIC_TYPE_SCOPE]: runtime, registry, and fine-grained refs
[REGISTRY]: `get` `set` `modify` `update` `mount` `subscribe` `refresh` `setSerializable` `reset` `dispose` `getNodes`
- `AtomHttpApiClient`/`AtomRpcClient` extend `Context.Tag` with `.layer`/`.runtime`/`.query`/`.mutation`.

| [INDEX] | [SYMBOL]                                          | [TYPE_FAMILY]        | [CAPABILITY]                                                 |
| :-----: | :------------------------------------------------ | :------------------- | :----------------------------------------------------------- |
|  [01]   | `AtomRuntime<R, ER>` / `RuntimeFactory`           | layer-backed runtime | one `Layer`-built graph shared by every atom built from it   |
|  [02]   | `Registry`                                        | store                | the imperative store; `browser`/tests drive it directly      |
|  [03]   | `AtomRegistry`                                    | registry Tag         | the `Context.Tag` for the ambient registry                   |
|  [04]   | `AtomRef<A>` / `ReadonlyRef<A>` / `Collection<A>` | mutable ref          | `Equal`-based cursor into a sub-value; owner not re-run      |
|  [05]   | `Serializable<S>`                                 | serializable marker  | an atom carrying a `Schema` for the wire or `KeyValueStore`  |
|  [06]   | `AtomHttpApiClient<Self, Id, Groups, ApiE, E>`    | api-client Tag       | a bound `HttpApi`; `.query` read, `.mutation` `AtomResultFn` |
|  [07]   | `AtomRpcClient<Self, Id, Rpcs, E>`                | rpc-client Tag       | a bound `RpcGroup`; streaming `.query` → `PullResult` atom   |
|  [08]   | `DehydratedAtom` / `DehydratedAtomValue`          | SSR snapshot         | the serialized registry state emitted and rehydrated         |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: constructing atoms — one polymorphic `make` and its explicit rows

| [INDEX] | [SURFACE]                                            | [SHAPE]         | [CAPABILITY]                                             |
| :-----: | :--------------------------------------------------- | :-------------- | :------------------------------------------------------- |
|  [01]   | `Atom.make(effect \| stream \| value \| (get) => …)` | universal ctor  | the one polymorphic entry; discriminates on argument     |
|  [02]   | `Atom.readable(read, refresh?)`                      | explicit ctor   | a hand-built read-only atom                              |
|  [03]   | `Atom.writable(read, write, refresh?)`               | explicit ctor   | a hand-built read/write atom when `make` is too loose    |
|  [04]   | `Atom.fn(create, options?)` / `Atom.fnSync(f)`       | callable atom   | `AtomResultFn`: call with `Arg` to run a computation     |
|  [05]   | `Atom.pull(streamOrCreate, options?)`                | paged stream    | a `PullResult` atom over a `Stream`; write advances page |
|  [06]   | `Atom.subscriptionRef(…)` / `Atom.subscribable(…)`   | effect-ref atom | bridge a `SubscriptionRef`/`Subscribable` into an atom   |
|  [07]   | `Atom.family((arg) => atom)`                         | keyed factory   | memoize one atom per argument key (per-entity), no leak  |
|  [08]   | `Atom.runtime(layer)` / `Atom.context({ memoMap })`  | runtime root    | build an `AtomRuntime` from a `Layer`; shared graph      |
|  [09]   | `Atom.defaultMemoMap`                                | runtime root    | the shared memo map atoms build through                  |

[ENTRYPOINT_SCOPE]: combinators — deriving, mapping, and tuning atoms in place

| [INDEX] | [SURFACE]                                                      | [SHAPE]         | [CAPABILITY]                                         |
| :-----: | :------------------------------------------------------------- | :-------------- | :--------------------------------------------------- |
|  [01]   | `Atom.map(f)` / `Atom.mapResult(f)` / `Atom.transform(f)`      | projection      | `mapResult` maps `Success`; `transform` via `get`    |
|  [02]   | `Atom.debounce(ms)` / `Atom.withReactivity(keys)`              | update policy   | rate-limit writes; declare reactivity keys           |
|  [03]   | `Atom.keepAlive` / `Atom.autoDispose`                          | lifecycle       | pin a hot atom or GC an idle one                     |
|  [04]   | `Atom.setLazy(bool)` / `Atom.setIdleTTL(duration)`             | lifecycle       | lazy toggle; GC after an idle TTL, per node          |
|  [05]   | `Atom.withFallback(fallbackAtom)` / `Atom.initialValue(value)` | seed            | seed a `Result` atom's first render                  |
|  [06]   | `Atom.withLabel(name)`                                         | debug           | label for devtools/inspection                        |
|  [07]   | `Atom.optimistic(self)` / `Atom.optimisticFn(options)`         | optimistic UI   | write optimistically, reconcile the real `Result`    |
|  [08]   | `Atom.refreshOnWindowFocus` / `Atom.windowFocusSignal`         | refresh trigger | re-run an atom on window focus (stale-while-focus)   |
|  [09]   | `Atom.makeRefreshOnSignal(signal)`                             | refresh trigger | re-run an atom on any signal atom                    |
|  [10]   | `Atom.kvs({ runtime, key, schema, defaultValue })`             | persistence     | back an atom by a `KeyValueStore` runtime + `Schema` |
|  [11]   | `Atom.searchParam(name, { schema? })`                          | persistence     | bind an atom to a URL search param, `Schema`-typed   |

[ENTRYPOINT_SCOPE]: `Result` folds — rendering async state exhaustively

| [INDEX] | [SURFACE]                                                 | [SHAPE]         | [CAPABILITY]                                        |
| :-----: | :-------------------------------------------------------- | :-------------- | :-------------------------------------------------- |
|  [01]   | `Result.match(self, { onInitial, onSuccess, onFailure })` | exhaustive fold | the total dispatch over `Result`                    |
|  [02]   | `.matchWithWaiting(…)` / `.matchWithError(…)`             | exhaustive fold | `matchWithWaiting` folds `waiting` into its own arm |
|  [03]   | `Result.builder(self)`                                    | fluent fold     | pipeable fluent fold, arms added incrementally      |
|  [04]   | `Result.value` / `.getOrElse` / `.getOrThrow`             | accessor        | pull the `Option<A>`/`Option<E>` out of a `Result`  |
|  [05]   | `Result.error` / `.cause`                                 | accessor        | the `Option<E>`/`Cause` of a `Failure`              |
|  [06]   | `Result.map(f)` / `.flatMap(f)`                           | combinator      | map/chain a `Result` value                          |
|  [07]   | `Result.all(results)` / `.toExit(self)`                   | combinator      | join several, or `.toExit` back to an `Exit`        |
|  [08]   | `Result.success` / `.failure` / `.fail` / `.initial`      | constructor     | build a `Result` arm directly                       |
|  [09]   | `Result.waiting` / `.fromExit`                            | constructor     | `waiting`/`replacePrevious` keep the previous value |
|  [10]   | `Result.isSuccess` / `.isFailure` / `.isInitial`          | guard           | narrow before an accessor                           |
|  [11]   | `Result.isWaiting` / `.isInterrupted`                     | guard           | `isInterrupted` marks a cancelled fiber             |

[ENTRYPOINT_SCOPE]: registry and effectful accessors — driving atoms from Effect/imperative code
- `Registry.make` accepts `{ initialValues?, scheduleTask?, timeoutResolution?, defaultIdleTTL? }`.

| [INDEX] | [SURFACE]                                                  | [SHAPE]            | [CAPABILITY]                                           |
| :-----: | :--------------------------------------------------------- | :----------------- | :----------------------------------------------------- |
|  [01]   | `Registry.make(options?)`                                  | store ctor         | the one registry per app; tests build an isolated one  |
|  [02]   | `Registry.layer` / `Registry.layerOptions(opts)`           | registry Layer     | provide the registry as an Effect service              |
|  [03]   | `Registry.AtomRegistry`                                    | registry Tag       | the `AtomRegistry` Tag effectful atoms resolve         |
|  [04]   | `registry.get(atom)` / `.set(atom, w)` / `.refresh(atom)`  | imperative rw      | read or drive an atom outside React                    |
|  [05]   | `registry.modify(atom, f)` / `.update(atom, f)`            | imperative rw      | `modify` returns a value and the next state atomically |
|  [06]   | `registry.mount(atom)`                                     | lifecycle          | keep a node hot for non-React subscribers              |
|  [07]   | `registry.subscribe(atom, f, { immediate? })`              | lifecycle          | returns an unsubscribe fn; `immediate` fires now       |
|  [08]   | `registry.reset()` / `.dispose()`                          | lifecycle          | reset all nodes or dispose the registry                |
|  [09]   | `Atom.get(self)` / `.set(self, w)` / `.modify` / `.update` | effectful accessor | read/write an atom in `Effect.gen`                     |
|  [10]   | `Atom.refresh(self)` / `.getResult(self)`                  | effectful accessor | each returns `Effect<…, AtomRegistry>`                 |
|  [11]   | `Atom.toStream(self)` / `.toStreamResult(self)`            | stream bridge      | observe an atom as an Effect `Stream`                  |
|  [12]   | `Registry.toStream` / `Registry.getResult`                 | stream bridge      | feed atom changes into a pipeline                      |
|  [13]   | `Atom.batch(f)`                                            | batch commit       | coalesce many writes into one notification pass        |

[ENTRYPOINT_SCOPE]: effect-service direct binding and SSR — the `AtomHttpApi`/`AtomRpc` service rows
- `class Api extends AtomHttpApi.Tag<Api>()(id, { api, httpClient, baseUrl?, transformClient?, transformResponse?, runtime? })`, then `Api.query(group, endpoint, { …request, reactivityKeys?, timeToLive? })` / `Api.mutation(group, endpoint)`.
- `class Rpc extends AtomRpc.Tag<Rpc>()(id, { group, protocol, spanPrefix?, disableTracing?, runtime? })`, then `Rpc.query(tag, payload, { reactivityKeys?, timeToLive? })` / `Rpc.mutation(tag)`.

| [INDEX] | [SURFACE]                                               | [SHAPE]          | [CAPABILITY]                                          |
| :-----: | :------------------------------------------------------ | :--------------- | :---------------------------------------------------- |
|  [01]   | `AtomHttpApi.Tag<Self>()(id, options)`                  | http binding     | bind an `HttpApi`; `.query`/`.mutation` atoms         |
|  [02]   | `AtomRpc.Tag<Self>()(id, options)`                      | rpc binding      | bind an `RpcGroup`; streaming `.query` → `PullResult` |
|  [03]   | `AtomRef.make(value)` / `AtomRef.collection(items)`     | fine-grained ref | mutable cursor / ordered ref collection               |
|  [04]   | `Atom.serializable(self, { schema })`                   | serializable     | mark an atom's value `Schema`-typed                   |
|  [05]   | `Atom.withServerValue(self, …)` / `Atom.getServerValue` | serializable     | carry a server-computed initial value                 |
|  [06]   | `Hydration.dehydrate(registry, options?)`               | SSR handoff      | dehydrate the registry to `DehydratedAtom[]`          |
|  [07]   | `Hydration.toValues(state)`                             | SSR handoff      | the dehydrated values array                           |
|  [08]   | `Hydration.hydrate(registry, state)`                    | SSR handoff      | rehydrate into a fresh registry                       |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `Atom<A>` is pure and lazy: it computes only when a `Registry` mounts it and recomputes only when a tracked dependency (another atom read through the `Context`) changes. `Registry` owns all state — mount/subscribe/refresh/dispose, idle-TTL GC, write scheduling — so an atom graph tests against an isolated `Registry.make()`.
- Async is a value, not a control path: an effect-backed atom holds `Result<A, E>`, every arm carrying `waiting` (a refresh is in flight) and `previous` (the last non-initial value). Refresh flips `waiting` true and keeps the old `Success` visible, so stale-while-revalidate is structural. `Result` maps to Effect `Exit` through `fromExit`/`toExit`, so the error channel is the branch's tagged `Cause`.
- `AtomRuntime` is the DI seam: `Atom.runtime(layer)` memoizes one `Layer` build (`defaultMemoMap`), and every atom built through it resolves services from that shared graph — one app runtime shares `wire`/`edge` clients, config, and telemetry across all client state.
- `AtomHttpApi.Tag`/`AtomRpc.Tag` invert data fetching: the `@effect/platform` `HttpApi` or `@effect/rpc` `RpcGroup` declaration is the single source, and the generated `Context.Tag` exposes each endpoint as an atom-returning method. Invalidation is `Atom.refresh`/`withReactivity` on the typed atom, never a string query-key cache; the response type is the endpoint's `Schema` success type.
- `Atom.make` owns creation, discriminating on its argument (effect, stream, value, read fn) into the right atom kind; `readable`/`writable`/`fn`/`pull`/`subscriptionRef` pin discrimination explicitly. New reactive behavior is a combinator on the owning atom (`optimistic`, `debounce`, `kvs`, `searchParam`), never a parallel atom type.

[STACKING]:
- `effect` (`libs/typescript/.api/effect.md`): the atom IS the Effect-to-reactive adapter — `Atom.make(effect)` lifts `Effect<A, E, R>` into `Atom<Result<A, E>>`, `Result` mirrors `Exit`/`Cause`, `Atom.pull` consumes a `Stream`, `Schema` powers `Atom.serializable`/`Result.Schema`, and `Match`/`Option` fold the `Result` in derivations. Effect owns the async/DI/error rails; atom adds reactive memoization and a store, no new effect primitive.
- `@effect/platform` (`libs/typescript/.api/effect-platform.md`): `AtomHttpApi.Tag` binds an `HttpApi` value into reactive atoms, so the declarative HTTP contract deriving server, client, and OpenAPI also derives the client-state atoms; `Atom.kvs` persists through the platform `KeyValueStore` Tag, honoring the same runtime-portable `Layer` selection.
- `@effect/rpc` (peer): `AtomRpc.Tag` binds an `RpcGroup` into atom-wrapped procedures — the second `edge` contribution family (beside `HttpApiGroup`) becomes reactive under the identical binding law, so a folder codes one `RpcGroup` and gets typed reactive calls.
- `@effect/experimental` (peer): the `withReactivity(keys)` invalidation graph and durable/persisted-atom extras ride this peer — a `.query` request and a `.mutation` call each accept `reactivityKeys`, a mutation's keys invalidate every query atom holding a matching key, and `timeToLive` bounds a query, replacing a stringly cache-key protocol with typed keys.
- `@effect-atom/atom-react` (`libs/typescript/ui/.api/effect-atom-atom-react.md`): the sibling React binding re-exports this whole algebra and adds the hooks that mount atoms into components — every type here (`Atom`, `Writable`, `Result`, `Registry`) is the input to those hooks.
- `state` (kernel folder) + `wire` (`#vocab` subpath): atoms hold kernel-branded domain values decoded once at `wire`; `Atom.searchParam`/`Atom.kvs` sync selected atoms to URL/storage with the kernel `Schema` as the codec, never an ad-hoc serializer.

[LOCAL_ADMISSION]:
- Construct through one `Atom.make` discriminating on argument; reach for `readable`/`writable`/`fn`/`pull` only where the value kind must be pinned. Arity lives in the argument shape — a `makeQuery`/`makeMutation`/`makeAsync` parallel constructor is the foreclosed form.
- Bind server contracts through `AtomHttpApi.Tag`/`AtomRpc.Tag` where the `@effect/platform`/`@effect/rpc` declaration already types the endpoint, and invalidate with `Atom.refresh`/`withReactivity` — a hand-written fetch atom or string-keyed query cache is the foreclosed form.
- Render async state by folding `Result` (`match`/`matchWithWaiting`/`builder`), keeping the `waiting`/`previous` arms visible so refresh never blanks the view — a `.value` read without a guard or a render-path `getOrThrow` is the foreclosed form.
- Build one `AtomRuntime` from the app `Layer` and create effectful atoms through it; provide the `Registry` once at the app root (`Registry.layer`), a per-subtree registry being a test-only isolation — a per-atom `Layer` provision is the foreclosed form.
- Persist through `Atom.kvs`/`Atom.searchParam` with a `Schema`; a `view` row reaching into `localStorage`/`URLSearchParams` is the foreclosed form.

[RAIL_LAW]:
- Package: `@effect-atom/atom`
- Owns: the reactive `Atom`/`Writable` algebra, the `Result<A,E>` async ADT, the `Registry` store and `AtomRuntime` DI seam, the combinator surface (map/optimistic/debounce/kvs/searchParam/refresh-triggers), the `AtomHttpApi`/`AtomRpc` service-binding rows, `AtomRef` fine-grained cursors, and SSR `Hydration`
- Accept: `Effect`/`Stream`/`Layer`/`Schema` values lifted through `make`/`pull`/`runtime`/`serializable`, `@effect/platform` `HttpApi` and `@effect/rpc` `RpcGroup` declarations bound through the `Tag` generators, one app-root `Registry`, `Result` folds in every consumer
- Reject: hand-rolled fetch/query-cache atoms where a platform/rpc declaration binds directly, string cache keys where `withReactivity`/`refresh` invalidate typed atoms, per-atom `Layer` provision, untyped `localStorage`/URL access, blanking a view on refresh instead of folding `waiting`/`previous`, parallel per-arity constructors
