import { Array, Cause, Deferred, Effect, Record, Schema, Struct } from 'effect';
import { Model } from '#/dom/model';

// --- [MODELS] --------------------------------------------------------------------------

abstract class Mutation extends Schema.Class<Mutation, { readonly brand: unique symbol }>('Mutation')({}) {
    abstract get label(): string;

    commit<A, E, R>(body: Effect.Effect<A, E, R>) {
        const { label } = this;
        return Effect.gen(function* () {
            const { app, SaveOptions, ScriptLanguage, UndoModes } = yield* Model;
            const [opened, done, sync] = [app.documents.everyItem().id, yield* Deferred.make<A, E | Cause.UnknownError>(), Effect.runSyncWith(yield* Effect.context<R>())];
            const closed = (id: number) => Effect.sync(() => app.documents.itemByID(id).close(SaveOptions.NO));
            yield* Effect.try({ try: (): unknown => app.doScript(() => sync(Effect.andThen(Deferred.into(body, done), Deferred.await(done))), ScriptLanguage.UXPSCRIPT, [], UndoModes.ENTIRE_SCRIPT, label), catch: (cause) => new Cause.UnknownError(cause, label) }).pipe(
                Effect.catch((error) => Deferred.fail(done, error)),
            );
            return yield* Deferred.await(done).pipe(Effect.onError(() => Effect.forEach(Array.difference(app.documents.everyItem().id, opened), closed, { discard: true })));
        });
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const preserve = <T extends object>(target: T, replacements: Partial<T>) => {
    const refused = (cause: unknown) => new Cause.UnknownError(cause, `preferences ${Struct.keys(replacements).join(', ')}`);
    return Effect.acquireRelease(Effect.try({ try: () => Record.fromIterableWith(Struct.keys(replacements), (key) => [key, target[key]]), catch: refused }), (saved) => Effect.sync(() => Object.assign(target, saved))).pipe(Effect.tap(() => Effect.try({ try: () => Object.assign(target, replacements), catch: refused })));
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Mutation, preserve };
