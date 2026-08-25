# [TS_UI_API_TANSTACK_STORE]

`@tanstack/store` owns the reactive cell TanStack's own packages speak: an atom holds a value, `get` links whoever reads it, `set` propagates to every dependent, and a computed atom re-derives lazily on the next read. Zero-dependency, DOM-free, and framework-agnostic, it reaches `ui` only as the vocabulary `@tanstack/react-table`'s `options.atoms` ownership rail demands — `view/table`'s `Grid.edge` mints an atom-shaped adapter over the one `@effect-atom` fold, and nothing here owns state.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the atom family — one readable cell, one writable extension

| [INDEX] | [SYMBOL]                                          | [TYPE_FAMILY] | [CAPABILITY]                             |
| :-----: | :------------------------------------------------ | :------------ | :--------------------------------------- |
|  [01]   | `Atom<T>`                                         | writable cell | `get`, `set`, `subscribe`                |
|  [02]   | `ReadonlyAtom<T>`                                 | derived cell  | the computed atom, no `set`              |
|  [03]   | `BaseAtom<T>` / `AnyAtom` (`BaseAtom<any>`)       | shared base   | the `Readable` + `Subscribable` floor    |
|  [04]   | `AtomOptions<T>` (`{ compare }`)                  | options bag   | the propagation-gate comparator          |
|  [05]   | `InternalBaseAtom<T>` / `InternalReadonlyAtom<T>` | internal      | `_snapshot`/`_update` and `ReactiveNode` |

- [01]-[WRITABLE_CELL]: `Atom<T>` widens `BaseAtom<T>` with an intersected `set` — `(value: T)` and `((prev: T) => T)` are two call signatures, so one parameter typed as either arm satisfies both.
- [02]-[DERIVED_CELL]: `ReadonlyAtom<T>` is structurally `BaseAtom<T>` — the distinction is the absent `set`, and the compiler is the only thing enforcing it.
- [03]-[SHARED_BASE]: `BaseAtom<T>` is `Subscribable<T>` + `Readable<T>`; `AnyAtom` is the erased receiver internal plumbing passes around.
- [04]-[OPTIONS_BAG]: `compare` defaults to `Object.is` and gates propagation — a `false` return marks the atom changed and pushes; `shallow` is the drop-in for object slices.
- [05]-[INTERNAL]: `_snapshot`/`_update` and the `ReactiveNode` link fields are `@internal` graph state; a consumer that reaches for them has left the contract.

[PUBLIC_TYPE_SCOPE]: the subscription protocol — an observer contract shared with the wider reactive world

| [INDEX] | [SYMBOL]                                       | [TYPE_FAMILY]   | [CAPABILITY]                         |
| :-----: | :--------------------------------------------- | :-------------- | :----------------------------------- |
|  [01]   | `Observer<T>` (`{ next?, error?, complete? }`) | observer        | every handler optional               |
|  [02]   | `Subscription` (`{ unsubscribe }`)             | teardown        | the one disposal handle              |
|  [03]   | `Subscribable<T>` / `InteropSubscribable<T>`   | subscribe shape | intersected arms, observer-only arm  |
|  [04]   | `Readable<T>` / `Selection<TSelected>`         | readable        | `Subscribable` plus a `get` snapshot |

- [01]-[OBSERVER]: every handler is optional, so an adapter implementing `subscribe` calls `observer.next?.(value)` rather than assuming a handler exists.
- [02]-[TEARDOWN]: `subscribe` returns `{ unsubscribe }` and nothing else — no `closed` flag, no chaining.
- [03]-[SUBSCRIBE_SHAPE]: `Subscribable.subscribe` is an intersection of two signatures — `(observer)` and `(next, error?, complete?)`; `InteropSubscribable` keeps only the observer arm, the TC39/rxjs-shaped seam.
- [04]-[READABLE]: `Readable<T>` adds the `get(): T` snapshot; `Selection<TSelected>` is its alias for a projected read.

[PUBLIC_TYPE_SCOPE]: the store — a class wrapper over one atom, with an optional action surface

