# [TS_UI_API_EFFECT_ATOM_ATOM]

`@effect-atom/atom` is the framework-neutral reactive layer that turns an `Effect`/`Stream`/`Layer` graph into memoized, subscribable `Atom` nodes — the single `ONE_FOLD_ONE_BINDING` store the `ui` folder composes for all client state. An `Atom<A>` is a lazily-computed, dependency-tracked cell; a `Writable<R, W>` also accepts writes; async atoms hold a `Result<A, E>` (`Initial | Success | Failure`, each carrying a `waiting` flag and a `previous` value) that mirrors an Effect `Exit` so a pending refresh never blanks the last-good value. The `Registry` is the store that owns every node's lifecycle (mount/subscribe/refresh/dispose, idle-TTL GC); `AtomRuntime` shares one `Layer`-built dependency graph across atoms. `AtomHttpApi.Tag`/`AtomRpc.Tag` are the direct-binding rows: a `@effect/platform` `HttpApi` declaration or an `@effect/rpc` `RpcGroup` becomes a `Context.Tag` whose members are atom-wrapped `Result`s, so the typed client contract is reactive with zero data-fetching glue. This package carries no React dependency — `@effect-atom/atom-react` mounts these atoms into the component spine.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@effect-atom/atom`
- package: `@effect-atom/atom` (MIT)
- module format: ESM + CJS dual (`dist/esm`, `dist/cjs`, `.d.ts` under `dist/dts`), `sideEffects: []` (fully tree-shakeable); per-namespace deep-import subpaths `@effect-atom/atom/{Atom,Result,Registry,AtomRef,AtomHttpApi,AtomRpc,Hydration}`
- runtime target: framework-neutral — runs in any JS runtime; the reactive graph is pure, only the `Registry` is stateful. React is not a peer here; `atom-react` adds the hooks
- peer: `effect@^catalog`, `@effect/platform@^catalog`, `@effect/rpc@^catalog`, `@effect/experimental@^catalog`
- asset: pure-TypeScript reactive-state library — the `Atom`/`Result`/`Registry` algebra plus `Context.Tag` client generators over platform/rpc contracts
- rail: state binding (the one `ONE_FOLD_ONE_BINDING` store; `atom/binding` + `atom/derive` own it, every `view` row reads it)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: atom algebra core — the reactive cell and its read/write context
- rail: state binding

| [INDEX] | [SYMBOL]                                                                                                  | [TYPE_FAMILY]    | [CONSUMER]                                                                                                    |
| :-----: | :-------------------------------------------------------------------------------------------------------- | :--------------- | :------------------------------------------------------------------------------------------------------------ |
|  [01]   | `Atom<A>` (extends `Pipeable`, `Inspectable`)                                                             | reactive cell    | `atom/binding` — every state node; `.pipe`-composable, structurally memoized                                  |
|  [02]   | `Writable<R, W = R>`                                                                                      | writable cell    | `atom/binding` — read type `R`, write type `W`; the only atom `useAtom`/`set` accept                          |
|  [03]   | `Context` (`get`/`result`/`once`/`mount`/`subscribe`/`stream`/`self`/`setSelf`/`addFinalizer`/`registry`) | read context     | `atom/derive` — the `get` argument of a derived atom; reads track dependencies, `addFinalizer` scopes cleanup |
|  [04]   | `WriteContext<A>` (`get`/`set`/`setSelf`/`refreshSelf`)                                                   | write context    | `atom/binding` — the write callback argument of `writable`; commits derived writes                            |
|  [05]   | `FnContext`                                                                                               | fn read context  | `atom/derive` — the `get`-plus-args context of a `fn`/`fnSync` atom                                           |
|  [06]   | `Type<T>` / `Success<T>` / `Failure<T>` / `PullSuccess<T>` / `WithoutSerializable<T>`                     | type projections | design-page generics — extract the value/success/error type of an atom without a runtime cost                 |
|  [07]   | `isAtom` / `isWritable` / `isSerializable`                                                                | guards           | boundary narrowing before `set`/serialization                                                                 |

[PUBLIC_TYPE_SCOPE]: async result ADT — the value every effect-backed atom holds
- rail: state binding

