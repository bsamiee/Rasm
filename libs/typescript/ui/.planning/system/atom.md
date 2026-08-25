# [UI_ATOM]

`@effect-atom` codes the ONE_FOLD_ONE_BINDING law as the folder's single state binding, and this module owns the whole bridge — the runtime root standing the app's Layer graph behind one atom registry, and every plane above it: persistence, SSR handoff, contract binding, derivation, write modality, undo/redo. Components are projection surfaces: they reach the Effect graph only through this bridge — never running effects, never owning Layers, never holding a second copy of domain state in `useState`; derived state is computed, never mirrored. Module: `ui/src/system/atom.ts`.

## [01]-[INDEX]

- [02]-[STORE_ROOT]: `Store.make` — the runtime root, registry policy, shared `MemoMap`, persistence rows; `Store`.
- [03]-[REMOTE_BINDING]: `AtomHttpApi` and `AtomRpc` carry the contract-binding rows; —.
- [04]-[SELECTOR_RAIL]: projection law — `map`/`mapResult`/`transform`, `family`, `debounce`, reactivity keys; —.
- [05]-[LIVE_BRIDGE]: host-fold ingress (`subscriptionRef`/`subscribable`), paged `pull`, stream egress; —.
- [06]-[WRITE_AND_FOLD]: write modality, optimistic reconcile, refresh triggers, the Suspense/boundary rail; —.
- [07]-[HISTORY_FOLD]: `History` folds a command-vocabulary undo/redo stack over any value atom; `History`.

## [02]-[STORE_ROOT]

[STORE_ROOT]:
- Owner: `Store` — one assembled owner: `make({ layer, memoMap? })` builds the `AtomRuntime` through `Atom.context({ memoMap })` so runtime atoms and the host `ManagedRuntime` share one construction of every Layer node; `policy` is the registry row (`defaultIdleTTL`, `timeoutResolution`) the app's `RegistryProvider` spreads.
- Packages: `@effect-atom/atom-react` (the barrel — `Atom`, `Registry`, `Result`, `Hydration` and the hook surface all reach the folder through it); `effect` (`Layer`, `Duration`, `Option`, `Schema`); `@rasm/core` (`Shape.Json` — the residue leaf a refused parcel preserves).
- Entry: `Store.make` is the one runtime mint; a per-atom `Layer` provision, a second registry outside test isolation, or a module-level `Atom.runtime` call beside it is the named defect.
- Law: one `RegistryProvider` at the app root supplies the store; scoped per-instance state covers component-local cells — a global atom keyed by component id never exists.
- Law: persistence is Schema-coded and the package surface IS the row, no wrapper — `Atom.kvs({ runtime, key, schema, defaultValue })` backs an atom by the platform `KeyValueStore` and `Atom.searchParam(name, { schema })` links one to a URL search param, each with the owning kernel schema (a brand, a `Schema.Literal` vocabulary) as the only codec, so `localStorage`/IndexedDB is never touched raw and a malformed stored value re-decodes to the default instead of poisoning the store.
- Law: stored state is a DECODE ADMISSION — `Store.sealed(schema, seal)` wraps a grain's own schema in the `{ generation, value }` parcel, so the generation its writer declared rides INSIDE the stored bytes and the comparison runs before the inner decode; a parcel written under another generation refuses on CONTENT, which is what forecloses the silent mis-restore where yesterday's bytes decode cleanly into today's schema while meaning something else, and `Atom.kvs`'s malformed-to-default arm seats the seeded default.
- Law: the `residue` column on the seal decides what a refused parcel leaves behind — `discard` drops it whole and the grain boots on its default, while `hold` decodes the refusal into `Store.Held` with the raw stored leaf preserved beside an absent value, so a precious grain keeps its evidence visible, counted, and hand-recoverable; a consumer unwraps `Store.Held.value` against its own seed and recovers from `residue` by hand.
- Law: the persisted key mints ONCE — `Store.key({ domain, grain })` derives `rasm.ui.<domain>.<grain>` and that key holds stable for the grain's whole life, since the generation seals the VALUE and a shape change moves the seal alone; a hand-spelled key beside the mint is the two-producer drift `UNREAD_KEY_ROW` names, so every persisted grain reaches storage through this member and no page spells the format again.
- Law: URL write SHAPING is a combinator on the persisted atom, never a routing-package hook tier — `runtime:browser/route` owns the typed query codec through `nuqs/server` and refuses `throttle`/`debounce`/`LimitUrlUpdates` at that seam because `createSerializer` ignores them, deferring the rate question to the hook tier it names a `ui` surface; that tier is `Atom.debounce` composed on the `Atom.searchParam` node, so how fast a hot control writes the URL is one combinator on the owning atom and this folder imports no query hook, no adapter, and no second URL-state binding. Placing `useQueryState` beside the store mints the second binding `ONE_FOLD_ONE_BINDING` forecloses, and a hand `URLSearchParams` write is the same defect wearing the platform's name.
- Law: SSR handoff is a real member pair — `Store.dehydrate(registry)` emits the `DehydratedAtom` state the server serializes, `HydrationBoundary` rehydrates it before children read, `Atom.serializable(self, { schema })` marks the atoms that cross with the kernel schema as codec, and a client refetch of server-computed data is the named defect.
- Law: the observe weave terminates here — `Store.make`'s `layer` is the ONE seam where an app's telemetry bridge rows (tracer, metric registry, log exporter) enter, so every `rasm.ui.<domain>.<verb>`-named rail behind a runtime atom exports the moment the app composes the bridge; owners state `Effect.withSpan`/`Effect.annotateLogs`/`Metric` rows at their own rails, span names share the `system/hook` rail vocabulary so a hook fact and its span correlate by name, and this folder imports zero collectors and mints zero exporters — removing the bridge layer removes every emission with zero owner edits.
- Boundary: the `ManagedRuntime` and boot seam are the browser composition root's — this module never calls a `run*` method; the shared `memoMap` argument is how the app hands both runtimes one acquisition map at composition, with `Atom.defaultMemoMap` as the shared floor when the app supplies none.
- Growth: a new registry knob is one field on `policy`; a persisted atom is one `Atom.kvs`/`Atom.searchParam` call over a `Store.key` grain row and a `Store.sealed` seal row, and a shape change is one `generation` bump on that seal — the `KeyValueStore` Layer swap is app composition.