| [INDEX] | [SYMBOL]                           | [TYPE_FAMILY] | [CAPABILITY]                              |
| :-----: | :--------------------------------- | :------------ | :---------------------------------------- |
|  [01]   | `Store<T, TActions>`               | class         | `setState`, `state`, `get`, `subscribe`   |
|  [02]   | `ReadonlyStore<T>`                 | class         | `Omit<Store<T>, 'setState' \| 'actions'>` |
|  [03]   | `StoreAction` / `StoreActionMap`   | action shape  | one action, the named action map          |
|  [04]   | `StoreActionsFactory<T, TActions>` | factory       | `({ setState, get }) => TActions`         |

- [01]-[STORE]: `Store` wraps one `createAtom` cell — `setState` takes the functional updater alone, `state` and `get()` are the same read, and `get`/`setState`/`subscribe` are constructor-bound so a destructured handle still works.
- [02]-[READONLY_STORE]: `ReadonlyStore` is what `createStore` returns for the computed form; it drops `setState` and `actions` and nothing else.
- [03]-[ACTION_SHAPE]: actions are plain functions of any arity keyed by name — no reducer, no dispatch, no message type.
- [04]-[ACTION_FACTORY]: the factory receives the store's own `setState`/`get` pair, so an action closes over the cell without a circular reference to the instance.

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: minting a cell

| [INDEX] | [SURFACE]                                            | [SHAPE]  | [CAPABILITY]                                |
| :-----: | :--------------------------------------------------- | :------- | :------------------------------------------ |
|  [01]   | `createAtom(initialValue, options?) -> Atom<T>`      | overload | the writable cell                           |
|  [02]   | `createAtom((prev?) => T, options?) -> ReadonlyAtom` | overload | the computed cell, deps tracked on read     |
|  [03]   | `createAsyncAtom(() => Promise<T>, options?)`        | fn       | `ReadonlyAtom<AsyncAtomState<T>>`           |
|  [04]   | `createStore(value \| fn, actionsFactory?)`          | overload | `Store` / `ReadonlyStore` / `Store+actions` |

- [01]-[WRITABLE]: the value form is the writable arm — a function argument is never a value here, so a cell holding a function needs the store's `NonFunction` route or a wrapper object.
- [02]-[COMPUTED]: the function form derives; whatever it reads through `.get()` during the call becomes its dependency set, re-linked on each recompute, so a conditional read narrows the set and an unwatched computed atom drops its deps entirely.
- [03]-[ASYNC]: `AsyncAtomState<T>` is the three-arm union `{ status: 'pending' } | { status: 'done', data } | { status: 'error', error }` — the promise fires on first read and the atom pushes each settle.
- [04]-[STORE_BUILD]: `createStore` mirrors `createAtom` — function to `ReadonlyStore`, value to `Store`, value plus factory to `Store<T, TActions>` with `store.actions` populated.

[ENTRYPOINT_SCOPE]: reading, writing, and propagation control

| [INDEX] | [SURFACE]                                    | [SHAPE] | [CAPABILITY]                            |
| :-----: | :------------------------------------------- | :------ | :-------------------------------------- |
|  [01]   | `atom.get()` / `store.get()` / `store.state` | read    | the current snapshot                    |
|  [02]   | `atom.set(value \| (prev) => T)`             | write   | compare, then propagate and flush       |
|  [03]   | `store.setState((prev) => T)`                | write   | the functional updater alone            |
|  [04]   | `atom.subscribe(observer \| next)`           | listen  | returns `Subscription`                  |
|  [05]   | `batch(fn)` / `flush()`                      | control | defer notification, drain the queue     |
|  [06]   | `shallow(objA, objB)` / `toObserver(...)`    | util    | the `compare` companion, the normalizer |