| [INDEX] | [SYMBOL]                                                                          | [TYPE_FAMILY]        | [CONSUMER]                                                                                                          |
| :-----: | :-------------------------------------------------------------------------------- | :------------------- | :------------------------------------------------------------------------------------------------------------------ |
|  [01]   | `Result<A, E>` = `Initial<A,E> \| Success<A,E> \| Failure<A,E>`                   | async union          | `view` rows fold this in render; every `AtomHttpApi`/`AtomRpc`/`pull` atom yields it                                |
|  [02]   | `Initial<A,E>` / `Success<A,E>` / `Failure<A,E>`                                  | union arms           | `Result.match` arms; each carries `waiting: boolean` and `previous: Option<Result>` so refresh keeps last-good data |
|  [03]   | `AtomResultFn<Arg, A, E>` (`Writable<Result<A,E>, Arg \| Reset \| Interrupt>`)    | callable-result atom | `atom/derive` — a parameterized async atom written with an `Arg`, `Reset`, or `Interrupt`                           |
|  [04]   | `Reset` / `Interrupt` (symbol + type)                                             | write commands       | write these to an `AtomResultFn` to clear to `Initial` or cancel the running fiber                                  |
|  [05]   | `PullResult<A, E>`                                                                | paged result         | `atom/derive` — the `Result` of a `pull` atom over a `Stream`, with `{ done, items }`                               |
|  [06]   | `Result.Schema<Success, Error>` / `PartialEncoded` / `Encoded` / `schemaFromSelf` | wire schema          | `wire`-typed atoms — encode/decode a `Result` across the SSR/persistence boundary                                   |
|  [07]   | `Result.Builder<Out, A, E, I>`                                                    | fold builder         | `view` render — pipeable exhaustive `Result` fold with fluent case arms                                             |

[PUBLIC_TYPE_SCOPE]: runtime, registry, and fine-grained refs
- rail: state binding

| [INDEX] | [SYMBOL]                                                                                                                 | [TYPE_FAMILY]        | [CONSUMER]                                                                                                                                                                         |
| :-----: | :----------------------------------------------------------------------------------------------------------------------- | :------------------- | :--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `AtomRuntime<R, ER>` / `RuntimeFactory`                                                                                  | layer-backed runtime | `atom/binding` — one `Layer`-built dependency graph shared by every atom built from it                                                                                             |
|  [02]   | `Registry` (`get`/`set`/`modify`/`update`/`mount`/`subscribe`/`refresh`/`setSerializable`/`reset`/`dispose`/`getNodes`)  | store                | `atom/binding` — the imperative store surface; `browser`/tests read/drive it directly                                                                                              |
|  [03]   | `AtomRegistry` (`Context.TagClass`)                                                                                      | registry Tag         | the Effect `Context.Tag` for the ambient registry; `AtomRuntime` and effectful accessors require it                                                                                |
|  [04]   | `AtomRef<A>` / `ReadonlyRef<A>` (extends `Equal`) / `Collection<A>`                                                      | mutable ref          | `atom/derive` — fine-grained cursor into a sub-value (form field, list item) without re-running the owning atom                                                                    |
|  [05]   | `Serializable<S extends Schema.Schema.Any>`                                                                              | serializable marker  | SSR/persistence — an atom carrying a `Schema` so its value crosses the wire or `KeyValueStore`                                                                                     |
|  [06]   | `AtomHttpApiClient<Self, Id, Groups, ApiE, E>` (extends `Context.Tag`; members `.layer`/`.runtime`/`.query`/`.mutation`) | api-client Tag       | `atom/binding` — a `@effect/platform` `HttpApi` bound so `.query(group, endpoint, request)` is a read `Atom<Result>` and `.mutation(group, endpoint)` is a callable `AtomResultFn` |
|  [07]   | `AtomRpcClient<Self, Id, Rpcs, E>` (extends `Context.Tag`; members `.layer`/`.runtime`/`.query`/`.mutation`)             | rpc-client Tag       | `atom/binding` — an `@effect/rpc` `RpcGroup` bound; `.query(tag, payload)` yields `Atom<Result>` (or a `PullResult` atom for a streaming rpc), `.mutation(tag)` an `AtomResultFn`  |
|  [08]   | `DehydratedAtom` / `DehydratedAtomValue`                                                                                 | SSR snapshot         | `Hydration` — the serialized registry state a server emits and a client rehydrates                                                                                                 |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: constructing atoms — the one polymorphic `make` plus its specialized rows
- rail: state binding

