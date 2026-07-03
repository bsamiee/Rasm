# [UI_DERIVE]

`atom/derive.ts` is the derivation plane over the one store: selectors and projections compute from owning atoms (`Atom.map`/`mapResult`/`transform`, keyed `Atom.family`, `debounce`, `withReactivity`), `state`-folder folds enter as live atoms through the `Subscribable`/`SubscriptionRef` bridge and leave as `Stream`s through the egress bridge, and the module's own owner — `History` — is the undo/redo stack fold: one writable atom whose write vocabulary is a closed command family and whose projections (`present`, `undoable`, `redoable`) are derived atoms every consumer reads. Derived state is computed, never mirrored; a `useState` copy of an atom projection is the named defect.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]       | [OWNS]                                                                              |
| :-----: | :-------------- | :----------------------------------------------------------------------------------- |
|   [1]   | `SELECTOR_RAIL` | projection law — `map`/`mapResult`/`transform`, `family`, `debounce`, reactivity keys |
|   [2]   | `LIVE_BRIDGE`   | `state`-fold ingress (`subscriptionRef`/`subscribable`), paged `pull`, stream egress  |
|   [3]   | `HISTORY_FOLD`  | the `History` owner — command-vocabulary undo/redo stack over any value atom          |

## [2]-[SELECTOR_RAIL]

- Law: a projection is a derived atom or a hook selector, decided by reach — cross-component projections are `Atom.map(atom, f)` (memoized once in the registry, shared by every reader); component-local slices are the `useAtomValue(atom, selector)` overload (subscription scoped to the slice); the same projection existing as both is a duplicate fold.
- Law: `Atom.mapResult` projects the `Success` arm only, preserving `waiting`/`previous` so derived async state inherits stale-while-revalidate; `Atom.transform` rebuilds through `get` when a derivation reads several atoms — dependency tracking stays structural, never a hand-wired subscription.
- Law: per-entity atoms are `Atom.family((key) => atom)` — one memoized atom per key with no leak (the registry's idle TTL governs); a `Map` of atoms or an atom-of-`Map` re-derives what `family` owns. Keys are kernel brands or `Data`-constructed values so family identity is structural.
- Law: update shaping is a combinator on the owner — `Atom.debounce(ms)` rate-limits a hot derivation (search input feeding a filter), `Atom.withReactivity(keys)` re-runs on typed invalidation coordinates, `Atom.keepAlive`/`Atom.setIdleTTL` pin or age a node; shaping never lives in an effect body.
- Boundary: the store root and write modality are `atom/binding`'s; the `state` fold surfaces these selectors project are `@rasm/ts/state`'s and arrive already-typed.

```typescript
import { Atom } from "@effect-atom/atom-react"
import { Array, Duration, Number } from "effect"
import type { ContentKey } from "@rasm/ts/kernel"

declare const _rows: Atom.Atom<ReadonlyArray<{ readonly key: ContentKey; readonly rank: number }>>

const _byKey = Atom.family((key: ContentKey) =>
  Atom.map(_rows, (rows) => Array.findFirst(rows, (row) => row.key === key)))

const _crest = Atom.map(_rows, (rows) => Array.reduce(rows, 0, (peak, row) => Number.max(peak, row.rank)))

const _query = Atom.make("").pipe(Atom.debounce(Duration.millis(150)))
```

## [3]-[LIVE_BRIDGE]

- Law: a `state` fold enters the view plane as an atom, never as a hand subscription — `Atom.subscriptionRef(ref)` binds a `SubscriptionRef` writable, `Atom.subscribable(sub)` binds the read-only `Subscribable` projection (`state/query/live` feeds exactly this), and the component reads through `useAtomValue` like any other node; a `useSyncExternalStore` call outside the atom binding is the named defect.
- Law: a paged stream is `Atom.pull(stream)` — the atom holds `PullResult` (`{ done, items }` folded into `Result`), a write advances the page, and the pull geometry never leaks as a cursor cell beside the atom.
- Law: egress mirrors ingress — `Atom.toStream(atom)`/`Atom.toStreamResult(atom)` observe an atom as an Effect `Stream` where a pipeline (a `wire` egress, a probe fold) consumes view-plane state; `Atom.batch(f)` coalesces multi-atom writes into one notification pass at imperative seams.
- Law: effectful reads from Effect code go through the accessor family — `Atom.get`/`Atom.set`/`Atom.refresh` return `Effect<_, _, AtomRegistry>` and resolve the ambient registry; a captured registry reference threaded by hand restates the Tag.
- Boundary: `Stream` pipeline law is settled (`streams`); which `state` folds exist is `@rasm/ts/state`'s; this cluster owns only the crossing.

```typescript
import { Atom, Result } from "@effect-atom/atom-react"
import type { Stream, SubscriptionRef } from "effect"

declare const _live: SubscriptionRef.SubscriptionRef<ReadonlyArray<string>>
declare const _feed: Stream.Stream<{ readonly at: number }, { readonly _tag: "FeedFault" }>

const _labels = Atom.subscriptionRef(_live)

const _page = Atom.pull(_feed)

const _drained = Atom.toStreamResult(_page)
```

## [4]-[HISTORY_FOLD]

- Owner: `History` — the undo/redo owner: `History.make(seed, options?)` returns one writable atom whose read is the full `History.State<A>` (`past`/`present`/`future` over `Chunk`) and whose write is the closed command family `History.Op<A>` — `Push` (new present, past capped at `limit`, future cleared), `Undo`, `Redo`, `Clear`; the derived projections `History.present`, `History.undoable`, `History.redoable` are `Atom.map` folds consumers subscribe to individually so a stack mutation re-renders only the affected readers.
- Packages: `@effect-atom/atom-react` (`Atom.writable`), `effect` (`Chunk`, `Data`, `Match`).
- Entry: one `make` per undoable concern — selection sets (`viewer/mark/selection`), form drafts, camera bookmarks; the command union is the only write surface, so every mutation is replayable and the fold is total by `Match.exhaustive`.
- Law: `Push` with an `Equal`-identical present is a no-op — identity-aware deduplication keeps gesture streams from flooding the stack; `limit` is a policy value, never an unbounded array.
- Law: the fold is pure and lives in the write function — no effect, no clock; time-travel over effectful state is composition (`History` of the INPUT, replay through the owning fold), never a snapshot of an effect's output.
- Growth: a new stack behavior (a `Mark` checkpoint, a coalescing window) is one command case plus one fold arm — every consumer breaks loudly at the missing arm.

```typescript
import { Atom } from "@effect-atom/atom-react"
import { Chunk, Data, Equal } from "effect"

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
      Chunk.isEmpty(state.past)
        ? state
        : {
            past: Chunk.dropRight(state.past, 1),
            present: Chunk.unsafeLast(state.past),
            future: Chunk.prepend(state.future, state.present),
          },
    Redo: () =>
      Chunk.isEmpty(state.future)
        ? state
        : {
            past: Chunk.append(state.past, state.present),
            present: Chunk.unsafeHead(state.future),
            future: Chunk.drop(state.future, 1),
          },
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

export { History }
```