- [01]-[READ]: `get()` doubles as the dependency link — called inside a computed atom or an active subscription it registers the edge, called outside it is a bare snapshot.
- [02]-[WRITE]: `set` runs `compare` first and returns without notifying when the value is unchanged; a real change propagates and flushes synchronously.
- [03]-[STORE_WRITE]: `Store.setState` is typed for the updater function only — pass `() => next` for a plain replacement.
- [04]-[LISTEN]: `subscribe` runs its effect once to register the dependency but does NOT emit the current value; the first `next` is the first change, so an adapter needing initial delivery reads `get()` itself.
- [05]-[CONTROL]: `batch` increments a depth counter and flushes on unwind, so nested batches notify once at the outermost exit; a bare `flush()` no-ops while a batch is open.
- [06]-[UTIL]: `shallow` fast-paths `Object.is`, handles `Map`/`Set`/`Date` by content, then compares own keys and symbols one level deep; `toObserver` normalizes either `subscribe` arm into one `Observer` and binds the handlers to their host.

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- One primitive, two projections: `createAtom` is the whole engine, and `Store`/`ReadonlyStore` are a class facade over a single private atom. Nothing in the package composes atoms into a graph for you — a derived value is a computed `createAtom` reading other atoms, and that is the only combinator.
- Push on write, pull on read: `set` marks dirty, propagates to subscribers, and flushes queued effects synchronously; a computed atom recomputes only when a read finds it dirty. A chain of computed atoms therefore costs nothing until someone reads the tail.
- `compare` is the propagation gate, not a render optimization: returning `true` stops the write at the source, so an unstable object identity re-notifies every dependent unless `shallow` or a domain comparator lands in `AtomOptions`.
- Subscription is change-only and effect-backed: each `subscribe` opens an internal effect that reads the atom to link it, suppresses the first emission, and tears the link down on `unsubscribe`. There is no replay, no initial value, and no `closed` handle.
- Structural typing is the whole contract: `Atom<T>` is an interface, not a class or a branded nominal type, so any object with `get`/`set`/`subscribe` of the right shape IS an atom to every consumer — which is exactly what makes an adapter over a foreign reactive cell legal.

[STACKING]:
- `@tanstack/react-table` (`.api/tanstack-react-table.md`): the sole reason this package is admitted. Every registered state slice rides an atom — `table.baseAtoms.<slice>` is the internal `Atom`, `table.atoms.<slice>` the derived `ReadonlyAtom`, `table.store` a `ReadonlyStore<TableState>`, and `table.optionsStore` an `Atom<TableOptions>`. `options.atoms[slice]` types as `Atom<TableState[slice]>` and takes ownership of that slice outright: `makeStateUpdater` resolves `options.atoms?.[key] ?? baseAtoms[key]` and calls `.set` on whichever it finds, so an outside atom receives table writes with no callback glue and survives `table.reset()`. `SubscribeSource` accepts all four shapes — `Atom`, `ReadonlyAtom`, `Store`, `ReadonlyStore`.
- `@effect-atom/atom` + `@effect-atom/atom-react` (`.api/effect-atom-atom-react.md`): the branch's actual state owner. `view/table`'s `Grid.edge` builds an `Atom<Grid.Slice[K]>` by hand over the one fold — `get` reads `registry.get(fold)[key]`, `set` folds an `Updater` back through `registry.update`, and `subscribe` wraps `registry.subscribe`, dispatching on the `Observer | next` union to satisfy the intersected signature. Structural typing makes this adapter a first-class atom to the table, so the fold stays the single writer and this package contributes vocabulary alone.
- `@tanstack/react-store` (installed transitively under `@tanstack/react-table`): the React adapter — `useStore`/`useSelector`/`useAtom` over `use-sync-external-store`, re-exporting this package whole. `ui` reads table state through `table.state` and `table.Subscribe`, which already ride those hooks, so the adapter is never imported directly.

[LOCAL_ADMISSION]:
- Compose this package ONLY as the `Grid.edge` adapter seam that hands one `@effect-atom` fold slice to `options.atoms` (`view/table` `#STATE_FOLD`); a store-native state owner minted beside the atom registry is the rejected shape and a second writer over the same fact.
- Implement the adapter against the published interface — the intersected `set` takes one `Updater`-typed parameter, and `subscribe` must dispatch on the `Observer | next` union and return `{ unsubscribe }`; never reach for `_snapshot`, `_update`, or the `ReactiveNode` link fields.
- Read the fold through the registry inside `get`, never a cached snapshot field: the table calls `get()` on every derivation, and a stale mirror is a torn read no comparator can catch.
- Let `@effect-atom` own batching and equality for anything the fold holds; reach for `batch`/`flush`/`compare`/`shallow` only inside an adapter whose peer is a genuine `@tanstack/store` cell.
- Never call `createAtom`, `createAsyncAtom`, or `createStore` in `ui` — async work is an `Effect`, derived state is a derived `@effect-atom`, and a bare cell here is a registry the fold cannot see.