| [INDEX] | [SURFACE]                                                                   | [ENTRY_FAMILY]  | [CONSUMER]                                                                                                                                           |
| :-----: | :-------------------------------------------------------------------------- | :-------------- | :--------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `Atom.make(effect \| stream \| value \| (get) => …)`                        | universal ctor  | `atom/binding` — the one entry: lifts an `Effect<A,E,R>`/`Stream` to `Atom<Result<A,E>>`, a plain value to `Writable`, a read fn to a derived `Atom` |
|  [02]   | `Atom.readable(read, refresh?)` / `Atom.writable(read, write, refresh?)`    | explicit ctor   | `atom/derive` — hand-built read-only or read/write atoms when `make`'s discrimination is too loose                                                   |
|  [03]   | `Atom.fn(create, options?)` / `Atom.fnSync(f)`                              | callable atom   | `atom/derive` — an `AtomResultFn`: call with an `Arg` to (re)run an effectful/sync computation; the write is the argument                            |
|  [04]   | `Atom.pull(streamOrCreate, options?)`                                       | paged stream    | `atom/derive` — a `PullResult` atom over a `Stream`; write to advance the page                                                                       |
|  [05]   | `Atom.subscriptionRef(…)` / `Atom.subscribable(…)`                          | effect-ref atom | bridge an Effect `SubscriptionRef`/`Subscribable` into a writable atom                                                                               |
|  [06]   | `Atom.family((arg) => atom)`                                                | keyed factory   | `atom/derive` — memoize one atom per argument key (per-entity atoms) without a leak                                                                  |
|  [07]   | `Atom.runtime(layer)` / `Atom.context({ memoMap })` / `Atom.defaultMemoMap` | runtime root    | `atom/binding` — build an `AtomRuntime` from a `Layer`; atoms made through it share the dependency graph                                             |

[ENTRYPOINT_SCOPE]: combinators — deriving, mapping, and tuning atoms in place
- rail: state binding

| [INDEX] | [SURFACE]                                                                                   | [ENTRY_FAMILY]  | [CONSUMER]                                                                                                                                       |
| :-----: | :------------------------------------------------------------------------------------------ | :-------------- | :----------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `Atom.map(f)` / `Atom.mapResult(f)` / `Atom.transform(f)`                                   | projection      | `atom/derive` — the selector rail; `mapResult` maps the `Success` arm only, `transform` rebuilds via `get`                                       |
|  [02]   | `Atom.debounce(ms)` / `Atom.withReactivity(keys)`                                           | update policy   | `atom/derive` — rate-limit writes; declare reactivity keys the `@effect/experimental` invalidation graph refreshes on                            |
|  [03]   | `Atom.keepAlive` / `Atom.autoDispose` / `Atom.setLazy(bool)` / `Atom.setIdleTTL(duration)`  | lifecycle       | `atom/binding` — pin a hot atom, or GC an idle one after a TTL; the registry honors these per node                                               |
|  [04]   | `Atom.withFallback(fallbackAtom)` / `Atom.initialValue(value)` / `Atom.withLabel(name)`     | seed / debug    | `atom/derive` — seed a `Result` atom's first render; label for devtools/inspection                                                               |
|  [05]   | `Atom.optimistic(self)` / `Atom.optimisticFn({ …, reducer })`                               | optimistic UI   | `atom/binding` — write an optimistic value immediately, reconcile against the effect's real `Result`                                             |
|  [06]   | `Atom.refreshOnWindowFocus` / `Atom.makeRefreshOnSignal(signal)` / `Atom.windowFocusSignal` | refresh trigger | `atom/binding` — re-run an atom on window focus or any signal atom; the stale-while-focus row                                                    |
|  [07]   | `Atom.kvs({ runtime, key, schema, defaultValue })` / `Atom.searchParam(name, { schema? })`  | persistence     | `atom/binding` — back an atom by a `KeyValueStore`-runtime + `Schema` (localStorage/IndexedDB), or bind it to a URL search param, `Schema`-typed |

