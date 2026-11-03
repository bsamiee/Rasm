import { Effect, Exit, flow } from 'effect';

// --- [OPERATIONS] ----------------------------------------------------------------------

const accumulate = <A, E, R>(items: Iterable<A>, f: (item: A) => Effect.Effect<unknown, E, R>): Effect.Effect<void, E, R> => Effect.forEach(items, flow(f, Effect.exit), { concurrency: 'unbounded' }).pipe(Effect.flatMap(Exit.asVoidAll));

// --- [EXPORTS] -------------------------------------------------------------------------

export { accumulate };