```typescript signature
import { Atom, Hydration } from "@effect-atom/atom-react"
import type { Shape } from "@rasm/core"
import { Duration, type Layer, type Option, type Schema, type Types } from "effect"

const _policy = {
  defaultIdleTTL: Duration.minutes(5),
  timeoutResolution: Duration.millis(100),
} as const

type _Held<A> = { readonly value: Option.Option<A>; readonly residue: Option.Option<Shape.Json> }

declare namespace Store {
  type Options<R, E> = {
    readonly layer: Layer.Layer<R, E>
    readonly memoMap?: Layer.MemoMap
  }
  type Disposition = "hold" | "discard"
  type Segment<S extends string> = S extends `${string}.${string}` ? never : S
  type Grain<D extends string, G extends string> = { readonly domain: Store.Segment<D>; readonly grain: Store.Segment<G> }
  type Key = `rasm.ui.${string}.${string}`
  type Seal<P extends Store.Disposition> = { readonly generation: number; readonly residue: P }
  type Parcel<I> = { readonly generation: number; readonly value: I }
  type Held<A> = _Held<A>
  type Decoded<A, P extends Store.Disposition> = P extends "hold" ? _Held<A> : A
  type Shape = Types.Simplify<{
    readonly policy: typeof _policy
    readonly make: <R, E>(options: Store.Options<R, E>) => Atom.AtomRuntime<R, E>
    readonly key: typeof _key
    readonly sealed: typeof _sealed
    readonly dehydrate: typeof Hydration.dehydrate
    readonly hydrate: typeof Hydration.hydrate
  }>
}

const _key = <D extends string, G extends string>(row: Store.Grain<D, G>): Store.Key =>
  `rasm.ui.${row.domain}.${row.grain}`

declare const _sealed: <A, I, P extends Store.Disposition>(
  schema: Schema.Schema<A, I>,
  seal: Store.Seal<P>,
) => Schema.Schema<Store.Decoded<A, P>, Store.Parcel<I>>

const Store: Store.Shape = {
  policy: _policy,
  make: (options) => Atom.context({ memoMap: options.memoMap ?? Atom.defaultMemoMap })(options.layer),
  key: _key,
  sealed: _sealed,
  dehydrate: Hydration.dehydrate,
  hydrate: Hydration.hydrate,
}
```

