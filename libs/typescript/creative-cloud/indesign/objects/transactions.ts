import type { Document } from 'adobe:indesign';
import { Array, Cause, Context, Deferred, Effect, Layer, Option, Record, Ref, Schema, Struct } from 'effect';
import { Host } from '../host.ts';
import { Preflight, Reports } from '../preflight.ts';
import { read } from './specifiers.ts';

// --- [MODELS] --------------------------------------------------------------------------

const Phase = Schema.Literals(['Idle', 'Held', 'Entered']);
class Mutation extends Schema.Class<Mutation, { readonly brand: unique symbol }>('Mutation')({}) {
    static readonly Result = <Fields extends Schema.Struct.Fields>(fields: Fields) => Schema.Struct({ ...fields, preflight: Preflight.Summary });

    get label(): string {
        return this.constructor.name;
    }

    commit<Fields extends Schema.Struct.Fields, E, R>(result: Schema.Struct<Fields & { readonly preflight: typeof Preflight.Summary }>, body: Effect.Effect<unknown, E, R>) {
        const own = Schema.Struct(Struct.omit(result.fields, ['preflight']));
        const { label } = this;
        return Effect.gen(function* () {
            const { value, touched } = yield* transacted(
                { label },
                Effect.flatMap(body, (raw) => read(own, () => raw)),
            );
            const { summarized } = yield* Reports;
            return { ...value, preflight: yield* summarized(touched) };
        });
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

class Handoff extends Context.Service<Handoff>()('@rasm/indesign/objects/transactions/Handoff', {
    make: Effect.map(Ref.make<typeof Phase.Type | Effect.Effect<void>>('Idle'), (state) => ({
        enter: Effect.uninterruptible(Effect.flatten(Ref.modify(state, (now) => (Effect.isEffect(now) ? [Effect.as(now, 'Entered' as const), 'Held' as const] : [Effect.succeed(now), now])))),
        entered: Ref.set(state, 'Entered'),
        handed: Effect.fnUntraced(
            function* <A, E, R, W>(wait: Effect.Effect<unknown, W>, step: Effect.Effect<A, E, R>) {
                const [done, services] = yield* Effect.all([Deferred.make<A, E>(), Effect.context<R>()]);
                yield* Ref.set(state, Effect.asVoid(Deferred.into(Effect.provideContext(step, services), done)));
                yield* wait.pipe(Effect.repeat({ until: () => Deferred.isDone(done) }));
                return yield* Deferred.await(done);
            },
            Effect.ensuring(Ref.set(state, 'Idle')),
        ),
    })),
}) {
    static readonly layer = Layer.effect(this, this.make);
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const transacted = Effect.fn('transacted')(function* <A, E, R>({ label }: Pick<Mutation, 'label'>, body: Effect.Effect<A, E, R>) {
    const { indesign } = yield* Host;
    const { app, SaveOptions, ScriptLanguage, UndoModes } = indesign;
    yield* Effect.filterOrFail(
        Effect.sync(() => app.modalState),
        (modal) => !modal,
        () => new Cause.IllegalArgumentError('A dialog open in InDesign blocks the call: close it, then retry'),
    );
    const { handed, entered } = yield* Handoff;
    const steps = () => Record.fromIterableWith(app.documents.everyItem().getElements(), (document) => [String(document.id), document.undoHistory.length]);
    const [done, run, before] = [yield* Deferred.make<A, E | Cause.UnknownError>(), Effect.runSyncWith(yield* Effect.context<R>()), steps()];
    const recorded = Effect.flatMap(Effect.exit(body), (exit) => Effect.andThen(Deferred.done(done, exit), exit));
    const close = Effect.sync(() => {
        Array.difference(app.documents.everyItem().id, Record.keys(before).map(Number)).forEach((id) => {
            app.documents.itemByID(id).close(SaveOptions.NO);
        });
    });
    const refused = (cause: unknown) => new Cause.UnknownError(cause, label);
    const wait = Effect.try({ try: (): unknown => app.doScript('$.sleep(5)', ScriptLanguage.JAVASCRIPT), catch: refused });
    const step = Effect.catch(Effect.try({ try: (): unknown => app.doScript(() => run(recorded), ScriptLanguage.UXPSCRIPT, [], UndoModes.ENTIRE_SCRIPT, label), catch: refused }), (error) => Deferred.fail(done, error));
    const value = yield* handed(wait, Effect.andThen(entered, Effect.andThen(step, Deferred.await(done)))).pipe(Effect.onError(() => close));
    const touched: readonly Document[] = app.documents
        .everyItem()
        .getElements()
        .filter((document) => !Option.contains(Record.get(before, String(document.id)), document.undoHistory.length));
    return { value, touched };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { Handoff, Mutation, Phase, transacted };
