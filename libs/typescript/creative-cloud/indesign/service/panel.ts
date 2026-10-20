import indesign from 'adobe:indesign';
import { Link, linked } from '@rasm/uxp/link';
import { Plugin } from '@rasm/uxp/runtime';
import { Effect, Layer, Option, Schema, Stream, SubscriptionRef } from 'effect';
import { Rpc } from 'effect/rpc';
import { Model } from '../model.ts';
import { Mutation, preserve, Transactions } from '../transactions.ts';
import { Hold, InDesign, PATH, Peer, WIRE } from './contract.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

const hold = Layer.effect(Hold)(
    Effect.map(Link, ({ call }) => (effect, { payload, rpc }) => {
        const { app, MeasurementUnits, UserInteractionLevels } = indesign;
        return Effect.andThen(
            Effect.all([
                preserve(app.scriptPreferences, { userInteractionLevel: UserInteractionLevels.NEVER_INTERACT, enableRedraw: false, measurementUnit: MeasurementUnits.POINTS }),
                Effect.acquireRelease(SubscriptionRef.set(call, Option.some(Schema.is(Mutation)(payload) ? payload.label : rpc._tag)), () => SubscriptionRef.set(call, Option.none())),
            ]),
            effect,
        );
    }),
);
const handlers = InDesign.toLayer({
    enter: () => Rpc.fork(Transactions.use(({ enter }) => enter)),
    changed: () => Rpc.fork(Stream.unwrap(Transactions.useSync(({ changed }) => Stream.fromPubSub(changed)))),
    inspect: (payload) => payload.run(),
    change: (payload) => payload.run(),
    create: (payload) => payload.run(),
    remove: (payload) => payload.run(),
    execute_script: (payload) => payload.run(),
    find_change: (payload) => payload.run(),
    font: (payload) => payload.run(),
    kind: (payload) => payload.run(),
    render: (payload) => payload.run(),
    menu: (payload) => payload.run(),
    invoke: (payload) => payload.run(),
    preflight: (payload) => payload.run(),
    queries: (payload) => payload.run(),
}).pipe(Layer.provideMerge(hold), Layer.provide(Layer.effect(Transactions, Transactions.make)), Layer.provide(Layer.effect(Model, Model.make(indesign))));
const panel = ({ peer, origin }: { readonly peer: string; readonly origin: string }) =>
    linked(InDesign, handlers, {
        peer,
        url: `${origin}${PATH}`,
        identity: Effect.gen(function* () {
            const [{ url }, { name }] = yield* Effect.all([Effect.tryPromise(() => indesign.app.fullName), Plugin]);
            return yield* Schema.encodeEffect(Peer)({ application: new URL(url), extension: name });
        }),
        serialization: WIRE,
    });

// --- [EXPORTS] -------------------------------------------------------------------------

export { panel };