## [03]-[REMOTE_BINDING]

[REMOTE_BINDING]:
- Owner: the contract-binding rows — an app declares `class Api extends AtomHttpApi.Tag<Api>()(id, { api, httpClient, baseUrl })` over its `@effect/platform` `HttpApi` value and `class Rpc extends AtomRpc.Tag<Rpc>()(id, { group, protocol })` over its `@effect/rpc` `RpcGroup`; each endpoint then IS a reactive atom (`.query(group, endpoint, request)` a read `Atom<Result>`, `.mutation(group, endpoint)` a callable `AtomResultFn`) with no query-key registry, no request cache, and no fetch glue. Invalidation is typed: `reactivityKeys` on queries and mutations join the invalidation graph, and `timeToLive` ages a query per row.
- Law: the contract is the single source — the fence's shape is the app-side declaration this lib legislates, and a hand-written fetch atom, a string cache key, or a data-fetching library beside the binding is the named defect.
- Law: invalidation is the typed graph, never a string protocol — a query names `reactivityKeys` and ages by `timeToLive`, a mutation names the keys it dirties, and firing the mutation re-runs every query atom holding a matching key through the `@effect/experimental` `Reactivity` peer; `Atom.withReactivity(keys)` joins any derived atom to the same graph, and `Atom.refresh` is the point invalidation.
- Law: a streaming rpc's `.query` is a `PullResult` atom — write to advance the page; the pull geometry stays inside the atom, never a hand-rolled cursor cell.
- Law: identifier-grade `GlobalId` and `Digest.Key<"content">` context rides spans and logs, never metric attributes.
- Boundary: the `HttpApi`/`RpcGroup` values are edge contract material the app supplies, so the binding class is an APP-SIDE declaration this page legislates the exact shape of — the fence below is that shape, not a member of this module's export surface.

```typescript signature
import { AtomHttpApi, AtomRpc } from "@effect-atom/atom-react"
import type { HttpApi } from "@effect/platform"
import { FetchHttpClient } from "@effect/platform"
import { RpcClient, type RpcGroup } from "@effect/rpc"
import { Duration } from "effect"

declare const _contract: HttpApi.HttpApi<never, never>
declare const _procedures: RpcGroup.RpcGroup<never>

class Api extends AtomHttpApi.Tag<Api>()("app/Api", {
  api: _contract,
  httpClient: FetchHttpClient.layer,
  baseUrl: "<origin>",
}) {}

const _roster = Api.query("crew", "list", {
  path: {},
  reactivityKeys: ["crew"],
  timeToLive: Duration.minutes(2),
})

const _enroll = Api.mutation("crew", "enroll", { reactivityKeys: ["crew"] })

class Rpc extends AtomRpc.Tag<Rpc>()("app/Rpc", {
  group: _procedures,
  protocol: RpcClient.layerProtocolHttp({ url: "<origin>/rpc" }),
  spanPrefix: "rasm.ui.rpc",
}) {}

const _tail = Rpc.query("tail", { key: "<value-a>" }, { reactivityKeys: ["tail"] })

const _commit = Rpc.mutation("commit")
```

## [04]-[SELECTOR_RAIL]

