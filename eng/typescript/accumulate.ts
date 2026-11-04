import { Effect, Exit, Function, Predicate, type Types } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

interface Options {
    readonly concurrency?: Types.Concurrency;
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const accumulate: {
    <A, E, R>(f: (item: A) => Effect.Effect<unknown, E, R>, options?: Options): (items: Iterable<A>) => Effect.Effect<void, E, R>;
    <A, E, R>(items: Iterable<A>, f: (item: A) => Effect.Effect<unknown, E, R>, options?: Options): Effect.Effect<void, E, R>;
} = Function.dual(
    (args) => Predicate.isFunction(args[1]),
    <A, E, R>(items: Iterable<A>, f: (item: A) => Effect.Effect<unknown, E, R>, options?: Options) => Effect.forEach(items, (item) => Effect.exit(f(item)), options).pipe(Effect.flatMap(Exit.asVoidAll)),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { accumulate };
