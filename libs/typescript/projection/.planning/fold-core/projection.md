# [PROJECTION_FOLD_FACE]

`Projection<A>` is the one `Subscribable<A>` read face every store exposes — the `{ get, changes }` contract the `@effect-atom/atom` bridge consumes verbatim through `Atom.subscribable`, lifting any store's raw `SubscriptionRef` into a derived concept the `ui` boundary binds without reaching the ref interior. `projectStore` is the single lift that turns a `SubscriptionRef<HashMap>` into its baseline `Projection`; `derive` is the single combinator that maps one `Projection` into a projected view — a filtered map, a sorted slice, a scalar gate — as one new `Projection` carrying its own `get` snapshot and `changes` stream. Every store in the folder (`feedStore` of `feed-stores/live-cells#LIVE_CELLS`, `causalStore` of `causality-graph/version-vector#VERSION_VECTOR`, the `convergence/lww-merge#LWW_MERGE` CRDT fold, `availabilityStore` of `availability/availability-gate#AVAILABILITY_GATE`, the `evidence/evidence-correlation#EVIDENCE` projection, `queryStore` of `live-query/reactive-query#REACTIVE_QUERY`) presents its read as one `Projection` row, and every derived read the `ui` would otherwise recombine at the bind site is one `derive` row at the `projection` altitude — the `projection` half of the branch `ONE_FOLD_ONE_BINDING` concert. The face dials no transport, owns no fold, and adds no second snapshot: the `changes` stream is the store's own `SubscriptionRef.changes`, deduplicated by `Stream.changes`, never a re-fold.

## [1]-[INDEX]

| CLUSTER | OWNS |
| --- | --- |
| `[2]-[PROJECTION]` | `Projection<A>` — the `Subscribable<A>` read alias; `projectStore`, the baseline lift of a store ref into a `Projection`; `derive`, the snapshot-and-stream view combinator; `deriveEffect`, the effectful-snapshot variant that captures the ambient context and provides it into the projected `get`/`changes` so the resulting `Subscribable` carries no dependency tail. |
| `[3]-[BINDING_SEAM]` | The `Atom.subscribable` consumer contract — the `ui` atom-bridge bind law the `Projection` face produces against, and the boundary discipline that keeps the raw `SubscriptionRef` interior off the bind site. |

## [2]-[PROJECTION]

- Owner: `Projection<A>`, the type alias for `Subscribable.Subscribable<A>` — the `{ get: Effect<A>, changes: Stream<A> }` read contract — that names the folder's read face once so no store re-declares the pair; `projectStore`, the lift that reads a `SubscriptionRef<HashMap<K, V>>` and returns it as `Projection<HashMap<K, V>>` deduplicated through `Stream.changes`; `derive`, the combinator that maps one `Projection<A>` into a `Projection<B>` under a pure `view` arm, snapshotting `get` through `Effect.map` and the `changes` stream through `Stream.map` then `Stream.changes`; `deriveEffect`, the variant whose `view` arm is itself effectful, capturing the ambient context through `Effect.context` and providing it into the projected `get` (`Effect.provide`) and `changes` (`Stream.provideContext`) so the resulting `Subscribable<B, E>` carries no `R` tail.
- Cases: a store with a stable read is one `projectStore` row; a derived read — the `availabilityStore` `isEnabled` scalar gate, the `evidenceProjection` `byContentKey` slice, an as-of projection of the keyed map — is one `derive` row whose `view` is a pure projection of the held value; a derived read whose projection requires an effect (a content-key digest assembly, a scoped lookup) is one `deriveEffect` row. `derive` never re-folds the source: it maps the held snapshot and the change events the store already emits, so the projected `changes` stream fires only when the projected value actually differs, the `Stream.changes` dedup collapsing a source event that leaves the view equal.
- Entry: every store's read crosses the boundary as a `Projection`; the `ui` atom bridge binds it through `Atom.subscribable`, never the raw `SubscriptionRef`.
- Packages: `effect` for `Subscribable` (the `Subscribable<A>` `{ get, changes }` contract, `Subscribable.make`), `SubscriptionRef` (the store ref whose `.changes` stream the lift reads, and `SubscriptionRef.get` for the snapshot), `Stream` (`Stream.map`, `Stream.mapEffect`, `Stream.changes`, `Stream.provideContext` for the deduplicated projected change stream), `Effect` (`Effect.map`, `Effect.flatMap`, `Effect.context`, `Effect.provide` for the projected snapshot and the effectful-view context provision), and the `Equal`-backed `Stream.changes` deduplication.
- Growth: a new store lands as one `projectStore` row; a new derived read lands as one `derive` (or `deriveEffect`) row; neither the `Projection` alias nor the lift grows. A second read face, a per-store sibling adapter, or a hand-rolled `{ get, subscribe }` pair is the deleted form — the `Subscribable` contract is the one face.
- Boundary: the face re-validates nothing and re-folds nothing — `get` and `changes` are the store's own `SubscriptionRef` read and dedup-stream; the projected `changes` borrows the store's emission cadence (the `stream-policy#STREAM_POLICY` reconnect-and-back-pressure is owned upstream, never re-applied here); `derive`'s `view` is a pure function of the held value and adds no I/O, no clock, no slot, no hash; the `@effect-atom/atom` `Atom.subscribable` constructor consumes the `{ get, changes }` contract at the `ui` boundary and is never imported into the fold interior.