[SELECTOR_RAIL]:
- Law: a projection is a derived atom or a hook selector, decided by reach — cross-component projections are `Atom.map(atom, f)` (memoized once in the registry, shared by every reader); component-local slices are the `useAtomValue(atom, selector)` overload (subscription scoped to the slice); the same projection existing as both is a duplicate fold.
- Law: `Atom.mapResult` projects the `Success` arm only, preserving `waiting`/`previous` so derived async state inherits stale-while-revalidate; `Atom.transform` rebuilds through `get` when a derivation reads several atoms — dependency tracking stays structural, never a hand-wired subscription.
- Law: per-entity atoms are `Atom.family((key) => atom)` — one memoized atom per key with no leak (the registry's idle TTL governs); a `Map` of atoms or an atom-of-`Map` re-derives what `family` owns. Keys are kernel brands or `Data`-constructed values so family identity is structural.
- Law: update shaping is a combinator on the owner — `Atom.debounce(ms)` rate-limits a hot derivation (search input feeding a filter), `Atom.withReactivity(keys)` re-runs on typed invalidation coordinates, `Atom.keepAlive`/`Atom.setIdleTTL` pin or age a node, `Atom.withFallback(fallbackAtom)`/`Atom.initialValue(value)` seed a first render; shaping never lives in an effect body.
- Law: refresh triggers are combinator rows, never effects — `Atom.refreshOnWindowFocus` is the stale-while-focus row, and `Atom.makeRefreshOnSignal(signal)` derives a refresh trigger from any signal atom (`Atom.windowFocusSignal` is the shipped one); a `visibilitychange` listener beside the store restates them.
- Law: fine-grained sub-value subscription is the `AtomRef` cursor — `AtomRef.make(value)` mints the mutable root, `useAtomRefProp(ref, key)` derives the per-property child so a large draft re-renders only the edited field, and `AtomRef.collection(items)` is the ordered ref collection for per-item subscriptions without re-running the owning atom; `view/form` drafts and `view/table` row edits ride exactly this cursor, and a per-field atom family over one draft is the named defect.
- Law: a foreign store crossing is an adapter atom minted over the fold's own cell — `view/table`'s `Grid.edge` wraps a registry cell as a `@tanstack/store` `Atom` whose `get`/`set`/`subscribe` all delegate to the registry, so the effect-atom cell stays the ONE writer and the foreign store holds no second copy; a foreign-store base atom left live beside the fold is the two-writer defect the adapter exists to close.

```typescript signature
import { AtomRef } from "@effect-atom/atom-react"
import type { Digest } from "@rasm/core"
import { Array, Duration, Number } from "effect"

declare const _rows: Atom.Atom<ReadonlyArray<{ readonly key: Digest.Key<"content">; readonly rank: number }>>

const _byKey = Atom.family((key: Digest.Key<"content">) =>
  Atom.map(_rows, (rows) => Array.findFirst(rows, (row) => row.key === key)))

const _crest = Atom.map(_rows, (rows) => Array.reduce(rows, 0, (peak, row) => Number.max(peak, row.rank)))

const _query = Atom.make("").pipe(Atom.debounce(Duration.millis(150)))

const _fresh = Atom.map(_rows, (rows) => rows.length).pipe(Atom.refreshOnWindowFocus)

const _pinned = Atom.make(0).pipe(Atom.keepAlive)

const _cursor = AtomRef.make({ label: "", rank: 0, note: "" })
```

## [05]-[LIVE_BRIDGE]

[LIVE_BRIDGE]:
- Law: a host or state fold enters the view plane as an atom, never as a hand subscription — `Atom.subscriptionRef(ref)` binds a `SubscriptionRef` writable, `Atom.subscribable(sub)` binds the read-only `Subscribable` projection, and the component reads through `useAtomValue` like any other node; a `useSyncExternalStore` call outside the atom binding is the named defect.
- Law: the browser host planes bind through exactly these rows — the router's `location`/`pending` subscribables, the service-worker phase cell, the install stance, the navigation guard, the session-vault status, and the viewer's frame clock all publish `Subscribable`/`SubscriptionRef` surfaces, and each enters the component tree as one `Atom.subscribable`/`Atom.subscriptionRef` binding at app composition; a component reading a host service directly restates the bridge.
- Law: the frame clock is one of those rows and the spatial stratum's ONLY time coordinate — `viewer/geo#FRAME_CLOCK` owns the single `requestAnimationFrame` registration and publishes its `{ now, delta }` frame as a `SubscriptionRef` the bridge binds here, so mixer advance, layer animation, and construction-sequence scrub all derive from one coordinate; a renderer minting a clock of its own beside this binding is a second time producer whose drift makes a scrub position mean two things at once, and a `requestAnimationFrame` registration anywhere but that owner is the named defect. Each renderer still owns its own DRAW cadence — the clock owns the time, never the pump.
- Law: a paged stream is `Atom.pull(stream)` — the atom holds `PullResult` (`{ done, items }` folded into `Result`), a write advances the page, and the pull geometry never leaks as a cursor cell beside the atom.
- Law: egress mirrors ingress — `Atom.toStream(atom)`/`Atom.toStreamResult(atom)` observe an atom as an Effect `Stream` where a pipeline (a wire egress, a probe fold) consumes view-plane state; `Atom.batch(f)` coalesces multi-atom writes into one notification pass at imperative seams.
- Law: effectful reads from Effect code go through the accessor family — `Atom.get`/`Atom.set`/`Atom.refresh`/`Atom.getResult` return `Effect<_, _, AtomRegistry>` and resolve the ambient registry; imperative non-React drivers read through `registry.get`/`registry.modify(atom, f)` (value and next state atomically) and `registry.subscribe`; a captured registry reference threaded by hand restates the Tag.
- Law: a statechart actor binds through the SAME row — `Machine.boot(machine, input)` yields a `Machine.Actor` that IS a `Subscribable` of its state, so `Atom.subscribable(actor)` is the whole machine→view seam: `viewer/scene#BACKEND_SELECT`'s `Glb.lifecycle` (the realized viewer instance — its phase reaches React through exactly this row), wizard flows, and multi-step overlay statecharts reach React as ordinary atoms, `snapshot`/`restore` cross remounts, and no second machine-binding mechanism exists.
- Boundary: `Stream` pipeline law is settled; which host folds exist is the owning runtime page's; `Machine` definitions live with the owning plane (`viewer/scene` lifecycle, `view/form` wizard) — this cluster owns only the crossing.

```typescript signature
import { Result } from "@effect-atom/atom-react"
import type { Machine } from "@effect/experimental"
import type { Stream, SubscriptionRef } from "effect"

declare const _live: SubscriptionRef.SubscriptionRef<ReadonlyArray<string>>
declare const _feed: Stream.Stream<{ readonly at: number }, { readonly _tag: "FeedFault" }>
declare const _actor: Machine.Actor<
  Machine.Machine<{ readonly stage: "collect" | "confirm" | "done" }, never, never, void, never, never>
>

const _labels = Atom.subscriptionRef(_live)

const _page = Atom.pull(_feed)

const _drained = Atom.toStreamResult(_page)

const _stage = Atom.subscribable(_actor)
```

## [06]-[WRITE_AND_FOLD]

[WRITE_AND_FOLD]:
- Owner: the modality and fold laws every consumer composes — no code beyond what the package ships, because the law IS the composition: `useAtomValue(atom, selector)` scopes re-render to the projected slice (the selector overload replaces every `useMemo`-over-selector idiom; react-compiler owns the rest); `useAtomSet(atom, { mode })` selects the write shape by value — `"value"` fire-and-forget, `"promise"` awaitable to `Success`, `"promiseExit"` awaitable to `Exit` — one hook, three shapes, never a sibling; `Atom.optimistic`/`Atom.optimisticFn` write the optimistic value and reconcile against the effect's real `Result`; `Atom.refreshOnWindowFocus` and `Atom.withReactivity(keys)` are the refresh triggers.
- Law: async renders as a fold, never a flag pair — `useAtomValue` + `Result.match` for total inline arms, `Result.matchWithWaiting` where the stale-while-revalidate affordance is its own arm, `Result.builder(self).onInitial(…).onSuccess(…).orNull()` where arms accrete fluently, or `useAtomSuspense(atom)` where `waiting` suspends to `<Suspense>` and `Failure` throws `Cause.squash(cause)` (the squashed tagged `E`) to the nearest boundary; the `waiting`/`previous` arms keep last-good data visible so a refresh never blanks the view.
- Law: multi-step optimism carries its reducer — `Atom.optimisticFn({ …, reducer })` folds a sequence of optimistic writes (a form field batch) into the pending value and reconciles the whole fold against the effect's real `Result`; bare `Atom.optimistic` is the single-value form.
- Law: the failure rail is Suspense with the boundary — `system/primitive#FAILURE_ENVELOPE` catches the squashed `E` and `Match.tagsExhaustive` folds it; `includeFailure: true` is the inline escape hatch; a per-component `try`/`catch` or `isLoading`/`error` boolean pair is the named defect.
- Law: a mutation completing is awaited, never polled — the awaited mode is the one the caller's fold needs: `"promise"` where a plain success suffices, `"promiseExit"` where the Cause rail itself is folded (`view/form#SUBMIT_TRIP` reads refusal, defect, and interrupt off the `Exit` a rejecting promise erases); an atom poll to detect completion marks a missing write mode.
- Boundary: `Match` mechanics and error-family design are settled law; the boundary component row is `system/primitive`'s; the form round-trip composing these modalities is `view/form`'s.

```typescript signature
declare const _quota: Atom.Atom<Result.Result<{ readonly used: number; readonly cap: number }, { readonly _tag: "QuotaFault" }>>

const _ratio = Atom.map(_quota, (result) =>
  Result.match(result, {
    onInitial: () => 0,
    onSuccess: ({ value }) => value.used / value.cap,
    onFailure: () => 1,
  }))

const _phase = Atom.map(_quota, (result) =>
  Result.matchWithWaiting(result, {
    onWaiting: () => "refreshing" as const,
    onSuccess: () => "live" as const,
    onError: () => "refused" as const,
    onDefect: () => "torn" as const,
  }))

const _draft = Atom.optimistic(Atom.make(0))
```

## [07]-[HISTORY_FOLD]

[HISTORY_FOLD]:
- Owner: `History` — the undo/redo owner: `History.make(seed, options?)` returns one writable atom whose read is the full `History.State<A>` (`past`/`present`/`future` over `Chunk`) and whose write is the closed command family `History.Op<A>` — `Push` (new present, past capped at `limit`, future cleared), `Undo`, `Redo`, `Clear`; the derived projections `History.present`, `History.undoable`, `History.redoable` are `Atom.map` folds consumers subscribe to individually so a stack mutation re-renders only the affected readers.
- Packages: `@effect-atom/atom-react` (`Atom.writable`); `effect` (`Chunk`, `Data`, `Equal`, `Option`).
- Entry: one `make` per undoable concern — selection sets, form drafts, camera bookmarks; the command union is the only write surface, so every mutation is replayable and the fold is total by `$match`.
- Law: `Push` with an `Equal`-identical present is a no-op — identity-aware deduplication keeps gesture streams from flooding the stack; `limit` is a policy value, never an unbounded array.
- Law: the fold is pure and lives in the write function — no effect, no clock; time-travel over effectful state is composition (`History` of the INPUT, replay through the owning fold), never a snapshot of an effect's output.
- Boundary: `History` is the pure in-registry transition fold; a transition family that must answer typed requests, survive process restarts, or snapshot/restore is a `Machine` actor bound through `[5]`'s `Atom.subscribable` row — the two never shadow one concern.
- Growth: a new stack behavior (a `Mark` checkpoint, a coalescing window) is one command case and one fold arm — every consumer breaks loudly at the missing arm.

```typescript signature
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
  type Shape = Types.Simplify<{
    readonly Op: typeof _Op
    readonly make: <A>(seed: A, options?: History.Options) => Atom.Writable<History.State<A>, History.Op<A>>
    readonly present: <A>(self: Atom.Atom<History.State<A>>) => Atom.Atom<A>
    readonly undoable: <A>(self: Atom.Atom<History.State<A>>) => Atom.Atom<boolean>
    readonly redoable: <A>(self: Atom.Atom<History.State<A>>) => Atom.Atom<boolean>
  }>
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

const History: History.Shape = {
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

// --- [EXPORTS] -------------------------------------------------------------------------

export { History, Store }
```

## [08]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
