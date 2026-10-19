import { Array, Cause, Context, Data, Deferred, Effect, PubSub, Record, Ref, Schema, Struct } from 'effect';
import { Dom } from './dom.ts';
import { Preflight } from './preflight.ts';
import { read } from './references.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Handoff = Data.TaggedEnum<{ Idle: Record.ReadonlyRecord<never, never>; Held: { readonly step: Effect.Effect<void> }; Entered: Record.ReadonlyRecord<never, never> }>;

// --- [MODELS] --------------------------------------------------------------------------

const Handoff = Data.taggedEnum<Handoff>();
const Phase = Schema.TaggedUnion({ Idle: {}, Entered: {} }).annotate({ description: 'What an enter call did: found no held step, or ran the step a mutation held' });
abstract class Mutation extends Schema.Class<Mutation, { readonly brand: unique symbol }>('Mutation')({}) {
    static readonly summarized = <F extends Schema.Struct.Fields>(own: Schema.Struct<F>) => own.pipe(Schema.fieldsAssign({ preflight: Preflight.Summary }));

    abstract get label(): string;

    commit<F extends Schema.Struct.Fields, A, E, R, E2 = never, R2 = never>(own: Schema.Struct<F>, body: Effect.Effect<A, E, R>, settle: (value: A) => Effect.Effect<unknown, E2, R2>) {
        const { label } = this;
        return Effect.gen(function* () {
            const { run, changed } = yield* Transactions;
            const baseline = yield* Preflight.baseline();
            const { value, touched } = yield* run(label, body);
            const settled = yield* settle(value);
            const preflight = yield* Preflight.summarized(baseline, touched);
            yield* PubSub.publishAll(changed, preflight);
            return { ...(yield* read(own, () => settled)), preflight };
        });
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

class Transactions extends Context.Service<Transactions>()('@rasm/indesign/transactions', {
    make: Effect.gen(function* () {
        const [{ app, SaveOptions, ScriptLanguage, UndoModes }, state, changed] = yield* Effect.all([Dom, Ref.make<Handoff>(Handoff.Idle()), PubSub.unbounded<typeof Preflight.Summary.value.Type>()]);
        const steps = () => {
            const every = app.documents.everyItem();
            return Array.zipWith(every.id, every.undoHistory, (id, history) => [id, history.length] as const);
        };
        const close = (opened: readonly number[]) =>
            Effect.sync(() =>
                Array.difference(app.documents.everyItem().id, opened).forEach((id) => {
                    app.documents.itemByID(id).close(SaveOptions.NO);
                }),
            );
        return {
            changed,
            enter: Effect.uninterruptible(
                Effect.flatten(
                    Ref.modify<Handoff, Effect.Effect<typeof Phase.Type>>(
                        state,
                        Handoff.$match({
                            Idle: () => [Effect.succeed(Phase.cases.Idle.make({})), Handoff.Idle()] as const,
                            Held: ({ step }) => [Effect.as(step, Phase.cases.Entered.make({})), Handoff.Entered()] as const,
                            Entered: () => [Effect.succeed(Phase.cases.Entered.make({})), Handoff.Entered()] as const,
                        }),
                    ),
                ),
            ),
            run: Effect.fn('run')(function* <A, E, R>(label: string, body: Effect.Effect<A, E, R>) {
                yield* Effect.filterOrFail(
                    Effect.sync(() => app.modalState),
                    (modalState) => !modalState,
                    (modalState) => new Cause.UnknownError({ modalState }, label),
                );
                const [before, done, sync] = [new Map(steps()), yield* Deferred.make<A, E | Cause.UnknownError>(), Effect.runSyncWith(yield* Effect.context<R>())];
                const refused = (cause: unknown) => new Cause.UnknownError(cause, label);
                const step = Effect.try({ try: (): unknown => app.doScript(() => sync(Effect.andThen(Deferred.into(body, done), Deferred.await(done))), ScriptLanguage.UXPSCRIPT, [], UndoModes.ENTIRE_SCRIPT, label), catch: refused }).pipe(
                    Effect.catch((error) => Deferred.fail(done, error)),
                    Effect.asVoid,
                );
                const value = yield* Ref.set(state, Handoff.Held({ step })).pipe(
                    Effect.andThen(Effect.try({ try: (): unknown => app.doScript('$.sleep(5)', ScriptLanguage.JAVASCRIPT), catch: refused }).pipe(Effect.repeat({ until: () => Deferred.isDone(done) }))),
                    Effect.andThen(Deferred.await(done)),
                    Effect.ensuring(Ref.set(state, Handoff.Idle())),
                    Effect.onError(() => close([...before.keys()])),
                );
                return { value, touched: steps().flatMap(([id, length]) => (before.get(id) === length ? [] : [app.documents.itemByID(id)])) };
            }),
        };
    }),
}) {}

// --- [OPERATIONS] ----------------------------------------------------------------------

const preserve = <T extends object>(target: T, replacements: Partial<T>) =>
    Effect.acquireRelease(
        Effect.try(() => Record.fromIterableWith(Struct.keys(replacements), (key) => [key, target[key]])),
        (saved) => Effect.orDie(Effect.try(() => Object.assign(target, saved))),
    ).pipe(Effect.tap(() => Effect.try(() => Object.assign(target, replacements))));

// --- [EXPORTS] -------------------------------------------------------------------------

export { Mutation, Phase, preserve, Transactions };