```ts contract
import { Effect, Stream, Subscribable, SubscriptionRef } from "effect";

// --- [TYPES] -------------------------------------------------------------------------

type Projection<A> = Subscribable.Subscribable<A>;

// --- [OPERATIONS] --------------------------------------------------------------------

const projectStore = <A>(ref: SubscriptionRef.SubscriptionRef<A>): Projection<A> =>
  Subscribable.make({
    get: SubscriptionRef.get(ref),
    changes: Stream.changes(ref.changes),
  });

const derive = <A, B>(source: Projection<A>, view: (value: A) => B): Projection<B> =>
  Subscribable.make({
    get: Effect.map(source.get, view),
    changes: Stream.changes(Stream.map(source.changes, view)),
  });

const deriveEffect = <A, B, E, R>(
  source: Projection<A>,
  view: (value: A) => Effect.Effect<B, E, R>,
): Effect.Effect<Subscribable.Subscribable<B, E>, never, R> =>
  Effect.map(Effect.context<R>(), (context) =>
    Subscribable.make({
      get: Effect.flatMap(source.get, view).pipe(Effect.provide(context)),
      changes: Stream.changes(
        Stream.mapEffect(source.changes, view).pipe(Stream.provideContext(context)),
      ),
    }),
  );

// --- [EXPORTS] -----------------------------------------------------------------------

export { derive, deriveEffect, projectStore, type Projection };
```

## [3]-[BINDING_SEAM]

- Owner: the `Atom.subscribable` consumer contract — the produced read law the `ui` folder binds against. The seam is documentation of the boundary the `Projection` face produces, not an imported surface: the fold interior names `@effect-atom/atom` nowhere, and the `ui` folder names `SubscriptionRef` nowhere.
- Cases: a store-baseline bind is `Atom.subscribable(() => projectStore(ref))`; a derived-read bind is `Atom.subscribable(() => derive(storeProjection, view))`; each binds one `Subscribable<A>` and reads its `get`/`changes` contract, never the `SubscriptionRef` `Synchronized`-ref interior (which carries `modify`/`update` write capability the read face must not surface to `ui`).
- Entry: the `ui` atom-native-binding-collapse idea consumes one `Atom` per derived concept; every derived reactive view the `ui` would otherwise recombine at the bind site is moved to one `derive` row at the `projection` altitude, so `ui` carries no view-state recombination and a new derived read is one `derive` row rather than a parallel bind-site fold.
- Packages: `@effect-atom/atom` for `Atom.subscribable` (the consumer; bound at the `ui` boundary, never imported here).
- Growth: a new derived view the `ui` needs lands as a `derive`/`deriveEffect` row this folder owns and a single `Atom.subscribable` bind the `ui` folder owns — the two folders meet at the `{ get, changes }` contract and neither reinvents the other's plumbing.
- Boundary: the wire is one-way — `projection` produces the `Subscribable`, `ui` consumes it; the `Subscribable<A>` surfaces read-only `get`/`changes` so no `ui` bind can mutate store state through the face; `Atom.subscribable`'s own subscription lifecycle owns the `changes` stream's scope at the bind site, so the face holds no fiber and forks nothing.

```ts contract
// pkg:ui/.planning#ATOM_BRIDGE — the ui-side consumer law (this folder produces the Subscribable; ui owns the bind)
//
// import { Atom } from "@effect-atom/atom";
// import { availabilityProjection, isEnabled } from "@rasm/projection";
//
// const enabledAtom = Atom.subscribable(() => isEnabled(availabilityProjection, "geometry.boolean"));
//
// ui reads enabledAtom; it never reaches availabilityProjection.get's SubscriptionRef,
// and projection never imports Atom. The seam is the { get, changes } contract.
```
