# [UI_ATOM]

The ONE_FOLD_ONE_BINDING law made code: `@effect-atom` is the single state binding of the folder, and this module owns the whole bridge — one `Store.make` standing the app's Layer graph behind the atom registry with a shared `MemoMap`, one registry policy row, the persisted-atom rows (`Atom.kvs`/`Atom.searchParam` with kernel `Schema` codecs), the settled `AtomHttpApi`/`AtomRpc` contract-binding rows, the derivation plane (selectors, `family`, `debounce`, the live `Subscribable`/`SubscriptionRef` bridge, paged `pull`, stream egress), the write-modality and async-fold laws every view row obeys, and the `History` undo/redo command fold. Components are projection surfaces: they reach the Effect graph only through this bridge — never running effects, never owning Layers, never holding a second copy of domain state in `useState`; derived state is computed, never mirrored. The module is `ui/src/system/atom.ts`.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]        | [OWNS]                                                                               | [PUBLIC]  |
| :-----: | :--------------- | :------------------------------------------------------------------------------------- | :-------- |
|  [01]   | `STORE_ROOT`     | `Store.make` — the runtime root, registry policy, shared `MemoMap`, persistence rows    | `Store`   |
|  [02]   | `REMOTE_BINDING` | the `AtomHttpApi`/`AtomRpc` contract-binding rows                                       | —         |
|  [03]   | `SELECTOR_RAIL`  | projection law — `map`/`mapResult`/`transform`, `family`, `debounce`, reactivity keys   | —         |
|  [04]   | `LIVE_BRIDGE`    | host-fold ingress (`subscriptionRef`/`subscribable`), paged `pull`, stream egress       | —         |
|  [05]   | `WRITE_AND_FOLD` | write modality, optimistic reconcile, refresh triggers, the Suspense/boundary rail      | —         |
|  [06]   | `HISTORY_FOLD`   | the `History` owner — command-vocabulary undo/redo stack over any value atom            | `History` |

## [2]-[STORE_ROOT]

[STORE_ROOT]:
- Owner: `Store` — one assembled owner: `make({ layer, memoMap? })` builds the `AtomRuntime` through `Atom.context({ memoMap })` so runtime atoms and the host `ManagedRuntime` share one construction of every Layer node; `policy` is the registry row (`defaultIdleTTL`, `timeoutResolution`) the app's `RegistryProvider` spreads.
- Packages: `@effect-atom/atom-react` (the barrel — `Atom`, `Registry`, `Result`, `Hydration` and the hook surface all reach the folder through it); `effect` (`Layer`, `Duration`).
- Entry: `Store.make` is the one runtime mint; a per-atom `Layer` provision, a second registry outside test isolation, or a module-level `Atom.runtime` call beside it is the named defect.
- Law: one `RegistryProvider` at the app root supplies the store; scoped per-instance state covers component-local cells — a global atom keyed by component id never exists.
- Law: persistence is Schema-coded and the package surface IS the row, no wrapper — `Atom.kvs({ runtime, key, schema, defaultValue })` backs an atom by the platform `KeyValueStore` and `Atom.searchParam(name, { schema })` links one to a URL search param, each with the owning kernel schema (a brand, a `Schema.Literal` vocabulary) as the only codec, so `localStorage`/IndexedDB is never touched raw and a malformed stored value re-decodes to the default instead of poisoning the store.
- Law: SSR handoff rides `Hydration` — the server dehydrates the registry, `HydrationBoundary` rehydrates before children read, and a client refetch of server-computed data is the named defect; `Atom.serializable` marks the atoms that cross.
- Boundary: the `ManagedRuntime` and boot seam are the browser composition root's — this module never calls a `run*` method; the shared `memoMap` argument is how the app hands both runtimes one acquisition map at composition.
- Growth: a new registry knob is one field on `policy`; a persisted atom is one `Atom.kvs`/`Atom.searchParam` call with its owning schema — the `KeyValueStore` Layer swap is app composition.

