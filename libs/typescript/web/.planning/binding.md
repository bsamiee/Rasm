# [WEB_BINDING]

One page owns the single sanctioned React state binding and the client-state folds composed under it — `AtomBinding`, the sole `@effect-atom` React state binding; `DeepLinkBinding`, the query-string state sibling; the url-as-state, undo/redo, and offline-state client-state concerns expressed as Effect-native folds over the `@rasm/projection` stores; and the development-build-only atom inspector. Every domain read flows through a store and the atom binding, the only sanctioned React state binding under the collapse-scan law. The page owns no wire cluster and holds no domain state; it subscribes to the `@rasm/projection` folds through the one binding.

## [1]-[INDEX]

| [INDEX] | [CLUSTER] | [OWNS]                                                      |
| :-----: | :-------- | :--------------------------------------------------------- |
|   [1]   | BINDING   | the sanctioned state binding and the client-state folds     |

## [2]-[BINDING]

- Owner: `AtomBinding`, the single sanctioned React state binding, plus the client-state folds composed under it — `DeepLinkBinding` owning URL-resident state in the query string, `UndoStack` owning the undo/redo history as an Effect-native fold, and `OfflineState` exposing the offline persistence cell; the development-build-only atom inspector is one row on `AtomBinding`.
- Cases: components subscribe at the leaf, not the root; all domain state lives in the owning `@rasm/projection` fold and reaches the component only through `AtomBinding`; local component state holding domain data is the named defect the collapse-scan law deletes; `DeepLinkBinding` resolves route-resident state through the query string and command intents resolve from stable string keys so deep links survive a reload, never duplicated into component state; `UndoStack` is a bounded history fold over a domain value with `undo`/`redo`/`push` as Effect transitions, never a mutable array; `OfflineState` reads the last-good cell the `platform-substrate.md` `LocalPersistence` holds; the development-build-only atom inspector is one row on `AtomBinding` so fold state is inspectable without a second state binding, stripped from the production bundle by `BuildPipeline` and never present in the shipped bundle.
- Entry: the view renders document, health, progress, conflict, presence, command, and evidence surfaces by reading `@rasm/projection` folds through the atom hooks; the login-logout affordance and the session-status indicator are one leaf subscriber reading the `AuthSession` status through the atom binding, never a second state binding; url-as-state and undo/redo compose under the same binding rather than a parallel client-state library.
- Packages: `react` and `react-dom` for the renderer, `@effect-atom/atom` and `@effect-atom/atom-react` for the binding and its development-build atom inspector, `nuqs` for the query-string state codec, and `effect` for the `Schema` codec the deep-link key round-trips through.
- Growth: a new leaf surface lands as one subscriber component; a new deep-link surface lands as one query-string-bound key; a new client-state concern lands as one fold under the binding; a new inspector capability lands as one development-build row on `AtomBinding`, never a production-shipped state binding.
- Boundary: a second state binding beside the atom layer is the named defect; the atom inspector is a development-build-only row stripped by `BuildPipeline`; the view emits intents only through the `@rasm/interchange` `CommandGateway` and never dials a transport directly; URL-resident state is never duplicated into local component state; the offline cell is read through `LocalPersistence`, never a direct storage read.

```ts contract
interface AtomBinding {
  readonly use: <A>(atom: Atom.Atom<A>) => A;
  readonly useSet: <R, W>(atom: Atom.Writable<R, W>) => (write: W) => void;
}

interface DeepLinkBinding {
  readonly useKey: <A>(key: string, schema: Schema.Schema<A, string>) => readonly [A, (next: A) => void];
}

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