[ENTRYPOINT_SCOPE]: `Result` folds — rendering async state exhaustively
- rail: state binding

| [INDEX] | [SURFACE]                                                                                                 | [ENTRY_FAMILY]  | [CONSUMER]                                                                                                         |
| :-----: | :-------------------------------------------------------------------------------------------------------- | :-------------- | :----------------------------------------------------------------------------------------------------------------- |
|  [01]   | `Result.match(self, { onInitial, onSuccess, onFailure })` / `.matchWithWaiting(…)` / `.matchWithError(…)` | exhaustive fold | `view` render — the total dispatch over `Result`; `matchWithWaiting` folds the `waiting` flag into a dedicated arm |
|  [02]   | `Result.builder(self).onInitial(…).onSuccess(…).orNull()`                                                 | fluent fold     | `view` render — pipeable case-by-case fold when arms are added incrementally                                       |
|  [03]   | `Result.value` / `.getOrElse` / `.getOrThrow` / `.error` / `.cause`                                       | accessor        | `view` — pull the `Option<A>`/`Option<E>` out of a `Result` for a quick read                                       |
|  [04]   | `Result.map(f)` / `.flatMap(f)` / `.all(results)` / `.toExit(self)`                                       | combinator      | `atom/derive` — transform a `Result` value, join several, or convert back to an Effect `Exit`                      |
|  [05]   | `Result.success` / `.failure` / `.fail` / `.initial` / `.waiting` / `.fromExit`                           | constructor     | tests + `atom/derive` — build a `Result` directly; `waiting`/`replacePrevious` preserve the previous value         |
|  [06]   | `Result.isSuccess` / `.isFailure` / `.isInitial` / `.isWaiting` / `.isInterrupted`                        | guard           | `view` — narrow before an accessor; `isInterrupted` distinguishes a cancelled fiber from a real failure            |

[ENTRYPOINT_SCOPE]: registry and effectful accessors — driving atoms from Effect/imperative code
- rail: state binding

| [INDEX] | [SURFACE]                                                                                           | [ENTRY_FAMILY]     | [CONSUMER]                                                                                                    |
| :-----: | :-------------------------------------------------------------------------------------------------- | :----------------- | :------------------------------------------------------------------------------------------------------------ |
|  [01]   | `Registry.make({ initialValues?, scheduleTask?, timeoutResolution?, defaultIdleTTL? })`             | store ctor         | `browser`/app root — the one registry per app; tests build an isolated one per case                           |
|  [02]   | `Registry.layer` / `Registry.layerOptions(opts)` / `Registry.AtomRegistry`                          | registry Layer     | `atom/binding` — provide the registry as an Effect service so effectful atoms resolve `AtomRegistry`          |
|  [03]   | `registry.get(atom)` / `.set(atom, w)` / `.modify(atom, f)` / `.update(atom, f)` / `.refresh(atom)` | imperative rw      | `browser`/panel — read or drive an atom outside React; `modify` returns a value and the next state atomically |
|  [04]   | `registry.mount(atom)` / `.subscribe(atom, f, { immediate? })` / `.reset()` / `.dispose()`          | lifecycle          | non-React subscribers; `mount` keeps a node hot, `subscribe` returns an unsubscribe fn                        |
|  [05]   | `Atom.get(self)` / `.set(self, w)` / `.modify` / `.update` / `.refresh` / `.getResult(self)`        | effectful accessor | `atom/binding` — read/write an atom from inside `Effect.gen`; each returns `Effect<…, AtomRegistry>`          |
|  [06]   | `Atom.toStream(self)` / `.toStreamResult(self)` / `Registry.toStream` / `Registry.getResult`        | stream bridge      | `wire` — observe an atom as an Effect `Stream`; feed atom changes into a pipeline                             |
|  [07]   | `Atom.batch(f)`                                                                                     | batch commit       | `atom/binding` — coalesce many writes into one notification pass                                              |

[ENTRYPOINT_SCOPE]: effect-service direct binding and SSR — the `AtomHttpApi`/`AtomRpc` rows
- rail: boundaries

