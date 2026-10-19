import indesign from 'adobe:indesign';
import { Array, Context, Deferred, Effect, Layer, Option, Ref, Schema } from 'effect';
import { InDesignError } from '../models.ts';

// --- [MODELS] --------------------------------------------------------------------------

const Phase = Schema.Literals(['Idle', 'Held', 'Entered']);
const Description = Schema.String.annotate({ description: 'Present-progressive sentence shown while the call runs, naming its undo step' });

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

const modal = Effect.suspend(() => (indesign.app.modalState ? Effect.fail(InDesignError.make({ reasons: [{ _tag: 'Modal' }] })) : Effect.void));
const transacted = Effect.fn('transacted')(function* <A, E, R>(name: string, body: Effect.Effect<A, E, R>) {
    const { app, SaveOptions, ScriptLanguage, UndoModes } = indesign;
    yield* modal;
    const { handed, entered } = yield* Handoff;
    const [done, run, opened] = [yield* Deferred.make<A, E | InDesignError>(), Effect.runSyncWith(yield* Effect.context<R>()), app.documents.everyItem().id];
    const recorded = Effect.flatMap(Effect.exit(body), (exit) => Effect.andThen(Deferred.done(done, exit), exit));
    const close = Effect.sync(() => {
        Array.difference(app.documents.everyItem().id, opened).forEach((id) => {
            app.documents.itemByID(id).close(SaveOptions.NO);
        });
    });
    const wait = Effect.try({ try: (): unknown => app.doScript('$.sleep(5)', ScriptLanguage.JAVASCRIPT), catch: InDesignError.refused(Option.some(name)) });
    const step = Effect.catch(Effect.try({ try: (): unknown => app.doScript(() => run(recorded), ScriptLanguage.UXPSCRIPT, [], UndoModes.ENTIRE_SCRIPT, name), catch: InDesignError.refused(Option.some(name)) }), (refused) => Deferred.fail(done, refused));
    return yield* handed(wait, Effect.andThen(entered, Effect.andThen(step, Deferred.await(done)))).pipe(Effect.onError(() => close));
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { Description, Handoff, Phase, transacted };
