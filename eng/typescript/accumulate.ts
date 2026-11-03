import { Effect, Exit, flow, type Types } from 'effect';

// --- [OPERATIONS] ----------------------------------------------------------------------

const accumulate = <A, E, R>(items: Iterable<A>, f: (item: A) => Effect.Effect<unknown, E, R>, options?: { readonly concurrency?: Types.Concurrency }): Effect.Effect<void, E, R> => Effect.forEach(items, flow(f, Effect.exit), options).pipe(Effect.flatMap(Exit.asVoidAll));

// --- [EXPORTS] -------------------------------------------------------------------------

export { accumulate };
