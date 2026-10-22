import { Array, Cause, Context, Deferred, Effect, Option, Record, Ref, Schema, Struct } from 'effect';
import { Model } from './model.ts';

// --- [MODELS] --------------------------------------------------------------------------

abstract class Mutation extends Schema.Class<Mutation, { readonly brand: unique symbol }>('Mutation')({}) {
    abstract get label(): string;

    commit<A, E, R>(body: Effect.Effect<A, E, R>) {
        const { label } = this;
        return Transactions.use(({ run }) => run(label, body));
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

class Transactions extends Context.Service<Transactions>()('@rasm/indesign/transactions', {
    make: Effect.gen(function* () {
        const [{ app, SaveOptions, ScriptLanguage, UndoModes }, held] = yield* Effect.all([Model, Ref.make(Option.none<Effect.Effect<void>>())]);
        const close = (opened: readonly number[]) =>
            Effect.sync(() =>
                Array.difference(app.documents.everyItem().id, opened).forEach((id) => {
                    app.documents.itemByID(id).close(SaveOptions.NO);
                }),
            );
        return {
            enter: Effect.uninterruptible(
                Effect.flatMap(
                    Ref.getAndSet(held, Option.none()),
                    Option.getOrElse(() => Effect.void),
                ),
            ),
            run: Effect.fn('run')(function* <A, E, R>(label: string, body: Effect.Effect<A, E, R>) {
                const [opened, done, sync] = [app.documents.everyItem().id, yield* Deferred.make<A, E | Cause.UnknownError>(), Effect.runSyncWith(yield* Effect.context<R>())];
                const refused = (cause: unknown) => new Cause.UnknownError(cause, label);
                const step = Effect.try({ try: (): unknown => app.doScript(() => sync(Effect.andThen(Deferred.into(body, done), Deferred.await(done))), ScriptLanguage.UXPSCRIPT, [], UndoModes.ENTIRE_SCRIPT, label), catch: refused }).pipe(
                    Effect.catch((error) => Deferred.fail(done, error)),
                    Effect.asVoid,
                );
                return yield* Ref.set(held, Option.some(step)).pipe(
                    Effect.andThen(Effect.try({ try: (): unknown => app.doScript('$.sleep(5)', ScriptLanguage.JAVASCRIPT), catch: refused }).pipe(Effect.repeat({ until: () => Deferred.isDone(done) }))),
                    Effect.andThen(Deferred.await(done)),
                    Effect.ensuring(Ref.set(held, Option.none())),
                    Effect.onError(() => close(opened)),
                );
            }),
        };
    }),
}) {}

// --- [OPERATIONS] ----------------------------------------------------------------------

const preserve = <T extends object>(target: T, replacements: Partial<T>) => {
    const refused = (cause: unknown) => new Cause.UnknownError(cause, `preferences ${Struct.keys(replacements).join(', ')}`);
    return Effect.acquireRelease(Effect.try({ try: () => Record.fromIterableWith(Struct.keys(replacements), (key) => [key, target[key]]), catch: refused }), (saved) => Effect.sync(() => Object.assign(target, saved))).pipe(Effect.tap(() => Effect.try({ try: () => Object.assign(target, replacements), catch: refused })));
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Mutation, preserve, Transactions };
