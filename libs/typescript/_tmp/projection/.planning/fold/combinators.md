# [PROJECTION_COMBINATORS]

The two fold combinators every store rests on: `foldStream`, the scalar accumulator that reduces a `Stream` into one `SubscriptionRef<S>` under a `step` arm, and `keyedFold`, the keyed-map specialization that discriminates each event by the verbatim wire key and merges it into its slot. Every feed, the window engine, and the convergence fold compose one of these two — keyed maps through `keyedFold`, the single-accumulator convergence state through `foldStream` directly — never a parallel store implementation. Both pipe the source through `policy#STREAM_POLICY` `withPolicy`, so the bounded reconnect, buffer, throttle, and batch land identically and the make-fork-update scaffold exists once.

## [01]-[INDEX]

- [01]-[KEYED_FOLD]: Owns `foldStream`, the scalar fold primitive, and `keyedFold`, the keyed-map combinator built on it.

## [02]-[KEYED_FOLD]

- Owner: `foldStream`, the scalar primitive that pipes a `Stream<In>` through `withPolicy`, reduces it into a `SubscriptionRef<S>` under a `step` arm, and forks into the enclosing `Scope`; `keyedFold`, the keyed-map specialization carrying a `key` discriminator and a slot `merge` over one persistent `HashMap`. Every fold elsewhere in the folder is one application of one of the two; the variation is the state type, the step or merge arm, and the wire union folded.
- Packages: `effect` for `Stream`, `SubscriptionRef`, `Effect`, `Option`, `HashMap`, and `Scope`.
- Growth: a new keyed boundary concept lands as one `keyedFold` row, a new single-accumulator fold as one `foldStream` row; neither combinator grows.
- Boundary: neither combinator re-validates a value an earlier decode admitted; `keyedFold`'s key reads the discriminant the wire shape carries verbatim; the keyed accumulator is `HashMap.modifyAt` over the persistent `HashMap` — structural sharing under retry, never a `new Map().set` rebuild that breaks referential transparency on reconnect-replay; the source pipes through `withPolicy` so reconnect-replay re-folds identically; consumers read the `SubscriptionRef` over `HashMap.HashMap` and the `@effect-atom/atom` bridge binds it at the `ui` boundary, never imported here.

```ts contract
import { Effect, HashMap, Option, Scope, Stream, SubscriptionRef } from "effect";
import { withPolicy, type StreamPolicy } from "./stream-policy";

const foldStream = <In, S>(
  source: Stream.Stream<In>,
  initial: S,
  step: (state: S, event: In) => S,
  policy: StreamPolicy,
): Effect.Effect<SubscriptionRef.SubscriptionRef<S>, never, Scope.Scope> =>
  Effect.gen(function* () {
    const ref = yield* SubscriptionRef.make(initial);
    yield* withPolicy(source, policy).pipe(
      Stream.runForEach((event) => SubscriptionRef.update(ref, (state) => step(state, event))),
      Effect.forkScoped,
    );
    return ref;
  });

const keyedFold = <In, K, V>(
  source: Stream.Stream<In>,
  key: (event: In) => K,
  merge: (prior: Option.Option<V>, event: In) => V,
  policy: StreamPolicy,
): Effect.Effect<SubscriptionRef.SubscriptionRef<HashMap.HashMap<K, V>>, never, Scope.Scope> =>
  foldStream(source, HashMap.empty<K, V>(), (m, event) =>
    HashMap.modifyAt(m, key(event), (prior) => Option.some(merge(prior, event))), policy);
```
