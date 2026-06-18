# [UI_ATOM_BINDING]

The single sanctioned React state binding and the client-state folds composed under it. `AtomBinding` is the sole `@effect-atom` React surface every leaf subscribes through; URL-resident state resolves through `Atom.searchParam`, the offline cell through `Atom.kvs`, keyed per-entity subscription through `Atom.family`, the streamed `projection` feeds through `Atom.pull`, and `UndoStack` is the bounded history fold. Status-discriminated render is one `Result.builder` chain so no leaf re-implements a three-state match. The page owns no wire cluster and holds no domain state; every domain read flows through a `projection` store and the one binding.

## [1]-[INDEX]

One cluster: `ATOM_BINDING` owns the sanctioned `@effect-atom` hooks, the native state constructors, and the `UndoStack` history fold.

## [2]-[ATOM_BINDING]

- Owner: `AtomBinding`, the single sanctioned React state binding — the `@effect-atom/atom-react` hook set (`useAtomValue` for a read, `useAtomSet` for a `Atom.Writable` write, `useAtom` for the read-write pair, `useAtomSuspense` for a `Result` atom) consumed with no rename layer — over the native `@effect-atom/atom` constructors: `Atom.searchParam` owns URL-resident state, `Atom.kvs` owns the offline cell, `Atom.family` owns keyed per-entity subscription, `Atom.pull` owns the streamed `projection` feeds, and `Atom.runtime` carries the registry layer. `UndoStack` is the one client-state fold the binding does not own natively — a bounded history over a domain value. The dev-build atom inspector is one row on `Atom.runtime`, stripped at the build edge.
- Cases: components subscribe at the leaf, not the root; all domain state lives in the owning `projection` fold and reaches the component only through `AtomBinding`; local component state holding domain data is the named defect. URL-resident state is one `Atom.searchParam(name, { schema })` cell whose `Schema` round-trip survives reload through the library's own reactivity — never a hand-rolled query-string parser duplicated into component state. The offline cell is one `Atom.kvs({ runtime, key, schema, defaultValue })` `Writable` over the `platform` `LocalPersistence` `KeyValueStore` — never a bespoke `SubscriptionRef`/`localStorage` fold. A keyed subscription is one `Atom.family((key) => …)` so a per-entity feed memoizes one cell per key. A streamed feed is one `Atom.pull(stream)` accumulating the `projection` `Stream` into a `Result`. `UndoStack` is a bounded `undo`/`redo`/`push` fold over a domain value, never a mutable array. The dev inspector is one `Atom.runtime` row so fold state is inspectable without a second binding, stripped from the production bundle by the `platform` `BuildPipeline`.
- Entry: leaf surfaces read document, health, progress, conflict, presence, command, and evidence by reading `projection` folds through the atom hooks; the login-logout affordance and the session-status indicator are one leaf reading the `platform` `AuthSession` status through the binding, never a second binding; a `Result`-bearing feed renders through the `Result.builder` chain (`onWaiting`/`onSuccess`/`onError`/`render`) so the loading/success/failure arms render uniformly at every leaf.
- Packages: `react`, `react-dom`, `@effect-atom/atom`, `@effect-atom/atom-react`, `effect`.
- Growth: a new leaf surface lands as one subscriber component; a new URL-resident surface lands as one `Atom.searchParam` cell; a new offline cell lands as one `Atom.kvs` cell; a new keyed feed lands as one `Atom.family` arg; a new streamed feed lands as one `Atom.pull` source; a new client-state concern lands as one fold under the binding; a new inspector capability lands as one dev-build row on `Atom.runtime`.
- Boundary: a second state binding beside the atom layer is the named defect; a hand-rolled query-string parser beside `Atom.searchParam`, a bespoke storage fold beside `Atom.kvs`, or a manual stream subscription beside `Atom.pull` is the named defect; the atom inspector is a dev-build-only row stripped by the `platform` `BuildPipeline`; the view emits intents only through the `interchange` `CommandGateway`, never a transport directly; URL-resident state is never duplicated into local component state; the offline cell is read through `LocalPersistence`, never a direct storage read.

```ts contract
import { useAtom, useAtomSet, useAtomSuspense, useAtomValue } from "@effect-atom/atom-react";
import { Atom, Result } from "@effect-atom/atom";
import type { KeyValueStore } from "@effect/platform";

declare const projectionRuntime: Atom.AtomRuntime<KeyValueStore.KeyValueStore, never>;

const deepLink = <A, I extends string>(name: string, schema: Schema.Schema<A, I>): Atom.Writable<Option.Option<A>> =>
  Atom.searchParam(name, { schema });

const offlineCell = <A>(key: string, schema: Schema.Schema<A>, defaultValue: () => A): Atom.Writable<A> =>
  Atom.kvs({ runtime: projectionRuntime, key, schema, defaultValue });

const feedFamily = <Arg, A, E>(create: (arg: Arg) => Stream.Stream<A, E>): (arg: Arg) => Atom.Writable<Result.PullResult<A, E>, void> =>
  Atom.family((arg: Arg) => Atom.pull(create(arg)));

const renderFeed = <A, E>(result: Result.Result<A, E>, on: {
  readonly waiting: () => React.ReactElement;
  readonly success: (value: A) => React.ReactElement;
  readonly failure: (error: E) => React.ReactElement;
}): React.ReactElement | null =>
  Result.builder(result)
    .onWaiting(on.waiting)
    .onSuccess((s) => on.success(s.value))
    .onError((e) => on.failure(e))
    .render();

interface UndoStack<A> {
  readonly current: SubscriptionRef.SubscriptionRef<A>;
  readonly push: (next: A) => Effect.Effect<void>;
  readonly undo: Effect.Effect<Option.Option<A>>;
  readonly redo: Effect.Effect<Option.Option<A>>;
}

const makeUndoStack = <A>(initial: A, capacity: number): Effect.Effect<UndoStack<A>> =>
  Effect.gen(function* () {
    const current = yield* SubscriptionRef.make(initial);
    const past = yield* Ref.make<ReadonlyArray<A>>([]);
    const future = yield* Ref.make<ReadonlyArray<A>>([]);
    const push = (next: A) =>
      SubscriptionRef.get(current).pipe(
        Effect.flatMap((prev) => Ref.update(past, (p) => [...p, prev].slice(-capacity))),
        Effect.zipRight(Ref.set(future, [])),
        Effect.zipRight(SubscriptionRef.set(current, next)),
      );
    const undo = Ref.get(past).pipe(
      Effect.flatMap((p) =>
        Array.matchRight(p, {
          onEmpty: () => Effect.succeed(Option.none<A>()),
          onNonEmpty: (init, last) =>
            SubscriptionRef.get(current).pipe(
              Effect.flatMap((cur) => Ref.update(future, (f) => [cur, ...f])),
              Effect.zipRight(Ref.set(past, init)),
              Effect.zipRight(SubscriptionRef.set(current, last)),
              Effect.as(Option.some(last)),
            ),
        })),
    );
    const redo = Ref.get(future).pipe(
      Effect.flatMap((f) =>
        Array.matchLeft(f, {
          onEmpty: () => Effect.succeed(Option.none<A>()),
          onNonEmpty: (head, tail) =>
            SubscriptionRef.get(current).pipe(
              Effect.flatMap((cur) => Ref.update(past, (p) => [...p, cur])),
              Effect.zipRight(Ref.set(future, tail)),
              Effect.zipRight(SubscriptionRef.set(current, head)),
              Effect.as(Option.some(head)),
            ),
        })),
    );
    return { current, push, undo, redo };
  });
```
