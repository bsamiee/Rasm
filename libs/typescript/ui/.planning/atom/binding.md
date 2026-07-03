# [UI_BINDING]

`atom/binding.ts` is the ONE_FOLD_ONE_BINDING law made code: `@effect-atom` is the single state binding of the folder, and this module owns its root — one `Store.make` standing the app's Layer graph behind the atom registry with a shared `MemoMap`, one registry policy row, the persisted-atom rows (`Atom.kvs`/`Atom.searchParam` with kernel `Schema` codecs), the write-modality and async-fold laws every view row obeys, and the `AtomHttpApi`/`AtomRpc` direct-binding rows gated `[R25]`. Components are projection surfaces: they reach the Effect graph only through this bridge — never running effects, never owning Layers, never holding a second copy of domain state in `useState`.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]        | [OWNS]                                                                             |
| :-----: | :--------------- | :---------------------------------------------------------------------------------- |
|   [1]   | `STORE_ROOT`     | `Store.make` — the runtime root, registry policy, shared `MemoMap`, persistence rows |
|   [2]   | `REMOTE_BINDING` | the `AtomHttpApi`/`AtomRpc` contract-binding rows, `[R25]`-gated with the carry law  |
|   [3]   | `WRITE_AND_FOLD` | write modality, optimistic reconcile, refresh triggers, the Suspense/boundary rail   |

## [2]-[STORE_ROOT]

- Owner: `Store` — one assembled owner: `make({ layer, memoMap? })` builds the `AtomRuntime` through `Atom.context({ memoMap })` so runtime atoms and the host `ManagedRuntime` share one construction of every Layer node; `policy` is the registry row (`defaultIdleTTL`, `timeoutResolution`) the app's `RegistryProvider` spreads.
- Packages: `@effect-atom/atom-react` (the barrel — `Atom`, `Registry`, `Result`, `Hydration` and the hook surface all reach the folder through it), `effect` (`Layer`, `Duration`).
- Entry: `Store.make` is the one runtime mint; a per-atom `Layer` provision, a second registry outside test isolation, or a module-level `Atom.runtime` call beside it is the named defect.
- Law: one `RegistryProvider` at the app root supplies the store; `ScopedAtom` (at `view/compose`) covers per-instance state — a global atom keyed by component id never exists.
- Law: persistence is Schema-coded and the package surface IS the row, no wrapper — `Atom.kvs({ runtime, key, schema, defaultValue })` backs an atom by the platform `KeyValueStore` and `Atom.searchParam(name, { schema })` links one to a URL search param, each with the owning kernel schema (a brand, a `Schema.Literal` vocabulary) as the only codec, so `localStorage`/IndexedDB is never touched raw and a malformed stored value re-decodes to the default instead of poisoning the store.
- Law: SSR handoff rides `Hydration` — the server dehydrates the registry, `HydrationBoundary` rehydrates before children read, and a client refetch of server-computed data is the named defect; `Atom.serializable` marks the atoms that cross.
- Boundary: the `ManagedRuntime` and boot seam are `browser`'s — this module never calls a `run*` method; the shared `memoMap` argument is how the app hands both runtimes one acquisition map at composition.
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

- Owner: the contract-binding rows — an app declares `class Api extends AtomHttpApi.Tag<Api>()(...)` over its `@effect/platform` `HttpApi` value and `class Rpc extends AtomRpc.Tag<Rpc>()(...)` over its `@effect/rpc` `RpcGroup`; each endpoint then IS a reactive atom (`Api.query(group, endpoint, request)` a read `Atom<Result>`, `Api.mutation(group, endpoint)` a callable `AtomResultFn`) with no query-key registry, no request cache, and no fetch glue. Invalidation is typed: `reactivityKeys` on queries and mutations join the `@effect/experimental` invalidation graph, and `timeToLive` ages a query per row.
- Gate: `[R25]` — the `AtomHttpApi`/`AtomRpc` member surface is RESEARCH; until it closes, the carry law holds: bind the derived `HttpApiClient.make(contract)` client through `Atom.make(effect)` on the `Store` runtime and fold `Result` identically — the consumer surface (`Atom<Result<A, E>>`) is the same either way, so closing the gate rewrites only the declaration row.
- Law: the contract is the single source — the fence's shape is the app-side declaration this lib legislates, and a hand-written fetch atom, a string cache key, or a data-fetching library beside the binding is the named defect.
- Law: a streaming rpc's `.query` is a `PullResult` atom — write to advance the page; the pull geometry stays inside the atom, never a hand-rolled cursor cell.
- Boundary: the `HttpApi`/`RpcGroup` values are `edge` contract material the app supplies, so the binding class is an APP-SIDE declaration this page legislates the exact shape of — the fence below is that shape, not a member of this module's export surface.

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

## [4]-[WRITE_AND_FOLD]

- Owner: the modality and fold laws every consumer composes — no code beyond what the package ships, because the law IS the composition: `useAtomValue(atom, selector)` scopes re-render to the projected slice (the selector overload replaces every `useMemo`-over-selector idiom; react-compiler owns the rest); `useAtomSet(atom, { mode })` selects the write shape by value — `"value"` fire-and-forget, `"promise"` awaitable to `Success`, `"promiseExit"` awaitable to `Exit` — one hook, three shapes, never a sibling; `Atom.optimistic`/`Atom.optimisticFn` write the optimistic value and reconcile against the effect's real `Result`; `Atom.refreshOnWindowFocus` and `Atom.withReactivity(keys)` are the refresh triggers.
- Law: async renders as a fold, never a flag pair — `useAtomValue` + `Result.match`/`Result.builder` for inline arms, or `useAtomSuspense(atom)` where `waiting` suspends to `<Suspense>` and `Failure` throws `Cause.squash(cause)` (the squashed tagged `E`) to the nearest boundary; the `waiting`/`previous` arms keep last-good data visible so a refresh never blanks the view.
- Law: the failure rail is Suspense plus the boundary — `view/primitive`'s boundary row catches the squashed `E` and `Match.tagsExhaustive` folds it; `includeFailure: true` is the inline escape hatch; a per-component `try`/`catch` or `isLoading`/`error` boolean pair is the named defect.
- Law: a mutation completing is awaited, never polled — a form submit awaits `mode: "promise"` inside `startTransition`; an atom poll to detect completion marks a missing write mode.
- Boundary: `Match` mechanics and error-family design are settled law (`rails-and-effects`); the boundary component row is `view/primitive`'s; undo/redo and derived selectors are `atom/derive`'s.

```typescript
import { Atom, Result } from "@effect-atom/atom-react"

declare const _quota: Atom.Atom<Result.Result<{ readonly used: number; readonly cap: number }, { readonly _tag: "QuotaFault" }>>

const _ratio = Atom.map(_quota, (result) =>
  Result.match(result, {
    onInitial: () => 0,
    onSuccess: ({ value }) => value.used / value.cap,
    onFailure: () => 1,
  }))

const _draft = Atom.optimistic(Atom.make(0))

// --- [EXPORTS] --------------------------------------------------------------------------

export { Store }
```