```typescript
import { Atom } from "@effect-atom/atom-react"
import { Duration, type Layer } from "effect"

const _policy = {
  defaultIdleTTL: Duration.minutes(5),
  timeoutResolution: Duration.millis(100),
} as const

declare namespace Store {
  type Options<R, E> = {
    readonly layer: Layer.Layer<R, E>
    readonly memoMap?: Layer.MemoMap
  }
}

const Store: {
  readonly policy: typeof _policy
  readonly make: <R, E>(options: Store.Options<R, E>) => Atom.AtomRuntime<R, E>
} = {
  policy: _policy,
  make: (options) =>
    options.memoMap === undefined
      ? Atom.runtime(options.layer)
      : Atom.context({ memoMap: options.memoMap })(options.layer),
}
```

## [3]-[REMOTE_BINDING]

[REMOTE_BINDING]:
- Owner: the contract-binding rows — an app declares `class Api extends AtomHttpApi.Tag<Api>()(id, { api, httpClient, baseUrl })` over its `@effect/platform` `HttpApi` value and `class Rpc extends AtomRpc.Tag<Rpc>()(id, { group, protocol })` over its `@effect/rpc` `RpcGroup`; each endpoint then IS a reactive atom (`.query(group, endpoint, request)` a read `Atom<Result>`, `.mutation(group, endpoint)` a callable `AtomResultFn`) with no query-key registry, no request cache, and no fetch glue. Invalidation is typed: `reactivityKeys` on queries and mutations join the invalidation graph, and `timeToLive` ages a query per row.
- Law: the contract is the single source — the fence's shape is the app-side declaration this lib legislates, and a hand-written fetch atom, a string cache key, or a data-fetching library beside the binding is the named defect.
- Law: a streaming rpc's `.query` is a `PullResult` atom — write to advance the page; the pull geometry stays inside the atom, never a hand-rolled cursor cell.
- Boundary: the `HttpApi`/`RpcGroup` values are edge contract material the app supplies, so the binding class is an APP-SIDE declaration this page legislates the exact shape of — the fence below is that shape, not a member of this module's export surface.

```typescript
import { AtomHttpApi } from "@effect-atom/atom-react"
import type { HttpApi } from "@effect/platform"
import { FetchHttpClient } from "@effect/platform"

declare const _contract: HttpApi.HttpApi<never, never>

class Api extends AtomHttpApi.Tag<Api>()("app/Api", {
  api: _contract,
  httpClient: FetchHttpClient.layer,
  baseUrl: "<origin>",
}) {}
```

## [4]-[SELECTOR_RAIL]