| [INDEX] | [SURFACE]                                                                                                                     | [ENTRY_FAMILY]   | [CONSUMER]                                                                                                                                                                                                                   |
| :-----: | :---------------------------------------------------------------------------------------------------------------------------- | :--------------- | :--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `class Api extends AtomHttpApi.Tag<Api>()(id, { api, httpClient, baseUrl?, transformClient?, transformResponse?, runtime? })` | http binding     | `atom/binding` — bind a `@effect/platform` `HttpApi`; `Api.query(group, endpoint, { …request, reactivityKeys?, timeToLive? })` / `Api.mutation(group, endpoint)` yield read/callable atoms sharing `Api.runtime`/`Api.layer` |
|  [02]   | `class Rpc extends AtomRpc.Tag<Rpc>()(id, { group, protocol, spanPrefix?, disableTracing?, runtime? })`                       | rpc binding      | `atom/binding` — bind an `@effect/rpc` `RpcGroup`; `Rpc.query(tag, payload, { reactivityKeys?, timeToLive? })` / `Rpc.mutation(tag)` yield atoms; a streaming rpc's `.query` is a `PullResult` atom                          |
|  [03]   | `AtomRef.make(value)` / `AtomRef.collection(items)`                                                                           | fine-grained ref | `atom/derive` — a mutable cursor / an ordered collection of refs for per-item subscriptions (undo/redo cursor)                                                                                                               |
|  [04]   | `Atom.serializable(self, { schema })` / `Atom.withServerValue(self, …)` / `Atom.getServerValue`                               | serializable     | SSR/persistence — mark an atom's value `Schema`-typed and carry a server-computed initial value                                                                                                                              |
|  [05]   | `Hydration.dehydrate(registry, options?)` / `.toValues(state)` / `.hydrate(registry, state)`                                  | SSR handoff      | server dehydrates the registry to `DehydratedAtom[]`; the client rehydrates into a fresh registry                                                                                                                            |

## [04]-[IMPLEMENTATION_LAW]

[ATOM_TOPOLOGY]:
- An `Atom<A>` is pure and lazy: it computes only when a `Registry` mounts it, and recomputes only when a tracked dependency (another atom read through the `Context`) changes. The `Registry` is the sole stateful owner — mount/subscribe/refresh/dispose, idle-TTL GC, and write scheduling all live there, so an atom graph is trivially testable against an isolated `Registry.make()`.
- Async is a value, not a control path: an effect-backed atom holds `Result<A, E>`, and every arm carries `waiting` (a refresh is in flight) and `previous` (the last non-initial value). A re-fetch flips `waiting` true while keeping the old `Success` visible — the stale-while-revalidate behavior is structural, never a hand-written flag. `Result` maps to Effect `Exit` through `fromExit`/`toExit`, so the error channel is the same tagged `Cause` the rest of the branch uses.
- `AtomRuntime` is the DI seam: `Atom.runtime(layer)` memoizes one `Layer` build (`defaultMemoMap`) and every atom created through it resolves its services from that shared graph — the app provides one runtime, and `wire`/`edge` clients, config, and telemetry are shared across all client state without re-provisioning.
- `AtomHttpApi.Tag`/`AtomRpc.Tag` invert the usual data-fetching layer: the `@effect/platform` `HttpApi` or `@effect/rpc` `RpcGroup` declaration is the single source, and the generated `Context.Tag` exposes each endpoint/procedure as an atom-returning method. There is no query-key registry, no request cache to invalidate by string — invalidation is `Atom.refresh`/`withReactivity` on the typed atom, and the response type is the endpoint's `Schema` success type with no restatement.
- One polymorphic constructor owns creation: `Atom.make` discriminates on its argument (effect, stream, plain value, read function) into the right atom kind; `readable`/`writable`/`fn`/`pull`/`subscriptionRef` are the explicit rows for when discrimination must be pinned. New reactive behavior is a combinator on the owning atom (`optimistic`, `debounce`, `kvs`, `searchParam`), never a parallel atom type.

