import { Effect, Exit } from 'effect';

// --- [OPERATIONS] ----------------------------------------------------------------------

const accumulate = <A, E, R>(items: Iterable<A>, run: (item: A) => Effect.Effect<unknown, E, R>): Effect.Effect<void, E, R> => Effect.forEach(items, (item) => Effect.exit(run(item))).pipe(Effect.flatMap(Exit.asVoidAll));

// --- [EXPORTS] -------------------------------------------------------------------------

export { accumulate };