[SELECTOR_RAIL]:
- Law: a projection is a derived atom or a hook selector, decided by reach — cross-component projections are `Atom.map(atom, f)` (memoized once in the registry, shared by every reader); component-local slices are the `useAtomValue(atom, selector)` overload (subscription scoped to the slice); the same projection existing as both is a duplicate fold.
- Law: `Atom.mapResult` projects the `Success` arm only, preserving `waiting`/`previous` so derived async state inherits stale-while-revalidate; `Atom.transform` rebuilds through `get` when a derivation reads several atoms — dependency tracking stays structural, never a hand-wired subscription.
- Law: per-entity atoms are `Atom.family((key) => atom)` — one memoized atom per key with no leak (the registry's idle TTL governs); a `Map` of atoms or an atom-of-`Map` re-derives what `family` owns. Keys are kernel brands or `Data`-constructed values so family identity is structural.
- Law: update shaping is a combinator on the owner — `Atom.debounce(ms)` rate-limits a hot derivation (search input feeding a filter), `Atom.withReactivity(keys)` re-runs on typed invalidation coordinates, `Atom.keepAlive`/`Atom.setIdleTTL` pin or age a node; shaping never lives in an effect body.

```typescript
import { ContentKey } from "@rasm/ts/core"
import { Array, Duration, Number } from "effect"

declare const _rows: Atom.Atom<ReadonlyArray<{ readonly key: ContentKey; readonly rank: number }>>

const _byKey = Atom.family((key: ContentKey) =>
  Atom.map(_rows, (rows) => Array.findFirst(rows, (row) => row.key === key)))

const _crest = Atom.map(_rows, (rows) => Array.reduce(rows, 0, (peak, row) => Number.max(peak, row.rank)))

const _query = Atom.make("").pipe(Atom.debounce(Duration.millis(150)))
```

## [5]-[LIVE_BRIDGE]

[LIVE_BRIDGE]:
- Law: a host or state fold enters the view plane as an atom, never as a hand subscription — `Atom.subscriptionRef(ref)` binds a `SubscriptionRef` writable, `Atom.subscribable(sub)` binds the read-only `Subscribable` projection, and the component reads through `useAtomValue` like any other node; a `useSyncExternalStore` call outside the atom binding is the named defect.
- Law: the browser host planes bind through exactly these rows — the router's `location`/`pending` subscribables, the service-worker phase cell, the install stance, the navigation guard, and the session-vault status all publish `Subscribable`/`SubscriptionRef` surfaces, and each enters the component tree as one `Atom.subscribable`/`Atom.subscriptionRef` binding at app composition; a component reading a host service directly restates the bridge.
- Law: a paged stream is `Atom.pull(stream)` — the atom holds `PullResult` (`{ done, items }` folded into `Result`), a write advances the page, and the pull geometry never leaks as a cursor cell beside the atom.
- Law: egress mirrors ingress — `Atom.toStream(atom)`/`Atom.toStreamResult(atom)` observe an atom as an Effect `Stream` where a pipeline (a wire egress, a probe fold) consumes view-plane state; `Atom.batch(f)` coalesces multi-atom writes into one notification pass at imperative seams.
- Law: effectful reads from Effect code go through the accessor family — `Atom.get`/`Atom.set`/`Atom.refresh` return `Effect<_, _, AtomRegistry>` and resolve the ambient registry; a captured registry reference threaded by hand restates the Tag.
- Boundary: `Stream` pipeline law is settled; which host folds exist is the owning runtime page's; this cluster owns only the crossing.

```typescript
import { Result } from "@effect-atom/atom-react"
import type { Stream, SubscriptionRef } from "effect"

declare const _live: SubscriptionRef.SubscriptionRef<ReadonlyArray<string>>
declare const _feed: Stream.Stream<{ readonly at: number }, { readonly _tag: "FeedFault" }>

const _labels = Atom.subscriptionRef(_live)

const _page = Atom.pull(_feed)

const _drained = Atom.toStreamResult(_page)
```

## [6]-[WRITE_AND_FOLD]

[WRITE_AND_FOLD]:
- Owner: the modality and fold laws every consumer composes — no code beyond what the package ships, because the law IS the composition: `useAtomValue(atom, selector)` scopes re-render to the projected slice (the selector overload replaces every `useMemo`-over-selector idiom; react-compiler owns the rest); `useAtomSet(atom, { mode })` selects the write shape by value — `"value"` fire-and-forget, `"promise"` awaitable to `Success`, `"promiseExit"` awaitable to `Exit` — one hook, three shapes, never a sibling; `Atom.optimistic`/`Atom.optimisticFn` write the optimistic value and reconcile against the effect's real `Result`; `Atom.refreshOnWindowFocus` and `Atom.withReactivity(keys)` are the refresh triggers.
- Law: async renders as a fold, never a flag pair — `useAtomValue` + `Result.match`/`Result.builder` for inline arms, or `useAtomSuspense(atom)` where `waiting` suspends to `<Suspense>` and `Failure` throws `Cause.squash(cause)` (the squashed tagged `E`) to the nearest boundary; the `waiting`/`previous` arms keep last-good data visible so a refresh never blanks the view.
- Law: the failure rail is Suspense plus the boundary — `system/primitive#FAILURE_ENVELOPE` catches the squashed `E` and `Match.tagsExhaustive` folds it; `includeFailure: true` is the inline escape hatch; a per-component `try`/`catch` or `isLoading`/`error` boolean pair is the named defect.
- Law: a mutation completing is awaited, never polled — a form submit awaits `mode: "promise"` inside `startTransition`; an atom poll to detect completion marks a missing write mode.
- Boundary: `Match` mechanics and error-family design are settled law; the boundary component row is `system/primitive`'s; the form round-trip composing these modalities is `view/form`'s.

```typescript
declare const _quota: Atom.Atom<Result.Result<{ readonly used: number; readonly cap: number }, { readonly _tag: "QuotaFault" }>>

const _ratio = Atom.map(_quota, (result) =>
  Result.match(result, {
    onInitial: () => 0,
    onSuccess: ({ value }) => value.used / value.cap,
    onFailure: () => 1,
  }))

const _draft = Atom.optimistic(Atom.make(0))
```

## [7]-[HISTORY_FOLD]

[HISTORY_FOLD]:
- Owner: `History` — the undo/redo owner: `History.make(seed, options?)` returns one writable atom whose read is the full `History.State<A>` (`past`/`present`/`future` over `Chunk`) and whose write is the closed command family `History.Op<A>` — `Push` (new present, past capped at `limit`, future cleared), `Undo`, `Redo`, `Clear`; the derived projections `History.present`, `History.undoable`, `History.redoable` are `Atom.map` folds consumers subscribe to individually so a stack mutation re-renders only the affected readers.
- Packages: `@effect-atom/atom-react` (`Atom.writable`); `effect` (`Chunk`, `Data`, `Equal`, `Match`).
- Entry: one `make` per undoable concern — selection sets, form drafts, camera bookmarks; the command union is the only write surface, so every mutation is replayable and the fold is total by `$match`.
- Law: `Push` with an `Equal`-identical present is a no-op — identity-aware deduplication keeps gesture streams from flooding the stack; `limit` is a policy value, never an unbounded array.
- Law: the fold is pure and lives in the write function — no effect, no clock; time-travel over effectful state is composition (`History` of the INPUT, replay through the owning fold), never a snapshot of an effect's output.
- Growth: a new stack behavior (a `Mark` checkpoint, a coalescing window) is one command case plus one fold arm — every consumer breaks loudly at the missing arm.

```typescript
import { Chunk, Data, Equal, Option } from "effect"

declare namespace History {
  type State<A> = { readonly past: Chunk.Chunk<A>; readonly present: A; readonly future: Chunk.Chunk<A> }
  type Op<A> = Data.TaggedEnum<{
    Push: { readonly next: A }
    Undo: {}
    Redo: {}
    Clear: {}
  }>
  type Options = { readonly limit?: number }
}

interface _OpDefinition extends Data.TaggedEnum.WithGenerics<1> {
  readonly taggedEnum: History.Op<this["A"]>
}

const _Op = Data.taggedEnum<_OpDefinition>()

const _step = <A>(state: History.State<A>, op: History.Op<A>, limit: number): History.State<A> =>
  _Op.$match(op, {
    Push: ({ next }) =>
      Equal.equals(state.present, next)
        ? state
        : {
            past: Chunk.takeRight(Chunk.append(state.past, state.present), limit),
            present: next,
            future: Chunk.empty<A>(),
          },
    Undo: () =>
      Option.match(Chunk.last(state.past), {
        onNone: () => state,
        onSome: (present) => ({
          past: Chunk.dropRight(state.past, 1),
          present,
          future: Chunk.prepend(state.future, state.present),
        }),
      }),
    Redo: () =>
      Option.match(Chunk.head(state.future), {
        onNone: () => state,
        onSome: (present) => ({
          past: Chunk.append(state.past, state.present),
          present,
          future: Chunk.drop(state.future, 1),
        }),
      }),
    Clear: () => ({ past: Chunk.empty<A>(), present: state.present, future: Chunk.empty<A>() }),
  })

const History: {
  readonly Op: typeof _Op
  readonly make: <A>(seed: A, options?: History.Options) => Atom.Writable<History.State<A>, History.Op<A>>
  readonly present: <A>(self: Atom.Atom<History.State<A>>) => Atom.Atom<A>
  readonly undoable: <A>(self: Atom.Atom<History.State<A>>) => Atom.Atom<boolean>
  readonly redoable: <A>(self: Atom.Atom<History.State<A>>) => Atom.Atom<boolean>
} = {
  Op: _Op,
  make: <A>(seed: A, options?: History.Options) => {
    const limit = options?.limit ?? 128
    const cell = Atom.make<History.State<A>>({ past: Chunk.empty<A>(), present: seed, future: Chunk.empty<A>() })
    return Atom.writable(
      (get) => get(cell),
      (ctx, op: History.Op<A>) => ctx.set(cell, _step(ctx.get(cell), op, limit)),
    )
  },
  present: (self) => Atom.map(self, (state) => state.present),
  undoable: (self) => Atom.map(self, (state) => Chunk.isNonEmpty(state.past)),
  redoable: (self) => Atom.map(self, (state) => Chunk.isNonEmpty(state.future)),
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { History, Store }
```