[STACKS_WITH]:
- `effect` (`libs/typescript/.api/effect.md`): the atom IS the Effect-to-reactive adapter — `Atom.make(effect)` lifts `Effect<A, E, R>` into `Atom<Result<A, E>>`, `Result` mirrors `Exit`/`Cause`, `Atom.pull` consumes a `Stream`, `Schema` powers `Atom.serializable`/`Result.Schema`, and `Match`/`Option` fold the `Result` in derivations. Effect owns the async/DI/error rails; atom adds only reactive memoization and a store — no new effect primitive.
- `@effect/platform` (`libs/typescript/.api/effect-platform.md`): `AtomHttpApi.Tag` binds an `HttpApi` value into reactive atoms, so the declarative HTTP contract (one source deriving server, client, and OpenAPI) also derives the client-state atoms; `Atom.kvs` persists through the platform `KeyValueStore` Tag, honoring the same runtime-portable `Layer` selection.
- `@effect/rpc` (peer): `AtomRpc.Tag` binds an `RpcGroup` into atom-wrapped procedures — the second `edge` contribution family (beside `HttpApiGroup`) becomes reactive with the identical binding law, so a folder codes one `RpcGroup` and gets typed reactive calls for free.
- `@effect/experimental` (peer): the `withReactivity(keys)` invalidation graph and durable/persisted-atom extras ride this peer — a `.query` request and a `.mutation` call each accept `reactivityKeys`, and a mutation's keys invalidate every query atom holding a matching key, replacing a stringly cache-key protocol with typed keys and a `timeToLive` per query.
- `@effect-atom/atom-react` (`libs/typescript/ui/.api/effect-atom-atom-react.md`): the sibling React binding; it re-exports this entire algebra and adds only the hooks that mount atoms into components — every type here (`Atom`, `Writable`, `Result`, `Registry`) is the input to those hooks.
- `state` (kernel folder) + `wire` (`#vocab` subpath): atoms hold kernel-branded domain values decoded once at `wire`; `Atom.searchParam`/`Atom.kvs` sync selected atoms to URL/storage with the kernel `Schema` as the codec, never an ad-hoc serializer.

[LOCAL_ADMISSION]:
- Use one `Atom.make` and discriminate on argument; reach for `readable`/`writable`/`fn`/`pull` only when the value kind must be pinned. Never spawn `makeQuery`/`makeMutation`/`makeAsync` parallel constructors — arity lives in the argument shape.
- Bind server contracts through `AtomHttpApi.Tag`/`AtomRpc.Tag`; never hand-write a fetch atom or a query-key cache when the `@effect/platform`/`@effect/rpc` declaration already types the endpoint. Invalidate with `Atom.refresh`/`withReactivity`, never a string key.
- Render async state by folding `Result` (`match`/`matchWithWaiting`/`builder`); never read `.value` without a guard or throw with `getOrThrow` in a render path. Keep the `waiting`/`previous` arms visible so refresh does not blank the view.
- Build one `AtomRuntime` from the app `Layer` and create effectful atoms through it; never provide a `Layer` per atom. Provide the `Registry` once at the app root (`Registry.layer`) — a per-subtree registry is a test-only isolation, not a composition pattern.
- Persist through `Atom.kvs`/`Atom.searchParam` with a `Schema`; never reach into `localStorage`/`URLSearchParams` from a `view` row.

[RAIL_LAW]:
- Package: `@effect-atom/atom`
- Owns: the reactive `Atom`/`Writable` algebra, the `Result<A,E>` async ADT, the `Registry` store + `AtomRuntime` DI seam, the combinator surface (map/optimistic/debounce/kvs/searchParam/refresh-triggers), the `AtomHttpApi`/`AtomRpc` service-binding rows, `AtomRef` fine-grained cursors, and SSR `Hydration`
- Accept: `Effect`/`Stream`/`Layer`/`Schema` values lifted through `make`/`pull`/`runtime`/`serializable`, `@effect/platform` `HttpApi` and `@effect/rpc` `RpcGroup` declarations bound through the `Tag` generators, one app-root `Registry`, `Result` folds in every consumer
- Reject: hand-rolled fetch/query-cache atoms where a platform/rpc declaration binds directly, string cache keys where `withReactivity`/`refresh` invalidate typed atoms, per-atom `Layer` provision, untyped `localStorage`/URL access, blanking a view on refresh instead of folding `waiting`/`previous`, parallel per-arity constructors
