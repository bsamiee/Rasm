import { HostError } from '@rasm/automation/requests';
import { Array, Deferred, Effect, Record, Struct } from 'effect';
import { Model } from '#/dom/model';

// --- [OPERATIONS] ----------------------------------------------------------------------

const commit = Effect.fnUntraced(function* <A, E, R>(label: string, body: Effect.Effect<A, E, R>) {
    const { app, SaveOptions, ScriptLanguage, UndoModes } = yield* Model;
    const [opened, done, sync] = [app.documents.everyItem().id, yield* Deferred.make<A, E | HostError>(), Effect.runSyncWith(yield* Effect.context<R>())];
    const closed = (id: number) => Effect.sync(() => app.documents.itemByID(id).close(SaveOptions.NO));
    yield* Effect.try({ try: (): unknown => app.doScript(() => sync(Effect.andThen(Deferred.into(body, done), Deferred.await(done))), ScriptLanguage.UXPSCRIPT, [], UndoModes.ENTIRE_SCRIPT, label), catch: HostError.refused(label) }).pipe(Effect.catch((error) => Deferred.fail(done, error)));
    return yield* Deferred.await(done).pipe(Effect.onError(() => Effect.forEach(Array.difference(app.documents.everyItem().id, opened), closed, { discard: true })));
});
const preserve = <T extends object>(target: T, replacements: Partial<T>, held: readonly string[] = Struct.keys(replacements)) => {
    const refused = HostError.refused(`preferences ${held.join(', ')}`);
    return Effect.acquireRelease(Effect.try({ try: () => Record.fromIterableWith(held, (key): readonly [string, unknown] => [key, Reflect.get(target, key)]), catch: refused }), (saved) => Effect.sync(() => Object.assign(target, saved))).pipe(
        Effect.tap(() => Effect.try({ try: () => Object.assign(target, replacements), catch: refused })),
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { commit, preserve };
